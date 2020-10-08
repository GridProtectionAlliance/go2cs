// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package bytes implements functions for the manipulation of byte slices.
// It is analogous to the facilities of the strings package.
// package bytes -- go2cs converted at 2020 October 08 03:49:09 UTC
// import "bytes" ==> using bytes = go.bytes_package
// Original source: C:\Go\src\bytes\bytes.go
using bytealg = go.@internal.bytealg_package;
using unicode = go.unicode_package;
using utf8 = go.unicode.utf8_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class bytes_package
    {
        // Equal reports whether a and b
        // are the same length and contain the same bytes.
        // A nil argument is equivalent to an empty slice.
        public static bool Equal(slice<byte> a, slice<byte> b)
        { 
            // Neither cmd/compile nor gccgo allocates for these string conversions.
            return string(a) == string(b);

        }

        // Compare returns an integer comparing two byte slices lexicographically.
        // The result will be 0 if a==b, -1 if a < b, and +1 if a > b.
        // A nil argument is equivalent to an empty slice.
        public static long Compare(slice<byte> a, slice<byte> b)
        {
            return bytealg.Compare(a, b);
        }

        // explode splits s into a slice of UTF-8 sequences, one per Unicode code point (still slices of bytes),
        // up to a maximum of n byte slices. Invalid UTF-8 sequences are chopped into individual bytes.
        private static slice<slice<byte>> explode(slice<byte> s, long n)
        {
            if (n <= 0L)
            {
                n = len(s);
            }

            var a = make_slice<slice<byte>>(n);
            long size = default;
            long na = 0L;
            while (len(s) > 0L)
            {
                if (na + 1L >= n)
                {
                    a[na] = s;
                    na++;
                    break;
                }

                _, size = utf8.DecodeRune(s);
                a[na] = s.slice(0L, size, size);
                s = s[size..];
                na++;

            }

            return a[0L..na];

        }

        // Count counts the number of non-overlapping instances of sep in s.
        // If sep is an empty slice, Count returns 1 + the number of UTF-8-encoded code points in s.
        public static long Count(slice<byte> s, slice<byte> sep)
        { 
            // special case
            if (len(sep) == 0L)
            {
                return utf8.RuneCount(s) + 1L;
            }

            if (len(sep) == 1L)
            {
                return bytealg.Count(s, sep[0L]);
            }

            long n = 0L;
            while (true)
            {
                var i = Index(s, sep);
                if (i == -1L)
                {
                    return n;
                }

                n++;
                s = s[i + len(sep)..];

            }


        }

        // Contains reports whether subslice is within b.
        public static bool Contains(slice<byte> b, slice<byte> subslice)
        {
            return Index(b, subslice) != -1L;
        }

        // ContainsAny reports whether any of the UTF-8-encoded code points in chars are within b.
        public static bool ContainsAny(slice<byte> b, @string chars)
        {
            return IndexAny(b, chars) >= 0L;
        }

        // ContainsRune reports whether the rune is contained in the UTF-8-encoded byte slice b.
        public static bool ContainsRune(slice<byte> b, int r)
        {
            return IndexRune(b, r) >= 0L;
        }

        // IndexByte returns the index of the first instance of c in b, or -1 if c is not present in b.
        public static long IndexByte(slice<byte> b, byte c)
        {
            return bytealg.IndexByte(b, c);
        }

        private static long indexBytePortable(slice<byte> s, byte c)
        {
            foreach (var (i, b) in s)
            {
                if (b == c)
                {
                    return i;
                }

            }
            return -1L;

        }

        // LastIndex returns the index of the last instance of sep in s, or -1 if sep is not present in s.
        public static long LastIndex(slice<byte> s, slice<byte> sep)
        {
            var n = len(sep);

            if (n == 0L) 
                return len(s);
            else if (n == 1L) 
                return LastIndexByte(s, sep[0L]);
            else if (n == len(s)) 
                if (Equal(s, sep))
                {
                    return 0L;
                }

                return -1L;
            else if (n > len(s)) 
                return -1L;
            // Rabin-Karp search from the end of the string
            var (hashss, pow) = bytealg.HashStrRevBytes(sep);
            var last = len(s) - n;
            uint h = default;
            {
                var i__prev1 = i;

                for (var i = len(s) - 1L; i >= last; i--)
                {
                    h = h * bytealg.PrimeRK + uint32(s[i]);
                }


                i = i__prev1;
            }
            if (h == hashss && Equal(s[last..], sep))
            {
                return last;
            }

            {
                var i__prev1 = i;

                for (i = last - 1L; i >= 0L; i--)
                {
                    h *= bytealg.PrimeRK;
                    h += uint32(s[i]);
                    h -= pow * uint32(s[i + n]);
                    if (h == hashss && Equal(s[i..i + n], sep))
                    {
                        return i;
                    }

                }


                i = i__prev1;
            }
            return -1L;

        }

        // LastIndexByte returns the index of the last instance of c in s, or -1 if c is not present in s.
        public static long LastIndexByte(slice<byte> s, byte c)
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

        // IndexRune interprets s as a sequence of UTF-8-encoded code points.
        // It returns the byte index of the first occurrence in s of the given rune.
        // It returns -1 if rune is not present in s.
        // If r is utf8.RuneError, it returns the first instance of any
        // invalid UTF-8 byte sequence.
        public static long IndexRune(slice<byte> s, int r)
        {

            if (0L <= r && r < utf8.RuneSelf) 
                return IndexByte(s, byte(r));
            else if (r == utf8.RuneError) 
                {
                    long i = 0L;

                    while (i < len(s))
                    {
                        var (r1, n) = utf8.DecodeRune(s[i..]);
                        if (r1 == utf8.RuneError)
                        {
                            return i;
                        }

                        i += n;

                    }

                }
                return -1L;
            else if (!utf8.ValidRune(r)) 
                return -1L;
            else 
                array<byte> b = new array<byte>(utf8.UTFMax);
                var n = utf8.EncodeRune(b[..], r);
                return Index(s, b[..n]);
            
        }

        // IndexAny interprets s as a sequence of UTF-8-encoded Unicode code points.
        // It returns the byte index of the first occurrence in s of any of the Unicode
        // code points in chars. It returns -1 if chars is empty or if there is no code
        // point in common.
        public static long IndexAny(slice<byte> s, @string chars)
        {
            if (chars == "")
            { 
                // Avoid scanning all of s.
                return -1L;

            }

            if (len(s) == 1L)
            {
                var r = rune(s[0L]);
                if (r >= utf8.RuneSelf)
                { 
                    // search utf8.RuneError.
                    foreach (var (_, __r) in chars)
                    {
                        r = __r;
                        if (r == utf8.RuneError)
                        {
                            return 0L;
                        }

                    }

                    return -1L;

                }

                if (bytealg.IndexByteString(chars, s[0L]) >= 0L)
                {
                    return 0L;
                }

                return -1L;

            }

            if (len(chars) == 1L)
            {
                r = rune(chars[0L]);
                if (r >= utf8.RuneSelf)
                {
                    r = utf8.RuneError;
                }

                return IndexRune(s, r);

            }

            if (len(s) > 8L)
            {
                {
                    var (as, isASCII) = makeASCIISet(chars);

                    if (isASCII)
                    {
                        {
                            var i__prev1 = i;

                            foreach (var (__i, __c) in s)
                            {
                                i = __i;
                                c = __c;
                                if (@as.contains(c))
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

            long width = default;
            {
                var i__prev1 = i;

                long i = 0L;

                while (i < len(s))
                {
                    r = rune(s[i]);
                    if (r < utf8.RuneSelf)
                    {
                        if (bytealg.IndexByteString(chars, s[i]) >= 0L)
                        {
                            return i;
                    i += width;
                        }

                        width = 1L;
                        continue;

                    }

                    r, width = utf8.DecodeRune(s[i..]);
                    if (r == utf8.RuneError)
                    {
                        foreach (var (_, __r) in chars)
                        {
                            r = __r;
                            if (r == utf8.RuneError)
                            {
                                return i;
                            }

                        }

                        continue;

                    } 
                    // r is 2 to 4 bytes. Using strings.Index is more reasonable, but as the bytes
                    // package should not import the strings package, use bytealg.IndexString
                    // instead. And this does not seem to lose much performance.
                    if (chars == string(r) || bytealg.IndexString(chars, string(r)) >= 0L)
                    {
                        return i;
                    }

                }


                i = i__prev1;
            }
            return -1L;

        }

        // LastIndexAny interprets s as a sequence of UTF-8-encoded Unicode code
        // points. It returns the byte index of the last occurrence in s of any of
        // the Unicode code points in chars. It returns -1 if chars is empty or if
        // there is no code point in common.
        public static long LastIndexAny(slice<byte> s, @string chars)
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

            if (len(s) == 1L)
            {
                var r = rune(s[0L]);
                if (r >= utf8.RuneSelf)
                {
                    foreach (var (_, __r) in chars)
                    {
                        r = __r;
                        if (r == utf8.RuneError)
                        {
                            return 0L;
                        }

                    }

                    return -1L;

                }

                if (bytealg.IndexByteString(chars, s[0L]) >= 0L)
                {
                    return 0L;
                }

                return -1L;

            }

            if (len(chars) == 1L)
            {
                var cr = rune(chars[0L]);
                if (cr >= utf8.RuneSelf)
                {
                    cr = utf8.RuneError;
                }

                {
                    var i__prev1 = i;

                    i = len(s);

                    while (i > 0L)
                    {
                        var (r, size) = utf8.DecodeLastRune(s[..i]);
                        i -= size;
                        if (r == cr)
                        {
                            return i;
                        }

                    }


                    i = i__prev1;
                }
                return -1L;

            }

            {
                var i__prev1 = i;

                i = len(s);

                while (i > 0L)
                {
                    r = rune(s[i - 1L]);
                    if (r < utf8.RuneSelf)
                    {
                        if (bytealg.IndexByteString(chars, s[i - 1L]) >= 0L)
                        {
                            return i - 1L;
                        }

                        i--;
                        continue;

                    }

                    (r, size) = utf8.DecodeLastRune(s[..i]);
                    i -= size;
                    if (r == utf8.RuneError)
                    {
                        foreach (var (_, __r) in chars)
                        {
                            r = __r;
                            if (r == utf8.RuneError)
                            {
                                return i;
                            }

                        }

                        continue;

                    } 
                    // r is 2 to 4 bytes. Using strings.Index is more reasonable, but as the bytes
                    // package should not import the strings package, use bytealg.IndexString
                    // instead. And this does not seem to lose much performance.
                    if (chars == string(r) || bytealg.IndexString(chars, string(r)) >= 0L)
                    {
                        return i;
                    }

                }


                i = i__prev1;
            }
            return -1L;

        }

        // Generic split: splits after each instance of sep,
        // including sepSave bytes of sep in the subslices.
        private static slice<slice<byte>> genSplit(slice<byte> s, slice<byte> sep, long sepSave, long n)
        {
            if (n == 0L)
            {
                return null;
            }

            if (len(sep) == 0L)
            {
                return explode(s, n);
            }

            if (n < 0L)
            {
                n = Count(s, sep) + 1L;
            }

            var a = make_slice<slice<byte>>(n);
            n--;
            long i = 0L;
            while (i < n)
            {
                var m = Index(s, sep);
                if (m < 0L)
                {
                    break;
                }

                a[i] = s.slice(-1, m + sepSave, m + sepSave);
                s = s[m + len(sep)..];
                i++;

            }

            a[i] = s;
            return a[..i + 1L];

        }

        // SplitN slices s into subslices separated by sep and returns a slice of
        // the subslices between those separators.
        // If sep is empty, SplitN splits after each UTF-8 sequence.
        // The count determines the number of subslices to return:
        //   n > 0: at most n subslices; the last subslice will be the unsplit remainder.
        //   n == 0: the result is nil (zero subslices)
        //   n < 0: all subslices
        public static slice<slice<byte>> SplitN(slice<byte> s, slice<byte> sep, long n)
        {
            return genSplit(s, sep, 0L, n);
        }

        // SplitAfterN slices s into subslices after each instance of sep and
        // returns a slice of those subslices.
        // If sep is empty, SplitAfterN splits after each UTF-8 sequence.
        // The count determines the number of subslices to return:
        //   n > 0: at most n subslices; the last subslice will be the unsplit remainder.
        //   n == 0: the result is nil (zero subslices)
        //   n < 0: all subslices
        public static slice<slice<byte>> SplitAfterN(slice<byte> s, slice<byte> sep, long n)
        {
            return genSplit(s, sep, len(sep), n);
        }

        // Split slices s into all subslices separated by sep and returns a slice of
        // the subslices between those separators.
        // If sep is empty, Split splits after each UTF-8 sequence.
        // It is equivalent to SplitN with a count of -1.
        public static slice<slice<byte>> Split(slice<byte> s, slice<byte> sep)
        {
            return genSplit(s, sep, 0L, -1L);
        }

        // SplitAfter slices s into all subslices after each instance of sep and
        // returns a slice of those subslices.
        // If sep is empty, SplitAfter splits after each UTF-8 sequence.
        // It is equivalent to SplitAfterN with a count of -1.
        public static slice<slice<byte>> SplitAfter(slice<byte> s, slice<byte> sep)
        {
            return genSplit(s, sep, len(sep), -1L);
        }

        private static array<byte> asciiSpace = new array<byte>(InitKeyedValues<byte>(256, ('\t', 1), ('\n', 1), ('\v', 1), ('\f', 1), ('\r', 1), (' ', 1)));

        // Fields interprets s as a sequence of UTF-8-encoded code points.
        // It splits the slice s around each instance of one or more consecutive white space
        // characters, as defined by unicode.IsSpace, returning a slice of subslices of s or an
        // empty slice if s contains only white space.
        public static slice<slice<byte>> Fields(slice<byte> s)
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

            if (setBits >= utf8.RuneSelf)
            { 
                // Some runes in the input slice are not ASCII.
                return FieldsFunc(s, unicode.IsSpace);

            } 

            // ASCII fast path
            var a = make_slice<slice<byte>>(n);
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

                a[na] = s.slice(fieldStart, i, i);
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
        public static slice<slice<byte>> FieldsFunc(slice<byte> s, Func<int, bool> f)
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
            // Doing this in a separate pass (rather than slicing the string s
            // and collecting the result substrings right away) is significantly
            // more efficient, possibly due to cache effects.
            long start = -1L; // valid span start if >= 0
            {
                long i__prev1 = i;

                long i = 0L;

                while (i < len(s))
                {
                    long size = 1L;
                    var r = rune(s[i]);
                    if (r >= utf8.RuneSelf)
                    {
                        r, size = utf8.DecodeRune(s[i..]);
                    }

                    if (f(r))
                    {
                        if (start >= 0L)
                        {
                            spans = append(spans, new span(start,i));
                            start = -1L;
                        }

                    }
                    else
                    {
                        if (start < 0L)
                        {
                            start = i;
                        }

                    }

                    i += size;

                } 

                // Last field might end at EOF.


                i = i__prev1;
            } 

            // Last field might end at EOF.
            if (start >= 0L)
            {
                spans = append(spans, new span(start,len(s)));
            } 

            // Create subslices from recorded field indices.
            var a = make_slice<slice<byte>>(len(spans));
            {
                long i__prev1 = i;

                foreach (var (__i, __span) in spans)
                {
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
        public static slice<byte> Join(slice<slice<byte>> s, slice<byte> sep)
        {
            if (len(s) == 0L)
            {
                return new slice<byte>(new byte[] {  });
            }

            if (len(s) == 1L)
            { 
                // Just return a copy.
                return append((slice<byte>)null, s[0L]);

            }

            var n = len(sep) * (len(s) - 1L);
            {
                var v__prev1 = v;

                foreach (var (_, __v) in s)
                {
                    v = __v;
                    n += len(v);
                }

                v = v__prev1;
            }

            var b = make_slice<byte>(n);
            var bp = copy(b, s[0L]);
            {
                var v__prev1 = v;

                foreach (var (_, __v) in s[1L..])
                {
                    v = __v;
                    bp += copy(b[bp..], sep);
                    bp += copy(b[bp..], v);
                }

                v = v__prev1;
            }

            return b;

        }

        // HasPrefix tests whether the byte slice s begins with prefix.
        public static bool HasPrefix(slice<byte> s, slice<byte> prefix)
        {
            return len(s) >= len(prefix) && Equal(s[0L..len(prefix)], prefix);
        }

        // HasSuffix tests whether the byte slice s ends with suffix.
        public static bool HasSuffix(slice<byte> s, slice<byte> suffix)
        {
            return len(s) >= len(suffix) && Equal(s[len(s) - len(suffix)..], suffix);
        }

        // Map returns a copy of the byte slice s with all its characters modified
        // according to the mapping function. If mapping returns a negative value, the character is
        // dropped from the byte slice with no replacement. The characters in s and the
        // output are interpreted as UTF-8-encoded code points.
        public static slice<byte> Map(Func<int, int> mapping, slice<byte> s)
        { 
            // In the worst case, the slice can grow when mapped, making
            // things unpleasant. But it's so rare we barge in assuming it's
            // fine. It could also shrink but that falls out naturally.
            var maxbytes = len(s); // length of b
            long nbytes = 0L; // number of bytes encoded in b
            var b = make_slice<byte>(maxbytes);
            {
                long i = 0L;

                while (i < len(s))
                {
                    long wid = 1L;
                    var r = rune(s[i]);
                    if (r >= utf8.RuneSelf)
                    {
                        r, wid = utf8.DecodeRune(s[i..]);
                    }

                    r = mapping(r);
                    if (r >= 0L)
                    {
                        var rl = utf8.RuneLen(r);
                        if (rl < 0L)
                        {
                            rl = len(string(utf8.RuneError));
                        }

                        if (nbytes + rl > maxbytes)
                        { 
                            // Grow the buffer.
                            maxbytes = maxbytes * 2L + utf8.UTFMax;
                            var nb = make_slice<byte>(maxbytes);
                            copy(nb, b[0L..nbytes]);
                            b = nb;

                        }

                        nbytes += utf8.EncodeRune(b[nbytes..maxbytes], r);

                    }

                    i += wid;

                }

            }
            return b[0L..nbytes];

        }

        // Repeat returns a new byte slice consisting of count copies of b.
        //
        // It panics if count is negative or if
        // the result of (len(b) * count) overflows.
        public static slice<byte> Repeat(slice<byte> b, long count) => func((_, panic, __) =>
        {
            if (count == 0L)
            {
                return new slice<byte>(new byte[] {  });
            } 
            // Since we cannot return an error on overflow,
            // we should panic if the repeat will generate
            // an overflow.
            // See Issue golang.org/issue/16237.
            if (count < 0L)
            {
                panic("bytes: negative Repeat count");
            }
            else if (len(b) * count / count != len(b))
            {
                panic("bytes: Repeat count causes overflow");
            }

            var nb = make_slice<byte>(len(b) * count);
            var bp = copy(nb, b);
            while (bp < len(nb))
            {
                copy(nb[bp..], nb[..bp]);
                bp *= 2L;
            }

            return nb;

        });

        // ToUpper returns a copy of the byte slice s with all Unicode letters mapped to
        // their upper case.
        public static slice<byte> ToUpper(slice<byte> s)
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

                    hasLower = hasLower || ('a' <= c && c <= 'z');

                }


                i = i__prev1;
            }

            if (isASCII)
            { // optimize for ASCII-only byte slices.
                if (!hasLower)
                { 
                    // Just return a copy.
                    return append((slice<byte>)"", s);

                }

                var b = make_slice<byte>(len(s));
                {
                    long i__prev1 = i;

                    for (i = 0L; i < len(s); i++)
                    {
                        c = s[i];
                        if ('a' <= c && c <= 'z')
                        {
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
        public static slice<byte> ToLower(slice<byte> s)
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

                    hasUpper = hasUpper || ('A' <= c && c <= 'Z');

                }


                i = i__prev1;
            }

            if (isASCII)
            { // optimize for ASCII-only byte slices.
                if (!hasUpper)
                {
                    return append((slice<byte>)"", s);
                }

                var b = make_slice<byte>(len(s));
                {
                    long i__prev1 = i;

                    for (i = 0L; i < len(s); i++)
                    {
                        c = s[i];
                        if ('A' <= c && c <= 'Z')
                        {
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
        public static slice<byte> ToTitle(slice<byte> s)
        {
            return Map(unicode.ToTitle, s);
        }

        // ToUpperSpecial treats s as UTF-8-encoded bytes and returns a copy with all the Unicode letters mapped to their
        // upper case, giving priority to the special casing rules.
        public static slice<byte> ToUpperSpecial(unicode.SpecialCase c, slice<byte> s)
        {
            return Map(c.ToUpper, s);
        }

        // ToLowerSpecial treats s as UTF-8-encoded bytes and returns a copy with all the Unicode letters mapped to their
        // lower case, giving priority to the special casing rules.
        public static slice<byte> ToLowerSpecial(unicode.SpecialCase c, slice<byte> s)
        {
            return Map(c.ToLower, s);
        }

        // ToTitleSpecial treats s as UTF-8-encoded bytes and returns a copy with all the Unicode letters mapped to their
        // title case, giving priority to the special casing rules.
        public static slice<byte> ToTitleSpecial(unicode.SpecialCase c, slice<byte> s)
        {
            return Map(c.ToTitle, s);
        }

        // ToValidUTF8 treats s as UTF-8-encoded bytes and returns a copy with each run of bytes
        // representing invalid UTF-8 replaced with the bytes in replacement, which may be empty.
        public static slice<byte> ToValidUTF8(slice<byte> s, slice<byte> replacement)
        {
            var b = make_slice<byte>(0L, len(s) + len(replacement));
            var invalid = false; // previous byte was from an invalid UTF-8 sequence
            {
                long i = 0L;

                while (i < len(s))
                {
                    var c = s[i];
                    if (c < utf8.RuneSelf)
                    {
                        i++;
                        invalid = false;
                        b = append(b, byte(c));
                        continue;
                    }

                    var (_, wid) = utf8.DecodeRune(s[i..]);
                    if (wid == 1L)
                    {
                        i++;
                        if (!invalid)
                        {
                            invalid = true;
                            b = append(b, replacement);
                        }

                        continue;

                    }

                    invalid = false;
                    b = append(b, s[i..i + wid]);
                    i += wid;

                }

            }
            return b;

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

        // Title treats s as UTF-8-encoded bytes and returns a copy with all Unicode letters that begin
        // words mapped to their title case.
        //
        // BUG(rsc): The rule Title uses for word boundaries does not handle Unicode punctuation properly.
        public static slice<byte> Title(slice<byte> s)
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

        // TrimLeftFunc treats s as UTF-8-encoded bytes and returns a subslice of s by slicing off
        // all leading UTF-8-encoded code points c that satisfy f(c).
        public static slice<byte> TrimLeftFunc(slice<byte> s, Func<int, bool> f)
        {
            var i = indexFunc(s, f, false);
            if (i == -1L)
            {
                return null;
            }

            return s[i..];

        }

        // TrimRightFunc returns a subslice of s by slicing off all trailing
        // UTF-8-encoded code points c that satisfy f(c).
        public static slice<byte> TrimRightFunc(slice<byte> s, Func<int, bool> f)
        {
            var i = lastIndexFunc(s, f, false);
            if (i >= 0L && s[i] >= utf8.RuneSelf)
            {
                var (_, wid) = utf8.DecodeRune(s[i..]);
                i += wid;
            }
            else
            {
                i++;
            }

            return s[0L..i];

        }

        // TrimFunc returns a subslice of s by slicing off all leading and trailing
        // UTF-8-encoded code points c that satisfy f(c).
        public static slice<byte> TrimFunc(slice<byte> s, Func<int, bool> f)
        {
            return TrimRightFunc(TrimLeftFunc(s, f), f);
        }

        // TrimPrefix returns s without the provided leading prefix string.
        // If s doesn't start with prefix, s is returned unchanged.
        public static slice<byte> TrimPrefix(slice<byte> s, slice<byte> prefix)
        {
            if (HasPrefix(s, prefix))
            {
                return s[len(prefix)..];
            }

            return s;

        }

        // TrimSuffix returns s without the provided trailing suffix string.
        // If s doesn't end with suffix, s is returned unchanged.
        public static slice<byte> TrimSuffix(slice<byte> s, slice<byte> suffix)
        {
            if (HasSuffix(s, suffix))
            {
                return s[..len(s) - len(suffix)];
            }

            return s;

        }

        // IndexFunc interprets s as a sequence of UTF-8-encoded code points.
        // It returns the byte index in s of the first Unicode
        // code point satisfying f(c), or -1 if none do.
        public static long IndexFunc(slice<byte> s, Func<int, bool> f)
        {
            return indexFunc(s, f, true);
        }

        // LastIndexFunc interprets s as a sequence of UTF-8-encoded code points.
        // It returns the byte index in s of the last Unicode
        // code point satisfying f(c), or -1 if none do.
        public static long LastIndexFunc(slice<byte> s, Func<int, bool> f)
        {
            return lastIndexFunc(s, f, true);
        }

        // indexFunc is the same as IndexFunc except that if
        // truth==false, the sense of the predicate function is
        // inverted.
        private static long indexFunc(slice<byte> s, Func<int, bool> f, bool truth)
        {
            long start = 0L;
            while (start < len(s))
            {
                long wid = 1L;
                var r = rune(s[start]);
                if (r >= utf8.RuneSelf)
                {
                    r, wid = utf8.DecodeRune(s[start..]);
                }

                if (f(r) == truth)
                {
                    return start;
                }

                start += wid;

            }

            return -1L;

        }

        // lastIndexFunc is the same as LastIndexFunc except that if
        // truth==false, the sense of the predicate function is
        // inverted.
        private static long lastIndexFunc(slice<byte> s, Func<int, bool> f, bool truth)
        {
            {
                var i = len(s);

                while (i > 0L)
                {
                    var r = rune(s[i - 1L]);
                    long size = 1L;
                    if (r >= utf8.RuneSelf)
                    {
                        r, size = utf8.DecodeLastRune(s[0L..i]);
                    }

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
            asciiSet @as = default;
            bool ok = default;

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
        private static bool contains(this ptr<asciiSet> _addr_@as, byte c)
        {
            ref asciiSet @as = ref _addr_@as.val;

            return (as[c >> (int)(5L)] & (1L << (int)(uint(c & 31L)))) != 0L;
        }

        private static Func<int, bool> makeCutsetFunc(@string cutset)
        {
            if (len(cutset) == 1L && cutset[0L] < utf8.RuneSelf)
            {
                return r =>
                {
                    return r == rune(cutset[0L]);
                };

            }

            {
                var (as, isASCII) = makeASCIISet(cutset);

                if (isASCII)
                {
                    return r =>
                    {
                        return r < utf8.RuneSelf && @as.contains(byte(r));
                    };

                }

            }

            return r =>
            {
                foreach (var (_, c) in cutset)
                {
                    if (c == r)
                    {
                        return true;
                    }

                }
                return false;

            };

        }

        // Trim returns a subslice of s by slicing off all leading and
        // trailing UTF-8-encoded code points contained in cutset.
        public static slice<byte> Trim(slice<byte> s, @string cutset)
        {
            return TrimFunc(s, makeCutsetFunc(cutset));
        }

        // TrimLeft returns a subslice of s by slicing off all leading
        // UTF-8-encoded code points contained in cutset.
        public static slice<byte> TrimLeft(slice<byte> s, @string cutset)
        {
            return TrimLeftFunc(s, makeCutsetFunc(cutset));
        }

        // TrimRight returns a subslice of s by slicing off all trailing
        // UTF-8-encoded code points that are contained in cutset.
        public static slice<byte> TrimRight(slice<byte> s, @string cutset)
        {
            return TrimRightFunc(s, makeCutsetFunc(cutset));
        }

        // TrimSpace returns a subslice of s by slicing off all leading and
        // trailing white space, as defined by Unicode.
        public static slice<byte> TrimSpace(slice<byte> s)
        { 
            // Fast path for ASCII: look for the first ASCII non-space byte
            long start = 0L;
            while (start < len(s))
            {
                var c = s[start];
                if (c >= utf8.RuneSelf)
                { 
                    // If we run into a non-ASCII byte, fall back to the
                    // slower unicode-aware method on the remaining bytes
                    return TrimFunc(s[start..], unicode.IsSpace);
                start++;
                }

                if (asciiSpace[c] == 0L)
                {
                    break;
                }

            } 

            // Now look for the first ASCII non-space byte from the end
 

            // Now look for the first ASCII non-space byte from the end
            var stop = len(s);
            while (stop > start)
            {
                c = s[stop - 1L];
                if (c >= utf8.RuneSelf)
                {
                    return TrimFunc(s[start..stop], unicode.IsSpace);
                stop--;
                }

                if (asciiSpace[c] == 0L)
                {
                    break;
                }

            } 

            // At this point s[start:stop] starts and ends with an ASCII
            // non-space bytes, so we're done. Non-ASCII cases have already
            // been handled above.
 

            // At this point s[start:stop] starts and ends with an ASCII
            // non-space bytes, so we're done. Non-ASCII cases have already
            // been handled above.
            if (start == stop)
            { 
                // Special case to preserve previous TrimLeftFunc behavior,
                // returning nil instead of empty slice if all spaces.
                return null;

            }

            return s[start..stop];

        }

        // Runes interprets s as a sequence of UTF-8-encoded code points.
        // It returns a slice of runes (Unicode code points) equivalent to s.
        public static slice<int> Runes(slice<byte> s)
        {
            var t = make_slice<int>(utf8.RuneCount(s));
            long i = 0L;
            while (len(s) > 0L)
            {
                var (r, l) = utf8.DecodeRune(s);
                t[i] = r;
                i++;
                s = s[l..];
            }

            return t;

        }

        // Replace returns a copy of the slice s with the first n
        // non-overlapping instances of old replaced by new.
        // If old is empty, it matches at the beginning of the slice
        // and after each UTF-8 sequence, yielding up to k+1 replacements
        // for a k-rune slice.
        // If n < 0, there is no limit on the number of replacements.
        public static slice<byte> Replace(slice<byte> s, slice<byte> old, slice<byte> @new, long n)
        {
            long m = 0L;
            if (n != 0L)
            { 
                // Compute number of replacements.
                m = Count(s, old);

            }

            if (m == 0L)
            { 
                // Just return a copy.
                return append((slice<byte>)null, s);

            }

            if (n < 0L || m < n)
            {
                n = m;
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
                        var (_, wid) = utf8.DecodeRune(s[start..]);
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
            return t[0L..w];

        }

        // ReplaceAll returns a copy of the slice s with all
        // non-overlapping instances of old replaced by new.
        // If old is empty, it matches at the beginning of the slice
        // and after each UTF-8 sequence, yielding up to k+1 replacements
        // for a k-rune slice.
        public static slice<byte> ReplaceAll(slice<byte> s, slice<byte> old, slice<byte> @new)
        {
            return Replace(s, old, new, -1L);
        }

        // EqualFold reports whether s and t, interpreted as UTF-8 strings,
        // are equal under Unicode case-folding, which is a more general
        // form of case-insensitivity.
        public static bool EqualFold(slice<byte> s, slice<byte> t)
        {
            while (len(s) != 0L && len(t) != 0L)
            { 
                // Extract first rune from each.
                int sr = default;                int tr = default;

                if (s[0L] < utf8.RuneSelf)
                {
                    sr = rune(s[0L]);
                    s = s[1L..];

                }
                else
                {
                    var (r, size) = utf8.DecodeRune(s);
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
                    (r, size) = utf8.DecodeRune(t);
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
                if (tr < utf8.RuneSelf)
                { 
                    // ASCII only, sr/tr must be upper/lower case
                    if ('A' <= sr && sr <= 'Z' && tr == sr + 'a' - 'A')
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
            return len(s) == len(t);

        }

        // Index returns the index of the first instance of sep in s, or -1 if sep is not present in s.
        public static long Index(slice<byte> s, slice<byte> sep)
        {
            var n = len(sep);

            if (n == 0L) 
                return 0L;
            else if (n == 1L) 
                return IndexByte(s, sep[0L]);
            else if (n == len(s)) 
                if (Equal(sep, s))
                {
                    return 0L;
                }

                return -1L;
            else if (n > len(s)) 
                return -1L;
            else if (n <= bytealg.MaxLen) 
                // Use brute force when s and sep both are small
                if (len(s) <= bytealg.MaxBruteForce)
                {
                    return bytealg.Index(s, sep);
                }

                var c0 = sep[0L];
                var c1 = sep[1L];
                long i = 0L;
                var t = len(s) - n + 1L;
                long fails = 0L;
                while (i < t)
                {
                    if (s[i] != c0)
                    { 
                        // IndexByte is faster than bytealg.Index, so use it as long as
                        // we're not getting lots of false positives.
                        var o = IndexByte(s[i + 1L..t], c0);
                        if (o < 0L)
                        {
                            return -1L;
                        }

                        i += o + 1L;

                    }

                    if (s[i + 1L] == c1 && Equal(s[i..i + n], sep))
                    {
                        return i;
                    }

                    fails++;
                    i++; 
                    // Switch to bytealg.Index when IndexByte produces too many false positives.
                    if (fails > bytealg.Cutover(i))
                    {
                        var r = bytealg.Index(s[i..], sep);
                        if (r >= 0L)
                        {
                            return r + i;
                        }

                        return -1L;

                    }

                }

                return -1L;
                        c0 = sep[0L];
            c1 = sep[1L];
            i = 0L;
            fails = 0L;
            t = len(s) - n + 1L;
            while (i < t)
            {
                if (s[i] != c0)
                {
                    o = IndexByte(s[i + 1L..t], c0);
                    if (o < 0L)
                    {
                        break;
                    }

                    i += o + 1L;

                }

                if (s[i + 1L] == c1 && Equal(s[i..i + n], sep))
                {
                    return i;
                }

                i++;
                fails++;
                if (fails >= 4L + i >> (int)(4L) && i < t)
                { 
                    // Give up on IndexByte, it isn't skipping ahead
                    // far enough to be better than Rabin-Karp.
                    // Experiments (using IndexPeriodic) suggest
                    // the cutover is about 16 byte skips.
                    // TODO: if large prefixes of sep are matching
                    // we should cutover at even larger average skips,
                    // because Equal becomes that much more expensive.
                    // This code does not take that effect into account.
                    var j = bytealg.IndexRabinKarpBytes(s[i..], sep);
                    if (j < 0L)
                    {
                        return -1L;
                    }

                    return i + j;

                }

            }

            return -1L;

        }
    }
}
