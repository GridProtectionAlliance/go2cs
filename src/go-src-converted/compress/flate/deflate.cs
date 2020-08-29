// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package flate -- go2cs converted at 2020 August 29 08:23:11 UTC
// import "compress/flate" ==> using flate = go.compress.flate_package
// Original source: C:\Go\src\compress\flate\deflate.go
using fmt = go.fmt_package;
using io = go.io_package;
using math = go.math_package;
using static go.builtin;
using System;

namespace go {
namespace compress
{
    public static partial class flate_package
    {
        public static readonly long NoCompression = 0L;
        public static readonly long BestSpeed = 1L;
        public static readonly long BestCompression = 9L;
        public static readonly long DefaultCompression = -1L; 

        // HuffmanOnly disables Lempel-Ziv match searching and only performs Huffman
        // entropy encoding. This mode is useful in compressing data that has
        // already been compressed with an LZ style algorithm (e.g. Snappy or LZ4)
        // that lacks an entropy encoder. Compression gains are achieved when
        // certain bytes in the input stream occur more frequently than others.
        //
        // Note that HuffmanOnly produces a compressed output that is
        // RFC 1951 compliant. That is, any valid DEFLATE decompressor will
        // continue to be able to decompress this output.
        public static readonly long HuffmanOnly = -2L;

        private static readonly long logWindowSize = 15L;
        private static readonly long windowSize = 1L << (int)(logWindowSize);
        private static readonly var windowMask = windowSize - 1L; 

        // The LZ77 step produces a sequence of literal tokens and <length, offset>
        // pair tokens. The offset is also known as distance. The underlying wire
        // format limits the range of lengths and offsets. For example, there are
        // 256 legitimate lengths: those in the range [3, 258]. This package's
        // compressor uses a higher minimum match length, enabling optimizations
        // such as finding matches via 32-bit loads and compares.
        private static readonly long baseMatchLength = 3L; // The smallest match length per the RFC section 3.2.5
        private static readonly long minMatchLength = 4L; // The smallest match length that the compressor actually emits
        private static readonly long maxMatchLength = 258L; // The largest match length
        private static readonly long baseMatchOffset = 1L; // The smallest match offset
        private static readonly long maxMatchOffset = 1L << (int)(15L); // The largest match offset

        // The maximum number of tokens we put into a single flate block, just to
        // stop things from getting too large.
        private static readonly long maxFlateBlockTokens = 1L << (int)(14L);
        private static readonly long maxStoreBlockSize = 65535L;
        private static readonly long hashBits = 17L; // After 17 performance degrades
        private static readonly long hashSize = 1L << (int)(hashBits);
        private static readonly long hashMask = (1L << (int)(hashBits)) - 1L;
        private static readonly long maxHashOffset = 1L << (int)(24L);

        private static readonly var skipNever = math.MaxInt32;

        private partial struct compressionLevel
        {
            public long level;
            public long good;
            public long lazy;
            public long nice;
            public long chain;
            public long fastSkipHashing;
        }

        private static compressionLevel levels = new slice<compressionLevel>(new compressionLevel[] { {0,0,0,0,0,0}, {1,0,0,0,0,0}, {2,4,0,16,8,5}, {3,4,0,32,32,6}, {4,4,4,16,16,skipNever}, {5,8,16,32,32,skipNever}, {6,8,16,128,128,skipNever}, {7,8,32,128,256,skipNever}, {8,32,128,258,1024,skipNever}, {9,32,258,258,4096,skipNever} });

