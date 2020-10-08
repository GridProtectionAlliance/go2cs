// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2020 October 08 04:26:44 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Go\src\cmd\compile\internal\ssa\stackframe.go

using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class ssa_package
    {
        // stackframe calls back into the frontend to assign frame offsets.
        private static void stackframe(ptr<Func> _addr_f)
        {
            ref Func f = ref _addr_f.val;

            f.fe.AllocFrame(f);
        }
    }
}}}}
