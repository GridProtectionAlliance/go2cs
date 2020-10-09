// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements runtime support for signal handling.

// package runtime -- go2cs converted at 2020 October 09 04:48:37 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\sigqueue_plan9.go
using _@unsafe_ = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        private static readonly long qsize = (long)64L;



        private static var sig = default;

        private partial struct noteData
        {
            public array<byte> s;
            public long n; // n bytes of s are valid
        }

        private partial struct noteQueue
        {
            public mutex @lock;
            public array<noteData> data;
            public long ri;
            public long wi;
            public bool full;
        }

        // It is not allowed to allocate memory in the signal handler.
        private static bool push(this ptr<noteQueue> _addr_q, ptr<byte> _addr_item)
        {
            ref noteQueue q = ref _addr_q.val;
            ref byte item = ref _addr_item.val;

            lock(_addr_q.@lock);
            if (q.full)
            {
                unlock(_addr_q.@lock);
                return false;
            }

            var s = gostringnocopy(item);
            copy(q.data[q.wi].s[..], s);
            q.data[q.wi].n = len(s);
            q.wi++;
            if (q.wi == qsize)
            {
                q.wi = 0L;
            }

            if (q.wi == q.ri)
            {
                q.full = true;
            }

            unlock(_addr_q.@lock);
            return true;

        }

        private static @string pop(this ptr<noteQueue> _addr_q)
        {
            ref noteQueue q = ref _addr_q.val;

            lock(_addr_q.@lock);
            q.full = false;
            if (q.ri == q.wi)
            {
                unlock(_addr_q.@lock);
                return "";
            }

            var note = _addr_q.data[q.ri];
            var item = string(note.s[..note.n]);
            q.ri++;
            if (q.ri == qsize)
            {
                q.ri = 0L;
            }

            unlock(_addr_q.@lock);
            return item;

        }

        // Called from sighandler to send a signal back out of the signal handling thread.
        // Reports whether the signal was sent. If not, the caller typically crashes the program.
        private static bool sendNote(ptr<byte> _addr_s)
        {
            ref byte s = ref _addr_s.val;

            if (!sig.inuse)
            {
                return false;
            } 

            // Add signal to outgoing queue.
            if (!sig.q.push(s))
            {
                return false;
            }

            lock(_addr_sig.@lock);
            if (sig.sleeping)
            {
                sig.sleeping = false;
                notewakeup(_addr_sig.note);
            }

            unlock(_addr_sig.@lock);

            return true;

        }

        // Called to receive the next queued signal.
        // Must only be called from a single goroutine at a time.
        //go:linkname signal_recv os/signal.signal_recv
        private static @string signal_recv()
        {
            while (true)
            {
                var note = sig.q.pop();
                if (note != "")
                {
                    return note;
                }

                lock(_addr_sig.@lock);
                sig.sleeping = true;
                noteclear(_addr_sig.note);
                unlock(_addr_sig.@lock);
                notetsleepg(_addr_sig.note, -1L);

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
            while (true)
            {
                lock(_addr_sig.@lock);
                var sleeping = sig.sleeping;
                unlock(_addr_sig.@lock);
                if (sleeping)
                {
                    return ;
                }

                Gosched();

            }


        }

        // Must only be called from a single goroutine at a time.
        //go:linkname signal_enable os/signal.signal_enable
        private static void signal_enable(uint s)
        {
            if (!sig.inuse)
            { 
                // This is the first call to signal_enable. Initialize.
                sig.inuse = true; // enable reception of signals; cannot disable
                noteclear(_addr_sig.note);

            }

        }

        // Must only be called from a single goroutine at a time.
        //go:linkname signal_disable os/signal.signal_disable
        private static void signal_disable(uint s)
        {
        }

        // Must only be called from a single goroutine at a time.
        //go:linkname signal_ignore os/signal.signal_ignore
        private static void signal_ignore(uint s)
        {
        }

        //go:linkname signal_ignored os/signal.signal_ignored
        private static bool signal_ignored(uint s)
        {
            return false;
        }
    }
}
