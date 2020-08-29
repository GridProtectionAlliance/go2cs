// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Pipe adapter to connect code expecting an io.Reader
// with code expecting an io.Writer.

// package io -- go2cs converted at 2020 August 29 08:21:54 UTC
// import "io" ==> using io = go.io_package
// Original source: C:\Go\src\io\pipe.go
using errors = go.errors_package;
using sync = go.sync_package;
using atomic = go.sync.atomic_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class io_package
    {
        // atomicError is a type-safe atomic value for errors.
        // We use a struct{ error } to ensure consistent use of a concrete type.
        private partial struct atomicError
        {
            public atomic.Value v;
        }

        private static void Store(this ref atomicError a, error err)
        {
            a.v.Store(/* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{error}{err});
        }
        private static error Load(this ref atomicError a)
        {
            return error.As(err.error);
        }

        // ErrClosedPipe is the error used for read or write operations on a closed pipe.
        public static var ErrClosedPipe = errors.New("io: read/write on closed pipe");

        // A pipe is the shared pipe structure underlying PipeReader and PipeWriter.
        private partial struct pipe
        {
            public sync.Mutex wrMu; // Serializes Write operations
            public channel<slice<byte>> wrCh;
            public channel<long> rdCh;
            public sync.Once once; // Protects closing done
            public channel<object> done;
            public atomicError rerr;
            public atomicError werr;
        }

        private static (long, error) Read(this ref pipe p, slice<byte> b)
        {
            return (0L, p.readCloseError());
            var nr = copy(b, bw);
            p.rdCh.Send(nr);
            return (nr, null);
            return (0L, p.readCloseError());
        }

        private static error readCloseError(this ref pipe p)
        {
            var rerr = p.rerr.Load();
            {
                var werr = p.werr.Load();

                if (rerr == null && werr != null)
                {
                    return error.As(werr);
                }

            }
            return error.As(ErrClosedPipe);
        }

        private static error CloseRead(this ref pipe p, error err)
        {
            if (err == null)
            {
                err = ErrClosedPipe;
            }
            p.rerr.Store(err);
            p.once.Do(() =>
            {
                close(p.done);

            });
            return error.As(null);
        }

        private static (long, error) Write(this ref pipe _p, slice<byte> b) => func(_p, (ref pipe p, Defer defer, Panic _, Recover __) =>
        {
            return (0L, p.writeCloseError());
            p.wrMu.Lock();
            defer(p.wrMu.Unlock());
            {
                var once = true;

                while (once || len(b) > 0L)
                {
                    var nw = p.rdCh.Receive();
                    b = b[nw..];
                    n += nw;
                    return (n, p.writeCloseError());
                    once = false;
                }

            }
            return (n, null);
        });

        private static error writeCloseError(this ref pipe p)
        {
            var werr = p.werr.Load();
            {
                var rerr = p.rerr.Load();

                if (werr == null && rerr != null)
                {
                    return error.As(rerr);
                }

            }
            return error.As(ErrClosedPipe);
        }

        private static error CloseWrite(this ref pipe p, error err)
        {
            if (err == null)
            {
                err = EOF;
            }
            p.werr.Store(err);
            p.once.Do(() =>
            {
                close(p.done);

            });
            return error.As(null);
        }

        // A PipeReader is the read half of a pipe.
        public partial struct PipeReader
        {
            public ptr<pipe> p;
        }

        // Read implements the standard Read interface:
        // it reads data from the pipe, blocking until a writer
        // arrives or the write end is closed.
        // If the write end is closed with an error, that error is
        // returned as err; otherwise err is EOF.
        private static (long, error) Read(this ref PipeReader r, slice<byte> data)
        {
            return r.p.Read(data);
        }

        // Close closes the reader; subsequent writes to the
        // write half of the pipe will return the error ErrClosedPipe.
        private static error Close(this ref PipeReader r)
        {
            return error.As(r.CloseWithError(null));
        }

        // CloseWithError closes the reader; subsequent writes
        // to the write half of the pipe will return the error err.
        private static error CloseWithError(this ref PipeReader r, error err)
        {
            return error.As(r.p.CloseRead(err));
        }

        // A PipeWriter is the write half of a pipe.
        public partial struct PipeWriter
        {
            public ptr<pipe> p;
        }

        // Write implements the standard Write interface:
        // it writes data to the pipe, blocking until one or more readers
        // have consumed all the data or the read end is closed.
        // If the read end is closed with an error, that err is
        // returned as err; otherwise err is ErrClosedPipe.
        private static (long, error) Write(this ref PipeWriter w, slice<byte> data)
        {
            return w.p.Write(data);
        }

        // Close closes the writer; subsequent reads from the
        // read half of the pipe will return no bytes and EOF.
        private static error Close(this ref PipeWriter w)
        {
            return error.As(w.CloseWithError(null));
        }

        // CloseWithError closes the writer; subsequent reads from the
        // read half of the pipe will return no bytes and the error err,
        // or EOF if err is nil.
        //
        // CloseWithError always returns nil.
        private static error CloseWithError(this ref PipeWriter w, error err)
        {
            return error.As(w.p.CloseWrite(err));
        }

        // Pipe creates a synchronous in-memory pipe.
        // It can be used to connect code expecting an io.Reader
        // with code expecting an io.Writer.
        //
        // Reads and Writes on the pipe are matched one to one
        // except when multiple Reads are needed to consume a single Write.
        // That is, each Write to the PipeWriter blocks until it has satisfied
        // one or more Reads from the PipeReader that fully consume
        // the written data.
        // The data is copied directly from the Write to the corresponding
        // Read (or Reads); there is no internal buffering.
        //
        // It is safe to call Read and Write in parallel with each other or with Close.
        // Parallel calls to Read and parallel calls to Write are also safe:
        // the individual calls will be gated sequentially.
        public static (ref PipeReader, ref PipeWriter) Pipe()
        {
            pipe p = ref new pipe(wrCh:make(chan[]byte),rdCh:make(chanint),done:make(chanstruct{}),);
            return (ref new PipeReader(p), ref new PipeWriter(p));
        }
    }
}
