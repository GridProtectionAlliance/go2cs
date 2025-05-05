// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using io = io_package;
using bits = math.bits_package;
using math;

partial class zstd_package {

// maxHuffmanBits is the largest possible Huffman table bits.
internal static readonly UntypedInt maxHuffmanBits = 11;

// readHuff reads Huffman table from data starting at off into table.
// Each entry in a Huffman table is a pair of bytes.
// The high byte is the encoded value. The low byte is the number
// of bits used to encode that value. We index into the table
// with a value of size tableBits. A value that requires fewer bits
// appear in the table multiple times.
// This returns the number of bits in the Huffman table and the new offset.
// RFC 4.2.1.
[GoRecv] internal static (nint tableBits, nint roff, error err) readHuff(this ref Reader r, block data, nint off, slice<uint16> table) {
    nint tableBits = default!;
    nint roff = default!;
    error err = default!;

    if (off >= len(data)) {
        return (0, 0, r.makeEOFError(off));
    }
    var hdr = data[off];
    off++;
    array<uint8> weights = new(256);
    nint count = default!;
    if (hdr < 128){
        // The table is compressed using an FSE. RFC 4.2.1.2.
        if (len(r.fseScratch) < 1 << (int)(6)) {
            r.fseScratch = new slice<fseEntry>(1 << (int)(6));
        }
        var (fseBits, noff, errΔ1) = r.readFSE(data, off, 255, 6, r.fseScratch);
        if (errΔ1 != default!) {
            return (0, 0, errΔ1);
        }
        var fseTable = r.fseScratch;
        if (off + ((nint)hdr) > len(data)) {
            return (0, 0, r.makeEOFError(off));
        }
        var (rbr, errΔ1) = r.makeReverseBitReader(data, off + ((nint)hdr) - 1, noff);
        if (errΔ1 != default!) {
            return (0, 0, errΔ1);
        }
        var (state1, errΔ1) = rbr.val(((uint8)fseBits));
        if (errΔ1 != default!) {
            return (0, 0, errΔ1);
        }
        var (state2, errΔ1) = rbr.val(((uint8)fseBits));
        if (errΔ1 != default!) {
            return (0, 0, errΔ1);
        }
        // There are two independent FSE streams, tracked by
        // state1 and state2. We decode them alternately.
        while (ᐧ) {
            var pt = Ꮡ(fseTable, state1);
            if (!rbr.fetch((~pt).bits)) {
                if (count >= 254) {
                    return (0, 0, rbr.makeError("Huffman count overflow"u8));
                }
                weights[count] = pt.val.sym;
                weights[count + 1] = fseTable[state2].sym;
                count += 2;
                break;
            }
            var (v, errΔ2) = rbr.val((~pt).bits);
            if (errΔ2 != default!) {
                return (0, 0, errΔ2);
            }
            state1 = ((uint32)(~pt).@base) + v;
            if (count >= 255) {
                return (0, 0, rbr.makeError("Huffman count overflow"u8));
            }
            weights[count] = pt.val.sym;
            count++;
            pt = Ꮡ(fseTable, state2);
            if (!rbr.fetch((~pt).bits)) {
                if (count >= 254) {
                    return (0, 0, rbr.makeError("Huffman count overflow"u8));
                }
                weights[count] = pt.val.sym;
                weights[count + 1] = fseTable[state1].sym;
                count += 2;
                break;
            }
            (v, errΔ2) = rbr.val((~pt).bits);
            if (errΔ2 != default!) {
                return (0, 0, errΔ2);
            }
            state2 = ((uint32)(~pt).@base) + v;
            if (count >= 255) {
                return (0, 0, rbr.makeError("Huffman count overflow"u8));
            }
            weights[count] = pt.val.sym;
            count++;
        }
        off += ((nint)hdr);
    } else {
        // The table is not compressed. Each weight is 4 bits.
        count = ((nint)hdr) - 127;
        if (off + ((count + 1) / 2) >= len(data)) {
            return (0, 0, io.ErrUnexpectedEOF);
        }
        for (nint iΔ1 = 0; iΔ1 < count;  += 2) {
            var b = data[off];
            off++;
            weights[i] = b >> (int)(4);
            weights[i + 1] = (byte)(b & 15);
        }
    }
    // RFC 4.2.1.3.
    array<uint32> weightMark = new(13);
    var weightMask = ((uint32)0);
    foreach (var (_, w) in weights[..(int)(count)]) {
        if (w > 12) {
            return (0, 0, r.makeError(off, "Huffman weight overflow"u8));
        }
        weightMark[w]++;
        if (w > 0) {
            weightMask += 1 << (int)((w - 1));
        }
    }
    if (weightMask == 0) {
        return (0, 0, r.makeError(off, "bad Huffman weights"u8));
    }
    tableBits = 32 - bits.LeadingZeros32(weightMask);
    if (tableBits > maxHuffmanBits) {
        return (0, 0, r.makeError(off, "bad Huffman weights"u8));
    }
    if (len(table) < 1 << (int)(tableBits)) {
        return (0, 0, r.makeError(off, "Huffman table too small"u8));
    }
    // Work out the last weight value, which is omitted because
    // the weights must sum to a power of two.
    var left = (((uint32)1) << (int)(tableBits)) - weightMask;
    if (left == 0) {
        return (0, 0, r.makeError(off, "bad Huffman weights"u8));
    }
    nint highBit = 31 - bits.LeadingZeros32(left);
    if (((uint32)1) << (int)(highBit) != left) {
        return (0, 0, r.makeError(off, "bad Huffman weights"u8));
    }
    if (count >= 256) {
        return (0, 0, r.makeError(off, "Huffman weight overflow"u8));
    }
    weights[count] = ((uint8)(highBit + 1));
    count++;
    weightMark[highBit + 1]++;
    if (weightMark[1] < 2 || (uint32)(weightMark[1] & 1) != 0) {
        return (0, 0, r.makeError(off, "bad Huffman weights"u8));
    }
    // Change weightMark from a count of weights to the index of
    // the first symbol for that weight. We shift the indexes to
    // also store how many we have seen so far,
    var next = ((uint32)0);
    for (nint i = 0; i < tableBits; i++) {
        var cur = next;
        next += weightMark[i + 1] << (int)(i);
        weightMark[i + 1] = cur;
    }
    foreach (var (i, w) in weights[..(int)(count)]) {
        if (w == 0) {
            continue;
        }
        var length = ((uint32)1) << (int)((w - 1));
        var tval = (uint16)(((uint16)i) << (int)(8) | (((uint16)tableBits) + 1 - ((uint16)w)));
        var start = weightMark[w];
        for (var j = ((uint32)0); j < length; j++) {
            table[start + j] = tval;
        }
        weightMark[w] += length;
    }
    return (tableBits, off, default!);
}

} // end zstd_package
