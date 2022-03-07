// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package strings implements simple functions to manipulate UTF-8 encoded strings.
//
// For information about UTF-8 strings in Go, see https://blog.golang.org/strings.
// package strings -- go2cs converted at 2022 March 06 22:30:26 UTC
// import "strings" ==> using strings = go.strings_package
// Original source: C:\Program Files\Go\src\strings\strings.go
using bytealg = go.@internal.bytealg_package;
using unicode = go.unicode_package;
using utf8 = go.unicode.utf8_package;
using System;


namespace go;

public static partial class strings_package {

    // explode splits s into a slice of UTF-8 strings,
    // one string per Unicode character up to a maximum of n (n < 0 means no limit).
    // Invalid UTF-8 sequences become correct encodings of U+FFFD.
private static slice<@string> explode(@string s, nint n) {
    var l = utf8.RuneCountInString(s);
    if (n < 0 || n > l) {
        n = l;
    }
    var a = make_slice<@string>(n);
    for (nint i = 0; i < n - 1; i++) {
        var (ch, size) = utf8.DecodeRuneInString(s);
        a[i] = s[..(int)size];
        s = s[(int)size..];
        if (ch == utf8.RuneError) {
            a[i] = string(utf8.RuneError);
        }
    }
    if (n > 0) {
        a[n - 1] = s;
    }
    return a;

}

// Count counts the number of non-overlapping instances of substr in s.
// If substr is an empty string, Count returns 1 + the number of Unicode code points in s.
public static nint Count(@string s, @string substr) { 
    // special case
    if (len(substr) == 0) {
        return utf8.RuneCountInString(s) + 1;
    }
    if (len(substr) == 1) {
        return bytealg.CountString(s, substr[0]);
    }
    nint n = 0;
    while (true) {
        var i = Index(s, substr);
        if (i == -1) {
            return n;
        }
        n++;
        s = s[(int)i + len(substr)..];

    }

}

// Contains reports whether substr is within s.
public static bool Contains(@string s, @string substr) {
    return Index(s, substr) >= 0;
}

// ContainsAny reports whether any Unicode code points in chars are within s.
public static bool ContainsAny(@string s, @string chars) {
    return IndexAny(s, chars) >= 0;
}

// ContainsRune reports whether the Unicode code point r is within s.
public static bool ContainsRune(@string s, int r) {
    return IndexRune(s, r) >= 0;
}

// LastIndex returns the index of the last instance of substr in s, or -1 if substr is not present in s.
public static nint LastIndex(@string s, @string substr) {
    var n = len(substr);

    if (n == 0) 
        return len(s);
    else if (n == 1) 
        return LastIndexByte(s, substr[0]);
    else if (n == len(s)) 
        if (substr == s) {
            return 0;
        }
        return -1;
    else if (n > len(s)) 
        return -1;
    // Rabin-Karp search from the end of the string
    var (hashss, pow) = bytealg.HashStrRev(substr);
    var last = len(s) - n;
    uint h = default;
    {
        var i__prev1 = i;

        for (var i = len(s) - 1; i >= last; i--) {
            h = h * bytealg.PrimeRK + uint32(s[i]);
        }

        i = i__prev1;
    }
    if (h == hashss && s[(int)last..] == substr) {
        return last;
    }
    {
        var i__prev1 = i;

        for (i = last - 1; i >= 0; i--) {
            h *= bytealg.PrimeRK;
            h += uint32(s[i]);
            h -= pow * uint32(s[i + n]);
            if (h == hashss && s[(int)i..(int)i + n] == substr) {
                return i;
            }
        }

        i = i__prev1;
    }
    return -1;

}

// IndexByte returns the index of the first instance of c in s, or -1 if c is not present in s.
public static nint IndexByte(@string s, byte c) {
    return bytealg.IndexByteString(s, c);
}

// IndexRune returns the index of the first instance of the Unicode code point
// r, or -1 if rune is not present in s.
// If r is utf8.RuneError, it returns the first instance of any
// invalid UTF-8 byte sequence.
public static nint IndexRune(@string s, int r) {

    if (0 <= r && r < utf8.RuneSelf) 
        return IndexByte(s, byte(r));
    else if (r == utf8.RuneError) 
        foreach (var (i, r) in s) {
            if (r == utf8.RuneError) {
                return i;
            }
        }        return -1;
    else if (!utf8.ValidRune(r)) 
        return -1;
    else 
        return Index(s, string(r));
    
}

// IndexAny returns the index of the first instance of any Unicode code point
// from chars in s, or -1 if no Unicode code point from chars is present in s.
public static nint IndexAny(@string s, @string chars) {
    if (chars == "") { 
        // Avoid scanning all of s.
        return -1;

    }
    if (len(chars) == 1) { 
        // Avoid scanning all of s.
        var r = rune(chars[0]);
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
                    nint i__prev1 = i;

                    for (nint i = 0; i < len(s); i++) {
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
    {
        nint i__prev1 = i;

        foreach (var (__i, __c) in s) {
            i = __i;
            c = __c;
            if (IndexRune(chars, c) >= 0) {
                return i;
            }
        }
        i = i__prev1;
    }

    return -1;

}

// LastIndexAny returns the index of the last instance of any Unicode code
// point from chars in s, or -1 if no Unicode code point from chars is
// present in s.
public static nint LastIndexAny(@string s, @string chars) {
    if (chars == "") { 
        // Avoid scanning all of s.
        return -1;

    }
    if (len(s) == 1) {
        var rc = rune(s[0]);
        if (rc >= utf8.RuneSelf) {
            rc = utf8.RuneError;
        }
        if (IndexRune(chars, rc) >= 0) {
            return 0;
        }
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
    if (len(chars) == 1) {
        rc = rune(chars[0]);
        if (rc >= utf8.RuneSelf) {
            rc = utf8.RuneError;
        }
        {
            var i__prev1 = i;

            i = len(s);

            while (i > 0) {
                var (r, size) = utf8.DecodeLastRuneInString(s[..(int)i]);
                i -= size;
                if (rc == r) {
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
            (r, size) = utf8.DecodeLastRuneInString(s[..(int)i]);
            i -= size;
            if (IndexRune(chars, r) >= 0) {
                return i;
            }
        }

        i = i__prev1;
    }
    return -1;

}

// LastIndexByte returns the index of the last instance of c in s, or -1 if c is not present in s.
public static nint LastIndexByte(@string s, byte c) {
    for (var i = len(s) - 1; i >= 0; i--) {
        if (s[i] == c) {
            return i;
        }
    }
    return -1;

}

// Generic split: splits after each instance of sep,
// including sepSave bytes of sep in the subarrays.
private static slice<@string> genSplit(@string s, @string sep, nint sepSave, nint n) {
    if (n == 0) {
        return null;
    }
    if (sep == "") {
        return explode(s, n);
    }
    if (n < 0) {
        n = Count(s, sep) + 1;
    }
    var a = make_slice<@string>(n);
    n--;
    nint i = 0;
    while (i < n) {
        var m = Index(s, sep);
        if (m < 0) {
            break;
        }
        a[i] = s[..(int)m + sepSave];
        s = s[(int)m + len(sep)..];
        i++;

    }
    a[i] = s;
    return a[..(int)i + 1];

}

// SplitN slices s into substrings separated by sep and returns a slice of
// the substrings between those separators.
//
// The count determines the number of substrings to return:
//   n > 0: at most n substrings; the last substring will be the unsplit remainder.
//   n == 0: the result is nil (zero substrings)
//   n < 0: all substrings
//
// Edge cases for s and sep (for example, empty strings) are handled
// as described in the documentation for Split.
public static slice<@string> SplitN(@string s, @string sep, nint n) {
    return genSplit(s, sep, 0, n);
}

// SplitAfterN slices s into substrings after each instance of sep and
// returns a slice of those substrings.
//
// The count determines the number of substrings to return:
//   n > 0: at most n substrings; the last substring will be the unsplit remainder.
//   n == 0: the result is nil (zero substrings)
//   n < 0: all substrings
//
// Edge cases for s and sep (for example, empty strings) are handled
// as described in the documentation for SplitAfter.
public static slice<@string> SplitAfterN(@string s, @string sep, nint n) {
    return genSplit(s, sep, len(sep), n);
}

// Split slices s into all substrings separated by sep and returns a slice of
// the substrings between those separators.
//
// If s does not contain sep and sep is not empty, Split returns a
// slice of length 1 whose only element is s.
//
// If sep is empty, Split splits after each UTF-8 sequence. If both s
// and sep are empty, Split returns an empty slice.
//
// It is equivalent to SplitN with a count of -1.
public static slice<@string> Split(@string s, @string sep) {
    return genSplit(s, sep, 0, -1);
}

// SplitAfter slices s into all substrings after each instance of sep and
// returns a slice of those substrings.
//
// If s does not contain sep and sep is not empty, SplitAfter returns
// a slice of length 1 whose only element is s.
//
// If sep is empty, SplitAfter splits after each UTF-8 sequence. If
// both s and sep are empty, SplitAfter returns an empty slice.
//
// It is equivalent to SplitAfterN with a count of -1.
public static slice<@string> SplitAfter(@string s, @string sep) {
    return genSplit(s, sep, len(sep), -1);
}

private static array<byte> asciiSpace = new array<byte>(InitKeyedValues<byte>(256, ('\t', 1), ('\n', 1), ('\v', 1), ('\f', 1), ('\r', 1), (' ', 1)));

// Fields splits the string s around each instance of one or more consecutive white space
// characters, as defined by unicode.IsSpace, returning a slice of substrings of s or an
// empty slice if s contains only white space.
public static slice<@string> Fields(@string s) { 
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
        // Some runes in the input string are not ASCII.
        return FieldsFunc(s, unicode.IsSpace);

    }
    var a = make_slice<@string>(n);
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
        a[na] = s[(int)fieldStart..(int)i];
        na++;
        i++; 
        // Skip spaces in between fields.
        while (i < len(s) && asciiSpace[s[i]] != 0) {
            i++;
        }
        fieldStart = i;

    }
    if (fieldStart < len(s)) { // Last field might end at EOF.
        a[na] = s[(int)fieldStart..];

    }
    return a;

}

// FieldsFunc splits the string s at each run of Unicode code points c satisfying f(c)
// and returns an array of slices of s. If all code points in s satisfy f(c) or the
// string is empty, an empty slice is returned.
//
// FieldsFunc makes no guarantees about the order in which it calls f(c)
// and assumes that f always returns the same value for a given c.
public static slice<@string> FieldsFunc(@string s, Func<int, bool> f) { 
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
    foreach (var (end, rune) in s) {
        if (f(rune)) {
            if (start >= 0) {
                spans = append(spans, new span(start,end)); 
                // Set start to a negative value.
                // Note: using -1 here consistently and reproducibly
                // slows down this code by a several percent on amd64.
                start = ~start;

            }

        }
        else
 {
            if (start < 0) {
                start = end;
            }
        }
    }    if (start >= 0) {
        spans = append(spans, new span(start,len(s)));
    }
    var a = make_slice<@string>(len(spans));
    foreach (var (i, span) in spans) {
        a[i] = s[(int)span.start..(int)span.end];
    }    return a;

}

// Join concatenates the elements of its first argument to create a single string. The separator
// string sep is placed between elements in the resulting string.
public static @string Join(slice<@string> elems, @string sep) {
    switch (len(elems)) {
        case 0: 
            return "";
            break;
        case 1: 
            return elems[0];
            break;
    }
    var n = len(sep) * (len(elems) - 1);
    for (nint i = 0; i < len(elems); i++) {
        n += len(elems[i]);
    }

    Builder b = default;
    b.Grow(n);
    b.WriteString(elems[0]);
    foreach (var (_, s) in elems[(int)1..]) {
        b.WriteString(sep);
        b.WriteString(s);
    }    return b.String();

}

// HasPrefix tests whether the string s begins with prefix.
public static bool HasPrefix(@string s, @string prefix) {
    return len(s) >= len(prefix) && s[(int)0..(int)len(prefix)] == prefix;
}

// HasSuffix tests whether the string s ends with suffix.
public static bool HasSuffix(@string s, @string suffix) {
    return len(s) >= len(suffix) && s[(int)len(s) - len(suffix)..] == suffix;
}

// Map returns a copy of the string s with all its characters modified
// according to the mapping function. If mapping returns a negative value, the character is
// dropped from the string with no replacement.
public static @string Map(Func<int, int> mapping, @string s) { 
    // In the worst case, the string can grow when mapped, making
    // things unpleasant. But it's so rare we barge in assuming it's
    // fine. It could also shrink but that falls out naturally.

    // The output buffer b is initialized on demand, the first
    // time a character differs.
    Builder b = default;

    {
        var c__prev1 = c;

        foreach (var (__i, __c) in s) {
            i = __i;
            c = __c;
            var r = mapping(c);
            if (r == c && c != utf8.RuneError) {
                continue;
            }
            nint width = default;
            if (c == utf8.RuneError) {
                c, width = utf8.DecodeRuneInString(s[(int)i..]);
                if (width != 1 && r == c) {
                    continue;
                }
            }
            else
 {
                width = utf8.RuneLen(c);
            }

            b.Grow(len(s) + utf8.UTFMax);
            b.WriteString(s[..(int)i]);
            if (r >= 0) {
                b.WriteRune(r);
            }

            s = s[(int)i + width..];
            break;

        }
        c = c__prev1;
    }

    if (b.Cap() == 0) { // didn't call b.Grow above
        return s;

    }
    {
        var c__prev1 = c;

        foreach (var (_, __c) in s) {
            c = __c;
            r = mapping(c);

            if (r >= 0) { 
                // common case
                // Due to inlining, it is more performant to determine if WriteByte should be
                // invoked rather than always call WriteRune
                if (r < utf8.RuneSelf) {
                    b.WriteByte(byte(r));
                }
                else
 { 
                    // r is not a ASCII rune.
                    b.WriteRune(r);

                }

            }

        }
        c = c__prev1;
    }

    return b.String();

}

// Repeat returns a new string consisting of count copies of the string s.
//
// It panics if count is negative or if
// the result of (len(s) * count) overflows.
public static @string Repeat(@string s, nint count) => func((_, panic, _) => {
    if (count == 0) {
        return "";
    }
    if (count < 0) {
        panic("strings: negative Repeat count");
    }
    else if (len(s) * count / count != len(s)) {
        panic("strings: Repeat count causes overflow");
    }
    var n = len(s) * count;
    Builder b = default;
    b.Grow(n);
    b.WriteString(s);
    while (b.Len() < n) {
        if (b.Len() <= n / 2) {
            b.WriteString(b.String());
        }
        else
 {
            b.WriteString(b.String()[..(int)n - b.Len()]);
            break;
        }
    }
    return b.String();

});

// ToUpper returns s with all Unicode letters mapped to their upper case.
public static @string ToUpper(@string s) {
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

    if (isASCII) { // optimize for ASCII-only strings.
        if (!hasLower) {
            return s;
        }
        Builder b = default;
        b.Grow(len(s));
        {
            nint i__prev1 = i;

            for (i = 0; i < len(s); i++) {
                c = s[i];
                if ('a' <= c && c <= 'z') {
                    c -= 'a' - 'A';
                }
                b.WriteByte(c);
            }


            i = i__prev1;
        }
        return b.String();

    }
    return Map(unicode.ToUpper, s);

}

// ToLower returns s with all Unicode letters mapped to their lower case.
public static @string ToLower(@string s) {
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

    if (isASCII) { // optimize for ASCII-only strings.
        if (!hasUpper) {
            return s;
        }
        Builder b = default;
        b.Grow(len(s));
        {
            nint i__prev1 = i;

            for (i = 0; i < len(s); i++) {
                c = s[i];
                if ('A' <= c && c <= 'Z') {
                    c += 'a' - 'A';
                }
                b.WriteByte(c);
            }


            i = i__prev1;
        }
        return b.String();

    }
    return Map(unicode.ToLower, s);

}

// ToTitle returns a copy of the string s with all Unicode letters mapped to
// their Unicode title case.
public static @string ToTitle(@string s) {
    return Map(unicode.ToTitle, s);
}

// ToUpperSpecial returns a copy of the string s with all Unicode letters mapped to their
// upper case using the case mapping specified by c.
public static @string ToUpperSpecial(unicode.SpecialCase c, @string s) {
    return Map(c.ToUpper, s);
}

// ToLowerSpecial returns a copy of the string s with all Unicode letters mapped to their
// lower case using the case mapping specified by c.
public static @string ToLowerSpecial(unicode.SpecialCase c, @string s) {
    return Map(c.ToLower, s);
}

// ToTitleSpecial returns a copy of the string s with all Unicode letters mapped to their
// Unicode title case, giving priority to the special casing rules.
public static @string ToTitleSpecial(unicode.SpecialCase c, @string s) {
    return Map(c.ToTitle, s);
}

// ToValidUTF8 returns a copy of the string s with each run of invalid UTF-8 byte sequences
// replaced by the replacement string, which may be empty.
public static @string ToValidUTF8(@string s, @string replacement) {
    Builder b = default;

    {
        var i__prev1 = i;
        var c__prev1 = c;

        foreach (var (__i, __c) in s) {
            i = __i;
            c = __c;
            if (c != utf8.RuneError) {
                continue;
            }
            var (_, wid) = utf8.DecodeRuneInString(s[(int)i..]);
            if (wid == 1) {
                b.Grow(len(s) + len(replacement));
                b.WriteString(s[..(int)i]);
                s = s[(int)i..];
                break;
            }
        }
        i = i__prev1;
        c = c__prev1;
    }

    if (b.Cap() == 0) { // didn't call b.Grow above
        return s;

    }
    var invalid = false; // previous byte was from an invalid UTF-8 sequence
    {
        var i__prev1 = i;

        nint i = 0;

        while (i < len(s)) {
            var c = s[i];
            if (c < utf8.RuneSelf) {
                i++;
                invalid = false;
                b.WriteByte(c);
                continue;
            }
            (_, wid) = utf8.DecodeRuneInString(s[(int)i..]);
            if (wid == 1) {
                i++;
                if (!invalid) {
                    invalid = true;
                    b.WriteString(replacement);
                }
                continue;
            }
            invalid = false;
            b.WriteString(s[(int)i..(int)i + wid]);
            i += wid;
        }

        i = i__prev1;
    }

    return b.String();

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

// Title returns a copy of the string s with all Unicode letters that begin words
// mapped to their Unicode title case.
//
// BUG(rsc): The rule Title uses for word boundaries does not handle Unicode punctuation properly.
public static @string Title(@string s) { 
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

// TrimLeftFunc returns a slice of the string s with all leading
// Unicode code points c satisfying f(c) removed.
public static @string TrimLeftFunc(@string s, Func<int, bool> f) {
    var i = indexFunc(s, f, false);
    if (i == -1) {
        return "";
    }
    return s[(int)i..];

}

// TrimRightFunc returns a slice of the string s with all trailing
// Unicode code points c satisfying f(c) removed.
public static @string TrimRightFunc(@string s, Func<int, bool> f) {
    var i = lastIndexFunc(s, f, false);
    if (i >= 0 && s[i] >= utf8.RuneSelf) {
        var (_, wid) = utf8.DecodeRuneInString(s[(int)i..]);
        i += wid;
    }
    else
 {
        i++;
    }
    return s[(int)0..(int)i];

}

// TrimFunc returns a slice of the string s with all leading
// and trailing Unicode code points c satisfying f(c) removed.
public static @string TrimFunc(@string s, Func<int, bool> f) {
    return TrimRightFunc(TrimLeftFunc(s, f), f);
}

// IndexFunc returns the index into s of the first Unicode
// code point satisfying f(c), or -1 if none do.
public static nint IndexFunc(@string s, Func<int, bool> f) {
    return indexFunc(s, f, true);
}

// LastIndexFunc returns the index into s of the last
// Unicode code point satisfying f(c), or -1 if none do.
public static nint LastIndexFunc(@string s, Func<int, bool> f) {
    return lastIndexFunc(s, f, true);
}

// indexFunc is the same as IndexFunc except that if
// truth==false, the sense of the predicate function is
// inverted.
private static nint indexFunc(@string s, Func<int, bool> f, bool truth) {
    foreach (var (i, r) in s) {
        if (f(r) == truth) {
            return i;
        }
    }    return -1;

}

// lastIndexFunc is the same as LastIndexFunc except that if
// truth==false, the sense of the predicate function is
// inverted.
private static nint lastIndexFunc(@string s, Func<int, bool> f, bool truth) {
    {
        var i = len(s);

        while (i > 0) {
            var (r, size) = utf8.DecodeLastRuneInString(s[(int)0..(int)i]);
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
        return r => {
            return r == rune(cutset[0]);
        };
    }
    {
        var (as, isASCII) = makeASCIISet(cutset);

        if (isASCII) {
            return r => {
                return r < utf8.RuneSelf && @as.contains(byte(r));
            };
        }
    }

    return r => IndexRune(cutset, r) >= 0;

}

// Trim returns a slice of the string s with all leading and
// trailing Unicode code points contained in cutset removed.
public static @string Trim(@string s, @string cutset) {
    if (s == "" || cutset == "") {
        return s;
    }
    return TrimFunc(s, makeCutsetFunc(cutset));

}

// TrimLeft returns a slice of the string s with all leading
// Unicode code points contained in cutset removed.
//
// To remove a prefix, use TrimPrefix instead.
public static @string TrimLeft(@string s, @string cutset) {
    if (s == "" || cutset == "") {
        return s;
    }
    return TrimLeftFunc(s, makeCutsetFunc(cutset));

}

// TrimRight returns a slice of the string s, with all trailing
// Unicode code points contained in cutset removed.
//
// To remove a suffix, use TrimSuffix instead.
public static @string TrimRight(@string s, @string cutset) {
    if (s == "" || cutset == "") {
        return s;
    }
    return TrimRightFunc(s, makeCutsetFunc(cutset));

}

// TrimSpace returns a slice of the string s, with all leading
// and trailing white space removed, as defined by Unicode.
public static @string TrimSpace(@string s) { 
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
    return s[(int)start..(int)stop];

}

// TrimPrefix returns s without the provided leading prefix string.
// If s doesn't start with prefix, s is returned unchanged.
public static @string TrimPrefix(@string s, @string prefix) {
    if (HasPrefix(s, prefix)) {
        return s[(int)len(prefix)..];
    }
    return s;

}

// TrimSuffix returns s without the provided trailing suffix string.
// If s doesn't end with suffix, s is returned unchanged.
public static @string TrimSuffix(@string s, @string suffix) {
    if (HasSuffix(s, suffix)) {
        return s[..(int)len(s) - len(suffix)];
    }
    return s;

}

// Replace returns a copy of the string s with the first n
// non-overlapping instances of old replaced by new.
// If old is empty, it matches at the beginning of the string
// and after each UTF-8 sequence, yielding up to k+1 replacements
// for a k-rune string.
// If n < 0, there is no limit on the number of replacements.
public static @string Replace(@string s, @string old, @string @new, nint n) {
    if (old == new || n == 0) {
        return s; // avoid allocation
    }
    {
        var m = Count(s, old);

        if (m == 0) {
            return s; // avoid allocation
        }
        else if (n < 0 || m < n) {
            n = m;
        }

    } 

    // Apply replacements to buffer.
    Builder b = default;
    b.Grow(len(s) + n * (len(new) - len(old)));
    nint start = 0;
    for (nint i = 0; i < n; i++) {
        var j = start;
        if (len(old) == 0) {
            if (i > 0) {
                var (_, wid) = utf8.DecodeRuneInString(s[(int)start..]);
                j += wid;
            }
        }
        else
 {
            j += Index(s[(int)start..], old);
        }
        b.WriteString(s[(int)start..(int)j]);
        b.WriteString(new);
        start = j + len(old);

    }
    b.WriteString(s[(int)start..]);
    return b.String();

}

// ReplaceAll returns a copy of the string s with all
// non-overlapping instances of old replaced by new.
// If old is empty, it matches at the beginning of the string
// and after each UTF-8 sequence, yielding up to k+1 replacements
// for a k-rune string.
public static @string ReplaceAll(@string s, @string old, @string @new) {
    return Replace(s, old, new, -1);
}

// EqualFold reports whether s and t, interpreted as UTF-8 strings,
// are equal under Unicode case-folding, which is a more general
// form of case-insensitivity.
public static bool EqualFold(@string s, @string t) {
    while (s != "" && t != "") { 
        // Extract first rune from each string.
        int sr = default;        int tr = default;

        if (s[0] < utf8.RuneSelf) {
            (sr, s) = (rune(s[0]), s[(int)1..]);
        }
        else
 {
            var (r, size) = utf8.DecodeRuneInString(s);
            (sr, s) = (r, s[(int)size..]);
        }
        if (t[0] < utf8.RuneSelf) {
            (tr, t) = (rune(t[0]), t[(int)1..]);
        }
        else
 {
            (r, size) = utf8.DecodeRuneInString(t);
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
    return s == t;

}

// Index returns the index of the first instance of substr in s, or -1 if substr is not present in s.
public static nint Index(@string s, @string substr) {
    var n = len(substr);

    if (n == 0) 
        return 0;
    else if (n == 1) 
        return IndexByte(s, substr[0]);
    else if (n == len(s)) 
        if (substr == s) {
            return 0;
        }
        return -1;
    else if (n > len(s)) 
        return -1;
    else if (n <= bytealg.MaxLen) 
        // Use brute force when s and substr both are small
        if (len(s) <= bytealg.MaxBruteForce) {
            return bytealg.IndexString(s, substr);
        }
        var c0 = substr[0];
        var c1 = substr[1];
        nint i = 0;
        var t = len(s) - n + 1;
        nint fails = 0;
        while (i < t) {
            if (s[i] != c0) { 
                // IndexByte is faster than bytealg.IndexString, so use it as long as
                // we're not getting lots of false positives.
                var o = IndexByte(s[(int)i + 1..(int)t], c0);
                if (o < 0) {
                    return -1;
                }

                i += o + 1;

            }

            if (s[i + 1] == c1 && s[(int)i..(int)i + n] == substr) {
                return i;
            }

            fails++;
            i++; 
            // Switch to bytealg.IndexString when IndexByte produces too many false positives.
            if (fails > bytealg.Cutover(i)) {
                var r = bytealg.IndexString(s[(int)i..], substr);
                if (r >= 0) {
                    return r + i;
                }
                return -1;
            }

        }
        return -1;
        c0 = substr[0];
    c1 = substr[1];
    i = 0;
    t = len(s) - n + 1;
    fails = 0;
    while (i < t) {
        if (s[i] != c0) {
            o = IndexByte(s[(int)i + 1..(int)t], c0);
            if (o < 0) {
                return -1;
            }
            i += o + 1;
        }
        if (s[i + 1] == c1 && s[(int)i..(int)i + n] == substr) {
            return i;
        }
        i++;
        fails++;
        if (fails >= 4 + i >> 4 && i < t) { 
            // See comment in ../bytes/bytes.go.
            var j = bytealg.IndexRabinKarp(s[(int)i..], substr);
            if (j < 0) {
                return -1;
            }

            return i + j;

        }
    }
    return -1;

}

} // end strings_package
