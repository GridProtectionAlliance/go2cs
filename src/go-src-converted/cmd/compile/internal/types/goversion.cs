// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package types -- go2cs converted at 2022 March 06 22:47:51 UTC
// import "cmd/compile/internal/types" ==> using types = go.cmd.compile.@internal.types_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\types\goversion.go
using fmt = go.fmt_package;
using goversion = go.@internal.goversion_package;
using log = go.log_package;
using regexp = go.regexp_package;
using strconv = go.strconv_package;

using @base = go.cmd.compile.@internal.@base_package;

namespace go.cmd.compile.@internal;

public static partial class types_package {

    // A lang is a language version broken into major and minor numbers.
private partial struct lang {
    public nint major;
    public nint minor;
}

// langWant is the desired language version set by the -lang flag.
// If the -lang flag is not set, this is the zero value, meaning that
// any language version is supported.
private static lang langWant = default;

// AllowsGoVersion reports whether a particular package
// is allowed to use Go version major.minor.
// We assume the imported packages have all been checked,
// so we only have to check the local package against the -lang flag.
public static bool AllowsGoVersion(ptr<Pkg> _addr_pkg, nint major, nint minor) {
    ref Pkg pkg = ref _addr_pkg.val;

    if (pkg == null) { 
        // TODO(mdempsky): Set Pkg for local types earlier.
        pkg = LocalPkg;

    }
    if (pkg != LocalPkg) { 
        // Assume imported packages passed type-checking.
        return true;

    }
    if (langWant.major == 0 && langWant.minor == 0) {
        return true;
    }
    return langWant.major > major || (langWant.major == major && langWant.minor >= minor);

}

// ParseLangFlag verifies that the -lang flag holds a valid value, and
// exits if not. It initializes data used by langSupported.
public static void ParseLangFlag() {
    if (@base.Flag.Lang == "") {
        return ;
    }
    error err = default!;
    langWant, err = parseLang(@base.Flag.Lang);
    if (err != null) {
        log.Fatalf("invalid value %q for -lang: %v", @base.Flag.Lang, err);
    }
    {
        var def = currentLang();

        if (@base.Flag.Lang != def) {
            var (defVers, err) = parseLang(def);
            if (err != null) {
                log.Fatalf("internal error parsing default lang %q: %v", def, err);
            }
            if (langWant.major > defVers.major || (langWant.major == defVers.major && langWant.minor > defVers.minor)) {
                log.Fatalf("invalid value %q for -lang: max known version is %q", @base.Flag.Lang, def);
            }
        }
    }

}

// parseLang parses a -lang option into a langVer.
private static (lang, error) parseLang(@string s) {
    lang _p0 = default;
    error _p0 = default!;

    var matches = goVersionRE.FindStringSubmatch(s);
    if (matches == null) {
        return (new lang(), error.As(fmt.Errorf("should be something like \"go1.12\""))!);
    }
    var (major, err) = strconv.Atoi(matches[1]);
    if (err != null) {
        return (new lang(), error.As(err)!);
    }
    var (minor, err) = strconv.Atoi(matches[2]);
    if (err != null) {
        return (new lang(), error.As(err)!);
    }
    return (new lang(major:major,minor:minor), error.As(null!)!);

}

// currentLang returns the current language version.
private static @string currentLang() {
    return fmt.Sprintf("go1.%d", goversion.Version);
}

// goVersionRE is a regular expression that matches the valid
// arguments to the -lang flag.
private static var goVersionRE = regexp.MustCompile("^go([1-9][0-9]*)\\.(0|[1-9][0-9]*)$");

} // end types_package
