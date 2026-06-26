// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Page heap.
//
// See malloc.go for overview.
namespace go;

using cpu = @internal.cpu_package;
using goarch = @internal.goarch_package;
using atomic = @internal.runtime.atomic_package;
using sys = runtime.@internal.sys_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.runtime;
using runtime.@internal;

partial class runtime_package {

internal static readonly UntypedInt minPhysPageSize = 4096;
internal static readonly UntypedInt maxPhysPageSize = /* 512 << 10 */ 524288;
internal static readonly UntypedInt maxPhysHugePageSize = /* pallocChunkBytes */ 4194304;
internal static readonly UntypedInt pagesPerReclaimerChunk = 512;
internal const bool physPageAlignedStacks = /* GOOS == "openbsd" */ false;

[GoType("dyn")] partial struct mheap_curArena {
    internal uintptr @base;
    internal uintptr end;
}

[GoType("dyn")] partial struct mheap_userArena {
    // arenaHints is a list of addresses at which to attempt to
    // add more heap arenas for user arena chunks. This is initially
    // populated with a set of general hint addresses, and grown with
    // the bounds of actual heap arena ranges.
    internal ж<arenaHint> arenaHints;
    // quarantineList is a list of user arena spans that have been set to fault, but
    // are waiting for all pointers into them to go away. Sweeping handles
    // identifying when this is true, and moves the span to the ready list.
    internal mSpanList quarantineList;
    // readyList is a list of empty user arena spans that are ready for reuse.
    internal mSpanList readyList;
}

// Main malloc heap.
// The heap itself is the "free" and "scav" treaps,
// but all the other global data is here too.
//
// mheap must not be heap-allocated because it contains mSpanLists,
// which must not be heap-allocated.
[GoType] partial struct mheap {
    internal runtime.@internal.sys_package.NotInHeap _;
    // lock must only be acquired on the system stack, otherwise a g
    // could self-deadlock if its stack grows with the lock held.
    internal mutex @lock;
    internal pageAlloc pages; // page allocation data structure
    internal uint32 sweepgen; // sweep generation, see comment in mspan; written during STW
    // allspans is a slice of all mspans ever created. Each mspan
    // appears exactly once.
    //
    // The memory for allspans is manually managed and can be
    // reallocated and move as the heap grows.
    //
    // In general, allspans is protected by mheap_.lock, which
    // prevents concurrent access as well as freeing the backing
    // store. Accesses during STW might not hold the lock, but
    // must ensure that allocation cannot happen around the
    // access (since that may free the backing store).
    internal slice<ж<mspan>> allspans; // all spans out there
    // Proportional sweep
    //
    // These parameters represent a linear function from gcController.heapLive
    // to page sweep count. The proportional sweep system works to
    // stay in the black by keeping the current page sweep count
    // above this line at the current gcController.heapLive.
    //
    // The line has slope sweepPagesPerByte and passes through a
    // basis point at (sweepHeapLiveBasis, pagesSweptBasis). At
    // any given time, the system is at (gcController.heapLive,
    // pagesSwept) in this space.
    //
    // It is important that the line pass through a point we
    // control rather than simply starting at a 0,0 origin
    // because that lets us adjust sweep pacing at any time while
    // accounting for current progress. If we could only adjust
    // the slope, it would create a discontinuity in debt if any
    // progress has already been made.
    internal @internal.runtime.atomic_package.Uintptr pagesInUse; // pages of spans in stats mSpanInUse
    internal @internal.runtime.atomic_package.Uint64 pagesSwept; // pages swept this cycle
    internal @internal.runtime.atomic_package.Uint64 pagesSweptBasis; // pagesSwept to use as the origin of the sweep ratio
    internal uint64 sweepHeapLiveBasis;         // value of gcController.heapLive to use as the origin of sweep ratio; written with lock, read without
    internal float64 sweepPagesPerByte;        // proportional sweep ratio; written with lock, read without
// Page reclaimer state

    // reclaimIndex is the page index in allArenas of next page to
    // reclaim. Specifically, it refers to page (i %
    // pagesPerArena) of arena allArenas[i / pagesPerArena].
    //
    // If this is >= 1<<63, the page reclaimer is done scanning
    // the page marks.
    internal @internal.runtime.atomic_package.Uint64 reclaimIndex;
    // reclaimCredit is spare credit for extra pages swept. Since
    // the page reclaimer works in large chunks, it may reclaim
    // more than requested. Any spare pages released go to this
    // credit pool.
    internal @internal.runtime.atomic_package.Uintptr reclaimCredit;
    internal @internal.cpu_package.CacheLinePad __; // prevents false-sharing between arenas and preceding variables
    // arenas is the heap arena map. It points to the metadata for
    // the heap for every arena frame of the entire usable virtual
    // address space.
    //
    // Use arenaIndex to compute indexes into this array.
    //
    // For regions of the address space that are not backed by the
    // Go heap, the arena map contains nil.
    //
    // Modifications are protected by mheap_.lock. Reads can be
    // performed without locking; however, a given entry can
    // transition from nil to non-nil at any time when the lock
    // isn't held. (Entries never transitions back to nil.)
    //
    // In general, this is a two-level mapping consisting of an L1
    // map and possibly many L2 maps. This saves space when there
    // are a huge number of arena frames. However, on many
    // platforms (even 64-bit), arenaL1Bits is 0, making this
    // effectively a single-level map. In this case, arenas[0]
    // will never be nil.
    internal array<ж<array<ж<heapArena>>>> arenas = new(1 << (int)(arenaL1Bits));
    // arenasHugePages indicates whether arenas' L2 entries are eligible
    // to be backed by huge pages.
    internal bool arenasHugePages;
    // heapArenaAlloc is pre-reserved space for allocating heapArena
    // objects. This is only used on 32-bit, where we pre-reserve
    // this space to avoid interleaving it with the heap itself.
    internal linearAlloc heapArenaAlloc;
    // arenaHints is a list of addresses at which to attempt to
    // add more heap arenas. This is initially populated with a
    // set of general hint addresses, and grown with the bounds of
    // actual heap arena ranges.
    internal ж<arenaHint> arenaHints;
    // arena is a pre-reserved space for allocating heap arenas
    // (the actual arenas). This is only used on 32-bit.
    internal linearAlloc arena;
    // allArenas is the arenaIndex of every mapped arena. This can
    // be used to iterate through the address space.
    //
    // Access is protected by mheap_.lock. However, since this is
    // append-only and old backing arrays are never freed, it is
    // safe to acquire mheap_.lock, copy the slice header, and
    // then release mheap_.lock.
    internal slice<arenaIdx> allArenas;
    // sweepArenas is a snapshot of allArenas taken at the
    // beginning of the sweep cycle. This can be read safely by
    // simply blocking GC (by disabling preemption).
    internal slice<arenaIdx> sweepArenas;
    // markArenas is a snapshot of allArenas taken at the beginning
    // of the mark cycle. Because allArenas is append-only, neither
    // this slice nor its contents will change during the mark, so
    // it can be read safely.
    internal slice<arenaIdx> markArenas;
    // curArena is the arena that the heap is currently growing
    // into. This should always be physPageSize-aligned.
    internal mheap_curArena curArena;
    // central free lists for small size classes.
    // the padding makes sure that the mcentrals are
    // spaced CacheLinePadSize bytes apart, so that each mcentral.lock
    // gets its own cache line.
    // central is indexed by spanClass.
    internal array<struct{mcentral mcentral; pad <24>byte}> central = new(numSpanClasses);
    internal fixalloc spanalloc; // allocator for span*
    internal fixalloc cachealloc; // allocator for mcache*
    internal fixalloc specialfinalizeralloc; // allocator for specialfinalizer*
    internal fixalloc specialprofilealloc; // allocator for specialprofile*
    internal fixalloc specialReachableAlloc; // allocator for specialReachable
    internal fixalloc specialPinCounterAlloc; // allocator for specialPinCounter
    internal fixalloc specialWeakHandleAlloc; // allocator for specialWeakHandle
    internal mutex speciallock;    // lock for special record allocators.
    internal fixalloc arenaHintAlloc; // allocator for arenaHints
    // User arena state.
    //
    // Protected by mheap_.lock.
    internal mheap_userArena userArena;
    internal ж<specialfinalizer> unused; // never set, just here to force the specialfinalizer type into DWARF
}

internal static mheap mheap_;

// A heapArena stores metadata for a heap arena. heapArenas are stored
// outside of the Go heap and accessed via the mheap_.arenas index.
[GoType] partial struct heapArena {
    internal runtime.@internal.sys_package.NotInHeap _;
    // spans maps from virtual address page ID within this arena to *mspan.
    // For allocated spans, their pages map to the span itself.
    // For free spans, only the lowest and highest pages map to the span itself.
    // Internal pages map to an arbitrary span.
    // For pages that have never been allocated, spans entries are nil.
    //
    // Modifications are protected by mheap.lock. Reads can be
    // performed without locking, but ONLY from indexes that are
    // known to contain in-use or stack spans. This means there
    // must not be a safe-point between establishing that an
    // address is live and looking it up in the spans array.
    internal array<ж<mspan>> spans = new(pagesPerArena);
    // pageInUse is a bitmap that indicates which spans are in
    // state mSpanInUse. This bitmap is indexed by page number,
    // but only the bit corresponding to the first page in each
    // span is used.
    //
    // Reads and writes are atomic.
    internal array<uint8> pageInUse = new(pagesPerArena / 8);
    // pageMarks is a bitmap that indicates which spans have any
    // marked objects on them. Like pageInUse, only the bit
    // corresponding to the first page in each span is used.
    //
    // Writes are done atomically during marking. Reads are
    // non-atomic and lock-free since they only occur during
    // sweeping (and hence never race with writes).
    //
    // This is used to quickly find whole spans that can be freed.
    //
    // TODO(austin): It would be nice if this was uint64 for
    // faster scanning, but we don't have 64-bit atomic bit
    // operations.
    internal array<uint8> pageMarks = new(pagesPerArena / 8);
    // pageSpecials is a bitmap that indicates which spans have
    // specials (finalizers or other). Like pageInUse, only the bit
    // corresponding to the first page in each span is used.
    //
    // Writes are done atomically whenever a special is added to
    // a span and whenever the last special is removed from a span.
    // Reads are done atomically to find spans containing specials
    // during marking.
    internal array<uint8> pageSpecials = new(pagesPerArena / 8);
    // checkmarks stores the debug.gccheckmark state. It is only
    // used if debug.gccheckmark > 0.
    internal ж<checkmarksMap> checkmarks;
    // zeroedBase marks the first byte of the first page in this
    // arena which hasn't been used yet and is therefore already
    // zero. zeroedBase is relative to the arena base.
    // Increases monotonically until it hits heapArenaBytes.
    //
    // This field is sufficient to determine if an allocation
    // needs to be zeroed because the page allocator follows an
    // address-ordered first-fit policy.
    //
    // Read atomically and written with an atomic CAS.
    internal uintptr zeroedBase;
}

// arenaHint is a hint for where to grow the heap arenas. See
// mheap_.arenaHints.
[GoType] partial struct arenaHint {
    internal runtime.@internal.sys_package.NotInHeap _;
    internal uintptr addr;
    internal bool down;
    internal ж<arenaHint> next;
}

[GoType("num:uint8")] partial struct mSpanState;

// An mspan is a run of pages.
//
// When a mspan is in the heap free treap, state == mSpanFree
// and heapmap(s->start) == span, heapmap(s->start+s->npages-1) == span.
// If the mspan is in the heap scav treap, then in addition to the
// above scavenged == true. scavenged == false in all other cases.
//
// When a mspan is allocated, state == mSpanInUse or mSpanManual
// and heapmap(i) == span for all s->start <= i < s->start+s->npages.
// Every mspan is in one doubly-linked list, either in the mheap's
// busy list or one of the mcentral's span lists.
internal static readonly mSpanState mSpanDead = /* iota */ 0;
internal static readonly mSpanState mSpanInUse = 1; // allocated for garbage collected heap
internal static readonly mSpanState mSpanManual = 2; // allocated for manual management (e.g., stack allocator)

// mSpanStateNames are the names of the span states, indexed by
// mSpanState.
internal static slice<@string> mSpanStateNames = new @string[]{
    "mSpanDead",
    "mSpanInUse",
    "mSpanManual"
}.slice();

// mSpanStateBox holds an atomic.Uint8 to provide atomic operations on
// an mSpanState. This is a separate type to disallow accidental comparison
// or assignment with mSpanState.
[GoType] partial struct mSpanStateBox {
    internal @internal.runtime.atomic_package.Uint8 s;
}

// It is nosplit to match get, below.

//go:nosplit
[GoRecv] internal static void set(this ref mSpanStateBox b, mSpanState s) {
    b.s.Store(((uint8)s));
}

// It is nosplit because it's called indirectly by typedmemclr,
// which must not be preempted.

//go:nosplit
[GoRecv] internal static mSpanState get(this ref mSpanStateBox b) {
    return ((mSpanState)b.s.Load());
}

// mSpanList heads a linked list of spans.
[GoType] partial struct mSpanList {
    internal runtime.@internal.sys_package.NotInHeap _;
    internal ж<mspan> first; // first span in list, or nil if none
    internal ж<mspan> last; // last span in list, or nil if none
}

[GoType] partial struct mspan {
    internal runtime.@internal.sys_package.NotInHeap _;
    internal ж<mspan> next;  // next span in list, or nil if none
    internal ж<mspan> prev;  // previous span in list, or nil if none
    internal ж<mSpanList> list; // For debugging.
    internal uintptr startAddr; // address of first byte of span aka s.base()
    internal uintptr npages; // number of pages in span
    internal gclinkptr manualFreeList; // list of free objects in mSpanManual spans
    // freeindex is the slot index between 0 and nelems at which to begin scanning
    // for the next free object in this span.
    // Each allocation scans allocBits starting at freeindex until it encounters a 0
    // indicating a free object. freeindex is then adjusted so that subsequent scans begin
    // just past the newly discovered free object.
    //
    // If freeindex == nelem, this span has no free objects.
    //
    // allocBits is a bitmap of objects in this span.
    // If n >= freeindex and allocBits[n/8] & (1<<(n%8)) is 0
    // then object n is free;
    // otherwise, object n is allocated. Bits starting at nelem are
    // undefined and should never be referenced.
    //
    // Object n starts at address n*elemsize + (start << pageShift).
    internal uint16 freeindex;
    // TODO: Look up nelems from sizeclass and remove this field if it
    // helps performance.
    internal uint16 nelems; // number of object in the span.
    // freeIndexForScan is like freeindex, except that freeindex is
    // used by the allocator whereas freeIndexForScan is used by the
    // GC scanner. They are two fields so that the GC sees the object
    // is allocated only when the object and the heap bits are
    // initialized (see also the assignment of freeIndexForScan in
    // mallocgc, and issue 54596).
    internal uint16 freeIndexForScan;
    // Cache of the allocBits at freeindex. allocCache is shifted
    // such that the lowest bit corresponds to the bit freeindex.
    // allocCache holds the complement of allocBits, thus allowing
    // ctz (count trailing zero) to use it directly.
    // allocCache may contain bits beyond s.nelems; the caller must ignore
    // these.
    internal uint64 allocCache;
    // allocBits and gcmarkBits hold pointers to a span's mark and
    // allocation bits. The pointers are 8 byte aligned.
    // There are three arenas where this data is held.
    // free: Dirty arenas that are no longer accessed
    //       and can be reused.
    // next: Holds information to be used in the next GC cycle.
    // current: Information being used during this GC cycle.
    // previous: Information being used during the last GC cycle.
    // A new GC cycle starts with the call to finishsweep_m.
    // finishsweep_m moves the previous arena to the free arena,
    // the current arena to the previous arena, and
    // the next arena to the current arena.
    // The next arena is populated as the spans request
    // memory to hold gcmarkBits for the next GC cycle as well
    // as allocBits for newly allocated spans.
    //
    // The pointer arithmetic is done "by hand" instead of using
    // arrays to avoid bounds checks along critical performance
    // paths.
    // The sweep will free the old allocBits and set allocBits to the
    // gcmarkBits. The gcmarkBits are replaced with a fresh zeroed
    // out memory.
    internal ж<gcBits> allocBits;
    internal ж<gcBits> gcmarkBits;
    internal ж<gcBits> pinnerBits; // bitmap for pinned objects; accessed atomically
// sweep generation:
// if sweepgen == h->sweepgen - 2, the span needs sweeping
// if sweepgen == h->sweepgen - 1, the span is currently being swept
// if sweepgen == h->sweepgen, the span is swept and ready to use
// if sweepgen == h->sweepgen + 1, the span was cached before sweep began and is still cached, and needs sweeping
// if sweepgen == h->sweepgen + 3, the span was swept and then cached and is still cached
// h->sweepgen is incremented by 2 after every GC
    internal uint32 sweepgen;
    internal uint32 divMul;        // for divide by elemsize
    internal uint16 allocCount;        // number of allocated objects
    internal spanClass spanclass;     // size class and noscan (uint8)
    internal mSpanStateBox state; // mSpanInUse etc; accessed atomically (get/set methods)
    internal uint8 needzero;         // needs to be zeroed before allocation
    internal bool isUserArenaChunk;          // whether or not this span represents a user arena
    internal uint16 allocCountBeforeCache;        // a copy of allocCount that is stored just before this span is cached
    internal uintptr elemsize;       // computed from sizeclass or from npages
    internal uintptr limit;       // end of data in span
    internal mutex speciallock;         // guards specials list and changes to pinnerBits
    internal ж<special> specials;   // linked list of special records sorted by offset.
    internal addrRange userArenaChunkFree;     // interval for managing chunk allocation
    internal ж<_type> largeType;     // malloc header for large objects.
}

[GoRecv] internal static uintptr @base(this ref mspan s) {
    return s.startAddr;
}

[GoRecv] internal static (uintptr size, uintptr n, uintptr total) layout(this ref mspan s) {
    uintptr size = default!;
    uintptr n = default!;
    uintptr total = default!;

    total = s.npages << (int)(_PageShift);
    size = s.elemsize;
    if (size > 0) {
        n = total / size;
    }
    return (size, n, total);
}

// recordspan adds a newly allocated span to h.allspans.
//
// This only happens the first time a span is allocated from
// mheap.spanalloc (it is not called when a span is reused).
//
// Write barriers are disallowed here because it can be called from
// gcWork when allocating new workbufs. However, because it's an
// indirect call from the fixalloc initializer, the compiler can't see
// this.
//
// The heap lock must be held.
//
//go:nowritebarrierrec
internal static void recordspan(@unsafe.Pointer vh, @unsafe.Pointer Δp) {
    var h = (ж<mheap>)(uintptr)(vh);
    var s = (ж<mspan>)(uintptr)(Δp);
    assertLockHeld(Ꮡ((~h).@lock));
    if (len((~h).allspans) >= cap((~h).allspans)) {
        nint n = 64 * 1024 / goarch.PtrSize;
        if (n < cap((~h).allspans) * 3 / 2) {
            n = cap((~h).allspans) * 3 / 2;
        }
        slice<ж<mspan>> @new = default!;
        var sp = (ж<Δslice>)(uintptr)(new @unsafe.Pointer(Ꮡ(@new)));
        sp.val.Δarray = (uintptr)sysAlloc(((uintptr)n) * goarch.PtrSize, Ꮡmemstats.of(mstats.Ꮡother_sys));
        if ((~sp).Δarray == nil) {
            @throw("runtime: cannot allocate memory"u8);
        }
        sp.val.len = len((~h).allspans);
        sp.val.cap = n;
        if (len((~h).allspans) > 0) {
            copy(@new, (~h).allspans);
        }
        var oldAllspans = h.val.allspans;
        ((ж<notInHeapSlice>)(uintptr)(new @unsafe.Pointer(Ꮡ((~h).allspans)))).val = ((ж<notInHeapSlice>)(uintptr)(new @unsafe.Pointer(Ꮡ(@new)))).val;
        if (len(oldAllspans) != 0) {
            sysFree(((@unsafe.Pointer)(Ꮡ(oldAllspans, 0))), ((uintptr)cap(oldAllspans)) * @unsafe.Sizeof(oldAllspans[0]), Ꮡmemstats.of(mstats.Ꮡother_sys));
        }
    }
    h.val.allspans = (~h).allspans[..(int)(len((~h).allspans) + 1)];
    (~h).allspans[len((~h).allspans) - 1] = s;
}

[GoType("num:uint8")] partial struct spanClass;

internal static readonly UntypedInt numSpanClasses = /* _NumSizeClasses << 1 */ 136;
internal static readonly spanClass tinySpanClass = /* spanClass(tinySizeClass<<1 | 1) */ 5;

internal static spanClass makeSpanClass(uint8 sizeclass, bool noscan) {
    return (spanClass)(((spanClass)(sizeclass << (int)(1))) | ((spanClass)bool2int(noscan)));
}

//go:nosplit
internal static int8 sizeclass(this spanClass sc) {
    return ((int8)(sc >> (int)(1)));
}

//go:nosplit
internal static bool noscan(this spanClass sc) {
    return (spanClass)(sc & 1) != 0;
}

// arenaIndex returns the index into mheap_.arenas of the arena
// containing metadata for p. This index combines of an index into the
// L1 map and an index into the L2 map and should be used as
// mheap_.arenas[ai.l1()][ai.l2()].
//
// If p is outside the range of valid heap addresses, either l1() or
// l2() will be out of bounds.
//
// It is nosplit because it's called by spanOf and several other
// nosplit functions.
//
//go:nosplit
internal static arenaIdx arenaIndex(uintptr Δp) {
    return ((arenaIdx)((Δp - arenaBaseOffset) / heapArenaBytes));
}

// arenaBase returns the low address of the region covered by heap
// arena i.
internal static uintptr arenaBase(arenaIdx i) {
    return ((uintptr)i) * heapArenaBytes + arenaBaseOffset;
}

[GoType("num:nuint")] partial struct arenaIdx;

// l1 returns the "l1" portion of an arenaIdx.
//
// Marked nosplit because it's called by spanOf and other nosplit
// functions.
//
//go:nosplit
internal static nuint l1(this arenaIdx i) {
    if (arenaL1Bits == 0){
        // Let the compiler optimize this away if there's no
        // L1 map.
        return 0;
    } else {
        return ((nuint)i) >> (int)(arenaL1Shift);
    }
}

// l2 returns the "l2" portion of an arenaIdx.
//
// Marked nosplit because it's called by spanOf and other nosplit funcs.
// functions.
//
//go:nosplit
internal static nuint l2(this arenaIdx i) {
    if (arenaL1Bits == 0){
        return ((nuint)i);
    } else {
        return (nuint)(((nuint)i) & (1 << (int)(arenaL2Bits) - 1));
    }
}

// inheap reports whether b is a pointer into a (potentially dead) heap object.
// It returns false for pointers into mSpanManual spans.
// Non-preemptible because it is used by write barriers.
//
//go:nowritebarrier
//go:nosplit
internal static bool inheap(uintptr b) {
    return spanOfHeap(b) != nil;
}

// inHeapOrStack is a variant of inheap that returns true for pointers
// into any allocated heap span.
//
//go:nowritebarrier
//go:nosplit
internal static bool inHeapOrStack(uintptr b) {
    var s = spanOf(b);
    if (s == nil || b < s.@base()) {
        return false;
    }
    var exprᴛ1 = (~s).state.get();
    if (exprᴛ1 == mSpanInUse || exprᴛ1 == mSpanManual) {
        return b < (~s).limit;
    }
    { /* default: */
        return false;
    }

}

// spanOf returns the span of p. If p does not point into the heap
// arena or no span has ever contained p, spanOf returns nil.
//
// If p does not point to allocated memory, this may return a non-nil
// span that does *not* contain p. If this is a possibility, the
// caller should either call spanOfHeap or check the span bounds
// explicitly.
//
// Must be nosplit because it has callers that are nosplit.
//
//go:nosplit
internal static ж<mspan> spanOf(uintptr Δp) {
    // This function looks big, but we use a lot of constant
    // folding around arenaL1Bits to get it under the inlining
    // budget. Also, many of the checks here are safety checks
    // that Go needs to do anyway, so the generated code is quite
    // short.
    arenaIdx ri = arenaIndex(Δp);
    if (arenaL1Bits == 0){
        // If there's no L1, then ri.l1() can't be out of bounds but ri.l2() can.
        if (ri.l2() >= ((nuint)len(mheap_.arenas[0]))) {
            return default!;
        }
    } else {
        // If there's an L1, then ri.l1() can be out of bounds but ri.l2() can't.
        if (ri.l1() >= ((nuint)len(mheap_.arenas))) {
            return default!;
        }
    }
    var l2 = mheap_.arenas[ri.l1()];
    if (arenaL1Bits != 0 && l2 == nil) {
        // Should never happen if there's no L1.
        return default!;
    }
    var ha = l2.val[ri.l2()];
    if (ha == nil) {
        return default!;
    }
    return (~ha).spans[(Δp / pageSize) % pagesPerArena];
}

// spanOfUnchecked is equivalent to spanOf, but the caller must ensure
// that p points into an allocated heap arena.
//
// Must be nosplit because it has callers that are nosplit.
//
//go:nosplit
internal static ж<mspan> spanOfUnchecked(uintptr Δp) {
    arenaIdx ai = arenaIndex(Δp);
    return (~mheap_.arenas[ai.l1()].val[ai.l2()]).spans[(Δp / pageSize) % pagesPerArena];
}

// spanOfHeap is like spanOf, but returns nil if p does not point to a
// heap object.
//
// Must be nosplit because it has callers that are nosplit.
//
//go:nosplit
internal static ж<mspan> spanOfHeap(uintptr Δp) {
    var s = spanOf(Δp);
    // s is nil if it's never been allocated. Otherwise, we check
    // its state first because we don't trust this pointer, so we
    // have to synchronize with span initialization. Then, it's
    // still possible we picked up a stale span pointer, so we
    // have to check the span's bounds.
    if (s == nil || (~s).state.get() != mSpanInUse || Δp < s.@base() || Δp >= (~s).limit) {
        return default!;
    }
    return s;
}

// pageIndexOf returns the arena, page index, and page mask for pointer p.
// The caller must ensure p is in the heap.
internal static (ж<heapArena> arena, uintptr pageIdx, uint8 pageMask) pageIndexOf(uintptr Δp) {
    ж<heapArena> arena = default!;
    uintptr pageIdx = default!;
    uint8 pageMask = default!;

    arenaIdx ai = arenaIndex(Δp);
    arena = mheap_.arenas[ai.l1()].val[ai.l2()];
    pageIdx = ((Δp / pageSize) / 8) % ((uintptr)len((~arena).pageInUse));
    pageMask = ((byte)(1 << (int)(((Δp / pageSize) % 8))));
    return (arena, pageIdx, pageMask);
}

// Initialize the heap.
[GoRecv] internal static void init(this ref mheap h) {
    lockInit(Ꮡ(h.@lock), lockRankMheap);
    lockInit(Ꮡ(h.speciallock), lockRankMheapSpecial);
    h.spanalloc.init(@unsafe.Sizeof(new mspan(nil)), recordspan, (uintptr)@unsafe.Pointer.FromRef(ref h), Ꮡmemstats.of(mstats.Ꮡmspan_sys));
    h.cachealloc.init(@unsafe.Sizeof(new mcache(nil)), default!, nil, Ꮡmemstats.of(mstats.Ꮡmcache_sys));
    h.specialfinalizeralloc.init(@unsafe.Sizeof(new specialfinalizer(nil)), default!, nil, Ꮡmemstats.of(mstats.Ꮡother_sys));
    h.specialprofilealloc.init(@unsafe.Sizeof(new specialprofile(nil)), default!, nil, Ꮡmemstats.of(mstats.Ꮡother_sys));
    h.specialReachableAlloc.init(@unsafe.Sizeof(new specialReachable(nil)), default!, nil, Ꮡmemstats.of(mstats.Ꮡother_sys));
    h.specialPinCounterAlloc.init(@unsafe.Sizeof(new specialPinCounter(nil)), default!, nil, Ꮡmemstats.of(mstats.Ꮡother_sys));
    h.specialWeakHandleAlloc.init(@unsafe.Sizeof(new specialWeakHandle(nil)), default!, nil, Ꮡmemstats.of(mstats.ᏑgcMiscSys));
    h.arenaHintAlloc.init(@unsafe.Sizeof(new arenaHint(nil)), default!, nil, Ꮡmemstats.of(mstats.Ꮡother_sys));
    // Don't zero mspan allocations. Background sweeping can
    // inspect a span concurrently with allocating it, so it's
    // important that the span's sweepgen survive across freeing
    // and re-allocating a span to prevent background sweeping
    // from improperly cas'ing it from 0.
    //
    // This is safe because mspan contains no heap pointers.
    h.spanalloc.zero = false;
    // h->mapcache needs no init
    foreach (var (i, _) in h.central) {
        h.central[i].mcentral.init(((spanClass)i));
    }
    h.pages.init(Ꮡ(h.@lock), Ꮡmemstats.of(mstats.ᏑgcMiscSys), false);
}

// reclaim sweeps and reclaims at least npage pages into the heap.
// It is called before allocating npage pages to keep growth in check.
//
// reclaim implements the page-reclaimer half of the sweeper.
//
// h.lock must NOT be held.
[GoRecv] internal static void reclaim(this ref mheap h, uintptr npage) {
    // TODO(austin): Half of the time spent freeing spans is in
    // locking/unlocking the heap (even with low contention). We
    // could make the slow path here several times faster by
    // batching heap frees.
    // Bail early if there's no more reclaim work.
    if (h.reclaimIndex.Load() >= 1 << (int)(63)) {
        return;
    }
    // Disable preemption so the GC can't start while we're
    // sweeping, so we can read h.sweepArenas, and so
    // traceGCSweepStart/Done pair on the P.
    var mp = acquirem();
    var Δtrace = traceAcquire();
    if (Δtrace.ok()) {
        Δtrace.GCSweepStart();
        traceRelease(Δtrace);
    }
    var arenas = h.sweepArenas;
    var locked = false;
    while (npage > 0) {
        // Pull from accumulated credit first.
        {
            var credit = h.reclaimCredit.Load(); if (credit > 0) {
                var take = credit;
                if (take > npage) {
                    // Take only what we need.
                    take = npage;
                }
                if (h.reclaimCredit.CompareAndSwap(credit, credit - take)) {
                    npage -= take;
                }
                continue;
            }
        }
        // Claim a chunk of work.
        var idx = ((uintptr)(h.reclaimIndex.Add(pagesPerReclaimerChunk) - pagesPerReclaimerChunk));
        if (idx / pagesPerArena >= ((uintptr)len(arenas))) {
            // Page reclaiming is done.
            h.reclaimIndex.Store(1 << (int)(63));
            break;
        }
        if (!locked) {
            // Lock the heap for reclaimChunk.
            @lock(Ꮡ(h.@lock));
            locked = true;
        }
        // Scan this chunk.
        var nfound = h.reclaimChunk(arenas, idx, pagesPerReclaimerChunk);
        if (nfound <= npage){
            npage -= nfound;
        } else {
            // Put spare pages toward global credit.
            h.reclaimCredit.Add(nfound - npage);
            npage = 0;
        }
    }
    if (locked) {
        unlock(Ꮡ(h.@lock));
    }
    Δtrace = traceAcquire();
    if (Δtrace.ok()) {
        Δtrace.GCSweepDone();
        traceRelease(Δtrace);
    }
    releasem(mp);
}

// reclaimChunk sweeps unmarked spans that start at page indexes [pageIdx, pageIdx+n).
// It returns the number of pages returned to the heap.
//
// h.lock must be held and the caller must be non-preemptible. Note: h.lock may be
// temporarily unlocked and re-locked in order to do sweeping or if tracing is
// enabled.
[GoRecv] internal static uintptr reclaimChunk(this ref mheap h, slice<arenaIdx> arenas, uintptr pageIdx, uintptr n) {
    // The heap lock must be held because this accesses the
    // heapArena.spans arrays using potentially non-live pointers.
    // In particular, if a span were freed and merged concurrently
    // with this probing heapArena.spans, it would be possible to
    // observe arbitrary, stale span pointers.
    assertLockHeld(Ꮡ(h.@lock));
    var n0 = n;
    uintptr nFreed = default!;
    var sl = Δsweep.active.begin();
    if (!sl.valid) {
        return 0;
    }
    while (n > 0) {
        arenaIdx ai = arenas[pageIdx / pagesPerArena];
        var ha = h.arenas[ai.l1()].val[ai.l2()];
        // Get a chunk of the bitmap to work on.
        nuint arenaPage = ((nuint)(pageIdx % pagesPerArena));
        var inUse = (~ha).pageInUse[(int)(arenaPage / 8)..];
        var marked = (~ha).pageMarks[(int)(arenaPage / 8)..];
        if (((uintptr)len(inUse)) > n / 8) {
            inUse = inUse[..(int)(n / 8)];
            marked = marked[..(int)(n / 8)];
        }
        // Scan this bitmap chunk for spans that are in-use
        // but have no marked objects on them.
        ref var i = ref heap(new nint(), out var Ꮡi);

        foreach (var (i, _) in inUse) {
            var inUseUnmarked = (uint8)(atomic.Load8(Ꮡ(inUse, i)) & ~marked[i]);
            if (inUseUnmarked == 0) {
                continue;
            }
            for (nuint j = ((nuint)0); j < 8; j++) {
                if ((uint8)(inUseUnmarked & (1 << (int)(j))) != 0) {
                    var s = (~ha).spans[arenaPage + ((nuint)i) * 8 + j];
                    {
                        var (sΔ1, ok) = sl.tryAcquire(s); if (ok) {
                            var npages = sΔ1.npages;
                            unlock(Ꮡ(h.@lock));
                            if (sΔ1.sweep(false)) {
                                nFreed += npages;
                            }
                            @lock(Ꮡ(h.@lock));
                            // Reload inUse. It's possible nearby
                            // spans were freed when we dropped the
                            // lock and we don't want to get stale
                            // pointers from the spans array.
                            inUseUnmarked = (uint8)(atomic.Load8(Ꮡ(inUse, i)) & ~marked[i]);
                        }
                    }
                }
            }
        }
        // Advance.
        pageIdx += ((uintptr)(len(inUse) * 8));
        n -= ((uintptr)(len(inUse) * 8));
    }
    Δsweep.active.end(sl);
    var Δtrace = traceAcquire();
    if (Δtrace.ok()) {
        unlock(Ꮡ(h.@lock));
        // Account for pages scanned but not reclaimed.
        Δtrace.GCSweepSpan((n0 - nFreed) * pageSize);
        traceRelease(Δtrace);
        @lock(Ꮡ(h.@lock));
    }
    assertLockHeld(Ꮡ(h.@lock));
    // Must be locked on return.
    return nFreed;
}

[GoType("num:uint8")] partial struct spanAllocType;

internal static readonly spanAllocType spanAllocHeap = /* iota */ 0;         // heap span
internal static readonly spanAllocType spanAllocStack = 1;        // stack span
internal static readonly spanAllocType spanAllocPtrScalarBits = 2; // unrolled GC prog bitmap span
internal static readonly spanAllocType spanAllocWorkBuf = 3;      // work buf span

// manual returns true if the span allocation is manually managed.
internal static bool manual(this spanAllocType s) {
    return s != spanAllocHeap;
}

// alloc allocates a new span of npage pages from the GC'd heap.
//
// spanclass indicates the span's size class and scannability.
//
// Returns a span that has been fully initialized. span.needzero indicates
// whether the span has been zeroed. Note that it may not be.
[GoRecv] internal static ж<mspan> alloc(this ref mheap h, uintptr npages, spanClass spanclass) {
    // Don't do any operations that lock the heap on the G stack.
    // It might trigger stack growth, and the stack growth code needs
    // to be able to allocate heap.
    ж<mspan> s = default!;
    systemstack(
    var sʗ2 = s;
    () => {
        if (!isSweepDone()) {
            h.reclaim(npages);
        }
        sʗ2 = h.allocSpan(npages, spanAllocHeap, spanclass);
    });
    return s;
}

// allocManual allocates a manually-managed span of npage pages.
// allocManual returns nil if allocation fails.
//
// allocManual adds the bytes used to *stat, which should be a
// memstats in-use field. Unlike allocations in the GC'd heap, the
// allocation does *not* count toward heapInUse.
//
// The memory backing the returned span may not be zeroed if
// span.needzero is set.
//
// allocManual must be called on the system stack because it may
// acquire the heap lock via allocSpan. See mheap for details.
//
// If new code is written to call allocManual, do NOT use an
// existing spanAllocType value and instead declare a new one.
//
//go:systemstack
[GoRecv] internal static ж<mspan> allocManual(this ref mheap h, uintptr npages, spanAllocType typ) {
    if (!typ.manual()) {
        @throw("manual span allocation called with non-manually-managed type"u8);
    }
    return h.allocSpan(npages, typ, 0);
}

// setSpans modifies the span map so [spanOf(base), spanOf(base+npage*pageSize))
// is s.
[GoRecv] internal static void setSpans(this ref mheap h, uintptr @base, uintptr npage, ж<mspan> Ꮡs) {
    ref var s = ref Ꮡs.val;

    var Δp = @base / pageSize;
    arenaIdx ai = arenaIndex(@base);
    var ha = h.arenas[ai.l1()].val[ai.l2()];
    for (var n = ((uintptr)0); n < npage; n++) {
        var i = (Δp + n) % pagesPerArena;
        if (i == 0) {
            ai = arenaIndex(@base + n * pageSize);
            ha = h.arenas[ai.l1()].val[ai.l2()];
        }
        (~ha).spans[i] = s;
    }
}

// allocNeedsZero checks if the region of address space [base, base+npage*pageSize),
// assumed to be allocated, needs to be zeroed, updating heap arena metadata for
// future allocations.
//
// This must be called each time pages are allocated from the heap, even if the page
// allocator can otherwise prove the memory it's allocating is already zero because
// they're fresh from the operating system. It updates heapArena metadata that is
// critical for future page allocations.
//
// There are no locking constraints on this method.
[GoRecv] internal static bool /*needZero*/ allocNeedsZero(this ref mheap h, uintptr @base, uintptr npage) {
    bool needZero = default!;

    while (npage > 0) {
        arenaIdx ai = arenaIndex(@base);
        var ha = h.arenas[ai.l1()].val[ai.l2()];
        var zeroedBase = atomic.Loaduintptr(Ꮡ((~ha).zeroedBase));
        var arenaBase = @base % heapArenaBytes;
        if (arenaBase < zeroedBase) {
            // We extended into the non-zeroed part of the
            // arena, so this region needs to be zeroed before use.
            //
            // zeroedBase is monotonically increasing, so if we see this now then
            // we can be sure we need to zero this memory region.
            //
            // We still need to update zeroedBase for this arena, and
            // potentially more arenas.
            needZero = true;
        }
        // We may observe arenaBase > zeroedBase if we're racing with one or more
        // allocations which are acquiring memory directly before us in the address
        // space. But, because we know no one else is acquiring *this* memory, it's
        // still safe to not zero.
        // Compute how far into the arena we extend into, capped
        // at heapArenaBytes.
        var arenaLimit = arenaBase + npage * pageSize;
        if (arenaLimit > heapArenaBytes) {
            arenaLimit = heapArenaBytes;
        }
        // Increase ha.zeroedBase so it's >= arenaLimit.
        // We may be racing with other updates.
        while (arenaLimit > zeroedBase) {
            if (atomic.Casuintptr(Ꮡ((~ha).zeroedBase), zeroedBase, arenaLimit)) {
                break;
            }
            zeroedBase = atomic.Loaduintptr(Ꮡ((~ha).zeroedBase));
            // Double check basic conditions of zeroedBase.
            if (zeroedBase <= arenaLimit && zeroedBase > arenaBase) {
                // The zeroedBase moved into the space we were trying to
                // claim. That's very bad, and indicates someone allocated
                // the same region we did.
                @throw("potentially overlapping in-use allocations detected"u8);
            }
        }
        // Move base forward and subtract from npage to move into
        // the next arena, or finish.
        @base += arenaLimit - arenaBase;
        npage -= (arenaLimit - arenaBase) / pageSize;
    }
    return needZero;
}

// tryAllocMSpan attempts to allocate an mspan object from
// the P-local cache, but may fail.
//
// h.lock need not be held.
//
// This caller must ensure that its P won't change underneath
// it during this function. Currently to ensure that we enforce
// that the function is run on the system stack, because that's
// the only place it is used now. In the future, this requirement
// may be relaxed if its use is necessary elsewhere.
//
//go:systemstack
[GoRecv] internal static ж<mspan> tryAllocMSpan(this ref mheap h) {
    var pp = (~(~getg()).m).p.ptr();
    // If we don't have a p or the cache is empty, we can't do
    // anything here.
    if (pp == nil || (~pp).mspancache.len == 0) {
        return default!;
    }
    // Pull off the last entry in the cache.
    var s = (~pp).mspancache.buf[(~pp).mspancache.len - 1];
    (~pp).mspancache.len--;
    return s;
}

// allocMSpanLocked allocates an mspan object.
//
// h.lock must be held.
//
// allocMSpanLocked must be called on the system stack because
// its caller holds the heap lock. See mheap for details.
// Running on the system stack also ensures that we won't
// switch Ps during this function. See tryAllocMSpan for details.
//
//go:systemstack
[GoRecv] internal static ж<mspan> allocMSpanLocked(this ref mheap h) {
    assertLockHeld(Ꮡ(h.@lock));
    var pp = (~(~getg()).m).p.ptr();
    if (pp == nil) {
        // We don't have a p so just do the normal thing.
        return (ж<mspan>)(uintptr)(h.spanalloc.alloc());
    }
    // Refill the cache if necessary.
    if ((~pp).mspancache.len == 0) {
        const nint refillCount = /* len(pp.mspancache.buf) / 2 */ 64;
        for (nint i = 0; i < refillCount; i++) {
            (~pp).mspancache.buf[i] = (ж<mspan>)(uintptr)(h.spanalloc.alloc());
        }
        (~pp).mspancache.len = refillCount;
    }
    // Pull off the last entry in the cache.
    var s = (~pp).mspancache.buf[(~pp).mspancache.len - 1];
    (~pp).mspancache.len--;
    return s;
}

// freeMSpanLocked free an mspan object.
//
// h.lock must be held.
//
// freeMSpanLocked must be called on the system stack because
// its caller holds the heap lock. See mheap for details.
// Running on the system stack also ensures that we won't
// switch Ps during this function. See tryAllocMSpan for details.
//
//go:systemstack
[GoRecv] internal static void freeMSpanLocked(this ref mheap h, ж<mspan> Ꮡs) {
    ref var s = ref Ꮡs.val;

    assertLockHeld(Ꮡ(h.@lock));
    var pp = (~(~getg()).m).p.ptr();
    // First try to free the mspan directly to the cache.
    if (pp != nil && (~pp).mspancache.len < len((~pp).mspancache.buf)) {
        (~pp).mspancache.buf[(~pp).mspancache.len] = s;
        (~pp).mspancache.len++;
        return;
    }
    // Failing that (or if we don't have a p), just free it to
    // the heap.
    h.spanalloc.free(new @unsafe.Pointer(Ꮡs));
}

// allocSpan allocates an mspan which owns npages worth of memory.
//
// If typ.manual() == false, allocSpan allocates a heap span of class spanclass
// and updates heap accounting. If manual == true, allocSpan allocates a
// manually-managed span (spanclass is ignored), and the caller is
// responsible for any accounting related to its use of the span. Either
// way, allocSpan will atomically add the bytes in the newly allocated
// span to *sysStat.
//
// The returned span is fully initialized.
//
// h.lock must not be held.
//
// allocSpan must be called on the system stack both because it acquires
// the heap lock and because it must block GC transitions.
//
//go:systemstack
[GoRecv] internal static ж<mspan> /*s*/ allocSpan(this ref mheap h, uintptr npages, spanAllocType typ, spanClass spanclass) {
    ж<mspan> s = default!;

    // Function-global state.
    var gp = getg();
    var (@base, scav) = (((uintptr)0), ((uintptr)0));
    var growth = ((uintptr)0);
    // On some platforms we need to provide physical page aligned stack
    // allocations. Where the page size is less than the physical page
    // size, we already manage to do this by default.
    var needPhysPageAlign = physPageAlignedStacks && typ == spanAllocStack && pageSize < physPageSize;
    // If the allocation is small enough, try the page cache!
    // The page cache does not support aligned allocations, so we cannot use
    // it if we need to provide a physical page aligned stack allocation.
    var pp = (~(~gp).m).p.ptr();
    if (!needPhysPageAlign && pp != nil && npages < pageCachePages / 4) {
        var c = Ꮡ((~pp).pcache);
        // If the cache is empty, refill it.
        if (c.empty()) {
            @lock(Ꮡ(h.@lock));
            c.val = h.pages.allocToCache();
            unlock(Ꮡ(h.@lock));
        }
        // Try to allocate from the cache.
        (@base, scav) = c.alloc(npages);
        if (@base != 0) {
            s = h.tryAllocMSpan();
            if (s != nil) {
                goto HaveSpan;
            }
        }
    }
    // We have a base but no mspan, so we need
    // to lock the heap.
    // For one reason or another, we couldn't get the
    // whole job done without the heap lock.
    @lock(Ꮡ(h.@lock));
    if (needPhysPageAlign) {
        // Overallocate by a physical page to allow for later alignment.
        var extraPages = physPageSize / pageSize;
        // Find a big enough region first, but then only allocate the
        // aligned portion. We can't just allocate and then free the
        // edges because we need to account for scavenged memory, and
        // that's difficult with alloc.
        //
        // Note that we skip updates to searchAddr here. It's OK if
        // it's stale and higher than normal; it'll operate correctly,
        // just come with a performance cost.
        (@base, _) = h.pages.find(npages + extraPages);
        if (@base == 0) {
            bool okΔ1 = default!;
            (growth, ) = h.grow(npages + extraPages);
            if (!okΔ1) {
                unlock(Ꮡ(h.@lock));
                return default!;
            }
            (@base, _) = h.pages.find(npages + extraPages);
            if (@base == 0) {
                @throw("grew heap, but no adequate free space found"u8);
            }
        }
        @base = alignUp(@base, physPageSize);
        scav = h.pages.allocRange(@base, npages);
    }
    if (@base == 0) {
        // Try to acquire a base address.
        (@base, scav) = h.pages.alloc(npages);
        if (@base == 0) {
            bool ok = default!;
            (growth, ok) = h.grow(npages);
            if (!ok) {
                unlock(Ꮡ(h.@lock));
                return default!;
            }
            (@base, scav) = h.pages.alloc(npages);
            if (@base == 0) {
                @throw("grew heap, but no adequate free space found"u8);
            }
        }
    }
    if (s == nil) {
        // We failed to get an mspan earlier, so grab
        // one now that we have the heap lock.
        s = h.allocMSpanLocked();
    }
    unlock(Ꮡ(h.@lock));
HaveSpan:
    var bytesToScavenge = ((uintptr)0);
    // Decide if we need to scavenge in response to what we just allocated.
    // Specifically, we track the maximum amount of memory to scavenge of all
    // the alternatives below, assuming that the maximum satisfies *all*
    // conditions we check (e.g. if we need to scavenge X to satisfy the
    // memory limit and Y to satisfy heap-growth scavenging, and Y > X, then
    // it's fine to pick Y, because the memory limit is still satisfied).
    //
    // It's fine to do this after allocating because we expect any scavenged
    // pages not to get touched until we return. Simultaneously, it's important
    // to do this before calling sysUsed because that may commit address space.
    var forceScavenge = false;
    {
        var limit = gcController.memoryLimit.Load(); if (!gcCPULimiter.limiting()) {
            // Assist with scavenging to maintain the memory limit by the amount
            // that we expect to page in.
            var inuse = gcController.mappedReady.Load();
            // Be careful about overflow, especially with uintptrs. Even on 32-bit platforms
            // someone can set a really big memory limit that isn't maxInt64.
            if (((uint64)scav) + inuse > ((uint64)limit)) {
                bytesToScavenge = ((uintptr)(((uint64)scav) + inuse - ((uint64)limit)));
                forceScavenge = true;
            }
        }
    }
    {
        var goal = Δscavenge.gcPercentGoal.Load(); if (goal != ~((uint64)0) && growth > 0) {
            // We just caused a heap growth, so scavenge down what will soon be used.
            // By scavenging inline we deal with the failure to allocate out of
            // memory fragments by scavenging the memory fragments that are least
            // likely to be re-used.
            //
            // Only bother with this because we're not using a memory limit. We don't
            // care about heap growths as long as we're under the memory limit, and the
            // previous check for scaving already handles that.
            {
                var retained = heapRetained(); if (retained + ((uint64)growth) > goal) {
                    // The scavenging algorithm requires the heap lock to be dropped so it
                    // can acquire it only sparingly. This is a potentially expensive operation
                    // so it frees up other goroutines to allocate in the meanwhile. In fact,
                    // they can make use of the growth we just created.
                    var todo = growth;
                    {
                        var overage = ((uintptr)(retained + ((uint64)growth) - goal)); if (todo > overage) {
                            todo = overage;
                        }
                    }
                    if (todo > bytesToScavenge) {
                        bytesToScavenge = todo;
                    }
                }
            }
        }
    }
    // There are a few very limited circumstances where we won't have a P here.
    // It's OK to simply skip scavenging in these cases. Something else will notice
    // and pick up the tab.
    int64 now = default!;
    if (pp != nil && bytesToScavenge > 0) {
        // Measure how long we spent scavenging and add that measurement to the assist
        // time so we can track it for the GC CPU limiter.
        //
        // Limiter event tracking might be disabled if we end up here
        // while on a mark worker.
        var start = nanotime();
        var track = (~pp).limiterEvent.start(limiterEventScavengeAssist, start);
        // Scavenge, but back out if the limiter turns on.
        var released = h.pages.scavenge(bytesToScavenge, 
        var gcCPULimiterʗ1 = gcCPULimiter;
        () => gcCPULimiterʗ1.limiting(), forceScavenge);
        mheap_.pages.scav.releasedEager.Add(released);
        // Finish up accounting.
        now = nanotime();
        if (track) {
            (~pp).limiterEvent.stop(limiterEventScavengeAssist, now);
        }
        Δscavenge.assistTime.Add(now - start);
    }
    // Initialize the span.
    h.initSpan(s, typ, spanclass, @base, npages);
    // Commit and account for any scavenged memory that the span now owns.
    var nbytes = npages * pageSize;
    if (scav != 0) {
        // sysUsed all the pages that are actually available
        // in the span since some of them might be scavenged.
        sysUsed(((@unsafe.Pointer)@base), nbytes, scav);
        gcController.heapReleased.add(-((int64)scav));
    }
    // Update stats.
    gcController.heapFree.add(-((int64)(nbytes - scav)));
    if (typ == spanAllocHeap) {
        gcController.heapInUse.add(((int64)nbytes));
    }
    // Update consistent stats.
    var stats = memstats.heapStats.acquire();
    atomic.Xaddint64(Ꮡ((~stats).committed), ((int64)scav));
    atomic.Xaddint64(Ꮡ((~stats).released), -((int64)scav));
    var exprᴛ1 = typ;
    if (exprᴛ1 == spanAllocHeap) {
        atomic.Xaddint64(Ꮡ((~stats).inHeap), ((int64)nbytes));
    }
    else if (exprᴛ1 == spanAllocStack) {
        atomic.Xaddint64(Ꮡ((~stats).inStacks), ((int64)nbytes));
    }
    else if (exprᴛ1 == spanAllocPtrScalarBits) {
        atomic.Xaddint64(Ꮡ((~stats).inPtrScalarBits), ((int64)nbytes));
    }
    else if (exprᴛ1 == spanAllocWorkBuf) {
        atomic.Xaddint64(Ꮡ((~stats).inWorkBufs), ((int64)nbytes));
    }

    memstats.heapStats.release();
    // Trace the span alloc.
    if (traceAllocFreeEnabled()) {
        var Δtrace = traceTryAcquire();
        if (Δtrace.ok()) {
            Δtrace.SpanAlloc(s);
            traceRelease(Δtrace);
        }
    }
    return s;
}

// initSpan initializes a blank span s which will represent the range
// [base, base+npages*pageSize). typ is the type of span being allocated.
[GoRecv] internal static void initSpan(this ref mheap h, ж<mspan> Ꮡs, spanAllocType typ, spanClass spanclass, uintptr @base, uintptr npages) {
    ref var s = ref Ꮡs.val;

    // At this point, both s != nil and base != 0, and the heap
    // lock is no longer held. Initialize the span.
    s.init(@base, npages);
    if (h.allocNeedsZero(@base, npages)) {
        s.needzero = 1;
    }
    var nbytes = npages * pageSize;
    if (typ.manual()){
        s.manualFreeList = 0;
        s.nelems = 0;
        s.limit = s.@base() + s.npages * pageSize;
        s.state.set(mSpanManual);
    } else {
        // We must set span properties before the span is published anywhere
        // since we're not holding the heap lock.
        s.spanclass = spanclass;
        {
            var sizeclass = spanclass.sizeclass(); if (sizeclass == 0){
                s.elemsize = nbytes;
                s.nelems = 1;
                s.divMul = 0;
            } else {
                s.elemsize = ((uintptr)class_to_size[sizeclass]);
                if (!s.spanclass.noscan() && heapBitsInSpan(s.elemsize)){
                    // Reserve space for the pointer/scan bitmap at the end.
                    s.nelems = ((uint16)((nbytes - (nbytes / goarch.PtrSize / 8)) / s.elemsize));
                } else {
                    s.nelems = ((uint16)(nbytes / s.elemsize));
                }
                s.divMul = class_to_divmagic[sizeclass];
            }
        }
        // Initialize mark and allocation structures.
        s.freeindex = 0;
        s.freeIndexForScan = 0;
        s.allocCache = ~((uint64)0);
        // all 1s indicating all free.
        s.gcmarkBits = newMarkBits(((uintptr)s.nelems));
        s.allocBits = newAllocBits(((uintptr)s.nelems));
        // It's safe to access h.sweepgen without the heap lock because it's
        // only ever updated with the world stopped and we run on the
        // systemstack which blocks a STW transition.
        atomic.Store(Ꮡ(s.sweepgen), h.sweepgen);
        // Now that the span is filled in, set its state. This
        // is a publication barrier for the other fields in
        // the span. While valid pointers into this span
        // should never be visible until the span is returned,
        // if the garbage collector finds an invalid pointer,
        // access to the span may race with initialization of
        // the span. We resolve this race by atomically
        // setting the state after the span is fully
        // initialized, and atomically checking the state in
        // any situation where a pointer is suspect.
        s.state.set(mSpanInUse);
    }
    // Publish the span in various locations.
    // This is safe to call without the lock held because the slots
    // related to this span will only ever be read or modified by
    // this thread until pointers into the span are published (and
    // we execute a publication barrier at the end of this function
    // before that happens) or pageInUse is updated.
    h.setSpans(s.@base(), npages, Ꮡs);
    if (!typ.manual()) {
        // Mark in-use span in arena page bitmap.
        //
        // This publishes the span to the page sweeper, so
        // it's imperative that the span be completely initialized
        // prior to this line.
        var (arena, pageIdx, pageMask) = pageIndexOf(s.@base());
        atomic.Or8(Ꮡ(~arena).pageInUse.at<uint8>(pageIdx), pageMask);
        // Update related page sweeper stats.
        h.pagesInUse.Add(npages);
    }
    // Make sure the newly allocated span will be observed
    // by the GC before pointers into the span are published.
    publicationBarrier();
}

// Try to add at least npage pages of memory to the heap,
// returning how much the heap grew by and whether it worked.
//
// h.lock must be held.
[GoRecv] internal static (uintptr, bool) grow(this ref mheap h, uintptr npage) {
    assertLockHeld(Ꮡ(h.@lock));
    // We must grow the heap in whole palloc chunks.
    // We call sysMap below but note that because we
    // round up to pallocChunkPages which is on the order
    // of MiB (generally >= to the huge page size) we
    // won't be calling it too much.
    var ask = alignUp(npage, pallocChunkPages) * pageSize;
    var totalGrowth = ((uintptr)0);
    // This may overflow because ask could be very large
    // and is otherwise unrelated to h.curArena.base.
    var end = h.curArena.@base + ask;
    var nBase = alignUp(end, physPageSize);
    if (nBase > h.curArena.end || end < h.curArena.@base) {
        /* overflow */
        // Not enough room in the current arena. Allocate more
        // arena space. This may not be contiguous with the
        // current arena, so we have to request the full ask.
        var (av, asize) = h.sysAlloc(ask, Ꮡ(h.arenaHints), true);
        if (av == nil) {
            var inUse = gcController.heapFree.load() + gcController.heapReleased.load() + gcController.heapInUse.load();
            print("runtime: out of memory: cannot allocate ", ask, "-byte block (", inUse, " in use)\n");
            return (0, false);
        }
        if (((uintptr)av) == h.curArena.end){
            // The new space is contiguous with the old
            // space, so just extend the current space.
            h.curArena.end = ((uintptr)av) + asize;
        } else {
            // The new space is discontiguous. Track what
            // remains of the current space and switch to
            // the new space. This should be rare.
            {
                var size = h.curArena.end - h.curArena.@base; if (size != 0) {
                    // Transition this space from Reserved to Prepared and mark it
                    // as released since we'll be able to start using it after updating
                    // the page allocator and releasing the lock at any time.
                    sysMap(((@unsafe.Pointer)h.curArena.@base), size, ᏑgcController.of(gcControllerState.ᏑheapReleased));
                    // Update stats.
                    var statsΔ1 = memstats.heapStats.acquire();
                    atomic.Xaddint64(Ꮡ((~statsΔ1).released), ((int64)size));
                    memstats.heapStats.release();
                    // Update the page allocator's structures to make this
                    // space ready for allocation.
                    h.pages.grow(h.curArena.@base, size);
                    totalGrowth += size;
                }
            }
            // Switch to the new space.
            h.curArena.@base = ((uintptr)av);
            h.curArena.end = ((uintptr)av) + asize;
        }
        // Recalculate nBase.
        // We know this won't overflow, because sysAlloc returned
        // a valid region starting at h.curArena.base which is at
        // least ask bytes in size.
        nBase = alignUp(h.curArena.@base + ask, physPageSize);
    }
    // Grow into the current arena.
    var v = h.curArena.@base;
    h.curArena.@base = nBase;
    // Transition the space we're going to use from Reserved to Prepared.
    //
    // The allocation is always aligned to the heap arena
    // size which is always > physPageSize, so its safe to
    // just add directly to heapReleased.
    sysMap(((@unsafe.Pointer)v), nBase - v, ᏑgcController.of(gcControllerState.ᏑheapReleased));
    // The memory just allocated counts as both released
    // and idle, even though it's not yet backed by spans.
    var stats = memstats.heapStats.acquire();
    atomic.Xaddint64(Ꮡ((~stats).released), ((int64)(nBase - v)));
    memstats.heapStats.release();
    // Update the page allocator's structures to make this
    // space ready for allocation.
    h.pages.grow(v, nBase - v);
    totalGrowth += nBase - v;
    return (totalGrowth, true);
}

// Free the span back into the heap.
[GoRecv] internal static void freeSpan(this ref mheap h, ж<mspan> Ꮡs) {
    ref var s = ref Ꮡs.val;

    systemstack(() => {
        if (traceAllocFreeEnabled()) {
            ref var trace = ref heap<traceLocker>(out var Ꮡtrace);
            Δtrace = traceTryAcquire();
            if (Δtrace.ok()) {
                Δtrace.SpanFree(Ꮡs);
                traceRelease(Δtrace);
            }
        }
        @lock(Ꮡ(h.@lock));
        if (msanenabled) {
            @unsafe.Pointer @base = ((@unsafe.Pointer)s.@base());
            var bytes = s.npages << (int)(_PageShift);
            msanfree(@base, bytes);
        }
        if (asanenabled) {
            @unsafe.Pointer @base = ((@unsafe.Pointer)s.@base());
            var bytes = s.npages << (int)(_PageShift);
            asanpoison(@base, bytes);
        }
        h.freeSpanLocked(Ꮡs, spanAllocHeap);
        unlock(Ꮡ(h.@lock));
    });
}

// freeManual frees a manually-managed span returned by allocManual.
// typ must be the same as the spanAllocType passed to the allocManual that
// allocated s.
//
// This must only be called when gcphase == _GCoff. See mSpanState for
// an explanation.
//
// freeManual must be called on the system stack because it acquires
// the heap lock. See mheap for details.
//
//go:systemstack
[GoRecv] internal static void freeManual(this ref mheap h, ж<mspan> Ꮡs, spanAllocType typ) {
    ref var s = ref Ꮡs.val;

    // Trace the span free.
    if (traceAllocFreeEnabled()) {
        var Δtrace = traceTryAcquire();
        if (Δtrace.ok()) {
            Δtrace.SpanFree(Ꮡs);
            traceRelease(Δtrace);
        }
    }
    s.needzero = 1;
    @lock(Ꮡ(h.@lock));
    h.freeSpanLocked(Ꮡs, typ);
    unlock(Ꮡ(h.@lock));
}

[GoRecv] internal static void freeSpanLocked(this ref mheap h, ж<mspan> Ꮡs, spanAllocType typ) {
    ref var s = ref Ꮡs.val;

    assertLockHeld(Ꮡ(h.@lock));
    var exprᴛ1 = s.state.get();
    if (exprᴛ1 == mSpanManual) {
        if (s.allocCount != 0) {
            @throw("mheap.freeSpanLocked - invalid stack free"u8);
        }
    }
    else if (exprᴛ1 == mSpanInUse) {
        if (s.isUserArenaChunk) {
            @throw("mheap.freeSpanLocked - invalid free of user arena chunk"u8);
        }
        if (s.allocCount != 0 || s.sweepgen != h.sweepgen) {
            print("mheap.freeSpanLocked - span ", s, " ptr ", ((Δhex)s.@base()), " allocCount ", s.allocCount, " sweepgen ", s.sweepgen, "/", h.sweepgen, "\n");
            @throw("mheap.freeSpanLocked - invalid free"u8);
        }
        h.pagesInUse.Add(-s.npages);
        var (arena, pageIdx, pageMask) = pageIndexOf(s.@base());
        atomic.And8(Ꮡ(~arena).pageInUse.at<uint8>(pageIdx), // Clear in-use bit in arena page bitmap.
 ~pageMask);
    }
    else { /* default: */
        @throw("mheap.freeSpanLocked - invalid span state"u8);
    }

    // Update stats.
    //
    // Mirrors the code in allocSpan.
    var nbytes = s.npages * pageSize;
    gcController.heapFree.add(((int64)nbytes));
    if (typ == spanAllocHeap) {
        gcController.heapInUse.add(-((int64)nbytes));
    }
    // Update consistent stats.
    var stats = memstats.heapStats.acquire();
    var exprᴛ2 = typ;
    if (exprᴛ2 == spanAllocHeap) {
        atomic.Xaddint64(Ꮡ((~stats).inHeap), -((int64)nbytes));
    }
    else if (exprᴛ2 == spanAllocStack) {
        atomic.Xaddint64(Ꮡ((~stats).inStacks), -((int64)nbytes));
    }
    else if (exprᴛ2 == spanAllocPtrScalarBits) {
        atomic.Xaddint64(Ꮡ((~stats).inPtrScalarBits), -((int64)nbytes));
    }
    else if (exprᴛ2 == spanAllocWorkBuf) {
        atomic.Xaddint64(Ꮡ((~stats).inWorkBufs), -((int64)nbytes));
    }

    memstats.heapStats.release();
    // Mark the space as free.
    h.pages.free(s.@base(), s.npages);
    // Free the span structure. We no longer have a use for it.
    s.state.set(mSpanDead);
    h.freeMSpanLocked(Ꮡs);
}

// scavengeAll acquires the heap lock (blocking any additional
// manipulation of the page allocator) and iterates over the whole
// heap, scavenging every free page available.
//
// Must run on the system stack because it acquires the heap lock.
//
//go:systemstack
[GoRecv] internal static void scavengeAll(this ref mheap h) {
    // Disallow malloc or panic while holding the heap lock. We do
    // this here because this is a non-mallocgc entry-point to
    // the mheap API.
    var gp = getg();
    (~(~gp).m).mallocing++;
    // Force scavenge everything.
    var released = h.pages.scavenge(~((uintptr)0), default!, true);
    (~(~gp).m).mallocing--;
    if (debug.scavtrace > 0) {
        printScavTrace(0, released, true);
    }
}

//go:linkname runtime_debug_freeOSMemory runtime/debug.freeOSMemory
internal static void runtime_debug_freeOSMemory() {
    GC();
    systemstack(
    var mheap_ʗ2 = mheap_;
    () => {
        mheap_ʗ2.scavengeAll();
    });
}

// Initialize a new span with the given start and npages.
[GoRecv] internal static void init(this ref mspan span, uintptr @base, uintptr npages) {
    // span is *not* zeroed.
    span.next = default!;
    span.prev = default!;
    span.list = default!;
    span.startAddr = @base;
    span.npages = npages;
    span.allocCount = 0;
    span.spanclass = 0;
    span.elemsize = 0;
    span.speciallock.key = 0;
    span.specials = default!;
    span.needzero = 0;
    span.freeindex = 0;
    span.freeIndexForScan = 0;
    span.allocBits = default!;
    span.gcmarkBits = default!;
    span.pinnerBits = default!;
    span.state.set(mSpanDead);
    lockInit(Ꮡ(span.speciallock), lockRankMspanSpecial);
}

[GoRecv] internal static bool inList(this ref mspan span) {
    return span.list != nil;
}

// Initialize an empty doubly-linked list.
[GoRecv] internal static void init(this ref mSpanList list) {
    list.first = default!;
    list.last = default!;
}

[GoRecv] internal static void remove(this ref mSpanList list, ж<mspan> Ꮡspan) {
    ref var span = ref Ꮡspan.val;

    if (span.list != list) {
        print("runtime: failed mSpanList.remove span.npages=", span.npages,
            " span=", span, " prev=", span.prev, " span.list=", span.list, " list=", list, "\n");
        @throw("mSpanList.remove"u8);
    }
    if (list.first == Ꮡspan){
        list.first = span.next;
    } else {
        span.prev.next = span.next;
    }
    if (list.last == Ꮡspan){
        list.last = span.prev;
    } else {
        span.next.prev = span.prev;
    }
    span.next = default!;
    span.prev = default!;
    span.list = default!;
}

[GoRecv] internal static bool isEmpty(this ref mSpanList list) {
    return list.first == nil;
}

[GoRecv] internal static void insert(this ref mSpanList list, ж<mspan> Ꮡspan) {
    ref var span = ref Ꮡspan.val;

    if (span.next != nil || span.prev != nil || span.list != nil) {
        println("runtime: failed mSpanList.insert", span, span.next, span.prev, span.list);
        @throw("mSpanList.insert"u8);
    }
    span.next = list.first;
    if (list.first != nil){
        // The list contains at least one span; link it in.
        // The last span in the list doesn't change.
        list.first.prev = span;
    } else {
        // The list contains no spans, so this is also the last span.
        list.last = span;
    }
    list.first = span;
    span.list = list;
}

[GoRecv] internal static void insertBack(this ref mSpanList list, ж<mspan> Ꮡspan) {
    ref var span = ref Ꮡspan.val;

    if (span.next != nil || span.prev != nil || span.list != nil) {
        println("runtime: failed mSpanList.insertBack", span, span.next, span.prev, span.list);
        @throw("mSpanList.insertBack"u8);
    }
    span.prev = list.last;
    if (list.last != nil){
        // The list contains at least one span.
        list.last.next = span;
    } else {
        // The list contains no spans, so this is also the first span.
        list.first = span;
    }
    list.last = span;
    span.list = list;
}

// takeAll removes all spans from other and inserts them at the front
// of list.
[GoRecv] internal static void takeAll(this ref mSpanList list, ж<mSpanList> Ꮡother) {
    ref var other = ref Ꮡother.val;

    if (other.isEmpty()) {
        return;
    }
    // Reparent everything in other to list.
    for (var s = other.first; s != nil; s = s.val.next) {
        s.val.list = list;
    }
    // Concatenate the lists.
    if (list.isEmpty()){
        list = other;
    } else {
        // Neither list is empty. Put other before list.
        other.last.next = list.first;
        list.first.prev = other.last;
        list.first = other.first;
    }
    (other.first, other.last) = (default!, default!);
}

internal static readonly UntypedInt _KindSpecialFinalizer = 1;
internal static readonly UntypedInt _KindSpecialWeakHandle = 2;
internal static readonly UntypedInt _KindSpecialProfile = 3;
internal static readonly UntypedInt _KindSpecialReachable = 4;
internal static readonly UntypedInt _KindSpecialPinCounter = 5;

[GoType] partial struct special {
    internal runtime.@internal.sys_package.NotInHeap _;
    internal ж<special> next; // linked list in span
    internal uint16 offset;   // span offset of object
    internal byte kind;     // kind of special
}

// spanHasSpecials marks a span as having specials in the arena bitmap.
internal static void spanHasSpecials(ж<mspan> Ꮡs) {
    ref var s = ref Ꮡs.val;

    ref var arenaPage = ref heap<uintptr>(out var ᏑarenaPage);
    arenaPage = (s.@base() / pageSize) % pagesPerArena;
    arenaIdx ai = arenaIndex(s.@base());
    var ha = mheap_.arenas[ai.l1()].val[ai.l2()];
    atomic.Or8(Ꮡ(~ha).pageSpecials.at<uint8>(arenaPage / 8), ((uint8)1) << (int)((arenaPage % 8)));
}

// spanHasNoSpecials marks a span as having no specials in the arena bitmap.
internal static void spanHasNoSpecials(ж<mspan> Ꮡs) {
    ref var s = ref Ꮡs.val;

    ref var arenaPage = ref heap<uintptr>(out var ᏑarenaPage);
    arenaPage = (s.@base() / pageSize) % pagesPerArena;
    arenaIdx ai = arenaIndex(s.@base());
    var ha = mheap_.arenas[ai.l1()].val[ai.l2()];
    atomic.And8(Ꮡ(~ha).pageSpecials.at<uint8>(arenaPage / 8), ~(((uint8)1) << (int)((arenaPage % 8))));
}

// Adds the special record s to the list of special records for
// the object p. All fields of s should be filled in except for
// offset & next, which this routine will fill in.
// Returns true if the special was successfully added, false otherwise.
// (The add will fail only if a record with the same p and s->kind
// already exists.)
internal static bool addspecial(@unsafe.Pointer Δp, ж<special> Ꮡs) {
    ref var s = ref Ꮡs.val;

    var span = spanOfHeap(((uintptr)Δp));
    if (span == nil) {
        @throw("addspecial on invalid pointer"u8);
    }
    // Ensure that the span is swept.
    // Sweeping accesses the specials list w/o locks, so we have
    // to synchronize with it. And it's just much safer.
    var mp = acquirem();
    span.ensureSwept();
    var offset = ((uintptr)Δp) - span.@base();
    var kind = s.kind;
    @lock(Ꮡ((~span).speciallock));
    // Find splice point, check for existing record.
    var (iter, exists) = span.specialFindSplicePoint(offset, kind);
    if (!exists) {
        // Splice in record, fill in offset.
        s.offset = ((uint16)offset);
        s.next = iter.val;
        iter.val = s;
        spanHasSpecials(span);
    }
    unlock(Ꮡ((~span).speciallock));
    releasem(mp);
    return !exists;
}

// already exists

// Removes the Special record of the given kind for the object p.
// Returns the record if the record existed, nil otherwise.
// The caller must FixAlloc_Free the result.
internal static ж<special> removespecial(@unsafe.Pointer Δp, uint8 kind) {
    var span = spanOfHeap(((uintptr)Δp));
    if (span == nil) {
        @throw("removespecial on invalid pointer"u8);
    }
    // Ensure that the span is swept.
    // Sweeping accesses the specials list w/o locks, so we have
    // to synchronize with it. And it's just much safer.
    var mp = acquirem();
    span.ensureSwept();
    var offset = ((uintptr)Δp) - span.@base();
    ж<special> result = default!;
    @lock(Ꮡ((~span).speciallock));
    var (iter, exists) = span.specialFindSplicePoint(offset, kind);
    if (exists) {
        var s = iter.val;
        iter.val = s.val.next;
        result = s;
    }
    if ((~span).specials == nil) {
        spanHasNoSpecials(span);
    }
    unlock(Ꮡ((~span).speciallock));
    releasem(mp);
    return result;
}

// Find a splice point in the sorted list and check for an already existing
// record. Returns a pointer to the next-reference in the list predecessor.
// Returns true, if the referenced item is an exact match.
[GoRecv] internal static (ж<ж<special>>, bool) specialFindSplicePoint(this ref mspan span, uintptr offset, byte kind) {
    // Find splice point, check for existing record.
    var iter = Ꮡ(span.specials);
    var found = false;
    while (ᐧ) {
        var s = iter.val;
        if (s == nil) {
            break;
        }
        if (offset == ((uintptr)(~s).offset) && kind == (~s).kind) {
            found = true;
            break;
        }
        if (offset < ((uintptr)(~s).offset) || (offset == ((uintptr)(~s).offset) && kind < (~s).kind)) {
            break;
        }
        iter = Ꮡ((~s).next);
    }
    return (iter, found);
}

// The described object has a finalizer set for it.
//
// specialfinalizer is allocated from non-GC'd memory, so any heap
// pointers must be specially handled.
[GoType] partial struct specialfinalizer {
    internal runtime.@internal.sys_package.NotInHeap _;
    internal special special;
    internal ж<funcval> fn; // May be a heap pointer.
    internal uintptr nret;
    internal ж<_type> fint; // May be a heap pointer, but always live.
    internal ж<ptrtype> ot; // May be a heap pointer, but always live.
}

// Adds a finalizer to the object p. Returns true if it succeeded.
internal static bool addfinalizer(@unsafe.Pointer Δp, ж<funcval> Ꮡf, uintptr nret, ж<_type> Ꮡfint, ж<ptrtype> Ꮡot) {
    ref var f = ref Ꮡf.val;
    ref var fint = ref Ꮡfint.val;
    ref var ot = ref Ꮡot.val;

    @lock(Ꮡmheap_.of(mheap.Ꮡspeciallock));
    var s = (ж<specialfinalizer>)(uintptr)(mheap_.specialfinalizeralloc.alloc());
    unlock(Ꮡmheap_.of(mheap.Ꮡspeciallock));
    (~s).special.kind = _KindSpecialFinalizer;
    s.val.fn = f;
    s.val.nret = nret;
    s.val.fint = fint;
    s.val.ot = ot;
    if (addspecial(p.val, Ꮡ((~s).special))) {
        // This is responsible for maintaining the same
        // GC-related invariants as markrootSpans in any
        // situation where it's possible that markrootSpans
        // has already run but mark termination hasn't yet.
        if (gcphase != _GCoff) {
            var (@base, span, _) = findObject(((uintptr)Δp), 0, 0);
            var mp = acquirem();
            var gcw = Ꮡ((~(~mp).p.ptr()).gcw);
            // Mark everything reachable from the object
            // so it's retained for the finalizer.
            if (!(~span).spanclass.noscan()) {
                scanobject(@base, gcw);
            }
            // Mark the finalizer itself, since the
            // special isn't part of the GC'd heap.
            scanblock(((uintptr)((@unsafe.Pointer)(Ꮡ((~s).fn)))), goarch.PtrSize, Ꮡoneptrmask.at<uint8>(0), gcw, nil);
            releasem(mp);
        }
        return true;
    }
    // There was an old finalizer
    @lock(Ꮡmheap_.of(mheap.Ꮡspeciallock));
    mheap_.specialfinalizeralloc.free(new @unsafe.Pointer(s));
    unlock(Ꮡmheap_.of(mheap.Ꮡspeciallock));
    return false;
}

// Removes the finalizer (if any) from the object p.
internal static void removefinalizer(@unsafe.Pointer Δp) {
    var s = (ж<specialfinalizer>)(uintptr)(new @unsafe.Pointer(removespecial(p.val, _KindSpecialFinalizer)));
    if (s == nil) {
        return;
    }
    // there wasn't a finalizer to remove
    @lock(Ꮡmheap_.of(mheap.Ꮡspeciallock));
    mheap_.specialfinalizeralloc.free(new @unsafe.Pointer(s));
    unlock(Ꮡmheap_.of(mheap.Ꮡspeciallock));
}

// The described object has a weak pointer.
//
// Weak pointers in the GC have the following invariants:
//
//   - Strong-to-weak conversions must ensure the strong pointer
//     remains live until the weak handle is installed. This ensures
//     that creating a weak pointer cannot fail.
//
//   - Weak-to-strong conversions require the weakly-referenced
//     object to be swept before the conversion may proceed. This
//     ensures that weak-to-strong conversions cannot resurrect
//     dead objects by sweeping them before that happens.
//
//   - Weak handles are unique and canonical for each byte offset into
//     an object that a strong pointer may point to, until an object
//     becomes unreachable.
//
//   - Weak handles contain nil as soon as an object becomes unreachable
//     the first time, before a finalizer makes it reachable again. New
//     weak handles created after resurrection are newly unique.
//
// specialWeakHandle is allocated from non-GC'd memory, so any heap
// pointers must be specially handled.
[GoType] partial struct specialWeakHandle {
    internal runtime.@internal.sys_package.NotInHeap _;
    internal special special;
    // handle is a reference to the actual weak pointer.
    // It is always heap-allocated and must be explicitly kept
    // live so long as this special exists.
    internal ж<@internal.runtime.atomic_package.Uintptr> handle;
}

//go:linkname internal_weak_runtime_registerWeakPointer internal/weak.runtime_registerWeakPointer
internal static @unsafe.Pointer internal_weak_runtime_registerWeakPointer(@unsafe.Pointer Δp) {
    return new @unsafe.Pointer(getOrAddWeakHandle(((@unsafe.Pointer)Δp)));
}

//go:linkname internal_weak_runtime_makeStrongFromWeak internal/weak.runtime_makeStrongFromWeak
internal static @unsafe.Pointer internal_weak_runtime_makeStrongFromWeak(@unsafe.Pointer u) {
    var handle = (ж<atomic.Uintptr>)(uintptr)(u);
    // Prevent preemption. We want to make sure that another GC cycle can't start.
    var mp = acquirem();
    var Δp = handle.Load();
    if (Δp == 0) {
        releasem(mp);
        return default!;
    }
    // Be careful. p may or may not refer to valid memory anymore, as it could've been
    // swept and released already. It's always safe to ensure a span is swept, though,
    // even if it's just some random span.
    var span = spanOfHeap(Δp);
    if (span == nil) {
        // The span probably got swept and released.
        releasem(mp);
        return default!;
    }
    // Ensure the span is swept.
    span.ensureSwept();
    // Now we can trust whatever we get from handle, so make a strong pointer.
    //
    // Even if we just swept some random span that doesn't contain this object, because
    // this object is long dead and its memory has since been reused, we'll just observe nil.
    @unsafe.Pointer ptr = ((@unsafe.Pointer)handle.Load());
    // This is responsible for maintaining the same GC-related
    // invariants as the Yuasa part of the write barrier. During
    // the mark phase, it's possible that we just created the only
    // valid pointer to the object pointed to by ptr. If it's only
    // ever referenced from our stack, and our stack is blackened
    // already, we could fail to mark it. So, mark it now.
    if (gcphase != _GCoff) {
        shade(((uintptr)ptr));
    }
    releasem(mp);
    // Explicitly keep ptr alive. This seems unnecessary since we return ptr,
    // but let's be explicit since it's important we keep ptr alive across the
    // call to shade.
    KeepAlive(ptr);
    return ptr;
}

// Retrieves or creates a weak pointer handle for the object p.
internal static ж<atomic.Uintptr> getOrAddWeakHandle(@unsafe.Pointer Δp) {
    // First try to retrieve without allocating.
    {
        var handleΔ1 = getWeakHandle(p.val); if (handleΔ1 != nil) {
            // Keep p alive for the duration of the function to ensure
            // that it cannot die while we're trying to do this.
            KeepAlive(Δp);
            return handleΔ1;
        }
    }
    @lock(Ꮡmheap_.of(mheap.Ꮡspeciallock));
    var s = (ж<specialWeakHandle>)(uintptr)(mheap_.specialWeakHandleAlloc.alloc());
    unlock(Ꮡmheap_.of(mheap.Ꮡspeciallock));
    var handle = @new<atomic.Uintptr>();
    (~s).special.kind = _KindSpecialWeakHandle;
    s.val.handle = handle;
    handle.Store(((uintptr)Δp));
    if (addspecial(p.val, Ꮡ((~s).special))) {
        // This is responsible for maintaining the same
        // GC-related invariants as markrootSpans in any
        // situation where it's possible that markrootSpans
        // has already run but mark termination hasn't yet.
        if (gcphase != _GCoff) {
            var mp = acquirem();
            var gcw = Ꮡ((~(~mp).p.ptr()).gcw);
            // Mark the weak handle itself, since the
            // special isn't part of the GC'd heap.
            scanblock(((uintptr)((@unsafe.Pointer)(Ꮡ((~s).handle)))), goarch.PtrSize, Ꮡoneptrmask.at<uint8>(0), gcw, nil);
            releasem(mp);
        }
        // Keep p alive for the duration of the function to ensure
        // that it cannot die while we're trying to do this.
        KeepAlive(Δp);
        return (~s).handle;
    }
    // There was an existing handle. Free the special
    // and try again. We must succeed because we're explicitly
    // keeping p live until the end of this function. Either
    // we, or someone else, must have succeeded, because we can
    // only fail in the event of a race, and p will still be
    // be valid no matter how much time we spend here.
    @lock(Ꮡmheap_.of(mheap.Ꮡspeciallock));
    mheap_.specialWeakHandleAlloc.free(new @unsafe.Pointer(s));
    unlock(Ꮡmheap_.of(mheap.Ꮡspeciallock));
    handle = getWeakHandle(p.val);
    if (handle == nil) {
        @throw("failed to get or create weak handle"u8);
    }
    // Keep p alive for the duration of the function to ensure
    // that it cannot die while we're trying to do this.
    KeepAlive(Δp);
    return handle;
}

internal static ж<atomic.Uintptr> getWeakHandle(@unsafe.Pointer Δp) {
    var span = spanOfHeap(((uintptr)Δp));
    if (span == nil) {
        @throw("getWeakHandle on invalid pointer"u8);
    }
    // Ensure that the span is swept.
    // Sweeping accesses the specials list w/o locks, so we have
    // to synchronize with it. And it's just much safer.
    var mp = acquirem();
    span.ensureSwept();
    var offset = ((uintptr)Δp) - span.@base();
    @lock(Ꮡ((~span).speciallock));
    // Find the existing record and return the handle if one exists.
    ж<atomic.Uintptr> handle = default!;
    var (iter, exists) = span.specialFindSplicePoint(offset, _KindSpecialWeakHandle);
    if (exists) {
        handle = ((ж<specialWeakHandle>)(uintptr)(new @unsafe.Pointer(iter.val))).handle;
    }
    unlock(Ꮡ((~span).speciallock));
    releasem(mp);
    // Keep p alive for the duration of the function to ensure
    // that it cannot die while we're trying to do this.
    KeepAlive(Δp);
    return handle;
}

// The described object is being heap profiled.
[GoType] partial struct specialprofile {
    internal runtime.@internal.sys_package.NotInHeap _;
    internal special special;
    internal ж<bucket> b;
}

// Set the heap profile bucket associated with addr to b.
internal static void setprofilebucket(@unsafe.Pointer Δp, ж<bucket> Ꮡb) {
    ref var b = ref Ꮡb.val;

    @lock(Ꮡmheap_.of(mheap.Ꮡspeciallock));
    var s = (ж<specialprofile>)(uintptr)(mheap_.specialprofilealloc.alloc());
    unlock(Ꮡmheap_.of(mheap.Ꮡspeciallock));
    (~s).special.kind = _KindSpecialProfile;
    s.val.b = b;
    if (!addspecial(p.val, Ꮡ((~s).special))) {
        @throw("setprofilebucket: profile already set"u8);
    }
}

// specialReachable tracks whether an object is reachable on the next
// GC cycle. This is used by testing.
[GoType] partial struct specialReachable {
    internal special special;
    internal bool done;
    internal bool reachable;
}

// specialPinCounter tracks whether an object is pinned multiple times.
[GoType] partial struct specialPinCounter {
    internal special special;
    internal uintptr counter;
}

// specialsIter helps iterate over specials lists.
[GoType] partial struct specialsIter {
    internal ж<ж<special>> pprev;
    internal ж<special> s;
}

internal static specialsIter newSpecialsIter(ж<mspan> Ꮡspan) {
    ref var span = ref Ꮡspan.val;

    return new specialsIter(Ꮡ(span.specials), span.specials);
}

[GoRecv] internal static bool valid(this ref specialsIter i) {
    return i.s != nil;
}

[GoRecv] internal static void next(this ref specialsIter i) {
    i.pprev = Ꮡ(i.s.next);
    i.s = i.pprev.val.val;
}

// unlinkAndNext removes the current special from the list and moves
// the iterator to the next special. It returns the unlinked special.
[GoRecv] internal static ж<special> unlinkAndNext(this ref specialsIter i) {
    var cur = i.s;
    i.s = cur.val.next;
    i.pprev.val.val = i.s;
    return cur;
}

// freeSpecial performs any cleanup on special s and deallocates it.
// s must already be unlinked from the specials list.
internal static void freeSpecial(ж<special> Ꮡs, @unsafe.Pointer Δp, uintptr size) {
    ref var s = ref Ꮡs.val;

    var exprᴛ1 = s.kind;
    if (exprᴛ1 == _KindSpecialFinalizer) {
        var sf = (ж<specialfinalizer>)(uintptr)(new @unsafe.Pointer(Ꮡs));
        queuefinalizer(p.val, (~sf).fn, (~sf).nret, (~sf).fint, (~sf).ot);
        @lock(Ꮡmheap_.of(mheap.Ꮡspeciallock));
        mheap_.specialfinalizeralloc.free(new @unsafe.Pointer(sf));
        unlock(Ꮡmheap_.of(mheap.Ꮡspeciallock));
    }
    else if (exprᴛ1 == _KindSpecialWeakHandle) {
        var sw = (ж<specialWeakHandle>)(uintptr)(new @unsafe.Pointer(Ꮡs));
        (~sw).handle.Store(0);
        @lock(Ꮡmheap_.of(mheap.Ꮡspeciallock));
        mheap_.specialWeakHandleAlloc.free(new @unsafe.Pointer(Ꮡs));
        unlock(Ꮡmheap_.of(mheap.Ꮡspeciallock));
    }
    else if (exprᴛ1 == _KindSpecialProfile) {
        var sp = (ж<specialprofile>)(uintptr)(new @unsafe.Pointer(Ꮡs));
        mProf_Free((~sp).b, size);
        @lock(Ꮡmheap_.of(mheap.Ꮡspeciallock));
        mheap_.specialprofilealloc.free(new @unsafe.Pointer(sp));
        unlock(Ꮡmheap_.of(mheap.Ꮡspeciallock));
    }
    else if (exprᴛ1 == _KindSpecialReachable) {
        var sp = (ж<specialReachable>)(uintptr)(new @unsafe.Pointer(Ꮡs));
        sp.val.done = true;
    }
    else if (exprᴛ1 == _KindSpecialPinCounter) {
        @lock(Ꮡmheap_.of(mheap.Ꮡspeciallock));
        mheap_.specialPinCounterAlloc.free(new @unsafe.Pointer(Ꮡs));
        unlock(Ꮡmheap_.of(mheap.Ꮡspeciallock));
    }
    else { /* default: */
        @throw("bad special kind"u8);
        throw panic("not reached");
    }

}

// The creator frees these.

// gcBits is an alloc/mark bitmap. This is always used as gcBits.x.
[GoType] partial struct gcBits {
    internal runtime.@internal.sys_package.NotInHeap _;
    internal uint8 x;
}

// bytep returns a pointer to the n'th byte of b.
[GoRecv] internal static ж<uint8> bytep(this ref gcBits b, uintptr n) {
    return addb(Ꮡ(b.x), n);
}

// bitp returns a pointer to the byte containing bit n and a mask for
// selecting that bit from *bytep.
[GoRecv] internal static (ж<uint8> bytep, uint8 mask) bitp(this ref gcBits b, uintptr n) {
    ж<uint8> bytep = default!;
    uint8 mask = default!;

    return (b.bytep(n / 8), 1 << (int)((n % 8)));
}

internal const uintptr gcBitsChunkBytes = /* uintptr(64 << 10) */ 65536;

internal const uintptr gcBitsHeaderBytes = /* unsafe.Sizeof(gcBitsHeader{}) */ 16;

[GoType] partial struct gcBitsHeader {
    internal uintptr free; // free is the index into bits of the next free byte.
    internal uintptr next; // *gcBits triggers recursive type bug. (issue 14620)
}

[GoType] partial struct gcBitsArena {
    internal runtime.@internal.sys_package.NotInHeap _;
    // gcBitsHeader // side step recursive type bug (issue 14620) by including fields by hand.
    internal uintptr free; // free is the index into bits of the next free byte; read/write atomically
    internal ж<gcBitsArena> next;
    internal array<gcBits> bits = new(gcBitsChunkBytes - gcBitsHeaderBytes);
}


[GoType("dyn")] partial struct gcBitsArenasᴛ1 {
    internal mutex @lock;
    internal ж<gcBitsArena> free;
    internal ж<gcBitsArena> next; // Read atomically. Write atomically under lock.
    internal ж<gcBitsArena> current;
    internal ж<gcBitsArena> previous;
}
internal static gcBitsArenasᴛ1 gcBitsArenas;

// tryAlloc allocates from b or returns nil if b does not have enough room.
// This is safe to call concurrently.
[GoRecv] internal static ж<gcBits> tryAlloc(this ref gcBitsArena b, uintptr bytes) {
    if (b == nil || atomic.Loaduintptr(Ꮡ(b.free)) + bytes > ((uintptr)len(b.bits))) {
        return default!;
    }
    // Try to allocate from this block.
    var end = atomic.Xadduintptr(Ꮡ(b.free), bytes);
    if (end > ((uintptr)len(b.bits))) {
        return default!;
    }
    // There was enough room.
    var start = end - bytes;
    return Ꮡ(b.bits[start]);
}

// newMarkBits returns a pointer to 8 byte aligned bytes
// to be used for a span's mark bits.
internal static ж<gcBits> newMarkBits(uintptr nelems) {
    var blocksNeeded = (nelems + 63) / 64;
    var bytesNeeded = blocksNeeded * 8;
    // Try directly allocating from the current head arena.
    var head = (ж<gcBitsArena>)(uintptr)(atomic.Loadp(((@unsafe.Pointer)(ᏑgcBitsArenas.of(gcBitsArenasᴛ1.Ꮡnext)))));
    {
        var pΔ1 = head.tryAlloc(bytesNeeded); if (pΔ1 != nil) {
            return pΔ1;
        }
    }
    // There's not enough room in the head arena. We may need to
    // allocate a new arena.
    @lock(ᏑgcBitsArenas.of(gcBitsArenasᴛ1.Ꮡlock));
    // Try the head arena again, since it may have changed. Now
    // that we hold the lock, the list head can't change, but its
    // free position still can.
    {
        var pΔ2 = gcBitsArenas.next.tryAlloc(bytesNeeded); if (pΔ2 != nil) {
            unlock(ᏑgcBitsArenas.of(gcBitsArenasᴛ1.Ꮡlock));
            return pΔ2;
        }
    }
    // Allocate a new arena. This may temporarily drop the lock.
    var fresh = newArenaMayUnlock();
    // If newArenaMayUnlock dropped the lock, another thread may
    // have put a fresh arena on the "next" list. Try allocating
    // from next again.
    {
        var pΔ3 = gcBitsArenas.next.tryAlloc(bytesNeeded); if (pΔ3 != nil) {
            // Put fresh back on the free list.
            // TODO: Mark it "already zeroed"
            fresh.val.next = gcBitsArenas.free;
            gcBitsArenas.free = fresh;
            unlock(ᏑgcBitsArenas.of(gcBitsArenasᴛ1.Ꮡlock));
            return pΔ3;
        }
    }
    // Allocate from the fresh arena. We haven't linked it in yet, so
    // this cannot race and is guaranteed to succeed.
    var Δp = fresh.tryAlloc(bytesNeeded);
    if (Δp == nil) {
        @throw("markBits overflow"u8);
    }
    // Add the fresh arena to the "next" list.
    fresh.val.next = gcBitsArenas.next;
    atomic.StorepNoWB(((@unsafe.Pointer)(ᏑgcBitsArenas.of(gcBitsArenasᴛ1.Ꮡnext))), new @unsafe.Pointer(fresh));
    unlock(ᏑgcBitsArenas.of(gcBitsArenasᴛ1.Ꮡlock));
    return Δp;
}

// newAllocBits returns a pointer to 8 byte aligned bytes
// to be used for this span's alloc bits.
// newAllocBits is used to provide newly initialized spans
// allocation bits. For spans not being initialized the
// mark bits are repurposed as allocation bits when
// the span is swept.
internal static ж<gcBits> newAllocBits(uintptr nelems) {
    return newMarkBits(nelems);
}

// nextMarkBitArenaEpoch establishes a new epoch for the arenas
// holding the mark bits. The arenas are named relative to the
// current GC cycle which is demarcated by the call to finishweep_m.
//
// All current spans have been swept.
// During that sweep each span allocated room for its gcmarkBits in
// gcBitsArenas.next block. gcBitsArenas.next becomes the gcBitsArenas.current
// where the GC will mark objects and after each span is swept these bits
// will be used to allocate objects.
// gcBitsArenas.current becomes gcBitsArenas.previous where the span's
// gcAllocBits live until all the spans have been swept during this GC cycle.
// The span's sweep extinguishes all the references to gcBitsArenas.previous
// by pointing gcAllocBits into the gcBitsArenas.current.
// The gcBitsArenas.previous is released to the gcBitsArenas.free list.
internal static void nextMarkBitArenaEpoch() {
    @lock(ᏑgcBitsArenas.of(gcBitsArenasᴛ1.Ꮡlock));
    if (gcBitsArenas.previous != nil) {
        if (gcBitsArenas.free == nil){
            gcBitsArenas.free = gcBitsArenas.previous;
        } else {
            // Find end of previous arenas.
            var last = gcBitsArenas.previous;
            for (last = gcBitsArenas.previous; (~last).next != nil; last = last.val.next) {
            }
            last.val.next = gcBitsArenas.free;
            gcBitsArenas.free = gcBitsArenas.previous;
        }
    }
    gcBitsArenas.previous = gcBitsArenas.current;
    gcBitsArenas.current = gcBitsArenas.next;
    atomic.StorepNoWB(((@unsafe.Pointer)(ᏑgcBitsArenas.of(gcBitsArenasᴛ1.Ꮡnext))), nil);
    // newMarkBits calls newArena when needed
    unlock(ᏑgcBitsArenas.of(gcBitsArenasᴛ1.Ꮡlock));
}

// newArenaMayUnlock allocates and zeroes a gcBits arena.
// The caller must hold gcBitsArena.lock. This may temporarily release it.
internal static ж<gcBitsArena> newArenaMayUnlock() {
    ж<gcBitsArena> result = default!;
    if (gcBitsArenas.free == nil){
        unlock(ᏑgcBitsArenas.of(gcBitsArenasᴛ1.Ꮡlock));
        result = (ж<gcBitsArena>)(uintptr)(sysAlloc(gcBitsChunkBytes, Ꮡmemstats.of(mstats.ᏑgcMiscSys)));
        if (result == nil) {
            @throw("runtime: cannot allocate memory"u8);
        }
        @lock(ᏑgcBitsArenas.of(gcBitsArenasᴛ1.Ꮡlock));
    } else {
        result = gcBitsArenas.free;
        gcBitsArenas.free = gcBitsArenas.free.val.next;
        memclrNoHeapPointers(new @unsafe.Pointer(result), gcBitsChunkBytes);
    }
    result.val.next = default!;
    // If result.bits is not 8 byte aligned adjust index so
    // that &result.bits[result.free] is 8 byte aligned.
    if ((uintptr)(@unsafe.Offsetof(new gcBitsArena(nil).GetType(), "bits") & 7) == 0){
        result.val.free = 0;
    } else {
        result.val.free = 8 - ((uintptr)(((uintptr)new @unsafe.Pointer(Ꮡ(~result).bits.at<gcBits>(0))) & 7));
    }
    return result;
}

} // end runtime_package
