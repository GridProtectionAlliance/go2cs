// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package flate implements the DEFLATE compressed data format, described in
// RFC 1951.  The gzip and zlib packages implement access to DEFLATE-based file
// formats.

// package flate -- go2cs converted at 2022 March 13 05:29:14 UTC
// import "compress/flate" ==> using flate = go.compress.flate_package
// Original source: C:\Program Files\Go\src\compress\flate\inflate.go
namespace go.compress;

using bufio = bufio_package;
using io = io_package;
using bits = math.bits_package;
using strconv = strconv_package;
using sync = sync_package;
using System;

public static partial class flate_package {

private static readonly nint maxCodeLen = 16; // max length of Huffman code
// The next three numbers come from the RFC section 3.2.7, with the
// additional proviso in section 3.2.5 which implies that distance codes
// 30 and 31 should never occur in compressed data.
private static readonly nint maxNumLit = 286;
private static readonly nint maxNumDist = 30;
private static readonly nint numCodes = 19; // number of codes in Huffman meta-code

// Initialize the fixedHuffmanDecoder only once upon first use.
private static sync.Once fixedOnce = default;
private static huffmanDecoder fixedHuffmanDecoder = default;

// A CorruptInputError reports the presence of corrupt input at a given offset.
public partial struct CorruptInputError { // : long
}

public static @string Error(this CorruptInputError e) {
    return "flate: corrupt input before offset " + strconv.FormatInt(int64(e), 10);
}

// An InternalError reports an error in the flate code itself.
public partial struct InternalError { // : @string
}

public static @string Error(this InternalError e) {
    return "flate: internal error: " + string(e);
}

// A ReadError reports an error encountered while reading input.
//
// Deprecated: No longer returned.
public partial struct ReadError {
    public long Offset; // byte offset where error occurred
    public error Err; // error returned by underlying Read
}

private static @string Error(this ptr<ReadError> _addr_e) {
    ref ReadError e = ref _addr_e.val;

    return "flate: read error at offset " + strconv.FormatInt(e.Offset, 10) + ": " + e.Err.Error();
}

// A WriteError reports an error encountered while writing output.
//
// Deprecated: No longer returned.
public partial struct WriteError {
    public long Offset; // byte offset where error occurred
    public error Err; // error returned by underlying Write
}

private static @string Error(this ptr<WriteError> _addr_e) {
    ref WriteError e = ref _addr_e.val;

    return "flate: write error at offset " + strconv.FormatInt(e.Offset, 10) + ": " + e.Err.Error();
}

// Resetter resets a ReadCloser returned by NewReader or NewReaderDict
// to switch to a new underlying Reader. This permits reusing a ReadCloser
// instead of allocating a new one.
public partial interface Resetter {
    error Reset(io.Reader r, slice<byte> dict);
}

// The data structure for decoding Huffman tables is based on that of
// zlib. There is a lookup table of a fixed bit width (huffmanChunkBits),
// For codes smaller than the table width, there are multiple entries
// (each combination of trailing bits has the same value). For codes
// larger than the table width, the table contains a link to an overflow
// table. The width of each entry in the link table is the maximum code
// size minus the chunk width.
//
// Note that you can do a lookup in the table even without all bits
// filled. Since the extra bits are zero, and the DEFLATE Huffman codes
// have the property that shorter codes come before longer ones, the
// bit length estimate in the result is a lower bound on the actual
// number of bits.
//
// See the following:
//    https://github.com/madler/zlib/raw/master/doc/algorithm.txt

// chunk & 15 is number of bits
// chunk >> 4 is value, including table link

private static readonly nint huffmanChunkBits = 9;
private static readonly nint huffmanNumChunks = 1 << (int)(huffmanChunkBits);
private static readonly nint huffmanCountMask = 15;
private static readonly nint huffmanValueShift = 4;

private partial struct huffmanDecoder {
    public nint min; // the minimum code length
    public array<uint> chunks; // chunks as described above
    public slice<slice<uint>> links; // overflow links
    public uint linkMask; // mask the width of the link table
}

// Initialize Huffman decoding tables from array of code lengths.
// Following this function, h is guaranteed to be initialized into a complete
// tree (i.e., neither over-subscribed nor under-subscribed). The exception is a
// degenerate case where the tree has only a single symbol with length 1. Empty
// trees are permitted.
private static bool init(this ptr<huffmanDecoder> _addr_h, slice<nint> lengths) => func((_, panic, _) => {
    ref huffmanDecoder h = ref _addr_h.val;
 
    // Sanity enables additional runtime tests during Huffman
    // table construction. It's intended to be used during
    // development to supplement the currently ad-hoc unit tests.
    const var sanity = false;



    if (h.min != 0) {
        h.val = new huffmanDecoder();
    }
    array<nint> count = new array<nint>(maxCodeLen);
    nint min = default;    nint max = default;

    {
        var n__prev1 = n;

        foreach (var (_, __n) in lengths) {
            n = __n;
            if (n == 0) {
                continue;
            }
            if (min == 0 || n < min) {
                min = n;
            }
            if (n > max) {
                max = n;
            }
            count[n]++;
        }
        n = n__prev1;
    }

    if (max == 0) {
        return true;
    }
    nint code = 0;
    array<nint> nextcode = new array<nint>(maxCodeLen);
    {
        var i__prev1 = i;

        for (var i = min; i <= max; i++) {
            code<<=1;
            nextcode[i] = code;
            code += count[i];
        }

        i = i__prev1;
    } 

    // Check that the coding is complete (i.e., that we've
    // assigned all 2-to-the-max possible bit sequences).
    // Exception: To be compatible with zlib, we also need to
    // accept degenerate single-code codings. See also
    // TestDegenerateHuffmanCoding.
    if (code != 1 << (int)(uint(max)) && !(code == 1 && max == 1)) {
        return false;
    }
    h.min = min;
    if (max > huffmanChunkBits) {
        nint numLinks = 1 << (int)((uint(max) - huffmanChunkBits));
        h.linkMask = uint32(numLinks - 1); 

        // create link tables
        var link = nextcode[huffmanChunkBits + 1] >> 1;
        h.links = make_slice<slice<uint>>(huffmanNumChunks - link);
        {
            var j__prev1 = j;

            for (var j = uint(link); j < huffmanNumChunks; j++) {
                var reverse = int(bits.Reverse16(uint16(j)));
                reverse>>=uint(16 - huffmanChunkBits);
                var off = j - uint(link);
                if (sanity && h.chunks[reverse] != 0) {
                    panic("impossible: overwriting existing chunk");
                }
                h.chunks[reverse] = uint32(off << (int)(huffmanValueShift) | (huffmanChunkBits + 1));
                h.links[off] = make_slice<uint>(numLinks);
            }


            j = j__prev1;
        }
    }
    {
        var i__prev1 = i;
        var n__prev1 = n;

        foreach (var (__i, __n) in lengths) {
            i = __i;
            n = __n;
            if (n == 0) {
                continue;
            }
            code = nextcode[n];
            nextcode[n]++;
            var chunk = uint32(i << (int)(huffmanValueShift) | n);
            reverse = int(bits.Reverse16(uint16(code)));
            reverse>>=uint(16 - n);
            if (n <= huffmanChunkBits) {
                {
                    var off__prev2 = off;

                    off = reverse;

                    while (off < len(h.chunks)) { 
                        // We should never need to overwrite
                        // an existing chunk. Also, 0 is
                        // never a valid chunk, because the
                        // lower 4 "count" bits should be
                        // between 1 and 15.
                        if (sanity && h.chunks[off] != 0) {
                            panic("impossible: overwriting existing chunk");
                        off += 1 << (int)(uint(n));
                        }
                        h.chunks[off] = chunk;
                    }
            else


                    off = off__prev2;
                }
            } {
                j = reverse & (huffmanNumChunks - 1);
                if (sanity && h.chunks[j] & huffmanCountMask != huffmanChunkBits + 1) { 
                    // Longer codes should have been
                    // associated with a link table above.
                    panic("impossible: not an indirect chunk");
                }
                var value = h.chunks[j] >> (int)(huffmanValueShift);
                var linktab = h.links[value];
                reverse>>=huffmanChunkBits;
                {
                    var off__prev2 = off;

                    off = reverse;

                    while (off < len(linktab)) {
                        if (sanity && linktab[off] != 0) {
                            panic("impossible: overwriting existing chunk");
                        off += 1 << (int)(uint(n - huffmanChunkBits));
                        }
                        linktab[off] = chunk;
                    }


                    off = off__prev2;
                }
            }
        }
        i = i__prev1;
        n = n__prev1;
    }

    if (sanity) { 
        // Above we've sanity checked that we never overwrote
        // an existing entry. Here we additionally check that
        // we filled the tables completely.
        {
            var i__prev1 = i;
            var chunk__prev1 = chunk;

            foreach (var (__i, __chunk) in h.chunks) {
                i = __i;
                chunk = __chunk;
                if (chunk == 0) { 
                    // As an exception, in the degenerate
                    // single-code case, we allow odd
                    // chunks to be missing.
                    if (code == 1 && i % 2 == 1) {
                        continue;
                    }
                    panic("impossible: missing chunk");
                }
            }

            i = i__prev1;
            chunk = chunk__prev1;
        }

        {
            var linktab__prev1 = linktab;

            foreach (var (_, __linktab) in h.links) {
                linktab = __linktab;
                {
                    var chunk__prev2 = chunk;

                    foreach (var (_, __chunk) in linktab) {
                        chunk = __chunk;
                        if (chunk == 0) {
                            panic("impossible: missing chunk");
                        }
                    }

                    chunk = chunk__prev2;
                }
            }

            linktab = linktab__prev1;
        }
    }
    return true;
});

// The actual read interface needed by NewReader.
// If the passed in io.Reader does not also have ReadByte,
// the NewReader will introduce its own buffering.
public partial interface Reader {
}

// Decompress state.
private partial struct decompressor {
    public Reader r;
    public long roffset; // Input bits, in top of b.
    public uint b;
    public nuint nb; // Huffman decoders for literal/length, distance.
    public huffmanDecoder h1; // Length arrays used to define Huffman codes.
    public huffmanDecoder h2; // Length arrays used to define Huffman codes.
    public ptr<array<nint>> bits;
    public ptr<array<nint>> codebits; // Output history, buffer.
    public dictDecoder dict; // Temporary buffer (avoids repeated allocation).
    public array<byte> buf; // Next step in the decompression,
// and decompression state.
    public Action<ptr<decompressor>> step;
    public nint stepState;
    public bool final;
    public error err;
    public slice<byte> toRead;
    public ptr<huffmanDecoder> hl;
    public ptr<huffmanDecoder> hd;
    public nint copyLen;
    public nint copyDist;
}

private static void nextBlock(this ptr<decompressor> _addr_f) {
    ref decompressor f = ref _addr_f.val;

    while (f.nb < 1 + 2) {
        f.err = f.moreBits();

        if (f.err != null) {
            return ;
        }
    }
    f.final = f.b & 1 == 1;
    f.b>>=1;
    var typ = f.b & 3;
    f.b>>=2;
    f.nb -= 1 + 2;
    switch (typ) {
        case 0: 
            f.dataBlock();
            break;
        case 1: 
            // compressed, fixed Huffman tables
            f.hl = _addr_fixedHuffmanDecoder;
            f.hd = null;
            f.huffmanBlock();
            break;
        case 2: 
            // compressed, dynamic Huffman tables
            f.err = f.readHuffman();

            if (f.err != null) {
                break;
            }
            f.hl = _addr_f.h1;
            f.hd = _addr_f.h2;
            f.huffmanBlock();
            break;
        default: 
            // 3 is reserved.
            f.err = CorruptInputError(f.roffset);
            break;
    }
}

private static (nint, error) Read(this ptr<decompressor> _addr_f, slice<byte> b) {
    nint _p0 = default;
    error _p0 = default!;
    ref decompressor f = ref _addr_f.val;

    while (true) {
        if (len(f.toRead) > 0) {
            var n = copy(b, f.toRead);
            f.toRead = f.toRead[(int)n..];
            if (len(f.toRead) == 0) {
                return (n, error.As(f.err)!);
            }
            return (n, error.As(null!)!);
        }
        if (f.err != null) {
            return (0, error.As(f.err)!);
        }
        f.step(f);
        if (f.err != null && len(f.toRead) == 0) {
            f.toRead = f.dict.readFlush(); // Flush what's left in case of error
        }
    }
}

private static error Close(this ptr<decompressor> _addr_f) {
    ref decompressor f = ref _addr_f.val;

    if (f.err == io.EOF) {
        return error.As(null!)!;
    }
    return error.As(f.err)!;
}

// RFC 1951 section 3.2.7.
// Compression with dynamic Huffman codes

private static array<nint> codeOrder = new array<nint>(new nint[] { 16, 17, 18, 0, 8, 7, 9, 6, 10, 5, 11, 4, 12, 3, 13, 2, 14, 1, 15 });

private static error readHuffman(this ptr<decompressor> _addr_f) {
    ref decompressor f = ref _addr_f.val;
 
    // HLIT[5], HDIST[5], HCLEN[4].
    while (f.nb < 5 + 5 + 4) {
        {
            var err__prev1 = err;

            var err = f.moreBits();

            if (err != null) {
                return error.As(err)!;
            }

            err = err__prev1;

        }
    }
    var nlit = int(f.b & 0x1F) + 257;
    if (nlit > maxNumLit) {
        return error.As(CorruptInputError(f.roffset))!;
    }
    f.b>>=5;
    var ndist = int(f.b & 0x1F) + 1;
    if (ndist > maxNumDist) {
        return error.As(CorruptInputError(f.roffset))!;
    }
    f.b>>=5;
    var nclen = int(f.b & 0xF) + 4; 
    // numCodes is 19, so nclen is always valid.
    f.b>>=4;
    f.nb -= 5 + 5 + 4; 

    // (HCLEN+4)*3 bits: code lengths in the magic codeOrder order.
    {
        nint i__prev1 = i;

        for (nint i = 0; i < nclen; i++) {
            while (f.nb < 3) {
                {
                    var err__prev1 = err;

                    err = f.moreBits();

                    if (err != null) {
                        return error.As(err)!;
                    }

                    err = err__prev1;

                }
            }

            f.codebits[codeOrder[i]] = int(f.b & 0x7);
            f.b>>=3;
            f.nb -= 3;
        }

        i = i__prev1;
    }
    {
        nint i__prev1 = i;

        for (i = nclen; i < len(codeOrder); i++) {
            f.codebits[codeOrder[i]] = 0;
        }

        i = i__prev1;
    }
    if (!f.h1.init(f.codebits[(int)0..])) {
        return error.As(CorruptInputError(f.roffset))!;
    }
    {
        nint i__prev1 = i;

        i = 0;
        var n = nlit + ndist;

        while (i < n) {
            var (x, err) = f.huffSym(_addr_f.h1);
            if (err != null) {
                return error.As(err)!;
            }
            if (x < 16) { 
                // Actual length.
                f.bits[i] = x;
                i++;
                continue;
            } 
            // Repeat previous length or zero.
            nint rep = default;
            nuint nb = default;
            nint b = default;
            switch (x) {
                case 16: 
                    rep = 3;
                    nb = 2;
                    if (i == 0) {
                        return error.As(CorruptInputError(f.roffset))!;
                    }
                    b = f.bits[i - 1];
                    break;
                case 17: 
                    rep = 3;
                    nb = 3;
                    b = 0;
                    break;
                case 18: 
                    rep = 11;
                    nb = 7;
                    b = 0;
                    break;
                default: 
                    return error.As(InternalError("unexpected length code"))!;
                    break;
            }
            while (f.nb < nb) {
                {
                    var err__prev1 = err;

                    err = f.moreBits();

                    if (err != null) {
                        return error.As(err)!;
                    }

                    err = err__prev1;

                }
            }

            rep += int(f.b & uint32(1 << (int)(nb) - 1));
            f.b>>=nb;
            f.nb -= nb;
            if (i + rep > n) {
                return error.As(CorruptInputError(f.roffset))!;
            }
            for (nint j = 0; j < rep; j++) {
                f.bits[i] = b;
                i++;
            }
        }

        i = i__prev1;
    }

    if (!f.h1.init(f.bits[(int)0..(int)nlit]) || !f.h2.init(f.bits[(int)nlit..(int)nlit + ndist])) {
        return error.As(CorruptInputError(f.roffset))!;
    }
    if (f.h1.min < f.bits[endBlockMarker]) {
        f.h1.min = f.bits[endBlockMarker];
    }
    return error.As(null!)!;
}

// Decode a single Huffman block from f.
// hl and hd are the Huffman states for the lit/length values
// and the distance values, respectively. If hd == nil, using the
// fixed distance encoding associated with fixed Huffman blocks.
private static void huffmanBlock(this ptr<decompressor> _addr_f) {
    ref decompressor f = ref _addr_f.val;

    const var stateInit = iota; // Zero value must be stateInit
    const var stateDict = 0;


    if (f.stepState == stateInit) 
        goto readLiteral;
    else if (f.stepState == stateDict) 
        goto copyHistory;
    readLiteral:

 {
        var (v, err) = f.huffSym(f.hl);
        if (err != null) {
            f.err = err;
            return ;
        }
        nuint n = default; // number of bits extra
        nint length = default;

        if (v < 256) 
            f.dict.writeByte(byte(v));
            if (f.dict.availWrite() == 0) {
                f.toRead = f.dict.readFlush();
                f.step = (decompressor.val).huffmanBlock;
                f.stepState = stateInit;
                return ;
            }
            goto readLiteral;
        else if (v == 256) 
            f.finishBlock();
            return ; 
            // otherwise, reference to older data
        else if (v < 265) 
            length = v - (257 - 3);
            n = 0;
        else if (v < 269) 
            length = v * 2 - (265 * 2 - 11);
            n = 1;
        else if (v < 273) 
            length = v * 4 - (269 * 4 - 19);
            n = 2;
        else if (v < 277) 
            length = v * 8 - (273 * 8 - 35);
            n = 3;
        else if (v < 281) 
            length = v * 16 - (277 * 16 - 67);
            n = 4;
        else if (v < 285) 
            length = v * 32 - (281 * 32 - 131);
            n = 5;
        else if (v < maxNumLit) 
            length = 258;
            n = 0;
        else 
            f.err = CorruptInputError(f.roffset);
            return ;
                if (n > 0) {
            while (f.nb < n) {
                err = f.moreBits();

                if (err != null) {
                    f.err = err;
                    return ;
                }
            }

            length += int(f.b & uint32(1 << (int)(n) - 1));
            f.b>>=n;
            f.nb -= n;
        }
        nint dist = default;
        if (f.hd == null) {
            while (f.nb < 5) {
                err = f.moreBits();

                if (err != null) {
                    f.err = err;
                    return ;
                }
            }
        else

            dist = int(bits.Reverse8(uint8(f.b & 0x1F << 3)));
            f.b>>=5;
            f.nb -= 5;
        } {
            dist, err = f.huffSym(f.hd);

            if (err != null) {
                f.err = err;
                return ;
            }
        }

        if (dist < 4) 
            dist++;
        else if (dist < maxNumDist) 
            var nb = uint(dist - 2) >> 1; 
            // have 1 bit in bottom of dist, need nb more.
            var extra = (dist & 1) << (int)(nb);
            while (f.nb < nb) {
                err = f.moreBits();

                if (err != null) {
                    f.err = err;
                    return ;
                }
            }

            extra |= int(f.b & uint32(1 << (int)(nb) - 1));
            f.b>>=nb;
            f.nb -= nb;
            dist = 1 << (int)((nb + 1)) + 1 + extra;
        else 
            f.err = CorruptInputError(f.roffset);
            return ;
        // No check on length; encoding can be prescient.
        if (dist > f.dict.histSize()) {
            f.err = CorruptInputError(f.roffset);
            return ;
        }
        (f.copyLen, f.copyDist) = (length, dist);        goto copyHistory;
    }copyHistory:
 {
        var cnt = f.dict.tryWriteCopy(f.copyDist, f.copyLen);
        if (cnt == 0) {
            cnt = f.dict.writeCopy(f.copyDist, f.copyLen);
        }
        f.copyLen -= cnt;

        if (f.dict.availWrite() == 0 || f.copyLen > 0) {
            f.toRead = f.dict.readFlush();
            f.step = (decompressor.val).huffmanBlock; // We need to continue this work
            f.stepState = stateDict;
            return ;
        }
        goto readLiteral;
    }
}

// Copy a single uncompressed data block from input to output.
private static void dataBlock(this ptr<decompressor> _addr_f) {
    ref decompressor f = ref _addr_f.val;
 
    // Uncompressed.
    // Discard current half-byte.
    f.nb = 0;
    f.b = 0; 

    // Length then ones-complement of length.
    var (nr, err) = io.ReadFull(f.r, f.buf[(int)0..(int)4]);
    f.roffset += int64(nr);
    if (err != null) {
        f.err = noEOF(err);
        return ;
    }
    var n = int(f.buf[0]) | int(f.buf[1]) << 8;
    var nn = int(f.buf[2]) | int(f.buf[3]) << 8;
    if (uint16(nn) != uint16(~n)) {
        f.err = CorruptInputError(f.roffset);
        return ;
    }
    if (n == 0) {
        f.toRead = f.dict.readFlush();
        f.finishBlock();
        return ;
    }
    f.copyLen = n;
    f.copyData();
}

// copyData copies f.copyLen bytes from the underlying reader into f.hist.
// It pauses for reads when f.hist is full.
private static void copyData(this ptr<decompressor> _addr_f) {
    ref decompressor f = ref _addr_f.val;

    var buf = f.dict.writeSlice();
    if (len(buf) > f.copyLen) {
        buf = buf[..(int)f.copyLen];
    }
    var (cnt, err) = io.ReadFull(f.r, buf);
    f.roffset += int64(cnt);
    f.copyLen -= cnt;
    f.dict.writeMark(cnt);
    if (err != null) {
        f.err = noEOF(err);
        return ;
    }
    if (f.dict.availWrite() == 0 || f.copyLen > 0) {
        f.toRead = f.dict.readFlush();
        f.step = (decompressor.val).copyData;
        return ;
    }
    f.finishBlock();
}

private static void finishBlock(this ptr<decompressor> _addr_f) {
    ref decompressor f = ref _addr_f.val;

    if (f.final) {
        if (f.dict.availRead() > 0) {
            f.toRead = f.dict.readFlush();
        }
        f.err = io.EOF;
    }
    f.step = (decompressor.val).nextBlock;
}

// noEOF returns err, unless err == io.EOF, in which case it returns io.ErrUnexpectedEOF.
private static error noEOF(error e) {
    if (e == io.EOF) {
        return error.As(io.ErrUnexpectedEOF)!;
    }
    return error.As(e)!;
}

private static error moreBits(this ptr<decompressor> _addr_f) {
    ref decompressor f = ref _addr_f.val;

    var (c, err) = f.r.ReadByte();
    if (err != null) {
        return error.As(noEOF(err))!;
    }
    f.roffset++;
    f.b |= uint32(c) << (int)(f.nb);
    f.nb += 8;
    return error.As(null!)!;
}

// Read the next Huffman-encoded symbol from f according to h.
private static (nint, error) huffSym(this ptr<decompressor> _addr_f, ptr<huffmanDecoder> _addr_h) {
    nint _p0 = default;
    error _p0 = default!;
    ref decompressor f = ref _addr_f.val;
    ref huffmanDecoder h = ref _addr_h.val;
 
    // Since a huffmanDecoder can be empty or be composed of a degenerate tree
    // with single element, huffSym must error on these two edge cases. In both
    // cases, the chunks slice will be 0 for the invalid sequence, leading it
    // satisfy the n == 0 check below.
    var n = uint(h.min); 
    // Optimization. Compiler isn't smart enough to keep f.b,f.nb in registers,
    // but is smart enough to keep local variables in registers, so use nb and b,
    // inline call to moreBits and reassign b,nb back to f on return.
    var nb = f.nb;
    var b = f.b;
    while (true) {
        while (nb < n) {
            var (c, err) = f.r.ReadByte();
            if (err != null) {
                f.b = b;
                f.nb = nb;
                return (0, error.As(noEOF(err))!);
            }
            f.roffset++;
            b |= uint32(c) << (int)((nb & 31));
            nb += 8;
        }
        var chunk = h.chunks[b & (huffmanNumChunks - 1)];
        n = uint(chunk & huffmanCountMask);
        if (n > huffmanChunkBits) {
            chunk = h.links[chunk >> (int)(huffmanValueShift)][(b >> (int)(huffmanChunkBits)) & h.linkMask];
            n = uint(chunk & huffmanCountMask);
        }
        if (n <= nb) {
            if (n == 0) {
                f.b = b;
                f.nb = nb;
                f.err = CorruptInputError(f.roffset);
                return (0, error.As(f.err)!);
            }
            f.b = b >> (int)((n & 31));
            f.nb = nb - n;
            return (int(chunk >> (int)(huffmanValueShift)), error.As(null!)!);
        }
    }
}

private static Reader makeReader(io.Reader r) {
    {
        Reader (rr, ok) = Reader.As(r._<Reader>())!;

        if (ok) {
            return rr;
        }
    }
    return bufio.NewReader(r);
}

private static void fixedHuffmanDecoderInit() {
    fixedOnce.Do(() => { 
        // These come from the RFC section 3.2.6.
        array<nint> bits = new array<nint>(288);
        {
            nint i__prev1 = i;

            for (nint i = 0; i < 144; i++) {
                bits[i] = 8;
            }


            i = i__prev1;
        }
        {
            nint i__prev1 = i;

            for (i = 144; i < 256; i++) {
                bits[i] = 9;
            }


            i = i__prev1;
        }
        {
            nint i__prev1 = i;

            for (i = 256; i < 280; i++) {
                bits[i] = 7;
            }


            i = i__prev1;
        }
        {
            nint i__prev1 = i;

            for (i = 280; i < 288; i++) {
                bits[i] = 8;
            }


            i = i__prev1;
        }
        fixedHuffmanDecoder.init(bits[..]);
    });
}

private static error Reset(this ptr<decompressor> _addr_f, io.Reader r, slice<byte> dict) {
    ref decompressor f = ref _addr_f.val;

    f.val = new decompressor(r:makeReader(r),bits:f.bits,codebits:f.codebits,dict:f.dict,step:(*decompressor).nextBlock,);
    f.dict.init(maxMatchOffset, dict);
    return error.As(null!)!;
}

// NewReader returns a new ReadCloser that can be used
// to read the uncompressed version of r.
// If r does not also implement io.ByteReader,
// the decompressor may read more data than necessary from r.
// It is the caller's responsibility to call Close on the ReadCloser
// when finished reading.
//
// The ReadCloser returned by NewReader also implements Resetter.
public static io.ReadCloser NewReader(io.Reader r) {
    fixedHuffmanDecoderInit();

    ref decompressor f = ref heap(out ptr<decompressor> _addr_f);
    f.r = makeReader(r);
    f.bits = @new<[maxNumLit+maxNumDist]int>();
    f.codebits = @new<[numCodes]int>();
    f.step = (decompressor.val).nextBlock;
    f.dict.init(maxMatchOffset, null);
    return _addr_f;
}

// NewReaderDict is like NewReader but initializes the reader
// with a preset dictionary. The returned Reader behaves as if
// the uncompressed data stream started with the given dictionary,
// which has already been read. NewReaderDict is typically used
// to read data compressed by NewWriterDict.
//
// The ReadCloser returned by NewReader also implements Resetter.
public static io.ReadCloser NewReaderDict(io.Reader r, slice<byte> dict) {
    fixedHuffmanDecoderInit();

    ref decompressor f = ref heap(out ptr<decompressor> _addr_f);
    f.r = makeReader(r);
    f.bits = @new<[maxNumLit+maxNumDist]int>();
    f.codebits = @new<[numCodes]int>();
    f.step = (decompressor.val).nextBlock;
    f.dict.init(maxMatchOffset, dict);
    return _addr_f;
}

} // end flate_package
