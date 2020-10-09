// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package arm64asm -- go2cs converted at 2020 October 09 05:54:44 UTC
// import "cmd/vendor/golang.org/x/arch/arm64/arm64asm" ==> using arm64asm = go.cmd.vendor.golang.org.x.arch.arm64.arm64asm_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\arch\arm64\arm64asm\decode.go
using binary = go.encoding.binary_package;
using fmt = go.fmt_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace vendor {
namespace golang.org {
namespace x {
namespace arch {
namespace arm64
{
    public static partial class arm64asm_package
    {
        private partial struct instArgs // : array<instArg>
        {
        }

        // An instFormat describes the format of an instruction encoding.
        // An instruction with 32-bit value x matches the format if x&mask == value
        // and the predicator: canDecode(x) return true.
        private partial struct instFormat
        {
            public uint mask;
            public uint value;
            public Op op; // args describe how to decode the instruction arguments.
// args is stored as a fixed-size array.
// if there are fewer than len(args) arguments, args[i] == 0 marks
// the end of the argument list.
            public instArgs args;
            public Func<uint, bool> canDecode;
        }

        private static var errShort = fmt.Errorf("truncated instruction");        private static var errUnknown = fmt.Errorf("unknown instruction");

        private static slice<bool> decoderCover = default;

        private static void init()
        {
            decoderCover = make_slice<bool>(len(instFormats));
        }

        // Decode decodes the 4 bytes in src as a single instruction.
        public static (Inst, error) Decode(slice<byte> src)
        {
            Inst inst = default;
            error err = default!;

            if (len(src) < 4L)
            {
                return (new Inst(), error.As(errShort)!);
            }

            var x = binary.LittleEndian.Uint32(src);

Search:
            foreach (var (i) in instFormats)
            {
                var f = _addr_instFormats[i];
                if (x & f.mask != f.value)
                {
                    continue;
                }

                if (f.canDecode != null && !f.canDecode(x))
                {
                    continue;
                } 
                // Decode args.
                Args args = default;
                foreach (var (j, aop) in f.args)
                {
                    if (aop == 0L)
                    {
                        break;
                    }

                    var arg = decodeArg(aop, x);
                    if (arg == null)
                    { // Cannot decode argument
                        _continueSearch = true;
                        break;
                    }

                    args[j] = arg;

                }
                decoderCover[i] = true;
                inst = new Inst(Op:f.op,Args:args,Enc:x,);
                return (inst, error.As(null!)!);

            }
            return (new Inst(), error.As(errUnknown)!);

        }

        // decodeArg decodes the arg described by aop from the instruction bits x.
        // It returns nil if x cannot be decoded according to aop.
        private static Arg decodeArg(instArg aop, uint x)
        {

            if (aop == arg_Da)
            {
                return D0 + Reg((x >> (int)(10L)) & (1L << (int)(5L) - 1L));
                goto __switch_break0;
            }
            if (aop == arg_Dd)
            {
                return D0 + Reg(x & (1L << (int)(5L) - 1L));
                goto __switch_break0;
            }
            if (aop == arg_Dm)
            {
                return D0 + Reg((x >> (int)(16L)) & (1L << (int)(5L) - 1L));
                goto __switch_break0;
            }
            if (aop == arg_Dn)
            {
                return D0 + Reg((x >> (int)(5L)) & (1L << (int)(5L) - 1L));
                goto __switch_break0;
            }
            if (aop == arg_Hd)
            {
                return H0 + Reg(x & (1L << (int)(5L) - 1L));
                goto __switch_break0;
            }
            if (aop == arg_Hn)
            {
                return H0 + Reg((x >> (int)(5L)) & (1L << (int)(5L) - 1L));
                goto __switch_break0;
            }
            if (aop == arg_IAddSub)
            {
                var imm12 = (x >> (int)(10L)) & (1L << (int)(12L) - 1L);
                var shift = (x >> (int)(22L)) & (1L << (int)(2L) - 1L);
                if (shift > 1L)
                {
                    return null;
                }

                shift = shift * 12L;
                return new ImmShift(uint16(imm12),uint8(shift));
                goto __switch_break0;
            }
            if (aop == arg_Sa)
            {
                return S0 + Reg((x >> (int)(10L)) & (1L << (int)(5L) - 1L));
                goto __switch_break0;
            }
            if (aop == arg_Sd)
            {
                return S0 + Reg(x & (1L << (int)(5L) - 1L));
                goto __switch_break0;
            }
            if (aop == arg_Sm)
            {
                return S0 + Reg((x >> (int)(16L)) & (1L << (int)(5L) - 1L));
                goto __switch_break0;
            }
            if (aop == arg_Sn)
            {
                return S0 + Reg((x >> (int)(5L)) & (1L << (int)(5L) - 1L));
                goto __switch_break0;
            }
            if (aop == arg_Wa)
            {
                return W0 + Reg((x >> (int)(10L)) & (1L << (int)(5L) - 1L));
                goto __switch_break0;
            }
            if (aop == arg_Wd)
            {
                return W0 + Reg(x & (1L << (int)(5L) - 1L));
                goto __switch_break0;
            }
            if (aop == arg_Wds)
            {
                return RegSP(W0) + RegSP(x & (1L << (int)(5L) - 1L));
                goto __switch_break0;
            }
            if (aop == arg_Wm)
            {
                return W0 + Reg((x >> (int)(16L)) & (1L << (int)(5L) - 1L));
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
                return W0 + Reg((x >> (int)(5L)) & (1L << (int)(5L) - 1L));
                goto __switch_break0;
            }
            if (aop == arg_Wns)
            {
                return RegSP(W0) + RegSP((x >> (int)(5L)) & (1L << (int)(5L) - 1L));
                goto __switch_break0;
            }
            if (aop == arg_Xa)
            {
                return X0 + Reg((x >> (int)(10L)) & (1L << (int)(5L) - 1L));
                goto __switch_break0;
            }
            if (aop == arg_Xd)
            {
                return X0 + Reg(x & (1L << (int)(5L) - 1L));
                goto __switch_break0;
            }
            if (aop == arg_Xds)
            {
                return RegSP(X0) + RegSP(x & (1L << (int)(5L) - 1L));
                goto __switch_break0;
            }
            if (aop == arg_Xm)
            {
                return X0 + Reg((x >> (int)(16L)) & (1L << (int)(5L) - 1L));
                goto __switch_break0;
            }
            if (aop == arg_Wm_shift__LSL_0__LSR_1__ASR_2__0_31)
            {
                return handle_ImmediateShiftedRegister(x, 31L, true, false);
                goto __switch_break0;
            }
            if (aop == arg_Wm_shift__LSL_0__LSR_1__ASR_2__ROR_3__0_31)
            {
                return handle_ImmediateShiftedRegister(x, 31L, true, true);
                goto __switch_break0;
            }
            if (aop == arg_Xm_shift__LSL_0__LSR_1__ASR_2__0_63)
            {
                return handle_ImmediateShiftedRegister(x, 63L, false, false);
                goto __switch_break0;
            }
            if (aop == arg_Xm_shift__LSL_0__LSR_1__ASR_2__ROR_3__0_63)
            {
                return handle_ImmediateShiftedRegister(x, 63L, false, true);
                goto __switch_break0;
            }
            if (aop == arg_Xn)
            {
                return X0 + Reg((x >> (int)(5L)) & (1L << (int)(5L) - 1L));
                goto __switch_break0;
            }
            if (aop == arg_Xns)
            {
                return RegSP(X0) + RegSP((x >> (int)(5L)) & (1L << (int)(5L) - 1L));
                goto __switch_break0;
            }
            if (aop == arg_slabel_imm14_2)
            {
                var imm14 = ((x >> (int)(5L)) & (1L << (int)(14L) - 1L));
                return PCRel(((int64(imm14) << (int)(2L)) << (int)(48L)) >> (int)(48L));
                goto __switch_break0;
            }
            if (aop == arg_slabel_imm19_2)
            {
                var imm19 = ((x >> (int)(5L)) & (1L << (int)(19L) - 1L));
                return PCRel(((int64(imm19) << (int)(2L)) << (int)(43L)) >> (int)(43L));
                goto __switch_break0;
            }
            if (aop == arg_slabel_imm26_2)
            {
                var imm26 = (x & (1L << (int)(26L) - 1L));
                return PCRel(((int64(imm26) << (int)(2L)) << (int)(36L)) >> (int)(36L));
                goto __switch_break0;
            }
            if (aop == arg_slabel_immhi_immlo_0)
            {
                var immhi = ((x >> (int)(5L)) & (1L << (int)(19L) - 1L));
                var immlo = ((x >> (int)(29L)) & (1L << (int)(2L) - 1L));
                var immhilo = (immhi) << (int)(2L) | immlo;
                return PCRel((int64(immhilo) << (int)(43L)) >> (int)(43L));
                goto __switch_break0;
            }
            if (aop == arg_slabel_immhi_immlo_12)
            {
                immhi = ((x >> (int)(5L)) & (1L << (int)(19L) - 1L));
                immlo = ((x >> (int)(29L)) & (1L << (int)(2L) - 1L));
                immhilo = (immhi) << (int)(2L) | immlo;
                return PCRel(((int64(immhilo) << (int)(12L)) << (int)(31L)) >> (int)(31L));
                goto __switch_break0;
            }
            if (aop == arg_Xns_mem)
            {
                var Rn = RegSP(X0) + RegSP(x >> (int)(5L) & (1L << (int)(5L) - 1L));
                return new MemImmediate(Rn,AddrOffset,0);
                goto __switch_break0;
            }
            if (aop == arg_Xns_mem_extend_m__UXTW_2__LSL_3__SXTW_6__SXTX_7__0_0__1_1)
            {
                return handle_MemExtend(x, 1L, false);
                goto __switch_break0;
            }
            if (aop == arg_Xns_mem_extend_m__UXTW_2__LSL_3__SXTW_6__SXTX_7__0_0__2_1)
            {
                return handle_MemExtend(x, 2L, false);
                goto __switch_break0;
            }
            if (aop == arg_Xns_mem_extend_m__UXTW_2__LSL_3__SXTW_6__SXTX_7__0_0__3_1)
            {
                return handle_MemExtend(x, 3L, false);
                goto __switch_break0;
            }
            if (aop == arg_Xns_mem_extend_m__UXTW_2__LSL_3__SXTW_6__SXTX_7__absent_0__0_1)
            {
                return handle_MemExtend(x, 1L, true);
                goto __switch_break0;
            }
            if (aop == arg_Xns_mem_optional_imm12_1_unsigned)
            {
                Rn = RegSP(X0) + RegSP(x >> (int)(5L) & (1L << (int)(5L) - 1L));
                imm12 = (x >> (int)(10L)) & (1L << (int)(12L) - 1L);
                return new MemImmediate(Rn,AddrOffset,int32(imm12));
                goto __switch_break0;
            }
            if (aop == arg_Xns_mem_optional_imm12_2_unsigned)
            {
                Rn = RegSP(X0) + RegSP(x >> (int)(5L) & (1L << (int)(5L) - 1L));
                imm12 = (x >> (int)(10L)) & (1L << (int)(12L) - 1L);
                return new MemImmediate(Rn,AddrOffset,int32(imm12<<1));
                goto __switch_break0;
            }
            if (aop == arg_Xns_mem_optional_imm12_4_unsigned)
            {
                Rn = RegSP(X0) + RegSP(x >> (int)(5L) & (1L << (int)(5L) - 1L));
                imm12 = (x >> (int)(10L)) & (1L << (int)(12L) - 1L);
                return new MemImmediate(Rn,AddrOffset,int32(imm12<<2));
                goto __switch_break0;
            }
            if (aop == arg_Xns_mem_optional_imm12_8_unsigned)
            {
                Rn = RegSP(X0) + RegSP(x >> (int)(5L) & (1L << (int)(5L) - 1L));
                imm12 = (x >> (int)(10L)) & (1L << (int)(12L) - 1L);
                return new MemImmediate(Rn,AddrOffset,int32(imm12<<3));
                goto __switch_break0;
            }
            if (aop == arg_Xns_mem_optional_imm7_4_signed)
            {
                Rn = RegSP(X0) + RegSP(x >> (int)(5L) & (1L << (int)(5L) - 1L));
                var imm7 = (x >> (int)(15L)) & (1L << (int)(7L) - 1L);
                return new MemImmediate(Rn,AddrOffset,((int32(imm7<<2))<<23)>>23);
                goto __switch_break0;
            }
            if (aop == arg_Xns_mem_optional_imm7_8_signed)
            {
                Rn = RegSP(X0) + RegSP(x >> (int)(5L) & (1L << (int)(5L) - 1L));
                imm7 = (x >> (int)(15L)) & (1L << (int)(7L) - 1L);
                return new MemImmediate(Rn,AddrOffset,((int32(imm7<<3))<<22)>>22);
                goto __switch_break0;
            }
            if (aop == arg_Xns_mem_optional_imm9_1_signed)
            {
                Rn = RegSP(X0) + RegSP(x >> (int)(5L) & (1L << (int)(5L) - 1L));
                var imm9 = (x >> (int)(12L)) & (1L << (int)(9L) - 1L);
                return new MemImmediate(Rn,AddrOffset,(int32(imm9)<<23)>>23);
                goto __switch_break0;
            }
            if (aop == arg_Xns_mem_post_imm7_4_signed)
            {
                Rn = RegSP(X0) + RegSP(x >> (int)(5L) & (1L << (int)(5L) - 1L));
                imm7 = (x >> (int)(15L)) & (1L << (int)(7L) - 1L);
                return new MemImmediate(Rn,AddrPostIndex,((int32(imm7<<2))<<23)>>23);
                goto __switch_break0;
            }
            if (aop == arg_Xns_mem_post_imm7_8_signed)
            {
                Rn = RegSP(X0) + RegSP(x >> (int)(5L) & (1L << (int)(5L) - 1L));
                imm7 = (x >> (int)(15L)) & (1L << (int)(7L) - 1L);
                return new MemImmediate(Rn,AddrPostIndex,((int32(imm7<<3))<<22)>>22);
                goto __switch_break0;
            }
            if (aop == arg_Xns_mem_post_imm9_1_signed)
            {
                Rn = RegSP(X0) + RegSP(x >> (int)(5L) & (1L << (int)(5L) - 1L));
                imm9 = (x >> (int)(12L)) & (1L << (int)(9L) - 1L);
                return new MemImmediate(Rn,AddrPostIndex,((int32(imm9))<<23)>>23);
                goto __switch_break0;
            }
            if (aop == arg_Xns_mem_wb_imm7_4_signed)
            {
                Rn = RegSP(X0) + RegSP(x >> (int)(5L) & (1L << (int)(5L) - 1L));
                imm7 = (x >> (int)(15L)) & (1L << (int)(7L) - 1L);
                return new MemImmediate(Rn,AddrPreIndex,((int32(imm7<<2))<<23)>>23);
                goto __switch_break0;
            }
            if (aop == arg_Xns_mem_wb_imm7_8_signed)
            {
                Rn = RegSP(X0) + RegSP(x >> (int)(5L) & (1L << (int)(5L) - 1L));
                imm7 = (x >> (int)(15L)) & (1L << (int)(7L) - 1L);
                return new MemImmediate(Rn,AddrPreIndex,((int32(imm7<<3))<<22)>>22);
                goto __switch_break0;
            }
            if (aop == arg_Xns_mem_wb_imm9_1_signed)
            {
                Rn = RegSP(X0) + RegSP(x >> (int)(5L) & (1L << (int)(5L) - 1L));
                imm9 = (x >> (int)(12L)) & (1L << (int)(9L) - 1L);
                return new MemImmediate(Rn,AddrPreIndex,((int32(imm9))<<23)>>23);
                goto __switch_break0;
            }
            if (aop == arg_Ws)
            {
                return W0 + Reg((x >> (int)(16L)) & (1L << (int)(5L) - 1L));
                goto __switch_break0;
            }
            if (aop == arg_Wt)
            {
                return W0 + Reg(x & (1L << (int)(5L) - 1L));
                goto __switch_break0;
            }
            if (aop == arg_Wt2)
            {
                return W0 + Reg((x >> (int)(10L)) & (1L << (int)(5L) - 1L));
                goto __switch_break0;
            }
            if (aop == arg_Xs)
            {
                return X0 + Reg((x >> (int)(16L)) & (1L << (int)(5L) - 1L));
                goto __switch_break0;
            }
            if (aop == arg_Xt)
            {
                return X0 + Reg(x & (1L << (int)(5L) - 1L));
                goto __switch_break0;
            }
            if (aop == arg_Xt2)
            {
                return X0 + Reg((x >> (int)(10L)) & (1L << (int)(5L) - 1L));
                goto __switch_break0;
            }
            if (aop == arg_immediate_0_127_CRm_op2)
            {
                var crm_op2 = (x >> (int)(5L)) & (1L << (int)(7L) - 1L);
                return Imm_hint(crm_op2);
                goto __switch_break0;
            }
            if (aop == arg_immediate_0_15_CRm)
            {
                var crm = (x >> (int)(8L)) & (1L << (int)(4L) - 1L);
                return new Imm(crm,false);
                goto __switch_break0;
            }
            if (aop == arg_immediate_0_15_nzcv)
            {
                var nzcv = x & (1L << (int)(4L) - 1L);
                return new Imm(nzcv,false);
                goto __switch_break0;
            }
            if (aop == arg_immediate_0_31_imm5)
            {
                var imm5 = (x >> (int)(16L)) & (1L << (int)(5L) - 1L);
                return new Imm(imm5,false);
                goto __switch_break0;
            }
            if (aop == arg_immediate_0_31_immr)
            {
                var immr = (x >> (int)(16L)) & (1L << (int)(6L) - 1L);
                if (immr > 31L)
                {
                    return null;
                }

                return new Imm(immr,false);
                goto __switch_break0;
            }
            if (aop == arg_immediate_0_31_imms)
            {
                var imms = (x >> (int)(10L)) & (1L << (int)(6L) - 1L);
                if (imms > 31L)
                {
                    return null;
                }

                return new Imm(imms,true);
                goto __switch_break0;
            }
            if (aop == arg_immediate_0_63_b5_b40)
            {
                var b5 = (x >> (int)(31L)) & 1L;
                var b40 = (x >> (int)(19L)) & (1L << (int)(5L) - 1L);
                return new Imm((b5<<5)|b40,true);
                goto __switch_break0;
            }
            if (aop == arg_immediate_0_63_immr)
            {
                immr = (x >> (int)(16L)) & (1L << (int)(6L) - 1L);
                return new Imm(immr,false);
                goto __switch_break0;
            }
            if (aop == arg_immediate_0_63_imms)
            {
                imms = (x >> (int)(10L)) & (1L << (int)(6L) - 1L);
                return new Imm(imms,true);
                goto __switch_break0;
            }
            if (aop == arg_immediate_0_65535_imm16)
            {
                var imm16 = (x >> (int)(5L)) & (1L << (int)(16L) - 1L);
                return new Imm(imm16,false);
                goto __switch_break0;
            }
            if (aop == arg_immediate_0_7_op1)
            {
                var op1 = (x >> (int)(16L)) & (1L << (int)(3L) - 1L);
                return new Imm(op1,true);
                goto __switch_break0;
            }
            if (aop == arg_immediate_0_7_op2)
            {
                var op2 = (x >> (int)(5L)) & (1L << (int)(3L) - 1L);
                return new Imm(op2,true);
                goto __switch_break0;
            }
            if (aop == arg_immediate_ASR_SBFM_32M_bitfield_0_31_immr)
            {
                immr = (x >> (int)(16L)) & (1L << (int)(6L) - 1L);
                if (immr > 31L)
                {
                    return null;
                }

                return new Imm(immr,true);
                goto __switch_break0;
            }
            if (aop == arg_immediate_ASR_SBFM_64M_bitfield_0_63_immr)
            {
                immr = (x >> (int)(16L)) & (1L << (int)(6L) - 1L);
                return new Imm(immr,true);
                goto __switch_break0;
            }
            if (aop == arg_immediate_BFI_BFM_32M_bitfield_lsb_32_immr)
            {
                immr = (x >> (int)(16L)) & (1L << (int)(6L) - 1L);
                if (immr > 31L)
                {
                    return null;
                }

                return new Imm(32-immr,true);
                goto __switch_break0;
            }
            if (aop == arg_immediate_BFI_BFM_32M_bitfield_width_32_imms)
            {
                imms = (x >> (int)(10L)) & (1L << (int)(6L) - 1L);
                if (imms > 31L)
                {
                    return null;
                }

                return new Imm(imms+1,true);
                goto __switch_break0;
            }
            if (aop == arg_immediate_BFI_BFM_64M_bitfield_lsb_64_immr)
            {
                immr = (x >> (int)(16L)) & (1L << (int)(6L) - 1L);
                return new Imm(64-immr,true);
                goto __switch_break0;
            }
            if (aop == arg_immediate_BFI_BFM_64M_bitfield_width_64_imms)
            {
                imms = (x >> (int)(10L)) & (1L << (int)(6L) - 1L);
                return new Imm(imms+1,true);
                goto __switch_break0;
            }
            if (aop == arg_immediate_BFXIL_BFM_32M_bitfield_lsb_32_immr)
            {
                immr = (x >> (int)(16L)) & (1L << (int)(6L) - 1L);
                if (immr > 31L)
                {
                    return null;
                }

                return new Imm(immr,true);
                goto __switch_break0;
            }
            if (aop == arg_immediate_BFXIL_BFM_32M_bitfield_width_32_imms)
            {
                immr = (x >> (int)(16L)) & (1L << (int)(6L) - 1L);
                imms = (x >> (int)(10L)) & (1L << (int)(6L) - 1L);
                var width = imms - immr + 1L;
                if (width < 1L || width > 32L - immr)
                {
                    return null;
                }

                return new Imm(width,true);
                goto __switch_break0;
            }
            if (aop == arg_immediate_BFXIL_BFM_64M_bitfield_lsb_64_immr)
            {
                immr = (x >> (int)(16L)) & (1L << (int)(6L) - 1L);
                return new Imm(immr,true);
                goto __switch_break0;
            }
            if (aop == arg_immediate_BFXIL_BFM_64M_bitfield_width_64_imms)
            {
                immr = (x >> (int)(16L)) & (1L << (int)(6L) - 1L);
                imms = (x >> (int)(10L)) & (1L << (int)(6L) - 1L);
                width = imms - immr + 1L;
                if (width < 1L || width > 64L - immr)
                {
                    return null;
                }

                return new Imm(width,true);
                goto __switch_break0;
            }
            if (aop == arg_immediate_bitmask_32_imms_immr)
            {
                return handle_bitmasks(x, 32L);
                goto __switch_break0;
            }
            if (aop == arg_immediate_bitmask_64_N_imms_immr)
            {
                return handle_bitmasks(x, 64L);
                goto __switch_break0;
            }
            if (aop == arg_immediate_LSL_UBFM_32M_bitfield_0_31_immr)
            {
                imms = (x >> (int)(10L)) & (1L << (int)(6L) - 1L);
                shift = 31L - imms;
                if (shift > 31L)
                {
                    return null;
                }

                return new Imm(shift,true);
                goto __switch_break0;
            }
            if (aop == arg_immediate_LSL_UBFM_64M_bitfield_0_63_immr)
            {
                imms = (x >> (int)(10L)) & (1L << (int)(6L) - 1L);
                shift = 63L - imms;
                if (shift > 63L)
                {
                    return null;
                }

                return new Imm(shift,true);
                goto __switch_break0;
            }
            if (aop == arg_immediate_LSR_UBFM_32M_bitfield_0_31_immr)
            {
                immr = (x >> (int)(16L)) & (1L << (int)(6L) - 1L);
                if (immr > 31L)
                {
                    return null;
                }

                return new Imm(immr,true);
                goto __switch_break0;
            }
            if (aop == arg_immediate_LSR_UBFM_64M_bitfield_0_63_immr)
            {
                immr = (x >> (int)(16L)) & (1L << (int)(6L) - 1L);
                return new Imm(immr,true);
                goto __switch_break0;
            }
            if (aop == arg_immediate_optional_0_15_CRm)
            {
                crm = (x >> (int)(8L)) & (1L << (int)(4L) - 1L);
                return Imm_clrex(crm);
                goto __switch_break0;
            }
            if (aop == arg_immediate_optional_0_65535_imm16)
            {
                imm16 = (x >> (int)(5L)) & (1L << (int)(16L) - 1L);
                return Imm_dcps(imm16);
                goto __switch_break0;
            }
            if (aop == arg_immediate_OptLSL_amount_16_0_16)
            {
                imm16 = (x >> (int)(5L)) & (1L << (int)(16L) - 1L);
                var hw = (x >> (int)(21L)) & (1L << (int)(2L) - 1L);
                shift = hw * 16L;
                if (shift > 16L)
                {
                    return null;
                }

                return new ImmShift(uint16(imm16),uint8(shift));
                goto __switch_break0;
            }
            if (aop == arg_immediate_OptLSL_amount_16_0_48)
            {
                imm16 = (x >> (int)(5L)) & (1L << (int)(16L) - 1L);
                hw = (x >> (int)(21L)) & (1L << (int)(2L) - 1L);
                shift = hw * 16L;
                return new ImmShift(uint16(imm16),uint8(shift));
                goto __switch_break0;
            }
            if (aop == arg_immediate_SBFIZ_SBFM_32M_bitfield_lsb_32_immr)
            {
                immr = (x >> (int)(16L)) & (1L << (int)(6L) - 1L);
                if (immr > 31L)
                {
                    return null;
                }

                return new Imm(32-immr,true);
                goto __switch_break0;
            }
            if (aop == arg_immediate_SBFIZ_SBFM_32M_bitfield_width_32_imms)
            {
                imms = (x >> (int)(10L)) & (1L << (int)(6L) - 1L);
                if (imms > 31L)
                {
                    return null;
                }

                return new Imm(imms+1,true);
                goto __switch_break0;
            }
            if (aop == arg_immediate_SBFIZ_SBFM_64M_bitfield_lsb_64_immr)
            {
                immr = (x >> (int)(16L)) & (1L << (int)(6L) - 1L);
                return new Imm(64-immr,true);
                goto __switch_break0;
            }
            if (aop == arg_immediate_SBFIZ_SBFM_64M_bitfield_width_64_imms)
            {
                imms = (x >> (int)(10L)) & (1L << (int)(6L) - 1L);
                return new Imm(imms+1,true);
                goto __switch_break0;
            }
            if (aop == arg_immediate_SBFX_SBFM_32M_bitfield_lsb_32_immr)
            {
                immr = (x >> (int)(16L)) & (1L << (int)(6L) - 1L);
                if (immr > 31L)
                {
                    return null;
                }

                return new Imm(immr,true);
                goto __switch_break0;
            }
            if (aop == arg_immediate_SBFX_SBFM_32M_bitfield_width_32_imms)
            {
                immr = (x >> (int)(16L)) & (1L << (int)(6L) - 1L);
                imms = (x >> (int)(10L)) & (1L << (int)(6L) - 1L);
                width = imms - immr + 1L;
                if (width < 1L || width > 32L - immr)
                {
                    return null;
                }

                return new Imm(width,true);
                goto __switch_break0;
            }
            if (aop == arg_immediate_SBFX_SBFM_64M_bitfield_lsb_64_immr)
            {
                immr = (x >> (int)(16L)) & (1L << (int)(6L) - 1L);
                return new Imm(immr,true);
                goto __switch_break0;
            }
            if (aop == arg_immediate_SBFX_SBFM_64M_bitfield_width_64_imms)
            {
                immr = (x >> (int)(16L)) & (1L << (int)(6L) - 1L);
                imms = (x >> (int)(10L)) & (1L << (int)(6L) - 1L);
                width = imms - immr + 1L;
                if (width < 1L || width > 64L - immr)
                {
                    return null;
                }

                return new Imm(width,true);
                goto __switch_break0;
            }
            if (aop == arg_immediate_shift_32_implicit_imm16_hw)
            {
                imm16 = (x >> (int)(5L)) & (1L << (int)(16L) - 1L);
                hw = (x >> (int)(21L)) & (1L << (int)(2L) - 1L);
                shift = hw * 16L;
                if (shift > 16L)
                {
                    return null;
                }

                var result = uint32(imm16) << (int)(shift);
                return new Imm(result,false);
                goto __switch_break0;
            }
            if (aop == arg_immediate_shift_32_implicit_inverse_imm16_hw)
            {
                imm16 = (x >> (int)(5L)) & (1L << (int)(16L) - 1L);
                hw = (x >> (int)(21L)) & (1L << (int)(2L) - 1L);
                shift = hw * 16L;
                if (shift > 16L)
                {
                    return null;
                }

                result = uint32(imm16) << (int)(shift);
                return new Imm(^result,false);
                goto __switch_break0;
            }
            if (aop == arg_immediate_shift_64_implicit_imm16_hw)
            {
                imm16 = (x >> (int)(5L)) & (1L << (int)(16L) - 1L);
                hw = (x >> (int)(21L)) & (1L << (int)(2L) - 1L);
                shift = hw * 16L;
                result = uint64(imm16) << (int)(shift);
                return new Imm64(result,false);
                goto __switch_break0;
            }
            if (aop == arg_immediate_shift_64_implicit_inverse_imm16_hw)
            {
                imm16 = (x >> (int)(5L)) & (1L << (int)(16L) - 1L);
                hw = (x >> (int)(21L)) & (1L << (int)(2L) - 1L);
                shift = hw * 16L;
                result = uint64(imm16) << (int)(shift);
                return new Imm64(^result,false);
                goto __switch_break0;
            }
            if (aop == arg_immediate_UBFIZ_UBFM_32M_bitfield_lsb_32_immr)
            {
                immr = (x >> (int)(16L)) & (1L << (int)(6L) - 1L);
                if (immr > 31L)
                {
                    return null;
                }

                return new Imm(32-immr,true);
                goto __switch_break0;
            }
            if (aop == arg_immediate_UBFIZ_UBFM_32M_bitfield_width_32_imms)
            {
                imms = (x >> (int)(10L)) & (1L << (int)(6L) - 1L);
                if (imms > 31L)
                {
                    return null;
                }

                return new Imm(imms+1,true);
                goto __switch_break0;
            }
            if (aop == arg_immediate_UBFIZ_UBFM_64M_bitfield_lsb_64_immr)
            {
                immr = (x >> (int)(16L)) & (1L << (int)(6L) - 1L);
                return new Imm(64-immr,true);
                goto __switch_break0;
            }
            if (aop == arg_immediate_UBFIZ_UBFM_64M_bitfield_width_64_imms)
            {
                imms = (x >> (int)(10L)) & (1L << (int)(6L) - 1L);
                return new Imm(imms+1,true);
                goto __switch_break0;
            }
            if (aop == arg_immediate_UBFX_UBFM_32M_bitfield_lsb_32_immr)
            {
                immr = (x >> (int)(16L)) & (1L << (int)(6L) - 1L);
                if (immr > 31L)
                {
                    return null;
                }

                return new Imm(immr,true);
                goto __switch_break0;
            }
            if (aop == arg_immediate_UBFX_UBFM_32M_bitfield_width_32_imms)
            {
                immr = (x >> (int)(16L)) & (1L << (int)(6L) - 1L);
                imms = (x >> (int)(10L)) & (1L << (int)(6L) - 1L);
                width = imms - immr + 1L;
                if (width < 1L || width > 32L - immr)
                {
                    return null;
                }

                return new Imm(width,true);
                goto __switch_break0;
            }
            if (aop == arg_immediate_UBFX_UBFM_64M_bitfield_lsb_64_immr)
            {
                immr = (x >> (int)(16L)) & (1L << (int)(6L) - 1L);
                return new Imm(immr,true);
                goto __switch_break0;
            }
            if (aop == arg_immediate_UBFX_UBFM_64M_bitfield_width_64_imms)
            {
                immr = (x >> (int)(16L)) & (1L << (int)(6L) - 1L);
                imms = (x >> (int)(10L)) & (1L << (int)(6L) - 1L);
                width = imms - immr + 1L;
                if (width < 1L || width > 64L - immr)
                {
                    return null;
                }

                return new Imm(width,true);
                goto __switch_break0;
            }
            if (aop == arg_Rt_31_1__W_0__X_1)
            {
                b5 = (x >> (int)(31L)) & 1L;
                var Rt = x & (1L << (int)(5L) - 1L);
                if (b5 == 0L)
                {
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
                var cond = (x >> (int)(12L)) & (1L << (int)(4L) - 1L);
                return new Cond(uint8(cond),false);
                goto __switch_break0;
            }
            if (aop == arg_conditional)
            {
                cond = x & (1L << (int)(4L) - 1L);
                return new Cond(uint8(cond),false);
                goto __switch_break0;
            }
            if (aop == arg_cond_NotAllowALNV_Invert)
            {
                cond = (x >> (int)(12L)) & (1L << (int)(4L) - 1L);
                if ((cond >> (int)(1L)) == 7L)
                {
                    return null;
                }

                return new Cond(uint8(cond),true);
                goto __switch_break0;
            }
            if (aop == arg_Cm)
            {
                var CRm = (x >> (int)(8L)) & (1L << (int)(4L) - 1L);
                return Imm_c(CRm);
                goto __switch_break0;
            }
            if (aop == arg_Cn)
            {
                var CRn = (x >> (int)(12L)) & (1L << (int)(4L) - 1L);
                return Imm_c(CRn);
                goto __switch_break0;
            }
            if (aop == arg_option_DMB_BO_system_CRm)
            {
                CRm = (x >> (int)(8L)) & (1L << (int)(4L) - 1L);
                return Imm_option(CRm);
                goto __switch_break0;
            }
            if (aop == arg_option_DSB_BO_system_CRm)
            {
                CRm = (x >> (int)(8L)) & (1L << (int)(4L) - 1L);
                return Imm_option(CRm);
                goto __switch_break0;
            }
            if (aop == arg_option_ISB_BI_system_CRm)
            {
                CRm = (x >> (int)(8L)) & (1L << (int)(4L) - 1L);
                if (CRm == 15L)
                {
                    return Imm_option(CRm);
                }

                return new Imm(CRm,false);
                goto __switch_break0;
            }
            if (aop == arg_prfop_Rt)
            {
                Rt = x & (1L << (int)(5L) - 1L);
                return Imm_prfop(Rt);
                goto __switch_break0;
            }
            if (aop == arg_pstatefield_op1_op2__SPSel_05__DAIFSet_36__DAIFClr_37)
            {
                op1 = (x >> (int)(16L)) & (1L << (int)(3L) - 1L);
                op2 = (x >> (int)(5L)) & (1L << (int)(3L) - 1L);
                if ((op1 == 0L) && (op2 == 5L))
                {
                    return SPSel;
                }
                else if ((op1 == 3L) && (op2 == 6L))
                {
                    return DAIFSet;
                }
                else if ((op1 == 3L) && (op2 == 7L))
                {
                    return DAIFClr;
                }

                return null;
                goto __switch_break0;
            }
            if (aop == arg_sysreg_o0_op1_CRn_CRm_op2)
            {
                var op0 = (x >> (int)(19L)) & (1L << (int)(2L) - 1L);
                op1 = (x >> (int)(16L)) & (1L << (int)(3L) - 1L);
                CRn = (x >> (int)(12L)) & (1L << (int)(4L) - 1L);
                CRm = (x >> (int)(8L)) & (1L << (int)(4L) - 1L);
                op2 = (x >> (int)(5L)) & (1L << (int)(3L) - 1L);
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
                return B0 + Reg(x & (1L << (int)(5L) - 1L));
                goto __switch_break0;
            }
            if (aop == arg_Dt)
            {
                return D0 + Reg(x & (1L << (int)(5L) - 1L));
                goto __switch_break0;
            }
            if (aop == arg_Dt2)
            {
                return D0 + Reg((x >> (int)(10L)) & (1L << (int)(5L) - 1L));
                goto __switch_break0;
            }
            if (aop == arg_Ht)
            {
                return H0 + Reg(x & (1L << (int)(5L) - 1L));
                goto __switch_break0;
            }
            if (aop == arg_immediate_0_63_immh_immb__UIntimmhimmb64_8)
            {
                var immh = (x >> (int)(19L)) & (1L << (int)(4L) - 1L);
                if ((immh & 8L) == 0L)
                {
                    return null;
                }

                var immb = (x >> (int)(16L)) & (1L << (int)(3L) - 1L);
                return new Imm((immh<<3)+immb-64,true);
                goto __switch_break0;
            }
            if (aop == arg_immediate_0_width_immh_immb__SEEAdvancedSIMDmodifiedimmediate_0__UIntimmhimmb8_1__UIntimmhimmb16_2__UIntimmhimmb32_4)
            {
                immh = (x >> (int)(19L)) & (1L << (int)(4L) - 1L);
                immb = (x >> (int)(16L)) & (1L << (int)(3L) - 1L);
                if (immh == 1L)
                {
                    return new Imm((immh<<3)+immb-8,true);
                }
                else if ((immh >> (int)(1L)) == 1L)
                {
                    return new Imm((immh<<3)+immb-16,true);
                }
                else if ((immh >> (int)(2L)) == 1L)
                {
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
                immh = (x >> (int)(19L)) & (1L << (int)(4L) - 1L);
                immb = (x >> (int)(16L)) & (1L << (int)(3L) - 1L);
                if (immh == 1L)
                {
                    return new Imm((immh<<3)+immb-8,true);
                }
                else if ((immh >> (int)(1L)) == 1L)
                {
                    return new Imm((immh<<3)+immb-16,true);
                }
                else if ((immh >> (int)(2L)) == 1L)
                {
                    return new Imm((immh<<3)+immb-32,true);
                }
                else if ((immh >> (int)(3L)) == 1L)
                {
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
                var size = (x >> (int)(22L)) & (1L << (int)(2L) - 1L);
                switch (size)
                {
                    case 0L: 
                        return new Imm(8,true);
                        break;
                    case 1L: 
                        return new Imm(16,true);
                        break;
                    case 2L: 
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
                immh = (x >> (int)(19L)) & (1L << (int)(4L) - 1L);
                if ((immh & 8L) == 0L)
                {
                    return null;
                }

                immb = (x >> (int)(16L)) & (1L << (int)(3L) - 1L);
                return new Imm(128-((immh<<3)+immb),true);
                goto __switch_break0;
            }
            if (aop == arg_immediate_1_width_immh_immb__16UIntimmhimmb_1__32UIntimmhimmb_2__64UIntimmhimmb_4)
            {
                fallthrough = true;

            }
            if (fallthrough || aop == arg_immediate_1_width_immh_immb__SEEAdvancedSIMDmodifiedimmediate_0__16UIntimmhimmb_1__32UIntimmhimmb_2__64UIntimmhimmb_4)
            {
                immh = (x >> (int)(19L)) & (1L << (int)(4L) - 1L);
                immb = (x >> (int)(16L)) & (1L << (int)(3L) - 1L);
                if (immh == 1L)
                {
                    return new Imm(16-((immh<<3)+immb),true);
                }
                else if ((immh >> (int)(1L)) == 1L)
                {
                    return new Imm(32-((immh<<3)+immb),true);
                }
                else if ((immh >> (int)(2L)) == 1L)
                {
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
                immh = (x >> (int)(19L)) & (1L << (int)(4L) - 1L);
                immb = (x >> (int)(16L)) & (1L << (int)(3L) - 1L);
                if (immh == 1L)
                {
                    return new Imm(16-((immh<<3)+immb),true);
                }
                else if ((immh >> (int)(1L)) == 1L)
                {
                    return new Imm(32-((immh<<3)+immb),true);
                }
                else if ((immh >> (int)(2L)) == 1L)
                {
                    return new Imm(64-((immh<<3)+immb),true);
                }
                else if ((immh >> (int)(3L)) == 1L)
                {
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
                if (x & (1L << (int)(5L)) != 0L)
                {
                    imm = (1L << (int)(8L)) - 1L;
                }
                else
                {
                    imm = 0L;
                }

                if (x & (1L << (int)(6L)) != 0L)
                {
                    imm += ((1L << (int)(8L)) - 1L) << (int)(8L);
                }

                if (x & (1L << (int)(7L)) != 0L)
                {
                    imm += ((1L << (int)(8L)) - 1L) << (int)(16L);
                }

                if (x & (1L << (int)(8L)) != 0L)
                {
                    imm += ((1L << (int)(8L)) - 1L) << (int)(24L);
                }

                if (x & (1L << (int)(9L)) != 0L)
                {
                    imm += ((1L << (int)(8L)) - 1L) << (int)(32L);
                }

                if (x & (1L << (int)(16L)) != 0L)
                {
                    imm += ((1L << (int)(8L)) - 1L) << (int)(40L);
                }

                if (x & (1L << (int)(17L)) != 0L)
                {
                    imm += ((1L << (int)(8L)) - 1L) << (int)(48L);
                }

                if (x & (1L << (int)(18L)) != 0L)
                {
                    imm += ((1L << (int)(8L)) - 1L) << (int)(56L);
                }

                return new Imm64(imm,false);
                goto __switch_break0;
            }
            if (aop == arg_immediate_exp_3_pre_4_a_b_c_d_e_f_g_h)
            {
                var pre = (x >> (int)(5L)) & (1L << (int)(4L) - 1L);
                long exp = 1L - ((x >> (int)(17L)) & 1L);
                exp = (exp << (int)(2L)) + (((x >> (int)(16L)) & 1L) << (int)(1L)) + ((x >> (int)(9L)) & 1L);
                var s = ((x >> (int)(18L)) & 1L);
                return new Imm_fp(uint8(s),int8(exp)-3,uint8(pre));
                goto __switch_break0;
            }
            if (aop == arg_immediate_exp_3_pre_4_imm8)
            {
                pre = (x >> (int)(13L)) & (1L << (int)(4L) - 1L);
                exp = 1L - ((x >> (int)(19L)) & 1L);
                exp = (exp << (int)(2L)) + ((x >> (int)(17L)) & (1L << (int)(2L) - 1L));
                s = ((x >> (int)(20L)) & 1L);
                return new Imm_fp(uint8(s),int8(exp)-3,uint8(pre));
                goto __switch_break0;
            }
            if (aop == arg_immediate_fbits_min_1_max_0_sub_0_immh_immb__64UIntimmhimmb_4__128UIntimmhimmb_8)
            {
                fallthrough = true;

            }
            if (fallthrough || aop == arg_immediate_fbits_min_1_max_0_sub_0_immh_immb__SEEAdvancedSIMDmodifiedimmediate_0__64UIntimmhimmb_4__128UIntimmhimmb_8)
            {
                immh = (x >> (int)(19L)) & (1L << (int)(4L) - 1L);
                immb = (x >> (int)(16L)) & (1L << (int)(3L) - 1L);
                if ((immh >> (int)(2L)) == 1L)
                {
                    return new Imm(64-((immh<<3)+immb),true);
                }
                else if ((immh >> (int)(3L)) == 1L)
                {
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
                var scale = (x >> (int)(10L)) & (1L << (int)(6L) - 1L);
                long fbits = 64L - scale;
                if (fbits > 32L)
                {
                    return null;
                }

                return new Imm(fbits,true);
                goto __switch_break0;
            }
            if (aop == arg_immediate_fbits_min_1_max_64_sub_64_scale)
            {
                scale = (x >> (int)(10L)) & (1L << (int)(6L) - 1L);
                fbits = 64L - scale;
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
                var Q = (x >> (int)(30L)) & 1L;
                var imm4 = (x >> (int)(11L)) & (1L << (int)(4L) - 1L);
                if (Q == 1L || (imm4 >> (int)(3L)) == 0L)
                {
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
                var imm8 = (x >> (int)(16L)) & (1L << (int)(3L) - 1L);
                imm8 = (imm8 << (int)(5L)) | ((x >> (int)(5L)) & (1L << (int)(5L) - 1L));
                if ((x >> (int)(12L)) & 1L == 0L)
                {
                    shift = 8L + 128L;
                }
                else
                {
                    shift = 16L + 128L;
                }

                return new ImmShift(uint16(imm8),shift);
                goto __switch_break0;
            }
            if (aop == arg_immediate_OptLSL__a_b_c_d_e_f_g_h_cmode__0_0__8_1)
            {
                imm8 = (x >> (int)(16L)) & (1L << (int)(3L) - 1L);
                imm8 = (imm8 << (int)(5L)) | ((x >> (int)(5L)) & (1L << (int)(5L) - 1L));
                var cmode1 = (x >> (int)(13L)) & 1L;
                shift = 8L * cmode1;
                return new ImmShift(uint16(imm8),uint8(shift));
                goto __switch_break0;
            }
            if (aop == arg_immediate_OptLSL__a_b_c_d_e_f_g_h_cmode__0_0__8_1__16_2__24_3)
            {
                imm8 = (x >> (int)(16L)) & (1L << (int)(3L) - 1L);
                imm8 = (imm8 << (int)(5L)) | ((x >> (int)(5L)) & (1L << (int)(5L) - 1L));
                cmode1 = (x >> (int)(13L)) & (1L << (int)(2L) - 1L);
                shift = 8L * cmode1;
                return new ImmShift(uint16(imm8),uint8(shift));
                goto __switch_break0;
            }
            if (aop == arg_immediate_OptLSLZero__a_b_c_d_e_f_g_h)
            {
                imm8 = (x >> (int)(16L)) & (1L << (int)(3L) - 1L);
                imm8 = (imm8 << (int)(5L)) | ((x >> (int)(5L)) & (1L << (int)(5L) - 1L));
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
                return Q0 + Reg(x & (1L << (int)(5L) - 1L));
                goto __switch_break0;
            }
            if (aop == arg_Qn)
            {
                return Q0 + Reg((x >> (int)(5L)) & (1L << (int)(5L) - 1L));
                goto __switch_break0;
            }
            if (aop == arg_Qt)
            {
                return Q0 + Reg(x & (1L << (int)(5L) - 1L));
                goto __switch_break0;
            }
            if (aop == arg_Qt2)
            {
                return Q0 + Reg((x >> (int)(10L)) & (1L << (int)(5L) - 1L));
                goto __switch_break0;
            }
            if (aop == arg_Rn_16_5__W_1__W_2__W_4__X_8)
            {
                imm5 = (x >> (int)(16L)) & (1L << (int)(5L) - 1L);
                if (((imm5 & 1L) == 1L) || ((imm5 & 2L) == 2L) || ((imm5 & 4L) == 4L))
                {
                    return W0 + Reg((x >> (int)(5L)) & (1L << (int)(5L) - 1L));
                }
                else if ((imm5 & 8L) == 8L)
                {
                    return X0 + Reg((x >> (int)(5L)) & (1L << (int)(5L) - 1L));
                }
                else
                {
                    return null;
                }

                goto __switch_break0;
            }
            if (aop == arg_St)
            {
                return S0 + Reg(x & (1L << (int)(5L) - 1L));
                goto __switch_break0;
            }
            if (aop == arg_St2)
            {
                return S0 + Reg((x >> (int)(10L)) & (1L << (int)(5L) - 1L));
                goto __switch_break0;
            }
            if (aop == arg_Vd_16_5__B_1__H_2__S_4__D_8)
            {
                imm5 = (x >> (int)(16L)) & (1L << (int)(5L) - 1L);
                var Rd = x & (1L << (int)(5L) - 1L);
                if (imm5 & 1L == 1L)
                {
                    return B0 + Reg(Rd);
                }
                else if (imm5 & 2L == 2L)
                {
                    return H0 + Reg(Rd);
                }
                else if (imm5 & 4L == 4L)
                {
                    return S0 + Reg(Rd);
                }
                else if (imm5 & 8L == 8L)
                {
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
                immh = (x >> (int)(19L)) & (1L << (int)(4L) - 1L);
                Rd = x & (1L << (int)(5L) - 1L);
                if (immh == 1L)
                {
                    return B0 + Reg(Rd);
                }
                else if (immh >> (int)(1L) == 1L)
                {
                    return H0 + Reg(Rd);
                }
                else if (immh >> (int)(2L) == 1L)
                {
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
                immh = (x >> (int)(19L)) & (1L << (int)(4L) - 1L);
                Rd = x & (1L << (int)(5L) - 1L);
                if (immh == 1L)
                {
                    return B0 + Reg(Rd);
                }
                else if (immh >> (int)(1L) == 1L)
                {
                    return H0 + Reg(Rd);
                }
                else if (immh >> (int)(2L) == 1L)
                {
                    return S0 + Reg(Rd);
                }
                else if (immh >> (int)(3L) == 1L)
                {
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
                immh = (x >> (int)(19L)) & (1L << (int)(4L) - 1L);
                Rd = x & (1L << (int)(5L) - 1L);
                if (immh >> (int)(3L) == 1L)
                {
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
                immh = (x >> (int)(19L)) & (1L << (int)(4L) - 1L);
                Rd = x & (1L << (int)(5L) - 1L);
                if (immh >> (int)(2L) == 1L)
                {
                    return S0 + Reg(Rd);
                }
                else if (immh >> (int)(3L) == 1L)
                {
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
                var sz = (x >> (int)(22L)) & 1L;
                Rd = x & (1L << (int)(5L) - 1L);
                if (sz == 0L)
                {
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
                sz = (x >> (int)(22L)) & 1L;
                Rd = x & (1L << (int)(5L) - 1L);
                if (sz == 0L)
                {
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
                sz = (x >> (int)(22L)) & 1L;
                Rd = x & (1L << (int)(5L) - 1L);
                if (sz == 1L)
                {
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
                size = (x >> (int)(22L)) & (1L << (int)(2L) - 1L);
                Rd = x & (1L << (int)(5L) - 1L);
                if (size == 0L)
                {
                    return B0 + Reg(Rd);
                }
                else if (size == 1L)
                {
                    return H0 + Reg(Rd);
                }
                else if (size == 2L)
                {
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
                size = (x >> (int)(22L)) & (1L << (int)(2L) - 1L);
                Rd = x & (1L << (int)(5L) - 1L);
                if (size == 0L)
                {
                    return B0 + Reg(Rd);
                }
                else if (size == 1L)
                {
                    return H0 + Reg(Rd);
                }
                else if (size == 2L)
                {
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
                size = (x >> (int)(22L)) & (1L << (int)(2L) - 1L);
                Rd = x & (1L << (int)(5L) - 1L);
                if (size == 3L)
                {
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
                size = (x >> (int)(22L)) & (1L << (int)(2L) - 1L);
                Rd = x & (1L << (int)(5L) - 1L);
                if (size == 0L)
                {
                    return H0 + Reg(Rd);
                }
                else if (size == 1L)
                {
                    return S0 + Reg(Rd);
                }
                else if (size == 2L)
                {
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
                size = (x >> (int)(22L)) & (1L << (int)(2L) - 1L);
                Rd = x & (1L << (int)(5L) - 1L);
                if (size == 1L)
                {
                    return H0 + Reg(Rd);
                }
                else if (size == 2L)
                {
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
                size = (x >> (int)(22L)) & (1L << (int)(2L) - 1L);
                Rd = x & (1L << (int)(5L) - 1L);
                if (size == 1L)
                {
                    return S0 + Reg(Rd);
                }
                else if (size == 2L)
                {
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
                Rd = x & (1L << (int)(5L) - 1L);
                return new RegisterWithArrangement(V0+Reg(Rd),Arrangement16B,0);
                goto __switch_break0;
            }
            if (aop == arg_Vd_arrangement_2D)
            {
                Rd = x & (1L << (int)(5L) - 1L);
                return new RegisterWithArrangement(V0+Reg(Rd),Arrangement2D,0);
                goto __switch_break0;
            }
            if (aop == arg_Vd_arrangement_4S)
            {
                Rd = x & (1L << (int)(5L) - 1L);
                return new RegisterWithArrangement(V0+Reg(Rd),Arrangement4S,0);
                goto __switch_break0;
            }
            if (aop == arg_Vd_arrangement_D_index__1)
            {
                Rd = x & (1L << (int)(5L) - 1L);
                return new RegisterWithArrangementAndIndex(V0+Reg(Rd),ArrangementD,1,0);
                goto __switch_break0;
            }
            if (aop == arg_Vd_arrangement_imm5___B_1__H_2__S_4__D_8_index__imm5__imm5lt41gt_1__imm5lt42gt_2__imm5lt43gt_4__imm5lt4gt_8_1)
            {
                Arrangement a = default;
                uint index = default;
                Rd = x & (1L << (int)(5L) - 1L);
                imm5 = (x >> (int)(16L)) & (1L << (int)(5L) - 1L);
                if (imm5 & 1L == 1L)
                {
                    a = ArrangementB;
                    index = imm5 >> (int)(1L);
                }
                else if (imm5 & 2L == 2L)
                {
                    a = ArrangementH;
                    index = imm5 >> (int)(2L);
                }
                else if (imm5 & 4L == 4L)
                {
                    a = ArrangementS;
                    index = imm5 >> (int)(3L);
                }
                else if (imm5 & 8L == 8L)
                {
                    a = ArrangementD;
                    index = imm5 >> (int)(4L);
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
                Rd = x & (1L << (int)(5L) - 1L);
                imm5 = (x >> (int)(16L)) & (1L << (int)(5L) - 1L);
                Q = (x >> (int)(30L)) & 1L;
                if (imm5 & 1L == 1L)
                {
                    if (Q == 0L)
                    {
                        return new RegisterWithArrangement(V0+Reg(Rd),Arrangement8B,0);
                    }
                    else
                    {
                        return new RegisterWithArrangement(V0+Reg(Rd),Arrangement16B,0);
                    }

                }
                else if (imm5 & 2L == 2L)
                {
                    if (Q == 0L)
                    {
                        return new RegisterWithArrangement(V0+Reg(Rd),Arrangement4H,0);
                    }
                    else
                    {
                        return new RegisterWithArrangement(V0+Reg(Rd),Arrangement8H,0);
                    }

                }
                else if (imm5 & 4L == 4L)
                {
                    if (Q == 0L)
                    {
                        return new RegisterWithArrangement(V0+Reg(Rd),Arrangement2S,0);
                    }
                    else
                    {
                        return new RegisterWithArrangement(V0+Reg(Rd),Arrangement4S,0);
                    }

                }
                else if ((imm5 & 8L == 8L) && (Q == 1L))
                {
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
                Rd = x & (1L << (int)(5L) - 1L);
                immh = (x >> (int)(19L)) & (1L << (int)(4L) - 1L);
                Q = (x >> (int)(30L)) & 1L;
                if (immh >> (int)(2L) == 1L)
                {
                    if (Q == 0L)
                    {
                        return new RegisterWithArrangement(V0+Reg(Rd),Arrangement2S,0);
                    }
                    else
                    {
                        return new RegisterWithArrangement(V0+Reg(Rd),Arrangement4S,0);
                    }

                }
                else if (immh >> (int)(3L) == 1L)
                {
                    if (Q == 1L)
                    {
                        return new RegisterWithArrangement(V0+Reg(Rd),Arrangement2D,0);
                    }

                }

                return null;
                goto __switch_break0;
            }
            if (aop == arg_Vd_arrangement_immh_Q___SEEAdvancedSIMDmodifiedimmediate_00__8B_10__16B_11__4H_20__8H_21__2S_40__4S_41)
            {
                Rd = x & (1L << (int)(5L) - 1L);
                immh = (x >> (int)(19L)) & (1L << (int)(4L) - 1L);
                Q = (x >> (int)(30L)) & 1L;
                if (immh == 1L)
                {
                    if (Q == 0L)
                    {
                        return new RegisterWithArrangement(V0+Reg(Rd),Arrangement8B,0);
                    }
                    else
                    {
                        return new RegisterWithArrangement(V0+Reg(Rd),Arrangement16B,0);
                    }

                }
                else if (immh >> (int)(1L) == 1L)
                {
                    if (Q == 0L)
                    {
                        return new RegisterWithArrangement(V0+Reg(Rd),Arrangement4H,0);
                    }
                    else
                    {
                        return new RegisterWithArrangement(V0+Reg(Rd),Arrangement8H,0);
                    }

                }
                else if (immh >> (int)(2L) == 1L)
                {
                    if (Q == 0L)
                    {
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
                Rd = x & (1L << (int)(5L) - 1L);
                immh = (x >> (int)(19L)) & (1L << (int)(4L) - 1L);
                Q = (x >> (int)(30L)) & 1L;
                if (immh == 1L)
                {
                    if (Q == 0L)
                    {
                        return new RegisterWithArrangement(V0+Reg(Rd),Arrangement8B,0);
                    }
                    else
                    {
                        return new RegisterWithArrangement(V0+Reg(Rd),Arrangement16B,0);
                    }

                }
                else if (immh >> (int)(1L) == 1L)
                {
                    if (Q == 0L)
                    {
                        return new RegisterWithArrangement(V0+Reg(Rd),Arrangement4H,0);
                    }
                    else
                    {
                        return new RegisterWithArrangement(V0+Reg(Rd),Arrangement8H,0);
                    }

                }
                else if (immh >> (int)(2L) == 1L)
                {
                    if (Q == 0L)
                    {
                        return new RegisterWithArrangement(V0+Reg(Rd),Arrangement2S,0);
                    }
                    else
                    {
                        return new RegisterWithArrangement(V0+Reg(Rd),Arrangement4S,0);
                    }

                }
                else if (immh >> (int)(3L) == 1L)
                {
                    if (Q == 1L)
                    {
                        return new RegisterWithArrangement(V0+Reg(Rd),Arrangement2D,0);
                    }

                }

                return null;
                goto __switch_break0;
            }
            if (aop == arg_Vd_arrangement_immh___SEEAdvancedSIMDmodifiedimmediate_0__8H_1__4S_2__2D_4)
            {
                Rd = x & (1L << (int)(5L) - 1L);
                immh = (x >> (int)(19L)) & (1L << (int)(4L) - 1L);
                if (immh == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rd),Arrangement8H,0);
                }
                else if (immh >> (int)(1L) == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rd),Arrangement4S,0);
                }
                else if (immh >> (int)(2L) == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rd),Arrangement2D,0);
                }

                return null;
                goto __switch_break0;
            }
            if (aop == arg_Vd_arrangement_Q___2S_0__4S_1)
            {
                Rd = x & (1L << (int)(5L) - 1L);
                Q = (x >> (int)(30L)) & 1L;
                if (Q == 0L)
                {
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
                Rd = x & (1L << (int)(5L) - 1L);
                Q = (x >> (int)(30L)) & 1L;
                if (Q == 0L)
                {
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
                Rd = x & (1L << (int)(5L) - 1L);
                Q = (x >> (int)(30L)) & 1L;
                if (Q == 0L)
                {
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
                Rd = x & (1L << (int)(5L) - 1L);
                Q = (x >> (int)(30L)) & 1L;
                sz = (x >> (int)(22L)) & 1L;
                if (sz == 0L && Q == 0L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rd),Arrangement2S,0);
                }
                else if (sz == 0L && Q == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rd),Arrangement4S,0);
                }
                else if (sz == 1L && Q == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rd),Arrangement2D,0);
                }

                return null;
                goto __switch_break0;
            }
            if (aop == arg_Vd_arrangement_size___4S_1__2D_2)
            {
                Rd = x & (1L << (int)(5L) - 1L);
                size = (x >> (int)(22L)) & 3L;
                if (size == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rd),Arrangement4S,0);
                }
                else if (size == 2L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rd),Arrangement2D,0);
                }

                return null;
                goto __switch_break0;
            }
            if (aop == arg_Vd_arrangement_size___8H_0__1Q_3)
            {
                Rd = x & (1L << (int)(5L) - 1L);
                size = (x >> (int)(22L)) & 3L;
                if (size == 0L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rd),Arrangement8H,0);
                }
                else if (size == 3L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rd),Arrangement1Q,0);
                }

                return null;
                goto __switch_break0;
            }
            if (aop == arg_Vd_arrangement_size___8H_0__4S_1__2D_2)
            {
                Rd = x & (1L << (int)(5L) - 1L);
                size = (x >> (int)(22L)) & 3L;
                if (size == 0L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rd),Arrangement8H,0);
                }
                else if (size == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rd),Arrangement4S,0);
                }
                else if (size == 2L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rd),Arrangement2D,0);
                }

                return null;
                goto __switch_break0;
            }
            if (aop == arg_Vd_arrangement_size_Q___4H_00__8H_01__2S_10__4S_11__1D_20__2D_21)
            {
                Rd = x & (1L << (int)(5L) - 1L);
                size = (x >> (int)(22L)) & 3L;
                Q = (x >> (int)(30L)) & 1L;
                if (size == 0L && Q == 0L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rd),Arrangement4H,0);
                }
                else if (size == 0L && Q == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rd),Arrangement8H,0);
                }
                else if (size == 1L && Q == 0L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rd),Arrangement2S,0);
                }
                else if (size == 1L && Q == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rd),Arrangement4S,0);
                }
                else if (size == 2L && Q == 0L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rd),Arrangement1D,0);
                }
                else if (size == 2L && Q == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rd),Arrangement2D,0);
                }

                return null;
                goto __switch_break0;
            }
            if (aop == arg_Vd_arrangement_size_Q___4H_10__8H_11__2S_20__4S_21)
            {
                Rd = x & (1L << (int)(5L) - 1L);
                size = (x >> (int)(22L)) & 3L;
                Q = (x >> (int)(30L)) & 1L;
                if (size == 1L && Q == 0L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rd),Arrangement4H,0);
                }
                else if (size == 1L && Q == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rd),Arrangement8H,0);
                }
                else if (size == 2L && Q == 0L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rd),Arrangement2S,0);
                }
                else if (size == 2L && Q == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rd),Arrangement4S,0);
                }

                return null;
                goto __switch_break0;
            }
            if (aop == arg_Vd_arrangement_size_Q___8B_00__16B_01)
            {
                Rd = x & (1L << (int)(5L) - 1L);
                size = (x >> (int)(22L)) & 3L;
                Q = (x >> (int)(30L)) & 1L;
                if (size == 0L && Q == 0L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rd),Arrangement8B,0);
                }
                else if (size == 0L && Q == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rd),Arrangement16B,0);
                }

                return null;
                goto __switch_break0;
            }
            if (aop == arg_Vd_arrangement_size_Q___8B_00__16B_01__4H_10__8H_11)
            {
                Rd = x & (1L << (int)(5L) - 1L);
                size = (x >> (int)(22L)) & 3L;
                Q = (x >> (int)(30L)) & 1L;
                if (size == 0L && Q == 0L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rd),Arrangement8B,0);
                }
                else if (size == 0L && Q == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rd),Arrangement16B,0);
                }
                else if (size == 1L && Q == 0L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rd),Arrangement4H,0);
                }
                else if (size == 1L && Q == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rd),Arrangement8H,0);
                }

                return null;
                goto __switch_break0;
            }
            if (aop == arg_Vd_arrangement_size_Q___8B_00__16B_01__4H_10__8H_11__2S_20__4S_21)
            {
                Rd = x & (1L << (int)(5L) - 1L);
                size = (x >> (int)(22L)) & 3L;
                Q = (x >> (int)(30L)) & 1L;
                if (size == 0L && Q == 0L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rd),Arrangement8B,0);
                }
                else if (size == 0L && Q == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rd),Arrangement16B,0);
                }
                else if (size == 1L && Q == 0L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rd),Arrangement4H,0);
                }
                else if (size == 1L && Q == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rd),Arrangement8H,0);
                }
                else if (size == 2L && Q == 0L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rd),Arrangement2S,0);
                }
                else if (size == 2L && Q == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rd),Arrangement4S,0);
                }

                return null;
                goto __switch_break0;
            }
            if (aop == arg_Vd_arrangement_size_Q___8B_00__16B_01__4H_10__8H_11__2S_20__4S_21__2D_31)
            {
                Rd = x & (1L << (int)(5L) - 1L);
                size = (x >> (int)(22L)) & 3L;
                Q = (x >> (int)(30L)) & 1L;
                if (size == 0L && Q == 0L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rd),Arrangement8B,0);
                }
                else if (size == 0L && Q == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rd),Arrangement16B,0);
                }
                else if (size == 1L && Q == 0L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rd),Arrangement4H,0);
                }
                else if (size == 1L && Q == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rd),Arrangement8H,0);
                }
                else if (size == 2L && Q == 0L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rd),Arrangement2S,0);
                }
                else if (size == 2L && Q == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rd),Arrangement4S,0);
                }
                else if (size == 3L && Q == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rd),Arrangement2D,0);
                }

                return null;
                goto __switch_break0;
            }
            if (aop == arg_Vd_arrangement_sz___4S_0__2D_1)
            {
                Rd = x & (1L << (int)(5L) - 1L);
                sz = (x >> (int)(22L)) & 1L;
                if (sz == 0L)
                {
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
                Rd = x & (1L << (int)(5L) - 1L);
                sz = (x >> (int)(22L)) & 1L;
                Q = (x >> (int)(30L)) & 1L;
                if (sz == 0L && Q == 0L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rd),Arrangement2S,0);
                }
                else if (sz == 0L && Q == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rd),Arrangement4S,0);
                }

                return null;
                goto __switch_break0;
            }
            if (aop == arg_Vd_arrangement_sz_Q___2S_00__4S_01__2D_11)
            {
                Rd = x & (1L << (int)(5L) - 1L);
                sz = (x >> (int)(22L)) & 1L;
                Q = (x >> (int)(30L)) & 1L;
                if (sz == 0L && Q == 0L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rd),Arrangement2S,0);
                }
                else if (sz == 0L && Q == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rd),Arrangement4S,0);
                }
                else if (sz == 1L && Q == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rd),Arrangement2D,0);
                }

                return null;
                goto __switch_break0;
            }
            if (aop == arg_Vd_arrangement_sz_Q___2S_10__4S_11)
            {
                Rd = x & (1L << (int)(5L) - 1L);
                sz = (x >> (int)(22L)) & 1L;
                Q = (x >> (int)(30L)) & 1L;
                if (sz == 1L && Q == 0L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rd),Arrangement2S,0);
                }
                else if (sz == 1L && Q == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rd),Arrangement4S,0);
                }

                return null;
                goto __switch_break0;
            }
            if (aop == arg_Vd_arrangement_sz_Q___4H_00__8H_01__2S_10__4S_11)
            {
                Rd = x & (1L << (int)(5L) - 1L);
                sz = (x >> (int)(22L)) & 1L;
                Q = (x >> (int)(30L)) & 1L;
                if (sz == 0L && Q == 0L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rd),Arrangement4H,0);
                }
                else if (sz == 0L && Q == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rd),Arrangement8H,0);
                }
                else if (sz == 1L && Q == 0L)
                {
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
                sz = (x >> (int)(22L)) & 1L;
                var Rm = (x >> (int)(16L)) & (1L << (int)(5L) - 1L);
                if (sz == 0L)
                {
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
                size = (x >> (int)(22L)) & (1L << (int)(2L) - 1L);
                Rm = (x >> (int)(16L)) & (1L << (int)(5L) - 1L);
                if (size == 0L)
                {
                    return B0 + Reg(Rm);
                }
                else if (size == 1L)
                {
                    return H0 + Reg(Rm);
                }
                else if (size == 2L)
                {
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
                size = (x >> (int)(22L)) & (1L << (int)(2L) - 1L);
                Rm = (x >> (int)(16L)) & (1L << (int)(5L) - 1L);
                if (size == 3L)
                {
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
                size = (x >> (int)(22L)) & (1L << (int)(2L) - 1L);
                Rm = (x >> (int)(16L)) & (1L << (int)(5L) - 1L);
                if (size == 1L)
                {
                    return H0 + Reg(Rm);
                }
                else if (size == 2L)
                {
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
                Rm = (x >> (int)(16L)) & (1L << (int)(5L) - 1L);
                return new RegisterWithArrangement(V0+Reg(Rm),Arrangement4S,0);
                goto __switch_break0;
            }
            if (aop == arg_Vm_arrangement_Q___8B_0__16B_1)
            {
                Rm = (x >> (int)(16L)) & (1L << (int)(5L) - 1L);
                Q = (x >> (int)(30L)) & 1L;
                if (Q == 0L)
                {
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
                Rm = (x >> (int)(16L)) & (1L << (int)(5L) - 1L);
                size = (x >> (int)(22L)) & 3L;
                if (size == 0L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rm),Arrangement8H,0);
                }
                else if (size == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rm),Arrangement4S,0);
                }
                else if (size == 2L)
                {
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
                Rm = (x >> (int)(16L)) & (1L << (int)(4L) - 1L);
                size = (x >> (int)(22L)) & 3L;
                var H = (x >> (int)(11L)) & 1L;
                var L = (x >> (int)(21L)) & 1L;
                var M = (x >> (int)(20L)) & 1L;
                if (size == 1L)
                {
                    a = ArrangementH;
                    index = (H << (int)(2L)) | (L << (int)(1L)) | M;
                    vm = Rm;
                }
                else if (size == 2L)
                {
                    a = ArrangementS;
                    index = (H << (int)(1L)) | L;
                    vm = (M << (int)(4L)) | Rm;
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
                Rm = (x >> (int)(16L)) & (1L << (int)(5L) - 1L);
                size = (x >> (int)(22L)) & 3L;
                Q = (x >> (int)(30L)) & 1L;
                if (size == 1L && Q == 0L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rm),Arrangement4H,0);
                }
                else if (size == 1L && Q == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rm),Arrangement8H,0);
                }
                else if (size == 2L && Q == 0L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rm),Arrangement2S,0);
                }
                else if (size == 2L && Q == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rm),Arrangement4S,0);
                }

                return null;
                goto __switch_break0;
            }
            if (aop == arg_Vm_arrangement_size_Q___8B_00__16B_01)
            {
                Rm = (x >> (int)(16L)) & (1L << (int)(5L) - 1L);
                size = (x >> (int)(22L)) & 3L;
                Q = (x >> (int)(30L)) & 1L;
                if (size == 0L && Q == 0L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rm),Arrangement8B,0);
                }
                else if (size == 0L && Q == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rm),Arrangement16B,0);
                }

                return null;
                goto __switch_break0;
            }
            if (aop == arg_Vm_arrangement_size_Q___8B_00__16B_01__1D_30__2D_31)
            {
                Rm = (x >> (int)(16L)) & (1L << (int)(5L) - 1L);
                size = (x >> (int)(22L)) & 3L;
                Q = (x >> (int)(30L)) & 1L;
                if (size == 0L && Q == 0L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rm),Arrangement8B,0);
                }
                else if (size == 0L && Q == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rm),Arrangement16B,0);
                }
                else if (size == 3L && Q == 0L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rm),Arrangement1D,0);
                }
                else if (size == 3L && Q == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rm),Arrangement2D,0);
                }

                return null;
                goto __switch_break0;
            }
            if (aop == arg_Vm_arrangement_size_Q___8B_00__16B_01__4H_10__8H_11__2S_20__4S_21)
            {
                Rm = (x >> (int)(16L)) & (1L << (int)(5L) - 1L);
                size = (x >> (int)(22L)) & 3L;
                Q = (x >> (int)(30L)) & 1L;
                if (size == 0L && Q == 0L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rm),Arrangement8B,0);
                }
                else if (size == 0L && Q == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rm),Arrangement16B,0);
                }
                else if (size == 1L && Q == 0L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rm),Arrangement4H,0);
                }
                else if (size == 1L && Q == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rm),Arrangement8H,0);
                }
                else if (size == 2L && Q == 0L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rm),Arrangement2S,0);
                }
                else if (size == 2L && Q == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rm),Arrangement4S,0);
                }

                return null;
                goto __switch_break0;
            }
            if (aop == arg_Vm_arrangement_size_Q___8B_00__16B_01__4H_10__8H_11__2S_20__4S_21__2D_31)
            {
                Rm = (x >> (int)(16L)) & (1L << (int)(5L) - 1L);
                size = (x >> (int)(22L)) & 3L;
                Q = (x >> (int)(30L)) & 1L;
                if (size == 0L && Q == 0L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rm),Arrangement8B,0);
                }
                else if (size == 0L && Q == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rm),Arrangement16B,0);
                }
                else if (size == 1L && Q == 0L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rm),Arrangement4H,0);
                }
                else if (size == 1L && Q == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rm),Arrangement8H,0);
                }
                else if (size == 2L && Q == 0L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rm),Arrangement2S,0);
                }
                else if (size == 2L && Q == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rm),Arrangement4S,0);
                }
                else if (size == 3L && Q == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rm),Arrangement2D,0);
                }

                return null;
                goto __switch_break0;
            }
            if (aop == arg_Vm_arrangement_sz_Q___2S_00__4S_01__2D_11)
            {
                Rm = (x >> (int)(16L)) & (1L << (int)(5L) - 1L);
                sz = (x >> (int)(22L)) & 1L;
                Q = (x >> (int)(30L)) & 1L;
                if (sz == 0L && Q == 0L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rm),Arrangement2S,0);
                }
                else if (sz == 0L && Q == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rm),Arrangement4S,0);
                }
                else if (sz == 1L && Q == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rm),Arrangement2D,0);
                }

                return null;
                goto __switch_break0;
            }
            if (aop == arg_Vm_arrangement_sz___S_0__D_1_index__sz_L_H__HL_00__H_10_1)
            {
                a = default;
                index = default;
                Rm = (x >> (int)(16L)) & (1L << (int)(5L) - 1L);
                sz = (x >> (int)(22L)) & 1L;
                H = (x >> (int)(11L)) & 1L;
                L = (x >> (int)(21L)) & 1L;
                if (sz == 0L)
                {
                    a = ArrangementS;
                    index = (H << (int)(1L)) | L;
                }
                else if (sz == 1L && L == 0L)
                {
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
                immh = (x >> (int)(19L)) & (1L << (int)(4L) - 1L);
                Rn = (x >> (int)(5L)) & (1L << (int)(5L) - 1L);
                if (immh == 1L)
                {
                    return B0 + Reg(Rn);
                }
                else if (immh >> (int)(1L) == 1L)
                {
                    return H0 + Reg(Rn);
                }
                else if (immh >> (int)(2L) == 1L)
                {
                    return S0 + Reg(Rn);
                }
                else if (immh >> (int)(3L) == 1L)
                {
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
                immh = (x >> (int)(19L)) & (1L << (int)(4L) - 1L);
                Rn = (x >> (int)(5L)) & (1L << (int)(5L) - 1L);
                if (immh >> (int)(3L) == 1L)
                {
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
                immh = (x >> (int)(19L)) & (1L << (int)(4L) - 1L);
                Rn = (x >> (int)(5L)) & (1L << (int)(5L) - 1L);
                if (immh == 1L)
                {
                    return H0 + Reg(Rn);
                }
                else if (immh >> (int)(1L) == 1L)
                {
                    return S0 + Reg(Rn);
                }
                else if (immh >> (int)(2L) == 1L)
                {
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
                immh = (x >> (int)(19L)) & (1L << (int)(4L) - 1L);
                Rn = (x >> (int)(5L)) & (1L << (int)(5L) - 1L);
                if (immh >> (int)(2L) == 1L)
                {
                    return S0 + Reg(Rn);
                }
                else if (immh >> (int)(3L) == 1L)
                {
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
                Rn = (x >> (int)(5L)) & (1L << (int)(5L) - 1L);
                return new RegisterWithArrangement(V0+Reg(Rn),Arrangement16B,1);
                goto __switch_break0;
            }
            if (aop == arg_Vn_22_1__D_1)
            {
                sz = (x >> (int)(22L)) & 1L;
                Rn = (x >> (int)(5L)) & (1L << (int)(5L) - 1L);
                if (sz == 1L)
                {
                    return D0 + Reg(Rn);
                }

                return null;
                goto __switch_break0;
            }
            if (aop == arg_Vn_22_1__S_0__D_1)
            {
                sz = (x >> (int)(22L)) & 1L;
                Rn = (x >> (int)(5L)) & (1L << (int)(5L) - 1L);
                if (sz == 0L)
                {
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
                size = (x >> (int)(22L)) & (1L << (int)(2L) - 1L);
                Rn = (x >> (int)(5L)) & (1L << (int)(5L) - 1L);
                if (size == 0L)
                {
                    return B0 + Reg(Rn);
                }
                else if (size == 1L)
                {
                    return H0 + Reg(Rn);
                }
                else if (size == 2L)
                {
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
                size = (x >> (int)(22L)) & (1L << (int)(2L) - 1L);
                Rn = (x >> (int)(5L)) & (1L << (int)(5L) - 1L);
                if (size == 3L)
                {
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
                size = (x >> (int)(22L)) & (1L << (int)(2L) - 1L);
                Rn = (x >> (int)(5L)) & (1L << (int)(5L) - 1L);
                if (size == 0L)
                {
                    return H0 + Reg(Rn);
                }
                else if (size == 1L)
                {
                    return S0 + Reg(Rn);
                }
                else if (size == 2L)
                {
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
                size = (x >> (int)(22L)) & (1L << (int)(2L) - 1L);
                Rn = (x >> (int)(5L)) & (1L << (int)(5L) - 1L);
                if (size == 1L)
                {
                    return H0 + Reg(Rn);
                }
                else if (size == 2L)
                {
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
                Rn = (x >> (int)(5L)) & (1L << (int)(5L) - 1L);
                return new RegisterWithArrangement(V0+Reg(Rn),Arrangement16B,2);
                goto __switch_break0;
            }
            if (aop == arg_Vn_3_arrangement_16B)
            {
                Rn = (x >> (int)(5L)) & (1L << (int)(5L) - 1L);
                return new RegisterWithArrangement(V0+Reg(Rn),Arrangement16B,3);
                goto __switch_break0;
            }
            if (aop == arg_Vn_4_arrangement_16B)
            {
                Rn = (x >> (int)(5L)) & (1L << (int)(5L) - 1L);
                return new RegisterWithArrangement(V0+Reg(Rn),Arrangement16B,4);
                goto __switch_break0;
            }
            if (aop == arg_Vn_arrangement_16B)
            {
                Rn = (x >> (int)(5L)) & (1L << (int)(5L) - 1L);
                return new RegisterWithArrangement(V0+Reg(Rn),Arrangement16B,0);
                goto __switch_break0;
            }
            if (aop == arg_Vn_arrangement_4S)
            {
                Rn = (x >> (int)(5L)) & (1L << (int)(5L) - 1L);
                return new RegisterWithArrangement(V0+Reg(Rn),Arrangement4S,0);
                goto __switch_break0;
            }
            if (aop == arg_Vn_arrangement_D_index__1)
            {
                Rn = (x >> (int)(5L)) & (1L << (int)(5L) - 1L);
                return new RegisterWithArrangementAndIndex(V0+Reg(Rn),ArrangementD,1,0);
                goto __switch_break0;
            }
            if (aop == arg_Vn_arrangement_D_index__imm5_1)
            {
                Rn = (x >> (int)(5L)) & (1L << (int)(5L) - 1L);
                index = (x >> (int)(20L)) & 1L;
                return new RegisterWithArrangementAndIndex(V0+Reg(Rn),ArrangementD,uint8(index),0);
                goto __switch_break0;
            }
            if (aop == arg_Vn_arrangement_imm5___B_1__H_2_index__imm5__imm5lt41gt_1__imm5lt42gt_2_1)
            {
                a = default;
                index = default;
                Rn = (x >> (int)(5L)) & (1L << (int)(5L) - 1L);
                imm5 = (x >> (int)(16L)) & (1L << (int)(5L) - 1L);
                if (imm5 & 1L == 1L)
                {
                    a = ArrangementB;
                    index = imm5 >> (int)(1L);
                }
                else if (imm5 & 2L == 2L)
                {
                    a = ArrangementH;
                    index = imm5 >> (int)(2L);
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
                Rn = (x >> (int)(5L)) & (1L << (int)(5L) - 1L);
                imm5 = (x >> (int)(16L)) & (1L << (int)(5L) - 1L);
                imm4 = (x >> (int)(11L)) & (1L << (int)(4L) - 1L);
                if (imm5 & 1L == 1L)
                {
                    a = ArrangementB;
                    index = imm4;
                }
                else if (imm5 & 2L == 2L)
                {
                    a = ArrangementH;
                    index = imm4 >> (int)(1L);
                }
                else if (imm5 & 4L == 4L)
                {
                    a = ArrangementS;
                    index = imm4 >> (int)(2L);
                }
                else if (imm5 & 8L == 8L)
                {
                    a = ArrangementD;
                    index = imm4 >> (int)(3L);
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
                Rn = (x >> (int)(5L)) & (1L << (int)(5L) - 1L);
                imm5 = (x >> (int)(16L)) & (1L << (int)(5L) - 1L);
                if (imm5 & 1L == 1L)
                {
                    a = ArrangementB;
                    index = imm5 >> (int)(1L);
                }
                else if (imm5 & 2L == 2L)
                {
                    a = ArrangementH;
                    index = imm5 >> (int)(2L);
                }
                else if (imm5 & 4L == 4L)
                {
                    a = ArrangementS;
                    index = imm5 >> (int)(3L);
                }
                else if (imm5 & 8L == 8L)
                {
                    a = ArrangementD;
                    index = imm5 >> (int)(4L);
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
                Rn = (x >> (int)(5L)) & (1L << (int)(5L) - 1L);
                imm5 = (x >> (int)(16L)) & (1L << (int)(5L) - 1L);
                if (imm5 & 1L == 1L)
                {
                    a = ArrangementB;
                    index = imm5 >> (int)(1L);
                }
                else if (imm5 & 2L == 2L)
                {
                    a = ArrangementH;
                    index = imm5 >> (int)(2L);
                }
                else if (imm5 & 4L == 4L)
                {
                    a = ArrangementS;
                    index = imm5 >> (int)(3L);
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
                Rn = (x >> (int)(5L)) & (1L << (int)(5L) - 1L);
                imm5 = (x >> (int)(16L)) & (1L << (int)(5L) - 1L);
                if (imm5 & 15L == 8L)
                {
                    a = ArrangementD;
                    index = imm5 >> (int)(4L);
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
                Rn = (x >> (int)(5L)) & (1L << (int)(5L) - 1L);
                immh = (x >> (int)(19L)) & (1L << (int)(4L) - 1L);
                Q = (x >> (int)(30L)) & 1L;
                if (immh >> (int)(2L) == 1L)
                {
                    if (Q == 0L)
                    {
                        return new RegisterWithArrangement(V0+Reg(Rn),Arrangement2S,0);
                    }
                    else
                    {
                        return new RegisterWithArrangement(V0+Reg(Rn),Arrangement4S,0);
                    }

                }
                else if (immh >> (int)(3L) == 1L)
                {
                    if (Q == 1L)
                    {
                        return new RegisterWithArrangement(V0+Reg(Rn),Arrangement2D,0);
                    }

                }

                return null;
                goto __switch_break0;
            }
            if (aop == arg_Vn_arrangement_immh_Q___SEEAdvancedSIMDmodifiedimmediate_00__8B_10__16B_11__4H_20__8H_21__2S_40__4S_41)
            {
                Rn = (x >> (int)(5L)) & (1L << (int)(5L) - 1L);
                immh = (x >> (int)(19L)) & (1L << (int)(4L) - 1L);
                Q = (x >> (int)(30L)) & 1L;
                if (immh == 1L)
                {
                    if (Q == 0L)
                    {
                        return new RegisterWithArrangement(V0+Reg(Rn),Arrangement8B,0);
                    }
                    else
                    {
                        return new RegisterWithArrangement(V0+Reg(Rn),Arrangement16B,0);
                    }

                }
                else if (immh >> (int)(1L) == 1L)
                {
                    if (Q == 0L)
                    {
                        return new RegisterWithArrangement(V0+Reg(Rn),Arrangement4H,0);
                    }
                    else
                    {
                        return new RegisterWithArrangement(V0+Reg(Rn),Arrangement8H,0);
                    }

                }
                else if (immh >> (int)(2L) == 1L)
                {
                    if (Q == 0L)
                    {
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
                Rn = (x >> (int)(5L)) & (1L << (int)(5L) - 1L);
                immh = (x >> (int)(19L)) & (1L << (int)(4L) - 1L);
                Q = (x >> (int)(30L)) & 1L;
                if (immh == 1L)
                {
                    if (Q == 0L)
                    {
                        return new RegisterWithArrangement(V0+Reg(Rn),Arrangement8B,0);
                    }
                    else
                    {
                        return new RegisterWithArrangement(V0+Reg(Rn),Arrangement16B,0);
                    }

                }
                else if (immh >> (int)(1L) == 1L)
                {
                    if (Q == 0L)
                    {
                        return new RegisterWithArrangement(V0+Reg(Rn),Arrangement4H,0);
                    }
                    else
                    {
                        return new RegisterWithArrangement(V0+Reg(Rn),Arrangement8H,0);
                    }

                }
                else if (immh >> (int)(2L) == 1L)
                {
                    if (Q == 0L)
                    {
                        return new RegisterWithArrangement(V0+Reg(Rn),Arrangement2S,0);
                    }
                    else
                    {
                        return new RegisterWithArrangement(V0+Reg(Rn),Arrangement4S,0);
                    }

                }
                else if (immh >> (int)(3L) == 1L)
                {
                    if (Q == 1L)
                    {
                        return new RegisterWithArrangement(V0+Reg(Rn),Arrangement2D,0);
                    }

                }

                return null;
                goto __switch_break0;
            }
            if (aop == arg_Vn_arrangement_immh___SEEAdvancedSIMDmodifiedimmediate_0__8H_1__4S_2__2D_4)
            {
                Rn = (x >> (int)(5L)) & (1L << (int)(5L) - 1L);
                immh = (x >> (int)(19L)) & (1L << (int)(4L) - 1L);
                if (immh == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rn),Arrangement8H,0);
                }
                else if (immh >> (int)(1L) == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rn),Arrangement4S,0);
                }
                else if (immh >> (int)(2L) == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rn),Arrangement2D,0);
                }

                return null;
                goto __switch_break0;
            }
            if (aop == arg_Vn_arrangement_Q___8B_0__16B_1)
            {
                Rn = (x >> (int)(5L)) & (1L << (int)(5L) - 1L);
                Q = (x >> (int)(30L)) & 1L;
                if (Q == 0L)
                {
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
                Rn = (x >> (int)(5L)) & (1L << (int)(5L) - 1L);
                Q = (x >> (int)(30L)) & 1L;
                sz = (x >> (int)(22L)) & 1L;
                if (sz == 0L && Q == 0L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rn),Arrangement2S,0);
                }
                else if (sz == 0L && Q == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rn),Arrangement4S,0);
                }
                else if (sz == 1L && Q == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rn),Arrangement2D,0);
                }

                return null;
                goto __switch_break0;
            }
            if (aop == arg_Vn_arrangement_Q_sz___4S_10)
            {
                Rn = (x >> (int)(5L)) & (1L << (int)(5L) - 1L);
                Q = (x >> (int)(30L)) & 1L;
                sz = (x >> (int)(22L)) & 1L;
                if (sz == 0L && Q == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rn),Arrangement4S,0);
                }

                return null;
                goto __switch_break0;
            }
            if (aop == arg_Vn_arrangement_S_index__imm5__imm5lt41gt_1__imm5lt42gt_2__imm5lt43gt_4_1)
            {
                index = default;
                Rn = (x >> (int)(5L)) & (1L << (int)(5L) - 1L);
                imm5 = (x >> (int)(16L)) & (1L << (int)(5L) - 1L);
                index = imm5 >> (int)(3L);
                return new RegisterWithArrangementAndIndex(V0+Reg(Rn),ArrangementS,uint8(index),0);
                goto __switch_break0;
            }
            if (aop == arg_Vn_arrangement_size___2D_3)
            {
                Rn = (x >> (int)(5L)) & (1L << (int)(5L) - 1L);
                size = (x >> (int)(22L)) & 3L;
                if (size == 3L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rn),Arrangement2D,0);
                }

                return null;
                goto __switch_break0;
            }
            if (aop == arg_Vn_arrangement_size___8H_0__4S_1__2D_2)
            {
                Rn = (x >> (int)(5L)) & (1L << (int)(5L) - 1L);
                size = (x >> (int)(22L)) & 3L;
                if (size == 0L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rn),Arrangement8H,0);
                }
                else if (size == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rn),Arrangement4S,0);
                }
                else if (size == 2L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rn),Arrangement2D,0);
                }

                return null;
                goto __switch_break0;
            }
            if (aop == arg_Vn_arrangement_size_Q___4H_10__8H_11__2S_20__4S_21)
            {
                Rn = (x >> (int)(5L)) & (1L << (int)(5L) - 1L);
                size = (x >> (int)(22L)) & 3L;
                Q = (x >> (int)(30L)) & 1L;
                if (size == 1L && Q == 0L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rn),Arrangement4H,0);
                }
                else if (size == 1L && Q == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rn),Arrangement8H,0);
                }
                else if (size == 2L && Q == 0L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rn),Arrangement2S,0);
                }
                else if (size == 2L && Q == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rn),Arrangement4S,0);
                }

                return null;
                goto __switch_break0;
            }
            if (aop == arg_Vn_arrangement_size_Q___8B_00__16B_01)
            {
                Rn = (x >> (int)(5L)) & (1L << (int)(5L) - 1L);
                size = (x >> (int)(22L)) & 3L;
                Q = (x >> (int)(30L)) & 1L;
                if (size == 0L && Q == 0L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rn),Arrangement8B,0);
                }
                else if (size == 0L && Q == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rn),Arrangement16B,0);
                }

                return null;
                goto __switch_break0;
            }
            if (aop == arg_Vn_arrangement_size_Q___8B_00__16B_01__1D_30__2D_31)
            {
                Rn = (x >> (int)(5L)) & (1L << (int)(5L) - 1L);
                size = (x >> (int)(22L)) & 3L;
                Q = (x >> (int)(30L)) & 1L;
                if (size == 0L && Q == 0L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rn),Arrangement8B,0);
                }
                else if (size == 0L && Q == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rn),Arrangement16B,0);
                }
                else if (size == 3L && Q == 0L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rn),Arrangement1D,0);
                }
                else if (size == 3L && Q == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rn),Arrangement2D,0);
                }

                return null;
                goto __switch_break0;
            }
            if (aop == arg_Vn_arrangement_size_Q___8B_00__16B_01__4H_10__8H_11)
            {
                Rn = (x >> (int)(5L)) & (1L << (int)(5L) - 1L);
                size = (x >> (int)(22L)) & 3L;
                Q = (x >> (int)(30L)) & 1L;
                if (size == 0L && Q == 0L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rn),Arrangement8B,0);
                }
                else if (size == 0L && Q == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rn),Arrangement16B,0);
                }
                else if (size == 1L && Q == 0L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rn),Arrangement4H,0);
                }
                else if (size == 1L && Q == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rn),Arrangement8H,0);
                }

                return null;
                goto __switch_break0;
            }
            if (aop == arg_Vn_arrangement_size_Q___8B_00__16B_01__4H_10__8H_11__2S_20__4S_21)
            {
                Rn = (x >> (int)(5L)) & (1L << (int)(5L) - 1L);
                size = (x >> (int)(22L)) & 3L;
                Q = (x >> (int)(30L)) & 1L;
                if (size == 0L && Q == 0L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rn),Arrangement8B,0);
                }
                else if (size == 0L && Q == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rn),Arrangement16B,0);
                }
                else if (size == 1L && Q == 0L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rn),Arrangement4H,0);
                }
                else if (size == 1L && Q == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rn),Arrangement8H,0);
                }
                else if (size == 2L && Q == 0L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rn),Arrangement2S,0);
                }
                else if (size == 2L && Q == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rn),Arrangement4S,0);
                }

                return null;
                goto __switch_break0;
            }
            if (aop == arg_Vn_arrangement_size_Q___8B_00__16B_01__4H_10__8H_11__2S_20__4S_21__2D_31)
            {
                Rn = (x >> (int)(5L)) & (1L << (int)(5L) - 1L);
                size = (x >> (int)(22L)) & 3L;
                Q = (x >> (int)(30L)) & 1L;
                if (size == 0L && Q == 0L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rn),Arrangement8B,0);
                }
                else if (size == 0L && Q == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rn),Arrangement16B,0);
                }
                else if (size == 1L && Q == 0L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rn),Arrangement4H,0);
                }
                else if (size == 1L && Q == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rn),Arrangement8H,0);
                }
                else if (size == 2L && Q == 0L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rn),Arrangement2S,0);
                }
                else if (size == 2L && Q == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rn),Arrangement4S,0);
                }
                else if (size == 3L && Q == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rn),Arrangement2D,0);
                }

                return null;
                goto __switch_break0;
            }
            if (aop == arg_Vn_arrangement_size_Q___8B_00__16B_01__4H_10__8H_11__4S_21)
            {
                Rn = (x >> (int)(5L)) & (1L << (int)(5L) - 1L);
                size = (x >> (int)(22L)) & 3L;
                Q = (x >> (int)(30L)) & 1L;
                if (size == 0L && Q == 0L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rn),Arrangement8B,0);
                }
                else if (size == 0L && Q == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rn),Arrangement16B,0);
                }
                else if (size == 1L && Q == 0L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rn),Arrangement4H,0);
                }
                else if (size == 1L && Q == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rn),Arrangement8H,0);
                }
                else if (size == 2L && Q == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rn),Arrangement4S,0);
                }

                return null;
                goto __switch_break0;
            }
            if (aop == arg_Vn_arrangement_sz___2D_1)
            {
                Rn = (x >> (int)(5L)) & (1L << (int)(5L) - 1L);
                sz = (x >> (int)(22L)) & 1L;
                if (sz == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rn),Arrangement2D,0);
                }

                return null;
                goto __switch_break0;
            }
            if (aop == arg_Vn_arrangement_sz___2S_0__2D_1)
            {
                Rn = (x >> (int)(5L)) & (1L << (int)(5L) - 1L);
                sz = (x >> (int)(22L)) & 1L;
                if (sz == 0L)
                {
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
                Rn = (x >> (int)(5L)) & (1L << (int)(5L) - 1L);
                sz = (x >> (int)(22L)) & 1L;
                if (sz == 0L)
                {
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
                Rn = (x >> (int)(5L)) & (1L << (int)(5L) - 1L);
                sz = (x >> (int)(22L)) & 1L;
                Q = (x >> (int)(30L)) & 1L;
                if (sz == 0L && Q == 0L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rn),Arrangement2S,0);
                }
                else if (sz == 0L && Q == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rn),Arrangement4S,0);
                }

                return null;
                goto __switch_break0;
            }
            if (aop == arg_Vn_arrangement_sz_Q___2S_00__4S_01__2D_11)
            {
                Rn = (x >> (int)(5L)) & (1L << (int)(5L) - 1L);
                sz = (x >> (int)(22L)) & 1L;
                Q = (x >> (int)(30L)) & 1L;
                if (sz == 0L && Q == 0L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rn),Arrangement2S,0);
                }
                else if (sz == 0L && Q == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rn),Arrangement4S,0);
                }
                else if (sz == 1L && Q == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rn),Arrangement2D,0);
                }

                return null;
                goto __switch_break0;
            }
            if (aop == arg_Vn_arrangement_sz_Q___4H_00__8H_01__2S_10__4S_11)
            {
                Rn = (x >> (int)(5L)) & (1L << (int)(5L) - 1L);
                sz = (x >> (int)(22L)) & 1L;
                Q = (x >> (int)(30L)) & 1L;
                if (sz == 0L && Q == 0L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rn),Arrangement4H,0);
                }
                else if (sz == 0L && Q == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rn),Arrangement8H,0);
                }
                else if (sz == 1L && Q == 0L)
                {
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
                Rt = x & (1L << (int)(5L) - 1L);
                Q = (x >> (int)(30L)) & 1L;
                var S = (x >> (int)(12L)) & 1L;
                size = (x >> (int)(10L)) & 3L;
                index = (Q << (int)(3L)) | (S << (int)(2L)) | (size);
                return new RegisterWithArrangementAndIndex(V0+Reg(Rt),ArrangementB,uint8(index),1);
                goto __switch_break0;
            }
            if (aop == arg_Vt_1_arrangement_D_index__Q_1)
            {
                Rt = x & (1L << (int)(5L) - 1L);
                index = (x >> (int)(30L)) & 1L;
                return new RegisterWithArrangementAndIndex(V0+Reg(Rt),ArrangementD,uint8(index),1);
                goto __switch_break0;
            }
            if (aop == arg_Vt_1_arrangement_H_index__Q_S_size_1)
            {
                Rt = x & (1L << (int)(5L) - 1L);
                Q = (x >> (int)(30L)) & 1L;
                S = (x >> (int)(12L)) & 1L;
                size = (x >> (int)(11L)) & 1L;
                index = (Q << (int)(2L)) | (S << (int)(1L)) | (size);
                return new RegisterWithArrangementAndIndex(V0+Reg(Rt),ArrangementH,uint8(index),1);
                goto __switch_break0;
            }
            if (aop == arg_Vt_1_arrangement_S_index__Q_S_1)
            {
                Rt = x & (1L << (int)(5L) - 1L);
                Q = (x >> (int)(30L)) & 1L;
                S = (x >> (int)(12L)) & 1L;
                index = (Q << (int)(1L)) | S;
                return new RegisterWithArrangementAndIndex(V0+Reg(Rt),ArrangementS,uint8(index),1);
                goto __switch_break0;
            }
            if (aop == arg_Vt_1_arrangement_size_Q___8B_00__16B_01__4H_10__8H_11__2S_20__4S_21__1D_30__2D_31)
            {
                Rt = x & (1L << (int)(5L) - 1L);
                Q = (x >> (int)(30L)) & 1L;
                size = (x >> (int)(10L)) & 3L;
                if (size == 0L && Q == 0L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rt),Arrangement8B,1);
                }
                else if (size == 0L && Q == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rt),Arrangement16B,1);
                }
                else if (size == 1L && Q == 0L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rt),Arrangement4H,1);
                }
                else if (size == 1L && Q == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rt),Arrangement8H,1);
                }
                else if (size == 2L && Q == 0L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rt),Arrangement2S,1);
                }
                else if (size == 2L && Q == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rt),Arrangement4S,1);
                }
                else if (size == 3L && Q == 0L)
                {
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
                Rt = x & (1L << (int)(5L) - 1L);
                Q = (x >> (int)(30L)) & 1L;
                S = (x >> (int)(12L)) & 1L;
                size = (x >> (int)(10L)) & 3L;
                index = (Q << (int)(3L)) | (S << (int)(2L)) | (size);
                return new RegisterWithArrangementAndIndex(V0+Reg(Rt),ArrangementB,uint8(index),2);
                goto __switch_break0;
            }
            if (aop == arg_Vt_2_arrangement_D_index__Q_1)
            {
                Rt = x & (1L << (int)(5L) - 1L);
                index = (x >> (int)(30L)) & 1L;
                return new RegisterWithArrangementAndIndex(V0+Reg(Rt),ArrangementD,uint8(index),2);
                goto __switch_break0;
            }
            if (aop == arg_Vt_2_arrangement_H_index__Q_S_size_1)
            {
                Rt = x & (1L << (int)(5L) - 1L);
                Q = (x >> (int)(30L)) & 1L;
                S = (x >> (int)(12L)) & 1L;
                size = (x >> (int)(11L)) & 1L;
                index = (Q << (int)(2L)) | (S << (int)(1L)) | (size);
                return new RegisterWithArrangementAndIndex(V0+Reg(Rt),ArrangementH,uint8(index),2);
                goto __switch_break0;
            }
            if (aop == arg_Vt_2_arrangement_S_index__Q_S_1)
            {
                Rt = x & (1L << (int)(5L) - 1L);
                Q = (x >> (int)(30L)) & 1L;
                S = (x >> (int)(12L)) & 1L;
                index = (Q << (int)(1L)) | S;
                return new RegisterWithArrangementAndIndex(V0+Reg(Rt),ArrangementS,uint8(index),2);
                goto __switch_break0;
            }
            if (aop == arg_Vt_2_arrangement_size_Q___8B_00__16B_01__4H_10__8H_11__2S_20__4S_21__1D_30__2D_31)
            {
                Rt = x & (1L << (int)(5L) - 1L);
                Q = (x >> (int)(30L)) & 1L;
                size = (x >> (int)(10L)) & 3L;
                if (size == 0L && Q == 0L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rt),Arrangement8B,2);
                }
                else if (size == 0L && Q == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rt),Arrangement16B,2);
                }
                else if (size == 1L && Q == 0L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rt),Arrangement4H,2);
                }
                else if (size == 1L && Q == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rt),Arrangement8H,2);
                }
                else if (size == 2L && Q == 0L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rt),Arrangement2S,2);
                }
                else if (size == 2L && Q == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rt),Arrangement4S,2);
                }
                else if (size == 3L && Q == 0L)
                {
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
                Rt = x & (1L << (int)(5L) - 1L);
                Q = (x >> (int)(30L)) & 1L;
                size = (x >> (int)(10L)) & 3L;
                if (size == 0L && Q == 0L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rt),Arrangement8B,2);
                }
                else if (size == 0L && Q == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rt),Arrangement16B,2);
                }
                else if (size == 1L && Q == 0L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rt),Arrangement4H,2);
                }
                else if (size == 1L && Q == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rt),Arrangement8H,2);
                }
                else if (size == 2L && Q == 0L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rt),Arrangement2S,2);
                }
                else if (size == 2L && Q == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rt),Arrangement4S,2);
                }
                else if (size == 3L && Q == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rt),Arrangement2D,2);
                }

                return null;
                goto __switch_break0;
            }
            if (aop == arg_Vt_3_arrangement_B_index__Q_S_size_1)
            {
                Rt = x & (1L << (int)(5L) - 1L);
                Q = (x >> (int)(30L)) & 1L;
                S = (x >> (int)(12L)) & 1L;
                size = (x >> (int)(10L)) & 3L;
                index = (Q << (int)(3L)) | (S << (int)(2L)) | (size);
                return new RegisterWithArrangementAndIndex(V0+Reg(Rt),ArrangementB,uint8(index),3);
                goto __switch_break0;
            }
            if (aop == arg_Vt_3_arrangement_D_index__Q_1)
            {
                Rt = x & (1L << (int)(5L) - 1L);
                index = (x >> (int)(30L)) & 1L;
                return new RegisterWithArrangementAndIndex(V0+Reg(Rt),ArrangementD,uint8(index),3);
                goto __switch_break0;
            }
            if (aop == arg_Vt_3_arrangement_H_index__Q_S_size_1)
            {
                Rt = x & (1L << (int)(5L) - 1L);
                Q = (x >> (int)(30L)) & 1L;
                S = (x >> (int)(12L)) & 1L;
                size = (x >> (int)(11L)) & 1L;
                index = (Q << (int)(2L)) | (S << (int)(1L)) | (size);
                return new RegisterWithArrangementAndIndex(V0+Reg(Rt),ArrangementH,uint8(index),3);
                goto __switch_break0;
            }
            if (aop == arg_Vt_3_arrangement_S_index__Q_S_1)
            {
                Rt = x & (1L << (int)(5L) - 1L);
                Q = (x >> (int)(30L)) & 1L;
                S = (x >> (int)(12L)) & 1L;
                index = (Q << (int)(1L)) | S;
                return new RegisterWithArrangementAndIndex(V0+Reg(Rt),ArrangementS,uint8(index),3);
                goto __switch_break0;
            }
            if (aop == arg_Vt_3_arrangement_size_Q___8B_00__16B_01__4H_10__8H_11__2S_20__4S_21__1D_30__2D_31)
            {
                Rt = x & (1L << (int)(5L) - 1L);
                Q = (x >> (int)(30L)) & 1L;
                size = (x >> (int)(10L)) & 3L;
                if (size == 0L && Q == 0L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rt),Arrangement8B,3);
                }
                else if (size == 0L && Q == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rt),Arrangement16B,3);
                }
                else if (size == 1L && Q == 0L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rt),Arrangement4H,3);
                }
                else if (size == 1L && Q == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rt),Arrangement8H,3);
                }
                else if (size == 2L && Q == 0L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rt),Arrangement2S,3);
                }
                else if (size == 2L && Q == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rt),Arrangement4S,3);
                }
                else if (size == 3L && Q == 0L)
                {
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
                Rt = x & (1L << (int)(5L) - 1L);
                Q = (x >> (int)(30L)) & 1L;
                size = (x >> (int)(10L)) & 3L;
                if (size == 0L && Q == 0L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rt),Arrangement8B,3);
                }
                else if (size == 0L && Q == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rt),Arrangement16B,3);
                }
                else if (size == 1L && Q == 0L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rt),Arrangement4H,3);
                }
                else if (size == 1L && Q == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rt),Arrangement8H,3);
                }
                else if (size == 2L && Q == 0L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rt),Arrangement2S,3);
                }
                else if (size == 2L && Q == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rt),Arrangement4S,3);
                }
                else if (size == 3L && Q == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rt),Arrangement2D,3);
                }

                return null;
                goto __switch_break0;
            }
            if (aop == arg_Vt_4_arrangement_B_index__Q_S_size_1)
            {
                Rt = x & (1L << (int)(5L) - 1L);
                Q = (x >> (int)(30L)) & 1L;
                S = (x >> (int)(12L)) & 1L;
                size = (x >> (int)(10L)) & 3L;
                index = (Q << (int)(3L)) | (S << (int)(2L)) | (size);
                return new RegisterWithArrangementAndIndex(V0+Reg(Rt),ArrangementB,uint8(index),4);
                goto __switch_break0;
            }
            if (aop == arg_Vt_4_arrangement_D_index__Q_1)
            {
                Rt = x & (1L << (int)(5L) - 1L);
                index = (x >> (int)(30L)) & 1L;
                return new RegisterWithArrangementAndIndex(V0+Reg(Rt),ArrangementD,uint8(index),4);
                goto __switch_break0;
            }
            if (aop == arg_Vt_4_arrangement_H_index__Q_S_size_1)
            {
                Rt = x & (1L << (int)(5L) - 1L);
                Q = (x >> (int)(30L)) & 1L;
                S = (x >> (int)(12L)) & 1L;
                size = (x >> (int)(11L)) & 1L;
                index = (Q << (int)(2L)) | (S << (int)(1L)) | (size);
                return new RegisterWithArrangementAndIndex(V0+Reg(Rt),ArrangementH,uint8(index),4);
                goto __switch_break0;
            }
            if (aop == arg_Vt_4_arrangement_S_index__Q_S_1)
            {
                Rt = x & (1L << (int)(5L) - 1L);
                Q = (x >> (int)(30L)) & 1L;
                S = (x >> (int)(12L)) & 1L;
                index = (Q << (int)(1L)) | S;
                return new RegisterWithArrangementAndIndex(V0+Reg(Rt),ArrangementS,uint8(index),4);
                goto __switch_break0;
            }
            if (aop == arg_Vt_4_arrangement_size_Q___8B_00__16B_01__4H_10__8H_11__2S_20__4S_21__1D_30__2D_31)
            {
                Rt = x & (1L << (int)(5L) - 1L);
                Q = (x >> (int)(30L)) & 1L;
                size = (x >> (int)(10L)) & 3L;
                if (size == 0L && Q == 0L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rt),Arrangement8B,4);
                }
                else if (size == 0L && Q == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rt),Arrangement16B,4);
                }
                else if (size == 1L && Q == 0L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rt),Arrangement4H,4);
                }
                else if (size == 1L && Q == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rt),Arrangement8H,4);
                }
                else if (size == 2L && Q == 0L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rt),Arrangement2S,4);
                }
                else if (size == 2L && Q == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rt),Arrangement4S,4);
                }
                else if (size == 3L && Q == 0L)
                {
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
                Rt = x & (1L << (int)(5L) - 1L);
                Q = (x >> (int)(30L)) & 1L;
                size = (x >> (int)(10L)) & 3L;
                if (size == 0L && Q == 0L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rt),Arrangement8B,4);
                }
                else if (size == 0L && Q == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rt),Arrangement16B,4);
                }
                else if (size == 1L && Q == 0L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rt),Arrangement4H,4);
                }
                else if (size == 1L && Q == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rt),Arrangement8H,4);
                }
                else if (size == 2L && Q == 0L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rt),Arrangement2S,4);
                }
                else if (size == 2L && Q == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rt),Arrangement4S,4);
                }
                else if (size == 3L && Q == 1L)
                {
                    return new RegisterWithArrangement(V0+Reg(Rt),Arrangement2D,4);
                }

                return null;
                goto __switch_break0;
            }
            if (aop == arg_Xns_mem_extend_m__UXTW_2__LSL_3__SXTW_6__SXTX_7__0_0__4_1)
            {
                return handle_MemExtend(x, 4L, false);
                goto __switch_break0;
            }
            if (aop == arg_Xns_mem_offset)
            {
                Rn = RegSP(X0) + RegSP(x >> (int)(5L) & (1L << (int)(5L) - 1L));
                return new MemImmediate(Rn,AddrOffset,0);
                goto __switch_break0;
            }
            if (aop == arg_Xns_mem_optional_imm12_16_unsigned)
            {
                Rn = RegSP(X0) + RegSP(x >> (int)(5L) & (1L << (int)(5L) - 1L));
                imm12 = (x >> (int)(10L)) & (1L << (int)(12L) - 1L);
                return new MemImmediate(Rn,AddrOffset,int32(imm12<<4));
                goto __switch_break0;
            }
            if (aop == arg_Xns_mem_optional_imm7_16_signed)
            {
                Rn = RegSP(X0) + RegSP(x >> (int)(5L) & (1L << (int)(5L) - 1L));
                imm7 = (x >> (int)(15L)) & (1L << (int)(7L) - 1L);
                return new MemImmediate(Rn,AddrOffset,((int32(imm7<<4))<<21)>>21);
                goto __switch_break0;
            }
            if (aop == arg_Xns_mem_post_fixedimm_1)
            {
                Rn = RegSP(X0) + RegSP(x >> (int)(5L) & (1L << (int)(5L) - 1L));
                return new MemImmediate(Rn,AddrPostIndex,1);
                goto __switch_break0;
            }
            if (aop == arg_Xns_mem_post_fixedimm_12)
            {
                Rn = RegSP(X0) + RegSP(x >> (int)(5L) & (1L << (int)(5L) - 1L));
                return new MemImmediate(Rn,AddrPostIndex,12);
                goto __switch_break0;
            }
            if (aop == arg_Xns_mem_post_fixedimm_16)
            {
                Rn = RegSP(X0) + RegSP(x >> (int)(5L) & (1L << (int)(5L) - 1L));
                return new MemImmediate(Rn,AddrPostIndex,16);
                goto __switch_break0;
            }
            if (aop == arg_Xns_mem_post_fixedimm_2)
            {
                Rn = RegSP(X0) + RegSP(x >> (int)(5L) & (1L << (int)(5L) - 1L));
                return new MemImmediate(Rn,AddrPostIndex,2);
                goto __switch_break0;
            }
            if (aop == arg_Xns_mem_post_fixedimm_24)
            {
                Rn = RegSP(X0) + RegSP(x >> (int)(5L) & (1L << (int)(5L) - 1L));
                return new MemImmediate(Rn,AddrPostIndex,24);
                goto __switch_break0;
            }
            if (aop == arg_Xns_mem_post_fixedimm_3)
            {
                Rn = RegSP(X0) + RegSP(x >> (int)(5L) & (1L << (int)(5L) - 1L));
                return new MemImmediate(Rn,AddrPostIndex,3);
                goto __switch_break0;
            }
            if (aop == arg_Xns_mem_post_fixedimm_32)
            {
                Rn = RegSP(X0) + RegSP(x >> (int)(5L) & (1L << (int)(5L) - 1L));
                return new MemImmediate(Rn,AddrPostIndex,32);
                goto __switch_break0;
            }
            if (aop == arg_Xns_mem_post_fixedimm_4)
            {
                Rn = RegSP(X0) + RegSP(x >> (int)(5L) & (1L << (int)(5L) - 1L));
                return new MemImmediate(Rn,AddrPostIndex,4);
                goto __switch_break0;
            }
            if (aop == arg_Xns_mem_post_fixedimm_6)
            {
                Rn = RegSP(X0) + RegSP(x >> (int)(5L) & (1L << (int)(5L) - 1L));
                return new MemImmediate(Rn,AddrPostIndex,6);
                goto __switch_break0;
            }
            if (aop == arg_Xns_mem_post_fixedimm_8)
            {
                Rn = RegSP(X0) + RegSP(x >> (int)(5L) & (1L << (int)(5L) - 1L));
                return new MemImmediate(Rn,AddrPostIndex,8);
                goto __switch_break0;
            }
            if (aop == arg_Xns_mem_post_imm7_16_signed)
            {
                Rn = RegSP(X0) + RegSP(x >> (int)(5L) & (1L << (int)(5L) - 1L));
                imm7 = (x >> (int)(15L)) & (1L << (int)(7L) - 1L);
                return new MemImmediate(Rn,AddrPostIndex,((int32(imm7<<4))<<21)>>21);
                goto __switch_break0;
            }
            if (aop == arg_Xns_mem_post_Q__16_0__32_1)
            {
                Rn = RegSP(X0) + RegSP(x >> (int)(5L) & (1L << (int)(5L) - 1L));
                Q = (x >> (int)(30L)) & 1L;
                return new MemImmediate(Rn,AddrPostIndex,int32((Q+1)*16));
                goto __switch_break0;
            }
            if (aop == arg_Xns_mem_post_Q__24_0__48_1)
            {
                Rn = RegSP(X0) + RegSP(x >> (int)(5L) & (1L << (int)(5L) - 1L));
                Q = (x >> (int)(30L)) & 1L;
                return new MemImmediate(Rn,AddrPostIndex,int32((Q+1)*24));
                goto __switch_break0;
            }
            if (aop == arg_Xns_mem_post_Q__32_0__64_1)
            {
                Rn = RegSP(X0) + RegSP(x >> (int)(5L) & (1L << (int)(5L) - 1L));
                Q = (x >> (int)(30L)) & 1L;
                return new MemImmediate(Rn,AddrPostIndex,int32((Q+1)*32));
                goto __switch_break0;
            }
            if (aop == arg_Xns_mem_post_Q__8_0__16_1)
            {
                Rn = RegSP(X0) + RegSP(x >> (int)(5L) & (1L << (int)(5L) - 1L));
                Q = (x >> (int)(30L)) & 1L;
                return new MemImmediate(Rn,AddrPostIndex,int32((Q+1)*8));
                goto __switch_break0;
            }
            if (aop == arg_Xns_mem_post_size__1_0__2_1__4_2__8_3)
            {
                Rn = RegSP(X0) + RegSP(x >> (int)(5L) & (1L << (int)(5L) - 1L));
                size = (x >> (int)(10L)) & 3L;
                return new MemImmediate(Rn,AddrPostIndex,int32(1<<size));
                goto __switch_break0;
            }
            if (aop == arg_Xns_mem_post_size__2_0__4_1__8_2__16_3)
            {
                Rn = RegSP(X0) + RegSP(x >> (int)(5L) & (1L << (int)(5L) - 1L));
                size = (x >> (int)(10L)) & 3L;
                return new MemImmediate(Rn,AddrPostIndex,int32(2<<size));
                goto __switch_break0;
            }
            if (aop == arg_Xns_mem_post_size__3_0__6_1__12_2__24_3)
            {
                Rn = RegSP(X0) + RegSP(x >> (int)(5L) & (1L << (int)(5L) - 1L));
                size = (x >> (int)(10L)) & 3L;
                return new MemImmediate(Rn,AddrPostIndex,int32(3<<size));
                goto __switch_break0;
            }
            if (aop == arg_Xns_mem_post_size__4_0__8_1__16_2__32_3)
            {
                Rn = RegSP(X0) + RegSP(x >> (int)(5L) & (1L << (int)(5L) - 1L));
                size = (x >> (int)(10L)) & 3L;
                return new MemImmediate(Rn,AddrPostIndex,int32(4<<size));
                goto __switch_break0;
            }
            if (aop == arg_Xns_mem_post_Xm)
            {
                Rn = RegSP(X0) + RegSP(x >> (int)(5L) & (1L << (int)(5L) - 1L));
                Rm = (x >> (int)(16L)) & (1L << (int)(5L) - 1L);
                return new MemImmediate(Rn,AddrPostReg,int32(Rm));
                goto __switch_break0;
            }
            if (aop == arg_Xns_mem_wb_imm7_16_signed)
            {
                Rn = RegSP(X0) + RegSP(x >> (int)(5L) & (1L << (int)(5L) - 1L));
                imm7 = (x >> (int)(15L)) & (1L << (int)(7L) - 1L);
                return new MemImmediate(Rn,AddrPreIndex,((int32(imm7<<4))<<21)>>21);
                goto __switch_break0;
            }
            // default: 
                return null;

            __switch_break0:;

        }

        private static Arg handle_ExtendedRegister(uint x, bool has_width)
        {
            var s = (x >> (int)(29L)) & 1L;
            var rm = (x >> (int)(16L)) & (1L << (int)(5L) - 1L);
            var option = (x >> (int)(13L)) & (1L << (int)(3L) - 1L);
            var imm3 = (x >> (int)(10L)) & (1L << (int)(3L) - 1L);
            var rn = (x >> (int)(5L)) & (1L << (int)(5L) - 1L);
            var rd = x & (1L << (int)(5L) - 1L);
            var is_32bit = !has_width;
            RegExtshiftAmount rea = default;
            if (has_width)
            {
                if (option & 0x3UL != 0x3UL)
                {
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

            switch (option)
            {
                case 0L: 
                    rea.extShift = uxtb;
                    break;
                case 1L: 
                    rea.extShift = uxth;
                    break;
                case 2L: 
                    if (is_32bit && (rn == 31L || (s == 0L && rd == 31L)))
                    {
                        if (imm3 != 0L)
                        {
                            rea.extShift = lsl;
                        }
                        else
                        {
                            rea.extShift = ExtShift(0L);
                        }

                    }
                    else
                    {
                        rea.extShift = uxtw;
                    }

                    break;
                case 3L: 
                    if (!is_32bit && (rn == 31L || (s == 0L && rd == 31L)))
                    {
                        if (imm3 != 0L)
                        {
                            rea.extShift = lsl;
                        }
                        else
                        {
                            rea.extShift = ExtShift(0L);
                        }

                    }
                    else
                    {
                        rea.extShift = uxtx;
                    }

                    break;
                case 4L: 
                    rea.extShift = sxtb;
                    break;
                case 5L: 
                    rea.extShift = sxth;
                    break;
                case 6L: 
                    rea.extShift = sxtw;
                    break;
                case 7L: 
                    rea.extShift = sxtx;
                    break;
            }
            rea.show_zero = false;
            rea.amount = uint8(imm3);
            return rea;

        }

        private static Arg handle_ImmediateShiftedRegister(uint x, byte max, bool is_w, bool has_ror)
        {
            RegExtshiftAmount rsa = default;
            if (is_w)
            {
                rsa.reg = W0 + Reg((x >> (int)(16L)) & (1L << (int)(5L) - 1L));
            }
            else
            {
                rsa.reg = X0 + Reg((x >> (int)(16L)) & (1L << (int)(5L) - 1L));
            }

            switch ((x >> (int)(22L)) & 0x3UL)
            {
                case 0L: 
                    rsa.extShift = lsl;
                    break;
                case 1L: 
                    rsa.extShift = lsr;
                    break;
                case 2L: 
                    rsa.extShift = asr;
                    break;
                case 3L: 
                    if (has_ror)
                    {
                        rsa.extShift = ror;
                    }
                    else
                    {
                        return null;
                    }

                    break;
            }
            rsa.show_zero = true;
            rsa.amount = uint8((x >> (int)(10L)) & (1L << (int)(6L) - 1L));
            if (rsa.amount == 0L && rsa.extShift == lsl)
            {
                rsa.extShift = ExtShift(0L);
            }
            else if (rsa.amount > max)
            {
                return null;
            }

            return rsa;

        }

        private static Arg handle_MemExtend(uint x, byte mult, bool absent)
        {
            ExtShift extend = default;
            Reg Rm = default;
            var option = (x >> (int)(13L)) & (1L << (int)(3L) - 1L);
            var Rn = RegSP(X0) + RegSP(x >> (int)(5L) & (1L << (int)(5L) - 1L));
            if ((option & 1L) != 0L)
            {
                Rm = Reg(X0) + Reg(x >> (int)(16L) & (1L << (int)(5L) - 1L));
            }
            else
            {
                Rm = Reg(W0) + Reg(x >> (int)(16L) & (1L << (int)(5L) - 1L));
            }

            switch (option)
            {
                case 2L: 
                    extend = uxtw;
                    break;
                case 3L: 
                    extend = lsl;
                    break;
                case 6L: 
                    extend = sxtw;
                    break;
                case 7L: 
                    extend = sxtx;
                    break;
                default: 
                    return null;
                    break;
            }
            var amount = (uint8((x >> (int)(12L)) & 1L)) * mult;
            return new MemExtend(Rn,Rm,extend,amount,absent);

        }

        private static Arg handle_bitmasks(uint x, byte datasize)
        {
            byte length = default;            byte levels = default;            byte esize = default;            byte i = default;

            ulong welem = default;            ulong wmask = default;

            var n = (x >> (int)(22L)) & 1L;
            var imms = uint8((x >> (int)(10L)) & (1L << (int)(6L) - 1L));
            var immr = uint8((x >> (int)(16L)) & (1L << (int)(6L) - 1L));
            if (n != 0L)
            {
                length = 6L;
            }
            else if ((imms & 32L) == 0L)
            {
                length = 5L;
            }
            else if ((imms & 16L) == 0L)
            {
                length = 4L;
            }
            else if ((imms & 8L) == 0L)
            {
                length = 3L;
            }
            else if ((imms & 4L) == 0L)
            {
                length = 2L;
            }
            else if ((imms & 2L) == 0L)
            {
                length = 1L;
            }
            else
            {
                return null;
            }

            levels = 1L << (int)(length) - 1L;
            var s = imms & levels;
            var r = immr & levels;
            esize = 1L << (int)(length);
            if (esize > datasize)
            {
                return null;
            }

            welem = 1L << (int)((s + 1L)) - 1L;
            var ror = (welem >> (int)(r)) | (welem << (int)((esize - r)));
            ror &= ((1L << (int)(esize)) - 1L);
            wmask = 0L;
            i = 0L;

            while (i < datasize)
            {
                wmask = (wmask << (int)(esize)) | ror;
                i += esize;
            }

            return new Imm64(wmask,false);

        }
    }
}}}}}}}
