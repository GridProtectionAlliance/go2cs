// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package amd64 -- go2cs converted at 2022 March 13 05:58:48 UTC
// import "cmd/compile/internal/amd64" ==> using amd64 = go.cmd.compile.@internal.amd64_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\amd64\galign.go
namespace go.cmd.compile.@internal;

using ssagen = cmd.compile.@internal.ssagen_package;
using x86 = cmd.@internal.obj.x86_package;

public static partial class amd64_package {

private static var leaptr = x86.ALEAQ;

public static void Init(ptr<ssagen.ArchInfo> _addr_arch) {
    ref ssagen.ArchInfo arch = ref _addr_arch.val;

    arch.LinkArch = _addr_x86.Linkamd64;
    arch.REGSP = x86.REGSP;
    arch.MAXWIDTH = 1 << 50;

    arch.ZeroRange = zerorange;
    arch.Ginsnop = ginsnop;
    arch.Ginsnopdefer = ginsnop;

    arch.SSAMarkMoves = ssaMarkMoves;
    arch.SSAGenValue = ssaGenValue;
    arch.SSAGenBlock = ssaGenBlock;
    arch.LoadRegResults = loadRegResults;
    arch.SpillArgReg = spillArgReg;
}

} // end amd64_package
