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

// package ld -- go2cs converted at 2020 October 08 04:38:57 UTC
// import "cmd/link/internal/ld" ==> using ld = go.cmd.link.@internal.ld_package
// Original source: C:\Go\src\cmd\link\internal\ld\lib.go
using bytes = go.bytes_package;
using bio = go.cmd.@internal.bio_package;
using goobj2 = go.cmd.@internal.goobj2_package;
using obj = go.cmd.@internal.obj_package;
using objabi = go.cmd.@internal.objabi_package;
using sys = go.cmd.@internal.sys_package;
using loadelf = go.cmd.link.@internal.loadelf_package;
using loader = go.cmd.link.@internal.loader_package;
using loadmacho = go.cmd.link.@internal.loadmacho_package;
using loadpe = go.cmd.link.@internal.loadpe_package;
using loadxcoff = go.cmd.link.@internal.loadxcoff_package;
using sym = go.cmd.link.@internal.sym_package;
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
namespace link {
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
        public delegate  ptr<sym.Symbol> LookupFn(@string,  long);

        // ArchSyms holds a number of architecture specific symbols used during
        // relocation.  Rather than allowing them universal access to all symbols,
        // we keep a subset for relocation application.
        public partial struct ArchSyms
        {
            public ptr<sym.Symbol> TOC;
            public slice<ptr<sym.Symbol>> DotTOC; // for each version

            public ptr<sym.Symbol> GOT;
            public ptr<sym.Symbol> PLT;
            public ptr<sym.Symbol> GOTPLT;
            public ptr<sym.Symbol> Tlsg;
            public long Tlsoffset;
            public ptr<sym.Symbol> Dynamic;
            public ptr<sym.Symbol> DynSym;
            public ptr<sym.Symbol> DynStr; // Elf specific
            public ptr<sym.Symbol> Rel;
            public ptr<sym.Symbol> Rela;
            public ptr<sym.Symbol> RelPLT;
            public ptr<sym.Symbol> RelaPLT; // Darwin symbols
            public ptr<sym.Symbol> LinkEditGOT;
            public ptr<sym.Symbol> LinkEditPLT; // ----- loader.Sym equivalents -----

            public loader.Sym Rel2;
            public loader.Sym Rela2;
            public loader.Sym RelPLT2;
            public loader.Sym RelaPLT2;
            public loader.Sym LinkEditGOT2;
            public loader.Sym LinkEditPLT2;
            public loader.Sym TOC2;
            public slice<loader.Sym> DotTOC2; // for each version

            public loader.Sym GOT2;
            public loader.Sym PLT2;
            public loader.Sym GOTPLT2;
            public loader.Sym Tlsg2;
            public loader.Sym Dynamic2;
            public loader.Sym DynSym2;
            public loader.Sym DynStr2;
        }

        public static readonly long BeforeLoadlibFull = (long)1L;

        public static readonly long AfterLoadlibFull = (long)2L;

        // mkArchSym is a helper for setArchSyms, invoked once before loadlibfull
        // and once after. On the first call it creates a loader.Sym with the
        // specified name, and on the second call a corresponding sym.Symbol.


        // mkArchSym is a helper for setArchSyms, invoked once before loadlibfull
        // and once after. On the first call it creates a loader.Sym with the
        // specified name, and on the second call a corresponding sym.Symbol.
        private static void mkArchSym(this ptr<Link> _addr_ctxt, long which, @string name, long ver, ptr<loader.Sym> _addr_ls, ptr<ptr<sym.Symbol>> _addr_ss)
        {
            ref Link ctxt = ref _addr_ctxt.val;
            ref loader.Sym ls = ref _addr_ls.val;
            ref ptr<sym.Symbol> ss = ref _addr_ss.val;

            if (which == BeforeLoadlibFull)
            {
                ls = ctxt.loader.LookupOrCreateSym(name, ver);
                ctxt.loader.SetAttrReachable(ls, true);
            }
            else
            {
                ss.val = ctxt.loader.Syms[ls];
            }

        }

        // mkArchVecSym is similar to  setArchSyms, but operates on elements within
        // a slice, where each element corresponds to some symbol version.
        private static void mkArchSymVec(this ptr<Link> _addr_ctxt, long which, @string name, long ver, slice<loader.Sym> ls, slice<ptr<sym.Symbol>> ss)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            if (which == BeforeLoadlibFull)
            {
                ls[ver] = ctxt.loader.LookupOrCreateSym(name, ver);
                ctxt.loader.SetAttrReachable(ls[ver], true);
            }
            else if (ls[ver] != 0L)
            {
                ss[ver] = ctxt.loader.Syms[ls[ver]];
            }

        }

        // setArchSyms sets up the ArchSyms structure, and must be called before
        // relocations are applied. This function is invoked twice, once prior
        // to loadlibfull(), and once after the work of loadlibfull is complete.
        private static void setArchSyms(this ptr<Link> _addr_ctxt, long which) => func((_, panic, __) =>
        {
            ref Link ctxt = ref _addr_ctxt.val;

            if (which != BeforeLoadlibFull && which != AfterLoadlibFull)
            {
                panic("internal error");
            }

            ctxt.mkArchSym(which, ".got", 0L, _addr_ctxt.GOT2, _addr_ctxt.GOT);
            ctxt.mkArchSym(which, ".plt", 0L, _addr_ctxt.PLT2, _addr_ctxt.PLT);
            ctxt.mkArchSym(which, ".got.plt", 0L, _addr_ctxt.GOTPLT2, _addr_ctxt.GOTPLT);
            ctxt.mkArchSym(which, ".dynamic", 0L, _addr_ctxt.Dynamic2, _addr_ctxt.Dynamic);
            ctxt.mkArchSym(which, ".dynsym", 0L, _addr_ctxt.DynSym2, _addr_ctxt.DynSym);
            ctxt.mkArchSym(which, ".dynstr", 0L, _addr_ctxt.DynStr2, _addr_ctxt.DynStr);

            if (ctxt.IsPPC64())
            {
                ctxt.mkArchSym(which, "TOC", 0L, _addr_ctxt.TOC2, _addr_ctxt.TOC); 

                // NB: note the +2 below for DotTOC2 compared to the +1 for
                // DocTOC. This is because loadlibfull() creates an additional
                // syms version during conversion of loader.Sym symbols to
                // *sym.Symbol symbols. Symbols that are assigned this final
                // version are not going to have TOC references, so it should
                // be ok for them to inherit an invalid .TOC. symbol.
                if (which == BeforeLoadlibFull)
                {
                    ctxt.DotTOC2 = make_slice<loader.Sym>(ctxt.Syms.MaxVersion() + 2L);
                }
                else
                {
                    ctxt.DotTOC = make_slice<ptr<sym.Symbol>>(ctxt.Syms.MaxVersion() + 1L);
                }

                for (long i = 0L; i <= ctxt.Syms.MaxVersion(); i++)
                {
                    if (i >= 2L && i < sym.SymVerStatic)
                    { // these versions are not used currently
                        continue;

                    }

                    ctxt.mkArchSymVec(which, ".TOC.", i, ctxt.DotTOC2, ctxt.DotTOC);

                }


            }

            if (ctxt.IsElf())
            {
                ctxt.mkArchSym(which, ".rel", 0L, _addr_ctxt.Rel2, _addr_ctxt.Rel);
                ctxt.mkArchSym(which, ".rela", 0L, _addr_ctxt.Rela2, _addr_ctxt.Rela);
                ctxt.mkArchSym(which, ".rel.plt", 0L, _addr_ctxt.RelPLT2, _addr_ctxt.RelPLT);
                ctxt.mkArchSym(which, ".rela.plt", 0L, _addr_ctxt.RelaPLT2, _addr_ctxt.RelaPLT);
            }

            if (ctxt.IsDarwin())
            {
                ctxt.mkArchSym(which, ".linkedit.got", 0L, _addr_ctxt.LinkEditGOT2, _addr_ctxt.LinkEditGOT);
                ctxt.mkArchSym(which, ".linkedit.plt", 0L, _addr_ctxt.LinkEditPLT2, _addr_ctxt.LinkEditPLT);
            }

        });

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
            public Func<ptr<Target>, ptr<loader.Loader>, ptr<ArchSyms>, ptr<sym.Symbol>, ptr<sym.Reloc>, bool> Adddynrel;
            public Func<ptr<Target>, ptr<loader.Loader>, ptr<ArchSyms>, loader.Sym, loader.Reloc2, long, bool> Adddynrel2;
            public Action<ptr<Link>> Archinit; // Archreloc is an arch-specific hook that assists in relocation processing
