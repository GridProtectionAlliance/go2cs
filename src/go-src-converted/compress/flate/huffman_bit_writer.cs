// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package flate -- go2cs converted at 2020 August 29 08:23:28 UTC
// import "compress/flate" ==> using flate = go.compress.flate_package
// Original source: C:\Go\src\compress\flate\huffman_bit_writer.go
using io = go.io_package;
using static go.builtin;

namespace go {
namespace compress
{
    public static partial class flate_package
    {
 
        // The largest offset code.
        private static readonly long offsetCodeCount = 30L; 

        // The special code used to mark the end of a block.
        private static readonly long endBlockMarker = 256L; 

        // The first length code.
        private static readonly long lengthCodesStart = 257L; 

        // The number of codegen codes.
        private static readonly long codegenCodeCount = 19L;
        private static readonly long badCode = 255L; 

        // bufferFlushSize indicates the buffer size
        // after which bytes are flushed to the writer.
        // Should preferably be a multiple of 6, since
        // we accumulate 6 bytes between writes to the buffer.
        private static readonly long bufferFlushSize = 240L; 

        // bufferSize is the actual output byte buffer size.
        // It must have additional headroom for a flush
        // which can contain up to 8 bytes.
        private static readonly var bufferSize = bufferFlushSize + 8L;

        // The number of extra bits needed by length code X - LENGTH_CODES_START.
        private static sbyte lengthExtraBits = new slice<sbyte>(new sbyte[] { 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 4, 4, 4, 4, 5, 5, 5, 5, 0 });

        // The length indicated by length code X - LENGTH_CODES_START.
        private static uint lengthBase = new slice<uint>(new uint[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 10, 12, 14, 16, 20, 24, 28, 32, 40, 48, 56, 64, 80, 96, 112, 128, 160, 192, 224, 255 });

        // offset code word extra bits.
        private static sbyte offsetExtraBits = new slice<sbyte>(new sbyte[] { 0, 0, 0, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 7, 7, 8, 8, 9, 9, 10, 10, 11, 11, 12, 12, 13, 13 });

        private static uint offsetBase = new slice<uint>(new uint[] { 0x000000, 0x000001, 0x000002, 0x000003, 0x000004, 0x000006, 0x000008, 0x00000c, 0x000010, 0x000018, 0x000020, 0x000030, 0x000040, 0x000060, 0x000080, 0x0000c0, 0x000100, 0x000180, 0x000200, 0x000300, 0x000400, 0x000600, 0x000800, 0x000c00, 0x001000, 0x001800, 0x002000, 0x003000, 0x004000, 0x006000 });

        // The odd order in which the codegen code sizes are written.
        private static uint codegenOrder = new slice<uint>(new uint[] { 16, 17, 18, 0, 8, 7, 9, 6, 10, 5, 11, 4, 12, 3, 13, 2, 14, 1, 15 });

        private partial struct huffmanBitWriter
        {
            public io.Writer writer; // Data waiting to be written is bytes[0:nbytes]
// and then the low nbits of bits.
            public ulong bits;
            public ulong nbits;
            public array<byte> bytes;
            public array<int> codegenFreq;
            public long nbytes;
            public slice<int> literalFreq;
            public slice<int> offsetFreq;
            public slice<byte> codegen;
            public ptr<huffmanEncoder> literalEncoding;
            public ptr<huffmanEncoder> offsetEncoding;
            public ptr<huffmanEncoder> codegenEncoding;
            public error err;
        }

        private static ref huffmanBitWriter newHuffmanBitWriter(io.Writer w)
        {
            return ref new huffmanBitWriter(writer:w,literalFreq:make([]int32,maxNumLit),offsetFreq:make([]int32,offsetCodeCount),codegen:make([]uint8,maxNumLit+offsetCodeCount+1),literalEncoding:newHuffmanEncoder(maxNumLit),codegenEncoding:newHuffmanEncoder(codegenCodeCount),offsetEncoding:newHuffmanEncoder(offsetCodeCount),);
        }

        private static void reset(this ref huffmanBitWriter w, io.Writer writer)
        {
            w.writer = writer;
            w.bits = 0L;
            w.nbits = 0L;
            w.nbytes = 0L;
            w.err = null;
            w.bytes = new array<byte>(new byte[] {  });
        }

