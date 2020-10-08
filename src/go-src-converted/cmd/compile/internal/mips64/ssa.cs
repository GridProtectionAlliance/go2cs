// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package mips64 -- go2cs converted at 2020 October 08 04:27:47 UTC
// import "cmd/compile/internal/mips64" ==> using mips64 = go.cmd.compile.@internal.mips64_package
// Original source: C:\Go\src\cmd\compile\internal\mips64\ssa.go
using math = go.math_package;

using gc = go.cmd.compile.@internal.gc_package;
using logopt = go.cmd.compile.@internal.logopt_package;
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
    public static partial class mips64_package
    {
        // isFPreg reports whether r is an FP register
        private static bool isFPreg(short r)
        {
            return mips.REG_F0 <= r && r <= mips.REG_F31;
        }

        // isHILO reports whether r is HI or LO register
        private static bool isHILO(short r)
        {
            return r == mips.REG_HI || r == mips.REG_LO;
        }

        // loadByType returns the load instruction of the given type.
        private static obj.As loadByType(ptr<types.Type> _addr_t, short r) => func((_, panic, __) =>
        {
            ref types.Type t = ref _addr_t.val;

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
                        if (t.IsSigned())
                        {
                            return mips.AMOVW;
                        }
                        else
                        {
                            return mips.AMOVWU;
                        }

                        break;
                    case 8L: 
                        return mips.AMOVV;
                        break;
                }

            }

            panic("bad load type");

        });

        // storeByType returns the store instruction of the given type.
        private static obj.As storeByType(ptr<types.Type> _addr_t, short r) => func((_, panic, __) =>
        {
            ref types.Type t = ref _addr_t.val;

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
                    case 8L: 
                        return mips.AMOVV;
                        break;
                }

            }

            panic("bad store type");

        });

        private static void ssaGenValue(ptr<gc.SSAGenState> _addr_s, ptr<ssa.Value> _addr_v)
        {
            ref gc.SSAGenState s = ref _addr_s.val;
            ref ssa.Value v = ref _addr_v.val;


            if (v.Op == ssa.OpCopy || v.Op == ssa.OpMIPS64MOVVreg)
            {
                if (v.Type.IsMemory())
                {
                    return ;
                }

                var x = v.Args[0L].Reg();
                var y = v.Reg();
                if (x == y)
                {
                    return ;
                }

                var @as = mips.AMOVV;
                if (isFPreg(x) && isFPreg(y))
                {
                    as = mips.AMOVD;
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
                    p = s.Prog(mips.AMOVV);
                    p.From.Type = obj.TYPE_REG;
                    p.From.Reg = mips.REGTMP;
                    p.To.Type = obj.TYPE_REG;
                    p.To.Reg = y;

                }

                goto __switch_break0;
            }
            if (v.Op == ssa.OpMIPS64MOVVnop)
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
                    return ;
                }

                var r = v.Reg();
                p = s.Prog(loadByType(_addr_v.Type, r));
                gc.AddrAuto(_addr_p.From, v.Args[0L]);
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = r;
                if (isHILO(r))
                { 
                    // cannot directly load, load to TMP and move
                    p.To.Reg = mips.REGTMP;
                    p = s.Prog(mips.AMOVV);
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
                    return ;
                }

                r = v.Args[0L].Reg();
                if (isHILO(r))
                { 
                    // cannot directly store, move to TMP and store
                    p = s.Prog(mips.AMOVV);
                    p.From.Type = obj.TYPE_REG;
                    p.From.Reg = r;
                    p.To.Type = obj.TYPE_REG;
                    p.To.Reg = mips.REGTMP;
                    r = mips.REGTMP;

                }

                p = s.Prog(storeByType(_addr_v.Type, r));
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = r;
                gc.AddrAuto(_addr_p.To, v);
                goto __switch_break0;
            }
            if (v.Op == ssa.OpMIPS64ADDV || v.Op == ssa.OpMIPS64SUBV || v.Op == ssa.OpMIPS64AND || v.Op == ssa.OpMIPS64OR || v.Op == ssa.OpMIPS64XOR || v.Op == ssa.OpMIPS64NOR || v.Op == ssa.OpMIPS64SLLV || v.Op == ssa.OpMIPS64SRLV || v.Op == ssa.OpMIPS64SRAV || v.Op == ssa.OpMIPS64ADDF || v.Op == ssa.OpMIPS64ADDD || v.Op == ssa.OpMIPS64SUBF || v.Op == ssa.OpMIPS64SUBD || v.Op == ssa.OpMIPS64MULF || v.Op == ssa.OpMIPS64MULD || v.Op == ssa.OpMIPS64DIVF || v.Op == ssa.OpMIPS64DIVD)
            {
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = v.Args[1L].Reg();
                p.Reg = v.Args[0L].Reg();
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
                goto __switch_break0;
            }
            if (v.Op == ssa.OpMIPS64SGT || v.Op == ssa.OpMIPS64SGTU)
            {
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = v.Args[0L].Reg();
                p.Reg = v.Args[1L].Reg();
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
                goto __switch_break0;
            }
            if (v.Op == ssa.OpMIPS64ADDVconst || v.Op == ssa.OpMIPS64SUBVconst || v.Op == ssa.OpMIPS64ANDconst || v.Op == ssa.OpMIPS64ORconst || v.Op == ssa.OpMIPS64XORconst || v.Op == ssa.OpMIPS64NORconst || v.Op == ssa.OpMIPS64SLLVconst || v.Op == ssa.OpMIPS64SRLVconst || v.Op == ssa.OpMIPS64SRAVconst || v.Op == ssa.OpMIPS64SGTconst || v.Op == ssa.OpMIPS64SGTUconst)
            {
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_CONST;
                p.From.Offset = v.AuxInt;
                p.Reg = v.Args[0L].Reg();
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
                goto __switch_break0;
            }
            if (v.Op == ssa.OpMIPS64MULV || v.Op == ssa.OpMIPS64MULVU || v.Op == ssa.OpMIPS64DIVV || v.Op == ssa.OpMIPS64DIVVU) 
            {
                // result in hi,lo
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = v.Args[1L].Reg();
                p.Reg = v.Args[0L].Reg();
                goto __switch_break0;
            }
            if (v.Op == ssa.OpMIPS64MOVVconst)
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
                    p = s.Prog(mips.AMOVV);
                    p.From.Type = obj.TYPE_REG;
                    p.From.Reg = mips.REGTMP;
                    p.To.Type = obj.TYPE_REG;
                    p.To.Reg = r;

                }

                goto __switch_break0;
            }
            if (v.Op == ssa.OpMIPS64MOVFconst || v.Op == ssa.OpMIPS64MOVDconst)
            {
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_FCONST;
                p.From.Val = math.Float64frombits(uint64(v.AuxInt));
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
                goto __switch_break0;
            }
            if (v.Op == ssa.OpMIPS64CMPEQF || v.Op == ssa.OpMIPS64CMPEQD || v.Op == ssa.OpMIPS64CMPGEF || v.Op == ssa.OpMIPS64CMPGED || v.Op == ssa.OpMIPS64CMPGTF || v.Op == ssa.OpMIPS64CMPGTD)
            {
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = v.Args[0L].Reg();
                p.Reg = v.Args[1L].Reg();
                goto __switch_break0;
            }
            if (v.Op == ssa.OpMIPS64MOVVaddr)
            {
                p = s.Prog(mips.AMOVV);
                p.From.Type = obj.TYPE_ADDR;
                p.From.Reg = v.Args[0L].Reg();
                @string wantreg = default; 
                // MOVV $sym+off(base), R
                // the assembler expands it as the following:
                // - base is SP: add constant offset to SP (R29)
                //               when constant is large, tmp register (R23) may be used
                // - base is SB: load external address with relocation
                switch (v.Aux.type())
                {
                    case ptr<obj.LSym> _:
                        wantreg = "SB";
                        gc.AddAux(_addr_p.From, v);
                        break;
                    case ptr<gc.Node> _:
                        wantreg = "SP";
                        gc.AddAux(_addr_p.From, v);
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
            if (v.Op == ssa.OpMIPS64MOVBload || v.Op == ssa.OpMIPS64MOVBUload || v.Op == ssa.OpMIPS64MOVHload || v.Op == ssa.OpMIPS64MOVHUload || v.Op == ssa.OpMIPS64MOVWload || v.Op == ssa.OpMIPS64MOVWUload || v.Op == ssa.OpMIPS64MOVVload || v.Op == ssa.OpMIPS64MOVFload || v.Op == ssa.OpMIPS64MOVDload)
            {
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_MEM;
                p.From.Reg = v.Args[0L].Reg();
                gc.AddAux(_addr_p.From, v);
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
                goto __switch_break0;
            }
            if (v.Op == ssa.OpMIPS64MOVBstore || v.Op == ssa.OpMIPS64MOVHstore || v.Op == ssa.OpMIPS64MOVWstore || v.Op == ssa.OpMIPS64MOVVstore || v.Op == ssa.OpMIPS64MOVFstore || v.Op == ssa.OpMIPS64MOVDstore)
            {
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = v.Args[1L].Reg();
                p.To.Type = obj.TYPE_MEM;
                p.To.Reg = v.Args[0L].Reg();
                gc.AddAux(_addr_p.To, v);
                goto __switch_break0;
            }
            if (v.Op == ssa.OpMIPS64MOVBstorezero || v.Op == ssa.OpMIPS64MOVHstorezero || v.Op == ssa.OpMIPS64MOVWstorezero || v.Op == ssa.OpMIPS64MOVVstorezero)
            {
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = mips.REGZERO;
                p.To.Type = obj.TYPE_MEM;
                p.To.Reg = v.Args[0L].Reg();
                gc.AddAux(_addr_p.To, v);
                goto __switch_break0;
            }
            if (v.Op == ssa.OpMIPS64MOVBreg || v.Op == ssa.OpMIPS64MOVBUreg || v.Op == ssa.OpMIPS64MOVHreg || v.Op == ssa.OpMIPS64MOVHUreg || v.Op == ssa.OpMIPS64MOVWreg || v.Op == ssa.OpMIPS64MOVWUreg)
            {
                var a = v.Args[0L];
                while (a.Op == ssa.OpCopy || a.Op == ssa.OpMIPS64MOVVreg)
                {
                    a = a.Args[0L];
                }

                if (a.Op == ssa.OpLoadReg)
                {
                    var t = a.Type;

                    if (v.Op == ssa.OpMIPS64MOVBreg && t.Size() == 1L && t.IsSigned() || v.Op == ssa.OpMIPS64MOVBUreg && t.Size() == 1L && !t.IsSigned() || v.Op == ssa.OpMIPS64MOVHreg && t.Size() == 2L && t.IsSigned() || v.Op == ssa.OpMIPS64MOVHUreg && t.Size() == 2L && !t.IsSigned() || v.Op == ssa.OpMIPS64MOVWreg && t.Size() == 4L && t.IsSigned() || v.Op == ssa.OpMIPS64MOVWUreg && t.Size() == 4L && !t.IsSigned()) 
                        // arg is a proper-typed load, already zero/sign-extended, don't extend again
                        if (v.Reg() == v.Args[0L].Reg())
                        {
                            return ;
                        }

                        p = s.Prog(mips.AMOVV);
                        p.From.Type = obj.TYPE_REG;
                        p.From.Reg = v.Args[0L].Reg();
                        p.To.Type = obj.TYPE_REG;
                        p.To.Reg = v.Reg();
                        return ;
                    else                     
                }

                fallthrough = true;
            }
            if (fallthrough || v.Op == ssa.OpMIPS64MOVWF || v.Op == ssa.OpMIPS64MOVWD || v.Op == ssa.OpMIPS64TRUNCFW || v.Op == ssa.OpMIPS64TRUNCDW || v.Op == ssa.OpMIPS64MOVVF || v.Op == ssa.OpMIPS64MOVVD || v.Op == ssa.OpMIPS64TRUNCFV || v.Op == ssa.OpMIPS64TRUNCDV || v.Op == ssa.OpMIPS64MOVFD || v.Op == ssa.OpMIPS64MOVDF || v.Op == ssa.OpMIPS64NEGF || v.Op == ssa.OpMIPS64NEGD || v.Op == ssa.OpMIPS64SQRTD)
            {
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = v.Args[0L].Reg();
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
                goto __switch_break0;
            }
            if (v.Op == ssa.OpMIPS64NEGV) 
            {
                // SUB from REGZERO
                p = s.Prog(mips.ASUBVU);
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = v.Args[0L].Reg();
                p.Reg = mips.REGZERO;
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
                goto __switch_break0;
            }
            if (v.Op == ssa.OpMIPS64DUFFZERO) 
            {
                // runtime.duffzero expects start address - 8 in R1
                p = s.Prog(mips.ASUBVU);
                p.From.Type = obj.TYPE_CONST;
                p.From.Offset = 8L;
                p.Reg = v.Args[0L].Reg();
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = mips.REG_R1;
                p = s.Prog(obj.ADUFFZERO);
                p.To.Type = obj.TYPE_MEM;
                p.To.Name = obj.NAME_EXTERN;
                p.To.Sym = gc.Duffzero;
                p.To.Offset = v.AuxInt;
                goto __switch_break0;
            }
            if (v.Op == ssa.OpMIPS64LoweredZero) 
            {
                // SUBV    $8, R1
                // MOVV    R0, 8(R1)
                // ADDV    $8, R1
                // BNE    Rarg1, R1, -2(PC)
                // arg1 is the address of the last element to zero
                long sz = default;
                obj.As mov = default;

                if (v.AuxInt % 8L == 0L) 
                    sz = 8L;
                    mov = mips.AMOVV;
                else if (v.AuxInt % 4L == 0L) 
                    sz = 4L;
                    mov = mips.AMOVW;
                else if (v.AuxInt % 2L == 0L) 
                    sz = 2L;
                    mov = mips.AMOVH;
                else 
                    sz = 1L;
                    mov = mips.AMOVB;
                                p = s.Prog(mips.ASUBVU);
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
                var p3 = s.Prog(mips.AADDVU);
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
            if (v.Op == ssa.OpMIPS64DUFFCOPY)
            {
                p = s.Prog(obj.ADUFFCOPY);
                p.To.Type = obj.TYPE_MEM;
                p.To.Name = obj.NAME_EXTERN;
                p.To.Sym = gc.Duffcopy;
                p.To.Offset = v.AuxInt;
                goto __switch_break0;
            }
            if (v.Op == ssa.OpMIPS64LoweredMove) 
            {
                // SUBV    $8, R1
                // MOVV    8(R1), Rtmp
                // MOVV    Rtmp, (R2)
                // ADDV    $8, R1
                // ADDV    $8, R2
                // BNE    Rarg2, R1, -4(PC)
                // arg2 is the address of the last element of src
                sz = default;
                mov = default;

                if (v.AuxInt % 8L == 0L) 
                    sz = 8L;
                    mov = mips.AMOVV;
                else if (v.AuxInt % 4L == 0L) 
                    sz = 4L;
                    mov = mips.AMOVW;
                else if (v.AuxInt % 2L == 0L) 
                    sz = 2L;
                    mov = mips.AMOVH;
                else 
                    sz = 1L;
                    mov = mips.AMOVB;
                                p = s.Prog(mips.ASUBVU);
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
                p4 = s.Prog(mips.AADDVU);
                p4.From.Type = obj.TYPE_CONST;
                p4.From.Offset = sz;
                p4.To.Type = obj.TYPE_REG;
                p4.To.Reg = mips.REG_R1;
                var p5 = s.Prog(mips.AADDVU);
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
            if (v.Op == ssa.OpMIPS64CALLstatic || v.Op == ssa.OpMIPS64CALLclosure || v.Op == ssa.OpMIPS64CALLinter)
            {
                s.Call(v);
                goto __switch_break0;
            }
            if (v.Op == ssa.OpMIPS64LoweredWB)
            {
                p = s.Prog(obj.ACALL);
                p.To.Type = obj.TYPE_MEM;
                p.To.Name = obj.NAME_EXTERN;
                p.To.Sym = v.Aux._<ptr<obj.LSym>>();
                goto __switch_break0;
            }
            if (v.Op == ssa.OpMIPS64LoweredPanicBoundsA || v.Op == ssa.OpMIPS64LoweredPanicBoundsB || v.Op == ssa.OpMIPS64LoweredPanicBoundsC)
            {
                p = s.Prog(obj.ACALL);
                p.To.Type = obj.TYPE_MEM;
                p.To.Name = obj.NAME_EXTERN;
                p.To.Sym = gc.BoundsCheckFunc[v.AuxInt];
                s.UseArgs(16L); // space used in callee args area by assembly stubs
                goto __switch_break0;
            }
            if (v.Op == ssa.OpMIPS64LoweredAtomicLoad8 || v.Op == ssa.OpMIPS64LoweredAtomicLoad32 || v.Op == ssa.OpMIPS64LoweredAtomicLoad64)
            {
                @as = mips.AMOVV;

                if (v.Op == ssa.OpMIPS64LoweredAtomicLoad8) 
                    as = mips.AMOVB;
                else if (v.Op == ssa.OpMIPS64LoweredAtomicLoad32) 
                    as = mips.AMOVW;
                                s.Prog(mips.ASYNC);
                p = s.Prog(as);
                p.From.Type = obj.TYPE_MEM;
                p.From.Reg = v.Args[0L].Reg();
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg0();
                s.Prog(mips.ASYNC);
                goto __switch_break0;
            }
            if (v.Op == ssa.OpMIPS64LoweredAtomicStore8 || v.Op == ssa.OpMIPS64LoweredAtomicStore32 || v.Op == ssa.OpMIPS64LoweredAtomicStore64)
            {
                @as = mips.AMOVV;

                if (v.Op == ssa.OpMIPS64LoweredAtomicStore8) 
                    as = mips.AMOVB;
                else if (v.Op == ssa.OpMIPS64LoweredAtomicStore32) 
                    as = mips.AMOVW;
                                s.Prog(mips.ASYNC);
                p = s.Prog(as);
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = v.Args[1L].Reg();
                p.To.Type = obj.TYPE_MEM;
                p.To.Reg = v.Args[0L].Reg();
                s.Prog(mips.ASYNC);
                goto __switch_break0;
            }
            if (v.Op == ssa.OpMIPS64LoweredAtomicStorezero32 || v.Op == ssa.OpMIPS64LoweredAtomicStorezero64)
            {
                @as = mips.AMOVV;
                if (v.Op == ssa.OpMIPS64LoweredAtomicStorezero32)
                {
                    as = mips.AMOVW;
                }

                s.Prog(mips.ASYNC);
                p = s.Prog(as);
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = mips.REGZERO;
                p.To.Type = obj.TYPE_MEM;
                p.To.Reg = v.Args[0L].Reg();
                s.Prog(mips.ASYNC);
                goto __switch_break0;
            }
            if (v.Op == ssa.OpMIPS64LoweredAtomicExchange32 || v.Op == ssa.OpMIPS64LoweredAtomicExchange64) 
            {
                // SYNC
                // MOVV    Rarg1, Rtmp
                // LL    (Rarg0), Rout
                // SC    Rtmp, (Rarg0)
                // BEQ    Rtmp, -3(PC)
                // SYNC
                var ll = mips.ALLV;
                var sc = mips.ASCV;
                if (v.Op == ssa.OpMIPS64LoweredAtomicExchange32)
                {
                    ll = mips.ALL;
                    sc = mips.ASC;
                }

                s.Prog(mips.ASYNC);
                p = s.Prog(mips.AMOVV);
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = v.Args[1L].Reg();
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = mips.REGTMP;
                var p1 = s.Prog(ll);
                p1.From.Type = obj.TYPE_MEM;
                p1.From.Reg = v.Args[0L].Reg();
                p1.To.Type = obj.TYPE_REG;
                p1.To.Reg = v.Reg0();
                p2 = s.Prog(sc);
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
            if (v.Op == ssa.OpMIPS64LoweredAtomicAdd32 || v.Op == ssa.OpMIPS64LoweredAtomicAdd64) 
            {
                // SYNC
                // LL    (Rarg0), Rout
                // ADDV Rarg1, Rout, Rtmp
                // SC    Rtmp, (Rarg0)
                // BEQ    Rtmp, -3(PC)
                // SYNC
                // ADDV Rarg1, Rout
                ll = mips.ALLV;
                sc = mips.ASCV;
                if (v.Op == ssa.OpMIPS64LoweredAtomicAdd32)
                {
                    ll = mips.ALL;
                    sc = mips.ASC;
                }

                s.Prog(mips.ASYNC);
                p = s.Prog(ll);
                p.From.Type = obj.TYPE_MEM;
                p.From.Reg = v.Args[0L].Reg();
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg0();
                p1 = s.Prog(mips.AADDVU);
                p1.From.Type = obj.TYPE_REG;
                p1.From.Reg = v.Args[1L].Reg();
                p1.Reg = v.Reg0();
                p1.To.Type = obj.TYPE_REG;
                p1.To.Reg = mips.REGTMP;
                p2 = s.Prog(sc);
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
                p4 = s.Prog(mips.AADDVU);
                p4.From.Type = obj.TYPE_REG;
                p4.From.Reg = v.Args[1L].Reg();
                p4.Reg = v.Reg0();
                p4.To.Type = obj.TYPE_REG;
                p4.To.Reg = v.Reg0();
                goto __switch_break0;
            }
            if (v.Op == ssa.OpMIPS64LoweredAtomicAddconst32 || v.Op == ssa.OpMIPS64LoweredAtomicAddconst64) 
            {
                // SYNC
                // LL    (Rarg0), Rout
                // ADDV $auxint, Rout, Rtmp
                // SC    Rtmp, (Rarg0)
                // BEQ    Rtmp, -3(PC)
                // SYNC
                // ADDV $auxint, Rout
                ll = mips.ALLV;
                sc = mips.ASCV;
                if (v.Op == ssa.OpMIPS64LoweredAtomicAddconst32)
                {
                    ll = mips.ALL;
                    sc = mips.ASC;
                }

                s.Prog(mips.ASYNC);
                p = s.Prog(ll);
                p.From.Type = obj.TYPE_MEM;
                p.From.Reg = v.Args[0L].Reg();
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg0();
                p1 = s.Prog(mips.AADDVU);
                p1.From.Type = obj.TYPE_CONST;
                p1.From.Offset = v.AuxInt;
                p1.Reg = v.Reg0();
                p1.To.Type = obj.TYPE_REG;
                p1.To.Reg = mips.REGTMP;
                p2 = s.Prog(sc);
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
                p4 = s.Prog(mips.AADDVU);
                p4.From.Type = obj.TYPE_CONST;
                p4.From.Offset = v.AuxInt;
                p4.Reg = v.Reg0();
                p4.To.Type = obj.TYPE_REG;
                p4.To.Reg = v.Reg0();
                goto __switch_break0;
            }
            if (v.Op == ssa.OpMIPS64LoweredAtomicCas32 || v.Op == ssa.OpMIPS64LoweredAtomicCas64) 
            {
                // MOVV $0, Rout
                // SYNC
                // LL    (Rarg0), Rtmp
                // BNE    Rtmp, Rarg1, 4(PC)
                // MOVV Rarg2, Rout
                // SC    Rout, (Rarg0)
                // BEQ    Rout, -4(PC)
                // SYNC
                ll = mips.ALLV;
                sc = mips.ASCV;
                if (v.Op == ssa.OpMIPS64LoweredAtomicCas32)
                {
                    ll = mips.ALL;
                    sc = mips.ASC;
                }

                p = s.Prog(mips.AMOVV);
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = mips.REGZERO;
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg0();
                s.Prog(mips.ASYNC);
                p1 = s.Prog(ll);
                p1.From.Type = obj.TYPE_MEM;
                p1.From.Reg = v.Args[0L].Reg();
                p1.To.Type = obj.TYPE_REG;
                p1.To.Reg = mips.REGTMP;
                p2 = s.Prog(mips.ABNE);
                p2.From.Type = obj.TYPE_REG;
                p2.From.Reg = v.Args[1L].Reg();
                p2.Reg = mips.REGTMP;
                p2.To.Type = obj.TYPE_BRANCH;
                p3 = s.Prog(mips.AMOVV);
                p3.From.Type = obj.TYPE_REG;
                p3.From.Reg = v.Args[2L].Reg();
                p3.To.Type = obj.TYPE_REG;
                p3.To.Reg = v.Reg0();
                p4 = s.Prog(sc);
                p4.From.Type = obj.TYPE_REG;
                p4.From.Reg = v.Reg0();
                p4.To.Type = obj.TYPE_MEM;
                p4.To.Reg = v.Args[0L].Reg();
                p5 = s.Prog(mips.ABEQ);
                p5.From.Type = obj.TYPE_REG;
                p5.From.Reg = v.Reg0();
                p5.To.Type = obj.TYPE_BRANCH;
                gc.Patch(p5, p1);
                p6 = s.Prog(mips.ASYNC);
                gc.Patch(p2, p6);
                goto __switch_break0;
            }
            if (v.Op == ssa.OpMIPS64LoweredNilCheck) 
            {
                // Issue a load which will fault if arg is nil.
                p = s.Prog(mips.AMOVB);
                p.From.Type = obj.TYPE_MEM;
                p.From.Reg = v.Args[0L].Reg();
                gc.AddAux(_addr_p.From, v);
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = mips.REGTMP;
                if (logopt.Enabled())
                {
                    logopt.LogOpt(v.Pos, "nilcheck", "genssa", v.Block.Func.Name);
                }

                if (gc.Debug_checknil != 0L && v.Pos.Line() > 1L)
                { // v.Pos.Line()==1 in generated wrappers
                    gc.Warnl(v.Pos, "generated nil check");

                }

                goto __switch_break0;
            }
            if (v.Op == ssa.OpMIPS64FPFlagTrue || v.Op == ssa.OpMIPS64FPFlagFalse) 
            {
                // MOVV    $0, r
                // BFPF    2(PC)
                // MOVV    $1, r
                var branch = mips.ABFPF;
                if (v.Op == ssa.OpMIPS64FPFlagFalse)
                {
                    branch = mips.ABFPT;
                }

                p = s.Prog(mips.AMOVV);
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = mips.REGZERO;
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
                p2 = s.Prog(branch);
                p2.To.Type = obj.TYPE_BRANCH;
                p3 = s.Prog(mips.AMOVV);
                p3.From.Type = obj.TYPE_CONST;
                p3.From.Offset = 1L;
                p3.To.Type = obj.TYPE_REG;
                p3.To.Reg = v.Reg();
                p4 = s.Prog(obj.ANOP); // not a machine instruction, for branch to land
                gc.Patch(p2, p4);
                goto __switch_break0;
            }
            if (v.Op == ssa.OpMIPS64LoweredGetClosurePtr) 
            {
                // Closure pointer is R22 (mips.REGCTXT).
                gc.CheckLoweredGetClosurePtr(v);
                goto __switch_break0;
            }
            if (v.Op == ssa.OpMIPS64LoweredGetCallerSP) 
            {
                // caller's SP is FixedFrameSize below the address of the first arg
                p = s.Prog(mips.AMOVV);
                p.From.Type = obj.TYPE_ADDR;
                p.From.Offset = -gc.Ctxt.FixedFrameSize();
                p.From.Name = obj.NAME_PARAM;
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
                goto __switch_break0;
            }
            if (v.Op == ssa.OpMIPS64LoweredGetCallerPC)
            {
                p = s.Prog(obj.AGETCALLERPC);
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



        private static void ssaGenBlock(ptr<gc.SSAGenState> _addr_s, ptr<ssa.Block> _addr_b, ptr<ssa.Block> _addr_next)
        {
            ref gc.SSAGenState s = ref _addr_s.val;
            ref ssa.Block b = ref _addr_b.val;
            ref ssa.Block next = ref _addr_next.val;


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

            else if (b.Kind == ssa.BlockExit)             else if (b.Kind == ssa.BlockRet) 
                s.Prog(obj.ARET);
            else if (b.Kind == ssa.BlockRetJmp) 
                p = s.Prog(obj.ARET);
                p.To.Type = obj.TYPE_MEM;
                p.To.Name = obj.NAME_EXTERN;
                p.To.Sym = b.Aux._<ptr<obj.LSym>>();
            else if (b.Kind == ssa.BlockMIPS64EQ || b.Kind == ssa.BlockMIPS64NE || b.Kind == ssa.BlockMIPS64LTZ || b.Kind == ssa.BlockMIPS64GEZ || b.Kind == ssa.BlockMIPS64LEZ || b.Kind == ssa.BlockMIPS64GTZ || b.Kind == ssa.BlockMIPS64FPT || b.Kind == ssa.BlockMIPS64FPF) 
                var jmp = blockJump[b.Kind];
                p = ;

                if (next == b.Succs[0L].Block()) 
                    p = s.Br(jmp.invasm, b.Succs[1L].Block());
                else if (next == b.Succs[1L].Block()) 
                    p = s.Br(jmp.asm, b.Succs[0L].Block());
                else 
                    if (b.Likely != ssa.BranchUnlikely)
                    {
                        p = s.Br(jmp.asm, b.Succs[0L].Block());
                        s.Br(obj.AJMP, b.Succs[1L].Block());
                    }
                    else
                    {
                        p = s.Br(jmp.invasm, b.Succs[1L].Block());
                        s.Br(obj.AJMP, b.Succs[0L].Block());
                    }

                                if (!b.Controls[0L].Type.IsFlags())
                {
                    p.From.Type = obj.TYPE_REG;
                    p.From.Reg = b.Controls[0L].Reg();
                }

            else 
                b.Fatalf("branch not implemented: %s", b.LongString());
            
        }
    }
}}}}
