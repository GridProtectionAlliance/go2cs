// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package testing -- go2cs converted at 2022 March 13 06:43:01 UTC
// import "testing" ==> using testing = go.testing_package
// Original source: C:\Program Files\Go\src\testing\match.go
namespace go;

using fmt = fmt_package;
using os = os_package;
using strconv = strconv_package;
using strings = strings_package;
using sync = sync_package;


// matcher sanitizes, uniques, and filters names of subtests and subbenchmarks.

using System;
public static partial class testing_package {

private partial struct matcher {
    public slice<@string> filter;
    public Func<@string, @string, (bool, error)> matchFunc;
    public sync.Mutex mu;
    public map<@string, long> subNames;
}

// TODO: fix test_main to avoid race and improve caching, also allowing to
// eliminate this Mutex.
private static sync.Mutex matchMutex = default;

private static ptr<matcher> newMatcher(Func<@string, @string, (bool, error)> matchString, @string patterns, @string name) {
    slice<@string> filter = default;
    if (patterns != "") {
        filter = splitRegexp(patterns);
        {
            var i__prev1 = i;
            var s__prev1 = s;

            foreach (var (__i, __s) in filter) {
                i = __i;
                s = __s;
                filter[i] = rewrite(s);
            } 
            // Verify filters before doing any processing.

            i = i__prev1;
            s = s__prev1;
        }

        {
            var i__prev1 = i;
            var s__prev1 = s;

            foreach (var (__i, __s) in filter) {
                i = __i;
                s = __s;
                {
                    var (_, err) = matchString(s, "non-empty");

                    if (err != null) {
                        fmt.Fprintf(os.Stderr, "testing: invalid regexp for element %d of %s (%q): %s\n", i, name, s, err);
                        os.Exit(1);
                    }

                }
            }

            i = i__prev1;
            s = s__prev1;
        }
    }
    return addr(new matcher(filter:filter,matchFunc:matchString,subNames:map[string]int64{},));
}

private static (@string, bool, bool) fullName(this ptr<matcher> _addr_m, ptr<common> _addr_c, @string subname) => func((defer, _, _) => {
    @string name = default;
    bool ok = default;
    bool partial = default;
    ref matcher m = ref _addr_m.val;
    ref common c = ref _addr_c.val;

    name = subname;

    m.mu.Lock();
    defer(m.mu.Unlock());

    if (c != null && c.level > 0) {
        name = m.unique(c.name, rewrite(subname));
    }
    matchMutex.Lock();
    defer(matchMutex.Unlock()); 

    // We check the full array of paths each time to allow for the case that
    // a pattern contains a '/'.
    var elem = strings.Split(name, "/");
    foreach (var (i, s) in elem) {
        if (i >= len(m.filter)) {
            break;
        }
        {
            var (ok, _) = m.matchFunc(m.filter[i], s);

            if (!ok) {
                return (name, false, false);
            }

        }
    }    return (name, true, len(elem) < len(m.filter));
});

private static slice<@string> splitRegexp(@string s) {
    var a = make_slice<@string>(0, strings.Count(s, "/"));
    nint cs = 0;
    nint cp = 0;
    {
        nint i = 0;

        while (i < len(s)) {
            switch (s[i]) {
                case '[': 
                    cs++;
                    break;
                case ']': 
                    cs--;

                    if (cs < 0) { // An unmatched ']' is legal.
                        cs = 0;
                    }
                    break;
                case '(': 
                    if (cs == 0) {
                        cp++;
                    }
                    break;
                case ')': 
                    if (cs == 0) {
                        cp--;
                    }
                    break;
                case '\\': 
                    i++;
                    break;
                case '/': 
                    if (cs == 0 && cp == 0) {
                        a = append(a, s[..(int)i]);
                        s = s[(int)i + 1..];
                        i = 0;
                        continue;
                    }
                    break;
            }
            i++;
        }
    }
    return append(a, s);
}

// unique creates a unique name for the given parent and subname by affixing it
// with one or more counts, if necessary.
private static @string unique(this ptr<matcher> _addr_m, @string parent, @string subname) {
    ref matcher m = ref _addr_m.val;

    var name = fmt.Sprintf("%s/%s", parent, subname);
    var empty = subname == "";
    while (true) {
        var (next, exists) = m.subNames[name];
        if (!empty && !exists) {
            m.subNames[name] = 1; // next count is 1
            return name;
        }
        m.subNames[name] = next + 1; 

        // Add a count to guarantee uniqueness.
        name = fmt.Sprintf("%s#%02d", name, next);
        empty = false;
    }
}

// rewrite rewrites a subname to having only printable characters and no white
// space.
private static @string rewrite(@string s) {
    byte b = new slice<byte>(new byte[] {  });
    foreach (var (_, r) in s) {

        if (isSpace(r)) 
            b = append(b, '_');
        else if (!strconv.IsPrint(r)) 
            var s = strconv.QuoteRune(r);
            b = append(b, s[(int)1..(int)len(s) - 1]);
        else 
            b = append(b, string(r));
            }    return string(b);
}

private static bool isSpace(int r) {
    if (r < 0x2000) {
        switch (r) { 
        // Note: not the same as Unicode Z class.
            case '\t': 

            case '\n': 

            case '\v': 

            case '\f': 

            case '\r': 

            case ' ': 

            case 0x85: 

            case 0xA0: 

            case 0x1680: 
                return true;
                break;
        }
    }
    else
 {
        if (r <= 0x200a) {
            return true;
        }
        switch (r) {
            case 0x2028: 

            case 0x2029: 

            case 0x202f: 

            case 0x205f: 

            case 0x3000: 
                return true;
                break;
        }
    }
    return false;
}

} // end testing_package
