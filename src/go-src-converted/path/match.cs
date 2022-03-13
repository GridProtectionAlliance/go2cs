// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package path -- go2cs converted at 2022 March 13 05:23:50 UTC
// import "path" ==> using path = go.path_package
// Original source: C:\Program Files\Go\src\path\match.go
namespace go;

using errors = errors_package;
using bytealg = @internal.bytealg_package;
using utf8 = unicode.utf8_package;


// ErrBadPattern indicates a pattern was malformed.

public static partial class path_package {

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
public static (bool, error) Match(@string pattern, @string name) {
    bool matched = default;
    error err = default!;

Pattern:
    while (len(pattern) > 0) {
        bool star = default;
        @string chunk = default;
        star, chunk, pattern = scanChunk(pattern);
        if (star && chunk == "") { 
            // Trailing * matches rest of string unless it has a /.
            return (bytealg.IndexByteString(name, '/') < 0, error.As(null!)!);
        }
        var (t, ok, err) = matchChunk(chunk, name); 
        // if we're the last chunk, make sure we've exhausted the name
        // otherwise we'll give a false result even if we could still match
        // using the star
        if (ok && (len(t) == 0 || len(pattern) > 0)) {
            name = t;
            continue;
        }
        if (err != null) {
            return (false, error.As(err)!);
        }
        if (star) { 
            // Look for match skipping i+1 bytes.
            // Cannot skip /.
            for (nint i = 0; i < len(name) && name[i] != '/'; i++) {
                (t, ok, err) = matchChunk(chunk, name[(int)i + 1..]);
                if (ok) { 
                    // if we're the last chunk, make sure we exhausted the name
                    if (len(pattern) == 0 && len(t) > 0) {
                        continue;
                    }
                    name = t;
                    _continuePattern = true;
                    break;
                }
                if (err != null) {
                    return (false, error.As(err)!);
                }
            }
        }
        while (len(pattern) > 0) {
            _, chunk, pattern = scanChunk(pattern);
            {
                var (_, _, err) = matchChunk(chunk, "");

                if (err != null) {
                    return (false, error.As(err)!);
                }

            }
        }
        return (false, error.As(null!)!);
    }
    return (len(name) == 0, error.As(null!)!);
}

// scanChunk gets the next segment of pattern, which is a non-star string
// possibly preceded by a star.
private static (bool, @string, @string) scanChunk(@string pattern) {
    bool star = default;
    @string chunk = default;
    @string rest = default;

    while (len(pattern) > 0 && pattern[0] == '*') {
        pattern = pattern[(int)1..];
        star = true;
    }
    var inrange = false;
    nint i = default;
Scan:
    for (i = 0; i < len(pattern); i++) {
        switch (pattern[i]) {
            case '\\': 
                // error check handled in matchChunk: bad pattern.
                if (i + 1 < len(pattern)) {
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
                if (!inrange) {
                    _breakScan = true;
                    break;
                }
                break;
        }
    }
    return (star, pattern[(int)0..(int)i], pattern[(int)i..]);
}

// matchChunk checks whether chunk matches the beginning of s.
// If so, it returns the remainder of s (after the match).
// Chunk is all single-character operators: literals, char classes, and ?.
private static (@string, bool, error) matchChunk(@string chunk, @string s) {
    @string rest = default;
    bool ok = default;
    error err = default!;
 
    // failed records whether the match has failed.
    // After the match fails, the loop continues on processing chunk,
    // checking that the pattern is well-formed but no longer reading s.
    var failed = false;
    while (len(chunk) > 0) {
        if (!failed && len(s) == 0) {
            failed = true;
        }

        if (chunk[0] == '[') 
        {
            // character class
            int r = default;
            if (!failed) {
                nint n = default;
                r, n = utf8.DecodeRuneInString(s);
                s = s[(int)n..];
            }
            chunk = chunk[(int)1..]; 
            // possibly negated
            var negated = false;
            if (len(chunk) > 0 && chunk[0] == '^') {
                negated = true;
                chunk = chunk[(int)1..];
            } 
            // parse all ranges
            var match = false;
            nint nrange = 0;
            while (true) {
                if (len(chunk) > 0 && chunk[0] == ']' && nrange > 0) {
                    chunk = chunk[(int)1..];
                    break;
                }
                int lo = default;                int hi = default;

                lo, chunk, err = getEsc(chunk);

                if (err != null) {
                    return ("", false, error.As(err)!);
                }
                hi = lo;
                if (chunk[0] == '-') {
                    hi, chunk, err = getEsc(chunk[(int)1..]);

                    if (err != null) {
                        return ("", false, error.As(err)!);
                    }
                }
                if (lo <= r && r <= hi) {
                    match = true;
                }
                nrange++;
            }

            if (match == negated) {
                failed = true;
            }
            goto __switch_break0;
        }
        if (chunk[0] == '?')
        {
            if (!failed) {
                if (s[0] == '/') {
                    failed = true;
                }
                var (_, n) = utf8.DecodeRuneInString(s);
                s = s[(int)n..];
            }
            chunk = chunk[(int)1..];
            goto __switch_break0;
        }
        if (chunk[0] == '\\')
        {
            chunk = chunk[(int)1..];
            if (len(chunk) == 0) {
                return ("", false, error.As(ErrBadPattern)!);
            }
        }
        // default: 
            if (!failed) {
                if (chunk[0] != s[0]) {
                    failed = true;
                }
                s = s[(int)1..];
            }
            chunk = chunk[(int)1..];

        __switch_break0:;
    }
    if (failed) {
        return ("", false, error.As(null!)!);
    }
    return (s, true, error.As(null!)!);
}

// getEsc gets a possibly-escaped character from chunk, for a character class.
private static (int, @string, error) getEsc(@string chunk) {
    int r = default;
    @string nchunk = default;
    error err = default!;

    if (len(chunk) == 0 || chunk[0] == '-' || chunk[0] == ']') {
        err = ErrBadPattern;
        return ;
    }
    if (chunk[0] == '\\') {
        chunk = chunk[(int)1..];
        if (len(chunk) == 0) {
            err = ErrBadPattern;
            return ;
        }
    }
    var (r, n) = utf8.DecodeRuneInString(chunk);
    if (r == utf8.RuneError && n == 1) {
        err = ErrBadPattern;
    }
    nchunk = chunk[(int)n..];
    if (len(nchunk) == 0) {
        err = ErrBadPattern;
    }
    return ;
}

} // end path_package