        private partial struct compressor
        {
            public ref compressionLevel compressionLevel => ref compressionLevel_val;
            public ptr<huffmanBitWriter> w;
            public Action<slice<byte>, slice<uint>> bulkHasher; // compression algorithm
            public Func<ref compressor, slice<byte>, long> fill; // copy data to window
            public Action<ref compressor> step; // process window
            public bool sync; // requesting flush
            public ptr<deflateFast> bestSpeed; // Encoder for BestSpeed

// Input hash chains
// hashHead[hashValue] contains the largest inputIndex with the specified hash value
// If hashHead[hashValue] is within the current window, then
// hashPrev[hashHead[hashValue] & windowMask] contains the previous index
// with the same hash value.
            public long chainHead;
            public array<uint> hashHead;
            public array<uint> hashPrev;
            public long hashOffset; // input window: unprocessed data is window[index:windowEnd]
            public long index;
            public slice<byte> window;
            public long windowEnd;
            public long blockStart; // window index where current tokens start
            public bool byteAvailable; // if true, still need to process window[index-1].

// queued output tokens
            public slice<token> tokens; // deflate state
            public long length;
            public long offset;
            public uint hash;
            public long maxInsertIndex;
            public error err; // hashMatch must be able to contain hashes for the maximum match length.
            public array<uint> hashMatch;
        }

        private static long fillDeflate(this ref compressor d, slice<byte> b)
        {
            if (d.index >= 2L * windowSize - (minMatchLength + maxMatchLength))
            { 
                // shift the window by windowSize
                copy(d.window, d.window[windowSize..2L * windowSize]);
                d.index -= windowSize;
                d.windowEnd -= windowSize;
                if (d.blockStart >= windowSize)
                {
                    d.blockStart -= windowSize;
                }
                else
                {
                    d.blockStart = math.MaxInt32;
                }
                d.hashOffset += windowSize;
                if (d.hashOffset > maxHashOffset)
                {
                    var delta = d.hashOffset - 1L;
                    d.hashOffset -= delta;
                    d.chainHead -= delta; 

                    // Iterate over slices instead of arrays to avoid copying
                    // the entire table onto the stack (Issue #18625).
                    {
                        var i__prev1 = i;
                        var v__prev1 = v;

                        foreach (var (__i, __v) in d.hashPrev[..])
                        {
                            i = __i;
                            v = __v;
                            if (int(v) > delta)
                            {
                                d.hashPrev[i] = uint32(int(v) - delta);
                            }
                            else
                            {
                                d.hashPrev[i] = 0L;
                            }
                        }

                        i = i__prev1;
                        v = v__prev1;
                    }

                    {
                        var i__prev1 = i;
                        var v__prev1 = v;

                        foreach (var (__i, __v) in d.hashHead[..])
                        {
                            i = __i;
                            v = __v;
                            if (int(v) > delta)
                            {
                                d.hashHead[i] = uint32(int(v) - delta);
                            }
                            else
                            {
                                d.hashHead[i] = 0L;
                            }
                        }

                        i = i__prev1;
                        v = v__prev1;
                    }

                }
            }
            var n = copy(d.window[d.windowEnd..], b);
            d.windowEnd += n;
            return n;
        }

        private static error writeBlock(this ref compressor d, slice<token> tokens, long index)
        {
            if (index > 0L)
            {
                slice<byte> window = default;
                if (d.blockStart <= index)
                {
                    window = d.window[d.blockStart..index];
                }
                d.blockStart = index;
                d.w.writeBlock(tokens, false, window);
                return error.As(d.w.err);
            }
            return error.As(null);
        }

        // fillWindow will fill the current window with the supplied
        // dictionary and calculate all hashes.
        // This is much faster than doing a full encode.
        // Should only be used after a reset.
        private static void fillWindow(this ref compressor _d, slice<byte> b) => func(_d, (ref compressor d, Defer _, Panic panic, Recover __) =>
        { 
            // Do not fill window if we are in store-only mode.
            if (d.compressionLevel.level < 2L)
            {
                return;
            }
            if (d.index != 0L || d.windowEnd != 0L)
            {
                panic("internal error: fillWindow called with stale data");
            } 

            // If we are given too much, cut it.
            if (len(b) > windowSize)
            {
                b = b[len(b) - windowSize..];
            } 
            // Add all to window.
            var n = copy(d.window, b); 

            // Calculate 256 hashes at the time (more L1 cache hits)
            var loops = (n + 256L - minMatchLength) / 256L;
            for (long j = 0L; j < loops; j++)
            {
                var index = j * 256L;
                var end = index + 256L + minMatchLength - 1L;
                if (end > n)
                {
                    end = n;
                }
                var toCheck = d.window[index..end];
                var dstSize = len(toCheck) - minMatchLength + 1L;

                if (dstSize <= 0L)
                {
                    continue;
                }
                var dst = d.hashMatch[..dstSize];
                d.bulkHasher(toCheck, dst);
                uint newH = default;
                foreach (var (i, val) in dst)
                {
                    var di = i + index;
                    newH = val;
                    var hh = ref d.hashHead[newH & hashMask]; 
                    // Get previous value with the same hash.
                    // Our chain should point to the previous value.
                    d.hashPrev[di & windowMask] = hh.Value; 
                    // Set the head of the hash chain to us.
                    hh.Value = uint32(di + d.hashOffset);
                }
                d.hash = newH;
            } 
            // Update window information.
 
            // Update window information.
            d.windowEnd = n;
            d.index = n;
        });

