// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package testing -- go2cs converted at 2020 October 08 04:36:30 UTC
// import "testing" ==> using testing = go.testing_package
// Original source: C:\Go\src\testing\match.go
using fmt = go.fmt_package;
using os = go.os_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using sync = go.sync_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class testing_package
    {
        // matcher sanitizes, uniques, and filters names of subtests and subbenchmarks.
        private partial struct matcher
        {
            public slice<@string> filter;
            public Func<@string, @string, (bool, error)> matchFunc;
            public sync.Mutex mu;
            public map<@string, long> subNames;
        }

        // TODO: fix test_main to avoid race and improve caching, also allowing to
        // eliminate this Mutex.
        private static sync.Mutex matchMutex = default;

        private static ptr<matcher> newMatcher(Func<@string, @string, (bool, error)> matchString, @string patterns, @string name)
        {
            slice<@string> filter = default;
            if (patterns != "")
            {
                filter = splitRegexp(patterns);
                {
                    var i__prev1 = i;
                    var s__prev1 = s;

                    foreach (var (__i, __s) in filter)
                    {
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

                    foreach (var (__i, __s) in filter)
                    {
                        i = __i;
                        s = __s;
                        {
                            var (_, err) = matchString(s, "non-empty");

                            if (err != null)
                            {
                                fmt.Fprintf(os.Stderr, "testing: invalid regexp for element %d of %s (%q): %s\n", i, name, s, err);
                                os.Exit(1L);
                            }

                        }

                    }

                    i = i__prev1;
                    s = s__prev1;
                }
            }

            return addr(new matcher(filter:filter,matchFunc:matchString,subNames:map[string]int64{},));

        }

        private static (@string, bool, bool) fullName(this ptr<matcher> _addr_m, ptr<common> _addr_c, @string subname) => func((defer, _, __) =>
        {
            @string name = default;
            bool ok = default;
            bool partial = default;
            ref matcher m = ref _addr_m.val;
            ref common c = ref _addr_c.val;

            name = subname;

            m.mu.Lock();
            defer(m.mu.Unlock());

            if (c != null && c.level > 0L)
            {
                name = m.unique(c.name, rewrite(subname));
            }

            matchMutex.Lock();
            defer(matchMutex.Unlock()); 

            // We check the full array of paths each time to allow for the case that
            // a pattern contains a '/'.
            var elem = strings.Split(name, "/");
            foreach (var (i, s) in elem)
            {
                if (i >= len(m.filter))
                {
                    break;
                }

                {
                    var (ok, _) = m.matchFunc(m.filter[i], s);

                    if (!ok)
                    {
                        return (name, false, false);
                    }

                }

            }
            return (name, true, len(elem) < len(m.filter));

        });

        private static slice<@string> splitRegexp(@string s)
        {
            var a = make_slice<@string>(0L, strings.Count(s, "/"));
            long cs = 0L;
            long cp = 0L;
            {
                long i = 0L;

                while (i < len(s))
                {
                    switch (s[i])
                    {
                        case '[': 
                            cs++;
                            break;
                        case ']': 
                            cs--;

                            if (cs < 0L)
                            { // An unmatched ']' is legal.
                                cs = 0L;

                            }

                            break;
                        case '(': 
                            if (cs == 0L)
                            {
                                cp++;
                            }

                            break;
                        case ')': 
                            if (cs == 0L)
                            {
                                cp--;
                            }

                            break;
                        case '\\': 
                            i++;
                            break;
                        case '/': 
                            if (cs == 0L && cp == 0L)
                            {
                                a = append(a, s[..i]);
                                s = s[i + 1L..];
                                i = 0L;
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
        private static @string unique(this ptr<matcher> _addr_m, @string parent, @string subname)
        {
            ref matcher m = ref _addr_m.val;

            var name = fmt.Sprintf("%s/%s", parent, subname);
            var empty = subname == "";
            while (true)
            {
                var (next, exists) = m.subNames[name];
                if (!empty && !exists)
                {
                    m.subNames[name] = 1L; // next count is 1
                    return name;

                } 
                // Name was already used. We increment with the count and append a
                // string with the count.
                m.subNames[name] = next + 1L; 

                // Add a count to guarantee uniqueness.
                name = fmt.Sprintf("%s#%02d", name, next);
                empty = false;

            }


        }

        // rewrite rewrites a subname to having only printable characters and no white
        // space.
        private static @string rewrite(@string s)
        {
            byte b = new slice<byte>(new byte[] {  });
            foreach (var (_, r) in s)
            {

                if (isSpace(r)) 
                    b = append(b, '_');
                else if (!strconv.IsPrint(r)) 
                    var s = strconv.QuoteRune(r);
                    b = append(b, s[1L..len(s) - 1L]);
                else 
                    b = append(b, string(r));
                
            }
            return string(b);

        }

        private static bool isSpace(int r)
        {
            if (r < 0x2000UL)
            {
                switch (r)
                { 
                // Note: not the same as Unicode Z class.
                    case '\t': 

                    case '\n': 

                    case '\v': 

                    case '\f': 

                    case '\r': 

                    case ' ': 

                    case 0x85UL: 

                    case 0xA0UL: 

                    case 0x1680UL: 
                        return true;
                        break;
                }

            }
            else
            {
                if (r <= 0x200aUL)
                {
                    return true;
                }

                switch (r)
                {
                    case 0x2028UL: 

                    case 0x2029UL: 

                    case 0x202fUL: 

                    case 0x205fUL: 

                    case 0x3000UL: 
                        return true;
                        break;
                }

            }

            return false;

        }
    }
}
