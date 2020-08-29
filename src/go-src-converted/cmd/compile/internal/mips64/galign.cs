// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package mips64 -- go2cs converted at 2020 August 29 08:53:05 UTC
// import "cmd/compile/internal/mips64" ==> using mips64 = go.cmd.compile.@internal.mips64_package
// Original source: C:\Go\src\cmd\compile\internal\mips64\galign.go
using gc = go.cmd.compile.@internal.gc_package;
using ssa = go.cmd.compile.@internal.ssa_package;
using mips = go.cmd.@internal.obj.mips_package;
using objabi = go.cmd.@internal.objabi_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class mips64_package
    {
        public static void Init(ref gc.Arch arch)
        {
            arch.LinkArch = ref mips.Linkmips64;
            if (objabi.GOARCH == "mips64le")
            {
                arch.LinkArch = ref mips.Linkmips64le;
            }
            arch.REGSP = mips.REGSP;
            arch.MAXWIDTH = 1L << (int)(50L);

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
