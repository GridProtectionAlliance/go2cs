// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2022 March 06 22:12:27 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\utf8.go


namespace go;

public static partial class runtime_package {

    // Numbers fundamental to the encoding.
private static readonly char runeError = '\uFFFD'; // the "error" Rune or "Unicode replacement character"
private static readonly nuint runeSelf = 0x80; // characters below runeSelf are represented as themselves in a single byte.
private static readonly char maxRune = '\U0010FFFF'; // Maximum valid Unicode code point.

// Code points in the surrogate range are not valid for UTF-8.
private static readonly nuint surrogateMin = 0xD800;
private static readonly nuint surrogateMax = 0xDFFF;


private static readonly nuint t1 = 0x00; // 0000 0000
private static readonly nuint tx = 0x80; // 1000 0000
private static readonly nuint t2 = 0xC0; // 1100 0000
private static readonly nuint t3 = 0xE0; // 1110 0000
private static readonly nuint t4 = 0xF0; // 1111 0000
private static readonly nuint t5 = 0xF8; // 1111 1000

private static readonly nuint maskx = 0x3F; // 0011 1111
private static readonly nuint mask2 = 0x1F; // 0001 1111
private static readonly nuint mask3 = 0x0F; // 0000 1111
private static readonly nuint mask4 = 0x07; // 0000 0111

private static readonly nint rune1Max = 1 << 7 - 1;
private static readonly nint rune2Max = 1 << 11 - 1;
private static readonly nint rune3Max = 1 << 16 - 1; 

// The default lowest and highest continuation byte.
private static readonly nuint locb = 0x80; // 1000 0000
private static readonly nuint hicb = 0xBF; // 1011 1111

// countrunes returns the number of runes in s.
private static nint countrunes(@string s) {
    nint n = 0;
    foreach (>>MARKER:FORRANGEEXPRESSIONS_LEVEL_1<< in s) {>>MARKER:FORRANGEMUTABLEEXPRESSIONS_LEVEL_1<<
        n++;
    }    return n;
}

// decoderune returns the non-ASCII rune at the start of
// s[k:] and the index after the rune in s.
//
// decoderune assumes that caller has checked that
// the to be decoded rune is a non-ASCII rune.
//
// If the string appears to be incomplete or decoding problems
// are encountered (runeerror, k + 1) is returned to ensure
// progress when decoderune is used to iterate over a string.
private static (int, nint) decoderune(@string s, nint k) {
    int r = default;
    nint pos = default;

    pos = k;

    if (k >= len(s)) {
        return (runeError, k + 1);
    }
    s = s[(int)k..];


    if (t2 <= s[0] && s[0] < t3) 
        // 0080-07FF two byte sequence
        if (len(s) > 1 && (locb <= s[1] && s[1] <= hicb)) {
            r = rune(s[0] & mask2) << 6 | rune(s[1] & maskx);
            pos += 2;
            if (rune1Max < r) {
                return ;
            }
        }
    else if (t3 <= s[0] && s[0] < t4) 
        // 0800-FFFF three byte sequence
        if (len(s) > 2 && (locb <= s[1] && s[1] <= hicb) && (locb <= s[2] && s[2] <= hicb)) {
            r = rune(s[0] & mask3) << 12 | rune(s[1] & maskx) << 6 | rune(s[2] & maskx);
            pos += 3;
            if (rune2Max < r && !(surrogateMin <= r && r <= surrogateMax)) {
                return ;
            }
        }
    else if (t4 <= s[0] && s[0] < t5) 
        // 10000-1FFFFF four byte sequence
        if (len(s) > 3 && (locb <= s[1] && s[1] <= hicb) && (locb <= s[2] && s[2] <= hicb) && (locb <= s[3] && s[3] <= hicb)) {
            r = rune(s[0] & mask4) << 18 | rune(s[1] & maskx) << 12 | rune(s[2] & maskx) << 6 | rune(s[3] & maskx);
            pos += 4;
            if (rune3Max < r && r <= maxRune) {
                return ;
            }
        }
        return (runeError, k + 1);

}

// encoderune writes into p (which must be large enough) the UTF-8 encoding of the rune.
// It returns the number of bytes written.
private static nint encoderune(slice<byte> p, int r) { 
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
        if (i > maxRune || surrogateMin <= i && i <= surrogateMax)
        {
            r = runeError;
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

} // end runtime_package
