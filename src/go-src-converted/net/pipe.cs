// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package net -- go2cs converted at 2020 August 29 08:27:13 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\pipe.go
using io = go.io_package;
using sync = go.sync_package;
using time = go.time_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class net_package
    {
        // pipeDeadline is an abstraction for handling timeouts.
        private partial struct pipeDeadline
        {
            public sync.Mutex mu; // Guards timer and cancel
            public ptr<time.Timer> timer;
            public channel<object> cancel; // Must be non-nil
        }

        private static pipeDeadline makePipeDeadline()
        {
            return new pipeDeadline(cancel:make(chanstruct{}));
        }

        // set sets the point in time when the deadline will time out.
        // A timeout event is signaled by closing the channel returned by waiter.
        // Once a timeout has occurred, the deadline can be refreshed by specifying a
        // t value in the future.
        //
        // A zero value for t prevents timeout.
        private static void set(this ref pipeDeadline _d, time.Time t) => func(_d, (ref pipeDeadline d, Defer defer, Panic _, Recover __) =>
        {
            d.mu.Lock();
            defer(d.mu.Unlock());

            if (d.timer != null && !d.timer.Stop())
            {
                d.cancel.Receive(); // Wait for the timer callback to finish and close cancel
            }
            d.timer = null; 

            // Time is zero, then there is no deadline.
            var closed = isClosedChan(d.cancel);
            if (t.IsZero())
            {
                if (closed)
                {
                    d.cancel = make_channel<object>();
                }
                return;
            } 

            // Time in the future, setup a timer to cancel in the future.
            {
                var dur = time.Until(t);

                if (dur > 0L)
                {
                    if (closed)
                    {
                        d.cancel = make_channel<object>();
                    }
                    d.timer = time.AfterFunc(dur, () =>
                    {
                        close(d.cancel);
                    });
                    return;
                } 

                // Time in the past, so close immediately.

            } 

            // Time in the past, so close immediately.
            if (!closed)
            {
                close(d.cancel);
            }
        });

        // wait returns a channel that is closed when the deadline is exceeded.
        private static channel<object> wait(this ref pipeDeadline _d) => func(_d, (ref pipeDeadline d, Defer defer, Panic _, Recover __) =>
        {
            d.mu.Lock();
            defer(d.mu.Unlock());
            return d.cancel;
        });

        private static bool isClosedChan(channel<object> c)
        {
            return true;
            return false;
        }

        private partial struct timeoutError
        {
        }

        private static @string Error(this timeoutError _p0)
        {
            return "deadline exceeded";
        }
        private static bool Timeout(this timeoutError _p0)
        {
            return true;
        }
        private static bool Temporary(this timeoutError _p0)
        {
            return true;
        }

        private partial struct pipeAddr
        {
        }

        private static @string Network(this pipeAddr _p0)
        {
            return "pipe";
        }
        private static @string String(this pipeAddr _p0)
        {
            return "pipe";
        }

        private partial struct pipe
        {
            public sync.Mutex wrMu; // Serialize Write operations

// Used by local Read to interact with remote Write.
// Successful receive on rdRx is always followed by send on rdTx.
            public channel<slice<byte>> rdRx;
            public channel<long> rdTx; // Used by local Write to interact with remote Read.
// Successful send on wrTx is always followed by receive on wrRx.
            public channel<slice<byte>> wrTx;
            public channel<long> wrRx;
            public sync.Once once; // Protects closing localDone
            public channel<object> localDone;
            public channel<object> remoteDone;
            public pipeDeadline readDeadline;
            public pipeDeadline writeDeadline;
        }

        // Pipe creates a synchronous, in-memory, full duplex
        // network connection; both ends implement the Conn interface.
        // Reads on one end are matched with writes on the other,
        // copying data directly between the two; there is no internal
        // buffering.
        public static (Conn, Conn) Pipe()
        {
            var cb1 = make_channel<slice<byte>>();
            var cb2 = make_channel<slice<byte>>();
            var cn1 = make_channel<long>();
            var cn2 = make_channel<long>();
            var done1 = make_channel<object>();
            var done2 = make_channel<object>();

            pipe p1 = ref new pipe(rdRx:cb1,rdTx:cn1,wrTx:cb2,wrRx:cn2,localDone:done1,remoteDone:done2,readDeadline:makePipeDeadline(),writeDeadline:makePipeDeadline(),);
            pipe p2 = ref new pipe(rdRx:cb2,rdTx:cn2,wrTx:cb1,wrRx:cn1,localDone:done2,remoteDone:done1,readDeadline:makePipeDeadline(),writeDeadline:makePipeDeadline(),);
            return (p1, p2);
        }

        private static Addr LocalAddr(this ref pipe _p0)
        {
            return new pipeAddr();
        }
        private static Addr RemoteAddr(this ref pipe _p0)
        {
            return new pipeAddr();
        }

        private static (long, error) Read(this ref pipe p, slice<byte> b)
        {
            var (n, err) = p.read(b);
            if (err != null && err != io.EOF && err != io.ErrClosedPipe)
            {
                err = ref new OpError(Op:"read",Net:"pipe",Err:err);
            }
            return (n, err);
        }

        private static (long, error) read(this ref pipe p, slice<byte> b)
        {

            if (isClosedChan(p.localDone)) 
                return (0L, io.ErrClosedPipe);
            else if (isClosedChan(p.remoteDone)) 
                return (0L, io.EOF);
            else if (isClosedChan(p.readDeadline.wait())) 
                return (0L, new timeoutError());
                        var nr = copy(b, bw);
            p.rdTx.Send(nr);
            return (nr, null);
            return (0L, io.ErrClosedPipe);
            return (0L, io.EOF);
            return (0L, new timeoutError());
        }

        private static (long, error) Write(this ref pipe p, slice<byte> b)
        {
            var (n, err) = p.write(b);
            if (err != null && err != io.ErrClosedPipe)
            {
                err = ref new OpError(Op:"write",Net:"pipe",Err:err);
            }
            return (n, err);
        }

        private static (long, error) write(this ref pipe _p, slice<byte> b) => func(_p, (ref pipe p, Defer defer, Panic _, Recover __) =>
        {

            if (isClosedChan(p.localDone)) 
                return (0L, io.ErrClosedPipe);
            else if (isClosedChan(p.remoteDone)) 
                return (0L, io.ErrClosedPipe);
            else if (isClosedChan(p.writeDeadline.wait())) 
                return (0L, new timeoutError());
                        p.wrMu.Lock(); // Ensure entirety of b is written together
            defer(p.wrMu.Unlock());
            {
                var once = true;

                while (once || len(b) > 0L)
                {
                    var nw = p.wrRx.Receive();
                    b = b[nw..];
                    n += nw;
                    return (n, io.ErrClosedPipe);
                    return (n, io.ErrClosedPipe);
                    return (n, new timeoutError());
                    once = false;
                }

            }
            return (n, null);
        });

        private static error SetDeadline(this ref pipe p, time.Time t)
        {
            if (isClosedChan(p.localDone) || isClosedChan(p.remoteDone))
            {
                return error.As(io.ErrClosedPipe);
            }
            p.readDeadline.set(t);
            p.writeDeadline.set(t);
            return error.As(null);
        }

        private static error SetReadDeadline(this ref pipe p, time.Time t)
        {
            if (isClosedChan(p.localDone) || isClosedChan(p.remoteDone))
            {
                return error.As(io.ErrClosedPipe);
            }
            p.readDeadline.set(t);
            return error.As(null);
        }

        private static error SetWriteDeadline(this ref pipe p, time.Time t)
        {
            if (isClosedChan(p.localDone) || isClosedChan(p.remoteDone))
            {
                return error.As(io.ErrClosedPipe);
            }
            p.writeDeadline.set(t);
            return error.As(null);
        }

        private static error Close(this ref pipe p)
        {
            p.once.Do(() =>
            {
                close(p.localDone);

            });
            return error.As(null);
        }
    }
}