        private static void flush(this ref huffmanBitWriter w)
        {
            if (w.err != null)
            {
                w.nbits = 0L;
                return;
            }
            var n = w.nbytes;
            while (w.nbits != 0L)
            {
                w.bytes[n] = byte(w.bits);
                w.bits >>= 8L;
                if (w.nbits > 8L)
                { // Avoid underflow
                    w.nbits -= 8L;
                }
                else
                {
                    w.nbits = 0L;
                }
                n++;
            }

            w.bits = 0L;
            w.write(w.bytes[..n]);
            w.nbytes = 0L;
        }

        private static void write(this ref huffmanBitWriter w, slice<byte> b)
        {
            if (w.err != null)
            {
                return;
            }
            _, w.err = w.writer.Write(b);
        }

        private static void writeBits(this ref huffmanBitWriter w, int b, ulong nb)
        {
            if (w.err != null)
            {
                return;
            }
            w.bits |= uint64(b) << (int)(w.nbits);
            w.nbits += nb;
            if (w.nbits >= 48L)
            {
                var bits = w.bits;
                w.bits >>= 48L;
                w.nbits -= 48L;
                var n = w.nbytes;
                var bytes = w.bytes[n..n + 6L];
                bytes[0L] = byte(bits);
                bytes[1L] = byte(bits >> (int)(8L));
                bytes[2L] = byte(bits >> (int)(16L));
                bytes[3L] = byte(bits >> (int)(24L));
                bytes[4L] = byte(bits >> (int)(32L));
                bytes[5L] = byte(bits >> (int)(40L));
                n += 6L;
                if (n >= bufferFlushSize)
                {
                    w.write(w.bytes[..n]);
                    n = 0L;
                }
                w.nbytes = n;
            }
        }

        private static void writeBytes(this ref huffmanBitWriter w, slice<byte> bytes)
        {
            if (w.err != null)
            {
                return;
            }
            var n = w.nbytes;
            if (w.nbits & 7L != 0L)
            {
                w.err = InternalError("writeBytes with unfinished bits");
                return;
            }
            while (w.nbits != 0L)
            {
                w.bytes[n] = byte(w.bits);
                w.bits >>= 8L;
                w.nbits -= 8L;
                n++;
            }

            if (n != 0L)
            {
                w.write(w.bytes[..n]);
            }
            w.nbytes = 0L;
            w.write(bytes);
        }

        // RFC 1951 3.2.7 specifies a special run-length encoding for specifying
        // the literal and offset lengths arrays (which are concatenated into a single
        // array).  This method generates that run-length encoding.
        //
        // The result is written into the codegen array, and the frequencies
        // of each code is written into the codegenFreq array.
        // Codes 0-15 are single byte codes. Codes 16-18 are followed by additional
        // information. Code badCode is an end marker
        //
        //  numLiterals      The number of literals in literalEncoding
        //  numOffsets       The number of offsets in offsetEncoding
        //  litenc, offenc   The literal and offset encoder to use
        private static void generateCodegen(this ref huffmanBitWriter w, long numLiterals, long numOffsets, ref huffmanEncoder litEnc, ref huffmanEncoder offEnc)
        {
            {
                var i__prev1 = i;

                foreach (var (__i) in w.codegenFreq)
                {
                    i = __i;
                    w.codegenFreq[i] = 0L;
                } 
                // Note that we are using codegen both as a temporary variable for holding
                // a copy of the frequencies, and as the place where we put the result.
                // This is fine because the output is always shorter than the input used
                // so far.

                i = i__prev1;
            }

            var codegen = w.codegen; // cache
            // Copy the concatenated code sizes to codegen. Put a marker at the end.
            var cgnl = codegen[..numLiterals];
            {
                var i__prev1 = i;

                foreach (var (__i) in cgnl)
                {
                    i = __i;
                    cgnl[i] = uint8(litEnc.codes[i].len);
                }

                i = i__prev1;
            }

            cgnl = codegen[numLiterals..numLiterals + numOffsets];
            {
                var i__prev1 = i;

                foreach (var (__i) in cgnl)
                {
                    i = __i;
                    cgnl[i] = uint8(offEnc.codes[i].len);
                }

                i = i__prev1;
            }

            codegen[numLiterals + numOffsets] = badCode;

            var size = codegen[0L];
            long count = 1L;
            long outIndex = 0L;
            for (long inIndex = 1L; size != badCode; inIndex++)
            { 
                // INVARIANT: We have seen "count" copies of size that have not yet
                // had output generated for them.
                var nextSize = codegen[inIndex];
                if (nextSize == size)
                {
                    count++;
                    continue;
                } 
                // We need to generate codegen indicating "count" of size.
                if (size != 0L)
                {
                    codegen[outIndex] = size;
                    outIndex++;
                    w.codegenFreq[size]++;
                    count--;
                    while (count >= 3L)
                    {
                        long n = 6L;
                        if (n > count)
                        {
                            n = count;
                        }
                        codegen[outIndex] = 16L;
                        outIndex++;
                        codegen[outIndex] = uint8(n - 3L);
                        outIndex++;
                        w.codegenFreq[16L]++;
                        count -= n;
                    }
                else

                }                {
                    while (count >= 11L)
                    {
                        n = 138L;
                        if (n > count)
                        {
                            n = count;
                        }
                        codegen[outIndex] = 18L;
                        outIndex++;
                        codegen[outIndex] = uint8(n - 11L);
                        outIndex++;
                        w.codegenFreq[18L]++;
                        count -= n;
                    }

                    if (count >= 3L)
                    { 
                        // count >= 3 && count <= 10
                        codegen[outIndex] = 17L;
                        outIndex++;
                        codegen[outIndex] = uint8(count - 3L);
                        outIndex++;
                        w.codegenFreq[17L]++;
                        count = 0L;
                    }
                }
                count--;
                while (count >= 0L)
                {
                    codegen[outIndex] = size;
                    outIndex++;
                    w.codegenFreq[size]++;
                    count--;
                } 
                // Set up invariant for next time through the loop.
 
                // Set up invariant for next time through the loop.
                size = nextSize;
                count = 1L;
            } 
            // Marker indicating the end of the codegen.
 
            // Marker indicating the end of the codegen.
            codegen[outIndex] = badCode;
        }

