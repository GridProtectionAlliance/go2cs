// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using fmt = fmt_package;
using ast = go.ast_package;
using token = go.token_package;
using version = go.version_package;
using goversion = @internal.goversion_package;
using @internal;
using ꓸꓸꓸany = Span<any>;

partial class types_package {

[GoType("@string")] partial struct goVersion;

// asGoVersion returns v as a goVersion (e.g., "go1.20.1" becomes "go1.20").
// If v is not a valid Go version, the result is the empty string.
internal static goVersion asGoVersion(@string v) {
    return ((goVersion)version.Lang(v));
}

// isValid reports whether v is a valid Go version.
internal static bool isValid(this goVersion v) {
    return v != ""u8;
}

// cmp returns -1, 0, or +1 depending on whether x < y, x == y, or x > y,
// interpreted as Go versions.
internal static nint cmp(this goVersion x, goVersion y) {
    return version.Compare(((@string)x), ((@string)y));
}

internal static goVersion go1_9 = asGoVersion("go1.9"u8);
internal static goVersion go1_13 = asGoVersion("go1.13"u8);
internal static goVersion go1_14 = asGoVersion("go1.14"u8);
internal static goVersion go1_17 = asGoVersion("go1.17"u8);
internal static goVersion go1_18 = asGoVersion("go1.18"u8);
internal static goVersion go1_20 = asGoVersion("go1.20"u8);
internal static goVersion go1_21 = asGoVersion("go1.21"u8);
internal static goVersion go1_22 = asGoVersion("go1.22"u8);
internal static goVersion go1_23 = asGoVersion("go1.23"u8);
internal static goVersion go_current = asGoVersion(fmt.Sprintf("go1.%d"u8, goversion.Version));

// allowVersion reports whether the current package at the given position
// is allowed to use version v. If the position is unknown, the specified
// module version (Config.GoVersion) is used. If that version is invalid,
// allowVersion returns true.
[GoRecv] internal static bool allowVersion(this ref Checker check, positioner at, goVersion v) {
    @string fileVersion = check.conf.GoVersion;
    {
        tokenꓸPos pos = at.Pos(); if (pos.IsValid()) {
            fileVersion = check.versions[check.fileFor(pos)];
        }
    }
    // We need asGoVersion (which calls version.Lang) below
    // because fileVersion may be the (unaltered) Config.GoVersion
    // string which may contain dot-release information.
    @string version = asGoVersion(fileVersion);
    return !version.isValid() || version.cmp(v) >= 0;
}

// verifyVersionf is like allowVersion but also accepts a format string and arguments
// which are used to report a version error if allowVersion returns false. It uses the
// current package.
[GoRecv] internal static bool verifyVersionf(this ref Checker check, positioner at, goVersion v, @string format, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.slice();

    if (!check.allowVersion(at, v)) {
        check.versionErrorf(at, v, format, args.ꓸꓸꓸ);
        return false;
    }
    return true;
}

// TODO(gri) Consider a more direct (position-independent) mechanism
//           to identify which file we're in so that version checks
//           work correctly in the absence of correct position info.

// fileFor returns the *ast.File which contains the position pos.
// If there are no files, the result is nil.
// The position must be valid.
[GoRecv] internal static ж<ast.File> fileFor(this ref Checker check, tokenꓸPos pos) {
    assert(pos.IsValid());
    // Eval and CheckExpr tests may not have any source files.
    if (len(check.files) == 0) {
        return default!;
    }
    foreach (var (_, file) in check.files) {
        if ((~file).FileStart <= pos && pos < (~file).FileEnd) {
            return file;
        }
    }
    throw panic(check.sprintf("file not found for pos = %d (%s)"u8, ((nint)pos), check.fset.Position(pos)));
}

} // end types_package
