// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package mips64 -- go2cs converted at 2022 March 06 23:11:11 UTC
// import "cmd/compile/internal/mips64" ==> using mips64 = go.cmd.compile.@internal.mips64_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\mips64\ggen.go
using ir = go.cmd.compile.@internal.ir_package;
using objw = go.cmd.compile.@internal.objw_package;
using types = go.cmd.compile.@internal.types_package;
using obj = go.cmd.@internal.obj_package;
using mips = go.cmd.@internal.obj.mips_package;

namespace go.cmd.compile.@internal;

public static partial class mips64_package {

private static ptr<obj.Prog> zerorange(ptr<objw.Progs> _addr_pp, ptr<obj.Prog> _addr_p, long off, long cnt, ptr<uint> _addr__) {
    ref objw.Progs pp = ref _addr_pp.val;
    ref obj.Prog p = ref _addr_p.val;
    ref uint _ = ref _addr__.val;

    if (cnt == 0) {
        return _addr_p!;
    }
    if (cnt < int64(4 * types.PtrSize)) {
        {
            var i = int64(0);

            while (i < cnt) {
                p = pp.Append(p, mips.AMOVV, obj.TYPE_REG, mips.REGZERO, 0, obj.TYPE_MEM, mips.REGSP, 8 + off + i);
                i += int64(types.PtrSize);
            }
        }

    }
    else if (cnt <= int64(128 * types.PtrSize)) {
        p = pp.Append(p, mips.AADDV, obj.TYPE_CONST, 0, 8 + off - 8, obj.TYPE_REG, mips.REGRT1, 0);
        p.Reg = mips.REGSP;
        p = pp.Append(p, obj.ADUFFZERO, obj.TYPE_NONE, 0, 0, obj.TYPE_MEM, 0, 0);
        p.To.Name = obj.NAME_EXTERN;
        p.To.Sym = ir.Syms.Duffzero;
        p.To.Offset = 8 * (128 - cnt / int64(types.PtrSize));
    }
    else
 { 
        //    ADDV    $(8+frame+lo-8), SP, r1
        //    ADDV    $cnt, r1, r2
        // loop:
        //    MOVV    R0, (Widthptr)r1
        //    ADDV    $Widthptr, r1
        //    BNE        r1, r2, loop
        p = pp.Append(p, mips.AADDV, obj.TYPE_CONST, 0, 8 + off - 8, obj.TYPE_REG, mips.REGRT1, 0);
        p.Reg = mips.REGSP;
        p = pp.Append(p, mips.AADDV, obj.TYPE_CONST, 0, cnt, obj.TYPE_REG, mips.REGRT2, 0);
        p.Reg = mips.REGRT1;
        p = pp.Append(p, mips.AMOVV, obj.TYPE_REG, mips.REGZERO, 0, obj.TYPE_MEM, mips.REGRT1, int64(types.PtrSize));
        var p1 = p;
        p = pp.Append(p, mips.AADDV, obj.TYPE_CONST, 0, int64(types.PtrSize), obj.TYPE_REG, mips.REGRT1, 0);
        p = pp.Append(p, mips.ABNE, obj.TYPE_REG, mips.REGRT1, 0, obj.TYPE_BRANCH, 0, 0);
        p.Reg = mips.REGRT2;
        p.To.SetTarget(p1);

    }
    return _addr_p!;

}

private static ptr<obj.Prog> ginsnop(ptr<objw.Progs> _addr_pp) {
    ref objw.Progs pp = ref _addr_pp.val;

    var p = pp.Prog(mips.ANOR);
    p.From.Type = obj.TYPE_REG;
    p.From.Reg = mips.REG_R0;
    p.To.Type = obj.TYPE_REG;
    p.To.Reg = mips.REG_R0;
    return _addr_p!;
}

} // end mips64_package
