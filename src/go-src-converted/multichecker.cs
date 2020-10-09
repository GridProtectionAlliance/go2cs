// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package multichecker defines the main function for an analysis driver
// with several analyzers. This package makes it easy for anyone to build
// an analysis tool containing just the analyzers they need.
// package multichecker -- go2cs converted at 2020 October 09 06:02:36 UTC
// import "golang.org/x/tools/go/analysis/multichecker" ==> using multichecker = go.golang.org.x.tools.go.analysis.multichecker_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\analysis\multichecker\multichecker.go
using flag = go.flag_package;
using fmt = go.fmt_package;
using log = go.log_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using strings = go.strings_package;

using analysis = go.golang.org.x.tools.go.analysis_package;
using analysisflags = go.golang.org.x.tools.go.analysis.@internal.analysisflags_package;
using checker = go.golang.org.x.tools.go.analysis.@internal.checker_package;
using unitchecker = go.golang.org.x.tools.go.analysis.unitchecker_package;
using static go.builtin;

namespace go {
namespace golang.org {
namespace x {
namespace tools {
namespace go {
namespace analysis
{
    public static partial class multichecker_package
    {
        public static void Main(params ptr<ptr<analysis.Analyzer>>[] _addr_analyzers) => func((_, panic, __) =>
        {
            analyzers = analyzers.Clone();
            ref analysis.Analyzer analyzers = ref _addr_analyzers.val;

            var progname = filepath.Base(os.Args[0L]);
            log.SetFlags(0L);
            log.SetPrefix(progname + ": "); // e.g. "vet: "

            {
                var err = analysis.Validate(analyzers);

                if (err != null)
                {
                    log.Fatal(err);
                }
            }


            checker.RegisterFlags();

            analyzers = analysisflags.Parse(analyzers, true);

            var args = flag.Args();
            if (len(args) == 0L)
            {
                fmt.Fprintf(os.Stderr, "%[1]s is a tool for static analysis of Go programs.\n\nUsage: %[1]s [-flag] [packag" +
    "e]\n\nRun \'%[1]s help\' for more detail,\n or \'%[1]s help name\' for details and flag" +
    "s of a specific analyzer.\n", progname);
                os.Exit(1L);

            }
            if (args[0L] == "help")
            {
                analysisflags.Help(progname, analyzers, args[1L..]);
                os.Exit(0L);
            }
            if (len(args) == 1L && strings.HasSuffix(args[0L], ".cfg"))
            {
                unitchecker.Run(args[0L], analyzers);
                panic("unreachable");
            }
            os.Exit(checker.Run(args, analyzers));

        });
    }
}}}}}}
