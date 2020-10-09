// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package flate -- go2cs converted at 2020 October 09 04:50:13 UTC
// import "compress/flate" ==> using flate = go.compress.flate_package
// Original source: C:\Go\src\compress\flate\deflatefast.go
using math = go.math_package;
using static go.builtin;

namespace go {
namespace compress
{
    public static partial class flate_package
    {
        // This encoding algorithm, which prioritizes speed over output size, is
        // based on Snappy's LZ77-style encoder: github.com/golang/snappy
        private static readonly long tableBits = (long)14L; // Bits used in the table.
        private static readonly long tableSize = (long)1L << (int)(tableBits); // Size of the table.
        private static readonly var tableMask = tableSize - 1L; // Mask for table indices. Redundant, but can eliminate bounds checks.
        private static readonly long tableShift = (long)32L - tableBits; // Right-shift to get the tableBits most significant bits of a uint32.

        // Reset the buffer offset when reaching this.
        // Offsets are stored between blocks as int32 values.
        // Since the offset we are checking against is at the beginning
        // of the buffer, we need to subtract the current and input
        // buffer to not risk overflowing the int32.
        private static readonly var bufferReset = math.MaxInt32 - maxStoreBlockSize * 2L;


        private static uint load32(slice<byte> b, int i)
        {
            b = b.slice(i, i + 4L, len(b)); // Help the compiler eliminate bounds checks on the next line.
            return uint32(b[0L]) | uint32(b[1L]) << (int)(8L) | uint32(b[2L]) << (int)(16L) | uint32(b[3L]) << (int)(24L);

        }

        private static ulong load64(slice<byte> b, int i)
        {
            b = b.slice(i, i + 8L, len(b)); // Help the compiler eliminate bounds checks on the next line.
            return uint64(b[0L]) | uint64(b[1L]) << (int)(8L) | uint64(b[2L]) << (int)(16L) | uint64(b[3L]) << (int)(24L) | uint64(b[4L]) << (int)(32L) | uint64(b[5L]) << (int)(40L) | uint64(b[6L]) << (int)(48L) | uint64(b[7L]) << (int)(56L);

        }

        private static uint hash(uint u)
        {
            return (u * 0x1e35a7bdUL) >> (int)(tableShift);
        }

        // These constants are defined by the Snappy implementation so that its
        // assembly implementation can fast-path some 16-bytes-at-a-time copies. They
        // aren't necessary in the pure Go implementation, as we don't use those same
        // optimizations, but using the same thresholds doesn't really hurt.
        private static readonly long inputMargin = (long)16L - 1L;
        private static readonly long minNonLiteralBlockSize = (long)1L + 1L + inputMargin;


        private partial struct tableEntry
        {
            public uint val; // Value at destination
            public int offset;
        }

        // deflateFast maintains the table for matches,
        // and the previous byte block for cross block matching.
        private partial struct deflateFast
        {
            public array<tableEntry> table;
            public slice<byte> prev; // Previous block, zero length if unknown.
            public int cur; // Current match offset.
        }

        private static ptr<deflateFast> newDeflateFast()
        {
            return addr(new deflateFast(cur:maxStoreBlockSize,prev:make([]byte,0,maxStoreBlockSize)));
        }

