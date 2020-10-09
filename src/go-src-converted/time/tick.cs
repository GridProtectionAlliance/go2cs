// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package time -- go2cs converted at 2020 October 09 05:06:06 UTC
// import "time" ==> using time = go.time_package
// Original source: C:\Go\src\time\tick.go
using errors = go.errors_package;
using static go.builtin;

namespace go
{
    public static partial class time_package
    {
        // A Ticker holds a channel that delivers `ticks' of a clock
        // at intervals.
        public partial struct Ticker
        {
            public channel<Time> C; // The channel on which the ticks are delivered.
            public runtimeTimer r;
        }

        // NewTicker returns a new Ticker containing a channel that will send the
        // time with a period specified by the duration argument.
        // It adjusts the intervals or drops ticks to make up for slow receivers.
        // The duration d must be greater than zero; if not, NewTicker will panic.
        // Stop the ticker to release associated resources.
        public static ptr<Ticker> NewTicker(Duration d) => func((_, panic, __) =>
        {
            if (d <= 0L)
            {
                panic(errors.New("non-positive interval for NewTicker"));
            } 
            // Give the channel a 1-element time buffer.
            // If the client falls behind while reading, we drop ticks
            // on the floor until the client catches up.
            var c = make_channel<Time>(1L);
            ptr<Ticker> t = addr(new Ticker(C:c,r:runtimeTimer{when:when(d),period:int64(d),f:sendTime,arg:c,},));
            startTimer(_addr_t.r);
            return _addr_t!;

        });

        // Stop turns off a ticker. After Stop, no more ticks will be sent.
        // Stop does not close the channel, to prevent a concurrent goroutine
        // reading from the channel from seeing an erroneous "tick".
        private static void Stop(this ptr<Ticker> _addr_t)
        {
            ref Ticker t = ref _addr_t.val;

            stopTimer(_addr_t.r);
        }

        // Reset stops a ticker and resets its period to the specified duration.
        // The next tick will arrive after the new period elapses.
        private static void Reset(this ptr<Ticker> _addr_t, Duration d) => func((_, panic, __) =>
        {
            ref Ticker t = ref _addr_t.val;

            if (t.r.f == null)
            {
                panic("time: Reset called on uninitialized Ticker");
            }

            modTimer(_addr_t.r, when(d), int64(d), t.r.f, t.r.arg, t.r.seq);

        });

        // Tick is a convenience wrapper for NewTicker providing access to the ticking
        // channel only. While Tick is useful for clients that have no need to shut down
        // the Ticker, be aware that without a way to shut it down the underlying
        // Ticker cannot be recovered by the garbage collector; it "leaks".
        // Unlike NewTicker, Tick will return nil if d <= 0.
        public static channel<Time> Tick(Duration d)
        {
            if (d <= 0L)
            {
                return null;
            }

            return NewTicker(d).C;

        }
    }
}
