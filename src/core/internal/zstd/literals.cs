// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using binary = encoding.binary_package;
using encoding;

partial class zstd_package {

// readLiterals reads and decompresses the literals from data at off.
// The literals are appended to outbuf, which is returned.
// Also returns the new input offset. RFC 3.1.1.3.1.
[GoRecv] internal static (nint, slice<byte>, error) readLiterals(this ref Reader r, block data, nint off, slice<byte> outbuf) {
    if (off >= len(data)) {
        return (0, default!, r.makeEOFError(off));
    }
    // Literals section header. RFC 3.1.1.3.1.1.
    var hdr = data[off];
    off++;
    if (((byte)(hdr & 3)) == 0 || ((byte)(hdr & 3)) == 1){
        return r.readRawRLELiterals(data, off, hdr, outbuf);
    } else {
        return r.readHuffLiterals(data, off, hdr, outbuf);
    }
}

// readRawRLELiterals reads and decompresses a Raw_Literals_Block or
// a RLE_Literals_Block. RFC 3.1.1.3.1.1.
[GoRecv] internal static (nint, slice<byte>, error) readRawRLELiterals(this ref Reader r, block data, nint off, byte hdr, slice<byte> outbuf) {
    var raw = ((byte)(hdr & 3)) == 0;
    nint regeneratedSize = default!;
    switch ((byte)((hdr >> (int)(2)) & 3)) {
    case 0 or 2: {
        regeneratedSize = ((nint)(hdr >> (int)(3)));
        break;
    }
    case 1: {
        if (off >= len(data)) {
            return (0, default!, r.makeEOFError(off));
        }
        regeneratedSize = ((nint)(hdr >> (int)(4))) + (((nint)data[off]) << (int)(4));
        off++;
        break;
    }
    case 3: {
        if (off + 1 >= len(data)) {
            return (0, default!, r.makeEOFError(off));
        }
        regeneratedSize = ((nint)(hdr >> (int)(4))) + (((nint)data[off]) << (int)(4)) + (((nint)data[off + 1]) << (int)(12));
        off += 2;
        break;
    }}

    // We are going to use the entire literal block in the output.
    // The maximum size of one decompressed block is 128K,
    // so we can't have more literals than that.
    if (regeneratedSize > 128 << (int)(10)) {
        return (0, default!, r.makeError(off, "literal size too large"u8));
    }
    if (raw){
        // RFC 3.1.1.3.1.2.
        if (off + regeneratedSize > len(data)) {
            return (0, default!, r.makeError(off, "raw literal size too large"u8));
        }
        outbuf = append(outbuf, data[(int)(off)..(int)(off + regeneratedSize)].ꓸꓸꓸ);
        off += regeneratedSize;
    } else {
        // RFC 3.1.1.3.1.3.
        if (off >= len(data)) {
            return (0, default!, r.makeError(off, "RLE literal missing"u8));
        }
        var rle = data[off];
        off++;
        for (nint i = 0; i < regeneratedSize; i++) {
            outbuf = append(outbuf, rle);
        }
    }
    return (off, outbuf, default!);
}

// readHuffLiterals reads and decompresses a Compressed_Literals_Block or
// a Treeless_Literals_Block. RFC 3.1.1.3.1.4.
[GoRecv] internal static (nint, slice<byte>, error) readHuffLiterals(this ref Reader r, block data, nint off, byte hdr, slice<byte> outbuf) {
    nint regeneratedSize = default!;
    nint compressedSize = default!;
    nint streams = default!;
    switch ((byte)((hdr >> (int)(2)) & 3)) {
    case 0 or 1: {
        if (off + 1 >= len(data)) {
            return (0, default!, r.makeEOFError(off));
        }
        regeneratedSize = (nint)((((nint)hdr) >> (int)(4)) | (((nint)(((nint)data[off]) & 63)) << (int)(4)));
        compressedSize = (nint)((((nint)data[off]) >> (int)(6)) | (((nint)data[off + 1]) << (int)(2)));
        off += 2;
        if (((byte)((hdr >> (int)(2)) & 3)) == 0){
            streams = 1;
        } else {
            streams = 4;
        }
        break;
    }
    case 2: {
        if (off + 2 >= len(data)) {
            return (0, default!, r.makeEOFError(off));
        }
        regeneratedSize = (nint)((nint)((((nint)hdr) >> (int)(4)) | (((nint)data[off]) << (int)(4))) | (((nint)(((nint)data[off + 1]) & 3)) << (int)(12)));
        compressedSize = (nint)((((nint)data[off + 1]) >> (int)(2)) | (((nint)data[off + 2]) << (int)(6)));
        off += 3;
        streams = 4;
        break;
    }
    case 3: {
        if (off + 3 >= len(data)) {
            return (0, default!, r.makeEOFError(off));
        }
        regeneratedSize = (nint)((nint)((((nint)hdr) >> (int)(4)) | (((nint)data[off]) << (int)(4))) | (((nint)(((nint)data[off + 1]) & 63)) << (int)(12)));
        compressedSize = (nint)((nint)((((nint)data[off + 1]) >> (int)(6)) | (((nint)data[off + 2]) << (int)(2))) | (((nint)data[off + 3]) << (int)(10)));
        off += 4;
        streams = 4;
        break;
    }}

    // We are going to use the entire literal block in the output.
    // The maximum size of one decompressed block is 128K,
    // so we can't have more literals than that.
    if (regeneratedSize > 128 << (int)(10)) {
        return (0, default!, r.makeError(off, "literal size too large"u8));
    }
    nint roff = off + compressedSize;
    if (roff > len(data) || roff < 0) {
        return (0, default!, r.makeEOFError(off));
    }
    nint totalStreamsSize = compressedSize;
    if (((byte)(hdr & 3)) == 2){
        // Compressed_Literals_Block.
        // Read new huffman tree.
        if (len(r.huffmanTable) < 1 << (int)(maxHuffmanBits)) {
            r.huffmanTable = new slice<uint16>(1 << (int)(maxHuffmanBits));
        }
        var (huffmanTableBits, hoff, errΔ1) = r.readHuff(data, off, r.huffmanTable);
        if (errΔ1 != default!) {
            return (0, default!, errΔ1);
        }
        r.huffmanTableBits = huffmanTableBits;
        if (totalStreamsSize < hoff - off) {
            return (0, default!, r.makeError(off, "Huffman table too big"u8));
        }
        totalStreamsSize -= hoff - off;
        off = hoff;
    } else {
        // Treeless_Literals_Block
        // Reuse previous Huffman tree.
        if (r.huffmanTableBits == 0) {
            return (0, default!, r.makeError(off, "missing literals Huffman tree"u8));
        }
    }
    // Decompress compressedSize bytes of data at off using the
    // Huffman tree.
    error err = default!;
    if (streams == 1){
        (outbuf, err) = r.readLiteralsOneStream(data, off, totalStreamsSize, regeneratedSize, outbuf);
    } else {
        (outbuf, err) = r.readLiteralsFourStreams(data, off, totalStreamsSize, regeneratedSize, outbuf);
    }
    if (err != default!) {
        return (0, default!, err);
    }
    return (roff, outbuf, default!);
}

// readLiteralsOneStream reads a single stream of compressed literals.
[GoRecv] internal static (slice<byte>, error) readLiteralsOneStream(this ref Reader r, block data, nint off, nint compressedSize, nint regeneratedSize, slice<byte> outbuf) {
    // We let the reverse bit reader read earlier bytes,
    // because the Huffman table ignores bits that it doesn't need.
    var (rbr, err) = r.makeReverseBitReader(data, off + compressedSize - 1, off - 2);
    if (err != default!) {
        return (default!, err);
    }
    var huffTable = r.huffmanTable;
    var huffBits = ((uint32)r.huffmanTableBits);
    var huffMask = (((uint32)1) << (int)(huffBits)) - 1;
    for (nint i = 0; i < regeneratedSize; i++) {
        if (!rbr.fetch(((uint8)huffBits))) {
            return (default!, rbr.makeError("literals Huffman stream out of bits"u8));
        }
        uint16 t = default!;
        var idx = (uint32)((rbr.bits >> (int)((rbr.cnt - huffBits))) & huffMask);
        t = huffTable[idx];
        outbuf = append(outbuf, ((byte)(t >> (int)(8))));
        rbr.cnt -= ((uint32)((uint16)(t & 255)));
    }
    return (outbuf, default!);
}

// readLiteralsFourStreams reads four interleaved streams of
// compressed literals.
[GoRecv] internal static (slice<byte>, error) readLiteralsFourStreams(this ref Reader r, block data, nint off, nint totalStreamsSize, nint regeneratedSize, slice<byte> outbuf) {
    // Read the jump table to find out where the streams are.
    // RFC 3.1.1.3.1.6.
    if (off + 5 >= len(data)) {
        return (default!, r.makeEOFError(off));
    }
    if (totalStreamsSize < 6) {
        return (default!, r.makeError(off, "total streams size too small for jump table"u8));
    }
    // RFC 3.1.1.3.1.6.
    // "The decompressed size of each stream is equal to (Regenerated_Size+3)/4,
    // except for the last stream, which may be up to 3 bytes smaller,
    // to reach a total decompressed size as specified in Regenerated_Size."
    nint regeneratedStreamSize = (regeneratedSize + 3) / 4;
    if (regeneratedSize < regeneratedStreamSize * 3) {
        return (default!, r.makeError(off, "regenerated size too small to decode streams"u8));
    }
    var streamSize1 = binary.LittleEndian.Uint16(data[(int)(off)..]);
    var streamSize2 = binary.LittleEndian.Uint16(data[(int)(off + 2)..]);
    var streamSize3 = binary.LittleEndian.Uint16(data[(int)(off + 4)..]);
    off += 6;
    var tot = ((uint64)streamSize1) + ((uint64)streamSize2) + ((uint64)streamSize3);
    if (tot > ((uint64)totalStreamsSize) - 6) {
        return (default!, r.makeEOFError(off));
    }
    var streamSize4 = ((uint32)totalStreamsSize) - 6 - ((uint32)tot);
    off--;
    nint off1 = off + ((nint)streamSize1);
    nint start1 = off + 1;
    nint off2 = off1 + ((nint)streamSize2);
    nint start2 = off1 + 1;
    nint off3 = off2 + ((nint)streamSize3);
    nint start3 = off2 + 1;
    nint off4 = off3 + ((nint)streamSize4);
    nint start4 = off3 + 1;
    // We let the reverse bit readers read earlier bytes,
    // because the Huffman tables ignore bits that they don't need.
    (rbr1, err) = r.makeReverseBitReader(data, off1, start1 - 2);
    if (err != default!) {
        return (default!, err);
    }
    (rbr2, err) = r.makeReverseBitReader(data, off2, start2 - 2);
    if (err != default!) {
        return (default!, err);
    }
    (rbr3, err) = r.makeReverseBitReader(data, off3, start3 - 2);
    if (err != default!) {
        return (default!, err);
    }
    (rbr4, err) = r.makeReverseBitReader(data, off4, start4 - 2);
    if (err != default!) {
        return (default!, err);
    }
    nint out1 = len(outbuf);
    nint out2 = out1 + regeneratedStreamSize;
    nint out3 = out2 + regeneratedStreamSize;
    nint out4 = out3 + regeneratedStreamSize;
    nint regeneratedStreamSize4 = regeneratedSize - regeneratedStreamSize * 3;
    outbuf = append(outbuf, new slice<byte>(regeneratedSize).ꓸꓸꓸ);
    var huffTable = r.huffmanTable;
    var huffBits = ((uint32)r.huffmanTableBits);
    var huffMask = (((uint32)1) << (int)(huffBits)) - 1;
    for (nint i = 0; i < regeneratedStreamSize; i++) {
        var use4 = i < regeneratedStreamSize4;
        var fetchHuff = 
        var huffTableʗ1 = huffTable;
        (ж<reverseBitReader> rbr) => {
            if (!rbr.fetch(((uint8)huffBits))) {
                return (0, rbr.makeError("literals Huffman stream out of bits"u8));
            }
            var idx = (uint32)(((~rbr).bits >> (int)(((~rbr).cnt - huffBits))) & huffMask);
            return (huffTableʗ1[idx], default!);
        };
        var (t1, err) = fetchHuff(Ꮡrbr1);
        if (err != default!) {
            return (default!, err);
        }
        var (t2, err) = fetchHuff(Ꮡrbr2);
        if (err != default!) {
            return (default!, err);
        }
        var (t3, err) = fetchHuff(Ꮡrbr3);
        if (err != default!) {
            return (default!, err);
        }
        if (use4) {
            var (t4, errΔ1) = fetchHuff(Ꮡrbr4);
            if (errΔ1 != default!) {
                return (default!, errΔ1);
            }
            outbuf[out4] = ((byte)(t4 >> (int)(8)));
            out4++;
            rbr4.cnt -= ((uint32)((uint16)(t4 & 255)));
        }
        outbuf[out1] = ((byte)(t1 >> (int)(8)));
        out1++;
        rbr1.cnt -= ((uint32)((uint16)(t1 & 255)));
        outbuf[out2] = ((byte)(t2 >> (int)(8)));
        out2++;
        rbr2.cnt -= ((uint32)((uint16)(t2 & 255)));
        outbuf[out3] = ((byte)(t3 >> (int)(8)));
        out3++;
        rbr3.cnt -= ((uint32)((uint16)(t3 & 255)));
    }
    return (outbuf, default!);
}

} // end zstd_package
