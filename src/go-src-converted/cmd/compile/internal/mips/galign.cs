// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package mips -- go2cs converted at 2020 October 09 05:24:02 UTC
// import "cmd/compile/internal/mips" ==> using mips = go.cmd.compile.@internal.mips_package
// Original source: C:\Go\src\cmd\compile\internal\mips\galign.go
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
    public static partial class mips_package
    {
        public static void Init(ptr<gc.Arch> _addr_arch)
        {
            ref gc.Arch arch = ref _addr_arch.val;

            arch.LinkArch = _addr_mips.Linkmips;
            if (objabi.GOARCH == "mipsle")
            {
                arch.LinkArch = _addr_mips.Linkmipsle;
            }
            arch.REGSP = mips.REGSP;
            arch.MAXWIDTH = (1L << (int)(31L)) - 1L;
            arch.SoftFloat = (objabi.GOMIPS == "softfloat");
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
