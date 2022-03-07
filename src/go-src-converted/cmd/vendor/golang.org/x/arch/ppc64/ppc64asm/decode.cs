// Copyright 2014 The Go Authors.  All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ppc64asm -- go2cs converted at 2022 March 06 23:25:04 UTC
// import "cmd/vendor/golang.org/x/arch/ppc64/ppc64asm" ==> using ppc64asm = go.cmd.vendor.golang.org.x.arch.ppc64.ppc64asm_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\arch\ppc64\ppc64asm\decode.go
using binary = go.encoding.binary_package;
using fmt = go.fmt_package;
using log = go.log_package;

namespace go.cmd.vendor.golang.org.x.arch.ppc64;

public static partial class ppc64asm_package {

private static readonly var debugDecode = false;



private static readonly nint prefixOpcode = 1;

// instFormat is a decoding rule for one specific instruction form.
// an instruction ins matches the rule if ins&Mask == Value
// DontCare bits should be zero, but the machine might not reject
// ones in those bits, they are mainly reserved for future expansion
// of the instruction set.
// The Args are stored in the same order as the instruction manual.
//
// Prefixed instructions are stored as:
//   prefix << 32 | suffix,
// Regular instructions are:
//   inst << 32


// instFormat is a decoding rule for one specific instruction form.
// an instruction ins matches the rule if ins&Mask == Value
// DontCare bits should be zero, but the machine might not reject
// ones in those bits, they are mainly reserved for future expansion
// of the instruction set.
// The Args are stored in the same order as the instruction manual.
//
// Prefixed instructions are stored as:
//   prefix << 32 | suffix,
// Regular instructions are:
//   inst << 32
private partial struct instFormat {
    public Op Op;
    public ulong Mask;
    public ulong Value;
    public ulong DontCare;
    public array<ptr<argField>> Args;
}

// argField indicate how to decode an argument to an instruction.
// First parse the value from the BitFields, shift it left by Shift
// bits to get the actual numerical value.
private partial struct argField {
    public ArgType Type;
    public byte Shift;
    public ref BitFields BitFields => ref BitFields_val;
}

// Parse parses the Arg out from the given binary instruction i.
private static Arg Parse(this argField a, array<uint> i) {
    i = i.Clone();


    if (a.Type == TypeUnknown) 
        return null;
    else if (a.Type == TypeReg) 
        return R0 + Reg(a.BitFields.Parse(i));
    else if (a.Type == TypeCondRegBit) 
        return Cond0LT + CondReg(a.BitFields.Parse(i));
    else if (a.Type == TypeCondRegField) 
        return CR0 + CondReg(a.BitFields.Parse(i));
    else if (a.Type == TypeFPReg) 
        return F0 + Reg(a.BitFields.Parse(i));
    else if (a.Type == TypeVecReg) 
        return V0 + Reg(a.BitFields.Parse(i));
    else if (a.Type == TypeVecSReg) 
        return VS0 + Reg(a.BitFields.Parse(i));
    else if (a.Type == TypeVecSpReg) 
        return VS0 + Reg(a.BitFields.Parse(i)) * 2;
    else if (a.Type == TypeMMAReg) 
        return A0 + Reg(a.BitFields.Parse(i));
    else if (a.Type == TypeSpReg) 
        return SpReg(a.BitFields.Parse(i));
    else if (a.Type == TypeImmSigned) 
        return Imm(a.BitFields.ParseSigned(i) << (int)(a.Shift));
    else if (a.Type == TypeImmUnsigned) 
        return Imm(a.BitFields.Parse(i) << (int)(a.Shift));
    else if (a.Type == TypePCRel) 
        return PCRel(a.BitFields.ParseSigned(i) << (int)(a.Shift));
    else if (a.Type == TypeLabel) 
        return Label(a.BitFields.ParseSigned(i) << (int)(a.Shift));
    else if (a.Type == TypeOffset) 
        return Offset(a.BitFields.ParseSigned(i) << (int)(a.Shift));
    else 
        return null;
    
}

public partial struct ArgType { // : sbyte
}

public static readonly ArgType TypeUnknown = iota;
public static readonly var TypePCRel = 0; // PC-relative address
public static readonly var TypeLabel = 1; // absolute address
public static readonly var TypeReg = 2; // integer register
public static readonly var TypeCondRegBit = 3; // conditional register bit (0-31)
public static readonly var TypeCondRegField = 4; // conditional register field (0-7)
public static readonly var TypeFPReg = 5; // floating point register
public static readonly var TypeVecReg = 6; // vector register
public static readonly var TypeVecSReg = 7; // VSX register
public static readonly var TypeVecSpReg = 8; // VSX register pair (even only encoding)
public static readonly var TypeMMAReg = 9; // MMA register
public static readonly var TypeSpReg = 10; // special register (depends on Op)
public static readonly var TypeImmSigned = 11; // signed immediate
public static readonly var TypeImmUnsigned = 12; // unsigned immediate/flag/mask, this is the catch-all type
public static readonly var TypeOffset = 13; // signed offset in load/store
public static readonly var TypeLast = 14; // must be the last one

public static @string String(this ArgType t) {

    if (t == TypeUnknown) 
        return "Unknown";
    else if (t == TypeReg) 
        return "Reg";
    else if (t == TypeCondRegBit) 
        return "CondRegBit";
    else if (t == TypeCondRegField) 
        return "CondRegField";
    else if (t == TypeFPReg) 
        return "FPReg";
    else if (t == TypeVecReg) 
        return "VecReg";
    else if (t == TypeVecSReg) 
        return "VecSReg";
    else if (t == TypeVecSpReg) 
        return "VecSpReg";
    else if (t == TypeMMAReg) 
        return "MMAReg";
    else if (t == TypeSpReg) 
        return "SpReg";
    else if (t == TypeImmSigned) 
        return "ImmSigned";
    else if (t == TypeImmUnsigned) 
        return "ImmUnsigned";
    else if (t == TypePCRel) 
        return "PCRel";
    else if (t == TypeLabel) 
        return "Label";
    else if (t == TypeOffset) 
        return "Offset";
    else 
        return fmt.Sprintf("ArgType(%d)", int(t));
    
}

public static @string GoString(this ArgType t) {
    var s = t.String();
    if (t > 0 && t < TypeLast) {
        return "Type" + s;
    }
    return s;

}

 
// Errors
private static var errShort = fmt.Errorf("truncated instruction");private static var errUnknown = fmt.Errorf("unknown instruction");

private static slice<bool> decoderCover = default;

// Decode decodes the leading bytes in src as a single instruction using
// byte order ord.
public static (Inst, error) Decode(slice<byte> src, binary.ByteOrder ord) {
    Inst inst = default;
    error err = default!;

    if (len(src) < 4) {
        return (inst, error.As(errShort)!);
    }
    if (decoderCover == null) {
        decoderCover = make_slice<bool>(len(instFormats));
    }
    inst.Len = 4;
    array<uint> ui_extn = new array<uint>(new uint[] { ord.Uint32(src[:inst.Len]), 0 });
    var ui = uint64(ui_extn[0]) << 32;
    inst.Enc = ui_extn[0];
    var opcode = inst.Enc >> 26;
    if (opcode == prefixOpcode) { 
        // This is a prefixed instruction
        inst.Len = 8;
        if (len(src) < 8) {
            return (inst, error.As(errShort)!);
        }
        ui_extn[1] = ord.Uint32(src[(int)4..(int)inst.Len]);
        ui |= uint64(ui_extn[1]);
        inst.SuffixEnc = ui_extn[1];

    }
    {
        var i__prev1 = i;

        foreach (var (__i, __iform) in instFormats) {
            i = __i;
            iform = __iform;
            if (ui & iform.Mask != iform.Value) {
                continue;
            }
            if (ui & iform.DontCare != 0) {
                if (debugDecode) {
                    log.Printf("Decode(%#x): unused bit is 1 for Op %s", ui, iform.Op);
                } 
                // to match GNU objdump (libopcodes), we ignore don't care bits
            }

            {
                var i__prev2 = i;

                foreach (var (__i, __argfield) in iform.Args) {
                    i = __i;
                    argfield = __argfield;
                    if (argfield == null) {
                        break;
                    }
                    inst.Args[i] = argfield.Parse(ui_extn);
                }

                i = i__prev2;
            }

            inst.Op = iform.Op;
            if (debugDecode) {
                log.Printf("%#x: search entry %d", ui, i);
                continue;
            }

            break;

        }
        i = i__prev1;
    }

    if (inst.Op == 0 && inst.Enc != 0) {
        return (inst, error.As(errUnknown)!);
    }
    return (inst, error.As(null!)!);

}

} // end ppc64asm_package
