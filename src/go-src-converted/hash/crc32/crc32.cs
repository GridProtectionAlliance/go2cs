// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package crc32 implements the 32-bit cyclic redundancy check, or CRC-32,
// checksum. See http://en.wikipedia.org/wiki/Cyclic_redundancy_check for
// information.
//
// Polynomials are represented in LSB-first form also known as reversed representation.
//
// See http://en.wikipedia.org/wiki/Mathematics_of_cyclic_redundancy_checks#Reversed_representations_and_reciprocal_polynomials
// for information.
// package crc32 -- go2cs converted at 2020 August 29 08:23:12 UTC
// import "hash/crc32" ==> using crc32 = go.hash.crc32_package
// Original source: C:\Go\src\hash\crc32\crc32.go
using errors = go.errors_package;
using hash = go.hash_package;
using sync = go.sync_package;
using static go.builtin;
using System;

namespace go {
namespace hash
{
    public static partial class crc32_package
    {
        // The size of a CRC-32 checksum in bytes.
        public static readonly long Size = 4L;

        // Predefined polynomials.


        // Predefined polynomials.
 
        // IEEE is by far and away the most common CRC-32 polynomial.
        // Used by ethernet (IEEE 802.3), v.42, fddi, gzip, zip, png, ...
        public static readonly ulong IEEE = 0xedb88320UL; 

        // Castagnoli's polynomial, used in iSCSI.
        // Has better error detection characteristics than IEEE.
        // http://dx.doi.org/10.1109/26.231911
        public static readonly ulong Castagnoli = 0x82f63b78UL; 

        // Koopman's polynomial.
        // Also has better error detection characteristics than IEEE.
        // http://dx.doi.org/10.1109/DSN.2002.1028931
        public static readonly ulong Koopman = 0xeb31d82eUL;

        // Table is a 256-word table representing the polynomial for efficient processing.
        public partial struct Table // : array<uint>
        {
        }

        // This file makes use of functions implemented in architecture-specific files.
        // The interface that they implement is as follows:
        //
        //    // archAvailableIEEE reports whether an architecture-specific CRC32-IEEE
        //    // algorithm is available.
        //    archAvailableIEEE() bool
        //
        //    // archInitIEEE initializes the architecture-specific CRC3-IEEE algorithm.
        //    // It can only be called if archAvailableIEEE() returns true.
        //    archInitIEEE()
        //
        //    // archUpdateIEEE updates the given CRC32-IEEE. It can only be called if
        //    // archInitIEEE() was previously called.
        //    archUpdateIEEE(crc uint32, p []byte) uint32
        //
        //    // archAvailableCastagnoli reports whether an architecture-specific
        //    // CRC32-C algorithm is available.
        //    archAvailableCastagnoli() bool
        //
        //    // archInitCastagnoli initializes the architecture-specific CRC32-C
        //    // algorithm. It can only be called if archAvailableCastagnoli() returns
        //    // true.
        //    archInitCastagnoli()
        //
        //    // archUpdateCastagnoli updates the given CRC32-C. It can only be called
        //    // if archInitCastagnoli() was previously called.
        //    archUpdateCastagnoli(crc uint32, p []byte) uint32

        // castagnoliTable points to a lazily initialized Table for the Castagnoli
        // polynomial. MakeTable will always return this value when asked to make a
        // Castagnoli table so we can compare against it to find when the caller is
        // using this polynomial.
        private static ref Table castagnoliTable = default;
        private static ref slicing8Table castagnoliTable8 = default;
        private static bool castagnoliArchImpl = default;
        private static Func<uint, slice<byte>, uint> updateCastagnoli = default;
        private static sync.Once castagnoliOnce = default;

        private static void castagnoliInit()
        {
            castagnoliTable = simpleMakeTable(Castagnoli);
            castagnoliArchImpl = archAvailableCastagnoli();

            if (castagnoliArchImpl)
            {
                archInitCastagnoli();
                updateCastagnoli = archUpdateCastagnoli;
            }
            else
            { 
                // Initialize the slicing-by-8 table.
                castagnoliTable8 = slicingMakeTable(Castagnoli);
                updateCastagnoli = (crc, p) =>
                {
                    return slicingUpdate(crc, castagnoliTable8, p);
                }
;
            }
        }

        // IEEETable is the table for the IEEE polynomial.
        public static var IEEETable = simpleMakeTable(IEEE);

        // ieeeTable8 is the slicing8Table for IEEE
        private static ref slicing8Table ieeeTable8 = default;
        private static bool ieeeArchImpl = default;
        private static Func<uint, slice<byte>, uint> updateIEEE = default;
        private static sync.Once ieeeOnce = default;

        private static void ieeeInit()
        {
            ieeeArchImpl = archAvailableIEEE();

            if (ieeeArchImpl)
            {
                archInitIEEE();
                updateIEEE = archUpdateIEEE;
            }
            else
            { 
                // Initialize the slicing-by-8 table.
                ieeeTable8 = slicingMakeTable(IEEE);
                updateIEEE = (crc, p) =>
                {
                    return slicingUpdate(crc, ieeeTable8, p);
                }
;
            }
        }