        // Try to find a match starting at index whose length is greater than prevSize.
        // We only look at chainCount possibilities before giving up.
        private static (long, long, bool) findMatch(this ref compressor d, long pos, long prevHead, long prevLength, long lookahead)
        {
            var minMatchLook = maxMatchLength;
            if (lookahead < minMatchLook)
            {
                minMatchLook = lookahead;
            }
            var win = d.window[0L..pos + minMatchLook]; 

            // We quit when we get a match that's at least nice long
            var nice = len(win) - pos;
            if (d.nice < nice)
            {
                nice = d.nice;
            } 

            // If we've got a match that's good enough, only look in 1/4 the chain.
            var tries = d.chain;
            length = prevLength;
            if (length >= d.good)
            {
                tries >>= 2L;
            }
            var wEnd = win[pos + length];
            var wPos = win[pos..];
            var minIndex = pos - windowSize;

            for (var i = prevHead; tries > 0L; tries--)
            {
                if (wEnd == win[i + length])
                {
                    var n = matchLen(win[i..], wPos, minMatchLook);

                    if (n > length && (n > minMatchLength || pos - i <= 4096L))
                    {
                        length = n;
                        offset = pos - i;
                        ok = true;
                        if (n >= nice)
                        { 
                            // The match is good enough that we don't try to find a better one.
                            break;
                        }
                        wEnd = win[pos + n];
                    }
                }
                if (i == minIndex)
                { 
                    // hashPrev[i & windowMask] has already been overwritten, so stop now.
                    break;
                }
                i = int(d.hashPrev[i & windowMask]) - d.hashOffset;
                if (i < minIndex || i < 0L)
                {
                    break;
                }
            }

            return;
        }

        private static error writeStoredBlock(this ref compressor d, slice<byte> buf)
        {
            d.w.writeStoredHeader(len(buf), false);

            if (d.w.err != null)
            {
                return error.As(d.w.err);
            }
            d.w.writeBytes(buf);
            return error.As(d.w.err);
        }

        private static readonly ulong hashmul = 0x1e35a7bdUL;

        // hash4 returns a hash representation of the first 4 bytes
        // of the supplied slice.
        // The caller must ensure that len(b) >= 4.


        // hash4 returns a hash representation of the first 4 bytes
        // of the supplied slice.
        // The caller must ensure that len(b) >= 4.
        private static uint hash4(slice<byte> b)
        {
            return ((uint32(b[3L]) | uint32(b[2L]) << (int)(8L) | uint32(b[1L]) << (int)(16L) | uint32(b[0L]) << (int)(24L)) * hashmul) >> (int)((32L - hashBits));
        }

        // bulkHash4 will compute hashes using the same
        // algorithm as hash4
        private static void bulkHash4(slice<byte> b, slice<uint> dst)
        {
            if (len(b) < minMatchLength)
            {
                return;
            }
            var hb = uint32(b[3L]) | uint32(b[2L]) << (int)(8L) | uint32(b[1L]) << (int)(16L) | uint32(b[0L]) << (int)(24L);
            dst[0L] = (hb * hashmul) >> (int)((32L - hashBits));
            var end = len(b) - minMatchLength + 1L;
            for (long i = 1L; i < end; i++)
            {
                hb = (hb << (int)(8L)) | uint32(b[i + 3L]);
                dst[i] = (hb * hashmul) >> (int)((32L - hashBits));
            }

        }

