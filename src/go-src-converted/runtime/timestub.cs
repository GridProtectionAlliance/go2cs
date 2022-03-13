// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Declarations for operating systems implementing time.now
// indirectly, in terms of walltime and nanotime assembly.

//go:build !faketime && !windows && !(linux && amd64)
// +build !faketime
// +build !windows
// +build !linux !amd64

// package runtime -- go2cs converted at 2022 March 13 05:27:18 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\timestub.go
namespace go;

using _@unsafe_ = @unsafe_package;

public static partial class runtime_package { // for go:linkname

//go:linkname time_now time.now
private static (long, int, long) time_now() {
    long sec = default;
    int nsec = default;
    long mono = default;

    sec, nsec = walltime();
    return (sec, nsec, nanotime());
}

} // end runtime_package
