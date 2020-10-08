// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package iotest -- go2cs converted at 2020 October 08 04:36:39 UTC
// import "testing/iotest" ==> using iotest = go.testing.iotest_package
// Original source: C:\Go\src\testing\iotest\logger.go
using io = go.io_package;
using log = go.log_package;
using static go.builtin;

namespace go {
namespace testing
{
    public static partial class iotest_package
    {
        private partial struct writeLogger
        {
            public @string prefix;
            public io.Writer w;
        }

        private static (long, error) Write(this ptr<writeLogger> _addr_l, slice<byte> p)
        {
            long n = default;
            error err = default!;
            ref writeLogger l = ref _addr_l.val;

            n, err = l.w.Write(p);
            if (err != null)
            {
                log.Printf("%s %x: %v", l.prefix, p[0L..n], err);
            }
            else
            {
                log.Printf("%s %x", l.prefix, p[0L..n]);
            }

            return ;

        }

        // NewWriteLogger returns a writer that behaves like w except
        // that it logs (using log.Printf) each write to standard error,
        // printing the prefix and the hexadecimal data written.
        public static io.Writer NewWriteLogger(@string prefix, io.Writer w)
        {
            return addr(new writeLogger(prefix,w));
        }

        private partial struct readLogger
        {
            public @string prefix;
            public io.Reader r;
        }

        private static (long, error) Read(this ptr<readLogger> _addr_l, slice<byte> p)
        {
            long n = default;
            error err = default!;
            ref readLogger l = ref _addr_l.val;

            n, err = l.r.Read(p);
            if (err != null)
            {
                log.Printf("%s %x: %v", l.prefix, p[0L..n], err);
            }
            else
            {
                log.Printf("%s %x", l.prefix, p[0L..n]);
            }

            return ;

        }

        // NewReadLogger returns a reader that behaves like r except
        // that it logs (using log.Printf) each read to standard error,
        // printing the prefix and the hexadecimal data read.
        public static io.Reader NewReadLogger(@string prefix, io.Reader r)
        {
            return addr(new readLogger(prefix,r));
        }
    }
}}
