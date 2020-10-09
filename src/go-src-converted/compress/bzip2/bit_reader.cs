// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package bzip2 -- go2cs converted at 2020 October 09 06:05:12 UTC
// import "compress/bzip2" ==> using bzip2 = go.compress.bzip2_package
// Original source: C:\Go\src\compress\bzip2\bit_reader.go
using bufio = go.bufio_package;
using io = go.io_package;
using static go.builtin;

namespace go {
namespace compress
{
    public static partial class bzip2_package
    {
        // bitReader wraps an io.Reader and provides the ability to read values,
        // bit-by-bit, from it. Its Read* methods don't return the usual error
        // because the error handling was verbose. Instead, any error is kept and can
        // be checked afterwards.
        private partial struct bitReader
        {
            public io.ByteReader r;
            public ulong n;
            public ulong bits;
            public error err;
        }

        // newBitReader returns a new bitReader reading from r. If r is not
        // already an io.ByteReader, it will be converted via a bufio.Reader.
        private static bitReader newBitReader(io.Reader r)
        {
            io.ByteReader (byter, ok) = r._<io.ByteReader>();
            if (!ok)
            {
                byter = bufio.NewReader(r);
            }

            return new bitReader(r:byter);

        }

        // ReadBits64 reads the given number of bits and returns them in the
        // least-significant part of a uint64. In the event of an error, it returns 0
        // and the error can be obtained by calling Err().
        private static ulong ReadBits64(this ptr<bitReader> _addr_br, ulong bits)
        {
            ulong n = default;
            ref bitReader br = ref _addr_br.val;

            while (bits > br.bits)
            {
                var (b, err) = br.r.ReadByte();
                if (err == io.EOF)
                {
                    err = io.ErrUnexpectedEOF;
                }

                if (err != null)
                {
                    br.err = err;
                    return 0L;
                }

                br.n <<= 8L;
                br.n |= uint64(b);
                br.bits += 8L;

            } 

            // br.n looks like this (assuming that br.bits = 14 and bits = 6):
            // Bit: 111111
            //      5432109876543210
            //
            //         (6 bits, the desired output)
            //        |-----|
            //        V     V
            //      0101101101001110
            //        ^            ^
            //        |------------|
            //           br.bits (num valid bits)
            //
            // This the next line right shifts the desired bits into the
            // least-significant places and masks off anything above.
 

            // br.n looks like this (assuming that br.bits = 14 and bits = 6):
            // Bit: 111111
            //      5432109876543210
            //
            //         (6 bits, the desired output)
            //        |-----|
            //        V     V
            //      0101101101001110
            //        ^            ^
            //        |------------|
            //           br.bits (num valid bits)
            //
            // This the next line right shifts the desired bits into the
            // least-significant places and masks off anything above.
            n = (br.n >> (int)((br.bits - bits))) & ((1L << (int)(bits)) - 1L);
            br.bits -= bits;
            return ;

        }

        private static long ReadBits(this ptr<bitReader> _addr_br, ulong bits)
        {
            long n = default;
            ref bitReader br = ref _addr_br.val;

            var n64 = br.ReadBits64(bits);
            return int(n64);
        }

        private static bool ReadBit(this ptr<bitReader> _addr_br)
        {
            ref bitReader br = ref _addr_br.val;

            var n = br.ReadBits(1L);
            return n != 0L;
        }

        private static error Err(this ptr<bitReader> _addr_br)
        {
            ref bitReader br = ref _addr_br.val;

            return error.As(br.err)!;
        }
    }
}}
