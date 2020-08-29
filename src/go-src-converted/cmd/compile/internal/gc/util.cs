// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gc -- go2cs converted at 2020 August 29 09:29:52 UTC
// import "cmd/compile/internal/gc" ==> using gc = go.cmd.compile.@internal.gc_package
// Original source: C:\Go\src\cmd\compile\internal\gc\util.go
using os = go.os_package;
using runtime = go.runtime_package;
using pprof = go.runtime.pprof_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class gc_package
    {
        // Line returns n's position as a string. If n has been inlined,
        // it uses the outermost position where n has been inlined.
        private static @string Line(this ref Node n)
        {
            return linestr(n.Pos);
        }

        private static slice<Action> atExitFuncs = default;

        private static void atExit(Action f)
        {
            atExitFuncs = append(atExitFuncs, f);
        }

        public static void Exit(long code)
        {
            for (var i = len(atExitFuncs) - 1L; i >= 0L; i--)
            {
                var f = atExitFuncs[i];
                atExitFuncs = atExitFuncs[..i];
                f();
            }

            os.Exit(code);
        }

        private static @string blockprofile = default;        private static @string cpuprofile = default;        private static @string memprofile = default;        private static long memprofilerate = default;        private static @string traceprofile = default;        private static Action<@string> traceHandler = default;        private static @string mutexprofile = default;

        private static void startProfile()
        {
            if (cpuprofile != "")
            {
                var (f, err) = os.Create(cpuprofile);
                if (err != null)
                {
                    Fatalf("%v", err);
                }
                {
                    var err__prev2 = err;

                    var err = pprof.StartCPUProfile(f);

                    if (err != null)
                    {
                        Fatalf("%v", err);
                    }

                    err = err__prev2;

                }
                atExit(pprof.StopCPUProfile);
            }
            if (memprofile != "")
            {
                if (memprofilerate != 0L)
                {
                    runtime.MemProfileRate = int(memprofilerate);
                }
                (f, err) = os.Create(memprofile);
                if (err != null)
                {
                    Fatalf("%v", err);
                }
                atExit(() =>
                { 
                    // Profile all outstanding allocations.
                    runtime.GC(); 
                    // compilebench parses the memory profile to extract memstats,
                    // which are only written in the legacy pprof format.
                    // See golang.org/issue/18641 and runtime/pprof/pprof.go:writeHeap.
                    const long writeLegacyFormat = 1L;

                    {
                        var err__prev2 = err;

                        err = pprof.Lookup("heap").WriteTo(f, writeLegacyFormat);

                        if (err != null)
                        {
                            Fatalf("%v", err);
                        }

                        err = err__prev2;

                    }
                }
            else
);
            }            { 
                // Not doing memory profiling; disable it entirely.
                runtime.MemProfileRate = 0L;
            }
            if (blockprofile != "")
            {
                (f, err) = os.Create(blockprofile);
                if (err != null)
                {
                    Fatalf("%v", err);
                }
                runtime.SetBlockProfileRate(1L);
                atExit(() =>
                {
                    pprof.Lookup("block").WriteTo(f, 0L);
                    f.Close();
                });
            }
            if (mutexprofile != "")
            {
                (f, err) = os.Create(mutexprofile);
                if (err != null)
                {
                    Fatalf("%v", err);
                }
                startMutexProfiling();
                atExit(() =>
                {
                    pprof.Lookup("mutex").WriteTo(f, 0L);
                    f.Close();
                });
            }
            if (traceprofile != "" && traceHandler != null)
            {
                traceHandler(traceprofile);
            }
        }
    }
}}}}
