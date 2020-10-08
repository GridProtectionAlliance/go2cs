// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package jpeg -- go2cs converted at 2020 October 08 04:59:25 UTC
// import "image/jpeg" ==> using jpeg = go.image.jpeg_package
// Original source: C:\Go\src\image\jpeg\huffman.go
using io = go.io_package;
using static go.builtin;

namespace go {
namespace image
{
    public static partial class jpeg_package
    {
        // maxCodeLength is the maximum (inclusive) number of bits in a Huffman code.
        private static readonly long maxCodeLength = (long)16L;

        // maxNCodes is the maximum (inclusive) number of codes in a Huffman tree.


        // maxNCodes is the maximum (inclusive) number of codes in a Huffman tree.
        private static readonly long maxNCodes = (long)256L;

        // lutSize is the log-2 size of the Huffman decoder's look-up table.


        // lutSize is the log-2 size of the Huffman decoder's look-up table.
        private static readonly long lutSize = (long)8L;

        // huffman is a Huffman decoder, specified in section C.


        // huffman is a Huffman decoder, specified in section C.
        private partial struct huffman
        {
            public int nCodes; // lut is the look-up table for the next lutSize bits in the bit-stream.
// The high 8 bits of the uint16 are the encoded value. The low 8 bits
// are 1 plus the code length, or 0 if the value is too large to fit in
// lutSize bits.
            public array<ushort> lut; // vals are the decoded values, sorted by their encoding.
            public array<byte> vals; // minCodes[i] is the minimum code of length i, or -1 if there are no
// codes of that length.
            public array<int> minCodes; // maxCodes[i] is the maximum code of length i, or -1 if there are no
// codes of that length.
            public array<int> maxCodes; // valsIndices[i] is the index into vals of minCodes[i].
            public array<int> valsIndices;
        }

        // errShortHuffmanData means that an unexpected EOF occurred while decoding
        // Huffman data.
        private static var errShortHuffmanData = FormatError("short Huffman data");

        // ensureNBits reads bytes from the byte buffer to ensure that d.bits.n is at
        // least n. For best performance (avoiding function calls inside hot loops),
        // the caller is the one responsible for first checking that d.bits.n < n.
        private static error ensureNBits(this ptr<decoder> _addr_d, int n)
        {
            ref decoder d = ref _addr_d.val;

            while (true)
            {
                var (c, err) = d.readByteStuffedByte();
                if (err != null)
                {
                    if (err == io.EOF)
                    {
                        return error.As(errShortHuffmanData)!;
                    }

                    return error.As(err)!;

                }

                d.bits.a = d.bits.a << (int)(8L) | uint32(c);
                d.bits.n += 8L;
                if (d.bits.m == 0L)
                {
                    d.bits.m = 1L << (int)(7L);
                }
                else
                {
                    d.bits.m <<= 8L;
                }

                if (d.bits.n >= n)
                {
                    break;
                }

            }

            return error.As(null!)!;

        }

        // receiveExtend is the composition of RECEIVE and EXTEND, specified in section
        // F.2.2.1.
        private static (int, error) receiveExtend(this ptr<decoder> _addr_d, byte t)
        {
            int _p0 = default;
            error _p0 = default!;
            ref decoder d = ref _addr_d.val;

            if (d.bits.n < int32(t))
            {
                {
                    var err = d.ensureNBits(int32(t));

                    if (err != null)
                    {
                        return (0L, error.As(err)!);
                    }

                }

            }

            d.bits.n -= int32(t);
            d.bits.m >>= t;
            var s = int32(1L) << (int)(t);
            var x = int32(d.bits.a >> (int)(uint8(d.bits.n))) & (s - 1L);
            if (x < s >> (int)(1L))
            {
                x += ((-1L) << (int)(t)) + 1L;
            }

            return (x, error.As(null!)!);

        }

