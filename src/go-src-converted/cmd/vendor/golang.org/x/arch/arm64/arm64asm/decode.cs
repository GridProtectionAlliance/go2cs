// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package arm64asm -- go2cs converted at 2022 March 06 23:24:56 UTC
// import "cmd/vendor/golang.org/x/arch/arm64/arm64asm" ==> using arm64asm = go.cmd.vendor.golang.org.x.arch.arm64.arm64asm_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\arch\arm64\arm64asm\decode.go
using binary = go.encoding.binary_package;
using fmt = go.fmt_package;
using System;


namespace go.cmd.vendor.golang.org.x.arch.arm64;

public static partial class arm64asm_package {

private partial struct instArgs { // : array<instArg>
}

// An instFormat describes the format of an instruction encoding.
// An instruction with 32-bit value x matches the format if x&mask == value
// and the predicator: canDecode(x) return true.
private partial struct instFormat {
    public uint mask;
    public uint value;
    public Op op; // args describe how to decode the instruction arguments.
// args is stored as a fixed-size array.
// if there are fewer than len(args) arguments, args[i] == 0 marks
// the end of the argument list.
    public instArgs args;
    public Func<uint, bool> canDecode;
}

private static var errShort = fmt.Errorf("truncated instruction");private static var errUnknown = fmt.Errorf("unknown instruction");

private static slice<bool> decoderCover = default;

private static void init() {
    decoderCover = make_slice<bool>(len(instFormats));
}

// Decode decodes the 4 bytes in src as a single instruction.
public static (Inst, error) Decode(slice<byte> src) {
    Inst inst = default;
    error err = default!;

    if (len(src) < 4) {
        return (new Inst(), error.As(errShort)!);
    }
    var x = binary.LittleEndian.Uint32(src);

Search:
    foreach (var (i) in instFormats) {
        var f = _addr_instFormats[i];
        if (x & f.mask != f.value) {
            continue;
        }
        if (f.canDecode != null && !f.canDecode(x)) {
            continue;
        }
        Args args = default;
        foreach (var (j, aop) in f.args) {
            if (aop == 0) {
                break;
            }
            var arg = decodeArg(aop, x);
            if (arg == null) { // Cannot decode argument
                _continueSearch = true;
                break;
            }

            args[j] = arg;

        }        decoderCover[i] = true;
        inst = new Inst(Op:f.op,Args:args,Enc:x,);
        return (inst, error.As(null!)!);

    }    return (new Inst(), error.As(errUnknown)!);

}

// decodeArg decodes the arg described by aop from the instruction bits x.
// It returns nil if x cannot be decoded according to aop.
private static Arg decodeArg(instArg aop, uint x) {

    if (aop == arg_Da)
    {
        return D0 + Reg((x >> 10) & (1 << 5 - 1));
        goto __switch_break0;
    }
    if (aop == arg_Dd)
    {
        return D0 + Reg(x & (1 << 5 - 1));
        goto __switch_break0;
    }
    if (aop == arg_Dm)
    {
        return D0 + Reg((x >> 16) & (1 << 5 - 1));
        goto __switch_break0;
    }
    if (aop == arg_Dn)
    {
        return D0 + Reg((x >> 5) & (1 << 5 - 1));
        goto __switch_break0;
    }
    if (aop == arg_Hd)
    {
        return H0 + Reg(x & (1 << 5 - 1));
        goto __switch_break0;
    }
    if (aop == arg_Hn)
    {
        return H0 + Reg((x >> 5) & (1 << 5 - 1));
        goto __switch_break0;
    }
    if (aop == arg_IAddSub)
    {
        var imm12 = (x >> 10) & (1 << 12 - 1);
        var shift = (x >> 22) & (1 << 2 - 1);
        if (shift > 1) {
            return null;
        }
        shift = shift * 12;
        return new ImmShift(uint16(imm12),uint8(shift));
        goto __switch_break0;
    }
    if (aop == arg_Sa)
    {
        return S0 + Reg((x >> 10) & (1 << 5 - 1));
        goto __switch_break0;
    }
    if (aop == arg_Sd)
    {
        return S0 + Reg(x & (1 << 5 - 1));
        goto __switch_break0;
    }
    if (aop == arg_Sm)
    {
        return S0 + Reg((x >> 16) & (1 << 5 - 1));
        goto __switch_break0;
    }
    if (aop == arg_Sn)
    {
        return S0 + Reg((x >> 5) & (1 << 5 - 1));
        goto __switch_break0;
    }
    if (aop == arg_Wa)
    {
        return W0 + Reg((x >> 10) & (1 << 5 - 1));
        goto __switch_break0;
    }
    if (aop == arg_Wd)
    {
        return W0 + Reg(x & (1 << 5 - 1));
        goto __switch_break0;
    }
    if (aop == arg_Wds)
    {
        return RegSP(W0) + RegSP(x & (1 << 5 - 1));
        goto __switch_break0;
    }
    if (aop == arg_Wm)
    {
        return W0 + Reg((x >> 16) & (1 << 5 - 1));
        goto __switch_break0;
    }
    if (aop == arg_Rm_extend__UXTB_0__UXTH_1__UXTW_2__LSL_UXTX_3__SXTB_4__SXTH_5__SXTW_6__SXTX_7__0_4)
    {
        return handle_ExtendedRegister(x, true);
        goto __switch_break0;
    }
    if (aop == arg_Wm_extend__UXTB_0__UXTH_1__LSL_UXTW_2__UXTX_3__SXTB_4__SXTH_5__SXTW_6__SXTX_7__0_4)
    {
        return handle_ExtendedRegister(x, false);
        goto __switch_break0;
    }
    if (aop == arg_Wn)
    {
        return W0 + Reg((x >> 5) & (1 << 5 - 1));
        goto __switch_break0;
    }
    if (aop == arg_Wns)
    {
        return RegSP(W0) + RegSP((x >> 5) & (1 << 5 - 1));
        goto __switch_break0;
    }
    if (aop == arg_Xa)
    {
        return X0 + Reg((x >> 10) & (1 << 5 - 1));
        goto __switch_break0;
    }
    if (aop == arg_Xd)
    {
        return X0 + Reg(x & (1 << 5 - 1));
        goto __switch_break0;
    }
    if (aop == arg_Xds)
    {
        return RegSP(X0) + RegSP(x & (1 << 5 - 1));
        goto __switch_break0;
    }
    if (aop == arg_Xm)
    {
        return X0 + Reg((x >> 16) & (1 << 5 - 1));
        goto __switch_break0;
    }
    if (aop == arg_Wm_shift__LSL_0__LSR_1__ASR_2__0_31)
    {
        return handle_ImmediateShiftedRegister(x, 31, true, false);
        goto __switch_break0;
    }
    if (aop == arg_Wm_shift__LSL_0__LSR_1__ASR_2__ROR_3__0_31)
    {
        return handle_ImmediateShiftedRegister(x, 31, true, true);
        goto __switch_break0;
    }
    if (aop == arg_Xm_shift__LSL_0__LSR_1__ASR_2__0_63)
    {
        return handle_ImmediateShiftedRegister(x, 63, false, false);
        goto __switch_break0;
    }
    if (aop == arg_Xm_shift__LSL_0__LSR_1__ASR_2__ROR_3__0_63)
    {
        return handle_ImmediateShiftedRegister(x, 63, false, true);
        goto __switch_break0;
    }
    if (aop == arg_Xn)
    {
        return X0 + Reg((x >> 5) & (1 << 5 - 1));
        goto __switch_break0;
    }
    if (aop == arg_Xns)
    {
        return RegSP(X0) + RegSP((x >> 5) & (1 << 5 - 1));
        goto __switch_break0;
    }
    if (aop == arg_slabel_imm14_2)
    {
        var imm14 = ((x >> 5) & (1 << 14 - 1));
        return PCRel(((int64(imm14) << 2) << 48) >> 48);
        goto __switch_break0;
    }
    if (aop == arg_slabel_imm19_2)
    {
        var imm19 = ((x >> 5) & (1 << 19 - 1));
        return PCRel(((int64(imm19) << 2) << 43) >> 43);
        goto __switch_break0;
    }
    if (aop == arg_slabel_imm26_2)
    {
        var imm26 = (x & (1 << 26 - 1));
        return PCRel(((int64(imm26) << 2) << 36) >> 36);
        goto __switch_break0;
    }
    if (aop == arg_slabel_immhi_immlo_0)
    {
        var immhi = ((x >> 5) & (1 << 19 - 1));
        var immlo = ((x >> 29) & (1 << 2 - 1));
        var immhilo = (immhi) << 2 | immlo;
        return PCRel((int64(immhilo) << 43) >> 43);
        goto __switch_break0;
    }
    if (aop == arg_slabel_immhi_immlo_12)
    {
        immhi = ((x >> 5) & (1 << 19 - 1));
        immlo = ((x >> 29) & (1 << 2 - 1));
        immhilo = (immhi) << 2 | immlo;
        return PCRel(((int64(immhilo) << 12) << 31) >> 31);
        goto __switch_break0;
    }
    if (aop == arg_Xns_mem)
    {
        var Rn = RegSP(X0) + RegSP(x >> 5 & (1 << 5 - 1));
        return new MemImmediate(Rn,AddrOffset,0);
        goto __switch_break0;
    }
    if (aop == arg_Xns_mem_extend_m__UXTW_2__LSL_3__SXTW_6__SXTX_7__0_0__1_1)
    {
        return handle_MemExtend(x, 1, false);
        goto __switch_break0;
    }
    if (aop == arg_Xns_mem_extend_m__UXTW_2__LSL_3__SXTW_6__SXTX_7__0_0__2_1)
    {
        return handle_MemExtend(x, 2, false);
        goto __switch_break0;
    }
    if (aop == arg_Xns_mem_extend_m__UXTW_2__LSL_3__SXTW_6__SXTX_7__0_0__3_1)
    {
        return handle_MemExtend(x, 3, false);
        goto __switch_break0;
    }
    if (aop == arg_Xns_mem_extend_m__UXTW_2__LSL_3__SXTW_6__SXTX_7__absent_0__0_1)
    {
        return handle_MemExtend(x, 1, true);
        goto __switch_break0;
    }
    if (aop == arg_Xns_mem_optional_imm12_1_unsigned)
    {
        Rn = RegSP(X0) + RegSP(x >> 5 & (1 << 5 - 1));
        imm12 = (x >> 10) & (1 << 12 - 1);
        return new MemImmediate(Rn,AddrOffset,int32(imm12));
        goto __switch_break0;
    }
    if (aop == arg_Xns_mem_optional_imm12_2_unsigned)
    {
        Rn = RegSP(X0) + RegSP(x >> 5 & (1 << 5 - 1));
        imm12 = (x >> 10) & (1 << 12 - 1);
        return new MemImmediate(Rn,AddrOffset,int32(imm12<<1));
        goto __switch_break0;
    }
    if (aop == arg_Xns_mem_optional_imm12_4_unsigned)
    {
        Rn = RegSP(X0) + RegSP(x >> 5 & (1 << 5 - 1));
        imm12 = (x >> 10) & (1 << 12 - 1);
        return new MemImmediate(Rn,AddrOffset,int32(imm12<<2));
        goto __switch_break0;
    }
    if (aop == arg_Xns_mem_optional_imm12_8_unsigned)
    {
        Rn = RegSP(X0) + RegSP(x >> 5 & (1 << 5 - 1));
        imm12 = (x >> 10) & (1 << 12 - 1);
        return new MemImmediate(Rn,AddrOffset,int32(imm12<<3));
        goto __switch_break0;
    }
    if (aop == arg_Xns_mem_optional_imm7_4_signed)
    {
        Rn = RegSP(X0) + RegSP(x >> 5 & (1 << 5 - 1));
        var imm7 = (x >> 15) & (1 << 7 - 1);
        return new MemImmediate(Rn,AddrOffset,((int32(imm7<<2))<<23)>>23);
        goto __switch_break0;
    }
    if (aop == arg_Xns_mem_optional_imm7_8_signed)
    {
        Rn = RegSP(X0) + RegSP(x >> 5 & (1 << 5 - 1));
        imm7 = (x >> 15) & (1 << 7 - 1);
        return new MemImmediate(Rn,AddrOffset,((int32(imm7<<3))<<22)>>22);
        goto __switch_break0;
    }
    if (aop == arg_Xns_mem_optional_imm9_1_signed)
    {
        Rn = RegSP(X0) + RegSP(x >> 5 & (1 << 5 - 1));
        var imm9 = (x >> 12) & (1 << 9 - 1);
        return new MemImmediate(Rn,AddrOffset,(int32(imm9)<<23)>>23);
        goto __switch_break0;
    }
    if (aop == arg_Xns_mem_post_imm7_4_signed)
    {
        Rn = RegSP(X0) + RegSP(x >> 5 & (1 << 5 - 1));
        imm7 = (x >> 15) & (1 << 7 - 1);
        return new MemImmediate(Rn,AddrPostIndex,((int32(imm7<<2))<<23)>>23);
        goto __switch_break0;
    }
    if (aop == arg_Xns_mem_post_imm7_8_signed)
    {
        Rn = RegSP(X0) + RegSP(x >> 5 & (1 << 5 - 1));
        imm7 = (x >> 15) & (1 << 7 - 1);
        return new MemImmediate(Rn,AddrPostIndex,((int32(imm7<<3))<<22)>>22);
        goto __switch_break0;
    }
    if (aop == arg_Xns_mem_post_imm9_1_signed)
    {
        Rn = RegSP(X0) + RegSP(x >> 5 & (1 << 5 - 1));
        imm9 = (x >> 12) & (1 << 9 - 1);
        return new MemImmediate(Rn,AddrPostIndex,((int32(imm9))<<23)>>23);
        goto __switch_break0;
    }
    if (aop == arg_Xns_mem_wb_imm7_4_signed)
    {
        Rn = RegSP(X0) + RegSP(x >> 5 & (1 << 5 - 1));
        imm7 = (x >> 15) & (1 << 7 - 1);
        return new MemImmediate(Rn,AddrPreIndex,((int32(imm7<<2))<<23)>>23);
        goto __switch_break0;
    }
    if (aop == arg_Xns_mem_wb_imm7_8_signed)
    {
        Rn = RegSP(X0) + RegSP(x >> 5 & (1 << 5 - 1));
        imm7 = (x >> 15) & (1 << 7 - 1);
        return new MemImmediate(Rn,AddrPreIndex,((int32(imm7<<3))<<22)>>22);
        goto __switch_break0;
    }
    if (aop == arg_Xns_mem_wb_imm9_1_signed)
    {
        Rn = RegSP(X0) + RegSP(x >> 5 & (1 << 5 - 1));
        imm9 = (x >> 12) & (1 << 9 - 1);
        return new MemImmediate(Rn,AddrPreIndex,((int32(imm9))<<23)>>23);
        goto __switch_break0;
    }
    if (aop == arg_Ws)
    {
        return W0 + Reg((x >> 16) & (1 << 5 - 1));
        goto __switch_break0;
    }
    if (aop == arg_Wt)
    {
        return W0 + Reg(x & (1 << 5 - 1));
        goto __switch_break0;
    }
    if (aop == arg_Wt2)
    {
        return W0 + Reg((x >> 10) & (1 << 5 - 1));
        goto __switch_break0;
    }
    if (aop == arg_Xs)
    {
        return X0 + Reg((x >> 16) & (1 << 5 - 1));
        goto __switch_break0;
    }
    if (aop == arg_Xt)
    {
        return X0 + Reg(x & (1 << 5 - 1));
        goto __switch_break0;
    }
    if (aop == arg_Xt2)
    {
        return X0 + Reg((x >> 10) & (1 << 5 - 1));
        goto __switch_break0;
    }
    if (aop == arg_immediate_0_127_CRm_op2)
    {
        var crm_op2 = (x >> 5) & (1 << 7 - 1);
        return Imm_hint(crm_op2);
        goto __switch_break0;
    }
    if (aop == arg_immediate_0_15_CRm)
    {
        var crm = (x >> 8) & (1 << 4 - 1);
        return new Imm(crm,false);
        goto __switch_break0;
    }
    if (aop == arg_immediate_0_15_nzcv)
    {
        var nzcv = x & (1 << 4 - 1);
        return new Imm(nzcv,false);
        goto __switch_break0;
    }
    if (aop == arg_immediate_0_31_imm5)
    {
        var imm5 = (x >> 16) & (1 << 5 - 1);
        return new Imm(imm5,false);
        goto __switch_break0;
    }
    if (aop == arg_immediate_0_31_immr)
    {
        var immr = (x >> 16) & (1 << 6 - 1);
        if (immr > 31) {
            return null;
        }
        return new Imm(immr,false);
        goto __switch_break0;
    }
    if (aop == arg_immediate_0_31_imms)
    {
        var imms = (x >> 10) & (1 << 6 - 1);
        if (imms > 31) {
            return null;
        }
        return new Imm(imms,true);
        goto __switch_break0;
    }
    if (aop == arg_immediate_0_63_b5_b40)
    {
        var b5 = (x >> 31) & 1;
        var b40 = (x >> 19) & (1 << 5 - 1);
        return new Imm((b5<<5)|b40,true);
        goto __switch_break0;
    }
    if (aop == arg_immediate_0_63_immr)
    {
        immr = (x >> 16) & (1 << 6 - 1);
        return new Imm(immr,false);
        goto __switch_break0;
    }
    if (aop == arg_immediate_0_63_imms)
    {
        imms = (x >> 10) & (1 << 6 - 1);
        return new Imm(imms,true);
        goto __switch_break0;
    }
    if (aop == arg_immediate_0_65535_imm16)
    {
        var imm16 = (x >> 5) & (1 << 16 - 1);
        return new Imm(imm16,false);
        goto __switch_break0;
    }
    if (aop == arg_immediate_0_7_op1)
    {
        var op1 = (x >> 16) & (1 << 3 - 1);
        return new Imm(op1,true);
        goto __switch_break0;
    }
    if (aop == arg_immediate_0_7_op2)
    {
        var op2 = (x >> 5) & (1 << 3 - 1);
        return new Imm(op2,true);
        goto __switch_break0;
    }
    if (aop == arg_immediate_ASR_SBFM_32M_bitfield_0_31_immr)
    {
        immr = (x >> 16) & (1 << 6 - 1);
        if (immr > 31) {
            return null;
        }
        return new Imm(immr,true);
        goto __switch_break0;
    }
    if (aop == arg_immediate_ASR_SBFM_64M_bitfield_0_63_immr)
    {
        immr = (x >> 16) & (1 << 6 - 1);
        return new Imm(immr,true);
        goto __switch_break0;
    }
    if (aop == arg_immediate_BFI_BFM_32M_bitfield_lsb_32_immr)
    {
        immr = (x >> 16) & (1 << 6 - 1);
        if (immr > 31) {
            return null;
        }
        return new Imm(32-immr,true);
        goto __switch_break0;
    }
    if (aop == arg_immediate_BFI_BFM_32M_bitfield_width_32_imms)
    {
        imms = (x >> 10) & (1 << 6 - 1);
        if (imms > 31) {
            return null;
        }
        return new Imm(imms+1,true);
        goto __switch_break0;
    }
    if (aop == arg_immediate_BFI_BFM_64M_bitfield_lsb_64_immr)
    {
        immr = (x >> 16) & (1 << 6 - 1);
        return new Imm(64-immr,true);
        goto __switch_break0;
    }
    if (aop == arg_immediate_BFI_BFM_64M_bitfield_width_64_imms)
    {
        imms = (x >> 10) & (1 << 6 - 1);
        return new Imm(imms+1,true);
        goto __switch_break0;
    }
    if (aop == arg_immediate_BFXIL_BFM_32M_bitfield_lsb_32_immr)
    {
        immr = (x >> 16) & (1 << 6 - 1);
        if (immr > 31) {
            return null;
        }
        return new Imm(immr,true);
        goto __switch_break0;
    }
    if (aop == arg_immediate_BFXIL_BFM_32M_bitfield_width_32_imms)
    {
        immr = (x >> 16) & (1 << 6 - 1);
        imms = (x >> 10) & (1 << 6 - 1);
        var width = imms - immr + 1;
        if (width < 1 || width > 32 - immr) {
            return null;
        }
        return new Imm(width,true);
        goto __switch_break0;
    }
    if (aop == arg_immediate_BFXIL_BFM_64M_bitfield_lsb_64_immr)
    {
        immr = (x >> 16) & (1 << 6 - 1);
        return new Imm(immr,true);
        goto __switch_break0;
    }
    if (aop == arg_immediate_BFXIL_BFM_64M_bitfield_width_64_imms)
    {
        immr = (x >> 16) & (1 << 6 - 1);
        imms = (x >> 10) & (1 << 6 - 1);
        width = imms - immr + 1;
        if (width < 1 || width > 64 - immr) {
            return null;
        }
        return new Imm(width,true);
        goto __switch_break0;
    }
    if (aop == arg_immediate_bitmask_32_imms_immr)
    {
        return handle_bitmasks(x, 32);
        goto __switch_break0;
    }
    if (aop == arg_immediate_bitmask_64_N_imms_immr)
    {
        return handle_bitmasks(x, 64);
        goto __switch_break0;
    }
    if (aop == arg_immediate_LSL_UBFM_32M_bitfield_0_31_immr)
    {
        imms = (x >> 10) & (1 << 6 - 1);
        shift = 31 - imms;
        if (shift > 31) {
            return null;
        }
        return new Imm(shift,true);
        goto __switch_break0;
    }
    if (aop == arg_immediate_LSL_UBFM_64M_bitfield_0_63_immr)
    {
        imms = (x >> 10) & (1 << 6 - 1);
        shift = 63 - imms;
        if (shift > 63) {
            return null;
        }
        return new Imm(shift,true);
        goto __switch_break0;
    }
    if (aop == arg_immediate_LSR_UBFM_32M_bitfield_0_31_immr)
    {
        immr = (x >> 16) & (1 << 6 - 1);
        if (immr > 31) {
            return null;
        }
        return new Imm(immr,true);
        goto __switch_break0;
    }
    if (aop == arg_immediate_LSR_UBFM_64M_bitfield_0_63_immr)
    {
        immr = (x >> 16) & (1 << 6 - 1);
        return new Imm(immr,true);
        goto __switch_break0;
    }
    if (aop == arg_immediate_optional_0_15_CRm)
    {
        crm = (x >> 8) & (1 << 4 - 1);
        return Imm_clrex(crm);
        goto __switch_break0;
    }
    if (aop == arg_immediate_optional_0_65535_imm16)
    {
        imm16 = (x >> 5) & (1 << 16 - 1);
        return Imm_dcps(imm16);
        goto __switch_break0;
    }
    if (aop == arg_immediate_OptLSL_amount_16_0_16)
    {
        imm16 = (x >> 5) & (1 << 16 - 1);
        var hw = (x >> 21) & (1 << 2 - 1);
        shift = hw * 16;
        if (shift > 16) {
            return null;
        }
        return new ImmShift(uint16(imm16),uint8(shift));
        goto __switch_break0;
    }
    if (aop == arg_immediate_OptLSL_amount_16_0_48)
    {
        imm16 = (x >> 5) & (1 << 16 - 1);
        hw = (x >> 21) & (1 << 2 - 1);
        shift = hw * 16;
        return new ImmShift(uint16(imm16),uint8(shift));
        goto __switch_break0;
    }
    if (aop == arg_immediate_SBFIZ_SBFM_32M_bitfield_lsb_32_immr)
    {
        immr = (x >> 16) & (1 << 6 - 1);
        if (immr > 31) {
            return null;
        }
        return new Imm(32-immr,true);
        goto __switch_break0;
    }
    if (aop == arg_immediate_SBFIZ_SBFM_32M_bitfield_width_32_imms)
    {
        imms = (x >> 10) & (1 << 6 - 1);
        if (imms > 31) {
            return null;
        }
        return new Imm(imms+1,true);
        goto __switch_break0;
    }
    if (aop == arg_immediate_SBFIZ_SBFM_64M_bitfield_lsb_64_immr)
    {
        immr = (x >> 16) & (1 << 6 - 1);
        return new Imm(64-immr,true);
        goto __switch_break0;
    }
    if (aop == arg_immediate_SBFIZ_SBFM_64M_bitfield_width_64_imms)
    {
        imms = (x >> 10) & (1 << 6 - 1);
        return new Imm(imms+1,true);
        goto __switch_break0;
    }
    if (aop == arg_immediate_SBFX_SBFM_32M_bitfield_lsb_32_immr)
    {
        immr = (x >> 16) & (1 << 6 - 1);
        if (immr > 31) {
            return null;
        }
        return new Imm(immr,true);
        goto __switch_break0;
    }
    if (aop == arg_immediate_SBFX_SBFM_32M_bitfield_width_32_imms)
    {
        immr = (x >> 16) & (1 << 6 - 1);
        imms = (x >> 10) & (1 << 6 - 1);
        width = imms - immr + 1;
        if (width < 1 || width > 32 - immr) {
            return null;
        }
        return new Imm(width,true);
        goto __switch_break0;
    }
    if (aop == arg_immediate_SBFX_SBFM_64M_bitfield_lsb_64_immr)
    {
        immr = (x >> 16) & (1 << 6 - 1);
        return new Imm(immr,true);
        goto __switch_break0;
    }
    if (aop == arg_immediate_SBFX_SBFM_64M_bitfield_width_64_imms)
    {
        immr = (x >> 16) & (1 << 6 - 1);
        imms = (x >> 10) & (1 << 6 - 1);
        width = imms - immr + 1;
        if (width < 1 || width > 64 - immr) {
            return null;
        }
        return new Imm(width,true);
        goto __switch_break0;
    }
    if (aop == arg_immediate_shift_32_implicit_imm16_hw)
    {
        imm16 = (x >> 5) & (1 << 16 - 1);
        hw = (x >> 21) & (1 << 2 - 1);
        shift = hw * 16;
        if (shift > 16) {
            return null;
        }
        var result = uint32(imm16) << (int)(shift);
        return new Imm(result,false);
        goto __switch_break0;
    }
    if (aop == arg_immediate_shift_32_implicit_inverse_imm16_hw)
    {
        imm16 = (x >> 5) & (1 << 16 - 1);
        hw = (x >> 21) & (1 << 2 - 1);
        shift = hw * 16;
        if (shift > 16) {
            return null;
        }
        result = uint32(imm16) << (int)(shift);
        return new Imm(^result,false);
        goto __switch_break0;
    }
    if (aop == arg_immediate_shift_64_implicit_imm16_hw)
    {
        imm16 = (x >> 5) & (1 << 16 - 1);
        hw = (x >> 21) & (1 << 2 - 1);
        shift = hw * 16;
        result = uint64(imm16) << (int)(shift);
        return new Imm64(result,false);
        goto __switch_break0;
    }
    if (aop == arg_immediate_shift_64_implicit_inverse_imm16_hw)
    {
        imm16 = (x >> 5) & (1 << 16 - 1);
        hw = (x >> 21) & (1 << 2 - 1);
        shift = hw * 16;
        result = uint64(imm16) << (int)(shift);
        return new Imm64(^result,false);
        goto __switch_break0;
    }
    if (aop == arg_immediate_UBFIZ_UBFM_32M_bitfield_lsb_32_immr)
    {
        immr = (x >> 16) & (1 << 6 - 1);
        if (immr > 31) {
            return null;
        }
        return new Imm(32-immr,true);
        goto __switch_break0;
    }
    if (aop == arg_immediate_UBFIZ_UBFM_32M_bitfield_width_32_imms)
    {
        imms = (x >> 10) & (1 << 6 - 1);
        if (imms > 31) {
            return null;
        }
        return new Imm(imms+1,true);
        goto __switch_break0;
    }
    if (aop == arg_immediate_UBFIZ_UBFM_64M_bitfield_lsb_64_immr)
    {
        immr = (x >> 16) & (1 << 6 - 1);
        return new Imm(64-immr,true);
        goto __switch_break0;
    }
    if (aop == arg_immediate_UBFIZ_UBFM_64M_bitfield_width_64_imms)
    {
        imms = (x >> 10) & (1 << 6 - 1);
        return new Imm(imms+1,true);
        goto __switch_break0;
    }
    if (aop == arg_immediate_UBFX_UBFM_32M_bitfield_lsb_32_immr)
    {
        immr = (x >> 16) & (1 << 6 - 1);
        if (immr > 31) {
            return null;
        }
        return new Imm(immr,true);
        goto __switch_break0;
    }
    if (aop == arg_immediate_UBFX_UBFM_32M_bitfield_width_32_imms)
    {
        immr = (x >> 16) & (1 << 6 - 1);
        imms = (x >> 10) & (1 << 6 - 1);
        width = imms - immr + 1;
        if (width < 1 || width > 32 - immr) {
            return null;
        }
        return new Imm(width,true);
        goto __switch_break0;
    }
    if (aop == arg_immediate_UBFX_UBFM_64M_bitfield_lsb_64_immr)
    {
        immr = (x >> 16) & (1 << 6 - 1);
        return new Imm(immr,true);
        goto __switch_break0;
    }
    if (aop == arg_immediate_UBFX_UBFM_64M_bitfield_width_64_imms)
    {
        immr = (x >> 16) & (1 << 6 - 1);
        imms = (x >> 10) & (1 << 6 - 1);
        width = imms - immr + 1;
        if (width < 1 || width > 64 - immr) {
            return null;
        }
        return new Imm(width,true);
        goto __switch_break0;
    }
    if (aop == arg_Rt_31_1__W_0__X_1)
    {
        b5 = (x >> 31) & 1;
        var Rt = x & (1 << 5 - 1);
        if (b5 == 0) {
            return W0 + Reg(Rt);
        }
        else
 {
            return X0 + Reg(Rt);
        }
        goto __switch_break0;
    }
    if (aop == arg_cond_AllowALNV_Normal)
    {
        var cond = (x >> 12) & (1 << 4 - 1);
        return new Cond(uint8(cond),false);
        goto __switch_break0;
    }
    if (aop == arg_conditional)
    {
        cond = x & (1 << 4 - 1);
        return new Cond(uint8(cond),false);
        goto __switch_break0;
    }
    if (aop == arg_cond_NotAllowALNV_Invert)
    {
        cond = (x >> 12) & (1 << 4 - 1);
        if ((cond >> 1) == 7) {
            return null;
        }
        return new Cond(uint8(cond),true);
        goto __switch_break0;
    }
    if (aop == arg_Cm)
    {
        var CRm = (x >> 8) & (1 << 4 - 1);
        return Imm_c(CRm);
        goto __switch_break0;
    }
    if (aop == arg_Cn)
    {
        var CRn = (x >> 12) & (1 << 4 - 1);
        return Imm_c(CRn);
        goto __switch_break0;
    }
    if (aop == arg_option_DMB_BO_system_CRm)
    {
        CRm = (x >> 8) & (1 << 4 - 1);
        return Imm_option(CRm);
        goto __switch_break0;
    }
    if (aop == arg_option_DSB_BO_system_CRm)
    {
        CRm = (x >> 8) & (1 << 4 - 1);
        return Imm_option(CRm);
        goto __switch_break0;
    }
    if (aop == arg_option_ISB_BI_system_CRm)
    {
        CRm = (x >> 8) & (1 << 4 - 1);
        if (CRm == 15) {
            return Imm_option(CRm);
        }
        return new Imm(CRm,false);
        goto __switch_break0;
    }
    if (aop == arg_prfop_Rt)
    {
        Rt = x & (1 << 5 - 1);
        return Imm_prfop(Rt);
        goto __switch_break0;
    }
    if (aop == arg_pstatefield_op1_op2__SPSel_05__DAIFSet_36__DAIFClr_37)
    {
        op1 = (x >> 16) & (1 << 3 - 1);
        op2 = (x >> 5) & (1 << 3 - 1);
        if ((op1 == 0) && (op2 == 5)) {
            return SPSel;
        }
        else if ((op1 == 3) && (op2 == 6)) {
            return DAIFSet;
        }
        else if ((op1 == 3) && (op2 == 7)) {
            return DAIFClr;
        }
        return null;
        goto __switch_break0;
    }
    if (aop == arg_sysreg_o0_op1_CRn_CRm_op2)
    {
        var op0 = (x >> 19) & (1 << 2 - 1);
        op1 = (x >> 16) & (1 << 3 - 1);
        CRn = (x >> 12) & (1 << 4 - 1);
        CRm = (x >> 8) & (1 << 4 - 1);
        op2 = (x >> 5) & (1 << 3 - 1);
        return new Systemreg(uint8(op0),uint8(op1),uint8(CRn),uint8(CRm),uint8(op2));
        goto __switch_break0;
    }
    if (aop == arg_sysop_AT_SYS_CR_system) 
    {
        //TODO: system instruction
        return null;
        goto __switch_break0;
    }
    if (aop == arg_sysop_DC_SYS_CR_system) 
    {
        //TODO: system instruction
        return null;
        goto __switch_break0;
    }
    if (aop == arg_sysop_SYS_CR_system) 
    {
        //TODO: system instruction
        return null;
        goto __switch_break0;
    }
    if (aop == arg_sysop_TLBI_SYS_CR_system) 
    {
        //TODO: system instruction
        return null;
        goto __switch_break0;
    }
    if (aop == arg_Bt)
    {
        return B0 + Reg(x & (1 << 5 - 1));
        goto __switch_break0;
    }
    if (aop == arg_Dt)
    {
        return D0 + Reg(x & (1 << 5 - 1));
        goto __switch_break0;
    }
    if (aop == arg_Dt2)
    {
        return D0 + Reg((x >> 10) & (1 << 5 - 1));
        goto __switch_break0;
    }
    if (aop == arg_Ht)
    {
        return H0 + Reg(x & (1 << 5 - 1));
        goto __switch_break0;
    }
    if (aop == arg_immediate_0_63_immh_immb__UIntimmhimmb64_8)
    {
        var immh = (x >> 19) & (1 << 4 - 1);
        if ((immh & 8) == 0) {
            return null;
        }
        var immb = (x >> 16) & (1 << 3 - 1);
        return new Imm((immh<<3)+immb-64,true);
        goto __switch_break0;
    }
    if (aop == arg_immediate_0_width_immh_immb__SEEAdvancedSIMDmodifiedimmediate_0__UIntimmhimmb8_1__UIntimmhimmb16_2__UIntimmhimmb32_4)
    {
        immh = (x >> 19) & (1 << 4 - 1);
        immb = (x >> 16) & (1 << 3 - 1);
        if (immh == 1) {
            return new Imm((immh<<3)+immb-8,true);
        }
        else if ((immh >> 1) == 1) {
            return new Imm((immh<<3)+immb-16,true);
        }
        else if ((immh >> 2) == 1) {
            return new Imm((immh<<3)+immb-32,true);
        }
        else
 {
            return null;
        }
        goto __switch_break0;
    }
    if (aop == arg_immediate_0_width_immh_immb__SEEAdvancedSIMDmodifiedimmediate_0__UIntimmhimmb8_1__UIntimmhimmb16_2__UIntimmhimmb32_4__UIntimmhimmb64_8)
    {
        fallthrough = true;

    }
    if (fallthrough || aop == arg_immediate_0_width_m1_immh_immb__UIntimmhimmb8_1__UIntimmhimmb16_2__UIntimmhimmb32_4__UIntimmhimmb64_8)
    {
        immh = (x >> 19) & (1 << 4 - 1);
        immb = (x >> 16) & (1 << 3 - 1);
        if (immh == 1) {
            return new Imm((immh<<3)+immb-8,true);
        }
        else if ((immh >> 1) == 1) {
            return new Imm((immh<<3)+immb-16,true);
        }
        else if ((immh >> 2) == 1) {
            return new Imm((immh<<3)+immb-32,true);
        }
        else if ((immh >> 3) == 1) {
            return new Imm((immh<<3)+immb-64,true);
        }
        else
 {
            return null;
        }
        goto __switch_break0;
    }
    if (aop == arg_immediate_0_width_size__8_0__16_1__32_2)
    {
        var size = (x >> 22) & (1 << 2 - 1);
        switch (size) {
            case 0: 
                return new Imm(8,true);
                break;
            case 1: 
                return new Imm(16,true);
                break;
            case 2: 
                return new Imm(32,true);
                break;
            default: 
                return null;
                break;
        }
        goto __switch_break0;
    }
    if (aop == arg_immediate_1_64_immh_immb__128UIntimmhimmb_8)
    {
        immh = (x >> 19) & (1 << 4 - 1);
        if ((immh & 8) == 0) {
            return null;
        }
        immb = (x >> 16) & (1 << 3 - 1);
        return new Imm(128-((immh<<3)+immb),true);
        goto __switch_break0;
    }
    if (aop == arg_immediate_1_width_immh_immb__16UIntimmhimmb_1__32UIntimmhimmb_2__64UIntimmhimmb_4)
    {
        fallthrough = true;

    }
    if (fallthrough || aop == arg_immediate_1_width_immh_immb__SEEAdvancedSIMDmodifiedimmediate_0__16UIntimmhimmb_1__32UIntimmhimmb_2__64UIntimmhimmb_4)
    {
        immh = (x >> 19) & (1 << 4 - 1);
        immb = (x >> 16) & (1 << 3 - 1);
        if (immh == 1) {
            return new Imm(16-((immh<<3)+immb),true);
        }
        else if ((immh >> 1) == 1) {
            return new Imm(32-((immh<<3)+immb),true);
        }
        else if ((immh >> 2) == 1) {
            return new Imm(64-((immh<<3)+immb),true);
        }
        else
 {
            return null;
        }
        goto __switch_break0;
    }
    if (aop == arg_immediate_1_width_immh_immb__SEEAdvancedSIMDmodifiedimmediate_0__16UIntimmhimmb_1__32UIntimmhimmb_2__64UIntimmhimmb_4__128UIntimmhimmb_8)
    {
        immh = (x >> 19) & (1 << 4 - 1);
        immb = (x >> 16) & (1 << 3 - 1);
        if (immh == 1) {
            return new Imm(16-((immh<<3)+immb),true);
        }
        else if ((immh >> 1) == 1) {
            return new Imm(32-((immh<<3)+immb),true);
        }
        else if ((immh >> 2) == 1) {
            return new Imm(64-((immh<<3)+immb),true);
        }
        else if ((immh >> 3) == 1) {
            return new Imm(128-((immh<<3)+immb),true);
        }
        else
 {
            return null;
        }
        goto __switch_break0;
    }
    if (aop == arg_immediate_8x8_a_b_c_d_e_f_g_h)
    {
        ulong imm = default;
        if (x & (1 << 5) != 0) {
            imm = (1 << 8) - 1;
        }
        else
 {
            imm = 0;
        }
        if (x & (1 << 6) != 0) {
            imm += ((1 << 8) - 1) << 8;
        }
        if (x & (1 << 7) != 0) {
            imm += ((1 << 8) - 1) << 16;
        }
        if (x & (1 << 8) != 0) {
            imm += ((1 << 8) - 1) << 24;
        }
        if (x & (1 << 9) != 0) {
            imm += ((1 << 8) - 1) << 32;
        }
        if (x & (1 << 16) != 0) {
            imm += ((1 << 8) - 1) << 40;
        }
        if (x & (1 << 17) != 0) {
            imm += ((1 << 8) - 1) << 48;
        }
        if (x & (1 << 18) != 0) {
            imm += ((1 << 8) - 1) << 56;
        }
        return new Imm64(imm,false);
        goto __switch_break0;
    }
    if (aop == arg_immediate_exp_3_pre_4_a_b_c_d_e_f_g_h)
    {
        var pre = (x >> 5) & (1 << 4 - 1);
        nint exp = 1 - ((x >> 17) & 1);
        exp = (exp << 2) + (((x >> 16) & 1) << 1) + ((x >> 9) & 1);
        var s = ((x >> 18) & 1);
        return new Imm_fp(uint8(s),int8(exp)-3,uint8(pre));
        goto __switch_break0;
    }
    if (aop == arg_immediate_exp_3_pre_4_imm8)
    {
        pre = (x >> 13) & (1 << 4 - 1);
        exp = 1 - ((x >> 19) & 1);
        exp = (exp << 2) + ((x >> 17) & (1 << 2 - 1));
        s = ((x >> 20) & 1);
        return new Imm_fp(uint8(s),int8(exp)-3,uint8(pre));
        goto __switch_break0;
    }
    if (aop == arg_immediate_fbits_min_1_max_0_sub_0_immh_immb__64UIntimmhimmb_4__128UIntimmhimmb_8)
    {
        fallthrough = true;

    }
    if (fallthrough || aop == arg_immediate_fbits_min_1_max_0_sub_0_immh_immb__SEEAdvancedSIMDmodifiedimmediate_0__64UIntimmhimmb_4__128UIntimmhimmb_8)
    {
        immh = (x >> 19) & (1 << 4 - 1);
        immb = (x >> 16) & (1 << 3 - 1);
        if ((immh >> 2) == 1) {
            return new Imm(64-((immh<<3)+immb),true);
        }
        else if ((immh >> 3) == 1) {
            return new Imm(128-((immh<<3)+immb),true);
        }
        else
 {
            return null;
        }
        goto __switch_break0;
    }
    if (aop == arg_immediate_fbits_min_1_max_32_sub_64_scale)
    {
        var scale = (x >> 10) & (1 << 6 - 1);
        nint fbits = 64 - scale;
        if (fbits > 32) {
            return null;
        }
        return new Imm(fbits,true);
        goto __switch_break0;
    }
    if (aop == arg_immediate_fbits_min_1_max_64_sub_64_scale)
    {
        scale = (x >> 10) & (1 << 6 - 1);
        fbits = 64 - scale;
        return new Imm(fbits,true);
        goto __switch_break0;
    }
    if (aop == arg_immediate_floatzero)
    {
        return new Imm(0,true);
        goto __switch_break0;
    }
    if (aop == arg_immediate_index_Q_imm4__imm4lt20gt_00__imm4_10)
    {
        var Q = (x >> 30) & 1;
        var imm4 = (x >> 11) & (1 << 4 - 1);
        if (Q == 1 || (imm4 >> 3) == 0) {
            return new Imm(imm4,true);
        }
        else
 {
            return null;
        }
        goto __switch_break0;
    }
    if (aop == arg_immediate_MSL__a_b_c_d_e_f_g_h_cmode__8_0__16_1)
    {
        shift = default;
        var imm8 = (x >> 16) & (1 << 3 - 1);
        imm8 = (imm8 << 5) | ((x >> 5) & (1 << 5 - 1));
        if ((x >> 12) & 1 == 0) {
            shift = 8 + 128;
        }
        else
 {
            shift = 16 + 128;
        }
        return new ImmShift(uint16(imm8),shift);
        goto __switch_break0;
    }
    if (aop == arg_immediate_OptLSL__a_b_c_d_e_f_g_h_cmode__0_0__8_1)
    {
        imm8 = (x >> 16) & (1 << 3 - 1);
        imm8 = (imm8 << 5) | ((x >> 5) & (1 << 5 - 1));
        var cmode1 = (x >> 13) & 1;
        shift = 8 * cmode1;
        return new ImmShift(uint16(imm8),uint8(shift));
        goto __switch_break0;
    }
    if (aop == arg_immediate_OptLSL__a_b_c_d_e_f_g_h_cmode__0_0__8_1__16_2__24_3)
    {
        imm8 = (x >> 16) & (1 << 3 - 1);
        imm8 = (imm8 << 5) | ((x >> 5) & (1 << 5 - 1));
        cmode1 = (x >> 13) & (1 << 2 - 1);
        shift = 8 * cmode1;
        return new ImmShift(uint16(imm8),uint8(shift));
        goto __switch_break0;
    }
    if (aop == arg_immediate_OptLSLZero__a_b_c_d_e_f_g_h)
    {
        imm8 = (x >> 16) & (1 << 3 - 1);
        imm8 = (imm8 << 5) | ((x >> 5) & (1 << 5 - 1));
        return new ImmShift(uint16(imm8),0);
        goto __switch_break0;
    }
    if (aop == arg_immediate_zero)
    {
        return new Imm(0,true);
        goto __switch_break0;
    }
    if (aop == arg_Qd)
    {
        return Q0 + Reg(x & (1 << 5 - 1));
        goto __switch_break0;
    }
    if (aop == arg_Qn)
    {
        return Q0 + Reg((x >> 5) & (1 << 5 - 1));
        goto __switch_break0;
    }
    if (aop == arg_Qt)
    {
        return Q0 + Reg(x & (1 << 5 - 1));
        goto __switch_break0;
    }
    if (aop == arg_Qt2)
    {
        return Q0 + Reg((x >> 10) & (1 << 5 - 1));
        goto __switch_break0;
    }
    if (aop == arg_Rn_16_5__W_1__W_2__W_4__X_8)
    {
        imm5 = (x >> 16) & (1 << 5 - 1);
        if (((imm5 & 1) == 1) || ((imm5 & 2) == 2) || ((imm5 & 4) == 4)) {
            return W0 + Reg((x >> 5) & (1 << 5 - 1));
        }
        else if ((imm5 & 8) == 8) {
            return X0 + Reg((x >> 5) & (1 << 5 - 1));
        }
        else
 {
            return null;
        }
        goto __switch_break0;
    }
    if (aop == arg_St)
    {
        return S0 + Reg(x & (1 << 5 - 1));
        goto __switch_break0;
    }
    if (aop == arg_St2)
    {
        return S0 + Reg((x >> 10) & (1 << 5 - 1));
        goto __switch_break0;
    }
    if (aop == arg_Vd_16_5__B_1__H_2__S_4__D_8)
    {
        imm5 = (x >> 16) & (1 << 5 - 1);
        var Rd = x & (1 << 5 - 1);
        if (imm5 & 1 == 1) {
            return B0 + Reg(Rd);
        }
        else if (imm5 & 2 == 2) {
            return H0 + Reg(Rd);
        }
        else if (imm5 & 4 == 4) {
            return S0 + Reg(Rd);
        }
        else if (imm5 & 8 == 8) {
            return D0 + Reg(Rd);
        }
        else
 {
            return null;
        }
        goto __switch_break0;
    }
    if (aop == arg_Vd_19_4__B_1__H_2__S_4)
    {
        immh = (x >> 19) & (1 << 4 - 1);
        Rd = x & (1 << 5 - 1);
        if (immh == 1) {
            return B0 + Reg(Rd);
        }
        else if (immh >> 1 == 1) {
            return H0 + Reg(Rd);
        }
        else if (immh >> 2 == 1) {
            return S0 + Reg(Rd);
        }
        else
 {
            return null;
        }
        goto __switch_break0;
    }
    if (aop == arg_Vd_19_4__B_1__H_2__S_4__D_8)
    {
        immh = (x >> 19) & (1 << 4 - 1);
        Rd = x & (1 << 5 - 1);
        if (immh == 1) {
            return B0 + Reg(Rd);
        }
        else if (immh >> 1 == 1) {
            return H0 + Reg(Rd);
        }
        else if (immh >> 2 == 1) {
            return S0 + Reg(Rd);
        }
        else if (immh >> 3 == 1) {
            return D0 + Reg(Rd);
        }
        else
 {
            return null;
        }
        goto __switch_break0;
    }
    if (aop == arg_Vd_19_4__D_8)
    {
        immh = (x >> 19) & (1 << 4 - 1);
        Rd = x & (1 << 5 - 1);
        if (immh >> 3 == 1) {
            return D0 + Reg(Rd);
        }
        else
 {
            return null;
        }
        goto __switch_break0;
    }
    if (aop == arg_Vd_19_4__S_4__D_8)
    {
        immh = (x >> 19) & (1 << 4 - 1);
        Rd = x & (1 << 5 - 1);
        if (immh >> 2 == 1) {
            return S0 + Reg(Rd);
        }
        else if (immh >> 3 == 1) {
            return D0 + Reg(Rd);
        }
        else
 {
            return null;
        }
        goto __switch_break0;
    }
    if (aop == arg_Vd_22_1__S_0)
    {
        var sz = (x >> 22) & 1;
        Rd = x & (1 << 5 - 1);
        if (sz == 0) {
            return S0 + Reg(Rd);
        }
        else
 {
            return null;
        }
        goto __switch_break0;
    }
    if (aop == arg_Vd_22_1__S_0__D_1)
    {
        sz = (x >> 22) & 1;
        Rd = x & (1 << 5 - 1);
        if (sz == 0) {
            return S0 + Reg(Rd);
        }
        else
 {
            return D0 + Reg(Rd);
        }
        goto __switch_break0;
    }
    if (aop == arg_Vd_22_1__S_1)
    {
        sz = (x >> 22) & 1;
        Rd = x & (1 << 5 - 1);
        if (sz == 1) {
            return S0 + Reg(Rd);
        }
        else
 {
            return null;
        }
        goto __switch_break0;
    }
    if (aop == arg_Vd_22_2__B_0__H_1__S_2)
    {
        size = (x >> 22) & (1 << 2 - 1);
        Rd = x & (1 << 5 - 1);
        if (size == 0) {
            return B0 + Reg(Rd);
        }
        else if (size == 1) {
            return H0 + Reg(Rd);
        }
        else if (size == 2) {
            return S0 + Reg(Rd);
        }
        else
 {
            return null;
        }
        goto __switch_break0;
    }
    if (aop == arg_Vd_22_2__B_0__H_1__S_2__D_3)
    {
        size = (x >> 22) & (1 << 2 - 1);
        Rd = x & (1 << 5 - 1);
        if (size == 0) {
            return B0 + Reg(Rd);
        }
        else if (size == 1) {
            return H0 + Reg(Rd);
        }
        else if (size == 2) {
            return S0 + Reg(Rd);
        }
        else
 {
            return D0 + Reg(Rd);
        }
        goto __switch_break0;
    }
    if (aop == arg_Vd_22_2__D_3)
    {
        size = (x >> 22) & (1 << 2 - 1);
        Rd = x & (1 << 5 - 1);
        if (size == 3) {
            return D0 + Reg(Rd);
        }
        else
 {
            return null;
        }
        goto __switch_break0;
    }
    if (aop == arg_Vd_22_2__H_0__S_1__D_2)
    {
        size = (x >> 22) & (1 << 2 - 1);
        Rd = x & (1 << 5 - 1);
        if (size == 0) {
            return H0 + Reg(Rd);
        }
        else if (size == 1) {
            return S0 + Reg(Rd);
        }
        else if (size == 2) {
            return D0 + Reg(Rd);
        }
        else
 {
            return null;
        }
        goto __switch_break0;
    }
    if (aop == arg_Vd_22_2__H_1__S_2)
    {
        size = (x >> 22) & (1 << 2 - 1);
        Rd = x & (1 << 5 - 1);
        if (size == 1) {
            return H0 + Reg(Rd);
        }
        else if (size == 2) {
            return S0 + Reg(Rd);
        }
        else
 {
            return null;
        }
        goto __switch_break0;
    }
    if (aop == arg_Vd_22_2__S_1__D_2)
    {
        size = (x >> 22) & (1 << 2 - 1);
        Rd = x & (1 << 5 - 1);
        if (size == 1) {
            return S0 + Reg(Rd);
        }
        else if (size == 2) {
            return D0 + Reg(Rd);
        }
        else
 {
            return null;
        }
        goto __switch_break0;
    }
    if (aop == arg_Vd_arrangement_16B)
    {
        Rd = x & (1 << 5 - 1);
        return new RegisterWithArrangement(V0+Reg(Rd),Arrangement16B,0);
        goto __switch_break0;
    }
    if (aop == arg_Vd_arrangement_2D)
    {
        Rd = x & (1 << 5 - 1);
        return new RegisterWithArrangement(V0+Reg(Rd),Arrangement2D,0);
        goto __switch_break0;
    }
    if (aop == arg_Vd_arrangement_4S)
    {
        Rd = x & (1 << 5 - 1);
        return new RegisterWithArrangement(V0+Reg(Rd),Arrangement4S,0);
        goto __switch_break0;
    }
    if (aop == arg_Vd_arrangement_D_index__1)
    {
        Rd = x & (1 << 5 - 1);
        return new RegisterWithArrangementAndIndex(V0+Reg(Rd),ArrangementD,1,0);
        goto __switch_break0;
    }
    if (aop == arg_Vd_arrangement_imm5___B_1__H_2__S_4__D_8_index__imm5__imm5lt41gt_1__imm5lt42gt_2__imm5lt43gt_4__imm5lt4gt_8_1)
    {
        Arrangement a = default;
        uint index = default;
        Rd = x & (1 << 5 - 1);
        imm5 = (x >> 16) & (1 << 5 - 1);
        if (imm5 & 1 == 1) {
            a = ArrangementB;
            index = imm5 >> 1;
        }
        else if (imm5 & 2 == 2) {
            a = ArrangementH;
            index = imm5 >> 2;
        }
        else if (imm5 & 4 == 4) {
            a = ArrangementS;
            index = imm5 >> 3;
        }
        else if (imm5 & 8 == 8) {
            a = ArrangementD;
            index = imm5 >> 4;
        }
        else
 {
            return null;
        }
        return new RegisterWithArrangementAndIndex(V0+Reg(Rd),a,uint8(index),0);
        goto __switch_break0;
    }
    if (aop == arg_Vd_arrangement_imm5_Q___8B_10__16B_11__4H_20__8H_21__2S_40__4S_41__2D_81)
    {
        Rd = x & (1 << 5 - 1);
        imm5 = (x >> 16) & (1 << 5 - 1);
        Q = (x >> 30) & 1;
        if (imm5 & 1 == 1) {
            if (Q == 0) {
                return new RegisterWithArrangement(V0+Reg(Rd),Arrangement8B,0);
            }
            else
 {
                return new RegisterWithArrangement(V0+Reg(Rd),Arrangement16B,0);
            }

        }
        else if (imm5 & 2 == 2) {
            if (Q == 0) {
                return new RegisterWithArrangement(V0+Reg(Rd),Arrangement4H,0);
            }
            else
 {
                return new RegisterWithArrangement(V0+Reg(Rd),Arrangement8H,0);
            }

        }
        else if (imm5 & 4 == 4) {
            if (Q == 0) {
                return new RegisterWithArrangement(V0+Reg(Rd),Arrangement2S,0);
            }
            else
 {
                return new RegisterWithArrangement(V0+Reg(Rd),Arrangement4S,0);
            }

        }
        else if ((imm5 & 8 == 8) && (Q == 1)) {
            return new RegisterWithArrangement(V0+Reg(Rd),Arrangement2D,0);
        }
        else
 {
            return null;
        }
        goto __switch_break0;
    }
    if (aop == arg_Vd_arrangement_immh_Q___SEEAdvancedSIMDmodifiedimmediate_00__2S_40__4S_41__2D_81)
    {
        Rd = x & (1 << 5 - 1);
        immh = (x >> 19) & (1 << 4 - 1);
        Q = (x >> 30) & 1;
        if (immh >> 2 == 1) {
            if (Q == 0) {
                return new RegisterWithArrangement(V0+Reg(Rd),Arrangement2S,0);
            }
            else
 {
                return new RegisterWithArrangement(V0+Reg(Rd),Arrangement4S,0);
            }

        }
        else if (immh >> 3 == 1) {
            if (Q == 1) {
                return new RegisterWithArrangement(V0+Reg(Rd),Arrangement2D,0);
            }
        }
        return null;
        goto __switch_break0;
    }
    if (aop == arg_Vd_arrangement_immh_Q___SEEAdvancedSIMDmodifiedimmediate_00__8B_10__16B_11__4H_20__8H_21__2S_40__4S_41)
    {
        Rd = x & (1 << 5 - 1);
        immh = (x >> 19) & (1 << 4 - 1);
        Q = (x >> 30) & 1;
        if (immh == 1) {
            if (Q == 0) {
                return new RegisterWithArrangement(V0+Reg(Rd),Arrangement8B,0);
            }
            else
 {
                return new RegisterWithArrangement(V0+Reg(Rd),Arrangement16B,0);
            }

        }
        else if (immh >> 1 == 1) {
            if (Q == 0) {
                return new RegisterWithArrangement(V0+Reg(Rd),Arrangement4H,0);
            }
            else
 {
                return new RegisterWithArrangement(V0+Reg(Rd),Arrangement8H,0);
            }

        }
        else if (immh >> 2 == 1) {
            if (Q == 0) {
                return new RegisterWithArrangement(V0+Reg(Rd),Arrangement2S,0);
            }
            else
 {
                return new RegisterWithArrangement(V0+Reg(Rd),Arrangement4S,0);
            }

        }
        return null;
        goto __switch_break0;
    }
    if (aop == arg_Vd_arrangement_immh_Q___SEEAdvancedSIMDmodifiedimmediate_00__8B_10__16B_11__4H_20__8H_21__2S_40__4S_41__2D_81)
    {
        Rd = x & (1 << 5 - 1);
        immh = (x >> 19) & (1 << 4 - 1);
        Q = (x >> 30) & 1;
        if (immh == 1) {
            if (Q == 0) {
                return new RegisterWithArrangement(V0+Reg(Rd),Arrangement8B,0);
            }
            else
 {
                return new RegisterWithArrangement(V0+Reg(Rd),Arrangement16B,0);
            }

        }
        else if (immh >> 1 == 1) {
            if (Q == 0) {
                return new RegisterWithArrangement(V0+Reg(Rd),Arrangement4H,0);
            }
            else
 {
                return new RegisterWithArrangement(V0+Reg(Rd),Arrangement8H,0);
            }

        }
        else if (immh >> 2 == 1) {
            if (Q == 0) {
                return new RegisterWithArrangement(V0+Reg(Rd),Arrangement2S,0);
            }
            else
 {
                return new RegisterWithArrangement(V0+Reg(Rd),Arrangement4S,0);
            }

        }
        else if (immh >> 3 == 1) {
            if (Q == 1) {
                return new RegisterWithArrangement(V0+Reg(Rd),Arrangement2D,0);
            }
        }
        return null;
        goto __switch_break0;
    }
    if (aop == arg_Vd_arrangement_immh___SEEAdvancedSIMDmodifiedimmediate_0__8H_1__4S_2__2D_4)
    {
        Rd = x & (1 << 5 - 1);
        immh = (x >> 19) & (1 << 4 - 1);
        if (immh == 1) {
            return new RegisterWithArrangement(V0+Reg(Rd),Arrangement8H,0);
        }
        else if (immh >> 1 == 1) {
            return new RegisterWithArrangement(V0+Reg(Rd),Arrangement4S,0);
        }
        else if (immh >> 2 == 1) {
            return new RegisterWithArrangement(V0+Reg(Rd),Arrangement2D,0);
        }
        return null;
        goto __switch_break0;
    }
    if (aop == arg_Vd_arrangement_Q___2S_0__4S_1)
    {
        Rd = x & (1 << 5 - 1);
        Q = (x >> 30) & 1;
        if (Q == 0) {
            return new RegisterWithArrangement(V0+Reg(Rd),Arrangement2S,0);
        }
        else
 {
            return new RegisterWithArrangement(V0+Reg(Rd),Arrangement4S,0);
        }
        goto __switch_break0;
    }
    if (aop == arg_Vd_arrangement_Q___4H_0__8H_1)
    {
        Rd = x & (1 << 5 - 1);
        Q = (x >> 30) & 1;
        if (Q == 0) {
            return new RegisterWithArrangement(V0+Reg(Rd),Arrangement4H,0);
        }
        else
 {
            return new RegisterWithArrangement(V0+Reg(Rd),Arrangement8H,0);
        }
        goto __switch_break0;
    }
    if (aop == arg_Vd_arrangement_Q___8B_0__16B_1)
    {
        Rd = x & (1 << 5 - 1);
        Q = (x >> 30) & 1;
        if (Q == 0) {
            return new RegisterWithArrangement(V0+Reg(Rd),Arrangement8B,0);
        }
        else
 {
            return new RegisterWithArrangement(V0+Reg(Rd),Arrangement16B,0);
        }
        goto __switch_break0;
    }
    if (aop == arg_Vd_arrangement_Q_sz___2S_00__4S_10__2D_11)
    {
        Rd = x & (1 << 5 - 1);
        Q = (x >> 30) & 1;
        sz = (x >> 22) & 1;
        if (sz == 0 && Q == 0) {
            return new RegisterWithArrangement(V0+Reg(Rd),Arrangement2S,0);
        }
        else if (sz == 0 && Q == 1) {
            return new RegisterWithArrangement(V0+Reg(Rd),Arrangement4S,0);
        }
        else if (sz == 1 && Q == 1) {
            return new RegisterWithArrangement(V0+Reg(Rd),Arrangement2D,0);
        }
        return null;
        goto __switch_break0;
    }
    if (aop == arg_Vd_arrangement_size___4S_1__2D_2)
    {
        Rd = x & (1 << 5 - 1);
        size = (x >> 22) & 3;
        if (size == 1) {
            return new RegisterWithArrangement(V0+Reg(Rd),Arrangement4S,0);
        }
        else if (size == 2) {
            return new RegisterWithArrangement(V0+Reg(Rd),Arrangement2D,0);
        }
        return null;
        goto __switch_break0;
    }
    if (aop == arg_Vd_arrangement_size___8H_0__1Q_3)
    {
        Rd = x & (1 << 5 - 1);
        size = (x >> 22) & 3;
        if (size == 0) {
            return new RegisterWithArrangement(V0+Reg(Rd),Arrangement8H,0);
        }
        else if (size == 3) {
            return new RegisterWithArrangement(V0+Reg(Rd),Arrangement1Q,0);
        }
        return null;
        goto __switch_break0;
    }
    if (aop == arg_Vd_arrangement_size___8H_0__4S_1__2D_2)
    {
        Rd = x & (1 << 5 - 1);
        size = (x >> 22) & 3;
        if (size == 0) {
            return new RegisterWithArrangement(V0+Reg(Rd),Arrangement8H,0);
        }
        else if (size == 1) {
            return new RegisterWithArrangement(V0+Reg(Rd),Arrangement4S,0);
        }
        else if (size == 2) {
            return new RegisterWithArrangement(V0+Reg(Rd),Arrangement2D,0);
        }
        return null;
        goto __switch_break0;
    }
    if (aop == arg_Vd_arrangement_size_Q___4H_00__8H_01__2S_10__4S_11__1D_20__2D_21)
    {
        Rd = x & (1 << 5 - 1);
        size = (x >> 22) & 3;
        Q = (x >> 30) & 1;
        if (size == 0 && Q == 0) {
            return new RegisterWithArrangement(V0+Reg(Rd),Arrangement4H,0);
        }
        else if (size == 0 && Q == 1) {
            return new RegisterWithArrangement(V0+Reg(Rd),Arrangement8H,0);
        }
        else if (size == 1 && Q == 0) {
            return new RegisterWithArrangement(V0+Reg(Rd),Arrangement2S,0);
        }
        else if (size == 1 && Q == 1) {
            return new RegisterWithArrangement(V0+Reg(Rd),Arrangement4S,0);
        }
        else if (size == 2 && Q == 0) {
            return new RegisterWithArrangement(V0+Reg(Rd),Arrangement1D,0);
        }
        else if (size == 2 && Q == 1) {
            return new RegisterWithArrangement(V0+Reg(Rd),Arrangement2D,0);
        }
        return null;
        goto __switch_break0;
    }
    if (aop == arg_Vd_arrangement_size_Q___4H_10__8H_11__2S_20__4S_21)
    {
        Rd = x & (1 << 5 - 1);
        size = (x >> 22) & 3;
        Q = (x >> 30) & 1;
        if (size == 1 && Q == 0) {
            return new RegisterWithArrangement(V0+Reg(Rd),Arrangement4H,0);
        }
        else if (size == 1 && Q == 1) {
            return new RegisterWithArrangement(V0+Reg(Rd),Arrangement8H,0);
        }
        else if (size == 2 && Q == 0) {
            return new RegisterWithArrangement(V0+Reg(Rd),Arrangement2S,0);
        }
        else if (size == 2 && Q == 1) {
            return new RegisterWithArrangement(V0+Reg(Rd),Arrangement4S,0);
        }
        return null;
        goto __switch_break0;
    }
    if (aop == arg_Vd_arrangement_size_Q___8B_00__16B_01)
    {
        Rd = x & (1 << 5 - 1);
        size = (x >> 22) & 3;
        Q = (x >> 30) & 1;
        if (size == 0 && Q == 0) {
            return new RegisterWithArrangement(V0+Reg(Rd),Arrangement8B,0);
        }
        else if (size == 0 && Q == 1) {
            return new RegisterWithArrangement(V0+Reg(Rd),Arrangement16B,0);
        }
        return null;
        goto __switch_break0;
    }
    if (aop == arg_Vd_arrangement_size_Q___8B_00__16B_01__4H_10__8H_11)
    {
        Rd = x & (1 << 5 - 1);
        size = (x >> 22) & 3;
        Q = (x >> 30) & 1;
        if (size == 0 && Q == 0) {
            return new RegisterWithArrangement(V0+Reg(Rd),Arrangement8B,0);
        }
        else if (size == 0 && Q == 1) {
            return new RegisterWithArrangement(V0+Reg(Rd),Arrangement16B,0);
        }
        else if (size == 1 && Q == 0) {
            return new RegisterWithArrangement(V0+Reg(Rd),Arrangement4H,0);
        }
        else if (size == 1 && Q == 1) {
            return new RegisterWithArrangement(V0+Reg(Rd),Arrangement8H,0);
        }
        return null;
        goto __switch_break0;
    }
    if (aop == arg_Vd_arrangement_size_Q___8B_00__16B_01__4H_10__8H_11__2S_20__4S_21)
    {
        Rd = x & (1 << 5 - 1);
        size = (x >> 22) & 3;
        Q = (x >> 30) & 1;
        if (size == 0 && Q == 0) {
            return new RegisterWithArrangement(V0+Reg(Rd),Arrangement8B,0);
        }
        else if (size == 0 && Q == 1) {
            return new RegisterWithArrangement(V0+Reg(Rd),Arrangement16B,0);
        }
        else if (size == 1 && Q == 0) {
            return new RegisterWithArrangement(V0+Reg(Rd),Arrangement4H,0);
        }
        else if (size == 1 && Q == 1) {
            return new RegisterWithArrangement(V0+Reg(Rd),Arrangement8H,0);
        }
        else if (size == 2 && Q == 0) {
            return new RegisterWithArrangement(V0+Reg(Rd),Arrangement2S,0);
        }
        else if (size == 2 && Q == 1) {
            return new RegisterWithArrangement(V0+Reg(Rd),Arrangement4S,0);
        }
        return null;
        goto __switch_break0;
    }
    if (aop == arg_Vd_arrangement_size_Q___8B_00__16B_01__4H_10__8H_11__2S_20__4S_21__2D_31)
    {
        Rd = x & (1 << 5 - 1);
        size = (x >> 22) & 3;
        Q = (x >> 30) & 1;
        if (size == 0 && Q == 0) {
            return new RegisterWithArrangement(V0+Reg(Rd),Arrangement8B,0);
        }
        else if (size == 0 && Q == 1) {
            return new RegisterWithArrangement(V0+Reg(Rd),Arrangement16B,0);
        }
        else if (size == 1 && Q == 0) {
            return new RegisterWithArrangement(V0+Reg(Rd),Arrangement4H,0);
        }
        else if (size == 1 && Q == 1) {
            return new RegisterWithArrangement(V0+Reg(Rd),Arrangement8H,0);
        }
        else if (size == 2 && Q == 0) {
            return new RegisterWithArrangement(V0+Reg(Rd),Arrangement2S,0);
        }
        else if (size == 2 && Q == 1) {
            return new RegisterWithArrangement(V0+Reg(Rd),Arrangement4S,0);
        }
        else if (size == 3 && Q == 1) {
            return new RegisterWithArrangement(V0+Reg(Rd),Arrangement2D,0);
        }
        return null;
        goto __switch_break0;
    }
    if (aop == arg_Vd_arrangement_sz___4S_0__2D_1)
    {
        Rd = x & (1 << 5 - 1);
        sz = (x >> 22) & 1;
        if (sz == 0) {
            return new RegisterWithArrangement(V0+Reg(Rd),Arrangement4S,0);
        }
        else
 {
            return new RegisterWithArrangement(V0+Reg(Rd),Arrangement2D,0);
        }
        goto __switch_break0;
    }
    if (aop == arg_Vd_arrangement_sz_Q___2S_00__4S_01)
    {
        Rd = x & (1 << 5 - 1);
        sz = (x >> 22) & 1;
        Q = (x >> 30) & 1;
        if (sz == 0 && Q == 0) {
            return new RegisterWithArrangement(V0+Reg(Rd),Arrangement2S,0);
        }
        else if (sz == 0 && Q == 1) {
            return new RegisterWithArrangement(V0+Reg(Rd),Arrangement4S,0);
        }
        return null;
        goto __switch_break0;
    }
    if (aop == arg_Vd_arrangement_sz_Q___2S_00__4S_01__2D_11)
    {
        Rd = x & (1 << 5 - 1);
        sz = (x >> 22) & 1;
        Q = (x >> 30) & 1;
        if (sz == 0 && Q == 0) {
            return new RegisterWithArrangement(V0+Reg(Rd),Arrangement2S,0);
        }
        else if (sz == 0 && Q == 1) {
            return new RegisterWithArrangement(V0+Reg(Rd),Arrangement4S,0);
        }
        else if (sz == 1 && Q == 1) {
            return new RegisterWithArrangement(V0+Reg(Rd),Arrangement2D,0);
        }
        return null;
        goto __switch_break0;
    }
    if (aop == arg_Vd_arrangement_sz_Q___2S_10__4S_11)
    {
        Rd = x & (1 << 5 - 1);
        sz = (x >> 22) & 1;
        Q = (x >> 30) & 1;
        if (sz == 1 && Q == 0) {
            return new RegisterWithArrangement(V0+Reg(Rd),Arrangement2S,0);
        }
        else if (sz == 1 && Q == 1) {
            return new RegisterWithArrangement(V0+Reg(Rd),Arrangement4S,0);
        }
        return null;
        goto __switch_break0;
    }
    if (aop == arg_Vd_arrangement_sz_Q___4H_00__8H_01__2S_10__4S_11)
    {
        Rd = x & (1 << 5 - 1);
        sz = (x >> 22) & 1;
        Q = (x >> 30) & 1;
        if (sz == 0 && Q == 0) {
            return new RegisterWithArrangement(V0+Reg(Rd),Arrangement4H,0);
        }
        else if (sz == 0 && Q == 1) {
            return new RegisterWithArrangement(V0+Reg(Rd),Arrangement8H,0);
        }
        else if (sz == 1 && Q == 0) {
            return new RegisterWithArrangement(V0+Reg(Rd),Arrangement2S,0);
        }
        else
 {
            return new RegisterWithArrangement(V0+Reg(Rd),Arrangement4S,0);
        }
        goto __switch_break0;
    }
    if (aop == arg_Vm_22_1__S_0__D_1)
    {
        sz = (x >> 22) & 1;
        var Rm = (x >> 16) & (1 << 5 - 1);
        if (sz == 0) {
            return S0 + Reg(Rm);
        }
        else
 {
            return D0 + Reg(Rm);
        }
        goto __switch_break0;
    }
    if (aop == arg_Vm_22_2__B_0__H_1__S_2__D_3)
    {
        size = (x >> 22) & (1 << 2 - 1);
        Rm = (x >> 16) & (1 << 5 - 1);
        if (size == 0) {
            return B0 + Reg(Rm);
        }
        else if (size == 1) {
            return H0 + Reg(Rm);
        }
        else if (size == 2) {
            return S0 + Reg(Rm);
        }
        else
 {
            return D0 + Reg(Rm);
        }
        goto __switch_break0;
    }
    if (aop == arg_Vm_22_2__D_3)
    {
        size = (x >> 22) & (1 << 2 - 1);
        Rm = (x >> 16) & (1 << 5 - 1);
        if (size == 3) {
            return D0 + Reg(Rm);
        }
        else
 {
            return null;
        }
        goto __switch_break0;
    }
    if (aop == arg_Vm_22_2__H_1__S_2)
    {
        size = (x >> 22) & (1 << 2 - 1);
        Rm = (x >> 16) & (1 << 5 - 1);
        if (size == 1) {
            return H0 + Reg(Rm);
        }
        else if (size == 2) {
            return S0 + Reg(Rm);
        }
        else
 {
            return null;
        }
        goto __switch_break0;
    }
    if (aop == arg_Vm_arrangement_4S)
    {
        Rm = (x >> 16) & (1 << 5 - 1);
        return new RegisterWithArrangement(V0+Reg(Rm),Arrangement4S,0);
        goto __switch_break0;
    }
    if (aop == arg_Vm_arrangement_Q___8B_0__16B_1)
    {
        Rm = (x >> 16) & (1 << 5 - 1);
        Q = (x >> 30) & 1;
        if (Q == 0) {
            return new RegisterWithArrangement(V0+Reg(Rm),Arrangement8B,0);
        }
        else
 {
            return new RegisterWithArrangement(V0+Reg(Rm),Arrangement16B,0);
        }
        goto __switch_break0;
    }
    if (aop == arg_Vm_arrangement_size___8H_0__4S_1__2D_2)
    {
        Rm = (x >> 16) & (1 << 5 - 1);
        size = (x >> 22) & 3;
        if (size == 0) {
            return new RegisterWithArrangement(V0+Reg(Rm),Arrangement8H,0);
        }
        else if (size == 1) {
            return new RegisterWithArrangement(V0+Reg(Rm),Arrangement4S,0);
        }
        else if (size == 2) {
            return new RegisterWithArrangement(V0+Reg(Rm),Arrangement2D,0);
        }
        return null;
        goto __switch_break0;
    }
    if (aop == arg_Vm_arrangement_size___H_1__S_2_index__size_L_H_M__HLM_1__HL_2_1)
    {
        a = default;
        index = default;
        uint vm = default;
        Rm = (x >> 16) & (1 << 4 - 1);
        size = (x >> 22) & 3;
        var H = (x >> 11) & 1;
        var L = (x >> 21) & 1;
        var M = (x >> 20) & 1;
        if (size == 1) {
            a = ArrangementH;
            index = (H << 2) | (L << 1) | M;
            vm = Rm;
        }
        else if (size == 2) {
            a = ArrangementS;
            index = (H << 1) | L;
            vm = (M << 4) | Rm;
        }
        else
 {
            return null;
        }
        return new RegisterWithArrangementAndIndex(V0+Reg(vm),a,uint8(index),0);
        goto __switch_break0;
    }
    if (aop == arg_Vm_arrangement_size_Q___4H_10__8H_11__2S_20__4S_21)
    {
        Rm = (x >> 16) & (1 << 5 - 1);
        size = (x >> 22) & 3;
        Q = (x >> 30) & 1;
        if (size == 1 && Q == 0) {
            return new RegisterWithArrangement(V0+Reg(Rm),Arrangement4H,0);
        }
        else if (size == 1 && Q == 1) {
            return new RegisterWithArrangement(V0+Reg(Rm),Arrangement8H,0);
        }
        else if (size == 2 && Q == 0) {
            return new RegisterWithArrangement(V0+Reg(Rm),Arrangement2S,0);
        }
        else if (size == 2 && Q == 1) {
            return new RegisterWithArrangement(V0+Reg(Rm),Arrangement4S,0);
        }
        return null;
        goto __switch_break0;
    }
    if (aop == arg_Vm_arrangement_size_Q___8B_00__16B_01)
    {
        Rm = (x >> 16) & (1 << 5 - 1);
        size = (x >> 22) & 3;
        Q = (x >> 30) & 1;
        if (size == 0 && Q == 0) {
            return new RegisterWithArrangement(V0+Reg(Rm),Arrangement8B,0);
        }
        else if (size == 0 && Q == 1) {
            return new RegisterWithArrangement(V0+Reg(Rm),Arrangement16B,0);
        }
        return null;
        goto __switch_break0;
    }
    if (aop == arg_Vm_arrangement_size_Q___8B_00__16B_01__1D_30__2D_31)
    {
        Rm = (x >> 16) & (1 << 5 - 1);
        size = (x >> 22) & 3;
        Q = (x >> 30) & 1;
        if (size == 0 && Q == 0) {
            return new RegisterWithArrangement(V0+Reg(Rm),Arrangement8B,0);
        }
        else if (size == 0 && Q == 1) {
            return new RegisterWithArrangement(V0+Reg(Rm),Arrangement16B,0);
        }
        else if (size == 3 && Q == 0) {
            return new RegisterWithArrangement(V0+Reg(Rm),Arrangement1D,0);
        }
        else if (size == 3 && Q == 1) {
            return new RegisterWithArrangement(V0+Reg(Rm),Arrangement2D,0);
        }
        return null;
        goto __switch_break0;
    }
    if (aop == arg_Vm_arrangement_size_Q___8B_00__16B_01__4H_10__8H_11__2S_20__4S_21)
    {
        Rm = (x >> 16) & (1 << 5 - 1);
        size = (x >> 22) & 3;
        Q = (x >> 30) & 1;
        if (size == 0 && Q == 0) {
            return new RegisterWithArrangement(V0+Reg(Rm),Arrangement8B,0);
        }
        else if (size == 0 && Q == 1) {
            return new RegisterWithArrangement(V0+Reg(Rm),Arrangement16B,0);
        }
        else if (size == 1 && Q == 0) {
            return new RegisterWithArrangement(V0+Reg(Rm),Arrangement4H,0);
        }
        else if (size == 1 && Q == 1) {
            return new RegisterWithArrangement(V0+Reg(Rm),Arrangement8H,0);
        }
        else if (size == 2 && Q == 0) {
            return new RegisterWithArrangement(V0+Reg(Rm),Arrangement2S,0);
        }
        else if (size == 2 && Q == 1) {
            return new RegisterWithArrangement(V0+Reg(Rm),Arrangement4S,0);
        }
        return null;
        goto __switch_break0;
    }
    if (aop == arg_Vm_arrangement_size_Q___8B_00__16B_01__4H_10__8H_11__2S_20__4S_21__2D_31)
    {
        Rm = (x >> 16) & (1 << 5 - 1);
        size = (x >> 22) & 3;
        Q = (x >> 30) & 1;
        if (size == 0 && Q == 0) {
            return new RegisterWithArrangement(V0+Reg(Rm),Arrangement8B,0);
        }
        else if (size == 0 && Q == 1) {
            return new RegisterWithArrangement(V0+Reg(Rm),Arrangement16B,0);
        }
        else if (size == 1 && Q == 0) {
            return new RegisterWithArrangement(V0+Reg(Rm),Arrangement4H,0);
        }
        else if (size == 1 && Q == 1) {
            return new RegisterWithArrangement(V0+Reg(Rm),Arrangement8H,0);
        }
        else if (size == 2 && Q == 0) {
            return new RegisterWithArrangement(V0+Reg(Rm),Arrangement2S,0);
        }
        else if (size == 2 && Q == 1) {
            return new RegisterWithArrangement(V0+Reg(Rm),Arrangement4S,0);
        }
        else if (size == 3 && Q == 1) {
            return new RegisterWithArrangement(V0+Reg(Rm),Arrangement2D,0);
        }
        return null;
        goto __switch_break0;
    }
    if (aop == arg_Vm_arrangement_sz_Q___2S_00__4S_01__2D_11)
    {
        Rm = (x >> 16) & (1 << 5 - 1);
        sz = (x >> 22) & 1;
        Q = (x >> 30) & 1;
        if (sz == 0 && Q == 0) {
            return new RegisterWithArrangement(V0+Reg(Rm),Arrangement2S,0);
        }
        else if (sz == 0 && Q == 1) {
            return new RegisterWithArrangement(V0+Reg(Rm),Arrangement4S,0);
        }
        else if (sz == 1 && Q == 1) {
            return new RegisterWithArrangement(V0+Reg(Rm),Arrangement2D,0);
        }
        return null;
        goto __switch_break0;
    }
    if (aop == arg_Vm_arrangement_sz___S_0__D_1_index__sz_L_H__HL_00__H_10_1)
    {
        a = default;
        index = default;
        Rm = (x >> 16) & (1 << 5 - 1);
        sz = (x >> 22) & 1;
        H = (x >> 11) & 1;
        L = (x >> 21) & 1;
        if (sz == 0) {
            a = ArrangementS;
            index = (H << 1) | L;
        }
        else if (sz == 1 && L == 0) {
            a = ArrangementD;
            index = H;
        }
        else
 {
            return null;
        }
        return new RegisterWithArrangementAndIndex(V0+Reg(Rm),a,uint8(index),0);
        goto __switch_break0;
    }
    if (aop == arg_Vn_19_4__B_1__H_2__S_4__D_8)
    {
        immh = (x >> 19) & (1 << 4 - 1);
        Rn = (x >> 5) & (1 << 5 - 1);
        if (immh == 1) {
            return B0 + Reg(Rn);
        }
        else if (immh >> 1 == 1) {
            return H0 + Reg(Rn);
        }
        else if (immh >> 2 == 1) {
            return S0 + Reg(Rn);
        }
        else if (immh >> 3 == 1) {
            return D0 + Reg(Rn);
        }
        else
 {
            return null;
        }
        goto __switch_break0;
    }
    if (aop == arg_Vn_19_4__D_8)
    {
        immh = (x >> 19) & (1 << 4 - 1);
        Rn = (x >> 5) & (1 << 5 - 1);
        if (immh >> 3 == 1) {
            return D0 + Reg(Rn);
        }
        else
 {
            return null;
        }
        goto __switch_break0;
    }
    if (aop == arg_Vn_19_4__H_1__S_2__D_4)
    {
        immh = (x >> 19) & (1 << 4 - 1);
        Rn = (x >> 5) & (1 << 5 - 1);
        if (immh == 1) {
            return H0 + Reg(Rn);
        }
        else if (immh >> 1 == 1) {
            return S0 + Reg(Rn);
        }
        else if (immh >> 2 == 1) {
            return D0 + Reg(Rn);
        }
        else
 {
            return null;
        }
        goto __switch_break0;
    }
    if (aop == arg_Vn_19_4__S_4__D_8)
    {
        immh = (x >> 19) & (1 << 4 - 1);
        Rn = (x >> 5) & (1 << 5 - 1);
        if (immh >> 2 == 1) {
            return S0 + Reg(Rn);
        }
        else if (immh >> 3 == 1) {
            return D0 + Reg(Rn);
        }
        else
 {
            return null;
        }
        goto __switch_break0;
    }
    if (aop == arg_Vn_1_arrangement_16B)
    {
        Rn = (x >> 5) & (1 << 5 - 1);
        return new RegisterWithArrangement(V0+Reg(Rn),Arrangement16B,1);
        goto __switch_break0;
    }
    if (aop == arg_Vn_22_1__D_1)
    {
        sz = (x >> 22) & 1;
        Rn = (x >> 5) & (1 << 5 - 1);
        if (sz == 1) {
            return D0 + Reg(Rn);
        }
        return null;
        goto __switch_break0;
    }
    if (aop == arg_Vn_22_1__S_0__D_1)
    {
        sz = (x >> 22) & 1;
        Rn = (x >> 5) & (1 << 5 - 1);
        if (sz == 0) {
            return S0 + Reg(Rn);
        }
        else
 {
            return D0 + Reg(Rn);
        }
        goto __switch_break0;
    }
    if (aop == arg_Vn_22_2__B_0__H_1__S_2__D_3)
    {
        size = (x >> 22) & (1 << 2 - 1);
        Rn = (x >> 5) & (1 << 5 - 1);
        if (size == 0) {
            return B0 + Reg(Rn);
        }
        else if (size == 1) {
            return H0 + Reg(Rn);
        }
        else if (size == 2) {
            return S0 + Reg(Rn);
        }
        else
 {
            return D0 + Reg(Rn);
        }
        goto __switch_break0;
    }
    if (aop == arg_Vn_22_2__D_3)
    {
        size = (x >> 22) & (1 << 2 - 1);
        Rn = (x >> 5) & (1 << 5 - 1);
        if (size == 3) {
            return D0 + Reg(Rn);
        }
        else
 {
            return null;
        }
        goto __switch_break0;
    }
    if (aop == arg_Vn_22_2__H_0__S_1__D_2)
    {
        size = (x >> 22) & (1 << 2 - 1);
        Rn = (x >> 5) & (1 << 5 - 1);
        if (size == 0) {
            return H0 + Reg(Rn);
        }
        else if (size == 1) {
            return S0 + Reg(Rn);
        }
        else if (size == 2) {
            return D0 + Reg(Rn);
        }
        else
 {
            return null;
        }
        goto __switch_break0;
    }
    if (aop == arg_Vn_22_2__H_1__S_2)
    {
        size = (x >> 22) & (1 << 2 - 1);
        Rn = (x >> 5) & (1 << 5 - 1);
        if (size == 1) {
            return H0 + Reg(Rn);
        }
        else if (size == 2) {
            return S0 + Reg(Rn);
        }
        else
 {
            return null;
        }
        goto __switch_break0;
    }
    if (aop == arg_Vn_2_arrangement_16B)
    {
        Rn = (x >> 5) & (1 << 5 - 1);
        return new RegisterWithArrangement(V0+Reg(Rn),Arrangement16B,2);
        goto __switch_break0;
    }
    if (aop == arg_Vn_3_arrangement_16B)
    {
        Rn = (x >> 5) & (1 << 5 - 1);
        return new RegisterWithArrangement(V0+Reg(Rn),Arrangement16B,3);
        goto __switch_break0;
    }
    if (aop == arg_Vn_4_arrangement_16B)
    {
        Rn = (x >> 5) & (1 << 5 - 1);
        return new RegisterWithArrangement(V0+Reg(Rn),Arrangement16B,4);
        goto __switch_break0;
    }
    if (aop == arg_Vn_arrangement_16B)
    {
        Rn = (x >> 5) & (1 << 5 - 1);
        return new RegisterWithArrangement(V0+Reg(Rn),Arrangement16B,0);
        goto __switch_break0;
    }
    if (aop == arg_Vn_arrangement_4S)
    {
        Rn = (x >> 5) & (1 << 5 - 1);
        return new RegisterWithArrangement(V0+Reg(Rn),Arrangement4S,0);
        goto __switch_break0;
    }
    if (aop == arg_Vn_arrangement_D_index__1)
    {
        Rn = (x >> 5) & (1 << 5 - 1);
        return new RegisterWithArrangementAndIndex(V0+Reg(Rn),ArrangementD,1,0);
        goto __switch_break0;
    }
    if (aop == arg_Vn_arrangement_D_index__imm5_1)
    {
        Rn = (x >> 5) & (1 << 5 - 1);
        index = (x >> 20) & 1;
        return new RegisterWithArrangementAndIndex(V0+Reg(Rn),ArrangementD,uint8(index),0);
        goto __switch_break0;
    }
    if (aop == arg_Vn_arrangement_imm5___B_1__H_2_index__imm5__imm5lt41gt_1__imm5lt42gt_2_1)
    {
        a = default;
        index = default;
        Rn = (x >> 5) & (1 << 5 - 1);
        imm5 = (x >> 16) & (1 << 5 - 1);
        if (imm5 & 1 == 1) {
            a = ArrangementB;
            index = imm5 >> 1;
        }
        else if (imm5 & 2 == 2) {
            a = ArrangementH;
            index = imm5 >> 2;
        }
        else
 {
            return null;
        }
        return new RegisterWithArrangementAndIndex(V0+Reg(Rn),a,uint8(index),0);
        goto __switch_break0;
    }
    if (aop == arg_Vn_arrangement_imm5___B_1__H_2__S_4__D_8_index__imm5_imm4__imm4lt30gt_1__imm4lt31gt_2__imm4lt32gt_4__imm4lt3gt_8_1)
    {
        a = default;
        index = default;
        Rn = (x >> 5) & (1 << 5 - 1);
        imm5 = (x >> 16) & (1 << 5 - 1);
        imm4 = (x >> 11) & (1 << 4 - 1);
        if (imm5 & 1 == 1) {
            a = ArrangementB;
            index = imm4;
        }
        else if (imm5 & 2 == 2) {
            a = ArrangementH;
            index = imm4 >> 1;
        }
        else if (imm5 & 4 == 4) {
            a = ArrangementS;
            index = imm4 >> 2;
        }
        else if (imm5 & 8 == 8) {
            a = ArrangementD;
            index = imm4 >> 3;
        }
        else
 {
            return null;
        }
        return new RegisterWithArrangementAndIndex(V0+Reg(Rn),a,uint8(index),0);
        goto __switch_break0;
    }
    if (aop == arg_Vn_arrangement_imm5___B_1__H_2__S_4__D_8_index__imm5__imm5lt41gt_1__imm5lt42gt_2__imm5lt43gt_4__imm5lt4gt_8_1)
    {
        a = default;
        index = default;
        Rn = (x >> 5) & (1 << 5 - 1);
        imm5 = (x >> 16) & (1 << 5 - 1);
        if (imm5 & 1 == 1) {
            a = ArrangementB;
            index = imm5 >> 1;
        }
        else if (imm5 & 2 == 2) {
            a = ArrangementH;
            index = imm5 >> 2;
        }
        else if (imm5 & 4 == 4) {
            a = ArrangementS;
            index = imm5 >> 3;
        }
        else if (imm5 & 8 == 8) {
            a = ArrangementD;
            index = imm5 >> 4;
        }
        else
 {
            return null;
        }
        return new RegisterWithArrangementAndIndex(V0+Reg(Rn),a,uint8(index),0);
        goto __switch_break0;
    }
    if (aop == arg_Vn_arrangement_imm5___B_1__H_2__S_4_index__imm5__imm5lt41gt_1__imm5lt42gt_2__imm5lt43gt_4_1)
    {
        a = default;
        index = default;
        Rn = (x >> 5) & (1 << 5 - 1);
        imm5 = (x >> 16) & (1 << 5 - 1);
        if (imm5 & 1 == 1) {
            a = ArrangementB;
            index = imm5 >> 1;
        }
        else if (imm5 & 2 == 2) {
            a = ArrangementH;
            index = imm5 >> 2;
        }
        else if (imm5 & 4 == 4) {
            a = ArrangementS;
            index = imm5 >> 3;
        }
        else
 {
            return null;
        }
        return new RegisterWithArrangementAndIndex(V0+Reg(Rn),a,uint8(index),0);
        goto __switch_break0;
    }
    if (aop == arg_Vn_arrangement_imm5___D_8_index__imm5_1)
    {
        a = default;
        index = default;
        Rn = (x >> 5) & (1 << 5 - 1);
        imm5 = (x >> 16) & (1 << 5 - 1);
        if (imm5 & 15 == 8) {
            a = ArrangementD;
            index = imm5 >> 4;
        }
        else
 {
            return null;
        }
        return new RegisterWithArrangementAndIndex(V0+Reg(Rn),a,uint8(index),0);
        goto __switch_break0;
    }
    if (aop == arg_Vn_arrangement_immh_Q___SEEAdvancedSIMDmodifiedimmediate_00__2S_40__4S_41__2D_81)
    {
        Rn = (x >> 5) & (1 << 5 - 1);
        immh = (x >> 19) & (1 << 4 - 1);
        Q = (x >> 30) & 1;
        if (immh >> 2 == 1) {
            if (Q == 0) {
                return new RegisterWithArrangement(V0+Reg(Rn),Arrangement2S,0);
            }
            else
 {
                return new RegisterWithArrangement(V0+Reg(Rn),Arrangement4S,0);
            }

        }
        else if (immh >> 3 == 1) {
            if (Q == 1) {
                return new RegisterWithArrangement(V0+Reg(Rn),Arrangement2D,0);
            }
        }
        return null;
        goto __switch_break0;
    }
    if (aop == arg_Vn_arrangement_immh_Q___SEEAdvancedSIMDmodifiedimmediate_00__8B_10__16B_11__4H_20__8H_21__2S_40__4S_41)
    {
        Rn = (x >> 5) & (1 << 5 - 1);
        immh = (x >> 19) & (1 << 4 - 1);
        Q = (x >> 30) & 1;
        if (immh == 1) {
            if (Q == 0) {
                return new RegisterWithArrangement(V0+Reg(Rn),Arrangement8B,0);
            }
            else
 {
                return new RegisterWithArrangement(V0+Reg(Rn),Arrangement16B,0);
            }

        }
        else if (immh >> 1 == 1) {
            if (Q == 0) {
                return new RegisterWithArrangement(V0+Reg(Rn),Arrangement4H,0);
            }
            else
 {
                return new RegisterWithArrangement(V0+Reg(Rn),Arrangement8H,0);
            }

        }
        else if (immh >> 2 == 1) {
            if (Q == 0) {
                return new RegisterWithArrangement(V0+Reg(Rn),Arrangement2S,0);
            }
            else
 {
                return new RegisterWithArrangement(V0+Reg(Rn),Arrangement4S,0);
            }

        }
        return null;
        goto __switch_break0;
    }
    if (aop == arg_Vn_arrangement_immh_Q___SEEAdvancedSIMDmodifiedimmediate_00__8B_10__16B_11__4H_20__8H_21__2S_40__4S_41__2D_81)
    {
        Rn = (x >> 5) & (1 << 5 - 1);
        immh = (x >> 19) & (1 << 4 - 1);
        Q = (x >> 30) & 1;
        if (immh == 1) {
            if (Q == 0) {
                return new RegisterWithArrangement(V0+Reg(Rn),Arrangement8B,0);
            }
            else
 {
                return new RegisterWithArrangement(V0+Reg(Rn),Arrangement16B,0);
            }

        }
        else if (immh >> 1 == 1) {
            if (Q == 0) {
                return new RegisterWithArrangement(V0+Reg(Rn),Arrangement4H,0);
            }
            else
 {
                return new RegisterWithArrangement(V0+Reg(Rn),Arrangement8H,0);
            }

        }
        else if (immh >> 2 == 1) {
            if (Q == 0) {
                return new RegisterWithArrangement(V0+Reg(Rn),Arrangement2S,0);
            }
            else
 {
                return new RegisterWithArrangement(V0+Reg(Rn),Arrangement4S,0);
            }

        }
        else if (immh >> 3 == 1) {
            if (Q == 1) {
                return new RegisterWithArrangement(V0+Reg(Rn),Arrangement2D,0);
            }
        }
        return null;
        goto __switch_break0;
    }
    if (aop == arg_Vn_arrangement_immh___SEEAdvancedSIMDmodifiedimmediate_0__8H_1__4S_2__2D_4)
    {
        Rn = (x >> 5) & (1 << 5 - 1);
        immh = (x >> 19) & (1 << 4 - 1);
        if (immh == 1) {
            return new RegisterWithArrangement(V0+Reg(Rn),Arrangement8H,0);
        }
        else if (immh >> 1 == 1) {
            return new RegisterWithArrangement(V0+Reg(Rn),Arrangement4S,0);
        }
        else if (immh >> 2 == 1) {
            return new RegisterWithArrangement(V0+Reg(Rn),Arrangement2D,0);
        }
        return null;
        goto __switch_break0;
    }
    if (aop == arg_Vn_arrangement_Q___8B_0__16B_1)
    {
        Rn = (x >> 5) & (1 << 5 - 1);
        Q = (x >> 30) & 1;
        if (Q == 0) {
            return new RegisterWithArrangement(V0+Reg(Rn),Arrangement8B,0);
        }
        else
 {
            return new RegisterWithArrangement(V0+Reg(Rn),Arrangement16B,0);
        }
        goto __switch_break0;
    }
    if (aop == arg_Vn_arrangement_Q_sz___2S_00__4S_10__2D_11)
    {
        Rn = (x >> 5) & (1 << 5 - 1);
        Q = (x >> 30) & 1;
        sz = (x >> 22) & 1;
        if (sz == 0 && Q == 0) {
            return new RegisterWithArrangement(V0+Reg(Rn),Arrangement2S,0);
        }
        else if (sz == 0 && Q == 1) {
            return new RegisterWithArrangement(V0+Reg(Rn),Arrangement4S,0);
        }
        else if (sz == 1 && Q == 1) {
            return new RegisterWithArrangement(V0+Reg(Rn),Arrangement2D,0);
        }
        return null;
        goto __switch_break0;
    }
    if (aop == arg_Vn_arrangement_Q_sz___4S_10)
    {
        Rn = (x >> 5) & (1 << 5 - 1);
        Q = (x >> 30) & 1;
        sz = (x >> 22) & 1;
        if (sz == 0 && Q == 1) {
            return new RegisterWithArrangement(V0+Reg(Rn),Arrangement4S,0);
        }
        return null;
        goto __switch_break0;
    }
    if (aop == arg_Vn_arrangement_S_index__imm5__imm5lt41gt_1__imm5lt42gt_2__imm5lt43gt_4_1)
    {
        index = default;
        Rn = (x >> 5) & (1 << 5 - 1);
        imm5 = (x >> 16) & (1 << 5 - 1);
        index = imm5 >> 3;
        return new RegisterWithArrangementAndIndex(V0+Reg(Rn),ArrangementS,uint8(index),0);
        goto __switch_break0;
    }
    if (aop == arg_Vn_arrangement_size___2D_3)
    {
        Rn = (x >> 5) & (1 << 5 - 1);
        size = (x >> 22) & 3;
        if (size == 3) {
            return new RegisterWithArrangement(V0+Reg(Rn),Arrangement2D,0);
        }
        return null;
        goto __switch_break0;
    }
    if (aop == arg_Vn_arrangement_size___8H_0__4S_1__2D_2)
    {
        Rn = (x >> 5) & (1 << 5 - 1);
        size = (x >> 22) & 3;
        if (size == 0) {
            return new RegisterWithArrangement(V0+Reg(Rn),Arrangement8H,0);
        }
        else if (size == 1) {
            return new RegisterWithArrangement(V0+Reg(Rn),Arrangement4S,0);
        }
        else if (size == 2) {
            return new RegisterWithArrangement(V0+Reg(Rn),Arrangement2D,0);
        }
        return null;
        goto __switch_break0;
    }
    if (aop == arg_Vn_arrangement_size_Q___4H_10__8H_11__2S_20__4S_21)
    {
        Rn = (x >> 5) & (1 << 5 - 1);
        size = (x >> 22) & 3;
        Q = (x >> 30) & 1;
        if (size == 1 && Q == 0) {
            return new RegisterWithArrangement(V0+Reg(Rn),Arrangement4H,0);
        }
        else if (size == 1 && Q == 1) {
            return new RegisterWithArrangement(V0+Reg(Rn),Arrangement8H,0);
        }
        else if (size == 2 && Q == 0) {
            return new RegisterWithArrangement(V0+Reg(Rn),Arrangement2S,0);
        }
        else if (size == 2 && Q == 1) {
            return new RegisterWithArrangement(V0+Reg(Rn),Arrangement4S,0);
        }
        return null;
        goto __switch_break0;
    }
    if (aop == arg_Vn_arrangement_size_Q___8B_00__16B_01)
    {
        Rn = (x >> 5) & (1 << 5 - 1);
        size = (x >> 22) & 3;
        Q = (x >> 30) & 1;
        if (size == 0 && Q == 0) {
            return new RegisterWithArrangement(V0+Reg(Rn),Arrangement8B,0);
        }
        else if (size == 0 && Q == 1) {
            return new RegisterWithArrangement(V0+Reg(Rn),Arrangement16B,0);
        }
        return null;
        goto __switch_break0;
    }
    if (aop == arg_Vn_arrangement_size_Q___8B_00__16B_01__1D_30__2D_31)
    {
        Rn = (x >> 5) & (1 << 5 - 1);
        size = (x >> 22) & 3;
        Q = (x >> 30) & 1;
        if (size == 0 && Q == 0) {
            return new RegisterWithArrangement(V0+Reg(Rn),Arrangement8B,0);
        }
        else if (size == 0 && Q == 1) {
            return new RegisterWithArrangement(V0+Reg(Rn),Arrangement16B,0);
        }
        else if (size == 3 && Q == 0) {
            return new RegisterWithArrangement(V0+Reg(Rn),Arrangement1D,0);
        }
        else if (size == 3 && Q == 1) {
            return new RegisterWithArrangement(V0+Reg(Rn),Arrangement2D,0);
        }
        return null;
        goto __switch_break0;
    }
    if (aop == arg_Vn_arrangement_size_Q___8B_00__16B_01__4H_10__8H_11)
    {
        Rn = (x >> 5) & (1 << 5 - 1);
        size = (x >> 22) & 3;
        Q = (x >> 30) & 1;
        if (size == 0 && Q == 0) {
            return new RegisterWithArrangement(V0+Reg(Rn),Arrangement8B,0);
        }
        else if (size == 0 && Q == 1) {
            return new RegisterWithArrangement(V0+Reg(Rn),Arrangement16B,0);
        }
        else if (size == 1 && Q == 0) {
            return new RegisterWithArrangement(V0+Reg(Rn),Arrangement4H,0);
        }
        else if (size == 1 && Q == 1) {
            return new RegisterWithArrangement(V0+Reg(Rn),Arrangement8H,0);
        }
        return null;
        goto __switch_break0;
    }
    if (aop == arg_Vn_arrangement_size_Q___8B_00__16B_01__4H_10__8H_11__2S_20__4S_21)
    {
        Rn = (x >> 5) & (1 << 5 - 1);
        size = (x >> 22) & 3;
        Q = (x >> 30) & 1;
        if (size == 0 && Q == 0) {
            return new RegisterWithArrangement(V0+Reg(Rn),Arrangement8B,0);
        }
        else if (size == 0 && Q == 1) {
            return new RegisterWithArrangement(V0+Reg(Rn),Arrangement16B,0);
        }
        else if (size == 1 && Q == 0) {
            return new RegisterWithArrangement(V0+Reg(Rn),Arrangement4H,0);
        }
        else if (size == 1 && Q == 1) {
            return new RegisterWithArrangement(V0+Reg(Rn),Arrangement8H,0);
        }
        else if (size == 2 && Q == 0) {
            return new RegisterWithArrangement(V0+Reg(Rn),Arrangement2S,0);
        }
        else if (size == 2 && Q == 1) {
            return new RegisterWithArrangement(V0+Reg(Rn),Arrangement4S,0);
        }
        return null;
        goto __switch_break0;
    }
    if (aop == arg_Vn_arrangement_size_Q___8B_00__16B_01__4H_10__8H_11__2S_20__4S_21__2D_31)
    {
        Rn = (x >> 5) & (1 << 5 - 1);
        size = (x >> 22) & 3;
        Q = (x >> 30) & 1;
        if (size == 0 && Q == 0) {
            return new RegisterWithArrangement(V0+Reg(Rn),Arrangement8B,0);
        }
        else if (size == 0 && Q == 1) {
            return new RegisterWithArrangement(V0+Reg(Rn),Arrangement16B,0);
        }
        else if (size == 1 && Q == 0) {
            return new RegisterWithArrangement(V0+Reg(Rn),Arrangement4H,0);
        }
        else if (size == 1 && Q == 1) {
            return new RegisterWithArrangement(V0+Reg(Rn),Arrangement8H,0);
        }
        else if (size == 2 && Q == 0) {
            return new RegisterWithArrangement(V0+Reg(Rn),Arrangement2S,0);
        }
        else if (size == 2 && Q == 1) {
            return new RegisterWithArrangement(V0+Reg(Rn),Arrangement4S,0);
        }
        else if (size == 3 && Q == 1) {
            return new RegisterWithArrangement(V0+Reg(Rn),Arrangement2D,0);
        }
        return null;
        goto __switch_break0;
    }
    if (aop == arg_Vn_arrangement_size_Q___8B_00__16B_01__4H_10__8H_11__4S_21)
    {
        Rn = (x >> 5) & (1 << 5 - 1);
        size = (x >> 22) & 3;
        Q = (x >> 30) & 1;
        if (size == 0 && Q == 0) {
            return new RegisterWithArrangement(V0+Reg(Rn),Arrangement8B,0);
        }
        else if (size == 0 && Q == 1) {
            return new RegisterWithArrangement(V0+Reg(Rn),Arrangement16B,0);
        }
        else if (size == 1 && Q == 0) {
            return new RegisterWithArrangement(V0+Reg(Rn),Arrangement4H,0);
        }
        else if (size == 1 && Q == 1) {
            return new RegisterWithArrangement(V0+Reg(Rn),Arrangement8H,0);
        }
        else if (size == 2 && Q == 1) {
            return new RegisterWithArrangement(V0+Reg(Rn),Arrangement4S,0);
        }
        return null;
        goto __switch_break0;
    }
    if (aop == arg_Vn_arrangement_sz___2D_1)
    {
        Rn = (x >> 5) & (1 << 5 - 1);
        sz = (x >> 22) & 1;
        if (sz == 1) {
            return new RegisterWithArrangement(V0+Reg(Rn),Arrangement2D,0);
        }
        return null;
        goto __switch_break0;
    }
    if (aop == arg_Vn_arrangement_sz___2S_0__2D_1)
    {
        Rn = (x >> 5) & (1 << 5 - 1);
        sz = (x >> 22) & 1;
        if (sz == 0) {
            return new RegisterWithArrangement(V0+Reg(Rn),Arrangement2S,0);
        }
        else
 {
            return new RegisterWithArrangement(V0+Reg(Rn),Arrangement2D,0);
        }
        goto __switch_break0;
    }
    if (aop == arg_Vn_arrangement_sz___4S_0__2D_1)
    {
        Rn = (x >> 5) & (1 << 5 - 1);
        sz = (x >> 22) & 1;
        if (sz == 0) {
            return new RegisterWithArrangement(V0+Reg(Rn),Arrangement4S,0);
        }
        else
 {
            return new RegisterWithArrangement(V0+Reg(Rn),Arrangement2D,0);
        }
        goto __switch_break0;
    }
    if (aop == arg_Vn_arrangement_sz_Q___2S_00__4S_01)
    {
        Rn = (x >> 5) & (1 << 5 - 1);
        sz = (x >> 22) & 1;
        Q = (x >> 30) & 1;
        if (sz == 0 && Q == 0) {
            return new RegisterWithArrangement(V0+Reg(Rn),Arrangement2S,0);
        }
        else if (sz == 0 && Q == 1) {
            return new RegisterWithArrangement(V0+Reg(Rn),Arrangement4S,0);
        }
        return null;
        goto __switch_break0;
    }
    if (aop == arg_Vn_arrangement_sz_Q___2S_00__4S_01__2D_11)
    {
        Rn = (x >> 5) & (1 << 5 - 1);
        sz = (x >> 22) & 1;
        Q = (x >> 30) & 1;
        if (sz == 0 && Q == 0) {
            return new RegisterWithArrangement(V0+Reg(Rn),Arrangement2S,0);
        }
        else if (sz == 0 && Q == 1) {
            return new RegisterWithArrangement(V0+Reg(Rn),Arrangement4S,0);
        }
        else if (sz == 1 && Q == 1) {
            return new RegisterWithArrangement(V0+Reg(Rn),Arrangement2D,0);
        }
        return null;
        goto __switch_break0;
    }
    if (aop == arg_Vn_arrangement_sz_Q___4H_00__8H_01__2S_10__4S_11)
    {
        Rn = (x >> 5) & (1 << 5 - 1);
        sz = (x >> 22) & 1;
        Q = (x >> 30) & 1;
        if (sz == 0 && Q == 0) {
            return new RegisterWithArrangement(V0+Reg(Rn),Arrangement4H,0);
        }
        else if (sz == 0 && Q == 1) {
            return new RegisterWithArrangement(V0+Reg(Rn),Arrangement8H,0);
        }
        else if (sz == 1 && Q == 0) {
            return new RegisterWithArrangement(V0+Reg(Rn),Arrangement2S,0);
        }
        else
 {
            return new RegisterWithArrangement(V0+Reg(Rn),Arrangement4S,0);
        }
        goto __switch_break0;
    }
    if (aop == arg_Vt_1_arrangement_B_index__Q_S_size_1)
    {
        Rt = x & (1 << 5 - 1);
        Q = (x >> 30) & 1;
        var S = (x >> 12) & 1;
        size = (x >> 10) & 3;
        index = (Q << 3) | (S << 2) | (size);
        return new RegisterWithArrangementAndIndex(V0+Reg(Rt),ArrangementB,uint8(index),1);
        goto __switch_break0;
    }
    if (aop == arg_Vt_1_arrangement_D_index__Q_1)
    {
        Rt = x & (1 << 5 - 1);
        index = (x >> 30) & 1;
        return new RegisterWithArrangementAndIndex(V0+Reg(Rt),ArrangementD,uint8(index),1);
        goto __switch_break0;
    }
    if (aop == arg_Vt_1_arrangement_H_index__Q_S_size_1)
    {
        Rt = x & (1 << 5 - 1);
        Q = (x >> 30) & 1;
        S = (x >> 12) & 1;
        size = (x >> 11) & 1;
        index = (Q << 2) | (S << 1) | (size);
        return new RegisterWithArrangementAndIndex(V0+Reg(Rt),ArrangementH,uint8(index),1);
        goto __switch_break0;
    }
    if (aop == arg_Vt_1_arrangement_S_index__Q_S_1)
    {
        Rt = x & (1 << 5 - 1);
        Q = (x >> 30) & 1;
        S = (x >> 12) & 1;
        index = (Q << 1) | S;
        return new RegisterWithArrangementAndIndex(V0+Reg(Rt),ArrangementS,uint8(index),1);
        goto __switch_break0;
    }
    if (aop == arg_Vt_1_arrangement_size_Q___8B_00__16B_01__4H_10__8H_11__2S_20__4S_21__1D_30__2D_31)
    {
        Rt = x & (1 << 5 - 1);
        Q = (x >> 30) & 1;
        size = (x >> 10) & 3;
        if (size == 0 && Q == 0) {
            return new RegisterWithArrangement(V0+Reg(Rt),Arrangement8B,1);
        }
        else if (size == 0 && Q == 1) {
            return new RegisterWithArrangement(V0+Reg(Rt),Arrangement16B,1);
        }
        else if (size == 1 && Q == 0) {
            return new RegisterWithArrangement(V0+Reg(Rt),Arrangement4H,1);
        }
        else if (size == 1 && Q == 1) {
            return new RegisterWithArrangement(V0+Reg(Rt),Arrangement8H,1);
        }
        else if (size == 2 && Q == 0) {
            return new RegisterWithArrangement(V0+Reg(Rt),Arrangement2S,1);
        }
        else if (size == 2 && Q == 1) {
            return new RegisterWithArrangement(V0+Reg(Rt),Arrangement4S,1);
        }
        else if (size == 3 && Q == 0) {
            return new RegisterWithArrangement(V0+Reg(Rt),Arrangement1D,1);
        }
        else
 {
            return new RegisterWithArrangement(V0+Reg(Rt),Arrangement2D,1);
        }
        goto __switch_break0;
    }
    if (aop == arg_Vt_2_arrangement_B_index__Q_S_size_1)
    {
        Rt = x & (1 << 5 - 1);
        Q = (x >> 30) & 1;
        S = (x >> 12) & 1;
        size = (x >> 10) & 3;
        index = (Q << 3) | (S << 2) | (size);
        return new RegisterWithArrangementAndIndex(V0+Reg(Rt),ArrangementB,uint8(index),2);
        goto __switch_break0;
    }
    if (aop == arg_Vt_2_arrangement_D_index__Q_1)
    {
        Rt = x & (1 << 5 - 1);
        index = (x >> 30) & 1;
        return new RegisterWithArrangementAndIndex(V0+Reg(Rt),ArrangementD,uint8(index),2);
        goto __switch_break0;
    }
    if (aop == arg_Vt_2_arrangement_H_index__Q_S_size_1)
    {
        Rt = x & (1 << 5 - 1);
        Q = (x >> 30) & 1;
        S = (x >> 12) & 1;
        size = (x >> 11) & 1;
        index = (Q << 2) | (S << 1) | (size);
        return new RegisterWithArrangementAndIndex(V0+Reg(Rt),ArrangementH,uint8(index),2);
        goto __switch_break0;
    }
    if (aop == arg_Vt_2_arrangement_S_index__Q_S_1)
    {
        Rt = x & (1 << 5 - 1);
        Q = (x >> 30) & 1;
        S = (x >> 12) & 1;
        index = (Q << 1) | S;
        return new RegisterWithArrangementAndIndex(V0+Reg(Rt),ArrangementS,uint8(index),2);
        goto __switch_break0;
    }
    if (aop == arg_Vt_2_arrangement_size_Q___8B_00__16B_01__4H_10__8H_11__2S_20__4S_21__1D_30__2D_31)
    {
        Rt = x & (1 << 5 - 1);
        Q = (x >> 30) & 1;
        size = (x >> 10) & 3;
        if (size == 0 && Q == 0) {
            return new RegisterWithArrangement(V0+Reg(Rt),Arrangement8B,2);
        }
        else if (size == 0 && Q == 1) {
            return new RegisterWithArrangement(V0+Reg(Rt),Arrangement16B,2);
        }
        else if (size == 1 && Q == 0) {
            return new RegisterWithArrangement(V0+Reg(Rt),Arrangement4H,2);
        }
        else if (size == 1 && Q == 1) {
            return new RegisterWithArrangement(V0+Reg(Rt),Arrangement8H,2);
        }
        else if (size == 2 && Q == 0) {
            return new RegisterWithArrangement(V0+Reg(Rt),Arrangement2S,2);
        }
        else if (size == 2 && Q == 1) {
            return new RegisterWithArrangement(V0+Reg(Rt),Arrangement4S,2);
        }
        else if (size == 3 && Q == 0) {
            return new RegisterWithArrangement(V0+Reg(Rt),Arrangement1D,2);
        }
        else
 {
            return new RegisterWithArrangement(V0+Reg(Rt),Arrangement2D,2);
        }
        goto __switch_break0;
    }
    if (aop == arg_Vt_2_arrangement_size_Q___8B_00__16B_01__4H_10__8H_11__2S_20__4S_21__2D_31)
    {
        Rt = x & (1 << 5 - 1);
        Q = (x >> 30) & 1;
        size = (x >> 10) & 3;
        if (size == 0 && Q == 0) {
            return new RegisterWithArrangement(V0+Reg(Rt),Arrangement8B,2);
        }
        else if (size == 0 && Q == 1) {
            return new RegisterWithArrangement(V0+Reg(Rt),Arrangement16B,2);
        }
        else if (size == 1 && Q == 0) {
            return new RegisterWithArrangement(V0+Reg(Rt),Arrangement4H,2);
        }
        else if (size == 1 && Q == 1) {
            return new RegisterWithArrangement(V0+Reg(Rt),Arrangement8H,2);
        }
        else if (size == 2 && Q == 0) {
            return new RegisterWithArrangement(V0+Reg(Rt),Arrangement2S,2);
        }
        else if (size == 2 && Q == 1) {
            return new RegisterWithArrangement(V0+Reg(Rt),Arrangement4S,2);
        }
        else if (size == 3 && Q == 1) {
            return new RegisterWithArrangement(V0+Reg(Rt),Arrangement2D,2);
        }
        return null;
        goto __switch_break0;
    }
    if (aop == arg_Vt_3_arrangement_B_index__Q_S_size_1)
    {
        Rt = x & (1 << 5 - 1);
        Q = (x >> 30) & 1;
        S = (x >> 12) & 1;
        size = (x >> 10) & 3;
        index = (Q << 3) | (S << 2) | (size);
        return new RegisterWithArrangementAndIndex(V0+Reg(Rt),ArrangementB,uint8(index),3);
        goto __switch_break0;
    }
    if (aop == arg_Vt_3_arrangement_D_index__Q_1)
    {
        Rt = x & (1 << 5 - 1);
        index = (x >> 30) & 1;
        return new RegisterWithArrangementAndIndex(V0+Reg(Rt),ArrangementD,uint8(index),3);
        goto __switch_break0;
    }
    if (aop == arg_Vt_3_arrangement_H_index__Q_S_size_1)
    {
        Rt = x & (1 << 5 - 1);
        Q = (x >> 30) & 1;
        S = (x >> 12) & 1;
        size = (x >> 11) & 1;
        index = (Q << 2) | (S << 1) | (size);
        return new RegisterWithArrangementAndIndex(V0+Reg(Rt),ArrangementH,uint8(index),3);
        goto __switch_break0;
    }
    if (aop == arg_Vt_3_arrangement_S_index__Q_S_1)
    {
        Rt = x & (1 << 5 - 1);
        Q = (x >> 30) & 1;
        S = (x >> 12) & 1;
        index = (Q << 1) | S;
        return new RegisterWithArrangementAndIndex(V0+Reg(Rt),ArrangementS,uint8(index),3);
        goto __switch_break0;
    }
    if (aop == arg_Vt_3_arrangement_size_Q___8B_00__16B_01__4H_10__8H_11__2S_20__4S_21__1D_30__2D_31)
    {
        Rt = x & (1 << 5 - 1);
        Q = (x >> 30) & 1;
        size = (x >> 10) & 3;
        if (size == 0 && Q == 0) {
            return new RegisterWithArrangement(V0+Reg(Rt),Arrangement8B,3);
        }
        else if (size == 0 && Q == 1) {
            return new RegisterWithArrangement(V0+Reg(Rt),Arrangement16B,3);
        }
        else if (size == 1 && Q == 0) {
            return new RegisterWithArrangement(V0+Reg(Rt),Arrangement4H,3);
        }
        else if (size == 1 && Q == 1) {
            return new RegisterWithArrangement(V0+Reg(Rt),Arrangement8H,3);
        }
        else if (size == 2 && Q == 0) {
            return new RegisterWithArrangement(V0+Reg(Rt),Arrangement2S,3);
        }
        else if (size == 2 && Q == 1) {
            return new RegisterWithArrangement(V0+Reg(Rt),Arrangement4S,3);
        }
        else if (size == 3 && Q == 0) {
            return new RegisterWithArrangement(V0+Reg(Rt),Arrangement1D,3);
        }
        else
 {
            return new RegisterWithArrangement(V0+Reg(Rt),Arrangement2D,3);
        }
        goto __switch_break0;
    }
    if (aop == arg_Vt_3_arrangement_size_Q___8B_00__16B_01__4H_10__8H_11__2S_20__4S_21__2D_31)
    {
        Rt = x & (1 << 5 - 1);
        Q = (x >> 30) & 1;
        size = (x >> 10) & 3;
        if (size == 0 && Q == 0) {
            return new RegisterWithArrangement(V0+Reg(Rt),Arrangement8B,3);
        }
        else if (size == 0 && Q == 1) {
            return new RegisterWithArrangement(V0+Reg(Rt),Arrangement16B,3);
        }
        else if (size == 1 && Q == 0) {
            return new RegisterWithArrangement(V0+Reg(Rt),Arrangement4H,3);
        }
        else if (size == 1 && Q == 1) {
            return new RegisterWithArrangement(V0+Reg(Rt),Arrangement8H,3);
        }
        else if (size == 2 && Q == 0) {
            return new RegisterWithArrangement(V0+Reg(Rt),Arrangement2S,3);
        }
        else if (size == 2 && Q == 1) {
            return new RegisterWithArrangement(V0+Reg(Rt),Arrangement4S,3);
        }
        else if (size == 3 && Q == 1) {
            return new RegisterWithArrangement(V0+Reg(Rt),Arrangement2D,3);
        }
        return null;
        goto __switch_break0;
    }
    if (aop == arg_Vt_4_arrangement_B_index__Q_S_size_1)
    {
        Rt = x & (1 << 5 - 1);
        Q = (x >> 30) & 1;
        S = (x >> 12) & 1;
        size = (x >> 10) & 3;
        index = (Q << 3) | (S << 2) | (size);
        return new RegisterWithArrangementAndIndex(V0+Reg(Rt),ArrangementB,uint8(index),4);
        goto __switch_break0;
    }
    if (aop == arg_Vt_4_arrangement_D_index__Q_1)
    {
        Rt = x & (1 << 5 - 1);
        index = (x >> 30) & 1;
        return new RegisterWithArrangementAndIndex(V0+Reg(Rt),ArrangementD,uint8(index),4);
        goto __switch_break0;
    }
    if (aop == arg_Vt_4_arrangement_H_index__Q_S_size_1)
    {
        Rt = x & (1 << 5 - 1);
        Q = (x >> 30) & 1;
        S = (x >> 12) & 1;
        size = (x >> 11) & 1;
        index = (Q << 2) | (S << 1) | (size);
        return new RegisterWithArrangementAndIndex(V0+Reg(Rt),ArrangementH,uint8(index),4);
        goto __switch_break0;
    }
    if (aop == arg_Vt_4_arrangement_S_index__Q_S_1)
    {
        Rt = x & (1 << 5 - 1);
        Q = (x >> 30) & 1;
        S = (x >> 12) & 1;
        index = (Q << 1) | S;
        return new RegisterWithArrangementAndIndex(V0+Reg(Rt),ArrangementS,uint8(index),4);
        goto __switch_break0;
    }
    if (aop == arg_Vt_4_arrangement_size_Q___8B_00__16B_01__4H_10__8H_11__2S_20__4S_21__1D_30__2D_31)
    {
        Rt = x & (1 << 5 - 1);
        Q = (x >> 30) & 1;
        size = (x >> 10) & 3;
        if (size == 0 && Q == 0) {
            return new RegisterWithArrangement(V0+Reg(Rt),Arrangement8B,4);
        }
        else if (size == 0 && Q == 1) {
            return new RegisterWithArrangement(V0+Reg(Rt),Arrangement16B,4);
        }
        else if (size == 1 && Q == 0) {
            return new RegisterWithArrangement(V0+Reg(Rt),Arrangement4H,4);
        }
        else if (size == 1 && Q == 1) {
            return new RegisterWithArrangement(V0+Reg(Rt),Arrangement8H,4);
        }
        else if (size == 2 && Q == 0) {
            return new RegisterWithArrangement(V0+Reg(Rt),Arrangement2S,4);
        }
        else if (size == 2 && Q == 1) {
            return new RegisterWithArrangement(V0+Reg(Rt),Arrangement4S,4);
        }
        else if (size == 3 && Q == 0) {
            return new RegisterWithArrangement(V0+Reg(Rt),Arrangement1D,4);
        }
        else
 {
            return new RegisterWithArrangement(V0+Reg(Rt),Arrangement2D,4);
        }
        goto __switch_break0;
    }
    if (aop == arg_Vt_4_arrangement_size_Q___8B_00__16B_01__4H_10__8H_11__2S_20__4S_21__2D_31)
    {
        Rt = x & (1 << 5 - 1);
        Q = (x >> 30) & 1;
        size = (x >> 10) & 3;
        if (size == 0 && Q == 0) {
            return new RegisterWithArrangement(V0+Reg(Rt),Arrangement8B,4);
        }
        else if (size == 0 && Q == 1) {
            return new RegisterWithArrangement(V0+Reg(Rt),Arrangement16B,4);
        }
        else if (size == 1 && Q == 0) {
            return new RegisterWithArrangement(V0+Reg(Rt),Arrangement4H,4);
        }
        else if (size == 1 && Q == 1) {
            return new RegisterWithArrangement(V0+Reg(Rt),Arrangement8H,4);
        }
        else if (size == 2 && Q == 0) {
            return new RegisterWithArrangement(V0+Reg(Rt),Arrangement2S,4);
        }
        else if (size == 2 && Q == 1) {
            return new RegisterWithArrangement(V0+Reg(Rt),Arrangement4S,4);
        }
        else if (size == 3 && Q == 1) {
            return new RegisterWithArrangement(V0+Reg(Rt),Arrangement2D,4);
        }
        return null;
        goto __switch_break0;
    }
    if (aop == arg_Xns_mem_extend_m__UXTW_2__LSL_3__SXTW_6__SXTX_7__0_0__4_1)
    {
        return handle_MemExtend(x, 4, false);
        goto __switch_break0;
    }
    if (aop == arg_Xns_mem_offset)
    {
        Rn = RegSP(X0) + RegSP(x >> 5 & (1 << 5 - 1));
        return new MemImmediate(Rn,AddrOffset,0);
        goto __switch_break0;
    }
    if (aop == arg_Xns_mem_optional_imm12_16_unsigned)
    {
        Rn = RegSP(X0) + RegSP(x >> 5 & (1 << 5 - 1));
        imm12 = (x >> 10) & (1 << 12 - 1);
        return new MemImmediate(Rn,AddrOffset,int32(imm12<<4));
        goto __switch_break0;
    }
    if (aop == arg_Xns_mem_optional_imm7_16_signed)
    {
        Rn = RegSP(X0) + RegSP(x >> 5 & (1 << 5 - 1));
        imm7 = (x >> 15) & (1 << 7 - 1);
        return new MemImmediate(Rn,AddrOffset,((int32(imm7<<4))<<21)>>21);
        goto __switch_break0;
    }
    if (aop == arg_Xns_mem_post_fixedimm_1)
    {
        Rn = RegSP(X0) + RegSP(x >> 5 & (1 << 5 - 1));
        return new MemImmediate(Rn,AddrPostIndex,1);
        goto __switch_break0;
    }
    if (aop == arg_Xns_mem_post_fixedimm_12)
    {
        Rn = RegSP(X0) + RegSP(x >> 5 & (1 << 5 - 1));
        return new MemImmediate(Rn,AddrPostIndex,12);
        goto __switch_break0;
    }
    if (aop == arg_Xns_mem_post_fixedimm_16)
    {
        Rn = RegSP(X0) + RegSP(x >> 5 & (1 << 5 - 1));
        return new MemImmediate(Rn,AddrPostIndex,16);
        goto __switch_break0;
    }
    if (aop == arg_Xns_mem_post_fixedimm_2)
    {
        Rn = RegSP(X0) + RegSP(x >> 5 & (1 << 5 - 1));
        return new MemImmediate(Rn,AddrPostIndex,2);
        goto __switch_break0;
    }
    if (aop == arg_Xns_mem_post_fixedimm_24)
    {
        Rn = RegSP(X0) + RegSP(x >> 5 & (1 << 5 - 1));
        return new MemImmediate(Rn,AddrPostIndex,24);
        goto __switch_break0;
    }
    if (aop == arg_Xns_mem_post_fixedimm_3)
    {
        Rn = RegSP(X0) + RegSP(x >> 5 & (1 << 5 - 1));
        return new MemImmediate(Rn,AddrPostIndex,3);
        goto __switch_break0;
    }
    if (aop == arg_Xns_mem_post_fixedimm_32)
    {
        Rn = RegSP(X0) + RegSP(x >> 5 & (1 << 5 - 1));
        return new MemImmediate(Rn,AddrPostIndex,32);
        goto __switch_break0;
    }
    if (aop == arg_Xns_mem_post_fixedimm_4)
    {
        Rn = RegSP(X0) + RegSP(x >> 5 & (1 << 5 - 1));
        return new MemImmediate(Rn,AddrPostIndex,4);
        goto __switch_break0;
    }
    if (aop == arg_Xns_mem_post_fixedimm_6)
    {
        Rn = RegSP(X0) + RegSP(x >> 5 & (1 << 5 - 1));
        return new MemImmediate(Rn,AddrPostIndex,6);
        goto __switch_break0;
    }
    if (aop == arg_Xns_mem_post_fixedimm_8)
    {
        Rn = RegSP(X0) + RegSP(x >> 5 & (1 << 5 - 1));
        return new MemImmediate(Rn,AddrPostIndex,8);
        goto __switch_break0;
    }
    if (aop == arg_Xns_mem_post_imm7_16_signed)
    {
        Rn = RegSP(X0) + RegSP(x >> 5 & (1 << 5 - 1));
        imm7 = (x >> 15) & (1 << 7 - 1);
        return new MemImmediate(Rn,AddrPostIndex,((int32(imm7<<4))<<21)>>21);
        goto __switch_break0;
    }
    if (aop == arg_Xns_mem_post_Q__16_0__32_1)
    {
        Rn = RegSP(X0) + RegSP(x >> 5 & (1 << 5 - 1));
        Q = (x >> 30) & 1;
        return new MemImmediate(Rn,AddrPostIndex,int32((Q+1)*16));
        goto __switch_break0;
    }
    if (aop == arg_Xns_mem_post_Q__24_0__48_1)
    {
        Rn = RegSP(X0) + RegSP(x >> 5 & (1 << 5 - 1));
        Q = (x >> 30) & 1;
        return new MemImmediate(Rn,AddrPostIndex,int32((Q+1)*24));
        goto __switch_break0;
    }
    if (aop == arg_Xns_mem_post_Q__32_0__64_1)
    {
        Rn = RegSP(X0) + RegSP(x >> 5 & (1 << 5 - 1));
        Q = (x >> 30) & 1;
        return new MemImmediate(Rn,AddrPostIndex,int32((Q+1)*32));
        goto __switch_break0;
    }
    if (aop == arg_Xns_mem_post_Q__8_0__16_1)
    {
        Rn = RegSP(X0) + RegSP(x >> 5 & (1 << 5 - 1));
        Q = (x >> 30) & 1;
        return new MemImmediate(Rn,AddrPostIndex,int32((Q+1)*8));
        goto __switch_break0;
    }
    if (aop == arg_Xns_mem_post_size__1_0__2_1__4_2__8_3)
    {
        Rn = RegSP(X0) + RegSP(x >> 5 & (1 << 5 - 1));
        size = (x >> 10) & 3;
        return new MemImmediate(Rn,AddrPostIndex,int32(1<<size));
        goto __switch_break0;
    }
    if (aop == arg_Xns_mem_post_size__2_0__4_1__8_2__16_3)
    {
        Rn = RegSP(X0) + RegSP(x >> 5 & (1 << 5 - 1));
        size = (x >> 10) & 3;
        return new MemImmediate(Rn,AddrPostIndex,int32(2<<size));
        goto __switch_break0;
    }
    if (aop == arg_Xns_mem_post_size__3_0__6_1__12_2__24_3)
    {
        Rn = RegSP(X0) + RegSP(x >> 5 & (1 << 5 - 1));
        size = (x >> 10) & 3;
        return new MemImmediate(Rn,AddrPostIndex,int32(3<<size));
        goto __switch_break0;
    }
    if (aop == arg_Xns_mem_post_size__4_0__8_1__16_2__32_3)
    {
        Rn = RegSP(X0) + RegSP(x >> 5 & (1 << 5 - 1));
        size = (x >> 10) & 3;
        return new MemImmediate(Rn,AddrPostIndex,int32(4<<size));
        goto __switch_break0;
    }
    if (aop == arg_Xns_mem_post_Xm)
    {
        Rn = RegSP(X0) + RegSP(x >> 5 & (1 << 5 - 1));
        Rm = (x >> 16) & (1 << 5 - 1);
        return new MemImmediate(Rn,AddrPostReg,int32(Rm));
        goto __switch_break0;
    }
    if (aop == arg_Xns_mem_wb_imm7_16_signed)
    {
        Rn = RegSP(X0) + RegSP(x >> 5 & (1 << 5 - 1));
        imm7 = (x >> 15) & (1 << 7 - 1);
        return new MemImmediate(Rn,AddrPreIndex,((int32(imm7<<4))<<21)>>21);
        goto __switch_break0;
    }
    // default: 
        return null;

    __switch_break0:;

}

private static Arg handle_ExtendedRegister(uint x, bool has_width) {
    var s = (x >> 29) & 1;
    var rm = (x >> 16) & (1 << 5 - 1);
    var option = (x >> 13) & (1 << 3 - 1);
    var imm3 = (x >> 10) & (1 << 3 - 1);
    var rn = (x >> 5) & (1 << 5 - 1);
    var rd = x & (1 << 5 - 1);
    var is_32bit = !has_width;
    RegExtshiftAmount rea = default;
    if (has_width) {
        if (option & 0x3 != 0x3) {
            rea.reg = W0 + Reg(rm);
        }
        else
 {
            rea.reg = X0 + Reg(rm);
        }
    }
    else
 {
        rea.reg = W0 + Reg(rm);
    }
    switch (option) {
        case 0: 
            rea.extShift = uxtb;
            break;
        case 1: 
            rea.extShift = uxth;
            break;
        case 2: 
                   if (is_32bit && (rn == 31 || (s == 0 && rd == 31))) {
                       if (imm3 != 0) {
                           rea.extShift = lsl;
                       }
                       else
            {
                           rea.extShift = ExtShift(0);
                       }

                   }
                   else
            {
                       rea.extShift = uxtw;
                   }
            break;
        case 3: 
                   if (!is_32bit && (rn == 31 || (s == 0 && rd == 31))) {
                       if (imm3 != 0) {
                           rea.extShift = lsl;
                       }
                       else
            {
                           rea.extShift = ExtShift(0);
                       }

                   }
                   else
            {
                       rea.extShift = uxtx;
                   }
            break;
        case 4: 
            rea.extShift = sxtb;
            break;
        case 5: 
            rea.extShift = sxth;
            break;
        case 6: 
            rea.extShift = sxtw;
            break;
        case 7: 
            rea.extShift = sxtx;
            break;
    }
    rea.show_zero = false;
    rea.amount = uint8(imm3);
    return rea;

}

private static Arg handle_ImmediateShiftedRegister(uint x, byte max, bool is_w, bool has_ror) {
    RegExtshiftAmount rsa = default;
    if (is_w) {
        rsa.reg = W0 + Reg((x >> 16) & (1 << 5 - 1));
    }
    else
 {
        rsa.reg = X0 + Reg((x >> 16) & (1 << 5 - 1));
    }
    switch ((x >> 22) & 0x3) {
        case 0: 
            rsa.extShift = lsl;
            break;
        case 1: 
            rsa.extShift = lsr;
            break;
        case 2: 
            rsa.extShift = asr;
            break;
        case 3: 
                   if (has_ror) {
                       rsa.extShift = ror;
                   }
                   else
            {
                       return null;
                   }
            break;
    }
    rsa.show_zero = true;
    rsa.amount = uint8((x >> 10) & (1 << 6 - 1));
    if (rsa.amount == 0 && rsa.extShift == lsl) {
        rsa.extShift = ExtShift(0);
    }
    else if (rsa.amount > max) {
        return null;
    }
    return rsa;

}

private static Arg handle_MemExtend(uint x, byte mult, bool absent) {
    ExtShift extend = default;
    Reg Rm = default;
    var option = (x >> 13) & (1 << 3 - 1);
    var Rn = RegSP(X0) + RegSP(x >> 5 & (1 << 5 - 1));
    if ((option & 1) != 0) {
        Rm = Reg(X0) + Reg(x >> 16 & (1 << 5 - 1));
    }
    else
 {
        Rm = Reg(W0) + Reg(x >> 16 & (1 << 5 - 1));
    }
    switch (option) {
        case 2: 
            extend = uxtw;
            break;
        case 3: 
            extend = lsl;
            break;
        case 6: 
            extend = sxtw;
            break;
        case 7: 
            extend = sxtx;
            break;
        default: 
            return null;
            break;
    }
    var amount = (uint8((x >> 12) & 1)) * mult;
    return new MemExtend(Rn,Rm,extend,amount,absent);

}

private static Arg handle_bitmasks(uint x, byte datasize) {
    byte length = default;    byte levels = default;    byte esize = default;    byte i = default;

    ulong welem = default;    ulong wmask = default;

    var n = (x >> 22) & 1;
    var imms = uint8((x >> 10) & (1 << 6 - 1));
    var immr = uint8((x >> 16) & (1 << 6 - 1));
    if (n != 0) {
        length = 6;
    }
    else if ((imms & 32) == 0) {
        length = 5;
    }
    else if ((imms & 16) == 0) {
        length = 4;
    }
    else if ((imms & 8) == 0) {
        length = 3;
    }
    else if ((imms & 4) == 0) {
        length = 2;
    }
    else if ((imms & 2) == 0) {
        length = 1;
    }
    else
 {
        return null;
    }
    levels = 1 << (int)(length) - 1;
    var s = imms & levels;
    var r = immr & levels;
    esize = 1 << (int)(length);
    if (esize > datasize) {
        return null;
    }
    welem = 1 << (int)((s + 1)) - 1;
    var ror = (welem >> (int)(r)) | (welem << (int)((esize - r)));
    ror &= ((1 << (int)(esize)) - 1);
    wmask = 0;
    i = 0;

    while (i < datasize) {
        wmask = (wmask << (int)(esize)) | ror;
        i += esize;
    }
    return new Imm64(wmask,false);

}

} // end arm64asm_package
