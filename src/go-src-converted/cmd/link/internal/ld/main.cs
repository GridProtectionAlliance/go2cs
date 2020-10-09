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

// package ld -- go2cs converted at 2020 October 09 05:50:10 UTC
// import "cmd/link/internal/ld" ==> using ld = go.cmd.link.@internal.ld_package
// Original source: C:\Go\src\cmd\link\internal\ld\main.go
using bufio = go.bufio_package;
using goobj2 = go.cmd.@internal.goobj2_package;
using objabi = go.cmd.@internal.objabi_package;
using sys = go.cmd.@internal.sys_package;
using benchmark = go.cmd.link.@internal.benchmark_package;
using flag = go.flag_package;
using log = go.log_package;
using os = go.os_package;
using exec = go.os.exec_package;
using runtime = go.runtime_package;
using pprof = go.runtime.pprof_package;
using strings = go.strings_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace link {
namespace @internal
{
    public static partial class ld_package
    {
        private static slice<byte> pkglistfornote = default;        private static bool windowsgui = default;        private static bool ownTmpDir = default;

        private static void init()
        {
            flag.Var(_addr_rpath, "r", "set the ELF dynamic linker search `path` to dir1:dir2:...");
        }

        // Flags used by the linker. The exported flags are used by the architecture-specific packages.
        private static var flagBuildid = flag.String("buildid", "", "record `id` as Go toolchain build id");        private static var flagOutfile = flag.String("o", "", "write output to `file`");        private static var flagPluginPath = flag.String("pluginpath", "", "full path name for plugin");        private static var flagInstallSuffix = flag.String("installsuffix", "", "set package directory `suffix`");        private static var flagDumpDep = flag.Bool("dumpdep", false, "dump symbol dependency graph");        private static var flagRace = flag.Bool("race", false, "enable race detector");        private static var flagMsan = flag.Bool("msan", false, "enable MSan interface");        private static var flagFieldTrack = flag.String("k", "", "set field tracking `symbol`");        private static var flagLibGCC = flag.String("libgcc", "", "compiler support lib for internal linking; use \"none\" to disable");        private static var flagTmpdir = flag.String("tmpdir", "", "use `directory` for temporary files");        private static var flagExtld = flag.String("extld", "", "use `linker` when linking in external mode");        private static var flagExtldflags = flag.String("extldflags", "", "pass `flags` to external linker");        private static var flagExtar = flag.String("extar", "", "archive program for buildmode=c-archive");        private static var flagA = flag.Bool("a", false, "no-op (deprecated)");        public static var FlagC = flag.Bool("c", false, "dump call graph");        public static var FlagD = flag.Bool("d", false, "disable dynamic executable");        private static var flagF = flag.Bool("f", false, "ignore version mismatch");        private static var flagG = flag.Bool("g", false, "disable go package data checks");        private static var flagH = flag.Bool("h", false, "halt on error");        private static var flagN = flag.Bool("n", false, "dump symbol table");        public static var FlagS = flag.Bool("s", false, "disable symbol table");        private static var flagU = flag.Bool("u", false, "reject unsafe packages");        public static var FlagW = flag.Bool("w", false, "disable DWARF generation");        public static bool Flag8 = default;        private static var flagInterpreter = flag.String("I", "", "use `linker` as ELF dynamic linker");        public static var FlagDebugTramp = flag.Int("debugtramp", 0L, "debug trampolines");        public static var FlagStrictDups = flag.Int("strictdups", 0L, "sanity check duplicate symbol contents during object file reading (1=warn 2=err).");        public static var FlagRound = flag.Int("R", -1L, "set address rounding `quantum`");        public static var FlagTextAddr = flag.Int64("T", -1L, "set text segment `address`");        private static var flagEntrySymbol = flag.String("E", "", "set `entry` symbol name");        private static var cpuprofile = flag.String("cpuprofile", "", "write cpu profile to `file`");        private static var memprofile = flag.String("memprofile", "", "write memory profile to `file`");        private static var memprofilerate = flag.Int64("memprofilerate", 0L, "set runtime.MemProfileRate to `rate`");        private static var benchmarkFlag = flag.String("benchmark", "", "set to 'mem' or 'cpu' to enable phase benchmarking");        private static var benchmarkFileFlag = flag.String("benchmarkprofile", "", "emit phase profiles to `base`_phase.{cpu,mem}prof");        private static var flagGo115Newobj = flag.Bool("go115newobj", true, "use new object file format");

        // Main is the main entry point for the linker code.
        public static void Main(ptr<sys.Arch> _addr_arch, Arch theArch) => func((_, panic, __) =>
        {
            ref sys.Arch arch = ref _addr_arch.val;

            thearch = theArch;
            var ctxt = linknew(arch);
            ctxt.Bso = bufio.NewWriter(os.Stdout); 

            // For testing behavior of go command when tools crash silently.
            // Undocumented, not in standard flag parser to avoid
            // exposing in usage message.
            {
                var arg__prev1 = arg;

                foreach (var (_, __arg) in os.Args)
                {
                    arg = __arg;
                    if (arg == "-crash_for_testing")
                    {
                        os.Exit(2L);
                    }

                }

                arg = arg__prev1;
            }

            var final = gorootFinal();
            addstrdata1(ctxt, "runtime/internal/sys.DefaultGoroot=" + final);
            addstrdata1(ctxt, "cmd/internal/objabi.defaultGOROOT=" + final); 

            // TODO(matloob): define these above and then check flag values here
            if (ctxt.Arch.Family == sys.AMD64 && objabi.GOOS == "plan9")
            {
                flag.BoolVar(_addr_Flag8, "8", false, "use 64-bit addresses in symbol table");
            }

            var flagHeadType = flag.String("H", "", "set header `type`");
            flag.BoolVar(_addr_ctxt.linkShared, "linkshared", false, "link against installed Go shared libraries");
            flag.Var(_addr_ctxt.LinkMode, "linkmode", "set link `mode`");
            flag.Var(_addr_ctxt.BuildMode, "buildmode", "set build `mode`");
            flag.BoolVar(_addr_ctxt.compressDWARF, "compressdwarf", true, "compress DWARF if possible");
            objabi.Flagfn1("B", "add an ELF NT_GNU_BUILD_ID `note` when using ELF", addbuildinfo);
            objabi.Flagfn1("L", "add specified `directory` to library path", a =>
            {
                Lflag(ctxt, a);
            });
            objabi.AddVersionFlag(); // -V
            objabi.Flagfn1("X", "add string value `definition` of the form importpath.name=value", s =>
            {
                addstrdata1(ctxt, s);
            });
            objabi.Flagcount("v", "print link trace", _addr_ctxt.Debugvlog);
            objabi.Flagfn1("importcfg", "read import configuration from `file`", ctxt.readImportCfg);

            objabi.Flagparse(usage);

            if (!flagGo115Newobj.val)
            {
                oldlink();
            }

            switch (flagHeadType.val)
            {
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

                        if (err != null)
                        {
                            Errorf(null, "%v", err);
                            usage();
                        }

                        err = err__prev1;

                    }

                    break;
            }
            if (ctxt.HeadType == objabi.Hunknown)
            {
                ctxt.HeadType.Set(objabi.GOOS);
            }

            checkStrictDups = FlagStrictDups.val;

            startProfile();
            if (ctxt.BuildMode == BuildModeUnset)
            {
                ctxt.BuildMode = BuildModeExe;
            }

            if (ctxt.BuildMode != BuildModeShared && flag.NArg() != 1L)
            {
                usage();
            }

            if (flagOutfile == "".val)
            {
                flagOutfile.val = "a.out";
                if (ctxt.HeadType == objabi.Hwindows)
                {
                    flagOutfile.val += ".exe";
                }

            }

            interpreter = flagInterpreter.val; 

            // enable benchmarking
            ptr<benchmark.Metrics> bench;
            if (len(benchmarkFlag.val) != 0L)
            {
                if (benchmarkFlag == "mem".val)
                {
                    bench = benchmark.New(benchmark.GC, benchmarkFileFlag.val);
                }
                else if (benchmarkFlag == "cpu".val)
                {
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

            if (ctxt.linkShared && !ctxt.IsELF)
            {
                Exitf("-linkshared can only be used on elf systems");
            }

            if (ctxt.Debugvlog != 0L)
            {
                ctxt.Logf("HEADER = -H%d -T0x%x -R0x%x\n", ctxt.HeadType, uint64(FlagTextAddr.val), uint32(FlagRound.val));
            }

            goobj2.FingerprintType zerofp = new goobj2.FingerprintType();

            if (ctxt.BuildMode == BuildModeShared) 
                for (long i = 0L; i < flag.NArg(); i++)
                {
                    var arg = flag.Arg(i);
                    var parts = strings.SplitN(arg, "=", 2L);
                    @string pkgpath = default;                    @string file = default;

                    if (len(parts) == 1L)
                    {
                        pkgpath = "main";
                        file = arg;

                    }
                    else
                    {
                        pkgpath = parts[0L];
                        file = parts[1L];

                    }

                    pkglistfornote = append(pkglistfornote, pkgpath);
                    pkglistfornote = append(pkglistfornote, '\n');
                    addlibpath(ctxt, "command line", "command line", file, pkgpath, "", zerofp);

                }
            else if (ctxt.BuildMode == BuildModePlugin) 
                addlibpath(ctxt, "command line", "command line", flag.Arg(0L), flagPluginPath.val, "", zerofp);
            else 
                addlibpath(ctxt, "command line", "command line", flag.Arg(0L), "main", "", zerofp);
                        bench.Start("loadlib");
            ctxt.loadlib();

            bench.Start("deadcode");
            deadcode(ctxt);

            bench.Start("linksetup");
            ctxt.linksetup();

            bench.Start("dostrdata");
            ctxt.dostrdata();
            if (objabi.Fieldtrack_enabled != 0L)
            {
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

            if (ctxt.IsELF)
            {
                bench.Start("doelf");
                ctxt.doelf();
            }

            if (ctxt.IsDarwin())
            {
                bench.Start("domacho");
                ctxt.domacho();
            }

            if (ctxt.IsWindows())
            {
                bench.Start("dope");
                ctxt.dope();
                bench.Start("windynrelocsyms");
                ctxt.windynrelocsyms();
            }

            if (ctxt.IsAIX())
            {
                bench.Start("doxcoff");
                ctxt.doxcoff();
            }

            bench.Start("textbuildid");
            ctxt.textbuildid();
            bench.Start("addexport");
            setupdynexp(ctxt);
            ctxt.setArchSyms(BeforeLoadlibFull);
            ctxt.addexport();
            bench.Start("Gentext");
            thearch.Gentext2(ctxt, ctxt.loader); // trampolines, call stubs, etc.

            bench.Start("textaddress");
            ctxt.textaddress();
            bench.Start("typelink");
            ctxt.typelink();
            bench.Start("buildinfo");
            ctxt.buildinfo();
            bench.Start("pclntab");
            var container = ctxt.pclntab();
            bench.Start("findfunctab");
            ctxt.findfunctab(container);
            bench.Start("dwarfGenerateDebugSyms");
            dwarfGenerateDebugSyms(ctxt);
            bench.Start("symtab");
            var symGroupType = ctxt.symtab();
            bench.Start("dodata");
            ctxt.dodata2(symGroupType);
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
            if (ctxt.Arch.Family != sys.Wasm)
            { 
                // Don't mmap if we're building for Wasm. Wasm file
                // layout is very different so filesize is meaningless.
                {
                    var err__prev2 = err;

                    err = ctxt.Out.Mmap(filesize);

                    if (err != null)
                    {
                        panic(err);
                    }

                    err = err__prev2;

                }

            } 
            // Asmb will redirect symbols to the output file mmap, and relocations
            // will be applied directly there.
            bench.Start("Asmb");
            ctxt.loader.InitOutData();
            thearch.Asmb(ctxt, ctxt.loader);

            var newreloc = ctxt.IsAMD64() || ctxt.Is386() || ctxt.IsWasm();
            if (newreloc)
            {
                bench.Start("reloc");
                ctxt.reloc();
                bench.Start("loadlibfull"); 
                // We don't need relocations at this point.
                // An exception is internal linking on Windows, see pe.go:addPEBaseRelocSym
                // Wasm is another exception, where it applies text relocations in Asmb2.
                var needReloc = (ctxt.IsWindows() && ctxt.IsInternal()) || ctxt.IsWasm(); 
                // On AMD64 ELF, we directly use the loader's ExtRelocs, so we don't
                // need conversion. Otherwise we do.
                var needExtReloc = ctxt.IsExternal() && !(ctxt.IsAMD64() && ctxt.IsELF);
                ctxt.loadlibfull(symGroupType, needReloc, needExtReloc); // XXX do it here for now
            }
            else
            {
                bench.Start("loadlibfull");
                ctxt.loadlibfull(symGroupType, true, false); // XXX do it here for now
                bench.Start("reloc");
                ctxt.reloc2();

            }

            bench.Start("Asmb2");
            thearch.Asmb2(ctxt);

            bench.Start("Munmap");
            ctxt.Out.Close(); // Close handles Munmapping if necessary.

            bench.Start("undef");
            ctxt.undef();
            bench.Start("hostlink");
            ctxt.hostlink();
            if (ctxt.Debugvlog != 0L)
            {
                ctxt.Logf("%d symbols, %d reachable\n", len(ctxt.loader.Syms), ctxt.loader.NReachableSym());
                ctxt.Logf("%d liveness data\n", liveness);
            }

            bench.Start("Flush");
            ctxt.Bso.Flush();
            bench.Start("archive");
            ctxt.archive();
            bench.Report(os.Stdout);

            errorexit();

        });

        public partial struct Rpath
        {
            public bool set;
            public @string val;
        }

        private static error Set(this ptr<Rpath> _addr_r, @string val)
        {
            ref Rpath r = ref _addr_r.val;

            r.set = true;
            r.val = val;
            return error.As(null!)!;
        }

        private static @string String(this ptr<Rpath> _addr_r)
        {
            ref Rpath r = ref _addr_r.val;

            return r.val;
        }

        private static void startProfile()
        {
            if (cpuprofile != "".val)
            {
                var (f, err) = os.Create(cpuprofile.val);
                if (err != null)
                {
                    log.Fatalf("%v", err);
                }

                {
                    var err__prev2 = err;

                    var err = pprof.StartCPUProfile(f);

                    if (err != null)
                    {
                        log.Fatalf("%v", err);
                    }

                    err = err__prev2;

                }

                AtExit(pprof.StopCPUProfile);

            }

            if (memprofile != "".val)
            {
                if (memprofilerate != 0L.val)
                {
                    runtime.MemProfileRate = int(memprofilerate.val);
                }

                (f, err) = os.Create(memprofile.val);
                if (err != null)
                {
                    log.Fatalf("%v", err);
                }

                AtExit(() =>
                { 
                    // Profile all outstanding allocations.
                    runtime.GC(); 
                    // compilebench parses the memory profile to extract memstats,
                    // which are only written in the legacy pprof format.
                    // See golang.org/issue/18641 and runtime/pprof/pprof.go:writeHeap.
                    const long writeLegacyFormat = (long)1L;

                    {
                        var err__prev2 = err;

                        err = pprof.Lookup("heap").WriteTo(f, writeLegacyFormat);

                        if (err != null)
                        {
                            log.Fatalf("%v", err);
                        }

                        err = err__prev2;

                    }

                });

            }

        }

        // Invoke the old linker and exit.
        private static void oldlink()
        {
            var linker = os.Args[0L];
            if (strings.HasSuffix(linker, "link"))
            {
                linker = linker[..len(linker) - 4L] + "oldlink";
            }
            else if (strings.HasSuffix(linker, "link.exe"))
            {
                linker = linker[..len(linker) - 8L] + "oldlink.exe";
            }
            else
            {
                log.Fatal("cannot find oldlink. arg0=", linker);
            } 

            // Copy args, filter out -go115newobj flag
            var args = make_slice<@string>(0L, len(os.Args) - 1L);
            var skipNext = false;
            foreach (var (i, a) in os.Args)
            {
                if (i == 0L)
                {
                    continue; // skip arg0
                }

                if (skipNext)
                {
                    skipNext = false;
                    continue;
                }

                if (a == "-go115newobj")
                {
                    skipNext = true;
                    continue;
                }

                if (strings.HasPrefix(a, "-go115newobj="))
                {
                    continue;
                }

                args = append(args, a);

            }
            var cmd = exec.Command(linker, args);
            cmd.Stdout = os.Stdout;
            cmd.Stderr = os.Stderr;
            var err = cmd.Run();
            if (err == null)
            {
                os.Exit(0L);
            }

            {
                ptr<exec.ExitError> (_, ok) = err._<ptr<exec.ExitError>>();

                if (ok)
                {
                    os.Exit(2L); // would be nice to use ExitError.ExitCode(), but that is too new
                }

            }

            log.Fatal("invoke oldlink failed:", err);

        }
    }
}}}}
