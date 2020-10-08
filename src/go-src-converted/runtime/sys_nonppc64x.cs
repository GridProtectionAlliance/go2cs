// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !ppc64,!ppc64le

// package runtime -- go2cs converted at 2020 October 08 03:23:59 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\sys_nonppc64x.go

using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        private static void prepGoExitFrame(System.UIntPtr sp)
        {
        }
    }
}