        // matchLen returns the number of matching bytes in a and b
        // up to length 'max'. Both slices must be at least 'max'
        // bytes in size.
        private static long matchLen(slice<byte> a, slice<byte> b, long max)
        {
            a = a[..max];
            b = b[..len(a)];
            foreach (var (i, av) in a)
            {
                if (b[i] != av)
                {
                    return i;
                }
            }
            return max;
        }

        // encSpeed will compress and store the currently added data,
        // if enough has been accumulated or we at the end of the stream.
        // Any error that occurred will be in d.err
        private static void encSpeed(this ref compressor d)
        { 
            // We only compress if we have maxStoreBlockSize.
            if (d.windowEnd < maxStoreBlockSize)
            {
                if (!d.sync)
                {
                    return;
                } 

                // Handle small sizes.
                if (d.windowEnd < 128L)
                {

                    if (d.windowEnd == 0L) 
                        return;
                    else if (d.windowEnd <= 16L) 
                        d.err = d.writeStoredBlock(d.window[..d.windowEnd]);
                    else 
                        d.w.writeBlockHuff(false, d.window[..d.windowEnd]);
                        d.err = d.w.err;
                                        d.windowEnd = 0L;
                    d.bestSpeed.reset();
                    return;
                }
            } 
            // Encode the block.
            d.tokens = d.bestSpeed.encode(d.tokens[..0L], d.window[..d.windowEnd]); 

            // If we removed less than 1/16th, Huffman compress the block.
            if (len(d.tokens) > d.windowEnd - (d.windowEnd >> (int)(4L)))
            {
                d.w.writeBlockHuff(false, d.window[..d.windowEnd]);
            }
            else
            {
                d.w.writeBlockDynamic(d.tokens, false, d.window[..d.windowEnd]);
            }
            d.err = d.w.err;
            d.windowEnd = 0L;
        }

        private static void initDeflate(this ref compressor d)
        {
            d.window = make_slice<byte>(2L * windowSize);
            d.hashOffset = 1L;
            d.tokens = make_slice<token>(0L, maxFlateBlockTokens + 1L);
            d.length = minMatchLength - 1L;
            d.offset = 0L;
            d.byteAvailable = false;
            d.index = 0L;
            d.hash = 0L;
            d.chainHead = -1L;
            d.bulkHasher = bulkHash4;
        }

