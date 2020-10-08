// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package arm -- go2cs converted at 2020 October 08 04:32:18 UTC
// import "cmd/compile/internal/arm" ==> using arm = go.cmd.compile.@internal.arm_package
// Original source: C:\Go\src\cmd\compile\internal\arm\ssa.go
using fmt = go.fmt_package;
using math = go.math_package;
using bits = go.math.bits_package;

using gc = go.cmd.compile.@internal.gc_package;
using logopt = go.cmd.compile.@internal.logopt_package;
using ssa = go.cmd.compile.@internal.ssa_package;
using types = go.cmd.compile.@internal.types_package;
using obj = go.cmd.@internal.obj_package;
using arm = go.cmd.@internal.obj.arm_package;
using objabi = go.cmd.@internal.objabi_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class arm_package
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
                        return arm.AMOVF;
                        break;
                    case 8L: 
                        return arm.AMOVD;
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
                            return arm.AMOVB;
                        }
                        else
                        {
                            return arm.AMOVBU;
                        }
                        break;
                    case 2L: 
                        if (t.IsSigned())
                        {
                            return arm.AMOVH;
                        }
                        else
                        {
                            return arm.AMOVHU;
                        }
                        break;
                    case 4L: 
                        return arm.AMOVW;
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
                        return arm.AMOVF;
                        break;
                    case 8L: 
                        return arm.AMOVD;
                        break;
                }

            }
            else
            {
                switch (t.Size())
                {
                    case 1L: 
                        return arm.AMOVB;
                        break;
                    case 2L: 
                        return arm.AMOVH;
                        break;
                    case 4L: 
                        return arm.AMOVW;
                        break;
                }

            }

            panic("bad store type");

        });

        // shift type is used as Offset in obj.TYPE_SHIFT operands to encode shifted register operands
        private partial struct shift // : long
        {
        }

        // copied from ../../../internal/obj/util.go:/TYPE_SHIFT
        private static @string String(this shift v)
        {
            @string op = "<<>>->@>"[((v >> (int)(5L)) & 3L) << (int)(1L)..];
            if (v & (1L << (int)(4L)) != 0L)
            { 
                // register shift
                return fmt.Sprintf("R%d%c%cR%d", v & 15L, op[0L], op[1L], (v >> (int)(8L)) & 15L);

            }
            else
            { 
                // constant shift
                return fmt.Sprintf("R%d%c%c%d", v & 15L, op[0L], op[1L], (v >> (int)(7L)) & 31L);

            }

        }

        // makeshift encodes a register shifted by a constant
        private static shift makeshift(short reg, long typ, long s)
        {
            return shift(int64(reg & 0xfUL) | typ | (s & 31L) << (int)(7L));
        }

        // genshift generates a Prog for r = r0 op (r1 shifted by n)
        private static ptr<obj.Prog> genshift(ptr<gc.SSAGenState> _addr_s, obj.As @as, short r0, short r1, short r, long typ, long n)
        {
            ref gc.SSAGenState s = ref _addr_s.val;

            var p = s.Prog(as);
            p.From.Type = obj.TYPE_SHIFT;
            p.From.Offset = int64(makeshift(r1, typ, n));
            p.Reg = r0;
            if (r != 0L)
            {
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = r;
            }

            return _addr_p!;

        }

        // makeregshift encodes a register shifted by a register
        private static shift makeregshift(short r1, long typ, short r2)
        {
            return shift(int64(r1 & 0xfUL) | typ | int64(r2 & 0xfUL) << (int)(8L) | 1L << (int)(4L));
        }

        // genregshift generates a Prog for r = r0 op (r1 shifted by r2)
        private static ptr<obj.Prog> genregshift(ptr<gc.SSAGenState> _addr_s, obj.As @as, short r0, short r1, short r2, short r, long typ)
        {
            ref gc.SSAGenState s = ref _addr_s.val;

            var p = s.Prog(as);
            p.From.Type = obj.TYPE_SHIFT;
            p.From.Offset = int64(makeregshift(r1, typ, r2));
            p.Reg = r0;
            if (r != 0L)
            {
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = r;
            }

            return _addr_p!;

        }

        // find a (lsb, width) pair for BFC
        // lsb must be in [0, 31], width must be in [1, 32 - lsb]
        // return (0xffffffff, 0) if v is not a binary like 0...01...10...0
        private static (uint, uint) getBFC(uint v)
        {
            uint _p0 = default;
            uint _p0 = default;

            uint m = default;            uint l = default; 
            // BFC is not applicable with zero
 
            // BFC is not applicable with zero
            if (v == 0L)
            {
                return (0xffffffffUL, 0L);
            } 
            // find the lowest set bit, for example l=2 for 0x3ffffffc
            l = uint32(bits.TrailingZeros32(v)); 
            // m-1 represents the highest set bit index, for example m=30 for 0x3ffffffc
            m = 32L - uint32(bits.LeadingZeros32(v)); 
            // check if v is a binary like 0...01...10...0
            if ((1L << (int)(m)) - (1L << (int)(l)) == v)
            { 
                // it must be m > l for non-zero v
                return (l, m - l);

            } 
            // invalid
            return (0xffffffffUL, 0L);

        }

        private static void ssaGenValue(ptr<gc.SSAGenState> _addr_s, ptr<ssa.Value> _addr_v) => func((_, panic, __) =>
        {
            ref gc.SSAGenState s = ref _addr_s.val;
            ref ssa.Value v = ref _addr_v.val;


            if (v.Op == ssa.OpCopy || v.Op == ssa.OpARMMOVWreg)
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

                var @as = arm.AMOVW;
                if (v.Type.IsFloat())
                {
                    switch (v.Type.Size())
                    {
                        case 4L: 
                            as = arm.AMOVF;
                            break;
                        case 8L: 
                            as = arm.AMOVD;
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
            if (v.Op == ssa.OpARMMOVWnop)
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
            if (v.Op == ssa.OpARMADD || v.Op == ssa.OpARMADC || v.Op == ssa.OpARMSUB || v.Op == ssa.OpARMSBC || v.Op == ssa.OpARMRSB || v.Op == ssa.OpARMAND || v.Op == ssa.OpARMOR || v.Op == ssa.OpARMXOR || v.Op == ssa.OpARMBIC || v.Op == ssa.OpARMMUL || v.Op == ssa.OpARMADDF || v.Op == ssa.OpARMADDD || v.Op == ssa.OpARMSUBF || v.Op == ssa.OpARMSUBD || v.Op == ssa.OpARMSLL || v.Op == ssa.OpARMSRL || v.Op == ssa.OpARMSRA || v.Op == ssa.OpARMMULF || v.Op == ssa.OpARMMULD || v.Op == ssa.OpARMNMULF || v.Op == ssa.OpARMNMULD || v.Op == ssa.OpARMDIVF || v.Op == ssa.OpARMDIVD)
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
            if (v.Op == ssa.OpARMSRR)
            {
                genregshift(_addr_s, arm.AMOVW, 0L, v.Args[0L].Reg(), v.Args[1L].Reg(), v.Reg(), arm.SHIFT_RR);
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARMMULAF || v.Op == ssa.OpARMMULAD || v.Op == ssa.OpARMMULSF || v.Op == ssa.OpARMMULSD || v.Op == ssa.OpARMFMULAD)
            {
                r = v.Reg();
                var r0 = v.Args[0L].Reg();
                r1 = v.Args[1L].Reg();
                r2 = v.Args[2L].Reg();
                if (r != r0)
                {
                    v.Fatalf("result and addend are not in the same register: %v", v.LongString());
                }

                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = r2;
                p.Reg = r1;
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = r;
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARMADDS || v.Op == ssa.OpARMSUBS)
            {
                r = v.Reg0();
                r1 = v.Args[0L].Reg();
                r2 = v.Args[1L].Reg();
                p = s.Prog(v.Op.Asm());
                p.Scond = arm.C_SBIT;
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = r2;
                p.Reg = r1;
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = r;
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARMSRAcond) 
            {
                // ARM shift instructions uses only the low-order byte of the shift amount
                // generate conditional instructions to deal with large shifts
                // flag is already set
                // SRA.HS    $31, Rarg0, Rdst // shift 31 bits to get the sign bit
                // SRA.LO    Rarg1, Rarg0, Rdst
                r = v.Reg();
                r1 = v.Args[0L].Reg();
                r2 = v.Args[1L].Reg();
                p = s.Prog(arm.ASRA);
                p.Scond = arm.C_SCOND_HS;
                p.From.Type = obj.TYPE_CONST;
                p.From.Offset = 31L;
                p.Reg = r1;
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = r;
                p = s.Prog(arm.ASRA);
                p.Scond = arm.C_SCOND_LO;
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = r2;
                p.Reg = r1;
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = r;
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARMBFX || v.Op == ssa.OpARMBFXU)
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
            if (v.Op == ssa.OpARMANDconst || v.Op == ssa.OpARMBICconst) 
            {
                // try to optimize ANDconst and BICconst to BFC, which saves bytes and ticks
                // BFC is only available on ARMv7, and its result and source are in the same register
                if (objabi.GOARM == 7L && v.Reg() == v.Args[0L].Reg())
                {
                    uint val = default;
                    if (v.Op == ssa.OpARMANDconst)
                    {
                        val = ~uint32(v.AuxInt);
                    }
                    else
                    { // BICconst
                        val = uint32(v.AuxInt);

                    }

                    var (lsb, width) = getBFC(val); 
                    // omit BFC for ARM's imm12
                    if (8L < width && width < 24L)
                    {
                        p = s.Prog(arm.ABFC);
                        p.From.Type = obj.TYPE_CONST;
                        p.From.Offset = int64(width);
                        p.SetFrom3(new obj.Addr(Type:obj.TYPE_CONST,Offset:int64(lsb)));
                        p.To.Type = obj.TYPE_REG;
                        p.To.Reg = v.Reg();
                        break;
                    }

                } 
                // fall back to ordinary form
                fallthrough = true;
            }
            if (fallthrough || v.Op == ssa.OpARMADDconst || v.Op == ssa.OpARMADCconst || v.Op == ssa.OpARMSUBconst || v.Op == ssa.OpARMSBCconst || v.Op == ssa.OpARMRSBconst || v.Op == ssa.OpARMRSCconst || v.Op == ssa.OpARMORconst || v.Op == ssa.OpARMXORconst || v.Op == ssa.OpARMSLLconst || v.Op == ssa.OpARMSRLconst || v.Op == ssa.OpARMSRAconst)
            {
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_CONST;
                p.From.Offset = v.AuxInt;
                p.Reg = v.Args[0L].Reg();
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARMADDSconst || v.Op == ssa.OpARMSUBSconst || v.Op == ssa.OpARMRSBSconst)
            {
                p = s.Prog(v.Op.Asm());
                p.Scond = arm.C_SBIT;
                p.From.Type = obj.TYPE_CONST;
                p.From.Offset = v.AuxInt;
                p.Reg = v.Args[0L].Reg();
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg0();
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARMSRRconst)
            {
                genshift(_addr_s, arm.AMOVW, 0L, v.Args[0L].Reg(), v.Reg(), arm.SHIFT_RR, v.AuxInt);
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARMADDshiftLL || v.Op == ssa.OpARMADCshiftLL || v.Op == ssa.OpARMSUBshiftLL || v.Op == ssa.OpARMSBCshiftLL || v.Op == ssa.OpARMRSBshiftLL || v.Op == ssa.OpARMRSCshiftLL || v.Op == ssa.OpARMANDshiftLL || v.Op == ssa.OpARMORshiftLL || v.Op == ssa.OpARMXORshiftLL || v.Op == ssa.OpARMBICshiftLL)
            {
                genshift(_addr_s, v.Op.Asm(), v.Args[0L].Reg(), v.Args[1L].Reg(), v.Reg(), arm.SHIFT_LL, v.AuxInt);
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARMADDSshiftLL || v.Op == ssa.OpARMSUBSshiftLL || v.Op == ssa.OpARMRSBSshiftLL)
            {
                p = genshift(_addr_s, v.Op.Asm(), v.Args[0L].Reg(), v.Args[1L].Reg(), v.Reg0(), arm.SHIFT_LL, v.AuxInt);
                p.Scond = arm.C_SBIT;
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARMADDshiftRL || v.Op == ssa.OpARMADCshiftRL || v.Op == ssa.OpARMSUBshiftRL || v.Op == ssa.OpARMSBCshiftRL || v.Op == ssa.OpARMRSBshiftRL || v.Op == ssa.OpARMRSCshiftRL || v.Op == ssa.OpARMANDshiftRL || v.Op == ssa.OpARMORshiftRL || v.Op == ssa.OpARMXORshiftRL || v.Op == ssa.OpARMBICshiftRL)
            {
                genshift(_addr_s, v.Op.Asm(), v.Args[0L].Reg(), v.Args[1L].Reg(), v.Reg(), arm.SHIFT_LR, v.AuxInt);
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARMADDSshiftRL || v.Op == ssa.OpARMSUBSshiftRL || v.Op == ssa.OpARMRSBSshiftRL)
            {
                p = genshift(_addr_s, v.Op.Asm(), v.Args[0L].Reg(), v.Args[1L].Reg(), v.Reg0(), arm.SHIFT_LR, v.AuxInt);
                p.Scond = arm.C_SBIT;
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARMADDshiftRA || v.Op == ssa.OpARMADCshiftRA || v.Op == ssa.OpARMSUBshiftRA || v.Op == ssa.OpARMSBCshiftRA || v.Op == ssa.OpARMRSBshiftRA || v.Op == ssa.OpARMRSCshiftRA || v.Op == ssa.OpARMANDshiftRA || v.Op == ssa.OpARMORshiftRA || v.Op == ssa.OpARMXORshiftRA || v.Op == ssa.OpARMBICshiftRA)
            {
                genshift(_addr_s, v.Op.Asm(), v.Args[0L].Reg(), v.Args[1L].Reg(), v.Reg(), arm.SHIFT_AR, v.AuxInt);
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARMADDSshiftRA || v.Op == ssa.OpARMSUBSshiftRA || v.Op == ssa.OpARMRSBSshiftRA)
            {
                p = genshift(_addr_s, v.Op.Asm(), v.Args[0L].Reg(), v.Args[1L].Reg(), v.Reg0(), arm.SHIFT_AR, v.AuxInt);
                p.Scond = arm.C_SBIT;
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARMXORshiftRR)
            {
                genshift(_addr_s, v.Op.Asm(), v.Args[0L].Reg(), v.Args[1L].Reg(), v.Reg(), arm.SHIFT_RR, v.AuxInt);
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARMMVNshiftLL)
            {
                genshift(_addr_s, v.Op.Asm(), 0L, v.Args[0L].Reg(), v.Reg(), arm.SHIFT_LL, v.AuxInt);
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARMMVNshiftRL)
            {
                genshift(_addr_s, v.Op.Asm(), 0L, v.Args[0L].Reg(), v.Reg(), arm.SHIFT_LR, v.AuxInt);
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARMMVNshiftRA)
            {
                genshift(_addr_s, v.Op.Asm(), 0L, v.Args[0L].Reg(), v.Reg(), arm.SHIFT_AR, v.AuxInt);
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARMMVNshiftLLreg)
            {
                genregshift(_addr_s, v.Op.Asm(), 0L, v.Args[0L].Reg(), v.Args[1L].Reg(), v.Reg(), arm.SHIFT_LL);
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARMMVNshiftRLreg)
            {
                genregshift(_addr_s, v.Op.Asm(), 0L, v.Args[0L].Reg(), v.Args[1L].Reg(), v.Reg(), arm.SHIFT_LR);
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARMMVNshiftRAreg)
            {
                genregshift(_addr_s, v.Op.Asm(), 0L, v.Args[0L].Reg(), v.Args[1L].Reg(), v.Reg(), arm.SHIFT_AR);
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARMADDshiftLLreg || v.Op == ssa.OpARMADCshiftLLreg || v.Op == ssa.OpARMSUBshiftLLreg || v.Op == ssa.OpARMSBCshiftLLreg || v.Op == ssa.OpARMRSBshiftLLreg || v.Op == ssa.OpARMRSCshiftLLreg || v.Op == ssa.OpARMANDshiftLLreg || v.Op == ssa.OpARMORshiftLLreg || v.Op == ssa.OpARMXORshiftLLreg || v.Op == ssa.OpARMBICshiftLLreg)
            {
                genregshift(_addr_s, v.Op.Asm(), v.Args[0L].Reg(), v.Args[1L].Reg(), v.Args[2L].Reg(), v.Reg(), arm.SHIFT_LL);
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARMADDSshiftLLreg || v.Op == ssa.OpARMSUBSshiftLLreg || v.Op == ssa.OpARMRSBSshiftLLreg)
            {
                p = genregshift(_addr_s, v.Op.Asm(), v.Args[0L].Reg(), v.Args[1L].Reg(), v.Args[2L].Reg(), v.Reg0(), arm.SHIFT_LL);
                p.Scond = arm.C_SBIT;
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARMADDshiftRLreg || v.Op == ssa.OpARMADCshiftRLreg || v.Op == ssa.OpARMSUBshiftRLreg || v.Op == ssa.OpARMSBCshiftRLreg || v.Op == ssa.OpARMRSBshiftRLreg || v.Op == ssa.OpARMRSCshiftRLreg || v.Op == ssa.OpARMANDshiftRLreg || v.Op == ssa.OpARMORshiftRLreg || v.Op == ssa.OpARMXORshiftRLreg || v.Op == ssa.OpARMBICshiftRLreg)
            {
                genregshift(_addr_s, v.Op.Asm(), v.Args[0L].Reg(), v.Args[1L].Reg(), v.Args[2L].Reg(), v.Reg(), arm.SHIFT_LR);
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARMADDSshiftRLreg || v.Op == ssa.OpARMSUBSshiftRLreg || v.Op == ssa.OpARMRSBSshiftRLreg)
            {
                p = genregshift(_addr_s, v.Op.Asm(), v.Args[0L].Reg(), v.Args[1L].Reg(), v.Args[2L].Reg(), v.Reg0(), arm.SHIFT_LR);
                p.Scond = arm.C_SBIT;
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARMADDshiftRAreg || v.Op == ssa.OpARMADCshiftRAreg || v.Op == ssa.OpARMSUBshiftRAreg || v.Op == ssa.OpARMSBCshiftRAreg || v.Op == ssa.OpARMRSBshiftRAreg || v.Op == ssa.OpARMRSCshiftRAreg || v.Op == ssa.OpARMANDshiftRAreg || v.Op == ssa.OpARMORshiftRAreg || v.Op == ssa.OpARMXORshiftRAreg || v.Op == ssa.OpARMBICshiftRAreg)
            {
                genregshift(_addr_s, v.Op.Asm(), v.Args[0L].Reg(), v.Args[1L].Reg(), v.Args[2L].Reg(), v.Reg(), arm.SHIFT_AR);
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARMADDSshiftRAreg || v.Op == ssa.OpARMSUBSshiftRAreg || v.Op == ssa.OpARMRSBSshiftRAreg)
            {
                p = genregshift(_addr_s, v.Op.Asm(), v.Args[0L].Reg(), v.Args[1L].Reg(), v.Args[2L].Reg(), v.Reg0(), arm.SHIFT_AR);
                p.Scond = arm.C_SBIT;
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARMHMUL || v.Op == ssa.OpARMHMULU) 
            {
                // 32-bit high multiplication
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = v.Args[0L].Reg();
                p.Reg = v.Args[1L].Reg();
                p.To.Type = obj.TYPE_REGREG;
                p.To.Reg = v.Reg();
                p.To.Offset = arm.REGTMP; // throw away low 32-bit into tmp register
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARMMULLU) 
            {
                // 32-bit multiplication, results 64-bit, high 32-bit in out0, low 32-bit in out1
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = v.Args[0L].Reg();
                p.Reg = v.Args[1L].Reg();
                p.To.Type = obj.TYPE_REGREG;
                p.To.Reg = v.Reg0(); // high 32-bit
                p.To.Offset = int64(v.Reg1()); // low 32-bit
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARMMULA || v.Op == ssa.OpARMMULS)
            {
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = v.Args[0L].Reg();
                p.Reg = v.Args[1L].Reg();
                p.To.Type = obj.TYPE_REGREG2;
                p.To.Reg = v.Reg(); // result
                p.To.Offset = int64(v.Args[2L].Reg()); // addend
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARMMOVWconst)
            {
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_CONST;
                p.From.Offset = v.AuxInt;
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARMMOVFconst || v.Op == ssa.OpARMMOVDconst)
            {
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_FCONST;
                p.From.Val = math.Float64frombits(uint64(v.AuxInt));
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARMCMP || v.Op == ssa.OpARMCMN || v.Op == ssa.OpARMTST || v.Op == ssa.OpARMTEQ || v.Op == ssa.OpARMCMPF || v.Op == ssa.OpARMCMPD)
            {
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_REG; 
                // Special layout in ARM assembly
                // Comparing to x86, the operands of ARM's CMP are reversed.
                p.From.Reg = v.Args[1L].Reg();
                p.Reg = v.Args[0L].Reg();
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARMCMPconst || v.Op == ssa.OpARMCMNconst || v.Op == ssa.OpARMTSTconst || v.Op == ssa.OpARMTEQconst) 
            {
                // Special layout in ARM assembly
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_CONST;
                p.From.Offset = v.AuxInt;
                p.Reg = v.Args[0L].Reg();
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARMCMPF0 || v.Op == ssa.OpARMCMPD0)
            {
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = v.Args[0L].Reg();
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARMCMPshiftLL || v.Op == ssa.OpARMCMNshiftLL || v.Op == ssa.OpARMTSTshiftLL || v.Op == ssa.OpARMTEQshiftLL)
            {
                genshift(_addr_s, v.Op.Asm(), v.Args[0L].Reg(), v.Args[1L].Reg(), 0L, arm.SHIFT_LL, v.AuxInt);
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARMCMPshiftRL || v.Op == ssa.OpARMCMNshiftRL || v.Op == ssa.OpARMTSTshiftRL || v.Op == ssa.OpARMTEQshiftRL)
            {
                genshift(_addr_s, v.Op.Asm(), v.Args[0L].Reg(), v.Args[1L].Reg(), 0L, arm.SHIFT_LR, v.AuxInt);
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARMCMPshiftRA || v.Op == ssa.OpARMCMNshiftRA || v.Op == ssa.OpARMTSTshiftRA || v.Op == ssa.OpARMTEQshiftRA)
            {
                genshift(_addr_s, v.Op.Asm(), v.Args[0L].Reg(), v.Args[1L].Reg(), 0L, arm.SHIFT_AR, v.AuxInt);
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARMCMPshiftLLreg || v.Op == ssa.OpARMCMNshiftLLreg || v.Op == ssa.OpARMTSTshiftLLreg || v.Op == ssa.OpARMTEQshiftLLreg)
            {
                genregshift(_addr_s, v.Op.Asm(), v.Args[0L].Reg(), v.Args[1L].Reg(), v.Args[2L].Reg(), 0L, arm.SHIFT_LL);
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARMCMPshiftRLreg || v.Op == ssa.OpARMCMNshiftRLreg || v.Op == ssa.OpARMTSTshiftRLreg || v.Op == ssa.OpARMTEQshiftRLreg)
            {
                genregshift(_addr_s, v.Op.Asm(), v.Args[0L].Reg(), v.Args[1L].Reg(), v.Args[2L].Reg(), 0L, arm.SHIFT_LR);
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARMCMPshiftRAreg || v.Op == ssa.OpARMCMNshiftRAreg || v.Op == ssa.OpARMTSTshiftRAreg || v.Op == ssa.OpARMTEQshiftRAreg)
            {
                genregshift(_addr_s, v.Op.Asm(), v.Args[0L].Reg(), v.Args[1L].Reg(), v.Args[2L].Reg(), 0L, arm.SHIFT_AR);
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARMMOVWaddr)
            {
                p = s.Prog(arm.AMOVW);
                p.From.Type = obj.TYPE_ADDR;
                p.From.Reg = v.Args[0L].Reg();
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();

                @string wantreg = default; 
                // MOVW $sym+off(base), R
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
            if (v.Op == ssa.OpARMMOVBload || v.Op == ssa.OpARMMOVBUload || v.Op == ssa.OpARMMOVHload || v.Op == ssa.OpARMMOVHUload || v.Op == ssa.OpARMMOVWload || v.Op == ssa.OpARMMOVFload || v.Op == ssa.OpARMMOVDload)
            {
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_MEM;
                p.From.Reg = v.Args[0L].Reg();
                gc.AddAux(_addr_p.From, v);
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARMMOVBstore || v.Op == ssa.OpARMMOVHstore || v.Op == ssa.OpARMMOVWstore || v.Op == ssa.OpARMMOVFstore || v.Op == ssa.OpARMMOVDstore)
            {
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = v.Args[1L].Reg();
                p.To.Type = obj.TYPE_MEM;
                p.To.Reg = v.Args[0L].Reg();
                gc.AddAux(_addr_p.To, v);
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARMMOVWloadidx || v.Op == ssa.OpARMMOVBUloadidx || v.Op == ssa.OpARMMOVBloadidx || v.Op == ssa.OpARMMOVHUloadidx || v.Op == ssa.OpARMMOVHloadidx) 
            {
                // this is just shift 0 bits
                fallthrough = true;
            }
            if (fallthrough || v.Op == ssa.OpARMMOVWloadshiftLL)
            {
                p = genshift(_addr_s, v.Op.Asm(), 0L, v.Args[1L].Reg(), v.Reg(), arm.SHIFT_LL, v.AuxInt);
                p.From.Reg = v.Args[0L].Reg();
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARMMOVWloadshiftRL)
            {
                p = genshift(_addr_s, v.Op.Asm(), 0L, v.Args[1L].Reg(), v.Reg(), arm.SHIFT_LR, v.AuxInt);
                p.From.Reg = v.Args[0L].Reg();
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARMMOVWloadshiftRA)
            {
                p = genshift(_addr_s, v.Op.Asm(), 0L, v.Args[1L].Reg(), v.Reg(), arm.SHIFT_AR, v.AuxInt);
                p.From.Reg = v.Args[0L].Reg();
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARMMOVWstoreidx || v.Op == ssa.OpARMMOVBstoreidx || v.Op == ssa.OpARMMOVHstoreidx) 
            {
                // this is just shift 0 bits
                fallthrough = true;
            }
            if (fallthrough || v.Op == ssa.OpARMMOVWstoreshiftLL)
            {
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = v.Args[2L].Reg();
                p.To.Type = obj.TYPE_SHIFT;
                p.To.Reg = v.Args[0L].Reg();
                p.To.Offset = int64(makeshift(v.Args[1L].Reg(), arm.SHIFT_LL, v.AuxInt));
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARMMOVWstoreshiftRL)
            {
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = v.Args[2L].Reg();
                p.To.Type = obj.TYPE_SHIFT;
                p.To.Reg = v.Args[0L].Reg();
                p.To.Offset = int64(makeshift(v.Args[1L].Reg(), arm.SHIFT_LR, v.AuxInt));
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARMMOVWstoreshiftRA)
            {
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = v.Args[2L].Reg();
                p.To.Type = obj.TYPE_SHIFT;
                p.To.Reg = v.Args[0L].Reg();
                p.To.Offset = int64(makeshift(v.Args[1L].Reg(), arm.SHIFT_AR, v.AuxInt));
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARMMOVBreg || v.Op == ssa.OpARMMOVBUreg || v.Op == ssa.OpARMMOVHreg || v.Op == ssa.OpARMMOVHUreg)
            {
                var a = v.Args[0L];
                while (a.Op == ssa.OpCopy || a.Op == ssa.OpARMMOVWreg || a.Op == ssa.OpARMMOVWnop)
                {
                    a = a.Args[0L];
                }

                if (a.Op == ssa.OpLoadReg)
                {
                    var t = a.Type;

                    if (v.Op == ssa.OpARMMOVBreg && t.Size() == 1L && t.IsSigned() || v.Op == ssa.OpARMMOVBUreg && t.Size() == 1L && !t.IsSigned() || v.Op == ssa.OpARMMOVHreg && t.Size() == 2L && t.IsSigned() || v.Op == ssa.OpARMMOVHUreg && t.Size() == 2L && !t.IsSigned()) 
                        // arg is a proper-typed load, already zero/sign-extended, don't extend again
                        if (v.Reg() == v.Args[0L].Reg())
                        {
                            return ;
                        }

                        p = s.Prog(arm.AMOVW);
                        p.From.Type = obj.TYPE_REG;
                        p.From.Reg = v.Args[0L].Reg();
                        p.To.Type = obj.TYPE_REG;
                        p.To.Reg = v.Reg();
                        return ;
                    else                     
                }

                if (objabi.GOARM >= 6L)
                { 
                    // generate more efficient "MOVB/MOVBU/MOVH/MOVHU Reg@>0, Reg" on ARMv6 & ARMv7
                    genshift(_addr_s, v.Op.Asm(), 0L, v.Args[0L].Reg(), v.Reg(), arm.SHIFT_RR, 0L);
                    return ;

                }

                fallthrough = true;
            }
            if (fallthrough || v.Op == ssa.OpARMMVN || v.Op == ssa.OpARMCLZ || v.Op == ssa.OpARMREV || v.Op == ssa.OpARMREV16 || v.Op == ssa.OpARMRBIT || v.Op == ssa.OpARMSQRTD || v.Op == ssa.OpARMNEGF || v.Op == ssa.OpARMNEGD || v.Op == ssa.OpARMABSD || v.Op == ssa.OpARMMOVWF || v.Op == ssa.OpARMMOVWD || v.Op == ssa.OpARMMOVFW || v.Op == ssa.OpARMMOVDW || v.Op == ssa.OpARMMOVFD || v.Op == ssa.OpARMMOVDF)
            {
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = v.Args[0L].Reg();
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARMMOVWUF || v.Op == ssa.OpARMMOVWUD || v.Op == ssa.OpARMMOVFWU || v.Op == ssa.OpARMMOVDWU)
            {
                p = s.Prog(v.Op.Asm());
                p.Scond = arm.C_UBIT;
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = v.Args[0L].Reg();
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARMCMOVWHSconst)
            {
                p = s.Prog(arm.AMOVW);
                p.Scond = arm.C_SCOND_HS;
                p.From.Type = obj.TYPE_CONST;
                p.From.Offset = v.AuxInt;
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARMCMOVWLSconst)
            {
                p = s.Prog(arm.AMOVW);
                p.Scond = arm.C_SCOND_LS;
                p.From.Type = obj.TYPE_CONST;
                p.From.Offset = v.AuxInt;
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARMCALLstatic || v.Op == ssa.OpARMCALLclosure || v.Op == ssa.OpARMCALLinter)
            {
                s.Call(v);
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARMCALLudiv)
            {
                p = s.Prog(obj.ACALL);
                p.To.Type = obj.TYPE_MEM;
                p.To.Name = obj.NAME_EXTERN;
                p.To.Sym = gc.Udiv;
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARMLoweredWB)
            {
                p = s.Prog(obj.ACALL);
                p.To.Type = obj.TYPE_MEM;
                p.To.Name = obj.NAME_EXTERN;
                p.To.Sym = v.Aux._<ptr<obj.LSym>>();
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARMLoweredPanicBoundsA || v.Op == ssa.OpARMLoweredPanicBoundsB || v.Op == ssa.OpARMLoweredPanicBoundsC)
            {
                p = s.Prog(obj.ACALL);
                p.To.Type = obj.TYPE_MEM;
                p.To.Name = obj.NAME_EXTERN;
                p.To.Sym = gc.BoundsCheckFunc[v.AuxInt];
                s.UseArgs(8L); // space used in callee args area by assembly stubs
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARMLoweredPanicExtendA || v.Op == ssa.OpARMLoweredPanicExtendB || v.Op == ssa.OpARMLoweredPanicExtendC)
            {
                p = s.Prog(obj.ACALL);
                p.To.Type = obj.TYPE_MEM;
                p.To.Name = obj.NAME_EXTERN;
                p.To.Sym = gc.ExtendCheckFunc[v.AuxInt];
                s.UseArgs(12L); // space used in callee args area by assembly stubs
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARMDUFFZERO)
            {
                p = s.Prog(obj.ADUFFZERO);
                p.To.Type = obj.TYPE_MEM;
                p.To.Name = obj.NAME_EXTERN;
                p.To.Sym = gc.Duffzero;
                p.To.Offset = v.AuxInt;
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARMDUFFCOPY)
            {
                p = s.Prog(obj.ADUFFCOPY);
                p.To.Type = obj.TYPE_MEM;
                p.To.Name = obj.NAME_EXTERN;
                p.To.Sym = gc.Duffcopy;
                p.To.Offset = v.AuxInt;
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARMLoweredNilCheck) 
            {
                // Issue a load which will fault if arg is nil.
                p = s.Prog(arm.AMOVB);
                p.From.Type = obj.TYPE_MEM;
                p.From.Reg = v.Args[0L].Reg();
                gc.AddAux(_addr_p.From, v);
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = arm.REGTMP;
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
            if (v.Op == ssa.OpARMLoweredZero) 
            {
                // MOVW.P    Rarg2, 4(R1)
                // CMP    Rarg1, R1
                // BLE    -2(PC)
                // arg1 is the address of the last element to zero
                // arg2 is known to be zero
                // auxint is alignment
                long sz = default;
                obj.As mov = default;

                if (v.AuxInt % 4L == 0L) 
                    sz = 4L;
                    mov = arm.AMOVW;
                else if (v.AuxInt % 2L == 0L) 
                    sz = 2L;
                    mov = arm.AMOVH;
                else 
                    sz = 1L;
                    mov = arm.AMOVB;
                                p = s.Prog(mov);
                p.Scond = arm.C_PBIT;
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = v.Args[2L].Reg();
                p.To.Type = obj.TYPE_MEM;
                p.To.Reg = arm.REG_R1;
                p.To.Offset = sz;
                var p2 = s.Prog(arm.ACMP);
                p2.From.Type = obj.TYPE_REG;
                p2.From.Reg = v.Args[1L].Reg();
                p2.Reg = arm.REG_R1;
                var p3 = s.Prog(arm.ABLE);
                p3.To.Type = obj.TYPE_BRANCH;
                gc.Patch(p3, p);
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARMLoweredMove) 
            {
                // MOVW.P    4(R1), Rtmp
                // MOVW.P    Rtmp, 4(R2)
                // CMP    Rarg2, R1
                // BLE    -3(PC)
                // arg2 is the address of the last element of src
                // auxint is alignment
                sz = default;
                mov = default;

                if (v.AuxInt % 4L == 0L) 
                    sz = 4L;
                    mov = arm.AMOVW;
                else if (v.AuxInt % 2L == 0L) 
                    sz = 2L;
                    mov = arm.AMOVH;
                else 
                    sz = 1L;
                    mov = arm.AMOVB;
                                p = s.Prog(mov);
                p.Scond = arm.C_PBIT;
                p.From.Type = obj.TYPE_MEM;
                p.From.Reg = arm.REG_R1;
                p.From.Offset = sz;
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = arm.REGTMP;
                p2 = s.Prog(mov);
                p2.Scond = arm.C_PBIT;
                p2.From.Type = obj.TYPE_REG;
                p2.From.Reg = arm.REGTMP;
                p2.To.Type = obj.TYPE_MEM;
                p2.To.Reg = arm.REG_R2;
                p2.To.Offset = sz;
                p3 = s.Prog(arm.ACMP);
                p3.From.Type = obj.TYPE_REG;
                p3.From.Reg = v.Args[2L].Reg();
                p3.Reg = arm.REG_R1;
                var p4 = s.Prog(arm.ABLE);
                p4.To.Type = obj.TYPE_BRANCH;
                gc.Patch(p4, p);
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARMEqual || v.Op == ssa.OpARMNotEqual || v.Op == ssa.OpARMLessThan || v.Op == ssa.OpARMLessEqual || v.Op == ssa.OpARMGreaterThan || v.Op == ssa.OpARMGreaterEqual || v.Op == ssa.OpARMLessThanU || v.Op == ssa.OpARMLessEqualU || v.Op == ssa.OpARMGreaterThanU || v.Op == ssa.OpARMGreaterEqualU) 
            {
                // generate boolean values
                // use conditional move
                p = s.Prog(arm.AMOVW);
                p.From.Type = obj.TYPE_CONST;
                p.From.Offset = 0L;
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
                p = s.Prog(arm.AMOVW);
                p.Scond = condBits[v.Op];
                p.From.Type = obj.TYPE_CONST;
                p.From.Offset = 1L;
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARMLoweredGetClosurePtr) 
            {
                // Closure pointer is R7 (arm.REGCTXT).
                gc.CheckLoweredGetClosurePtr(v);
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARMLoweredGetCallerSP) 
            {
                // caller's SP is FixedFrameSize below the address of the first arg
                p = s.Prog(arm.AMOVW);
                p.From.Type = obj.TYPE_ADDR;
                p.From.Offset = -gc.Ctxt.FixedFrameSize();
                p.From.Name = obj.NAME_PARAM;
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARMLoweredGetCallerPC)
            {
                p = s.Prog(obj.AGETCALLERPC);
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARMFlagConstant)
            {
                v.Fatalf("FlagConstant op should never make it to codegen %v", v.LongString());
                goto __switch_break0;
            }
            if (v.Op == ssa.OpARMInvertFlags)
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

        private static map condBits = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ssa.Op, byte>{ssa.OpARMEqual:arm.C_SCOND_EQ,ssa.OpARMNotEqual:arm.C_SCOND_NE,ssa.OpARMLessThan:arm.C_SCOND_LT,ssa.OpARMLessThanU:arm.C_SCOND_LO,ssa.OpARMLessEqual:arm.C_SCOND_LE,ssa.OpARMLessEqualU:arm.C_SCOND_LS,ssa.OpARMGreaterThan:arm.C_SCOND_GT,ssa.OpARMGreaterThanU:arm.C_SCOND_HI,ssa.OpARMGreaterEqual:arm.C_SCOND_GE,ssa.OpARMGreaterEqualU:arm.C_SCOND_HS,};



        // To model a 'LEnoov' ('<=' without overflow checking) branching
        private static array<array<gc.IndexJump>> leJumps = new array<array<gc.IndexJump>>(new array<gc.IndexJump>[] { {{Jump:arm.ABEQ,Index:0},{Jump:arm.ABPL,Index:1}}, {{Jump:arm.ABMI,Index:0},{Jump:arm.ABEQ,Index:0}} });

        // To model a 'GTnoov' ('>' without overflow checking) branching
        private static array<array<gc.IndexJump>> gtJumps = new array<array<gc.IndexJump>>(new array<gc.IndexJump>[] { {{Jump:arm.ABMI,Index:1},{Jump:arm.ABEQ,Index:1}}, {{Jump:arm.ABEQ,Index:1},{Jump:arm.ABPL,Index:0}} });

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
                p = s.Prog(arm.ACMP);
                p.From.Type = obj.TYPE_CONST;
                p.From.Offset = 0L;
                p.Reg = arm.REG_R0;
                p = s.Prog(arm.ABNE);
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
            else if (b.Kind == ssa.BlockARMEQ || b.Kind == ssa.BlockARMNE || b.Kind == ssa.BlockARMLT || b.Kind == ssa.BlockARMGE || b.Kind == ssa.BlockARMLE || b.Kind == ssa.BlockARMGT || b.Kind == ssa.BlockARMULT || b.Kind == ssa.BlockARMUGT || b.Kind == ssa.BlockARMULE || b.Kind == ssa.BlockARMUGE || b.Kind == ssa.BlockARMLTnoov || b.Kind == ssa.BlockARMGEnoov) 
                var jmp = blockJump[b.Kind];

                if (next == b.Succs[0L].Block()) 
                    s.Br(jmp.invasm, b.Succs[1L].Block());
                else if (next == b.Succs[1L].Block()) 
                    s.Br(jmp.asm, b.Succs[0L].Block());
                else 
                    if (b.Likely != ssa.BranchUnlikely)
                    {
                        s.Br(jmp.asm, b.Succs[0L].Block());
                        s.Br(obj.AJMP, b.Succs[1L].Block());
                    }
                    else
                    {
                        s.Br(jmp.invasm, b.Succs[1L].Block());
                        s.Br(obj.AJMP, b.Succs[0L].Block());
                    }

                            else if (b.Kind == ssa.BlockARMLEnoov) 
                s.CombJump(b, next, _addr_leJumps);
            else if (b.Kind == ssa.BlockARMGTnoov) 
                s.CombJump(b, next, _addr_gtJumps);
            else 
                b.Fatalf("branch not implemented: %s", b.LongString());
            
        }
    }
}}}}
