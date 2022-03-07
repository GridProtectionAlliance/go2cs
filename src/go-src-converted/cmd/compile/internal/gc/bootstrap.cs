// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build !go1.8
// +build !go1.8

// package gc -- go2cs converted at 2022 March 06 22:47:34 UTC
// import "cmd/compile/internal/gc" ==> using gc = go.cmd.compile.@internal.gc_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\gc\bootstrap.go
using @base = go.cmd.compile.@internal.@base_package;
using runtime = go.runtime_package;

namespace go.cmd.compile.@internal;

public static partial class gc_package {

private static void startMutexProfiling() {
    @base.Fatalf("mutex profiling unavailable in version %v", runtime.Version());
}

} // end gc_package