        private static void deflate(this ref compressor _d) => func(_d, (ref compressor d, Defer _, Panic panic, Recover __) =>
        {
            if (d.windowEnd - d.index < minMatchLength + maxMatchLength && !d.sync)
            {
                return;
            }
            d.maxInsertIndex = d.windowEnd - (minMatchLength - 1L);
            if (d.index < d.maxInsertIndex)
            {
                d.hash = hash4(d.window[d.index..d.index + minMatchLength]);
            }
Loop:
            while (true)
            {
                if (d.index > d.windowEnd)
                {
                    panic("index > windowEnd");
                }
                var lookahead = d.windowEnd - d.index;
                if (lookahead < minMatchLength + maxMatchLength)
                {
                    if (!d.sync)
                    {
                        _breakLoop = true;
                        break;
                    }
                    if (d.index > d.windowEnd)
                    {
                        panic("index > windowEnd");
                    }
                    if (lookahead == 0L)
                    { 
                        // Flush current output block if any.
                        if (d.byteAvailable)
                        { 
                            // There is still one pending token that needs to be flushed
                            d.tokens = append(d.tokens, literalToken(uint32(d.window[d.index - 1L])));
                            d.byteAvailable = false;
                        }
                        if (len(d.tokens) > 0L)
                        {
                            d.err = d.writeBlock(d.tokens, d.index);

                            if (d.err != null)
                            {
                                return;
                            }
                            d.tokens = d.tokens[..0L];
                        }
                        _breakLoop = true;
                        break;
                    }
                }
                if (d.index < d.maxInsertIndex)
                { 
                    // Update the hash
                    d.hash = hash4(d.window[d.index..d.index + minMatchLength]);
                    var hh = ref d.hashHead[d.hash & hashMask];
                    d.chainHead = int(hh.Value);
                    d.hashPrev[d.index & windowMask] = uint32(d.chainHead);
                    hh.Value = uint32(d.index + d.hashOffset);
                }
                var prevLength = d.length;
                var prevOffset = d.offset;
                d.length = minMatchLength - 1L;
                d.offset = 0L;
                var minIndex = d.index - windowSize;
                if (minIndex < 0L)
                {
                    minIndex = 0L;
                }
                if (d.chainHead - d.hashOffset >= minIndex && (d.fastSkipHashing != skipNever && lookahead > minMatchLength - 1L || d.fastSkipHashing == skipNever && lookahead > prevLength && prevLength < d.lazy))
                {
                    {
                        var (newLength, newOffset, ok) = d.findMatch(d.index, d.chainHead - d.hashOffset, minMatchLength - 1L, lookahead);

                        if (ok)
                        {
                            d.length = newLength;
                            d.offset = newOffset;
                        }

                    }
                }
                if (d.fastSkipHashing != skipNever && d.length >= minMatchLength || d.fastSkipHashing == skipNever && prevLength >= minMatchLength && d.length <= prevLength)
                { 
                    // There was a match at the previous step, and the current match is
                    // not better. Output the previous match.
                    if (d.fastSkipHashing != skipNever)
                    {
                        d.tokens = append(d.tokens, matchToken(uint32(d.length - baseMatchLength), uint32(d.offset - baseMatchOffset)));
                    }
                    else
                    {
                        d.tokens = append(d.tokens, matchToken(uint32(prevLength - baseMatchLength), uint32(prevOffset - baseMatchOffset)));
                    } 
                    // Insert in the hash table all strings up to the end of the match.
                    // index and index-1 are already inserted. If there is not enough
                    // lookahead, the last two strings are not inserted into the hash
                    // table.
                    if (d.length <= d.fastSkipHashing)
                    {
                        long newIndex = default;
                        if (d.fastSkipHashing != skipNever)
                        {
                            newIndex = d.index + d.length;
                        }
                        else
                        {
                            newIndex = d.index + prevLength - 1L;
                        }
                        d.index++;

                        while (d.index < newIndex)
                        {
                            if (d.index < d.maxInsertIndex)
                            {
                                d.hash = hash4(d.window[d.index..d.index + minMatchLength]); 
                                // Get previous value with the same hash.
                                // Our chain should point to the previous value.
                                hh = ref d.hashHead[d.hash & hashMask];
                                d.hashPrev[d.index & windowMask] = hh.Value; 
                                // Set the head of the hash chain to us.
                                hh.Value = uint32(d.index + d.hashOffset);
                            d.index++;
                            }
                        }
                    else

                        if (d.fastSkipHashing == skipNever)
                        {
                            d.byteAvailable = false;
                            d.length = minMatchLength - 1L;
                        }
                    }                    { 
                        // For matches this long, we don't bother inserting each individual
                        // item into the table.
                        d.index += d.length;
                        if (d.index < d.maxInsertIndex)
                        {
                            d.hash = hash4(d.window[d.index..d.index + minMatchLength]);
                        }
                    }
                else
                    if (len(d.tokens) == maxFlateBlockTokens)
                    { 
                        // The block includes the current character
                        d.err = d.writeBlock(d.tokens, d.index);

                        if (d.err != null)
                        {
                            return;
                        }
                        d.tokens = d.tokens[..0L];
                    }
                }                {
                    if (d.fastSkipHashing != skipNever || d.byteAvailable)
                    {
                        var i = d.index - 1L;
                        if (d.fastSkipHashing != skipNever)
                        {
                            i = d.index;
                        }
                        d.tokens = append(d.tokens, literalToken(uint32(d.window[i])));
                        if (len(d.tokens) == maxFlateBlockTokens)
                        {
                            d.err = d.writeBlock(d.tokens, i + 1L);

                            if (d.err != null)
                            {
                                return;
                            }
                            d.tokens = d.tokens[..0L];
                        }
                    }
                    d.index++;
                    if (d.fastSkipHashing == skipNever)
                    {
                        d.byteAvailable = true;
                    }
                }
            }
        });

