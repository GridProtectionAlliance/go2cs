// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package strings implements simple functions to manipulate UTF-8 encoded strings.
//
// For information about UTF-8 strings in Go, see https://blog.golang.org/strings.
namespace go;

using bytealg = @internal.bytealg_package;
using stringslite = @internal.stringslite_package;
using unicode = unicode_package;
using utf8 = unicode.utf8_package;
using @internal;
using unicode;

partial class strings_package {

internal const nint maxInt = /* int(^uint(0) >> 1) */ 9223372036854775807;

// explode splits s into a slice of UTF-8 strings,
// one string per Unicode character up to a maximum of n (n < 0 means no limit).
// Invalid UTF-8 bytes are sliced individually.
internal static slice<@string> explode(@string s, nint n) {
    nint l = utf8.RuneCountInString(s);
    if (n < 0 || n > l) {
        n = l;
    }
    var a = new slice<@string>(n);
    for (nint i = 0; i < n - 1; i++) {
        var (_, size) = utf8.DecodeRuneInString(s);
        a[i] = s[..(int)(size)];
        s = s[(int)(size)..];
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
    while (ᐧ) {
        nint i = Index(s, substr);
        if (i == -1) {
            return n;
        }
        n++;
        s = s[(int)(i + len(substr))..];
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
public static bool ContainsRune(@string s, rune r) {
    return IndexRune(s, r) >= 0;
}

// ContainsFunc reports whether any Unicode code points r within s satisfy f(r).
public static bool ContainsFunc(@string s, Func<rune, bool> f) {
    return IndexFunc(s, f) >= 0;
}

// LastIndex returns the index of the last instance of substr in s, or -1 if substr is not present in s.
public static nint LastIndex(@string s, @string substr) {
    nint n = len(substr);
    switch (ᐧ) {
    case {} when n is 0: {
        return len(s);
    }
    case {} when n is 1: {
        return bytealg.LastIndexByteString(s, substr[0]);
    }
    case {} when n == len(s): {
        if (substr == s) {
            return 0;
        }
        return -1;
    }
    case {} when n > len(s): {
        return -1;
    }}

    // Rabin-Karp search from the end of the string
    var (hashss, pow) = bytealg.HashStrRev(substr);
    nint last = len(s) - n;
    uint32 h = default!;
    for (nint i = len(s) - 1; i >= last; i--) {
        h = h * bytealg.PrimeRK + ((uint32)s[i]);
    }
    if (h == hashss && s[(int)(last)..] == substr) {
        return last;
    }
    for (nint i = last - 1; i >= 0; i--) {
        h *= bytealg.PrimeRK;
        h += ((uint32)s[i]);
        h -= pow * ((uint32)s[i + n]);
        if (h == hashss && s[(int)(i)..(int)(i + n)] == substr) {
            return i;
        }
    }
    return -1;
}

// IndexByte returns the index of the first instance of c in s, or -1 if c is not present in s.
public static nint IndexByte(@string s, byte c) {
    return stringslite.IndexByte(s, c);
}

// IndexRune returns the index of the first instance of the Unicode code point
// r, or -1 if rune is not present in s.
// If r is [utf8.RuneError], it returns the first instance of any
// invalid UTF-8 byte sequence.
public static nint IndexRune(@string s, rune r) {
    switch (ᐧ) {
    case {} when 0 <= r && r < utf8.RuneSelf: {
        return IndexByte(s, ((byte)r));
    }
    case {} when r is utf8.RuneError: {
        foreach (var (i, rΔ2) in s) {
            if (rΔ2 == utf8.RuneError) {
                return i;
            }
        }
        return -1;
    }
    case {} when !utf8.ValidRune(r): {
        return -1;
    }
    default: {
        return Index(s, ((@string)r));
    }}

}

// IndexAny returns the index of the first instance of any Unicode code point
// from chars in s, or -1 if no Unicode code point from chars is present in s.
public static nint IndexAny(@string s, @string chars) {
    if (chars == ""u8) {
        // Avoid scanning all of s.
        return -1;
    }
    if (len(chars) == 1) {
        // Avoid scanning all of s.
        var r = ((rune)chars[0]);
        if (r >= utf8.RuneSelf) {
            r = utf8.RuneError;
        }
        return IndexRune(s, r);
    }
    if (len(s) > 8) {
        {
            var (@as, isASCII) = makeASCIISet(chars); if (isASCII) {
                for (nint i = 0; i < len(s); i++) {
                    if (@as.contains(s[i])) {
                        return i;
                    }
                }
                return -1;
            }
        }
    }
    foreach (var (i, c) in s) {
        if (IndexRune(chars, c) >= 0) {
            return i;
        }
    }
    return -1;
}

// LastIndexAny returns the index of the last instance of any Unicode code
// point from chars in s, or -1 if no Unicode code point from chars is
// present in s.
public static nint LastIndexAny(@string s, @string chars) {
    if (chars == ""u8) {
        // Avoid scanning all of s.
        return -1;
    }
    if (len(s) == 1) {
        var rc = ((rune)s[0]);
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
            var (@as, isASCII) = makeASCIISet(chars); if (isASCII) {
                for (nint iΔ1 = len(s) - 1; iΔ1 >= 0; iΔ1--) {
                    if (@as.contains(s[iΔ1])) {
                        return iΔ1;
                    }
                }
                return -1;
            }
        }
    }
    if (len(chars) == 1) {
        var rc = ((rune)chars[0]);
        if (rc >= utf8.RuneSelf) {
            rc = utf8.RuneError;
        }
        for (nint iΔ2 = len(s); iΔ2 > 0; ) {
            var (r, size) = utf8.DecodeLastRuneInString(s[..(int)(iΔ2)]);
             -= size;
            if (rc == r) {
                return iΔ2;
            }
        }
        return -1;
    }
    for (nint i = len(s); i > 0; ) {
        var (r, size) = utf8.DecodeLastRuneInString(s[..(int)(i)]);
        i -= size;
        if (IndexRune(chars, r) >= 0) {
            return i;
        }
    }
    return -1;
}

// LastIndexByte returns the index of the last instance of c in s, or -1 if c is not present in s.
public static nint LastIndexByte(@string s, byte c) {
    return bytealg.LastIndexByteString(s, c);
}

// Generic split: splits after each instance of sep,
// including sepSave bytes of sep in the subarrays.
internal static slice<@string> genSplit(@string s, @string sep, nint sepSave, nint n) {
    if (n == 0) {
        return default!;
    }
    if (sep == ""u8) {
        return explode(s, n);
    }
    if (n < 0) {
        n = Count(s, sep) + 1;
    }
    if (n > len(s) + 1) {
        n = len(s) + 1;
    }
    var a = new slice<@string>(n);
    n--;
    nint i = 0;
    while (i < n) {
        nint m = Index(s, sep);
        if (m < 0) {
            break;
        }
        a[i] = s[..(int)(m + sepSave)];
        s = s[(int)(m + len(sep))..];
        i++;
    }
    a[i] = s;
    return a[..(int)(i + 1)];
}

// SplitN slices s into substrings separated by sep and returns a slice of
// the substrings between those separators.
//
// The count determines the number of substrings to return:
//   - n > 0: at most n substrings; the last substring will be the unsplit remainder;
//   - n == 0: the result is nil (zero substrings);
//   - n < 0: all substrings.
//
// Edge cases for s and sep (for example, empty strings) are handled
// as described in the documentation for [Split].
//
// To split around the first instance of a separator, see [Cut].
public static slice<@string> SplitN(@string s, @string sep, nint n) {
    return genSplit(s, sep, 0, n);
}

// SplitAfterN slices s into substrings after each instance of sep and
// returns a slice of those substrings.
//
// The count determines the number of substrings to return:
//   - n > 0: at most n substrings; the last substring will be the unsplit remainder;
//   - n == 0: the result is nil (zero substrings);
//   - n < 0: all substrings.
//
// Edge cases for s and sep (for example, empty strings) are handled
// as described in the documentation for [SplitAfter].
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
// It is equivalent to [SplitN] with a count of -1.
//
// To split around the first instance of a separator, see [Cut].
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
// It is equivalent to [SplitAfterN] with a count of -1.
public static slice<@string> SplitAfter(@string s, @string sep) {
    return genSplit(s, sep, len(sep), -1);
}

internal static array<uint8> asciiSpace = new array<uint8>(256){[(rune)'\'] = 1, [(rune)'\'] = 1, [(rune)'\'] = 1, [(rune)'\'] = 1, [(rune)'\'] = 1, [(rune)' '] = 1};

// Fields splits the string s around each instance of one or more consecutive white space
// characters, as defined by [unicode.IsSpace], returning a slice of substrings of s or an
// empty slice if s contains only white space.
public static slice<@string> Fields(@string s) {
    // First count the fields.
    // This is an exact count if s is ASCII, otherwise it is an approximation.
    nint n = 0;
    nint wasSpace = 1;
    // setBits is used to track which bits are set in the bytes of s.
    var setBits = ((uint8)0);
    for (nint i = 0; i < len(s); i++) {
        var r = s[i];
        setBits |= (byte)(r);
        nint isSpace = ((nint)asciiSpace[r]);
        n += (nint)(wasSpace & ~isSpace);
        wasSpace = isSpace;
    }
    if (setBits >= utf8.RuneSelf) {
        // Some runes in the input string are not ASCII.
        return FieldsFunc(s, unicode.IsSpace);
    }
    // ASCII fast path
    var a = new slice<@string>(n);
    nint na = 0;
    nint fieldStart = 0;
    nint i = 0;
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
        a[na] = s[(int)(fieldStart)..(int)(i)];
        na++;
        i++;
        // Skip spaces in between fields.
        while (i < len(s) && asciiSpace[s[i]] != 0) {
            i++;
        }
        fieldStart = i;
    }
    if (fieldStart < len(s)) {
        // Last field might end at EOF.
        a[na] = s[(int)(fieldStart)..];
    }
    return a;
}

// A span is used to record a slice of s of the form s[start:end].
// The start index is inclusive and the end index is exclusive.
[GoType("dyn")] partial struct FieldsFunc_span {
    internal nint start;
    internal nint end;
}

// FieldsFunc splits the string s at each run of Unicode code points c satisfying f(c)
// and returns an array of slices of s. If all code points in s satisfy f(c) or the
// string is empty, an empty slice is returned.
//
// FieldsFunc makes no guarantees about the order in which it calls f(c)
// and assumes that f always returns the same value for a given c.
public static slice<@string> FieldsFunc(@string s, Func<rune, bool> f) {
    var spans = new slice<span>(0, 32);
    // Find the field start and end indices.
    // Doing this in a separate pass (rather than slicing the string s
    // and collecting the result substrings right away) is significantly
    // more efficient, possibly due to cache effects.
    nint start = -1;
    // valid span start if >= 0
    foreach (var (end, rune) in s) {
        if (f(rune)){
            if (start >= 0) {
                spans = append(spans, new span(start, end));
                // Set start to a negative value.
                // Note: using -1 here consistently and reproducibly
                // slows down this code by a several percent on amd64.
                start = ~start;
            }
        } else {
            if (start < 0) {
                start = end;
            }
        }
    }
    // Last field might end at EOF.
    if (start >= 0) {
        spans = append(spans, new span(start, len(s)));
    }
    // Create strings from recorded field indices.
    var a = new slice<@string>(len(spans));
    foreach (var (i, span) in spans) {
        a[i] = s[(int)(span.start)..(int)(span.end)];
    }
    return a;
}

// Join concatenates the elements of its first argument to create a single string. The separator
// string sep is placed between elements in the resulting string.
public static @string Join(slice<@string> elems, @string sep) {
    switch (len(elems)) {
    case 0: {
        return ""u8;
    }
    case 1: {
        return elems[0];
    }}

    nint n = default!;
    if (len(sep) > 0) {
        if (len(sep) >= maxInt / (len(elems) - 1)) {
            throw panic("strings: Join output length overflow");
        }
        n += len(sep) * (len(elems) - 1);
    }
    foreach (var (_, elem) in elems) {
        if (len(elem) > maxInt - n) {
            throw panic("strings: Join output length overflow");
        }
        n += len(elem);
    }
    Builder b = default!;
    b.Grow(n);
    b.WriteString(elems[0]);
    foreach (var (_, s) in elems[1..]) {
        b.WriteString(sep);
        b.WriteString(s);
    }
    return b.String();
}

// HasPrefix reports whether the string s begins with prefix.
public static bool HasPrefix(@string s, @string prefix) {
    return stringslite.HasPrefix(s, prefix);
}

// HasSuffix reports whether the string s ends with suffix.
public static bool HasSuffix(@string s, @string suffix) {
    return stringslite.HasSuffix(s, suffix);
}

// Map returns a copy of the string s with all its characters modified
// according to the mapping function. If mapping returns a negative value, the character is
// dropped from the string with no replacement.
public static @string Map(Func<rune, rune> mapping, @string s) {
    // In the worst case, the string can grow when mapped, making
    // things unpleasant. But it's so rare we barge in assuming it's
    // fine. It could also shrink but that falls out naturally.
    // The output buffer b is initialized on demand, the first
    // time a character differs.
    Builder b = default!;
    foreach (var (i, c) in s) {
        var r = mapping(c);
        if (r == c && c != utf8.RuneError) {
            continue;
        }
        nint width = default!;
        if (c == utf8.RuneError){
            (c, width) = utf8.DecodeRuneInString(s[(int)(i)..]);
            if (width != 1 && r == c) {
                continue;
            }
        } else {
            width = utf8.RuneLen(c);
        }
        b.Grow(len(s) + utf8.UTFMax);
        b.WriteString(s[..(int)(i)]);
        if (r >= 0) {
            b.WriteRune(r);
        }
        s = s[(int)(i + width)..];
        break;
    }
    // Fast path for unchanged input
    if (b.Cap() == 0) {
        // didn't call b.Grow above
        return s;
    }
    foreach (var (_, c) in s) {
        var r = mapping(c);
        if (r >= 0) {
            // common case
            // Due to inlining, it is more performant to determine if WriteByte should be
            // invoked rather than always call WriteRune
            if (r < utf8.RuneSelf){
                b.WriteByte(((byte)r));
            } else {
                // r is not an ASCII rune.
                b.WriteRune(r);
            }
        }
    }
    return b.String();
}

// According to static analysis, spaces, dashes, zeros, equals, and tabs
// are the most commonly repeated string literal,
// often used for display on fixed-width terminal windows.
// Pre-declare constants for these for O(1) repetition in the common-case.
internal static readonly @string repeatedSpaces = "                                                                                                                                ";

internal static readonly @string repeatedDashes = "--------------------------------------------------------------------------------------------------------------------------------";

internal static readonly @string repeatedZeroes = "0000000000000000000000000000000000000000000000000000000000000000";

internal static readonly @string repeatedEquals = "================================================================================================================================";

internal static readonly @string repeatedTabs = "\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t";

// Repeat returns a new string consisting of count copies of the string s.
//
// It panics if count is negative or if the result of (len(s) * count)
// overflows.
public static @string Repeat(@string s, nint count) {
    switch (count) {
    case 0: {
        return ""u8;
    }
    case 1: {
        return s;
    }}

    // Since we cannot return an error on overflow,
    // we should panic if the repeat will generate an overflow.
    // See golang.org/issue/16237.
    if (count < 0) {
        throw panic("strings: negative Repeat count");
    }
    if (len(s) > maxInt / count) {
        throw panic("strings: Repeat output length overflow");
    }
    nint n = len(s) * count;
    if (len(s) == 0) {
        return ""u8;
    }
    // Optimize for commonly repeated strings of relatively short length.
    switch (s[0]) {
    case (rune)' ' or (rune)'-' or (rune)'0' or (rune)'=' or (rune)'\t': {
        switch (ᐧ) {
        case {} when n <= len(repeatedSpaces) && HasPrefix(repeatedSpaces, s): {
            return repeatedSpaces[..(int)(n)];
        }
        case {} when n <= len(repeatedDashes) && HasPrefix(repeatedDashes, s): {
            return repeatedDashes[..(int)(n)];
        }
        case {} when n <= len(repeatedZeroes) && HasPrefix(repeatedZeroes, s): {
            return repeatedZeroes[..(int)(n)];
        }
        case {} when n <= len(repeatedEquals) && HasPrefix(repeatedEquals, s): {
            return repeatedEquals[..(int)(n)];
        }
        case {} when n <= len(repeatedTabs) && HasPrefix(repeatedTabs, s): {
            return repeatedTabs[..(int)(n)];
        }}

        break;
    }}

    // Past a certain chunk size it is counterproductive to use
    // larger chunks as the source of the write, as when the source
    // is too large we are basically just thrashing the CPU D-cache.
    // So if the result length is larger than an empirically-found
    // limit (8KB), we stop growing the source string once the limit
    // is reached and keep reusing the same source string - that
    // should therefore be always resident in the L1 cache - until we
    // have completed the construction of the result.
    // This yields significant speedups (up to +100%) in cases where
    // the result length is large (roughly, over L2 cache size).
    static readonly UntypedInt chunkLimit = /* 8 * 1024 */ 8192;
    nint chunkMax = n;
    if (n > chunkLimit) {
        chunkMax = chunkLimit / len(s) * len(s);
        if (chunkMax == 0) {
            chunkMax = len(s);
        }
    }
    Builder b = default!;
    b.Grow(n);
    b.WriteString(s);
    while (b.Len() < n) {
        nint chunk = n - b.Len();
        if (chunk > b.Len()) {
            chunk = b.Len();
        }
        if (chunk > chunkMax) {
            chunk = chunkMax;
        }
        b.WriteString(b.String()[..(int)(chunk)]);
    }
    return b.String();
}

// ToUpper returns s with all Unicode letters mapped to their upper case.
public static @string ToUpper(@string s) {
    var (isASCII, hasLower) = (true, false);
    for (nint i = 0; i < len(s); i++) {
        var c = s[i];
        if (c >= utf8.RuneSelf) {
            isASCII = false;
            break;
        }
        hasLower = hasLower || ((rune)'a' <= c && c <= (rune)'z');
    }
    if (isASCII) {
        // optimize for ASCII-only strings.
        if (!hasLower) {
            return s;
        }
        Builder b = default!;
        nint pos = default!;
        b.Grow(len(s));
        for (nint i = 0; i < len(s); i++) {
            var c = s[i];
            if ((rune)'a' <= c && c <= (rune)'z') {
                c -= (rune)'a' - (rune)'A';
                if (pos < i) {
                    b.WriteString(s[(int)(pos)..(int)(i)]);
                }
                b.WriteByte(c);
                pos = i + 1;
            }
        }
        if (pos < len(s)) {
            b.WriteString(s[(int)(pos)..]);
        }
        return b.String();
    }
    return Map(unicode.ToUpper, s);
}

// ToLower returns s with all Unicode letters mapped to their lower case.
public static @string ToLower(@string s) {
    var (isASCII, hasUpper) = (true, false);
    for (nint i = 0; i < len(s); i++) {
        var c = s[i];
        if (c >= utf8.RuneSelf) {
            isASCII = false;
            break;
        }
        hasUpper = hasUpper || ((rune)'A' <= c && c <= (rune)'Z');
    }
    if (isASCII) {
        // optimize for ASCII-only strings.
        if (!hasUpper) {
            return s;
        }
        Builder b = default!;
        nint pos = default!;
        b.Grow(len(s));
        for (nint i = 0; i < len(s); i++) {
            var c = s[i];
            if ((rune)'A' <= c && c <= (rune)'Z') {
                c += (rune)'a' - (rune)'A';
                if (pos < i) {
                    b.WriteString(s[(int)(pos)..(int)(i)]);
                }
                b.WriteByte(c);
                pos = i + 1;
            }
        }
        if (pos < len(s)) {
            b.WriteString(s[(int)(pos)..]);
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
    Builder b = default!;
    foreach (var (iΔ1, c) in s) {
        if (c != utf8.RuneError) {
            continue;
        }
        var (_, wid) = utf8.DecodeRuneInString(s[(int)(iΔ1)..]);
        if (wid == 1) {
            b.Grow(len(s) + len(replacement));
            b.WriteString(s[..(int)(iΔ1)]);
            s = s[(int)(iΔ1)..];
            break;
        }
    }
    // Fast path for unchanged input
    if (b.Cap() == 0) {
        // didn't call b.Grow above
        return s;
    }
    var invalid = false;
    // previous byte was from an invalid UTF-8 sequence
    for (nint i = 0; i < len(s); ) {
        var c = s[i];
        if (c < utf8.RuneSelf) {
            i++;
            invalid = false;
            b.WriteByte(c);
            continue;
        }
        var (_, wid) = utf8.DecodeRuneInString(s[(int)(i)..]);
        if (wid == 1) {
            i++;
            if (!invalid) {
                invalid = true;
                b.WriteString(replacement);
            }
            continue;
        }
        invalid = false;
        b.WriteString(s[(int)(i)..(int)(i + wid)]);
        i += wid;
    }
    return b.String();
}

// isSeparator reports whether the rune could mark a word boundary.
// TODO: update when package unicode captures more of the properties.
internal static bool isSeparator(rune r) {
    // ASCII alphanumerics and underscore are not separators
    if (r <= 127) {
        switch (ᐧ) {
        case {} when (rune)'0' <= r && r <= (rune)'9': {
            return false;
        }
        case {} when (rune)'a' <= r && r <= (rune)'z': {
            return false;
        }
        case {} when (rune)'A' <= r && r <= (rune)'Z': {
            return false;
        }
        case {} when r is (rune)'_': {
            return false;
        }}

        return true;
    }
    // Letters and digits are not separators
    if (unicode.IsLetter(r) || unicode.IsDigit(r)) {
        return false;
    }
    // Otherwise, all we can do for now is treat spaces as separators.
    return unicode.IsSpace(r);
}

// Title returns a copy of the string s with all Unicode letters that begin words
// mapped to their Unicode title case.
//
// Deprecated: The rule Title uses for word boundaries does not handle Unicode
// punctuation properly. Use golang.org/x/text/cases instead.
public static @string Title(@string s) {
    // Use a closure here to remember state.
    // Hackish but effective. Depends on Map scanning in order and calling
    // the closure once per rune.
    var prev = (rune)' ';
    return Map(
        (rune r) => {
            if (isSeparator(prev)) {
                prev = r;
                return unicode.ToTitle(r);
            }
            prev = r;
            return r;
        },
        s);
}

// TrimLeftFunc returns a slice of the string s with all leading
// Unicode code points c satisfying f(c) removed.
public static @string TrimLeftFunc(@string s, Func<rune, bool> f) {
    nint i = indexFunc(s, f, false);
    if (i == -1) {
        return ""u8;
    }
    return s[(int)(i)..];
}

// TrimRightFunc returns a slice of the string s with all trailing
// Unicode code points c satisfying f(c) removed.
public static @string TrimRightFunc(@string s, Func<rune, bool> f) {
    nint i = lastIndexFunc(s, f, false);
    if (i >= 0 && s[i] >= utf8.RuneSelf){
        var (_, wid) = utf8.DecodeRuneInString(s[(int)(i)..]);
        i += wid;
    } else {
        i++;
    }
    return s[0..(int)(i)];
}

// TrimFunc returns a slice of the string s with all leading
// and trailing Unicode code points c satisfying f(c) removed.
public static @string TrimFunc(@string s, Func<rune, bool> f) {
    return TrimRightFunc(TrimLeftFunc(s, f), f);
}

// IndexFunc returns the index into s of the first Unicode
// code point satisfying f(c), or -1 if none do.
public static nint IndexFunc(@string s, Func<rune, bool> f) {
    return indexFunc(s, f, true);
}

// LastIndexFunc returns the index into s of the last
// Unicode code point satisfying f(c), or -1 if none do.
public static nint LastIndexFunc(@string s, Func<rune, bool> f) {
    return lastIndexFunc(s, f, true);
}

// indexFunc is the same as IndexFunc except that if
// truth==false, the sense of the predicate function is
// inverted.
internal static nint indexFunc(@string s, Func<rune, bool> f, bool truth) {
    foreach (var (i, r) in s) {
        if (f(r) == truth) {
            return i;
        }
    }
    return -1;
}

// lastIndexFunc is the same as LastIndexFunc except that if
// truth==false, the sense of the predicate function is
// inverted.
internal static nint lastIndexFunc(@string s, Func<rune, bool> f, bool truth) {
    for (nint i = len(s); i > 0; ) {
        var (r, size) = utf8.DecodeLastRuneInString(s[0..(int)(i)]);
        i -= size;
        if (f(r) == truth) {
            return i;
        }
    }
    return -1;
}

[GoType("[8]uint32")] partial struct asciiSet;

// makeASCIISet creates a set of ASCII characters and reports whether all
// characters in chars are ASCII.
internal static (asciiSet @as, bool ok) makeASCIISet(@string chars) {
    asciiSet @as = default!;
    bool ok = default!;

    for (nint i = 0; i < len(chars); i++) {
        var c = chars[i];
        if (c >= utf8.RuneSelf) {
            return (@as, false);
        }
        @as[c / 32] |= (uint32)(1 << (int)((c % 32)));
    }
    return (@as, true);
}

// contains reports whether c is inside the set.
[GoRecv] internal static bool contains(this ref asciiSet @as, byte c) {
    return ((uint32)(@as.val[c / 32] & (1 << (int)((c % 32))))) != 0;
}

// Trim returns a slice of the string s with all leading and
// trailing Unicode code points contained in cutset removed.
public static @string Trim(@string s, @string cutset) {
    if (s == ""u8 || cutset == ""u8) {
        return s;
    }
    if (len(cutset) == 1 && cutset[0] < utf8.RuneSelf) {
        return trimLeftByte(trimRightByte(s, cutset[0]), cutset[0]);
    }
    {
        var (@as, ok) = makeASCIISet(cutset); if (ok) {
            return trimLeftASCII(trimRightASCII(s, Ꮡ@as), Ꮡ@as);
        }
    }
    return trimLeftUnicode(trimRightUnicode(s, cutset), cutset);
}

// TrimLeft returns a slice of the string s with all leading
// Unicode code points contained in cutset removed.
//
// To remove a prefix, use [TrimPrefix] instead.
public static @string TrimLeft(@string s, @string cutset) {
    if (s == ""u8 || cutset == ""u8) {
        return s;
    }
    if (len(cutset) == 1 && cutset[0] < utf8.RuneSelf) {
        return trimLeftByte(s, cutset[0]);
    }
    {
        var (@as, ok) = makeASCIISet(cutset); if (ok) {
            return trimLeftASCII(s, Ꮡ@as);
        }
    }
    return trimLeftUnicode(s, cutset);
}

internal static @string trimLeftByte(@string s, byte c) {
    while (len(s) > 0 && s[0] == c) {
        s = s[1..];
    }
    return s;
}

internal static @string trimLeftASCII(@string s, ж<asciiSet> Ꮡas) {
    ref var @as = ref Ꮡas.val;

    while (len(s) > 0) {
        if (!@as.contains(s[0])) {
            break;
        }
        s = s[1..];
    }
    return s;
}

internal static @string trimLeftUnicode(@string s, @string cutset) {
    while (len(s) > 0) {
        var r = ((rune)s[0]);
        nint n = 1;
        if (r >= utf8.RuneSelf) {
            (r, n) = utf8.DecodeRuneInString(s);
        }
        if (!ContainsRune(cutset, r)) {
            break;
        }
        s = s[(int)(n)..];
    }
    return s;
}

// TrimRight returns a slice of the string s, with all trailing
// Unicode code points contained in cutset removed.
//
// To remove a suffix, use [TrimSuffix] instead.
public static @string TrimRight(@string s, @string cutset) {
    if (s == ""u8 || cutset == ""u8) {
        return s;
    }
    if (len(cutset) == 1 && cutset[0] < utf8.RuneSelf) {
        return trimRightByte(s, cutset[0]);
    }
    {
        var (@as, ok) = makeASCIISet(cutset); if (ok) {
            return trimRightASCII(s, Ꮡ@as);
        }
    }
    return trimRightUnicode(s, cutset);
}

internal static @string trimRightByte(@string s, byte c) {
    while (len(s) > 0 && s[len(s) - 1] == c) {
        s = s[..(int)(len(s) - 1)];
    }
    return s;
}

internal static @string trimRightASCII(@string s, ж<asciiSet> Ꮡas) {
    ref var @as = ref Ꮡas.val;

    while (len(s) > 0) {
        if (!@as.contains(s[len(s) - 1])) {
            break;
        }
        s = s[..(int)(len(s) - 1)];
    }
    return s;
}

internal static @string trimRightUnicode(@string s, @string cutset) {
    while (len(s) > 0) {
        var r = ((rune)s[len(s) - 1]);
        nint n = 1;
        if (r >= utf8.RuneSelf) {
            (r, n) = utf8.DecodeLastRuneInString(s);
        }
        if (!ContainsRune(cutset, r)) {
            break;
        }
        s = s[..(int)(len(s) - n)];
    }
    return s;
}

// TrimSpace returns a slice of the string s, with all leading
// and trailing white space removed, as defined by Unicode.
public static @string TrimSpace(@string s) {
    // Fast path for ASCII: look for the first ASCII non-space byte
    nint start = 0;
    for (; start < len(s); start++) {
        var c = s[start];
        if (c >= utf8.RuneSelf) {
            // If we run into a non-ASCII byte, fall back to the
            // slower unicode-aware method on the remaining bytes
            return TrimFunc(s[(int)(start)..], unicode.IsSpace);
        }
        if (asciiSpace[c] == 0) {
            break;
        }
    }
    // Now look for the first ASCII non-space byte from the end
    nint stop = len(s);
    for (; stop > start; stop--) {
        var c = s[stop - 1];
        if (c >= utf8.RuneSelf) {
            // start has been already trimmed above, should trim end only
            return TrimRightFunc(s[(int)(start)..(int)(stop)], unicode.IsSpace);
        }
        if (asciiSpace[c] == 0) {
            break;
        }
    }
    // At this point s[start:stop] starts and ends with an ASCII
    // non-space bytes, so we're done. Non-ASCII cases have already
    // been handled above.
    return s[(int)(start)..(int)(stop)];
}

// TrimPrefix returns s without the provided leading prefix string.
// If s doesn't start with prefix, s is returned unchanged.
public static @string TrimPrefix(@string s, @string prefix) {
    return stringslite.TrimPrefix(s, prefix);
}

// TrimSuffix returns s without the provided trailing suffix string.
// If s doesn't end with suffix, s is returned unchanged.
public static @string TrimSuffix(@string s, @string suffix) {
    return stringslite.TrimSuffix(s, suffix);
}

// Replace returns a copy of the string s with the first n
// non-overlapping instances of old replaced by new.
// If old is empty, it matches at the beginning of the string
// and after each UTF-8 sequence, yielding up to k+1 replacements
// for a k-rune string.
// If n < 0, there is no limit on the number of replacements.
public static @string Replace(@string s, @string old, @string @new, nint n) {
    if (old == @new || n == 0) {
        return s;
    }
    // avoid allocation
    // Compute number of replacements.
    {
        nint m = Count(s, old); if (m == 0){
            return s;
        } else 
        if (n < 0 || m < n) {
            // avoid allocation
            n = m;
        }
    }
    // Apply replacements to buffer.
    Builder b = default!;
    b.Grow(len(s) + n * (len(@new) - len(old)));
    nint start = 0;
    for (nint i = 0; i < n; i++) {
        nint j = start;
        if (len(old) == 0){
            if (i > 0) {
                var (_, wid) = utf8.DecodeRuneInString(s[(int)(start)..]);
                j += wid;
            }
        } else {
            j += Index(s[(int)(start)..], old);
        }
        b.WriteString(s[(int)(start)..(int)(j)]);
        b.WriteString(@new);
        start = j + len(old);
    }
    b.WriteString(s[(int)(start)..]);
    return b.String();
}

// ReplaceAll returns a copy of the string s with all
// non-overlapping instances of old replaced by new.
// If old is empty, it matches at the beginning of the string
// and after each UTF-8 sequence, yielding up to k+1 replacements
// for a k-rune string.
public static @string ReplaceAll(@string s, @string old, @string @new) {
    return Replace(s, old, @new, -1);
}

// EqualFold reports whether s and t, interpreted as UTF-8 strings,
// are equal under simple Unicode case-folding, which is a more general
// form of case-insensitivity.
public static bool EqualFold(@string s, @string t) {
    // ASCII fast path
    nint i = 0;
    for (; i < len(s) && i < len(t); i++) {
        var sr = s[i];
        var trΔ1 = t[i];
        if ((byte)(sr | trΔ1) >= utf8.RuneSelf) {
            goto hasUnicode;
        }
        // Easy case.
        if (trΔ1 == sr) {
            continue;
        }
        // Make sr < tr to simplify what follows.
        if (trΔ1 < sr) {
            (, sr) = (sr, trΔ1);
        }
        // ASCII only, sr/tr must be upper/lower case
        if ((rune)'A' <= sr && sr <= (rune)'Z' && trΔ1 == sr + (rune)'a' - (rune)'A') {
            continue;
        }
        return false;
    }
    // Check if we've exhausted both strings.
    return len(s) == len(t);
hasUnicode:
    s = s[(int)(i)..];
    t = t[(int)(i)..];
    foreach (var (_, sr) in s) {
        // If t is exhausted the strings are not equal.
        if (len(t) == 0) {
            return false;
        }
        // Extract first rune from second string.
        rune tr = default!;
        if (t[0] < utf8.RuneSelf){
            (tr, t) = (((rune)t[0]), t[1..]);
        } else {
            var (r, size) = utf8.DecodeRuneInString(t);
            (tr, t) = (r, t[(int)(size)..]);
        }
        // If they match, keep going; if not, return false.
        // Easy case.
        if (tr == sr) {
            continue;
        }
        // Make sr < tr to simplify what follows.
        if (tr < sr) {
            (tr, sr) = (sr, tr);
        }
        // Fast check for ASCII.
        if (tr < utf8.RuneSelf) {
            // ASCII only, sr/tr must be upper/lower case
            if ((rune)'A' <= sr && sr <= (rune)'Z' && tr == sr + (rune)'a' - (rune)'A') {
                continue;
            }
            return false;
        }
        // General case. SimpleFold(x) returns the next equivalent rune > x
        // or wraps around to smaller values.
        var r = unicode.SimpleFold(sr);
        while (r != sr && r < tr) {
            r = unicode.SimpleFold(r);
        }
        if (r == tr) {
            continue;
        }
        return false;
    }
    // First string is empty, so check if the second one is also empty.
    return len(t) == 0;
}

// Index returns the index of the first instance of substr in s, or -1 if substr is not present in s.
public static nint Index(@string s, @string substr) {
    return stringslite.Index(s, substr);
}

// Cut slices s around the first instance of sep,
// returning the text before and after sep.
// The found result reports whether sep appears in s.
// If sep does not appear in s, cut returns s, "", false.
public static (@string before, @string after, bool found) Cut(@string s, @string sep) {
    @string before = default!;
    @string after = default!;
    bool found = default!;

    return stringslite.Cut(s, sep);
}

// CutPrefix returns s without the provided leading prefix string
// and reports whether it found the prefix.
// If s doesn't start with prefix, CutPrefix returns s, false.
// If prefix is the empty string, CutPrefix returns s, true.
public static (@string after, bool found) CutPrefix(@string s, @string prefix) {
    @string after = default!;
    bool found = default!;

    return stringslite.CutPrefix(s, prefix);
}

// CutSuffix returns s without the provided ending suffix string
// and reports whether it found the suffix.
// If s doesn't end with suffix, CutSuffix returns s, false.
// If suffix is the empty string, CutSuffix returns s, true.
public static (@string before, bool found) CutSuffix(@string s, @string suffix) {
    @string before = default!;
    bool found = default!;

    return stringslite.CutSuffix(s, suffix);
}

} // end strings_package
