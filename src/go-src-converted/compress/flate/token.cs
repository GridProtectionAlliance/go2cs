// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package flate -- go2cs converted at 2020 August 29 08:23:36 UTC
// import "compress/flate" ==> using flate = go.compress.flate_package
// Original source: C:\Go\src\compress\flate\token.go

using static go.builtin;

namespace go {
namespace compress
{
    public static partial class flate_package
    {
 
        // 2 bits:   type   0 = literal  1=EOF  2=Match   3=Unused
        // 8 bits:   xlength = length - MIN_MATCH_LENGTH
        // 22 bits   xoffset = offset - MIN_OFFSET_SIZE, or literal
        private static readonly long lengthShift = 22L;
        private static readonly long offsetMask = 1L << (int)(lengthShift) - 1L;
        private static readonly long typeMask = 3L << (int)(30L);
        private static readonly long literalType = 0L << (int)(30L);
        private static readonly long matchType = 1L << (int)(30L);

        // The length code for length X (MIN_MATCH_LENGTH <= X <= MAX_MATCH_LENGTH)
        // is lengthCodes[length - MIN_MATCH_LENGTH]
        private static array<uint> lengthCodes = new array<uint>(new uint[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 8, 9, 9, 10, 10, 11, 11, 12, 12, 12, 12, 13, 13, 13, 13, 14, 14, 14, 14, 15, 15, 15, 15, 16, 16, 16, 16, 16, 16, 16, 16, 17, 17, 17, 17, 17, 17, 17, 17, 18, 18, 18, 18, 18, 18, 18, 18, 19, 19, 19, 19, 19, 19, 19, 19, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 23, 23, 23, 23, 23, 23, 23, 23, 23, 23, 23, 23, 23, 23, 23, 23, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 28 });

        private static array<uint> offsetCodes = new array<uint>(new uint[] { 0, 1, 2, 3, 4, 4, 5, 5, 6, 6, 6, 6, 7, 7, 7, 7, 8, 8, 8, 8, 8, 8, 8, 8, 9, 9, 9, 9, 9, 9, 9, 9, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15 });

        private partial struct token // : uint
        {
        }

        // Convert a literal into a literal token.
        private static token literalToken(uint literal)
        {
            return token(literalType + literal);
        }

        // Convert a < xlength, xoffset > pair into a match token.
        private static token matchToken(uint xlength, uint xoffset)
        {
            return token(matchType + xlength << (int)(lengthShift) + xoffset);
        }

        // Returns the literal of a literal token
        private static uint literal(this token t)
        {
            return uint32(t - literalType);
        }

        // Returns the extra offset of a match token
        private static uint offset(this token t)
        {
            return uint32(t) & offsetMask;
        }

        private static uint length(this token t)
        {
            return uint32((t - matchType) >> (int)(lengthShift));
        }

        private static uint lengthCode(uint len)
        {
            return lengthCodes[len];
        }

        // Returns the offset code corresponding to a specific offset
        private static uint offsetCode(uint off)
        {
            if (off < uint32(len(offsetCodes)))
            {
                return offsetCodes[off];
            }
            if (off >> (int)(7L) < uint32(len(offsetCodes)))
            {
                return offsetCodes[off >> (int)(7L)] + 14L;
            }
            return offsetCodes[off >> (int)(14L)] + 28L;
        }
    }
}}
