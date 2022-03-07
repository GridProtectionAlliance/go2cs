// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package module defines the module.Version type along with support code.
//
// The module.Version type is a simple Path, Version pair:
//
//    type Version struct {
//        Path string
//        Version string
//    }
//
// There are no restrictions imposed directly by use of this structure,
// but additional checking functions, most notably Check, verify that
// a particular path, version pair is valid.
//
// Escaped Paths
//
// Module paths appear as substrings of file system paths
// (in the download cache) and of web server URLs in the proxy protocol.
// In general we cannot rely on file systems to be case-sensitive,
// nor can we rely on web servers, since they read from file systems.
// That is, we cannot rely on the file system to keep rsc.io/QUOTE
// and rsc.io/quote separate. Windows and macOS don't.
// Instead, we must never require two different casings of a file path.
// Because we want the download cache to match the proxy protocol,
// and because we want the proxy protocol to be possible to serve
// from a tree of static files (which might be stored on a case-insensitive
// file system), the proxy protocol must never require two different casings
// of a URL path either.
//
// One possibility would be to make the escaped form be the lowercase
// hexadecimal encoding of the actual path bytes. This would avoid ever
// needing different casings of a file path, but it would be fairly illegible
// to most programmers when those paths appeared in the file system
// (including in file paths in compiler errors and stack traces)
// in web server logs, and so on. Instead, we want a safe escaped form that
// leaves most paths unaltered.
//
// The safe escaped form is to replace every uppercase letter
// with an exclamation mark followed by the letter's lowercase equivalent.
//
// For example,
//
//    github.com/Azure/azure-sdk-for-go ->  github.com/!azure/azure-sdk-for-go.
//    github.com/GoogleCloudPlatform/cloudsql-proxy -> github.com/!google!cloud!platform/cloudsql-proxy
//    github.com/Sirupsen/logrus -> github.com/!sirupsen/logrus.
//
// Import paths that avoid upper-case letters are left unchanged.
// Note that because import paths are ASCII-only and avoid various
// problematic punctuation (like : < and >), the escaped form is also ASCII-only
// and avoids the same problematic punctuation.
//
// Import paths have never allowed exclamation marks, so there is no
// need to define how to escape a literal !.
//
// Unicode Restrictions
//
// Today, paths are disallowed from using Unicode.
//
// Although paths are currently disallowed from using Unicode,
// we would like at some point to allow Unicode letters as well, to assume that
// file systems and URLs are Unicode-safe (storing UTF-8), and apply
// the !-for-uppercase convention for escaping them in the file system.
// But there are at least two subtle considerations.
//
// First, note that not all case-fold equivalent distinct runes
// form an upper/lower pair.
// For example, U+004B ('K'), U+006B ('k'), and U+212A ('K' for Kelvin)
// are three distinct runes that case-fold to each other.
// When we do add Unicode letters, we must not assume that upper/lower
// are the only case-equivalent pairs.
// Perhaps the Kelvin symbol would be disallowed entirely, for example.
// Or perhaps it would escape as "!!k", or perhaps as "(212A)".
//
// Second, it would be nice to allow Unicode marks as well as letters,
// but marks include combining marks, and then we must deal not
// only with case folding but also normalization: both U+00E9 ('é')
// and U+0065 U+0301 ('e' followed by combining acute accent)
// look the same on the page and are treated by some file systems
// as the same path. If we do allow Unicode marks in paths, there
// must be some kind of normalization to allow only one canonical
// encoding of any character used in an import path.
// package module -- go2cs converted at 2022 March 06 23:16:41 UTC
// import "golang.org/x/mod/module" ==> using module = go.golang.org.x.mod.module_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\mod\module\module.go
// IMPORTANT NOTE
//
// This file essentially defines the set of valid import paths for the go command.
// There are many subtle considerations, including Unicode ambiguity,
// security, network, and file system representations.
//
// This file also defines the set of valid module path and version combinations,
// another topic with many subtle considerations.
//
// Changes to the semantics in this file require approval from rsc.

