// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2020 August 29 08:54:40 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Go\src\cmd\compile\internal\ssa\opt.go

using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class ssa_package
    {
        // machine-independent optimization
        private static void opt(ref Func f)
        {
            applyRewrite(f, rewriteBlockgeneric, rewriteValuegeneric);
        }
    }
}}}}
