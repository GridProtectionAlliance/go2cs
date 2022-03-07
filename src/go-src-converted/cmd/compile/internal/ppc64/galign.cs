// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ppc64 -- go2cs converted at 2022 March 06 22:47:35 UTC
// import "cmd/compile/internal/ppc64" ==> using ppc64 = go.cmd.compile.@internal.ppc64_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\ppc64\galign.go
using ssagen = go.cmd.compile.@internal.ssagen_package;
using ppc64 = go.cmd.@internal.obj.ppc64_package;
using buildcfg = go.@internal.buildcfg_package;

namespace go.cmd.compile.@internal;

public static partial class ppc64_package {

public static void Init(ptr<ssagen.ArchInfo> _addr_arch) {
    ref ssagen.ArchInfo arch = ref _addr_arch.val;

    arch.LinkArch = _addr_ppc64.Linkppc64;
    if (buildcfg.GOARCH == "ppc64le") {
        arch.LinkArch = _addr_ppc64.Linkppc64le;
    }
    arch.REGSP = ppc64.REGSP;
    arch.MAXWIDTH = 1 << 60;

    arch.ZeroRange = zerorange;
    arch.Ginsnop = ginsnop;
    arch.Ginsnopdefer = ginsnopdefer;

    arch.SSAMarkMoves = ssaMarkMoves;
    arch.SSAGenValue = ssaGenValue;
    arch.SSAGenBlock = ssaGenBlock;

}

} // end ppc64_package
