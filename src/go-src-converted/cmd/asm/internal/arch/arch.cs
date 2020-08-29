// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package arch -- go2cs converted at 2020 August 29 08:48:42 UTC
// import "cmd/asm/internal/arch" ==> using arch = go.cmd.asm.@internal.arch_package
// Original source: C:\Go\src\cmd\asm\internal\arch\arch.go
using obj = go.cmd.@internal.obj_package;
using arm = go.cmd.@internal.obj.arm_package;
using arm64 = go.cmd.@internal.obj.arm64_package;
using mips = go.cmd.@internal.obj.mips_package;
using ppc64 = go.cmd.@internal.obj.ppc64_package;
using s390x = go.cmd.@internal.obj.s390x_package;
using x86 = go.cmd.@internal.obj.x86_package;
using fmt = go.fmt_package;
using strings = go.strings_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace asm {
namespace @internal
{
    public static partial class arch_package
    {
        // Pseudo-registers whose names are the constant name without the leading R.
        public static readonly var RFP = -(iota + 1L);
        public static readonly var RSB = 0;
        public static readonly var RSP = 1;
        public static readonly var RPC = 2;

        // Arch wraps the link architecture object with more architecture-specific information.
        public partial struct Arch
        {
            public ref obj.LinkArch LinkArch => ref LinkArch_ptr; // Map of instruction names to enumeration.
            public map<@string, obj.As> Instructions; // Map of register names to enumeration.
            public map<@string, short> Register; // Table of register prefix names. These are things like R for R(0) and SPR for SPR(268).
            public map<@string, bool> RegisterPrefix; // RegisterNumber converts R(10) into arm.REG_R10.
            public Func<@string, short, (short, bool)> RegisterNumber; // Instruction is a jump.
            public Func<@string, bool> IsJump;
        }

        // nilRegisterNumber is the register number function for architectures
        // that do not accept the R(N) notation. It always returns failure.
        private static (short, bool) nilRegisterNumber(@string name, short n)
        {
            return (0L, false);
        }

        // Set configures the architecture specified by GOARCH and returns its representation.
        // It returns nil if GOARCH is not recognized.
        public static ref Arch Set(@string GOARCH)
        {
            switch (GOARCH)
            {
                case "386": 
                    return archX86(ref x86.Link386);
                    break;
                case "amd64": 
                    return archX86(ref x86.Linkamd64);
                    break;
                case "amd64p32": 
                    return archX86(ref x86.Linkamd64p32);
                    break;
                case "arm": 
                    return archArm();
                    break;
                case "arm64": 
                    return archArm64();
                    break;
                case "mips": 
                    var a = archMips();
                    a.LinkArch = ref mips.Linkmips;
                    return a;
                    break;
                case "mipsle": 
                    a = archMips();
                    a.LinkArch = ref mips.Linkmipsle;
                    return a;
                    break;
                case "mips64": 
                    a = archMips64();
                    a.LinkArch = ref mips.Linkmips64;
                    return a;
                    break;
                case "mips64le": 
                    a = archMips64();
                    a.LinkArch = ref mips.Linkmips64le;
                    return a;
                    break;
                case "ppc64": 
                    a = archPPC64();
                    a.LinkArch = ref ppc64.Linkppc64;
                    return a;
                    break;
                case "ppc64le": 
                    a = archPPC64();
                    a.LinkArch = ref ppc64.Linkppc64le;
                    return a;
                    break;
                case "s390x": 
                    a = archS390x();
                    a.LinkArch = ref s390x.Links390x;
                    return a;
                    break;
            }
            return null;
        }

        private static bool jumpX86(@string word)
        {
            return word[0L] == 'J' || word == "CALL" || strings.HasPrefix(word, "LOOP") || word == "XBEGIN";
        }

        private static ref Arch archX86(ref obj.LinkArch linkArch)
        {
            var register = make_map<@string, short>(); 
            // Create maps for easy lookup of instruction names etc.
            {
                var i__prev1 = i;
                var s__prev1 = s;

                foreach (var (__i, __s) in x86.Register)
                {
                    i = __i;
                    s = __s;
                    register[s] = int16(i + x86.REG_AL);
                } 
                // Pseudo-registers.

                i = i__prev1;
                s = s__prev1;
            }

            register["SB"] = RSB;
            register["FP"] = RFP;
            register["PC"] = RPC; 
            // Register prefix not used on this architecture.

            var instructions = make_map<@string, obj.As>();
            {
                var i__prev1 = i;
                var s__prev1 = s;

                foreach (var (__i, __s) in obj.Anames)
                {
                    i = __i;
                    s = __s;
                    instructions[s] = obj.As(i);
                }

                i = i__prev1;
                s = s__prev1;
            }

            {
                var i__prev1 = i;
                var s__prev1 = s;

                foreach (var (__i, __s) in x86.Anames)
                {
                    i = __i;
                    s = __s;
                    if (obj.As(i) >= obj.A_ARCHSPECIFIC)
                    {
                        instructions[s] = obj.As(i) + obj.ABaseAMD64;
                    }
                } 
                // Annoying aliases.

                i = i__prev1;
                s = s__prev1;
            }

            instructions["JA"] = x86.AJHI; /* alternate */
            instructions["JAE"] = x86.AJCC; /* alternate */
            instructions["JB"] = x86.AJCS; /* alternate */
            instructions["JBE"] = x86.AJLS; /* alternate */
            instructions["JC"] = x86.AJCS; /* alternate */
            instructions["JCC"] = x86.AJCC; /* carry clear (CF = 0) */
            instructions["JCS"] = x86.AJCS; /* carry set (CF = 1) */
            instructions["JE"] = x86.AJEQ; /* alternate */
            instructions["JEQ"] = x86.AJEQ; /* equal (ZF = 1) */
            instructions["JG"] = x86.AJGT; /* alternate */
            instructions["JGE"] = x86.AJGE; /* greater than or equal (signed) (SF = OF) */
            instructions["JGT"] = x86.AJGT; /* greater than (signed) (ZF = 0 && SF = OF) */
            instructions["JHI"] = x86.AJHI; /* higher (unsigned) (CF = 0 && ZF = 0) */
            instructions["JHS"] = x86.AJCC; /* alternate */
            instructions["JL"] = x86.AJLT; /* alternate */
            instructions["JLE"] = x86.AJLE; /* less than or equal (signed) (ZF = 1 || SF != OF) */
            instructions["JLO"] = x86.AJCS; /* alternate */
            instructions["JLS"] = x86.AJLS; /* lower or same (unsigned) (CF = 1 || ZF = 1) */
            instructions["JLT"] = x86.AJLT; /* less than (signed) (SF != OF) */
            instructions["JMI"] = x86.AJMI; /* negative (minus) (SF = 1) */
            instructions["JNA"] = x86.AJLS; /* alternate */
            instructions["JNAE"] = x86.AJCS; /* alternate */
            instructions["JNB"] = x86.AJCC; /* alternate */
            instructions["JNBE"] = x86.AJHI; /* alternate */
            instructions["JNC"] = x86.AJCC; /* alternate */
            instructions["JNE"] = x86.AJNE; /* not equal (ZF = 0) */
            instructions["JNG"] = x86.AJLE; /* alternate */
            instructions["JNGE"] = x86.AJLT; /* alternate */
            instructions["JNL"] = x86.AJGE; /* alternate */
            instructions["JNLE"] = x86.AJGT; /* alternate */
            instructions["JNO"] = x86.AJOC; /* alternate */
            instructions["JNP"] = x86.AJPC; /* alternate */
            instructions["JNS"] = x86.AJPL; /* alternate */
            instructions["JNZ"] = x86.AJNE; /* alternate */
            instructions["JO"] = x86.AJOS; /* alternate */
            instructions["JOC"] = x86.AJOC; /* overflow clear (OF = 0) */
            instructions["JOS"] = x86.AJOS; /* overflow set (OF = 1) */
            instructions["JP"] = x86.AJPS; /* alternate */
            instructions["JPC"] = x86.AJPC; /* parity clear (PF = 0) */
            instructions["JPE"] = x86.AJPS; /* alternate */
            instructions["JPL"] = x86.AJPL; /* non-negative (plus) (SF = 0) */
            instructions["JPO"] = x86.AJPC; /* alternate */
            instructions["JPS"] = x86.AJPS; /* parity set (PF = 1) */
            instructions["JS"] = x86.AJMI; /* alternate */
            instructions["JZ"] = x86.AJEQ; /* alternate */
            instructions["MASKMOVDQU"] = x86.AMASKMOVOU;
            instructions["MOVD"] = x86.AMOVQ;
            instructions["MOVDQ2Q"] = x86.AMOVQ;
            instructions["MOVNTDQ"] = x86.AMOVNTO;
            instructions["MOVOA"] = x86.AMOVO;
            instructions["PSLLDQ"] = x86.APSLLO;
            instructions["PSRLDQ"] = x86.APSRLO;
            instructions["PADDD"] = x86.APADDL;

            return ref new Arch(LinkArch:linkArch,Instructions:instructions,Register:register,RegisterPrefix:nil,RegisterNumber:nilRegisterNumber,IsJump:jumpX86,);
        }

        private static ref Arch archArm()
        {
            var register = make_map<@string, short>(); 
            // Create maps for easy lookup of instruction names etc.
            // Note that there is no list of names as there is for x86.
            {
                var i__prev1 = i;

                for (var i = arm.REG_R0; i < arm.REG_SPSR; i++)
                {
                    register[obj.Rconv(i)] = int16(i);
                } 
                // Avoid unintentionally clobbering g using R10.


                i = i__prev1;
            } 
            // Avoid unintentionally clobbering g using R10.
            delete(register, "R10");
            register["g"] = arm.REG_R10;
            {
                var i__prev1 = i;

                for (i = 0L; i < 16L; i++)
                {
                    register[fmt.Sprintf("C%d", i)] = int16(i);
                } 

                // Pseudo-registers.


                i = i__prev1;
            } 

            // Pseudo-registers.
            register["SB"] = RSB;
            register["FP"] = RFP;
            register["PC"] = RPC;
            register["SP"] = RSP;
            map registerPrefix = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, bool>{"F":true,"R":true,};

            var instructions = make_map<@string, obj.As>();
            {
                var i__prev1 = i;
                var s__prev1 = s;

                foreach (var (__i, __s) in obj.Anames)
                {
                    i = __i;
                    s = __s;
                    instructions[s] = obj.As(i);
                }

                i = i__prev1;
                s = s__prev1;
            }

            {
                var i__prev1 = i;
                var s__prev1 = s;

                foreach (var (__i, __s) in arm.Anames)
                {
                    i = __i;
                    s = __s;
                    if (obj.As(i) >= obj.A_ARCHSPECIFIC)
                    {
                        instructions[s] = obj.As(i) + obj.ABaseARM;
                    }
                } 
                // Annoying aliases.

                i = i__prev1;
                s = s__prev1;
            }

            instructions["B"] = obj.AJMP;
            instructions["BL"] = obj.ACALL; 
            // MCR differs from MRC by the way fields of the word are encoded.
            // (Details in arm.go). Here we add the instruction so parse will find
            // it, but give it an opcode number known only to us.
            instructions["MCR"] = aMCR;

            return ref new Arch(LinkArch:&arm.Linkarm,Instructions:instructions,Register:register,RegisterPrefix:registerPrefix,RegisterNumber:armRegisterNumber,IsJump:jumpArm,);
        }

        private static ref Arch archArm64()
        {
            var register = make_map<@string, short>(); 
            // Create maps for easy lookup of instruction names etc.
            // Note that there is no list of names as there is for 386 and amd64.
            register[obj.Rconv(arm64.REGSP)] = int16(arm64.REGSP);
            {
                var i__prev1 = i;

                for (var i = arm64.REG_R0; i <= arm64.REG_R31; i++)
                {
                    register[obj.Rconv(i)] = int16(i);
                }


                i = i__prev1;
            }
            {
                var i__prev1 = i;

                for (i = arm64.REG_F0; i <= arm64.REG_F31; i++)
                {
                    register[obj.Rconv(i)] = int16(i);
                }


                i = i__prev1;
            }
            {
                var i__prev1 = i;

                for (i = arm64.REG_V0; i <= arm64.REG_V31; i++)
                {
                    register[obj.Rconv(i)] = int16(i);
                }


                i = i__prev1;
            }
            register["LR"] = arm64.REGLINK;
            register["DAIF"] = arm64.REG_DAIF;
            register["NZCV"] = arm64.REG_NZCV;
            register["FPSR"] = arm64.REG_FPSR;
            register["FPCR"] = arm64.REG_FPCR;
            register["SPSR_EL1"] = arm64.REG_SPSR_EL1;
            register["ELR_EL1"] = arm64.REG_ELR_EL1;
            register["SPSR_EL2"] = arm64.REG_SPSR_EL2;
            register["ELR_EL2"] = arm64.REG_ELR_EL2;
            register["CurrentEL"] = arm64.REG_CurrentEL;
            register["SP_EL0"] = arm64.REG_SP_EL0;
            register["SPSel"] = arm64.REG_SPSel;
            register["DAIFSet"] = arm64.REG_DAIFSet;
            register["DAIFClr"] = arm64.REG_DAIFClr; 
            // Conditional operators, like EQ, NE, etc.
            register["EQ"] = arm64.COND_EQ;
            register["NE"] = arm64.COND_NE;
            register["HS"] = arm64.COND_HS;
            register["CS"] = arm64.COND_HS;
            register["LO"] = arm64.COND_LO;
            register["CC"] = arm64.COND_LO;
            register["MI"] = arm64.COND_MI;
            register["PL"] = arm64.COND_PL;
            register["VS"] = arm64.COND_VS;
            register["VC"] = arm64.COND_VC;
            register["HI"] = arm64.COND_HI;
            register["LS"] = arm64.COND_LS;
            register["GE"] = arm64.COND_GE;
            register["LT"] = arm64.COND_LT;
            register["GT"] = arm64.COND_GT;
            register["LE"] = arm64.COND_LE;
            register["AL"] = arm64.COND_AL;
            register["NV"] = arm64.COND_NV; 
            // Pseudo-registers.
            register["SB"] = RSB;
            register["FP"] = RFP;
            register["PC"] = RPC;
            register["SP"] = RSP; 
            // Avoid unintentionally clobbering g using R28.
            delete(register, "R28");
            register["g"] = arm64.REG_R28;
            map registerPrefix = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, bool>{"F":true,"R":true,"V":true,};

            var instructions = make_map<@string, obj.As>();
            {
                var i__prev1 = i;
                var s__prev1 = s;

                foreach (var (__i, __s) in obj.Anames)
                {
                    i = __i;
                    s = __s;
                    instructions[s] = obj.As(i);
                }

                i = i__prev1;
                s = s__prev1;
            }

            {
                var i__prev1 = i;
                var s__prev1 = s;

                foreach (var (__i, __s) in arm64.Anames)
                {
                    i = __i;
                    s = __s;
                    if (obj.As(i) >= obj.A_ARCHSPECIFIC)
                    {
                        instructions[s] = obj.As(i) + obj.ABaseARM64;
                    }
                } 
                // Annoying aliases.

                i = i__prev1;
                s = s__prev1;
            }

            instructions["B"] = arm64.AB;
            instructions["BL"] = arm64.ABL;

            return ref new Arch(LinkArch:&arm64.Linkarm64,Instructions:instructions,Register:register,RegisterPrefix:registerPrefix,RegisterNumber:arm64RegisterNumber,IsJump:jumpArm64,);

        }

        private static ref Arch archPPC64()
        {
            var register = make_map<@string, short>(); 
            // Create maps for easy lookup of instruction names etc.
            // Note that there is no list of names as there is for x86.
            {
                var i__prev1 = i;

                for (var i = ppc64.REG_R0; i <= ppc64.REG_R31; i++)
                {
                    register[obj.Rconv(i)] = int16(i);
                }


                i = i__prev1;
            }
            {
                var i__prev1 = i;

                for (i = ppc64.REG_F0; i <= ppc64.REG_F31; i++)
                {
                    register[obj.Rconv(i)] = int16(i);
                }


                i = i__prev1;
            }
            {
                var i__prev1 = i;

                for (i = ppc64.REG_V0; i <= ppc64.REG_V31; i++)
                {
                    register[obj.Rconv(i)] = int16(i);
                }


                i = i__prev1;
            }
            {
                var i__prev1 = i;

                for (i = ppc64.REG_VS0; i <= ppc64.REG_VS63; i++)
                {
                    register[obj.Rconv(i)] = int16(i);
                }


                i = i__prev1;
            }
            {
                var i__prev1 = i;

                for (i = ppc64.REG_CR0; i <= ppc64.REG_CR7; i++)
                {
                    register[obj.Rconv(i)] = int16(i);
                }


                i = i__prev1;
            }
            {
                var i__prev1 = i;

                for (i = ppc64.REG_MSR; i <= ppc64.REG_CR; i++)
                {
                    register[obj.Rconv(i)] = int16(i);
                }


                i = i__prev1;
            }
            register["CR"] = ppc64.REG_CR;
            register["XER"] = ppc64.REG_XER;
            register["LR"] = ppc64.REG_LR;
            register["CTR"] = ppc64.REG_CTR;
            register["FPSCR"] = ppc64.REG_FPSCR;
            register["MSR"] = ppc64.REG_MSR; 
            // Pseudo-registers.
            register["SB"] = RSB;
            register["FP"] = RFP;
            register["PC"] = RPC; 
            // Avoid unintentionally clobbering g using R30.
            delete(register, "R30");
            register["g"] = ppc64.REG_R30;
            map registerPrefix = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, bool>{"CR":true,"F":true,"R":true,"SPR":true,};

            var instructions = make_map<@string, obj.As>();
            {
                var i__prev1 = i;
                var s__prev1 = s;

                foreach (var (__i, __s) in obj.Anames)
                {
                    i = __i;
                    s = __s;
                    instructions[s] = obj.As(i);
                }

                i = i__prev1;
                s = s__prev1;
            }

            {
                var i__prev1 = i;
                var s__prev1 = s;

                foreach (var (__i, __s) in ppc64.Anames)
                {
                    i = __i;
                    s = __s;
                    if (obj.As(i) >= obj.A_ARCHSPECIFIC)
                    {
                        instructions[s] = obj.As(i) + obj.ABasePPC64;
                    }
                } 
                // Annoying aliases.

                i = i__prev1;
                s = s__prev1;
            }

            instructions["BR"] = ppc64.ABR;
            instructions["BL"] = ppc64.ABL;

            return ref new Arch(LinkArch:&ppc64.Linkppc64,Instructions:instructions,Register:register,RegisterPrefix:registerPrefix,RegisterNumber:ppc64RegisterNumber,IsJump:jumpPPC64,);
        }

        private static ref Arch archMips()
        {
            var register = make_map<@string, short>(); 
            // Create maps for easy lookup of instruction names etc.
            // Note that there is no list of names as there is for x86.
            {
                var i__prev1 = i;

                for (var i = mips.REG_R0; i <= mips.REG_R31; i++)
                {
                    register[obj.Rconv(i)] = int16(i);
                }


                i = i__prev1;
            }

            {
                var i__prev1 = i;

                for (i = mips.REG_F0; i <= mips.REG_F31; i++)
                {
                    register[obj.Rconv(i)] = int16(i);
                }


                i = i__prev1;
            }
            {
                var i__prev1 = i;

                for (i = mips.REG_M0; i <= mips.REG_M31; i++)
                {
                    register[obj.Rconv(i)] = int16(i);
                }


                i = i__prev1;
            }
            {
                var i__prev1 = i;

                for (i = mips.REG_FCR0; i <= mips.REG_FCR31; i++)
                {
                    register[obj.Rconv(i)] = int16(i);
                }


                i = i__prev1;
            }
            register["HI"] = mips.REG_HI;
            register["LO"] = mips.REG_LO; 
            // Pseudo-registers.
            register["SB"] = RSB;
            register["FP"] = RFP;
            register["PC"] = RPC; 
            // Avoid unintentionally clobbering g using R30.
            delete(register, "R30");
            register["g"] = mips.REG_R30;

            map registerPrefix = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, bool>{"F":true,"FCR":true,"M":true,"R":true,};

            var instructions = make_map<@string, obj.As>();
            {
                var i__prev1 = i;
                var s__prev1 = s;

                foreach (var (__i, __s) in obj.Anames)
                {
                    i = __i;
                    s = __s;
                    instructions[s] = obj.As(i);
                }

                i = i__prev1;
                s = s__prev1;
            }

            {
                var i__prev1 = i;
                var s__prev1 = s;

                foreach (var (__i, __s) in mips.Anames)
                {
                    i = __i;
                    s = __s;
                    if (obj.As(i) >= obj.A_ARCHSPECIFIC)
                    {
                        instructions[s] = obj.As(i) + obj.ABaseMIPS;
                    }
                } 
                // Annoying alias.

                i = i__prev1;
                s = s__prev1;
            }

            instructions["JAL"] = mips.AJAL;

            return ref new Arch(LinkArch:&mips.Linkmipsle,Instructions:instructions,Register:register,RegisterPrefix:registerPrefix,RegisterNumber:mipsRegisterNumber,IsJump:jumpMIPS,);
        }

        private static ref Arch archMips64()
        {
            var register = make_map<@string, short>(); 
            // Create maps for easy lookup of instruction names etc.
            // Note that there is no list of names as there is for x86.
            {
                var i__prev1 = i;

                for (var i = mips.REG_R0; i <= mips.REG_R31; i++)
                {
                    register[obj.Rconv(i)] = int16(i);
                }


                i = i__prev1;
            }
            {
                var i__prev1 = i;

                for (i = mips.REG_F0; i <= mips.REG_F31; i++)
                {
                    register[obj.Rconv(i)] = int16(i);
                }


                i = i__prev1;
            }
            {
                var i__prev1 = i;

                for (i = mips.REG_M0; i <= mips.REG_M31; i++)
                {
                    register[obj.Rconv(i)] = int16(i);
                }


                i = i__prev1;
            }
            {
                var i__prev1 = i;

                for (i = mips.REG_FCR0; i <= mips.REG_FCR31; i++)
                {
                    register[obj.Rconv(i)] = int16(i);
                }


                i = i__prev1;
            }
            register["HI"] = mips.REG_HI;
            register["LO"] = mips.REG_LO; 
            // Pseudo-registers.
            register["SB"] = RSB;
            register["FP"] = RFP;
            register["PC"] = RPC; 
            // Avoid unintentionally clobbering g using R30.
            delete(register, "R30");
            register["g"] = mips.REG_R30; 
            // Avoid unintentionally clobbering RSB using R28.
            delete(register, "R28");
            register["RSB"] = mips.REG_R28;
            map registerPrefix = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, bool>{"F":true,"FCR":true,"M":true,"R":true,};

            var instructions = make_map<@string, obj.As>();
            {
                var i__prev1 = i;
                var s__prev1 = s;

                foreach (var (__i, __s) in obj.Anames)
                {
                    i = __i;
                    s = __s;
                    instructions[s] = obj.As(i);
                }

                i = i__prev1;
                s = s__prev1;
            }

            {
                var i__prev1 = i;
                var s__prev1 = s;

                foreach (var (__i, __s) in mips.Anames)
                {
                    i = __i;
                    s = __s;
                    if (obj.As(i) >= obj.A_ARCHSPECIFIC)
                    {
                        instructions[s] = obj.As(i) + obj.ABaseMIPS;
                    }
                } 
                // Annoying alias.

                i = i__prev1;
                s = s__prev1;
            }

            instructions["JAL"] = mips.AJAL;

            return ref new Arch(LinkArch:&mips.Linkmips64,Instructions:instructions,Register:register,RegisterPrefix:registerPrefix,RegisterNumber:mipsRegisterNumber,IsJump:jumpMIPS,);
        }

        private static ref Arch archS390x()
        {
            var register = make_map<@string, short>(); 
            // Create maps for easy lookup of instruction names etc.
            // Note that there is no list of names as there is for x86.
            {
                var i__prev1 = i;

                for (var i = s390x.REG_R0; i <= s390x.REG_R15; i++)
                {
                    register[obj.Rconv(i)] = int16(i);
                }


                i = i__prev1;
            }
            {
                var i__prev1 = i;

                for (i = s390x.REG_F0; i <= s390x.REG_F15; i++)
                {
                    register[obj.Rconv(i)] = int16(i);
                }


                i = i__prev1;
            }
            {
                var i__prev1 = i;

                for (i = s390x.REG_V0; i <= s390x.REG_V31; i++)
                {
                    register[obj.Rconv(i)] = int16(i);
                }


                i = i__prev1;
            }
            {
                var i__prev1 = i;

                for (i = s390x.REG_AR0; i <= s390x.REG_AR15; i++)
                {
                    register[obj.Rconv(i)] = int16(i);
                }


                i = i__prev1;
            }
            register["LR"] = s390x.REG_LR; 
            // Pseudo-registers.
            register["SB"] = RSB;
            register["FP"] = RFP;
            register["PC"] = RPC; 
            // Avoid unintentionally clobbering g using R13.
            delete(register, "R13");
            register["g"] = s390x.REG_R13;
            map registerPrefix = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, bool>{"AR":true,"F":true,"R":true,};

            var instructions = make_map<@string, obj.As>();
            {
                var i__prev1 = i;
                var s__prev1 = s;

                foreach (var (__i, __s) in obj.Anames)
                {
                    i = __i;
                    s = __s;
                    instructions[s] = obj.As(i);
                }

                i = i__prev1;
                s = s__prev1;
            }

            {
                var i__prev1 = i;
                var s__prev1 = s;

                foreach (var (__i, __s) in s390x.Anames)
                {
                    i = __i;
                    s = __s;
                    if (obj.As(i) >= obj.A_ARCHSPECIFIC)
                    {
                        instructions[s] = obj.As(i) + obj.ABaseS390X;
                    }
                } 
                // Annoying aliases.

                i = i__prev1;
                s = s__prev1;
            }

            instructions["BR"] = s390x.ABR;
            instructions["BL"] = s390x.ABL;

            return ref new Arch(LinkArch:&s390x.Links390x,Instructions:instructions,Register:register,RegisterPrefix:registerPrefix,RegisterNumber:s390xRegisterNumber,IsJump:jumpS390x,);
        }
    }
}}}}
