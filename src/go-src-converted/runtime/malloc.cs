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
//    mspan: a run of in-use pages managed by the mheap.
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
//    3. Otherwise, if all objects in the mspan are free, the mspan's
//       pages are returned to the mheap and the mspan is now dead.
//
// Allocating and freeing a large object uses the mheap
// directly, bypassing the mcache and mcentral.
//
// If mspan.needzero is false, then free object slots in the mspan are
// already zeroed. Otherwise if needzero is true, objects are zeroed as
// they are allocated. There are various benefits to delaying zeroing
// this way:
//
//    1. Stack frame allocation can avoid zeroing altogether.
//
//    2. It exhibits better temporal locality, since the program is
//       probably about to write to the memory.
//
//    3. We don't zero pages that never get reused.

// Virtual memory layout
//
// The heap consists of a set of arenas, which are 64MB on 64-bit and
// 4MB on 32-bit (heapArenaBytes). Each arena's start address is also
// aligned to the arena size.
//
// Each arena has an associated heapArena object that stores the
// metadata for that arena: the heap bitmap for all words in the arena
// and the span map for all pages in the arena. heapArena objects are
// themselves allocated off-heap.
//
// Since arenas are aligned, the address space can be viewed as a
// series of arena frames. The arena map (mheap_.arenas) maps from
// arena frame number to *heapArena, or nil for parts of the address
// space not backed by the Go heap. The arena map is structured as a
// two-level array consisting of a "L1" arena map and many "L2" arena
// maps; however, since arenas are large, on many architectures, the
// arena map consists of a single, large L2 map.
//
// The arena map covers the entire possible address space, allowing
// the Go heap to use any part of the address space. The allocator
// attempts to keep arenas contiguous so that large spans (and hence
// large objects) can cross arenas.

