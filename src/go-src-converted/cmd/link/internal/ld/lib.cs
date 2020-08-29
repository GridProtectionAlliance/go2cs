// Inferno utils/8l/asm.c
// https://bitbucket.org/inferno-os/inferno-os/src/default/utils/8l/asm.c
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

// package ld -- go2cs converted at 2020 August 29 10:03:54 UTC
// import "cmd/link/internal/ld" ==> using ld = go.cmd.link.@internal.ld_package
// Original source: C:\Go\src\cmd\link\internal\ld\lib.go
using bufio = go.bufio_package;
using bytes = go.bytes_package;
using bio = go.cmd.@internal.bio_package;
using objabi = go.cmd.@internal.objabi_package;
using sys = go.cmd.@internal.sys_package;
using loadelf = go.cmd.link.@internal.loadelf_package;
using loadmacho = go.cmd.link.@internal.loadmacho_package;
using loadpe = go.cmd.link.@internal.loadpe_package;
using objfile = go.cmd.link.@internal.objfile_package;
using sym = go.cmd.link.@internal.sym_package;
using sha1 = go.crypto.sha1_package;
using elf = go.debug.elf_package;
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
using strings = go.strings_package;
using sync = go.sync_package;
using static go.builtin;
using System;
using System.Threading;

namespace go {
namespace cmd {
namespace link {
namespace @internal
{
    public static partial class ld_package
    {
        // Data layout and relocation.

        // Derived from Inferno utils/6l/l.h
        // https://bitbucket.org/inferno-os/inferno-os/src/default/utils/6l/l.h
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
            public @string Linuxdynld;
            public @string Freebsddynld;
            public @string Netbsddynld;
            public @string Openbsddynld;
            public @string Dragonflydynld;
            public @string Solarisdynld;
            public Func<ref Link, ref sym.Symbol, ref sym.Reloc, bool> Adddynrel;
            public Action<ref Link> Archinit;
            public Func<ref Link, ref sym.Reloc, ref sym.Symbol, ref long, bool> Archreloc;
            public Func<ref Link, ref sym.Reloc, ref sym.Symbol, long, long> Archrelocvariant;
            public Action<ref Link, ref sym.Reloc, ref sym.Symbol> Trampoline;
            public Action<ref Link> Asmb;
            public Func<ref Link, ref sym.Reloc, long, bool> Elfreloc1;
            public Action<ref Link> Elfsetupplt;
            public Action<ref Link> Gentext;
            public Func<ref sys.Arch, ref OutBuf, ref sym.Symbol, ref sym.Reloc, long, bool> Machoreloc1;
            public Func<ref sys.Arch, ref OutBuf, ref sym.Symbol, ref sym.Reloc, long, bool> PEreloc1; // TLSIEtoLE converts a TLS Initial Executable relocation to
// a TLS Local Executable relocation.
//
// This is possible when a TLS IE relocation refers to a local
// symbol in an executable, which is typical when internally
// linking PIE binaries.
            public Action<ref sym.Symbol, long, long> TLSIEtoLE;
        }

        public static Arch Thearch = default;        public static int Lcsize = default;        private static Rpath rpath = default;        public static int Spsize = default;        public static int Symsize = default;

        public static readonly long MINFUNC = 16L; // minimum size for a function

        // DynlinkingGo returns whether we are producing Go code that can live
        // in separate shared libraries linked together at runtime.
        private static bool DynlinkingGo(this ref Link _ctxt) => func(_ctxt, (ref Link ctxt, Defer _, Panic panic, Recover __) =>
        {
            if (!ctxt.Loaded)
            {
                panic("DynlinkingGo called before all symbols loaded");
            }
            return ctxt.BuildMode == BuildModeShared || ctxt.linkShared || ctxt.BuildMode == BuildModePlugin || ctxt.CanUsePlugins();
        });

        // CanUsePlugins returns whether a plugins can be used
        private static bool CanUsePlugins(this ref Link ctxt)
        {
            return ctxt.Syms.ROLookup("plugin.Open", 0L) != null;
        }

        // UseRelro returns whether to make use of "read only relocations" aka
        // relro.
        private static bool UseRelro(this ref Link ctxt)
        {

            if (ctxt.BuildMode == BuildModeCArchive || ctxt.BuildMode == BuildModeCShared || ctxt.BuildMode == BuildModeShared || ctxt.BuildMode == BuildModePIE || ctxt.BuildMode == BuildModePlugin) 
                return ctxt.IsELF;
            else 
                return ctxt.linkShared;
                    }

        private static slice<ref sym.Symbol> dynexp = default;        private static slice<@string> dynlib = default;        private static slice<@string> ldflag = default;        private static long havedynamic = default;        public static long Funcalign = default;        private static bool iscgo = default;        private static long elfglobalsymndx = default;        private static @string interpreter = default;        private static bool debug_s = default;        public static int HEADR = default;        private static long nerrors = default;        private static long liveness = default;

        public static sym.Segment Segtext = default;        public static sym.Segment Segrodata = default;        public static sym.Segment Segrelrodata = default;        public static sym.Segment Segdata = default;        public static sym.Segment Segdwarf = default;

        /* whence for ldpkg */
        public static readonly long FileObj = 0L + iota;
        public static readonly var ArchiveObj = 0;
        public static readonly var Pkgdef = 1;

        private static readonly @string pkgdef = "__.PKGDEF";



 
        // Set if we see an object compiled by the host compiler that is not
        // from a package that is known to support internal linking mode.
        private static var externalobj = false;        private static @string theline = default;

        public static void Lflag(ref Link ctxt, @string arg)
        {
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
                var (fi, err) = os.Lstat(flagOutfile.Value);

                if (err == null && !fi.Mode().IsRegular())
                {
                    return;
                }

            }
            os.Remove(flagOutfile.Value);
        }

        private static void libinit(ref Link ctxt)
        {
            Funcalign = Thearch.Funcalign; 

            // add goroot to the end of the libdir list.
            @string suffix = "";

            @string suffixsep = "";
            if (flagInstallSuffix != "".Value)
            {
                suffixsep = "_";
                suffix = flagInstallSuffix.Value;
            }
            else if (flagRace.Value)
            {
                suffixsep = "_";
                suffix = "race";
            }
            else if (flagMsan.Value)
            {
                suffixsep = "_";
                suffix = "msan";
            }
            Lflag(ctxt, filepath.Join(objabi.GOROOT, "pkg", fmt.Sprintf("%s_%s%s%s", objabi.GOOS, objabi.GOARCH, suffixsep, suffix)));

            mayberemoveoutfile();
            var (f, err) = os.OpenFile(flagOutfile.Value, os.O_WRONLY | os.O_CREATE | os.O_TRUNC, 0775L);
            if (err != null)
            {
                Exitf("cannot create %s: %v", flagOutfile.Value, err);
            }
            ctxt.Out.w = bufio.NewWriter(f);
            ctxt.Out.f = f;

            if (flagEntrySymbol == "".Value)
            {

                if (ctxt.BuildMode == BuildModeCShared || ctxt.BuildMode == BuildModeCArchive) 
                    flagEntrySymbol.Value = fmt.Sprintf("_rt0_%s_%s_lib", objabi.GOARCH, objabi.GOOS);
                else if (ctxt.BuildMode == BuildModeExe || ctxt.BuildMode == BuildModePIE) 
                    flagEntrySymbol.Value = fmt.Sprintf("_rt0_%s_%s", objabi.GOARCH, objabi.GOOS);
                else if (ctxt.BuildMode == BuildModeShared || ctxt.BuildMode == BuildModePlugin)                 else 
                    Errorf(null, "unknown *flagEntrySymbol for buildmode %v", ctxt.BuildMode);
                            }
        }

        private static void errorexit()
        {
            if (nerrors != 0L)
            {
                Exit(2L);
            }
            Exit(0L);
        }

