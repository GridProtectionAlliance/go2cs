// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package arm64asm -- go2cs converted at 2020 October 08 04:44:33 UTC
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
        public static readonly var W1 = (var)0;
        public static readonly var W2 = (var)1;
        public static readonly var W3 = (var)2;
        public static readonly var W4 = (var)3;
        public static readonly var W5 = (var)4;
        public static readonly var W6 = (var)5;
        public static readonly var W7 = (var)6;
        public static readonly var W8 = (var)7;
        public static readonly var W9 = (var)8;
        public static readonly var W10 = (var)9;
        public static readonly var W11 = (var)10;
        public static readonly var W12 = (var)11;
        public static readonly var W13 = (var)12;
        public static readonly var W14 = (var)13;
        public static readonly var W15 = (var)14;
        public static readonly var W16 = (var)15;
        public static readonly var W17 = (var)16;
        public static readonly var W18 = (var)17;
        public static readonly var W19 = (var)18;
        public static readonly var W20 = (var)19;
        public static readonly var W21 = (var)20;
        public static readonly var W22 = (var)21;
        public static readonly var W23 = (var)22;
        public static readonly var W24 = (var)23;
        public static readonly var W25 = (var)24;
        public static readonly var W26 = (var)25;
        public static readonly var W27 = (var)26;
        public static readonly var W28 = (var)27;
        public static readonly var W29 = (var)28;
        public static readonly var W30 = (var)29;
        public static readonly var WZR = (var)30;

        public static readonly var X0 = (var)31;
        public static readonly var X1 = (var)32;
        public static readonly var X2 = (var)33;
        public static readonly var X3 = (var)34;
        public static readonly var X4 = (var)35;
        public static readonly var X5 = (var)36;
        public static readonly var X6 = (var)37;
        public static readonly var X7 = (var)38;
        public static readonly var X8 = (var)39;
        public static readonly var X9 = (var)40;
        public static readonly var X10 = (var)41;
        public static readonly var X11 = (var)42;
        public static readonly var X12 = (var)43;
        public static readonly var X13 = (var)44;
        public static readonly var X14 = (var)45;
        public static readonly var X15 = (var)46;
        public static readonly var X16 = (var)47;
        public static readonly var X17 = (var)48;
        public static readonly var X18 = (var)49;
        public static readonly var X19 = (var)50;
        public static readonly var X20 = (var)51;
        public static readonly var X21 = (var)52;
        public static readonly var X22 = (var)53;
        public static readonly var X23 = (var)54;
        public static readonly var X24 = (var)55;
        public static readonly var X25 = (var)56;
        public static readonly var X26 = (var)57;
        public static readonly var X27 = (var)58;
        public static readonly var X28 = (var)59;
        public static readonly var X29 = (var)60;
        public static readonly var X30 = (var)61;
        public static readonly var XZR = (var)62;

        public static readonly var B0 = (var)63;
        public static readonly var B1 = (var)64;
        public static readonly var B2 = (var)65;
        public static readonly var B3 = (var)66;
        public static readonly var B4 = (var)67;
        public static readonly var B5 = (var)68;
        public static readonly var B6 = (var)69;
        public static readonly var B7 = (var)70;
        public static readonly var B8 = (var)71;
        public static readonly var B9 = (var)72;
        public static readonly var B10 = (var)73;
        public static readonly var B11 = (var)74;
        public static readonly var B12 = (var)75;
        public static readonly var B13 = (var)76;
        public static readonly var B14 = (var)77;
        public static readonly var B15 = (var)78;
        public static readonly var B16 = (var)79;
        public static readonly var B17 = (var)80;
        public static readonly var B18 = (var)81;
        public static readonly var B19 = (var)82;
        public static readonly var B20 = (var)83;
        public static readonly var B21 = (var)84;
        public static readonly var B22 = (var)85;
        public static readonly var B23 = (var)86;
        public static readonly var B24 = (var)87;
        public static readonly var B25 = (var)88;
        public static readonly var B26 = (var)89;
        public static readonly var B27 = (var)90;
        public static readonly var B28 = (var)91;
        public static readonly var B29 = (var)92;
        public static readonly var B30 = (var)93;
        public static readonly var B31 = (var)94;

        public static readonly var H0 = (var)95;
        public static readonly var H1 = (var)96;
        public static readonly var H2 = (var)97;
        public static readonly var H3 = (var)98;
        public static readonly var H4 = (var)99;
        public static readonly var H5 = (var)100;
        public static readonly var H6 = (var)101;
        public static readonly var H7 = (var)102;
        public static readonly var H8 = (var)103;
        public static readonly var H9 = (var)104;
        public static readonly var H10 = (var)105;
        public static readonly var H11 = (var)106;
        public static readonly var H12 = (var)107;
        public static readonly var H13 = (var)108;
        public static readonly var H14 = (var)109;
        public static readonly var H15 = (var)110;
        public static readonly var H16 = (var)111;
        public static readonly var H17 = (var)112;
        public static readonly var H18 = (var)113;
        public static readonly var H19 = (var)114;
        public static readonly var H20 = (var)115;
        public static readonly var H21 = (var)116;
        public static readonly var H22 = (var)117;
        public static readonly var H23 = (var)118;
        public static readonly var H24 = (var)119;
        public static readonly var H25 = (var)120;
        public static readonly var H26 = (var)121;
        public static readonly var H27 = (var)122;
        public static readonly var H28 = (var)123;
        public static readonly var H29 = (var)124;
        public static readonly var H30 = (var)125;
        public static readonly var H31 = (var)126;

        public static readonly var S0 = (var)127;
        public static readonly var S1 = (var)128;
        public static readonly var S2 = (var)129;
        public static readonly var S3 = (var)130;
        public static readonly var S4 = (var)131;
        public static readonly var S5 = (var)132;
        public static readonly var S6 = (var)133;
        public static readonly var S7 = (var)134;
        public static readonly var S8 = (var)135;
        public static readonly var S9 = (var)136;
        public static readonly var S10 = (var)137;
        public static readonly var S11 = (var)138;
        public static readonly var S12 = (var)139;
        public static readonly var S13 = (var)140;
        public static readonly var S14 = (var)141;
        public static readonly var S15 = (var)142;
        public static readonly var S16 = (var)143;
        public static readonly var S17 = (var)144;
        public static readonly var S18 = (var)145;
        public static readonly var S19 = (var)146;
        public static readonly var S20 = (var)147;
        public static readonly var S21 = (var)148;
        public static readonly var S22 = (var)149;
        public static readonly var S23 = (var)150;
        public static readonly var S24 = (var)151;
        public static readonly var S25 = (var)152;
        public static readonly var S26 = (var)153;
        public static readonly var S27 = (var)154;
        public static readonly var S28 = (var)155;
        public static readonly var S29 = (var)156;
        public static readonly var S30 = (var)157;
        public static readonly var S31 = (var)158;

        public static readonly var D0 = (var)159;
        public static readonly var D1 = (var)160;
        public static readonly var D2 = (var)161;
        public static readonly var D3 = (var)162;
        public static readonly var D4 = (var)163;
        public static readonly var D5 = (var)164;
        public static readonly var D6 = (var)165;
        public static readonly var D7 = (var)166;
        public static readonly var D8 = (var)167;
        public static readonly var D9 = (var)168;
        public static readonly var D10 = (var)169;
        public static readonly var D11 = (var)170;
        public static readonly var D12 = (var)171;
        public static readonly var D13 = (var)172;
        public static readonly var D14 = (var)173;
        public static readonly var D15 = (var)174;
        public static readonly var D16 = (var)175;
        public static readonly var D17 = (var)176;
        public static readonly var D18 = (var)177;
        public static readonly var D19 = (var)178;
        public static readonly var D20 = (var)179;
        public static readonly var D21 = (var)180;
        public static readonly var D22 = (var)181;
        public static readonly var D23 = (var)182;
        public static readonly var D24 = (var)183;
        public static readonly var D25 = (var)184;
        public static readonly var D26 = (var)185;
        public static readonly var D27 = (var)186;
        public static readonly var D28 = (var)187;
        public static readonly var D29 = (var)188;
        public static readonly var D30 = (var)189;
        public static readonly var D31 = (var)190;

        public static readonly var Q0 = (var)191;
        public static readonly var Q1 = (var)192;
        public static readonly var Q2 = (var)193;
        public static readonly var Q3 = (var)194;
        public static readonly var Q4 = (var)195;
        public static readonly var Q5 = (var)196;
        public static readonly var Q6 = (var)197;
        public static readonly var Q7 = (var)198;
        public static readonly var Q8 = (var)199;
        public static readonly var Q9 = (var)200;
        public static readonly var Q10 = (var)201;
        public static readonly var Q11 = (var)202;
        public static readonly var Q12 = (var)203;
        public static readonly var Q13 = (var)204;
        public static readonly var Q14 = (var)205;
        public static readonly var Q15 = (var)206;
        public static readonly var Q16 = (var)207;
        public static readonly var Q17 = (var)208;
        public static readonly var Q18 = (var)209;
        public static readonly var Q19 = (var)210;
        public static readonly var Q20 = (var)211;
        public static readonly var Q21 = (var)212;
        public static readonly var Q22 = (var)213;
        public static readonly var Q23 = (var)214;
        public static readonly var Q24 = (var)215;
        public static readonly var Q25 = (var)216;
        public static readonly var Q26 = (var)217;
        public static readonly var Q27 = (var)218;
        public static readonly var Q28 = (var)219;
        public static readonly var Q29 = (var)220;
        public static readonly var Q30 = (var)221;
        public static readonly var Q31 = (var)222;

        public static readonly var V0 = (var)223;
        public static readonly var V1 = (var)224;
        public static readonly var V2 = (var)225;
        public static readonly var V3 = (var)226;
        public static readonly var V4 = (var)227;
        public static readonly var V5 = (var)228;
        public static readonly var V6 = (var)229;
        public static readonly var V7 = (var)230;
        public static readonly var V8 = (var)231;
        public static readonly var V9 = (var)232;
        public static readonly var V10 = (var)233;
        public static readonly var V11 = (var)234;
        public static readonly var V12 = (var)235;
        public static readonly var V13 = (var)236;
        public static readonly var V14 = (var)237;
        public static readonly var V15 = (var)238;
        public static readonly var V16 = (var)239;
        public static readonly var V17 = (var)240;
        public static readonly var V18 = (var)241;
        public static readonly var V19 = (var)242;
        public static readonly var V20 = (var)243;
        public static readonly var V21 = (var)244;
        public static readonly var V22 = (var)245;
        public static readonly var V23 = (var)246;
        public static readonly var V24 = (var)247;
        public static readonly var V25 = (var)248;
        public static readonly var V26 = (var)249;
        public static readonly var V27 = (var)250;
        public static readonly var V28 = (var)251;
        public static readonly var V29 = (var)252;
        public static readonly var V30 = (var)253;
        public static readonly WSP V31 = (WSP)WZR; // These are different registers with the same encoding.
        public static readonly var SP = (var)XZR; // These are different registers with the same encoding.

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
        private static readonly var uxtb = (var)0;
        private static readonly var uxth = (var)1;
        private static readonly var uxtw = (var)2;
        private static readonly var uxtx = (var)3;
        private static readonly var sxtb = (var)4;
        private static readonly var sxth = (var)5;
        private static readonly var sxtw = (var)6;
        private static readonly var sxtx = (var)7;
        private static readonly var lsl = (var)8;
        private static readonly var lsr = (var)9;
        private static readonly var asr = (var)10;
        private static readonly var ror = (var)11;


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
        public static readonly var AddrPostIndex = (var)0; // [R], X - use address R, set R = R + X
        public static readonly var AddrPreIndex = (var)1; // [R, X]! - use address R + X, set R = R + X
        public static readonly var AddrOffset = (var)2; // [R, X] - use address R + X
        public static readonly var AddrPostReg = (var)3; // [Rn], Rm - - use address Rn, set Rn = Rn + Rm

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
        public static readonly var DAIFSet = (var)0;
        public static readonly var DAIFClr = (var)1;


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
        public static readonly var ArrangementB = (var)0;
        public static readonly var Arrangement8B = (var)1;
        public static readonly var Arrangement16B = (var)2;
        public static readonly var ArrangementH = (var)3;
        public static readonly var Arrangement4H = (var)4;
        public static readonly var Arrangement8H = (var)5;
        public static readonly var ArrangementS = (var)6;
        public static readonly var Arrangement2S = (var)7;
        public static readonly var Arrangement4S = (var)8;
        public static readonly var ArrangementD = (var)9;
        public static readonly var Arrangement1D = (var)10;
        public static readonly var Arrangement2D = (var)11;
        public static readonly var Arrangement1Q = (var)12;


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
