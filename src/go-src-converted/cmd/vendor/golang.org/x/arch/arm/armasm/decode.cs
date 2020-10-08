// Copyright 2014 The Go Authors.  All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package armasm -- go2cs converted at 2020 October 08 04:44:08 UTC
// import "cmd/vendor/golang.org/x/arch/arm/armasm" ==> using armasm = go.cmd.vendor.golang.org.x.arch.arm.armasm_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\arch\arm\armasm\decode.go
using binary = go.encoding.binary_package;
using fmt = go.fmt_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace vendor {
namespace golang.org {
namespace x {
namespace arch {
namespace arm
{
    public static partial class armasm_package
    {
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
        private partial struct instFormat
        {
            public uint mask;
            public uint value;
            public sbyte priority;
            public Op op;
            public ulong opBits;
            public instArgs args;
        }

        private partial struct instArgs // : array<instArg>
        {
        }

        private static var errMode = fmt.Errorf("unsupported execution mode");        private static var errShort = fmt.Errorf("truncated instruction");        private static var errUnknown = fmt.Errorf("unknown instruction");

        private static slice<bool> decoderCover = default;

        // Decode decodes the leading bytes in src as a single instruction.
        public static (Inst, error) Decode(slice<byte> src, Mode mode)
        {
            Inst inst = default;
            error err = default!;

            if (mode != ModeARM)
            {
                return (new Inst(), error.As(errMode)!);
            }

            if (len(src) < 4L)
            {
                return (new Inst(), error.As(errShort)!);
            }

            if (decoderCover == null)
            {
                decoderCover = make_slice<bool>(len(instFormats));
            }

            var x = binary.LittleEndian.Uint32(src); 

            // The instFormat table contains both conditional and unconditional instructions.
            // Considering only the top 4 bits, the conditional instructions use mask=0, value=0,
            // while the unconditional instructions use mask=f, value=f.
            // Prepare a version of x with the condition cleared to 0 in conditional instructions
            // and then assume mask=f during matching.
            const ulong condMask = (ulong)0xf0000000UL;

            var xNoCond = x;
            if (x & condMask != condMask)
            {
                xNoCond &= condMask;
            }

            sbyte priority = default;
Search:
            foreach (var (i) in instFormats)
            {
                var f = _addr_instFormats[i];
                if (xNoCond & (f.mask | condMask) != f.value || f.priority <= priority)
                {
                    continue;
                }

                var delta = uint32(0L);
                var deltaShift = uint(0L);
                {
                    var opBits = f.opBits;

                    while (opBits != 0L)
                    {
                        var n = uint(opBits & 0xFFUL);
                        var off = uint((opBits >> (int)(8L)) & 0xFFUL);
                        delta |= (x >> (int)(off)) & (1L << (int)(n) - 1L) << (int)(deltaShift);
                        deltaShift += n;
                        opBits >>= 16L;
                    }

                }
                var op = f.op + Op(delta); 

                // Special case: BKPT encodes with condition but cannot have one.
                if (op & ~15L == BKPT_EQ && op != BKPT)
                {
                    _continueSearch = true;
                    break;
                }

                Args args = default;
                foreach (var (j, aop) in f.args)
                {
                    if (aop == 0L)
                    {
                        break;
                    }

                    var arg = decodeArg(aop, x);
                    if (arg == null)
                    { // cannot decode argument
                        _continueSearch = true;
                        break;
                    }

                    args[j] = arg;

                }
                decoderCover[i] = true;

                inst = new Inst(Op:op,Args:args,Enc:x,Len:4,);
                priority = f.priority;
                _continueSearch = true;
                break;
            }
            if (inst.Op != 0L)
            {
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
        private partial struct instArg // : byte
        {
        }

        private static readonly instArg _ = (instArg)iota;
        private static readonly var arg_APSR = (var)0;
        private static readonly var arg_FPSCR = (var)1;
        private static readonly var arg_Dn_half = (var)2;
        private static readonly var arg_R1_0 = (var)3;
        private static readonly var arg_R1_12 = (var)4;
        private static readonly var arg_R2_0 = (var)5;
        private static readonly var arg_R2_12 = (var)6;
        private static readonly var arg_R_0 = (var)7;
        private static readonly var arg_R_12 = (var)8;
        private static readonly var arg_R_12_nzcv = (var)9;
        private static readonly var arg_R_16 = (var)10;
        private static readonly var arg_R_16_WB = (var)11;
        private static readonly var arg_R_8 = (var)12;
        private static readonly var arg_R_rotate = (var)13;
        private static readonly var arg_R_shift_R = (var)14;
        private static readonly var arg_R_shift_imm = (var)15;
        private static readonly var arg_SP = (var)16;
        private static readonly var arg_Sd = (var)17;
        private static readonly var arg_Sd_Dd = (var)18;
        private static readonly var arg_Dd_Sd = (var)19;
        private static readonly var arg_Sm = (var)20;
        private static readonly var arg_Sm_Dm = (var)21;
        private static readonly var arg_Sn = (var)22;
        private static readonly var arg_Sn_Dn = (var)23;
        private static readonly var arg_const = (var)24;
        private static readonly var arg_endian = (var)25;
        private static readonly var arg_fbits = (var)26;
        private static readonly var arg_fp_0 = (var)27;
        private static readonly var arg_imm24 = (var)28;
        private static readonly var arg_imm5 = (var)29;
        private static readonly var arg_imm5_32 = (var)30;
        private static readonly var arg_imm5_nz = (var)31;
        private static readonly var arg_imm_12at8_4at0 = (var)32;
        private static readonly var arg_imm_4at16_12at0 = (var)33;
        private static readonly var arg_imm_vfp = (var)34;
        private static readonly var arg_label24 = (var)35;
        private static readonly var arg_label24H = (var)36;
        private static readonly var arg_label_m_12 = (var)37;
        private static readonly var arg_label_p_12 = (var)38;
        private static readonly var arg_label_pm_12 = (var)39;
        private static readonly var arg_label_pm_4_4 = (var)40;
        private static readonly var arg_lsb_width = (var)41;
        private static readonly var arg_mem_R = (var)42;
        private static readonly var arg_mem_R_pm_R_W = (var)43;
        private static readonly var arg_mem_R_pm_R_postindex = (var)44;
        private static readonly var arg_mem_R_pm_R_shift_imm_W = (var)45;
        private static readonly var arg_mem_R_pm_R_shift_imm_offset = (var)46;
        private static readonly var arg_mem_R_pm_R_shift_imm_postindex = (var)47;
        private static readonly var arg_mem_R_pm_imm12_W = (var)48;
        private static readonly var arg_mem_R_pm_imm12_offset = (var)49;
        private static readonly var arg_mem_R_pm_imm12_postindex = (var)50;
        private static readonly var arg_mem_R_pm_imm8_W = (var)51;
        private static readonly var arg_mem_R_pm_imm8_postindex = (var)52;
        private static readonly var arg_mem_R_pm_imm8at0_offset = (var)53;
        private static readonly var arg_option = (var)54;
        private static readonly var arg_registers = (var)55;
        private static readonly var arg_registers1 = (var)56;
        private static readonly var arg_registers2 = (var)57;
        private static readonly var arg_satimm4 = (var)58;
        private static readonly var arg_satimm5 = (var)59;
        private static readonly var arg_satimm4m1 = (var)60;
        private static readonly var arg_satimm5m1 = (var)61;
        private static readonly var arg_widthm1 = (var)62;


        // decodeArg decodes the arg described by aop from the instruction bits x.
        // It returns nil if x cannot be decoded according to aop.
        private static Arg decodeArg(instArg aop, uint x)
        {

            if (aop == arg_APSR) 
                return APSR;
            else if (aop == arg_FPSCR) 
                return FPSCR;
            else if (aop == arg_R_0) 
                return Reg(x & (1L << (int)(4L) - 1L));
            else if (aop == arg_R_8) 
                return Reg((x >> (int)(8L)) & (1L << (int)(4L) - 1L));
            else if (aop == arg_R_12) 
                return Reg((x >> (int)(12L)) & (1L << (int)(4L) - 1L));
            else if (aop == arg_R_16) 
                return Reg((x >> (int)(16L)) & (1L << (int)(4L) - 1L));
            else if (aop == arg_R_12_nzcv) 
                var r = Reg((x >> (int)(12L)) & (1L << (int)(4L) - 1L));
                if (r == R15)
                {
                    return APSR_nzcv;
                }

                return r;
            else if (aop == arg_R_16_WB) 
                var mode = AddrLDM;
                if ((x >> (int)(21L)) & 1L != 0L)
                {
                    mode = AddrLDM_WB;
                }

                return new Mem(Base:Reg((x>>16)&(1<<4-1)),Mode:mode);
            else if (aop == arg_R_rotate) 
                var Rm = Reg(x & (1L << (int)(4L) - 1L));
                var (typ, count) = decodeShift(x); 
                // ROR #0 here means ROR #0, but decodeShift rewrites to RRX #1.
                if (typ == RotateRightExt)
                {
                    return Reg(Rm);
                }

                return new RegShift(Rm,typ,uint8(count));
            else if (aop == arg_R_shift_R) 
                Rm = Reg(x & (1L << (int)(4L) - 1L));
                var Rs = Reg((x >> (int)(8L)) & (1L << (int)(4L) - 1L));
                var typ = Shift((x >> (int)(5L)) & (1L << (int)(2L) - 1L));
                return new RegShiftReg(Rm,typ,Rs);
            else if (aop == arg_R_shift_imm) 
                Rm = Reg(x & (1L << (int)(4L) - 1L));
                (typ, count) = decodeShift(x);
                if (typ == ShiftLeft && count == 0L)
                {
                    return Reg(Rm);
                }

                return new RegShift(Rm,typ,uint8(count));
            else if (aop == arg_R1_0) 
                return Reg((x & (1L << (int)(4L) - 1L)));
            else if (aop == arg_R1_12) 
                return Reg(((x >> (int)(12L)) & (1L << (int)(4L) - 1L)));
            else if (aop == arg_R2_0) 
                return Reg((x & (1L << (int)(4L) - 1L)) | 1L);
            else if (aop == arg_R2_12) 
                return Reg(((x >> (int)(12L)) & (1L << (int)(4L) - 1L)) | 1L);
            else if (aop == arg_SP) 
                return SP;
            else if (aop == arg_Sd_Dd) 
                var v = (x >> (int)(12L)) & (1L << (int)(4L) - 1L);
                var vx = (x >> (int)(22L)) & 1L;
                var sz = (x >> (int)(8L)) & 1L;
                if (sz != 0L)
                {
                    return D0 + Reg(vx << (int)(4L) + v);
                }
                else
                {
                    return S0 + Reg(v << (int)(1L) + vx);
                }

            else if (aop == arg_Dd_Sd) 
                return decodeArg(arg_Sd_Dd, x ^ (1L << (int)(8L)));
            else if (aop == arg_Sd) 
                v = (x >> (int)(12L)) & (1L << (int)(4L) - 1L);
                vx = (x >> (int)(22L)) & 1L;
                return S0 + Reg(v << (int)(1L) + vx);
            else if (aop == arg_Sm_Dm) 
                v = (x >> (int)(0L)) & (1L << (int)(4L) - 1L);
                vx = (x >> (int)(5L)) & 1L;
                sz = (x >> (int)(8L)) & 1L;
                if (sz != 0L)
                {
                    return D0 + Reg(vx << (int)(4L) + v);
                }
                else
                {
                    return S0 + Reg(v << (int)(1L) + vx);
                }

            else if (aop == arg_Sm) 
                v = (x >> (int)(0L)) & (1L << (int)(4L) - 1L);
                vx = (x >> (int)(5L)) & 1L;
                return S0 + Reg(v << (int)(1L) + vx);
            else if (aop == arg_Dn_half) 
                v = (x >> (int)(16L)) & (1L << (int)(4L) - 1L);
                vx = (x >> (int)(7L)) & 1L;
                return new RegX(D0+Reg(vx<<4+v),int((x>>21)&1));
            else if (aop == arg_Sn_Dn) 
                v = (x >> (int)(16L)) & (1L << (int)(4L) - 1L);
                vx = (x >> (int)(7L)) & 1L;
                sz = (x >> (int)(8L)) & 1L;
                if (sz != 0L)
                {
                    return D0 + Reg(vx << (int)(4L) + v);
                }
                else
                {
                    return S0 + Reg(v << (int)(1L) + vx);
                }

            else if (aop == arg_Sn) 
                v = (x >> (int)(16L)) & (1L << (int)(4L) - 1L);
                vx = (x >> (int)(7L)) & 1L;
                return S0 + Reg(v << (int)(1L) + vx);
            else if (aop == arg_const) 
                v = x & (1L << (int)(8L) - 1L);
                var rot = (x >> (int)(8L)) & (1L << (int)(4L) - 1L) * 2L;
                if (rot > 0L && v & 3L == 0L)
                { 
                    // could rotate less
                    return new ImmAlt(uint8(v),uint8(rot));

                }

                if (rot >= 24L && ((v << (int)((32L - rot))) & 0xFFUL) >> (int)((32L - rot)) == v)
                { 
                    // could wrap around to rot==0.
                    return new ImmAlt(uint8(v),uint8(rot));

                }

                return Imm(v >> (int)(rot) | v << (int)((32L - rot)));
            else if (aop == arg_endian) 
                return Endian((x >> (int)(9L)) & 1L);
            else if (aop == arg_fbits) 
                return Imm((16L << (int)(((x >> (int)(7L)) & 1L))) - ((x & (1L << (int)(4L) - 1L)) << (int)(1L) | (x >> (int)(5L)) & 1L));
            else if (aop == arg_fp_0) 
                return Imm(0L);
            else if (aop == arg_imm24) 
                return Imm(x & (1L << (int)(24L) - 1L));
            else if (aop == arg_imm5) 
                return Imm((x >> (int)(7L)) & (1L << (int)(5L) - 1L));
            else if (aop == arg_imm5_32) 
                x = (x >> (int)(7L)) & (1L << (int)(5L) - 1L);
                if (x == 0L)
                {
                    x = 32L;
                }

                return Imm(x);
            else if (aop == arg_imm5_nz) 
                x = (x >> (int)(7L)) & (1L << (int)(5L) - 1L);
                if (x == 0L)
                {
                    return null;
                }

                return Imm(x);
            else if (aop == arg_imm_4at16_12at0) 
                return Imm((x >> (int)(16L)) & (1L << (int)(4L) - 1L) << (int)(12L) | x & (1L << (int)(12L) - 1L));
            else if (aop == arg_imm_12at8_4at0) 
                return Imm((x >> (int)(8L)) & (1L << (int)(12L) - 1L) << (int)(4L) | x & (1L << (int)(4L) - 1L));
            else if (aop == arg_imm_vfp) 
                x = (x >> (int)(16L)) & (1L << (int)(4L) - 1L) << (int)(4L) | x & (1L << (int)(4L) - 1L);
                return Imm(x);
            else if (aop == arg_label24) 
                var imm = (x & (1L << (int)(24L) - 1L)) << (int)(2L);
                return PCRel(int32(imm << (int)(6L)) >> (int)(6L));
            else if (aop == arg_label24H) 
                var h = (x >> (int)(24L)) & 1L;
                imm = (x & (1L << (int)(24L) - 1L)) << (int)(2L) | h << (int)(1L);
                return PCRel(int32(imm << (int)(6L)) >> (int)(6L));
            else if (aop == arg_label_m_12) 
                var d = int32(x & (1L << (int)(12L) - 1L));
                return new Mem(Base:PC,Mode:AddrOffset,Offset:int16(-d));
            else if (aop == arg_label_p_12) 
                d = int32(x & (1L << (int)(12L) - 1L));
                return new Mem(Base:PC,Mode:AddrOffset,Offset:int16(d));
            else if (aop == arg_label_pm_12) 
                d = int32(x & (1L << (int)(12L) - 1L));
                var u = (x >> (int)(23L)) & 1L;
                if (u == 0L)
                {
                    d = -d;
                }

                return new Mem(Base:PC,Mode:AddrOffset,Offset:int16(d));
            else if (aop == arg_label_pm_4_4) 
                d = int32((x >> (int)(8L)) & (1L << (int)(4L) - 1L) << (int)(4L) | x & (1L << (int)(4L) - 1L));
                u = (x >> (int)(23L)) & 1L;
                if (u == 0L)
                {
                    d = -d;
                }

                return PCRel(d);
            else if (aop == arg_lsb_width) 
                var lsb = (x >> (int)(7L)) & (1L << (int)(5L) - 1L);
                var msb = (x >> (int)(16L)) & (1L << (int)(5L) - 1L);
                if (msb < lsb || msb >= 32L)
                {
                    return null;
                }

                return Imm(msb + 1L - lsb);
            else if (aop == arg_mem_R) 
                var Rn = Reg((x >> (int)(16L)) & (1L << (int)(4L) - 1L));
                return new Mem(Base:Rn,Mode:AddrOffset);
            else if (aop == arg_mem_R_pm_R_postindex) 
                // Treat [<Rn>],+/-<Rm> like [<Rn>,+/-<Rm>{,<shift>}]{!}
                // by forcing shift bits to <<0 and P=0, W=0 (postindex=true).
                return decodeArg(arg_mem_R_pm_R_shift_imm_W, x & ~((1L << (int)(7L) - 1L) << (int)(5L) | 1L << (int)(24L) | 1L << (int)(21L)));
            else if (aop == arg_mem_R_pm_R_W) 
                // Treat [<Rn>,+/-<Rm>]{!} like [<Rn>,+/-<Rm>{,<shift>}]{!}
                // by forcing shift bits to <<0.
                return decodeArg(arg_mem_R_pm_R_shift_imm_W, x & ~((1L << (int)(7L) - 1L) << (int)(5L)));
            else if (aop == arg_mem_R_pm_R_shift_imm_offset) 
                // Treat [<Rn>],+/-<Rm>{,<shift>} like [<Rn>,+/-<Rm>{,<shift>}]{!}
                // by forcing P=1, W=0 (index=false, wback=false).
                return decodeArg(arg_mem_R_pm_R_shift_imm_W, x & ~(1L << (int)(21L)) | 1L << (int)(24L));
            else if (aop == arg_mem_R_pm_R_shift_imm_postindex) 
                // Treat [<Rn>],+/-<Rm>{,<shift>} like [<Rn>,+/-<Rm>{,<shift>}]{!}
                // by forcing P=0, W=0 (postindex=true).
                return decodeArg(arg_mem_R_pm_R_shift_imm_W, x & ~(1L << (int)(24L) | 1L << (int)(21L)));
            else if (aop == arg_mem_R_pm_R_shift_imm_W) 
                Rn = Reg((x >> (int)(16L)) & (1L << (int)(4L) - 1L));
                Rm = Reg(x & (1L << (int)(4L) - 1L));
                (typ, count) = decodeShift(x);
                u = (x >> (int)(23L)) & 1L;
                var w = (x >> (int)(21L)) & 1L;
                var p = (x >> (int)(24L)) & 1L;
                if (p == 0L && w == 1L)
                {
                    return null;
                }

                var sign = int8(+1L);
                if (u == 0L)
                {
                    sign = -1L;
                }

                mode = AddrMode(uint8(p << (int)(1L)) | uint8(w ^ 1L));
                return new Mem(Base:Rn,Mode:mode,Sign:sign,Index:Rm,Shift:typ,Count:count);
            else if (aop == arg_mem_R_pm_imm12_offset) 
                // Treat [<Rn>,#+/-<imm12>] like [<Rn>{,#+/-<imm12>}]{!}
                // by forcing P=1, W=0 (index=false, wback=false).
                return decodeArg(arg_mem_R_pm_imm12_W, x & ~(1L << (int)(21L)) | 1L << (int)(24L));
            else if (aop == arg_mem_R_pm_imm12_postindex) 
                // Treat [<Rn>],#+/-<imm12> like [<Rn>{,#+/-<imm12>}]{!}
                // by forcing P=0, W=0 (postindex=true).
                return decodeArg(arg_mem_R_pm_imm12_W, x & ~(1L << (int)(24L) | 1L << (int)(21L)));
            else if (aop == arg_mem_R_pm_imm12_W) 
                Rn = Reg((x >> (int)(16L)) & (1L << (int)(4L) - 1L));
                u = (x >> (int)(23L)) & 1L;
                w = (x >> (int)(21L)) & 1L;
                p = (x >> (int)(24L)) & 1L;
                if (p == 0L && w == 1L)
                {
                    return null;
                }

                sign = int8(+1L);
                if (u == 0L)
                {
                    sign = -1L;
                }

                imm = int16(x & (1L << (int)(12L) - 1L));
                mode = AddrMode(uint8(p << (int)(1L)) | uint8(w ^ 1L));
                return new Mem(Base:Rn,Mode:mode,Offset:int16(sign)*imm);
            else if (aop == arg_mem_R_pm_imm8_postindex) 
                // Treat [<Rn>],#+/-<imm8> like [<Rn>{,#+/-<imm8>}]{!}
                // by forcing P=0, W=0 (postindex=true).
                return decodeArg(arg_mem_R_pm_imm8_W, x & ~(1L << (int)(24L) | 1L << (int)(21L)));
            else if (aop == arg_mem_R_pm_imm8_W) 
                Rn = Reg((x >> (int)(16L)) & (1L << (int)(4L) - 1L));
                u = (x >> (int)(23L)) & 1L;
                w = (x >> (int)(21L)) & 1L;
                p = (x >> (int)(24L)) & 1L;
                if (p == 0L && w == 1L)
                {
                    return null;
                }

                sign = int8(+1L);
                if (u == 0L)
                {
                    sign = -1L;
                }

                imm = int16((x >> (int)(8L)) & (1L << (int)(4L) - 1L) << (int)(4L) | x & (1L << (int)(4L) - 1L));
                mode = AddrMode(uint8(p << (int)(1L)) | uint8(w ^ 1L));
                return new Mem(Base:Rn,Mode:mode,Offset:int16(sign)*imm);
            else if (aop == arg_mem_R_pm_imm8at0_offset) 
                Rn = Reg((x >> (int)(16L)) & (1L << (int)(4L) - 1L));
                u = (x >> (int)(23L)) & 1L;
                sign = int8(+1L);
                if (u == 0L)
                {
                    sign = -1L;
                }

                imm = int16(x & (1L << (int)(8L) - 1L)) << (int)(2L);
                return new Mem(Base:Rn,Mode:AddrOffset,Offset:int16(sign)*imm);
            else if (aop == arg_option) 
                return Imm(x & (1L << (int)(4L) - 1L));
            else if (aop == arg_registers) 
                return RegList(x & (1L << (int)(16L) - 1L));
            else if (aop == arg_registers2) 
                x &= 1L << (int)(16L) - 1L;
                long n = 0L;
                for (long i = 0L; i < 16L; i++)
                {
                    if (x >> (int)(uint(i)) & 1L != 0L)
                    {
                        n++;
                    }

                }

                if (n < 2L)
                {
                    return null;
                }

                return RegList(x);
            else if (aop == arg_registers1) 
                var Rt = (x >> (int)(12L)) & (1L << (int)(4L) - 1L);
                return RegList(1L << (int)(Rt));
            else if (aop == arg_satimm4) 
                return Imm((x >> (int)(16L)) & (1L << (int)(4L) - 1L));
            else if (aop == arg_satimm5) 
                return Imm((x >> (int)(16L)) & (1L << (int)(5L) - 1L));
            else if (aop == arg_satimm4m1) 
                return Imm((x >> (int)(16L)) & (1L << (int)(4L) - 1L) + 1L);
            else if (aop == arg_satimm5m1) 
                return Imm((x >> (int)(16L)) & (1L << (int)(5L) - 1L) + 1L);
            else if (aop == arg_widthm1) 
                return Imm((x >> (int)(16L)) & (1L << (int)(5L) - 1L) + 1L);
            else 
                return null;
            
        }

        // decodeShift decodes the shift-by-immediate encoded in x.
        private static (Shift, byte) decodeShift(uint x)
        {
            Shift _p0 = default;
            byte _p0 = default;

            var count = (x >> (int)(7L)) & (1L << (int)(5L) - 1L);
            var typ = Shift((x >> (int)(5L)) & (1L << (int)(2L) - 1L));

            if (typ == ShiftRight || typ == ShiftRightSigned) 
                if (count == 0L)
                {
                    count = 32L;
                }

            else if (typ == RotateRight) 
                if (count == 0L)
                {
                    typ = RotateRightExt;
                    count = 1L;
                }

                        return (typ, uint8(count));

        }
    }
}}}}}}}
