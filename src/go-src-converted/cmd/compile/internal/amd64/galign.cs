// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package amd64 -- go2cs converted at 2020 August 29 08:52:57 UTC
// import "cmd/compile/internal/amd64" ==> using amd64 = go.cmd.compile.@internal.amd64_package
// Original source: C:\Go\src\cmd\compile\internal\amd64\galign.go
using gc = go.cmd.compile.@internal.gc_package;
using x86 = go.cmd.@internal.obj.x86_package;
using objabi = go.cmd.@internal.objabi_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class amd64_package
    {
        private static var leaptr = x86.ALEAQ;

        public static void Init(ref gc.Arch arch)
        {
            arch.LinkArch = ref x86.Linkamd64;
            if (objabi.GOARCH == "amd64p32")
            {
                arch.LinkArch = ref x86.Linkamd64p32;
                leaptr = x86.ALEAL;
            }
            arch.REGSP = x86.REGSP;
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
