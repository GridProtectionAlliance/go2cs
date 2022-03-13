// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package mips -- go2cs converted at 2022 March 13 06:24:37 UTC
// import "cmd/compile/internal/mips" ==> using mips = go.cmd.compile.@internal.mips_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\mips\ggen.go
namespace go.cmd.compile.@internal;

using @base = cmd.compile.@internal.@base_package;
using objw = cmd.compile.@internal.objw_package;
using types = cmd.compile.@internal.types_package;
using obj = cmd.@internal.obj_package;
using mips = cmd.@internal.obj.mips_package;


// TODO(mips): implement DUFFZERO

public static partial class mips_package {

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
                p = pp.Append(p, mips.AMOVW, obj.TYPE_REG, mips.REGZERO, 0, obj.TYPE_MEM, mips.REGSP, @base.Ctxt.FixedFrameSize() + off + i);
                i += int64(types.PtrSize);
            }
    else

        }
    } { 
        //fmt.Printf("zerorange frame:%v, lo: %v, hi:%v \n", frame ,lo, hi)
        //    ADD     $(FIXED_FRAME+frame+lo-4), SP, r1
        //    ADD     $cnt, r1, r2
        // loop:
        //    MOVW    R0, (Widthptr)r1
        //    ADD     $Widthptr, r1
        //    BNE        r1, r2, loop
        p = pp.Append(p, mips.AADD, obj.TYPE_CONST, 0, @base.Ctxt.FixedFrameSize() + off - 4, obj.TYPE_REG, mips.REGRT1, 0);
        p.Reg = mips.REGSP;
        p = pp.Append(p, mips.AADD, obj.TYPE_CONST, 0, cnt, obj.TYPE_REG, mips.REGRT2, 0);
        p.Reg = mips.REGRT1;
        p = pp.Append(p, mips.AMOVW, obj.TYPE_REG, mips.REGZERO, 0, obj.TYPE_MEM, mips.REGRT1, int64(types.PtrSize));
        var p1 = p;
        p = pp.Append(p, mips.AADD, obj.TYPE_CONST, 0, int64(types.PtrSize), obj.TYPE_REG, mips.REGRT1, 0);
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

} // end mips_package
