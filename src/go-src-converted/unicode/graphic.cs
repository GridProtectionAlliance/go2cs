// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package unicode -- go2cs converted at 2022 March 06 22:14:11 UTC
// import "unicode" ==> using unicode = go.unicode_package
// Original source: C:\Program Files\Go\src\unicode\graphic.go


namespace go;

public static partial class unicode_package {

    // Bit masks for each code point under U+0100, for fast lookup.
private static readonly nint pC = 1 << (int)(iota); // a control character.
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
public static ptr<RangeTable> GraphicRanges = new slice<ptr<RangeTable>>(new ptr<RangeTable>[] { L, M, N, P, S, Zs });

// PrintRanges defines the set of printable characters according to Go.
// ASCII space, U+0020, is handled separately.
public static ptr<RangeTable> PrintRanges = new slice<ptr<RangeTable>>(new ptr<RangeTable>[] { L, M, N, P, S });

// IsGraphic reports whether the rune is defined as a Graphic by Unicode.
// Such characters include letters, marks, numbers, punctuation, symbols, and
// spaces, from categories L, M, N, P, S, Zs.
public static bool IsGraphic(int r) { 
    // We convert to uint32 to avoid the extra test for negative,
    // and in the index we convert to uint8 to avoid the range check.
    if (uint32(r) <= MaxLatin1) {
        return properties[uint8(r)] & pg != 0;
    }
    return In(r, _addr_GraphicRanges);

}

// IsPrint reports whether the rune is defined as printable by Go. Such
// characters include letters, marks, numbers, punctuation, symbols, and the
// ASCII space character, from categories L, M, N, P, S and the ASCII space
// character. This categorization is the same as IsGraphic except that the
// only spacing character is ASCII space, U+0020.
public static bool IsPrint(int r) {
    if (uint32(r) <= MaxLatin1) {
        return properties[uint8(r)] & pp != 0;
    }
    return In(r, _addr_PrintRanges);

}

// IsOneOf reports whether the rune is a member of one of the ranges.
// The function "In" provides a nicer signature and should be used in preference to IsOneOf.
public static bool IsOneOf(slice<ptr<RangeTable>> ranges, int r) {
    foreach (var (_, inside) in ranges) {
        if (Is(inside, r)) {
            return true;
        }
    }    return false;

}

// In reports whether the rune is a member of one of the ranges.
public static bool In(int r, params ptr<ptr<RangeTable>>[] _addr_ranges) {
    ranges = ranges.Clone();
    ref RangeTable ranges = ref _addr_ranges.val;

    foreach (var (_, inside) in ranges) {
        if (Is(inside, r)) {
            return true;
        }
    }    return false;

}

// IsControl reports whether the rune is a control character.
// The C (Other) Unicode category includes more code points
// such as surrogates; use Is(C, r) to test for them.
public static bool IsControl(int r) {
    if (uint32(r) <= MaxLatin1) {
        return properties[uint8(r)] & pC != 0;
    }
    return false;

}

// IsLetter reports whether the rune is a letter (category L).
public static bool IsLetter(int r) {
    if (uint32(r) <= MaxLatin1) {
        return properties[uint8(r)] & (pLmask) != 0;
    }
    return isExcludingLatin(Letter, r);

}

// IsMark reports whether the rune is a mark character (category M).
public static bool IsMark(int r) { 
    // There are no mark characters in Latin-1.
    return isExcludingLatin(Mark, r);

}

// IsNumber reports whether the rune is a number (category N).
public static bool IsNumber(int r) {
    if (uint32(r) <= MaxLatin1) {
        return properties[uint8(r)] & pN != 0;
    }
    return isExcludingLatin(Number, r);

}

// IsPunct reports whether the rune is a Unicode punctuation character
// (category P).
public static bool IsPunct(int r) {
    if (uint32(r) <= MaxLatin1) {
        return properties[uint8(r)] & pP != 0;
    }
    return Is(Punct, r);

}

// IsSpace reports whether the rune is a space character as defined
// by Unicode's White Space property; in the Latin-1 space
// this is
//    '\t', '\n', '\v', '\f', '\r', ' ', U+0085 (NEL), U+00A0 (NBSP).
// Other definitions of spacing characters are set by category
// Z and property Pattern_White_Space.
public static bool IsSpace(int r) { 
    // This property isn't the same as Z; special-case it.
    if (uint32(r) <= MaxLatin1) {
        switch (r) {
            case '\t': 

            case '\n': 

            case '\v': 

            case '\f': 

            case '\r': 

            case ' ': 

            case 0x85: 

            case 0xA0: 
                return true;
                break;
        }
        return false;

    }
    return isExcludingLatin(White_Space, r);

}

// IsSymbol reports whether the rune is a symbolic character.
public static bool IsSymbol(int r) {
    if (uint32(r) <= MaxLatin1) {
        return properties[uint8(r)] & pS != 0;
    }
    return isExcludingLatin(Symbol, r);

}

} // end unicode_package