        // encode encodes a block given in src and appends tokens
        // to dst and returns the result.
        private static slice<token> encode(this ptr<deflateFast> _addr_e, slice<token> dst, slice<byte> src)
        {
            ref deflateFast e = ref _addr_e.val;
 
            // Ensure that e.cur doesn't wrap.
            if (e.cur >= bufferReset)
            {
                e.shiftOffsets();
            } 

            // This check isn't in the Snappy implementation, but there, the caller
            // instead of the callee handles this case.
            if (len(src) < minNonLiteralBlockSize)
            {
                e.cur += maxStoreBlockSize;
                e.prev = e.prev[..0L];
                return emitLiteral(dst, src);
            } 

            // sLimit is when to stop looking for offset/length copies. The inputMargin
            // lets us use a fast path for emitLiteral in the main loop, while we are
            // looking for copies.
            var sLimit = int32(len(src) - inputMargin); 

            // nextEmit is where in src the next emitLiteral should start from.
            var nextEmit = int32(0L);
            var s = int32(0L);
            var cv = load32(src, s);
            var nextHash = hash(cv);

            while (true)
            { 
                // Copied from the C++ snappy implementation:
                //
                // Heuristic match skipping: If 32 bytes are scanned with no matches
                // found, start looking only at every other byte. If 32 more bytes are
                // scanned (or skipped), look at every third byte, etc.. When a match
                // is found, immediately go back to looking at every byte. This is a
                // small loss (~5% performance, ~0.1% density) for compressible data
                // due to more bookkeeping, but for non-compressible data (such as
                // JPEG) it's a huge win since the compressor quickly "realizes" the
                // data is incompressible and doesn't bother looking for matches
                // everywhere.
                //
                // The "skip" variable keeps track of how many bytes there are since
                // the last match; dividing it by 32 (ie. right-shifting by five) gives
                // the number of bytes to move ahead for each iteration.
                var skip = int32(32L);

                var nextS = s;
                tableEntry candidate = default;
                while (true)
                {
                    s = nextS;
                    var bytesBetweenHashLookups = skip >> (int)(5L);
                    nextS = s + bytesBetweenHashLookups;
                    skip += bytesBetweenHashLookups;
                    if (nextS > sLimit)
                    {
                        goto emitRemainder;
                    }

                    candidate = e.table[nextHash & tableMask];
                    var now = load32(src, nextS);
                    e.table[nextHash & tableMask] = new tableEntry(offset:s+e.cur,val:cv);
                    nextHash = hash(now);

                    var offset = s - (candidate.offset - e.cur);
                    if (offset > maxMatchOffset || cv != candidate.val)
                    { 
                        // Out of range or not matched.
                        cv = now;
                        continue;

                    }

                    break;

                } 

                // A 4-byte match has been found. We'll later see if more than 4 bytes
                // match. But, prior to the match, src[nextEmit:s] are unmatched. Emit
                // them as literal bytes.
 

                // A 4-byte match has been found. We'll later see if more than 4 bytes
                // match. But, prior to the match, src[nextEmit:s] are unmatched. Emit
                // them as literal bytes.
                dst = emitLiteral(dst, src[nextEmit..s]); 

                // Call emitCopy, and then see if another emitCopy could be our next
                // move. Repeat until we find no match for the input immediately after
                // what was consumed by the last emitCopy call.
                //
                // If we exit this loop normally then we need to call emitLiteral next,
                // though we don't yet know how big the literal will be. We handle that
                // by proceeding to the next iteration of the main loop. We also can
                // exit this loop via goto if we get close to exhausting the input.
                while (true)
                { 
                    // Invariant: we have a 4-byte match at s, and no need to emit any
                    // literal bytes prior to s.

                    // Extend the 4-byte match as long as possible.
                    //
                    s += 4L;
                    var t = candidate.offset - e.cur + 4L;
                    var l = e.matchLen(s, t, src); 

                    // matchToken is flate's equivalent of Snappy's emitCopy. (length,offset)
                    dst = append(dst, matchToken(uint32(l + 4L - baseMatchLength), uint32(s - t - baseMatchOffset)));
                    s += l;
                    nextEmit = s;
                    if (s >= sLimit)
                    {
                        goto emitRemainder;
                    } 

                    // We could immediately start working at s now, but to improve
                    // compression we first update the hash table at s-1 and at s. If
                    // another emitCopy is not our next move, also calculate nextHash
                    // at s+1. At least on GOARCH=amd64, these three hash calculations
                    // are faster as one load64 call (with some shifts) instead of
                    // three load32 calls.
                    var x = load64(src, s - 1L);
                    var prevHash = hash(uint32(x));
                    e.table[prevHash & tableMask] = new tableEntry(offset:e.cur+s-1,val:uint32(x));
                    x >>= 8L;
                    var currHash = hash(uint32(x));
                    candidate = e.table[currHash & tableMask];
                    e.table[currHash & tableMask] = new tableEntry(offset:e.cur+s,val:uint32(x));

                    offset = s - (candidate.offset - e.cur);
                    if (offset > maxMatchOffset || uint32(x) != candidate.val)
                    {
                        cv = uint32(x >> (int)(8L));
                        nextHash = hash(cv);
                        s++;
                        break;
                    }

                }


            }


emitRemainder:
            if (int(nextEmit) < len(src))
            {
                dst = emitLiteral(dst, src[nextEmit..]);
            }

            e.cur += int32(len(src));
            e.prev = e.prev[..len(src)];
            copy(e.prev, src);
            return dst;

        }

