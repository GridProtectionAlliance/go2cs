// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file encapsulates some of the odd characteristics of the
// 64-bit PowerPC (PPC64) instruction set, to minimize its interaction
// with the core of the assembler.

// package arch -- go2cs converted at 2020 October 08 04:08:21 UTC
// import "cmd/asm/internal/arch" ==> using arch = go.cmd.asm.@internal.arch_package
// Original source: C:\Go\src\cmd\asm\internal\arch\ppc64.go
using obj = go.cmd.@internal.obj_package;
using ppc64 = go.cmd.@internal.obj.ppc64_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace asm {
namespace @internal
{
    public static partial class arch_package
    {
        private static bool jumpPPC64(@string word)
        {
            switch (word)
            {
                case "BC": 

                case "BCL": 

                case "BEQ": 

                case "BGE": 

                case "BGT": 

                case "BL": 

                case "BLE": 

                case "BLT": 

                case "BNE": 

                case "BR": 

                case "BVC": 

                case "BVS": 

                case "CALL": 

                case "JMP": 
                    return true;
                    break;
            }
            return false;

        }

        // IsPPC64RLD reports whether the op (as defined by an ppc64.A* constant) is
        // one of the RLD-like instructions that require special handling.
        // The FMADD-like instructions behave similarly.
        public static bool IsPPC64RLD(obj.As op)
        {

            if (op == ppc64.ARLDC || op == ppc64.ARLDCCC || op == ppc64.ARLDCL || op == ppc64.ARLDCLCC || op == ppc64.ARLDCR || op == ppc64.ARLDCRCC || op == ppc64.ARLDMI || op == ppc64.ARLDMICC || op == ppc64.ARLWMI || op == ppc64.ARLWMICC || op == ppc64.ARLWNM || op == ppc64.ARLWNMCC) 
                return true;
            else if (op == ppc64.AFMADD || op == ppc64.AFMADDCC || op == ppc64.AFMADDS || op == ppc64.AFMADDSCC || op == ppc64.AFMSUB || op == ppc64.AFMSUBCC || op == ppc64.AFMSUBS || op == ppc64.AFMSUBSCC || op == ppc64.AFNMADD || op == ppc64.AFNMADDCC || op == ppc64.AFNMADDS || op == ppc64.AFNMADDSCC || op == ppc64.AFNMSUB || op == ppc64.AFNMSUBCC || op == ppc64.AFNMSUBS || op == ppc64.AFNMSUBSCC) 
                return true;
                        return false;

        }

        public static bool IsPPC64ISEL(obj.As op)
        {
            return op == ppc64.AISEL;
        }

        // IsPPC64CMP reports whether the op (as defined by an ppc64.A* constant) is
        // one of the CMP instructions that require special handling.
        public static bool IsPPC64CMP(obj.As op)
        {

            if (op == ppc64.ACMP || op == ppc64.ACMPU || op == ppc64.ACMPW || op == ppc64.ACMPWU || op == ppc64.AFCMPU) 
                return true;
                        return false;

        }

        // IsPPC64NEG reports whether the op (as defined by an ppc64.A* constant) is
        // one of the NEG-like instructions that require special handling.
        public static bool IsPPC64NEG(obj.As op)
        {

            if (op == ppc64.AADDMECC || op == ppc64.AADDMEVCC || op == ppc64.AADDMEV || op == ppc64.AADDME || op == ppc64.AADDZECC || op == ppc64.AADDZEVCC || op == ppc64.AADDZEV || op == ppc64.AADDZE || op == ppc64.ACNTLZDCC || op == ppc64.ACNTLZD || op == ppc64.ACNTLZWCC || op == ppc64.ACNTLZW || op == ppc64.AEXTSBCC || op == ppc64.AEXTSB || op == ppc64.AEXTSHCC || op == ppc64.AEXTSH || op == ppc64.AEXTSWCC || op == ppc64.AEXTSW || op == ppc64.ANEGCC || op == ppc64.ANEGVCC || op == ppc64.ANEGV || op == ppc64.ANEG || op == ppc64.ASLBMFEE || op == ppc64.ASLBMFEV || op == ppc64.ASLBMTE || op == ppc64.ASUBMECC || op == ppc64.ASUBMEVCC || op == ppc64.ASUBMEV || op == ppc64.ASUBME || op == ppc64.ASUBZECC || op == ppc64.ASUBZEVCC || op == ppc64.ASUBZEV || op == ppc64.ASUBZE) 
                return true;
                        return false;

        }

        private static (short, bool) ppc64RegisterNumber(@string name, short n)
        {
            short _p0 = default;
            bool _p0 = default;

            switch (name)
            {
                case "CR": 
                    if (0L <= n && n <= 7L)
                    {
                        return (ppc64.REG_CR0 + n, true);
                    }

                    break;
                case "VS": 
                    if (0L <= n && n <= 63L)
                    {
                        return (ppc64.REG_VS0 + n, true);
                    }

                    break;
                case "V": 
                    if (0L <= n && n <= 31L)
                    {
                        return (ppc64.REG_V0 + n, true);
                    }

                    break;
                case "F": 
                    if (0L <= n && n <= 31L)
                    {
                        return (ppc64.REG_F0 + n, true);
                    }

                    break;
                case "R": 
                    if (0L <= n && n <= 31L)
                    {
                        return (ppc64.REG_R0 + n, true);
                    }

                    break;
                case "SPR": 
                    if (0L <= n && n <= 1024L)
                    {
                        return (ppc64.REG_SPR0 + n, true);
                    }

                    break;
            }
            return (0L, false);

        }
    }
}}}}
