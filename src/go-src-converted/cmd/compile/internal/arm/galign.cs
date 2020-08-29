// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package arm -- go2cs converted at 2020 August 29 08:52:57 UTC
// import "cmd/compile/internal/arm" ==> using arm = go.cmd.compile.@internal.arm_package
// Original source: C:\Go\src\cmd\compile\internal\arm\galign.go
using gc = go.cmd.compile.@internal.gc_package;
using ssa = go.cmd.compile.@internal.ssa_package;
using arm = go.cmd.@internal.obj.arm_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class arm_package
    {
        public static void Init(ref gc.Arch arch)
        {
            arch.LinkArch = ref arm.Linkarm;
            arch.REGSP = arm.REGSP;
            arch.MAXWIDTH = (1L << (int)(32L)) - 1L;

            arch.ZeroRange = zerorange;
            arch.ZeroAuto = zeroAuto;
            arch.Ginsnop = ginsnop;

            arch.SSAMarkMoves = (s, b) =>
            {
            };
            arch.SSAGenValue = ssaGenValue;
            arch.SSAGenBlock = ssaGenBlock;
        }
    }
}}}}
