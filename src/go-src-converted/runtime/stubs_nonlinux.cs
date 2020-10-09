// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !linux

// package runtime -- go2cs converted at 2020 October 09 04:48:48 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\stubs_nonlinux.go

using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        // sbrk0 returns the current process brk, or 0 if not implemented.
        private static System.UIntPtr sbrk0()
        {
            return 0L;
        }
    }
}