        // dynamicSize returns the size of dynamically encoded data in bits.
        private static (long, long) dynamicSize(this ref huffmanBitWriter w, ref huffmanEncoder litEnc, ref huffmanEncoder offEnc, long extraBits)
        {
            numCodegens = len(w.codegenFreq);
            while (numCodegens > 4L && w.codegenFreq[codegenOrder[numCodegens - 1L]] == 0L)
            {
                numCodegens--;
            }

            long header = 3L + 5L + 5L + 4L + (3L * numCodegens) + w.codegenEncoding.bitLength(w.codegenFreq[..]) + int(w.codegenFreq[16L]) * 2L + int(w.codegenFreq[17L]) * 3L + int(w.codegenFreq[18L]) * 7L;
            size = header + litEnc.bitLength(w.literalFreq) + offEnc.bitLength(w.offsetFreq) + extraBits;

            return (size, numCodegens);
        }

        // fixedSize returns the size of dynamically encoded data in bits.
        private static long fixedSize(this ref huffmanBitWriter w, long extraBits)
        {
            return 3L + fixedLiteralEncoding.bitLength(w.literalFreq) + fixedOffsetEncoding.bitLength(w.offsetFreq) + extraBits;
        }

        // storedSize calculates the stored size, including header.
        // The function returns the size in bits and whether the block
        // fits inside a single block.
        private static (long, bool) storedSize(this ref huffmanBitWriter w, slice<byte> @in)
        {
            if (in == null)
            {
                return (0L, false);
            }
            if (len(in) <= maxStoreBlockSize)
            {
                return ((len(in) + 5L) * 8L, true);
            }
            return (0L, false);
        }

        private static void writeCode(this ref huffmanBitWriter w, hcode c)
        {
            if (w.err != null)
            {
                return;
            }
            w.bits |= uint64(c.code) << (int)(w.nbits);
            w.nbits += uint(c.len);
            if (w.nbits >= 48L)
            {
                var bits = w.bits;
                w.bits >>= 48L;
                w.nbits -= 48L;
                var n = w.nbytes;
                var bytes = w.bytes[n..n + 6L];
                bytes[0L] = byte(bits);
                bytes[1L] = byte(bits >> (int)(8L));
                bytes[2L] = byte(bits >> (int)(16L));
                bytes[3L] = byte(bits >> (int)(24L));
                bytes[4L] = byte(bits >> (int)(32L));
                bytes[5L] = byte(bits >> (int)(40L));
                n += 6L;
                if (n >= bufferFlushSize)
                {
                    w.write(w.bytes[..n]);
                    n = 0L;
                }
                w.nbytes = n;
            }
        }

