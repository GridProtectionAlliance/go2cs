// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package s390x -- go2cs converted at 2020 August 29 08:53:07 UTC
// import "cmd/compile/internal/s390x" ==> using s390x = go.cmd.compile.@internal.s390x_package
// Original source: C:\Go\src\cmd\compile\internal\s390x\galign.go
using gc = go.cmd.compile.@internal.gc_package;
using s390x = go.cmd.@internal.obj.s390x_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class s390x_package
    {
        public static void Init(ref gc.Arch arch)
        {
            arch.LinkArch = ref s390x.Links390x;
            arch.REGSP = s390x.REGSP;
            arch.MAXWIDTH = 1L << (int)(50L);

            arch.ZeroRange = zerorange;
            arch.ZeroAuto = zeroAuto;
            arch.Ginsnop = ginsnop;

            arch.SSAMarkMoves = ssaMarkMoves;
            arch.SSAGenValue = ssaGenValue;
            arch.SSAGenBlock = ssaGenBlock;
        }
    }
}}}}