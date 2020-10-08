// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// ARM64-specific hardware-assisted CRC32 algorithms. See crc32.go for a
// description of the interface that each architecture-specific file
// implements.

// package crc32 -- go2cs converted at 2020 October 08 03:30:49 UTC
// import "hash/crc32" ==> using crc32 = go.hash.crc32_package
// Original source: C:\Go\src\hash\crc32\crc32_arm64.go
using cpu = go.@internal.cpu_package;
using static go.builtin;

namespace go {
namespace hash
{
    public static partial class crc32_package
    {
        private static uint castagnoliUpdate(uint crc, slice<byte> p)
;
        private static uint ieeeUpdate(uint crc, slice<byte> p)
;

        private static bool archAvailableCastagnoli()
        {
            return cpu.ARM64.HasCRC32;
        }

        private static void archInitCastagnoli() => func((_, panic, __) =>
        {
            if (!cpu.ARM64.HasCRC32)
            {>>MARKER:FUNCTION_ieeeUpdate_BLOCK_PREFIX<<
                panic("arch-specific crc32 instruction for Catagnoli not available");
            }

        });

        private static uint archUpdateCastagnoli(uint crc, slice<byte> p) => func((_, panic, __) =>
        {
            if (!cpu.ARM64.HasCRC32)
            {>>MARKER:FUNCTION_castagnoliUpdate_BLOCK_PREFIX<<
                panic("arch-specific crc32 instruction for Castagnoli not available");
            }

            return ~castagnoliUpdate(~crc, p);

        });

        private static bool archAvailableIEEE()
        {
            return cpu.ARM64.HasCRC32;
        }

        private static void archInitIEEE() => func((_, panic, __) =>
        {
            if (!cpu.ARM64.HasCRC32)
            {
                panic("arch-specific crc32 instruction for IEEE not available");
            }

        });

        private static uint archUpdateIEEE(uint crc, slice<byte> p) => func((_, panic, __) =>
        {
            if (!cpu.ARM64.HasCRC32)
            {
                panic("arch-specific crc32 instruction for IEEE not available");
            }

            return ~ieeeUpdate(~crc, p);

        });
    }
}}