        // Write the header of a dynamic Huffman block to the output stream.
        //
        //  numLiterals  The number of literals specified in codegen
        //  numOffsets   The number of offsets specified in codegen
        //  numCodegens  The number of codegens used in codegen
        private static void writeDynamicHeader(this ref huffmanBitWriter w, long numLiterals, long numOffsets, long numCodegens, bool isEof)
        {
            if (w.err != null)
            {
                return;
            }
            int firstBits = 4L;
            if (isEof)
            {
                firstBits = 5L;
            }
            w.writeBits(firstBits, 3L);
            w.writeBits(int32(numLiterals - 257L), 5L);
            w.writeBits(int32(numOffsets - 1L), 5L);
            w.writeBits(int32(numCodegens - 4L), 4L);

            {
                long i__prev1 = i;

                for (long i = 0L; i < numCodegens; i++)
                {
                    var value = uint(w.codegenEncoding.codes[codegenOrder[i]].len);
                    w.writeBits(int32(value), 3L);
                }


                i = i__prev1;
            }

            i = 0L;
            while (true)
            {
                long codeWord = int(w.codegen[i]);
                i++;
                if (codeWord == badCode)
                {
                    break;
                }
                w.writeCode(w.codegenEncoding.codes[uint32(codeWord)]);

                switch (codeWord)
                {
                    case 16L: 
                        w.writeBits(int32(w.codegen[i]), 2L);
                        i++;
                        break;
                        break;
                    case 17L: 
                        w.writeBits(int32(w.codegen[i]), 3L);
                        i++;
                        break;
                        break;
                    case 18L: 
                        w.writeBits(int32(w.codegen[i]), 7L);
                        i++;
                        break;
                        break;
                }
            }

        }

        private static void writeStoredHeader(this ref huffmanBitWriter w, long length, bool isEof)
        {
            if (w.err != null)
            {
                return;
            }
            int flag = default;
            if (isEof)
            {
                flag = 1L;
            }
            w.writeBits(flag, 3L);
            w.flush();
            w.writeBits(int32(length), 16L);
            w.writeBits(int32(~uint16(length)), 16L);
        }

        private static void writeFixedHeader(this ref huffmanBitWriter w, bool isEof)
        {
            if (w.err != null)
            {
                return;
            } 
            // Indicate that we are a fixed Huffman block
            int value = 2L;
            if (isEof)
            {
                value = 3L;
            }
            w.writeBits(value, 3L);
        }

        // writeBlock will write a block of tokens with the smallest encoding.
        // The original input can be supplied, and if the huffman encoded data
        // is larger than the original bytes, the data will be written as a
        // stored block.
        // If the input is nil, the tokens will always be Huffman encoded.
        private static void writeBlock(this ref huffmanBitWriter w, slice<token> tokens, bool eof, slice<byte> input)
        {
            if (w.err != null)
            {
                return;
            }
            tokens = append(tokens, endBlockMarker);
            var (numLiterals, numOffsets) = w.indexTokens(tokens);

            long extraBits = default;
            var (storedSize, storable) = w.storedSize(input);
            if (storable)
            { 
                // We only bother calculating the costs of the extra bits required by
                // the length of offset fields (which will be the same for both fixed
                // and dynamic encoding), if we need to compare those two encodings
                // against stored encoding.
                for (var lengthCode = lengthCodesStart + 8L; lengthCode < numLiterals; lengthCode++)
                { 
                    // First eight length codes have extra size = 0.
                    extraBits += int(w.literalFreq[lengthCode]) * int(lengthExtraBits[lengthCode - lengthCodesStart]);
                }

                for (long offsetCode = 4L; offsetCode < numOffsets; offsetCode++)
                { 
                    // First four offset codes have extra size = 0.
                    extraBits += int(w.offsetFreq[offsetCode]) * int(offsetExtraBits[offsetCode]);
                }

            } 

            // Figure out smallest code.
            // Fixed Huffman baseline.
            var literalEncoding = fixedLiteralEncoding;
            var offsetEncoding = fixedOffsetEncoding;
            var size = w.fixedSize(extraBits); 

            // Dynamic Huffman?
            long numCodegens = default; 

            // Generate codegen and codegenFrequencies, which indicates how to encode
            // the literalEncoding and the offsetEncoding.
            w.generateCodegen(numLiterals, numOffsets, w.literalEncoding, w.offsetEncoding);
            w.codegenEncoding.generate(w.codegenFreq[..], 7L);
            var (dynamicSize, numCodegens) = w.dynamicSize(w.literalEncoding, w.offsetEncoding, extraBits);

            if (dynamicSize < size)
            {
                size = dynamicSize;
                literalEncoding = w.literalEncoding;
                offsetEncoding = w.offsetEncoding;
            } 

            // Stored bytes?
            if (storable && storedSize < size)
            {
                w.writeStoredHeader(len(input), eof);
                w.writeBytes(input);
                return;
            } 

            // Huffman.
            if (literalEncoding == fixedLiteralEncoding)
            {
                w.writeFixedHeader(eof);
            }
            else
            {
                w.writeDynamicHeader(numLiterals, numOffsets, numCodegens, eof);
            } 

            // Write the tokens.
            w.writeTokens(tokens, literalEncoding.codes, offsetEncoding.codes);
        }

