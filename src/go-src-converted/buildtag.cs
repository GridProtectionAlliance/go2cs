// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package buildtag defines an Analyzer that checks build tags.
// package buildtag -- go2cs converted at 2022 March 06 23:33:51 UTC
// import "golang.org/x/tools/go/analysis/passes/buildtag" ==> using buildtag = go.golang.org.x.tools.go.analysis.passes.buildtag_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\analysis\passes\buildtag\buildtag.go
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using ast = go.go.ast_package;
using strings = go.strings_package;
using unicode = go.unicode_package;

using analysis = go.golang.org.x.tools.go.analysis_package;
using analysisutil = go.golang.org.x.tools.go.analysis.passes.@internal.analysisutil_package;

namespace go.golang.org.x.tools.go.analysis.passes;

public static partial class buildtag_package {

public static readonly @string Doc = "check that +build tags are well-formed and correctly located";



public static ptr<analysis.Analyzer> Analyzer = addr(new analysis.Analyzer(Name:"buildtag",Doc:Doc,Run:runBuildTag,));

private static (object, error) runBuildTag(ptr<analysis.Pass> _addr_pass) {
    object _p0 = default;
    error _p0 = default!;
    ref analysis.Pass pass = ref _addr_pass.val;

    foreach (var (_, f) in pass.Files) {
        checkGoFile(_addr_pass, _addr_f);
    }    foreach (var (_, name) in pass.OtherFiles) {
        {
            var err = checkOtherFile(_addr_pass, name);

            if (err != null) {
                return (null, error.As(err)!);
            }

        }

    }    return (null, error.As(null!)!);

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
    // The original version of this checker in vet could examine
    // files with malformed build tags that would cause the file to
    // be always ignored by "go build". However, drivers for the new
    // analysis API will analyze only the files selected to form a
    // package, so these checks will never fire.
    // TODO(adonovan): rethink this.

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
