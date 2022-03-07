// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package arm -- go2cs converted at 2022 March 06 23:14:40 UTC
// import "cmd/compile/internal/arm" ==> using arm = go.cmd.compile.@internal.arm_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\arm\ggen.go
using ir = go.cmd.compile.@internal.ir_package;
using objw = go.cmd.compile.@internal.objw_package;
using types = go.cmd.compile.@internal.types_package;
using obj = go.cmd.@internal.obj_package;
using arm = go.cmd.@internal.obj.arm_package;

namespace go.cmd.compile.@internal;

public static partial class arm_package {

private static ptr<obj.Prog> zerorange(ptr<objw.Progs> _addr_pp, ptr<obj.Prog> _addr_p, long off, long cnt, ptr<uint> _addr_r0) {
    ref objw.Progs pp = ref _addr_pp.val;
    ref obj.Prog p = ref _addr_p.val;
    ref uint r0 = ref _addr_r0.val;

    if (cnt == 0) {
        return _addr_p!;
    }
    if (r0 == 0.val) {
        p = pp.Append(p, arm.AMOVW, obj.TYPE_CONST, 0, 0, obj.TYPE_REG, arm.REG_R0, 0);
        r0 = 1;
    }
    if (cnt < int64(4 * types.PtrSize)) {
        {
            var i = int64(0);

            while (i < cnt) {
                p = pp.Append(p, arm.AMOVW, obj.TYPE_REG, arm.REG_R0, 0, obj.TYPE_MEM, arm.REGSP, 4 + off + i);
                i += int64(types.PtrSize);
            }
        }

    }
    else if (cnt <= int64(128 * types.PtrSize)) {
        p = pp.Append(p, arm.AADD, obj.TYPE_CONST, 0, 4 + off, obj.TYPE_REG, arm.REG_R1, 0);
        p.Reg = arm.REGSP;
        p = pp.Append(p, obj.ADUFFZERO, obj.TYPE_NONE, 0, 0, obj.TYPE_MEM, 0, 0);
        p.To.Name = obj.NAME_EXTERN;
        p.To.Sym = ir.Syms.Duffzero;
        p.To.Offset = 4 * (128 - cnt / int64(types.PtrSize));
    }
    else
 {
        p = pp.Append(p, arm.AADD, obj.TYPE_CONST, 0, 4 + off, obj.TYPE_REG, arm.REG_R1, 0);
        p.Reg = arm.REGSP;
        p = pp.Append(p, arm.AADD, obj.TYPE_CONST, 0, cnt, obj.TYPE_REG, arm.REG_R2, 0);
        p.Reg = arm.REG_R1;
        p = pp.Append(p, arm.AMOVW, obj.TYPE_REG, arm.REG_R0, 0, obj.TYPE_MEM, arm.REG_R1, 4);
        var p1 = p;
        p.Scond |= arm.C_PBIT;
        p = pp.Append(p, arm.ACMP, obj.TYPE_REG, arm.REG_R1, 0, obj.TYPE_NONE, 0, 0);
        p.Reg = arm.REG_R2;
        p = pp.Append(p, arm.ABNE, obj.TYPE_NONE, 0, 0, obj.TYPE_BRANCH, 0, 0);
        p.To.SetTarget(p1);
    }
    return _addr_p!;

}

private static ptr<obj.Prog> ginsnop(ptr<objw.Progs> _addr_pp) {
    ref objw.Progs pp = ref _addr_pp.val;

    var p = pp.Prog(arm.AAND);
    p.From.Type = obj.TYPE_REG;
    p.From.Reg = arm.REG_R0;
    p.To.Type = obj.TYPE_REG;
    p.To.Reg = arm.REG_R0;
    p.Scond = arm.C_SCOND_EQ;
    return _addr_p!;
}

} // end arm_package
