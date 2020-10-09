// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package crc32 -- go2cs converted at 2020 October 09 04:50:08 UTC
// import "hash/crc32" ==> using crc32 = go.hash.crc32_package
// Original source: C:\Go\src\hash\crc32\crc32_s390x.go
using cpu = go.@internal.cpu_package;
using static go.builtin;

namespace go {
namespace hash
{
    public static partial class crc32_package
    {
        private static readonly long vxMinLen = (long)64L;
        private static readonly long vxAlignMask = (long)15L; // align to 16 bytes

        // hasVX reports whether the machine has the z/Architecture
        // vector facility installed and enabled.
        private static var hasVX = cpu.S390X.HasVX;

        // vectorizedCastagnoli implements CRC32 using vector instructions.
        // It is defined in crc32_s390x.s.
        //go:noescape
        private static uint vectorizedCastagnoli(uint crc, slice<byte> p)
;

        // vectorizedIEEE implements CRC32 using vector instructions.
        // It is defined in crc32_s390x.s.
        //go:noescape
        private static uint vectorizedIEEE(uint crc, slice<byte> p)
;

        private static bool archAvailableCastagnoli()
        {
            return hasVX;
        }

        private static ptr<slicing8Table> archCastagnoliTable8;

        private static void archInitCastagnoli() => func((_, panic, __) =>
        {
            if (!hasVX)
            {>>MARKER:FUNCTION_vectorizedIEEE_BLOCK_PREFIX<<
                panic("not available");
            } 
            // We still use slicing-by-8 for small buffers.
            archCastagnoliTable8 = slicingMakeTable(Castagnoli);

        });

        // archUpdateCastagnoli calculates the checksum of p using
        // vectorizedCastagnoli.
        private static uint archUpdateCastagnoli(uint crc, slice<byte> p) => func((_, panic, __) =>
        {
            if (!hasVX)
            {>>MARKER:FUNCTION_vectorizedCastagnoli_BLOCK_PREFIX<<
                panic("not available");
            } 
            // Use vectorized function if data length is above threshold.
            if (len(p) >= vxMinLen)
            {
                var aligned = len(p) & ~vxAlignMask;
                crc = vectorizedCastagnoli(crc, p[..aligned]);
                p = p[aligned..];
            }

            if (len(p) == 0L)
            {
                return crc;
            }

            return slicingUpdate(crc, archCastagnoliTable8, p);

        });

        private static bool archAvailableIEEE()
        {
            return hasVX;
        }

        private static ptr<slicing8Table> archIeeeTable8;

        private static void archInitIEEE() => func((_, panic, __) =>
        {
            if (!hasVX)
            {
                panic("not available");
            } 
            // We still use slicing-by-8 for small buffers.
            archIeeeTable8 = slicingMakeTable(IEEE);

        });

        // archUpdateIEEE calculates the checksum of p using vectorizedIEEE.
        private static uint archUpdateIEEE(uint crc, slice<byte> p) => func((_, panic, __) =>
        {
            if (!hasVX)
            {
                panic("not available");
            } 
            // Use vectorized function if data length is above threshold.
            if (len(p) >= vxMinLen)
            {
                var aligned = len(p) & ~vxAlignMask;
                crc = vectorizedIEEE(crc, p[..aligned]);
                p = p[aligned..];
            }

            if (len(p) == 0L)
            {
                return crc;
            }

            return slicingUpdate(crc, archIeeeTable8, p);

        });
    }
}}
