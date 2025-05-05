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
//	fixalloc: a free-list allocator for fixed-size off-heap objects,
//		used to manage storage used by the allocator.
//	mheap: the malloc heap, managed at page (8192-byte) granularity.
//	mspan: a run of in-use pages managed by the mheap.
//	mcentral: collects all spans of a given size class.
//	mcache: a per-P cache of mspans with free space.
//	mstats: allocation statistics.
//
// Allocating a small object proceeds up a hierarchy of caches:
//
//	1. Round the size up to one of the small size classes
//	   and look in the corresponding mspan in this P's mcache.
//	   Scan the mspan's free bitmap to find a free slot.
//	   If there is a free slot, allocate it.
//	   This can all be done without acquiring a lock.
//
//	2. If the mspan has no free slots, obtain a new mspan
//	   from the mcentral's list of mspans of the required size
//	   class that have free space.
//	   Obtaining a whole span amortizes the cost of locking
//	   the mcentral.
//
//	3. If the mcentral's mspan list is empty, obtain a run
//	   of pages from the mheap to use for the mspan.
//
//	4. If the mheap is empty or has no page runs large enough,
//	   allocate a new group of pages (at least 1MB) from the
//	   operating system. Allocating a large run of pages
//	   amortizes the cost of talking to the operating system.
//
// Sweeping an mspan and freeing objects on it proceeds up a similar
// hierarchy:
//
//	1. If the mspan is being swept in response to allocation, it
//	   is returned to the mcache to satisfy the allocation.
//
//	2. Otherwise, if the mspan still has allocated objects in it,
//	   it is placed on the mcentral free list for the mspan's size
//	   class.
//
//	3. Otherwise, if all objects in the mspan are free, the mspan's
//	   pages are returned to the mheap and the mspan is now dead.
//
// Allocating and freeing a large object uses the mheap
// directly, bypassing the mcache and mcentral.
//
// If mspan.needzero is false, then free object slots in the mspan are
// already zeroed. Otherwise if needzero is true, objects are zeroed as
// they are allocated. There are various benefits to delaying zeroing
// this way:
//
//	1. Stack frame allocation can avoid zeroing altogether.
//
//	2. It exhibits better temporal locality, since the program is
//	   probably about to write to the memory.
//
//	3. We don't zero pages that never get reused.
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
namespace go;

using goarch = @internal.goarch_package;
using goos = @internal.goos_package;
using atomic = @internal.runtime.atomic_package;
using math = runtime.@internal.math_package;
using sys = runtime.@internal.sys_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.runtime;
using runtime.@internal;

