// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package flate implements the DEFLATE compressed data format, described in
// RFC 1951.  The gzip and zlib packages implement access to DEFLATE-based file
// formats.
// package flate -- go2cs converted at 2020 August 29 08:23:35 UTC
// import "compress/flate" ==> using flate = go.compress.flate_package
// Original source: C:\Go\src\compress\flate\inflate.go
using bufio = go.bufio_package;
using io = go.io_package;
using mathbits = go.math.bits_package;
using strconv = go.strconv_package;
using sync = go.sync_package;
using static go.builtin;
using System;

namespace go {
namespace compress
{
    public static partial class flate_package
    {
        private static readonly long maxCodeLen = 16L; // max length of Huffman code
        // The next three numbers come from the RFC section 3.2.7, with the
        // additional proviso in section 3.2.5 which implies that distance codes
        // 30 and 31 should never occur in compressed data.
        private static readonly long maxNumLit = 286L;
        private static readonly long maxNumDist = 30L;
        private static readonly long numCodes = 19L; // number of codes in Huffman meta-code

        // Initialize the fixedHuffmanDecoder only once upon first use.
        private static sync.Once fixedOnce = default;
        private static huffmanDecoder fixedHuffmanDecoder = default;

        // A CorruptInputError reports the presence of corrupt input at a given offset.
        public partial struct CorruptInputError // : long
        {
        }

        public static @string Error(this CorruptInputError e)
        {
            return "flate: corrupt input before offset " + strconv.FormatInt(int64(e), 10L);
        }

        // An InternalError reports an error in the flate code itself.
        public partial struct InternalError // : @string
        {
        }

        public static @string Error(this InternalError e)
        {
            return "flate: internal error: " + string(e);
        }

        // A ReadError reports an error encountered while reading input.
        //
        // Deprecated: No longer returned.
        public partial struct ReadError
        {
            public long Offset; // byte offset where error occurred
            public error Err; // error returned by underlying Read
        }

        private static @string Error(this ref ReadError e)
        {
            return "flate: read error at offset " + strconv.FormatInt(e.Offset, 10L) + ": " + e.Err.Error();
        }

        // A WriteError reports an error encountered while writing output.
        //
        // Deprecated: No longer returned.
        public partial struct WriteError
        {
            public long Offset; // byte offset where error occurred
            public error Err; // error returned by underlying Write
        }

        private static @string Error(this ref WriteError e)
        {
            return "flate: write error at offset " + strconv.FormatInt(e.Offset, 10L) + ": " + e.Err.Error();
        }

        // Resetter resets a ReadCloser returned by NewReader or NewReaderDict to
        // to switch to a new underlying Reader. This permits reusing a ReadCloser
        // instead of allocating a new one.
        public partial interface Resetter
        {
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
        //    http://www.gzip.org/algorithm.txt

        // chunk & 15 is number of bits
        // chunk >> 4 is value, including table link

        private static readonly long huffmanChunkBits = 9L;
        private static readonly long huffmanNumChunks = 1L << (int)(huffmanChunkBits);
        private static readonly long huffmanCountMask = 15L;
        private static readonly long huffmanValueShift = 4L;

        private partial struct huffmanDecoder
        {
            public long min; // the minimum code length
            public array<uint> chunks; // chunks as described above
            public slice<slice<uint>> links; // overflow links
            public uint linkMask; // mask the width of the link table
        }

