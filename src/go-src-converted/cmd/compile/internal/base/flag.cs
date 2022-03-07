// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package @base -- go2cs converted at 2022 March 06 23:14:30 UTC
// import "cmd/compile/internal/base" ==> using @base = go.cmd.compile.@internal.@base_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\base\flag.go
using json = go.encoding.json_package;
using flag = go.flag_package;
using fmt = go.fmt_package;
using buildcfg = go.@internal.buildcfg_package;
using ioutil = go.io.ioutil_package;
using log = go.log_package;
using os = go.os_package;
using reflect = go.reflect_package;
using runtime = go.runtime_package;
using strings = go.strings_package;

using objabi = go.cmd.@internal.objabi_package;
using sys = go.cmd.@internal.sys_package;
using System;
using System.ComponentModel;


namespace go.cmd.compile.@internal;

public static partial class @base_package {

private static void usage() {
    fmt.Fprintf(os.Stderr, "usage: compile [options] file.go...\n");
    objabi.Flagprint(os.Stderr);
    Exit(2);
}

// Flag holds the parsed command-line flags.
// See ParseFlag for non-zero defaults.
public static CmdFlags Flag = default;

// A CountFlag is a counting integer flag.
// It accepts -name=value to set the value directly,
// but it also accepts -name with no =value to increment the count.
public partial struct CountFlag { // : nint
}

// CmdFlags defines the command-line flags (see var Flag).
// Each struct field is a different flag, by default named for the lower-case of the field name.
// If the flag name is a single letter, the default flag name is left upper-case.
// If the flag name is "Lower" followed by a single letter, the default flag name is the lower-case of the last letter.
//
// If this default flag name can't be made right, the `flag` struct tag can be used to replace it,
// but this should be done only in exceptional circumstances: it helps everyone if the flag name
// is obvious from the field name when the flag is used elsewhere in the compiler sources.
// The `flag:"-"` struct tag makes a field invisible to the flag logic and should also be used sparingly.
//
// Each field must have a `help` struct tag giving the flag help message.
//
// The allowed field types are bool, int, string, pointers to those (for values stored elsewhere),
// CountFlag (for a counting flag), and func(string) (for a flag that uses special code for parsing).
public partial struct CmdFlags {
    [Description("help:\"disable bounds checking\"")]
    public CountFlag B;
    [Description("help:\"disable printing of columns in error messages\"")]
    public CountFlag C;
    [Description("help:\"set relative `path` for local imports\"")]
    public @string D;
    [Description("help:\"debug symbol export\"")]
    public CountFlag E;
    [Description("help:\"accept generic code\"")]
    public CountFlag G;
    [Description("help:\"add `directory` to import search path\"")]
    public Action<@string> I;
    [Description("help:\"debug missing line numbers\"")]
    public CountFlag K;
    [Description("help:\"show full file names in error messages\"")]
    public CountFlag L;
    [Description("help:\"disable optimizations\"")]
    public CountFlag N;
    [Description("help:\"print assembly listing\"")]
    public CountFlag S; // V is added by objabi.AddVersionFlag
    [Description("help:\"debug parse tree after type checking\"")]
    public CountFlag W;
    [Description("help:\"concurrency during compilation (1 means no concurrency)\"")]
    public nint LowerC;
    [Description("help:\"enable debugging settings; try -d help\"")]
    public Action<@string> LowerD;
    [Description("help:\"no limit on number of errors reported\"")]
    public CountFlag LowerE;
    [Description("help:\"halt on error\"")]
    public CountFlag LowerH;
    [Description("help:\"debug runtime-initialized variables\"")]
    public CountFlag LowerJ;
    [Description("help:\"disable inlining\"")]
    public CountFlag LowerL;
    [Description("help:\"print optimization decisions\"")]
    public CountFlag LowerM;
    [Description("help:\"write output to `file`\"")]
    public @string LowerO;
    [Description("help:\"set expected package import `path`\"")]
    public ptr<@string> LowerP; // &Ctxt.Pkgpath, set below
    [Description("help:\"debug generated wrappers\"")]
    public CountFlag LowerR;
    [Description("help:\"enable tracing for debugging the compiler\"")]
    public bool LowerT;
    [Description("help:\"debug type checking\"")]
    public CountFlag LowerW;
    [Description("help:\"increase debug verbosity\"")]
    public ptr<bool> LowerV; // Special characters
    [Description("flag:\"%\" help:\"debug non-static initializers\"")]
    public nint Percent;
    [Description("flag:\"+\" help:\"compiling runtime\"")]
    public bool CompilingRuntime; // Longer names
    [Description("help:\"write assembly header to `file`\"")]
    public @string AsmHdr;
    [Description("help:\"append benchmark times to `file`\"")]
    public @string Bench;
    [Description("help:\"write block profile to `file`\"")]
    public @string BlockProfile;
    [Description("help:\"record `id` as the build id in the export metadata\"")]
    public @string BuildID;
    [Description("help:\"write cpu profile to `file`\"")]
    public @string CPUProfile;
    [Description("help:\"compiling complete package (no C or assembly)\"")]
    public bool Complete;
    [Description("help:\"clobber dead stack slots (for debugging)\"")]
    public bool ClobberDead;
    [Description("help:\"clobber dead registers (for debugging)\"")]
    public bool ClobberDeadReg;
    [Description("help:\"generate DWARF symbols\"")]
    public bool Dwarf;
    [Description("help:\"use base address selection entries in DWARF\"")]
    public ptr<bool> DwarfBASEntries; // &Ctxt.UseBASEntries, set below
    [Description("help:\"add location lists to DWARF in optimized mode\"")]
    public ptr<bool> DwarfLocationLists; // &Ctxt.Flag_locationlists, set below
    [Description("help:\"support references to Go symbols defined in other shared libraries\"")]
    public ptr<bool> Dynlink; // &Ctxt.Flag_dynlink, set below
    [Description("help:\"read go:embed configuration from `file`\"")]
    public Action<@string> EmbedCfg;
    [Description("help:\"generate DWARF inline info records\"")]
    public nint GenDwarfInl; // 0=disabled, 1=funcs, 2=funcs+formals/locals
    [Description("help:\"required version of the runtime\"")]
    public @string GoVersion;
    [Description("help:\"read import configuration from `file`\"")]
    public Action<@string> ImportCfg;
    [Description("help:\"add `definition` of the form source=actual to import map\"")]
    public Action<@string> ImportMap;
    [Description("help:\"set pkg directory `suffix`\"")]
    public @string InstallSuffix;
    [Description("help:\"version,file for JSON compiler/optimizer detail output\"")]
    public @string JSON;
    [Description("help:\"Go language version source code expects\"")]
    public @string Lang;
    [Description("help:\"write linker-specific object to `file`\"")]
    public @string LinkObj;
    [Description("help:\"generate code that will be linked against Go shared libraries\"")]
    public ptr<bool> LinkShared; // &Ctxt.Flag_linkshared, set below
    [Description("help:\"debug liveness analysis\"")]
    public CountFlag Live;
    [Description("help:\"build code compatible with C/C++ memory sanitizer\"")]
    public bool MSan;
    [Description("help:\"write memory profile to `file`\"")]
    public @string MemProfile;
    [Description("help:\"set runtime.MemProfileRate to `rate`\"")]
    public long MemProfileRate;
    [Description("help:\"write mutex profile to `file`\"")]
    public @string MutexProfile;
    [Description("help:\"reject local (relative) imports\"")]
    public bool NoLocalImports;
    [Description("help:\"write to file.a instead of file.o\"")]
    public bool Pack;
    [Description("help:\"enable race detector\"")]
    public bool Race;
    [Description("help:\"generate code that can be linked into a shared library\"")]
    public ptr<bool> Shared; // &Ctxt.Flag_shared, set below
    [Description("help:\"reduce the size limit for stack allocated objects\"")]
    public bool SmallFrames; // small stacks, to diagnose GC latency; see golang.org/issue/27732
    [Description("help:\"enable spectre mitigations in `list` (all, index, ret)\"")]
    public @string Spectre;
    [Description("help:\"compiling standard library\"")]
    public bool Std;
    [Description("help:\"read symbol ABIs from `file`\"")]
    public @string SymABIs;
    [Description("help:\"write an execution trace to `file`\"")]
    public @string TraceProfile;
    [Description("help:\"remove `prefix` from recorded source file paths\"")]
    public @string TrimPath;
    [Description("help:\"enable write barrier\"")]
    public bool WB; // TODO: remove

// Configuration derived from flags; not a flag itself.
}

// ParseFlags parses the command-line flags into Flag.
public static void ParseFlags() {
    Flag.I = addImportDir;

    Flag.LowerC = 1;
    Flag.LowerD = parseDebug;
    Flag.LowerP = _addr_Ctxt.Pkgpath;
    Flag.LowerV = _addr_Ctxt.Debugvlog;

    Flag.Dwarf = buildcfg.GOARCH != "wasm";
    Flag.DwarfBASEntries = _addr_Ctxt.UseBASEntries;
    Flag.DwarfLocationLists = _addr_Ctxt.Flag_locationlists;
    Flag.DwarfLocationLists.val = true;
    Flag.Dynlink = _addr_Ctxt.Flag_dynlink;
    Flag.EmbedCfg = readEmbedCfg;
    Flag.GenDwarfInl = 2;
    Flag.ImportCfg = readImportCfg;
    Flag.ImportMap = addImportMap;
    Flag.LinkShared = _addr_Ctxt.Flag_linkshared;
    Flag.Shared = _addr_Ctxt.Flag_shared;
    Flag.WB = true;
    Debug.InlFuncsWithClosures = 1;

    Debug.Checkptr = -1; // so we can tell whether it is set explicitly

    Flag.Cfg.ImportMap = make_map<@string, @string>();

    objabi.AddVersionFlag(); // -V
    registerFlags();
    objabi.Flagparse(usage);

    if (Flag.MSan && !sys.MSanSupported(buildcfg.GOOS, buildcfg.GOARCH)) {
        log.Fatalf("%s/%s does not support -msan", buildcfg.GOOS, buildcfg.GOARCH);
    }
    if (Flag.Race && !sys.RaceDetectorSupported(buildcfg.GOOS, buildcfg.GOARCH)) {
        log.Fatalf("%s/%s does not support -race", buildcfg.GOOS, buildcfg.GOARCH);
    }
    if ((Flag.Shared || Flag.Dynlink || Flag.LinkShared.val) && !Ctxt.Arch.InFamily(sys.AMD64, sys.ARM, sys.ARM64, sys.I386, sys.PPC64, sys.RISCV64, sys.S390X)) {
        log.Fatalf("%s/%s does not support -shared", buildcfg.GOOS, buildcfg.GOARCH);
    }
    parseSpectre(Flag.Spectre); // left as string for RecordFlags

    Ctxt.Flag_shared = Ctxt.Flag_dynlink || Ctxt.Flag_shared;
    Ctxt.Flag_optimize = Flag.N == 0;
    Ctxt.Debugasm = int(Flag.S);

    if (flag.NArg() < 1) {
        usage();
    }
    if (Flag.GoVersion != "" && Flag.GoVersion != runtime.Version()) {
        fmt.Printf("compile: version %q does not match go tool version %q\n", runtime.Version(), Flag.GoVersion);
        Exit(2);
    }
    if (Flag.LowerO == "") {
        var p = flag.Arg(0);
        {
            var i__prev2 = i;

            var i = strings.LastIndex(p, "/");

            if (i >= 0) {
                p = p[(int)i + 1..];
            }

            i = i__prev2;

        }

        if (runtime.GOOS == "windows") {
            {
                var i__prev3 = i;

                i = strings.LastIndex(p, "\\");

                if (i >= 0) {
                    p = p[(int)i + 1..];
                }

                i = i__prev3;

            }

        }
        {
            var i__prev2 = i;

            i = strings.LastIndex(p, ".");

            if (i >= 0) {
                p = p[..(int)i];
            }

            i = i__prev2;

        }

        @string suffix = ".o";
        if (Flag.Pack) {
            suffix = ".a";
        }
        Flag.LowerO = p + suffix;

    }
    if (Flag.Race && Flag.MSan) {
        log.Fatal("cannot use both -race and -msan");
    }
    if (Flag.Race || Flag.MSan) { 
        // -race and -msan imply -d=checkptr for now.
        if (Debug.Checkptr == -1) { // if not set explicitly
            Debug.Checkptr = 1;

        }
    }
    if (Flag.CompilingRuntime && Flag.N != 0) {
        log.Fatal("cannot disable optimizations while compiling runtime");
    }
    if (Flag.LowerC < 1) {
        log.Fatalf("-c must be at least 1, got %d", Flag.LowerC);
    }
    if (Flag.LowerC > 1 && !concurrentBackendAllowed()) {
        log.Fatalf("cannot use concurrent backend compilation with provided flags; invoked as %v", os.Args);
    }
    if (Flag.CompilingRuntime) { 
        // Runtime can't use -d=checkptr, at least not yet.
        Debug.Checkptr = 0; 

        // Fuzzing the runtime isn't interesting either.
        Debug.Libfuzzer = 0;

    }
    if (Debug.Checkptr == -1) { // if not set explicitly
        Debug.Checkptr = 0;

    }
    Ctxt.Debugpcln = Debug.PCTab;

}

// registerFlags adds flag registrations for all the fields in Flag.
// See the comment on type CmdFlags for the rules.
private static void registerFlags() => func((_, panic, _) => {
    var boolType = reflect.TypeOf(bool(false));    var intType = reflect.TypeOf(int(0));    var stringType = reflect.TypeOf(string(""));    var ptrBoolType = reflect.TypeOf(@new<bool>());    var ptrIntType = reflect.TypeOf(@new<int>());    var ptrStringType = reflect.TypeOf(@new<string>());    var countType = reflect.TypeOf(CountFlag(0));    var funcType = reflect.TypeOf((Action<@string>)null);

    var v = reflect.ValueOf(_addr_Flag).Elem();
    var t = v.Type();
    for (nint i = 0; i < t.NumField(); i++) {
        var f = t.Field(i);
        if (f.Name == "Cfg") {
            continue;
        }
        @string name = default;
        if (len(f.Name) == 1) {
            name = f.Name;
        }
        else if (len(f.Name) == 6 && f.Name[..(int)5] == "Lower" && 'A' <= f.Name[5] && f.Name[5] <= 'Z') {
            name = string(rune(f.Name[5] + 'a' - 'A'));
        }
        else
 {
            name = strings.ToLower(f.Name);
        }
        {
            var tag = f.Tag.Get("flag");

            if (tag != "") {
                name = tag;
            }

        }


        var help = f.Tag.Get("help");
        if (help == "") {
            panic(fmt.Sprintf("base.Flag.%s is missing help text", f.Name));
        }
        {
            var k = f.Type.Kind();

            if ((k == reflect.Ptr || k == reflect.Func) && v.Field(i).IsNil()) {
                panic(fmt.Sprintf("base.Flag.%s is uninitialized %v", f.Name, f.Type));
            }

        }



        if (f.Type == boolType) 
            ptr<bool> p = v.Field(i).Addr().Interface()._<ptr<bool>>();
            flag.BoolVar(p, name, p.val, help);
        else if (f.Type == intType) 
            p = v.Field(i).Addr().Interface()._<ptr<nint>>();
            flag.IntVar(p, name, p.val, help);
        else if (f.Type == stringType) 
            p = v.Field(i).Addr().Interface()._<ptr<@string>>();
            flag.StringVar(p, name, p.val, help);
        else if (f.Type == ptrBoolType) 
            p = v.Field(i).Interface()._<ptr<bool>>();
            flag.BoolVar(p, name, p.val, help);
        else if (f.Type == ptrIntType) 
            p = v.Field(i).Interface()._<ptr<nint>>();
            flag.IntVar(p, name, p.val, help);
        else if (f.Type == ptrStringType) 
            p = v.Field(i).Interface()._<ptr<@string>>();
            flag.StringVar(p, name, p.val, help);
        else if (f.Type == countType) 
            p = (int.val)(v.Field(i).Addr().Interface()._<ptr<CountFlag>>());
            objabi.Flagcount(name, help, p);
        else if (f.Type == funcType) 
            f = v.Field(i).Interface()._<Action<@string>>();
            objabi.Flagfn1(name, help, f);
        
    }

});

// concurrentFlagOk reports whether the current compiler flags
// are compatible with concurrent compilation.
private static bool concurrentFlagOk() { 
    // TODO(rsc): Many of these are fine. Remove them.
    return Flag.Percent == 0 && Flag.E == 0 && Flag.K == 0 && Flag.L == 0 && Flag.LowerH == 0 && Flag.LowerJ == 0 && Flag.LowerM == 0 && Flag.LowerR == 0;

}

private static bool concurrentBackendAllowed() {
    if (!concurrentFlagOk()) {
        return false;
    }
    if (Ctxt.Debugvlog || Debug.Any() || Flag.Live > 0) {
        return false;
    }
    if (buildcfg.Experiment.FieldTrack) {
        return false;
    }
    if (Ctxt.Flag_shared || Ctxt.Flag_dynlink || Flag.Race) {
        return false;
    }
    return true;

}

private static void addImportDir(@string dir) {
    if (dir != "") {
        Flag.Cfg.ImportDirs = append(Flag.Cfg.ImportDirs, dir);
    }
}

private static void addImportMap(@string s) {
    if (Flag.Cfg.ImportMap == null) {
        Flag.Cfg.ImportMap = make_map<@string, @string>();
    }
    if (strings.Count(s, "=") != 1) {
        log.Fatal("-importmap argument must be of the form source=actual");
    }
    var i = strings.Index(s, "=");
    var source = s[..(int)i];
    var actual = s[(int)i + 1..];
    if (source == "" || actual == "") {
        log.Fatal("-importmap argument must be of the form source=actual; source and actual must be non-empty");
    }
    Flag.Cfg.ImportMap[source] = actual;

}

private static void readImportCfg(@string file) {
    if (Flag.Cfg.ImportMap == null) {
        Flag.Cfg.ImportMap = make_map<@string, @string>();
    }
    Flag.Cfg.PackageFile = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, @string>{};
    var (data, err) = ioutil.ReadFile(file);
    if (err != null) {
        log.Fatalf("-importcfg: %v", err);
    }
    foreach (var (lineNum, line) in strings.Split(string(data), "\n")) {
        lineNum++; // 1-based
        line = strings.TrimSpace(line);
        if (line == "" || strings.HasPrefix(line, "#")) {
            continue;
        }
        @string verb = default;        @string args = default;

        {
            var i__prev1 = i;

            var i = strings.Index(line, " ");

            if (i < 0) {
                verb = line;
            }
            else
 {
                (verb, args) = (line[..(int)i], strings.TrimSpace(line[(int)i + 1..]));
            }

            i = i__prev1;

        }

        @string before = default;        @string after = default;

        {
            var i__prev1 = i;

            i = strings.Index(args, "=");

            if (i >= 0) {
                (before, after) = (args[..(int)i], args[(int)i + 1..]);
            }

            i = i__prev1;

        }

        switch (verb) {
            case "importmap": 
                if (before == "" || after == "") {
                    log.Fatalf("%s:%d: invalid importmap: syntax is \"importmap old=new\"", file, lineNum);
                }
                Flag.Cfg.ImportMap[before] = after;
                break;
            case "packagefile": 
                if (before == "" || after == "") {
                    log.Fatalf("%s:%d: invalid packagefile: syntax is \"packagefile path=filename\"", file, lineNum);
                }
                Flag.Cfg.PackageFile[before] = after;
                break;
            default: 
                log.Fatalf("%s:%d: unknown directive %q", file, lineNum, verb);
                break;
        }

    }
}

private static void readEmbedCfg(@string file) {
    var (data, err) = ioutil.ReadFile(file);
    if (err != null) {
        log.Fatalf("-embedcfg: %v", err);
    }
    {
        var err = json.Unmarshal(data, _addr_Flag.Cfg.Embed);

        if (err != null) {
            log.Fatalf("%s: %v", file, err);
        }
    }

    if (Flag.Cfg.Embed.Patterns == null) {
        log.Fatalf("%s: invalid embedcfg: missing Patterns", file);
    }
    if (Flag.Cfg.Embed.Files == null) {
        log.Fatalf("%s: invalid embedcfg: missing Files", file);
    }
}

// parseSpectre parses the spectre configuration from the string s.
private static void parseSpectre(@string s) {
    foreach (var (_, f) in strings.Split(s, ",")) {
        f = strings.TrimSpace(f);
        switch (f) {
            case "": 

                break;
            case "all": 
                Flag.Cfg.SpectreIndex = true;
                Ctxt.Retpoline = true;
                break;
            case "index": 
                Flag.Cfg.SpectreIndex = true;
                break;
            case "ret": 
                Ctxt.Retpoline = true;
                break;
            default: 
                log.Fatalf("unknown setting -spectre=%s", f);
                break;
        }

    }    if (Flag.Cfg.SpectreIndex) {
        switch (buildcfg.GOARCH) {
            case "amd64": 

                break;
            default: 
                log.Fatalf("GOARCH=%s does not support -spectre=index", buildcfg.GOARCH);
                break;
        }

    }
}

} // end @base_package
