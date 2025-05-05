// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using ꓸꓸꓸж<RangeTable> = Span<ж<RangeTable>>;

partial class unicode_package {

// Bit masks for each code point under U+0100, for fast lookup.
internal static readonly UntypedInt pC = /* 1 << iota */ 1; // a control character.

internal static readonly UntypedInt pP = 2; // a punctuation character.

internal static readonly UntypedInt pN = 4; // a numeral.

internal static readonly UntypedInt pS = 8; // a symbolic character.

internal static readonly UntypedInt pZ = 16; // a spacing character.

internal static readonly UntypedInt pLu = 32; // an upper-case letter.

internal static readonly UntypedInt pLl = 64; // a lower-case letter.

internal static readonly UntypedInt pp = 128; // a printable character according to Go's definition.

internal static readonly UntypedInt pg = /* pp | pZ */ 144; // a graphical character according to the Unicode definition.

internal static readonly UntypedInt pLo = /* pLl | pLu */ 96; // a letter that is neither upper nor lower case.

internal static readonly UntypedInt pLmask = /* pLo */ 96;

// GraphicRanges defines the set of graphic characters according to Unicode.
public static slice<ж<RangeTable>> GraphicRanges = new ж<RangeTable>[]{
    L, M, N, P, S, Zs
}.slice();

// PrintRanges defines the set of printable characters according to Go.
// ASCII space, U+0020, is handled separately.
public static slice<ж<RangeTable>> PrintRanges = new ж<RangeTable>[]{
    L, M, N, P, S
}.slice();

// IsGraphic reports whether the rune is defined as a Graphic by Unicode.
// Such characters include letters, marks, numbers, punctuation, symbols, and
// spaces, from categories [L], [M], [N], [P], [S], [Zs].
public static bool IsGraphic(rune r) {
    // We convert to uint32 to avoid the extra test for negative,
    // and in the index we convert to uint8 to avoid the range check.
    if (((uint32)r) <= MaxLatin1) {
        return (uint8)(properties[((uint8)r)] & pg) != 0;
    }
    return In(r, ᏑGraphicRanges.ꓸꓸꓸ);
}

// IsPrint reports whether the rune is defined as printable by Go. Such
// characters include letters, marks, numbers, punctuation, symbols, and the
// ASCII space character, from categories [L], [M], [N], [P], [S] and the ASCII space
// character. This categorization is the same as [IsGraphic] except that the
// only spacing character is ASCII space, U+0020.
public static bool IsPrint(rune r) {
    if (((uint32)r) <= MaxLatin1) {
        return (uint8)(properties[((uint8)r)] & pp) != 0;
    }
    return In(r, ᏑPrintRanges.ꓸꓸꓸ);
}

// IsOneOf reports whether the rune is a member of one of the ranges.
// The function "In" provides a nicer signature and should be used in preference to IsOneOf.
public static bool IsOneOf(slice<ж<RangeTable>> ranges, rune r) {
    foreach (var (_, inside) in ranges) {
        if (Is(inside, r)) {
            return true;
        }
    }
    return false;
}

// In reports whether the rune is a member of one of the ranges.
public static bool In(rune r, params ꓸꓸꓸж<RangeTable> rangesʗp) {
    var ranges = rangesʗp.slice();

    foreach (var (_, inside) in ranges) {
        if (Is(inside, r)) {
            return true;
        }
    }
    return false;
}

// IsControl reports whether the rune is a control character.
// The [C] ([Other]) Unicode category includes more code points
// such as surrogates; use [Is](C, r) to test for them.
public static bool IsControl(rune r) {
    if (((uint32)r) <= MaxLatin1) {
        return (uint8)(properties[((uint8)r)] & pC) != 0;
    }
    // All control characters are < MaxLatin1.
    return false;
}

// IsLetter reports whether the rune is a letter (category [L]).
public static bool IsLetter(rune r) {
    if (((uint32)r) <= MaxLatin1) {
        return (uint8)(properties[((uint8)r)] & (pLmask)) != 0;
    }
    return isExcludingLatin(Letter, r);
}

// IsMark reports whether the rune is a mark character (category [M]).
public static bool IsMark(rune r) {
    // There are no mark characters in Latin-1.
    return isExcludingLatin(Mark, r);
}

// IsNumber reports whether the rune is a number (category [N]).
public static bool IsNumber(rune r) {
    if (((uint32)r) <= MaxLatin1) {
        return (uint8)(properties[((uint8)r)] & pN) != 0;
    }
    return isExcludingLatin(Number, r);
}

// IsPunct reports whether the rune is a Unicode punctuation character
// (category [P]).
public static bool IsPunct(rune r) {
    if (((uint32)r) <= MaxLatin1) {
        return (uint8)(properties[((uint8)r)] & pP) != 0;
    }
    return Is(Punct, r);
}

// IsSpace reports whether the rune is a space character as defined
// by Unicode's White Space property; in the Latin-1 space
// this is
//
//	'\t', '\n', '\v', '\f', '\r', ' ', U+0085 (NEL), U+00A0 (NBSP).
//
// Other definitions of spacing characters are set by category
// Z and property [Pattern_White_Space].
public static bool IsSpace(rune r) {
    // This property isn't the same as Z; special-case it.
    if (((uint32)r) <= MaxLatin1) {
        switch (r) {
        case (rune)'\t' or (rune)'\n' or (rune)'\v' or (rune)'\f' or (rune)'\r' or (rune)' ' or 133 or 160: {
            return true;
        }}

        return false;
    }
    return isExcludingLatin(White_Space, r);
}

// IsSymbol reports whether the rune is a symbolic character.
public static bool IsSymbol(rune r) {
    if (((uint32)r) <= MaxLatin1) {
        return (uint8)(properties[((uint8)r)] & pS) != 0;
    }
    return isExcludingLatin(Symbol, r);
}

} // end unicode_package
