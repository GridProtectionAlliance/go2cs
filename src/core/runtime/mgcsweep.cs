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
namespace go;

using abi = @internal.abi_package;
using atomic = @internal.runtime.atomic_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.runtime;

partial class runtime_package {

public static sweepdata Δsweep;

// State of background sweep.
[GoType] partial struct sweepdata {
    internal mutex @lock;
    internal ж<g> g;
    internal bool parked;
    // active tracks outstanding sweepers and the sweep
    // termination condition.
    internal activeSweep active;
    // centralIndex is the current unswept span class.
    // It represents an index into the mcentral span
    // sets. Accessed and updated via its load and
    // update methods. Not protected by a lock.
    //
    // Reset at mark termination.
    // Used by mheap.nextSpanForSweep.
    internal sweepClass centralIndex;
}

[GoType("num:uint32")] partial struct sweepClass;

internal static readonly UntypedInt numSweepClasses = /* numSpanClasses * 2 */ 272;
internal static readonly sweepClass sweepClassDone = /* sweepClass(^uint32(0)) */ 4294967295;

[GoRecv] internal static sweepClass load(this ref sweepClass s) {
    return ((sweepClass)atomic.Load(((ж<uint32>)s)));
}

[GoRecv] internal static void update(this ref sweepClass s, sweepClass sNew) {
    // Only update *s if its current value is less than sNew,
    // since *s increases monotonically.
    var sOld = s.load();
    while (sOld < sNew && !atomic.Cas(((ж<uint32>)s), ((uint32)sOld), ((uint32)sNew))) {
        sOld = s.load();
    }
}

// TODO(mknyszek): This isn't the only place we have
// an atomic monotonically increasing counter. It would
// be nice to have an "atomic max" which is just implemented
// as the above on most architectures. Some architectures
// like RISC-V however have native support for an atomic max.
[GoRecv] internal static void clear(this ref sweepClass s) {
    atomic.Store(((ж<uint32>)s), 0);
}

// split returns the underlying span class as well as
// whether we're interested in the full or partial
// unswept lists for that class, indicated as a boolean
// (true means "full").
internal static (spanClass spc, bool full) split(this sweepClass s) {
    spanClass spc = default!;
    bool full = default!;

    return (((spanClass)(s >> (int)(1))), (sweepClass)(s & 1) == 0);
}

// nextSpanForSweep finds and pops the next span for sweeping from the
// central sweep buffers. It returns ownership of the span to the caller.
// Returns nil if no such span exists.
[GoRecv] internal static ж<mspan> nextSpanForSweep(this ref mheap h) {
    var sg = h.sweepgen;
    for (var sc = Δsweep.centralIndex.load(); sc < numSweepClasses; sc++) {
        var (spc, full) = sc.split();
        var c = Ꮡh.central[spc].of(struct{mcentral mcentral; pad [24]byte}.Ꮡmcentral);
        ж<mspan> s = default!;
        if (full){
            s = c.fullUnswept(sg).pop();
        } else {
            s = c.partialUnswept(sg).pop();
        }
        if (s != nil) {
            // Write down that we found something so future sweepers
            // can start from here.
            Δsweep.centralIndex.update(sc);
            return s;
        }
    }
    // Write down that we found nothing.
    Δsweep.centralIndex.update(sweepClassDone);
    return default!;
}

internal static readonly UntypedInt sweepDrainedMask = /* 1 << 31 */ 2147483648;

// activeSweep is a type that captures whether sweeping
// is done, and whether there are any outstanding sweepers.
//
// Every potential sweeper must call begin() before they look
// for work, and end() after they've finished sweeping.
[GoType] partial struct activeSweep {
    // state is divided into two parts.
    //
    // The top bit (masked by sweepDrainedMask) is a boolean
    // value indicating whether all the sweep work has been
    // drained from the queue.
    //
    // The rest of the bits are a counter, indicating the
    // number of outstanding concurrent sweepers.
    internal @internal.runtime.atomic_package.Uint32 state;
}

// begin registers a new sweeper. Returns a sweepLocker
// for acquiring spans for sweeping. Any outstanding sweeper blocks
// sweep termination.
//
// If the sweepLocker is invalid, the caller can be sure that all
// outstanding sweep work has been drained, so there is nothing left
// to sweep. Note that there may be sweepers currently running, so
// this does not indicate that all sweeping has completed.
//
// Even if the sweepLocker is invalid, its sweepGen is always valid.
[GoRecv] internal static sweepLocker begin(this ref activeSweep a) {
    while (ᐧ) {
        var state = a.state.Load();
        if ((uint32)(state & sweepDrainedMask) != 0) {
            return new sweepLocker(mheap_.sweepgen, false);
        }
        if (a.state.CompareAndSwap(state, state + 1)) {
            return new sweepLocker(mheap_.sweepgen, true);
        }
    }
}

// end deregisters a sweeper. Must be called once for each time
// begin is called if the sweepLocker is valid.
[GoRecv] internal static void end(this ref activeSweep a, sweepLocker sl) {
    if (sl.sweepGen != mheap_.sweepgen) {
        @throw("sweeper left outstanding across sweep generations"u8);
    }
    while (ᐧ) {
        var state = a.state.Load();
        if (((uint32)(state & ~sweepDrainedMask)) - 1 >= sweepDrainedMask) {
            @throw("mismatched begin/end of activeSweep"u8);
        }
        if (a.state.CompareAndSwap(state, state - 1)) {
            if (state != sweepDrainedMask) {
                return;
            }
            if (debug.gcpacertrace > 0) {
                var live = gcController.heapLive.Load();
                print("pacer: sweep done at heap size ", live >> (int)(20), "MB; allocated ", (live - mheap_.sweepHeapLiveBasis) >> (int)(20), "MB during sweep; swept ", mheap_.pagesSwept.Load(), " pages at ", mheap_.sweepPagesPerByte, " pages/byte\n");
            }
            return;
        }
    }
}

// markDrained marks the active sweep cycle as having drained
// all remaining work. This is safe to be called concurrently
// with all other methods of activeSweep, though may race.
//
// Returns true if this call was the one that actually performed
// the mark.
[GoRecv] internal static bool markDrained(this ref activeSweep a) {
    while (ᐧ) {
        var state = a.state.Load();
        if ((uint32)(state & sweepDrainedMask) != 0) {
            return false;
        }
        if (a.state.CompareAndSwap(state, (uint32)(state | sweepDrainedMask))) {
            return true;
        }
    }
}

// sweepers returns the current number of active sweepers.
[GoRecv] internal static uint32 sweepers(this ref activeSweep a) {
    return (uint32)(a.state.Load() & ~sweepDrainedMask);
}

// isDone returns true if all sweep work has been drained and no more
// outstanding sweepers exist. That is, when the sweep phase is
// completely done.
[GoRecv] internal static bool isDone(this ref activeSweep a) {
    return a.state.Load() == sweepDrainedMask;
}

// reset sets up the activeSweep for the next sweep cycle.
//
// The world must be stopped.
[GoRecv] internal static void reset(this ref activeSweep a) {
    assertWorldStopped();
    a.state.Store(0);
}

// finishsweep_m ensures that all spans are swept.
//
// The world must be stopped. This ensures there are no sweeps in
// progress.
//
//go:nowritebarrier
internal static void finishsweep_m() {
    assertWorldStopped();
    // Sweeping must be complete before marking commences, so
    // sweep any unswept spans. If this is a concurrent GC, there
    // shouldn't be any spans left to sweep, so this should finish
    // instantly. If GC was forced before the concurrent sweep
    // finished, there may be spans to sweep.
    while (sweepone() != ^((uintptr)0)) {
    }
    // Make sure there aren't any outstanding sweepers left.
    // At this point, with the world stopped, it means one of two
    // things. Either we were able to preempt a sweeper, or that
    // a sweeper didn't call sweep.active.end when it should have.
    // Both cases indicate a bug, so throw.
    if (Δsweep.active.sweepers() != 0) {
        @throw("active sweepers found at start of mark phase"u8);
    }
    // Reset all the unswept buffers, which should be empty.
    // Do this in sweep termination as opposed to mark termination
    // so that we can catch unswept spans and reclaim blocks as
    // soon as possible.
    var sg = mheap_.sweepgen;
    foreach (var (i, _) in mheap_.central) {
        var c = Ꮡmheap_.central[i].of(struct{mcentral mcentral; pad [24]byte}.Ꮡmcentral);
        c.partialUnswept(sg).reset();
        c.fullUnswept(sg).reset();
    }
    // Sweeping is done, so there won't be any new memory to
    // scavenge for a bit.
    //
    // If the scavenger isn't already awake, wake it up. There's
    // definitely work for it to do at this point.
    scavenger.wake();
    nextMarkBitArenaEpoch();
}

internal static void bgsweep(channel<nint> c) {
    Δsweep.g = getg();
    lockInit(ᏑΔsweep.of(sweepdata.Ꮡlock), lockRankSweep);
    @lock(ᏑΔsweep.of(sweepdata.Ꮡlock));
    Δsweep.parked = true;
    c.ᐸꟷ(1);
    goparkunlock(ᏑΔsweep.of(sweepdata.Ꮡlock), waitReasonGCSweepWait, traceBlockGCSweep, 1);
    while (ᐧ) {
        // bgsweep attempts to be a "low priority" goroutine by intentionally
        // yielding time. It's OK if it doesn't run, because goroutines allocating
        // memory will sweep and ensure that all spans are swept before the next
        // GC cycle. We really only want to run when we're idle.
        //
        // However, calling Gosched after each span swept produces a tremendous
        // amount of tracing events, sometimes up to 50% of events in a trace. It's
        // also inefficient to call into the scheduler so much because sweeping a
        // single span is in general a very fast operation, taking as little as 30 ns
        // on modern hardware. (See #54767.)
        //
        // As a result, bgsweep sweeps in batches, and only calls into the scheduler
        // at the end of every batch. Furthermore, it only yields its time if there
        // isn't spare idle time available on other cores. If there's available idle
        // time, helping to sweep can reduce allocation latencies by getting ahead of
        // the proportional sweeper and having spans ready to go for allocation.
        static readonly UntypedInt sweepBatchSize = 10;
        nint nSwept = 0;
        while (sweepone() != ^((uintptr)0)) {
            nSwept++;
            if (nSwept % sweepBatchSize == 0) {
                goschedIfBusy();
            }
        }
        while (freeSomeWbufs(true)) {
            // N.B. freeSomeWbufs is already batched internally.
            goschedIfBusy();
        }
        @lock(ᏑΔsweep.of(sweepdata.Ꮡlock));
        if (!isSweepDone()) {
            // This can happen if a GC runs between
            // gosweepone returning ^0 above
            // and the lock being acquired.
            unlock(ᏑΔsweep.of(sweepdata.Ꮡlock));
            continue;
        }
        Δsweep.parked = true;
        goparkunlock(ᏑΔsweep.of(sweepdata.Ꮡlock), waitReasonGCSweepWait, traceBlockGCSweep, 1);
    }
}

// sweepLocker acquires sweep ownership of spans.
[GoType] partial struct sweepLocker {
    // sweepGen is the sweep generation of the heap.
    internal uint32 sweepGen;
    internal bool valid;
}

// sweepLocked represents sweep ownership of a span.
[GoType] partial struct sweepLocked {
    public partial ref ж<mspan> mspan { get; }
}

// tryAcquire attempts to acquire sweep ownership of span s. If it
// successfully acquires ownership, it blocks sweep completion.
[GoRecv] internal static (sweepLocked, bool) tryAcquire(this ref sweepLocker l, ж<mspan> Ꮡs) {
    ref var s = ref Ꮡs.val;

    if (!l.valid) {
        @throw("use of invalid sweepLocker"u8);
    }
    // Check before attempting to CAS.
    if (atomic.Load(Ꮡ(s.sweepgen)) != l.sweepGen - 2) {
        return (new sweepLocked(nil), false);
    }
    // Attempt to acquire sweep ownership of s.
    if (!atomic.Cas(Ꮡ(s.sweepgen), l.sweepGen - 2, l.sweepGen - 1)) {
        return (new sweepLocked(nil), false);
    }
    return (new sweepLocked(Ꮡs), true);
}

// sweepone sweeps some unswept heap span and returns the number of pages returned
// to the heap, or ^uintptr(0) if there was nothing to sweep.
internal static uintptr sweepone() {
    var gp = getg();
    // Increment locks to ensure that the goroutine is not preempted
    // in the middle of sweep thus leaving the span in an inconsistent state for next GC
    (~(~gp).m).locks++;
    // TODO(austin): sweepone is almost always called in a loop;
    // lift the sweepLocker into its callers.
    var sl = Δsweep.active.begin();
    if (!sl.valid) {
        (~(~gp).m).locks--;
        return ^((uintptr)0);
    }
    // Find a span to sweep.
    var npages = ^((uintptr)0);
    bool noMoreWork = default!;
    while (ᐧ) {
        var s = mheap_.nextSpanForSweep();
        if (s == nil) {
            noMoreWork = Δsweep.active.markDrained();
            break;
        }
        {
            var state = (~s).state.get(); if (state != mSpanInUse) {
                // This can happen if direct sweeping already
                // swept this span, but in that case the sweep
                // generation should always be up-to-date.
                if (!((~s).sweepgen == sl.sweepGen || (~s).sweepgen == sl.sweepGen + 3)) {
                    print("runtime: bad span s.state=", state, " s.sweepgen=", (~s).sweepgen, " sweepgen=", sl.sweepGen, "\n");
                    @throw("non in-use span in unswept list"u8);
                }
                continue;
            }
        }
        {
            var (sΔ1, ok) = sl.tryAcquire(s); if (ok) {
                // Sweep the span we found.
                npages = sΔ1.npages;
                if (sΔ1.sweep(false)){
                    // Whole span was freed. Count it toward the
                    // page reclaimer credit since these pages can
                    // now be used for span allocation.
                    mheap_.reclaimCredit.Add(npages);
                } else {
                    // Span is still in-use, so this returned no
                    // pages to the heap and the span needs to
                    // move to the swept in-use list.
                    npages = 0;
                }
                break;
            }
        }
    }
    Δsweep.active.end(sl);
    if (noMoreWork) {
        // The sweep list is empty. There may still be
        // concurrent sweeps running, but we're at least very
        // close to done sweeping.
        // Move the scavenge gen forward (signaling
        // that there's new work to do) and wake the scavenger.
        //
        // The scavenger is signaled by the last sweeper because once
        // sweeping is done, we will definitely have useful work for
        // the scavenger to do, since the scavenger only runs over the
        // heap once per GC cycle. This update is not done during sweep
        // termination because in some cases there may be a long delay
        // between sweep done and sweep termination (e.g. not enough
        // allocations to trigger a GC) which would be nice to fill in
        // with scavenging work.
        if (debug.scavtrace > 0) {
            systemstack(
            var mheap_ʗ2 = mheap_;
            () => {
                @lock(Ꮡmheap_ʗ2.of(mheap.Ꮡlock));
                var releasedBg = mheap_ʗ2.pages.scav.releasedBg.Load();
                var releasedEager = mheap_ʗ2.pages.scav.releasedEager.Load();
                printScavTrace(releasedBg, releasedEager, false);
                mheap_ʗ2.pages.scav.releasedBg.Add(-releasedBg);
                mheap_ʗ2.pages.scav.releasedEager.Add(-releasedEager);
                unlock(Ꮡmheap_ʗ2.of(mheap.Ꮡlock));
            });
        }
        scavenger.ready();
    }
    (~(~gp).m).locks--;
    return npages;
}

// isSweepDone reports whether all spans are swept.
//
// Note that this condition may transition from false to true at any
// time as the sweeper runs. It may transition from true to false if a
// GC runs; to prevent that the caller must be non-preemptible or must
// somehow block GC progress.
internal static bool isSweepDone() {
    return Δsweep.active.isDone();
}

// Returns only when span s has been swept.
//
//go:nowritebarrier
[GoRecv] internal static void ensureSwept(this ref mspan s) {
    // Caller must disable preemption.
    // Otherwise when this function returns the span can become unswept again
    // (if GC is triggered on another goroutine).
    var gp = getg();
    if ((~(~gp).m).locks == 0 && (~(~gp).m).mallocing == 0 && gp != (~(~gp).m).g0) {
        @throw("mspan.ensureSwept: m is not locked"u8);
    }
    // If this operation fails, then that means that there are
    // no more spans to be swept. In this case, either s has already
    // been swept, or is about to be acquired for sweeping and swept.
    var sl = Δsweep.active.begin();
    if (sl.valid) {
        // The caller must be sure that the span is a mSpanInUse span.
        {
            var (sΔ1, ok) = sl.tryAcquire(s); if (ok) {
                sΔ1.sweep(false);
                Δsweep.active.end(sl);
                return;
            }
        }
        Δsweep.active.end(sl);
    }
    // Unfortunately we can't sweep the span ourselves. Somebody else
    // got to it first. We don't have efficient means to wait, but that's
    // OK, it will be swept fairly soon.
    while (ᐧ) {
        var spangen = atomic.Load(Ꮡ(s.sweepgen));
        if (spangen == sl.sweepGen || spangen == sl.sweepGen + 3) {
            break;
        }
        osyield();
    }
}

// sweep frees or collects finalizers for blocks not marked in the mark phase.
// It clears the mark bits in preparation for the next GC round.
// Returns true if the span was returned to heap.
// If preserve=true, don't return it to heap nor relink in mcentral lists;
// caller takes care of it.
[GoRecv] internal static bool sweep(this ref sweepLocked sl, bool preserve) {
    // It's critical that we enter this function with preemption disabled,
    // GC must not start while we are in the middle of this function.
    var gp = getg();
    if ((~(~gp).m).locks == 0 && (~(~gp).m).mallocing == 0 && gp != (~(~gp).m).g0) {
        @throw("mspan.sweep: m is not locked"u8);
    }
    var s = sl.mspan;
    if (!preserve) {
        // We'll release ownership of this span. Nil it out to
        // prevent the caller from accidentally using it.
        sl.mspan = default!;
    }
    var sweepgen = mheap_.sweepgen;
    {
        var state = (~s).state.get(); if (state != mSpanInUse || (~s).sweepgen != sweepgen - 1) {
            print("mspan.sweep: state=", state, " sweepgen=", (~s).sweepgen, " mheap.sweepgen=", sweepgen, "\n");
            @throw("mspan.sweep: bad span state"u8);
        }
    }
    var Δtrace = traceAcquire();
    if (Δtrace.ok()) {
        Δtrace.GCSweepSpan((~s).npages * _PageSize);
        traceRelease(Δtrace);
    }
    mheap_.pagesSwept.Add(((int64)(~s).npages));
    ref var spc = ref heap<spanClass>(out var Ꮡspc);
    spc = s.val.spanclass;
    var size = s.val.elemsize;
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
    var hadSpecials = (~s).specials != nil;
    var siter = newSpecialsIter(s);
    while (siter.valid()) {
        // A finalizer can be set for an inner byte of an object, find object beginning.
        var objIndex = ((uintptr)(~siter.s).offset) / size;
        var Δp = s.@base() + objIndex * size;
        var mbits = s.markBitsForIndex(objIndex);
        if (!mbits.isMarked()){
            // This object is not marked and has at least one special record.
            // Pass 1: see if it has a finalizer.
            var hasFinAndRevived = false;
            var endOffset = Δp - s.@base() + size;
            for (var tmp = siter.s; tmp != nil && ((uintptr)(~tmp).offset) < endOffset; tmp = tmp.val.next) {
                if ((~tmp).kind == _KindSpecialFinalizer) {
                    // Stop freeing of object if it has a finalizer.
                    mbits.setMarkedNonAtomic();
                    hasFinAndRevived = true;
                    break;
                }
            }
            if (hasFinAndRevived){
                // Pass 2: queue all finalizers and clear any weak handles. Weak handles are cleared
                // before finalization as specified by the internal/weak package. See the documentation
                // for that package for more details.
                while (siter.valid() && ((uintptr)(~siter.s).offset) < endOffset) {
                    // Find the exact byte for which the special was setup
                    // (as opposed to object beginning).
                    var special = siter.s;
                    var pΔ1 = s.@base() + ((uintptr)(~special).offset);
                    if ((~special).kind == _KindSpecialFinalizer || (~special).kind == _KindSpecialWeakHandle){
                        siter.unlinkAndNext();
                        freeSpecial(special, ((@unsafe.Pointer)pΔ1), size);
                    } else {
                        // All other specials only apply when an object is freed,
                        // so just keep the special record.
                        siter.next();
                    }
                }
            } else {
                // Pass 2: the object is truly dead, free (and handle) all specials.
                while (siter.valid() && ((uintptr)(~siter.s).offset) < endOffset) {
                    // Find the exact byte for which the special was setup
                    // (as opposed to object beginning).
                    var special = siter.s;
                    var pΔ2 = s.@base() + ((uintptr)(~special).offset);
                    siter.unlinkAndNext();
                    freeSpecial(special, ((@unsafe.Pointer)pΔ2), size);
                }
            }
        } else {
            // object is still live
            if ((~siter.s).kind == _KindSpecialReachable){
                var special = siter.unlinkAndNext();
                ((ж<specialReachable>)(uintptr)(new @unsafe.Pointer(special))).val.reachable = true;
                freeSpecial(special, ((@unsafe.Pointer)Δp), size);
            } else {
                // keep special record
                siter.next();
            }
        }
    }
    if (hadSpecials && (~s).specials == nil) {
        spanHasNoSpecials(s);
    }
    if (traceAllocFreeEnabled() || debug.clobberfree != 0 || raceenabled || msanenabled || asanenabled) {
        // Find all newly freed objects.
        var mbits = s.markBitsForBase();
        var abits = s.allocBitsForIndex(0);
        for (var i = ((uintptr)0); i < ((uintptr)(~s).nelems); i++) {
            if (!mbits.isMarked() && (abits.index < ((uintptr)(~s).freeindex) || abits.isMarked())) {
                var x = s.@base() + i * (~s).elemsize;
                if (traceAllocFreeEnabled()) {
                    var traceΔ1 = traceAcquire();
                    if (traceΔ1.ok()) {
                        traceΔ1.HeapObjectFree(x);
                        traceRelease(traceΔ1);
                    }
                }
                if (debug.clobberfree != 0) {
                    clobberfree(((@unsafe.Pointer)x), size);
                }
                // User arenas are handled on explicit free.
                if (raceenabled && !(~s).isUserArenaChunk) {
                    racefree(((@unsafe.Pointer)x), size);
                }
                if (msanenabled && !(~s).isUserArenaChunk) {
                    msanfree(((@unsafe.Pointer)x), size);
                }
                if (asanenabled && !(~s).isUserArenaChunk) {
                    asanpoison(((@unsafe.Pointer)x), size);
                }
            }
            mbits.advance();
            abits.advance();
        }
    }
    // Check for zombie objects.
    if ((~s).freeindex < (~s).nelems) {
        // Everything < freeindex is allocated and hence
        // cannot be zombies.
        //
        // Check the first bitmap byte, where we have to be
        // careful with freeindex.
        var obj = ((uintptr)(~s).freeindex);
        if (((uint8)((~s).gcmarkBits.bytep(obj / 8).val & ~(~s).allocBits.bytep(obj / 8).val)) >> (int)((obj % 8)) != 0) {
            s.reportZombies();
        }
        // Check remaining bytes.
        for (var i = obj / 8 + 1; i < divRoundUp(((uintptr)(~s).nelems), 8); i++) {
            if ((uint8)((~s).gcmarkBits.bytep(i).val & ~(~s).allocBits.bytep(i).val) != 0) {
                s.reportZombies();
            }
        }
    }
    // Count the number of free objects in this span.
    var nalloc = ((uint16)s.countAlloc());
    var nfreed = (~s).allocCount - nalloc;
    if (nalloc > (~s).allocCount) {
        // The zombie check above should have caught this in
        // more detail.
        print("runtime: nelems=", (~s).nelems, " nalloc=", nalloc, " previous allocCount=", (~s).allocCount, " nfreed=", nfreed, "\n");
        @throw("sweep increased allocation count"u8);
    }
    s.val.allocCount = nalloc;
    s.val.freeindex = 0;
    // reset allocation index to start of span.
    s.val.freeIndexForScan = 0;
    if (traceEnabled()) {
        (~(~(~getg()).m).p.ptr()).trace.reclaimed += ((uintptr)nfreed) * (~s).elemsize;
    }
    // gcmarkBits becomes the allocBits.
    // get a fresh cleared gcmarkBits in preparation for next GC
    s.val.allocBits = s.val.gcmarkBits;
    s.val.gcmarkBits = newMarkBits(((uintptr)(~s).nelems));
    // refresh pinnerBits if they exists
    if ((~s).pinnerBits != nil) {
        s.refreshPinnerBits();
    }
    // Initialize alloc bits cache.
    s.refillAllocCache(0);
    // The span must be in our exclusive ownership until we update sweepgen,
    // check for potential races.
    {
        var state = (~s).state.get(); if (state != mSpanInUse || (~s).sweepgen != sweepgen - 1) {
            print("mspan.sweep: state=", state, " sweepgen=", (~s).sweepgen, " mheap.sweepgen=", sweepgen, "\n");
            @throw("mspan.sweep: bad span state after sweep"u8);
        }
    }
    if ((~s).sweepgen == sweepgen + 1 || (~s).sweepgen == sweepgen + 3) {
        @throw("swept cached span"u8);
    }
    // We need to set s.sweepgen = h.sweepgen only when all blocks are swept,
    // because of the potential for a concurrent free/SetFinalizer.
    //
    // But we need to set it before we make the span available for allocation
    // (return it to heap or mcentral), because allocation code assumes that a
    // span is already swept if available for allocation.
    //
    // Serialization point.
    // At this point the mark bits are cleared and allocation ready
    // to go so release the span.
    atomic.Store(Ꮡ((~s).sweepgen), sweepgen);
    if ((~s).isUserArenaChunk) {
        if (preserve) {
            // This is a case that should never be handled by a sweeper that
            // preserves the span for reuse.
            @throw("sweep: tried to preserve a user arena span"u8);
        }
        if (nalloc > 0) {
            // There still exist pointers into the span or the span hasn't been
            // freed yet. It's not ready to be reused. Put it back on the
            // full swept list for the next cycle.
            mheap_.central[spc].mcentral.fullSwept(sweepgen).push(s);
            return false;
        }
        // It's only at this point that the sweeper doesn't actually need to look
        // at this arena anymore, so subtract from pagesInUse now.
        mheap_.pagesInUse.Add(-(~s).npages);
        (~s).state.set(mSpanDead);
        // The arena is ready to be recycled. Remove it from the quarantine list
        // and place it on the ready list. Don't add it back to any sweep lists.
        systemstack(
        var mheap_ʗ2 = mheap_;
        var sʗ2 = s;
        () => {
            if ((~sʗ2).list != Ꮡmheap_ʗ2.userArena.of(struct{arenaHints *arenaHint; quarantineList runtime.mSpanList; readyList runtime.mSpanList}.ᏑquarantineList)) {
                @throw("user arena span is on the wrong list"u8);
            }
            @lock(Ꮡmheap_ʗ2.of(mheap.Ꮡlock));
            mheap_ʗ2.userArena.quarantineList.remove(sʗ2);
            mheap_ʗ2.userArena.readyList.insert(sʗ2);
            unlock(Ꮡmheap_ʗ2.of(mheap.Ꮡlock));
        });
        return false;
    }
    if (spc.sizeclass() != 0){
        // Handle spans for small objects.
        if (nfreed > 0) {
            // Only mark the span as needing zeroing if we've freed any
            // objects, because a fresh span that had been allocated into,
            // wasn't totally filled, but then swept, still has all of its
            // free slots zeroed.
            s.val.needzero = 1;
            var stats = memstats.heapStats.acquire();
            atomic.Xadd64(Ꮡ(~stats).smallFreeCount.at<uint64>(spc.sizeclass()), ((int64)nfreed));
            memstats.heapStats.release();
            // Count the frees in the inconsistent, internal stats.
            gcController.totalFree.Add(((int64)nfreed) * ((int64)(~s).elemsize));
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
            if (nalloc == (~s).nelems){
                mheap_.central[spc].mcentral.fullSwept(sweepgen).push(s);
            } else {
                mheap_.central[spc].mcentral.partialSwept(sweepgen).push(s);
            }
        }
    } else 
    if (!preserve) {
        // Handle spans for large objects.
        if (nfreed != 0) {
            // Free large object span to heap.
            // Count the free in the consistent, external stats.
            //
            // Do this before freeSpan, which might update heapStats' inHeap
            // value. If it does so, then metrics that subtract object footprint
            // from inHeap might overflow. See #67019.
            var stats = memstats.heapStats.acquire();
            atomic.Xadd64(Ꮡ((~stats).largeFreeCount), 1);
            atomic.Xadd64(Ꮡ((~stats).largeFree), ((int64)size));
            memstats.heapStats.release();
            // Count the free in the inconsistent, internal stats.
            gcController.totalFree.Add(((int64)size));
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
            if (debug.efence > 0){
                s.val.limit = 0;
                // prevent mlookup from finding this span
                sysFault(((@unsafe.Pointer)s.@base()), size);
            } else {
                mheap_.freeSpan(s);
            }
            if ((~s).largeType != nil && (abi.TFlag)((~(~s).largeType).TFlag & abi.TFlagUnrolledBitmap) != 0) {
                // The unrolled GCProg bitmap is allocated separately.
                // Free the space for the unrolled bitmap.
                systemstack(
                var mheap_ʗ5 = mheap_;
                var sʗ5 = s;
                () => {
                    var sʗ5 = spanOf(((uintptr)new @unsafe.Pointer((~sʗ5).largeType)));
                    mheap_ʗ5.freeManual(sʗ5, spanAllocPtrScalarBits);
                });
                // Make sure to zero this pointer without putting the old
                // value in a write buffer, as the old value might be an
                // invalid pointer. See arena.go:(*mheap).allocUserArenaChunk.
                ((ж<uintptr>)(uintptr)(((@unsafe.Pointer)(Ꮡ((~s).largeType))))).val = 0;
            }
            return true;
        }
        // Add a large span directly onto the full+swept list.
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
[GoRecv] internal static void reportZombies(this ref mspan s) {
    printlock();
    print("runtime: marked free object in span ", s, ", elemsize=", s.elemsize, " freeindex=", s.freeindex, " (bad use of unsafe.Pointer? try -d=checkptr)\n");
    var mbits = s.markBitsForBase();
    var abits = s.allocBitsForIndex(0);
    for (var i = ((uintptr)0); i < ((uintptr)s.nelems); i++) {
        var addr = s.@base() + i * s.elemsize;
        print(((Δhex)addr));
        var alloc = i < ((uintptr)s.freeindex) || abits.isMarked();
        if (alloc){
            print(" alloc");
        } else {
            print(" free ");
        }
        if (mbits.isMarked()){
            print(" marked  ");
        } else {
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
            hexdumpWords(addr, addr + length, default!);
        }
        mbits.advance();
        abits.advance();
    }
    @throw("found pointer to free object"u8);
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
internal static void deductSweepCredit(uintptr spanBytes, uintptr callerSweepPages) {
    if (mheap_.sweepPagesPerByte == 0) {
        // Proportional sweep is done or disabled.
        return;
    }
    var Δtrace = traceAcquire();
    if (Δtrace.ok()) {
        Δtrace.GCSweepStart();
        traceRelease(Δtrace);
    }
    // Fix debt if necessary.
retry:
    var sweptBasis = mheap_.pagesSweptBasis.Load();
    var live = gcController.heapLive.Load();
    var liveBasis = mheap_.sweepHeapLiveBasis;
    var newHeapLive = spanBytes;
    if (liveBasis < live) {
        // Only do this subtraction when we don't overflow. Otherwise, pagesTarget
        // might be computed as something really huge, causing us to get stuck
        // sweeping here until the next mark phase.
        //
        // Overflow can happen here if gcPaceSweeper is called concurrently with
        // sweeping (i.e. not during a STW, like it usually is) because this code
        // is intentionally racy. A concurrent call to gcPaceSweeper can happen
        // if a GC tuning parameter is modified and we read an older value of
        // heapLive than what was used to set the basis.
        //
        // This state should be transient, so it's fine to just let newHeapLive
        // be a relatively small number. We'll probably just skip this attempt to
        // sweep.
        //
        // See issue #57523.
        newHeapLive += ((uintptr)(live - liveBasis));
    }
    var pagesTarget = ((int64)(mheap_.sweepPagesPerByte * ((float64)newHeapLive))) - ((int64)callerSweepPages);
    while (pagesTarget > ((int64)(mheap_.pagesSwept.Load() - sweptBasis))) {
        if (sweepone() == ^((uintptr)0)) {
            mheap_.sweepPagesPerByte = 0;
            break;
        }
        if (mheap_.pagesSweptBasis.Load() != sweptBasis) {
            // Sweep pacing changed. Recompute debt.
            goto retry;
        }
    }
    Δtrace = traceAcquire();
    if (Δtrace.ok()) {
        Δtrace.GCSweepDone();
        traceRelease(Δtrace);
    }
}

// clobberfree sets the memory content at x to bad content, for debugging
// purposes.
internal static void clobberfree(@unsafe.Pointer x, uintptr size) {
    // size (span.elemsize) is always a multiple of 4.
    for (var i = ((uintptr)0); i < size; i += 4) {
        ((ж<uint32>)(uintptr)(add(x.val, i))).val = (nint)3735928559L;
    }
}

// gcPaceSweeper updates the sweeper's pacing parameters.
//
// Must be called whenever the GC's pacing is updated.
//
// The world must be stopped, or mheap_.lock must be held.
internal static void gcPaceSweeper(uint64 trigger) {
    assertWorldStoppedOrLockHeld(Ꮡmheap_.of(mheap.Ꮡlock));
    // Update sweep pacing.
    if (isSweepDone()){
        mheap_.sweepPagesPerByte = 0;
    } else {
        // Concurrent sweep needs to sweep all of the in-use
        // pages by the time the allocated heap reaches the GC
        // trigger. Compute the ratio of in-use pages to sweep
        // per byte allocated, accounting for the fact that
        // some might already be swept.
        var heapLiveBasis = gcController.heapLive.Load();
        var heapDistance = ((int64)trigger) - ((int64)heapLiveBasis);
        // Add a little margin so rounding errors and
        // concurrent sweep are less likely to leave pages
        // unswept when GC starts.
        heapDistance -= 1024 * 1024;
        if (heapDistance < _PageSize) {
            // Avoid setting the sweep ratio extremely high
            heapDistance = _PageSize;
        }
        var pagesSwept = mheap_.pagesSwept.Load();
        var pagesInUse = mheap_.pagesInUse.Load();
        var sweepDistancePages = ((int64)pagesInUse) - ((int64)pagesSwept);
        if (sweepDistancePages <= 0){
            mheap_.sweepPagesPerByte = 0;
        } else {
            mheap_.sweepPagesPerByte = ((float64)sweepDistancePages) / ((float64)heapDistance);
            mheap_.sweepHeapLiveBasis = heapLiveBasis;
            // Write pagesSweptBasis last, since this
            // signals concurrent sweeps to recompute
            // their debt.
            mheap_.pagesSweptBasis.Store(pagesSwept);
        }
    }
}

} // end runtime_package
