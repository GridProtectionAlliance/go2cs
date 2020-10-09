// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package lazyregexp is a thin wrapper over regexp, allowing the use of global
// regexp variables without forcing them to be compiled at init.
// package lazyregexp -- go2cs converted at 2020 October 09 05:46:46 UTC
// import "golang.org/x/mod/internal/lazyregexp" ==> using lazyregexp = go.golang.org.x.mod.@internal.lazyregexp_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\mod\internal\lazyregexp\lazyre.go
using os = go.os_package;
using regexp = go.regexp_package;
using strings = go.strings_package;
using sync = go.sync_package;
using static go.builtin;

namespace go {
namespace golang.org {
namespace x {
namespace mod {
namespace @internal
{
    public static partial class lazyregexp_package
    {
        // Regexp is a wrapper around regexp.Regexp, where the underlying regexp will be
        // compiled the first time it is needed.
        public partial struct Regexp
        {
            public @string str;
            public sync.Once once;
            public ptr<regexp.Regexp> rx;
        }

        private static ptr<regexp.Regexp> re(this ptr<Regexp> _addr_r)
        {
            ref Regexp r = ref _addr_r.val;

            r.once.Do(r.build);
            return _addr_r.rx!;
        }

        private static void build(this ptr<Regexp> _addr_r)
        {
            ref Regexp r = ref _addr_r.val;

            r.rx = regexp.MustCompile(r.str);
            r.str = "";
        }

        private static slice<slice<byte>> FindSubmatch(this ptr<Regexp> _addr_r, slice<byte> s)
        {
            ref Regexp r = ref _addr_r.val;

            return r.re().FindSubmatch(s);
        }

        private static slice<@string> FindStringSubmatch(this ptr<Regexp> _addr_r, @string s)
        {
            ref Regexp r = ref _addr_r.val;

            return r.re().FindStringSubmatch(s);
        }

        private static slice<long> FindStringSubmatchIndex(this ptr<Regexp> _addr_r, @string s)
        {
            ref Regexp r = ref _addr_r.val;

            return r.re().FindStringSubmatchIndex(s);
        }

        private static @string ReplaceAllString(this ptr<Regexp> _addr_r, @string src, @string repl)
        {
            ref Regexp r = ref _addr_r.val;

            return r.re().ReplaceAllString(src, repl);
        }

        private static @string FindString(this ptr<Regexp> _addr_r, @string s)
        {
            ref Regexp r = ref _addr_r.val;

            return r.re().FindString(s);
        }

        private static slice<@string> FindAllString(this ptr<Regexp> _addr_r, @string s, long n)
        {
            ref Regexp r = ref _addr_r.val;

            return r.re().FindAllString(s, n);
        }

        private static bool MatchString(this ptr<Regexp> _addr_r, @string s)
        {
            ref Regexp r = ref _addr_r.val;

            return r.re().MatchString(s);
        }

        private static slice<@string> SubexpNames(this ptr<Regexp> _addr_r)
        {
            ref Regexp r = ref _addr_r.val;

            return r.re().SubexpNames();
        }

        private static var inTest = len(os.Args) > 0L && strings.HasSuffix(strings.TrimSuffix(os.Args[0L], ".exe"), ".test");

        // New creates a new lazy regexp, delaying the compiling work until it is first
        // needed. If the code is being run as part of tests, the regexp compiling will
        // happen immediately.
        public static ptr<Regexp> New(@string str)
        {
            ptr<Regexp> lr = addr(new Regexp(str:str));
            if (inTest)
            { 
                // In tests, always compile the regexps early.
                lr.re();
            }
            return _addr_lr!;
        }
    }
}}}}}
