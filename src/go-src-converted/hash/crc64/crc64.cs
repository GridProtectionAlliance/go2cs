// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package crc64 implements the 64-bit cyclic redundancy check, or CRC-64,
// checksum. See http://en.wikipedia.org/wiki/Cyclic_redundancy_check for
// information.
// package crc64 -- go2cs converted at 2020 August 29 08:23:19 UTC
// import "hash/crc64" ==> using crc64 = go.hash.crc64_package
// Original source: C:\Go\src\hash\crc64\crc64.go
using errors = go.errors_package;
using hash = go.hash_package;
using static go.builtin;

namespace go {
namespace hash
{
    public static partial class crc64_package
    {
        // The size of a CRC-64 checksum in bytes.
        public static readonly long Size = 8L;

        // Predefined polynomials.


        // Predefined polynomials.
 
        // The ISO polynomial, defined in ISO 3309 and used in HDLC.
        public static readonly ulong ISO = 0xD800000000000000UL; 

        // The ECMA polynomial, defined in ECMA 182.
        public static readonly ulong ECMA = 0xC96C5795D7870F42UL;

        // Table is a 256-word table representing the polynomial for efficient processing.
        public partial struct Table // : array<ulong>
        {
        }

        private static var slicing8TableISO = makeSlicingBy8Table(makeTable(ISO));        private static var slicing8TableECMA = makeSlicingBy8Table(makeTable(ECMA));

        // MakeTable returns a Table constructed from the specified polynomial.
        // The contents of this Table must not be modified.
        public static ref Table MakeTable(ulong poly)
        {

            if (poly == ISO) 
                return ref slicing8TableISO[0L];
            else if (poly == ECMA) 
                return ref slicing8TableECMA[0L];
            else 
                return makeTable(poly);
                    }