        private static ref sym.Library loadinternal(ref Link ctxt, @string name)
        {
            if (ctxt.linkShared && ctxt.PackageShlib != null)
            {
                {
                    var shlib = ctxt.PackageShlib[name];

                    if (shlib != "")
                    {
                        return addlibpath(ctxt, "internal", "internal", "", name, shlib);
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
                        return addlibpath(ctxt, "internal", "internal", pname, name, "");
                    }

                    pname = pname__prev2;

                }
                ctxt.Logf("loadinternal: cannot find %s\n", name);
                return null;
            }
            for (long i = 0L; i < len(ctxt.Libdir); i++)
            {
                if (ctxt.linkShared)
                {
                    var shlibname = filepath.Join(ctxt.Libdir[i], name + ".shlibname");
                    if (ctxt.Debugvlog != 0L)
                    {
                        ctxt.Logf("searching for %s.a in %s\n", name, shlibname);
                    }
                    {
                        var (_, err) = os.Stat(shlibname);

                        if (err == null)
                        {
                            return addlibpath(ctxt, "internal", "internal", "", name, shlibname);
                        }

                    }
                }
                pname = filepath.Join(ctxt.Libdir[i], name + ".a");
                if (ctxt.Debugvlog != 0L)
                {
                    ctxt.Logf("searching for %s.a in %s\n", name, pname);
                }
                {
                    (_, err) = os.Stat(pname);

                    if (err == null)
                    {
                        return addlibpath(ctxt, "internal", "internal", pname, name, "");
                    }

                }
            }


            ctxt.Logf("warning: unable to find %s.a\n", name);
            return null;
        }

        // findLibPathCmd uses cmd command to find gcc library libname.
        // It returns library full path if found, or "none" if not found.
        private static @string findLibPathCmd(this ref Link ctxt, @string cmd, @string libname)
        {
            if (flagExtld == "".Value)
            {
                flagExtld.Value = "gcc";
            }
            var args = hostlinkArchArgs(ctxt.Arch);
            args = append(args, cmd);
            if (ctxt.Debugvlog != 0L)
            {
                ctxt.Logf("%s %v\n", flagExtld.Value, args);
            }
            var (out, err) = exec.Command(flagExtld.Value, args).Output();
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
        private static @string findLibPath(this ref Link ctxt, @string libname)
        {
            return ctxt.findLibPathCmd("--print-file-name=" + libname, libname);
        }

        private static void loadlib(this ref Link ctxt)
        {

            if (ctxt.BuildMode == BuildModeCShared || ctxt.BuildMode == BuildModePlugin) 
                var s = ctxt.Syms.Lookup("runtime.islibrary", 0L);
                s.Attr |= sym.AttrDuplicateOK;
                s.AddUint8(1L);
            else if (ctxt.BuildMode == BuildModeCArchive) 
                s = ctxt.Syms.Lookup("runtime.isarchive", 0L);
                s.Attr |= sym.AttrDuplicateOK;
                s.AddUint8(1L);
                        loadinternal(ctxt, "runtime");
            if (ctxt.Arch.Family == sys.ARM)
            {
                loadinternal(ctxt, "math");
            }
            if (flagRace.Value)
            {
                loadinternal(ctxt, "runtime/race");
            }
            if (flagMsan.Value)
            {
                loadinternal(ctxt, "runtime/msan");
            } 

            // ctxt.Library grows during the loop, so not a range loop.
            {
                long i__prev1 = i;

                for (long i = 0L; i < len(ctxt.Library); i++)
                {
                    var lib = ctxt.Library[i];
                    if (lib.Shlib == "")
                    {
                        if (ctxt.Debugvlog > 1L)
                        {
                            ctxt.Logf("%5.2f autolib: %s (from %s)\n", Cputime(), lib.File, lib.Objref);
                        }
                        loadobjfile(ctxt, lib);
                    }
                }


                i = i__prev1;
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
                            ctxt.Logf("%5.2f autolib: %s (from %s)\n", Cputime(), lib.Shlib, lib.Objref);
                        }
                        ldshlibsyms(ctxt, lib.Shlib);
                    }
                }

                lib = lib__prev1;
            }

            iscgo = ctxt.Syms.ROLookup("x_cgo_init", 0L) != null; 

            // We now have enough information to determine the link mode.
            determineLinkMode(ctxt); 