// (invoked by 'relocsym'); it handles target-specific relocation tasks.
// Here "rel" is the current relocation being examined, "sym" is the symbol
// containing the chunk of data to which the relocation applies, and "off"
// is the contents of the to-be-relocated data item (from sym.P). Return
// value is the appropriately relocated value (to be written back to the
// same spot in sym.P), a boolean indicating if the external relocations'
// been used, and a boolean indicating success/failure (a failing value
// indicates a fatal error).
            public Func<ptr<Target>, ptr<ArchSyms>, ptr<sym.Reloc>, ptr<sym.Symbol>, long, (long, bool)> Archreloc;
            public Func<ptr<Target>, ptr<loader.Loader>, ptr<ArchSyms>, loader.Reloc2, ptr<loader.ExtReloc>, loader.Sym, long, (long, bool, bool)> Archreloc2; // Archrelocvariant is a second arch-specific hook used for
// relocation processing; it handles relocations where r.Type is
// insufficient to describe the relocation (r.Variant !=
// sym.RV_NONE). Here "rel" is the relocation being applied, "sym"
// is the symbol containing the chunk of data to which the
// relocation applies, and "off" is the contents of the
// to-be-relocated data item (from sym.P). Return is an updated
// offset value.
            public Func<ptr<Target>, ptr<ArchSyms>, ptr<sym.Reloc>, ptr<sym.Symbol>, long, long> Archrelocvariant; // Generate a trampoline for a call from s to rs if necessary. ri is
// index of the relocation.
            public Action<ptr<Link>, ptr<loader.Loader>, long, loader.Sym, loader.Sym> Trampoline; // Asmb and Asmb2 are arch-specific routines that write the output
// file. Typically, Asmb writes most of the content (sections and
// segments), for which we have computed the size and offset. Asmb2
// writes the rest.
            public Action<ptr<Link>, ptr<loader.Loader>> Asmb;
            public Action<ptr<Link>> Asmb2;
            public Func<ptr<Link>, ptr<sym.Reloc>, long, bool> Elfreloc1;
            public Func<ptr<Link>, ptr<loader.Loader>, loader.Sym, loader.ExtRelocView, long, bool> Elfreloc2;
            public Action<ptr<Link>, ptr<loader.SymbolBuilder>, ptr<loader.SymbolBuilder>, loader.Sym> Elfsetupplt;
            public Action<ptr<Link>> Gentext;
            public Action<ptr<Link>, ptr<loader.Loader>> Gentext2;
            public Func<ptr<sys.Arch>, ptr<OutBuf>, ptr<sym.Symbol>, ptr<sym.Reloc>, long, bool> Machoreloc1;
            public Func<ptr<sys.Arch>, ptr<OutBuf>, ptr<sym.Symbol>, ptr<sym.Reloc>, long, bool> PEreloc1;
            public Func<ptr<sys.Arch>, ptr<OutBuf>, ptr<sym.Symbol>, ptr<sym.Reloc>, long, bool> Xcoffreloc1; // TLSIEtoLE converts a TLS Initial Executable relocation to
