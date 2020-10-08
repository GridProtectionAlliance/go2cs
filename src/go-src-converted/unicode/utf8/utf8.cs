// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package utf8 implements functions and constants to support text encoded in
// UTF-8. It includes functions to translate between runes and UTF-8 byte sequences.
// See https://en.wikipedia.org/wiki/UTF-8
// package utf8 -- go2cs converted at 2020 October 08 00:33:52 UTC
// import "unicode/utf8" ==> using utf8 = go.unicode.utf8_package
// Original source: C:\Go\src\unicode\utf8\utf8.go

using static go.builtin;

namespace go {
namespace unicode
{
    public static partial class utf8_package
    {
        // The conditions RuneError==unicode.ReplacementChar and
        // MaxRune==unicode.MaxRune are verified in the tests.
        // Defining them locally avoids this package depending on package unicode.

        // Numbers fundamental to the encoding.
        public static readonly char RuneError = (char)'\uFFFD'; // the "error" Rune or "Unicode replacement character"
        public static readonly ulong RuneSelf = (ulong)0x80UL; // characters below RuneSelf are represented as themselves in a single byte.
        public static readonly char MaxRune = (char)'\U0010FFFF'; // Maximum valid Unicode code point.
        public static readonly long UTFMax = (long)4L; // maximum number of bytes of a UTF-8 encoded Unicode character.

        // Code points in the surrogate range are not valid for UTF-8.
        private static readonly ulong surrogateMin = (ulong)0xD800UL;
        private static readonly ulong surrogateMax = (ulong)0xDFFFUL;


        private static readonly long t1 = (long)0L;        private static readonly tx b00000000 = (tx)0L;        private static readonly t2 b10000000 = (t2)0L;        private static readonly t3 b11000000 = (t3)0L;        private static readonly t4 b11100000 = (t4)0L;        private static readonly t5 b11110000 = (t5)0L;        private static readonly maskx b11111000 = (maskx)0L;        private static readonly mask2 b00111111 = (mask2)0L;        private static readonly mask3 b00011111 = (mask3)0L;        private static readonly mask4 b00001111 = (mask4)0L;        private static readonly rune1Max b00000111 = (rune1Max)1L << (int)(7L) - 1L;
        private static readonly long rune2Max = (long)1L << (int)(11L) - 1L;
        private static readonly long rune3Max = (long)1L << (int)(16L) - 1L; 

        // The default lowest and highest continuation byte.
        private static readonly long locb = (long)0L;        private static readonly hicb b10000000 = (hicb)0L;        private static readonly xx b10111111 = (xx)0xF1UL; // invalid: size 1
        private static readonly ulong as = (ulong)0xF0UL; // ASCII: size 1
        private static readonly ulong s1 = (ulong)0x02UL; // accept 0, size 2
        private static readonly ulong s2 = (ulong)0x13UL; // accept 1, size 3
        private static readonly ulong s3 = (ulong)0x03UL; // accept 0, size 3
        private static readonly ulong s4 = (ulong)0x23UL; // accept 2, size 3
        private static readonly ulong s5 = (ulong)0x34UL; // accept 3, size 4
        private static readonly ulong s6 = (ulong)0x04UL; // accept 0, size 4
        private static readonly ulong s7 = (ulong)0x44UL; // accept 4, size 4

        // first is information about the first byte in a UTF-8 sequence.
        private static array<byte> first = new array<byte>(new byte[] { as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s2, s3, s3, s3, s3, s3, s3, s3, s3, s3, s3, s3, s3, s4, s3, s3, s5, s6, s6, s6, s7, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx });

        // acceptRange gives the range of valid values for the second byte in a UTF-8
        // sequence.
        private partial struct acceptRange
        {
            public byte lo; // lowest value for second byte.
            public byte hi; // highest value for second byte.
        }

        // acceptRanges has size 16 to avoid bounds checks in the code that uses it.
        private static array<acceptRange> acceptRanges = new array<acceptRange>(InitKeyedValues<acceptRange>(16, (0, {locb,hicb}), (1, {0xA0,hicb}), (2, {locb,0x9F}), (3, {0x90,hicb}), (4, {locb,0x8F})));

        // FullRune reports whether the bytes in p begin with a full UTF-8 encoding of a rune.
        // An invalid encoding is considered a full Rune since it will convert as a width-1 error rune.
        public static bool FullRune(slice<byte> p)
        {
            var n = len(p);
            if (n == 0L)
            {
                return false;
            }

            var x = first[p[0L]];
            if (n >= int(x & 7L))
            {
                return true; // ASCII, invalid or valid.
            } 
            // Must be short or invalid.
            var accept = acceptRanges[x >> (int)(4L)];
            if (n > 1L && (p[1L] < accept.lo || accept.hi < p[1L]))
            {
                return true;
            }
            else if (n > 2L && (p[2L] < locb || hicb < p[2L]))
            {
                return true;
            }

            return false;

        }

