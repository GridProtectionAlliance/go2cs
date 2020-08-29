// compile

// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Test that a defer in a function with no return
// statement will compile correctly.

// package foo -- go2cs converted at 2020 August 29 09:58:11 UTC
// import "cmd/compile/internal/gc.foo" ==> using foo = go.cmd.compile.@internal.gc.foo_package
// Original source: C:\Go\src\cmd\compile\internal\gc\testdata\deferNoReturn.go

using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class foo_package
    {
        private static void deferNoReturn_ssa() => func((defer, _, __) =>
        {
            defer(() =>
            {
                println("returned");

            }());
            while (true)
            {
                println("loop");
            }
        });
    }
}}}}