// a TLS Local Executable relocation.
//
// This is possible when a TLS IE relocation refers to a local
// symbol in an executable, which is typical when internally
// linking PIE binaries.
            public Action<slice<byte>, long, long> TLSIEtoLE; // optional override for assignAddress
            public Func<ptr<loader.Loader>, ptr<sym.Section>, long, loader.Sym, ulong, bool, (ptr<sym.Section>, long, ulong)> AssignAddress;
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

            {
                var err = ctxt.Out.Open(flagOutfile.val);

                if (err != null)
                {
                    Exitf("cannot create %s: %v", flagOutfile.val, err);
                }

            }


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

            goobj2.FingerprintType zerofp = new goobj2.FingerprintType();
            if (ctxt.linkShared && ctxt.PackageShlib != null)
            {
                {
                    var shlib = ctxt.PackageShlib[name];

                    if (shlib != "")
                    {
                        return _addr_addlibpath(ctxt, "internal", "internal", "", name, shlib, zerofp)!;
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
                        return _addr_addlibpath(ctxt, "internal", "internal", pname, name, "", zerofp)!;
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
                            return _addr_addlibpath(ctxt, "internal", "internal", "", name, shlibname, zerofp)!;
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
                        return _addr_addlibpath(ctxt, "internal", "internal", pname, name, "", zerofp)!;
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
            ctxt.loader = loader.NewLoader(flags, elfsetstring, _addr_ctxt.ErrorReporter.ErrorReporter);
            ctxt.ErrorReporter.SymName = s =>
            {
                return ctxt.loader.SymName(s);
            }
;

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
            // At this point, the Go objects are "preloaded". Not all the symbols are
            // added to the symbol table (only defined package symbols are). Looking
            // up symbol by name may not get expected result.
 
            // At this point, the Go objects are "preloaded". Not all the symbols are
            // added to the symbol table (only defined package symbols are). Looking
            // up symbol by name may not get expected result.

            iscgo = ctxt.LibraryByPkg["runtime/cgo"] != null;
            ctxt.canUsePlugins = ctxt.LibraryByPkg["plugin"] != null; 

            // We now have enough information to determine the link mode.
            determineLinkMode(ctxt);

            if (ctxt.LinkMode == LinkExternal && !iscgo && !(objabi.GOOS == "darwin" && ctxt.BuildMode != BuildModePlugin && ctxt.Arch.Family == sys.AMD64))
            { 
                // This indicates a user requested -linkmode=external.
                // The startup code uses an import of runtime/cgo to decide
                // whether to initialize the TLS.  So give it one. This could
                // be handled differently but it's an unusual case.
                {
                    var lib__prev2 = lib;

                    lib = loadinternal(_addr_ctxt, "runtime/cgo");

                    if (lib != null && lib.Shlib == "")
                    {
                        if (ctxt.BuildMode == BuildModeShared || ctxt.linkShared)
                        {
                            Exitf("cannot implicitly include runtime/cgo in a shared library");
                        }

                        loadobjfile(_addr_ctxt, _addr_lib);

                    }

                    lib = lib__prev2;

                }

            } 

            // Add non-package symbols and references of externally defined symbols.
            ctxt.loader.LoadNonpkgSyms(ctxt.Arch); 

            // Load symbols from shared libraries, after all Go object symbols are loaded.
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

                // Process cgo directives (has to be done before host object loading).

                lib = lib__prev1;
            }

            ctxt.loadcgodirectives(); 

            // Conditionally load host objects, or setup for external linking.
            hostobjs(_addr_ctxt);
            hostlinksetup(_addr_ctxt);

            if (ctxt.LinkMode == LinkInternal && len(hostobj) != 0L)
            { 
                // If we have any undefined symbols in external
                // objects, try to read them from the libgcc file.
                var any = false;
                var undefs = ctxt.loader.UndefinedRelocTargets(1L);
                if (len(undefs) > 0L)
                {
                    any = true;
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

            strictDupMsgCount = ctxt.loader.NStrictDupMsgs();

        }

        // genSymsForDynexp constructs a *sym.Symbol version of ctxt.dynexp,
        // writing to the global variable 'dynexp'.
        private static void genSymsForDynexp(ptr<Link> _addr_ctxt)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            dynexp = make_slice<ptr<sym.Symbol>>(len(ctxt.dynexp2));
            foreach (var (i, s) in ctxt.dynexp2)
            {
                dynexp[i] = ctxt.loader.Syms[s];
            }

        }

        // setupdynexp constructs ctxt.dynexp, a list of loader.Sym.
        private static void setupdynexp(ptr<Link> _addr_ctxt) => func((_, panic, __) =>
        {
            ref Link ctxt = ref _addr_ctxt.val;

            var dynexpMap = ctxt.cgo_export_dynamic;
            if (ctxt.LinkMode == LinkExternal)
            {
                dynexpMap = ctxt.cgo_export_static;
            }

            var d = make_slice<loader.Sym>(0L, len(dynexpMap));
            foreach (var (exp) in dynexpMap)
            {
                var s = ctxt.loader.LookupOrCreateSym(exp, 0L);
                d = append(d, s); 
                // sanity check
                if (!ctxt.loader.AttrReachable(s))
                {
                    panic("dynexp entry not reachable");
                }

            }
            sort.Slice(d, (i, j) =>
            {
                return ctxt.loader.SymName(d[i]) < ctxt.loader.SymName(d[j]);
            }); 

            // Resolve ABI aliases in the list of cgo-exported functions.
            // This is necessary because we load the ABI0 symbol for all
            // cgo exports.
            {
                var s__prev1 = s;

                foreach (var (__i, __s) in d)
                {
                    i = __i;
                    s = __s;
                    if (ctxt.loader.SymType(s) != sym.SABIALIAS)
                    {
                        continue;
                    }

                    var t = ctxt.loader.ResolveABIAlias(s);
                    ctxt.loader.CopyAttributes(s, t);
                    ctxt.loader.SetSymExtname(t, ctxt.loader.SymExtname(s));
                    d[i] = t;

                }

                s = s__prev1;
            }

            ctxt.dynexp2 = d;

            ctxt.cgo_export_static = null;
            ctxt.cgo_export_dynamic = null;

        });

        // loadcgodirectives reads the previously discovered cgo directives, creating
        // symbols in preparation for host object loading or use later in the link.
        private static void loadcgodirectives(this ptr<Link> _addr_ctxt)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            var l = ctxt.loader;
            var hostObjSyms = make();
            foreach (var (_, d) in ctxt.cgodata)
            {
                setCgoAttr(ctxt, ctxt.loader.LookupOrCreateSym, d.file, d.pkg, d.directives, hostObjSyms);
            }
            ctxt.cgodata = null;

            if (ctxt.LinkMode == LinkInternal)
            { 
                // Drop all the cgo_import_static declarations.
                // Turns out we won't be needing them.
                foreach (var (symIdx) in hostObjSyms)
                {
                    if (l.SymType(symIdx) == sym.SHOSTOBJ)
                    { 
                        // If a symbol was marked both
                        // cgo_import_static and cgo_import_dynamic,
                        // then we want to make it cgo_import_dynamic
                        // now.
                        var su = l.MakeSymbolUpdater(symIdx);
                        if (l.SymExtname(symIdx) != "" && l.SymDynimplib(symIdx) != "" && !(l.AttrCgoExportStatic(symIdx) || l.AttrCgoExportDynamic(symIdx)))
                        {
                            su.SetType(sym.SDYNIMPORT);
                        }
                        else
                        {
                            su.SetType(0L);
                        }

                    }

                }

            }

        }

        // Set up flags and special symbols depending on the platform build mode.
        // This version works with loader.Loader.
        private static void linksetup(this ptr<Link> _addr_ctxt)
        {
            ref Link ctxt = ref _addr_ctxt.val;


            if (ctxt.BuildMode == BuildModeCShared || ctxt.BuildMode == BuildModePlugin) 
                var symIdx = ctxt.loader.LookupOrCreateSym("runtime.islibrary", 0L);
                var sb = ctxt.loader.MakeSymbolUpdater(symIdx);
                sb.SetType(sym.SNOPTRDATA);
                sb.AddUint8(1L);
            else if (ctxt.BuildMode == BuildModeCArchive) 
                symIdx = ctxt.loader.LookupOrCreateSym("runtime.isarchive", 0L);
                sb = ctxt.loader.MakeSymbolUpdater(symIdx);
                sb.SetType(sym.SNOPTRDATA);
                sb.AddUint8(1L);
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
                var toc = ctxt.loader.LookupOrCreateSym(".TOC.", 0L);
                sb = ctxt.loader.MakeSymbolUpdater(toc);
                sb.SetType(sym.SDYNIMPORT);
            } 

            // The Android Q linker started to complain about underalignment of the our TLS
            // section. We don't actually use the section on android, so don't
            // generate it.
            if (objabi.GOOS != "android")
            {
                var tlsg = ctxt.loader.LookupOrCreateSym("runtime.tlsg", 0L);
                sb = ctxt.loader.MakeSymbolUpdater(tlsg); 

                // runtime.tlsg is used for external linking on platforms that do not define
                // a variable to hold g in assembly (currently only intel).
                if (sb.Type() == 0L)
                {
                    sb.SetType(sym.STLSBSS);
                    sb.SetSize(int64(ctxt.Arch.PtrSize));
                }
                else if (sb.Type() != sym.SDYNIMPORT)
                {
                    Errorf(null, "runtime declared tlsg variable %v", sb.Type());
                }

                ctxt.loader.SetAttrReachable(tlsg, true);
                ctxt.Tlsg2 = tlsg;

            }

            loader.Sym moduledata = default;
            ptr<loader.SymbolBuilder> mdsb;
            if (ctxt.BuildMode == BuildModePlugin)
            {
                moduledata = ctxt.loader.LookupOrCreateSym("local.pluginmoduledata", 0L);
                mdsb = ctxt.loader.MakeSymbolUpdater(moduledata);
                ctxt.loader.SetAttrLocal(moduledata, true);
            }
            else
            {
                moduledata = ctxt.loader.LookupOrCreateSym("runtime.firstmoduledata", 0L);
                mdsb = ctxt.loader.MakeSymbolUpdater(moduledata);
            }

            if (mdsb.Type() != 0L && mdsb.Type() != sym.SDYNIMPORT)
            { 
                // If the module (toolchain-speak for "executable or shared
                // library") we are linking contains the runtime package, it
                // will define the runtime.firstmoduledata symbol and we
                // truncate it back to 0 bytes so we can define its entire
                // contents in symtab.go:symtab().
                mdsb.SetSize(0L); 

                // In addition, on ARM, the runtime depends on the linker
                // recording the value of GOARM.
                if (ctxt.Arch.Family == sys.ARM)
                {
                    var goarm = ctxt.loader.LookupOrCreateSym("runtime.goarm", 0L);
                    sb = ctxt.loader.MakeSymbolUpdater(goarm);
                    sb.SetType(sym.SDATA);
                    sb.SetSize(0L);
                    sb.AddUint8(uint8(objabi.GOARM));
                }

                if (objabi.Framepointer_enabled(objabi.GOOS, objabi.GOARCH))
                {
                    var fpe = ctxt.loader.LookupOrCreateSym("runtime.framepointer_enabled", 0L);
                    sb = ctxt.loader.MakeSymbolUpdater(fpe);
                    sb.SetType(sym.SNOPTRDATA);
                    sb.SetSize(0L);
                    sb.AddUint8(1L);
                }

            }
            else
            { 
                // If OTOH the module does not contain the runtime package,
                // create a local symbol for the moduledata.
                moduledata = ctxt.loader.LookupOrCreateSym("local.moduledata", 0L);
                mdsb = ctxt.loader.MakeSymbolUpdater(moduledata);
                ctxt.loader.SetAttrLocal(moduledata, true);

            } 
            // In all cases way we mark the moduledata as noptrdata to hide it from
            // the GC.
            mdsb.SetType(sym.SNOPTRDATA);
            ctxt.loader.SetAttrReachable(moduledata, true);
            ctxt.Moduledata2 = moduledata; 

            // If package versioning is required, generate a hash of the
            // packages used in the link.
            if (ctxt.BuildMode == BuildModeShared || ctxt.BuildMode == BuildModePlugin || ctxt.CanUsePlugins())
            {
                {
                    var lib__prev1 = lib;

                    foreach (var (_, __lib) in ctxt.Library)
                    {
                        lib = __lib;
                        if (lib.Shlib == "")
                        {
                            genhash(_addr_ctxt, _addr_lib);
                        }

                    }

                    lib = lib__prev1;
                }
            }

            if (ctxt.Arch == sys.Arch386 && ctxt.HeadType != objabi.Hwindows)
            {
                if ((ctxt.BuildMode == BuildModeCArchive && ctxt.IsELF) || ctxt.BuildMode == BuildModeCShared || ctxt.BuildMode == BuildModePIE || ctxt.DynlinkingGo())
                {
                    var got = ctxt.loader.LookupOrCreateSym("_GLOBAL_OFFSET_TABLE_", 0L);
                    sb = ctxt.loader.MakeSymbolUpdater(got);
                    sb.SetType(sym.SDYNIMPORT);
                    ctxt.loader.SetAttrReachable(got, true);
                }

            } 

            // DWARF-gen and other phases require that the unit Textp2 slices
            // be populated, so that it can walk the functions in each unit.
            // Call into the loader to do this (requires that we collect the
            // set of internal libraries first). NB: might be simpler if we
            // moved isRuntimeDepPkg to cmd/internal and then did the test in
            // loader.AssignTextSymbolOrder.
            ctxt.Library = postorder(ctxt.Library);
            bool intlibs = new slice<bool>(new bool[] {  });
            {
                var lib__prev1 = lib;

                foreach (var (_, __lib) in ctxt.Library)
                {
                    lib = __lib;
                    intlibs = append(intlibs, isRuntimeDepPkg(lib.Pkg));
                }

                lib = lib__prev1;
            }

            ctxt.Textp2 = ctxt.loader.AssignTextSymbolOrder(ctxt.Library, intlibs, ctxt.Textp2);

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

            var ldr = ctxt.loader;
            for (var s = loader.Sym(1L); s < loader.Sym(ldr.NSym()); s++)
            {
                if (!ldr.AttrReachable(s))
                {
                    continue;
                }

                var name = ldr.SymName(s);
                var newName = typeSymbolMangle(name);
                if (newName != name)
                {
                    ldr.SetSymExtname(s, newName); 

                    // When linking against a shared library, the Go object file may
                    // have reference to the original symbol name whereas the shared
                    // library provides a symbol with the mangled name. We need to
                    // copy the payload of mangled to original.
                    // XXX maybe there is a better way to do this.
                    var dup = ldr.Lookup(newName, ldr.SymVersion(s));
                    if (dup != 0L)
                    {
                        var st = ldr.SymType(s);
                        var dt = ldr.SymType(dup);
                        if (st == sym.Sxxx && dt != sym.Sxxx)
                        {
                            ldr.CopySym(dup, s);
                        }

                    }

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
                    ctxt.Out.Close();
                    os.RemoveAll(flagTmpdir.val);
                });

            } 

            // change our output to temporary object file
            {
                var err__prev1 = err;

                var err = ctxt.Out.Close();

                if (err != null)
                {
                    Exitf("error closing output file");
                }

                err = err__prev1;

            }

            mayberemoveoutfile();

            var p = filepath.Join(flagTmpdir.val, "go.o");
            {
                var err__prev1 = err;

                err = ctxt.Out.Open(p);

                if (err != null)
                {
                    Exitf("cannot create %s: %v", p, err);
                }

                err = err__prev1;

            }

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
            {
                var err = ctxt.Out.Close();

                if (err != null)
                {
                    Exitf("error closing %v", flagOutfile.val);
                }

            }


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

                        @string altLinker = default;
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

                if (objabi.GOOS == "android")
                { 
                    // Use lld to avoid errors from default linker (issue #38838)
                    altLinker = "lld";

                }

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
                    altLinker = "gold"; 

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
                altLinker = "bfd"; 

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

            if (altLinker != "")
            {
                argv = append(argv, "-fuse-ld=" + altLinker);
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

            if (ctxt.compressDWARF && linkerFlagSupported(argv[0L], altLinker, compressDWARF))
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
                    if (linkerFlagSupported(argv[0L], altLinker, nopie))
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

        private static bool linkerFlagSupported(@string linker, @string altLinker, @string flag)
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
            if (altLinker != "")
            {
                flags = append(flags, "-fuse-ld=" + altLinker);
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
                Action<ptr<Link>, ptr<bio.Reader>, @string, long, @string> ldelf = (ctxt, f, pkg, length, pn) =>
                {
                    var (textp, flags, err) = loadelf.Load(ctxt.loader, ctxt.Arch, ctxt.Syms.IncVersion(), f, pkg, length, pn, ehdr.flags);
                    if (err != null)
                    {
                        Errorf(null, "%v", err);
                        return ;
                    }

                    ehdr.flags = flags;
                    ctxt.Textp2 = append(ctxt.Textp2, textp);

                }
;
                return _addr_ldhostobj(ldelf, ctxt.HeadType, _addr_f, pkg, length, pn, file)!;

            }

            if (magic & ~1L == 0xfeedfaceUL || magic & ~0x01000000UL == 0xcefaedfeUL)
            {
                Action<ptr<Link>, ptr<bio.Reader>, @string, long, @string> ldmacho = (ctxt, f, pkg, length, pn) =>
                {
                    var (textp, err) = loadmacho.Load(ctxt.loader, ctxt.Arch, ctxt.Syms.IncVersion(), f, pkg, length, pn);
                    if (err != null)
                    {
                        Errorf(null, "%v", err);
                        return ;
                    }

                    ctxt.Textp2 = append(ctxt.Textp2, textp);

                }
;
                return _addr_ldhostobj(ldmacho, ctxt.HeadType, _addr_f, pkg, length, pn, file)!;

            }

            if (c1 == 0x4cUL && c2 == 0x01UL || c1 == 0x64UL && c2 == 0x86UL)
            {
                Action<ptr<Link>, ptr<bio.Reader>, @string, long, @string> ldpe = (ctxt, f, pkg, length, pn) =>
                {
                    var (textp, rsrc, err) = loadpe.Load(ctxt.loader, ctxt.Arch, ctxt.Syms.IncVersion(), f, pkg, length, pn);
                    if (err != null)
                    {
                        Errorf(null, "%v", err);
                        return ;
                    }

                    if (rsrc != 0L)
                    {
                        setpersrc(ctxt, rsrc);
                    }

                    ctxt.Textp2 = append(ctxt.Textp2, textp);

                }
;
                return _addr_ldhostobj(ldpe, ctxt.HeadType, _addr_f, pkg, length, pn, file)!;

            }

            if (c1 == 0x01UL && (c2 == 0xD7UL || c2 == 0xF7UL))
            {
                Action<ptr<Link>, ptr<bio.Reader>, @string, long, @string> ldxcoff = (ctxt, f, pkg, length, pn) =>
                {
                    (textp, err) = loadxcoff.Load(ctxt.loader, ctxt.Arch, ctxt.Syms.IncVersion(), f, pkg, length, pn);
                    if (err != null)
                    {
                        Errorf(null, "%v", err);
                        return ;
                    }

                    ctxt.Textp2 = append(ctxt.Textp2, textp);

                }
;
                return _addr_ldhostobj(ldxcoff, ctxt.HeadType, _addr_f, pkg, length, pn, file)!;

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

            var fingerprint = ctxt.loader.Preload(ctxt.Syms, f, lib, unit, eof - f.Offset());
            if (!fingerprint.IsZero())
            { // Assembly objects don't have fingerprints. Ignore them.
                // Check fingerprint, to ensure the importing and imported packages
                // have consistent view of symbol indices.
                // Normally the go command should ensure this. But in case something
                // goes wrong, it could lead to obscure bugs like run-time crash.
                // Check it here to be sure.
                if (lib.Fingerprint.IsZero())
                { // Not yet imported. Update its fingerprint.
                    lib.Fingerprint = fingerprint;

                }

                checkFingerprint(_addr_lib, fingerprint, lib.Srcref, lib.Fingerprint);

            }

            addImports(ctxt, lib, pn);
            return _addr_null!;

        }

        private static void checkFingerprint(ptr<sym.Library> _addr_lib, goobj2.FingerprintType libfp, @string src, goobj2.FingerprintType srcfp)
        {
            ref sym.Library lib = ref _addr_lib.val;

            if (libfp != srcfp)
            {
                Exitf("fingerprint mismatch: %s has %x, import from %s expecting %x", lib, libfp, src, srcfp);
            }

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

            var gcdataLocations = make_map<ulong, loader.Sym>();
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

                var l = ctxt.loader;
                var s = l.LookupOrCreateSym(elfsym.Name, ver); 

                // Because loadlib above loads all .a files before loading
                // any shared libraries, any non-dynimport symbols we find
                // that duplicate symbols already loaded should be ignored
                // (the symbols from the .a files "win").
                if (l.SymType(s) != 0L && l.SymType(s) != sym.SDYNIMPORT)
                {
                    continue;
                }

                var su = l.MakeSymbolUpdater(s);
                su.SetType(sym.SDYNIMPORT);
                l.SetSymElfType(s, elf.ST_TYPE(elfsym.Info));
                su.SetSize(int64(elfsym.Size));
                if (elfsym.Section != elf.SHN_UNDEF)
                { 
                    // Set .File for the library that actually defines the symbol.
                    l.SetSymPkg(s, libpath); 

                    // The decodetype_* functions in decodetype.go need access to
                    // the type data.
                    var sname = l.SymName(s);
                    if (strings.HasPrefix(sname, "type.") && !strings.HasPrefix(sname, "type.."))
                    {
                        su.SetData(readelfsymboldata(_addr_ctxt, _addr_f, _addr_elfsym));
                        gcdataLocations[elfsym.Value + 2L * uint64(ctxt.Arch.PtrSize) + 8L + 1L * uint64(ctxt.Arch.PtrSize)] = s;
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
                    var alias = ctxt.loader.LookupOrCreateSym(elfsym.Name, sym.SymVerABIInternal);
                    if (l.SymType(alias) != 0L)
                    {
                        continue;
                    }

                    su = l.MakeSymbolUpdater(alias);
                    su.SetType(sym.SABIALIAS);
                    su.AddReloc(new loader.Reloc(Sym:s));

                }

            }
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

                        }


                    }

                }

            }

            ctxt.Shlibs = append(ctxt.Shlibs, new Shlib(Path:libpath,Hash:hash,Deps:deps,File:f));

        }

        private static ptr<sym.Section> addsection(ptr<loader.Loader> _addr_ldr, ptr<sys.Arch> _addr_arch, ptr<sym.Segment> _addr_seg, @string name, long rwx)
        {
            ref loader.Loader ldr = ref _addr_ldr.val;
            ref sys.Arch arch = ref _addr_arch.val;
            ref sym.Segment seg = ref _addr_seg.val;

            var sect = ldr.NewSection();
            sect.Rwx = uint8(rwx);
            sect.Name = name;
            sect.Seg = seg;
            sect.Align = int32(arch.PtrSize); // everything is at least pointer-aligned
            seg.Sections = append(seg.Sections, sect);
            return _addr_sect!;

        }

        private partial struct chain
        {
            public loader.Sym sym;
            public ptr<chain> up;
            public long limit; // limit on entry to sym
        }

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

        private partial struct stkChk
        {
            public ptr<loader.Loader> ldr;
            public ptr<Link> ctxt;
            public loader.Sym morestack;
            public loader.Bitmap done;
        }

        // Walk the call tree and check that there is always enough stack space
        // for the call frames, especially for a chain of nosplit functions.
        private static void dostkcheck(this ptr<Link> _addr_ctxt)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            var ldr = ctxt.loader;
            stkChk sc = new stkChk(ldr:ldr,ctxt:ctxt,morestack:ldr.Lookup("runtime.morestack",0),done:loader.MakeBitmap(ldr.NSym()),); 

            // Every splitting function ensures that there are at least StackLimit
            // bytes available below SP when the splitting prologue finishes.
            // If the splitting function calls F, then F begins execution with
            // at least StackLimit - callsize() bytes available.
            // Check that every function behaves correctly with this amount
            // of stack, following direct calls in order to piece together chains
            // of non-splitting functions.
            ref chain ch = ref heap(out ptr<chain> _addr_ch);
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

                foreach (var (_, __s) in ctxt.Textp2)
                {
                    s = __s;
                    if (ldr.IsNoSplit(s))
                    {
                        ch.sym = s;
                        sc.check(_addr_ch, 0L);
                    }

                }

                s = s__prev1;
            }

            {
                var s__prev1 = s;

                foreach (var (_, __s) in ctxt.Textp2)
                {
                    s = __s;
                    if (!ldr.IsNoSplit(s))
                    {
                        ch.sym = s;
                        sc.check(_addr_ch, 0L);
                    }

                }

                s = s__prev1;
            }
        }

        private static long check(this ptr<stkChk> _addr_sc, ptr<chain> _addr_up, long depth)
        {
            ref stkChk sc = ref _addr_sc.val;
            ref chain up = ref _addr_up.val;

            var limit = up.limit;
            var s = up.sym;
            var ldr = sc.ldr;
            var ctxt = sc.ctxt; 

            // Don't duplicate work: only need to consider each
            // function at top of safe zone once.
            var top = limit == objabi.StackLimit - callsize(_addr_ctxt);
            if (top)
            {
                if (sc.done.Has(s))
                {
                    return 0L;
                }

                sc.done.Set(s);

            }

            if (depth > 500L)
            {
                sc.ctxt.Errorf(s, "nosplit stack check too deep");
                sc.broke(up, 0L);
                return -1L;
            }

            if (ldr.AttrExternal(s))
            { 
                // external function.
                // should never be called directly.
                // onlyctxt.Diagnose the direct caller.
                // TODO(mwhudson): actually think about this.
                // TODO(khr): disabled for now. Calls to external functions can only happen on the g0 stack.
                // See the trampolines in src/runtime/sys_darwin_$ARCH.go.
                //if depth == 1 && ldr.SymType(s) != sym.SXREF && !ctxt.DynlinkingGo() &&
                //    ctxt.BuildMode != BuildModeCArchive && ctxt.BuildMode != BuildModePIE && ctxt.BuildMode != BuildModeCShared && ctxt.BuildMode != BuildModePlugin {
                //    Errorf(s, "call to external function")
                //}
                return -1L;

            }

            var info = ldr.FuncInfo(s);
            if (!info.Valid())
            { // external function. see above.
                return -1L;

            }

            if (limit < 0L)
            {
                sc.broke(up, limit);
                return -1L;
            } 

            // morestack looks like it calls functions,
            // but it switches the stack pointer first.
            if (s == sc.morestack)
            {
                return 0L;
            }

            ref chain ch = ref heap(out ptr<chain> _addr_ch);
            ch.up = up;

            if (!ldr.IsNoSplit(s))
            { 
                // Ensure we have enough stack to call morestack.
                ch.limit = limit - callsize(_addr_ctxt);
                ch.sym = sc.morestack;
                if (sc.check(_addr_ch, depth + 1L) < 0L)
                {
                    return -1L;
                }

                if (!top)
                {
                    return 0L;
                } 
                // Raise limit to allow frame.
                var locals = info.Locals();
                limit = objabi.StackLimit + int(locals) + int(ctxt.FixedFrameSize());

            } 

            // Walk through sp adjustments in function, consuming relocs.
            var relocs = ldr.Relocs(s);
            ref chain ch1 = ref heap(out ptr<chain> _addr_ch1);
            var pcsp = obj.NewPCIter(uint32(ctxt.Arch.MinLC));
            long ri = 0L;
            pcsp.Init(info.Pcsp());

            while (!pcsp.Done)
            { 
                // pcsp.value is in effect for [pcsp.pc, pcsp.nextpc).

                // Check stack size in effect for this span.
                if (int32(limit) - pcsp.Value < 0L)
                {
                    sc.broke(up, int(int32(limit) - pcsp.Value));
                    return -1L;
                pcsp.Next();
                } 

                // Process calls in this span.
                while (ri < relocs.Count())
                {
                    var r = relocs.At2(ri);
                    if (uint32(r.Off()) >= pcsp.NextPC)
                    {
                        break;
                    ri++;
                    }

                    var t = r.Type();

                    if (t.IsDirectCall()) 
                        ch.limit = int(int32(limit) - pcsp.Value - int32(callsize(_addr_ctxt)));
                        ch.sym = r.Sym();
                        if (sc.check(_addr_ch, depth + 1L) < 0L)
                        {
                            return -1L;
                        } 

                        // Indirect call. Assume it is a call to a splitting function,
                        // so we have to make sure it can call morestack.
                        // Arrange the data structures to report both calls, so that
                        // if there is an error, stkprint shows all the steps involved.
                    else if (t == objabi.R_CALLIND) 
                        ch.limit = int(int32(limit) - pcsp.Value - int32(callsize(_addr_ctxt)));
                        ch.sym = 0L;
                        ch1.limit = ch.limit - callsize(_addr_ctxt); // for morestack in called prologue
                        _addr_ch1.up = _addr_ch;
                        ch1.up = ref _addr_ch1.up.val;
                        ch1.sym = sc.morestack;
                        if (sc.check(_addr_ch1, depth + 2L) < 0L)
                        {
                            return -1L;
                        }

                                    }


            }


            return 0L;

        }

        private static void broke(this ptr<stkChk> _addr_sc, ptr<chain> _addr_ch, long limit)
        {
            ref stkChk sc = ref _addr_sc.val;
            ref chain ch = ref _addr_ch.val;

            sc.ctxt.Errorf(ch.sym, "nosplit stack overflow");
            sc.print(ch, limit);
        }

        private static void print(this ptr<stkChk> _addr_sc, ptr<chain> _addr_ch, long limit)
        {
            ref stkChk sc = ref _addr_sc.val;
            ref chain ch = ref _addr_ch.val;

            var ldr = sc.ldr;
            var ctxt = sc.ctxt;
            @string name = default;
            if (ch.sym != 0L)
            {
                name = ldr.SymName(ch.sym);
                if (ldr.IsNoSplit(ch.sym))
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
                // top of chain. ch.sym != 0.
                if (ldr.IsNoSplit(ch.sym))
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
                sc.print(ch.up, ch.limit + callsize(_addr_ctxt));
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


        private static void genasmsym(ptr<Link> _addr_ctxt, Action<ptr<Link>, ptr<sym.Symbol>, @string, SymbolType, long> put)
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
                    put(ctxt, s, s.Name, TextSym, s.Value);
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
                    put(ctxt, s, s.Name, TextSym, s.Value);
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
                    put(ctxt, s, s.Name, TextSym, s.Value);
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

                foreach (var (_, __s) in ctxt.loader.Syms)
                {
                    s = __s;
                    if (s == null)
                    {
                        continue;
                    }

                    if (!shouldBeInSymbolTable(s))
                    {
                        continue;
                    }


                    if (s.Type == sym.SCONST || s.Type == sym.SRODATA || s.Type == sym.SSYMTAB || s.Type == sym.SPCLNTAB || s.Type == sym.SINITARR || s.Type == sym.SDATA || s.Type == sym.SNOPTRDATA || s.Type == sym.SELFROSECT || s.Type == sym.SMACHOGOT || s.Type == sym.STYPE || s.Type == sym.SSTRING || s.Type == sym.SGOSTRING || s.Type == sym.SGOFUNC || s.Type == sym.SGCBITS || s.Type == sym.STYPERELRO || s.Type == sym.SSTRINGRELRO || s.Type == sym.SGOSTRINGRELRO || s.Type == sym.SGOFUNCRELRO || s.Type == sym.SGCBITSRELRO || s.Type == sym.SRODATARELRO || s.Type == sym.STYPELINK || s.Type == sym.SITABLINK || s.Type == sym.SWINDOWS) 
                        if (!s.Attr.Reachable())
                        {
                            continue;
                        }

                        put(ctxt, s, s.Name, DataSym, Symaddr(_addr_s));
                    else if (s.Type == sym.SBSS || s.Type == sym.SNOPTRBSS || s.Type == sym.SLIBFUZZER_EXTRA_COUNTER) 
                        if (!s.Attr.Reachable())
                        {
                            continue;
                        }

                        if (len(s.P) > 0L)
                        {
                            Errorf(s, "should not be bss (size=%d type=%v special=%v)", len(s.P), s.Type, s.Attr.Special());
                        }

                        put(ctxt, s, s.Name, BSSSym, Symaddr(_addr_s));
                    else if (s.Type == sym.SUNDEFEXT) 
                        if (ctxt.HeadType == objabi.Hwindows || ctxt.HeadType == objabi.Haix || ctxt.IsELF)
                        {
                            put(ctxt, s, s.Name, UndefinedSym, s.Value);
                        }

                    else if (s.Type == sym.SHOSTOBJ) 
                        if (!s.Attr.Reachable())
                        {
                            continue;
                        }

                        if (ctxt.HeadType == objabi.Hwindows || ctxt.IsELF)
                        {
                            put(ctxt, s, s.Name, UndefinedSym, s.Value);
                        }

                    else if (s.Type == sym.SDYNIMPORT) 
                        if (!s.Attr.Reachable())
                        {
                            continue;
                        }

                        put(ctxt, s, s.Extname(), UndefinedSym, 0L);
                    else if (s.Type == sym.STLSBSS) 
                        if (ctxt.LinkMode == LinkExternal)
                        {
                            put(ctxt, s, s.Name, TLSSym, Symaddr(_addr_s));
                        }

                                    }

                s = s__prev1;
            }

            {
                var s__prev1 = s;

                foreach (var (_, __s) in ctxt.Textp)
                {
                    s = __s;
                    put(ctxt, s, s.Name, TextSym, s.Value);
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

        private static void xdefine2(this ptr<Link> _addr_ctxt, @string p, sym.SymKind t, long v)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            var ldr = ctxt.loader;
            var s = ldr.CreateSymForUpdate(p, 0L);
            s.SetType(t);
            s.SetValue(v);
            s.SetReachable(true);
            s.SetSpecial(true);
            s.SetLocal(true);
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

                foreach (var (_, __s) in ctxt.datap)
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

            var ldr = ctxt.loader;
            foreach (var (_, s) in ctxt.Textp2)
            {
                var relocs = ldr.Relocs(s);
                for (long i = 0L; i < relocs.Count(); i++)
                {
                    var r = relocs.At2(i);
                    var rs = r.Sym();
                    if (rs == 0L)
                    {
                        continue;
                    }

                    if (r.Type().IsDirectCall() && (ldr.SymType(rs) == sym.STEXT || ldr.SymType(rs) == sym.SABIALIAS))
                    {
                        ctxt.Logf("%s calls %s\n", ldr.SymName(s), ldr.SymName(rs));
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

        // addToTextp populates the context Textp slice.
        private static void addToTextp(ptr<Link> _addr_ctxt) => func((_, panic, __) =>
        {
            ref Link ctxt = ref _addr_ctxt.val;
 
            // Set up ctxt.Textp, based on ctxt.Textp2.
            var textp = make_slice<ptr<sym.Symbol>>(0L, len(ctxt.Textp2));
            var haveshlibs = len(ctxt.Shlibs) > 0L;
            foreach (var (_, tsym) in ctxt.Textp2)
            {
                var sp = ctxt.loader.Syms[tsym];
                if (sp == null || !ctxt.loader.AttrReachable(tsym))
                {
                    panic("should never happen");
                }

                if (haveshlibs && sp.Type == sym.SDYNIMPORT)
                {
                    continue;
                }

                textp = append(textp, sp);

            }
            ctxt.Textp = textp;

        });

        private static void loadlibfull(this ptr<Link> _addr_ctxt, slice<sym.SymKind> symGroupType, bool needReloc, bool needExtReloc) => func((_, panic, __) =>
        {
            ref Link ctxt = ref _addr_ctxt.val;
 
            // Load full symbol contents, resolve indexed references.
            ctxt.loader.LoadFull(ctxt.Arch, ctxt.Syms, needReloc, needExtReloc); 

            // Convert ctxt.Moduledata2 to ctxt.Moduledata, etc
            if (ctxt.Moduledata2 != 0L)
            {
                ctxt.Moduledata = ctxt.loader.Syms[ctxt.Moduledata2];
                ctxt.Tlsg = ctxt.loader.Syms[ctxt.Tlsg2];
            } 

            // Pull the symbols out.
            ctxt.loader.ExtractSymbols(ctxt.Syms);
            ctxt.lookup = ctxt.Syms.ROLookup; 

            // Recreate dynexp using *sym.Symbol instead of loader.Sym
            genSymsForDynexp(_addr_ctxt); 

            // Drop the cgodata reference.
            ctxt.cgodata = null;

            addToTextp(_addr_ctxt); 

            // Set special global symbols.
            ctxt.setArchSyms(AfterLoadlibFull); 

            // Populate dwarfp from dwarfp2. If we see a symbol index
            // whose loader.Syms entry is nil, something went wrong.
            foreach (var (_, si) in dwarfp2)
            {
                var syms = make_slice<ptr<sym.Symbol>>(0L, len(si.syms));
                {
                    var symIdx__prev2 = symIdx;

                    foreach (var (_, __symIdx) in si.syms)
                    {
                        symIdx = __symIdx;
                        var s = ctxt.loader.Syms[symIdx];
                        if (s == null)
                        {
                            panic(fmt.Sprintf("nil sym for dwarfp2 element %d", symIdx));
                        }

                        s.Attr |= sym.AttrLocal;
                        syms = append(syms, s);

                    }

                    symIdx = symIdx__prev2;
                }

                dwarfp = append(dwarfp, new dwarfSecInfo2(syms:syms));

            } 

            // Populate datap from datap2
            ctxt.datap = make_slice<ptr<sym.Symbol>>(len(ctxt.datap2));
            {
                var i__prev1 = i;
                var symIdx__prev1 = symIdx;

                foreach (var (__i, __symIdx) in ctxt.datap2)
                {
                    i = __i;
                    symIdx = __symIdx;
                    s = ctxt.loader.Syms[symIdx];
                    if (s == null)
                    {
                        panic(fmt.Sprintf("nil sym for datap2 element %d", symIdx));
                    }

                    ctxt.datap[i] = s;

                } 

                // Populate the sym.Section 'Sym' fields based on their 'Sym2'
                // fields.

                i = i__prev1;
                symIdx = symIdx__prev1;
            }

            ptr<sym.Segment> allSegments = new slice<ptr<sym.Segment>>(new ptr<sym.Segment>[] { &Segtext, &Segrodata, &Segrelrodata, &Segdata, &Segdwarf });
            foreach (var (_, seg) in allSegments)
            {
                foreach (var (_, sect) in seg.Sections)
                {
                    if (sect.Sym2 != 0L)
                    {
                        s = ctxt.loader.Syms[sect.Sym2];
                        if (s == null)
                        {
                            panic(fmt.Sprintf("nil sym for sect %s sym %d", sect.Name, sect.Sym2));
                        }

                        sect.Sym = s;

                    }

                }

            } 

            // For now, overwrite symbol type with its "group" type, as dodata
            // expected. Once we converted dodata, this will probably not be
            // needed.
            {
                var i__prev1 = i;

                foreach (var (__i, __t) in symGroupType)
                {
                    i = __i;
                    t = __t;
                    if (t != sym.Sxxx)
                    {
                        s = ctxt.loader.Syms[i];
                        if (s == null)
                        {
                            continue; // in dwarfcompress we drop compressed DWARF symbols
                        }

                        s.Type = t;

                    }

                }

                i = i__prev1;
            }

            symGroupType = null;

            if (ctxt.Debugvlog > 1L)
            { 
                // loadlibfull is likely a good place to dump.
                // Only dump under -v=2 and above.
                ctxt.dumpsyms();

            }

        });

        private static @string symPkg(ptr<Link> _addr_ctxt, ptr<sym.Symbol> _addr_s)
        {
            ref Link ctxt = ref _addr_ctxt.val;
            ref sym.Symbol s = ref _addr_s.val;

            if (s == null)
            {
                return "";
            }

            return ctxt.loader.SymPkg(loader.Sym(s.SymIdx));

        }

        public static int ElfSymForReloc(ptr<Link> _addr_ctxt, ptr<sym.Symbol> _addr_s)
        {
            ref Link ctxt = ref _addr_ctxt.val;
            ref sym.Symbol s = ref _addr_s.val;
 
            // If putelfsym created a local version of this symbol, use that in all
            // relocations.
            var les = ctxt.loader.SymLocalElfSym(loader.Sym(s.SymIdx));
            if (les != 0L)
            {
                return les;
            }
            else
            {
                return ctxt.loader.SymElfSym(loader.Sym(s.SymIdx));
            }

        }

        private static ptr<sym.Symbol> symSub(ptr<Link> _addr_ctxt, ptr<sym.Symbol> _addr_s)
        {
            ref Link ctxt = ref _addr_ctxt.val;
            ref sym.Symbol s = ref _addr_s.val;

            {
                var lsub = ctxt.loader.SubSym(loader.Sym(s.SymIdx));

                if (lsub != 0L)
                {
                    return _addr_ctxt.loader.Syms[lsub]!;
                }

            }

            return _addr_null!;

        }

        private static void dumpsyms(this ptr<Link> _addr_ctxt)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            foreach (var (_, s) in ctxt.loader.Syms)
            {
                if (s == null)
                {
                    continue;
                }

                fmt.Printf("%s %s reachable=%v onlist=%v outer=%v sub=%v\n", s, s.Type, s.Attr.Reachable(), s.Attr.OnList(), s.Outer, symSub(_addr_ctxt, _addr_s));
                foreach (var (i) in s.R)
                {
                    fmt.Println("\t", s.R[i].Type, s.R[i].Sym);
                }

            }

        }
    }
}}}}
