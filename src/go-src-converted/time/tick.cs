// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package time -- go2cs converted at 2022 March 06 22:30:09 UTC
// import "time" ==> using time = go.time_package
// Original source: C:\Program Files\Go\src\time\tick.go
using errors = go.errors_package;

namespace go;

public static partial class time_package {

    // A Ticker holds a channel that delivers ``ticks'' of a clock
    // at intervals.
public partial struct Ticker {
    public channel<Time> C; // The channel on which the ticks are delivered.
    public runtimeTimer r;
}

// NewTicker returns a new Ticker containing a channel that will send
// the time on the channel after each tick. The period of the ticks is
// specified by the duration argument. The ticker will adjust the time
// interval or drop ticks to make up for slow receivers.
// The duration d must be greater than zero; if not, NewTicker will
// panic. Stop the ticker to release associated resources.
public static ptr<Ticker> NewTicker(Duration d) => func((_, panic, _) => {
    if (d <= 0) {
        panic(errors.New("non-positive interval for NewTicker"));
    }
    var c = make_channel<Time>(1);
    ptr<Ticker> t = addr(new Ticker(C:c,r:runtimeTimer{when:when(d),period:int64(d),f:sendTime,arg:c,},));
    startTimer(_addr_t.r);
    return _addr_t!;

});

// Stop turns off a ticker. After Stop, no more ticks will be sent.
// Stop does not close the channel, to prevent a concurrent goroutine
// reading from the channel from seeing an erroneous "tick".
private static void Stop(this ptr<Ticker> _addr_t) {
    ref Ticker t = ref _addr_t.val;

    stopTimer(_addr_t.r);
}

// Reset stops a ticker and resets its period to the specified duration.
// The next tick will arrive after the new period elapses.
private static void Reset(this ptr<Ticker> _addr_t, Duration d) => func((_, panic, _) => {
    ref Ticker t = ref _addr_t.val;

    if (t.r.f == null) {
        panic("time: Reset called on uninitialized Ticker");
    }
    modTimer(_addr_t.r, when(d), int64(d), t.r.f, t.r.arg, t.r.seq);

});

// Tick is a convenience wrapper for NewTicker providing access to the ticking
// channel only. While Tick is useful for clients that have no need to shut down
// the Ticker, be aware that without a way to shut it down the underlying
// Ticker cannot be recovered by the garbage collector; it "leaks".
// Unlike NewTicker, Tick will return nil if d <= 0.
public static channel<Time> Tick(Duration d) {
    if (d <= 0) {
        return null;
    }
    return NewTicker(d).C;

}

} // end time_package
