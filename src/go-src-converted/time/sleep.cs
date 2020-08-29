// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package time -- go2cs converted at 2020 August 29 08:16:12 UTC
// import "time" ==> using time = go.time_package
// Original source: C:\Go\src\time\sleep.go

using static go.builtin;
using System;
using System.Threading;

namespace go
{
    public static partial class time_package
    {
        // Sleep pauses the current goroutine for at least the duration d.
        // A negative or zero duration causes Sleep to return immediately.
        public static void Sleep(Duration d)
;

        // runtimeNano returns the current value of the runtime clock in nanoseconds.
        private static long runtimeNano()
;

        // Interface to timers implemented in package runtime.
        // Must be in sync with ../runtime/time.go:/^type timer
        private partial struct runtimeTimer
        {
            public System.UIntPtr tb;
            public long i;
            public long when;
            public long period;
            public Action<object, System.UIntPtr> f; // NOTE: must not be closure
            public System.UIntPtr seq;
        }

        // when is a helper function for setting the 'when' field of a runtimeTimer.
        // It returns what the time will be, in nanoseconds, Duration d in the future.
        // If d is negative, it is ignored. If the returned value would be less than
        // zero because of an overflow, MaxInt64 is returned.
        private static long when(Duration d)
        {
            if (d <= 0L)
            {>>MARKER:FUNCTION_runtimeNano_BLOCK_PREFIX<<
                return runtimeNano();
            }
            var t = runtimeNano() + int64(d);
            if (t < 0L)
            {>>MARKER:FUNCTION_Sleep_BLOCK_PREFIX<<
                t = 1L << (int)(63L) - 1L; // math.MaxInt64
            }
            return t;
        }

        private static void startTimer(ref runtimeTimer _p0)
;
        private static bool stopTimer(ref runtimeTimer _p0)
;

        // The Timer type represents a single event.
        // When the Timer expires, the current time will be sent on C,
        // unless the Timer was created by AfterFunc.
        // A Timer must be created with NewTimer or AfterFunc.
        public partial struct Timer
        {
            public channel<Time> C;
            public runtimeTimer r;
        }

        // Stop prevents the Timer from firing.
        // It returns true if the call stops the timer, false if the timer has already
        // expired or been stopped.
        // Stop does not close the channel, to prevent a read from the channel succeeding
        // incorrectly.
        //
        // To prevent a timer created with NewTimer from firing after a call to Stop,
        // check the return value and drain the channel.
        // For example, assuming the program has not received from t.C already:
        //
        //     if !t.Stop() {
        //         <-t.C
        //     }
        //
        // This cannot be done concurrent to other receives from the Timer's
        // channel.
        //
        // For a timer created with AfterFunc(d, f), if t.Stop returns false, then the timer
        // has already expired and the function f has been started in its own goroutine;
        // Stop does not wait for f to complete before returning.
        // If the caller needs to know whether f is completed, it must coordinate
        // with f explicitly.
        private static bool Stop(this ref Timer _t) => func(_t, (ref Timer t, Defer _, Panic panic, Recover __) =>
        {>>MARKER:FUNCTION_stopTimer_BLOCK_PREFIX<<
            if (t.r.f == null)
            {>>MARKER:FUNCTION_startTimer_BLOCK_PREFIX<<
                panic("time: Stop called on uninitialized Timer");
            }
            return stopTimer(ref t.r);
        });

        // NewTimer creates a new Timer that will send
        // the current time on its channel after at least duration d.
        public static ref Timer NewTimer(Duration d)
        {
            var c = make_channel<Time>(1L);
            Timer t = ref new Timer(C:c,r:runtimeTimer{when:when(d),f:sendTime,arg:c,},);
            startTimer(ref t.r);
            return t;
        }

        // Reset changes the timer to expire after duration d.
        // It returns true if the timer had been active, false if the timer had
        // expired or been stopped.
        //
        // Resetting a timer must take care not to race with the send into t.C
        // that happens when the current timer expires.
        // If a program has already received a value from t.C, the timer is known
        // to have expired, and t.Reset can be used directly.
        // If a program has not yet received a value from t.C, however,
        // the timer must be stopped and—if Stop reports that the timer expired
        // before being stopped—the channel explicitly drained:
        //
        //     if !t.Stop() {
        //         <-t.C
        //     }
        //     t.Reset(d)
        //
        // This should not be done concurrent to other receives from the Timer's
        // channel.
        //
        // Note that it is not possible to use Reset's return value correctly, as there
        // is a race condition between draining the channel and the new timer expiring.
        // Reset should always be invoked on stopped or expired channels, as described above.
        // The return value exists to preserve compatibility with existing programs.
        private static bool Reset(this ref Timer _t, Duration d) => func(_t, (ref Timer t, Defer _, Panic panic, Recover __) =>
        {
            if (t.r.f == null)
            {
                panic("time: Reset called on uninitialized Timer");
            }
            var w = when(d);
            var active = stopTimer(ref t.r);
            t.r.when = w;
            startTimer(ref t.r);
            return active;
        });

        private static void sendTime(object c, System.UIntPtr seq)
        { 
            // Non-blocking send of time on c.
            // Used in NewTimer, it cannot block anyway (buffer).
            // Used in NewTicker, dropping sends on the floor is
            // the desired behavior when the reader gets behind,
            // because the sends are periodic.
        }

        // After waits for the duration to elapse and then sends the current time
        // on the returned channel.
        // It is equivalent to NewTimer(d).C.
        // The underlying Timer is not recovered by the garbage collector
        // until the timer fires. If efficiency is a concern, use NewTimer
        // instead and call Timer.Stop if the timer is no longer needed.
        public static channel<Time> After(Duration d)
        {
            return NewTimer(d).C;
        }

        // AfterFunc waits for the duration to elapse and then calls f
        // in its own goroutine. It returns a Timer that can
        // be used to cancel the call using its Stop method.
        public static ref Timer AfterFunc(Duration d, Action f)
        {
            Timer t = ref new Timer(r:runtimeTimer{when:when(d),f:goFunc,arg:f,},);
            startTimer(ref t.r);
            return t;
        }

        private static void goFunc(object arg, System.UIntPtr seq)
        {
            go_(() => arg._<Action>()());
        }
    }
}
