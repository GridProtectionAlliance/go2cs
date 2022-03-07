// Copyright 2014 The Go Authors.  All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package armasm -- go2cs converted at 2022 March 06 23:24:33 UTC
// import "cmd/vendor/golang.org/x/arch/arm/armasm" ==> using armasm = go.cmd.vendor.golang.org.x.arch.arm.armasm_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\arch\arm\armasm\decode.go
using binary = go.encoding.binary_package;
using fmt = go.fmt_package;

namespace go.cmd.vendor.golang.org.x.arch.arm;

public static partial class armasm_package {

    // An instFormat describes the format of an instruction encoding.
    // An instruction with 32-bit value x matches the format if x&mask == value
    // and the condition matches.
    // The condition matches if x>>28 == 0xF && value>>28==0xF
    // or if x>>28 != 0xF and value>>28 == 0.
    // If x matches the format, then the rest of the fields describe how to interpret x.
    // The opBits describe bits that should be extracted from x and added to the opcode.
    // For example opBits = 0x1234 means that the value
    //    (2 bits at offset 1) followed by (4 bits at offset 3)
    // should be added to op.
    // Finally the args describe how to decode the instruction arguments.
    // args is stored as a fixed-size array; if there are fewer than len(args) arguments,
    // args[i] == 0 marks the end of the argument list.
private partial struct instFormat {
    public uint mask;
    public uint value;
    public sbyte priority;
    public Op op;
    public ulong opBits;
    public instArgs args;
}

private partial struct instArgs { // : array<instArg>
}

private static var errMode = fmt.Errorf("unsupported execution mode");private static var errShort = fmt.Errorf("truncated instruction");private static var errUnknown = fmt.Errorf("unknown instruction");

private static slice<bool> decoderCover = default;

// Decode decodes the leading bytes in src as a single instruction.
public static (Inst, error) Decode(slice<byte> src, Mode mode) {
    Inst inst = default;
    error err = default!;

    if (mode != ModeARM) {
        return (new Inst(), error.As(errMode)!);
    }
    if (len(src) < 4) {
        return (new Inst(), error.As(errShort)!);
    }
    if (decoderCover == null) {
        decoderCover = make_slice<bool>(len(instFormats));
    }
    var x = binary.LittleEndian.Uint32(src); 

    // The instFormat table contains both conditional and unconditional instructions.
    // Considering only the top 4 bits, the conditional instructions use mask=0, value=0,
    // while the unconditional instructions use mask=f, value=f.
    // Prepare a version of x with the condition cleared to 0 in conditional instructions
    // and then assume mask=f during matching.
    const nuint condMask = 0xf0000000;

    var xNoCond = x;
    if (x & condMask != condMask) {
        xNoCond &= condMask;
    }
    sbyte priority = default;
Search:
    foreach (var (i) in instFormats) {
        var f = _addr_instFormats[i];
        if (xNoCond & (f.mask | condMask) != f.value || f.priority <= priority) {
            continue;
        }
        var delta = uint32(0);
        var deltaShift = uint(0);
        {
            var opBits = f.opBits;

            while (opBits != 0) {
                var n = uint(opBits & 0xFF);
                var off = uint((opBits >> 8) & 0xFF);
                delta |= (x >> (int)(off)) & (1 << (int)(n) - 1) << (int)(deltaShift);
                deltaShift += n;
                opBits>>=16;
            }

        }
        var op = f.op + Op(delta); 

        // Special case: BKPT encodes with condition but cannot have one.
        if (op & ~15 == BKPT_EQ && op != BKPT) {
            _continueSearch = true;
            break;
        }
        Args args = default;
        foreach (var (j, aop) in f.args) {
            if (aop == 0) {
                break;
            }
            var arg = decodeArg(aop, x);
            if (arg == null) { // cannot decode argument
                _continueSearch = true;
                break;
            }

            args[j] = arg;

        }        decoderCover[i] = true;

        inst = new Inst(Op:op,Args:args,Enc:x,Len:4,);
        priority = f.priority;
        _continueSearch = true;
        break;
    }    if (inst.Op != 0) {
        return (inst, error.As(null!)!);
    }
    return (new Inst(), error.As(errUnknown)!);

}

// An instArg describes the encoding of a single argument.
// In the names used for arguments, _p_ means +, _m_ means -,
// _pm_ means ± (usually keyed by the U bit).
// The _W suffix indicates a general addressing mode based on the P and W bits.
// The _offset and _postindex suffixes force the given addressing mode.
// The rest should be somewhat self-explanatory, at least given
// the decodeArg function.
private partial struct instArg { // : byte
}

private static readonly instArg _ = iota;
private static readonly var arg_APSR = 0;
private static readonly var arg_FPSCR = 1;
private static readonly var arg_Dn_half = 2;
private static readonly var arg_R1_0 = 3;
private static readonly var arg_R1_12 = 4;
private static readonly var arg_R2_0 = 5;
private static readonly var arg_R2_12 = 6;
private static readonly var arg_R_0 = 7;
private static readonly var arg_R_12 = 8;
private static readonly var arg_R_12_nzcv = 9;
private static readonly var arg_R_16 = 10;
private static readonly var arg_R_16_WB = 11;
private static readonly var arg_R_8 = 12;
private static readonly var arg_R_rotate = 13;
private static readonly var arg_R_shift_R = 14;
private static readonly var arg_R_shift_imm = 15;
private static readonly var arg_SP = 16;
private static readonly var arg_Sd = 17;
private static readonly var arg_Sd_Dd = 18;
private static readonly var arg_Dd_Sd = 19;
private static readonly var arg_Sm = 20;
private static readonly var arg_Sm_Dm = 21;
private static readonly var arg_Sn = 22;
private static readonly var arg_Sn_Dn = 23;
private static readonly var arg_const = 24;
private static readonly var arg_endian = 25;
private static readonly var arg_fbits = 26;
private static readonly var arg_fp_0 = 27;
private static readonly var arg_imm24 = 28;
private static readonly var arg_imm5 = 29;
private static readonly var arg_imm5_32 = 30;
private static readonly var arg_imm5_nz = 31;
private static readonly var arg_imm_12at8_4at0 = 32;
private static readonly var arg_imm_4at16_12at0 = 33;
private static readonly var arg_imm_vfp = 34;
private static readonly var arg_label24 = 35;
private static readonly var arg_label24H = 36;
private static readonly var arg_label_m_12 = 37;
private static readonly var arg_label_p_12 = 38;
private static readonly var arg_label_pm_12 = 39;
private static readonly var arg_label_pm_4_4 = 40;
private static readonly var arg_lsb_width = 41;
private static readonly var arg_mem_R = 42;
private static readonly var arg_mem_R_pm_R_W = 43;
private static readonly var arg_mem_R_pm_R_postindex = 44;
private static readonly var arg_mem_R_pm_R_shift_imm_W = 45;
private static readonly var arg_mem_R_pm_R_shift_imm_offset = 46;
private static readonly var arg_mem_R_pm_R_shift_imm_postindex = 47;
private static readonly var arg_mem_R_pm_imm12_W = 48;
private static readonly var arg_mem_R_pm_imm12_offset = 49;
private static readonly var arg_mem_R_pm_imm12_postindex = 50;
private static readonly var arg_mem_R_pm_imm8_W = 51;
private static readonly var arg_mem_R_pm_imm8_postindex = 52;
private static readonly var arg_mem_R_pm_imm8at0_offset = 53;
private static readonly var arg_option = 54;
private static readonly var arg_registers = 55;
private static readonly var arg_registers1 = 56;
private static readonly var arg_registers2 = 57;
private static readonly var arg_satimm4 = 58;
private static readonly var arg_satimm5 = 59;
private static readonly var arg_satimm4m1 = 60;
private static readonly var arg_satimm5m1 = 61;
private static readonly var arg_widthm1 = 62;


// decodeArg decodes the arg described by aop from the instruction bits x.
// It returns nil if x cannot be decoded according to aop.
private static Arg decodeArg(instArg aop, uint x) {

    if (aop == arg_APSR) 
        return APSR;
    else if (aop == arg_FPSCR) 
        return FPSCR;
    else if (aop == arg_R_0) 
        return Reg(x & (1 << 4 - 1));
    else if (aop == arg_R_8) 
        return Reg((x >> 8) & (1 << 4 - 1));
    else if (aop == arg_R_12) 
        return Reg((x >> 12) & (1 << 4 - 1));
    else if (aop == arg_R_16) 
        return Reg((x >> 16) & (1 << 4 - 1));
    else if (aop == arg_R_12_nzcv) 
        var r = Reg((x >> 12) & (1 << 4 - 1));
        if (r == R15) {
            return APSR_nzcv;
        }
        return r;
    else if (aop == arg_R_16_WB) 
        var mode = AddrLDM;
        if ((x >> 21) & 1 != 0) {
            mode = AddrLDM_WB;
        }
        return new Mem(Base:Reg((x>>16)&(1<<4-1)),Mode:mode);
    else if (aop == arg_R_rotate) 
        var Rm = Reg(x & (1 << 4 - 1));
        var (typ, count) = decodeShift(x); 
        // ROR #0 here means ROR #0, but decodeShift rewrites to RRX #1.
        if (typ == RotateRightExt) {
            return Reg(Rm);
        }
        return new RegShift(Rm,typ,uint8(count));
    else if (aop == arg_R_shift_R) 
        Rm = Reg(x & (1 << 4 - 1));
        var Rs = Reg((x >> 8) & (1 << 4 - 1));
        var typ = Shift((x >> 5) & (1 << 2 - 1));
        return new RegShiftReg(Rm,typ,Rs);
    else if (aop == arg_R_shift_imm) 
        Rm = Reg(x & (1 << 4 - 1));
        (typ, count) = decodeShift(x);
        if (typ == ShiftLeft && count == 0) {
            return Reg(Rm);
        }
        return new RegShift(Rm,typ,uint8(count));
    else if (aop == arg_R1_0) 
        return Reg((x & (1 << 4 - 1)));
    else if (aop == arg_R1_12) 
        return Reg(((x >> 12) & (1 << 4 - 1)));
    else if (aop == arg_R2_0) 
        return Reg((x & (1 << 4 - 1)) | 1);
    else if (aop == arg_R2_12) 
        return Reg(((x >> 12) & (1 << 4 - 1)) | 1);
    else if (aop == arg_SP) 
        return SP;
    else if (aop == arg_Sd_Dd) 
        var v = (x >> 12) & (1 << 4 - 1);
        var vx = (x >> 22) & 1;
        var sz = (x >> 8) & 1;
        if (sz != 0) {
            return D0 + Reg(vx << 4 + v);
        }
        else
 {
            return S0 + Reg(v << 1 + vx);
        }
    else if (aop == arg_Dd_Sd) 
        return decodeArg(arg_Sd_Dd, x ^ (1 << 8));
    else if (aop == arg_Sd) 
        v = (x >> 12) & (1 << 4 - 1);
        vx = (x >> 22) & 1;
        return S0 + Reg(v << 1 + vx);
    else if (aop == arg_Sm_Dm) 
        v = (x >> 0) & (1 << 4 - 1);
        vx = (x >> 5) & 1;
        sz = (x >> 8) & 1;
        if (sz != 0) {
            return D0 + Reg(vx << 4 + v);
        }
        else
 {
            return S0 + Reg(v << 1 + vx);
        }
    else if (aop == arg_Sm) 
        v = (x >> 0) & (1 << 4 - 1);
        vx = (x >> 5) & 1;
        return S0 + Reg(v << 1 + vx);
    else if (aop == arg_Dn_half) 
        v = (x >> 16) & (1 << 4 - 1);
        vx = (x >> 7) & 1;
        return new RegX(D0+Reg(vx<<4+v),int((x>>21)&1));
    else if (aop == arg_Sn_Dn) 
        v = (x >> 16) & (1 << 4 - 1);
        vx = (x >> 7) & 1;
        sz = (x >> 8) & 1;
        if (sz != 0) {
            return D0 + Reg(vx << 4 + v);
        }
        else
 {
            return S0 + Reg(v << 1 + vx);
        }
    else if (aop == arg_Sn) 
        v = (x >> 16) & (1 << 4 - 1);
        vx = (x >> 7) & 1;
        return S0 + Reg(v << 1 + vx);
    else if (aop == arg_const) 
        v = x & (1 << 8 - 1);
        var rot = (x >> 8) & (1 << 4 - 1) * 2;
        if (rot > 0 && v & 3 == 0) { 
            // could rotate less
            return new ImmAlt(uint8(v),uint8(rot));

        }
        if (rot >= 24 && ((v << (int)((32 - rot))) & 0xFF) >> (int)((32 - rot)) == v) { 
            // could wrap around to rot==0.
            return new ImmAlt(uint8(v),uint8(rot));

        }
        return Imm(v >> (int)(rot) | v << (int)((32 - rot)));
    else if (aop == arg_endian) 
        return Endian((x >> 9) & 1);
    else if (aop == arg_fbits) 
        return Imm((16 << (int)(((x >> 7) & 1))) - ((x & (1 << 4 - 1)) << 1 | (x >> 5) & 1));
    else if (aop == arg_fp_0) 
        return Imm(0);
    else if (aop == arg_imm24) 
        return Imm(x & (1 << 24 - 1));
    else if (aop == arg_imm5) 
        return Imm((x >> 7) & (1 << 5 - 1));
    else if (aop == arg_imm5_32) 
        x = (x >> 7) & (1 << 5 - 1);
        if (x == 0) {
            x = 32;
        }
        return Imm(x);
    else if (aop == arg_imm5_nz) 
        x = (x >> 7) & (1 << 5 - 1);
        if (x == 0) {
            return null;
        }
        return Imm(x);
    else if (aop == arg_imm_4at16_12at0) 
        return Imm((x >> 16) & (1 << 4 - 1) << 12 | x & (1 << 12 - 1));
    else if (aop == arg_imm_12at8_4at0) 
        return Imm((x >> 8) & (1 << 12 - 1) << 4 | x & (1 << 4 - 1));
    else if (aop == arg_imm_vfp) 
        x = (x >> 16) & (1 << 4 - 1) << 4 | x & (1 << 4 - 1);
        return Imm(x);
    else if (aop == arg_label24) 
        var imm = (x & (1 << 24 - 1)) << 2;
        return PCRel(int32(imm << 6) >> 6);
    else if (aop == arg_label24H) 
        var h = (x >> 24) & 1;
        imm = (x & (1 << 24 - 1)) << 2 | h << 1;
        return PCRel(int32(imm << 6) >> 6);
    else if (aop == arg_label_m_12) 
        var d = int32(x & (1 << 12 - 1));
        return new Mem(Base:PC,Mode:AddrOffset,Offset:int16(-d));
    else if (aop == arg_label_p_12) 
        d = int32(x & (1 << 12 - 1));
        return new Mem(Base:PC,Mode:AddrOffset,Offset:int16(d));
    else if (aop == arg_label_pm_12) 
        d = int32(x & (1 << 12 - 1));
        var u = (x >> 23) & 1;
        if (u == 0) {
            d = -d;
        }
        return new Mem(Base:PC,Mode:AddrOffset,Offset:int16(d));
    else if (aop == arg_label_pm_4_4) 
        d = int32((x >> 8) & (1 << 4 - 1) << 4 | x & (1 << 4 - 1));
        u = (x >> 23) & 1;
        if (u == 0) {
            d = -d;
        }
        return PCRel(d);
    else if (aop == arg_lsb_width) 
        var lsb = (x >> 7) & (1 << 5 - 1);
        var msb = (x >> 16) & (1 << 5 - 1);
        if (msb < lsb || msb >= 32) {
            return null;
        }
        return Imm(msb + 1 - lsb);
    else if (aop == arg_mem_R) 
        var Rn = Reg((x >> 16) & (1 << 4 - 1));
        return new Mem(Base:Rn,Mode:AddrOffset);
    else if (aop == arg_mem_R_pm_R_postindex) 
        // Treat [<Rn>],+/-<Rm> like [<Rn>,+/-<Rm>{,<shift>}]{!}
        // by forcing shift bits to <<0 and P=0, W=0 (postindex=true).
        return decodeArg(arg_mem_R_pm_R_shift_imm_W, x & ~((1 << 7 - 1) << 5 | 1 << 24 | 1 << 21));
    else if (aop == arg_mem_R_pm_R_W) 
        // Treat [<Rn>,+/-<Rm>]{!} like [<Rn>,+/-<Rm>{,<shift>}]{!}
        // by forcing shift bits to <<0.
        return decodeArg(arg_mem_R_pm_R_shift_imm_W, x & ~((1 << 7 - 1) << 5));
    else if (aop == arg_mem_R_pm_R_shift_imm_offset) 
        // Treat [<Rn>],+/-<Rm>{,<shift>} like [<Rn>,+/-<Rm>{,<shift>}]{!}
        // by forcing P=1, W=0 (index=false, wback=false).
        return decodeArg(arg_mem_R_pm_R_shift_imm_W, x & ~(1 << 21) | 1 << 24);
    else if (aop == arg_mem_R_pm_R_shift_imm_postindex) 
        // Treat [<Rn>],+/-<Rm>{,<shift>} like [<Rn>,+/-<Rm>{,<shift>}]{!}
        // by forcing P=0, W=0 (postindex=true).
        return decodeArg(arg_mem_R_pm_R_shift_imm_W, x & ~(1 << 24 | 1 << 21));
    else if (aop == arg_mem_R_pm_R_shift_imm_W) 
        Rn = Reg((x >> 16) & (1 << 4 - 1));
        Rm = Reg(x & (1 << 4 - 1));
        (typ, count) = decodeShift(x);
        u = (x >> 23) & 1;
        var w = (x >> 21) & 1;
        var p = (x >> 24) & 1;
        if (p == 0 && w == 1) {
            return null;
        }
        var sign = int8(+1);
        if (u == 0) {
            sign = -1;
        }
        mode = AddrMode(uint8(p << 1) | uint8(w ^ 1));
        return new Mem(Base:Rn,Mode:mode,Sign:sign,Index:Rm,Shift:typ,Count:count);
    else if (aop == arg_mem_R_pm_imm12_offset) 
        // Treat [<Rn>,#+/-<imm12>] like [<Rn>{,#+/-<imm12>}]{!}
        // by forcing P=1, W=0 (index=false, wback=false).
        return decodeArg(arg_mem_R_pm_imm12_W, x & ~(1 << 21) | 1 << 24);
    else if (aop == arg_mem_R_pm_imm12_postindex) 
        // Treat [<Rn>],#+/-<imm12> like [<Rn>{,#+/-<imm12>}]{!}
        // by forcing P=0, W=0 (postindex=true).
        return decodeArg(arg_mem_R_pm_imm12_W, x & ~(1 << 24 | 1 << 21));
    else if (aop == arg_mem_R_pm_imm12_W) 
        Rn = Reg((x >> 16) & (1 << 4 - 1));
        u = (x >> 23) & 1;
        w = (x >> 21) & 1;
        p = (x >> 24) & 1;
        if (p == 0 && w == 1) {
            return null;
        }
        sign = int8(+1);
        if (u == 0) {
            sign = -1;
        }
        imm = int16(x & (1 << 12 - 1));
        mode = AddrMode(uint8(p << 1) | uint8(w ^ 1));
        return new Mem(Base:Rn,Mode:mode,Offset:int16(sign)*imm);
    else if (aop == arg_mem_R_pm_imm8_postindex) 
        // Treat [<Rn>],#+/-<imm8> like [<Rn>{,#+/-<imm8>}]{!}
        // by forcing P=0, W=0 (postindex=true).
        return decodeArg(arg_mem_R_pm_imm8_W, x & ~(1 << 24 | 1 << 21));
    else if (aop == arg_mem_R_pm_imm8_W) 
        Rn = Reg((x >> 16) & (1 << 4 - 1));
        u = (x >> 23) & 1;
        w = (x >> 21) & 1;
        p = (x >> 24) & 1;
        if (p == 0 && w == 1) {
            return null;
        }
        sign = int8(+1);
        if (u == 0) {
            sign = -1;
        }
        imm = int16((x >> 8) & (1 << 4 - 1) << 4 | x & (1 << 4 - 1));
        mode = AddrMode(uint8(p << 1) | uint8(w ^ 1));
        return new Mem(Base:Rn,Mode:mode,Offset:int16(sign)*imm);
    else if (aop == arg_mem_R_pm_imm8at0_offset) 
        Rn = Reg((x >> 16) & (1 << 4 - 1));
        u = (x >> 23) & 1;
        sign = int8(+1);
        if (u == 0) {
            sign = -1;
        }
        imm = int16(x & (1 << 8 - 1)) << 2;
        return new Mem(Base:Rn,Mode:AddrOffset,Offset:int16(sign)*imm);
    else if (aop == arg_option) 
        return Imm(x & (1 << 4 - 1));
    else if (aop == arg_registers) 
        return RegList(x & (1 << 16 - 1));
    else if (aop == arg_registers2) 
        x &= 1 << 16 - 1;
        nint n = 0;
        for (nint i = 0; i < 16; i++) {
            if (x >> (int)(uint(i)) & 1 != 0) {
                n++;
            }
        }
        if (n < 2) {
            return null;
        }
        return RegList(x);
    else if (aop == arg_registers1) 
        var Rt = (x >> 12) & (1 << 4 - 1);
        return RegList(1 << (int)(Rt));
    else if (aop == arg_satimm4) 
        return Imm((x >> 16) & (1 << 4 - 1));
    else if (aop == arg_satimm5) 
        return Imm((x >> 16) & (1 << 5 - 1));
    else if (aop == arg_satimm4m1) 
        return Imm((x >> 16) & (1 << 4 - 1) + 1);
    else if (aop == arg_satimm5m1) 
        return Imm((x >> 16) & (1 << 5 - 1) + 1);
    else if (aop == arg_widthm1) 
        return Imm((x >> 16) & (1 << 5 - 1) + 1);
    else 
        return null;
    
}

// decodeShift decodes the shift-by-immediate encoded in x.
private static (Shift, byte) decodeShift(uint x) {
    Shift _p0 = default;
    byte _p0 = default;

    var count = (x >> 7) & (1 << 5 - 1);
    var typ = Shift((x >> 5) & (1 << 2 - 1));

    if (typ == ShiftRight || typ == ShiftRightSigned) 
        if (count == 0) {
            count = 32;
        }
    else if (typ == RotateRight) 
        if (count == 0) {
            typ = RotateRightExt;
            count = 1;
        }
        return (typ, uint8(count));

}

} // end armasm_package
