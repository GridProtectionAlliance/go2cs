// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package bio -- go2cs converted at 2020 October 09 05:08:51 UTC
// import "cmd/internal/bio" ==> using bio = go.cmd.@internal.bio_package
// Original source: C:\Go\src\cmd\internal\bio\must.go
using io = go.io_package;
using log = go.log_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace @internal
{
    public static partial class bio_package
    {
        // MustClose closes Closer c and calls log.Fatal if it returns a non-nil error.
        public static void MustClose(io.Closer c)
        {
            {
                var err = c.Close();

                if (err != null)
                {
                    log.Fatal(err);
                }
            }

        }

        // MustWriter returns a Writer that wraps the provided Writer,
        // except that it calls log.Fatal instead of returning a non-nil error.
        public static io.Writer MustWriter(io.Writer w)
        {
            return new mustWriter(w);
        }

        private partial struct mustWriter
        {
            public io.Writer w;
        }

        private static (long, error) Write(this mustWriter w, slice<byte> b)
        {
            long _p0 = default;
            error _p0 = default!;

            var (n, err) = w.w.Write(b);
            if (err != null)
            {
                log.Fatal(err);
            }

            return (n, error.As(null!)!);

        }

        private static (long, error) WriteString(this mustWriter w, @string s)
        {
            long _p0 = default;
            error _p0 = default!;

            var (n, err) = io.WriteString(w.w, s);
            if (err != null)
            {
                log.Fatal(err);
            }

            return (n, error.As(null!)!);

        }
    }
}}}
