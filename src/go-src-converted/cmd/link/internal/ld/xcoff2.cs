// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ld -- go2cs converted at 2020 October 09 05:50:36 UTC
// import "cmd/link/internal/ld" ==> using ld = go.cmd.link.@internal.ld_package
// Original source: C:\Go\src\cmd\link\internal\ld\xcoff2.go
using objabi = go.cmd.@internal.objabi_package;
using loader = go.cmd.link.@internal.loader_package;
using sym = go.cmd.link.@internal.sym_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace link {
namespace @internal
{
    public static partial class ld_package
    {
        // Temporary dumping around for sym.Symbol version of helper
        // functions in xcoff.go, still being used for some archs/oses.
        // FIXME: get rid of this file when dodata() is completely
        // converted.

        // xcoffUpdateOuterSize stores the size of outer symbols in order to have it
        // in the symbol table.
        private static void xcoffUpdateOuterSize(ptr<Link> _addr_ctxt, long size, sym.SymKind stype)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            if (size == 0L)
            {
                return ;
            }

            if (stype == sym.SRODATA || stype == sym.SRODATARELRO || stype == sym.SFUNCTAB || stype == sym.SSTRING)
            {
                goto __switch_break0;
            }
            if (stype == sym.STYPERELRO)
            {
                if (ctxt.UseRelro() && (ctxt.BuildMode == BuildModeCArchive || ctxt.BuildMode == BuildModeCShared || ctxt.BuildMode == BuildModePIE))
                { 
                    // runtime.types size must be removed, as it's a real symbol.
                    outerSymSize["typerel.*"] = size - ctxt.Syms.ROLookup("runtime.types", 0L).Size;
                    return ;

                }
                fallthrough = true;
            }
            if (fallthrough || stype == sym.STYPE)
            {
                if (!ctxt.DynlinkingGo())
                { 
                    // runtime.types size must be removed, as it's a real symbol.
                    outerSymSize["type.*"] = size - ctxt.Syms.ROLookup("runtime.types", 0L).Size;

                }
                goto __switch_break0;
            }
            if (stype == sym.SGOSTRING)
            {
                outerSymSize["go.string.*"] = size;
                goto __switch_break0;
            }
            if (stype == sym.SGOFUNC)
            {
                if (!ctxt.DynlinkingGo())
                {
                    outerSymSize["go.func.*"] = size;
                }
                goto __switch_break0;
            }
            if (stype == sym.SGOFUNCRELRO)
            {
                outerSymSize["go.funcrel.*"] = size;
                goto __switch_break0;
            }
            if (stype == sym.SGCBITS)
            {
                outerSymSize["runtime.gcbits.*"] = size;
                goto __switch_break0;
            }
            if (stype == sym.SITABLINK)
            {
                outerSymSize["runtime.itablink"] = size;
                goto __switch_break0;
            }
            // default: 
                Errorf(null, "unknown XCOFF outer symbol for type %s", stype.String());

            __switch_break0:;

        }

        // Xcoffadddynrel adds a dynamic relocation in a XCOFF file.
        // This relocation will be made by the loader.
        public static bool Xcoffadddynrel(ptr<Target> _addr_target, ptr<loader.Loader> _addr_ldr, ptr<sym.Symbol> _addr_s, ptr<sym.Reloc> _addr_r)
        {
            ref Target target = ref _addr_target.val;
            ref loader.Loader ldr = ref _addr_ldr.val;
            ref sym.Symbol s = ref _addr_s.val;
            ref sym.Reloc r = ref _addr_r.val;

            if (target.IsExternal())
            {
                return true;
            }

            if (s.Type <= sym.SPCLNTAB)
            {
                Errorf(s, "cannot have a relocation to %s in a text section symbol", r.Sym.Name);
                return false;
            }

            ptr<xcoffLoaderReloc> xldr = addr(new xcoffLoaderReloc(sym:s,roff:r.Off,));


            if (r.Type == objabi.R_ADDR) 
                if (s.Type == sym.SXCOFFTOC && r.Sym.Type == sym.SDYNIMPORT)
                { 
                    // Imported symbol relocation
                    foreach (var (i, dynsym) in xfile.loaderSymbols)
                    {
                        if (ldr.Syms[dynsym.sym].Name == r.Sym.Name)
                        {
                            xldr.symndx = int32(i + 3L); // +3 because of 3 section symbols
                            break;

                        }

                    }

                }
                else if (s.Type == sym.SDATA || s.Type == sym.SNOPTRDATA || s.Type == sym.SBUILDINFO || s.Type == sym.SXCOFFTOC)
                {

                    if (r.Sym.Sect.Seg == _addr_Segtext)                     else if (r.Sym.Sect.Seg == _addr_Segrodata) 
                        xldr.symndx = 0L; // .text
                    else if (r.Sym.Sect.Seg == _addr_Segdata) 
                        if (r.Sym.Type == sym.SBSS || r.Sym.Type == sym.SNOPTRBSS)
                        {
                            xldr.symndx = 2L; // .bss
                        }
                        else
                        {
                            xldr.symndx = 1L; // .data
                        }

                    else 
                        Errorf(s, "unknown segment for .loader relocation with symbol %s", r.Sym.Name);
                    
                }
                else
                {
                    Errorf(s, "unexpected type for .loader relocation R_ADDR for symbol %s: %s to %s", r.Sym.Name, s.Type, r.Sym.Type);
                    return false;
                }

                xldr.rtype = 0x3FUL << (int)(8L) + XCOFF_R_POS;
            else 
                Errorf(s, "unexpected .loader relocation to symbol: %s (type: %s)", r.Sym.Name, r.Type.String());
                return false;
                        xfile.loaderReloc = append(xfile.loaderReloc, xldr);
            return true;

        }
    }
}}}}
