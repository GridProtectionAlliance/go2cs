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

// package ld -- go2cs converted at 2020 October 08 04:39:45 UTC
// import "cmd/link/internal/ld" ==> using ld = go.cmd.link.@internal.ld_package
// Original source: C:\Go\src\cmd\link\internal\ld\symtab.go
using objabi = go.cmd.@internal.objabi_package;
using sys = go.cmd.@internal.sys_package;
using loader = go.cmd.link.@internal.loader_package;
using sym = go.cmd.link.@internal.sym_package;
using fmt = go.fmt_package;
using filepath = go.path.filepath_package;
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
        // Symbol table.
        private static long putelfstr(@string s)
        {
            if (len(Elfstrdat) == 0L && s != "")
            { 
                // first entry must be empty string
                putelfstr("");

            }
            var off = len(Elfstrdat);
            Elfstrdat = append(Elfstrdat, s);
            Elfstrdat = append(Elfstrdat, 0L);
            return off;

        }

        private static void putelfsyment(ptr<OutBuf> _addr_@out, long off, long addr, long size, long info, long shndx, long other)
        {
            ref OutBuf @out = ref _addr_@out.val;

            if (elf64)
            {
                @out.Write32(uint32(off));
                @out.Write8(uint8(info));
                @out.Write8(uint8(other));
                @out.Write16(uint16(shndx));
                @out.Write64(uint64(addr));
                @out.Write64(uint64(size));
                Symsize += ELF64SYMSIZE;
            }
            else
            {
                @out.Write32(uint32(off));
                @out.Write32(uint32(addr));
                @out.Write32(uint32(size));
                @out.Write8(uint8(info));
                @out.Write8(uint8(other));
                @out.Write16(uint16(shndx));
                Symsize += ELF32SYMSIZE;
            }

        }

        private static void putelfsym(ptr<Link> _addr_ctxt, ptr<sym.Symbol> _addr_x, @string s, SymbolType t, long addr)
        {
            ref Link ctxt = ref _addr_ctxt.val;
            ref sym.Symbol x = ref _addr_x.val;

            long typ = default;


            if (t == TextSym) 
                typ = STT_FUNC;
            else if (t == DataSym || t == BSSSym) 
                typ = STT_OBJECT;
            else if (t == UndefinedSym) 
                // ElfType is only set for symbols read from Go shared libraries, but
                // for other symbols it is left as STT_NOTYPE which is fine.
                typ = int(x.ElfType());
            else if (t == TLSSym) 
                typ = STT_TLS;
            else 
                return ;
                        var size = x.Size;
            if (t == UndefinedSym)
            {
                size = 0L;
            }

            var xo = x;
            while (xo.Outer != null)
            {
                xo = xo.Outer;
            }


            long elfshnum = default;
            if (xo.Type == sym.SDYNIMPORT || xo.Type == sym.SHOSTOBJ || xo.Type == sym.SUNDEFEXT)
            {
                elfshnum = SHN_UNDEF;
            }
            else
            {
                if (xo.Sect == null)
                {
                    Errorf(x, "missing section in putelfsym");
                    return ;
                }

                if (xo.Sect.Elfsect == null)
                {
                    Errorf(x, "missing ELF section in putelfsym");
                    return ;
                }

                elfshnum = xo.Sect.Elfsect._<ptr<ElfShdr>>().shnum;

            } 

            // One pass for each binding: STB_LOCAL, STB_GLOBAL,
            // maybe one day STB_WEAK.
            var bind = STB_GLOBAL;

            if (x.IsFileLocal() || x.Attr.VisibilityHidden() || x.Attr.Local())
            {
                bind = STB_LOCAL;
            } 

            // In external linking mode, we have to invoke gcc with -rdynamic
            // to get the exported symbols put into the dynamic symbol table.
            // To avoid filling the dynamic table with lots of unnecessary symbols,
            // mark all Go symbols local (not global) in the final executable.
            // But when we're dynamically linking, we need all those global symbols.
            if (!ctxt.DynlinkingGo() && ctxt.LinkMode == LinkExternal && !x.Attr.CgoExportStatic() && elfshnum != SHN_UNDEF)
            {
                bind = STB_LOCAL;
            }

            if (ctxt.LinkMode == LinkExternal && elfshnum != SHN_UNDEF)
            {
                addr -= int64(xo.Sect.Vaddr);
            }

            var other = STV_DEFAULT;
            if (x.Attr.VisibilityHidden())
            { 
                // TODO(mwhudson): We only set AttrVisibilityHidden in ldelf, i.e. when
                // internally linking. But STV_HIDDEN visibility only matters in object
                // files and shared libraries, and as we are a long way from implementing
                // internal linking for shared libraries and only create object files when
                // externally linking, I don't think this makes a lot of sense.
                other = STV_HIDDEN;

            }

            if (ctxt.Arch.Family == sys.PPC64 && typ == STT_FUNC && x.Attr.Shared() && x.Name != "runtime.duffzero" && x.Name != "runtime.duffcopy")
            { 
                // On ppc64 the top three bits of the st_other field indicate how
                // many instructions separate the global and local entry points. In
                // our case it is two instructions, indicated by the value 3.
                // The conditions here match those in preprocess in
                // cmd/internal/obj/ppc64/obj9.go, which is where the
                // instructions are inserted.
                other |= 3L << (int)(5L);

            }

            if (s == x.Name)
            { 
                // We should use Extname for ELF symbol table.
                // TODO: maybe genasmsym should have done this. That function is too
                // overloaded and I would rather not change it for now.
                s = x.Extname();

            } 

            // When dynamically linking, we create Symbols by reading the names from
            // the symbol tables of the shared libraries and so the names need to
            // match exactly. Tools like DTrace will have to wait for now.
            if (!ctxt.DynlinkingGo())
            { 
                // Rewrite · to . for ASCII-only tools like DTrace (sigh)
                s = strings.Replace(s, "·", ".", -1L);

            }

            if (ctxt.DynlinkingGo() && bind == STB_GLOBAL && ctxt.elfbind == STB_LOCAL && x.Type == sym.STEXT)
            { 
                // When dynamically linking, we want references to functions defined
                // in this module to always be to the function object, not to the
                // PLT. We force this by writing an additional local symbol for every
                // global function symbol and making all relocations against the
                // global symbol refer to this local symbol instead (see
                // (*sym.Symbol).ElfsymForReloc). This is approximately equivalent to the
                // ELF linker -Bsymbolic-functions option, but that is buggy on
                // several platforms.
                putelfsyment(_addr_ctxt.Out, putelfstr("local." + s), addr, size, STB_LOCAL << (int)(4L) | typ & 0xfUL, elfshnum, other);
                ctxt.loader.SetSymLocalElfSym(loader.Sym(x.SymIdx), int32(ctxt.numelfsym));
                ctxt.numelfsym++;
                return ;

            }
            else if (bind != ctxt.elfbind)
            {
                return ;
            }

            putelfsyment(_addr_ctxt.Out, putelfstr(s), addr, size, bind << (int)(4L) | typ & 0xfUL, elfshnum, other);
            ctxt.loader.SetSymElfSym(loader.Sym(x.SymIdx), int32(ctxt.numelfsym));
            ctxt.numelfsym++;

        }

        private static void putelfsectionsym(ptr<Link> _addr_ctxt, ptr<OutBuf> _addr_@out, ptr<sym.Symbol> _addr_s, long shndx)
        {
            ref Link ctxt = ref _addr_ctxt.val;
            ref OutBuf @out = ref _addr_@out.val;
            ref sym.Symbol s = ref _addr_s.val;

            putelfsyment(_addr_out, 0L, 0L, 0L, STB_LOCAL << (int)(4L) | STT_SECTION, shndx, 0L);
            ctxt.loader.SetSymElfSym(loader.Sym(s.SymIdx), int32(ctxt.numelfsym));
            ctxt.numelfsym++;
        }

        public static void Asmelfsym(ptr<Link> _addr_ctxt)
        {
            ref Link ctxt = ref _addr_ctxt.val;
 
            // the first symbol entry is reserved
            putelfsyment(_addr_ctxt.Out, 0L, 0L, 0L, STB_LOCAL << (int)(4L) | STT_NOTYPE, 0L, 0L);

            dwarfaddelfsectionsyms(ctxt); 

            // Some linkers will add a FILE sym if one is not present.
            // Avoid having the working directory inserted into the symbol table.
            // It is added with a name to avoid problems with external linking
            // encountered on some versions of Solaris. See issue #14957.
            putelfsyment(_addr_ctxt.Out, putelfstr("go.go"), 0L, 0L, STB_LOCAL << (int)(4L) | STT_FILE, SHN_ABS, 0L);
            ctxt.numelfsym++;

            ctxt.elfbind = STB_LOCAL;
            genasmsym(ctxt, putelfsym);

            ctxt.elfbind = STB_GLOBAL;
            elfglobalsymndx = ctxt.numelfsym;
            genasmsym(ctxt, putelfsym);

        }

        private static void putplan9sym(ptr<Link> _addr_ctxt, ptr<sym.Symbol> _addr_x, @string s, SymbolType typ, long addr)
        {
            ref Link ctxt = ref _addr_ctxt.val;
            ref sym.Symbol x = ref _addr_x.val;

            var t = int(typ);

            if (typ == TextSym || typ == DataSym || typ == BSSSym)
            {
                if (x.IsFileLocal())
                {
                    t += 'a' - 'A';
                }

                fallthrough = true;

            }
            if (fallthrough || typ == AutoSym || typ == ParamSym || typ == FrameSym)
            {
                long l = 4L;
                if (ctxt.HeadType == objabi.Hplan9 && ctxt.Arch.Family == sys.AMD64 && !Flag8)
                {
                    ctxt.Out.Write32b(uint32(addr >> (int)(32L)));
                    l = 8L;
                }

                ctxt.Out.Write32b(uint32(addr));
                ctxt.Out.Write8(uint8(t + 0x80UL));                /* 0x80 is variable length */

                ctxt.Out.WriteString(s);
                ctxt.Out.Write8(0L);

                Symsize += int32(l) + 1L + int32(len(s)) + 1L;
                goto __switch_break0;
            }
            // default: 
                return ;

            __switch_break0:;

        }

        public static void Asmplan9sym(ptr<Link> _addr_ctxt)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            genasmsym(ctxt, putplan9sym);
        }

        private partial struct byPkg // : slice<ptr<sym.Library>>
        {
        }

        private static long Len(this byPkg libs)
        {
            return len(libs);
        }

        private static bool Less(this byPkg libs, long a, long b)
        {
            return libs[a].Pkg < libs[b].Pkg;
        }

        private static void Swap(this byPkg libs, long a, long b)
        {
            libs[a] = libs[b];
            libs[b] = libs[a];

        }

        // Create a table with information on the text sections.
        // Return the symbol of the table, and number of sections.
        private static (loader.Sym, uint) textsectionmap(ptr<Link> _addr_ctxt)
        {
            loader.Sym _p0 = default;
            uint _p0 = default;
            ref Link ctxt = ref _addr_ctxt.val;

            var ldr = ctxt.loader;
            var t = ldr.CreateSymForUpdate("runtime.textsectionmap", 0L);
            t.SetType(sym.SRODATA);
            t.SetReachable(true);
            var nsections = int64(0L);

            {
                var sect__prev1 = sect;

                foreach (var (_, __sect) in Segtext.Sections)
                {
                    sect = __sect;
                    if (sect.Name == ".text")
                    {
                        nsections++;
                    }
                    else
                    {
                        break;
                    }

                }

                sect = sect__prev1;
            }

            t.Grow(3L * nsections * int64(ctxt.Arch.PtrSize));

            var off = int64(0L);
            long n = 0L; 

            // The vaddr for each text section is the difference between the section's
            // Vaddr and the Vaddr for the first text section as determined at compile
            // time.

            // The symbol for the first text section is named runtime.text as before.
            // Additional text sections are named runtime.text.n where n is the
            // order of creation starting with 1. These symbols provide the section's
            // address after relocation by the linker.

            var textbase = Segtext.Sections[0L].Vaddr;
            {
                var sect__prev1 = sect;

                foreach (var (_, __sect) in Segtext.Sections)
                {
                    sect = __sect;
                    if (sect.Name != ".text")
                    {
                        break;
                    }

                    off = t.SetUint(ctxt.Arch, off, sect.Vaddr - textbase);
                    off = t.SetUint(ctxt.Arch, off, sect.Length);
                    if (n == 0L)
                    {
                        var s = ldr.Lookup("runtime.text", 0L);
                        if (s == 0L)
                        {
                            ctxt.Errorf(s, "Unable to find symbol runtime.text\n");
                        }

                        off = t.SetAddr(ctxt.Arch, off, s);


                    }
                    else
                    {
                        s = ldr.Lookup(fmt.Sprintf("runtime.text.%d", n), 0L);
                        if (s == 0L)
                        {
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

        private static slice<sym.SymKind> symtab(this ptr<Link> _addr_ctxt) => func((_, panic, __) =>
        {
            ref Link ctxt = ref _addr_ctxt.val;

            var ldr = ctxt.loader;

            if (!ctxt.IsAIX())
            {

                if (ctxt.BuildMode == BuildModeCArchive || ctxt.BuildMode == BuildModeCShared) 
                    var s = ldr.Lookup(flagEntrySymbol.val, sym.SymVerABI0);
                    if (s != 0L)
                    {
                        addinitarrdata(ctxt, ldr, s);
                    }

                            } 

            // Define these so that they'll get put into the symbol table.
            // data.c:/^address will provide the actual values.
            ctxt.xdefine2("runtime.text", sym.STEXT, 0L);
            ctxt.xdefine2("runtime.etext", sym.STEXT, 0L);
            ctxt.xdefine2("runtime.itablink", sym.SRODATA, 0L);
            ctxt.xdefine2("runtime.eitablink", sym.SRODATA, 0L);
            ctxt.xdefine2("runtime.rodata", sym.SRODATA, 0L);
            ctxt.xdefine2("runtime.erodata", sym.SRODATA, 0L);
            ctxt.xdefine2("runtime.types", sym.SRODATA, 0L);
            ctxt.xdefine2("runtime.etypes", sym.SRODATA, 0L);
            ctxt.xdefine2("runtime.noptrdata", sym.SNOPTRDATA, 0L);
            ctxt.xdefine2("runtime.enoptrdata", sym.SNOPTRDATA, 0L);
            ctxt.xdefine2("runtime.data", sym.SDATA, 0L);
            ctxt.xdefine2("runtime.edata", sym.SDATA, 0L);
            ctxt.xdefine2("runtime.bss", sym.SBSS, 0L);
            ctxt.xdefine2("runtime.ebss", sym.SBSS, 0L);
            ctxt.xdefine2("runtime.noptrbss", sym.SNOPTRBSS, 0L);
            ctxt.xdefine2("runtime.enoptrbss", sym.SNOPTRBSS, 0L);
            ctxt.xdefine2("runtime.end", sym.SBSS, 0L);
            ctxt.xdefine2("runtime.epclntab", sym.SRODATA, 0L);
            ctxt.xdefine2("runtime.esymtab", sym.SRODATA, 0L); 

            // garbage collection symbols
            s = ldr.CreateSymForUpdate("runtime.gcdata", 0L);
            s.SetType(sym.SRODATA);
            s.SetSize(0L);
            s.SetReachable(true);
            ctxt.xdefine2("runtime.egcdata", sym.SRODATA, 0L);

            s = ldr.CreateSymForUpdate("runtime.gcbss", 0L);
            s.SetType(sym.SRODATA);
            s.SetSize(0L);
            s.SetReachable(true);
            ctxt.xdefine2("runtime.egcbss", sym.SRODATA, 0L); 

            // pseudo-symbols to mark locations of type, string, and go string data.
            loader.Sym symtype = default;            loader.Sym symtyperel = default;

            if (!ctxt.DynlinkingGo())
            {
                if (ctxt.UseRelro() && (ctxt.BuildMode == BuildModeCArchive || ctxt.BuildMode == BuildModeCShared || ctxt.BuildMode == BuildModePIE))
                {
                    s = ldr.CreateSymForUpdate("type.*", 0L);
                    s.SetType(sym.STYPE);
                    s.SetSize(0L);
                    s.SetReachable(true);
                    symtype = s.Sym();

                    s = ldr.CreateSymForUpdate("typerel.*", 0L);
                    s.SetType(sym.STYPERELRO);
                    s.SetSize(0L);
                    s.SetReachable(true);
                    symtyperel = s.Sym();
                }
                else
                {
                    s = ldr.CreateSymForUpdate("type.*", 0L);
                    s.SetType(sym.STYPE);
                    s.SetSize(0L);
                    s.SetReachable(true);
                    symtype = s.Sym();
                    symtyperel = s.Sym();
                }

            }

            Func<@string, sym.SymKind, loader.Sym> groupSym = (name, t) =>
            {
                s = ldr.CreateSymForUpdate(name, 0L);
                s.SetType(t);
                s.SetSize(0L);
                s.SetLocal(true);
                s.SetReachable(true);
                return s.Sym();
            }
;
            var symgostring = groupSym("go.string.*", sym.SGOSTRING);            var symgofunc = groupSym("go.func.*", sym.SGOFUNC);            var symgcbits = groupSym("runtime.gcbits.*", sym.SGCBITS);

            loader.Sym symgofuncrel = default;
            if (!ctxt.DynlinkingGo())
            {
                if (ctxt.UseRelro())
                {
                    symgofuncrel = groupSym("go.funcrel.*", sym.SGOFUNCRELRO);
                }
                else
                {
                    symgofuncrel = symgofunc;
                }

            }

            var symitablink = ldr.CreateSymForUpdate("runtime.itablink", 0L);
            symitablink.SetType(sym.SITABLINK);

            var symt = ldr.CreateSymForUpdate("runtime.symtab", 0L);
            symt.SetType(sym.SSYMTAB);
            symt.SetSize(0L);
            symt.SetReachable(true);
            symt.SetLocal(true);

            long nitablinks = 0L; 

            // assign specific types so that they sort together.
            // within a type they sort by size, so the .* symbols
            // just defined above will be first.
            // hide the specific symbols.
            var nsym = loader.Sym(ldr.NSym());
            var symGroupType = make_slice<sym.SymKind>(nsym);
            {
                var s__prev1 = s;

                for (s = loader.Sym(1L); s < nsym; s++)
                {
                    var name = ldr.SymName(s);
                    if (!ctxt.IsExternal() && isStaticTemp(name))
                    {
                        ldr.SetAttrNotInSymbolTable(s, true);
                    }

                    if (!ldr.AttrReachable(s) || ldr.AttrSpecial(s) || (ldr.SymType(s) != sym.SRODATA && ldr.SymType(s) != sym.SGOFUNC))
                    {
                        continue;
                    }


                    if (strings.HasPrefix(name, "type.")) 
                        if (!ctxt.DynlinkingGo())
                        {
                            ldr.SetAttrNotInSymbolTable(s, true);
                        }

                        if (ctxt.UseRelro())
                        {
                            symGroupType[s] = sym.STYPERELRO;
                            ldr.SetOuterSym(s, symtyperel);
                        }
                        else
                        {
                            symGroupType[s] = sym.STYPE;
                            ldr.SetOuterSym(s, symtype);
                        }

                    else if (strings.HasPrefix(name, "go.importpath.") && ctxt.UseRelro()) 
                        // Keep go.importpath symbols in the same section as types and
                        // names, as they can be referred to by a section offset.
                        symGroupType[s] = sym.STYPERELRO;
                    else if (strings.HasPrefix(name, "go.itablink.")) 
                        nitablinks++;
                        symGroupType[s] = sym.SITABLINK;
                        ldr.SetAttrNotInSymbolTable(s, true);
                        ldr.SetOuterSym(s, symitablink.Sym());
                    else if (strings.HasPrefix(name, "go.string.")) 
                        symGroupType[s] = sym.SGOSTRING;
                        ldr.SetAttrNotInSymbolTable(s, true);
                        ldr.SetOuterSym(s, symgostring);
                    else if (strings.HasPrefix(name, "runtime.gcbits.")) 
                        symGroupType[s] = sym.SGCBITS;
                        ldr.SetAttrNotInSymbolTable(s, true);
                        ldr.SetOuterSym(s, symgcbits);
                    else if (strings.HasSuffix(name, "·f")) 
                        if (!ctxt.DynlinkingGo())
                        {
                            ldr.SetAttrNotInSymbolTable(s, true);
                        }

                        if (ctxt.UseRelro())
                        {
                            symGroupType[s] = sym.SGOFUNCRELRO;
                            ldr.SetOuterSym(s, symgofuncrel);
                        }
                        else
                        {
                            symGroupType[s] = sym.SGOFUNC;
                            ldr.SetOuterSym(s, symgofunc);
                        }

                    else if (strings.HasPrefix(name, "gcargs.") || strings.HasPrefix(name, "gclocals.") || strings.HasPrefix(name, "gclocals·") || ldr.SymType(s) == sym.SGOFUNC && s != symgofunc || strings.HasSuffix(name, ".opendefer")) 
                        symGroupType[s] = sym.SGOFUNC;
                        ldr.SetAttrNotInSymbolTable(s, true);
                        ldr.SetOuterSym(s, symgofunc);
                        const long align = (long)4L;

                        ldr.SetSymAlign(s, align);
                        liveness += (ldr.SymSize(s) + int64(align) - 1L) & ~(int64(align) - 1L);
                    
                }


                s = s__prev1;
            }

            if (ctxt.BuildMode == BuildModeShared)
            {
                var abihashgostr = ldr.CreateSymForUpdate("go.link.abihash." + filepath.Base(flagOutfile.val), 0L);
                abihashgostr.SetReachable(true);
                abihashgostr.SetType(sym.SRODATA);
                var hashsym = ldr.LookupOrCreateSym("go.link.abihashbytes", 0L);
                abihashgostr.AddAddr(ctxt.Arch, hashsym);
                abihashgostr.AddUint(ctxt.Arch, uint64(ldr.SymSize(hashsym)));
            }

            if (ctxt.BuildMode == BuildModePlugin || ctxt.CanUsePlugins())
            {
                {
                    var l__prev1 = l;

                    foreach (var (_, __l) in ctxt.Library)
                    {
                        l = __l;
                        s = ldr.CreateSymForUpdate("go.link.pkghashbytes." + l.Pkg, 0L);
                        s.SetReachable(true);
                        s.SetType(sym.SRODATA);
                        s.SetSize(int64(len(l.Hash)));
                        s.SetData((slice<byte>)l.Hash);
                        var str = ldr.CreateSymForUpdate("go.link.pkghash." + l.Pkg, 0L);
                        str.SetReachable(true);
                        str.SetType(sym.SRODATA);
                        str.AddAddr(ctxt.Arch, s.Sym());
                        str.AddUint(ctxt.Arch, uint64(len(l.Hash)));
                    }

                    l = l__prev1;
                }
            }

            var (textsectionmapSym, nsections) = textsectionmap(_addr_ctxt); 

            // Information about the layout of the executable image for the
            // runtime to use. Any changes here must be matched by changes to
            // the definition of moduledata in runtime/symtab.go.
            // This code uses several global variables that are set by pcln.go:pclntab.
            var moduledata = ldr.MakeSymbolUpdater(ctxt.Moduledata2);
            var pclntab = ldr.Lookup("runtime.pclntab", 0L); 
            // The pclntab slice
            moduledata.AddAddr(ctxt.Arch, pclntab);
            moduledata.AddUint(ctxt.Arch, uint64(ldr.SymSize(pclntab)));
            moduledata.AddUint(ctxt.Arch, uint64(ldr.SymSize(pclntab))); 
            // The ftab slice
            moduledata.AddAddrPlus(ctxt.Arch, pclntab, int64(pclntabPclntabOffset));
            moduledata.AddUint(ctxt.Arch, uint64(pclntabNfunc + 1L));
            moduledata.AddUint(ctxt.Arch, uint64(pclntabNfunc + 1L)); 
            // The filetab slice
            moduledata.AddAddrPlus(ctxt.Arch, pclntab, int64(pclntabFiletabOffset));
            moduledata.AddUint(ctxt.Arch, uint64(ctxt.NumFilesyms) + 1L);
            moduledata.AddUint(ctxt.Arch, uint64(ctxt.NumFilesyms) + 1L); 
            // findfunctab
            moduledata.AddAddr(ctxt.Arch, ldr.Lookup("runtime.findfunctab", 0L)); 
            // minpc, maxpc
            moduledata.AddAddr(ctxt.Arch, pclntabFirstFunc);
            moduledata.AddAddrPlus(ctxt.Arch, pclntabLastFunc, ldr.SymSize(pclntabLastFunc)); 
            // pointers to specific parts of the module
            moduledata.AddAddr(ctxt.Arch, ldr.Lookup("runtime.text", 0L));
            moduledata.AddAddr(ctxt.Arch, ldr.Lookup("runtime.etext", 0L));
            moduledata.AddAddr(ctxt.Arch, ldr.Lookup("runtime.noptrdata", 0L));
            moduledata.AddAddr(ctxt.Arch, ldr.Lookup("runtime.enoptrdata", 0L));
            moduledata.AddAddr(ctxt.Arch, ldr.Lookup("runtime.data", 0L));
            moduledata.AddAddr(ctxt.Arch, ldr.Lookup("runtime.edata", 0L));
            moduledata.AddAddr(ctxt.Arch, ldr.Lookup("runtime.bss", 0L));
            moduledata.AddAddr(ctxt.Arch, ldr.Lookup("runtime.ebss", 0L));
            moduledata.AddAddr(ctxt.Arch, ldr.Lookup("runtime.noptrbss", 0L));
            moduledata.AddAddr(ctxt.Arch, ldr.Lookup("runtime.enoptrbss", 0L));
            moduledata.AddAddr(ctxt.Arch, ldr.Lookup("runtime.end", 0L));
            moduledata.AddAddr(ctxt.Arch, ldr.Lookup("runtime.gcdata", 0L));
            moduledata.AddAddr(ctxt.Arch, ldr.Lookup("runtime.gcbss", 0L));
            moduledata.AddAddr(ctxt.Arch, ldr.Lookup("runtime.types", 0L));
            moduledata.AddAddr(ctxt.Arch, ldr.Lookup("runtime.etypes", 0L));

            if (ctxt.IsAIX() && ctxt.IsExternal())
            { 
                // Add R_XCOFFREF relocation to prevent ld's garbage collection of
                // runtime.rodata, runtime.erodata and runtime.epclntab.
                Action<@string> addRef = name =>
                {
                    var (r, _) = moduledata.AddRel(objabi.R_XCOFFREF);
                    r.SetSym(ldr.Lookup(name, 0L));
                    r.SetSiz(uint8(ctxt.Arch.PtrSize));
                }
;
                addRef("runtime.rodata");
                addRef("runtime.erodata");
                addRef("runtime.epclntab");

            } 

            // text section information
            moduledata.AddAddr(ctxt.Arch, textsectionmapSym);
            moduledata.AddUint(ctxt.Arch, uint64(nsections));
            moduledata.AddUint(ctxt.Arch, uint64(nsections)); 

            // The typelinks slice
            var typelinkSym = ldr.Lookup("runtime.typelink", 0L);
            var ntypelinks = uint64(ldr.SymSize(typelinkSym)) / 4L;
            moduledata.AddAddr(ctxt.Arch, typelinkSym);
            moduledata.AddUint(ctxt.Arch, ntypelinks);
            moduledata.AddUint(ctxt.Arch, ntypelinks); 
            // The itablinks slice
            moduledata.AddAddr(ctxt.Arch, symitablink.Sym());
            moduledata.AddUint(ctxt.Arch, uint64(nitablinks));
            moduledata.AddUint(ctxt.Arch, uint64(nitablinks)); 
            // The ptab slice
            {
                var ptab = ldr.Lookup("go.plugin.tabs", 0L);

                if (ptab != 0L && ldr.AttrReachable(ptab))
                {
                    ldr.SetAttrLocal(ptab, true);
                    if (ldr.SymType(ptab) != sym.SRODATA)
                    {
                        panic(fmt.Sprintf("go.plugin.tabs is %v, not SRODATA", ldr.SymType(ptab)));
                    }

                    var nentries = uint64(len(ldr.Data(ptab)) / 8L); // sizeof(nameOff) + sizeof(typeOff)
                    moduledata.AddAddr(ctxt.Arch, ptab);
                    moduledata.AddUint(ctxt.Arch, nentries);
                    moduledata.AddUint(ctxt.Arch, nentries);

                }
                else
                {
                    moduledata.AddUint(ctxt.Arch, 0L);
                    moduledata.AddUint(ctxt.Arch, 0L);
                    moduledata.AddUint(ctxt.Arch, 0L);
                }

            }

            if (ctxt.BuildMode == BuildModePlugin)
            {
                addgostring(ctxt, ldr, moduledata, "go.link.thispluginpath", objabi.PathToPrefix(flagPluginPath.val));

                var pkghashes = ldr.CreateSymForUpdate("go.link.pkghashes", 0L);
                pkghashes.SetReachable(true);
                pkghashes.SetLocal(true);
                pkghashes.SetType(sym.SRODATA);

                {
                    var i__prev1 = i;
                    var l__prev1 = l;

                    foreach (var (__i, __l) in ctxt.Library)
                    {
                        i = __i;
                        l = __l; 
                        // pkghashes[i].name
                        addgostring(ctxt, ldr, pkghashes, fmt.Sprintf("go.link.pkgname.%d", i), l.Pkg); 
                        // pkghashes[i].linktimehash
                        addgostring(ctxt, ldr, pkghashes, fmt.Sprintf("go.link.pkglinkhash.%d", i), l.Hash); 
                        // pkghashes[i].runtimehash
                        var hash = ldr.Lookup("go.link.pkghash." + l.Pkg, 0L);
                        pkghashes.AddAddr(ctxt.Arch, hash);

                    }
            else

                    i = i__prev1;
                    l = l__prev1;
                }

                moduledata.AddAddr(ctxt.Arch, pkghashes.Sym());
                moduledata.AddUint(ctxt.Arch, uint64(len(ctxt.Library)));
                moduledata.AddUint(ctxt.Arch, uint64(len(ctxt.Library)));

            }            {
                moduledata.AddUint(ctxt.Arch, 0L); // pluginpath
                moduledata.AddUint(ctxt.Arch, 0L);
                moduledata.AddUint(ctxt.Arch, 0L); // pkghashes slice
                moduledata.AddUint(ctxt.Arch, 0L);
                moduledata.AddUint(ctxt.Arch, 0L);

            }

            if (len(ctxt.Shlibs) > 0L)
            {
                var thismodulename = filepath.Base(flagOutfile.val);

                if (ctxt.BuildMode == BuildModeExe || ctxt.BuildMode == BuildModePIE) 
                    // When linking an executable, outfile is just "a.out". Make
                    // it something slightly more comprehensible.
                    thismodulename = "the executable";
                                addgostring(ctxt, ldr, moduledata, "go.link.thismodulename", thismodulename);

                var modulehashes = ldr.CreateSymForUpdate("go.link.abihashes", 0L);
                modulehashes.SetReachable(true);
                modulehashes.SetLocal(true);
                modulehashes.SetType(sym.SRODATA);

                {
                    var i__prev1 = i;

                    foreach (var (__i, __shlib) in ctxt.Shlibs)
                    {
                        i = __i;
                        shlib = __shlib; 
                        // modulehashes[i].modulename
                        var modulename = filepath.Base(shlib.Path);
                        addgostring(ctxt, ldr, modulehashes, fmt.Sprintf("go.link.libname.%d", i), modulename); 

                        // modulehashes[i].linktimehash
                        addgostring(ctxt, ldr, modulehashes, fmt.Sprintf("go.link.linkhash.%d", i), string(shlib.Hash)); 

                        // modulehashes[i].runtimehash
                        var abihash = ldr.LookupOrCreateSym("go.link.abihash." + modulename, 0L);
                        ldr.SetAttrReachable(abihash, true);
                        modulehashes.AddAddr(ctxt.Arch, abihash);

                    }
            else


                    i = i__prev1;
                }

                moduledata.AddAddr(ctxt.Arch, modulehashes.Sym());
                moduledata.AddUint(ctxt.Arch, uint64(len(ctxt.Shlibs)));
                moduledata.AddUint(ctxt.Arch, uint64(len(ctxt.Shlibs)));

            }            {
                moduledata.AddUint(ctxt.Arch, 0L); // modulename
                moduledata.AddUint(ctxt.Arch, 0L);
                moduledata.AddUint(ctxt.Arch, 0L); // moduleshashes slice
                moduledata.AddUint(ctxt.Arch, 0L);
                moduledata.AddUint(ctxt.Arch, 0L);

            }

            var hasmain = ctxt.BuildMode == BuildModeExe || ctxt.BuildMode == BuildModePIE;
            if (hasmain)
            {
                moduledata.AddUint8(1L);
            }
            else
            {
                moduledata.AddUint8(0L);
            } 

            // The rest of moduledata is zero initialized.
            // When linking an object that does not contain the runtime we are
            // creating the moduledata from scratch and it does not have a
            // compiler-provided size, so read it from the type data.
            var moduledatatype = ldr.Lookup("type.runtime.moduledata", 0L);
            moduledata.SetSize(decodetypeSize(ctxt.Arch, ldr.Data(moduledatatype)));
            moduledata.Grow(moduledata.Size());

            var lastmoduledatap = ldr.CreateSymForUpdate("runtime.lastmoduledatap", 0L);
            if (lastmoduledatap.Type() != sym.SDYNIMPORT)
            {
                lastmoduledatap.SetType(sym.SNOPTRDATA);
                lastmoduledatap.SetSize(0L); // overwrite existing value
                lastmoduledatap.SetData(null);
                lastmoduledatap.AddAddr(ctxt.Arch, moduledata.Sym());

            }

            return symGroupType;

        });

        private static bool isStaticTemp(@string name)
        {
            {
                var i = strings.LastIndex(name, "/");

                if (i >= 0L)
                {
                    name = name[i..];
                }

            }

            return strings.Contains(name, "..stmp_");

        }
    }
}}}}
