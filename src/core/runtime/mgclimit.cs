// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using atomic = @internal.runtime.atomic_package;
using @internal.runtime;

partial class runtime_package {

// gcCPULimiter is a mechanism to limit GC CPU utilization in situations
// where it might become excessive and inhibit application progress (e.g.
// a death spiral).
//
// The core of the limiter is a leaky bucket mechanism that fills with GC
// CPU time and drains with mutator time. Because the bucket fills and
// drains with time directly (i.e. without any weighting), this effectively
// sets a very conservative limit of 50%. This limit could be enforced directly,
// however, but the purpose of the bucket is to accommodate spikes in GC CPU
// utilization without hurting throughput.
//
// Note that the bucket in the leaky bucket mechanism can never go negative,
// so the GC never gets credit for a lot of CPU time spent without the GC
// running. This is intentional, as an application that stays idle for, say,
// an entire day, could build up enough credit to fail to prevent a death
// spiral the following day. The bucket's capacity is the GC's only leeway.
//
// The capacity thus also sets the window the limiter considers. For example,
// if the capacity of the bucket is 1 cpu-second, then the limiter will not
// kick in until at least 1 full cpu-second in the last 2 cpu-second window
// is spent on GC CPU time.
internal static gcCPULimiterState gcCPULimiter;

[GoType("dyn")] partial struct gcCPULimiterState_bucket {
    // Invariants:
    // - fill >= 0
    // - capacity >= 0
    // - fill <= capacity
    internal uint64 fill;
    internal uint64 capacity;
}

[GoType] partial struct gcCPULimiterState {
    internal @internal.runtime.atomic_package.Uint32 @lock;
    internal @internal.runtime.atomic_package.Bool enabled;
    // gcEnabled is an internal copy of gcBlackenEnabled that determines
    // whether the limiter tracks total assist time.
    //
    // gcBlackenEnabled isn't used directly so as to keep this structure
    // unit-testable.
    internal bool gcEnabled;
    // transitioning is true when the GC is in a STW and transitioning between
    // the mark and sweep phases.
    internal bool transitioning;
    // test indicates whether this instance of the struct was made for testing purposes.
    internal bool test;
    internal gcCPULimiterState_bucket bucket;
    // overflow is the cumulative amount of GC CPU time that we tried to fill the
    // bucket with but exceeded its capacity.
    internal uint64 overflow;
    // assistTimePool is the accumulated assist time since the last update.
    internal @internal.runtime.atomic_package.Int64 assistTimePool;
    // idleMarkTimePool is the accumulated idle mark time since the last update.
    internal @internal.runtime.atomic_package.Int64 idleMarkTimePool;
    // idleTimePool is the accumulated time Ps spent on the idle list since the last update.
    internal @internal.runtime.atomic_package.Int64 idleTimePool;
    // lastUpdate is the nanotime timestamp of the last time update was called.
    //
    // Updated under lock, but may be read concurrently.
    internal @internal.runtime.atomic_package.Int64 lastUpdate;
    // lastEnabledCycle is the GC cycle that last had the limiter enabled.
    internal @internal.runtime.atomic_package.Uint32 lastEnabledCycle;
    // nprocs is an internal copy of gomaxprocs, used to determine total available
    // CPU time.
    //
    // gomaxprocs isn't used directly so as to keep this structure unit-testable.
    internal int32 nprocs;
}

// limiting returns true if the CPU limiter is currently enabled, meaning the Go GC
// should take action to limit CPU utilization.
//
// It is safe to call concurrently with other operations.
[GoRecv] internal static bool limiting(this ref gcCPULimiterState l) {
    return l.enabled.Load();
}

// startGCTransition notifies the limiter of a GC transition.
//
// This call takes ownership of the limiter and disables all other means of
// updating the limiter. Release ownership by calling finishGCTransition.
//
// It is safe to call concurrently with other operations.
[GoRecv] internal static void startGCTransition(this ref gcCPULimiterState l, bool enableGC, int64 now) {
    if (!l.tryLock()) {
        // This must happen during a STW, so we can't fail to acquire the lock.
        // If we did, something went wrong. Throw.
        @throw("failed to acquire lock to start a GC transition"u8);
    }
    if (l.gcEnabled == enableGC) {
        @throw("transitioning GC to the same state as before?"u8);
    }
    // Flush whatever was left between the last update and now.
    l.updateLocked(now);
    l.gcEnabled = enableGC;
    l.transitioning = true;
}

// N.B. finishGCTransition releases the lock.
//
// We don't release here to increase the chance that if there's a failure
// to finish the transition, that we throw on failing to acquire the lock.

// finishGCTransition notifies the limiter that the GC transition is complete
// and releases ownership of it. It also accumulates STW time in the bucket.
// now must be the timestamp from the end of the STW pause.
[GoRecv] internal static void finishGCTransition(this ref gcCPULimiterState l, int64 now) {
    if (!l.transitioning) {
        @throw("finishGCTransition called without starting one?"u8);
    }
    // Count the full nprocs set of CPU time because the world is stopped
    // between startGCTransition and finishGCTransition. Even though the GC
    // isn't running on all CPUs, it is preventing user code from doing so,
    // so it might as well be.
    {
        var lastUpdate = l.lastUpdate.Load(); if (now >= lastUpdate) {
            l.accumulate(0, (now - lastUpdate) * ((int64)l.nprocs));
        }
    }
    l.lastUpdate.Store(now);
    l.transitioning = false;
    l.unlock();
}

// gcCPULimiterUpdatePeriod dictates the maximum amount of wall-clock time
// we can go before updating the limiter.
internal static readonly UntypedFloat gcCPULimiterUpdatePeriod = 1e+07; // 10ms

// needUpdate returns true if the limiter's maximum update period has been
// exceeded, and so would benefit from an update.
[GoRecv] internal static bool needUpdate(this ref gcCPULimiterState l, int64 now) {
    return now - l.lastUpdate.Load() > gcCPULimiterUpdatePeriod;
}

// addAssistTime notifies the limiter of additional assist time. It will be
// included in the next update.
[GoRecv] internal static void addAssistTime(this ref gcCPULimiterState l, int64 t) {
    l.assistTimePool.Add(t);
}

// addIdleTime notifies the limiter of additional time a P spent on the idle list. It will be
// subtracted from the total CPU time in the next update.
[GoRecv] internal static void addIdleTime(this ref gcCPULimiterState l, int64 t) {
    l.idleTimePool.Add(t);
}

// update updates the bucket given runtime-specific information. now is the
// current monotonic time in nanoseconds.
//
// This is safe to call concurrently with other operations, except *GCTransition.
[GoRecv] internal static void update(this ref gcCPULimiterState l, int64 now) {
    if (!l.tryLock()) {
        // We failed to acquire the lock, which means something else is currently
        // updating. Just drop our update, the next one to update will include
        // our total assist time.
        return;
    }
    if (l.transitioning) {
        @throw("update during transition"u8);
    }
    l.updateLocked(now);
    l.unlock();
}

// updateLocked is the implementation of update. l.lock must be held.
[GoRecv] internal static void updateLocked(this ref gcCPULimiterState l, int64 now) {
    var lastUpdate = l.lastUpdate.Load();
    if (now < lastUpdate) {
        // Defensively avoid overflow. This isn't even the latest update anyway.
        return;
    }
    var windowTotalTime = (now - lastUpdate) * ((int64)l.nprocs);
    l.lastUpdate.Store(now);
    // Drain the pool of assist time.
    var assistTime = l.assistTimePool.Load();
    if (assistTime != 0) {
        l.assistTimePool.Add(-assistTime);
    }
    // Drain the pool of idle time.
    var idleTime = l.idleTimePool.Load();
    if (idleTime != 0) {
        l.idleTimePool.Add(-idleTime);
    }
    if (!l.test) {
        // Consume time from in-flight events. Make sure we're not preemptible so allp can't change.
        //
        // The reason we do this instead of just waiting for those events to finish and push updates
        // is to ensure that all the time we're accounting for happened sometime between lastUpdate
        // and now. This dramatically simplifies reasoning about the limiter because we're not at
        // risk of extra time being accounted for in this window than actually happened in this window,
        // leading to all sorts of weird transient behavior.
        var mp = acquirem();
        foreach (var (_, pp) in allp) {
            var (typ, duration) = (~pp).limiterEvent.consume(now);
            var exprᴛ1 = typ;
            var matchᴛ1 = false;
            if (exprᴛ1 == limiterEventIdleMarkWork) { matchᴛ1 = true;
                fallthrough = true;
            }
            if (fallthrough || !matchᴛ1 && exprᴛ1 == limiterEventIdle)) { matchᴛ1 = true;
                idleTime += duration;
                sched.idleTime.Add(duration);
            }
            else if (exprᴛ1 == limiterEventMarkAssist) { matchᴛ1 = true;
                fallthrough = true;
            }
            if (fallthrough || !matchᴛ1 && exprᴛ1 == limiterEventScavengeAssist)) { matchᴛ1 = true;
                assistTime += duration;
            }
            else if (exprᴛ1 == limiterEventNone) {
                break;
            }
            else { /* default: */
                @throw("invalid limiter event type found"u8);
            }

        }
        releasem(mp);
    }
    // Compute total GC time.
    var windowGCTime = assistTime;
    if (l.gcEnabled) {
        windowGCTime += ((int64)(((float64)windowTotalTime) * gcBackgroundUtilization));
    }
    // Subtract out all idle time from the total time. Do this after computing
    // GC time, because the background utilization is dependent on the *real*
    // total time, not the total time after idle time is subtracted.
    //
    // Idle time is counted as any time that a P is on the P idle list plus idle mark
    // time. Idle mark workers soak up time that the application spends idle.
    //
    // On a heavily undersubscribed system, any additional idle time can skew GC CPU
    // utilization, because the GC might be executing continuously and thrashing,
    // yet the CPU utilization with respect to GOMAXPROCS will be quite low, so
    // the limiter fails to turn on. By subtracting idle time, we're removing time that
    // we know the application was idle giving a more accurate picture of whether
    // the GC is thrashing.
    //
    // Note that this can cause the limiter to turn on even if it's not needed. For
    // instance, on a system with 32 Ps but only 1 running goroutine, each GC will have
    // 8 dedicated GC workers. Assuming the GC cycle is half mark phase and half sweep
    // phase, then the GC CPU utilization over that cycle, with idle time removed, will
    // be 8/(8+2) = 80%. Even though the limiter turns on, though, assist should be
    // unnecessary, as the GC has way more CPU time to outpace the 1 goroutine that's
    // running.
    windowTotalTime -= idleTime;
    l.accumulate(windowTotalTime - windowGCTime, windowGCTime);
}

