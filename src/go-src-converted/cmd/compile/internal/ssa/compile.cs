// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2020 August 29 08:53:28 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Go\src\cmd\compile\internal\ssa\compile.go
using objabi = go.cmd.@internal.objabi_package;
using src = go.cmd.@internal.src_package;
using fmt = go.fmt_package;
using log = go.log_package;
using os = go.os_package;
using regexp = go.regexp_package;
using runtime = go.runtime_package;
using strings = go.strings_package;
using time = go.time_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class ssa_package
    {
        // Compile is the main entry point for this package.
        // Compile modifies f so that on return:
        //   · all Values in f map to 0 or 1 assembly instructions of the target architecture
        //   · the order of f.Blocks is the order to emit the Blocks
        //   · the order of b.Values is the order to emit the Values in each Block
        //   · f has a non-nil regAlloc field
        public static void Compile(ref Func _f) => func(_f, (ref Func f, Defer defer, Panic _, Recover __) =>
        { 
            // TODO: debugging - set flags to control verbosity of compiler,
            // which phases to dump IR before/after, etc.
            if (f.Log())
            {
                f.Logf("compiling %s\n", f.Name);
            }
            @string phaseName = "init";
            defer(() =>
            {
                if (phaseName != "")
                {
                    var err = recover();
                    var stack = make_slice<byte>(16384L);
                    var n = runtime.Stack(stack, false);
                    stack = stack[..n];
                    f.Fatalf("panic during %s while compiling %s:\n\n%v\n\n%s\n", phaseName, f.Name, err, stack);
                }
            }()); 

            // Run all the passes
            printFunc(f);
            f.HTMLWriter.WriteFunc("start", f);
            if (BuildDump != "" && BuildDump == f.Name)
            {
                f.dumpFile("build");
            }
            if (checkEnabled)
            {
                checkFunc(f);
            }
            const var logMemStats = false;

            foreach (var (_, p) in passes)
            {
                if (!f.Config.optimize && !p.required || p.disabled)
                {
                    continue;
                }
                f.pass = ref p;
                phaseName = p.name;
                if (f.Log())
                {
                    f.Logf("  pass %s begin\n", p.name);
                }
                runtime.MemStats mStart = default;
                if (logMemStats || p.mem)
                {
                    runtime.ReadMemStats(ref mStart);
                }
                var tStart = time.Now();
                p.fn(f);
                var tEnd = time.Now(); 

                // Need something less crude than "Log the whole intermediate result".
                if (f.Log() || f.HTMLWriter != null)
                {
                    var time = tEnd.Sub(tStart).Nanoseconds();
                    @string stats = default;
                    if (logMemStats)
                    {
                        runtime.MemStats mEnd = default;
                        runtime.ReadMemStats(ref mEnd);
                        var nBytes = mEnd.TotalAlloc - mStart.TotalAlloc;
                        var nAllocs = mEnd.Mallocs - mStart.Mallocs;
                        stats = fmt.Sprintf("[%d ns %d allocs %d bytes]", time, nAllocs, nBytes);
                    }
                    else
                    {
                        stats = fmt.Sprintf("[%d ns]", time);
                    }
                    f.Logf("  pass %s end %s\n", p.name, stats);
                    printFunc(f);
                    f.HTMLWriter.WriteFunc(fmt.Sprintf("after %s <span class=\"stats\">%s</span>", phaseName, stats), f);
                }
                if (p.time || p.mem)
                { 
                    // Surround timing information w/ enough context to allow comparisons.
                    time = tEnd.Sub(tStart).Nanoseconds();
                    if (p.time)
                    {
                        f.LogStat("TIME(ns)", time);
                    }
                    if (p.mem)
                    {
                        mEnd = default;
                        runtime.ReadMemStats(ref mEnd);
                        nBytes = mEnd.TotalAlloc - mStart.TotalAlloc;
                        nAllocs = mEnd.Mallocs - mStart.Mallocs;
                        f.LogStat("TIME(ns):BYTES:ALLOCS", time, nBytes, nAllocs);
                    }
                }
                if (p.dump != null && p.dump[f.Name])
                { 
                    // Dump function to appropriately named file
                    f.dumpFile(phaseName);
                }
                if (checkEnabled)
                {
                    checkFunc(f);
                }
            }            phaseName = "";
        });

        // TODO: should be a config field
        private static long dumpFileSeq = default;

        // dumpFile creates a file from the phase name and function name
        // Dumping is done to files to avoid buffering huge strings before
        // output.
        private static void dumpFile(this ref Func f, @string phaseName)
        {
            dumpFileSeq++;
            var fname = fmt.Sprintf("%s_%02d__%s.dump", f.Name, dumpFileSeq, phaseName);
            fname = strings.Replace(fname, " ", "_", -1L);
            fname = strings.Replace(fname, "/", "_", -1L);
            fname = strings.Replace(fname, ":", "_", -1L);

            var (fi, err) = os.Create(fname);
            if (err != null)
            {
                f.Warnl(src.NoXPos, "Unable to create after-phase dump file %s", fname);
                return;
            }
            stringFuncPrinter p = new stringFuncPrinter(w:fi);
            fprintFunc(p, f);
            fi.Close();
        }

        private partial struct pass
        {
            public @string name;
            public Action<ref Func> fn;
            public bool required;
            public bool disabled;
            public bool time; // report time to run pass
            public bool mem; // report mem stats to run pass
            public long stats; // pass reports own "stats" (e.g., branches removed)
            public long debug; // pass performs some debugging. =1 should be in error-testing-friendly Warnl format.
            public long test; // pass-specific ad-hoc option, perhaps useful in development
            public map<@string, bool> dump; // dump if function name matches
        }

        private static void addDump(this ref pass p, @string s)
        {
            if (p.dump == null)
            {
                p.dump = make_map<@string, bool>();
            }
            p.dump[s] = true;
        }

        // Run consistency checker between each phase
        private static var checkEnabled = false;

        // Debug output
        public static long IntrinsicsDebug = default;
        public static bool IntrinsicsDisable = default;

        public static long BuildDebug = default;
        public static long BuildTest = default;
        public static long BuildStats = default;
        public static @string BuildDump = default; // name of function to dump after initial build of ssa

        // PhaseOption sets the specified flag in the specified ssa phase,
        // returning empty string if this was successful or a string explaining
        // the error if it was not.
        // A version of the phase name with "_" replaced by " " is also checked for a match.
        // If the phase name begins a '~' then the rest of the underscores-replaced-with-blanks
        // version is used as a regular expression to match the phase name(s).
        //
        // Special cases that have turned out to be useful:
        //  ssa/check/on enables checking after each phase
        //  ssa/all/time enables time reporting for all phases
        //
        // See gc/lex.go for dissection of the option string.
        // Example uses:
        //
        // GO_GCFLAGS=-d=ssa/generic_cse/time,ssa/generic_cse/stats,ssa/generic_cse/debug=3 ./make.bash
        //
        // BOOT_GO_GCFLAGS=-d='ssa/~^.*scc$/off' GO_GCFLAGS='-d=ssa/~^.*scc$/off' ./make.bash
        //
        public static @string PhaseOption(@string phase, @string flag, long val, @string valString)
        {
            if (phase == "help")
            {
                long lastcr = 0L;
                @string phasenames = "check, all, build, intrinsics";
                {
                    var p__prev1 = p;

                    foreach (var (_, __p) in passes)
                    {
                        p = __p;
                        var pn = strings.Replace(p.name, " ", "_", -1L);
                        if (len(pn) + len(phasenames) - lastcr > 70L)
                        {
                            phasenames += "\n";
                            lastcr = len(phasenames);
                            phasenames += pn;
                        }
                        else
                        {
                            phasenames += ", " + pn;
                        }
                    }

                    p = p__prev1;
                }

                return "" + "GcFlag -d=ssa/<phase>/<flag>[=<value>|<function_name>]\n<phase> is one of:\n" + phasenames + @"
<flag> is one of on, off, debug, mem, time, test, stats, dump
<value> defaults to 1
<function_name> is required for ""dump"", specifies name of function to dump after <phase>
Except for dump, output is directed to standard out; dump appears in a file.
Phase ""all"" supports flags ""time"", ""mem"", and ""dump"".
Phases ""intrinsics"" supports flags ""on"", ""off"", and ""debug"".
Interpretation of the ""debug"" value depends on the phase.
Dump files are named <phase>__<function_name>_<seq>.dump.
";
            }
            if (phase == "check" && flag == "on")
            {
                checkEnabled = val != 0L;
                return "";
            }
            if (phase == "check" && flag == "off")
            {
                checkEnabled = val == 0L;
                return "";
            }
            var alltime = false;
            var allmem = false;
            var alldump = false;
            if (phase == "all")
            {
                if (flag == "time")
                {
                    alltime = val != 0L;
                }
                else if (flag == "mem")
                {
                    allmem = val != 0L;
                }
                else if (flag == "dump")
                {
                    alldump = val != 0L;
                    if (alldump)
                    {
                        BuildDump = valString;
                    }
                }
                else
                {
                    return fmt.Sprintf("Did not find a flag matching %s in -d=ssa/%s debug option", flag, phase);
                }
            }
            if (phase == "intrinsics")
            {
                switch (flag)
                {
                    case "on": 
                        IntrinsicsDisable = val == 0L;
                        break;
                    case "off": 
                        IntrinsicsDisable = val != 0L;
                        break;
                    case "debug": 
                        IntrinsicsDebug = val;
                        break;
                    default: 
                        return fmt.Sprintf("Did not find a flag matching %s in -d=ssa/%s debug option", flag, phase);
                        break;
                }
                return "";
            }
            if (phase == "build")
            {
                switch (flag)
                {
                    case "debug": 
                        BuildDebug = val;
                        break;
                    case "test": 
                        BuildTest = val;
                        break;
                    case "stats": 
                        BuildStats = val;
                        break;
                    case "dump": 
                        BuildDump = valString;
                        break;
                    default: 
                        return fmt.Sprintf("Did not find a flag matching %s in -d=ssa/%s debug option", flag, phase);
                        break;
                }
                return "";
            }
            var underphase = strings.Replace(phase, "_", " ", -1L);
            ref regexp.Regexp re = default;
            if (phase[0L] == '~')
            {
                var (r, ok) = regexp.Compile(underphase[1L..]);
                if (ok != null)
                {
                    return fmt.Sprintf("Error %s in regexp for phase %s, flag %s", ok.Error(), phase, flag);
                }
                re = r;
            }
            var matchedOne = false;
            {
                var p__prev1 = p;

                foreach (var (__i, __p) in passes)
                {
                    i = __i;
                    p = __p;
                    if (phase == "all")
                    {
                        p.time = alltime;
                        p.mem = allmem;
                        if (alldump)
                        {
                            p.addDump(valString);
                        }
                        passes[i] = p;
                        matchedOne = true;
                    }
                    else if (p.name == phase || p.name == underphase || re != null && re.MatchString(p.name))
                    {
                        switch (flag)
                        {
                            case "on": 
                                p.disabled = val == 0L;
                                break;
                            case "off": 
                                p.disabled = val != 0L;
                                break;
                            case "time": 
                                p.time = val != 0L;
                                break;
                            case "mem": 
                                p.mem = val != 0L;
                                break;
                            case "debug": 
                                p.debug = val;
                                break;
                            case "stats": 
                                p.stats = val;
                                break;
                            case "test": 
                                p.test = val;
                                break;
                            case "dump": 
                                p.addDump(valString);
                                break;
                            default: 
                                return fmt.Sprintf("Did not find a flag matching %s in -d=ssa/%s debug option", flag, phase);
                                break;
                        }
                        if (p.disabled && p.required)
                        {
                            return fmt.Sprintf("Cannot disable required SSA phase %s using -d=ssa/%s debug option", phase, phase);
                        }
                        passes[i] = p;
                        matchedOne = true;
                    }
                }

                p = p__prev1;
            }

            if (matchedOne)
            {
                return "";
            }
            return fmt.Sprintf("Did not find a phase matching %s in -d=ssa/... debug option", phase);
        }

        // list of passes for the compiler
        private static array<pass> passes = new array<pass>(new pass[] { {name:"early phielim",fn:phielim}, {name:"early copyelim",fn:copyelim}, {name:"early deadcode",fn:deadcode}, {name:"short circuit",fn:shortcircuit}, {name:"decompose user",fn:decomposeUser,required:true}, {name:"opt",fn:opt,required:true}, {name:"zero arg cse",fn:zcse,required:true}, {name:"opt deadcode",fn:deadcode,required:true}, {name:"generic cse",fn:cse}, {name:"phiopt",fn:phiopt}, {name:"nilcheckelim",fn:nilcheckelim}, {name:"prove",fn:prove}, {name:"loopbce",fn:loopbce}, {name:"decompose builtin",fn:decomposeBuiltIn,required:true}, {name:"softfloat",fn:softfloat,required:true}, {name:"late opt",fn:opt,required:true}, {name:"generic deadcode",fn:deadcode}, {name:"check bce",fn:checkbce}, {name:"fuse",fn:fuse}, {name:"dse",fn:dse}, {name:"writebarrier",fn:writebarrier,required:true}, {name:"insert resched checks",fn:insertLoopReschedChecks,disabled:objabi.Preemptibleloops_enabled==0}, {name:"tighten",fn:tighten}, {name:"lower",fn:lower,required:true}, {name:"lowered cse",fn:cse}, {name:"elim unread autos",fn:elimUnreadAutos}, {name:"lowered deadcode",fn:deadcode,required:true}, {name:"checkLower",fn:checkLower,required:true}, {name:"late phielim",fn:phielim}, {name:"late copyelim",fn:copyelim}, {name:"phi tighten",fn:phiTighten}, {name:"late deadcode",fn:deadcode}, {name:"critical",fn:critical,required:true}, {name:"likelyadjust",fn:likelyadjust}, {name:"layout",fn:layout,required:true}, {name:"schedule",fn:schedule,required:true}, {name:"late nilcheck",fn:nilcheckelim2}, {name:"flagalloc",fn:flagalloc,required:true}, {name:"regalloc",fn:regalloc,required:true}, {name:"loop rotate",fn:loopRotate}, {name:"stackframe",fn:stackframe,required:true}, {name:"trim",fn:trim} });

        // Double-check phase ordering constraints.
        // This code is intended to document the ordering requirements
        // between different phases. It does not override the passes
        // list above.
        private partial struct constraint
        {
            public @string a; // a must come before b
            public @string b; // a must come before b
        }

        private static array<constraint> passOrder = new array<constraint>(new constraint[] { {"dse","insert resched checks"}, {"insert resched checks","lower"}, {"insert resched checks","tighten"}, {"generic cse","prove"}, {"prove","generic deadcode"}, {"generic cse","dse"}, {"generic cse","nilcheckelim"}, {"nilcheckelim","generic deadcode"}, {"nilcheckelim","fuse"}, {"opt","nilcheckelim"}, {"tighten","lower"}, {"generic deadcode","tighten"}, {"generic cse","tighten"}, {"generic deadcode","check bce"}, {"decompose builtin","late opt"}, {"decompose builtin","softfloat"}, {"critical","layout"}, {"critical","regalloc"}, {"schedule","regalloc"}, {"lower","checkLower"}, {"lowered deadcode","checkLower"}, {"schedule","late nilcheck"}, {"schedule","flagalloc"}, {"flagalloc","regalloc"}, {"regalloc","loop rotate"}, {"regalloc","stackframe"}, {"regalloc","trim"} });

        private static void init()
        {
            foreach (var (_, c) in passOrder)
            {
                var a = c.a;
                var b = c.b;
                long i = -1L;
                long j = -1L;
                foreach (var (k, p) in passes)
                {
                    if (p.name == a)
                    {
                        i = k;
                    }
                    if (p.name == b)
                    {
                        j = k;
                    }
                }
                if (i < 0L)
                {
                    log.Panicf("pass %s not found", a);
                }
                if (j < 0L)
                {
                    log.Panicf("pass %s not found", b);
                }
                if (i >= j)
                {
                    log.Panicf("passes %s and %s out of order", a, b);
                }
            }
        }
    }
}}}}