// package runtime -- go2cs converted at 2020 October 08 03:20:07 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\malloc.go
using atomic = go.runtime.@internal.atomic_package;
using math = go.runtime.@internal.math_package;
using sys = go.runtime.@internal.sys_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class runtime_package
    {
        private static readonly var debugMalloc = (var)false;

        private static readonly var maxTinySize = (var)_TinySize;
        private static readonly var tinySizeClass = (var)_TinySizeClass;
        private static readonly var maxSmallSize = (var)_MaxSmallSize;

        private static readonly var pageShift = (var)_PageShift;
        private static readonly var pageSize = (var)_PageSize;
        private static readonly var pageMask = (var)_PageMask; 
        // By construction, single page spans of the smallest object class
        // have the most objects per span.
        private static readonly var maxObjsPerSpan = (var)pageSize / 8L;

        private static readonly var concurrentSweep = (var)_ConcurrentSweep;

        private static readonly long _PageSize = (long)1L << (int)(_PageShift);
        private static readonly var _PageMask = (var)_PageSize - 1L; 

        // _64bit = 1 on 64-bit systems, 0 on 32-bit systems
        private static readonly long _64bit = (long)1L << (int)((~uintptr(0L) >> (int)(63L))) / 2L; 

        // Tiny allocator parameters, see "Tiny allocator" comment in malloc.go.
        private static readonly long _TinySize = (long)16L;
        private static readonly var _TinySizeClass = (var)int8(2L);

        private static readonly long _FixAllocChunk = (long)16L << (int)(10L); // Chunk size for FixAlloc

        // Per-P, per order stack segment cache size.
        private static readonly long _StackCacheSize = (long)32L * 1024L; 

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
        private static readonly long _NumStackOrders = (long)4L - sys.PtrSize / 4L * sys.GoosWindows - 1L * sys.GoosPlan9; 

        // heapAddrBits is the number of bits in a heap address. On
        // amd64, addresses are sign-extended beyond heapAddrBits. On
        // other arches, they are zero-extended.
        //
        // On most 64-bit platforms, we limit this to 48 bits based on a
        // combination of hardware and OS limitations.
        //
        // amd64 hardware limits addresses to 48 bits, sign-extended
        // to 64 bits. Addresses where the top 16 bits are not either
        // all 0 or all 1 are "non-canonical" and invalid. Because of
        // these "negative" addresses, we offset addresses by 1<<47
        // (arenaBaseOffset) on amd64 before computing indexes into
        // the heap arenas index. In 2017, amd64 hardware added
        // support for 57 bit addresses; however, currently only Linux
        // supports this extension and the kernel will never choose an
        // address above 1<<47 unless mmap is called with a hint
        // address above 1<<47 (which we never do).
        //
        // arm64 hardware (as of ARMv8) limits user addresses to 48
        // bits, in the range [0, 1<<48).
        //
        // ppc64, mips64, and s390x support arbitrary 64 bit addresses
        // in hardware. On Linux, Go leans on stricter OS limits. Based
        // on Linux's processor.h, the user address space is limited as
        // follows on 64-bit architectures:
        //
        // Architecture  Name              Maximum Value (exclusive)
        // ---------------------------------------------------------------------
        // amd64         TASK_SIZE_MAX     0x007ffffffff000 (47 bit addresses)
        // arm64         TASK_SIZE_64      0x01000000000000 (48 bit addresses)
        // ppc64{,le}    TASK_SIZE_USER64  0x00400000000000 (46 bit addresses)
        // mips64{,le}   TASK_SIZE64       0x00010000000000 (40 bit addresses)
        // s390x         TASK_SIZE         1<<64 (64 bit addresses)
        //
        // These limits may increase over time, but are currently at
        // most 48 bits except on s390x. On all architectures, Linux
        // starts placing mmap'd regions at addresses that are
        // significantly below 48 bits, so even if it's possible to
        // exceed Go's 48 bit limit, it's extremely unlikely in
        // practice.
        //
        // On 32-bit platforms, we accept the full 32-bit address
        // space because doing so is cheap.
        // mips32 only has access to the low 2GB of virtual memory, so
        // we further limit it to 31 bits.
        //
        // On darwin/arm64, although 64-bit pointers are presumably
        // available, pointers are truncated to 33 bits. Furthermore,
        // only the top 4 GiB of the address space are actually available
        // to the application, but we allow the whole 33 bits anyway for
        // simplicity.
        // TODO(mknyszek): Consider limiting it to 32 bits and using
        // arenaBaseOffset to offset into the top 4 GiB.
        //
        // WebAssembly currently has a limit of 4GB linear memory.
        private static readonly var heapAddrBits = (var)(_64bit * (1L - sys.GoarchWasm) * (1L - sys.GoosDarwin * sys.GoarchArm64)) * 48L + (1L - _64bit + sys.GoarchWasm) * (32L - (sys.GoarchMips + sys.GoarchMipsle)) + 33L * sys.GoosDarwin * sys.GoarchArm64; 

        // maxAlloc is the maximum size of an allocation. On 64-bit,
        // it's theoretically possible to allocate 1<<heapAddrBits bytes. On
        // 32-bit, however, this is one less than 1<<32 because the
        // number of bytes in the address space doesn't actually fit
        // in a uintptr.
        private static readonly long maxAlloc = (long)(1L << (int)(heapAddrBits)) - (1L - _64bit) * 1L; 

        // The number of bits in a heap address, the size of heap
        // arenas, and the L1 and L2 arena map sizes are related by
        //
        //   (1 << addr bits) = arena size * L1 entries * L2 entries
        //
        // Currently, we balance these as follows:
        //
        //       Platform  Addr bits  Arena size  L1 entries   L2 entries
        // --------------  ---------  ----------  ----------  -----------
        //       */64-bit         48        64MB           1    4M (32MB)
        // windows/64-bit         48         4MB          64    1M  (8MB)
        //       */32-bit         32         4MB           1  1024  (4KB)
        //     */mips(le)         31         4MB           1   512  (2KB)

        // heapArenaBytes is the size of a heap arena. The heap
        // consists of mappings of size heapArenaBytes, aligned to
        // heapArenaBytes. The initial heap mapping is one arena.
        //
        // This is currently 64MB on 64-bit non-Windows and 4MB on
        // 32-bit and on Windows. We use smaller arenas on Windows
        // because all committed memory is charged to the process,
        // even if it's not touched. Hence, for processes with small
        // heaps, the mapped arena space needs to be commensurate.
        // This is particularly important with the race detector,
        // since it significantly amplifies the cost of committed
        // memory.
        private static readonly long heapArenaBytes = (long)1L << (int)(logHeapArenaBytes); 

        // logHeapArenaBytes is log_2 of heapArenaBytes. For clarity,
        // prefer using heapArenaBytes where possible (we need the
        // constant to compute some other constants).
        private static readonly long logHeapArenaBytes = (long)(6L + 20L) * (_64bit * (1L - sys.GoosWindows) * (1L - sys.GoarchWasm)) + (2L + 20L) * (_64bit * sys.GoosWindows) + (2L + 20L) * (1L - _64bit) + (2L + 20L) * sys.GoarchWasm; 

        // heapArenaBitmapBytes is the size of each heap arena's bitmap.
        private static readonly var heapArenaBitmapBytes = (var)heapArenaBytes / (sys.PtrSize * 8L / 2L);

        private static readonly var pagesPerArena = (var)heapArenaBytes / pageSize; 

        // arenaL1Bits is the number of bits of the arena number
        // covered by the first level arena map.
        //
        // This number should be small, since the first level arena
        // map requires PtrSize*(1<<arenaL1Bits) of space in the
        // binary's BSS. It can be zero, in which case the first level
        // index is effectively unused. There is a performance benefit
        // to this, since the generated code can be more efficient,
        // but comes at the cost of having a large L2 mapping.
        //
        // We use the L1 map on 64-bit Windows because the arena size
        // is small, but the address space is still 48 bits, and
        // there's a high cost to having a large L2.
        private static readonly long arenaL1Bits = (long)6L * (_64bit * sys.GoosWindows); 

        // arenaL2Bits is the number of bits of the arena number
        // covered by the second level arena index.
        //
        // The size of each arena map allocation is proportional to
        // 1<<arenaL2Bits, so it's important that this not be too
        // large. 48 bits leads to 32MB arena index allocations, which
        // is about the practical threshold.
        private static readonly var arenaL2Bits = (var)heapAddrBits - logHeapArenaBytes - arenaL1Bits; 

        // arenaL1Shift is the number of bits to shift an arena frame
        // number by to compute an index into the first level arena map.
        private static readonly var arenaL1Shift = (var)arenaL2Bits; 

        // arenaBits is the total bits in a combined arena map index.
        // This is split between the index into the L1 arena map and
        // the L2 arena map.
        private static readonly var arenaBits = (var)arenaL1Bits + arenaL2Bits; 

        // arenaBaseOffset is the pointer value that corresponds to
        // index 0 in the heap arena map.
        //
        // On amd64, the address space is 48 bits, sign extended to 64
        // bits. This offset lets us handle "negative" addresses (or
        // high addresses if viewed as unsigned).
        //
        // On aix/ppc64, this offset allows to keep the heapAddrBits to
        // 48. Otherwize, it would be 60 in order to handle mmap addresses
        // (in range 0x0a00000000000000 - 0x0afffffffffffff). But in this
        // case, the memory reserved in (s *pageAlloc).init for chunks
        // is causing important slowdowns.
        //
        // On other platforms, the user address space is contiguous
        // and starts at 0, so no offset is necessary.
        private static readonly ulong arenaBaseOffset = (ulong)0xffff800000000000UL * sys.GoarchAmd64 + 0x0a00000000000000UL * sys.GoosAix; 
        // A typed version of this constant that will make it into DWARF (for viewcore).
        private static readonly var arenaBaseOffsetUintptr = (var)uintptr(arenaBaseOffset); 

        // Max number of threads to run garbage collection.
        // 2, 3, and 4 are all plausible maximums depending
        // on the hardware details of the machine. The garbage
        // collector scales well to 32 cpus.
        private static readonly long _MaxGcproc = (long)32L; 

        // minLegalPointer is the smallest possible legal pointer.
        // This is the smallest possible architectural page size,
        // since we assume that the first page is never mapped.
        //
        // This should agree with minZeroPage in the compiler.
        private static readonly System.UIntPtr minLegalPointer = (System.UIntPtr)4096L;


        // physPageSize is the size in bytes of the OS's physical pages.
        // Mapping and unmapping operations must be done at multiples of
        // physPageSize.
        //
        // This must be set by the OS init code (typically in osinit) before
        // mallocinit.
        private static System.UIntPtr physPageSize = default;

        // physHugePageSize is the size in bytes of the OS's default physical huge
        // page size whose allocation is opaque to the application. It is assumed
        // and verified to be a power of two.
        //
        // If set, this must be set by the OS init code (typically in osinit) before
        // mallocinit. However, setting it at all is optional, and leaving the default
        // value is always safe (though potentially less efficient).
        //
        // Since physHugePageSize is always assumed to be a power of two,
        // physHugePageShift is defined as physHugePageSize == 1 << physHugePageShift.
        // The purpose of physHugePageShift is to avoid doing divisions in
        // performance critical functions.
        private static System.UIntPtr physHugePageSize = default;        private static ulong physHugePageShift = default;

        // OS memory management abstraction layer
        //
        // Regions of the address space managed by the runtime may be in one of four
        // states at any given time:
        // 1) None - Unreserved and unmapped, the default state of any region.
        // 2) Reserved - Owned by the runtime, but accessing it would cause a fault.
        //               Does not count against the process' memory footprint.
        // 3) Prepared - Reserved, intended not to be backed by physical memory (though
        //               an OS may implement this lazily). Can transition efficiently to
        //               Ready. Accessing memory in such a region is undefined (may
        //               fault, may give back unexpected zeroes, etc.).
        // 4) Ready - may be accessed safely.
        //
        // This set of states is more than is strictly necessary to support all the
        // currently supported platforms. One could get by with just None, Reserved, and
        // Ready. However, the Prepared state gives us flexibility for performance
        // purposes. For example, on POSIX-y operating systems, Reserved is usually a
        // private anonymous mmap'd region with PROT_NONE set, and to transition
        // to Ready would require setting PROT_READ|PROT_WRITE. However the
        // underspecification of Prepared lets us use just MADV_FREE to transition from
        // Ready to Prepared. Thus with the Prepared state we can set the permission
        // bits just once early on, we can efficiently tell the OS that it's free to
        // take pages away from us when we don't strictly need them.
        //
        // For each OS there is a common set of helpers defined that transition
        // memory regions between these states. The helpers are as follows:
        //
        // sysAlloc transitions an OS-chosen region of memory from None to Ready.
        // More specifically, it obtains a large chunk of zeroed memory from the
        // operating system, typically on the order of a hundred kilobytes
        // or a megabyte. This memory is always immediately available for use.
        //
        // sysFree transitions a memory region from any state to None. Therefore, it
        // returns memory unconditionally. It is used if an out-of-memory error has been
        // detected midway through an allocation or to carve out an aligned section of
        // the address space. It is okay if sysFree is a no-op only if sysReserve always
        // returns a memory region aligned to the heap allocator's alignment
        // restrictions.
        //
        // sysReserve transitions a memory region from None to Reserved. It reserves
        // address space in such a way that it would cause a fatal fault upon access
        // (either via permissions or not committing the memory). Such a reservation is
        // thus never backed by physical memory.
        // If the pointer passed to it is non-nil, the caller wants the
        // reservation there, but sysReserve can still choose another
        // location if that one is unavailable.
        // NOTE: sysReserve returns OS-aligned memory, but the heap allocator
        // may use larger alignment, so the caller must be careful to realign the
        // memory obtained by sysReserve.
        //
        // sysMap transitions a memory region from Reserved to Prepared. It ensures the
        // memory region can be efficiently transitioned to Ready.
        //
        // sysUsed transitions a memory region from Prepared to Ready. It notifies the
        // operating system that the memory region is needed and ensures that the region
        // may be safely accessed. This is typically a no-op on systems that don't have
        // an explicit commit step and hard over-commit limits, but is critical on
        // Windows, for example.
        //
        // sysUnused transitions a memory region from Ready to Prepared. It notifies the
        // operating system that the physical pages backing this memory region are no
        // longer needed and can be reused for other purposes. The contents of a
        // sysUnused memory region are considered forfeit and the region must not be
        // accessed again until sysUsed is called.
        //
        // sysFault transitions a memory region from Ready or Prepared to Reserved. It
        // marks a region such that it will always fault if accessed. Used only for
        // debugging the runtime.

        private static void mallocinit()
        {
            if (class_to_size[_TinySizeClass] != _TinySize)
            {
                throw("bad TinySizeClass");
            }

            testdefersizes();

            if (heapArenaBitmapBytes & (heapArenaBitmapBytes - 1L) != 0L)
            { 
                // heapBits expects modular arithmetic on bitmap
                // addresses to work.
                throw("heapArenaBitmapBytes not a power of 2");

            } 

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

            if (physPageSize > maxPhysPageSize)
            {
                print("system page size (", physPageSize, ") is larger than maximum page size (", maxPhysPageSize, ")\n");
                throw("bad system page size");
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

            if (physHugePageSize & (physHugePageSize - 1L) != 0L)
            {
                print("system huge page size (", physHugePageSize, ") must be a power of 2\n");
                throw("bad system huge page size");
            }

            if (physHugePageSize > maxPhysHugePageSize)
            { 
                // physHugePageSize is greater than the maximum supported huge page size.
                // Don't throw here, like in the other cases, since a system configured
                // in this way isn't wrong, we just don't have the code to support them.
                // Instead, silently set the huge page size to zero.
                physHugePageSize = 0L;

            }

            if (physHugePageSize != 0L)
            { 
                // Since physHugePageSize is a power of 2, it suffices to increase
                // physHugePageShift until 1<<physHugePageShift == physHugePageSize.
                while (1L << (int)(physHugePageShift) != physHugePageSize)
                {
                    physHugePageShift++;
                }


            }

            if (pagesPerArena % pagesPerSpanRoot != 0L)
            {
                print("pagesPerArena (", pagesPerArena, ") is not divisible by pagesPerSpanRoot (", pagesPerSpanRoot, ")\n");
                throw("bad pagesPerSpanRoot");
            }

            if (pagesPerArena % pagesPerReclaimerChunk != 0L)
            {
                print("pagesPerArena (", pagesPerArena, ") is not divisible by pagesPerReclaimerChunk (", pagesPerReclaimerChunk, ")\n");
                throw("bad pagesPerReclaimerChunk");
            } 

            // Initialize the heap.
            mheap_.init();
            mcache0 = allocmcache();
            lockInit(_addr_gcBitsArenas.@lock, lockRankGcBitsArenas);
            lockInit(_addr_proflock, lockRankProf);
            lockInit(_addr_globalAlloc.mutex, lockRankGlobalAlloc); 

            // Create initial arena growth hints.
            if (sys.PtrSize == 8L)
            { 
                // On a 64-bit machine, we pick the following hints
                // because:
                //
                // 1. Starting from the middle of the address space
                // makes it easier to grow out a contiguous range
                // without running in to some other mapping.
                //
                // 2. This makes Go heap addresses more easily
                // recognizable when debugging.
                //
                // 3. Stack scanning in gccgo is still conservative,
                // so it's important that addresses be distinguishable
                // from other data.
                //
                // Starting at 0x00c0 means that the valid memory addresses
                // will begin 0x00c0, 0x00c1, ...
                // In little-endian, that's c0 00, c1 00, ... None of those are valid
                // UTF-8 sequences, and they are otherwise as far away from
                // ff (likely a common byte) as possible. If that fails, we try other 0xXXc0
                // addresses. An earlier attempt to use 0x11f8 caused out of memory errors
                // on OS X during thread allocations.  0x00c0 causes conflicts with
                // AddressSanitizer which reserves all memory up to 0x0100.
                // These choices reduce the odds of a conservative garbage collector
                // not collecting memory because some non-pointer block of memory
                // had a bit pattern that matched a memory address.
                //
                // However, on arm64, we ignore all this advice above and slam the
                // allocation at 0x40 << 32 because when using 4k pages with 3-level
                // translation buffers, the user address space is limited to 39 bits
                // On darwin/arm64, the address space is even smaller.
                //
                // On AIX, mmaps starts at 0x0A00000000000000 for 64-bit.
                // processes.
                {
                    var i__prev1 = i;

                    for (ulong i = 0x7fUL; i >= 0L; i--)
                    {
                        System.UIntPtr p = default;

                        if (GOARCH == "arm64" && GOOS == "darwin") 
                            p = uintptr(i) << (int)(40L) | uintptrMask & (0x0013UL << (int)(28L));
                        else if (GOARCH == "arm64") 
                            p = uintptr(i) << (int)(40L) | uintptrMask & (0x0040UL << (int)(32L));
                        else if (GOOS == "aix") 
                            if (i == 0L)
                            { 
                                // We don't use addresses directly after 0x0A00000000000000
                                // to avoid collisions with others mmaps done by non-go programs.
                                continue;

                            }

                            p = uintptr(i) << (int)(40L) | uintptrMask & (0xa0UL << (int)(52L));
                        else if (raceenabled) 
                            // The TSAN runtime requires the heap
                            // to be in the range [0x00c000000000,
                            // 0x00e000000000).
                            p = uintptr(i) << (int)(32L) | uintptrMask & (0x00c0UL << (int)(32L));
                            if (p >= uintptrMask & 0x00e000000000UL)
                            {
                                continue;
                            }

                        else 
                            p = uintptr(i) << (int)(40L) | uintptrMask & (0x00c0UL << (int)(32L));
                                                var hint = (arenaHint.val)(mheap_.arenaHintAlloc.alloc());
                        hint.addr = p;
                        hint.next = mheap_.arenaHints;
                        mheap_.arenaHints = hint;

                    }
            else


                    i = i__prev1;
                }

            }            { 
                // On a 32-bit machine, we're much more concerned
                // about keeping the usable heap contiguous.
                // Hence:
                //
                // 1. We reserve space for all heapArenas up front so
                // they don't get interleaved with the heap. They're
                // ~258MB, so this isn't too bad. (We could reserve a
                // smaller amount of space up front if this is a
                // problem.)
                //
                // 2. We hint the heap to start right above the end of
                // the binary so we have the best chance of keeping it
                // contiguous.
                //
                // 3. We try to stake out a reasonably large initial
                // heap reservation.

                const long arenaMetaSize = (long)(1L << (int)(arenaBits)) * @unsafe.Sizeof(new heapArena());

                var meta = uintptr(sysReserve(null, arenaMetaSize));
                if (meta != 0L)
                {
                    mheap_.heapArenaAlloc.init(meta, arenaMetaSize);
                } 

                // We want to start the arena low, but if we're linked
                // against C code, it's possible global constructors
                // have called malloc and adjusted the process' brk.
                // Query the brk so we can avoid trying to map the
                // region over it (which will cause the kernel to put
                // the region somewhere else, likely at a high
                // address).
                var procBrk = sbrk0(); 

                // If we ask for the end of the data segment but the
                // operating system requires a little more space
                // before we can start allocating, it will give out a
                // slightly higher pointer. Except QEMU, which is
                // buggy, as usual: it won't adjust the pointer
                // upward. So adjust it upward a little bit ourselves:
                // 1/4 MB to get away from the running binary image.
                p = firstmoduledata.end;
                if (p < procBrk)
                {
                    p = procBrk;
                }

                if (mheap_.heapArenaAlloc.next <= p && p < mheap_.heapArenaAlloc.end)
                {
                    p = mheap_.heapArenaAlloc.end;
                }

                p = alignUp(p + (256L << (int)(10L)), heapArenaBytes); 
                // Because we're worried about fragmentation on
                // 32-bit, we try to make a large initial reservation.
                System.UIntPtr arenaSizes = new slice<System.UIntPtr>(new System.UIntPtr[] { 512<<20, 256<<20, 128<<20 });
                foreach (var (_, arenaSize) in arenaSizes)
                {
                    var (a, size) = sysReserveAligned(@unsafe.Pointer(p), arenaSize, heapArenaBytes);
                    if (a != null)
                    {
                        mheap_.arena.init(uintptr(a), size);
                        p = mheap_.arena.end; // For hint below
                        break;

                    }

                }
                hint = (arenaHint.val)(mheap_.arenaHintAlloc.alloc());
                hint.addr = p;
                hint.next = mheap_.arenaHints;
                mheap_.arenaHints = hint;

            }

        }

        // sysAlloc allocates heap arena space for at least n bytes. The
        // returned pointer is always heapArenaBytes-aligned and backed by
        // h.arenas metadata. The returned size is always a multiple of
        // heapArenaBytes. sysAlloc returns nil on failure.
        // There is no corresponding free function.
        //
        // sysAlloc returns a memory region in the Prepared state. This region must
        // be transitioned to Ready before use.
        //
        // h must be locked.
        private static (unsafe.Pointer, System.UIntPtr) sysAlloc(this ptr<mheap> _addr_h, System.UIntPtr n)
        {
            unsafe.Pointer v = default;
            System.UIntPtr size = default;
            ref mheap h = ref _addr_h.val;

            n = alignUp(n, heapArenaBytes); 

            // First, try the arena pre-reservation.
            v = h.arena.alloc(n, heapArenaBytes, _addr_memstats.heap_sys);
            if (v != null)
            {
                size = n;
                goto mapped;
            } 

            // Try to grow the heap at a hint address.
            while (h.arenaHints != null)
            {
                var hint = h.arenaHints;
                var p = hint.addr;
                if (hint.down)
                {
                    p -= n;
                }

                if (p + n < p)
                { 
                    // We can't use this, so don't ask.
                    v = null;

                }
                else if (arenaIndex(p + n - 1L) >= 1L << (int)(arenaBits))
                { 
                    // Outside addressable heap. Can't use.
                    v = null;

                }
                else
                {
                    v = sysReserve(@unsafe.Pointer(p), n);
                }

                if (p == uintptr(v))
                { 
                    // Success. Update the hint.
                    if (!hint.down)
                    {
                        p += n;
                    }

                    hint.addr = p;
                    size = n;
                    break;

                } 
                // Failed. Discard this hint and try the next.
                //
                // TODO: This would be cleaner if sysReserve could be
                // told to only return the requested address. In
                // particular, this is already how Windows behaves, so
                // it would simplify things there.
                if (v != null)
                {
                    sysFree(v, n, null);
                }

                h.arenaHints = hint.next;
                h.arenaHintAlloc.free(@unsafe.Pointer(hint));

            }


            if (size == 0L)
            {
                if (raceenabled)
                { 
                    // The race detector assumes the heap lives in
                    // [0x00c000000000, 0x00e000000000), but we
                    // just ran out of hints in this region. Give
                    // a nice failure.
                    throw("too many address space collisions for -race mode");

                } 

                // All of the hints failed, so we'll take any
                // (sufficiently aligned) address the kernel will give
                // us.
                v, size = sysReserveAligned(null, n, heapArenaBytes);
                if (v == null)
                {
                    return (null, 0L);
                } 

                // Create new hints for extending this region.
                hint = (arenaHint.val)(h.arenaHintAlloc.alloc());
                hint.addr = uintptr(v);
                hint.down = true;
                hint.next = mheap_.arenaHints;
                mheap_.arenaHints = hint;
                hint = (arenaHint.val)(h.arenaHintAlloc.alloc());
                hint.addr = uintptr(v) + size;
                hint.next = mheap_.arenaHints;
                mheap_.arenaHints = hint;

            } 

            // Check for bad pointers or pointers we can't use.
            {
                @string bad = default;
                p = uintptr(v);
                if (p + size < p)
                {
                    bad = "region exceeds uintptr range";
                }
                else if (arenaIndex(p) >= 1L << (int)(arenaBits))
                {
                    bad = "base outside usable address space";
                }
                else if (arenaIndex(p + size - 1L) >= 1L << (int)(arenaBits))
                {
                    bad = "end outside usable address space";
                }

                if (bad != "")
                { 
                    // This should be impossible on most architectures,
                    // but it would be really confusing to debug.
                    print("runtime: memory allocated by OS [", hex(p), ", ", hex(p + size), ") not in usable address space: ", bad, "\n");
                    throw("memory reservation exceeds address space limit");

                }

            }
            if (uintptr(v) & (heapArenaBytes - 1L) != 0L)
            {
                throw("misrounded allocation in sysAlloc");
            } 

            // Transition from Reserved to Prepared.
            sysMap(v, size, _addr_memstats.heap_sys);

mapped: 

            // Tell the race detector about the new heap memory.
            for (var ri = arenaIndex(uintptr(v)); ri <= arenaIndex(uintptr(v) + size - 1L); ri++)
            {
                var l2 = h.arenas[ri.l1()];
                if (l2 == null)
                { 
                    // Allocate an L2 arena map.
                    l2 = new ptr<ptr<array<ptr<heapArena>>>>(persistentalloc(@unsafe.Sizeof(l2.val), sys.PtrSize, _addr_null));
                    if (l2 == null)
                    {
                        throw("out of memory allocating heap arena map");
                    }

                    atomic.StorepNoWB(@unsafe.Pointer(_addr_h.arenas[ri.l1()]), @unsafe.Pointer(l2));

                }

                if (l2[ri.l2()] != null)
                {
                    throw("arena already initialized");
                }

                ptr<heapArena> r;
                r = (heapArena.val)(h.heapArenaAlloc.alloc(@unsafe.Sizeof(r.val), sys.PtrSize, _addr_memstats.gc_sys));
                if (r == null)
                {
                    r = (heapArena.val)(persistentalloc(@unsafe.Sizeof(r.val), sys.PtrSize, _addr_memstats.gc_sys));
                    if (r == null)
                    {
                        throw("out of memory allocating heap arena metadata");
                    }

                } 

                // Add the arena to the arenas list.
                if (len(h.allArenas) == cap(h.allArenas))
                {
                    long size = 2L * uintptr(cap(h.allArenas)) * sys.PtrSize;
                    if (size == 0L)
                    {
                        size = physPageSize;
                    }

                    var newArray = (notInHeap.val)(persistentalloc(size, sys.PtrSize, _addr_memstats.gc_sys));
                    if (newArray == null)
                    {
                        throw("out of memory allocating allArenas");
                    }

                    var oldSlice = h.allArenas * (notInHeapSlice.val)(@unsafe.Pointer(_addr_h.allArenas));

                    new notInHeapSlice(newArray,len(h.allArenas),int(size/sys.PtrSize));
                    copy(h.allArenas, oldSlice); 
                    // Do not free the old backing array because
                    // there may be concurrent readers. Since we
                    // double the array each time, this can lead
                    // to at most 2x waste.
                }

                h.allArenas = h.allArenas[..len(h.allArenas) + 1L];
                h.allArenas[len(h.allArenas) - 1L] = ri; 

                // Store atomically just in case an object from the
                // new heap arena becomes visible before the heap lock
                // is released (which shouldn't happen, but there's
                // little downside to this).
                atomic.StorepNoWB(@unsafe.Pointer(_addr_l2[ri.l2()]), @unsafe.Pointer(r));

            } 

            // Tell the race detector about the new heap memory.
 

            // Tell the race detector about the new heap memory.
            if (raceenabled)
            {
                racemapshadow(v, size);
            }

            return ;

        }

        // sysReserveAligned is like sysReserve, but the returned pointer is
        // aligned to align bytes. It may reserve either n or n+align bytes,
        // so it returns the size that was reserved.
        private static (unsafe.Pointer, System.UIntPtr) sysReserveAligned(unsafe.Pointer v, System.UIntPtr size, System.UIntPtr align)
        {
            unsafe.Pointer _p0 = default;
            System.UIntPtr _p0 = default;
 
            // Since the alignment is rather large in uses of this
            // function, we're not likely to get it by chance, so we ask
            // for a larger region and remove the parts we don't need.
            long retries = 0L;
retry:
            var p = uintptr(sysReserve(v, size + align));

            if (p == 0L) 
                return (null, 0L);
            else if (p & (align - 1L) == 0L) 
                // We got lucky and got an aligned region, so we can
                // use the whole thing.
                return (@unsafe.Pointer(p), size + align);
            else if (GOOS == "windows") 
                // On Windows we can't release pieces of a
                // reservation, so we release the whole thing and
                // re-reserve the aligned sub-region. This may race,
                // so we may have to try again.
                sysFree(@unsafe.Pointer(p), size + align, null);
                p = alignUp(p, align);
                var p2 = sysReserve(@unsafe.Pointer(p), size);
                if (p != uintptr(p2))
                { 
                    // Must have raced. Try again.
                    sysFree(p2, size, null);
                    retries++;

                    if (retries == 100L)
                    {
                        throw("failed to allocate aligned heap memory; too many retries");
                    }

                    goto retry;

                } 
                // Success.
                return (p2, size);
            else 
                // Trim off the unaligned parts.
                var pAligned = alignUp(p, align);
                sysFree(@unsafe.Pointer(p), pAligned - p, null);
                var end = pAligned + size;
                var endLen = (p + size + align) - end;
                if (endLen > 0L)
                {
                    sysFree(@unsafe.Pointer(end), endLen, null);
                }

                return (@unsafe.Pointer(pAligned), size);
            
        }

        // base address for all 0-byte allocations
        private static System.UIntPtr zerobase = default;

        // nextFreeFast returns the next free object if one is quickly available.
        // Otherwise it returns 0.
        private static gclinkptr nextFreeFast(ptr<mspan> _addr_s)
        {
            ref mspan s = ref _addr_s.val;

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
        //
        // Must run in a non-preemptible context since otherwise the owner of
        // c could change.
        private static (gclinkptr, ptr<mspan>, bool) nextFree(this ptr<mcache> _addr_c, spanClass spc)
        {
            gclinkptr v = default;
            ptr<mspan> s = default!;
            bool shouldhelpgc = default;
            ref mcache c = ref _addr_c.val;

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

                c.refill(spc);
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

            return ;

        }

        // Allocate an object of size bytes.
        // Small objects are allocated from the per-P cache's free lists.
        // Large objects (> 32 kB) are allocated straight from the heap.
        private static unsafe.Pointer mallocgc(System.UIntPtr size, ptr<_type> _addr_typ, bool needzero)
        {
            ref _type typ = ref _addr_typ.val;

            if (gcphase == _GCmarktermination)
            {
                throw("mallocgc called with gcphase == _GCmarktermination");
            }

            if (size == 0L)
            {
                return @unsafe.Pointer(_addr_zerobase);
            }

            if (debug.sbrk != 0L)
            {
                var align = uintptr(16L);
                if (typ != null)
                { 
                    // TODO(austin): This should be just
                    //   align = uintptr(typ.align)
                    // but that's only 4 on 32-bit platforms,
                    // even if there's a uint64 field in typ (see #599).
                    // This causes 64-bit atomic accesses to panic.
                    // Hence, we use stricter alignment that matches
                    // the normal allocator better.
                    if (size & 7L == 0L)
                    {
                        align = 8L;
                    }
                    else if (size & 3L == 0L)
                    {
                        align = 4L;
                    }
                    else if (size & 1L == 0L)
                    {
                        align = 2L;
                    }
                    else
                    {
                        align = 1L;
                    }

                }

                return persistentalloc(size, align, _addr_memstats.other_sys);

            } 

            // assistG is the G to charge for this allocation, or nil if
            // GC is not currently active.
            ptr<g> assistG;
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
            ptr<mcache> c;
            if (mp.p != 0L)
            {
                c = mp.p.ptr().mcache;
            }
            else
            { 
                // We will be called without a P while bootstrapping,
                // in which case we use mcache0, which is set in mallocinit.
                // mcache0 is cleared when bootstrapping is complete,
                // by procresize.
                c = mcache0;
                if (c == null)
                {
                    throw("malloc called with no P");
                }

            }

            ptr<mspan> span;
            unsafe.Pointer x = default;
            var noscan = typ == null || typ.ptrdata == 0L;
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
                        off = alignUp(off, 8L);
                    }
                    else if (size & 3L == 0L)
                    {
                        off = alignUp(off, 4L);
                    }
                    else if (size & 1L == 0L)
                    {
                        off = alignUp(off, 2L);
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
                    span = c.alloc[tinySpanClass];
                    var v = nextFreeFast(span);
                    if (v == 0L)
                    {
                        v, span, shouldhelpgc = c.nextFree(tinySpanClass);
                    }

                    x = @unsafe.Pointer(v)(typeof(ptr<array<ulong>>))(x)[0L];

                    0L(typeof(ptr<array<ulong>>))(x)[1L];

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
                        sizeclass = size_to_class8[divRoundUp(size, smallSizeDiv)];
                    }
                    else
                    {
                        sizeclass = size_to_class128[divRoundUp(size - smallSizeMax, largeSizeDiv)];
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
                shouldhelpgc = true;
                systemstack(() =>
                {
                    span = largeAlloc(size, needzero, noscan);
                });
                span.freeindex = 1L;
                span.allocCount = 1L;
                x = @unsafe.Pointer(span.@base());
                size = span.elemsize;

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
                gcmarknewobject(span, uintptr(x), size, scanSize);
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
                    if (rate != 1L && size < c.next_sample)
                    {
                        c.next_sample -= size;
                    }
                    else
                    {
                        mp = acquirem();
                        profilealloc(_addr_mp, x, size);
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
                        gcStart(t);
                    }

                }

            }

            return x;

        }

        private static ptr<mspan> largeAlloc(System.UIntPtr size, bool needzero, bool noscan)
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

            var spc = makeSpanClass(0L, noscan);
            var s = mheap_.alloc(npages, spc, needzero);
            if (s == null)
            {
                throw("out of memory");
            }

            if (go115NewMCentralImpl)
            { 
                // Put the large span in the mcentral swept list so that it's
                // visible to the background sweeper.
                mheap_.central[spc].mcentral.fullSwept(mheap_.sweepgen).push(s);

            }

            s.limit = s.@base() + size;
            heapBitsForAddr(s.@base()).initSpan(s);
            return _addr_s!;

        }

        // implementation of new builtin
        // compiler (both frontend and SSA backend) knows the signature
        // of this function
        private static unsafe.Pointer newobject(ptr<_type> _addr_typ)
        {
            ref _type typ = ref _addr_typ.val;

            return mallocgc(typ.size, _addr_typ, true);
        }

        //go:linkname reflect_unsafe_New reflect.unsafe_New
        private static unsafe.Pointer reflect_unsafe_New(ptr<_type> _addr_typ)
        {
            ref _type typ = ref _addr_typ.val;

            return mallocgc(typ.size, _addr_typ, true);
        }

        //go:linkname reflectlite_unsafe_New internal/reflectlite.unsafe_New
        private static unsafe.Pointer reflectlite_unsafe_New(ptr<_type> _addr_typ)
        {
            ref _type typ = ref _addr_typ.val;

            return mallocgc(typ.size, _addr_typ, true);
        }

        // newarray allocates an array of n elements of type typ.
        private static unsafe.Pointer newarray(ptr<_type> _addr_typ, long n) => func((_, panic, __) =>
        {
            ref _type typ = ref _addr_typ.val;

            if (n == 1L)
            {
                return mallocgc(typ.size, _addr_typ, true);
            }

            var (mem, overflow) = math.MulUintptr(typ.size, uintptr(n));
            if (overflow || mem > maxAlloc || n < 0L)
            {
                panic(plainError("runtime: allocation size out of range"));
            }

            return mallocgc(mem, _addr_typ, true);

        });

        //go:linkname reflect_unsafe_NewArray reflect.unsafe_NewArray
        private static unsafe.Pointer reflect_unsafe_NewArray(ptr<_type> _addr_typ, long n)
        {
            ref _type typ = ref _addr_typ.val;

            return newarray(_addr_typ, n);
        }

        private static void profilealloc(ptr<m> _addr_mp, unsafe.Pointer x, System.UIntPtr size)
        {
            ref m mp = ref _addr_mp.val;

            ptr<mcache> c;
            if (mp.p != 0L)
            {
                c = mp.p.ptr().mcache;
            }
            else
            {
                c = mcache0;
                if (c == null)
                {
                    throw("profilealloc called with no P");
                }

            }

            c.next_sample = nextSample();
            mProf_Malloc(x, size);

        }

        // nextSample returns the next sampling point for heap profiling. The goal is
        // to sample allocations on average every MemProfileRate bytes, but with a
        // completely random distribution over the allocation timeline; this
        // corresponds to a Poisson process with parameter MemProfileRate. In Poisson
        // processes, the distance between two samples follows the exponential
        // distribution (exp(MemProfileRate)), so the best return value is a random
        // number taken from an exponential distribution whose mean is MemProfileRate.
        private static System.UIntPtr nextSample()
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

            return uintptr(fastexprand(MemProfileRate));

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
            const long randomBitCount = (long)26L;

            var q = fastrand() % (1L << (int)(randomBitCount)) + 1L;
            var qlog = fastlog2(float64(q)) - randomBitCount;
            if (qlog > 0L)
            {
                qlog = 0L;
            }

            const float minusLog2 = (float)-0.6931471805599453F; // -ln(2)
 // -ln(2)
            return int32(qlog * (minusLog2 * float64(mean))) + 1L;

        }

        // nextSampleNoFP is similar to nextSample, but uses older,
        // simpler code to avoid floating point.
        private static System.UIntPtr nextSampleNoFP()
        { 
            // Set first allocation sample size.
            var rate = MemProfileRate;
            if (rate > 0x3fffffffUL)
            { // make 2*rate not overflow
                rate = 0x3fffffffUL;

            }

            if (rate != 0L)
            {
                return uintptr(fastrand() % uint32(2L * rate));
            }

            return 0L;

        }

        private partial struct persistentAlloc
        {
            public ptr<notInHeap> @base;
            public System.UIntPtr off;
        }

        private static var globalAlloc = default;

        // persistentChunkSize is the number of bytes we allocate when we grow
        // a persistentAlloc.
        private static readonly long persistentChunkSize = (long)256L << (int)(10L);

        // persistentChunks is a list of all the persistent chunks we have
        // allocated. The list is maintained through the first word in the
        // persistent chunk. This is updated atomically.


        // persistentChunks is a list of all the persistent chunks we have
        // allocated. The list is maintained through the first word in the
        // persistent chunk. This is updated atomically.
        private static ptr<notInHeap> persistentChunks;

        // Wrapper around sysAlloc that can allocate small chunks.
        // There is no associated free operation.
        // Intended for things like function/type/debug-related persistent data.
        // If align is 0, uses default align (currently 8).
        // The returned memory will be zeroed.
        //
        // Consider marking persistentalloc'd types go:notinheap.
        private static unsafe.Pointer persistentalloc(System.UIntPtr size, System.UIntPtr align, ptr<ulong> _addr_sysStat)
        {
            ref ulong sysStat = ref _addr_sysStat.val;

            ptr<notInHeap> p;
            systemstack(() =>
            {
                p = persistentalloc1(size, align, _addr_sysStat);
            });
            return @unsafe.Pointer(p);

        }

        // Must run on system stack because stack growth can (re)invoke it.
        // See issue 9174.
        //go:systemstack
        private static ptr<notInHeap> persistentalloc1(System.UIntPtr size, System.UIntPtr align, ptr<ulong> _addr_sysStat)
        {
            ref ulong sysStat = ref _addr_sysStat.val;

            const long maxBlock = (long)64L << (int)(10L); // VM reservation granularity is 64K on windows

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
                return _addr_(notInHeap.val)(sysAlloc(size, sysStat))!;
            }

            var mp = acquirem();
            ptr<persistentAlloc> persistent;
            if (mp != null && mp.p != 0L)
            {
                persistent = _addr_mp.p.ptr().palloc;
            }
            else
            {
                lock(_addr_globalAlloc.mutex);
                persistent = _addr_globalAlloc.persistentAlloc;
            }

            persistent.off = alignUp(persistent.off, align);
            if (persistent.off + size > persistentChunkSize || persistent.@base == null)
            {
                persistent.@base = (notInHeap.val)(sysAlloc(persistentChunkSize, _addr_memstats.other_sys));
                if (persistent.@base == null)
                {
                    if (persistent == _addr_globalAlloc.persistentAlloc)
                    {
                        unlock(_addr_globalAlloc.mutex);
                    }

                    throw("runtime: cannot allocate memory");

                } 

                // Add the new chunk to the persistentChunks list.
                while (true)
                {
                    var chunks = uintptr(@unsafe.Pointer(persistentChunks)) * (uintptr.val)(@unsafe.Pointer(persistent.@base));

                    chunks;
                    if (atomic.Casuintptr((uintptr.val)(@unsafe.Pointer(_addr_persistentChunks)), chunks, uintptr(@unsafe.Pointer(persistent.@base))))
                    {
                        break;
                    }

                }

                persistent.off = alignUp(sys.PtrSize, align);

            }

            var p = persistent.@base.add(persistent.off);
            persistent.off += size;
            releasem(mp);
            if (persistent == _addr_globalAlloc.persistentAlloc)
            {
                unlock(_addr_globalAlloc.mutex);
            }

            if (sysStat != _addr_memstats.other_sys)
            {
                mSysStatInc(sysStat, size);
                mSysStatDec(_addr_memstats.other_sys, size);
            }

            return _addr_p!;

        }

        // inPersistentAlloc reports whether p points to memory allocated by
        // persistentalloc. This must be nosplit because it is called by the
        // cgo checker code, which is called by the write barrier code.
        //go:nosplit
        private static bool inPersistentAlloc(System.UIntPtr p)
        {
            var chunk = atomic.Loaduintptr((uintptr.val)(@unsafe.Pointer(_addr_persistentChunks)));
            while (chunk != 0L)
            {
                if (p >= chunk && p < chunk + persistentChunkSize)
                {
                    return true;
                }

                chunk = new ptr<ptr<ptr<System.UIntPtr>>>(@unsafe.Pointer(chunk));

            }

            return false;

        }

        // linearAlloc is a simple linear allocator that pre-reserves a region
        // of memory and then maps that region into the Ready state as needed. The
        // caller is responsible for locking.
        private partial struct linearAlloc
        {
            public System.UIntPtr next; // next free byte
            public System.UIntPtr mapped; // one byte past end of mapped space
            public System.UIntPtr end; // end of reserved space
        }

        private static void init(this ptr<linearAlloc> _addr_l, System.UIntPtr @base, System.UIntPtr size)
        {
            ref linearAlloc l = ref _addr_l.val;

            if (base + size < base)
            { 
                // Chop off the last byte. The runtime isn't prepared
                // to deal with situations where the bounds could overflow.
                // Leave that memory reserved, though, so we don't map it
                // later.
                size -= 1L;

            }

            l.next = base;
            l.mapped = base;
            l.end = base + size;

        }

        private static unsafe.Pointer alloc(this ptr<linearAlloc> _addr_l, System.UIntPtr size, System.UIntPtr align, ptr<ulong> _addr_sysStat)
        {
            ref linearAlloc l = ref _addr_l.val;
            ref ulong sysStat = ref _addr_sysStat.val;

            var p = alignUp(l.next, align);
            if (p + size > l.end)
            {
                return null;
            }

            l.next = p + size;
            {
                var pEnd = alignUp(l.next - 1L, physPageSize);

                if (pEnd > l.mapped)
                { 
                    // Transition from Reserved to Prepared to Ready.
                    sysMap(@unsafe.Pointer(l.mapped), pEnd - l.mapped, sysStat);
                    sysUsed(@unsafe.Pointer(l.mapped), pEnd - l.mapped);
                    l.mapped = pEnd;

                }

            }

            return @unsafe.Pointer(p);

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

        private static ptr<notInHeap> add(this ptr<notInHeap> _addr_p, System.UIntPtr bytes)
        {
            ref notInHeap p = ref _addr_p.val;

            return _addr_(notInHeap.val)(@unsafe.Pointer(uintptr(@unsafe.Pointer(p)) + bytes))!;
        }
    }
}
