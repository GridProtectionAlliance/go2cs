// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Solaris code that doesn't also apply to illumos.

// +build !illumos

// package runtime -- go2cs converted at 2020 October 09 04:47:32 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\os_only_solaris.go

using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        private static int getncpu()
        {
            var n = int32(sysconf(__SC_NPROCESSORS_ONLN));
            if (n < 1L)
            {
                return 1L;
            }
            return n;

        }
    }
}
