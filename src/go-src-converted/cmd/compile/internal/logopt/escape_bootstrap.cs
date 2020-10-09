// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !go1.8

// package logopt -- go2cs converted at 2020 October 09 05:24:10 UTC
// import "cmd/compile/internal/logopt" ==> using logopt = go.cmd.compile.@internal.logopt_package
// Original source: C:\Go\src\cmd\compile\internal\logopt\escape_bootstrap.go

using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class logopt_package
    {
        // For bootstrapping with an early version of Go
        private static @string pathEscape(@string s) => func((_, panic, __) =>
        {
            panic("This should never be called; the compiler is not fully bootstrapped if it is.");
        });
    }
}}}}