        // processDHT processes a Define Huffman Table marker, and initializes a huffman
        // struct from its contents. Specified in section B.2.4.2.
        private static error processDHT(this ptr<decoder> _addr_d, long n)
        {
            ref decoder d = ref _addr_d.val;

            while (n > 0L)
            {
                if (n < 17L)
                {
                    return error.As(FormatError("DHT has wrong length"))!;
                }

                {
                    var err__prev1 = err;

                    var err = d.readFull(d.tmp[..17L]);

                    if (err != null)
                    {
                        return error.As(err)!;
                    }

                    err = err__prev1;

                }

                var tc = d.tmp[0L] >> (int)(4L);
                if (tc > maxTc)
                {
                    return error.As(FormatError("bad Tc value"))!;
                }

                var th = d.tmp[0L] & 0x0fUL; 
                // The baseline th <= 1 restriction is specified in table B.5.
                if (th > maxTh || (d.baseline && th > 1L))
                {
                    return error.As(FormatError("bad Th value"))!;
                }

                var h = _addr_d.huff[tc][th]; 

                // Read nCodes and h.vals (and derive h.nCodes).
                // nCodes[i] is the number of codes with code length i.
                // h.nCodes is the total number of codes.
                h.nCodes = 0L;
                array<int> nCodes = new array<int>(maxCodeLength);
                {
                    var i__prev2 = i;

                    foreach (var (__i) in nCodes)
                    {
                        i = __i;
                        nCodes[i] = int32(d.tmp[i + 1L]);
                        h.nCodes += nCodes[i];
                    }

                    i = i__prev2;
                }

                if (h.nCodes == 0L)
                {
                    return error.As(FormatError("Huffman table has zero length"))!;
                }

                if (h.nCodes > maxNCodes)
                {
                    return error.As(FormatError("Huffman table has excessive length"))!;
                }

                n -= int(h.nCodes) + 17L;
                if (n < 0L)
                {
                    return error.As(FormatError("DHT has wrong length"))!;
                }

                {
                    var err__prev1 = err;

                    err = d.readFull(h.vals[..h.nCodes]);

                    if (err != null)
                    {
                        return error.As(err)!;
                    } 

                    // Derive the look-up table.

                    err = err__prev1;

                } 

                // Derive the look-up table.
                {
                    var i__prev2 = i;

                    foreach (var (__i) in h.lut)
                    {
                        i = __i;
                        h.lut[i] = 0L;
                    }

                    i = i__prev2;
                }

                uint x = default;                uint code = default;

                {
                    var i__prev2 = i;

                    for (var i = uint32(0L); i < lutSize; i++)
                    {
                        code <<= 1L;
                        for (var j = int32(0L); j < nCodes[i]; j++)
                        { 
                            // The codeLength is 1+i, so shift code by 8-(1+i) to
                            // calculate the high bits for every 8-bit sequence
                            // whose codeLength's high bits matches code.
                            // The high 8 bits of lutValue are the encoded value.
                            // The low 8 bits are 1 plus the codeLength.
                            var @base = uint8(code << (int)((7L - i)));
                            var lutValue = uint16(h.vals[x]) << (int)(8L) | uint16(2L + i);
                            for (var k = uint8(0L); k < 1L << (int)((7L - i)); k++)
                            {
                                h.lut[base | k] = lutValue;
                            }

                            code++;
                            x++;

                        }


                    } 

                    // Derive minCodes, maxCodes, and valsIndices.


                    i = i__prev2;
                } 

                // Derive minCodes, maxCodes, and valsIndices.
                int c = default;                int index = default;

                {
                    var i__prev2 = i;

                    foreach (var (__i, __n) in nCodes)
                    {
                        i = __i;
                        n = __n;
                        if (n == 0L)
                        {
                            h.minCodes[i] = -1L;
                            h.maxCodes[i] = -1L;
                            h.valsIndices[i] = -1L;
                        }
                        else
                        {
                            h.minCodes[i] = c;
                            h.maxCodes[i] = c + n - 1L;
                            h.valsIndices[i] = index;
                            c += n;
                            index += n;
                        }

                        c <<= 1L;

                    }

                    i = i__prev2;
                }
            }

            return error.As(null!)!;

        }