        // writeBlockDynamic encodes a block using a dynamic Huffman table.
        // This should be used if the symbols used have a disproportionate
        // histogram distribution.
        // If input is supplied and the compression savings are below 1/16th of the
        // input size the block is stored.
        private static void writeBlockDynamic(this ref huffmanBitWriter w, slice<token> tokens, bool eof, slice<byte> input)
        {
            if (w.err != null)
            {
                return;
            }
            tokens = append(tokens, endBlockMarker);
            var (numLiterals, numOffsets) = w.indexTokens(tokens); 

            // Generate codegen and codegenFrequencies, which indicates how to encode
            // the literalEncoding and the offsetEncoding.
            w.generateCodegen(numLiterals, numOffsets, w.literalEncoding, w.offsetEncoding);
            w.codegenEncoding.generate(w.codegenFreq[..], 7L);
            var (size, numCodegens) = w.dynamicSize(w.literalEncoding, w.offsetEncoding, 0L); 

            // Store bytes, if we don't get a reasonable improvement.
            {
                var (ssize, storable) = w.storedSize(input);

                if (storable && ssize < (size + size >> (int)(4L)))
                {
                    w.writeStoredHeader(len(input), eof);
                    w.writeBytes(input);
                    return;
                } 

                // Write Huffman table.

            } 

            // Write Huffman table.
            w.writeDynamicHeader(numLiterals, numOffsets, numCodegens, eof); 

            // Write the tokens.
            w.writeTokens(tokens, w.literalEncoding.codes, w.offsetEncoding.codes);
        }

        // indexTokens indexes a slice of tokens, and updates
        // literalFreq and offsetFreq, and generates literalEncoding
        // and offsetEncoding.
        // The number of literal and offset tokens is returned.
        private static (long, long) indexTokens(this ref huffmanBitWriter w, slice<token> tokens)
        {
            {
                var i__prev1 = i;

                foreach (var (__i) in w.literalFreq)
                {
                    i = __i;
                    w.literalFreq[i] = 0L;
                }

                i = i__prev1;
            }

            {
                var i__prev1 = i;

                foreach (var (__i) in w.offsetFreq)
                {
                    i = __i;
                    w.offsetFreq[i] = 0L;
                }

                i = i__prev1;
            }

            foreach (var (_, t) in tokens)
            {
                if (t < matchType)
                {
                    w.literalFreq[t.literal()]++;
                    continue;
                }
                var length = t.length();
                var offset = t.offset();
                w.literalFreq[lengthCodesStart + lengthCode(length)]++;
                w.offsetFreq[offsetCode(offset)]++;
            } 

            // get the number of literals
            numLiterals = len(w.literalFreq);
            while (w.literalFreq[numLiterals - 1L] == 0L)
            {
                numLiterals--;
            } 
            // get the number of offsets
 
            // get the number of offsets
            numOffsets = len(w.offsetFreq);
            while (numOffsets > 0L && w.offsetFreq[numOffsets - 1L] == 0L)
            {
                numOffsets--;
            }

            if (numOffsets == 0L)
            { 
                // We haven't found a single match. If we want to go with the dynamic encoding,
                // we should count at least one offset to be sure that the offset huffman tree could be encoded.
                w.offsetFreq[0L] = 1L;
                numOffsets = 1L;
            }
            w.literalEncoding.generate(w.literalFreq, 15L);
            w.offsetEncoding.generate(w.offsetFreq, 15L);
            return;
        }

