// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !linux

// package main -- go2cs converted at 2020 October 08 03:43:45 UTC
// Original source: C:\Go\src\runtime\testdata\testprog\syscalls_none.go

using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private static long gettid()
        {
            return 0L;
        }

        private static (bool, bool) tidExists(long tid)
        {
            bool exists = default;
            bool supported = default;

            return (false, false);
        }

        private static (@string, error) getcwd()
        {
            @string _p0 = default;
            error _p0 = default!;

            return ("", error.As(null!)!);
        }

        private static error unshareFs()
        {
            return error.As(null!)!;
        }

        private static error chdir(@string path)
        {
            return error.As(null!)!;
        }
    }
}
