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

// package ld -- go2cs converted at 2020 October 08 04:41:43 UTC
// import "cmd/oldlink/internal/ld" ==> using ld = go.cmd.oldlink.@internal.ld_package
// Original source: C:\Go\src\cmd\oldlink\internal\ld\main.go
using bufio = go.bufio_package;
using objabi = go.cmd.@internal.objabi_package;
using sys = go.cmd.@internal.sys_package;
using sym = go.cmd.oldlink.@internal.sym_package;
using flag = go.flag_package;
using log = go.log_package;
using os = go.os_package;
using runtime = go.runtime_package;
using pprof = go.runtime.pprof_package;
using strings = go.strings_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace oldlink {
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
        private static var flagBuildid = flag.String("buildid", "", "record `id` as Go toolchain build id");        private static var flagOutfile = flag.String("o", "", "write output to `file`");        private static var flagPluginPath = flag.String("pluginpath", "", "full path name for plugin");        private static var flagInstallSuffix = flag.String("installsuffix", "", "set package directory `suffix`");        private static var flagDumpDep = flag.Bool("dumpdep", false, "dump symbol dependency graph");        private static var flagRace = flag.Bool("race", false, "enable race detector");        private static var flagMsan = flag.Bool("msan", false, "enable MSan interface");        private static var flagFieldTrack = flag.String("k", "", "set field tracking `symbol`");        private static var flagLibGCC = flag.String("libgcc", "", "compiler support lib for internal linking; use \"none\" to disable");        private static var flagTmpdir = flag.String("tmpdir", "", "use `directory` for temporary files");        private static var flagExtld = flag.String("extld", "", "use `linker` when linking in external mode");        private static var flagExtldflags = flag.String("extldflags", "", "pass `flags` to external linker");        private static var flagExtar = flag.String("extar", "", "archive program for buildmode=c-archive");        private static var flagA = flag.Bool("a", false, "disassemble output");        public static var FlagC = flag.Bool("c", false, "dump call graph");        public static var FlagD = flag.Bool("d", false, "disable dynamic executable");        private static var flagF = flag.Bool("f", false, "ignore version mismatch");        private static var flagG = flag.Bool("g", false, "disable go package data checks");        private static var flagH = flag.Bool("h", false, "halt on error");        private static var flagN = flag.Bool("n", false, "dump symbol table");        public static var FlagS = flag.Bool("s", false, "disable symbol table");        private static var flagU = flag.Bool("u", false, "reject unsafe packages");        public static var FlagW = flag.Bool("w", false, "disable DWARF generation");        public static bool Flag8 = default;        private static var flagInterpreter = flag.String("I", "", "use `linker` as ELF dynamic linker");        public static var FlagDebugTramp = flag.Int("debugtramp", 0L, "debug trampolines");        public static var FlagStrictDups = flag.Int("strictdups", 0L, "sanity check duplicate symbol contents during object file reading (1=warn 2=err).");        private static var flagNewobj = flag.Bool("newobj", false, "use new object file format");        public static var FlagRound = flag.Int("R", -1L, "set address rounding `quantum`");        public static var FlagTextAddr = flag.Int64("T", -1L, "set text segment `address`");        private static var flagEntrySymbol = flag.String("E", "", "set `entry` symbol name");        private static var cpuprofile = flag.String("cpuprofile", "", "write cpu profile to `file`");        private static var memprofile = flag.String("memprofile", "", "write memory profile to `file`");        private static var memprofilerate = flag.Int64("memprofilerate", 0L, "set runtime.MemProfileRate to `rate`");

        // Main is the main entry point for the linker code.
        public static void Main(ptr<sys.Arch> _addr_arch, Arch theArch)
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

            if (objabi.Fieldtrack_enabled != 0L)
            {
                ctxt.Reachparent = make_map<ptr<sym.Symbol>, ptr<sym.Symbol>>();
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

            libinit(ctxt); // creates outfile

            if (ctxt.HeadType == objabi.Hunknown)
            {
                ctxt.HeadType.Set(objabi.GOOS);
            }

            ctxt.computeTLSOffset();
            thearch.Archinit(ctxt);

            if (ctxt.linkShared && !ctxt.IsELF)
            {
                Exitf("-linkshared can only be used on elf systems");
            }

            if (ctxt.Debugvlog != 0L)
            {
                ctxt.Logf("HEADER = -H%d -T0x%x -R0x%x\n", ctxt.HeadType, uint64(FlagTextAddr.val), uint32(FlagRound.val));
            }


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
                    addlibpath(ctxt, "command line", "command line", file, pkgpath, "");

                }
            else if (ctxt.BuildMode == BuildModePlugin) 
                addlibpath(ctxt, "command line", "command line", flag.Arg(0L), flagPluginPath.val, "");
            else 
                addlibpath(ctxt, "command line", "command line", flag.Arg(0L), "main", "");
                        ctxt.loadlib();

            deadcode(ctxt);
            if (flagNewobj.val)
            {
                ctxt.loadlibfull(); // XXX do it here for now
            }

            ctxt.linksetup();
            ctxt.dostrdata();

            dwarfGenerateDebugInfo(ctxt);
            if (objabi.Fieldtrack_enabled != 0L)
            {
                fieldtrack(ctxt);
            }

            ctxt.mangleTypeSym();
            ctxt.callgraph();

            ctxt.doelf();
            if (ctxt.HeadType == objabi.Hdarwin)
            {
                ctxt.domacho();
            }

            ctxt.dostkcheck();
            if (ctxt.HeadType == objabi.Hwindows)
            {
                ctxt.dope();
                ctxt.windynrelocsyms();
            }

            if (ctxt.HeadType == objabi.Haix)
            {
                ctxt.doxcoff();
            }

            ctxt.addexport();
            thearch.Gentext(ctxt); // trampolines, call stubs, etc.
            ctxt.textbuildid();
            ctxt.textaddress();
            ctxt.pclntab();
            ctxt.findfunctab();
            ctxt.typelink();
            ctxt.symtab();
            ctxt.buildinfo();
            ctxt.dodata();
            var order = ctxt.address();
            dwarfcompress(ctxt);
            var filesize = ctxt.layout(order); 

            // Write out the output file.
            // It is split into two parts (Asmb and Asmb2). The first
            // part writes most of the content (sections and segments),
            // for which we have computed the size and offset, in a
            // mmap'd region. The second part writes more content, for
            // which we don't know the size.
            bool outputMmapped = default;
            if (ctxt.Arch.Family != sys.Wasm)
            { 
                // Don't mmap if we're building for Wasm. Wasm file
                // layout is very different so filesize is meaningless.
                err = ctxt.Out.Mmap(filesize);
                outputMmapped = err == null;

            }

            if (outputMmapped)
            { 
                // Asmb will redirect symbols to the output file mmap, and relocations
                // will be applied directly there.
                thearch.Asmb(ctxt);
                ctxt.reloc();
                ctxt.Out.Munmap();

            }
            else
            { 
                // If we don't mmap, we need to apply relocations before
                // writing out.
                ctxt.reloc();
                thearch.Asmb(ctxt);

            }

            thearch.Asmb2(ctxt);

            ctxt.undef();
            ctxt.hostlink();
            if (ctxt.Debugvlog != 0L)
            {
                ctxt.Logf("%d symbols\n", len(ctxt.Syms.Allsym));
                ctxt.Logf("%d liveness data\n", liveness);
            }

            ctxt.Bso.Flush();
            ctxt.archive();

            errorexit();

        }

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
    }
}}}}
