// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file contains extra hooks for testing the go command.

//go:build testgo
// +build testgo

// package work -- go2cs converted at 2022 March 13 06:31:06 UTC
// import "cmd/go/internal/work" ==> using work = go.cmd.go.@internal.work_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\work\testgo.go
namespace go.cmd.go.@internal;

using cfg = cmd.go.@internal.cfg_package;
using search = cmd.go.@internal.search_package;
using fmt = fmt_package;
using os = os_package;
using filepath = path.filepath_package;
using runtime = runtime_package;
using System;

public static partial class work_package {

private static void init() {
    {
        var v = os.Getenv("TESTGO_VERSION");

        if (v != "") {
            runtimeVersion = v;
        }
    }

    {
        var testGOROOT = os.Getenv("TESTGO_GOROOT");

        if (testGOROOT != "") { 
            // Disallow installs to the GOROOT from which testgo was built.
            // Installs to other GOROOTs — such as one set explicitly within a test — are ok.
            allowInstall = a => {
                if (cfg.BuildN) {
                    return null;
                }
                var rel = search.InDir(a.Target, testGOROOT);
                if (rel == "") {
                    return null;
                }
                @string callerPos = "";
                {
                    var (_, file, line, ok) = runtime.Caller(1);

                    if (ok) {
                        {
                            var shortFile = search.InDir(file, filepath.Join(testGOROOT, "src"));

                            if (shortFile != "") {
                                file = shortFile;
                            }
                        }
                        callerPos = fmt.Sprintf("%s:%d: ", file, line);
                    }
                }
                return fmt.Errorf("%stestgo must not write to GOROOT (installing to %s)", callerPos, filepath.Join("GOROOT", rel));
            };
        }
    }
}

} // end work_package