        // FullRuneInString is like FullRune but its input is a string.
        public static bool FullRuneInString(@string s)
        {
            var n = len(s);
            if (n == 0L)
            {
                return false;
            }

            var x = first[s[0L]];
            if (n >= int(x & 7L))
            {
                return true; // ASCII, invalid, or valid.
            } 
            // Must be short or invalid.
            var accept = acceptRanges[x >> (int)(4L)];
            if (n > 1L && (s[1L] < accept.lo || accept.hi < s[1L]))
            {
                return true;
            }
            else if (n > 2L && (s[2L] < locb || hicb < s[2L]))
            {
                return true;
            }

            return false;

        }

        // DecodeRune unpacks the first UTF-8 encoding in p and returns the rune and
        // its width in bytes. If p is empty it returns (RuneError, 0). Otherwise, if
        // the encoding is invalid, it returns (RuneError, 1). Both are impossible
        // results for correct, non-empty UTF-8.
        //
        // An encoding is invalid if it is incorrect UTF-8, encodes a rune that is
        // out of range, or is not the shortest possible UTF-8 encoding for the
        // value. No other validation is performed.
        public static (int, long) DecodeRune(slice<byte> p)
        {
            int r = default;
            long size = default;

            var n = len(p);
            if (n < 1L)
            {
                return (RuneError, 0L);
            }

            var p0 = p[0L];
            var x = first[p0];
            if (x >= as)
            { 
                // The following code simulates an additional check for x == xx and
                // handling the ASCII and invalid cases accordingly. This mask-and-or
                // approach prevents an additional branch.
                var mask = rune(x) << (int)(31L) >> (int)(31L); // Create 0x0000 or 0xFFFF.
                return (rune(p[0L]) & ~mask | RuneError & mask, 1L);

            }

            var sz = int(x & 7L);
            var accept = acceptRanges[x >> (int)(4L)];
            if (n < sz)
            {
                return (RuneError, 1L);
            }

            var b1 = p[1L];
            if (b1 < accept.lo || accept.hi < b1)
            {
                return (RuneError, 1L);
            }

            if (sz <= 2L)
            { // <= instead of == to help the compiler eliminate some bounds checks
                return (rune(p0 & mask2) << (int)(6L) | rune(b1 & maskx), 2L);

            }

            var b2 = p[2L];
            if (b2 < locb || hicb < b2)
            {
                return (RuneError, 1L);
            }

            if (sz <= 3L)
            {
                return (rune(p0 & mask3) << (int)(12L) | rune(b1 & maskx) << (int)(6L) | rune(b2 & maskx), 3L);
            }

            var b3 = p[3L];
            if (b3 < locb || hicb < b3)
            {
                return (RuneError, 1L);
            }

            return (rune(p0 & mask4) << (int)(18L) | rune(b1 & maskx) << (int)(12L) | rune(b2 & maskx) << (int)(6L) | rune(b3 & maskx), 4L);

        }

        // DecodeRuneInString is like DecodeRune but its input is a string. If s is
        // empty it returns (RuneError, 0). Otherwise, if the encoding is invalid, it
        // returns (RuneError, 1). Both are impossible results for correct, non-empty
        // UTF-8.
        //
        // An encoding is invalid if it is incorrect UTF-8, encodes a rune that is
        // out of range, or is not the shortest possible UTF-8 encoding for the
        // value. No other validation is performed.
        public static (int, long) DecodeRuneInString(@string s)
        {
            int r = default;
            long size = default;

            var n = len(s);
            if (n < 1L)
            {
                return (RuneError, 0L);
            }

            var s0 = s[0L];
            var x = first[s0];
            if (x >= as)
            { 
                // The following code simulates an additional check for x == xx and
                // handling the ASCII and invalid cases accordingly. This mask-and-or
                // approach prevents an additional branch.
                var mask = rune(x) << (int)(31L) >> (int)(31L); // Create 0x0000 or 0xFFFF.
                return (rune(s[0L]) & ~mask | RuneError & mask, 1L);

            }

            var sz = int(x & 7L);
            var accept = acceptRanges[x >> (int)(4L)];
            if (n < sz)
            {
                return (RuneError, 1L);
            }

            var s1 = s[1L];
            if (s1 < accept.lo || accept.hi < s1)
            {
                return (RuneError, 1L);
            }

            if (sz <= 2L)
            { // <= instead of == to help the compiler eliminate some bounds checks
                return (rune(s0 & mask2) << (int)(6L) | rune(s1 & maskx), 2L);

            }

            var s2 = s[2L];
            if (s2 < locb || hicb < s2)
            {
                return (RuneError, 1L);
            }

            if (sz <= 3L)
            {
                return (rune(s0 & mask3) << (int)(12L) | rune(s1 & maskx) << (int)(6L) | rune(s2 & maskx), 3L);
            }

            var s3 = s[3L];
            if (s3 < locb || hicb < s3)
            {
                return (RuneError, 1L);
            }

            return (rune(s0 & mask4) << (int)(18L) | rune(s1 & maskx) << (int)(12L) | rune(s2 & maskx) << (int)(6L) | rune(s3 & maskx), 4L);

        }

