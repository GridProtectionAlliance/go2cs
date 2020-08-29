// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package bio implements common I/O abstractions used within the Go toolchain.
// package bio -- go2cs converted at 2020 August 29 08:48:52 UTC
// import "cmd/internal/bio" ==> using bio = go.cmd.@internal.bio_package
// Original source: C:\Go\src\cmd\internal\bio\buf.go
using bufio = go.bufio_package;
using log = go.log_package;
using os = go.os_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace @internal
{
    public static partial class bio_package
    {
        // Reader implements a seekable buffered io.Reader.
        public partial struct Reader
        {
            public ptr<os.File> f;
            public ref bufio.Reader Reader => ref Reader_ptr;
        }

        // Writer implements a seekable buffered io.Writer.
        public partial struct Writer
        {
            public ptr<os.File> f;
            public ref bufio.Writer Writer => ref Writer_ptr;
        }

        // Create creates the file named name and returns a Writer
        // for that file.
        public static (ref Writer, error) Create(@string name)
        {
            var (f, err) = os.Create(name);
            if (err != null)
            {
                return (null, err);
            }
            return (ref new Writer(f:f,Writer:bufio.NewWriter(f)), null);
        }

        // Open returns a Reader for the file named name.
        public static (ref Reader, error) Open(@string name)
        {
            var (f, err) = os.Open(name);
            if (err != null)
            {
                return (null, err);
            }
            return (ref new Reader(f:f,Reader:bufio.NewReader(f)), null);
        }

        private static long Seek(this ref Reader r, long offset, long whence)
        {
            if (whence == 1L)
            {
                offset -= int64(r.Buffered());
            }
            var (off, err) = r.f.Seek(offset, whence);
            if (err != null)
            {
                log.Fatalf("seeking in output: %v", err);
            }
            r.Reset(r.f);
            return off;
        }

        private static long Seek(this ref Writer w, long offset, long whence)
        {
            {
                var err = w.Flush();

                if (err != null)
                {
                    log.Fatalf("writing output: %v", err);
                }

            }
            var (off, err) = w.f.Seek(offset, whence);
            if (err != null)
            {
                log.Fatalf("seeking in output: %v", err);
            }
            return off;
        }

        private static long Offset(this ref Reader r)
        {
            var (off, err) = r.f.Seek(0L, 1L);
            if (err != null)
            {
                log.Fatalf("seeking in output [0, 1]: %v", err);
            }
            off -= int64(r.Buffered());
            return off;
        }

        private static long Offset(this ref Writer w)
        {
            {
                var err = w.Flush();

                if (err != null)
                {
                    log.Fatalf("writing output: %v", err);
                }

            }
            var (off, err) = w.f.Seek(0L, 1L);
            if (err != null)
            {
                log.Fatalf("seeking in output [0, 1]: %v", err);
            }
            return off;
        }

        private static error Close(this ref Reader r)
        {
            return error.As(r.f.Close());
        }

        private static error Close(this ref Writer w)
        {
            var err = w.Flush();
            var err1 = w.f.Close();
            if (err == null)
            {
                err = err1;
            }
            return error.As(err);
        }
    }
}}}
