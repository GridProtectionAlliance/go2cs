// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Small in-memory unzip implementation.
// A simplified copy of the pre-Go 1 compress/flate/inflate.go
// and a modified copy of the zip reader in package time.
// (The one in package time does not support decompression; this one does.)

// package syscall -- go2cs converted at 2020 August 29 08:38:36 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\unzip_nacl.go

using static go.builtin;

namespace go
{
    public static partial class syscall_package
    {
        private static readonly long maxCodeLen = 16L; // max length of Huffman code
        private static readonly long maxHist = 32768L; // max history required
        private static readonly long maxLit = 286L;
        private static readonly long maxDist = 32L;
        private static readonly long numCodes = 19L; // number of codes in Huffman meta-code

        private partial struct decompressor
        {
            public @string @in; // compressed input
            public slice<byte> @out; // uncompressed output
            public uint b; // input bits, at top of b
            public ulong nb;
            public bool err; // invalid input
            public bool eof; // reached EOF

            public huffmanDecoder h1; // decoders for literal/length, distance
            public huffmanDecoder h2; // decoders for literal/length, distance
            public array<long> bits; // lengths defining Huffman codes
            public array<long> codebits;
        }

        private static void nextBlock(this ref decompressor f)
        {
            while (f.nb < 1L + 2L)
            {
                f.moreBits();

                if (f.err)
                {
                    return;
                }
            }

            f.eof = f.b & 1L == 1L;
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
                    f.huffmanBlock(ref fixedHuffmanDecoder, null);
                    break;
                case 2L: 
                    // compressed, dynamic Huffman tables
                    f.readHuffman();

                    if (f.err)
                    {
                        break;
                    }
                    f.huffmanBlock(ref f.h1, ref f.h2);
                    break;
                default: 
                    // 3 is reserved.
                    f.err = true;
                    break;
            }
        }

        // RFC 1951 section 3.2.7.
        // Compression with dynamic Huffman codes

        private static array<long> codeOrder = new array<long>(new long[] { 16, 17, 18, 0, 8, 7, 9, 6, 10, 5, 11, 4, 12, 3, 13, 2, 14, 1, 15 });