            // Recalculate pe parameters now that we have ctxt.LinkMode set.
            if (ctxt.HeadType == objabi.Hwindows)
            {
                Peinit(ctxt);
            }
            if (ctxt.HeadType == objabi.Hdarwin && ctxt.LinkMode == LinkExternal)
            {
                FlagTextAddr.Value = 0L;
            }
            if (ctxt.LinkMode == LinkExternal && ctxt.Arch.Family == sys.PPC64)
            {
                var toc = ctxt.Syms.Lookup(".TOC.", 0L);
                toc.Type = sym.SDYNIMPORT;
            }
            if (ctxt.LinkMode == LinkExternal && !iscgo && ctxt.LibraryByPkg["runtime/cgo"] == null)
            { 
                // This indicates a user requested -linkmode=external.
                // The startup code uses an import of runtime/cgo to decide
                // whether to initialize the TLS.  So give it one. This could
                // be handled differently but it's an unusual case.
                {
                    var lib__prev2 = lib;

                    lib = loadinternal(ctxt, "runtime/cgo");

                    if (lib != null)
                    {
                        if (lib.Shlib != "")
                        {
                            ldshlibsyms(ctxt, lib.Shlib);
                        }
                        else
                        {
                            if (ctxt.BuildMode == BuildModeShared || ctxt.linkShared)
                            {
                                Exitf("cannot implicitly include runtime/cgo in a shared library");
                            }
                            loadobjfile(ctxt, lib);
                        }
                    }

                    lib = lib__prev2;

                }
            }
            if (ctxt.LinkMode == LinkInternal)
            { 
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
                            if (s.Extname != "" && s.Dynimplib != "" && !s.Attr.CgoExport())
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

            ref sym.Symbol moduledata = default;
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
                    s.Type = sym.SRODATA;
                    s.Size = 0L;
                    s.AddUint8(uint8(objabi.GOARM));
                }
                if (objabi.Framepointer_enabled(objabi.GOOS, objabi.GOARCH))
                {
                    s = ctxt.Syms.Lookup("runtime.framepointer_enabled", 0L);
                    s.Type = sym.SRODATA;
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

            // Now that we know the link mode, trim the dynexp list.
            var x = sym.AttrCgoExportDynamic;

            if (ctxt.LinkMode == LinkExternal)
            {
                x = sym.AttrCgoExportStatic;
            }
            long w = 0L;
            {
                long i__prev1 = i;

                for (i = 0L; i < len(dynexp); i++)
                {
                    if (dynexp[i].Attr & x != 0L)
                    {
                        dynexp[w] = dynexp[i];
                        w++;
                    }
                }


                i = i__prev1;
            }
            dynexp = dynexp[..w]; 

            // In internal link mode, read the host object files.
            if (ctxt.LinkMode == LinkInternal)
            {
                hostobjs(ctxt); 

                // If we have any undefined symbols in external
                // objects, try to read them from the libgcc file.
                var any = false;
                {
                    var s__prev1 = s;

                    foreach (var (_, __s) in ctxt.Syms.Allsym)
                    {
                        s = __s;
                        foreach (var (_, r) in s.R)
                        {
                            if (r.Sym != null && r.Sym.Type == sym.SXREF && r.Sym.Name != ".got")
                            {
                                any = true;
                                break;
                            }
                        }
            else
                    }

                    s = s__prev1;
                }

                if (any)
                {
                    if (flagLibGCC == "".Value)
                    {
                        flagLibGCC.Value = ctxt.findLibPathCmd("--print-libgcc-file-name", "libgcc");
                    }
                    if (flagLibGCC != "none".Value)
                    {
                        hostArchive(ctxt, flagLibGCC.Value);
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
            }            {
                hostlinksetup(ctxt);
            } 

            // We've loaded all the code now.
            ctxt.Loaded = true; 

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
                    FlagD.Value = true;
                }
            } 

            // If type. symbols are visible in the symbol table, rename them
            // using a SHA-1 prefix. This reduces binary size (the full
            // string of a type symbol can be multiple kilobytes) and removes
            // characters that upset external linkers.
            //
            // Keep the type.. prefix, which parts of the linker (like the
            // DWARF generator) know means the symbol is not decodable.
            //
            // Leave type.runtime. symbols alone, because other parts of
            // the linker manipulates them, and also symbols whose names
            // would not be shortened by this process.
            if (typeSymbolMangling(ctxt))
            {
                FlagW.Value = true; // disable DWARF generation
                {
                    var s__prev1 = s;

                    foreach (var (_, __s) in ctxt.Syms.Allsym)
                    {
                        s = __s;
                        var newName = typeSymbolMangle(s.Name);
                        if (newName != s.Name)
                        {
                            ctxt.Syms.Rename(s.Name, newName, int(s.Version));
                        }
                    }

                    s = s__prev1;
                }

            } 

            // If package versioning is required, generate a hash of the
            // the packages used in the link.
            if (ctxt.BuildMode == BuildModeShared || ctxt.BuildMode == BuildModePlugin || ctxt.CanUsePlugins())
            {
                {
                    var lib__prev1 = lib;

                    foreach (var (_, __lib) in ctxt.Library)
                    {
                        lib = __lib;
                        if (lib.Shlib == "")
                        {
                            genhash(ctxt, lib);
                        }
                    }

                    lib = lib__prev1;
                }

            }
            if (ctxt.Arch == sys.Arch386)
            {
                if ((ctxt.BuildMode == BuildModeCArchive && ctxt.IsELF) || (ctxt.BuildMode == BuildModeCShared && ctxt.HeadType != objabi.Hwindows) || ctxt.BuildMode == BuildModePIE || ctxt.DynlinkingGo())
                {
                    var got = ctxt.Syms.Lookup("_GLOBAL_OFFSET_TABLE_", 0L);
                    got.Type = sym.SDYNIMPORT;
                    got.Attr |= sym.AttrReachable;
                }
            }
            importcycles(); 

            // put symbols into Textp
            // do it in postorder so that packages are laid down in dependency order
            // internal first, then everything else
            ctxt.Library = postorder(ctxt.Library);
            foreach (var (_, doInternal) in new array<bool>(new bool[] { true, false }))
            {
                {
                    var lib__prev2 = lib;

                    foreach (var (_, __lib) in ctxt.Library)
                    {
                        lib = __lib;
                        if (isRuntimeDepPkg(lib.Pkg) != doInternal)
                        {
                            continue;
                        }
                        ctxt.Textp = append(ctxt.Textp, lib.Textp);
                        {
                            var s__prev3 = s;

                            foreach (var (_, __s) in lib.DupTextSyms)
                            {
                                s = __s;
                                if (!s.Attr.OnList())
                                {
                                    ctxt.Textp = append(ctxt.Textp, s);
                                    s.Attr |= sym.AttrOnList; 
                                    // dupok symbols may be defined in multiple packages. its
                                    // associated package is chosen sort of arbitrarily (the
                                    // first containing package that the linker loads). canonicalize
                                    // it here to the package with which it will be laid down
                                    // in text.
                                    s.File = objabi.PathToPrefix(lib.Pkg);
                                }
                            }

                            s = s__prev3;
                        }

                    }

                    lib = lib__prev2;
                }

            }
            if (len(ctxt.Shlibs) > 0L)
            { 
                // We might have overwritten some functions above (this tends to happen for the
                // autogenerated type equality/hashing functions) and we don't want to generated
                // pcln table entries for these any more so remove them from Textp.
                var textp = make_slice<ref sym.Symbol>(0L, len(ctxt.Textp));
                {
                    var s__prev1 = s;

                    foreach (var (_, __s) in ctxt.Textp)
                    {
                        s = __s;
                        if (s.Type != sym.SDYNIMPORT)
                        {
                            textp = append(textp, s);
                        }
                    }

                    s = s__prev1;
                }

                ctxt.Textp = textp;
            }
        }

        // typeSymbolMangling reports whether the linker should shorten the
        // names of symbols that represent Go types.
        //
        // As the names of these symbols are derived from the string of
        // the type, they can run to many kilobytes long. So we shorten
        // them using a SHA-1 when the name appears in the final binary.
        //
        // These are the symbols that begin with the prefix 'type.' and
        // contain run-time type information used by the runtime and reflect
        // packages. All Go binaries contain these symbols, but only only
        // those programs loaded dynamically in multiple parts need these
        // symbols to have entries in the symbol table.
        private static bool typeSymbolMangling(ref Link ctxt)
        {
            return ctxt.BuildMode == BuildModeShared || ctxt.linkShared || ctxt.BuildMode == BuildModePlugin || ctxt.Syms.ROLookup("plugin.Open", 0L) != null;
        }

