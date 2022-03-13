// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build go1.16
// +build go1.16

// Package buildtag defines an Analyzer that checks build tags.

// package buildtag -- go2cs converted at 2022 March 13 06:41:47 UTC
// import "cmd/vendor/golang.org/x/tools/go/analysis/passes/buildtag" ==> using buildtag = go.cmd.vendor.golang.org.x.tools.go.analysis.passes.buildtag_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\tools\go\analysis\passes\buildtag\buildtag.go
namespace go.cmd.vendor.golang.org.x.tools.go.analysis.passes;

using ast = go.ast_package;
using constraint = go.build.constraint_package;
using parser = go.parser_package;
using token = go.token_package;
using strings = strings_package;
using unicode = unicode_package;

using analysis = golang.org.x.tools.go.analysis_package;
using analysisutil = golang.org.x.tools.go.analysis.passes.@internal.analysisutil_package;

public static partial class buildtag_package {

public static readonly @string Doc = "check that +build tags are well-formed and correctly located";



public static ptr<analysis.Analyzer> Analyzer = addr(new analysis.Analyzer(Name:"buildtag",Doc:Doc,Run:runBuildTag,));

private static (object, error) runBuildTag(ptr<analysis.Pass> _addr_pass) {
    object _p0 = default;
    error _p0 = default!;
    ref analysis.Pass pass = ref _addr_pass.val;

    {
        var f__prev1 = f;

        foreach (var (_, __f) in pass.Files) {
            f = __f;
            checkGoFile(_addr_pass, _addr_f);
        }
        f = f__prev1;
    }

    {
        var name__prev1 = name;

        foreach (var (_, __name) in pass.OtherFiles) {
            name = __name;
            {
                var err__prev1 = err;

                var err = checkOtherFile(_addr_pass, name);

                if (err != null) {
                    return (null, error.As(err)!);
                }

                err = err__prev1;

            }
        }
        name = name__prev1;
    }

    {
        var name__prev1 = name;

        foreach (var (_, __name) in pass.IgnoredFiles) {
            name = __name;
            if (strings.HasSuffix(name, ".go")) {
                var (f, err) = parser.ParseFile(pass.Fset, name, null, parser.ParseComments);
                if (err != null) { 
                    // Not valid Go source code - not our job to diagnose, so ignore.
                    return (null, error.As(null!)!);
                }
                checkGoFile(_addr_pass, _addr_f);
            }
            else
 {
                {
                    var err__prev2 = err;

                    err = checkOtherFile(_addr_pass, name);

                    if (err != null) {
                        return (null, error.As(err)!);
                    }

                    err = err__prev2;

                }
            }
        }
        name = name__prev1;
    }

    return (null, error.As(null!)!);
}

private static void checkGoFile(ptr<analysis.Pass> _addr_pass, ptr<ast.File> _addr_f) => func((defer, _, _) => {
    ref analysis.Pass pass = ref _addr_pass.val;
    ref ast.File f = ref _addr_f.val;

    checker check = default;
    check.init(pass);
    defer(check.finish());

    foreach (var (_, group) in f.Comments) { 
        // A +build comment is ignored after or adjoining the package declaration.
        if (group.End() + 1 >= f.Package) {
            check.plusBuildOK = false;
        }
        if (group.Pos() >= f.Package) {
            check.goBuildOK = false;
        }
        foreach (var (_, c) in group.List) { 
            // "+build" is ignored within or after a /*...*/ comment.
            if (!strings.HasPrefix(c.Text, "//")) {
                check.plusBuildOK = false;
            }
            check.comment(c.Slash, c.Text);
        }
    }
});

private static error checkOtherFile(ptr<analysis.Pass> _addr_pass, @string filename) => func((defer, _, _) => {
    ref analysis.Pass pass = ref _addr_pass.val;

    checker check = default;
    check.init(pass);
    defer(check.finish()); 

    // We cannot use the Go parser, since this may not be a Go source file.
    // Read the raw bytes instead.
    var (content, tf, err) = analysisutil.ReadFile(pass.Fset, filename);
    if (err != null) {
        return error.As(err)!;
    }
    check.file(token.Pos(tf.Base()), string(content));
    return error.As(null!)!;
});

private partial struct checker {
    public ptr<analysis.Pass> pass;
    public bool plusBuildOK; // "+build" lines still OK
    public bool goBuildOK; // "go:build" lines still OK
    public bool crossCheck; // cross-check go:build and +build lines when done reading file
    public bool inStar; // currently in a /* */ comment
    public token.Pos goBuildPos; // position of first go:build line found
    public token.Pos plusBuildPos; // position of first "+build" line found
    public constraint.Expr goBuild; // go:build constraint found
    public constraint.Expr plusBuild; // AND of +build constraints found
}

private static void init(this ptr<checker> _addr_check, ptr<analysis.Pass> _addr_pass) {
    ref checker check = ref _addr_check.val;
    ref analysis.Pass pass = ref _addr_pass.val;

    check.pass = pass;
    check.goBuildOK = true;
    check.plusBuildOK = true;
    check.crossCheck = true;
}

private static void file(this ptr<checker> _addr_check, token.Pos pos, @string text) {
    ref checker check = ref _addr_check.val;
 
    // Determine cutpoint where +build comments are no longer valid.
    // They are valid in leading // comments in the file followed by
    // a blank line.
    //
    // This must be done as a separate pass because of the
    // requirement that the comment be followed by a blank line.
    nint plusBuildCutoff = default;
    var fullText = text;
    while (text != "") {
        var i = strings.Index(text, "\n");
        if (i < 0) {
            i = len(text);
        }
        else
 {
            i++;
        }
        var offset = len(fullText) - len(text);
        var line = text[..(int)i];
        text = text[(int)i..];
        line = strings.TrimSpace(line);
        if (!strings.HasPrefix(line, "//") && line != "") {
            break;
        }
        if (line == "") {
            plusBuildCutoff = offset;
        }
    } 

    // Process each line.
    // Must stop once we hit goBuildOK == false
    text = fullText;
    check.inStar = false;
    while (text != "") {
        i = strings.Index(text, "\n");
        if (i < 0) {
            i = len(text);
        }
        else
 {
            i++;
        }
        offset = len(fullText) - len(text);
        line = text[..(int)i];
        text = text[(int)i..];
        check.plusBuildOK = offset < plusBuildCutoff;

        if (strings.HasPrefix(line, "//")) {
            check.comment(pos + token.Pos(offset), line);
            continue;
        }
        while (true) {
            line = strings.TrimSpace(line);
            if (check.inStar) {
                i = strings.Index(line, "*/");
                if (i < 0) {
                    line = "";
                    break;
                }
                line = line[(int)i + len("*/")..];
                check.inStar = false;
                continue;
            }
            if (strings.HasPrefix(line, "/*")) {
                check.inStar = true;
                line = line[(int)len("/*")..];
                continue;
            }
            break;
        }
        if (line != "") { 
            // Found non-comment non-blank line.
            // Ends space for valid //go:build comments,
            // but also ends the fraction of the file we can
            // reliably parse. From this point on we might
            // incorrectly flag "comments" inside multiline
            // string constants or anything else (this might
            // not even be a Go program). So stop.
            break;
        }
    }
}

private static void comment(this ptr<checker> _addr_check, token.Pos pos, @string text) {
    ref checker check = ref _addr_check.val;

    if (strings.HasPrefix(text, "//")) {
        if (strings.Contains(text, "+build")) {
            check.plusBuildLine(pos, text);
        }
        if (strings.Contains(text, "//go:build")) {
            check.goBuildLine(pos, text);
        }
    }
    if (strings.HasPrefix(text, "/*")) {
        {
            var i__prev2 = i;

            var i = strings.Index(text, "\n");

            if (i >= 0) { 
                // multiline /* */ comment - process interior lines
                check.inStar = true;
                i++;
                pos += token.Pos(i);
                text = text[(int)i..];
                while (text != "") {
                    i = strings.Index(text, "\n");
                    if (i < 0) {
                        i = len(text);
                    }
                    else
 {
                        i++;
                    }
                    var line = text[..(int)i];
                    if (strings.HasPrefix(line, "//")) {
                        check.comment(pos, line);
                    }
                    pos += token.Pos(i);
                    text = text[(int)i..];
                }

                check.inStar = false;
            }

            i = i__prev2;

        }
    }
}

private static void goBuildLine(this ptr<checker> _addr_check, token.Pos pos, @string line) {
    ref checker check = ref _addr_check.val;

    if (!constraint.IsGoBuild(line)) {
        if (!strings.HasPrefix(line, "//go:build") && constraint.IsGoBuild("//" + strings.TrimSpace(line[(int)len("//")..]))) {
            check.pass.Reportf(pos, "malformed //go:build line (space between // and go:build)");
        }
        return ;
    }
    if (!check.goBuildOK || check.inStar) {
        check.pass.Reportf(pos, "misplaced //go:build comment");
        check.crossCheck = false;
        return ;
    }
    if (check.goBuildPos == token.NoPos) {
        check.goBuildPos = pos;
    }
    else
 {
        check.pass.Reportf(pos, "unexpected extra //go:build line");
        check.crossCheck = false;
    }
    {
        var i = strings.Index(line, " // ERROR ");

        if (i >= 0) {
            line = line[..(int)i];
        }
    }

    var (x, err) = constraint.Parse(line);
    if (err != null) {
        check.pass.Reportf(pos, "%v", err);
        check.crossCheck = false;
        return ;
    }
    if (check.goBuild == null) {
        check.goBuild = x;
    }
}

private static void plusBuildLine(this ptr<checker> _addr_check, token.Pos pos, @string line) {
    ref checker check = ref _addr_check.val;

    line = strings.TrimSpace(line);
    if (!constraint.IsPlusBuild(line)) { 
        // Comment with +build but not at beginning.
        // Only report early in file.
        if (check.plusBuildOK && !strings.HasPrefix(line, "// want")) {
            check.pass.Reportf(pos, "possible malformed +build comment");
        }
        return ;
    }
    if (!check.plusBuildOK) { // inStar implies !plusBuildOK
        check.pass.Reportf(pos, "misplaced +build comment");
        check.crossCheck = false;
    }
    if (check.plusBuildPos == token.NoPos) {
        check.plusBuildPos = pos;
    }
    {
        var i = strings.Index(line, " // ERROR ");

        if (i >= 0) {
            line = line[..(int)i];
        }
    }

    var fields = strings.Fields(line[(int)len("//")..]); 
    // IsPlusBuildConstraint check above implies fields[0] == "+build"
    foreach (var (_, arg) in fields[(int)1..]) {
        foreach (var (_, elem) in strings.Split(arg, ",")) {
            if (strings.HasPrefix(elem, "!!")) {
                check.pass.Reportf(pos, "invalid double negative in build constraint: %s", arg);
                check.crossCheck = false;
                continue;
            }
            elem = strings.TrimPrefix(elem, "!");
            foreach (var (_, c) in elem) {
                if (!unicode.IsLetter(c) && !unicode.IsDigit(c) && c != '_' && c != '.') {
                    check.pass.Reportf(pos, "invalid non-alphanumeric build constraint: %s", arg);
                    check.crossCheck = false;
                    break;
                }
            }
        }
    }    if (check.crossCheck) {
        var (y, err) = constraint.Parse(line);
        if (err != null) { 
            // Should never happen - constraint.Parse never rejects a // +build line.
            // Also, we just checked the syntax above.
            // Even so, report.
            check.pass.Reportf(pos, "%v", err);
            check.crossCheck = false;
            return ;
        }
        if (check.plusBuild == null) {
            check.plusBuild = y;
        }
        else
 {
            check.plusBuild = addr(new constraint.AndExpr(X:check.plusBuild,Y:y));
        }
    }
}

private static void finish(this ptr<checker> _addr_check) {
    ref checker check = ref _addr_check.val;

    if (!check.crossCheck || check.plusBuildPos == token.NoPos || check.goBuildPos == token.NoPos) {
        return ;
    }
    constraint.Expr want = default;
    var (lines, err) = constraint.PlusBuildLines(check.goBuild);
    if (err != null) {
        check.pass.Reportf(check.goBuildPos, "%v", err);
        return ;
    }
    foreach (var (_, line) in lines) {
        var (y, err) = constraint.Parse(line);
        if (err != null) { 
            // Definitely should not happen, but not the user's fault.
            // Do not report.
            return ;
        }
        if (want == null) {
            want = y;
        }
        else
 {
            want = addr(new constraint.AndExpr(X:want,Y:y));
        }
    }    if (want.String() != check.plusBuild.String()) {
        check.pass.Reportf(check.plusBuildPos, "+build lines do not match //go:build condition");
        return ;
    }
}

} // end buildtag_package
