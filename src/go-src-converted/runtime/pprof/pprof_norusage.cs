// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build !darwin && !linux
// +build !darwin,!linux

// package pprof -- go2cs converted at 2022 March 13 05:28:49 UTC
// import "runtime/pprof" ==> using pprof = go.runtime.pprof_package
// Original source: C:\Program Files\Go\src\runtime\pprof\pprof_norusage.go
namespace go.runtime;

using io = io_package;


// Stub call for platforms that don't support rusage.

public static partial class pprof_package {

private static void addMaxRSS(io.Writer w) {
}

} // end pprof_package
