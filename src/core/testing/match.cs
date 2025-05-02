// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using fmt = fmt_package;
using os = os_package;
using strconv = strconv_package;
using strings = strings_package;
using sync = sync_package;

partial class testing_package {

// matcher sanitizes, uniques, and filters names of subtests and subbenchmarks.
[GoType] partial struct matcher {
    internal filterMatch filter;
    internal filterMatch skip;
    internal Func<@string, @string, (bool, error)> matchFunc;
    internal sync_package.Mutex mu;
    // subNames is used to deduplicate subtest names.
    // Each key is the subtest name joined to the deduplicated name of the parent test.
    // Each value is the count of the number of occurrences of the given subtest name
    // already seen.
    internal map<@string, int32> subNames;
}

[GoType] partial interface filterMatch {
    // matches checks the name against the receiver's pattern strings using the
    // given match function.
    (bool ok, bool partial) matches(slice<@string> name, Func<@string, @string, (bool, error)> matchString);
    // verify checks that the receiver's pattern strings are valid filters by
    // calling the given match function.
    error verify(@string name, Func<@string, @string, (bool, error)> matchString);
}

[GoType("[]@string")] partial struct simpleMatch;

[GoType("[]filterMatch")] partial struct alternationMatch;

// TODO: fix test_main to avoid race and improve caching, also allowing to
// eliminate this Mutex.
internal static sync.Mutex matchMutex;

internal static ж<matcher> allMatcher() {
    return newMatcher(default!, ""u8, ""u8, ""u8);
}

internal static ж<matcher> newMatcher(Func<@string, @string, (bool, error)> matchString, @string patterns, @string name, @string skips) {
    filterMatch filter = default!;
    filterMatch skip = default!;
    if (patterns == ""u8){
        filter = new simpleMatch{nil};
    } else {
        // always partial true
        filter = splitRegexp(patterns);
        {
            var err = filter.verify(name, matchString); if (err != default!) {
                fmt.Fprintf(~os.Stderr, "testing: invalid regexp for %s\n"u8, err);
                os.Exit(1);
            }
        }
    }
    if (skips == ""u8){
        skip = new alternationMatch{nil};
    } else {
        // always false
        skip = splitRegexp(skips);
        {
            var err = skip.verify("-test.skip"u8, matchString); if (err != default!) {
                fmt.Fprintf(~os.Stderr, "testing: invalid regexp for %v\n"u8, err);
                os.Exit(1);
            }
        }
    }
    return Ꮡ(new matcher(
        filter: filter,
        skip: skip,
        matchFunc: matchString,
        subNames: new map<@string, int32>{}
    ));
}

[GoRecv] internal static (@string name, bool ok, bool partial) fullName(this ref matcher m, ж<common> Ꮡc, @string subname) => func((defer, _) => {
    @string name = default!;
    bool ok = default!;
    bool partial = default!;

    ref var c = ref Ꮡc.val;
    name = subname;
    m.mu.Lock();
    defer(m.mu.Unlock);
    if (c != nil && c.level > 0) {
        name = m.unique(c.name, rewrite(subname));
    }
    matchMutex.Lock();
    var matchMutexʗ1 = matchMutex;
    defer(matchMutexʗ1.Unlock);
    // We check the full array of paths each time to allow for the case that a pattern contains a '/'.
    var elem = strings.Split(name, "/"u8);
    // filter must match.
    // accept partial match that may produce full match later.
    (ok, partial) = m.filter.matches(elem, m.matchFunc);
    if (!ok) {
        return (name, false, false);
    }
    // skip must not match.
    // ignore partial match so we can get to more precise match later.
    var (skip, partialSkip) = m.skip.matches(elem, m.matchFunc);
    if (skip && !partialSkip) {
        return (name, false, false);
    }
    return (name, ok, partial);
});

// clearSubNames clears the matcher's internal state, potentially freeing
// memory. After this is called, T.Name may return the same strings as it did
// for earlier subtests.
[GoRecv] internal static void clearSubNames(this ref matcher m) => func((defer, _) => {
    m.mu.Lock();
    defer(m.mu.Unlock);
    clear(m.subNames);
});

internal static (bool ok, bool partial) matches(this simpleMatch m, slice<@string> name, Func<@string, @string, (bool, error)> matchString) {
    bool ok = default!;
    bool partial = default!;

    foreach (var (i, s) in name) {
        if (i >= len(m)) {
            break;
        }
        {
            var (okΔ1, _) = matchString(m[i], s); if (!okΔ1) {
                return (false, false);
            }
        }
    }
    return (true, len(name) < len(m));
}

internal static error verify(this simpleMatch m, @string name, Func<@string, @string, (bool, error)> matchString) {
    foreach (var (i, s) in m) {
        m[i] = rewrite(s);
    }
    // Verify filters before doing any processing.
    foreach (var (i, s) in m) {
        {
            var (_, err) = matchString(s, "non-empty"u8); if (err != default!) {
                return fmt.Errorf("element %d of %s (%q): %s"u8, i, name, s, err);
            }
        }
    }
    return default!;
}

internal static (bool ok, bool partial) matches(this alternationMatch m, slice<@string> name, Func<@string, @string, (bool, error)> matchString) {
    bool ok = default!;
    bool partial = default!;

    foreach (var (_, mΔ1) in m) {
        {
            (ok, partial) = mΔ1.matches(name, matchString); if (ok) {
                return (ok, partial);
            }
        }
    }
    return (false, false);
}

internal static error verify(this alternationMatch m, @string name, Func<@string, @string, (bool, error)> matchString) {
    foreach (var (i, mΔ1) in m) {
        {
            var err = mΔ1.verify(name, matchString); if (err != default!) {
                return fmt.Errorf("alternation %d of %s"u8, i, err);
            }
        }
    }
    return default!;
}

internal static filterMatch splitRegexp(@string s) {
    var a = new simpleMatch(0, strings.Count(s, "/"u8));
    var b = new alternationMatch(0, strings.Count(s, "|"u8));
    nint cs = 0;
    nint cp = 0;
    for (nint i = 0; i < len(s); ) {
        switch (s[i]) {
        case (rune)'[': {
            cs++;
            break;
        }
        case (rune)']': {
            {
                cs--; if (cs < 0) {
                    // An unmatched ']' is legal.
                    cs = 0;
                }
            }
            break;
        }
        case (rune)'(': {
            if (cs == 0) {
                cp++;
            }
            break;
        }
        case (rune)')': {
            if (cs == 0) {
                cp--;
            }
            break;
        }
        case (rune)'\\': {
            i++;
            break;
        }
        case (rune)'/': {
            if (cs == 0 && cp == 0) {
                a = append(a, s[..(int)(i)]);
                s = s[(int)(i + 1)..];
                i = 0;
                continue;
            }
            break;
        }
        case (rune)'|': {
            if (cs == 0 && cp == 0) {
                a = append(a, s[..(int)(i)]);
                s = s[(int)(i + 1)..];
                i = 0;
                b = append(b, a);
                a = new simpleMatch(0, len(a));
                continue;
            }
            break;
        }}

        i++;
    }
    a = append(a, s);
    if (len(b) == 0) {
        return a;
    }
    return append(b, a);
}

// unique creates a unique name for the given parent and subname by affixing it
// with one or more counts, if necessary.
[GoRecv] internal static @string unique(this ref matcher m, @string parent, @string subname) {
    @string @base = parent + "/"u8 + subname;
    while (ᐧ) {
        var n = m.subNames[@base];
        if (n < 0) {
            throw panic("subtest count overflow");
        }
        m.subNames[@base] = n + 1;
        if (n == 0 && subname != ""u8) {
            var (prefix, nn) = parseSubtestNumber(@base);
            if (len(prefix) < len(@base) && nn < m.subNames[prefix]) {
                // This test is explicitly named like "parent/subname#NN",
                // and #NN was already used for the NNth occurrence of "parent/subname".
                // Loop to add a disambiguating suffix.
                continue;
            }
            return @base;
        }
        @string name = fmt.Sprintf("%s#%02d"u8, @base, n);
        if (m.subNames[name] != 0) {
            // This is the nth occurrence of base, but the name "parent/subname#NN"
            // collides with the first occurrence of a subtest *explicitly* named
            // "parent/subname#NN". Try the next number.
            continue;
        }
        return name;
    }
}

// parseSubtestNumber splits a subtest name into a "#%02d"-formatted int32
// suffix (if present), and a prefix preceding that suffix (always).
internal static (@string prefix, int32 nn) parseSubtestNumber(@string s) {
    @string prefix = default!;
    int32 nn = default!;

    nint i = strings.LastIndex(s, "#"u8);
    if (i < 0) {
        return (s, 0);
    }
    prefix = s[..(int)(i)];
    @string suffix = s[(int)(i + 1)..];
    if (len(suffix) < 2 || (len(suffix) > 2 && suffix[0] == (rune)'0')) {
        // Even if suffix is numeric, it is not a possible output of a "%02" format
        // string: it has either too few digits or too many leading zeroes.
        return (s, 0);
    }
    if (suffix == "00"u8) {
        if (!strings.HasSuffix(prefix, "/"u8)) {
            // We only use "#00" as a suffix for subtests named with the empty
            // string — it isn't a valid suffix if the subtest name is non-empty.
            return (s, 0);
        }
    }
    var (n, err) = strconv.ParseInt(suffix, 10, 32);
    if (err != default! || n < 0) {
        return (s, 0);
    }
    return (prefix, ((int32)n));
}

// rewrite rewrites a subname to having only printable characters and no white
// space.
internal static @string rewrite(@string s) {
    var b = new byte[]{}.slice();
    foreach (var (_, r) in s) {
        switch (ᐧ) {
        case {} when isSpace(r): {
            b = append(b, (rune)'_');
            break;
        }
        case {} when !strconv.IsPrint(r): {
            @string sΔ2 = strconv.QuoteRune(r);
            b = append(b, sΔ2[1..(int)(len(sΔ2) - 1)].ꓸꓸꓸ);
            break;
        }
        default: {
            b = append(b, ((@string)r).ꓸꓸꓸ);
            break;
        }}

    }
    return ((@string)b);
}

internal static bool isSpace(rune r) {
    if (r < 8192){
        switch (r) {
        case (rune)'\t' or (rune)'\n' or (rune)'\v' or (rune)'\f' or (rune)'\r' or (rune)' ' or 133 or 160 or 5760: {
            return true;
        }}

    } else {
        // Note: not the same as Unicode Z class.
        if (r <= 8202) {
            return true;
        }
        switch (r) {
        case 8232 or 8233 or 8239 or 8287 or 12288: {
            return true;
        }}

    }
    return false;
}

} // end testing_package
