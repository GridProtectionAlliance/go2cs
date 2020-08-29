// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Memory allocator.
//
// This was originally based on tcmalloc, but has diverged quite a bit.
// http://goog-perftools.sourceforge.net/doc/tcmalloc.html

// The main allocator works in runs of pages.
// Small allocation sizes (up to and including 32 kB) are
// rounded to one of about 70 size classes, each of which
// has its own free set of objects of exactly that size.
// Any free page of memory can be split into a set of objects
// of one size class, which are then managed using a free bitmap.
//
// The allocator's data structures are:
//
//    fixalloc: a free-list allocator for fixed-size off-heap objects,
//        used to manage storage used by the allocator.
//    mheap: the malloc heap, managed at page (8192-byte) granularity.
//    mspan: a run of pages managed by the mheap.
//    mcentral: collects all spans of a given size class.
//    mcache: a per-P cache of mspans with free space.
//    mstats: allocation statistics.
//
// Allocating a small object proceeds up a hierarchy of caches:
//
//    1. Round the size up to one of the small size classes
//       and look in the corresponding mspan in this P's mcache.
//       Scan the mspan's free bitmap to find a free slot.
//       If there is a free slot, allocate it.
//       This can all be done without acquiring a lock.
//
//    2. If the mspan has no free slots, obtain a new mspan
//       from the mcentral's list of mspans of the required size
//       class that have free space.
//       Obtaining a whole span amortizes the cost of locking
//       the mcentral.
//
//    3. If the mcentral's mspan list is empty, obtain a run
//       of pages from the mheap to use for the mspan.
//
//    4. If the mheap is empty or has no page runs large enough,
//       allocate a new group of pages (at least 1MB) from the
//       operating system. Allocating a large run of pages
//       amortizes the cost of talking to the operating system.
//
// Sweeping an mspan and freeing objects on it proceeds up a similar
// hierarchy:
//
//    1. If the mspan is being swept in response to allocation, it
//       is returned to the mcache to satisfy the allocation.
//
//    2. Otherwise, if the mspan still has allocated objects in it,
//       it is placed on the mcentral free list for the mspan's size
//       class.
//
//    3. Otherwise, if all objects in the mspan are free, the mspan
//       is now "idle", so it is returned to the mheap and no longer
//       has a size class.
//       This may coalesce it with adjacent idle mspans.
//
//    4. If an mspan remains idle for long enough, return its pages
//       to the operating system.
//
// Allocating and freeing a large object uses the mheap
// directly, bypassing the mcache and mcentral.
//
// Free object slots in an mspan are zeroed only if mspan.needzero is
// false. If needzero is true, objects are zeroed as they are
// allocated. There are various benefits to delaying zeroing this way:
//
//    1. Stack frame allocation can avoid zeroing altogether.
//
//    2. It exhibits better temporal locality, since the program is
//       probably about to write to the memory.
//
//    3. We don't zero pages that never get reused.

