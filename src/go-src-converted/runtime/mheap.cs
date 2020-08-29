// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Page heap.
//
// See malloc.go for overview.

// package runtime -- go2cs converted at 2020 August 29 08:18:23 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\mheap.go
using atomic = go.runtime.@internal.atomic_package;
using sys = go.runtime.@internal.sys_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;

namespace go
{
    public static unsafe partial class runtime_package
    {
        // minPhysPageSize is a lower-bound on the physical page size. The
        // true physical page size may be larger than this. In contrast,
        // sys.PhysPageSize is an upper-bound on the physical page size.
        private static readonly long minPhysPageSize = 4096L;

        // Main malloc heap.
        // The heap itself is the "free[]" and "large" arrays,
        // but all the other global data is here too.
        //
        // mheap must not be heap-allocated because it contains mSpanLists,
        // which must not be heap-allocated.
        //
        //go:notinheap


        // Main malloc heap.
        // The heap itself is the "free[]" and "large" arrays,
        // but all the other global data is here too.
        //
        // mheap must not be heap-allocated because it contains mSpanLists,
        // which must not be heap-allocated.
        //
        //go:notinheap
        private partial struct mheap
        {
            public mutex @lock;
            public array<mSpanList> free; // free lists of given length up to _MaxMHeapList
            public mTreap freelarge; // free treap of length >= _MaxMHeapList
            public array<mSpanList> busy; // busy lists of large spans of given length
            public mSpanList busylarge; // busy lists of large spans length >= _MaxMHeapList
            public uint sweepgen; // sweep generation, see comment in mspan
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
            public slice<ref mspan> allspans; // all spans out there

// spans is a lookup table to map virtual address page IDs to *mspan.
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
//
// This is backed by a reserved region of the address space so
// it can grow without moving. The memory up to len(spans) is
// mapped. cap(spans) indicates the total reserved memory.
            public slice<ref mspan> spans; // sweepSpans contains two mspan stacks: one of swept in-use
// spans, and one of unswept in-use spans. These two trade
// roles on each GC cycle. Since the sweepgen increases by 2
// on each cycle, this means the swept spans are in
// sweepSpans[sweepgen/2%2] and the unswept spans are in
// sweepSpans[1-sweepgen/2%2]. Sweeping pops spans from the
// unswept stack and pushes spans that are still in-use on the
// swept stack. Likewise, allocating an in-use span pushes it
// on the swept stack.
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
            public ulong pagesInUse; // pages of spans in stats _MSpanInUse; R/W with mheap.lock
            public ulong pagesSwept; // pages swept this cycle; updated atomically
            public ulong pagesSweptBasis; // pagesSwept to use as the origin of the sweep ratio; updated atomically
            public ulong sweepHeapLiveBasis; // value of heap_live to use as the origin of sweep ratio; written with lock, read without
            public double sweepPagesPerByte; // proportional sweep ratio; written with lock, read without
// TODO(austin): pagesInUse should be a uintptr, but the 386
// compiler can't 8-byte align fields.

// Malloc stats.
            public ulong largealloc; // bytes allocated for large objects
            public ulong nlargealloc; // number of large object allocations
            public ulong largefree; // bytes freed for large objects (>maxsmallsize)
            public ulong nlargefree; // number of frees for large objects (>maxsmallsize)
            public array<ulong> nsmallfree; // number of frees for small objects (<=maxsmallsize)

// range of addresses we might see in the heap
            public System.UIntPtr bitmap; // Points to one byte past the end of the bitmap
            public System.UIntPtr bitmap_mapped; // The arena_* fields indicate the addresses of the Go heap.
//
// The maximum range of the Go heap is
// [arena_start, arena_start+_MaxMem+1).
//
// The range of the current Go heap is
// [arena_start, arena_used). Parts of this range may not be
// mapped, but the metadata structures are always mapped for
// the full range.
            public System.UIntPtr arena_start;
            public System.UIntPtr arena_used; // Set with setArenaUsed.

// The heap is grown using a linear allocator that allocates
// from the block [arena_alloc, arena_end). arena_alloc is
// often, but *not always* equal to arena_used.
            public System.UIntPtr arena_alloc;
            public System.UIntPtr arena_end; // arena_reserved indicates that the memory [arena_alloc,
// arena_end) is reserved (e.g., mapped PROT_NONE). If this is
// false, we have to be careful not to clobber existing
// mappings here. If this is true, then we own the mapping
// here and *must* clobber it to use it.
            public bool arena_reserved;
            public uint _; // ensure 64-bit alignment

// central free lists for small size classes.
// the padding makes sure that the MCentrals are
// spaced CacheLineSize bytes apart, so that each MCentral.lock
// gets its own cache line.
// central is indexed by spanClass.
            public fixalloc spanalloc; // allocator for span*
            public fixalloc cachealloc; // allocator for mcache*
            public fixalloc treapalloc; // allocator for treapNodes* used by large objects
            public fixalloc specialfinalizeralloc; // allocator for specialfinalizer*
            public fixalloc specialprofilealloc; // allocator for specialprofile*
            public mutex speciallock; // lock for special record allocators.

            public ptr<specialfinalizer> unused; // never set, just here to force the specialfinalizer type into DWARF
        }

        private static mheap mheap_ = default;

        // An MSpan is a run of pages.
        //
        // When a MSpan is in the heap free list, state == MSpanFree
        // and heapmap(s->start) == span, heapmap(s->start+s->npages-1) == span.
        //
        // When a MSpan is allocated, state == MSpanInUse or MSpanManual
        // and heapmap(i) == span for all s->start <= i < s->start+s->npages.

        // Every MSpan is in one doubly-linked list,
        // either one of the MHeap's free lists or one of the
        // MCentral's span lists.

        // An MSpan representing actual memory has state _MSpanInUse,
        // _MSpanManual, or _MSpanFree. Transitions between these states are
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
        private partial struct mSpanState // : byte
        {
        }

        private static readonly mSpanState _MSpanDead = iota;
        private static readonly var _MSpanInUse = 0; // allocated for garbage collected heap
        private static readonly var _MSpanManual = 1; // allocated for manual management (e.g., stack allocator)
        private static readonly var _MSpanFree = 2;

        // mSpanStateNames are the names of the span states, indexed by
        // mSpanState.
        private static @string mSpanStateNames = new slice<@string>(new @string[] { "_MSpanDead", "_MSpanInUse", "_MSpanManual", "_MSpanFree" });

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

