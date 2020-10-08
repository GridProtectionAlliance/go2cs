// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build linux,!386,!amd64,!arm,!arm64,!mips64,!mips64le,!ppc64,!ppc64le !linux

// package runtime -- go2cs converted at 2020 October 08 03:24:22 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\vdso_in_none.go

using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        // A dummy version of inVDSOPage for targets that don't use a VDSO.
        private static bool inVDSOPage(System.UIntPtr pc)
        {
            return false;
        }
    }
}