        private static ref Table makeTable(ulong poly)
        {
            ptr<Table> t = @new<Table>();
            for (long i = 0L; i < 256L; i++)
            {
                var crc = uint64(i);
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

            return t;
        }

        private static ref array<Table> makeSlicingBy8Table(ref Table t)
        {
            array<Table> helperTable = new array<Table>(8L);
            helperTable[0L] = t.Value;
            for (long i = 0L; i < 256L; i++)
            {
                var crc = t[i];
                for (long j = 1L; j < 8L; j++)
                {
                    crc = t[crc & 0xffUL] ^ (crc >> (int)(8L));
                    helperTable[j][i] = crc;
                }

            }

            return ref helperTable;
        }

        // digest represents the partial evaluation of a checksum.
        private partial struct digest
        {
            public ulong crc;
            public ptr<Table> tab;
        }

        // New creates a new hash.Hash64 computing the CRC-64 checksum using the
        // polynomial represented by the Table. Its Sum method will lay the
        // value out in big-endian byte order. The returned Hash64 also
        // implements encoding.BinaryMarshaler and encoding.BinaryUnmarshaler to
        // marshal and unmarshal the internal state of the hash.
        public static hash.Hash64 New(ref Table tab)
        {
            return ref new digest(0,tab);
        }

        private static long Size(this ref digest d)
        {
            return Size;
        }

        private static long BlockSize(this ref digest d)
        {
            return 1L;
        }

        private static void Reset(this ref digest d)
        {
            d.crc = 0L;

        }

        private static readonly @string magic = "crc\x02";
        private static readonly var marshaledSize = len(magic) + 8L + 8L;

        private static (slice<byte>, error) MarshalBinary(this ref digest d)
        {
            var b = make_slice<byte>(0L, marshaledSize);
            b = append(b, magic);
            b = appendUint64(b, tableSum(d.tab));
            b = appendUint64(b, d.crc);
            return (b, null);
        }

        private static error UnmarshalBinary(this ref digest d, slice<byte> b)
        {
            if (len(b) < len(magic) || string(b[..len(magic)]) != magic)
            {
                return error.As(errors.New("hash/crc64: invalid hash state identifier"));
            }
            if (len(b) != marshaledSize)
            {
                return error.As(errors.New("hash/crc64: invalid hash state size"));
            }
            if (tableSum(d.tab) != readUint64(b[4L..]))
            {
                return error.As(errors.New("hash/crc64: tables do not match"));
            }
            d.crc = readUint64(b[12L..]);
            return error.As(null);
        }

        private static slice<byte> appendUint64(slice<byte> b, ulong x)
        {
            array<byte> a = new array<byte>(new byte[] { byte(x>>56), byte(x>>48), byte(x>>40), byte(x>>32), byte(x>>24), byte(x>>16), byte(x>>8), byte(x) });
            return append(b, a[..]);
        }

        private static ulong readUint64(slice<byte> b)
        {
            _ = b[7L];
            return uint64(b[7L]) | uint64(b[6L]) << (int)(8L) | uint64(b[5L]) << (int)(16L) | uint64(b[4L]) << (int)(24L) | uint64(b[3L]) << (int)(32L) | uint64(b[2L]) << (int)(40L) | uint64(b[1L]) << (int)(48L) | uint64(b[0L]) << (int)(56L);
        }

        private static ulong update(ulong crc, ref Table tab, slice<byte> p)
        {
            crc = ~crc; 
            // Table comparison is somewhat expensive, so avoid it for small sizes
            while (len(p) >= 64L)
            {
                ref array<Table> helperTable = default;
                if (tab == slicing8TableECMA[0L].Value)
                {
                    helperTable = slicing8TableECMA;
                }
                else if (tab == slicing8TableISO[0L].Value)
                {
                    helperTable = slicing8TableISO; 
                    // For smaller sizes creating extended table takes too much time
                }
                else if (len(p) > 16384L)
                {
                    helperTable = makeSlicingBy8Table(tab);
                }
                else
                {
                    break;
                } 
                // Update using slicing-by-8
                while (len(p) > 8L)
                {
                    crc ^= uint64(p[0L]) | uint64(p[1L]) << (int)(8L) | uint64(p[2L]) << (int)(16L) | uint64(p[3L]) << (int)(24L) | uint64(p[4L]) << (int)(32L) | uint64(p[5L]) << (int)(40L) | uint64(p[6L]) << (int)(48L) | uint64(p[7L]) << (int)(56L);
                    crc = helperTable[7L][crc & 0xffUL] ^ helperTable[6L][(crc >> (int)(8L)) & 0xffUL] ^ helperTable[5L][(crc >> (int)(16L)) & 0xffUL] ^ helperTable[4L][(crc >> (int)(24L)) & 0xffUL] ^ helperTable[3L][(crc >> (int)(32L)) & 0xffUL] ^ helperTable[2L][(crc >> (int)(40L)) & 0xffUL] ^ helperTable[1L][(crc >> (int)(48L)) & 0xffUL] ^ helperTable[0L][crc >> (int)(56L)];
                    p = p[8L..];
                }

            } 
            // For reminders or small sizes
 
            // For reminders or small sizes
            foreach (var (_, v) in p)
            {
                crc = tab[byte(crc) ^ v] ^ (crc >> (int)(8L));
            }
            return ~crc;
        }

        // Update returns the result of adding the bytes in p to the crc.
        public static ulong Update(ulong crc, ref Table tab, slice<byte> p)
        {
            return update(crc, tab, p);
        }

        private static (long, error) Write(this ref digest d, slice<byte> p)
        {
            d.crc = update(d.crc, d.tab, p);
            return (len(p), null);
        }

        private static ulong Sum64(this ref digest d)
        {
            return d.crc;
        }

        private static slice<byte> Sum(this ref digest d, slice<byte> @in)
        {
            var s = d.Sum64();
            return append(in, byte(s >> (int)(56L)), byte(s >> (int)(48L)), byte(s >> (int)(40L)), byte(s >> (int)(32L)), byte(s >> (int)(24L)), byte(s >> (int)(16L)), byte(s >> (int)(8L)), byte(s));
        }

        // Checksum returns the CRC-64 checksum of data
        // using the polynomial represented by the Table.
        public static ulong Checksum(slice<byte> data, ref Table tab)
        {
            return update(0L, tab, data);
        }

        // tableSum returns the ISO checksum of table t.
        private static ulong tableSum(ref Table t)
        {
            array<byte> a = new array<byte>(2048L);
            var b = a[..0L];
            if (t != null)
            {
                foreach (var (_, x) in t)
                {
                    b = appendUint64(b, x);
                }
            }
            return Checksum(b, MakeTable(ISO));
        }
    }
}}
