// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Page heap.
//
// See malloc.go for overview.

// package runtime -- go2cs converted at 2020 October 09 04:46:59 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\mheap.go
using cpu = go.@internal.cpu_package;
using atomic = go.runtime.@internal.atomic_package;
using sys = go.runtime.@internal.sys_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class runtime_package
    {
 
        // minPhysPageSize is a lower-bound on the physical page size. The
        // true physical page size may be larger than this. In contrast,
        // sys.PhysPageSize is an upper-bound on the physical page size.
        private static readonly long minPhysPageSize = (long)4096L; 

        // maxPhysPageSize is the maximum page size the runtime supports.
        private static readonly long maxPhysPageSize = (long)512L << (int)(10L); 

        // maxPhysHugePageSize sets an upper-bound on the maximum huge page size
        // that the runtime supports.
        private static readonly var maxPhysHugePageSize = pallocChunkBytes; 

        // pagesPerReclaimerChunk indicates how many pages to scan from the
        // pageInUse bitmap at a time. Used by the page reclaimer.
        //
        // Higher values reduce contention on scanning indexes (such as
        // h.reclaimIndex), but increase the minimum latency of the
        // operation.
        //
        // The time required to scan this many pages can vary a lot depending
        // on how many spans are actually freed. Experimentally, it can
        // scan for pages at ~300 GB/ms on a 2.6GHz Core i7, but can only
        // free spans at ~32 MB/ms. Using 512 pages bounds this at
        // roughly 100µs.
        //
        // Must be a multiple of the pageInUse bitmap element size and
        // must also evenly divid pagesPerArena.
        private static readonly long pagesPerReclaimerChunk = (long)512L; 

        // go115NewMCentralImpl is a feature flag for the new mcentral implementation.
        //
        // This flag depends on go115NewMarkrootSpans because the new mcentral
        // implementation requires that markroot spans no longer rely on mgcsweepbufs.
        // The definition of this flag helps ensure that if there's a problem with
        // the new markroot spans implementation and it gets turned off, that the new
        // mcentral implementation also gets turned off so the runtime isn't broken.
        private static readonly var go115NewMCentralImpl = true && go115NewMarkrootSpans;


        // Main malloc heap.
        // The heap itself is the "free" and "scav" treaps,
        // but all the other global data is here too.
        //
        // mheap must not be heap-allocated because it contains mSpanLists,
        // which must not be heap-allocated.
        //
        //go:notinheap
        private partial struct mheap
        {
            public mutex @lock;
            public pageAlloc pages; // page allocation data structure
            public uint sweepgen; // sweep generation, see comment in mspan; written during STW
            public uint sweepdone; // all spans are swept
            public uint sweepers; // number of active sweepone calls

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
            public slice<ptr<mspan>> allspans; // all spans out there

// sweepSpans contains two mspan stacks: one of swept in-use
// spans, and one of unswept in-use spans. These two trade
// roles on each GC cycle. Since the sweepgen increases by 2
// on each cycle, this means the swept spans are in
// sweepSpans[sweepgen/2%2] and the unswept spans are in
// sweepSpans[1-sweepgen/2%2]. Sweeping pops spans from the
// unswept stack and pushes spans that are still in-use on the
// swept stack. Likewise, allocating an in-use span pushes it
// on the swept stack.
//
// For !go115NewMCentralImpl.
            public array<gcSweepBuf> sweepSpans;
            public uint _; // align uint64 fields on 32-bit for atomics

// Proportional sweep
//
// These parameters represent a linear function from heap_live
// to page sweep count. The proportional sweep system works to
// stay in the black by keeping the current page sweep count
// above this line at the current heap_live.
//
// The line has slope sweepPagesPerByte and passes through a
// basis point at (sweepHeapLiveBasis, pagesSweptBasis). At
// any given time, the system is at (memstats.heap_live,
// pagesSwept) in this space.
//
// It's important that the line pass through a point we
// control rather than simply starting at a (0,0) origin
// because that lets us adjust sweep pacing at any time while
// accounting for current progress. If we could only adjust
// the slope, it would create a discontinuity in debt if any
// progress has already been made.
            public ulong pagesInUse; // pages of spans in stats mSpanInUse; updated atomically
            public ulong pagesSwept; // pages swept this cycle; updated atomically
            public ulong pagesSweptBasis; // pagesSwept to use as the origin of the sweep ratio; updated atomically
            public ulong sweepHeapLiveBasis; // value of heap_live to use as the origin of sweep ratio; written with lock, read without
            public double sweepPagesPerByte; // proportional sweep ratio; written with lock, read without
// TODO(austin): pagesInUse should be a uintptr, but the 386
// compiler can't 8-byte align fields.

// scavengeGoal is the amount of total retained heap memory (measured by
// heapRetained) that the runtime will try to maintain by returning memory
// to the OS.
            public ulong scavengeGoal; // Page reclaimer state

// reclaimIndex is the page index in allArenas of next page to
// reclaim. Specifically, it refers to page (i %
// pagesPerArena) of arena allArenas[i / pagesPerArena].
//
// If this is >= 1<<63, the page reclaimer is done scanning
// the page marks.
//
// This is accessed atomically.
            public ulong reclaimIndex; // reclaimCredit is spare credit for extra pages swept. Since
// the page reclaimer works in large chunks, it may reclaim
// more than requested. Any spare pages released go to this
// credit pool.
//
// This is accessed atomically.
            public System.UIntPtr reclaimCredit; // Malloc stats.
            public ulong largealloc; // bytes allocated for large objects
            public ulong nlargealloc; // number of large object allocations
            public ulong largefree; // bytes freed for large objects (>maxsmallsize)
            public ulong nlargefree; // number of frees for large objects (>maxsmallsize)
            public array<ulong> nsmallfree; // number of frees for small objects (<=maxsmallsize)

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
            public array<ptr<array<ptr<heapArena>>>> arenas; // heapArenaAlloc is pre-reserved space for allocating heapArena
// objects. This is only used on 32-bit, where we pre-reserve
// this space to avoid interleaving it with the heap itself.
            public linearAlloc heapArenaAlloc; // arenaHints is a list of addresses at which to attempt to
// add more heap arenas. This is initially populated with a
// set of general hint addresses, and grown with the bounds of
// actual heap arena ranges.
            public ptr<arenaHint> arenaHints; // arena is a pre-reserved space for allocating heap arenas
// (the actual arenas). This is only used on 32-bit.
            public linearAlloc arena; // allArenas is the arenaIndex of every mapped arena. This can
// be used to iterate through the address space.
//
// Access is protected by mheap_.lock. However, since this is
// append-only and old backing arrays are never freed, it is
// safe to acquire mheap_.lock, copy the slice header, and
// then release mheap_.lock.
            public slice<arenaIdx> allArenas; // sweepArenas is a snapshot of allArenas taken at the
// beginning of the sweep cycle. This can be read safely by
// simply blocking GC (by disabling preemption).
            public slice<arenaIdx> sweepArenas; // markArenas is a snapshot of allArenas taken at the beginning
// of the mark cycle. Because allArenas is append-only, neither
// this slice nor its contents will change during the mark, so
// it can be read safely.
            public slice<arenaIdx> markArenas; // curArena is the arena that the heap is currently growing
// into. This should always be physPageSize-aligned.
            public fixalloc spanalloc; // allocator for span*
            public fixalloc cachealloc; // allocator for mcache*
            public fixalloc specialfinalizeralloc; // allocator for specialfinalizer*
            public fixalloc specialprofilealloc; // allocator for specialprofile*
            public mutex speciallock; // lock for special record allocators.
            public fixalloc arenaHintAlloc; // allocator for arenaHints

            public ptr<specialfinalizer> unused; // never set, just here to force the specialfinalizer type into DWARF
        }

        private static mheap mheap_ = default;

        // A heapArena stores metadata for a heap arena. heapArenas are stored
        // outside of the Go heap and accessed via the mheap_.arenas index.
        //
        //go:notinheap
        private partial struct heapArena
        {
            public array<byte> bitmap; // spans maps from virtual address page ID within this arena to *mspan.
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
            public array<ptr<mspan>> spans; // pageInUse is a bitmap that indicates which spans are in
// state mSpanInUse. This bitmap is indexed by page number,
// but only the bit corresponding to the first page in each
// span is used.
//
// Reads and writes are atomic.
            public array<byte> pageInUse; // pageMarks is a bitmap that indicates which spans have any
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
            public array<byte> pageMarks; // pageSpecials is a bitmap that indicates which spans have
// specials (finalizers or other). Like pageInUse, only the bit
// corresponding to the first page in each span is used.
//
// Writes are done atomically whenever a special is added to
// a span and whenever the last special is removed from a span.
// Reads are done atomically to find spans containing specials
// during marking.
            public array<byte> pageSpecials; // zeroedBase marks the first byte of the first page in this
// arena which hasn't been used yet and is therefore already
// zero. zeroedBase is relative to the arena base.
// Increases monotonically until it hits heapArenaBytes.
//
// This field is sufficient to determine if an allocation
// needs to be zeroed because the page allocator follows an
// address-ordered first-fit policy.
//
// Read atomically and written with an atomic CAS.
            public System.UIntPtr zeroedBase;
        }

        // arenaHint is a hint for where to grow the heap arenas. See
        // mheap_.arenaHints.
        //
        //go:notinheap
        private partial struct arenaHint
        {
            public System.UIntPtr addr;
            public bool down;
            public ptr<arenaHint> next;
        }

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

        // An mspan representing actual memory has state mSpanInUse,
        // mSpanManual, or mSpanFree. Transitions between these states are
        // constrained as follows:
        //
        // * A span may transition from free to in-use or manual during any GC
        //   phase.
        //
        // * During sweeping (gcphase == _GCoff), a span may transition from
        //   in-use to free (as a result of sweeping) or manual to free (as a
        //   result of stacks being freed).
        //
        // * During GC (gcphase != _GCoff), a span *must not* transition from
        //   manual or in-use to free. Because concurrent GC may read a pointer
        //   and then look up its span, the span state must be monotonic.
        //
        // Setting mspan.state to mSpanInUse or mSpanManual must be done
        // atomically and only after all other span fields are valid.
        // Likewise, if inspecting a span is contingent on it being
        // mSpanInUse, the state should be loaded atomically and checked
        // before depending on other fields. This allows the garbage collector
        // to safely deal with potentially invalid pointers, since resolving
        // such pointers may race with a span being allocated.
        private partial struct mSpanState // : byte
        {
        }

        private static readonly mSpanState mSpanDead = (mSpanState)iota;
        private static readonly var mSpanInUse = 0; // allocated for garbage collected heap
        private static readonly var mSpanManual = 1; // allocated for manual management (e.g., stack allocator)

        // mSpanStateNames are the names of the span states, indexed by
        // mSpanState.
        private static @string mSpanStateNames = new slice<@string>(new @string[] { "mSpanDead", "mSpanInUse", "mSpanManual", "mSpanFree" });

        // mSpanStateBox holds an mSpanState and provides atomic operations on
        // it. This is a separate type to disallow accidental comparison or
        // assignment with mSpanState.
        private partial struct mSpanStateBox
        {
            public mSpanState s;
        }

        private static void set(this ptr<mSpanStateBox> _addr_b, mSpanState s)
        {
            ref mSpanStateBox b = ref _addr_b.val;

            atomic.Store8((uint8.val)(_addr_b.s), uint8(s));
        }

        private static mSpanState get(this ptr<mSpanStateBox> _addr_b)
        {
            ref mSpanStateBox b = ref _addr_b.val;

            return mSpanState(atomic.Load8((uint8.val)(_addr_b.s)));
        }

        // mSpanList heads a linked list of spans.
        //
        //go:notinheap
        private partial struct mSpanList
        {
            public ptr<mspan> first; // first span in list, or nil if none
            public ptr<mspan> last; // last span in list, or nil if none
        }

        //go:notinheap
        private partial struct mspan
        {
            public ptr<mspan> next; // next span in list, or nil if none
            public ptr<mspan> prev; // previous span in list, or nil if none
            public ptr<mSpanList> list; // For debugging. TODO: Remove.

            public System.UIntPtr startAddr; // address of first byte of span aka s.base()
            public System.UIntPtr npages; // number of pages in span

            public gclinkptr manualFreeList; // list of free objects in mSpanManual spans

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
            public System.UIntPtr freeindex; // TODO: Look up nelems from sizeclass and remove this field if it
// helps performance.
            public System.UIntPtr nelems; // number of object in the span.

// Cache of the allocBits at freeindex. allocCache is shifted
// such that the lowest bit corresponds to the bit freeindex.
// allocCache holds the complement of allocBits, thus allowing
// ctz (count trailing zero) to use it directly.
// allocCache may contain bits beyond s.nelems; the caller must ignore
// these.
            public ulong allocCache; // allocBits and gcmarkBits hold pointers to a span's mark and
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
            public ptr<gcBits> allocBits;
            public ptr<gcBits> gcmarkBits; // sweep generation:
// if sweepgen == h->sweepgen - 2, the span needs sweeping
// if sweepgen == h->sweepgen - 1, the span is currently being swept
// if sweepgen == h->sweepgen, the span is swept and ready to use
// if sweepgen == h->sweepgen + 1, the span was cached before sweep began and is still cached, and needs sweeping
// if sweepgen == h->sweepgen + 3, the span was swept and then cached and is still cached
// h->sweepgen is incremented by 2 after every GC

            public uint sweepgen;
            public ushort divMul; // for divide by elemsize - divMagic.mul
            public ushort baseMask; // if non-0, elemsize is a power of 2, & this will get object allocation base
            public ushort allocCount; // number of allocated objects
            public spanClass spanclass; // size class and noscan (uint8)
            public mSpanStateBox state; // mSpanInUse etc; accessed atomically (get/set methods)
            public byte needzero; // needs to be zeroed before allocation
            public byte divShift; // for divide by elemsize - divMagic.shift
            public byte divShift2; // for divide by elemsize - divMagic.shift2
            public System.UIntPtr elemsize; // computed from sizeclass or from npages
            public System.UIntPtr limit; // end of data in span
            public mutex speciallock; // guards specials list
            public ptr<special> specials; // linked list of special records sorted by offset.
        }

        private static System.UIntPtr @base(this ptr<mspan> _addr_s)
        {
            ref mspan s = ref _addr_s.val;

            return s.startAddr;
        }

        private static (System.UIntPtr, System.UIntPtr, System.UIntPtr) layout(this ptr<mspan> _addr_s)
        {
            System.UIntPtr size = default;
            System.UIntPtr n = default;
            System.UIntPtr total = default;
            ref mspan s = ref _addr_s.val;

            total = s.npages << (int)(_PageShift);
            size = s.elemsize;
            if (size > 0L)
            {
                n = total / size;
            }

            return ;

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
        //go:nowritebarrierrec
        private static void recordspan(unsafe.Pointer vh, unsafe.Pointer p)
        {
            var h = (mheap.val)(vh);
            var s = (mspan.val)(p);
            if (len(h.allspans) >= cap(h.allspans))
            {
                long n = 64L * 1024L / sys.PtrSize;
                if (n < cap(h.allspans) * 3L / 2L)
                {
                    n = cap(h.allspans) * 3L / 2L;
                }

                ref slice<ptr<mspan>> @new = ref heap(out ptr<slice<ptr<mspan>>> _addr_@new);
                var sp = (slice.val)(@unsafe.Pointer(_addr_new));
                sp.array = sysAlloc(uintptr(n) * sys.PtrSize, _addr_memstats.other_sys);
                if (sp.array == null)
                {
                    throw("runtime: cannot allocate memory");
                }

                sp.len = len(h.allspans);
                sp.cap = n;
                if (len(h.allspans) > 0L)
                {
                    copy(new, h.allspans);
                }

                var oldAllspans = h.allspans * (notInHeapSlice.val)(@unsafe.Pointer(_addr_h.allspans));

                new ptr<ptr<ptr<notInHeapSlice>>>(@unsafe.Pointer(_addr_new));
                if (len(oldAllspans) != 0L)
                {
                    sysFree(@unsafe.Pointer(_addr_oldAllspans[0L]), uintptr(cap(oldAllspans)) * @unsafe.Sizeof(oldAllspans[0L]), _addr_memstats.other_sys);
                }

            }

            h.allspans = h.allspans[..len(h.allspans) + 1L];
            h.allspans[len(h.allspans) - 1L] = s;

        }

        // A spanClass represents the size class and noscan-ness of a span.
        //
        // Each size class has a noscan spanClass and a scan spanClass. The
        // noscan spanClass contains only noscan objects, which do not contain
        // pointers and thus do not need to be scanned by the garbage
        // collector.
        private partial struct spanClass // : byte
        {
        }

        private static readonly var numSpanClasses = _NumSizeClasses << (int)(1L);
        private static readonly var tinySpanClass = spanClass(tinySizeClass << (int)(1L) | 1L);


        private static spanClass makeSpanClass(byte sizeclass, bool noscan)
        {
            return spanClass(sizeclass << (int)(1L)) | spanClass(bool2int(noscan));
        }

        private static sbyte sizeclass(this spanClass sc)
        {
            return int8(sc >> (int)(1L));
        }

        private static bool noscan(this spanClass sc)
        {
            return sc & 1L != 0L;
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
        private static arenaIdx arenaIndex(System.UIntPtr p)
        {
            return arenaIdx((p - arenaBaseOffset) / heapArenaBytes);
        }

        // arenaBase returns the low address of the region covered by heap
        // arena i.
        private static System.UIntPtr arenaBase(arenaIdx i)
        {
            return uintptr(i) * heapArenaBytes + arenaBaseOffset;
        }

        private partial struct arenaIdx // : ulong
        {
        }

        private static ulong l1(this arenaIdx i)
        {
            if (arenaL1Bits == 0L)
            { 
                // Let the compiler optimize this away if there's no
                // L1 map.
                return 0L;

            }
            else
            {
                return uint(i) >> (int)(arenaL1Shift);
            }

        }

        private static ulong l2(this arenaIdx i)
        {
            if (arenaL1Bits == 0L)
            {
                return uint(i);
            }
            else
            {
                return uint(i) & (1L << (int)(arenaL2Bits) - 1L);
            }

        }

        // inheap reports whether b is a pointer into a (potentially dead) heap object.
        // It returns false for pointers into mSpanManual spans.
        // Non-preemptible because it is used by write barriers.
        //go:nowritebarrier
        //go:nosplit
        private static bool inheap(System.UIntPtr b)
        {
            return spanOfHeap(b) != null;
        }

        // inHeapOrStack is a variant of inheap that returns true for pointers
        // into any allocated heap span.
        //
        //go:nowritebarrier
        //go:nosplit
        private static bool inHeapOrStack(System.UIntPtr b)
        {
            var s = spanOf(b);
            if (s == null || b < s.@base())
            {
                return false;
            }


            if (s.state.get() == mSpanInUse || s.state.get() == mSpanManual) 
                return b < s.limit;
            else 
                return false;
            
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
        private static ptr<mspan> spanOf(System.UIntPtr p)
        { 
            // This function looks big, but we use a lot of constant
            // folding around arenaL1Bits to get it under the inlining
            // budget. Also, many of the checks here are safety checks
            // that Go needs to do anyway, so the generated code is quite
            // short.
            var ri = arenaIndex(p);
            if (arenaL1Bits == 0L)
            { 
                // If there's no L1, then ri.l1() can't be out of bounds but ri.l2() can.
                if (ri.l2() >= uint(len(mheap_.arenas[0L])))
                {
                    return _addr_null!;
                }

            }
            else
            { 
                // If there's an L1, then ri.l1() can be out of bounds but ri.l2() can't.
                if (ri.l1() >= uint(len(mheap_.arenas)))
                {
                    return _addr_null!;
                }

            }

            var l2 = mheap_.arenas[ri.l1()];
            if (arenaL1Bits != 0L && l2 == null)
            { // Should never happen if there's no L1.
                return _addr_null!;

            }

            var ha = l2[ri.l2()];
            if (ha == null)
            {
                return _addr_null!;
            }

            return _addr_ha.spans[(p / pageSize) % pagesPerArena]!;

        }

        // spanOfUnchecked is equivalent to spanOf, but the caller must ensure
        // that p points into an allocated heap arena.
        //
        // Must be nosplit because it has callers that are nosplit.
        //
        //go:nosplit
        private static ptr<mspan> spanOfUnchecked(System.UIntPtr p)
        {
            var ai = arenaIndex(p);
            return _addr_mheap_.arenas[ai.l1()][ai.l2()].spans[(p / pageSize) % pagesPerArena]!;
        }

        // spanOfHeap is like spanOf, but returns nil if p does not point to a
        // heap object.
        //
        // Must be nosplit because it has callers that are nosplit.
        //
        //go:nosplit
        private static ptr<mspan> spanOfHeap(System.UIntPtr p)
        {
            var s = spanOf(p); 
            // s is nil if it's never been allocated. Otherwise, we check
            // its state first because we don't trust this pointer, so we
            // have to synchronize with span initialization. Then, it's
            // still possible we picked up a stale span pointer, so we
            // have to check the span's bounds.
            if (s == null || s.state.get() != mSpanInUse || p < s.@base() || p >= s.limit)
            {
                return _addr_null!;
            }

            return _addr_s!;

        }

        // pageIndexOf returns the arena, page index, and page mask for pointer p.
        // The caller must ensure p is in the heap.
        private static (ptr<heapArena>, System.UIntPtr, byte) pageIndexOf(System.UIntPtr p)
        {
            ptr<heapArena> arena = default!;
            System.UIntPtr pageIdx = default;
            byte pageMask = default;

            var ai = arenaIndex(p);
            arena = mheap_.arenas[ai.l1()][ai.l2()];
            pageIdx = ((p / pageSize) / 8L) % uintptr(len(arena.pageInUse));
            pageMask = byte(1L << (int)(((p / pageSize) % 8L)));
            return ;
        }

        // Initialize the heap.
        private static void init(this ptr<mheap> _addr_h)
        {
            ref mheap h = ref _addr_h.val;

            lockInit(_addr_h.@lock, lockRankMheap);
            lockInit(_addr_h.sweepSpans[0L].spineLock, lockRankSpine);
            lockInit(_addr_h.sweepSpans[1L].spineLock, lockRankSpine);
            lockInit(_addr_h.speciallock, lockRankMheapSpecial);

            h.spanalloc.init(@unsafe.Sizeof(new mspan()), recordspan, @unsafe.Pointer(h), _addr_memstats.mspan_sys);
            h.cachealloc.init(@unsafe.Sizeof(new mcache()), null, null, _addr_memstats.mcache_sys);
            h.specialfinalizeralloc.init(@unsafe.Sizeof(new specialfinalizer()), null, null, _addr_memstats.other_sys);
            h.specialprofilealloc.init(@unsafe.Sizeof(new specialprofile()), null, null, _addr_memstats.other_sys);
            h.arenaHintAlloc.init(@unsafe.Sizeof(new arenaHint()), null, null, _addr_memstats.other_sys); 

            // Don't zero mspan allocations. Background sweeping can
            // inspect a span concurrently with allocating it, so it's
            // important that the span's sweepgen survive across freeing
            // and re-allocating a span to prevent background sweeping
            // from improperly cas'ing it from 0.
            //
            // This is safe because mspan contains no heap pointers.
            h.spanalloc.zero = false; 

            // h->mapcache needs no init

            foreach (var (i) in h.central)
            {
                h.central[i].mcentral.init(spanClass(i));
            }
            h.pages.init(_addr_h.@lock, _addr_memstats.gc_sys);

        }

        // reclaim sweeps and reclaims at least npage pages into the heap.
        // It is called before allocating npage pages to keep growth in check.
        //
        // reclaim implements the page-reclaimer half of the sweeper.
        //
        // h must NOT be locked.
        private static void reclaim(this ptr<mheap> _addr_h, System.UIntPtr npage)
        {
            ref mheap h = ref _addr_h.val;
 
            // TODO(austin): Half of the time spent freeing spans is in
            // locking/unlocking the heap (even with low contention). We
            // could make the slow path here several times faster by
            // batching heap frees.

            // Bail early if there's no more reclaim work.
            if (atomic.Load64(_addr_h.reclaimIndex) >= 1L << (int)(63L))
            {
                return ;
            } 

            // Disable preemption so the GC can't start while we're
            // sweeping, so we can read h.sweepArenas, and so
            // traceGCSweepStart/Done pair on the P.
            var mp = acquirem();

            if (trace.enabled)
            {
                traceGCSweepStart();
            }

            var arenas = h.sweepArenas;
            var locked = false;
            while (npage > 0L)
            { 
                // Pull from accumulated credit first.
                {
                    var credit = atomic.Loaduintptr(_addr_h.reclaimCredit);

                    if (credit > 0L)
                    {
                        var take = credit;
                        if (take > npage)
                        { 
                            // Take only what we need.
                            take = npage;

                        }

                        if (atomic.Casuintptr(_addr_h.reclaimCredit, credit, credit - take))
                        {
                            npage -= take;
                        }

                        continue;

                    } 

                    // Claim a chunk of work.

                } 

                // Claim a chunk of work.
                var idx = uintptr(atomic.Xadd64(_addr_h.reclaimIndex, pagesPerReclaimerChunk) - pagesPerReclaimerChunk);
                if (idx / pagesPerArena >= uintptr(len(arenas)))
                { 
                    // Page reclaiming is done.
                    atomic.Store64(_addr_h.reclaimIndex, 1L << (int)(63L));
                    break;

                }

                if (!locked)
                { 
                    // Lock the heap for reclaimChunk.
                    lock(_addr_h.@lock);
                    locked = true;

                } 

                // Scan this chunk.
                var nfound = h.reclaimChunk(arenas, idx, pagesPerReclaimerChunk);
                if (nfound <= npage)
                {
                    npage -= nfound;
                }
                else
                { 
                    // Put spare pages toward global credit.
                    atomic.Xadduintptr(_addr_h.reclaimCredit, nfound - npage);
                    npage = 0L;

                }

            }

            if (locked)
            {
                unlock(_addr_h.@lock);
            }

            if (trace.enabled)
            {
                traceGCSweepDone();
            }

            releasem(mp);

        }

        // reclaimChunk sweeps unmarked spans that start at page indexes [pageIdx, pageIdx+n).
        // It returns the number of pages returned to the heap.
        //
        // h.lock must be held and the caller must be non-preemptible. Note: h.lock may be
        // temporarily unlocked and re-locked in order to do sweeping or if tracing is
        // enabled.
        private static System.UIntPtr reclaimChunk(this ptr<mheap> _addr_h, slice<arenaIdx> arenas, System.UIntPtr pageIdx, System.UIntPtr n)
        {
            ref mheap h = ref _addr_h.val;
 
            // The heap lock must be held because this accesses the
            // heapArena.spans arrays using potentially non-live pointers.
            // In particular, if a span were freed and merged concurrently
            // with this probing heapArena.spans, it would be possible to
            // observe arbitrary, stale span pointers.
            var n0 = n;
            System.UIntPtr nFreed = default;
            var sg = h.sweepgen;
            while (n > 0L)
            {
                var ai = arenas[pageIdx / pagesPerArena];
                var ha = h.arenas[ai.l1()][ai.l2()]; 

                // Get a chunk of the bitmap to work on.
                var arenaPage = uint(pageIdx % pagesPerArena);
                var inUse = ha.pageInUse[arenaPage / 8L..];
                var marked = ha.pageMarks[arenaPage / 8L..];
                if (uintptr(len(inUse)) > n / 8L)
                {
                    inUse = inUse[..n / 8L];
                    marked = marked[..n / 8L];
                } 

                // Scan this bitmap chunk for spans that are in-use
                // but have no marked objects on them.
                foreach (var (i) in inUse)
                {
                    var inUseUnmarked = atomic.Load8(_addr_inUse[i]) & ~marked[i];
                    if (inUseUnmarked == 0L)
                    {
                        continue;
                    }

                    for (var j = uint(0L); j < 8L; j++)
                    {
                        if (inUseUnmarked & (1L << (int)(j)) != 0L)
                        {
                            var s = ha.spans[arenaPage + uint(i) * 8L + j];
                            if (atomic.Load(_addr_s.sweepgen) == sg - 2L && atomic.Cas(_addr_s.sweepgen, sg - 2L, sg - 1L))
                            {
                                var npages = s.npages;
                                unlock(_addr_h.@lock);
                                if (s.sweep(false))
                                {
                                    nFreed += npages;
                                }

                                lock(_addr_h.@lock); 
                                // Reload inUse. It's possible nearby
                                // spans were freed when we dropped the
                                // lock and we don't want to get stale
                                // pointers from the spans array.
                                inUseUnmarked = atomic.Load8(_addr_inUse[i]) & ~marked[i];

                            }

                        }

                    }


                } 

                // Advance.
                pageIdx += uintptr(len(inUse) * 8L);
                n -= uintptr(len(inUse) * 8L);

            }

            if (trace.enabled)
            {
                unlock(_addr_h.@lock); 
                // Account for pages scanned but not reclaimed.
                traceGCSweepSpan((n0 - nFreed) * pageSize);
                lock(_addr_h.@lock);

            }

            return nFreed;

        }

        // alloc allocates a new span of npage pages from the GC'd heap.
        //
        // spanclass indicates the span's size class and scannability.
        //
        // If needzero is true, the memory for the returned span will be zeroed.
        private static ptr<mspan> alloc(this ptr<mheap> _addr_h, System.UIntPtr npages, spanClass spanclass, bool needzero)
        {
            ref mheap h = ref _addr_h.val;
 
            // Don't do any operations that lock the heap on the G stack.
            // It might trigger stack growth, and the stack growth code needs
            // to be able to allocate heap.
            ptr<mspan> s;
            systemstack(() =>
            { 
                // To prevent excessive heap growth, before allocating n pages
                // we need to sweep and reclaim at least n pages.
                if (h.sweepdone == 0L)
                {
                    h.reclaim(npages);
                }

                s = h.allocSpan(npages, false, spanclass, _addr_memstats.heap_inuse);

            });

            if (s != null)
            {
                if (needzero && s.needzero != 0L)
                {
                    memclrNoHeapPointers(@unsafe.Pointer(s.@base()), s.npages << (int)(_PageShift));
                }

                s.needzero = 0L;

            }

            return _addr_s!;

        }

        // allocManual allocates a manually-managed span of npage pages.
        // allocManual returns nil if allocation fails.
        //
        // allocManual adds the bytes used to *stat, which should be a
        // memstats in-use field. Unlike allocations in the GC'd heap, the
        // allocation does *not* count toward heap_inuse or heap_sys.
        //
        // The memory backing the returned span may not be zeroed if
        // span.needzero is set.
        //
        // allocManual must be called on the system stack because it may
        // acquire the heap lock via allocSpan. See mheap for details.
        //
        //go:systemstack
        private static ptr<mspan> allocManual(this ptr<mheap> _addr_h, System.UIntPtr npages, ptr<ulong> _addr_stat)
        {
            ref mheap h = ref _addr_h.val;
            ref ulong stat = ref _addr_stat.val;

            return _addr_h.allocSpan(npages, true, 0L, stat)!;
        }

        // setSpans modifies the span map so [spanOf(base), spanOf(base+npage*pageSize))
        // is s.
        private static void setSpans(this ptr<mheap> _addr_h, System.UIntPtr @base, System.UIntPtr npage, ptr<mspan> _addr_s)
        {
            ref mheap h = ref _addr_h.val;
            ref mspan s = ref _addr_s.val;

            var p = base / pageSize;
            var ai = arenaIndex(base);
            var ha = h.arenas[ai.l1()][ai.l2()];
            for (var n = uintptr(0L); n < npage; n++)
            {
                var i = (p + n) % pagesPerArena;
                if (i == 0L)
                {
                    ai = arenaIndex(base + n * pageSize);
                    ha = h.arenas[ai.l1()][ai.l2()];
                }

                ha.spans[i] = s;

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
        private static bool allocNeedsZero(this ptr<mheap> _addr_h, System.UIntPtr @base, System.UIntPtr npage)
        {
            bool needZero = default;
            ref mheap h = ref _addr_h.val;

            while (npage > 0L)
            {
                var ai = arenaIndex(base);
                var ha = h.arenas[ai.l1()][ai.l2()];

                var zeroedBase = atomic.Loaduintptr(_addr_ha.zeroedBase);
                var arenaBase = base % heapArenaBytes;
                if (arenaBase < zeroedBase)
                { 
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
                if (arenaLimit > heapArenaBytes)
                {
                    arenaLimit = heapArenaBytes;
                } 
                // Increase ha.zeroedBase so it's >= arenaLimit.
                // We may be racing with other updates.
                while (arenaLimit > zeroedBase)
                {
                    if (atomic.Casuintptr(_addr_ha.zeroedBase, zeroedBase, arenaLimit))
                    {
                        break;
                    }

                    zeroedBase = atomic.Loaduintptr(_addr_ha.zeroedBase); 
                    // Sanity check zeroedBase.
                    if (zeroedBase <= arenaLimit && zeroedBase > arenaBase)
                    { 
                        // The zeroedBase moved into the space we were trying to
                        // claim. That's very bad, and indicates someone allocated
                        // the same region we did.
                        throw("potentially overlapping in-use allocations detected");

                    }

                } 

                // Move base forward and subtract from npage to move into
                // the next arena, or finish.
 

                // Move base forward and subtract from npage to move into
                // the next arena, or finish.
                base += arenaLimit - arenaBase;
                npage -= (arenaLimit - arenaBase) / pageSize;

            }

            return ;

        }

        // tryAllocMSpan attempts to allocate an mspan object from
        // the P-local cache, but may fail.
        //
        // h need not be locked.
        //
        // This caller must ensure that its P won't change underneath
        // it during this function. Currently to ensure that we enforce
        // that the function is run on the system stack, because that's
        // the only place it is used now. In the future, this requirement
        // may be relaxed if its use is necessary elsewhere.
        //
        //go:systemstack
        private static ptr<mspan> tryAllocMSpan(this ptr<mheap> _addr_h)
        {
            ref mheap h = ref _addr_h.val;

            var pp = getg().m.p.ptr(); 
            // If we don't have a p or the cache is empty, we can't do
            // anything here.
            if (pp == null || pp.mspancache.len == 0L)
            {
                return _addr_null!;
            } 
            // Pull off the last entry in the cache.
            var s = pp.mspancache.buf[pp.mspancache.len - 1L];
            pp.mspancache.len--;
            return _addr_s!;

        }

        // allocMSpanLocked allocates an mspan object.
        //
        // h must be locked.
        //
        // allocMSpanLocked must be called on the system stack because
        // its caller holds the heap lock. See mheap for details.
        // Running on the system stack also ensures that we won't
        // switch Ps during this function. See tryAllocMSpan for details.
        //
        //go:systemstack
        private static ptr<mspan> allocMSpanLocked(this ptr<mheap> _addr_h)
        {
            ref mheap h = ref _addr_h.val;

            var pp = getg().m.p.ptr();
            if (pp == null)
            { 
                // We don't have a p so just do the normal thing.
                return _addr_(mspan.val)(h.spanalloc.alloc())!;

            } 
            // Refill the cache if necessary.
            if (pp.mspancache.len == 0L)
            {
                const var refillCount = len(pp.mspancache.buf) / 2L;

                for (long i = 0L; i < refillCount; i++)
                {
                    pp.mspancache.buf[i] = (mspan.val)(h.spanalloc.alloc());
                }

                pp.mspancache.len = refillCount;

            } 
            // Pull off the last entry in the cache.
            var s = pp.mspancache.buf[pp.mspancache.len - 1L];
            pp.mspancache.len--;
            return _addr_s!;

        }

        // freeMSpanLocked free an mspan object.
        //
        // h must be locked.
        //
        // freeMSpanLocked must be called on the system stack because
        // its caller holds the heap lock. See mheap for details.
        // Running on the system stack also ensures that we won't
        // switch Ps during this function. See tryAllocMSpan for details.
        //
        //go:systemstack
        private static void freeMSpanLocked(this ptr<mheap> _addr_h, ptr<mspan> _addr_s)
        {
            ref mheap h = ref _addr_h.val;
            ref mspan s = ref _addr_s.val;

            var pp = getg().m.p.ptr(); 
            // First try to free the mspan directly to the cache.
            if (pp != null && pp.mspancache.len < len(pp.mspancache.buf))
            {
                pp.mspancache.buf[pp.mspancache.len] = s;
                pp.mspancache.len++;
                return ;
            } 
            // Failing that (or if we don't have a p), just free it to
            // the heap.
            h.spanalloc.free(@unsafe.Pointer(s));

        }

        // allocSpan allocates an mspan which owns npages worth of memory.
        //
        // If manual == false, allocSpan allocates a heap span of class spanclass
        // and updates heap accounting. If manual == true, allocSpan allocates a
        // manually-managed span (spanclass is ignored), and the caller is
        // responsible for any accounting related to its use of the span. Either
        // way, allocSpan will atomically add the bytes in the newly allocated
        // span to *sysStat.
        //
        // The returned span is fully initialized.
        //
        // h must not be locked.
        //
        // allocSpan must be called on the system stack both because it acquires
        // the heap lock and because it must block GC transitions.
        //
        //go:systemstack
        private static ptr<mspan> allocSpan(this ptr<mheap> _addr_h, System.UIntPtr npages, bool manual, spanClass spanclass, ptr<ulong> _addr_sysStat)
        {
            ptr<mspan> s = default!;
            ref mheap h = ref _addr_h.val;
            ref ulong sysStat = ref _addr_sysStat.val;
 
            // Function-global state.
            var gp = getg();
            var @base = uintptr(0L);
            var scav = uintptr(0L); 

            // If the allocation is small enough, try the page cache!
            var pp = gp.m.p.ptr();
            if (pp != null && npages < pageCachePages / 4L)
            {
                var c = _addr_pp.pcache; 

                // If the cache is empty, refill it.
                if (c.empty())
                {
                    lock(_addr_h.@lock);
                    c.val = h.pages.allocToCache();
                    unlock(_addr_h.@lock);
                } 

                // Try to allocate from the cache.
                base, scav = c.alloc(npages);
                if (base != 0L)
                {
                    s = h.tryAllocMSpan();

                    if (s != null && gcBlackenEnabled == 0L && (manual || spanclass.sizeclass() != 0L))
                    {
                        goto HaveSpan;
                    } 
                    // We're either running duing GC, failed to acquire a mspan,
                    // or the allocation is for a large object. This means we
                    // have to lock the heap and do a bunch of extra work,
                    // so go down the HaveBaseLocked path.
                    //
                    // We must do this during GC to avoid skew with heap_scan
                    // since we flush mcache stats whenever we lock.
                    //
                    // TODO(mknyszek): It would be nice to not have to
                    // lock the heap if it's a large allocation, but
                    // it's fine for now. The critical section here is
                    // short and large object allocations are relatively
                    // infrequent.
                }

            } 

            // For one reason or another, we couldn't get the
            // whole job done without the heap lock.
            lock(_addr_h.@lock);

            if (base == 0L)
            { 
                // Try to acquire a base address.
                base, scav = h.pages.alloc(npages);
                if (base == 0L)
                {
                    if (!h.grow(npages))
                    {
                        unlock(_addr_h.@lock);
                        return _addr_null!;
                    }

                    base, scav = h.pages.alloc(npages);
                    if (base == 0L)
                    {
                        throw("grew heap, but no adequate free space found");
                    }

                }

            }

            if (s == null)
            { 
                // We failed to get an mspan earlier, so grab
                // one now that we have the heap lock.
                s = h.allocMSpanLocked();

            }

            if (!manual)
            { 
                // This is a heap span, so we should do some additional accounting
                // which may only be done with the heap locked.

                // Transfer stats from mcache to global.
                c = ;
                if (gp.m.p != 0L)
                {
                    c = gp.m.p.ptr().mcache;
                }
                else
                { 
                    // This case occurs while bootstrapping.
                    // See the similar code in mallocgc.
                    c = mcache0;
                    if (c == null)
                    {
                        throw("mheap.allocSpan called with no P");
                    }

                }

                memstats.heap_scan += uint64(c.local_scan);
                c.local_scan = 0L;
                memstats.tinyallocs += uint64(c.local_tinyallocs);
                c.local_tinyallocs = 0L; 

                // Do some additional accounting if it's a large allocation.
                if (spanclass.sizeclass() == 0L)
                {
                    mheap_.largealloc += uint64(npages * pageSize);
                    mheap_.nlargealloc++;
                    atomic.Xadd64(_addr_memstats.heap_live, int64(npages * pageSize));
                } 

                // Either heap_live or heap_scan could have been updated.
                if (gcBlackenEnabled != 0L)
                {
                    gcController.revise();
                }

            }

            unlock(_addr_h.@lock);

HaveSpan:
            s.init(base, npages);
            if (h.allocNeedsZero(base, npages))
            {
                s.needzero = 1L;
            }

            var nbytes = npages * pageSize;
            if (manual)
            {
                s.manualFreeList = 0L;
                s.nelems = 0L;
                s.limit = s.@base() + s.npages * pageSize; 
                // Manually managed memory doesn't count toward heap_sys.
                mSysStatDec(_addr_memstats.heap_sys, s.npages * pageSize);
                s.state.set(mSpanManual);

            }
            else
            { 
                // We must set span properties before the span is published anywhere
                // since we're not holding the heap lock.
                s.spanclass = spanclass;
                {
                    var sizeclass = spanclass.sizeclass();

                    if (sizeclass == 0L)
                    {
                        s.elemsize = nbytes;
                        s.nelems = 1L;

                        s.divShift = 0L;
                        s.divMul = 0L;
                        s.divShift2 = 0L;
                        s.baseMask = 0L;
                    }
                    else
                    {
                        s.elemsize = uintptr(class_to_size[sizeclass]);
                        s.nelems = nbytes / s.elemsize;

                        var m = _addr_class_to_divmagic[sizeclass];
                        s.divShift = m.shift;
                        s.divMul = m.mul;
                        s.divShift2 = m.shift2;
                        s.baseMask = m.baseMask;
                    } 

                    // Initialize mark and allocation structures.

                } 

                // Initialize mark and allocation structures.
                s.freeindex = 0L;
                s.allocCache = ~uint64(0L); // all 1s indicating all free.
                s.gcmarkBits = newMarkBits(s.nelems);
                s.allocBits = newAllocBits(s.nelems); 

                // It's safe to access h.sweepgen without the heap lock because it's
                // only ever updated with the world stopped and we run on the
                // systemstack which blocks a STW transition.
                atomic.Store(_addr_s.sweepgen, h.sweepgen); 

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

            // Commit and account for any scavenged memory that the span now owns.
            if (scav != 0L)
            { 
                // sysUsed all the pages that are actually available
                // in the span since some of them might be scavenged.
                sysUsed(@unsafe.Pointer(base), nbytes);
                mSysStatDec(_addr_memstats.heap_released, scav);

            } 
            // Update stats.
            mSysStatInc(sysStat, nbytes);
            mSysStatDec(_addr_memstats.heap_idle, nbytes); 

            // Publish the span in various locations.

            // This is safe to call without the lock held because the slots
            // related to this span will only ever be read or modified by
            // this thread until pointers into the span are published (and
            // we execute a publication barrier at the end of this function
            // before that happens) or pageInUse is updated.
            h.setSpans(s.@base(), npages, s);

            if (!manual)
            {
                if (!go115NewMCentralImpl)
                { 
                    // Add to swept in-use list.
                    //
                    // This publishes the span to root marking.
                    //
                    // h.sweepgen is guaranteed to only change during STW,
                    // and preemption is disabled in the page allocator.
                    h.sweepSpans[h.sweepgen / 2L % 2L].push(s);

                } 

                // Mark in-use span in arena page bitmap.
                //
                // This publishes the span to the page sweeper, so
                // it's imperative that the span be completely initialized
                // prior to this line.
                var (arena, pageIdx, pageMask) = pageIndexOf(s.@base());
                atomic.Or8(_addr_arena.pageInUse[pageIdx], pageMask); 

                // Update related page sweeper stats.
                atomic.Xadd64(_addr_h.pagesInUse, int64(npages));

                if (trace.enabled)
                { 
                    // Trace that a heap alloc occurred.
                    traceHeapAlloc();

                }

            } 

            // Make sure the newly allocated span will be observed
            // by the GC before pointers into the span are published.
            publicationBarrier();

            return _addr_s!;

        }

        // Try to add at least npage pages of memory to the heap,
        // returning whether it worked.
        //
        // h must be locked.
        private static bool grow(this ptr<mheap> _addr_h, System.UIntPtr npage)
        {
            ref mheap h = ref _addr_h.val;
 
            // We must grow the heap in whole palloc chunks.
            var ask = alignUp(npage, pallocChunkPages) * pageSize;

            var totalGrowth = uintptr(0L); 
            // This may overflow because ask could be very large
            // and is otherwise unrelated to h.curArena.base.
            var end = h.curArena.@base + ask;
            var nBase = alignUp(end, physPageSize);
            if (nBase > h.curArena.end || end < h.curArena.@base)
            { 
                // Not enough room in the current arena. Allocate more
                // arena space. This may not be contiguous with the
                // current arena, so we have to request the full ask.
                var (av, asize) = h.sysAlloc(ask);
                if (av == null)
                {
                    print("runtime: out of memory: cannot allocate ", ask, "-byte block (", memstats.heap_sys, " in use)\n");
                    return false;
                }

                if (uintptr(av) == h.curArena.end)
                { 
                    // The new space is contiguous with the old
                    // space, so just extend the current space.
                    h.curArena.end = uintptr(av) + asize;

                }
                else
                { 
                    // The new space is discontiguous. Track what
                    // remains of the current space and switch to
                    // the new space. This should be rare.
                    {
                        var size = h.curArena.end - h.curArena.@base;

                        if (size != 0L)
                        {
                            h.pages.grow(h.curArena.@base, size);
                            totalGrowth += size;
                        } 
                        // Switch to the new space.

                    } 
                    // Switch to the new space.
                    h.curArena.@base = uintptr(av);
                    h.curArena.end = uintptr(av) + asize;

                } 

                // The memory just allocated counts as both released
                // and idle, even though it's not yet backed by spans.
                //
                // The allocation is always aligned to the heap arena
                // size which is always > physPageSize, so its safe to
                // just add directly to heap_released.
                mSysStatInc(_addr_memstats.heap_released, asize);
                mSysStatInc(_addr_memstats.heap_idle, asize); 

                // Recalculate nBase.
                // We know this won't overflow, because sysAlloc returned
                // a valid region starting at h.curArena.base which is at
                // least ask bytes in size.
                nBase = alignUp(h.curArena.@base + ask, physPageSize);

            } 

            // Grow into the current arena.
            var v = h.curArena.@base;
            h.curArena.@base = nBase;
            h.pages.grow(v, nBase - v);
            totalGrowth += nBase - v; 

            // We just caused a heap growth, so scavenge down what will soon be used.
            // By scavenging inline we deal with the failure to allocate out of
            // memory fragments by scavenging the memory fragments that are least
            // likely to be re-used.
            {
                var retained = heapRetained();

                if (retained + uint64(totalGrowth) > h.scavengeGoal)
                {
                    var todo = totalGrowth;
                    {
                        var overage = uintptr(retained + uint64(totalGrowth) - h.scavengeGoal);

                        if (todo > overage)
                        {
                            todo = overage;
                        }

                    }

                    h.pages.scavenge(todo, false);

                }

            }

            return true;

        }

        // Free the span back into the heap.
        private static void freeSpan(this ptr<mheap> _addr_h, ptr<mspan> _addr_s)
        {
            ref mheap h = ref _addr_h.val;
            ref mspan s = ref _addr_s.val;

            systemstack(() =>
            {
                var c = getg().m.p.ptr().mcache;
                lock(_addr_h.@lock);
                memstats.heap_scan += uint64(c.local_scan);
                c.local_scan = 0L;
                memstats.tinyallocs += uint64(c.local_tinyallocs);
                c.local_tinyallocs = 0L;
                if (msanenabled)
                { 
                    // Tell msan that this entire span is no longer in use.
                    var @base = @unsafe.Pointer(s.@base());
                    var bytes = s.npages << (int)(_PageShift);
                    msanfree(base, bytes);

                }

                if (gcBlackenEnabled != 0L)
                { 
                    // heap_scan changed.
                    gcController.revise();

                }

                h.freeSpanLocked(s, true, true);
                unlock(_addr_h.@lock);

            });

        }

        // freeManual frees a manually-managed span returned by allocManual.
        // stat must be the same as the stat passed to the allocManual that
        // allocated s.
        //
        // This must only be called when gcphase == _GCoff. See mSpanState for
        // an explanation.
        //
        // freeManual must be called on the system stack because it acquires
        // the heap lock. See mheap for details.
        //
        //go:systemstack
        private static void freeManual(this ptr<mheap> _addr_h, ptr<mspan> _addr_s, ptr<ulong> _addr_stat)
        {
            ref mheap h = ref _addr_h.val;
            ref mspan s = ref _addr_s.val;
            ref ulong stat = ref _addr_stat.val;

            s.needzero = 1L;
            lock(_addr_h.@lock);
            mSysStatDec(stat, s.npages * pageSize);
            mSysStatInc(_addr_memstats.heap_sys, s.npages * pageSize);
            h.freeSpanLocked(s, false, true);
            unlock(_addr_h.@lock);
        }

        private static void freeSpanLocked(this ptr<mheap> _addr_h, ptr<mspan> _addr_s, bool acctinuse, bool acctidle)
        {
            ref mheap h = ref _addr_h.val;
            ref mspan s = ref _addr_s.val;


            if (s.state.get() == mSpanManual) 
                if (s.allocCount != 0L)
                {
                    throw("mheap.freeSpanLocked - invalid stack free");
                }

            else if (s.state.get() == mSpanInUse) 
                if (s.allocCount != 0L || s.sweepgen != h.sweepgen)
                {
                    print("mheap.freeSpanLocked - span ", s, " ptr ", hex(s.@base()), " allocCount ", s.allocCount, " sweepgen ", s.sweepgen, "/", h.sweepgen, "\n");
                    throw("mheap.freeSpanLocked - invalid free");
                }

                atomic.Xadd64(_addr_h.pagesInUse, -int64(s.npages)); 

                // Clear in-use bit in arena page bitmap.
                var (arena, pageIdx, pageMask) = pageIndexOf(s.@base());
                atomic.And8(_addr_arena.pageInUse[pageIdx], ~pageMask);
            else 
                throw("mheap.freeSpanLocked - invalid span state");
                        if (acctinuse)
            {
                mSysStatDec(_addr_memstats.heap_inuse, s.npages * pageSize);
            }

            if (acctidle)
            {
                mSysStatInc(_addr_memstats.heap_idle, s.npages * pageSize);
            } 

            // Mark the space as free.
            h.pages.free(s.@base(), s.npages); 

            // Free the span structure. We no longer have a use for it.
            s.state.set(mSpanDead);
            h.freeMSpanLocked(s);

        }

        // scavengeAll acquires the heap lock (blocking any additional
        // manipulation of the page allocator) and iterates over the whole
        // heap, scavenging every free page available.
        private static void scavengeAll(this ptr<mheap> _addr_h)
        {
            ref mheap h = ref _addr_h.val;
 
            // Disallow malloc or panic while holding the heap lock. We do
            // this here because this is a non-mallocgc entry-point to
            // the mheap API.
            var gp = getg();
            gp.m.mallocing++;
            lock(_addr_h.@lock); 
            // Start a new scavenge generation so we have a chance to walk
            // over the whole heap.
            h.pages.scavengeStartGen();
            var released = h.pages.scavenge(~uintptr(0L), false);
            var gen = h.pages.scav.gen;
            unlock(_addr_h.@lock);
            gp.m.mallocing--;

            if (debug.scavtrace > 0L)
            {
                printScavTrace(gen, released, true);
            }

        }

        //go:linkname runtime_debug_freeOSMemory runtime/debug.freeOSMemory
        private static void runtime_debug_freeOSMemory()
        {
            GC();
            systemstack(() =>
            {
                mheap_.scavengeAll();
            });

        }

        // Initialize a new span with the given start and npages.
        private static void init(this ptr<mspan> _addr_span, System.UIntPtr @base, System.UIntPtr npages)
        {
            ref mspan span = ref _addr_span.val;
 
            // span is *not* zeroed.
            span.next = null;
            span.prev = null;
            span.list = null;
            span.startAddr = base;
            span.npages = npages;
            span.allocCount = 0L;
            span.spanclass = 0L;
            span.elemsize = 0L;
            span.speciallock.key = 0L;
            span.specials = null;
            span.needzero = 0L;
            span.freeindex = 0L;
            span.allocBits = null;
            span.gcmarkBits = null;
            span.state.set(mSpanDead);
            lockInit(_addr_span.speciallock, lockRankMspanSpecial);

        }

        private static bool inList(this ptr<mspan> _addr_span)
        {
            ref mspan span = ref _addr_span.val;

            return span.list != null;
        }

        // Initialize an empty doubly-linked list.
        private static void init(this ptr<mSpanList> _addr_list)
        {
            ref mSpanList list = ref _addr_list.val;

            list.first = null;
            list.last = null;
        }

        private static void remove(this ptr<mSpanList> _addr_list, ptr<mspan> _addr_span)
        {
            ref mSpanList list = ref _addr_list.val;
            ref mspan span = ref _addr_span.val;

            if (span.list != list)
            {
                print("runtime: failed mSpanList.remove span.npages=", span.npages, " span=", span, " prev=", span.prev, " span.list=", span.list, " list=", list, "\n");
                throw("mSpanList.remove");
            }

            if (list.first == span)
            {
                list.first = span.next;
            }
            else
            {
                span.prev.next = span.next;
            }

            if (list.last == span)
            {
                list.last = span.prev;
            }
            else
            {
                span.next.prev = span.prev;
            }

            span.next = null;
            span.prev = null;
            span.list = null;

        }

        private static bool isEmpty(this ptr<mSpanList> _addr_list)
        {
            ref mSpanList list = ref _addr_list.val;

            return list.first == null;
        }

        private static void insert(this ptr<mSpanList> _addr_list, ptr<mspan> _addr_span)
        {
            ref mSpanList list = ref _addr_list.val;
            ref mspan span = ref _addr_span.val;

            if (span.next != null || span.prev != null || span.list != null)
            {
                println("runtime: failed mSpanList.insert", span, span.next, span.prev, span.list);
                throw("mSpanList.insert");
            }

            span.next = list.first;
            if (list.first != null)
            { 
                // The list contains at least one span; link it in.
                // The last span in the list doesn't change.
                list.first.prev = span;

            }
            else
            { 
                // The list contains no spans, so this is also the last span.
                list.last = span;

            }

            list.first = span;
            span.list = list;

        }

        private static void insertBack(this ptr<mSpanList> _addr_list, ptr<mspan> _addr_span)
        {
            ref mSpanList list = ref _addr_list.val;
            ref mspan span = ref _addr_span.val;

            if (span.next != null || span.prev != null || span.list != null)
            {
                println("runtime: failed mSpanList.insertBack", span, span.next, span.prev, span.list);
                throw("mSpanList.insertBack");
            }

            span.prev = list.last;
            if (list.last != null)
            { 
                // The list contains at least one span.
                list.last.next = span;

            }
            else
            { 
                // The list contains no spans, so this is also the first span.
                list.first = span;

            }

            list.last = span;
            span.list = list;

        }

        // takeAll removes all spans from other and inserts them at the front
        // of list.
        private static void takeAll(this ptr<mSpanList> _addr_list, ptr<mSpanList> _addr_other)
        {
            ref mSpanList list = ref _addr_list.val;
            ref mSpanList other = ref _addr_other.val;

            if (other.isEmpty())
            {
                return ;
            } 

            // Reparent everything in other to list.
            {
                var s = other.first;

                while (s != null)
                {
                    s.list = list;
                    s = s.next;
                } 

                // Concatenate the lists.

            } 

            // Concatenate the lists.
            if (list.isEmpty())
            {
                list.val = other;
            }
            else
            { 
                // Neither list is empty. Put other before list.
                other.last.next = list.first;
                list.first.prev = other.last;
                list.first = other.first;

            }

            other.first = null;
            other.last = null;

        }

        private static readonly long _KindSpecialFinalizer = (long)1L;
        private static readonly long _KindSpecialProfile = (long)2L; 
        // Note: The finalizer special must be first because if we're freeing
        // an object, a finalizer special will cause the freeing operation
        // to abort, and we want to keep the other special records around
        // if that happens.

        //go:notinheap
        private partial struct special
        {
            public ptr<special> next; // linked list in span
            public ushort offset; // span offset of object
            public byte kind; // kind of special
        }

        // spanHasSpecials marks a span as having specials in the arena bitmap.
        private static void spanHasSpecials(ptr<mspan> _addr_s)
        {
            ref mspan s = ref _addr_s.val;

            var arenaPage = (s.@base() / pageSize) % pagesPerArena;
            var ai = arenaIndex(s.@base());
            var ha = mheap_.arenas[ai.l1()][ai.l2()];
            atomic.Or8(_addr_ha.pageSpecials[arenaPage / 8L], uint8(1L) << (int)((arenaPage % 8L)));
        }

        // spanHasNoSpecials marks a span as having no specials in the arena bitmap.
        private static void spanHasNoSpecials(ptr<mspan> _addr_s)
        {
            ref mspan s = ref _addr_s.val;

            var arenaPage = (s.@base() / pageSize) % pagesPerArena;
            var ai = arenaIndex(s.@base());
            var ha = mheap_.arenas[ai.l1()][ai.l2()];
            atomic.And8(_addr_ha.pageSpecials[arenaPage / 8L], ~(uint8(1L) << (int)((arenaPage % 8L))));
        }

        // Adds the special record s to the list of special records for
        // the object p. All fields of s should be filled in except for
        // offset & next, which this routine will fill in.
        // Returns true if the special was successfully added, false otherwise.
        // (The add will fail only if a record with the same p and s->kind
        //  already exists.)
        private static bool addspecial(unsafe.Pointer p, ptr<special> _addr_s)
        {
            ref special s = ref _addr_s.val;

            var span = spanOfHeap(uintptr(p));
            if (span == null)
            {
                throw("addspecial on invalid pointer");
            } 

            // Ensure that the span is swept.
            // Sweeping accesses the specials list w/o locks, so we have
            // to synchronize with it. And it's just much safer.
            var mp = acquirem();
            span.ensureSwept();

            var offset = uintptr(p) - span.@base();
            var kind = s.kind;

            lock(_addr_span.speciallock); 

            // Find splice point, check for existing record.
            var t = _addr_span.specials;
            while (true)
            {
                var x = t.val;
                if (x == null)
                {
                    break;
                }

                if (offset == uintptr(x.offset) && kind == x.kind)
                {
                    unlock(_addr_span.speciallock);
                    releasem(mp);
                    return false; // already exists
                }

                if (offset < uintptr(x.offset) || (offset == uintptr(x.offset) && kind < x.kind))
                {
                    break;
                }

                t = _addr_x.next;

            } 

            // Splice in record, fill in offset.
 

            // Splice in record, fill in offset.
            s.offset = uint16(offset);
            s.next = t.val;
            t.val = s;
            if (go115NewMarkrootSpans)
            {
                spanHasSpecials(_addr_span);
            }

            unlock(_addr_span.speciallock);
            releasem(mp);

            return true;

        }

        // Removes the Special record of the given kind for the object p.
        // Returns the record if the record existed, nil otherwise.
        // The caller must FixAlloc_Free the result.
        private static ptr<special> removespecial(unsafe.Pointer p, byte kind)
        {
            var span = spanOfHeap(uintptr(p));
            if (span == null)
            {
                throw("removespecial on invalid pointer");
            } 

            // Ensure that the span is swept.
            // Sweeping accesses the specials list w/o locks, so we have
            // to synchronize with it. And it's just much safer.
            var mp = acquirem();
            span.ensureSwept();

            var offset = uintptr(p) - span.@base();

            ptr<special> result;
            lock(_addr_span.speciallock);
            var t = _addr_span.specials;
            while (true)
            {
                var s = t.val;
                if (s == null)
                {
                    break;
                } 
                // This function is used for finalizers only, so we don't check for
                // "interior" specials (p must be exactly equal to s->offset).
                if (offset == uintptr(s.offset) && kind == s.kind)
                {
                    t.val = s.next;
                    result = s;
                    break;
                }

                t = _addr_s.next;

            }

            if (go115NewMarkrootSpans && span.specials == null)
            {
                spanHasNoSpecials(_addr_span);
            }

            unlock(_addr_span.speciallock);
            releasem(mp);
            return _addr_result!;

        }

        // The described object has a finalizer set for it.
        //
        // specialfinalizer is allocated from non-GC'd memory, so any heap
        // pointers must be specially handled.
        //
        //go:notinheap
        private partial struct specialfinalizer
        {
            public special special;
            public ptr<funcval> fn; // May be a heap pointer.
            public System.UIntPtr nret;
            public ptr<_type> fint; // May be a heap pointer, but always live.
            public ptr<ptrtype> ot; // May be a heap pointer, but always live.
        }

        // Adds a finalizer to the object p. Returns true if it succeeded.
        private static bool addfinalizer(unsafe.Pointer p, ptr<funcval> _addr_f, System.UIntPtr nret, ptr<_type> _addr_fint, ptr<ptrtype> _addr_ot)
        {
            ref funcval f = ref _addr_f.val;
            ref _type fint = ref _addr_fint.val;
            ref ptrtype ot = ref _addr_ot.val;

            lock(_addr_mheap_.speciallock);
            var s = (specialfinalizer.val)(mheap_.specialfinalizeralloc.alloc());
            unlock(_addr_mheap_.speciallock);
            s.special.kind = _KindSpecialFinalizer;
            s.fn = f;
            s.nret = nret;
            s.fint = fint;
            s.ot = ot;
            if (addspecial(p, _addr_s.special))
            { 
                // This is responsible for maintaining the same
                // GC-related invariants as markrootSpans in any
                // situation where it's possible that markrootSpans
                // has already run but mark termination hasn't yet.
                if (gcphase != _GCoff)
                {
                    var (base, _, _) = findObject(uintptr(p), 0L, 0L);
                    var mp = acquirem();
                    var gcw = _addr_mp.p.ptr().gcw; 
                    // Mark everything reachable from the object
                    // so it's retained for the finalizer.
                    scanobject(base, gcw); 
                    // Mark the finalizer itself, since the
                    // special isn't part of the GC'd heap.
                    scanblock(uintptr(@unsafe.Pointer(_addr_s.fn)), sys.PtrSize, _addr_oneptrmask[0L], gcw, null);
                    releasem(mp);

                }

                return true;

            } 

            // There was an old finalizer
            lock(_addr_mheap_.speciallock);
            mheap_.specialfinalizeralloc.free(@unsafe.Pointer(s));
            unlock(_addr_mheap_.speciallock);
            return false;

        }

        // Removes the finalizer (if any) from the object p.
        private static void removefinalizer(unsafe.Pointer p)
        {
            var s = (specialfinalizer.val)(@unsafe.Pointer(removespecial(p, _KindSpecialFinalizer)));
            if (s == null)
            {
                return ; // there wasn't a finalizer to remove
            }

            lock(_addr_mheap_.speciallock);
            mheap_.specialfinalizeralloc.free(@unsafe.Pointer(s));
            unlock(_addr_mheap_.speciallock);

        }

        // The described object is being heap profiled.
        //
        //go:notinheap
        private partial struct specialprofile
        {
            public special special;
            public ptr<bucket> b;
        }

        // Set the heap profile bucket associated with addr to b.
        private static void setprofilebucket(unsafe.Pointer p, ptr<bucket> _addr_b)
        {
            ref bucket b = ref _addr_b.val;

            lock(_addr_mheap_.speciallock);
            var s = (specialprofile.val)(mheap_.specialprofilealloc.alloc());
            unlock(_addr_mheap_.speciallock);
            s.special.kind = _KindSpecialProfile;
            s.b = b;
            if (!addspecial(p, _addr_s.special))
            {
                throw("setprofilebucket: profile already set");
            }

        }

        // Do whatever cleanup needs to be done to deallocate s. It has
        // already been unlinked from the mspan specials list.
        private static void freespecial(ptr<special> _addr_s, unsafe.Pointer p, System.UIntPtr size) => func((_, panic, __) =>
        {
            ref special s = ref _addr_s.val;


            if (s.kind == _KindSpecialFinalizer) 
                var sf = (specialfinalizer.val)(@unsafe.Pointer(s));
                queuefinalizer(p, sf.fn, sf.nret, sf.fint, sf.ot);
                lock(_addr_mheap_.speciallock);
                mheap_.specialfinalizeralloc.free(@unsafe.Pointer(sf));
                unlock(_addr_mheap_.speciallock);
            else if (s.kind == _KindSpecialProfile) 
                var sp = (specialprofile.val)(@unsafe.Pointer(s));
                mProf_Free(sp.b, size);
                lock(_addr_mheap_.speciallock);
                mheap_.specialprofilealloc.free(@unsafe.Pointer(sp));
                unlock(_addr_mheap_.speciallock);
            else 
                throw("bad special kind");
                panic("not reached");
            
        });

        // gcBits is an alloc/mark bitmap. This is always used as *gcBits.
        //
        //go:notinheap
        private partial struct gcBits // : byte
        {
        }

        // bytep returns a pointer to the n'th byte of b.
        private static ptr<byte> bytep(this ptr<gcBits> _addr_b, System.UIntPtr n)
        {
            ref gcBits b = ref _addr_b.val;

            return _addr_addb((uint8.val)(b), n)!;
        }

        // bitp returns a pointer to the byte containing bit n and a mask for
        // selecting that bit from *bytep.
        private static (ptr<byte>, byte) bitp(this ptr<gcBits> _addr_b, System.UIntPtr n)
        {
            ptr<byte> bytep = default!;
            byte mask = default;
            ref gcBits b = ref _addr_b.val;

            return (_addr_b.bytep(n / 8L)!, 1L << (int)((n % 8L)));
        }

        private static readonly var gcBitsChunkBytes = uintptr(64L << (int)(10L));

        private static readonly var gcBitsHeaderBytes = @unsafe.Sizeof(new gcBitsHeader());



        private partial struct gcBitsHeader
        {
            public System.UIntPtr free; // free is the index into bits of the next free byte.
            public System.UIntPtr next; // *gcBits triggers recursive type bug. (issue 14620)
        }

        //go:notinheap
        private partial struct gcBitsArena
        {
            public System.UIntPtr free; // free is the index into bits of the next free byte; read/write atomically
            public ptr<gcBitsArena> next;
            public array<gcBits> bits;
        }

        private static var gcBitsArenas = default;

        // tryAlloc allocates from b or returns nil if b does not have enough room.
        // This is safe to call concurrently.
        private static ptr<gcBits> tryAlloc(this ptr<gcBitsArena> _addr_b, System.UIntPtr bytes)
        {
            ref gcBitsArena b = ref _addr_b.val;

            if (b == null || atomic.Loaduintptr(_addr_b.free) + bytes > uintptr(len(b.bits)))
            {
                return _addr_null!;
            } 
            // Try to allocate from this block.
            var end = atomic.Xadduintptr(_addr_b.free, bytes);
            if (end > uintptr(len(b.bits)))
            {
                return _addr_null!;
            } 
            // There was enough room.
            var start = end - bytes;
            return _addr__addr_b.bits[start]!;

        }

        // newMarkBits returns a pointer to 8 byte aligned bytes
        // to be used for a span's mark bits.
        private static ptr<gcBits> newMarkBits(System.UIntPtr nelems)
        {
            var blocksNeeded = uintptr((nelems + 63L) / 64L);
            var bytesNeeded = blocksNeeded * 8L; 

            // Try directly allocating from the current head arena.
            var head = (gcBitsArena.val)(atomic.Loadp(@unsafe.Pointer(_addr_gcBitsArenas.next)));
            {
                var p__prev1 = p;

                var p = head.tryAlloc(bytesNeeded);

                if (p != null)
                {
                    return _addr_p!;
                } 

                // There's not enough room in the head arena. We may need to
                // allocate a new arena.

                p = p__prev1;

            } 

            // There's not enough room in the head arena. We may need to
            // allocate a new arena.
            lock(_addr_gcBitsArenas.@lock); 
            // Try the head arena again, since it may have changed. Now
            // that we hold the lock, the list head can't change, but its
            // free position still can.
            {
                var p__prev1 = p;

                p = gcBitsArenas.next.tryAlloc(bytesNeeded);

                if (p != null)
                {
                    unlock(_addr_gcBitsArenas.@lock);
                    return _addr_p!;
                } 

                // Allocate a new arena. This may temporarily drop the lock.

                p = p__prev1;

            } 

            // Allocate a new arena. This may temporarily drop the lock.
            var fresh = newArenaMayUnlock(); 
            // If newArenaMayUnlock dropped the lock, another thread may
            // have put a fresh arena on the "next" list. Try allocating
            // from next again.
            {
                var p__prev1 = p;

                p = gcBitsArenas.next.tryAlloc(bytesNeeded);

                if (p != null)
                { 
                    // Put fresh back on the free list.
                    // TODO: Mark it "already zeroed"
                    fresh.next = gcBitsArenas.free;
                    gcBitsArenas.free = fresh;
                    unlock(_addr_gcBitsArenas.@lock);
                    return _addr_p!;

                } 

                // Allocate from the fresh arena. We haven't linked it in yet, so
                // this cannot race and is guaranteed to succeed.

                p = p__prev1;

            } 

            // Allocate from the fresh arena. We haven't linked it in yet, so
            // this cannot race and is guaranteed to succeed.
            p = fresh.tryAlloc(bytesNeeded);
            if (p == null)
            {
                throw("markBits overflow");
            } 

            // Add the fresh arena to the "next" list.
            fresh.next = gcBitsArenas.next;
            atomic.StorepNoWB(@unsafe.Pointer(_addr_gcBitsArenas.next), @unsafe.Pointer(fresh));

            unlock(_addr_gcBitsArenas.@lock);
            return _addr_p!;

        }

        // newAllocBits returns a pointer to 8 byte aligned bytes
        // to be used for this span's alloc bits.
        // newAllocBits is used to provide newly initialized spans
        // allocation bits. For spans not being initialized the
        // mark bits are repurposed as allocation bits when
        // the span is swept.
        private static ptr<gcBits> newAllocBits(System.UIntPtr nelems)
        {
            return _addr_newMarkBits(nelems)!;
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
        private static void nextMarkBitArenaEpoch()
        {
            lock(_addr_gcBitsArenas.@lock);
            if (gcBitsArenas.previous != null)
            {
                if (gcBitsArenas.free == null)
                {
                    gcBitsArenas.free = gcBitsArenas.previous;
                }
                else
                { 
                    // Find end of previous arenas.
                    var last = gcBitsArenas.previous;
                    last = gcBitsArenas.previous;

                    while (last.next != null)
                    {
                        last = last.next;
                    }

                    last.next = gcBitsArenas.free;
                    gcBitsArenas.free = gcBitsArenas.previous;

                }

            }

            gcBitsArenas.previous = gcBitsArenas.current;
            gcBitsArenas.current = gcBitsArenas.next;
            atomic.StorepNoWB(@unsafe.Pointer(_addr_gcBitsArenas.next), null); // newMarkBits calls newArena when needed
            unlock(_addr_gcBitsArenas.@lock);

        }

        // newArenaMayUnlock allocates and zeroes a gcBits arena.
        // The caller must hold gcBitsArena.lock. This may temporarily release it.
        private static ptr<gcBitsArena> newArenaMayUnlock()
        {
            ptr<gcBitsArena> result;
            if (gcBitsArenas.free == null)
            {
                unlock(_addr_gcBitsArenas.@lock);
                result = (gcBitsArena.val)(sysAlloc(gcBitsChunkBytes, _addr_memstats.gc_sys));
                if (result == null)
                {
                    throw("runtime: cannot allocate memory");
                }

                lock(_addr_gcBitsArenas.@lock);

            }
            else
            {
                result = gcBitsArenas.free;
                gcBitsArenas.free = gcBitsArenas.free.next;
                memclrNoHeapPointers(@unsafe.Pointer(result), gcBitsChunkBytes);
            }

            result.next = null; 
            // If result.bits is not 8 byte aligned adjust index so
            // that &result.bits[result.free] is 8 byte aligned.
            if (uintptr(@unsafe.Offsetof(new gcBitsArena().bits)) & 7L == 0L)
            {
                result.free = 0L;
            }
            else
            {
                result.free = 8L - (uintptr(@unsafe.Pointer(_addr_result.bits[0L])) & 7L);
            }

            return _addr_result!;

        }
    }
}
