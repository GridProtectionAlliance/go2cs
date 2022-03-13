// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package arm -- go2cs converted at 2022 March 13 05:58:48 UTC
// import "cmd/compile/internal/arm" ==> using arm = go.cmd.compile.@internal.arm_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\arm\galign.go
namespace go.cmd.compile.@internal;

using ssa = cmd.compile.@internal.ssa_package;
using ssagen = cmd.compile.@internal.ssagen_package;
using arm = cmd.@internal.obj.arm_package;
using buildcfg = @internal.buildcfg_package;
using System;

public static partial class arm_package {

public static void Init(ptr<ssagen.ArchInfo> _addr_arch) {
    ref ssagen.ArchInfo arch = ref _addr_arch.val;

    arch.LinkArch = _addr_arm.Linkarm;
    arch.REGSP = arm.REGSP;
    arch.MAXWIDTH = (1 << 32) - 1;
    arch.SoftFloat = buildcfg.GOARM == 5;
    arch.ZeroRange = zerorange;
    arch.Ginsnop = ginsnop;
    arch.Ginsnopdefer = ginsnop;

    arch.SSAMarkMoves = (s, b) => {
    };
    arch.SSAGenValue = ssaGenValue;
    arch.SSAGenBlock = ssaGenBlock;
}

} // end arm_package
