// Derived from Inferno utils/6l/obj.c and utils/6l/span.c
// https://bitbucket.org/inferno-os/inferno-os/src/master/utils/6l/obj.c
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

// package ld -- go2cs converted at 2020 October 09 05:49:11 UTC
// import "cmd/link/internal/ld" ==> using ld = go.cmd.link.@internal.ld_package
// Original source: C:\Go\src\cmd\link\internal\ld\data.go
using bytes = go.bytes_package;
using gcprog = go.cmd.@internal.gcprog_package;
using objabi = go.cmd.@internal.objabi_package;
using sys = go.cmd.@internal.sys_package;
using loader = go.cmd.link.@internal.loader_package;
using sym = go.cmd.link.@internal.sym_package;
using zlib = go.compress.zlib_package;
using binary = go.encoding.binary_package;
using fmt = go.fmt_package;
using log = go.log_package;
using os = go.os_package;
using sort = go.sort_package;
using strconv = go.strconv_package;
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
        // isRuntimeDepPkg reports whether pkg is the runtime package or its dependency
        private static bool isRuntimeDepPkg(@string pkg)
        {
            switch (pkg)
            {
                case "runtime": // for cpu features

                case "sync/atomic": // for cpu features

                case "internal/bytealg": // for cpu features

                case "internal/cpu": // for cpu features
                    return true;
                    break;
            }
            return strings.HasPrefix(pkg, "runtime/internal/") && !strings.HasSuffix(pkg, "_test");

        }

        // Estimate the max size needed to hold any new trampolines created for this function. This
        // is used to determine when the section can be split if it becomes too large, to ensure that
        // the trampolines are in the same section as the function that uses them.
        private static ulong maxSizeTrampolinesPPC64(ptr<loader.Loader> _addr_ldr, loader.Sym s, bool isTramp)
        {
            ref loader.Loader ldr = ref _addr_ldr.val;
 
            // If thearch.Trampoline is nil, then trampoline support is not available on this arch.
            // A trampoline does not need any dependent trampolines.
            if (thearch.Trampoline == null || isTramp)
            {
                return 0L;
            }

            var n = uint64(0L);
            var relocs = ldr.Relocs(s);
            for (long ri = 0L; ri < relocs.Count(); ri++)
            {
                var r = relocs.At2(ri);
                if (r.Type().IsDirectCallOrJump())
                {
                    n++;
                }

            } 
            // Trampolines in ppc64 are 4 instructions.
 
            // Trampolines in ppc64 are 4 instructions.
            return n * 16L;

        }

        // detect too-far jumps in function s, and add trampolines if necessary
        // ARM, PPC64 & PPC64LE support trampoline insertion for internal and external linking
        // On PPC64 & PPC64LE the text sections might be split but will still insert trampolines
        // where necessary.
        private static void trampoline(ptr<Link> _addr_ctxt, loader.Sym s)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            if (thearch.Trampoline == null)
            {
                return ; // no need or no support of trampolines on this arch
            }

            var ldr = ctxt.loader;
            var relocs = ldr.Relocs(s);
            for (long ri = 0L; ri < relocs.Count(); ri++)
            {
                var r = relocs.At2(ri);
                if (!r.Type().IsDirectCallOrJump())
                {
                    continue;
                }

                var rs = r.Sym();
                if (!ldr.AttrReachable(rs) || ldr.SymType(rs) == sym.Sxxx)
                {
                    continue; // something is wrong. skip it here and we'll emit a better error later
                }

                rs = ldr.ResolveABIAlias(rs);
                if (ldr.SymValue(rs) == 0L && (ldr.SymType(rs) != sym.SDYNIMPORT && ldr.SymType(rs) != sym.SUNDEFEXT))
                {
                    if (ldr.SymPkg(rs) != ldr.SymPkg(s))
                    {
                        if (!isRuntimeDepPkg(ldr.SymPkg(s)) || !isRuntimeDepPkg(ldr.SymPkg(rs)))
                        {
                            ctxt.Errorf(s, "unresolved inter-package jump to %s(%s) from %s", ldr.SymName(rs), ldr.SymPkg(rs), ldr.SymPkg(s));
                        } 
                        // runtime and its dependent packages may call to each other.
                        // they are fine, as they will be laid down together.
                    }

                    continue;

                }

                thearch.Trampoline(ctxt, ldr, ri, rs, s);

            }



        }

        // foldSubSymbolOffset computes the offset of symbol s to its top-level outer
        // symbol. Returns the top-level symbol and the offset.
        // This is used in generating external relocations.
        private static (loader.Sym, long) foldSubSymbolOffset(ptr<loader.Loader> _addr_ldr, loader.Sym s)
        {
            loader.Sym _p0 = default;
            long _p0 = default;
            ref loader.Loader ldr = ref _addr_ldr.val;

            var outer = ldr.OuterSym(s);
            var off = int64(0L);
            while (outer != 0L)
            {
                off += ldr.SymValue(s) - ldr.SymValue(outer);
                s = outer;
                outer = ldr.OuterSym(s);
            }

            return (s, off);

        }

        // relocsym resolve relocations in "s", updating the symbol's content
        // in "P".
        // The main loop walks through the list of relocations attached to "s"
        // and resolves them where applicable. Relocations are often
        // architecture-specific, requiring calls into the 'archreloc' and/or
        // 'archrelocvariant' functions for the architecture. When external
        // linking is in effect, it may not be  possible to completely resolve
        // the address/offset for a symbol, in which case the goal is to lay
        // the groundwork for turning a given relocation into an external reloc
        // (to be applied by the external linker). For more on how relocations
        // work in general, see
        //
        //  "Linkers and Loaders", by John R. Levine (Morgan Kaufmann, 1999), ch. 7
        //
        // This is a performance-critical function for the linker; be careful
        // to avoid introducing unnecessary allocations in the main loop.
        private static void relocsym(this ptr<relocSymState> _addr_st, loader.Sym s, slice<byte> P) => func((_, panic, __) =>
        {
            ref relocSymState st = ref _addr_st.val;

            var ldr = st.ldr;
            var relocs = ldr.Relocs(s);
            if (relocs.Count() == 0L)
            {
                return ;
            }

            var target = st.target;
            var syms = st.syms;
            slice<loader.ExtReloc> extRelocs = default;
            if (target.IsExternal())
            { 
                // preallocate a slice conservatively assuming that all
                // relocs will require an external reloc
                extRelocs = st.preallocExtRelocSlice(relocs.Count());

            }

            for (long ri = 0L; ri < relocs.Count(); ri++)
            {
                var r = relocs.At2(ri);
                var off = r.Off();
                var siz = int32(r.Siz());
                var rs = r.Sym();
                rs = ldr.ResolveABIAlias(rs);
                var rt = r.Type();
                if (off < 0L || off + siz > int32(len(P)))
                {
                    @string rname = "";
                    if (rs != 0L)
                    {
                        rname = ldr.SymName(rs);
                    }

                    st.err.Errorf(s, "invalid relocation %s: %d+%d not in [%d,%d)", rname, off, siz, 0L, len(P));
                    continue;

                }

                sym.SymKind rst = default;
                if (rs != 0L)
                {
                    rst = ldr.SymType(rs);
                }

                if (rs != 0L && ((rst == sym.Sxxx && !ldr.AttrVisibilityHidden(rs)) || rst == sym.SXREF))
                { 
                    // When putting the runtime but not main into a shared library
                    // these symbols are undefined and that's OK.
                    if (target.IsShared() || target.IsPlugin())
                    {
                        if (ldr.SymName(rs) == "main.main" || (!target.IsPlugin() && ldr.SymName(rs) == "main..inittask"))
                        {
                            var sb = ldr.MakeSymbolUpdater(rs);
                            sb.SetType(sym.SDYNIMPORT);
                        }
                        else if (strings.HasPrefix(ldr.SymName(rs), "go.info."))
                        { 
                            // Skip go.info symbols. They are only needed to communicate
                            // DWARF info between the compiler and linker.
                            continue;

                        }

                    }
                    else
                    {
                        st.err.errorUnresolved(ldr, s, rs);
                        continue;
                    }

                }

                if (rt >= objabi.ElfRelocOffset)
                {
                    continue;
                }

                if (siz == 0L)
                { // informational relocation - no work to do
                    continue;

                } 

                // We need to be able to reference dynimport symbols when linking against
                // shared libraries, and Solaris, Darwin and AIX need it always
                if (!target.IsSolaris() && !target.IsDarwin() && !target.IsAIX() && rs != 0L && rst == sym.SDYNIMPORT && !target.IsDynlinkingGo() && !ldr.AttrSubSymbol(rs))
                {
                    if (!(target.IsPPC64() && target.IsExternal() && ldr.SymName(rs) == ".TOC."))
                    {
                        st.err.Errorf(s, "unhandled relocation for %s (type %d (%s) rtype %d (%s))", ldr.SymName(rs), rst, rst, rt, sym.RelocName(target.Arch, rt));
                    }

                }

                if (rs != 0L && rst != sym.STLSBSS && rt != objabi.R_WEAKADDROFF && rt != objabi.R_METHODOFF && !ldr.AttrReachable(rs))
                {
                    st.err.Errorf(s, "unreachable sym in relocation: %s", ldr.SymName(rs));
                }

                loader.ExtReloc rr = default;
                var needExtReloc = false; // will set to true below in case it is needed
                if (target.IsExternal())
                {
                    rr.Idx = ri;
                } 

                // TODO(mundaym): remove this special case - see issue 14218.
                //if target.IsS390X() {
                //    switch r.Type {
                //    case objabi.R_PCRELDBL:
                //        r.InitExt()
                //        r.Type = objabi.R_PCREL
                //        r.Variant = sym.RV_390_DBL
                //    case objabi.R_CALL:
                //        r.InitExt()
                //        r.Variant = sym.RV_390_DBL
                //    }
                //}
                long o = default;

                if (rt == objabi.R_TLS_LE)
                {
                    if (target.IsExternal() && target.IsElf())
                    {
                        needExtReloc = true;
                        rr.Xsym = rs;
                        if (rr.Xsym == 0L)
                        {
                            rr.Xsym = syms.Tlsg2;
                        }

                        rr.Xadd = r.Add();
                        o = 0L;
                        if (!target.IsAMD64())
                        {
                            o = r.Add();
                        }

                        break;

                    }

                    if (target.IsElf() && target.IsARM())
                    { 
                        // On ELF ARM, the thread pointer is 8 bytes before
                        // the start of the thread-local data block, so add 8
                        // to the actual TLS offset (r->sym->value).
                        // This 8 seems to be a fundamental constant of
                        // ELF on ARM (or maybe Glibc on ARM); it is not
                        // related to the fact that our own TLS storage happens
                        // to take up 8 bytes.
                        o = 8L + ldr.SymValue(rs);

                    }
                    else if (target.IsElf() || target.IsPlan9() || target.IsDarwin())
                    {
                        o = int64(syms.Tlsoffset) + r.Add();
                    }
                    else if (target.IsWindows())
                    {
                        o = r.Add();
                    }
                    else
                    {
                        log.Fatalf("unexpected R_TLS_LE relocation for %v", target.HeadType);
                    }

                    goto __switch_break0;
                }
                if (rt == objabi.R_TLS_IE)
                {
                    if (target.IsExternal() && target.IsElf())
                    {
                        needExtReloc = true;
                        rr.Xsym = rs;
                        if (rr.Xsym == 0L)
                        {
                            rr.Xsym = syms.Tlsg2;
                        }

                        rr.Xadd = r.Add();
                        o = 0L;
                        if (!target.IsAMD64())
                        {
                            o = r.Add();
                        }

                        break;

                    }

                    if (target.IsPIE() && target.IsElf())
                    { 
                        // We are linking the final executable, so we
                        // can optimize any TLS IE relocation to LE.
                        if (thearch.TLSIEtoLE == null)
                        {
                            log.Fatalf("internal linking of TLS IE not supported on %v", target.Arch.Family);
                        }

                        thearch.TLSIEtoLE(P, int(off), int(siz));
                        o = int64(syms.Tlsoffset);

                    }
                    else
                    {
                        log.Fatalf("cannot handle R_TLS_IE (sym %s) when linking internally", ldr.SymName(s));
                    }

                    goto __switch_break0;
                }
                if (rt == objabi.R_ADDR)
                {
                    if (target.IsExternal() && rst != sym.SCONST)
                    {
                        needExtReloc = true; 

                        // set up addend for eventual relocation via outer symbol.
                        rs = rs;
                        var (rs, off) = foldSubSymbolOffset(_addr_ldr, rs);
                        rr.Xadd = r.Add() + off;
                        rst = ldr.SymType(rs);
                        if (rst != sym.SHOSTOBJ && rst != sym.SDYNIMPORT && rst != sym.SUNDEFEXT && ldr.SymSect(rs) == null)
                        {
                            st.err.Errorf(s, "missing section for relocation target %s", ldr.SymName(rs));
                        }

                        rr.Xsym = rs;

                        o = rr.Xadd;
                        if (target.IsElf())
                        {
                            if (target.IsAMD64())
                            {
                                o = 0L;
                            }

                        }
                        else if (target.IsDarwin())
                        {
                            if (ldr.SymType(rs) != sym.SHOSTOBJ)
                            {
                                o += ldr.SymValue(rs);
                            }

                        }
                        else if (target.IsWindows())
                        { 
                            // nothing to do
                        }
                        else if (target.IsAIX())
                        {
                            o = ldr.SymValue(rs) + r.Add();
                        }
                        else
                        {
                            st.err.Errorf(s, "unhandled pcrel relocation to %s on %v", ldr.SymName(rs), target.HeadType);
                        }

                        break;

                    } 

                    // On AIX, a second relocation must be done by the loader,
                    // as section addresses can change once loaded.
                    // The "default" symbol address is still needed by the loader so
                    // the current relocation can't be skipped.
                    if (target.IsAIX() && rst != sym.SDYNIMPORT)
                    { 
                        // It's not possible to make a loader relocation in a
                        // symbol which is not inside .data section.
                        // FIXME: It should be forbidden to have R_ADDR from a
                        // symbol which isn't in .data. However, as .text has the
                        // same address once loaded, this is possible.
                        if (ldr.SymSect(s).Seg == _addr_Segdata)
                        {
                            panic("not implemented"); 
                            //Xcoffadddynrel(target, ldr, err, s, &r) // XXX
                        }

                    }

                    o = ldr.SymValue(rs) + r.Add(); 

                    // On amd64, 4-byte offsets will be sign-extended, so it is impossible to
                    // access more than 2GB of static data; fail at link time is better than
                    // fail at runtime. See https://golang.org/issue/7980.
                    // Instead of special casing only amd64, we treat this as an error on all
                    // 64-bit architectures so as to be future-proof.
                    if (int32(o) < 0L && target.Arch.PtrSize > 4L && siz == 4L)
                    {
                        st.err.Errorf(s, "non-pc-relative relocation address for %s is too big: %#x (%#x + %#x)", ldr.SymName(rs), uint64(o), ldr.SymValue(rs), r.Add());
                        errorexit();
                    }

                    goto __switch_break0;
                }
                if (rt == objabi.R_DWARFSECREF)
                {
                    if (ldr.SymSect(rs) == null)
                    {
                        st.err.Errorf(s, "missing DWARF section for relocation target %s", ldr.SymName(rs));
                    }

                    if (target.IsExternal())
                    {
                        needExtReloc = true; 

                        // On most platforms, the external linker needs to adjust DWARF references
                        // as it combines DWARF sections. However, on Darwin, dsymutil does the
                        // DWARF linking, and it understands how to follow section offsets.
                        // Leaving in the relocation records confuses it (see
                        // https://golang.org/issue/22068) so drop them for Darwin.
                        if (target.IsDarwin())
                        {
                            needExtReloc = false;
                        }

                        rr.Xsym = loader.Sym(ldr.SymSect(rs).Sym2);
                        rr.Xadd = r.Add() + ldr.SymValue(rs) - int64(ldr.SymSect(rs).Vaddr);

                        o = rr.Xadd;
                        if (target.IsElf() && target.IsAMD64())
                        {
                            o = 0L;
                        }

                        break;

                    }

                    o = ldr.SymValue(rs) + r.Add() - int64(ldr.SymSect(rs).Vaddr);
                    goto __switch_break0;
                }
                if (rt == objabi.R_WEAKADDROFF || rt == objabi.R_METHODOFF)
                {
                    if (!ldr.AttrReachable(rs))
                    {
                        continue;
                    }

                    fallthrough = true;
                }
                if (fallthrough || rt == objabi.R_ADDROFF) 
                {
                    // The method offset tables using this relocation expect the offset to be relative
                    // to the start of the first text section, even if there are multiple.
                    if (ldr.SymSect(rs).Name == ".text")
                    {
                        o = ldr.SymValue(rs) - int64(Segtext.Sections[0L].Vaddr) + r.Add();
                    }
                    else
                    {
                        o = ldr.SymValue(rs) - int64(ldr.SymSect(rs).Vaddr) + r.Add();
                    }

                    goto __switch_break0;
                }
                if (rt == objabi.R_ADDRCUOFF) 
                {
                    // debug_range and debug_loc elements use this relocation type to get an
                    // offset from the start of the compile unit.
                    o = ldr.SymValue(rs) + r.Add() - ldr.SymValue(loader.Sym(ldr.SymUnit(rs).Textp2[0L])); 

                    // r.Sym() can be 0 when CALL $(constant) is transformed from absolute PC to relative PC call.
                    goto __switch_break0;
                }
                if (rt == objabi.R_GOTPCREL)
                {
                    if (target.IsDynlinkingGo() && target.IsDarwin() && rs != 0L && rst != sym.SCONST)
                    {
                        needExtReloc = true;
                        rr.Xadd = r.Add();
                        rr.Xadd -= int64(siz); // relative to address after the relocated chunk
                        rr.Xsym = rs;

                        o = rr.Xadd;
                        o += int64(siz);
                        break;

                    }

                    fallthrough = true;
                }
                if (fallthrough || rt == objabi.R_CALL || rt == objabi.R_PCREL)
                {
                    if (target.IsExternal() && rs != 0L && rst == sym.SUNDEFEXT)
                    { 
                        // pass through to the external linker.
                        needExtReloc = true;
                        rr.Xadd = 0L;
                        if (target.IsElf())
                        {
                            rr.Xadd -= int64(siz);
                        }

                        rr.Xsym = rs;
                        o = 0L;
                        break;

                    }

                    if (target.IsExternal() && rs != 0L && rst != sym.SCONST && (ldr.SymSect(rs) != ldr.SymSect(s) || rt == objabi.R_GOTPCREL))
                    {
                        needExtReloc = true; 

                        // set up addend for eventual relocation via outer symbol.
                        rs = rs;
                        (rs, off) = foldSubSymbolOffset(_addr_ldr, rs);
                        rr.Xadd = r.Add() + off;
                        rr.Xadd -= int64(siz); // relative to address after the relocated chunk
                        rst = ldr.SymType(rs);
                        if (rst != sym.SHOSTOBJ && rst != sym.SDYNIMPORT && ldr.SymSect(rs) == null)
                        {
                            st.err.Errorf(s, "missing section for relocation target %s", ldr.SymName(rs));
                        }

                        rr.Xsym = rs;

                        o = rr.Xadd;
                        if (target.IsElf())
                        {
                            if (target.IsAMD64())
                            {
                                o = 0L;
                            }

                        }
                        else if (target.IsDarwin())
                        {
                            if (rt == objabi.R_CALL)
                            {
                                if (target.IsExternal() && rst == sym.SDYNIMPORT)
                                {
                                    if (target.IsAMD64())
                                    { 
                                        // AMD64 dynamic relocations are relative to the end of the relocation.
                                        o += int64(siz);

                                    }

                                }
                                else
                                {
                                    if (rst != sym.SHOSTOBJ)
                                    {
                                        o += int64(uint64(ldr.SymValue(rs)) - ldr.SymSect(rs).Vaddr);
                                    }

                                    o -= int64(off); // relative to section offset, not symbol
                                }

                            }
                            else
                            {
                                o += int64(siz);
                            }

                        }
                        else if (target.IsWindows() && target.IsAMD64())
                        { // only amd64 needs PCREL
                            // PE/COFF's PC32 relocation uses the address after the relocated
                            // bytes as the base. Compensate by skewing the addend.
                            o += int64(siz);

                        }
                        else
                        {
                            st.err.Errorf(s, "unhandled pcrel relocation to %s on %v", ldr.SymName(rs), target.HeadType);
                        }

                        break;

                    }

                    o = 0L;
                    if (rs != 0L)
                    {
                        o = ldr.SymValue(rs);
                    }

                    o += r.Add() - (ldr.SymValue(s) + int64(off) + int64(siz));
                    goto __switch_break0;
                }
                if (rt == objabi.R_SIZE)
                {
                    o = ldr.SymSize(rs) + r.Add();
                    goto __switch_break0;
                }
                if (rt == objabi.R_XCOFFREF)
                {
                    if (!target.IsAIX())
                    {
                        st.err.Errorf(s, "find XCOFF R_REF on non-XCOFF files");
                    }

                    if (!target.IsExternal())
                    {
                        st.err.Errorf(s, "find XCOFF R_REF with internal linking");
                    }

                    needExtReloc = true;
                    rr.Xsym = rs;
                    rr.Xadd = r.Add(); 

                    // This isn't a real relocation so it must not update
                    // its offset value.
                    continue;
                    goto __switch_break0;
                }
                if (rt == objabi.R_DWARFFILEREF) 
                {
                    // We don't renumber files in dwarf.go:writelines anymore.
                    continue;
                    goto __switch_break0;
                }
                if (rt == objabi.R_CONST)
                {
                    o = r.Add();
                    goto __switch_break0;
                }
                if (rt == objabi.R_GOTOFF)
                {
                    o = ldr.SymValue(rs) + r.Add() - ldr.SymValue(syms.GOT2);
                    goto __switch_break0;
                }
                // default: 
                    switch (siz)
                    {
                        case 1L: 
                            o = int64(P[off]);
                            break;
                        case 2L: 
                            o = int64(target.Arch.ByteOrder.Uint16(P[off..]));
                            break;
                        case 4L: 
                            o = int64(target.Arch.ByteOrder.Uint32(P[off..]));
                            break;
                        case 8L: 
                            o = int64(target.Arch.ByteOrder.Uint64(P[off..]));
                            break;
                        default: 
                            st.err.Errorf(s, "bad reloc size %#x for %s", uint32(siz), ldr.SymName(rs));
                            break;
                    }
                    ptr<loader.ExtReloc> rp;
                    if (target.IsExternal())
                    { 
                        // Don't pass &rr directly to Archreloc2, which will escape rr
                        // even if this case is not taken. Instead, as Archreloc2 will
                        // likely return true, we speculatively add rr to extRelocs
                        // and use that space to pass to Archreloc2.
                        extRelocs = append(extRelocs, rr);
                        rp = _addr_extRelocs[len(extRelocs) - 1L];

                    }

                    var (out, needExtReloc1, ok) = thearch.Archreloc2(target, ldr, syms, r, rp, s, o);
                    if (target.IsExternal() && !needExtReloc1)
                    { 
                        // Speculation failed. Undo the append.
                        extRelocs = extRelocs[..len(extRelocs) - 1L];

                    }

                    needExtReloc = false; // already appended
                    if (ok)
                    {
                        o = out;
                    }
                    else
                    {
                        st.err.Errorf(s, "unknown reloc to %v: %d (%s)", ldr.SymName(rs), rt, sym.RelocName(target.Arch, rt));
                    }


                __switch_break0:; 

                //if target.IsPPC64() || target.IsS390X() {
                //    r.InitExt()
                //    if r.Variant != sym.RV_NONE {
                //        o = thearch.Archrelocvariant(ldr, target, syms, &r, s, o)
                //    }
                //}

                switch (siz)
                {
                    case 1L: 
                        P[off] = byte(int8(o));
                        break;
                    case 2L: 
                        if (o != int64(int16(o)))
                        {
                            st.err.Errorf(s, "relocation address for %s is too big: %#x", ldr.SymName(rs), o);
                        }

                        target.Arch.ByteOrder.PutUint16(P[off..], uint16(o));
                        break;
                    case 4L: 
                        if (rt == objabi.R_PCREL || rt == objabi.R_CALL)
                        {
                            if (o != int64(int32(o)))
                            {
                                st.err.Errorf(s, "pc-relative relocation address for %s is too big: %#x", ldr.SymName(rs), o);
                            }

                        }
                        else
                        {
                            if (o != int64(int32(o)) && o != int64(uint32(o)))
                            {
                                st.err.Errorf(s, "non-pc-relative relocation address for %s is too big: %#x", ldr.SymName(rs), uint64(o));
                            }

                        }

                        target.Arch.ByteOrder.PutUint32(P[off..], uint32(o));
                        break;
                    case 8L: 
                        target.Arch.ByteOrder.PutUint64(P[off..], uint64(o));
                        break;
                    default: 
                        st.err.Errorf(s, "bad reloc size %#x for %s", uint32(siz), ldr.SymName(rs));
                        break;
                }

                if (needExtReloc)
                {
                    extRelocs = append(extRelocs, rr);
                }

            }

            if (len(extRelocs) != 0L)
            {
                st.finalizeExtRelocSlice(extRelocs);
                ldr.SetExtRelocs(s, extRelocs);
            }

        });

        private static readonly long extRelocSlabSize = (long)2048L;

        // relocSymState hold state information needed when making a series of
        // successive calls to relocsym(). The items here are invariant
        // (meaning that they are set up once initially and then don't change
        // during the execution of relocsym), with the exception of a slice
        // used to facilitate batch allocation of external relocations. Calls
        // to relocsym happen in parallel; the assumption is that each
        // parallel thread will have its own state object.


        // relocSymState hold state information needed when making a series of
        // successive calls to relocsym(). The items here are invariant
        // (meaning that they are set up once initially and then don't change
        // during the execution of relocsym), with the exception of a slice
        // used to facilitate batch allocation of external relocations. Calls
        // to relocsym happen in parallel; the assumption is that each
        // parallel thread will have its own state object.
        private partial struct relocSymState
        {
            public ptr<Target> target;
            public ptr<loader.Loader> ldr;
            public ptr<ErrorReporter> err;
            public ptr<ArchSyms> syms;
            public slice<loader.ExtReloc> batch;
        }

        // preallocExtRelocs returns a subslice from an internally allocated
        // slab owned by the state object. Client requests a slice of size
        // 'sz', however it may be that fewer relocs are needed; the
        // assumption is that the final size is set in a [required] subsequent
        // call to 'finalizeExtRelocSlice'.
        private static slice<loader.ExtReloc> preallocExtRelocSlice(this ptr<relocSymState> _addr_st, long sz)
        {
            ref relocSymState st = ref _addr_st.val;

            if (len(st.batch) < sz)
            {
                var slabSize = extRelocSlabSize;
                if (sz > extRelocSlabSize)
                {
                    slabSize = sz;
                }

                st.batch = make_slice<loader.ExtReloc>(slabSize);

            }

            var rval = st.batch.slice(-1, sz, sz);
            return rval[..0L];

        }

        // finalizeExtRelocSlice takes a slice returned from preallocExtRelocSlice,
        // from which it determines how many of the pre-allocated relocs were
        // actually needed; it then carves that number off the batch slice.
        private static void finalizeExtRelocSlice(this ptr<relocSymState> _addr_st, slice<loader.ExtReloc> finalsl) => func((_, panic, __) =>
        {
            ref relocSymState st = ref _addr_st.val;

            if (_addr_st.batch[0L] != _addr_finalsl[0L])
            {
                panic("preallocExtRelocSlice size invariant violation");
            }

            st.batch = st.batch[len(finalsl)..];

        });

        // makeRelocSymState creates a relocSymState container object to
        // pass to relocsym(). If relocsym() calls happen in parallel,
        // each parallel thread should have its own state object.
        private static ptr<relocSymState> makeRelocSymState(this ptr<Link> _addr_ctxt)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            return addr(new relocSymState(target:&ctxt.Target,ldr:ctxt.loader,err:&ctxt.ErrorReporter,syms:&ctxt.ArchSyms,));
        }

        private static void reloc(this ptr<Link> _addr_ctxt)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            sync.WaitGroup wg = default;
            var ldr = ctxt.loader;
            if (ctxt.IsExternal())
            {
                ldr.InitExtRelocs();
            }

            wg.Add(3L);
            go_(() => () =>
            {
                if (!ctxt.IsWasm())
                { // On Wasm, text relocations are applied in Asmb2.
                    var st = ctxt.makeRelocSymState();
                    {
                        var s__prev1 = s;

                        foreach (var (_, __s) in ctxt.Textp2)
                        {
                            s = __s;
                            st.relocsym(s, ldr.OutData(s));
                        }

                        s = s__prev1;
                    }
                }

                wg.Done();

            }());
            go_(() => () =>
            {
                st = ctxt.makeRelocSymState();
                {
                    var s__prev1 = s;

                    foreach (var (_, __s) in ctxt.datap2)
                    {
                        s = __s;
                        st.relocsym(s, ldr.OutData(s));
                    }

                    s = s__prev1;
                }

                wg.Done();

            }());
            go_(() => () =>
            {
                st = ctxt.makeRelocSymState();
                foreach (var (_, si) in dwarfp2)
                {
                    {
                        var s__prev2 = s;

                        foreach (var (_, __s) in si.syms)
                        {
                            s = __s;
                            st.relocsym(s, ldr.OutData(s));
                        }

                        s = s__prev2;
                    }
                }
                wg.Done();

            }());
            wg.Wait();

        }

        private static void windynrelocsym(ptr<Link> _addr_ctxt, ptr<loader.SymbolBuilder> _addr_rel, loader.Sym s)
        {
            ref Link ctxt = ref _addr_ctxt.val;
            ref loader.SymbolBuilder rel = ref _addr_rel.val;

            ptr<loader.SymbolBuilder> su;
            var relocs = ctxt.loader.Relocs(s);
            for (long ri = 0L; ri < relocs.Count(); ri++)
            {
                var r = relocs.At2(ri);
                var targ = r.Sym();
                if (targ == 0L)
                {
                    continue;
                }

                var rt = r.Type();
                if (!ctxt.loader.AttrReachable(targ))
                {
                    if (rt == objabi.R_WEAKADDROFF)
                    {
                        continue;
                    }

                    ctxt.Errorf(s, "dynamic relocation to unreachable symbol %s", ctxt.loader.SymName(targ));

                }

                var tplt = ctxt.loader.SymPlt(targ);
                var tgot = ctxt.loader.SymGot(targ);
                if (tplt == -2L && tgot != -2L)
                { // make dynimport JMP table for PE object files.
                    tplt = int32(rel.Size());
                    ctxt.loader.SetPlt(targ, tplt);

                    if (su == null)
                    {
                        su = ctxt.loader.MakeSymbolUpdater(s);
                    }

                    r.SetSym(rel.Sym());
                    r.SetAdd(int64(tplt)); 

                    // jmp *addr

                    if (ctxt.Arch.Family == sys.I386) 
                        rel.AddUint8(0xffUL);
                        rel.AddUint8(0x25UL);
                        rel.AddAddrPlus(ctxt.Arch, targ, 0L);
                        rel.AddUint8(0x90UL);
                        rel.AddUint8(0x90UL);
                    else if (ctxt.Arch.Family == sys.AMD64) 
                        rel.AddUint8(0xffUL);
                        rel.AddUint8(0x24UL);
                        rel.AddUint8(0x25UL);
                        rel.AddAddrPlus4(ctxt.Arch, targ, 0L);
                        rel.AddUint8(0x90UL);
                    else 
                        ctxt.Errorf(s, "unsupported arch %v", ctxt.Arch.Family);
                        return ;
                    
                }
                else if (tplt >= 0L)
                {
                    if (su == null)
                    {
                        su = ctxt.loader.MakeSymbolUpdater(s);
                    }

                    r.SetSym(rel.Sym());
                    r.SetAdd(int64(tplt));

                }

            }


        }

        // windynrelocsyms generates jump table to C library functions that will be
        // added later. windynrelocsyms writes the table into .rel symbol.
        private static void windynrelocsyms(this ptr<Link> _addr_ctxt)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            if (!(ctxt.IsWindows() && iscgo && ctxt.IsInternal()))
            {
                return ;
            }

            var rel = ctxt.loader.LookupOrCreateSym(".rel", 0L);
            var relu = ctxt.loader.MakeSymbolUpdater(rel);
            relu.SetType(sym.STEXT);

            foreach (var (_, s) in ctxt.Textp2)
            {
                windynrelocsym(_addr_ctxt, _addr_relu, s);
            }
            ctxt.Textp2 = append(ctxt.Textp2, rel);

        }

        private static void dynrelocsym2(ptr<Link> _addr_ctxt, loader.Sym s)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            var target = _addr_ctxt.Target;
            var ldr = ctxt.loader;
            var syms = _addr_ctxt.ArchSyms;
            var relocs = ldr.Relocs(s);
            for (long ri = 0L; ri < relocs.Count(); ri++)
            {
                var r = relocs.At2(ri);
                if (ctxt.BuildMode == BuildModePIE && ctxt.LinkMode == LinkInternal)
                { 
                    // It's expected that some relocations will be done
                    // later by relocsym (R_TLS_LE, R_ADDROFF), so
                    // don't worry if Adddynrel returns false.
                    thearch.Adddynrel2(target, ldr, syms, s, r, ri);
                    continue;

                }

                var rSym = r.Sym();
                if (rSym != 0L && ldr.SymType(rSym) == sym.SDYNIMPORT || r.Type() >= objabi.ElfRelocOffset)
                {
                    if (rSym != 0L && !ldr.AttrReachable(rSym))
                    {
                        ctxt.Errorf(s, "dynamic relocation to unreachable symbol %s", ldr.SymName(rSym));
                    }

                    if (!thearch.Adddynrel2(target, ldr, syms, s, r, ri))
                    {
                        ctxt.Errorf(s, "unsupported dynamic relocation for symbol %s (type=%d (%s) stype=%d (%s))", ldr.SymName(rSym), r.Type(), sym.RelocName(ctxt.Arch, r.Type()), ldr.SymType(rSym), ldr.SymType(rSym));
                    }

                }

            }


        }

        private static void dynreloc2(this ptr<dodataState> _addr_state, ptr<Link> _addr_ctxt)
        {
            ref dodataState state = ref _addr_state.val;
            ref Link ctxt = ref _addr_ctxt.val;

            if (ctxt.HeadType == objabi.Hwindows)
            {
                return ;
            } 
            // -d suppresses dynamic loader format, so we may as well not
            // compute these sections or mark their symbols as reachable.
            if (FlagD.val)
            {
                return ;
            }

            {
                var s__prev1 = s;

                foreach (var (_, __s) in ctxt.Textp2)
                {
                    s = __s;
                    dynrelocsym2(_addr_ctxt, s);
                }

                s = s__prev1;
            }

            foreach (var (_, syms) in state.data2)
            {
                {
                    var s__prev2 = s;

                    foreach (var (_, __s) in syms)
                    {
                        s = __s;
                        dynrelocsym2(_addr_ctxt, s);
                    }

                    s = s__prev2;
                }
            }
            if (ctxt.IsELF)
            {
                elfdynhash2(ctxt);
            }

        }

        public static void Codeblk(ptr<Link> _addr_ctxt, ptr<OutBuf> _addr_@out, long addr, long size)
        {
            ref Link ctxt = ref _addr_ctxt.val;
            ref OutBuf @out = ref _addr_@out.val;

            CodeblkPad(_addr_ctxt, _addr_out, addr, size, zeros[..]);
        }

        public static void CodeblkPad(ptr<Link> _addr_ctxt, ptr<OutBuf> _addr_@out, long addr, long size, slice<byte> pad)
        {
            ref Link ctxt = ref _addr_ctxt.val;
            ref OutBuf @out = ref _addr_@out.val;

            writeBlocks(_addr_out, ctxt.outSem, _addr_ctxt.loader, ctxt.Textp2, addr, size, pad);
        }

        private static readonly long blockSize = (long)1L << (int)(20L); // 1MB chunks written at a time.

        // writeBlocks writes a specified chunk of symbols to the output buffer. It
        // breaks the write up into ≥blockSize chunks to write them out, and schedules
        // as many goroutines as necessary to accomplish this task. This call then
        // blocks, waiting on the writes to complete. Note that we use the sem parameter
        // to limit the number of concurrent writes taking place.
 // 1MB chunks written at a time.

        // writeBlocks writes a specified chunk of symbols to the output buffer. It
        // breaks the write up into ≥blockSize chunks to write them out, and schedules
        // as many goroutines as necessary to accomplish this task. This call then
        // blocks, waiting on the writes to complete. Note that we use the sem parameter
        // to limit the number of concurrent writes taking place.
        private static void writeBlocks(ptr<OutBuf> _addr_@out, channel<long> sem, ptr<loader.Loader> _addr_ldr, slice<loader.Sym> syms, long addr, long size, slice<byte> pad)
        {
            ref OutBuf @out = ref _addr_@out.val;
            ref loader.Loader ldr = ref _addr_ldr.val;

            {
                var i__prev1 = i;
                var s__prev1 = s;

                foreach (var (__i, __s) in syms)
                {
                    i = __i;
                    s = __s;
                    if (ldr.SymValue(s) >= addr && !ldr.AttrSubSymbol(s))
                    {
                        syms = syms[i..];
                        break;
                    }

                }

                i = i__prev1;
                s = s__prev1;
            }

            sync.WaitGroup wg = default;
            var max = int64(blockSize);
            var lastAddr = addr + size;
            var written = int64(0L);
            while (addr < lastAddr)
            { 
                // Find the last symbol we'd write.
                long idx = -1L;
                {
                    var i__prev2 = i;
                    var s__prev2 = s;

                    foreach (var (__i, __s) in syms)
                    {
                        i = __i;
                        s = __s;
                        if (ldr.AttrSubSymbol(s))
                        {
                            continue;
                        } 

                        // If the next symbol's size would put us out of bounds on the total length,
                        // stop looking.
                        var end = ldr.SymValue(s) + ldr.SymSize(s);
                        if (end > lastAddr)
                        {
                            break;
                        } 

                        // We're gonna write this symbol.
                        idx = i; 

                        // If we cross over the max size, we've got enough symbols.
                        if (end > addr + max)
                        {
                            break;
                        }

                    } 

                    // If we didn't find any symbols to write, we're done here.

                    i = i__prev2;
                    s = s__prev2;
                }

                if (idx < 0L)
                {
                    break;
                } 

                // Compute the length to write, including padding.
                // We need to write to the end address (lastAddr), or the next symbol's
                // start address, whichever comes first. If there is no more symbols,
                // just write to lastAddr. This ensures we don't leave holes between the
                // blocks or at the end.
                var length = int64(0L);
                if (idx + 1L < len(syms))
                { 
                    // Find the next top-level symbol.
                    // Skip over sub symbols so we won't split a containter symbol
                    // into two blocks.
                    var next = syms[idx + 1L];
                    while (ldr.AttrSubSymbol(next))
                    {
                        idx++;
                        next = syms[idx + 1L];
                    }

                    length = ldr.SymValue(next) - addr;

                }

                if (length == 0L || length > lastAddr - addr)
                {
                    length = lastAddr - addr;
                } 

                // Start the block output operator.
                {
                    var (o, err) = @out.View(uint64(@out.Offset() + written));

                    if (err == null)
                    {
                        sem.Send(1L);
                        wg.Add(1L);
                        go_(() => (o, ldr, syms, addr, size, pad) =>
                        {
                            writeBlock(_addr_o, _addr_ldr, syms, addr, size, pad);
                            wg.Done().Send(sem);
                        }
                    else
(o, ldr, syms, addr, length, pad));

                    }                    { // output not mmaped, don't parallelize.
                        writeBlock(_addr_out, _addr_ldr, syms, addr, length, pad);

                    } 

                    // Prepare for the next loop.

                } 

                // Prepare for the next loop.
                if (idx != -1L)
                {
                    syms = syms[idx + 1L..];
                }

                written += length;
                addr += length;

            }

            wg.Wait();

        }

        private static void writeBlock(ptr<OutBuf> _addr_@out, ptr<loader.Loader> _addr_ldr, slice<loader.Sym> syms, long addr, long size, slice<byte> pad)
        {
            ref OutBuf @out = ref _addr_@out.val;
            ref loader.Loader ldr = ref _addr_ldr.val;

            {
                var s__prev1 = s;

                foreach (var (__i, __s) in syms)
                {
                    i = __i;
                    s = __s;
                    if (ldr.SymValue(s) >= addr && !ldr.AttrSubSymbol(s))
                    {
                        syms = syms[i..];
                        break;
                    }

                } 

                // This doesn't distinguish the memory size from the file
                // size, and it lays out the file based on Symbol.Value, which
                // is the virtual address. DWARF compression changes file sizes,
                // so dwarfcompress will fix this up later if necessary.

                s = s__prev1;
            }

            var eaddr = addr + size;
            {
                var s__prev1 = s;

                foreach (var (_, __s) in syms)
                {
                    s = __s;
                    if (ldr.AttrSubSymbol(s))
                    {
                        continue;
                    }

                    var val = ldr.SymValue(s);
                    if (val >= eaddr)
                    {
                        break;
                    }

                    if (val < addr)
                    {
                        ldr.Errorf(s, "phase error: addr=%#x but sym=%#x type=%d", addr, val, ldr.SymType(s));
                        errorexit();
                    }

                    if (addr < val)
                    {
                        @out.WriteStringPad("", int(val - addr), pad);
                        addr = val;
                    }

                    @out.WriteSym(ldr, s);
                    addr += int64(len(ldr.Data(s)));
                    var siz = ldr.SymSize(s);
                    if (addr < val + siz)
                    {
                        @out.WriteStringPad("", int(val + siz - addr), pad);
                        addr = val + siz;
                    }

                    if (addr != val + siz)
                    {
                        ldr.Errorf(s, "phase error: addr=%#x value+size=%#x", addr, val + siz);
                        errorexit();
                    }

                    if (val + siz >= eaddr)
                    {
                        break;
                    }

                }

                s = s__prev1;
            }

            if (addr < eaddr)
            {
                @out.WriteStringPad("", int(eaddr - addr), pad);
            }

        }

        public delegate void writeFn(ptr<Link>, ptr<OutBuf>, long, long);

        // WriteParallel handles scheduling parallel execution of data write functions.
        public static void WriteParallel(ptr<sync.WaitGroup> _addr_wg, writeFn fn, ptr<Link> _addr_ctxt, ulong seek, ulong vaddr, ulong length) => func((defer, _, __) =>
        {
            ref sync.WaitGroup wg = ref _addr_wg.val;
            ref Link ctxt = ref _addr_ctxt.val;

            {
                var (out, err) = ctxt.Out.View(seek);

                if (err != null)
                {
                    ctxt.Out.SeekSet(int64(seek));
                    fn(ctxt, ctxt.Out, int64(vaddr), int64(length));
                }
                else
                {
                    wg.Add(1L);
                    go_(() => () =>
                    {
                        defer(wg.Done());
                        fn(ctxt, out, int64(vaddr), int64(length));
                    }());

                }

            }

        });

        public static void Datblk(ptr<Link> _addr_ctxt, ptr<OutBuf> _addr_@out, long addr, long size)
        {
            ref Link ctxt = ref _addr_ctxt.val;
            ref OutBuf @out = ref _addr_@out.val;

            writeDatblkToOutBuf(_addr_ctxt, _addr_out, addr, size);
        }

        // Used only on Wasm for now.
        public static slice<byte> DatblkBytes(ptr<Link> _addr_ctxt, long addr, long size)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            var buf = make_slice<byte>(size);
            ptr<OutBuf> @out = addr(new OutBuf(heap:buf));
            writeDatblkToOutBuf(_addr_ctxt, out, addr, size);
            return buf;
        }

        private static void writeDatblkToOutBuf(ptr<Link> _addr_ctxt, ptr<OutBuf> _addr_@out, long addr, long size)
        {
            ref Link ctxt = ref _addr_ctxt.val;
            ref OutBuf @out = ref _addr_@out.val;

            writeBlocks(_addr_out, ctxt.outSem, _addr_ctxt.loader, ctxt.datap2, addr, size, zeros[..]);
        }

        public static void Dwarfblk(ptr<Link> _addr_ctxt, ptr<OutBuf> _addr_@out, long addr, long size)
        {
            ref Link ctxt = ref _addr_ctxt.val;
            ref OutBuf @out = ref _addr_@out.val;
 
            // Concatenate the section symbol lists into a single list to pass
            // to writeBlocks.
            //
            // NB: ideally we would do a separate writeBlocks call for each
            // section, but this would run the risk of undoing any file offset
            // adjustments made during layout.
            long n = 0L;
            {
                var i__prev1 = i;

                foreach (var (__i) in dwarfp2)
                {
                    i = __i;
                    n += len(dwarfp2[i].syms);
                }

                i = i__prev1;
            }

            var syms = make_slice<loader.Sym>(0L, n);
            {
                var i__prev1 = i;

                foreach (var (__i) in dwarfp2)
                {
                    i = __i;
                    syms = append(syms, dwarfp2[i].syms);
                }

                i = i__prev1;
            }

            writeBlocks(_addr_out, ctxt.outSem, _addr_ctxt.loader, syms, addr, size, zeros[..]);

        }

        private static array<byte> zeros = new array<byte>(512L);

        private static var strdata = make_map<@string, @string>();        private static slice<@string> strnames = default;

        private static void addstrdata1(ptr<Link> _addr_ctxt, @string arg)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            var eq = strings.Index(arg, "=");
            var dot = strings.LastIndex(arg[..eq + 1L], ".");
            if (eq < 0L || dot < 0L)
            {
                Exitf("-X flag requires argument of the form importpath.name=value");
            }

            var pkg = arg[..dot];
            if (ctxt.BuildMode == BuildModePlugin && pkg == "main")
            {
                pkg = flagPluginPath.val;
            }

            pkg = objabi.PathToPrefix(pkg);
            var name = pkg + arg[dot..eq];
            var value = arg[eq + 1L..];
            {
                var (_, ok) = strdata[name];

                if (!ok)
                {
                    strnames = append(strnames, name);
                }

            }

            strdata[name] = value;

        }

        // addstrdata sets the initial value of the string variable name to value.
        private static void addstrdata(ptr<sys.Arch> _addr_arch, ptr<loader.Loader> _addr_l, @string name, @string value)
        {
            ref sys.Arch arch = ref _addr_arch.val;
            ref loader.Loader l = ref _addr_l.val;

            var s = l.Lookup(name, 0L);
            if (s == 0L)
            {
                return ;
            }

            {
                var goType = l.SymGoType(s);

                if (goType == 0L)
                {
                    return ;
                }                {
                    var typeName = l.SymName(goType);


                    else if (typeName != "type.string")
                    {
                        Errorf(null, "%s: cannot set with -X: not a var of type string (%s)", name, typeName);
                        return ;
                    }

                }


            }

            if (!l.AttrReachable(s))
            {
                return ; // don't bother setting unreachable variable
            }

            var bld = l.MakeSymbolUpdater(s);
            if (bld.Type() == sym.SBSS)
            {
                bld.SetType(sym.SDATA);
            }

            var p = fmt.Sprintf("%s.str", name);
            var sp = l.LookupOrCreateSym(p, 0L);
            var sbld = l.MakeSymbolUpdater(sp);

            sbld.Addstring(value);
            sbld.SetType(sym.SRODATA);

            bld.SetSize(0L);
            bld.SetData(make_slice<byte>(0L, arch.PtrSize * 2L));
            bld.SetReadOnly(false);
            bld.SetRelocs(null);
            bld.AddAddrPlus(arch, sp, 0L);
            bld.AddUint(arch, uint64(len(value)));

        }

        private static void dostrdata(this ptr<Link> _addr_ctxt)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            foreach (var (_, name) in strnames)
            {
                addstrdata(_addr_ctxt.Arch, _addr_ctxt.loader, name, strdata[name]);
            }

        }

        // addgostring adds str, as a Go string value, to s. symname is the name of the
        // symbol used to define the string data and must be unique per linked object.
        private static void addgostring(ptr<Link> _addr_ctxt, ptr<loader.Loader> _addr_ldr, ptr<loader.SymbolBuilder> _addr_s, @string symname, @string str)
        {
            ref Link ctxt = ref _addr_ctxt.val;
            ref loader.Loader ldr = ref _addr_ldr.val;
            ref loader.SymbolBuilder s = ref _addr_s.val;

            var sdata = ldr.CreateSymForUpdate(symname, 0L);
            if (sdata.Type() != sym.Sxxx)
            {
                ctxt.Errorf(s.Sym(), "duplicate symname in addgostring: %s", symname);
            }

            sdata.SetReachable(true);
            sdata.SetLocal(true);
            sdata.SetType(sym.SRODATA);
            sdata.SetSize(int64(len(str)));
            sdata.SetData((slice<byte>)str);
            s.AddAddr(ctxt.Arch, sdata.Sym());
            s.AddUint(ctxt.Arch, uint64(len(str)));

        }

        private static void addinitarrdata(ptr<Link> _addr_ctxt, ptr<loader.Loader> _addr_ldr, loader.Sym s)
        {
            ref Link ctxt = ref _addr_ctxt.val;
            ref loader.Loader ldr = ref _addr_ldr.val;

            var p = ldr.SymName(s) + ".ptr";
            var sp = ldr.CreateSymForUpdate(p, 0L);
            sp.SetType(sym.SINITARR);
            sp.SetSize(0L);
            sp.SetDuplicateOK(true);
            sp.AddAddr(ctxt.Arch, s);
        }

        // symalign returns the required alignment for the given symbol s.
        private static int symalign2(this ptr<dodataState> _addr_state, loader.Sym s)
        {
            ref dodataState state = ref _addr_state.val;

            var min = int32(thearch.Minalign);
            var ldr = state.ctxt.loader;
            var align = ldr.SymAlign(s);
            if (align >= min)
            {
                return align;
            }
            else if (align != 0L)
            {
                return min;
            } 
            // FIXME: figure out a way to avoid checking by name here.
            var sname = ldr.SymName(s);
            if (strings.HasPrefix(sname, "go.string.") || strings.HasPrefix(sname, "type..namedata."))
            { 
                // String data is just bytes.
                // If we align it, we waste a lot of space to padding.
                return min;

            }

            align = int32(thearch.Maxalign);
            var ssz = ldr.SymSize(s);
            while (int64(align) > ssz && align > min)
            {
                align >>= 1L;
            }

            ldr.SetSymAlign(s, align);
            return align;

        }

        private static long aligndatsize2(ptr<dodataState> _addr_state, long datsize, loader.Sym s)
        {
            ref dodataState state = ref _addr_state.val;

            return Rnd(datsize, int64(state.symalign2(s)));
        }

        private static readonly var debugGCProg = false;



        public partial struct GCProg2
        {
            public ptr<Link> ctxt;
            public ptr<loader.SymbolBuilder> sym;
            public gcprog.Writer w;
        }

        private static void Init(this ptr<GCProg2> _addr_p, ptr<Link> _addr_ctxt, @string name)
        {
            ref GCProg2 p = ref _addr_p.val;
            ref Link ctxt = ref _addr_ctxt.val;

            p.ctxt = ctxt;
            var symIdx = ctxt.loader.LookupOrCreateSym(name, 0L);
            p.sym = ctxt.loader.MakeSymbolUpdater(symIdx);
            p.w.Init(p.writeByte());
            if (debugGCProg)
            {
                fmt.Fprintf(os.Stderr, "ld: start GCProg %s\n", name);
                p.w.Debug(os.Stderr);
            }

        }

        private static Action<byte> writeByte(this ptr<GCProg2> _addr_p)
        {
            ref GCProg2 p = ref _addr_p.val;

            return x =>
            {
                p.sym.AddUint8(x);
            };

        }

        private static void End(this ptr<GCProg2> _addr_p, long size)
        {
            ref GCProg2 p = ref _addr_p.val;

            p.w.ZeroUntil(size / int64(p.ctxt.Arch.PtrSize));
            p.w.End();
            if (debugGCProg)
            {
                fmt.Fprintf(os.Stderr, "ld: end GCProg\n");
            }

        }

        private static void AddSym(this ptr<GCProg2> _addr_p, loader.Sym s)
        {
            ref GCProg2 p = ref _addr_p.val;

            var ldr = p.ctxt.loader;
            var typ = ldr.SymGoType(s); 

            // Things without pointers should be in sym.SNOPTRDATA or sym.SNOPTRBSS;
            // everything we see should have pointers and should therefore have a type.
            if (typ == 0L)
            {
                switch (ldr.SymName(s))
                {
                    case "runtime.data": 
                        // Ignore special symbols that are sometimes laid out
                        // as real symbols. See comment about dyld on darwin in
                        // the address function.

                    case "runtime.edata": 
                        // Ignore special symbols that are sometimes laid out
                        // as real symbols. See comment about dyld on darwin in
                        // the address function.

                    case "runtime.bss": 
                        // Ignore special symbols that are sometimes laid out
                        // as real symbols. See comment about dyld on darwin in
                        // the address function.

                    case "runtime.ebss": 
                        // Ignore special symbols that are sometimes laid out
                        // as real symbols. See comment about dyld on darwin in
                        // the address function.
                        return ;
                        break;
                }
                p.ctxt.Errorf(p.sym.Sym(), "missing Go type information for global symbol %s: size %d", ldr.SymName(s), ldr.SymSize(s));
                return ;

            }

            var ptrsize = int64(p.ctxt.Arch.PtrSize);
            var typData = ldr.Data(typ);
            var nptr = decodetypePtrdata(p.ctxt.Arch, typData) / ptrsize;

            if (debugGCProg)
            {
                fmt.Fprintf(os.Stderr, "gcprog sym: %s at %d (ptr=%d+%d)\n", ldr.SymName(s), ldr.SymValue(s), ldr.SymValue(s) / ptrsize, nptr);
            }

            var sval = ldr.SymValue(s);
            if (decodetypeUsegcprog(p.ctxt.Arch, typData) == 0L)
            { 
                // Copy pointers from mask into program.
                var mask = decodetypeGcmask(p.ctxt, typ);
                for (var i = int64(0L); i < nptr; i++)
                {
                    if ((mask[i / 8L] >> (int)(uint(i % 8L))) & 1L != 0L)
                    {
                        p.w.Ptr(sval / ptrsize + i);
                    }

                }

                return ;

            } 

            // Copy program.
            var prog = decodetypeGcprog(p.ctxt, typ);
            p.w.ZeroUntil(sval / ptrsize);
            p.w.Append(prog[4L..], nptr);

        }

        // cutoff is the maximum data section size permitted by the linker
        // (see issue #9862).
        private static readonly float cutoff = (float)2e9F; // 2 GB (or so; looks better in errors than 2^31)

 // 2 GB (or so; looks better in errors than 2^31)

        private static void checkdatsize(this ptr<dodataState> _addr_state, sym.SymKind symn)
        {
            ref dodataState state = ref _addr_state.val;

            if (state.datsize > cutoff)
            {
                Errorf(null, "too much data in section %v (over %v bytes)", symn, cutoff);
            }

        }

        // fixZeroSizedSymbols gives a few special symbols with zero size some space.
        private static void fixZeroSizedSymbols2(ptr<Link> _addr_ctxt)
        {
            ref Link ctxt = ref _addr_ctxt.val;
 
            // The values in moduledata are filled out by relocations
            // pointing to the addresses of these special symbols.
            // Typically these symbols have no size and are not laid
            // out with their matching section.
            //
            // However on darwin, dyld will find the special symbol
            // in the first loaded module, even though it is local.
            //
            // (An hypothesis, formed without looking in the dyld sources:
            // these special symbols have no size, so their address
            // matches a real symbol. The dynamic linker assumes we
            // want the normal symbol with the same address and finds
            // it in the other module.)
            //
            // To work around this we lay out the symbls whose
            // addresses are vital for multi-module programs to work
            // as normal symbols, and give them a little size.
            //
            // On AIX, as all DATA sections are merged together, ld might not put
            // these symbols at the beginning of their respective section if there
            // aren't real symbols, their alignment might not match the
            // first symbol alignment. Therefore, there are explicitly put at the
            // beginning of their section with the same alignment.
            if (!(ctxt.DynlinkingGo() && ctxt.HeadType == objabi.Hdarwin) && !(ctxt.HeadType == objabi.Haix && ctxt.LinkMode == LinkExternal))
            {
                return ;
            }

            var ldr = ctxt.loader;
            var bss = ldr.CreateSymForUpdate("runtime.bss", 0L);
            bss.SetSize(8L);
            ldr.SetAttrSpecial(bss.Sym(), false);

            var ebss = ldr.CreateSymForUpdate("runtime.ebss", 0L);
            ldr.SetAttrSpecial(ebss.Sym(), false);

            var data = ldr.CreateSymForUpdate("runtime.data", 0L);
            data.SetSize(8L);
            ldr.SetAttrSpecial(data.Sym(), false);

            var edata = ldr.CreateSymForUpdate("runtime.edata", 0L);
            ldr.SetAttrSpecial(edata.Sym(), false);

            if (ctxt.HeadType == objabi.Haix)
            { 
                // XCOFFTOC symbols are part of .data section.
                edata.SetType(sym.SXCOFFTOC);

            }

            var types = ldr.CreateSymForUpdate("runtime.types", 0L);
            types.SetType(sym.STYPE);
            types.SetSize(8L);
            ldr.SetAttrSpecial(types.Sym(), false);

            var etypes = ldr.CreateSymForUpdate("runtime.etypes", 0L);
            etypes.SetType(sym.SFUNCTAB);
            ldr.SetAttrSpecial(etypes.Sym(), false);

            if (ctxt.HeadType == objabi.Haix)
            {
                var rodata = ldr.CreateSymForUpdate("runtime.rodata", 0L);
                rodata.SetType(sym.SSTRING);
                rodata.SetSize(8L);
                ldr.SetAttrSpecial(rodata.Sym(), false);

                var erodata = ldr.CreateSymForUpdate("runtime.erodata", 0L);
                ldr.SetAttrSpecial(erodata.Sym(), false);
            }

        }

        // makeRelroForSharedLib creates a section of readonly data if necessary.
        private static void makeRelroForSharedLib2(this ptr<dodataState> _addr_state, ptr<Link> _addr_target)
        {
            ref dodataState state = ref _addr_state.val;
            ref Link target = ref _addr_target.val;

            if (!target.UseRelro())
            {
                return ;
            } 

            // "read only" data with relocations needs to go in its own section
            // when building a shared library. We do this by boosting objects of
            // type SXXX with relocations to type SXXXRELRO.
            var ldr = target.loader;
            foreach (var (_, symnro) in sym.ReadOnly)
            {
                var symnrelro = sym.RelROMap[symnro];

                loader.Sym ro = new slice<loader.Sym>(new loader.Sym[] {  });
                var relro = state.data2[symnrelro];

                {
                    var s__prev2 = s;

                    foreach (var (_, __s) in state.data2[symnro])
                    {
                        s = __s;
                        var relocs = ldr.Relocs(s);
                        var isRelro = relocs.Count() > 0L;

                        if (state.symType(s) == sym.STYPE || state.symType(s) == sym.STYPERELRO || state.symType(s) == sym.SGOFUNCRELRO) 
                            // Symbols are not sorted yet, so it is possible
                            // that an Outer symbol has been changed to a
                            // relro Type before it reaches here.
                            isRelro = true;
                        else if (state.symType(s) == sym.SFUNCTAB) 
                            if (target.IsAIX() && ldr.SymName(s) == "runtime.etypes")
                            { 
                                // runtime.etypes must be at the end of
                                // the relro datas.
                                isRelro = true;

                            }

                                                if (isRelro)
                        {
                            state.setSymType(s, symnrelro);
                            {
                                var outer__prev2 = outer;

                                var outer = ldr.OuterSym(s);

                                if (outer != 0L)
                                {
                                    state.setSymType(outer, symnrelro);
                                }

                                outer = outer__prev2;

                            }

                            relro = append(relro, s);

                        }
                        else
                        {
                            ro = append(ro, s);
                        }

                    } 

                    // Check that we haven't made two symbols with the same .Outer into
                    // different types (because references two symbols with non-nil Outer
                    // become references to the outer symbol + offset it's vital that the
                    // symbol and the outer end up in the same section).

                    s = s__prev2;
                }

                {
                    var s__prev2 = s;

                    foreach (var (_, __s) in relro)
                    {
                        s = __s;
                        {
                            var outer__prev1 = outer;

                            outer = ldr.OuterSym(s);

                            if (outer != 0L)
                            {
                                var st = state.symType(s);
                                var ost = state.symType(outer);
                                if (st != ost)
                                {
                                    state.ctxt.Errorf(s, "inconsistent types for symbol and its Outer %s (%v != %v)", ldr.SymName(outer), st, ost);
                                }

                            }

                            outer = outer__prev1;

                        }

                    }

                    s = s__prev2;
                }

                state.data2[symnro] = ro;
                state.data2[symnrelro] = relro;

            }

        }

        // dodataState holds bits of state information needed by dodata() and the
        // various helpers it calls. The lifetime of these items should not extend
        // past the end of dodata().
        private partial struct dodataState
        {
            public ptr<Link> ctxt; // Data symbols bucketed by type.
            public array<slice<ptr<sym.Symbol>>> data; // Data symbols bucketed by type.
            public array<slice<loader.Sym>> data2; // Max alignment for each flavor of data symbol.
            public array<int> dataMaxAlign; // Overridden sym type
            public slice<sym.SymKind> symGroupType; // Current data size so far.
            public long datsize;
        }

        // A note on symType/setSymType below:
        //
        // In the legacy linker, the types of symbols (notably data symbols) are
        // changed during the symtab() phase so as to insure that similar symbols
        // are bucketed together, then their types are changed back again during
        // dodata. Symbol to section assignment also plays tricks along these lines
        // in the case where a relro segment is needed.
        //
        // The value returned from setType() below reflects the effects of
        // any overrides made by symtab and/or dodata.

        // symType returns the (possibly overridden) type of 's'.
        private static sym.SymKind symType(this ptr<dodataState> _addr_state, loader.Sym s)
        {
            ref dodataState state = ref _addr_state.val;

            if (int(s) < len(state.symGroupType))
            {
                {
                    var @override = state.symGroupType[s];

                    if (override != 0L)
                    {
                        return override;
                    }

                }

            }

            return state.ctxt.loader.SymType(s);

        }

        // setSymType sets a new override type for 's'.
        private static void setSymType(this ptr<dodataState> _addr_state, loader.Sym s, sym.SymKind kind) => func((_, panic, __) =>
        {
            ref dodataState state = ref _addr_state.val;

            if (s == 0L)
            {
                panic("bad");
            }

            if (int(s) < len(state.symGroupType))
            {
                state.symGroupType[s] = kind;
            }
            else
            {
                var su = state.ctxt.loader.MakeSymbolUpdater(s);
                su.SetType(kind);
            }

        });

        private static void dodata2(this ptr<Link> _addr_ctxt, slice<sym.SymKind> symGroupType)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            // Give zeros sized symbols space if necessary.
            fixZeroSizedSymbols2(_addr_ctxt); 

            // Collect data symbols by type into data.
            dodataState state = new dodataState(ctxt:ctxt,symGroupType:symGroupType);
            var ldr = ctxt.loader;
            {
                var s__prev1 = s;

                for (var s = loader.Sym(1L); s < loader.Sym(ldr.NSym()); s++)
                {
                    if (!ldr.AttrReachable(s) || ldr.AttrSpecial(s) || ldr.AttrSubSymbol(s) || !ldr.TopLevelSym(s))
                    {
                        continue;
                    }

                    var st = state.symType(s);

                    if (st <= sym.STEXT || st >= sym.SXREF)
                    {
                        continue;
                    }

                    state.data2[st] = append(state.data2[st], s); 

                    // Similarly with checking the onlist attr.
                    if (ldr.AttrOnList(s))
                    {
                        log.Fatalf("symbol %s listed multiple times", ldr.SymName(s));
                    }

                    ldr.SetAttrOnList(s, true);

                } 

                // Now that we have the data symbols, but before we start
                // to assign addresses, record all the necessary
                // dynamic relocations. These will grow the relocation
                // symbol, which is itself data.
                //
                // On darwin, we need the symbol table numbers for dynreloc.


                s = s__prev1;
            } 

            // Now that we have the data symbols, but before we start
            // to assign addresses, record all the necessary
            // dynamic relocations. These will grow the relocation
            // symbol, which is itself data.
            //
            // On darwin, we need the symbol table numbers for dynreloc.
            if (ctxt.HeadType == objabi.Hdarwin)
            {
                machosymorder(ctxt);
            }

            state.dynreloc2(ctxt); 

            // Move any RO data with relocations to a separate section.
            state.makeRelroForSharedLib2(ctxt); 

            // Set alignment for the symbol with the largest known index,
            // so as to trigger allocation of the loader's internal
            // alignment array. This will avoid data races in the parallel
            // section below.
            var lastSym = loader.Sym(ldr.NSym() - 1L);
            ldr.SetSymAlign(lastSym, ldr.SymAlign(lastSym)); 

            // Sort symbols.
            sync.WaitGroup wg = default;
            {
                var symn__prev1 = symn;

                foreach (var (__symn) in state.data2)
                {
                    symn = __symn;
                    var symn = sym.SymKind(symn);
                    wg.Add(1L);
                    go_(() => () =>
                    {
                        state.data2[symn], state.dataMaxAlign[symn] = state.dodataSect2(ctxt, symn, state.data2[symn]);
                        wg.Done();
                    }());

                }

                symn = symn__prev1;
            }

            wg.Wait();

            if (ctxt.IsELF)
            { 
                // Make .rela and .rela.plt contiguous, the ELF ABI requires this
                // and Solaris actually cares.
                var syms = state.data2[sym.SELFROSECT];
                long reli = -1L;
                long plti = -1L;
                {
                    var s__prev1 = s;

                    foreach (var (__i, __s) in syms)
                    {
                        i = __i;
                        s = __s;
                        switch (ldr.SymName(s))
                        {
                            case ".rel.plt": 

                            case ".rela.plt": 
                                plti = i;
                                break;
                            case ".rel": 

                            case ".rela": 
                                reli = i;
                                break;
                        }

                    }

                    s = s__prev1;
                }

                if (reli >= 0L && plti >= 0L && plti != reli + 1L)
                {
                    long first = default;                    long second = default;

                    if (plti > reli)
                    {
                        first = reli;
                        second = plti;

                    }
                    else
                    {
                        first = plti;
                        second = reli;

                    }

                    var rel = syms[reli];
                    var plt = syms[plti];
                    copy(syms[first + 2L..], syms[first + 1L..second]);
                    syms[first + 0L] = rel;
                    syms[first + 1L] = plt; 

                    // Make sure alignment doesn't introduce a gap.
                    // Setting the alignment explicitly prevents
                    // symalign from basing it on the size and
                    // getting it wrong.
                    ldr.SetSymAlign(rel, int32(ctxt.Arch.RegSize));
                    ldr.SetSymAlign(plt, int32(ctxt.Arch.RegSize));

                }

                state.data2[sym.SELFROSECT] = syms;

            }

            if (ctxt.HeadType == objabi.Haix && ctxt.LinkMode == LinkExternal)
            { 
                // These symbols must have the same alignment as their section.
                // Otherwize, ld might change the layout of Go sections.
                ldr.SetSymAlign(ldr.Lookup("runtime.data", 0L), state.dataMaxAlign[sym.SDATA]);
                ldr.SetSymAlign(ldr.Lookup("runtime.bss", 0L), state.dataMaxAlign[sym.SBSS]);

            } 

            // Create *sym.Section objects and assign symbols to sections for
            // data/rodata (and related) symbols.
            state.allocateDataSections2(ctxt); 

            // Create *sym.Section objects and assign symbols to sections for
            // DWARF symbols.
            state.allocateDwarfSections2(ctxt); 

            /* number the sections */
            var n = int16(1L);

            {
                var sect__prev1 = sect;

                foreach (var (_, __sect) in Segtext.Sections)
                {
                    sect = __sect;
                    sect.Extnum = n;
                    n++;
                }

                sect = sect__prev1;
            }

            {
                var sect__prev1 = sect;

                foreach (var (_, __sect) in Segrodata.Sections)
                {
                    sect = __sect;
                    sect.Extnum = n;
                    n++;
                }

                sect = sect__prev1;
            }

            {
                var sect__prev1 = sect;

                foreach (var (_, __sect) in Segrelrodata.Sections)
                {
                    sect = __sect;
                    sect.Extnum = n;
                    n++;
                }

                sect = sect__prev1;
            }

            {
                var sect__prev1 = sect;

                foreach (var (_, __sect) in Segdata.Sections)
                {
                    sect = __sect;
                    sect.Extnum = n;
                    n++;
                }

                sect = sect__prev1;
            }

            {
                var sect__prev1 = sect;

                foreach (var (_, __sect) in Segdwarf.Sections)
                {
                    sect = __sect;
                    sect.Extnum = n;
                    n++;
                }

                sect = sect__prev1;
            }
        }

        // allocateDataSectionForSym creates a new sym.Section into which a a
        // single symbol will be placed. Here "seg" is the segment into which
        // the section will go, "s" is the symbol to be placed into the new
        // section, and "rwx" contains permissions for the section.
        private static ptr<sym.Section> allocateDataSectionForSym2(this ptr<dodataState> _addr_state, ptr<sym.Segment> _addr_seg, loader.Sym s, long rwx)
        {
            ref dodataState state = ref _addr_state.val;
            ref sym.Segment seg = ref _addr_seg.val;

            var ldr = state.ctxt.loader;
            var sname = ldr.SymName(s);
            var sect = addsection(ldr, state.ctxt.Arch, seg, sname, rwx);
            sect.Align = state.symalign2(s);
            state.datsize = Rnd(state.datsize, int64(sect.Align));
            sect.Vaddr = uint64(state.datsize);
            return _addr_sect!;
        }

        // allocateNamedDataSection creates a new sym.Section for a category
        // of data symbols. Here "seg" is the segment into which the section
        // will go, "sName" is the name to give to the section, "types" is a
        // range of symbol types to be put into the section, and "rwx"
        // contains permissions for the section.
        private static ptr<sym.Section> allocateNamedDataSection(this ptr<dodataState> _addr_state, ptr<sym.Segment> _addr_seg, @string sName, slice<sym.SymKind> types, long rwx)
        {
            ref dodataState state = ref _addr_state.val;
            ref sym.Segment seg = ref _addr_seg.val;

            var sect = addsection(state.ctxt.loader, state.ctxt.Arch, seg, sName, rwx);
            if (len(types) == 0L)
            {
                sect.Align = 1L;
            }
            else if (len(types) == 1L)
            {
                sect.Align = state.dataMaxAlign[types[0L]];
            }
            else
            {
                foreach (var (_, symn) in types)
                {
                    var align = state.dataMaxAlign[symn];
                    if (sect.Align < align)
                    {
                        sect.Align = align;
                    }

                }

            }

            state.datsize = Rnd(state.datsize, int64(sect.Align));
            sect.Vaddr = uint64(state.datsize);
            return _addr_sect!;

        }

        // assignDsymsToSection assigns a collection of data symbols to a
        // newly created section. "sect" is the section into which to place
        // the symbols, "syms" holds the list of symbols to assign,
        // "forceType" (if non-zero) contains a new sym type to apply to each
        // sym during the assignment, and "aligner" is a hook to call to
        // handle alignment during the assignment process.
        private static long assignDsymsToSection2(this ptr<dodataState> _addr_state, ptr<sym.Section> _addr_sect, slice<loader.Sym> syms, sym.SymKind forceType, Func<ptr<dodataState>, long, loader.Sym, long> aligner)
        {
            ref dodataState state = ref _addr_state.val;
            ref sym.Section sect = ref _addr_sect.val;

            var ldr = state.ctxt.loader;
            foreach (var (_, s) in syms)
            {
                state.datsize = aligner(state, state.datsize, s);
                ldr.SetSymSect(s, sect);
                if (forceType != sym.Sxxx)
                {
                    state.setSymType(s, forceType);
                }

                ldr.SetSymValue(s, int64(uint64(state.datsize) - sect.Vaddr));
                state.datsize += ldr.SymSize(s);

            }
            sect.Length = uint64(state.datsize) - sect.Vaddr;

        }

        private static void assignToSection2(this ptr<dodataState> _addr_state, ptr<sym.Section> _addr_sect, sym.SymKind symn, sym.SymKind forceType)
        {
            ref dodataState state = ref _addr_state.val;
            ref sym.Section sect = ref _addr_sect.val;

            state.assignDsymsToSection2(sect, state.data2[symn], forceType, aligndatsize2);
            state.checkdatsize(symn);
        }

        // allocateSingleSymSections walks through the bucketed data symbols
        // with type 'symn', creates a new section for each sym, and assigns
        // the sym to a newly created section. Section name is set from the
        // symbol name. "Seg" is the segment into which to place the new
        // section, "forceType" is the new sym.SymKind to assign to the symbol
        // within the section, and "rwx" holds section permissions.
        private static void allocateSingleSymSections2(this ptr<dodataState> _addr_state, ptr<sym.Segment> _addr_seg, sym.SymKind symn, sym.SymKind forceType, long rwx)
        {
            ref dodataState state = ref _addr_state.val;
            ref sym.Segment seg = ref _addr_seg.val;

            var ldr = state.ctxt.loader;
            foreach (var (_, s) in state.data2[symn])
            {
                var sect = state.allocateDataSectionForSym2(seg, s, rwx);
                ldr.SetSymSect(s, sect);
                state.setSymType(s, forceType);
                ldr.SetSymValue(s, int64(uint64(state.datsize) - sect.Vaddr));
                state.datsize += ldr.SymSize(s);
                sect.Length = uint64(state.datsize) - sect.Vaddr;
            }
            state.checkdatsize(symn);

        }

        // allocateNamedSectionAndAssignSyms creates a new section with the
        // specified name, then walks through the bucketed data symbols with
        // type 'symn' and assigns each of them to this new section. "Seg" is
        // the segment into which to place the new section, "secName" is the
        // name to give to the new section, "forceType" (if non-zero) contains
        // a new sym type to apply to each sym during the assignment, and
        // "rwx" holds section permissions.
        private static ptr<sym.Section> allocateNamedSectionAndAssignSyms2(this ptr<dodataState> _addr_state, ptr<sym.Segment> _addr_seg, @string secName, sym.SymKind symn, sym.SymKind forceType, long rwx)
        {
            ref dodataState state = ref _addr_state.val;
            ref sym.Segment seg = ref _addr_seg.val;

            var sect = state.allocateNamedDataSection(seg, secName, new slice<sym.SymKind>(new sym.SymKind[] { symn }), rwx);
            state.assignDsymsToSection2(sect, state.data2[symn], forceType, aligndatsize2);
            return _addr_sect!;
        }

        // allocateDataSections allocates sym.Section objects for data/rodata
        // (and related) symbols, and then assigns symbols to those sections.
        private static void allocateDataSections2(this ptr<dodataState> _addr_state, ptr<Link> _addr_ctxt)
        {
            ref dodataState state = ref _addr_state.val;
            ref Link ctxt = ref _addr_ctxt.val;
 
            // Allocate sections.
            // Data is processed before segtext, because we need
            // to see all symbols in the .data and .bss sections in order
            // to generate garbage collection information.

            // Writable data sections that do not need any specialized handling.
            sym.SymKind writable = new slice<sym.SymKind>(new sym.SymKind[] { sym.SBUILDINFO, sym.SELFSECT, sym.SMACHO, sym.SMACHOGOT, sym.SWINDOWS });
            {
                var symn__prev1 = symn;

                foreach (var (_, __symn) in writable)
                {
                    symn = __symn;
                    state.allocateSingleSymSections2(_addr_Segdata, symn, sym.SDATA, 06L);
                }

                symn = symn__prev1;
            }

            var ldr = ctxt.loader; 

            // .got (and .toc on ppc64)
            if (len(state.data2[sym.SELFGOT]) > 0L)
            {
                var sect = state.allocateNamedSectionAndAssignSyms2(_addr_Segdata, ".got", sym.SELFGOT, sym.SDATA, 06L);
                if (ctxt.IsPPC64())
                {
                    {
                        var s__prev1 = s;

                        foreach (var (_, __s) in state.data2[sym.SELFGOT])
                        {
                            s = __s; 
                            // Resolve .TOC. symbol for this object file (ppc64)

                            var toc = ldr.Lookup(".TOC.", int(ldr.SymVersion(s)));
                            if (toc != 0L)
                            {
                                ldr.SetSymSect(toc, sect);
                                ldr.PrependSub(s, toc);
                                ldr.SetSymValue(toc, 0x8000UL);
                            }

                        }

                        s = s__prev1;
                    }
                }

            } 

            /* pointer-free data */
            sect = state.allocateNamedSectionAndAssignSyms2(_addr_Segdata, ".noptrdata", sym.SNOPTRDATA, sym.SDATA, 06L);
            ldr.SetSymSect(ldr.LookupOrCreateSym("runtime.noptrdata", 0L), sect);
            ldr.SetSymSect(ldr.LookupOrCreateSym("runtime.enoptrdata", 0L), sect);

            var hasinitarr = ctxt.linkShared; 

            /* shared library initializer */

            if (ctxt.BuildMode == BuildModeCArchive || ctxt.BuildMode == BuildModeCShared || ctxt.BuildMode == BuildModeShared || ctxt.BuildMode == BuildModePlugin) 
                hasinitarr = true;
                        if (ctxt.HeadType == objabi.Haix)
            {
                if (len(state.data2[sym.SINITARR]) > 0L)
                {
                    Errorf(null, "XCOFF format doesn't allow .init_array section");
                }

            }

            if (hasinitarr && len(state.data2[sym.SINITARR]) > 0L)
            {
                state.allocateNamedSectionAndAssignSyms2(_addr_Segdata, ".init_array", sym.SINITARR, sym.Sxxx, 06L);
            } 

            /* data */
            sect = state.allocateNamedSectionAndAssignSyms2(_addr_Segdata, ".data", sym.SDATA, sym.SDATA, 06L);
            ldr.SetSymSect(ldr.LookupOrCreateSym("runtime.data", 0L), sect);
            ldr.SetSymSect(ldr.LookupOrCreateSym("runtime.edata", 0L), sect);
            var dataGcEnd = state.datsize - int64(sect.Vaddr); 

            // On AIX, TOC entries must be the last of .data
            // These aren't part of gc as they won't change during the runtime.
            state.assignToSection2(sect, sym.SXCOFFTOC, sym.SDATA);
            state.checkdatsize(sym.SDATA);
            sect.Length = uint64(state.datsize) - sect.Vaddr; 

            /* bss */
            sect = state.allocateNamedSectionAndAssignSyms2(_addr_Segdata, ".bss", sym.SBSS, sym.Sxxx, 06L);
            ldr.SetSymSect(ldr.LookupOrCreateSym("runtime.bss", 0L), sect);
            ldr.SetSymSect(ldr.LookupOrCreateSym("runtime.ebss", 0L), sect);
            var bssGcEnd = state.datsize - int64(sect.Vaddr); 

            // Emit gcdata for bcc symbols now that symbol values have been assigned.
            foreach (var (_, g) in gcsToEmit)
            {
                GCProg2 gc = default;
                gc.Init(ctxt, g.symName);
                {
                    var s__prev2 = s;

                    foreach (var (_, __s) in state.data2[g.symKind])
                    {
                        s = __s;
                        gc.AddSym(s);
                    }

                    s = s__prev2;
                }

                gc.End(g.gcEnd);

            } 

            /* pointer-free bss */
            sect = state.allocateNamedSectionAndAssignSyms2(_addr_Segdata, ".noptrbss", sym.SNOPTRBSS, sym.Sxxx, 06L);
            ldr.SetSymSect(ldr.LookupOrCreateSym("runtime.noptrbss", 0L), sect);
            ldr.SetSymSect(ldr.LookupOrCreateSym("runtime.enoptrbss", 0L), sect);
            ldr.SetSymSect(ldr.LookupOrCreateSym("runtime.end", 0L), sect); 

            // Coverage instrumentation counters for libfuzzer.
            if (len(state.data2[sym.SLIBFUZZER_EXTRA_COUNTER]) > 0L)
            {
                state.allocateNamedSectionAndAssignSyms2(_addr_Segdata, "__libfuzzer_extra_counters", sym.SLIBFUZZER_EXTRA_COUNTER, sym.Sxxx, 06L);
            }

            if (len(state.data2[sym.STLSBSS]) > 0L)
            {
                sect = ; 
                // FIXME: not clear why it is sometimes necessary to suppress .tbss section creation.
                if ((ctxt.IsELF || ctxt.HeadType == objabi.Haix) && (ctxt.LinkMode == LinkExternal || !FlagD.val))
                {
                    sect = addsection(ldr, ctxt.Arch, _addr_Segdata, ".tbss", 06L);
                    sect.Align = int32(ctxt.Arch.PtrSize); 
                    // FIXME: why does this need to be set to zero?
                    sect.Vaddr = 0L;

                }

                state.datsize = 0L;

                {
                    var s__prev1 = s;

                    foreach (var (_, __s) in state.data2[sym.STLSBSS])
                    {
                        s = __s;
                        state.datsize = aligndatsize2(_addr_state, state.datsize, s);
                        if (sect != null)
                        {
                            ldr.SetSymSect(s, sect);
                        }

                        ldr.SetSymValue(s, state.datsize);
                        state.datsize += ldr.SymSize(s);

                    }

                    s = s__prev1;
                }

                state.checkdatsize(sym.STLSBSS);

                if (sect != null)
                {
                    sect.Length = uint64(state.datsize);
                }

            }

            /*
                 * We finished data, begin read-only data.
                 * Not all systems support a separate read-only non-executable data section.
                 * ELF and Windows PE systems do.
                 * OS X and Plan 9 do not.
                 * And if we're using external linking mode, the point is moot,
                 * since it's not our decision; that code expects the sections in
                 * segtext.
                 */
            ptr<sym.Segment> segro;
            if (ctxt.IsELF && ctxt.LinkMode == LinkInternal)
            {
                segro = _addr_Segrodata;
            }
            else if (ctxt.HeadType == objabi.Hwindows)
            {
                segro = _addr_Segrodata;
            }
            else
            {
                segro = _addr_Segtext;
            }

            state.datsize = 0L; 

            /* read-only executable ELF, Mach-O sections */
            if (len(state.data2[sym.STEXT]) != 0L)
            {
                var culprit = ldr.SymName(state.data2[sym.STEXT][0L]);
                Errorf(null, "dodata found an sym.STEXT symbol: %s", culprit);
            }

            state.allocateSingleSymSections2(_addr_Segtext, sym.SELFRXSECT, sym.SRODATA, 04L); 

            /* read-only data */
            sect = state.allocateNamedDataSection(segro, ".rodata", sym.ReadOnly, 04L);
            ldr.SetSymSect(ldr.LookupOrCreateSym("runtime.rodata", 0L), sect);
            ldr.SetSymSect(ldr.LookupOrCreateSym("runtime.erodata", 0L), sect);
            if (!ctxt.UseRelro())
            {
                ldr.SetSymSect(ldr.LookupOrCreateSym("runtime.types", 0L), sect);
                ldr.SetSymSect(ldr.LookupOrCreateSym("runtime.etypes", 0L), sect);
            }

            {
                var symn__prev1 = symn;

                foreach (var (_, __symn) in sym.ReadOnly)
                {
                    symn = __symn;
                    var symnStartValue = state.datsize;
                    state.assignToSection2(sect, symn, sym.SRODATA);
                    if (ctxt.HeadType == objabi.Haix)
                    { 
                        // Read-only symbols might be wrapped inside their outer
                        // symbol.
                        // XCOFF symbol table needs to know the size of
                        // these outer symbols.
                        xcoffUpdateOuterSize2(ctxt, state.datsize - symnStartValue, symn);

                    }

                } 

                /* read-only ELF, Mach-O sections */

                symn = symn__prev1;
            }

            state.allocateSingleSymSections2(segro, sym.SELFROSECT, sym.SRODATA, 04L);
            state.allocateSingleSymSections2(segro, sym.SMACHOPLT, sym.SRODATA, 04L); 

            // There is some data that are conceptually read-only but are written to by
            // relocations. On GNU systems, we can arrange for the dynamic linker to
            // mprotect sections after relocations are applied by giving them write
            // permissions in the object file and calling them ".data.rel.ro.FOO". We
            // divide the .rodata section between actual .rodata and .data.rel.ro.rodata,
            // but for the other sections that this applies to, we just write a read-only
            // .FOO section or a read-write .data.rel.ro.FOO section depending on the
            // situation.
            // TODO(mwhudson): It would make sense to do this more widely, but it makes
            // the system linker segfault on darwin.
            const long relroPerm = (long)06L;

            const long fallbackPerm = (long)04L;

            var relroSecPerm = fallbackPerm;
            Func<@string, @string> genrelrosecname = suffix =>
            {
                return suffix;
            }
;
            var seg = segro;

            if (ctxt.UseRelro())
            {
                var segrelro = _addr_Segrelrodata;
                if (ctxt.LinkMode == LinkExternal && ctxt.HeadType != objabi.Haix)
                { 
                    // Using a separate segment with an external
                    // linker results in some programs moving
                    // their data sections unexpectedly, which
                    // corrupts the moduledata. So we use the
                    // rodata segment and let the external linker
                    // sort out a rel.ro segment.
                    segrelro = segro;

                }
                else
                { 
                    // Reset datsize for new segment.
                    state.datsize = 0L;

                }

                genrelrosecname = suffix =>
                {
                    return ".data.rel.ro" + suffix;
                }
;
                sym.SymKind relroReadOnly = new slice<sym.SymKind>(new sym.SymKind[] {  });
                {
                    var symnro__prev1 = symnro;

                    foreach (var (_, __symnro) in sym.ReadOnly)
                    {
                        symnro = __symnro;
                        var symn = sym.RelROMap[symnro];
                        relroReadOnly = append(relroReadOnly, symn);
                    }

                    symnro = symnro__prev1;
                }

                seg = segrelro;
                relroSecPerm = relroPerm; 

                /* data only written by relocations */
                sect = state.allocateNamedDataSection(segrelro, genrelrosecname(""), relroReadOnly, relroSecPerm);

                ldr.SetSymSect(ldr.LookupOrCreateSym("runtime.types", 0L), sect);
                ldr.SetSymSect(ldr.LookupOrCreateSym("runtime.etypes", 0L), sect);

                {
                    var symnro__prev1 = symnro;

                    foreach (var (__i, __symnro) in sym.ReadOnly)
                    {
                        i = __i;
                        symnro = __symnro;
                        if (i == 0L && symnro == sym.STYPE && ctxt.HeadType != objabi.Haix)
                        { 
                            // Skip forward so that no type
                            // reference uses a zero offset.
                            // This is unlikely but possible in small
                            // programs with no other read-only data.
                            state.datsize++;

                        }

                        symn = sym.RelROMap[symnro];
                        symnStartValue = state.datsize;

                        {
                            var s__prev2 = s;

                            foreach (var (_, __s) in state.data2[symn])
                            {
                                s = __s;
                                var outer = ldr.OuterSym(s);
                                if (s != 0L && ldr.SymSect(outer) != null && ldr.SymSect(outer) != sect)
                                {
                                    ctxt.Errorf(s, "s.Outer (%s) in different section from s, %s != %s", ldr.SymName(outer), ldr.SymSect(outer).Name, sect.Name);
                                }

                            }

                            s = s__prev2;
                        }

                        state.assignToSection2(sect, symn, sym.SRODATA);
                        if (ctxt.HeadType == objabi.Haix)
                        { 
                            // Read-only symbols might be wrapped inside their outer
                            // symbol.
                            // XCOFF symbol table needs to know the size of
                            // these outer symbols.
                            xcoffUpdateOuterSize2(ctxt, state.datsize - symnStartValue, symn);

                        }

                    }

                    symnro = symnro__prev1;
                }

                sect.Length = uint64(state.datsize) - sect.Vaddr;

            } 

            /* typelink */
            sect = state.allocateNamedDataSection(seg, genrelrosecname(".typelink"), new slice<sym.SymKind>(new sym.SymKind[] { sym.STYPELINK }), relroSecPerm);

            var typelink = ldr.CreateSymForUpdate("runtime.typelink", 0L);
            ldr.SetSymSect(typelink.Sym(), sect);
            typelink.SetType(sym.SRODATA);
            state.datsize += typelink.Size();
            state.checkdatsize(sym.STYPELINK);
            sect.Length = uint64(state.datsize) - sect.Vaddr; 

            /* itablink */
            sect = state.allocateNamedSectionAndAssignSyms2(seg, genrelrosecname(".itablink"), sym.SITABLINK, sym.Sxxx, relroSecPerm);
            ldr.SetSymSect(ldr.LookupOrCreateSym("runtime.itablink", 0L), sect);
            ldr.SetSymSect(ldr.LookupOrCreateSym("runtime.eitablink", 0L), sect);
            if (ctxt.HeadType == objabi.Haix)
            { 
                // Store .itablink size because its symbols are wrapped
                // under an outer symbol: runtime.itablink.
                xcoffUpdateOuterSize2(ctxt, int64(sect.Length), sym.SITABLINK);

            } 

            /* gosymtab */
            sect = state.allocateNamedSectionAndAssignSyms2(seg, genrelrosecname(".gosymtab"), sym.SSYMTAB, sym.SRODATA, relroSecPerm);
            ldr.SetSymSect(ldr.LookupOrCreateSym("runtime.symtab", 0L), sect);
            ldr.SetSymSect(ldr.LookupOrCreateSym("runtime.esymtab", 0L), sect); 

            /* gopclntab */
            sect = state.allocateNamedSectionAndAssignSyms2(seg, genrelrosecname(".gopclntab"), sym.SPCLNTAB, sym.SRODATA, relroSecPerm);
            ldr.SetSymSect(ldr.LookupOrCreateSym("runtime.pclntab", 0L), sect);
            ldr.SetSymSect(ldr.LookupOrCreateSym("runtime.epclntab", 0L), sect); 

            // 6g uses 4-byte relocation offsets, so the entire segment must fit in 32 bits.
            if (state.datsize != int64(uint32(state.datsize)))
            {
                Errorf(null, "read-only data segment too large: %d", state.datsize);
            }

            long siz = 0L;
            {
                var symn__prev1 = symn;

                for (symn = sym.SELFRXSECT; symn < sym.SXREF; symn++)
                {
                    siz += len(state.data2[symn]);
                }


                symn = symn__prev1;
            }
            ctxt.datap2 = make_slice<loader.Sym>(0L, siz);
            {
                var symn__prev1 = symn;

                for (symn = sym.SELFRXSECT; symn < sym.SXREF; symn++)
                {
                    ctxt.datap2 = append(ctxt.datap2, state.data2[symn]);
                }


                symn = symn__prev1;
            }

        }

        // allocateDwarfSections allocates sym.Section objects for DWARF
        // symbols, and assigns symbols to sections.
        private static void allocateDwarfSections2(this ptr<dodataState> _addr_state, ptr<Link> _addr_ctxt)
        {
            ref dodataState state = ref _addr_state.val;
            ref Link ctxt = ref _addr_ctxt.val;

            Func<ptr<dodataState>, long, loader.Sym, long> alignOne = (state, datsize, s) => datsize;

            var ldr = ctxt.loader;
            for (long i = 0L; i < len(dwarfp2); i++)
            { 
                // First the section symbol.
                var s = dwarfp2[i].secSym();
                var sect = state.allocateNamedDataSection(_addr_Segdwarf, ldr.SymName(s), new slice<sym.SymKind>(new sym.SymKind[] {  }), 04L);
                ldr.SetSymSect(s, sect);
                sect.Sym2 = sym.LoaderSym(s);
                var curType = ldr.SymType(s);
                state.setSymType(s, sym.SRODATA);
                ldr.SetSymValue(s, int64(uint64(state.datsize) - sect.Vaddr));
                state.datsize += ldr.SymSize(s); 

                // Then any sub-symbols for the section symbol.
                var subSyms = dwarfp2[i].subSyms();
                state.assignDsymsToSection2(sect, subSyms, sym.SRODATA, alignOne);

                for (long j = 0L; j < len(subSyms); j++)
                {
                    s = subSyms[j];
                    if (ctxt.HeadType == objabi.Haix && curType == sym.SDWARFLOC)
                    { 
                        // Update the size of .debug_loc for this symbol's
                        // package.
                        addDwsectCUSize(".debug_loc", ldr.SymPkg(s), uint64(ldr.SymSize(s)));

                    }

                }

                sect.Length = uint64(state.datsize) - sect.Vaddr;
                state.checkdatsize(curType);

            }


        }

        private partial struct symNameSize
        {
            public @string name;
            public long sz;
            public loader.Sym sym;
        }

        private static (slice<loader.Sym>, int) dodataSect2(this ptr<dodataState> _addr_state, ptr<Link> _addr_ctxt, sym.SymKind symn, slice<loader.Sym> syms)
        {
            slice<loader.Sym> result = default;
            int maxAlign = default;
            ref dodataState state = ref _addr_state.val;
            ref Link ctxt = ref _addr_ctxt.val;

            loader.Sym head = default;            loader.Sym tail = default;

            var ldr = ctxt.loader;
            var sl = make_slice<symNameSize>(len(syms));
            {
                var k__prev1 = k;
                var s__prev1 = s;

                foreach (var (__k, __s) in syms)
                {
                    k = __k;
                    s = __s;
                    var ss = ldr.SymSize(s);
                    sl[k] = new symNameSize(name:ldr.SymName(s),sz:ss,sym:s);
                    var ds = int64(len(ldr.Data(s)));

                    if (ss < ds) 
                        ctxt.Errorf(s, "initialize bounds (%d < %d)", ss, ds);
                    else if (ss < 0L) 
                        ctxt.Errorf(s, "negative size (%d bytes)", ss);
                    else if (ss > cutoff) 
                        ctxt.Errorf(s, "symbol too large (%d bytes)", ss);
                    // If the usually-special section-marker symbols are being laid
                    // out as regular symbols, put them either at the beginning or
                    // end of their section.
                    if ((ctxt.DynlinkingGo() && ctxt.HeadType == objabi.Hdarwin) || (ctxt.HeadType == objabi.Haix && ctxt.LinkMode == LinkExternal))
                    {
                        switch (ldr.SymName(s))
                        {
                            case "runtime.text": 

                            case "runtime.bss": 

                            case "runtime.data": 

                            case "runtime.types": 

                            case "runtime.rodata": 
                                head = s;
                                continue;
                                break;
                            case "runtime.etext": 

                            case "runtime.ebss": 

                            case "runtime.edata": 

                            case "runtime.etypes": 

                            case "runtime.erodata": 
                                tail = s;
                                continue;
                                break;
                        }

                    }

                } 

                // For ppc64, we want to interleave the .got and .toc sections
                // from input files. Both are type sym.SELFGOT, so in that case
                // we skip size comparison and fall through to the name
                // comparison (conveniently, .got sorts before .toc).

                k = k__prev1;
                s = s__prev1;
            }

            var checkSize = symn != sym.SELFGOT; 

            // Perform the sort.
            sort.Slice(sl, (i, j) =>
            {
                var si = sl[i].sym;
                var sj = sl[j].sym;

                if (si == head || sj == tail) 
                    return true;
                else if (sj == head || si == tail) 
                    return false;
                                if (checkSize)
                {
                    var isz = sl[i].sz;
                    var jsz = sl[j].sz;
                    if (isz != jsz)
                    {
                        return isz < jsz;
                    }

                }

                var iname = sl[i].name;
                var jname = sl[j].name;
                if (iname != jname)
                {
                    return iname < jname;
                }

                return si < sj;

            }); 

            // Set alignment, construct result
            syms = syms[..0L];
            {
                var k__prev1 = k;

                foreach (var (__k) in sl)
                {
                    k = __k;
                    var s = sl[k].sym;
                    if (s != head && s != tail)
                    {
                        var align = state.symalign2(s);
                        if (maxAlign < align)
                        {
                            maxAlign = align;
                        }

                    }

                    syms = append(syms, s);

                }

                k = k__prev1;
            }

            return (syms, maxAlign);

        }

        // Add buildid to beginning of text segment, on non-ELF systems.
        // Non-ELF binary formats are not always flexible enough to
        // give us a place to put the Go build ID. On those systems, we put it
        // at the very beginning of the text segment.
        // This ``header'' is read by cmd/go.
        private static void textbuildid(this ptr<Link> _addr_ctxt)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            if (ctxt.IsELF || ctxt.BuildMode == BuildModePlugin || flagBuildid == "".val)
            {
                return ;
            }

            var ldr = ctxt.loader;
            var s = ldr.CreateSymForUpdate("go.buildid", 0L);
            s.SetReachable(true); 
            // The \xff is invalid UTF-8, meant to make it less likely
            // to find one of these accidentally.
            @string data = "\xff Go build ID: " + strconv.Quote(flagBuildid.val) + "\n \xff";
            s.SetType(sym.STEXT);
            s.SetData((slice<byte>)data);
            s.SetSize(int64(len(data)));

            ctxt.Textp2 = append(ctxt.Textp2, 0L);
            copy(ctxt.Textp2[1L..], ctxt.Textp2);
            ctxt.Textp2[0L] = s.Sym();

        }

        private static void buildinfo(this ptr<Link> _addr_ctxt)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            if (ctxt.linkShared || ctxt.BuildMode == BuildModePlugin)
            { 
                // -linkshared and -buildmode=plugin get confused
                // about the relocations in go.buildinfo
                // pointing at the other data sections.
                // The version information is only available in executables.
                return ;

            }

            var ldr = ctxt.loader;
            var s = ldr.CreateSymForUpdate(".go.buildinfo", 0L);
            s.SetReachable(true);
            s.SetType(sym.SBUILDINFO);
            s.SetAlign(16L); 
            // The \xff is invalid UTF-8, meant to make it less likely
            // to find one of these accidentally.
            const @string prefix = (@string)"\xff Go buildinf:"; // 14 bytes, plus 2 data bytes filled in below
 // 14 bytes, plus 2 data bytes filled in below
            var data = make_slice<byte>(32L);
            copy(data, prefix);
            data[len(prefix)] = byte(ctxt.Arch.PtrSize);
            data[len(prefix) + 1L] = 0L;
            if (ctxt.Arch.ByteOrder == binary.BigEndian)
            {
                data[len(prefix) + 1L] = 1L;
            }

            s.SetData(data);
            s.SetSize(int64(len(data)));
            var (r, _) = s.AddRel(objabi.R_ADDR);
            r.SetOff(16L);
            r.SetSiz(uint8(ctxt.Arch.PtrSize));
            r.SetSym(ldr.LookupOrCreateSym("runtime.buildVersion", 0L));
            r, _ = s.AddRel(objabi.R_ADDR);
            r.SetOff(16L + int32(ctxt.Arch.PtrSize));
            r.SetSiz(uint8(ctxt.Arch.PtrSize));
            r.SetSym(ldr.LookupOrCreateSym("runtime.modinfo", 0L));

        }

        // assign addresses to text
        private static void textaddress(this ptr<Link> _addr_ctxt)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            addsection(ctxt.loader, ctxt.Arch, _addr_Segtext, ".text", 05L); 

            // Assign PCs in text segment.
            // Could parallelize, by assigning to text
            // and then letting threads copy down, but probably not worth it.
            var sect = Segtext.Sections[0L];

            sect.Align = int32(Funcalign);

            var ldr = ctxt.loader;
            var text = ldr.LookupOrCreateSym("runtime.text", 0L);
            ldr.SetAttrReachable(text, true);
            ldr.SetSymSect(text, sect);
            if (ctxt.IsAIX() && ctxt.IsExternal())
            { 
                // Setting runtime.text has a real symbol prevents ld to
                // change its base address resulting in wrong offsets for
                // reflect methods.
                var u = ldr.MakeSymbolUpdater(text);
                u.SetAlign(sect.Align);
                u.SetSize(8L);

            }

            if ((ctxt.DynlinkingGo() && ctxt.IsDarwin()) || (ctxt.IsAIX() && ctxt.IsExternal()))
            {
                var etext = ldr.LookupOrCreateSym("runtime.etext", 0L);
                ldr.SetSymSect(etext, sect);

                ctxt.Textp2 = append(ctxt.Textp2, etext, 0L);
                copy(ctxt.Textp2[1L..], ctxt.Textp2);
                ctxt.Textp2[0L] = text;
            }

            var va = uint64(FlagTextAddr.val);
            long n = 1L;
            sect.Vaddr = va;
            long ntramps = 0L;
            {
                var s__prev1 = s;

                foreach (var (_, __s) in ctxt.Textp2)
                {
                    s = __s;
                    sect, n, va = assignAddress(_addr_ctxt, _addr_sect, n, s, va, false);

                    trampoline(_addr_ctxt, s); // resolve jumps, may add trampolines if jump too far

                    // lay down trampolines after each function
                    while (ntramps < len(ctxt.tramps))
                    {
                        var tramp = ctxt.tramps[ntramps];
                        if (ctxt.IsAIX() && strings.HasPrefix(ldr.SymName(tramp), "runtime.text."))
                        { 
                            // Already set in assignAddress
                            continue;
                        ntramps++;
                        }

                        sect, n, va = assignAddress(_addr_ctxt, _addr_sect, n, tramp, va, true);

                    }


                }

                s = s__prev1;
            }

            sect.Length = va - sect.Vaddr;
            etext = ldr.LookupOrCreateSym("runtime.etext", 0L);
            ldr.SetAttrReachable(etext, true);
            ldr.SetSymSect(etext, sect); 

            // merge tramps into Textp, keeping Textp in address order
            if (ntramps != 0L)
            {
                var newtextp = make_slice<loader.Sym>(0L, len(ctxt.Textp2) + ntramps);
                long i = 0L;
                {
                    var s__prev1 = s;

                    foreach (var (_, __s) in ctxt.Textp2)
                    {
                        s = __s;
                        while (i < ntramps && ldr.SymValue(ctxt.tramps[i]) < ldr.SymValue(s))
                        {
                            newtextp = append(newtextp, ctxt.tramps[i]);
                            i++;
                        }

                        newtextp = append(newtextp, s);

                    }

                    s = s__prev1;
                }

                newtextp = append(newtextp, ctxt.tramps[i..ntramps]);

                ctxt.Textp2 = newtextp;

            }

        }

        // assigns address for a text symbol, returns (possibly new) section, its number, and the address
        private static (ptr<sym.Section>, long, ulong) assignAddress(ptr<Link> _addr_ctxt, ptr<sym.Section> _addr_sect, long n, loader.Sym s, ulong va, bool isTramp)
        {
            ptr<sym.Section> _p0 = default!;
            long _p0 = default;
            ulong _p0 = default;
            ref Link ctxt = ref _addr_ctxt.val;
            ref sym.Section sect = ref _addr_sect.val;

            var ldr = ctxt.loader;
            if (thearch.AssignAddress != null)
            {
                return _addr_thearch.AssignAddress(ldr, sect, n, s, va, isTramp)!;
            }

            ldr.SetSymSect(s, sect);
            if (ldr.AttrSubSymbol(s))
            {
                return (_addr_sect!, n, va);
            }

            var align = ldr.SymAlign(s);
            if (align == 0L)
            {
                align = int32(Funcalign);
            }

            va = uint64(Rnd(int64(va), int64(align)));
            if (sect.Align < align)
            {
                sect.Align = align;
            }

            var funcsize = uint64(MINFUNC); // spacing required for findfunctab
            if (ldr.SymSize(s) > MINFUNC)
            {
                funcsize = uint64(ldr.SymSize(s));
            } 

            // On ppc64x a text section should not be larger than 2^26 bytes due to the size of
            // call target offset field in the bl instruction.  Splitting into smaller text
            // sections smaller than this limit allows the GNU linker to modify the long calls
            // appropriately.  The limit allows for the space needed for tables inserted by the linker.

            // If this function doesn't fit in the current text section, then create a new one.

            // Only break at outermost syms.
            if (ctxt.Arch.InFamily(sys.PPC64) && ldr.OuterSym(s) == 0L && ctxt.IsExternal() && va - sect.Vaddr + funcsize + maxSizeTrampolinesPPC64(_addr_ldr, s, isTramp) > 0x1c00000UL)
            { 
                // Set the length for the previous text section
                sect.Length = va - sect.Vaddr; 

                // Create new section, set the starting Vaddr
                sect = addsection(ctxt.loader, ctxt.Arch, _addr_Segtext, ".text", 05L);
                sect.Vaddr = va;
                ldr.SetSymSect(s, sect); 

                // Create a symbol for the start of the secondary text sections
                var ntext = ldr.CreateSymForUpdate(fmt.Sprintf("runtime.text.%d", n), 0L);
                ntext.SetReachable(true);
                ntext.SetSect(sect);
                if (ctxt.IsAIX())
                { 
                    // runtime.text.X must be a real symbol on AIX.
                    // Assign its address directly in order to be the
                    // first symbol of this new section.
                    ntext.SetType(sym.STEXT);
                    ntext.SetSize(int64(MINFUNC));
                    ntext.SetOnList(true);
                    ctxt.tramps = append(ctxt.tramps, ntext.Sym());

                    ntext.SetValue(int64(va));
                    va += uint64(ntext.Size());

                    {
                        var align__prev3 = align;

                        align = ldr.SymAlign(s);

                        if (align != 0L)
                        {
                            va = uint64(Rnd(int64(va), int64(align)));
                        }
                        else
                        {
                            va = uint64(Rnd(int64(va), int64(Funcalign)));
                        }

                        align = align__prev3;

                    }

                }

                n++;

            }

            ldr.SetSymValue(s, 0L);
            {
                var sub = s;

                while (sub != 0L)
                {
                    ldr.SetSymValue(sub, ldr.SymValue(sub) + int64(va));
                    sub = ldr.SubSym(sub);
                }

            }

            va += funcsize;

            return (_addr_sect!, n, va);

        }

        // address assigns virtual addresses to all segments and sections and
        // returns all segments in file order.
        private static slice<ptr<sym.Segment>> address(this ptr<Link> _addr_ctxt) => func((_, panic, __) =>
        {
            ref Link ctxt = ref _addr_ctxt.val;

            slice<ptr<sym.Segment>> order = default; // Layout order

            var va = uint64(FlagTextAddr.val);
            order = append(order, _addr_Segtext);
            Segtext.Rwx = 05L;
            Segtext.Vaddr = va;
            {
                var s__prev1 = s;

                foreach (var (_, __s) in Segtext.Sections)
                {
                    s = __s;
                    va = uint64(Rnd(int64(va), int64(s.Align)));
                    s.Vaddr = va;
                    va += s.Length;
                }

                s = s__prev1;
            }

            Segtext.Length = va - uint64(FlagTextAddr.val);

            if (len(Segrodata.Sections) > 0L)
            { 
                // align to page boundary so as not to mix
                // rodata and executable text.
                //
                // Note: gold or GNU ld will reduce the size of the executable
                // file by arranging for the relro segment to end at a page
                // boundary, and overlap the end of the text segment with the
                // start of the relro segment in the file.  The PT_LOAD segments
                // will be such that the last page of the text segment will be
                // mapped twice, once r-x and once starting out rw- and, after
                // relocation processing, changed to r--.
                //
                // Ideally the last page of the text segment would not be
                // writable even for this short period.
                va = uint64(Rnd(int64(va), int64(FlagRound.val)));

                order = append(order, _addr_Segrodata);
                Segrodata.Rwx = 04L;
                Segrodata.Vaddr = va;
                {
                    var s__prev1 = s;

                    foreach (var (_, __s) in Segrodata.Sections)
                    {
                        s = __s;
                        va = uint64(Rnd(int64(va), int64(s.Align)));
                        s.Vaddr = va;
                        va += s.Length;
                    }

                    s = s__prev1;
                }

                Segrodata.Length = va - Segrodata.Vaddr;

            }

            if (len(Segrelrodata.Sections) > 0L)
            { 
                // align to page boundary so as not to mix
                // rodata, rel-ro data, and executable text.
                va = uint64(Rnd(int64(va), int64(FlagRound.val)));
                if (ctxt.HeadType == objabi.Haix)
                { 
                    // Relro data are inside data segment on AIX.
                    va += uint64(XCOFFDATABASE) - uint64(XCOFFTEXTBASE);

                }

                order = append(order, _addr_Segrelrodata);
                Segrelrodata.Rwx = 06L;
                Segrelrodata.Vaddr = va;
                {
                    var s__prev1 = s;

                    foreach (var (_, __s) in Segrelrodata.Sections)
                    {
                        s = __s;
                        va = uint64(Rnd(int64(va), int64(s.Align)));
                        s.Vaddr = va;
                        va += s.Length;
                    }

                    s = s__prev1;
                }

                Segrelrodata.Length = va - Segrelrodata.Vaddr;

            }

            va = uint64(Rnd(int64(va), int64(FlagRound.val)));
            if (ctxt.HeadType == objabi.Haix && len(Segrelrodata.Sections) == 0L)
            { 
                // Data sections are moved to an unreachable segment
                // to ensure that they are position-independent.
                // Already done if relro sections exist.
                va += uint64(XCOFFDATABASE) - uint64(XCOFFTEXTBASE);

            }

            order = append(order, _addr_Segdata);
            Segdata.Rwx = 06L;
            Segdata.Vaddr = va;
            ptr<sym.Section> data;
            ptr<sym.Section> noptr;
            ptr<sym.Section> bss;
            ptr<sym.Section> noptrbss;
            {
                var i__prev1 = i;
                var s__prev1 = s;

                foreach (var (__i, __s) in Segdata.Sections)
                {
                    i = __i;
                    s = __s;
                    if ((ctxt.IsELF || ctxt.HeadType == objabi.Haix) && s.Name == ".tbss")
                    {
                        continue;
                    }

                    var vlen = int64(s.Length);
                    if (i + 1L < len(Segdata.Sections) && !((ctxt.IsELF || ctxt.HeadType == objabi.Haix) && Segdata.Sections[i + 1L].Name == ".tbss"))
                    {
                        vlen = int64(Segdata.Sections[i + 1L].Vaddr - s.Vaddr);
                    }

                    s.Vaddr = va;
                    va += uint64(vlen);
                    Segdata.Length = va - Segdata.Vaddr;
                    if (s.Name == ".data")
                    {
                        data = s;
                    }

                    if (s.Name == ".noptrdata")
                    {
                        noptr = s;
                    }

                    if (s.Name == ".bss")
                    {
                        bss = s;
                    }

                    if (s.Name == ".noptrbss")
                    {
                        noptrbss = s;
                    }

                } 

                // Assign Segdata's Filelen omitting the BSS. We do this here
                // simply because right now we know where the BSS starts.

                i = i__prev1;
                s = s__prev1;
            }

            Segdata.Filelen = bss.Vaddr - Segdata.Vaddr;

            va = uint64(Rnd(int64(va), int64(FlagRound.val)));
            order = append(order, _addr_Segdwarf);
            Segdwarf.Rwx = 06L;
            Segdwarf.Vaddr = va;
            {
                var i__prev1 = i;
                var s__prev1 = s;

                foreach (var (__i, __s) in Segdwarf.Sections)
                {
                    i = __i;
                    s = __s;
                    vlen = int64(s.Length);
                    if (i + 1L < len(Segdwarf.Sections))
                    {
                        vlen = int64(Segdwarf.Sections[i + 1L].Vaddr - s.Vaddr);
                    }

                    s.Vaddr = va;
                    va += uint64(vlen);
                    if (ctxt.HeadType == objabi.Hwindows)
                    {
                        va = uint64(Rnd(int64(va), PEFILEALIGN));
                    }

                    Segdwarf.Length = va - Segdwarf.Vaddr;

                }

                i = i__prev1;
                s = s__prev1;
            }

            var ldr = ctxt.loader;
            var text = Segtext.Sections[0L];            var rodata = ldr.SymSect(ldr.LookupOrCreateSym("runtime.rodata", 0L));            var itablink = ldr.SymSect(ldr.LookupOrCreateSym("runtime.itablink", 0L));            var symtab = ldr.SymSect(ldr.LookupOrCreateSym("runtime.symtab", 0L));            var pclntab = ldr.SymSect(ldr.LookupOrCreateSym("runtime.pclntab", 0L));            var types = ldr.SymSect(ldr.LookupOrCreateSym("runtime.types", 0L));
            var lasttext = text; 
            // Could be multiple .text sections
            {
                var sect__prev1 = sect;

                foreach (var (_, __sect) in Segtext.Sections)
                {
                    sect = __sect;
                    if (sect.Name == ".text")
                    {
                        lasttext = sect;
                    }

                }

                sect = sect__prev1;
            }

            {
                var s__prev1 = s;

                foreach (var (_, __s) in ctxt.datap2)
                {
                    s = __s;
                    {
                        var sect__prev1 = sect;

                        var sect = ldr.SymSect(s);

                        if (sect != null)
                        {
                            ldr.AddToSymValue(s, int64(sect.Vaddr));
                        }

                        sect = sect__prev1;

                    }

                    var v = ldr.SymValue(s);
                    {
                        var sub__prev2 = sub;

                        var sub = ldr.SubSym(s);

                        while (sub != 0L)
                        {
                            ldr.AddToSymValue(sub, v);
                            sub = ldr.SubSym(sub);
                        }


                        sub = sub__prev2;
                    }

                }

                s = s__prev1;
            }

            foreach (var (_, si) in dwarfp2)
            {
                {
                    var s__prev2 = s;

                    foreach (var (_, __s) in si.syms)
                    {
                        s = __s;
                        {
                            var sect__prev1 = sect;

                            sect = ldr.SymSect(s);

                            if (sect != null)
                            {
                                ldr.AddToSymValue(s, int64(sect.Vaddr));
                            }

                            sect = sect__prev1;

                        }

                        sub = ldr.SubSym(s);
                        if (sub != 0L)
                        {
                            panic(fmt.Sprintf("unexpected sub-sym for %s %s", ldr.SymName(s), ldr.SymType(s).String()));
                        }

                        v = ldr.SymValue(s);
                        while (sub != 0L)
                        {
                            ldr.AddToSymValue(s, v);
                            sub = ldr.SubSym(sub);
                        }


                    }

                    s = s__prev2;
                }
            }
            if (ctxt.BuildMode == BuildModeShared)
            {
                var s = ldr.LookupOrCreateSym("go.link.abihashbytes", 0L);
                sect = ldr.SymSect(ldr.LookupOrCreateSym(".note.go.abihash", 0L));
                ldr.SetSymSect(s, sect);
                ldr.SetSymValue(s, int64(sect.Vaddr + 16L));
            }

            ctxt.xdefine2("runtime.text", sym.STEXT, int64(text.Vaddr));
            ctxt.xdefine2("runtime.etext", sym.STEXT, int64(lasttext.Vaddr + lasttext.Length)); 

            // If there are multiple text sections, create runtime.text.n for
            // their section Vaddr, using n for index
            long n = 1L;
            {
                var sect__prev1 = sect;

                foreach (var (_, __sect) in Segtext.Sections[1L..])
                {
                    sect = __sect;
                    if (sect.Name != ".text")
                    {
                        break;
                    }

                    var symname = fmt.Sprintf("runtime.text.%d", n);
                    if (ctxt.HeadType != objabi.Haix || ctxt.LinkMode != LinkExternal)
                    { 
                        // Addresses are already set on AIX with external linker
                        // because these symbols are part of their sections.
                        ctxt.xdefine2(symname, sym.STEXT, int64(sect.Vaddr));

                    }

                    n++;

                }

                sect = sect__prev1;
            }

            ctxt.xdefine2("runtime.rodata", sym.SRODATA, int64(rodata.Vaddr));
            ctxt.xdefine2("runtime.erodata", sym.SRODATA, int64(rodata.Vaddr + rodata.Length));
            ctxt.xdefine2("runtime.types", sym.SRODATA, int64(types.Vaddr));
            ctxt.xdefine2("runtime.etypes", sym.SRODATA, int64(types.Vaddr + types.Length));
            ctxt.xdefine2("runtime.itablink", sym.SRODATA, int64(itablink.Vaddr));
            ctxt.xdefine2("runtime.eitablink", sym.SRODATA, int64(itablink.Vaddr + itablink.Length));

            s = ldr.Lookup("runtime.gcdata", 0L);
            ldr.SetAttrLocal(s, true);
            ctxt.xdefine2("runtime.egcdata", sym.SRODATA, ldr.SymAddr(s) + ldr.SymSize(s));
            ldr.SetSymSect(ldr.LookupOrCreateSym("runtime.egcdata", 0L), ldr.SymSect(s));

            s = ldr.LookupOrCreateSym("runtime.gcbss", 0L);
            ldr.SetAttrLocal(s, true);
            ctxt.xdefine2("runtime.egcbss", sym.SRODATA, ldr.SymAddr(s) + ldr.SymSize(s));
            ldr.SetSymSect(ldr.LookupOrCreateSym("runtime.egcbss", 0L), ldr.SymSect(s));

            ctxt.xdefine2("runtime.symtab", sym.SRODATA, int64(symtab.Vaddr));
            ctxt.xdefine2("runtime.esymtab", sym.SRODATA, int64(symtab.Vaddr + symtab.Length));
            ctxt.xdefine2("runtime.pclntab", sym.SRODATA, int64(pclntab.Vaddr));
            ctxt.xdefine2("runtime.epclntab", sym.SRODATA, int64(pclntab.Vaddr + pclntab.Length));
            ctxt.xdefine2("runtime.noptrdata", sym.SNOPTRDATA, int64(noptr.Vaddr));
            ctxt.xdefine2("runtime.enoptrdata", sym.SNOPTRDATA, int64(noptr.Vaddr + noptr.Length));
            ctxt.xdefine2("runtime.bss", sym.SBSS, int64(bss.Vaddr));
            ctxt.xdefine2("runtime.ebss", sym.SBSS, int64(bss.Vaddr + bss.Length));
            ctxt.xdefine2("runtime.data", sym.SDATA, int64(data.Vaddr));
            ctxt.xdefine2("runtime.edata", sym.SDATA, int64(data.Vaddr + data.Length));
            ctxt.xdefine2("runtime.noptrbss", sym.SNOPTRBSS, int64(noptrbss.Vaddr));
            ctxt.xdefine2("runtime.enoptrbss", sym.SNOPTRBSS, int64(noptrbss.Vaddr + noptrbss.Length));
            ctxt.xdefine2("runtime.end", sym.SBSS, int64(Segdata.Vaddr + Segdata.Length));

            if (ctxt.IsSolaris())
            { 
                // On Solaris, in the runtime it sets the external names of the
                // end symbols. Unset them and define separate symbols, so we
                // keep both.
                var etext = ldr.Lookup("runtime.etext", 0L);
                var edata = ldr.Lookup("runtime.edata", 0L);
                var end = ldr.Lookup("runtime.end", 0L);
                ldr.SetSymExtname(etext, "runtime.etext");
                ldr.SetSymExtname(edata, "runtime.edata");
                ldr.SetSymExtname(end, "runtime.end");
                ctxt.xdefine2("_etext", ldr.SymType(etext), ldr.SymValue(etext));
                ctxt.xdefine2("_edata", ldr.SymType(edata), ldr.SymValue(edata));
                ctxt.xdefine2("_end", ldr.SymType(end), ldr.SymValue(end));
                ldr.SetSymSect(ldr.Lookup("_etext", 0L), ldr.SymSect(etext));
                ldr.SetSymSect(ldr.Lookup("_edata", 0L), ldr.SymSect(edata));
                ldr.SetSymSect(ldr.Lookup("_end", 0L), ldr.SymSect(end));

            }

            return order;

        });

        // layout assigns file offsets and lengths to the segments in order.
        // Returns the file size containing all the segments.
        private static ulong layout(this ptr<Link> _addr_ctxt, slice<ptr<sym.Segment>> order)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            ptr<sym.Segment> prev;
            foreach (var (_, seg) in order)
            {
                if (prev == null)
                {
                    seg.Fileoff = uint64(HEADR);
                }
                else
                {

                    if (ctxt.HeadType == objabi.Hwindows) 
                        seg.Fileoff = prev.Fileoff + uint64(Rnd(int64(prev.Filelen), PEFILEALIGN));
                    else if (ctxt.HeadType == objabi.Hplan9) 
                        seg.Fileoff = prev.Fileoff + prev.Filelen;
                    else 
                        // Assuming the previous segment was
                        // aligned, the following rounding
                        // should ensure that this segment's
                        // VA ≡ Fileoff mod FlagRound.
                        seg.Fileoff = uint64(Rnd(int64(prev.Fileoff + prev.Filelen), int64(FlagRound.val)));
                        if (seg.Vaddr % uint64(FlagRound.val) != seg.Fileoff % uint64(FlagRound.val))
                        {
                            Exitf("bad segment rounding (Vaddr=%#x Fileoff=%#x FlagRound=%#x)", seg.Vaddr, seg.Fileoff, FlagRound.val);
                        }

                                    }

                if (seg != _addr_Segdata)
                { 
                    // Link.address already set Segdata.Filelen to
                    // account for BSS.
                    seg.Filelen = seg.Length;

                }

                prev = seg;

            }
            return prev.Fileoff + prev.Filelen;

        }

        // add a trampoline with symbol s (to be laid down after the current function)
        private static void AddTramp(this ptr<Link> _addr_ctxt, ptr<loader.SymbolBuilder> _addr_s)
        {
            ref Link ctxt = ref _addr_ctxt.val;
            ref loader.SymbolBuilder s = ref _addr_s.val;

            s.SetType(sym.STEXT);
            s.SetReachable(true);
            s.SetOnList(true);
            ctxt.tramps = append(ctxt.tramps, s.Sym());
            if (FlagDebugTramp > 0L && ctxt.Debugvlog > 0L.val)
            {
                ctxt.Logf("trampoline %s inserted\n", s.Name());
            }

        }

        // compressSyms compresses syms and returns the contents of the
        // compressed section. If the section would get larger, it returns nil.
        private static slice<byte> compressSyms(ptr<Link> _addr_ctxt, slice<loader.Sym> syms)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            var ldr = ctxt.loader;
            long total = default;
            foreach (var (_, sym) in syms)
            {
                total += ldr.SymSize(sym);
            }
            ref bytes.Buffer buf = ref heap(out ptr<bytes.Buffer> _addr_buf);
            buf.Write((slice<byte>)"ZLIB");
            array<byte> sizeBytes = new array<byte>(8L);
            binary.BigEndian.PutUint64(sizeBytes[..], uint64(total));
            buf.Write(sizeBytes[..]);

            slice<byte> relocbuf = default; // temporary buffer for applying relocations

            // Using zlib.BestSpeed achieves very nearly the same
            // compression levels of zlib.DefaultCompression, but takes
            // substantially less time. This is important because DWARF
            // compression can be a significant fraction of link time.
            var (z, err) = zlib.NewWriterLevel(_addr_buf, zlib.BestSpeed);
            if (err != null)
            {
                log.Fatalf("NewWriterLevel failed: %s", err);
            }

            var st = ctxt.makeRelocSymState();
            foreach (var (_, s) in syms)
            { 
                // Symbol data may be read-only. Apply relocations in a
                // temporary buffer, and immediately write it out.
                var P = ldr.Data(s);
                var relocs = ldr.Relocs(s);
                if (relocs.Count() != 0L)
                {
                    relocbuf = append(relocbuf[..0L], P);
                    P = relocbuf;
                }

                st.relocsym(s, P);
                {
                    var (_, err) = z.Write(P);

                    if (err != null)
                    {
                        log.Fatalf("compression failed: %s", err);
                    }

                }

                {
                    var i = ldr.SymSize(s) - int64(len(P));

                    while (i > 0L)
                    {
                        var b = zeros[..];
                        if (i < int64(len(b)))
                        {
                            b = b[..i];
                        }

                        var (n, err) = z.Write(b);
                        if (err != null)
                        {
                            log.Fatalf("compression failed: %s", err);
                        }

                        i -= int64(n);

                    }

                }

            }
            {
                var err = z.Close();

                if (err != null)
                {
                    log.Fatalf("compression failed: %s", err);
                }

            }

            if (int64(buf.Len()) >= total)
            { 
                // Compression didn't save any space.
                return null;

            }

            return buf.Bytes();

        }
    }
}}}}