        // DecodeLastRune unpacks the last UTF-8 encoding in p and returns the rune and
        // its width in bytes. If p is empty it returns (RuneError, 0). Otherwise, if
        // the encoding is invalid, it returns (RuneError, 1). Both are impossible
        // results for correct, non-empty UTF-8.
        //
        // An encoding is invalid if it is incorrect UTF-8, encodes a rune that is
        // out of range, or is not the shortest possible UTF-8 encoding for the
        // value. No other validation is performed.
        public static (int, long) DecodeLastRune(slice<byte> p)
        {
            int r = default;
            long size = default;

            var end = len(p);
            if (end == 0L)
            {
                return (RuneError, 0L);
            }

            var start = end - 1L;
            r = rune(p[start]);
            if (r < RuneSelf)
            {
                return (r, 1L);
            } 
            // guard against O(n^2) behavior when traversing
            // backwards through strings with long sequences of
            // invalid UTF-8.
            var lim = end - UTFMax;
            if (lim < 0L)
            {
                lim = 0L;
            }

            start--;

            while (start >= lim)
            {
                if (RuneStart(p[start]))
                {
                    break;
                start--;
                }

            }

            if (start < 0L)
            {
                start = 0L;
            }

            r, size = DecodeRune(p[start..end]);
            if (start + size != end)
            {
                return (RuneError, 1L);
            }

            return (r, size);

        }

        // DecodeLastRuneInString is like DecodeLastRune but its input is a string. If
        // s is empty it returns (RuneError, 0). Otherwise, if the encoding is invalid,
        // it returns (RuneError, 1). Both are impossible results for correct,
        // non-empty UTF-8.
        //
        // An encoding is invalid if it is incorrect UTF-8, encodes a rune that is
        // out of range, or is not the shortest possible UTF-8 encoding for the
        // value. No other validation is performed.
        public static (int, long) DecodeLastRuneInString(@string s)
        {
            int r = default;
            long size = default;

            var end = len(s);
            if (end == 0L)
            {
                return (RuneError, 0L);
            }

            var start = end - 1L;
            r = rune(s[start]);
            if (r < RuneSelf)
            {
                return (r, 1L);
            } 
            // guard against O(n^2) behavior when traversing
            // backwards through strings with long sequences of
            // invalid UTF-8.
            var lim = end - UTFMax;
            if (lim < 0L)
            {
                lim = 0L;
            }

            start--;

            while (start >= lim)
            {
                if (RuneStart(s[start]))
                {
                    break;
                start--;
                }

            }

            if (start < 0L)
            {
                start = 0L;
            }

            r, size = DecodeRuneInString(s[start..end]);
            if (start + size != end)
            {
                return (RuneError, 1L);
            }

            return (r, size);

        }

        // RuneLen returns the number of bytes required to encode the rune.
        // It returns -1 if the rune is not a valid value to encode in UTF-8.
        public static long RuneLen(int r)
        {

            if (r < 0L) 
                return -1L;
            else if (r <= rune1Max) 
                return 1L;
            else if (r <= rune2Max) 
                return 2L;
            else if (surrogateMin <= r && r <= surrogateMax) 
                return -1L;
            else if (r <= rune3Max) 
                return 3L;
            else if (r <= MaxRune) 
                return 4L;
                        return -1L;

        }

