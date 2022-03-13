// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package flate -- go2cs converted at 2022 March 13 05:28:55 UTC
// import "compress/flate" ==> using flate = go.compress.flate_package
// Original source: C:\Program Files\Go\src\compress\flate\deflate.go
namespace go.compress;

using fmt = fmt_package;
using io = io_package;
using math = math_package;
using System;

public static partial class flate_package {

public static readonly nint NoCompression = 0;
public static readonly nint BestSpeed = 1;
public static readonly nint BestCompression = 9;
public static readonly nint DefaultCompression = -1; 

// HuffmanOnly disables Lempel-Ziv match searching and only performs Huffman
// entropy encoding. This mode is useful in compressing data that has
// already been compressed with an LZ style algorithm (e.g. Snappy or LZ4)
// that lacks an entropy encoder. Compression gains are achieved when
// certain bytes in the input stream occur more frequently than others.
//
// Note that HuffmanOnly produces a compressed output that is
// RFC 1951 compliant. That is, any valid DEFLATE decompressor will
// continue to be able to decompress this output.
public static readonly nint HuffmanOnly = -2;

private static readonly nint logWindowSize = 15;
private static readonly nint windowSize = 1 << (int)(logWindowSize);
private static readonly var windowMask = windowSize - 1; 

// The LZ77 step produces a sequence of literal tokens and <length, offset>
// pair tokens. The offset is also known as distance. The underlying wire
// format limits the range of lengths and offsets. For example, there are
// 256 legitimate lengths: those in the range [3, 258]. This package's
// compressor uses a higher minimum match length, enabling optimizations
// such as finding matches via 32-bit loads and compares.
private static readonly nint baseMatchLength = 3; // The smallest match length per the RFC section 3.2.5
private static readonly nint minMatchLength = 4; // The smallest match length that the compressor actually emits
private static readonly nint maxMatchLength = 258; // The largest match length
private static readonly nint baseMatchOffset = 1; // The smallest match offset
private static readonly nint maxMatchOffset = 1 << 15; // The largest match offset

// The maximum number of tokens we put into a single flate block, just to
// stop things from getting too large.
private static readonly nint maxFlateBlockTokens = 1 << 14;
private static readonly nint maxStoreBlockSize = 65535;
private static readonly nint hashBits = 17; // After 17 performance degrades
private static readonly nint hashSize = 1 << (int)(hashBits);
private static readonly nint hashMask = (1 << (int)(hashBits)) - 1;
private static readonly nint maxHashOffset = 1 << 24;

private static readonly var skipNever = math.MaxInt32;

private partial struct compressionLevel {
    public nint level;
    public nint good;
    public nint lazy;
    public nint nice;
    public nint chain;
    public nint fastSkipHashing;
}

private static compressionLevel levels = new slice<compressionLevel>(new compressionLevel[] { {0,0,0,0,0,0}, {1,0,0,0,0,0}, {2,4,0,16,8,5}, {3,4,0,32,32,6}, {4,4,4,16,16,skipNever}, {5,8,16,32,32,skipNever}, {6,8,16,128,128,skipNever}, {7,8,32,128,256,skipNever}, {8,32,128,258,1024,skipNever}, {9,32,258,258,4096,skipNever} });

private partial struct compressor {
    public ref compressionLevel compressionLevel => ref compressionLevel_val;
    public ptr<huffmanBitWriter> w;
    public Action<slice<byte>, slice<uint>> bulkHasher; // compression algorithm
    public Func<ptr<compressor>, slice<byte>, nint> fill; // copy data to window
    public Action<ptr<compressor>> step; // process window
    public bool sync; // requesting flush
    public ptr<deflateFast> bestSpeed; // Encoder for BestSpeed

// Input hash chains
// hashHead[hashValue] contains the largest inputIndex with the specified hash value
// If hashHead[hashValue] is within the current window, then
// hashPrev[hashHead[hashValue] & windowMask] contains the previous index
// with the same hash value.
    public nint chainHead;
    public array<uint> hashHead;
    public array<uint> hashPrev;
    public nint hashOffset; // input window: unprocessed data is window[index:windowEnd]
    public nint index;
    public slice<byte> window;
    public nint windowEnd;
    public nint blockStart; // window index where current tokens start
    public bool byteAvailable; // if true, still need to process window[index-1].

// queued output tokens
    public slice<token> tokens; // deflate state
    public nint length;
    public nint offset;
    public uint hash;
    public nint maxInsertIndex;
    public error err; // hashMatch must be able to contain hashes for the maximum match length.
    public array<uint> hashMatch;
}

private static nint fillDeflate(this ptr<compressor> _addr_d, slice<byte> b) {
    ref compressor d = ref _addr_d.val;

    if (d.index >= 2 * windowSize - (minMatchLength + maxMatchLength)) { 
        // shift the window by windowSize
        copy(d.window, d.window[(int)windowSize..(int)2 * windowSize]);
        d.index -= windowSize;
        d.windowEnd -= windowSize;
        if (d.blockStart >= windowSize) {
            d.blockStart -= windowSize;
        }
        else
 {
            d.blockStart = math.MaxInt32;
        }
        d.hashOffset += windowSize;
        if (d.hashOffset > maxHashOffset) {
            var delta = d.hashOffset - 1;
            d.hashOffset -= delta;
            d.chainHead -= delta; 

            // Iterate over slices instead of arrays to avoid copying
            // the entire table onto the stack (Issue #18625).
            {
                var i__prev1 = i;
                var v__prev1 = v;

                foreach (var (__i, __v) in d.hashPrev[..]) {
                    i = __i;
                    v = __v;
                    if (int(v) > delta) {
                        d.hashPrev[i] = uint32(int(v) - delta);
                    }
                    else
 {
                        d.hashPrev[i] = 0;
                    }
                }

                i = i__prev1;
                v = v__prev1;
            }

            {
                var i__prev1 = i;
                var v__prev1 = v;

                foreach (var (__i, __v) in d.hashHead[..]) {
                    i = __i;
                    v = __v;
                    if (int(v) > delta) {
                        d.hashHead[i] = uint32(int(v) - delta);
                    }
                    else
 {
                        d.hashHead[i] = 0;
                    }
                }

                i = i__prev1;
                v = v__prev1;
            }
        }
    }
    var n = copy(d.window[(int)d.windowEnd..], b);
    d.windowEnd += n;
    return n;
}

private static error writeBlock(this ptr<compressor> _addr_d, slice<token> tokens, nint index) {
    ref compressor d = ref _addr_d.val;

    if (index > 0) {
        slice<byte> window = default;
        if (d.blockStart <= index) {
            window = d.window[(int)d.blockStart..(int)index];
        }
        d.blockStart = index;
        d.w.writeBlock(tokens, false, window);
        return error.As(d.w.err)!;
    }
    return error.As(null!)!;
}

// fillWindow will fill the current window with the supplied
// dictionary and calculate all hashes.
// This is much faster than doing a full encode.
// Should only be used after a reset.
private static void fillWindow(this ptr<compressor> _addr_d, slice<byte> b) => func((_, panic, _) => {
    ref compressor d = ref _addr_d.val;
 
    // Do not fill window if we are in store-only mode.
    if (d.compressionLevel.level < 2) {
        return ;
    }
    if (d.index != 0 || d.windowEnd != 0) {
        panic("internal error: fillWindow called with stale data");
    }
    if (len(b) > windowSize) {
        b = b[(int)len(b) - windowSize..];
    }
    var n = copy(d.window, b); 

    // Calculate 256 hashes at the time (more L1 cache hits)
    var loops = (n + 256 - minMatchLength) / 256;
    for (nint j = 0; j < loops; j++) {
        var index = j * 256;
        var end = index + 256 + minMatchLength - 1;
        if (end > n) {
            end = n;
        }
        var toCheck = d.window[(int)index..(int)end];
        var dstSize = len(toCheck) - minMatchLength + 1;

        if (dstSize <= 0) {
            continue;
        }
        var dst = d.hashMatch[..(int)dstSize];
        d.bulkHasher(toCheck, dst);
        uint newH = default;
        foreach (var (i, val) in dst) {
            var di = i + index;
            newH = val;
            var hh = _addr_d.hashHead[newH & hashMask]; 
            // Get previous value with the same hash.
            // Our chain should point to the previous value.
            d.hashPrev[di & windowMask] = hh.val; 
            // Set the head of the hash chain to us.
            hh.val = uint32(di + d.hashOffset);
        }        d.hash = newH;
    } 
    // Update window information.
    d.windowEnd = n;
    d.index = n;
});

// Try to find a match starting at index whose length is greater than prevSize.
// We only look at chainCount possibilities before giving up.
private static (nint, nint, bool) findMatch(this ptr<compressor> _addr_d, nint pos, nint prevHead, nint prevLength, nint lookahead) {
    nint length = default;
    nint offset = default;
    bool ok = default;
    ref compressor d = ref _addr_d.val;

    var minMatchLook = maxMatchLength;
    if (lookahead < minMatchLook) {
        minMatchLook = lookahead;
    }
    var win = d.window[(int)0..(int)pos + minMatchLook]; 

    // We quit when we get a match that's at least nice long
    var nice = len(win) - pos;
    if (d.nice < nice) {
        nice = d.nice;
    }
    var tries = d.chain;
    length = prevLength;
    if (length >= d.good) {
        tries>>=2;
    }
    var wEnd = win[pos + length];
    var wPos = win[(int)pos..];
    var minIndex = pos - windowSize;

    for (var i = prevHead; tries > 0; tries--) {
        if (wEnd == win[i + length]) {
            var n = matchLen(win[(int)i..], wPos, minMatchLook);

            if (n > length && (n > minMatchLength || pos - i <= 4096)) {
                length = n;
                offset = pos - i;
                ok = true;
                if (n >= nice) { 
                    // The match is good enough that we don't try to find a better one.
                    break;
                }
                wEnd = win[pos + n];
            }
        }
        if (i == minIndex) { 
            // hashPrev[i & windowMask] has already been overwritten, so stop now.
            break;
        }
        i = int(d.hashPrev[i & windowMask]) - d.hashOffset;
        if (i < minIndex || i < 0) {
            break;
        }
    }
    return ;
}

private static error writeStoredBlock(this ptr<compressor> _addr_d, slice<byte> buf) {
    ref compressor d = ref _addr_d.val;

    d.w.writeStoredHeader(len(buf), false);

    if (d.w.err != null) {
        return error.As(d.w.err)!;
    }
    d.w.writeBytes(buf);
    return error.As(d.w.err)!;
}

private static readonly nuint hashmul = 0x1e35a7bd;

// hash4 returns a hash representation of the first 4 bytes
// of the supplied slice.
// The caller must ensure that len(b) >= 4.


// hash4 returns a hash representation of the first 4 bytes
// of the supplied slice.
// The caller must ensure that len(b) >= 4.
private static uint hash4(slice<byte> b) {
    return ((uint32(b[3]) | uint32(b[2]) << 8 | uint32(b[1]) << 16 | uint32(b[0]) << 24) * hashmul) >> (int)((32 - hashBits));
}

// bulkHash4 will compute hashes using the same
// algorithm as hash4
private static void bulkHash4(slice<byte> b, slice<uint> dst) {
    if (len(b) < minMatchLength) {
        return ;
    }
    var hb = uint32(b[3]) | uint32(b[2]) << 8 | uint32(b[1]) << 16 | uint32(b[0]) << 24;
    dst[0] = (hb * hashmul) >> (int)((32 - hashBits));
    var end = len(b) - minMatchLength + 1;
    for (nint i = 1; i < end; i++) {
        hb = (hb << 8) | uint32(b[i + 3]);
        dst[i] = (hb * hashmul) >> (int)((32 - hashBits));
    }
}

// matchLen returns the number of matching bytes in a and b
// up to length 'max'. Both slices must be at least 'max'
// bytes in size.
private static nint matchLen(slice<byte> a, slice<byte> b, nint max) {
    a = a[..(int)max];
    b = b[..(int)len(a)];
    foreach (var (i, av) in a) {
        if (b[i] != av) {
            return i;
        }
    }    return max;
}

// encSpeed will compress and store the currently added data,
// if enough has been accumulated or we at the end of the stream.
// Any error that occurred will be in d.err
private static void encSpeed(this ptr<compressor> _addr_d) {
    ref compressor d = ref _addr_d.val;
 
    // We only compress if we have maxStoreBlockSize.
    if (d.windowEnd < maxStoreBlockSize) {
        if (!d.sync) {
            return ;
        }
        if (d.windowEnd < 128) {

            if (d.windowEnd == 0) 
                return ;
            else if (d.windowEnd <= 16) 
                d.err = d.writeStoredBlock(d.window[..(int)d.windowEnd]);
            else 
                d.w.writeBlockHuff(false, d.window[..(int)d.windowEnd]);
                d.err = d.w.err;
                        d.windowEnd = 0;
            d.bestSpeed.reset();
            return ;
        }
    }
    d.tokens = d.bestSpeed.encode(d.tokens[..(int)0], d.window[..(int)d.windowEnd]); 

    // If we removed less than 1/16th, Huffman compress the block.
    if (len(d.tokens) > d.windowEnd - (d.windowEnd >> 4)) {
        d.w.writeBlockHuff(false, d.window[..(int)d.windowEnd]);
    }
    else
 {
        d.w.writeBlockDynamic(d.tokens, false, d.window[..(int)d.windowEnd]);
    }
    d.err = d.w.err;
    d.windowEnd = 0;
}

private static void initDeflate(this ptr<compressor> _addr_d) {
    ref compressor d = ref _addr_d.val;

    d.window = make_slice<byte>(2 * windowSize);
    d.hashOffset = 1;
    d.tokens = make_slice<token>(0, maxFlateBlockTokens + 1);
    d.length = minMatchLength - 1;
    d.offset = 0;
    d.byteAvailable = false;
    d.index = 0;
    d.hash = 0;
    d.chainHead = -1;
    d.bulkHasher = bulkHash4;
}

private static void deflate(this ptr<compressor> _addr_d) => func((_, panic, _) => {
    ref compressor d = ref _addr_d.val;

    if (d.windowEnd - d.index < minMatchLength + maxMatchLength && !d.sync) {
        return ;
    }
    d.maxInsertIndex = d.windowEnd - (minMatchLength - 1);
    if (d.index < d.maxInsertIndex) {
        d.hash = hash4(d.window[(int)d.index..(int)d.index + minMatchLength]);
    }
Loop:
    while (true) {
        if (d.index > d.windowEnd) {
            panic("index > windowEnd");
        }
        var lookahead = d.windowEnd - d.index;
        if (lookahead < minMatchLength + maxMatchLength) {
            if (!d.sync) {
                _breakLoop = true;
                break;
            }
            if (d.index > d.windowEnd) {
                panic("index > windowEnd");
            }
            if (lookahead == 0) { 
                // Flush current output block if any.
                if (d.byteAvailable) { 
                    // There is still one pending token that needs to be flushed
                    d.tokens = append(d.tokens, literalToken(uint32(d.window[d.index - 1])));
                    d.byteAvailable = false;
                }
                if (len(d.tokens) > 0) {
                    d.err = d.writeBlock(d.tokens, d.index);

                    if (d.err != null) {
                        return ;
                    }
                    d.tokens = d.tokens[..(int)0];
                }
                _breakLoop = true;
                break;
            }
        }
        if (d.index < d.maxInsertIndex) { 
            // Update the hash
            d.hash = hash4(d.window[(int)d.index..(int)d.index + minMatchLength]);
            var hh = _addr_d.hashHead[d.hash & hashMask];
            d.chainHead = int(hh.val);
            d.hashPrev[d.index & windowMask] = uint32(d.chainHead);
            hh.val = uint32(d.index + d.hashOffset);
        }
        var prevLength = d.length;
        var prevOffset = d.offset;
        d.length = minMatchLength - 1;
        d.offset = 0;
        var minIndex = d.index - windowSize;
        if (minIndex < 0) {
            minIndex = 0;
        }
        if (d.chainHead - d.hashOffset >= minIndex && (d.fastSkipHashing != skipNever && lookahead > minMatchLength - 1 || d.fastSkipHashing == skipNever && lookahead > prevLength && prevLength < d.lazy)) {
            {
                var (newLength, newOffset, ok) = d.findMatch(d.index, d.chainHead - d.hashOffset, minMatchLength - 1, lookahead);

                if (ok) {
                    d.length = newLength;
                    d.offset = newOffset;
                }

            }
        }
        if (d.fastSkipHashing != skipNever && d.length >= minMatchLength || d.fastSkipHashing == skipNever && prevLength >= minMatchLength && d.length <= prevLength) { 
            // There was a match at the previous step, and the current match is
            // not better. Output the previous match.
            if (d.fastSkipHashing != skipNever) {
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
            if (d.length <= d.fastSkipHashing) {
                nint newIndex = default;
                if (d.fastSkipHashing != skipNever) {
                    newIndex = d.index + d.length;
                }
                else
 {
                    newIndex = d.index + prevLength - 1;
                }
                var index = d.index;
                index++;

                while (index < newIndex) {
                    if (index < d.maxInsertIndex) {
                        d.hash = hash4(d.window[(int)index..(int)index + minMatchLength]); 
                        // Get previous value with the same hash.
                        // Our chain should point to the previous value.
                        hh = _addr_d.hashHead[d.hash & hashMask];
                        d.hashPrev[index & windowMask] = hh.val; 
                        // Set the head of the hash chain to us.
                        hh.val = uint32(index + d.hashOffset);
                    index++;
                    }
                }
            else

                d.index = index;

                if (d.fastSkipHashing == skipNever) {
                    d.byteAvailable = false;
                    d.length = minMatchLength - 1;
                }
            } { 
                // For matches this long, we don't bother inserting each individual
                // item into the table.
                d.index += d.length;
                if (d.index < d.maxInsertIndex) {
                    d.hash = hash4(d.window[(int)d.index..(int)d.index + minMatchLength]);
                }
            }
        else
            if (len(d.tokens) == maxFlateBlockTokens) { 
                // The block includes the current character
                d.err = d.writeBlock(d.tokens, d.index);

                if (d.err != null) {
                    return ;
                }
                d.tokens = d.tokens[..(int)0];
            }
        } {
            if (d.fastSkipHashing != skipNever || d.byteAvailable) {
                var i = d.index - 1;
                if (d.fastSkipHashing != skipNever) {
                    i = d.index;
                }
                d.tokens = append(d.tokens, literalToken(uint32(d.window[i])));
                if (len(d.tokens) == maxFlateBlockTokens) {
                    d.err = d.writeBlock(d.tokens, i + 1);

                    if (d.err != null) {
                        return ;
                    }
                    d.tokens = d.tokens[..(int)0];
                }
            }
            d.index++;
            if (d.fastSkipHashing == skipNever) {
                d.byteAvailable = true;
            }
        }
    }
});

private static nint fillStore(this ptr<compressor> _addr_d, slice<byte> b) {
    ref compressor d = ref _addr_d.val;

    var n = copy(d.window[(int)d.windowEnd..], b);
    d.windowEnd += n;
    return n;
}

private static void store(this ptr<compressor> _addr_d) {
    ref compressor d = ref _addr_d.val;

    if (d.windowEnd > 0 && (d.windowEnd == maxStoreBlockSize || d.sync)) {
        d.err = d.writeStoredBlock(d.window[..(int)d.windowEnd]);
        d.windowEnd = 0;
    }
}

// storeHuff compresses and stores the currently added data
// when the d.window is full or we are at the end of the stream.
// Any error that occurred will be in d.err
private static void storeHuff(this ptr<compressor> _addr_d) {
    ref compressor d = ref _addr_d.val;

    if (d.windowEnd < len(d.window) && !d.sync || d.windowEnd == 0) {
        return ;
    }
    d.w.writeBlockHuff(false, d.window[..(int)d.windowEnd]);
    d.err = d.w.err;
    d.windowEnd = 0;
}

private static (nint, error) write(this ptr<compressor> _addr_d, slice<byte> b) {
    nint n = default;
    error err = default!;
    ref compressor d = ref _addr_d.val;

    if (d.err != null) {
        return (0, error.As(d.err)!);
    }
    n = len(b);
    while (len(b) > 0) {
        d.step(d);
        b = b[(int)d.fill(d, b)..];
        if (d.err != null) {
            return (0, error.As(d.err)!);
        }
    }
    return (n, error.As(null!)!);
}

private static error syncFlush(this ptr<compressor> _addr_d) {
    ref compressor d = ref _addr_d.val;

    if (d.err != null) {
        return error.As(d.err)!;
    }
    d.sync = true;
    d.step(d);
    if (d.err == null) {
        d.w.writeStoredHeader(0, false);
        d.w.flush();
        d.err = d.w.err;
    }
    d.sync = false;
    return error.As(d.err)!;
}

private static error init(this ptr<compressor> _addr_d, io.Writer w, nint level) {
    error err = default!;
    ref compressor d = ref _addr_d.val;

    d.w = newHuffmanBitWriter(w);


    if (level == NoCompression)
    {
        d.window = make_slice<byte>(maxStoreBlockSize);
        d.fill = (compressor.val).fillStore;
        d.step = (compressor.val).store;
        goto __switch_break0;
    }
    if (level == HuffmanOnly)
    {
        d.window = make_slice<byte>(maxStoreBlockSize);
        d.fill = (compressor.val).fillStore;
        d.step = (compressor.val).storeHuff;
        goto __switch_break0;
    }
    if (level == BestSpeed)
    {
        d.compressionLevel = levels[level];
        d.window = make_slice<byte>(maxStoreBlockSize);
        d.fill = (compressor.val).fillStore;
        d.step = (compressor.val).encSpeed;
        d.bestSpeed = newDeflateFast();
        d.tokens = make_slice<token>(maxStoreBlockSize);
        goto __switch_break0;
    }
    if (level == DefaultCompression)
    {
        level = 6;
        fallthrough = true;
    }
    if (fallthrough || 2 <= level && level <= 9)
    {
        d.compressionLevel = levels[level];
        d.initDeflate();
        d.fill = (compressor.val).fillDeflate;
        d.step = (compressor.val).deflate;
        goto __switch_break0;
    }
    // default: 
        return error.As(fmt.Errorf("flate: invalid compression level %d: want value in range [-2, 9]", level))!;

    __switch_break0:;
    return error.As(null!)!;
}

private static void reset(this ptr<compressor> _addr_d, io.Writer w) {
    ref compressor d = ref _addr_d.val;

    d.w.reset(w);
    d.sync = false;
    d.err = null;

    if (d.compressionLevel.level == NoCompression) 
        d.windowEnd = 0;
    else if (d.compressionLevel.level == BestSpeed) 
        d.windowEnd = 0;
        d.tokens = d.tokens[..(int)0];
        d.bestSpeed.reset();
    else 
        d.chainHead = -1;
        {
            var i__prev1 = i;

            foreach (var (__i) in d.hashHead) {
                i = __i;
                d.hashHead[i] = 0;
            }

            i = i__prev1;
        }

        {
            var i__prev1 = i;

            foreach (var (__i) in d.hashPrev) {
                i = __i;
                d.hashPrev[i] = 0;
            }

            i = i__prev1;
        }

        d.hashOffset = 1;
        (d.index, d.windowEnd) = (0, 0);        (d.blockStart, d.byteAvailable) = (0, false);        d.tokens = d.tokens[..(int)0];
        d.length = minMatchLength - 1;
        d.offset = 0;
        d.hash = 0;
        d.maxInsertIndex = 0;
    }

private static error close(this ptr<compressor> _addr_d) {
    ref compressor d = ref _addr_d.val;

    if (d.err != null) {
        return error.As(d.err)!;
    }
    d.sync = true;
    d.step(d);
    if (d.err != null) {
        return error.As(d.err)!;
    }
    d.w.writeStoredHeader(0, true);

    if (d.w.err != null) {
        return error.As(d.w.err)!;
    }
    d.w.flush();
    return error.As(d.w.err)!;
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
public static (ptr<Writer>, error) NewWriter(io.Writer w, nint level) {
    ptr<Writer> _p0 = default!;
    error _p0 = default!;

    ref Writer dw = ref heap(out ptr<Writer> _addr_dw);
    {
        var err = dw.d.init(w, level);

        if (err != null) {
            return (_addr_null!, error.As(err)!);
        }
    }
    return (_addr__addr_dw!, error.As(null!)!);
}

// NewWriterDict is like NewWriter but initializes the new
// Writer with a preset dictionary. The returned Writer behaves
// as if the dictionary had been written to it without producing
// any compressed output. The compressed data written to w
// can only be decompressed by a Reader initialized with the
// same dictionary.
public static (ptr<Writer>, error) NewWriterDict(io.Writer w, nint level, slice<byte> dict) {
    ptr<Writer> _p0 = default!;
    error _p0 = default!;

    ptr<dictWriter> dw = addr(new dictWriter(w));
    var (zw, err) = NewWriter(dw, level);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    zw.d.fillWindow(dict);
    zw.dict = append(zw.dict, dict); // duplicate dictionary for Reset method.
    return (_addr_zw!, error.As(err)!);
}

private partial struct dictWriter {
    public io.Writer w;
}

private static (nint, error) Write(this ptr<dictWriter> _addr_w, slice<byte> b) {
    nint n = default;
    error err = default!;
    ref dictWriter w = ref _addr_w.val;

    return w.w.Write(b);
}

// A Writer takes data written to it and writes the compressed
// form of that data to an underlying writer (see NewWriter).
public partial struct Writer {
    public compressor d;
    public slice<byte> dict;
}

// Write writes data to w, which will eventually write the
// compressed form of data to its underlying writer.
private static (nint, error) Write(this ptr<Writer> _addr_w, slice<byte> data) {
    nint n = default;
    error err = default!;
    ref Writer w = ref _addr_w.val;

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
private static error Flush(this ptr<Writer> _addr_w) {
    ref Writer w = ref _addr_w.val;
 
    // For more about flushing:
    // https://www.bolet.org/~pornin/deflate-flush.html
    return error.As(w.d.syncFlush())!;
}

// Close flushes and closes the writer.
private static error Close(this ptr<Writer> _addr_w) {
    ref Writer w = ref _addr_w.val;

    return error.As(w.d.close())!;
}

// Reset discards the writer's state and makes it equivalent to
// the result of NewWriter or NewWriterDict called with dst
// and w's level and dictionary.
private static void Reset(this ptr<Writer> _addr_w, io.Writer dst) {
    ref Writer w = ref _addr_w.val;

    {
        ptr<dictWriter> (dw, ok) = w.d.w.writer._<ptr<dictWriter>>();

        if (ok) { 
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

} // end flate_package
