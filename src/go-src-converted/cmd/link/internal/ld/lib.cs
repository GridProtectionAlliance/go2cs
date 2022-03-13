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

// package ld -- go2cs converted at 2022 March 13 06:34:36 UTC
// import "cmd/link/internal/ld" ==> using ld = go.cmd.link.@internal.ld_package
// Original source: C:\Program Files\Go\src\cmd\link\internal\ld\lib.go
namespace go.cmd.link.@internal;

using bytes = bytes_package;
using bio = cmd.@internal.bio_package;
using goobj = cmd.@internal.goobj_package;
using obj = cmd.@internal.obj_package;
using objabi = cmd.@internal.objabi_package;
using sys = cmd.@internal.sys_package;
using loadelf = cmd.link.@internal.loadelf_package;
using loader = cmd.link.@internal.loader_package;
using loadmacho = cmd.link.@internal.loadmacho_package;
using loadpe = cmd.link.@internal.loadpe_package;
using loadxcoff = cmd.link.@internal.loadxcoff_package;
using sym = cmd.link.@internal.sym_package;
using sha1 = crypto.sha1_package;
using elf = debug.elf_package;
using macho = debug.macho_package;
using base64 = encoding.base64_package;
using binary = encoding.binary_package;
using fmt = fmt_package;
using buildcfg = @internal.buildcfg_package;
using exec = @internal.execabs_package;
using io = io_package;
using ioutil = io.ioutil_package;
using log = log_package;
using os = os_package;
using filepath = path.filepath_package;
using runtime = runtime_package;
using strings = strings_package;
using sync = sync_package;


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

// ArchSyms holds a number of architecture specific symbols used during
// relocation.  Rather than allowing them universal access to all symbols,
// we keep a subset for relocation application.

using System;
using System.Threading;
public static partial class ld_package {

public partial struct ArchSyms {
    public loader.Sym Rel;
    public loader.Sym Rela;
    public loader.Sym RelPLT;
    public loader.Sym RelaPLT;
    public loader.Sym LinkEditGOT;
    public loader.Sym LinkEditPLT;
    public loader.Sym TOC;
    public slice<loader.Sym> DotTOC; // for each version

