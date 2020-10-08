// Inferno utils/8l/asm.c
// https://bitbucket.org/inferno-os/inferno-os/src/master/utils/8l/asm.c
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

// package ld -- go2cs converted at 2020 October 08 04:41:21 UTC
// import "cmd/oldlink/internal/ld" ==> using ld = go.cmd.oldlink.@internal.ld_package
// Original source: C:\Go\src\cmd\oldlink\internal\ld\lib.go
using bufio = go.bufio_package;
using bytes = go.bytes_package;
using bio = go.cmd.@internal.bio_package;
using obj = go.cmd.@internal.obj_package;
using objabi = go.cmd.@internal.objabi_package;
using sys = go.cmd.@internal.sys_package;
using loadelf = go.cmd.oldlink.@internal.loadelf_package;
using loader = go.cmd.oldlink.@internal.loader_package;
using loadmacho = go.cmd.oldlink.@internal.loadmacho_package;
using loadpe = go.cmd.oldlink.@internal.loadpe_package;
using loadxcoff = go.cmd.oldlink.@internal.loadxcoff_package;
using objfile = go.cmd.oldlink.@internal.objfile_package;
using sym = go.cmd.oldlink.@internal.sym_package;
using sha1 = go.crypto.sha1_package;
using elf = go.debug.elf_package;
using macho = go.debug.macho_package;
using base64 = go.encoding.base64_package;
using binary = go.encoding.binary_package;
using hex = go.encoding.hex_package;
using fmt = go.fmt_package;
using io = go.io_package;
using ioutil = go.io.ioutil_package;
using log = go.log_package;
using os = go.os_package;
using exec = go.os.exec_package;
using filepath = go.path.filepath_package;
using runtime = go.runtime_package;
using sort = go.sort_package;
using strings = go.strings_package;
using sync = go.sync_package;
using static go.builtin;
using System;
using System.Threading;

namespace go {
namespace cmd {
namespace oldlink {
namespace @internal
{
    public static partial class ld_package
    {
        // Data layout and relocation.

        // Derived from Inferno utils/6l/l.h
        // https://bitbucket.org/inferno-os/inferno-os/src/master/utils/6l/l.h
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
        public partial struct Arch
        {
            public long Funcalign;
            public long Maxalign;
            public long Minalign;
            public long Dwarfregsp;
            public long Dwarfreglr;
            public @string Androiddynld;
            public @string Linuxdynld;
            public @string Freebsddynld;
            public @string Netbsddynld;
            public @string Openbsddynld;
            public @string Dragonflydynld;
            public @string Solarisdynld;
            public Func<ptr<Link>, ptr<sym.Symbol>, ptr<sym.Reloc>, bool> Adddynrel;
            public Action<ptr<Link>> Archinit; // Archreloc is an arch-specific hook that assists in
// relocation processing (invoked by 'relocsym'); it handles
// target-specific relocation tasks. Here "rel" is the current
// relocation being examined, "sym" is the symbol containing the
// chunk of data to which the relocation applies, and "off" is the
// contents of the to-be-relocated data item (from sym.P). Return
// value is the appropriately relocated value (to be written back
// to the same spot in sym.P) and a boolean indicating
// success/failure (a failing value indicates a fatal error).
            public Func<ptr<Link>, ptr<sym.Reloc>, ptr<sym.Symbol>, long, (long, bool)> Archreloc; // Archrelocvariant is a second arch-specific hook used for
// relocation processing; it handles relocations where r.Type is
// insufficient to describe the relocation (r.Variant !=
// sym.RV_NONE). Here "rel" is the relocation being applied, "sym"
// is the symbol containing the chunk of data to which the
// relocation applies, and "off" is the contents of the
// to-be-relocated data item (from sym.P). Return is an updated
// offset value.
            public Func<ptr<Link>, ptr<sym.Reloc>, ptr<sym.Symbol>, long, long> Archrelocvariant;
            public Action<ptr<Link>, ptr<sym.Reloc>, ptr<sym.Symbol>> Trampoline; // Asmb and Asmb2 are arch-specific routines that write the output
// file. Typically, Asmb writes most of the content (sections and
// segments), for which we have computed the size and offset. Asmb2
// writes the rest.
            public Action<ptr<Link>> Asmb;
            public Action<ptr<Link>> Asmb2;
            public Func<ptr<Link>, ptr<sym.Reloc>, long, bool> Elfreloc1;
            public Action<ptr<Link>> Elfsetupplt;
            public Action<ptr<Link>> Gentext;
            public Func<ptr<sys.Arch>, ptr<OutBuf>, ptr<sym.Symbol>, ptr<sym.Reloc>, long, bool> Machoreloc1;
            public Func<ptr<sys.Arch>, ptr<OutBuf>, ptr<sym.Symbol>, ptr<sym.Reloc>, long, bool> PEreloc1;
            public Func<ptr<sys.Arch>, ptr<OutBuf>, ptr<sym.Symbol>, ptr<sym.Reloc>, long, bool> Xcoffreloc1; // TLSIEtoLE converts a TLS Initial Executable relocation to
// a TLS Local Executable relocation.
//
// This is possible when a TLS IE relocation refers to a local
// symbol in an executable, which is typical when internally
// linking PIE binaries.
            public Action<ptr<sym.Symbol>, long, long> TLSIEtoLE; // optional override for assignAddress
            public Func<ptr<Link>, ptr<sym.Section>, long, ptr<sym.Symbol>, ulong, bool, (ptr<sym.Section>, long, ulong)> AssignAddress;
        }

        private static Arch thearch = default;        public static int Lcsize = default;        private static Rpath rpath = default;        public static int Spsize = default;        public static int Symsize = default;

        public static readonly long MINFUNC = (long)16L; // minimum size for a function

        // DynlinkingGo reports whether we are producing Go code that can live
        // in separate shared libraries linked together at runtime.
        private static bool DynlinkingGo(this ptr<Link> _addr_ctxt) => func((_, panic, __) =>
        {
            ref Link ctxt = ref _addr_ctxt.val;

            if (!ctxt.Loaded)
            {
                panic("DynlinkingGo called before all symbols loaded");
            }

            return ctxt.BuildMode == BuildModeShared || ctxt.linkShared || ctxt.BuildMode == BuildModePlugin || ctxt.canUsePlugins;

        });

        // CanUsePlugins reports whether a plugins can be used
        private static bool CanUsePlugins(this ptr<Link> _addr_ctxt) => func((_, panic, __) =>
        {
            ref Link ctxt = ref _addr_ctxt.val;

            if (!ctxt.Loaded)
            {
                panic("CanUsePlugins called before all symbols loaded");
            }

            return ctxt.canUsePlugins;

        });

        // UseRelro reports whether to make use of "read only relocations" aka
        // relro.
        private static bool UseRelro(this ptr<Link> _addr_ctxt)
        {
            ref Link ctxt = ref _addr_ctxt.val;


            if (ctxt.BuildMode == BuildModeCArchive || ctxt.BuildMode == BuildModeCShared || ctxt.BuildMode == BuildModeShared || ctxt.BuildMode == BuildModePIE || ctxt.BuildMode == BuildModePlugin) 
                return ctxt.IsELF || ctxt.HeadType == objabi.Haix;
            else 
                return ctxt.linkShared || (ctxt.HeadType == objabi.Haix && ctxt.LinkMode == LinkExternal);
            
        }

        private static slice<ptr<sym.Symbol>> dynexp = default;        private static slice<@string> dynlib = default;        private static slice<@string> ldflag = default;        private static long havedynamic = default;        public static long Funcalign = default;        private static bool iscgo = default;        private static long elfglobalsymndx = default;        private static @string interpreter = default;        private static bool debug_s = default;        public static int HEADR = default;        private static long nerrors = default;        private static long liveness = default;        private static long checkStrictDups = default;        private static long strictDupMsgCount = default;

        public static sym.Segment Segtext = default;        public static sym.Segment Segrodata = default;        public static sym.Segment Segrelrodata = default;        public static sym.Segment Segdata = default;        public static sym.Segment Segdwarf = default;

        private static readonly @string pkgdef = (@string)"__.PKGDEF";



 
        // Set if we see an object compiled by the host compiler that is not
        // from a package that is known to support internal linking mode.
        private static var externalobj = false;        private static @string theline = default;

        public static void Lflag(ptr<Link> _addr_ctxt, @string arg)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            ctxt.Libdir = append(ctxt.Libdir, arg);
        }

        /*
         * Unix doesn't like it when we write to a running (or, sometimes,
         * recently run) binary, so remove the output file before writing it.
         * On Windows 7, remove() can force a subsequent create() to fail.
         * S_ISREG() does not exist on Plan 9.
         */
        private static void mayberemoveoutfile()
        {
            {
                var (fi, err) = os.Lstat(flagOutfile.val);

                if (err == null && !fi.Mode().IsRegular())
                {
                    return ;
                }

            }

            os.Remove(flagOutfile.val);

        }

        private static void libinit(ptr<Link> _addr_ctxt)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            Funcalign = thearch.Funcalign; 

            // add goroot to the end of the libdir list.
            @string suffix = "";

            @string suffixsep = "";
            if (flagInstallSuffix != "".val)
            {
                suffixsep = "_";
                suffix = flagInstallSuffix.val;
            }
            else if (flagRace.val)
            {
                suffixsep = "_";
                suffix = "race";
            }
            else if (flagMsan.val)
            {
                suffixsep = "_";
                suffix = "msan";
            }

            Lflag(_addr_ctxt, filepath.Join(objabi.GOROOT, "pkg", fmt.Sprintf("%s_%s%s%s", objabi.GOOS, objabi.GOARCH, suffixsep, suffix)));

            mayberemoveoutfile();
            var (f, err) = os.OpenFile(flagOutfile.val, os.O_RDWR | os.O_CREATE | os.O_TRUNC, 0775L);
            if (err != null)
            {
                Exitf("cannot create %s: %v", flagOutfile.val, err);
            }

            ctxt.Out.w = bufio.NewWriter(f);
            ctxt.Out.f = f;

            if (flagEntrySymbol == "".val)
            {

                if (ctxt.BuildMode == BuildModeCShared || ctxt.BuildMode == BuildModeCArchive) 
                    flagEntrySymbol.val = fmt.Sprintf("_rt0_%s_%s_lib", objabi.GOARCH, objabi.GOOS);
                else if (ctxt.BuildMode == BuildModeExe || ctxt.BuildMode == BuildModePIE) 
                    flagEntrySymbol.val = fmt.Sprintf("_rt0_%s_%s", objabi.GOARCH, objabi.GOOS);
                else if (ctxt.BuildMode == BuildModeShared || ctxt.BuildMode == BuildModePlugin)                 else 
                    Errorf(null, "unknown *flagEntrySymbol for buildmode %v", ctxt.BuildMode);
                
            }

        }

        private static void exitIfErrors()
        {
            if (nerrors != 0L || checkStrictDups > 1L && strictDupMsgCount > 0L)
            {
                mayberemoveoutfile();
                Exit(2L);
            }

        }

        private static void errorexit()
        {
            exitIfErrors();
            Exit(0L);
        }

        private static ptr<sym.Library> loadinternal(ptr<Link> _addr_ctxt, @string name)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            if (ctxt.linkShared && ctxt.PackageShlib != null)
            {
                {
                    var shlib = ctxt.PackageShlib[name];

                    if (shlib != "")
                    {
                        return _addr_addlibpath(ctxt, "internal", "internal", "", name, shlib)!;
                    }

                }

            }

            if (ctxt.PackageFile != null)
            {
                {
                    var pname__prev2 = pname;

                    var pname = ctxt.PackageFile[name];

                    if (pname != "")
                    {
                        return _addr_addlibpath(ctxt, "internal", "internal", pname, name, "")!;
                    }

                    pname = pname__prev2;

                }

                ctxt.Logf("loadinternal: cannot find %s\n", name);
                return _addr_null!;

            }

            foreach (var (_, libdir) in ctxt.Libdir)
            {
                if (ctxt.linkShared)
                {
                    var shlibname = filepath.Join(libdir, name + ".shlibname");
                    if (ctxt.Debugvlog != 0L)
                    {
                        ctxt.Logf("searching for %s.a in %s\n", name, shlibname);
                    }

                    {
                        var (_, err) = os.Stat(shlibname);

                        if (err == null)
                        {
                            return _addr_addlibpath(ctxt, "internal", "internal", "", name, shlibname)!;
                        }

                    }

                }

                pname = filepath.Join(libdir, name + ".a");
                if (ctxt.Debugvlog != 0L)
                {
                    ctxt.Logf("searching for %s.a in %s\n", name, pname);
                }

                {
                    (_, err) = os.Stat(pname);

                    if (err == null)
                    {
                        return _addr_addlibpath(ctxt, "internal", "internal", pname, name, "")!;
                    }

                }

            }
            ctxt.Logf("warning: unable to find %s.a\n", name);
            return _addr_null!;

        }

        // extld returns the current external linker.
        private static @string extld(this ptr<Link> _addr_ctxt)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            if (flagExtld == "".val)
            {
                flagExtld.val = "gcc";
            }

            return flagExtld.val;

        }

        // findLibPathCmd uses cmd command to find gcc library libname.
        // It returns library full path if found, or "none" if not found.
        private static @string findLibPathCmd(this ptr<Link> _addr_ctxt, @string cmd, @string libname)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            var extld = ctxt.extld();
            var args = hostlinkArchArgs(_addr_ctxt.Arch);
            args = append(args, cmd);
            if (ctxt.Debugvlog != 0L)
            {
                ctxt.Logf("%s %v\n", extld, args);
            }

            var (out, err) = exec.Command(extld, args).Output();
            if (err != null)
            {
                if (ctxt.Debugvlog != 0L)
                {
                    ctxt.Logf("not using a %s file because compiler failed\n%v\n%s\n", libname, err, out);
                }

                return "none";

            }

