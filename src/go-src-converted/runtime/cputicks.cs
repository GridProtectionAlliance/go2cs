// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build !arm && !arm64 && !mips64 && !mips64le && !mips && !mipsle && !wasm
// +build !arm,!arm64,!mips64,!mips64le,!mips,!mipsle,!wasm

// package runtime -- go2cs converted at 2022 March 06 22:08:27 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\cputicks.go


namespace go;

public static partial class runtime_package {

    // careful: cputicks is not guaranteed to be monotonic! In particular, we have
    // noticed drift between cpus on certain os/arch combinations. See issue 8976.
private static long cputicks();

} // end runtime_package
