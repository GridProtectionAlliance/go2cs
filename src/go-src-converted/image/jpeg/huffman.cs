// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package jpeg -- go2cs converted at 2022 March 06 23:36:07 UTC
// import "image/jpeg" ==> using jpeg = go.image.jpeg_package
// Original source: C:\Program Files\Go\src\image\jpeg\huffman.go
using io = go.io_package;

namespace go.image;

public static partial class jpeg_package {

    // maxCodeLength is the maximum (inclusive) number of bits in a Huffman code.
private static readonly nint maxCodeLength = 16;

// maxNCodes is the maximum (inclusive) number of codes in a Huffman tree.


// maxNCodes is the maximum (inclusive) number of codes in a Huffman tree.
private static readonly nint maxNCodes = 256;

// lutSize is the log-2 size of the Huffman decoder's look-up table.


// lutSize is the log-2 size of the Huffman decoder's look-up table.
private static readonly nint lutSize = 8;

// huffman is a Huffman decoder, specified in section C.


// huffman is a Huffman decoder, specified in section C.
private partial struct huffman {
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
private static error ensureNBits(this ptr<decoder> _addr_d, int n) {
    ref decoder d = ref _addr_d.val;

    while (true) {
        var (c, err) = d.readByteStuffedByte();
        if (err != null) {
            if (err == io.EOF) {
                return error.As(errShortHuffmanData)!;
            }
            return error.As(err)!;
        }
        d.bits.a = d.bits.a << 8 | uint32(c);
        d.bits.n += 8;
        if (d.bits.m == 0) {
            d.bits.m = 1 << 7;
        }
        else
 {
            d.bits.m<<=8;
        }
        if (d.bits.n >= n) {
            break;
        }
    }
    return error.As(null!)!;

}

// receiveExtend is the composition of RECEIVE and EXTEND, specified in section
// F.2.2.1.
private static (int, error) receiveExtend(this ptr<decoder> _addr_d, byte t) {
    int _p0 = default;
    error _p0 = default!;
    ref decoder d = ref _addr_d.val;

    if (d.bits.n < int32(t)) {
        {
            var err = d.ensureNBits(int32(t));

            if (err != null) {
                return (0, error.As(err)!);
            }

        }

    }
    d.bits.n -= int32(t);
    d.bits.m>>=t;
    var s = int32(1) << (int)(t);
    var x = int32(d.bits.a >> (int)(uint8(d.bits.n))) & (s - 1);
    if (x < s >> 1) {
        x += ((-1) << (int)(t)) + 1;
    }
    return (x, error.As(null!)!);

}

// processDHT processes a Define Huffman Table marker, and initializes a huffman
// struct from its contents. Specified in section B.2.4.2.
private static error processDHT(this ptr<decoder> _addr_d, nint n) {
    ref decoder d = ref _addr_d.val;

    while (n > 0) {
        if (n < 17) {
            return error.As(FormatError("DHT has wrong length"))!;
        }
        {
            var err__prev1 = err;

            var err = d.readFull(d.tmp[..(int)17]);

            if (err != null) {
                return error.As(err)!;
            }

            err = err__prev1;

        }

        var tc = d.tmp[0] >> 4;
        if (tc > maxTc) {
            return error.As(FormatError("bad Tc value"))!;
        }
        var th = d.tmp[0] & 0x0f; 
        // The baseline th <= 1 restriction is specified in table B.5.
        if (th > maxTh || (d.baseline && th > 1)) {
            return error.As(FormatError("bad Th value"))!;
        }
        var h = _addr_d.huff[tc][th]; 

        // Read nCodes and h.vals (and derive h.nCodes).
        // nCodes[i] is the number of codes with code length i.
        // h.nCodes is the total number of codes.
        h.nCodes = 0;
        array<int> nCodes = new array<int>(maxCodeLength);
        {
            var i__prev2 = i;

            foreach (var (__i) in nCodes) {
                i = __i;
                nCodes[i] = int32(d.tmp[i + 1]);
                h.nCodes += nCodes[i];
            }

            i = i__prev2;
        }

        if (h.nCodes == 0) {
            return error.As(FormatError("Huffman table has zero length"))!;
        }
        if (h.nCodes > maxNCodes) {
            return error.As(FormatError("Huffman table has excessive length"))!;
        }
        n -= int(h.nCodes) + 17;
        if (n < 0) {
            return error.As(FormatError("DHT has wrong length"))!;
        }
        {
            var err__prev1 = err;

            err = d.readFull(h.vals[..(int)h.nCodes]);

            if (err != null) {
                return error.As(err)!;
            } 

            // Derive the look-up table.

            err = err__prev1;

        } 

        // Derive the look-up table.
        {
            var i__prev2 = i;

            foreach (var (__i) in h.lut) {
                i = __i;
                h.lut[i] = 0;
            }

            i = i__prev2;
        }

        uint x = default;        uint code = default;

        {
            var i__prev2 = i;

            for (var i = uint32(0); i < lutSize; i++) {
                code<<=1;
                for (var j = int32(0); j < nCodes[i]; j++) { 
                    // The codeLength is 1+i, so shift code by 8-(1+i) to
                    // calculate the high bits for every 8-bit sequence
                    // whose codeLength's high bits matches code.
                    // The high 8 bits of lutValue are the encoded value.
                    // The low 8 bits are 1 plus the codeLength.
                    var @base = uint8(code << (int)((7 - i)));
                    var lutValue = uint16(h.vals[x]) << 8 | uint16(2 + i);
                    for (var k = uint8(0); k < 1 << (int)((7 - i)); k++) {
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
        int c = default;        int index = default;

        {
            var i__prev2 = i;

            foreach (var (__i, __n) in nCodes) {
                i = __i;
                n = __n;
                if (n == 0) {
                    h.minCodes[i] = -1;
                    h.maxCodes[i] = -1;
                    h.valsIndices[i] = -1;
                }
                else
 {
                    h.minCodes[i] = c;
                    h.maxCodes[i] = c + n - 1;
                    h.valsIndices[i] = index;
                    c += n;
                    index += n;
                }

                c<<=1;

            }

            i = i__prev2;
        }
    }
    return error.As(null!)!;

}

// decodeHuffman returns the next Huffman-coded value from the bit-stream,
// decoded according to h.
private static (byte, error) decodeHuffman(this ptr<decoder> _addr_d, ptr<huffman> _addr_h) {
    byte _p0 = default;
    error _p0 = default!;
    ref decoder d = ref _addr_d.val;
    ref huffman h = ref _addr_h.val;

    if (h.nCodes == 0) {
        return (0, error.As(FormatError("uninitialized Huffman table"))!);
    }
    if (d.bits.n < 8) {
        {
            var err__prev2 = err;

            var err = d.ensureNBits(8);

            if (err != null) {
                if (err != errMissingFF00 && err != errShortHuffmanData) {
                    return (0, error.As(err)!);
                } 
                // There are no more bytes of data in this segment, but we may still
                // be able to read the next symbol out of the previously read bits.
                // First, undo the readByte that the ensureNBits call made.
                if (d.bytes.nUnreadable != 0) {
                    d.unreadByteStuffedByte();
                }

                goto slowPath;

            }

            err = err__prev2;

        }

    }
    {
        var v = h.lut[(d.bits.a >> (int)(uint32(d.bits.n - lutSize))) & 0xff];

        if (v != 0) {
            var n = (v & 0xff) - 1;
            d.bits.n -= int32(n);
            d.bits.m>>=n;
            return (uint8(v >> 8), error.As(null!)!);
        }
    }


slowPath:
    for (nint i = 0;
    var code = int32(0); i < maxCodeLength; i++) {
        if (d.bits.n == 0) {
            {
                var err__prev2 = err;

                err = d.ensureNBits(1);

                if (err != null) {
                    return (0, error.As(err)!);
                }

                err = err__prev2;

            }

        }
        if (d.bits.a & d.bits.m != 0) {
            code |= 1;
        }
        d.bits.n--;
        d.bits.m>>=1;
        if (code <= h.maxCodes[i]) {
            return (h.vals[h.valsIndices[i] + code - h.minCodes[i]], error.As(null!)!);
        }
        code<<=1;

    }
    return (0, error.As(FormatError("bad Huffman code"))!);

}

private static (bool, error) decodeBit(this ptr<decoder> _addr_d) {
    bool _p0 = default;
    error _p0 = default!;
    ref decoder d = ref _addr_d.val;

    if (d.bits.n == 0) {
        {
            var err = d.ensureNBits(1);

            if (err != null) {
                return (false, error.As(err)!);
            }

        }

    }
    var ret = d.bits.a & d.bits.m != 0;
    d.bits.n--;
    d.bits.m>>=1;
    return (ret, error.As(null!)!);

}

private static (uint, error) decodeBits(this ptr<decoder> _addr_d, int n) {
    uint _p0 = default;
    error _p0 = default!;
    ref decoder d = ref _addr_d.val;

    if (d.bits.n < n) {
        {
            var err = d.ensureNBits(n);

            if (err != null) {
                return (0, error.As(err)!);
            }

        }

    }
    var ret = d.bits.a >> (int)(uint32(d.bits.n - n));
    ret &= (1 << (int)(uint32(n))) - 1;
    d.bits.n -= n;
    d.bits.m>>=uint32(n);
    return (ret, error.As(null!)!);

}

} // end jpeg_package
