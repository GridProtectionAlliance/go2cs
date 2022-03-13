// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ppc64 -- go2cs converted at 2022 March 13 06:24:15 UTC
// import "cmd/compile/internal/ppc64" ==> using ppc64 = go.cmd.compile.@internal.ppc64_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\ppc64\ggen.go
namespace go.cmd.compile.@internal;

using @base = cmd.compile.@internal.@base_package;
using ir = cmd.compile.@internal.ir_package;
using objw = cmd.compile.@internal.objw_package;
using types = cmd.compile.@internal.types_package;
using obj = cmd.@internal.obj_package;
using ppc64 = cmd.@internal.obj.ppc64_package;

public static partial class ppc64_package {

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
                p = pp.Append(p, ppc64.AMOVD, obj.TYPE_REG, ppc64.REGZERO, 0, obj.TYPE_MEM, ppc64.REGSP, @base.Ctxt.FixedFrameSize() + off + i);
                i += int64(types.PtrSize);
            }
        }
    }
    else if (cnt <= int64(128 * types.PtrSize)) {
        p = pp.Append(p, ppc64.AADD, obj.TYPE_CONST, 0, @base.Ctxt.FixedFrameSize() + off - 8, obj.TYPE_REG, ppc64.REGRT1, 0);
        p.Reg = ppc64.REGSP;
        p = pp.Append(p, obj.ADUFFZERO, obj.TYPE_NONE, 0, 0, obj.TYPE_MEM, 0, 0);
        p.To.Name = obj.NAME_EXTERN;
        p.To.Sym = ir.Syms.Duffzero;
        p.To.Offset = 4 * (128 - cnt / int64(types.PtrSize));
    }
    else
 {
        p = pp.Append(p, ppc64.AMOVD, obj.TYPE_CONST, 0, @base.Ctxt.FixedFrameSize() + off - 8, obj.TYPE_REG, ppc64.REGTMP, 0);
        p = pp.Append(p, ppc64.AADD, obj.TYPE_REG, ppc64.REGTMP, 0, obj.TYPE_REG, ppc64.REGRT1, 0);
        p.Reg = ppc64.REGSP;
        p = pp.Append(p, ppc64.AMOVD, obj.TYPE_CONST, 0, cnt, obj.TYPE_REG, ppc64.REGTMP, 0);
        p = pp.Append(p, ppc64.AADD, obj.TYPE_REG, ppc64.REGTMP, 0, obj.TYPE_REG, ppc64.REGRT2, 0);
        p.Reg = ppc64.REGRT1;
        p = pp.Append(p, ppc64.AMOVDU, obj.TYPE_REG, ppc64.REGZERO, 0, obj.TYPE_MEM, ppc64.REGRT1, int64(types.PtrSize));
        var p1 = p;
        p = pp.Append(p, ppc64.ACMP, obj.TYPE_REG, ppc64.REGRT1, 0, obj.TYPE_REG, ppc64.REGRT2, 0);
        p = pp.Append(p, ppc64.ABNE, obj.TYPE_NONE, 0, 0, obj.TYPE_BRANCH, 0, 0);
        p.To.SetTarget(p1);
    }
    return _addr_p!;
}

private static ptr<obj.Prog> ginsnop(ptr<objw.Progs> _addr_pp) {
    ref objw.Progs pp = ref _addr_pp.val;

    var p = pp.Prog(ppc64.AOR);
    p.From.Type = obj.TYPE_REG;
    p.From.Reg = ppc64.REG_R0;
    p.To.Type = obj.TYPE_REG;
    p.To.Reg = ppc64.REG_R0;
    return _addr_p!;
}

private static ptr<obj.Prog> ginsnopdefer(ptr<objw.Progs> _addr_pp) {
    ref objw.Progs pp = ref _addr_pp.val;
 
    // On PPC64 two nops are required in the defer case.
    //
    // (see gc/cgen.go, gc/plive.go -- copy of comment below)
    //
    // On ppc64, when compiling Go into position
    // independent code on ppc64le we insert an
    // instruction to reload the TOC pointer from the
    // stack as well. See the long comment near
    // jmpdefer in runtime/asm_ppc64.s for why.
    // If the MOVD is not needed, insert a hardware NOP
    // so that the same number of instructions are used
    // on ppc64 in both shared and non-shared modes.

    ginsnop(_addr_pp);
    if (@base.Ctxt.Flag_shared) {
        var p = pp.Prog(ppc64.AMOVD);
        p.From.Type = obj.TYPE_MEM;
        p.From.Offset = 24;
        p.From.Reg = ppc64.REGSP;
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = ppc64.REG_R2;
        return _addr_p!;
    }
    return _addr_ginsnop(_addr_pp)!;
}

} // end ppc64_package
