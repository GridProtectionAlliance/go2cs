// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package bzip2 implements bzip2 decompression.

// package bzip2 -- go2cs converted at 2022 March 13 06:43:20 UTC
// import "compress/bzip2" ==> using bzip2 = go.compress.bzip2_package
// Original source: C:\Program Files\Go\src\compress\bzip2\bzip2.go
namespace go.compress;

using io = io_package;

public static partial class bzip2_package {

// There's no RFC for bzip2. I used the Wikipedia page for reference and a lot
// of guessing: https://en.wikipedia.org/wiki/Bzip2
// The source code to pyflate was useful for debugging:
// http://www.paul.sladen.org/projects/pyflate

// A StructuralError is returned when the bzip2 data is found to be
// syntactically invalid.
public partial struct StructuralError { // : @string
}

public static @string Error(this StructuralError s) {
    return "bzip2 data invalid: " + string(s);
}

// A reader decompresses bzip2 compressed data.
private partial struct reader {
    public bitReader br;
    public uint fileCRC;
    public uint blockCRC;
    public uint wantBlockCRC;
    public bool setupDone; // true if we have parsed the bzip2 header.
    public nint blockSize; // blockSize in bytes, i.e. 900 * 1000.
    public bool eof;
    public array<nuint> c; // the ``C'' array for the inverse BWT.
    public slice<uint> tt; // mirrors the ``tt'' array in the bzip2 source and contains the P array in the upper 24 bits.
    public uint tPos; // Index of the next output byte in tt.

