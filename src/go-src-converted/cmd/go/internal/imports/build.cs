// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Copied from Go distribution src/go/build/build.go, syslist.go.
// That package does not export the ability to process raw file data,
// although we could fake it with an appropriate build.Context
// and a lot of unwrapping.
// More importantly, that package does not implement the tags["*"]
// special case, in which both tag and !tag are considered to be true
// for essentially all tags (except "ignore").
//
// If we added this API to go/build directly, we wouldn't need this
// file anymore, but this API is not terribly general-purpose and we
// don't really want to commit to any public form of it, nor do we
// want to move the core parts of go/build into a top-level internal package.
// These details change very infrequently, so the copy is fine.

// package imports -- go2cs converted at 2022 March 13 06:30:09 UTC
// import "cmd/go/internal/imports" ==> using imports = go.cmd.go.@internal.imports_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\imports\build.go
namespace go.cmd.go.@internal;

using bytes = bytes_package;
using errors = errors_package;
using fmt = fmt_package;
using constraint = go.build.constraint_package;
using strings = strings_package;
using unicode = unicode_package;

public static partial class imports_package {

private static slice<byte> bSlashSlash = (slice<byte>)"//";private static slice<byte> bStarSlash = (slice<byte>)"*/";private static slice<byte> bSlashStar = (slice<byte>)"/*";private static slice<byte> bPlusBuild = (slice<byte>)"+build";private static slice<byte> goBuildComment = (slice<byte>)"//go:build";private static var errGoBuildWithoutBuild = errors.New("//go:build comment without // +build comment");private static var errMultipleGoBuild = errors.New("multiple //go:build comments");

private static bool isGoBuildComment(slice<byte> line) {
    if (!bytes.HasPrefix(line, goBuildComment)) {
        return false;
    }
    line = bytes.TrimSpace(line);
    var rest = line[(int)len(goBuildComment)..];
    return len(rest) == 0 || len(bytes.TrimSpace(rest)) < len(rest);
}

// ShouldBuild reports whether it is okay to use this file,
// The rule is that in the file's leading run of // comments
// and blank lines, which must be followed by a blank line
// (to avoid including a Go package clause doc comment),
// lines beginning with '// +build' are taken as build directives.
//
// The file is accepted only if each such line lists something
// matching the file. For example:
//
//    // +build windows linux
//
// marks the file as applicable only on Windows and Linux.
//
// If tags["*"] is true, then ShouldBuild will consider every
// build tag except "ignore" to be both true and false for
// the purpose of satisfying build tags, in order to estimate
// (conservatively) whether a file could ever possibly be used
// in any build.
//
public static bool ShouldBuild(slice<byte> content, map<@string, bool> tags) { 
    // Identify leading run of // comments and blank lines,
    // which must be followed by a blank line.
    // Also identify any //go:build comments.
    var (content, goBuild, _, err) = parseFileHeader(content);
    if (err != null) {
        return false;
    }
    bool shouldBuild = default;

    if (goBuild != null) 
        var (x, err) = constraint.Parse(string(goBuild));
        if (err != null) {
            return false;
        }
        shouldBuild = eval(x, tags, true);
    else 
        shouldBuild = true;
        var p = content;
        while (len(p) > 0) {
            var line = p;
            {
                var i = bytes.IndexByte(line, '\n');

                if (i >= 0) {
                    (line, p) = (line[..(int)i], p[(int)i + 1..]);
                }
                else
 {
                    p = p[(int)len(p)..];
                }

            }
            line = bytes.TrimSpace(line);
            if (!bytes.HasPrefix(line, bSlashSlash) || !bytes.Contains(line, bPlusBuild)) {
                continue;
            }
            var text = string(line);
            if (!constraint.IsPlusBuild(text)) {
                continue;
            }
            {
                var x__prev1 = x;

                (x, err) = constraint.Parse(text);

                if (err == null) {
                    if (!eval(x, tags, true)) {
                        shouldBuild = false;
                    }
                }

                x = x__prev1;

            }
        }
        return shouldBuild;
}

private static (slice<byte>, slice<byte>, bool, error) parseFileHeader(slice<byte> content) {
    slice<byte> trimmed = default;
    slice<byte> goBuild = default;
    bool sawBinaryOnly = default;
    error err = default!;

    nint end = 0;
    var p = content;
    var ended = false; // found non-blank, non-// line, so stopped accepting // +build lines
    var inSlashStar = false; // in /* */ comment

Lines:

    while (len(p) > 0) {
        var line = p;
        {
            var i__prev1 = i;

            var i = bytes.IndexByte(line, '\n');

            if (i >= 0) {
                (line, p) = (line[..(int)i], p[(int)i + 1..]);
            }
            else
 {
                p = p[(int)len(p)..];
            }

            i = i__prev1;

        }
        line = bytes.TrimSpace(line);
        if (len(line) == 0 && !ended) { // Blank line
            // Remember position of most recent blank line.
            // When we find the first non-blank, non-// line,
            // this "end" position marks the latest file position
            // where a // +build line can appear.
            // (It must appear _before_ a blank line before the non-blank, non-// line.
            // Yes, that's confusing, which is part of why we moved to //go:build lines.)
            // Note that ended==false here means that inSlashStar==false,
            // since seeing a /* would have set ended==true.
            end = len(content) - len(p);
            _continueLines = true;
            break;
        }
        if (!bytes.HasPrefix(line, bSlashSlash)) { // Not comment line
            ended = true;
        }
        if (!inSlashStar && isGoBuildComment(line)) {
            if (goBuild != null) {
                return (null, null, false, error.As(errMultipleGoBuild)!);
            }
            goBuild = line;
        }
Comments:
        while (len(line) > 0) {
            if (inSlashStar) {
                {
                    var i__prev2 = i;

                    i = bytes.Index(line, bStarSlash);

                    if (i >= 0) {
                        inSlashStar = false;
                        line = bytes.TrimSpace(line[(int)i + len(bStarSlash)..]);
                        _continueComments = true;
                        break;
                    }

                    i = i__prev2;

                }
                _continueLines = true;
                break;
            }
            if (bytes.HasPrefix(line, bSlashSlash)) {
                _continueLines = true;
                break;
            }
            if (bytes.HasPrefix(line, bSlashStar)) {
                inSlashStar = true;
                line = bytes.TrimSpace(line[(int)len(bSlashStar)..]);
                _continueComments = true;
                break;
            } 
            // Found non-comment text.
            _breakLines = true;
            break;
        }
    }
    return (content[..(int)end], goBuild, sawBinaryOnly, error.As(null!)!);
}

// matchTag reports whether the tag name is valid and tags[name] is true.
// As a special case, if tags["*"] is true and name is not empty or ignore,
// then matchTag will return prefer instead of the actual answer,
// which allows the caller to pretend in that case that most tags are
// both true and false.
private static bool matchTag(@string name, map<@string, bool> tags, bool prefer) { 
    // Tags must be letters, digits, underscores or dots.
    // Unlike in Go identifiers, all digits are fine (e.g., "386").
    foreach (var (_, c) in name) {
        if (!unicode.IsLetter(c) && !unicode.IsDigit(c) && c != '_' && c != '.') {
            return false;
        }
    }    if (tags["*"] && name != "" && name != "ignore") { 
        // Special case for gathering all possible imports:
        // if we put * in the tags map then all tags
        // except "ignore" are considered both present and not
        // (so we return true no matter how 'want' is set).
        return prefer;
    }
    var have = tags[name];
    if (name == "linux") {
        have = have || tags["android"];
    }
    if (name == "solaris") {
        have = have || tags["illumos"];
    }
    if (name == "darwin") {
        have = have || tags["ios"];
    }
    return have;
}

// eval is like
//    x.Eval(func(tag string) bool { return matchTag(tag, tags) })
// except that it implements the special case for tags["*"] meaning
// all tags are both true and false at the same time.
private static bool eval(constraint.Expr x, map<@string, bool> tags, bool prefer) => func((_, panic, _) => {
    switch (x.type()) {
        case ptr<constraint.TagExpr> x:
            return matchTag(x.Tag, tags, prefer);
            break;
        case ptr<constraint.NotExpr> x:
            return !eval(x.X, tags, !prefer);
            break;
        case ptr<constraint.AndExpr> x:
            return eval(x.X, tags, prefer) && eval(x.Y, tags, prefer);
            break;
        case ptr<constraint.OrExpr> x:
            return eval(x.X, tags, prefer) || eval(x.Y, tags, prefer);
            break;
    }
    panic(fmt.Sprintf("unexpected constraint expression %T", x));
});

// MatchFile returns false if the name contains a $GOOS or $GOARCH
// suffix which does not match the current system.
// The recognized name formats are:
//
//     name_$(GOOS).*
//     name_$(GOARCH).*
//     name_$(GOOS)_$(GOARCH).*
//     name_$(GOOS)_test.*
//     name_$(GOARCH)_test.*
//     name_$(GOOS)_$(GOARCH)_test.*
//
// Exceptions:
//     if GOOS=android, then files with GOOS=linux are also matched.
//     if GOOS=illumos, then files with GOOS=solaris are also matched.
//     if GOOS=ios, then files with GOOS=darwin are also matched.
//
// If tags["*"] is true, then MatchFile will consider all possible
// GOOS and GOARCH to be available and will consequently
// always return true.
public static bool MatchFile(@string name, map<@string, bool> tags) {
    if (tags["*"]) {
        return true;
    }
    {
        var dot = strings.Index(name, ".");

        if (dot != -1) {
            name = name[..(int)dot];
        }
    } 

    // Before Go 1.4, a file called "linux.go" would be equivalent to having a
    // build tag "linux" in that file. For Go 1.4 and beyond, we require this
    // auto-tagging to apply only to files with a non-empty prefix, so
    // "foo_linux.go" is tagged but "linux.go" is not. This allows new operating
    // systems, such as android, to arrive without breaking existing code with
    // innocuous source code in "android.go". The easiest fix: cut everything
    // in the name before the initial _.
    var i = strings.Index(name, "_");
    if (i < 0) {
        return true;
    }
    name = name[(int)i..]; // ignore everything before first _

    var l = strings.Split(name, "_");
    {
        var n__prev1 = n;

        var n = len(l);

        if (n > 0 && l[n - 1] == "test") {
            l = l[..(int)n - 1];
        }
        n = n__prev1;

    }
    n = len(l);
    if (n >= 2 && KnownOS[l[n - 2]] && KnownArch[l[n - 1]]) {
        return matchTag(l[n - 2], tags, true) && matchTag(l[n - 1], tags, true);
    }
    if (n >= 1 && KnownOS[l[n - 1]]) {
        return matchTag(l[n - 1], tags, true);
    }
    if (n >= 1 && KnownArch[l[n - 1]]) {
        return matchTag(l[n - 1], tags, true);
    }
    return true;
}

public static map KnownOS = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, bool>{"aix":true,"android":true,"darwin":true,"dragonfly":true,"freebsd":true,"hurd":true,"illumos":true,"ios":true,"js":true,"linux":true,"nacl":true,"netbsd":true,"openbsd":true,"plan9":true,"solaris":true,"windows":true,"zos":true,};

public static map KnownArch = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, bool>{"386":true,"amd64":true,"amd64p32":true,"arm":true,"armbe":true,"arm64":true,"arm64be":true,"ppc64":true,"ppc64le":true,"mips":true,"mipsle":true,"mips64":true,"mips64le":true,"mips64p32":true,"mips64p32le":true,"ppc":true,"riscv":true,"riscv64":true,"s390":true,"s390x":true,"sparc":true,"sparc64":true,"wasm":true,};

} // end imports_package
