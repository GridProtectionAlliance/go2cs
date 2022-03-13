// Inferno utils/6l/obj.c
// https://bitbucket.org/inferno-os/inferno-os/src/master/utils/6l/obj.c
//
//    Copyright © 1994-1999 Lucent Technologies Inc.  All rights reserved.
//    Portions Copyright © 1995-1997 C H Forsyth (forsyth@terzarima.net)
//    Portions Copyright © 1997-1999 Vita Nuova Limited
//    Portions Copyright © 2000-2007 Vita Nuova Holdings Limited (www.vitanuova.com)
//    Portions Copyright © 2004,2006 Bruce Ellis
//    Portions Copyright © 2005-2007 C H Forsyth (forsyth@terzarima.net)
//    Revisions Copyright © 2000-2007 Lucent Technologies Inc. and others
//    Portions Copyright © 2009 The Go Authors. All rights reserved.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.  IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

// package ld -- go2cs converted at 2022 March 13 06:35:04 UTC
// import "cmd/link/internal/ld" ==> using ld = go.cmd.link.@internal.ld_package
// Original source: C:\Program Files\Go\src\cmd\link\internal\ld\main.go
namespace go.cmd.link.@internal;

using bufio = bufio_package;
using goobj = cmd.@internal.goobj_package;
using objabi = cmd.@internal.objabi_package;
using sys = cmd.@internal.sys_package;
using benchmark = cmd.link.@internal.benchmark_package;
using flag = flag_package;
using buildcfg = @internal.buildcfg_package;
using log = log_package;
using os = os_package;
using runtime = runtime_package;
using pprof = runtime.pprof_package;
using strings = strings_package;
using System;

