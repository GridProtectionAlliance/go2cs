// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Time-related runtime and pieces of package time.

// package runtime -- go2cs converted at 2020 October 09 04:48:58 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\time.go
using atomic = go.runtime.@internal.atomic_package;
using sys = go.runtime.@internal.sys_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class runtime_package
    {
        // Package time knows the layout of this structure.
        // If this struct changes, adjust ../time/sleep.go:/runtimeTimer.
        private partial struct timer
        {
            public puintptr pp; // Timer wakes up at when, and then at when+period, ... (period > 0 only)
// each time calling f(arg, now) in the timer goroutine, so f must be
// a well-behaved function and not block.
            public long when;
            public long period;
            public Action<object, System.UIntPtr> f;
            public System.UIntPtr seq; // What to set the when field to in timerModifiedXX status.
            public long nextwhen; // The status field holds one of the values below.
            public uint status;
        }

        // Code outside this file has to be careful in using a timer value.
        //
        // The pp, status, and nextwhen fields may only be used by code in this file.
        //
        // Code that creates a new timer value can set the when, period, f,
        // arg, and seq fields.
        // A new timer value may be passed to addtimer (called by time.startTimer).
        // After doing that no fields may be touched.
        //
        // An active timer (one that has been passed to addtimer) may be
        // passed to deltimer (time.stopTimer), after which it is no longer an
        // active timer. It is an inactive timer.
        // In an inactive timer the period, f, arg, and seq fields may be modified,
        // but not the when field.
        // It's OK to just drop an inactive timer and let the GC collect it.
        // It's not OK to pass an inactive timer to addtimer.
        // Only newly allocated timer values may be passed to addtimer.
        //
        // An active timer may be passed to modtimer. No fields may be touched.
        // It remains an active timer.
        //
        // An inactive timer may be passed to resettimer to turn into an
        // active timer with an updated when field.
        // It's OK to pass a newly allocated timer value to resettimer.
        //
        // Timer operations are addtimer, deltimer, modtimer, resettimer,
        // cleantimers, adjusttimers, and runtimer.
        //
        // We don't permit calling addtimer/deltimer/modtimer/resettimer simultaneously,
        // but adjusttimers and runtimer can be called at the same time as any of those.
        //
        // Active timers live in heaps attached to P, in the timers field.
        // Inactive timers live there too temporarily, until they are removed.
        //
        // addtimer:
        //   timerNoStatus   -> timerWaiting
        //   anything else   -> panic: invalid value
        // deltimer:
        //   timerWaiting         -> timerModifying -> timerDeleted
        //   timerModifiedEarlier -> timerModifying -> timerDeleted
        //   timerModifiedLater   -> timerModifying -> timerDeleted
        //   timerNoStatus        -> do nothing
        //   timerDeleted         -> do nothing
        //   timerRemoving        -> do nothing
        //   timerRemoved         -> do nothing
        //   timerRunning         -> wait until status changes
        //   timerMoving          -> wait until status changes
        //   timerModifying       -> wait until status changes
        // modtimer:
        //   timerWaiting    -> timerModifying -> timerModifiedXX
        //   timerModifiedXX -> timerModifying -> timerModifiedYY
        //   timerNoStatus   -> timerModifying -> timerWaiting
        //   timerRemoved    -> timerModifying -> timerWaiting
        //   timerDeleted    -> timerModifying -> timerModifiedXX
        //   timerRunning    -> wait until status changes
        //   timerMoving     -> wait until status changes
        //   timerRemoving   -> wait until status changes
        //   timerModifying  -> wait until status changes
        // cleantimers (looks in P's timer heap):
        //   timerDeleted    -> timerRemoving -> timerRemoved
        //   timerModifiedXX -> timerMoving -> timerWaiting
        // adjusttimers (looks in P's timer heap):
        //   timerDeleted    -> timerRemoving -> timerRemoved
        //   timerModifiedXX -> timerMoving -> timerWaiting
        // runtimer (looks in P's timer heap):
        //   timerNoStatus   -> panic: uninitialized timer
        //   timerWaiting    -> timerWaiting or
        //   timerWaiting    -> timerRunning -> timerNoStatus or
        //   timerWaiting    -> timerRunning -> timerWaiting
        //   timerModifying  -> wait until status changes
        //   timerModifiedXX -> timerMoving -> timerWaiting
        //   timerDeleted    -> timerRemoving -> timerRemoved
        //   timerRunning    -> panic: concurrent runtimer calls
        //   timerRemoved    -> panic: inconsistent timer heap
        //   timerRemoving   -> panic: inconsistent timer heap
        //   timerMoving     -> panic: inconsistent timer heap

        // Values for the timer status field.
 
        // Timer has no status set yet.
        private static readonly var timerNoStatus = iota; 

        // Waiting for timer to fire.
        // The timer is in some P's heap.
        private static readonly var timerWaiting = 0; 

        // Running the timer function.
        // A timer will only have this status briefly.
        private static readonly var timerRunning = 1; 

        // The timer is deleted and should be removed.
        // It should not be run, but it is still in some P's heap.
        private static readonly var timerDeleted = 2; 

        // The timer is being removed.
        // The timer will only have this status briefly.
        private static readonly var timerRemoving = 3; 

        // The timer has been stopped.
        // It is not in any P's heap.
        private static readonly var timerRemoved = 4; 

        // The timer is being modified.
        // The timer will only have this status briefly.
        private static readonly var timerModifying = 5; 

        // The timer has been modified to an earlier time.
        // The new when value is in the nextwhen field.
        // The timer is in some P's heap, possibly in the wrong place.
        private static readonly var timerModifiedEarlier = 6; 

        // The timer has been modified to the same or a later time.
        // The new when value is in the nextwhen field.
        // The timer is in some P's heap, possibly in the wrong place.
        private static readonly var timerModifiedLater = 7; 

        // The timer has been modified and is being moved.
        // The timer will only have this status briefly.
        private static readonly var timerMoving = 8;


        // maxWhen is the maximum value for timer's when field.
        private static readonly long maxWhen = (long)1L << (int)(63L) - 1L;

        // verifyTimers can be set to true to add debugging checks that the
        // timer heaps are valid.


        // verifyTimers can be set to true to add debugging checks that the
        // timer heaps are valid.
        private static readonly var verifyTimers = false;

        // Package time APIs.
        // Godoc uses the comments in package time, not these.

        // time.now is implemented in assembly.

        // timeSleep puts the current goroutine to sleep for at least ns nanoseconds.
        //go:linkname timeSleep time.Sleep


        // Package time APIs.
        // Godoc uses the comments in package time, not these.

        // time.now is implemented in assembly.

        // timeSleep puts the current goroutine to sleep for at least ns nanoseconds.
        //go:linkname timeSleep time.Sleep
        private static void timeSleep(long ns)
        {
            if (ns <= 0L)
            {
                return ;
            }

            var gp = getg();
            var t = gp.timer;
            if (t == null)
            {
                t = @new<timer>();
                gp.timer = t;
            }

            t.f = goroutineReady;
            t.arg = gp;
            t.nextwhen = nanotime() + ns;
            gopark(resetForSleep, @unsafe.Pointer(t), waitReasonSleep, traceEvGoSleep, 1L);

        }

        // resetForSleep is called after the goroutine is parked for timeSleep.
        // We can't call resettimer in timeSleep itself because if this is a short
        // sleep and there are many goroutines then the P can wind up running the
        // timer function, goroutineReady, before the goroutine has been parked.
        private static bool resetForSleep(ptr<g> _addr_gp, unsafe.Pointer ut)
        {
            ref g gp = ref _addr_gp.val;

            var t = (timer.val)(ut);
            resettimer(_addr_t, t.nextwhen);
            return true;
        }

        // startTimer adds t to the timer heap.
        //go:linkname startTimer time.startTimer
        private static void startTimer(ptr<timer> _addr_t)
        {
            ref timer t = ref _addr_t.val;

            if (raceenabled)
            {
                racerelease(@unsafe.Pointer(t));
            }

            addtimer(_addr_t);

        }

        // stopTimer stops a timer.
        // It reports whether t was stopped before being run.
        //go:linkname stopTimer time.stopTimer
        private static bool stopTimer(ptr<timer> _addr_t)
        {
            ref timer t = ref _addr_t.val;

            return deltimer(_addr_t);
        }

        // resetTimer resets an inactive timer, adding it to the heap.
        //go:linkname resetTimer time.resetTimer
        // Reports whether the timer was modified before it was run.
        private static bool resetTimer(ptr<timer> _addr_t, long when)
        {
            ref timer t = ref _addr_t.val;

            if (raceenabled)
            {
                racerelease(@unsafe.Pointer(t));
            }

            return resettimer(_addr_t, when);

        }

        // modTimer modifies an existing timer.
        //go:linkname modTimer time.modTimer
        private static void modTimer(ptr<timer> _addr_t, long when, long period, Action<object, System.UIntPtr> f, object arg, System.UIntPtr seq)
        {
            ref timer t = ref _addr_t.val;

            modtimer(_addr_t, when, period, f, arg, seq);
        }

        // Go runtime.

        // Ready the goroutine arg.
        private static void goroutineReady(object arg, System.UIntPtr seq)
        {
            goready(arg._<ptr<g>>(), 0L);
        }

        // addtimer adds a timer to the current P.
        // This should only be called with a newly created timer.
        // That avoids the risk of changing the when field of a timer in some P's heap,
        // which could cause the heap to become unsorted.
        private static void addtimer(ptr<timer> _addr_t)
        {
            ref timer t = ref _addr_t.val;
 
            // when must never be negative; otherwise runtimer will overflow
            // during its delta calculation and never expire other runtime timers.
            if (t.when < 0L)
            {
                t.when = maxWhen;
            }

            if (t.status != timerNoStatus)
            {
                throw("addtimer called with initialized timer");
            }

            t.status = timerWaiting;

            var when = t.when;

            var pp = getg().m.p.ptr();
            lock(_addr_pp.timersLock);
            cleantimers(_addr_pp);
            doaddtimer(_addr_pp, _addr_t);
            unlock(_addr_pp.timersLock);

            wakeNetPoller(when);

        }

        // doaddtimer adds t to the current P's heap.
        // The caller must have locked the timers for pp.
        private static void doaddtimer(ptr<p> _addr_pp, ptr<timer> _addr_t)
        {
            ref p pp = ref _addr_pp.val;
            ref timer t = ref _addr_t.val;
 
            // Timers rely on the network poller, so make sure the poller
            // has started.
            if (netpollInited == 0L)
            {
                netpollGenericInit();
            }

            if (t.pp != 0L)
            {
                throw("doaddtimer: P already set in timer");
            }

            t.pp.set(pp);
            var i = len(pp.timers);
            pp.timers = append(pp.timers, t);
            siftupTimer(pp.timers, i);
            if (t == pp.timers[0L])
            {
                atomic.Store64(_addr_pp.timer0When, uint64(t.when));
            }

            atomic.Xadd(_addr_pp.numTimers, 1L);

        }

        // deltimer deletes the timer t. It may be on some other P, so we can't
        // actually remove it from the timers heap. We can only mark it as deleted.
        // It will be removed in due course by the P whose heap it is on.
        // Reports whether the timer was removed before it was run.
        private static bool deltimer(ptr<timer> _addr_t)
        {
            ref timer t = ref _addr_t.val;

            while (true)
            {
                {
                    var s = atomic.Load(_addr_t.status);


                    if (s == timerWaiting || s == timerModifiedLater) 
                        // Prevent preemption while the timer is in timerModifying.
                        // This could lead to a self-deadlock. See #38070.
                        var mp = acquirem();
                        if (atomic.Cas(_addr_t.status, s, timerModifying))
                        { 
                            // Must fetch t.pp before changing status,
                            // as cleantimers in another goroutine
                            // can clear t.pp of a timerDeleted timer.
                            var tpp = t.pp.ptr();
                            if (!atomic.Cas(_addr_t.status, timerModifying, timerDeleted))
                            {
                                badTimer();
                            }

                            releasem(mp);
                            atomic.Xadd(_addr_tpp.deletedTimers, 1L); 
                            // Timer was not yet run.
                            return true;

                        }
                        else
                        {
                            releasem(mp);
                        }

                    else if (s == timerModifiedEarlier) 
                        // Prevent preemption while the timer is in timerModifying.
                        // This could lead to a self-deadlock. See #38070.
                        mp = acquirem();
                        if (atomic.Cas(_addr_t.status, s, timerModifying))
                        { 
                            // Must fetch t.pp before setting status
                            // to timerDeleted.
                            tpp = t.pp.ptr();
                            atomic.Xadd(_addr_tpp.adjustTimers, -1L);
                            if (!atomic.Cas(_addr_t.status, timerModifying, timerDeleted))
                            {
                                badTimer();
                            }

                            releasem(mp);
                            atomic.Xadd(_addr_tpp.deletedTimers, 1L); 
                            // Timer was not yet run.
                            return true;

                        }
                        else
                        {
                            releasem(mp);
                        }

                    else if (s == timerDeleted || s == timerRemoving || s == timerRemoved) 
                        // Timer was already run.
                        return false;
                    else if (s == timerRunning || s == timerMoving) 
                        // The timer is being run or moved, by a different P.
                        // Wait for it to complete.
                        osyield();
                    else if (s == timerNoStatus) 
                        // Removing timer that was never added or
                        // has already been run. Also see issue 21874.
                        return false;
                    else if (s == timerModifying) 
                        // Simultaneous calls to deltimer and modtimer.
                        // Wait for the other call to complete.
                        osyield();
                    else 
                        badTimer();

                }

            }


        }

        // dodeltimer removes timer i from the current P's heap.
        // We are locked on the P when this is called.
        // It reports whether it saw no problems due to races.
        // The caller must have locked the timers for pp.
        private static void dodeltimer(ptr<p> _addr_pp, long i)
        {
            ref p pp = ref _addr_pp.val;

            {
                var t = pp.timers[i];

                if (t.pp.ptr() != pp)
                {
                    throw("dodeltimer: wrong P");
                }
                else
                {
                    t.pp = 0L;
                }

            }

            var last = len(pp.timers) - 1L;
            if (i != last)
            {
                pp.timers[i] = pp.timers[last];
            }

            pp.timers[last] = null;
            pp.timers = pp.timers[..last];
            if (i != last)
            { 
                // Moving to i may have moved the last timer to a new parent,
                // so sift up to preserve the heap guarantee.
                siftupTimer(pp.timers, i);
                siftdownTimer(pp.timers, i);

            }

            if (i == 0L)
            {
                updateTimer0When(_addr_pp);
            }

            atomic.Xadd(_addr_pp.numTimers, -1L);

        }

        // dodeltimer0 removes timer 0 from the current P's heap.
        // We are locked on the P when this is called.
        // It reports whether it saw no problems due to races.
        // The caller must have locked the timers for pp.
        private static void dodeltimer0(ptr<p> _addr_pp)
        {
            ref p pp = ref _addr_pp.val;

            {
                var t = pp.timers[0L];

                if (t.pp.ptr() != pp)
                {
                    throw("dodeltimer0: wrong P");
                }
                else
                {
                    t.pp = 0L;
                }

            }

            var last = len(pp.timers) - 1L;
            if (last > 0L)
            {
                pp.timers[0L] = pp.timers[last];
            }

            pp.timers[last] = null;
            pp.timers = pp.timers[..last];
            if (last > 0L)
            {
                siftdownTimer(pp.timers, 0L);
            }

            updateTimer0When(_addr_pp);
            atomic.Xadd(_addr_pp.numTimers, -1L);

        }

        // modtimer modifies an existing timer.
        // This is called by the netpoll code or time.Ticker.Reset.
        // Reports whether the timer was modified before it was run.
        private static bool modtimer(ptr<timer> _addr_t, long when, long period, Action<object, System.UIntPtr> f, object arg, System.UIntPtr seq)
        {
            ref timer t = ref _addr_t.val;

            if (when < 0L)
            {
                when = maxWhen;
            }

            var status = uint32(timerNoStatus);
            var wasRemoved = false;
            bool pending = default;
            ptr<m> mp;
loop:

            while (true)
            {
                status = atomic.Load(_addr_t.status);


                if (status == timerWaiting || status == timerModifiedEarlier || status == timerModifiedLater) 
                    // Prevent preemption while the timer is in timerModifying.
                    // This could lead to a self-deadlock. See #38070.
                    mp = acquirem();
                    if (atomic.Cas(_addr_t.status, status, timerModifying))
                    {
                        pending = true; // timer not yet run
                        _breakloop = true;
                        break;
                    }

                    releasem(mp);
                else if (status == timerNoStatus || status == timerRemoved) 
                    // Prevent preemption while the timer is in timerModifying.
                    // This could lead to a self-deadlock. See #38070.
                    mp = acquirem(); 

                    // Timer was already run and t is no longer in a heap.
                    // Act like addtimer.
                    if (atomic.Cas(_addr_t.status, status, timerModifying))
                    {
                        wasRemoved = true;
                        pending = false; // timer already run or stopped
                        _breakloop = true;
                        break;
                    }

                    releasem(mp);
                else if (status == timerDeleted) 
                    // Prevent preemption while the timer is in timerModifying.
                    // This could lead to a self-deadlock. See #38070.
                    mp = acquirem();
                    if (atomic.Cas(_addr_t.status, status, timerModifying))
                    {
                        atomic.Xadd(_addr_t.pp.ptr().deletedTimers, -1L);
                        pending = false; // timer already stopped
                        _breakloop = true;
                        break;
                    }

                    releasem(mp);
                else if (status == timerRunning || status == timerRemoving || status == timerMoving) 
                    // The timer is being run or moved, by a different P.
                    // Wait for it to complete.
                    osyield();
                else if (status == timerModifying) 
                    // Multiple simultaneous calls to modtimer.
                    // Wait for the other call to complete.
                    osyield();
                else 
                    badTimer();

            }

            t.period = period;
            t.f = f;
            t.arg = arg;
            t.seq = seq;

            if (wasRemoved)
            {
                t.when = when;
                var pp = getg().m.p.ptr();
                lock(_addr_pp.timersLock);
                doaddtimer(_addr_pp, _addr_t);
                unlock(_addr_pp.timersLock);
                if (!atomic.Cas(_addr_t.status, timerModifying, timerWaiting))
                {
                    badTimer();
                }

                releasem(mp);
                wakeNetPoller(when);

            }
            else
            { 
                // The timer is in some other P's heap, so we can't change
                // the when field. If we did, the other P's heap would
                // be out of order. So we put the new when value in the
                // nextwhen field, and let the other P set the when field
                // when it is prepared to resort the heap.
                t.nextwhen = when;

                var newStatus = uint32(timerModifiedLater);
                if (when < t.when)
                {
                    newStatus = timerModifiedEarlier;
                } 

                // Update the adjustTimers field.  Subtract one if we
                // are removing a timerModifiedEarlier, add one if we
                // are adding a timerModifiedEarlier.
                var adjust = int32(0L);
                if (status == timerModifiedEarlier)
                {
                    adjust--;
                }

                if (newStatus == timerModifiedEarlier)
                {
                    adjust++;
                }

                if (adjust != 0L)
                {
                    atomic.Xadd(_addr_t.pp.ptr().adjustTimers, adjust);
                } 

                // Set the new status of the timer.
                if (!atomic.Cas(_addr_t.status, timerModifying, newStatus))
                {
                    badTimer();
                }

                releasem(mp); 

                // If the new status is earlier, wake up the poller.
                if (newStatus == timerModifiedEarlier)
                {
                    wakeNetPoller(when);
                }

            }

            return pending;

        }

        // resettimer resets the time when a timer should fire.
        // If used for an inactive timer, the timer will become active.
        // This should be called instead of addtimer if the timer value has been,
        // or may have been, used previously.
        // Reports whether the timer was modified before it was run.
        private static bool resettimer(ptr<timer> _addr_t, long when)
        {
            ref timer t = ref _addr_t.val;

            return modtimer(_addr_t, when, t.period, t.f, t.arg, t.seq);
        }

        // cleantimers cleans up the head of the timer queue. This speeds up
        // programs that create and delete timers; leaving them in the heap
        // slows down addtimer. Reports whether no timer problems were found.
        // The caller must have locked the timers for pp.
        private static void cleantimers(ptr<p> _addr_pp)
        {
            ref p pp = ref _addr_pp.val;

            var gp = getg();
            while (true)
            {
                if (len(pp.timers) == 0L)
                {
                    return ;
                } 

                // This loop can theoretically run for a while, and because
                // it is holding timersLock it cannot be preempted.
                // If someone is trying to preempt us, just return.
                // We can clean the timers later.
                if (gp.preemptStop)
                {
                    return ;
                }

                var t = pp.timers[0L];
                if (t.pp.ptr() != pp)
                {
                    throw("cleantimers: bad p");
                }

                {
                    var s = atomic.Load(_addr_t.status);


                    if (s == timerDeleted) 
                        if (!atomic.Cas(_addr_t.status, s, timerRemoving))
                        {
                            continue;
                        }

                        dodeltimer0(_addr_pp);
                        if (!atomic.Cas(_addr_t.status, timerRemoving, timerRemoved))
                        {
                            badTimer();
                        }

                        atomic.Xadd(_addr_pp.deletedTimers, -1L);
                    else if (s == timerModifiedEarlier || s == timerModifiedLater) 
                        if (!atomic.Cas(_addr_t.status, s, timerMoving))
                        {
                            continue;
                        } 
                        // Now we can change the when field.
                        t.when = t.nextwhen; 
                        // Move t to the right position.
                        dodeltimer0(_addr_pp);
                        doaddtimer(_addr_pp, _addr_t);
                        if (s == timerModifiedEarlier)
                        {
                            atomic.Xadd(_addr_pp.adjustTimers, -1L);
                        }

                        if (!atomic.Cas(_addr_t.status, timerMoving, timerWaiting))
                        {
                            badTimer();
                        }

                    else 
                        // Head of timers does not need adjustment.
                        return ;

                }

            }


        }

        // moveTimers moves a slice of timers to pp. The slice has been taken
        // from a different P.
        // This is currently called when the world is stopped, but the caller
        // is expected to have locked the timers for pp.
        private static void moveTimers(ptr<p> _addr_pp, slice<ptr<timer>> timers)
        {
            ref p pp = ref _addr_pp.val;

            foreach (var (_, t) in timers)
            {
loop:
                while (true)
                {
                    {
                        var s = atomic.Load(_addr_t.status);


                        if (s == timerWaiting) 
                            t.pp = 0L;
                            doaddtimer(_addr_pp, _addr_t);
                            _breakloop = true;
                            break;
                        else if (s == timerModifiedEarlier || s == timerModifiedLater) 
                            if (!atomic.Cas(_addr_t.status, s, timerMoving))
                            {
                                continue;
                            }

                            t.when = t.nextwhen;
                            t.pp = 0L;
                            doaddtimer(_addr_pp, _addr_t);
                            if (!atomic.Cas(_addr_t.status, timerMoving, timerWaiting))
                            {
                                badTimer();
                            }

                            _breakloop = true;
                            break;
                        else if (s == timerDeleted) 
                            if (!atomic.Cas(_addr_t.status, s, timerRemoved))
                            {
                                continue;
                            }

                            t.pp = 0L; 
                            // We no longer need this timer in the heap.
                            _breakloop = true;
                            break;
                        else if (s == timerModifying) 
                            // Loop until the modification is complete.
                            osyield();
                        else if (s == timerNoStatus || s == timerRemoved) 
                            // We should not see these status values in a timers heap.
                            badTimer();
                        else if (s == timerRunning || s == timerRemoving || s == timerMoving) 
                            // Some other P thinks it owns this timer,
                            // which should not happen.
                            badTimer();
                        else 
                            badTimer();

                    }

                }

            }

        }

        // adjusttimers looks through the timers in the current P's heap for
        // any timers that have been modified to run earlier, and puts them in
        // the correct place in the heap. While looking for those timers,
        // it also moves timers that have been modified to run later,
        // and removes deleted timers. The caller must have locked the timers for pp.
        private static void adjusttimers(ptr<p> _addr_pp)
        {
            ref p pp = ref _addr_pp.val;

            if (len(pp.timers) == 0L)
            {
                return ;
            }

            if (atomic.Load(_addr_pp.adjustTimers) == 0L)
            {
                if (verifyTimers)
                {
                    verifyTimerHeap(_addr_pp);
                }

                return ;

            }

            slice<ptr<timer>> moved = default;
loop:

            for (long i = 0L; i < len(pp.timers); i++)
            {
                var t = pp.timers[i];
                if (t.pp.ptr() != pp)
                {
                    throw("adjusttimers: bad p");
                }

                {
                    var s = atomic.Load(_addr_t.status);


                    if (s == timerDeleted) 
                        if (atomic.Cas(_addr_t.status, s, timerRemoving))
                        {
                            dodeltimer(_addr_pp, i);
                            if (!atomic.Cas(_addr_t.status, timerRemoving, timerRemoved))
                            {
                                badTimer();
                            }

                            atomic.Xadd(_addr_pp.deletedTimers, -1L); 
                            // Look at this heap position again.
                            i--;

                        }

                    else if (s == timerModifiedEarlier || s == timerModifiedLater) 
                        if (atomic.Cas(_addr_t.status, s, timerMoving))
                        { 
                            // Now we can change the when field.
                            t.when = t.nextwhen; 
                            // Take t off the heap, and hold onto it.
                            // We don't add it back yet because the
                            // heap manipulation could cause our
                            // loop to skip some other timer.
                            dodeltimer(_addr_pp, i);
                            moved = append(moved, t);
                            if (s == timerModifiedEarlier)
                            {
                                {
                                    var n = atomic.Xadd(_addr_pp.adjustTimers, -1L);

                                    if (int32(n) <= 0L)
                                    {
                                        _breakloop = true;
                                        break;
                                    }

                                }

                            } 
                            // Look at this heap position again.
                            i--;

                        }

                    else if (s == timerNoStatus || s == timerRunning || s == timerRemoving || s == timerRemoved || s == timerMoving) 
                        badTimer();
                    else if (s == timerWaiting)                     else if (s == timerModifying) 
                        // Check again after modification is complete.
                        osyield();
                        i--;
                    else 
                        badTimer();

                }

            }

            if (len(moved) > 0L)
            {
                addAdjustedTimers(_addr_pp, moved);
            }

            if (verifyTimers)
            {
                verifyTimerHeap(_addr_pp);
            }

        }

        // addAdjustedTimers adds any timers we adjusted in adjusttimers
        // back to the timer heap.
        private static void addAdjustedTimers(ptr<p> _addr_pp, slice<ptr<timer>> moved)
        {
            ref p pp = ref _addr_pp.val;

            foreach (var (_, t) in moved)
            {
                doaddtimer(_addr_pp, _addr_t);
                if (!atomic.Cas(_addr_t.status, timerMoving, timerWaiting))
                {
                    badTimer();
                }

            }

        }

        // nobarrierWakeTime looks at P's timers and returns the time when we
        // should wake up the netpoller. It returns 0 if there are no timers.
        // This function is invoked when dropping a P, and must run without
        // any write barriers. Therefore, if there are any timers that needs
        // to be moved earlier, it conservatively returns the current time.
        // The netpoller M will wake up and adjust timers before sleeping again.
        //go:nowritebarrierrec
        private static long nobarrierWakeTime(ptr<p> _addr_pp)
        {
            ref p pp = ref _addr_pp.val;

            if (atomic.Load(_addr_pp.adjustTimers) > 0L)
            {
                return nanotime();
            }
            else
            {
                return int64(atomic.Load64(_addr_pp.timer0When));
            }

        }

        // runtimer examines the first timer in timers. If it is ready based on now,
        // it runs the timer and removes or updates it.
        // Returns 0 if it ran a timer, -1 if there are no more timers, or the time
        // when the first timer should run.
        // The caller must have locked the timers for pp.
        // If a timer is run, this will temporarily unlock the timers.
        //go:systemstack
        private static long runtimer(ptr<p> _addr_pp, long now)
        {
            ref p pp = ref _addr_pp.val;

            while (true)
            {
                var t = pp.timers[0L];
                if (t.pp.ptr() != pp)
                {
                    throw("runtimer: bad p");
                }

                {
                    var s = atomic.Load(_addr_t.status);


                    if (s == timerWaiting) 
                        if (t.when > now)
                        { 
                            // Not ready to run.
                            return t.when;

                        }

                        if (!atomic.Cas(_addr_t.status, s, timerRunning))
                        {
                            continue;
                        } 
                        // Note that runOneTimer may temporarily unlock
                        // pp.timersLock.
                        runOneTimer(_addr_pp, _addr_t, now);
                        return 0L;
                    else if (s == timerDeleted) 
                        if (!atomic.Cas(_addr_t.status, s, timerRemoving))
                        {
                            continue;
                        }

                        dodeltimer0(_addr_pp);
                        if (!atomic.Cas(_addr_t.status, timerRemoving, timerRemoved))
                        {
                            badTimer();
                        }

                        atomic.Xadd(_addr_pp.deletedTimers, -1L);
                        if (len(pp.timers) == 0L)
                        {
                            return -1L;
                        }

                    else if (s == timerModifiedEarlier || s == timerModifiedLater) 
                        if (!atomic.Cas(_addr_t.status, s, timerMoving))
                        {
                            continue;
                        }

                        t.when = t.nextwhen;
                        dodeltimer0(_addr_pp);
                        doaddtimer(_addr_pp, _addr_t);
                        if (s == timerModifiedEarlier)
                        {
                            atomic.Xadd(_addr_pp.adjustTimers, -1L);
                        }

                        if (!atomic.Cas(_addr_t.status, timerMoving, timerWaiting))
                        {
                            badTimer();
                        }

                    else if (s == timerModifying) 
                        // Wait for modification to complete.
                        osyield();
                    else if (s == timerNoStatus || s == timerRemoved) 
                        // Should not see a new or inactive timer on the heap.
                        badTimer();
                    else if (s == timerRunning || s == timerRemoving || s == timerMoving) 
                        // These should only be set when timers are locked,
                        // and we didn't do it.
                        badTimer();
                    else 
                        badTimer();

                }

            }


        }

        // runOneTimer runs a single timer.
        // The caller must have locked the timers for pp.
        // This will temporarily unlock the timers while running the timer function.
        //go:systemstack
        private static void runOneTimer(ptr<p> _addr_pp, ptr<timer> _addr_t, long now)
        {
            ref p pp = ref _addr_pp.val;
            ref timer t = ref _addr_t.val;

            if (raceenabled)
            {
                var ppcur = getg().m.p.ptr();
                if (ppcur.timerRaceCtx == 0L)
                {
                    ppcur.timerRaceCtx = racegostart(funcPC(runtimer) + sys.PCQuantum);
                }

                raceacquirectx(ppcur.timerRaceCtx, @unsafe.Pointer(t));

            }

            var f = t.f;
            var arg = t.arg;
            var seq = t.seq;

            if (t.period > 0L)
            { 
                // Leave in heap but adjust next time to fire.
                var delta = t.when - now;
                t.when += t.period * (1L + -delta / t.period);
                siftdownTimer(pp.timers, 0L);
                if (!atomic.Cas(_addr_t.status, timerRunning, timerWaiting))
                {
                    badTimer();
                }

                updateTimer0When(_addr_pp);

            }
            else
            { 
                // Remove from heap.
                dodeltimer0(_addr_pp);
                if (!atomic.Cas(_addr_t.status, timerRunning, timerNoStatus))
                {
                    badTimer();
                }

            }

            if (raceenabled)
            { 
                // Temporarily use the current P's racectx for g0.
                var gp = getg();
                if (gp.racectx != 0L)
                {
                    throw("runOneTimer: unexpected racectx");
                }

                gp.racectx = gp.m.p.ptr().timerRaceCtx;

            }

            unlock(_addr_pp.timersLock);

            f(arg, seq);

            lock(_addr_pp.timersLock);

            if (raceenabled)
            {
                gp = getg();
                gp.racectx = 0L;
            }

        }

        // clearDeletedTimers removes all deleted timers from the P's timer heap.
        // This is used to avoid clogging up the heap if the program
        // starts a lot of long-running timers and then stops them.
        // For example, this can happen via context.WithTimeout.
        //
        // This is the only function that walks through the entire timer heap,
        // other than moveTimers which only runs when the world is stopped.
        //
        // The caller must have locked the timers for pp.
        private static void clearDeletedTimers(ptr<p> _addr_pp)
        {
            ref p pp = ref _addr_pp.val;

            var cdel = int32(0L);
            var cearlier = int32(0L);
            long to = 0L;
            var changedHeap = false;
            var timers = pp.timers;
nextTimer: 

            // Set remaining slots in timers slice to nil,
            // so that the timer values can be garbage collected.
            foreach (var (_, t) in timers)
            {
                while (true)
                {
                    {
                        var s = atomic.Load(_addr_t.status);


                        if (s == timerWaiting) 
                            if (changedHeap)
                            {
                                timers[to] = t;
                                siftupTimer(timers, to);
                            }

                            to++;
                            _continuenextTimer = true;
                            break;
                        else if (s == timerModifiedEarlier || s == timerModifiedLater) 
                            if (atomic.Cas(_addr_t.status, s, timerMoving))
                            {
                                t.when = t.nextwhen;
                                timers[to] = t;
                                siftupTimer(timers, to);
                                to++;
                                changedHeap = true;
                                if (!atomic.Cas(_addr_t.status, timerMoving, timerWaiting))
                                {
                                    badTimer();
                                }

                                if (s == timerModifiedEarlier)
                                {
                                    cearlier++;
                                }

                                _continuenextTimer = true;
                                break;
                            }

                        else if (s == timerDeleted) 
                            if (atomic.Cas(_addr_t.status, s, timerRemoving))
                            {
                                t.pp = 0L;
                                cdel++;
                                if (!atomic.Cas(_addr_t.status, timerRemoving, timerRemoved))
                                {
                                    badTimer();
                                }

                                changedHeap = true;
                                _continuenextTimer = true;
                                break;
                            }

                        else if (s == timerModifying) 
                            // Loop until modification complete.
                            osyield();
                        else if (s == timerNoStatus || s == timerRemoved) 
                            // We should not see these status values in a timer heap.
                            badTimer();
                        else if (s == timerRunning || s == timerRemoving || s == timerMoving) 
                            // Some other P thinks it owns this timer,
                            // which should not happen.
                            badTimer();
                        else 
                            badTimer();

                    }

                }


            } 

            // Set remaining slots in timers slice to nil,
            // so that the timer values can be garbage collected.
            for (var i = to; i < len(timers); i++)
            {
                timers[i] = null;
            }


            atomic.Xadd(_addr_pp.deletedTimers, -cdel);
            atomic.Xadd(_addr_pp.numTimers, -cdel);
            atomic.Xadd(_addr_pp.adjustTimers, -cearlier);

            timers = timers[..to];
            pp.timers = timers;
            updateTimer0When(_addr_pp);

            if (verifyTimers)
            {
                verifyTimerHeap(_addr_pp);
            }

        }

        // verifyTimerHeap verifies that the timer heap is in a valid state.
        // This is only for debugging, and is only called if verifyTimers is true.
        // The caller must have locked the timers.
        private static void verifyTimerHeap(ptr<p> _addr_pp)
        {
            ref p pp = ref _addr_pp.val;

            foreach (var (i, t) in pp.timers)
            {
                if (i == 0L)
                { 
                    // First timer has no parent.
                    continue;

                } 

                // The heap is 4-ary. See siftupTimer and siftdownTimer.
                var p = (i - 1L) / 4L;
                if (t.when < pp.timers[p].when)
                {
                    print("bad timer heap at ", i, ": ", p, ": ", pp.timers[p].when, ", ", i, ": ", t.when, "\n");
                    throw("bad timer heap");
                }

            }
            {
                var numTimers = int(atomic.Load(_addr_pp.numTimers));

                if (len(pp.timers) != numTimers)
                {
                    println("timer heap len", len(pp.timers), "!= numTimers", numTimers);
                    throw("bad timer heap len");
                }

            }

        }

        // updateTimer0When sets the P's timer0When field.
        // The caller must have locked the timers for pp.
        private static void updateTimer0When(ptr<p> _addr_pp)
        {
            ref p pp = ref _addr_pp.val;

            if (len(pp.timers) == 0L)
            {
                atomic.Store64(_addr_pp.timer0When, 0L);
            }
            else
            {
                atomic.Store64(_addr_pp.timer0When, uint64(pp.timers[0L].when));
            }

        }

        // timeSleepUntil returns the time when the next timer should fire,
        // and the P that holds the timer heap that that timer is on.
        // This is only called by sysmon and checkdead.
        private static (long, ptr<p>) timeSleepUntil()
        {
            long _p0 = default;
            ptr<p> _p0 = default!;

            var next = int64(maxWhen);
            ptr<p> pret; 

            // Prevent allp slice changes. This is like retake.
            lock(_addr_allpLock);
            foreach (var (_, pp) in allp)
            {
                if (pp == null)
                { 
                    // This can happen if procresize has grown
                    // allp but not yet created new Ps.
                    continue;

                }

                var c = atomic.Load(_addr_pp.adjustTimers);
                if (c == 0L)
                {
                    var w = int64(atomic.Load64(_addr_pp.timer0When));
                    if (w != 0L && w < next)
                    {
                        next = w;
                        pret = pp;
                    }

                    continue;

                }

                lock(_addr_pp.timersLock);
                foreach (var (_, t) in pp.timers)
                {
                    {
                        var s = atomic.Load(_addr_t.status);


                        if (s == timerWaiting) 
                            if (t.when < next)
                            {
                                next = t.when;
                            }

                        else if (s == timerModifiedEarlier || s == timerModifiedLater) 
                            if (t.nextwhen < next)
                            {
                                next = t.nextwhen;
                            }

                            if (s == timerModifiedEarlier)
                            {
                                c--;
                            }


                    } 
                    // The timers are sorted, so we only have to check
                    // the first timer for each P, unless there are
                    // some timerModifiedEarlier timers. The number
                    // of timerModifiedEarlier timers is in the adjustTimers
                    // field, used to initialize c, above.
                    //
                    // We don't worry about cases like timerModifying.
                    // New timers can show up at any time,
                    // so this function is necessarily imprecise.
                    // Do a signed check here since we aren't
                    // synchronizing the read of pp.adjustTimers
                    // with the check of a timer status.
                    if (int32(c) <= 0L)
                    {
                        break;
                    }

                }
                unlock(_addr_pp.timersLock);

            }
            unlock(_addr_allpLock);

            return (next, _addr_pret!);

        }

        // Heap maintenance algorithms.
        // These algorithms check for slice index errors manually.
        // Slice index error can happen if the program is using racy
        // access to timers. We don't want to panic here, because
        // it will cause the program to crash with a mysterious
        // "panic holding locks" message. Instead, we panic while not
        // holding a lock.

        private static void siftupTimer(slice<ptr<timer>> t, long i)
        {
            if (i >= len(t))
            {
                badTimer();
            }

            var when = t[i].when;
            var tmp = t[i];
            while (i > 0L)
            {
                var p = (i - 1L) / 4L; // parent
                if (when >= t[p].when)
                {
                    break;
                }

                t[i] = t[p];
                i = p;

            }

            if (tmp != t[i])
            {
                t[i] = tmp;
            }

        }

        private static void siftdownTimer(slice<ptr<timer>> t, long i)
        {
            var n = len(t);
            if (i >= n)
            {
                badTimer();
            }

            var when = t[i].when;
            var tmp = t[i];
            while (true)
            {
                var c = i * 4L + 1L; // left child
                var c3 = c + 2L; // mid child
                if (c >= n)
                {
                    break;
                }

                var w = t[c].when;
                if (c + 1L < n && t[c + 1L].when < w)
                {
                    w = t[c + 1L].when;
                    c++;
                }

                if (c3 < n)
                {
                    var w3 = t[c3].when;
                    if (c3 + 1L < n && t[c3 + 1L].when < w3)
                    {
                        w3 = t[c3 + 1L].when;
                        c3++;
                    }

                    if (w3 < w)
                    {
                        w = w3;
                        c = c3;
                    }

                }

                if (w >= when)
                {
                    break;
                }

                t[i] = t[c];
                i = c;

            }

            if (tmp != t[i])
            {
                t[i] = tmp;
            }

        }

        // badTimer is called if the timer data structures have been corrupted,
        // presumably due to racy use by the program. We panic here rather than
        // panicing due to invalid slice access while holding locks.
        // See issue #25686.
        private static void badTimer()
        {
            throw("timer data corruption");
        }
    }
}
