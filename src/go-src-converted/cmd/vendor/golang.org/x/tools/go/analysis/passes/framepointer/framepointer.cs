// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package framepointer defines an Analyzer that reports assembly code
// that clobbers the frame pointer before saving it.
// package framepointer -- go2cs converted at 2022 March 06 23:34:35 UTC
// import "cmd/vendor/golang.org/x/tools/go/analysis/passes/framepointer" ==> using framepointer = go.cmd.vendor.golang.org.x.tools.go.analysis.passes.framepointer_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\tools\go\analysis\passes\framepointer\framepointer.go
using build = go.go.build_package;
using regexp = go.regexp_package;
using strings = go.strings_package;

using analysis = go.golang.org.x.tools.go.analysis_package;
using analysisutil = go.golang.org.x.tools.go.analysis.passes.@internal.analysisutil_package;

namespace go.cmd.vendor.golang.org.x.tools.go.analysis.passes;

public static partial class framepointer_package {

public static readonly @string Doc = "report assembly that clobbers the frame pointer before saving it";



public static ptr<analysis.Analyzer> Analyzer = addr(new analysis.Analyzer(Name:"framepointer",Doc:Doc,Run:run,));

private static var re = regexp.MustCompile;private static var asmWriteBP = re(",\\s*BP$");private static var asmMentionBP = re("\\bBP\\b");private static var asmControlFlow = re("^(J|RET)");

private static (object, error) run(ptr<analysis.Pass> _addr_pass) {
    object _p0 = default;
    error _p0 = default!;
    ref analysis.Pass pass = ref _addr_pass.val;

    if (build.Default.GOARCH != "amd64") { // TODO: arm64 also?
        return (null, error.As(null!)!);

    }
    if (build.Default.GOOS != "linux" && build.Default.GOOS != "darwin") {
        return (null, error.As(null!)!);
    }
    slice<@string> sfiles = default;
    {
        var fname__prev1 = fname;

        foreach (var (_, __fname) in pass.OtherFiles) {
            fname = __fname;
            if (strings.HasSuffix(fname, ".s") && pass.Pkg.Path() != "runtime") {
                sfiles = append(sfiles, fname);
            }
        }
        fname = fname__prev1;
    }

    {
        var fname__prev1 = fname;

        foreach (var (_, __fname) in sfiles) {
            fname = __fname;
            var (content, tf, err) = analysisutil.ReadFile(pass.Fset, fname);
            if (err != null) {
                return (null, error.As(err)!);
            }
            var lines = strings.SplitAfter(string(content), "\n");
            var active = false;
            foreach (var (lineno, line) in lines) {
                lineno++; 

                // Ignore comments and commented-out code.
                {
                    var i = strings.Index(line, "//");

                    if (i >= 0) {
                        line = line[..(int)i];
                    }

                }

                line = strings.TrimSpace(line); 

                // We start checking code at a TEXT line for a frameless function.
                if (strings.HasPrefix(line, "TEXT") && strings.Contains(line, "(SB)") && strings.Contains(line, "$0")) {
                    active = true;
                    continue;
                }

                if (!active) {
                    continue;
                }

                if (asmWriteBP.MatchString(line)) { // clobber of BP, function is not OK
                    pass.Reportf(analysisutil.LineStart(tf, lineno), "frame pointer is clobbered before saving");
                    active = false;
                    continue;

                }

                if (asmMentionBP.MatchString(line)) { // any other use of BP might be a read, so function is OK
                    active = false;
                    continue;

                }

                if (asmControlFlow.MatchString(line)) { // give up after any branch instruction
                    active = false;
                    continue;

                }

            }

        }
        fname = fname__prev1;
    }

    return (null, error.As(null!)!);

}

} // end framepointer_package
