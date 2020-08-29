// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package mips -- go2cs converted at 2020 August 29 09:25:27 UTC
// import "cmd/compile/internal/mips" ==> using mips = go.cmd.compile.@internal.mips_package
// Original source: C:\Go\src\cmd\compile\internal\mips\ssa.go
using math = go.math_package;

using gc = go.cmd.compile.@internal.gc_package;
using ssa = go.cmd.compile.@internal.ssa_package;
using types = go.cmd.compile.@internal.types_package;
using obj = go.cmd.@internal.obj_package;
using mips = go.cmd.@internal.obj.mips_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class mips_package
    {
        // isFPreg returns whether r is an FP register
        private static bool isFPreg(short r)
        {
            return mips.REG_F0 <= r && r <= mips.REG_F31;
        }

        // isHILO returns whether r is HI or LO register
        private static bool isHILO(short r)
        {
            return r == mips.REG_HI || r == mips.REG_LO;
        }

        // loadByType returns the load instruction of the given type.
        private static obj.As loadByType(ref types.Type _t, short r) => func(_t, (ref types.Type t, Defer _, Panic panic, Recover __) =>
        {
            if (isFPreg(r))
            {
                if (t.Size() == 4L)
                { // float32 or int32
                    return mips.AMOVF;
                }
                else
                { // float64 or int64
                    return mips.AMOVD;
                }
            }
            else
            {
                switch (t.Size())
                {
                    case 1L: 
                        if (t.IsSigned())
                        {
                            return mips.AMOVB;
                        }
                        else
                        {
                            return mips.AMOVBU;
                        }
                        break;
                    case 2L: 
                        if (t.IsSigned())
                        {
                            return mips.AMOVH;
                        }
                        else
                        {
                            return mips.AMOVHU;
                        }
                        break;
                    case 4L: 
                        return mips.AMOVW;
                        break;
                }
            }
            panic("bad load type");
        });

        // storeByType returns the store instruction of the given type.
        private static obj.As storeByType(ref types.Type _t, short r) => func(_t, (ref types.Type t, Defer _, Panic panic, Recover __) =>
        {
            if (isFPreg(r))
            {
                if (t.Size() == 4L)
                { // float32 or int32
                    return mips.AMOVF;
                }
                else
                { // float64 or int64
                    return mips.AMOVD;
                }
            }
            else
            {
                switch (t.Size())
                {
                    case 1L: 
                        return mips.AMOVB;
                        break;
                    case 2L: 
                        return mips.AMOVH;
                        break;
                    case 4L: 
                        return mips.AMOVW;
                        break;
                }
            }
            panic("bad store type");
        });

        private static void ssaGenValue(ref gc.SSAGenState s, ref ssa.Value v)
        {

            if (v.Op == ssa.OpCopy || v.Op == ssa.OpMIPSMOVWconvert || v.Op == ssa.OpMIPSMOVWreg)
            {
                var t = v.Type;
                if (t.IsMemory())
                {
                    return;
                }
                var x = v.Args[0L].Reg();
                var y = v.Reg();
                if (x == y)
                {
                    return;
                }
                var @as = mips.AMOVW;
                if (isFPreg(x) && isFPreg(y))
                {
                    as = mips.AMOVF;
                    if (t.Size() == 8L)
                    {
                        as = mips.AMOVD;
                    }
                }
                var p = s.Prog(as);
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = x;
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = y;
                if (isHILO(x) && isHILO(y) || isHILO(x) && isFPreg(y) || isFPreg(x) && isHILO(y))
                { 
                    // cannot move between special registers, use TMP as intermediate
                    p.To.Reg = mips.REGTMP;
                    p = s.Prog(mips.AMOVW);
                    p.From.Type = obj.TYPE_REG;
                    p.From.Reg = mips.REGTMP;
                    p.To.Type = obj.TYPE_REG;
                    p.To.Reg = y;
                }
                goto __switch_break0;
            }
            if (v.Op == ssa.OpMIPSMOVWnop)
            {
                if (v.Reg() != v.Args[0L].Reg())
                {
                    v.Fatalf("input[0] and output not in same register %s", v.LongString());
                } 
                // nothing to do
                goto __switch_break0;
            }
            if (v.Op == ssa.OpLoadReg)
            {
                if (v.Type.IsFlags())
                {
                    v.Fatalf("load flags not implemented: %v", v.LongString());
                    return;
                }
                var r = v.Reg();
                p = s.Prog(loadByType(v.Type, r));
                gc.AddrAuto(ref p.From, v.Args[0L]);
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = r;
                if (isHILO(r))
                { 
                    // cannot directly load, load to TMP and move
                    p.To.Reg = mips.REGTMP;
                    p = s.Prog(mips.AMOVW);
                    p.From.Type = obj.TYPE_REG;
                    p.From.Reg = mips.REGTMP;
                    p.To.Type = obj.TYPE_REG;
                    p.To.Reg = r;
                }
                goto __switch_break0;
            }
            if (v.Op == ssa.OpStoreReg)
            {
                if (v.Type.IsFlags())
                {
                    v.Fatalf("store flags not implemented: %v", v.LongString());
                    return;
                }
                r = v.Args[0L].Reg();
                if (isHILO(r))
                { 
                    // cannot directly store, move to TMP and store
                    p = s.Prog(mips.AMOVW);
                    p.From.Type = obj.TYPE_REG;
                    p.From.Reg = r;
                    p.To.Type = obj.TYPE_REG;
                    p.To.Reg = mips.REGTMP;
                    r = mips.REGTMP;
                }
                p = s.Prog(storeByType(v.Type, r));
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = r;
                gc.AddrAuto(ref p.To, v);
                goto __switch_break0;
            }
            if (v.Op == ssa.OpMIPSADD || v.Op == ssa.OpMIPSSUB || v.Op == ssa.OpMIPSAND || v.Op == ssa.OpMIPSOR || v.Op == ssa.OpMIPSXOR || v.Op == ssa.OpMIPSNOR || v.Op == ssa.OpMIPSSLL || v.Op == ssa.OpMIPSSRL || v.Op == ssa.OpMIPSSRA || v.Op == ssa.OpMIPSADDF || v.Op == ssa.OpMIPSADDD || v.Op == ssa.OpMIPSSUBF || v.Op == ssa.OpMIPSSUBD || v.Op == ssa.OpMIPSMULF || v.Op == ssa.OpMIPSMULD || v.Op == ssa.OpMIPSDIVF || v.Op == ssa.OpMIPSDIVD || v.Op == ssa.OpMIPSMUL)
            {
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = v.Args[1L].Reg();
                p.Reg = v.Args[0L].Reg();
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
                goto __switch_break0;
            }
            if (v.Op == ssa.OpMIPSSGT || v.Op == ssa.OpMIPSSGTU)
            {
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = v.Args[0L].Reg();
                p.Reg = v.Args[1L].Reg();
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
                goto __switch_break0;
            }
            if (v.Op == ssa.OpMIPSSGTzero || v.Op == ssa.OpMIPSSGTUzero)
            {
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = v.Args[0L].Reg();
                p.Reg = mips.REGZERO;
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
                goto __switch_break0;
            }
            if (v.Op == ssa.OpMIPSADDconst || v.Op == ssa.OpMIPSSUBconst || v.Op == ssa.OpMIPSANDconst || v.Op == ssa.OpMIPSORconst || v.Op == ssa.OpMIPSXORconst || v.Op == ssa.OpMIPSNORconst || v.Op == ssa.OpMIPSSLLconst || v.Op == ssa.OpMIPSSRLconst || v.Op == ssa.OpMIPSSRAconst || v.Op == ssa.OpMIPSSGTconst || v.Op == ssa.OpMIPSSGTUconst)
            {
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_CONST;
                p.From.Offset = v.AuxInt;
                p.Reg = v.Args[0L].Reg();
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
                goto __switch_break0;
            }
            if (v.Op == ssa.OpMIPSMULT || v.Op == ssa.OpMIPSMULTU || v.Op == ssa.OpMIPSDIV || v.Op == ssa.OpMIPSDIVU) 
            {
                // result in hi,lo
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = v.Args[1L].Reg();
                p.Reg = v.Args[0L].Reg();
                goto __switch_break0;
            }
            if (v.Op == ssa.OpMIPSMOVWconst)
            {
                r = v.Reg();
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_CONST;
                p.From.Offset = v.AuxInt;
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = r;
                if (isFPreg(r) || isHILO(r))
                { 
                    // cannot move into FP or special registers, use TMP as intermediate
                    p.To.Reg = mips.REGTMP;
                    p = s.Prog(mips.AMOVW);
                    p.From.Type = obj.TYPE_REG;
                    p.From.Reg = mips.REGTMP;
                    p.To.Type = obj.TYPE_REG;
                    p.To.Reg = r;
                }
                goto __switch_break0;
            }
            if (v.Op == ssa.OpMIPSMOVFconst || v.Op == ssa.OpMIPSMOVDconst)
            {
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_FCONST;
                p.From.Val = math.Float64frombits(uint64(v.AuxInt));
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
                goto __switch_break0;
            }
            if (v.Op == ssa.OpMIPSCMOVZ)
            {
                if (v.Reg() != v.Args[0L].Reg())
                {
                    v.Fatalf("input[0] and output not in same register %s", v.LongString());
                }
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = v.Args[2L].Reg();
                p.Reg = v.Args[1L].Reg();
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
                goto __switch_break0;
            }
            if (v.Op == ssa.OpMIPSCMOVZzero)
            {
                if (v.Reg() != v.Args[0L].Reg())
                {
                    v.Fatalf("input[0] and output not in same register %s", v.LongString());
                }
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = v.Args[1L].Reg();
                p.Reg = mips.REGZERO;
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
                goto __switch_break0;
            }
            if (v.Op == ssa.OpMIPSCMPEQF || v.Op == ssa.OpMIPSCMPEQD || v.Op == ssa.OpMIPSCMPGEF || v.Op == ssa.OpMIPSCMPGED || v.Op == ssa.OpMIPSCMPGTF || v.Op == ssa.OpMIPSCMPGTD)
            {
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = v.Args[0L].Reg();
                p.Reg = v.Args[1L].Reg();
                goto __switch_break0;
            }
            if (v.Op == ssa.OpMIPSMOVWaddr)
            {
                p = s.Prog(mips.AMOVW);
                p.From.Type = obj.TYPE_ADDR;
                p.From.Reg = v.Args[0L].Reg();
                @string wantreg = default; 
                // MOVW $sym+off(base), R
                // the assembler expands it as the following:
                // - base is SP: add constant offset to SP (R29)
                //               when constant is large, tmp register (R23) may be used
                // - base is SB: load external address with relocation
                switch (v.Aux.type())
                {
                    case ref obj.LSym _:
                        wantreg = "SB";
                        gc.AddAux(ref p.From, v);
                        break;
                    case ref gc.Node _:
                        wantreg = "SP";
                        gc.AddAux(ref p.From, v);
                        break;
                    case 
                        wantreg = "SP";
                        p.From.Offset = v.AuxInt;
                        break;
                    default:
                    {
                        v.Fatalf("aux is of unknown type %T", v.Aux);
                        break;
                    }
                }
                {
                    var reg = v.Args[0L].RegName();

                    if (reg != wantreg)
                    {
                        v.Fatalf("bad reg %s for symbol type %T, want %s", reg, v.Aux, wantreg);
                    }

                }
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
                goto __switch_break0;
            }
            if (v.Op == ssa.OpMIPSMOVBload || v.Op == ssa.OpMIPSMOVBUload || v.Op == ssa.OpMIPSMOVHload || v.Op == ssa.OpMIPSMOVHUload || v.Op == ssa.OpMIPSMOVWload || v.Op == ssa.OpMIPSMOVFload || v.Op == ssa.OpMIPSMOVDload)
            {
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_MEM;
                p.From.Reg = v.Args[0L].Reg();
                gc.AddAux(ref p.From, v);
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
                goto __switch_break0;
            }
            if (v.Op == ssa.OpMIPSMOVBstore || v.Op == ssa.OpMIPSMOVHstore || v.Op == ssa.OpMIPSMOVWstore || v.Op == ssa.OpMIPSMOVFstore || v.Op == ssa.OpMIPSMOVDstore)
            {
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = v.Args[1L].Reg();
                p.To.Type = obj.TYPE_MEM;
                p.To.Reg = v.Args[0L].Reg();
                gc.AddAux(ref p.To, v);
                goto __switch_break0;
            }
            if (v.Op == ssa.OpMIPSMOVBstorezero || v.Op == ssa.OpMIPSMOVHstorezero || v.Op == ssa.OpMIPSMOVWstorezero)
            {
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = mips.REGZERO;
                p.To.Type = obj.TYPE_MEM;
                p.To.Reg = v.Args[0L].Reg();
                gc.AddAux(ref p.To, v);
                goto __switch_break0;
            }
            if (v.Op == ssa.OpMIPSMOVBreg || v.Op == ssa.OpMIPSMOVBUreg || v.Op == ssa.OpMIPSMOVHreg || v.Op == ssa.OpMIPSMOVHUreg)
            {
                var a = v.Args[0L];
                while (a.Op == ssa.OpCopy || a.Op == ssa.OpMIPSMOVWreg || a.Op == ssa.OpMIPSMOVWnop)
                {
                    a = a.Args[0L];
                }

                if (a.Op == ssa.OpLoadReg)
                {
                    t = a.Type;

                    if (v.Op == ssa.OpMIPSMOVBreg && t.Size() == 1L && t.IsSigned() || v.Op == ssa.OpMIPSMOVBUreg && t.Size() == 1L && !t.IsSigned() || v.Op == ssa.OpMIPSMOVHreg && t.Size() == 2L && t.IsSigned() || v.Op == ssa.OpMIPSMOVHUreg && t.Size() == 2L && !t.IsSigned()) 
                        // arg is a proper-typed load, already zero/sign-extended, don't extend again
                        if (v.Reg() == v.Args[0L].Reg())
                        {
                            return;
                        }
                        p = s.Prog(mips.AMOVW);
                        p.From.Type = obj.TYPE_REG;
                        p.From.Reg = v.Args[0L].Reg();
                        p.To.Type = obj.TYPE_REG;
                        p.To.Reg = v.Reg();
                        return;
                    else                     
                }
                fallthrough = true;
            }
            if (fallthrough || v.Op == ssa.OpMIPSMOVWF || v.Op == ssa.OpMIPSMOVWD || v.Op == ssa.OpMIPSTRUNCFW || v.Op == ssa.OpMIPSTRUNCDW || v.Op == ssa.OpMIPSMOVFD || v.Op == ssa.OpMIPSMOVDF || v.Op == ssa.OpMIPSNEGF || v.Op == ssa.OpMIPSNEGD || v.Op == ssa.OpMIPSSQRTD || v.Op == ssa.OpMIPSCLZ)
            {
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = v.Args[0L].Reg();
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
                goto __switch_break0;
            }
            if (v.Op == ssa.OpMIPSNEG) 
            {
                // SUB from REGZERO
                p = s.Prog(mips.ASUBU);
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = v.Args[0L].Reg();
                p.Reg = mips.REGZERO;
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
                goto __switch_break0;
            }
            if (v.Op == ssa.OpMIPSLoweredZero) 
            {
                // SUBU    $4, R1
                // MOVW    R0, 4(R1)
                // ADDU    $4, R1
                // BNE    Rarg1, R1, -2(PC)
                // arg1 is the address of the last element to zero
                long sz = default;
                obj.As mov = default;

                if (v.AuxInt % 4L == 0L) 
                    sz = 4L;
                    mov = mips.AMOVW;
                else if (v.AuxInt % 2L == 0L) 
                    sz = 2L;
                    mov = mips.AMOVH;
                else 
                    sz = 1L;
                    mov = mips.AMOVB;
                                p = s.Prog(mips.ASUBU);
                p.From.Type = obj.TYPE_CONST;
                p.From.Offset = sz;
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = mips.REG_R1;
                var p2 = s.Prog(mov);
                p2.From.Type = obj.TYPE_REG;
                p2.From.Reg = mips.REGZERO;
                p2.To.Type = obj.TYPE_MEM;
                p2.To.Reg = mips.REG_R1;
                p2.To.Offset = sz;
                var p3 = s.Prog(mips.AADDU);
                p3.From.Type = obj.TYPE_CONST;
                p3.From.Offset = sz;
                p3.To.Type = obj.TYPE_REG;
                p3.To.Reg = mips.REG_R1;
                var p4 = s.Prog(mips.ABNE);
                p4.From.Type = obj.TYPE_REG;
                p4.From.Reg = v.Args[1L].Reg();
                p4.Reg = mips.REG_R1;
                p4.To.Type = obj.TYPE_BRANCH;
                gc.Patch(p4, p2);
                goto __switch_break0;
            }
            if (v.Op == ssa.OpMIPSLoweredMove) 
            {
                // SUBU    $4, R1
                // MOVW    4(R1), Rtmp
                // MOVW    Rtmp, (R2)
                // ADDU    $4, R1
                // ADDU    $4, R2
                // BNE    Rarg2, R1, -4(PC)
                // arg2 is the address of the last element of src
                sz = default;
                mov = default;

                if (v.AuxInt % 4L == 0L) 
                    sz = 4L;
                    mov = mips.AMOVW;
                else if (v.AuxInt % 2L == 0L) 
                    sz = 2L;
                    mov = mips.AMOVH;
                else 
                    sz = 1L;
                    mov = mips.AMOVB;
                                p = s.Prog(mips.ASUBU);
                p.From.Type = obj.TYPE_CONST;
                p.From.Offset = sz;
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = mips.REG_R1;
                p2 = s.Prog(mov);
                p2.From.Type = obj.TYPE_MEM;
                p2.From.Reg = mips.REG_R1;
                p2.From.Offset = sz;
                p2.To.Type = obj.TYPE_REG;
                p2.To.Reg = mips.REGTMP;
                p3 = s.Prog(mov);
                p3.From.Type = obj.TYPE_REG;
                p3.From.Reg = mips.REGTMP;
                p3.To.Type = obj.TYPE_MEM;
                p3.To.Reg = mips.REG_R2;
                p4 = s.Prog(mips.AADDU);
                p4.From.Type = obj.TYPE_CONST;
                p4.From.Offset = sz;
                p4.To.Type = obj.TYPE_REG;
                p4.To.Reg = mips.REG_R1;
                var p5 = s.Prog(mips.AADDU);
                p5.From.Type = obj.TYPE_CONST;
                p5.From.Offset = sz;
                p5.To.Type = obj.TYPE_REG;
                p5.To.Reg = mips.REG_R2;
                var p6 = s.Prog(mips.ABNE);
                p6.From.Type = obj.TYPE_REG;
                p6.From.Reg = v.Args[2L].Reg();
                p6.Reg = mips.REG_R1;
                p6.To.Type = obj.TYPE_BRANCH;
                gc.Patch(p6, p2);
                goto __switch_break0;
            }
            if (v.Op == ssa.OpMIPSCALLstatic || v.Op == ssa.OpMIPSCALLclosure || v.Op == ssa.OpMIPSCALLinter)
            {
                s.Call(v);
                goto __switch_break0;
            }
            if (v.Op == ssa.OpMIPSLoweredAtomicLoad)
            {
                s.Prog(mips.ASYNC);

                p = s.Prog(mips.AMOVW);
                p.From.Type = obj.TYPE_MEM;
                p.From.Reg = v.Args[0L].Reg();
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg0();

                s.Prog(mips.ASYNC);
                goto __switch_break0;
            }
            if (v.Op == ssa.OpMIPSLoweredAtomicStore)
            {
                s.Prog(mips.ASYNC);

                p = s.Prog(mips.AMOVW);
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = v.Args[1L].Reg();
                p.To.Type = obj.TYPE_MEM;
                p.To.Reg = v.Args[0L].Reg();

                s.Prog(mips.ASYNC);
                goto __switch_break0;
            }
            if (v.Op == ssa.OpMIPSLoweredAtomicStorezero)
            {
                s.Prog(mips.ASYNC);

                p = s.Prog(mips.AMOVW);
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = mips.REGZERO;
                p.To.Type = obj.TYPE_MEM;
                p.To.Reg = v.Args[0L].Reg();

                s.Prog(mips.ASYNC);
                goto __switch_break0;
            }
            if (v.Op == ssa.OpMIPSLoweredAtomicExchange) 
            {
                // SYNC
                // MOVW Rarg1, Rtmp
                // LL    (Rarg0), Rout
                // SC    Rtmp, (Rarg0)
                // BEQ    Rtmp, -3(PC)
                // SYNC
                s.Prog(mips.ASYNC);

                p = s.Prog(mips.AMOVW);
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = v.Args[1L].Reg();
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = mips.REGTMP;

                var p1 = s.Prog(mips.ALL);
                p1.From.Type = obj.TYPE_MEM;
                p1.From.Reg = v.Args[0L].Reg();
                p1.To.Type = obj.TYPE_REG;
                p1.To.Reg = v.Reg0();

                p2 = s.Prog(mips.ASC);
                p2.From.Type = obj.TYPE_REG;
                p2.From.Reg = mips.REGTMP;
                p2.To.Type = obj.TYPE_MEM;
                p2.To.Reg = v.Args[0L].Reg();

                p3 = s.Prog(mips.ABEQ);
                p3.From.Type = obj.TYPE_REG;
                p3.From.Reg = mips.REGTMP;
                p3.To.Type = obj.TYPE_BRANCH;
                gc.Patch(p3, p);

                s.Prog(mips.ASYNC);
                goto __switch_break0;
            }
            if (v.Op == ssa.OpMIPSLoweredAtomicAdd) 
            {
                // SYNC
                // LL    (Rarg0), Rout
                // ADDU Rarg1, Rout, Rtmp
                // SC    Rtmp, (Rarg0)
                // BEQ    Rtmp, -3(PC)
                // SYNC
                // ADDU Rarg1, Rout
                s.Prog(mips.ASYNC);

                p = s.Prog(mips.ALL);
                p.From.Type = obj.TYPE_MEM;
                p.From.Reg = v.Args[0L].Reg();
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg0();

                p1 = s.Prog(mips.AADDU);
                p1.From.Type = obj.TYPE_REG;
                p1.From.Reg = v.Args[1L].Reg();
                p1.Reg = v.Reg0();
                p1.To.Type = obj.TYPE_REG;
                p1.To.Reg = mips.REGTMP;

                p2 = s.Prog(mips.ASC);
                p2.From.Type = obj.TYPE_REG;
                p2.From.Reg = mips.REGTMP;
                p2.To.Type = obj.TYPE_MEM;
                p2.To.Reg = v.Args[0L].Reg();

                p3 = s.Prog(mips.ABEQ);
                p3.From.Type = obj.TYPE_REG;
                p3.From.Reg = mips.REGTMP;
                p3.To.Type = obj.TYPE_BRANCH;
                gc.Patch(p3, p);

                s.Prog(mips.ASYNC);

                p4 = s.Prog(mips.AADDU);
                p4.From.Type = obj.TYPE_REG;
                p4.From.Reg = v.Args[1L].Reg();
                p4.Reg = v.Reg0();
                p4.To.Type = obj.TYPE_REG;
                p4.To.Reg = v.Reg0();
                goto __switch_break0;
            }
            if (v.Op == ssa.OpMIPSLoweredAtomicAddconst) 
            {
                // SYNC
                // LL    (Rarg0), Rout
                // ADDU $auxInt, Rout, Rtmp
                // SC    Rtmp, (Rarg0)
                // BEQ    Rtmp, -3(PC)
                // SYNC
                // ADDU $auxInt, Rout
                s.Prog(mips.ASYNC);

                p = s.Prog(mips.ALL);
                p.From.Type = obj.TYPE_MEM;
                p.From.Reg = v.Args[0L].Reg();
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg0();

                p1 = s.Prog(mips.AADDU);
                p1.From.Type = obj.TYPE_CONST;
                p1.From.Offset = v.AuxInt;
                p1.Reg = v.Reg0();
                p1.To.Type = obj.TYPE_REG;
                p1.To.Reg = mips.REGTMP;

                p2 = s.Prog(mips.ASC);
                p2.From.Type = obj.TYPE_REG;
                p2.From.Reg = mips.REGTMP;
                p2.To.Type = obj.TYPE_MEM;
                p2.To.Reg = v.Args[0L].Reg();

                p3 = s.Prog(mips.ABEQ);
                p3.From.Type = obj.TYPE_REG;
                p3.From.Reg = mips.REGTMP;
                p3.To.Type = obj.TYPE_BRANCH;
                gc.Patch(p3, p);

                s.Prog(mips.ASYNC);

                p4 = s.Prog(mips.AADDU);
                p4.From.Type = obj.TYPE_CONST;
                p4.From.Offset = v.AuxInt;
                p4.Reg = v.Reg0();
                p4.To.Type = obj.TYPE_REG;
                p4.To.Reg = v.Reg0();
                goto __switch_break0;
            }
            if (v.Op == ssa.OpMIPSLoweredAtomicAnd || v.Op == ssa.OpMIPSLoweredAtomicOr) 
            {
                // SYNC
                // LL    (Rarg0), Rtmp
                // AND/OR    Rarg1, Rtmp
                // SC    Rtmp, (Rarg0)
                // BEQ    Rtmp, -3(PC)
                // SYNC
                s.Prog(mips.ASYNC);

                p = s.Prog(mips.ALL);
                p.From.Type = obj.TYPE_MEM;
                p.From.Reg = v.Args[0L].Reg();
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = mips.REGTMP;

                p1 = s.Prog(v.Op.Asm());
                p1.From.Type = obj.TYPE_REG;
                p1.From.Reg = v.Args[1L].Reg();
                p1.Reg = mips.REGTMP;
                p1.To.Type = obj.TYPE_REG;
                p1.To.Reg = mips.REGTMP;

                p2 = s.Prog(mips.ASC);
                p2.From.Type = obj.TYPE_REG;
                p2.From.Reg = mips.REGTMP;
                p2.To.Type = obj.TYPE_MEM;
                p2.To.Reg = v.Args[0L].Reg();

                p3 = s.Prog(mips.ABEQ);
                p3.From.Type = obj.TYPE_REG;
                p3.From.Reg = mips.REGTMP;
                p3.To.Type = obj.TYPE_BRANCH;
                gc.Patch(p3, p);

                s.Prog(mips.ASYNC);
                goto __switch_break0;
            }
            if (v.Op == ssa.OpMIPSLoweredAtomicCas) 
            {
                // MOVW $0, Rout
                // SYNC
                // LL    (Rarg0), Rtmp
                // BNE    Rtmp, Rarg1, 4(PC)
                // MOVW Rarg2, Rout
                // SC    Rout, (Rarg0)
                // BEQ    Rout, -4(PC)
                // SYNC
                p = s.Prog(mips.AMOVW);
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = mips.REGZERO;
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg0();

                s.Prog(mips.ASYNC);

                p1 = s.Prog(mips.ALL);
                p1.From.Type = obj.TYPE_MEM;
                p1.From.Reg = v.Args[0L].Reg();
                p1.To.Type = obj.TYPE_REG;
                p1.To.Reg = mips.REGTMP;

                p2 = s.Prog(mips.ABNE);
                p2.From.Type = obj.TYPE_REG;
                p2.From.Reg = v.Args[1L].Reg();
                p2.Reg = mips.REGTMP;
                p2.To.Type = obj.TYPE_BRANCH;

                p3 = s.Prog(mips.AMOVW);
                p3.From.Type = obj.TYPE_REG;
                p3.From.Reg = v.Args[2L].Reg();
                p3.To.Type = obj.TYPE_REG;
                p3.To.Reg = v.Reg0();

                p4 = s.Prog(mips.ASC);
                p4.From.Type = obj.TYPE_REG;
                p4.From.Reg = v.Reg0();
                p4.To.Type = obj.TYPE_MEM;
                p4.To.Reg = v.Args[0L].Reg();

                p5 = s.Prog(mips.ABEQ);
                p5.From.Type = obj.TYPE_REG;
                p5.From.Reg = v.Reg0();
                p5.To.Type = obj.TYPE_BRANCH;
                gc.Patch(p5, p1);

                s.Prog(mips.ASYNC);

                p6 = s.Prog(obj.ANOP);
                gc.Patch(p2, p6);
                goto __switch_break0;
            }
            if (v.Op == ssa.OpMIPSLoweredNilCheck) 
            {
                // Issue a load which will fault if arg is nil.
                p = s.Prog(mips.AMOVB);
                p.From.Type = obj.TYPE_MEM;
                p.From.Reg = v.Args[0L].Reg();
                gc.AddAux(ref p.From, v);
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = mips.REGTMP;
                if (gc.Debug_checknil != 0L && v.Pos.Line() > 1L)
                { // v.Pos.Line()==1 in generated wrappers
                    gc.Warnl(v.Pos, "generated nil check");
                }
                goto __switch_break0;
            }
            if (v.Op == ssa.OpMIPSFPFlagTrue || v.Op == ssa.OpMIPSFPFlagFalse) 
            {
                // MOVW        $1, r
                // CMOVF    R0, r

                var cmov = mips.ACMOVF;
                if (v.Op == ssa.OpMIPSFPFlagFalse)
                {
                    cmov = mips.ACMOVT;
                }
                p = s.Prog(mips.AMOVW);
                p.From.Type = obj.TYPE_CONST;
                p.From.Offset = 1L;
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
                p1 = s.Prog(cmov);
                p1.From.Type = obj.TYPE_REG;
                p1.From.Reg = mips.REGZERO;
                p1.To.Type = obj.TYPE_REG;
                p1.To.Reg = v.Reg();
                goto __switch_break0;
            }
            if (v.Op == ssa.OpMIPSLoweredGetClosurePtr) 
            {
                // Closure pointer is R22 (mips.REGCTXT).
                gc.CheckLoweredGetClosurePtr(v);
                goto __switch_break0;
            }
            if (v.Op == ssa.OpMIPSLoweredGetCallerSP) 
            {
                // caller's SP is FixedFrameSize below the address of the first arg
                p = s.Prog(mips.AMOVW);
                p.From.Type = obj.TYPE_ADDR;
                p.From.Offset = -gc.Ctxt.FixedFrameSize();
                p.From.Name = obj.NAME_PARAM;
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
                goto __switch_break0;
            }
            if (v.Op == ssa.OpClobber)
            {
                goto __switch_break0;
            }
            // default: 
                v.Fatalf("genValue not implemented: %s", v.LongString());

            __switch_break0:;
        }



        private static void ssaGenBlock(ref gc.SSAGenState s, ref ssa.Block b, ref ssa.Block next)
        {

            if (b.Kind == ssa.BlockPlain) 
                if (b.Succs[0L].Block() != next)
                {
                    var p = s.Prog(obj.AJMP);
                    p.To.Type = obj.TYPE_BRANCH;
                    s.Branches = append(s.Branches, new gc.Branch(P:p,B:b.Succs[0].Block()));
                }
            else if (b.Kind == ssa.BlockDefer) 
                // defer returns in R1:
                // 0 if we should continue executing
                // 1 if we should jump to deferreturn call
                p = s.Prog(mips.ABNE);
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = mips.REGZERO;
                p.Reg = mips.REG_R1;
                p.To.Type = obj.TYPE_BRANCH;
                s.Branches = append(s.Branches, new gc.Branch(P:p,B:b.Succs[1].Block()));
                if (b.Succs[0L].Block() != next)
                {
                    p = s.Prog(obj.AJMP);
                    p.To.Type = obj.TYPE_BRANCH;
                    s.Branches = append(s.Branches, new gc.Branch(P:p,B:b.Succs[0].Block()));
                }
            else if (b.Kind == ssa.BlockExit) 
                s.Prog(obj.AUNDEF); // tell plive.go that we never reach here
            else if (b.Kind == ssa.BlockRet) 
                s.Prog(obj.ARET);
            else if (b.Kind == ssa.BlockRetJmp) 
                p = s.Prog(obj.ARET);
                p.To.Type = obj.TYPE_MEM;
                p.To.Name = obj.NAME_EXTERN;
                p.To.Sym = b.Aux._<ref obj.LSym>();
            else if (b.Kind == ssa.BlockMIPSEQ || b.Kind == ssa.BlockMIPSNE || b.Kind == ssa.BlockMIPSLTZ || b.Kind == ssa.BlockMIPSGEZ || b.Kind == ssa.BlockMIPSLEZ || b.Kind == ssa.BlockMIPSGTZ || b.Kind == ssa.BlockMIPSFPT || b.Kind == ssa.BlockMIPSFPF) 
                var jmp = blockJump[b.Kind];
                p = default;

                if (next == b.Succs[0L].Block()) 
                    p = s.Prog(jmp.invasm);
                    p.To.Type = obj.TYPE_BRANCH;
                    s.Branches = append(s.Branches, new gc.Branch(P:p,B:b.Succs[1].Block()));
                else if (next == b.Succs[1L].Block()) 
                    p = s.Prog(jmp.asm);
                    p.To.Type = obj.TYPE_BRANCH;
                    s.Branches = append(s.Branches, new gc.Branch(P:p,B:b.Succs[0].Block()));
                else 
                    p = s.Prog(jmp.asm);
                    p.To.Type = obj.TYPE_BRANCH;
                    s.Branches = append(s.Branches, new gc.Branch(P:p,B:b.Succs[0].Block()));
                    var q = s.Prog(obj.AJMP);
                    q.To.Type = obj.TYPE_BRANCH;
                    s.Branches = append(s.Branches, new gc.Branch(P:q,B:b.Succs[1].Block()));
                                if (!b.Control.Type.IsFlags())
                {
                    p.From.Type = obj.TYPE_REG;
                    p.From.Reg = b.Control.Reg();
                }
            else 
                b.Fatalf("branch not implemented: %s. Control: %s", b.LongString(), b.Control.LongString());
                    }
    }
}}}}
