// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package iotest implements Readers and Writers useful mainly for testing.
// package iotest -- go2cs converted at 2020 October 09 05:47:48 UTC
// import "testing/iotest" ==> using iotest = go.testing.iotest_package
// Original source: C:\Go\src\testing\iotest\reader.go
using errors = go.errors_package;
using io = go.io_package;
using static go.builtin;

namespace go {
namespace testing
{
    public static partial class iotest_package
    {
        // OneByteReader returns a Reader that implements
        // each non-empty Read by reading one byte from r.
        public static io.Reader OneByteReader(io.Reader r)
        {
            return addr(new oneByteReader(r));
        }

        private partial struct oneByteReader
        {
            public io.Reader r;
        }

        private static (long, error) Read(this ptr<oneByteReader> _addr_r, slice<byte> p)
        {
            long _p0 = default;
            error _p0 = default!;
            ref oneByteReader r = ref _addr_r.val;

            if (len(p) == 0L)
            {
                return (0L, error.As(null!)!);
            }

            return r.r.Read(p[0L..1L]);

        }

        // HalfReader returns a Reader that implements Read
        // by reading half as many requested bytes from r.
        public static io.Reader HalfReader(io.Reader r)
        {
            return addr(new halfReader(r));
        }

        private partial struct halfReader
        {
            public io.Reader r;
        }

        private static (long, error) Read(this ptr<halfReader> _addr_r, slice<byte> p)
        {
            long _p0 = default;
            error _p0 = default!;
            ref halfReader r = ref _addr_r.val;

            return r.r.Read(p[0L..(len(p) + 1L) / 2L]);
        }

        // DataErrReader changes the way errors are handled by a Reader. Normally, a
        // Reader returns an error (typically EOF) from the first Read call after the
        // last piece of data is read. DataErrReader wraps a Reader and changes its
        // behavior so the final error is returned along with the final data, instead
        // of in the first call after the final data.
        public static io.Reader DataErrReader(io.Reader r)
        {
            return addr(new dataErrReader(r,nil,make([]byte,1024)));
        }

        private partial struct dataErrReader
        {
            public io.Reader r;
            public slice<byte> unread;
            public slice<byte> data;
        }

        private static (long, error) Read(this ptr<dataErrReader> _addr_r, slice<byte> p)
        {
            long n = default;
            error err = default!;
            ref dataErrReader r = ref _addr_r.val;
 
            // loop because first call needs two reads:
            // one to get data and a second to look for an error.
            while (true)
            {
                if (len(r.unread) == 0L)
                {
                    var (n1, err1) = r.r.Read(r.data);
                    r.unread = r.data[0L..n1];
                    err = err1;
                }

                if (n > 0L || err != null)
                {
                    break;
                }

                n = copy(p, r.unread);
                r.unread = r.unread[n..];

            }

            return ;

        }

        public static var ErrTimeout = errors.New("timeout");

        // TimeoutReader returns ErrTimeout on the second read
        // with no data. Subsequent calls to read succeed.
        public static io.Reader TimeoutReader(io.Reader r)
        {
            return addr(new timeoutReader(r,0));
        }

        private partial struct timeoutReader
        {
            public io.Reader r;
            public long count;
        }

        private static (long, error) Read(this ptr<timeoutReader> _addr_r, slice<byte> p)
        {
            long _p0 = default;
            error _p0 = default!;
            ref timeoutReader r = ref _addr_r.val;

            r.count++;
            if (r.count == 2L)
            {
                return (0L, error.As(ErrTimeout)!);
            }

            return r.r.Read(p);

        }
    }
}}
