// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build go1.7

// package gc -- go2cs converted at 2020 August 29 09:29:30 UTC
// import "cmd/compile/internal/gc" ==> using gc = go.cmd.compile.@internal.gc_package
// Original source: C:\Go\src\cmd\compile\internal\gc\trace.go
using os = go.os_package;
using tracepkg = go.runtime.trace_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class gc_package
    {
        private static void init()
        {
            traceHandler = traceHandlerGo17;
        }

        private static void traceHandlerGo17(@string traceprofile)
        {
            var (f, err) = os.Create(traceprofile);
            if (err != null)
            {
                Fatalf("%v", err);
            }
            {
                var err = tracepkg.Start(f);

                if (err != null)
                {
                    Fatalf("%v", err);
                }

            }
            atExit(tracepkg.Stop);
        }
    }
}}}}