        private static long fillStore(this ref compressor d, slice<byte> b)
        {
            var n = copy(d.window[d.windowEnd..], b);
            d.windowEnd += n;
            return n;
        }

        private static void store(this ref compressor d)
        {
            if (d.windowEnd > 0L && (d.windowEnd == maxStoreBlockSize || d.sync))
            {
                d.err = d.writeStoredBlock(d.window[..d.windowEnd]);
                d.windowEnd = 0L;
            }
        }

        // storeHuff compresses and stores the currently added data
        // when the d.window is full or we are at the end of the stream.
        // Any error that occurred will be in d.err
        private static void storeHuff(this ref compressor d)
        {
            if (d.windowEnd < len(d.window) && !d.sync || d.windowEnd == 0L)
            {
                return;
            }
            d.w.writeBlockHuff(false, d.window[..d.windowEnd]);
            d.err = d.w.err;
            d.windowEnd = 0L;
        }

        private static (long, error) write(this ref compressor d, slice<byte> b)
        {
            if (d.err != null)
            {
                return (0L, d.err);
            }
            n = len(b);
            while (len(b) > 0L)
            {
                d.step(d);
                b = b[d.fill(d, b)..];
                if (d.err != null)
                {
                    return (0L, d.err);
                }
            }

            return (n, null);
        }

        private static error syncFlush(this ref compressor d)
        {
            if (d.err != null)
            {
                return error.As(d.err);
            }
            d.sync = true;
            d.step(d);
            if (d.err == null)
            {
                d.w.writeStoredHeader(0L, false);
                d.w.flush();
                d.err = d.w.err;
            }
            d.sync = false;
            return error.As(d.err);
        }

        private static error init(this ref compressor d, io.Writer w, long level)
        {
            d.w = newHuffmanBitWriter(w);


            if (level == NoCompression)
            {
                d.window = make_slice<byte>(maxStoreBlockSize);
                d.fill = ref compressor;
                d.step = ref compressor;
                goto __switch_break0;
            }
            if (level == HuffmanOnly)
            {
                d.window = make_slice<byte>(maxStoreBlockSize);
                d.fill = ref compressor;
                d.step = ref compressor;
                goto __switch_break0;
            }
            if (level == BestSpeed)
            {
                d.compressionLevel = levels[level];
                d.window = make_slice<byte>(maxStoreBlockSize);
                d.fill = ref compressor;
                d.step = ref compressor;
                d.bestSpeed = newDeflateFast();
                d.tokens = make_slice<token>(maxStoreBlockSize);
                goto __switch_break0;
            }
            if (level == DefaultCompression)
            {
                level = 6L;
                fallthrough = true;
            }
            if (fallthrough || 2L <= level && level <= 9L)
            {
                d.compressionLevel = levels[level];
                d.initDeflate();
                d.fill = ref compressor;
                d.step = ref compressor;
                goto __switch_break0;
            }
            // default: 
                return error.As(fmt.Errorf("flate: invalid compression level %d: want value in range [-2, 9]", level));

            __switch_break0:;
            return error.As(null);
        }

        private static void reset(this ref compressor d, io.Writer w)
        {
            d.w.reset(w);
            d.sync = false;
            d.err = null;

            if (d.compressionLevel.level == NoCompression) 
                d.windowEnd = 0L;
            else if (d.compressionLevel.level == BestSpeed) 
                d.windowEnd = 0L;
                d.tokens = d.tokens[..0L];
                d.bestSpeed.reset();
            else 
                d.chainHead = -1L;
                {
                    var i__prev1 = i;

                    foreach (var (__i) in d.hashHead)
                    {
                        i = __i;
                        d.hashHead[i] = 0L;
                    }

                    i = i__prev1;
                }

                {
                    var i__prev1 = i;

                    foreach (var (__i) in d.hashPrev)
                    {
                        i = __i;
                        d.hashPrev[i] = 0L;
                    }

                    i = i__prev1;
                }

                d.hashOffset = 1L;
                d.index = 0L;
                d.windowEnd = 0L;
                d.blockStart = 0L;
                d.byteAvailable = false;
                d.tokens = d.tokens[..0L];
                d.length = minMatchLength - 1L;
                d.offset = 0L;
                d.hash = 0L;
                d.maxInsertIndex = 0L;
                    }

