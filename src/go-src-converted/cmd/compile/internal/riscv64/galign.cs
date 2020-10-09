// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package riscv64 -- go2cs converted at 2020 October 09 05:24:03 UTC
// import "cmd/compile/internal/riscv64" ==> using riscv64 = go.cmd.compile.@internal.riscv64_package
// Original source: C:\Go\src\cmd\compile\internal\riscv64\galign.go
using gc = go.cmd.compile.@internal.gc_package;
using riscv = go.cmd.@internal.obj.riscv_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class riscv64_package
    {
        public static void Init(ptr<gc.Arch> _addr_arch)
        {
            ref gc.Arch arch = ref _addr_arch.val;

            arch.LinkArch = _addr_riscv.LinkRISCV64;

            arch.REGSP = riscv.REG_SP;
            arch.MAXWIDTH = 1L << (int)(50L);

            arch.Ginsnop = ginsnop;
            arch.Ginsnopdefer = ginsnop;
            arch.ZeroRange = zeroRange;

            arch.SSAMarkMoves = ssaMarkMoves;
            arch.SSAGenValue = ssaGenValue;
            arch.SSAGenBlock = ssaGenBlock;
        }
    }
}}}}
