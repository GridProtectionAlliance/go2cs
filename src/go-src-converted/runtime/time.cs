// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Time-related runtime and pieces of package time.

// package runtime -- go2cs converted at 2020 August 29 08:21:16 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\time.go
using sys = go.runtime.@internal.sys_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;
using System.Threading;

namespace go
{
    public static partial class runtime_package
    {
        // Package time knows the layout of this structure.
        // If this struct changes, adjust ../time/sleep.go:/runtimeTimer.
        // For GOOS=nacl, package syscall knows the layout of this structure.
        // If this struct changes, adjust ../syscall/net_nacl.go:/runtimeTimer.
        private partial struct timer
        {
            public ptr<timersBucket> tb; // the bucket the timer lives in
            public long i; // heap index

// Timer wakes up at when, and then at when+period, ... (period > 0 only)
// each time calling f(arg, now) in the timer goroutine, so f must be
// a well-behaved function and not block.
            public long when;
            public long period;
            public Action<object, System.UIntPtr> f;
            public System.UIntPtr seq;
        }

        // timersLen is the length of timers array.
        //
        // Ideally, this would be set to GOMAXPROCS, but that would require
        // dynamic reallocation
        //
        // The current value is a compromise between memory usage and performance
        // that should cover the majority of GOMAXPROCS values used in the wild.
        private static readonly long timersLen = 64L;

        // timers contains "per-P" timer heaps.
        //
        // Timers are queued into timersBucket associated with the current P,
        // so each P may work with its own timers independently of other P instances.
        //
        // Each timersBucket may be associated with multiple P
        // if GOMAXPROCS > timersLen.


        // timers contains "per-P" timer heaps.
        //
        // Timers are queued into timersBucket associated with the current P,
        // so each P may work with its own timers independently of other P instances.
        //
        // Each timersBucket may be associated with multiple P
        // if GOMAXPROCS > timersLen.
        private static var timers = default;

        private static ref timersBucket assignBucket(this ref timer t)
        {
            var id = uint8(getg().m.p.ptr().id) % timersLen;
            t.tb = ref timers[id].timersBucket;
            return t.tb;
        }

        //go:notinheap
        private partial struct timersBucket
        {
            public mutex @lock;
            public ptr<g> gp;
            public bool created;
            public bool sleeping;
            public bool rescheduling;
            public long sleepUntil;
            public note waitnote;
            public slice<ref timer> t;
        }

        // nacl fake time support - time in nanoseconds since 1970
        private static long faketime = default;

        // Package time APIs.
        // Godoc uses the comments in package time, not these.

        // time.now is implemented in assembly.

        // timeSleep puts the current goroutine to sleep for at least ns nanoseconds.
        //go:linkname timeSleep time.Sleep
        private static void timeSleep(long ns)
        {
            if (ns <= 0L)
            {
                return;
            }
            var gp = getg();
            var t = gp.timer;
            if (t == null)
            {
                t = @new<timer>();
                gp.timer = t;
            }
            t.Value = new timer();
            t.when = nanotime() + ns;
            t.f = goroutineReady;
            t.arg = gp;
            var tb = t.assignBucket();
            lock(ref tb.@lock);
            tb.addtimerLocked(t);
            goparkunlock(ref tb.@lock, "sleep", traceEvGoSleep, 2L);
        }

        // startTimer adds t to the timer heap.
        //go:linkname startTimer time.startTimer
        private static void startTimer(ref timer t)
        {
            if (raceenabled)
            {
                racerelease(@unsafe.Pointer(t));
            }
            addtimer(t);
        }

        // stopTimer removes t from the timer heap if it is there.
        // It returns true if t was removed, false if t wasn't even there.
        //go:linkname stopTimer time.stopTimer
        private static bool stopTimer(ref timer t)
        {
            return deltimer(t);
        }

        // Go runtime.

        // Ready the goroutine arg.
        private static void goroutineReady(object arg, System.UIntPtr seq)
        {
            goready(arg._<ref g>(), 0L);
        }

        private static void addtimer(ref timer t)
        {
            var tb = t.assignBucket();
            lock(ref tb.@lock);
            tb.addtimerLocked(t);
            unlock(ref tb.@lock);
        }

        // Add a timer to the heap and start or kick timerproc if the new timer is
        // earlier than any of the others.
        // Timers are locked.
        private static void addtimerLocked(this ref timersBucket tb, ref timer t)
        { 
            // when must never be negative; otherwise timerproc will overflow
            // during its delta calculation and never expire other runtime timers.
            if (t.when < 0L)
            {
                t.when = 1L << (int)(63L) - 1L;
            }
            t.i = len(tb.t);
            tb.t = append(tb.t, t);
            siftupTimer(tb.t, t.i);
            if (t.i == 0L)
            { 
                // siftup moved to top: new earliest deadline.
                if (tb.sleeping)
                {
                    tb.sleeping = false;
                    notewakeup(ref tb.waitnote);
                }
                if (tb.rescheduling)
                {
                    tb.rescheduling = false;
                    goready(tb.gp, 0L);
                }
            }
            if (!tb.created)
            {
                tb.created = true;
                go_(() => timerproc(tb));
            }
        }

        // Delete timer t from the heap.
        // Do not need to update the timerproc: if it wakes up early, no big deal.
        private static bool deltimer(ref timer t)
        {
            if (t.tb == null)
            { 
                // t.tb can be nil if the user created a timer
                // directly, without invoking startTimer e.g
                //    time.Ticker{C: c}
                // In this case, return early without any deletion.
                // See Issue 21874.
                return false;
            }
            var tb = t.tb;

            lock(ref tb.@lock); 
            // t may not be registered anymore and may have
            // a bogus i (typically 0, if generated by Go).
            // Verify it before proceeding.
            var i = t.i;
            var last = len(tb.t) - 1L;
            if (i < 0L || i > last || tb.t[i] != t)
            {
                unlock(ref tb.@lock);
                return false;
            }
            if (i != last)
            {
                tb.t[i] = tb.t[last];
                tb.t[i].i = i;
            }
            tb.t[last] = null;
            tb.t = tb.t[..last];
            if (i != last)
            {
                siftupTimer(tb.t, i);
                siftdownTimer(tb.t, i);
            }
            unlock(ref tb.@lock);
            return true;
        }