        // typeSymbolMangle mangles the given symbol name into something shorter.
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
        private static long nextar(ref bio.Reader bp, long off, ref ArHdr a)
        {
            if (off & 1L != 0L)
            {
                off++;
            }
            bp.Seek(off, 0L);
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

        private static void genhash(ref Link _ctxt, ref sym.Library _lib) => func(_ctxt, _lib, (ref Link ctxt, ref sym.Library lib, Defer defer, Panic _, Recover __) =>
        {
            var (f, err) = bio.Open(lib.File);
            if (err != null)
            {
                Errorf(null, "cannot open file %s for hash generation: %v", lib.File, err);
                return;
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
            ArHdr arhdr = default;
            var l = nextar(f, f.Offset(), ref arhdr);
            if (l <= 0L)
            {
                Errorf(null, "%s: short read on archive file symbol header", lib.File);
                return;
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
                return;
            }
            var firstEOL = bytes.IndexByte(pkgDefBytes, '\n');
            if (firstEOL < 0L)
            {
                Errorf(null, "cannot parse package data of %s for hash generation, no newline found", lib.File);
                return;
            }
            var firstDoubleDollar = bytes.Index(pkgDefBytes, (slice<byte>)"\n$$");
            if (firstDoubleDollar < 0L)
            {
                Errorf(null, "cannot parse package data of %s for hash generation, no \\n$$ found", lib.File);
                return;
            }
            var secondDoubleDollar = bytes.Index(pkgDefBytes[firstDoubleDollar + 1L..], (slice<byte>)"\n$$");
            if (secondDoubleDollar < 0L)
            {
                Errorf(null, "cannot parse package data of %s for hash generation, only one \\n$$ found", lib.File);
                return;
            }
            h.Write(pkgDefBytes[0L..firstEOL]);
            h.Write(pkgDefBytes[firstDoubleDollar..firstDoubleDollar + secondDoubleDollar]);
            lib.Hash = hex.EncodeToString(h.Sum(null));
        });

        private static void loadobjfile(ref Link ctxt, ref sym.Library lib)
        {
            var pkg = objabi.PathToPrefix(lib.Pkg);

            if (ctxt.Debugvlog > 1L)
            {
                ctxt.Logf("%5.2f ldobj: %s (%s)\n", Cputime(), lib.File, pkg);
            }
            var (f, err) = bio.Open(lib.File);
            if (err != null)
            {
                Exitf("cannot open file %s: %v", lib.File, err);
            }
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
                var l = f.Seek(0L, 2L);

                f.Seek(0L, 0L);
                ldobj(ctxt, f, lib, l, lib.File, lib.File, FileObj);
                f.Close();

                return;
            } 

            /* process __.PKGDEF */
 

            /* process __.PKGDEF */
            var off = f.Offset();

            ArHdr arhdr = default;
            l = nextar(f, off, ref arhdr);
            @string pname = default;
            if (l <= 0L)
            {
                Errorf(null, "%s: short read on archive file symbol header", lib.File);
                goto @out;
            }
            if (!strings.HasPrefix(arhdr.name, pkgdef))
            {
                Errorf(null, "%s: cannot find package header", lib.File);
                goto @out;
            }
            off += l;

            ldpkg(ctxt, f, pkg, atolwhex(arhdr.size), lib.File, Pkgdef);

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
            while (true)
            {
                l = nextar(f, off, ref arhdr);
                if (l == 0L)
                {
                    break;
                }
                if (l < 0L)
                {
                    Exitf("%s: malformed archive", lib.File);
                }
                off += l;

                pname = fmt.Sprintf("%s(%s)", lib.File, arhdr.name);
                l = atolwhex(arhdr.size);
                ldobj(ctxt, f, lib, l, pname, lib.File, ArchiveObj);
            }


@out:
            f.Close();
        }

        public partial struct Hostobj
        {
            public Action<ref Link, ref bio.Reader, @string, long, @string> ld;
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

        private static ref Hostobj ldhostobj(Action<ref Link, ref bio.Reader, @string, long, @string> ld, objabi.HeadType headType, ref bio.Reader f, @string pkg, long length, @string pn, @string file)
        {
            var isinternal = false;
            for (long i = 0L; i < len(internalpkg); i++)
            {
                if (pkg == internalpkg[i])
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
            var h = ref hostobj[len(hostobj) - 1L];
            h.ld = ld;
            h.pkg = pkg;
            h.pn = pn;
            h.file = file;
            h.off = f.Offset();
            h.length = length;
            return h;
        }

        private static void hostobjs(ref Link ctxt)
        {
            ref Hostobj h = default;

            for (long i = 0L; i < len(hostobj); i++)
            {
                h = ref hostobj[i];
                var (f, err) = bio.Open(h.file);
                if (err != null)
                {
                    Exitf("cannot reopen %s: %v", h.pn, err);
                }
                f.Seek(h.off, 0L);
                h.ld(ctxt, f, h.pkg, h.length, h.pn);
                f.Close();
            }

        }

        // provided by lib9

        private static void rmtemp()
        {
            os.RemoveAll(flagTmpdir.Value);
        }

        private static void hostlinksetup(ref Link ctxt)
        {
            if (ctxt.LinkMode != LinkExternal)
            {
                return;
            } 

            // For external link, record that we need to tell the external linker -s,
            // and turn off -s internally: the external linker needs the symbol
            // information for its final link.
            debug_s = FlagS.Value;
            FlagS.Value = false; 

            // create temporary directory and arrange cleanup
            if (flagTmpdir == "".Value)
            {
                var (dir, err) = ioutil.TempDir("", "go-link-");
                if (err != null)
                {
                    log.Fatal(err);
                }
                flagTmpdir.Value = dir;
                AtExit(rmtemp);
            } 

            // change our output to temporary object file
            ctxt.Out.f.Close();
            mayberemoveoutfile();

            var p = filepath.Join(flagTmpdir.Value, "go.o");
            error err = default;
            var (f, err) = os.OpenFile(p, os.O_WRONLY | os.O_CREATE | os.O_TRUNC, 0775L);
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
            sync.WaitGroup wg = default;
            var sema = make_channel<object>(runtime.NumCPU()); // limit open file descriptors
            {
                var h__prev1 = h;

                foreach (var (__i, __h) in hostobj)
                {
                    i = __i;
                    h = __h;
                    var h = h;
                    var dst = filepath.Join(flagTmpdir.Value, fmt.Sprintf("%06d.o", i));
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
            var path = filepath.Join(flagTmpdir.Value, name);
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
        private static void archive(this ref Link ctxt)
        {
            if (ctxt.BuildMode != BuildModeCArchive)
            {
                return;
            }
            if (flagExtar == "".Value)
            {
                flagExtar.Value = "ar";
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

            @string argv = new slice<@string>(new @string[] { *flagExtar, "-q", "-c", "-s", *flagOutfile });
            argv = append(argv, filepath.Join(flagTmpdir.Value, "go.o"));
            argv = append(argv, hostobjCopy());

            if (ctxt.Debugvlog != 0L)
            {
                ctxt.Logf("archive: %s\n", strings.Join(argv, " "));
            }
            {
                var (out, err) = exec.Command(argv[0L], argv[1L..]).CombinedOutput();

                if (err != null)
                {
                    Exitf("running %s failed: %v\n%s", argv[0L], err, out);
                }

            }
        }

        private static void hostlink(this ref Link ctxt)
        {
            if (ctxt.LinkMode != LinkExternal || nerrors > 0L)
            {
                return;
            }
            if (ctxt.BuildMode == BuildModeCArchive)
            {
                return;
            }
            if (flagExtld == "".Value)
            {
                flagExtld.Value = "gcc";
            }
            slice<@string> argv = default;
            argv = append(argv, flagExtld.Value);
            argv = append(argv, hostlinkArchArgs(ctxt.Arch));

            if (FlagS || debug_s.Value)
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
                argv = append(argv, "-Wl,-headerpad,1144");
                if (ctxt.DynlinkingGo())
                {
                    argv = append(argv, "-Wl,-flat_namespace");
                }
                if (ctxt.BuildMode == BuildModeExe && !ctxt.Arch.InFamily(sys.ARM64))
                {
                    argv = append(argv, "-Wl,-no_pie");
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
            
            if (ctxt.BuildMode == BuildModeExe) 
                if (ctxt.HeadType == objabi.Hdarwin)
                {
                    if (ctxt.Arch.Family == sys.ARM64)
                    { 
                        // __PAGEZERO segment size determined empirically.
                        // XCode 9.0.1 successfully uploads an iOS app with this value.
                        argv = append(argv, "-Wl,-pagezero_size,100000000");
                    }
                    else
                    {
                        argv = append(argv, "-Wl,-pagezero_size,4000000");
                    }
                }
            else if (ctxt.BuildMode == BuildModePIE) 
                // ELF.
                if (ctxt.HeadType != objabi.Hdarwin)
                {
                    if (ctxt.UseRelro())
                    {
                        argv = append(argv, "-Wl,-z,relro");
                    }
                    argv = append(argv, "-pie");
                }
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

                if (ctxt.Arch.InFamily(sys.ARM, sys.ARM64))
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
                    var cmd = exec.Command(flagExtld.Value, "-fuse-ld=gold", "-Wl,--version");
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
            var outopt = flagOutfile.Value;
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
            if (strings.Contains(argv[0L], "clang"))
            {
                argv = append(argv, "-Qunused-arguments");
            }
            argv = append(argv, filepath.Join(flagTmpdir.Value, "go.o"));
            argv = append(argv, hostobjCopy());

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
                        var libpath = findshlib(ctxt, dep);
                        if (libpath != "")
                        {
                            addshlib(libpath);
                        }
                    }
                }
            }
            argv = append(argv, ldflag); 

            // When building a program with the default -buildmode=exe the
            // gc compiler generates code requires DT_TEXTREL in a
            // position independent executable (PIE). On systems where the
            // toolchain creates PIEs by default, and where DT_TEXTREL
            // does not work, the resulting programs will not run. See
            // issue #17847. To avoid this problem pass -no-pie to the
            // toolchain if it is supported.
            if (ctxt.BuildMode == BuildModeExe)
            {
                var src = filepath.Join(flagTmpdir.Value, "trivial.c");
                {
                    var err__prev2 = err;

                    var err = ioutil.WriteFile(src, (slice<byte>)"int main() { return 0; }", 0666L);

                    if (err != null)
                    {
                        Errorf(null, "WriteFile trivial.c failed: %v", err);
                    } 

                    // GCC uses -no-pie, clang uses -nopie.

                    err = err__prev2;

                } 

                // GCC uses -no-pie, clang uses -nopie.
                foreach (var (_, nopie) in new slice<@string>(new @string[] { "-no-pie", "-nopie" }))
                {
                    cmd = exec.Command(argv[0L], nopie, "trivial.c");
                    cmd.Dir = flagTmpdir.Value;
                    cmd.Env = append(new slice<@string>(new @string[] { "LC_ALL=C" }), os.Environ());
                    (out, err) = cmd.CombinedOutput(); 
                    // GCC says "unrecognized command line option ‘-no-pie’"
                    // clang says "unknown argument: '-no-pie'"
                    var supported = err == null && !bytes.Contains(out, (slice<byte>)"unrecognized") && !bytes.Contains(out, (slice<byte>)"unknown");
                    if (supported)
                    {
                        argv = append(argv, nopie);
                        break;
                    }
                }
            }
            {
                var p__prev1 = p;

                foreach (var (_, __p) in strings.Fields(flagExtldflags.Value))
                {
                    p = __p;
                    argv = append(argv, p); 

                    // clang, unlike GCC, passes -rdynamic to the linker
                    // even when linking with -static, causing a linker
                    // error when using GNU ld. So take out -rdynamic if
                    // we added it. We do it in this order, rather than
                    // only adding -rdynamic later, so that -*extldflags
                    // can override -rdynamic without using -static.
                    if (ctxt.IsELF && p == "-static")
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
                ctxt.Logf("%5.2f host link:", Cputime());
                foreach (var (_, v) in argv)
                {
                    ctxt.Logf(" %q", v);
                }
                ctxt.Logf("\n");
            }
            {
                var out__prev1 = out;
                var err__prev1 = err;

                (out, err) = exec.Command(argv[0L], argv[1L..]).CombinedOutput();

                if (err != null)
                {
                    Exitf("running %s failed: %v\n%s", argv[0L], err, out);
                }
                else if (len(out) > 0L)
                { 
                    // always print external output even if the command is successful, so that we don't
                    // swallow linker warnings (see https://golang.org/issue/17935).
                    ctxt.Logf("%s", out);
                }

                out = out__prev1;
                err = err__prev1;

            }

            if (!FlagS && !FlagW && !debug_s && ctxt.HeadType == objabi.Hdarwin.Value.Value)
            { 
                // Skip combining dwarf on arm.
                if (!ctxt.Arch.InFamily(sys.ARM, sys.ARM64))
                {
                    var dsym = filepath.Join(flagTmpdir.Value, "go.dwarf");
                    {
                        var out__prev3 = out;
                        var err__prev3 = err;

                        (out, err) = exec.Command("dsymutil", "-f", flagOutfile.Value, "-o", dsym).CombinedOutput();

                        if (err != null)
                        {
                            Exitf("%s: running dsymutil failed: %v\n%s", os.Args[0L], err, out);
                        } 
                        // Skip combining if `dsymutil` didn't generate a file. See #11994.

                        out = out__prev3;
                        err = err__prev3;

                    } 
                    // Skip combining if `dsymutil` didn't generate a file. See #11994.
                    {
                        var err__prev3 = err;

                        var (_, err) = os.Stat(dsym);

                        if (os.IsNotExist(err))
                        {
                            return;
                        } 
                        // For os.Rename to work reliably, must be in same directory as outfile.

                        err = err__prev3;

                    } 
                    // For os.Rename to work reliably, must be in same directory as outfile.
                    var combinedOutput = flagOutfile + "~".Value;
                    {
                        var err__prev3 = err;

                        err = machoCombineDwarf(flagOutfile.Value, dsym, combinedOutput, ctxt.BuildMode);

                        if (err != null)
                        {
                            Exitf("%s: combining dwarf failed: %v", os.Args[0L], err);
                        }

                        err = err__prev3;

                    }
                    os.Remove(flagOutfile.Value);
                    {
                        var err__prev3 = err;

                        err = os.Rename(combinedOutput, flagOutfile.Value);

                        if (err != null)
                        {
                            Exitf("%s: %v", os.Args[0L], err);
                        }

                        err = err__prev3;

                    }
                }
            }
        }

        // hostlinkArchArgs returns arguments to pass to the external linker
        // based on the architecture.
        private static slice<@string> hostlinkArchArgs(ref sys.Arch arch)
        {

            if (arch.Family == sys.I386) 
                return new slice<@string>(new @string[] { "-m32" });
            else if (arch.Family == sys.AMD64 || arch.Family == sys.PPC64 || arch.Family == sys.S390X) 
                return new slice<@string>(new @string[] { "-m64" });
            else if (arch.Family == sys.ARM) 
                return new slice<@string>(new @string[] { "-marm" });
            else if (arch.Family == sys.ARM64)             else if (arch.Family == sys.MIPS64) 
                return new slice<@string>(new @string[] { "-mabi=64" });
            else if (arch.Family == sys.MIPS) 
                return new slice<@string>(new @string[] { "-mabi=32" });
                        return null;
        }

        // ldobj loads an input object. If it is a host object (an object
        // compiled by a non-Go compiler) it returns the Hostobj pointer. If
        // it is a Go object, it returns nil.
        private static ref Hostobj ldobj(ref Link ctxt, ref bio.Reader f, ref sym.Library lib, long length, @string pn, @string file, long whence)
        {
            var pkg = objabi.PathToPrefix(lib.Pkg);

            var eof = f.Offset() + length;
            var start = f.Offset();
            var c1 = bgetc(f);
            var c2 = bgetc(f);
            var c3 = bgetc(f);
            var c4 = bgetc(f);
            f.Seek(start, 0L);

            var magic = uint32(c1) << (int)(24L) | uint32(c2) << (int)(16L) | uint32(c3) << (int)(8L) | uint32(c4);
            if (magic == 0x7f454c46UL)
            { // \x7F E L F
                Action<ref Link, ref bio.Reader, @string, long, @string> ldelf = (ctxt, f, pkg, length, pn) =>
                {
                    var (textp, flags, err) = loadelf.Load(ctxt.Arch, ctxt.Syms, f, pkg, length, pn, ehdr.flags);
                    if (err != null)
                    {
                        Errorf(null, "%v", err);
                        return;
                    }
                    ehdr.flags = flags;
                    ctxt.Textp = append(ctxt.Textp, textp);
                }
;
                return ldhostobj(ldelf, ctxt.HeadType, f, pkg, length, pn, file);
            }
            if (magic & ~1L == 0xfeedfaceUL || magic & ~0x01000000UL == 0xcefaedfeUL)
            {
                Action<ref Link, ref bio.Reader, @string, long, @string> ldmacho = (ctxt, f, pkg, length, pn) =>
                {
                    var (textp, err) = loadmacho.Load(ctxt.Arch, ctxt.Syms, f, pkg, length, pn);
                    if (err != null)
                    {
                        Errorf(null, "%v", err);
                        return;
                    }
                    ctxt.Textp = append(ctxt.Textp, textp);
                }
;
                return ldhostobj(ldmacho, ctxt.HeadType, f, pkg, length, pn, file);
            }
            if (c1 == 0x4cUL && c2 == 0x01UL || c1 == 0x64UL && c2 == 0x86UL)
            {
                Action<ref Link, ref bio.Reader, @string, long, @string> ldpe = (ctxt, f, pkg, length, pn) =>
                {
                    var (textp, rsrc, err) = loadpe.Load(ctxt.Arch, ctxt.Syms, f, pkg, length, pn);
                    if (err != null)
                    {
                        Errorf(null, "%v", err);
                        return;
                    }
                    if (rsrc != null)
                    {
                        setpersrc(ctxt, rsrc);
                    }
                    ctxt.Textp = append(ctxt.Textp, textp);
                }
;
                return ldhostobj(ldpe, ctxt.HeadType, f, pkg, length, pn, file);
            } 

            /* check the header */
            var (line, err) = f.ReadString('\n');
            if (err != null)
            {
                Errorf(null, "truncated object file: %s: %v", pn, err);
                return null;
            }
            if (!strings.HasPrefix(line, "go object "))
            {
                if (strings.HasSuffix(pn, ".go"))
                {
                    Exitf("%s: uncompiled .go source file", pn);
                    return null;
                }
                if (line == ctxt.Arch.Name)
                { 
                    // old header format: just $GOOS
                    Errorf(null, "%s: stale object file", pn);
                    return null;
                }
                Errorf(null, "%s: not an object file", pn);
                return null;
            } 

            // First, check that the basic GOOS, GOARCH, and Version match.
            var t = fmt.Sprintf("%s %s %s ", objabi.GOOS, objabi.GOARCH, objabi.Version);

            line = strings.TrimRight(line, "\n");
            if (!strings.HasPrefix(line[10L..] + " ", t) && !flagF.Value)
            {
                Errorf(null, "%s: object is [%s] expected [%s]", pn, line[10L..], t);
                return null;
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
                    return null;
                }
            } 

            // Skip over exports and other info -- ends with \n!\n.
            //
            // Note: It's possible for "\n!\n" to appear within the binary
            // package export data format. To avoid truncating the package
            // definition prematurely (issue 21703), we keep keep track of
            // how many "$$" delimiters we've seen.
            var import0 = f.Offset();

            c1 = '\n'; // the last line ended in \n
            c2 = bgetc(f);
            c3 = bgetc(f);
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
                c3 = bgetc(f);
                if (c3 == -1L)
                {
                    Errorf(null, "truncated object file: %s", pn);
                    return null;
                }
            }


            var import1 = f.Offset();

            f.Seek(import0, 0L);
            ldpkg(ctxt, f, pkg, import1 - import0 - 2L, pn, whence); // -2 for !\n
            f.Seek(import1, 0L);

            objfile.Load(ctxt.Arch, ctxt.Syms, f, lib, eof - f.Offset(), pn);
            addImports(ctxt, lib, pn);
            return null;
        }

        private static slice<byte> readelfsymboldata(ref Link ctxt, ref elf.File f, ref elf.Symbol sym)
        {
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
            var data = make_slice<byte>(Rnd(int64(sz), 4L));
            var (_, err) = io.ReadFull(r, data);
            if (err != null)
            {
                return (null, err);
            }
            data = data[..sz];
            return (data, null);
        }

        private static (slice<byte>, error) readnote(ref elf.File f, slice<byte> name, int typ)
        {
            foreach (var (_, sect) in f.Sections)
            {
                if (sect.Type != elf.SHT_NOTE)
                {
                    continue;
                }
                var r = sect.Open();
                while (true)
                {
                    int namesize = default;                    int descsize = default;                    int noteType = default;

                    var err = binary.Read(r, f.ByteOrder, ref namesize);
                    if (err != null)
                    {
                        if (err == io.EOF)
                        {
                            break;
                        }
                        return (null, fmt.Errorf("read namesize failed: %v", err));
                    }
                    err = binary.Read(r, f.ByteOrder, ref descsize);
                    if (err != null)
                    {
                        return (null, fmt.Errorf("read descsize failed: %v", err));
                    }
                    err = binary.Read(r, f.ByteOrder, ref noteType);
                    if (err != null)
                    {
                        return (null, fmt.Errorf("read type failed: %v", err));
                    }
                    var (noteName, err) = readwithpad(r, namesize);
                    if (err != null)
                    {
                        return (null, fmt.Errorf("read name failed: %v", err));
                    }
                    var (desc, err) = readwithpad(r, descsize);
                    if (err != null)
                    {
                        return (null, fmt.Errorf("read desc failed: %v", err));
                    }
                    if (string(name) == string(noteName) && typ == noteType)
                    {
                        return (desc, null);
                    }
                }

            }
            return (null, null);
        }

        private static @string findshlib(ref Link ctxt, @string shlib)
        {
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

        private static void ldshlibsyms(ref Link _ctxt, @string shlib) => func(_ctxt, (ref Link ctxt, Defer defer, Panic _, Recover __) =>
        {
            @string libpath = default;
            if (filepath.IsAbs(shlib))
            {
                libpath = shlib;
                shlib = filepath.Base(shlib);
            }
            else
            {
                libpath = findshlib(ctxt, shlib);
                if (libpath == "")
                {
                    return;
                }
            }
            foreach (var (_, processedlib) in ctxt.Shlibs)
            {
                if (processedlib.Path == libpath)
                {
                    return;
                }
            }
            if (ctxt.Debugvlog > 1L)
            {
                ctxt.Logf("%5.2f ldshlibsyms: found library with name %s at %s\n", Cputime(), shlib, libpath);
            }
            var (f, err) = elf.Open(libpath);
            if (err != null)
            {
                Errorf(null, "cannot open shared library: %s", libpath);
                return;
            }
            defer(f.Close());

            var (hash, err) = readnote(f, ELF_NOTE_GO_NAME, ELF_NOTE_GOABIHASH_TAG);
            if (err != null)
            {
                Errorf(null, "cannot read ABI hash from shared library %s: %v", libpath, err);
                return;
            }
            var (depsbytes, err) = readnote(f, ELF_NOTE_GO_NAME, ELF_NOTE_GODEPS_TAG);
            if (err != null)
            {
                Errorf(null, "cannot read dep list from shared library %s: %v", libpath, err);
                return;
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
                return;
            }
            var gcdataLocations = make_map<ulong, ref sym.Symbol>();
            foreach (var (_, elfsym) in syms)
            {
                if (elf.ST_TYPE(elfsym.Info) == elf.STT_NOTYPE || elf.ST_TYPE(elfsym.Info) == elf.STT_SECTION)
                {
                    continue;
                }
                var lsym = ctxt.Syms.Lookup(elfsym.Name, 0L); 
                // Because loadlib above loads all .a files before loading any shared
                // libraries, any non-dynimport symbols we find that duplicate symbols
                // already loaded should be ignored (the symbols from the .a files
                // "win").
                if (lsym.Type != 0L && lsym.Type != sym.SDYNIMPORT)
                {
                    continue;
                }
                lsym.Type = sym.SDYNIMPORT;
                lsym.ElfType = elf.ST_TYPE(elfsym.Info);
                lsym.Size = int64(elfsym.Size);
                if (elfsym.Section != elf.SHN_UNDEF)
                { 
                    // Set .File for the library that actually defines the symbol.
                    lsym.File = libpath; 
                    // The decodetype_* functions in decodetype.go need access to
                    // the type data.
                    if (strings.HasPrefix(lsym.Name, "type.") && !strings.HasPrefix(lsym.Name, "type.."))
                    {
                        lsym.P = readelfsymboldata(ctxt, f, ref elfsym);
                        gcdataLocations[elfsym.Value + 2L * uint64(ctxt.Arch.PtrSize) + 8L + 1L * uint64(ctxt.Arch.PtrSize)] = lsym;
                    }
                }
            }
            var gcdataAddresses = make_map<ref sym.Symbol, ulong>();
            if (ctxt.Arch.Family == sys.ARM64)
            {
                foreach (var (_, sect) in f.Sections)
                {
                    if (sect.Type == elf.SHT_RELA)
                    {
                        elf.Rela64 rela = default;
                        var rdr = sect.Open();
                        while (true)
                        {
                            var err = binary.Read(rdr, f.ByteOrder, ref rela);
                            if (err == io.EOF)
                            {
                                break;
                            }
                            else if (err != null)
                            {
                                Errorf(null, "reading relocation failed %v", err);
                                return;
                            }
                            var t = elf.R_AARCH64(rela.Info & 0xffffUL);
                            if (t != elf.R_AARCH64_RELATIVE)
                            {
                                continue;
                            }
                            {
                                var lsym__prev3 = lsym;

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
        });

        private static ref sym.Section addsection(ref sys.Arch arch, ref sym.Segment seg, @string name, long rwx)
        {
            ptr<sym.Section> sect = @new<sym.Section>();
            sect.Rwx = uint8(rwx);
            sect.Name = name;
            sect.Seg = seg;
            sect.Align = int32(arch.PtrSize); // everything is at least pointer-aligned
            seg.Sections = append(seg.Sections, sect);
            return sect;
        }

        public static ushort Le16(slice<byte> b)
        {
            return uint16(b[0L]) | uint16(b[1L]) << (int)(8L);
        }

        public static uint Le32(slice<byte> b)
        {
            return uint32(b[0L]) | uint32(b[1L]) << (int)(8L) | uint32(b[2L]) << (int)(16L) | uint32(b[3L]) << (int)(24L);
        }

        public static ulong Le64(slice<byte> b)
        {
            return uint64(Le32(b)) | uint64(Le32(b[4L..])) << (int)(32L);
        }

        public static ushort Be16(slice<byte> b)
        {
            return uint16(b[0L]) << (int)(8L) | uint16(b[1L]);
        }

        public static uint Be32(slice<byte> b)
        {
            return uint32(b[0L]) << (int)(24L) | uint32(b[1L]) << (int)(16L) | uint32(b[2L]) << (int)(8L) | uint32(b[3L]);
        }

        private partial struct chain
        {
            public ptr<sym.Symbol> sym;
            public ptr<chain> up;
            public long limit; // limit on entry to sym
        }

        private static ref sym.Symbol morestack = default;

        // TODO: Record enough information in new object files to
        // allow stack checks here.

        private static bool haslinkregister(ref Link ctxt)
        {
            return ctxt.FixedFrameSize() != 0L;
        }

        private static long callsize(ref Link ctxt)
        {
            if (haslinkregister(ctxt))
            {
                return 0L;
            }
            return ctxt.Arch.RegSize;
        }

        private static void dostkcheck(this ref Link ctxt)
        {
            chain ch = default;

            morestack = ctxt.Syms.Lookup("runtime.morestack", 0L); 

            // Every splitting function ensures that there are at least StackLimit
            // bytes available below SP when the splitting prologue finishes.
            // If the splitting function calls F, then F begins execution with
            // at least StackLimit - callsize() bytes available.
            // Check that every function behaves correctly with this amount
            // of stack, following direct calls in order to piece together chains
            // of non-splitting functions.
            ch.up = null;

            ch.limit = objabi.StackLimit - callsize(ctxt); 

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
                        stkcheck(ctxt, ref ch, 0L);
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
                        stkcheck(ctxt, ref ch, 0L);
                    }
                }

                s = s__prev1;
            }

        }

        private static long stkcheck(ref Link ctxt, ref chain up, long depth)
        {
            var limit = up.limit;
            var s = up.sym; 

            // Don't duplicate work: only need to consider each
            // function at top of safe zone once.
            var top = limit == objabi.StackLimit - callsize(ctxt);
            if (top)
            {
                if (s.Attr.StackCheck())
                {
                    return 0L;
                }
                s.Attr |= sym.AttrStackCheck;
            }
            if (depth > 100L)
            {
                Errorf(s, "nosplit stack check too deep");
                stkbroke(ctxt, up, 0L);
                return -1L;
            }
            if (s.Attr.External() || s.FuncInfo == null)
            { 
                // external function.
                // should never be called directly.
                // onlyctxt.Diagnose the direct caller.
                // TODO(mwhudson): actually think about this.
                if (depth == 1L && s.Type != sym.SXREF && !ctxt.DynlinkingGo() && ctxt.BuildMode != BuildModeCArchive && ctxt.BuildMode != BuildModePIE && ctxt.BuildMode != BuildModeCShared && ctxt.BuildMode != BuildModePlugin)
                {
                    Errorf(s, "call to external function");
                }
                return -1L;
            }
            if (limit < 0L)
            {
                stkbroke(ctxt, up, limit);
                return -1L;
            } 

            // morestack looks like it calls functions,
            // but it switches the stack pointer first.
            if (s == morestack)
            {
                return 0L;
            }
            chain ch = default;
            ch.up = up;

            if (!s.Attr.NoSplit())
            { 
                // Ensure we have enough stack to call morestack.
                ch.limit = limit - callsize(ctxt);
                ch.sym = morestack;
                if (stkcheck(ctxt, ref ch, depth + 1L) < 0L)
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
                limit = int(objabi.StackLimit + locals) + int(ctxt.FixedFrameSize());
            } 

            // Walk through sp adjustments in function, consuming relocs.
            long ri = 0L;

            var endr = len(s.R);
            chain ch1 = default;
            Pciter pcsp = default;
            ref sym.Reloc r = default;
            pciterinit(ctxt, ref pcsp, ref s.FuncInfo.Pcsp);

            while (pcsp.done == 0L)
            { 
                // pcsp.value is in effect for [pcsp.pc, pcsp.nextpc).

                // Check stack size in effect for this span.
                if (int32(limit) - pcsp.value < 0L)
                {
                    stkbroke(ctxt, up, int(int32(limit) - pcsp.value));
                    return -1L;
                pciternext(ref pcsp);
                } 

                // Process calls in this span.
                while (ri < endr && uint32(s.R[ri].Off) < pcsp.nextpc)
                {
                    r = ref s.R[ri];

                    // Direct call.
                    if (r.Type == objabi.R_CALL || r.Type == objabi.R_CALLARM || r.Type == objabi.R_CALLARM64 || r.Type == objabi.R_CALLPOWER || r.Type == objabi.R_CALLMIPS) 
                        ch.limit = int(int32(limit) - pcsp.value - int32(callsize(ctxt)));
                        ch.sym = r.Sym;
                        if (stkcheck(ctxt, ref ch, depth + 1L) < 0L)
                        {
                            return -1L;
                    ri++;
                        } 

                        // Indirect call. Assume it is a call to a splitting function,
                        // so we have to make sure it can call morestack.
                        // Arrange the data structures to report both calls, so that
                        // if there is an error, stkprint shows all the steps involved.
                    else if (r.Type == objabi.R_CALLIND) 
                        ch.limit = int(int32(limit) - pcsp.value - int32(callsize(ctxt)));

                        ch.sym = null;
                        ch1.limit = ch.limit - callsize(ctxt); // for morestack in called prologue
                        ch1.up = ref ch;
                        ch1.sym = morestack;
                        if (stkcheck(ctxt, ref ch1, depth + 2L) < 0L)
                        {
                            return -1L;
                        }
                                    }

            }


            return 0L;
        }

        private static void stkbroke(ref Link ctxt, ref chain ch, long limit)
        {
            Errorf(ch.sym, "nosplit stack overflow");
            stkprint(ctxt, ch, limit);
        }

        private static void stkprint(ref Link ctxt, ref chain ch, long limit)
        {
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
                stkprint(ctxt, ch.up, ch.limit + callsize(ctxt));
                if (!haslinkregister(ctxt))
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
            objabi.Flagprint(2L);
            Exit(2L);
        }

        private static void doversion()
        {
            Exitf("version %s", objabi.Version);
        }

        public partial struct SymbolType // : sbyte
        {
        }

 
        // see also http://9p.io/magic/man2html/1/nm
        public static readonly SymbolType TextSym = 'T';
        public static readonly char DataSym = 'D';
        public static readonly char BSSSym = 'B';
        public static readonly char UndefinedSym = 'U';
        public static readonly char TLSSym = 't';
        public static readonly char FrameSym = 'm';
        public static readonly char ParamSym = 'p';
        public static readonly char AutoSym = 'a'; 

        // Deleted auto (not a real sym, just placeholder for type)
        public static readonly char DeletedAutoSym = 'x';

        private static void genasmsym(ref Link ctxt, Action<ref Link, ref sym.Symbol, @string, SymbolType, long, ref sym.Symbol> put)
        { 
            // These symbols won't show up in the first loop below because we
            // skip sym.STEXT symbols. Normal sym.STEXT symbols are emitted by walking textp.
            var s = ctxt.Syms.Lookup("runtime.text", 0L);
            if (s.Type == sym.STEXT)
            { 
                // We've already included this symbol in ctxt.Textp
                // if ctxt.DynlinkingGo() && ctxt.HeadType == objabi.Hdarwin.
                // See data.go:/textaddress
                if (!(ctxt.DynlinkingGo() && ctxt.HeadType == objabi.Hdarwin))
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
                if (sect.Name != ".text")
                {
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
                // if ctxt.DynlinkingGo() && ctxt.HeadType == objabi.Hdarwin.
                // See data.go:/textaddress
                if (!(ctxt.DynlinkingGo() && ctxt.HeadType == objabi.Hdarwin))
                {
                    put(ctxt, s, s.Name, TextSym, s.Value, null);
                }
            }
            {
                var s__prev1 = s;

                foreach (var (_, __s) in ctxt.Syms.Allsym)
                {
                    s = __s;
                    if (s.Attr.NotInSymbolTable())
                    {
                        continue;
                    }
                    if ((s.Name == "" || s.Name[0L] == '.') && s.Version == 0L && s.Name != ".rathole" && s.Name != ".TOC.")
                    {
                        continue;
                    }

                    if (s.Type == sym.SCONST || s.Type == sym.SRODATA || s.Type == sym.SSYMTAB || s.Type == sym.SPCLNTAB || s.Type == sym.SINITARR || s.Type == sym.SDATA || s.Type == sym.SNOPTRDATA || s.Type == sym.SELFROSECT || s.Type == sym.SMACHOGOT || s.Type == sym.STYPE || s.Type == sym.SSTRING || s.Type == sym.SGOSTRING || s.Type == sym.SGOFUNC || s.Type == sym.SGCBITS || s.Type == sym.STYPERELRO || s.Type == sym.SSTRINGRELRO || s.Type == sym.SGOSTRINGRELRO || s.Type == sym.SGOFUNCRELRO || s.Type == sym.SGCBITSRELRO || s.Type == sym.SRODATARELRO || s.Type == sym.STYPELINK || s.Type == sym.SITABLINK || s.Type == sym.SWINDOWS) 
                        if (!s.Attr.Reachable())
                        {
                            continue;
                        }
                        put(ctxt, s, s.Name, DataSym, Symaddr(s), s.Gotype);
                    else if (s.Type == sym.SBSS || s.Type == sym.SNOPTRBSS) 
                        if (!s.Attr.Reachable())
                        {
                            continue;
                        }
                        if (len(s.P) > 0L)
                        {
                            Errorf(s, "should not be bss (size=%d type=%v special=%v)", len(s.P), s.Type, s.Attr.Special());
                        }
                        put(ctxt, s, s.Name, BSSSym, Symaddr(s), s.Gotype);
                    else if (s.Type == sym.SHOSTOBJ) 
                        if (ctxt.HeadType == objabi.Hwindows || ctxt.IsELF)
                        {
                            put(ctxt, s, s.Name, UndefinedSym, s.Value, null);
                        }
                    else if (s.Type == sym.SDYNIMPORT) 
                        if (!s.Attr.Reachable())
                        {
                            continue;
                        }
                        put(ctxt, s, s.Extname, UndefinedSym, 0L, null);
                    else if (s.Type == sym.STLSBSS) 
                        if (ctxt.LinkMode == LinkExternal)
                        {
                            put(ctxt, s, s.Name, TLSSym, Symaddr(s), s.Gotype);
                        }
                                    }

                s = s__prev1;
            }

            int off = default;
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
                    foreach (var (_, a) in s.FuncInfo.Autom)
                    {
                        if (a.Name == objabi.A_DELETED_AUTO)
                        {
                            put(ctxt, null, "", DeletedAutoSym, 0L, a.Gotype);
                            continue;
                        } 

                        // Emit a or p according to actual offset, even if label is wrong.
                        // This avoids negative offsets, which cannot be encoded.
                        if (a.Name != objabi.A_AUTO && a.Name != objabi.A_PARAM)
                        {
                            continue;
                        } 

                        // compute offset relative to FP
                        if (a.Name == objabi.A_PARAM)
                        {
                            off = a.Aoffset;
                        }
                        else
                        {
                            off = a.Aoffset - int32(ctxt.Arch.PtrSize);
                        } 

                        // FP
                        if (off >= 0L)
                        {
                            put(ctxt, null, a.Asym.Name, ParamSym, int64(off), a.Gotype);
                            continue;
                        } 

                        // SP
                        if (off <= int32(-ctxt.Arch.PtrSize))
                        {
                            put(ctxt, null, a.Asym.Name, AutoSym, -(int64(off) + int64(ctxt.Arch.PtrSize)), a.Gotype);
                            continue;
                        } 
                        // Otherwise, off is addressing the saved program counter.
                        // Something underhanded is going on. Say nothing.
                    }
                }

                s = s__prev1;
            }

            if (ctxt.Debugvlog != 0L || flagN.Value)
            {
                ctxt.Logf("%5.2f symsize = %d\n", Cputime(), uint32(Symsize));
            }
        }

        public static long Symaddr(ref sym.Symbol s)
        {
            if (!s.Attr.Reachable())
            {
                Errorf(s, "unreachable symbol in symaddr");
            }
            return s.Value;
        }

        private static void xdefine(this ref Link ctxt, @string p, sym.SymKind t, long v)
        {
            var s = ctxt.Syms.Lookup(p, 0L);
            s.Type = t;
            s.Value = v;
            s.Attr |= sym.AttrReachable;
            s.Attr |= sym.AttrSpecial;
            s.Attr |= sym.AttrLocal;
        }

        private static long datoff(ref sym.Symbol s, long addr)
        {
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

        public static long Entryvalue(ref Link ctxt)
        {
            var a = flagEntrySymbol.Value;
            if (a[0L] >= '0' && a[0L] <= '9')
            {
                return atolwhex(a);
            }
            var s = ctxt.Syms.Lookup(a, 0L);
            if (s.Type == 0L)
            {
                return FlagTextAddr.Value;
            }
            if (s.Type != sym.STEXT)
            {
                Errorf(s, "entry not text");
            }
            return s.Value;
        }

        private static void undefsym(ref Link ctxt, ref sym.Symbol s)
        {
            ref sym.Reloc r = default;

            for (long i = 0L; i < len(s.R); i++)
            {
                r = ref s.R[i];
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

        private static void undef(this ref Link ctxt)
        {
            {
                var s__prev1 = s;

                foreach (var (_, __s) in ctxt.Textp)
                {
                    s = __s;
                    undefsym(ctxt, s);
                }

                s = s__prev1;
            }

            {
                var s__prev1 = s;

                foreach (var (_, __s) in datap)
                {
                    s = __s;
                    undefsym(ctxt, s);
                }

                s = s__prev1;
            }

            if (nerrors > 0L)
            {
                errorexit();
            }
        }

        private static void callgraph(this ref Link ctxt)
        {
            if (!FlagC.Value)
            {
                return;
            }
            long i = default;
            ref sym.Reloc r = default;
            foreach (var (_, s) in ctxt.Textp)
            {
                for (i = 0L; i < len(s.R); i++)
                {
                    r = ref s.R[i];
                    if (r.Sym == null)
                    {
                        continue;
                    }
                    if ((r.Type == objabi.R_CALL || r.Type == objabi.R_CALLARM || r.Type == objabi.R_CALLPOWER || r.Type == objabi.R_CALLMIPS) && r.Sym.Type == sym.STEXT)
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

        private static long bgetc(ref bio.Reader r)
        {
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
        private static readonly markKind unvisited = iota;
        private static readonly var visiting = 0;
        private static readonly var visited = 1;

        private static slice<ref sym.Library> postorder(slice<ref sym.Library> libs)
        {
            var order = make_slice<ref sym.Library>(0L, len(libs)); // hold the result
            var mark = make_map<ref sym.Library, markKind>(len(libs));
            foreach (var (_, lib) in libs)
            {
                dfs(lib, mark, ref order);
            }
            return order;
        }

        private static void dfs(ref sym.Library _lib, map<ref sym.Library, markKind> mark, ref slice<ref sym.Library> _order) => func(_lib, _order, (ref sym.Library lib, ref slice<ref sym.Library> order, Defer _, Panic panic, Recover __) =>
        {
            if (mark[lib] == visited)
            {
                return;
            }
            if (mark[lib] == visiting)
            {
                panic("found import cycle while visiting " + lib.Pkg);
            }
            mark[lib] = visiting;
            foreach (var (_, i) in lib.Imports)
            {
                dfs(i, mark, order);
            }
            mark[lib] = visited;
            order.Value = append(order.Value, lib);
        });
    }
}}}}
