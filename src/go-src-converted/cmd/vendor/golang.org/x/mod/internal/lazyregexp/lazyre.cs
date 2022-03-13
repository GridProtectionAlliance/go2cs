// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package lazyregexp is a thin wrapper over regexp, allowing the use of global
// regexp variables without forcing them to be compiled at init.

// package lazyregexp -- go2cs converted at 2022 March 13 06:40:47 UTC
// import "cmd/vendor/golang.org/x/mod/internal/lazyregexp" ==> using lazyregexp = go.cmd.vendor.golang.org.x.mod.@internal.lazyregexp_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\mod\internal\lazyregexp\lazyre.go
namespace go.cmd.vendor.golang.org.x.mod.@internal;

using os = os_package;
using regexp = regexp_package;
using strings = strings_package;
using sync = sync_package;


// Regexp is a wrapper around regexp.Regexp, where the underlying regexp will be
// compiled the first time it is needed.

public static partial class lazyregexp_package {

public partial struct Regexp {
    public @string str;
    public sync.Once once;
    public ptr<regexp.Regexp> rx;
}

private static ptr<regexp.Regexp> re(this ptr<Regexp> _addr_r) {
    ref Regexp r = ref _addr_r.val;

    r.once.Do(r.build);
    return _addr_r.rx!;
}

private static void build(this ptr<Regexp> _addr_r) {
    ref Regexp r = ref _addr_r.val;

    r.rx = regexp.MustCompile(r.str);
    r.str = "";
}

private static slice<slice<byte>> FindSubmatch(this ptr<Regexp> _addr_r, slice<byte> s) {
    ref Regexp r = ref _addr_r.val;

    return r.re().FindSubmatch(s);
}

private static slice<@string> FindStringSubmatch(this ptr<Regexp> _addr_r, @string s) {
    ref Regexp r = ref _addr_r.val;

    return r.re().FindStringSubmatch(s);
}

private static slice<nint> FindStringSubmatchIndex(this ptr<Regexp> _addr_r, @string s) {
    ref Regexp r = ref _addr_r.val;

    return r.re().FindStringSubmatchIndex(s);
}

private static @string ReplaceAllString(this ptr<Regexp> _addr_r, @string src, @string repl) {
    ref Regexp r = ref _addr_r.val;

    return r.re().ReplaceAllString(src, repl);
}

private static @string FindString(this ptr<Regexp> _addr_r, @string s) {
    ref Regexp r = ref _addr_r.val;

    return r.re().FindString(s);
}

private static slice<@string> FindAllString(this ptr<Regexp> _addr_r, @string s, nint n) {
    ref Regexp r = ref _addr_r.val;

    return r.re().FindAllString(s, n);
}

private static bool MatchString(this ptr<Regexp> _addr_r, @string s) {
    ref Regexp r = ref _addr_r.val;

    return r.re().MatchString(s);
}

private static slice<@string> SubexpNames(this ptr<Regexp> _addr_r) {
    ref Regexp r = ref _addr_r.val;

    return r.re().SubexpNames();
}

private static var inTest = len(os.Args) > 0 && strings.HasSuffix(strings.TrimSuffix(os.Args[0], ".exe"), ".test");

// New creates a new lazy regexp, delaying the compiling work until it is first
// needed. If the code is being run as part of tests, the regexp compiling will
// happen immediately.
public static ptr<Regexp> New(@string str) {
    ptr<Regexp> lr = addr(new Regexp(str:str));
    if (inTest) { 
        // In tests, always compile the regexps early.
        lr.re();
    }
    return _addr_lr!;
}

} // end lazyregexp_package