        // writeTokens writes a slice of tokens to the output.
        // codes for literal and offset encoding must be supplied.
        private static void writeTokens(this ref huffmanBitWriter w, slice<token> tokens, slice<hcode> leCodes, slice<hcode> oeCodes)
        {
            if (w.err != null)
            {
                return;
            }
            foreach (var (_, t) in tokens)
            {
                if (t < matchType)
                {
                    w.writeCode(leCodes[t.literal()]);
                    continue;
                } 
                // Write the length
                var length = t.length();
                var lengthCode = lengthCode(length);
                w.writeCode(leCodes[lengthCode + lengthCodesStart]);
                var extraLengthBits = uint(lengthExtraBits[lengthCode]);
                if (extraLengthBits > 0L)
                {
                    var extraLength = int32(length - lengthBase[lengthCode]);
                    w.writeBits(extraLength, extraLengthBits);
                } 
                // Write the offset
                var offset = t.offset();
                var offsetCode = offsetCode(offset);
                w.writeCode(oeCodes[offsetCode]);
                var extraOffsetBits = uint(offsetExtraBits[offsetCode]);
                if (extraOffsetBits > 0L)
                {
                    var extraOffset = int32(offset - offsetBase[offsetCode]);
                    w.writeBits(extraOffset, extraOffsetBits);
                }
            }
        }

        // huffOffset is a static offset encoder used for huffman only encoding.
        // It can be reused since we will not be encoding offset values.
        private static ref huffmanEncoder huffOffset = default;

        private static void init()
        {
            var w = newHuffmanBitWriter(null);
            w.offsetFreq[0L] = 1L;
            huffOffset = newHuffmanEncoder(offsetCodeCount);
            huffOffset.generate(w.offsetFreq, 15L);
        }

        // writeBlockHuff encodes a block of bytes as either
        // Huffman encoded literals or uncompressed bytes if the
        // results only gains very little from compression.
        private static void writeBlockHuff(this ref huffmanBitWriter w, bool eof, slice<byte> input)
        {
            if (w.err != null)
            {
                return;
            } 

            // Clear histogram
            foreach (var (i) in w.literalFreq)
            {
                w.literalFreq[i] = 0L;
            } 

            // Add everything as literals
            histogram(input, w.literalFreq);

            w.literalFreq[endBlockMarker] = 1L;

            const var numLiterals = endBlockMarker + 1L;

            const long numOffsets = 1L;



            w.literalEncoding.generate(w.literalFreq, 15L); 

            // Figure out smallest code.
            // Always use dynamic Huffman or Store
            long numCodegens = default; 

            // Generate codegen and codegenFrequencies, which indicates how to encode
            // the literalEncoding and the offsetEncoding.
            w.generateCodegen(numLiterals, numOffsets, w.literalEncoding, huffOffset);
            w.codegenEncoding.generate(w.codegenFreq[..], 7L);
            var (size, numCodegens) = w.dynamicSize(w.literalEncoding, huffOffset, 0L); 

            // Store bytes, if we don't get a reasonable improvement.
            {
                var (ssize, storable) = w.storedSize(input);

                if (storable && ssize < (size + size >> (int)(4L)))
                {
                    w.writeStoredHeader(len(input), eof);
                    w.writeBytes(input);
                    return;
                } 

                // Huffman.

            } 

            // Huffman.
            w.writeDynamicHeader(numLiterals, numOffsets, numCodegens, eof);
            var encoding = w.literalEncoding.codes[..257L];
            var n = w.nbytes;
            foreach (var (_, t) in input)
            { 
                // Bitwriting inlined, ~30% speedup
                var c = encoding[t];
                w.bits |= uint64(c.code) << (int)(w.nbits);
                w.nbits += uint(c.len);
                if (w.nbits < 48L)
                {
                    continue;
                } 
                // Store 6 bytes
                var bits = w.bits;
                w.bits >>= 48L;
                w.nbits -= 48L;
                var bytes = w.bytes[n..n + 6L];
                bytes[0L] = byte(bits);
                bytes[1L] = byte(bits >> (int)(8L));
                bytes[2L] = byte(bits >> (int)(16L));
                bytes[3L] = byte(bits >> (int)(24L));
                bytes[4L] = byte(bits >> (int)(32L));
                bytes[5L] = byte(bits >> (int)(40L));
                n += 6L;
                if (n < bufferFlushSize)
                {
                    continue;
                }
                w.write(w.bytes[..n]);
                if (w.err != null)
                {
                    return; // Return early in the event of write failures
                }
                n = 0L;
            }
            w.nbytes = n;
            w.writeCode(encoding[endBlockMarker]);
        }

        // histogram accumulates a histogram of b in h.
        //
        // len(h) must be >= 256, and h's elements must be all zeroes.
        private static void histogram(slice<byte> b, slice<int> h)
        {
            h = h[..256L];
            foreach (var (_, t) in b)
            {
                h[t]++;
            }
        }
    }
}}
