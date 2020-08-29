// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file encapsulates some of the odd characteristics of the ARM64
// instruction set, to minimize its interaction with the core of the
// assembler.

// package arch -- go2cs converted at 2020 August 29 08:51:55 UTC
// import "cmd/asm/internal/arch" ==> using arch = go.cmd.asm.@internal.arch_package
// Original source: C:\Go\src\cmd\asm\internal\arch\arm64.go
using obj = go.cmd.@internal.obj_package;
using arm64 = go.cmd.@internal.obj.arm64_package;
using errors = go.errors_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace asm {
namespace @internal
{
    public static partial class arch_package
    {
        private static map arm64LS = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, byte>{"P":arm64.C_XPOST,"W":arm64.C_XPRE,};

        private static map arm64Jump = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, bool>{"B":true,"BL":true,"BEQ":true,"BNE":true,"BCS":true,"BHS":true,"BCC":true,"BLO":true,"BMI":true,"BPL":true,"BVS":true,"BVC":true,"BHI":true,"BLS":true,"BGE":true,"BLT":true,"BGT":true,"BLE":true,"CALL":true,"CBZ":true,"CBZW":true,"CBNZ":true,"CBNZW":true,"JMP":true,"TBNZ":true,"TBZ":true,};

        private static bool jumpArm64(@string word)
        {
            return arm64Jump[word];
        }

        // IsARM64CMP reports whether the op (as defined by an arm.A* constant) is
        // one of the comparison instructions that require special handling.
        public static bool IsARM64CMP(obj.As op)
        {

            if (op == arm64.ACMN || op == arm64.ACMP || op == arm64.ATST || op == arm64.ACMNW || op == arm64.ACMPW || op == arm64.ATSTW || op == arm64.AFCMPS || op == arm64.AFCMPD || op == arm64.AFCMPES || op == arm64.AFCMPED) 
                return true;
                        return false;
        }

        // IsARM64STLXR reports whether the op (as defined by an arm64.A*
        // constant) is one of the STLXR-like instructions that require special
        // handling.
        public static bool IsARM64STLXR(obj.As op)
        {

            if (op == arm64.ASTLXRB || op == arm64.ASTLXRH || op == arm64.ASTLXRW || op == arm64.ASTLXR || op == arm64.ASTXRB || op == arm64.ASTXRH || op == arm64.ASTXRW || op == arm64.ASTXR) 
                return true;
                        return false;
        }

        // ARM64Suffix handles the special suffix for the ARM64.
        // It returns a boolean to indicate success; failure means
        // cond was unrecognized.
        public static bool ARM64Suffix(ref obj.Prog prog, @string cond)
        {
            if (cond == "")
            {
                return true;
            }
            var (bits, ok) = ParseARM64Suffix(cond);
            if (!ok)
            {
                return false;
            }
            prog.Scond = bits;
            return true;
        }

        // ParseARM64Suffix parses the suffix attached to an ARM64 instruction.
        // The input is a single string consisting of period-separated condition
        // codes, such as ".P.W". An initial period is ignored.
        public static (byte, bool) ParseARM64Suffix(@string cond)
        {
            if (cond == "")
            {
                return (0L, true);
            }
            return parseARMCondition(cond, arm64LS, null);
        }

        private static (short, bool) arm64RegisterNumber(@string name, short n)
        {
            switch (name)
            {
                case "F": 
                    if (0L <= n && n <= 31L)
                    {
                        return (arm64.REG_F0 + n, true);
                    }
                    break;
                case "R": 
                    if (0L <= n && n <= 30L)
                    { // not 31
                        return (arm64.REG_R0 + n, true);
                    }
                    break;
                case "V": 
                    if (0L <= n && n <= 31L)
                    {
                        return (arm64.REG_V0 + n, true);
                    }
                    break;
            }
            return (0L, false);
        }

        // ARM64RegisterExtension parses an ARM64 register with extension or arrangment.
        public static error ARM64RegisterExtension(ref obj.Addr a, @string ext, short reg, short num, bool isAmount, bool isIndex)
        {
            var rm = uint32(reg);
            if (isAmount)
            {
                if (num < 0L || num > 7L)
                {
                    return error.As(errors.New("shift amount out of range"));
                }
            }
            switch (ext)
            {
                case "UXTB": 
                    if (!isAmount)
                    {
                        return error.As(errors.New("invalid register extension"));
                    }
                    a.Reg = arm64.REG_UXTB + (reg & 31L) + int16(num << (int)(5L));
                    a.Offset = int64(((rm & 31L) << (int)(16L)) | (uint32(num) << (int)(10L)));
                    break;
                case "UXTH": 
                    if (!isAmount)
                    {
                        return error.As(errors.New("invalid register extension"));
                    }
                    a.Reg = arm64.REG_UXTH + (reg & 31L) + int16(num << (int)(5L));
                    a.Offset = int64(((rm & 31L) << (int)(16L)) | (1L << (int)(13L)) | (uint32(num) << (int)(10L)));
                    break;
                case "UXTW": 
                    if (!isAmount)
                    {
                        return error.As(errors.New("invalid register extension"));
                    }
                    a.Reg = arm64.REG_UXTW + (reg & 31L) + int16(num << (int)(5L));
                    a.Offset = int64(((rm & 31L) << (int)(16L)) | (2L << (int)(13L)) | (uint32(num) << (int)(10L)));
                    break;
                case "UXTX": 
                    if (!isAmount)
                    {
                        return error.As(errors.New("invalid register extension"));
                    }
                    a.Reg = arm64.REG_UXTX + (reg & 31L) + int16(num << (int)(5L));
                    a.Offset = int64(((rm & 31L) << (int)(16L)) | (3L << (int)(13L)) | (uint32(num) << (int)(10L)));
                    break;
                case "SXTB": 
                    if (!isAmount)
                    {
                        return error.As(errors.New("invalid register extension"));
                    }
                    a.Reg = arm64.REG_SXTB + (reg & 31L) + int16(num << (int)(5L));
                    a.Offset = int64(((rm & 31L) << (int)(16L)) | (4L << (int)(13L)) | (uint32(num) << (int)(10L)));
                    break;
                case "SXTH": 
                    if (!isAmount)
                    {
                        return error.As(errors.New("invalid register extension"));
                    }
                    a.Reg = arm64.REG_SXTH + (reg & 31L) + int16(num << (int)(5L));
                    a.Offset = int64(((rm & 31L) << (int)(16L)) | (5L << (int)(13L)) | (uint32(num) << (int)(10L)));
                    break;
                case "SXTW": 
                    if (!isAmount)
                    {
                        return error.As(errors.New("invalid register extension"));
                    }
                    a.Reg = arm64.REG_SXTW + (reg & 31L) + int16(num << (int)(5L));
                    a.Offset = int64(((rm & 31L) << (int)(16L)) | (6L << (int)(13L)) | (uint32(num) << (int)(10L)));
                    break;
                case "SXTX": 
                    if (!isAmount)
                    {
                        return error.As(errors.New("invalid register extension"));
                    }
                    a.Reg = arm64.REG_SXTX + (reg & 31L) + int16(num << (int)(5L));
                    a.Offset = int64(((rm & 31L) << (int)(16L)) | (7L << (int)(13L)) | (uint32(num) << (int)(10L)));
                    break;
                case "B8": 
                    a.Reg = arm64.REG_ARNG + (reg & 31L) + ((arm64.ARNG_8B & 15L) << (int)(5L));
                    break;
                case "B16": 
                    a.Reg = arm64.REG_ARNG + (reg & 31L) + ((arm64.ARNG_16B & 15L) << (int)(5L));
                    break;
                case "H4": 
                    a.Reg = arm64.REG_ARNG + (reg & 31L) + ((arm64.ARNG_4H & 15L) << (int)(5L));
                    break;
                case "H8": 
                    a.Reg = arm64.REG_ARNG + (reg & 31L) + ((arm64.ARNG_8H & 15L) << (int)(5L));
                    break;
                case "S2": 
                    a.Reg = arm64.REG_ARNG + (reg & 31L) + ((arm64.ARNG_2S & 15L) << (int)(5L));
                    break;
                case "S4": 
                    a.Reg = arm64.REG_ARNG + (reg & 31L) + ((arm64.ARNG_4S & 15L) << (int)(5L));
                    break;
                case "D2": 
                    a.Reg = arm64.REG_ARNG + (reg & 31L) + ((arm64.ARNG_2D & 15L) << (int)(5L));
                    break;
                case "B": 
                    if (!isIndex)
                    {
                        return error.As(null);
                    }
                    a.Reg = arm64.REG_ELEM + (reg & 31L) + ((arm64.ARNG_B & 15L) << (int)(5L));
                    a.Index = num;
                    break;
                case "H": 
                    if (!isIndex)
                    {
                        return error.As(null);
                    }
                    a.Reg = arm64.REG_ELEM + (reg & 31L) + ((arm64.ARNG_H & 15L) << (int)(5L));
                    a.Index = num;
                    break;
                case "S": 
                    if (!isIndex)
                    {
                        return error.As(null);
                    }
                    a.Reg = arm64.REG_ELEM + (reg & 31L) + ((arm64.ARNG_S & 15L) << (int)(5L));
                    a.Index = num;
                    break;
                case "D": 
                    if (!isIndex)
                    {
                        return error.As(null);
                    }
                    a.Reg = arm64.REG_ELEM + (reg & 31L) + ((arm64.ARNG_D & 15L) << (int)(5L));
                    a.Index = num;
                    break;
                default: 
                    return error.As(errors.New("unsupported register extension type: " + ext));
                    break;
            }
            a.Type = obj.TYPE_REG;
            return error.As(null);
        }

        // ARM64RegisterArrangement parses an ARM64 vector register arrangment.
        public static (long, error) ARM64RegisterArrangement(short reg, @string name, @string arng)
        {
            ushort curQ = default;            ushort curSize = default;

            if (name[0L] != 'V')
            {
                return (0L, errors.New("expect V0 through V31; found: " + name));
            }
            if (reg < 0L)
            {
                return (0L, errors.New("invalid register number: " + name));
            }
            switch (arng)
            {
                case "B8": 
                    curSize = 0L;
                    curQ = 0L;
                    break;
                case "B16": 
                    curSize = 0L;
                    curQ = 1L;
                    break;
                case "H4": 
                    curSize = 1L;
                    curQ = 0L;
                    break;
                case "H8": 
                    curSize = 1L;
                    curQ = 1L;
                    break;
                case "S2": 
                    curSize = 1L;
                    curQ = 0L;
                    break;
                case "S4": 
                    curSize = 2L;
                    curQ = 1L;
                    break;
                case "D1": 
                    curSize = 3L;
                    curQ = 0L;
                    break;
                case "D2": 
                    curSize = 3L;
                    curQ = 1L;
                    break;
                default: 
                    return (0L, errors.New("invalid arrangement in ARM64 register list"));
                    break;
            }
            return ((int64(curQ) & 1L << (int)(30L)) | (int64(curSize & 3L) << (int)(10L)), null);
        }

        // ARM64RegisterListOffset generates offset encoding according to AArch64 specification.
        public static (long, error) ARM64RegisterListOffset(long firstReg, long regCnt, long arrangement)
        {
            var offset = int64(firstReg);
            switch (regCnt)
            {
                case 1L: 
                    offset |= 0x7UL << (int)(12L);
                    break;
                case 2L: 
                    offset |= 0xaUL << (int)(12L);
                    break;
                case 3L: 
                    offset |= 0x6UL << (int)(12L);
                    break;
                case 4L: 
                    offset |= 0x2UL << (int)(12L);
                    break;
                default: 
                    return (0L, errors.New("invalid register numbers in ARM64 register list"));
                    break;
            }
            offset |= arrangement; 
            // arm64 uses the 60th bit to differentiate from other archs
            // For more details, refer to: obj/arm64/list7.go
            offset |= 1L << (int)(60L);
            return (offset, null);
        }
    }
}}}}
