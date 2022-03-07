// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package utf8 implements functions and constants to support text encoded in
// UTF-8. It includes functions to translate between runes and UTF-8 byte sequences.
// See https://en.wikipedia.org/wiki/UTF-8
// package utf8 -- go2cs converted at 2022 March 06 22:08:05 UTC
// import "unicode/utf8" ==> using utf8 = go.unicode.utf8_package
// Original source: C:\Program Files\Go\src\unicode\utf8\utf8.go


namespace go.unicode;

public static partial class utf8_package {

    // The conditions RuneError==unicode.ReplacementChar and
    // MaxRune==unicode.MaxRune are verified in the tests.
    // Defining them locally avoids this package depending on package unicode.

    // Numbers fundamental to the encoding.
public static readonly char RuneError = '\uFFFD'; // the "error" Rune or "Unicode replacement character"
public static readonly nuint RuneSelf = 0x80; // characters below RuneSelf are represented as themselves in a single byte.
public static readonly char MaxRune = '\U0010FFFF'; // Maximum valid Unicode code point.
public static readonly nint UTFMax = 4; // maximum number of bytes of a UTF-8 encoded Unicode character.

// Code points in the surrogate range are not valid for UTF-8.
private static readonly nuint surrogateMin = 0xD800;
private static readonly nuint surrogateMax = 0xDFFF;


private static readonly nuint t1 = 0b00000000;
private static readonly nuint tx = 0b10000000;
private static readonly nuint t2 = 0b11000000;
private static readonly nuint t3 = 0b11100000;
private static readonly nuint t4 = 0b11110000;
private static readonly nuint t5 = 0b11111000;

private static readonly nuint maskx = 0b00111111;
private static readonly nuint mask2 = 0b00011111;
private static readonly nuint mask3 = 0b00001111;
private static readonly nuint mask4 = 0b00000111;

private static readonly nint rune1Max = 1 << 7 - 1;
private static readonly nint rune2Max = 1 << 11 - 1;
private static readonly nint rune3Max = 1 << 16 - 1; 

// The default lowest and highest continuation byte.
private static readonly nuint locb = 0b10000000;
private static readonly nuint hicb = 0b10111111; 

// These names of these constants are chosen to give nice alignment in the
// table below. The first nibble is an index into acceptRanges or F for
// special one-byte cases. The second nibble is the Rune length or the
// Status for the special one-byte case.
private static readonly nuint xx = 0xF1; // invalid: size 1
private static readonly nuint as = 0xF0; // ASCII: size 1
private static readonly nuint s1 = 0x02; // accept 0, size 2
private static readonly nuint s2 = 0x13; // accept 1, size 3
private static readonly nuint s3 = 0x03; // accept 0, size 3
private static readonly nuint s4 = 0x23; // accept 2, size 3
private static readonly nuint s5 = 0x34; // accept 3, size 4
private static readonly nuint s6 = 0x04; // accept 0, size 4
private static readonly nuint s7 = 0x44; // accept 4, size 4

// first is information about the first byte in a UTF-8 sequence.
private static array<byte> first = new array<byte>(new byte[] { as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, as, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s2, s3, s3, s3, s3, s3, s3, s3, s3, s3, s3, s3, s3, s4, s3, s3, s5, s6, s6, s6, s7, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx });

// acceptRange gives the range of valid values for the second byte in a UTF-8
// sequence.
private partial struct acceptRange {
    public byte lo; // lowest value for second byte.
    public byte hi; // highest value for second byte.
}

// acceptRanges has size 16 to avoid bounds checks in the code that uses it.
private static array<acceptRange> acceptRanges = new array<acceptRange>(InitKeyedValues<acceptRange>(16, (0, {locb,hicb}), (1, {0xA0,hicb}), (2, {locb,0x9F}), (3, {0x90,hicb}), (4, {locb,0x8F})));

// FullRune reports whether the bytes in p begin with a full UTF-8 encoding of a rune.
// An invalid encoding is considered a full Rune since it will convert as a width-1 error rune.
public static bool FullRune(slice<byte> p) {
    var n = len(p);
    if (n == 0) {
        return false;
    }
    var x = first[p[0]];
    if (n >= int(x & 7)) {
        return true; // ASCII, invalid or valid.
    }
    var accept = acceptRanges[x >> 4];
    if (n > 1 && (p[1] < accept.lo || accept.hi < p[1])) {
        return true;
    }
    else if (n > 2 && (p[2] < locb || hicb < p[2])) {
        return true;
    }
    return false;

}

// FullRuneInString is like FullRune but its input is a string.
public static bool FullRuneInString(@string s) {
    var n = len(s);
    if (n == 0) {
        return false;
    }
    var x = first[s[0]];
    if (n >= int(x & 7)) {
        return true; // ASCII, invalid, or valid.
    }
    var accept = acceptRanges[x >> 4];
    if (n > 1 && (s[1] < accept.lo || accept.hi < s[1])) {
        return true;
    }
    else if (n > 2 && (s[2] < locb || hicb < s[2])) {
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
public static (int, nint) DecodeRune(slice<byte> p) {
    int r = default;
    nint size = default;

    var n = len(p);
    if (n < 1) {
        return (RuneError, 0);
    }
    var p0 = p[0];
    var x = first[p0];
    if (x >= as) { 
        // The following code simulates an additional check for x == xx and
        // handling the ASCII and invalid cases accordingly. This mask-and-or
        // approach prevents an additional branch.
        var mask = rune(x) << 31 >> 31; // Create 0x0000 or 0xFFFF.
        return (rune(p[0]) & ~mask | RuneError & mask, 1);

    }
    var sz = int(x & 7);
    var accept = acceptRanges[x >> 4];
    if (n < sz) {
        return (RuneError, 1);
    }
    var b1 = p[1];
    if (b1 < accept.lo || accept.hi < b1) {
        return (RuneError, 1);
    }
    if (sz <= 2) { // <= instead of == to help the compiler eliminate some bounds checks
        return (rune(p0 & mask2) << 6 | rune(b1 & maskx), 2);

    }
    var b2 = p[2];
    if (b2 < locb || hicb < b2) {
        return (RuneError, 1);
    }
    if (sz <= 3) {
        return (rune(p0 & mask3) << 12 | rune(b1 & maskx) << 6 | rune(b2 & maskx), 3);
    }
    var b3 = p[3];
    if (b3 < locb || hicb < b3) {
        return (RuneError, 1);
    }
    return (rune(p0 & mask4) << 18 | rune(b1 & maskx) << 12 | rune(b2 & maskx) << 6 | rune(b3 & maskx), 4);

}

// DecodeRuneInString is like DecodeRune but its input is a string. If s is
// empty it returns (RuneError, 0). Otherwise, if the encoding is invalid, it
// returns (RuneError, 1). Both are impossible results for correct, non-empty
// UTF-8.
//
// An encoding is invalid if it is incorrect UTF-8, encodes a rune that is
// out of range, or is not the shortest possible UTF-8 encoding for the
// value. No other validation is performed.
public static (int, nint) DecodeRuneInString(@string s) {
    int r = default;
    nint size = default;

    var n = len(s);
    if (n < 1) {
        return (RuneError, 0);
    }
    var s0 = s[0];
    var x = first[s0];
    if (x >= as) { 
        // The following code simulates an additional check for x == xx and
        // handling the ASCII and invalid cases accordingly. This mask-and-or
        // approach prevents an additional branch.
        var mask = rune(x) << 31 >> 31; // Create 0x0000 or 0xFFFF.
        return (rune(s[0]) & ~mask | RuneError & mask, 1);

    }
    var sz = int(x & 7);
    var accept = acceptRanges[x >> 4];
    if (n < sz) {
        return (RuneError, 1);
    }
    var s1 = s[1];
    if (s1 < accept.lo || accept.hi < s1) {
        return (RuneError, 1);
    }
    if (sz <= 2) { // <= instead of == to help the compiler eliminate some bounds checks
        return (rune(s0 & mask2) << 6 | rune(s1 & maskx), 2);

    }
    var s2 = s[2];
    if (s2 < locb || hicb < s2) {
        return (RuneError, 1);
    }
    if (sz <= 3) {
        return (rune(s0 & mask3) << 12 | rune(s1 & maskx) << 6 | rune(s2 & maskx), 3);
    }
    var s3 = s[3];
    if (s3 < locb || hicb < s3) {
        return (RuneError, 1);
    }
    return (rune(s0 & mask4) << 18 | rune(s1 & maskx) << 12 | rune(s2 & maskx) << 6 | rune(s3 & maskx), 4);

}

// DecodeLastRune unpacks the last UTF-8 encoding in p and returns the rune and
// its width in bytes. If p is empty it returns (RuneError, 0). Otherwise, if
// the encoding is invalid, it returns (RuneError, 1). Both are impossible
// results for correct, non-empty UTF-8.
//
// An encoding is invalid if it is incorrect UTF-8, encodes a rune that is
// out of range, or is not the shortest possible UTF-8 encoding for the
// value. No other validation is performed.
public static (int, nint) DecodeLastRune(slice<byte> p) {
    int r = default;
    nint size = default;

    var end = len(p);
    if (end == 0) {
        return (RuneError, 0);
    }
    var start = end - 1;
    r = rune(p[start]);
    if (r < RuneSelf) {
        return (r, 1);
    }
    var lim = end - UTFMax;
    if (lim < 0) {
        lim = 0;
    }
    start--;

    while (start >= lim) {
        if (RuneStart(p[start])) {
            break;
        start--;
        }
    }
    if (start < 0) {
        start = 0;
    }
    r, size = DecodeRune(p[(int)start..(int)end]);
    if (start + size != end) {
        return (RuneError, 1);
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
public static (int, nint) DecodeLastRuneInString(@string s) {
    int r = default;
    nint size = default;

    var end = len(s);
    if (end == 0) {
        return (RuneError, 0);
    }
    var start = end - 1;
    r = rune(s[start]);
    if (r < RuneSelf) {
        return (r, 1);
    }
    var lim = end - UTFMax;
    if (lim < 0) {
        lim = 0;
    }
    start--;

    while (start >= lim) {
        if (RuneStart(s[start])) {
            break;
        start--;
        }
    }
    if (start < 0) {
        start = 0;
    }
    r, size = DecodeRuneInString(s[(int)start..(int)end]);
    if (start + size != end) {
        return (RuneError, 1);
    }
    return (r, size);

}

// RuneLen returns the number of bytes required to encode the rune.
// It returns -1 if the rune is not a valid value to encode in UTF-8.
public static nint RuneLen(int r) {

    if (r < 0) 
        return -1;
    else if (r <= rune1Max) 
        return 1;
    else if (r <= rune2Max) 
        return 2;
    else if (surrogateMin <= r && r <= surrogateMax) 
        return -1;
    else if (r <= rune3Max) 
        return 3;
    else if (r <= MaxRune) 
        return 4;
        return -1;

}

// EncodeRune writes into p (which must be large enough) the UTF-8 encoding of the rune.
// If the rune is out of range, it writes the encoding of RuneError.
// It returns the number of bytes written.
public static nint EncodeRune(slice<byte> p, int r) { 
    // Negative values are erroneous. Making it unsigned addresses the problem.
    {
        var i = uint32(r);


        if (i <= rune1Max)
        {
            p[0] = byte(r);
            return 1;
            goto __switch_break0;
        }
        if (i <= rune2Max)
        {
            _ = p[1]; // eliminate bounds checks
            p[0] = t2 | byte(r >> 6);
            p[1] = tx | byte(r) & maskx;
            return 2;
            goto __switch_break0;
        }
        if (i > MaxRune || surrogateMin <= i && i <= surrogateMax)
        {
            r = RuneError;
            fallthrough = true;
        }
        if (fallthrough || i <= rune3Max)
        {
            _ = p[2]; // eliminate bounds checks
            p[0] = t3 | byte(r >> 12);
            p[1] = tx | byte(r >> 6) & maskx;
            p[2] = tx | byte(r) & maskx;
            return 3;
            goto __switch_break0;
        }
        // default: 
            _ = p[3]; // eliminate bounds checks
            p[0] = t4 | byte(r >> 18);
            p[1] = tx | byte(r >> 12) & maskx;
            p[2] = tx | byte(r >> 6) & maskx;
            p[3] = tx | byte(r) & maskx;
            return 4;

        __switch_break0:;
    }

}

// RuneCount returns the number of runes in p. Erroneous and short
// encodings are treated as single runes of width 1 byte.
public static nint RuneCount(slice<byte> p) {
    var np = len(p);
    nint n = default;
    {
        nint i = 0;

        while (i < np) {
            n++;
            var c = p[i];
            if (c < RuneSelf) { 
                // ASCII fast path
                i++;
                continue;

            }

            var x = first[c];
            if (x == xx) {
                i++; // invalid.
                continue;

            }

            var size = int(x & 7);
            if (i + size > np) {
                i++; // Short or invalid.
                continue;

            }

            var accept = acceptRanges[x >> 4];
            {
                var c__prev1 = c;

                c = p[i + 1];

                if (c < accept.lo || accept.hi < c) {
                    size = 1;
                }
                else if (size == 2)                 }                {
                    var c__prev3 = c;

                    c = p[i + 2];


                    else if (c < locb || hicb < c) {
                        size = 1;
                    }
                    else if (size == 3)                     }                    {
                        var c__prev5 = c;

                        c = p[i + 3];


                        else if (c < locb || hicb < c) {
                            size = 1;
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
public static nint RuneCountInString(@string s) {
    nint n = default;

    var ns = len(s);
    for (nint i = 0; i < ns; n++) {
        var c = s[i];
        if (c < RuneSelf) { 
            // ASCII fast path
            i++;
            continue;

        }
        var x = first[c];
        if (x == xx) {
            i++; // invalid.
            continue;

        }
        var size = int(x & 7);
        if (i + size > ns) {
            i++; // Short or invalid.
            continue;

        }
        var accept = acceptRanges[x >> 4];
        {
            var c__prev1 = c;

            c = s[i + 1];

            if (c < accept.lo || accept.hi < c) {
                size = 1;
            }
            else if (size == 2)             }            {
                var c__prev3 = c;

                c = s[i + 2];


                else if (c < locb || hicb < c) {
                    size = 1;
                }
                else if (size == 3)                 }                {
                    var c__prev5 = c;

                    c = s[i + 3];


                    else if (c < locb || hicb < c) {
                        size = 1;
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
public static bool RuneStart(byte b) {
    return b & 0xC0 != 0x80;
}

// Valid reports whether p consists entirely of valid UTF-8-encoded runes.
public static bool Valid(slice<byte> p) { 
    // Fast path. Check for and skip 8 bytes of ASCII characters per iteration.
    while (len(p) >= 8) { 
        // Combining two 32 bit loads allows the same code to be used
        // for 32 and 64 bit platforms.
        // The compiler can generate a 32bit load for first32 and second32
        // on many platforms. See test/codegen/memcombine.go.
        var first32 = uint32(p[0]) | uint32(p[1]) << 8 | uint32(p[2]) << 16 | uint32(p[3]) << 24;
        var second32 = uint32(p[4]) | uint32(p[5]) << 8 | uint32(p[6]) << 16 | uint32(p[7]) << 24;
        if ((first32 | second32) & 0x80808080 != 0) { 
            // Found a non ASCII byte (>= RuneSelf).
            break;

        }
        p = p[(int)8..];

    }
    var n = len(p);
    {
        nint i = 0;

        while (i < n) {
            var pi = p[i];
            if (pi < RuneSelf) {
                i++;
                continue;
            }
            var x = first[pi];
            if (x == xx) {
                return false; // Illegal starter byte.
            }

            var size = int(x & 7);
            if (i + size > n) {
                return false; // Short or invalid.
            }

            var accept = acceptRanges[x >> 4];
            {
                var c__prev1 = c;

                var c = p[i + 1];

                if (c < accept.lo || accept.hi < c) {
                    return false;
                }
                else if (size == 2)                 }                {
                    var c__prev3 = c;

                    c = p[i + 2];


                    else if (c < locb || hicb < c) {
                        return false;
                    }
                    else if (size == 3)                     }                    {
                        var c__prev5 = c;

                        c = p[i + 3];


                        else if (c < locb || hicb < c) {
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
public static bool ValidString(@string s) { 
    // Fast path. Check for and skip 8 bytes of ASCII characters per iteration.
    while (len(s) >= 8) { 
        // Combining two 32 bit loads allows the same code to be used
        // for 32 and 64 bit platforms.
        // The compiler can generate a 32bit load for first32 and second32
        // on many platforms. See test/codegen/memcombine.go.
        var first32 = uint32(s[0]) | uint32(s[1]) << 8 | uint32(s[2]) << 16 | uint32(s[3]) << 24;
        var second32 = uint32(s[4]) | uint32(s[5]) << 8 | uint32(s[6]) << 16 | uint32(s[7]) << 24;
        if ((first32 | second32) & 0x80808080 != 0) { 
            // Found a non ASCII byte (>= RuneSelf).
            break;

        }
        s = s[(int)8..];

    }
    var n = len(s);
    {
        nint i = 0;

        while (i < n) {
            var si = s[i];
            if (si < RuneSelf) {
                i++;
                continue;
            }
            var x = first[si];
            if (x == xx) {
                return false; // Illegal starter byte.
            }

            var size = int(x & 7);
            if (i + size > n) {
                return false; // Short or invalid.
            }

            var accept = acceptRanges[x >> 4];
            {
                var c__prev1 = c;

                var c = s[i + 1];

                if (c < accept.lo || accept.hi < c) {
                    return false;
                }
                else if (size == 2)                 }                {
                    var c__prev3 = c;

                    c = s[i + 2];


                    else if (c < locb || hicb < c) {
                        return false;
                    }
                    else if (size == 3)                     }                    {
                        var c__prev5 = c;

                        c = s[i + 3];


                        else if (c < locb || hicb < c) {
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
public static bool ValidRune(int r) {

    if (0 <= r && r < surrogateMin) 
        return true;
    else if (surrogateMax < r && r <= MaxRune) 
        return true;
        return false;

}

} // end utf8_package
