// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package arm64 -- go2cs converted at 2020 October 08 04:09:26 UTC
// import "cmd/compile/internal/arm64" ==> using arm64 = go.cmd.compile.@internal.arm64_package
// Original source: C:\Go\src\cmd\compile\internal\arm64\galign.go
using gc = go.cmd.compile.@internal.gc_package;
using ssa = go.cmd.compile.@internal.ssa_package;
using arm64 = go.cmd.@internal.obj.arm64_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class arm64_package
    {
        public static void Init(ptr<gc.Arch> _addr_arch)
        {
            ref gc.Arch arch = ref _addr_arch.val;

            arch.LinkArch = _addr_arm64.Linkarm64;
            arch.REGSP = arm64.REGSP;
            arch.MAXWIDTH = 1L << (int)(50L);

            arch.PadFrame = padframe;
            arch.ZeroRange = zerorange;
            arch.Ginsnop = ginsnop;
            arch.Ginsnopdefer = ginsnop;

            arch.SSAMarkMoves = (s, b) =>
            {
            };
            arch.SSAGenValue = ssaGenValue;
            arch.SSAGenBlock = ssaGenBlock;

        }
    }
}}}}
