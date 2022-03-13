// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package riscv64 -- go2cs converted at 2022 March 13 06:24:14 UTC
// import "cmd/compile/internal/riscv64" ==> using riscv64 = go.cmd.compile.@internal.riscv64_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\riscv64\ssa.go
namespace go.cmd.compile.@internal;

using @base = cmd.compile.@internal.@base_package;
using ir = cmd.compile.@internal.ir_package;
using ssa = cmd.compile.@internal.ssa_package;
using ssagen = cmd.compile.@internal.ssagen_package;
using types = cmd.compile.@internal.types_package;
using obj = cmd.@internal.obj_package;
using riscv = cmd.@internal.obj.riscv_package;


// ssaRegToReg maps ssa register numbers to obj register numbers.

public static partial class riscv64_package {

private static short ssaRegToReg = new slice<short>(new short[] { riscv.REG_X0, riscv.REG_X2, riscv.REG_X3, riscv.REG_X4, riscv.REG_X5, riscv.REG_X6, riscv.REG_X7, riscv.REG_X8, riscv.REG_X9, riscv.REG_X10, riscv.REG_X11, riscv.REG_X12, riscv.REG_X13, riscv.REG_X14, riscv.REG_X15, riscv.REG_X16, riscv.REG_X17, riscv.REG_X18, riscv.REG_X19, riscv.REG_X20, riscv.REG_X21, riscv.REG_X22, riscv.REG_X23, riscv.REG_X24, riscv.REG_X25, riscv.REG_X26, riscv.REG_X27, riscv.REG_X28, riscv.REG_X29, riscv.REG_X30, riscv.REG_X31, riscv.REG_F0, riscv.REG_F1, riscv.REG_F2, riscv.REG_F3, riscv.REG_F4, riscv.REG_F5, riscv.REG_F6, riscv.REG_F7, riscv.REG_F8, riscv.REG_F9, riscv.REG_F10, riscv.REG_F11, riscv.REG_F12, riscv.REG_F13, riscv.REG_F14, riscv.REG_F15, riscv.REG_F16, riscv.REG_F17, riscv.REG_F18, riscv.REG_F19, riscv.REG_F20, riscv.REG_F21, riscv.REG_F22, riscv.REG_F23, riscv.REG_F24, riscv.REG_F25, riscv.REG_F26, riscv.REG_F27, riscv.REG_F28, riscv.REG_F29, riscv.REG_F30, riscv.REG_F31, 0 });

private static obj.As loadByType(ptr<types.Type> _addr_t) {
    ref types.Type t = ref _addr_t.val;

    var width = t.Size();

    if (t.IsFloat()) {
        switch (width) {
            case 4: 
                return riscv.AMOVF;
                break;
            case 8: 
                return riscv.AMOVD;
                break;
            default: 
                @base.Fatalf("unknown float width for load %d in type %v", width, t);
                return 0;
                break;
        }
    }
    switch (width) {
        case 1: 
                   if (t.IsSigned()) {
                       return riscv.AMOVB;
                   }
                   else
            {
                       return riscv.AMOVBU;
                   }
            break;
        case 2: 
                   if (t.IsSigned()) {
                       return riscv.AMOVH;
                   }
                   else
            {
                       return riscv.AMOVHU;
                   }
            break;
        case 4: 
                   if (t.IsSigned()) {
                       return riscv.AMOVW;
                   }
                   else
            {
                       return riscv.AMOVWU;
                   }
            break;
        case 8: 
            return riscv.AMOV;
            break;
        default: 
            @base.Fatalf("unknown width for load %d in type %v", width, t);
            return 0;
            break;
    }
}

// storeByType returns the store instruction of the given type.
private static obj.As storeByType(ptr<types.Type> _addr_t) {
    ref types.Type t = ref _addr_t.val;

    var width = t.Size();

    if (t.IsFloat()) {
        switch (width) {
            case 4: 
                return riscv.AMOVF;
                break;
            case 8: 
                return riscv.AMOVD;
                break;
            default: 
                @base.Fatalf("unknown float width for store %d in type %v", width, t);
                return 0;
                break;
        }
    }
    switch (width) {
        case 1: 
            return riscv.AMOVB;
            break;
        case 2: 
            return riscv.AMOVH;
            break;
        case 4: 
            return riscv.AMOVW;
            break;
        case 8: 
            return riscv.AMOV;
            break;
        default: 
            @base.Fatalf("unknown width for store %d in type %v", width, t);
            return 0;
            break;
    }
}

// largestMove returns the largest move instruction possible and its size,
// given the alignment of the total size of the move.
//
// e.g., a 16-byte move may use MOV, but an 11-byte move must use MOVB.
//
// Note that the moves may not be on naturally aligned addresses depending on
// the source and destination.
//
// This matches the calculation in ssa.moveSize.
private static (obj.As, long) largestMove(long alignment) {
    obj.As _p0 = default;
    long _p0 = default;


    if (alignment % 8 == 0) 
        return (riscv.AMOV, 8);
    else if (alignment % 4 == 0) 
        return (riscv.AMOVW, 4);
    else if (alignment % 2 == 0) 
        return (riscv.AMOVH, 2);
    else 
        return (riscv.AMOVB, 1);
    }

// markMoves marks any MOVXconst ops that need to avoid clobbering flags.
// RISC-V has no flags, so this is a no-op.
private static void ssaMarkMoves(ptr<ssagen.State> _addr_s, ptr<ssa.Block> _addr_b) {
    ref ssagen.State s = ref _addr_s.val;
    ref ssa.Block b = ref _addr_b.val;

}

private static void ssaGenValue(ptr<ssagen.State> _addr_s, ptr<ssa.Value> _addr_v) {
    ref ssagen.State s = ref _addr_s.val;
    ref ssa.Value v = ref _addr_v.val;

    s.SetPos(v.Pos);


    if (v.Op == ssa.OpInitMem)     else if (v.Op == ssa.OpArg)     else if (v.Op == ssa.OpPhi) 
        ssagen.CheckLoweredPhi(v);
    else if (v.Op == ssa.OpCopy || v.Op == ssa.OpRISCV64MOVconvert || v.Op == ssa.OpRISCV64MOVDreg) 
        if (v.Type.IsMemory()) {
            return ;
        }
        var rs = v.Args[0].Reg();
        var rd = v.Reg();
        if (rs == rd) {
            return ;
        }
        var @as = riscv.AMOV;
        if (v.Type.IsFloat()) {
            as = riscv.AMOVD;
        }
        var p = s.Prog(as);
        p.From.Type = obj.TYPE_REG;
        p.From.Reg = rs;
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = rd;
    else if (v.Op == ssa.OpRISCV64MOVDnop)     else if (v.Op == ssa.OpLoadReg) 
        if (v.Type.IsFlags()) {
            v.Fatalf("load flags not implemented: %v", v.LongString());
            return ;
        }
        p = s.Prog(loadByType(_addr_v.Type));
        ssagen.AddrAuto(_addr_p.From, v.Args[0]);
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = v.Reg();
    else if (v.Op == ssa.OpStoreReg) 
        if (v.Type.IsFlags()) {
            v.Fatalf("store flags not implemented: %v", v.LongString());
            return ;
        }
        p = s.Prog(storeByType(_addr_v.Type));
        p.From.Type = obj.TYPE_REG;
        p.From.Reg = v.Args[0].Reg();
        ssagen.AddrAuto(_addr_p.To, v);
    else if (v.Op == ssa.OpSP || v.Op == ssa.OpSB || v.Op == ssa.OpGetG)     else if (v.Op == ssa.OpRISCV64MOVBreg || v.Op == ssa.OpRISCV64MOVHreg || v.Op == ssa.OpRISCV64MOVWreg || v.Op == ssa.OpRISCV64MOVBUreg || v.Op == ssa.OpRISCV64MOVHUreg || v.Op == ssa.OpRISCV64MOVWUreg) 
        var a = v.Args[0];
        while (a.Op == ssa.OpCopy || a.Op == ssa.OpRISCV64MOVDreg) {
            a = a.Args[0];
        }
        @as = v.Op.Asm();
        rs = v.Args[0].Reg();
        rd = v.Reg();
        if (a.Op == ssa.OpLoadReg) {
            var t = a.Type;

            if (v.Op == ssa.OpRISCV64MOVBreg && t.Size() == 1 && t.IsSigned() || v.Op == ssa.OpRISCV64MOVHreg && t.Size() == 2 && t.IsSigned() || v.Op == ssa.OpRISCV64MOVWreg && t.Size() == 4 && t.IsSigned() || v.Op == ssa.OpRISCV64MOVBUreg && t.Size() == 1 && !t.IsSigned() || v.Op == ssa.OpRISCV64MOVHUreg && t.Size() == 2 && !t.IsSigned() || v.Op == ssa.OpRISCV64MOVWUreg && t.Size() == 4 && !t.IsSigned()) 
                // arg is a proper-typed load and already sign/zero-extended
                if (rs == rd) {
                    return ;
                }
                as = riscv.AMOV;
            else             
        }
        p = s.Prog(as);
        p.From.Type = obj.TYPE_REG;
        p.From.Reg = rs;
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = rd;
    else if (v.Op == ssa.OpRISCV64ADD || v.Op == ssa.OpRISCV64SUB || v.Op == ssa.OpRISCV64SUBW || v.Op == ssa.OpRISCV64XOR || v.Op == ssa.OpRISCV64OR || v.Op == ssa.OpRISCV64AND || v.Op == ssa.OpRISCV64SLL || v.Op == ssa.OpRISCV64SRA || v.Op == ssa.OpRISCV64SRL || v.Op == ssa.OpRISCV64SLT || v.Op == ssa.OpRISCV64SLTU || v.Op == ssa.OpRISCV64MUL || v.Op == ssa.OpRISCV64MULW || v.Op == ssa.OpRISCV64MULH || v.Op == ssa.OpRISCV64MULHU || v.Op == ssa.OpRISCV64DIV || v.Op == ssa.OpRISCV64DIVU || v.Op == ssa.OpRISCV64DIVW || v.Op == ssa.OpRISCV64DIVUW || v.Op == ssa.OpRISCV64REM || v.Op == ssa.OpRISCV64REMU || v.Op == ssa.OpRISCV64REMW || v.Op == ssa.OpRISCV64REMUW || v.Op == ssa.OpRISCV64FADDS || v.Op == ssa.OpRISCV64FSUBS || v.Op == ssa.OpRISCV64FMULS || v.Op == ssa.OpRISCV64FDIVS || v.Op == ssa.OpRISCV64FEQS || v.Op == ssa.OpRISCV64FNES || v.Op == ssa.OpRISCV64FLTS || v.Op == ssa.OpRISCV64FLES || v.Op == ssa.OpRISCV64FADDD || v.Op == ssa.OpRISCV64FSUBD || v.Op == ssa.OpRISCV64FMULD || v.Op == ssa.OpRISCV64FDIVD || v.Op == ssa.OpRISCV64FEQD || v.Op == ssa.OpRISCV64FNED || v.Op == ssa.OpRISCV64FLTD || v.Op == ssa.OpRISCV64FLED) 
        var r = v.Reg();
        var r1 = v.Args[0].Reg();
        var r2 = v.Args[1].Reg();
        p = s.Prog(v.Op.Asm());
        p.From.Type = obj.TYPE_REG;
        p.From.Reg = r2;
        p.Reg = r1;
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = r;
    else if (v.Op == ssa.OpRISCV64FSQRTS || v.Op == ssa.OpRISCV64FNEGS || v.Op == ssa.OpRISCV64FSQRTD || v.Op == ssa.OpRISCV64FNEGD || v.Op == ssa.OpRISCV64FMVSX || v.Op == ssa.OpRISCV64FMVDX || v.Op == ssa.OpRISCV64FCVTSW || v.Op == ssa.OpRISCV64FCVTSL || v.Op == ssa.OpRISCV64FCVTWS || v.Op == ssa.OpRISCV64FCVTLS || v.Op == ssa.OpRISCV64FCVTDW || v.Op == ssa.OpRISCV64FCVTDL || v.Op == ssa.OpRISCV64FCVTWD || v.Op == ssa.OpRISCV64FCVTLD || v.Op == ssa.OpRISCV64FCVTDS || v.Op == ssa.OpRISCV64FCVTSD || v.Op == ssa.OpRISCV64NOT || v.Op == ssa.OpRISCV64NEG || v.Op == ssa.OpRISCV64NEGW) 
        p = s.Prog(v.Op.Asm());
        p.From.Type = obj.TYPE_REG;
        p.From.Reg = v.Args[0].Reg();
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = v.Reg();
    else if (v.Op == ssa.OpRISCV64ADDI || v.Op == ssa.OpRISCV64ADDIW || v.Op == ssa.OpRISCV64XORI || v.Op == ssa.OpRISCV64ORI || v.Op == ssa.OpRISCV64ANDI || v.Op == ssa.OpRISCV64SLLI || v.Op == ssa.OpRISCV64SRAI || v.Op == ssa.OpRISCV64SRLI || v.Op == ssa.OpRISCV64SLTI || v.Op == ssa.OpRISCV64SLTIU) 
        p = s.Prog(v.Op.Asm());
        p.From.Type = obj.TYPE_CONST;
        p.From.Offset = v.AuxInt;
        p.Reg = v.Args[0].Reg();
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = v.Reg();
    else if (v.Op == ssa.OpRISCV64MOVDconst) 
        p = s.Prog(v.Op.Asm());
        p.From.Type = obj.TYPE_CONST;
        p.From.Offset = v.AuxInt;
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = v.Reg();
    else if (v.Op == ssa.OpRISCV64MOVaddr) 
        p = s.Prog(v.Op.Asm());
        p.From.Type = obj.TYPE_ADDR;
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = v.Reg();

        @string wantreg = default; 
        // MOVW $sym+off(base), R
        switch (v.Aux.type()) {
            case ptr<obj.LSym> _:
                wantreg = "SB";
                ssagen.AddAux(_addr_p.From, v);
                break;
            case ptr<ir.Name> _:
                wantreg = "SP";
                ssagen.AddAux(_addr_p.From, v);
                break;
            case 
                wantreg = "SP";
                p.From.Reg = riscv.REG_SP;
                p.From.Offset = v.AuxInt;
                break;
            default:
            {
                v.Fatalf("aux is of unknown type %T", v.Aux);
                break;
            }
        }
        {
            var reg = v.Args[0].RegName();

            if (reg != wantreg) {
                v.Fatalf("bad reg %s for symbol type %T, want %s", reg, v.Aux, wantreg);
            }

        }
    else if (v.Op == ssa.OpRISCV64MOVBload || v.Op == ssa.OpRISCV64MOVHload || v.Op == ssa.OpRISCV64MOVWload || v.Op == ssa.OpRISCV64MOVDload || v.Op == ssa.OpRISCV64MOVBUload || v.Op == ssa.OpRISCV64MOVHUload || v.Op == ssa.OpRISCV64MOVWUload || v.Op == ssa.OpRISCV64FMOVWload || v.Op == ssa.OpRISCV64FMOVDload) 
        p = s.Prog(v.Op.Asm());
        p.From.Type = obj.TYPE_MEM;
        p.From.Reg = v.Args[0].Reg();
        ssagen.AddAux(_addr_p.From, v);
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = v.Reg();
    else if (v.Op == ssa.OpRISCV64MOVBstore || v.Op == ssa.OpRISCV64MOVHstore || v.Op == ssa.OpRISCV64MOVWstore || v.Op == ssa.OpRISCV64MOVDstore || v.Op == ssa.OpRISCV64FMOVWstore || v.Op == ssa.OpRISCV64FMOVDstore) 
        p = s.Prog(v.Op.Asm());
        p.From.Type = obj.TYPE_REG;
        p.From.Reg = v.Args[1].Reg();
        p.To.Type = obj.TYPE_MEM;
        p.To.Reg = v.Args[0].Reg();
        ssagen.AddAux(_addr_p.To, v);
    else if (v.Op == ssa.OpRISCV64MOVBstorezero || v.Op == ssa.OpRISCV64MOVHstorezero || v.Op == ssa.OpRISCV64MOVWstorezero || v.Op == ssa.OpRISCV64MOVDstorezero) 
        p = s.Prog(v.Op.Asm());
        p.From.Type = obj.TYPE_REG;
        p.From.Reg = riscv.REG_ZERO;
        p.To.Type = obj.TYPE_MEM;
        p.To.Reg = v.Args[0].Reg();
        ssagen.AddAux(_addr_p.To, v);
    else if (v.Op == ssa.OpRISCV64SEQZ || v.Op == ssa.OpRISCV64SNEZ) 
        p = s.Prog(v.Op.Asm());
        p.From.Type = obj.TYPE_REG;
        p.From.Reg = v.Args[0].Reg();
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = v.Reg();
    else if (v.Op == ssa.OpRISCV64CALLstatic || v.Op == ssa.OpRISCV64CALLclosure || v.Op == ssa.OpRISCV64CALLinter) 
        s.Call(v);
    else if (v.Op == ssa.OpRISCV64LoweredWB) 
        p = s.Prog(obj.ACALL);
        p.To.Type = obj.TYPE_MEM;
        p.To.Name = obj.NAME_EXTERN;
        p.To.Sym = v.Aux._<ptr<obj.LSym>>();
    else if (v.Op == ssa.OpRISCV64LoweredPanicBoundsA || v.Op == ssa.OpRISCV64LoweredPanicBoundsB || v.Op == ssa.OpRISCV64LoweredPanicBoundsC) 
        p = s.Prog(obj.ACALL);
        p.To.Type = obj.TYPE_MEM;
        p.To.Name = obj.NAME_EXTERN;
        p.To.Sym = ssagen.BoundsCheckFunc[v.AuxInt];
        s.UseArgs(16); // space used in callee args area by assembly stubs
    else if (v.Op == ssa.OpRISCV64LoweredAtomicLoad8) 
        s.Prog(riscv.AFENCE);
        p = s.Prog(riscv.AMOVBU);
        p.From.Type = obj.TYPE_MEM;
        p.From.Reg = v.Args[0].Reg();
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = v.Reg0();
        s.Prog(riscv.AFENCE);
    else if (v.Op == ssa.OpRISCV64LoweredAtomicLoad32 || v.Op == ssa.OpRISCV64LoweredAtomicLoad64) 
        @as = riscv.ALRW;
        if (v.Op == ssa.OpRISCV64LoweredAtomicLoad64) {
            as = riscv.ALRD;
        }
        p = s.Prog(as);
        p.From.Type = obj.TYPE_MEM;
        p.From.Reg = v.Args[0].Reg();
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = v.Reg0();
    else if (v.Op == ssa.OpRISCV64LoweredAtomicStore8) 
        s.Prog(riscv.AFENCE);
        p = s.Prog(riscv.AMOVB);
        p.From.Type = obj.TYPE_REG;
        p.From.Reg = v.Args[1].Reg();
        p.To.Type = obj.TYPE_MEM;
        p.To.Reg = v.Args[0].Reg();
        s.Prog(riscv.AFENCE);
    else if (v.Op == ssa.OpRISCV64LoweredAtomicStore32 || v.Op == ssa.OpRISCV64LoweredAtomicStore64) 
        @as = riscv.AAMOSWAPW;
        if (v.Op == ssa.OpRISCV64LoweredAtomicStore64) {
            as = riscv.AAMOSWAPD;
        }
        p = s.Prog(as);
        p.From.Type = obj.TYPE_REG;
        p.From.Reg = v.Args[1].Reg();
        p.To.Type = obj.TYPE_MEM;
        p.To.Reg = v.Args[0].Reg();
        p.RegTo2 = riscv.REG_ZERO;
    else if (v.Op == ssa.OpRISCV64LoweredAtomicAdd32 || v.Op == ssa.OpRISCV64LoweredAtomicAdd64) 
        @as = riscv.AAMOADDW;
        if (v.Op == ssa.OpRISCV64LoweredAtomicAdd64) {
            as = riscv.AAMOADDD;
        }
        p = s.Prog(as);
        p.From.Type = obj.TYPE_REG;
        p.From.Reg = v.Args[1].Reg();
        p.To.Type = obj.TYPE_MEM;
        p.To.Reg = v.Args[0].Reg();
        p.RegTo2 = riscv.REG_TMP;

        var p2 = s.Prog(riscv.AADD);
        p2.From.Type = obj.TYPE_REG;
        p2.From.Reg = riscv.REG_TMP;
        p2.Reg = v.Args[1].Reg();
        p2.To.Type = obj.TYPE_REG;
        p2.To.Reg = v.Reg0();
    else if (v.Op == ssa.OpRISCV64LoweredAtomicExchange32 || v.Op == ssa.OpRISCV64LoweredAtomicExchange64) 
        @as = riscv.AAMOSWAPW;
        if (v.Op == ssa.OpRISCV64LoweredAtomicExchange64) {
            as = riscv.AAMOSWAPD;
        }
        p = s.Prog(as);
        p.From.Type = obj.TYPE_REG;
        p.From.Reg = v.Args[1].Reg();
        p.To.Type = obj.TYPE_MEM;
        p.To.Reg = v.Args[0].Reg();
        p.RegTo2 = v.Reg0();
    else if (v.Op == ssa.OpRISCV64LoweredAtomicCas32 || v.Op == ssa.OpRISCV64LoweredAtomicCas64) 
        // MOV  ZERO, Rout
        // LR    (Rarg0), Rtmp
        // BNE    Rtmp, Rarg1, 3(PC)
        // SC    Rarg2, (Rarg0), Rtmp
        // BNE    Rtmp, ZERO, -3(PC)
        // MOV    $1, Rout

        var lr = riscv.ALRW;
        var sc = riscv.ASCW;
        if (v.Op == ssa.OpRISCV64LoweredAtomicCas64) {
            lr = riscv.ALRD;
            sc = riscv.ASCD;
        }
        var r0 = v.Args[0].Reg();
        r1 = v.Args[1].Reg();
        r2 = v.Args[2].Reg();
        var @out = v.Reg0();

        p = s.Prog(riscv.AMOV);
        p.From.Type = obj.TYPE_REG;
        p.From.Reg = riscv.REG_ZERO;
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = out;

        var p1 = s.Prog(lr);
        p1.From.Type = obj.TYPE_MEM;
        p1.From.Reg = r0;
        p1.To.Type = obj.TYPE_REG;
        p1.To.Reg = riscv.REG_TMP;

        p2 = s.Prog(riscv.ABNE);
        p2.From.Type = obj.TYPE_REG;
        p2.From.Reg = r1;
        p2.Reg = riscv.REG_TMP;
        p2.To.Type = obj.TYPE_BRANCH;

        var p3 = s.Prog(sc);
        p3.From.Type = obj.TYPE_REG;
        p3.From.Reg = r2;
        p3.To.Type = obj.TYPE_MEM;
        p3.To.Reg = r0;
        p3.RegTo2 = riscv.REG_TMP;

        var p4 = s.Prog(riscv.ABNE);
        p4.From.Type = obj.TYPE_REG;
        p4.From.Reg = riscv.REG_TMP;
        p4.Reg = riscv.REG_ZERO;
        p4.To.Type = obj.TYPE_BRANCH;
        p4.To.SetTarget(p1);

        var p5 = s.Prog(riscv.AMOV);
        p5.From.Type = obj.TYPE_CONST;
        p5.From.Offset = 1;
        p5.To.Type = obj.TYPE_REG;
        p5.To.Reg = out;

        var p6 = s.Prog(obj.ANOP);
        p2.To.SetTarget(p6);
    else if (v.Op == ssa.OpRISCV64LoweredAtomicAnd32 || v.Op == ssa.OpRISCV64LoweredAtomicOr32) 
        p = s.Prog(v.Op.Asm());
        p.From.Type = obj.TYPE_REG;
        p.From.Reg = v.Args[1].Reg();
        p.To.Type = obj.TYPE_MEM;
        p.To.Reg = v.Args[0].Reg();
        p.RegTo2 = riscv.REG_ZERO;
    else if (v.Op == ssa.OpRISCV64LoweredZero) 
        var (mov, sz) = largestMove(v.AuxInt); 

        //    mov    ZERO, (Rarg0)
        //    ADD    $sz, Rarg0
        //    BGEU    Rarg1, Rarg0, -2(PC)

        p = s.Prog(mov);
        p.From.Type = obj.TYPE_REG;
        p.From.Reg = riscv.REG_ZERO;
        p.To.Type = obj.TYPE_MEM;
        p.To.Reg = v.Args[0].Reg();

        p2 = s.Prog(riscv.AADD);
        p2.From.Type = obj.TYPE_CONST;
        p2.From.Offset = sz;
        p2.To.Type = obj.TYPE_REG;
        p2.To.Reg = v.Args[0].Reg();

        p3 = s.Prog(riscv.ABGEU);
        p3.To.Type = obj.TYPE_BRANCH;
        p3.Reg = v.Args[0].Reg();
        p3.From.Type = obj.TYPE_REG;
        p3.From.Reg = v.Args[1].Reg();
        p3.To.SetTarget(p);
    else if (v.Op == ssa.OpRISCV64LoweredMove) 
        (mov, sz) = largestMove(v.AuxInt); 

        //    mov    (Rarg1), T2
        //    mov    T2, (Rarg0)
        //    ADD    $sz, Rarg0
        //    ADD    $sz, Rarg1
        //    BGEU    Rarg2, Rarg0, -4(PC)

        p = s.Prog(mov);
        p.From.Type = obj.TYPE_MEM;
        p.From.Reg = v.Args[1].Reg();
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = riscv.REG_T2;

        p2 = s.Prog(mov);
        p2.From.Type = obj.TYPE_REG;
        p2.From.Reg = riscv.REG_T2;
        p2.To.Type = obj.TYPE_MEM;
        p2.To.Reg = v.Args[0].Reg();

        p3 = s.Prog(riscv.AADD);
        p3.From.Type = obj.TYPE_CONST;
        p3.From.Offset = sz;
        p3.To.Type = obj.TYPE_REG;
        p3.To.Reg = v.Args[0].Reg();

        p4 = s.Prog(riscv.AADD);
        p4.From.Type = obj.TYPE_CONST;
        p4.From.Offset = sz;
        p4.To.Type = obj.TYPE_REG;
        p4.To.Reg = v.Args[1].Reg();

        p5 = s.Prog(riscv.ABGEU);
        p5.To.Type = obj.TYPE_BRANCH;
        p5.Reg = v.Args[1].Reg();
        p5.From.Type = obj.TYPE_REG;
        p5.From.Reg = v.Args[2].Reg();
        p5.To.SetTarget(p);
    else if (v.Op == ssa.OpRISCV64LoweredNilCheck) 
        // Issue a load which will fault if arg is nil.
        // TODO: optimizations. See arm and amd64 LoweredNilCheck.
        p = s.Prog(riscv.AMOVB);
        p.From.Type = obj.TYPE_MEM;
        p.From.Reg = v.Args[0].Reg();
        ssagen.AddAux(_addr_p.From, v);
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = riscv.REG_ZERO;
        if (@base.Debug.Nil != 0 && v.Pos.Line() > 1) { // v.Pos == 1 in generated wrappers
            @base.WarnfAt(v.Pos, "generated nil check");
        }
    else if (v.Op == ssa.OpRISCV64LoweredGetClosurePtr) 
        // Closure pointer is S4 (riscv.REG_CTXT).
        ssagen.CheckLoweredGetClosurePtr(v);
    else if (v.Op == ssa.OpRISCV64LoweredGetCallerSP) 
        // caller's SP is FixedFrameSize below the address of the first arg
        p = s.Prog(riscv.AMOV);
        p.From.Type = obj.TYPE_ADDR;
        p.From.Offset = -@base.Ctxt.FixedFrameSize();
        p.From.Name = obj.NAME_PARAM;
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = v.Reg();
    else if (v.Op == ssa.OpRISCV64LoweredGetCallerPC) 
        p = s.Prog(obj.AGETCALLERPC);
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = v.Reg();
    else if (v.Op == ssa.OpRISCV64DUFFZERO) 
        p = s.Prog(obj.ADUFFZERO);
        p.To.Type = obj.TYPE_MEM;
        p.To.Name = obj.NAME_EXTERN;
        p.To.Sym = ir.Syms.Duffzero;
        p.To.Offset = v.AuxInt;
    else if (v.Op == ssa.OpRISCV64DUFFCOPY) 
        p = s.Prog(obj.ADUFFCOPY);
        p.To.Type = obj.TYPE_MEM;
        p.To.Name = obj.NAME_EXTERN;
        p.To.Sym = ir.Syms.Duffcopy;
        p.To.Offset = v.AuxInt;
    else if (v.Op == ssa.OpClobber || v.Op == ssa.OpClobberReg)     else 
        v.Fatalf("Unhandled op %v", v.Op);
    }

private static array<obj.As> blockBranch = new array<obj.As>(InitKeyedValues<obj.As>((ssa.BlockRISCV64BEQ, riscv.ABEQ), (ssa.BlockRISCV64BEQZ, riscv.ABEQZ), (ssa.BlockRISCV64BGE, riscv.ABGE), (ssa.BlockRISCV64BGEU, riscv.ABGEU), (ssa.BlockRISCV64BGEZ, riscv.ABGEZ), (ssa.BlockRISCV64BGTZ, riscv.ABGTZ), (ssa.BlockRISCV64BLEZ, riscv.ABLEZ), (ssa.BlockRISCV64BLT, riscv.ABLT), (ssa.BlockRISCV64BLTU, riscv.ABLTU), (ssa.BlockRISCV64BLTZ, riscv.ABLTZ), (ssa.BlockRISCV64BNE, riscv.ABNE), (ssa.BlockRISCV64BNEZ, riscv.ABNEZ)));

private static void ssaGenBlock(ptr<ssagen.State> _addr_s, ptr<ssa.Block> _addr_b, ptr<ssa.Block> _addr_next) {
    ref ssagen.State s = ref _addr_s.val;
    ref ssa.Block b = ref _addr_b.val;
    ref ssa.Block next = ref _addr_next.val;

    s.SetPos(b.Pos);


    if (b.Kind == ssa.BlockDefer) 
        // defer returns in A0:
        // 0 if we should continue executing
        // 1 if we should jump to deferreturn call
        var p = s.Prog(riscv.ABNE);
        p.To.Type = obj.TYPE_BRANCH;
        p.From.Type = obj.TYPE_REG;
        p.From.Reg = riscv.REG_ZERO;
        p.Reg = riscv.REG_A0;
        s.Branches = append(s.Branches, new ssagen.Branch(P:p,B:b.Succs[1].Block()));
        if (b.Succs[0].Block() != next) {
            p = s.Prog(obj.AJMP);
            p.To.Type = obj.TYPE_BRANCH;
            s.Branches = append(s.Branches, new ssagen.Branch(P:p,B:b.Succs[0].Block()));
        }
    else if (b.Kind == ssa.BlockPlain) 
        if (b.Succs[0].Block() != next) {
            p = s.Prog(obj.AJMP);
            p.To.Type = obj.TYPE_BRANCH;
            s.Branches = append(s.Branches, new ssagen.Branch(P:p,B:b.Succs[0].Block()));
        }
    else if (b.Kind == ssa.BlockExit)     else if (b.Kind == ssa.BlockRet) 
        s.Prog(obj.ARET);
    else if (b.Kind == ssa.BlockRetJmp) 
        p = s.Prog(obj.ARET);
        p.To.Type = obj.TYPE_MEM;
        p.To.Name = obj.NAME_EXTERN;
        p.To.Sym = b.Aux._<ptr<obj.LSym>>();
    else if (b.Kind == ssa.BlockRISCV64BEQ || b.Kind == ssa.BlockRISCV64BEQZ || b.Kind == ssa.BlockRISCV64BNE || b.Kind == ssa.BlockRISCV64BNEZ || b.Kind == ssa.BlockRISCV64BLT || b.Kind == ssa.BlockRISCV64BLEZ || b.Kind == ssa.BlockRISCV64BGE || b.Kind == ssa.BlockRISCV64BGEZ || b.Kind == ssa.BlockRISCV64BLTZ || b.Kind == ssa.BlockRISCV64BGTZ || b.Kind == ssa.BlockRISCV64BLTU || b.Kind == ssa.BlockRISCV64BGEU) 

        var @as = blockBranch[b.Kind];
        var invAs = riscv.InvertBranch(as);

        p = ;

        if (next == b.Succs[0].Block()) 
            p = s.Br(invAs, b.Succs[1].Block());
        else if (next == b.Succs[1].Block()) 
            p = s.Br(as, b.Succs[0].Block());
        else 
            if (b.Likely != ssa.BranchUnlikely) {
                p = s.Br(as, b.Succs[0].Block());
                s.Br(obj.AJMP, b.Succs[1].Block());
            }
            else
 {
                p = s.Br(invAs, b.Succs[1].Block());
                s.Br(obj.AJMP, b.Succs[0].Block());
            }
                p.From.Type = obj.TYPE_REG;

        if (b.Kind == ssa.BlockRISCV64BEQ || b.Kind == ssa.BlockRISCV64BNE || b.Kind == ssa.BlockRISCV64BLT || b.Kind == ssa.BlockRISCV64BGE || b.Kind == ssa.BlockRISCV64BLTU || b.Kind == ssa.BlockRISCV64BGEU) 
            if (b.NumControls() != 2) {
                b.Fatalf("Unexpected number of controls (%d != 2): %s", b.NumControls(), b.LongString());
            }
            p.From.Reg = b.Controls[0].Reg();
            p.Reg = b.Controls[1].Reg();
        else if (b.Kind == ssa.BlockRISCV64BEQZ || b.Kind == ssa.BlockRISCV64BNEZ || b.Kind == ssa.BlockRISCV64BGEZ || b.Kind == ssa.BlockRISCV64BLEZ || b.Kind == ssa.BlockRISCV64BLTZ || b.Kind == ssa.BlockRISCV64BGTZ) 
            if (b.NumControls() != 1) {
                b.Fatalf("Unexpected number of controls (%d != 1): %s", b.NumControls(), b.LongString());
            }
            p.From.Reg = b.Controls[0].Reg();
            else 
        b.Fatalf("Unhandled block: %s", b.LongString());
    }

} // end riscv64_package
