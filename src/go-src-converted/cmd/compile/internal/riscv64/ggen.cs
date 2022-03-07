// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package riscv64 -- go2cs converted at 2022 March 06 23:10:52 UTC
// import "cmd/compile/internal/riscv64" ==> using riscv64 = go.cmd.compile.@internal.riscv64_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\riscv64\ggen.go
using @base = go.cmd.compile.@internal.@base_package;
using ir = go.cmd.compile.@internal.ir_package;
using objw = go.cmd.compile.@internal.objw_package;
using types = go.cmd.compile.@internal.types_package;
using obj = go.cmd.@internal.obj_package;
using riscv = go.cmd.@internal.obj.riscv_package;

namespace go.cmd.compile.@internal;

public static partial class riscv64_package {

private static ptr<obj.Prog> zeroRange(ptr<objw.Progs> _addr_pp, ptr<obj.Prog> _addr_p, long off, long cnt, ptr<uint> _addr__) {
    ref objw.Progs pp = ref _addr_pp.val;
    ref obj.Prog p = ref _addr_p.val;
    ref uint _ = ref _addr__.val;

    if (cnt == 0) {
        return _addr_p!;
    }
    off += @base.Ctxt.FixedFrameSize();

    if (cnt < int64(4 * types.PtrSize)) {
        {
            var i = int64(0);

            while (i < cnt) {
                p = pp.Append(p, riscv.AMOV, obj.TYPE_REG, riscv.REG_ZERO, 0, obj.TYPE_MEM, riscv.REG_SP, off + i);
                i += int64(types.PtrSize);
            }
        }
        return _addr_p!;

    }
    if (cnt <= int64(128 * types.PtrSize)) {
        p = pp.Append(p, riscv.AADDI, obj.TYPE_CONST, 0, off, obj.TYPE_REG, riscv.REG_A0, 0);
        p.Reg = riscv.REG_SP;
        p = pp.Append(p, obj.ADUFFZERO, obj.TYPE_NONE, 0, 0, obj.TYPE_MEM, 0, 0);
        p.To.Name = obj.NAME_EXTERN;
        p.To.Sym = ir.Syms.Duffzero;
        p.To.Offset = 8 * (128 - cnt / int64(types.PtrSize));
        return _addr_p!;
    }
    p = pp.Append(p, riscv.AADD, obj.TYPE_CONST, 0, off, obj.TYPE_REG, riscv.REG_T0, 0);
    p.Reg = riscv.REG_SP;
    p = pp.Append(p, riscv.AADD, obj.TYPE_CONST, 0, cnt, obj.TYPE_REG, riscv.REG_T1, 0);
    p.Reg = riscv.REG_T0;
    p = pp.Append(p, riscv.AMOV, obj.TYPE_REG, riscv.REG_ZERO, 0, obj.TYPE_MEM, riscv.REG_T0, 0);
    var loop = p;
    p = pp.Append(p, riscv.AADD, obj.TYPE_CONST, 0, int64(types.PtrSize), obj.TYPE_REG, riscv.REG_T0, 0);
    p = pp.Append(p, riscv.ABNE, obj.TYPE_REG, riscv.REG_T0, 0, obj.TYPE_BRANCH, 0, 0);
    p.Reg = riscv.REG_T1;
    p.To.SetTarget(loop);
    return _addr_p!;

}

} // end riscv64_package
