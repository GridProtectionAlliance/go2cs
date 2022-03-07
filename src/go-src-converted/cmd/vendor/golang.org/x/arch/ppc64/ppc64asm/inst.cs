// Copyright 2014 The Go Authors.  All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ppc64asm -- go2cs converted at 2022 March 06 23:25:06 UTC
// import "cmd/vendor/golang.org/x/arch/ppc64/ppc64asm" ==> using ppc64asm = go.cmd.vendor.golang.org.x.arch.ppc64.ppc64asm_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\arch\ppc64\ppc64asm\inst.go
using bytes = go.bytes_package;
using fmt = go.fmt_package;

namespace go.cmd.vendor.golang.org.x.arch.ppc64;

public static partial class ppc64asm_package {

public partial struct Inst {
    public Op Op; // Opcode mnemonic
    public uint Enc; // Raw encoding bits (if Len == 8, this is the prefix word)
    public nint Len; // Length of encoding in bytes.
    public uint SuffixEnc; // Raw encoding bits of second word (if Len == 8)
    public Args Args; // Instruction arguments, in Power ISA manual order.
}

public static @string String(this Inst i) {
    bytes.Buffer buf = default;
    buf.WriteString(i.Op.String());
    foreach (var (j, arg) in i.Args) {
        if (arg == null) {
            break;
        }
        if (j == 0) {
            buf.WriteString(" ");
        }
        else
 {
            buf.WriteString(", ");
        }
        buf.WriteString(arg.String());

    }    return buf.String();

}

// An Op is an instruction operation.
public partial struct Op { // : ushort
}

public static @string String(this Op o) {
    if (int(o) >= len(opstr) || opstr[o] == "") {
        return fmt.Sprintf("Op(%d)", int(o));
    }
    return opstr[o];

}

// An Arg is a single instruction argument, one of these types: Reg, CondReg, SpReg, Imm, PCRel, Label, or Offset.
public partial interface Arg {
    @string IsArg();
    @string String();
}

// An Args holds the instruction arguments.
// If an instruction has fewer than 6 arguments,
// the final elements in the array are nil.
public partial struct Args { // : array<Arg>
}

// A Reg is a single register. The zero value means R0, not the absence of a register.
// It also includes special registers.
public partial struct Reg { // : ushort
}

private static readonly Reg _ = iota;
public static readonly var R0 = 0;
public static readonly var R1 = 1;
public static readonly var R2 = 2;
public static readonly var R3 = 3;
public static readonly var R4 = 4;
public static readonly var R5 = 5;
public static readonly var R6 = 6;
public static readonly var R7 = 7;
public static readonly var R8 = 8;
public static readonly var R9 = 9;
public static readonly var R10 = 10;
public static readonly var R11 = 11;
public static readonly var R12 = 12;
public static readonly var R13 = 13;
public static readonly var R14 = 14;
public static readonly var R15 = 15;
public static readonly var R16 = 16;
public static readonly var R17 = 17;
public static readonly var R18 = 18;
public static readonly var R19 = 19;
public static readonly var R20 = 20;
public static readonly var R21 = 21;
public static readonly var R22 = 22;
public static readonly var R23 = 23;
public static readonly var R24 = 24;
public static readonly var R25 = 25;
public static readonly var R26 = 26;
public static readonly var R27 = 27;
public static readonly var R28 = 28;
public static readonly var R29 = 29;
public static readonly var R30 = 30;
public static readonly var R31 = 31;
public static readonly var F0 = 32;
public static readonly var F1 = 33;
public static readonly var F2 = 34;
public static readonly var F3 = 35;
public static readonly var F4 = 36;
public static readonly var F5 = 37;
public static readonly var F6 = 38;
public static readonly var F7 = 39;
public static readonly var F8 = 40;
public static readonly var F9 = 41;
public static readonly var F10 = 42;
public static readonly var F11 = 43;
public static readonly var F12 = 44;
public static readonly var F13 = 45;
public static readonly var F14 = 46;
public static readonly var F15 = 47;
public static readonly var F16 = 48;
public static readonly var F17 = 49;
public static readonly var F18 = 50;
public static readonly var F19 = 51;
public static readonly var F20 = 52;
public static readonly var F21 = 53;
public static readonly var F22 = 54;
public static readonly var F23 = 55;
public static readonly var F24 = 56;
public static readonly var F25 = 57;
public static readonly var F26 = 58;
public static readonly var F27 = 59;
public static readonly var F28 = 60;
public static readonly var F29 = 61;
public static readonly var F30 = 62;
public static readonly var F31 = 63;
public static readonly var V0 = 64; // VSX extension, F0 is V0[0:63].
public static readonly var V1 = 65;
public static readonly var V2 = 66;
public static readonly var V3 = 67;
public static readonly var V4 = 68;
public static readonly var V5 = 69;
public static readonly var V6 = 70;
public static readonly var V7 = 71;
public static readonly var V8 = 72;
public static readonly var V9 = 73;
public static readonly var V10 = 74;
public static readonly var V11 = 75;
public static readonly var V12 = 76;
public static readonly var V13 = 77;
public static readonly var V14 = 78;
public static readonly var V15 = 79;
public static readonly var V16 = 80;
public static readonly var V17 = 81;
public static readonly var V18 = 82;
public static readonly var V19 = 83;
public static readonly var V20 = 84;
public static readonly var V21 = 85;
public static readonly var V22 = 86;
public static readonly var V23 = 87;
public static readonly var V24 = 88;
public static readonly var V25 = 89;
public static readonly var V26 = 90;
public static readonly var V27 = 91;
public static readonly var V28 = 92;
public static readonly var V29 = 93;
public static readonly var V30 = 94;
public static readonly var V31 = 95;
public static readonly var VS0 = 96;
public static readonly var VS1 = 97;
public static readonly var VS2 = 98;
public static readonly var VS3 = 99;
public static readonly var VS4 = 100;
public static readonly var VS5 = 101;
public static readonly var VS6 = 102;
public static readonly var VS7 = 103;
public static readonly var VS8 = 104;
public static readonly var VS9 = 105;
public static readonly var VS10 = 106;
public static readonly var VS11 = 107;
public static readonly var VS12 = 108;
public static readonly var VS13 = 109;
public static readonly var VS14 = 110;
public static readonly var VS15 = 111;
public static readonly var VS16 = 112;
public static readonly var VS17 = 113;
public static readonly var VS18 = 114;
public static readonly var VS19 = 115;
public static readonly var VS20 = 116;
public static readonly var VS21 = 117;
public static readonly var VS22 = 118;
public static readonly var VS23 = 119;
public static readonly var VS24 = 120;
public static readonly var VS25 = 121;
public static readonly var VS26 = 122;
public static readonly var VS27 = 123;
public static readonly var VS28 = 124;
public static readonly var VS29 = 125;
public static readonly var VS30 = 126;
public static readonly var VS31 = 127;
public static readonly var VS32 = 128;
public static readonly var VS33 = 129;
public static readonly var VS34 = 130;
public static readonly var VS35 = 131;
public static readonly var VS36 = 132;
public static readonly var VS37 = 133;
public static readonly var VS38 = 134;
public static readonly var VS39 = 135;
public static readonly var VS40 = 136;
public static readonly var VS41 = 137;
public static readonly var VS42 = 138;
public static readonly var VS43 = 139;
public static readonly var VS44 = 140;
public static readonly var VS45 = 141;
public static readonly var VS46 = 142;
public static readonly var VS47 = 143;
public static readonly var VS48 = 144;
public static readonly var VS49 = 145;
public static readonly var VS50 = 146;
public static readonly var VS51 = 147;
public static readonly var VS52 = 148;
public static readonly var VS53 = 149;
public static readonly var VS54 = 150;
public static readonly var VS55 = 151;
public static readonly var VS56 = 152;
public static readonly var VS57 = 153;
public static readonly var VS58 = 154;
public static readonly var VS59 = 155;
public static readonly var VS60 = 156;
public static readonly var VS61 = 157;
public static readonly var VS62 = 158;
public static readonly var VS63 = 159;
public static readonly var A0 = 160; // MMA registers.  These are effectively shadow registers of four adjacent VSR's [An*4,An*4+3]
public static readonly var A1 = 161;
public static readonly var A2 = 162;
public static readonly var A3 = 163;
public static readonly var A4 = 164;
public static readonly var A5 = 165;
public static readonly var A6 = 166;
public static readonly var A7 = 167;


public static void IsArg(this Reg _p0) {
}
public static @string String(this Reg r) {

    if (R0 <= r && r <= R31) 
        return fmt.Sprintf("r%d", int(r - R0));
    else if (F0 <= r && r <= F31) 
        return fmt.Sprintf("f%d", int(r - F0));
    else if (V0 <= r && r <= V31) 
        return fmt.Sprintf("v%d", int(r - V0));
    else if (VS0 <= r && r <= VS63) 
        return fmt.Sprintf("vs%d", int(r - VS0));
    else if (A0 <= r && r <= A7) 
        return fmt.Sprintf("a%d", int(r - A0));
    else 
        return fmt.Sprintf("Reg(%d)", int(r));
    
}

// CondReg is a bit or field in the condition register.
public partial struct CondReg { // : sbyte
}

private static readonly CondReg _ = iota; 
// Condition Regster bits
public static readonly var Cond0LT = 0;
public static readonly var Cond0GT = 1;
public static readonly var Cond0EQ = 2;
public static readonly var Cond0SO = 3;
public static readonly var Cond1LT = 4;
public static readonly var Cond1GT = 5;
public static readonly var Cond1EQ = 6;
public static readonly var Cond1SO = 7;
public static readonly var Cond2LT = 8;
public static readonly var Cond2GT = 9;
public static readonly var Cond2EQ = 10;
public static readonly var Cond2SO = 11;
public static readonly var Cond3LT = 12;
public static readonly var Cond3GT = 13;
public static readonly var Cond3EQ = 14;
public static readonly var Cond3SO = 15;
public static readonly var Cond4LT = 16;
public static readonly var Cond4GT = 17;
public static readonly var Cond4EQ = 18;
public static readonly var Cond4SO = 19;
public static readonly var Cond5LT = 20;
public static readonly var Cond5GT = 21;
public static readonly var Cond5EQ = 22;
public static readonly var Cond5SO = 23;
public static readonly var Cond6LT = 24;
public static readonly var Cond6GT = 25;
public static readonly var Cond6EQ = 26;
public static readonly var Cond6SO = 27;
public static readonly var Cond7LT = 28;
public static readonly var Cond7GT = 29;
public static readonly var Cond7EQ = 30;
public static readonly var Cond7SO = 31; 
// Condition Register Fields
public static readonly var CR0 = 32;
public static readonly var CR1 = 33;
public static readonly var CR2 = 34;
public static readonly var CR3 = 35;
public static readonly var CR4 = 36;
public static readonly var CR5 = 37;
public static readonly var CR6 = 38;
public static readonly var CR7 = 39;


public static void IsArg(this CondReg _p0) {
}
public static @string String(this CondReg c) {

    if (c >= CR0) 
        return fmt.Sprintf("CR%d", int(c - CR0));
    else if (c >= Cond0LT && c < CR0) 
        return fmt.Sprintf("Cond%d%s", int((c - Cond0LT) / 4), new array<@string>(new @string[] { "LT", "GT", "EQ", "SO" })[(c - Cond0LT) % 4]);
    else 
        return fmt.Sprintf("CondReg(%d)", int(c));
    
}

// SpReg is a special register, its meaning depends on Op.
public partial struct SpReg { // : ushort
}

public static readonly SpReg SpRegZero = 0;


public static void IsArg(this SpReg _p0) {
}
public static @string String(this SpReg s) {
    return fmt.Sprintf("SpReg(%d)", int(s));
}

// PCRel is a PC-relative offset, used only in branch instructions.
public partial struct PCRel { // : int
}

public static void IsArg(this PCRel _p0) {
}
public static @string String(this PCRel r) {
    return fmt.Sprintf("PC%+#x", int32(r));
}

// A Label is a code (text) address, used only in absolute branch instructions.
public partial struct Label { // : uint
}

public static void IsArg(this Label _p0) {
}
public static @string String(this Label l) {
    return fmt.Sprintf("%#x", uint32(l));
}

// Imm represents an immediate number.
public partial struct Imm { // : long
}

public static void IsArg(this Imm _p0) {
}
public static @string String(this Imm i) {
    return fmt.Sprintf("%d", int32(i));
}

// Offset represents a memory offset immediate.
public partial struct Offset { // : long
}

public static void IsArg(this Offset _p0) {
}
public static @string String(this Offset o) {
    return fmt.Sprintf("%+d", int32(o));
}

} // end ppc64asm_package
