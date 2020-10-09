// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package binary -- go2cs converted at 2020 October 09 04:49:48 UTC
// import "encoding/binary" ==> using binary = go.encoding.binary_package
// Original source: C:\Go\src\encoding\binary\varint.go
// This file implements "varint" encoding of 64-bit integers.
// The encoding is:
// - unsigned integers are serialized 7 bits at a time, starting with the
//   least significant bits
// - the most significant bit (msb) in each output byte indicates if there
//   is a continuation byte (msb = 1)
// - signed integers are mapped to unsigned integers using "zig-zag"
//   encoding: Positive values x are written as 2*x + 0, negative values
//   are written as 2*(^x) + 1; that is, negative numbers are complemented
//   and whether to complement is encoded in bit 0.
//
// Design note:
// At most 10 bytes are needed for 64-bit values. The encoding could
// be more dense: a full 64-bit value needs an extra byte just to hold bit 63.
// Instead, the msb of the previous byte could be used to hold bit 63 since we
// know there can't be more than 64 bits. This is a trivial improvement and
// would reduce the maximum encoding length to 9 bytes. However, it breaks the
// invariant that the msb is always the "continuation bit" and thus makes the
// format incompatible with a varint encoding for larger numbers (say 128-bit).

using errors = go.errors_package;
using io = go.io_package;
using static go.builtin;

namespace go {
namespace encoding
{
    public static partial class binary_package
    {
        // MaxVarintLenN is the maximum length of a varint-encoded N-bit integer.
        public static readonly long MaxVarintLen16 = (long)3L;
        public static readonly long MaxVarintLen32 = (long)5L;
        public static readonly long MaxVarintLen64 = (long)10L;


        // PutUvarint encodes a uint64 into buf and returns the number of bytes written.
        // If the buffer is too small, PutUvarint will panic.
        public static long PutUvarint(slice<byte> buf, ulong x)
        {
            long i = 0L;
            while (x >= 0x80UL)
            {
                buf[i] = byte(x) | 0x80UL;
                x >>= 7L;
                i++;
            }

            buf[i] = byte(x);
            return i + 1L;

        }

        // Uvarint decodes a uint64 from buf and returns that value and the
        // number of bytes read (> 0). If an error occurred, the value is 0
        // and the number of bytes n is <= 0 meaning:
        //
        //     n == 0: buf too small
        //     n  < 0: value larger than 64 bits (overflow)
        //             and -n is the number of bytes read
        //
        public static (ulong, long) Uvarint(slice<byte> buf)
        {
            ulong _p0 = default;
            long _p0 = default;

            ulong x = default;
            ulong s = default;
            foreach (var (i, b) in buf)
            {
                if (b < 0x80UL)
                {
                    if (i > 9L || i == 9L && b > 1L)
                    {
                        return (0L, -(i + 1L)); // overflow
                    }

                    return (x | uint64(b) << (int)(s), i + 1L);

                }

                x |= uint64(b & 0x7fUL) << (int)(s);
                s += 7L;

            }
            return (0L, 0L);

        }

        // PutVarint encodes an int64 into buf and returns the number of bytes written.
        // If the buffer is too small, PutVarint will panic.
        public static long PutVarint(slice<byte> buf, long x)
        {
            var ux = uint64(x) << (int)(1L);
            if (x < 0L)
            {
                ux = ~ux;
            }

            return PutUvarint(buf, ux);

        }

        // Varint decodes an int64 from buf and returns that value and the
        // number of bytes read (> 0). If an error occurred, the value is 0
        // and the number of bytes n is <= 0 with the following meaning:
        //
        //     n == 0: buf too small
        //     n  < 0: value larger than 64 bits (overflow)
        //             and -n is the number of bytes read
        //
        public static (long, long) Varint(slice<byte> buf)
        {
            long _p0 = default;
            long _p0 = default;

            var (ux, n) = Uvarint(buf); // ok to continue in presence of error
            var x = int64(ux >> (int)(1L));
            if (ux & 1L != 0L)
            {
                x = ~x;
            }

            return (x, n);

        }

        private static var overflow = errors.New("binary: varint overflows a 64-bit integer");

        // ReadUvarint reads an encoded unsigned integer from r and returns it as a uint64.
        public static (ulong, error) ReadUvarint(io.ByteReader r)
        {
            ulong _p0 = default;
            error _p0 = default!;

            ulong x = default;
            ulong s = default;
            for (long i = 0L; i < MaxVarintLen64; i++)
            {
                var (b, err) = r.ReadByte();
                if (err != null)
                {
                    return (x, error.As(err)!);
                }

                if (b < 0x80UL)
                {
                    if (i == 9L && b > 1L)
                    {
                        return (x, error.As(overflow)!);
                    }

                    return (x | uint64(b) << (int)(s), error.As(null!)!);

                }

                x |= uint64(b & 0x7fUL) << (int)(s);
                s += 7L;

            }

            return (x, error.As(overflow)!);

        }

        // ReadVarint reads an encoded signed integer from r and returns it as an int64.
        public static (long, error) ReadVarint(io.ByteReader r)
        {
            long _p0 = default;
            error _p0 = default!;

            var (ux, err) = ReadUvarint(r); // ok to continue in presence of error
            var x = int64(ux >> (int)(1L));
            if (ux & 1L != 0L)
            {
                x = ~x;
            }

            return (x, error.As(err)!);

        }
    }
}}
