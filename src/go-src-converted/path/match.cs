// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package path -- go2cs converted at 2020 October 08 00:33:48 UTC
// import "path" ==> using path = go.path_package
// Original source: C:\Go\src\path\match.go
using errors = go.errors_package;
using strings = go.strings_package;
using utf8 = go.unicode.utf8_package;
using static go.builtin;

namespace go
{
    public static partial class path_package
    {
        // ErrBadPattern indicates a pattern was malformed.
        public static var ErrBadPattern = errors.New("syntax error in pattern");

        // Match reports whether name matches the shell pattern.
        // The pattern syntax is:
        //
        //    pattern:
        //        { term }
        //    term:
        //        '*'         matches any sequence of non-/ characters
        //        '?'         matches any single non-/ character
        //        '[' [ '^' ] { character-range } ']'
        //                    character class (must be non-empty)
        //        c           matches character c (c != '*', '?', '\\', '[')
        //        '\\' c      matches character c
        //
        //    character-range:
        //        c           matches character c (c != '\\', '-', ']')
        //        '\\' c      matches character c
        //        lo '-' hi   matches character c for lo <= c <= hi
        //
        // Match requires pattern to match all of name, not just a substring.
        // The only possible returned error is ErrBadPattern, when pattern
        // is malformed.
        //
        public static (bool, error) Match(@string pattern, @string name)
        {
            bool matched = default;
            error err = default!;

Pattern:
            while (len(pattern) > 0L)
            {
                bool star = default;
                @string chunk = default;
                star, chunk, pattern = scanChunk(pattern);
                if (star && chunk == "")
                { 
                    // Trailing * matches rest of string unless it has a /.
                    return (!strings.Contains(name, "/"), error.As(null!)!);

                } 
                // Look for match at current position.
                var (t, ok, err) = matchChunk(chunk, name); 
                // if we're the last chunk, make sure we've exhausted the name
                // otherwise we'll give a false result even if we could still match
                // using the star
                if (ok && (len(t) == 0L || len(pattern) > 0L))
                {
                    name = t;
                    continue;
                }

                if (err != null)
                {
                    return (false, error.As(err)!);
                }

                if (star)
                { 
                    // Look for match skipping i+1 bytes.
                    // Cannot skip /.
                    for (long i = 0L; i < len(name) && name[i] != '/'; i++)
                    {
                        (t, ok, err) = matchChunk(chunk, name[i + 1L..]);
                        if (ok)
                        { 
                            // if we're the last chunk, make sure we exhausted the name
                            if (len(pattern) == 0L && len(t) > 0L)
                            {
                                continue;
                            }

                            name = t;
                            _continuePattern = true;
                            break;
                        }

                        if (err != null)
                        {
                            return (false, error.As(err)!);
                        }

                    }


                }

                return (false, error.As(null!)!);

            }
            return (len(name) == 0L, error.As(null!)!);

        }

        // scanChunk gets the next segment of pattern, which is a non-star string
        // possibly preceded by a star.
        private static (bool, @string, @string) scanChunk(@string pattern)
        {
            bool star = default;
            @string chunk = default;
            @string rest = default;

            while (len(pattern) > 0L && pattern[0L] == '*')
            {
                pattern = pattern[1L..];
                star = true;
            }

            var inrange = false;
            long i = default;
Scan:
            for (i = 0L; i < len(pattern); i++)
            {
                switch (pattern[i])
                {
                    case '\\': 
                        // error check handled in matchChunk: bad pattern.
                        if (i + 1L < len(pattern))
                        {
                            i++;
                        }

                        break;
                    case '[': 
                        inrange = true;
                        break;
                    case ']': 
                        inrange = false;
                        break;
                    case '*': 
                        if (!inrange)
                        {
                            _breakScan = true;
                            break;
                        }

                        break;
                }

            }
            return (star, pattern[0L..i], pattern[i..]);

        }

        // matchChunk checks whether chunk matches the beginning of s.
        // If so, it returns the remainder of s (after the match).
        // Chunk is all single-character operators: literals, char classes, and ?.
        private static (@string, bool, error) matchChunk(@string chunk, @string s)
        {
            @string rest = default;
            bool ok = default;
            error err = default!;

            while (len(chunk) > 0L)
            {
                if (len(s) == 0L)
                {
                    return ;
                }


                if (chunk[0L] == '[') 
                {
                    // character class
                    var (r, n) = utf8.DecodeRuneInString(s);
                    s = s[n..];
                    chunk = chunk[1L..]; 
                    // possibly negated
                    var notNegated = true;
                    if (len(chunk) > 0L && chunk[0L] == '^')
                    {
                        notNegated = false;
                        chunk = chunk[1L..];
                    } 
                    // parse all ranges
                    var match = false;
                    long nrange = 0L;
                    while (true)
                    {
                        if (len(chunk) > 0L && chunk[0L] == ']' && nrange > 0L)
                        {
                            chunk = chunk[1L..];
                            break;
                        }

                        int lo = default;                        int hi = default;

                        lo, chunk, err = getEsc(chunk);

                        if (err != null)
                        {
                            return ;
                        }

                        hi = lo;
                        if (chunk[0L] == '-')
                        {
                            hi, chunk, err = getEsc(chunk[1L..]);

                            if (err != null)
                            {
                                return ;
                            }

                        }

                        if (lo <= r && r <= hi)
                        {
                            match = true;
                        }

                        nrange++;

                    }

                    if (match != notNegated)
                    {
                        return ;
                    }

                    goto __switch_break0;
                }
                if (chunk[0L] == '?')
                {
                    if (s[0L] == '/')
                    {
                        return ;
                    }

                    var (_, n) = utf8.DecodeRuneInString(s);
                    s = s[n..];
                    chunk = chunk[1L..];
                    goto __switch_break0;
                }
                if (chunk[0L] == '\\')
                {
                    chunk = chunk[1L..];
                    if (len(chunk) == 0L)
                    {
                        err = ErrBadPattern;
                        return ;
                    }

                }
                // default: 
                    if (chunk[0L] != s[0L])
                    {
                        return ;
                    }

                    s = s[1L..];
                    chunk = chunk[1L..];

                __switch_break0:;

            }

            return (s, true, error.As(null!)!);

        }

        // getEsc gets a possibly-escaped character from chunk, for a character class.
        private static (int, @string, error) getEsc(@string chunk)
        {
            int r = default;
            @string nchunk = default;
            error err = default!;

            if (len(chunk) == 0L || chunk[0L] == '-' || chunk[0L] == ']')
            {
                err = ErrBadPattern;
                return ;
            }

            if (chunk[0L] == '\\')
            {
                chunk = chunk[1L..];
                if (len(chunk) == 0L)
                {
                    err = ErrBadPattern;
                    return ;
                }

            }

            var (r, n) = utf8.DecodeRuneInString(chunk);
            if (r == utf8.RuneError && n == 1L)
            {
                err = ErrBadPattern;
            }

            nchunk = chunk[n..];
            if (len(nchunk) == 0L)
            {
                err = ErrBadPattern;
            }

            return ;

        }
    }
}
