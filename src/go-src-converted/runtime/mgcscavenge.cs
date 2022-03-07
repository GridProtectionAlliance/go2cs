// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Scavenging free pages.
//
// This file implements scavenging (the release of physical pages backing mapped
// memory) of free and unused pages in the heap as a way to deal with page-level
// fragmentation and reduce the RSS of Go applications.
//
// Scavenging in Go happens on two fronts: there's the background
// (asynchronous) scavenger and the heap-growth (synchronous) scavenger.
//
// The former happens on a goroutine much like the background sweeper which is
// soft-capped at using scavengePercent of the mutator's time, based on
// order-of-magnitude estimates of the costs of scavenging. The background
// scavenger's primary goal is to bring the estimated heap RSS of the
// application down to a goal.
//
// That goal is defined as:
//   (retainExtraPercent+100) / 100 * (heapGoal / lastHeapGoal) * last_heap_inuse
//
// Essentially, we wish to have the application's RSS track the heap goal, but
// the heap goal is defined in terms of bytes of objects, rather than pages like
// RSS. As a result, we need to take into account for fragmentation internal to
// spans. heapGoal / lastHeapGoal defines the ratio between the current heap goal
// and the last heap goal, which tells us by how much the heap is growing and
// shrinking. We estimate what the heap will grow to in terms of pages by taking
// this ratio and multiplying it by heap_inuse at the end of the last GC, which
// allows us to account for this additional fragmentation. Note that this
// procedure makes the assumption that the degree of fragmentation won't change
// dramatically over the next GC cycle. Overestimating the amount of
// fragmentation simply results in higher memory use, which will be accounted
// for by the next pacing up date. Underestimating the fragmentation however
// could lead to performance degradation. Handling this case is not within the
// scope of the scavenger. Situations where the amount of fragmentation balloons
// over the course of a single GC cycle should be considered pathologies,
// flagged as bugs, and fixed appropriately.
//
// An additional factor of retainExtraPercent is added as a buffer to help ensure
// that there's more unscavenged memory to allocate out of, since each allocation
// out of scavenged memory incurs a potentially expensive page fault.
//
// The goal is updated after each GC and the scavenger's pacing parameters
// (which live in mheap_) are updated to match. The pacing parameters work much
// like the background sweeping parameters. The parameters define a line whose
// horizontal axis is time and vertical axis is estimated heap RSS, and the
// scavenger attempts to stay below that line at all times.
//
// The synchronous heap-growth scavenging happens whenever the heap grows in
// size, for some definition of heap-growth. The intuition behind this is that
// the application had to grow the heap because existing fragments were
// not sufficiently large to satisfy a page-level memory allocation, so we
// scavenge those fragments eagerly to offset the growth in RSS that results.

// package runtime -- go2cs converted at 2022 March 06 22:09:45 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\mgcscavenge.go
using atomic = go.runtime.@internal.atomic_package;
using sys = go.runtime.@internal.sys_package;
using @unsafe = go.@unsafe_package;
using System;


namespace go;