    public slice<uint> preRLE; // contains the RLE data still to be processed.
    public nint preRLEUsed; // number of entries of preRLE used.
    public nint lastByte; // the last byte value seen.
    public nuint byteRepeats; // the number of repeats of lastByte seen.
    public nuint repeats; // the number of copies of lastByte to output.
}

// NewReader returns an io.Reader which decompresses bzip2 data from r.
// If r does not also implement io.ByteReader,
// the decompressor may read more data than necessary from r.
public static io.Reader NewReader(io.Reader r) {
    ptr<reader> bz2 = @new<reader>();
    bz2.br = newBitReader(r);
    return bz2;
}

private static readonly nuint bzip2FileMagic = 0x425a; // "BZ"
 // "BZ"
private static readonly nuint bzip2BlockMagic = 0x314159265359;

private static readonly nuint bzip2FinalMagic = 0x177245385090;

// setup parses the bzip2 header.


// setup parses the bzip2 header.
private static error setup(this ptr<reader> _addr_bz2, bool needMagic) {
    ref reader bz2 = ref _addr_bz2.val;

    var br = _addr_bz2.br;

    if (needMagic) {
        var magic = br.ReadBits(16);
        if (magic != bzip2FileMagic) {
            return error.As(StructuralError("bad magic value"))!;
        }
    }
    var t = br.ReadBits(8);
    if (t != 'h') {
        return error.As(StructuralError("non-Huffman entropy encoding"))!;
    }
    var level = br.ReadBits(8);
    if (level < '1' || level > '9') {
        return error.As(StructuralError("invalid compression level"))!;
    }
    bz2.fileCRC = 0;
    bz2.blockSize = 100 * 1000 * (level - '0');
    if (bz2.blockSize > len(bz2.tt)) {
        bz2.tt = make_slice<uint>(bz2.blockSize);
    }
    return error.As(null!)!;
}

private static (nint, error) Read(this ptr<reader> _addr_bz2, slice<byte> buf) {
    nint n = default;
    error err = default!;
    ref reader bz2 = ref _addr_bz2.val;

    if (bz2.eof) {
        return (0, error.As(io.EOF)!);
    }
    if (!bz2.setupDone) {
        err = bz2.setup(true);
        var brErr = bz2.br.Err();
        if (brErr != null) {
            err = brErr;
        }
        if (err != null) {
            return (0, error.As(err)!);
        }
        bz2.setupDone = true;
    }
    n, err = bz2.read(buf);
    brErr = bz2.br.Err();
    if (brErr != null) {
        err = brErr;
    }
    return ;
}

private static nint readFromBlock(this ptr<reader> _addr_bz2, slice<byte> buf) {
    ref reader bz2 = ref _addr_bz2.val;
 
    // bzip2 is a block based compressor, except that it has a run-length
    // preprocessing step. The block based nature means that we can
    // preallocate fixed-size buffers and reuse them. However, the RLE
    // preprocessing would require allocating huge buffers to store the
    // maximum expansion. Thus we process blocks all at once, except for
    // the RLE which we decompress as required.
    nint n = 0;
    while ((bz2.repeats > 0 || bz2.preRLEUsed < len(bz2.preRLE)) && n < len(buf)) { 
        // We have RLE data pending.

        // The run-length encoding works like this:
        // Any sequence of four equal bytes is followed by a length
        // byte which contains the number of repeats of that byte to
        // include. (The number of repeats can be zero.) Because we are
        // decompressing on-demand our state is kept in the reader
        // object.

        if (bz2.repeats > 0) {
            buf[n] = byte(bz2.lastByte);
            n++;
            bz2.repeats--;
            if (bz2.repeats == 0) {
                bz2.lastByte = -1;
            }
            continue;
        }
        bz2.tPos = bz2.preRLE[bz2.tPos];
        var b = byte(bz2.tPos);
        bz2.tPos>>=8;
        bz2.preRLEUsed++;

        if (bz2.byteRepeats == 3) {
            bz2.repeats = uint(b);
            bz2.byteRepeats = 0;
            continue;
        }
        if (bz2.lastByte == int(b)) {
            bz2.byteRepeats++;
        }
        else
 {
            bz2.byteRepeats = 0;
        }
        bz2.lastByte = int(b);

        buf[n] = b;
        n++;
    }

    return n;
}

private static (nint, error) read(this ptr<reader> _addr_bz2, slice<byte> buf) {
    nint _p0 = default;
    error _p0 = default!;
    ref reader bz2 = ref _addr_bz2.val;

    while (true) {
        var n = bz2.readFromBlock(buf);
        if (n > 0 || len(buf) == 0) {
            bz2.blockCRC = updateCRC(bz2.blockCRC, buf[..(int)n]);
            return (n, error.As(null!)!);
        }
        if (bz2.blockCRC != bz2.wantBlockCRC) {
            bz2.br.err = StructuralError("block checksum mismatch");
            return (0, error.As(bz2.br.err)!);
        }
        var br = _addr_bz2.br;

        if (br.ReadBits64(48) == bzip2BlockMagic) 
            // Start of block.
            var err = bz2.readBlock();
            if (err != null) {
                return (0, error.As(err)!);
            }
        else if (br.ReadBits64(48) == bzip2FinalMagic) 
            // Check end-of-file CRC.
            var wantFileCRC = uint32(br.ReadBits64(32));
            if (br.err != null) {
                return (0, error.As(br.err)!);
            }
            if (bz2.fileCRC != wantFileCRC) {
                br.err = StructuralError("file checksum mismatch");
                return (0, error.As(br.err)!);
            } 

            // Skip ahead to byte boundary.
            // Is there a file concatenated to this one?
            // It would start with BZ.
            if (br.bits % 8 != 0) {
                br.ReadBits(br.bits % 8);
            }
            var (b, err) = br.r.ReadByte();
            if (err == io.EOF) {
                br.err = io.EOF;
                bz2.eof = true;
                return (0, error.As(io.EOF)!);
            }
            if (err != null) {
                br.err = err;
                return (0, error.As(err)!);
            }
            var (z, err) = br.r.ReadByte();
            if (err != null) {
                if (err == io.EOF) {
                    err = io.ErrUnexpectedEOF;
                }
                br.err = err;
                return (0, error.As(err)!);
            }
            if (b != 'B' || z != 'Z') {
                return (0, error.As(StructuralError("bad magic value in continuation file"))!);
            }
            {
                var err__prev1 = err;

                err = bz2.setup(false);

                if (err != null) {
                    return (0, error.As(err)!);
                }

                err = err__prev1;

            }
        else 
            return (0, error.As(StructuralError("bad magic value found"))!);
            }
}

// readBlock reads a bzip2 block. The magic number should already have been consumed.
private static error readBlock(this ptr<reader> _addr_bz2) {
    error err = default!;
    ref reader bz2 = ref _addr_bz2.val;

    var br = _addr_bz2.br;
    bz2.wantBlockCRC = uint32(br.ReadBits64(32)); // skip checksum. TODO: check it if we can figure out what it is.
    bz2.blockCRC = 0;
    bz2.fileCRC = (bz2.fileCRC << 1 | bz2.fileCRC >> 31) ^ bz2.wantBlockCRC;
    var randomized = br.ReadBits(1);
    if (randomized != 0) {
        return error.As(StructuralError("deprecated randomized files"))!;
    }
    var origPtr = uint(br.ReadBits(24)); 

    // If not every byte value is used in the block (i.e., it's text) then
    // the symbol set is reduced. The symbols used are stored as a
    // two-level, 16x16 bitmap.
    var symbolRangeUsedBitmap = br.ReadBits(16);
    var symbolPresent = make_slice<bool>(256);
    nint numSymbols = 0;
    for (var symRange = uint(0); symRange < 16; symRange++) {
        if (symbolRangeUsedBitmap & (1 << (int)((15 - symRange))) != 0) {
            var bits = br.ReadBits(16);
            for (var symbol = uint(0); symbol < 16; symbol++) {
                if (bits & (1 << (int)((15 - symbol))) != 0) {
                    symbolPresent[16 * symRange + symbol] = true;
                    numSymbols++;
                }
            }
        }
    }

    if (numSymbols == 0) { 
        // There must be an EOF symbol.
        return error.As(StructuralError("no symbols in input"))!;
    }
    var numHuffmanTrees = br.ReadBits(3);
    if (numHuffmanTrees < 2 || numHuffmanTrees > 6) {
        return error.As(StructuralError("invalid number of Huffman trees"))!;
    }
    var numSelectors = br.ReadBits(15);
    var treeIndexes = make_slice<byte>(numSelectors); 

    // The tree indexes are move-to-front transformed and stored as unary
    // numbers.
    var mtfTreeDecoder = newMTFDecoderWithRange(numHuffmanTrees);
    {
        var i__prev1 = i;

        foreach (var (__i) in treeIndexes) {
            i = __i;
            nint c = 0;
            while (true) {
                var inc = br.ReadBits(1);
                if (inc == 0) {
                    break;
                }
                c++;
            }

            if (c >= numHuffmanTrees) {
                return error.As(StructuralError("tree index too large"))!;
            }
            treeIndexes[i] = mtfTreeDecoder.Decode(c);
        }
        i = i__prev1;
    }

    var symbols = make_slice<byte>(numSymbols);
    nint nextSymbol = 0;
    {
        var i__prev1 = i;

        for (nint i = 0; i < 256; i++) {
            if (symbolPresent[i]) {
                symbols[nextSymbol] = byte(i);
                nextSymbol++;
            }
        }

        i = i__prev1;
    }
    var mtf = newMTFDecoder(symbols);

    numSymbols += 2; // to account for RUNA and RUNB symbols
    var huffmanTrees = make_slice<huffmanTree>(numHuffmanTrees); 

    // Now we decode the arrays of code-lengths for each tree.
    var lengths = make_slice<byte>(numSymbols);
    {
        var i__prev1 = i;

        foreach (var (__i) in huffmanTrees) {
            i = __i; 
            // The code lengths are delta encoded from a 5-bit base value.
            var length = br.ReadBits(5);
            foreach (var (j) in lengths) {
                while (true) {
                    if (length < 1 || length > 20) {
                        return error.As(StructuralError("Huffman length out of range"))!;
                    }
                    if (!br.ReadBit()) {
                        break;
                    }
                    if (br.ReadBit()) {
                        length--;
                    }
                    else
 {
                        length++;
                    }
                }

                lengths[j] = uint8(length);
            }
            huffmanTrees[i], err = newHuffmanTree(lengths);
            if (err != null) {
                return error.As(err)!;
            }
        }
        i = i__prev1;
    }

    nint selectorIndex = 1; // the next tree index to use
    if (len(treeIndexes) == 0) {
        return error.As(StructuralError("no tree selectors given"))!;
    }
    if (int(treeIndexes[0]) >= len(huffmanTrees)) {
        return error.As(StructuralError("tree selector out of range"))!;
    }
    var currentHuffmanTree = huffmanTrees[treeIndexes[0]];
    nint bufIndex = 0; // indexes bz2.buf, the output buffer.
    // The output of the move-to-front transform is run-length encoded and
    // we merge the decoding into the Huffman parsing loop. These two
    // variables accumulate the repeat count. See the Wikipedia page for
    // details.
    nint repeat = 0;
    nint repeatPower = 0; 

    // The `C' array (used by the inverse BWT) needs to be zero initialized.
    {
        var i__prev1 = i;

        foreach (var (__i) in bz2.c) {
            i = __i;
            bz2.c[i] = 0;
        }
        i = i__prev1;
    }

    nint decoded = 0; // counts the number of symbols decoded by the current tree.
    while (true) {
        if (decoded == 50) {
            if (selectorIndex >= numSelectors) {
                return error.As(StructuralError("insufficient selector indices for number of symbols"))!;
            }
            if (int(treeIndexes[selectorIndex]) >= len(huffmanTrees)) {
                return error.As(StructuralError("tree selector out of range"))!;
            }
            currentHuffmanTree = huffmanTrees[treeIndexes[selectorIndex]];
            selectorIndex++;
            decoded = 0;
        }
        var v = currentHuffmanTree.Decode(br);
        decoded++;

        if (v < 2) { 
            // This is either the RUNA or RUNB symbol.
            if (repeat == 0) {
                repeatPower = 1;
            }
            repeat += repeatPower << (int)(v);
            repeatPower<<=1; 

            // This limit of 2 million comes from the bzip2 source
            // code. It prevents repeat from overflowing.
            if (repeat > 2 * 1024 * 1024) {
                return error.As(StructuralError("repeat count too large"))!;
            }
            continue;
        }
        if (repeat > 0) { 
            // We have decoded a complete run-length so we need to
            // replicate the last output symbol.
            if (repeat > bz2.blockSize - bufIndex) {
                return error.As(StructuralError("repeats past end of block"))!;
            }
            {
                var i__prev2 = i;

                for (i = 0; i < repeat; i++) {
                    var b = mtf.First();
                    bz2.tt[bufIndex] = uint32(b);
                    bz2.c[b]++;
                    bufIndex++;
                }


                i = i__prev2;
            }
            repeat = 0;
        }
        if (int(v) == numSymbols - 1) { 
            // This is the EOF symbol. Because it's always at the
            // end of the move-to-front list, and never gets moved
            // to the front, it has this unique value.
            break;
        }
        b = mtf.Decode(int(v - 1));
        if (bufIndex >= bz2.blockSize) {
            return error.As(StructuralError("data exceeds block size"))!;
        }
        bz2.tt[bufIndex] = uint32(b);
        bz2.c[b]++;
        bufIndex++;
    }

    if (origPtr >= uint(bufIndex)) {
        return error.As(StructuralError("origPtr out of bounds"))!;
    }
    bz2.preRLE = bz2.tt[..(int)bufIndex];
    bz2.preRLEUsed = 0;
    bz2.tPos = inverseBWT(bz2.preRLE, origPtr, bz2.c[..]);
    bz2.lastByte = -1;
    bz2.byteRepeats = 0;
    bz2.repeats = 0;

    return error.As(null!)!;
}

// inverseBWT implements the inverse Burrows-Wheeler transform as described in
// http://www.hpl.hp.com/techreports/Compaq-DEC/SRC-RR-124.pdf, section 4.2.
// In that document, origPtr is called ``I'' and c is the ``C'' array after the
// first pass over the data. It's an argument here because we merge the first
// pass with the Huffman decoding.
//
// This also implements the ``single array'' method from the bzip2 source code
// which leaves the output, still shuffled, in the bottom 8 bits of tt with the
// index of the next byte in the top 24-bits. The index of the first byte is
// returned.
private static uint inverseBWT(slice<uint> tt, nuint origPtr, slice<nuint> c) {
    var sum = uint(0);
    {
        nint i__prev1 = i;

        for (nint i = 0; i < 256; i++) {
            sum += c[i];
            c[i] = sum - c[i];
        }

        i = i__prev1;
    }

    {
        nint i__prev1 = i;

        foreach (var (__i) in tt) {
            i = __i;
            var b = tt[i] & 0xff;
            tt[c[b]] |= uint32(i) << 8;
            c[b]++;
        }
        i = i__prev1;
    }

    return tt[origPtr] >> 8;
}

// This is a standard CRC32 like in hash/crc32 except that all the shifts are reversed,
// causing the bits in the input to be processed in the reverse of the usual order.

private static array<uint> crctab = new array<uint>(256);

private static void init() {
    const nuint poly = 0x04C11DB7;

    foreach (var (i) in crctab) {
        var crc = uint32(i) << 24;
        for (nint j = 0; j < 8; j++) {
            if (crc & 0x80000000 != 0) {
                crc = (crc << 1) ^ poly;
            }
            else
 {
                crc<<=1;
            }
        }
        crctab[i] = crc;
    }
}

// updateCRC updates the crc value to incorporate the data in b.
// The initial value is 0.
private static uint updateCRC(uint val, slice<byte> b) {
    var crc = ~val;
    foreach (var (_, v) in b) {
        crc = crctab[byte(crc >> 24) ^ v] ^ (crc << 8);
    }    return ~crc;
}

} // end bzip2_package
