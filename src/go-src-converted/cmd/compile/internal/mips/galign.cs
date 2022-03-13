// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package mips -- go2cs converted at 2022 March 13 05:58:49 UTC
// import "cmd/compile/internal/mips" ==> using mips = go.cmd.compile.@internal.mips_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\mips\galign.go
namespace go.cmd.compile.@internal;

using ssa = cmd.compile.@internal.ssa_package;
using ssagen = cmd.compile.@internal.ssagen_package;
using mips = cmd.@internal.obj.mips_package;
using buildcfg = @internal.buildcfg_package;
using System;

public static partial class mips_package {

public static void Init(ptr<ssagen.ArchInfo> _addr_arch) {
    ref ssagen.ArchInfo arch = ref _addr_arch.val;

    arch.LinkArch = _addr_mips.Linkmips;
    if (buildcfg.GOARCH == "mipsle") {
        arch.LinkArch = _addr_mips.Linkmipsle;
    }
    arch.REGSP = mips.REGSP;
    arch.MAXWIDTH = (1 << 31) - 1;
    arch.SoftFloat = (buildcfg.GOMIPS == "softfloat");
    arch.ZeroRange = zerorange;
    arch.Ginsnop = ginsnop;
    arch.Ginsnopdefer = ginsnop;
    arch.SSAMarkMoves = (s, b) => {
    };
    arch.SSAGenValue = ssaGenValue;
    arch.SSAGenBlock = ssaGenBlock;
}

} // end mips_package