        private static void readHuffman(this ref decompressor f)
        { 
            // HLIT[5], HDIST[5], HCLEN[4].
            while (f.nb < 5L + 5L + 4L)
            {
                f.moreBits();

                if (f.err)
                {
                    return;
                }
            }

            var nlit = int(f.b & 0x1FUL) + 257L;
            f.b >>= 5L;
            var ndist = int(f.b & 0x1FUL) + 1L;
            f.b >>= 5L;
            var nclen = int(f.b & 0xFUL) + 4L;
            f.b >>= 4L;
            f.nb -= 5L + 5L + 4L; 

            // (HCLEN+4)*3 bits: code lengths in the magic codeOrder order.
            {
                long i__prev1 = i;

                for (long i = 0L; i < nclen; i++)
                {
                    while (f.nb < 3L)
                    {
                        f.moreBits();

                        if (f.err)
                        {
                            return;
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
                f.err = true;
                return;
            } 

            // HLIT + 257 code lengths, HDIST + 1 code lengths,
            // using the code length Huffman code.
            {
                long i__prev1 = i;

                i = 0L;
                var n = nlit + ndist;

                while (i < n)
                {
                    var x = f.huffSym(ref f.h1);
                    if (f.err)
                    {
                        return;
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
                                f.err = true;
                                return;
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
                            f.err = true;
                            return;
                            break;
                    }
                    while (f.nb < nb)
                    {
                        f.moreBits();

                        if (f.err)
                        {
                            return;
                        }
                    }

                    rep += int(f.b & uint32(1L << (int)(nb) - 1L));
                    f.b >>= nb;
                    f.nb -= nb;
                    if (i + rep > n)
                    {
                        f.err = true;
                        return;
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
                f.err = true;
                return;
            }
        }

        // Decode a single Huffman block from f.
        // hl and hd are the Huffman states for the lit/length values
        // and the distance values, respectively. If hd == nil, using the
        // fixed distance encoding associated with fixed Huffman blocks.
        private static void huffmanBlock(this ref decompressor f, ref huffmanDecoder hl, ref huffmanDecoder hd)
        {
            while (true)
            {
                var v = f.huffSym(hl);
                if (f.err)
                {
                    return;
                }
                ulong n = default; // number of bits extra
                long length = default;

                if (v < 256L) 
                    f.@out = append(f.@out, byte(v));
                    continue;
                else if (v == 256L) 
                    // Done with huffman block; read next block.
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
                else 
                    length = 258L;
                    n = 0L;
                                if (n > 0L)
                {
                    while (f.nb < n)
                    {
                        f.moreBits();

                        if (f.err)
                        {
                            return;
                        }
                    }

                    length += int(f.b & uint32(1L << (int)(n) - 1L));
                    f.b >>= n;
                    f.nb -= n;
                }
                long dist = default;
                if (hd == null)
                {
                    while (f.nb < 5L)
                    {
                        f.moreBits();

                        if (f.err)
                        {
                            return;
                        }
                    }
                else

                    dist = int(reverseByte[(f.b & 0x1FUL) << (int)(3L)]);
                    f.b >>= 5L;
                    f.nb -= 5L;
                }                {
                    dist = f.huffSym(hd);

                    if (f.err)
                    {
                        return;
                    }
                }

                if (dist < 4L) 
                    dist++;
                else if (dist >= 30L) 
                    f.err = true;
                    return;
                else 
                    var nb = uint(dist - 2L) >> (int)(1L); 
                    // have 1 bit in bottom of dist, need nb more.
                    var extra = (dist & 1L) << (int)(nb);
                    while (f.nb < nb)
                    {
                        f.moreBits();

                        if (f.err)
                        {
                            return;
                        }
                    }

                    extra |= int(f.b & uint32(1L << (int)(nb) - 1L));
                    f.b >>= nb;
                    f.nb -= nb;
                    dist = 1L << (int)((nb + 1L)) + 1L + extra;
                // Copy [-dist:-dist+length] into output.
                // Encoding can be prescient, so no check on length.
                if (dist > len(f.@out))
                {
                    f.err = true;
                    return;
                }
                var p = len(f.@out) - dist;
                for (long i = 0L; i < length; i++)
                {
                    f.@out = append(f.@out, f.@out[p]);
                    p++;
                }

            }

        }

        // Copy a single uncompressed data block from input to output.
        private static void dataBlock(this ref decompressor f)
        { 
            // Uncompressed.
            // Discard current half-byte.
            f.nb = 0L;
            f.b = 0L;

            if (len(f.@in) < 4L)
            {
                f.err = true;
                return;
            }
            var buf = f.@in[..4L];
            f.@in = f.@in[4L..];
            var n = int(buf[0L]) | int(buf[1L]) << (int)(8L);
            var nn = int(buf[2L]) | int(buf[3L]) << (int)(8L);
            if (uint16(nn) != uint16(~n))
            {
                f.err = true;
                return;
            }
            if (len(f.@in) < n)
            {
                f.err = true;
                return;
            }
            f.@out = append(f.@out, f.@in[..n]);
            f.@in = f.@in[n..];
        }

        private static void moreBits(this ref decompressor f)
        {
            if (len(f.@in) == 0L)
            {
                f.err = true;
                return;
            }
            var c = f.@in[0L];
            f.@in = f.@in[1L..];
            f.b |= uint32(c) << (int)(f.nb);
            f.nb += 8L;
        }

        // Read the next Huffman-encoded symbol from f according to h.
        private static long huffSym(this ref decompressor f, ref huffmanDecoder h)
        {
            for (var n = uint(h.min); n <= uint(h.max); n++)
            {
                var lim = h.limit[n];
                if (lim == -1L)
                {
                    continue;
                }
                while (f.nb < n)
                {
                    f.moreBits();

                    if (f.err)
                    {
                        return 0L;
                    }
                }

                var v = int(f.b & uint32(1L << (int)(n) - 1L));
                v <<= 16L - n;
                v = int(reverseByte[v >> (int)(8L)]) | int(reverseByte[v & 0xFFUL]) << (int)(8L); // reverse bits
                if (v <= lim)
                {
                    f.b >>= n;
                    f.nb -= n;
                    return h.codes[v - h.@base[n]];
                }
            }

            f.err = true;
            return 0L;
        }

        private static array<byte> reverseByte = new array<byte>(new byte[] { 0x00, 0x80, 0x40, 0xc0, 0x20, 0xa0, 0x60, 0xe0, 0x10, 0x90, 0x50, 0xd0, 0x30, 0xb0, 0x70, 0xf0, 0x08, 0x88, 0x48, 0xc8, 0x28, 0xa8, 0x68, 0xe8, 0x18, 0x98, 0x58, 0xd8, 0x38, 0xb8, 0x78, 0xf8, 0x04, 0x84, 0x44, 0xc4, 0x24, 0xa4, 0x64, 0xe4, 0x14, 0x94, 0x54, 0xd4, 0x34, 0xb4, 0x74, 0xf4, 0x0c, 0x8c, 0x4c, 0xcc, 0x2c, 0xac, 0x6c, 0xec, 0x1c, 0x9c, 0x5c, 0xdc, 0x3c, 0xbc, 0x7c, 0xfc, 0x02, 0x82, 0x42, 0xc2, 0x22, 0xa2, 0x62, 0xe2, 0x12, 0x92, 0x52, 0xd2, 0x32, 0xb2, 0x72, 0xf2, 0x0a, 0x8a, 0x4a, 0xca, 0x2a, 0xaa, 0x6a, 0xea, 0x1a, 0x9a, 0x5a, 0xda, 0x3a, 0xba, 0x7a, 0xfa, 0x06, 0x86, 0x46, 0xc6, 0x26, 0xa6, 0x66, 0xe6, 0x16, 0x96, 0x56, 0xd6, 0x36, 0xb6, 0x76, 0xf6, 0x0e, 0x8e, 0x4e, 0xce, 0x2e, 0xae, 0x6e, 0xee, 0x1e, 0x9e, 0x5e, 0xde, 0x3e, 0xbe, 0x7e, 0xfe, 0x01, 0x81, 0x41, 0xc1, 0x21, 0xa1, 0x61, 0xe1, 0x11, 0x91, 0x51, 0xd1, 0x31, 0xb1, 0x71, 0xf1, 0x09, 0x89, 0x49, 0xc9, 0x29, 0xa9, 0x69, 0xe9, 0x19, 0x99, 0x59, 0xd9, 0x39, 0xb9, 0x79, 0xf9, 0x05, 0x85, 0x45, 0xc5, 0x25, 0xa5, 0x65, 0xe5, 0x15, 0x95, 0x55, 0xd5, 0x35, 0xb5, 0x75, 0xf5, 0x0d, 0x8d, 0x4d, 0xcd, 0x2d, 0xad, 0x6d, 0xed, 0x1d, 0x9d, 0x5d, 0xdd, 0x3d, 0xbd, 0x7d, 0xfd, 0x03, 0x83, 0x43, 0xc3, 0x23, 0xa3, 0x63, 0xe3, 0x13, 0x93, 0x53, 0xd3, 0x33, 0xb3, 0x73, 0xf3, 0x0b, 0x8b, 0x4b, 0xcb, 0x2b, 0xab, 0x6b, 0xeb, 0x1b, 0x9b, 0x5b, 0xdb, 0x3b, 0xbb, 0x7b, 0xfb, 0x07, 0x87, 0x47, 0xc7, 0x27, 0xa7, 0x67, 0xe7, 0x17, 0x97, 0x57, 0xd7, 0x37, 0xb7, 0x77, 0xf7, 0x0f, 0x8f, 0x4f, 0xcf, 0x2f, 0xaf, 0x6f, 0xef, 0x1f, 0x9f, 0x5f, 0xdf, 0x3f, 0xbf, 0x7f, 0xff });

        // Hard-coded Huffman tables for DEFLATE algorithm.
        // See RFC 1951, section 3.2.6.
        private static huffmanDecoder fixedHuffmanDecoder = new huffmanDecoder(7,9,[maxCodeLen+1]int{7:23,199,511},[maxCodeLen+1]int{7:0,24,224},[]int{256,257,258,259,260,261,262,263,264,265,266,267,268,269,270,271,272,273,274,275,276,277,278,279,0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33,34,35,36,37,38,39,40,41,42,43,44,45,46,47,48,49,50,51,52,53,54,55,56,57,58,59,60,61,62,63,64,65,66,67,68,69,70,71,72,73,74,75,76,77,78,79,80,81,82,83,84,85,86,87,88,89,90,91,92,93,94,95,96,97,98,99,100,101,102,103,104,105,106,107,108,109,110,111,112,113,114,115,116,117,118,119,120,121,122,123,124,125,126,127,128,129,130,131,132,133,134,135,136,137,138,139,140,141,142,143,280,281,282,283,284,285,286,287,144,145,146,147,148,149,150,151,152,153,154,155,156,157,158,159,160,161,162,163,164,165,166,167,168,169,170,171,172,173,174,175,176,177,178,179,180,181,182,183,184,185,186,187,188,189,190,191,192,193,194,195,196,197,198,199,200,201,202,203,204,205,206,207,208,209,210,211,212,213,214,215,216,217,218,219,220,221,222,223,224,225,226,227,228,229,230,231,232,233,234,235,236,237,238,239,240,241,242,243,244,245,246,247,248,249,250,251,252,253,254,255,},);

        // Huffman decoder is based on
        // J. Brian Connell, ``A Huffman-Shannon-Fano Code,''
        // Proceedings of the IEEE, 61(7) (July 1973), pp 1046-1047.
        private partial struct huffmanDecoder
        {
            public long min; // limit[i] = largest code word of length i
// Given code v of length n,
// need more bits if v > limit[n].
            public long max; // limit[i] = largest code word of length i
// Given code v of length n,
// need more bits if v > limit[n].
            public array<long> limit; // base[i] = smallest code word of length i - seq number
            public array<long> @base; // codes[seq number] = output code.
// Given code v of length n, value is
// codes[v - base[n]].
            public slice<long> codes;
        }

        // Initialize Huffman decoding tables from array of code lengths.
        private static bool init(this ref huffmanDecoder h, slice<long> bits)
        { 
            // Count number of codes of each length,
            // compute min and max length.
            array<long> count = new array<long>(maxCodeLen + 1L);
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

                n = n__prev1;
            }

            if (max == 0L)
            {
                return false;
            }
            h.min = min;
            h.max = max; 

            // For each code range, compute
            // nextcode (first code of that length),
            // limit (last code of that length), and
            // base (offset from first code to sequence number).
            long code = 0L;
            long seq = 0L;
            array<long> nextcode = new array<long>(maxCodeLen);
            {
                var i__prev1 = i;

                for (var i = min; i <= max; i++)
                {
                    var n = count[i];
                    nextcode[i] = code;
                    h.@base[i] = code - seq;
                    code += n;
                    seq += n;
                    h.limit[i] = code - 1L;
                    code <<= 1L;
                } 

                // Make array mapping sequence numbers to codes.


                i = i__prev1;
            } 

            // Make array mapping sequence numbers to codes.
            if (len(h.codes) < len(bits))
            {
                h.codes = make_slice<long>(len(bits));
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
                    seq = code - h.@base[n];
                    h.codes[seq] = i;
                }

                i = i__prev1;
                n = n__prev1;
            }

            return true;
        }

        private static slice<byte> inflate(@string @in)
        {
            decompressor d = default;
            d.@in = in;
            while (!d.err && !d.eof)
            {
                d.nextBlock();
            }

            if (len(d.@in) != 0L)
            {
                println("fs unzip: junk at end of compressed data");
                return null;
            }
            return d.@out;
        }

        // get4 returns the little-endian 32-bit value in b.
        private static long zget4(@string b)
        {
            if (len(b) < 4L)
            {
                return 0L;
            }
            return int(b[0L]) | int(b[1L]) << (int)(8L) | int(b[2L]) << (int)(16L) | int(b[3L]) << (int)(24L);
        }

        // get2 returns the little-endian 16-bit value in b.
        private static long zget2(@string b)
        {
            if (len(b) < 2L)
            {
                return 0L;
            }
            return int(b[0L]) | int(b[1L]) << (int)(8L);
        }

        private static void unzip(@string data)
        {
            const ulong zecheader = 0x06054b50UL;
            const ulong zcheader = 0x02014b50UL;
            const long ztailsize = 22L;
            const long zheadersize = 30L;
            const ulong zheader = 0x04034b50UL;

            var buf = data[len(data) - ztailsize..];
            var n = zget2(buf[10L..]);
            var size = zget4(buf[12L..]);
            var off = zget4(buf[16L..]);

            var hdr = data[off..off + size];
            for (long i = 0L; i < n; i++)
            { 
                // zip entry layout:
                //    0    magic[4]
                //    4    madevers[1]
                //    5    madeos[1]
                //    6    extvers[1]
                //    7    extos[1]
                //    8    flags[2]
                //    10    meth[2]
                //    12    modtime[2]
                //    14    moddate[2]
                //    16    crc[4]
                //    20    csize[4]
                //    24    uncsize[4]
                //    28    namelen[2]
                //    30    xlen[2]
                //    32    fclen[2]
                //    34    disknum[2]
                //    36    iattr[2]
                //    38    eattr[4]
                //    42    off[4]
                //    46    name[namelen]
                //    46+namelen+xlen+fclen - next header
                //
                if (zget4(hdr) != zcheader)
                {
                    println("fs unzip: bad magic");
                    break;
                }
                var meth = zget2(hdr[10L..]);
                var mtime = zget2(hdr[12L..]);
                var mdate = zget2(hdr[14L..]);
                var csize = zget4(hdr[20L..]);
                size = zget4(hdr[24L..]);
                var namelen = zget2(hdr[28L..]);
                var xlen = zget2(hdr[30L..]);
                var fclen = zget2(hdr[32L..]);
                var xattr = uint32(zget4(hdr[38L..])) >> (int)(16L);
                off = zget4(hdr[42L..]);
                var name = hdr[46L..46L + namelen];
                hdr = hdr[46L + namelen + xlen + fclen..]; 

                // zip per-file header layout:
                //    0    magic[4]
                //    4    extvers[1]
                //    5    extos[1]
                //    6    flags[2]
                //    8    meth[2]
                //    10    modtime[2]
                //    12    moddate[2]
                //    14    crc[4]
                //    18    csize[4]
                //    22    uncsize[4]
                //    26    namelen[2]
                //    28    xlen[2]
                //    30    name[namelen]
                //    30+namelen+xlen - file data
                //
                buf = data[off..off + zheadersize + namelen];
                if (zget4(buf) != zheader || zget2(buf[8L..]) != meth || zget2(buf[26L..]) != namelen || buf[30L..30L + namelen] != name)
                {
                    println("fs unzip: inconsistent zip file");
                    return;
                }
                xlen = zget2(buf[28L..]);

                off += zheadersize + namelen + xlen;

                slice<byte> fdata = default;
                switch (meth)
                {
                    case 0L: 
                        // buf is uncompressed
                        buf = data[off..off + size];
                        fdata = (slice<byte>)buf;
                        break;
                    case 8L: 
                        // buf is deflate-compressed
                        buf = data[off..off + csize];
                        fdata = inflate(buf);
                        if (len(fdata) != size)
                        {
                            println("fs unzip: inconsistent size in zip file");
                            return;
                        }
                        break;
                }

                if (xattr & S_IFMT == 0L)
                {
                    if (xattr & 0777L == 0L)
                    {
                        xattr |= 0666L;
                    }
                    if (len(name) > 0L && name[len(name) - 1L] == '/')
                    {
                        xattr |= S_IFDIR;
                        xattr |= 0111L;
                    }
                    else
                    {
                        xattr |= S_IFREG;
                    }
                }
                {
                    var err = create(name, xattr, zipToTime(mdate, mtime), fdata);

                    if (err != null)
                    {
                        print("fs unzip: create ", name, ": ", err.Error(), "\n");
                    }

                }
            }


            chdirEnv();
        }

        private static long zipToTime(long date, long time)
        {
            var dd = date & 0x1fUL;
            var mm = date >> (int)(5L) & 0xfUL;
            var yy = date >> (int)(9L); // since 1980

            var sec = int64(315532800L); // jan 1 1980
            sec += int64(yy) * 365L * 86400L;
            sec += int64(yy) / 4L * 86400L;
            if (yy % 4L > 0L || mm >= 3L)
            {
                sec += 86400L;
            }
            sec += int64(daysBeforeMonth[mm]) * 86400L;
            sec += int64(dd - 1L) * 86400L;

            var h = time >> (int)(11L);
            var m = time >> (int)(5L) & 0x3FUL;
            var s = time & 0x1fUL * 2L;
            sec += int64(h * 3600L + m * 60L + s);

            return sec;
        }

        private static array<int> daysBeforeMonth = new array<int>(new int[] { 0, 0, 31, 31+28, 31+28+31, 31+28+31+30, 31+28+31+30+31, 31+28+31+30+31+30, 31+28+31+30+31+30+31, 31+28+31+30+31+30+31+31, 31+28+31+30+31+30+31+31+30, 31+28+31+30+31+30+31+31+30+31, 31+28+31+30+31+30+31+31+30+31+30, 31+28+31+30+31+30+31+31+30+31+30+31 });
    }
}
