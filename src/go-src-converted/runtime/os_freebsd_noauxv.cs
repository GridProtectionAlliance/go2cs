// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build freebsd
// +build !arm,!arm64

// package runtime -- go2cs converted at 2020 October 09 04:47:26 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\os_freebsd_noauxv.go

using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        private static void archauxv(System.UIntPtr tag, System.UIntPtr val)
        {
        }
    }
}