public static partial class ld_package {

private static slice<byte> pkglistfornote = default;private static bool windowsgui = default;private static bool ownTmpDir = default;

private static void init() {
    flag.Var(_addr_rpath, "r", "set the ELF dynamic linker search `path` to dir1:dir2:...");
}

// Flags used by the linker. The exported flags are used by the architecture-specific packages.
private static var flagBuildid = flag.String("buildid", "", "record `id` as Go toolchain build id");private static var flagOutfile = flag.String("o", "", "write output to `file`");private static var flagPluginPath = flag.String("pluginpath", "", "full path name for plugin");private static var flagInstallSuffix = flag.String("installsuffix", "", "set package directory `suffix`");private static var flagDumpDep = flag.Bool("dumpdep", false, "dump symbol dependency graph");private static var flagRace = flag.Bool("race", false, "enable race detector");private static var flagMsan = flag.Bool("msan", false, "enable MSan interface");private static var flagAslr = flag.Bool("aslr", true, "enable ASLR for buildmode=c-shared on windows");private static var flagFieldTrack = flag.String("k", "", "set field tracking `symbol`");private static var flagLibGCC = flag.String("libgcc", "", "compiler support lib for internal linking; use \"none\" to disable");private static var flagTmpdir = flag.String("tmpdir", "", "use `directory` for temporary files");private static var flagExtld = flag.String("extld", "", "use `linker` when linking in external mode");private static var flagExtldflags = flag.String("extldflags", "", "pass `flags` to external linker");private static var flagExtar = flag.String("extar", "", "archive program for buildmode=c-archive");private static var flagA = flag.Bool("a", false, "no-op (deprecated)");public static var FlagC = flag.Bool("c", false, "dump call graph");public static var FlagD = flag.Bool("d", false, "disable dynamic executable");private static var flagF = flag.Bool("f", false, "ignore version mismatch");private static var flagG = flag.Bool("g", false, "disable go package data checks");private static var flagH = flag.Bool("h", false, "halt on error");private static var flagN = flag.Bool("n", false, "dump symbol table");public static var FlagS = flag.Bool("s", false, "disable symbol table");public static var FlagW = flag.Bool("w", false, "disable DWARF generation");private static bool flag8 = default;private static var flagInterpreter = flag.String("I", "", "use `linker` as ELF dynamic linker");public static var FlagDebugTramp = flag.Int("debugtramp", 0, "debug trampolines");public static var FlagDebugTextSize = flag.Int("debugtextsize", 0, "debug text section max size");public static var FlagStrictDups = flag.Int("strictdups", 0, "sanity check duplicate symbol contents during object file reading (1=warn 2=err).");public static var FlagRound = flag.Int("R", -1, "set address rounding `quantum`");public static var FlagTextAddr = flag.Int64("T", -1, "set text segment `address`");private static var flagEntrySymbol = flag.String("E", "", "set `entry` symbol name");private static var cpuprofile = flag.String("cpuprofile", "", "write cpu profile to `file`");private static var memprofile = flag.String("memprofile", "", "write memory profile to `file`");private static var memprofilerate = flag.Int64("memprofilerate", 0, "set runtime.MemProfileRate to `rate`");private static var benchmarkFlag = flag.String("benchmark", "", "set to 'mem' or 'cpu' to enable phase benchmarking");private static var benchmarkFileFlag = flag.String("benchmarkprofile", "", "emit phase profiles to `base`_phase.{cpu,mem}prof");

// Main is the main entry point for the linker code.
public static void Main(ptr<sys.Arch> _addr_arch, Arch theArch) => func((defer, _, _) => {
    ref sys.Arch arch = ref _addr_arch.val;

    thearch = theArch;
    var ctxt = linknew(arch);
    ctxt.Bso = bufio.NewWriter(os.Stdout); 

    // For testing behavior of go command when tools crash silently.
    // Undocumented, not in standard flag parser to avoid
    // exposing in usage message.
    {
        var arg__prev1 = arg;

        foreach (var (_, __arg) in os.Args) {
            arg = __arg;
            if (arg == "-crash_for_testing") {
                os.Exit(2);
            }
        }
        arg = arg__prev1;
    }

    var final = gorootFinal();
    addstrdata1(ctxt, "runtime.defaultGOROOT=" + final);
    addstrdata1(ctxt, "internal/buildcfg.defaultGOROOT=" + final);

    var buildVersion = buildcfg.Version;
    {
        var goexperiment = buildcfg.GOEXPERIMENT();

        if (goexperiment != "") {
            buildVersion += " X:" + goexperiment;
        }
    }
    addstrdata1(ctxt, "runtime.buildVersion=" + buildVersion); 

    // TODO(matloob): define these above and then check flag values here
    if (ctxt.Arch.Family == sys.AMD64 && buildcfg.GOOS == "plan9") {
        flag.BoolVar(_addr_flag8, "8", false, "use 64-bit addresses in symbol table");
    }
    var flagHeadType = flag.String("H", "", "set header `type`");
    flag.BoolVar(_addr_ctxt.linkShared, "linkshared", false, "link against installed Go shared libraries");
    flag.Var(_addr_ctxt.LinkMode, "linkmode", "set link `mode`");
    flag.Var(_addr_ctxt.BuildMode, "buildmode", "set build `mode`");
    flag.BoolVar(_addr_ctxt.compressDWARF, "compressdwarf", true, "compress DWARF if possible");
    objabi.Flagfn1("B", "add an ELF NT_GNU_BUILD_ID `note` when using ELF", addbuildinfo);
    objabi.Flagfn1("L", "add specified `directory` to library path", a => {
        Lflag(ctxt, a);
    });
    objabi.AddVersionFlag(); // -V
    objabi.Flagfn1("X", "add string value `definition` of the form importpath.name=value", s => {
        addstrdata1(ctxt, s);
    });
    objabi.Flagcount("v", "print link trace", _addr_ctxt.Debugvlog);
    objabi.Flagfn1("importcfg", "read import configuration from `file`", ctxt.readImportCfg);

    objabi.Flagparse(usage);

    if (ctxt.Debugvlog > 0) { 
        // dump symbol info on crash
        defer(() => {
            ctxt.loader.Dump();
        }());
    }
    switch (flagHeadType.val) {
        case "": 

            break;
        case "windowsgui": 
            ctxt.HeadType = objabi.Hwindows;
            windowsgui = true;
            break;
        default: 
            {
                var err__prev1 = err;

                var err = ctxt.HeadType.Set(flagHeadType.val);

                if (err != null) {
                    Errorf(null, "%v", err);
                    usage();
                }

                err = err__prev1;

            }
            break;
    }
    if (ctxt.HeadType == objabi.Hunknown) {
        ctxt.HeadType.Set(buildcfg.GOOS);
    }
    if (!flagAslr && ctxt.BuildMode != BuildModeCShared.val) {
        Errorf(null, "-aslr=false is only allowed for -buildmode=c-shared");
        usage();
    }
    checkStrictDups = FlagStrictDups.val;

    startProfile();
    if (ctxt.BuildMode == BuildModeUnset) {
        ctxt.BuildMode.Set("exe");
    }
    if (ctxt.BuildMode != BuildModeShared && flag.NArg() != 1) {
        usage();
    }
    if (flagOutfile == "".val) {
        flagOutfile.val = "a.out";
        if (ctxt.HeadType == objabi.Hwindows) {
            flagOutfile.val += ".exe";
        }
    }
    interpreter = flagInterpreter.val;

    if (flagBuildid == "" && ctxt.Target.IsOpenbsd().val) { 
        // TODO(jsing): Remove once direct syscalls are no longer in use.
        // OpenBSD 6.7 onwards will not permit direct syscalls from a
        // dynamically linked binary unless it identifies the binary
        // contains a .note.go.buildid ELF note. See issue #36435.
        flagBuildid.val = "go-openbsd";
    }
    ptr<benchmark.Metrics> bench;
    if (len(benchmarkFlag.val) != 0) {
        if (benchmarkFlag == "mem".val) {
            bench = benchmark.New(benchmark.GC, benchmarkFileFlag.val);
        }
        else if (benchmarkFlag == "cpu".val) {
            bench = benchmark.New(benchmark.NoGC, benchmarkFileFlag.val);
        }
        else
 {
            Errorf(null, "unknown benchmark flag: %q", benchmarkFlag.val);
            usage();
        }
    }
    bench.Start("libinit");
    libinit(ctxt); // creates outfile
    bench.Start("computeTLSOffset");
    ctxt.computeTLSOffset();
    bench.Start("Archinit");
    thearch.Archinit(ctxt);

    if (ctxt.linkShared && !ctxt.IsELF) {
        Exitf("-linkshared can only be used on elf systems");
    }
    if (ctxt.Debugvlog != 0) {
        ctxt.Logf("HEADER = -H%d -T0x%x -R0x%x\n", ctxt.HeadType, uint64(FlagTextAddr.val), uint32(FlagRound.val));
    }
    goobj.FingerprintType zerofp = new goobj.FingerprintType();

    if (ctxt.BuildMode == BuildModeShared) 
        for (nint i = 0; i < flag.NArg(); i++) {
            var arg = flag.Arg(i);
            var parts = strings.SplitN(arg, "=", 2);
            @string pkgpath = default;            @string file = default;

            if (len(parts) == 1) {
                (pkgpath, file) = ("main", arg);
            }
            else
 {
                (pkgpath, file) = (parts[0], parts[1]);
            }
            pkglistfornote = append(pkglistfornote, pkgpath);
            pkglistfornote = append(pkglistfornote, '\n');
            addlibpath(ctxt, "command line", "command line", file, pkgpath, "", zerofp);
        }
    else if (ctxt.BuildMode == BuildModePlugin) 
        addlibpath(ctxt, "command line", "command line", flag.Arg(0), flagPluginPath.val, "", zerofp);
    else 
        addlibpath(ctxt, "command line", "command line", flag.Arg(0), "main", "", zerofp);
        bench.Start("loadlib");
    ctxt.loadlib();

    bench.Start("deadcode");
    deadcode(ctxt);

    bench.Start("linksetup");
    ctxt.linksetup();

    bench.Start("dostrdata");
    ctxt.dostrdata();
    if (buildcfg.Experiment.FieldTrack) {
        bench.Start("fieldtrack");
        fieldtrack(ctxt.Arch, ctxt.loader);
    }
    bench.Start("dwarfGenerateDebugInfo");
    dwarfGenerateDebugInfo(ctxt);

    bench.Start("callgraph");
    ctxt.callgraph();

    bench.Start("dostkcheck");
    ctxt.dostkcheck();

    bench.Start("mangleTypeSym");
    ctxt.mangleTypeSym();

    if (ctxt.IsELF) {
        bench.Start("doelf");
        ctxt.doelf();
    }
    if (ctxt.IsDarwin()) {
        bench.Start("domacho");
        ctxt.domacho();
    }
    if (ctxt.IsWindows()) {
        bench.Start("dope");
        ctxt.dope();
        bench.Start("windynrelocsyms");
        ctxt.windynrelocsyms();
    }
    if (ctxt.IsAIX()) {
        bench.Start("doxcoff");
        ctxt.doxcoff();
    }
    bench.Start("textbuildid");
    ctxt.textbuildid();
    bench.Start("addexport");
    ctxt.setArchSyms();
    ctxt.addexport();
    bench.Start("Gentext");
    thearch.Gentext(ctxt, ctxt.loader); // trampolines, call stubs, etc.

    bench.Start("textaddress");
    ctxt.textaddress();
    bench.Start("typelink");
    ctxt.typelink();
    bench.Start("buildinfo");
    ctxt.buildinfo();
    bench.Start("pclntab");
    var containers = ctxt.findContainerSyms();
    var pclnState = ctxt.pclntab(containers);
    bench.Start("findfunctab");
    ctxt.findfunctab(pclnState, containers);
    bench.Start("dwarfGenerateDebugSyms");
    dwarfGenerateDebugSyms(ctxt);
    bench.Start("symtab");
    var symGroupType = ctxt.symtab(pclnState);
    bench.Start("dodata");
    ctxt.dodata(symGroupType);
    bench.Start("address");
    var order = ctxt.address();
    bench.Start("dwarfcompress");
    dwarfcompress(ctxt);
    bench.Start("layout");
    var filesize = ctxt.layout(order); 

    // Write out the output file.
    // It is split into two parts (Asmb and Asmb2). The first
    // part writes most of the content (sections and segments),
    // for which we have computed the size and offset, in a
    // mmap'd region. The second part writes more content, for
    // which we don't know the size.
    if (ctxt.Arch.Family != sys.Wasm) { 
        // Don't mmap if we're building for Wasm. Wasm file
        // layout is very different so filesize is meaningless.
        {
            var err__prev2 = err;

            err = ctxt.Out.Mmap(filesize);

            if (err != null) {
                Exitf("mapping output file failed: %v", err);
            }

            err = err__prev2;

        }
    }
    bench.Start("Asmb");
    asmb(ctxt);

    exitIfErrors(); 

    // Generate additional symbols for the native symbol table just prior
    // to code generation.
    bench.Start("GenSymsLate");
    if (thearch.GenSymsLate != null) {
        thearch.GenSymsLate(ctxt, ctxt.loader);
    }
    bench.Start("Asmb2");
    asmb2(ctxt);

    bench.Start("Munmap");
    ctxt.Out.Close(); // Close handles Munmapping if necessary.

    bench.Start("hostlink");
    ctxt.hostlink();
    if (ctxt.Debugvlog != 0) {
        ctxt.Logf("%s", ctxt.loader.Stat());
        ctxt.Logf("%d liveness data\n", liveness);
    }
    bench.Start("Flush");
    ctxt.Bso.Flush();
    bench.Start("archive");
    ctxt.archive();
    bench.Report(os.Stdout);

    errorexit();
});

public partial struct Rpath {
    public bool set;
    public @string val;
}

private static error Set(this ptr<Rpath> _addr_r, @string val) {
    ref Rpath r = ref _addr_r.val;

    r.set = true;
    r.val = val;
    return error.As(null!)!;
}

private static @string String(this ptr<Rpath> _addr_r) {
    ref Rpath r = ref _addr_r.val;

    return r.val;
}

private static void startProfile() {
    if (cpuprofile != "".val) {
        var (f, err) = os.Create(cpuprofile.val);
        if (err != null) {
            log.Fatalf("%v", err);
        }
        {
            var err__prev2 = err;

            var err = pprof.StartCPUProfile(f);

            if (err != null) {
                log.Fatalf("%v", err);
            }

            err = err__prev2;

        }
        AtExit(pprof.StopCPUProfile);
    }
    if (memprofile != "".val) {
        if (memprofilerate != 0.val) {
            runtime.MemProfileRate = int(memprofilerate.val);
        }
        (f, err) = os.Create(memprofile.val);
        if (err != null) {
            log.Fatalf("%v", err);
        }
        AtExit(() => { 
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
                    log.Fatalf("%v", err);
                }

                err = err__prev2;

            }
        });
    }
}

} // end ld_package
