// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using @unsafe = unsafe_package;

partial class time_package {

// Note: The runtime knows the layout of struct Ticker, since newTimer allocates it.
// Note also that Ticker and Timer have the same layout, so that newTimer can handle both.
// The initTimer and initTicker fields are named differently so that
// users cannot convert between the two without unsafe.

// A Ticker holds a channel that delivers “ticks” of a clock
// at intervals.
[GoType] partial struct Ticker {
    public /*<-*/channel<Time> C; // The channel on which the ticks are delivered.
    internal bool initTicker;
}

// NewTicker returns a new [Ticker] containing a channel that will send
// the current time on the channel after each tick. The period of the
// ticks is specified by the duration argument. The ticker will adjust
// the time interval or drop ticks to make up for slow receivers.
// The duration d must be greater than zero; if not, NewTicker will
// panic.
//
// Before Go 1.23, the garbage collector did not recover
// tickers that had not yet expired or been stopped, so code often
// immediately deferred t.Stop after calling NewTicker, to make
// the ticker recoverable when it was no longer needed.
// As of Go 1.23, the garbage collector can recover unreferenced
// tickers, even if they haven't been stopped.
// The Stop method is no longer necessary to help the garbage collector.
// (Code may of course still want to call Stop to stop the ticker for other reasons.)
public static ж<Ticker> NewTicker(Duration d) {
    if (d <= 0) {
        throw panic("non-positive interval for NewTicker");
    }
    // Give the channel a 1-element time buffer.
    // If the client falls behind while reading, we drop ticks
    // on the floor until the client catches up.
    var c = new channel<Time>(1);
    var t = (ж<Ticker>)(uintptr)(new @unsafe.Pointer(newTimer(when(d), ((int64)d), sendTime, c, (uintptr)syncTimer(c))));
    t.val.C = c;
    return t;
}

// Stop turns off a ticker. After Stop, no more ticks will be sent.
// Stop does not close the channel, to prevent a concurrent goroutine
// reading from the channel from seeing an erroneous "tick".
[GoRecv] public static void Stop(this ref Ticker t) {
    if (!t.initTicker) {
        // This is misuse, and the same for time.Timer would panic,
        // but this didn't always panic, and we keep it not panicking
        // to avoid breaking old programs. See issue 21874.
        return;
    }
    stopTimer((ж<Timer>)(uintptr)(@unsafe.Pointer.FromRef(ref t)));
}

// Reset stops a ticker and resets its period to the specified duration.
// The next tick will arrive after the new period elapses. The duration d
// must be greater than zero; if not, Reset will panic.
[GoRecv] public static void Reset(this ref Ticker t, Duration d) {
    if (d <= 0) {
        throw panic("non-positive interval for Ticker.Reset");
    }
    if (!t.initTicker) {
        throw panic("time: Reset called on uninitialized Ticker");
    }
    resetTimer((ж<Timer>)(uintptr)(@unsafe.Pointer.FromRef(ref t)), when(d), ((int64)d));
}

// Tick is a convenience wrapper for [NewTicker] providing access to the ticking
// channel only. Unlike NewTicker, Tick will return nil if d <= 0.
//
// Before Go 1.23, this documentation warned that the underlying
// [Ticker] would never be recovered by the garbage collector, and that
// if efficiency was a concern, code should use NewTicker instead and
// call [Ticker.Stop] when the ticker is no longer needed.
// As of Go 1.23, the garbage collector can recover unreferenced
// tickers, even if they haven't been stopped.
// The Stop method is no longer necessary to help the garbage collector.
// There is no longer any reason to prefer NewTicker when Tick will do.
public static /*<-*/channel<Time> Tick(Duration d) {
    if (d <= 0) {
        return default!;
    }
    return (~NewTicker(d)).C;
}

} // end time_package