            public gclinkptr manualFreeList; // list of free objects in _MSpanManual spans

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
// h->sweepgen is incremented by 2 after every GC

            public uint sweepgen;
            public ushort divMul; // for divide by elemsize - divMagic.mul
            public ushort baseMask; // if non-0, elemsize is a power of 2, & this will get object allocation base
            public ushort allocCount; // number of allocated objects
            public spanClass spanclass; // size class and noscan (uint8)
            public bool incache; // being used by an mcache
            public mSpanState state; // mspaninuse etc
            public byte needzero; // needs to be zeroed before allocation
            public byte divShift; // for divide by elemsize - divMagic.shift
            public byte divShift2; // for divide by elemsize - divMagic.shift2
            public System.UIntPtr elemsize; // computed from sizeclass or from npages
            public long unusedsince; // first time spotted by gc in mspanfree state
            public System.UIntPtr npreleased; // number of pages released to the os
            public System.UIntPtr limit; // end of data in span
            public mutex speciallock; // guards specials list
            public ptr<special> specials; // linked list of special records sorted by offset.
        }

        private static System.UIntPtr @base(this ref mspan s)
        {
            return s.startAddr;
        }

        private static (System.UIntPtr, System.UIntPtr, System.UIntPtr) layout(this ref mspan s)
        {
            total = s.npages << (int)(_PageShift);
            size = s.elemsize;
            if (size > 0L)
            {
                n = total / size;
            }
            return;
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
            var h = (mheap.Value)(vh);
            var s = (mspan.Value)(p);
            if (len(h.allspans) >= cap(h.allspans))
            {
                long n = 64L * 1024L / sys.PtrSize;
                if (n < cap(h.allspans) * 3L / 2L)
                {
                    n = cap(h.allspans) * 3L / 2L;
                }
                slice<ref mspan> @new = default;
                var sp = (slice.Value)(@unsafe.Pointer(ref new));
                sp.array = sysAlloc(uintptr(n) * sys.PtrSize, ref memstats.other_sys);
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
                var oldAllspans = h.allspans * (notInHeapSlice.Value)(@unsafe.Pointer(ref h.allspans));

                @unsafe.Pointer(ref new).Value;
                if (len(oldAllspans) != 0L)
                {
                    sysFree(@unsafe.Pointer(ref oldAllspans[0L]), uintptr(cap(oldAllspans)) * @unsafe.Sizeof(oldAllspans[0L]), ref memstats.other_sys);
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

        // inheap reports whether b is a pointer into a (potentially dead) heap object.
        // It returns false for pointers into _MSpanManual spans.
        // Non-preemptible because it is used by write barriers.
        //go:nowritebarrier
        //go:nosplit
        private static bool inheap(System.UIntPtr b)
        {
            if (b == 0L || b < mheap_.arena_start || b >= mheap_.arena_used)
            {
                return false;
            } 
            // Not a beginning of a block, consult span table to find the block beginning.
            var s = mheap_.spans[(b - mheap_.arena_start) >> (int)(_PageShift)];
            if (s == null || b < s.@base() || b >= s.limit || s.state != mSpanInUse)
            {
                return false;
            }
            return true;
        }

        // inHeapOrStack is a variant of inheap that returns true for pointers
        // into any allocated heap span.
        //
        //go:nowritebarrier
        //go:nosplit
        private static bool inHeapOrStack(System.UIntPtr b)
        {
            if (b == 0L || b < mheap_.arena_start || b >= mheap_.arena_used)
            {
                return false;
            } 
            // Not a beginning of a block, consult span table to find the block beginning.
            var s = mheap_.spans[(b - mheap_.arena_start) >> (int)(_PageShift)];
            if (s == null || b < s.@base())
            {
                return false;
            }

            if (s.state == mSpanInUse || s.state == _MSpanManual) 
                return b < s.limit;
            else 
                return false;
                    }

        // TODO: spanOf and spanOfUnchecked are open-coded in a lot of places.
        // Use the functions instead.

        // spanOf returns the span of p. If p does not point into the heap or
        // no span contains p, spanOf returns nil.
        private static ref mspan spanOf(System.UIntPtr p)
        {
            if (p == 0L || p < mheap_.arena_start || p >= mheap_.arena_used)
            {
                return null;
            }
            return spanOfUnchecked(p);
        }

        // spanOfUnchecked is equivalent to spanOf, but the caller must ensure
        // that p points into the heap (that is, mheap_.arena_start <= p <
        // mheap_.arena_used).
        private static ref mspan spanOfUnchecked(System.UIntPtr p)
        {
            return mheap_.spans[(p - mheap_.arena_start) >> (int)(_PageShift)];
        }

        private static int mlookup(System.UIntPtr v, ref System.UIntPtr @base, ref System.UIntPtr size, ptr<ptr<mspan>> sp)
        {
            var _g_ = getg();

            _g_.m.mcache.local_nlookup++;
            if (sys.PtrSize == 4L && _g_.m.mcache.local_nlookup >= 1L << (int)(30L))
            { 
                // purge cache stats to prevent overflow
                lock(ref mheap_.@lock);
                purgecachedstats(_g_.m.mcache);
                unlock(ref mheap_.@lock);
            }
            var s = mheap_.lookupMaybe(@unsafe.Pointer(v));
            if (sp != null)
            {
                sp.Value = s;
            }
            if (s == null)
            {
                if (base != null)
                {
                    base.Value = 0L;
                }
                if (size != null)
                {
                    size.Value = 0L;
                }
                return 0L;
            }
            var p = s.@base();
            if (s.spanclass.sizeclass() == 0L)
            { 
                // Large object.
                if (base != null)
                {
                    base.Value = p;
                }
                if (size != null)
                {
                    size.Value = s.npages << (int)(_PageShift);
                }
                return 1L;
            }
            var n = s.elemsize;
            if (base != null)
            {
                var i = (v - p) / n;
                base.Value = p + i * n;
            }
            if (size != null)
            {
                size.Value = n;
            }
            return 1L;
        }

        // Initialize the heap.
        private static void init(this ref mheap h, System.UIntPtr spansStart, System.UIntPtr spansBytes)
        {
            h.treapalloc.init(@unsafe.Sizeof(new treapNode()), null, null, ref memstats.other_sys);
            h.spanalloc.init(@unsafe.Sizeof(new mspan()), recordspan, @unsafe.Pointer(h), ref memstats.mspan_sys);
            h.cachealloc.init(@unsafe.Sizeof(new mcache()), null, null, ref memstats.mcache_sys);
            h.specialfinalizeralloc.init(@unsafe.Sizeof(new specialfinalizer()), null, null, ref memstats.other_sys);
            h.specialprofilealloc.init(@unsafe.Sizeof(new specialprofile()), null, null, ref memstats.other_sys); 

            // Don't zero mspan allocations. Background sweeping can
            // inspect a span concurrently with allocating it, so it's
            // important that the span's sweepgen survive across freeing
            // and re-allocating a span to prevent background sweeping
            // from improperly cas'ing it from 0.
            //
            // This is safe because mspan contains no heap pointers.
            h.spanalloc.zero = false; 

            // h->mapcache needs no init
            {
                var i__prev1 = i;

                foreach (var (__i) in h.free)
                {
                    i = __i;
                    h.free[i].init();
                    h.busy[i].init();
                }

                i = i__prev1;
            }

            h.busylarge.init();
            {
                var i__prev1 = i;

                foreach (var (__i) in h.central)
                {
                    i = __i;
                    h.central[i].mcentral.init(spanClass(i));
                }

                i = i__prev1;
            }

            var sp = (slice.Value)(@unsafe.Pointer(ref h.spans));
            sp.array = @unsafe.Pointer(spansStart);
            sp.len = 0L;
            sp.cap = int(spansBytes / sys.PtrSize); 

            // Map metadata structures. But don't map race detector memory
            // since we're not actually growing the arena here (and TSAN
            // gets mad if you map 0 bytes).
            h.setArenaUsed(h.arena_used, false);
        }

        // setArenaUsed extends the usable arena to address arena_used and
        // maps auxiliary VM regions for any newly usable arena space.
        //
        // racemap indicates that this memory should be managed by the race
        // detector. racemap should be true unless this is covering a VM hole.
        private static void setArenaUsed(this ref mheap h, System.UIntPtr arena_used, bool racemap)
        { 
            // Map auxiliary structures *before* h.arena_used is updated.
            // Waiting to update arena_used until after the memory has been mapped
            // avoids faults when other threads try access these regions immediately
            // after observing the change to arena_used.

            // Map the bitmap.
            h.mapBits(arena_used); 

            // Map spans array.
            h.mapSpans(arena_used); 

            // Tell the race detector about the new heap memory.
            if (racemap && raceenabled)
            {
                racemapshadow(@unsafe.Pointer(h.arena_used), arena_used - h.arena_used);
            }
            h.arena_used = arena_used;
        }

        // mapSpans makes sure that the spans are mapped
        // up to the new value of arena_used.
        //
        // Don't call this directly. Call mheap.setArenaUsed.
        private static void mapSpans(this ref mheap h, System.UIntPtr arena_used)
        { 
            // Map spans array, PageSize at a time.
            var n = arena_used;
            n -= h.arena_start;
            n = n / _PageSize * sys.PtrSize;
            n = round(n, physPageSize);
            var need = n / @unsafe.Sizeof(h.spans[0L]);
            var have = uintptr(len(h.spans));
            if (have >= need)
            {
                return;
            }
            h.spans = h.spans[..need];
            sysMap(@unsafe.Pointer(ref h.spans[have]), (need - have) * @unsafe.Sizeof(h.spans[0L]), h.arena_reserved, ref memstats.other_sys);
        }

        // Sweeps spans in list until reclaims at least npages into heap.
        // Returns the actual number of pages reclaimed.
        private static System.UIntPtr reclaimList(this ref mheap h, ref mSpanList list, System.UIntPtr npages)
        {
            var n = uintptr(0L);
            var sg = mheap_.sweepgen;
retry:
            {
                var s = list.first;

                while (s != null)
                {
                    if (s.sweepgen == sg - 2L && atomic.Cas(ref s.sweepgen, sg - 2L, sg - 1L))
                    {
                        list.remove(s); 
                        // swept spans are at the end of the list
                        list.insertBack(s); // Puts it back on a busy list. s is not in the treap at this point.
                        unlock(ref h.@lock);
                        var snpages = s.npages;
                        if (s.sweep(false))
                        {
                            n += snpages;
                    s = s.next;
                        }
                        lock(ref h.@lock);
                        if (n >= npages)
                        {
                            return n;
                        } 
                        // the span could have been moved elsewhere
                        goto retry;
                    }
                    if (s.sweepgen == sg - 1L)
                    { 
                        // the span is being sweept by background sweeper, skip
                        continue;
                    } 
                    // already swept empty span,
                    // all subsequent ones must also be either swept or in process of sweeping
                    break;
                }

            }
            return n;
        }

        // Sweeps and reclaims at least npage pages into heap.
        // Called before allocating npage pages.
        private static void reclaim(this ref mheap h, System.UIntPtr npage)
        { 
            // First try to sweep busy spans with large objects of size >= npage,
            // this has good chances of reclaiming the necessary space.
            {
                var i__prev1 = i;

                for (var i = int(npage); i < len(h.busy); i++)
                {
                    if (h.reclaimList(ref h.busy[i], npage) != 0L)
                    {
                        return; // Bingo!
                    }
                } 

                // Then -- even larger objects.


                i = i__prev1;
            } 

            // Then -- even larger objects.
            if (h.reclaimList(ref h.busylarge, npage) != 0L)
            {
                return; // Bingo!
            } 

            // Now try smaller objects.
            // One such object is not enough, so we need to reclaim several of them.
            var reclaimed = uintptr(0L);
            {
                var i__prev1 = i;

                for (i = 0L; i < int(npage) && i < len(h.busy); i++)
                {
                    reclaimed += h.reclaimList(ref h.busy[i], npage - reclaimed);
                    if (reclaimed >= npage)
                    {
                        return;
                    }
                } 

                // Now sweep everything that is not yet swept.


                i = i__prev1;
            } 

            // Now sweep everything that is not yet swept.
            unlock(ref h.@lock);
            while (true)
            {
                var n = sweepone();
                if (n == ~uintptr(0L))
                { // all spans are swept
                    break;
                }
                reclaimed += n;
                if (reclaimed >= npage)
                {
                    break;
                }
            }

            lock(ref h.@lock);
        }

        // Allocate a new span of npage pages from the heap for GC'd memory
        // and record its size class in the HeapMap and HeapMapCache.
        private static ref mspan alloc_m(this ref mheap h, System.UIntPtr npage, spanClass spanclass, bool large)
        {
            var _g_ = getg();
            if (_g_ != _g_.m.g0)
            {
                throw("_mheap_alloc not on g0 stack");
            }
            lock(ref h.@lock); 

            // To prevent excessive heap growth, before allocating n pages
            // we need to sweep and reclaim at least n pages.
            if (h.sweepdone == 0L)
            { 
                // TODO(austin): This tends to sweep a large number of
                // spans in order to find a few completely free spans
                // (for example, in the garbage benchmark, this sweeps
                // ~30x the number of pages its trying to allocate).
                // If GC kept a bit for whether there were any marks
                // in a span, we could release these free spans
                // at the end of GC and eliminate this entirely.
                if (trace.enabled)
                {
                    traceGCSweepStart();
                }
                h.reclaim(npage);
                if (trace.enabled)
                {
                    traceGCSweepDone();
                }
            } 

            // transfer stats from cache to global
            memstats.heap_scan += uint64(_g_.m.mcache.local_scan);
            _g_.m.mcache.local_scan = 0L;
            memstats.tinyallocs += uint64(_g_.m.mcache.local_tinyallocs);
            _g_.m.mcache.local_tinyallocs = 0L;

            var s = h.allocSpanLocked(npage, ref memstats.heap_inuse);
            if (s != null)
            { 
                // Record span info, because gc needs to be
                // able to map interior pointer to containing span.
                atomic.Store(ref s.sweepgen, h.sweepgen);
                h.sweepSpans[h.sweepgen / 2L % 2L].push(s); // Add to swept in-use list.
                s.state = _MSpanInUse;
                s.allocCount = 0L;
                s.spanclass = spanclass;
                {
                    var sizeclass = spanclass.sizeclass();

                    if (sizeclass == 0L)
                    {
                        s.elemsize = s.npages << (int)(_PageShift);
                        s.divShift = 0L;
                        s.divMul = 0L;
                        s.divShift2 = 0L;
                        s.baseMask = 0L;
                    }
                    else
                    {
                        s.elemsize = uintptr(class_to_size[sizeclass]);
                        var m = ref class_to_divmagic[sizeclass];
                        s.divShift = m.shift;
                        s.divMul = m.mul;
                        s.divShift2 = m.shift2;
                        s.baseMask = m.baseMask;
                    } 

                    // update stats, sweep lists

                } 

                // update stats, sweep lists
                h.pagesInUse += uint64(npage);
                if (large)
                {
                    memstats.heap_objects++;
                    mheap_.largealloc += uint64(s.elemsize);
                    mheap_.nlargealloc++;
                    atomic.Xadd64(ref memstats.heap_live, int64(npage << (int)(_PageShift))); 
                    // Swept spans are at the end of lists.
                    if (s.npages < uintptr(len(h.busy)))
                    {
                        h.busy[s.npages].insertBack(s);
                    }
                    else
                    {
                        h.busylarge.insertBack(s);
                    }
                }
            } 
            // heap_scan and heap_live were updated.
            if (gcBlackenEnabled != 0L)
            {
                gcController.revise();
            }
            if (trace.enabled)
            {
                traceHeapAlloc();
            } 

            // h.spans is accessed concurrently without synchronization
            // from other threads. Hence, there must be a store/store
            // barrier here to ensure the writes to h.spans above happen
            // before the caller can publish a pointer p to an object
            // allocated from s. As soon as this happens, the garbage
            // collector running on another processor could read p and
            // look up s in h.spans. The unlock acts as the barrier to
            // order these writes. On the read side, the data dependency
            // between p and the index in h.spans orders the reads.
            unlock(ref h.@lock);
            return s;
        }

        private static ref mspan alloc(this ref mheap h, System.UIntPtr npage, spanClass spanclass, bool large, bool needzero)
        { 
            // Don't do any operations that lock the heap on the G stack.
            // It might trigger stack growth, and the stack growth code needs
            // to be able to allocate heap.
            ref mspan s = default;
            systemstack(() =>
            {
                s = h.alloc_m(npage, spanclass, large);
            });

            if (s != null)
            {
                if (needzero && s.needzero != 0L)
                {
                    memclrNoHeapPointers(@unsafe.Pointer(s.@base()), s.npages << (int)(_PageShift));
                }
                s.needzero = 0L;
            }
            return s;
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
        // allocManual must be called on the system stack to prevent stack
        // growth. Since this is used by the stack allocator, stack growth
        // during allocManual would self-deadlock.
        //
        //go:systemstack
        private static ref mspan allocManual(this ref mheap h, System.UIntPtr npage, ref ulong stat)
        {
            lock(ref h.@lock);
            var s = h.allocSpanLocked(npage, stat);
            if (s != null)
            {
                s.state = _MSpanManual;
                s.manualFreeList = 0L;
                s.allocCount = 0L;
                s.spanclass = 0L;
                s.nelems = 0L;
                s.elemsize = 0L;
                s.limit = s.@base() + s.npages << (int)(_PageShift); 
                // Manually manged memory doesn't count toward heap_sys.
                memstats.heap_sys -= uint64(s.npages << (int)(_PageShift));
            } 

            // This unlock acts as a release barrier. See mheap.alloc_m.
            unlock(ref h.@lock);

            return s;
        }

        // Allocates a span of the given size.  h must be locked.
        // The returned span has been removed from the
        // free list, but its state is still MSpanFree.
        private static ref mspan allocSpanLocked(this ref mheap h, System.UIntPtr npage, ref ulong stat)
        {
            ref mSpanList list = default;
            ref mspan s = default; 

            // Try in fixed-size lists up to max.
            for (var i = int(npage); i < len(h.free); i++)
            {
                list = ref h.free[i];
                if (!list.isEmpty())
                {
                    s = list.first;
                    list.remove(s);
                    goto HaveSpan;
                }
            } 
            // Best fit in list of large spans.
 
            // Best fit in list of large spans.
            s = h.allocLarge(npage); // allocLarge removed s from h.freelarge for us
            if (s == null)
            {
                if (!h.grow(npage))
                {
                    return null;
                }
                s = h.allocLarge(npage);
                if (s == null)
                {
                    return null;
                }
            }
HaveSpan:
            if (s.state != _MSpanFree)
            {
                throw("MHeap_AllocLocked - MSpan not free");
            }
            if (s.npages < npage)
            {
                throw("MHeap_AllocLocked - bad npages");
            }
            if (s.npreleased > 0L)
            {
                sysUsed(@unsafe.Pointer(s.@base()), s.npages << (int)(_PageShift));
                memstats.heap_released -= uint64(s.npreleased << (int)(_PageShift));
                s.npreleased = 0L;
            }
            if (s.npages > npage)
            { 
                // Trim extra and put it back in the heap.
                var t = (mspan.Value)(h.spanalloc.alloc());
                t.init(s.@base() + npage << (int)(_PageShift), s.npages - npage);
                s.npages = npage;
                var p = (t.@base() - h.arena_start) >> (int)(_PageShift);
                if (p > 0L)
                {
                    h.spans[p - 1L] = s;
                }
                h.spans[p] = t;
                h.spans[p + t.npages - 1L] = t;
                t.needzero = s.needzero;
                s.state = _MSpanManual; // prevent coalescing with s
                t.state = _MSpanManual;
                h.freeSpanLocked(t, false, false, s.unusedsince);
                s.state = _MSpanFree;
            }
            s.unusedsince = 0L;

            p = (s.@base() - h.arena_start) >> (int)(_PageShift);
            for (var n = uintptr(0L); n < npage; n++)
            {
                h.spans[p + n] = s;
            }


            stat.Value += uint64(npage << (int)(_PageShift));
            memstats.heap_idle -= uint64(npage << (int)(_PageShift)); 

            //println("spanalloc", hex(s.start<<_PageShift))
            if (s.inList())
            {
                throw("still in list");
            }
            return s;
        }

        // Large spans have a minimum size of 1MByte. The maximum number of large spans to support
        // 1TBytes is 1 million, experimentation using random sizes indicates that the depth of
        // the tree is less that 2x that of a perfectly balanced tree. For 1TByte can be referenced
        // by a perfectly balanced tree with a depth of 20. Twice that is an acceptable 40.
        private static bool isLargeSpan(this ref mheap h, System.UIntPtr npages)
        {
            return npages >= uintptr(len(h.free));
        }

        // allocLarge allocates a span of at least npage pages from the treap of large spans.
        // Returns nil if no such span currently exists.
        private static ref mspan allocLarge(this ref mheap h, System.UIntPtr npage)
        { 
            // Search treap for smallest span with >= npage pages.
            return h.freelarge.remove(npage);
        }

        // Try to add at least npage pages of memory to the heap,
        // returning whether it worked.
        //
        // h must be locked.
        private static bool grow(this ref mheap h, System.UIntPtr npage)
        { 
            // Ask for a big chunk, to reduce the number of mappings
            // the operating system needs to track; also amortizes
            // the overhead of an operating system mapping.
            // Allocate a multiple of 64kB.
            npage = round(npage, (64L << (int)(10L)) / _PageSize);
            var ask = npage << (int)(_PageShift);
            if (ask < _HeapAllocChunk)
            {
                ask = _HeapAllocChunk;
            }
            var v = h.sysAlloc(ask);
            if (v == null)
            {
                if (ask > npage << (int)(_PageShift))
                {
                    ask = npage << (int)(_PageShift);
                    v = h.sysAlloc(ask);
                }
                if (v == null)
                {
                    print("runtime: out of memory: cannot allocate ", ask, "-byte block (", memstats.heap_sys, " in use)\n");
                    return false;
                }
            } 

            // Create a fake "in use" span and free it, so that the
            // right coalescing happens.
            var s = (mspan.Value)(h.spanalloc.alloc());
            s.init(uintptr(v), ask >> (int)(_PageShift));
            var p = (s.@base() - h.arena_start) >> (int)(_PageShift);
            for (var i = p; i < p + s.npages; i++)
            {
                h.spans[i] = s;
            }

            atomic.Store(ref s.sweepgen, h.sweepgen);
            s.state = _MSpanInUse;
            h.pagesInUse += uint64(s.npages);
            h.freeSpanLocked(s, false, true, 0L);
            return true;
        }

        // Look up the span at the given address.
        // Address is guaranteed to be in map
        // and is guaranteed to be start or end of span.
        private static ref mspan lookup(this ref mheap h, unsafe.Pointer v)
        {
            var p = uintptr(v);
            p -= h.arena_start;
            return h.spans[p >> (int)(_PageShift)];
        }

        // Look up the span at the given address.
        // Address is *not* guaranteed to be in map
        // and may be anywhere in the span.
        // Map entries for the middle of a span are only
        // valid for allocated spans. Free spans may have
        // other garbage in their middles, so we have to
        // check for that.
        private static ref mspan lookupMaybe(this ref mheap h, unsafe.Pointer v)
        {
            if (uintptr(v) < h.arena_start || uintptr(v) >= h.arena_used)
            {
                return null;
            }
            var s = h.spans[(uintptr(v) - h.arena_start) >> (int)(_PageShift)];
            if (s == null || uintptr(v) < s.@base() || uintptr(v) >= uintptr(@unsafe.Pointer(s.limit)) || s.state != _MSpanInUse)
            {
                return null;
            }
            return s;
        }

        // Free the span back into the heap.
        private static void freeSpan(this ref mheap h, ref mspan s, int acct)
        {
            systemstack(() =>
            {
                var mp = getg().m;
                lock(ref h.@lock);
                memstats.heap_scan += uint64(mp.mcache.local_scan);
                mp.mcache.local_scan = 0L;
                memstats.tinyallocs += uint64(mp.mcache.local_tinyallocs);
                mp.mcache.local_tinyallocs = 0L;
                if (msanenabled)
                { 
                    // Tell msan that this entire span is no longer in use.
                    var @base = @unsafe.Pointer(s.@base());
                    var bytes = s.npages << (int)(_PageShift);
                    msanfree(base, bytes);
                }
                if (acct != 0L)
                {
                    memstats.heap_objects--;
                }
                if (gcBlackenEnabled != 0L)
                { 
                    // heap_scan changed.
                    gcController.revise();
                }
                h.freeSpanLocked(s, true, true, 0L);
                unlock(ref h.@lock);
            });
        }

        // freeManual frees a manually-managed span returned by allocManual.
        // stat must be the same as the stat passed to the allocManual that
        // allocated s.
        //
        // This must only be called when gcphase == _GCoff. See mSpanState for
        // an explanation.
        //
        // freeManual must be called on the system stack to prevent stack
        // growth, just like allocManual.
        //
        //go:systemstack
        private static void freeManual(this ref mheap h, ref mspan s, ref ulong stat)
        {
            s.needzero = 1L;
            lock(ref h.@lock);
            stat.Value -= uint64(s.npages << (int)(_PageShift));
            memstats.heap_sys += uint64(s.npages << (int)(_PageShift));
            h.freeSpanLocked(s, false, true, 0L);
            unlock(ref h.@lock);
        }

        // s must be on a busy list (h.busy or h.busylarge) or unlinked.
        private static void freeSpanLocked(this ref mheap h, ref mspan s, bool acctinuse, bool acctidle, long unusedsince)
        {

            if (s.state == _MSpanManual) 
                if (s.allocCount != 0L)
                {
                    throw("MHeap_FreeSpanLocked - invalid stack free");
                }
            else if (s.state == _MSpanInUse) 
                if (s.allocCount != 0L || s.sweepgen != h.sweepgen)
                {
                    print("MHeap_FreeSpanLocked - span ", s, " ptr ", hex(s.@base()), " allocCount ", s.allocCount, " sweepgen ", s.sweepgen, "/", h.sweepgen, "\n");
                    throw("MHeap_FreeSpanLocked - invalid free");
                }
                h.pagesInUse -= uint64(s.npages);
            else 
                throw("MHeap_FreeSpanLocked - invalid span state");
                        if (acctinuse)
            {
                memstats.heap_inuse -= uint64(s.npages << (int)(_PageShift));
            }
            if (acctidle)
            {
                memstats.heap_idle += uint64(s.npages << (int)(_PageShift));
            }
            s.state = _MSpanFree;
            if (s.inList())
            {
                h.busyList(s.npages).remove(s);
            } 

            // Stamp newly unused spans. The scavenger will use that
            // info to potentially give back some pages to the OS.
            s.unusedsince = unusedsince;
            if (unusedsince == 0L)
            {
                s.unusedsince = nanotime();
            }
            s.npreleased = 0L; 

            // Coalesce with earlier, later spans.
            var p = (s.@base() - h.arena_start) >> (int)(_PageShift);
            if (p > 0L)
            {
                var before = h.spans[p - 1L];
                if (before != null && before.state == _MSpanFree)
                { 
                    // Now adjust s.
                    s.startAddr = before.startAddr;
                    s.npages += before.npages;
                    s.npreleased = before.npreleased; // absorb released pages
                    s.needzero |= before.needzero;
                    p -= before.npages;
                    h.spans[p] = s; 
                    // The size is potentially changing so the treap needs to delete adjacent nodes and
                    // insert back as a combined node.
                    if (h.isLargeSpan(before.npages))
                    { 
                        // We have a t, it is large so it has to be in the treap so we can remove it.
                        h.freelarge.removeSpan(before);
                    }
                    else
                    {
                        h.freeList(before.npages).remove(before);
                    }
                    before.state = _MSpanDead;
                    h.spanalloc.free(@unsafe.Pointer(before));
                }
            } 

            // Now check to see if next (greater addresses) span is free and can be coalesced.
            if ((p + s.npages) < uintptr(len(h.spans)))
            {
                var after = h.spans[p + s.npages];
                if (after != null && after.state == _MSpanFree)
                {
                    s.npages += after.npages;
                    s.npreleased += after.npreleased;
                    s.needzero |= after.needzero;
                    h.spans[p + s.npages - 1L] = s;
                    if (h.isLargeSpan(after.npages))
                    {
                        h.freelarge.removeSpan(after);
                    }
                    else
                    {
                        h.freeList(after.npages).remove(after);
                    }
                    after.state = _MSpanDead;
                    h.spanalloc.free(@unsafe.Pointer(after));
                }
            } 

            // Insert s into appropriate list or treap.
            if (h.isLargeSpan(s.npages))
            {
                h.freelarge.insert(s);
            }
            else
            {
                h.freeList(s.npages).insert(s);
            }
        }

        private static ref mSpanList freeList(this ref mheap h, System.UIntPtr npages)
        {
            return ref h.free[npages];
        }

        private static ref mSpanList busyList(this ref mheap h, System.UIntPtr npages)
        {
            if (npages < uintptr(len(h.busy)))
            {
                return ref h.busy[npages];
            }
            return ref h.busylarge;
        }

        private static System.UIntPtr scavengeTreapNode(ref treapNode t, ulong now, ulong limit)
        {
            var s = t.spanKey;
            System.UIntPtr sumreleased = default;
            if ((now - uint64(s.unusedsince)) > limit && s.npreleased != s.npages)
            {
                var start = s.@base();
                var end = start + s.npages << (int)(_PageShift);
                if (physPageSize > _PageSize)
                { 
                    // We can only release pages in
                    // physPageSize blocks, so round start
                    // and end in. (Otherwise, madvise
                    // will round them *out* and release
                    // more memory than we want.)
                    start = (start + physPageSize - 1L) & ~(physPageSize - 1L);
                    end &= physPageSize - 1L;
                    if (end <= start)
                    { 
                        // start and end don't span a
                        // whole physical page.
                        return sumreleased;
                    }
                }
                var len = end - start;
                var released = len - (s.npreleased << (int)(_PageShift));
                if (physPageSize > _PageSize && released == 0L)
                {
                    return sumreleased;
                }
                memstats.heap_released += uint64(released);
                sumreleased += released;
                s.npreleased = len >> (int)(_PageShift);
                sysUnused(@unsafe.Pointer(start), len);
            }
            return sumreleased;
        }

        private static System.UIntPtr scavengelist(ref mSpanList list, ulong now, ulong limit)
        {
            if (list.isEmpty())
            {
                return 0L;
            }
            System.UIntPtr sumreleased = default;
            {
                var s = list.first;

                while (s != null)
                {
                    if ((now - uint64(s.unusedsince)) <= limit || s.npreleased == s.npages)
                    {
                        continue;
                    s = s.next;
                    }
                    var start = s.@base();
                    var end = start + s.npages << (int)(_PageShift);
                    if (physPageSize > _PageSize)
                    { 
                        // We can only release pages in
                        // physPageSize blocks, so round start
                        // and end in. (Otherwise, madvise
                        // will round them *out* and release
                        // more memory than we want.)
                        start = (start + physPageSize - 1L) & ~(physPageSize - 1L);
                        end &= physPageSize - 1L;
                        if (end <= start)
                        { 
                            // start and end don't span a
                            // whole physical page.
                            continue;
                        }
                    }
                    var len = end - start;

                    var released = len - (s.npreleased << (int)(_PageShift));
                    if (physPageSize > _PageSize && released == 0L)
                    {
                        continue;
                    }
                    memstats.heap_released += uint64(released);
                    sumreleased += released;
                    s.npreleased = len >> (int)(_PageShift);
                    sysUnused(@unsafe.Pointer(start), len);
                }

            }
            return sumreleased;
        }

        private static void scavenge(this ref mheap h, int k, ulong now, ulong limit)
        { 
            // Disallow malloc or panic while holding the heap lock. We do
            // this here because this is an non-mallocgc entry-point to
            // the mheap API.
            var gp = getg();
            gp.m.mallocing++;
            lock(ref h.@lock);
            System.UIntPtr sumreleased = default;
            for (long i = 0L; i < len(h.free); i++)
            {
                sumreleased += scavengelist(ref h.free[i], now, limit);
            }

            sumreleased += scavengetreap(h.freelarge.treap, now, limit);
            unlock(ref h.@lock);
            gp.m.mallocing--;

            if (debug.gctrace > 0L)
            {
                if (sumreleased > 0L)
                {
                    print("scvg", k, ": ", sumreleased >> (int)(20L), " MB released\n");
                }
                print("scvg", k, ": inuse: ", memstats.heap_inuse >> (int)(20L), ", idle: ", memstats.heap_idle >> (int)(20L), ", sys: ", memstats.heap_sys >> (int)(20L), ", released: ", memstats.heap_released >> (int)(20L), ", consumed: ", (memstats.heap_sys - memstats.heap_released) >> (int)(20L), " (MB)\n");
            }
        }

        //go:linkname runtime_debug_freeOSMemory runtime/debug.freeOSMemory
        private static void runtime_debug_freeOSMemory()
        {
            GC();
            systemstack(() =>
            {
                mheap_.scavenge(-1L, ~uint64(0L), 0L);

            });
        }

        // Initialize a new span with the given start and npages.
        private static void init(this ref mspan span, System.UIntPtr @base, System.UIntPtr npages)
        { 
            // span is *not* zeroed.
            span.next = null;
            span.prev = null;
            span.list = null;
            span.startAddr = base;
            span.npages = npages;
            span.allocCount = 0L;
            span.spanclass = 0L;
            span.incache = false;
            span.elemsize = 0L;
            span.state = _MSpanDead;
            span.unusedsince = 0L;
            span.npreleased = 0L;
            span.speciallock.key = 0L;
            span.specials = null;
            span.needzero = 0L;
            span.freeindex = 0L;
            span.allocBits = null;
            span.gcmarkBits = null;
        }

        private static bool inList(this ref mspan span)
        {
            return span.list != null;
        }

        // Initialize an empty doubly-linked list.
        private static void init(this ref mSpanList list)
        {
            list.first = null;
            list.last = null;
        }

        private static void remove(this ref mSpanList list, ref mspan span)
        {
            if (span.list != list)
            {
                print("runtime: failed MSpanList_Remove span.npages=", span.npages, " span=", span, " prev=", span.prev, " span.list=", span.list, " list=", list, "\n");
                throw("MSpanList_Remove");
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

        private static bool isEmpty(this ref mSpanList list)
        {
            return list.first == null;
        }

        private static void insert(this ref mSpanList list, ref mspan span)
        {
            if (span.next != null || span.prev != null || span.list != null)
            {
                println("runtime: failed MSpanList_Insert", span, span.next, span.prev, span.list);
                throw("MSpanList_Insert");
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

        private static void insertBack(this ref mSpanList list, ref mspan span)
        {
            if (span.next != null || span.prev != null || span.list != null)
            {
                println("runtime: failed MSpanList_InsertBack", span, span.next, span.prev, span.list);
                throw("MSpanList_InsertBack");
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
        private static void takeAll(this ref mSpanList list, ref mSpanList other)
        {
            if (other.isEmpty())
            {
                return;
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
                list.Value = other.Value;
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

        private static readonly long _KindSpecialFinalizer = 1L;
        private static readonly long _KindSpecialProfile = 2L; 
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

        // Adds the special record s to the list of special records for
        // the object p. All fields of s should be filled in except for
        // offset & next, which this routine will fill in.
        // Returns true if the special was successfully added, false otherwise.
        // (The add will fail only if a record with the same p and s->kind
        //  already exists.)
        private static bool addspecial(unsafe.Pointer p, ref special s)
        {
            var span = mheap_.lookupMaybe(p);
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

            lock(ref span.speciallock); 

            // Find splice point, check for existing record.
            var t = ref span.specials;
            while (true)
            {
                var x = t.Value;
                if (x == null)
                {
                    break;
                }
                if (offset == uintptr(x.offset) && kind == x.kind)
                {
                    unlock(ref span.speciallock);
                    releasem(mp);
                    return false; // already exists
                }
                if (offset < uintptr(x.offset) || (offset == uintptr(x.offset) && kind < x.kind))
                {
                    break;
                }
                t = ref x.next;
            } 

            // Splice in record, fill in offset.
 

            // Splice in record, fill in offset.
            s.offset = uint16(offset);
            s.next = t.Value;
            t.Value = s;
            unlock(ref span.speciallock);
            releasem(mp);

            return true;
        }

        // Removes the Special record of the given kind for the object p.
        // Returns the record if the record existed, nil otherwise.
        // The caller must FixAlloc_Free the result.
        private static ref special removespecial(unsafe.Pointer p, byte kind)
        {
            var span = mheap_.lookupMaybe(p);
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

            lock(ref span.speciallock);
            var t = ref span.specials;
            while (true)
            {
                var s = t.Value;
                if (s == null)
                {
                    break;
                } 
                // This function is used for finalizers only, so we don't check for
                // "interior" specials (p must be exactly equal to s->offset).
                if (offset == uintptr(s.offset) && kind == s.kind)
                {
                    t.Value = s.next;
                    unlock(ref span.speciallock);
                    releasem(mp);
                    return s;
                }
                t = ref s.next;
            }

            unlock(ref span.speciallock);
            releasem(mp);
            return null;
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
        private static bool addfinalizer(unsafe.Pointer p, ref funcval f, System.UIntPtr nret, ref _type fint, ref ptrtype ot)
        {
            lock(ref mheap_.speciallock);
            var s = (specialfinalizer.Value)(mheap_.specialfinalizeralloc.alloc());
            unlock(ref mheap_.speciallock);
            s.special.kind = _KindSpecialFinalizer;
            s.fn = f;
            s.nret = nret;
            s.fint = fint;
            s.ot = ot;
            if (addspecial(p, ref s.special))
            { 
                // This is responsible for maintaining the same
                // GC-related invariants as markrootSpans in any
                // situation where it's possible that markrootSpans
                // has already run but mark termination hasn't yet.
                if (gcphase != _GCoff)
                {
                    var (_, base, _) = findObject(p);
                    var mp = acquirem();
                    var gcw = ref mp.p.ptr().gcw; 
                    // Mark everything reachable from the object
                    // so it's retained for the finalizer.
                    scanobject(uintptr(base), gcw); 
                    // Mark the finalizer itself, since the
                    // special isn't part of the GC'd heap.
                    scanblock(uintptr(@unsafe.Pointer(ref s.fn)), sys.PtrSize, ref oneptrmask[0L], gcw);
                    if (gcBlackenPromptly)
                    {
                        gcw.dispose();
                    }
                    releasem(mp);
                }
                return true;
            } 

            // There was an old finalizer
            lock(ref mheap_.speciallock);
            mheap_.specialfinalizeralloc.free(@unsafe.Pointer(s));
            unlock(ref mheap_.speciallock);
            return false;
        }

        // Removes the finalizer (if any) from the object p.
        private static void removefinalizer(unsafe.Pointer p)
        {
            var s = (specialfinalizer.Value)(@unsafe.Pointer(removespecial(p, _KindSpecialFinalizer)));
            if (s == null)
            {
                return; // there wasn't a finalizer to remove
            }
            lock(ref mheap_.speciallock);
            mheap_.specialfinalizeralloc.free(@unsafe.Pointer(s));
            unlock(ref mheap_.speciallock);
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
        private static void setprofilebucket(unsafe.Pointer p, ref bucket b)
        {
            lock(ref mheap_.speciallock);
            var s = (specialprofile.Value)(mheap_.specialprofilealloc.alloc());
            unlock(ref mheap_.speciallock);
            s.special.kind = _KindSpecialProfile;
            s.b = b;
            if (!addspecial(p, ref s.special))
            {
                throw("setprofilebucket: profile already set");
            }
        }

        // Do whatever cleanup needs to be done to deallocate s. It has
        // already been unlinked from the MSpan specials list.
        private static void freespecial(ref special _s, unsafe.Pointer p, System.UIntPtr size) => func(_s, (ref special s, Defer _, Panic panic, Recover __) =>
        {

            if (s.kind == _KindSpecialFinalizer) 
                var sf = (specialfinalizer.Value)(@unsafe.Pointer(s));
                queuefinalizer(p, sf.fn, sf.nret, sf.fint, sf.ot);
                lock(ref mheap_.speciallock);
                mheap_.specialfinalizeralloc.free(@unsafe.Pointer(sf));
                unlock(ref mheap_.speciallock);
            else if (s.kind == _KindSpecialProfile) 
                var sp = (specialprofile.Value)(@unsafe.Pointer(s));
                mProf_Free(sp.b, size);
                lock(ref mheap_.speciallock);
                mheap_.specialprofilealloc.free(@unsafe.Pointer(sp));
                unlock(ref mheap_.speciallock);
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
        private static ref byte bytep(this ref gcBits b, System.UIntPtr n)
        {
            return addb((uint8.Value)(b), n);
        }

        // bitp returns a pointer to the byte containing bit n and a mask for
        // selecting that bit from *bytep.
        private static (ref byte, byte) bitp(this ref gcBits b, System.UIntPtr n)
        {
            return (b.bytep(n / 8L), 1L << (int)((n % 8L)));
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
        private static ref gcBits tryAlloc(this ref gcBitsArena b, System.UIntPtr bytes)
        {
            if (b == null || atomic.Loaduintptr(ref b.free) + bytes > uintptr(len(b.bits)))
            {
                return null;
            } 
            // Try to allocate from this block.
            var end = atomic.Xadduintptr(ref b.free, bytes);
            if (end > uintptr(len(b.bits)))
            {
                return null;
            } 
            // There was enough room.
            var start = end - bytes;
            return ref b.bits[start];
        }

        // newMarkBits returns a pointer to 8 byte aligned bytes
        // to be used for a span's mark bits.
        private static ref gcBits newMarkBits(System.UIntPtr nelems)
        {
            var blocksNeeded = uintptr((nelems + 63L) / 64L);
            var bytesNeeded = blocksNeeded * 8L; 

            // Try directly allocating from the current head arena.
            var head = (gcBitsArena.Value)(atomic.Loadp(@unsafe.Pointer(ref gcBitsArenas.next)));
            {
                var p__prev1 = p;

                var p = head.tryAlloc(bytesNeeded);

                if (p != null)
                {
                    return p;
                } 

                // There's not enough room in the head arena. We may need to
                // allocate a new arena.

                p = p__prev1;

            } 

            // There's not enough room in the head arena. We may need to
            // allocate a new arena.
            lock(ref gcBitsArenas.@lock); 
            // Try the head arena again, since it may have changed. Now
            // that we hold the lock, the list head can't change, but its
            // free position still can.
            {
                var p__prev1 = p;

                p = gcBitsArenas.next.tryAlloc(bytesNeeded);

                if (p != null)
                {
                    unlock(ref gcBitsArenas.@lock);
                    return p;
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
                    unlock(ref gcBitsArenas.@lock);
                    return p;
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
            atomic.StorepNoWB(@unsafe.Pointer(ref gcBitsArenas.next), @unsafe.Pointer(fresh));

            unlock(ref gcBitsArenas.@lock);
            return p;
        }

        // newAllocBits returns a pointer to 8 byte aligned bytes
        // to be used for this span's alloc bits.
        // newAllocBits is used to provide newly initialized spans
        // allocation bits. For spans not being initialized the
        // the mark bits are repurposed as allocation bits when
        // the span is swept.
        private static ref gcBits newAllocBits(System.UIntPtr nelems)
        {
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
        private static void nextMarkBitArenaEpoch()
        {
            lock(ref gcBitsArenas.@lock);
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
            atomic.StorepNoWB(@unsafe.Pointer(ref gcBitsArenas.next), null); // newMarkBits calls newArena when needed
            unlock(ref gcBitsArenas.@lock);
        }

        // newArenaMayUnlock allocates and zeroes a gcBits arena.
        // The caller must hold gcBitsArena.lock. This may temporarily release it.
        private static ref gcBitsArena newArenaMayUnlock()
        {
            ref gcBitsArena result = default;
            if (gcBitsArenas.free == null)
            {
                unlock(ref gcBitsArenas.@lock);
                result = (gcBitsArena.Value)(sysAlloc(gcBitsChunkBytes, ref memstats.gc_sys));
                if (result == null)
                {
                    throw("runtime: cannot allocate memory");
                }
                lock(ref gcBitsArenas.@lock);
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
                result.free = 8L - (uintptr(@unsafe.Pointer(ref result.bits[0L])) & 7L);
            }
            return result;
        }
    }
}
