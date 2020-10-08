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

// package ld -- go2cs converted at 2020 October 08 04:42:00 UTC
// import "cmd/oldlink/internal/ld" ==> using ld = go.cmd.oldlink.@internal.ld_package
// Original source: C:\Go\src\cmd\oldlink\internal\ld\symtab.go
using objabi = go.cmd.@internal.objabi_package;
using sys = go.cmd.@internal.sys_package;
using sym = go.cmd.oldlink.@internal.sym_package;
using fmt = go.fmt_package;
using filepath = go.path.filepath_package;
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

        private static long numelfsym = 1L; // 0 is reserved

        private static long elfbind = default;

        private static void putelfsym(ptr<Link> _addr_ctxt, ptr<sym.Symbol> _addr_x, @string s, SymbolType t, long addr, ptr<sym.Symbol> _addr_go_)
        {
            ref Link ctxt = ref _addr_ctxt.val;
            ref sym.Symbol x = ref _addr_x.val;
            ref sym.Symbol go_ = ref _addr_go_.val;

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

            // When dynamically linking, we create Symbols by reading the names from
            // the symbol tables of the shared libraries and so the names need to
            // match exactly. Tools like DTrace will have to wait for now.
            if (!ctxt.DynlinkingGo())
            { 
                // Rewrite · to . for ASCII-only tools like DTrace (sigh)
                s = strings.Replace(s, "·", ".", -1L);

            }

            if (ctxt.DynlinkingGo() && bind == STB_GLOBAL && elfbind == STB_LOCAL && x.Type == sym.STEXT)
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
                x.LocalElfsym = int32(numelfsym);
                numelfsym++;
                return ;

            }
            else if (bind != elfbind)
            {
                return ;
            }

            putelfsyment(_addr_ctxt.Out, putelfstr(s), addr, size, bind << (int)(4L) | typ & 0xfUL, elfshnum, other);
            x.Elfsym = int32(numelfsym);
            numelfsym++;

        }

        private static void putelfsectionsym(ptr<OutBuf> _addr_@out, ptr<sym.Symbol> _addr_s, long shndx)
        {
            ref OutBuf @out = ref _addr_@out.val;
            ref sym.Symbol s = ref _addr_s.val;

            putelfsyment(_addr_out, 0L, 0L, 0L, STB_LOCAL << (int)(4L) | STT_SECTION, shndx, 0L);
            s.Elfsym = int32(numelfsym);
            numelfsym++;
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
            numelfsym++;

            elfbind = STB_LOCAL;
            genasmsym(ctxt, putelfsym);

            elfbind = STB_GLOBAL;
            elfglobalsymndx = numelfsym;
            genasmsym(ctxt, putelfsym);

        }

        private static void putplan9sym(ptr<Link> _addr_ctxt, ptr<sym.Symbol> _addr_x, @string s, SymbolType typ, long addr, ptr<sym.Symbol> _addr_go_)
        {
            ref Link ctxt = ref _addr_ctxt.val;
            ref sym.Symbol x = ref _addr_x.val;
            ref sym.Symbol go_ = ref _addr_go_.val;

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

        private static ptr<sym.Symbol> symt;

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

        private static uint textsectionmap(ptr<Link> _addr_ctxt)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            var t = ctxt.Syms.Lookup("runtime.textsectionmap", 0L);
            t.Type = sym.SRODATA;
            t.Attr |= sym.AttrReachable;
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
                        var s = ctxt.Syms.ROLookup("runtime.text", 0L);
                        if (s == null)
                        {
                            Errorf(null, "Unable to find symbol runtime.text\n");
                        }

                        off = t.SetAddr(ctxt.Arch, off, s);


                    }
                    else
                    {
                        s = ctxt.Syms.Lookup(fmt.Sprintf("runtime.text.%d", n), 0L);
                        if (s == null)
                        {
                            Errorf(null, "Unable to find symbol runtime.text.%d\n", n);
                        }

                        off = t.SetAddr(ctxt.Arch, off, s);

                    }

                    n++;

                }

                sect = sect__prev1;
            }

            return uint32(n);

        }

        private static void symtab(this ptr<Link> _addr_ctxt)
        {
            ref Link ctxt = ref _addr_ctxt.val;


            if (ctxt.BuildMode == BuildModeCArchive || ctxt.BuildMode == BuildModeCShared) 
                {
                    var s__prev1 = s;

                    foreach (var (_, __s) in ctxt.Syms.Allsym)
                    {
                        s = __s; 
                        // Create a new entry in the .init_array section that points to the
                        // library initializer function.
                        if (s.Name == flagEntrySymbol && ctxt.HeadType != objabi.Haix.val)
                        {
                            addinitarrdata(ctxt, s);
                        }

                    }

                    s = s__prev1;
                }
            // Define these so that they'll get put into the symbol table.
            // data.c:/^address will provide the actual values.
            ctxt.xdefine("runtime.text", sym.STEXT, 0L);

            ctxt.xdefine("runtime.etext", sym.STEXT, 0L);
            ctxt.xdefine("runtime.itablink", sym.SRODATA, 0L);
            ctxt.xdefine("runtime.eitablink", sym.SRODATA, 0L);
            ctxt.xdefine("runtime.rodata", sym.SRODATA, 0L);
            ctxt.xdefine("runtime.erodata", sym.SRODATA, 0L);
            ctxt.xdefine("runtime.types", sym.SRODATA, 0L);
            ctxt.xdefine("runtime.etypes", sym.SRODATA, 0L);
            ctxt.xdefine("runtime.noptrdata", sym.SNOPTRDATA, 0L);
            ctxt.xdefine("runtime.enoptrdata", sym.SNOPTRDATA, 0L);
            ctxt.xdefine("runtime.data", sym.SDATA, 0L);
            ctxt.xdefine("runtime.edata", sym.SDATA, 0L);
            ctxt.xdefine("runtime.bss", sym.SBSS, 0L);
            ctxt.xdefine("runtime.ebss", sym.SBSS, 0L);
            ctxt.xdefine("runtime.noptrbss", sym.SNOPTRBSS, 0L);
            ctxt.xdefine("runtime.enoptrbss", sym.SNOPTRBSS, 0L);
            ctxt.xdefine("runtime.end", sym.SBSS, 0L);
            ctxt.xdefine("runtime.epclntab", sym.SRODATA, 0L);
            ctxt.xdefine("runtime.esymtab", sym.SRODATA, 0L); 

            // garbage collection symbols
            var s = ctxt.Syms.Lookup("runtime.gcdata", 0L);

            s.Type = sym.SRODATA;
            s.Size = 0L;
            s.Attr |= sym.AttrReachable;
            ctxt.xdefine("runtime.egcdata", sym.SRODATA, 0L);

            s = ctxt.Syms.Lookup("runtime.gcbss", 0L);
            s.Type = sym.SRODATA;
            s.Size = 0L;
            s.Attr |= sym.AttrReachable;
            ctxt.xdefine("runtime.egcbss", sym.SRODATA, 0L); 

            // pseudo-symbols to mark locations of type, string, and go string data.
            ptr<sym.Symbol> symtype;
            ptr<sym.Symbol> symtyperel;
            if (!ctxt.DynlinkingGo())
            {
                if (ctxt.UseRelro() && (ctxt.BuildMode == BuildModeCArchive || ctxt.BuildMode == BuildModeCShared || ctxt.BuildMode == BuildModePIE))
                {
                    s = ctxt.Syms.Lookup("type.*", 0L);

                    s.Type = sym.STYPE;
                    s.Size = 0L;
                    s.Attr |= sym.AttrReachable;
                    symtype = s;

                    s = ctxt.Syms.Lookup("typerel.*", 0L);

                    s.Type = sym.STYPERELRO;
                    s.Size = 0L;
                    s.Attr |= sym.AttrReachable;
                    symtyperel = s;
                }
                else
                {
                    s = ctxt.Syms.Lookup("type.*", 0L);

                    s.Type = sym.STYPE;
                    s.Size = 0L;
                    s.Attr |= sym.AttrReachable;
                    symtype = s;
                    symtyperel = s;
                }

            }

            Func<@string, sym.SymKind, ptr<sym.Symbol>> groupSym = (name, t) =>
            {
                s = ctxt.Syms.Lookup(name, 0L);
                s.Type = t;
                s.Size = 0L;
                s.Attr |= sym.AttrLocal | sym.AttrReachable;
                return s;
            }
;
            var symgostring = groupSym("go.string.*", sym.SGOSTRING);            var symgofunc = groupSym("go.func.*", sym.SGOFUNC);            var symgcbits = groupSym("runtime.gcbits.*", sym.SGCBITS);

            ptr<sym.Symbol> symgofuncrel;
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

            var symitablink = ctxt.Syms.Lookup("runtime.itablink", 0L);
            symitablink.Type = sym.SITABLINK;

            symt = ctxt.Syms.Lookup("runtime.symtab", 0L);
            symt.Attr |= sym.AttrLocal;
            symt.Type = sym.SSYMTAB;
            symt.Size = 0L;
            symt.Attr |= sym.AttrReachable;

            long nitablinks = 0L; 

            // assign specific types so that they sort together.
            // within a type they sort by size, so the .* symbols
            // just defined above will be first.
            // hide the specific symbols.
            {
                var s__prev1 = s;

                foreach (var (_, __s) in ctxt.Syms.Allsym)
                {
                    s = __s;
                    if (ctxt.LinkMode != LinkExternal && isStaticTemp(s.Name))
                    {
                        s.Attr |= sym.AttrNotInSymbolTable;
                    }

                    if (!s.Attr.Reachable() || s.Attr.Special() || s.Type != sym.SRODATA)
                    {
                        continue;
                    }


                    if (strings.HasPrefix(s.Name, "type.")) 
                        if (!ctxt.DynlinkingGo())
                        {
                            s.Attr |= sym.AttrNotInSymbolTable;
                        }

                        if (ctxt.UseRelro())
                        {
                            s.Type = sym.STYPERELRO;
                            s.Outer = symtyperel;
                        }
                        else
                        {
                            s.Type = sym.STYPE;
                            s.Outer = symtype;
                        }

                    else if (strings.HasPrefix(s.Name, "go.importpath.") && ctxt.UseRelro()) 
                        // Keep go.importpath symbols in the same section as types and
                        // names, as they can be referred to by a section offset.
                        s.Type = sym.STYPERELRO;
                    else if (strings.HasPrefix(s.Name, "go.itablink.")) 
                        nitablinks++;
                        s.Type = sym.SITABLINK;
                        s.Attr |= sym.AttrNotInSymbolTable;
                        s.Outer = symitablink;
                    else if (strings.HasPrefix(s.Name, "go.string.")) 
                        s.Type = sym.SGOSTRING;
                        s.Attr |= sym.AttrNotInSymbolTable;
                        s.Outer = symgostring;
                    else if (strings.HasPrefix(s.Name, "runtime.gcbits.")) 
                        s.Type = sym.SGCBITS;
                        s.Attr |= sym.AttrNotInSymbolTable;
                        s.Outer = symgcbits;
                    else if (strings.HasSuffix(s.Name, "·f")) 
                        if (!ctxt.DynlinkingGo())
                        {
                            s.Attr |= sym.AttrNotInSymbolTable;
                        }

                        if (ctxt.UseRelro())
                        {
                            s.Type = sym.SGOFUNCRELRO;
                            s.Outer = symgofuncrel;
                        }
                        else
                        {
                            s.Type = sym.SGOFUNC;
                            s.Outer = symgofunc;
                        }

                    else if (strings.HasPrefix(s.Name, "gcargs.") || strings.HasPrefix(s.Name, "gclocals.") || strings.HasPrefix(s.Name, "gclocals·") || strings.HasPrefix(s.Name, "inltree.") || strings.HasSuffix(s.Name, ".opendefer")) 
                        s.Type = sym.SGOFUNC;
                        s.Attr |= sym.AttrNotInSymbolTable;
                        s.Outer = symgofunc;
                        s.Align = 4L;
                        liveness += (s.Size + int64(s.Align) - 1L) & ~(int64(s.Align) - 1L);
                    
                }

                s = s__prev1;
            }

            if (ctxt.BuildMode == BuildModeShared)
            {
                var abihashgostr = ctxt.Syms.Lookup("go.link.abihash." + filepath.Base(flagOutfile.val), 0L);
                abihashgostr.Attr |= sym.AttrReachable;
                abihashgostr.Type = sym.SRODATA;
                var hashsym = ctxt.Syms.Lookup("go.link.abihashbytes", 0L);
                abihashgostr.AddAddr(ctxt.Arch, hashsym);
                abihashgostr.AddUint(ctxt.Arch, uint64(hashsym.Size));
            }

            if (ctxt.BuildMode == BuildModePlugin || ctxt.CanUsePlugins())
            {
                {
                    var l__prev1 = l;

                    foreach (var (_, __l) in ctxt.Library)
                    {
                        l = __l;
                        s = ctxt.Syms.Lookup("go.link.pkghashbytes." + l.Pkg, 0L);
                        s.Attr |= sym.AttrReachable;
                        s.Type = sym.SRODATA;
                        s.Size = int64(len(l.Hash));
                        s.P = (slice<byte>)l.Hash;
                        var str = ctxt.Syms.Lookup("go.link.pkghash." + l.Pkg, 0L);
                        str.Attr |= sym.AttrReachable;
                        str.Type = sym.SRODATA;
                        str.AddAddr(ctxt.Arch, s);
                        str.AddUint(ctxt.Arch, uint64(len(l.Hash)));
                    }

                    l = l__prev1;
                }
            }

            var nsections = textsectionmap(_addr_ctxt); 

            // Information about the layout of the executable image for the
            // runtime to use. Any changes here must be matched by changes to
            // the definition of moduledata in runtime/symtab.go.
            // This code uses several global variables that are set by pcln.go:pclntab.
            var moduledata = ctxt.Moduledata; 
            // The pclntab slice
            moduledata.AddAddr(ctxt.Arch, ctxt.Syms.Lookup("runtime.pclntab", 0L));
            moduledata.AddUint(ctxt.Arch, uint64(ctxt.Syms.Lookup("runtime.pclntab", 0L).Size));
            moduledata.AddUint(ctxt.Arch, uint64(ctxt.Syms.Lookup("runtime.pclntab", 0L).Size)); 
            // The ftab slice
            moduledata.AddAddrPlus(ctxt.Arch, ctxt.Syms.Lookup("runtime.pclntab", 0L), int64(pclntabPclntabOffset));
            moduledata.AddUint(ctxt.Arch, uint64(pclntabNfunc + 1L));
            moduledata.AddUint(ctxt.Arch, uint64(pclntabNfunc + 1L)); 
            // The filetab slice
            moduledata.AddAddrPlus(ctxt.Arch, ctxt.Syms.Lookup("runtime.pclntab", 0L), int64(pclntabFiletabOffset));
            moduledata.AddUint(ctxt.Arch, uint64(len(ctxt.Filesyms)) + 1L);
            moduledata.AddUint(ctxt.Arch, uint64(len(ctxt.Filesyms)) + 1L); 
            // findfunctab
            moduledata.AddAddr(ctxt.Arch, ctxt.Syms.Lookup("runtime.findfunctab", 0L)); 
            // minpc, maxpc
            moduledata.AddAddr(ctxt.Arch, pclntabFirstFunc);
            moduledata.AddAddrPlus(ctxt.Arch, pclntabLastFunc, pclntabLastFunc.Size); 
            // pointers to specific parts of the module
            moduledata.AddAddr(ctxt.Arch, ctxt.Syms.Lookup("runtime.text", 0L));
            moduledata.AddAddr(ctxt.Arch, ctxt.Syms.Lookup("runtime.etext", 0L));
            moduledata.AddAddr(ctxt.Arch, ctxt.Syms.Lookup("runtime.noptrdata", 0L));
            moduledata.AddAddr(ctxt.Arch, ctxt.Syms.Lookup("runtime.enoptrdata", 0L));
            moduledata.AddAddr(ctxt.Arch, ctxt.Syms.Lookup("runtime.data", 0L));
            moduledata.AddAddr(ctxt.Arch, ctxt.Syms.Lookup("runtime.edata", 0L));
            moduledata.AddAddr(ctxt.Arch, ctxt.Syms.Lookup("runtime.bss", 0L));
            moduledata.AddAddr(ctxt.Arch, ctxt.Syms.Lookup("runtime.ebss", 0L));
            moduledata.AddAddr(ctxt.Arch, ctxt.Syms.Lookup("runtime.noptrbss", 0L));
            moduledata.AddAddr(ctxt.Arch, ctxt.Syms.Lookup("runtime.enoptrbss", 0L));
            moduledata.AddAddr(ctxt.Arch, ctxt.Syms.Lookup("runtime.end", 0L));
            moduledata.AddAddr(ctxt.Arch, ctxt.Syms.Lookup("runtime.gcdata", 0L));
            moduledata.AddAddr(ctxt.Arch, ctxt.Syms.Lookup("runtime.gcbss", 0L));
            moduledata.AddAddr(ctxt.Arch, ctxt.Syms.Lookup("runtime.types", 0L));
            moduledata.AddAddr(ctxt.Arch, ctxt.Syms.Lookup("runtime.etypes", 0L));

            if (ctxt.HeadType == objabi.Haix && ctxt.LinkMode == LinkExternal)
            { 
                // Add R_REF relocation to prevent ld's garbage collection of
                // runtime.rodata, runtime.erodata and runtime.epclntab.
                Action<@string> addRef = name =>
                {
                    var r = moduledata.AddRel();
                    r.Sym = ctxt.Syms.Lookup(name, 0L);
                    r.Type = objabi.R_XCOFFREF;
                    r.Siz = uint8(ctxt.Arch.PtrSize);
                }
;
                addRef("runtime.rodata");
                addRef("runtime.erodata");
                addRef("runtime.epclntab");

            } 

            // text section information
            moduledata.AddAddr(ctxt.Arch, ctxt.Syms.Lookup("runtime.textsectionmap", 0L));
            moduledata.AddUint(ctxt.Arch, uint64(nsections));
            moduledata.AddUint(ctxt.Arch, uint64(nsections)); 

            // The typelinks slice
            var typelinkSym = ctxt.Syms.Lookup("runtime.typelink", 0L);
            var ntypelinks = uint64(typelinkSym.Size) / 4L;
            moduledata.AddAddr(ctxt.Arch, typelinkSym);
            moduledata.AddUint(ctxt.Arch, ntypelinks);
            moduledata.AddUint(ctxt.Arch, ntypelinks); 
            // The itablinks slice
            moduledata.AddAddr(ctxt.Arch, ctxt.Syms.Lookup("runtime.itablink", 0L));
            moduledata.AddUint(ctxt.Arch, uint64(nitablinks));
            moduledata.AddUint(ctxt.Arch, uint64(nitablinks)); 
            // The ptab slice
            {
                var ptab = ctxt.Syms.ROLookup("go.plugin.tabs", 0L);

                if (ptab != null && ptab.Attr.Reachable())
                {
                    ptab.Attr |= sym.AttrLocal;
                    ptab.Type = sym.SRODATA;

                    var nentries = uint64(len(ptab.P) / 8L); // sizeof(nameOff) + sizeof(typeOff)
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
                addgostring(ctxt, moduledata, "go.link.thispluginpath", objabi.PathToPrefix(flagPluginPath.val));

                var pkghashes = ctxt.Syms.Lookup("go.link.pkghashes", 0L);
                pkghashes.Attr |= sym.AttrReachable;
                pkghashes.Attr |= sym.AttrLocal;
                pkghashes.Type = sym.SRODATA;

                {
                    var i__prev1 = i;
                    var l__prev1 = l;

                    foreach (var (__i, __l) in ctxt.Library)
                    {
                        i = __i;
                        l = __l; 
                        // pkghashes[i].name
                        addgostring(ctxt, pkghashes, fmt.Sprintf("go.link.pkgname.%d", i), l.Pkg); 
                        // pkghashes[i].linktimehash
                        addgostring(ctxt, pkghashes, fmt.Sprintf("go.link.pkglinkhash.%d", i), l.Hash); 
                        // pkghashes[i].runtimehash
                        var hash = ctxt.Syms.ROLookup("go.link.pkghash." + l.Pkg, 0L);
                        pkghashes.AddAddr(ctxt.Arch, hash);

                    }
            else

                    i = i__prev1;
                    l = l__prev1;
                }

                moduledata.AddAddr(ctxt.Arch, pkghashes);
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
                                addgostring(ctxt, moduledata, "go.link.thismodulename", thismodulename);

                var modulehashes = ctxt.Syms.Lookup("go.link.abihashes", 0L);
                modulehashes.Attr |= sym.AttrReachable;
                modulehashes.Attr |= sym.AttrLocal;
                modulehashes.Type = sym.SRODATA;

                {
                    var i__prev1 = i;

                    foreach (var (__i, __shlib) in ctxt.Shlibs)
                    {
                        i = __i;
                        shlib = __shlib; 
                        // modulehashes[i].modulename
                        var modulename = filepath.Base(shlib.Path);
                        addgostring(ctxt, modulehashes, fmt.Sprintf("go.link.libname.%d", i), modulename); 

                        // modulehashes[i].linktimehash
                        addgostring(ctxt, modulehashes, fmt.Sprintf("go.link.linkhash.%d", i), string(shlib.Hash)); 

                        // modulehashes[i].runtimehash
                        var abihash = ctxt.Syms.Lookup("go.link.abihash." + modulename, 0L);
                        abihash.Attr |= sym.AttrReachable;
                        modulehashes.AddAddr(ctxt.Arch, abihash);

                    }
            else


                    i = i__prev1;
                }

                moduledata.AddAddr(ctxt.Arch, modulehashes);
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
            var moduledatatype = ctxt.Syms.ROLookup("type.runtime.moduledata", 0L);
            moduledata.Size = decodetypeSize(ctxt.Arch, moduledatatype.P);
            moduledata.Grow(moduledata.Size);

            var lastmoduledatap = ctxt.Syms.Lookup("runtime.lastmoduledatap", 0L);
            if (lastmoduledatap.Type != sym.SDYNIMPORT)
            {
                lastmoduledatap.Type = sym.SNOPTRDATA;
                lastmoduledatap.Size = 0L; // overwrite existing value
                lastmoduledatap.AddAddr(ctxt.Arch, moduledata);

            }

        }

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
