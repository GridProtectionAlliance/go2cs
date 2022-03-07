// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package amd64 -- go2cs converted at 2022 March 06 23:14:57 UTC
// import "cmd/compile/internal/amd64" ==> using amd64 = go.cmd.compile.@internal.amd64_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\amd64\ssa.go
using fmt = go.fmt_package;
using buildcfg = go.@internal.buildcfg_package;
using math = go.math_package;

using @base = go.cmd.compile.@internal.@base_package;
using ir = go.cmd.compile.@internal.ir_package;
using logopt = go.cmd.compile.@internal.logopt_package;
using objw = go.cmd.compile.@internal.objw_package;
using ssa = go.cmd.compile.@internal.ssa_package;
using ssagen = go.cmd.compile.@internal.ssagen_package;
using types = go.cmd.compile.@internal.types_package;
using obj = go.cmd.@internal.obj_package;
using x86 = go.cmd.@internal.obj.x86_package;

namespace go.cmd.compile.@internal;

public static partial class amd64_package {

    // markMoves marks any MOVXconst ops that need to avoid clobbering flags.
private static void ssaMarkMoves(ptr<ssagen.State> _addr_s, ptr<ssa.Block> _addr_b) {
    ref ssagen.State s = ref _addr_s.val;
    ref ssa.Block b = ref _addr_b.val;

    var flive = b.FlagsLiveAtEnd;
    foreach (var (_, c) in b.ControlValues()) {
        flive = c.Type.IsFlags() || flive;
    }    for (var i = len(b.Values) - 1; i >= 0; i--) {
        var v = b.Values[i];
        if (flive && (v.Op == ssa.OpAMD64MOVLconst || v.Op == ssa.OpAMD64MOVQconst)) { 
            // The "mark" is any non-nil Aux value.
            v.Aux = v;

        }
        if (v.Type.IsFlags()) {
            flive = false;
        }
        foreach (var (_, a) in v.Args) {
            if (a.Type.IsFlags()) {
                flive = true;
            }
        }
    }

}

// loadByType returns the load instruction of the given type.
private static obj.As loadByType(ptr<types.Type> _addr_t) {
    ref types.Type t = ref _addr_t.val;
 
    // Avoid partial register write
    if (!t.IsFloat()) {
        switch (t.Size()) {
            case 1: 
                return x86.AMOVBLZX;
                break;
            case 2: 
                return x86.AMOVWLZX;
                break;
        }

    }
    return storeByType(_addr_t);

}

// storeByType returns the store instruction of the given type.
private static obj.As storeByType(ptr<types.Type> _addr_t) => func((_, panic, _) => {
    ref types.Type t = ref _addr_t.val;

    var width = t.Size();
    if (t.IsFloat()) {
        switch (width) {
            case 4: 
                return x86.AMOVSS;
                break;
            case 8: 
                return x86.AMOVSD;
                break;
        }

    }
    else
 {
        switch (width) {
            case 1: 
                return x86.AMOVB;
                break;
            case 2: 
                return x86.AMOVW;
                break;
            case 4: 
                return x86.AMOVL;
                break;
            case 8: 
                return x86.AMOVQ;
                break;
        }

    }
    panic(fmt.Sprintf("bad store type %v", t));

});

// moveByType returns the reg->reg move instruction of the given type.
private static obj.As moveByType(ptr<types.Type> _addr_t) => func((_, panic, _) => {
    ref types.Type t = ref _addr_t.val;

    if (t.IsFloat()) { 
        // Moving the whole sse2 register is faster
        // than moving just the correct low portion of it.
        // There is no xmm->xmm move with 1 byte opcode,
        // so use movups, which has 2 byte opcode.
        return x86.AMOVUPS;

    }
    else
 {
        switch (t.Size()) {
            case 1: 
                // Avoids partial register write
                return x86.AMOVL;
                break;
            case 2: 
                return x86.AMOVL;
                break;
            case 4: 
                return x86.AMOVL;
                break;
            case 8: 
                return x86.AMOVQ;
                break;
            case 16: 
                return x86.AMOVUPS; // int128s are in SSE registers
                break;
            default: 
                panic(fmt.Sprintf("bad int register width %d:%v", t.Size(), t));
                break;
        }

    }
});

// opregreg emits instructions for
//     dest := dest(To) op src(From)
// and also returns the created obj.Prog so it
// may be further adjusted (offset, scale, etc).
private static ptr<obj.Prog> opregreg(ptr<ssagen.State> _addr_s, obj.As op, short dest, short src) {
    ref ssagen.State s = ref _addr_s.val;

    var p = s.Prog(op);
    p.From.Type = obj.TYPE_REG;
    p.To.Type = obj.TYPE_REG;
    p.To.Reg = dest;
    p.From.Reg = src;
    return _addr_p!;
}

// memIdx fills out a as an indexed memory reference for v.
// It assumes that the base register and the index register
// are v.Args[0].Reg() and v.Args[1].Reg(), respectively.
// The caller must still use gc.AddAux/gc.AddAux2 to handle v.Aux as necessary.
private static void memIdx(ptr<obj.Addr> _addr_a, ptr<ssa.Value> _addr_v) {
    ref obj.Addr a = ref _addr_a.val;
    ref ssa.Value v = ref _addr_v.val;

    var r = v.Args[0].Reg();
    var i = v.Args[1].Reg();
    a.Type = obj.TYPE_MEM;
    a.Scale = v.Op.Scale();
    if (a.Scale == 1 && i == x86.REG_SP) {
        (r, i) = (i, r);
    }
    a.Reg = r;
    a.Index = i;

}

// DUFFZERO consists of repeated blocks of 4 MOVUPSs + LEAQ,
// See runtime/mkduff.go.
private static long duffStart(long size) {
    var (x, _) = duff(size);
    return x;
}
private static long duffAdj(long size) {
    var (_, x) = duff(size);
    return x;
}

// duff returns the offset (from duffzero, in bytes) and pointer adjust (in bytes)
// required to use the duffzero mechanism for a block of the given size.
private static (long, long) duff(long size) => func((_, panic, _) => {
    long _p0 = default;
    long _p0 = default;

    if (size < 32 || size > 1024 || size % dzClearStep != 0) {
        panic("bad duffzero size");
    }
    var steps = size / dzClearStep;
    var blocks = steps / dzBlockLen;
    steps %= dzBlockLen;
    var off = dzBlockSize * (dzBlocks - blocks);
    long adj = default;
    if (steps != 0) {
        off -= dzLeaqSize;
        off -= dzMovSize * steps;
        adj -= dzClearStep * (dzBlockLen - steps);
    }
    return (off, adj);

});

private static void getgFromTLS(ptr<ssagen.State> _addr_s, short r) {
    ref ssagen.State s = ref _addr_s.val;
 
    // See the comments in cmd/internal/obj/x86/obj6.go
    // near CanUse1InsnTLS for a detailed explanation of these instructions.
    if (x86.CanUse1InsnTLS(@base.Ctxt)) { 
        // MOVQ (TLS), r
        var p = s.Prog(x86.AMOVQ);
        p.From.Type = obj.TYPE_MEM;
        p.From.Reg = x86.REG_TLS;
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = r;

    }
    else
 { 
        // MOVQ TLS, r
        // MOVQ (r)(TLS*1), r
        p = s.Prog(x86.AMOVQ);
        p.From.Type = obj.TYPE_REG;
        p.From.Reg = x86.REG_TLS;
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = r;
        var q = s.Prog(x86.AMOVQ);
        q.From.Type = obj.TYPE_MEM;
        q.From.Reg = r;
        q.From.Index = x86.REG_TLS;
        q.From.Scale = 1;
        q.To.Type = obj.TYPE_REG;
        q.To.Reg = r;

    }
}

private static void ssaGenValue(ptr<ssagen.State> _addr_s, ptr<ssa.Value> _addr_v) {
    ref ssagen.State s = ref _addr_s.val;
    ref ssa.Value v = ref _addr_v.val;


    if (v.Op == ssa.OpAMD64VFMADD231SD)
    {
        var p = s.Prog(v.Op.Asm());
        p.From = new obj.Addr(Type:obj.TYPE_REG,Reg:v.Args[2].Reg());
        p.To = new obj.Addr(Type:obj.TYPE_REG,Reg:v.Reg());
        p.SetFrom3Reg(v.Args[1].Reg());
        goto __switch_break0;
    }
    if (v.Op == ssa.OpAMD64ADDQ || v.Op == ssa.OpAMD64ADDL)
    {
        var r = v.Reg();
        var r1 = v.Args[0].Reg();
        var r2 = v.Args[1].Reg();

        if (r == r1) 
            p = s.Prog(v.Op.Asm());
            p.From.Type = obj.TYPE_REG;
            p.From.Reg = r2;
            p.To.Type = obj.TYPE_REG;
            p.To.Reg = r;
        else if (r == r2) 
            p = s.Prog(v.Op.Asm());
            p.From.Type = obj.TYPE_REG;
            p.From.Reg = r1;
            p.To.Type = obj.TYPE_REG;
            p.To.Reg = r;
        else 
            obj.As asm = default;
            if (v.Op == ssa.OpAMD64ADDQ) {
                asm = x86.ALEAQ;
            }
            else
 {
                asm = x86.ALEAL;
            }

            p = s.Prog(asm);
            p.From.Type = obj.TYPE_MEM;
            p.From.Reg = r1;
            p.From.Scale = 1;
            p.From.Index = r2;
            p.To.Type = obj.TYPE_REG;
            p.To.Reg = r;
        // 2-address opcode arithmetic
        goto __switch_break0;
    }
    if (v.Op == ssa.OpAMD64SUBQ || v.Op == ssa.OpAMD64SUBL || v.Op == ssa.OpAMD64MULQ || v.Op == ssa.OpAMD64MULL || v.Op == ssa.OpAMD64ANDQ || v.Op == ssa.OpAMD64ANDL || v.Op == ssa.OpAMD64ORQ || v.Op == ssa.OpAMD64ORL || v.Op == ssa.OpAMD64XORQ || v.Op == ssa.OpAMD64XORL || v.Op == ssa.OpAMD64SHLQ || v.Op == ssa.OpAMD64SHLL || v.Op == ssa.OpAMD64SHRQ || v.Op == ssa.OpAMD64SHRL || v.Op == ssa.OpAMD64SHRW || v.Op == ssa.OpAMD64SHRB || v.Op == ssa.OpAMD64SARQ || v.Op == ssa.OpAMD64SARL || v.Op == ssa.OpAMD64SARW || v.Op == ssa.OpAMD64SARB || v.Op == ssa.OpAMD64ROLQ || v.Op == ssa.OpAMD64ROLL || v.Op == ssa.OpAMD64ROLW || v.Op == ssa.OpAMD64ROLB || v.Op == ssa.OpAMD64RORQ || v.Op == ssa.OpAMD64RORL || v.Op == ssa.OpAMD64RORW || v.Op == ssa.OpAMD64RORB || v.Op == ssa.OpAMD64ADDSS || v.Op == ssa.OpAMD64ADDSD || v.Op == ssa.OpAMD64SUBSS || v.Op == ssa.OpAMD64SUBSD || v.Op == ssa.OpAMD64MULSS || v.Op == ssa.OpAMD64MULSD || v.Op == ssa.OpAMD64DIVSS || v.Op == ssa.OpAMD64DIVSD || v.Op == ssa.OpAMD64PXOR || v.Op == ssa.OpAMD64BTSL || v.Op == ssa.OpAMD64BTSQ || v.Op == ssa.OpAMD64BTCL || v.Op == ssa.OpAMD64BTCQ || v.Op == ssa.OpAMD64BTRL || v.Op == ssa.OpAMD64BTRQ)
    {
        opregreg(_addr_s, v.Op.Asm(), v.Reg(), v.Args[1].Reg());
        goto __switch_break0;
    }
    if (v.Op == ssa.OpAMD64SHRDQ || v.Op == ssa.OpAMD64SHLDQ)
    {
        p = s.Prog(v.Op.Asm());
        var lo = v.Args[0].Reg();
        var hi = v.Args[1].Reg();
        var bits = v.Args[2].Reg();
        p.From.Type = obj.TYPE_REG;
        p.From.Reg = bits;
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = lo;
        p.SetFrom3Reg(hi);
        goto __switch_break0;
    }
    if (v.Op == ssa.OpAMD64DIVQU || v.Op == ssa.OpAMD64DIVLU || v.Op == ssa.OpAMD64DIVWU) 
    {
        // Arg[0] (the dividend) is in AX.
        // Arg[1] (the divisor) can be in any other register.
        // Result[0] (the quotient) is in AX.
        // Result[1] (the remainder) is in DX.
        r = v.Args[1].Reg(); 

        // Zero extend dividend.
        var c = s.Prog(x86.AXORL);
        c.From.Type = obj.TYPE_REG;
        c.From.Reg = x86.REG_DX;
        c.To.Type = obj.TYPE_REG;
        c.To.Reg = x86.REG_DX; 

        // Issue divide.
        p = s.Prog(v.Op.Asm());
        p.From.Type = obj.TYPE_REG;
        p.From.Reg = r;
        goto __switch_break0;
    }
    if (v.Op == ssa.OpAMD64DIVQ || v.Op == ssa.OpAMD64DIVL || v.Op == ssa.OpAMD64DIVW) 
    {
        // Arg[0] (the dividend) is in AX.
        // Arg[1] (the divisor) can be in any other register.
        // Result[0] (the quotient) is in AX.
        // Result[1] (the remainder) is in DX.
        r = v.Args[1].Reg();
        ptr<obj.Prog> j1; 

        // CPU faults upon signed overflow, which occurs when the most
        // negative int is divided by -1. Handle divide by -1 as a special case.
        if (ssa.DivisionNeedsFixUp(v)) {
            c = ;

            if (v.Op == ssa.OpAMD64DIVQ) 
                c = s.Prog(x86.ACMPQ);
            else if (v.Op == ssa.OpAMD64DIVL) 
                c = s.Prog(x86.ACMPL);
            else if (v.Op == ssa.OpAMD64DIVW) 
                c = s.Prog(x86.ACMPW);
                        c.From.Type = obj.TYPE_REG;
            c.From.Reg = r;
            c.To.Type = obj.TYPE_CONST;
            c.To.Offset = -1;
            j1 = s.Prog(x86.AJEQ);
            j1.To.Type = obj.TYPE_BRANCH;

        }

        if (v.Op == ssa.OpAMD64DIVQ) 
            s.Prog(x86.ACQO);
        else if (v.Op == ssa.OpAMD64DIVL) 
            s.Prog(x86.ACDQ);
        else if (v.Op == ssa.OpAMD64DIVW) 
            s.Prog(x86.ACWD);
        // Issue divide.
        p = s.Prog(v.Op.Asm());
        p.From.Type = obj.TYPE_REG;
        p.From.Reg = r;

        if (j1 != null) { 
            // Skip over -1 fixup code.
            var j2 = s.Prog(obj.AJMP);
            j2.To.Type = obj.TYPE_BRANCH; 

            // Issue -1 fixup code.
            // n / -1 = -n
            ptr<obj.Prog> n1;

            if (v.Op == ssa.OpAMD64DIVQ) 
                n1 = s.Prog(x86.ANEGQ);
            else if (v.Op == ssa.OpAMD64DIVL) 
                n1 = s.Prog(x86.ANEGL);
            else if (v.Op == ssa.OpAMD64DIVW) 
                n1 = s.Prog(x86.ANEGW);
                        n1.To.Type = obj.TYPE_REG;
            n1.To.Reg = x86.REG_AX; 

            // n % -1 == 0
            var n2 = s.Prog(x86.AXORL);
            n2.From.Type = obj.TYPE_REG;
            n2.From.Reg = x86.REG_DX;
            n2.To.Type = obj.TYPE_REG;
            n2.To.Reg = x86.REG_DX; 

            // TODO(khr): issue only the -1 fixup code we need.
            // For instance, if only the quotient is used, no point in zeroing the remainder.

            j1.To.SetTarget(n1);
            j2.To.SetTarget(s.Pc());

        }
        goto __switch_break0;
    }
    if (v.Op == ssa.OpAMD64HMULQ || v.Op == ssa.OpAMD64HMULL || v.Op == ssa.OpAMD64HMULQU || v.Op == ssa.OpAMD64HMULLU) 
    {
        // the frontend rewrites constant division by 8/16/32 bit integers into
        // HMUL by a constant
        // SSA rewrites generate the 64 bit versions

        // Arg[0] is already in AX as it's the only register we allow
        // and DX is the only output we care about (the high bits)
        p = s.Prog(v.Op.Asm());
        p.From.Type = obj.TYPE_REG;
        p.From.Reg = v.Args[1].Reg(); 

        // IMULB puts the high portion in AH instead of DL,
        // so move it to DL for consistency
        if (v.Type.Size() == 1) {
            var m = s.Prog(x86.AMOVB);
            m.From.Type = obj.TYPE_REG;
            m.From.Reg = x86.REG_AH;
            m.To.Type = obj.TYPE_REG;
            m.To.Reg = x86.REG_DX;
        }
        goto __switch_break0;
    }
    if (v.Op == ssa.OpAMD64MULQU || v.Op == ssa.OpAMD64MULLU) 
    {
        // Arg[0] is already in AX as it's the only register we allow
        // results lo in AX
        p = s.Prog(v.Op.Asm());
        p.From.Type = obj.TYPE_REG;
        p.From.Reg = v.Args[1].Reg();
        goto __switch_break0;
    }
    if (v.Op == ssa.OpAMD64MULQU2) 
    {
        // Arg[0] is already in AX as it's the only register we allow
        // results hi in DX, lo in AX
        p = s.Prog(v.Op.Asm());
        p.From.Type = obj.TYPE_REG;
        p.From.Reg = v.Args[1].Reg();
        goto __switch_break0;
    }
    if (v.Op == ssa.OpAMD64DIVQU2) 
    {
        // Arg[0], Arg[1] are already in Dx, AX, as they're the only registers we allow
        // results q in AX, r in DX
        p = s.Prog(v.Op.Asm());
        p.From.Type = obj.TYPE_REG;
        p.From.Reg = v.Args[2].Reg();
        goto __switch_break0;
    }
    if (v.Op == ssa.OpAMD64AVGQU) 
    {
        // compute (x+y)/2 unsigned.
        // Do a 64-bit add, the overflow goes into the carry.
        // Shift right once and pull the carry back into the 63rd bit.
        p = s.Prog(x86.AADDQ);
        p.From.Type = obj.TYPE_REG;
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = v.Reg();
        p.From.Reg = v.Args[1].Reg();
        p = s.Prog(x86.ARCRQ);
        p.From.Type = obj.TYPE_CONST;
        p.From.Offset = 1;
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = v.Reg();
        goto __switch_break0;
    }
    if (v.Op == ssa.OpAMD64ADDQcarry || v.Op == ssa.OpAMD64ADCQ)
    {
        r = v.Reg0();
        var r0 = v.Args[0].Reg();
        r1 = v.Args[1].Reg();

        if (r == r0) 
            p = s.Prog(v.Op.Asm());
            p.From.Type = obj.TYPE_REG;
            p.From.Reg = r1;
            p.To.Type = obj.TYPE_REG;
            p.To.Reg = r;
        else if (r == r1) 
            p = s.Prog(v.Op.Asm());
            p.From.Type = obj.TYPE_REG;
            p.From.Reg = r0;
            p.To.Type = obj.TYPE_REG;
            p.To.Reg = r;
        else 
            v.Fatalf("output not in same register as an input %s", v.LongString());
                goto __switch_break0;
    }
    if (v.Op == ssa.OpAMD64SUBQborrow || v.Op == ssa.OpAMD64SBBQ)
    {
        p = s.Prog(v.Op.Asm());
        p.From.Type = obj.TYPE_REG;
        p.From.Reg = v.Args[1].Reg();
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = v.Reg0();
        goto __switch_break0;
    }
    if (v.Op == ssa.OpAMD64ADDQconstcarry || v.Op == ssa.OpAMD64ADCQconst || v.Op == ssa.OpAMD64SUBQconstborrow || v.Op == ssa.OpAMD64SBBQconst)
    {
        p = s.Prog(v.Op.Asm());
        p.From.Type = obj.TYPE_CONST;
        p.From.Offset = v.AuxInt;
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = v.Reg0();
        goto __switch_break0;
    }
    if (v.Op == ssa.OpAMD64ADDQconst || v.Op == ssa.OpAMD64ADDLconst)
    {
        r = v.Reg();
        var a = v.Args[0].Reg();
        if (r == a) {
            switch (v.AuxInt) {
                case 1: 
                                   asm = default; 
                                   // Software optimization manual recommends add $1,reg.
                                   // But inc/dec is 1 byte smaller. ICC always uses inc
                                   // Clang/GCC choose depending on flags, but prefer add.
                                   // Experiments show that inc/dec is both a little faster
                                   // and make a binary a little smaller.
                                   if (v.Op == ssa.OpAMD64ADDQconst) {
                                       asm = x86.AINCQ;
                                   }
                                   else
                    {
                                       asm = x86.AINCL;
                                   }

                                   p = s.Prog(asm);
                                   p.To.Type = obj.TYPE_REG;
                                   p.To.Reg = r;
                                   return ;

                    break;
                case -1: 
                                   asm = default;
                                   if (v.Op == ssa.OpAMD64ADDQconst) {
                                       asm = x86.ADECQ;
                                   }
                                   else
                    {
                                       asm = x86.ADECL;
                                   }

                                   p = s.Prog(asm);
                                   p.To.Type = obj.TYPE_REG;
                                   p.To.Reg = r;
                                   return ;

                    break;
                case 0x80: 
                    // 'SUBQ $-0x80, r' is shorter to encode than
                    // and functionally equivalent to 'ADDQ $0x80, r'.
                    asm = x86.ASUBL;
                    if (v.Op == ssa.OpAMD64ADDQconst) {
                        asm = x86.ASUBQ;
                    }
                    p = s.Prog(asm);
                    p.From.Type = obj.TYPE_CONST;
                    p.From.Offset = -0x80;
                    p.To.Type = obj.TYPE_REG;
                    p.To.Reg = r;
                    return ;
                    break;
            }
            p = s.Prog(v.Op.Asm());
            p.From.Type = obj.TYPE_CONST;
            p.From.Offset = v.AuxInt;
            p.To.Type = obj.TYPE_REG;
            p.To.Reg = r;
            return ;

        }
        asm = default;
        if (v.Op == ssa.OpAMD64ADDQconst) {
            asm = x86.ALEAQ;
        }
        else
 {
            asm = x86.ALEAL;
        }
        p = s.Prog(asm);
        p.From.Type = obj.TYPE_MEM;
        p.From.Reg = a;
        p.From.Offset = v.AuxInt;
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = r;
        goto __switch_break0;
    }
    if (v.Op == ssa.OpAMD64CMOVQEQ || v.Op == ssa.OpAMD64CMOVLEQ || v.Op == ssa.OpAMD64CMOVWEQ || v.Op == ssa.OpAMD64CMOVQLT || v.Op == ssa.OpAMD64CMOVLLT || v.Op == ssa.OpAMD64CMOVWLT || v.Op == ssa.OpAMD64CMOVQNE || v.Op == ssa.OpAMD64CMOVLNE || v.Op == ssa.OpAMD64CMOVWNE || v.Op == ssa.OpAMD64CMOVQGT || v.Op == ssa.OpAMD64CMOVLGT || v.Op == ssa.OpAMD64CMOVWGT || v.Op == ssa.OpAMD64CMOVQLE || v.Op == ssa.OpAMD64CMOVLLE || v.Op == ssa.OpAMD64CMOVWLE || v.Op == ssa.OpAMD64CMOVQGE || v.Op == ssa.OpAMD64CMOVLGE || v.Op == ssa.OpAMD64CMOVWGE || v.Op == ssa.OpAMD64CMOVQHI || v.Op == ssa.OpAMD64CMOVLHI || v.Op == ssa.OpAMD64CMOVWHI || v.Op == ssa.OpAMD64CMOVQLS || v.Op == ssa.OpAMD64CMOVLLS || v.Op == ssa.OpAMD64CMOVWLS || v.Op == ssa.OpAMD64CMOVQCC || v.Op == ssa.OpAMD64CMOVLCC || v.Op == ssa.OpAMD64CMOVWCC || v.Op == ssa.OpAMD64CMOVQCS || v.Op == ssa.OpAMD64CMOVLCS || v.Op == ssa.OpAMD64CMOVWCS || v.Op == ssa.OpAMD64CMOVQGTF || v.Op == ssa.OpAMD64CMOVLGTF || v.Op == ssa.OpAMD64CMOVWGTF || v.Op == ssa.OpAMD64CMOVQGEF || v.Op == ssa.OpAMD64CMOVLGEF || v.Op == ssa.OpAMD64CMOVWGEF)
    {
        p = s.Prog(v.Op.Asm());
        p.From.Type = obj.TYPE_REG;
        p.From.Reg = v.Args[1].Reg();
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = v.Reg();
        goto __switch_break0;
    }
    if (v.Op == ssa.OpAMD64CMOVQNEF || v.Op == ssa.OpAMD64CMOVLNEF || v.Op == ssa.OpAMD64CMOVWNEF) 
    {
        // Flag condition: ^ZERO || PARITY
        // Generate:
        //   CMOV*NE  SRC,DST
        //   CMOV*PS  SRC,DST
        p = s.Prog(v.Op.Asm());
        p.From.Type = obj.TYPE_REG;
        p.From.Reg = v.Args[1].Reg();
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = v.Reg();
        ptr<obj.Prog> q;
        if (v.Op == ssa.OpAMD64CMOVQNEF) {
            q = s.Prog(x86.ACMOVQPS);
        }
        else if (v.Op == ssa.OpAMD64CMOVLNEF) {
            q = s.Prog(x86.ACMOVLPS);
        }
        else
 {
            q = s.Prog(x86.ACMOVWPS);
        }
        q.From.Type = obj.TYPE_REG;
        q.From.Reg = v.Args[1].Reg();
        q.To.Type = obj.TYPE_REG;
        q.To.Reg = v.Reg();
        goto __switch_break0;
    }
    if (v.Op == ssa.OpAMD64CMOVQEQF || v.Op == ssa.OpAMD64CMOVLEQF || v.Op == ssa.OpAMD64CMOVWEQF) 
    {
        // Flag condition: ZERO && !PARITY
        // Generate:
        //   MOV      SRC,AX
        //   CMOV*NE  DST,AX
        //   CMOV*PC  AX,DST
        //
        // TODO(rasky): we could generate:
        //   CMOV*NE  DST,SRC
        //   CMOV*PC  SRC,DST
        // But this requires a way for regalloc to know that SRC might be
        // clobbered by this instruction.
        if (v.Args[1].Reg() != x86.REG_AX) {
            opregreg(_addr_s, moveByType(_addr_v.Type), x86.REG_AX, v.Args[1].Reg());
        }
        p = s.Prog(v.Op.Asm());
        p.From.Type = obj.TYPE_REG;
        p.From.Reg = v.Reg();
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = x86.REG_AX;
        q = ;
        if (v.Op == ssa.OpAMD64CMOVQEQF) {
            q = s.Prog(x86.ACMOVQPC);
        }
        else if (v.Op == ssa.OpAMD64CMOVLEQF) {
            q = s.Prog(x86.ACMOVLPC);
        }
        else
 {
            q = s.Prog(x86.ACMOVWPC);
        }
        q.From.Type = obj.TYPE_REG;
        q.From.Reg = x86.REG_AX;
        q.To.Type = obj.TYPE_REG;
        q.To.Reg = v.Reg();
        goto __switch_break0;
    }
    if (v.Op == ssa.OpAMD64MULQconst || v.Op == ssa.OpAMD64MULLconst)
    {
        r = v.Reg();
        p = s.Prog(v.Op.Asm());
        p.From.Type = obj.TYPE_CONST;
        p.From.Offset = v.AuxInt;
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = r;
        p.SetFrom3Reg(v.Args[0].Reg());
        goto __switch_break0;
    }
    if (v.Op == ssa.OpAMD64SUBQconst || v.Op == ssa.OpAMD64SUBLconst || v.Op == ssa.OpAMD64ANDQconst || v.Op == ssa.OpAMD64ANDLconst || v.Op == ssa.OpAMD64ORQconst || v.Op == ssa.OpAMD64ORLconst || v.Op == ssa.OpAMD64XORQconst || v.Op == ssa.OpAMD64XORLconst || v.Op == ssa.OpAMD64SHLQconst || v.Op == ssa.OpAMD64SHLLconst || v.Op == ssa.OpAMD64SHRQconst || v.Op == ssa.OpAMD64SHRLconst || v.Op == ssa.OpAMD64SHRWconst || v.Op == ssa.OpAMD64SHRBconst || v.Op == ssa.OpAMD64SARQconst || v.Op == ssa.OpAMD64SARLconst || v.Op == ssa.OpAMD64SARWconst || v.Op == ssa.OpAMD64SARBconst || v.Op == ssa.OpAMD64ROLQconst || v.Op == ssa.OpAMD64ROLLconst || v.Op == ssa.OpAMD64ROLWconst || v.Op == ssa.OpAMD64ROLBconst)
    {
        p = s.Prog(v.Op.Asm());
        p.From.Type = obj.TYPE_CONST;
        p.From.Offset = v.AuxInt;
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = v.Reg();
        goto __switch_break0;
    }
    if (v.Op == ssa.OpAMD64SBBQcarrymask || v.Op == ssa.OpAMD64SBBLcarrymask)
    {
        r = v.Reg();
        p = s.Prog(v.Op.Asm());
        p.From.Type = obj.TYPE_REG;
        p.From.Reg = r;
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = r;
        goto __switch_break0;
    }
    if (v.Op == ssa.OpAMD64LEAQ1 || v.Op == ssa.OpAMD64LEAQ2 || v.Op == ssa.OpAMD64LEAQ4 || v.Op == ssa.OpAMD64LEAQ8 || v.Op == ssa.OpAMD64LEAL1 || v.Op == ssa.OpAMD64LEAL2 || v.Op == ssa.OpAMD64LEAL4 || v.Op == ssa.OpAMD64LEAL8 || v.Op == ssa.OpAMD64LEAW1 || v.Op == ssa.OpAMD64LEAW2 || v.Op == ssa.OpAMD64LEAW4 || v.Op == ssa.OpAMD64LEAW8)
    {
        p = s.Prog(v.Op.Asm());
        memIdx(_addr_p.From, _addr_v);
        var o = v.Reg();
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = o;
        if (v.AuxInt != 0 && v.Aux == null) { 
            // Emit an additional LEA to add the displacement instead of creating a slow 3 operand LEA.

            if (v.Op == ssa.OpAMD64LEAQ1 || v.Op == ssa.OpAMD64LEAQ2 || v.Op == ssa.OpAMD64LEAQ4 || v.Op == ssa.OpAMD64LEAQ8) 
                p = s.Prog(x86.ALEAQ);
            else if (v.Op == ssa.OpAMD64LEAL1 || v.Op == ssa.OpAMD64LEAL2 || v.Op == ssa.OpAMD64LEAL4 || v.Op == ssa.OpAMD64LEAL8) 
                p = s.Prog(x86.ALEAL);
            else if (v.Op == ssa.OpAMD64LEAW1 || v.Op == ssa.OpAMD64LEAW2 || v.Op == ssa.OpAMD64LEAW4 || v.Op == ssa.OpAMD64LEAW8) 
                p = s.Prog(x86.ALEAW);
                        p.From.Type = obj.TYPE_MEM;
            p.From.Reg = o;
            p.To.Type = obj.TYPE_REG;
            p.To.Reg = o;

        }
        ssagen.AddAux(_addr_p.From, v);
        goto __switch_break0;
    }
    if (v.Op == ssa.OpAMD64LEAQ || v.Op == ssa.OpAMD64LEAL || v.Op == ssa.OpAMD64LEAW)
    {
        p = s.Prog(v.Op.Asm());
        p.From.Type = obj.TYPE_MEM;
        p.From.Reg = v.Args[0].Reg();
        ssagen.AddAux(_addr_p.From, v);
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = v.Reg();
        goto __switch_break0;
    }
    if (v.Op == ssa.OpAMD64CMPQ || v.Op == ssa.OpAMD64CMPL || v.Op == ssa.OpAMD64CMPW || v.Op == ssa.OpAMD64CMPB || v.Op == ssa.OpAMD64TESTQ || v.Op == ssa.OpAMD64TESTL || v.Op == ssa.OpAMD64TESTW || v.Op == ssa.OpAMD64TESTB || v.Op == ssa.OpAMD64BTL || v.Op == ssa.OpAMD64BTQ)
    {
        opregreg(_addr_s, v.Op.Asm(), v.Args[1].Reg(), v.Args[0].Reg());
        goto __switch_break0;
    }
    if (v.Op == ssa.OpAMD64UCOMISS || v.Op == ssa.OpAMD64UCOMISD) 
    {
        // Go assembler has swapped operands for UCOMISx relative to CMP,
        // must account for that right here.
        opregreg(_addr_s, v.Op.Asm(), v.Args[0].Reg(), v.Args[1].Reg());
        goto __switch_break0;
    }
    if (v.Op == ssa.OpAMD64CMPQconst || v.Op == ssa.OpAMD64CMPLconst || v.Op == ssa.OpAMD64CMPWconst || v.Op == ssa.OpAMD64CMPBconst)
    {
        p = s.Prog(v.Op.Asm());
        p.From.Type = obj.TYPE_REG;
        p.From.Reg = v.Args[0].Reg();
        p.To.Type = obj.TYPE_CONST;
        p.To.Offset = v.AuxInt;
        goto __switch_break0;
    }
    if (v.Op == ssa.OpAMD64BTLconst || v.Op == ssa.OpAMD64BTQconst || v.Op == ssa.OpAMD64TESTQconst || v.Op == ssa.OpAMD64TESTLconst || v.Op == ssa.OpAMD64TESTWconst || v.Op == ssa.OpAMD64TESTBconst || v.Op == ssa.OpAMD64BTSLconst || v.Op == ssa.OpAMD64BTSQconst || v.Op == ssa.OpAMD64BTCLconst || v.Op == ssa.OpAMD64BTCQconst || v.Op == ssa.OpAMD64BTRLconst || v.Op == ssa.OpAMD64BTRQconst)
    {
        var op = v.Op;
        if (op == ssa.OpAMD64BTQconst && v.AuxInt < 32) { 
            // Emit 32-bit version because it's shorter
            op = ssa.OpAMD64BTLconst;

        }
        p = s.Prog(op.Asm());
        p.From.Type = obj.TYPE_CONST;
        p.From.Offset = v.AuxInt;
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = v.Args[0].Reg();
        goto __switch_break0;
    }
    if (v.Op == ssa.OpAMD64CMPQload || v.Op == ssa.OpAMD64CMPLload || v.Op == ssa.OpAMD64CMPWload || v.Op == ssa.OpAMD64CMPBload)
    {
        p = s.Prog(v.Op.Asm());
        p.From.Type = obj.TYPE_MEM;
        p.From.Reg = v.Args[0].Reg();
        ssagen.AddAux(_addr_p.From, v);
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = v.Args[1].Reg();
        goto __switch_break0;
    }
    if (v.Op == ssa.OpAMD64CMPQconstload || v.Op == ssa.OpAMD64CMPLconstload || v.Op == ssa.OpAMD64CMPWconstload || v.Op == ssa.OpAMD64CMPBconstload)
    {
        var sc = v.AuxValAndOff();
        p = s.Prog(v.Op.Asm());
        p.From.Type = obj.TYPE_MEM;
        p.From.Reg = v.Args[0].Reg();
        ssagen.AddAux2(_addr_p.From, v, sc.Off64());
        p.To.Type = obj.TYPE_CONST;
        p.To.Offset = sc.Val64();
        goto __switch_break0;
    }
    if (v.Op == ssa.OpAMD64CMPQloadidx8 || v.Op == ssa.OpAMD64CMPQloadidx1 || v.Op == ssa.OpAMD64CMPLloadidx4 || v.Op == ssa.OpAMD64CMPLloadidx1 || v.Op == ssa.OpAMD64CMPWloadidx2 || v.Op == ssa.OpAMD64CMPWloadidx1 || v.Op == ssa.OpAMD64CMPBloadidx1)
    {
        p = s.Prog(v.Op.Asm());
        memIdx(_addr_p.From, _addr_v);
        ssagen.AddAux(_addr_p.From, v);
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = v.Args[2].Reg();
        goto __switch_break0;
    }
    if (v.Op == ssa.OpAMD64CMPQconstloadidx8 || v.Op == ssa.OpAMD64CMPQconstloadidx1 || v.Op == ssa.OpAMD64CMPLconstloadidx4 || v.Op == ssa.OpAMD64CMPLconstloadidx1 || v.Op == ssa.OpAMD64CMPWconstloadidx2 || v.Op == ssa.OpAMD64CMPWconstloadidx1 || v.Op == ssa.OpAMD64CMPBconstloadidx1)
    {
        sc = v.AuxValAndOff();
        p = s.Prog(v.Op.Asm());
        memIdx(_addr_p.From, _addr_v);
        ssagen.AddAux2(_addr_p.From, v, sc.Off64());
        p.To.Type = obj.TYPE_CONST;
        p.To.Offset = sc.Val64();
        goto __switch_break0;
    }
    if (v.Op == ssa.OpAMD64MOVLconst || v.Op == ssa.OpAMD64MOVQconst)
    {
        var x = v.Reg(); 

        // If flags aren't live (indicated by v.Aux == nil),
        // then we can rewrite MOV $0, AX into XOR AX, AX.
        if (v.AuxInt == 0 && v.Aux == null) {
            p = s.Prog(x86.AXORL);
            p.From.Type = obj.TYPE_REG;
            p.From.Reg = x;
            p.To.Type = obj.TYPE_REG;
            p.To.Reg = x;
            break;
        }
        asm = v.Op.Asm(); 
        // Use MOVL to move a small constant into a register
        // when the constant is positive and fits into 32 bits.
        if (0 <= v.AuxInt && v.AuxInt <= (1 << 32 - 1)) { 
            // The upper 32bit are zeroed automatically when using MOVL.
            asm = x86.AMOVL;

        }
        p = s.Prog(asm);
        p.From.Type = obj.TYPE_CONST;
        p.From.Offset = v.AuxInt;
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = x;
        goto __switch_break0;
    }
    if (v.Op == ssa.OpAMD64MOVSSconst || v.Op == ssa.OpAMD64MOVSDconst)
    {
        x = v.Reg();
        p = s.Prog(v.Op.Asm());
        p.From.Type = obj.TYPE_FCONST;
        p.From.Val = math.Float64frombits(uint64(v.AuxInt));
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = x;
        goto __switch_break0;
    }
    if (v.Op == ssa.OpAMD64MOVQload || v.Op == ssa.OpAMD64MOVSSload || v.Op == ssa.OpAMD64MOVSDload || v.Op == ssa.OpAMD64MOVLload || v.Op == ssa.OpAMD64MOVWload || v.Op == ssa.OpAMD64MOVBload || v.Op == ssa.OpAMD64MOVBQSXload || v.Op == ssa.OpAMD64MOVWQSXload || v.Op == ssa.OpAMD64MOVLQSXload || v.Op == ssa.OpAMD64MOVOload)
    {
        p = s.Prog(v.Op.Asm());
        p.From.Type = obj.TYPE_MEM;
        p.From.Reg = v.Args[0].Reg();
        ssagen.AddAux(_addr_p.From, v);
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = v.Reg();
        goto __switch_break0;
    }
    if (v.Op == ssa.OpAMD64MOVBloadidx1 || v.Op == ssa.OpAMD64MOVWloadidx1 || v.Op == ssa.OpAMD64MOVLloadidx1 || v.Op == ssa.OpAMD64MOVQloadidx1 || v.Op == ssa.OpAMD64MOVSSloadidx1 || v.Op == ssa.OpAMD64MOVSDloadidx1 || v.Op == ssa.OpAMD64MOVQloadidx8 || v.Op == ssa.OpAMD64MOVSDloadidx8 || v.Op == ssa.OpAMD64MOVLloadidx8 || v.Op == ssa.OpAMD64MOVLloadidx4 || v.Op == ssa.OpAMD64MOVSSloadidx4 || v.Op == ssa.OpAMD64MOVWloadidx2)
    {
        p = s.Prog(v.Op.Asm());
        memIdx(_addr_p.From, _addr_v);
        ssagen.AddAux(_addr_p.From, v);
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = v.Reg();
        goto __switch_break0;
    }
    if (v.Op == ssa.OpAMD64MOVQstore || v.Op == ssa.OpAMD64MOVSSstore || v.Op == ssa.OpAMD64MOVSDstore || v.Op == ssa.OpAMD64MOVLstore || v.Op == ssa.OpAMD64MOVWstore || v.Op == ssa.OpAMD64MOVBstore || v.Op == ssa.OpAMD64MOVOstore || v.Op == ssa.OpAMD64ADDQmodify || v.Op == ssa.OpAMD64SUBQmodify || v.Op == ssa.OpAMD64ANDQmodify || v.Op == ssa.OpAMD64ORQmodify || v.Op == ssa.OpAMD64XORQmodify || v.Op == ssa.OpAMD64ADDLmodify || v.Op == ssa.OpAMD64SUBLmodify || v.Op == ssa.OpAMD64ANDLmodify || v.Op == ssa.OpAMD64ORLmodify || v.Op == ssa.OpAMD64XORLmodify)
    {
        p = s.Prog(v.Op.Asm());
        p.From.Type = obj.TYPE_REG;
        p.From.Reg = v.Args[1].Reg();
        p.To.Type = obj.TYPE_MEM;
        p.To.Reg = v.Args[0].Reg();
        ssagen.AddAux(_addr_p.To, v);
        goto __switch_break0;
    }
    if (v.Op == ssa.OpAMD64MOVBstoreidx1 || v.Op == ssa.OpAMD64MOVWstoreidx1 || v.Op == ssa.OpAMD64MOVLstoreidx1 || v.Op == ssa.OpAMD64MOVQstoreidx1 || v.Op == ssa.OpAMD64MOVSSstoreidx1 || v.Op == ssa.OpAMD64MOVSDstoreidx1 || v.Op == ssa.OpAMD64MOVQstoreidx8 || v.Op == ssa.OpAMD64MOVSDstoreidx8 || v.Op == ssa.OpAMD64MOVLstoreidx8 || v.Op == ssa.OpAMD64MOVSSstoreidx4 || v.Op == ssa.OpAMD64MOVLstoreidx4 || v.Op == ssa.OpAMD64MOVWstoreidx2 || v.Op == ssa.OpAMD64ADDLmodifyidx1 || v.Op == ssa.OpAMD64ADDLmodifyidx4 || v.Op == ssa.OpAMD64ADDLmodifyidx8 || v.Op == ssa.OpAMD64ADDQmodifyidx1 || v.Op == ssa.OpAMD64ADDQmodifyidx8 || v.Op == ssa.OpAMD64SUBLmodifyidx1 || v.Op == ssa.OpAMD64SUBLmodifyidx4 || v.Op == ssa.OpAMD64SUBLmodifyidx8 || v.Op == ssa.OpAMD64SUBQmodifyidx1 || v.Op == ssa.OpAMD64SUBQmodifyidx8 || v.Op == ssa.OpAMD64ANDLmodifyidx1 || v.Op == ssa.OpAMD64ANDLmodifyidx4 || v.Op == ssa.OpAMD64ANDLmodifyidx8 || v.Op == ssa.OpAMD64ANDQmodifyidx1 || v.Op == ssa.OpAMD64ANDQmodifyidx8 || v.Op == ssa.OpAMD64ORLmodifyidx1 || v.Op == ssa.OpAMD64ORLmodifyidx4 || v.Op == ssa.OpAMD64ORLmodifyidx8 || v.Op == ssa.OpAMD64ORQmodifyidx1 || v.Op == ssa.OpAMD64ORQmodifyidx8 || v.Op == ssa.OpAMD64XORLmodifyidx1 || v.Op == ssa.OpAMD64XORLmodifyidx4 || v.Op == ssa.OpAMD64XORLmodifyidx8 || v.Op == ssa.OpAMD64XORQmodifyidx1 || v.Op == ssa.OpAMD64XORQmodifyidx8)
    {
        p = s.Prog(v.Op.Asm());
        p.From.Type = obj.TYPE_REG;
        p.From.Reg = v.Args[2].Reg();
        memIdx(_addr_p.To, _addr_v);
        ssagen.AddAux(_addr_p.To, v);
        goto __switch_break0;
    }
    if (v.Op == ssa.OpAMD64ADDQconstmodify || v.Op == ssa.OpAMD64ADDLconstmodify)
    {
        sc = v.AuxValAndOff();
        var off = sc.Off64();
        var val = sc.Val();
        if (val == 1 || val == -1) {
            asm = default;
            if (v.Op == ssa.OpAMD64ADDQconstmodify) {
                if (val == 1) {
                    asm = x86.AINCQ;
                }
                else
 {
                    asm = x86.ADECQ;
                }

            }
            else
 {
                if (val == 1) {
                    asm = x86.AINCL;
                }
                else
 {
                    asm = x86.ADECL;
                }

            }

            p = s.Prog(asm);
            p.To.Type = obj.TYPE_MEM;
            p.To.Reg = v.Args[0].Reg();
            ssagen.AddAux2(_addr_p.To, v, off);
            break;

        }
        fallthrough = true;
    }
    if (fallthrough || v.Op == ssa.OpAMD64ANDQconstmodify || v.Op == ssa.OpAMD64ANDLconstmodify || v.Op == ssa.OpAMD64ORQconstmodify || v.Op == ssa.OpAMD64ORLconstmodify || v.Op == ssa.OpAMD64XORQconstmodify || v.Op == ssa.OpAMD64XORLconstmodify)
    {
        sc = v.AuxValAndOff();
        off = sc.Off64();
        val = sc.Val64();
        p = s.Prog(v.Op.Asm());
        p.From.Type = obj.TYPE_CONST;
        p.From.Offset = val;
        p.To.Type = obj.TYPE_MEM;
        p.To.Reg = v.Args[0].Reg();
        ssagen.AddAux2(_addr_p.To, v, off);
        goto __switch_break0;
    }
    if (v.Op == ssa.OpAMD64MOVQstoreconst || v.Op == ssa.OpAMD64MOVLstoreconst || v.Op == ssa.OpAMD64MOVWstoreconst || v.Op == ssa.OpAMD64MOVBstoreconst)
    {
        p = s.Prog(v.Op.Asm());
        p.From.Type = obj.TYPE_CONST;
        sc = v.AuxValAndOff();
        p.From.Offset = sc.Val64();
        p.To.Type = obj.TYPE_MEM;
        p.To.Reg = v.Args[0].Reg();
        ssagen.AddAux2(_addr_p.To, v, sc.Off64());
        goto __switch_break0;
    }
    if (v.Op == ssa.OpAMD64MOVOstorezero)
    {
        if (!buildcfg.Experiment.RegabiG || s.ABI != obj.ABIInternal) { 
            // zero X15 manually
            opregreg(_addr_s, x86.AXORPS, x86.REG_X15, x86.REG_X15);

        }
        p = s.Prog(v.Op.Asm());
        p.From.Type = obj.TYPE_REG;
        p.From.Reg = x86.REG_X15;
        p.To.Type = obj.TYPE_MEM;
        p.To.Reg = v.Args[0].Reg();
        ssagen.AddAux(_addr_p.To, v);
        goto __switch_break0;
    }
    if (v.Op == ssa.OpAMD64MOVQstoreconstidx1 || v.Op == ssa.OpAMD64MOVQstoreconstidx8 || v.Op == ssa.OpAMD64MOVLstoreconstidx1 || v.Op == ssa.OpAMD64MOVLstoreconstidx4 || v.Op == ssa.OpAMD64MOVWstoreconstidx1 || v.Op == ssa.OpAMD64MOVWstoreconstidx2 || v.Op == ssa.OpAMD64MOVBstoreconstidx1 || v.Op == ssa.OpAMD64ADDLconstmodifyidx1 || v.Op == ssa.OpAMD64ADDLconstmodifyidx4 || v.Op == ssa.OpAMD64ADDLconstmodifyidx8 || v.Op == ssa.OpAMD64ADDQconstmodifyidx1 || v.Op == ssa.OpAMD64ADDQconstmodifyidx8 || v.Op == ssa.OpAMD64ANDLconstmodifyidx1 || v.Op == ssa.OpAMD64ANDLconstmodifyidx4 || v.Op == ssa.OpAMD64ANDLconstmodifyidx8 || v.Op == ssa.OpAMD64ANDQconstmodifyidx1 || v.Op == ssa.OpAMD64ANDQconstmodifyidx8 || v.Op == ssa.OpAMD64ORLconstmodifyidx1 || v.Op == ssa.OpAMD64ORLconstmodifyidx4 || v.Op == ssa.OpAMD64ORLconstmodifyidx8 || v.Op == ssa.OpAMD64ORQconstmodifyidx1 || v.Op == ssa.OpAMD64ORQconstmodifyidx8 || v.Op == ssa.OpAMD64XORLconstmodifyidx1 || v.Op == ssa.OpAMD64XORLconstmodifyidx4 || v.Op == ssa.OpAMD64XORLconstmodifyidx8 || v.Op == ssa.OpAMD64XORQconstmodifyidx1 || v.Op == ssa.OpAMD64XORQconstmodifyidx8)
    {
        p = s.Prog(v.Op.Asm());
        p.From.Type = obj.TYPE_CONST;
        sc = v.AuxValAndOff();
        p.From.Offset = sc.Val64();

        if (p.As == x86.AADDQ && p.From.Offset == 1) 
            p.As = x86.AINCQ;
            p.From.Type = obj.TYPE_NONE;
        else if (p.As == x86.AADDQ && p.From.Offset == -1) 
            p.As = x86.ADECQ;
            p.From.Type = obj.TYPE_NONE;
        else if (p.As == x86.AADDL && p.From.Offset == 1) 
            p.As = x86.AINCL;
            p.From.Type = obj.TYPE_NONE;
        else if (p.As == x86.AADDL && p.From.Offset == -1) 
            p.As = x86.ADECL;
            p.From.Type = obj.TYPE_NONE;
                memIdx(_addr_p.To, _addr_v);
        ssagen.AddAux2(_addr_p.To, v, sc.Off64());
        goto __switch_break0;
    }
    if (v.Op == ssa.OpAMD64MOVLQSX || v.Op == ssa.OpAMD64MOVWQSX || v.Op == ssa.OpAMD64MOVBQSX || v.Op == ssa.OpAMD64MOVLQZX || v.Op == ssa.OpAMD64MOVWQZX || v.Op == ssa.OpAMD64MOVBQZX || v.Op == ssa.OpAMD64CVTTSS2SL || v.Op == ssa.OpAMD64CVTTSD2SL || v.Op == ssa.OpAMD64CVTTSS2SQ || v.Op == ssa.OpAMD64CVTTSD2SQ || v.Op == ssa.OpAMD64CVTSS2SD || v.Op == ssa.OpAMD64CVTSD2SS)
    {
        opregreg(_addr_s, v.Op.Asm(), v.Reg(), v.Args[0].Reg());
        goto __switch_break0;
    }
    if (v.Op == ssa.OpAMD64CVTSL2SD || v.Op == ssa.OpAMD64CVTSQ2SD || v.Op == ssa.OpAMD64CVTSQ2SS || v.Op == ssa.OpAMD64CVTSL2SS)
    {
        r = v.Reg(); 
        // Break false dependency on destination register.
        opregreg(_addr_s, x86.AXORPS, r, r);
        opregreg(_addr_s, v.Op.Asm(), r, v.Args[0].Reg());
        goto __switch_break0;
    }
    if (v.Op == ssa.OpAMD64MOVQi2f || v.Op == ssa.OpAMD64MOVQf2i || v.Op == ssa.OpAMD64MOVLi2f || v.Op == ssa.OpAMD64MOVLf2i)
    {
        p = ;

        if (v.Op == ssa.OpAMD64MOVQi2f || v.Op == ssa.OpAMD64MOVQf2i) 
            p = s.Prog(x86.AMOVQ);
        else if (v.Op == ssa.OpAMD64MOVLi2f || v.Op == ssa.OpAMD64MOVLf2i) 
            p = s.Prog(x86.AMOVL);
                p.From.Type = obj.TYPE_REG;
        p.From.Reg = v.Args[0].Reg();
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = v.Reg();
        goto __switch_break0;
    }
    if (v.Op == ssa.OpAMD64ADDQload || v.Op == ssa.OpAMD64ADDLload || v.Op == ssa.OpAMD64SUBQload || v.Op == ssa.OpAMD64SUBLload || v.Op == ssa.OpAMD64ANDQload || v.Op == ssa.OpAMD64ANDLload || v.Op == ssa.OpAMD64ORQload || v.Op == ssa.OpAMD64ORLload || v.Op == ssa.OpAMD64XORQload || v.Op == ssa.OpAMD64XORLload || v.Op == ssa.OpAMD64ADDSDload || v.Op == ssa.OpAMD64ADDSSload || v.Op == ssa.OpAMD64SUBSDload || v.Op == ssa.OpAMD64SUBSSload || v.Op == ssa.OpAMD64MULSDload || v.Op == ssa.OpAMD64MULSSload || v.Op == ssa.OpAMD64DIVSDload || v.Op == ssa.OpAMD64DIVSSload)
    {
        p = s.Prog(v.Op.Asm());
        p.From.Type = obj.TYPE_MEM;
        p.From.Reg = v.Args[1].Reg();
        ssagen.AddAux(_addr_p.From, v);
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = v.Reg();
        goto __switch_break0;
    }
    if (v.Op == ssa.OpAMD64ADDLloadidx1 || v.Op == ssa.OpAMD64ADDLloadidx4 || v.Op == ssa.OpAMD64ADDLloadidx8 || v.Op == ssa.OpAMD64ADDQloadidx1 || v.Op == ssa.OpAMD64ADDQloadidx8 || v.Op == ssa.OpAMD64SUBLloadidx1 || v.Op == ssa.OpAMD64SUBLloadidx4 || v.Op == ssa.OpAMD64SUBLloadidx8 || v.Op == ssa.OpAMD64SUBQloadidx1 || v.Op == ssa.OpAMD64SUBQloadidx8 || v.Op == ssa.OpAMD64ANDLloadidx1 || v.Op == ssa.OpAMD64ANDLloadidx4 || v.Op == ssa.OpAMD64ANDLloadidx8 || v.Op == ssa.OpAMD64ANDQloadidx1 || v.Op == ssa.OpAMD64ANDQloadidx8 || v.Op == ssa.OpAMD64ORLloadidx1 || v.Op == ssa.OpAMD64ORLloadidx4 || v.Op == ssa.OpAMD64ORLloadidx8 || v.Op == ssa.OpAMD64ORQloadidx1 || v.Op == ssa.OpAMD64ORQloadidx8 || v.Op == ssa.OpAMD64XORLloadidx1 || v.Op == ssa.OpAMD64XORLloadidx4 || v.Op == ssa.OpAMD64XORLloadidx8 || v.Op == ssa.OpAMD64XORQloadidx1 || v.Op == ssa.OpAMD64XORQloadidx8 || v.Op == ssa.OpAMD64ADDSSloadidx1 || v.Op == ssa.OpAMD64ADDSSloadidx4 || v.Op == ssa.OpAMD64ADDSDloadidx1 || v.Op == ssa.OpAMD64ADDSDloadidx8 || v.Op == ssa.OpAMD64SUBSSloadidx1 || v.Op == ssa.OpAMD64SUBSSloadidx4 || v.Op == ssa.OpAMD64SUBSDloadidx1 || v.Op == ssa.OpAMD64SUBSDloadidx8 || v.Op == ssa.OpAMD64MULSSloadidx1 || v.Op == ssa.OpAMD64MULSSloadidx4 || v.Op == ssa.OpAMD64MULSDloadidx1 || v.Op == ssa.OpAMD64MULSDloadidx8 || v.Op == ssa.OpAMD64DIVSSloadidx1 || v.Op == ssa.OpAMD64DIVSSloadidx4 || v.Op == ssa.OpAMD64DIVSDloadidx1 || v.Op == ssa.OpAMD64DIVSDloadidx8)
    {
        p = s.Prog(v.Op.Asm());

        r = v.Args[1].Reg();
        var i = v.Args[2].Reg();
        p.From.Type = obj.TYPE_MEM;
        p.From.Scale = v.Op.Scale();
        if (p.From.Scale == 1 && i == x86.REG_SP) {
            (r, i) = (i, r);
        }
        p.From.Reg = r;
        p.From.Index = i;

        ssagen.AddAux(_addr_p.From, v);
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = v.Reg();
        goto __switch_break0;
    }
    if (v.Op == ssa.OpAMD64DUFFZERO)
    {
        if (!buildcfg.Experiment.RegabiG || s.ABI != obj.ABIInternal) { 
            // zero X15 manually
            opregreg(_addr_s, x86.AXORPS, x86.REG_X15, x86.REG_X15);

        }
        off = duffStart(v.AuxInt);
        var adj = duffAdj(v.AuxInt);
        p = ;
        if (adj != 0) {
            p = s.Prog(x86.ALEAQ);
            p.From.Type = obj.TYPE_MEM;
            p.From.Offset = adj;
            p.From.Reg = x86.REG_DI;
            p.To.Type = obj.TYPE_REG;
            p.To.Reg = x86.REG_DI;
        }
        p = s.Prog(obj.ADUFFZERO);
        p.To.Type = obj.TYPE_ADDR;
        p.To.Sym = ir.Syms.Duffzero;
        p.To.Offset = off;
        goto __switch_break0;
    }
    if (v.Op == ssa.OpAMD64DUFFCOPY)
    {
        p = s.Prog(obj.ADUFFCOPY);
        p.To.Type = obj.TYPE_ADDR;
        p.To.Sym = ir.Syms.Duffcopy;
        if (v.AuxInt % 16 != 0) {
            v.Fatalf("bad DUFFCOPY AuxInt %v", v.AuxInt);
        }
        p.To.Offset = 14 * (64 - v.AuxInt / 16); 
        // 14 and 64 are magic constants.  14 is the number of bytes to encode:
        //    MOVUPS    (SI), X0
        //    ADDQ    $16, SI
        //    MOVUPS    X0, (DI)
        //    ADDQ    $16, DI
        // and 64 is the number of such blocks. See src/runtime/duff_amd64.s:duffcopy.
        goto __switch_break0;
    }
    if (v.Op == ssa.OpCopy) // TODO: use MOVQreg for reg->reg copies instead of OpCopy?
    {
        if (v.Type.IsMemory()) {
            return ;
        }
        x = v.Args[0].Reg();
        var y = v.Reg();
        if (x != y) {
            opregreg(_addr_s, moveByType(_addr_v.Type), y, x);
        }
        goto __switch_break0;
    }
    if (v.Op == ssa.OpLoadReg)
    {
        if (v.Type.IsFlags()) {
            v.Fatalf("load flags not implemented: %v", v.LongString());
            return ;
        }
        p = s.Prog(loadByType(_addr_v.Type));
        ssagen.AddrAuto(_addr_p.From, v.Args[0]);
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = v.Reg();
        goto __switch_break0;
    }
    if (v.Op == ssa.OpStoreReg)
    {
        if (v.Type.IsFlags()) {
            v.Fatalf("store flags not implemented: %v", v.LongString());
            return ;
        }
        p = s.Prog(storeByType(_addr_v.Type));
        p.From.Type = obj.TYPE_REG;
        p.From.Reg = v.Args[0].Reg();
        ssagen.AddrAuto(_addr_p.To, v);
        goto __switch_break0;
    }
    if (v.Op == ssa.OpAMD64LoweredHasCPUFeature)
    {
        p = s.Prog(x86.AMOVBQZX);
        p.From.Type = obj.TYPE_MEM;
        ssagen.AddAux(_addr_p.From, v);
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = v.Reg();
        goto __switch_break0;
    }
    if (v.Op == ssa.OpArgIntReg || v.Op == ssa.OpArgFloatReg) 
    {
        // The assembler needs to wrap the entry safepoint/stack growth code with spill/unspill
        // The loop only runs once.
        foreach (var (_, ap) in v.Block.Func.RegArgs) { 
            // Pass the spill/unspill information along to the assembler, offset by size of return PC pushed on stack.
            var addr = ssagen.SpillSlotAddr(ap, x86.REG_SP, v.Block.Func.Config.PtrSize);
            s.FuncInfo().AddSpill(new obj.RegSpill(Reg:ap.Reg,Addr:addr,Unspill:loadByType(ap.Type),Spill:storeByType(ap.Type)));

        }        v.Block.Func.RegArgs = null;
        ssagen.CheckArgReg(v);
        goto __switch_break0;
    }
    if (v.Op == ssa.OpAMD64LoweredGetClosurePtr) 
    {
        // Closure pointer is DX.
        ssagen.CheckLoweredGetClosurePtr(v);
        goto __switch_break0;
    }
    if (v.Op == ssa.OpAMD64LoweredGetG)
    {
        if (buildcfg.Experiment.RegabiG && s.ABI == obj.ABIInternal) {
            v.Fatalf("LoweredGetG should not appear in ABIInternal");
        }
        r = v.Reg();
        getgFromTLS(_addr_s, r);
        goto __switch_break0;
    }
    if (v.Op == ssa.OpAMD64CALLstatic)
    {
        if (buildcfg.Experiment.RegabiG && s.ABI == obj.ABI0 && v.Aux._<ptr<ssa.AuxCall>>().Fn.ABI() == obj.ABIInternal) { 
            // zeroing X15 when entering ABIInternal from ABI0
            opregreg(_addr_s, x86.AXORPS, x86.REG_X15, x86.REG_X15); 
            // set G register from TLS
            getgFromTLS(_addr_s, x86.REG_R14);

        }
        s.Call(v);
        if (buildcfg.Experiment.RegabiG && s.ABI == obj.ABIInternal && v.Aux._<ptr<ssa.AuxCall>>().Fn.ABI() == obj.ABI0) { 
            // zeroing X15 when entering ABIInternal from ABI0
            opregreg(_addr_s, x86.AXORPS, x86.REG_X15, x86.REG_X15); 
            // set G register from TLS
            getgFromTLS(_addr_s, x86.REG_R14);

        }
        goto __switch_break0;
    }
    if (v.Op == ssa.OpAMD64CALLclosure || v.Op == ssa.OpAMD64CALLinter)
    {
        s.Call(v);
        goto __switch_break0;
    }
    if (v.Op == ssa.OpAMD64LoweredGetCallerPC)
    {
        p = s.Prog(x86.AMOVQ);
        p.From.Type = obj.TYPE_MEM;
        p.From.Offset = -8; // PC is stored 8 bytes below first parameter.
        p.From.Name = obj.NAME_PARAM;
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = v.Reg();
        goto __switch_break0;
    }
    if (v.Op == ssa.OpAMD64LoweredGetCallerSP) 
    {
        // caller's SP is the address of the first arg
        var mov = x86.AMOVQ;
        if (types.PtrSize == 4) {
            mov = x86.AMOVL;
        }
        p = s.Prog(mov);
        p.From.Type = obj.TYPE_ADDR;
        p.From.Offset = -@base.Ctxt.FixedFrameSize(); // 0 on amd64, just to be consistent with other architectures
        p.From.Name = obj.NAME_PARAM;
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = v.Reg();
        goto __switch_break0;
    }
    if (v.Op == ssa.OpAMD64LoweredWB)
    {
        p = s.Prog(obj.ACALL);
        p.To.Type = obj.TYPE_MEM;
        p.To.Name = obj.NAME_EXTERN; 
        // arg0 is in DI. Set sym to match where regalloc put arg1.
        p.To.Sym = ssagen.GCWriteBarrierReg[v.Args[1].Reg()];
        goto __switch_break0;
    }
    if (v.Op == ssa.OpAMD64LoweredPanicBoundsA || v.Op == ssa.OpAMD64LoweredPanicBoundsB || v.Op == ssa.OpAMD64LoweredPanicBoundsC)
    {
        p = s.Prog(obj.ACALL);
        p.To.Type = obj.TYPE_MEM;
        p.To.Name = obj.NAME_EXTERN;
        p.To.Sym = ssagen.BoundsCheckFunc[v.AuxInt];
        s.UseArgs(int64(2 * types.PtrSize)); // space used in callee args area by assembly stubs
        goto __switch_break0;
    }
    if (v.Op == ssa.OpAMD64NEGQ || v.Op == ssa.OpAMD64NEGL || v.Op == ssa.OpAMD64BSWAPQ || v.Op == ssa.OpAMD64BSWAPL || v.Op == ssa.OpAMD64NOTQ || v.Op == ssa.OpAMD64NOTL)
    {
        p = s.Prog(v.Op.Asm());
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = v.Reg();
        goto __switch_break0;
    }
    if (v.Op == ssa.OpAMD64NEGLflags)
    {
        p = s.Prog(v.Op.Asm());
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = v.Reg0();
        goto __switch_break0;
    }
    if (v.Op == ssa.OpAMD64BSFQ || v.Op == ssa.OpAMD64BSRQ || v.Op == ssa.OpAMD64BSFL || v.Op == ssa.OpAMD64BSRL || v.Op == ssa.OpAMD64SQRTSD || v.Op == ssa.OpAMD64SQRTSS)
    {
        p = s.Prog(v.Op.Asm());
        p.From.Type = obj.TYPE_REG;
        p.From.Reg = v.Args[0].Reg();
        p.To.Type = obj.TYPE_REG;

        if (v.Op == ssa.OpAMD64BSFQ || v.Op == ssa.OpAMD64BSRQ) 
            p.To.Reg = v.Reg0();
        else if (v.Op == ssa.OpAMD64BSFL || v.Op == ssa.OpAMD64BSRL || v.Op == ssa.OpAMD64SQRTSD || v.Op == ssa.OpAMD64SQRTSS) 
            p.To.Reg = v.Reg();
                goto __switch_break0;
    }
    if (v.Op == ssa.OpAMD64ROUNDSD)
    {
        p = s.Prog(v.Op.Asm());
        val = v.AuxInt; 
        // 0 means math.RoundToEven, 1 Floor, 2 Ceil, 3 Trunc
        if (val < 0 || val > 3) {
            v.Fatalf("Invalid rounding mode");
        }
        p.From.Offset = val;
        p.From.Type = obj.TYPE_CONST;
        p.SetFrom3Reg(v.Args[0].Reg());
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = v.Reg();
        goto __switch_break0;
    }
    if (v.Op == ssa.OpAMD64POPCNTQ || v.Op == ssa.OpAMD64POPCNTL)
    {
        if (v.Args[0].Reg() != v.Reg()) { 
            // POPCNT on Intel has a false dependency on the destination register.
            // Xor register with itself to break the dependency.
            p = s.Prog(x86.AXORQ);
            p.From.Type = obj.TYPE_REG;
            p.From.Reg = v.Reg();
            p.To.Type = obj.TYPE_REG;
            p.To.Reg = v.Reg();

        }
        p = s.Prog(v.Op.Asm());
        p.From.Type = obj.TYPE_REG;
        p.From.Reg = v.Args[0].Reg();
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = v.Reg();
        goto __switch_break0;
    }
    if (v.Op == ssa.OpAMD64SETEQ || v.Op == ssa.OpAMD64SETNE || v.Op == ssa.OpAMD64SETL || v.Op == ssa.OpAMD64SETLE || v.Op == ssa.OpAMD64SETG || v.Op == ssa.OpAMD64SETGE || v.Op == ssa.OpAMD64SETGF || v.Op == ssa.OpAMD64SETGEF || v.Op == ssa.OpAMD64SETB || v.Op == ssa.OpAMD64SETBE || v.Op == ssa.OpAMD64SETORD || v.Op == ssa.OpAMD64SETNAN || v.Op == ssa.OpAMD64SETA || v.Op == ssa.OpAMD64SETAE || v.Op == ssa.OpAMD64SETO)
    {
        p = s.Prog(v.Op.Asm());
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = v.Reg();
        goto __switch_break0;
    }
    if (v.Op == ssa.OpAMD64SETEQstore || v.Op == ssa.OpAMD64SETNEstore || v.Op == ssa.OpAMD64SETLstore || v.Op == ssa.OpAMD64SETLEstore || v.Op == ssa.OpAMD64SETGstore || v.Op == ssa.OpAMD64SETGEstore || v.Op == ssa.OpAMD64SETBstore || v.Op == ssa.OpAMD64SETBEstore || v.Op == ssa.OpAMD64SETAstore || v.Op == ssa.OpAMD64SETAEstore)
    {
        p = s.Prog(v.Op.Asm());
        p.To.Type = obj.TYPE_MEM;
        p.To.Reg = v.Args[0].Reg();
        ssagen.AddAux(_addr_p.To, v);
        goto __switch_break0;
    }
    if (v.Op == ssa.OpAMD64SETNEF)
    {
        p = s.Prog(v.Op.Asm());
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = v.Reg();
        q = s.Prog(x86.ASETPS);
        q.To.Type = obj.TYPE_REG;
        q.To.Reg = x86.REG_AX; 
        // ORL avoids partial register write and is smaller than ORQ, used by old compiler
        opregreg(_addr_s, x86.AORL, v.Reg(), x86.REG_AX);
        goto __switch_break0;
    }
    if (v.Op == ssa.OpAMD64SETEQF)
    {
        p = s.Prog(v.Op.Asm());
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = v.Reg();
        q = s.Prog(x86.ASETPC);
        q.To.Type = obj.TYPE_REG;
        q.To.Reg = x86.REG_AX; 
        // ANDL avoids partial register write and is smaller than ANDQ, used by old compiler
        opregreg(_addr_s, x86.AANDL, v.Reg(), x86.REG_AX);
        goto __switch_break0;
    }
    if (v.Op == ssa.OpAMD64InvertFlags)
    {
        v.Fatalf("InvertFlags should never make it to codegen %v", v.LongString());
        goto __switch_break0;
    }
    if (v.Op == ssa.OpAMD64FlagEQ || v.Op == ssa.OpAMD64FlagLT_ULT || v.Op == ssa.OpAMD64FlagLT_UGT || v.Op == ssa.OpAMD64FlagGT_ULT || v.Op == ssa.OpAMD64FlagGT_UGT)
    {
        v.Fatalf("Flag* ops should never make it to codegen %v", v.LongString());
        goto __switch_break0;
    }
    if (v.Op == ssa.OpAMD64AddTupleFirst32 || v.Op == ssa.OpAMD64AddTupleFirst64)
    {
        v.Fatalf("AddTupleFirst* should never make it to codegen %v", v.LongString());
        goto __switch_break0;
    }
    if (v.Op == ssa.OpAMD64REPSTOSQ)
    {
        s.Prog(x86.AREP);
        s.Prog(x86.ASTOSQ);
        goto __switch_break0;
    }
    if (v.Op == ssa.OpAMD64REPMOVSQ)
    {
        s.Prog(x86.AREP);
        s.Prog(x86.AMOVSQ);
        goto __switch_break0;
    }
    if (v.Op == ssa.OpAMD64LoweredNilCheck) 
    {
        // Issue a load which will fault if the input is nil.
        // TODO: We currently use the 2-byte instruction TESTB AX, (reg).
        // Should we use the 3-byte TESTB $0, (reg) instead? It is larger
        // but it doesn't have false dependency on AX.
        // Or maybe allocate an output register and use MOVL (reg),reg2 ?
        // That trades clobbering flags for clobbering a register.
        p = s.Prog(x86.ATESTB);
        p.From.Type = obj.TYPE_REG;
        p.From.Reg = x86.REG_AX;
        p.To.Type = obj.TYPE_MEM;
        p.To.Reg = v.Args[0].Reg();
        if (logopt.Enabled()) {
            logopt.LogOpt(v.Pos, "nilcheck", "genssa", v.Block.Func.Name);
        }
        if (@base.Debug.Nil != 0 && v.Pos.Line() > 1) { // v.Pos.Line()==1 in generated wrappers
            @base.WarnfAt(v.Pos, "generated nil check");

        }
        goto __switch_break0;
    }
    if (v.Op == ssa.OpAMD64MOVBatomicload || v.Op == ssa.OpAMD64MOVLatomicload || v.Op == ssa.OpAMD64MOVQatomicload)
    {
        p = s.Prog(v.Op.Asm());
        p.From.Type = obj.TYPE_MEM;
        p.From.Reg = v.Args[0].Reg();
        ssagen.AddAux(_addr_p.From, v);
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = v.Reg0();
        goto __switch_break0;
    }
    if (v.Op == ssa.OpAMD64XCHGB || v.Op == ssa.OpAMD64XCHGL || v.Op == ssa.OpAMD64XCHGQ)
    {
        p = s.Prog(v.Op.Asm());
        p.From.Type = obj.TYPE_REG;
        p.From.Reg = v.Reg0();
        p.To.Type = obj.TYPE_MEM;
        p.To.Reg = v.Args[1].Reg();
        ssagen.AddAux(_addr_p.To, v);
        goto __switch_break0;
    }
    if (v.Op == ssa.OpAMD64XADDLlock || v.Op == ssa.OpAMD64XADDQlock)
    {
        s.Prog(x86.ALOCK);
        p = s.Prog(v.Op.Asm());
        p.From.Type = obj.TYPE_REG;
        p.From.Reg = v.Reg0();
        p.To.Type = obj.TYPE_MEM;
        p.To.Reg = v.Args[1].Reg();
        ssagen.AddAux(_addr_p.To, v);
        goto __switch_break0;
    }
    if (v.Op == ssa.OpAMD64CMPXCHGLlock || v.Op == ssa.OpAMD64CMPXCHGQlock)
    {
        if (v.Args[1].Reg() != x86.REG_AX) {
            v.Fatalf("input[1] not in AX %s", v.LongString());
        }
        s.Prog(x86.ALOCK);
        p = s.Prog(v.Op.Asm());
        p.From.Type = obj.TYPE_REG;
        p.From.Reg = v.Args[2].Reg();
        p.To.Type = obj.TYPE_MEM;
        p.To.Reg = v.Args[0].Reg();
        ssagen.AddAux(_addr_p.To, v);
        p = s.Prog(x86.ASETEQ);
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = v.Reg0();
        goto __switch_break0;
    }
    if (v.Op == ssa.OpAMD64ANDBlock || v.Op == ssa.OpAMD64ANDLlock || v.Op == ssa.OpAMD64ORBlock || v.Op == ssa.OpAMD64ORLlock)
    {
        s.Prog(x86.ALOCK);
        p = s.Prog(v.Op.Asm());
        p.From.Type = obj.TYPE_REG;
        p.From.Reg = v.Args[1].Reg();
        p.To.Type = obj.TYPE_MEM;
        p.To.Reg = v.Args[0].Reg();
        ssagen.AddAux(_addr_p.To, v);
        goto __switch_break0;
    }
    if (v.Op == ssa.OpClobber)
    {
        p = s.Prog(x86.AMOVL);
        p.From.Type = obj.TYPE_CONST;
        p.From.Offset = 0xdeaddead;
        p.To.Type = obj.TYPE_MEM;
        p.To.Reg = x86.REG_SP;
        ssagen.AddAux(_addr_p.To, v);
        p = s.Prog(x86.AMOVL);
        p.From.Type = obj.TYPE_CONST;
        p.From.Offset = 0xdeaddead;
        p.To.Type = obj.TYPE_MEM;
        p.To.Reg = x86.REG_SP;
        ssagen.AddAux(_addr_p.To, v);
        p.To.Offset += 4;
        goto __switch_break0;
    }
    if (v.Op == ssa.OpClobberReg)
    {
        x = uint64(0xdeaddeaddeaddead);
        p = s.Prog(x86.AMOVQ);
        p.From.Type = obj.TYPE_CONST;
        p.From.Offset = int64(x);
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = v.Reg();
        goto __switch_break0;
    }
    // default: 
        v.Fatalf("genValue not implemented: %s", v.LongString());

    __switch_break0:;

}



private static array<array<ssagen.IndexJump>> eqfJumps = new array<array<ssagen.IndexJump>>(new array<ssagen.IndexJump>[] { {{Jump:x86.AJNE,Index:1},{Jump:x86.AJPS,Index:1}}, {{Jump:x86.AJNE,Index:1},{Jump:x86.AJPC,Index:0}} });
private static array<array<ssagen.IndexJump>> nefJumps = new array<array<ssagen.IndexJump>>(new array<ssagen.IndexJump>[] { {{Jump:x86.AJNE,Index:0},{Jump:x86.AJPC,Index:1}}, {{Jump:x86.AJNE,Index:0},{Jump:x86.AJPS,Index:0}} });

private static void ssaGenBlock(ptr<ssagen.State> _addr_s, ptr<ssa.Block> _addr_b, ptr<ssa.Block> _addr_next) {
    ref ssagen.State s = ref _addr_s.val;
    ref ssa.Block b = ref _addr_b.val;
    ref ssa.Block next = ref _addr_next.val;


    if (b.Kind == ssa.BlockPlain) 
        if (b.Succs[0].Block() != next) {
            var p = s.Prog(obj.AJMP);
            p.To.Type = obj.TYPE_BRANCH;
            s.Branches = append(s.Branches, new ssagen.Branch(P:p,B:b.Succs[0].Block()));
        }
    else if (b.Kind == ssa.BlockDefer) 
        // defer returns in rax:
        // 0 if we should continue executing
        // 1 if we should jump to deferreturn call
        p = s.Prog(x86.ATESTL);
        p.From.Type = obj.TYPE_REG;
        p.From.Reg = x86.REG_AX;
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = x86.REG_AX;
        p = s.Prog(x86.AJNE);
        p.To.Type = obj.TYPE_BRANCH;
        s.Branches = append(s.Branches, new ssagen.Branch(P:p,B:b.Succs[1].Block()));
        if (b.Succs[0].Block() != next) {
            p = s.Prog(obj.AJMP);
            p.To.Type = obj.TYPE_BRANCH;
            s.Branches = append(s.Branches, new ssagen.Branch(P:p,B:b.Succs[0].Block()));
        }
    else if (b.Kind == ssa.BlockExit)     else if (b.Kind == ssa.BlockRet) 
        s.Prog(obj.ARET);
    else if (b.Kind == ssa.BlockRetJmp) 
        if (buildcfg.Experiment.RegabiG && s.ABI == obj.ABI0 && b.Aux._<ptr<obj.LSym>>().ABI() == obj.ABIInternal) { 
            // zeroing X15 when entering ABIInternal from ABI0
            opregreg(_addr_s, x86.AXORPS, x86.REG_X15, x86.REG_X15); 
            // set G register from TLS
            getgFromTLS(_addr_s, x86.REG_R14);

        }
        p = s.Prog(obj.ARET);
        p.To.Type = obj.TYPE_MEM;
        p.To.Name = obj.NAME_EXTERN;
        p.To.Sym = b.Aux._<ptr<obj.LSym>>();
    else if (b.Kind == ssa.BlockAMD64EQF) 
        s.CombJump(b, next, _addr_eqfJumps);
    else if (b.Kind == ssa.BlockAMD64NEF) 
        s.CombJump(b, next, _addr_nefJumps);
    else if (b.Kind == ssa.BlockAMD64EQ || b.Kind == ssa.BlockAMD64NE || b.Kind == ssa.BlockAMD64LT || b.Kind == ssa.BlockAMD64GE || b.Kind == ssa.BlockAMD64LE || b.Kind == ssa.BlockAMD64GT || b.Kind == ssa.BlockAMD64OS || b.Kind == ssa.BlockAMD64OC || b.Kind == ssa.BlockAMD64ULT || b.Kind == ssa.BlockAMD64UGT || b.Kind == ssa.BlockAMD64ULE || b.Kind == ssa.BlockAMD64UGE) 
        var jmp = blockJump[b.Kind];

        if (next == b.Succs[0].Block()) 
            s.Br(jmp.invasm, b.Succs[1].Block());
        else if (next == b.Succs[1].Block()) 
            s.Br(jmp.asm, b.Succs[0].Block());
        else 
            if (b.Likely != ssa.BranchUnlikely) {
                s.Br(jmp.asm, b.Succs[0].Block());
                s.Br(obj.AJMP, b.Succs[1].Block());
            }
            else
 {
                s.Br(jmp.invasm, b.Succs[1].Block());
                s.Br(obj.AJMP, b.Succs[0].Block());
            }

            else 
        b.Fatalf("branch not implemented: %s", b.LongString());
    
}

private static void loadRegResults(ptr<ssagen.State> _addr_s, ptr<ssa.Func> _addr_f) {
    ref ssagen.State s = ref _addr_s.val;
    ref ssa.Func f = ref _addr_f.val;

    foreach (var (_, o) in f.OwnAux.ABIInfo().OutParams()) {
        ptr<ir.Name> n = o.Name._<ptr<ir.Name>>();
        var (rts, offs) = o.RegisterTypesAndOffsets();
        foreach (var (i) in o.Registers) {
            var p = s.Prog(loadByType(_addr_rts[i]));
            p.From.Type = obj.TYPE_MEM;
            p.From.Name = obj.NAME_AUTO;
            p.From.Sym = n.Linksym();
            p.From.Offset = n.FrameOffset() + offs[i];
            p.To.Type = obj.TYPE_REG;
            p.To.Reg = ssa.ObjRegForAbiReg(o.Registers[i], f.Config);
        }
    }
}

private static ptr<obj.Prog> spillArgReg(ptr<objw.Progs> _addr_pp, ptr<obj.Prog> _addr_p, ptr<ssa.Func> _addr_f, ptr<types.Type> _addr_t, short reg, ptr<ir.Name> _addr_n, long off) {
    ref objw.Progs pp = ref _addr_pp.val;
    ref obj.Prog p = ref _addr_p.val;
    ref ssa.Func f = ref _addr_f.val;
    ref types.Type t = ref _addr_t.val;
    ref ir.Name n = ref _addr_n.val;

    p = pp.Append(p, storeByType(_addr_t), obj.TYPE_REG, reg, 0, obj.TYPE_MEM, 0, n.FrameOffset() + off);
    p.To.Name = obj.NAME_PARAM;
    p.To.Sym = n.Linksym();
    p.Pos = p.Pos.WithNotStmt();
    return _addr_p!;
}

} // end amd64_package
