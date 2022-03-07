// Inferno utils/6l/span.c
// https://bitbucket.org/inferno-os/inferno-os/src/master/utils/6l/span.c
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

// package ld -- go2cs converted at 2022 March 06 23:22:25 UTC
// import "cmd/link/internal/ld" ==> using ld = go.cmd.link.@internal.ld_package
// Original source: C:\Program Files\Go\src\cmd\link\internal\ld\symtab.go
using obj = go.cmd.@internal.obj_package;
using objabi = go.cmd.@internal.objabi_package;
using loader = go.cmd.link.@internal.loader_package;
using sym = go.cmd.link.@internal.sym_package;
using elf = go.debug.elf_package;
using fmt = go.fmt_package;
using buildcfg = go.@internal.buildcfg_package;
using filepath = go.path.filepath_package;
using strings = go.strings_package;
using System;


namespace go.cmd.link.@internal;

public static partial class ld_package {

    // Symbol table.
private static nint putelfstr(@string s) {
    if (len(Elfstrdat) == 0 && s != "") { 
        // first entry must be empty string
        putelfstr("");

    }
    var off = len(Elfstrdat);
    Elfstrdat = append(Elfstrdat, s);
    Elfstrdat = append(Elfstrdat, 0);
    return off;

}

private static void putelfsyment(ptr<OutBuf> _addr_@out, nint off, long addr, long size, byte info, elf.SectionIndex shndx, nint other) {
    ref OutBuf @out = ref _addr_@out.val;

    if (elf64) {
        @out.Write32(uint32(off));
        @out.Write8(info);
        @out.Write8(uint8(other));
        @out.Write16(uint16(shndx));
        @out.Write64(uint64(addr));
        @out.Write64(uint64(size));
        symSize += ELF64SYMSIZE;
    }
    else
 {
        @out.Write32(uint32(off));
        @out.Write32(uint32(addr));
        @out.Write32(uint32(size));
        @out.Write8(info);
        @out.Write8(uint8(other));
        @out.Write16(uint16(shndx));
        symSize += ELF32SYMSIZE;
    }
}

private static void putelfsym(ptr<Link> _addr_ctxt, loader.Sym x, elf.SymType typ, elf.SymBind curbind) {
    ref Link ctxt = ref _addr_ctxt.val;

    var ldr = ctxt.loader;
    var addr = ldr.SymValue(x);
    var size = ldr.SymSize(x);

    var xo = x;
    if (ldr.OuterSym(x) != 0) {
        xo = ldr.OuterSym(x);
    }
    var xot = ldr.SymType(xo);
    var xosect = ldr.SymSect(xo);

    elf.SectionIndex elfshnum = default;
    if (xot == sym.SDYNIMPORT || xot == sym.SHOSTOBJ || xot == sym.SUNDEFEXT) {
        elfshnum = elf.SHN_UNDEF;
        size = 0;
    }
    else
 {
        if (xosect == null) {
            ldr.Errorf(x, "missing section in putelfsym");
            return ;
        }
        if (xosect.Elfsect == null) {
            ldr.Errorf(x, "missing ELF section in putelfsym");
            return ;
        }
        elfshnum = xosect.Elfsect._<ptr<ElfShdr>>().shnum;

    }
    var sname = ldr.SymExtname(x);
    sname = mangleABIName(_addr_ctxt, _addr_ldr, x, sname); 

    // One pass for each binding: elf.STB_LOCAL, elf.STB_GLOBAL,
    // maybe one day elf.STB_WEAK.
    var bind = elf.STB_GLOBAL;
    if (ldr.IsFileLocal(x) && !isStaticTmp(sname) || ldr.AttrVisibilityHidden(x) || ldr.AttrLocal(x)) { 
        // Static tmp is package local, but a package can be shared among multiple DSOs.
        // They need to have a single view of the static tmp that are writable.
        bind = elf.STB_LOCAL;

    }
    if (!ctxt.DynlinkingGo() && ctxt.IsExternal() && !ldr.AttrCgoExportStatic(x) && elfshnum != elf.SHN_UNDEF) {
        bind = elf.STB_LOCAL;
    }
    if (ctxt.LinkMode == LinkExternal && elfshnum != elf.SHN_UNDEF) {
        addr -= int64(xosect.Vaddr);
    }
    var other = int(elf.STV_DEFAULT);
    if (ldr.AttrVisibilityHidden(x)) { 
        // TODO(mwhudson): We only set AttrVisibilityHidden in ldelf, i.e. when
        // internally linking. But STV_HIDDEN visibility only matters in object
        // files and shared libraries, and as we are a long way from implementing
        // internal linking for shared libraries and only create object files when
        // externally linking, I don't think this makes a lot of sense.
        other = int(elf.STV_HIDDEN);

    }
    if (ctxt.IsPPC64() && typ == elf.STT_FUNC && ldr.AttrShared(x) && ldr.SymName(x) != "runtime.duffzero" && ldr.SymName(x) != "runtime.duffcopy") { 
        // On ppc64 the top three bits of the st_other field indicate how
        // many instructions separate the global and local entry points. In
        // our case it is two instructions, indicated by the value 3.
        // The conditions here match those in preprocess in
        // cmd/internal/obj/ppc64/obj9.go, which is where the
        // instructions are inserted.
        other |= 3 << 5;

    }
    if (!ctxt.DynlinkingGo()) { 
        // Rewrite · to . for ASCII-only tools like DTrace (sigh)
        sname = strings.Replace(sname, "·", ".", -1);

    }
    if (ctxt.DynlinkingGo() && bind == elf.STB_GLOBAL && curbind == elf.STB_LOCAL && ldr.SymType(x) == sym.STEXT) { 
        // When dynamically linking, we want references to functions defined
        // in this module to always be to the function object, not to the
        // PLT. We force this by writing an additional local symbol for every
        // global function symbol and making all relocations against the
        // global symbol refer to this local symbol instead (see
        // (*sym.Symbol).ElfsymForReloc). This is approximately equivalent to the
        // ELF linker -Bsymbolic-functions option, but that is buggy on
        // several platforms.
        putelfsyment(_addr_ctxt.Out, putelfstr("local." + sname), addr, size, elf.ST_INFO(elf.STB_LOCAL, typ), elfshnum, other);
        ldr.SetSymLocalElfSym(x, int32(ctxt.numelfsym));
        ctxt.numelfsym++;
        return ;

    }
    else if (bind != curbind) {
        return ;
    }
    putelfsyment(_addr_ctxt.Out, putelfstr(sname), addr, size, elf.ST_INFO(bind, typ), elfshnum, other);
    ldr.SetSymElfSym(x, int32(ctxt.numelfsym));
    ctxt.numelfsym++;

}

private static void putelfsectionsym(ptr<Link> _addr_ctxt, ptr<OutBuf> _addr_@out, loader.Sym s, elf.SectionIndex shndx) {
    ref Link ctxt = ref _addr_ctxt.val;
    ref OutBuf @out = ref _addr_@out.val;

    putelfsyment(_addr_out, 0, 0, 0, elf.ST_INFO(elf.STB_LOCAL, elf.STT_SECTION), shndx, 0);
    ctxt.loader.SetSymElfSym(s, int32(ctxt.numelfsym));
    ctxt.numelfsym++;
}

private static void genelfsym(ptr<Link> _addr_ctxt, elf.SymBind elfbind) => func((_, panic, _) => {
    ref Link ctxt = ref _addr_ctxt.val;

    var ldr = ctxt.loader; 

    // runtime.text marker symbol(s).
    var s = ldr.Lookup("runtime.text", 0);
    putelfsym(_addr_ctxt, s, elf.STT_FUNC, elfbind);
    foreach (var (k, sect) in Segtext.Sections[(int)1..]) {
        var n = k + 1;
        if (sect.Name != ".text" || (ctxt.IsAIX() && ctxt.IsExternal())) { 
            // On AIX, runtime.text.X are symbols already in the symtab.
            break;

        }
        s = ldr.Lookup(fmt.Sprintf("runtime.text.%d", n), 0);
        if (s == 0) {
            break;
        }
        if (ldr.SymType(s) != sym.STEXT) {
            panic("unexpected type for runtime.text symbol");
        }
        putelfsym(_addr_ctxt, s, elf.STT_FUNC, elfbind);

    }    {
        var s__prev1 = s;

        foreach (var (_, __s) in ctxt.Textp) {
            s = __s;
            putelfsym(_addr_ctxt, s, elf.STT_FUNC, elfbind);
        }
        s = s__prev1;
    }

    s = ldr.Lookup("runtime.etext", 0);
    if (ldr.SymType(s) == sym.STEXT) {
        putelfsym(_addr_ctxt, s, elf.STT_FUNC, elfbind);
    }
    Func<loader.Sym, bool> shouldBeInSymbolTable = s => {
        if (ldr.AttrNotInSymbolTable(s)) {
            return false;
        }
        var sn = ldr.SymName(s);
        if ((sn == "" || sn[0] == '.') && ldr.IsFileLocal(s)) {
            panic(fmt.Sprintf("unexpected file local symbol %d %s<%d>\n", s, sn, ldr.SymVersion(s)));
        }
        if ((sn == "" || sn[0] == '.') && !ldr.IsFileLocal(s)) {
            return false;
        }
        return true;

    }; 

    // Data symbols.
    {
        var s__prev1 = s;

        for (s = loader.Sym(1); s < loader.Sym(ldr.NSym()); s++) {
            if (!ldr.AttrReachable(s)) {
                continue;
            }
            var st = ldr.SymType(s);
            if (st >= sym.SELFRXSECT && st < sym.SXREF) {
                var typ = elf.STT_OBJECT;
                if (st == sym.STLSBSS) {
                    if (ctxt.IsInternal()) {
                        continue;
                    }
                    typ = elf.STT_TLS;
                }
                if (!shouldBeInSymbolTable(s)) {
                    continue;
                }
                putelfsym(_addr_ctxt, s, typ, elfbind);
                continue;
            }
            if (st == sym.SHOSTOBJ || st == sym.SDYNIMPORT || st == sym.SUNDEFEXT) {
                putelfsym(_addr_ctxt, s, ldr.SymElfType(s), elfbind);
            }
        }

        s = s__prev1;
    }

});

private static void asmElfSym(ptr<Link> _addr_ctxt) {
    ref Link ctxt = ref _addr_ctxt.val;

    // the first symbol entry is reserved
    putelfsyment(_addr_ctxt.Out, 0, 0, 0, elf.ST_INFO(elf.STB_LOCAL, elf.STT_NOTYPE), 0, 0);

    dwarfaddelfsectionsyms(ctxt); 

    // Some linkers will add a FILE sym if one is not present.
    // Avoid having the working directory inserted into the symbol table.
    // It is added with a name to avoid problems with external linking
    // encountered on some versions of Solaris. See issue #14957.
    putelfsyment(_addr_ctxt.Out, putelfstr("go.go"), 0, 0, elf.ST_INFO(elf.STB_LOCAL, elf.STT_FILE), elf.SHN_ABS, 0);
    ctxt.numelfsym++;

    elf.SymBind bindings = new slice<elf.SymBind>(new elf.SymBind[] { elf.STB_LOCAL, elf.STB_GLOBAL });
    foreach (var (_, elfbind) in bindings) {
        if (elfbind == elf.STB_GLOBAL) {
            elfglobalsymndx = ctxt.numelfsym;
        }
        genelfsym(_addr_ctxt, elfbind);

    }
}

private static void putplan9sym(ptr<Link> _addr_ctxt, ptr<loader.Loader> _addr_ldr, loader.Sym s, SymbolType @char) {
    ref Link ctxt = ref _addr_ctxt.val;
    ref loader.Loader ldr = ref _addr_ldr.val;

    var t = int(char);
    if (ldr.IsFileLocal(s)) {
        t += 'a' - 'A';
    }
    nint l = 4;
    var addr = ldr.SymValue(s);
    if (ctxt.IsAMD64() && !flag8) {
        ctxt.Out.Write32b(uint32(addr >> 32));
        l = 8;
    }
    ctxt.Out.Write32b(uint32(addr));
    ctxt.Out.Write8(uint8(t + 0x80));    /* 0x80 is variable length */

    var name = ldr.SymName(s);
    ctxt.Out.WriteString(name);
    ctxt.Out.Write8(0);

    symSize += int32(l) + 1 + int32(len(name)) + 1;

}

private static void asmbPlan9Sym(ptr<Link> _addr_ctxt) {
    ref Link ctxt = ref _addr_ctxt.val;

    var ldr = ctxt.loader; 

    // Add special runtime.text and runtime.etext symbols.
    var s = ldr.Lookup("runtime.text", 0);
    if (ldr.SymType(s) == sym.STEXT) {
        putplan9sym(_addr_ctxt, _addr_ldr, s, TextSym);
    }
    s = ldr.Lookup("runtime.etext", 0);
    if (ldr.SymType(s) == sym.STEXT) {
        putplan9sym(_addr_ctxt, _addr_ldr, s, TextSym);
    }
    {
        var s__prev1 = s;

        foreach (var (_, __s) in ctxt.Textp) {
            s = __s;
            putplan9sym(_addr_ctxt, _addr_ldr, s, TextSym);
        }
        s = s__prev1;
    }

    Func<loader.Sym, bool> shouldBeInSymbolTable = s => {
        if (ldr.AttrNotInSymbolTable(s)) {
            return false;
        }
        var name = ldr.RawSymName(s); // TODO: try not to read the name
        if (name == "" || name[0] == '.') {
            return false;
        }
        return true;

    }; 

    // Add data symbols and external references.
    {
        var s__prev1 = s;

        for (s = loader.Sym(1); s < loader.Sym(ldr.NSym()); s++) {
            if (!ldr.AttrReachable(s)) {
                continue;
            }
            var t = ldr.SymType(s);
            if (t >= sym.SELFRXSECT && t < sym.SXREF) { // data sections handled in dodata
                if (t == sym.STLSBSS) {
                    continue;
                }

                if (!shouldBeInSymbolTable(s)) {
                    continue;
                }

                var @char = DataSym;
                if (t == sym.SBSS || t == sym.SNOPTRBSS) {
                    char = BSSSym;
                }

                putplan9sym(_addr_ctxt, _addr_ldr, s, char);

            }

        }

        s = s__prev1;
    }

}

private partial struct byPkg { // : slice<ptr<sym.Library>>
}

private static nint Len(this byPkg libs) {
    return len(libs);
}

private static bool Less(this byPkg libs, nint a, nint b) {
    return libs[a].Pkg < libs[b].Pkg;
}

private static void Swap(this byPkg libs, nint a, nint b) {
    (libs[a], libs[b]) = (libs[b], libs[a]);
}

// Create a table with information on the text sections.
// Return the symbol of the table, and number of sections.
private static (loader.Sym, uint) textsectionmap(ptr<Link> _addr_ctxt) {
    loader.Sym _p0 = default;
    uint _p0 = default;
    ref Link ctxt = ref _addr_ctxt.val;

    var ldr = ctxt.loader;
    var t = ldr.CreateSymForUpdate("runtime.textsectionmap", 0);
    t.SetType(sym.SRODATA);
    var nsections = int64(0);

    {
        var sect__prev1 = sect;

        foreach (var (_, __sect) in Segtext.Sections) {
            sect = __sect;
            if (sect.Name == ".text") {
                nsections++;
            }
            else
 {
                break;
            }

        }
        sect = sect__prev1;
    }

    t.Grow(3 * nsections * int64(ctxt.Arch.PtrSize));

    var off = int64(0);
    nint n = 0; 

    // The vaddr for each text section is the difference between the section's
    // Vaddr and the Vaddr for the first text section as determined at compile
    // time.

    // The symbol for the first text section is named runtime.text as before.
    // Additional text sections are named runtime.text.n where n is the
    // order of creation starting with 1. These symbols provide the section's
    // address after relocation by the linker.

    var textbase = Segtext.Sections[0].Vaddr;
    {
        var sect__prev1 = sect;

        foreach (var (_, __sect) in Segtext.Sections) {
            sect = __sect;
            if (sect.Name != ".text") {
                break;
            }
            off = t.SetUint(ctxt.Arch, off, sect.Vaddr - textbase);
            off = t.SetUint(ctxt.Arch, off, sect.Length);
            if (n == 0) {
                var s = ldr.Lookup("runtime.text", 0);
                if (s == 0) {
                    ctxt.Errorf(s, "Unable to find symbol runtime.text\n");
                }
                off = t.SetAddr(ctxt.Arch, off, s);
            }
            else
 {
                s = ldr.Lookup(fmt.Sprintf("runtime.text.%d", n), 0);
                if (s == 0) {
                    ctxt.Errorf(s, "Unable to find symbol runtime.text.%d\n", n);
                }
                off = t.SetAddr(ctxt.Arch, off, s);
            }

            n++;

        }
        sect = sect__prev1;
    }

    return (t.Sym(), uint32(n));

}

private static slice<sym.SymKind> symtab(this ptr<Link> _addr_ctxt, ptr<pclntab> _addr_pcln) => func((_, panic, _) => {
    ref Link ctxt = ref _addr_ctxt.val;
    ref pclntab pcln = ref _addr_pcln.val;

    var ldr = ctxt.loader;

    if (!ctxt.IsAIX()) {

        if (ctxt.BuildMode == BuildModeCArchive || ctxt.BuildMode == BuildModeCShared) 
            var s = ldr.Lookup(flagEntrySymbol.val, sym.SymVerABI0);
            if (s != 0) {
                addinitarrdata(ctxt, ldr, s);
            }
        
    }
    ctxt.xdefine("runtime.rodata", sym.SRODATA, 0);
    ctxt.xdefine("runtime.erodata", sym.SRODATA, 0);
    ctxt.xdefine("runtime.types", sym.SRODATA, 0);
    ctxt.xdefine("runtime.etypes", sym.SRODATA, 0);
    ctxt.xdefine("runtime.noptrdata", sym.SNOPTRDATA, 0);
    ctxt.xdefine("runtime.enoptrdata", sym.SNOPTRDATA, 0);
    ctxt.xdefine("runtime.data", sym.SDATA, 0);
    ctxt.xdefine("runtime.edata", sym.SDATA, 0);
    ctxt.xdefine("runtime.bss", sym.SBSS, 0);
    ctxt.xdefine("runtime.ebss", sym.SBSS, 0);
    ctxt.xdefine("runtime.noptrbss", sym.SNOPTRBSS, 0);
    ctxt.xdefine("runtime.enoptrbss", sym.SNOPTRBSS, 0);
    ctxt.xdefine("runtime.end", sym.SBSS, 0);
    ctxt.xdefine("runtime.epclntab", sym.SRODATA, 0);
    ctxt.xdefine("runtime.esymtab", sym.SRODATA, 0); 

    // garbage collection symbols
    s = ldr.CreateSymForUpdate("runtime.gcdata", 0);
    s.SetType(sym.SRODATA);
    s.SetSize(0);
    ctxt.xdefine("runtime.egcdata", sym.SRODATA, 0);

    s = ldr.CreateSymForUpdate("runtime.gcbss", 0);
    s.SetType(sym.SRODATA);
    s.SetSize(0);
    ctxt.xdefine("runtime.egcbss", sym.SRODATA, 0); 

    // pseudo-symbols to mark locations of type, string, and go string data.
    loader.Sym symtype = default;    loader.Sym symtyperel = default;

    if (!ctxt.DynlinkingGo()) {
        if (ctxt.UseRelro() && (ctxt.BuildMode == BuildModeCArchive || ctxt.BuildMode == BuildModeCShared || ctxt.BuildMode == BuildModePIE)) {
            s = ldr.CreateSymForUpdate("type.*", 0);
            s.SetType(sym.STYPE);
            s.SetSize(0);
            symtype = s.Sym();

            s = ldr.CreateSymForUpdate("typerel.*", 0);
            s.SetType(sym.STYPERELRO);
            s.SetSize(0);
            symtyperel = s.Sym();
        }
        else
 {
            s = ldr.CreateSymForUpdate("type.*", 0);
            s.SetType(sym.STYPE);
            s.SetSize(0);
            symtype = s.Sym();
            symtyperel = s.Sym();
        }
        setCarrierSym(sym.STYPE, symtype);
        setCarrierSym(sym.STYPERELRO, symtyperel);

    }
    Func<@string, sym.SymKind, loader.Sym> groupSym = (name, t) => {
        s = ldr.CreateSymForUpdate(name, 0);
        s.SetType(t);
        s.SetSize(0);
        s.SetLocal(true);
        setCarrierSym(t, s.Sym());
        return s.Sym();
    };
    var symgostring = groupSym("go.string.*", sym.SGOSTRING);    var symgofunc = groupSym("go.func.*", sym.SGOFUNC);    var symgcbits = groupSym("runtime.gcbits.*", sym.SGCBITS);

    loader.Sym symgofuncrel = default;
    if (!ctxt.DynlinkingGo()) {
        if (ctxt.UseRelro()) {
            symgofuncrel = groupSym("go.funcrel.*", sym.SGOFUNCRELRO);
        }
        else
 {
            symgofuncrel = symgofunc;
        }
    }
    var symt = ldr.CreateSymForUpdate("runtime.symtab", 0);
    symt.SetType(sym.SSYMTAB);
    symt.SetSize(0);
    symt.SetLocal(true); 

    // assign specific types so that they sort together.
    // within a type they sort by size, so the .* symbols
    // just defined above will be first.
    // hide the specific symbols.
    var nsym = loader.Sym(ldr.NSym());
    var symGroupType = make_slice<sym.SymKind>(nsym);
    {
        var s__prev1 = s;

        for (s = loader.Sym(1); s < nsym; s++) {
            if (!ctxt.IsExternal() && ldr.IsFileLocal(s) && !ldr.IsFromAssembly(s) && ldr.SymPkg(s) != "") {
                ldr.SetAttrNotInSymbolTable(s, true);
            }
            if (!ldr.AttrReachable(s) || ldr.AttrSpecial(s) || (ldr.SymType(s) != sym.SRODATA && ldr.SymType(s) != sym.SGOFUNC)) {
                continue;
            }
            var name = ldr.SymName(s);

            if (strings.HasPrefix(name, "type.")) 
                if (!ctxt.DynlinkingGo()) {
                    ldr.SetAttrNotInSymbolTable(s, true);
                }
                if (ctxt.UseRelro()) {
                    symGroupType[s] = sym.STYPERELRO;
                    if (symtyperel != 0) {
                        ldr.SetCarrierSym(s, symtyperel);
                    }
                }
                else
 {
                    symGroupType[s] = sym.STYPE;
                    if (symtyperel != 0) {
                        ldr.SetCarrierSym(s, symtype);
                    }
                }

            else if (strings.HasPrefix(name, "go.importpath.") && ctxt.UseRelro()) 
                // Keep go.importpath symbols in the same section as types and
                // names, as they can be referred to by a section offset.
                symGroupType[s] = sym.STYPERELRO;
            else if (strings.HasPrefix(name, "go.string.")) 
                symGroupType[s] = sym.SGOSTRING;
                ldr.SetAttrNotInSymbolTable(s, true);
                ldr.SetCarrierSym(s, symgostring);
            else if (strings.HasPrefix(name, "runtime.gcbits.")) 
                symGroupType[s] = sym.SGCBITS;
                ldr.SetAttrNotInSymbolTable(s, true);
                ldr.SetCarrierSym(s, symgcbits);
            else if (strings.HasSuffix(name, "·f")) 
                if (!ctxt.DynlinkingGo()) {
                    ldr.SetAttrNotInSymbolTable(s, true);
                }
                if (ctxt.UseRelro()) {
                    symGroupType[s] = sym.SGOFUNCRELRO;
                    if (symgofuncrel != 0) {
                        ldr.SetCarrierSym(s, symgofuncrel);
                    }
                }
                else
 {
                    symGroupType[s] = sym.SGOFUNC;
                    ldr.SetCarrierSym(s, symgofunc);
                }

            else if (strings.HasPrefix(name, "gcargs.") || strings.HasPrefix(name, "gclocals.") || strings.HasPrefix(name, "gclocals·") || ldr.SymType(s) == sym.SGOFUNC && s != symgofunc || strings.HasSuffix(name, ".opendefer") || strings.HasSuffix(name, ".arginfo0") || strings.HasSuffix(name, ".arginfo1")) 
                symGroupType[s] = sym.SGOFUNC;
                ldr.SetAttrNotInSymbolTable(s, true);
                ldr.SetCarrierSym(s, symgofunc);
                var align = int32(4);
                {
                    var a = ldr.SymAlign(s);

                    if (a < align) {
                        ldr.SetSymAlign(s, align);
                    }
                    else
 {
                        align = a;
                    }

                }

                liveness += (ldr.SymSize(s) + int64(align) - 1) & ~(int64(align) - 1);
            
        }

        s = s__prev1;
    }

    if (ctxt.BuildMode == BuildModeShared) {
        var abihashgostr = ldr.CreateSymForUpdate("go.link.abihash." + filepath.Base(flagOutfile.val), 0);
        abihashgostr.SetType(sym.SRODATA);
        var hashsym = ldr.LookupOrCreateSym("go.link.abihashbytes", 0);
        abihashgostr.AddAddr(ctxt.Arch, hashsym);
        abihashgostr.AddUint(ctxt.Arch, uint64(ldr.SymSize(hashsym)));
    }
    if (ctxt.BuildMode == BuildModePlugin || ctxt.CanUsePlugins()) {
        {
            var l__prev1 = l;

            foreach (var (_, __l) in ctxt.Library) {
                l = __l;
                s = ldr.CreateSymForUpdate("go.link.pkghashbytes." + l.Pkg, 0);
                s.SetType(sym.SRODATA);
                s.SetSize(int64(len(l.Fingerprint)));
                s.SetData(l.Fingerprint[..]);
                var str = ldr.CreateSymForUpdate("go.link.pkghash." + l.Pkg, 0);
                str.SetType(sym.SRODATA);
                str.AddAddr(ctxt.Arch, s.Sym());
                str.AddUint(ctxt.Arch, uint64(len(l.Fingerprint)));
            }

            l = l__prev1;
        }
    }
    var (textsectionmapSym, nsections) = textsectionmap(_addr_ctxt); 

    // Information about the layout of the executable image for the
    // runtime to use. Any changes here must be matched by changes to
    // the definition of moduledata in runtime/symtab.go.
    // This code uses several global variables that are set by pcln.go:pclntab.
    var moduledata = ldr.MakeSymbolUpdater(ctxt.Moduledata); 
    // The pcHeader
    moduledata.AddAddr(ctxt.Arch, pcln.pcheader); 
    // The function name slice
    moduledata.AddAddr(ctxt.Arch, pcln.funcnametab);
    moduledata.AddUint(ctxt.Arch, uint64(ldr.SymSize(pcln.funcnametab)));
    moduledata.AddUint(ctxt.Arch, uint64(ldr.SymSize(pcln.funcnametab))); 
    // The cutab slice
    moduledata.AddAddr(ctxt.Arch, pcln.cutab);
    moduledata.AddUint(ctxt.Arch, uint64(ldr.SymSize(pcln.cutab)));
    moduledata.AddUint(ctxt.Arch, uint64(ldr.SymSize(pcln.cutab))); 
    // The filetab slice
    moduledata.AddAddr(ctxt.Arch, pcln.filetab);
    moduledata.AddUint(ctxt.Arch, uint64(ldr.SymSize(pcln.filetab)));
    moduledata.AddUint(ctxt.Arch, uint64(ldr.SymSize(pcln.filetab))); 
    // The pctab slice
    moduledata.AddAddr(ctxt.Arch, pcln.pctab);
    moduledata.AddUint(ctxt.Arch, uint64(ldr.SymSize(pcln.pctab)));
    moduledata.AddUint(ctxt.Arch, uint64(ldr.SymSize(pcln.pctab))); 
    // The pclntab slice
    moduledata.AddAddr(ctxt.Arch, pcln.pclntab);
    moduledata.AddUint(ctxt.Arch, uint64(ldr.SymSize(pcln.pclntab)));
    moduledata.AddUint(ctxt.Arch, uint64(ldr.SymSize(pcln.pclntab))); 
    // The ftab slice
    moduledata.AddAddr(ctxt.Arch, pcln.pclntab);
    moduledata.AddUint(ctxt.Arch, uint64(pcln.nfunc + 1));
    moduledata.AddUint(ctxt.Arch, uint64(pcln.nfunc + 1)); 
    // findfunctab
    moduledata.AddAddr(ctxt.Arch, pcln.findfunctab); 
    // minpc, maxpc
    moduledata.AddAddr(ctxt.Arch, pcln.firstFunc);
    moduledata.AddAddrPlus(ctxt.Arch, pcln.lastFunc, ldr.SymSize(pcln.lastFunc)); 
    // pointers to specific parts of the module
    moduledata.AddAddr(ctxt.Arch, ldr.Lookup("runtime.text", 0));
    moduledata.AddAddr(ctxt.Arch, ldr.Lookup("runtime.etext", 0));
    moduledata.AddAddr(ctxt.Arch, ldr.Lookup("runtime.noptrdata", 0));
    moduledata.AddAddr(ctxt.Arch, ldr.Lookup("runtime.enoptrdata", 0));
    moduledata.AddAddr(ctxt.Arch, ldr.Lookup("runtime.data", 0));
    moduledata.AddAddr(ctxt.Arch, ldr.Lookup("runtime.edata", 0));
    moduledata.AddAddr(ctxt.Arch, ldr.Lookup("runtime.bss", 0));
    moduledata.AddAddr(ctxt.Arch, ldr.Lookup("runtime.ebss", 0));
    moduledata.AddAddr(ctxt.Arch, ldr.Lookup("runtime.noptrbss", 0));
    moduledata.AddAddr(ctxt.Arch, ldr.Lookup("runtime.enoptrbss", 0));
    moduledata.AddAddr(ctxt.Arch, ldr.Lookup("runtime.end", 0));
    moduledata.AddAddr(ctxt.Arch, ldr.Lookup("runtime.gcdata", 0));
    moduledata.AddAddr(ctxt.Arch, ldr.Lookup("runtime.gcbss", 0));
    moduledata.AddAddr(ctxt.Arch, ldr.Lookup("runtime.types", 0));
    moduledata.AddAddr(ctxt.Arch, ldr.Lookup("runtime.etypes", 0));

    if (ctxt.IsAIX() && ctxt.IsExternal()) { 
        // Add R_XCOFFREF relocation to prevent ld's garbage collection of
        // runtime.rodata, runtime.erodata and runtime.epclntab.
        Action<@string> addRef = name => {
            var (r, _) = moduledata.AddRel(objabi.R_XCOFFREF);
            r.SetSym(ldr.Lookup(name, 0));
            r.SetSiz(uint8(ctxt.Arch.PtrSize));
        };
        addRef("runtime.rodata");
        addRef("runtime.erodata");
        addRef("runtime.epclntab");

    }
    moduledata.AddAddr(ctxt.Arch, textsectionmapSym);
    moduledata.AddUint(ctxt.Arch, uint64(nsections));
    moduledata.AddUint(ctxt.Arch, uint64(nsections)); 

    // The typelinks slice
    var typelinkSym = ldr.Lookup("runtime.typelink", 0);
    var ntypelinks = uint64(ldr.SymSize(typelinkSym)) / 4;
    moduledata.AddAddr(ctxt.Arch, typelinkSym);
    moduledata.AddUint(ctxt.Arch, ntypelinks);
    moduledata.AddUint(ctxt.Arch, ntypelinks); 
    // The itablinks slice
    var itablinkSym = ldr.Lookup("runtime.itablink", 0);
    var nitablinks = uint64(ldr.SymSize(itablinkSym)) / uint64(ctxt.Arch.PtrSize);
    moduledata.AddAddr(ctxt.Arch, itablinkSym);
    moduledata.AddUint(ctxt.Arch, nitablinks);
    moduledata.AddUint(ctxt.Arch, nitablinks); 
    // The ptab slice
    {
        var ptab = ldr.Lookup("go.plugin.tabs", 0);

        if (ptab != 0 && ldr.AttrReachable(ptab)) {
            ldr.SetAttrLocal(ptab, true);
            if (ldr.SymType(ptab) != sym.SRODATA) {
                panic(fmt.Sprintf("go.plugin.tabs is %v, not SRODATA", ldr.SymType(ptab)));
            }
            var nentries = uint64(len(ldr.Data(ptab)) / 8); // sizeof(nameOff) + sizeof(typeOff)
            moduledata.AddAddr(ctxt.Arch, ptab);
            moduledata.AddUint(ctxt.Arch, nentries);
            moduledata.AddUint(ctxt.Arch, nentries);

        }
        else
 {
            moduledata.AddUint(ctxt.Arch, 0);
            moduledata.AddUint(ctxt.Arch, 0);
            moduledata.AddUint(ctxt.Arch, 0);
        }
    }

    if (ctxt.BuildMode == BuildModePlugin) {
        addgostring(ctxt, ldr, moduledata, "go.link.thispluginpath", objabi.PathToPrefix(flagPluginPath.val));

        var pkghashes = ldr.CreateSymForUpdate("go.link.pkghashes", 0);
        pkghashes.SetLocal(true);
        pkghashes.SetType(sym.SRODATA);

        {
            var i__prev1 = i;
            var l__prev1 = l;

            foreach (var (__i, __l) in ctxt.Library) {
                i = __i;
                l = __l; 
                // pkghashes[i].name
                addgostring(ctxt, ldr, pkghashes, fmt.Sprintf("go.link.pkgname.%d", i), l.Pkg); 
                // pkghashes[i].linktimehash
                addgostring(ctxt, ldr, pkghashes, fmt.Sprintf("go.link.pkglinkhash.%d", i), string(l.Fingerprint[..])); 
                // pkghashes[i].runtimehash
                var hash = ldr.Lookup("go.link.pkghash." + l.Pkg, 0);
                pkghashes.AddAddr(ctxt.Arch, hash);

            }
    else

            i = i__prev1;
            l = l__prev1;
        }

        moduledata.AddAddr(ctxt.Arch, pkghashes.Sym());
        moduledata.AddUint(ctxt.Arch, uint64(len(ctxt.Library)));
        moduledata.AddUint(ctxt.Arch, uint64(len(ctxt.Library)));

    } {
        moduledata.AddUint(ctxt.Arch, 0); // pluginpath
        moduledata.AddUint(ctxt.Arch, 0);
        moduledata.AddUint(ctxt.Arch, 0); // pkghashes slice
        moduledata.AddUint(ctxt.Arch, 0);
        moduledata.AddUint(ctxt.Arch, 0);

    }
    if (len(ctxt.Shlibs) > 0) {
        var thismodulename = filepath.Base(flagOutfile.val);

        if (ctxt.BuildMode == BuildModeExe || ctxt.BuildMode == BuildModePIE) 
            // When linking an executable, outfile is just "a.out". Make
            // it something slightly more comprehensible.
            thismodulename = "the executable";
                addgostring(ctxt, ldr, moduledata, "go.link.thismodulename", thismodulename);

        var modulehashes = ldr.CreateSymForUpdate("go.link.abihashes", 0);
        modulehashes.SetLocal(true);
        modulehashes.SetType(sym.SRODATA);

        {
            var i__prev1 = i;

            foreach (var (__i, __shlib) in ctxt.Shlibs) {
                i = __i;
                shlib = __shlib; 
                // modulehashes[i].modulename
                var modulename = filepath.Base(shlib.Path);
                addgostring(ctxt, ldr, modulehashes, fmt.Sprintf("go.link.libname.%d", i), modulename); 

                // modulehashes[i].linktimehash
                addgostring(ctxt, ldr, modulehashes, fmt.Sprintf("go.link.linkhash.%d", i), string(shlib.Hash)); 

                // modulehashes[i].runtimehash
                var abihash = ldr.LookupOrCreateSym("go.link.abihash." + modulename, 0);
                ldr.SetAttrReachable(abihash, true);
                modulehashes.AddAddr(ctxt.Arch, abihash);

            }
    else


            i = i__prev1;
        }

        moduledata.AddAddr(ctxt.Arch, modulehashes.Sym());
        moduledata.AddUint(ctxt.Arch, uint64(len(ctxt.Shlibs)));
        moduledata.AddUint(ctxt.Arch, uint64(len(ctxt.Shlibs)));

    } {
        moduledata.AddUint(ctxt.Arch, 0); // modulename
        moduledata.AddUint(ctxt.Arch, 0);
        moduledata.AddUint(ctxt.Arch, 0); // moduleshashes slice
        moduledata.AddUint(ctxt.Arch, 0);
        moduledata.AddUint(ctxt.Arch, 0);

    }
    var hasmain = ctxt.BuildMode == BuildModeExe || ctxt.BuildMode == BuildModePIE;
    if (hasmain) {
        moduledata.AddUint8(1);
    }
    else
 {
        moduledata.AddUint8(0);
    }
    var moduledatatype = ldr.Lookup("type.runtime.moduledata", 0);
    moduledata.SetSize(decodetypeSize(ctxt.Arch, ldr.Data(moduledatatype)));
    moduledata.Grow(moduledata.Size());

    var lastmoduledatap = ldr.CreateSymForUpdate("runtime.lastmoduledatap", 0);
    if (lastmoduledatap.Type() != sym.SDYNIMPORT) {
        lastmoduledatap.SetType(sym.SNOPTRDATA);
        lastmoduledatap.SetSize(0); // overwrite existing value
        lastmoduledatap.SetData(null);
        lastmoduledatap.AddAddr(ctxt.Arch, moduledata.Sym());

    }
    return symGroupType;

});

// CarrierSymByType tracks carrier symbols and their sizes.
public static var CarrierSymByType = default;

private static void setCarrierSym(sym.SymKind typ, loader.Sym s) => func((_, panic, _) => {
    if (CarrierSymByType[typ].Sym != 0) {
        panic(fmt.Sprintf("carrier symbol for type %v already set", typ));
    }
    CarrierSymByType[typ].Sym = s;

});

private static void setCarrierSize(sym.SymKind typ, long sz) => func((_, panic, _) => {
    if (CarrierSymByType[typ].Size != 0) {
        panic(fmt.Sprintf("carrier symbol size for type %v already set", typ));
    }
    CarrierSymByType[typ].Size = sz;

});

private static bool isStaticTmp(@string name) {
    return strings.Contains(name, "." + obj.StaticNamePref);
}

// Mangle function name with ABI information.
private static @string mangleABIName(ptr<Link> _addr_ctxt, ptr<loader.Loader> _addr_ldr, loader.Sym x, @string name) {
    ref Link ctxt = ref _addr_ctxt.val;
    ref loader.Loader ldr = ref _addr_ldr.val;
 
    // For functions with ABI wrappers, we have to make sure that we
    // don't wind up with two symbol table entries with the same
    // name (since this will generated an error from the external
    // linker). If we have wrappers, keep the ABIInternal name
    // unmangled since we want cross-load-module calls to target
    // ABIInternal, and rename other symbols.
    //
    // TODO: avoid the ldr.Lookup calls below by instead using an aux
    // sym or marker relocation to associate the wrapper with the
    // wrapped function.
    if (!buildcfg.Experiment.RegabiWrappers) {
        return name;
    }
    if (!ldr.IsExternal(x) && ldr.SymType(x) == sym.STEXT && ldr.SymVersion(x) != sym.SymVerABIInternal) {
        {
            var s2 = ldr.Lookup(name, sym.SymVerABIInternal);

            if (s2 != 0 && ldr.SymType(s2) == sym.STEXT) {
                name = fmt.Sprintf("%s.abi%d", name, ldr.SymVersion(x));
            }

        }

    }
    if (ctxt.IsShared()) {
        if (ldr.SymType(x) == sym.STEXT && ldr.SymVersion(x) == sym.SymVerABIInternal && !ldr.AttrCgoExport(x) && !strings.HasPrefix(name, "type.")) {
            name = fmt.Sprintf("%s.abiinternal", name);
        }
    }
    return name;

}

} // end ld_package
