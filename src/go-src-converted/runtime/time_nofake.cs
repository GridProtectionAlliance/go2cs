// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build !faketime
// +build !faketime

// package runtime -- go2cs converted at 2022 March 06 22:12:15 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\time_nofake.go
using @unsafe = go.@unsafe_package;

namespace go;

public static partial class runtime_package {

    // faketime is the simulated time in nanoseconds since 1970 for the
    // playground.
    //
    // Zero means not to use faketime.
private static long faketime = default;

//go:nosplit
private static long nanotime() {
    return nanotime1();
}

// write must be nosplit on Windows (see write1)
//
//go:nosplit
private static int write(System.UIntPtr fd, unsafe.Pointer p, int n) {
    return write1(fd, p, n);
}

} // end runtime_package
