// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package crc32 -- go2cs converted at 2020 August 29 08:23:15 UTC
// import "hash/crc32" ==> using crc32 = go.hash.crc32_package
// Original source: C:\Go\src\hash\crc32\crc32_amd64p32.go
using cpu = go.@internal.cpu_package;
using static go.builtin;

namespace go {
namespace hash
{
    public static partial class crc32_package
    {
        // This file contains the code to call the SSE 4.2 version of the Castagnoli
        // CRC.

        // castagnoliSSE42 is defined in crc32_amd64p32.s and uses the SSE4.2 CRC32
        // instruction.
        //go:noescape
        private static uint castagnoliSSE42(uint crc, slice<byte> p)
;

        private static bool archAvailableCastagnoli()
        {
            return cpu.X86.HasSSE42;
        }

        private static void archInitCastagnoli() => func((_, panic, __) =>
        {
            if (!cpu.X86.HasSSE42)
            {>>MARKER:FUNCTION_castagnoliSSE42_BLOCK_PREFIX<<
                panic("not available");
            } 
            // No initialization necessary.
        });

        private static uint archUpdateCastagnoli(uint crc, slice<byte> p) => func((_, panic, __) =>
        {
            if (!cpu.X86.HasSSE42)
            {
                panic("not available");
            }
            return castagnoliSSE42(crc, p);
        });

        private static bool archAvailableIEEE()
        {
            return false;
        }
        private static void archInitIEEE() => func((_, panic, __) =>
        {
            panic("not available");

        });
        private static uint archUpdateIEEE(uint crc, slice<byte> p) => func((_, panic, __) =>
        {
            panic("not available");

        });
    }
}}
