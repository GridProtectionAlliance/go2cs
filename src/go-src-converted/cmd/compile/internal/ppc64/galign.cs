// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ppc64 -- go2cs converted at 2020 October 09 05:24:03 UTC
// import "cmd/compile/internal/ppc64" ==> using ppc64 = go.cmd.compile.@internal.ppc64_package
// Original source: C:\Go\src\cmd\compile\internal\ppc64\galign.go
using gc = go.cmd.compile.@internal.gc_package;
using ppc64 = go.cmd.@internal.obj.ppc64_package;
using objabi = go.cmd.@internal.objabi_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class ppc64_package
    {
        public static void Init(ptr<gc.Arch> _addr_arch)
        {
            ref gc.Arch arch = ref _addr_arch.val;

            arch.LinkArch = _addr_ppc64.Linkppc64;
            if (objabi.GOARCH == "ppc64le")
            {
                arch.LinkArch = _addr_ppc64.Linkppc64le;
            }
            arch.REGSP = ppc64.REGSP;
            arch.MAXWIDTH = 1L << (int)(60L);

            arch.ZeroRange = zerorange;
            arch.Ginsnop = ginsnop;
            arch.Ginsnopdefer = ginsnopdefer;

            arch.SSAMarkMoves = ssaMarkMoves;
            arch.SSAGenValue = ssaGenValue;
            arch.SSAGenBlock = ssaGenBlock;

        }
    }
}}}}
