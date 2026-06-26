// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package lazyregexp is a thin wrapper over regexp, allowing the use of global
// regexp variables without forcing them to be compiled at init.
namespace go.@internal;

using os = os_package;
using regexp = regexp_package;
using strings = strings_package;
using sync = sync_package;

partial class lazyregexp_package {

// Regexp is a wrapper around regexp.Regexp, where the underlying regexp will be
// compiled the first time it is needed.
[GoType] partial struct Regexp {
    internal @string str;
    internal sync_package.Once once;
    internal ж<regexp_package.Regexp> rx;
}

[GoRecv] internal static ж<regexp.Regexp> re(this ref Regexp r) {
    r.once.Do(r.build);
    return r.rx;
}

[GoRecv] internal static void build(this ref Regexp r) {
    r.rx = regexp.MustCompile(r.str);
    r.str = ""u8;
}

[GoRecv] public static slice<slice<byte>> FindSubmatch(this ref Regexp r, slice<byte> s) {
    return r.re().FindSubmatch(s);
}

[GoRecv] public static slice<@string> FindStringSubmatch(this ref Regexp r, @string s) {
    return r.re().FindStringSubmatch(s);
}

[GoRecv] public static slice<nint> FindStringSubmatchIndex(this ref Regexp r, @string s) {
    return r.re().FindStringSubmatchIndex(s);
}

[GoRecv] public static @string ReplaceAllString(this ref Regexp r, @string src, @string repl) {
    return r.re().ReplaceAllString(src, repl);
}

[GoRecv] public static @string FindString(this ref Regexp r, @string s) {
    return r.re().FindString(s);
}

[GoRecv] public static slice<@string> FindAllString(this ref Regexp r, @string s, nint n) {
    return r.re().FindAllString(s, n);
}

[GoRecv] public static bool MatchString(this ref Regexp r, @string s) {
    return r.re().MatchString(s);
}

[GoRecv] public static slice<@string> SubexpNames(this ref Regexp r) {
    return r.re().SubexpNames();
}

internal static bool inTest = len(os.Args) > 0 && strings.HasSuffix(strings.TrimSuffix(os.Args[0], ".exe"u8), ".test"u8);

// New creates a new lazy regexp, delaying the compiling work until it is first
// needed. If the code is being run as part of tests, the regexp compiling will
// happen immediately.
public static ж<Regexp> New(@string str) {
    var lr = Ꮡ(new Regexp(str: str));
    if (inTest) {
        // In tests, always compile the regexps early.
        lr.re();
    }
    return lr;
}

} // end lazyregexp_package
