// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package arm -- go2cs converted at 2022 March 06 23:14:46 UTC
// import "cmd/compile/internal/arm" ==> using arm = go.cmd.compile.@internal.arm_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\arm\ssa.go
using fmt = go.fmt_package;
using buildcfg = go.@internal.buildcfg_package;
using math = go.math_package;
using bits = go.math.bits_package;

using @base = go.cmd.compile.@internal.@base_package;
using ir = go.cmd.compile.@internal.ir_package;
using logopt = go.cmd.compile.@internal.logopt_package;
using ssa = go.cmd.compile.@internal.ssa_package;
using ssagen = go.cmd.compile.@internal.ssagen_package;
using types = go.cmd.compile.@internal.types_package;
using obj = go.cmd.@internal.obj_package;
using arm = go.cmd.@internal.obj.arm_package;

namespace go.cmd.compile.@internal;

public static partial class arm_package {

    // loadByType returns the load instruction of the given type.
private static obj.As loadByType(ptr<types.Type> _addr_t) => func((_, panic, _) => {
    ref types.Type t = ref _addr_t.val;

    if (t.IsFloat()) {
        switch (t.Size()) {
            case 4: 
                return arm.AMOVF;
                break;
            case 8: 
                return arm.AMOVD;
                break;
        }

    }
    else
 {
        switch (t.Size()) {
            case 1: 
                           if (t.IsSigned()) {
                               return arm.AMOVB;
                           }
                           else
                {
                               return arm.AMOVBU;
                           }
                break;
            case 2: 
                           if (t.IsSigned()) {
                               return arm.AMOVH;
                           }
                           else
                {
                               return arm.AMOVHU;
                           }
                break;
            case 4: 
                return arm.AMOVW;
                break;
        }

    }
    panic("bad load type");

});

// storeByType returns the store instruction of the given type.
private static obj.As storeByType(ptr<types.Type> _addr_t) => func((_, panic, _) => {
    ref types.Type t = ref _addr_t.val;

    if (t.IsFloat()) {
        switch (t.Size()) {
            case 4: 
                return arm.AMOVF;
                break;
            case 8: 
                return arm.AMOVD;
                break;
        }

    }
    else
 {
        switch (t.Size()) {
            case 1: 
                return arm.AMOVB;
                break;
            case 2: 
                return arm.AMOVH;
                break;
            case 4: 
                return arm.AMOVW;
                break;
        }

    }
    panic("bad store type");

});

// shift type is used as Offset in obj.TYPE_SHIFT operands to encode shifted register operands
private partial struct shift { // : long
}

// copied from ../../../internal/obj/util.go:/TYPE_SHIFT
private static @string String(this shift v) {
    @string op = "<<>>->@>"[(int)((v >> 5) & 3) << 1..];
    if (v & (1 << 4) != 0) { 
        // register shift
        return fmt.Sprintf("R%d%c%cR%d", v & 15, op[0], op[1], (v >> 8) & 15);

    }
    else
 { 
        // constant shift
        return fmt.Sprintf("R%d%c%c%d", v & 15, op[0], op[1], (v >> 7) & 31);

    }
}

// makeshift encodes a register shifted by a constant
private static shift makeshift(short reg, long typ, long s) {
    return shift(int64(reg & 0xf) | typ | (s & 31) << 7);
}

// genshift generates a Prog for r = r0 op (r1 shifted by n)
private static ptr<obj.Prog> genshift(ptr<ssagen.State> _addr_s, obj.As @as, short r0, short r1, short r, long typ, long n) {
    ref ssagen.State s = ref _addr_s.val;

    var p = s.Prog(as);
    p.From.Type = obj.TYPE_SHIFT;
    p.From.Offset = int64(makeshift(r1, typ, n));
    p.Reg = r0;
    if (r != 0) {
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = r;
    }
    return _addr_p!;

}

// makeregshift encodes a register shifted by a register
private static shift makeregshift(short r1, long typ, short r2) {
    return shift(int64(r1 & 0xf) | typ | int64(r2 & 0xf) << 8 | 1 << 4);
}

// genregshift generates a Prog for r = r0 op (r1 shifted by r2)
private static ptr<obj.Prog> genregshift(ptr<ssagen.State> _addr_s, obj.As @as, short r0, short r1, short r2, short r, long typ) {
    ref ssagen.State s = ref _addr_s.val;

    var p = s.Prog(as);
    p.From.Type = obj.TYPE_SHIFT;
    p.From.Offset = int64(makeregshift(r1, typ, r2));
    p.Reg = r0;
    if (r != 0) {
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = r;
    }
    return _addr_p!;

}

// find a (lsb, width) pair for BFC
// lsb must be in [0, 31], width must be in [1, 32 - lsb]
// return (0xffffffff, 0) if v is not a binary like 0...01...10...0
private static (uint, uint) getBFC(uint v) {
    uint _p0 = default;
    uint _p0 = default;

    uint m = default;    uint l = default; 
    // BFC is not applicable with zero
 
    // BFC is not applicable with zero
    if (v == 0) {
        return (0xffffffff, 0);
    }
    l = uint32(bits.TrailingZeros32(v)); 
    // m-1 represents the highest set bit index, for example m=30 for 0x3ffffffc
    m = 32 - uint32(bits.LeadingZeros32(v)); 
    // check if v is a binary like 0...01...10...0
    if ((1 << (int)(m)) - (1 << (int)(l)) == v) { 
        // it must be m > l for non-zero v
        return (l, m - l);

    }
    return (0xffffffff, 0);

}

private static void ssaGenValue(ptr<ssagen.State> _addr_s, ptr<ssa.Value> _addr_v) => func((_, panic, _) => {
    ref ssagen.State s = ref _addr_s.val;
    ref ssa.Value v = ref _addr_v.val;


    if (v.Op == ssa.OpCopy || v.Op == ssa.OpARMMOVWreg)
    {
        if (v.Type.IsMemory()) {
            return ;
        }
        var x = v.Args[0].Reg();
        var y = v.Reg();
        if (x == y) {
            return ;
        }
        var @as = arm.AMOVW;
        if (v.Type.IsFloat()) {
            switch (v.Type.Size()) {
                case 4: 
                    as = arm.AMOVF;
                    break;
                case 8: 
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
    if (v.Op == ssa.OpARMADD || v.Op == ssa.OpARMADC || v.Op == ssa.OpARMSUB || v.Op == ssa.OpARMSBC || v.Op == ssa.OpARMRSB || v.Op == ssa.OpARMAND || v.Op == ssa.OpARMOR || v.Op == ssa.OpARMXOR || v.Op == ssa.OpARMBIC || v.Op == ssa.OpARMMUL || v.Op == ssa.OpARMADDF || v.Op == ssa.OpARMADDD || v.Op == ssa.OpARMSUBF || v.Op == ssa.OpARMSUBD || v.Op == ssa.OpARMSLL || v.Op == ssa.OpARMSRL || v.Op == ssa.OpARMSRA || v.Op == ssa.OpARMMULF || v.Op == ssa.OpARMMULD || v.Op == ssa.OpARMNMULF || v.Op == ssa.OpARMNMULD || v.Op == ssa.OpARMDIVF || v.Op == ssa.OpARMDIVD)
    {
        var r = v.Reg();
        var r1 = v.Args[0].Reg();
        var r2 = v.Args[1].Reg();
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
        genregshift(_addr_s, arm.AMOVW, 0, v.Args[0].Reg(), v.Args[1].Reg(), v.Reg(), arm.SHIFT_RR);
        goto __switch_break0;
    }
    if (v.Op == ssa.OpARMMULAF || v.Op == ssa.OpARMMULAD || v.Op == ssa.OpARMMULSF || v.Op == ssa.OpARMMULSD || v.Op == ssa.OpARMFMULAD)
    {
        r = v.Reg();
        var r0 = v.Args[0].Reg();
        r1 = v.Args[1].Reg();
        r2 = v.Args[2].Reg();
        if (r != r0) {
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
        r1 = v.Args[0].Reg();
        r2 = v.Args[1].Reg();
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
        r1 = v.Args[0].Reg();
        r2 = v.Args[1].Reg();
        p = s.Prog(arm.ASRA);
        p.Scond = arm.C_SCOND_HS;
        p.From.Type = obj.TYPE_CONST;
        p.From.Offset = 31;
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
        p.From.Offset = v.AuxInt >> 8;
        p.SetFrom3Const(v.AuxInt & 0xff);
        p.Reg = v.Args[0].Reg();
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = v.Reg();
        goto __switch_break0;
    }
    if (v.Op == ssa.OpARMANDconst || v.Op == ssa.OpARMBICconst) 
    {
        // try to optimize ANDconst and BICconst to BFC, which saves bytes and ticks
        // BFC is only available on ARMv7, and its result and source are in the same register
        if (buildcfg.GOARM == 7 && v.Reg() == v.Args[0].Reg()) {
            uint val = default;
            if (v.Op == ssa.OpARMANDconst) {
                val = ~uint32(v.AuxInt);
            }
            else
 { // BICconst
                val = uint32(v.AuxInt);

            }

            var (lsb, width) = getBFC(val); 
            // omit BFC for ARM's imm12
            if (8 < width && width < 24) {
                p = s.Prog(arm.ABFC);
                p.From.Type = obj.TYPE_CONST;
                p.From.Offset = int64(width);
                p.SetFrom3Const(int64(lsb));
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
                break;
            }

        }
        fallthrough = true;
    }
    if (fallthrough || v.Op == ssa.OpARMADDconst || v.Op == ssa.OpARMADCconst || v.Op == ssa.OpARMSUBconst || v.Op == ssa.OpARMSBCconst || v.Op == ssa.OpARMRSBconst || v.Op == ssa.OpARMRSCconst || v.Op == ssa.OpARMORconst || v.Op == ssa.OpARMXORconst || v.Op == ssa.OpARMSLLconst || v.Op == ssa.OpARMSRLconst || v.Op == ssa.OpARMSRAconst)
    {
        p = s.Prog(v.Op.Asm());
        p.From.Type = obj.TYPE_CONST;
        p.From.Offset = v.AuxInt;
        p.Reg = v.Args[0].Reg();
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
        p.Reg = v.Args[0].Reg();
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = v.Reg0();
        goto __switch_break0;
    }
    if (v.Op == ssa.OpARMSRRconst)
    {
        genshift(_addr_s, arm.AMOVW, 0, v.Args[0].Reg(), v.Reg(), arm.SHIFT_RR, v.AuxInt);
        goto __switch_break0;
    }
    if (v.Op == ssa.OpARMADDshiftLL || v.Op == ssa.OpARMADCshiftLL || v.Op == ssa.OpARMSUBshiftLL || v.Op == ssa.OpARMSBCshiftLL || v.Op == ssa.OpARMRSBshiftLL || v.Op == ssa.OpARMRSCshiftLL || v.Op == ssa.OpARMANDshiftLL || v.Op == ssa.OpARMORshiftLL || v.Op == ssa.OpARMXORshiftLL || v.Op == ssa.OpARMBICshiftLL)
    {
        genshift(_addr_s, v.Op.Asm(), v.Args[0].Reg(), v.Args[1].Reg(), v.Reg(), arm.SHIFT_LL, v.AuxInt);
        goto __switch_break0;
    }
    if (v.Op == ssa.OpARMADDSshiftLL || v.Op == ssa.OpARMSUBSshiftLL || v.Op == ssa.OpARMRSBSshiftLL)
    {
        p = genshift(_addr_s, v.Op.Asm(), v.Args[0].Reg(), v.Args[1].Reg(), v.Reg0(), arm.SHIFT_LL, v.AuxInt);
        p.Scond = arm.C_SBIT;
        goto __switch_break0;
    }
    if (v.Op == ssa.OpARMADDshiftRL || v.Op == ssa.OpARMADCshiftRL || v.Op == ssa.OpARMSUBshiftRL || v.Op == ssa.OpARMSBCshiftRL || v.Op == ssa.OpARMRSBshiftRL || v.Op == ssa.OpARMRSCshiftRL || v.Op == ssa.OpARMANDshiftRL || v.Op == ssa.OpARMORshiftRL || v.Op == ssa.OpARMXORshiftRL || v.Op == ssa.OpARMBICshiftRL)
    {
        genshift(_addr_s, v.Op.Asm(), v.Args[0].Reg(), v.Args[1].Reg(), v.Reg(), arm.SHIFT_LR, v.AuxInt);
        goto __switch_break0;
    }
    if (v.Op == ssa.OpARMADDSshiftRL || v.Op == ssa.OpARMSUBSshiftRL || v.Op == ssa.OpARMRSBSshiftRL)
    {
        p = genshift(_addr_s, v.Op.Asm(), v.Args[0].Reg(), v.Args[1].Reg(), v.Reg0(), arm.SHIFT_LR, v.AuxInt);
        p.Scond = arm.C_SBIT;
        goto __switch_break0;
    }
    if (v.Op == ssa.OpARMADDshiftRA || v.Op == ssa.OpARMADCshiftRA || v.Op == ssa.OpARMSUBshiftRA || v.Op == ssa.OpARMSBCshiftRA || v.Op == ssa.OpARMRSBshiftRA || v.Op == ssa.OpARMRSCshiftRA || v.Op == ssa.OpARMANDshiftRA || v.Op == ssa.OpARMORshiftRA || v.Op == ssa.OpARMXORshiftRA || v.Op == ssa.OpARMBICshiftRA)
    {
        genshift(_addr_s, v.Op.Asm(), v.Args[0].Reg(), v.Args[1].Reg(), v.Reg(), arm.SHIFT_AR, v.AuxInt);
        goto __switch_break0;
    }
    if (v.Op == ssa.OpARMADDSshiftRA || v.Op == ssa.OpARMSUBSshiftRA || v.Op == ssa.OpARMRSBSshiftRA)
    {
        p = genshift(_addr_s, v.Op.Asm(), v.Args[0].Reg(), v.Args[1].Reg(), v.Reg0(), arm.SHIFT_AR, v.AuxInt);
        p.Scond = arm.C_SBIT;
        goto __switch_break0;
    }
    if (v.Op == ssa.OpARMXORshiftRR)
    {
        genshift(_addr_s, v.Op.Asm(), v.Args[0].Reg(), v.Args[1].Reg(), v.Reg(), arm.SHIFT_RR, v.AuxInt);
        goto __switch_break0;
    }
    if (v.Op == ssa.OpARMMVNshiftLL)
    {
        genshift(_addr_s, v.Op.Asm(), 0, v.Args[0].Reg(), v.Reg(), arm.SHIFT_LL, v.AuxInt);
        goto __switch_break0;
    }
    if (v.Op == ssa.OpARMMVNshiftRL)
    {
        genshift(_addr_s, v.Op.Asm(), 0, v.Args[0].Reg(), v.Reg(), arm.SHIFT_LR, v.AuxInt);
        goto __switch_break0;
    }
    if (v.Op == ssa.OpARMMVNshiftRA)
    {
        genshift(_addr_s, v.Op.Asm(), 0, v.Args[0].Reg(), v.Reg(), arm.SHIFT_AR, v.AuxInt);
        goto __switch_break0;
    }
    if (v.Op == ssa.OpARMMVNshiftLLreg)
    {
        genregshift(_addr_s, v.Op.Asm(), 0, v.Args[0].Reg(), v.Args[1].Reg(), v.Reg(), arm.SHIFT_LL);
        goto __switch_break0;
    }
    if (v.Op == ssa.OpARMMVNshiftRLreg)
    {
        genregshift(_addr_s, v.Op.Asm(), 0, v.Args[0].Reg(), v.Args[1].Reg(), v.Reg(), arm.SHIFT_LR);
        goto __switch_break0;
    }
    if (v.Op == ssa.OpARMMVNshiftRAreg)
    {
        genregshift(_addr_s, v.Op.Asm(), 0, v.Args[0].Reg(), v.Args[1].Reg(), v.Reg(), arm.SHIFT_AR);
        goto __switch_break0;
    }
    if (v.Op == ssa.OpARMADDshiftLLreg || v.Op == ssa.OpARMADCshiftLLreg || v.Op == ssa.OpARMSUBshiftLLreg || v.Op == ssa.OpARMSBCshiftLLreg || v.Op == ssa.OpARMRSBshiftLLreg || v.Op == ssa.OpARMRSCshiftLLreg || v.Op == ssa.OpARMANDshiftLLreg || v.Op == ssa.OpARMORshiftLLreg || v.Op == ssa.OpARMXORshiftLLreg || v.Op == ssa.OpARMBICshiftLLreg)
    {
        genregshift(_addr_s, v.Op.Asm(), v.Args[0].Reg(), v.Args[1].Reg(), v.Args[2].Reg(), v.Reg(), arm.SHIFT_LL);
        goto __switch_break0;
    }
    if (v.Op == ssa.OpARMADDSshiftLLreg || v.Op == ssa.OpARMSUBSshiftLLreg || v.Op == ssa.OpARMRSBSshiftLLreg)
    {
        p = genregshift(_addr_s, v.Op.Asm(), v.Args[0].Reg(), v.Args[1].Reg(), v.Args[2].Reg(), v.Reg0(), arm.SHIFT_LL);
        p.Scond = arm.C_SBIT;
        goto __switch_break0;
    }
    if (v.Op == ssa.OpARMADDshiftRLreg || v.Op == ssa.OpARMADCshiftRLreg || v.Op == ssa.OpARMSUBshiftRLreg || v.Op == ssa.OpARMSBCshiftRLreg || v.Op == ssa.OpARMRSBshiftRLreg || v.Op == ssa.OpARMRSCshiftRLreg || v.Op == ssa.OpARMANDshiftRLreg || v.Op == ssa.OpARMORshiftRLreg || v.Op == ssa.OpARMXORshiftRLreg || v.Op == ssa.OpARMBICshiftRLreg)
    {
        genregshift(_addr_s, v.Op.Asm(), v.Args[0].Reg(), v.Args[1].Reg(), v.Args[2].Reg(), v.Reg(), arm.SHIFT_LR);
        goto __switch_break0;
    }
    if (v.Op == ssa.OpARMADDSshiftRLreg || v.Op == ssa.OpARMSUBSshiftRLreg || v.Op == ssa.OpARMRSBSshiftRLreg)
    {
        p = genregshift(_addr_s, v.Op.Asm(), v.Args[0].Reg(), v.Args[1].Reg(), v.Args[2].Reg(), v.Reg0(), arm.SHIFT_LR);
        p.Scond = arm.C_SBIT;
        goto __switch_break0;
    }
    if (v.Op == ssa.OpARMADDshiftRAreg || v.Op == ssa.OpARMADCshiftRAreg || v.Op == ssa.OpARMSUBshiftRAreg || v.Op == ssa.OpARMSBCshiftRAreg || v.Op == ssa.OpARMRSBshiftRAreg || v.Op == ssa.OpARMRSCshiftRAreg || v.Op == ssa.OpARMANDshiftRAreg || v.Op == ssa.OpARMORshiftRAreg || v.Op == ssa.OpARMXORshiftRAreg || v.Op == ssa.OpARMBICshiftRAreg)
    {
        genregshift(_addr_s, v.Op.Asm(), v.Args[0].Reg(), v.Args[1].Reg(), v.Args[2].Reg(), v.Reg(), arm.SHIFT_AR);
        goto __switch_break0;
    }
    if (v.Op == ssa.OpARMADDSshiftRAreg || v.Op == ssa.OpARMSUBSshiftRAreg || v.Op == ssa.OpARMRSBSshiftRAreg)
    {
        p = genregshift(_addr_s, v.Op.Asm(), v.Args[0].Reg(), v.Args[1].Reg(), v.Args[2].Reg(), v.Reg0(), arm.SHIFT_AR);
        p.Scond = arm.C_SBIT;
        goto __switch_break0;
    }
    if (v.Op == ssa.OpARMHMUL || v.Op == ssa.OpARMHMULU) 
    {
        // 32-bit high multiplication
        p = s.Prog(v.Op.Asm());
        p.From.Type = obj.TYPE_REG;
        p.From.Reg = v.Args[0].Reg();
        p.Reg = v.Args[1].Reg();
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
        p.From.Reg = v.Args[0].Reg();
        p.Reg = v.Args[1].Reg();
        p.To.Type = obj.TYPE_REGREG;
        p.To.Reg = v.Reg0(); // high 32-bit
        p.To.Offset = int64(v.Reg1()); // low 32-bit
        goto __switch_break0;
    }
    if (v.Op == ssa.OpARMMULA || v.Op == ssa.OpARMMULS)
    {
        p = s.Prog(v.Op.Asm());
        p.From.Type = obj.TYPE_REG;
        p.From.Reg = v.Args[0].Reg();
        p.Reg = v.Args[1].Reg();
        p.To.Type = obj.TYPE_REGREG2;
        p.To.Reg = v.Reg(); // result
        p.To.Offset = int64(v.Args[2].Reg()); // addend
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
        p.From.Reg = v.Args[1].Reg();
        p.Reg = v.Args[0].Reg();
        goto __switch_break0;
    }
    if (v.Op == ssa.OpARMCMPconst || v.Op == ssa.OpARMCMNconst || v.Op == ssa.OpARMTSTconst || v.Op == ssa.OpARMTEQconst) 
    {
        // Special layout in ARM assembly
        p = s.Prog(v.Op.Asm());
        p.From.Type = obj.TYPE_CONST;
        p.From.Offset = v.AuxInt;
        p.Reg = v.Args[0].Reg();
        goto __switch_break0;
    }
    if (v.Op == ssa.OpARMCMPF0 || v.Op == ssa.OpARMCMPD0)
    {
        p = s.Prog(v.Op.Asm());
        p.From.Type = obj.TYPE_REG;
        p.From.Reg = v.Args[0].Reg();
        goto __switch_break0;
    }
    if (v.Op == ssa.OpARMCMPshiftLL || v.Op == ssa.OpARMCMNshiftLL || v.Op == ssa.OpARMTSTshiftLL || v.Op == ssa.OpARMTEQshiftLL)
    {
        genshift(_addr_s, v.Op.Asm(), v.Args[0].Reg(), v.Args[1].Reg(), 0, arm.SHIFT_LL, v.AuxInt);
        goto __switch_break0;
    }
    if (v.Op == ssa.OpARMCMPshiftRL || v.Op == ssa.OpARMCMNshiftRL || v.Op == ssa.OpARMTSTshiftRL || v.Op == ssa.OpARMTEQshiftRL)
    {
        genshift(_addr_s, v.Op.Asm(), v.Args[0].Reg(), v.Args[1].Reg(), 0, arm.SHIFT_LR, v.AuxInt);
        goto __switch_break0;
    }
    if (v.Op == ssa.OpARMCMPshiftRA || v.Op == ssa.OpARMCMNshiftRA || v.Op == ssa.OpARMTSTshiftRA || v.Op == ssa.OpARMTEQshiftRA)
    {
        genshift(_addr_s, v.Op.Asm(), v.Args[0].Reg(), v.Args[1].Reg(), 0, arm.SHIFT_AR, v.AuxInt);
        goto __switch_break0;
    }
    if (v.Op == ssa.OpARMCMPshiftLLreg || v.Op == ssa.OpARMCMNshiftLLreg || v.Op == ssa.OpARMTSTshiftLLreg || v.Op == ssa.OpARMTEQshiftLLreg)
    {
        genregshift(_addr_s, v.Op.Asm(), v.Args[0].Reg(), v.Args[1].Reg(), v.Args[2].Reg(), 0, arm.SHIFT_LL);
        goto __switch_break0;
    }
    if (v.Op == ssa.OpARMCMPshiftRLreg || v.Op == ssa.OpARMCMNshiftRLreg || v.Op == ssa.OpARMTSTshiftRLreg || v.Op == ssa.OpARMTEQshiftRLreg)
    {
        genregshift(_addr_s, v.Op.Asm(), v.Args[0].Reg(), v.Args[1].Reg(), v.Args[2].Reg(), 0, arm.SHIFT_LR);
        goto __switch_break0;
    }
    if (v.Op == ssa.OpARMCMPshiftRAreg || v.Op == ssa.OpARMCMNshiftRAreg || v.Op == ssa.OpARMTSTshiftRAreg || v.Op == ssa.OpARMTEQshiftRAreg)
    {
        genregshift(_addr_s, v.Op.Asm(), v.Args[0].Reg(), v.Args[1].Reg(), v.Args[2].Reg(), 0, arm.SHIFT_AR);
        goto __switch_break0;
    }
    if (v.Op == ssa.OpARMMOVWaddr)
    {
        p = s.Prog(arm.AMOVW);
        p.From.Type = obj.TYPE_ADDR;
        p.From.Reg = v.Args[0].Reg();
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = v.Reg();

        @string wantreg = default; 
        // MOVW $sym+off(base), R
        // the assembler expands it as the following:
        // - base is SP: add constant offset to SP (R13)
        //               when constant is large, tmp register (R11) may be used
        // - base is SB: load external address from constant pool (use relocation)
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


        goto __switch_break0;
    }
    if (v.Op == ssa.OpARMMOVBload || v.Op == ssa.OpARMMOVBUload || v.Op == ssa.OpARMMOVHload || v.Op == ssa.OpARMMOVHUload || v.Op == ssa.OpARMMOVWload || v.Op == ssa.OpARMMOVFload || v.Op == ssa.OpARMMOVDload)
    {
        p = s.Prog(v.Op.Asm());
        p.From.Type = obj.TYPE_MEM;
        p.From.Reg = v.Args[0].Reg();
        ssagen.AddAux(_addr_p.From, v);
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = v.Reg();
        goto __switch_break0;
    }
    if (v.Op == ssa.OpARMMOVBstore || v.Op == ssa.OpARMMOVHstore || v.Op == ssa.OpARMMOVWstore || v.Op == ssa.OpARMMOVFstore || v.Op == ssa.OpARMMOVDstore)
    {
        p = s.Prog(v.Op.Asm());
        p.From.Type = obj.TYPE_REG;
        p.From.Reg = v.Args[1].Reg();
        p.To.Type = obj.TYPE_MEM;
        p.To.Reg = v.Args[0].Reg();
        ssagen.AddAux(_addr_p.To, v);
        goto __switch_break0;
    }
    if (v.Op == ssa.OpARMMOVWloadidx || v.Op == ssa.OpARMMOVBUloadidx || v.Op == ssa.OpARMMOVBloadidx || v.Op == ssa.OpARMMOVHUloadidx || v.Op == ssa.OpARMMOVHloadidx) 
    {
        // this is just shift 0 bits
        fallthrough = true;
    }
    if (fallthrough || v.Op == ssa.OpARMMOVWloadshiftLL)
    {
        p = genshift(_addr_s, v.Op.Asm(), 0, v.Args[1].Reg(), v.Reg(), arm.SHIFT_LL, v.AuxInt);
        p.From.Reg = v.Args[0].Reg();
        goto __switch_break0;
    }
    if (v.Op == ssa.OpARMMOVWloadshiftRL)
    {
        p = genshift(_addr_s, v.Op.Asm(), 0, v.Args[1].Reg(), v.Reg(), arm.SHIFT_LR, v.AuxInt);
        p.From.Reg = v.Args[0].Reg();
        goto __switch_break0;
    }
    if (v.Op == ssa.OpARMMOVWloadshiftRA)
    {
        p = genshift(_addr_s, v.Op.Asm(), 0, v.Args[1].Reg(), v.Reg(), arm.SHIFT_AR, v.AuxInt);
        p.From.Reg = v.Args[0].Reg();
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
        p.From.Reg = v.Args[2].Reg();
        p.To.Type = obj.TYPE_SHIFT;
        p.To.Reg = v.Args[0].Reg();
        p.To.Offset = int64(makeshift(v.Args[1].Reg(), arm.SHIFT_LL, v.AuxInt));
        goto __switch_break0;
    }
    if (v.Op == ssa.OpARMMOVWstoreshiftRL)
    {
        p = s.Prog(v.Op.Asm());
        p.From.Type = obj.TYPE_REG;
        p.From.Reg = v.Args[2].Reg();
        p.To.Type = obj.TYPE_SHIFT;
        p.To.Reg = v.Args[0].Reg();
        p.To.Offset = int64(makeshift(v.Args[1].Reg(), arm.SHIFT_LR, v.AuxInt));
        goto __switch_break0;
    }
    if (v.Op == ssa.OpARMMOVWstoreshiftRA)
    {
        p = s.Prog(v.Op.Asm());
        p.From.Type = obj.TYPE_REG;
        p.From.Reg = v.Args[2].Reg();
        p.To.Type = obj.TYPE_SHIFT;
        p.To.Reg = v.Args[0].Reg();
        p.To.Offset = int64(makeshift(v.Args[1].Reg(), arm.SHIFT_AR, v.AuxInt));
        goto __switch_break0;
    }
    if (v.Op == ssa.OpARMMOVBreg || v.Op == ssa.OpARMMOVBUreg || v.Op == ssa.OpARMMOVHreg || v.Op == ssa.OpARMMOVHUreg)
    {
        var a = v.Args[0];
        while (a.Op == ssa.OpCopy || a.Op == ssa.OpARMMOVWreg || a.Op == ssa.OpARMMOVWnop) {
            a = a.Args[0];
        }
        if (a.Op == ssa.OpLoadReg) {
            var t = a.Type;

            if (v.Op == ssa.OpARMMOVBreg && t.Size() == 1 && t.IsSigned() || v.Op == ssa.OpARMMOVBUreg && t.Size() == 1 && !t.IsSigned() || v.Op == ssa.OpARMMOVHreg && t.Size() == 2 && t.IsSigned() || v.Op == ssa.OpARMMOVHUreg && t.Size() == 2 && !t.IsSigned()) 
                // arg is a proper-typed load, already zero/sign-extended, don't extend again
                if (v.Reg() == v.Args[0].Reg()) {
                    return ;
                }
                p = s.Prog(arm.AMOVW);
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = v.Args[0].Reg();
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
                return ;
            else             
        }
        if (buildcfg.GOARM >= 6) { 
            // generate more efficient "MOVB/MOVBU/MOVH/MOVHU Reg@>0, Reg" on ARMv6 & ARMv7
            genshift(_addr_s, v.Op.Asm(), 0, v.Args[0].Reg(), v.Reg(), arm.SHIFT_RR, 0);
            return ;

        }
        fallthrough = true;
    }
    if (fallthrough || v.Op == ssa.OpARMMVN || v.Op == ssa.OpARMCLZ || v.Op == ssa.OpARMREV || v.Op == ssa.OpARMREV16 || v.Op == ssa.OpARMRBIT || v.Op == ssa.OpARMSQRTF || v.Op == ssa.OpARMSQRTD || v.Op == ssa.OpARMNEGF || v.Op == ssa.OpARMNEGD || v.Op == ssa.OpARMABSD || v.Op == ssa.OpARMMOVWF || v.Op == ssa.OpARMMOVWD || v.Op == ssa.OpARMMOVFW || v.Op == ssa.OpARMMOVDW || v.Op == ssa.OpARMMOVFD || v.Op == ssa.OpARMMOVDF)
    {
        p = s.Prog(v.Op.Asm());
        p.From.Type = obj.TYPE_REG;
        p.From.Reg = v.Args[0].Reg();
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = v.Reg();
        goto __switch_break0;
    }
    if (v.Op == ssa.OpARMMOVWUF || v.Op == ssa.OpARMMOVWUD || v.Op == ssa.OpARMMOVFWU || v.Op == ssa.OpARMMOVDWU)
    {
        p = s.Prog(v.Op.Asm());
        p.Scond = arm.C_UBIT;
        p.From.Type = obj.TYPE_REG;
        p.From.Reg = v.Args[0].Reg();
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
        p.To.Sym = ir.Syms.Udiv;
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
        p.To.Sym = ssagen.BoundsCheckFunc[v.AuxInt];
        s.UseArgs(8); // space used in callee args area by assembly stubs
        goto __switch_break0;
    }
    if (v.Op == ssa.OpARMLoweredPanicExtendA || v.Op == ssa.OpARMLoweredPanicExtendB || v.Op == ssa.OpARMLoweredPanicExtendC)
    {
        p = s.Prog(obj.ACALL);
        p.To.Type = obj.TYPE_MEM;
        p.To.Name = obj.NAME_EXTERN;
        p.To.Sym = ssagen.ExtendCheckFunc[v.AuxInt];
        s.UseArgs(12); // space used in callee args area by assembly stubs
        goto __switch_break0;
    }
    if (v.Op == ssa.OpARMDUFFZERO)
    {
        p = s.Prog(obj.ADUFFZERO);
        p.To.Type = obj.TYPE_MEM;
        p.To.Name = obj.NAME_EXTERN;
        p.To.Sym = ir.Syms.Duffzero;
        p.To.Offset = v.AuxInt;
        goto __switch_break0;
    }
    if (v.Op == ssa.OpARMDUFFCOPY)
    {
        p = s.Prog(obj.ADUFFCOPY);
        p.To.Type = obj.TYPE_MEM;
        p.To.Name = obj.NAME_EXTERN;
        p.To.Sym = ir.Syms.Duffcopy;
        p.To.Offset = v.AuxInt;
        goto __switch_break0;
    }
    if (v.Op == ssa.OpARMLoweredNilCheck) 
    {
        // Issue a load which will fault if arg is nil.
        p = s.Prog(arm.AMOVB);
        p.From.Type = obj.TYPE_MEM;
        p.From.Reg = v.Args[0].Reg();
        ssagen.AddAux(_addr_p.From, v);
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = arm.REGTMP;
        if (logopt.Enabled()) {
            logopt.LogOpt(v.Pos, "nilcheck", "genssa", v.Block.Func.Name);
        }
        if (@base.Debug.Nil != 0 && v.Pos.Line() > 1) { // v.Pos.Line()==1 in generated wrappers
            @base.WarnfAt(v.Pos, "generated nil check");

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

        if (v.AuxInt % 4 == 0) 
            sz = 4;
            mov = arm.AMOVW;
        else if (v.AuxInt % 2 == 0) 
            sz = 2;
            mov = arm.AMOVH;
        else 
            sz = 1;
            mov = arm.AMOVB;
                p = s.Prog(mov);
        p.Scond = arm.C_PBIT;
        p.From.Type = obj.TYPE_REG;
        p.From.Reg = v.Args[2].Reg();
        p.To.Type = obj.TYPE_MEM;
        p.To.Reg = arm.REG_R1;
        p.To.Offset = sz;
        var p2 = s.Prog(arm.ACMP);
        p2.From.Type = obj.TYPE_REG;
        p2.From.Reg = v.Args[1].Reg();
        p2.Reg = arm.REG_R1;
        var p3 = s.Prog(arm.ABLE);
        p3.To.Type = obj.TYPE_BRANCH;
        p3.To.SetTarget(p);
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

        if (v.AuxInt % 4 == 0) 
            sz = 4;
            mov = arm.AMOVW;
        else if (v.AuxInt % 2 == 0) 
            sz = 2;
            mov = arm.AMOVH;
        else 
            sz = 1;
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
        p3.From.Reg = v.Args[2].Reg();
        p3.Reg = arm.REG_R1;
        var p4 = s.Prog(arm.ABLE);
        p4.To.Type = obj.TYPE_BRANCH;
        p4.To.SetTarget(p);
        goto __switch_break0;
    }
    if (v.Op == ssa.OpARMEqual || v.Op == ssa.OpARMNotEqual || v.Op == ssa.OpARMLessThan || v.Op == ssa.OpARMLessEqual || v.Op == ssa.OpARMGreaterThan || v.Op == ssa.OpARMGreaterEqual || v.Op == ssa.OpARMLessThanU || v.Op == ssa.OpARMLessEqualU || v.Op == ssa.OpARMGreaterThanU || v.Op == ssa.OpARMGreaterEqualU) 
    {
        // generate boolean values
        // use conditional move
        p = s.Prog(arm.AMOVW);
        p.From.Type = obj.TYPE_CONST;
        p.From.Offset = 0;
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = v.Reg();
        p = s.Prog(arm.AMOVW);
        p.Scond = condBits[v.Op];
        p.From.Type = obj.TYPE_CONST;
        p.From.Offset = 1;
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = v.Reg();
        goto __switch_break0;
    }
    if (v.Op == ssa.OpARMLoweredGetClosurePtr) 
    {
        // Closure pointer is R7 (arm.REGCTXT).
        ssagen.CheckLoweredGetClosurePtr(v);
        goto __switch_break0;
    }
    if (v.Op == ssa.OpARMLoweredGetCallerSP) 
    {
        // caller's SP is FixedFrameSize below the address of the first arg
        p = s.Prog(arm.AMOVW);
        p.From.Type = obj.TYPE_ADDR;
        p.From.Offset = -@base.Ctxt.FixedFrameSize();
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
    if (v.Op == ssa.OpClobber || v.Op == ssa.OpClobberReg)
    {
        goto __switch_break0;
    }
    // default: 
        v.Fatalf("genValue not implemented: %s", v.LongString());

    __switch_break0:;

});

private static map condBits = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ssa.Op, byte>{ssa.OpARMEqual:arm.C_SCOND_EQ,ssa.OpARMNotEqual:arm.C_SCOND_NE,ssa.OpARMLessThan:arm.C_SCOND_LT,ssa.OpARMLessThanU:arm.C_SCOND_LO,ssa.OpARMLessEqual:arm.C_SCOND_LE,ssa.OpARMLessEqualU:arm.C_SCOND_LS,ssa.OpARMGreaterThan:arm.C_SCOND_GT,ssa.OpARMGreaterThanU:arm.C_SCOND_HI,ssa.OpARMGreaterEqual:arm.C_SCOND_GE,ssa.OpARMGreaterEqualU:arm.C_SCOND_HS,};



// To model a 'LEnoov' ('<=' without overflow checking) branching
private static array<array<ssagen.IndexJump>> leJumps = new array<array<ssagen.IndexJump>>(new array<ssagen.IndexJump>[] { {{Jump:arm.ABEQ,Index:0},{Jump:arm.ABPL,Index:1}}, {{Jump:arm.ABMI,Index:0},{Jump:arm.ABEQ,Index:0}} });

// To model a 'GTnoov' ('>' without overflow checking) branching
private static array<array<ssagen.IndexJump>> gtJumps = new array<array<ssagen.IndexJump>>(new array<ssagen.IndexJump>[] { {{Jump:arm.ABMI,Index:1},{Jump:arm.ABEQ,Index:1}}, {{Jump:arm.ABEQ,Index:1},{Jump:arm.ABPL,Index:0}} });

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
        // defer returns in R0:
        // 0 if we should continue executing
        // 1 if we should jump to deferreturn call
        p = s.Prog(arm.ACMP);
        p.From.Type = obj.TYPE_CONST;
        p.From.Offset = 0;
        p.Reg = arm.REG_R0;
        p = s.Prog(arm.ABNE);
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
        p = s.Prog(obj.ARET);
        p.To.Type = obj.TYPE_MEM;
        p.To.Name = obj.NAME_EXTERN;
        p.To.Sym = b.Aux._<ptr<obj.LSym>>();
    else if (b.Kind == ssa.BlockARMEQ || b.Kind == ssa.BlockARMNE || b.Kind == ssa.BlockARMLT || b.Kind == ssa.BlockARMGE || b.Kind == ssa.BlockARMLE || b.Kind == ssa.BlockARMGT || b.Kind == ssa.BlockARMULT || b.Kind == ssa.BlockARMUGT || b.Kind == ssa.BlockARMULE || b.Kind == ssa.BlockARMUGE || b.Kind == ssa.BlockARMLTnoov || b.Kind == ssa.BlockARMGEnoov) 
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

            else if (b.Kind == ssa.BlockARMLEnoov) 
        s.CombJump(b, next, _addr_leJumps);
    else if (b.Kind == ssa.BlockARMGTnoov) 
        s.CombJump(b, next, _addr_gtJumps);
    else 
        b.Fatalf("branch not implemented: %s", b.LongString());
    
}

} // end arm_package
