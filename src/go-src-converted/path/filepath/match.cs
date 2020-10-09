// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package filepath -- go2cs converted at 2020 October 09 04:49:45 UTC
// import "path/filepath" ==> using filepath = go.path.filepath_package
// Original source: C:\Go\src\path\filepath\match.go
using errors = go.errors_package;
using os = go.os_package;
using runtime = go.runtime_package;
using sort = go.sort_package;
using strings = go.strings_package;
using utf8 = go.unicode.utf8_package;
using static go.builtin;

namespace go {
namespace path
{
    public static partial class filepath_package
    {
        // ErrBadPattern indicates a pattern was malformed.
        public static var ErrBadPattern = errors.New("syntax error in pattern");

        // Match reports whether name matches the shell file name pattern.
        // The pattern syntax is:
        //
        //    pattern:
        //        { term }
        //    term:
        //        '*'         matches any sequence of non-Separator characters
        //        '?'         matches any single non-Separator character
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
        // On Windows, escaping is disabled. Instead, '\\' is treated as
        // path separator.
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
                    return (!strings.Contains(name, string(Separator)), error.As(null!)!);

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
                    for (long i = 0L; i < len(name) && name[i] != Separator; i++)
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
                        if (runtime.GOOS != "windows")
                        { 
                            // error check handled in matchChunk: bad pattern.
                            if (i + 1L < len(pattern))
                            {
                                i++;
                            }

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
                    // We can't end right after '[', we're expecting at least
                    // a closing bracket and possibly a caret.
                    if (len(chunk) == 0L)
                    {
                        err = ErrBadPattern;
                        return ;
                    } 
                    // possibly negated
                    var negated = chunk[0L] == '^';
                    if (negated)
                    {
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

                    if (match == negated)
                    {
                        return ;
                    }

                    goto __switch_break0;
                }
                if (chunk[0L] == '?')
                {
                    if (s[0L] == Separator)
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
                    if (runtime.GOOS != "windows")
                    {
                        chunk = chunk[1L..];
                        if (len(chunk) == 0L)
                        {
                            err = ErrBadPattern;
                            return ;
                        }

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

            if (chunk[0L] == '\\' && runtime.GOOS != "windows")
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

        // Glob returns the names of all files matching pattern or nil
        // if there is no matching file. The syntax of patterns is the same
        // as in Match. The pattern may describe hierarchical names such as
        // /usr/*/bin/ed (assuming the Separator is '/').
        //
        // Glob ignores file system errors such as I/O errors reading directories.
        // The only possible returned error is ErrBadPattern, when pattern
        // is malformed.
        public static (slice<@string>, error) Glob(@string pattern)
        {
            slice<@string> matches = default;
            error err = default!;

            if (!hasMeta(pattern))
            {
                _, err = os.Lstat(pattern);

                if (err != null)
                {
                    return (null, error.As(null!)!);
                }

                return (new slice<@string>(new @string[] { pattern }), error.As(null!)!);

            }

            var (dir, file) = Split(pattern);
            long volumeLen = 0L;
            if (runtime.GOOS == "windows")
            {
                volumeLen, dir = cleanGlobPathWindows(dir);
            }
            else
            {
                dir = cleanGlobPath(dir);
            }

            if (!hasMeta(dir[volumeLen..]))
            {
                return glob(dir, file, null);
            } 

            // Prevent infinite recursion. See issue 15879.
            if (dir == pattern)
            {
                return (null, error.As(ErrBadPattern)!);
            }

            slice<@string> m = default;
            m, err = Glob(dir);
            if (err != null)
            {
                return ;
            }

            foreach (var (_, d) in m)
            {
                matches, err = glob(d, file, matches);
                if (err != null)
                {
                    return ;
                }

            }
            return ;

        }

        // cleanGlobPath prepares path for glob matching.
        private static @string cleanGlobPath(@string path)
        {

            if (path == "") 
                return ".";
            else if (path == string(Separator)) 
                // do nothing to the path
                return path;
            else 
                return path[0L..len(path) - 1L]; // chop off trailing separator
                    }

        // cleanGlobPathWindows is windows version of cleanGlobPath.
        private static (long, @string) cleanGlobPathWindows(@string path)
        {
            long prefixLen = default;
            @string cleaned = default;

            var vollen = volumeNameLen(path);

            if (path == "") 
                return (0L, ".");
            else if (vollen + 1L == len(path) && os.IsPathSeparator(path[len(path) - 1L])) // /, \, C:\ and C:/
                // do nothing to the path
                return (vollen + 1L, path);
            else if (vollen == len(path) && len(path) == 2L) // C:
                return (vollen, path + "."); // convert C: into C:.
            else 
                if (vollen >= len(path))
                {
                    vollen = len(path) - 1L;
                }

                return (vollen, path[0L..len(path) - 1L]); // chop off trailing separator
                    }

        // glob searches for files matching pattern in the directory dir
        // and appends them to matches. If the directory cannot be
        // opened, it returns the existing matches. New matches are
        // added in lexicographical order.
        private static (slice<@string>, error) glob(@string dir, @string pattern, slice<@string> matches) => func((defer, _, __) =>
        {
            slice<@string> m = default;
            error e = default!;

            m = matches;
            var (fi, err) = os.Stat(dir);
            if (err != null)
            {
                return ; // ignore I/O error
            }

            if (!fi.IsDir())
            {
                return ; // ignore I/O error
            }

            var (d, err) = os.Open(dir);
            if (err != null)
            {
                return ; // ignore I/O error
            }

            defer(d.Close());

            var (names, _) = d.Readdirnames(-1L);
            sort.Strings(names);

            foreach (var (_, n) in names)
            {
                var (matched, err) = Match(pattern, n);
                if (err != null)
                {
                    return (m, error.As(err)!);
                }

                if (matched)
                {
                    m = append(m, Join(dir, n));
                }

            }
            return ;

        });

        // hasMeta reports whether path contains any of the magic characters
        // recognized by Match.
        private static bool hasMeta(@string path)
        {
            @string magicChars = "*?[";
            if (runtime.GOOS != "windows")
            {
                magicChars = "*?[\\";
            }

            return strings.ContainsAny(path, magicChars);

        }
    }
}}
