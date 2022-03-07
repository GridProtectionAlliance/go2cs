// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package s390x -- go2cs converted at 2022 March 06 22:47:35 UTC
// import "cmd/compile/internal/s390x" ==> using s390x = go.cmd.compile.@internal.s390x_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\s390x\galign.go
using ssagen = go.cmd.compile.@internal.ssagen_package;
using s390x = go.cmd.@internal.obj.s390x_package;

namespace go.cmd.compile.@internal;

public static partial class s390x_package {

public static void Init(ptr<ssagen.ArchInfo> _addr_arch) {
    ref ssagen.ArchInfo arch = ref _addr_arch.val;

    arch.LinkArch = _addr_s390x.Links390x;
    arch.REGSP = s390x.REGSP;
    arch.MAXWIDTH = 1 << 50;

    arch.ZeroRange = zerorange;
    arch.Ginsnop = ginsnop;
    arch.Ginsnopdefer = ginsnop;

    arch.SSAMarkMoves = ssaMarkMoves;
    arch.SSAGenValue = ssaGenValue;
    arch.SSAGenBlock = ssaGenBlock;
}

} // end s390x_package