// package runtime -- go2cs converted at 2020 August 29 08:17:31 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\malloc.go
using sys = go.runtime.@internal.sys_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class runtime_package
    {
        private static readonly var debugMalloc = false;

        private static readonly var maxTinySize = _TinySize;
        private static readonly var tinySizeClass = _TinySizeClass;
        private static readonly var maxSmallSize = _MaxSmallSize;

        private static readonly var pageShift = _PageShift;
        private static readonly var pageSize = _PageSize;
        private static readonly var pageMask = _PageMask; 
        // By construction, single page spans of the smallest object class
        // have the most objects per span.
        private static readonly var maxObjsPerSpan = pageSize / 8L;

        private static readonly var mSpanInUse = _MSpanInUse;

        private static readonly var concurrentSweep = _ConcurrentSweep;

        private static readonly long _PageSize = 1L << (int)(_PageShift);
        private static readonly var _PageMask = _PageSize - 1L; 

        // _64bit = 1 on 64-bit systems, 0 on 32-bit systems
        private static readonly long _64bit = 1L << (int)((~uintptr(0L) >> (int)(63L))) / 2L; 

        // Tiny allocator parameters, see "Tiny allocator" comment in malloc.go.
        private static readonly long _TinySize = 16L;
        private static readonly var _TinySizeClass = int8(2L);

        private static readonly long _FixAllocChunk = 16L << (int)(10L); // Chunk size for FixAlloc
        private static readonly long _MaxMHeapList = 1L << (int)((20L - _PageShift)); // Maximum page length for fixed-size list in MHeap.
        private static readonly long _HeapAllocChunk = 1L << (int)(20L); // Chunk size for heap growth

        // Per-P, per order stack segment cache size.
        private static readonly long _StackCacheSize = 32L * 1024L; 

        // Number of orders that get caching. Order 0 is FixedStack
        // and each successive order is twice as large.
        // We want to cache 2KB, 4KB, 8KB, and 16KB stacks. Larger stacks
        // will be allocated directly.
        // Since FixedStack is different on different systems, we
        // must vary NumStackOrders to keep the same maximum cached size.
        //   OS               | FixedStack | NumStackOrders
        //   -----------------+------------+---------------
        //   linux/darwin/bsd | 2KB        | 4
        //   windows/32       | 4KB        | 3
        //   windows/64       | 8KB        | 2
        //   plan9            | 4KB        | 3
        private static readonly long _NumStackOrders = 4L - sys.PtrSize / 4L * sys.GoosWindows - 1L * sys.GoosPlan9; 

        // Number of bits in page to span calculations (4k pages).
        // On Windows 64-bit we limit the arena to 32GB or 35 bits.
        // Windows counts memory used by page table into committed memory
        // of the process, so we can't reserve too much memory.
        // See https://golang.org/issue/5402 and https://golang.org/issue/5236.
        // On other 64-bit platforms, we limit the arena to 512GB, or 39 bits.
        // On 32-bit, we don't bother limiting anything, so we use the full 32-bit address.
        // The only exception is mips32 which only has access to low 2GB of virtual memory.
        // On Darwin/arm64, we cannot reserve more than ~5GB of virtual memory,
        // but as most devices have less than 4GB of physical memory anyway, we
        // try to be conservative here, and only ask for a 2GB heap.
        private static readonly var _MHeapMap_TotalBits = (_64bit * sys.GoosWindows) * 35L + (_64bit * (1L - sys.GoosWindows) * (1L - sys.GoosDarwin * sys.GoarchArm64)) * 39L + sys.GoosDarwin * sys.GoarchArm64 * 31L + (1L - _64bit) * (32L - (sys.GoarchMips + sys.GoarchMipsle));
        private static readonly var _MHeapMap_Bits = _MHeapMap_TotalBits - _PageShift; 

        // _MaxMem is the maximum heap arena size minus 1.
        //
        // On 32-bit, this is also the maximum heap pointer value,
        // since the arena starts at address 0.
        private static readonly long _MaxMem = 1L << (int)(_MHeapMap_TotalBits) - 1L; 

        // Max number of threads to run garbage collection.
        // 2, 3, and 4 are all plausible maximums depending
        // on the hardware details of the machine. The garbage
        // collector scales well to 32 cpus.
        private static readonly long _MaxGcproc = 32L; 

        // minLegalPointer is the smallest possible legal pointer.
        // This is the smallest possible architectural page size,
        // since we assume that the first page is never mapped.
        //
        // This should agree with minZeroPage in the compiler.
        private static readonly System.UIntPtr minLegalPointer = 4096L;

        // physPageSize is the size in bytes of the OS's physical pages.
        // Mapping and unmapping operations must be done at multiples of
        // physPageSize.
        //
        // This must be set by the OS init code (typically in osinit) before
        // mallocinit.
        private static System.UIntPtr physPageSize = default;

        // OS-defined helpers:
        //
        // sysAlloc obtains a large chunk of zeroed memory from the
        // operating system, typically on the order of a hundred kilobytes
        // or a megabyte.
        // NOTE: sysAlloc returns OS-aligned memory, but the heap allocator
        // may use larger alignment, so the caller must be careful to realign the
        // memory obtained by sysAlloc.
        //
        // SysUnused notifies the operating system that the contents
        // of the memory region are no longer needed and can be reused
        // for other purposes.
        // SysUsed notifies the operating system that the contents
        // of the memory region are needed again.
        //
        // SysFree returns it unconditionally; this is only used if
        // an out-of-memory error has been detected midway through
        // an allocation. It is okay if SysFree is a no-op.
        //
        // SysReserve reserves address space without allocating memory.
        // If the pointer passed to it is non-nil, the caller wants the
        // reservation there, but SysReserve can still choose another
        // location if that one is unavailable. On some systems and in some
        // cases SysReserve will simply check that the address space is
        // available and not actually reserve it. If SysReserve returns
        // non-nil, it sets *reserved to true if the address space is
        // reserved, false if it has merely been checked.
        // NOTE: SysReserve returns OS-aligned memory, but the heap allocator
        // may use larger alignment, so the caller must be careful to realign the
        // memory obtained by sysAlloc.
        //
        // SysMap maps previously reserved address space for use.
        // The reserved argument is true if the address space was really
        // reserved, not merely checked.
        //
        // SysFault marks a (already sysAlloc'd) region to fault
        // if accessed. Used only for debugging the runtime.

        private static void mallocinit()
        {
            if (class_to_size[_TinySizeClass] != _TinySize)
            {
                throw("bad TinySizeClass");
            }
            testdefersizes(); 

            // Copy class sizes out for statistics table.
            {
                var i__prev1 = i;

                foreach (var (__i) in class_to_size)
                {
                    i = __i;
                    memstats.by_size[i].size = uint32(class_to_size[i]);
                } 

                // Check physPageSize.

                i = i__prev1;
            }

            if (physPageSize == 0L)
            { 
                // The OS init code failed to fetch the physical page size.
                throw("failed to get system page size");
            }
            if (physPageSize < minPhysPageSize)
            {
                print("system page size (", physPageSize, ") is smaller than minimum page size (", minPhysPageSize, ")\n");
                throw("bad system page size");
            }
            if (physPageSize & (physPageSize - 1L) != 0L)
            {
                print("system page size (", physPageSize, ") must be a power of 2\n");
                throw("bad system page size");
            } 

            // The auxiliary regions start at p and are laid out in the
            // following order: spans, bitmap, arena.
            System.UIntPtr p = default;            System.UIntPtr pSize = default;

            bool reserved = default; 

            // The spans array holds one *mspan per _PageSize of arena.
            System.UIntPtr spansSize = (_MaxMem + 1L) / _PageSize * sys.PtrSize;
            spansSize = round(spansSize, _PageSize); 
            // The bitmap holds 2 bits per word of arena.
            System.UIntPtr bitmapSize = (_MaxMem + 1L) / (sys.PtrSize * 8L / 2L);
            bitmapSize = round(bitmapSize, _PageSize); 

            // Set up the allocation arena, a contiguous area of memory where
            // allocated data will be found.
            if (sys.PtrSize == 8L)
            { 
                // On a 64-bit machine, allocate from a single contiguous reservation.
                // 512 GB (MaxMem) should be big enough for now.
                //
                // The code will work with the reservation at any address, but ask
                // SysReserve to use 0x0000XXc000000000 if possible (XX=00...7f).
                // Allocating a 512 GB region takes away 39 bits, and the amd64
                // doesn't let us choose the top 17 bits, so that leaves the 9 bits
                // in the middle of 0x00c0 for us to choose. Choosing 0x00c0 means
                // that the valid memory addresses will begin 0x00c0, 0x00c1, ..., 0x00df.
                // In little-endian, that's c0 00, c1 00, ..., df 00. None of those are valid
                // UTF-8 sequences, and they are otherwise as far away from
                // ff (likely a common byte) as possible. If that fails, we try other 0xXXc0
                // addresses. An earlier attempt to use 0x11f8 caused out of memory errors
                // on OS X during thread allocations.  0x00c0 causes conflicts with
                // AddressSanitizer which reserves all memory up to 0x0100.
                // These choices are both for debuggability and to reduce the
                // odds of a conservative garbage collector (as is still used in gccgo)
                // not collecting memory because some non-pointer block of memory
                // had a bit pattern that matched a memory address.
                //
                // Actually we reserve 544 GB (because the bitmap ends up being 32 GB)
                // but it hardly matters: e0 00 is not valid UTF-8 either.
                //
                // If this fails we fall back to the 32 bit memory mechanism
                //
                // However, on arm64, we ignore all this advice above and slam the
                // allocation at 0x40 << 32 because when using 4k pages with 3-level
                // translation buffers, the user address space is limited to 39 bits
                // On darwin/arm64, the address space is even smaller.
                var arenaSize = round(_MaxMem, _PageSize);
                pSize = bitmapSize + spansSize + arenaSize + _PageSize;
                {
                    var i__prev1 = i;

                    for (long i = 0L; i <= 0x7fUL; i++)
                    {

                        if (GOARCH == "arm64" && GOOS == "darwin") 
                            p = uintptr(i) << (int)(40L) | uintptrMask & (0x0013UL << (int)(28L));
                        else if (GOARCH == "arm64") 
                            p = uintptr(i) << (int)(40L) | uintptrMask & (0x0040UL << (int)(32L));
                        else 
                            p = uintptr(i) << (int)(40L) | uintptrMask & (0x00c0UL << (int)(32L));
                                                p = uintptr(sysReserve(@unsafe.Pointer(p), pSize, ref reserved));
                        if (p != 0L)
                        {
                            break;
                        }
                    }


                    i = i__prev1;
                }
            }
            if (p == 0L)
            { 
                // On a 32-bit machine, we can't typically get away
                // with a giant virtual address space reservation.
                // Instead we map the memory information bitmap
                // immediately after the data segment, large enough
                // to handle the entire 4GB address space (256 MB),
                // along with a reservation for an initial arena.
                // When that gets used up, we'll start asking the kernel
                // for any memory anywhere.

                // We want to start the arena low, but if we're linked
                // against C code, it's possible global constructors
                // have called malloc and adjusted the process' brk.
                // Query the brk so we can avoid trying to map the
                // arena over it (which will cause the kernel to put
                // the arena somewhere else, likely at a high
                // address).
                var procBrk = sbrk0(); 

                // If we fail to allocate, try again with a smaller arena.
                // This is necessary on Android L where we share a process
                // with ART, which reserves virtual memory aggressively.
                // In the worst case, fall back to a 0-sized initial arena,
                // in the hope that subsequent reservations will succeed.
                System.UIntPtr arenaSizes = new slice<System.UIntPtr>(new System.UIntPtr[] { 512<<20, 256<<20, 128<<20, 0 });

                {
                    var arenaSize__prev1 = arenaSize;

                    foreach (var (_, __arenaSize) in arenaSizes)
                    {
                        arenaSize = __arenaSize; 
                        // SysReserve treats the address we ask for, end, as a hint,
                        // not as an absolute requirement. If we ask for the end
                        // of the data segment but the operating system requires
                        // a little more space before we can start allocating, it will
                        // give out a slightly higher pointer. Except QEMU, which
                        // is buggy, as usual: it won't adjust the pointer upward.
                        // So adjust it upward a little bit ourselves: 1/4 MB to get
                        // away from the running binary image and then round up
                        // to a MB boundary.
                        p = round(firstmoduledata.end + (1L << (int)(18L)), 1L << (int)(20L));
                        pSize = bitmapSize + spansSize + arenaSize + _PageSize;
                        if (p <= procBrk && procBrk < p + pSize)
                        { 
                            // Move the start above the brk,
                            // leaving some room for future brk
                            // expansion.
                            p = round(procBrk + (1L << (int)(20L)), 1L << (int)(20L));
                        }
                        p = uintptr(sysReserve(@unsafe.Pointer(p), pSize, ref reserved));
                        if (p != 0L)
                        {
                            break;
                        }
                    }

                    arenaSize = arenaSize__prev1;
                }

                if (p == 0L)
                {
                    throw("runtime: cannot reserve arena virtual address space");
                }
            } 

            // PageSize can be larger than OS definition of page size,
            // so SysReserve can give us a PageSize-unaligned pointer.
            // To overcome this we ask for PageSize more and round up the pointer.
            var p1 = round(p, _PageSize);
            pSize -= p1 - p;

            var spansStart = p1;
            p1 += spansSize;
            mheap_.bitmap = p1 + bitmapSize;
            p1 += bitmapSize;
            if (sys.PtrSize == 4L)
            { 
                // Set arena_start such that we can accept memory
                // reservations located anywhere in the 4GB virtual space.
                mheap_.arena_start = 0L;
            }
            else
            {
                mheap_.arena_start = p1;
            }
            mheap_.arena_end = p + pSize;
            mheap_.arena_used = p1;
            mheap_.arena_alloc = p1;
            mheap_.arena_reserved = reserved;

            if (mheap_.arena_start & (_PageSize - 1L) != 0L)
            {
                println("bad pagesize", hex(p), hex(p1), hex(spansSize), hex(bitmapSize), hex(_PageSize), "start", hex(mheap_.arena_start));
                throw("misrounded allocation in mallocinit");
            } 

            // Initialize the rest of the allocator.
            mheap_.init(spansStart, spansSize);
            var _g_ = getg();
            _g_.m.mcache = allocmcache();
        }

        // sysAlloc allocates the next n bytes from the heap arena. The
        // returned pointer is always _PageSize aligned and between
        // h.arena_start and h.arena_end. sysAlloc returns nil on failure.
        // There is no corresponding free function.
        private static unsafe.Pointer sysAlloc(this ref mheap h, System.UIntPtr n)
        { 
            // strandLimit is the maximum number of bytes to strand from
            // the current arena block. If we would need to strand more
            // than this, we fall back to sysAlloc'ing just enough for
            // this allocation.
            const long strandLimit = 16L << (int)(20L);



            if (n > h.arena_end - h.arena_alloc)
            { 
                // If we haven't grown the arena to _MaxMem yet, try
                // to reserve some more address space.
                var p_size = round(n + _PageSize, 256L << (int)(20L));
                var new_end = h.arena_end + p_size; // Careful: can overflow
                if (h.arena_end <= new_end && new_end - h.arena_start - 1L <= _MaxMem)
                { 
                    // TODO: It would be bad if part of the arena
                    // is reserved and part is not.
                    bool reserved = default;
                    var p = uintptr(sysReserve(@unsafe.Pointer(h.arena_end), p_size, ref reserved));
                    if (p == 0L)
                    { 
                        // TODO: Try smaller reservation
                        // growths in case we're in a crowded
                        // 32-bit address space.
                        goto reservationFailed;
                    } 
                    // p can be just about anywhere in the address
                    // space, including before arena_end.
                    if (p == h.arena_end)
                    { 
                        // The new block is contiguous with
                        // the current block. Extend the
                        // current arena block.
                        h.arena_end = new_end;
                        h.arena_reserved = reserved;
                    }
                    else if (h.arena_start <= p && p + p_size - h.arena_start - 1L <= _MaxMem && h.arena_end - h.arena_alloc < strandLimit)
                    { 
                        // We were able to reserve more memory
                        // within the arena space, but it's
                        // not contiguous with our previous
                        // reservation. It could be before or
                        // after our current arena_used.
                        //
                        // Keep everything page-aligned.
                        // Our pages are bigger than hardware pages.
                        h.arena_end = p + p_size;
                        p = round(p, _PageSize);
                        h.arena_alloc = p;
                        h.arena_reserved = reserved;
                    }
                    else
                    { 
                        // We got a mapping, but either
                        //
                        // 1) It's not in the arena, so we
                        // can't use it. (This should never
                        // happen on 32-bit.)
                        //
                        // 2) We would need to discard too
                        // much of our current arena block to
                        // use it.
                        //
                        // We haven't added this allocation to
                        // the stats, so subtract it from a
                        // fake stat (but avoid underflow).
                        //
                        // We'll fall back to a small sysAlloc.
                        var stat = uint64(p_size);
                        sysFree(@unsafe.Pointer(p), p_size, ref stat);
                    }
                }
            }
            if (n <= h.arena_end - h.arena_alloc)
            { 
                // Keep taking from our reservation.
                p = h.arena_alloc;
                sysMap(@unsafe.Pointer(p), n, h.arena_reserved, ref memstats.heap_sys);
                h.arena_alloc += n;
                if (h.arena_alloc > h.arena_used)
                {
                    h.setArenaUsed(h.arena_alloc, true);
                }
                if (p & (_PageSize - 1L) != 0L)
                {
                    throw("misrounded allocation in MHeap_SysAlloc");
                }
                return @unsafe.Pointer(p);
            }
reservationFailed: 

            // On 32-bit, once the reservation is gone we can
            // try to get memory at a location chosen by the OS.
            if (sys.PtrSize != 4L)
            {
                return null;
            } 

            // On 32-bit, once the reservation is gone we can
            // try to get memory at a location chosen by the OS.
            p_size = round(n, _PageSize) + _PageSize;
            p = uintptr(sysAlloc(p_size, ref memstats.heap_sys));
            if (p == 0L)
            {
                return null;
            }
            if (p < h.arena_start || p + p_size - h.arena_start > _MaxMem)
            { 
                // This shouldn't be possible because _MaxMem is the
                // whole address space on 32-bit.
                var top = uint64(h.arena_start) + _MaxMem;
                print("runtime: memory allocated by OS (", hex(p), ") not in usable range [", hex(h.arena_start), ",", hex(top), ")\n");
                sysFree(@unsafe.Pointer(p), p_size, ref memstats.heap_sys);
                return null;
            }
            p += -p & (_PageSize - 1L);
            if (p + n > h.arena_used)
            {
                h.setArenaUsed(p + n, true);
            }
            if (p & (_PageSize - 1L) != 0L)
            {
                throw("misrounded allocation in MHeap_SysAlloc");
            }
            return @unsafe.Pointer(p);
        }

        // base address for all 0-byte allocations
        private static System.UIntPtr zerobase = default;

        // nextFreeFast returns the next free object if one is quickly available.
        // Otherwise it returns 0.
        private static gclinkptr nextFreeFast(ref mspan s)
        {
            var theBit = sys.Ctz64(s.allocCache); // Is there a free object in the allocCache?
            if (theBit < 64L)
            {
                var result = s.freeindex + uintptr(theBit);
                if (result < s.nelems)
                {
                    var freeidx = result + 1L;
                    if (freeidx % 64L == 0L && freeidx != s.nelems)
                    {
                        return 0L;
                    }
                    s.allocCache >>= uint(theBit + 1L);
                    s.freeindex = freeidx;
                    s.allocCount++;
                    return gclinkptr(result * s.elemsize + s.@base());
                }
            }
            return 0L;
        }

        // nextFree returns the next free object from the cached span if one is available.
        // Otherwise it refills the cache with a span with an available object and
        // returns that object along with a flag indicating that this was a heavy
        // weight allocation. If it is a heavy weight allocation the caller must
        // determine whether a new GC cycle needs to be started or if the GC is active
        // whether this goroutine needs to assist the GC.
        private static (gclinkptr, ref mspan, bool) nextFree(this ref mcache c, spanClass spc)
        {
            s = c.alloc[spc];
            shouldhelpgc = false;
            var freeIndex = s.nextFreeIndex();
            if (freeIndex == s.nelems)
            { 
                // The span is full.
                if (uintptr(s.allocCount) != s.nelems)
                {
                    println("runtime: s.allocCount=", s.allocCount, "s.nelems=", s.nelems);
                    throw("s.allocCount != s.nelems && freeIndex == s.nelems");
                }
                systemstack(() =>
                {
                    c.refill(spc);
                });
                shouldhelpgc = true;
                s = c.alloc[spc];

                freeIndex = s.nextFreeIndex();
            }
            if (freeIndex >= s.nelems)
            {
                throw("freeIndex is not valid");
            }
            v = gclinkptr(freeIndex * s.elemsize + s.@base());
            s.allocCount++;
            if (uintptr(s.allocCount) > s.nelems)
            {
                println("s.allocCount=", s.allocCount, "s.nelems=", s.nelems);
                throw("s.allocCount > s.nelems");
            }
            return;
        }

        // Allocate an object of size bytes.
        // Small objects are allocated from the per-P cache's free lists.
        // Large objects (> 32 kB) are allocated straight from the heap.
        private static unsafe.Pointer mallocgc(System.UIntPtr size, ref _type typ, bool needzero)
        {
            if (gcphase == _GCmarktermination)
            {
                throw("mallocgc called with gcphase == _GCmarktermination");
            }
            if (size == 0L)
            {
                return @unsafe.Pointer(ref zerobase);
            }
            if (debug.sbrk != 0L)
            {
                var align = uintptr(16L);
                if (typ != null)
                {
                    align = uintptr(typ.align);
                }
                return persistentalloc(size, align, ref memstats.other_sys);
            } 

            // assistG is the G to charge for this allocation, or nil if
            // GC is not currently active.
            ref g assistG = default;
            if (gcBlackenEnabled != 0L)
            { 
                // Charge the current user G for this allocation.
                assistG = getg();
                if (assistG.m.curg != null)
                {
                    assistG = assistG.m.curg;
                } 
                // Charge the allocation against the G. We'll account
                // for internal fragmentation at the end of mallocgc.
                assistG.gcAssistBytes -= int64(size);

                if (assistG.gcAssistBytes < 0L)
                { 
                    // This G is in debt. Assist the GC to correct
                    // this before allocating. This must happen
                    // before disabling preemption.
                    gcAssistAlloc(assistG);
                }
            } 

            // Set mp.mallocing to keep from being preempted by GC.
            var mp = acquirem();
            if (mp.mallocing != 0L)
            {
                throw("malloc deadlock");
            }
            if (mp.gsignal == getg())
            {
                throw("malloc during signal");
            }
            mp.mallocing = 1L;

            var shouldhelpgc = false;
            var dataSize = size;
            var c = gomcache();
            unsafe.Pointer x = default;
            var noscan = typ == null || typ.kind & kindNoPointers != 0L;
            if (size <= maxSmallSize)
            {
                if (noscan && size < maxTinySize)
                { 
                    // Tiny allocator.
                    //
                    // Tiny allocator combines several tiny allocation requests
                    // into a single memory block. The resulting memory block
                    // is freed when all subobjects are unreachable. The subobjects
                    // must be noscan (don't have pointers), this ensures that
                    // the amount of potentially wasted memory is bounded.
                    //
                    // Size of the memory block used for combining (maxTinySize) is tunable.
                    // Current setting is 16 bytes, which relates to 2x worst case memory
                    // wastage (when all but one subobjects are unreachable).
                    // 8 bytes would result in no wastage at all, but provides less
                    // opportunities for combining.
                    // 32 bytes provides more opportunities for combining,
                    // but can lead to 4x worst case wastage.
                    // The best case winning is 8x regardless of block size.
                    //
                    // Objects obtained from tiny allocator must not be freed explicitly.
                    // So when an object will be freed explicitly, we ensure that
                    // its size >= maxTinySize.
                    //
                    // SetFinalizer has a special case for objects potentially coming
                    // from tiny allocator, it such case it allows to set finalizers
                    // for an inner byte of a memory block.
                    //
                    // The main targets of tiny allocator are small strings and
                    // standalone escaping variables. On a json benchmark
                    // the allocator reduces number of allocations by ~12% and
                    // reduces heap size by ~20%.
                    var off = c.tinyoffset; 
                    // Align tiny pointer for required (conservative) alignment.
                    if (size & 7L == 0L)
                    {
                        off = round(off, 8L);
                    }
                    else if (size & 3L == 0L)
                    {
                        off = round(off, 4L);
                    }
                    else if (size & 1L == 0L)
                    {
                        off = round(off, 2L);
                    }
                    if (off + size <= maxTinySize && c.tiny != 0L)
                    { 
                        // The object fits into existing tiny block.
                        x = @unsafe.Pointer(c.tiny + off);
                        c.tinyoffset = off + size;
                        c.local_tinyallocs++;
                        mp.mallocing = 0L;
                        releasem(mp);
                        return x;
                    } 
                    // Allocate a new maxTinySize block.
                    var span = c.alloc[tinySpanClass];
                    var v = nextFreeFast(span);
                    if (v == 0L)
                    {
                        v, _, shouldhelpgc = c.nextFree(tinySpanClass);
                    }
                    x = @unsafe.Pointer(v)(typeof(ref array<ulong>))(x)[0L];

                    0L(typeof(ref array<ulong>))(x)[1L];

                    0L; 
                    // See if we need to replace the existing tiny block with the new one
                    // based on amount of remaining free space.
                    if (size < c.tinyoffset || c.tiny == 0L)
                    {
                        c.tiny = uintptr(x);
                        c.tinyoffset = size;
                    }
                    size = maxTinySize;
                }
                else
                {
                    byte sizeclass = default;
                    if (size <= smallSizeMax - 8L)
                    {
                        sizeclass = size_to_class8[(size + smallSizeDiv - 1L) / smallSizeDiv];
                    }
                    else
                    {
                        sizeclass = size_to_class128[(size - smallSizeMax + largeSizeDiv - 1L) / largeSizeDiv];
                    }
                    size = uintptr(class_to_size[sizeclass]);
                    var spc = makeSpanClass(sizeclass, noscan);
                    span = c.alloc[spc];
                    v = nextFreeFast(span);
                    if (v == 0L)
                    {
                        v, span, shouldhelpgc = c.nextFree(spc);
                    }
                    x = @unsafe.Pointer(v);
                    if (needzero && span.needzero != 0L)
                    {
                        memclrNoHeapPointers(@unsafe.Pointer(v), size);
                    }
                }
            }
            else
            {
                ref mspan s = default;
                shouldhelpgc = true;
                systemstack(() =>
                {
                    s = largeAlloc(size, needzero, noscan);
                });
                s.freeindex = 1L;
                s.allocCount = 1L;
                x = @unsafe.Pointer(s.@base());
                size = s.elemsize;
            }
            System.UIntPtr scanSize = default;
            if (!noscan)
            { 
                // If allocating a defer+arg block, now that we've picked a malloc size
                // large enough to hold everything, cut the "asked for" size down to
                // just the defer header, so that the GC bitmap will record the arg block
                // as containing nothing at all (as if it were unused space at the end of
                // a malloc block caused by size rounding).
                // The defer arg areas are scanned as part of scanstack.
                if (typ == deferType)
                {
                    dataSize = @unsafe.Sizeof(new _defer());
                }
                heapBitsSetType(uintptr(x), size, dataSize, typ);
                if (dataSize > typ.size)
                { 
                    // Array allocation. If there are any
                    // pointers, GC has to scan to the last
                    // element.
                    if (typ.ptrdata != 0L)
                    {
                        scanSize = dataSize - typ.size + typ.ptrdata;
                    }
                }
                else
                {
                    scanSize = typ.ptrdata;
                }
                c.local_scan += scanSize;
            } 

            // Ensure that the stores above that initialize x to
            // type-safe memory and set the heap bits occur before
            // the caller can make x observable to the garbage
            // collector. Otherwise, on weakly ordered machines,
            // the garbage collector could follow a pointer to x,
            // but see uninitialized memory or stale heap bits.
            publicationBarrier(); 

            // Allocate black during GC.
            // All slots hold nil so no scanning is needed.
            // This may be racing with GC so do it atomically if there can be
            // a race marking the bit.
            if (gcphase != _GCoff)
            {
                gcmarknewobject(uintptr(x), size, scanSize);
            }
            if (raceenabled)
            {
                racemalloc(x, size);
            }
            if (msanenabled)
            {
                msanmalloc(x, size);
            }
            mp.mallocing = 0L;
            releasem(mp);

            if (debug.allocfreetrace != 0L)
            {
                tracealloc(x, size, typ);
            }
            {
                var rate = MemProfileRate;

                if (rate > 0L)
                {
                    if (size < uintptr(rate) && int32(size) < c.next_sample)
                    {
                        c.next_sample -= int32(size);
                    }
                    else
                    {
                        mp = acquirem();
                        profilealloc(mp, x, size);
                        releasem(mp);
                    }
                }

            }

            if (assistG != null)
            { 
                // Account for internal fragmentation in the assist
                // debt now that we know it.
                assistG.gcAssistBytes -= int64(size - dataSize);
            }
            if (shouldhelpgc)
            {
                {
                    gcTrigger t = (new gcTrigger(kind:gcTriggerHeap));

                    if (t.test())
                    {
                        gcStart(gcBackgroundMode, t);
                    }

                }
            }
            return x;
        }

        private static ref mspan largeAlloc(System.UIntPtr size, bool needzero, bool noscan)
        { 
            // print("largeAlloc size=", size, "\n")

            if (size + _PageSize < size)
            {
                throw("out of memory");
            }
            var npages = size >> (int)(_PageShift);
            if (size & _PageMask != 0L)
            {
                npages++;
            } 

            // Deduct credit for this span allocation and sweep if
            // necessary. mHeap_Alloc will also sweep npages, so this only
            // pays the debt down to npage pages.
            deductSweepCredit(npages * _PageSize, npages);

            var s = mheap_.alloc(npages, makeSpanClass(0L, noscan), true, needzero);
            if (s == null)
            {
                throw("out of memory");
            }
            s.limit = s.@base() + size;
            heapBitsForSpan(s.@base()).initSpan(s);
            return s;
        }

        // implementation of new builtin
        // compiler (both frontend and SSA backend) knows the signature
        // of this function
        private static unsafe.Pointer newobject(ref _type typ)
        {
            return mallocgc(typ.size, typ, true);
        }

        //go:linkname reflect_unsafe_New reflect.unsafe_New
        private static unsafe.Pointer reflect_unsafe_New(ref _type typ)
        {
            return newobject(typ);
        }

        // newarray allocates an array of n elements of type typ.
        private static unsafe.Pointer newarray(ref _type _typ, long n) => func(_typ, (ref _type typ, Defer _, Panic panic, Recover __) =>
        {
            if (n == 1L)
            {
                return mallocgc(typ.size, typ, true);
            }
            if (n < 0L || uintptr(n) > maxSliceCap(typ.size))
            {
                panic(plainError("runtime: allocation size out of range"));
            }
            return mallocgc(typ.size * uintptr(n), typ, true);
        });

        //go:linkname reflect_unsafe_NewArray reflect.unsafe_NewArray
        private static unsafe.Pointer reflect_unsafe_NewArray(ref _type typ, long n)
        {
            return newarray(typ, n);
        }

        private static void profilealloc(ref m mp, unsafe.Pointer x, System.UIntPtr size)
        {
            mp.mcache.next_sample = nextSample();
            mProf_Malloc(x, size);
        }

        // nextSample returns the next sampling point for heap profiling. The goal is
        // to sample allocations on average every MemProfileRate bytes, but with a
        // completely random distribution over the allocation timeline; this
        // corresponds to a Poisson process with parameter MemProfileRate. In Poisson
        // processes, the distance between two samples follows the exponential
        // distribution (exp(MemProfileRate)), so the best return value is a random
        // number taken from an exponential distribution whose mean is MemProfileRate.
        private static int nextSample()
        {
            if (GOOS == "plan9")
            { 
                // Plan 9 doesn't support floating point in note handler.
                {
                    var g = getg();

                    if (g == g.m.gsignal)
                    {
                        return nextSampleNoFP();
                    }

                }
            }
            return fastexprand(MemProfileRate);
        }

        // fastexprand returns a random number from an exponential distribution with
        // the specified mean.
        private static int fastexprand(long mean)
        { 
            // Avoid overflow. Maximum possible step is
            // -ln(1/(1<<randomBitCount)) * mean, approximately 20 * mean.

            if (mean > 0x7000000UL) 
                mean = 0x7000000UL;
            else if (mean == 0L) 
                return 0L;
            // Take a random sample of the exponential distribution exp(-mean*x).
            // The probability distribution function is mean*exp(-mean*x), so the CDF is
            // p = 1 - exp(-mean*x), so
            // q = 1 - p == exp(-mean*x)
            // log_e(q) = -mean*x
            // -log_e(q)/mean = x
            // x = -log_e(q) * mean
            // x = log_2(q) * (-log_e(2)) * mean    ; Using log_2 for efficiency
            const long randomBitCount = 26L;

            var q = fastrand() % (1L << (int)(randomBitCount)) + 1L;
            var qlog = fastlog2(float64(q)) - randomBitCount;
            if (qlog > 0L)
            {
                qlog = 0L;
            }
            const float minusLog2 = -0.6931471805599453F; // -ln(2)
 // -ln(2)
            return int32(qlog * (minusLog2 * float64(mean))) + 1L;
        }

        // nextSampleNoFP is similar to nextSample, but uses older,
        // simpler code to avoid floating point.
        private static int nextSampleNoFP()
        { 
            // Set first allocation sample size.
            var rate = MemProfileRate;
            if (rate > 0x3fffffffUL)
            { // make 2*rate not overflow
                rate = 0x3fffffffUL;
            }
            if (rate != 0L)
            {
                return int32(fastrand() % uint32(2L * rate));
            }
            return 0L;
        }

        private partial struct persistentAlloc
        {
            public ptr<notInHeap> @base;
            public System.UIntPtr off;
        }

        private static var globalAlloc = default;

        // Wrapper around sysAlloc that can allocate small chunks.
        // There is no associated free operation.
        // Intended for things like function/type/debug-related persistent data.
        // If align is 0, uses default align (currently 8).
        // The returned memory will be zeroed.
        //
        // Consider marking persistentalloc'd types go:notinheap.
        private static unsafe.Pointer persistentalloc(System.UIntPtr size, System.UIntPtr align, ref ulong sysStat)
        {
            ref notInHeap p = default;
            systemstack(() =>
            {
                p = persistentalloc1(size, align, sysStat);
            });
            return @unsafe.Pointer(p);
        }

        // Must run on system stack because stack growth can (re)invoke it.
        // See issue 9174.
        //go:systemstack
        private static ref notInHeap persistentalloc1(System.UIntPtr size, System.UIntPtr align, ref ulong sysStat)
        {
            const long chunk = 256L << (int)(10L);
            const long maxBlock = 64L << (int)(10L); // VM reservation granularity is 64K on windows

            if (size == 0L)
            {
                throw("persistentalloc: size == 0");
            }
            if (align != 0L)
            {
                if (align & (align - 1L) != 0L)
                {
                    throw("persistentalloc: align is not a power of 2");
                }
                if (align > _PageSize)
                {
                    throw("persistentalloc: align is too large");
                }
            }
            else
            {
                align = 8L;
            }
            if (size >= maxBlock)
            {
                return (notInHeap.Value)(sysAlloc(size, sysStat));
            }
            var mp = acquirem();
            ref persistentAlloc persistent = default;
            if (mp != null && mp.p != 0L)
            {
                persistent = ref mp.p.ptr().palloc;
            }
            else
            {
                lock(ref globalAlloc.mutex);
                persistent = ref globalAlloc.persistentAlloc;
            }
            persistent.off = round(persistent.off, align);
            if (persistent.off + size > chunk || persistent.@base == null)
            {
                persistent.@base = (notInHeap.Value)(sysAlloc(chunk, ref memstats.other_sys));
                if (persistent.@base == null)
                {
                    if (persistent == ref globalAlloc.persistentAlloc)
                    {
                        unlock(ref globalAlloc.mutex);
                    }
                    throw("runtime: cannot allocate memory");
                }
                persistent.off = 0L;
            }
            var p = persistent.@base.add(persistent.off);
            persistent.off += size;
            releasem(mp);
            if (persistent == ref globalAlloc.persistentAlloc)
            {
                unlock(ref globalAlloc.mutex);
            }
            if (sysStat != ref memstats.other_sys)
            {
                mSysStatInc(sysStat, size);
                mSysStatDec(ref memstats.other_sys, size);
            }
            return p;
        }

        // notInHeap is off-heap memory allocated by a lower-level allocator
        // like sysAlloc or persistentAlloc.
        //
        // In general, it's better to use real types marked as go:notinheap,
        // but this serves as a generic type for situations where that isn't
        // possible (like in the allocators).
        //
        // TODO: Use this as the return type of sysAlloc, persistentAlloc, etc?
        //
        //go:notinheap
        private partial struct notInHeap
        {
        }

        private static ref notInHeap add(this ref notInHeap p, System.UIntPtr bytes)
        {
            return (notInHeap.Value)(@unsafe.Pointer(uintptr(@unsafe.Pointer(p)) + bytes));
        }
    }
}