// accumulate adds time to the bucket and signals whether the limiter is enabled.
//
// This is an internal function that deals just with the bucket. Prefer update.
// l.lock must be held.
[GoRecv] internal static void accumulate(this ref gcCPULimiterState l, int64 mutatorTime, int64 gcTime) {
    var headroom = l.bucket.capacity - l.bucket.fill;
    var enabled = headroom == 0;
    // Let's be careful about three things here:
    // 1. The addition and subtraction, for the invariants.
    // 2. Overflow.
    // 3. Excessive mutation of l.enabled, which is accessed
    //    by all assists, potentially more than once.
    var change = gcTime - mutatorTime;
    // Handle limiting case.
    if (change > 0 && headroom <= ((uint64)change)) {
        l.overflow += ((uint64)change) - headroom;
        l.bucket.fill = l.bucket.capacity;
        if (!enabled) {
            l.enabled.Store(true);
            l.lastEnabledCycle.Store(memstats.numgc + 1);
        }
        return;
    }
    // Handle non-limiting cases.
    if (change < 0 && l.bucket.fill <= ((uint64)(-change))){
        // Bucket emptied.
        l.bucket.fill = 0;
    } else {
        // All other cases.
        l.bucket.fill -= ((uint64)(-change));
    }
    if (change != 0 && enabled) {
        l.enabled.Store(false);
    }
}

