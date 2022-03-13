// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package bytes implements functions for the manipulation of byte slices.
// It is analogous to the facilities of the strings package.

// package bytes -- go2cs converted at 2022 March 13 05:28:20 UTC
// import "bytes" ==> using bytes = go.bytes_package
// Original source: C:\Program Files\Go\src\bytes\bytes.go
namespace go;

using bytealg = @internal.bytealg_package;
using unicode = unicode_package;
using utf8 = unicode.utf8_package;


// Equal reports whether a and b
// are the same length and contain the same bytes.
// A nil argument is equivalent to an empty slice.

using System;
public static partial class bytes_package {

public static bool Equal(slice<byte> a, slice<byte> b) { 
    // Neither cmd/compile nor gccgo allocates for these string conversions.
    return string(a) == string(b);
}

// Compare returns an integer comparing two byte slices lexicographically.
// The result will be 0 if a==b, -1 if a < b, and +1 if a > b.
// A nil argument is equivalent to an empty slice.
public static nint Compare(slice<byte> a, slice<byte> b) {
    return bytealg.Compare(a, b);
}

// explode splits s into a slice of UTF-8 sequences, one per Unicode code point (still slices of bytes),
// up to a maximum of n byte slices. Invalid UTF-8 sequences are chopped into individual bytes.
private static slice<slice<byte>> explode(slice<byte> s, nint n) {
    if (n <= 0) {
        n = len(s);
    }
    var a = make_slice<slice<byte>>(n);
    nint size = default;
    nint na = 0;
    while (len(s) > 0) {
        if (na + 1 >= n) {
            a[na] = s;
            na++;
            break;
        }
        _, size = utf8.DecodeRune(s);
        a[na] = s.slice(0, size, size);
        s = s[(int)size..];
        na++;
    }
    return a[(int)0..(int)na];
}

// Count counts the number of non-overlapping instances of sep in s.
// If sep is an empty slice, Count returns 1 + the number of UTF-8-encoded code points in s.
public static nint Count(slice<byte> s, slice<byte> sep) { 
    // special case
    if (len(sep) == 0) {
        return utf8.RuneCount(s) + 1;
    }
    if (len(sep) == 1) {
        return bytealg.Count(s, sep[0]);
    }
    nint n = 0;
    while (true) {
        var i = Index(s, sep);
        if (i == -1) {
            return n;
        }
        n++;
        s = s[(int)i + len(sep)..];
    }
}

// Contains reports whether subslice is within b.
public static bool Contains(slice<byte> b, slice<byte> subslice) {
    return Index(b, subslice) != -1;
}

// ContainsAny reports whether any of the UTF-8-encoded code points in chars are within b.
public static bool ContainsAny(slice<byte> b, @string chars) {
    return IndexAny(b, chars) >= 0;
}

// ContainsRune reports whether the rune is contained in the UTF-8-encoded byte slice b.
public static bool ContainsRune(slice<byte> b, int r) {
    return IndexRune(b, r) >= 0;
}

// IndexByte returns the index of the first instance of c in b, or -1 if c is not present in b.
public static nint IndexByte(slice<byte> b, byte c) {
    return bytealg.IndexByte(b, c);
}

private static nint indexBytePortable(slice<byte> s, byte c) {
    foreach (var (i, b) in s) {
        if (b == c) {
            return i;
        }
    }    return -1;
}

// LastIndex returns the index of the last instance of sep in s, or -1 if sep is not present in s.
public static nint LastIndex(slice<byte> s, slice<byte> sep) {
    var n = len(sep);

    if (n == 0) 
        return len(s);
    else if (n == 1) 
        return LastIndexByte(s, sep[0]);
    else if (n == len(s)) 
        if (Equal(s, sep)) {
            return 0;
        }
        return -1;
    else if (n > len(s)) 
        return -1;
    // Rabin-Karp search from the end of the string
    var (hashss, pow) = bytealg.HashStrRevBytes(sep);
    var last = len(s) - n;
    uint h = default;
    {
        var i__prev1 = i;

        for (var i = len(s) - 1; i >= last; i--) {
            h = h * bytealg.PrimeRK + uint32(s[i]);
        }

        i = i__prev1;
    }
    if (h == hashss && Equal(s[(int)last..], sep)) {
        return last;
    }
    {
        var i__prev1 = i;

        for (i = last - 1; i >= 0; i--) {
            h *= bytealg.PrimeRK;
            h += uint32(s[i]);
            h -= pow * uint32(s[i + n]);
            if (h == hashss && Equal(s[(int)i..(int)i + n], sep)) {
                return i;
            }
        }

        i = i__prev1;
    }
    return -1;
}

// LastIndexByte returns the index of the last instance of c in s, or -1 if c is not present in s.
public static nint LastIndexByte(slice<byte> s, byte c) {
    for (var i = len(s) - 1; i >= 0; i--) {
        if (s[i] == c) {
            return i;
        }
    }
    return -1;
}

// IndexRune interprets s as a sequence of UTF-8-encoded code points.
// It returns the byte index of the first occurrence in s of the given rune.
// It returns -1 if rune is not present in s.
// If r is utf8.RuneError, it returns the first instance of any
// invalid UTF-8 byte sequence.
public static nint IndexRune(slice<byte> s, int r) {

    if (0 <= r && r < utf8.RuneSelf) 
        return IndexByte(s, byte(r));
    else if (r == utf8.RuneError) 
        {
            nint i = 0;

            while (i < len(s)) {
                var (r1, n) = utf8.DecodeRune(s[(int)i..]);
                if (r1 == utf8.RuneError) {
                    return i;
                }
                i += n;
            }

        }
        return -1;
    else if (!utf8.ValidRune(r)) 
        return -1;
    else 
        array<byte> b = new array<byte>(utf8.UTFMax);
        var n = utf8.EncodeRune(b[..], r);
        return Index(s, b[..(int)n]);
    }

// IndexAny interprets s as a sequence of UTF-8-encoded Unicode code points.
// It returns the byte index of the first occurrence in s of any of the Unicode
// code points in chars. It returns -1 if chars is empty or if there is no code
// point in common.
public static nint IndexAny(slice<byte> s, @string chars) {
    if (chars == "") { 
        // Avoid scanning all of s.
        return -1;
    }
    if (len(s) == 1) {
        var r = rune(s[0]);
        if (r >= utf8.RuneSelf) { 
            // search utf8.RuneError.
            foreach (var (_, __r) in chars) {
                r = __r;
                if (r == utf8.RuneError) {
                    return 0;
                }
            }

            return -1;
        }
        if (bytealg.IndexByteString(chars, s[0]) >= 0) {
            return 0;
        }
        return -1;
    }
    if (len(chars) == 1) {
        r = rune(chars[0]);
        if (r >= utf8.RuneSelf) {
            r = utf8.RuneError;
        }
        return IndexRune(s, r);
    }
    if (len(s) > 8) {
        {
            var (as, isASCII) = makeASCIISet(chars);

            if (isASCII) {
                {
                    var i__prev1 = i;

                    foreach (var (__i, __c) in s) {
                        i = __i;
                        c = __c;
                        if (@as.contains(c)) {
                            return i;
                        }
                    }

                    i = i__prev1;
                }

                return -1;
            }

        }
    }
    nint width = default;
    {
        var i__prev1 = i;

        nint i = 0;

        while (i < len(s)) {
            r = rune(s[i]);
            if (r < utf8.RuneSelf) {
                if (bytealg.IndexByteString(chars, s[i]) >= 0) {
                    return i;
            i += width;
                }
                width = 1;
                continue;
            }
            r, width = utf8.DecodeRune(s[(int)i..]);
            if (r != utf8.RuneError) { 
                // r is 2 to 4 bytes
                if (len(chars) == width) {
                    if (chars == string(r)) {
                        return i;
                    }
                    continue;
                } 
                // Use bytealg.IndexString for performance if available.
                if (bytealg.MaxLen >= width) {
                    if (bytealg.IndexString(chars, string(r)) >= 0) {
                        return i;
                    }
                    continue;
                }
            }
            foreach (var (_, ch) in chars) {
                if (r == ch) {
                    return i;
                }
            }
        }

        i = i__prev1;
    }
    return -1;
}

// LastIndexAny interprets s as a sequence of UTF-8-encoded Unicode code
// points. It returns the byte index of the last occurrence in s of any of
// the Unicode code points in chars. It returns -1 if chars is empty or if
// there is no code point in common.
public static nint LastIndexAny(slice<byte> s, @string chars) {
    if (chars == "") { 
        // Avoid scanning all of s.
        return -1;
    }
    if (len(s) > 8) {
        {
            var (as, isASCII) = makeASCIISet(chars);

            if (isASCII) {
                {
                    var i__prev1 = i;

                    for (var i = len(s) - 1; i >= 0; i--) {
                        if (@as.contains(s[i])) {
                            return i;
                        }
                    }


                    i = i__prev1;
                }
                return -1;
            }

        }
    }
    if (len(s) == 1) {
        var r = rune(s[0]);
        if (r >= utf8.RuneSelf) {
            foreach (var (_, __r) in chars) {
                r = __r;
                if (r == utf8.RuneError) {
                    return 0;
                }
            }

            return -1;
        }
        if (bytealg.IndexByteString(chars, s[0]) >= 0) {
            return 0;
        }
        return -1;
    }
    if (len(chars) == 1) {
        var cr = rune(chars[0]);
        if (cr >= utf8.RuneSelf) {
            cr = utf8.RuneError;
        }
        {
            var i__prev1 = i;

            i = len(s);

            while (i > 0) {
                var (r, size) = utf8.DecodeLastRune(s[..(int)i]);
                i -= size;
                if (r == cr) {
                    return i;
                }
            }


            i = i__prev1;
        }
        return -1;
    }
    {
        var i__prev1 = i;

        i = len(s);

        while (i > 0) {
            r = rune(s[i - 1]);
            if (r < utf8.RuneSelf) {
                if (bytealg.IndexByteString(chars, s[i - 1]) >= 0) {
                    return i - 1;
                }
                i--;
                continue;
            }
            (r, size) = utf8.DecodeLastRune(s[..(int)i]);
            i -= size;
            if (r != utf8.RuneError) { 
                // r is 2 to 4 bytes
                if (len(chars) == size) {
                    if (chars == string(r)) {
                        return i;
                    }
                    continue;
                } 
                // Use bytealg.IndexString for performance if available.
                if (bytealg.MaxLen >= size) {
                    if (bytealg.IndexString(chars, string(r)) >= 0) {
                        return i;
                    }
                    continue;
                }
            }
            foreach (var (_, ch) in chars) {
                if (r == ch) {
                    return i;
                }
            }
        }

        i = i__prev1;
    }
    return -1;
}

// Generic split: splits after each instance of sep,
// including sepSave bytes of sep in the subslices.
private static slice<slice<byte>> genSplit(slice<byte> s, slice<byte> sep, nint sepSave, nint n) {
    if (n == 0) {
        return null;
    }
    if (len(sep) == 0) {
        return explode(s, n);
    }
    if (n < 0) {
        n = Count(s, sep) + 1;
    }
    var a = make_slice<slice<byte>>(n);
    n--;
    nint i = 0;
    while (i < n) {
        var m = Index(s, sep);
        if (m < 0) {
            break;
        }
        a[i] = s.slice(-1, m + sepSave, m + sepSave);
        s = s[(int)m + len(sep)..];
        i++;
    }
    a[i] = s;
    return a[..(int)i + 1];
}

// SplitN slices s into subslices separated by sep and returns a slice of
// the subslices between those separators.
// If sep is empty, SplitN splits after each UTF-8 sequence.
// The count determines the number of subslices to return:
//   n > 0: at most n subslices; the last subslice will be the unsplit remainder.
//   n == 0: the result is nil (zero subslices)
//   n < 0: all subslices
public static slice<slice<byte>> SplitN(slice<byte> s, slice<byte> sep, nint n) {
    return genSplit(s, sep, 0, n);
}

// SplitAfterN slices s into subslices after each instance of sep and
// returns a slice of those subslices.
// If sep is empty, SplitAfterN splits after each UTF-8 sequence.
// The count determines the number of subslices to return:
//   n > 0: at most n subslices; the last subslice will be the unsplit remainder.
//   n == 0: the result is nil (zero subslices)
//   n < 0: all subslices
public static slice<slice<byte>> SplitAfterN(slice<byte> s, slice<byte> sep, nint n) {
    return genSplit(s, sep, len(sep), n);
}

// Split slices s into all subslices separated by sep and returns a slice of
// the subslices between those separators.
// If sep is empty, Split splits after each UTF-8 sequence.
// It is equivalent to SplitN with a count of -1.
public static slice<slice<byte>> Split(slice<byte> s, slice<byte> sep) {
    return genSplit(s, sep, 0, -1);
}

// SplitAfter slices s into all subslices after each instance of sep and
// returns a slice of those subslices.
// If sep is empty, SplitAfter splits after each UTF-8 sequence.
// It is equivalent to SplitAfterN with a count of -1.
public static slice<slice<byte>> SplitAfter(slice<byte> s, slice<byte> sep) {
    return genSplit(s, sep, len(sep), -1);
}

private static array<byte> asciiSpace = new array<byte>(InitKeyedValues<byte>(256, ('\t', 1), ('\n', 1), ('\v', 1), ('\f', 1), ('\r', 1), (' ', 1)));

// Fields interprets s as a sequence of UTF-8-encoded code points.
// It splits the slice s around each instance of one or more consecutive white space
// characters, as defined by unicode.IsSpace, returning a slice of subslices of s or an
// empty slice if s contains only white space.
public static slice<slice<byte>> Fields(slice<byte> s) { 
    // First count the fields.
    // This is an exact count if s is ASCII, otherwise it is an approximation.
    nint n = 0;
    nint wasSpace = 1; 
    // setBits is used to track which bits are set in the bytes of s.
    var setBits = uint8(0);
    {
        nint i__prev1 = i;

        for (nint i = 0; i < len(s); i++) {
            var r = s[i];
            setBits |= r;
            var isSpace = int(asciiSpace[r]);
            n += wasSpace & ~isSpace;
            wasSpace = isSpace;
        }

        i = i__prev1;
    }

    if (setBits >= utf8.RuneSelf) { 
        // Some runes in the input slice are not ASCII.
        return FieldsFunc(s, unicode.IsSpace);
    }
    var a = make_slice<slice<byte>>(n);
    nint na = 0;
    nint fieldStart = 0;
    i = 0; 
    // Skip spaces in the front of the input.
    while (i < len(s) && asciiSpace[s[i]] != 0) {
        i++;
    }
    fieldStart = i;
    while (i < len(s)) {
        if (asciiSpace[s[i]] == 0) {
            i++;
            continue;
        }
        a[na] = s.slice(fieldStart, i, i);
        na++;
        i++; 
        // Skip spaces in between fields.
        while (i < len(s) && asciiSpace[s[i]] != 0) {
            i++;
        }
        fieldStart = i;
    }
    if (fieldStart < len(s)) { // Last field might end at EOF.
        a[na] = s.slice(fieldStart, len(s), len(s));
    }
    return a;
}

// FieldsFunc interprets s as a sequence of UTF-8-encoded code points.
// It splits the slice s at each run of code points c satisfying f(c) and
// returns a slice of subslices of s. If all code points in s satisfy f(c), or
// len(s) == 0, an empty slice is returned.
//
// FieldsFunc makes no guarantees about the order in which it calls f(c)
// and assumes that f always returns the same value for a given c.
public static slice<slice<byte>> FieldsFunc(slice<byte> s, Func<int, bool> f) { 
    // A span is used to record a slice of s of the form s[start:end].
    // The start index is inclusive and the end index is exclusive.
    private partial struct span {
        public nint start;
        public nint end;
    }
    var spans = make_slice<span>(0, 32); 

    // Find the field start and end indices.
    // Doing this in a separate pass (rather than slicing the string s
    // and collecting the result substrings right away) is significantly
    // more efficient, possibly due to cache effects.
    nint start = -1; // valid span start if >= 0
    {
        nint i__prev1 = i;

        nint i = 0;

        while (i < len(s)) {
            nint size = 1;
            var r = rune(s[i]);
            if (r >= utf8.RuneSelf) {
                r, size = utf8.DecodeRune(s[(int)i..]);
            }
            if (f(r)) {
                if (start >= 0) {
                    spans = append(spans, new span(start,i));
                    start = -1;
                }
            }
            else
 {
                if (start < 0) {
                    start = i;
                }
            }
            i += size;
        }

        i = i__prev1;
    } 

    // Last field might end at EOF.
    if (start >= 0) {
        spans = append(spans, new span(start,len(s)));
    }
    var a = make_slice<slice<byte>>(len(spans));
    {
        nint i__prev1 = i;

        foreach (var (__i, __span) in spans) {
            i = __i;
            span = __span;
            a[i] = s.slice(span.start, span.end, span.end);
        }
        i = i__prev1;
    }

    return a;
}

// Join concatenates the elements of s to create a new byte slice. The separator
// sep is placed between elements in the resulting slice.
public static slice<byte> Join(slice<slice<byte>> s, slice<byte> sep) {
    if (len(s) == 0) {
        return new slice<byte>(new byte[] {  });
    }
    if (len(s) == 1) { 
        // Just return a copy.
        return append((slice<byte>)null, s[0]);
    }
    var n = len(sep) * (len(s) - 1);
    {
        var v__prev1 = v;

        foreach (var (_, __v) in s) {
            v = __v;
            n += len(v);
        }
        v = v__prev1;
    }

    var b = make_slice<byte>(n);
    var bp = copy(b, s[0]);
    {
        var v__prev1 = v;

        foreach (var (_, __v) in s[(int)1..]) {
            v = __v;
            bp += copy(b[(int)bp..], sep);
            bp += copy(b[(int)bp..], v);
        }
        v = v__prev1;
    }

    return b;
}

// HasPrefix tests whether the byte slice s begins with prefix.
public static bool HasPrefix(slice<byte> s, slice<byte> prefix) {
    return len(s) >= len(prefix) && Equal(s[(int)0..(int)len(prefix)], prefix);
}

// HasSuffix tests whether the byte slice s ends with suffix.
public static bool HasSuffix(slice<byte> s, slice<byte> suffix) {
    return len(s) >= len(suffix) && Equal(s[(int)len(s) - len(suffix)..], suffix);
}

// Map returns a copy of the byte slice s with all its characters modified
// according to the mapping function. If mapping returns a negative value, the character is
// dropped from the byte slice with no replacement. The characters in s and the
// output are interpreted as UTF-8-encoded code points.
public static slice<byte> Map(Func<int, int> mapping, slice<byte> s) { 
    // In the worst case, the slice can grow when mapped, making
    // things unpleasant. But it's so rare we barge in assuming it's
    // fine. It could also shrink but that falls out naturally.
    var maxbytes = len(s); // length of b
    nint nbytes = 0; // number of bytes encoded in b
    var b = make_slice<byte>(maxbytes);
    {
        nint i = 0;

        while (i < len(s)) {
            nint wid = 1;
            var r = rune(s[i]);
            if (r >= utf8.RuneSelf) {
                r, wid = utf8.DecodeRune(s[(int)i..]);
            }
            r = mapping(r);
            if (r >= 0) {
                var rl = utf8.RuneLen(r);
                if (rl < 0) {
                    rl = len(string(utf8.RuneError));
                }
                if (nbytes + rl > maxbytes) { 
                    // Grow the buffer.
                    maxbytes = maxbytes * 2 + utf8.UTFMax;
                    var nb = make_slice<byte>(maxbytes);
                    copy(nb, b[(int)0..(int)nbytes]);
                    b = nb;
                }
                nbytes += utf8.EncodeRune(b[(int)nbytes..(int)maxbytes], r);
            }
            i += wid;
        }
    }
    return b[(int)0..(int)nbytes];
}

// Repeat returns a new byte slice consisting of count copies of b.
//
// It panics if count is negative or if
// the result of (len(b) * count) overflows.
public static slice<byte> Repeat(slice<byte> b, nint count) => func((_, panic, _) => {
    if (count == 0) {
        return new slice<byte>(new byte[] {  });
    }
    if (count < 0) {
        panic("bytes: negative Repeat count");
    }
    else if (len(b) * count / count != len(b)) {
        panic("bytes: Repeat count causes overflow");
    }
    var nb = make_slice<byte>(len(b) * count);
    var bp = copy(nb, b);
    while (bp < len(nb)) {
        copy(nb[(int)bp..], nb[..(int)bp]);
        bp *= 2;
    }
    return nb;
});

// ToUpper returns a copy of the byte slice s with all Unicode letters mapped to
// their upper case.
public static slice<byte> ToUpper(slice<byte> s) {
    var isASCII = true;
    var hasLower = false;
    {
        nint i__prev1 = i;

        for (nint i = 0; i < len(s); i++) {
            var c = s[i];
            if (c >= utf8.RuneSelf) {
                isASCII = false;
                break;
            }
            hasLower = hasLower || ('a' <= c && c <= 'z');
        }

        i = i__prev1;
    }

    if (isASCII) { // optimize for ASCII-only byte slices.
        if (!hasLower) { 
            // Just return a copy.
            return append((slice<byte>)"", s);
        }
        var b = make_slice<byte>(len(s));
        {
            nint i__prev1 = i;

            for (i = 0; i < len(s); i++) {
                c = s[i];
                if ('a' <= c && c <= 'z') {
                    c -= 'a' - 'A';
                }
                b[i] = c;
            }


            i = i__prev1;
        }
        return b;
    }
    return Map(unicode.ToUpper, s);
}

// ToLower returns a copy of the byte slice s with all Unicode letters mapped to
// their lower case.
public static slice<byte> ToLower(slice<byte> s) {
    var isASCII = true;
    var hasUpper = false;
    {
        nint i__prev1 = i;

        for (nint i = 0; i < len(s); i++) {
            var c = s[i];
            if (c >= utf8.RuneSelf) {
                isASCII = false;
                break;
            }
            hasUpper = hasUpper || ('A' <= c && c <= 'Z');
        }

        i = i__prev1;
    }

    if (isASCII) { // optimize for ASCII-only byte slices.
        if (!hasUpper) {
            return append((slice<byte>)"", s);
        }
        var b = make_slice<byte>(len(s));
        {
            nint i__prev1 = i;

            for (i = 0; i < len(s); i++) {
                c = s[i];
                if ('A' <= c && c <= 'Z') {
                    c += 'a' - 'A';
                }
                b[i] = c;
            }


            i = i__prev1;
        }
        return b;
    }
    return Map(unicode.ToLower, s);
}

// ToTitle treats s as UTF-8-encoded bytes and returns a copy with all the Unicode letters mapped to their title case.
public static slice<byte> ToTitle(slice<byte> s) {
    return Map(unicode.ToTitle, s);
}

// ToUpperSpecial treats s as UTF-8-encoded bytes and returns a copy with all the Unicode letters mapped to their
// upper case, giving priority to the special casing rules.
public static slice<byte> ToUpperSpecial(unicode.SpecialCase c, slice<byte> s) {
    return Map(c.ToUpper, s);
}

// ToLowerSpecial treats s as UTF-8-encoded bytes and returns a copy with all the Unicode letters mapped to their
// lower case, giving priority to the special casing rules.
public static slice<byte> ToLowerSpecial(unicode.SpecialCase c, slice<byte> s) {
    return Map(c.ToLower, s);
}

// ToTitleSpecial treats s as UTF-8-encoded bytes and returns a copy with all the Unicode letters mapped to their
// title case, giving priority to the special casing rules.
public static slice<byte> ToTitleSpecial(unicode.SpecialCase c, slice<byte> s) {
    return Map(c.ToTitle, s);
}

// ToValidUTF8 treats s as UTF-8-encoded bytes and returns a copy with each run of bytes
// representing invalid UTF-8 replaced with the bytes in replacement, which may be empty.
public static slice<byte> ToValidUTF8(slice<byte> s, slice<byte> replacement) {
    var b = make_slice<byte>(0, len(s) + len(replacement));
    var invalid = false; // previous byte was from an invalid UTF-8 sequence
    {
        nint i = 0;

        while (i < len(s)) {
            var c = s[i];
            if (c < utf8.RuneSelf) {
                i++;
                invalid = false;
                b = append(b, byte(c));
                continue;
            }
            var (_, wid) = utf8.DecodeRune(s[(int)i..]);
            if (wid == 1) {
                i++;
                if (!invalid) {
                    invalid = true;
                    b = append(b, replacement);
                }
                continue;
            }
            invalid = false;
            b = append(b, s[(int)i..(int)i + wid]);
            i += wid;
        }
    }
    return b;
}

// isSeparator reports whether the rune could mark a word boundary.
// TODO: update when package unicode captures more of the properties.
private static bool isSeparator(int r) { 
    // ASCII alphanumerics and underscore are not separators
    if (r <= 0x7F) {

        if ('0' <= r && r <= '9') 
            return false;
        else if ('a' <= r && r <= 'z') 
            return false;
        else if ('A' <= r && r <= 'Z') 
            return false;
        else if (r == '_') 
            return false;
                return true;
    }
    if (unicode.IsLetter(r) || unicode.IsDigit(r)) {
        return false;
    }
    return unicode.IsSpace(r);
}

// Title treats s as UTF-8-encoded bytes and returns a copy with all Unicode letters that begin
// words mapped to their title case.
//
// BUG(rsc): The rule Title uses for word boundaries does not handle Unicode punctuation properly.
public static slice<byte> Title(slice<byte> s) { 
    // Use a closure here to remember state.
    // Hackish but effective. Depends on Map scanning in order and calling
    // the closure once per rune.
    char prev = ' ';
    return Map(r => {
        if (isSeparator(prev)) {
            prev = r;
            return unicode.ToTitle(r);
        }
        prev = r;
        return r;
    }, s);
}

// TrimLeftFunc treats s as UTF-8-encoded bytes and returns a subslice of s by slicing off
// all leading UTF-8-encoded code points c that satisfy f(c).
public static slice<byte> TrimLeftFunc(slice<byte> s, Func<int, bool> f) {
    var i = indexFunc(s, f, false);
    if (i == -1) {
        return null;
    }
    return s[(int)i..];
}

// TrimRightFunc returns a subslice of s by slicing off all trailing
// UTF-8-encoded code points c that satisfy f(c).
public static slice<byte> TrimRightFunc(slice<byte> s, Func<int, bool> f) {
    var i = lastIndexFunc(s, f, false);
    if (i >= 0 && s[i] >= utf8.RuneSelf) {
        var (_, wid) = utf8.DecodeRune(s[(int)i..]);
        i += wid;
    }
    else
 {
        i++;
    }
    return s[(int)0..(int)i];
}

// TrimFunc returns a subslice of s by slicing off all leading and trailing
// UTF-8-encoded code points c that satisfy f(c).
public static slice<byte> TrimFunc(slice<byte> s, Func<int, bool> f) {
    return TrimRightFunc(TrimLeftFunc(s, f), f);
}

// TrimPrefix returns s without the provided leading prefix string.
// If s doesn't start with prefix, s is returned unchanged.
public static slice<byte> TrimPrefix(slice<byte> s, slice<byte> prefix) {
    if (HasPrefix(s, prefix)) {
        return s[(int)len(prefix)..];
    }
    return s;
}

// TrimSuffix returns s without the provided trailing suffix string.
// If s doesn't end with suffix, s is returned unchanged.
public static slice<byte> TrimSuffix(slice<byte> s, slice<byte> suffix) {
    if (HasSuffix(s, suffix)) {
        return s[..(int)len(s) - len(suffix)];
    }
    return s;
}

// IndexFunc interprets s as a sequence of UTF-8-encoded code points.
// It returns the byte index in s of the first Unicode
// code point satisfying f(c), or -1 if none do.
public static nint IndexFunc(slice<byte> s, Func<int, bool> f) {
    return indexFunc(s, f, true);
}

// LastIndexFunc interprets s as a sequence of UTF-8-encoded code points.
// It returns the byte index in s of the last Unicode
// code point satisfying f(c), or -1 if none do.
public static nint LastIndexFunc(slice<byte> s, Func<int, bool> f) {
    return lastIndexFunc(s, f, true);
}

// indexFunc is the same as IndexFunc except that if
// truth==false, the sense of the predicate function is
// inverted.
private static nint indexFunc(slice<byte> s, Func<int, bool> f, bool truth) {
    nint start = 0;
    while (start < len(s)) {
        nint wid = 1;
        var r = rune(s[start]);
        if (r >= utf8.RuneSelf) {
            r, wid = utf8.DecodeRune(s[(int)start..]);
        }
        if (f(r) == truth) {
            return start;
        }
        start += wid;
    }
    return -1;
}

// lastIndexFunc is the same as LastIndexFunc except that if
// truth==false, the sense of the predicate function is
// inverted.
private static nint lastIndexFunc(slice<byte> s, Func<int, bool> f, bool truth) {
    {
        var i = len(s);

        while (i > 0) {
            var r = rune(s[i - 1]);
            nint size = 1;
            if (r >= utf8.RuneSelf) {
                r, size = utf8.DecodeLastRune(s[(int)0..(int)i]);
            }
            i -= size;
            if (f(r) == truth) {
                return i;
            }
        }
    }
    return -1;
}

// asciiSet is a 32-byte value, where each bit represents the presence of a
// given ASCII character in the set. The 128-bits of the lower 16 bytes,
// starting with the least-significant bit of the lowest word to the
// most-significant bit of the highest word, map to the full range of all
// 128 ASCII characters. The 128-bits of the upper 16 bytes will be zeroed,
// ensuring that any non-ASCII character will be reported as not in the set.
private partial struct asciiSet { // : array<uint>
}

// makeASCIISet creates a set of ASCII characters and reports whether all
// characters in chars are ASCII.
private static (asciiSet, bool) makeASCIISet(@string chars) {
    asciiSet @as = default;
    bool ok = default;

    for (nint i = 0; i < len(chars); i++) {
        var c = chars[i];
        if (c >= utf8.RuneSelf) {
            return (as, false);
        }
        as[c >> 5] |= 1 << (int)(uint(c & 31));
    }
    return (as, true);
}

// contains reports whether c is inside the set.
private static bool contains(this ptr<asciiSet> _addr_@as, byte c) {
    ref asciiSet @as = ref _addr_@as.val;

    return (as[c >> 5] & (1 << (int)(uint(c & 31)))) != 0;
}

private static Func<int, bool> makeCutsetFunc(@string cutset) {
    if (len(cutset) == 1 && cutset[0] < utf8.RuneSelf) {
        return r => r == rune(cutset[0]);
    }
    {
        var (as, isASCII) = makeASCIISet(cutset);

        if (isASCII) {
            return r => r < utf8.RuneSelf && @as.contains(byte(r));
        }
    }
    return r => {
        foreach (var (_, c) in cutset) {
            if (c == r) {
                return true;
            }
        }        return false;
    };
}

// Trim returns a subslice of s by slicing off all leading and
// trailing UTF-8-encoded code points contained in cutset.
public static slice<byte> Trim(slice<byte> s, @string cutset) {
    return TrimFunc(s, makeCutsetFunc(cutset));
}

// TrimLeft returns a subslice of s by slicing off all leading
// UTF-8-encoded code points contained in cutset.
public static slice<byte> TrimLeft(slice<byte> s, @string cutset) {
    return TrimLeftFunc(s, makeCutsetFunc(cutset));
}

// TrimRight returns a subslice of s by slicing off all trailing
// UTF-8-encoded code points that are contained in cutset.
public static slice<byte> TrimRight(slice<byte> s, @string cutset) {
    return TrimRightFunc(s, makeCutsetFunc(cutset));
}

// TrimSpace returns a subslice of s by slicing off all leading and
// trailing white space, as defined by Unicode.
public static slice<byte> TrimSpace(slice<byte> s) { 
    // Fast path for ASCII: look for the first ASCII non-space byte
    nint start = 0;
    while (start < len(s)) {
        var c = s[start];
        if (c >= utf8.RuneSelf) { 
            // If we run into a non-ASCII byte, fall back to the
            // slower unicode-aware method on the remaining bytes
            return TrimFunc(s[(int)start..], unicode.IsSpace);
        start++;
        }
        if (asciiSpace[c] == 0) {
            break;
        }
    } 

    // Now look for the first ASCII non-space byte from the end
    var stop = len(s);
    while (stop > start) {
        c = s[stop - 1];
        if (c >= utf8.RuneSelf) {
            return TrimFunc(s[(int)start..(int)stop], unicode.IsSpace);
        stop--;
        }
        if (asciiSpace[c] == 0) {
            break;
        }
    } 

    // At this point s[start:stop] starts and ends with an ASCII
    // non-space bytes, so we're done. Non-ASCII cases have already
    // been handled above.
    if (start == stop) { 
        // Special case to preserve previous TrimLeftFunc behavior,
        // returning nil instead of empty slice if all spaces.
        return null;
    }
    return s[(int)start..(int)stop];
}

// Runes interprets s as a sequence of UTF-8-encoded code points.
// It returns a slice of runes (Unicode code points) equivalent to s.
public static slice<int> Runes(slice<byte> s) {
    var t = make_slice<int>(utf8.RuneCount(s));
    nint i = 0;
    while (len(s) > 0) {
        var (r, l) = utf8.DecodeRune(s);
        t[i] = r;
        i++;
        s = s[(int)l..];
    }
    return t;
}

// Replace returns a copy of the slice s with the first n
// non-overlapping instances of old replaced by new.
// If old is empty, it matches at the beginning of the slice
// and after each UTF-8 sequence, yielding up to k+1 replacements
// for a k-rune slice.
// If n < 0, there is no limit on the number of replacements.
public static slice<byte> Replace(slice<byte> s, slice<byte> old, slice<byte> @new, nint n) {
    nint m = 0;
    if (n != 0) { 
        // Compute number of replacements.
        m = Count(s, old);
    }
    if (m == 0) { 
        // Just return a copy.
        return append((slice<byte>)null, s);
    }
    if (n < 0 || m < n) {
        n = m;
    }
    var t = make_slice<byte>(len(s) + n * (len(new) - len(old)));
    nint w = 0;
    nint start = 0;
    for (nint i = 0; i < n; i++) {
        var j = start;
        if (len(old) == 0) {
            if (i > 0) {
                var (_, wid) = utf8.DecodeRune(s[(int)start..]);
                j += wid;
            }
        }
        else
 {
            j += Index(s[(int)start..], old);
        }
        w += copy(t[(int)w..], s[(int)start..(int)j]);
        w += copy(t[(int)w..], new);
        start = j + len(old);
    }
    w += copy(t[(int)w..], s[(int)start..]);
    return t[(int)0..(int)w];
}

// ReplaceAll returns a copy of the slice s with all
// non-overlapping instances of old replaced by new.
// If old is empty, it matches at the beginning of the slice
// and after each UTF-8 sequence, yielding up to k+1 replacements
// for a k-rune slice.
public static slice<byte> ReplaceAll(slice<byte> s, slice<byte> old, slice<byte> @new) {
    return Replace(s, old, new, -1);
}

// EqualFold reports whether s and t, interpreted as UTF-8 strings,
// are equal under Unicode case-folding, which is a more general
// form of case-insensitivity.
public static bool EqualFold(slice<byte> s, slice<byte> t) {
    while (len(s) != 0 && len(t) != 0) { 
        // Extract first rune from each.
        int sr = default;        int tr = default;

        if (s[0] < utf8.RuneSelf) {
            (sr, s) = (rune(s[0]), s[(int)1..]);
        }
        else
 {
            var (r, size) = utf8.DecodeRune(s);
            (sr, s) = (r, s[(int)size..]);
        }
        if (t[0] < utf8.RuneSelf) {
            (tr, t) = (rune(t[0]), t[(int)1..]);
        }
        else
 {
            (r, size) = utf8.DecodeRune(t);
            (tr, t) = (r, t[(int)size..]);
        }
        if (tr == sr) {
            continue;
        }
        if (tr < sr) {
            (tr, sr) = (sr, tr);
        }
        if (tr < utf8.RuneSelf) { 
            // ASCII only, sr/tr must be upper/lower case
            if ('A' <= sr && sr <= 'Z' && tr == sr + 'a' - 'A') {
                continue;
            }
            return false;
        }
        var r = unicode.SimpleFold(sr);
        while (r != sr && r < tr) {
            r = unicode.SimpleFold(r);
        }
        if (r == tr) {
            continue;
        }
        return false;
    } 

    // One string is empty. Are both?
    return len(s) == len(t);
}

// Index returns the index of the first instance of sep in s, or -1 if sep is not present in s.
public static nint Index(slice<byte> s, slice<byte> sep) {
    var n = len(sep);

    if (n == 0) 
        return 0;
    else if (n == 1) 
        return IndexByte(s, sep[0]);
    else if (n == len(s)) 
        if (Equal(sep, s)) {
            return 0;
        }
        return -1;
    else if (n > len(s)) 
        return -1;
    else if (n <= bytealg.MaxLen) 
        // Use brute force when s and sep both are small
        if (len(s) <= bytealg.MaxBruteForce) {
            return bytealg.Index(s, sep);
        }
        var c0 = sep[0];
        var c1 = sep[1];
        nint i = 0;
        var t = len(s) - n + 1;
        nint fails = 0;
        while (i < t) {
            if (s[i] != c0) { 
                // IndexByte is faster than bytealg.Index, so use it as long as
                // we're not getting lots of false positives.
                var o = IndexByte(s[(int)i + 1..(int)t], c0);
                if (o < 0) {
                    return -1;
                }
                i += o + 1;
            }
            if (s[i + 1] == c1 && Equal(s[(int)i..(int)i + n], sep)) {
                return i;
            }
            fails++;
            i++; 
            // Switch to bytealg.Index when IndexByte produces too many false positives.
            if (fails > bytealg.Cutover(i)) {
                var r = bytealg.Index(s[(int)i..], sep);
                if (r >= 0) {
                    return r + i;
                }
                return -1;
            }
        }
        return -1;
        c0 = sep[0];
    c1 = sep[1];
    i = 0;
    fails = 0;
    t = len(s) - n + 1;
    while (i < t) {
        if (s[i] != c0) {
            o = IndexByte(s[(int)i + 1..(int)t], c0);
            if (o < 0) {
                break;
            }
            i += o + 1;
        }
        if (s[i + 1] == c1 && Equal(s[(int)i..(int)i + n], sep)) {
            return i;
        }
        i++;
        fails++;
        if (fails >= 4 + i >> 4 && i < t) { 
            // Give up on IndexByte, it isn't skipping ahead
            // far enough to be better than Rabin-Karp.
            // Experiments (using IndexPeriodic) suggest
            // the cutover is about 16 byte skips.
            // TODO: if large prefixes of sep are matching
            // we should cutover at even larger average skips,
            // because Equal becomes that much more expensive.
            // This code does not take that effect into account.
            var j = bytealg.IndexRabinKarpBytes(s[(int)i..], sep);
            if (j < 0) {
                return -1;
            }
            return i + j;
        }
    }
    return -1;
}

} // end bytes_package
