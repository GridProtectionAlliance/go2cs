// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package utf16 implements encoding and decoding of UTF-16 sequences.
// package utf16 -- go2cs converted at 2020 October 09 04:49:43 UTC
// import "unicode/utf16" ==> using utf16 = go.unicode.utf16_package
// Original source: C:\Go\src\unicode\utf16\utf16.go

using static go.builtin;

namespace go {
namespace unicode
{
    public static partial class utf16_package
    {
        // The conditions replacementChar==unicode.ReplacementChar and
        // maxRune==unicode.MaxRune are verified in the tests.
        // Defining them locally avoids this package depending on package unicode.
        private static readonly char replacementChar = (char)'\uFFFD'; // Unicode replacement character
        private static readonly char maxRune = (char)'\U0010FFFF'; // Maximum valid Unicode code point.

 
        // 0xd800-0xdc00 encodes the high 10 bits of a pair.
        // 0xdc00-0xe000 encodes the low 10 bits of a pair.
        // the value is those 20 bits plus 0x10000.
        private static readonly ulong surr1 = (ulong)0xd800UL;
        private static readonly ulong surr2 = (ulong)0xdc00UL;
        private static readonly ulong surr3 = (ulong)0xe000UL;

        private static readonly ulong surrSelf = (ulong)0x10000UL;


        // IsSurrogate reports whether the specified Unicode code point
        // can appear in a surrogate pair.
        public static bool IsSurrogate(int r)
        {
            return surr1 <= r && r < surr3;
        }

        // DecodeRune returns the UTF-16 decoding of a surrogate pair.
        // If the pair is not a valid UTF-16 surrogate pair, DecodeRune returns
        // the Unicode replacement code point U+FFFD.
        public static int DecodeRune(int r1, int r2)
        {
            if (surr1 <= r1 && r1 < surr2 && surr2 <= r2 && r2 < surr3)
            {
                return (r1 - surr1) << (int)(10L) | (r2 - surr2) + surrSelf;
            }

            return replacementChar;

        }

        // EncodeRune returns the UTF-16 surrogate pair r1, r2 for the given rune.
        // If the rune is not a valid Unicode code point or does not need encoding,
        // EncodeRune returns U+FFFD, U+FFFD.
        public static (int, int) EncodeRune(int r)
        {
            int r1 = default;
            int r2 = default;

            if (r < surrSelf || r > maxRune)
            {
                return (replacementChar, replacementChar);
            }

            r -= surrSelf;
            return (surr1 + (r >> (int)(10L)) & 0x3ffUL, surr2 + r & 0x3ffUL);

        }

        // Encode returns the UTF-16 encoding of the Unicode code point sequence s.
        public static slice<ushort> Encode(slice<int> s)
        {
            var n = len(s);
            {
                var v__prev1 = v;

                foreach (var (_, __v) in s)
                {
                    v = __v;
                    if (v >= surrSelf)
                    {
                        n++;
                    }

                }

                v = v__prev1;
            }

            var a = make_slice<ushort>(n);
            n = 0L;
            {
                var v__prev1 = v;

                foreach (var (_, __v) in s)
                {
                    v = __v;

                    if (0L <= v && v < surr1 || surr3 <= v && v < surrSelf) 
                        // normal rune
                        a[n] = uint16(v);
                        n++;
                    else if (surrSelf <= v && v <= maxRune) 
                        // needs surrogate sequence
                        var (r1, r2) = EncodeRune(v);
                        a[n] = uint16(r1);
                        a[n + 1L] = uint16(r2);
                        n += 2L;
                    else 
                        a[n] = uint16(replacementChar);
                        n++;
                    
                }

                v = v__prev1;
            }

            return a[..n];

        }

        // Decode returns the Unicode code point sequence represented
        // by the UTF-16 encoding s.
        public static slice<int> Decode(slice<ushort> s)
        {
            var a = make_slice<int>(len(s));
            long n = 0L;
            for (long i = 0L; i < len(s); i++)
            {
                {
                    var r = s[i];


                    if (r < surr1 || surr3 <= r) 
                        // normal rune
                        a[n] = rune(r);
                    else if (surr1 <= r && r < surr2 && i + 1L < len(s) && surr2 <= s[i + 1L] && s[i + 1L] < surr3) 
                        // valid surrogate sequence
                        a[n] = DecodeRune(rune(r), rune(s[i + 1L]));
                        i++;
                    else 
                        // invalid surrogate sequence
                        a[n] = replacementChar;

                }
                n++;

            }

            return a[..n];

        }
    }
}}
