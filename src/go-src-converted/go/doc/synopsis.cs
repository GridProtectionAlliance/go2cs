// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package doc -- go2cs converted at 2020 October 08 04:02:50 UTC
// import "go/doc" ==> using doc = go.go.doc_package
// Original source: C:\Go\src\go\doc\synopsis.go
using strings = go.strings_package;
using unicode = go.unicode_package;
using static go.builtin;

namespace go {
namespace go
{
    public static partial class doc_package
    {
        // firstSentenceLen returns the length of the first sentence in s.
        // The sentence ends after the first period followed by space and
        // not preceded by exactly one uppercase letter.
        //
        private static long firstSentenceLen(@string s)
        {
            int ppp = default;            int pp = default;            int p = default;

            foreach (var (i, q) in s)
            {
                if (q == '\n' || q == '\r' || q == '\t')
                {
                    q = ' ';
                }
                if (q == ' ' && p == '.' && (!unicode.IsUpper(pp) || unicode.IsUpper(ppp)))
                {
                    return i;
                }
                if (p == '。' || p == '．')
                {
                    return i;
                }
                ppp = pp;
                pp = p;
                p = q;

            }            return len(s);

        }

        private static readonly long keepNL = (long)1L << (int)(iota);


        // clean replaces each sequence of space, \n, \r, or \t characters
        // with a single space and removes any trailing and leading spaces.
        // If the keepNL flag is set, newline characters are passed through
        // instead of being change to spaces.
        private static @string clean(@string s, long flags)
        {
            slice<byte> b = default;
            var p = byte(' ');
            for (long i = 0L; i < len(s); i++)
            {
                var q = s[i];
                if ((flags & keepNL) == 0L && q == '\n' || q == '\r' || q == '\t')
                {
                    q = ' ';
                }

                if (q != ' ' || p != ' ')
                {
                    b = append(b, q);
                    p = q;
                }

            } 
            // remove trailing blank, if any
 
            // remove trailing blank, if any
            {
                var n = len(b);

                if (n > 0L && p == ' ')
                {
                    b = b[0L..n - 1L];
                }

            }

            return string(b);

        }

        // Synopsis returns a cleaned version of the first sentence in s.
        // That sentence ends after the first period followed by space and
        // not preceded by exactly one uppercase letter. The result string
        // has no \n, \r, or \t characters and uses only single spaces between
        // words. If s starts with any of the IllegalPrefixes, the result
        // is the empty string.
        //
        public static @string Synopsis(@string s)
        {
            s = clean(s[0L..firstSentenceLen(s)], 0L);
            foreach (var (_, prefix) in IllegalPrefixes)
            {
                if (strings.HasPrefix(strings.ToLower(s), prefix))
                {
                    return "";
                }

            }
            s = convertQuotes(s);
            return s;

        }

        public static @string IllegalPrefixes = new slice<@string>(new @string[] { "copyright", "all rights", "author" });
    }
}}
