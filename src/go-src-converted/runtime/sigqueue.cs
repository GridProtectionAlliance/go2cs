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
// variable. It can be in 3 states: sigIdle, sigReceiving and sigSending.
// sigReceiving means that signal_recv is blocked on sig.Note and there are no
// new pending signals.
// sigSending means that sig.mask *may* contain new pending signals,
// signal_recv can't be blocked in this state.
// sigIdle means that there are no new pending signals and signal_recv is not blocked.
// Transitions between states are done atomically with CAS.
// When signal_recv is unblocked, it resets sig.Note and rechecks sig.mask.
// If several sigsends and signal_recv execute concurrently, it can lead to
// unnecessary rechecks of sig.mask, but it cannot lead to missed signals
// nor deadlocks.

// +build !plan9

// package runtime -- go2cs converted at 2020 August 29 08:20:42 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\sigqueue.go
using atomic = go.runtime.@internal.atomic_package;
using _@unsafe_ = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
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
        private static var sig = default;

        private static readonly var sigIdle = iota;
        private static readonly var sigReceiving = 0;
        private static readonly var sigSending = 1;

        // sigsend delivers a signal from sighandler to the internal signal delivery queue.
        // It reports whether the signal was sent. If not, the caller typically crashes the program.
        // It runs from the signal handler, so it's limited in what it can do.
        private static bool sigsend(uint s)
        {
            var bit = uint32(1L) << (int)(uint(s & 31L));
            if (!sig.inuse || s >= uint32(32L * len(sig.wanted)))
            {
                return false;
            }
            atomic.Xadd(ref sig.delivering, 1L); 
            // We are running in the signal handler; defer is not available.

            {
                var w = atomic.Load(ref sig.wanted[s / 32L]);

                if (w & bit == 0L)
                {
                    atomic.Xadd(ref sig.delivering, -1L);
                    return false;
                } 

                // Add signal to outgoing queue.

            } 

            // Add signal to outgoing queue.
            while (true)
            {
                var mask = sig.mask[s / 32L];
                if (mask & bit != 0L)
                {
                    atomic.Xadd(ref sig.delivering, -1L);
                    return true; // signal already in queue
                }
                if (atomic.Cas(ref sig.mask[s / 32L], mask, mask | bit))
                {
                    break;
                }
            } 

            // Notify receiver that queue has new bit.
 

            // Notify receiver that queue has new bit.
Send:

            while (true)
            {

                if (atomic.Load(ref sig.state) == sigIdle) 
                    if (atomic.Cas(ref sig.state, sigIdle, sigSending))
                    {
                        _breakSend = true;
                        break;
                    }
                else if (atomic.Load(ref sig.state) == sigSending) 
                    // notification already pending
                    _breakSend = true;
                    break;
                else if (atomic.Load(ref sig.state) == sigReceiving) 
                    if (atomic.Cas(ref sig.state, sigReceiving, sigIdle))
                    {
                        notewakeup(ref sig.note);
                        _breakSend = true;
                        break;
                    }
                else 
                    throw("sigsend: inconsistent state");
                            }

            atomic.Xadd(ref sig.delivering, -1L);
            return true;
        }

        // Called to receive the next queued signal.
        // Must only be called from a single goroutine at a time.
        //go:linkname signal_recv os/signal.signal_recv
        private static uint signal_recv()
        {
            while (true)
            { 
                // Serve any signals from local copy.
                {
                    var i__prev2 = i;

                    for (var i = uint32(0L); i < _NSIG; i++)
                    {
                        if (sig.recv[i / 32L] & (1L << (int)((i & 31L))) != 0L)
                        {
                            sig.recv[i / 32L] &= 1L << (int)((i & 31L));
                            return i;
                        }
                    } 

                    // Wait for updates to be available from signal sender.


                    i = i__prev2;
                } 

                // Wait for updates to be available from signal sender.
Receive: 

                // Incorporate updates from sender into local copy.
                while (true)
                {

                    if (atomic.Load(ref sig.state) == sigIdle) 
                        if (atomic.Cas(ref sig.state, sigIdle, sigReceiving))
                        {
                            notetsleepg(ref sig.note, -1L);
                            noteclear(ref sig.note);
                            _breakReceive = true;
                            break;
                        }
                    else if (atomic.Load(ref sig.state) == sigSending) 
                        if (atomic.Cas(ref sig.state, sigSending, sigIdle))
                        {
                            _breakReceive = true;
                            break;
                        }
                    else 
                        throw("signal_recv: inconsistent state");
                                    } 

                // Incorporate updates from sender into local copy.
 

                // Incorporate updates from sender into local copy.
                {
                    var i__prev2 = i;

                    foreach (var (__i) in sig.mask)
                    {
                        i = __i;
                        sig.recv[i] = atomic.Xchg(ref sig.mask[i], 0L);
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
        private static void signalWaitUntilIdle()
        { 
            // Although the signals we care about have been removed from
            // sig.wanted, it is possible that another thread has received
            // a signal, has read from sig.wanted, is now updating sig.mask,
            // and has not yet woken up the processor thread. We need to wait
            // until all current signal deliveries have completed.
            while (atomic.Load(ref sig.delivering) != 0L)
            {
                Gosched();
            } 

            // Although WaitUntilIdle seems like the right name for this
            // function, the state we are looking for is sigReceiving, not
            // sigIdle.  The sigIdle state is really more like sigProcessing.
 

            // Although WaitUntilIdle seems like the right name for this
            // function, the state we are looking for is sigReceiving, not
            // sigIdle.  The sigIdle state is really more like sigProcessing.
            while (atomic.Load(ref sig.state) != sigReceiving)
            {
                Gosched();
            }

        }

        // Must only be called from a single goroutine at a time.
        //go:linkname signal_enable os/signal.signal_enable
        private static void signal_enable(uint s)
        {
            if (!sig.inuse)
            { 
                // The first call to signal_enable is for us
                // to use for initialization. It does not pass
                // signal information in m.
                sig.inuse = true; // enable reception of signals; cannot disable
                noteclear(ref sig.note);
                return;
            }
            if (s >= uint32(len(sig.wanted) * 32L))
            {
                return;
            }
            var w = sig.wanted[s / 32L];
            w |= 1L << (int)((s & 31L));
            atomic.Store(ref sig.wanted[s / 32L], w);

            var i = sig.ignored[s / 32L];
            i &= 1L << (int)((s & 31L));
            atomic.Store(ref sig.ignored[s / 32L], i);

            sigenable(s);
        }

        // Must only be called from a single goroutine at a time.
        //go:linkname signal_disable os/signal.signal_disable
        private static void signal_disable(uint s)
        {
            if (s >= uint32(len(sig.wanted) * 32L))
            {
                return;
            }
            sigdisable(s);

            var w = sig.wanted[s / 32L];
            w &= 1L << (int)((s & 31L));
            atomic.Store(ref sig.wanted[s / 32L], w);
        }

        // Must only be called from a single goroutine at a time.
        //go:linkname signal_ignore os/signal.signal_ignore
        private static void signal_ignore(uint s)
        {
            if (s >= uint32(len(sig.wanted) * 32L))
            {
                return;
            }
            sigignore(s);

            var w = sig.wanted[s / 32L];
            w &= 1L << (int)((s & 31L));
            atomic.Store(ref sig.wanted[s / 32L], w);

            var i = sig.ignored[s / 32L];
            i |= 1L << (int)((s & 31L));
            atomic.Store(ref sig.ignored[s / 32L], i);
        }

        // Checked by signal handlers.
        private static bool signal_ignored(uint s)
        {
            var i = atomic.Load(ref sig.ignored[s / 32L]);
            return i & (1L << (int)((s & 31L))) != 0L;
        }
    }
}
