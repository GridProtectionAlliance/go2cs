// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2022 March 13 06:00:49 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\ssa\compile.go
namespace go.cmd.compile.@internal;

using bytes = bytes_package;
using src = cmd.@internal.src_package;
using fmt = fmt_package;
using crc32 = hash.crc32_package;
using buildcfg = @internal.buildcfg_package;
using log = log_package;
using rand = math.rand_package;
using os = os_package;
using regexp = regexp_package;
using runtime = runtime_package;
using sort = sort_package;
using strings = strings_package;
using time = time_package;


// Compile is the main entry point for this package.
// Compile modifies f so that on return:
//   · all Values in f map to 0 or 1 assembly instructions of the target architecture
//   · the order of f.Blocks is the order to emit the Blocks
//   · the order of b.Values is the order to emit the Values in each Block
//   · f has a non-nil regAlloc field

using System;
public static partial class ssa_package {

public static void Compile(ptr<Func> _addr_f) => func((defer, _, _) => {
    ref Func f = ref _addr_f.val;
 
    // TODO: debugging - set flags to control verbosity of compiler,
    // which phases to dump IR before/after, etc.
    if (f.Log()) {
        f.Logf("compiling %s\n", f.Name);
    }
    ptr<rand.Rand> rnd;
    if (checkEnabled) {
        var seed = int64(crc32.ChecksumIEEE((slice<byte>)f.Name)) ^ int64(checkRandSeed);
        rnd = rand.New(rand.NewSource(seed));
    }
    @string phaseName = "init";
    defer(() => {
        if (phaseName != "") {
            var err = recover();
            var stack = make_slice<byte>(16384);
            var n = runtime.Stack(stack, false);
            stack = stack[..(int)n];
            if (f.HTMLWriter != null) {
                f.HTMLWriter.flushPhases();
            }
            f.Fatalf("panic during %s while compiling %s:\n\n%v\n\n%s\n", phaseName, f.Name, err, stack);
        }
    }()); 

    // Run all the passes
    if (f.Log()) {
        printFunc(f);
    }
    f.HTMLWriter.WritePhase("start", "start");
    if (BuildDump != "" && BuildDump == f.Name) {
        f.dumpFile("build");
    }
    if (checkEnabled) {
        checkFunc(f);
    }
    const var logMemStats = false;

    foreach (var (_, p) in passes) {
        if (!f.Config.optimize && !p.required || p.disabled) {
            continue;
        }
        f.pass = _addr_p;
        phaseName = p.name;
        if (f.Log()) {
            f.Logf("  pass %s begin\n", p.name);
        }
        ref runtime.MemStats mStart = ref heap(out ptr<runtime.MemStats> _addr_mStart);
        if (logMemStats || p.mem) {
            runtime.ReadMemStats(_addr_mStart);
        }
        if (checkEnabled && !f.scheduled) { 
            // Test that we don't depend on the value order, by randomizing
            // the order of values in each block. See issue 18169.
            foreach (var (_, b) in f.Blocks) {
                for (nint i = 0; i < len(b.Values) - 1; i++) {
                    var j = i + rnd.Intn(len(b.Values) - i);
                    (b.Values[i], b.Values[j]) = (b.Values[j], b.Values[i]);
                }
            }
        }
        var tStart = time.Now();
        p.fn(f);
        var tEnd = time.Now(); 

        // Need something less crude than "Log the whole intermediate result".
        if (f.Log() || f.HTMLWriter != null) {
            var time = tEnd.Sub(tStart).Nanoseconds();
            @string stats = default;
            if (logMemStats) {
                ref runtime.MemStats mEnd = ref heap(out ptr<runtime.MemStats> _addr_mEnd);
                runtime.ReadMemStats(_addr_mEnd);
                var nBytes = mEnd.TotalAlloc - mStart.TotalAlloc;
                var nAllocs = mEnd.Mallocs - mStart.Mallocs;
                stats = fmt.Sprintf("[%d ns %d allocs %d bytes]", time, nAllocs, nBytes);
            }
            else
 {
                stats = fmt.Sprintf("[%d ns]", time);
            }
            if (f.Log()) {
                f.Logf("  pass %s end %s\n", p.name, stats);
                printFunc(f);
            }
            f.HTMLWriter.WritePhase(phaseName, fmt.Sprintf("%s <span class=\"stats\">%s</span>", phaseName, stats));
        }
        if (p.time || p.mem) { 
            // Surround timing information w/ enough context to allow comparisons.
            time = tEnd.Sub(tStart).Nanoseconds();
            if (p.time) {
                f.LogStat("TIME(ns)", time);
            }
            if (p.mem) {
                mEnd = default;
                runtime.ReadMemStats(_addr_mEnd);
                nBytes = mEnd.TotalAlloc - mStart.TotalAlloc;
                nAllocs = mEnd.Mallocs - mStart.Mallocs;
                f.LogStat("TIME(ns):BYTES:ALLOCS", time, nBytes, nAllocs);
            }
        }
        if (p.dump != null && p.dump[f.Name]) { 
            // Dump function to appropriately named file
            f.dumpFile(phaseName);
        }
        if (checkEnabled) {
            checkFunc(f);
        }
    }    if (f.HTMLWriter != null) { 
        // Ensure we write any pending phases to the html
        f.HTMLWriter.flushPhases();
    }
    if (f.ruleMatches != null) {
        slice<@string> keys = default;
        {
            var key__prev1 = key;

            foreach (var (__key) in f.ruleMatches) {
                key = __key;
                keys = append(keys, key);
            }
            key = key__prev1;
        }

        sort.Strings(keys);
        ptr<object> buf = @new<bytes.Buffer>();
        fmt.Fprintf(buf, "%s: ", f.Name);
        {
            var key__prev1 = key;

            foreach (var (_, __key) in keys) {
                key = __key;
                fmt.Fprintf(buf, "%s=%d ", key, f.ruleMatches[key]);
            }
            key = key__prev1;
        }

        fmt.Fprint(buf, "\n");
        fmt.Print(buf.String());
    }
    phaseName = "";
});

// dumpFile creates a file from the phase name and function name
// Dumping is done to files to avoid buffering huge strings before
// output.
private static void dumpFile(this ptr<Func> _addr_f, @string phaseName) {
    ref Func f = ref _addr_f.val;

    f.dumpFileSeq++;
    var fname = fmt.Sprintf("%s_%02d__%s.dump", f.Name, int(f.dumpFileSeq), phaseName);
    fname = strings.Replace(fname, " ", "_", -1);
    fname = strings.Replace(fname, "/", "_", -1);
    fname = strings.Replace(fname, ":", "_", -1);

    var (fi, err) = os.Create(fname);
    if (err != null) {
        f.Warnl(src.NoXPos, "Unable to create after-phase dump file %s", fname);
        return ;
    }
    stringFuncPrinter p = new stringFuncPrinter(w:fi);
    fprintFunc(p, f);
    fi.Close();
}

private partial struct pass {
    public @string name;
    public Action<ptr<Func>> fn;
    public bool required;
    public bool disabled;
    public bool time; // report time to run pass
    public bool mem; // report mem stats to run pass
    public nint stats; // pass reports own "stats" (e.g., branches removed)
    public nint debug; // pass performs some debugging. =1 should be in error-testing-friendly Warnl format.
    public nint test; // pass-specific ad-hoc option, perhaps useful in development
    public map<@string, bool> dump; // dump if function name matches
}

private static void addDump(this ptr<pass> _addr_p, @string s) {
    ref pass p = ref _addr_p.val;

    if (p.dump == null) {
        p.dump = make_map<@string, bool>();
    }
    p.dump[s] = true;
}

private static @string String(this ptr<pass> _addr_p) {
    ref pass p = ref _addr_p.val;

    if (p == null) {
        return "nil pass";
    }
    return p.name;
}

// Run consistency checker between each phase
private static var checkEnabled = false;private static nint checkRandSeed = 0;

// Debug output
public static nint IntrinsicsDebug = default;
public static bool IntrinsicsDisable = default;

public static nint BuildDebug = default;
public static nint BuildTest = default;
public static nint BuildStats = default;
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
public static @string PhaseOption(@string phase, @string flag, nint val, @string valString) {
    switch (phase) {
        case "": 

        case "help": 
                    nint lastcr = 0;
                    @string phasenames = "    check, all, build, intrinsics";
                    {
                        var p__prev1 = p;

                        foreach (var (_, __p) in passes) {
                            p = __p;
                            var pn = strings.Replace(p.name, " ", "_", -1);
                            if (len(pn) + len(phasenames) - lastcr > 70) {
                                phasenames += "\n    ";
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

                    return "PhaseOptions usage:\n\n    go tool compile -d=ssa/<phase>/<flag>[=<value>|<function" +
                "_name>]\n\nwhere:\n\n- <phase> is one of:\n" + phasenames + @"

            - <flag> is one of:
                on, off, debug, mem, time, test, stats, dump, seed

            - <value> defaults to 1

            - <function_name> is required for the ""dump"" flag, and specifies the
              name of function to dump after <phase>

            Phase ""all"" supports flags ""time"", ""mem"", and ""dump"".
            Phase ""intrinsics"" supports flags ""on"", ""off"", and ""debug"".

            If the ""dump"" flag is specified, the output is written on a file named
            <phase>__<function_name>_<seq>.dump; otherwise it is directed to stdout.

            Examples:

                -d=ssa/check/on
            enables checking after each phase

            	-d=ssa/check/seed=1234
            enables checking after each phase, using 1234 to seed the PRNG
            used for value order randomization

                -d=ssa/all/time
            enables time reporting for all phases

                -d=ssa/prove/debug=2
            sets debugging level to 2 in the prove pass

            Be aware that when ""/debug=X"" is applied to a pass, some passes
            will emit debug output for all functions, and other passes will
            only emit debug output for functions that match the current
            GOSSAFUNC value.

            Multiple flags can be passed at once, by separating them with
            commas. For example:

                -d=ssa/check/on,ssa/all/time
            ";
            break;
    }

    if (phase == "check") {
        switch (flag) {
            case "on": 
                checkEnabled = val != 0;
                debugPoset = checkEnabled; // also turn on advanced self-checking in prove's datastructure
                return "";
                break;
            case "off": 
                checkEnabled = val == 0;
                debugPoset = checkEnabled;
                return "";
                break;
            case "seed": 
                checkEnabled = true;
                checkRandSeed = val;
                debugPoset = checkEnabled;
                return "";
                break;
        }
    }
    var alltime = false;
    var allmem = false;
    var alldump = false;
    if (phase == "all") {
        switch (flag) {
            case "time": 
                alltime = val != 0;
                break;
            case "mem": 
                allmem = val != 0;
                break;
            case "dump": 
                alldump = val != 0;
                if (alldump) {
                    BuildDump = valString;
                }
                break;
            default: 
                return fmt.Sprintf("Did not find a flag matching %s in -d=ssa/%s debug option", flag, phase);
                break;
        }
    }
    if (phase == "intrinsics") {
        switch (flag) {
            case "on": 
                IntrinsicsDisable = val == 0;
                break;
            case "off": 
                IntrinsicsDisable = val != 0;
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
    if (phase == "build") {
        switch (flag) {
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
    var underphase = strings.Replace(phase, "_", " ", -1);
    ptr<regexp.Regexp> re;
    if (phase[0] == '~') {
        var (r, ok) = regexp.Compile(underphase[(int)1..]);
        if (ok != null) {
            return fmt.Sprintf("Error %s in regexp for phase %s, flag %s", ok.Error(), phase, flag);
        }
        re = r;
    }
    var matchedOne = false;
    {
        var p__prev1 = p;

        foreach (var (__i, __p) in passes) {
            i = __i;
            p = __p;
            if (phase == "all") {
                p.time = alltime;
                p.mem = allmem;
                if (alldump) {
                    p.addDump(valString);
                }
                passes[i] = p;
                matchedOne = true;
            }
            else if (p.name == phase || p.name == underphase || re != null && re.MatchString(p.name)) {
                switch (flag) {
                    case "on": 
                        p.disabled = val == 0;
                        break;
                    case "off": 
                        p.disabled = val != 0;
                        break;
                    case "time": 
                        p.time = val != 0;
                        break;
                    case "mem": 
                        p.mem = val != 0;
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
                if (p.disabled && p.required) {
                    return fmt.Sprintf("Cannot disable required SSA phase %s using -d=ssa/%s debug option", phase, phase);
                }
                passes[i] = p;
                matchedOne = true;
            }
        }
        p = p__prev1;
    }

    if (matchedOne) {
        return "";
    }
    return fmt.Sprintf("Did not find a phase matching %s in -d=ssa/... debug option", phase);
}

// list of passes for the compiler
private static array<pass> passes = new array<pass>(new pass[] { {name:"number lines",fn:numberLines,required:true}, {name:"early phielim",fn:phielim}, {name:"early copyelim",fn:copyelim}, {name:"early deadcode",fn:deadcode}, {name:"short circuit",fn:shortcircuit}, {name:"decompose user",fn:decomposeUser,required:true}, {name:"pre-opt deadcode",fn:deadcode}, {name:"opt",fn:opt,required:true}, {name:"zero arg cse",fn:zcse,required:true}, {name:"opt deadcode",fn:deadcode,required:true}, {name:"generic cse",fn:cse}, {name:"phiopt",fn:phiopt}, {name:"gcse deadcode",fn:deadcode,required:true}, {name:"nilcheckelim",fn:nilcheckelim}, {name:"prove",fn:prove}, {name:"early fuse",fn:fuseEarly}, {name:"decompose builtin",fn:decomposeBuiltIn,required:true}, {name:"expand calls",fn:expandCalls,required:true}, {name:"softfloat",fn:softfloat,required:true}, {name:"late opt",fn:opt,required:true}, {name:"dead auto elim",fn:elimDeadAutosGeneric}, {name:"generic deadcode",fn:deadcode,required:true}, {name:"check bce",fn:checkbce}, {name:"branchelim",fn:branchelim}, {name:"late fuse",fn:fuseLate}, {name:"dse",fn:dse}, {name:"writebarrier",fn:writebarrier,required:true}, {name:"insert resched checks",fn:insertLoopReschedChecks,disabled:!buildcfg.Experiment.PreemptibleLoops}, {name:"lower",fn:lower,required:true}, {name:"addressing modes",fn:addressingModes,required:false}, {name:"lowered deadcode for cse",fn:deadcode}, {name:"lowered cse",fn:cse}, {name:"elim unread autos",fn:elimUnreadAutos}, {name:"tighten tuple selectors",fn:tightenTupleSelectors,required:true}, {name:"lowered deadcode",fn:deadcode,required:true}, {name:"checkLower",fn:checkLower,required:true}, {name:"late phielim",fn:phielim}, {name:"late copyelim",fn:copyelim}, {name:"tighten",fn:tighten}, {name:"late deadcode",fn:deadcode}, {name:"critical",fn:critical,required:true}, {name:"phi tighten",fn:phiTighten}, {name:"likelyadjust",fn:likelyadjust}, {name:"layout",fn:layout,required:true}, {name:"schedule",fn:schedule,required:true}, {name:"late nilcheck",fn:nilcheckelim2}, {name:"flagalloc",fn:flagalloc,required:true}, {name:"regalloc",fn:regalloc,required:true}, {name:"loop rotate",fn:loopRotate}, {name:"stackframe",fn:stackframe,required:true}, {name:"trim",fn:trim} });

// Double-check phase ordering constraints.
// This code is intended to document the ordering requirements
// between different phases. It does not override the passes
// list above.
private partial struct constraint {
    public @string a; // a must come before b
    public @string b; // a must come before b
}

private static array<constraint> passOrder = new array<constraint>(new constraint[] { {"dse","insert resched checks"}, {"insert resched checks","lower"}, {"insert resched checks","tighten"}, {"generic cse","prove"}, {"prove","generic deadcode"}, {"generic cse","dse"}, {"generic cse","nilcheckelim"}, {"nilcheckelim","generic deadcode"}, {"nilcheckelim","late fuse"}, {"opt","nilcheckelim"}, {"generic deadcode","tighten"}, {"generic cse","tighten"}, {"generic deadcode","check bce"}, {"decompose builtin","late opt"}, {"decompose builtin","softfloat"}, {"tighten tuple selectors","schedule"}, {"critical","phi tighten"}, {"critical","layout"}, {"critical","regalloc"}, {"schedule","regalloc"}, {"lower","checkLower"}, {"lowered deadcode","checkLower"}, {"schedule","late nilcheck"}, {"schedule","flagalloc"}, {"flagalloc","regalloc"}, {"regalloc","loop rotate"}, {"regalloc","stackframe"}, {"regalloc","trim"} });

private static void init() {
    foreach (var (_, c) in passOrder) {
        var a = c.a;
        var b = c.b;
        nint i = -1;
        nint j = -1;
        foreach (var (k, p) in passes) {
            if (p.name == a) {
                i = k;
            }
            if (p.name == b) {
                j = k;
            }
        }        if (i < 0) {
            log.Panicf("pass %s not found", a);
        }
        if (j < 0) {
            log.Panicf("pass %s not found", b);
        }
        if (i >= j) {
            log.Panicf("passes %s and %s out of order", a, b);
        }
    }
}

} // end ssa_package
