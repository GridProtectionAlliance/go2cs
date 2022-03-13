// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements runtime support for signal handling.
//
// Most synchronization primitives are not available from
// the signal handler (it cannot block, allocate memory, or use locks)
// so the handler communicates with a processing goroutine
// via struct sig, below.
//
// sigsend is called by the signal handler to queue a new signal.
// signal_recv is called by the Go program to receive a newly queued signal.
// Synchronization between sigsend and signal_recv is based on the sig.state
// variable. It can be in 4 states: sigIdle, sigReceiving, sigSending and sigFixup.
// sigReceiving means that signal_recv is blocked on sig.Note and there are no
// new pending signals.
// sigSending means that sig.mask *may* contain new pending signals,
// signal_recv can't be blocked in this state.
// sigIdle means that there are no new pending signals and signal_recv is not blocked.
// sigFixup is a transient state that can only exist as a short
// transition from sigReceiving and then on to sigIdle: it is
// used to ensure the AllThreadsSyscall()'s mDoFixup() operation
// occurs on the sleeping m, waiting to receive a signal.
// Transitions between states are done atomically with CAS.
// When signal_recv is unblocked, it resets sig.Note and rechecks sig.mask.
// If several sigsends and signal_recv execute concurrently, it can lead to
// unnecessary rechecks of sig.mask, but it cannot lead to missed signals
// nor deadlocks.

//go:build !plan9
// +build !plan9

// package runtime -- go2cs converted at 2022 March 13 05:27:00 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\sigqueue.go
namespace go;

using atomic = runtime.@internal.atomic_package;
using _@unsafe_ = @unsafe_package; // for go:linkname


// sig handles communication between the signal handler and os/signal.
// Other than the inuse and recv fields, the fields are accessed atomically.
//
// The wanted and ignored fields are only written by one goroutine at
// a time; access is controlled by the handlers Mutex in os/signal.
// The fields are only read by that one goroutine and by the signal handler.
// We access them atomically to minimize the race between setting them
// in the goroutine calling os/signal and the signal handler,
// which may be running in a different thread. That race is unavoidable,
// as there is no connection between handling a signal and receiving one,
// but atomic instructions should minimize it.

