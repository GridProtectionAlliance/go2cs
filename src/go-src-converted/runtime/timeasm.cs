// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Declarations for operating systems implementing time.now directly in assembly.
// Those systems are also expected to have nanotime subtract startNano,
// so that time.now and nanotime return the same monotonic clock readings.

// +build darwin,amd64 darwin,386 windows

// package runtime -- go2cs converted at 2020 August 29 08:21:16 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\timeasm.go
using _@unsafe_ = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        //go:linkname time_now time.now
        private static (long, int, long) time_now()
;
    }
}
