// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gob -- go2cs converted at 2020 October 08 03:42:43 UTC
// import "encoding/gob" ==> using gob = go.encoding.gob_package
// Original source: C:\Go\src\encoding\gob\error.go
using fmt = go.fmt_package;
using static go.builtin;

namespace go {
namespace encoding
{
    public static partial class gob_package
    {
        // Errors in decoding and encoding are handled using panic and recover.
        // Panics caused by user error (that is, everything except run-time panics
        // such as "index out of bounds" errors) do not leave the file that caused
        // them, but are instead turned into plain error returns. Encoding and
        // decoding functions and methods that do not return an error either use
        // panic to report an error or are guaranteed error-free.

        // A gobError is used to distinguish errors (panics) generated in this package.
        private partial struct gobError
        {
            public error err;
        }

        // errorf is like error_ but takes Printf-style arguments to construct an error.
        // It always prefixes the message with "gob: ".
        private static void errorf(@string format, params object[] args)
        {
            args = args.Clone();

            error_(fmt.Errorf("gob: " + format, args));
        }

        // error wraps the argument error and uses it as the argument to panic.
        private static void error_(error err) => func((_, panic, __) =>
        {
            panic(new gobError(err));
        });

        // catchError is meant to be used as a deferred function to turn a panic(gobError) into a
        // plain error. It overwrites the error return of the function that deferred its call.
        private static void catchError(ptr<error> _addr_err) => func((_, panic, __) =>
        {
            ref error err = ref _addr_err.val;

            {
                var e = recover();

                if (e != null)
                {
                    gobError (ge, ok) = e._<gobError>();
                    if (!ok)
                    {
                        panic(e);
                    }

                    err = error.As(ge.err)!;

                }

            }

        });
    }
}}
