// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 August 29 08:21:36 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\utf8.go

using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        // Numbers fundamental to the encoding.
        private static readonly char runeError = '\uFFFD'; // the "error" Rune or "Unicode replacement character"
        private static readonly ulong runeSelf = 0x80UL; // characters below Runeself are represented as themselves in a single byte.
        private static readonly char maxRune = '\U0010FFFF'; // Maximum valid Unicode code point.

        // Code points in the surrogate range are not valid for UTF-8.
        private static readonly ulong surrogateMin = 0xD800UL;
        private static readonly ulong surrogateMax = 0xDFFFUL;

        private static readonly ulong t1 = 0x00UL; // 0000 0000
        private static readonly ulong tx = 0x80UL; // 1000 0000
        private static readonly ulong t2 = 0xC0UL; // 1100 0000
        private static readonly ulong t3 = 0xE0UL; // 1110 0000
        private static readonly ulong t4 = 0xF0UL; // 1111 0000
        private static readonly ulong t5 = 0xF8UL; // 1111 1000

        private static readonly ulong maskx = 0x3FUL; // 0011 1111
        private static readonly ulong mask2 = 0x1FUL; // 0001 1111
        private static readonly ulong mask3 = 0x0FUL; // 0000 1111
        private static readonly ulong mask4 = 0x07UL; // 0000 0111

        private static readonly long rune1Max = 1L << (int)(7L) - 1L;
        private static readonly long rune2Max = 1L << (int)(11L) - 1L;
        private static readonly long rune3Max = 1L << (int)(16L) - 1L; 

        // The default lowest and highest continuation byte.
        private static readonly ulong locb = 0x80UL; // 1000 0000
        private static readonly ulong hicb = 0xBFUL; // 1011 1111

        // decoderune returns the non-ASCII rune at the start of
        // s[k:] and the index after the rune in s.
        //
        // decoderune assumes that caller has checked that
        // the to be decoded rune is a non-ASCII rune.
        //
        // If the string appears to be incomplete or decoding problems
        // are encountered (runeerror, k + 1) is returned to ensure
        // progress when decoderune is used to iterate over a string.
        private static (int, long) decoderune(@string s, long k)
        {
            pos = k;

            if (k >= len(s))
            {
                return (runeError, k + 1L);
            }
            s = s[k..];


            if (t2 <= s[0L] && s[0L] < t3) 
                // 0080-07FF two byte sequence
                if (len(s) > 1L && (locb <= s[1L] && s[1L] <= hicb))
                {
                    r = rune(s[0L] & mask2) << (int)(6L) | rune(s[1L] & maskx);
                    pos += 2L;
                    if (rune1Max < r)
                    {
                        return;
                    }
                }
            else if (t3 <= s[0L] && s[0L] < t4) 
                // 0800-FFFF three byte sequence
                if (len(s) > 2L && (locb <= s[1L] && s[1L] <= hicb) && (locb <= s[2L] && s[2L] <= hicb))
                {
                    r = rune(s[0L] & mask3) << (int)(12L) | rune(s[1L] & maskx) << (int)(6L) | rune(s[2L] & maskx);
                    pos += 3L;
                    if (rune2Max < r && !(surrogateMin <= r && r <= surrogateMax))
                    {
                        return;
                    }
                }
            else if (t4 <= s[0L] && s[0L] < t5) 
                // 10000-1FFFFF four byte sequence
                if (len(s) > 3L && (locb <= s[1L] && s[1L] <= hicb) && (locb <= s[2L] && s[2L] <= hicb) && (locb <= s[3L] && s[3L] <= hicb))
                {
                    r = rune(s[0L] & mask4) << (int)(18L) | rune(s[1L] & maskx) << (int)(12L) | rune(s[2L] & maskx) << (int)(6L) | rune(s[3L] & maskx);
                    pos += 4L;
                    if (rune3Max < r && r <= maxRune)
                    {
                        return;
                    }
                }
                        return (runeError, k + 1L);
        }

        // encoderune writes into p (which must be large enough) the UTF-8 encoding of the rune.
        // It returns the number of bytes written.
        private static long encoderune(slice<byte> p, int r)
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
                if (i > maxRune || surrogateMin <= i && i <= surrogateMax)
                {
                    r = runeError;
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
    }
}