        // EncodeRune writes into p (which must be large enough) the UTF-8 encoding of the rune.
        // It returns the number of bytes written.
        public static long EncodeRune(slice<byte> p, int r)
        { 
            // Negative values are erroneous. Making it unsigned addresses the problem.
            {
                var i = uint32(r);


                if (i <= rune1Max)
                {
                    p[0L] = byte(r);
                    return 1L;
                    goto __switch_break0;
                }
                if (i <= rune2Max)
                {
                    _ = p[1L]; // eliminate bounds checks
                    p[0L] = t2 | byte(r >> (int)(6L));
                    p[1L] = tx | byte(r) & maskx;
                    return 2L;
                    goto __switch_break0;
                }
                if (i > MaxRune || surrogateMin <= i && i <= surrogateMax)
                {
                    r = RuneError;
                    fallthrough = true;
                }
                if (fallthrough || i <= rune3Max)
                {
                    _ = p[2L]; // eliminate bounds checks
                    p[0L] = t3 | byte(r >> (int)(12L));
                    p[1L] = tx | byte(r >> (int)(6L)) & maskx;
                    p[2L] = tx | byte(r) & maskx;
                    return 3L;
                    goto __switch_break0;
                }
                // default: 
                    _ = p[3L]; // eliminate bounds checks
                    p[0L] = t4 | byte(r >> (int)(18L));
                    p[1L] = tx | byte(r >> (int)(12L)) & maskx;
                    p[2L] = tx | byte(r >> (int)(6L)) & maskx;
                    p[3L] = tx | byte(r) & maskx;
                    return 4L;

                __switch_break0:;
            }

        }

        // RuneCount returns the number of runes in p. Erroneous and short
        // encodings are treated as single runes of width 1 byte.
        public static long RuneCount(slice<byte> p)
        {
            var np = len(p);
            long n = default;
            {
                long i = 0L;

                while (i < np)
                {
                    n++;
                    var c = p[i];
                    if (c < RuneSelf)
                    { 
                        // ASCII fast path
                        i++;
                        continue;

                    }

                    var x = first[c];
                    if (x == xx)
                    {
                        i++; // invalid.
                        continue;

                    }

                    var size = int(x & 7L);
                    if (i + size > np)
                    {
                        i++; // Short or invalid.
                        continue;

                    }

                    var accept = acceptRanges[x >> (int)(4L)];
                    {
                        var c__prev1 = c;

                        c = p[i + 1L];

                        if (c < accept.lo || accept.hi < c)
                        {
                            size = 1L;
                        }
                        else if (size == 2L)
                        {
                        }                        {
                            var c__prev3 = c;

                            c = p[i + 2L];


                            else if (c < locb || hicb < c)
                            {
                                size = 1L;
                            }
                            else if (size == 3L)
                            {
                            }                            {
                                var c__prev5 = c;

                                c = p[i + 3L];


                                else if (c < locb || hicb < c)
                                {
                                    size = 1L;
                                }

                                c = c__prev5;

                            }


                            c = c__prev3;

                        }


                        c = c__prev1;

                    }

                    i += size;

                }

            }
            return n;

        }

        // RuneCountInString is like RuneCount but its input is a string.
        public static long RuneCountInString(@string s)
        {
            long n = default;

            var ns = len(s);
            for (long i = 0L; i < ns; n++)
            {
                var c = s[i];
                if (c < RuneSelf)
                { 
                    // ASCII fast path
                    i++;
                    continue;

                }

                var x = first[c];
                if (x == xx)
                {
                    i++; // invalid.
                    continue;

                }

                var size = int(x & 7L);
                if (i + size > ns)
                {
                    i++; // Short or invalid.
                    continue;

                }

                var accept = acceptRanges[x >> (int)(4L)];
                {
                    var c__prev1 = c;

                    c = s[i + 1L];

                    if (c < accept.lo || accept.hi < c)
                    {
                        size = 1L;
                    }
                    else if (size == 2L)
                    {
                    }                    {
                        var c__prev3 = c;

                        c = s[i + 2L];


                        else if (c < locb || hicb < c)
                        {
                            size = 1L;
                        }
                        else if (size == 3L)
                        {
                        }                        {
                            var c__prev5 = c;

                            c = s[i + 3L];


                            else if (c < locb || hicb < c)
                            {
                                size = 1L;
                            }

                            c = c__prev5;

                        }


                        c = c__prev3;

                    }


                    c = c__prev1;

                }

                i += size;

            }

            return n;

        }

        // RuneStart reports whether the byte could be the first byte of an encoded,
        // possibly invalid rune. Second and subsequent bytes always have the top two
        // bits set to 10.
        public static bool RuneStart(byte b)
        {
            return b & 0xC0UL != 0x80UL;
        }