partial class runtime_package {

internal static readonly UntypedInt maxTinySize = /* _TinySize */ 16;
internal const int8 tinySizeClass = /* _TinySizeClass */ 2;
internal static readonly UntypedInt maxSmallSize = /* _MaxSmallSize */ 32768;
internal static readonly UntypedInt pageShift = /* _PageShift */ 13;
internal static readonly UntypedInt pageSize = /* _PageSize */ 8192;
internal static readonly UntypedInt _PageSize = /* 1 << _PageShift */ 8192;
internal static readonly UntypedInt _PageMask = /* _PageSize - 1 */ 8191;
internal static readonly UntypedInt _64bit = /* 1 << (^uintptr(0) >> 63) / 2 */ 1;
internal static readonly UntypedInt _TinySize = 16;
internal const int8 _TinySizeClass = /* int8(2) */ 2;
internal static readonly UntypedInt _FixAllocChunk = /* 16 << 10 */ 16384; // Chunk size for FixAlloc
internal static readonly UntypedInt _StackCacheSize = /* 32 * 1024 */ 32768;
internal static readonly UntypedInt _NumStackOrders = /* 4 - goarch.PtrSize/4*goos.IsWindows - 1*goos.IsPlan9 */ 2;
internal static readonly UntypedInt heapAddrBits = /* (_64bit*(1-goarch.IsWasm)*(1-goos.IsIos*goarch.IsArm64))*48 + (1-_64bit+goarch.IsWasm)*(32-(goarch.IsMips+goarch.IsMipsle)) + 40*goos.IsIos*goarch.IsArm64 */ 48;
internal static readonly UntypedInt maxAlloc = /* (1 << heapAddrBits) - (1-_64bit)*1 */ 281474976710656;
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
//      ios/arm64         33         4MB           1  2048  (8KB)
//       */32-bit         32         4MB           1  1024  (4KB)
//     */mips(le)         31         4MB           1   512  (2KB)
internal static readonly UntypedInt heapArenaBytes = /* 1 << logHeapArenaBytes */ 4194304;
internal static readonly UntypedInt heapArenaWords = /* heapArenaBytes / goarch.PtrSize */ 524288;
internal static readonly UntypedInt logHeapArenaBytes = /* (6+20)*(_64bit*(1-goos.IsWindows)*(1-goarch.IsWasm)*(1-goos.IsIos*goarch.IsArm64)) + (2+20)*(_64bit*goos.IsWindows) + (2+20)*(1-_64bit) + (2+20)*goarch.IsWasm + (2+20)*goos.IsIos*goarch.IsArm64 */ 22;
internal static readonly UntypedInt heapArenaBitmapWords = /* heapArenaWords / (8 * goarch.PtrSize) */ 8192;
internal static readonly UntypedInt pagesPerArena = /* heapArenaBytes / pageSize */ 512;
internal static readonly UntypedInt arenaL1Bits = /* 6 * (_64bit * goos.IsWindows) */ 6;
internal static readonly UntypedInt arenaL2Bits = /* heapAddrBits - logHeapArenaBytes - arenaL1Bits */ 20;
internal static readonly UntypedInt arenaL1Shift = /* arenaL2Bits */ 20;
internal static readonly UntypedInt arenaBits = /* arenaL1Bits + arenaL2Bits */ 26;
internal static readonly GoUntyped arenaBaseOffset = /* 0xffff800000000000*goarch.IsAmd64 + 0x0a00000000000000*goos.IsAix */
    GoUntyped.Parse("18446603336221196288");
internal static readonly GoUntyped arenaBaseOffsetUintptr = /* uintptr(arenaBaseOffset) */
    GoUntyped.Parse("18446603336221196288");
internal static readonly UntypedInt _MaxGcproc = 32;
internal const uintptr minLegalPointer = 4096;
internal static readonly UntypedInt minHeapForMetadataHugePages = /* 1 << 30 */ 1073741824;

// physPageSize is the size in bytes of the OS's physical pages.
// Mapping and unmapping operations must be done at multiples of
// physPageSize.
//
// This must be set by the OS init code (typically in osinit) before
// mallocinit.
internal static uintptr physPageSize;

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
internal static uintptr physHugePageSize;

internal static nuint physHugePageShift;

internal static void mallocinit() {
    if (class_to_size[_TinySizeClass] != _TinySize) {
        @throw("bad TinySizeClass"u8);
    }
    if ((UntypedInt)(heapArenaBitmapWords & (heapArenaBitmapWords - 1)) != 0) {
        // heapBits expects modular arithmetic on bitmap
        // addresses to work.
        @throw("heapArenaBitmapWords not a power of 2"u8);
    }
    // Check physPageSize.
    if (physPageSize == 0) {
        // The OS init code failed to fetch the physical page size.
        @throw("failed to get system page size"u8);
    }
    if (physPageSize > maxPhysPageSize) {
        print("system page size (", physPageSize, ") is larger than maximum page size (", maxPhysPageSize, ")\n");
        @throw("bad system page size"u8);
    }
    if (physPageSize < minPhysPageSize) {
        print("system page size (", physPageSize, ") is smaller than minimum page size (", minPhysPageSize, ")\n");
        @throw("bad system page size"u8);
    }
    if ((uintptr)(physPageSize & (physPageSize - 1)) != 0) {
        print("system page size (", physPageSize, ") must be a power of 2\n");
        @throw("bad system page size"u8);
    }
    if ((uintptr)(physHugePageSize & (physHugePageSize - 1)) != 0) {
        print("system huge page size (", physHugePageSize, ") must be a power of 2\n");
        @throw("bad system huge page size"u8);
    }
    if (physHugePageSize > maxPhysHugePageSize) {
        // physHugePageSize is greater than the maximum supported huge page size.
        // Don't throw here, like in the other cases, since a system configured
        // in this way isn't wrong, we just don't have the code to support them.
        // Instead, silently set the huge page size to zero.
        physHugePageSize = 0;
    }
    if (physHugePageSize != 0) {
        // Since physHugePageSize is a power of 2, it suffices to increase
        // physHugePageShift until 1<<physHugePageShift == physHugePageSize.
        while (1 << (int)(physHugePageShift) != physHugePageSize) {
            physHugePageShift++;
        }
    }
    if (pagesPerArena % pagesPerSpanRoot != 0) {
        print("pagesPerArena (", pagesPerArena, ") is not divisible by pagesPerSpanRoot (", pagesPerSpanRoot, ")\n");
        @throw("bad pagesPerSpanRoot"u8);
    }
    if (pagesPerArena % pagesPerReclaimerChunk != 0) {
        print("pagesPerArena (", pagesPerArena, ") is not divisible by pagesPerReclaimerChunk (", pagesPerReclaimerChunk, ")\n");
        @throw("bad pagesPerReclaimerChunk"u8);
    }
    // Check that the minimum size (exclusive) for a malloc header is also
    // a size class boundary. This is important to making sure checks align
    // across different parts of the runtime.
    var minSizeForMallocHeaderIsSizeClass = false;
    for (nint i = 0; i < len(class_to_size); i++) {
        if (minSizeForMallocHeader == ((uintptr)class_to_size[i])) {
            minSizeForMallocHeaderIsSizeClass = true;
            break;
        }
    }
    if (!minSizeForMallocHeaderIsSizeClass) {
        @throw("min size of malloc header is not a size class boundary"u8);
    }
    // Check that the pointer bitmap for all small sizes without a malloc header
    // fits in a word.
    if (minSizeForMallocHeader / goarch.PtrSize > 8 * goarch.PtrSize) {
        @throw("max pointer/scan bitmap size for headerless objects is too large"u8);
    }
    if (minTagBits > taggedPointerBits) {
        @throw("taggedPointerbits too small"u8);
    }
    // Initialize the heap.
    mheap_.init();
    mcache0 = allocmcache();
    lockInit(ᏑgcBitsArenas.of(struct{lock mutex; free *runtime.gcBitsArena; next *runtime.gcBitsArena; current *runtime.gcBitsArena; previous *runtime.gcBitsArena}.Ꮡlock), lockRankGcBitsArenas);
    lockInit(Ꮡ(profInsertLock), lockRankProfInsert);
    lockInit(Ꮡ(profBlockLock), lockRankProfBlock);
    lockInit(Ꮡ(profMemActiveLock), lockRankProfMemActive);
    ref var i = ref heap(new nint(), out var Ꮡi);

    foreach (var (i, _) in profMemFutureLock) {
        lockInit(ᏑprofMemFutureLock.at<mutex>(i), lockRankProfMemFuture);
    }
    lockInit(ᏑglobalAlloc.of(struct{mutex; runtime.persistentAlloc}.Ꮡmutex), lockRankGlobalAlloc);
    // Create initial arena growth hints.
    if (goarch.PtrSize == 8){
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
        // On ios/arm64, the address space is even smaller.
        //
        // On AIX, mmaps starts at 0x0A00000000000000 for 64-bit.
        // processes.
        //
        // Space mapped for user arenas comes immediately after the range
        // originally reserved for the regular heap when race mode is not
        // enabled because user arena chunks can never be used for regular heap
        // allocations and we want to avoid fragmenting the address space.
        //
        // In race mode we have no choice but to just use the same hints because
        // the race detector requires that the heap be mapped contiguously.
        for (nint i = 127; i >= 0; i--) {
            uintptr Δp = default!;
            switch (ᐧ) {
            case {} when raceenabled: {
                Δp = (uintptr)(((uintptr)i) << (int)(32) | (uintptr)(uintptrMask & (192 << (int)(32))));
                if (Δp >= (uintptr)(uintptrMask & (nint)962072674304L)) {
                    // The TSAN runtime requires the heap
                    // to be in the range [0x00c000000000,
                    // 0x00e000000000).
                    continue;
                }
                break;
            }
            case {} when GOARCH == "arm64"u8 && GOOS == "ios"u8: {
                Δp = (uintptr)(((uintptr)i) << (int)(40) | (uintptr)(uintptrMask & (19 << (int)(28))));
                break;
            }
            case {} when GOARCH == "arm64"u8: {
                Δp = (uintptr)(((uintptr)i) << (int)(40) | (uintptr)(uintptrMask & (64 << (int)(32))));
                break;
            }
            case {} when GOOS == "aix"u8: {
                if (i == 0) {
                    // We don't use addresses directly after 0x0A00000000000000
                    // to avoid collisions with others mmaps done by non-go programs.
                    continue;
                }
                Δp = (uintptr)(((uintptr)i) << (int)(40) | (uintptr)(uintptrMask & (160 << (int)(52))));
                break;
            }
            default: {
                Δp = (uintptr)(((uintptr)i) << (int)(40) | (uintptr)(uintptrMask & (192 << (int)(32))));
                break;
            }}

            // Switch to generating hints for user arenas if we've gone
            // through about half the hints. In race mode, take only about
            // a quarter; we don't have very much space to work with.
            var hintList = Ꮡmheap_.of(mheap.ᏑarenaHints);
            if ((!raceenabled && i > 63) || (raceenabled && i > 95)) {
                hintList = Ꮡmheap_.userArena.of(struct{arenaHints *arenaHint; quarantineList runtime.mSpanList; readyList runtime.mSpanList}.ᏑarenaHints);
            }
            var hint = (ж<arenaHint>)(uintptr)(mheap_.arenaHintAlloc.alloc());
            hint.val.addr = Δp;
            (hint.val.next, hintList.val) = (hintList.val, hint);
        }
    } else {
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
        const uintptr arenaMetaSize = /* (1 << arenaBits) * unsafe.Sizeof(heapArena{}) */ 288836550656;
        var meta = ((uintptr)(uintptr)sysReserve(nil, arenaMetaSize));
        if (meta != 0) {
            mheap_.heapArenaAlloc.init(meta, arenaMetaSize, true);
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
        var Δp = firstmoduledata.end;
        if (Δp < procBrk) {
            Δp = procBrk;
        }
        if (mheap_.heapArenaAlloc.next <= Δp && Δp < mheap_.heapArenaAlloc.end) {
            Δp = mheap_.heapArenaAlloc.end;
        }
        Δp = alignUp(Δp + (256 << (int)(10)), heapArenaBytes);
        // Because we're worried about fragmentation on
        // 32-bit, we try to make a large initial reservation.
        var arenaSizes = new uintptr[]{
            512 << (int)(20),
            256 << (int)(20),
            128 << (int)(20)
        }.slice();
        foreach (var (_, arenaSize) in arenaSizes) {
            var (a, size) = sysReserveAligned(((@unsafe.Pointer)Δp), arenaSize, heapArenaBytes);
            if (a != nil) {
                mheap_.arena.init(((uintptr)a), size, false);
                Δp = mheap_.arena.end;
                // For hint below
                break;
            }
        }
        var hint = (ж<arenaHint>)(uintptr)(mheap_.arenaHintAlloc.alloc());
        hint.val.addr = Δp;
        (hint.val.next, mheap_.arenaHints) = (mheap_.arenaHints, hint);
        // Place the hint for user arenas just after the large reservation.
        //
        // While this potentially competes with the hint above, in practice we probably
        // aren't going to be getting this far anyway on 32-bit platforms.
        var userArenaHint = (ж<arenaHint>)(uintptr)(mheap_.arenaHintAlloc.alloc());
        userArenaHint.val.addr = Δp;
        (userArenaHint.val.next, mheap_.userArena.arenaHints) = (mheap_.userArena.arenaHints, userArenaHint);
    }
    // Initialize the memory limit here because the allocator is going to look at it
    // but we haven't called gcinit yet and we're definitely going to allocate memory before then.
    gcController.memoryLimit.Store(maxInt64);
}

// sysAlloc allocates heap arena space for at least n bytes. The
// returned pointer is always heapArenaBytes-aligned and backed by
// h.arenas metadata. The returned size is always a multiple of
// heapArenaBytes. sysAlloc returns nil on failure.
// There is no corresponding free function.
//
// hintList is a list of hint addresses for where to allocate new
// heap arenas. It must be non-nil.
//
// register indicates whether the heap arena should be registered
// in allArenas.
//
// sysAlloc returns a memory region in the Reserved state. This region must
// be transitioned to Prepared and then Ready before use.
//
// h must be locked.
[GoRecv] internal static (@unsafe.Pointer v, uintptr size) sysAlloc(this ref mheap h, uintptr n, ж<ж<arenaHint>> ᏑhintList, bool register) {
    @unsafe.Pointer v = default!;
    uintptr size = default!;

    ref var hintList = ref ᏑhintList.val;
    assertLockHeld(Ꮡ(h.@lock));
    n = alignUp(n, heapArenaBytes);
    if (ᏑhintList == Ꮡ(h.arenaHints)) {
        // First, try the arena pre-reservation.
        // Newly-used mappings are considered released.
        //
        // Only do this if we're using the regular heap arena hints.
        // This behavior is only for the heap.
        v = (uintptr)h.arena.alloc(n, heapArenaBytes, ᏑgcController.of(gcControllerState.ᏑheapReleased));
        if (v != nil) {
            size = n;
            goto mapped;
        }
    }
    // Try to grow the heap at a hint address.
    while (hintList != nil) {
        var hint = hintList;
        var Δp = hint.val.addr;
        if ((~hint).down) {
            Δp -= n;
        }
        if (Δp + n < Δp){
            // We can't use this, so don't ask.
            v = default!;
        } else 
        if (arenaIndex(Δp + n - 1) >= 1 << (int)(arenaBits)){
            // Outside addressable heap. Can't use.
            v = default!;
        } else {
            v = (uintptr)sysReserve(((@unsafe.Pointer)Δp), n);
        }
        if (Δp == ((uintptr)v)) {
            // Success. Update the hint.
            if (!(~hint).down) {
                Δp += n;
            }
            hint.val.addr = Δp;
            size = n;
            break;
        }
        // Failed. Discard this hint and try the next.
        //
        // TODO: This would be cleaner if sysReserve could be
        // told to only return the requested address. In
        // particular, this is already how Windows behaves, so
        // it would simplify things there.
        if (v != nil) {
            sysFreeOS(v, n);
        }
        hintList = hint.val.next;
        h.arenaHintAlloc.free(new @unsafe.Pointer(hint));
    }
    if (size == 0) {
        if (raceenabled) {
            // The race detector assumes the heap lives in
            // [0x00c000000000, 0x00e000000000), but we
            // just ran out of hints in this region. Give
            // a nice failure.
            @throw("too many address space collisions for -race mode"u8);
        }
        // All of the hints failed, so we'll take any
        // (sufficiently aligned) address the kernel will give
        // us.
        (v, size) = sysReserveAligned(nil, n, heapArenaBytes);
        if (v == nil) {
            return (default!, 0);
        }
        // Create new hints for extending this region.
        var hint = (ж<arenaHint>)(uintptr)(h.arenaHintAlloc.alloc());
        (hint.val.addr, hint.val.down) = (((uintptr)v), true);
        (hint.val.next, mheap_.arenaHints) = (mheap_.arenaHints, hint);
        hint = (ж<arenaHint>)(uintptr)(h.arenaHintAlloc.alloc());
        hint.val.addr = ((uintptr)v) + size;
        (hint.val.next, mheap_.arenaHints) = (mheap_.arenaHints, hint);
    }
    // Check for bad pointers or pointers we can't use.
    {
        @string bad = default!;
        var Δp = ((uintptr)v);
        if (Δp + size < Δp){
            bad = "region exceeds uintptr range"u8;
        } else 
        if (arenaIndex(Δp) >= 1 << (int)(arenaBits)){
            bad = "base outside usable address space"u8;
        } else 
        if (arenaIndex(Δp + size - 1) >= 1 << (int)(arenaBits)) {
            bad = "end outside usable address space"u8;
        }
        if (bad != ""u8) {
            // This should be impossible on most architectures,
            // but it would be really confusing to debug.
            print("runtime: memory allocated by OS [", ((Δhex)Δp), ", ", ((Δhex)(Δp + size)), ") not in usable address space: ", bad, "\n");
            @throw("memory reservation exceeds address space limit"u8);
        }
    }
    if ((uintptr)(((uintptr)v) & (heapArenaBytes - 1)) != 0) {
        @throw("misrounded allocation in sysAlloc"u8);
    }
mapped:
    for (arenaIdx ri = arenaIndex(((uintptr)v)); ri <= arenaIndex(((uintptr)v) + size - 1); ri++) {
        // Create arena metadata.
        var l2 = h.arenas[ri.l1()];
        if (l2 == nil) {
            // Allocate an L2 arena map.
            //
            // Use sysAllocOS instead of sysAlloc or persistentalloc because there's no
            // statistic we can comfortably account for this space in. With this structure,
            // we rely on demand paging to avoid large overheads, but tracking which memory
            // is paged in is too expensive. Trying to account for the whole region means
            // that it will appear like an enormous memory overhead in statistics, even though
            // it is not.
            l2 = (ж<array<ж<heapArena>>>)(uintptr)(sysAllocOS(@unsafe.Sizeof(l2.val)));
            if (l2 == nil) {
                @throw("out of memory allocating heap arena map"u8);
            }
            if (h.arenasHugePages){
                sysHugePage(new @unsafe.Pointer(l2), @unsafe.Sizeof(l2.val));
            } else {
                sysNoHugePage(new @unsafe.Pointer(l2), @unsafe.Sizeof(l2.val));
            }
            atomic.StorepNoWB(((@unsafe.Pointer)(Ꮡ(h.arenas[ri.l1()]))), new @unsafe.Pointer(l2));
        }
        if (l2[ri.l2()] != nil) {
            @throw("arena already initialized"u8);
        }
        ж<heapArena> r = default!;
        r = (ж<heapArena>)(uintptr)(h.heapArenaAlloc.alloc(@unsafe.Sizeof(r.val), goarch.PtrSize, Ꮡmemstats.of(mstats.ᏑgcMiscSys)));
        if (r == nil) {
            r = (ж<heapArena>)(uintptr)(persistentalloc(@unsafe.Sizeof(r.val), goarch.PtrSize, Ꮡmemstats.of(mstats.ᏑgcMiscSys)));
            if (r == nil) {
                @throw("out of memory allocating heap arena metadata"u8);
            }
        }
        // Register the arena in allArenas if requested.
        if (register) {
            if (len(h.allArenas) == cap(h.allArenas)) {
                var sizeΔ1 = 2 * ((uintptr)cap(h.allArenas)) * goarch.PtrSize;
                if (sizeΔ1 == 0) {
                    sizeΔ1 = physPageSize;
                }
                var newArray = (ж<notInHeap>)(uintptr)(persistentalloc(sizeΔ1, goarch.PtrSize, Ꮡmemstats.of(mstats.ᏑgcMiscSys)));
                if (newArray == nil) {
                    @throw("out of memory allocating allArenas"u8);
                }
                var oldSlice = h.allArenas;
                ((ж<notInHeapSlice>)(uintptr)(new @unsafe.Pointer(Ꮡ(h.allArenas)))).val = new notInHeapSlice(newArray, len(h.allArenas), ((nint)(sizeΔ1 / goarch.PtrSize)));
                copy(h.allArenas, oldSlice);
            }
            // Do not free the old backing array because
            // there may be concurrent readers. Since we
            // double the array each time, this can lead
            // to at most 2x waste.
            h.allArenas = h.allArenas[..(int)(len(h.allArenas) + 1)];
            h.allArenas[len(h.allArenas) - 1] = ri;
        }
        // Store atomically just in case an object from the
        // new heap arena becomes visible before the heap lock
        // is released (which shouldn't happen, but there's
        // little downside to this).
        atomic.StorepNoWB(((@unsafe.Pointer)(Ꮡ(l2[ri.l2()]))), new @unsafe.Pointer(r));
continue_mapped:;
    }
break_mapped:;
    // Tell the race detector about the new heap memory.
    if (raceenabled) {
        racemapshadow(v, size);
    }
    return (v, size);
}

// sysReserveAligned is like sysReserve, but the returned pointer is
// aligned to align bytes. It may reserve either n or n+align bytes,
// so it returns the size that was reserved.
internal static (@unsafe.Pointer, uintptr) sysReserveAligned(@unsafe.Pointer v, uintptr size, uintptr align) {
    // Since the alignment is rather large in uses of this
    // function, we're not likely to get it by chance, so we ask
    // for a larger region and remove the parts we don't need.
    nint retries = 0;
retry:
    var Δp = ((uintptr)(uintptr)sysReserve(v.val, size + align));
    switch (ᐧ) {
    case {} when Δp is 0: {
        return (default!, 0);
    }
    case {} when (uintptr)(Δp & (align - 1)) == 0: {
        return (((@unsafe.Pointer)Δp), size + align);
    }
    case {} when GOOS == "windows"u8: {
        sysFreeOS(((@unsafe.Pointer)Δp), // On Windows we can't release pieces of a
 // reservation, so we release the whole thing and
 // re-reserve the aligned sub-region. This may race,
 // so we may have to try again.
 size + align);
        Δp = alignUp(Δp, align);
        @unsafe.Pointer p2 = (uintptr)sysReserve(((@unsafe.Pointer)Δp), size);
        if (Δp != ((uintptr)p2)) {
            // Must have raced. Try again.
            sysFreeOS(p2, size);
            {
                retries++; if (retries == 100) {
                    @throw("failed to allocate aligned heap memory; too many retries"u8);
                }
            }
            goto retry;
        }
        return (p2, size);
    }
    default: {
        var pAligned = alignUp(Δp, // Success.
 // Trim off the unaligned parts.
 align);
        sysFreeOS(((@unsafe.Pointer)Δp), pAligned - Δp);
        var end = pAligned + size;
        var endLen = (Δp + size + align) - end;
        if (endLen > 0) {
            sysFreeOS(((@unsafe.Pointer)end), endLen);
        }
        return (((@unsafe.Pointer)pAligned), size);
    }}

}

// enableMetadataHugePages enables huge pages for various sources of heap metadata.
//
// A note on latency: for sufficiently small heaps (<10s of GiB) this function will take constant
// time, but may take time proportional to the size of the mapped heap beyond that.
//
// This function is idempotent.
//
// The heap lock must not be held over this operation, since it will briefly acquire
// the heap lock.
//
// Must be called on the system stack because it acquires the heap lock.
//
//go:systemstack
[GoRecv] internal static void enableMetadataHugePages(this ref mheap h) {
    // Enable huge pages for page structure.
    h.pages.enableChunkHugePages();
    // Grab the lock and set arenasHugePages if it's not.
    //
    // Once arenasHugePages is set, all new L2 entries will be eligible for
    // huge pages. We'll set all the old entries after we release the lock.
    @lock(Ꮡ(h.@lock));
    if (h.arenasHugePages) {
        unlock(Ꮡ(h.@lock));
        return;
    }
    h.arenasHugePages = true;
    unlock(Ꮡ(h.@lock));
    // N.B. The arenas L1 map is quite small on all platforms, so it's fine to
    // just iterate over the whole thing.
    foreach (var (i, _) in h.arenas) {
        var l2 = (ж<array<ж<heapArena>>>)(uintptr)(atomic.Loadp(((@unsafe.Pointer)(Ꮡ(h.arenas[i])))));
        if (l2 == nil) {
            continue;
        }
        sysHugePage(new @unsafe.Pointer(l2), @unsafe.Sizeof(l2.val));
    }
}

// base address for all 0-byte allocations
internal static uintptr zerobase;

// nextFreeFast returns the next free object if one is quickly available.
// Otherwise it returns 0.
internal static gclinkptr nextFreeFast(ж<mspan> Ꮡs) {
    ref var s = ref Ꮡs.val;

    nint theBit = sys.TrailingZeros64(s.allocCache);
    // Is there a free object in the allocCache?
    if (theBit < 64) {
        var result = s.freeindex + ((uint16)theBit);
        if (result < s.nelems) {
            var freeidx = result + 1;
            if (freeidx % 64 == 0 && freeidx != s.nelems) {
                return 0;
            }
            s.allocCache >>= (nuint)(((nuint)(theBit + 1)));
            s.freeindex = freeidx;
            s.allocCount++;
            return ((gclinkptr)(((uintptr)result) * s.elemsize + s.@base()));
        }
    }
    return 0;
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
[GoRecv] internal static (gclinkptr v, ж<mspan> s, bool shouldhelpgc) nextFree(this ref mcache c, spanClass spc) {
    gclinkptr v = default!;
    ж<mspan> s = default!;
    bool shouldhelpgc = default!;

    s = c.alloc[spc];
    shouldhelpgc = false;
    var freeIndex = s.nextFreeIndex();
    if (freeIndex == (~s).nelems) {
        // The span is full.
        if ((~s).allocCount != (~s).nelems) {
            println("runtime: s.allocCount=", (~s).allocCount, "s.nelems=", (~s).nelems);
            @throw("s.allocCount != s.nelems && freeIndex == s.nelems"u8);
        }
        c.refill(spc);
        shouldhelpgc = true;
        s = c.alloc[spc];
        freeIndex = s.nextFreeIndex();
    }
    if (freeIndex >= (~s).nelems) {
        @throw("freeIndex is not valid"u8);
    }
    v = ((gclinkptr)(((uintptr)freeIndex) * (~s).elemsize + s.@base()));
    (~s).allocCount++;
    if ((~s).allocCount > (~s).nelems) {
        println("s.allocCount=", (~s).allocCount, "s.nelems=", (~s).nelems);
        @throw("s.allocCount > s.nelems"u8);
    }
    return (v, s, shouldhelpgc);
}

// Allocate an object of size bytes.
// Small objects are allocated from the per-P cache's free lists.
// Large objects (> 32 kB) are allocated straight from the heap.
//
// mallocgc should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/bytedance/gopkg
//   - github.com/bytedance/sonic
//   - github.com/cloudwego/frugal
//   - github.com/cockroachdb/cockroach
//   - github.com/cockroachdb/pebble
//   - github.com/ugorji/go/codec
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname mallocgc
internal static @unsafe.Pointer mallocgc(uintptr size, ж<_type> Ꮡtyp, bool needzero) {
    ref var typ = ref Ꮡtyp.val;

    if (gcphase == _GCmarktermination) {
        @throw("mallocgc called with gcphase == _GCmarktermination"u8);
    }
    if (size == 0) {
        return ((@unsafe.Pointer)(Ꮡ(zerobase)));
    }
    // It's possible for any malloc to trigger sweeping, which may in
    // turn queue finalizers. Record this dynamic lock edge.
    lockRankMayQueueFinalizer();
    var userSize = size;
    if (asanenabled) {
        // Refer to ASAN runtime library, the malloc() function allocates extra memory,
        // the redzone, around the user requested memory region. And the redzones are marked
        // as unaddressable. We perform the same operations in Go to detect the overflows or
        // underflows.
        size += computeRZlog(size);
    }
    if (debug.malloc) {
        if (debug.sbrk != 0) {
            var align = ((uintptr)16);
            if (typ != nil) {
                // TODO(austin): This should be just
                //   align = uintptr(typ.align)
                // but that's only 4 on 32-bit platforms,
                // even if there's a uint64 field in typ (see #599).
                // This causes 64-bit atomic accesses to panic.
                // Hence, we use stricter alignment that matches
                // the normal allocator better.
                if ((uintptr)(size & 7) == 0){
                    align = 8;
                } else 
                if ((uintptr)(size & 3) == 0){
                    align = 4;
                } else 
                if ((uintptr)(size & 1) == 0){
                    align = 2;
                } else {
                    align = 1;
                }
            }
            return (uintptr)persistentalloc(size, align, Ꮡmemstats.of(mstats.Ꮡother_sys));
        }
        if (inittrace.active && inittrace.id == (~getg()).goid) {
            // Init functions are executed sequentially in a single goroutine.
            inittrace.allocs += 1;
        }
    }
    // assistG is the G to charge for this allocation, or nil if
    // GC is not currently active.
    var assistG = deductAssistCredit(size);
    // Set mp.mallocing to keep from being preempted by GC.
    var mp = acquirem();
    if ((~mp).mallocing != 0) {
        @throw("malloc deadlock"u8);
    }
    if ((~mp).gsignal == getg()) {
        @throw("malloc during signal"u8);
    }
    mp.val.mallocing = 1;
    var shouldhelpgc = false;
    var dataSize = userSize;
    var c = getMCache(mp);
    if (c == nil) {
        @throw("mallocgc called without a P or outside bootstrapping"u8);
    }
    ж<mspan> span = default!;
    ж<ж<_type>> header = default!;
    @unsafe.Pointer x = default!;
    var noscan = typ == nil || !typ.Pointers();
    // In some cases block zeroing can profitably (for latency reduction purposes)
    // be delayed till preemption is possible; delayedZeroing tracks that state.
    var delayedZeroing = false;
    // Determine if it's a 'small' object that goes into a size-classed span.
    //
    // Note: This comparison looks a little strange, but it exists to smooth out
    // the crossover between the largest size class and large objects that have
    // their own spans. The small window of object sizes between maxSmallSize-mallocHeaderSize
    // and maxSmallSize will be considered large, even though they might fit in
    // a size class. In practice this is completely fine, since the largest small
    // size class has a single object in it already, precisely to make the transition
    // to large objects smooth.
    if (size <= maxSmallSize - mallocHeaderSize){
        if (noscan && size < maxTinySize){
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
            var off = c.val.tinyoffset;
            // Align tiny pointer for required (conservative) alignment.
            if ((uintptr)(size & 7) == 0){
                off = alignUp(off, 8);
            } else 
            if (goarch.PtrSize == 4 && size == 12){
                // Conservatively align 12-byte objects to 8 bytes on 32-bit
                // systems so that objects whose first field is a 64-bit
                // value is aligned to 8 bytes and does not cause a fault on
                // atomic access. See issue 37262.
                // TODO(mknyszek): Remove this workaround if/when issue 36606
                // is resolved.
                off = alignUp(off, 8);
            } else 
            if ((uintptr)(size & 3) == 0){
                off = alignUp(off, 4);
            } else 
            if ((uintptr)(size & 1) == 0) {
                off = alignUp(off, 2);
            }
            if (off + size <= maxTinySize && (~c).tiny != 0) {
                // The object fits into existing tiny block.
                x = ((@unsafe.Pointer)((~c).tiny + off));
                c.val.tinyoffset = off + size;
                (~c).tinyAllocs++;
                mp.val.mallocing = 0;
                releasem(mp);
                return x;
            }
            // Allocate a new maxTinySize block.
            span = (~c).alloc[tinySpanClass];
            var v = nextFreeFast(span);
            if (v == 0) {
                (v, span, shouldhelpgc) = c.nextFree(tinySpanClass);
            }
            x = ((@unsafe.Pointer)v);
            (ж<array<uint64>>)(uintptr)(x)[0] = 0;
            (ж<array<uint64>>)(uintptr)(x)[1] = 0;
            // See if we need to replace the existing tiny block with the new one
            // based on amount of remaining free space.
            if (!raceenabled && (size < (~c).tinyoffset || (~c).tiny == 0)) {
                // Note: disabled when race detector is on, see comment near end of this function.
                c.val.tiny = ((uintptr)x);
                c.val.tinyoffset = size;
            }
            size = maxTinySize;
        } else {
            var hasHeader = !noscan && !heapBitsInSpan(size);
            if (hasHeader) {
                size += mallocHeaderSize;
            }
            uint8 sizeclass = default!;
            if (size <= smallSizeMax - 8){
                sizeclass = size_to_class8[divRoundUp(size, smallSizeDiv)];
            } else {
                sizeclass = size_to_class128[divRoundUp(size - smallSizeMax, largeSizeDiv)];
            }
            size = ((uintptr)class_to_size[sizeclass]);
            var spc = makeSpanClass(sizeclass, noscan);
            span = (~c).alloc[spc];
            var v = nextFreeFast(span);
            if (v == 0) {
                (v, span, shouldhelpgc) = c.nextFree(spc);
            }
            x = ((@unsafe.Pointer)v);
            if (needzero && (~span).needzero != 0) {
                memclrNoHeapPointers(x, size);
            }
            if (hasHeader) {
                header = (ж<ж<_type>>)(uintptr)(x);
                x = (uintptr)add(x, mallocHeaderSize);
                size -= mallocHeaderSize;
            }
        }
    } else {
        shouldhelpgc = true;
        // For large allocations, keep track of zeroed state so that
        // bulk zeroing can be happen later in a preemptible context.
        span = c.allocLarge(size, noscan);
        span.val.freeindex = 1;
        span.val.allocCount = 1;
        size = span.val.elemsize;
        x = ((@unsafe.Pointer)span.@base());
        if (needzero && (~span).needzero != 0) {
            delayedZeroing = true;
        }
        if (!noscan) {
            // Tell the GC not to look at this yet.
            span.val.largeType = default!;
            header = Ꮡ((~span).largeType);
        }
    }
    if (!noscan && !delayedZeroing) {
        c.val.scanAlloc += heapSetType(((uintptr)x), dataSize, Ꮡtyp, header, span);
    }
    // Ensure that the stores above that initialize x to
    // type-safe memory and set the heap bits occur before
    // the caller can make x observable to the garbage
    // collector. Otherwise, on weakly ordered machines,
    // the garbage collector could follow a pointer to x,
    // but see uninitialized memory or stale heap bits.
    publicationBarrier();
    // As x and the heap bits are initialized, update
    // freeIndexForScan now so x is seen by the GC
    // (including conservative scan) as an allocated object.
    // While this pointer can't escape into user code as a
    // _live_ pointer until we return, conservative scanning
    // may find a dead pointer that happens to point into this
    // object. Delaying this update until now ensures that
    // conservative scanning considers this pointer dead until
    // this point.
    span.val.freeIndexForScan = span.val.freeindex;
    // Allocate black during GC.
    // All slots hold nil so no scanning is needed.
    // This may be racing with GC so do it atomically if there can be
    // a race marking the bit.
    if (gcphase != _GCoff) {
        gcmarknewobject(span, ((uintptr)x));
    }
    if (raceenabled) {
        racemalloc(x, size);
    }
    if (msanenabled) {
        msanmalloc(x, size);
    }
    if (asanenabled) {
        // We should only read/write the memory with the size asked by the user.
        // The rest of the allocated memory should be poisoned, so that we can report
        // errors when accessing poisoned memory.
        // The allocated memory is larger than required userSize, it will also include
        // redzone and some other padding bytes.
        @unsafe.Pointer rzBeg = (uintptr)@unsafe.Add(x, userSize);
        asanpoison(rzBeg, size - userSize);
        asanunpoison(x, userSize);
    }
    // TODO(mknyszek): We should really count the header as part
    // of gc_sys or something. The code below just pretends it is
    // internal fragmentation and matches the GC's accounting by
    // using the whole allocation slot.
    var fullSize = span.val.elemsize;
    {
        nint rate = MemProfileRate; if (rate > 0) {
            // Note cache c only valid while m acquired; see #47302
            //
            // N.B. Use the full size because that matches how the GC
            // will update the mem profile on the "free" side.
            if (rate != 1 && fullSize < (~c).nextSample){
                c.val.nextSample -= fullSize;
            } else {
                profilealloc(mp, x, fullSize);
            }
        }
    }
    mp.val.mallocing = 0;
    releasem(mp);
    // Objects can be zeroed late in a context where preemption can occur.
    // If the object contains pointers, its pointer data must be cleared
    // or otherwise indicate that the GC shouldn't scan it.
    // x will keep the memory alive.
    if (delayedZeroing) {
        // N.B. size == fullSize always in this case.
        memclrNoHeapPointersChunked(size, x);
        // This is a possible preemption point: see #47302
        // Finish storing the type information for this case.
        if (!noscan) {
            var mpΔ1 = acquirem();
            getMCache(mp).val.scanAlloc += heapSetType(((uintptr)x), dataSize, Ꮡtyp, header, span);
            // Publish the type information with the zeroed memory.
            publicationBarrier();
            releasem(mpΔ1);
        }
    }
    if (debug.malloc) {
        if (inittrace.active && inittrace.id == (~getg()).goid) {
            // Init functions are executed sequentially in a single goroutine.
            inittrace.bytes += ((uint64)fullSize);
        }
        if (traceAllocFreeEnabled()) {
            var Δtrace = traceAcquire();
            if (Δtrace.ok()) {
                Δtrace.HeapObjectAlloc(((uintptr)x), Ꮡtyp);
                traceRelease(Δtrace);
            }
        }
    }
    if (assistG != nil) {
        // Account for internal fragmentation in the assist
        // debt now that we know it.
        //
        // N.B. Use the full size because that's how the rest
        // of the GC accounts for bytes marked.
        assistG.val.gcAssistBytes -= ((int64)(fullSize - dataSize));
    }
    if (shouldhelpgc) {
        {
            var t = (new gcTrigger(kind: gcTriggerHeap)); if (t.test()) {
                gcStart(t);
            }
        }
    }
    if (raceenabled && noscan && dataSize < maxTinySize) {
        // Pad tinysize allocations so they are aligned with the end
        // of the tinyalloc region. This ensures that any arithmetic
        // that goes off the top end of the object will be detectable
        // by checkptr (issue 38872).
        // Note that we disable tinyalloc when raceenabled for this to work.
        // TODO: This padding is only performed when the race detector
        // is enabled. It would be nice to enable it if any package
        // was compiled with checkptr, but there's no easy way to
        // detect that (especially at compile time).
        // TODO: enable this padding for all allocations, not just
        // tinyalloc ones. It's tricky because of pointer maps.
        // Maybe just all noscan objects?
        x = (uintptr)add(x, size - dataSize);
    }
    return x;
}

// deductAssistCredit reduces the current G's assist credit
// by size bytes, and assists the GC if necessary.
//
// Caller must be preemptible.
//
// Returns the G for which the assist credit was accounted.
internal static ж<g> deductAssistCredit(uintptr size) {
    ж<g> assistG = default!;
    if (gcBlackenEnabled != 0) {
        // Charge the current user G for this allocation.
        assistG = getg();
        if ((~(~assistG).m).curg != nil) {
            assistG = (~assistG).m.val.curg;
        }
        // Charge the allocation against the G. We'll account
        // for internal fragmentation at the end of mallocgc.
        assistG.val.gcAssistBytes -= ((int64)size);
        if ((~assistG).gcAssistBytes < 0) {
            // This G is in debt. Assist the GC to correct
            // this before allocating. This must happen
            // before disabling preemption.
            gcAssistAlloc(assistG);
        }
    }
    return assistG;
}

// memclrNoHeapPointersChunked repeatedly calls memclrNoHeapPointers
// on chunks of the buffer to be zeroed, with opportunities for preemption
// along the way.  memclrNoHeapPointers contains no safepoints and also
// cannot be preemptively scheduled, so this provides a still-efficient
// block copy that can also be preempted on a reasonable granularity.
//
// Use this with care; if the data being cleared is tagged to contain
// pointers, this allows the GC to run before it is all cleared.
internal static void memclrNoHeapPointersChunked(uintptr size, @unsafe.Pointer x) {
    var v = ((uintptr)x);
    // got this from benchmarking. 128k is too small, 512k is too large.
    static readonly UntypedInt chunkBytes = /* 256 * 1024 */ 262144;
    var vsize = v + size;
    for (var voff = v; voff < vsize; voff = voff + chunkBytes) {
        if ((~getg()).preempt) {
            // may hold locks, e.g., profiling
            goschedguarded();
        }
        // clear min(avail, lump) bytes
        var n = vsize - voff;
        if (n > chunkBytes) {
            n = chunkBytes;
        }
        memclrNoHeapPointers(((@unsafe.Pointer)voff), n);
    }
}

// implementation of new builtin
// compiler (both frontend and SSA backend) knows the signature
// of this function.
internal static @unsafe.Pointer newobject(ж<_type> Ꮡtyp) {
    ref var typ = ref Ꮡtyp.val;

    return (uintptr)mallocgc(typ.Size_, Ꮡtyp, true);
}

// reflect_unsafe_New is meant for package reflect,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - gitee.com/quant1x/gox
//   - github.com/goccy/json
//   - github.com/modern-go/reflect2
//   - github.com/v2pro/plz
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname reflect_unsafe_New reflect.unsafe_New
internal static @unsafe.Pointer reflect_unsafe_New(ж<_type> Ꮡtyp) {
    ref var typ = ref Ꮡtyp.val;

    return (uintptr)mallocgc(typ.Size_, Ꮡtyp, true);
}

//go:linkname reflectlite_unsafe_New internal/reflectlite.unsafe_New
internal static @unsafe.Pointer reflectlite_unsafe_New(ж<_type> Ꮡtyp) {
    ref var typ = ref Ꮡtyp.val;

    return (uintptr)mallocgc(typ.Size_, Ꮡtyp, true);
}

// newarray allocates an array of n elements of type typ.
//
// newarray should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/RomiChan/protobuf
//   - github.com/segmentio/encoding
//   - github.com/ugorji/go/codec
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname newarray
internal static @unsafe.Pointer newarray(ж<_type> Ꮡtyp, nint n) {
    ref var typ = ref Ꮡtyp.val;

    if (n == 1) {
        return (uintptr)mallocgc(typ.Size_, Ꮡtyp, true);
    }
    var (mem, overflow) = math.MulUintptr(typ.Size_, ((uintptr)n));
    if (overflow || mem > maxAlloc || n < 0) {
        throw panic(((plainError)"runtime: allocation size out of range"u8));
    }
    return (uintptr)mallocgc(mem, Ꮡtyp, true);
}

// reflect_unsafe_NewArray is meant for package reflect,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - gitee.com/quant1x/gox
//   - github.com/bytedance/sonic
//   - github.com/goccy/json
//   - github.com/modern-go/reflect2
//   - github.com/segmentio/encoding
//   - github.com/segmentio/kafka-go
//   - github.com/v2pro/plz
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname reflect_unsafe_NewArray reflect.unsafe_NewArray
internal static @unsafe.Pointer reflect_unsafe_NewArray(ж<_type> Ꮡtyp, nint n) {
    ref var typ = ref Ꮡtyp.val;

    return (uintptr)newarray(Ꮡtyp, n);
}

internal static void profilealloc(ж<m> Ꮡmp, @unsafe.Pointer x, uintptr size) {
    ref var mp = ref Ꮡmp.val;

    var c = getMCache(Ꮡmp);
    if (c == nil) {
        @throw("profilealloc called without a P or outside bootstrapping"u8);
    }
    c.val.nextSample = nextSample();
    mProf_Malloc(Ꮡmp, x.val, size);
}

// nextSample returns the next sampling point for heap profiling. The goal is
// to sample allocations on average every MemProfileRate bytes, but with a
// completely random distribution over the allocation timeline; this
// corresponds to a Poisson process with parameter MemProfileRate. In Poisson
// processes, the distance between two samples follows the exponential
// distribution (exp(MemProfileRate)), so the best return value is a random
// number taken from an exponential distribution whose mean is MemProfileRate.
internal static uintptr nextSample() {
    if (MemProfileRate == 1) {
        // Callers assign our return value to
        // mcache.next_sample, but next_sample is not used
        // when the rate is 1. So avoid the math below and
        // just return something.
        return 0;
    }
    if (GOOS == "plan9"u8) {
        // Plan 9 doesn't support floating point in note handler.
        {
            var gp = getg(); if (gp == (~(~gp).m).gsignal) {
                return nextSampleNoFP();
            }
        }
    }
    return ((uintptr)fastexprand(MemProfileRate));
}

// fastexprand returns a random number from an exponential distribution with
// the specified mean.
internal static int32 fastexprand(nint mean) {
    // Avoid overflow. Maximum possible step is
    // -ln(1/(1<<randomBitCount)) * mean, approximately 20 * mean.
    switch (ᐧ) {
    case {} when mean is > 117440512: {
        mean = 117440512;
        break;
    }
    case {} when mean is 0: {
        return 0;
    }}

    // Take a random sample of the exponential distribution exp(-mean*x).
    // The probability distribution function is mean*exp(-mean*x), so the CDF is
    // p = 1 - exp(-mean*x), so
    // q = 1 - p == exp(-mean*x)
    // log_e(q) = -mean*x
    // -log_e(q)/mean = x
    // x = -log_e(q) * mean
    // x = log_2(q) * (-log_e(2)) * mean    ; Using log_2 for efficiency
    static readonly UntypedInt randomBitCount = 26;
    var q = cheaprandn(1 << (int)(randomBitCount)) + 1;
    var qlog = fastlog2(((float64)q)) - randomBitCount;
    if (qlog > 0) {
        qlog = 0;
    }
    static readonly UntypedFloat minusLog2 = /* -0.6931471805599453 */ -0.693147; // -ln(2)
    return ((int32)(qlog * (minusLog2 * ((float64)mean)))) + 1;
}

// nextSampleNoFP is similar to nextSample, but uses older,
// simpler code to avoid floating point.
internal static uintptr nextSampleNoFP() {
    // Set first allocation sample size.
    nint rate = MemProfileRate;
    if (rate > 1073741823) {
        // make 2*rate not overflow
        rate = 1073741823;
    }
    if (rate != 0) {
        return ((uintptr)cheaprandn(((uint32)(2 * rate))));
    }
    return 0;
}

[GoType] partial struct persistentAlloc {
    internal ж<notInHeap> @base;
    internal uintptr off;
}


[GoType("dyn")] partial struct globalAllocᴛ1 {
    internal partial ref mutex mutex { get; }
    internal partial ref persistentAlloc persistentAlloc { get; }
}
internal static globalAllocᴛ1 globalAlloc;

// persistentChunkSize is the number of bytes we allocate when we grow
// a persistentAlloc.
internal static readonly UntypedInt persistentChunkSize = /* 256 << 10 */ 262144;

// persistentChunks is a list of all the persistent chunks we have
// allocated. The list is maintained through the first word in the
// persistent chunk. This is updated atomically.
internal static ж<notInHeap> persistentChunks;

// Wrapper around sysAlloc that can allocate small chunks.
// There is no associated free operation.
// Intended for things like function/type/debug-related persistent data.
// If align is 0, uses default align (currently 8).
// The returned memory will be zeroed.
// sysStat must be non-nil.
//
// Consider marking persistentalloc'd types not in heap by embedding
// runtime/internal/sys.NotInHeap.
internal static @unsafe.Pointer persistentalloc(uintptr size, uintptr align, ж<sysMemStat> ᏑsysStat) {
    ref var sysStat = ref ᏑsysStat.val;

    ж<notInHeap> Δp = default!;
    systemstack(
    var pʗ2 = Δp;
    () => {
        pʗ2 = persistentalloc1(size, align, ᏑsysStat);
    });
    return new @unsafe.Pointer(Δp);
}

// Must run on system stack because stack growth can (re)invoke it.
// See issue 9174.
//
//go:systemstack
internal static ж<notInHeap> persistentalloc1(uintptr size, uintptr align, ж<sysMemStat> ᏑsysStat) {
    ref var sysStat = ref ᏑsysStat.val;

    static readonly UntypedInt maxBlock = /* 64 << 10 */ 65536; // VM reservation granularity is 64K on windows
    if (size == 0) {
        @throw("persistentalloc: size == 0"u8);
    }
    if (align != 0){
        if ((uintptr)(align & (align - 1)) != 0) {
            @throw("persistentalloc: align is not a power of 2"u8);
        }
        if (align > _PageSize) {
            @throw("persistentalloc: align is too large"u8);
        }
    } else {
        align = 8;
    }
    if (size >= maxBlock) {
        return (ж<notInHeap>)(uintptr)(sysAlloc(size, ᏑsysStat));
    }
    var mp = acquirem();
    ж<persistentAlloc> persistent = default!;
    if (mp != nil && (~mp).p != 0){
        persistent = Ꮡ((~(~mp).p.ptr()).palloc);
    } else {
        @lock(ᏑglobalAlloc.of(globalAllocᴛ1.Ꮡmutex));
        persistent = ᏑglobalAlloc.of(globalAllocᴛ1.ᏑpersistentAlloc);
    }
    persistent.val.off = alignUp((~persistent).off, align);
    if ((~persistent).off + size > persistentChunkSize || (~persistent).@base == nil) {
        persistent.val.@base = (ж<notInHeap>)(uintptr)(sysAlloc(persistentChunkSize, Ꮡmemstats.of(mstats.Ꮡother_sys)));
        if ((~persistent).@base == nil) {
            if (persistent == ᏑglobalAlloc.of(globalAllocᴛ1.ᏑpersistentAlloc)) {
                unlock(ᏑglobalAlloc.of(globalAllocᴛ1.Ꮡmutex));
            }
            @throw("runtime: cannot allocate memory"u8);
        }
        // Add the new chunk to the persistentChunks list.
        while (ᐧ) {
            var chunks = ((uintptr)new @unsafe.Pointer(persistentChunks));
            ((ж<uintptr>)(uintptr)(new @unsafe.Pointer((~persistent).@base))).val = chunks;
            if (atomic.Casuintptr(((ж<uintptr>)((@unsafe.Pointer)(Ꮡ(persistentChunks)))), chunks, ((uintptr)new @unsafe.Pointer((~persistent).@base)))) {
                break;
            }
        }
        persistent.val.off = alignUp(goarch.PtrSize, align);
    }
    var Δp = (~persistent).@base.add((~persistent).off);
    persistent.val.off += size;
    releasem(mp);
    if (persistent == ᏑglobalAlloc.of(globalAllocᴛ1.ᏑpersistentAlloc)) {
        unlock(ᏑglobalAlloc.of(globalAllocᴛ1.Ꮡmutex));
    }
    if (ᏑsysStat != Ꮡmemstats.of(mstats.Ꮡother_sys)) {
        sysStat.add(((int64)size));
        memstats.other_sys.add(-((int64)size));
    }
    return Δp;
}

// inPersistentAlloc reports whether p points to memory allocated by
// persistentalloc. This must be nosplit because it is called by the
// cgo checker code, which is called by the write barrier code.
//
//go:nosplit
internal static bool inPersistentAlloc(uintptr Δp) {
    var chunk = atomic.Loaduintptr(((ж<uintptr>)((@unsafe.Pointer)(Ꮡ(persistentChunks)))));
    while (chunk != 0) {
        if (Δp >= chunk && Δp < chunk + persistentChunkSize) {
            return true;
        }
        chunk = ~(ж<uintptr>)(uintptr)(((@unsafe.Pointer)chunk));
    }
    return false;
}

// linearAlloc is a simple linear allocator that pre-reserves a region
// of memory and then optionally maps that region into the Ready state
// as needed.
//
// The caller is responsible for locking.
[GoType] partial struct linearAlloc {
    internal uintptr next; // next free byte
    internal uintptr mapped; // one byte past end of mapped space
    internal uintptr end; // end of reserved space
    internal bool mapMemory; // transition memory from Reserved to Ready if true
}

[GoRecv] internal static void init(this ref linearAlloc l, uintptr @base, uintptr size, bool mapMemory) {
    if (@base + size < @base) {
        // Chop off the last byte. The runtime isn't prepared
        // to deal with situations where the bounds could overflow.
        // Leave that memory reserved, though, so we don't map it
        // later.
        size -= 1;
    }
    (l.next, l.mapped) = (@base, @base);
    l.end = @base + size;
    l.mapMemory = mapMemory;
}

[GoRecv] internal static @unsafe.Pointer alloc(this ref linearAlloc l, uintptr size, uintptr align, ж<sysMemStat> ᏑsysStat) {
    ref var sysStat = ref ᏑsysStat.val;

    var Δp = alignUp(l.next, align);
    if (Δp + size > l.end) {
        return default!;
    }
    l.next = Δp + size;
    {
        var pEnd = alignUp(l.next - 1, physPageSize); if (pEnd > l.mapped) {
            if (l.mapMemory) {
                // Transition from Reserved to Prepared to Ready.
                var n = pEnd - l.mapped;
                sysMap(((@unsafe.Pointer)l.mapped), n, ᏑsysStat);
                sysUsed(((@unsafe.Pointer)l.mapped), n, n);
            }
            l.mapped = pEnd;
        }
    }
    return ((@unsafe.Pointer)Δp);
}

// notInHeap is off-heap memory allocated by a lower-level allocator
// like sysAlloc or persistentAlloc.
//
// In general, it's better to use real types which embed
// runtime/internal/sys.NotInHeap, but this serves as a generic type
// for situations where that isn't possible (like in the allocators).
//
// TODO: Use this as the return type of sysAlloc, persistentAlloc, etc?
[GoType] partial struct notInHeap {
    internal runtime.@internal.sys_package.NotInHeap _;
}

[GoRecv] internal static ж<notInHeap> add(this ref notInHeap Δp, uintptr bytes) {
    return (ж<notInHeap>)(uintptr)(((@unsafe.Pointer)(((uintptr)(uintptr)@unsafe.Pointer.FromRef(ref Δp)) + bytes)));
}

// computeRZlog computes the size of the redzone.
// Refer to the implementation of the compiler-rt.
internal static uintptr computeRZlog(uintptr userSize) {
    switch (ᐧ) {
    case {} when userSize is <= (64 - 16): {
        return 16 << (int)(0);
    }
    case {} when userSize is <= (128 - 32): {
        return 16 << (int)(1);
    }
    case {} when userSize is <= (512 - 64): {
        return 16 << (int)(2);
    }
    case {} when userSize is <= (4096 - 128): {
        return 16 << (int)(3);
    }
    case {} when userSize is <= (1 << (int)(14)) - 256: {
        return 16 << (int)(4);
    }
    case {} when userSize is <= (1 << (int)(15)) - 512: {
        return 16 << (int)(5);
    }
    case {} when userSize is <= (1 << (int)(16)) - 1024: {
        return 16 << (int)(6);
    }
    default: {
        return 16 << (int)(7);
    }}

}

} // end runtime_package