        // Timerproc runs the time-driven events.
        // It sleeps until the next event in the tb heap.
        // If addtimer inserts a new earlier event, it wakes timerproc early.
        private static void timerproc(ref timersBucket tb)
        {
            tb.gp = getg();
            while (true)
            {
                lock(ref tb.@lock);
                tb.sleeping = false;
                var now = nanotime();
                var delta = int64(-1L);
                while (true)
                {
                    if (len(tb.t) == 0L)
                    {
                        delta = -1L;
                        break;
                    }
                    var t = tb.t[0L];
                    delta = t.when - now;
                    if (delta > 0L)
                    {
                        break;
                    }
                    if (t.period > 0L)
                    { 
                        // leave in heap but adjust next time to fire
                        t.when += t.period * (1L + -delta / t.period);
                        siftdownTimer(tb.t, 0L);
                    }
                    else
                    { 
                        // remove from heap
                        var last = len(tb.t) - 1L;
                        if (last > 0L)
                        {
                            tb.t[0L] = tb.t[last];
                            tb.t[0L].i = 0L;
                        }
                        tb.t[last] = null;
                        tb.t = tb.t[..last];
                        if (last > 0L)
                        {
                            siftdownTimer(tb.t, 0L);
                        }
                        t.i = -1L; // mark as removed
                    }
                    var f = t.f;
                    var arg = t.arg;
                    var seq = t.seq;
                    unlock(ref tb.@lock);
                    if (raceenabled)
                    {
                        raceacquire(@unsafe.Pointer(t));
                    }
                    f(arg, seq);
                    lock(ref tb.@lock);
                }

                if (delta < 0L || faketime > 0L)
                { 
                    // No timers left - put goroutine to sleep.
                    tb.rescheduling = true;
                    goparkunlock(ref tb.@lock, "timer goroutine (idle)", traceEvGoBlock, 1L);
                    continue;
                } 
                // At least one timer pending. Sleep until then.
                tb.sleeping = true;
                tb.sleepUntil = now + delta;
                noteclear(ref tb.waitnote);
                unlock(ref tb.@lock);
                notetsleepg(ref tb.waitnote, delta);
            }

        }

        private static ref g timejump()
        {
            if (faketime == 0L)
            {
                return null;
            }
            {
                var i__prev1 = i;

                foreach (var (__i) in timers)
                {
                    i = __i;
                    lock(ref timers[i].@lock);
                }

                i = i__prev1;
            }

            var gp = timejumpLocked();
            {
                var i__prev1 = i;

                foreach (var (__i) in timers)
                {
                    i = __i;
                    unlock(ref timers[i].@lock);
                }

                i = i__prev1;
            }

            return gp;
        }

        private static ref g timejumpLocked()
        { 
            // Determine a timer bucket with minimum when.
            ref timer minT = default;
            foreach (var (i) in timers)
            {
                var tb = ref timers[i];
                if (!tb.created || len(tb.t) == 0L)
                {
                    continue;
                }
                var t = tb.t[0L];
                if (minT == null || t.when < minT.when)
                {
                    minT = t;
                }
            }
            if (minT == null || minT.when <= faketime)
            {
                return null;
            }
            faketime = minT.when;
            tb = minT.tb;
            if (!tb.rescheduling)
            {
                return null;
            }
            tb.rescheduling = false;
            return tb.gp;
        }

        private static long timeSleepUntil()
        {
            var next = int64(1L << (int)(63L) - 1L); 

            // Determine minimum sleepUntil across all the timer buckets.
            //
            // The function can not return a precise answer,
            // as another timer may pop in as soon as timers have been unlocked.
            // So lock the timers one by one instead of all at once.
            foreach (var (i) in timers)
            {
                var tb = ref timers[i];

                lock(ref tb.@lock);
                if (tb.sleeping && tb.sleepUntil < next)
                {
                    next = tb.sleepUntil;
                }
                unlock(ref tb.@lock);
            }
            return next;
        }

        // Heap maintenance algorithms.

        private static void siftupTimer(slice<ref timer> t, long i)
        {
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
                t[i].i = i;
                i = p;
            }

            if (tmp != t[i])
            {
                t[i] = tmp;
                t[i].i = i;
            }
        }

        private static void siftdownTimer(slice<ref timer> t, long i)
        {
            var n = len(t);
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
                t[i].i = i;
                i = c;
            }

            if (tmp != t[i])
            {
                t[i] = tmp;
                t[i].i = i;
            }
        }

        // Entry points for net, time to call nanotime.

        //go:linkname poll_runtimeNano internal/poll.runtimeNano
        private static long poll_runtimeNano()
        {
            return nanotime();
        }

        //go:linkname time_runtimeNano time.runtimeNano
        private static long time_runtimeNano()
        {
            return nanotime();
        }

        // Monotonic times are reported as offsets from startNano.
        // We initialize startNano to nanotime() - 1 so that on systems where
        // monotonic time resolution is fairly low (e.g. Windows 2008
        // which appears to have a default resolution of 15ms),
        // we avoid ever reporting a nanotime of 0.
        // (Callers may want to use 0 as "time not set".)
        private static long startNano = nanotime() - 1L;
    }
}
