// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package unicode -- go2cs converted at 2020 August 29 08:22:01 UTC
// import "unicode" ==> using unicode = go.unicode_package
// Original source: C:\Go\src\unicode\graphic.go

using static go.builtin;

namespace go
{
    public static partial class unicode_package
    {
        // Bit masks for each code point under U+0100, for fast lookup.
        private static readonly long pC = 1L << (int)(iota); // a control character.
        private static readonly var pP = 0; // a punctuation character.
        private static readonly var pN = 1; // a numeral.
        private static readonly var pS = 2; // a symbolic character.
        private static readonly var pZ = 3; // a spacing character.
        private static readonly var pLu = 4; // an upper-case letter.
        private static readonly var pLl = 5; // a lower-case letter.
        private static readonly pg pp = pp | pZ; // a graphical character according to the Unicode definition.
        private static readonly var pLo = pLl | pLu; // a letter that is neither upper nor lower case.
        private static readonly var pLmask = pLo;

        // GraphicRanges defines the set of graphic characters according to Unicode.
        public static ref RangeTable GraphicRanges = new slice<ref RangeTable>(new ref RangeTable[] { L, M, N, P, S, Zs });

        // PrintRanges defines the set of printable characters according to Go.
        // ASCII space, U+0020, is handled separately.
        public static ref RangeTable PrintRanges = new slice<ref RangeTable>(new ref RangeTable[] { L, M, N, P, S });

        // IsGraphic reports whether the rune is defined as a Graphic by Unicode.
        // Such characters include letters, marks, numbers, punctuation, symbols, and
        // spaces, from categories L, M, N, P, S, Zs.
        public static bool IsGraphic(int r)
        { 
            // We convert to uint32 to avoid the extra test for negative,
            // and in the index we convert to uint8 to avoid the range check.
            if (uint32(r) <= MaxLatin1)
            {
                return properties[uint8(r)] & pg != 0L;
            }
            return In(r, GraphicRanges);
        }

        // IsPrint reports whether the rune is defined as printable by Go. Such
        // characters include letters, marks, numbers, punctuation, symbols, and the
        // ASCII space character, from categories L, M, N, P, S and the ASCII space
        // character. This categorization is the same as IsGraphic except that the
        // only spacing character is ASCII space, U+0020.
        public static bool IsPrint(int r)
        {
            if (uint32(r) <= MaxLatin1)
            {
                return properties[uint8(r)] & pp != 0L;
            }
            return In(r, PrintRanges);
        }

        // IsOneOf reports whether the rune is a member of one of the ranges.
        // The function "In" provides a nicer signature and should be used in preference to IsOneOf.
        public static bool IsOneOf(slice<ref RangeTable> ranges, int r)
        {
            foreach (var (_, inside) in ranges)
            {
                if (Is(inside, r))
                {
                    return true;
                }
            }
            return false;
        }

        // In reports whether the rune is a member of one of the ranges.
        public static bool In(int r, params ptr<RangeTable>[] ranges)
        {
            ranges = ranges.Clone();

            foreach (var (_, inside) in ranges)
            {
                if (Is(inside, r))
                {
                    return true;
                }
            }
            return false;
        }

        // IsControl reports whether the rune is a control character.
        // The C (Other) Unicode category includes more code points
        // such as surrogates; use Is(C, r) to test for them.
        public static bool IsControl(int r)
        {
            if (uint32(r) <= MaxLatin1)
            {
                return properties[uint8(r)] & pC != 0L;
            } 
            // All control characters are < MaxLatin1.
            return false;
        }

        // IsLetter reports whether the rune is a letter (category L).
        public static bool IsLetter(int r)
        {
            if (uint32(r) <= MaxLatin1)
            {
                return properties[uint8(r)] & (pLmask) != 0L;
            }
            return isExcludingLatin(Letter, r);
        }

        // IsMark reports whether the rune is a mark character (category M).
        public static bool IsMark(int r)
        { 
            // There are no mark characters in Latin-1.
            return isExcludingLatin(Mark, r);
        }

        // IsNumber reports whether the rune is a number (category N).
        public static bool IsNumber(int r)
        {
            if (uint32(r) <= MaxLatin1)
            {
                return properties[uint8(r)] & pN != 0L;
            }
            return isExcludingLatin(Number, r);
        }

        // IsPunct reports whether the rune is a Unicode punctuation character
        // (category P).
        public static bool IsPunct(int r)
        {
            if (uint32(r) <= MaxLatin1)
            {
                return properties[uint8(r)] & pP != 0L;
            }
            return Is(Punct, r);
        }

        // IsSpace reports whether the rune is a space character as defined
        // by Unicode's White Space property; in the Latin-1 space
        // this is
        //    '\t', '\n', '\v', '\f', '\r', ' ', U+0085 (NEL), U+00A0 (NBSP).
        // Other definitions of spacing characters are set by category
        // Z and property Pattern_White_Space.
        public static bool IsSpace(int r)
        { 
            // This property isn't the same as Z; special-case it.
            if (uint32(r) <= MaxLatin1)
            {
                switch (r)
                {
                    case '\t': 

                    case '\n': 

                    case '\v': 

                    case '\f': 

                    case '\r': 

                    case ' ': 

                    case 0x85UL: 

                    case 0xA0UL: 
                        return true;
                        break;
                }
                return false;
            }
            return isExcludingLatin(White_Space, r);
        }

        // IsSymbol reports whether the rune is a symbolic character.
        public static bool IsSymbol(int r)
        {
            if (uint32(r) <= MaxLatin1)
            {
                return properties[uint8(r)] & pS != 0L;
            }
            return isExcludingLatin(Symbol, r);
        }
    }
}
