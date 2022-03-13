// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Debug arguments, set by -d flag.

// package @base -- go2cs converted at 2022 March 13 06:27:57 UTC
// import "cmd/compile/internal/base" ==> using @base = go.cmd.compile.@internal.@base_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\base\debug.go
namespace go.cmd.compile.@internal;

using fmt = fmt_package;
using log = log_package;
using os = os_package;
using reflect = reflect_package;
using strconv = strconv_package;
using strings = strings_package;


// Debug holds the parsed debugging configuration values.

using System.ComponentModel;
using System;
public static partial class @base_package {

public static DebugFlags Debug = default;

// DebugFlags defines the debugging configuration values (see var Debug).
// Each struct field is a different value, named for the lower-case of the field name.
// Each field must be an int or string and must have a `help` struct tag.
//
// The -d option takes a comma-separated list of settings.
// Each setting is name=value; for ints, name is short for name=1.
public partial struct DebugFlags {
    [Description("help:\"print information about append compilation\"")]
    public nint Append;
    [Description("help:\"instrument unsafe pointer conversions\"")]
    public nint Checkptr;
    [Description("help:\"print information about closure compilation\"")]
    public nint Closure;
    [Description("help:\"run internal dclstack check\"")]
    public nint DclStack;
    [Description("help:\"print information about defer compilation\"")]
    public nint Defer;
    [Description("help:\"disable nil checks\"")]
    public nint DisableNil;
    [Description("help:\"show Node pointers values in dump output\"")]
    public nint DumpPtrs;
    [Description("help:\"print information about DWARF inlined function creation\"")]
    public nint DwarfInl;
    [Description("help:\"print export data\"")]
    public nint Export;
    [Description("help:\"print dump of GC programs\"")]
    public nint GCProg;
    [Description("help:\"allow functions with closures to be inlined\"")]
    public nint InlFuncsWithClosures;
    [Description("help:\"enable coverage instrumentation for libfuzzer\"")]
    public nint Libfuzzer;
    [Description("help:\"print information about DWARF location list creation\"")]
    public nint LocationLists;
    [Description("help:\"print information about nil checks\"")]
    public nint Nil;
    [Description("help:\"disable open-coded defers\"")]
    public nint NoOpenDefer;
    [Description("help:\"print named pc-value table\"")]
    public @string PCTab;
    [Description("help:\"show all compiler panics\"")]
    public nint Panic;
    [Description("help:\"print information about slice compilation\"")]
    public nint Slice;
    [Description("help:\"force compiler to emit soft-float code\"")]
    public nint SoftFloat;
    [Description("help:\"print information about type assertion inlining\"")]
    public nint TypeAssert;
    [Description("help:\"eager typechecking of inline function bodies\"")]
    public nint TypecheckInl;
    [Description("help:\"print information about write barriers\"")]
    public nint WB;
    [Description("help:\"print information about ABI wrapper generation\"")]
    public nint ABIWrap;
    public bool any; // set when any of the values have been set
}

// Any reports whether any of the debug flags have been set.
private static bool Any(this ptr<DebugFlags> _addr_d) {
    ref DebugFlags d = ref _addr_d.val;

    return d.any;
}

private partial struct debugField {
    public @string name;
    public @string help;
}

private static slice<debugField> debugTab = default;

private static void init() => func((_, panic, _) => {
    var v = reflect.ValueOf(_addr_Debug).Elem();
    var t = v.Type();
    for (nint i = 0; i < t.NumField(); i++) {
        var f = t.Field(i);
        if (f.Name == "any") {
            continue;
        }
        var name = strings.ToLower(f.Name);
        var help = f.Tag.Get("help");
        if (help == "") {
            panic(fmt.Sprintf("base.Debug.%s is missing help text", f.Name));
        }
        var ptr = v.Field(i).Addr().Interface();
        switch (ptr.type()) {
            case ptr<nint> _:
                break;
            case ptr<@string> _:
                break;
            default:
            {
                panic(fmt.Sprintf("base.Debug.%s has invalid type %v (must be int or string)", f.Name, f.Type));
                break;
            }
        }
        debugTab = append(debugTab, new debugField(name,help,ptr));
    }
});

// DebugSSA is called to set a -d ssa/... option.
// If nil, those options are reported as invalid options.
// If DebugSSA returns a non-empty string, that text is reported as a compiler error.
public static Func<@string, @string, nint, @string, @string> DebugSSA = default;

// parseDebug parses the -d debug string argument.
private static void parseDebug(@string debugstr) => func((_, panic, _) => { 
    // parse -d argument
    if (debugstr == "") {
        return ;
    }
    Debug.any = true;
Split:
    foreach (var (_, name) in strings.Split(debugstr, ",")) {
        if (name == "") {
            continue;
        }
        if (name == "help") {
            fmt.Print(debugHelpHeader);
            var maxLen = len("ssa/help");
            {
                var t__prev2 = t;

                foreach (var (_, __t) in debugTab) {
                    t = __t;
                    if (len(t.name) > maxLen) {
                        maxLen = len(t.name);
                    }
                }

                t = t__prev2;
            }

            {
                var t__prev2 = t;

                foreach (var (_, __t) in debugTab) {
                    t = __t;
                    fmt.Printf("\t%-*s\t%s\n", maxLen, t.name, t.help);
                } 
                // ssa options have their own help

                t = t__prev2;
            }

            fmt.Printf("\t%-*s\t%s\n", maxLen, "ssa/help", "print help about SSA debugging");
            fmt.Print(debugHelpFooter);
            os.Exit(0);
        }
        nint val = 1;
        @string valstring = "";
        var haveInt = true;
        {
            var i__prev1 = i;

            var i = strings.IndexAny(name, "=:");

            if (i >= 0) {
                error err = default!;
                (name, valstring) = (name[..(int)i], name[(int)i + 1..]);                val, err = strconv.Atoi(valstring);
                if (err != null) {
                    (val, haveInt) = (1, false);
                }
            }

            i = i__prev1;

        }
        {
            var t__prev2 = t;

            foreach (var (_, __t) in debugTab) {
                t = __t;
                if (t.name != name) {
                    continue;
                }
                switch (t.val.type()) {
                    case 
                        break;
                    case ptr<@string> vp:
                        vp.val = valstring;
                        break;
                    case ptr<nint> vp:
                        if (!haveInt) {
                            log.Fatalf("invalid debug value %v", name);
                        }
                        vp.val = val;
                        break;
                    default:
                    {
                        var vp = t.val.type();
                        panic("bad debugtab type");
                        break;
                    }
                }
                _continueSplit = true;
                break;
            } 
            // special case for ssa for now

            t = t__prev2;
        }

        if (DebugSSA != null && strings.HasPrefix(name, "ssa/")) { 
            // expect form ssa/phase/flag
            // e.g. -d=ssa/generic_cse/time
            // _ in phase name also matches space
            var phase = name[(int)4..];
            @string flag = "debug"; // default flag is debug
            {
                var i__prev2 = i;

                i = strings.Index(phase, "/");

                if (i >= 0) {
                    flag = phase[(int)i + 1..];
                    phase = phase[..(int)i];
                }

                i = i__prev2;

            }
            err = DebugSSA(phase, flag, val, valstring);
            if (err != "") {
                log.Fatalf(err);
            }
            _continueSplit = true;
            break;
        }
        log.Fatalf("unknown debug key -d %s\n", name);
    }
});

private static readonly @string debugHelpHeader = "usage: -d arg[,arg]* and arg is <key>[=<value>]\n\n<key> is one of:\n\n";



private static readonly @string debugHelpFooter = @"
<value> is key-specific.

Key ""checkptr"" supports values:
	""0"": instrumentation disabled
	""1"": conversions involving unsafe.Pointer are instrumented
	""2"": conversions to unsafe.Pointer force heap allocation

Key ""pctab"" supports values:
	""pctospadj"", ""pctofile"", ""pctoline"", ""pctoinline"", ""pctopcdata""
";


} // end @base_package
