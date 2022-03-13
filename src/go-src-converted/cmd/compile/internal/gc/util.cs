// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gc -- go2cs converted at 2022 March 13 06:27:56 UTC
// import "cmd/compile/internal/gc" ==> using gc = go.cmd.compile.@internal.gc_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\gc\util.go
namespace go.cmd.compile.@internal;

using os = os_package;
using runtime = runtime_package;
using pprof = runtime.pprof_package;

using @base = cmd.compile.@internal.@base_package;
using System;

public static partial class gc_package {

private static long memprofilerate = default;private static Action<@string> traceHandler = default;

private static void startProfile() {
    if (@base.Flag.CPUProfile != "") {
        var (f, err) = os.Create(@base.Flag.CPUProfile);
        if (err != null) {
            @base.Fatalf("%v", err);
        }
        {
            var err__prev2 = err;

            var err = pprof.StartCPUProfile(f);

            if (err != null) {
                @base.Fatalf("%v", err);
            }

            err = err__prev2;

        }
        @base.AtExit(pprof.StopCPUProfile);
    }
    if (@base.Flag.MemProfile != "") {
        if (memprofilerate != 0) {
            runtime.MemProfileRate = int(memprofilerate);
        }
        (f, err) = os.Create(@base.Flag.MemProfile);
        if (err != null) {
            @base.Fatalf("%v", err);
        }
        @base.AtExit(() => { 
            // Profile all outstanding allocations.
            runtime.GC(); 
            // compilebench parses the memory profile to extract memstats,
            // which are only written in the legacy pprof format.
            // See golang.org/issue/18641 and runtime/pprof/pprof.go:writeHeap.
            const nint writeLegacyFormat = 1;

            {
                var err__prev2 = err;

                err = pprof.Lookup("heap").WriteTo(f, writeLegacyFormat);

                if (err != null) {
                    @base.Fatalf("%v", err);
                }

                err = err__prev2;

            }
        }
    else
);
    } { 
        // Not doing memory profiling; disable it entirely.
        runtime.MemProfileRate = 0;
    }
    if (@base.Flag.BlockProfile != "") {
        (f, err) = os.Create(@base.Flag.BlockProfile);
        if (err != null) {
            @base.Fatalf("%v", err);
        }
        runtime.SetBlockProfileRate(1);
        @base.AtExit(() => {
            pprof.Lookup("block").WriteTo(f, 0);
            f.Close();
        });
    }
    if (@base.Flag.MutexProfile != "") {
        (f, err) = os.Create(@base.Flag.MutexProfile);
        if (err != null) {
            @base.Fatalf("%v", err);
        }
        startMutexProfiling();
        @base.AtExit(() => {
            pprof.Lookup("mutex").WriteTo(f, 0);
            f.Close();
        });
    }
    if (@base.Flag.TraceProfile != "" && traceHandler != null) {
        traceHandler(@base.Flag.TraceProfile);
    }
}

} // end gc_package
