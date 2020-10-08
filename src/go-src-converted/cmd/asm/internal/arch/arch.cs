// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package arch defines architecture-specific information and support functions.
// package arch -- go2cs converted at 2020 October 08 04:04:30 UTC
// import "cmd/asm/internal/arch" ==> using arch = go.cmd.asm.@internal.arch_package
// Original source: C:\Go\src\cmd\asm\internal\arch\arch.go
using obj = go.cmd.@internal.obj_package;
using arm = go.cmd.@internal.obj.arm_package;
using arm64 = go.cmd.@internal.obj.arm64_package;
using mips = go.cmd.@internal.obj.mips_package;
using ppc64 = go.cmd.@internal.obj.ppc64_package;
using riscv = go.cmd.@internal.obj.riscv_package;
using s390x = go.cmd.@internal.obj.s390x_package;
using wasm = go.cmd.@internal.obj.wasm_package;
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
        public static readonly var RFP = (var)-(iota + 1L);
        public static readonly var RSB = (var)0;
        public static readonly var RSP = (var)1;
        public static readonly var RPC = (var)2;


        // Arch wraps the link architecture object with more architecture-specific information.
        public partial struct Arch
        {
            public ref ptr<obj.LinkArch> LinkArch> => ref LinkArch>_ptr; // Map of instruction names to enumeration.
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
            short _p0 = default;
            bool _p0 = default;

            return (0L, false);
        }

        // Set configures the architecture specified by GOARCH and returns its representation.
        // It returns nil if GOARCH is not recognized.
        public static ptr<Arch> Set(@string GOARCH)
        {
            switch (GOARCH)
            {
                case "386": 
                    return _addr_archX86(_addr_x86.Link386)!;
                    break;
                case "amd64": 
                    return _addr_archX86(_addr_x86.Linkamd64)!;
                    break;
                case "arm": 
                    return _addr_archArm()!;
                    break;
                case "arm64": 
                    return _addr_archArm64()!;
                    break;
                case "mips": 
                    return _addr_archMips(_addr_mips.Linkmips)!;
                    break;
                case "mipsle": 
                    return _addr_archMips(_addr_mips.Linkmipsle)!;
                    break;
                case "mips64": 
                    return _addr_archMips64(_addr_mips.Linkmips64)!;
                    break;
                case "mips64le": 
                    return _addr_archMips64(_addr_mips.Linkmips64le)!;
                    break;
                case "ppc64": 
                    return _addr_archPPC64(_addr_ppc64.Linkppc64)!;
                    break;
                case "ppc64le": 
                    return _addr_archPPC64(_addr_ppc64.Linkppc64le)!;
                    break;
                case "riscv64": 
                    return _addr_archRISCV64()!;
                    break;
                case "s390x": 
                    return _addr_archS390x()!;
                    break;
                case "wasm": 
                    return _addr_archWasm()!;
                    break;
            }
            return _addr_null!;

        }

        private static bool jumpX86(@string word)
        {
            return word[0L] == 'J' || word == "CALL" || strings.HasPrefix(word, "LOOP") || word == "XBEGIN";
        }

        private static bool jumpRISCV(@string word)
        {
            switch (word)
            {
                case "BEQ": 

                case "BEQZ": 

                case "BGE": 

                case "BGEU": 

                case "BGEZ": 

                case "BGT": 

                case "BGTU": 

                case "BGTZ": 

                case "BLE": 

                case "BLEU": 

                case "BLEZ": 

                case "BLT": 

                case "BLTU": 

                case "BLTZ": 

                case "BNE": 

                case "BNEZ": 

                case "CALL": 

                case "JAL": 

                case "JALR": 

                case "JMP": 
                    return true;
                    break;
            }
            return false;

        }

        private static bool jumpWasm(@string word)
        {
            return word == "JMP" || word == "CALL" || word == "Call" || word == "Br" || word == "BrIf";
        }

        private static ptr<Arch> archX86(ptr<obj.LinkArch> _addr_linkArch)
        {
            ref obj.LinkArch linkArch = ref _addr_linkArch.val;

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

            return addr(new Arch(LinkArch:linkArch,Instructions:instructions,Register:register,RegisterPrefix:nil,RegisterNumber:nilRegisterNumber,IsJump:jumpX86,));

        }

        private static ptr<Arch> archArm()
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

            // special operands for DMB/DSB instructions
            register["MB_SY"] = arm.REG_MB_SY;
            register["MB_ST"] = arm.REG_MB_ST;
            register["MB_ISH"] = arm.REG_MB_ISH;
            register["MB_ISHST"] = arm.REG_MB_ISHST;
            register["MB_NSH"] = arm.REG_MB_NSH;
            register["MB_NSHST"] = arm.REG_MB_NSHST;
            register["MB_OSH"] = arm.REG_MB_OSH;
            register["MB_OSHST"] = arm.REG_MB_OSHST;

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

            return addr(new Arch(LinkArch:&arm.Linkarm,Instructions:instructions,Register:register,RegisterPrefix:registerPrefix,RegisterNumber:armRegisterNumber,IsJump:jumpArm,));

        }

        private static ptr<Arch> archArm64()
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
                // Rename R18 to R18_PLATFORM to avoid accidental use.


                i = i__prev1;
            } 
            // Rename R18 to R18_PLATFORM to avoid accidental use.
            register["R18_PLATFORM"] = register["R18"];
            delete(register, "R18");
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

                // System registers.


                i = i__prev1;
            } 

            // System registers.
            {
                var i__prev1 = i;

                for (i = 0L; i < len(arm64.SystemReg); i++)
                {
                    register[arm64.SystemReg[i].Name] = arm64.SystemReg[i].Reg;
                }


                i = i__prev1;
            }

            register["LR"] = arm64.REGLINK;
            register["DAIFSet"] = arm64.REG_DAIFSet;
            register["DAIFClr"] = arm64.REG_DAIFClr;
            register["PLDL1KEEP"] = arm64.REG_PLDL1KEEP;
            register["PLDL1STRM"] = arm64.REG_PLDL1STRM;
            register["PLDL2KEEP"] = arm64.REG_PLDL2KEEP;
            register["PLDL2STRM"] = arm64.REG_PLDL2STRM;
            register["PLDL3KEEP"] = arm64.REG_PLDL3KEEP;
            register["PLDL3STRM"] = arm64.REG_PLDL3STRM;
            register["PLIL1KEEP"] = arm64.REG_PLIL1KEEP;
            register["PLIL1STRM"] = arm64.REG_PLIL1STRM;
            register["PLIL2KEEP"] = arm64.REG_PLIL2KEEP;
            register["PLIL2STRM"] = arm64.REG_PLIL2STRM;
            register["PLIL3KEEP"] = arm64.REG_PLIL3KEEP;
            register["PLIL3STRM"] = arm64.REG_PLIL3STRM;
            register["PSTL1KEEP"] = arm64.REG_PSTL1KEEP;
            register["PSTL1STRM"] = arm64.REG_PSTL1STRM;
            register["PSTL2KEEP"] = arm64.REG_PSTL2KEEP;
            register["PSTL2STRM"] = arm64.REG_PSTL2STRM;
            register["PSTL3KEEP"] = arm64.REG_PSTL3KEEP;
            register["PSTL3STRM"] = arm64.REG_PSTL3STRM; 

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

            return addr(new Arch(LinkArch:&arm64.Linkarm64,Instructions:instructions,Register:register,RegisterPrefix:registerPrefix,RegisterNumber:arm64RegisterNumber,IsJump:jumpArm64,));


        }

        private static ptr<Arch> archPPC64(ptr<obj.LinkArch> _addr_linkArch)
        {
            ref obj.LinkArch linkArch = ref _addr_linkArch.val;

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

            return addr(new Arch(LinkArch:linkArch,Instructions:instructions,Register:register,RegisterPrefix:registerPrefix,RegisterNumber:ppc64RegisterNumber,IsJump:jumpPPC64,));

        }

        private static ptr<Arch> archMips(ptr<obj.LinkArch> _addr_linkArch)
        {
            ref obj.LinkArch linkArch = ref _addr_linkArch.val;

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

            return addr(new Arch(LinkArch:linkArch,Instructions:instructions,Register:register,RegisterPrefix:registerPrefix,RegisterNumber:mipsRegisterNumber,IsJump:jumpMIPS,));

        }

        private static ptr<Arch> archMips64(ptr<obj.LinkArch> _addr_linkArch)
        {
            ref obj.LinkArch linkArch = ref _addr_linkArch.val;

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
            {
                var i__prev1 = i;

                for (i = mips.REG_W0; i <= mips.REG_W31; i++)
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
            map registerPrefix = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, bool>{"F":true,"FCR":true,"M":true,"R":true,"W":true,};

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

            return addr(new Arch(LinkArch:linkArch,Instructions:instructions,Register:register,RegisterPrefix:registerPrefix,RegisterNumber:mipsRegisterNumber,IsJump:jumpMIPS,));

        }

        private static ptr<Arch> archRISCV64()
        {
            var register = make_map<@string, short>(); 

            // Standard register names.
            {
                var i__prev1 = i;

                for (var i = riscv.REG_X0; i <= riscv.REG_X31; i++)
                {
                    var name = fmt.Sprintf("X%d", i - riscv.REG_X0);
                    register[name] = int16(i);
                }


                i = i__prev1;
            }
            {
                var i__prev1 = i;

                for (i = riscv.REG_F0; i <= riscv.REG_F31; i++)
                {
                    name = fmt.Sprintf("F%d", i - riscv.REG_F0);
                    register[name] = int16(i);
                } 

                // General registers with ABI names.


                i = i__prev1;
            } 

            // General registers with ABI names.
            register["ZERO"] = riscv.REG_ZERO;
            register["RA"] = riscv.REG_RA;
            register["SP"] = riscv.REG_SP;
            register["GP"] = riscv.REG_GP;
            register["TP"] = riscv.REG_TP;
            register["T0"] = riscv.REG_T0;
            register["T1"] = riscv.REG_T1;
            register["T2"] = riscv.REG_T2;
            register["S0"] = riscv.REG_S0;
            register["S1"] = riscv.REG_S1;
            register["A0"] = riscv.REG_A0;
            register["A1"] = riscv.REG_A1;
            register["A2"] = riscv.REG_A2;
            register["A3"] = riscv.REG_A3;
            register["A4"] = riscv.REG_A4;
            register["A5"] = riscv.REG_A5;
            register["A6"] = riscv.REG_A6;
            register["A7"] = riscv.REG_A7;
            register["S2"] = riscv.REG_S2;
            register["S3"] = riscv.REG_S3;
            register["S4"] = riscv.REG_S4;
            register["S5"] = riscv.REG_S5;
            register["S6"] = riscv.REG_S6;
            register["S7"] = riscv.REG_S7;
            register["S8"] = riscv.REG_S8;
            register["S9"] = riscv.REG_S9;
            register["S10"] = riscv.REG_S10;
            register["S11"] = riscv.REG_S11;
            register["T3"] = riscv.REG_T3;
            register["T4"] = riscv.REG_T4;
            register["T5"] = riscv.REG_T5;
            register["T6"] = riscv.REG_T6; 

            // Go runtime register names.
            register["g"] = riscv.REG_G;
            register["CTXT"] = riscv.REG_CTXT;
            register["TMP"] = riscv.REG_TMP; 

            // ABI names for floating point register.
            register["FT0"] = riscv.REG_FT0;
            register["FT1"] = riscv.REG_FT1;
            register["FT2"] = riscv.REG_FT2;
            register["FT3"] = riscv.REG_FT3;
            register["FT4"] = riscv.REG_FT4;
            register["FT5"] = riscv.REG_FT5;
            register["FT6"] = riscv.REG_FT6;
            register["FT7"] = riscv.REG_FT7;
            register["FS0"] = riscv.REG_FS0;
            register["FS1"] = riscv.REG_FS1;
            register["FA0"] = riscv.REG_FA0;
            register["FA1"] = riscv.REG_FA1;
            register["FA2"] = riscv.REG_FA2;
            register["FA3"] = riscv.REG_FA3;
            register["FA4"] = riscv.REG_FA4;
            register["FA5"] = riscv.REG_FA5;
            register["FA6"] = riscv.REG_FA6;
            register["FA7"] = riscv.REG_FA7;
            register["FS2"] = riscv.REG_FS2;
            register["FS3"] = riscv.REG_FS3;
            register["FS4"] = riscv.REG_FS4;
            register["FS5"] = riscv.REG_FS5;
            register["FS6"] = riscv.REG_FS6;
            register["FS7"] = riscv.REG_FS7;
            register["FS8"] = riscv.REG_FS8;
            register["FS9"] = riscv.REG_FS9;
            register["FS10"] = riscv.REG_FS10;
            register["FS11"] = riscv.REG_FS11;
            register["FT8"] = riscv.REG_FT8;
            register["FT9"] = riscv.REG_FT9;
            register["FT10"] = riscv.REG_FT10;
            register["FT11"] = riscv.REG_FT11; 

            // Pseudo-registers.
            register["SB"] = RSB;
            register["FP"] = RFP;
            register["PC"] = RPC;

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

                foreach (var (__i, __s) in riscv.Anames)
                {
                    i = __i;
                    s = __s;
                    if (obj.As(i) >= obj.A_ARCHSPECIFIC)
                    {
                        instructions[s] = obj.As(i) + obj.ABaseRISCV;
                    }

                }

                i = i__prev1;
                s = s__prev1;
            }

            return addr(new Arch(LinkArch:&riscv.LinkRISCV64,Instructions:instructions,Register:register,RegisterPrefix:nil,RegisterNumber:nilRegisterNumber,IsJump:jumpRISCV,));

        }

        private static ptr<Arch> archS390x()
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

            return addr(new Arch(LinkArch:&s390x.Links390x,Instructions:instructions,Register:register,RegisterPrefix:registerPrefix,RegisterNumber:s390xRegisterNumber,IsJump:jumpS390x,));

        }

        private static ptr<Arch> archWasm()
        {
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

                foreach (var (__i, __s) in wasm.Anames)
                {
                    i = __i;
                    s = __s;
                    if (obj.As(i) >= obj.A_ARCHSPECIFIC)
                    {
                        instructions[s] = obj.As(i) + obj.ABaseWasm;
                    }

                }

                i = i__prev1;
                s = s__prev1;
            }

            return addr(new Arch(LinkArch:&wasm.Linkwasm,Instructions:instructions,Register:wasm.Register,RegisterPrefix:nil,RegisterNumber:nilRegisterNumber,IsJump:jumpWasm,));

        }
    }
}}}}
