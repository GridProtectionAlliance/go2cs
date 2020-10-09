// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Pipe adapter to connect code expecting an io.Reader
// with code expecting an io.Writer.

// package io -- go2cs converted at 2020 October 09 04:49:26 UTC
// import "io" ==> using io = go.io_package
// Original source: C:\Go\src\io\pipe.go
using errors = go.errors_package;
using sync = go.sync_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class io_package
    {
        // onceError is an object that will only store an error once.
        private partial struct onceError
        {
            public ref sync.Mutex Mutex => ref Mutex_val; // guards following
            public error err;
        }

        private static void Store(this ptr<onceError> _addr_a, error err) => func((defer, _, __) =>
        {
            ref onceError a = ref _addr_a.val;

            a.Lock();
            defer(a.Unlock());
            if (a.err != null)
            {
                return ;
            }

            a.err = err;

        });
        private static error Load(this ptr<onceError> _addr_a) => func((defer, _, __) =>
        {
            ref onceError a = ref _addr_a.val;

            a.Lock();
            defer(a.Unlock());
            return error.As(a.err)!;
        });

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
            public onceError rerr;
            public onceError werr;
        }

        private static (long, error) Read(this ptr<pipe> _addr_p, slice<byte> b)
        {
            long n = default;
            error err = default!;
            ref pipe p = ref _addr_p.val;

            return (0L, error.As(p.readCloseError())!);
            var nr = copy(b, bw);
            p.rdCh.Send(nr);
            return (nr, error.As(null!)!);
            return (0L, error.As(p.readCloseError())!);
        }

        private static error readCloseError(this ptr<pipe> _addr_p)
        {
            ref pipe p = ref _addr_p.val;

            var rerr = p.rerr.Load();
            {
                var werr = p.werr.Load();

                if (rerr == null && werr != null)
                {
                    return error.As(werr)!;
                }

            }

            return error.As(ErrClosedPipe)!;

        }

        private static error CloseRead(this ptr<pipe> _addr_p, error err)
        {
            ref pipe p = ref _addr_p.val;

            if (err == null)
            {
                err = ErrClosedPipe;
            }

            p.rerr.Store(err);
            p.once.Do(() =>
            {
                close(p.done);
            });
            return error.As(null!)!;

        }

        private static (long, error) Write(this ptr<pipe> _addr_p, slice<byte> b) => func((defer, _, __) =>
        {
            long n = default;
            error err = default!;
            ref pipe p = ref _addr_p.val;

            return (0L, error.As(p.writeCloseError())!);
            p.wrMu.Lock();
            defer(p.wrMu.Unlock());
            {
                var once = true;

                while (once || len(b) > 0L)
                {
                    var nw = p.rdCh.Receive();
                    b = b[nw..];
                    n += nw;
                    return (n, error.As(p.writeCloseError())!);
                    once = false;
                }

            }
            return (n, error.As(null!)!);

        });

        private static error writeCloseError(this ptr<pipe> _addr_p)
        {
            ref pipe p = ref _addr_p.val;

            var werr = p.werr.Load();
            {
                var rerr = p.rerr.Load();

                if (werr == null && rerr != null)
                {
                    return error.As(rerr)!;
                }

            }

            return error.As(ErrClosedPipe)!;

        }

        private static error CloseWrite(this ptr<pipe> _addr_p, error err)
        {
            ref pipe p = ref _addr_p.val;

            if (err == null)
            {
                err = EOF;
            }

            p.werr.Store(err);
            p.once.Do(() =>
            {
                close(p.done);
            });
            return error.As(null!)!;

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
        private static (long, error) Read(this ptr<PipeReader> _addr_r, slice<byte> data)
        {
            long n = default;
            error err = default!;
            ref PipeReader r = ref _addr_r.val;

            return r.p.Read(data);
        }

        // Close closes the reader; subsequent writes to the
        // write half of the pipe will return the error ErrClosedPipe.
        private static error Close(this ptr<PipeReader> _addr_r)
        {
            ref PipeReader r = ref _addr_r.val;

            return error.As(r.CloseWithError(null))!;
        }

        // CloseWithError closes the reader; subsequent writes
        // to the write half of the pipe will return the error err.
        //
        // CloseWithError never overwrites the previous error if it exists
        // and always returns nil.
        private static error CloseWithError(this ptr<PipeReader> _addr_r, error err)
        {
            ref PipeReader r = ref _addr_r.val;

            return error.As(r.p.CloseRead(err))!;
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
        private static (long, error) Write(this ptr<PipeWriter> _addr_w, slice<byte> data)
        {
            long n = default;
            error err = default!;
            ref PipeWriter w = ref _addr_w.val;

            return w.p.Write(data);
        }

        // Close closes the writer; subsequent reads from the
        // read half of the pipe will return no bytes and EOF.
        private static error Close(this ptr<PipeWriter> _addr_w)
        {
            ref PipeWriter w = ref _addr_w.val;

            return error.As(w.CloseWithError(null))!;
        }

        // CloseWithError closes the writer; subsequent reads from the
        // read half of the pipe will return no bytes and the error err,
        // or EOF if err is nil.
        //
        // CloseWithError never overwrites the previous error if it exists
        // and always returns nil.
        private static error CloseWithError(this ptr<PipeWriter> _addr_w, error err)
        {
            ref PipeWriter w = ref _addr_w.val;

            return error.As(w.p.CloseWrite(err))!;
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
        public static (ptr<PipeReader>, ptr<PipeWriter>) Pipe()
        {
            ptr<PipeReader> _p0 = default!;
            ptr<PipeWriter> _p0 = default!;

            ptr<pipe> p = addr(new pipe(wrCh:make(chan[]byte),rdCh:make(chanint),done:make(chanstruct{}),));
            return (addr(new PipeReader(p)), addr(new PipeWriter(p)));
        }
    }
}