        // Valid reports whether p consists entirely of valid UTF-8-encoded runes.
        public static bool Valid(slice<byte> p)
        { 
            // Fast path. Check for and skip 8 bytes of ASCII characters per iteration.
            while (len(p) >= 8L)
            { 
                // Combining two 32 bit loads allows the same code to be used
                // for 32 and 64 bit platforms.
                // The compiler can generate a 32bit load for first32 and second32
                // on many platforms. See test/codegen/memcombine.go.
                var first32 = uint32(p[0L]) | uint32(p[1L]) << (int)(8L) | uint32(p[2L]) << (int)(16L) | uint32(p[3L]) << (int)(24L);
                var second32 = uint32(p[4L]) | uint32(p[5L]) << (int)(8L) | uint32(p[6L]) << (int)(16L) | uint32(p[7L]) << (int)(24L);
                if ((first32 | second32) & 0x80808080UL != 0L)
                { 
                    // Found a non ASCII byte (>= RuneSelf).
                    break;

                }

                p = p[8L..];

            }

            var n = len(p);
            {
                long i = 0L;

                while (i < n)
                {
                    var pi = p[i];
                    if (pi < RuneSelf)
                    {
                        i++;
                        continue;
                    }

                    var x = first[pi];
                    if (x == xx)
                    {
                        return false; // Illegal starter byte.
                    }

                    var size = int(x & 7L);
                    if (i + size > n)
                    {
                        return false; // Short or invalid.
                    }

                    var accept = acceptRanges[x >> (int)(4L)];
                    {
                        var c__prev1 = c;

                        var c = p[i + 1L];

                        if (c < accept.lo || accept.hi < c)
                        {
                            return false;
                        }
                        else if (size == 2L)
                        {
                        }                        {
                            var c__prev3 = c;

                            c = p[i + 2L];


                            else if (c < locb || hicb < c)
                            {
                                return false;
                            }
                            else if (size == 3L)
                            {
                            }                            {
                                var c__prev5 = c;

                                c = p[i + 3L];


                                else if (c < locb || hicb < c)
                                {
                                    return false;
                                }

                                c = c__prev5;

                            }


                            c = c__prev3;

                        }


                        c = c__prev1;

                    }

                    i += size;

                }

            }
            return true;

        }

        // ValidString reports whether s consists entirely of valid UTF-8-encoded runes.
        public static bool ValidString(@string s)
        { 
            // Fast path. Check for and skip 8 bytes of ASCII characters per iteration.
            while (len(s) >= 8L)
            { 
                // Combining two 32 bit loads allows the same code to be used
                // for 32 and 64 bit platforms.
                // The compiler can generate a 32bit load for first32 and second32
                // on many platforms. See test/codegen/memcombine.go.
                var first32 = uint32(s[0L]) | uint32(s[1L]) << (int)(8L) | uint32(s[2L]) << (int)(16L) | uint32(s[3L]) << (int)(24L);
                var second32 = uint32(s[4L]) | uint32(s[5L]) << (int)(8L) | uint32(s[6L]) << (int)(16L) | uint32(s[7L]) << (int)(24L);
                if ((first32 | second32) & 0x80808080UL != 0L)
                { 
                    // Found a non ASCII byte (>= RuneSelf).
                    break;

                }

                s = s[8L..];

            }

            var n = len(s);
            {
                long i = 0L;

                while (i < n)
                {
                    var si = s[i];
                    if (si < RuneSelf)
                    {
                        i++;
                        continue;
                    }

                    var x = first[si];
                    if (x == xx)
                    {
                        return false; // Illegal starter byte.
                    }

                    var size = int(x & 7L);
                    if (i + size > n)
                    {
                        return false; // Short or invalid.
                    }

                    var accept = acceptRanges[x >> (int)(4L)];
                    {
                        var c__prev1 = c;

                        var c = s[i + 1L];

                        if (c < accept.lo || accept.hi < c)
                        {
                            return false;
                        }
                        else if (size == 2L)
                        {
                        }                        {
                            var c__prev3 = c;

                            c = s[i + 2L];


                            else if (c < locb || hicb < c)
                            {
                                return false;
                            }
                            else if (size == 3L)
                            {
                            }                            {
                                var c__prev5 = c;

                                c = s[i + 3L];


                                else if (c < locb || hicb < c)
                                {
                                    return false;
                                }

                                c = c__prev5;

                            }


                            c = c__prev3;

                        }


                        c = c__prev1;

                    }

                    i += size;

                }

            }
            return true;

        }

        // ValidRune reports whether r can be legally encoded as UTF-8.
        // Code points that are out of range or a surrogate half are illegal.
        public static bool ValidRune(int r)
        {

            if (0L <= r && r < surrogateMin) 
                return true;
            else if (surrogateMax < r && r <= MaxRune) 
                return true;
                        return false;

        }
    }
}}
