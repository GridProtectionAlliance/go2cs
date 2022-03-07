// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package mips64 -- go2cs converted at 2022 March 06 22:47:34 UTC
// import "cmd/compile/internal/mips64" ==> using mips64 = go.cmd.compile.@internal.mips64_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\mips64\galign.go
using ssa = go.cmd.compile.@internal.ssa_package;
using ssagen = go.cmd.compile.@internal.ssagen_package;
using mips = go.cmd.@internal.obj.mips_package;
using buildcfg = go.@internal.buildcfg_package;
using System;


namespace go.cmd.compile.@internal;

public static partial class mips64_package {

public static void Init(ptr<ssagen.ArchInfo> _addr_arch) {
    ref ssagen.ArchInfo arch = ref _addr_arch.val;

    arch.LinkArch = _addr_mips.Linkmips64;
    if (buildcfg.GOARCH == "mips64le") {
        arch.LinkArch = _addr_mips.Linkmips64le;
    }
    arch.REGSP = mips.REGSP;
    arch.MAXWIDTH = 1 << 50;
    arch.SoftFloat = buildcfg.GOMIPS64 == "softfloat";
    arch.ZeroRange = zerorange;
    arch.Ginsnop = ginsnop;
    arch.Ginsnopdefer = ginsnop;

    arch.SSAMarkMoves = (s, b) => {
    };
    arch.SSAGenValue = ssaGenValue;
    arch.SSAGenBlock = ssaGenBlock;

}

} // end mips64_package
