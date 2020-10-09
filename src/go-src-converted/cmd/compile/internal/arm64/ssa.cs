// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package arm64 -- go2cs converted at 2020 October 09 05:44:05 UTC
// import "cmd/compile/internal/arm64" ==> using arm64 = go.cmd.compile.@internal.arm64_package
// Original source: C:\Go\src\cmd\compile\internal\arm64\ssa.go
using math = go.math_package;

using gc = go.cmd.compile.@internal.gc_package;
using logopt = go.cmd.compile.@internal.logopt_package;
using ssa = go.cmd.compile.@internal.ssa_package;
using types = go.cmd.compile.@internal.types_package;
using obj = go.cmd.@internal.obj_package;
using arm64 = go.cmd.@internal.obj.arm64_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class arm64_package
    {
        // loadByType returns the load instruction of the given type.
        private static obj.As loadByType(ptr<types.Type> _addr_t) => func((_, panic, __) =>
        {
            ref types.Type t = ref _addr_t.val;

            if (t.IsFloat())
            {
                switch (t.Size())
                {
                    case 4L: 
                        return arm64.AFMOVS;
                        break;
                    case 8L: 
                        return arm64.AFMOVD;
                        break;
                }

            }
            else
            {
                switch (t.Size())
                {
                    case 1L: 
                        if (t.IsSigned())
                        {
                            return arm64.AMOVB;
                        }
                        else
                        {
                            return arm64.AMOVBU;
                        }
                        break;
                    case 2L: 
                        if (t.IsSigned())
                        {
                            return arm64.AMOVH;
                        }
                        else
                        {
                            return arm64.AMOVHU;
                        }
                        break;
                    case 4L: 
                        if (t.IsSigned())
                        {
                            return arm64.AMOVW;
                        }
                        else
                        {
                            return arm64.AMOVWU;
                        }
                        break;
                    case 8L: 
                        return arm64.AMOVD;
                        break;
                }

            }
            panic("bad load type");

        });

        // storeByType returns the store instruction of the given type.
        private static obj.As storeByType(ptr<types.Type> _addr_t) => func((_, panic, __) =>
        {
            ref types.Type t = ref _addr_t.val;

            if (t.IsFloat())
            {
                switch (t.Size())
                {
                    case 4L: 
                        return arm64.AFMOVS;
                        break;
                    case 8L: 
                        return arm64.AFMOVD;
                        break;
                }

            }
            else
            {
                switch (t.Size())
                {
                    case 1L: 
                        return arm64.AMOVB;
                        break;
                    case 2L: 
                        return arm64.AMOVH;
                        break;
                    case 4L: 
                        return arm64.AMOVW;
                        break;
                    case 8L: 
                        return arm64.AMOVD;
                        break;
                }

            }

            panic("bad store type");

        });

        // makeshift encodes a register shifted by a constant, used as an Offset in Prog
        private static long makeshift(short reg, long typ, long s)
        {
            return int64(reg & 31L) << (int)(16L) | typ | (s & 63L) << (int)(10L);
        }

        // genshift generates a Prog for r = r0 op (r1 shifted by n)
        private static ptr<obj.Prog> genshift(ptr<gc.SSAGenState> _addr_s, obj.As @as, short r0, short r1, short r, long typ, long n)
        {
            ref gc.SSAGenState s = ref _addr_s.val;

            var p = s.Prog(as);
            p.From.Type = obj.TYPE_SHIFT;
            p.From.Offset = makeshift(r1, typ, n);
            p.Reg = r0;
            if (r != 0L)
            {
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = r;
            }

            return _addr_p!;

        }

        // generate the memory operand for the indexed load/store instructions
        private static obj.Addr genIndexedOperand(ptr<ssa.Value> _addr_v)
        {
            ref ssa.Value v = ref _addr_v.val;
 
            // Reg: base register, Index: (shifted) index register
            obj.Addr mop = new obj.Addr(Type:obj.TYPE_MEM,Reg:v.Args[0].Reg());

            if (v.Op == ssa.OpARM64MOVDloadidx8 || v.Op == ssa.OpARM64MOVDstoreidx8 || v.Op == ssa.OpARM64MOVDstorezeroidx8) 
                mop.Index = arm64.REG_LSL | 3L << (int)(5L) | v.Args[1L].Reg() & 31L;
            else if (v.Op == ssa.OpARM64MOVWloadidx4 || v.Op == ssa.OpARM64MOVWUloadidx4 || v.Op == ssa.OpARM64MOVWstoreidx4 || v.Op == ssa.OpARM64MOVWstorezeroidx4) 
                mop.Index = arm64.REG_LSL | 2L << (int)(5L) | v.Args[1L].Reg() & 31L;
            else if (v.Op == ssa.OpARM64MOVHloadidx2 || v.Op == ssa.OpARM64MOVHUloadidx2 || v.Op == ssa.OpARM64MOVHstoreidx2 || v.Op == ssa.OpARM64MOVHstorezeroidx2) 
                mop.Index = arm64.REG_LSL | 1L << (int)(5L) | v.Args[1L].Reg() & 31L;
            else // not shifted
                mop.Index = v.Args[1L].Reg();
                        return mop;

        }

        private static void ssaGenValue(ptr<gc.SSAGenState> _addr_s, ptr<ssa.Value> _addr_v) => func((_, panic, __) =>
        {
            ref gc.SSAGenState s = ref _addr_s.val;
            ref ssa.Value v = ref _addr_v.val;


            if (v.Op == ssa.OpCopy || v.Op == ssa.OpARM64MOVDreg)
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

                var @as = arm64.AMOVD;
                if (v.Type.IsFloat())
                {
                    switch (v.Type.Size())
                    {
                        case 4L: 
                            as = arm64.AFMOVS;
                            break;
                        case 8L: 
                            as = arm64.AFMOVD;
                            break;
                        default: 
                            panic("bad float size");
                            break;
                    }

                }

                var p = s.Prog(as);
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = x;
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = y;
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARM64MOVDnop)
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

                p = s.Prog(loadByType(_addr_v.Type));
                gc.AddrAuto(_addr_p.From, v.Args[0L]);
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
                goto __switch_break0;
            }
            if (v.Op == ssa.OpStoreReg)
            {
                if (v.Type.IsFlags())
                {
                    v.Fatalf("store flags not implemented: %v", v.LongString());
                    return ;
                }

                p = s.Prog(storeByType(_addr_v.Type));
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = v.Args[0L].Reg();
                gc.AddrAuto(_addr_p.To, v);
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARM64ADD || v.Op == ssa.OpARM64SUB || v.Op == ssa.OpARM64AND || v.Op == ssa.OpARM64OR || v.Op == ssa.OpARM64XOR || v.Op == ssa.OpARM64BIC || v.Op == ssa.OpARM64EON || v.Op == ssa.OpARM64ORN || v.Op == ssa.OpARM64MUL || v.Op == ssa.OpARM64MULW || v.Op == ssa.OpARM64MNEG || v.Op == ssa.OpARM64MNEGW || v.Op == ssa.OpARM64MULH || v.Op == ssa.OpARM64UMULH || v.Op == ssa.OpARM64MULL || v.Op == ssa.OpARM64UMULL || v.Op == ssa.OpARM64DIV || v.Op == ssa.OpARM64UDIV || v.Op == ssa.OpARM64DIVW || v.Op == ssa.OpARM64UDIVW || v.Op == ssa.OpARM64MOD || v.Op == ssa.OpARM64UMOD || v.Op == ssa.OpARM64MODW || v.Op == ssa.OpARM64UMODW || v.Op == ssa.OpARM64SLL || v.Op == ssa.OpARM64SRL || v.Op == ssa.OpARM64SRA || v.Op == ssa.OpARM64FADDS || v.Op == ssa.OpARM64FADDD || v.Op == ssa.OpARM64FSUBS || v.Op == ssa.OpARM64FSUBD || v.Op == ssa.OpARM64FMULS || v.Op == ssa.OpARM64FMULD || v.Op == ssa.OpARM64FNMULS || v.Op == ssa.OpARM64FNMULD || v.Op == ssa.OpARM64FDIVS || v.Op == ssa.OpARM64FDIVD || v.Op == ssa.OpARM64ROR || v.Op == ssa.OpARM64RORW)
            {
                var r = v.Reg();
                var r1 = v.Args[0L].Reg();
                var r2 = v.Args[1L].Reg();
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = r2;
                p.Reg = r1;
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = r;
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARM64FMADDS || v.Op == ssa.OpARM64FMADDD || v.Op == ssa.OpARM64FNMADDS || v.Op == ssa.OpARM64FNMADDD || v.Op == ssa.OpARM64FMSUBS || v.Op == ssa.OpARM64FMSUBD || v.Op == ssa.OpARM64FNMSUBS || v.Op == ssa.OpARM64FNMSUBD || v.Op == ssa.OpARM64MADD || v.Op == ssa.OpARM64MADDW || v.Op == ssa.OpARM64MSUB || v.Op == ssa.OpARM64MSUBW)
            {
                var rt = v.Reg();
                var ra = v.Args[0L].Reg();
                var rm = v.Args[1L].Reg();
                var rn = v.Args[2L].Reg();
                p = s.Prog(v.Op.Asm());
                p.Reg = ra;
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = rm;
                p.SetFrom3(new obj.Addr(Type:obj.TYPE_REG,Reg:rn));
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = rt;
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARM64ADDconst || v.Op == ssa.OpARM64SUBconst || v.Op == ssa.OpARM64ANDconst || v.Op == ssa.OpARM64ORconst || v.Op == ssa.OpARM64XORconst || v.Op == ssa.OpARM64SLLconst || v.Op == ssa.OpARM64SRLconst || v.Op == ssa.OpARM64SRAconst || v.Op == ssa.OpARM64RORconst || v.Op == ssa.OpARM64RORWconst)
            {
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_CONST;
                p.From.Offset = v.AuxInt;
                p.Reg = v.Args[0L].Reg();
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARM64ADDSconstflags)
            {
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_CONST;
                p.From.Offset = v.AuxInt;
                p.Reg = v.Args[0L].Reg();
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg0();
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARM64ADCzerocarry)
            {
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = arm64.REGZERO;
                p.Reg = arm64.REGZERO;
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARM64ADCSflags || v.Op == ssa.OpARM64ADDSflags || v.Op == ssa.OpARM64SBCSflags || v.Op == ssa.OpARM64SUBSflags)
            {
                r = v.Reg0();
                r1 = v.Args[0L].Reg();
                r2 = v.Args[1L].Reg();
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = r2;
                p.Reg = r1;
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = r;
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARM64NEGSflags)
            {
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = v.Args[0L].Reg();
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg0();
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARM64NGCzerocarry)
            {
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = arm64.REGZERO;
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARM64EXTRconst || v.Op == ssa.OpARM64EXTRWconst)
            {
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_CONST;
                p.From.Offset = v.AuxInt;
                p.SetFrom3(new obj.Addr(Type:obj.TYPE_REG,Reg:v.Args[0].Reg()));
                p.Reg = v.Args[1L].Reg();
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARM64MVNshiftLL || v.Op == ssa.OpARM64NEGshiftLL)
            {
                genshift(_addr_s, v.Op.Asm(), 0L, v.Args[0L].Reg(), v.Reg(), arm64.SHIFT_LL, v.AuxInt);
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARM64MVNshiftRL || v.Op == ssa.OpARM64NEGshiftRL)
            {
                genshift(_addr_s, v.Op.Asm(), 0L, v.Args[0L].Reg(), v.Reg(), arm64.SHIFT_LR, v.AuxInt);
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARM64MVNshiftRA || v.Op == ssa.OpARM64NEGshiftRA)
            {
                genshift(_addr_s, v.Op.Asm(), 0L, v.Args[0L].Reg(), v.Reg(), arm64.SHIFT_AR, v.AuxInt);
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARM64ADDshiftLL || v.Op == ssa.OpARM64SUBshiftLL || v.Op == ssa.OpARM64ANDshiftLL || v.Op == ssa.OpARM64ORshiftLL || v.Op == ssa.OpARM64XORshiftLL || v.Op == ssa.OpARM64EONshiftLL || v.Op == ssa.OpARM64ORNshiftLL || v.Op == ssa.OpARM64BICshiftLL)
            {
                genshift(_addr_s, v.Op.Asm(), v.Args[0L].Reg(), v.Args[1L].Reg(), v.Reg(), arm64.SHIFT_LL, v.AuxInt);
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARM64ADDshiftRL || v.Op == ssa.OpARM64SUBshiftRL || v.Op == ssa.OpARM64ANDshiftRL || v.Op == ssa.OpARM64ORshiftRL || v.Op == ssa.OpARM64XORshiftRL || v.Op == ssa.OpARM64EONshiftRL || v.Op == ssa.OpARM64ORNshiftRL || v.Op == ssa.OpARM64BICshiftRL)
            {
                genshift(_addr_s, v.Op.Asm(), v.Args[0L].Reg(), v.Args[1L].Reg(), v.Reg(), arm64.SHIFT_LR, v.AuxInt);
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARM64ADDshiftRA || v.Op == ssa.OpARM64SUBshiftRA || v.Op == ssa.OpARM64ANDshiftRA || v.Op == ssa.OpARM64ORshiftRA || v.Op == ssa.OpARM64XORshiftRA || v.Op == ssa.OpARM64EONshiftRA || v.Op == ssa.OpARM64ORNshiftRA || v.Op == ssa.OpARM64BICshiftRA)
            {
                genshift(_addr_s, v.Op.Asm(), v.Args[0L].Reg(), v.Args[1L].Reg(), v.Reg(), arm64.SHIFT_AR, v.AuxInt);
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARM64MOVDconst)
            {
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_CONST;
                p.From.Offset = v.AuxInt;
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARM64FMOVSconst || v.Op == ssa.OpARM64FMOVDconst)
            {
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_FCONST;
                p.From.Val = math.Float64frombits(uint64(v.AuxInt));
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARM64FCMPS0 || v.Op == ssa.OpARM64FCMPD0)
            {
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_FCONST;
                p.From.Val = math.Float64frombits(0L);
                p.Reg = v.Args[0L].Reg();
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARM64CMP || v.Op == ssa.OpARM64CMPW || v.Op == ssa.OpARM64CMN || v.Op == ssa.OpARM64CMNW || v.Op == ssa.OpARM64TST || v.Op == ssa.OpARM64TSTW || v.Op == ssa.OpARM64FCMPS || v.Op == ssa.OpARM64FCMPD)
            {
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = v.Args[1L].Reg();
                p.Reg = v.Args[0L].Reg();
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARM64CMPconst || v.Op == ssa.OpARM64CMPWconst || v.Op == ssa.OpARM64CMNconst || v.Op == ssa.OpARM64CMNWconst || v.Op == ssa.OpARM64TSTconst || v.Op == ssa.OpARM64TSTWconst)
            {
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_CONST;
                p.From.Offset = v.AuxInt;
                p.Reg = v.Args[0L].Reg();
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARM64CMPshiftLL || v.Op == ssa.OpARM64CMNshiftLL || v.Op == ssa.OpARM64TSTshiftLL)
            {
                genshift(_addr_s, v.Op.Asm(), v.Args[0L].Reg(), v.Args[1L].Reg(), 0L, arm64.SHIFT_LL, v.AuxInt);
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARM64CMPshiftRL || v.Op == ssa.OpARM64CMNshiftRL || v.Op == ssa.OpARM64TSTshiftRL)
            {
                genshift(_addr_s, v.Op.Asm(), v.Args[0L].Reg(), v.Args[1L].Reg(), 0L, arm64.SHIFT_LR, v.AuxInt);
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARM64CMPshiftRA || v.Op == ssa.OpARM64CMNshiftRA || v.Op == ssa.OpARM64TSTshiftRA)
            {
                genshift(_addr_s, v.Op.Asm(), v.Args[0L].Reg(), v.Args[1L].Reg(), 0L, arm64.SHIFT_AR, v.AuxInt);
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARM64MOVDaddr)
            {
                p = s.Prog(arm64.AMOVD);
                p.From.Type = obj.TYPE_ADDR;
                p.From.Reg = v.Args[0L].Reg();
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();

                @string wantreg = default; 
                // MOVD $sym+off(base), R
                // the assembler expands it as the following:
                // - base is SP: add constant offset to SP (R13)
                //               when constant is large, tmp register (R11) may be used
                // - base is SB: load external address from constant pool (use relocation)
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

                goto __switch_break0;
            }
            if (v.Op == ssa.OpARM64MOVBload || v.Op == ssa.OpARM64MOVBUload || v.Op == ssa.OpARM64MOVHload || v.Op == ssa.OpARM64MOVHUload || v.Op == ssa.OpARM64MOVWload || v.Op == ssa.OpARM64MOVWUload || v.Op == ssa.OpARM64MOVDload || v.Op == ssa.OpARM64FMOVSload || v.Op == ssa.OpARM64FMOVDload)
            {
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_MEM;
                p.From.Reg = v.Args[0L].Reg();
                gc.AddAux(_addr_p.From, v);
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARM64MOVBloadidx || v.Op == ssa.OpARM64MOVBUloadidx || v.Op == ssa.OpARM64MOVHloadidx || v.Op == ssa.OpARM64MOVHUloadidx || v.Op == ssa.OpARM64MOVWloadidx || v.Op == ssa.OpARM64MOVWUloadidx || v.Op == ssa.OpARM64MOVDloadidx || v.Op == ssa.OpARM64FMOVSloadidx || v.Op == ssa.OpARM64FMOVDloadidx || v.Op == ssa.OpARM64MOVHloadidx2 || v.Op == ssa.OpARM64MOVHUloadidx2 || v.Op == ssa.OpARM64MOVWloadidx4 || v.Op == ssa.OpARM64MOVWUloadidx4 || v.Op == ssa.OpARM64MOVDloadidx8)
            {
                p = s.Prog(v.Op.Asm());
                p.From = genIndexedOperand(_addr_v);
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARM64LDAR || v.Op == ssa.OpARM64LDARB || v.Op == ssa.OpARM64LDARW)
            {
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_MEM;
                p.From.Reg = v.Args[0L].Reg();
                gc.AddAux(_addr_p.From, v);
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg0();
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARM64MOVBstore || v.Op == ssa.OpARM64MOVHstore || v.Op == ssa.OpARM64MOVWstore || v.Op == ssa.OpARM64MOVDstore || v.Op == ssa.OpARM64FMOVSstore || v.Op == ssa.OpARM64FMOVDstore || v.Op == ssa.OpARM64STLRB || v.Op == ssa.OpARM64STLR || v.Op == ssa.OpARM64STLRW)
            {
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = v.Args[1L].Reg();
                p.To.Type = obj.TYPE_MEM;
                p.To.Reg = v.Args[0L].Reg();
                gc.AddAux(_addr_p.To, v);
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARM64MOVBstoreidx || v.Op == ssa.OpARM64MOVHstoreidx || v.Op == ssa.OpARM64MOVWstoreidx || v.Op == ssa.OpARM64MOVDstoreidx || v.Op == ssa.OpARM64FMOVSstoreidx || v.Op == ssa.OpARM64FMOVDstoreidx || v.Op == ssa.OpARM64MOVHstoreidx2 || v.Op == ssa.OpARM64MOVWstoreidx4 || v.Op == ssa.OpARM64MOVDstoreidx8)
            {
                p = s.Prog(v.Op.Asm());
                p.To = genIndexedOperand(_addr_v);
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = v.Args[2L].Reg();
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARM64STP)
            {
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_REGREG;
                p.From.Reg = v.Args[1L].Reg();
                p.From.Offset = int64(v.Args[2L].Reg());
                p.To.Type = obj.TYPE_MEM;
                p.To.Reg = v.Args[0L].Reg();
                gc.AddAux(_addr_p.To, v);
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARM64MOVBstorezero || v.Op == ssa.OpARM64MOVHstorezero || v.Op == ssa.OpARM64MOVWstorezero || v.Op == ssa.OpARM64MOVDstorezero)
            {
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = arm64.REGZERO;
                p.To.Type = obj.TYPE_MEM;
                p.To.Reg = v.Args[0L].Reg();
                gc.AddAux(_addr_p.To, v);
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARM64MOVBstorezeroidx || v.Op == ssa.OpARM64MOVHstorezeroidx || v.Op == ssa.OpARM64MOVWstorezeroidx || v.Op == ssa.OpARM64MOVDstorezeroidx || v.Op == ssa.OpARM64MOVHstorezeroidx2 || v.Op == ssa.OpARM64MOVWstorezeroidx4 || v.Op == ssa.OpARM64MOVDstorezeroidx8)
            {
                p = s.Prog(v.Op.Asm());
                p.To = genIndexedOperand(_addr_v);
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = arm64.REGZERO;
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARM64MOVQstorezero)
            {
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_REGREG;
                p.From.Reg = arm64.REGZERO;
                p.From.Offset = int64(arm64.REGZERO);
                p.To.Type = obj.TYPE_MEM;
                p.To.Reg = v.Args[0L].Reg();
                gc.AddAux(_addr_p.To, v);
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARM64BFI || v.Op == ssa.OpARM64BFXIL)
            {
                r = v.Reg();
                if (r != v.Args[0L].Reg())
                {
                    v.Fatalf("input[0] and output not in same register %s", v.LongString());
                }

                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_CONST;
                p.From.Offset = v.AuxInt >> (int)(8L);
                p.SetFrom3(new obj.Addr(Type:obj.TYPE_CONST,Offset:v.AuxInt&0xff));
                p.Reg = v.Args[1L].Reg();
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = r;
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARM64SBFIZ || v.Op == ssa.OpARM64SBFX || v.Op == ssa.OpARM64UBFIZ || v.Op == ssa.OpARM64UBFX)
            {
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_CONST;
                p.From.Offset = v.AuxInt >> (int)(8L);
                p.SetFrom3(new obj.Addr(Type:obj.TYPE_CONST,Offset:v.AuxInt&0xff));
                p.Reg = v.Args[0L].Reg();
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARM64LoweredMuluhilo)
            {
                var r0 = v.Args[0L].Reg();
                r1 = v.Args[1L].Reg();
                p = s.Prog(arm64.AUMULH);
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = r1;
                p.Reg = r0;
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg0();
                var p1 = s.Prog(arm64.AMUL);
                p1.From.Type = obj.TYPE_REG;
                p1.From.Reg = r1;
                p1.Reg = r0;
                p1.To.Type = obj.TYPE_REG;
                p1.To.Reg = v.Reg1();
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARM64LoweredAtomicExchange64 || v.Op == ssa.OpARM64LoweredAtomicExchange32) 
            {
                // LDAXR    (Rarg0), Rout
                // STLXR    Rarg1, (Rarg0), Rtmp
                // CBNZ        Rtmp, -2(PC)
                var ld = arm64.ALDAXR;
                var st = arm64.ASTLXR;
                if (v.Op == ssa.OpARM64LoweredAtomicExchange32)
                {
                    ld = arm64.ALDAXRW;
                    st = arm64.ASTLXRW;
                }

                r0 = v.Args[0L].Reg();
                r1 = v.Args[1L].Reg();
                var @out = v.Reg0();
                p = s.Prog(ld);
                p.From.Type = obj.TYPE_MEM;
                p.From.Reg = r0;
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = out;
                p1 = s.Prog(st);
                p1.From.Type = obj.TYPE_REG;
                p1.From.Reg = r1;
                p1.To.Type = obj.TYPE_MEM;
                p1.To.Reg = r0;
                p1.RegTo2 = arm64.REGTMP;
                var p2 = s.Prog(arm64.ACBNZ);
                p2.From.Type = obj.TYPE_REG;
                p2.From.Reg = arm64.REGTMP;
                p2.To.Type = obj.TYPE_BRANCH;
                gc.Patch(p2, p);
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARM64LoweredAtomicAdd64 || v.Op == ssa.OpARM64LoweredAtomicAdd32) 
            {
                // LDAXR    (Rarg0), Rout
                // ADD        Rarg1, Rout
                // STLXR    Rout, (Rarg0), Rtmp
                // CBNZ        Rtmp, -3(PC)
                ld = arm64.ALDAXR;
                st = arm64.ASTLXR;
                if (v.Op == ssa.OpARM64LoweredAtomicAdd32)
                {
                    ld = arm64.ALDAXRW;
                    st = arm64.ASTLXRW;
                }

                r0 = v.Args[0L].Reg();
                r1 = v.Args[1L].Reg();
                @out = v.Reg0();
                p = s.Prog(ld);
                p.From.Type = obj.TYPE_MEM;
                p.From.Reg = r0;
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = out;
                p1 = s.Prog(arm64.AADD);
                p1.From.Type = obj.TYPE_REG;
                p1.From.Reg = r1;
                p1.To.Type = obj.TYPE_REG;
                p1.To.Reg = out;
                p2 = s.Prog(st);
                p2.From.Type = obj.TYPE_REG;
                p2.From.Reg = out;
                p2.To.Type = obj.TYPE_MEM;
                p2.To.Reg = r0;
                p2.RegTo2 = arm64.REGTMP;
                var p3 = s.Prog(arm64.ACBNZ);
                p3.From.Type = obj.TYPE_REG;
                p3.From.Reg = arm64.REGTMP;
                p3.To.Type = obj.TYPE_BRANCH;
                gc.Patch(p3, p);
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARM64LoweredAtomicAdd64Variant || v.Op == ssa.OpARM64LoweredAtomicAdd32Variant) 
            {
                // LDADDAL    Rarg1, (Rarg0), Rout
                // ADD        Rarg1, Rout
                var op = arm64.ALDADDALD;
                if (v.Op == ssa.OpARM64LoweredAtomicAdd32Variant)
                {
                    op = arm64.ALDADDALW;
                }

                r0 = v.Args[0L].Reg();
                r1 = v.Args[1L].Reg();
                @out = v.Reg0();
                p = s.Prog(op);
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = r1;
                p.To.Type = obj.TYPE_MEM;
                p.To.Reg = r0;
                p.RegTo2 = out;
                p1 = s.Prog(arm64.AADD);
                p1.From.Type = obj.TYPE_REG;
                p1.From.Reg = r1;
                p1.To.Type = obj.TYPE_REG;
                p1.To.Reg = out;
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARM64LoweredAtomicCas64 || v.Op == ssa.OpARM64LoweredAtomicCas32) 
            {
                // LDAXR    (Rarg0), Rtmp
                // CMP        Rarg1, Rtmp
                // BNE        3(PC)
                // STLXR    Rarg2, (Rarg0), Rtmp
                // CBNZ        Rtmp, -4(PC)
                // CSET        EQ, Rout
                ld = arm64.ALDAXR;
                st = arm64.ASTLXR;
                var cmp = arm64.ACMP;
                if (v.Op == ssa.OpARM64LoweredAtomicCas32)
                {
                    ld = arm64.ALDAXRW;
                    st = arm64.ASTLXRW;
                    cmp = arm64.ACMPW;
                }

                r0 = v.Args[0L].Reg();
                r1 = v.Args[1L].Reg();
                r2 = v.Args[2L].Reg();
                @out = v.Reg0();
                p = s.Prog(ld);
                p.From.Type = obj.TYPE_MEM;
                p.From.Reg = r0;
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = arm64.REGTMP;
                p1 = s.Prog(cmp);
                p1.From.Type = obj.TYPE_REG;
                p1.From.Reg = r1;
                p1.Reg = arm64.REGTMP;
                p2 = s.Prog(arm64.ABNE);
                p2.To.Type = obj.TYPE_BRANCH;
                p3 = s.Prog(st);
                p3.From.Type = obj.TYPE_REG;
                p3.From.Reg = r2;
                p3.To.Type = obj.TYPE_MEM;
                p3.To.Reg = r0;
                p3.RegTo2 = arm64.REGTMP;
                var p4 = s.Prog(arm64.ACBNZ);
                p4.From.Type = obj.TYPE_REG;
                p4.From.Reg = arm64.REGTMP;
                p4.To.Type = obj.TYPE_BRANCH;
                gc.Patch(p4, p);
                var p5 = s.Prog(arm64.ACSET);
                p5.From.Type = obj.TYPE_REG; // assembler encodes conditional bits in Reg
                p5.From.Reg = arm64.COND_EQ;
                p5.To.Type = obj.TYPE_REG;
                p5.To.Reg = out;
                gc.Patch(p2, p5);
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARM64LoweredAtomicAnd8 || v.Op == ssa.OpARM64LoweredAtomicOr8) 
            {
                // LDAXRB    (Rarg0), Rout
                // AND/OR    Rarg1, Rout
                // STLXRB    Rout, (Rarg0), Rtmp
                // CBNZ        Rtmp, -3(PC)
                r0 = v.Args[0L].Reg();
                r1 = v.Args[1L].Reg();
                @out = v.Reg0();
                p = s.Prog(arm64.ALDAXRB);
                p.From.Type = obj.TYPE_MEM;
                p.From.Reg = r0;
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = out;
                p1 = s.Prog(v.Op.Asm());
                p1.From.Type = obj.TYPE_REG;
                p1.From.Reg = r1;
                p1.To.Type = obj.TYPE_REG;
                p1.To.Reg = out;
                p2 = s.Prog(arm64.ASTLXRB);
                p2.From.Type = obj.TYPE_REG;
                p2.From.Reg = out;
                p2.To.Type = obj.TYPE_MEM;
                p2.To.Reg = r0;
                p2.RegTo2 = arm64.REGTMP;
                p3 = s.Prog(arm64.ACBNZ);
                p3.From.Type = obj.TYPE_REG;
                p3.From.Reg = arm64.REGTMP;
                p3.To.Type = obj.TYPE_BRANCH;
                gc.Patch(p3, p);
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARM64MOVBreg || v.Op == ssa.OpARM64MOVBUreg || v.Op == ssa.OpARM64MOVHreg || v.Op == ssa.OpARM64MOVHUreg || v.Op == ssa.OpARM64MOVWreg || v.Op == ssa.OpARM64MOVWUreg)
            {
                var a = v.Args[0L];
                while (a.Op == ssa.OpCopy || a.Op == ssa.OpARM64MOVDreg)
                {
                    a = a.Args[0L];
                }

                if (a.Op == ssa.OpLoadReg)
                {
                    var t = a.Type;

                    if (v.Op == ssa.OpARM64MOVBreg && t.Size() == 1L && t.IsSigned() || v.Op == ssa.OpARM64MOVBUreg && t.Size() == 1L && !t.IsSigned() || v.Op == ssa.OpARM64MOVHreg && t.Size() == 2L && t.IsSigned() || v.Op == ssa.OpARM64MOVHUreg && t.Size() == 2L && !t.IsSigned() || v.Op == ssa.OpARM64MOVWreg && t.Size() == 4L && t.IsSigned() || v.Op == ssa.OpARM64MOVWUreg && t.Size() == 4L && !t.IsSigned()) 
                        // arg is a proper-typed load, already zero/sign-extended, don't extend again
                        if (v.Reg() == v.Args[0L].Reg())
                        {
                            return ;
                        }

                        p = s.Prog(arm64.AMOVD);
                        p.From.Type = obj.TYPE_REG;
                        p.From.Reg = v.Args[0L].Reg();
                        p.To.Type = obj.TYPE_REG;
                        p.To.Reg = v.Reg();
                        return ;
                    else                     
                }

                fallthrough = true;
            }
            if (fallthrough || v.Op == ssa.OpARM64MVN || v.Op == ssa.OpARM64NEG || v.Op == ssa.OpARM64FABSD || v.Op == ssa.OpARM64FMOVDfpgp || v.Op == ssa.OpARM64FMOVDgpfp || v.Op == ssa.OpARM64FMOVSfpgp || v.Op == ssa.OpARM64FMOVSgpfp || v.Op == ssa.OpARM64FNEGS || v.Op == ssa.OpARM64FNEGD || v.Op == ssa.OpARM64FSQRTD || v.Op == ssa.OpARM64FCVTZSSW || v.Op == ssa.OpARM64FCVTZSDW || v.Op == ssa.OpARM64FCVTZUSW || v.Op == ssa.OpARM64FCVTZUDW || v.Op == ssa.OpARM64FCVTZSS || v.Op == ssa.OpARM64FCVTZSD || v.Op == ssa.OpARM64FCVTZUS || v.Op == ssa.OpARM64FCVTZUD || v.Op == ssa.OpARM64SCVTFWS || v.Op == ssa.OpARM64SCVTFWD || v.Op == ssa.OpARM64SCVTFS || v.Op == ssa.OpARM64SCVTFD || v.Op == ssa.OpARM64UCVTFWS || v.Op == ssa.OpARM64UCVTFWD || v.Op == ssa.OpARM64UCVTFS || v.Op == ssa.OpARM64UCVTFD || v.Op == ssa.OpARM64FCVTSD || v.Op == ssa.OpARM64FCVTDS || v.Op == ssa.OpARM64REV || v.Op == ssa.OpARM64REVW || v.Op == ssa.OpARM64REV16W || v.Op == ssa.OpARM64RBIT || v.Op == ssa.OpARM64RBITW || v.Op == ssa.OpARM64CLZ || v.Op == ssa.OpARM64CLZW || v.Op == ssa.OpARM64FRINTAD || v.Op == ssa.OpARM64FRINTMD || v.Op == ssa.OpARM64FRINTND || v.Op == ssa.OpARM64FRINTPD || v.Op == ssa.OpARM64FRINTZD)
            {
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = v.Args[0L].Reg();
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARM64LoweredRound32F || v.Op == ssa.OpARM64LoweredRound64F)
            {
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARM64VCNT)
            {
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = (v.Args[0L].Reg() - arm64.REG_F0) & 31L + arm64.REG_ARNG + ((arm64.ARNG_8B & 15L) << (int)(5L));
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = (v.Reg() - arm64.REG_F0) & 31L + arm64.REG_ARNG + ((arm64.ARNG_8B & 15L) << (int)(5L));
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARM64VUADDLV)
            {
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = (v.Args[0L].Reg() - arm64.REG_F0) & 31L + arm64.REG_ARNG + ((arm64.ARNG_8B & 15L) << (int)(5L));
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg() - arm64.REG_F0 + arm64.REG_V0;
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARM64CSEL || v.Op == ssa.OpARM64CSEL0)
            {
                r1 = int16(arm64.REGZERO);
                if (v.Op != ssa.OpARM64CSEL0)
                {
                    r1 = v.Args[1L].Reg();
                }

                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_REG; // assembler encodes conditional bits in Reg
                p.From.Reg = condBits[v.Aux._<ssa.Op>()];
                p.Reg = v.Args[0L].Reg();
                p.SetFrom3(new obj.Addr(Type:obj.TYPE_REG,Reg:r1));
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARM64DUFFZERO) 
            {
                // runtime.duffzero expects start address in R20
                p = s.Prog(obj.ADUFFZERO);
                p.To.Type = obj.TYPE_MEM;
                p.To.Name = obj.NAME_EXTERN;
                p.To.Sym = gc.Duffzero;
                p.To.Offset = v.AuxInt;
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARM64LoweredZero) 
            {
                // STP.P    (ZR,ZR), 16(R16)
                // CMP    Rarg1, R16
                // BLE    -2(PC)
                // arg1 is the address of the last 16-byte unit to zero
                p = s.Prog(arm64.ASTP);
                p.Scond = arm64.C_XPOST;
                p.From.Type = obj.TYPE_REGREG;
                p.From.Reg = arm64.REGZERO;
                p.From.Offset = int64(arm64.REGZERO);
                p.To.Type = obj.TYPE_MEM;
                p.To.Reg = arm64.REG_R16;
                p.To.Offset = 16L;
                p2 = s.Prog(arm64.ACMP);
                p2.From.Type = obj.TYPE_REG;
                p2.From.Reg = v.Args[1L].Reg();
                p2.Reg = arm64.REG_R16;
                p3 = s.Prog(arm64.ABLE);
                p3.To.Type = obj.TYPE_BRANCH;
                gc.Patch(p3, p);
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARM64DUFFCOPY)
            {
                p = s.Prog(obj.ADUFFCOPY);
                p.To.Type = obj.TYPE_MEM;
                p.To.Name = obj.NAME_EXTERN;
                p.To.Sym = gc.Duffcopy;
                p.To.Offset = v.AuxInt;
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARM64LoweredMove) 
            {
                // MOVD.P    8(R16), Rtmp
                // MOVD.P    Rtmp, 8(R17)
                // CMP    Rarg2, R16
                // BLE    -3(PC)
                // arg2 is the address of the last element of src
                p = s.Prog(arm64.AMOVD);
                p.Scond = arm64.C_XPOST;
                p.From.Type = obj.TYPE_MEM;
                p.From.Reg = arm64.REG_R16;
                p.From.Offset = 8L;
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = arm64.REGTMP;
                p2 = s.Prog(arm64.AMOVD);
                p2.Scond = arm64.C_XPOST;
                p2.From.Type = obj.TYPE_REG;
                p2.From.Reg = arm64.REGTMP;
                p2.To.Type = obj.TYPE_MEM;
                p2.To.Reg = arm64.REG_R17;
                p2.To.Offset = 8L;
                p3 = s.Prog(arm64.ACMP);
                p3.From.Type = obj.TYPE_REG;
                p3.From.Reg = v.Args[2L].Reg();
                p3.Reg = arm64.REG_R16;
                p4 = s.Prog(arm64.ABLE);
                p4.To.Type = obj.TYPE_BRANCH;
                gc.Patch(p4, p);
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARM64CALLstatic || v.Op == ssa.OpARM64CALLclosure || v.Op == ssa.OpARM64CALLinter)
            {
                s.Call(v);
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARM64LoweredWB)
            {
                p = s.Prog(obj.ACALL);
                p.To.Type = obj.TYPE_MEM;
                p.To.Name = obj.NAME_EXTERN;
                p.To.Sym = v.Aux._<ptr<obj.LSym>>();
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARM64LoweredPanicBoundsA || v.Op == ssa.OpARM64LoweredPanicBoundsB || v.Op == ssa.OpARM64LoweredPanicBoundsC)
            {
                p = s.Prog(obj.ACALL);
                p.To.Type = obj.TYPE_MEM;
                p.To.Name = obj.NAME_EXTERN;
                p.To.Sym = gc.BoundsCheckFunc[v.AuxInt];
                s.UseArgs(16L); // space used in callee args area by assembly stubs
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARM64LoweredNilCheck) 
            {
                // Issue a load which will fault if arg is nil.
                p = s.Prog(arm64.AMOVB);
                p.From.Type = obj.TYPE_MEM;
                p.From.Reg = v.Args[0L].Reg();
                gc.AddAux(_addr_p.From, v);
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = arm64.REGTMP;
                if (logopt.Enabled())
                {
                    logopt.LogOpt(v.Pos, "nilcheck", "genssa", v.Block.Func.Name);
                }

                if (gc.Debug_checknil != 0L && v.Pos.Line() > 1L)
                { // v.Line==1 in generated wrappers
                    gc.Warnl(v.Pos, "generated nil check");

                }

                goto __switch_break0;
            }
            if (v.Op == ssa.OpARM64Equal || v.Op == ssa.OpARM64NotEqual || v.Op == ssa.OpARM64LessThan || v.Op == ssa.OpARM64LessEqual || v.Op == ssa.OpARM64GreaterThan || v.Op == ssa.OpARM64GreaterEqual || v.Op == ssa.OpARM64LessThanU || v.Op == ssa.OpARM64LessEqualU || v.Op == ssa.OpARM64GreaterThanU || v.Op == ssa.OpARM64GreaterEqualU || v.Op == ssa.OpARM64LessThanF || v.Op == ssa.OpARM64LessEqualF || v.Op == ssa.OpARM64GreaterThanF || v.Op == ssa.OpARM64GreaterEqualF) 
            {
                // generate boolean values using CSET
                p = s.Prog(arm64.ACSET);
                p.From.Type = obj.TYPE_REG; // assembler encodes conditional bits in Reg
                p.From.Reg = condBits[v.Op];
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARM64LoweredGetClosurePtr) 
            {
                // Closure pointer is R26 (arm64.REGCTXT).
                gc.CheckLoweredGetClosurePtr(v);
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARM64LoweredGetCallerSP) 
            {
                // caller's SP is FixedFrameSize below the address of the first arg
                p = s.Prog(arm64.AMOVD);
                p.From.Type = obj.TYPE_ADDR;
                p.From.Offset = -gc.Ctxt.FixedFrameSize();
                p.From.Name = obj.NAME_PARAM;
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARM64LoweredGetCallerPC)
            {
                p = s.Prog(obj.AGETCALLERPC);
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARM64FlagConstant)
            {
                v.Fatalf("FlagConstant op should never make it to codegen %v", v.LongString());
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARM64InvertFlags)
            {
                v.Fatalf("InvertFlags should never make it to codegen %v", v.LongString());
                goto __switch_break0;
            }
            if (v.Op == ssa.OpClobber)
            {
                goto __switch_break0;
            }
            // default: 
                v.Fatalf("genValue not implemented: %s", v.LongString());

            __switch_break0:;

        });

        private static map condBits = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ssa.Op, short>{ssa.OpARM64Equal:arm64.COND_EQ,ssa.OpARM64NotEqual:arm64.COND_NE,ssa.OpARM64LessThan:arm64.COND_LT,ssa.OpARM64LessThanU:arm64.COND_LO,ssa.OpARM64LessEqual:arm64.COND_LE,ssa.OpARM64LessEqualU:arm64.COND_LS,ssa.OpARM64GreaterThan:arm64.COND_GT,ssa.OpARM64GreaterThanU:arm64.COND_HI,ssa.OpARM64GreaterEqual:arm64.COND_GE,ssa.OpARM64GreaterEqualU:arm64.COND_HS,ssa.OpARM64LessThanF:arm64.COND_MI,ssa.OpARM64LessEqualF:arm64.COND_LS,ssa.OpARM64GreaterThanF:arm64.COND_GT,ssa.OpARM64GreaterEqualF:arm64.COND_GE,};



        // To model a 'LEnoov' ('<=' without overflow checking) branching
        private static array<array<gc.IndexJump>> leJumps = new array<array<gc.IndexJump>>(new array<gc.IndexJump>[] { {{Jump:arm64.ABEQ,Index:0},{Jump:arm64.ABPL,Index:1}}, {{Jump:arm64.ABMI,Index:0},{Jump:arm64.ABEQ,Index:0}} });

        // To model a 'GTnoov' ('>' without overflow checking) branching
        private static array<array<gc.IndexJump>> gtJumps = new array<array<gc.IndexJump>>(new array<gc.IndexJump>[] { {{Jump:arm64.ABMI,Index:1},{Jump:arm64.ABEQ,Index:1}}, {{Jump:arm64.ABEQ,Index:1},{Jump:arm64.ABPL,Index:0}} });

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
                // defer returns in R0:
                // 0 if we should continue executing
                // 1 if we should jump to deferreturn call
                p = s.Prog(arm64.ACMP);
                p.From.Type = obj.TYPE_CONST;
                p.From.Offset = 0L;
                p.Reg = arm64.REG_R0;
                p = s.Prog(arm64.ABNE);
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
            else if (b.Kind == ssa.BlockARM64EQ || b.Kind == ssa.BlockARM64NE || b.Kind == ssa.BlockARM64LT || b.Kind == ssa.BlockARM64GE || b.Kind == ssa.BlockARM64LE || b.Kind == ssa.BlockARM64GT || b.Kind == ssa.BlockARM64ULT || b.Kind == ssa.BlockARM64UGT || b.Kind == ssa.BlockARM64ULE || b.Kind == ssa.BlockARM64UGE || b.Kind == ssa.BlockARM64Z || b.Kind == ssa.BlockARM64NZ || b.Kind == ssa.BlockARM64ZW || b.Kind == ssa.BlockARM64NZW || b.Kind == ssa.BlockARM64FLT || b.Kind == ssa.BlockARM64FGE || b.Kind == ssa.BlockARM64FLE || b.Kind == ssa.BlockARM64FGT || b.Kind == ssa.BlockARM64LTnoov || b.Kind == ssa.BlockARM64GEnoov) 
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

            else if (b.Kind == ssa.BlockARM64TBZ || b.Kind == ssa.BlockARM64TBNZ) 
                jmp = blockJump[b.Kind];
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

                                p.From.Offset = b.AuxInt;
                p.From.Type = obj.TYPE_CONST;
                p.Reg = b.Controls[0L].Reg();
            else if (b.Kind == ssa.BlockARM64LEnoov) 
                s.CombJump(b, next, _addr_leJumps);
            else if (b.Kind == ssa.BlockARM64GTnoov) 
                s.CombJump(b, next, _addr_gtJumps);
            else 
                b.Fatalf("branch not implemented: %s", b.LongString());
            
        }
    }
}}}}
