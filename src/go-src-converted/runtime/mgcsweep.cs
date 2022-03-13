// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Garbage collector: sweeping

// The sweeper consists of two different algorithms:
//
// * The object reclaimer finds and frees unmarked slots in spans. It
//   can free a whole span if none of the objects are marked, but that
//   isn't its goal. This can be driven either synchronously by
//   mcentral.cacheSpan for mcentral spans, or asynchronously by
//   sweepone, which looks at all the mcentral lists.
//
// * The span reclaimer looks for spans that contain no marked objects
//   and frees whole spans. This is a separate algorithm because
//   freeing whole spans is the hardest task for the object reclaimer,
//   but is critical when allocating new spans. The entry point for
//   this is mheap_.reclaim and it's driven by a sequential scan of
//   the page marks bitmap in the heap arenas.
//
// Both algorithms ultimately call mspan.sweep, which sweeps a single
// heap span.

// package runtime -- go2cs converted at 2022 March 13 05:25:37 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\mgcsweep.go
namespace go;

using atomic = runtime.@internal.atomic_package;
using @unsafe = @unsafe_package;
using System;

public static partial class runtime_package {

private static sweepdata sweep = default;

// State of background sweep.
private partial struct sweepdata {
    public mutex @lock;
    public ptr<g> g;
    public bool parked;
    public bool started;
    public uint nbgsweep;
    public uint npausesweep; // centralIndex is the current unswept span class.
// It represents an index into the mcentral span
// sets. Accessed and updated via its load and
// update methods. Not protected by a lock.
//
// Reset at mark termination.
// Used by mheap.nextSpanForSweep.
    public sweepClass centralIndex;
}

// sweepClass is a spanClass and one bit to represent whether we're currently
// sweeping partial or full spans.
private partial struct sweepClass { // : uint
}

private static readonly var numSweepClasses = numSpanClasses * 2;
private static readonly sweepClass sweepClassDone = sweepClass(~uint32(0));

private static sweepClass load(this ptr<sweepClass> _addr_s) {
    ref sweepClass s = ref _addr_s.val;

    return sweepClass(atomic.Load((uint32.val)(s)));
}

private static void update(this ptr<sweepClass> _addr_s, sweepClass sNew) {
    ref sweepClass s = ref _addr_s.val;
 
    // Only update *s if its current value is less than sNew,
    // since *s increases monotonically.
    var sOld = s.load();
    while (sOld < sNew && !atomic.Cas((uint32.val)(s), uint32(sOld), uint32(sNew))) {
        sOld = s.load();
    } 
    // TODO(mknyszek): This isn't the only place we have
    // an atomic monotonically increasing counter. It would
    // be nice to have an "atomic max" which is just implemented
    // as the above on most architectures. Some architectures
    // like RISC-V however have native support for an atomic max.
}

private static void clear(this ptr<sweepClass> _addr_s) {
    ref sweepClass s = ref _addr_s.val;

    atomic.Store((uint32.val)(s), 0);
}

// split returns the underlying span class as well as
// whether we're interested in the full or partial
// unswept lists for that class, indicated as a boolean
// (true means "full").
private static (spanClass, bool) split(this sweepClass s) {
    spanClass spc = default;
    bool full = default;

    return (spanClass(s >> 1), s & 1 == 0);
}

// nextSpanForSweep finds and pops the next span for sweeping from the
// central sweep buffers. It returns ownership of the span to the caller.
// Returns nil if no such span exists.
private static ptr<mspan> nextSpanForSweep(this ptr<mheap> _addr_h) {
    ref mheap h = ref _addr_h.val;

    var sg = h.sweepgen;
    for (var sc = sweep.centralIndex.load(); sc < numSweepClasses; sc++) {
        var (spc, full) = sc.split();
        var c = _addr_h.central[spc].mcentral;
        ptr<mspan> s;
        if (full) {
            s = c.fullUnswept(sg).pop();
        }
        else
 {
            s = c.partialUnswept(sg).pop();
        }
        if (s != null) { 
            // Write down that we found something so future sweepers
            // can start from here.
            sweep.centralIndex.update(sc);
            return _addr_s!;
        }
    } 
    // Write down that we found nothing.
    sweep.centralIndex.update(sweepClassDone);
    return _addr_null!;
}

// finishsweep_m ensures that all spans are swept.
//
// The world must be stopped. This ensures there are no sweeps in
// progress.
//
//go:nowritebarrier
private static void finishsweep_m() {
    assertWorldStopped(); 

    // Sweeping must be complete before marking commences, so
    // sweep any unswept spans. If this is a concurrent GC, there
    // shouldn't be any spans left to sweep, so this should finish
    // instantly. If GC was forced before the concurrent sweep
    // finished, there may be spans to sweep.
    while (sweepone() != ~uintptr(0)) {
        sweep.npausesweep++;
    } 

    // Reset all the unswept buffers, which should be empty.
    // Do this in sweep termination as opposed to mark termination
    // so that we can catch unswept spans and reclaim blocks as
    // soon as possible.
    var sg = mheap_.sweepgen;
    foreach (var (i) in mheap_.central) {
        var c = _addr_mheap_.central[i].mcentral;
        c.partialUnswept(sg).reset();
        c.fullUnswept(sg).reset();
    }    wakeScavenger();

    nextMarkBitArenaEpoch();
}

private static void bgsweep() {
    sweep.g = getg();

    lockInit(_addr_sweep.@lock, lockRankSweep);
    lock(_addr_sweep.@lock);
    sweep.parked = true;
    gcenable_setup.Send(1);
    goparkunlock(_addr_sweep.@lock, waitReasonGCSweepWait, traceEvGoBlock, 1);

    while (true) {
        while (sweepone() != ~uintptr(0)) {
            sweep.nbgsweep++;
            Gosched();
        }
        while (freeSomeWbufs(true)) {
            Gosched();
        }
        lock(_addr_sweep.@lock);
        if (!isSweepDone()) { 
            // This can happen if a GC runs between
            // gosweepone returning ^0 above
            // and the lock being acquired.
            unlock(_addr_sweep.@lock);
            continue;
        }
        sweep.parked = true;
        goparkunlock(_addr_sweep.@lock, waitReasonGCSweepWait, traceEvGoBlock, 1);
    }
}

// sweepLocker acquires sweep ownership of spans and blocks sweep
// completion.
private partial struct sweepLocker {
    public uint sweepGen; // blocking indicates that this tracker is blocking sweep
// completion, usually as a result of acquiring sweep
// ownership of at least one span.
    public bool blocking;
}

// sweepLocked represents sweep ownership of a span.
private partial struct sweepLocked {
    public ref ptr<mspan> ptr<mspan> => ref ptr<mspan>_ptr;
}

private static sweepLocker newSweepLocker() {
    return new sweepLocker(sweepGen:mheap_.sweepgen,);
}

// tryAcquire attempts to acquire sweep ownership of span s. If it
// successfully acquires ownership, it blocks sweep completion.
private static (sweepLocked, bool) tryAcquire(this ptr<sweepLocker> _addr_l, ptr<mspan> _addr_s) {
    sweepLocked _p0 = default;
    bool _p0 = default;
    ref sweepLocker l = ref _addr_l.val;
    ref mspan s = ref _addr_s.val;
 
    // Check before attempting to CAS.
    if (atomic.Load(_addr_s.sweepgen) != l.sweepGen - 2) {
        return (new sweepLocked(), false);
    }
    l.blockCompletion(); 
    // Attempt to acquire sweep ownership of s.
    if (!atomic.Cas(_addr_s.sweepgen, l.sweepGen - 2, l.sweepGen - 1)) {
        return (new sweepLocked(), false);
    }
    return (new sweepLocked(s), true);
}

// blockCompletion blocks sweep completion without acquiring any
// specific spans.
private static void blockCompletion(this ptr<sweepLocker> _addr_l) {
    ref sweepLocker l = ref _addr_l.val;

    if (!l.blocking) {
        atomic.Xadd(_addr_mheap_.sweepers, +1);
        l.blocking = true;
    }
}

private static void dispose(this ptr<sweepLocker> _addr_l) {
    ref sweepLocker l = ref _addr_l.val;

    if (!l.blocking) {
        return ;
    }
    l.blocking = false;
    if (atomic.Xadd(_addr_mheap_.sweepers, -1) == 0 && atomic.Load(_addr_mheap_.sweepDrained) != 0) {
        l.sweepIsDone();
    }
}

private static void sweepIsDone(this ptr<sweepLocker> _addr_l) {
    ref sweepLocker l = ref _addr_l.val;

    if (debug.gcpacertrace > 0) {
        print("pacer: sweep done at heap size ", gcController.heapLive >> 20, "MB; allocated ", (gcController.heapLive - mheap_.sweepHeapLiveBasis) >> 20, "MB during sweep; swept ", mheap_.pagesSwept, " pages at ", mheap_.sweepPagesPerByte, " pages/byte\n");
    }
}

// sweepone sweeps some unswept heap span and returns the number of pages returned
// to the heap, or ^uintptr(0) if there was nothing to sweep.
private static System.UIntPtr sweepone() {
    var _g_ = getg(); 

    // increment locks to ensure that the goroutine is not preempted
    // in the middle of sweep thus leaving the span in an inconsistent state for next GC
    _g_.m.locks++;
    if (atomic.Load(_addr_mheap_.sweepDrained) != 0) {
        _g_.m.locks--;
        return ~uintptr(0);
    }
    var sl = newSweepLocker(); 

    // Find a span to sweep.
    var npages = ~uintptr(0);
    bool noMoreWork = default;
    while (true) {
        var s = mheap_.nextSpanForSweep();
        if (s == null) {
            noMoreWork = atomic.Cas(_addr_mheap_.sweepDrained, 0, 1);
            break;
        }
        {
            var state = s.state.get();

            if (state != mSpanInUse) { 
                // This can happen if direct sweeping already
                // swept this span, but in that case the sweep
                // generation should always be up-to-date.
                if (!(s.sweepgen == sl.sweepGen || s.sweepgen == sl.sweepGen + 3)) {
                    print("runtime: bad span s.state=", state, " s.sweepgen=", s.sweepgen, " sweepgen=", sl.sweepGen, "\n");
                    throw("non in-use span in unswept list");
                }
                continue;
            }

        }
        {
            var s__prev1 = s;

            var (s, ok) = sl.tryAcquire(s);

            if (ok) { 
                // Sweep the span we found.
                npages = s.npages;
                if (s.sweep(false)) { 
                    // Whole span was freed. Count it toward the
                    // page reclaimer credit since these pages can
                    // now be used for span allocation.
                    atomic.Xadduintptr(_addr_mheap_.reclaimCredit, npages);
                }
                else
 { 
                    // Span is still in-use, so this returned no
                    // pages to the heap and the span needs to
                    // move to the swept in-use list.
                    npages = 0;
                }
                break;
            }

            s = s__prev1;

        }
    }

    sl.dispose();

    if (noMoreWork) { 
        // The sweep list is empty. There may still be
        // concurrent sweeps running, but we're at least very
        // close to done sweeping.

        // Move the scavenge gen forward (signalling
        // that there's new work to do) and wake the scavenger.
        //
        // The scavenger is signaled by the last sweeper because once
        // sweeping is done, we will definitely have useful work for
        // the scavenger to do, since the scavenger only runs over the
        // heap once per GC cyle. This update is not done during sweep
        // termination because in some cases there may be a long delay
        // between sweep done and sweep termination (e.g. not enough
        // allocations to trigger a GC) which would be nice to fill in
        // with scavenging work.
        systemstack(() => {
            lock(_addr_mheap_.@lock);
            mheap_.pages.scavengeStartGen();
            unlock(_addr_mheap_.@lock);
        }); 
        // Since we might sweep in an allocation path, it's not possible
        // for us to wake the scavenger directly via wakeScavenger, since
        // it could allocate. Ask sysmon to do it for us instead.
        readyForScavenger();
    }
    _g_.m.locks--;
    return npages;
}

// isSweepDone reports whether all spans are swept.
//
// Note that this condition may transition from false to true at any
// time as the sweeper runs. It may transition from true to false if a
// GC runs; to prevent that the caller must be non-preemptible or must
// somehow block GC progress.
private static bool isSweepDone() { 
    // Check that all spans have at least begun sweeping and there
    // are no active sweepers. If both are true, then all spans
    // have finished sweeping.
    return atomic.Load(_addr_mheap_.sweepDrained) != 0 && atomic.Load(_addr_mheap_.sweepers) == 0;
}

// Returns only when span s has been swept.
//go:nowritebarrier
private static void ensureSwept(this ptr<mspan> _addr_s) {
    ref mspan s = ref _addr_s.val;
 
    // Caller must disable preemption.
    // Otherwise when this function returns the span can become unswept again
    // (if GC is triggered on another goroutine).
    var _g_ = getg();
    if (_g_.m.locks == 0 && _g_.m.mallocing == 0 && _g_ != _g_.m.g0) {
        throw("mspan.ensureSwept: m is not locked");
    }
    var sl = newSweepLocker(); 
    // The caller must be sure that the span is a mSpanInUse span.
    {
        var (s, ok) = sl.tryAcquire(s);

        if (ok) {
            s.sweep(false);
            sl.dispose();
            return ;
        }
    }
    sl.dispose(); 

    // unfortunate condition, and we don't have efficient means to wait
    while (true) {
        var spangen = atomic.Load(_addr_s.sweepgen);
        if (spangen == sl.sweepGen || spangen == sl.sweepGen + 3) {
            break;
        }
        osyield();
    }
}

// Sweep frees or collects finalizers for blocks not marked in the mark phase.
// It clears the mark bits in preparation for the next GC round.
// Returns true if the span was returned to heap.
// If preserve=true, don't return it to heap nor relink in mcentral lists;
// caller takes care of it.
private static bool sweep(this ptr<sweepLocked> _addr_sl, bool preserve) {
    ref sweepLocked sl = ref _addr_sl.val;
 
    // It's critical that we enter this function with preemption disabled,
    // GC must not start while we are in the middle of this function.
    var _g_ = getg();
    if (_g_.m.locks == 0 && _g_.m.mallocing == 0 && _g_ != _g_.m.g0) {
        throw("mspan.sweep: m is not locked");
    }
    var s = sl.mspan;
    if (!preserve) { 
        // We'll release ownership of this span. Nil it out to
        // prevent the caller from accidentally using it.
        sl.mspan = null;
    }
    var sweepgen = mheap_.sweepgen;
    {
        var state__prev1 = state;

        var state = s.state.get();

        if (state != mSpanInUse || s.sweepgen != sweepgen - 1) {
            print("mspan.sweep: state=", state, " sweepgen=", s.sweepgen, " mheap.sweepgen=", sweepgen, "\n");
            throw("mspan.sweep: bad span state");
        }
        state = state__prev1;

    }

    if (trace.enabled) {
        traceGCSweepSpan(s.npages * _PageSize);
    }
    atomic.Xadd64(_addr_mheap_.pagesSwept, int64(s.npages));

    var spc = s.spanclass;
    var size = s.elemsize; 

    // The allocBits indicate which unmarked objects don't need to be
    // processed since they were free at the end of the last GC cycle
    // and were not allocated since then.
    // If the allocBits index is >= s.freeindex and the bit
    // is not marked then the object remains unallocated
    // since the last GC.
    // This situation is analogous to being on a freelist.

    // Unlink & free special records for any objects we're about to free.
    // Two complications here:
    // 1. An object can have both finalizer and profile special records.
    //    In such case we need to queue finalizer for execution,
    //    mark the object as live and preserve the profile special.
    // 2. A tiny object can have several finalizers setup for different offsets.
    //    If such object is not marked, we need to queue all finalizers at once.
    // Both 1 and 2 are possible at the same time.
    var hadSpecials = s.specials != null;
    var siter = newSpecialsIter(s);
    while (siter.valid()) { 
        // A finalizer can be set for an inner byte of an object, find object beginning.
        var objIndex = uintptr(siter.s.offset) / size;
        var p = s.@base() + objIndex * size;
        var mbits = s.markBitsForIndex(objIndex);
        if (!mbits.isMarked()) { 
            // This object is not marked and has at least one special record.
            // Pass 1: see if it has at least one finalizer.
            var hasFin = false;
            var endOffset = p - s.@base() + size;
            {
                var tmp = siter.s;

                while (tmp != null && uintptr(tmp.offset) < endOffset) {
                    if (tmp.kind == _KindSpecialFinalizer) { 
                        // Stop freeing of object if it has a finalizer.
                        mbits.setMarkedNonAtomic();
                        hasFin = true;
                        break;
                    tmp = tmp.next;
                    }
                }
        else
 
                // Pass 2: queue all finalizers _or_ handle profile record.

            } 
            // Pass 2: queue all finalizers _or_ handle profile record.
            while (siter.valid() && uintptr(siter.s.offset) < endOffset) { 
                // Find the exact byte for which the special was setup
                // (as opposed to object beginning).
                var special = siter.s;
                p = s.@base() + uintptr(special.offset);
                if (special.kind == _KindSpecialFinalizer || !hasFin) {
                    siter.unlinkAndNext();
                    freeSpecial(special, @unsafe.Pointer(p), size);
                }
                else
 { 
                    // The object has finalizers, so we're keeping it alive.
                    // All other specials only apply when an object is freed,
                    // so just keep the special record.
                    siter.next();
                }
            }
        } { 
            // object is still live
            if (siter.s.kind == _KindSpecialReachable) {
                special = siter.unlinkAndNext()(specialReachable.val);

                (@unsafe.Pointer(special)).reachable = true;
                freeSpecial(special, @unsafe.Pointer(p), size);
            }
            else
 { 
                // keep special record
                siter.next();
            }
        }
    }
    if (hadSpecials && s.specials == null) {
        spanHasNoSpecials(s);
    }
    if (debug.allocfreetrace != 0 || debug.clobberfree != 0 || raceenabled || msanenabled) { 
        // Find all newly freed objects. This doesn't have to
        // efficient; allocfreetrace has massive overhead.
        mbits = s.markBitsForBase();
        var abits = s.allocBitsForIndex(0);
        {
            var i__prev1 = i;

            for (var i = uintptr(0); i < s.nelems; i++) {
                if (!mbits.isMarked() && (abits.index < s.freeindex || abits.isMarked())) {
                    var x = s.@base() + i * s.elemsize;
                    if (debug.allocfreetrace != 0) {
                        tracefree(@unsafe.Pointer(x), size);
                    }
                    if (debug.clobberfree != 0) {
                        clobberfree(@unsafe.Pointer(x), size);
                    }
                    if (raceenabled) {
                        racefree(@unsafe.Pointer(x), size);
                    }
                    if (msanenabled) {
                        msanfree(@unsafe.Pointer(x), size);
                    }
                }
                mbits.advance();
                abits.advance();
            }


            i = i__prev1;
        }
    }
    if (s.freeindex < s.nelems) { 
        // Everything < freeindex is allocated and hence
        // cannot be zombies.
        //
        // Check the first bitmap byte, where we have to be
        // careful with freeindex.
        var obj = s.freeindex;
        if ((s.gcmarkBits.bytep(obj / 8) & ~s.allocBits.bytep(obj / 8).val) >> (int)((obj % 8)) != 0) {
            s.reportZombies();
        }
        {
            var i__prev1 = i;

            for (i = obj / 8 + 1; i < divRoundUp(s.nelems, 8); i++) {
                if (s.gcmarkBits.bytep(i) & ~s.allocBits.bytep(i) != 0.val) {
                    s.reportZombies();
                }
            }


            i = i__prev1;
        }
    }
    var nalloc = uint16(s.countAlloc());
    var nfreed = s.allocCount - nalloc;
    if (nalloc > s.allocCount) { 
        // The zombie check above should have caught this in
        // more detail.
        print("runtime: nelems=", s.nelems, " nalloc=", nalloc, " previous allocCount=", s.allocCount, " nfreed=", nfreed, "\n");
        throw("sweep increased allocation count");
    }
    s.allocCount = nalloc;
    s.freeindex = 0; // reset allocation index to start of span.
    if (trace.enabled) {
        getg().m.p.ptr().traceReclaimed += uintptr(nfreed) * s.elemsize;
    }
    s.allocBits = s.gcmarkBits;
    s.gcmarkBits = newMarkBits(s.nelems); 

    // Initialize alloc bits cache.
    s.refillAllocCache(0); 

    // The span must be in our exclusive ownership until we update sweepgen,
    // check for potential races.
    {
        var state__prev1 = state;

        state = s.state.get();

        if (state != mSpanInUse || s.sweepgen != sweepgen - 1) {
            print("mspan.sweep: state=", state, " sweepgen=", s.sweepgen, " mheap.sweepgen=", sweepgen, "\n");
            throw("mspan.sweep: bad span state after sweep");
        }
        state = state__prev1;

    }
    if (s.sweepgen == sweepgen + 1 || s.sweepgen == sweepgen + 3) {
        throw("swept cached span");
    }
    atomic.Store(_addr_s.sweepgen, sweepgen);

    if (spc.sizeclass() != 0) { 
        // Handle spans for small objects.
        if (nfreed > 0) { 
            // Only mark the span as needing zeroing if we've freed any
            // objects, because a fresh span that had been allocated into,
            // wasn't totally filled, but then swept, still has all of its
            // free slots zeroed.
            s.needzero = 1;
            var stats = memstats.heapStats.acquire();
            atomic.Xadduintptr(_addr_stats.smallFreeCount[spc.sizeclass()], uintptr(nfreed));
            memstats.heapStats.release();
        }
        if (!preserve) { 
            // The caller may not have removed this span from whatever
            // unswept set its on but taken ownership of the span for
            // sweeping by updating sweepgen. If this span still is in
            // an unswept set, then the mcentral will pop it off the
            // set, check its sweepgen, and ignore it.
            if (nalloc == 0) { 
                // Free totally free span directly back to the heap.
                mheap_.freeSpan(s);
                return true;
            } 
            // Return span back to the right mcentral list.
            if (uintptr(nalloc) == s.nelems) {
                mheap_.central[spc].mcentral.fullSwept(sweepgen).push(s);
            }
            else
 {
                mheap_.central[spc].mcentral.partialSwept(sweepgen).push(s);
            }
        }
    }
    else if (!preserve) { 
        // Handle spans for large objects.
        if (nfreed != 0) { 
            // Free large object span to heap.

            // NOTE(rsc,dvyukov): The original implementation of efence
            // in CL 22060046 used sysFree instead of sysFault, so that
            // the operating system would eventually give the memory
            // back to us again, so that an efence program could run
            // longer without running out of memory. Unfortunately,
            // calling sysFree here without any kind of adjustment of the
            // heap data structures means that when the memory does
            // come back to us, we have the wrong metadata for it, either in
            // the mspan structures or in the garbage collection bitmap.
            // Using sysFault here means that the program will run out of
            // memory fairly quickly in efence mode, but at least it won't
            // have mysterious crashes due to confused memory reuse.
            // It should be possible to switch back to sysFree if we also
            // implement and then call some kind of mheap.deleteSpan.
            if (debug.efence > 0) {
                s.limit = 0; // prevent mlookup from finding this span
                sysFault(@unsafe.Pointer(s.@base()), size);
            }
            else
 {
                mheap_.freeSpan(s);
            }
            stats = memstats.heapStats.acquire();
            atomic.Xadduintptr(_addr_stats.largeFreeCount, 1);
            atomic.Xadduintptr(_addr_stats.largeFree, size);
            memstats.heapStats.release();
            return true;
        }
        mheap_.central[spc].mcentral.fullSwept(sweepgen).push(s);
    }
    return false;
}

// reportZombies reports any marked but free objects in s and throws.
//
// This generally means one of the following:
//
// 1. User code converted a pointer to a uintptr and then back
// unsafely, and a GC ran while the uintptr was the only reference to
// an object.
//
// 2. User code (or a compiler bug) constructed a bad pointer that
// points to a free slot, often a past-the-end pointer.
//
// 3. The GC two cycles ago missed a pointer and freed a live object,
// but it was still live in the last cycle, so this GC cycle found a
// pointer to that object and marked it.
private static void reportZombies(this ptr<mspan> _addr_s) {
    ref mspan s = ref _addr_s.val;

    printlock();
    print("runtime: marked free object in span ", s, ", elemsize=", s.elemsize, " freeindex=", s.freeindex, " (bad use of unsafe.Pointer? try -d=checkptr)\n");
    var mbits = s.markBitsForBase();
    var abits = s.allocBitsForIndex(0);
    for (var i = uintptr(0); i < s.nelems; i++) {
        var addr = s.@base() + i * s.elemsize;
        print(hex(addr));
        var alloc = i < s.freeindex || abits.isMarked();
        if (alloc) {
            print(" alloc");
        }
        else
 {
            print(" free ");
        }
        if (mbits.isMarked()) {
            print(" marked  ");
        }
        else
 {
            print(" unmarked");
        }
        var zombie = mbits.isMarked() && !alloc;
        if (zombie) {
            print(" zombie");
        }
        print("\n");
        if (zombie) {
            var length = s.elemsize;
            if (length > 1024) {
                length = 1024;
            }
            hexdumpWords(addr, addr + length, null);
        }
        mbits.advance();
        abits.advance();
    }
    throw("found pointer to free object");
}

// deductSweepCredit deducts sweep credit for allocating a span of
// size spanBytes. This must be performed *before* the span is
// allocated to ensure the system has enough credit. If necessary, it
// performs sweeping to prevent going in to debt. If the caller will
// also sweep pages (e.g., for a large allocation), it can pass a
// non-zero callerSweepPages to leave that many pages unswept.
//
// deductSweepCredit makes a worst-case assumption that all spanBytes
// bytes of the ultimately allocated span will be available for object
// allocation.
//
// deductSweepCredit is the core of the "proportional sweep" system.
// It uses statistics gathered by the garbage collector to perform
// enough sweeping so that all pages are swept during the concurrent
// sweep phase between GC cycles.
//
// mheap_ must NOT be locked.
private static void deductSweepCredit(System.UIntPtr spanBytes, System.UIntPtr callerSweepPages) {
    if (mheap_.sweepPagesPerByte == 0) { 
        // Proportional sweep is done or disabled.
        return ;
    }
    if (trace.enabled) {
        traceGCSweepStart();
    }
retry: 

    // Fix debt if necessary.
    var sweptBasis = atomic.Load64(_addr_mheap_.pagesSweptBasis); 

    // Fix debt if necessary.
    var newHeapLive = uintptr(atomic.Load64(_addr_gcController.heapLive) - mheap_.sweepHeapLiveBasis) + spanBytes;
    var pagesTarget = int64(mheap_.sweepPagesPerByte * float64(newHeapLive)) - int64(callerSweepPages);
    while (pagesTarget > int64(atomic.Load64(_addr_mheap_.pagesSwept) - sweptBasis)) {
        if (sweepone() == ~uintptr(0)) {
            mheap_.sweepPagesPerByte = 0;
            break;
        }
        if (atomic.Load64(_addr_mheap_.pagesSweptBasis) != sweptBasis) { 
            // Sweep pacing changed. Recompute debt.
            goto retry;
        }
    }

    if (trace.enabled) {
        traceGCSweepDone();
    }
}

// clobberfree sets the memory content at x to bad content, for debugging
// purposes.
private static void clobberfree(unsafe.Pointer x, System.UIntPtr size) { 
    // size (span.elemsize) is always a multiple of 4.
    {
        var i = uintptr(0);

        while (i < size) {
            (uint32.val).val;

            (add(x, i)) = 0xdeadbeef;
            i += 4;
        }
    }
}

} // end runtime_package
