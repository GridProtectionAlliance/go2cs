// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package arm64asm -- go2cs converted at 2020 August 29 10:07:20 UTC
// import "cmd/vendor/golang.org/x/arch/arm64/arm64asm" ==> using arm64asm = go.cmd.vendor.golang.org.x.arch.arm64.arm64asm_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\arch\arm64\arm64asm\condition_util.go

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
        private static uint extract_bit(uint value, uint bit)
        {
            return (value >> (int)(bit)) & 1L;
        }

        private static bool bfxpreferred_4(uint sf, uint opc1, uint imms, uint immr)
        {
            if (imms < immr)
            {
                return false;
            }
            if ((imms >> (int)(5L) == sf) && (imms & 0x1fUL == 0x1fUL))
            {
                return false;
            }
            if (immr == 0L)
            {
                if (sf == 0L && (imms == 7L || imms == 15L))
                {
                    return false;
                }
                if (sf == 1L && opc1 == 0L && (imms == 7L || imms == 15L || imms == 31L))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool move_wide_preferred_4(uint sf, uint N, uint imms, uint immr)
        {
            if (sf == 1L && N != 1L)
            {
                return false;
            }
            if (sf == 0L && !(N == 0L && ((imms >> (int)(5L)) & 1L) == 0L))
            {
                return false;
            }
            if (imms < 16L)
            {
                return (-immr) % 16L <= (15L - imms);
            }
            var width = uint32(32L);
            if (sf == 1L)
            {
                width = uint32(64L);
            }
            if (imms >= (width - 15L))
            {
                return (immr % 16L) <= (imms - (width - 15L));
            }
            return false;
        }

        public partial struct Sys // : byte
        {
        }

        public static readonly Sys Sys_AT = iota;
        public static readonly var Sys_DC = 0;
        public static readonly var Sys_IC = 1;
        public static readonly var Sys_TLBI = 2;
        public static readonly var Sys_SYS = 3;

        private static Sys sys_op_4(uint op1, uint crn, uint crm, uint op2)
        { 
            // TODO: system instruction
            return Sys_SYS;
        }

        private static bool is_zero(uint x)
        {
            return x == 0L;
        }

        private static bool is_ones_n16(uint x)
        {
            return x == 0xffffUL;
        }

        private static byte bit_count(uint x)
        {
            byte count = default;
            count = 0L;

            while (x > 0L)
            {
                if ((x & 1L) == 1L)
                {
                    count++;
                x >>= 1L;
                }
            }

            return count;
        }
    }
}}}}}}}
