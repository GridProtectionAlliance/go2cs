// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package riscv64 -- go2cs converted at 2022 March 13 05:58:50 UTC
// import "cmd/compile/internal/riscv64" ==> using riscv64 = go.cmd.compile.@internal.riscv64_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\riscv64\galign.go
namespace go.cmd.compile.@internal;

using ssagen = cmd.compile.@internal.ssagen_package;
using riscv = cmd.@internal.obj.riscv_package;

public static partial class riscv64_package {

public static void Init(ptr<ssagen.ArchInfo> _addr_arch) {
    ref ssagen.ArchInfo arch = ref _addr_arch.val;

    arch.LinkArch = _addr_riscv.LinkRISCV64;

    arch.REGSP = riscv.REG_SP;
    arch.MAXWIDTH = 1 << 50;

    arch.Ginsnop = ginsnop;
    arch.Ginsnopdefer = ginsnop;
    arch.ZeroRange = zeroRange;

    arch.SSAMarkMoves = ssaMarkMoves;
    arch.SSAGenValue = ssaGenValue;
    arch.SSAGenBlock = ssaGenBlock;
}

} // end riscv64_package
