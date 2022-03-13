// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package types -- go2cs converted at 2022 March 13 05:53:41 UTC
// import "go/types" ==> using types = go.go.types_package
// Original source: C:\Program Files\Go\src\go\types\version.go
namespace go.go;

using fmt = fmt_package;
using ast = go.ast_package;
using token = go.token_package;
using regexp = regexp_package;
using strconv = strconv_package;
using strings = strings_package;


// langCompat reports an error if the representation of a numeric
// literal is not compatible with the current language version.

public static partial class types_package {

private static void langCompat(this ptr<Checker> _addr_check, ptr<ast.BasicLit> _addr_lit) {
    ref Checker check = ref _addr_check.val;
    ref ast.BasicLit lit = ref _addr_lit.val;

    var s = lit.Value;
    if (len(s) <= 2 || check.allowVersion(check.pkg, 1, 13)) {
        return ;
    }
    if (strings.Contains(s, "_")) {
        check.errorf(lit, _InvalidLit, "underscores in numeric literals requires go1.13 or later");
        return ;
    }
    if (s[0] != '0') {
        return ;
    }
    var radix = s[1];
    if (radix == 'b' || radix == 'B') {
        check.errorf(lit, _InvalidLit, "binary literals requires go1.13 or later");
        return ;
    }
    if (radix == 'o' || radix == 'O') {
        check.errorf(lit, _InvalidLit, "0o/0O-style octal literals requires go1.13 or later");
        return ;
    }
    if (lit.Kind != token.INT && (radix == 'x' || radix == 'X')) {
        check.errorf(lit, _InvalidLit, "hexadecimal floating-point literals requires go1.13 or later");
    }
}

// allowVersion reports whether the given package
// is allowed to use version major.minor.
private static bool allowVersion(this ptr<Checker> _addr_check, ptr<Package> _addr_pkg, nint major, nint minor) {
    ref Checker check = ref _addr_check.val;
    ref Package pkg = ref _addr_pkg.val;
 
    // We assume that imported packages have all been checked,
    // so we only have to check for the local package.
    if (pkg != check.pkg) {
        return true;
    }
    var ma = check.version.major;
    var mi = check.version.minor;
    return ma == 0 && mi == 0 || ma > major || ma == major && mi >= minor;
}

private partial struct version {
    public nint major;
    public nint minor;
}

// parseGoVersion parses a Go version string (such as "go1.12")
// and returns the version, or an error. If s is the empty
// string, the version is 0.0.
private static (version, error) parseGoVersion(@string s) {
    version v = default;
    error err = default!;

    if (s == "") {
        return ;
    }
    var matches = goVersionRx.FindStringSubmatch(s);
    if (matches == null) {
        err = fmt.Errorf("should be something like \"go1.12\"");
        return ;
    }
    v.major, err = strconv.Atoi(matches[1]);
    if (err != null) {
        return ;
    }
    v.minor, err = strconv.Atoi(matches[2]);
    return ;
}

// goVersionRx matches a Go version string, e.g. "go1.12".
private static var goVersionRx = regexp.MustCompile("^go([1-9][0-9]*)\\.(0|[1-9][0-9]*)$");

} // end types_package