// tryLock attempts to lock l. Returns true on success.
[GoRecv] internal static bool tryLock(this ref gcCPULimiterState l) {
    return l.@lock.CompareAndSwap(0, 1);
}

// unlock releases the lock on l. Must be called if tryLock returns true.
[GoRecv] internal static void unlock(this ref gcCPULimiterState l) {
    var old = l.@lock.Swap(0);
    if (old != 1) {
        @throw("double unlock"u8);
    }
}

// capacityPerProc is the limiter's bucket capacity for each P in GOMAXPROCS.
internal static readonly UntypedFloat capacityPerProc = 1e+09; // 1 second in nanoseconds

// resetCapacity updates the capacity based on GOMAXPROCS. Must not be called
// while the GC is enabled.
//
// It is safe to call concurrently with other operations.
[GoRecv] internal static void resetCapacity(this ref gcCPULimiterState l, int64 now, int32 nprocs) {
    if (!l.tryLock()) {
        // This must happen during a STW, so we can't fail to acquire the lock.
        // If we did, something went wrong. Throw.
        @throw("failed to acquire lock to reset capacity"u8);
    }
    // Flush the rest of the time for this period.
    l.updateLocked(now);
    l.nprocs = nprocs;
    l.bucket.capacity = ((uint64)nprocs) * capacityPerProc;
    if (l.bucket.fill > l.bucket.capacity){
        l.bucket.fill = l.bucket.capacity;
        l.enabled.Store(true);
        l.lastEnabledCycle.Store(memstats.numgc + 1);
    } else 
    if (l.bucket.fill < l.bucket.capacity) {
        l.enabled.Store(false);
    }
    l.unlock();
}

