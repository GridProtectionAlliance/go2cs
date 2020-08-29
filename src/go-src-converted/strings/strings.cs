// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package strings implements simple functions to manipulate UTF-8 encoded strings.
//
// For information about UTF-8 strings in Go, see https://blog.golang.org/strings.
// package strings -- go2cs converted at 2020 August 29 08:42:45 UTC
// import "strings" ==> using strings = go.strings_package
// Original source: C:\Go\src\strings\strings.go
using unicode = go.unicode_package;
using utf8 = go.unicode.utf8_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class strings_package
    {
        // explode splits s into a slice of UTF-8 strings,
        // one string per Unicode character up to a maximum of n (n < 0 means no limit).
        // Invalid UTF-8 sequences become correct encodings of U+FFFD.
        private static slice<@string> explode(@string s, long n)
        {
            var l = utf8.RuneCountInString(s);
            if (n < 0L || n > l)
            {
                n = l;
            }
            var a = make_slice<@string>(n);
            for (long i = 0L; i < n - 1L; i++)
            {
                var (ch, size) = utf8.DecodeRuneInString(s);
                a[i] = s[..size];
                s = s[size..];
                if (ch == utf8.RuneError)
                {
                    a[i] = string(utf8.RuneError);
                }
            }
            if (n > 0L)
            {
                a[n - 1L] = s;
            }
            return a;
        }

        // primeRK is the prime base used in Rabin-Karp algorithm.
        private static readonly long primeRK = 16777619L;

        // hashStr returns the hash and the appropriate multiplicative
        // factor for use in Rabin-Karp algorithm.


        // hashStr returns the hash and the appropriate multiplicative
        // factor for use in Rabin-Karp algorithm.
        private static (uint, uint) hashStr(@string sep)
        {
            var hash = uint32(0L);
            {
                long i__prev1 = i;

                for (long i = 0L; i < len(sep); i++)
                {
                    hash = hash * primeRK + uint32(sep[i]);
                }


                i = i__prev1;
            }
            uint pow = 1L;            uint sq = primeRK;

            {
                long i__prev1 = i;

                i = len(sep);

                while (i > 0L)
                {
                    if (i & 1L != 0L)
                    {
                        pow *= sq;
                    i >>= 1L;
                    }
                    sq *= sq;
                }


                i = i__prev1;
            }
            return (hash, pow);
        }

        // hashStrRev returns the hash of the reverse of sep and the
        // appropriate multiplicative factor for use in Rabin-Karp algorithm.
        private static (uint, uint) hashStrRev(@string sep)
        {
            var hash = uint32(0L);
            {
                var i__prev1 = i;

                for (var i = len(sep) - 1L; i >= 0L; i--)
                {
                    hash = hash * primeRK + uint32(sep[i]);
                }


                i = i__prev1;
            }
            uint pow = 1L;            uint sq = primeRK;

            {
                var i__prev1 = i;

                i = len(sep);

                while (i > 0L)
                {
                    if (i & 1L != 0L)
                    {
                        pow *= sq;
                    i >>= 1L;
                    }
                    sq *= sq;
                }


                i = i__prev1;
            }
            return (hash, pow);
        }

        // countGeneric implements Count.
        private static long countGeneric(@string s, @string substr)
        { 
            // special case
            if (len(substr) == 0L)
            {
                return utf8.RuneCountInString(s) + 1L;
            }
            long n = 0L;
            while (true)
            {
                var i = Index(s, substr);
                if (i == -1L)
                {
                    return n;
                }
                n++;
                s = s[i + len(substr)..];
            }

        }

        // Contains reports whether substr is within s.
        public static bool Contains(@string s, @string substr)
        {
            return Index(s, substr) >= 0L;
        }

        // ContainsAny reports whether any Unicode code points in chars are within s.
        public static bool ContainsAny(@string s, @string chars)
        {
            return IndexAny(s, chars) >= 0L;
        }

        // ContainsRune reports whether the Unicode code point r is within s.
        public static bool ContainsRune(@string s, int r)
        {
            return IndexRune(s, r) >= 0L;
        }

        // LastIndex returns the index of the last instance of substr in s, or -1 if substr is not present in s.
        public static long LastIndex(@string s, @string substr)
        {
            var n = len(substr);

            if (n == 0L) 
                return len(s);
            else if (n == 1L) 
                return LastIndexByte(s, substr[0L]);
            else if (n == len(s)) 
                if (substr == s)
                {
                    return 0L;
                }
                return -1L;
            else if (n > len(s)) 
                return -1L;
            // Rabin-Karp search from the end of the string
            var (hashss, pow) = hashStrRev(substr);
            var last = len(s) - n;
            uint h = default;
            {
                var i__prev1 = i;

                for (var i = len(s) - 1L; i >= last; i--)
                {
                    h = h * primeRK + uint32(s[i]);
                }


                i = i__prev1;
            }
            if (h == hashss && s[last..] == substr)
            {
                return last;
            }
            {
                var i__prev1 = i;

                for (i = last - 1L; i >= 0L; i--)
                {
                    h *= primeRK;
                    h += uint32(s[i]);
                    h -= pow * uint32(s[i + n]);
                    if (h == hashss && s[i..i + n] == substr)
                    {
                        return i;
                    }
                }


                i = i__prev1;
            }
            return -1L;
        }

        // IndexRune returns the index of the first instance of the Unicode code point
        // r, or -1 if rune is not present in s.
        // If r is utf8.RuneError, it returns the first instance of any
        // invalid UTF-8 byte sequence.
        public static long IndexRune(@string s, int r)
        {

            if (0L <= r && r < utf8.RuneSelf) 
                return IndexByte(s, byte(r));
            else if (r == utf8.RuneError) 
                foreach (var (i, r) in s)
                {
                    if (r == utf8.RuneError)
                    {
                        return i;
                    }
                }
                return -1L;
            else if (!utf8.ValidRune(r)) 
                return -1L;
            else 
                return Index(s, string(r));
                    }

        // IndexAny returns the index of the first instance of any Unicode code point
        // from chars in s, or -1 if no Unicode code point from chars is present in s.
        public static long IndexAny(@string s, @string chars)
        {
            if (chars == "")
            { 
                // Avoid scanning all of s.
                return -1L;
            }
            if (len(s) > 8L)
            {
                {
                    var (as, isASCII) = makeASCIISet(chars);

                    if (isASCII)
                    {
                        {
                            long i__prev1 = i;

                            for (long i = 0L; i < len(s); i++)
                            {
                                if (@as.contains(s[i]))
                                {
                                    return i;
                                }
                            }


                            i = i__prev1;
                        }
                        return -1L;
                    }

                }
            }
            {
                long i__prev1 = i;

                foreach (var (__i, __c) in s)
                {
                    i = __i;
                    c = __c;
                    foreach (var (_, m) in chars)
                    {
                        if (c == m)
                        {
                            return i;
                        }
                    }
                }

                i = i__prev1;
            }

            return -1L;
        }

        // LastIndexAny returns the index of the last instance of any Unicode code
        // point from chars in s, or -1 if no Unicode code point from chars is
        // present in s.
        public static long LastIndexAny(@string s, @string chars)
        {
            if (chars == "")
            { 
                // Avoid scanning all of s.
                return -1L;
            }
            if (len(s) > 8L)
            {
                {
                    var (as, isASCII) = makeASCIISet(chars);

                    if (isASCII)
                    {
                        {
                            var i__prev1 = i;

                            for (var i = len(s) - 1L; i >= 0L; i--)
                            {
                                if (@as.contains(s[i]))
                                {
                                    return i;
                                }
                            }


                            i = i__prev1;
                        }
                        return -1L;
                    }

                }
            }
            {
                var i__prev1 = i;

                i = len(s);

                while (i > 0L)
                {
                    var (r, size) = utf8.DecodeLastRuneInString(s[..i]);
                    i -= size;
                    foreach (var (_, c) in chars)
                    {
                        if (r == c)
                        {
                            return i;
                        }
                    }
                }


                i = i__prev1;
            }
            return -1L;
        }

        // LastIndexByte returns the index of the last instance of c in s, or -1 if c is not present in s.
        public static long LastIndexByte(@string s, byte c)
        {
            for (var i = len(s) - 1L; i >= 0L; i--)
            {
                if (s[i] == c)
                {
                    return i;
                }
            }

            return -1L;
        }

        // Generic split: splits after each instance of sep,
        // including sepSave bytes of sep in the subarrays.
        private static slice<@string> genSplit(@string s, @string sep, long sepSave, long n)
        {
            if (n == 0L)
            {
                return null;
            }
            if (sep == "")
            {
                return explode(s, n);
            }
            if (n < 0L)
            {
                n = Count(s, sep) + 1L;
            }
            var a = make_slice<@string>(n);
            n--;
            long i = 0L;
            while (i < n)
            {
                var m = Index(s, sep);
                if (m < 0L)
                {
                    break;
                }
                a[i] = s[..m + sepSave];
                s = s[m + len(sep)..];
                i++;
            }

            a[i] = s;
            return a[..i + 1L];
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
        public static slice<@string> SplitN(@string s, @string sep, long n)
        {
            return genSplit(s, sep, 0L, n);
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
        public static slice<@string> SplitAfterN(@string s, @string sep, long n)
        {
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
        public static slice<@string> Split(@string s, @string sep)
        {
            return genSplit(s, sep, 0L, -1L);
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
        public static slice<@string> SplitAfter(@string s, @string sep)
        {
            return genSplit(s, sep, len(sep), -1L);
        }

        private static array<byte> asciiSpace = new array<byte>(InitKeyedValues<byte>(256, ('\t', 1), ('\n', 1), ('\v', 1), ('\f', 1), ('\r', 1), (' ', 1)));

        // Fields splits the string s around each instance of one or more consecutive white space
        // characters, as defined by unicode.IsSpace, returning a slice of substrings of s or an
        // empty slice if s contains only white space.
        public static slice<@string> Fields(@string s)
        { 
            // First count the fields.
            // This is an exact count if s is ASCII, otherwise it is an approximation.
            long n = 0L;
            long wasSpace = 1L; 
            // setBits is used to track which bits are set in the bytes of s.
            var setBits = uint8(0L);
            {
                long i__prev1 = i;

                for (long i = 0L; i < len(s); i++)
                {
                    var r = s[i];
                    setBits |= r;
                    var isSpace = int(asciiSpace[r]);
                    n += wasSpace & ~isSpace;
                    wasSpace = isSpace;
                }


                i = i__prev1;
            }

            if (setBits < utf8.RuneSelf)
            { // ASCII fast path
                var a = make_slice<@string>(n);
                long na = 0L;
                long fieldStart = 0L;
                i = 0L; 
                // Skip spaces in the front of the input.
                while (i < len(s) && asciiSpace[s[i]] != 0L)
                {
                    i++;
                }

                fieldStart = i;
                while (i < len(s))
                {
                    if (asciiSpace[s[i]] == 0L)
                    {
                        i++;
                        continue;
                    }
                    a[na] = s[fieldStart..i];
                    na++;
                    i++; 
                    // Skip spaces in between fields.
                    while (i < len(s) && asciiSpace[s[i]] != 0L)
                    {
                        i++;
                    }

                    fieldStart = i;
                }

                if (fieldStart < len(s))
                { // Last field might end at EOF.
                    a[na] = s[fieldStart..];
                }
                return a;
            } 

            // Some runes in the input string are not ASCII.
            return FieldsFunc(s, unicode.IsSpace);
        }

        // FieldsFunc splits the string s at each run of Unicode code points c satisfying f(c)
        // and returns an array of slices of s. If all code points in s satisfy f(c) or the
        // string is empty, an empty slice is returned.
        // FieldsFunc makes no guarantees about the order in which it calls f(c).
        // If f does not return consistent results for a given c, FieldsFunc may crash.
        public static slice<@string> FieldsFunc(@string s, Func<int, bool> f)
        { 
            // A span is used to record a slice of s of the form s[start:end].
            // The start index is inclusive and the end index is exclusive.
            private partial struct span
            {
                public long start;
                public long end;
            }
            var spans = make_slice<span>(0L, 32L); 

            // Find the field start and end indices.
            var wasField = false;
            long fromIndex = 0L;
            {
                var i__prev1 = i;

                foreach (var (__i, __rune) in s)
                {
                    i = __i;
                    rune = __rune;
                    if (f(rune))
                    {
                        if (wasField)
                        {
                            spans = append(spans, new span(start:fromIndex,end:i));
                            wasField = false;
                        }
                    }
                    else
                    {
                        if (!wasField)
                        {
                            fromIndex = i;
                            wasField = true;
                        }
                    }
                } 

                // Last field might end at EOF.

                i = i__prev1;
            }

            if (wasField)
            {
                spans = append(spans, new span(fromIndex,len(s)));
            } 

            // Create strings from recorded field indices.
            var a = make_slice<@string>(len(spans));
            {
                var i__prev1 = i;

                foreach (var (__i, __span) in spans)
                {
                    i = __i;
                    span = __span;
                    a[i] = s[span.start..span.end];
                }

                i = i__prev1;
            }

            return a;
        }

        // Join concatenates the elements of a to create a single string. The separator string
        // sep is placed between elements in the resulting string.
        public static @string Join(slice<@string> a, @string sep)
        {
            switch (len(a))
            {
                case 0L: 
                    return "";
                    break;
                case 1L: 
                    return a[0L];
                    break;
                case 2L: 
                    // Special case for common small values.
                    // Remove if golang.org/issue/6714 is fixed
                    return a[0L] + sep + a[1L];
                    break;
                case 3L: 
                    // Special case for common small values.
                    // Remove if golang.org/issue/6714 is fixed
                    return a[0L] + sep + a[1L] + sep + a[2L];
                    break;
            }
            var n = len(sep) * (len(a) - 1L);
            for (long i = 0L; i < len(a); i++)
            {
                n += len(a[i]);
            }


            var b = make_slice<byte>(n);
            var bp = copy(b, a[0L]);
            foreach (var (_, s) in a[1L..])
            {
                bp += copy(b[bp..], sep);
                bp += copy(b[bp..], s);
            }
            return string(b);
        }

        // HasPrefix tests whether the string s begins with prefix.
        public static bool HasPrefix(@string s, @string prefix)
        {
            return len(s) >= len(prefix) && s[0L..len(prefix)] == prefix;
        }

        // HasSuffix tests whether the string s ends with suffix.
        public static bool HasSuffix(@string s, @string suffix)
        {
            return len(s) >= len(suffix) && s[len(s) - len(suffix)..] == suffix;
        }

        // Map returns a copy of the string s with all its characters modified
        // according to the mapping function. If mapping returns a negative value, the character is
        // dropped from the string with no replacement.
        public static @string Map(Func<int, int> mapping, @string s)
        { 
            // In the worst case, the string can grow when mapped, making
            // things unpleasant. But it's so rare we barge in assuming it's
            // fine. It could also shrink but that falls out naturally.

            // The output buffer b is initialized on demand, the first
            // time a character differs.
            slice<byte> b = default; 
            // nbytes is the number of bytes encoded in b.
            long nbytes = default;

            {
                var c__prev1 = c;

                foreach (var (__i, __c) in s)
                {
                    i = __i;
                    c = __c;
                    var r = mapping(c);
                    if (r == c)
                    {
                        continue;
                    }
                    b = make_slice<byte>(len(s) + utf8.UTFMax);
                    nbytes = copy(b, s[..i]);
                    if (r >= 0L)
                    {
                        if (r <= utf8.RuneSelf)
                        {
                            b[nbytes] = byte(r);
                            nbytes++;
                        }
                        else
                        {
                            nbytes += utf8.EncodeRune(b[nbytes..], r);
                        }
                    }
                    if (c == utf8.RuneError)
                    { 
                        // RuneError is the result of either decoding
                        // an invalid sequence or '\uFFFD'. Determine
                        // the correct number of bytes we need to advance.
                        var (_, w) = utf8.DecodeRuneInString(s[i..]);
                        i += w;
                    }
                    else
                    {
                        i += utf8.RuneLen(c);
                    }
                    s = s[i..];
                    break;
                }

                c = c__prev1;
            }

            if (b == null)
            {
                return s;
            }
            {
                var c__prev1 = c;

                foreach (var (_, __c) in s)
                {
                    c = __c;
                    r = mapping(c); 

                    // common case
                    if ((0L <= r && r <= utf8.RuneSelf) && nbytes < len(b))
                    {
                        b[nbytes] = byte(r);
                        nbytes++;
                        continue;
                    } 

                    // b is not big enough or r is not a ASCII rune.
                    if (r >= 0L)
                    {
                        if (nbytes + utf8.UTFMax >= len(b))
                        { 
                            // Grow the buffer.
                            var nb = make_slice<byte>(2L * len(b));
                            copy(nb, b[..nbytes]);
                            b = nb;
                        }
                        nbytes += utf8.EncodeRune(b[nbytes..], r);
                    }
                }

                c = c__prev1;
            }

            return string(b[..nbytes]);
        }

        // Repeat returns a new string consisting of count copies of the string s.
        //
        // It panics if count is negative or if
        // the result of (len(s) * count) overflows.
        public static @string Repeat(@string s, long count) => func((_, panic, __) =>
        { 
            // Since we cannot return an error on overflow,
            // we should panic if the repeat will generate
            // an overflow.
            // See Issue golang.org/issue/16237
            if (count < 0L)
            {
                panic("strings: negative Repeat count");
            }
            else if (count > 0L && len(s) * count / count != len(s))
            {
                panic("strings: Repeat count causes overflow");
            }
            var b = make_slice<byte>(len(s) * count);
            var bp = copy(b, s);
            while (bp < len(b))
            {
                copy(b[bp..], b[..bp]);
                bp *= 2L;
            }

            return string(b);
        });

        // ToUpper returns a copy of the string s with all Unicode letters mapped to their upper case.
        public static @string ToUpper(@string s)
        {
            var isASCII = true;
            var hasLower = false;
            {
                long i__prev1 = i;

                for (long i = 0L; i < len(s); i++)
                {
                    var c = s[i];
                    if (c >= utf8.RuneSelf)
                    {
                        isASCII = false;
                        break;
                    }
                    hasLower = hasLower || (c >= 'a' && c <= 'z');
                }


                i = i__prev1;
            }

            if (isASCII)
            { // optimize for ASCII-only strings.
                if (!hasLower)
                {
                    return s;
                }
                var b = make_slice<byte>(len(s));
                {
                    long i__prev1 = i;

                    for (i = 0L; i < len(s); i++)
                    {
                        c = s[i];
                        if (c >= 'a' && c <= 'z')
                        {
                            c -= 'a' - 'A';
                        }
                        b[i] = c;
                    }


                    i = i__prev1;
                }
                return string(b);
            }
            return Map(unicode.ToUpper, s);
        }

        // ToLower returns a copy of the string s with all Unicode letters mapped to their lower case.
        public static @string ToLower(@string s)
        {
            var isASCII = true;
            var hasUpper = false;
            {
                long i__prev1 = i;

                for (long i = 0L; i < len(s); i++)
                {
                    var c = s[i];
                    if (c >= utf8.RuneSelf)
                    {
                        isASCII = false;
                        break;
                    }
                    hasUpper = hasUpper || (c >= 'A' && c <= 'Z');
                }


                i = i__prev1;
            }

            if (isASCII)
            { // optimize for ASCII-only strings.
                if (!hasUpper)
                {
                    return s;
                }
                var b = make_slice<byte>(len(s));
                {
                    long i__prev1 = i;

                    for (i = 0L; i < len(s); i++)
                    {
                        c = s[i];
                        if (c >= 'A' && c <= 'Z')
                        {
                            c += 'a' - 'A';
                        }
                        b[i] = c;
                    }


                    i = i__prev1;
                }
                return string(b);
            }
            return Map(unicode.ToLower, s);
        }

        // ToTitle returns a copy of the string s with all Unicode letters mapped to their title case.
        public static @string ToTitle(@string s)
        {
            return Map(unicode.ToTitle, s);
        }

        // ToUpperSpecial returns a copy of the string s with all Unicode letters mapped to their
        // upper case, giving priority to the special casing rules.
        public static @string ToUpperSpecial(unicode.SpecialCase c, @string s)
        {
            return Map(r => c.ToUpper(r), s);
        }

        // ToLowerSpecial returns a copy of the string s with all Unicode letters mapped to their
        // lower case, giving priority to the special casing rules.
        public static @string ToLowerSpecial(unicode.SpecialCase c, @string s)
        {
            return Map(r => c.ToLower(r), s);
        }

        // ToTitleSpecial returns a copy of the string s with all Unicode letters mapped to their
        // title case, giving priority to the special casing rules.
        public static @string ToTitleSpecial(unicode.SpecialCase c, @string s)
        {
            return Map(r => c.ToTitle(r), s);
        }

        // isSeparator reports whether the rune could mark a word boundary.
        // TODO: update when package unicode captures more of the properties.
        private static bool isSeparator(int r)
        { 
            // ASCII alphanumerics and underscore are not separators
            if (r <= 0x7FUL)
            {

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
            // Letters and digits are not separators
            if (unicode.IsLetter(r) || unicode.IsDigit(r))
            {
                return false;
            } 
            // Otherwise, all we can do for now is treat spaces as separators.
            return unicode.IsSpace(r);
        }

        // Title returns a copy of the string s with all Unicode letters that begin words
        // mapped to their title case.
        //
        // BUG(rsc): The rule Title uses for word boundaries does not handle Unicode punctuation properly.
        public static @string Title(@string s)
        { 
            // Use a closure here to remember state.
            // Hackish but effective. Depends on Map scanning in order and calling
            // the closure once per rune.
            char prev = ' ';
            return Map(r =>
            {
                if (isSeparator(prev))
                {
                    prev = r;
                    return unicode.ToTitle(r);
                }
                prev = r;
                return r;
            }, s);
        }

        // TrimLeftFunc returns a slice of the string s with all leading
        // Unicode code points c satisfying f(c) removed.
        public static @string TrimLeftFunc(@string s, Func<int, bool> f)
        {
            var i = indexFunc(s, f, false);
            if (i == -1L)
            {
                return "";
            }
            return s[i..];
        }

        // TrimRightFunc returns a slice of the string s with all trailing
        // Unicode code points c satisfying f(c) removed.
        public static @string TrimRightFunc(@string s, Func<int, bool> f)
        {
            var i = lastIndexFunc(s, f, false);
            if (i >= 0L && s[i] >= utf8.RuneSelf)
            {
                var (_, wid) = utf8.DecodeRuneInString(s[i..]);
                i += wid;
            }
            else
            {
                i++;
            }
            return s[0L..i];
        }

        // TrimFunc returns a slice of the string s with all leading
        // and trailing Unicode code points c satisfying f(c) removed.
        public static @string TrimFunc(@string s, Func<int, bool> f)
        {
            return TrimRightFunc(TrimLeftFunc(s, f), f);
        }

        // IndexFunc returns the index into s of the first Unicode
        // code point satisfying f(c), or -1 if none do.
        public static long IndexFunc(@string s, Func<int, bool> f)
        {
            return indexFunc(s, f, true);
        }

        // LastIndexFunc returns the index into s of the last
        // Unicode code point satisfying f(c), or -1 if none do.
        public static long LastIndexFunc(@string s, Func<int, bool> f)
        {
            return lastIndexFunc(s, f, true);
        }

        // indexFunc is the same as IndexFunc except that if
        // truth==false, the sense of the predicate function is
        // inverted.
        private static long indexFunc(@string s, Func<int, bool> f, bool truth)
        {
            foreach (var (i, r) in s)
            {
                if (f(r) == truth)
                {
                    return i;
                }
            }
            return -1L;
        }

        // lastIndexFunc is the same as LastIndexFunc except that if
        // truth==false, the sense of the predicate function is
        // inverted.
        private static long lastIndexFunc(@string s, Func<int, bool> f, bool truth)
        {
            {
                var i = len(s);

                while (i > 0L)
                {
                    var (r, size) = utf8.DecodeLastRuneInString(s[0L..i]);
                    i -= size;
                    if (f(r) == truth)
                    {
                        return i;
                    }
                }

            }
            return -1L;
        }

        // asciiSet is a 32-byte value, where each bit represents the presence of a
        // given ASCII character in the set. The 128-bits of the lower 16 bytes,
        // starting with the least-significant bit of the lowest word to the
        // most-significant bit of the highest word, map to the full range of all
        // 128 ASCII characters. The 128-bits of the upper 16 bytes will be zeroed,
        // ensuring that any non-ASCII character will be reported as not in the set.
        private partial struct asciiSet // : array<uint>
        {
        }

        // makeASCIISet creates a set of ASCII characters and reports whether all
        // characters in chars are ASCII.
        private static (asciiSet, bool) makeASCIISet(@string chars)
        {
            for (long i = 0L; i < len(chars); i++)
            {
                var c = chars[i];
                if (c >= utf8.RuneSelf)
                {
                    return (as, false);
                }
                as[c >> (int)(5L)] |= 1L << (int)(uint(c & 31L));
            }

            return (as, true);
        }

        // contains reports whether c is inside the set.
        private static bool contains(this ref asciiSet @as, byte c)
        {
            return (as[c >> (int)(5L)] & (1L << (int)(uint(c & 31L)))) != 0L;
        }

        private static Func<int, bool> makeCutsetFunc(@string cutset)
        {
            if (len(cutset) == 1L && cutset[0L] < utf8.RuneSelf)
            {
                return r =>
                {
                    return r == rune(cutset[0L]);
                }
;
            }
            {
                var (as, isASCII) = makeASCIISet(cutset);

                if (isASCII)
                {
                    return r =>
                    {
                        return r < utf8.RuneSelf && @as.contains(byte(r));
                    }
;
                }

            }
            return r => IndexRune(cutset, r) >= 0L;
        }

        // Trim returns a slice of the string s with all leading and
        // trailing Unicode code points contained in cutset removed.
        public static @string Trim(@string s, @string cutset)
        {
            if (s == "" || cutset == "")
            {
                return s;
            }
            return TrimFunc(s, makeCutsetFunc(cutset));
        }

        // TrimLeft returns a slice of the string s with all leading
        // Unicode code points contained in cutset removed.
        public static @string TrimLeft(@string s, @string cutset)
        {
            if (s == "" || cutset == "")
            {
                return s;
            }
            return TrimLeftFunc(s, makeCutsetFunc(cutset));
        }

        // TrimRight returns a slice of the string s, with all trailing
        // Unicode code points contained in cutset removed.
        public static @string TrimRight(@string s, @string cutset)
        {
            if (s == "" || cutset == "")
            {
                return s;
            }
            return TrimRightFunc(s, makeCutsetFunc(cutset));
        }

        // TrimSpace returns a slice of the string s, with all leading
        // and trailing white space removed, as defined by Unicode.
        public static @string TrimSpace(@string s)
        {
            return TrimFunc(s, unicode.IsSpace);
        }

        // TrimPrefix returns s without the provided leading prefix string.
        // If s doesn't start with prefix, s is returned unchanged.
        public static @string TrimPrefix(@string s, @string prefix)
        {
            if (HasPrefix(s, prefix))
            {
                return s[len(prefix)..];
            }
            return s;
        }

        // TrimSuffix returns s without the provided trailing suffix string.
        // If s doesn't end with suffix, s is returned unchanged.
        public static @string TrimSuffix(@string s, @string suffix)
        {
            if (HasSuffix(s, suffix))
            {
                return s[..len(s) - len(suffix)];
            }
            return s;
        }

        // Replace returns a copy of the string s with the first n
        // non-overlapping instances of old replaced by new.
        // If old is empty, it matches at the beginning of the string
        // and after each UTF-8 sequence, yielding up to k+1 replacements
        // for a k-rune string.
        // If n < 0, there is no limit on the number of replacements.
        public static @string Replace(@string s, @string old, @string @new, long n)
        {
            if (old == new || n == 0L)
            {
                return s; // avoid allocation
            } 

            // Compute number of replacements.
            {
                var m = Count(s, old);

                if (m == 0L)
                {
                    return s; // avoid allocation
                }
                else if (n < 0L || m < n)
                {
                    n = m;
                } 

                // Apply replacements to buffer.

            } 

            // Apply replacements to buffer.
            var t = make_slice<byte>(len(s) + n * (len(new) - len(old)));
            long w = 0L;
            long start = 0L;
            for (long i = 0L; i < n; i++)
            {
                var j = start;
                if (len(old) == 0L)
                {
                    if (i > 0L)
                    {
                        var (_, wid) = utf8.DecodeRuneInString(s[start..]);
                        j += wid;
                    }
                }
                else
                {
                    j += Index(s[start..], old);
                }
                w += copy(t[w..], s[start..j]);
                w += copy(t[w..], new);
                start = j + len(old);
            }

            w += copy(t[w..], s[start..]);
            return string(t[0L..w]);
        }

        // EqualFold reports whether s and t, interpreted as UTF-8 strings,
        // are equal under Unicode case-folding.
        public static bool EqualFold(@string s, @string t)
        {
            while (s != "" && t != "")
            { 
                // Extract first rune from each string.
                int sr = default;                int tr = default;

                if (s[0L] < utf8.RuneSelf)
                {
                    sr = rune(s[0L]);
                    s = s[1L..];
                }
                else
                {
                    var (r, size) = utf8.DecodeRuneInString(s);
                    sr = r;
                    s = s[size..];
                }
                if (t[0L] < utf8.RuneSelf)
                {
                    tr = rune(t[0L]);
                    t = t[1L..];
                }
                else
                {
                    (r, size) = utf8.DecodeRuneInString(t);
                    tr = r;
                    t = t[size..];
                } 

                // If they match, keep going; if not, return false.

                // Easy case.
                if (tr == sr)
                {
                    continue;
                } 

                // Make sr < tr to simplify what follows.
                if (tr < sr)
                {
                    tr = sr;
                    sr = tr;
                } 
                // Fast check for ASCII.
                if (tr < utf8.RuneSelf && 'A' <= sr && sr <= 'Z')
                { 
                    // ASCII, and sr is upper case.  tr must be lower case.
                    if (tr == sr + 'a' - 'A')
                    {
                        continue;
                    }
                    return false;
                } 

                // General case. SimpleFold(x) returns the next equivalent rune > x
                // or wraps around to smaller values.
                var r = unicode.SimpleFold(sr);
                while (r != sr && r < tr)
                {
                    r = unicode.SimpleFold(r);
                }

                if (r == tr)
                {
                    continue;
                }
                return false;
            } 

            // One string is empty. Are both?
 

            // One string is empty. Are both?
            return s == t;
        }

        private static long indexRabinKarp(@string s, @string substr)
        { 
            // Rabin-Karp search
            var (hashss, pow) = hashStr(substr);
            var n = len(substr);
            uint h = default;
            {
                long i__prev1 = i;

                for (long i = 0L; i < n; i++)
                {
                    h = h * primeRK + uint32(s[i]);
                }


                i = i__prev1;
            }
            if (h == hashss && s[..n] == substr)
            {
                return 0L;
            }
            {
                long i__prev1 = i;

                i = n;

                while (i < len(s))
                {
                    h *= primeRK;
                    h += uint32(s[i]);
                    h -= pow * uint32(s[i - n]);
                    i++;
                    if (h == hashss && s[i - n..i] == substr)
                    {
                        return i - n;
                    }
                }


                i = i__prev1;
            }
            return -1L;

        }
    }
}
