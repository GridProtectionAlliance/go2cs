// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file contains CRC32 algorithms that are not specific to any architecture
// and don't use hardware acceleration.
//
// The simple (and slow) CRC32 implementation only uses a 256*4 bytes table.
//
// The slicing-by-8 algorithm is a faster implementation that uses a bigger
// table (8*256*4 bytes).

// package crc32 -- go2cs converted at 2020 August 29 08:23:16 UTC
// import "hash/crc32" ==> using crc32 = go.hash.crc32_package
// Original source: C:\Go\src\hash\crc32\crc32_generic.go

using static go.builtin;

namespace go {
namespace hash
{
    public static partial class crc32_package
    {
        // simpleMakeTable allocates and constructs a Table for the specified
        // polynomial. The table is suitable for use with the simple algorithm
        // (simpleUpdate).
        private static ref Table simpleMakeTable(uint poly)
        {
            ptr<Table> t = @new<Table>();
            simplePopulateTable(poly, t);
            return t;
        }

        // simplePopulateTable constructs a Table for the specified polynomial, suitable
        // for use with simpleUpdate.
        private static void simplePopulateTable(uint poly, ref Table t)
        {
            for (long i = 0L; i < 256L; i++)
            {
                var crc = uint32(i);
                for (long j = 0L; j < 8L; j++)
                {
                    if (crc & 1L == 1L)
                    {
                        crc = (crc >> (int)(1L)) ^ poly;
                    }
                    else
                    {
                        crc >>= 1L;
                    }
                }

                t[i] = crc;
            }

        }

        // simpleUpdate uses the simple algorithm to update the CRC, given a table that
        // was previously computed using simpleMakeTable.
        private static uint simpleUpdate(uint crc, ref Table tab, slice<byte> p)
        {
            crc = ~crc;
            foreach (var (_, v) in p)
            {
                crc = tab[byte(crc) ^ v] ^ (crc >> (int)(8L));
            }
            return ~crc;
        }

        // Use slicing-by-8 when payload >= this value.
        private static readonly long slicing8Cutoff = 16L;

        // slicing8Table is array of 8 Tables, used by the slicing-by-8 algorithm.


        // slicing8Table is array of 8 Tables, used by the slicing-by-8 algorithm.
        private partial struct slicing8Table // : array<Table>
        {
        }

        // slicingMakeTable constructs a slicing8Table for the specified polynomial. The
        // table is suitable for use with the slicing-by-8 algorithm (slicingUpdate).
        private static ref slicing8Table slicingMakeTable(uint poly)
        {
            ptr<slicing8Table> t = @new<slicing8Table>();
            simplePopulateTable(poly, ref t[0L]);
            for (long i = 0L; i < 256L; i++)
            {
                var crc = t[0L][i];
                for (long j = 1L; j < 8L; j++)
                {
                    crc = t[0L][crc & 0xFFUL] ^ (crc >> (int)(8L));
                    t[j][i] = crc;
                }

            }

            return t;
        }

        // slicingUpdate uses the slicing-by-8 algorithm to update the CRC, given a
        // table that was previously computed using slicingMakeTable.
        private static uint slicingUpdate(uint crc, ref slicing8Table tab, slice<byte> p)
        {
            if (len(p) >= slicing8Cutoff)
            {
                crc = ~crc;
                while (len(p) > 8L)
                {
                    crc ^= uint32(p[0L]) | uint32(p[1L]) << (int)(8L) | uint32(p[2L]) << (int)(16L) | uint32(p[3L]) << (int)(24L);
                    crc = tab[0L][p[7L]] ^ tab[1L][p[6L]] ^ tab[2L][p[5L]] ^ tab[3L][p[4L]] ^ tab[4L][crc >> (int)(24L)] ^ tab[5L][(crc >> (int)(16L)) & 0xFFUL] ^ tab[6L][(crc >> (int)(8L)) & 0xFFUL] ^ tab[7L][crc & 0xFFUL];
                    p = p[8L..];
                }

                crc = ~crc;
            }
            if (len(p) == 0L)
            {
                return crc;
            }
            return simpleUpdate(crc, ref tab[0L], p);
        }
    }
}}
