// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !linux
// +build !darwin

// package runtime -- go2cs converted at 2020 August 29 08:21:37 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\vdso_none.go

using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        private static void sysargs(int argc, ptr<ptr<byte>> argv)
        {
        }
    }
}