[GoType("num:uint8")] partial struct limiterEventType;

internal static readonly limiterEventType limiterEventNone = /* iota */ 0;          // None of the following events.
internal static readonly limiterEventType limiterEventIdleMarkWork = 1;  // Refers to an idle mark worker (see gcMarkWorkerMode).
internal static readonly limiterEventType limiterEventMarkAssist = 2;    // Refers to mark assist (see gcAssistAlloc).
internal static readonly limiterEventType limiterEventScavengeAssist = 3; // Refers to a scavenge assist (see allocSpan).
internal static readonly limiterEventType limiterEventIdle = 4;          // Refers to time a P spent on the idle list.
internal static readonly UntypedInt limiterEventBits = 3;

// limiterEventTypeMask is a mask for the bits in p.limiterEventStart that represent
// the event type. The rest of the bits of that field represent a timestamp.
internal const uint64 limiterEventTypeMask = /* uint64((1<<limiterEventBits)-1) << (64 - limiterEventBits) */ 16140901064495857664;

internal static readonly limiterEventStamp limiterEventStampNone = /* limiterEventStamp(0) */ 0;

[GoType("num:uint64")] partial struct limiterEventStamp;

// makeLimiterEventStamp creates a new stamp from the event type and the current timestamp.
internal static limiterEventStamp makeLimiterEventStamp(limiterEventType typ, int64 now) {
    return ((limiterEventStamp)((uint64)(((uint64)typ) << (int)((64 - limiterEventBits)) | ((uint64)(((uint64)now) & ~limiterEventTypeMask)))));
}

// duration computes the difference between now and the start time stored in the stamp.
//
// Returns 0 if the difference is negative, which may happen if now is stale or if the
// before and after timestamps cross a 2^(64-limiterEventBits) boundary.
internal static int64 duration(this limiterEventStamp s, int64 now) {
    // The top limiterEventBits bits of the timestamp are derived from the current time
    // when computing a duration.
    var start = ((int64)((uint64)(((uint64)(((uint64)now) & limiterEventTypeMask)) | ((uint64)(((uint64)s) & ~limiterEventTypeMask)))));
    if (now < start) {
        return 0;
    }
    return now - start;
}

// type extracts the event type from the stamp.
internal static limiterEventType typ(this limiterEventStamp s) {
    return ((limiterEventType)(s >> (int)((64 - limiterEventBits))));
}

