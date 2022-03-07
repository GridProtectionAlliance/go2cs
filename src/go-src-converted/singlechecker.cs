// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package singlechecker defines the main function for an analysis
// driver with only a single analysis.
// This package makes it easy for a provider of an analysis package to
// also provide a standalone tool that runs just that analysis.
//
// For example, if example.org/findbadness is an analysis package,
// all that is needed to define a standalone tool is a file,
// example.org/findbadness/cmd/findbadness/main.go, containing:
//
//      // The findbadness command runs an analysis.
//     package main
//
//     import (
//         "example.org/findbadness"
//         "golang.org/x/tools/go/analysis/singlechecker"
//     )
//
//     func main() { singlechecker.Main(findbadness.Analyzer) }
//
// package singlechecker -- go2cs converted at 2022 March 06 23:34:21 UTC
// import "golang.org/x/tools/go/analysis/singlechecker" ==> using singlechecker = go.golang.org.x.tools.go.analysis.singlechecker_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\analysis\singlechecker\singlechecker.go
using flag = go.flag_package;
using fmt = go.fmt_package;
using log = go.log_package;
using os = go.os_package;
using strings = go.strings_package;

using analysis = go.golang.org.x.tools.go.analysis_package;
using analysisflags = go.golang.org.x.tools.go.analysis.@internal.analysisflags_package;
using checker = go.golang.org.x.tools.go.analysis.@internal.checker_package;
using unitchecker = go.golang.org.x.tools.go.analysis.unitchecker_package;
using System;


namespace go.golang.org.x.tools.go.analysis;

public static partial class singlechecker_package {

    // Main is the main function for a checker command for a single analysis.
public static void Main(ptr<analysis.Analyzer> _addr_a) => func((_, panic, _) => {
    ref analysis.Analyzer a = ref _addr_a.val;

    log.SetFlags(0);
    log.SetPrefix(a.Name + ": ");

    ptr<analysis.Analyzer> analyzers = new slice<ptr<analysis.Analyzer>>(new ptr<analysis.Analyzer>[] { a });

    {
        var err = analysis.Validate(analyzers);

        if (err != null) {
            log.Fatal(err);
        }
    }


    checker.RegisterFlags();

    flag.Usage = () => {
        var paras = strings.Split(a.Doc, "\n\n");
        fmt.Fprintf(os.Stderr, "%s: %s\n\n", a.Name, paras[0]);
        fmt.Fprintf(os.Stderr, "Usage: %s [-flag] [package]\n\n", a.Name);
        if (len(paras) > 1) {
            fmt.Fprintln(os.Stderr, strings.Join(paras[(int)1..], "\n\n"));
        }
        fmt.Fprintln(os.Stderr, "\nFlags:");
        flag.PrintDefaults();

    };

    analyzers = analysisflags.Parse(analyzers, false);

    var args = flag.Args();
    if (len(args) == 0) {
        flag.Usage();
        os.Exit(1);
    }
    if (len(args) == 1 && strings.HasSuffix(args[0], ".cfg")) {
        unitchecker.Run(args[0], analyzers);
        panic("unreachable");
    }
    os.Exit(checker.Run(args, analyzers));

});

} // end singlechecker_package