    public loader.Sym GOT;
    public loader.Sym PLT;
    public loader.Sym GOTPLT;
    public loader.Sym Tlsg;
    public nint Tlsoffset;
    public loader.Sym Dynamic;
    public loader.Sym DynSym;
    public loader.Sym DynStr;
    public loader.Sym unreachableMethod;
}

// mkArchSym is a helper for setArchSyms, to set up a special symbol.
private static void mkArchSym(this ptr<Link> _addr_ctxt, @string name, nint ver, ptr<loader.Sym> _addr_ls) {
    ref Link ctxt = ref _addr_ctxt.val;
    ref loader.Sym ls = ref _addr_ls.val;

    ls = ctxt.loader.LookupOrCreateSym(name, ver);
    ctxt.loader.SetAttrReachable(ls, true);
}

// mkArchVecSym is similar to  setArchSyms, but operates on elements within
// a slice, where each element corresponds to some symbol version.
private static void mkArchSymVec(this ptr<Link> _addr_ctxt, @string name, nint ver, slice<loader.Sym> ls) {
    ref Link ctxt = ref _addr_ctxt.val;

    ls[ver] = ctxt.loader.LookupOrCreateSym(name, ver);
    ctxt.loader.SetAttrReachable(ls[ver], true);
}

// setArchSyms sets up the ArchSyms structure, and must be called before
// relocations are applied.
private static void setArchSyms(this ptr<Link> _addr_ctxt) {
    ref Link ctxt = ref _addr_ctxt.val;

    ctxt.mkArchSym(".got", 0, _addr_ctxt.GOT);
    ctxt.mkArchSym(".plt", 0, _addr_ctxt.PLT);
    ctxt.mkArchSym(".got.plt", 0, _addr_ctxt.GOTPLT);
    ctxt.mkArchSym(".dynamic", 0, _addr_ctxt.Dynamic);
    ctxt.mkArchSym(".dynsym", 0, _addr_ctxt.DynSym);
    ctxt.mkArchSym(".dynstr", 0, _addr_ctxt.DynStr);
    ctxt.mkArchSym("runtime.unreachableMethod", sym.SymVerABIInternal, _addr_ctxt.unreachableMethod);

    if (ctxt.IsPPC64()) {
        ctxt.mkArchSym("TOC", 0, _addr_ctxt.TOC); 

        // NB: note the +2 below for DotTOC2 compared to the +1 for
        // DocTOC. This is because loadlibfull() creates an additional
        // syms version during conversion of loader.Sym symbols to
        // *sym.Symbol symbols. Symbols that are assigned this final
        // version are not going to have TOC references, so it should
        // be ok for them to inherit an invalid .TOC. symbol.
        // TODO: revisit the +2, now that loadlibfull is gone.
        ctxt.DotTOC = make_slice<loader.Sym>(ctxt.MaxVersion() + 2);
        for (nint i = 0; i <= ctxt.MaxVersion(); i++) {
            if (i >= 2 && i < sym.SymVerStatic) { // these versions are not used currently
                continue;
            }
            ctxt.mkArchSymVec(".TOC.", i, ctxt.DotTOC);
        }
    }
    if (ctxt.IsElf()) {
        ctxt.mkArchSym(".rel", 0, _addr_ctxt.Rel);
        ctxt.mkArchSym(".rela", 0, _addr_ctxt.Rela);
        ctxt.mkArchSym(".rel.plt", 0, _addr_ctxt.RelPLT);
        ctxt.mkArchSym(".rela.plt", 0, _addr_ctxt.RelaPLT);
    }
    if (ctxt.IsDarwin()) {
        ctxt.mkArchSym(".linkedit.got", 0, _addr_ctxt.LinkEditGOT);
        ctxt.mkArchSym(".linkedit.plt", 0, _addr_ctxt.LinkEditPLT);
    }
}

public partial struct Arch {
    public nint Funcalign;
    public nint Maxalign;
    public nint Minalign;
    public nint Dwarfregsp;
    public nint Dwarfreglr; // Threshold of total text size, used for trampoline insertion. If the total
// text size is smaller than TrampLimit, we won't need to insert trampolines.
// It is pretty close to the offset range of a direct CALL machine instruction.
// We leave some room for extra stuff like PLT stubs.
    public ulong TrampLimit;
    public @string Androiddynld;
    public @string Linuxdynld;
    public @string Freebsddynld;
    public @string Netbsddynld;
    public @string Openbsddynld;
    public @string Dragonflydynld;
    public @string Solarisdynld; // Empty spaces between codeblocks will be padded with this value.
// For example an architecture might want to pad with a trap instruction to
// catch wayward programs. Architectures that do not define a padding value
// are padded with zeros.
    public slice<byte> CodePad; // Plan 9 variables.
    public uint Plan9Magic;
    public bool Plan9_64Bit;
    public Func<ptr<Target>, ptr<loader.Loader>, ptr<ArchSyms>, loader.Sym, loader.Reloc, nint, bool> Adddynrel;
    public Action<ptr<Link>> Archinit; // Archreloc is an arch-specific hook that assists in relocation processing
// (invoked by 'relocsym'); it handles target-specific relocation tasks.
// Here "rel" is the current relocation being examined, "sym" is the symbol
// containing the chunk of data to which the relocation applies, and "off"
// is the contents of the to-be-relocated data item (from sym.P). Return
// value is the appropriately relocated value (to be written back to the
// same spot in sym.P), number of external _host_ relocations needed (i.e.
// ELF/Mach-O/etc. relocations, not Go relocations, this must match Elfreloc1,
// etc.), and a boolean indicating success/failure (a failing value indicates
// a fatal error).
    public Func<ptr<Target>, ptr<loader.Loader>, ptr<ArchSyms>, loader.Reloc, loader.Sym, long, (long, nint, bool)> Archreloc; // Archrelocvariant is a second arch-specific hook used for
// relocation processing; it handles relocations where r.Type is
// insufficient to describe the relocation (r.Variant !=
// sym.RV_NONE). Here "rel" is the relocation being applied, "sym"
// is the symbol containing the chunk of data to which the
// relocation applies, and "off" is the contents of the
// to-be-relocated data item (from sym.P). Return is an updated
// offset value.
    public Func<ptr<Target>, ptr<loader.Loader>, loader.Reloc, sym.RelocVariant, loader.Sym, long, slice<byte>, long> Archrelocvariant; // Generate a trampoline for a call from s to rs if necessary. ri is
// index of the relocation.
    public Action<ptr<Link>, ptr<loader.Loader>, nint, loader.Sym, loader.Sym> Trampoline; // Assembling the binary breaks into two phases, writing the code/data/
// dwarf information (which is rather generic), and some more architecture
// specific work like setting up the elf headers/dynamic relocations, etc.
// The phases are called "Asmb" and "Asmb2". Asmb2 needs to be defined for
// every architecture, but only if architecture has an Asmb function will
// it be used for assembly.  Otherwise a generic assembly Asmb function is
// used.
    public Action<ptr<Link>, ptr<loader.Loader>> Asmb;
    public Action<ptr<Link>, ptr<loader.Loader>> Asmb2; // Extreloc is an arch-specific hook that converts a Go relocation to an
// external relocation. Return the external relocation and whether it is
// needed.
    public Func<ptr<Target>, ptr<loader.Loader>, loader.Reloc, loader.Sym, (loader.ExtReloc, bool)> Extreloc;
    public Func<ptr<Link>, ptr<OutBuf>, ptr<loader.Loader>, loader.Sym, loader.ExtReloc, nint, long, bool> Elfreloc1;
    public uint ElfrelocSize; // size of an ELF relocation record, must match Elfreloc1.
    public Action<ptr<Link>, ptr<loader.SymbolBuilder>, ptr<loader.SymbolBuilder>, loader.Sym> Elfsetupplt;
    public Action<ptr<Link>, ptr<loader.Loader>> Gentext; // Generate text before addressing has been performed.
    public Func<ptr<sys.Arch>, ptr<OutBuf>, ptr<loader.Loader>, loader.Sym, loader.ExtReloc, long, bool> Machoreloc1;
    public uint MachorelocSize; // size of an Mach-O relocation record, must match Machoreloc1.
    public Func<ptr<sys.Arch>, ptr<OutBuf>, ptr<loader.Loader>, loader.Sym, loader.ExtReloc, long, bool> PEreloc1;
    public Func<ptr<sys.Arch>, ptr<OutBuf>, ptr<loader.Loader>, loader.Sym, loader.ExtReloc, long, bool> Xcoffreloc1; // Generate additional symbols for the native symbol table just prior to
// code generation.
    public Action<ptr<Link>, ptr<loader.Loader>> GenSymsLate; // TLSIEtoLE converts a TLS Initial Executable relocation to
// a TLS Local Executable relocation.
//
// This is possible when a TLS IE relocation refers to a local
// symbol in an executable, which is typical when internally
// linking PIE binaries.
    public Action<slice<byte>, nint, nint> TLSIEtoLE; // optional override for assignAddress
    public Func<ptr<loader.Loader>, ptr<sym.Section>, nint, loader.Sym, ulong, bool, (ptr<sym.Section>, nint, ulong)> AssignAddress;
}

private static Arch thearch = default;private static int lcSize = default;private static Rpath rpath = default;private static int spSize = default;private static int symSize = default;

public static readonly nint MINFUNC = 16; // minimum size for a function

// DynlinkingGo reports whether we are producing Go code that can live
// in separate shared libraries linked together at runtime.
private static bool DynlinkingGo(this ptr<Link> _addr_ctxt) => func((_, panic, _) => {
    ref Link ctxt = ref _addr_ctxt.val;

    if (!ctxt.Loaded) {
        panic("DynlinkingGo called before all symbols loaded");
    }
    return ctxt.BuildMode == BuildModeShared || ctxt.linkShared || ctxt.BuildMode == BuildModePlugin || ctxt.canUsePlugins;
});

// CanUsePlugins reports whether a plugins can be used
private static bool CanUsePlugins(this ptr<Link> _addr_ctxt) => func((_, panic, _) => {
    ref Link ctxt = ref _addr_ctxt.val;

    if (!ctxt.Loaded) {
        panic("CanUsePlugins called before all symbols loaded");
    }
    return ctxt.canUsePlugins;
});

// NeedCodeSign reports whether we need to code-sign the output binary.
private static bool NeedCodeSign(this ptr<Link> _addr_ctxt) {
    ref Link ctxt = ref _addr_ctxt.val;

    return ctxt.IsDarwin() && ctxt.IsARM64();
}

private static slice<@string> dynlib = default;private static slice<@string> ldflag = default;private static nint havedynamic = default;public static nint Funcalign = default;private static bool iscgo = default;private static nint elfglobalsymndx = default;private static @string interpreter = default;private static bool debug_s = default;public static int HEADR = default;private static nint nerrors = default;private static long liveness = default;private static nint checkStrictDups = default;private static nint strictDupMsgCount = default;

public static sym.Segment Segtext = default;public static sym.Segment Segrodata = default;public static sym.Segment Segrelrodata = default;public static sym.Segment Segdata = default;public static sym.Segment Segdwarf = default;public static ptr<sym.Segment> Segments = new slice<ptr<sym.Segment>>(new ptr<sym.Segment>[] { &Segtext, &Segrodata, &Segrelrodata, &Segdata, &Segdwarf });

private static readonly @string pkgdef = "__.PKGDEF";



 
// externalobj is set to true if we see an object compiled by
// the host compiler that is not from a package that is known
// to support internal linking mode.
private static var externalobj = false;private static var unknownObjFormat = false;private static @string theline = default;

public static void Lflag(ptr<Link> _addr_ctxt, @string arg) {
    ref Link ctxt = ref _addr_ctxt.val;

    ctxt.Libdir = append(ctxt.Libdir, arg);
}

/*
 * Unix doesn't like it when we write to a running (or, sometimes,
 * recently run) binary, so remove the output file before writing it.
 * On Windows 7, remove() can force a subsequent create() to fail.
 * S_ISREG() does not exist on Plan 9.
 */
private static void mayberemoveoutfile() {
    {
        var (fi, err) = os.Lstat(flagOutfile.val);

        if (err == null && !fi.Mode().IsRegular()) {
            return ;
        }
    }
    os.Remove(flagOutfile.val);
}

private static void libinit(ptr<Link> _addr_ctxt) {
    ref Link ctxt = ref _addr_ctxt.val;

    Funcalign = thearch.Funcalign; 

    // add goroot to the end of the libdir list.
    @string suffix = "";

    @string suffixsep = "";
    if (flagInstallSuffix != "".val) {
        suffixsep = "_";
        suffix = flagInstallSuffix.val;
    }
    else if (flagRace.val) {
        suffixsep = "_";
        suffix = "race";
    }
    else if (flagMsan.val) {
        suffixsep = "_";
        suffix = "msan";
    }
    Lflag(_addr_ctxt, filepath.Join(buildcfg.GOROOT, "pkg", fmt.Sprintf("%s_%s%s%s", buildcfg.GOOS, buildcfg.GOARCH, suffixsep, suffix)));

    mayberemoveoutfile();

    {
        var err = ctxt.Out.Open(flagOutfile.val);

        if (err != null) {
            Exitf("cannot create %s: %v", flagOutfile.val, err);
        }
    }

    if (flagEntrySymbol == "".val) {

        if (ctxt.BuildMode == BuildModeCShared || ctxt.BuildMode == BuildModeCArchive) 
            flagEntrySymbol.val = fmt.Sprintf("_rt0_%s_%s_lib", buildcfg.GOARCH, buildcfg.GOOS);
        else if (ctxt.BuildMode == BuildModeExe || ctxt.BuildMode == BuildModePIE) 
            flagEntrySymbol.val = fmt.Sprintf("_rt0_%s_%s", buildcfg.GOARCH, buildcfg.GOOS);
        else if (ctxt.BuildMode == BuildModeShared || ctxt.BuildMode == BuildModePlugin)         else 
            Errorf(null, "unknown *flagEntrySymbol for buildmode %v", ctxt.BuildMode);
            }
}

private static void exitIfErrors() {
    if (nerrors != 0 || checkStrictDups > 1 && strictDupMsgCount > 0) {
        mayberemoveoutfile();
        Exit(2);
    }
}

private static void errorexit() {
    exitIfErrors();
    Exit(0);
}

private static ptr<sym.Library> loadinternal(ptr<Link> _addr_ctxt, @string name) {
    ref Link ctxt = ref _addr_ctxt.val;

    goobj.FingerprintType zerofp = new goobj.FingerprintType();
    if (ctxt.linkShared && ctxt.PackageShlib != null) {
        {
            var shlib = ctxt.PackageShlib[name];

            if (shlib != "") {
                return _addr_addlibpath(ctxt, "internal", "internal", "", name, shlib, zerofp)!;
            }

        }
    }
    if (ctxt.PackageFile != null) {
        {
            var pname__prev2 = pname;

            var pname = ctxt.PackageFile[name];

            if (pname != "") {
                return _addr_addlibpath(ctxt, "internal", "internal", pname, name, "", zerofp)!;
            }

            pname = pname__prev2;

        }
        ctxt.Logf("loadinternal: cannot find %s\n", name);
        return _addr_null!;
    }
    foreach (var (_, libdir) in ctxt.Libdir) {
        if (ctxt.linkShared) {
            var shlibname = filepath.Join(libdir, name + ".shlibname");
            if (ctxt.Debugvlog != 0) {
                ctxt.Logf("searching for %s.a in %s\n", name, shlibname);
            }
            {
                var (_, err) = os.Stat(shlibname);

                if (err == null) {
                    return _addr_addlibpath(ctxt, "internal", "internal", "", name, shlibname, zerofp)!;
                }

            }
        }
        pname = filepath.Join(libdir, name + ".a");
        if (ctxt.Debugvlog != 0) {
            ctxt.Logf("searching for %s.a in %s\n", name, pname);
        }
        {
            (_, err) = os.Stat(pname);

            if (err == null) {
                return _addr_addlibpath(ctxt, "internal", "internal", pname, name, "", zerofp)!;
            }

        }
    }    ctxt.Logf("warning: unable to find %s.a\n", name);
    return _addr_null!;
}

// extld returns the current external linker.
private static @string extld(this ptr<Link> _addr_ctxt) {
    ref Link ctxt = ref _addr_ctxt.val;

    if (flagExtld == "".val) {
        flagExtld.val = "gcc";
    }
    return flagExtld.val;
}

// findLibPathCmd uses cmd command to find gcc library libname.
// It returns library full path if found, or "none" if not found.
private static @string findLibPathCmd(this ptr<Link> _addr_ctxt, @string cmd, @string libname) {
    ref Link ctxt = ref _addr_ctxt.val;

    var extld = ctxt.extld();
    var args = hostlinkArchArgs(_addr_ctxt.Arch);
    args = append(args, cmd);
    if (ctxt.Debugvlog != 0) {
        ctxt.Logf("%s %v\n", extld, args);
    }
    var (out, err) = exec.Command(extld, args).Output();
    if (err != null) {
        if (ctxt.Debugvlog != 0) {
            ctxt.Logf("not using a %s file because compiler failed\n%v\n%s\n", libname, err, out);
        }
        return "none";
    }
    return strings.TrimSpace(string(out));
}

// findLibPath searches for library libname.
// It returns library full path if found, or "none" if not found.
private static @string findLibPath(this ptr<Link> _addr_ctxt, @string libname) {
    ref Link ctxt = ref _addr_ctxt.val;

    return ctxt.findLibPathCmd("--print-file-name=" + libname, libname);
}

private static void loadlib(this ptr<Link> _addr_ctxt) {
    ref Link ctxt = ref _addr_ctxt.val;

    uint flags = default;
    switch (FlagStrictDups.val) {
        case 0: 

            break;
        case 1: 

        case 2: 
            flags |= loader.FlagStrictDups;
            break;
        default: 
            log.Fatalf("invalid -strictdups flag value %d", FlagStrictDups.val);
            break;
    }
    if (!buildcfg.Experiment.RegabiWrappers) { 
        // Use ABI aliases if ABI wrappers are not used.
        flags |= loader.FlagUseABIAlias;
    }
    Action<@string, nint> elfsetstring1 = (str, off) => {
        elfsetstring(ctxt, 0, str, off);
    };
    ctxt.loader = loader.NewLoader(flags, elfsetstring1, _addr_ctxt.ErrorReporter.ErrorReporter);
    ctxt.ErrorReporter.SymName = s => ctxt.loader.SymName(s); 

    // ctxt.Library grows during the loop, so not a range loop.
    nint i = 0;
    while (i < len(ctxt.Library)) {
        var lib = ctxt.Library[i];
        if (lib.Shlib == "") {
            if (ctxt.Debugvlog > 1) {
                ctxt.Logf("autolib: %s (from %s)\n", lib.File, lib.Objref);
        i++;
            }
            loadobjfile(_addr_ctxt, _addr_lib);
        }
    } 

    // load internal packages, if not already
    if (flagRace.val) {
        loadinternal(_addr_ctxt, "runtime/race");
    }
    if (flagMsan.val) {
        loadinternal(_addr_ctxt, "runtime/msan");
    }
    loadinternal(_addr_ctxt, "runtime");
    while (i < len(ctxt.Library)) {
        lib = ctxt.Library[i];
        if (lib.Shlib == "") {
            loadobjfile(_addr_ctxt, _addr_lib);
        i++;
        }
    } 
    // At this point, the Go objects are "preloaded". Not all the symbols are
    // added to the symbol table (only defined package symbols are). Looking
    // up symbol by name may not get expected result.

    iscgo = ctxt.LibraryByPkg["runtime/cgo"] != null; 

    // Plugins a require cgo support to function. Similarly, plugins may require additional
    // internal linker support on some platforms which may not be implemented.
    ctxt.canUsePlugins = ctxt.LibraryByPkg["plugin"] != null && iscgo; 

    // We now have enough information to determine the link mode.
    determineLinkMode(ctxt);

    if (ctxt.LinkMode == LinkExternal && !iscgo && !(buildcfg.GOOS == "darwin" && ctxt.BuildMode != BuildModePlugin && ctxt.Arch.Family == sys.AMD64)) { 
        // This indicates a user requested -linkmode=external.
        // The startup code uses an import of runtime/cgo to decide
        // whether to initialize the TLS.  So give it one. This could
        // be handled differently but it's an unusual case.
        {
            var lib__prev2 = lib;

            lib = loadinternal(_addr_ctxt, "runtime/cgo");

            if (lib != null && lib.Shlib == "") {
                if (ctxt.BuildMode == BuildModeShared || ctxt.linkShared) {
                    Exitf("cannot implicitly include runtime/cgo in a shared library");
                }
                while (i < len(ctxt.Library)) {
                    lib = ctxt.Library[i];
                    if (lib.Shlib == "") {
                        loadobjfile(_addr_ctxt, _addr_lib);
                    i++;
                    }
                }
            }

            lib = lib__prev2;

        }
    }
    ctxt.loader.LoadSyms(ctxt.Arch); 

    // Load symbols from shared libraries, after all Go object symbols are loaded.
    {
        var lib__prev1 = lib;

        foreach (var (_, __lib) in ctxt.Library) {
            lib = __lib;
            if (lib.Shlib != "") {
                if (ctxt.Debugvlog > 1) {
                    ctxt.Logf("autolib: %s (from %s)\n", lib.Shlib, lib.Objref);
                }
                ldshlibsyms(_addr_ctxt, lib.Shlib);
            }
        }
        lib = lib__prev1;
    }

    ctxt.loadcgodirectives(); 

    // Conditionally load host objects, or setup for external linking.
    hostobjs(_addr_ctxt);
    hostlinksetup(_addr_ctxt);

    if (ctxt.LinkMode == LinkInternal && len(hostobj) != 0) { 
        // If we have any undefined symbols in external
        // objects, try to read them from the libgcc file.
        var any = false;
        var undefs = ctxt.loader.UndefinedRelocTargets(1);
        if (len(undefs) > 0) {
            any = true;
        }
        if (any) {
            if (flagLibGCC == "".val) {
                flagLibGCC.val = ctxt.findLibPathCmd("--print-libgcc-file-name", "libgcc");
            }
            if (runtime.GOOS == "openbsd" && flagLibGCC == "libgcc.a".val) { 
                // On OpenBSD `clang --print-libgcc-file-name` returns "libgcc.a".
                // In this case we fail to load libgcc.a and can encounter link
                // errors - see if we can find libcompiler_rt.a instead.
                flagLibGCC.val = ctxt.findLibPathCmd("--print-file-name=libcompiler_rt.a", "libcompiler_rt");
            }
            if (ctxt.HeadType == objabi.Hwindows) {
                {
                    var p__prev4 = p;

                    var p = ctxt.findLibPath("libmingwex.a");

                    if (p != "none") {
                        hostArchive(ctxt, p);
                    }

                    p = p__prev4;

                }
                {
                    var p__prev4 = p;

                    p = ctxt.findLibPath("libmingw32.a");

                    if (p != "none") {
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

                    if (p != "none") {
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
            if (flagLibGCC != "none".val) {
                hostArchive(ctxt, flagLibGCC.val);
            }
        }
    }
    ctxt.Loaded = true;

    importcycles();

    strictDupMsgCount = ctxt.loader.NStrictDupMsgs();
}

// loadcgodirectives reads the previously discovered cgo directives, creating
// symbols in preparation for host object loading or use later in the link.
private static void loadcgodirectives(this ptr<Link> _addr_ctxt) {
    ref Link ctxt = ref _addr_ctxt.val;

    var l = ctxt.loader;
    var hostObjSyms = make();
    foreach (var (_, d) in ctxt.cgodata) {
        setCgoAttr(ctxt, d.file, d.pkg, d.directives, hostObjSyms);
    }    ctxt.cgodata = null;

    if (ctxt.LinkMode == LinkInternal) { 
        // Drop all the cgo_import_static declarations.
        // Turns out we won't be needing them.
        foreach (var (symIdx) in hostObjSyms) {
            if (l.SymType(symIdx) == sym.SHOSTOBJ) { 
                // If a symbol was marked both
                // cgo_import_static and cgo_import_dynamic,
                // then we want to make it cgo_import_dynamic
                // now.
                var su = l.MakeSymbolUpdater(symIdx);
                if (l.SymExtname(symIdx) != "" && l.SymDynimplib(symIdx) != "" && !(l.AttrCgoExportStatic(symIdx) || l.AttrCgoExportDynamic(symIdx))) {
                    su.SetType(sym.SDYNIMPORT);
                }
                else
 {
                    su.SetType(0);
                }
            }
        }
    }
}

// Set up flags and special symbols depending on the platform build mode.
// This version works with loader.Loader.
private static void linksetup(this ptr<Link> _addr_ctxt) {
    ref Link ctxt = ref _addr_ctxt.val;


    if (ctxt.BuildMode == BuildModeCShared || ctxt.BuildMode == BuildModePlugin) 
        var symIdx = ctxt.loader.LookupOrCreateSym("runtime.islibrary", 0);
        var sb = ctxt.loader.MakeSymbolUpdater(symIdx);
        sb.SetType(sym.SNOPTRDATA);
        sb.AddUint8(1);
    else if (ctxt.BuildMode == BuildModeCArchive) 
        symIdx = ctxt.loader.LookupOrCreateSym("runtime.isarchive", 0);
        sb = ctxt.loader.MakeSymbolUpdater(symIdx);
        sb.SetType(sym.SNOPTRDATA);
        sb.AddUint8(1);
    // Recalculate pe parameters now that we have ctxt.LinkMode set.
    if (ctxt.HeadType == objabi.Hwindows) {
        Peinit(ctxt);
    }
    if (ctxt.HeadType == objabi.Hdarwin && ctxt.LinkMode == LinkExternal) {
        FlagTextAddr.val = 0;
    }
    if (ctxt.BuildMode == BuildModeExe) {
        if (havedynamic == 0 && ctxt.HeadType != objabi.Hdarwin && ctxt.HeadType != objabi.Hsolaris) {
            FlagD.val = true;
        }
    }
    if (ctxt.LinkMode == LinkExternal && ctxt.Arch.Family == sys.PPC64 && buildcfg.GOOS != "aix") {
        var toc = ctxt.loader.LookupOrCreateSym(".TOC.", 0);
        sb = ctxt.loader.MakeSymbolUpdater(toc);
        sb.SetType(sym.SDYNIMPORT);
    }
    if (buildcfg.GOOS != "android") {
        var tlsg = ctxt.loader.LookupOrCreateSym("runtime.tlsg", 0);
        sb = ctxt.loader.MakeSymbolUpdater(tlsg); 

        // runtime.tlsg is used for external linking on platforms that do not define
        // a variable to hold g in assembly (currently only intel).
        if (sb.Type() == 0) {
            sb.SetType(sym.STLSBSS);
            sb.SetSize(int64(ctxt.Arch.PtrSize));
        }
        else if (sb.Type() != sym.SDYNIMPORT) {
            Errorf(null, "runtime declared tlsg variable %v", sb.Type());
        }
        ctxt.loader.SetAttrReachable(tlsg, true);
        ctxt.Tlsg = tlsg;
    }
    loader.Sym moduledata = default;
    ptr<loader.SymbolBuilder> mdsb;
    if (ctxt.BuildMode == BuildModePlugin) {
        moduledata = ctxt.loader.LookupOrCreateSym("local.pluginmoduledata", 0);
        mdsb = ctxt.loader.MakeSymbolUpdater(moduledata);
        ctxt.loader.SetAttrLocal(moduledata, true);
    }
    else
 {
        moduledata = ctxt.loader.LookupOrCreateSym("runtime.firstmoduledata", 0);
        mdsb = ctxt.loader.MakeSymbolUpdater(moduledata);
    }
    if (mdsb.Type() != 0 && mdsb.Type() != sym.SDYNIMPORT) { 
        // If the module (toolchain-speak for "executable or shared
        // library") we are linking contains the runtime package, it
        // will define the runtime.firstmoduledata symbol and we
        // truncate it back to 0 bytes so we can define its entire
        // contents in symtab.go:symtab().
        mdsb.SetSize(0); 

        // In addition, on ARM, the runtime depends on the linker
        // recording the value of GOARM.
        if (ctxt.Arch.Family == sys.ARM) {
            var goarm = ctxt.loader.LookupOrCreateSym("runtime.goarm", 0);
            sb = ctxt.loader.MakeSymbolUpdater(goarm);
            sb.SetType(sym.SDATA);
            sb.SetSize(0);
            sb.AddUint8(uint8(buildcfg.GOARM));
        }
        var memProfile = ctxt.loader.Lookup("runtime.MemProfile", sym.SymVerABIInternal);
        if (memProfile != 0 && !ctxt.loader.AttrReachable(memProfile) && !ctxt.DynlinkingGo()) {
            var memProfSym = ctxt.loader.LookupOrCreateSym("runtime.disableMemoryProfiling", 0);
            sb = ctxt.loader.MakeSymbolUpdater(memProfSym);
            sb.SetType(sym.SDATA);
            sb.SetSize(0);
            sb.AddUint8(1); // true bool
        }
    }
    else
 { 
        // If OTOH the module does not contain the runtime package,
        // create a local symbol for the moduledata.
        moduledata = ctxt.loader.LookupOrCreateSym("local.moduledata", 0);
        mdsb = ctxt.loader.MakeSymbolUpdater(moduledata);
        ctxt.loader.SetAttrLocal(moduledata, true);
    }
    mdsb.SetType(sym.SNOPTRDATA);
    ctxt.loader.SetAttrReachable(moduledata, true);
    ctxt.Moduledata = moduledata;

    if (ctxt.Arch == sys.Arch386 && ctxt.HeadType != objabi.Hwindows) {
        if ((ctxt.BuildMode == BuildModeCArchive && ctxt.IsELF) || ctxt.BuildMode == BuildModeCShared || ctxt.BuildMode == BuildModePIE || ctxt.DynlinkingGo()) {
            var got = ctxt.loader.LookupOrCreateSym("_GLOBAL_OFFSET_TABLE_", 0);
            sb = ctxt.loader.MakeSymbolUpdater(got);
            sb.SetType(sym.SDYNIMPORT);
            ctxt.loader.SetAttrReachable(got, true);
        }
    }
    ctxt.Library = postorder(ctxt.Library);
    bool intlibs = new slice<bool>(new bool[] {  });
    foreach (var (_, lib) in ctxt.Library) {
        intlibs = append(intlibs, isRuntimeDepPkg(lib.Pkg));
    }    ctxt.Textp = ctxt.loader.AssignTextSymbolOrder(ctxt.Library, intlibs, ctxt.Textp);
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
private static void mangleTypeSym(this ptr<Link> _addr_ctxt) {
    ref Link ctxt = ref _addr_ctxt.val;

    if (ctxt.BuildMode != BuildModeShared && !ctxt.linkShared && ctxt.BuildMode != BuildModePlugin && !ctxt.CanUsePlugins()) {
        return ;
    }
    var ldr = ctxt.loader;
    for (var s = loader.Sym(1); s < loader.Sym(ldr.NSym()); s++) {
        if (!ldr.AttrReachable(s) && !ctxt.linkShared) { 
            // If -linkshared, the GCProg generation code may need to reach
            // out to the shared library for the type descriptor's data, even
            // the type descriptor itself is not actually needed at run time
            // (therefore not reachable). We still need to mangle its name,
            // so it is consistent with the one stored in the shared library.
            continue;
        }
        var name = ldr.SymName(s);
        var newName = typeSymbolMangle(name);
        if (newName != name) {
            ldr.SetSymExtname(s, newName); 

            // When linking against a shared library, the Go object file may
            // have reference to the original symbol name whereas the shared
            // library provides a symbol with the mangled name. We need to
            // copy the payload of mangled to original.
            // XXX maybe there is a better way to do this.
            var dup = ldr.Lookup(newName, ldr.SymVersion(s));
            if (dup != 0) {
                var st = ldr.SymType(s);
                var dt = ldr.SymType(dup);
                if (st == sym.Sxxx && dt != sym.Sxxx) {
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
private static @string typeSymbolMangle(@string name) {
    if (!strings.HasPrefix(name, "type.")) {
        return name;
    }
    if (strings.HasPrefix(name, "type.runtime.")) {
        return name;
    }
    if (len(name) <= 14 && !strings.Contains(name, "@")) { // Issue 19529
        return name;
    }
    var hash = sha1.Sum((slice<byte>)name);
    @string prefix = "type.";
    if (name[5] == '.') {
        prefix = "type..";
    }
    return prefix + base64.StdEncoding.EncodeToString(hash[..(int)6]);
}

/*
 * look for the next file in an archive.
 * adapted from libmach.
 */
private static long nextar(ptr<bio.Reader> _addr_bp, long off, ptr<ArHdr> _addr_a) {
    ref bio.Reader bp = ref _addr_bp.val;
    ref ArHdr a = ref _addr_a.val;

    if (off & 1 != 0) {
        off++;
    }
    bp.MustSeek(off, 0);
    array<byte> buf = new array<byte>(SAR_HDR);
    {
        var (n, err) = io.ReadFull(bp, buf[..]);

        if (err != null) {
            if (n == 0 && err != io.EOF) {
                return -1;
            }
            return 0;
        }
    }

    a.name = artrim(buf[(int)0..(int)16]);
    a.date = artrim(buf[(int)16..(int)28]);
    a.uid = artrim(buf[(int)28..(int)34]);
    a.gid = artrim(buf[(int)34..(int)40]);
    a.mode = artrim(buf[(int)40..(int)48]);
    a.size = artrim(buf[(int)48..(int)58]);
    a.fmag = artrim(buf[(int)58..(int)60]);

    var arsize = atolwhex(a.size);
    if (arsize & 1 != 0) {
        arsize++;
    }
    return arsize + SAR_HDR;
}

private static void loadobjfile(ptr<Link> _addr_ctxt, ptr<sym.Library> _addr_lib) => func((defer, _, _) => {
    ref Link ctxt = ref _addr_ctxt.val;
    ref sym.Library lib = ref _addr_lib.val;

    var pkg = objabi.PathToPrefix(lib.Pkg);

    if (ctxt.Debugvlog > 1) {
        ctxt.Logf("ldobj: %s (%s)\n", lib.File, pkg);
    }
    var (f, err) = bio.Open(lib.File);
    if (err != null) {
        Exitf("cannot open file %s: %v", lib.File, err);
    }
    defer(f.Close());
    defer(() => {
        if (pkg == "main" && !lib.Main) {
            Exitf("%s: not package main", lib.File);
        }
    }());

    for (nint i = 0; i < len(ARMAG); i++) {
        {
            var (c, err) = f.ReadByte();

            if (err == null && c == ARMAG[i]) {
                continue;
            } 

            /* load it as a regular file */

        } 

        /* load it as a regular file */
        var l = f.MustSeek(0, 2);
        f.MustSeek(0, 0);
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
    ref ArHdr arhdr = ref heap(out ptr<ArHdr> _addr_arhdr);
    var off = f.Offset();
    while (true) {
        l = nextar(_addr_f, off, _addr_arhdr);
        if (l == 0) {
            break;
        }
        if (l < 0) {
            Exitf("%s: malformed archive", lib.File);
        }
        off += l; 

        // __.PKGDEF isn't a real Go object file, and it's
        // absent in -linkobj builds anyway. Skipping it
        // ensures consistency between -linkobj and normal
        // build modes.
        if (arhdr.name == pkgdef) {
            continue;
        }
        if (len(arhdr.name) < 16) {
            {
                var ext = filepath.Ext(arhdr.name);

                if (ext != ".o" && ext != ".syso") {
                    continue;
                }

            }
        }
        var pname = fmt.Sprintf("%s(%s)", lib.File, arhdr.name);
        l = atolwhex(arhdr.size);
        ldobj(_addr_ctxt, _addr_f, _addr_lib, l, pname, lib.File);
    }
});

public partial struct Hostobj {
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

private static ptr<Hostobj> ldhostobj(Action<ptr<Link>, ptr<bio.Reader>, @string, long, @string> ld, objabi.HeadType headType, ptr<bio.Reader> _addr_f, @string pkg, long length, @string pn, @string file) {
    ref bio.Reader f = ref _addr_f.val;

    var isinternal = false;
    foreach (var (_, intpkg) in internalpkg) {
        if (pkg == intpkg) {
            isinternal = true;
            break;
        }
    }    if (headType == objabi.Hdragonfly) {
        if (pkg == "net" || pkg == "os/user") {
            isinternal = false;
        }
    }
    if (!isinternal) {
        externalobj = true;
    }
    hostobj = append(hostobj, new Hostobj());
    var h = _addr_hostobj[len(hostobj) - 1];
    h.ld = ld;
    h.pkg = pkg;
    h.pn = pn;
    h.file = file;
    h.off = f.Offset();
    h.length = length;
    return _addr_h!;
}

private static void hostobjs(ptr<Link> _addr_ctxt) {
    ref Link ctxt = ref _addr_ctxt.val;

    if (ctxt.LinkMode != LinkInternal) {
        return ;
    }
    ptr<Hostobj> h;

    for (nint i = 0; i < len(hostobj); i++) {
        h = _addr_hostobj[i];
        var (f, err) = bio.Open(h.file);
        if (err != null) {
            Exitf("cannot reopen %s: %v", h.pn, err);
        }
        f.MustSeek(h.off, 0);
        if (h.ld == null) {
            Errorf(null, "%s: unrecognized object file format", h.pn);
            continue;
        }
        h.ld(ctxt, f, h.pkg, h.length, h.pn);
        f.Close();
    }
}

private static void hostlinksetup(ptr<Link> _addr_ctxt) {
    ref Link ctxt = ref _addr_ctxt.val;

    if (ctxt.LinkMode != LinkExternal) {
        return ;
    }
    debug_s = FlagS.val;
    FlagS.val = false; 

    // create temporary directory and arrange cleanup
    if (flagTmpdir == "".val) {
        var (dir, err) = ioutil.TempDir("", "go-link-");
        if (err != null) {
            log.Fatal(err);
        }
        flagTmpdir.val = dir;
        ownTmpDir = true;
        AtExit(() => {
            ctxt.Out.Close();
            os.RemoveAll(flagTmpdir.val);
        });
    }
    {
        var err__prev1 = err;

        var err = ctxt.Out.Close();

        if (err != null) {
            Exitf("error closing output file");
        }
        err = err__prev1;

    }
    mayberemoveoutfile();

    var p = filepath.Join(flagTmpdir.val, "go.o");
    {
        var err__prev1 = err;

        err = ctxt.Out.Open(p);

        if (err != null) {
            Exitf("cannot create %s: %v", p, err);
        }
        err = err__prev1;

    }
}

// hostobjCopy creates a copy of the object files in hostobj in a
// temporary directory.
private static slice<@string> hostobjCopy() => func((defer, _, _) => {
    slice<@string> paths = default;

    sync.WaitGroup wg = default;
    var sema = make_channel<object>(runtime.NumCPU()); // limit open file descriptors
    {
        var h__prev1 = h;

        foreach (var (__i, __h) in hostobj) {
            i = __i;
            h = __h;
            var h = h;
            var dst = filepath.Join(flagTmpdir.val, fmt.Sprintf("%06d.o", i));
            paths = append(paths, dst);

            wg.Add(1);
            go_(() => () => {
                sema.Send(/* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{});
                defer(() => {
                    sema.Receive();
                    wg.Done();
                }());
                var (f, err) = os.Open(h.file);
                if (err != null) {
                    Exitf("cannot reopen %s: %v", h.pn, err);
                }
                defer(f.Close());
                {
                    var (_, err) = f.Seek(h.off, 0);

                    if (err != null) {
                        Exitf("cannot seek %s: %v", h.pn, err);
                    }

                }

                var (w, err) = os.Create(dst);
                if (err != null) {
                    Exitf("cannot create %s: %v", dst, err);
                }
                {
                    (_, err) = io.CopyN(w, f, h.length);

                    if (err != null) {
                        Exitf("cannot write %s: %v", dst, err);
                    }

                }
                {
                    var err = w.Close();

                    if (err != null) {
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
private static @string writeGDBLinkerScript() {
    @string name = "fix_debug_gdb_scripts.ld";
    var path = filepath.Join(flagTmpdir.val, name);
    @string src = "SECTIONS\n{\n  .debug_gdb_scripts BLOCK(__section_alignment__) (NOLOAD) :\n  {\n    *" +
    "(.debug_gdb_scripts)\n  }\n}\nINSERT AFTER .debug_types;\n";
    var err = ioutil.WriteFile(path, (slice<byte>)src, 0666);
    if (err != null) {
        Errorf(null, "WriteFile %s failed: %v", name, err);
    }
    return path;
}

// archive builds a .a archive from the hostobj object files.
private static void archive(this ptr<Link> _addr_ctxt) => func((_, panic, _) => {
    ref Link ctxt = ref _addr_ctxt.val;

    if (ctxt.BuildMode != BuildModeCArchive) {
        return ;
    }
    exitIfErrors();

    if (flagExtar == "".val) {
        flagExtar.val = "ar";
    }
    mayberemoveoutfile(); 

    // Force the buffer to flush here so that external
    // tools will see a complete file.
    {
        var err = ctxt.Out.Close();

        if (err != null) {
            Exitf("error closing %v", flagOutfile.val);
        }
    }

    @string argv = new slice<@string>(new @string[] { *flagExtar, "-q", "-c", "-s" });
    if (ctxt.HeadType == objabi.Haix) {
        argv = append(argv, "-X64");
    }
    argv = append(argv, flagOutfile.val);
    argv = append(argv, filepath.Join(flagTmpdir.val, "go.o"));
    argv = append(argv, hostobjCopy());

    if (ctxt.Debugvlog != 0) {
        ctxt.Logf("archive: %s\n", strings.Join(argv, " "));
    }
    if (syscallExecSupported && !ownTmpDir) {
        runAtExitFuncs();
        ctxt.execArchive(argv);
        panic("should not get here");
    }
    {
        var (out, err) = exec.Command(argv[0], argv[(int)1..]).CombinedOutput();

        if (err != null) {
            Exitf("running %s failed: %v\n%s", argv[0], err, out);
        }
    }
});

private static void hostlink(this ptr<Link> _addr_ctxt) => func((defer, _, _) => {
    ref Link ctxt = ref _addr_ctxt.val;

    if (ctxt.LinkMode != LinkExternal || nerrors > 0) {
        return ;
    }
    if (ctxt.BuildMode == BuildModeCArchive) {
        return ;
    }
    slice<@string> argv = default;
    argv = append(argv, ctxt.extld());
    argv = append(argv, hostlinkArchArgs(_addr_ctxt.Arch));

    if (FlagS || debug_s.val) {
        if (ctxt.HeadType == objabi.Hdarwin) { 
            // Recent versions of macOS print
            //    ld: warning: option -s is obsolete and being ignored
            // so do not pass any arguments.
        }
        else
 {
            argv = append(argv, "-s");
        }
    }
    var combineDwarf = ctxt.IsDarwin() && !FlagS && !FlagW && !debug_s && machoPlatform == PLATFORM_MACOS.val;


    if (ctxt.HeadType == objabi.Hdarwin) 
        if (combineDwarf) { 
            // Leave room for DWARF combining.
            // -headerpad is incompatible with -fembed-bitcode.
            argv = append(argv, "-Wl,-headerpad,1144");
        }
        if (ctxt.DynlinkingGo() && buildcfg.GOOS != "ios") { 
            // -flat_namespace is deprecated on iOS.
            // It is useful for supporting plugins. We don't support plugins on iOS.
            argv = append(argv, "-Wl,-flat_namespace");
        }
        if (!combineDwarf) {
            argv = append(argv, "-Wl,-S"); // suppress STAB (symbolic debugging) symbols
        }
    else if (ctxt.HeadType == objabi.Hopenbsd) 
        argv = append(argv, "-Wl,-nopie");
        argv = append(argv, "-pthread");
    else if (ctxt.HeadType == objabi.Hwindows) 
        if (windowsgui) {
            argv = append(argv, "-mwindows");
        }
        else
 {
            argv = append(argv, "-mconsole");
        }
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
    // Enable ASLR on Windows.
    Func<slice<@string>, slice<@string>> addASLRargs = argv => { 
        // Enable ASLR.
        argv = append(argv, "-Wl,--dynamicbase"); 
        // enable high-entropy ASLR on 64-bit.
        if (ctxt.Arch.PtrSize >= 8) {
            argv = append(argv, "-Wl,--high-entropy-va");
        }
        return argv;
    };


    if (ctxt.BuildMode == BuildModeExe) 
        if (ctxt.HeadType == objabi.Hdarwin) {
            if (machoPlatform == PLATFORM_MACOS && ctxt.IsAMD64()) {
                argv = append(argv, "-Wl,-no_pie");
                argv = append(argv, "-Wl,-pagezero_size,4000000");
            }
        }
    else if (ctxt.BuildMode == BuildModePIE) 

        if (ctxt.HeadType == objabi.Hdarwin || ctxt.HeadType == objabi.Haix)         else if (ctxt.HeadType == objabi.Hwindows) 
            argv = addASLRargs(argv);
        else 
            // ELF.
            if (ctxt.UseRelro()) {
                argv = append(argv, "-Wl,-z,relro");
            }
            argv = append(argv, "-pie");
            else if (ctxt.BuildMode == BuildModeCShared) 
        if (ctxt.HeadType == objabi.Hdarwin) {
            argv = append(argv, "-dynamiclib");
        }
        else
 {
            if (ctxt.UseRelro()) {
                argv = append(argv, "-Wl,-z,relro");
            }
            argv = append(argv, "-shared");
            if (ctxt.HeadType == objabi.Hwindows) {
                if (flagAslr.val) {
                    argv = addASLRargs(argv);
                }
            }
            else
 { 
                // Pass -z nodelete to mark the shared library as
                // non-closeable: a dlclose will do nothing.
                argv = append(argv, "-Wl,-z,nodelete"); 
                // Only pass Bsymbolic on non-Windows.
                argv = append(argv, "-Wl,-Bsymbolic");
            }
        }
    else if (ctxt.BuildMode == BuildModeShared) 
        if (ctxt.UseRelro()) {
            argv = append(argv, "-Wl,-z,relro");
        }
        argv = append(argv, "-shared");
    else if (ctxt.BuildMode == BuildModePlugin) 
        if (ctxt.HeadType == objabi.Hdarwin) {
            argv = append(argv, "-dynamiclib");
        }
        else
 {
            if (ctxt.UseRelro()) {
                argv = append(argv, "-Wl,-z,relro");
            }
            argv = append(argv, "-shared");
        }
        @string altLinker = default;
    if (ctxt.IsELF && ctxt.DynlinkingGo()) { 
        // We force all symbol resolution to be done at program startup
        // because lazy PLT resolution can use large amounts of stack at
        // times we cannot allow it to do so.
        argv = append(argv, "-Wl,-znow"); 

        // Do not let the host linker generate COPY relocations. These
        // can move symbols out of sections that rely on stable offsets
        // from the beginning of the section (like sym.STYPE).
        argv = append(argv, "-Wl,-znocopyreloc");

        if (buildcfg.GOOS == "android") { 
            // Use lld to avoid errors from default linker (issue #38838)
            altLinker = "lld";
        }
        if (ctxt.Arch.InFamily(sys.ARM, sys.ARM64) && buildcfg.GOOS == "linux") { 
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

                if (err == null) {
                    if (!bytes.Contains(out, (slice<byte>)"GNU gold")) {
                        log.Fatalf("ARM external linker must be gold (issue #15696), but is not: %s", out);
                    }
                }

                out = out__prev3;
                err = err__prev3;

            }
        }
    }
    if (ctxt.Arch.Family == sys.ARM64 && buildcfg.GOOS == "freebsd") { 
        // Switch to ld.bfd on freebsd/arm64.
        altLinker = "bfd"; 

        // Provide a useful error if ld.bfd is missing.
        cmd = exec.Command(flagExtld.val, "-fuse-ld=bfd", "-Wl,--version");
        {
            var out__prev2 = out;
            var err__prev2 = err;

            (out, err) = cmd.CombinedOutput();

            if (err == null) {
                if (!bytes.Contains(out, (slice<byte>)"GNU ld")) {
                    log.Fatalf("ARM64 external linker must be ld.bfd (issue #35197), please install devel/binutils");
                }
            }

            out = out__prev2;
            err = err__prev2;

        }
    }
    if (altLinker != "") {
        argv = append(argv, "-fuse-ld=" + altLinker);
    }
    if (ctxt.IsELF && len(buildinfo) > 0) {
        argv = append(argv, fmt.Sprintf("-Wl,--build-id=0x%x", buildinfo));
    }
    var outopt = flagOutfile.val;
    if (buildcfg.GOOS == "windows" && runtime.GOOS == "windows" && filepath.Ext(outopt) == "") {
        outopt += ".";
    }
    argv = append(argv, "-o");
    argv = append(argv, outopt);

    if (rpath.val != "") {
        argv = append(argv, fmt.Sprintf("-Wl,-rpath,%s", rpath.val));
    }
    if (flagInterpreter != "".val) { 
        // Many linkers support both -I and the --dynamic-linker flags
        // to set the ELF interpreter, but lld only supports
        // --dynamic-linker so prefer that (ld on very old Solaris only
        // supports -I but that seems less important).
        argv = append(argv, fmt.Sprintf("-Wl,--dynamic-linker,%s", flagInterpreter.val));
    }
    if (ctxt.IsELF) {
        argv = append(argv, "-rdynamic");
    }
    if (ctxt.HeadType == objabi.Haix) {
        var fileName = xcoffCreateExportFile(ctxt);
        argv = append(argv, "-Wl,-bE:" + fileName);
    }
    const @string unusedArguments = "-Qunused-arguments";

    if (linkerFlagSupported(_addr_ctxt.Arch, argv[0], altLinker, unusedArguments)) {
        argv = append(argv, unusedArguments);
    }
    const @string compressDWARF = "-Wl,--compress-debug-sections=zlib-gnu";

    if (ctxt.compressDWARF && linkerFlagSupported(_addr_ctxt.Arch, argv[0], altLinker, compressDWARF)) {
        argv = append(argv, compressDWARF);
    }
    argv = append(argv, filepath.Join(flagTmpdir.val, "go.o"));
    argv = append(argv, hostobjCopy());
    if (ctxt.HeadType == objabi.Haix) { 
        // We want to have C files after Go files to remove
        // trampolines csects made by ld.
        argv = append(argv, "-nostartfiles");
        argv = append(argv, "/lib/crt0_64.o");

        var extld = ctxt.extld(); 
        // Get starting files.
        Func<@string, @string> getPathFile = file => {
            @string args = new slice<@string>(new @string[] { "-maix64", "--print-file-name="+file });
            (out, err) = exec.Command(extld, args).CombinedOutput();
            if (err != null) {
                log.Fatalf("running %s failed: %v\n%s", extld, err, out);
            }
            return strings.Trim(string(out), "\n");
        };
        argv = append(argv, getPathFile("crtcxa.o"));
        argv = append(argv, getPathFile("crtdbase.o"));
    }
    if (ctxt.linkShared) {
        var seenDirs = make_map<@string, bool>();
        var seenLibs = make_map<@string, bool>();
        Action<@string> addshlib = path => {
            var (dir, base) = filepath.Split(path);
            if (!seenDirs[dir]) {
                argv = append(argv, "-L" + dir);
                if (!rpath.set) {
                    argv = append(argv, "-Wl,-rpath=" + dir);
                }
                seenDirs[dir] = true;
            }
            base = strings.TrimSuffix(base, ".so");
            base = strings.TrimPrefix(base, "lib");
            if (!seenLibs[base]) {
                argv = append(argv, "-l" + base);
                seenLibs[base] = true;
            }
        };
        foreach (var (_, shlib) in ctxt.Shlibs) {
            addshlib(shlib.Path);
            foreach (var (_, dep) in shlib.Deps) {
                if (dep == "") {
                    continue;
                }
                var libpath = findshlib(_addr_ctxt, dep);
                if (libpath != "") {
                    addshlib(libpath);
                }
            }
        }
    }
    Action<@string> checkStatic = arg => {
        if (ctxt.IsELF && arg == "-static") {
            foreach (var (i) in argv) {
                if (argv[i] == "-rdynamic" || strings.HasPrefix(argv[i], "-Wl,--dynamic-linker,")) {
                    argv[i] = "-static";
                }
            }
        }
    };

    {
        var p__prev1 = p;

        foreach (var (_, __p) in ldflag) {
            p = __p;
            argv = append(argv, p);
            checkStatic(p);
        }
        p = p__prev1;
    }

    if (ctxt.BuildMode == BuildModeExe && !ctxt.linkShared && !(ctxt.IsDarwin() && ctxt.IsARM64())) { 
        // GCC uses -no-pie, clang uses -nopie.
        foreach (var (_, nopie) in new slice<@string>(new @string[] { "-no-pie", "-nopie" })) {
            if (linkerFlagSupported(_addr_ctxt.Arch, argv[0], altLinker, nopie)) {
                argv = append(argv, nopie);
                break;
            }
        }
    }
    {
        var p__prev1 = p;

        foreach (var (_, __p) in strings.Fields(flagExtldflags.val)) {
            p = __p;
            argv = append(argv, p);
            checkStatic(p);
        }
        p = p__prev1;
    }

    if (ctxt.HeadType == objabi.Hwindows) { 
        // Determine which linker we're using. Add in the extldflags in
        // case used has specified "-fuse-ld=...".
        cmd = exec.Command(flagExtld.val, flagExtldflags.val, "-Wl,--version");
        var usingLLD = false;
        {
            var out__prev2 = out;
            var err__prev2 = err;

            (out, err) = cmd.CombinedOutput();

            if (err == null) {
                if (bytes.Contains(out, (slice<byte>)"LLD ")) {
                    usingLLD = true;
                }
            } 

            // use gcc linker script to work around gcc bug
            // (see https://golang.org/issue/20183 for details).

            out = out__prev2;
            err = err__prev2;

        } 

        // use gcc linker script to work around gcc bug
        // (see https://golang.org/issue/20183 for details).
        if (!usingLLD) {
            var p = writeGDBLinkerScript();
            argv = append(argv, "-Wl,-T," + p);
        }
        argv = append(argv, "-Wl,--start-group", "-lmingwex", "-lmingw32", "-Wl,--end-group");
        argv = append(argv, peimporteddlls());
    }
    if (ctxt.Debugvlog != 0) {
        ctxt.Logf("host link:");
        foreach (var (_, v) in argv) {
            ctxt.Logf(" %q", v);
        }        ctxt.Logf("\n");
    }
    (out, err) = exec.Command(argv[0], argv[(int)1..]).CombinedOutput();
    if (err != null) {
        Exitf("running %s failed: %v\n%s", argv[0], err, out);
    }
    slice<slice<byte>> save = default;
    nint skipLines = default;
    foreach (var (_, line) in bytes.SplitAfter(out, (slice<byte>)"\n")) { 
        // golang.org/issue/26073 - Apple Xcode bug
        if (bytes.Contains(line, (slice<byte>)"ld: warning: text-based stub file")) {
            continue;
        }
        if (skipLines > 0) {
            skipLines--;
            continue;
        }
        if (bytes.Contains(line, (slice<byte>)"ld: 0711-783")) {
            skipLines = 2;
            continue;
        }
        save = append(save, line);
    }    out = bytes.Join(save, null);

    if (len(out) > 0) { 
        // always print external output even if the command is successful, so that we don't
        // swallow linker warnings (see https://golang.org/issue/17935).
        ctxt.Logf("%s", out);
    }
    if (combineDwarf) {
        var dsym = filepath.Join(flagTmpdir.val, "go.dwarf");
        {
            var out__prev2 = out;
            var err__prev2 = err;

            (out, err) = exec.Command("xcrun", "dsymutil", "-f", flagOutfile.val, "-o", dsym).CombinedOutput();

            if (err != null) {
                Exitf("%s: running dsymutil failed: %v\n%s", os.Args[0], err, out);
            } 
            // Remove STAB (symbolic debugging) symbols after we are done with them (by dsymutil).
            // They contain temporary file paths and make the build not reproducible.

            out = out__prev2;
            err = err__prev2;

        } 
        // Remove STAB (symbolic debugging) symbols after we are done with them (by dsymutil).
        // They contain temporary file paths and make the build not reproducible.
        {
            var out__prev2 = out;
            var err__prev2 = err;

            (out, err) = exec.Command("xcrun", "strip", "-S", flagOutfile.val).CombinedOutput();

            if (err != null) {
                Exitf("%s: running strip failed: %v\n%s", os.Args[0], err, out);
            } 
            // Skip combining if `dsymutil` didn't generate a file. See #11994.

            out = out__prev2;
            err = err__prev2;

        } 
        // Skip combining if `dsymutil` didn't generate a file. See #11994.
        {
            var err__prev2 = err;

            var (_, err) = os.Stat(dsym);

            if (os.IsNotExist(err)) {
                return ;
            } 
            // For os.Rename to work reliably, must be in same directory as outfile.

            err = err__prev2;

        } 
        // For os.Rename to work reliably, must be in same directory as outfile.
        var combinedOutput = flagOutfile + "~".val;
        var (exef, err) = os.Open(flagOutfile.val);
        if (err != null) {
            Exitf("%s: combining dwarf failed: %v", os.Args[0], err);
        }
        defer(exef.Close());
        var (exem, err) = macho.NewFile(exef);
        if (err != null) {
            Exitf("%s: parsing Mach-O header failed: %v", os.Args[0], err);
        }
        {
            var err__prev2 = err;

            var err = machoCombineDwarf(ctxt, exef, exem, dsym, combinedOutput);

            if (err != null) {
                Exitf("%s: combining dwarf failed: %v", os.Args[0], err);
            }

            err = err__prev2;

        }
        os.Remove(flagOutfile.val);
        {
            var err__prev2 = err;

            err = os.Rename(combinedOutput, flagOutfile.val);

            if (err != null) {
                Exitf("%s: %v", os.Args[0], err);
            }

            err = err__prev2;

        }
    }
    if (ctxt.NeedCodeSign()) {
        err = machoCodeSign(ctxt, flagOutfile.val);
        if (err != null) {
            Exitf("%s: code signing failed: %v", os.Args[0], err);
        }
    }
});

private static sync.Once createTrivialCOnce = default;

private static bool linkerFlagSupported(ptr<sys.Arch> _addr_arch, @string linker, @string altLinker, @string flag) {
    ref sys.Arch arch = ref _addr_arch.val;

    createTrivialCOnce.Do(() => {
        var src = filepath.Join(flagTmpdir.val, "trivial.c");
        {
            var err = ioutil.WriteFile(src, (slice<byte>)"int main() { return 0; }", 0666);

            if (err != null) {
                Errorf(null, "WriteFile trivial.c failed: %v", err);
            }

        }
    });

    @string flagsWithNextArgSkip = new slice<@string>(new @string[] { "-F", "-l", "-L", "-framework", "-Wl,-framework", "-Wl,-rpath", "-Wl,-undefined" });
    @string flagsWithNextArgKeep = new slice<@string>(new @string[] { "-arch", "-isysroot", "--sysroot", "-target" });
    @string prefixesToKeep = new slice<@string>(new @string[] { "-f", "-m", "-p", "-Wl,", "-arch", "-isysroot", "--sysroot", "-target" });

    var flags = hostlinkArchArgs(_addr_arch);
    var keep = false;
    var skip = false;
    var extldflags = strings.Fields(flagExtldflags.val);
    foreach (var (_, f) in append(extldflags, ldflag)) {
        if (keep) {
            flags = append(flags, f);
            keep = false;
        }
        else if (skip) {
            skip = false;
        }
        else if (f == "" || f[0] != '-') {
        }
        else if (contains(flagsWithNextArgSkip, f)) {
            skip = true;
        }
        else if (contains(flagsWithNextArgKeep, f)) {
            flags = append(flags, f);
            keep = true;
        }
        else
 {
            foreach (var (_, p) in prefixesToKeep) {
                if (strings.HasPrefix(f, p)) {
                    flags = append(flags, f);
                    break;
                }
            }
        }
    }    if (altLinker != "") {
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
private static slice<@string> hostlinkArchArgs(ptr<sys.Arch> _addr_arch) {
    ref sys.Arch arch = ref _addr_arch.val;


    if (arch.Family == sys.I386) 
        return new slice<@string>(new @string[] { "-m32" });
    else if (arch.Family == sys.AMD64) 
        if (buildcfg.GOOS == "darwin") {
            return new slice<@string>(new @string[] { "-arch", "x86_64", "-m64" });
        }
        return new slice<@string>(new @string[] { "-m64" });
    else if (arch.Family == sys.S390X) 
        return new slice<@string>(new @string[] { "-m64" });
    else if (arch.Family == sys.ARM) 
        return new slice<@string>(new @string[] { "-marm" });
    else if (arch.Family == sys.ARM64) 
        if (buildcfg.GOOS == "darwin") {
            return new slice<@string>(new @string[] { "-arch", "arm64" });
        }
    else if (arch.Family == sys.MIPS64) 
        return new slice<@string>(new @string[] { "-mabi=64" });
    else if (arch.Family == sys.MIPS) 
        return new slice<@string>(new @string[] { "-mabi=32" });
    else if (arch.Family == sys.PPC64) 
        if (buildcfg.GOOS == "aix") {
            return new slice<@string>(new @string[] { "-maix64" });
        }
        else
 {
            return new slice<@string>(new @string[] { "-m64" });
        }
        return null;
}

private static var wantHdr = objabi.HeaderString();

// ldobj loads an input object. If it is a host object (an object
// compiled by a non-Go compiler) it returns the Hostobj pointer. If
// it is a Go object, it returns nil.
private static ptr<Hostobj> ldobj(ptr<Link> _addr_ctxt, ptr<bio.Reader> _addr_f, ptr<sym.Library> _addr_lib, long length, @string pn, @string file) {
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
    f.MustSeek(start, 0);

    ptr<sym.CompilationUnit> unit = addr(new sym.CompilationUnit(Lib:lib));
    lib.Units = append(lib.Units, unit);

    var magic = uint32(c1) << 24 | uint32(c2) << 16 | uint32(c3) << 8 | uint32(c4);
    if (magic == 0x7f454c46) { // \x7F E L F
        Action<ptr<Link>, ptr<bio.Reader>, @string, long, @string> ldelf = (ctxt, f, pkg, length, pn) => {
            var (textp, flags, err) = loadelf.Load(ctxt.loader, ctxt.Arch, ctxt.IncVersion(), f, pkg, length, pn, ehdr.Flags);
            if (err != null) {
                Errorf(null, "%v", err);
                return ;
            }
            ehdr.Flags = flags;
            ctxt.Textp = append(ctxt.Textp, textp);
        };
        return _addr_ldhostobj(ldelf, ctxt.HeadType, _addr_f, pkg, length, pn, file)!;
    }
    if (magic & ~1 == 0xfeedface || magic & ~0x01000000 == 0xcefaedfe) {
        Action<ptr<Link>, ptr<bio.Reader>, @string, long, @string> ldmacho = (ctxt, f, pkg, length, pn) => {
            var (textp, err) = loadmacho.Load(ctxt.loader, ctxt.Arch, ctxt.IncVersion(), f, pkg, length, pn);
            if (err != null) {
                Errorf(null, "%v", err);
                return ;
            }
            ctxt.Textp = append(ctxt.Textp, textp);
        };
        return _addr_ldhostobj(ldmacho, ctxt.HeadType, _addr_f, pkg, length, pn, file)!;
    }
    switch (c1 << 8 | c2) {
        case 0x4c01: // arm64

        case 0x6486: // arm64

        case 0xc401: // arm64

        case 0x64aa: // arm64
            Action<ptr<Link>, ptr<bio.Reader>, @string, long, @string> ldpe = (ctxt, f, pkg, length, pn) => {
                var (textp, rsrc, err) = loadpe.Load(ctxt.loader, ctxt.Arch, ctxt.IncVersion(), f, pkg, length, pn);
                if (err != null) {
                    Errorf(null, "%v", err);
                    return ;
                }
                if (len(rsrc) != 0) {
                    setpersrc(ctxt, rsrc);
                }
                ctxt.Textp = append(ctxt.Textp, textp);
            };
            return _addr_ldhostobj(ldpe, ctxt.HeadType, _addr_f, pkg, length, pn, file)!;
            break;
    }

    if (c1 == 0x01 && (c2 == 0xD7 || c2 == 0xF7)) {
        Action<ptr<Link>, ptr<bio.Reader>, @string, long, @string> ldxcoff = (ctxt, f, pkg, length, pn) => {
            (textp, err) = loadxcoff.Load(ctxt.loader, ctxt.Arch, ctxt.IncVersion(), f, pkg, length, pn);
            if (err != null) {
                Errorf(null, "%v", err);
                return ;
            }
            ctxt.Textp = append(ctxt.Textp, textp);
        };
        return _addr_ldhostobj(ldxcoff, ctxt.HeadType, _addr_f, pkg, length, pn, file)!;
    }
    if (c1 != 'g' || c2 != 'o' || c3 != ' ' || c4 != 'o') { 
        // An unrecognized object is just passed to the external linker.
        // If we try to read symbols from this object, we will
        // report an error at that time.
        unknownObjFormat = true;
        return _addr_ldhostobj(null, ctxt.HeadType, _addr_f, pkg, length, pn, file)!;
    }
    var (line, err) = f.ReadString('\n');
    if (err != null) {
        Errorf(null, "truncated object file: %s: %v", pn, err);
        return _addr_null!;
    }
    if (!strings.HasPrefix(line, "go object ")) {
        if (strings.HasSuffix(pn, ".go")) {
            Exitf("%s: uncompiled .go source file", pn);
            return _addr_null!;
        }
        if (line == ctxt.Arch.Name) { 
            // old header format: just $GOOS
            Errorf(null, "%s: stale object file", pn);
            return _addr_null!;
        }
        Errorf(null, "%s: not an object file: @%d %q", pn, start, line);
        return _addr_null!;
    }
    if (line != wantHdr) {
        Errorf(null, "%s: linked object header mismatch:\nhave %q\nwant %q\n", pn, line, wantHdr);
    }
    var import0 = f.Offset();

    c1 = '\n'; // the last line ended in \n
    c2 = bgetc(_addr_f);
    c3 = bgetc(_addr_f);
    nint markers = 0;
    while (true) {
        if (c1 == '\n') {
            if (markers % 2 == 0 && c2 == '!' && c3 == '\n') {
                break;
            }
            if (c2 == '$' && c3 == '$') {
                markers++;
            }
        }
        c1 = c2;
        c2 = c3;
        c3 = bgetc(_addr_f);
        if (c3 == -1) {
            Errorf(null, "truncated object file: %s", pn);
            return _addr_null!;
        }
    }

    var import1 = f.Offset();

    f.MustSeek(import0, 0);
    ldpkg(ctxt, f, lib, import1 - import0 - 2, pn); // -2 for !\n
    f.MustSeek(import1, 0);

    var fingerprint = ctxt.loader.Preload(ctxt.IncVersion(), f, lib, unit, eof - f.Offset());
    if (!fingerprint.IsZero()) { // Assembly objects don't have fingerprints. Ignore them.
        // Check fingerprint, to ensure the importing and imported packages
        // have consistent view of symbol indices.
        // Normally the go command should ensure this. But in case something
        // goes wrong, it could lead to obscure bugs like run-time crash.
        // Check it here to be sure.
        if (lib.Fingerprint.IsZero()) { // Not yet imported. Update its fingerprint.
            lib.Fingerprint = fingerprint;
        }
        checkFingerprint(_addr_lib, fingerprint, lib.Srcref, lib.Fingerprint);
    }
    addImports(ctxt, lib, pn);
    return _addr_null!;
}

private static void checkFingerprint(ptr<sym.Library> _addr_lib, goobj.FingerprintType libfp, @string src, goobj.FingerprintType srcfp) {
    ref sym.Library lib = ref _addr_lib.val;

    if (libfp != srcfp) {
        Exitf("fingerprint mismatch: %s has %x, import from %s expecting %x", lib, libfp, src, srcfp);
    }
}

private static slice<byte> readelfsymboldata(ptr<Link> _addr_ctxt, ptr<elf.File> _addr_f, ptr<elf.Symbol> _addr_sym) {
    ref Link ctxt = ref _addr_ctxt.val;
    ref elf.File f = ref _addr_f.val;
    ref elf.Symbol sym = ref _addr_sym.val;

    var data = make_slice<byte>(sym.Size);
    var sect = f.Sections[sym.Section];
    if (sect.Type != elf.SHT_PROGBITS && sect.Type != elf.SHT_NOTE) {
        Errorf(null, "reading %s from non-data section", sym.Name);
    }
    var (n, err) = sect.ReadAt(data, int64(sym.Value - sect.Addr));
    if (uint64(n) != sym.Size) {
        Errorf(null, "reading contents of %s: %v", sym.Name, err);
    }
    return data;
}

private static (slice<byte>, error) readwithpad(io.Reader r, int sz) {
    slice<byte> _p0 = default;
    error _p0 = default!;

    var data = make_slice<byte>(Rnd(int64(sz), 4));
    var (_, err) = io.ReadFull(r, data);
    if (err != null) {
        return (null, error.As(err)!);
    }
    data = data[..(int)sz];
    return (data, error.As(null!)!);
}

private static (slice<byte>, error) readnote(ptr<elf.File> _addr_f, slice<byte> name, int typ) {
    slice<byte> _p0 = default;
    error _p0 = default!;
    ref elf.File f = ref _addr_f.val;

    foreach (var (_, sect) in f.Sections) {
        if (sect.Type != elf.SHT_NOTE) {
            continue;
        }
        var r = sect.Open();
        while (true) {
            ref int namesize = ref heap(out ptr<int> _addr_namesize);            ref int descsize = ref heap(out ptr<int> _addr_descsize);            ref int noteType = ref heap(out ptr<int> _addr_noteType);

            var err = binary.Read(r, f.ByteOrder, _addr_namesize);
            if (err != null) {
                if (err == io.EOF) {
                    break;
                }
                return (null, error.As(fmt.Errorf("read namesize failed: %v", err))!);
            }
            err = binary.Read(r, f.ByteOrder, _addr_descsize);
            if (err != null) {
                return (null, error.As(fmt.Errorf("read descsize failed: %v", err))!);
            }
            err = binary.Read(r, f.ByteOrder, _addr_noteType);
            if (err != null) {
                return (null, error.As(fmt.Errorf("read type failed: %v", err))!);
            }
            var (noteName, err) = readwithpad(r, namesize);
            if (err != null) {
                return (null, error.As(fmt.Errorf("read name failed: %v", err))!);
            }
            var (desc, err) = readwithpad(r, descsize);
            if (err != null) {
                return (null, error.As(fmt.Errorf("read desc failed: %v", err))!);
            }
            if (string(name) == string(noteName) && typ == noteType) {
                return (desc, error.As(null!)!);
            }
        }
    }    return (null, error.As(null!)!);
}

private static @string findshlib(ptr<Link> _addr_ctxt, @string shlib) {
    ref Link ctxt = ref _addr_ctxt.val;

    if (filepath.IsAbs(shlib)) {
        return shlib;
    }
    foreach (var (_, libdir) in ctxt.Libdir) {
        var libpath = filepath.Join(libdir, shlib);
        {
            var (_, err) = os.Stat(libpath);

            if (err == null) {
                return libpath;
            }

        }
    }    Errorf(null, "cannot find shared library: %s", shlib);
    return "";
}

private static void ldshlibsyms(ptr<Link> _addr_ctxt, @string shlib) {
    ref Link ctxt = ref _addr_ctxt.val;

    @string libpath = default;
    if (filepath.IsAbs(shlib)) {
        libpath = shlib;
        shlib = filepath.Base(shlib);
    }
    else
 {
        libpath = findshlib(_addr_ctxt, shlib);
        if (libpath == "") {
            return ;
        }
    }
    foreach (var (_, processedlib) in ctxt.Shlibs) {
        if (processedlib.Path == libpath) {
            return ;
        }
    }    if (ctxt.Debugvlog > 1) {
        ctxt.Logf("ldshlibsyms: found library with name %s at %s\n", shlib, libpath);
    }
    var (f, err) = elf.Open(libpath);
    if (err != null) {
        Errorf(null, "cannot open shared library: %s", libpath);
        return ;
    }
    var (hash, err) = readnote(_addr_f, ELF_NOTE_GO_NAME, ELF_NOTE_GOABIHASH_TAG);
    if (err != null) {
        Errorf(null, "cannot read ABI hash from shared library %s: %v", libpath, err);
        return ;
    }
    var (depsbytes, err) = readnote(_addr_f, ELF_NOTE_GO_NAME, ELF_NOTE_GODEPS_TAG);
    if (err != null) {
        Errorf(null, "cannot read dep list from shared library %s: %v", libpath, err);
        return ;
    }
    slice<@string> deps = default;
    foreach (var (_, dep) in strings.Split(string(depsbytes), "\n")) {
        if (dep == "") {
            continue;
        }
        if (!filepath.IsAbs(dep)) { 
            // If the dep can be interpreted as a path relative to the shlib
            // in which it was found, do that. Otherwise, we will leave it
            // to be resolved by libdir lookup.
            var abs = filepath.Join(filepath.Dir(libpath), dep);
            {
                var (_, err) = os.Stat(abs);

                if (err == null) {
                    dep = abs;
                }

            }
        }
        deps = append(deps, dep);
    }    var (syms, err) = f.DynamicSymbols();
    if (err != null) {
        Errorf(null, "cannot read symbols from shared library: %s", libpath);
        return ;
    }
    foreach (var (_, elfsym) in syms) {
        if (elf.ST_TYPE(elfsym.Info) == elf.STT_NOTYPE || elf.ST_TYPE(elfsym.Info) == elf.STT_SECTION) {
            continue;
        }
        nint ver = 0;
        var symname = elfsym.Name; // (unmangled) symbol name
        if (elf.ST_TYPE(elfsym.Info) == elf.STT_FUNC && strings.HasPrefix(elfsym.Name, "type.")) {
            ver = sym.SymVerABIInternal;
        }
        else if (buildcfg.Experiment.RegabiWrappers && elf.ST_TYPE(elfsym.Info) == elf.STT_FUNC) { 
            // Demangle the ABI name. Keep in sync with symtab.go:mangleABIName.
            if (strings.HasSuffix(elfsym.Name, ".abiinternal")) {
                ver = sym.SymVerABIInternal;
                symname = strings.TrimSuffix(elfsym.Name, ".abiinternal");
            }
            else if (strings.HasSuffix(elfsym.Name, ".abi0")) {
                ver = 0;
                symname = strings.TrimSuffix(elfsym.Name, ".abi0");
            }
        }
        var l = ctxt.loader;
        var s = l.LookupOrCreateSym(symname, ver); 

        // Because loadlib above loads all .a files before loading
        // any shared libraries, any non-dynimport symbols we find
        // that duplicate symbols already loaded should be ignored
        // (the symbols from the .a files "win").
        if (l.SymType(s) != 0 && l.SymType(s) != sym.SDYNIMPORT) {
            continue;
        }
        var su = l.MakeSymbolUpdater(s);
        su.SetType(sym.SDYNIMPORT);
        l.SetSymElfType(s, elf.ST_TYPE(elfsym.Info));
        su.SetSize(int64(elfsym.Size));
        if (elfsym.Section != elf.SHN_UNDEF) { 
            // Set .File for the library that actually defines the symbol.
            l.SetSymPkg(s, libpath); 

            // The decodetype_* functions in decodetype.go need access to
            // the type data.
            var sname = l.SymName(s);
            if (strings.HasPrefix(sname, "type.") && !strings.HasPrefix(sname, "type..")) {
                su.SetData(readelfsymboldata(_addr_ctxt, _addr_f, _addr_elfsym));
            }
        }
        if (symname != elfsym.Name) {
            l.SetSymExtname(s, elfsym.Name);
        }
        if (!buildcfg.Experiment.RegabiWrappers && elf.ST_TYPE(elfsym.Info) == elf.STT_FUNC && ver == 0) {
            var alias = ctxt.loader.LookupOrCreateSym(symname, sym.SymVerABIInternal);
            if (l.SymType(alias) != 0) {
                continue;
            }
            su = l.MakeSymbolUpdater(alias);
            su.SetType(sym.SABIALIAS);
            var (r, _) = su.AddRel(0); // type doesn't matter
            r.SetSym(s);
        }
    }    ctxt.Shlibs = append(ctxt.Shlibs, new Shlib(Path:libpath,Hash:hash,Deps:deps,File:f));
}

private static ptr<sym.Section> addsection(ptr<loader.Loader> _addr_ldr, ptr<sys.Arch> _addr_arch, ptr<sym.Segment> _addr_seg, @string name, nint rwx) {
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

private partial struct chain {
    public loader.Sym sym;
    public ptr<chain> up;
    public nint limit; // limit on entry to sym
}

private static bool haslinkregister(ptr<Link> _addr_ctxt) {
    ref Link ctxt = ref _addr_ctxt.val;

    return ctxt.FixedFrameSize() != 0;
}

private static nint callsize(ptr<Link> _addr_ctxt) {
    ref Link ctxt = ref _addr_ctxt.val;

    if (haslinkregister(_addr_ctxt)) {
        return 0;
    }
    return ctxt.Arch.RegSize;
}

private partial struct stkChk {
    public ptr<loader.Loader> ldr;
    public ptr<Link> ctxt;
    public loader.Sym morestack;
    public loader.Bitmap done;
}

// Walk the call tree and check that there is always enough stack space
// for the call frames, especially for a chain of nosplit functions.
private static void dostkcheck(this ptr<Link> _addr_ctxt) {
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
    if (buildcfg.GOARCH == "arm64") { 
        // need extra 8 bytes below SP to save FP
        ch.limit -= 8;
    }
    {
        var s__prev1 = s;

        foreach (var (_, __s) in ctxt.Textp) {
            s = __s;
            if (ldr.IsNoSplit(s)) {
                ch.sym = s;
                sc.check(_addr_ch, 0);
            }
        }
        s = s__prev1;
    }

    {
        var s__prev1 = s;

        foreach (var (_, __s) in ctxt.Textp) {
            s = __s;
            if (!ldr.IsNoSplit(s)) {
                ch.sym = s;
                sc.check(_addr_ch, 0);
            }
        }
        s = s__prev1;
    }
}

private static nint check(this ptr<stkChk> _addr_sc, ptr<chain> _addr_up, nint depth) {
    ref stkChk sc = ref _addr_sc.val;
    ref chain up = ref _addr_up.val;

    var limit = up.limit;
    var s = up.sym;
    var ldr = sc.ldr;
    var ctxt = sc.ctxt; 

    // Don't duplicate work: only need to consider each
    // function at top of safe zone once.
    var top = limit == objabi.StackLimit - callsize(_addr_ctxt);
    if (top) {
        if (sc.done.Has(s)) {
            return 0;
        }
        sc.done.Set(s);
    }
    if (depth > 500) {
        sc.ctxt.Errorf(s, "nosplit stack check too deep");
        sc.broke(up, 0);
        return -1;
    }
    if (ldr.AttrExternal(s)) { 
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
        return -1;
    }
    var info = ldr.FuncInfo(s);
    if (!info.Valid()) { // external function. see above.
        return -1;
    }
    if (limit < 0) {
        sc.broke(up, limit);
        return -1;
    }
    if (s == sc.morestack) {
        return 0;
    }
    ref chain ch = ref heap(out ptr<chain> _addr_ch);
    ch.up = up;

    if (!ldr.IsNoSplit(s)) { 
        // Ensure we have enough stack to call morestack.
        ch.limit = limit - callsize(_addr_ctxt);
        ch.sym = sc.morestack;
        if (sc.check(_addr_ch, depth + 1) < 0) {
            return -1;
        }
        if (!top) {
            return 0;
        }
        var locals = info.Locals();
        limit = objabi.StackLimit + int(locals) + int(ctxt.FixedFrameSize());
    }
    var relocs = ldr.Relocs(s);
    ref chain ch1 = ref heap(out ptr<chain> _addr_ch1);
    var pcsp = obj.NewPCIter(uint32(ctxt.Arch.MinLC));
    nint ri = 0;
    pcsp.Init(ldr.Data(info.Pcsp()));

    while (!pcsp.Done) { 
        // pcsp.value is in effect for [pcsp.pc, pcsp.nextpc).

        // Check stack size in effect for this span.
        if (int32(limit) - pcsp.Value < 0) {
            sc.broke(up, int(int32(limit) - pcsp.Value));
            return -1;
        pcsp.Next();
        }
        while (ri < relocs.Count()) {
            var r = relocs.At(ri);
            if (uint32(r.Off()) >= pcsp.NextPC) {
                break;
            ri++;
            }
            var t = r.Type();

            if (t.IsDirectCall()) 
                ch.limit = int(int32(limit) - pcsp.Value - int32(callsize(_addr_ctxt)));
                ch.sym = r.Sym();
                if (sc.check(_addr_ch, depth + 1) < 0) {
                    return -1;
                } 

                // Indirect call. Assume it is a call to a splitting function,
                // so we have to make sure it can call morestack.
                // Arrange the data structures to report both calls, so that
                // if there is an error, stkprint shows all the steps involved.
            else if (t == objabi.R_CALLIND) 
                ch.limit = int(int32(limit) - pcsp.Value - int32(callsize(_addr_ctxt)));
                ch.sym = 0;
                ch1.limit = ch.limit - callsize(_addr_ctxt); // for morestack in called prologue
                _addr_ch1.up = _addr_ch;
                ch1.up = ref _addr_ch1.up.val;
                ch1.sym = sc.morestack;
                if (sc.check(_addr_ch1, depth + 2) < 0) {
                    return -1;
                }
                    }
    }

    return 0;
}

private static void broke(this ptr<stkChk> _addr_sc, ptr<chain> _addr_ch, nint limit) {
    ref stkChk sc = ref _addr_sc.val;
    ref chain ch = ref _addr_ch.val;

    sc.ctxt.Errorf(ch.sym, "nosplit stack overflow");
    sc.print(ch, limit);
}

private static void print(this ptr<stkChk> _addr_sc, ptr<chain> _addr_ch, nint limit) {
    ref stkChk sc = ref _addr_sc.val;
    ref chain ch = ref _addr_ch.val;

    var ldr = sc.ldr;
    var ctxt = sc.ctxt;
    @string name = default;
    if (ch.sym != 0) {
        name = fmt.Sprintf("%s<%d>", ldr.SymName(ch.sym), ldr.SymVersion(ch.sym));
        if (ldr.IsNoSplit(ch.sym)) {
            name += " (nosplit)";
        }
    }
    else
 {
        name = "function pointer";
    }
    if (ch.up == null) { 
        // top of chain. ch.sym != 0.
        if (ldr.IsNoSplit(ch.sym)) {
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
        if (!haslinkregister(_addr_ctxt)) {
            fmt.Printf("\t%d\ton entry to %s\n", ch.limit, name);
        }
    }
    if (ch.limit != limit) {
        fmt.Printf("\t%d\tafter %s uses %d\n", limit, name, ch.limit - limit);
    }
}

private static void usage() {
    fmt.Fprintf(os.Stderr, "usage: link [options] main.o\n");
    objabi.Flagprint(os.Stderr);
    Exit(2);
}

public partial struct SymbolType { // : sbyte
} // TODO: after genasmsym is gone, maybe rename to plan9typeChar or something

 
// see also https://9p.io/magic/man2html/1/nm
public static readonly SymbolType TextSym = 'T';
public static readonly SymbolType DataSym = 'D';
public static readonly SymbolType BSSSym = 'B';
public static readonly SymbolType UndefinedSym = 'U';
public static readonly SymbolType TLSSym = 't';
public static readonly SymbolType FrameSym = 'm';
public static readonly SymbolType ParamSym = 'p';
public static readonly SymbolType AutoSym = 'a'; 

// Deleted auto (not a real sym, just placeholder for type)
public static readonly char DeletedAutoSym = 'x';

// defineInternal defines a symbol used internally by the go runtime.
private static loader.Sym defineInternal(this ptr<Link> _addr_ctxt, @string p, sym.SymKind t) {
    ref Link ctxt = ref _addr_ctxt.val;

    var s = ctxt.loader.CreateSymForUpdate(p, 0);
    s.SetType(t);
    s.SetSpecial(true);
    s.SetLocal(true);
    return s.Sym();
}

private static loader.Sym xdefine(this ptr<Link> _addr_ctxt, @string p, sym.SymKind t, long v) {
    ref Link ctxt = ref _addr_ctxt.val;

    var s = ctxt.defineInternal(p, t);
    ctxt.loader.SetSymValue(s, v);
    return s;
}

private static long datoff(ptr<loader.Loader> _addr_ldr, loader.Sym s, long addr) {
    ref loader.Loader ldr = ref _addr_ldr.val;

    if (uint64(addr) >= Segdata.Vaddr) {
        return int64(uint64(addr) - Segdata.Vaddr + Segdata.Fileoff);
    }
    if (uint64(addr) >= Segtext.Vaddr) {
        return int64(uint64(addr) - Segtext.Vaddr + Segtext.Fileoff);
    }
    ldr.Errorf(s, "invalid datoff %#x", addr);
    return 0;
}

public static long Entryvalue(ptr<Link> _addr_ctxt) {
    ref Link ctxt = ref _addr_ctxt.val;

    var a = flagEntrySymbol.val;
    if (a[0] >= '0' && a[0] <= '9') {
        return atolwhex(a);
    }
    var ldr = ctxt.loader;
    var s = ldr.Lookup(a, 0);
    var st = ldr.SymType(s);
    if (st == 0) {
        return FlagTextAddr.val;
    }
    if (!ctxt.IsAIX() && st != sym.STEXT) {
        ldr.Errorf(s, "entry not text");
    }
    return ldr.SymValue(s);
}

private static void callgraph(this ptr<Link> _addr_ctxt) {
    ref Link ctxt = ref _addr_ctxt.val;

    if (!FlagC.val) {
        return ;
    }
    var ldr = ctxt.loader;
    foreach (var (_, s) in ctxt.Textp) {
        var relocs = ldr.Relocs(s);
        for (nint i = 0; i < relocs.Count(); i++) {
            var r = relocs.At(i);
            var rs = r.Sym();
            if (rs == 0) {
                continue;
            }
            if (r.Type().IsDirectCall() && (ldr.SymType(rs) == sym.STEXT || ldr.SymType(rs) == sym.SABIALIAS)) {
                ctxt.Logf("%s calls %s\n", ldr.SymName(s), ldr.SymName(rs));
            }
        }
    }
}

public static long Rnd(long v, long r) {
    if (r <= 0) {
        return v;
    }
    v += r - 1;
    var c = v % r;
    if (c < 0) {
        c += r;
    }
    v -= c;
    return v;
}

private static nint bgetc(ptr<bio.Reader> _addr_r) {
    ref bio.Reader r = ref _addr_r.val;

    var (c, err) = r.ReadByte();
    if (err != null) {
        if (err != io.EOF) {
            log.Fatalf("reading input: %v", err);
        }
        return -1;
    }
    return int(c);
}

private partial struct markKind { // : byte
} // for postorder traversal
private static readonly markKind _ = iota;
private static readonly var visiting = 0;
private static readonly var visited = 1;

private static slice<ptr<sym.Library>> postorder(slice<ptr<sym.Library>> libs) {
    ref var order = ref heap(make_slice<ptr<sym.Library>>(0, len(libs)), out ptr<var> _addr_order); // hold the result
    var mark = make_map<ptr<sym.Library>, markKind>(len(libs));
    foreach (var (_, lib) in libs) {
        dfs(_addr_lib, mark, _addr_order);
    }    return order;
}

private static void dfs(ptr<sym.Library> _addr_lib, map<ptr<sym.Library>, markKind> mark, ptr<slice<ptr<sym.Library>>> _addr_order) => func((_, panic, _) => {
    ref sym.Library lib = ref _addr_lib.val;
    ref slice<ptr<sym.Library>> order = ref _addr_order.val;

    if (mark[lib] == visited) {
        return ;
    }
    if (mark[lib] == visiting) {
        panic("found import cycle while visiting " + lib.Pkg);
    }
    mark[lib] = visiting;
    foreach (var (_, i) in lib.Imports) {
        dfs(_addr_i, mark, _addr_order);
    }    mark[lib] = visited;
    order.val = append(order.val, lib);
});

public static int ElfSymForReloc(ptr<Link> _addr_ctxt, loader.Sym s) {
    ref Link ctxt = ref _addr_ctxt.val;
 
    // If putelfsym created a local version of this symbol, use that in all
    // relocations.
    var les = ctxt.loader.SymLocalElfSym(s);
    if (les != 0) {
        return les;
    }
    else
 {
        return ctxt.loader.SymElfSym(s);
    }
}

public static void AddGotSym(ptr<Target> _addr_target, ptr<loader.Loader> _addr_ldr, ptr<ArchSyms> _addr_syms, loader.Sym s, uint elfRelocTyp) {
    ref Target target = ref _addr_target.val;
    ref loader.Loader ldr = ref _addr_ldr.val;
    ref ArchSyms syms = ref _addr_syms.val;

    if (ldr.SymGot(s) >= 0) {
        return ;
    }
    Adddynsym(ldr, target, syms, s);
    var got = ldr.MakeSymbolUpdater(syms.GOT);
    ldr.SetGot(s, int32(got.Size()));
    got.AddUint(target.Arch, 0);

    if (target.IsElf()) {
        if (target.Arch.PtrSize == 8) {
            var rela = ldr.MakeSymbolUpdater(syms.Rela);
            rela.AddAddrPlus(target.Arch, got.Sym(), int64(ldr.SymGot(s)));
            rela.AddUint64(target.Arch, elf.R_INFO(uint32(ldr.SymDynid(s)), elfRelocTyp));
            rela.AddUint64(target.Arch, 0);
        }
        else
 {
            var rel = ldr.MakeSymbolUpdater(syms.Rel);
            rel.AddAddrPlus(target.Arch, got.Sym(), int64(ldr.SymGot(s)));
            rel.AddUint32(target.Arch, elf.R_INFO32(uint32(ldr.SymDynid(s)), elfRelocTyp));
        }
    }
    else if (target.IsDarwin()) {
        var leg = ldr.MakeSymbolUpdater(syms.LinkEditGOT);
        leg.AddUint32(target.Arch, uint32(ldr.SymDynid(s)));
        if (target.IsPIE() && target.IsInternal()) { 
            // Mach-O relocations are a royal pain to lay out.
            // They use a compact stateful bytecode representation.
            // Here we record what are needed and encode them later.
            MachoAddBind(int64(ldr.SymGot(s)), s);
        }
    }
    else
 {
        ldr.Errorf(s, "addgotsym: unsupported binary format");
    }
}

} // end ld_package
