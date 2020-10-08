// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package riscv64 -- go2cs converted at 2020 October 08 04:27:25 UTC
// import "cmd/compile/internal/riscv64" ==> using riscv64 = go.cmd.compile.@internal.riscv64_package
// Original source: C:\Go\src\cmd\compile\internal\riscv64\gsubr.go
using gc = go.cmd.compile.@internal.gc_package;
using obj = go.cmd.@internal.obj_package;
using riscv = go.cmd.@internal.obj.riscv_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class riscv64_package
    {
        private static ptr<obj.Prog> ginsnop(ptr<gc.Progs> _addr_pp)
        {
            ref gc.Progs pp = ref _addr_pp.val;
 
            // Hardware nop is ADD $0, ZERO
            var p = pp.Prog(riscv.AADD);
            p.From.Type = obj.TYPE_CONST;
            p.Reg = riscv.REG_ZERO;
            p.To = new obj.Addr(Type:obj.TYPE_REG,Reg:riscv.REG_ZERO);
            return _addr_p!;

        }
    }
}}}}