            return strings.TrimSpace(string(out));

        }

        // findLibPath searches for library libname.
        // It returns library full path if found, or "none" if not found.
        private static @string findLibPath(this ptr<Link> _addr_ctxt, @string libname)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            return ctxt.findLibPathCmd("--print-file-name=" + libname, libname);
        }

        private static void loadlib(this ptr<Link> _addr_ctxt)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            if (flagNewobj.val)
            {
                uint flags = default;
                switch (FlagStrictDups.val)
                {
                    case 0L: 
                        break;
                    case 1L: 

                    case 2L: 
                        flags = loader.FlagStrictDups;
                        break;
                    default: 
                        log.Fatalf("invalid -strictdups flag value %d", FlagStrictDups.val);
                        break;
                }
                ctxt.loader = loader.NewLoader(flags);

            }

            ctxt.cgo_export_static = make_map<@string, bool>();
            ctxt.cgo_export_dynamic = make_map<@string, bool>(); 

            // ctxt.Library grows during the loop, so not a range loop.
            long i = 0L;
            while (i < len(ctxt.Library))
            {
                var lib = ctxt.Library[i];
                if (lib.Shlib == "")
                {
                    if (ctxt.Debugvlog > 1L)
                    {
                        ctxt.Logf("autolib: %s (from %s)\n", lib.File, lib.Objref);
                i++;
                    }

                    loadobjfile(_addr_ctxt, _addr_lib);

                }

            } 

            // load internal packages, if not already
 

            // load internal packages, if not already
            if (flagRace.val)
            {
                loadinternal(_addr_ctxt, "runtime/race");
            }

            if (flagMsan.val)
            {
                loadinternal(_addr_ctxt, "runtime/msan");
            }

            loadinternal(_addr_ctxt, "runtime");
            while (i < len(ctxt.Library))
            {
                lib = ctxt.Library[i];
                if (lib.Shlib == "")
                {
                    loadobjfile(_addr_ctxt, _addr_lib);
                i++;
                }

            }


            if (flagNewobj.val)
            {
                iscgo = ctxt.loader.Lookup("x_cgo_init", 0L) != 0L;
                ctxt.canUsePlugins = ctxt.loader.Lookup("plugin.Open", sym.SymVerABIInternal) != 0L;
            }
            else
            {
                iscgo = ctxt.Syms.ROLookup("x_cgo_init", 0L) != null;
                ctxt.canUsePlugins = ctxt.Syms.ROLookup("plugin.Open", sym.SymVerABIInternal) != null;
            } 

            // We now have enough information to determine the link mode.
            determineLinkMode(ctxt);

            if (ctxt.LinkMode == LinkExternal && !iscgo && ctxt.LibraryByPkg["runtime/cgo"] == null && !(objabi.GOOS == "darwin" && ctxt.BuildMode != BuildModePlugin && ctxt.Arch.Family == sys.AMD64))
            { 
                // This indicates a user requested -linkmode=external.
                // The startup code uses an import of runtime/cgo to decide
                // whether to initialize the TLS.  So give it one. This could
                // be handled differently but it's an unusual case.
                {
                    var lib__prev2 = lib;

                    lib = loadinternal(_addr_ctxt, "runtime/cgo");

                    if (lib != null)
                    {
                        if (lib.Shlib != "")
                        {
                            ldshlibsyms(_addr_ctxt, lib.Shlib);
                        }
                        else
                        {
                            if (ctxt.BuildMode == BuildModeShared || ctxt.linkShared)
                            {
                                Exitf("cannot implicitly include runtime/cgo in a shared library");
                            }

                            loadobjfile(_addr_ctxt, _addr_lib);

                        }

                    }

                    lib = lib__prev2;

                }

            }

            {
                var lib__prev1 = lib;

                foreach (var (_, __lib) in ctxt.Library)
                {
                    lib = __lib;
                    if (lib.Shlib != "")
                    {
                        if (ctxt.Debugvlog > 1L)
                        {
                            ctxt.Logf("autolib: %s (from %s)\n", lib.Shlib, lib.Objref);
                        }

                        ldshlibsyms(_addr_ctxt, lib.Shlib);

                    }

                }

                lib = lib__prev1;
            }

            if (ctxt.LinkMode == LinkInternal && len(hostobj) != 0L)
            {
                if (flagNewobj.val)
                { 
                    // In newobj mode, we typically create sym.Symbols later therefore
                    // also set cgo attributes later. However, for internal cgo linking,
                    // the host object loaders still work with sym.Symbols (for now),
                    // and they need cgo attributes set to work properly. So process
                    // them now.
                    Func<@string, long, ptr<sym.Symbol>> lookup = (name, ver) => ctxt.loader.LookupOrCreate(name, ver, ctxt.Syms);
                    foreach (var (_, d) in ctxt.cgodata)
                    {
                        setCgoAttr(ctxt, lookup, d.file, d.pkg, d.directives);
                    }
                    ctxt.cgodata = null;

                } 

                // Drop all the cgo_import_static declarations.
                // Turns out we won't be needing them.
                {
                    var s__prev1 = s;

                    foreach (var (_, __s) in ctxt.Syms.Allsym)
                    {
                        s = __s;
                        if (s.Type == sym.SHOSTOBJ)
                        { 
                            // If a symbol was marked both
                            // cgo_import_static and cgo_import_dynamic,
                            // then we want to make it cgo_import_dynamic
                            // now.
                            if (s.Extname() != "" && s.Dynimplib() != "" && !s.Attr.CgoExport())
                            {
                                s.Type = sym.SDYNIMPORT;
                            }
                            else
                            {
                                s.Type = 0L;
                            }

                        }

                    }

                    s = s__prev1;
                }
            } 

            // Conditionally load host objects, or setup for external linking.
            hostobjs(_addr_ctxt);
            hostlinksetup(_addr_ctxt);

            if (flagNewobj.val)
            { 
                // Add references of externally defined symbols.
                ctxt.loader.LoadRefs(ctxt.Arch, ctxt.Syms);

            } 

            // Now that we know the link mode, set the dynexp list.
            if (!flagNewobj.val)
            { // set this later in newobj mode
                setupdynexp(_addr_ctxt);

            }

            if (ctxt.LinkMode == LinkInternal && len(hostobj) != 0L)
            { 
                // If we have any undefined symbols in external
                // objects, try to read them from the libgcc file.
                var any = false;
                {
                    var s__prev1 = s;

                    foreach (var (_, __s) in ctxt.Syms.Allsym)
                    {
                        s = __s;
                        {
                            long i__prev2 = i;

                            foreach (var (__i) in s.R)
                            {
                                i = __i;
                                var r = _addr_s.R[i]; // Copying sym.Reloc has measurable impact on performance
                                if (r.Sym != null && r.Sym.Type == sym.SXREF && r.Sym.Name != ".got")
                                {
                                    any = true;
                                    break;
                                }

                            }

                            i = i__prev2;
                        }
                    }

                    s = s__prev1;
                }

                if (any)
                {
                    if (flagLibGCC == "".val)
                    {
                        flagLibGCC.val = ctxt.findLibPathCmd("--print-libgcc-file-name", "libgcc");
                    }

                    if (runtime.GOOS == "openbsd" && flagLibGCC == "libgcc.a".val)
                    { 
                        // On OpenBSD `clang --print-libgcc-file-name` returns "libgcc.a".
                        // In this case we fail to load libgcc.a and can encounter link
                        // errors - see if we can find libcompiler_rt.a instead.
                        flagLibGCC.val = ctxt.findLibPathCmd("--print-file-name=libcompiler_rt.a", "libcompiler_rt");

                    }

                    if (flagLibGCC != "none".val)
                    {
                        hostArchive(ctxt, flagLibGCC.val);
                    }

                    if (ctxt.HeadType == objabi.Hwindows)
                    {
                        {
                            var p__prev4 = p;

                            var p = ctxt.findLibPath("libmingwex.a");

                            if (p != "none")
                            {
                                hostArchive(ctxt, p);
                            }

                            p = p__prev4;

                        }

                        {
                            var p__prev4 = p;

                            p = ctxt.findLibPath("libmingw32.a");

                            if (p != "none")
                            {
                                hostArchive(ctxt, p);
                            } 
                            // Link libmsvcrt.a to resolve '__acrt_iob_func' symbol
                            // (see https://golang.org/issue/23649 for details).

                            p = p__prev4;

                        } 
                        // Link libmsvcrt.a to resolve '__acrt_iob_func' symbol
                        // (see https://golang.org/issue/23649 for details).
                        {
                            var p__prev4 = p;

                            p = ctxt.findLibPath("libmsvcrt.a");

                            if (p != "none")
                            {
                                hostArchive(ctxt, p);
                            } 
                            // TODO: maybe do something similar to peimporteddlls to collect all lib names
                            // and try link them all to final exe just like libmingwex.a and libmingw32.a:
                            /*
                                                for:
                                                #cgo windows LDFLAGS: -lmsvcrt -lm
                                                import:
                                                libmsvcrt.a libm.a
                                            */

                            p = p__prev4;

                        } 
                        // TODO: maybe do something similar to peimporteddlls to collect all lib names
                        // and try link them all to final exe just like libmingwex.a and libmingw32.a:
                        /*
                                            for:
                                            #cgo windows LDFLAGS: -lmsvcrt -lm
                                            import:
                                            libmsvcrt.a libm.a
                                        */
                    }

                }

            } 

            // We've loaded all the code now.
            ctxt.Loaded = true;

            importcycles();

            if (flagNewobj.val)
            {
                strictDupMsgCount = ctxt.loader.NStrictDupMsgs();
            }

        }

        // Set up dynexp list.
        private static void setupdynexp(ptr<Link> _addr_ctxt)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            var dynexpMap = ctxt.cgo_export_dynamic;
            if (ctxt.LinkMode == LinkExternal)
            {
                dynexpMap = ctxt.cgo_export_static;
            }

            dynexp = make_slice<ptr<sym.Symbol>>(0L, len(dynexpMap));
            foreach (var (exp) in dynexpMap)
            {
                var s = ctxt.Syms.Lookup(exp, 0L);
                dynexp = append(dynexp, s);
            }
            sort.Sort(byName(dynexp)); 

            // Resolve ABI aliases in the list of cgo-exported functions.
            // This is necessary because we load the ABI0 symbol for all
            // cgo exports.
            {
                var s__prev1 = s;

                foreach (var (__i, __s) in dynexp)
                {
                    i = __i;
                    s = __s;
                    if (s.Type != sym.SABIALIAS)
                    {
                        continue;
                    }

                    var t = resolveABIAlias(s);
                    t.Attr |= s.Attr;
                    t.SetExtname(s.Extname());
                    dynexp[i] = t;

                }

                s = s__prev1;
            }

            ctxt.cgo_export_static = null;
            ctxt.cgo_export_dynamic = null;

        }

        // Set up flags and special symbols depending on the platform build mode.
        private static void linksetup(this ptr<Link> _addr_ctxt)
        {
            ref Link ctxt = ref _addr_ctxt.val;


            if (ctxt.BuildMode == BuildModeCShared || ctxt.BuildMode == BuildModePlugin) 
                var s = ctxt.Syms.Lookup("runtime.islibrary", 0L);
                s.Type = sym.SNOPTRDATA;
                s.Attr |= sym.AttrDuplicateOK;
                s.AddUint8(1L);
            else if (ctxt.BuildMode == BuildModeCArchive) 
                s = ctxt.Syms.Lookup("runtime.isarchive", 0L);
                s.Type = sym.SNOPTRDATA;
                s.Attr |= sym.AttrDuplicateOK;
                s.AddUint8(1L);
            // Recalculate pe parameters now that we have ctxt.LinkMode set.
            if (ctxt.HeadType == objabi.Hwindows)
            {
                Peinit(ctxt);
            }

            if (ctxt.HeadType == objabi.Hdarwin && ctxt.LinkMode == LinkExternal)
            {
                FlagTextAddr.val = 0L;
            } 

            // If there are no dynamic libraries needed, gcc disables dynamic linking.
            // Because of this, glibc's dynamic ELF loader occasionally (like in version 2.13)
            // assumes that a dynamic binary always refers to at least one dynamic library.
            // Rather than be a source of test cases for glibc, disable dynamic linking
            // the same way that gcc would.
            //
            // Exception: on OS X, programs such as Shark only work with dynamic
            // binaries, so leave it enabled on OS X (Mach-O) binaries.
            // Also leave it enabled on Solaris which doesn't support
            // statically linked binaries.
            if (ctxt.BuildMode == BuildModeExe)
            {
                if (havedynamic == 0L && ctxt.HeadType != objabi.Hdarwin && ctxt.HeadType != objabi.Hsolaris)
                {
                    FlagD.val = true;
                }

            }

            if (ctxt.LinkMode == LinkExternal && ctxt.Arch.Family == sys.PPC64 && objabi.GOOS != "aix")
            {
                var toc = ctxt.Syms.Lookup(".TOC.", 0L);
                toc.Type = sym.SDYNIMPORT;
            } 

            // The Android Q linker started to complain about underalignment of the our TLS
            // section. We don't actually use the section on android, so dont't
            // generate it.
            if (objabi.GOOS != "android")
            {
                var tlsg = ctxt.Syms.Lookup("runtime.tlsg", 0L); 

                // runtime.tlsg is used for external linking on platforms that do not define
                // a variable to hold g in assembly (currently only intel).
                if (tlsg.Type == 0L)
                {
                    tlsg.Type = sym.STLSBSS;
                    tlsg.Size = int64(ctxt.Arch.PtrSize);
                }
                else if (tlsg.Type != sym.SDYNIMPORT)
                {
                    Errorf(null, "runtime declared tlsg variable %v", tlsg.Type);
                }

                tlsg.Attr |= sym.AttrReachable;
                ctxt.Tlsg = tlsg;

            }

            ptr<sym.Symbol> moduledata;
            if (ctxt.BuildMode == BuildModePlugin)
            {
                moduledata = ctxt.Syms.Lookup("local.pluginmoduledata", 0L);
                moduledata.Attr |= sym.AttrLocal;
            }
            else
            {
                moduledata = ctxt.Syms.Lookup("runtime.firstmoduledata", 0L);
            }

            if (moduledata.Type != 0L && moduledata.Type != sym.SDYNIMPORT)
            { 
                // If the module (toolchain-speak for "executable or shared
                // library") we are linking contains the runtime package, it
                // will define the runtime.firstmoduledata symbol and we
                // truncate it back to 0 bytes so we can define its entire
                // contents in symtab.go:symtab().
                moduledata.Size = 0L; 

                // In addition, on ARM, the runtime depends on the linker
                // recording the value of GOARM.
                if (ctxt.Arch.Family == sys.ARM)
                {
                    s = ctxt.Syms.Lookup("runtime.goarm", 0L);
                    s.Type = sym.SDATA;
                    s.Size = 0L;
                    s.AddUint8(uint8(objabi.GOARM));
                }

                if (objabi.Framepointer_enabled(objabi.GOOS, objabi.GOARCH))
                {
                    s = ctxt.Syms.Lookup("runtime.framepointer_enabled", 0L);
                    s.Type = sym.SDATA;
                    s.Size = 0L;
                    s.AddUint8(1L);
                }

            }
            else
            { 
                // If OTOH the module does not contain the runtime package,
                // create a local symbol for the moduledata.
                moduledata = ctxt.Syms.Lookup("local.moduledata", 0L);
                moduledata.Attr |= sym.AttrLocal;

            } 
            // In all cases way we mark the moduledata as noptrdata to hide it from
            // the GC.
            moduledata.Type = sym.SNOPTRDATA;
            moduledata.Attr |= sym.AttrReachable;
            ctxt.Moduledata = moduledata; 

            // If package versioning is required, generate a hash of the
            // packages used in the link.
            if (ctxt.BuildMode == BuildModeShared || ctxt.BuildMode == BuildModePlugin || ctxt.CanUsePlugins())
            {
                foreach (var (_, lib) in ctxt.Library)
                {
                    if (lib.Shlib == "")
                    {
                        genhash(_addr_ctxt, _addr_lib);
                    }

                }

            }

            if (ctxt.Arch == sys.Arch386 && ctxt.HeadType != objabi.Hwindows)
            {
                if ((ctxt.BuildMode == BuildModeCArchive && ctxt.IsELF) || ctxt.BuildMode == BuildModeCShared || ctxt.BuildMode == BuildModePIE || ctxt.DynlinkingGo())
                {
                    var got = ctxt.Syms.Lookup("_GLOBAL_OFFSET_TABLE_", 0L);
                    got.Type = sym.SDYNIMPORT;
                    got.Attr |= sym.AttrReachable;
                }

            }

        }

        // mangleTypeSym shortens the names of symbols that represent Go types
        // if they are visible in the symbol table.
        //
        // As the names of these symbols are derived from the string of
        // the type, they can run to many kilobytes long. So we shorten
        // them using a SHA-1 when the name appears in the final binary.
        // This also removes characters that upset external linkers.
        //
        // These are the symbols that begin with the prefix 'type.' and
        // contain run-time type information used by the runtime and reflect
        // packages. All Go binaries contain these symbols, but only
        // those programs loaded dynamically in multiple parts need these
        // symbols to have entries in the symbol table.
        private static void mangleTypeSym(this ptr<Link> _addr_ctxt)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            if (ctxt.BuildMode != BuildModeShared && !ctxt.linkShared && ctxt.BuildMode != BuildModePlugin && !ctxt.CanUsePlugins())
            {
                return ;
            }

            foreach (var (_, s) in ctxt.Syms.Allsym)
            {
                var newName = typeSymbolMangle(s.Name);
                if (newName != s.Name)
                {
                    ctxt.Syms.Rename(s.Name, newName, int(s.Version), ctxt.Reachparent);
                }

            }

        }

        // typeSymbolMangle mangles the given symbol name into something shorter.
        //
        // Keep the type.. prefix, which parts of the linker (like the
        // DWARF generator) know means the symbol is not decodable.
        // Leave type.runtime. symbols alone, because other parts of
        // the linker manipulates them.
        private static @string typeSymbolMangle(@string name)
        {
            if (!strings.HasPrefix(name, "type."))
            {
                return name;
            }

            if (strings.HasPrefix(name, "type.runtime."))
            {
                return name;
            }

            if (len(name) <= 14L && !strings.Contains(name, "@"))
            { // Issue 19529
                return name;

            }

            var hash = sha1.Sum((slice<byte>)name);
            @string prefix = "type.";
            if (name[5L] == '.')
            {
                prefix = "type..";
            }

            return prefix + base64.StdEncoding.EncodeToString(hash[..6L]);

        }

        /*
         * look for the next file in an archive.
         * adapted from libmach.
         */
        private static long nextar(ptr<bio.Reader> _addr_bp, long off, ptr<ArHdr> _addr_a)
        {
            ref bio.Reader bp = ref _addr_bp.val;
            ref ArHdr a = ref _addr_a.val;

            if (off & 1L != 0L)
            {
                off++;
            }

            bp.MustSeek(off, 0L);
            array<byte> buf = new array<byte>(SAR_HDR);
            {
                var (n, err) = io.ReadFull(bp, buf[..]);

                if (err != null)
                {
                    if (n == 0L && err != io.EOF)
                    {
                        return -1L;
                    }

                    return 0L;

                }

            }


            a.name = artrim(buf[0L..16L]);
            a.date = artrim(buf[16L..28L]);
            a.uid = artrim(buf[28L..34L]);
            a.gid = artrim(buf[34L..40L]);
            a.mode = artrim(buf[40L..48L]);
            a.size = artrim(buf[48L..58L]);
            a.fmag = artrim(buf[58L..60L]);

            var arsize = atolwhex(a.size);
            if (arsize & 1L != 0L)
            {
                arsize++;
            }

            return arsize + SAR_HDR;

        }

        private static void genhash(ptr<Link> _addr_ctxt, ptr<sym.Library> _addr_lib) => func((defer, _, __) =>
        {
            ref Link ctxt = ref _addr_ctxt.val;
            ref sym.Library lib = ref _addr_lib.val;

            var (f, err) = bio.Open(lib.File);
            if (err != null)
            {
                Errorf(null, "cannot open file %s for hash generation: %v", lib.File, err);
                return ;
            }

            defer(f.Close());

            array<byte> magbuf = new array<byte>(len(ARMAG));
            {
                var (_, err) = io.ReadFull(f, magbuf[..]);

                if (err != null)
                {
                    Exitf("file %s too short", lib.File);
                }

            }


            if (string(magbuf[..]) != ARMAG)
            {
                Exitf("%s is not an archive file", lib.File);
            }

            ref ArHdr arhdr = ref heap(out ptr<ArHdr> _addr_arhdr);
            var l = nextar(_addr_f, f.Offset(), _addr_arhdr);
            if (l <= 0L)
            {
                Errorf(null, "%s: short read on archive file symbol header", lib.File);
                return ;
            }

            if (arhdr.name != pkgdef)
            {
                Errorf(null, "%s: missing package data entry", lib.File);
                return ;
            }

            var h = sha1.New(); 

            // To compute the hash of a package, we hash the first line of
            // __.PKGDEF (which contains the toolchain version and any
            // GOEXPERIMENT flags) and the export data (which is between
            // the first two occurrences of "\n$$").

            var pkgDefBytes = make_slice<byte>(atolwhex(arhdr.size));
            _, err = io.ReadFull(f, pkgDefBytes);
            if (err != null)
            {
                Errorf(null, "%s: error reading package data: %v", lib.File, err);
                return ;
            }

            var firstEOL = bytes.IndexByte(pkgDefBytes, '\n');
            if (firstEOL < 0L)
            {
                Errorf(null, "cannot parse package data of %s for hash generation, no newline found", lib.File);
                return ;
            }

            var firstDoubleDollar = bytes.Index(pkgDefBytes, (slice<byte>)"\n$$");
            if (firstDoubleDollar < 0L)
            {
                Errorf(null, "cannot parse package data of %s for hash generation, no \\n$$ found", lib.File);
                return ;
            }

            var secondDoubleDollar = bytes.Index(pkgDefBytes[firstDoubleDollar + 1L..], (slice<byte>)"\n$$");
            if (secondDoubleDollar < 0L)
            {
                Errorf(null, "cannot parse package data of %s for hash generation, only one \\n$$ found", lib.File);
                return ;
            }

            h.Write(pkgDefBytes[0L..firstEOL]);
            h.Write(pkgDefBytes[firstDoubleDollar..firstDoubleDollar + secondDoubleDollar]);
            lib.Hash = hex.EncodeToString(h.Sum(null));

        });

        private static void loadobjfile(ptr<Link> _addr_ctxt, ptr<sym.Library> _addr_lib) => func((defer, _, __) =>
        {
            ref Link ctxt = ref _addr_ctxt.val;
            ref sym.Library lib = ref _addr_lib.val;

            var pkg = objabi.PathToPrefix(lib.Pkg);

            if (ctxt.Debugvlog > 1L)
            {
                ctxt.Logf("ldobj: %s (%s)\n", lib.File, pkg);
            }

            var (f, err) = bio.Open(lib.File);
            if (err != null)
            {
                Exitf("cannot open file %s: %v", lib.File, err);
            }

            defer(f.Close());
            defer(() =>
            {
                if (pkg == "main" && !lib.Main)
                {
                    Exitf("%s: not package main", lib.File);
                } 

                // Ideally, we'd check that *all* object files within
                // the archive were marked safe, but here we settle
                // for *any*.
                //
                // Historically, cmd/link only checked the __.PKGDEF
                // file, which in turn came from the first object
                // file, typically produced by cmd/compile. The
                // remaining object files are normally produced by
                // cmd/asm, which doesn't support marking files as
                // safe anyway. So at least in practice, this matches
                // how safe mode has always worked.
                if (flagU && !lib.Safe.val)
                {
                    Exitf("%s: load of unsafe package %s", lib.File, pkg);
                }

            }());

            for (long i = 0L; i < len(ARMAG); i++)
            {
                {
                    var (c, err) = f.ReadByte();

                    if (err == null && c == ARMAG[i])
                    {
                        continue;
                    } 

                    /* load it as a regular file */

                } 

                /* load it as a regular file */
                var l = f.MustSeek(0L, 2L);
                f.MustSeek(0L, 0L);
                ldobj(_addr_ctxt, _addr_f, _addr_lib, l, lib.File, lib.File);
                return ;

            }

            /*
                 * load all the object files from the archive now.
                 * this gives us sequential file access and keeps us
                 * from needing to come back later to pick up more
                 * objects.  it breaks the usual C archive model, but
                 * this is Go, not C.  the common case in Go is that
                 * we need to load all the objects, and then we throw away
                 * the individual symbols that are unused.
                 *
                 * loading every object will also make it possible to
                 * load foreign objects not referenced by __.PKGDEF.
                 */


            /*
                 * load all the object files from the archive now.
                 * this gives us sequential file access and keeps us
                 * from needing to come back later to pick up more
                 * objects.  it breaks the usual C archive model, but
                 * this is Go, not C.  the common case in Go is that
                 * we need to load all the objects, and then we throw away
                 * the individual symbols that are unused.
                 *
                 * loading every object will also make it possible to
                 * load foreign objects not referenced by __.PKGDEF.
                 */
            ref ArHdr arhdr = ref heap(out ptr<ArHdr> _addr_arhdr);
            var off = f.Offset();
            while (true)
            {
                l = nextar(_addr_f, off, _addr_arhdr);
                if (l == 0L)
                {
                    break;
                }

                if (l < 0L)
                {
                    Exitf("%s: malformed archive", lib.File);
                }

                off += l; 

                // __.PKGDEF isn't a real Go object file, and it's
                // absent in -linkobj builds anyway. Skipping it
                // ensures consistency between -linkobj and normal
                // build modes.
                if (arhdr.name == pkgdef)
                {
                    continue;
                } 

                // Skip other special (non-object-file) sections that
                // build tools may have added. Such sections must have
                // short names so that the suffix is not truncated.
                if (len(arhdr.name) < 16L)
                {
                    {
                        var ext = filepath.Ext(arhdr.name);

                        if (ext != ".o" && ext != ".syso")
                        {
                            continue;
                        }

                    }

                }

                var pname = fmt.Sprintf("%s(%s)", lib.File, arhdr.name);
                l = atolwhex(arhdr.size);
                ldobj(_addr_ctxt, _addr_f, _addr_lib, l, pname, lib.File);

            }


        });

        public partial struct Hostobj
        {
            public Action<ptr<Link>, ptr<bio.Reader>, @string, long, @string> ld;
            public @string pkg;
            public @string pn;
            public @string file;
            public long off;
            public long length;
        }

        private static slice<Hostobj> hostobj = default;

        // These packages can use internal linking mode.
        // Others trigger external mode.
        private static @string internalpkg = new slice<@string>(new @string[] { "crypto/x509", "net", "os/user", "runtime/cgo", "runtime/race", "runtime/msan" });

        private static ptr<Hostobj> ldhostobj(Action<ptr<Link>, ptr<bio.Reader>, @string, long, @string> ld, objabi.HeadType headType, ptr<bio.Reader> _addr_f, @string pkg, long length, @string pn, @string file)
        {
            ref bio.Reader f = ref _addr_f.val;

            var isinternal = false;
            foreach (var (_, intpkg) in internalpkg)
            {
                if (pkg == intpkg)
                {
                    isinternal = true;
                    break;
                }

            } 

            // DragonFly declares errno with __thread, which results in a symbol
            // type of R_386_TLS_GD or R_X86_64_TLSGD. The Go linker does not
            // currently know how to handle TLS relocations, hence we have to
            // force external linking for any libraries that link in code that
            // uses errno. This can be removed if the Go linker ever supports
            // these relocation types.
            if (headType == objabi.Hdragonfly)
            {
                if (pkg == "net" || pkg == "os/user")
                {
                    isinternal = false;
                }

            }

            if (!isinternal)
            {
                externalobj = true;
            }

            hostobj = append(hostobj, new Hostobj());
            var h = _addr_hostobj[len(hostobj) - 1L];
            h.ld = ld;
            h.pkg = pkg;
            h.pn = pn;
            h.file = file;
            h.off = f.Offset();
            h.length = length;
            return _addr_h!;

        }

        private static void hostobjs(ptr<Link> _addr_ctxt)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            if (ctxt.LinkMode != LinkInternal)
            {
                return ;
            }

            ptr<Hostobj> h;

            for (long i = 0L; i < len(hostobj); i++)
            {
                h = _addr_hostobj[i];
                var (f, err) = bio.Open(h.file);
                if (err != null)
                {
                    Exitf("cannot reopen %s: %v", h.pn, err);
                }

                f.MustSeek(h.off, 0L);
                h.ld(ctxt, f, h.pkg, h.length, h.pn);
                f.Close();

            }


        }

        private static void hostlinksetup(ptr<Link> _addr_ctxt)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            if (ctxt.LinkMode != LinkExternal)
            {
                return ;
            } 

            // For external link, record that we need to tell the external linker -s,
            // and turn off -s internally: the external linker needs the symbol
            // information for its final link.
            debug_s = FlagS.val;
            FlagS.val = false; 

            // create temporary directory and arrange cleanup
            if (flagTmpdir == "".val)
            {
                var (dir, err) = ioutil.TempDir("", "go-link-");
                if (err != null)
                {
                    log.Fatal(err);
                }

                flagTmpdir.val = dir;
                ownTmpDir = true;
                AtExit(() =>
                {
                    ctxt.Out.f.Close();
                    os.RemoveAll(flagTmpdir.val);
                });

            } 

            // change our output to temporary object file
            ctxt.Out.f.Close();
            mayberemoveoutfile();

            var p = filepath.Join(flagTmpdir.val, "go.o");
            error err = default!;
            var (f, err) = os.OpenFile(p, os.O_RDWR | os.O_CREATE | os.O_TRUNC, 0775L);
            if (err != null)
            {
                Exitf("cannot create %s: %v", p, err);
            }

            ctxt.Out.w = bufio.NewWriter(f);
            ctxt.Out.f = f;
            ctxt.Out.off = 0L;

        }

        // hostobjCopy creates a copy of the object files in hostobj in a
        // temporary directory.
        private static slice<@string> hostobjCopy() => func((defer, _, __) =>
        {
            slice<@string> paths = default;

            sync.WaitGroup wg = default;
            var sema = make_channel<object>(runtime.NumCPU()); // limit open file descriptors
            {
                var h__prev1 = h;

                foreach (var (__i, __h) in hostobj)
                {
                    i = __i;
                    h = __h;
                    var h = h;
                    var dst = filepath.Join(flagTmpdir.val, fmt.Sprintf("%06d.o", i));
                    paths = append(paths, dst);

                    wg.Add(1L);
                    go_(() => () =>
                    {
                        sema.Send(/* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{});
                        defer(() =>
                        {
                            sema.Receive();
                            wg.Done();
                        }());
                        var (f, err) = os.Open(h.file);
                        if (err != null)
                        {
                            Exitf("cannot reopen %s: %v", h.pn, err);
                        }

                        defer(f.Close());
                        {
                            var (_, err) = f.Seek(h.off, 0L);

                            if (err != null)
                            {
                                Exitf("cannot seek %s: %v", h.pn, err);
                            }

                        }


                        var (w, err) = os.Create(dst);
                        if (err != null)
                        {
                            Exitf("cannot create %s: %v", dst, err);
                        }

                        {
                            (_, err) = io.CopyN(w, f, h.length);

                            if (err != null)
                            {
                                Exitf("cannot write %s: %v", dst, err);
                            }

                        }

                        {
                            var err = w.Close();

                            if (err != null)
                            {
                                Exitf("cannot close %s: %v", dst, err);
                            }

                        }

                    }());

                }

                h = h__prev1;
            }

            wg.Wait();
            return paths;

        });

        // writeGDBLinkerScript creates gcc linker script file in temp
        // directory. writeGDBLinkerScript returns created file path.
        // The script is used to work around gcc bug
        // (see https://golang.org/issue/20183 for details).
        private static @string writeGDBLinkerScript()
        {
            @string name = "fix_debug_gdb_scripts.ld";
            var path = filepath.Join(flagTmpdir.val, name);
            @string src = "SECTIONS\n{\n  .debug_gdb_scripts BLOCK(__section_alignment__) (NOLOAD) :\n  {\n    *" +
    "(.debug_gdb_scripts)\n  }\n}\nINSERT AFTER .debug_types;\n";
            var err = ioutil.WriteFile(path, (slice<byte>)src, 0666L);
            if (err != null)
            {
                Errorf(null, "WriteFile %s failed: %v", name, err);
            }

            return path;

        }

        // archive builds a .a archive from the hostobj object files.
        private static void archive(this ptr<Link> _addr_ctxt) => func((_, panic, __) =>
        {
            ref Link ctxt = ref _addr_ctxt.val;

            if (ctxt.BuildMode != BuildModeCArchive)
            {
                return ;
            }

            exitIfErrors();

            if (flagExtar == "".val)
            {
                flagExtar.val = "ar";
            }

            mayberemoveoutfile(); 

            // Force the buffer to flush here so that external
            // tools will see a complete file.
            ctxt.Out.Flush();
            {
                var err = ctxt.Out.f.Close();

                if (err != null)
                {
                    Exitf("close: %v", err);
                }

            }

            ctxt.Out.f = null;

            @string argv = new slice<@string>(new @string[] { *flagExtar, "-q", "-c", "-s" });
            if (ctxt.HeadType == objabi.Haix)
            {
                argv = append(argv, "-X64");
            }

            argv = append(argv, flagOutfile.val);
            argv = append(argv, filepath.Join(flagTmpdir.val, "go.o"));
            argv = append(argv, hostobjCopy());

            if (ctxt.Debugvlog != 0L)
            {
                ctxt.Logf("archive: %s\n", strings.Join(argv, " "));
            } 

            // If supported, use syscall.Exec() to invoke the archive command,
            // which should be the final remaining step needed for the link.
            // This will reduce peak RSS for the link (and speed up linking of
            // large applications), since when the archive command runs we
            // won't be holding onto all of the linker's live memory.
            if (syscallExecSupported && !ownTmpDir)
            {
                runAtExitFuncs();
                ctxt.execArchive(argv);
                panic("should not get here");
            } 

            // Otherwise invoke 'ar' in the usual way (fork + exec).
            {
                var (out, err) = exec.Command(argv[0L], argv[1L..]).CombinedOutput();

                if (err != null)
                {
                    Exitf("running %s failed: %v\n%s", argv[0L], err, out);
                }

            }

        });

        private static void hostlink(this ptr<Link> _addr_ctxt) => func((defer, _, __) =>
        {
            ref Link ctxt = ref _addr_ctxt.val;

            if (ctxt.LinkMode != LinkExternal || nerrors > 0L)
            {
                return ;
            }

            if (ctxt.BuildMode == BuildModeCArchive)
            {
                return ;
            }

            slice<@string> argv = default;
            argv = append(argv, ctxt.extld());
            argv = append(argv, hostlinkArchArgs(_addr_ctxt.Arch));

            if (FlagS || debug_s.val)
            {
                if (ctxt.HeadType == objabi.Hdarwin)
                { 
                    // Recent versions of macOS print
                    //    ld: warning: option -s is obsolete and being ignored
                    // so do not pass any arguments.
                }
                else
                {
                    argv = append(argv, "-s");
                }

            }


            if (ctxt.HeadType == objabi.Hdarwin) 
                if (machoPlatform == PLATFORM_MACOS)
                { 
                    // -headerpad is incompatible with -fembed-bitcode.
                    argv = append(argv, "-Wl,-headerpad,1144");

                }

                if (ctxt.DynlinkingGo() && !ctxt.Arch.InFamily(sys.ARM, sys.ARM64))
                {
                    argv = append(argv, "-Wl,-flat_namespace");
                }

            else if (ctxt.HeadType == objabi.Hopenbsd) 
                argv = append(argv, "-Wl,-nopie");
            else if (ctxt.HeadType == objabi.Hwindows) 
                if (windowsgui)
                {
                    argv = append(argv, "-mwindows");
                }
                else
                {
                    argv = append(argv, "-mconsole");
                } 
                // Mark as having awareness of terminal services, to avoid
                // ancient compatibility hacks.
                argv = append(argv, "-Wl,--tsaware"); 

                // Enable DEP
                argv = append(argv, "-Wl,--nxcompat");

                argv = append(argv, fmt.Sprintf("-Wl,--major-os-version=%d", PeMinimumTargetMajorVersion));
                argv = append(argv, fmt.Sprintf("-Wl,--minor-os-version=%d", PeMinimumTargetMinorVersion));
                argv = append(argv, fmt.Sprintf("-Wl,--major-subsystem-version=%d", PeMinimumTargetMajorVersion));
                argv = append(argv, fmt.Sprintf("-Wl,--minor-subsystem-version=%d", PeMinimumTargetMinorVersion));
            else if (ctxt.HeadType == objabi.Haix) 
                argv = append(argv, "-pthread"); 
                // prevent ld to reorder .text functions to keep the same
                // first/last functions for moduledata.
                argv = append(argv, "-Wl,-bnoobjreorder"); 
                // mcmodel=large is needed for every gcc generated files, but
                // ld still need -bbigtoc in order to allow larger TOC.
                argv = append(argv, "-mcmodel=large");
                argv = append(argv, "-Wl,-bbigtoc");
            
            if (ctxt.BuildMode == BuildModeExe) 
                if (ctxt.HeadType == objabi.Hdarwin)
                {
                    if (machoPlatform == PLATFORM_MACOS)
                    {
                        argv = append(argv, "-Wl,-no_pie");
                        argv = append(argv, "-Wl,-pagezero_size,4000000");
                    }

                }

            else if (ctxt.BuildMode == BuildModePIE) 

                if (ctxt.HeadType == objabi.Hdarwin || ctxt.HeadType == objabi.Haix)                 else if (ctxt.HeadType == objabi.Hwindows) 
                    // Enable ASLR.
                    argv = append(argv, "-Wl,--dynamicbase"); 
                    // enable high-entropy ASLR on 64-bit.
                    if (ctxt.Arch.PtrSize >= 8L)
                    {
                        argv = append(argv, "-Wl,--high-entropy-va");
                    } 
                    // Work around binutils limitation that strips relocation table for dynamicbase.
                    // See https://sourceware.org/bugzilla/show_bug.cgi?id=19011
                    argv = append(argv, "-Wl,--export-all-symbols");
                else 
                    // ELF.
                    if (ctxt.UseRelro())
                    {
                        argv = append(argv, "-Wl,-z,relro");
                    }

                    argv = append(argv, "-pie");
                            else if (ctxt.BuildMode == BuildModeCShared) 
                if (ctxt.HeadType == objabi.Hdarwin)
                {
                    argv = append(argv, "-dynamiclib");
                    if (ctxt.Arch.Family != sys.AMD64)
                    {
                        argv = append(argv, "-Wl,-read_only_relocs,suppress");
                    }

                }
                else
                { 
                    // ELF.
                    argv = append(argv, "-Wl,-Bsymbolic");
                    if (ctxt.UseRelro())
                    {
                        argv = append(argv, "-Wl,-z,relro");
                    }

                    argv = append(argv, "-shared");
                    if (ctxt.HeadType != objabi.Hwindows)
                    { 
                        // Pass -z nodelete to mark the shared library as
                        // non-closeable: a dlclose will do nothing.
                        argv = append(argv, "-Wl,-z,nodelete");

                    }

                }

            else if (ctxt.BuildMode == BuildModeShared) 
                if (ctxt.UseRelro())
                {
                    argv = append(argv, "-Wl,-z,relro");
                }

                argv = append(argv, "-shared");
            else if (ctxt.BuildMode == BuildModePlugin) 
                if (ctxt.HeadType == objabi.Hdarwin)
                {
                    argv = append(argv, "-dynamiclib");
                }
                else
                {
                    if (ctxt.UseRelro())
                    {
                        argv = append(argv, "-Wl,-z,relro");
                    }

                    argv = append(argv, "-shared");

                }

                        if (ctxt.IsELF && ctxt.DynlinkingGo())
            { 
                // We force all symbol resolution to be done at program startup
                // because lazy PLT resolution can use large amounts of stack at
                // times we cannot allow it to do so.
                argv = append(argv, "-Wl,-znow"); 

                // Do not let the host linker generate COPY relocations. These
                // can move symbols out of sections that rely on stable offsets
                // from the beginning of the section (like sym.STYPE).
                argv = append(argv, "-Wl,-znocopyreloc");

                if (ctxt.Arch.InFamily(sys.ARM, sys.ARM64) && objabi.GOOS == "linux")
                { 
                    // On ARM, the GNU linker will generate COPY relocations
                    // even with -znocopyreloc set.
                    // https://sourceware.org/bugzilla/show_bug.cgi?id=19962
                    //
                    // On ARM64, the GNU linker will fail instead of
                    // generating COPY relocations.
                    //
                    // In both cases, switch to gold.
                    argv = append(argv, "-fuse-ld=gold"); 

                    // If gold is not installed, gcc will silently switch
                    // back to ld.bfd. So we parse the version information
                    // and provide a useful error if gold is missing.
                    var cmd = exec.Command(flagExtld.val, "-fuse-ld=gold", "-Wl,--version");
                    {
                        var out__prev3 = out;
                        var err__prev3 = err;

                        var (out, err) = cmd.CombinedOutput();

                        if (err == null)
                        {
                            if (!bytes.Contains(out, (slice<byte>)"GNU gold"))
                            {
                                log.Fatalf("ARM external linker must be gold (issue #15696), but is not: %s", out);
                            }

                        }

                        out = out__prev3;
                        err = err__prev3;

                    }

                }

            }

            if (ctxt.Arch.Family == sys.ARM64 && objabi.GOOS == "freebsd")
            { 
                // Switch to ld.bfd on freebsd/arm64.
                argv = append(argv, "-fuse-ld=bfd"); 

                // Provide a useful error if ld.bfd is missing.
                cmd = exec.Command(flagExtld.val, "-fuse-ld=bfd", "-Wl,--version");
                {
                    var out__prev2 = out;
                    var err__prev2 = err;

                    (out, err) = cmd.CombinedOutput();

                    if (err == null)
                    {
                        if (!bytes.Contains(out, (slice<byte>)"GNU ld"))
                        {
                            log.Fatalf("ARM64 external linker must be ld.bfd (issue #35197), please install devel/binutils");
                        }

                    }

                    out = out__prev2;
                    err = err__prev2;

                }

            }

            if (ctxt.IsELF && len(buildinfo) > 0L)
            {
                argv = append(argv, fmt.Sprintf("-Wl,--build-id=0x%x", buildinfo));
            } 

            // On Windows, given -o foo, GCC will append ".exe" to produce
            // "foo.exe".  We have decided that we want to honor the -o
            // option. To make this work, we append a '.' so that GCC
            // will decide that the file already has an extension. We
            // only want to do this when producing a Windows output file
            // on a Windows host.
            var outopt = flagOutfile.val;
            if (objabi.GOOS == "windows" && runtime.GOOS == "windows" && filepath.Ext(outopt) == "")
            {
                outopt += ".";
            }

            argv = append(argv, "-o");
            argv = append(argv, outopt);

            if (rpath.val != "")
            {
                argv = append(argv, fmt.Sprintf("-Wl,-rpath,%s", rpath.val));
            } 

            // Force global symbols to be exported for dlopen, etc.
            if (ctxt.IsELF)
            {
                argv = append(argv, "-rdynamic");
            }

            if (ctxt.HeadType == objabi.Haix)
            {
                var fileName = xcoffCreateExportFile(ctxt);
                argv = append(argv, "-Wl,-bE:" + fileName);
            }

            if (strings.Contains(argv[0L], "clang"))
            {
                argv = append(argv, "-Qunused-arguments");
            }

            const @string compressDWARF = (@string)"-Wl,--compress-debug-sections=zlib-gnu";

            if (ctxt.compressDWARF && linkerFlagSupported(argv[0L], compressDWARF))
            {
                argv = append(argv, compressDWARF);
            }

            argv = append(argv, filepath.Join(flagTmpdir.val, "go.o"));
            argv = append(argv, hostobjCopy());
            if (ctxt.HeadType == objabi.Haix)
            { 
                // We want to have C files after Go files to remove
                // trampolines csects made by ld.
                argv = append(argv, "-nostartfiles");
                argv = append(argv, "/lib/crt0_64.o");

                var extld = ctxt.extld(); 
                // Get starting files.
                Func<@string, @string> getPathFile = file =>
                {
                    @string args = new slice<@string>(new @string[] { "-maix64", "--print-file-name="+file });
                    (out, err) = exec.Command(extld, args).CombinedOutput();
                    if (err != null)
                    {
                        log.Fatalf("running %s failed: %v\n%s", extld, err, out);
                    }

                    return strings.Trim(string(out), "\n");

                }
;
                argv = append(argv, getPathFile("crtcxa.o"));
                argv = append(argv, getPathFile("crtdbase.o"));

            }

            if (ctxt.linkShared)
            {
                var seenDirs = make_map<@string, bool>();
                var seenLibs = make_map<@string, bool>();
                Action<@string> addshlib = path =>
                {
                    var (dir, base) = filepath.Split(path);
                    if (!seenDirs[dir])
                    {
                        argv = append(argv, "-L" + dir);
                        if (!rpath.set)
                        {
                            argv = append(argv, "-Wl,-rpath=" + dir);
                        }

                        seenDirs[dir] = true;

                    }

                    base = strings.TrimSuffix(base, ".so");
                    base = strings.TrimPrefix(base, "lib");
                    if (!seenLibs[base])
                    {
                        argv = append(argv, "-l" + base);
                        seenLibs[base] = true;
                    }

                }
;
                foreach (var (_, shlib) in ctxt.Shlibs)
                {
                    addshlib(shlib.Path);
                    foreach (var (_, dep) in shlib.Deps)
                    {
                        if (dep == "")
                        {
                            continue;
                        }

                        var libpath = findshlib(_addr_ctxt, dep);
                        if (libpath != "")
                        {
                            addshlib(libpath);
                        }

                    }

                }

            } 

            // clang, unlike GCC, passes -rdynamic to the linker
            // even when linking with -static, causing a linker
            // error when using GNU ld. So take out -rdynamic if
            // we added it. We do it in this order, rather than
            // only adding -rdynamic later, so that -*extldflags
            // can override -rdynamic without using -static.
            Action<@string> checkStatic = arg =>
            {
                if (ctxt.IsELF && arg == "-static")
                {
                    foreach (var (i) in argv)
                    {
                        if (argv[i] == "-rdynamic")
                        {
                            argv[i] = "-static";
                        }

                    }

                }

            }
;

            {
                var p__prev1 = p;

                foreach (var (_, __p) in ldflag)
                {
                    p = __p;
                    argv = append(argv, p);
                    checkStatic(p);
                } 

                // When building a program with the default -buildmode=exe the
                // gc compiler generates code requires DT_TEXTREL in a
                // position independent executable (PIE). On systems where the
                // toolchain creates PIEs by default, and where DT_TEXTREL
                // does not work, the resulting programs will not run. See
                // issue #17847. To avoid this problem pass -no-pie to the
                // toolchain if it is supported.

                p = p__prev1;
            }

            if (ctxt.BuildMode == BuildModeExe && !ctxt.linkShared)
            { 
                // GCC uses -no-pie, clang uses -nopie.
                foreach (var (_, nopie) in new slice<@string>(new @string[] { "-no-pie", "-nopie" }))
                {
                    if (linkerFlagSupported(argv[0L], nopie))
                    {
                        argv = append(argv, nopie);
                        break;
                    }

                }

            }

            {
                var p__prev1 = p;

                foreach (var (_, __p) in strings.Fields(flagExtldflags.val))
                {
                    p = __p;
                    argv = append(argv, p);
                    checkStatic(p);
                }

                p = p__prev1;
            }

            if (ctxt.HeadType == objabi.Hwindows)
            { 
                // use gcc linker script to work around gcc bug
                // (see https://golang.org/issue/20183 for details).
                var p = writeGDBLinkerScript();
                argv = append(argv, "-Wl,-T," + p); 
                // libmingw32 and libmingwex have some inter-dependencies,
                // so must use linker groups.
                argv = append(argv, "-Wl,--start-group", "-lmingwex", "-lmingw32", "-Wl,--end-group");
                argv = append(argv, peimporteddlls());

            }

            if (ctxt.Debugvlog != 0L)
            {
                ctxt.Logf("host link:");
                foreach (var (_, v) in argv)
                {
                    ctxt.Logf(" %q", v);
                }
                ctxt.Logf("\n");

            }

            (out, err) = exec.Command(argv[0L], argv[1L..]).CombinedOutput();
            if (err != null)
            {
                Exitf("running %s failed: %v\n%s", argv[0L], err, out);
            } 

            // Filter out useless linker warnings caused by bugs outside Go.
            // See also cmd/go/internal/work/exec.go's gccld method.
            slice<slice<byte>> save = default;
            long skipLines = default;
            foreach (var (_, line) in bytes.SplitAfter(out, (slice<byte>)"\n"))
            { 
                // golang.org/issue/26073 - Apple Xcode bug
                if (bytes.Contains(line, (slice<byte>)"ld: warning: text-based stub file"))
                {
                    continue;
                }

                if (skipLines > 0L)
                {
                    skipLines--;
                    continue;
                } 

                // Remove TOC overflow warning on AIX.
                if (bytes.Contains(line, (slice<byte>)"ld: 0711-783"))
                {
                    skipLines = 2L;
                    continue;
                }

                save = append(save, line);

            }
            out = bytes.Join(save, null);

            if (len(out) > 0L)
            { 
                // always print external output even if the command is successful, so that we don't
                // swallow linker warnings (see https://golang.org/issue/17935).
                ctxt.Logf("%s", out);

            }

            if (!FlagS && !FlagW && !debug_s && ctxt.HeadType == objabi.Hdarwin.val)
            {
                var dsym = filepath.Join(flagTmpdir.val, "go.dwarf");
                {
                    var out__prev2 = out;
                    var err__prev2 = err;

                    (out, err) = exec.Command("dsymutil", "-f", flagOutfile.val, "-o", dsym).CombinedOutput();

                    if (err != null)
                    {
                        Exitf("%s: running dsymutil failed: %v\n%s", os.Args[0L], err, out);
                    } 
                    // Skip combining if `dsymutil` didn't generate a file. See #11994.

                    out = out__prev2;
                    err = err__prev2;

                } 
                // Skip combining if `dsymutil` didn't generate a file. See #11994.
                {
                    var err__prev2 = err;

                    var (_, err) = os.Stat(dsym);

                    if (os.IsNotExist(err))
                    {
                        return ;
                    } 
                    // For os.Rename to work reliably, must be in same directory as outfile.

                    err = err__prev2;

                } 
                // For os.Rename to work reliably, must be in same directory as outfile.
                var combinedOutput = flagOutfile + "~".val;
                var (exef, err) = os.Open(flagOutfile.val);
                if (err != null)
                {
                    Exitf("%s: combining dwarf failed: %v", os.Args[0L], err);
                }

                defer(exef.Close());
                var (exem, err) = macho.NewFile(exef);
                if (err != null)
                {
                    Exitf("%s: parsing Mach-O header failed: %v", os.Args[0L], err);
                } 
                // Only macOS supports unmapped segments such as our __DWARF segment.
                if (machoPlatform == PLATFORM_MACOS)
                {
                    {
                        var err__prev3 = err;

                        var err = machoCombineDwarf(ctxt, exef, exem, dsym, combinedOutput);

                        if (err != null)
                        {
                            Exitf("%s: combining dwarf failed: %v", os.Args[0L], err);
                        }

                        err = err__prev3;

                    }

                    os.Remove(flagOutfile.val);
                    {
                        var err__prev3 = err;

                        err = os.Rename(combinedOutput, flagOutfile.val);

                        if (err != null)
                        {
                            Exitf("%s: %v", os.Args[0L], err);
                        }

                        err = err__prev3;

                    }

                }

            }

        });

        private static sync.Once createTrivialCOnce = default;

        private static bool linkerFlagSupported(@string linker, @string flag)
        {
            createTrivialCOnce.Do(() =>
            {
                var src = filepath.Join(flagTmpdir.val, "trivial.c");
                {
                    var err = ioutil.WriteFile(src, (slice<byte>)"int main() { return 0; }", 0666L);

                    if (err != null)
                    {
                        Errorf(null, "WriteFile trivial.c failed: %v", err);
                    }

                }

            });

            @string flagsWithNextArgSkip = new slice<@string>(new @string[] { "-F", "-l", "-L", "-framework", "-Wl,-framework", "-Wl,-rpath", "-Wl,-undefined" });
            @string flagsWithNextArgKeep = new slice<@string>(new @string[] { "-arch", "-isysroot", "--sysroot", "-target" });
            @string prefixesToKeep = new slice<@string>(new @string[] { "-f", "-m", "-p", "-Wl,", "-arch", "-isysroot", "--sysroot", "-target" });

            slice<@string> flags = default;
            var keep = false;
            var skip = false;
            var extldflags = strings.Fields(flagExtldflags.val);
            foreach (var (_, f) in append(extldflags, ldflag))
            {
                if (keep)
                {
                    flags = append(flags, f);
                    keep = false;
                }
                else if (skip)
                {
                    skip = false;
                }
                else if (f == "" || f[0L] != '-')
                {
                }
                else if (contains(flagsWithNextArgSkip, f))
                {
                    skip = true;
                }
                else if (contains(flagsWithNextArgKeep, f))
                {
                    flags = append(flags, f);
                    keep = true;
                }
                else
                {
                    foreach (var (_, p) in prefixesToKeep)
                    {
                        if (strings.HasPrefix(f, p))
                        {
                            flags = append(flags, f);
                            break;
                        }

                    }

                }

            }
            flags = append(flags, flag, "trivial.c");

            var cmd = exec.Command(linker, flags);
            cmd.Dir = flagTmpdir.val;
            cmd.Env = append(new slice<@string>(new @string[] { "LC_ALL=C" }), os.Environ());
            var (out, err) = cmd.CombinedOutput(); 
            // GCC says "unrecognized command line option ‘-no-pie’"
            // clang says "unknown argument: '-no-pie'"
            return err == null && !bytes.Contains(out, (slice<byte>)"unrecognized") && !bytes.Contains(out, (slice<byte>)"unknown");

        }

        // hostlinkArchArgs returns arguments to pass to the external linker
        // based on the architecture.
        private static slice<@string> hostlinkArchArgs(ptr<sys.Arch> _addr_arch)
        {
            ref sys.Arch arch = ref _addr_arch.val;


            if (arch.Family == sys.I386) 
                return new slice<@string>(new @string[] { "-m32" });
            else if (arch.Family == sys.AMD64 || arch.Family == sys.S390X) 
                return new slice<@string>(new @string[] { "-m64" });
            else if (arch.Family == sys.ARM) 
                return new slice<@string>(new @string[] { "-marm" });
            else if (arch.Family == sys.ARM64)             else if (arch.Family == sys.MIPS64) 
                return new slice<@string>(new @string[] { "-mabi=64" });
            else if (arch.Family == sys.MIPS) 
                return new slice<@string>(new @string[] { "-mabi=32" });
            else if (arch.Family == sys.PPC64) 
                if (objabi.GOOS == "aix")
                {
                    return new slice<@string>(new @string[] { "-maix64" });
                }
                else
                {
                    return new slice<@string>(new @string[] { "-m64" });
                }

                        return null;

        }

        // ldobj loads an input object. If it is a host object (an object
        // compiled by a non-Go compiler) it returns the Hostobj pointer. If
        // it is a Go object, it returns nil.
        private static ptr<Hostobj> ldobj(ptr<Link> _addr_ctxt, ptr<bio.Reader> _addr_f, ptr<sym.Library> _addr_lib, long length, @string pn, @string file)
        {
            ref Link ctxt = ref _addr_ctxt.val;
            ref bio.Reader f = ref _addr_f.val;
            ref sym.Library lib = ref _addr_lib.val;

            var pkg = objabi.PathToPrefix(lib.Pkg);

            var eof = f.Offset() + length;
            var start = f.Offset();
            var c1 = bgetc(_addr_f);
            var c2 = bgetc(_addr_f);
            var c3 = bgetc(_addr_f);
            var c4 = bgetc(_addr_f);
            f.MustSeek(start, 0L);

            ptr<sym.CompilationUnit> unit = addr(new sym.CompilationUnit(Lib:lib));
            lib.Units = append(lib.Units, unit);

            var magic = uint32(c1) << (int)(24L) | uint32(c2) << (int)(16L) | uint32(c3) << (int)(8L) | uint32(c4);
            if (magic == 0x7f454c46UL)
            { // \x7F E L F
                if (flagNewobj.val)
                {
                    Action<ptr<Link>, ptr<bio.Reader>, @string, long, @string> ldelf = (ctxt, f, pkg, length, pn) =>
                    {
                        var (textp, flags, err) = loadelf.Load(ctxt.loader, ctxt.Arch, ctxt.Syms, f, pkg, length, pn, ehdr.flags);
                        if (err != null)
                        {
                            Errorf(null, "%v", err);
                            return ;
                        }

                        ehdr.flags = flags;
                        ctxt.Textp = append(ctxt.Textp, textp);

                    }
                else
;
                    return _addr_ldhostobj(ldelf, ctxt.HeadType, _addr_f, pkg, length, pn, file)!;

                }                {
                    ldelf = (ctxt, f, pkg, length, pn) =>
                    {
                        (textp, flags, err) = loadelf.LoadOld(ctxt.Arch, ctxt.Syms, f, pkg, length, pn, ehdr.flags);
                        if (err != null)
                        {
                            Errorf(null, "%v", err);
                            return ;
                        }

                        ehdr.flags = flags;
                        ctxt.Textp = append(ctxt.Textp, textp);

                    }
;
                    return _addr_ldhostobj(ldelf, ctxt.HeadType, _addr_f, pkg, length, pn, file)!;

                }

            }

            if (magic & ~1L == 0xfeedfaceUL || magic & ~0x01000000UL == 0xcefaedfeUL)
            {
                if (flagNewobj.val)
                {
                    Action<ptr<Link>, ptr<bio.Reader>, @string, long, @string> ldmacho = (ctxt, f, pkg, length, pn) =>
                    {
                        var (textp, err) = loadmacho.Load(ctxt.loader, ctxt.Arch, ctxt.Syms, f, pkg, length, pn);
                        if (err != null)
                        {
                            Errorf(null, "%v", err);
                            return ;
                        }

                        ctxt.Textp = append(ctxt.Textp, textp);

                    }
                else
;
                    return _addr_ldhostobj(ldmacho, ctxt.HeadType, _addr_f, pkg, length, pn, file)!;

                }                {
                    ldmacho = (ctxt, f, pkg, length, pn) =>
                    {
                        (textp, err) = loadmacho.LoadOld(ctxt.Arch, ctxt.Syms, f, pkg, length, pn);
                        if (err != null)
                        {
                            Errorf(null, "%v", err);
                            return ;
                        }

                        ctxt.Textp = append(ctxt.Textp, textp);

                    }
;
                    return _addr_ldhostobj(ldmacho, ctxt.HeadType, _addr_f, pkg, length, pn, file)!;

                }

            }

            if (c1 == 0x4cUL && c2 == 0x01UL || c1 == 0x64UL && c2 == 0x86UL)
            {
                if (flagNewobj.val)
                {
                    Action<ptr<Link>, ptr<bio.Reader>, @string, long, @string> ldpe = (ctxt, f, pkg, length, pn) =>
                    {
                        var (textp, rsrc, err) = loadpe.Load(ctxt.loader, ctxt.Arch, ctxt.Syms, f, pkg, length, pn);
                        if (err != null)
                        {
                            Errorf(null, "%v", err);
                            return ;
                        }

                        if (rsrc != null)
                        {
                            setpersrc(ctxt, rsrc);
                        }

                        ctxt.Textp = append(ctxt.Textp, textp);

                    }
                else
;
                    return _addr_ldhostobj(ldpe, ctxt.HeadType, _addr_f, pkg, length, pn, file)!;

                }                {
                    ldpe = (ctxt, f, pkg, length, pn) =>
                    {
                        (textp, rsrc, err) = loadpe.LoadOld(ctxt.Arch, ctxt.Syms, f, pkg, length, pn);
                        if (err != null)
                        {
                            Errorf(null, "%v", err);
                            return ;
                        }

                        if (rsrc != null)
                        {
                            setpersrc(ctxt, rsrc);
                        }

                        ctxt.Textp = append(ctxt.Textp, textp);

                    }
;
                    return _addr_ldhostobj(ldpe, ctxt.HeadType, _addr_f, pkg, length, pn, file)!;

                }

            }

            if (c1 == 0x01UL && (c2 == 0xD7UL || c2 == 0xF7UL))
            {
                if (flagNewobj.val)
                {
                    Action<ptr<Link>, ptr<bio.Reader>, @string, long, @string> ldxcoff = (ctxt, f, pkg, length, pn) =>
                    {
                        (textp, err) = loadxcoff.Load(ctxt.loader, ctxt.Arch, ctxt.Syms, f, pkg, length, pn);
                        if (err != null)
                        {
                            Errorf(null, "%v", err);
                            return ;
                        }

                        ctxt.Textp = append(ctxt.Textp, textp);

                    }
                else
;
                    return _addr_ldhostobj(ldxcoff, ctxt.HeadType, _addr_f, pkg, length, pn, file)!;

                }                {
                    ldxcoff = (ctxt, f, pkg, length, pn) =>
                    {
                        (textp, err) = loadxcoff.LoadOld(ctxt.Arch, ctxt.Syms, f, pkg, length, pn);
                        if (err != null)
                        {
                            Errorf(null, "%v", err);
                            return ;
                        }

                        ctxt.Textp = append(ctxt.Textp, textp);

                    }
;
                    return _addr_ldhostobj(ldxcoff, ctxt.HeadType, _addr_f, pkg, length, pn, file)!;

                }

            } 

            /* check the header */
            var (line, err) = f.ReadString('\n');
            if (err != null)
            {
                Errorf(null, "truncated object file: %s: %v", pn, err);
                return _addr_null!;
            }

            if (!strings.HasPrefix(line, "go object "))
            {
                if (strings.HasSuffix(pn, ".go"))
                {
                    Exitf("%s: uncompiled .go source file", pn);
                    return _addr_null!;
                }

                if (line == ctxt.Arch.Name)
                { 
                    // old header format: just $GOOS
                    Errorf(null, "%s: stale object file", pn);
                    return _addr_null!;

                }

                Errorf(null, "%s: not an object file", pn);
                return _addr_null!;

            } 

            // First, check that the basic GOOS, GOARCH, and Version match.
            var t = fmt.Sprintf("%s %s %s ", objabi.GOOS, objabi.GOARCH, objabi.Version);

            line = strings.TrimRight(line, "\n");
            if (!strings.HasPrefix(line[10L..] + " ", t) && !flagF.val)
            {
                Errorf(null, "%s: object is [%s] expected [%s]", pn, line[10L..], t);
                return _addr_null!;
            } 

            // Second, check that longer lines match each other exactly,
            // so that the Go compiler and write additional information
            // that must be the same from run to run.
            if (len(line) >= len(t) + 10L)
            {
                if (theline == "")
                {
                    theline = line[10L..];
                }
                else if (theline != line[10L..])
                {
                    Errorf(null, "%s: object is [%s] expected [%s]", pn, line[10L..], theline);
                    return _addr_null!;
                }

            } 

            // Skip over exports and other info -- ends with \n!\n.
            //
            // Note: It's possible for "\n!\n" to appear within the binary
            // package export data format. To avoid truncating the package
            // definition prematurely (issue 21703), we keep track of
            // how many "$$" delimiters we've seen.
            var import0 = f.Offset();

            c1 = '\n'; // the last line ended in \n
            c2 = bgetc(_addr_f);
            c3 = bgetc(_addr_f);
            long markers = 0L;
            while (true)
            {
                if (c1 == '\n')
                {
                    if (markers % 2L == 0L && c2 == '!' && c3 == '\n')
                    {
                        break;
                    }

                    if (c2 == '$' && c3 == '$')
                    {
                        markers++;
                    }

                }

                c1 = c2;
                c2 = c3;
                c3 = bgetc(_addr_f);
                if (c3 == -1L)
                {
                    Errorf(null, "truncated object file: %s", pn);
                    return _addr_null!;
                }

            }


            var import1 = f.Offset();

            f.MustSeek(import0, 0L);
            ldpkg(ctxt, f, lib, import1 - import0 - 2L, pn); // -2 for !\n
            f.MustSeek(import1, 0L);

            long flags = 0L;
            switch (FlagStrictDups.val)
            {
                case 0L: 
                    break;
                    break;
                case 1L: 
                    flags = objfile.StrictDupsWarnFlag;
                    break;
                case 2L: 
                    flags = objfile.StrictDupsErrFlag;
                    break;
                default: 
                    log.Fatalf("invalid -strictdups flag value %d", FlagStrictDups.val);
                    break;
            }
            long c = default;
            if (flagNewobj.val)
            {
                ctxt.loader.Preload(ctxt.Arch, ctxt.Syms, f, lib, unit, eof - f.Offset(), pn, flags);
            }
            else
            {
                c = objfile.Load(ctxt.Arch, ctxt.Syms, f, lib, unit, eof - f.Offset(), pn, flags);
            }

            strictDupMsgCount += c;
            addImports(ctxt, lib, pn);
            return _addr_null!;

        }

        private static slice<byte> readelfsymboldata(ptr<Link> _addr_ctxt, ptr<elf.File> _addr_f, ptr<elf.Symbol> _addr_sym)
        {
            ref Link ctxt = ref _addr_ctxt.val;
            ref elf.File f = ref _addr_f.val;
            ref elf.Symbol sym = ref _addr_sym.val;

            var data = make_slice<byte>(sym.Size);
            var sect = f.Sections[sym.Section];
            if (sect.Type != elf.SHT_PROGBITS && sect.Type != elf.SHT_NOTE)
            {
                Errorf(null, "reading %s from non-data section", sym.Name);
            }

            var (n, err) = sect.ReadAt(data, int64(sym.Value - sect.Addr));
            if (uint64(n) != sym.Size)
            {
                Errorf(null, "reading contents of %s: %v", sym.Name, err);
            }

            return data;

        }

        private static (slice<byte>, error) readwithpad(io.Reader r, int sz)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;

            var data = make_slice<byte>(Rnd(int64(sz), 4L));
            var (_, err) = io.ReadFull(r, data);
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            data = data[..sz];
            return (data, error.As(null!)!);

        }

        private static (slice<byte>, error) readnote(ptr<elf.File> _addr_f, slice<byte> name, int typ)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;
            ref elf.File f = ref _addr_f.val;

            foreach (var (_, sect) in f.Sections)
            {
                if (sect.Type != elf.SHT_NOTE)
                {
                    continue;
                }

                var r = sect.Open();
                while (true)
                {
                    ref int namesize = ref heap(out ptr<int> _addr_namesize);                    ref int descsize = ref heap(out ptr<int> _addr_descsize);                    ref int noteType = ref heap(out ptr<int> _addr_noteType);

                    var err = binary.Read(r, f.ByteOrder, _addr_namesize);
                    if (err != null)
                    {
                        if (err == io.EOF)
                        {
                            break;
                        }

                        return (null, error.As(fmt.Errorf("read namesize failed: %v", err))!);

                    }

                    err = binary.Read(r, f.ByteOrder, _addr_descsize);
                    if (err != null)
                    {
                        return (null, error.As(fmt.Errorf("read descsize failed: %v", err))!);
                    }

                    err = binary.Read(r, f.ByteOrder, _addr_noteType);
                    if (err != null)
                    {
                        return (null, error.As(fmt.Errorf("read type failed: %v", err))!);
                    }

                    var (noteName, err) = readwithpad(r, namesize);
                    if (err != null)
                    {
                        return (null, error.As(fmt.Errorf("read name failed: %v", err))!);
                    }

                    var (desc, err) = readwithpad(r, descsize);
                    if (err != null)
                    {
                        return (null, error.As(fmt.Errorf("read desc failed: %v", err))!);
                    }

                    if (string(name) == string(noteName) && typ == noteType)
                    {
                        return (desc, error.As(null!)!);
                    }

                }


            }
            return (null, error.As(null!)!);

        }

        private static @string findshlib(ptr<Link> _addr_ctxt, @string shlib)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            if (filepath.IsAbs(shlib))
            {
                return shlib;
            }

            foreach (var (_, libdir) in ctxt.Libdir)
            {
                var libpath = filepath.Join(libdir, shlib);
                {
                    var (_, err) = os.Stat(libpath);

                    if (err == null)
                    {
                        return libpath;
                    }

                }

            }
            Errorf(null, "cannot find shared library: %s", shlib);
            return "";

        }

        private static void ldshlibsyms(ptr<Link> _addr_ctxt, @string shlib)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            @string libpath = default;
            if (filepath.IsAbs(shlib))
            {
                libpath = shlib;
                shlib = filepath.Base(shlib);
            }
            else
            {
                libpath = findshlib(_addr_ctxt, shlib);
                if (libpath == "")
                {
                    return ;
                }

            }

            foreach (var (_, processedlib) in ctxt.Shlibs)
            {
                if (processedlib.Path == libpath)
                {
                    return ;
                }

            }
            if (ctxt.Debugvlog > 1L)
            {
                ctxt.Logf("ldshlibsyms: found library with name %s at %s\n", shlib, libpath);
            }

            var (f, err) = elf.Open(libpath);
            if (err != null)
            {
                Errorf(null, "cannot open shared library: %s", libpath);
                return ;
            } 
            // Keep the file open as decodetypeGcprog needs to read from it.
            // TODO: fix. Maybe mmap the file.
            //defer f.Close()
            var (hash, err) = readnote(_addr_f, ELF_NOTE_GO_NAME, ELF_NOTE_GOABIHASH_TAG);
            if (err != null)
            {
                Errorf(null, "cannot read ABI hash from shared library %s: %v", libpath, err);
                return ;
            }

            var (depsbytes, err) = readnote(_addr_f, ELF_NOTE_GO_NAME, ELF_NOTE_GODEPS_TAG);
            if (err != null)
            {
                Errorf(null, "cannot read dep list from shared library %s: %v", libpath, err);
                return ;
            }

            slice<@string> deps = default;
            foreach (var (_, dep) in strings.Split(string(depsbytes), "\n"))
            {
                if (dep == "")
                {
                    continue;
                }

                if (!filepath.IsAbs(dep))
                { 
                    // If the dep can be interpreted as a path relative to the shlib
                    // in which it was found, do that. Otherwise, we will leave it
                    // to be resolved by libdir lookup.
                    var abs = filepath.Join(filepath.Dir(libpath), dep);
                    {
                        var (_, err) = os.Stat(abs);

                        if (err == null)
                        {
                            dep = abs;
                        }

                    }

                }

                deps = append(deps, dep);

            }
            var (syms, err) = f.DynamicSymbols();
            if (err != null)
            {
                Errorf(null, "cannot read symbols from shared library: %s", libpath);
                return ;
            }

            var gcdataLocations = make_map<ulong, ptr<sym.Symbol>>();
            foreach (var (_, elfsym) in syms)
            {
                if (elf.ST_TYPE(elfsym.Info) == elf.STT_NOTYPE || elf.ST_TYPE(elfsym.Info) == elf.STT_SECTION)
                {
                    continue;
                } 

                // Symbols whose names start with "type." are compiler
                // generated, so make functions with that prefix internal.
                long ver = 0L;
                if (elf.ST_TYPE(elfsym.Info) == elf.STT_FUNC && strings.HasPrefix(elfsym.Name, "type."))
                {
                    ver = sym.SymVerABIInternal;
                }

                ptr<sym.Symbol> lsym;
                if (flagNewobj.val)
                {
                    var i = ctxt.loader.AddExtSym(elfsym.Name, ver);
                    if (i == 0L)
                    {
                        continue;
                    }

                    lsym = ctxt.Syms.Newsym(elfsym.Name, ver);
                    ctxt.loader.Syms[i] = lsym;

                }
                else
                {
                    lsym = ctxt.Syms.Lookup(elfsym.Name, ver);
                } 
                // Because loadlib above loads all .a files before loading any shared
                // libraries, any non-dynimport symbols we find that duplicate symbols
                // already loaded should be ignored (the symbols from the .a files
                // "win").
                if (lsym.Type != 0L && lsym.Type != sym.SDYNIMPORT)
                {
                    continue;
                }

                lsym.Type = sym.SDYNIMPORT;
                lsym.SetElfType(elf.ST_TYPE(elfsym.Info));
                lsym.Size = int64(elfsym.Size);
                if (elfsym.Section != elf.SHN_UNDEF)
                { 
                    // Set .File for the library that actually defines the symbol.
                    lsym.File = libpath; 
                    // The decodetype_* functions in decodetype.go need access to
                    // the type data.
                    if (strings.HasPrefix(lsym.Name, "type.") && !strings.HasPrefix(lsym.Name, "type.."))
                    {
                        lsym.P = readelfsymboldata(_addr_ctxt, _addr_f, _addr_elfsym);
                        gcdataLocations[elfsym.Value + 2L * uint64(ctxt.Arch.PtrSize) + 8L + 1L * uint64(ctxt.Arch.PtrSize)] = lsym;
                    }

                } 
                // For function symbols, we don't know what ABI is
                // available, so alias it under both ABIs.
                //
                // TODO(austin): This is almost certainly wrong once
                // the ABIs are actually different. We might have to
                // mangle Go function names in the .so to include the
                // ABI.
                if (elf.ST_TYPE(elfsym.Info) == elf.STT_FUNC && ver == 0L)
                {
                    ptr<sym.Symbol> alias;
                    if (flagNewobj.val)
                    {
                        i = ctxt.loader.AddExtSym(elfsym.Name, sym.SymVerABIInternal);
                        if (i == 0L)
                        {
                            continue;
                        }

                        alias = ctxt.Syms.Newsym(elfsym.Name, sym.SymVerABIInternal);
                        ctxt.loader.Syms[i] = alias;

                    }
                    else
                    {
                        alias = ctxt.Syms.Lookup(elfsym.Name, sym.SymVerABIInternal);
                    }

                    if (alias.Type != 0L)
                    {
                        continue;
                    }

                    alias.Type = sym.SABIALIAS;
                    alias.R = new slice<sym.Reloc>(new sym.Reloc[] { {Sym:lsym} });

                }

            }
            var gcdataAddresses = make_map<ptr<sym.Symbol>, ulong>();
            if (ctxt.Arch.Family == sys.ARM64)
            {
                foreach (var (_, sect) in f.Sections)
                {
                    if (sect.Type == elf.SHT_RELA)
                    {
                        ref elf.Rela64 rela = ref heap(out ptr<elf.Rela64> _addr_rela);
                        var rdr = sect.Open();
                        while (true)
                        {
                            var err = binary.Read(rdr, f.ByteOrder, _addr_rela);
                            if (err == io.EOF)
                            {
                                break;
                            }
                            else if (err != null)
                            {
                                Errorf(null, "reading relocation failed %v", err);
                                return ;
                            }

                            var t = elf.R_AARCH64(rela.Info & 0xffffUL);
                            if (t != elf.R_AARCH64_RELATIVE)
                            {
                                continue;
                            }

                            {
                                ptr<sym.Symbol> lsym__prev3 = lsym;

                                var (lsym, ok) = gcdataLocations[rela.Off];

                                if (ok)
                                {
                                    gcdataAddresses[lsym] = uint64(rela.Addend);
                                }

                                lsym = lsym__prev3;

                            }

                        }


                    }

                }

            }

            ctxt.Shlibs = append(ctxt.Shlibs, new Shlib(Path:libpath,Hash:hash,Deps:deps,File:f,gcdataAddresses:gcdataAddresses));

        }

        private static ptr<sym.Section> addsection(ptr<sys.Arch> _addr_arch, ptr<sym.Segment> _addr_seg, @string name, long rwx)
        {
            ref sys.Arch arch = ref _addr_arch.val;
            ref sym.Segment seg = ref _addr_seg.val;

            ptr<sym.Section> sect = @new<sym.Section>();
            sect.Rwx = uint8(rwx);
            sect.Name = name;
            sect.Seg = seg;
            sect.Align = int32(arch.PtrSize); // everything is at least pointer-aligned
            seg.Sections = append(seg.Sections, sect);
            return _addr_sect!;

        }

        private partial struct chain
        {
            public ptr<sym.Symbol> sym;
            public ptr<chain> up;
            public long limit; // limit on entry to sym
        }

        private static ptr<sym.Symbol> morestack;

        // TODO: Record enough information in new object files to
        // allow stack checks here.

        private static bool haslinkregister(ptr<Link> _addr_ctxt)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            return ctxt.FixedFrameSize() != 0L;
        }

        private static long callsize(ptr<Link> _addr_ctxt)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            if (haslinkregister(_addr_ctxt))
            {
                return 0L;
            }

            return ctxt.Arch.RegSize;

        }

        private static void dostkcheck(this ptr<Link> _addr_ctxt)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            ref chain ch = ref heap(out ptr<chain> _addr_ch);

            morestack = ctxt.Syms.Lookup("runtime.morestack", 0L); 

            // Every splitting function ensures that there are at least StackLimit
            // bytes available below SP when the splitting prologue finishes.
            // If the splitting function calls F, then F begins execution with
            // at least StackLimit - callsize() bytes available.
            // Check that every function behaves correctly with this amount
            // of stack, following direct calls in order to piece together chains
            // of non-splitting functions.
            ch.up = null;

            ch.limit = objabi.StackLimit - callsize(_addr_ctxt);
            if (objabi.GOARCH == "arm64")
            { 
                // need extra 8 bytes below SP to save FP
                ch.limit -= 8L;

            } 

            // Check every function, but do the nosplit functions in a first pass,
            // to make the printed failure chains as short as possible.
            {
                var s__prev1 = s;

                foreach (var (_, __s) in ctxt.Textp)
                {
                    s = __s; 
                    // runtime.racesymbolizethunk is called from gcc-compiled C
                    // code running on the operating system thread stack.
                    // It uses more than the usual amount of stack but that's okay.
                    if (s.Name == "runtime.racesymbolizethunk")
                    {
                        continue;
                    }

                    if (s.Attr.NoSplit())
                    {
                        ch.sym = s;
                        stkcheck(_addr_ctxt, _addr_ch, 0L);
                    }

                }

                s = s__prev1;
            }

            {
                var s__prev1 = s;

                foreach (var (_, __s) in ctxt.Textp)
                {
                    s = __s;
                    if (!s.Attr.NoSplit())
                    {
                        ch.sym = s;
                        stkcheck(_addr_ctxt, _addr_ch, 0L);
                    }

                }

                s = s__prev1;
            }
        }

        private static long stkcheck(ptr<Link> _addr_ctxt, ptr<chain> _addr_up, long depth)
        {
            ref Link ctxt = ref _addr_ctxt.val;
            ref chain up = ref _addr_up.val;

            var limit = up.limit;
            var s = up.sym; 

            // Don't duplicate work: only need to consider each
            // function at top of safe zone once.
            var top = limit == objabi.StackLimit - callsize(_addr_ctxt);
            if (top)
            {
                if (s.Attr.StackCheck())
                {
                    return 0L;
                }

                s.Attr |= sym.AttrStackCheck;

            }

            if (depth > 500L)
            {
                Errorf(s, "nosplit stack check too deep");
                stkbroke(_addr_ctxt, _addr_up, 0L);
                return -1L;
            }

            if (s.Attr.External() || s.FuncInfo == null)
            { 
                // external function.
                // should never be called directly.
                // onlyctxt.Diagnose the direct caller.
                // TODO(mwhudson): actually think about this.
                // TODO(khr): disabled for now. Calls to external functions can only happen on the g0 stack.
                // See the trampolines in src/runtime/sys_darwin_$ARCH.go.
                if (depth == 1L && s.Type != sym.SXREF && !ctxt.DynlinkingGo() && ctxt.BuildMode != BuildModeCArchive && ctxt.BuildMode != BuildModePIE && ctxt.BuildMode != BuildModeCShared && ctxt.BuildMode != BuildModePlugin)
                { 
                    //Errorf(s, "call to external function")
                }

                return -1L;

            }

            if (limit < 0L)
            {
                stkbroke(_addr_ctxt, _addr_up, limit);
                return -1L;
            } 

            // morestack looks like it calls functions,
            // but it switches the stack pointer first.
            if (s == morestack)
            {
                return 0L;
            }

            ref chain ch = ref heap(out ptr<chain> _addr_ch);
            ch.up = up;

            if (!s.Attr.NoSplit())
            { 
                // Ensure we have enough stack to call morestack.
                ch.limit = limit - callsize(_addr_ctxt);
                ch.sym = morestack;
                if (stkcheck(_addr_ctxt, _addr_ch, depth + 1L) < 0L)
                {
                    return -1L;
                }

                if (!top)
                {
                    return 0L;
                } 
                // Raise limit to allow frame.
                var locals = int32(0L);
                if (s.FuncInfo != null)
                {
                    locals = s.FuncInfo.Locals;
                }

                limit = objabi.StackLimit + int(locals) + int(ctxt.FixedFrameSize());

            } 

            // Walk through sp adjustments in function, consuming relocs.
            long ri = 0L;

            var endr = len(s.R);
            ref chain ch1 = ref heap(out ptr<chain> _addr_ch1);
            var pcsp = obj.NewPCIter(uint32(ctxt.Arch.MinLC));
            ptr<sym.Reloc> r;
            pcsp.Init(s.FuncInfo.Pcsp.P);

            while (!pcsp.Done)
            { 
                // pcsp.value is in effect for [pcsp.pc, pcsp.nextpc).

                // Check stack size in effect for this span.
                if (int32(limit) - pcsp.Value < 0L)
                {
                    stkbroke(_addr_ctxt, _addr_up, int(int32(limit) - pcsp.Value));
                    return -1L;
                pcsp.Next();
                } 

                // Process calls in this span.
                while (ri < endr && uint32(s.R[ri].Off) < pcsp.NextPC)
                {
                    r = _addr_s.R[ri];

                    if (r.Type.IsDirectCall()) 
                        ch.limit = int(int32(limit) - pcsp.Value - int32(callsize(_addr_ctxt)));
                        ch.sym = r.Sym;
                        if (stkcheck(_addr_ctxt, _addr_ch, depth + 1L) < 0L)
                        {
                            return -1L;
                    ri++;
                        } 

                        // Indirect call. Assume it is a call to a splitting function,
                        // so we have to make sure it can call morestack.
                        // Arrange the data structures to report both calls, so that
                        // if there is an error, stkprint shows all the steps involved.
                    else if (r.Type == objabi.R_CALLIND) 
                        ch.limit = int(int32(limit) - pcsp.Value - int32(callsize(_addr_ctxt)));
                        ch.sym = null;
                        ch1.limit = ch.limit - callsize(_addr_ctxt); // for morestack in called prologue
                        _addr_ch1.up = _addr_ch;
                        ch1.up = ref _addr_ch1.up.val;
                        ch1.sym = morestack;
                        if (stkcheck(_addr_ctxt, _addr_ch1, depth + 2L) < 0L)
                        {
                            return -1L;
                        }

                                    }


            }


            return 0L;

        }

        private static void stkbroke(ptr<Link> _addr_ctxt, ptr<chain> _addr_ch, long limit)
        {
            ref Link ctxt = ref _addr_ctxt.val;
            ref chain ch = ref _addr_ch.val;

            Errorf(ch.sym, "nosplit stack overflow");
            stkprint(_addr_ctxt, _addr_ch, limit);
        }

        private static void stkprint(ptr<Link> _addr_ctxt, ptr<chain> _addr_ch, long limit)
        {
            ref Link ctxt = ref _addr_ctxt.val;
            ref chain ch = ref _addr_ch.val;

            @string name = default;

            if (ch.sym != null)
            {
                name = ch.sym.Name;
                if (ch.sym.Attr.NoSplit())
                {
                    name += " (nosplit)";
                }

            }
            else
            {
                name = "function pointer";
            }

            if (ch.up == null)
            { 
                // top of chain.  ch->sym != nil.
                if (ch.sym.Attr.NoSplit())
                {
                    fmt.Printf("\t%d\tassumed on entry to %s\n", ch.limit, name);
                }
                else
                {
                    fmt.Printf("\t%d\tguaranteed after split check in %s\n", ch.limit, name);
                }

            }
            else
            {
                stkprint(_addr_ctxt, _addr_ch.up, ch.limit + callsize(_addr_ctxt));
                if (!haslinkregister(_addr_ctxt))
                {
                    fmt.Printf("\t%d\ton entry to %s\n", ch.limit, name);
                }

            }

            if (ch.limit != limit)
            {
                fmt.Printf("\t%d\tafter %s uses %d\n", limit, name, ch.limit - limit);
            }

        }

        private static void usage()
        {
            fmt.Fprintf(os.Stderr, "usage: link [options] main.o\n");
            objabi.Flagprint(os.Stderr);
            Exit(2L);
        }

        public partial struct SymbolType // : sbyte
        {
        }

 
        // see also https://9p.io/magic/man2html/1/nm
        public static readonly SymbolType TextSym = (SymbolType)'T';
        public static readonly SymbolType DataSym = (SymbolType)'D';
        public static readonly SymbolType BSSSym = (SymbolType)'B';
        public static readonly SymbolType UndefinedSym = (SymbolType)'U';
        public static readonly SymbolType TLSSym = (SymbolType)'t';
        public static readonly SymbolType FrameSym = (SymbolType)'m';
        public static readonly SymbolType ParamSym = (SymbolType)'p';
        public static readonly SymbolType AutoSym = (SymbolType)'a'; 

        // Deleted auto (not a real sym, just placeholder for type)
        public static readonly char DeletedAutoSym = (char)'x';


        private static void genasmsym(ptr<Link> _addr_ctxt, Action<ptr<Link>, ptr<sym.Symbol>, @string, SymbolType, long, ptr<sym.Symbol>> put)
        {
            ref Link ctxt = ref _addr_ctxt.val;
 
            // These symbols won't show up in the first loop below because we
            // skip sym.STEXT symbols. Normal sym.STEXT symbols are emitted by walking textp.
            var s = ctxt.Syms.Lookup("runtime.text", 0L);
            if (s.Type == sym.STEXT)
            { 
                // We've already included this symbol in ctxt.Textp
                // if ctxt.DynlinkingGo() && ctxt.HeadType == objabi.Hdarwin or
                // on AIX with external linker.
                // See data.go:/textaddress
                if (!(ctxt.DynlinkingGo() && ctxt.HeadType == objabi.Hdarwin) && !(ctxt.HeadType == objabi.Haix && ctxt.LinkMode == LinkExternal))
                {
                    put(ctxt, s, s.Name, TextSym, s.Value, null);
                }

            }

            long n = 0L; 

            // Generate base addresses for all text sections if there are multiple
            foreach (var (_, sect) in Segtext.Sections)
            {
                if (n == 0L)
                {
                    n++;
                    continue;
                }

                if (sect.Name != ".text" || (ctxt.HeadType == objabi.Haix && ctxt.LinkMode == LinkExternal))
                { 
                    // On AIX, runtime.text.X are symbols already in the symtab.
                    break;

                }

                s = ctxt.Syms.ROLookup(fmt.Sprintf("runtime.text.%d", n), 0L);
                if (s == null)
                {
                    break;
                }

                if (s.Type == sym.STEXT)
                {
                    put(ctxt, s, s.Name, TextSym, s.Value, null);
                }

                n++;

            }
            s = ctxt.Syms.Lookup("runtime.etext", 0L);
            if (s.Type == sym.STEXT)
            { 
                // We've already included this symbol in ctxt.Textp
                // if ctxt.DynlinkingGo() && ctxt.HeadType == objabi.Hdarwin or
                // on AIX with external linker.
                // See data.go:/textaddress
                if (!(ctxt.DynlinkingGo() && ctxt.HeadType == objabi.Hdarwin) && !(ctxt.HeadType == objabi.Haix && ctxt.LinkMode == LinkExternal))
                {
                    put(ctxt, s, s.Name, TextSym, s.Value, null);
                }

            }

            Func<ptr<sym.Symbol>, bool> shouldBeInSymbolTable = s =>
            {
                if (s.Attr.NotInSymbolTable())
                {
                    return false;
                }

                if (ctxt.HeadType == objabi.Haix && s.Name == ".go.buildinfo")
                { 
                    // On AIX, .go.buildinfo must be in the symbol table as
                    // it has relocations.
                    return true;

                }

                if ((s.Name == "" || s.Name[0L] == '.') && !s.IsFileLocal() && s.Name != ".rathole" && s.Name != ".TOC.")
                {
                    return false;
                }

                return true;

            }
;

            {
                var s__prev1 = s;

                foreach (var (_, __s) in ctxt.Syms.Allsym)
                {
                    s = __s;
                    if (!shouldBeInSymbolTable(s))
                    {
                        continue;
                    }


                    if (s.Type == sym.SCONST || s.Type == sym.SRODATA || s.Type == sym.SSYMTAB || s.Type == sym.SPCLNTAB || s.Type == sym.SINITARR || s.Type == sym.SDATA || s.Type == sym.SNOPTRDATA || s.Type == sym.SELFROSECT || s.Type == sym.SMACHOGOT || s.Type == sym.STYPE || s.Type == sym.SSTRING || s.Type == sym.SGOSTRING || s.Type == sym.SGOFUNC || s.Type == sym.SGCBITS || s.Type == sym.STYPERELRO || s.Type == sym.SSTRINGRELRO || s.Type == sym.SGOSTRINGRELRO || s.Type == sym.SGOFUNCRELRO || s.Type == sym.SGCBITSRELRO || s.Type == sym.SRODATARELRO || s.Type == sym.STYPELINK || s.Type == sym.SITABLINK || s.Type == sym.SWINDOWS) 
                        if (!s.Attr.Reachable())
                        {
                            continue;
                        }

                        put(ctxt, s, s.Name, DataSym, Symaddr(_addr_s), s.Gotype);
                    else if (s.Type == sym.SBSS || s.Type == sym.SNOPTRBSS || s.Type == sym.SLIBFUZZER_EXTRA_COUNTER) 
                        if (!s.Attr.Reachable())
                        {
                            continue;
                        }

                        if (len(s.P) > 0L)
                        {
                            Errorf(s, "should not be bss (size=%d type=%v special=%v)", len(s.P), s.Type, s.Attr.Special());
                        }

                        put(ctxt, s, s.Name, BSSSym, Symaddr(_addr_s), s.Gotype);
                    else if (s.Type == sym.SUNDEFEXT) 
                        if (ctxt.HeadType == objabi.Hwindows || ctxt.HeadType == objabi.Haix || ctxt.IsELF)
                        {
                            put(ctxt, s, s.Name, UndefinedSym, s.Value, null);
                        }

                    else if (s.Type == sym.SHOSTOBJ) 
                        if (!s.Attr.Reachable())
                        {
                            continue;
                        }

                        if (ctxt.HeadType == objabi.Hwindows || ctxt.IsELF)
                        {
                            put(ctxt, s, s.Name, UndefinedSym, s.Value, null);
                        }

                    else if (s.Type == sym.SDYNIMPORT) 
                        if (!s.Attr.Reachable())
                        {
                            continue;
                        }

                        put(ctxt, s, s.Extname(), UndefinedSym, 0L, null);
                    else if (s.Type == sym.STLSBSS) 
                        if (ctxt.LinkMode == LinkExternal)
                        {
                            put(ctxt, s, s.Name, TLSSym, Symaddr(_addr_s), s.Gotype);
                        }

                                    }

                s = s__prev1;
            }

            {
                var s__prev1 = s;

                foreach (var (_, __s) in ctxt.Textp)
                {
                    s = __s;
                    put(ctxt, s, s.Name, TextSym, s.Value, s.Gotype);

                    var locals = int32(0L);
                    if (s.FuncInfo != null)
                    {
                        locals = s.FuncInfo.Locals;
                    } 
                    // NOTE(ality): acid can't produce a stack trace without .frame symbols
                    put(ctxt, null, ".frame", FrameSym, int64(locals) + int64(ctxt.Arch.PtrSize), null);

                    if (s.FuncInfo == null)
                    {
                        continue;
                    }

                }

                s = s__prev1;
            }

            if (ctxt.Debugvlog != 0L || flagN.val)
            {
                ctxt.Logf("symsize = %d\n", uint32(Symsize));
            }

        }

        public static long Symaddr(ptr<sym.Symbol> _addr_s)
        {
            ref sym.Symbol s = ref _addr_s.val;

            if (!s.Attr.Reachable())
            {
                Errorf(s, "unreachable symbol in symaddr");
            }

            return s.Value;

        }

        private static void xdefine(this ptr<Link> _addr_ctxt, @string p, sym.SymKind t, long v)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            var s = ctxt.Syms.Lookup(p, 0L);
            s.Type = t;
            s.Value = v;
            s.Attr |= sym.AttrReachable;
            s.Attr |= sym.AttrSpecial;
            s.Attr |= sym.AttrLocal;
        }

        private static long datoff(ptr<sym.Symbol> _addr_s, long addr)
        {
            ref sym.Symbol s = ref _addr_s.val;

            if (uint64(addr) >= Segdata.Vaddr)
            {
                return int64(uint64(addr) - Segdata.Vaddr + Segdata.Fileoff);
            }

            if (uint64(addr) >= Segtext.Vaddr)
            {
                return int64(uint64(addr) - Segtext.Vaddr + Segtext.Fileoff);
            }

            Errorf(s, "invalid datoff %#x", addr);
            return 0L;

        }

        public static long Entryvalue(ptr<Link> _addr_ctxt)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            var a = flagEntrySymbol.val;
            if (a[0L] >= '0' && a[0L] <= '9')
            {
                return atolwhex(a);
            }

            var s = ctxt.Syms.Lookup(a, 0L);
            if (s.Type == 0L)
            {
                return FlagTextAddr.val;
            }

            if (ctxt.HeadType != objabi.Haix && s.Type != sym.STEXT)
            {
                Errorf(s, "entry not text");
            }

            return s.Value;

        }

        private static void undefsym(ptr<Link> _addr_ctxt, ptr<sym.Symbol> _addr_s)
        {
            ref Link ctxt = ref _addr_ctxt.val;
            ref sym.Symbol s = ref _addr_s.val;

            ptr<sym.Reloc> r;

            for (long i = 0L; i < len(s.R); i++)
            {
                r = _addr_s.R[i];
                if (r.Sym == null)
                { // happens for some external ARM relocs
                    continue;

                } 
                // TODO(mwhudson): the test of VisibilityHidden here probably doesn't make
                // sense and should be removed when someone has thought about it properly.
                if ((r.Sym.Type == sym.Sxxx || r.Sym.Type == sym.SXREF) && !r.Sym.Attr.VisibilityHidden())
                {
                    Errorf(s, "undefined: %q", r.Sym.Name);
                }

                if (!r.Sym.Attr.Reachable() && r.Type != objabi.R_WEAKADDROFF)
                {
                    Errorf(s, "relocation target %q", r.Sym.Name);
                }

            }


        }

        private static void undef(this ptr<Link> _addr_ctxt)
        {
            ref Link ctxt = ref _addr_ctxt.val;
 
            // undefsym performs checks (almost) identical to checks
            // that report undefined relocations in relocsym.
            // Both undefsym and relocsym can report same symbol as undefined,
            // which results in error message duplication (see #10978).
            //
            // The undef is run after Arch.Asmb and could detect some
            // programming errors there, but if object being linked is already
            // failed with errors, it is better to avoid duplicated errors.
            if (nerrors > 0L)
            {
                return ;
            }

            {
                var s__prev1 = s;

                foreach (var (_, __s) in ctxt.Textp)
                {
                    s = __s;
                    undefsym(_addr_ctxt, _addr_s);
                }

                s = s__prev1;
            }

            {
                var s__prev1 = s;

                foreach (var (_, __s) in datap)
                {
                    s = __s;
                    undefsym(_addr_ctxt, _addr_s);
                }

                s = s__prev1;
            }

            if (nerrors > 0L)
            {
                errorexit();
            }

        }

        private static void callgraph(this ptr<Link> _addr_ctxt)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            if (!FlagC.val)
            {
                return ;
            }

            long i = default;
            ptr<sym.Reloc> r;
            foreach (var (_, s) in ctxt.Textp)
            {
                for (i = 0L; i < len(s.R); i++)
                {
                    r = _addr_s.R[i];
                    if (r.Sym == null)
                    {
                        continue;
                    }

                    if (r.Type.IsDirectCall() && r.Sym.Type == sym.STEXT)
                    {
                        ctxt.Logf("%s calls %s\n", s.Name, r.Sym.Name);
                    }

                }


            }

        }

        public static long Rnd(long v, long r)
        {
            if (r <= 0L)
            {
                return v;
            }

            v += r - 1L;
            var c = v % r;
            if (c < 0L)
            {
                c += r;
            }

            v -= c;
            return v;

        }

        private static long bgetc(ptr<bio.Reader> _addr_r)
        {
            ref bio.Reader r = ref _addr_r.val;

            var (c, err) = r.ReadByte();
            if (err != null)
            {
                if (err != io.EOF)
                {
                    log.Fatalf("reading input: %v", err);
                }

                return -1L;

            }

            return int(c);

        }

        private partial struct markKind // : byte
        {
        } // for postorder traversal
        private static readonly markKind _ = (markKind)iota;
        private static readonly var visiting = (var)0;
        private static readonly var visited = (var)1;


        private static slice<ptr<sym.Library>> postorder(slice<ptr<sym.Library>> libs)
        {
            ref var order = ref heap(make_slice<ptr<sym.Library>>(0L, len(libs)), out ptr<var> _addr_order); // hold the result
            var mark = make_map<ptr<sym.Library>, markKind>(len(libs));
            foreach (var (_, lib) in libs)
            {
                dfs(_addr_lib, mark, _addr_order);
            }
            return order;

        }

        private static void dfs(ptr<sym.Library> _addr_lib, map<ptr<sym.Library>, markKind> mark, ptr<slice<ptr<sym.Library>>> _addr_order) => func((_, panic, __) =>
        {
            ref sym.Library lib = ref _addr_lib.val;
            ref slice<ptr<sym.Library>> order = ref _addr_order.val;

            if (mark[lib] == visited)
            {
                return ;
            }

            if (mark[lib] == visiting)
            {
                panic("found import cycle while visiting " + lib.Pkg);
            }

            mark[lib] = visiting;
            foreach (var (_, i) in lib.Imports)
            {
                dfs(_addr_i, mark, _addr_order);
            }
            mark[lib] = visited;
            order.val = append(order.val, lib);

        });

        private static void loadlibfull(this ptr<Link> _addr_ctxt) => func((_, panic, __) =>
        {
            ref Link ctxt = ref _addr_ctxt.val;
 
            // Load full symbol contents, resolve indexed references.
            ctxt.loader.LoadFull(ctxt.Arch, ctxt.Syms); 

            // Pull the symbols out.
            ctxt.loader.ExtractSymbols(ctxt.Syms); 

            // Load cgo directives.
            foreach (var (_, d) in ctxt.cgodata)
            {
                setCgoAttr(ctxt, ctxt.Syms.Lookup, d.file, d.pkg, d.directives);
            }
            setupdynexp(_addr_ctxt); 

            // Populate ctxt.Reachparent if appropriate.
            if (ctxt.Reachparent != null)
            {
                for (long i = 0L; i < len(ctxt.loader.Reachparent); i++)
                {
                    var p = ctxt.loader.Reachparent[i];
                    if (p == 0L)
                    {
                        continue;
                    }

                    if (p == loader.Sym(i))
                    {
                        panic("self-cycle in reachparent");
                    }

                    var sym = ctxt.loader.Syms[i];
                    var psym = ctxt.loader.Syms[p];
                    ctxt.Reachparent[sym] = psym;

                }


            } 

            // Drop the reference.
            ctxt.loader = null;
            ctxt.cgodata = null;

            addToTextp(ctxt);

        });

        private static void dumpsyms(this ptr<Link> _addr_ctxt)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            foreach (var (_, s) in ctxt.Syms.Allsym)
            {
                fmt.Printf("%s %s %p %v %v\n", s, s.Type, s, s.Attr.Reachable(), s.Attr.OnList());
                foreach (var (i) in s.R)
                {
                    fmt.Println("\t", s.R[i].Type, s.R[i].Sym);
                }

            }

        }
    }
}}}}