        // decodeHuffman returns the next Huffman-coded value from the bit-stream,
        // decoded according to h.
        private static (byte, error) decodeHuffman(this ptr<decoder> _addr_d, ptr<huffman> _addr_h)
        {
            byte _p0 = default;
            error _p0 = default!;
            ref decoder d = ref _addr_d.val;
            ref huffman h = ref _addr_h.val;

            if (h.nCodes == 0L)
            {
                return (0L, error.As(FormatError("uninitialized Huffman table"))!);
            }

            if (d.bits.n < 8L)
            {
                {
                    var err__prev2 = err;

                    var err = d.ensureNBits(8L);

                    if (err != null)
                    {
                        if (err != errMissingFF00 && err != errShortHuffmanData)
                        {
                            return (0L, error.As(err)!);
                        } 
                        // There are no more bytes of data in this segment, but we may still
                        // be able to read the next symbol out of the previously read bits.
                        // First, undo the readByte that the ensureNBits call made.
                        if (d.bytes.nUnreadable != 0L)
                        {
                            d.unreadByteStuffedByte();
                        }

                        goto slowPath;

                    }

                    err = err__prev2;

                }

            }

            {
                var v = h.lut[(d.bits.a >> (int)(uint32(d.bits.n - lutSize))) & 0xffUL];

                if (v != 0L)
                {
                    var n = (v & 0xffUL) - 1L;
                    d.bits.n -= int32(n);
                    d.bits.m >>= n;
                    return (uint8(v >> (int)(8L)), error.As(null!)!);
                }

            }


slowPath:
            for (long i = 0L;
            var code = int32(0L); i < maxCodeLength; i++)
            {
                if (d.bits.n == 0L)
                {
                    {
                        var err__prev2 = err;

                        err = d.ensureNBits(1L);

                        if (err != null)
                        {
                            return (0L, error.As(err)!);
                        }

                        err = err__prev2;

                    }

                }

                if (d.bits.a & d.bits.m != 0L)
                {
                    code |= 1L;
                }

                d.bits.n--;
                d.bits.m >>= 1L;
                if (code <= h.maxCodes[i])
                {
                    return (h.vals[h.valsIndices[i] + code - h.minCodes[i]], error.As(null!)!);
                }

                code <<= 1L;

            }
            return (0L, error.As(FormatError("bad Huffman code"))!);

        }

        private static (bool, error) decodeBit(this ptr<decoder> _addr_d)
        {
            bool _p0 = default;
            error _p0 = default!;
            ref decoder d = ref _addr_d.val;

            if (d.bits.n == 0L)
            {
                {
                    var err = d.ensureNBits(1L);

                    if (err != null)
                    {
                        return (false, error.As(err)!);
                    }

                }

            }

            var ret = d.bits.a & d.bits.m != 0L;
            d.bits.n--;
            d.bits.m >>= 1L;
            return (ret, error.As(null!)!);

        }

        private static (uint, error) decodeBits(this ptr<decoder> _addr_d, int n)
        {
            uint _p0 = default;
            error _p0 = default!;
            ref decoder d = ref _addr_d.val;

            if (d.bits.n < n)
            {
                {
                    var err = d.ensureNBits(n);

                    if (err != null)
                    {
                        return (0L, error.As(err)!);
                    }

                }

            }

            var ret = d.bits.a >> (int)(uint32(d.bits.n - n));
            ret &= (1L << (int)(uint32(n))) - 1L;
            d.bits.n -= n;
            d.bits.m >>= uint32(n);
            return (ret, error.As(null!)!);

        }
    }
}}