public static partial class runtime_package {

 
// The background scavenger is paced according to these parameters.
//
// scavengePercent represents the portion of mutator time we're willing
// to spend on scavenging in percent.
private static readonly nint scavengePercent = 1; // 1%

// retainExtraPercent represents the amount of memory over the heap goal
// that the scavenger should keep as a buffer space for the allocator.
//
// The purpose of maintaining this overhead is to have a greater pool of
// unscavenged memory available for allocation (since using scavenged memory
// incurs an additional cost), to account for heap fragmentation and
// the ever-changing layout of the heap.
private static readonly nint retainExtraPercent = 10; 

// maxPagesPerPhysPage is the maximum number of supported runtime pages per
// physical page, based on maxPhysPageSize.
private static readonly var maxPagesPerPhysPage = maxPhysPageSize / pageSize; 

// scavengeCostRatio is the approximate ratio between the costs of using previously
// scavenged memory and scavenging memory.
//
// For most systems the cost of scavenging greatly outweighs the costs
// associated with using scavenged memory, making this constant 0. On other systems
// (especially ones where "sysUsed" is not just a no-op) this cost is non-trivial.
//
// This ratio is used as part of multiplicative factor to help the scavenger account
// for the additional costs of using scavenged memory in its pacing.
private static readonly float scavengeCostRatio = 0.7F * (sys.GoosDarwin + sys.GoosIos); 

// scavengeReservationShards determines the amount of memory the scavenger
// should reserve for scavenging at a time. Specifically, the amount of
// memory reserved is (heap size in bytes) / scavengeReservationShards.
private static readonly nint scavengeReservationShards = 64;


// heapRetained returns an estimate of the current heap RSS.
private static ulong heapRetained() {
    return memstats.heap_sys.load() - atomic.Load64(_addr_memstats.heap_released);
}

// gcPaceScavenger updates the scavenger's pacing, particularly
// its rate and RSS goal.
//
// The RSS goal is based on the current heap goal with a small overhead
// to accommodate non-determinism in the allocator.
//
// The pacing is based on scavengePageRate, which applies to both regular and
// huge pages. See that constant for more information.
//
// mheap_.lock must be held or the world must be stopped.
private static void gcPaceScavenger() { 
    // If we're called before the first GC completed, disable scavenging.
    // We never scavenge before the 2nd GC cycle anyway (we don't have enough
    // information about the heap yet) so this is fine, and avoids a fault
    // or garbage data later.
    if (gcController.lastHeapGoal == 0) {
        mheap_.scavengeGoal = ~uint64(0);
        return ;
    }
    var goalRatio = float64(atomic.Load64(_addr_gcController.heapGoal)) / float64(gcController.lastHeapGoal);
    var retainedGoal = uint64(float64(memstats.last_heap_inuse) * goalRatio); 
    // Add retainExtraPercent overhead to retainedGoal. This calculation
    // looks strange but the purpose is to arrive at an integer division
    // (e.g. if retainExtraPercent = 12.5, then we get a divisor of 8)
    // that also avoids the overflow from a multiplication.
    retainedGoal += retainedGoal / (1.0F / (retainExtraPercent / 100.0F)); 
    // Align it to a physical page boundary to make the following calculations
    // a bit more exact.
    retainedGoal = (retainedGoal + uint64(physPageSize) - 1) & ~(uint64(physPageSize) - 1); 

    // Represents where we are now in the heap's contribution to RSS in bytes.
    //
    // Guaranteed to always be a multiple of physPageSize on systems where
    // physPageSize <= pageSize since we map heap_sys at a rate larger than
    // any physPageSize and released memory in multiples of the physPageSize.
    //
    // However, certain functions recategorize heap_sys as other stats (e.g.
    // stack_sys) and this happens in multiples of pageSize, so on systems
    // where physPageSize > pageSize the calculations below will not be exact.
    // Generally this is OK since we'll be off by at most one regular
    // physical page.
    var retainedNow = heapRetained(); 

    // If we're already below our goal, or within one page of our goal, then disable
    // the background scavenger. We disable the background scavenger if there's
    // less than one physical page of work to do because it's not worth it.
    if (retainedNow <= retainedGoal || retainedNow - retainedGoal < uint64(physPageSize)) {
        mheap_.scavengeGoal = ~uint64(0);
        return ;
    }
    mheap_.scavengeGoal = retainedGoal;

}

// Sleep/wait state of the background scavenger.
private static var scavenge = default;

// readyForScavenger signals sysmon to wake the scavenger because
// there may be new work to do.
//
// There may be a significant delay between when this function runs
// and when the scavenger is kicked awake, but it may be safely invoked
// in contexts where wakeScavenger is unsafe to call directly.
private static void readyForScavenger() {
    atomic.Store(_addr_scavenge.sysmonWake, 1);
}

// wakeScavenger immediately unparks the scavenger if necessary.
//
// May run without a P, but it may allocate, so it must not be called
// on any allocation path.
//
// mheap_.lock, scavenge.lock, and sched.lock must not be held.
private static void wakeScavenger() {
    lock(_addr_scavenge.@lock);
    if (scavenge.parked) { 
        // Notify sysmon that it shouldn't bother waking up the scavenger.
        atomic.Store(_addr_scavenge.sysmonWake, 0); 

        // Try to stop the timer but we don't really care if we succeed.
        // It's possible that either a timer was never started, or that
        // we're racing with it.
        // In the case that we're racing with there's the low chance that
        // we experience a spurious wake-up of the scavenger, but that's
        // totally safe.
        stopTimer(scavenge.timer); 

        // Unpark the goroutine and tell it that there may have been a pacing
        // change. Note that we skip the scheduler's runnext slot because we
        // want to avoid having the scavenger interfere with the fair
        // scheduling of user goroutines. In effect, this schedules the
        // scavenger at a "lower priority" but that's OK because it'll
        // catch up on the work it missed when it does get scheduled.
        scavenge.parked = false; 

        // Ready the goroutine by injecting it. We use injectglist instead
        // of ready or goready in order to allow us to run this function
        // without a P. injectglist also avoids placing the goroutine in
        // the current P's runnext slot, which is desirable to prevent
        // the scavenger from interfering with user goroutine scheduling
        // too much.
        ref gList list = ref heap(out ptr<gList> _addr_list);
        list.push(scavenge.g);
        injectglist(_addr_list);

    }
    unlock(_addr_scavenge.@lock);

}

// scavengeSleep attempts to put the scavenger to sleep for ns.
//
// Note that this function should only be called by the scavenger.
//
// The scavenger may be woken up earlier by a pacing change, and it may not go
// to sleep at all if there's a pending pacing change.
//
// Returns the amount of time actually slept.
private static long scavengeSleep(long ns) {
    lock(_addr_scavenge.@lock); 

    // Set the timer.
    //
    // This must happen here instead of inside gopark
    // because we can't close over any variables without
    // failing escape analysis.
    var start = nanotime();
    resetTimer(scavenge.timer, start + ns); 

    // Mark ourself as asleep and go to sleep.
    scavenge.parked = true;
    goparkunlock(_addr_scavenge.@lock, waitReasonSleep, traceEvGoSleep, 2); 

    // Return how long we actually slept for.
    return nanotime() - start;

}

// Background scavenger.
//
// The background scavenger maintains the RSS of the application below
// the line described by the proportional scavenging statistics in
// the mheap struct.
private static void bgscavenge() {
    scavenge.g = getg();

    lockInit(_addr_scavenge.@lock, lockRankScavenge);
    lock(_addr_scavenge.@lock);
    scavenge.parked = true;

    scavenge.timer = @new<timer>();
    scavenge.timer.f = (_, _) => {
        wakeScavenger();
    };

    gcenable_setup.Send(1);
    goparkunlock(_addr_scavenge.@lock, waitReasonGCScavengeWait, traceEvGoBlock, 1); 

    // Exponentially-weighted moving average of the fraction of time this
    // goroutine spends scavenging (that is, percent of a single CPU).
    // It represents a measure of scheduling overheads which might extend
    // the sleep or the critical time beyond what's expected. Assume no
    // overhead to begin with.
    //
    // TODO(mknyszek): Consider making this based on total CPU time of the
    // application (i.e. scavengePercent * GOMAXPROCS). This isn't really
    // feasible now because the scavenger acquires the heap lock over the
    // scavenging operation, which means scavenging effectively blocks
    // allocators and isn't scalable. However, given a scalable allocator,
    // it makes sense to also make the scavenger scale with it; if you're
    // allocating more frequently, then presumably you're also generating
    // more work for the scavenger.
    const var idealFraction = scavengePercent / 100.0F;

    var scavengeEWMA = float64(idealFraction);

    while (true) {
        var released = uintptr(0); 

        // Time in scavenging critical section.
        var crit = float64(0); 

        // Run on the system stack since we grab the heap lock,
        // and a stack growth with the heap lock means a deadlock.
        systemstack(() => {
            lock(_addr_mheap_.@lock); 

            // If background scavenging is disabled or if there's no work to do just park.
            var retained = heapRetained();
            var goal = mheap_.scavengeGoal;
            if (retained <= goal) {
                unlock(_addr_mheap_.@lock);
                return ;
            } 

            // Scavenge one page, and measure the amount of time spent scavenging.
            var start = nanotime();
            released = mheap_.pages.scavenge(physPageSize, true);
            mheap_.pages.scav.released += released;
            crit = float64(nanotime() - start);

            unlock(_addr_mheap_.@lock);

        });

        if (released == 0) {
            lock(_addr_scavenge.@lock);
            scavenge.parked = true;
            goparkunlock(_addr_scavenge.@lock, waitReasonGCScavengeWait, traceEvGoBlock, 1);
            continue;
        }
        if (released < physPageSize) { 
            // If this happens, it means that we may have attempted to release part
            // of a physical page, but the likely effect of that is that it released
            // the whole physical page, some of which may have still been in-use.
            // This could lead to memory corruption. Throw.
            throw("released less than one physical page of memory");

        }
        const float approxCritNSPerPhysicalPage = 10e3F;

        if (crit <= 0) {
            crit = approxCritNSPerPhysicalPage * float64(released / physPageSize);
        }
        crit *= 1 + scavengeCostRatio; 

        // If we spent more than 10 ms (for example, if the OS scheduled us away, or someone
        // put their machine to sleep) in the critical section, bound the time we use to
        // calculate at 10 ms to avoid letting the sleep time get arbitrarily high.
        const float maxCrit = 10e6F;

        if (crit > maxCrit) {
            crit = maxCrit;
        }
        var adjust = scavengeEWMA / idealFraction;
        var sleepTime = int64(adjust * crit / (scavengePercent / 100.0F)); 

        // Go to sleep.
        var slept = scavengeSleep(sleepTime); 

        // Compute the new ratio.
        var fraction = crit / (crit + float64(slept)); 

        // Set a lower bound on the fraction.
        // Due to OS-related anomalies we may "sleep" for an inordinate amount
        // of time. Let's avoid letting the ratio get out of hand by bounding
        // the sleep time we use in our EWMA.
        const float minFraction = 1.0F / 1000.0F;

        if (fraction < minFraction) {
            fraction = minFraction;
        }
        const float alpha = 0.5F;

        scavengeEWMA = alpha * fraction + (1 - alpha) * scavengeEWMA;

    }

}

// scavenge scavenges nbytes worth of free pages, starting with the
// highest address first. Successive calls continue from where it left
// off until the heap is exhausted. Call scavengeStartGen to bring it
// back to the top of the heap.
//
// Returns the amount of memory scavenged in bytes.
//
// p.mheapLock must be held, but may be temporarily released if
// mayUnlock == true.
//
// Must run on the system stack because p.mheapLock must be held.
//
//go:systemstack
private static System.UIntPtr scavenge(this ptr<pageAlloc> _addr_p, System.UIntPtr nbytes, bool mayUnlock) {
    ref pageAlloc p = ref _addr_p.val;

    assertLockHeld(p.mheapLock);

    addrRange addrs = default;    uint gen = default;
    var released = uintptr(0);
    while (released < nbytes) {
        if (addrs.size() == 0) {
            addrs, gen = p.scavengeReserve();

            if (addrs.size() == 0) {
                break;
            }

        }
        var (r, a) = p.scavengeOne(addrs, nbytes - released, mayUnlock);
        released += r;
        addrs = a;

    } 
    // Only unreserve the space which hasn't been scavenged or searched
    // to ensure we always make progress.
    p.scavengeUnreserve(addrs, gen);
    return released;

}

// printScavTrace prints a scavenge trace line to standard error.
//
// released should be the amount of memory released since the last time this
// was called, and forced indicates whether the scavenge was forced by the
// application.
private static void printScavTrace(uint gen, System.UIntPtr released, bool forced) {
    printlock();
    print("scav ", gen, " ", released >> 10, " KiB work, ", atomic.Load64(_addr_memstats.heap_released) >> 10, " KiB total, ", (atomic.Load64(_addr_memstats.heap_inuse) * 100) / heapRetained(), "% util");
    if (forced) {
        print(" (forced)");
    }
    println();
    printunlock();

}

// scavengeStartGen starts a new scavenge generation, resetting
// the scavenger's search space to the full in-use address space.
//
// p.mheapLock must be held.
//
// Must run on the system stack because p.mheapLock must be held.
//
//go:systemstack
private static void scavengeStartGen(this ptr<pageAlloc> _addr_p) {
    ref pageAlloc p = ref _addr_p.val;

    assertLockHeld(p.mheapLock);

    if (debug.scavtrace > 0) {
        printScavTrace(p.scav.gen, p.scav.released, false);
    }
    p.inUse.cloneInto(_addr_p.scav.inUse); 

    // Pick the new starting address for the scavenger cycle.
    offAddr startAddr = default;
    if (p.scav.scavLWM.lessThan(p.scav.freeHWM)) { 
        // The "free" high watermark exceeds the "scavenged" low watermark,
        // so there are free scavengable pages in parts of the address space
        // that the scavenger already searched, the high watermark being the
        // highest one. Pick that as our new starting point to ensure we
        // see those pages.
        startAddr = p.scav.freeHWM;

    }
    else
 { 
        // The "free" high watermark does not exceed the "scavenged" low
        // watermark. This means the allocator didn't free any memory in
        // the range we scavenged last cycle, so we might as well continue
        // scavenging from where we were.
        startAddr = p.scav.scavLWM;

    }
    p.scav.inUse.removeGreaterEqual(startAddr.addr()); 

    // reservationBytes may be zero if p.inUse.totalBytes is small, or if
    // scavengeReservationShards is large. This case is fine as the scavenger
    // will simply be turned off, but it does mean that scavengeReservationShards,
    // in concert with pallocChunkBytes, dictates the minimum heap size at which
    // the scavenger triggers. In practice this minimum is generally less than an
    // arena in size, so virtually every heap has the scavenger on.
    p.scav.reservationBytes = alignUp(p.inUse.totalBytes, pallocChunkBytes) / scavengeReservationShards;
    p.scav.gen++;
    p.scav.released = 0;
    p.scav.freeHWM = minOffAddr;
    p.scav.scavLWM = maxOffAddr;

}

// scavengeReserve reserves a contiguous range of the address space
// for scavenging. The maximum amount of space it reserves is proportional
// to the size of the heap. The ranges are reserved from the high addresses
// first.
//
// Returns the reserved range and the scavenge generation number for it.
//
// p.mheapLock must be held.
//
// Must run on the system stack because p.mheapLock must be held.
//
//go:systemstack
private static (addrRange, uint) scavengeReserve(this ptr<pageAlloc> _addr_p) {
    addrRange _p0 = default;
    uint _p0 = default;
    ref pageAlloc p = ref _addr_p.val;

    assertLockHeld(p.mheapLock); 

    // Start by reserving the minimum.
    var r = p.scav.inUse.removeLast(p.scav.reservationBytes); 

    // Return early if the size is zero; we don't want to use
    // the bogus address below.
    if (r.size() == 0) {
        return (r, p.scav.gen);
    }
    var newBase = alignDown(r.@base.addr(), pallocChunkBytes); 

    // Remove from inUse however much extra we just pulled out.
    p.scav.inUse.removeGreaterEqual(newBase);
    r.@base = new offAddr(newBase);
    return (r, p.scav.gen);

}

// scavengeUnreserve returns an unscavenged portion of a range that was
// previously reserved with scavengeReserve.
//
// p.mheapLock must be held.
//
// Must run on the system stack because p.mheapLock must be held.
//
//go:systemstack
private static void scavengeUnreserve(this ptr<pageAlloc> _addr_p, addrRange r, uint gen) {
    ref pageAlloc p = ref _addr_p.val;

    assertLockHeld(p.mheapLock);

    if (r.size() == 0 || gen != p.scav.gen) {
        return ;
    }
    if (r.@base.addr() % pallocChunkBytes != 0) {
        throw("unreserving unaligned region");
    }
    p.scav.inUse.add(r);

}

// scavengeOne walks over address range work until it finds
// a contiguous run of pages to scavenge. It will try to scavenge
// at most max bytes at once, but may scavenge more to avoid
// breaking huge pages. Once it scavenges some memory it returns
// how much it scavenged in bytes.
//
// Returns the number of bytes scavenged and the part of work
// which was not yet searched.
//
// work's base address must be aligned to pallocChunkBytes.
//
// p.mheapLock must be held, but may be temporarily released if
// mayUnlock == true.
//
// Must run on the system stack because p.mheapLock must be held.
//
//go:systemstack
private static (System.UIntPtr, addrRange) scavengeOne(this ptr<pageAlloc> _addr_p, addrRange work, System.UIntPtr max, bool mayUnlock) {
    System.UIntPtr _p0 = default;
    addrRange _p0 = default;
    ref pageAlloc p = ref _addr_p.val;

    assertLockHeld(p.mheapLock); 

    // Defensively check if we've received an empty address range.
    // If so, just return.
    if (work.size() == 0) { 
        // Nothing to do.
        return (0, work);

    }
    if (work.@base.addr() % pallocChunkBytes != 0) {
        throw("scavengeOne called with unaligned work region");
    }
    var maxPages = max / pageSize;
    if (max % pageSize != 0) {
        maxPages++;
    }
    var minPages = physPageSize / pageSize;
    if (minPages < 1) {
        minPages = 1;
    }
    Action lockHeap = () => {
        if (mayUnlock) {
            lock(p.mheapLock);
        }
    };
    Action unlockHeap = () => {
        if (mayUnlock) {
            unlock(p.mheapLock);
        }
    }; 

    // Fast path: check the chunk containing the top-most address in work,
    // starting at that address's page index in the chunk.
    //
    // Note that work.end() is exclusive, so get the chunk we care about
    // by subtracting 1.
    var maxAddr = work.limit.addr() - 1;
    var maxChunk = chunkIndex(maxAddr);
    if (p.summary[len(p.summary) - 1][maxChunk].max() >= uint(minPages)) { 
        // We only bother looking for a candidate if there at least
        // minPages free pages at all.
        var (base, npages) = p.chunkOf(maxChunk).findScavengeCandidate(chunkPageIndex(maxAddr), minPages, maxPages); 

        // If we found something, scavenge it and return!
        if (npages != 0) {
            work.limit = new offAddr(p.scavengeRangeLocked(maxChunk,base,npages));

            assertLockHeld(p.mheapLock); // Must be locked on return.
            return (uintptr(npages) * pageSize, work);

        }
    }
    work.limit = new offAddr(chunkBase(maxChunk)); 

    // findCandidate finds the next scavenge candidate in work optimistically.
    //
    // Returns the candidate chunk index and true on success, and false on failure.
    //
    // The heap need not be locked.
    Func<addrRange, (chunkIdx, bool)> findCandidate = work => { 
        // Iterate over this work's chunks.
        for (var i = chunkIndex(work.limit.addr() - 1); i >= chunkIndex(work.@base.addr()); i--) { 
            // If this chunk is totally in-use or has no unscavenged pages, don't bother
            // doing a more sophisticated check.
            //
            // Note we're accessing the summary and the chunks without a lock, but
            // that's fine. We're being optimistic anyway.

            // Check quickly if there are enough free pages at all.
            if (p.summary[len(p.summary) - 1][i].max() < uint(minPages)) {
                continue;
            } 

            // Run over the chunk looking harder for a candidate. Again, we could
            // race with a lot of different pieces of code, but we're just being
            // optimistic. Make sure we load the l2 pointer atomically though, to
            // avoid races with heap growth. It may or may not be possible to also
            // see a nil pointer in this case if we do race with heap growth, but
            // just defensively ignore the nils. This operation is optimistic anyway.
            ptr<array<pallocData>> l2 = new ptr<ptr<array<pallocData>>>(atomic.Loadp(@unsafe.Pointer(_addr_p.chunks[i.l1()])));
            if (l2 != null && l2[i.l2()].hasScavengeCandidate(minPages)) {
                return (i, true);
            }

        }
        return (0, false);

    }; 

    // Slow path: iterate optimistically over the in-use address space
    // looking for any free and unscavenged page. If we think we see something,
    // lock and verify it!
    while (work.size() != 0) {
        unlockHeap(); 

        // Search for the candidate.
        var (candidateChunkIdx, ok) = findCandidate(work); 

        // Lock the heap. We need to do this now if we found a candidate or not.
        // If we did, we'll verify it. If not, we need to lock before returning
        // anyway.
        lockHeap();

        if (!ok) { 
            // We didn't find a candidate, so we're done.
            work.limit = work.@base;
            break;

        }
        var chunk = p.chunkOf(candidateChunkIdx);
        (base, npages) = chunk.findScavengeCandidate(pallocChunkPages - 1, minPages, maxPages);
        if (npages > 0) {
            work.limit = new offAddr(p.scavengeRangeLocked(candidateChunkIdx,base,npages));

            assertLockHeld(p.mheapLock); // Must be locked on return.
            return (uintptr(npages) * pageSize, work);

        }
        work.limit = new offAddr(chunkBase(candidateChunkIdx));

    }

    assertLockHeld(p.mheapLock); // Must be locked on return.
    return (0, work);

}

// scavengeRangeLocked scavenges the given region of memory.
// The region of memory is described by its chunk index (ci),
// the starting page index of the region relative to that
// chunk (base), and the length of the region in pages (npages).
//
// Returns the base address of the scavenged region.
//
// p.mheapLock must be held.
private static System.UIntPtr scavengeRangeLocked(this ptr<pageAlloc> _addr_p, chunkIdx ci, nuint @base, nuint npages) {
    ref pageAlloc p = ref _addr_p.val;

    assertLockHeld(p.mheapLock);

    p.chunkOf(ci).scavenged.setRange(base, npages); 

    // Compute the full address for the start of the range.
    var addr = chunkBase(ci) + uintptr(base) * pageSize; 

    // Update the scavenge low watermark.
    {
        offAddr oAddr = (new offAddr(addr));

        if (oAddr.lessThan(p.scav.scavLWM)) {
            p.scav.scavLWM = oAddr;
        }
    } 

    // Only perform the actual scavenging if we're not in a test.
    // It's dangerous to do so otherwise.
    if (p.test) {
        return addr;
    }
    sysUnused(@unsafe.Pointer(addr), uintptr(npages) * pageSize); 

    // Update global accounting only when not in test, otherwise
    // the runtime's accounting will be wrong.
    var nbytes = int64(npages) * pageSize;
    atomic.Xadd64(_addr_memstats.heap_released, nbytes); 

    // Update consistent accounting too.
    var stats = memstats.heapStats.acquire();
    atomic.Xaddint64(_addr_stats.committed, -nbytes);
    atomic.Xaddint64(_addr_stats.released, nbytes);
    memstats.heapStats.release();

    return addr;

}

// fillAligned returns x but with all zeroes in m-aligned
// groups of m bits set to 1 if any bit in the group is non-zero.
//
// For example, fillAligned(0x0100a3, 8) == 0xff00ff.
//
// Note that if m == 1, this is a no-op.
//
// m must be a power of 2 <= maxPagesPerPhysPage.
private static ulong fillAligned(ulong x, nuint m) {
    Func<ulong, ulong, ulong> apply = (x, c) => { 
        // The technique used it here is derived from
        // https://graphics.stanford.edu/~seander/bithacks.html#ZeroInWord
        // and extended for more than just bytes (like nibbles
        // and uint16s) by using an appropriate constant.
        //
        // To summarize the technique, quoting from that page:
        // "[It] works by first zeroing the high bits of the [8]
        // bytes in the word. Subsequently, it adds a number that
        // will result in an overflow to the high bit of a byte if
        // any of the low bits were initially set. Next the high
        // bits of the original word are ORed with these values;
        // thus, the high bit of a byte is set iff any bit in the
        // byte was set. Finally, we determine if any of these high
        // bits are zero by ORing with ones everywhere except the
        // high bits and inverting the result."
        return ~((((x & c) + c) | x) | c);

    }; 
    // Transform x to contain a 1 bit at the top of each m-aligned
    // group of m zero bits.
    switch (m) {
        case 1: 
            return x;
            break;
        case 2: 
            x = apply(x, 0x5555555555555555);
            break;
        case 4: 
            x = apply(x, 0x7777777777777777);
            break;
        case 8: 
            x = apply(x, 0x7f7f7f7f7f7f7f7f);
            break;
        case 16: 
            x = apply(x, 0x7fff7fff7fff7fff);
            break;
        case 32: 
            x = apply(x, 0x7fffffff7fffffff);
            break;
        case 64: // == maxPagesPerPhysPage
            x = apply(x, 0x7fffffffffffffff);
            break;
        default: 
            throw("bad m value");
            break;
    } 
    // Now, the top bit of each m-aligned group in x is set
    // that group was all zero in the original x.

    // From each group of m bits subtract 1.
    // Because we know only the top bits of each
    // m-aligned group are set, we know this will
    // set each group to have all the bits set except
    // the top bit, so just OR with the original
    // result to set all the bits.
    return ~((x - (x >> (int)((m - 1)))) | x);

}

// hasScavengeCandidate returns true if there's any min-page-aligned groups of
// min pages of free-and-unscavenged memory in the region represented by this
// pallocData.
//
// min must be a non-zero power of 2 <= maxPagesPerPhysPage.
private static bool hasScavengeCandidate(this ptr<pallocData> _addr_m, System.UIntPtr min) {
    ref pallocData m = ref _addr_m.val;

    if (min & (min - 1) != 0 || min == 0) {
        print("runtime: min = ", min, "\n");
        throw("min must be a non-zero power of 2");
    }
    else if (min > maxPagesPerPhysPage) {
        print("runtime: min = ", min, "\n");
        throw("min too large");
    }
    for (var i = len(m.scavenged) - 1; i >= 0; i--) { 
        // 1s are scavenged OR non-free => 0s are unscavenged AND free
        //
        // TODO(mknyszek): Consider splitting up fillAligned into two
        // functions, since here we technically could get by with just
        // the first half of its computation. It'll save a few instructions
        // but adds some additional code complexity.
        var x = fillAligned(m.scavenged[i] | m.pallocBits[i], uint(min)); 

        // Quickly skip over chunks of non-free or scavenged pages.
        if (x != ~uint64(0)) {
            return true;
        }
    }
    return false;

}

// findScavengeCandidate returns a start index and a size for this pallocData
// segment which represents a contiguous region of free and unscavenged memory.
//
// searchIdx indicates the page index within this chunk to start the search, but
// note that findScavengeCandidate searches backwards through the pallocData. As a
// a result, it will return the highest scavenge candidate in address order.
//
// min indicates a hard minimum size and alignment for runs of pages. That is,
// findScavengeCandidate will not return a region smaller than min pages in size,
// or that is min pages or greater in size but not aligned to min. min must be
// a non-zero power of 2 <= maxPagesPerPhysPage.
//
// max is a hint for how big of a region is desired. If max >= pallocChunkPages, then
// findScavengeCandidate effectively returns entire free and unscavenged regions.
// If max < pallocChunkPages, it may truncate the returned region such that size is
// max. However, findScavengeCandidate may still return a larger region if, for
// example, it chooses to preserve huge pages, or if max is not aligned to min (it
// will round up). That is, even if max is small, the returned size is not guaranteed
// to be equal to max. max is allowed to be less than min, in which case it is as if
// max == min.
private static (nuint, nuint) findScavengeCandidate(this ptr<pallocData> _addr_m, nuint searchIdx, System.UIntPtr min, System.UIntPtr max) {
    nuint _p0 = default;
    nuint _p0 = default;
    ref pallocData m = ref _addr_m.val;

    if (min & (min - 1) != 0 || min == 0) {
        print("runtime: min = ", min, "\n");
        throw("min must be a non-zero power of 2");
    }
    else if (min > maxPagesPerPhysPage) {
        print("runtime: min = ", min, "\n");
        throw("min too large");
    }
    if (max == 0) {
        max = min;
    }
    else
 {
        max = alignUp(max, min);
    }
    var i = int(searchIdx / 64); 
    // Start by quickly skipping over blocks of non-free or scavenged pages.
    while (i >= 0) { 
        // 1s are scavenged OR non-free => 0s are unscavenged AND free
        var x = fillAligned(m.scavenged[i] | m.pallocBits[i], uint(min));
        if (x != ~uint64(0)) {
            break;
        i--;
        }
    }
    if (i < 0) { 
        // Failed to find any free/unscavenged pages.
        return (0, 0);

    }
    x = fillAligned(m.scavenged[i] | m.pallocBits[i], uint(min));
    var z1 = uint(sys.LeadingZeros64(~x));
    var run = uint(0);
    var end = uint(i) * 64 + (64 - z1);
    if (x << (int)(z1) != 0) { 
        // After shifting out z1 bits, we still have 1s,
        // so the run ends inside this word.
        run = uint(sys.LeadingZeros64(x << (int)(z1)));

    }
    else
 { 
        // After shifting out z1 bits, we have no more 1s.
        // This means the run extends to the bottom of the
        // word so it may extend into further words.
        run = 64 - z1;
        for (var j = i - 1; j >= 0; j--) {
            x = fillAligned(m.scavenged[j] | m.pallocBits[j], uint(min));
            run += uint(sys.LeadingZeros64(x));
            if (x != 0) { 
                // The run stopped in this word.
                break;

            }

        }

    }
    var size = run;
    if (size > uint(max)) {
        size = uint(max);
    }
    var start = end - size; 

    // Each huge page is guaranteed to fit in a single palloc chunk.
    //
    // TODO(mknyszek): Support larger huge page sizes.
    // TODO(mknyszek): Consider taking pages-per-huge-page as a parameter
    // so we can write tests for this.
    if (physHugePageSize > pageSize && physHugePageSize > physPageSize) { 
        // We have huge pages, so let's ensure we don't break one by scavenging
        // over a huge page boundary. If the range [start, start+size) overlaps with
        // a free-and-unscavenged huge page, we want to grow the region we scavenge
        // to include that huge page.

        // Compute the huge page boundary above our candidate.
        var pagesPerHugePage = uintptr(physHugePageSize / pageSize);
        var hugePageAbove = uint(alignUp(uintptr(start), pagesPerHugePage)); 

        // If that boundary is within our current candidate, then we may be breaking
        // a huge page.
        if (hugePageAbove <= end) { 
            // Compute the huge page boundary below our candidate.
            var hugePageBelow = uint(alignDown(uintptr(start), pagesPerHugePage));

            if (hugePageBelow >= end - run) { 
                // We're in danger of breaking apart a huge page since start+size crosses
                // a huge page boundary and rounding down start to the nearest huge
                // page boundary is included in the full run we found. Include the entire
                // huge page in the bound by rounding down to the huge page size.
                size = size + (start - hugePageBelow);
                start = hugePageBelow;

            }

        }
    }
    return (start, size);

}

} // end runtime_package