public static partial class runtime_package {

private static var sig = default;

private static readonly var sigIdle = iota;
private static readonly var sigReceiving = 0;
private static readonly var sigSending = 1;
private static readonly var sigFixup = 2;

// sigsend delivers a signal from sighandler to the internal signal delivery queue.
// It reports whether the signal was sent. If not, the caller typically crashes the program.
// It runs from the signal handler, so it's limited in what it can do.
private static bool sigsend(uint s) {
    var bit = uint32(1) << (int)(uint(s & 31));
    if (s >= uint32(32 * len(sig.wanted))) {
        return false;
    }
    atomic.Xadd(_addr_sig.delivering, 1); 
    // We are running in the signal handler; defer is not available.

    {
        var w = atomic.Load(_addr_sig.wanted[s / 32]);

        if (w & bit == 0) {
            atomic.Xadd(_addr_sig.delivering, -1);
            return false;
        }
    } 

    // Add signal to outgoing queue.
    while (true) {
        var mask = sig.mask[s / 32];
        if (mask & bit != 0) {
            atomic.Xadd(_addr_sig.delivering, -1);
            return true; // signal already in queue
        }
        if (atomic.Cas(_addr_sig.mask[s / 32], mask, mask | bit)) {
            break;
        }
    } 

    // Notify receiver that queue has new bit.
Send:

    while (true) {

        if (atomic.Load(_addr_sig.state) == sigIdle) 
            if (atomic.Cas(_addr_sig.state, sigIdle, sigSending)) {
                _breakSend = true;
                break;
            }
        else if (atomic.Load(_addr_sig.state) == sigSending) 
            // notification already pending
            _breakSend = true;
            break;
        else if (atomic.Load(_addr_sig.state) == sigReceiving) 
            if (atomic.Cas(_addr_sig.state, sigReceiving, sigIdle)) {
                if (GOOS == "darwin" || GOOS == "ios") {
                    sigNoteWakeup(_addr_sig.note);
                    _breakSend = true;
                    break;
                }
                notewakeup(_addr_sig.note);
                _breakSend = true;
                break;
            }
        else if (atomic.Load(_addr_sig.state) == sigFixup) 
            // nothing to do - we need to wait for sigIdle.
            mDoFixupAndOSYield();
        else 
            throw("sigsend: inconsistent state");
            }
    atomic.Xadd(_addr_sig.delivering, -1);
    return true;
}

// sigRecvPrepareForFixup is used to temporarily wake up the
// signal_recv() running thread while it is blocked waiting for the
// arrival of a signal. If it causes the thread to wake up, the
// sig.state travels through this sequence: sigReceiving -> sigFixup
// -> sigIdle -> sigReceiving and resumes. (This is only called while
// GC is disabled.)
//go:nosplit
private static void sigRecvPrepareForFixup() {
    if (atomic.Cas(_addr_sig.state, sigReceiving, sigFixup)) {
        notewakeup(_addr_sig.note);
    }
}

// Called to receive the next queued signal.
// Must only be called from a single goroutine at a time.
//go:linkname signal_recv os/signal.signal_recv
private static uint signal_recv() {
    while (true) { 
        // Serve any signals from local copy.
        {
            var i__prev2 = i;

            for (var i = uint32(0); i < _NSIG; i++) {
                if (sig.recv[i / 32] & (1 << (int)((i & 31))) != 0) {
                    sig.recv[i / 32] &= 1 << (int)((i & 31));
                    return i;
                }
            } 

            // Wait for updates to be available from signal sender.


            i = i__prev2;
        } 

        // Wait for updates to be available from signal sender.
Receive: 

        // Incorporate updates from sender into local copy.
        while (true) {

            if (atomic.Load(_addr_sig.state) == sigIdle) 
                if (atomic.Cas(_addr_sig.state, sigIdle, sigReceiving)) {
                    if (GOOS == "darwin" || GOOS == "ios") {
                        sigNoteSleep(_addr_sig.note);
                        _breakReceive = true;
                        break;
                    }
                    notetsleepg(_addr_sig.note, -1);
                    noteclear(_addr_sig.note);
                    if (!atomic.Cas(_addr_sig.state, sigFixup, sigIdle)) {
                        _breakReceive = true;
                        break;
                    } 
                    // Getting here, the code will
                    // loop around again to sleep
                    // in state sigReceiving. This
                    // path is taken when
                    // sigRecvPrepareForFixup()
                    // has been called by another
                    // thread.
                }
            else if (atomic.Load(_addr_sig.state) == sigSending) 
                if (atomic.Cas(_addr_sig.state, sigSending, sigIdle)) {
                    _breakReceive = true;
                    break;
                }
            else 
                throw("signal_recv: inconsistent state");
                    } 

        // Incorporate updates from sender into local copy.
        {
            var i__prev2 = i;

            foreach (var (__i) in sig.mask) {
                i = __i;
                sig.recv[i] = atomic.Xchg(_addr_sig.mask[i], 0);
            }

            i = i__prev2;
        }
    }
}

// signalWaitUntilIdle waits until the signal delivery mechanism is idle.
// This is used to ensure that we do not drop a signal notification due
// to a race between disabling a signal and receiving a signal.
// This assumes that signal delivery has already been disabled for
// the signal(s) in question, and here we are just waiting to make sure
// that all the signals have been delivered to the user channels
// by the os/signal package.
//go:linkname signalWaitUntilIdle os/signal.signalWaitUntilIdle
private static void signalWaitUntilIdle() { 
    // Although the signals we care about have been removed from
    // sig.wanted, it is possible that another thread has received
    // a signal, has read from sig.wanted, is now updating sig.mask,
    // and has not yet woken up the processor thread. We need to wait
    // until all current signal deliveries have completed.
    while (atomic.Load(_addr_sig.delivering) != 0) {
        Gosched();
    } 

    // Although WaitUntilIdle seems like the right name for this
    // function, the state we are looking for is sigReceiving, not
    // sigIdle.  The sigIdle state is really more like sigProcessing.
    while (atomic.Load(_addr_sig.state) != sigReceiving) {
        Gosched();
    }
}

// Must only be called from a single goroutine at a time.
//go:linkname signal_enable os/signal.signal_enable
private static void signal_enable(uint s) {
    if (!sig.inuse) { 
        // This is the first call to signal_enable. Initialize.
        sig.inuse = true; // enable reception of signals; cannot disable
        if (GOOS == "darwin" || GOOS == "ios") {
            sigNoteSetup(_addr_sig.note);
        }
        else
 {
            noteclear(_addr_sig.note);
        }
    }
    if (s >= uint32(len(sig.wanted) * 32)) {
        return ;
    }
    var w = sig.wanted[s / 32];
    w |= 1 << (int)((s & 31));
    atomic.Store(_addr_sig.wanted[s / 32], w);

    var i = sig.ignored[s / 32];
    i &= 1 << (int)((s & 31));
    atomic.Store(_addr_sig.ignored[s / 32], i);

    sigenable(s);
}

// Must only be called from a single goroutine at a time.
//go:linkname signal_disable os/signal.signal_disable
private static void signal_disable(uint s) {
    if (s >= uint32(len(sig.wanted) * 32)) {
        return ;
    }
    sigdisable(s);

    var w = sig.wanted[s / 32];
    w &= 1 << (int)((s & 31));
    atomic.Store(_addr_sig.wanted[s / 32], w);
}

// Must only be called from a single goroutine at a time.
//go:linkname signal_ignore os/signal.signal_ignore
private static void signal_ignore(uint s) {
    if (s >= uint32(len(sig.wanted) * 32)) {
        return ;
    }
    sigignore(s);

    var w = sig.wanted[s / 32];
    w &= 1 << (int)((s & 31));
    atomic.Store(_addr_sig.wanted[s / 32], w);

    var i = sig.ignored[s / 32];
    i |= 1 << (int)((s & 31));
    atomic.Store(_addr_sig.ignored[s / 32], i);
}

// sigInitIgnored marks the signal as already ignored. This is called at
// program start by initsig. In a shared library initsig is called by
// libpreinit, so the runtime may not be initialized yet.
//go:nosplit
private static void sigInitIgnored(uint s) {
    var i = sig.ignored[s / 32];
    i |= 1 << (int)((s & 31));
    atomic.Store(_addr_sig.ignored[s / 32], i);
}

// Checked by signal handlers.
//go:linkname signal_ignored os/signal.signal_ignored
private static bool signal_ignored(uint s) {
    var i = atomic.Load(_addr_sig.ignored[s / 32]);
    return i & (1 << (int)((s & 31))) != 0;
}

} // end runtime_package