        private static slice<token> emitLiteral(slice<token> dst, slice<byte> lit)
        {
            foreach (var (_, v) in lit)
            {
                dst = append(dst, literalToken(uint32(v)));
            }
            return dst;

        }

        // matchLen returns the match length between src[s:] and src[t:].
        // t can be negative to indicate the match is starting in e.prev.
        // We assume that src[s-4:s] and src[t-4:t] already match.
        private static int matchLen(this ptr<deflateFast> _addr_e, int s, int t, slice<byte> src)
        {
            ref deflateFast e = ref _addr_e.val;

            var s1 = int(s) + maxMatchLength - 4L;
            if (s1 > len(src))
            {
                s1 = len(src);
            } 

            // If we are inside the current block
            if (t >= 0L)
            {
                var b = src[t..];
                var a = src[s..s1];
                b = b[..len(a)]; 
                // Extend the match to be as long as possible.
                {
                    var i__prev1 = i;

                    foreach (var (__i) in a)
                    {
                        i = __i;
                        if (a[i] != b[i])
                        {
                            return int32(i);
                        }

                    }

                    i = i__prev1;
                }

                return int32(len(a));

            } 

            // We found a match in the previous block.
            var tp = int32(len(e.prev)) + t;
            if (tp < 0L)
            {
                return 0L;
            } 

            // Extend the match to be as long as possible.
            a = src[s..s1];
            b = e.prev[tp..];
            if (len(b) > len(a))
            {
                b = b[..len(a)];
            }

            a = a[..len(b)];
            {
                var i__prev1 = i;

                foreach (var (__i) in b)
                {
                    i = __i;
                    if (a[i] != b[i])
                    {
                        return int32(i);
                    }

                } 

                // If we reached our limit, we matched everything we are
                // allowed to in the previous block and we return.

                i = i__prev1;
            }

            var n = int32(len(b));
            if (int(s + n) == s1)
            {
                return n;
            } 

            // Continue looking for more matches in the current block.
            a = src[s + n..s1];
            b = src[..len(a)];
            {
                var i__prev1 = i;

                foreach (var (__i) in a)
                {
                    i = __i;
                    if (a[i] != b[i])
                    {
                        return int32(i) + n;
                    }

                }

                i = i__prev1;
            }

            return int32(len(a)) + n;

        }

        // Reset resets the encoding history.
        // This ensures that no matches are made to the previous block.
        private static void reset(this ptr<deflateFast> _addr_e)
        {
            ref deflateFast e = ref _addr_e.val;

            e.prev = e.prev[..0L]; 
            // Bump the offset, so all matches will fail distance check.
            e.cur += maxMatchOffset; 

            // Protect against e.cur wraparound.
            if (e.cur >= bufferReset)
            {
                e.shiftOffsets();
            }

        }

        // shiftOffsets will shift down all match offset.
        // This is only called in rare situations to prevent integer overflow.
        //
        // See https://golang.org/issue/18636 and https://github.com/golang/go/issues/34121.
        private static void shiftOffsets(this ptr<deflateFast> _addr_e)
        {
            ref deflateFast e = ref _addr_e.val;

            if (len(e.prev) == 0L)
            { 
                // We have no history; just clear the table.
                {
                    var i__prev1 = i;

                    foreach (var (__i) in e.table[..])
                    {
                        i = __i;
                        e.table[i] = new tableEntry();
                    }

                    i = i__prev1;
                }

                e.cur = maxMatchOffset;
                return ;

            } 

            // Shift down everything in the table that isn't already too far away.
            {
                var i__prev1 = i;

                foreach (var (__i) in e.table[..])
                {
                    i = __i;
                    var v = e.table[i].offset - e.cur + maxMatchOffset;
                    if (v < 0L)
                    {
                        v = 0L;
                    }

                    e.table[i].offset = v;

                }

                i = i__prev1;
            }

            e.cur = maxMatchOffset;

        }
    }
}}
