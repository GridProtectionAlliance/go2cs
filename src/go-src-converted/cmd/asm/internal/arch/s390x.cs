// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file encapsulates some of the odd characteristics of the
// s390x instruction set, to minimize its interaction
// with the core of the assembler.

// package arch -- go2cs converted at 2020 August 29 08:51:55 UTC
// import "cmd/asm/internal/arch" ==> using arch = go.cmd.asm.@internal.arch_package
// Original source: C:\Go\src\cmd\asm\internal\arch\s390x.go
using obj = go.cmd.@internal.obj_package;
using s390x = go.cmd.@internal.obj.s390x_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace asm {
namespace @internal
{
    public static partial class arch_package
    {
        private static bool jumpS390x(@string word)
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

                case "BLEU": 

                case "BLT": 

                case "BLTU": 

                case "BNE": 

                case "BR": 

                case "BVC": 

                case "BVS": 

                case "CMPBEQ": 

                case "CMPBGE": 

                case "CMPBGT": 

                case "CMPBLE": 

                case "CMPBLT": 

                case "CMPBNE": 

                case "CMPUBEQ": 

                case "CMPUBGE": 

                case "CMPUBGT": 

                case "CMPUBLE": 

                case "CMPUBLT": 

                case "CMPUBNE": 

                case "CALL": 

                case "JMP": 
                    return true;
                    break;
            }
            return false;
        }

        // IsS390xCMP reports whether the op (as defined by an s390x.A* constant) is
        // one of the CMP instructions that require special handling.
        public static bool IsS390xCMP(obj.As op)
        {

            if (op == s390x.ACMP || op == s390x.ACMPU || op == s390x.ACMPW || op == s390x.ACMPWU) 
                return true;
                        return false;
        }

        // IsS390xNEG reports whether the op (as defined by an s390x.A* constant) is
        // one of the NEG-like instructions that require special handling.
        public static bool IsS390xNEG(obj.As op)
        {

            if (op == s390x.ANEG || op == s390x.ANEGW) 
                return true;
                        return false;
        }

        private static (short, bool) s390xRegisterNumber(@string name, short n)
        {
            switch (name)
            {
                case "AR": 
                    if (0L <= n && n <= 15L)
                    {
                        return (s390x.REG_AR0 + n, true);
                    }
                    break;
                case "F": 
                    if (0L <= n && n <= 15L)
                    {
                        return (s390x.REG_F0 + n, true);
                    }
                    break;
                case "R": 
                    if (0L <= n && n <= 15L)
                    {
                        return (s390x.REG_R0 + n, true);
                    }
                    break;
                case "V": 
                    if (0L <= n && n <= 31L)
                    {
                        return (s390x.REG_V0 + n, true);
                    }
                    break;
            }
            return (0L, false);
        }
    }
}}}}