        // Initialize Huffman decoding tables from array of code lengths.
        // Following this function, h is guaranteed to be initialized into a complete
        // tree (i.e., neither over-subscribed nor under-subscribed). The exception is a
        // degenerate case where the tree has only a single symbol with length 1. Empty
        // trees are permitted.
        private static bool init(this ref huffmanDecoder _h, slice<long> bits) => func(_h, (ref huffmanDecoder h, Defer _, Panic panic, Recover __) =>
        { 
            // Sanity enables additional runtime tests during Huffman
            // table construction. It's intended to be used during
            // development to supplement the currently ad-hoc unit tests.
            const var sanity = false;



            if (h.min != 0L)
            {
                h.Value = new huffmanDecoder();
            } 

            // Count number of codes of each length,
            // compute min and max length.
            array<long> count = new array<long>(maxCodeLen);
            long min = default;            long max = default;

            {
                var n__prev1 = n;

                foreach (var (_, __n) in bits)
                {
                    n = __n;
                    if (n == 0L)
                    {
                        continue;
                    }
                    if (min == 0L || n < min)
                    {
                        min = n;
                    }
                    if (n > max)
                    {
                        max = n;
                    }
                    count[n]++;
                } 

                // Empty tree. The decompressor.huffSym function will fail later if the tree
                // is used. Technically, an empty tree is only valid for the HDIST tree and
                // not the HCLEN and HLIT tree. However, a stream with an empty HCLEN tree
                // is guaranteed to fail since it will attempt to use the tree to decode the
                // codes for the HLIT and HDIST trees. Similarly, an empty HLIT tree is
                // guaranteed to fail later since the compressed data section must be
                // composed of at least one symbol (the end-of-block marker).

                n = n__prev1;
            }

            if (max == 0L)
            {
                return true;
            }
            long code = 0L;
            array<long> nextcode = new array<long>(maxCodeLen);
            {
                var i__prev1 = i;

                for (var i = min; i <= max; i++)
                {
                    code <<= 1L;
                    nextcode[i] = code;
                    code += count[i];
                } 

                // Check that the coding is complete (i.e., that we've
                // assigned all 2-to-the-max possible bit sequences).
                // Exception: To be compatible with zlib, we also need to
                // accept degenerate single-code codings. See also
                // TestDegenerateHuffmanCoding.


                i = i__prev1;
            } 

            // Check that the coding is complete (i.e., that we've
            // assigned all 2-to-the-max possible bit sequences).
            // Exception: To be compatible with zlib, we also need to
            // accept degenerate single-code codings. See also
            // TestDegenerateHuffmanCoding.
            if (code != 1L << (int)(uint(max)) && !(code == 1L && max == 1L))
            {
                return false;
            }
            h.min = min;
            if (max > huffmanChunkBits)
            {
                long numLinks = 1L << (int)((uint(max) - huffmanChunkBits));
                h.linkMask = uint32(numLinks - 1L); 

                // create link tables
                var link = nextcode[huffmanChunkBits + 1L] >> (int)(1L);
                h.links = make_slice<slice<uint>>(huffmanNumChunks - link);
                {
                    var j__prev1 = j;

                    for (var j = uint(link); j < huffmanNumChunks; j++)
                    {
                        var reverse = int(mathbits.Reverse16(uint16(j)));
                        reverse >>= uint(16L - huffmanChunkBits);
                        var off = j - uint(link);
                        if (sanity && h.chunks[reverse] != 0L)
                        {
                            panic("impossible: overwriting existing chunk");
                        }
                        h.chunks[reverse] = uint32(off << (int)(huffmanValueShift) | (huffmanChunkBits + 1L));
                        h.links[off] = make_slice<uint>(numLinks);
                    }


                    j = j__prev1;
                }
            }
            {
                var i__prev1 = i;
                var n__prev1 = n;

                foreach (var (__i, __n) in bits)
                {
                    i = __i;
                    n = __n;
                    if (n == 0L)
                    {
                        continue;
                    }
                    code = nextcode[n];
                    nextcode[n]++;
                    var chunk = uint32(i << (int)(huffmanValueShift) | n);
                    reverse = int(mathbits.Reverse16(uint16(code)));
                    reverse >>= uint(16L - n);
                    if (n <= huffmanChunkBits)
                    {
                        {
                            var off__prev2 = off;

                            off = reverse;

                            while (off < len(h.chunks))
                            { 
                                // We should never need to overwrite
                                // an existing chunk. Also, 0 is
                                // never a valid chunk, because the
                                // lower 4 "count" bits should be
                                // between 1 and 15.
                                if (sanity && h.chunks[off] != 0L)
                                {
                                    panic("impossible: overwriting existing chunk");
                                off += 1L << (int)(uint(n));
                                }
                                h.chunks[off] = chunk;
                            }
                    else


                            off = off__prev2;
                        }
                    }                    {
                        j = reverse & (huffmanNumChunks - 1L);
                        if (sanity && h.chunks[j] & huffmanCountMask != huffmanChunkBits + 1L)
                        { 
                            // Longer codes should have been
                            // associated with a link table above.
                            panic("impossible: not an indirect chunk");
                        }
                        var value = h.chunks[j] >> (int)(huffmanValueShift);
                        var linktab = h.links[value];
                        reverse >>= huffmanChunkBits;
                        {
                            var off__prev2 = off;

                            off = reverse;

                            while (off < len(linktab))
                            {
                                if (sanity && linktab[off] != 0L)
                                {
                                    panic("impossible: overwriting existing chunk");
                                off += 1L << (int)(uint(n - huffmanChunkBits));
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

            if (sanity)
            { 
                // Above we've sanity checked that we never overwrote
                // an existing entry. Here we additionally check that
                // we filled the tables completely.
                {
                    var i__prev1 = i;
                    var chunk__prev1 = chunk;

                    foreach (var (__i, __chunk) in h.chunks)
                    {
                        i = __i;
                        chunk = __chunk;
                        if (chunk == 0L)
                        { 
                            // As an exception, in the degenerate
                            // single-code case, we allow odd
                            // chunks to be missing.
                            if (code == 1L && i % 2L == 1L)
                            {
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

                    foreach (var (_, __linktab) in h.links)
                    {
                        linktab = __linktab;
                        {
                            var chunk__prev2 = chunk;

                            foreach (var (_, __chunk) in linktab)
                            {
                                chunk = __chunk;
                                if (chunk == 0L)
                                {
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
        public partial interface Reader : io.Reader, io.ByteReader
        {
        }

        // Decompress state.
        private partial struct decompressor
        {
            public Reader r;
            public long roffset; // Input bits, in top of b.
            public uint b;
            public ulong nb; // Huffman decoders for literal/length, distance.
            public huffmanDecoder h1; // Length arrays used to define Huffman codes.
            public huffmanDecoder h2; // Length arrays used to define Huffman codes.
            public ptr<array<long>> bits;
            public ptr<array<long>> codebits; // Output history, buffer.
            public dictDecoder dict; // Temporary buffer (avoids repeated allocation).
            public array<byte> buf; // Next step in the decompression,
// and decompression state.
            public Action<ref decompressor> step;
            public long stepState;
            public bool final;
            public error err;
            public slice<byte> toRead;
            public ptr<huffmanDecoder> hl;
            public ptr<huffmanDecoder> hd;
            public long copyLen;
            public long copyDist;
        }

        private static void nextBlock(this ref decompressor f)
        {
            while (f.nb < 1L + 2L)
            {
                f.err = f.moreBits();

                if (f.err != null)
                {
                    return;
                }
            }

            f.final = f.b & 1L == 1L;
            f.b >>= 1L;
            var typ = f.b & 3L;
            f.b >>= 2L;
            f.nb -= 1L + 2L;
            switch (typ)
            {
                case 0L: 
                    f.dataBlock();
                    break;
                case 1L: 
                    // compressed, fixed Huffman tables
                    f.hl = ref fixedHuffmanDecoder;
                    f.hd = null;
                    f.huffmanBlock();
                    break;
                case 2L: 
                    // compressed, dynamic Huffman tables
                    f.err = f.readHuffman();

                    if (f.err != null)
                    {
                        break;
                    }
                    f.hl = ref f.h1;
                    f.hd = ref f.h2;
                    f.huffmanBlock();
                    break;
                default: 
                    // 3 is reserved.
                    f.err = CorruptInputError(f.roffset);
                    break;
            }
        }

        private static (long, error) Read(this ref decompressor f, slice<byte> b)
        {
            while (true)
            {
                if (len(f.toRead) > 0L)
                {
                    var n = copy(b, f.toRead);
                    f.toRead = f.toRead[n..];
                    if (len(f.toRead) == 0L)
                    {
                        return (n, f.err);
                    }
                    return (n, null);
                }
                if (f.err != null)
                {
                    return (0L, f.err);
                }
                f.step(f);
                if (f.err != null && len(f.toRead) == 0L)
                {
                    f.toRead = f.dict.readFlush(); // Flush what's left in case of error
                }
            }

        }

        private static error Close(this ref decompressor f)
        {
            if (f.err == io.EOF)
            {
                return error.As(null);
            }
            return error.As(f.err);
        }

        // RFC 1951 section 3.2.7.
        // Compression with dynamic Huffman codes

        private static array<long> codeOrder = new array<long>(new long[] { 16, 17, 18, 0, 8, 7, 9, 6, 10, 5, 11, 4, 12, 3, 13, 2, 14, 1, 15 });

        private static error readHuffman(this ref decompressor f)
        { 
            // HLIT[5], HDIST[5], HCLEN[4].
            while (f.nb < 5L + 5L + 4L)
            {
                {
                    var err__prev1 = err;

                    var err = f.moreBits();

                    if (err != null)
                    {
                        return error.As(err);
                    }

                    err = err__prev1;

                }
            }

            var nlit = int(f.b & 0x1FUL) + 257L;
            if (nlit > maxNumLit)
            {
                return error.As(CorruptInputError(f.roffset));
            }
            f.b >>= 5L;
            var ndist = int(f.b & 0x1FUL) + 1L;
            if (ndist > maxNumDist)
            {
                return error.As(CorruptInputError(f.roffset));
            }
            f.b >>= 5L;
            var nclen = int(f.b & 0xFUL) + 4L; 
            // numCodes is 19, so nclen is always valid.
            f.b >>= 4L;
            f.nb -= 5L + 5L + 4L; 

            // (HCLEN+4)*3 bits: code lengths in the magic codeOrder order.
            {
                long i__prev1 = i;

                for (long i = 0L; i < nclen; i++)
                {
                    while (f.nb < 3L)
                    {
                        {
                            var err__prev1 = err;

                            err = f.moreBits();

                            if (err != null)
                            {
                                return error.As(err);
                            }

                            err = err__prev1;

                        }
                    }

                    f.codebits[codeOrder[i]] = int(f.b & 0x7UL);
                    f.b >>= 3L;
                    f.nb -= 3L;
                }


                i = i__prev1;
            }
            {
                long i__prev1 = i;

                for (i = nclen; i < len(codeOrder); i++)
                {
                    f.codebits[codeOrder[i]] = 0L;
                }


                i = i__prev1;
            }
            if (!f.h1.init(f.codebits[0L..]))
            {
                return error.As(CorruptInputError(f.roffset));
            } 

            // HLIT + 257 code lengths, HDIST + 1 code lengths,
            // using the code length Huffman code.
            {
                long i__prev1 = i;

                i = 0L;
                var n = nlit + ndist;

                while (i < n)
                {
                    var (x, err) = f.huffSym(ref f.h1);
                    if (err != null)
                    {
                        return error.As(err);
                    }
                    if (x < 16L)
                    { 
                        // Actual length.
                        f.bits[i] = x;
                        i++;
                        continue;
                    } 
                    // Repeat previous length or zero.
                    long rep = default;
                    ulong nb = default;
                    long b = default;
                    switch (x)
                    {
                        case 16L: 
                            rep = 3L;
                            nb = 2L;
                            if (i == 0L)
                            {
                                return error.As(CorruptInputError(f.roffset));
                            }
                            b = f.bits[i - 1L];
                            break;
                        case 17L: 
                            rep = 3L;
                            nb = 3L;
                            b = 0L;
                            break;
                        case 18L: 
                            rep = 11L;
                            nb = 7L;
                            b = 0L;
                            break;
                        default: 
                            return error.As(InternalError("unexpected length code"));
                            break;
                    }
                    while (f.nb < nb)
                    {
                        {
                            var err__prev1 = err;

                            err = f.moreBits();

                            if (err != null)
                            {
                                return error.As(err);
                            }

                            err = err__prev1;

                        }
                    }

                    rep += int(f.b & uint32(1L << (int)(nb) - 1L));
                    f.b >>= nb;
                    f.nb -= nb;
                    if (i + rep > n)
                    {
                        return error.As(CorruptInputError(f.roffset));
                    }
                    for (long j = 0L; j < rep; j++)
                    {
                        f.bits[i] = b;
                        i++;
                    }

                }


                i = i__prev1;
            }

            if (!f.h1.init(f.bits[0L..nlit]) || !f.h2.init(f.bits[nlit..nlit + ndist]))
            {
                return error.As(CorruptInputError(f.roffset));
            } 

            // As an optimization, we can initialize the min bits to read at a time
            // for the HLIT tree to the length of the EOB marker since we know that
            // every block must terminate with one. This preserves the property that
            // we never read any extra bytes after the end of the DEFLATE stream.
            if (f.h1.min < f.bits[endBlockMarker])
            {
                f.h1.min = f.bits[endBlockMarker];
            }
            return error.As(null);
        }

        // Decode a single Huffman block from f.
        // hl and hd are the Huffman states for the lit/length values
        // and the distance values, respectively. If hd == nil, using the
        // fixed distance encoding associated with fixed Huffman blocks.
        private static void huffmanBlock(this ref decompressor f)
        {
            const var stateInit = iota; // Zero value must be stateInit
            const var stateDict = 0;


            if (f.stepState == stateInit) 
                goto readLiteral;
            else if (f.stepState == stateDict) 
                goto copyHistory;
            readLiteral:

            {
                var (v, err) = f.huffSym(f.hl);
                if (err != null)
                {
                    f.err = err;
                    return;
                }
                ulong n = default; // number of bits extra
                long length = default;

                if (v < 256L) 
                    f.dict.writeByte(byte(v));
                    if (f.dict.availWrite() == 0L)
                    {
                        f.toRead = f.dict.readFlush();
                        f.step = ref decompressor;
                        f.stepState = stateInit;
                        return;
                    }
                    goto readLiteral;
                else if (v == 256L) 
                    f.finishBlock();
                    return; 
                    // otherwise, reference to older data
                else if (v < 265L) 
                    length = v - (257L - 3L);
                    n = 0L;
                else if (v < 269L) 
                    length = v * 2L - (265L * 2L - 11L);
                    n = 1L;
                else if (v < 273L) 
                    length = v * 4L - (269L * 4L - 19L);
                    n = 2L;
                else if (v < 277L) 
                    length = v * 8L - (273L * 8L - 35L);
                    n = 3L;
                else if (v < 281L) 
                    length = v * 16L - (277L * 16L - 67L);
                    n = 4L;
                else if (v < 285L) 
                    length = v * 32L - (281L * 32L - 131L);
                    n = 5L;
                else if (v < maxNumLit) 
                    length = 258L;
                    n = 0L;
                else 
                    f.err = CorruptInputError(f.roffset);
                    return;
                                if (n > 0L)
                {
                    while (f.nb < n)
                    {
                        err = f.moreBits();

                        if (err != null)
                        {
                            f.err = err;
                            return;
                        }
                    }

                    length += int(f.b & uint32(1L << (int)(n) - 1L));
                    f.b >>= n;
                    f.nb -= n;
                }
                long dist = default;
                if (f.hd == null)
                {
                    while (f.nb < 5L)
                    {
                        err = f.moreBits();

                        if (err != null)
                        {
                            f.err = err;
                            return;
                        }
                    }
                else

                    dist = int(mathbits.Reverse8(uint8(f.b & 0x1FUL << (int)(3L))));
                    f.b >>= 5L;
                    f.nb -= 5L;
                }                {
                    dist, err = f.huffSym(f.hd);

                    if (err != null)
                    {
                        f.err = err;
                        return;
                    }
                }

                if (dist < 4L) 
                    dist++;
                else if (dist < maxNumDist) 
                    var nb = uint(dist - 2L) >> (int)(1L); 
                    // have 1 bit in bottom of dist, need nb more.
                    var extra = (dist & 1L) << (int)(nb);
                    while (f.nb < nb)
                    {
                        err = f.moreBits();

                        if (err != null)
                        {
                            f.err = err;
                            return;
                        }
                    }

                    extra |= int(f.b & uint32(1L << (int)(nb) - 1L));
                    f.b >>= nb;
                    f.nb -= nb;
                    dist = 1L << (int)((nb + 1L)) + 1L + extra;
                else 
                    f.err = CorruptInputError(f.roffset);
                    return;
                // No check on length; encoding can be prescient.
                if (dist > f.dict.histSize())
                {
                    f.err = CorruptInputError(f.roffset);
                    return;
                }
                f.copyLen = length;
                f.copyDist = dist;
                goto copyHistory;
            }
copyHistory:
            {
                var cnt = f.dict.tryWriteCopy(f.copyDist, f.copyLen);
                if (cnt == 0L)
                {
                    cnt = f.dict.writeCopy(f.copyDist, f.copyLen);
                }
                f.copyLen -= cnt;

                if (f.dict.availWrite() == 0L || f.copyLen > 0L)
                {
                    f.toRead = f.dict.readFlush();
                    f.step = ref decompressor; // We need to continue this work
                    f.stepState = stateDict;
                    return;
                }
                goto readLiteral;
            }
        }

        // Copy a single uncompressed data block from input to output.
        private static void dataBlock(this ref decompressor f)
        { 
            // Uncompressed.
            // Discard current half-byte.
            f.nb = 0L;
            f.b = 0L; 

            // Length then ones-complement of length.
            var (nr, err) = io.ReadFull(f.r, f.buf[0L..4L]);
            f.roffset += int64(nr);
            if (err != null)
            {
                if (err == io.EOF)
                {
                    err = io.ErrUnexpectedEOF;
                }
                f.err = err;
                return;
            }
            var n = int(f.buf[0L]) | int(f.buf[1L]) << (int)(8L);
            var nn = int(f.buf[2L]) | int(f.buf[3L]) << (int)(8L);
            if (uint16(nn) != uint16(~n))
            {
                f.err = CorruptInputError(f.roffset);
                return;
            }
            if (n == 0L)
            {
                f.toRead = f.dict.readFlush();
                f.finishBlock();
                return;
            }
            f.copyLen = n;
            f.copyData();
        }

        // copyData copies f.copyLen bytes from the underlying reader into f.hist.
        // It pauses for reads when f.hist is full.
        private static void copyData(this ref decompressor f)
        {
            var buf = f.dict.writeSlice();
            if (len(buf) > f.copyLen)
            {
                buf = buf[..f.copyLen];
            }
            var (cnt, err) = io.ReadFull(f.r, buf);
            f.roffset += int64(cnt);
            f.copyLen -= cnt;
            f.dict.writeMark(cnt);
            if (err != null)
            {
                if (err == io.EOF)
                {
                    err = io.ErrUnexpectedEOF;
                }
                f.err = err;
                return;
            }
            if (f.dict.availWrite() == 0L || f.copyLen > 0L)
            {
                f.toRead = f.dict.readFlush();
                f.step = ref decompressor;
                return;
            }
            f.finishBlock();
        }

        private static void finishBlock(this ref decompressor f)
        {
            if (f.final)
            {
                if (f.dict.availRead() > 0L)
                {
                    f.toRead = f.dict.readFlush();
                }
                f.err = io.EOF;
            }
            f.step = ref decompressor;
        }

        private static error moreBits(this ref decompressor f)
        {
            var (c, err) = f.r.ReadByte();
            if (err != null)
            {
                if (err == io.EOF)
                {
                    err = io.ErrUnexpectedEOF;
                }
                return error.As(err);
            }
            f.roffset++;
            f.b |= uint32(c) << (int)(f.nb);
            f.nb += 8L;
            return error.As(null);
        }

        // Read the next Huffman-encoded symbol from f according to h.
        private static (long, error) huffSym(this ref decompressor f, ref huffmanDecoder h)
        { 
            // Since a huffmanDecoder can be empty or be composed of a degenerate tree
            // with single element, huffSym must error on these two edge cases. In both
            // cases, the chunks slice will be 0 for the invalid sequence, leading it
            // satisfy the n == 0 check below.
            var n = uint(h.min);
            while (true)
            {
                while (f.nb < n)
                {
                    {
                        var err = f.moreBits();

                        if (err != null)
                        {
                            return (0L, err);
                        }

                    }
                }

                var chunk = h.chunks[f.b & (huffmanNumChunks - 1L)];
                n = uint(chunk & huffmanCountMask);
                if (n > huffmanChunkBits)
                {
                    chunk = h.links[chunk >> (int)(huffmanValueShift)][(f.b >> (int)(huffmanChunkBits)) & h.linkMask];
                    n = uint(chunk & huffmanCountMask);
                }
                if (n <= f.nb)
                {
                    if (n == 0L)
                    {
                        f.err = CorruptInputError(f.roffset);
                        return (0L, f.err);
                    }
                    f.b >>= n;
                    f.nb -= n;
                    return (int(chunk >> (int)(huffmanValueShift)), null);
                }
            }

        }

        private static Reader makeReader(io.Reader r)
        {
            {
                Reader (rr, ok) = r._<Reader>();

                if (ok)
                {
                    return rr;
                }

            }
            return bufio.NewReader(r);
        }

        private static void fixedHuffmanDecoderInit()
        {
            fixedOnce.Do(() =>
            { 
                // These come from the RFC section 3.2.6.
                array<long> bits = new array<long>(288L);
                {
                    long i__prev1 = i;

                    for (long i = 0L; i < 144L; i++)
                    {
                        bits[i] = 8L;
                    }


                    i = i__prev1;
                }
                {
                    long i__prev1 = i;

                    for (i = 144L; i < 256L; i++)
                    {
                        bits[i] = 9L;
                    }


                    i = i__prev1;
                }
                {
                    long i__prev1 = i;

                    for (i = 256L; i < 280L; i++)
                    {
                        bits[i] = 7L;
                    }


                    i = i__prev1;
                }
                {
                    long i__prev1 = i;

                    for (i = 280L; i < 288L; i++)
                    {
                        bits[i] = 8L;
                    }


                    i = i__prev1;
                }
                fixedHuffmanDecoder.init(bits[..]);
            });
        }

        private static error Reset(this ref decompressor f, io.Reader r, slice<byte> dict)
        {
            f.Value = new decompressor(r:makeReader(r),bits:f.bits,codebits:f.codebits,dict:f.dict,step:(*decompressor).nextBlock,);
            f.dict.init(maxMatchOffset, dict);
            return error.As(null);
        }

        // NewReader returns a new ReadCloser that can be used
        // to read the uncompressed version of r.
        // If r does not also implement io.ByteReader,
        // the decompressor may read more data than necessary from r.
        // It is the caller's responsibility to call Close on the ReadCloser
        // when finished reading.
        //
        // The ReadCloser returned by NewReader also implements Resetter.
        public static io.ReadCloser NewReader(io.Reader r)
        {
            fixedHuffmanDecoderInit();

            decompressor f = default;
            f.r = makeReader(r);
            f.bits = @new<array<long>>();
            f.codebits = @new<array<long>>();
            f.step = ref decompressor;
            f.dict.init(maxMatchOffset, null);
            return ref f;
        }

        // NewReaderDict is like NewReader but initializes the reader
        // with a preset dictionary. The returned Reader behaves as if
        // the uncompressed data stream started with the given dictionary,
        // which has already been read. NewReaderDict is typically used
        // to read data compressed by NewWriterDict.
        //
        // The ReadCloser returned by NewReader also implements Resetter.
        public static io.ReadCloser NewReaderDict(io.Reader r, slice<byte> dict)
        {
            fixedHuffmanDecoderInit();

            decompressor f = default;
            f.r = makeReader(r);
            f.bits = @new<array<long>>();
            f.codebits = @new<array<long>>();
            f.step = ref decompressor;
            f.dict.init(maxMatchOffset, dict);
            return ref f;
        }
    }
}}
