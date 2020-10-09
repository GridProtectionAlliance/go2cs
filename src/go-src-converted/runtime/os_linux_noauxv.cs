// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build linux
// +build !arm,!arm64,!mips,!mipsle,!mips64,!mips64le,!s390x,!ppc64,!ppc64le

// package runtime -- go2cs converted at 2020 October 09 04:47:30 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\os_linux_noauxv.go

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
