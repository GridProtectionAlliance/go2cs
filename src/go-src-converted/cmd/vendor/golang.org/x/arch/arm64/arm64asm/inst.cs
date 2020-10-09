// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package arm64asm -- go2cs converted at 2020 October 09 05:54:46 UTC
// import "cmd/vendor/golang.org/x/arch/arm64/arm64asm" ==> using arm64asm = go.cmd.vendor.golang.org.x.arch.arm64.arm64asm_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\arch\arm64\arm64asm\inst.go
using fmt = go.fmt_package;
using strings = go.strings_package;
using static go.builtin;

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
        // An Op is an ARM64 opcode.
        public partial struct Op // : ushort
        {
        }

        // NOTE: The actual Op values are defined in tables.go.
        // They are chosen to simplify instruction decoding and
        // are not a dense packing from 0 to N, although the
        // density is high, probably at least 90%.

        public static @string String(this Op op)
        {
            if (op >= Op(len(opstr)) || opstr[op] == "")
            {
                return fmt.Sprintf("Op(%d)", int(op));
            }

            return opstr[op];

        }

        // An Inst is a single instruction.
        public partial struct Inst
        {
            public Op Op; // Opcode mnemonic
            public uint Enc; // Raw encoding bits.
            public Args Args; // Instruction arguments, in ARM manual order.
        }

        public static @string String(this Inst i)
        {
            slice<@string> args = default;
            foreach (var (_, arg) in i.Args)
            {
                if (arg == null)
                {
                    break;
                }

                args = append(args, arg.String());

            }
            return i.Op.String() + " " + strings.Join(args, ", ");

        }

        // An Args holds the instruction arguments.
        // If an instruction has fewer than 5 arguments,
        // the final elements in the array are nil.
        public partial struct Args // : array<Arg>
        {
        }

        // An Arg is a single instruction argument, one of these types:
        // Reg, RegSP, ImmShift, RegExtshiftAmount, PCRel, MemImmediate,
        // MemExtend, Imm, Imm64, Imm_hint, Imm_clrex, Imm_dcps, Cond,
        // Imm_c, Imm_option, Imm_prfop, Pstatefield, Systemreg, Imm_fp
        // RegisterWithArrangement, RegisterWithArrangementAndIndex.
        public partial interface Arg
        {
            @string isArg();
            @string String();
        }

        // A Reg is a single register.
        // The zero value denotes W0, not the absence of a register.
        public partial struct Reg // : ushort
        {
        }

        public static readonly Reg W0 = (Reg)iota;
        public static readonly var W1 = 0;
        public static readonly var W2 = 1;
        public static readonly var W3 = 2;
        public static readonly var W4 = 3;
        public static readonly var W5 = 4;
        public static readonly var W6 = 5;
        public static readonly var W7 = 6;
        public static readonly var W8 = 7;
        public static readonly var W9 = 8;
        public static readonly var W10 = 9;
        public static readonly var W11 = 10;
        public static readonly var W12 = 11;
        public static readonly var W13 = 12;
        public static readonly var W14 = 13;
        public static readonly var W15 = 14;
        public static readonly var W16 = 15;
        public static readonly var W17 = 16;
        public static readonly var W18 = 17;
        public static readonly var W19 = 18;
        public static readonly var W20 = 19;
        public static readonly var W21 = 20;
        public static readonly var W22 = 21;
        public static readonly var W23 = 22;
        public static readonly var W24 = 23;
        public static readonly var W25 = 24;
        public static readonly var W26 = 25;
        public static readonly var W27 = 26;
        public static readonly var W28 = 27;
        public static readonly var W29 = 28;
        public static readonly var W30 = 29;
        public static readonly var WZR = 30;

        public static readonly var X0 = 31;
        public static readonly var X1 = 32;
        public static readonly var X2 = 33;
        public static readonly var X3 = 34;
        public static readonly var X4 = 35;
        public static readonly var X5 = 36;
        public static readonly var X6 = 37;
        public static readonly var X7 = 38;
        public static readonly var X8 = 39;
        public static readonly var X9 = 40;
        public static readonly var X10 = 41;
        public static readonly var X11 = 42;
        public static readonly var X12 = 43;
        public static readonly var X13 = 44;
        public static readonly var X14 = 45;
        public static readonly var X15 = 46;
        public static readonly var X16 = 47;
        public static readonly var X17 = 48;
        public static readonly var X18 = 49;
        public static readonly var X19 = 50;
        public static readonly var X20 = 51;
        public static readonly var X21 = 52;
        public static readonly var X22 = 53;
        public static readonly var X23 = 54;
        public static readonly var X24 = 55;
        public static readonly var X25 = 56;
        public static readonly var X26 = 57;
        public static readonly var X27 = 58;
        public static readonly var X28 = 59;
        public static readonly var X29 = 60;
        public static readonly var X30 = 61;
        public static readonly var XZR = 62;

        public static readonly var B0 = 63;
        public static readonly var B1 = 64;
        public static readonly var B2 = 65;
        public static readonly var B3 = 66;
        public static readonly var B4 = 67;
        public static readonly var B5 = 68;
        public static readonly var B6 = 69;
        public static readonly var B7 = 70;
        public static readonly var B8 = 71;
        public static readonly var B9 = 72;
        public static readonly var B10 = 73;
        public static readonly var B11 = 74;
        public static readonly var B12 = 75;
        public static readonly var B13 = 76;
        public static readonly var B14 = 77;
        public static readonly var B15 = 78;
        public static readonly var B16 = 79;
        public static readonly var B17 = 80;
        public static readonly var B18 = 81;
        public static readonly var B19 = 82;
        public static readonly var B20 = 83;
        public static readonly var B21 = 84;
        public static readonly var B22 = 85;
        public static readonly var B23 = 86;
        public static readonly var B24 = 87;
        public static readonly var B25 = 88;
        public static readonly var B26 = 89;
        public static readonly var B27 = 90;
        public static readonly var B28 = 91;
        public static readonly var B29 = 92;
        public static readonly var B30 = 93;
        public static readonly var B31 = 94;

        public static readonly var H0 = 95;
        public static readonly var H1 = 96;
        public static readonly var H2 = 97;
        public static readonly var H3 = 98;
        public static readonly var H4 = 99;
        public static readonly var H5 = 100;
        public static readonly var H6 = 101;
        public static readonly var H7 = 102;
        public static readonly var H8 = 103;
        public static readonly var H9 = 104;
        public static readonly var H10 = 105;
        public static readonly var H11 = 106;
        public static readonly var H12 = 107;
        public static readonly var H13 = 108;
        public static readonly var H14 = 109;
        public static readonly var H15 = 110;
        public static readonly var H16 = 111;
        public static readonly var H17 = 112;
        public static readonly var H18 = 113;
        public static readonly var H19 = 114;
        public static readonly var H20 = 115;
        public static readonly var H21 = 116;
        public static readonly var H22 = 117;
        public static readonly var H23 = 118;
        public static readonly var H24 = 119;
        public static readonly var H25 = 120;
        public static readonly var H26 = 121;
        public static readonly var H27 = 122;
        public static readonly var H28 = 123;
        public static readonly var H29 = 124;
        public static readonly var H30 = 125;
        public static readonly var H31 = 126;

        public static readonly var S0 = 127;
        public static readonly var S1 = 128;
        public static readonly var S2 = 129;
        public static readonly var S3 = 130;
        public static readonly var S4 = 131;
        public static readonly var S5 = 132;
        public static readonly var S6 = 133;
        public static readonly var S7 = 134;
        public static readonly var S8 = 135;
        public static readonly var S9 = 136;
        public static readonly var S10 = 137;
        public static readonly var S11 = 138;
        public static readonly var S12 = 139;
        public static readonly var S13 = 140;
        public static readonly var S14 = 141;
        public static readonly var S15 = 142;
        public static readonly var S16 = 143;
        public static readonly var S17 = 144;
        public static readonly var S18 = 145;
        public static readonly var S19 = 146;
        public static readonly var S20 = 147;
        public static readonly var S21 = 148;
        public static readonly var S22 = 149;
        public static readonly var S23 = 150;
        public static readonly var S24 = 151;
        public static readonly var S25 = 152;
        public static readonly var S26 = 153;
        public static readonly var S27 = 154;
        public static readonly var S28 = 155;
        public static readonly var S29 = 156;
        public static readonly var S30 = 157;
        public static readonly var S31 = 158;

        public static readonly var D0 = 159;
        public static readonly var D1 = 160;
        public static readonly var D2 = 161;
        public static readonly var D3 = 162;
        public static readonly var D4 = 163;
        public static readonly var D5 = 164;
        public static readonly var D6 = 165;
        public static readonly var D7 = 166;
        public static readonly var D8 = 167;
        public static readonly var D9 = 168;
        public static readonly var D10 = 169;
        public static readonly var D11 = 170;
        public static readonly var D12 = 171;
        public static readonly var D13 = 172;
        public static readonly var D14 = 173;
        public static readonly var D15 = 174;
        public static readonly var D16 = 175;
        public static readonly var D17 = 176;
        public static readonly var D18 = 177;
        public static readonly var D19 = 178;
        public static readonly var D20 = 179;
        public static readonly var D21 = 180;
        public static readonly var D22 = 181;
        public static readonly var D23 = 182;
        public static readonly var D24 = 183;
        public static readonly var D25 = 184;
        public static readonly var D26 = 185;
        public static readonly var D27 = 186;
        public static readonly var D28 = 187;
        public static readonly var D29 = 188;
        public static readonly var D30 = 189;
        public static readonly var D31 = 190;

        public static readonly var Q0 = 191;
        public static readonly var Q1 = 192;
        public static readonly var Q2 = 193;
        public static readonly var Q3 = 194;
        public static readonly var Q4 = 195;
        public static readonly var Q5 = 196;
        public static readonly var Q6 = 197;
        public static readonly var Q7 = 198;
        public static readonly var Q8 = 199;
        public static readonly var Q9 = 200;
        public static readonly var Q10 = 201;
        public static readonly var Q11 = 202;
        public static readonly var Q12 = 203;
        public static readonly var Q13 = 204;
        public static readonly var Q14 = 205;
        public static readonly var Q15 = 206;
        public static readonly var Q16 = 207;
        public static readonly var Q17 = 208;
        public static readonly var Q18 = 209;
        public static readonly var Q19 = 210;
        public static readonly var Q20 = 211;
        public static readonly var Q21 = 212;
        public static readonly var Q22 = 213;
        public static readonly var Q23 = 214;
        public static readonly var Q24 = 215;
        public static readonly var Q25 = 216;
        public static readonly var Q26 = 217;
        public static readonly var Q27 = 218;
        public static readonly var Q28 = 219;
        public static readonly var Q29 = 220;
        public static readonly var Q30 = 221;
        public static readonly var Q31 = 222;

        public static readonly var V0 = 223;
        public static readonly var V1 = 224;
        public static readonly var V2 = 225;
        public static readonly var V3 = 226;
        public static readonly var V4 = 227;
        public static readonly var V5 = 228;
        public static readonly var V6 = 229;
        public static readonly var V7 = 230;
        public static readonly var V8 = 231;
        public static readonly var V9 = 232;
        public static readonly var V10 = 233;
        public static readonly var V11 = 234;
        public static readonly var V12 = 235;
        public static readonly var V13 = 236;
        public static readonly var V14 = 237;
        public static readonly var V15 = 238;
        public static readonly var V16 = 239;
        public static readonly var V17 = 240;
        public static readonly var V18 = 241;
        public static readonly var V19 = 242;
        public static readonly var V20 = 243;
        public static readonly var V21 = 244;
        public static readonly var V22 = 245;
        public static readonly var V23 = 246;
        public static readonly var V24 = 247;
        public static readonly var V25 = 248;
        public static readonly var V26 = 249;
        public static readonly var V27 = 250;
        public static readonly var V28 = 251;
        public static readonly var V29 = 252;
        public static readonly var V30 = 253;
        public static readonly WSP V31 = (WSP)WZR; // These are different registers with the same encoding.
        public static readonly var SP = XZR; // These are different registers with the same encoding.

        public static void isArg(this Reg _p0)
        {
        }

        public static @string String(this Reg r)
        {

            if (r == WZR) 
                return "WZR";
            else if (r == XZR) 
                return "XZR";
            else if (W0 <= r && r <= W30) 
                return fmt.Sprintf("W%d", int(r - W0));
            else if (X0 <= r && r <= X30) 
                return fmt.Sprintf("X%d", int(r - X0));
            else if (B0 <= r && r <= B31) 
                return fmt.Sprintf("B%d", int(r - B0));
            else if (H0 <= r && r <= H31) 
                return fmt.Sprintf("H%d", int(r - H0));
            else if (S0 <= r && r <= S31) 
                return fmt.Sprintf("S%d", int(r - S0));
            else if (D0 <= r && r <= D31) 
                return fmt.Sprintf("D%d", int(r - D0));
            else if (Q0 <= r && r <= Q31) 
                return fmt.Sprintf("Q%d", int(r - Q0));
            else if (V0 <= r && r <= V31) 
                return fmt.Sprintf("V%d", int(r - V0));
            else 
                return fmt.Sprintf("Reg(%d)", int(r));
            
        }

        // A RegSP represent a register and X31/W31 is regarded as SP/WSP.
        public partial struct RegSP // : Reg
        {
        }

        public static void isArg(this RegSP _p0)
        {
        }

        public static @string String(this RegSP r)
        {

            if (Reg(r) == WSP) 
                return "WSP";
            else if (Reg(r) == SP) 
                return "SP";
            else 
                return Reg(r).String();
            
        }

        public partial struct ImmShift
        {
            public ushort imm;
            public byte shift;
        }

        public static void isArg(this ImmShift _p0)
        {
        }

        public static @string String(this ImmShift @is)
        {
            if (@is.shift == 0L)
            {
                return fmt.Sprintf("#%#x", @is.imm);
            }

            if (@is.shift < 128L)
            {
                return fmt.Sprintf("#%#x, LSL #%d", @is.imm, @is.shift);
            }

            return fmt.Sprintf("#%#x, MSL #%d", @is.imm, @is.shift - 128L);

        }

        public partial struct ExtShift // : byte
        {
        }

        private static readonly ExtShift _ = (ExtShift)iota;
        private static readonly var uxtb = 0;
        private static readonly var uxth = 1;
        private static readonly var uxtw = 2;
        private static readonly var uxtx = 3;
        private static readonly var sxtb = 4;
        private static readonly var sxth = 5;
        private static readonly var sxtw = 6;
        private static readonly var sxtx = 7;
        private static readonly var lsl = 8;
        private static readonly var lsr = 9;
        private static readonly var asr = 10;
        private static readonly var ror = 11;


        public static @string String(this ExtShift extShift)
        {

            if (extShift == uxtb) 
                return "UXTB";
            else if (extShift == uxth) 
                return "UXTH";
            else if (extShift == uxtw) 
                return "UXTW";
            else if (extShift == uxtx) 
                return "UXTX";
            else if (extShift == sxtb) 
                return "SXTB";
            else if (extShift == sxth) 
                return "SXTH";
            else if (extShift == sxtw) 
                return "SXTW";
            else if (extShift == sxtx) 
                return "SXTX";
            else if (extShift == lsl) 
                return "LSL";
            else if (extShift == lsr) 
                return "LSR";
            else if (extShift == asr) 
                return "ASR";
            else if (extShift == ror) 
                return "ROR";
                        return "";

        }

        public partial struct RegExtshiftAmount
        {
            public Reg reg;
            public ExtShift extShift;
            public byte amount;
            public bool show_zero;
        }

        public static void isArg(this RegExtshiftAmount _p0)
        {
        }

        public static @string String(this RegExtshiftAmount rea)
        {
            var buf = rea.reg.String();
            if (rea.extShift != ExtShift(0L))
            {
                buf += ", " + rea.extShift.String();
                if (rea.amount != 0L)
                {
                    buf += fmt.Sprintf(" #%d", rea.amount);
                }
                else
                {
                    if (rea.show_zero == true)
                    {
                        buf += fmt.Sprintf(" #%d", rea.amount);
                    }

                }

            }

            return buf;

        }

        // A PCRel describes a memory address (usually a code label)
        // as a distance relative to the program counter.
        public partial struct PCRel // : long
        {
        }

        public static void isArg(this PCRel _p0)
        {
        }

        public static @string String(this PCRel r)
        {
            return fmt.Sprintf(".%+#x", uint64(r));
        }

        // An AddrMode is an ARM addressing mode.
        public partial struct AddrMode // : byte
        {
        }

        private static readonly AddrMode _ = (AddrMode)iota;
        public static readonly var AddrPostIndex = 0; // [R], X - use address R, set R = R + X
        public static readonly var AddrPreIndex = 1; // [R, X]! - use address R + X, set R = R + X
        public static readonly var AddrOffset = 2; // [R, X] - use address R + X
        public static readonly var AddrPostReg = 3; // [Rn], Rm - - use address Rn, set Rn = Rn + Rm

        // A MemImmediate is a memory reference made up of a base R and immediate X.
        // The effective memory address is R or R+X depending on AddrMode.
        public partial struct MemImmediate
        {
            public RegSP Base;
            public AddrMode Mode;
            public int imm;
        }

        public static void isArg(this MemImmediate _p0)
        {
        }

        public static @string String(this MemImmediate m)
        {
            var R = m.Base.String();
            var X = fmt.Sprintf("#%d", m.imm);


            if (m.Mode == AddrOffset) 
                if (X == "#0")
                {
                    return fmt.Sprintf("[%s]", R);
                }

                return fmt.Sprintf("[%s,%s]", R, X);
            else if (m.Mode == AddrPreIndex) 
                return fmt.Sprintf("[%s,%s]!", R, X);
            else if (m.Mode == AddrPostIndex) 
                return fmt.Sprintf("[%s],%s", R, X);
            else if (m.Mode == AddrPostReg) 
                var post = Reg(X0) + Reg(m.imm);
                var postR = post.String();
                return fmt.Sprintf("[%s], %s", R, postR);
                        return fmt.Sprintf("unimplemented!");

        }

        // A MemExtend is a memory reference made up of a base R and index expression X.
        // The effective memory address is R or R+X depending on Index, Extend and Amount.
        public partial struct MemExtend
        {
            public RegSP Base;
            public Reg Index;
            public ExtShift Extend; // Amount indicates the index shift amount (but also see ShiftMustBeZero field below).
            public byte Amount; // ShiftMustBeZero is set to true when the shift amount must be 0, even if the
// Amount field is not 0. In GNU syntax, a #0 shift amount is printed if Amount
// is not 0 but ShiftMustBeZero is true; #0 is not printed if Amount is 0 and
// ShiftMustBeZero is true. Both cases represent shift by 0 bit.
            public bool ShiftMustBeZero;
        }

        public static void isArg(this MemExtend _p0)
        {
        }

        public static @string String(this MemExtend m)
        {
            var Rbase = m.Base.String();
            var RIndex = m.Index.String();
            if (m.ShiftMustBeZero)
            {
                if (m.Amount != 0L)
                {
                    return fmt.Sprintf("[%s,%s,%s #0]", Rbase, RIndex, m.Extend.String());
                }
                else
                {
                    if (m.Extend != lsl)
                    {
                        return fmt.Sprintf("[%s,%s,%s]", Rbase, RIndex, m.Extend.String());
                    }
                    else
                    {
                        return fmt.Sprintf("[%s,%s]", Rbase, RIndex);
                    }

                }

            }
            else
            {
                if (m.Amount != 0L)
                {
                    return fmt.Sprintf("[%s,%s,%s #%d]", Rbase, RIndex, m.Extend.String(), m.Amount);
                }
                else
                {
                    if (m.Extend != lsl)
                    {
                        return fmt.Sprintf("[%s,%s,%s]", Rbase, RIndex, m.Extend.String());
                    }
                    else
                    {
                        return fmt.Sprintf("[%s,%s]", Rbase, RIndex);
                    }

                }

            }

        }

        // An Imm is an integer constant.
        public partial struct Imm
        {
            public uint Imm;
            public bool Decimal;
        }

        public static void isArg(this Imm _p0)
        {
        }

        public static @string String(this Imm i)
        {
            if (!i.Decimal)
            {
                return fmt.Sprintf("#%#x", i.Imm);
            }
            else
            {
                return fmt.Sprintf("#%d", i.Imm);
            }

        }

        public partial struct Imm64
        {
            public ulong Imm;
            public bool Decimal;
        }

        public static void isArg(this Imm64 _p0)
        {
        }

        public static @string String(this Imm64 i)
        {
            if (!i.Decimal)
            {
                return fmt.Sprintf("#%#x", i.Imm);
            }
            else
            {
                return fmt.Sprintf("#%d", i.Imm);
            }

        }

        // An Imm_hint is an integer constant for HINT instruction.
        public partial struct Imm_hint // : byte
        {
        }

        public static void isArg(this Imm_hint _p0)
        {
        }

        public static @string String(this Imm_hint i)
        {
            return fmt.Sprintf("#%#x", uint32(i));
        }

        // An Imm_clrex is an integer constant for CLREX instruction.
        public partial struct Imm_clrex // : byte
        {
        }

        public static void isArg(this Imm_clrex _p0)
        {
        }

        public static @string String(this Imm_clrex i)
        {
            if (i == 15L)
            {
                return "";
            }

            return fmt.Sprintf("#%#x", uint32(i));

        }

        // An Imm_dcps is an integer constant for DCPS[123] instruction.
        public partial struct Imm_dcps // : ushort
        {
        }

        public static void isArg(this Imm_dcps _p0)
        {
        }

        public static @string String(this Imm_dcps i)
        {
            if (i == 0L)
            {
                return "";
            }

            return fmt.Sprintf("#%#x", uint32(i));

        }

        // Standard conditions.
        public partial struct Cond
        {
            public byte Value;
            public bool Invert;
        }

        public static void isArg(this Cond _p0)
        {
        }

        public static @string String(this Cond c)
        {
            var cond31 = c.Value >> (int)(1L);
            var invert = bool((c.Value & 1L) == 1L);
            invert = (invert != c.Invert);
            switch (cond31)
            {
                case 0L: 
                    if (invert)
                    {
                        return "NE";
                    }
                    else
                    {
                        return "EQ";
                    }

                    break;
                case 1L: 
                    if (invert)
                    {
                        return "CC";
                    }
                    else
                    {
                        return "CS";
                    }

                    break;
                case 2L: 
                    if (invert)
                    {
                        return "PL";
                    }
                    else
                    {
                        return "MI";
                    }

                    break;
                case 3L: 
                    if (invert)
                    {
                        return "VC";
                    }
                    else
                    {
                        return "VS";
                    }

                    break;
                case 4L: 
                    if (invert)
                    {
                        return "LS";
                    }
                    else
                    {
                        return "HI";
                    }

                    break;
                case 5L: 
                    if (invert)
                    {
                        return "LT";
                    }
                    else
                    {
                        return "GE";
                    }

                    break;
                case 6L: 
                    if (invert)
                    {
                        return "LE";
                    }
                    else
                    {
                        return "GT";
                    }

                    break;
                case 7L: 
                    return "AL";
                    break;
            }
            return "";

        }

        // An Imm_c is an integer constant for SYS/SYSL/TLBI instruction.
        public partial struct Imm_c // : byte
        {
        }

        public static void isArg(this Imm_c _p0)
        {
        }

        public static @string String(this Imm_c i)
        {
            return fmt.Sprintf("C%d", uint8(i));
        }

        // An Imm_option is an integer constant for DMB/DSB/ISB instruction.
        public partial struct Imm_option // : byte
        {
        }

        public static void isArg(this Imm_option _p0)
        {
        }

        public static @string String(this Imm_option i)
        {
            switch (uint8(i))
            {
                case 15L: 
                    return "SY";
                    break;
                case 14L: 
                    return "ST";
                    break;
                case 13L: 
                    return "LD";
                    break;
                case 11L: 
                    return "ISH";
                    break;
                case 10L: 
                    return "ISHST";
                    break;
                case 9L: 
                    return "ISHLD";
                    break;
                case 7L: 
                    return "NSH";
                    break;
                case 6L: 
                    return "NSHST";
                    break;
                case 5L: 
                    return "NSHLD";
                    break;
                case 3L: 
                    return "OSH";
                    break;
                case 2L: 
                    return "OSHST";
                    break;
                case 1L: 
                    return "OSHLD";
                    break;
            }
            return fmt.Sprintf("#%#02x", uint8(i));

        }

        // An Imm_prfop is an integer constant for PRFM instruction.
        public partial struct Imm_prfop // : byte
        {
        }

        public static void isArg(this Imm_prfop _p0)
        {
        }

        public static @string String(this Imm_prfop i)
        {
            var prf_type = (i >> (int)(3L)) & (1L << (int)(2L) - 1L);
            var prf_target = (i >> (int)(1L)) & (1L << (int)(2L) - 1L);
            var prf_policy = i & 1L;
            @string result = default;

            switch (prf_type)
            {
                case 0L: 
                    result = "PLD";
                    break;
                case 1L: 
                    result = "PLI";
                    break;
                case 2L: 
                    result = "PST";
                    break;
                case 3L: 
                    return fmt.Sprintf("#%#02x", uint8(i));
                    break;
            }
            switch (prf_target)
            {
                case 0L: 
                    result += "L1";
                    break;
                case 1L: 
                    result += "L2";
                    break;
                case 2L: 
                    result += "L3";
                    break;
                case 3L: 
                    return fmt.Sprintf("#%#02x", uint8(i));
                    break;
            }
            if (prf_policy == 0L)
            {
                result += "KEEP";
            }
            else
            {
                result += "STRM";
            }

            return result;

        }

        public partial struct Pstatefield // : byte
        {
        }

        public static readonly Pstatefield SPSel = (Pstatefield)iota;
        public static readonly var DAIFSet = 0;
        public static readonly var DAIFClr = 1;


        public static void isArg(this Pstatefield _p0)
        {
        }

        public static @string String(this Pstatefield p)
        {

            if (p == SPSel) 
                return "SPSel";
            else if (p == DAIFSet) 
                return "DAIFSet";
            else if (p == DAIFClr) 
                return "DAIFClr";
            else 
                return "unimplemented";
            
        }

        public partial struct Systemreg
        {
            public byte op0;
            public byte op1;
            public byte cn;
            public byte cm;
            public byte op2;
        }

        public static void isArg(this Systemreg _p0)
        {
        }

        public static @string String(this Systemreg s)
        {
            return fmt.Sprintf("S%d_%d_C%d_C%d_%d", s.op0, s.op1, s.cn, s.cm, s.op2);
        }

        // An Imm_fp is a signed floating-point constant.
        public partial struct Imm_fp
        {
            public byte s;
            public sbyte exp;
            public byte pre;
        }

        public static void isArg(this Imm_fp _p0)
        {
        }

        public static @string String(this Imm_fp i)
        {
            short s = default;            short pre = default;            short numerator = default;            short denominator = default;

            double result = default;
            if (i.s == 0L)
            {
                s = 1L;
            }
            else
            {
                s = -1L;
            }

            pre = s * int16(16L + i.pre);
            if (i.exp > 0L)
            {
                numerator = (pre << (int)(uint8(i.exp)));
                denominator = 16L;
            }
            else
            {
                numerator = pre;
                denominator = (16L << (int)(uint8(-1L * i.exp)));
            }

            result = float64(numerator) / float64(denominator);
            return fmt.Sprintf("#%.18e", result);

        }

        public partial struct Arrangement // : byte
        {
        }

        private static readonly Arrangement _ = (Arrangement)iota;
        public static readonly var ArrangementB = 0;
        public static readonly var Arrangement8B = 1;
        public static readonly var Arrangement16B = 2;
        public static readonly var ArrangementH = 3;
        public static readonly var Arrangement4H = 4;
        public static readonly var Arrangement8H = 5;
        public static readonly var ArrangementS = 6;
        public static readonly var Arrangement2S = 7;
        public static readonly var Arrangement4S = 8;
        public static readonly var ArrangementD = 9;
        public static readonly var Arrangement1D = 10;
        public static readonly var Arrangement2D = 11;
        public static readonly var Arrangement1Q = 12;


        public static @string String(this Arrangement a)
        {
            @string result = default;


            if (a == ArrangementB) 
                result = ".B";
            else if (a == Arrangement8B) 
                result = ".8B";
            else if (a == Arrangement16B) 
                result = ".16B";
            else if (a == ArrangementH) 
                result = ".H";
            else if (a == Arrangement4H) 
                result = ".4H";
            else if (a == Arrangement8H) 
                result = ".8H";
            else if (a == ArrangementS) 
                result = ".S";
            else if (a == Arrangement2S) 
                result = ".2S";
            else if (a == Arrangement4S) 
                result = ".4S";
            else if (a == ArrangementD) 
                result = ".D";
            else if (a == Arrangement1D) 
                result = ".1D";
            else if (a == Arrangement2D) 
                result = ".2D";
            else if (a == Arrangement1Q) 
                result = ".1Q";
                        return ;

        }

        // Register with arrangement: <Vd>.<T>, { <Vt>.8B, <Vt2>.8B},
        public partial struct RegisterWithArrangement
        {
            public Reg r;
            public Arrangement a;
            public byte cnt;
        }

        public static void isArg(this RegisterWithArrangement _p0)
        {
        }

        public static @string String(this RegisterWithArrangement r)
        {
            var result = r.r.String();
            result += r.a.String();
            if (r.cnt > 0L)
            {
                result = "{" + result;
                if (r.cnt == 2L)
                {
                    var r1 = V0 + Reg((uint16(r.r) - uint16(V0) + 1L) & 31L);
                    result += ", " + r1.String() + r.a.String();
                }
                else if (r.cnt > 2L)
                {
                    if ((uint16(r.cnt) + ((uint16(r.r) - uint16(V0)) & 31L)) > 32L)
                    {
                        for (long i = 1L; i < int(r.cnt); i++)
                        {
                            var cur = V0 + Reg((uint16(r.r) - uint16(V0) + uint16(i)) & 31L);
                            result += ", " + cur.String() + r.a.String();
                        }
                    else


                    }                    {
                        r1 = V0 + Reg((uint16(r.r) - uint16(V0) + uint16(r.cnt) - 1L) & 31L);
                        result += "-" + r1.String() + r.a.String();
                    }

                }

                result += "}";

            }

            return result;

        }

        // Register with arrangement and index: <Vm>.<Ts>[<index>],
        //   { <Vt>.B, <Vt2>.B }[<index>].
        public partial struct RegisterWithArrangementAndIndex
        {
            public Reg r;
            public Arrangement a;
            public byte index;
            public byte cnt;
        }

        public static void isArg(this RegisterWithArrangementAndIndex _p0)
        {
        }

        public static @string String(this RegisterWithArrangementAndIndex r)
        {
            var result = r.r.String();
            result += r.a.String();
            if (r.cnt > 0L)
            {
                result = "{" + result;
                if (r.cnt == 2L)
                {
                    var r1 = V0 + Reg((uint16(r.r) - uint16(V0) + 1L) & 31L);
                    result += ", " + r1.String() + r.a.String();
                }
                else if (r.cnt > 2L)
                {
                    if ((uint16(r.cnt) + ((uint16(r.r) - uint16(V0)) & 31L)) > 32L)
                    {
                        for (long i = 1L; i < int(r.cnt); i++)
                        {
                            var cur = V0 + Reg((uint16(r.r) - uint16(V0) + uint16(i)) & 31L);
                            result += ", " + cur.String() + r.a.String();
                        }
                    else


                    }                    {
                        r1 = V0 + Reg((uint16(r.r) - uint16(V0) + uint16(r.cnt) - 1L) & 31L);
                        result += "-" + r1.String() + r.a.String();
                    }

                }

                result += "}";

            }

            return fmt.Sprintf("%s[%d]", result, r.index);

        }
    }
}}}}}}}