        // MakeTable returns a Table constructed from the specified polynomial.
        // The contents of this Table must not be modified.
        public static ref Table MakeTable(uint poly)
        {

            if (poly == IEEE) 
                ieeeOnce.Do(ieeeInit);
                return IEEETable;
            else if (poly == Castagnoli) 
                castagnoliOnce.Do(castagnoliInit);
                return castagnoliTable;
                        return simpleMakeTable(poly);
        }

        // digest represents the partial evaluation of a checksum.
        private partial struct digest
        {
            public uint crc;
            public ptr<Table> tab;
        }

        // New creates a new hash.Hash32 computing the CRC-32 checksum using the
        // polynomial represented by the Table. Its Sum method will lay the
        // value out in big-endian byte order. The returned Hash32 also
        // implements encoding.BinaryMarshaler and encoding.BinaryUnmarshaler to
        // marshal and unmarshal the internal state of the hash.
        public static hash.Hash32 New(ref Table tab)
        {
            if (tab == IEEETable)
            {
                ieeeOnce.Do(ieeeInit);
            }
            return ref new digest(0,tab);
        }

        // NewIEEE creates a new hash.Hash32 computing the CRC-32 checksum using
        // the IEEE polynomial. Its Sum method will lay the value out in
        // big-endian byte order. The returned Hash32 also implements
        // encoding.BinaryMarshaler and encoding.BinaryUnmarshaler to marshal
        // and unmarshal the internal state of the hash.
        public static hash.Hash32 NewIEEE()
        {
            return New(IEEETable);
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

        private static readonly @string magic = "crc\x01";
        private static readonly var marshaledSize = len(magic) + 4L + 4L;

        private static (slice<byte>, error) MarshalBinary(this ref digest d)
        {
            var b = make_slice<byte>(0L, marshaledSize);
            b = append(b, magic);
            b = appendUint32(b, tableSum(d.tab));
            b = appendUint32(b, d.crc);
            return (b, null);
        }

        private static error UnmarshalBinary(this ref digest d, slice<byte> b)
        {
            if (len(b) < len(magic) || string(b[..len(magic)]) != magic)
            {
                return error.As(errors.New("hash/crc32: invalid hash state identifier"));
            }
            if (len(b) != marshaledSize)
            {
                return error.As(errors.New("hash/crc32: invalid hash state size"));
            }
            if (tableSum(d.tab) != readUint32(b[4L..]))
            {
                return error.As(errors.New("hash/crc32: tables do not match"));
            }
            d.crc = readUint32(b[8L..]);
            return error.As(null);
        }

        private static slice<byte> appendUint32(slice<byte> b, uint x)
        {
            array<byte> a = new array<byte>(new byte[] { byte(x>>24), byte(x>>16), byte(x>>8), byte(x) });
            return append(b, a[..]);
        }

        private static uint readUint32(slice<byte> b)
        {
            _ = b[3L];
            return uint32(b[3L]) | uint32(b[2L]) << (int)(8L) | uint32(b[1L]) << (int)(16L) | uint32(b[0L]) << (int)(24L);
        }

        // Update returns the result of adding the bytes in p to the crc.
        public static uint Update(uint crc, ref Table tab, slice<byte> p)
        {

            if (tab == castagnoliTable) 
                return updateCastagnoli(crc, p);
            else if (tab == IEEETable) 
                // Unfortunately, because IEEETable is exported, IEEE may be used without a
                // call to MakeTable. We have to make sure it gets initialized in that case.
                ieeeOnce.Do(ieeeInit);
                return updateIEEE(crc, p);
            else 
                return simpleUpdate(crc, tab, p);
                    }

        private static (long, error) Write(this ref digest d, slice<byte> p)
        {

            if (d.tab == castagnoliTable) 
                d.crc = updateCastagnoli(d.crc, p);
            else if (d.tab == IEEETable) 
                // We only create digest objects through New() which takes care of
                // initialization in this case.
                d.crc = updateIEEE(d.crc, p);
            else 
                d.crc = simpleUpdate(d.crc, d.tab, p);
                        return (len(p), null);
        }

        private static uint Sum32(this ref digest d)
        {
            return d.crc;
        }

        private static slice<byte> Sum(this ref digest d, slice<byte> @in)
        {
            var s = d.Sum32();
            return append(in, byte(s >> (int)(24L)), byte(s >> (int)(16L)), byte(s >> (int)(8L)), byte(s));
        }

        // Checksum returns the CRC-32 checksum of data
        // using the polynomial represented by the Table.
        public static uint Checksum(slice<byte> data, ref Table tab)
        {
            return Update(0L, tab, data);
        }

        // ChecksumIEEE returns the CRC-32 checksum of data
        // using the IEEE polynomial.
        public static uint ChecksumIEEE(slice<byte> data)
        {
            ieeeOnce.Do(ieeeInit);
            return updateIEEE(0L, data);
        }

        // tableSum returns the IEEE checksum of table t.
        private static uint tableSum(ref Table t)
        {
            array<byte> a = new array<byte>(1024L);
            var b = a[..0L];
            if (t != null)
            {
                foreach (var (_, x) in t)
                {
                    b = appendUint32(b, x);
                }
            }
            return ChecksumIEEE(b);
        }
    }
}}
