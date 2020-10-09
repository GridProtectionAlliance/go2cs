// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !go1.8

// package gc -- go2cs converted at 2020 October 09 05:40:37 UTC
// import "cmd/compile/internal/gc" ==> using gc = go.cmd.compile.@internal.gc_package
// Original source: C:\Go\src\cmd\compile\internal\gc\bootstrap.go
using runtime = go.runtime_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class gc_package
    {
        private static void startMutexProfiling()
        {
            Fatalf("mutex profiling unavailable in version %v", runtime.Version());
        }
    }
}}}}