        private static error close(this ref compressor d)
        {
            if (d.err != null)
            {
                return error.As(d.err);
            }
            d.sync = true;
            d.step(d);
            if (d.err != null)
            {
                return error.As(d.err);
            }
            d.w.writeStoredHeader(0L, true);

            if (d.w.err != null)
            {
                return error.As(d.w.err);
            }
            d.w.flush();
            return error.As(d.w.err);
        }

        // NewWriter returns a new Writer compressing data at the given level.
        // Following zlib, levels range from 1 (BestSpeed) to 9 (BestCompression);
        // higher levels typically run slower but compress more. Level 0
        // (NoCompression) does not attempt any compression; it only adds the
        // necessary DEFLATE framing.
        // Level -1 (DefaultCompression) uses the default compression level.
        // Level -2 (HuffmanOnly) will use Huffman compression only, giving
        // a very fast compression for all types of input, but sacrificing considerable
        // compression efficiency.
        //
        // If level is in the range [-2, 9] then the error returned will be nil.
        // Otherwise the error returned will be non-nil.
        public static (ref Writer, error) NewWriter(io.Writer w, long level)
        {
            Writer dw = default;
            {
                var err = dw.d.init(w, level);

                if (err != null)
                {
                    return (null, err);
                }

            }
            return (ref dw, null);
        }

        // NewWriterDict is like NewWriter but initializes the new
        // Writer with a preset dictionary. The returned Writer behaves
        // as if the dictionary had been written to it without producing
        // any compressed output. The compressed data written to w
        // can only be decompressed by a Reader initialized with the
        // same dictionary.
        public static (ref Writer, error) NewWriterDict(io.Writer w, long level, slice<byte> dict)
        {
            dictWriter dw = ref new dictWriter(w);
            var (zw, err) = NewWriter(dw, level);
            if (err != null)
            {
                return (null, err);
            }
            zw.d.fillWindow(dict);
            zw.dict = append(zw.dict, dict); // duplicate dictionary for Reset method.
            return (zw, err);
        }

        private partial struct dictWriter
        {
            public io.Writer w;
        }

        private static (long, error) Write(this ref dictWriter w, slice<byte> b)
        {
            return w.w.Write(b);
        }

        // A Writer takes data written to it and writes the compressed
        // form of that data to an underlying writer (see NewWriter).
        public partial struct Writer
        {
            public compressor d;
            public slice<byte> dict;
        }

        // Write writes data to w, which will eventually write the
        // compressed form of data to its underlying writer.
        private static (long, error) Write(this ref Writer w, slice<byte> data)
        {
            return w.d.write(data);
        }

        // Flush flushes any pending data to the underlying writer.
        // It is useful mainly in compressed network protocols, to ensure that
        // a remote reader has enough data to reconstruct a packet.
        // Flush does not return until the data has been written.
        // Calling Flush when there is no pending data still causes the Writer
        // to emit a sync marker of at least 4 bytes.
        // If the underlying writer returns an error, Flush returns that error.
        //
        // In the terminology of the zlib library, Flush is equivalent to Z_SYNC_FLUSH.
        private static error Flush(this ref Writer w)
        { 
            // For more about flushing:
            // http://www.bolet.org/~pornin/deflate-flush.html
            return error.As(w.d.syncFlush());
        }

        // Close flushes and closes the writer.
        private static error Close(this ref Writer w)
        {
            return error.As(w.d.close());
        }

        // Reset discards the writer's state and makes it equivalent to
        // the result of NewWriter or NewWriterDict called with dst
        // and w's level and dictionary.
        private static void Reset(this ref Writer w, io.Writer dst)
        {
            {
                ref dictWriter (dw, ok) = w.d.w.writer._<ref dictWriter>();

                if (ok)
                { 
                    // w was created with NewWriterDict
                    dw.w = dst;
                    w.d.reset(dw);
                    w.d.fillWindow(w.dict);
                }
                else
                { 
                    // w was created with NewWriter
                    w.d.reset(dst);
                }

            }
        }
    }
}}
