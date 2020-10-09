// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !arm
// +build !arm64
// +build !mips64
// +build !mips64le
// +build !mips
// +build !mipsle
// +build !wasm

// package runtime -- go2cs converted at 2020 October 09 04:45:42 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\cputicks.go

using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        // careful: cputicks is not guaranteed to be monotonic! In particular, we have
        // noticed drift between cpus on certain os/arch combinations. See issue 8976.
        private static long cputicks()
;
    }
}
