// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !darwin,!linux

// package pprof -- go2cs converted at 2020 October 09 04:49:59 UTC
// import "runtime/pprof" ==> using pprof = go.runtime.pprof_package
// Original source: C:\Go\src\runtime\pprof\pprof_norusage.go
using io = go.io_package;
using static go.builtin;

namespace go {
namespace runtime
{
    public static partial class pprof_package
    {
        // Stub call for platforms that don't support rusage.
        private static void addMaxRSS(io.Writer w)
        {
        }
    }
}}
