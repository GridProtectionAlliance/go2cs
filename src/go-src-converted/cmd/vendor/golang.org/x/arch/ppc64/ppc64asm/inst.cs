// Copyright 2014 The Go Authors.  All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ppc64asm -- go2cs converted at 2020 October 08 04:44:44 UTC
// import "cmd/vendor/golang.org/x/arch/ppc64/ppc64asm" ==> using ppc64asm = go.cmd.vendor.golang.org.x.arch.ppc64.ppc64asm_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\arch\ppc64\ppc64asm\inst.go
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace vendor {
namespace golang.org {
namespace x {
namespace arch {
namespace ppc64
{
    public static partial class ppc64asm_package
    {
        public partial struct Inst
        {
            public Op Op; // Opcode mnemonic
            public uint Enc; // Raw encoding bits
            public long Len; // Length of encoding in bytes.
            public Args Args; // Instruction arguments, in Power ISA manual order.
        }

        public static @string String(this Inst i)
        {
            bytes.Buffer buf = default;
            buf.WriteString(i.Op.String());
            foreach (var (j, arg) in i.Args)
            {
                if (arg == null)
                {
                    break;
                }

                if (j == 0L)
                {
                    buf.WriteString(" ");
                }
                else
                {
                    buf.WriteString(", ");
                }

                buf.WriteString(arg.String());

            }
            return buf.String();

        }

        // An Op is an instruction operation.
        public partial struct Op // : ushort
        {
        }

        public static @string String(this Op o)
        {
            if (int(o) >= len(opstr) || opstr[o] == "")
            {
                return fmt.Sprintf("Op(%d)", int(o));
            }

            return opstr[o];

        }

        // An Arg is a single instruction argument, one of these types: Reg, CondReg, SpReg, Imm, PCRel, Label, or Offset.
        public partial interface Arg
        {
            @string IsArg();
            @string String();
        }

        // An Args holds the instruction arguments.
        // If an instruction has fewer than 4 arguments,
        // the final elements in the array are nil.
        public partial struct Args // : array<Arg>
        {
        }

        // A Reg is a single register. The zero value means R0, not the absence of a register.
        // It also includes special registers.
        public partial struct Reg // : ushort
        {
        }

        private static readonly Reg _ = (Reg)iota;
        public static readonly var R0 = (var)0;
        public static readonly var R1 = (var)1;
        public static readonly var R2 = (var)2;
        public static readonly var R3 = (var)3;
        public static readonly var R4 = (var)4;
        public static readonly var R5 = (var)5;
        public static readonly var R6 = (var)6;
        public static readonly var R7 = (var)7;
        public static readonly var R8 = (var)8;
        public static readonly var R9 = (var)9;
        public static readonly var R10 = (var)10;
        public static readonly var R11 = (var)11;
        public static readonly var R12 = (var)12;
        public static readonly var R13 = (var)13;
        public static readonly var R14 = (var)14;
        public static readonly var R15 = (var)15;
        public static readonly var R16 = (var)16;
        public static readonly var R17 = (var)17;
        public static readonly var R18 = (var)18;
        public static readonly var R19 = (var)19;
        public static readonly var R20 = (var)20;
        public static readonly var R21 = (var)21;
        public static readonly var R22 = (var)22;
        public static readonly var R23 = (var)23;
        public static readonly var R24 = (var)24;
        public static readonly var R25 = (var)25;
        public static readonly var R26 = (var)26;
        public static readonly var R27 = (var)27;
        public static readonly var R28 = (var)28;
        public static readonly var R29 = (var)29;
        public static readonly var R30 = (var)30;
        public static readonly var R31 = (var)31;
        public static readonly var F0 = (var)32;
        public static readonly var F1 = (var)33;
        public static readonly var F2 = (var)34;
        public static readonly var F3 = (var)35;
        public static readonly var F4 = (var)36;
        public static readonly var F5 = (var)37;
        public static readonly var F6 = (var)38;
        public static readonly var F7 = (var)39;
        public static readonly var F8 = (var)40;
        public static readonly var F9 = (var)41;
        public static readonly var F10 = (var)42;
        public static readonly var F11 = (var)43;
        public static readonly var F12 = (var)44;
        public static readonly var F13 = (var)45;
        public static readonly var F14 = (var)46;
        public static readonly var F15 = (var)47;
        public static readonly var F16 = (var)48;
        public static readonly var F17 = (var)49;
        public static readonly var F18 = (var)50;
        public static readonly var F19 = (var)51;
        public static readonly var F20 = (var)52;
        public static readonly var F21 = (var)53;
        public static readonly var F22 = (var)54;
        public static readonly var F23 = (var)55;
        public static readonly var F24 = (var)56;
        public static readonly var F25 = (var)57;
        public static readonly var F26 = (var)58;
        public static readonly var F27 = (var)59;
        public static readonly var F28 = (var)60;
        public static readonly var F29 = (var)61;
        public static readonly var F30 = (var)62;
        public static readonly var F31 = (var)63;
        public static readonly var V0 = (var)64; // VSX extension, F0 is V0[0:63].
        public static readonly var V1 = (var)65;
        public static readonly var V2 = (var)66;
        public static readonly var V3 = (var)67;
        public static readonly var V4 = (var)68;
        public static readonly var V5 = (var)69;
        public static readonly var V6 = (var)70;
        public static readonly var V7 = (var)71;
        public static readonly var V8 = (var)72;
        public static readonly var V9 = (var)73;
        public static readonly var V10 = (var)74;
        public static readonly var V11 = (var)75;
        public static readonly var V12 = (var)76;
        public static readonly var V13 = (var)77;
        public static readonly var V14 = (var)78;
        public static readonly var V15 = (var)79;
        public static readonly var V16 = (var)80;
        public static readonly var V17 = (var)81;
        public static readonly var V18 = (var)82;
        public static readonly var V19 = (var)83;
        public static readonly var V20 = (var)84;
        public static readonly var V21 = (var)85;
        public static readonly var V22 = (var)86;
        public static readonly var V23 = (var)87;
        public static readonly var V24 = (var)88;
        public static readonly var V25 = (var)89;
        public static readonly var V26 = (var)90;
        public static readonly var V27 = (var)91;
        public static readonly var V28 = (var)92;
        public static readonly var V29 = (var)93;
        public static readonly var V30 = (var)94;
        public static readonly var V31 = (var)95;
        public static readonly var VS0 = (var)96;
        public static readonly var VS1 = (var)97;
        public static readonly var VS2 = (var)98;
        public static readonly var VS3 = (var)99;
        public static readonly var VS4 = (var)100;
        public static readonly var VS5 = (var)101;
        public static readonly var VS6 = (var)102;
        public static readonly var VS7 = (var)103;
        public static readonly var VS8 = (var)104;
        public static readonly var VS9 = (var)105;
        public static readonly var VS10 = (var)106;
        public static readonly var VS11 = (var)107;
        public static readonly var VS12 = (var)108;
        public static readonly var VS13 = (var)109;
        public static readonly var VS14 = (var)110;
        public static readonly var VS15 = (var)111;
        public static readonly var VS16 = (var)112;
        public static readonly var VS17 = (var)113;
        public static readonly var VS18 = (var)114;
        public static readonly var VS19 = (var)115;
        public static readonly var VS20 = (var)116;
        public static readonly var VS21 = (var)117;
        public static readonly var VS22 = (var)118;
        public static readonly var VS23 = (var)119;
        public static readonly var VS24 = (var)120;
        public static readonly var VS25 = (var)121;
        public static readonly var VS26 = (var)122;
        public static readonly var VS27 = (var)123;
        public static readonly var VS28 = (var)124;
        public static readonly var VS29 = (var)125;
        public static readonly var VS30 = (var)126;
        public static readonly var VS31 = (var)127;
        public static readonly var VS32 = (var)128;
        public static readonly var VS33 = (var)129;
        public static readonly var VS34 = (var)130;
        public static readonly var VS35 = (var)131;
        public static readonly var VS36 = (var)132;
        public static readonly var VS37 = (var)133;
        public static readonly var VS38 = (var)134;
        public static readonly var VS39 = (var)135;
        public static readonly var VS40 = (var)136;
        public static readonly var VS41 = (var)137;
        public static readonly var VS42 = (var)138;
        public static readonly var VS43 = (var)139;
        public static readonly var VS44 = (var)140;
        public static readonly var VS45 = (var)141;
        public static readonly var VS46 = (var)142;
        public static readonly var VS47 = (var)143;
        public static readonly var VS48 = (var)144;
        public static readonly var VS49 = (var)145;
        public static readonly var VS50 = (var)146;
        public static readonly var VS51 = (var)147;
        public static readonly var VS52 = (var)148;
        public static readonly var VS53 = (var)149;
        public static readonly var VS54 = (var)150;
        public static readonly var VS55 = (var)151;
        public static readonly var VS56 = (var)152;
        public static readonly var VS57 = (var)153;
        public static readonly var VS58 = (var)154;
        public static readonly var VS59 = (var)155;
        public static readonly var VS60 = (var)156;
        public static readonly var VS61 = (var)157;
        public static readonly var VS62 = (var)158;
        public static readonly var VS63 = (var)159;


        public static void IsArg(this Reg _p0)
        {
        }
        public static @string String(this Reg r)
        {

            if (R0 <= r && r <= R31) 
                return fmt.Sprintf("r%d", int(r - R0));
            else if (F0 <= r && r <= F31) 
                return fmt.Sprintf("f%d", int(r - F0));
            else if (V0 <= r && r <= V31) 
                return fmt.Sprintf("v%d", int(r - V0));
            else if (VS0 <= r && r <= VS63) 
                return fmt.Sprintf("vs%d", int(r - VS0));
            else 
                return fmt.Sprintf("Reg(%d)", int(r));
            
        }

        // CondReg is a bit or field in the condition register.
        public partial struct CondReg // : sbyte
        {
        }

        private static readonly CondReg _ = (CondReg)iota; 
        // Condition Regster bits
        public static readonly var Cond0LT = (var)0;
        public static readonly var Cond0GT = (var)1;
        public static readonly var Cond0EQ = (var)2;
        public static readonly var Cond0SO = (var)3;
        public static readonly var Cond1LT = (var)4;
        public static readonly var Cond1GT = (var)5;
        public static readonly var Cond1EQ = (var)6;
        public static readonly var Cond1SO = (var)7;
        public static readonly var Cond2LT = (var)8;
        public static readonly var Cond2GT = (var)9;
        public static readonly var Cond2EQ = (var)10;
        public static readonly var Cond2SO = (var)11;
        public static readonly var Cond3LT = (var)12;
        public static readonly var Cond3GT = (var)13;
        public static readonly var Cond3EQ = (var)14;
        public static readonly var Cond3SO = (var)15;
        public static readonly var Cond4LT = (var)16;
        public static readonly var Cond4GT = (var)17;
        public static readonly var Cond4EQ = (var)18;
        public static readonly var Cond4SO = (var)19;
        public static readonly var Cond5LT = (var)20;
        public static readonly var Cond5GT = (var)21;
        public static readonly var Cond5EQ = (var)22;
        public static readonly var Cond5SO = (var)23;
        public static readonly var Cond6LT = (var)24;
        public static readonly var Cond6GT = (var)25;
        public static readonly var Cond6EQ = (var)26;
        public static readonly var Cond6SO = (var)27;
        public static readonly var Cond7LT = (var)28;
        public static readonly var Cond7GT = (var)29;
        public static readonly var Cond7EQ = (var)30;
        public static readonly var Cond7SO = (var)31; 
        // Condition Register Fields
        public static readonly var CR0 = (var)32;
        public static readonly var CR1 = (var)33;
        public static readonly var CR2 = (var)34;
        public static readonly var CR3 = (var)35;
        public static readonly var CR4 = (var)36;
        public static readonly var CR5 = (var)37;
        public static readonly var CR6 = (var)38;
        public static readonly var CR7 = (var)39;


        public static void IsArg(this CondReg _p0)
        {
        }
        public static @string String(this CondReg c)
        {

            if (c >= CR0) 
                return fmt.Sprintf("CR%d", int(c - CR0));
            else if (c >= Cond0LT && c < CR0) 
                return fmt.Sprintf("Cond%d%s", int((c - Cond0LT) / 4L), new array<@string>(new @string[] { "LT", "GT", "EQ", "SO" })[(c - Cond0LT) % 4L]);
            else 
                return fmt.Sprintf("CondReg(%d)", int(c));
            
        }

        // SpReg is a special register, its meaning depends on Op.
        public partial struct SpReg // : ushort
        {
        }

        public static readonly SpReg SpRegZero = (SpReg)0L;


        public static void IsArg(this SpReg _p0)
        {
        }
        public static @string String(this SpReg s)
        {
            return fmt.Sprintf("SpReg(%d)", int(s));
        }

        // PCRel is a PC-relative offset, used only in branch instructions.
        public partial struct PCRel // : int
        {
        }

        public static void IsArg(this PCRel _p0)
        {
        }
        public static @string String(this PCRel r)
        {
            return fmt.Sprintf("PC%+#x", int32(r));
        }

        // A Label is a code (text) address, used only in absolute branch instructions.
        public partial struct Label // : uint
        {
        }

        public static void IsArg(this Label _p0)
        {
        }
        public static @string String(this Label l)
        {
            return fmt.Sprintf("%#x", uint32(l));
        }

        // Imm represents an immediate number.
        public partial struct Imm // : int
        {
        }

        public static void IsArg(this Imm _p0)
        {
        }
        public static @string String(this Imm i)
        {
            return fmt.Sprintf("%d", int32(i));
        }

        // Offset represents a memory offset immediate.
        public partial struct Offset // : int
        {
        }

        public static void IsArg(this Offset _p0)
        {
        }
        public static @string String(this Offset o)
        {
            return fmt.Sprintf("%+d", int32(o));
        }
    }
}}}}}}}