using fmt = go.fmt_package;
using sort = go.sort_package;
using strings = go.strings_package;
using unicode = go.unicode_package;
using utf8 = go.unicode.utf8_package;

using semver = go.golang.org.x.mod.semver_package;
using errors = go.golang.org.x.xerrors_package;
using System.ComponentModel;
using System;


namespace go.golang.org.x.mod;

public static partial class module_package {

    // A Version (for clients, a module.Version) is defined by a module path and version pair.
    // These are stored in their plain (unescaped) form.
public partial struct Version {
    public @string Path; // Version is usually a semantic version in canonical form.
// There are three exceptions to this general rule.
// First, the top-level target of a build has no specific version
// and uses Version = "".
// Second, during MVS calculations the version "none" is used
// to represent the decision to take no version of a given module.
// Third, filesystem paths found in "replace" directives are
// represented by a path with an empty version.
    [Description("json:\",omitempty\"")]
    public @string Version;
}

// String returns a representation of the Version suitable for logging
// (Path@Version, or just Path if Version is empty).
public static @string String(this Version m) {
    if (m.Version == "") {
        return m.Path;
    }
    return m.Path + "@" + m.Version;
}

// A ModuleError indicates an error specific to a module.
public partial struct ModuleError {
    public @string Path;
    public @string Version;
    public error Err;
}

// VersionError returns a ModuleError derived from a Version and error,
// or err itself if it is already such an error.
public static error VersionError(Version v, error err) {
    ptr<ModuleError> mErr;
    if (errors.As(err, _addr_mErr) && mErr.Path == v.Path && mErr.Version == v.Version) {
        return error.As(err)!;
    }
    return error.As(addr(new ModuleError(Path:v.Path,Version:v.Version,Err:err,))!)!;
}

private static @string Error(this ptr<ModuleError> _addr_e) {
    ref ModuleError e = ref _addr_e.val;

    {
        ptr<InvalidVersionError> (v, ok) = e.Err._<ptr<InvalidVersionError>>();

        if (ok) {
            return fmt.Sprintf("%s@%s: invalid %s: %v", e.Path, v.Version, v.noun(), v.Err);
        }
    }
    if (e.Version != "") {
        return fmt.Sprintf("%s@%s: %v", e.Path, e.Version, e.Err);
    }
    return fmt.Sprintf("module %s: %v", e.Path, e.Err);
}

private static error Unwrap(this ptr<ModuleError> _addr_e) {
    ref ModuleError e = ref _addr_e.val;

    return error.As(e.Err)!;
}

// An InvalidVersionError indicates an error specific to a version, with the
// module path unknown or specified externally.
//
// A ModuleError may wrap an InvalidVersionError, but an InvalidVersionError
// must not wrap a ModuleError.
public partial struct InvalidVersionError {
    public @string Version;
    public bool Pseudo;
    public error Err;
}

// noun returns either "version" or "pseudo-version", depending on whether
// e.Version is a pseudo-version.
private static @string noun(this ptr<InvalidVersionError> _addr_e) {
    ref InvalidVersionError e = ref _addr_e.val;

    if (e.Pseudo) {
        return "pseudo-version";
    }
    return "version";
}

private static @string Error(this ptr<InvalidVersionError> _addr_e) {
    ref InvalidVersionError e = ref _addr_e.val;

    return fmt.Sprintf("%s %q invalid: %s", e.noun(), e.Version, e.Err);
}

private static error Unwrap(this ptr<InvalidVersionError> _addr_e) {
    ref InvalidVersionError e = ref _addr_e.val;

    return error.As(e.Err)!;
}

// Check checks that a given module path, version pair is valid.
// In addition to the path being a valid module path
// and the version being a valid semantic version,
// the two must correspond.
// For example, the path "yaml/v2" only corresponds to
// semantic versions beginning with "v2.".
public static error Check(@string path, @string version) {
    {
        var err__prev1 = err;

        var err = CheckPath(path);

        if (err != null) {
            return error.As(err)!;
        }
        err = err__prev1;

    }
    if (!semver.IsValid(version)) {
        return error.As(addr(new ModuleError(Path:path,Err:&InvalidVersionError{Version:version,Err:errors.New("not a semantic version")},))!)!;
    }
    var (_, pathMajor, _) = SplitPathVersion(path);
    {
        var err__prev1 = err;

        err = CheckPathMajor(version, pathMajor);

        if (err != null) {
            return error.As(addr(new ModuleError(Path:path,Err:err))!)!;
        }
        err = err__prev1;

    }
    return error.As(null!)!;
}

// firstPathOK reports whether r can appear in the first element of a module path.
// The first element of the path must be an LDH domain name, at least for now.
// To avoid case ambiguity, the domain name must be entirely lower case.
private static bool firstPathOK(int r) {
    return r == '-' || r == '.' || '0' <= r && r <= '9' || 'a' <= r && r <= 'z';
}

// pathOK reports whether r can appear in an import path element.
// Paths can be ASCII letters, ASCII digits, and limited ASCII punctuation: + - . _ and ~.
// This matches what "go get" has historically recognized in import paths.
// TODO(rsc): We would like to allow Unicode letters, but that requires additional
// care in the safe encoding (see "escaped paths" above).
private static bool pathOK(int r) {
    if (r < utf8.RuneSelf) {
        return r == '+' || r == '-' || r == '.' || r == '_' || r == '~' || '0' <= r && r <= '9' || 'A' <= r && r <= 'Z' || 'a' <= r && r <= 'z';
    }
    return false;
}

// fileNameOK reports whether r can appear in a file name.
// For now we allow all Unicode letters but otherwise limit to pathOK plus a few more punctuation characters.
// If we expand the set of allowed characters here, we have to
// work harder at detecting potential case-folding and normalization collisions.
// See note about "escaped paths" above.
private static bool fileNameOK(int r) {
    if (r < utf8.RuneSelf) { 
        // Entire set of ASCII punctuation, from which we remove characters:
        //     ! " # $ % & ' ( ) * + , - . / : ; < = > ? @ [ \ ] ^ _ ` { | } ~
        // We disallow some shell special characters: " ' * < > ? ` |
        // (Note that some of those are disallowed by the Windows file system as well.)
        // We also disallow path separators / : and \ (fileNameOK is only called on path element characters).
        // We allow spaces (U+0020) in file names.
        const @string allowed = "!#$%&()+,-.=@[]^_{}~ ";

        if ('0' <= r && r <= '9' || 'A' <= r && r <= 'Z' || 'a' <= r && r <= 'z') {
            return true;
        }
        for (nint i = 0; i < len(allowed); i++) {
            if (rune(allowed[i]) == r) {
                return true;
            }
        }
        return false;
    }
    return unicode.IsLetter(r);
}

// CheckPath checks that a module path is valid.
// A valid module path is a valid import path, as checked by CheckImportPath,
// with two additional constraints.
// First, the leading path element (up to the first slash, if any),
// by convention a domain name, must contain only lower-case ASCII letters,
// ASCII digits, dots (U+002E), and dashes (U+002D);
// it must contain at least one dot and cannot start with a dash.
// Second, for a final path element of the form /vN, where N looks numeric
// (ASCII digits and dots) must not begin with a leading zero, must not be /v1,
// and must not contain any dots. For paths beginning with "gopkg.in/",
// this second requirement is replaced by a requirement that the path
// follow the gopkg.in server's conventions.
public static error CheckPath(@string path) {
    {
        var err = checkPath(path, false);

        if (err != null) {
            return error.As(fmt.Errorf("malformed module path %q: %v", path, err))!;
        }
    }
    var i = strings.Index(path, "/");
    if (i < 0) {
        i = len(path);
    }
    if (i == 0) {
        return error.As(fmt.Errorf("malformed module path %q: leading slash", path))!;
    }
    if (!strings.Contains(path[..(int)i], ".")) {
        return error.As(fmt.Errorf("malformed module path %q: missing dot in first path element", path))!;
    }
    if (path[0] == '-') {
        return error.As(fmt.Errorf("malformed module path %q: leading dash in first path element", path))!;
    }
    foreach (var (_, r) in path[..(int)i]) {
        if (!firstPathOK(r)) {
            return error.As(fmt.Errorf("malformed module path %q: invalid char %q in first path element", path, r))!;
        }
    }    {
        var (_, _, ok) = SplitPathVersion(path);

        if (!ok) {
            return error.As(fmt.Errorf("malformed module path %q: invalid version", path))!;
        }
    }
    return error.As(null!)!;
}

// CheckImportPath checks that an import path is valid.
//
// A valid import path consists of one or more valid path elements
// separated by slashes (U+002F). (It must not begin with nor end in a slash.)
//
// A valid path element is a non-empty string made up of
// ASCII letters, ASCII digits, and limited ASCII punctuation: + - . _ and ~.
// It must not begin or end with a dot (U+002E), nor contain two dots in a row.
//
// The element prefix up to the first dot must not be a reserved file name
// on Windows, regardless of case (CON, com1, NuL, and so on).
//
// CheckImportPath may be less restrictive in the future, but see the
// top-level package documentation for additional information about
// subtleties of Unicode.
public static error CheckImportPath(@string path) {
    {
        var err = checkPath(path, false);

        if (err != null) {
            return error.As(fmt.Errorf("malformed import path %q: %v", path, err))!;
        }
    }
    return error.As(null!)!;
}

// checkPath checks that a general path is valid.
// It returns an error describing why but not mentioning path.
// Because these checks apply to both module paths and import paths,
// the caller is expected to add the "malformed ___ path %q: " prefix.
// fileName indicates whether the final element of the path is a file name
// (as opposed to a directory name).
private static error checkPath(@string path, bool fileName) {
    if (!utf8.ValidString(path)) {
        return error.As(fmt.Errorf("invalid UTF-8"))!;
    }
    if (path == "") {
        return error.As(fmt.Errorf("empty string"))!;
    }
    if (path[0] == '-') {
        return error.As(fmt.Errorf("leading dash"))!;
    }
    if (strings.Contains(path, "//")) {
        return error.As(fmt.Errorf("double slash"))!;
    }
    if (path[len(path) - 1] == '/') {
        return error.As(fmt.Errorf("trailing slash"))!;
    }
    nint elemStart = 0;
    foreach (var (i, r) in path) {
        if (r == '/') {
            {
                var err__prev2 = err;

                var err = checkElem(path[(int)elemStart..(int)i], fileName);

                if (err != null) {
                    return error.As(err)!;
                }

                err = err__prev2;

            }
            elemStart = i + 1;
        }
    }    {
        var err__prev1 = err;

        err = checkElem(path[(int)elemStart..], fileName);

        if (err != null) {
            return error.As(err)!;
        }
        err = err__prev1;

    }
    return error.As(null!)!;
}

// checkElem checks whether an individual path element is valid.
// fileName indicates whether the element is a file name (not a directory name).
private static error checkElem(@string elem, bool fileName) {
    if (elem == "") {
        return error.As(fmt.Errorf("empty path element"))!;
    }
    if (strings.Count(elem, ".") == len(elem)) {
        return error.As(fmt.Errorf("invalid path element %q", elem))!;
    }
    if (elem[0] == '.' && !fileName) {
        return error.As(fmt.Errorf("leading dot in path element"))!;
    }
    if (elem[len(elem) - 1] == '.') {
        return error.As(fmt.Errorf("trailing dot in path element"))!;
    }
    var charOK = pathOK;
    if (fileName) {
        charOK = fileNameOK;
    }
    foreach (var (_, r) in elem) {
        if (!charOK(r)) {
            return error.As(fmt.Errorf("invalid char %q", r))!;
        }
    }    var @short = elem;
    {
        var i = strings.Index(short, ".");

        if (i >= 0) {
            short = short[..(int)i];
        }
    }
    foreach (var (_, bad) in badWindowsNames) {
        if (strings.EqualFold(bad, short)) {
            return error.As(fmt.Errorf("%q disallowed as path element component on Windows", short))!;
        }
    }    return error.As(null!)!;
}

// CheckFilePath checks that a slash-separated file path is valid.
// The definition of a valid file path is the same as the definition
// of a valid import path except that the set of allowed characters is larger:
// all Unicode letters, ASCII digits, the ASCII space character (U+0020),
// and the ASCII punctuation characters
// “!#$%&()+,-.=@[]^_{}~”.
// (The excluded punctuation characters, " * < > ? ` ' | / \ and :,
// have special meanings in certain shells or operating systems.)
//
// CheckFilePath may be less restrictive in the future, but see the
// top-level package documentation for additional information about
// subtleties of Unicode.
public static error CheckFilePath(@string path) {
    {
        var err = checkPath(path, true);

        if (err != null) {
            return error.As(fmt.Errorf("malformed file path %q: %v", path, err))!;
        }
    }
    return error.As(null!)!;
}

// badWindowsNames are the reserved file path elements on Windows.
// See https://docs.microsoft.com/en-us/windows/desktop/fileio/naming-a-file
private static @string badWindowsNames = new slice<@string>(new @string[] { "CON", "PRN", "AUX", "NUL", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9" });

// SplitPathVersion returns prefix and major version such that prefix+pathMajor == path
// and version is either empty or "/vN" for N >= 2.
// As a special case, gopkg.in paths are recognized directly;
// they require ".vN" instead of "/vN", and for all N, not just N >= 2.
// SplitPathVersion returns with ok = false when presented with
// a path whose last path element does not satisfy the constraints
// applied by CheckPath, such as "example.com/pkg/v1" or "example.com/pkg/v1.2".
public static (@string, @string, bool) SplitPathVersion(@string path) {
    @string prefix = default;
    @string pathMajor = default;
    bool ok = default;

    if (strings.HasPrefix(path, "gopkg.in/")) {
        return splitGopkgIn(path);
    }
    var i = len(path);
    var dot = false;
    while (i > 0 && ('0' <= path[i - 1] && path[i - 1] <= '9' || path[i - 1] == '.')) {
        if (path[i - 1] == '.') {
            dot = true;
        }
        i--;
    }
    if (i <= 1 || i == len(path) || path[i - 1] != 'v' || path[i - 2] != '/') {
        return (path, "", true);
    }
    (prefix, pathMajor) = (path[..(int)i - 2], path[(int)i - 2..]);    if (dot || len(pathMajor) <= 2 || pathMajor[2] == '0' || pathMajor == "/v1") {
        return (path, "", false);
    }
    return (prefix, pathMajor, true);
}

// splitGopkgIn is like SplitPathVersion but only for gopkg.in paths.
private static (@string, @string, bool) splitGopkgIn(@string path) {
    @string prefix = default;
    @string pathMajor = default;
    bool ok = default;

    if (!strings.HasPrefix(path, "gopkg.in/")) {
        return (path, "", false);
    }
    var i = len(path);
    if (strings.HasSuffix(path, "-unstable")) {
        i -= len("-unstable");
    }
    while (i > 0 && ('0' <= path[i - 1] && path[i - 1] <= '9')) {
        i--;
    }
    if (i <= 1 || path[i - 1] != 'v' || path[i - 2] != '.') { 
        // All gopkg.in paths must end in vN for some N.
        return (path, "", false);
    }
    (prefix, pathMajor) = (path[..(int)i - 2], path[(int)i - 2..]);    if (len(pathMajor) <= 2 || pathMajor[2] == '0' && pathMajor != ".v0") {
        return (path, "", false);
    }
    return (prefix, pathMajor, true);
}

// MatchPathMajor reports whether the semantic version v
// matches the path major version pathMajor.
//
// MatchPathMajor returns true if and only if CheckPathMajor returns nil.
public static bool MatchPathMajor(@string v, @string pathMajor) {
    return CheckPathMajor(v, pathMajor) == null;
}

// CheckPathMajor returns a non-nil error if the semantic version v
// does not match the path major version pathMajor.
public static error CheckPathMajor(@string v, @string pathMajor) { 
    // TODO(jayconrod): return errors or panic for invalid inputs. This function
    // (and others) was covered by integration tests for cmd/go, and surrounding
    // code protected against invalid inputs like non-canonical versions.
    if (strings.HasPrefix(pathMajor, ".v") && strings.HasSuffix(pathMajor, "-unstable")) {
        pathMajor = strings.TrimSuffix(pathMajor, "-unstable");
    }
    if (strings.HasPrefix(v, "v0.0.0-") && pathMajor == ".v1") { 
        // Allow old bug in pseudo-versions that generated v0.0.0- pseudoversion for gopkg .v1.
        // For example, gopkg.in/yaml.v2@v2.2.1's go.mod requires gopkg.in/check.v1 v0.0.0-20161208181325-20d25e280405.
        return error.As(null!)!;
    }
    var m = semver.Major(v);
    if (pathMajor == "") {
        if (m == "v0" || m == "v1" || semver.Build(v) == "+incompatible") {
            return error.As(null!)!;
        }
        pathMajor = "v0 or v1";
    }
    else if (pathMajor[0] == '/' || pathMajor[0] == '.') {
        if (m == pathMajor[(int)1..]) {
            return error.As(null!)!;
        }
        pathMajor = pathMajor[(int)1..];
    }
    return error.As(addr(new InvalidVersionError(Version:v,Err:fmt.Errorf("should be %s, not %s",pathMajor,semver.Major(v)),))!)!;
}

// PathMajorPrefix returns the major-version tag prefix implied by pathMajor.
// An empty PathMajorPrefix allows either v0 or v1.
//
// Note that MatchPathMajor may accept some versions that do not actually begin
// with this prefix: namely, it accepts a 'v0.0.0-' prefix for a '.v1'
// pathMajor, even though that pathMajor implies 'v1' tagging.
public static @string PathMajorPrefix(@string pathMajor) => func((_, panic, _) => {
    if (pathMajor == "") {
        return "";
    }
    if (pathMajor[0] != '/' && pathMajor[0] != '.') {
        panic("pathMajor suffix " + pathMajor + " passed to PathMajorPrefix lacks separator");
    }
    if (strings.HasPrefix(pathMajor, ".v") && strings.HasSuffix(pathMajor, "-unstable")) {
        pathMajor = strings.TrimSuffix(pathMajor, "-unstable");
    }
    var m = pathMajor[(int)1..];
    if (m != semver.Major(m)) {
        panic("pathMajor suffix " + pathMajor + "passed to PathMajorPrefix is not a valid major version");
    }
    return m;
});

// CanonicalVersion returns the canonical form of the version string v.
// It is the same as semver.Canonical(v) except that it preserves the special build suffix "+incompatible".
public static @string CanonicalVersion(@string v) {
    var cv = semver.Canonical(v);
    if (semver.Build(v) == "+incompatible") {
        cv += "+incompatible";
    }
    return cv;
}

// Sort sorts the list by Path, breaking ties by comparing Version fields.
// The Version fields are interpreted as semantic versions (using semver.Compare)
// optionally followed by a tie-breaking suffix introduced by a slash character,
// like in "v0.0.1/go.mod".
public static void Sort(slice<Version> list) {
    sort.Slice(list, (i, j) => {
        var mi = list[i];
        var mj = list[j];
        if (mi.Path != mj.Path) {
            return mi.Path < mj.Path;
        }
        var vi = mi.Version;
        var vj = mj.Version;
        @string fi = default;        @string fj = default;

        {
            var k__prev1 = k;

            var k = strings.Index(vi, "/");

            if (k >= 0) {
                (vi, fi) = (vi[..(int)k], vi[(int)k..]);
            }

            k = k__prev1;

        }
        {
            var k__prev1 = k;

            k = strings.Index(vj, "/");

            if (k >= 0) {
                (vj, fj) = (vj[..(int)k], vj[(int)k..]);
            }

            k = k__prev1;

        }
        if (vi != vj) {
            return semver.Compare(vi, vj) < 0;
        }
        return fi < fj;
    });
}

// EscapePath returns the escaped form of the given module path.
// It fails if the module path is invalid.
public static (@string, error) EscapePath(@string path) {
    @string escaped = default;
    error err = default!;

    {
        var err = CheckPath(path);

        if (err != null) {
            return ("", error.As(err)!);
        }
    }

    return escapeString(path);
}

// EscapeVersion returns the escaped form of the given module version.
// Versions are allowed to be in non-semver form but must be valid file names
// and not contain exclamation marks.
public static (@string, error) EscapeVersion(@string v) {
    @string escaped = default;
    error err = default!;

    {
        var err = checkElem(v, true);

        if (err != null || strings.Contains(v, "!")) {
            return ("", error.As(addr(new InvalidVersionError(Version:v,Err:fmt.Errorf("disallowed version string"),))!)!);
        }
    }
    return escapeString(v);
}

private static (@string, error) escapeString(@string s) {
    @string escaped = default;
    error err = default!;

    var haveUpper = false;
    {
        var r__prev1 = r;

        foreach (var (_, __r) in s) {
            r = __r;
            if (r == '!' || r >= utf8.RuneSelf) { 
                // This should be disallowed by CheckPath, but diagnose anyway.
                // The correctness of the escaping loop below depends on it.
                return ("", error.As(fmt.Errorf("internal error: inconsistency in EscapePath"))!);
            }
            if ('A' <= r && r <= 'Z') {
                haveUpper = true;
            }
        }
        r = r__prev1;
    }

    if (!haveUpper) {
        return (s, error.As(null!)!);
    }
    slice<byte> buf = default;
    {
        var r__prev1 = r;

        foreach (var (_, __r) in s) {
            r = __r;
            if ('A' <= r && r <= 'Z') {
                buf = append(buf, '!', byte(r + 'a' - 'A'));
            }
            else
 {
                buf = append(buf, byte(r));
            }
        }
        r = r__prev1;
    }

    return (string(buf), error.As(null!)!);
}

// UnescapePath returns the module path for the given escaped path.
// It fails if the escaped path is invalid or describes an invalid path.
public static (@string, error) UnescapePath(@string escaped) {
    @string path = default;
    error err = default!;

    var (path, ok) = unescapeString(escaped);
    if (!ok) {
        return ("", error.As(fmt.Errorf("invalid escaped module path %q", escaped))!);
    }
    {
        var err = CheckPath(path);

        if (err != null) {
            return ("", error.As(fmt.Errorf("invalid escaped module path %q: %v", escaped, err))!);
        }
    }
    return (path, error.As(null!)!);
}

// UnescapeVersion returns the version string for the given escaped version.
// It fails if the escaped form is invalid or describes an invalid version.
// Versions are allowed to be in non-semver form but must be valid file names
// and not contain exclamation marks.
public static (@string, error) UnescapeVersion(@string escaped) {
    @string v = default;
    error err = default!;

    var (v, ok) = unescapeString(escaped);
    if (!ok) {
        return ("", error.As(fmt.Errorf("invalid escaped version %q", escaped))!);
    }
    {
        var err = checkElem(v, true);

        if (err != null) {
            return ("", error.As(fmt.Errorf("invalid escaped version %q: %v", v, err))!);
        }
    }
    return (v, error.As(null!)!);
}

private static (@string, bool) unescapeString(@string escaped) {
    @string _p0 = default;
    bool _p0 = default;

    slice<byte> buf = default;

    var bang = false;
    foreach (var (_, r) in escaped) {
        if (r >= utf8.RuneSelf) {
            return ("", false);
        }
        if (bang) {
            bang = false;
            if (r < 'a' || 'z' < r) {
                return ("", false);
            }
            buf = append(buf, byte(r + 'A' - 'a'));
            continue;
        }
        if (r == '!') {
            bang = true;
            continue;
        }
        if ('A' <= r && r <= 'Z') {
            return ("", false);
        }
        buf = append(buf, byte(r));
    }    if (bang) {
        return ("", false);
    }
    return (string(buf), true);
}

} // end module_package
