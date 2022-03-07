// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package arm64 -- go2cs converted at 2022 March 06 22:47:33 UTC
// import "cmd/compile/internal/arm64" ==> using arm64 = go.cmd.compile.@internal.arm64_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\arm64\galign.go
using ssa = go.cmd.compile.@internal.ssa_package;
using ssagen = go.cmd.compile.@internal.ssagen_package;
using arm64 = go.cmd.@internal.obj.arm64_package;
using System;


namespace go.cmd.compile.@internal;

public static partial class arm64_package {

public static void Init(ptr<ssagen.ArchInfo> _addr_arch) {
    ref ssagen.ArchInfo arch = ref _addr_arch.val;

    arch.LinkArch = _addr_arm64.Linkarm64;
    arch.REGSP = arm64.REGSP;
    arch.MAXWIDTH = 1 << 50;

    arch.PadFrame = padframe;
    arch.ZeroRange = zerorange;
    arch.Ginsnop = ginsnop;
    arch.Ginsnopdefer = ginsnop;

    arch.SSAMarkMoves = (s, b) => {
    };
    arch.SSAGenValue = ssaGenValue;
    arch.SSAGenBlock = ssaGenBlock;

}

} // end arm64_package