// limiterEvent represents tracking state for an event tracked by the GC CPU limiter.
[GoType] partial struct limiterEvent {
    internal @internal.runtime.atomic_package.Uint64 stamp; // Stores a limiterEventStamp.
}

// start begins tracking a new limiter event of the current type. If an event
// is already in flight, then a new event cannot begin because the current time is
// already being attributed to that event. In this case, this function returns false.
// Otherwise, it returns true.
//
// The caller must be non-preemptible until at least stop is called or this function
// returns false. Because this is trying to measure "on-CPU" time of some event, getting
// scheduled away during it can mean that whatever we're measuring isn't a reflection
// of "on-CPU" time. The OS could deschedule us at any time, but we want to maintain as
// close of an approximation as we can.
[GoRecv] internal static bool start(this ref limiterEvent e, limiterEventType typ, int64 now) {
    if (((limiterEventStamp)e.stamp.Load()).typ() != limiterEventNone) {
        return false;
    }
    e.stamp.Store(((uint64)makeLimiterEventStamp(typ, now)));
    return true;
}

// consume acquires the partial event CPU time from any in-flight event.
// It achieves this by storing the current time as the new event time.
//
// Returns the type of the in-flight event, as well as how long it's currently been
// executing for. Returns limiterEventNone if no event is active.
[GoRecv] internal static (limiterEventType typ, int64 duration) consume(this ref limiterEvent e, int64 now) {
    limiterEventType typ = default!;
    int64 duration = default!;

    // Read the limiter event timestamp and update it to now.
    while (ᐧ) {
        var old = ((limiterEventStamp)e.stamp.Load());
        typ = old.typ();
        if (typ == limiterEventNone) {
            // There's no in-flight event, so just push that up.
            return (typ, duration);
        }
        duration = old.duration(now);
        if (duration == 0) {
            // We might have a stale now value, or this crossed the
            // 2^(64-limiterEventBits) boundary in the clock readings.
            // Just ignore it.
            return (limiterEventNone, 0);
        }
        var @new = makeLimiterEventStamp(typ, now);
        if (e.stamp.CompareAndSwap(((uint64)old), ((uint64)@new))) {
            break;
        }
    }
    return (typ, duration);
}

// stop stops the active limiter event. Throws if the
//
// The caller must be non-preemptible across the event. See start as to why.
[GoRecv] internal static void stop(this ref limiterEvent e, limiterEventType typ, int64 now) {
    limiterEventStamp stamp = default!;
    while (ᐧ) {
        stamp = ((limiterEventStamp)e.stamp.Load());
        if (stamp.typ() != typ) {
            print("runtime: want=", typ, " got=", stamp.typ(), "\n");
            @throw("limiterEvent.stop: found wrong event in p's limiter event slot"u8);
        }
        if (e.stamp.CompareAndSwap(((uint64)stamp), ((uint64)limiterEventStampNone))) {
            break;
        }
    }
    var duration = stamp.duration(now);
    if (duration == 0) {
        // It's possible that we're missing time because we crossed a
        // 2^(64-limiterEventBits) boundary between the start and end.
        // In this case, we're dropping that information. This is OK because
        // at worst it'll cause a transient hiccup that will quickly resolve
        // itself as all new timestamps begin on the other side of the boundary.
        // Such a hiccup should be incredibly rare.
        return;
    }
    // Account for the event.
    var exprᴛ1 = typ;
    var matchᴛ1 = false;
    if (exprᴛ1 == limiterEventIdleMarkWork) { matchᴛ1 = true;
        gcCPULimiter.addIdleTime(duration);
    }
    else if (exprᴛ1 == limiterEventIdle) { matchᴛ1 = true;
        gcCPULimiter.addIdleTime(duration);
        sched.idleTime.Add(duration);
    }
    else if (exprᴛ1 == limiterEventMarkAssist) { matchᴛ1 = true;
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && exprᴛ1 == limiterEventScavengeAssist)) {
        gcCPULimiter.addAssistTime(duration);
    }
    else { /* default: */
        @throw("limiterEvent.stop: invalid limiter event type found"u8);
    }

}

} // end runtime_package
