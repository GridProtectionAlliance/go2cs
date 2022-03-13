// Copyright 2014 The Go Authors.  All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package armasm -- go2cs converted at 2022 March 13 06:37:44 UTC
// import "cmd/vendor/golang.org/x/arch/arm/armasm" ==> using armasm = go.cmd.vendor.golang.org.x.arch.arm.armasm_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\arch\arm\armasm\inst.go
namespace go.cmd.vendor.golang.org.x.arch.arm;

using bytes = bytes_package;
using fmt = fmt_package;


// A Mode is an instruction execution mode.

public static partial class armasm_package {

public partial struct Mode { // : nint
}

private static readonly Mode _ = iota;
public static readonly var ModeARM = 0;
public static readonly var ModeThumb = 1;

public static @string String(this Mode m) {

    if (m == ModeARM) 
        return "ARM";
    else if (m == ModeThumb) 
        return "Thumb";
        return fmt.Sprintf("Mode(%d)", int(m));
}

// An Op is an ARM opcode.
public partial struct Op { // : ushort
}

// NOTE: The actual Op values are defined in tables.go.
// They are chosen to simplify instruction decoding and
// are not a dense packing from 0 to N, although the
// density is high, probably at least 90%.

public static @string String(this Op op) {
    if (op >= Op(len(opstr)) || opstr[op] == "") {
        return fmt.Sprintf("Op(%d)", int(op));
    }
    return opstr[op];
}

// An Inst is a single instruction.
public partial struct Inst {
    public Op Op; // Opcode mnemonic
    public uint Enc; // Raw encoding bits.
    public nint Len; // Length of encoding in bytes.
    public Args Args; // Instruction arguments, in ARM manual order.
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

// An Args holds the instruction arguments.
// If an instruction has fewer than 4 arguments,
// the final elements in the array are nil.
public partial struct Args { // : array<Arg>
}

// An Arg is a single instruction argument, one of these types:
// Endian, Imm, Mem, PCRel, Reg, RegList, RegShift, RegShiftReg.
public partial interface Arg {
    @string IsArg();
    @string String();
}

public partial struct Float32Imm { // : float
}

public static void IsArg(this Float32Imm _p0) {
}

public static @string String(this Float32Imm f) {
    return fmt.Sprintf("#%v", float32(f));
}

public partial struct Float64Imm { // : float
}

public static void IsArg(this Float64Imm _p0) {
}

public static @string String(this Float64Imm f) {
    return fmt.Sprintf("#%v", float64(f));
}

// An Imm is an integer constant.
public partial struct Imm { // : uint
}

public static void IsArg(this Imm _p0) {
}

public static @string String(this Imm i) {
    return fmt.Sprintf("#%#x", uint32(i));
}

// An ImmAlt is an alternate encoding of an integer constant.
public partial struct ImmAlt {
    public byte Val;
    public byte Rot;
}

public static void IsArg(this ImmAlt _p0) {
}

public static Imm Imm(this ImmAlt i) {
    var v = uint32(i.Val);
    var r = uint(i.Rot);
    return Imm(v >> (int)(r) | v << (int)((32 - r)));
}

public static @string String(this ImmAlt i) {
    return fmt.Sprintf("#%#x, %d", i.Val, i.Rot);
}

// A Label is a text (code) address.
public partial struct Label { // : uint
}

public static void IsArg(this Label _p0) {
}

public static @string String(this Label i) {
    return fmt.Sprintf("%#x", uint32(i));
}

// A Reg is a single register.
// The zero value denotes R0, not the absence of a register.
public partial struct Reg { // : byte
}

public static readonly Reg R0 = iota;
public static readonly var R1 = 0;
public static readonly var R2 = 1;
public static readonly var R3 = 2;
public static readonly var R4 = 3;
public static readonly var R5 = 4;
public static readonly var R6 = 5;
public static readonly var R7 = 6;
public static readonly var R8 = 7;
public static readonly var R9 = 8;
public static readonly var R10 = 9;
public static readonly var R11 = 10;
public static readonly var R12 = 11;
public static readonly var R13 = 12;
public static readonly var R14 = 13;
public static readonly var R15 = 14;

public static readonly var S0 = 15;
public static readonly var S1 = 16;
public static readonly var S2 = 17;
public static readonly var S3 = 18;
public static readonly var S4 = 19;
public static readonly var S5 = 20;
public static readonly var S6 = 21;
public static readonly var S7 = 22;
public static readonly var S8 = 23;
public static readonly var S9 = 24;
public static readonly var S10 = 25;
public static readonly var S11 = 26;
public static readonly var S12 = 27;
public static readonly var S13 = 28;
public static readonly var S14 = 29;
public static readonly var S15 = 30;
public static readonly var S16 = 31;
public static readonly var S17 = 32;
public static readonly var S18 = 33;
public static readonly var S19 = 34;
public static readonly var S20 = 35;
public static readonly var S21 = 36;
public static readonly var S22 = 37;
public static readonly var S23 = 38;
public static readonly var S24 = 39;
public static readonly var S25 = 40;
public static readonly var S26 = 41;
public static readonly var S27 = 42;
public static readonly var S28 = 43;
public static readonly var S29 = 44;
public static readonly var S30 = 45;
public static readonly var S31 = 46;

public static readonly var D0 = 47;
public static readonly var D1 = 48;
public static readonly var D2 = 49;
public static readonly var D3 = 50;
public static readonly var D4 = 51;
public static readonly var D5 = 52;
public static readonly var D6 = 53;
public static readonly var D7 = 54;
public static readonly var D8 = 55;
public static readonly var D9 = 56;
public static readonly var D10 = 57;
public static readonly var D11 = 58;
public static readonly var D12 = 59;
public static readonly var D13 = 60;
public static readonly var D14 = 61;
public static readonly var D15 = 62;
public static readonly var D16 = 63;
public static readonly var D17 = 64;
public static readonly var D18 = 65;
public static readonly var D19 = 66;
public static readonly var D20 = 67;
public static readonly var D21 = 68;
public static readonly var D22 = 69;
public static readonly var D23 = 70;
public static readonly var D24 = 71;
public static readonly var D25 = 72;
public static readonly var D26 = 73;
public static readonly var D27 = 74;
public static readonly var D28 = 75;
public static readonly var D29 = 76;
public static readonly var D30 = 77;
public static readonly var D31 = 78;

public static readonly var APSR = 79;
public static readonly var APSR_nzcv = 80;
public static readonly SP FPSCR = R13;
public static readonly var LR = R14;
public static readonly var PC = R15;

public static void IsArg(this Reg _p0) {
}

public static @string String(this Reg r) {

    if (r == APSR) 
        return "APSR";
    else if (r == APSR_nzcv) 
        return "APSR_nzcv";
    else if (r == FPSCR) 
        return "FPSCR";
    else if (r == SP) 
        return "SP";
    else if (r == PC) 
        return "PC";
    else if (r == LR) 
        return "LR";
        if (R0 <= r && r <= R15) {
        return fmt.Sprintf("R%d", int(r - R0));
    }
    if (S0 <= r && r <= S31) {
        return fmt.Sprintf("S%d", int(r - S0));
    }
    if (D0 <= r && r <= D31) {
        return fmt.Sprintf("D%d", int(r - D0));
    }
    return fmt.Sprintf("Reg(%d)", int(r));
}

// A RegX represents a fraction of a multi-value register.
// The Index field specifies the index number,
// but the size of the fraction is not specified.
// It must be inferred from the instruction and the register type.
// For example, in a VMOV instruction, RegX{D5, 1} represents
// the top 32 bits of the 64-bit D5 register.
public partial struct RegX {
    public Reg Reg;
    public nint Index;
}

public static void IsArg(this RegX _p0) {
}

public static @string String(this RegX r) {
    return fmt.Sprintf("%s[%d]", r.Reg, r.Index);
}

// A RegList is a register list.
// Bits at indexes x = 0 through 15 indicate whether the corresponding Rx register is in the list.
public partial struct RegList { // : ushort
}

public static void IsArg(this RegList _p0) {
}

public static @string String(this RegList r) {
    ref bytes.Buffer buf = ref heap(out ptr<bytes.Buffer> _addr_buf);
    fmt.Fprintf(_addr_buf, "{");
    @string sep = "";
    for (nint i = 0; i < 16; i++) {
        if (r & (1 << (int)(uint(i))) != 0) {
            fmt.Fprintf(_addr_buf, "%s%s", sep, Reg(i).String());
            sep = ",";
        }
    }
    fmt.Fprintf(_addr_buf, "}");
    return buf.String();
}

// An Endian is the argument to the SETEND instruction.
public partial struct Endian { // : byte
}

public static readonly Endian LittleEndian = 0;
public static readonly Endian BigEndian = 1;

public static void IsArg(this Endian _p0) {
}

public static @string String(this Endian e) {
    if (e != 0) {
        return "BE";
    }
    return "LE";
}

// A Shift describes an ARM shift operation.
public partial struct Shift { // : byte
}

public static readonly Shift ShiftLeft = 0; // left shift
public static readonly Shift ShiftRight = 1; // logical (unsigned) right shift
public static readonly Shift ShiftRightSigned = 2; // arithmetic (signed) right shift
public static readonly Shift RotateRight = 3; // right rotate
public static readonly Shift RotateRightExt = 4; // right rotate through carry (Count will always be 1)

private static array<@string> shiftName = new array<@string>(new @string[] { "LSL", "LSR", "ASR", "ROR", "RRX" });

public static @string String(this Shift s) {
    if (s < 5) {
        return shiftName[s];
    }
    return fmt.Sprintf("Shift(%d)", int(s));
}

// A RegShift is a register shifted by a constant.
public partial struct RegShift {
    public Reg Reg;
    public Shift Shift;
    public byte Count;
}

public static void IsArg(this RegShift _p0) {
}

public static @string String(this RegShift r) {
    return fmt.Sprintf("%s %s #%d", r.Reg, r.Shift, r.Count);
}

// A RegShiftReg is a register shifted by a register.
public partial struct RegShiftReg {
    public Reg Reg;
    public Shift Shift;
    public Reg RegCount;
}

public static void IsArg(this RegShiftReg _p0) {
}

public static @string String(this RegShiftReg r) {
    return fmt.Sprintf("%s %s %s", r.Reg, r.Shift, r.RegCount);
}

// A PCRel describes a memory address (usually a code label)
// as a distance relative to the program counter.
// TODO(rsc): Define which program counter (PC+4? PC+8? PC?).
public partial struct PCRel { // : int
}

public static void IsArg(this PCRel _p0) {
}

public static @string String(this PCRel r) {
    return fmt.Sprintf("PC%+#x", int32(r));
}

// An AddrMode is an ARM addressing mode.
public partial struct AddrMode { // : byte
}

private static readonly AddrMode _ = iota;
public static readonly var AddrPostIndex = 0; // [R], X – use address R, set R = R + X
public static readonly var AddrPreIndex = 1; // [R, X]! – use address R + X, set R = R + X
public static readonly var AddrOffset = 2; // [R, X] – use address R + X
public static readonly var AddrLDM = 3; // R – [R] but formats as R, for LDM/STM only
public static readonly var AddrLDM_WB = 4; // R! - [R], X where X is instruction-specific amount, for LDM/STM only

// A Mem is a memory reference made up of a base R and index expression X.
// The effective memory address is R or R+X depending on AddrMode.
// The index expression is X = Sign*(Index Shift Count) + Offset,
// but in any instruction either Sign = 0 or Offset = 0.
public partial struct Mem {
    public Reg Base;
    public AddrMode Mode;
    public sbyte Sign;
    public Reg Index;
    public Shift Shift;
    public byte Count;
    public short Offset;
}

public static void IsArg(this Mem _p0) {
}

public static @string String(this Mem m) {
    var R = m.Base.String();
    @string X = "";
    if (m.Sign != 0) {
        X = "+";
        if (m.Sign < 0) {
            X = "-";
        }
        X += m.Index.String();
        if (m.Shift != ShiftLeft || m.Count != 0) {
            X += fmt.Sprintf(", %s #%d", m.Shift, m.Count);
        }
    }
    else
 {
        X = fmt.Sprintf("#%d", m.Offset);
    }

    if (m.Mode == AddrOffset) 
        if (X == "#0") {
            return fmt.Sprintf("[%s]", R);
        }
        return fmt.Sprintf("[%s, %s]", R, X);
    else if (m.Mode == AddrPreIndex) 
        return fmt.Sprintf("[%s, %s]!", R, X);
    else if (m.Mode == AddrPostIndex) 
        return fmt.Sprintf("[%s], %s", R, X);
    else if (m.Mode == AddrLDM) 
        if (X == "#0") {
            return R;
        }
    else if (m.Mode == AddrLDM_WB) 
        if (X == "#0") {
            return R + "!";
        }
        return fmt.Sprintf("[%s Mode(%d) %s]", R, int(m.Mode), X);
}

} // end armasm_package
