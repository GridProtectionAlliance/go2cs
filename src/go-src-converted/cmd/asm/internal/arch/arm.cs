// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file encapsulates some of the odd characteristics of the ARM
// instruction set, to minimize its interaction with the core of the
// assembler.

// package arch -- go2cs converted at 2022 March 13 05:57:50 UTC
// import "cmd/asm/internal/arch" ==> using arch = go.cmd.asm.@internal.arch_package
// Original source: C:\Program Files\Go\src\cmd\asm\internal\arch\arm.go
namespace go.cmd.asm.@internal;

using strings = strings_package;

using obj = cmd.@internal.obj_package;
using arm = cmd.@internal.obj.arm_package;

public static partial class arch_package {

private static map armLS = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, byte>{"U":arm.C_UBIT,"S":arm.C_SBIT,"W":arm.C_WBIT,"P":arm.C_PBIT,"PW":arm.C_WBIT|arm.C_PBIT,"WP":arm.C_WBIT|arm.C_PBIT,};

private static map armSCOND = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, byte>{"EQ":arm.C_SCOND_EQ,"NE":arm.C_SCOND_NE,"CS":arm.C_SCOND_HS,"HS":arm.C_SCOND_HS,"CC":arm.C_SCOND_LO,"LO":arm.C_SCOND_LO,"MI":arm.C_SCOND_MI,"PL":arm.C_SCOND_PL,"VS":arm.C_SCOND_VS,"VC":arm.C_SCOND_VC,"HI":arm.C_SCOND_HI,"LS":arm.C_SCOND_LS,"GE":arm.C_SCOND_GE,"LT":arm.C_SCOND_LT,"GT":arm.C_SCOND_GT,"LE":arm.C_SCOND_LE,"AL":arm.C_SCOND_NONE,"U":arm.C_UBIT,"S":arm.C_SBIT,"W":arm.C_WBIT,"P":arm.C_PBIT,"PW":arm.C_WBIT|arm.C_PBIT,"WP":arm.C_WBIT|arm.C_PBIT,"F":arm.C_FBIT,"IBW":arm.C_WBIT|arm.C_PBIT|arm.C_UBIT,"IAW":arm.C_WBIT|arm.C_UBIT,"DBW":arm.C_WBIT|arm.C_PBIT,"DAW":arm.C_WBIT,"IB":arm.C_PBIT|arm.C_UBIT,"IA":arm.C_UBIT,"DB":arm.C_PBIT,"DA":0,};

private static map armJump = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, bool>{"B":true,"BL":true,"BX":true,"BEQ":true,"BNE":true,"BCS":true,"BHS":true,"BCC":true,"BLO":true,"BMI":true,"BPL":true,"BVS":true,"BVC":true,"BHI":true,"BLS":true,"BGE":true,"BLT":true,"BGT":true,"BLE":true,"CALL":true,"JMP":true,};

private static bool jumpArm(@string word) {
    return armJump[word];
}

// IsARMCMP reports whether the op (as defined by an arm.A* constant) is
// one of the comparison instructions that require special handling.
public static bool IsARMCMP(obj.As op) {

    if (op == arm.ACMN || op == arm.ACMP || op == arm.ATEQ || op == arm.ATST) 
        return true;
        return false;
}

// IsARMSTREX reports whether the op (as defined by an arm.A* constant) is
// one of the STREX-like instructions that require special handling.
public static bool IsARMSTREX(obj.As op) {

    if (op == arm.ASTREX || op == arm.ASTREXD || op == arm.ASWPW || op == arm.ASWPBU) 
        return true;
        return false;
}

// MCR is not defined by the obj/arm; instead we define it privately here.
// It is encoded as an MRC with a bit inside the instruction word,
// passed to arch.ARMMRCOffset.
private static readonly var aMCR = arm.ALAST + 1;

// IsARMMRC reports whether the op (as defined by an arm.A* constant) is
// MRC or MCR


// IsARMMRC reports whether the op (as defined by an arm.A* constant) is
// MRC or MCR
public static bool IsARMMRC(obj.As op) {

    if (op == arm.AMRC || op == aMCR) // Note: aMCR is defined in this package.
        return true;
        return false;
}

// IsARMBFX reports whether the op (as defined by an arm.A* constant) is one the
// BFX-like instructions which are in the form of "op $width, $LSB, (Reg,) Reg".
public static bool IsARMBFX(obj.As op) {

    if (op == arm.ABFX || op == arm.ABFXU || op == arm.ABFC || op == arm.ABFI) 
        return true;
        return false;
}

// IsARMFloatCmp reports whether the op is a floating comparison instruction.
public static bool IsARMFloatCmp(obj.As op) {

    if (op == arm.ACMPF || op == arm.ACMPD) 
        return true;
        return false;
}

// ARMMRCOffset implements the peculiar encoding of the MRC and MCR instructions.
// The difference between MRC and MCR is represented by a bit high in the word, not
// in the usual way by the opcode itself. Asm must use AMRC for both instructions, so
// we return the opcode for MRC so that asm doesn't need to import obj/arm.
public static (long, obj.As, bool) ARMMRCOffset(obj.As op, @string cond, long x0, long x1, long x2, long x3, long x4, long x5) {
    long offset = default;
    obj.As op0 = default;
    bool ok = default;

    var op1 = int64(0);
    if (op == arm.AMRC) {
        op1 = 1;
    }
    var (bits, ok) = ParseARMCondition(cond);
    if (!ok) {
        return ;
    }
    offset = (0xe << 24) | (op1 << 20) | ((int64(bits) ^ arm.C_SCOND_XOR) << 28) | ((x0 & 15) << 8) | ((x1 & 7) << 21) | ((x2 & 15) << 12) | ((x3 & 15) << 16) | ((x4 & 15) << 0) | ((x5 & 7) << 5) | (1 << 4); /* must be set */
    return (offset, arm.AMRC, true);
}

// IsARMMULA reports whether the op (as defined by an arm.A* constant) is
// MULA, MULS, MMULA, MMULS, MULABB, MULAWB or MULAWT, the 4-operand instructions.
public static bool IsARMMULA(obj.As op) {

    if (op == arm.AMULA || op == arm.AMULS || op == arm.AMMULA || op == arm.AMMULS || op == arm.AMULABB || op == arm.AMULAWB || op == arm.AMULAWT) 
        return true;
        return false;
}

private static obj.As bcode = new slice<obj.As>(new obj.As[] { arm.ABEQ, arm.ABNE, arm.ABCS, arm.ABCC, arm.ABMI, arm.ABPL, arm.ABVS, arm.ABVC, arm.ABHI, arm.ABLS, arm.ABGE, arm.ABLT, arm.ABGT, arm.ABLE, arm.AB, obj.ANOP });

// ARMConditionCodes handles the special condition code situation for the ARM.
// It returns a boolean to indicate success; failure means cond was unrecognized.
public static bool ARMConditionCodes(ptr<obj.Prog> _addr_prog, @string cond) {
    ref obj.Prog prog = ref _addr_prog.val;

    if (cond == "") {
        return true;
    }
    var (bits, ok) = ParseARMCondition(cond);
    if (!ok) {
        return false;
    }
    if (prog.As == arm.AB) {
        prog.As = bcode[(bits ^ arm.C_SCOND_XOR) & 0xf];
        bits = (bits & ~0xf) | arm.C_SCOND_NONE;
    }
    prog.Scond = bits;
    return true;
}

// ParseARMCondition parses the conditions attached to an ARM instruction.
// The input is a single string consisting of period-separated condition
// codes, such as ".P.W". An initial period is ignored.
public static (byte, bool) ParseARMCondition(@string cond) {
    byte _p0 = default;
    bool _p0 = default;

    return parseARMCondition(cond, armLS, armSCOND);
}

private static (byte, bool) parseARMCondition(@string cond, map<@string, byte> ls, map<@string, byte> scond) {
    byte _p0 = default;
    bool _p0 = default;

    cond = strings.TrimPrefix(cond, ".");
    if (cond == "") {
        return (arm.C_SCOND_NONE, true);
    }
    var names = strings.Split(cond, ".");
    var bits = uint8(0);
    foreach (var (_, name) in names) {
        {
            var b__prev1 = b;

            var (b, present) = ls[name];

            if (present) {
                bits |= b;
                continue;
            }

            b = b__prev1;

        }
        {
            var b__prev1 = b;

            (b, present) = scond[name];

            if (present) {
                bits = (bits & ~arm.C_SCOND) | b;
                continue;
            }

            b = b__prev1;

        }
        return (0, false);
    }    return (bits, true);
}

private static (short, bool) armRegisterNumber(@string name, short n) {
    short _p0 = default;
    bool _p0 = default;

    if (n < 0 || 15 < n) {
        return (0, false);
    }
    switch (name) {
        case "R": 
            return (arm.REG_R0 + n, true);
            break;
        case "F": 
            return (arm.REG_F0 + n, true);
            break;
    }
    return (0, false);
}

} // end arch_package
