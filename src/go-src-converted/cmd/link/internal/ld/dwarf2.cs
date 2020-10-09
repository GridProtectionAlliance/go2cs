// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// TODO/NICETOHAVE:
//   - eliminate DW_CLS_ if not used
//   - package info in compilation units
//   - assign types to their packages
//   - gdb uses c syntax, meaning clumsy quoting is needed for go identifiers. eg
//     ptype struct '[]uint8' and qualifiers need to be quoted away
//   - file:line info for variables
//   - make strings a typedef so prettyprinters can see the underlying string type

// package ld -- go2cs converted at 2020 October 09 05:49:27 UTC
// import "cmd/link/internal/ld" ==> using ld = go.cmd.link.@internal.ld_package
// Original source: C:\Go\src\cmd\link\internal\ld\dwarf2.go
using objabi = go.cmd.@internal.objabi_package;
using loader = go.cmd.link.@internal.loader_package;
using sym = go.cmd.link.@internal.sym_package;
using log = go.log_package;
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
        private static bool isDwarf64(ptr<Link> _addr_ctxt)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            return ctxt.HeadType == objabi.Haix;
        }

        // dwarfSecInfo2 is a replica of the dwarfSecInfo struct but with
        // *sym.Symbol content instead of loader.Sym content.
        private partial struct dwarfSecInfo2
        {
            public slice<ptr<sym.Symbol>> syms;
        }

        private static ptr<sym.Symbol> secSym(this ptr<dwarfSecInfo2> _addr_dsi)
        {
            ref dwarfSecInfo2 dsi = ref _addr_dsi.val;

            if (len(dsi.syms) == 0L)
            {
                return _addr_null!;
            }

            return _addr_dsi.syms[0L]!;

        }

        private static slice<ptr<sym.Symbol>> subSyms(this ptr<dwarfSecInfo2> _addr_dsi)
        {
            ref dwarfSecInfo2 dsi = ref _addr_dsi.val;

            if (len(dsi.syms) == 0L)
            {
                return new slice<ptr<sym.Symbol>>(new ptr<sym.Symbol>[] {  });
            }

            return dsi.syms[1L..];

        }

        private static slice<dwarfSecInfo2> dwarfp = default;

        /*
         *  Elf.
         */
        private static void dwarfaddshstrings(ptr<Link> _addr_ctxt, ptr<loader.SymbolBuilder> _addr_shstrtab)
        {
            ref Link ctxt = ref _addr_ctxt.val;
            ref loader.SymbolBuilder shstrtab = ref _addr_shstrtab.val;

            if (FlagW.val)
            { // disable dwarf
                return ;

            }

            @string secs = new slice<@string>(new @string[] { "abbrev", "frame", "info", "loc", "line", "pubnames", "pubtypes", "gdb_scripts", "ranges" });
            foreach (var (_, sec) in secs)
            {
                shstrtab.Addstring(".debug_" + sec);
                if (ctxt.IsExternal())
                {
                    shstrtab.Addstring(elfRelType + ".debug_" + sec);
                }
                else
                {
                    shstrtab.Addstring(".zdebug_" + sec);
                }

            }

        }

        // Add section symbols for DWARF debug info.  This is called before
        // dwarfaddelfheaders.
        private static void dwarfaddelfsectionsyms(ptr<Link> _addr_ctxt)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            if (FlagW.val)
            { // disable dwarf
                return ;

            }

            if (ctxt.LinkMode != LinkExternal)
            {
                return ;
            }

            var s = ctxt.Syms.Lookup(".debug_info", 0L);
            putelfsectionsym(ctxt, ctxt.Out, s, s.Sect.Elfsect._<ptr<ElfShdr>>().shnum);
            s = ctxt.Syms.Lookup(".debug_abbrev", 0L);
            putelfsectionsym(ctxt, ctxt.Out, s, s.Sect.Elfsect._<ptr<ElfShdr>>().shnum);
            s = ctxt.Syms.Lookup(".debug_line", 0L);
            putelfsectionsym(ctxt, ctxt.Out, s, s.Sect.Elfsect._<ptr<ElfShdr>>().shnum);
            s = ctxt.Syms.Lookup(".debug_frame", 0L);
            putelfsectionsym(ctxt, ctxt.Out, s, s.Sect.Elfsect._<ptr<ElfShdr>>().shnum);
            s = ctxt.Syms.Lookup(".debug_loc", 0L);
            if (s.Sect != null)
            {
                putelfsectionsym(ctxt, ctxt.Out, s, s.Sect.Elfsect._<ptr<ElfShdr>>().shnum);
            }

            s = ctxt.Syms.Lookup(".debug_ranges", 0L);
            if (s.Sect != null)
            {
                putelfsectionsym(ctxt, ctxt.Out, s, s.Sect.Elfsect._<ptr<ElfShdr>>().shnum);
            }

        }

        // dwarfcompress compresses the DWARF sections. Relocations are applied
        // on the fly. After this, dwarfp will contain a different (new) set of
        // symbols, and sections may have been replaced.
        private static void dwarfcompress(ptr<Link> _addr_ctxt)
        {
            ref Link ctxt = ref _addr_ctxt.val;
 
            // compressedSect is a helper type for parallelizing compression.
            private partial struct compressedSect
            {
                public long index;
                public slice<byte> compressed;
                public slice<loader.Sym> syms;
            }

            var supported = ctxt.IsELF || ctxt.IsWindows() || ctxt.IsDarwin();
            if (!ctxt.compressDWARF || !supported || ctxt.IsExternal())
            {
                return ;
            }

            long compressedCount = default;
            var resChannel = make_channel<compressedSect>();
            foreach (var (i) in dwarfp2)
            {
                go_(() => (resIndex, syms) =>
                {
                    resChannel.Send(new compressedSect(resIndex,compressSyms(ctxt,syms),syms));
                }(compressedCount, dwarfp2[i].syms));
                compressedCount++;

            }
            var res = make_slice<compressedSect>(compressedCount);
            while (compressedCount > 0L)
            {
                var r = resChannel.Receive();
                res[r.index] = r;
                compressedCount--;
            }


            var ldr = ctxt.loader;
            slice<dwarfSecInfo> newDwarfp = default;
            Segdwarf.Sections = Segdwarf.Sections[..0L];
            foreach (var (_, z) in res)
            {
                var s = z.syms[0L];
                if (z.compressed == null)
                { 
                    // Compression didn't help.
                    dwarfSecInfo ds = new dwarfSecInfo(syms:z.syms);
                    newDwarfp = append(newDwarfp, ds);
                    Segdwarf.Sections = append(Segdwarf.Sections, ldr.SymSect(s));

                }
                else
                {
                    @string compressedSegName = ".zdebug_" + ldr.SymSect(s).Name[len(".debug_")..];
                    var sect = addsection(ctxt.loader, ctxt.Arch, _addr_Segdwarf, compressedSegName, 04L);
                    sect.Align = 1L;
                    sect.Length = uint64(len(z.compressed));
                    var newSym = ldr.CreateSymForUpdate(compressedSegName, 0L);
                    newSym.SetReachable(true);
                    newSym.SetData(z.compressed);
                    newSym.SetSize(int64(len(z.compressed)));
                    ldr.SetSymSect(newSym.Sym(), sect);
                    ds = new dwarfSecInfo(syms:[]loader.Sym{newSym.Sym()});
                    newDwarfp = append(newDwarfp, ds); 

                    // compressed symbols are no longer needed.
                    {
                        var s__prev2 = s;

                        foreach (var (_, __s) in z.syms)
                        {
                            s = __s;
                            ldr.SetAttrReachable(s, false);
                            ldr.FreeSym(s);
                        }

                        s = s__prev2;
                    }
                }

            }
            dwarfp2 = newDwarfp; 

            // Re-compute the locations of the compressed DWARF symbols
            // and sections, since the layout of these within the file is
            // based on Section.Vaddr and Symbol.Value.
            var pos = Segdwarf.Vaddr;
            ptr<sym.Section> prevSect;
            foreach (var (_, si) in dwarfp2)
            {
                {
                    var s__prev2 = s;

                    foreach (var (_, __s) in si.syms)
                    {
                        s = __s;
                        ldr.SetSymValue(s, int64(pos));
                        sect = ldr.SymSect(s);
                        if (sect != prevSect)
                        {
                            sect.Vaddr = uint64(pos);
                            prevSect = sect;
                        }

                        if (ldr.SubSym(s) != 0L)
                        {
                            log.Fatalf("%s: unexpected sub-symbols", ldr.SymName(s));
                        }

                        pos += uint64(ldr.SymSize(s));
                        if (ctxt.IsWindows())
                        {
                            pos = uint64(Rnd(int64(pos), PEFILEALIGN));
                        }

                    }

                    s = s__prev2;
                }
            }
            Segdwarf.Length = pos - Segdwarf.Vaddr;

        }

        private partial struct compilationUnitByStartPC // : slice<ptr<sym.CompilationUnit>>
        {
        }

        private static long Len(this compilationUnitByStartPC v)
        {
            return len(v);
        }
        private static void Swap(this compilationUnitByStartPC v, long i, long j)
        {
            v[i] = v[j];
            v[j] = v[i];
        }

        private static bool Less(this compilationUnitByStartPC v, long i, long j)
        {

            if (len(v[i].Textp2) == 0L && len(v[j].Textp2) == 0L) 
                return v[i].Lib.Pkg < v[j].Lib.Pkg;
            else if (len(v[i].Textp2) != 0L && len(v[j].Textp2) == 0L) 
                return true;
            else if (len(v[i].Textp2) == 0L && len(v[j].Textp2) != 0L) 
                return false;
            else 
                return v[i].PCs[0L].Start < v[j].PCs[0L].Start;
            
        }
    }
}}}}
