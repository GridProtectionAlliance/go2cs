// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !linux

// package main -- go2cs converted at 2020 August 29 08:24:26 UTC
// Original source: C:\Go\src\runtime\testdata\testprog\gettid_none.go

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
            return (false, false);
        }
    }
}
