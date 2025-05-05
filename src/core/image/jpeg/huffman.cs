// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.image;

using io = io_package;

partial class jpeg_package {

// maxCodeLength is the maximum (inclusive) number of bits in a Huffman code.
internal static readonly UntypedInt maxCodeLength = 16;

// maxNCodes is the maximum (inclusive) number of codes in a Huffman tree.
internal static readonly UntypedInt maxNCodes = 256;

// lutSize is the log-2 size of the Huffman decoder's look-up table.
internal static readonly UntypedInt lutSize = 8;

// huffman is a Huffman decoder, specified in section C.
[GoType] partial struct huffman {
    // length is the number of codes in the tree.
    internal int32 nCodes;
    // lut is the look-up table for the next lutSize bits in the bit-stream.
    // The high 8 bits of the uint16 are the encoded value. The low 8 bits
    // are 1 plus the code length, or 0 if the value is too large to fit in
    // lutSize bits.
    internal array<uint16> lut = new(1 << (int)(lutSize));
    // vals are the decoded values, sorted by their encoding.
    internal array<uint8> vals = new(maxNCodes);
    // minCodes[i] is the minimum code of length i, or -1 if there are no
    // codes of that length.
    internal array<int32> minCodes = new(maxCodeLength);
    // maxCodes[i] is the maximum code of length i, or -1 if there are no
    // codes of that length.
    internal array<int32> maxCodes = new(maxCodeLength);
    // valsIndices[i] is the index into vals of minCodes[i].
    internal array<int32> valsIndices = new(maxCodeLength);
}

// errShortHuffmanData means that an unexpected EOF occurred while decoding
// Huffman data.
internal static FormatError errShortHuffmanData = ((FormatError)"short Huffman data"u8);

// ensureNBits reads bytes from the byte buffer to ensure that d.bits.n is at
// least n. For best performance (avoiding function calls inside hot loops),
// the caller is the one responsible for first checking that d.bits.n < n.
[GoRecv] internal static error ensureNBits(this ref decoder d, int32 n) {
    while (ᐧ) {
        var (c, err) = d.readByteStuffedByte();
        if (err != default!) {
            if (AreEqual(err, io.ErrUnexpectedEOF)) {
                return errShortHuffmanData;
            }
            return err;
        }
        d.bits.a = (uint32)(d.bits.a << (int)(8) | ((uint32)c));
        d.bits.n += 8;
        if (d.bits.m == 0){
            d.bits.m = 1 << (int)(7);
        } else {
            d.bits.m <<= (UntypedInt)(8);
        }
        if (d.bits.n >= n) {
            break;
        }
    }
    return default!;
}

// receiveExtend is the composition of RECEIVE and EXTEND, specified in section
// F.2.2.1.
[GoRecv] internal static (int32, error) receiveExtend(this ref decoder d, uint8 t) {
    if (d.bits.n < ((int32)t)) {
        {
            var err = d.ensureNBits(((int32)t)); if (err != default!) {
                return (0, err);
            }
        }
    }
    d.bits.n -= ((int32)t);
    d.bits.m >>= (uint8)(t);
    var s = ((int32)1) << (int)(t);
    var x = (int32)(((int32)(d.bits.a >> (int)(((uint8)d.bits.n)))) & (s - 1));
    if (x < s >> (int)(1)) {
        x += ((-1) << (int)(t)) + 1;
    }
    return (x, default!);
}

// processDHT processes a Define Huffman Table marker, and initializes a huffman
// struct from its contents. Specified in section B.2.4.2.
[GoRecv] internal static error processDHT(this ref decoder d, nint n) {
    while (n > 0) {
        if (n < 17) {
            return ((FormatError)"DHT has wrong length"u8);
        }
        {
            var err = d.readFull(d.tmp[..17]); if (err != default!) {
                return err;
            }
        }
        var tc = d.tmp[0] >> (int)(4);
        if (tc > maxTc) {
            return ((FormatError)"bad Tc value"u8);
        }
        var th = (byte)(d.tmp[0] & 15);
        // The baseline th <= 1 restriction is specified in table B.5.
        if (th > maxTh || (d.baseline && th > 1)) {
            return ((FormatError)"bad Th value"u8);
        }
        var h = Ꮡ(d.huff[tc][th]);
        // Read nCodes and h.vals (and derive h.nCodes).
        // nCodes[i] is the number of codes with code length i.
        // h.nCodes is the total number of codes.
        h.val.nCodes = 0;
        array<int32> nCodes = new(16); /* maxCodeLength */
        foreach (var (i, _) in nCodes) {
            nCodes[i] = ((int32)d.tmp[i + 1]);
            h.val.nCodes += nCodes[i];
        }
        if ((~h).nCodes == 0) {
            return ((FormatError)"Huffman table has zero length"u8);
        }
        if ((~h).nCodes > maxNCodes) {
            return ((FormatError)"Huffman table has excessive length"u8);
        }
        n -= ((nint)(~h).nCodes) + 17;
        if (n < 0) {
            return ((FormatError)"DHT has wrong length"u8);
        }
        {
            var err = d.readFull((~h).vals[..(int)((~h).nCodes)]); if (err != default!) {
                return err;
            }
        }
        // Derive the look-up table.
        clear((~h).lut[..]);
        uint32 x = default!;
        uint32 code = default!;
        for (var i = ((uint32)0); i < lutSize; i++) {
            code <<= (UntypedInt)(1);
            for (var j = ((int32)0); j < nCodes[i]; j++) {
                // The codeLength is 1+i, so shift code by 8-(1+i) to
                // calculate the high bits for every 8-bit sequence
                // whose codeLength's high bits matches code.
                // The high 8 bits of lutValue are the encoded value.
                // The low 8 bits are 1 plus the codeLength.
                var @base = ((uint8)(code << (int)((7 - i))));
                var lutValue = (uint16)(((uint16)(~h).vals[x]) << (int)(8) | ((uint16)(2 + i)));
                for (var k = ((uint8)0); k < 1 << (int)((7 - i)); k++) {
                    (~h).lut[(uint8)(@base | k)] = lutValue;
                }
                code++;
                x++;
            }
        }
        // Derive minCodes, maxCodes, and valsIndices.
        int32 c = default!;
        int32 index = default!;
        foreach (var (i, nΔ1) in nCodes) {
            if (nΔ1 == 0){
                (~h).minCodes[i] = -1;
                (~h).maxCodes[i] = -1;
                (~h).valsIndices[i] = -1;
            } else {
                (~h).minCodes[i] = c;
                (~h).maxCodes[i] = c + nΔ1 - 1;
                (~h).valsIndices[i] = index;
                c += nΔ1;
                index += nΔ1;
            }
            c <<= (UntypedInt)(1);
        }
    }
    return default!;
}

// decodeHuffman returns the next Huffman-coded value from the bit-stream,
// decoded according to h.
[GoRecv] internal static (uint8, error) decodeHuffman(this ref decoder d, ж<huffman> Ꮡh) {
    ref var h = ref Ꮡh.val;

    if (h.nCodes == 0) {
        return (0, ((FormatError)"uninitialized Huffman table"u8));
    }
    if (d.bits.n < 8) {
        {
            var err = d.ensureNBits(8); if (err != default!) {
                if (err != errMissingFF00 && err != errShortHuffmanData) {
                    return (0, err);
                }
                // There are no more bytes of data in this segment, but we may still
                // be able to read the next symbol out of the previously read bits.
                // First, undo the readByte that the ensureNBits call made.
                if (d.bytes.nUnreadable != 0) {
                    d.unreadByteStuffedByte();
                }
                goto slowPath;
            }
        }
    }
    {
        var v = h.lut[(uint32)((d.bits.a >> (int)(((uint32)(d.bits.n - lutSize)))) & 255)]; if (v != 0) {
            var n = ((uint16)(v & 255)) - 1;
            d.bits.n -= ((int32)n);
            d.bits.m >>= (uint16)(n);
            return (((uint8)(v >> (int)(8))), default!);
        }
    }
slowPath:
    for (nint i = 0;var code = ((int32)0); i < maxCodeLength; i++) {
        if (d.bits.n == 0) {
            {
                var err = d.ensureNBits(1); if (err != default!) {
                    return (0, err);
                }
            }
        }
        if ((uint32)(d.bits.a & d.bits.m) != 0) {
            code |= (int32)(1);
        }
        d.bits.n--;
        d.bits.m >>= (UntypedInt)(1);
        if (code <= h.maxCodes[i]) {
            return (h.vals[h.valsIndices[i] + code - h.minCodes[i]], default!);
        }
        code <<= (UntypedInt)(1);
continue_slowPath:;
    }
break_slowPath:;
    return (0, ((FormatError)"bad Huffman code"u8));
}

[GoRecv] internal static (bool, error) decodeBit(this ref decoder d) {
    if (d.bits.n == 0) {
        {
            var err = d.ensureNBits(1); if (err != default!) {
                return (false, err);
            }
        }
    }
    var ret = (uint32)(d.bits.a & d.bits.m) != 0;
    d.bits.n--;
    d.bits.m >>= (UntypedInt)(1);
    return (ret, default!);
}

[GoRecv] internal static (uint32, error) decodeBits(this ref decoder d, int32 n) {
    if (d.bits.n < n) {
        {
            var err = d.ensureNBits(n); if (err != default!) {
                return (0, err);
            }
        }
    }
    var ret = d.bits.a >> (int)(((uint32)(d.bits.n - n)));
    ret &= (uint32)((1 << (int)(((uint32)n))) - 1);
    d.bits.n -= n;
    d.bits.m >>= (uint32)(((uint32)n));
    return (ret, default!);
}

} // end jpeg_package
