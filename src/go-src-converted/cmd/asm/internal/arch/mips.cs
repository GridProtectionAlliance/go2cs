// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file encapsulates some of the odd characteristics of the
// MIPS (MIPS64) instruction set, to minimize its interaction
// with the core of the assembler.

// package arch -- go2cs converted at 2020 October 08 04:08:20 UTC
// import "cmd/asm/internal/arch" ==> using arch = go.cmd.asm.@internal.arch_package
// Original source: C:\Go\src\cmd\asm\internal\arch\mips.go
using obj = go.cmd.@internal.obj_package;
using mips = go.cmd.@internal.obj.mips_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace asm {
namespace @internal
{
    public static partial class arch_package
    {
        private static bool jumpMIPS(@string word)
        {
            switch (word)
            {
                case "BEQ": 

                case "BFPF": 

                case "BFPT": 

                case "BGEZ": 

                case "BGEZAL": 

                case "BGTZ": 

                case "BLEZ": 

                case "BLTZ": 

                case "BLTZAL": 

                case "BNE": 

                case "JMP": 

                case "JAL": 

                case "CALL": 
                    return true;
                    break;
            }
            return false;

        }

        // IsMIPSCMP reports whether the op (as defined by an mips.A* constant) is
        // one of the CMP instructions that require special handling.
        public static bool IsMIPSCMP(obj.As op)
        {

            if (op == mips.ACMPEQF || op == mips.ACMPEQD || op == mips.ACMPGEF || op == mips.ACMPGED || op == mips.ACMPGTF || op == mips.ACMPGTD) 
                return true;
                        return false;

        }

        // IsMIPSMUL reports whether the op (as defined by an mips.A* constant) is
        // one of the MUL/DIV/REM/MADD/MSUB instructions that require special handling.
        public static bool IsMIPSMUL(obj.As op)
        {

            if (op == mips.AMUL || op == mips.AMULU || op == mips.AMULV || op == mips.AMULVU || op == mips.ADIV || op == mips.ADIVU || op == mips.ADIVV || op == mips.ADIVVU || op == mips.AREM || op == mips.AREMU || op == mips.AREMV || op == mips.AREMVU || op == mips.AMADD || op == mips.AMSUB) 
                return true;
                        return false;

        }

        private static (short, bool) mipsRegisterNumber(@string name, short n)
        {
            short _p0 = default;
            bool _p0 = default;

            switch (name)
            {
                case "F": 
                    if (0L <= n && n <= 31L)
                    {
                        return (mips.REG_F0 + n, true);
                    }

                    break;
                case "FCR": 
                    if (0L <= n && n <= 31L)
                    {
                        return (mips.REG_FCR0 + n, true);
                    }

                    break;
                case "M": 
                    if (0L <= n && n <= 31L)
                    {
                        return (mips.REG_M0 + n, true);
                    }

                    break;
                case "R": 
                    if (0L <= n && n <= 31L)
                    {
                        return (mips.REG_R0 + n, true);
                    }

                    break;
                case "W": 
                    if (0L <= n && n <= 31L)
                    {
                        return (mips.REG_W0 + n, true);
                    }

                    break;
            }
            return (0L, false);

        }
    }
}}}}
