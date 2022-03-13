// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// TODO(rsc): Delete this file once Go 1.17 comes out and we can retire Go 1.15 support.

//go:build !go1.16
// +build !go1.16

// Package buildtag defines an Analyzer that checks build tags.

// package buildtag -- go2cs converted at 2022 March 13 06:41:47 UTC
// import "cmd/vendor/golang.org/x/tools/go/analysis/passes/buildtag" ==> using buildtag = go.cmd.vendor.golang.org.x.tools.go.analysis.passes.buildtag_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\tools\go\analysis\passes\buildtag\buildtag_old.go
namespace go.cmd.vendor.golang.org.x.tools.go.analysis.passes;

using bytes = bytes_package;
using fmt = fmt_package;
using ast = go.ast_package;
using parser = go.parser_package;
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

private static void checkGoFile(ptr<analysis.Pass> _addr_pass, ptr<ast.File> _addr_f) {
    ref analysis.Pass pass = ref _addr_pass.val;
    ref ast.File f = ref _addr_f.val;

    var pastCutoff = false;
    foreach (var (_, group) in f.Comments) { 
        // A +build comment is ignored after or adjoining the package declaration.
        if (group.End() + 1 >= f.Package) {
            pastCutoff = true;
        }
        if (!strings.HasPrefix(group.List[0].Text, "//")) {
            pastCutoff = true;
            continue;
        }
        foreach (var (_, c) in group.List) {
            if (!strings.Contains(c.Text, "+build")) {
                continue;
            }
            {
                var err = checkLine(c.Text, pastCutoff);

                if (err != null) {
                    pass.Reportf(c.Pos(), "%s", err);
                }

            }
        }
    }
}

private static error checkOtherFile(ptr<analysis.Pass> _addr_pass, @string filename) {
    ref analysis.Pass pass = ref _addr_pass.val;

    var (content, tf, err) = analysisutil.ReadFile(pass.Fset, filename);
    if (err != null) {
        return error.As(err)!;
    }
    var lines = bytes.SplitAfter(content, nl); 

    // Determine cutpoint where +build comments are no longer valid.
    // They are valid in leading // comments in the file followed by
    // a blank line.
    //
    // This must be done as a separate pass because of the
    // requirement that the comment be followed by a blank line.
    nint cutoff = default;
    {
        var i__prev1 = i;
        var line__prev1 = line;

        foreach (var (__i, __line) in lines) {
            i = __i;
            line = __line;
            line = bytes.TrimSpace(line);
            if (!bytes.HasPrefix(line, slashSlash)) {
                if (len(line) > 0) {
                    break;
                }
                cutoff = i;
            }
        }
        i = i__prev1;
        line = line__prev1;
    }

    {
        var i__prev1 = i;
        var line__prev1 = line;

        foreach (var (__i, __line) in lines) {
            i = __i;
            line = __line;
            line = bytes.TrimSpace(line);
            if (!bytes.HasPrefix(line, slashSlash)) {
                continue;
            }
            if (!bytes.Contains(line, (slice<byte>)"+build")) {
                continue;
            }
            {
                var err = checkLine(string(line), i >= cutoff);

                if (err != null) {
                    pass.Reportf(analysisutil.LineStart(tf, i + 1), "%s", err);
                    continue;
                }

            }
        }
        i = i__prev1;
        line = line__prev1;
    }

    return error.As(null!)!;
}

// checkLine checks a line that starts with "//" and contains "+build".
private static error checkLine(@string line, bool pastCutoff) {
    line = strings.TrimPrefix(line, "//");
    line = strings.TrimSpace(line);

    if (strings.HasPrefix(line, "+build")) {
        var fields = strings.Fields(line);
        if (fields[0] != "+build") { 
            // Comment is something like +buildasdf not +build.
            return error.As(fmt.Errorf("possible malformed +build comment"))!;
        }
        if (pastCutoff) {
            return error.As(fmt.Errorf("+build comment must appear before package clause and be followed by a blank line"))!;
        }
        {
            var err = checkArguments(fields);

            if (err != null) {
                return error.As(err)!;
            }

        }
    }
    else
 { 
        // Comment with +build but not at beginning.
        if (!pastCutoff) {
            return error.As(fmt.Errorf("possible malformed +build comment"))!;
        }
    }
    return error.As(null!)!;
}

private static error checkArguments(slice<@string> fields) {
    foreach (var (_, arg) in fields[(int)1..]) {
        foreach (var (_, elem) in strings.Split(arg, ",")) {
            if (strings.HasPrefix(elem, "!!")) {
                return error.As(fmt.Errorf("invalid double negative in build constraint: %s", arg))!;
            }
            elem = strings.TrimPrefix(elem, "!");
            foreach (var (_, c) in elem) {
                if (!unicode.IsLetter(c) && !unicode.IsDigit(c) && c != '_' && c != '.') {
                    return error.As(fmt.Errorf("invalid non-alphanumeric build constraint: %s", arg))!;
                }
            }
        }
    }    return error.As(null!)!;
}

private static slice<byte> nl = (slice<byte>)"\n";private static slice<byte> slashSlash = (slice<byte>)"//";

} // end buildtag_package
