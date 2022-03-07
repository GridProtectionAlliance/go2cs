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

// package ld -- go2cs converted at 2022 March 06 23:20:54 UTC
// import "cmd/link/internal/ld" ==> using ld = go.cmd.link.@internal.ld_package
// Original source: C:\Program Files\Go\src\cmd\link\internal\ld\data.go
using bytes = go.bytes_package;
using gcprog = go.cmd.@internal.gcprog_package;
using objabi = go.cmd.@internal.objabi_package;
using sys = go.cmd.@internal.sys_package;
using loader = go.cmd.link.@internal.loader_package;
using sym = go.cmd.link.@internal.sym_package;
using zlib = go.compress.zlib_package;
using elf = go.debug.elf_package;
using binary = go.encoding.binary_package;
using fmt = go.fmt_package;
using log = go.log_package;
using os = go.os_package;
using sort = go.sort_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using sync = go.sync_package;
using atomic = go.sync.atomic_package;
using System;
using System.Threading;


namespace go.cmd.link.@internal;

public static partial class ld_package {

    // isRuntimeDepPkg reports whether pkg is the runtime package or its dependency
private static bool isRuntimeDepPkg(@string pkg) {
    switch (pkg) {
        case "runtime": // for cpu features

        case "sync/atomic": // for cpu features

        case "internal/abi": // for cpu features

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
private static ulong maxSizeTrampolines(ptr<Link> _addr_ctxt, ptr<loader.Loader> _addr_ldr, loader.Sym s, bool isTramp) => func((_, panic, _) => {
    ref Link ctxt = ref _addr_ctxt.val;
    ref loader.Loader ldr = ref _addr_ldr.val;
 
    // If thearch.Trampoline is nil, then trampoline support is not available on this arch.
    // A trampoline does not need any dependent trampolines.
    if (thearch.Trampoline == null || isTramp) {
        return 0;
    }
    var n = uint64(0);
    var relocs = ldr.Relocs(s);
    for (nint ri = 0; ri < relocs.Count(); ri++) {
        var r = relocs.At(ri);
        if (r.Type().IsDirectCallOrJump()) {
            n++;
        }
    }

    if (ctxt.IsPPC64()) {
        return n * 16; // Trampolines in PPC64 are 4 instructions.
    }
    if (ctxt.IsARM64()) {
        return n * 12; // Trampolines in ARM64 are 3 instructions.
    }
    panic("unreachable");

});

// detect too-far jumps in function s, and add trampolines if necessary
// ARM, PPC64 & PPC64LE support trampoline insertion for internal and external linking
// On PPC64 & PPC64LE the text sections might be split but will still insert trampolines
// where necessary.
private static void trampoline(ptr<Link> _addr_ctxt, loader.Sym s) {
    ref Link ctxt = ref _addr_ctxt.val;

    if (thearch.Trampoline == null) {
        return ; // no need or no support of trampolines on this arch
    }
    var ldr = ctxt.loader;
    var relocs = ldr.Relocs(s);
    for (nint ri = 0; ri < relocs.Count(); ri++) {
        var r = relocs.At(ri);
        var rt = r.Type();
        if (!rt.IsDirectCallOrJump() && !isPLTCall(rt)) {
            continue;
        }
        var rs = r.Sym();
        if (!ldr.AttrReachable(rs) || ldr.SymType(rs) == sym.Sxxx) {
            continue; // something is wrong. skip it here and we'll emit a better error later
        }
        rs = ldr.ResolveABIAlias(rs);
        if (ldr.SymValue(rs) == 0 && (ldr.SymType(rs) != sym.SDYNIMPORT && ldr.SymType(rs) != sym.SUNDEFEXT)) {
            if (ldr.SymPkg(s) != "" && ldr.SymPkg(rs) == ldr.SymPkg(s)) { 
                // Symbols in the same package are laid out together.
                // Except that if SymPkg(s) == "", it is a host object symbol
                // which may call an external symbol via PLT.
                continue;

            }

            if (isRuntimeDepPkg(ldr.SymPkg(s)) && isRuntimeDepPkg(ldr.SymPkg(rs))) {
                continue; // runtime packages are laid out together
            }

        }
        thearch.Trampoline(ctxt, ldr, ri, rs, s);

    }

}

// whether rt is a (host object) relocation that will be turned into
// a call to PLT.
private static bool isPLTCall(objabi.RelocType rt) {
    const nint pcrel = 1;


    // ARM64
    if (rt == objabi.ElfRelocOffset + objabi.RelocType(elf.R_AARCH64_CALL26) || rt == objabi.ElfRelocOffset + objabi.RelocType(elf.R_AARCH64_JUMP26) || rt == objabi.MachoRelocOffset + MACHO_ARM64_RELOC_BRANCH26 * 2 + pcrel) 
        return true; 

        // ARM
    else if (rt == objabi.ElfRelocOffset + objabi.RelocType(elf.R_ARM_CALL) || rt == objabi.ElfRelocOffset + objabi.RelocType(elf.R_ARM_PC24) || rt == objabi.ElfRelocOffset + objabi.RelocType(elf.R_ARM_JUMP24)) 
        return true;
    // TODO: other architectures.
    return false;

}

// FoldSubSymbolOffset computes the offset of symbol s to its top-level outer
// symbol. Returns the top-level symbol and the offset.
// This is used in generating external relocations.
public static (loader.Sym, long) FoldSubSymbolOffset(ptr<loader.Loader> _addr_ldr, loader.Sym s) {
    loader.Sym _p0 = default;
    long _p0 = default;
    ref loader.Loader ldr = ref _addr_ldr.val;

    var outer = ldr.OuterSym(s);
    var off = int64(0);
    if (outer != 0) {
        off += ldr.SymValue(s) - ldr.SymValue(outer);
        s = outer;
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
private static void relocsym(this ptr<relocSymState> _addr_st, loader.Sym s, slice<byte> P) {
    ref relocSymState st = ref _addr_st.val;

    var ldr = st.ldr;
    var relocs = ldr.Relocs(s);
    if (relocs.Count() == 0) {
        return ;
    }
    var target = st.target;
    var syms = st.syms;
    nint nExtReloc = 0; // number of external relocations
    for (nint ri = 0; ri < relocs.Count(); ri++) {
        var r = relocs.At(ri);
        var off = r.Off();
        var siz = int32(r.Siz());
        var rs = r.Sym();
        rs = ldr.ResolveABIAlias(rs);
        var rt = r.Type();
        var weak = r.Weak();
        if (off < 0 || off + siz > int32(len(P))) {
            @string rname = "";
            if (rs != 0) {
                rname = ldr.SymName(rs);
            }
            st.err.Errorf(s, "invalid relocation %s: %d+%d not in [%d,%d)", rname, off, siz, 0, len(P));
            continue;
        }
        if (siz == 0) { // informational relocation - no work to do
            continue;

        }
        sym.SymKind rst = default;
        if (rs != 0) {
            rst = ldr.SymType(rs);
        }
        if (rs != 0 && ((rst == sym.Sxxx && !ldr.AttrVisibilityHidden(rs)) || rst == sym.SXREF)) { 
            // When putting the runtime but not main into a shared library
            // these symbols are undefined and that's OK.
            if (target.IsShared() || target.IsPlugin()) {
                if (ldr.SymName(rs) == "main.main" || (!target.IsPlugin() && ldr.SymName(rs) == "main..inittask")) {
                    var sb = ldr.MakeSymbolUpdater(rs);
                    sb.SetType(sym.SDYNIMPORT);
                }
                else if (strings.HasPrefix(ldr.SymName(rs), "go.info.")) { 
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
        if (rt >= objabi.ElfRelocOffset) {
            continue;
        }
        if (!target.IsAIX() && !target.IsDarwin() && !target.IsSolaris() && !target.IsOpenbsd() && rs != 0 && rst == sym.SDYNIMPORT && !target.IsDynlinkingGo() && !ldr.AttrSubSymbol(rs)) {
            if (!(target.IsPPC64() && target.IsExternal() && ldr.SymName(rs) == ".TOC.")) {
                st.err.Errorf(s, "unhandled relocation for %s (type %d (%s) rtype %d (%s))", ldr.SymName(rs), rst, rst, rt, sym.RelocName(target.Arch, rt));
            }
        }
        if (rs != 0 && rst != sym.STLSBSS && !weak && rt != objabi.R_METHODOFF && !ldr.AttrReachable(rs)) {
            st.err.Errorf(s, "unreachable sym in relocation: %s", ldr.SymName(rs));
        }
        sym.RelocVariant rv = default;
        if (target.IsPPC64() || target.IsS390X()) {
            rv = ldr.RelocVariant(s, ri);
        }
        if (target.IsS390X()) {

            if (rt == objabi.R_PCRELDBL) 
                rt = objabi.R_PCREL;
                rv = sym.RV_390_DBL;
            else if (rt == objabi.R_CALL) 
                rv = sym.RV_390_DBL;
            
        }
        long o = default;

        if (rt == objabi.R_TLS_LE)
        {
            if (target.IsExternal() && target.IsElf()) {
                nExtReloc++;
                o = 0;
                if (!target.IsAMD64()) {
                    o = r.Add();
                }
                break;
            }
            if (target.IsElf() && target.IsARM()) { 
                // On ELF ARM, the thread pointer is 8 bytes before
                // the start of the thread-local data block, so add 8
                // to the actual TLS offset (r->sym->value).
                // This 8 seems to be a fundamental constant of
                // ELF on ARM (or maybe Glibc on ARM); it is not
                // related to the fact that our own TLS storage happens
                // to take up 8 bytes.
                o = 8 + ldr.SymValue(rs);

            }
            else if (target.IsElf() || target.IsPlan9() || target.IsDarwin()) {
                o = int64(syms.Tlsoffset) + r.Add();
            }
            else if (target.IsWindows()) {
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
            if (target.IsExternal() && target.IsElf()) {
                nExtReloc++;
                o = 0;
                if (!target.IsAMD64()) {
                    o = r.Add();
                }
                if (target.Is386()) {
                    nExtReloc++; // need two ELF relocations on 386, see ../x86/asm.go:elfreloc1
                }

                break;

            }

            if (target.IsPIE() && target.IsElf()) { 
                // We are linking the final executable, so we
                // can optimize any TLS IE relocation to LE.
                if (thearch.TLSIEtoLE == null) {
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
            if (weak && !ldr.AttrReachable(rs)) { 
                // Redirect it to runtime.unreachableMethod, which will throw if called.
                rs = syms.unreachableMethod;
                rs = ldr.ResolveABIAlias(rs);

            }

            if (target.IsExternal()) {
                nExtReloc++; 

                // set up addend for eventual relocation via outer symbol.
                rs = rs;
                var (rs, off) = FoldSubSymbolOffset(_addr_ldr, rs);
                var xadd = r.Add() + off;
                rst = ldr.SymType(rs);
                if (rst != sym.SHOSTOBJ && rst != sym.SDYNIMPORT && rst != sym.SUNDEFEXT && ldr.SymSect(rs) == null) {
                    st.err.Errorf(s, "missing section for relocation target %s", ldr.SymName(rs));
                }

                o = xadd;
                if (target.IsElf()) {
                    if (target.IsAMD64()) {
                        o = 0;
                    }
                }
                else if (target.IsDarwin()) {
                    if (ldr.SymType(rs) != sym.SHOSTOBJ) {
                        o += ldr.SymValue(rs);
                    }
                }
                else if (target.IsWindows()) { 
                    // nothing to do
                }
                else if (target.IsAIX()) {
                    o = ldr.SymValue(rs) + xadd;
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
            if (target.IsAIX() && rst != sym.SDYNIMPORT) { 
                // It's not possible to make a loader relocation in a
                // symbol which is not inside .data section.
                // FIXME: It should be forbidden to have R_ADDR from a
                // symbol which isn't in .data. However, as .text has the
                // same address once loaded, this is possible.
                if (ldr.SymSect(s).Seg == _addr_Segdata) {
                    Xcoffadddynrel(target, ldr, syms, s, r, ri);
                }

            }

            o = ldr.SymValue(rs) + r.Add(); 

            // On amd64, 4-byte offsets will be sign-extended, so it is impossible to
            // access more than 2GB of static data; fail at link time is better than
            // fail at runtime. See https://golang.org/issue/7980.
            // Instead of special casing only amd64, we treat this as an error on all
            // 64-bit architectures so as to be future-proof.
            if (int32(o) < 0 && target.Arch.PtrSize > 4 && siz == 4) {
                st.err.Errorf(s, "non-pc-relative relocation address for %s is too big: %#x (%#x + %#x)", ldr.SymName(rs), uint64(o), ldr.SymValue(rs), r.Add());
                errorexit();
            }

            goto __switch_break0;
        }
        if (rt == objabi.R_DWARFSECREF)
        {
            if (ldr.SymSect(rs) == null) {
                st.err.Errorf(s, "missing DWARF section for relocation target %s", ldr.SymName(rs));
            }
            if (target.IsExternal()) { 
                // On most platforms, the external linker needs to adjust DWARF references
                // as it combines DWARF sections. However, on Darwin, dsymutil does the
                // DWARF linking, and it understands how to follow section offsets.
                // Leaving in the relocation records confuses it (see
                // https://golang.org/issue/22068) so drop them for Darwin.
                if (!target.IsDarwin()) {
                    nExtReloc++;
                }

                xadd = r.Add() + ldr.SymValue(rs) - int64(ldr.SymSect(rs).Vaddr);

                o = xadd;
                if (target.IsElf() && target.IsAMD64()) {
                    o = 0;
                }

                break;

            }

            o = ldr.SymValue(rs) + r.Add() - int64(ldr.SymSect(rs).Vaddr);
            goto __switch_break0;
        }
        if (rt == objabi.R_METHODOFF)
        {
            if (!ldr.AttrReachable(rs)) { 
                // Set it to a sentinel value. The runtime knows this is not pointing to
                // anything valid.
                o = -1;
                break;

            }

            fallthrough = true;
        }
        if (fallthrough || rt == objabi.R_ADDROFF)
        {
            if (weak && !ldr.AttrReachable(rs)) {
                continue;
            } 
            // The method offset tables using this relocation expect the offset to be relative
            // to the start of the first text section, even if there are multiple.
            if (ldr.SymSect(rs).Name == ".text") {
                o = ldr.SymValue(rs) - int64(Segtext.Sections[0].Vaddr) + r.Add();
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
            o = ldr.SymValue(rs) + r.Add() - ldr.SymValue(loader.Sym(ldr.SymUnit(rs).Textp[0])); 

            // r.Sym() can be 0 when CALL $(constant) is transformed from absolute PC to relative PC call.
            goto __switch_break0;
        }
        if (rt == objabi.R_GOTPCREL)
        {
            if (target.IsDynlinkingGo() && target.IsDarwin() && rs != 0) {
                nExtReloc++;
                o = r.Add();
                break;
            }
            if (target.Is386() && target.IsExternal() && target.IsELF) {
                nExtReloc++; // need two ELF relocations on 386, see ../x86/asm.go:elfreloc1
            }

            fallthrough = true;
        }
        if (fallthrough || rt == objabi.R_CALL || rt == objabi.R_PCREL)
        {
            if (target.IsExternal() && rs != 0 && rst == sym.SUNDEFEXT) { 
                // pass through to the external linker.
                nExtReloc++;
                o = 0;
                break;

            }

            if (target.IsExternal() && rs != 0 && (ldr.SymSect(rs) != ldr.SymSect(s) || rt == objabi.R_GOTPCREL)) {
                nExtReloc++; 

                // set up addend for eventual relocation via outer symbol.
                rs = rs;
                (rs, off) = FoldSubSymbolOffset(_addr_ldr, rs);
                xadd = r.Add() + off - int64(siz); // relative to address after the relocated chunk
                rst = ldr.SymType(rs);
                if (rst != sym.SHOSTOBJ && rst != sym.SDYNIMPORT && ldr.SymSect(rs) == null) {
                    st.err.Errorf(s, "missing section for relocation target %s", ldr.SymName(rs));
                }

                o = xadd;
                if (target.IsElf()) {
                    if (target.IsAMD64()) {
                        o = 0;
                    }
                }
                else if (target.IsDarwin()) {
                    if (rt == objabi.R_CALL) {
                        if (target.IsExternal() && rst == sym.SDYNIMPORT) {
                            if (target.IsAMD64()) { 
                                // AMD64 dynamic relocations are relative to the end of the relocation.
                                o += int64(siz);

                            }

                        }
                        else
 {
                            if (rst != sym.SHOSTOBJ) {
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
                else if (target.IsWindows() && target.IsAMD64()) { // only amd64 needs PCREL
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

            o = 0;
            if (rs != 0) {
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
            if (!target.IsAIX()) {
                st.err.Errorf(s, "find XCOFF R_REF on non-XCOFF files");
            }
            if (!target.IsExternal()) {
                st.err.Errorf(s, "find XCOFF R_REF with internal linking");
            }
            nExtReloc++;
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
            o = ldr.SymValue(rs) + r.Add() - ldr.SymValue(syms.GOT);
            goto __switch_break0;
        }
        // default: 
            switch (siz) {
                case 1: 
                    o = int64(P[off]);
                    break;
                case 2: 
                    o = int64(target.Arch.ByteOrder.Uint16(P[(int)off..]));
                    break;
                case 4: 
                    o = int64(target.Arch.ByteOrder.Uint32(P[(int)off..]));
                    break;
                case 8: 
                    o = int64(target.Arch.ByteOrder.Uint64(P[(int)off..]));
                    break;
                default: 
                    st.err.Errorf(s, "bad reloc size %#x for %s", uint32(siz), ldr.SymName(rs));
                    break;
            }
            var (out, n, ok) = thearch.Archreloc(target, ldr, syms, r, s, o);
            if (target.IsExternal()) {
                nExtReloc += n;
            }

            if (ok) {
                o = out;
            }
            else
 {
                st.err.Errorf(s, "unknown reloc to %v: %d (%s)", ldr.SymName(rs), rt, sym.RelocName(target.Arch, rt));
            }


        __switch_break0:;

        if (target.IsPPC64() || target.IsS390X()) {
            if (rv != sym.RV_NONE) {
                o = thearch.Archrelocvariant(target, ldr, r, rv, s, o, P);
            }
        }
        switch (siz) {
            case 1: 
                P[off] = byte(int8(o));
                break;
            case 2: 
                if (o != int64(int16(o))) {
                    st.err.Errorf(s, "relocation address for %s is too big: %#x", ldr.SymName(rs), o);
                }
                target.Arch.ByteOrder.PutUint16(P[(int)off..], uint16(o));
                break;
            case 4: 
                           if (rt == objabi.R_PCREL || rt == objabi.R_CALL) {
                               if (o != int64(int32(o))) {
                                   st.err.Errorf(s, "pc-relative relocation address for %s is too big: %#x", ldr.SymName(rs), o);
                               }
                           }
                           else
                {
                               if (o != int64(int32(o)) && o != int64(uint32(o))) {
                                   st.err.Errorf(s, "non-pc-relative relocation address for %s is too big: %#x", ldr.SymName(rs), uint64(o));
                               }
                           }

                           target.Arch.ByteOrder.PutUint32(P[(int)off..], uint32(o));

                break;
            case 8: 
                target.Arch.ByteOrder.PutUint64(P[(int)off..], uint64(o));
                break;
            default: 
                st.err.Errorf(s, "bad reloc size %#x for %s", uint32(siz), ldr.SymName(rs));
                break;
        }

    }
    if (target.IsExternal()) { 
        // We'll stream out the external relocations in asmb2 (e.g. elfrelocsect)
        // and we only need the count here.
        atomic.AddUint32(_addr_ldr.SymSect(s).Relcount, uint32(nExtReloc));

    }
}

// Convert a Go relocation to an external relocation.
private static (loader.ExtReloc, bool) extreloc(ptr<Link> _addr_ctxt, ptr<loader.Loader> _addr_ldr, loader.Sym s, loader.Reloc r) {
    loader.ExtReloc _p0 = default;
    bool _p0 = default;
    ref Link ctxt = ref _addr_ctxt.val;
    ref loader.Loader ldr = ref _addr_ldr.val;

    loader.ExtReloc rr = default;
    var target = _addr_ctxt.Target;
    var siz = int32(r.Siz());
    if (siz == 0) { // informational relocation - no work to do
        return (rr, false);

    }
    var rt = r.Type();
    if (rt >= objabi.ElfRelocOffset) {
        return (rr, false);
    }
    rr.Type = rt;
    rr.Size = uint8(siz); 

    // TODO(mundaym): remove this special case - see issue 14218.
    if (target.IsS390X()) {

        if (rt == objabi.R_PCRELDBL) 
            rt = objabi.R_PCREL;
        
    }

    if (rt == objabi.R_TLS_LE || rt == objabi.R_TLS_IE) 
        if (target.IsElf()) {
            var rs = ldr.ResolveABIAlias(r.Sym());
            rr.Xsym = rs;
            if (rr.Xsym == 0) {
                rr.Xsym = ctxt.Tlsg;
            }
            rr.Xadd = r.Add();
            break;
        }
        return (rr, false);
    else if (rt == objabi.R_ADDR) 
        // set up addend for eventual relocation via outer symbol.
        rs = ldr.ResolveABIAlias(r.Sym());
        if (r.Weak() && !ldr.AttrReachable(rs)) {
            rs = ctxt.ArchSyms.unreachableMethod;
            rs = ldr.ResolveABIAlias(rs);
        }
        var (rs, off) = FoldSubSymbolOffset(_addr_ldr, rs);
        rr.Xadd = r.Add() + off;
        rr.Xsym = rs;
    else if (rt == objabi.R_DWARFSECREF) 
        // On most platforms, the external linker needs to adjust DWARF references
        // as it combines DWARF sections. However, on Darwin, dsymutil does the
        // DWARF linking, and it understands how to follow section offsets.
        // Leaving in the relocation records confuses it (see
        // https://golang.org/issue/22068) so drop them for Darwin.
        if (target.IsDarwin()) {
            return (rr, false);
        }
        rs = ldr.ResolveABIAlias(r.Sym());
        rr.Xsym = loader.Sym(ldr.SymSect(rs).Sym);
        rr.Xadd = r.Add() + ldr.SymValue(rs) - int64(ldr.SymSect(rs).Vaddr); 

        // r.Sym() can be 0 when CALL $(constant) is transformed from absolute PC to relative PC call.
    else if (rt == objabi.R_GOTPCREL || rt == objabi.R_CALL || rt == objabi.R_PCREL) 
        rs = ldr.ResolveABIAlias(r.Sym());
        if (rt == objabi.R_GOTPCREL && target.IsDynlinkingGo() && target.IsDarwin() && rs != 0) {
            rr.Xadd = r.Add();
            rr.Xadd -= int64(siz); // relative to address after the relocated chunk
            rr.Xsym = rs;
            break;

        }
        if (rs != 0 && ldr.SymType(rs) == sym.SUNDEFEXT) { 
            // pass through to the external linker.
            rr.Xadd = 0;
            if (target.IsElf()) {
                rr.Xadd -= int64(siz);
            }

            rr.Xsym = rs;
            break;

        }
        if (rs != 0 && (ldr.SymSect(rs) != ldr.SymSect(s) || rt == objabi.R_GOTPCREL)) { 
            // set up addend for eventual relocation via outer symbol.
            rs = rs;
            (rs, off) = FoldSubSymbolOffset(_addr_ldr, rs);
            rr.Xadd = r.Add() + off;
            rr.Xadd -= int64(siz); // relative to address after the relocated chunk
            rr.Xsym = rs;
            break;

        }
        return (rr, false);
    else if (rt == objabi.R_XCOFFREF) 
        return (ExtrelocSimple(_addr_ldr, r), true); 

        // These reloc types don't need external relocations.
    else if (rt == objabi.R_ADDROFF || rt == objabi.R_METHODOFF || rt == objabi.R_ADDRCUOFF || rt == objabi.R_SIZE || rt == objabi.R_CONST || rt == objabi.R_GOTOFF) 
        return (rr, false);
    else 
        return thearch.Extreloc(target, ldr, r, s);
        return (rr, true);

}

// ExtrelocSimple creates a simple external relocation from r, with the same
// symbol and addend.
public static loader.ExtReloc ExtrelocSimple(ptr<loader.Loader> _addr_ldr, loader.Reloc r) {
    ref loader.Loader ldr = ref _addr_ldr.val;

    loader.ExtReloc rr = default;
    var rs = ldr.ResolveABIAlias(r.Sym());
    rr.Xsym = rs;
    rr.Xadd = r.Add();
    rr.Type = r.Type();
    rr.Size = r.Siz();
    return rr;
}

// ExtrelocViaOuterSym creates an external relocation from r targeting the
// outer symbol and folding the subsymbol's offset into the addend.
public static loader.ExtReloc ExtrelocViaOuterSym(ptr<loader.Loader> _addr_ldr, loader.Reloc r, loader.Sym s) {
    ref loader.Loader ldr = ref _addr_ldr.val;
 
    // set up addend for eventual relocation via outer symbol.
    loader.ExtReloc rr = default;
    var rs = ldr.ResolveABIAlias(r.Sym());
    var (rs, off) = FoldSubSymbolOffset(_addr_ldr, rs);
    rr.Xadd = r.Add() + off;
    var rst = ldr.SymType(rs);
    if (rst != sym.SHOSTOBJ && rst != sym.SDYNIMPORT && rst != sym.SUNDEFEXT && ldr.SymSect(rs) == null) {
        ldr.Errorf(s, "missing section for %s", ldr.SymName(rs));
    }
    rr.Xsym = rs;
    rr.Type = r.Type();
    rr.Size = r.Siz();
    return rr;

}

// relocSymState hold state information needed when making a series of
// successive calls to relocsym(). The items here are invariant
// (meaning that they are set up once initially and then don't change
// during the execution of relocsym), with the exception of a slice
// used to facilitate batch allocation of external relocations. Calls
// to relocsym happen in parallel; the assumption is that each
// parallel thread will have its own state object.
private partial struct relocSymState {
    public ptr<Target> target;
    public ptr<loader.Loader> ldr;
    public ptr<ErrorReporter> err;
    public ptr<ArchSyms> syms;
}

// makeRelocSymState creates a relocSymState container object to
// pass to relocsym(). If relocsym() calls happen in parallel,
// each parallel thread should have its own state object.
private static ptr<relocSymState> makeRelocSymState(this ptr<Link> _addr_ctxt) {
    ref Link ctxt = ref _addr_ctxt.val;

    return addr(new relocSymState(target:&ctxt.Target,ldr:ctxt.loader,err:&ctxt.ErrorReporter,syms:&ctxt.ArchSyms,));
}

private static void windynrelocsym(ptr<Link> _addr_ctxt, ptr<loader.SymbolBuilder> _addr_rel, loader.Sym s) {
    ref Link ctxt = ref _addr_ctxt.val;
    ref loader.SymbolBuilder rel = ref _addr_rel.val;

    ptr<loader.SymbolBuilder> su;
    var relocs = ctxt.loader.Relocs(s);
    for (nint ri = 0; ri < relocs.Count(); ri++) {
        var r = relocs.At(ri);
        if (r.IsMarker()) {
            continue; // skip marker relocations
        }
        var targ = r.Sym();
        if (targ == 0) {
            continue;
        }
        if (!ctxt.loader.AttrReachable(targ)) {
            if (r.Weak()) {
                continue;
            }
            ctxt.Errorf(s, "dynamic relocation to unreachable symbol %s", ctxt.loader.SymName(targ));
        }
        var tplt = ctxt.loader.SymPlt(targ);
        var tgot = ctxt.loader.SymGot(targ);
        if (tplt == -2 && tgot != -2) { // make dynimport JMP table for PE object files.
            tplt = int32(rel.Size());
            ctxt.loader.SetPlt(targ, tplt);

            if (su == null) {
                su = ctxt.loader.MakeSymbolUpdater(s);
            }

            r.SetSym(rel.Sym());
            r.SetAdd(int64(tplt)); 

            // jmp *addr

            if (ctxt.Arch.Family == sys.I386) 
                rel.AddUint8(0xff);
                rel.AddUint8(0x25);
                rel.AddAddrPlus(ctxt.Arch, targ, 0);
                rel.AddUint8(0x90);
                rel.AddUint8(0x90);
            else if (ctxt.Arch.Family == sys.AMD64) 
                rel.AddUint8(0xff);
                rel.AddUint8(0x24);
                rel.AddUint8(0x25);
                rel.AddAddrPlus4(ctxt.Arch, targ, 0);
                rel.AddUint8(0x90);
            else 
                ctxt.Errorf(s, "unsupported arch %v", ctxt.Arch.Family);
                return ;
            
        }
        else if (tplt >= 0) {
            if (su == null) {
                su = ctxt.loader.MakeSymbolUpdater(s);
            }
            r.SetSym(rel.Sym());
            r.SetAdd(int64(tplt));
        }
    }

}

// windynrelocsyms generates jump table to C library functions that will be
// added later. windynrelocsyms writes the table into .rel symbol.
private static void windynrelocsyms(this ptr<Link> _addr_ctxt) {
    ref Link ctxt = ref _addr_ctxt.val;

    if (!(ctxt.IsWindows() && iscgo && ctxt.IsInternal())) {
        return ;
    }
    var rel = ctxt.loader.CreateSymForUpdate(".rel", 0);
    rel.SetType(sym.STEXT);

    foreach (var (_, s) in ctxt.Textp) {
        windynrelocsym(_addr_ctxt, _addr_rel, s);
    }    ctxt.Textp = append(ctxt.Textp, rel.Sym());

}

private static void dynrelocsym(ptr<Link> _addr_ctxt, loader.Sym s) {
    ref Link ctxt = ref _addr_ctxt.val;

    var target = _addr_ctxt.Target;
    var ldr = ctxt.loader;
    var syms = _addr_ctxt.ArchSyms;
    var relocs = ldr.Relocs(s);
    for (nint ri = 0; ri < relocs.Count(); ri++) {
        var r = relocs.At(ri);
        if (r.IsMarker()) {
            continue; // skip marker relocations
        }
        var rSym = r.Sym();
        if (r.Weak() && !ldr.AttrReachable(rSym)) {
            continue;
        }
        if (ctxt.BuildMode == BuildModePIE && ctxt.LinkMode == LinkInternal) { 
            // It's expected that some relocations will be done
            // later by relocsym (R_TLS_LE, R_ADDROFF), so
            // don't worry if Adddynrel returns false.
            thearch.Adddynrel(target, ldr, syms, s, r, ri);
            continue;

        }
        if (rSym != 0 && ldr.SymType(rSym) == sym.SDYNIMPORT || r.Type() >= objabi.ElfRelocOffset) {
            if (rSym != 0 && !ldr.AttrReachable(rSym)) {
                ctxt.Errorf(s, "dynamic relocation to unreachable symbol %s", ldr.SymName(rSym));
            }
            if (!thearch.Adddynrel(target, ldr, syms, s, r, ri)) {
                ctxt.Errorf(s, "unsupported dynamic relocation for symbol %s (type=%d (%s) stype=%d (%s))", ldr.SymName(rSym), r.Type(), sym.RelocName(ctxt.Arch, r.Type()), ldr.SymType(rSym), ldr.SymType(rSym));
            }
        }
    }

}

private static void dynreloc(this ptr<dodataState> _addr_state, ptr<Link> _addr_ctxt) {
    ref dodataState state = ref _addr_state.val;
    ref Link ctxt = ref _addr_ctxt.val;

    if (ctxt.HeadType == objabi.Hwindows) {
        return ;
    }
    if (FlagD.val) {
        return ;
    }
    {
        var s__prev1 = s;

        foreach (var (_, __s) in ctxt.Textp) {
            s = __s;
            dynrelocsym(_addr_ctxt, s);
        }
        s = s__prev1;
    }

    foreach (var (_, syms) in state.data) {
        {
            var s__prev2 = s;

            foreach (var (_, __s) in syms) {
                s = __s;
                dynrelocsym(_addr_ctxt, s);
            }

            s = s__prev2;
        }
    }    if (ctxt.IsELF) {
        elfdynhash(ctxt);
    }
}

public static void CodeblkPad(ptr<Link> _addr_ctxt, ptr<OutBuf> _addr_@out, long addr, long size, slice<byte> pad) {
    ref Link ctxt = ref _addr_ctxt.val;
    ref OutBuf @out = ref _addr_@out.val;

    writeBlocks(_addr_ctxt, _addr_out, ctxt.outSem, _addr_ctxt.loader, ctxt.Textp, addr, size, pad);
}

private static readonly nint blockSize = 1 << 20; // 1MB chunks written at a time.

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
private static void writeBlocks(ptr<Link> _addr_ctxt, ptr<OutBuf> _addr_@out, channel<nint> sem, ptr<loader.Loader> _addr_ldr, slice<loader.Sym> syms, long addr, long size, slice<byte> pad) {
    ref Link ctxt = ref _addr_ctxt.val;
    ref OutBuf @out = ref _addr_@out.val;
    ref loader.Loader ldr = ref _addr_ldr.val;

    {
        var i__prev1 = i;
        var s__prev1 = s;

        foreach (var (__i, __s) in syms) {
            i = __i;
            s = __s;
            if (ldr.SymValue(s) >= addr && !ldr.AttrSubSymbol(s)) {
                syms = syms[(int)i..];
                break;
            }
        }
        i = i__prev1;
        s = s__prev1;
    }

    sync.WaitGroup wg = default;
    var max = int64(blockSize);
    var lastAddr = addr + size;
    var written = int64(0);
    while (addr < lastAddr) { 
        // Find the last symbol we'd write.
        nint idx = -1;
        {
            var i__prev2 = i;
            var s__prev2 = s;

            foreach (var (__i, __s) in syms) {
                i = __i;
                s = __s;
                if (ldr.AttrSubSymbol(s)) {
                    continue;
                } 

                // If the next symbol's size would put us out of bounds on the total length,
                // stop looking.
                var end = ldr.SymValue(s) + ldr.SymSize(s);
                if (end > lastAddr) {
                    break;
                } 

                // We're gonna write this symbol.
                idx = i; 

                // If we cross over the max size, we've got enough symbols.
                if (end > addr + max) {
                    break;
                }

            } 

            // If we didn't find any symbols to write, we're done here.

            i = i__prev2;
            s = s__prev2;
        }

        if (idx < 0) {
            break;
        }
        var length = int64(0);
        if (idx + 1 < len(syms)) { 
            // Find the next top-level symbol.
            // Skip over sub symbols so we won't split a containter symbol
            // into two blocks.
            var next = syms[idx + 1];
            while (ldr.AttrSubSymbol(next)) {
                idx++;
                next = syms[idx + 1];
            }

            length = ldr.SymValue(next) - addr;

        }
        if (length == 0 || length > lastAddr - addr) {
            length = lastAddr - addr;
        }
        {
            var (o, err) = @out.View(uint64(@out.Offset() + written));

            if (err == null) {
                sem.Send(1);
                wg.Add(1);
                go_(() => (o, ldr, syms, addr, size, pad) => {
                    writeBlock(_addr_ctxt, _addr_o, _addr_ldr, syms, addr, size, pad);
                    wg.Done().Send(sem);
                }
            else
(o, ldr, syms, addr, length, pad));

            } { // output not mmaped, don't parallelize.
                writeBlock(_addr_ctxt, _addr_out, _addr_ldr, syms, addr, length, pad);

            } 

            // Prepare for the next loop.

        } 

        // Prepare for the next loop.
        if (idx != -1) {
            syms = syms[(int)idx + 1..];
        }
        written += length;
        addr += length;

    }
    wg.Wait();

}

private static void writeBlock(ptr<Link> _addr_ctxt, ptr<OutBuf> _addr_@out, ptr<loader.Loader> _addr_ldr, slice<loader.Sym> syms, long addr, long size, slice<byte> pad) {
    ref Link ctxt = ref _addr_ctxt.val;
    ref OutBuf @out = ref _addr_@out.val;
    ref loader.Loader ldr = ref _addr_ldr.val;

    var st = ctxt.makeRelocSymState(); 

    // This doesn't distinguish the memory size from the file
    // size, and it lays out the file based on Symbol.Value, which
    // is the virtual address. DWARF compression changes file sizes,
    // so dwarfcompress will fix this up later if necessary.
    var eaddr = addr + size;
    foreach (var (_, s) in syms) {
        if (ldr.AttrSubSymbol(s)) {
            continue;
        }
        var val = ldr.SymValue(s);
        if (val >= eaddr) {
            break;
        }
        if (val < addr) {
            ldr.Errorf(s, "phase error: addr=%#x but sym=%#x type=%v sect=%v", addr, val, ldr.SymType(s), ldr.SymSect(s).Name);
            errorexit();
        }
        if (addr < val) {
            @out.WriteStringPad("", int(val - addr), pad);
            addr = val;
        }
        var P = @out.WriteSym(ldr, s);
        st.relocsym(s, P);
        {
            var (f, ok) = ctxt.generatorSyms[s];

            if (ok) {
                f(ctxt, s);
            }

        }

        addr += int64(len(P));
        var siz = ldr.SymSize(s);
        if (addr < val + siz) {
            @out.WriteStringPad("", int(val + siz - addr), pad);
            addr = val + siz;
        }
        if (addr != val + siz) {
            ldr.Errorf(s, "phase error: addr=%#x value+size=%#x", addr, val + siz);
            errorexit();
        }
        if (val + siz >= eaddr) {
            break;
        }
    }    if (addr < eaddr) {
        @out.WriteStringPad("", int(eaddr - addr), pad);
    }
}

public delegate void writeFn(ptr<Link>, ptr<OutBuf>, long, long);

// writeParallel handles scheduling parallel execution of data write functions.
private static void writeParallel(ptr<sync.WaitGroup> _addr_wg, writeFn fn, ptr<Link> _addr_ctxt, ulong seek, ulong vaddr, ulong length) => func((defer, _, _) => {
    ref sync.WaitGroup wg = ref _addr_wg.val;
    ref Link ctxt = ref _addr_ctxt.val;

    {
        var (out, err) = ctxt.Out.View(seek);

        if (err != null) {
            ctxt.Out.SeekSet(int64(seek));
            fn(ctxt, ctxt.Out, int64(vaddr), int64(length));
        }
        else
 {
            wg.Add(1);
            go_(() => () => {
                defer(wg.Done());
                fn(ctxt, out, int64(vaddr), int64(length));
            }());
        }
    }

});

private static void datblk(ptr<Link> _addr_ctxt, ptr<OutBuf> _addr_@out, long addr, long size) {
    ref Link ctxt = ref _addr_ctxt.val;
    ref OutBuf @out = ref _addr_@out.val;

    writeDatblkToOutBuf(_addr_ctxt, _addr_out, addr, size);
}

// Used only on Wasm for now.
public static slice<byte> DatblkBytes(ptr<Link> _addr_ctxt, long addr, long size) {
    ref Link ctxt = ref _addr_ctxt.val;

    var buf = make_slice<byte>(size);
    ptr<OutBuf> @out = addr(new OutBuf(heap:buf));
    writeDatblkToOutBuf(_addr_ctxt, out, addr, size);
    return buf;
}

private static void writeDatblkToOutBuf(ptr<Link> _addr_ctxt, ptr<OutBuf> _addr_@out, long addr, long size) {
    ref Link ctxt = ref _addr_ctxt.val;
    ref OutBuf @out = ref _addr_@out.val;

    writeBlocks(_addr_ctxt, _addr_out, ctxt.outSem, _addr_ctxt.loader, ctxt.datap, addr, size, zeros[..]);
}

private static void dwarfblk(ptr<Link> _addr_ctxt, ptr<OutBuf> _addr_@out, long addr, long size) {
    ref Link ctxt = ref _addr_ctxt.val;
    ref OutBuf @out = ref _addr_@out.val;
 
    // Concatenate the section symbol lists into a single list to pass
    // to writeBlocks.
    //
    // NB: ideally we would do a separate writeBlocks call for each
    // section, but this would run the risk of undoing any file offset
    // adjustments made during layout.
    nint n = 0;
    {
        var i__prev1 = i;

        foreach (var (__i) in dwarfp) {
            i = __i;
            n += len(dwarfp[i].syms);
        }
        i = i__prev1;
    }

    var syms = make_slice<loader.Sym>(0, n);
    {
        var i__prev1 = i;

        foreach (var (__i) in dwarfp) {
            i = __i;
            syms = append(syms, dwarfp[i].syms);
        }
        i = i__prev1;
    }

    writeBlocks(_addr_ctxt, _addr_out, ctxt.outSem, _addr_ctxt.loader, syms, addr, size, zeros[..]);

}

private static array<byte> zeros = new array<byte>(512);

private static var strdata = make_map<@string, @string>();private static slice<@string> strnames = default;

private static void addstrdata1(ptr<Link> _addr_ctxt, @string arg) {
    ref Link ctxt = ref _addr_ctxt.val;

    var eq = strings.Index(arg, "=");
    var dot = strings.LastIndex(arg[..(int)eq + 1], ".");
    if (eq < 0 || dot < 0) {
        Exitf("-X flag requires argument of the form importpath.name=value");
    }
    var pkg = arg[..(int)dot];
    if (ctxt.BuildMode == BuildModePlugin && pkg == "main") {
        pkg = flagPluginPath.val;
    }
    pkg = objabi.PathToPrefix(pkg);
    var name = pkg + arg[(int)dot..(int)eq];
    var value = arg[(int)eq + 1..];
    {
        var (_, ok) = strdata[name];

        if (!ok) {
            strnames = append(strnames, name);
        }
    }

    strdata[name] = value;

}

// addstrdata sets the initial value of the string variable name to value.
private static void addstrdata(ptr<sys.Arch> _addr_arch, ptr<loader.Loader> _addr_l, @string name, @string value) {
    ref sys.Arch arch = ref _addr_arch.val;
    ref loader.Loader l = ref _addr_l.val;

    var s = l.Lookup(name, 0);
    if (s == 0) {
        return ;
    }
    {
        var goType = l.SymGoType(s);

        if (goType == 0) {
            return ;
        }        {
            var typeName = l.SymName(goType);


            else if (typeName != "type.string") {
                Errorf(null, "%s: cannot set with -X: not a var of type string (%s)", name, typeName);
                return ;
            }

        }


    }

    if (!l.AttrReachable(s)) {
        return ; // don't bother setting unreachable variable
    }
    var bld = l.MakeSymbolUpdater(s);
    if (bld.Type() == sym.SBSS) {
        bld.SetType(sym.SDATA);
    }
    var p = fmt.Sprintf("%s.str", name);
    var sbld = l.CreateSymForUpdate(p, 0);
    sbld.Addstring(value);
    sbld.SetType(sym.SRODATA);

    bld.SetSize(0);
    bld.SetData(make_slice<byte>(0, arch.PtrSize * 2));
    bld.SetReadOnly(false);
    bld.ResetRelocs();
    bld.AddAddrPlus(arch, sbld.Sym(), 0);
    bld.AddUint(arch, uint64(len(value)));

}

private static void dostrdata(this ptr<Link> _addr_ctxt) {
    ref Link ctxt = ref _addr_ctxt.val;

    foreach (var (_, name) in strnames) {
        addstrdata(_addr_ctxt.Arch, _addr_ctxt.loader, name, strdata[name]);
    }
}

// addgostring adds str, as a Go string value, to s. symname is the name of the
// symbol used to define the string data and must be unique per linked object.
private static void addgostring(ptr<Link> _addr_ctxt, ptr<loader.Loader> _addr_ldr, ptr<loader.SymbolBuilder> _addr_s, @string symname, @string str) {
    ref Link ctxt = ref _addr_ctxt.val;
    ref loader.Loader ldr = ref _addr_ldr.val;
    ref loader.SymbolBuilder s = ref _addr_s.val;

    var sdata = ldr.CreateSymForUpdate(symname, 0);
    if (sdata.Type() != sym.Sxxx) {
        ctxt.Errorf(s.Sym(), "duplicate symname in addgostring: %s", symname);
    }
    sdata.SetLocal(true);
    sdata.SetType(sym.SRODATA);
    sdata.SetSize(int64(len(str)));
    sdata.SetData((slice<byte>)str);
    s.AddAddr(ctxt.Arch, sdata.Sym());
    s.AddUint(ctxt.Arch, uint64(len(str)));

}

private static void addinitarrdata(ptr<Link> _addr_ctxt, ptr<loader.Loader> _addr_ldr, loader.Sym s) {
    ref Link ctxt = ref _addr_ctxt.val;
    ref loader.Loader ldr = ref _addr_ldr.val;

    var p = ldr.SymName(s) + ".ptr";
    var sp = ldr.CreateSymForUpdate(p, 0);
    sp.SetType(sym.SINITARR);
    sp.SetSize(0);
    sp.SetDuplicateOK(true);
    sp.AddAddr(ctxt.Arch, s);
}

// symalign returns the required alignment for the given symbol s.
private static int symalign(ptr<loader.Loader> _addr_ldr, loader.Sym s) {
    ref loader.Loader ldr = ref _addr_ldr.val;

    var min = int32(thearch.Minalign);
    var align = ldr.SymAlign(s);
    if (align >= min) {
        return align;
    }
    else if (align != 0) {
        return min;
    }
    var sname = ldr.SymName(s);
    if (strings.HasPrefix(sname, "go.string.") || strings.HasPrefix(sname, "type..namedata.")) { 
        // String data is just bytes.
        // If we align it, we waste a lot of space to padding.
        return min;

    }
    align = int32(thearch.Maxalign);
    var ssz = ldr.SymSize(s);
    while (int64(align) > ssz && align > min) {
        align>>=1;
    }
    ldr.SetSymAlign(s, align);
    return align;

}

private static long aligndatsize(ptr<dodataState> _addr_state, long datsize, loader.Sym s) {
    ref dodataState state = ref _addr_state.val;

    return Rnd(datsize, int64(symalign(_addr_state.ctxt.loader, s)));
}

private static readonly var debugGCProg = false;



public partial struct GCProg {
    public ptr<Link> ctxt;
    public ptr<loader.SymbolBuilder> sym;
    public gcprog.Writer w;
}

private static void Init(this ptr<GCProg> _addr_p, ptr<Link> _addr_ctxt, @string name) {
    ref GCProg p = ref _addr_p.val;
    ref Link ctxt = ref _addr_ctxt.val;

    p.ctxt = ctxt;
    p.sym = ctxt.loader.CreateSymForUpdate(name, 0);
    p.w.Init(p.writeByte());
    if (debugGCProg) {
        fmt.Fprintf(os.Stderr, "ld: start GCProg %s\n", name);
        p.w.Debug(os.Stderr);
    }
}

private static Action<byte> writeByte(this ptr<GCProg> _addr_p) {
    ref GCProg p = ref _addr_p.val;

    return x => {
        p.sym.AddUint8(x);
    };
}

private static void End(this ptr<GCProg> _addr_p, long size) {
    ref GCProg p = ref _addr_p.val;

    p.w.ZeroUntil(size / int64(p.ctxt.Arch.PtrSize));
    p.w.End();
    if (debugGCProg) {
        fmt.Fprintf(os.Stderr, "ld: end GCProg\n");
    }
}

private static void AddSym(this ptr<GCProg> _addr_p, loader.Sym s) {
    ref GCProg p = ref _addr_p.val;

    var ldr = p.ctxt.loader;
    var typ = ldr.SymGoType(s); 

    // Things without pointers should be in sym.SNOPTRDATA or sym.SNOPTRBSS;
    // everything we see should have pointers and should therefore have a type.
    if (typ == 0) {
        switch (ldr.SymName(s)) {
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

    if (debugGCProg) {
        fmt.Fprintf(os.Stderr, "gcprog sym: %s at %d (ptr=%d+%d)\n", ldr.SymName(s), ldr.SymValue(s), ldr.SymValue(s) / ptrsize, nptr);
    }
    var sval = ldr.SymValue(s);
    if (decodetypeUsegcprog(p.ctxt.Arch, typData) == 0) { 
        // Copy pointers from mask into program.
        var mask = decodetypeGcmask(p.ctxt, typ);
        for (var i = int64(0); i < nptr; i++) {
            if ((mask[i / 8] >> (int)(uint(i % 8))) & 1 != 0) {
                p.w.Ptr(sval / ptrsize + i);
            }
        }
        return ;

    }
    var prog = decodetypeGcprog(p.ctxt, typ);
    p.w.ZeroUntil(sval / ptrsize);
    p.w.Append(prog[(int)4..], nptr);

}

// cutoff is the maximum data section size permitted by the linker
// (see issue #9862).
private static readonly float cutoff = 2e9F; // 2 GB (or so; looks better in errors than 2^31)

 // 2 GB (or so; looks better in errors than 2^31)

private static void checkdatsize(this ptr<dodataState> _addr_state, sym.SymKind symn) {
    ref dodataState state = ref _addr_state.val;

    if (state.datsize > cutoff) {
        Errorf(null, "too much data in section %v (over %v bytes)", symn, cutoff);
    }
}

// fixZeroSizedSymbols gives a few special symbols with zero size some space.
private static void fixZeroSizedSymbols(ptr<Link> _addr_ctxt) {
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
    if (!(ctxt.DynlinkingGo() && ctxt.HeadType == objabi.Hdarwin) && !(ctxt.HeadType == objabi.Haix && ctxt.LinkMode == LinkExternal)) {
        return ;
    }
    var ldr = ctxt.loader;
    var bss = ldr.CreateSymForUpdate("runtime.bss", 0);
    bss.SetSize(8);
    ldr.SetAttrSpecial(bss.Sym(), false);

    var ebss = ldr.CreateSymForUpdate("runtime.ebss", 0);
    ldr.SetAttrSpecial(ebss.Sym(), false);

    var data = ldr.CreateSymForUpdate("runtime.data", 0);
    data.SetSize(8);
    ldr.SetAttrSpecial(data.Sym(), false);

    var edata = ldr.CreateSymForUpdate("runtime.edata", 0);
    ldr.SetAttrSpecial(edata.Sym(), false);

    if (ctxt.HeadType == objabi.Haix) { 
        // XCOFFTOC symbols are part of .data section.
        edata.SetType(sym.SXCOFFTOC);

    }
    var types = ldr.CreateSymForUpdate("runtime.types", 0);
    types.SetType(sym.STYPE);
    types.SetSize(8);
    ldr.SetAttrSpecial(types.Sym(), false);

    var etypes = ldr.CreateSymForUpdate("runtime.etypes", 0);
    etypes.SetType(sym.SFUNCTAB);
    ldr.SetAttrSpecial(etypes.Sym(), false);

    if (ctxt.HeadType == objabi.Haix) {
        var rodata = ldr.CreateSymForUpdate("runtime.rodata", 0);
        rodata.SetType(sym.SSTRING);
        rodata.SetSize(8);
        ldr.SetAttrSpecial(rodata.Sym(), false);

        var erodata = ldr.CreateSymForUpdate("runtime.erodata", 0);
        ldr.SetAttrSpecial(erodata.Sym(), false);
    }
}

// makeRelroForSharedLib creates a section of readonly data if necessary.
private static void makeRelroForSharedLib(this ptr<dodataState> _addr_state, ptr<Link> _addr_target) {
    ref dodataState state = ref _addr_state.val;
    ref Link target = ref _addr_target.val;

    if (!target.UseRelro()) {
        return ;
    }
    var ldr = target.loader;
    foreach (var (_, symnro) in sym.ReadOnly) {
        var symnrelro = sym.RelROMap[symnro];

        loader.Sym ro = new slice<loader.Sym>(new loader.Sym[] {  });
        var relro = state.data[symnrelro];

        {
            var s__prev2 = s;

            foreach (var (_, __s) in state.data[symnro]) {
                s = __s;
                var relocs = ldr.Relocs(s);
                var isRelro = relocs.Count() > 0;

                if (state.symType(s) == sym.STYPE || state.symType(s) == sym.STYPERELRO || state.symType(s) == sym.SGOFUNCRELRO) 
                    // Symbols are not sorted yet, so it is possible
                    // that an Outer symbol has been changed to a
                    // relro Type before it reaches here.
                    isRelro = true;
                else if (state.symType(s) == sym.SFUNCTAB) 
                    if (ldr.SymName(s) == "runtime.etypes") { 
                        // runtime.etypes must be at the end of
                        // the relro data.
                        isRelro = true;

                    }

                                if (isRelro) {
                    state.setSymType(s, symnrelro);
                    {
                        var outer__prev2 = outer;

                        var outer = ldr.OuterSym(s);

                        if (outer != 0) {
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

            foreach (var (_, __s) in relro) {
                s = __s;
                {
                    var outer__prev1 = outer;

                    outer = ldr.OuterSym(s);

                    if (outer != 0) {
                        var st = state.symType(s);
                        var ost = state.symType(outer);
                        if (st != ost) {
                            state.ctxt.Errorf(s, "inconsistent types for symbol and its Outer %s (%v != %v)", ldr.SymName(outer), st, ost);
                        }
                    }

                    outer = outer__prev1;

                }

            }

            s = s__prev2;
        }

        state.data[symnro] = ro;
        state.data[symnrelro] = relro;

    }
}

// dodataState holds bits of state information needed by dodata() and the
// various helpers it calls. The lifetime of these items should not extend
// past the end of dodata().
private partial struct dodataState {
    public ptr<Link> ctxt; // Data symbols bucketed by type.
    public array<slice<loader.Sym>> data; // Max alignment for each flavor of data symbol.
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
private static sym.SymKind symType(this ptr<dodataState> _addr_state, loader.Sym s) {
    ref dodataState state = ref _addr_state.val;

    if (int(s) < len(state.symGroupType)) {
        {
            var @override = state.symGroupType[s];

            if (override != 0) {
                return override;
            }

        }

    }
    return state.ctxt.loader.SymType(s);

}

// setSymType sets a new override type for 's'.
private static void setSymType(this ptr<dodataState> _addr_state, loader.Sym s, sym.SymKind kind) => func((_, panic, _) => {
    ref dodataState state = ref _addr_state.val;

    if (s == 0) {
        panic("bad");
    }
    if (int(s) < len(state.symGroupType)) {
        state.symGroupType[s] = kind;
    }
    else
 {
        var su = state.ctxt.loader.MakeSymbolUpdater(s);
        su.SetType(kind);
    }
});

private static void dodata(this ptr<Link> _addr_ctxt, slice<sym.SymKind> symGroupType) {
    ref Link ctxt = ref _addr_ctxt.val;

    // Give zeros sized symbols space if necessary.
    fixZeroSizedSymbols(_addr_ctxt); 

    // Collect data symbols by type into data.
    dodataState state = new dodataState(ctxt:ctxt,symGroupType:symGroupType);
    var ldr = ctxt.loader;
    {
        var s__prev1 = s;

        for (var s = loader.Sym(1); s < loader.Sym(ldr.NSym()); s++) {
            if (!ldr.AttrReachable(s) || ldr.AttrSpecial(s) || ldr.AttrSubSymbol(s) || !ldr.TopLevelSym(s)) {
                continue;
            }
            var st = state.symType(s);

            if (st <= sym.STEXT || st >= sym.SXREF) {
                continue;
            }
            state.data[st] = append(state.data[st], s); 

            // Similarly with checking the onlist attr.
            if (ldr.AttrOnList(s)) {
                log.Fatalf("symbol %s listed multiple times", ldr.SymName(s));
            }

            ldr.SetAttrOnList(s, true);

        }

        s = s__prev1;
    } 

    // Now that we have the data symbols, but before we start
    // to assign addresses, record all the necessary
    // dynamic relocations. These will grow the relocation
    // symbol, which is itself data.
    //
    // On darwin, we need the symbol table numbers for dynreloc.
    if (ctxt.HeadType == objabi.Hdarwin) {
        machosymorder(ctxt);
    }
    state.dynreloc(ctxt); 

    // Move any RO data with relocations to a separate section.
    state.makeRelroForSharedLib(ctxt); 

    // Set alignment for the symbol with the largest known index,
    // so as to trigger allocation of the loader's internal
    // alignment array. This will avoid data races in the parallel
    // section below.
    var lastSym = loader.Sym(ldr.NSym() - 1);
    ldr.SetSymAlign(lastSym, ldr.SymAlign(lastSym)); 

    // Sort symbols.
    sync.WaitGroup wg = default;
    {
        var symn__prev1 = symn;

        foreach (var (__symn) in state.data) {
            symn = __symn;
            var symn = sym.SymKind(symn);
            wg.Add(1);
            go_(() => () => {
                state.data[symn], state.dataMaxAlign[symn] = state.dodataSect(ctxt, symn, state.data[symn]);
                wg.Done();
            }());
        }
        symn = symn__prev1;
    }

    wg.Wait();

    if (ctxt.IsELF) { 
        // Make .rela and .rela.plt contiguous, the ELF ABI requires this
        // and Solaris actually cares.
        var syms = state.data[sym.SELFROSECT];
        nint reli = -1;
        nint plti = -1;
        {
            var s__prev1 = s;

            foreach (var (__i, __s) in syms) {
                i = __i;
                s = __s;
                switch (ldr.SymName(s)) {
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

        if (reli >= 0 && plti >= 0 && plti != reli + 1) {
            nint first = default;            nint second = default;

            if (plti > reli) {
                (first, second) = (reli, plti);
            }
            else
 {
                (first, second) = (plti, reli);
            }

            var rel = syms[reli];
            var plt = syms[plti];
            copy(syms[(int)first + 2..], syms[(int)first + 1..(int)second]);
            syms[first + 0] = rel;
            syms[first + 1] = plt; 

            // Make sure alignment doesn't introduce a gap.
            // Setting the alignment explicitly prevents
            // symalign from basing it on the size and
            // getting it wrong.
            ldr.SetSymAlign(rel, int32(ctxt.Arch.RegSize));
            ldr.SetSymAlign(plt, int32(ctxt.Arch.RegSize));

        }
        state.data[sym.SELFROSECT] = syms;

    }
    if (ctxt.HeadType == objabi.Haix && ctxt.LinkMode == LinkExternal) { 
        // These symbols must have the same alignment as their section.
        // Otherwise, ld might change the layout of Go sections.
        ldr.SetSymAlign(ldr.Lookup("runtime.data", 0), state.dataMaxAlign[sym.SDATA]);
        ldr.SetSymAlign(ldr.Lookup("runtime.bss", 0), state.dataMaxAlign[sym.SBSS]);

    }
    state.allocateDataSections(ctxt); 

    // Create *sym.Section objects and assign symbols to sections for
    // DWARF symbols.
    state.allocateDwarfSections(ctxt); 

    /* number the sections */
    var n = int16(1);

    {
        var sect__prev1 = sect;

        foreach (var (_, __sect) in Segtext.Sections) {
            sect = __sect;
            sect.Extnum = n;
            n++;
        }
        sect = sect__prev1;
    }

    {
        var sect__prev1 = sect;

        foreach (var (_, __sect) in Segrodata.Sections) {
            sect = __sect;
            sect.Extnum = n;
            n++;
        }
        sect = sect__prev1;
    }

    {
        var sect__prev1 = sect;

        foreach (var (_, __sect) in Segrelrodata.Sections) {
            sect = __sect;
            sect.Extnum = n;
            n++;
        }
        sect = sect__prev1;
    }

    {
        var sect__prev1 = sect;

        foreach (var (_, __sect) in Segdata.Sections) {
            sect = __sect;
            sect.Extnum = n;
            n++;
        }
        sect = sect__prev1;
    }

    {
        var sect__prev1 = sect;

        foreach (var (_, __sect) in Segdwarf.Sections) {
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
private static ptr<sym.Section> allocateDataSectionForSym(this ptr<dodataState> _addr_state, ptr<sym.Segment> _addr_seg, loader.Sym s, nint rwx) {
    ref dodataState state = ref _addr_state.val;
    ref sym.Segment seg = ref _addr_seg.val;

    var ldr = state.ctxt.loader;
    var sname = ldr.SymName(s);
    var sect = addsection(ldr, state.ctxt.Arch, seg, sname, rwx);
    sect.Align = symalign(_addr_ldr, s);
    state.datsize = Rnd(state.datsize, int64(sect.Align));
    sect.Vaddr = uint64(state.datsize);
    return _addr_sect!;
}

// allocateNamedDataSection creates a new sym.Section for a category
// of data symbols. Here "seg" is the segment into which the section
// will go, "sName" is the name to give to the section, "types" is a
// range of symbol types to be put into the section, and "rwx"
// contains permissions for the section.
private static ptr<sym.Section> allocateNamedDataSection(this ptr<dodataState> _addr_state, ptr<sym.Segment> _addr_seg, @string sName, slice<sym.SymKind> types, nint rwx) {
    ref dodataState state = ref _addr_state.val;
    ref sym.Segment seg = ref _addr_seg.val;

    var sect = addsection(state.ctxt.loader, state.ctxt.Arch, seg, sName, rwx);
    if (len(types) == 0) {
        sect.Align = 1;
    }
    else if (len(types) == 1) {
        sect.Align = state.dataMaxAlign[types[0]];
    }
    else
 {
        foreach (var (_, symn) in types) {
            var align = state.dataMaxAlign[symn];
            if (sect.Align < align) {
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
private static long assignDsymsToSection(this ptr<dodataState> _addr_state, ptr<sym.Section> _addr_sect, slice<loader.Sym> syms, sym.SymKind forceType, Func<ptr<dodataState>, long, loader.Sym, long> aligner) {
    ref dodataState state = ref _addr_state.val;
    ref sym.Section sect = ref _addr_sect.val;

    var ldr = state.ctxt.loader;
    foreach (var (_, s) in syms) {
        state.datsize = aligner(state, state.datsize, s);
        ldr.SetSymSect(s, sect);
        if (forceType != sym.Sxxx) {
            state.setSymType(s, forceType);
        }
        ldr.SetSymValue(s, int64(uint64(state.datsize) - sect.Vaddr));
        state.datsize += ldr.SymSize(s);

    }    sect.Length = uint64(state.datsize) - sect.Vaddr;

}

private static void assignToSection(this ptr<dodataState> _addr_state, ptr<sym.Section> _addr_sect, sym.SymKind symn, sym.SymKind forceType) {
    ref dodataState state = ref _addr_state.val;
    ref sym.Section sect = ref _addr_sect.val;

    state.assignDsymsToSection(sect, state.data[symn], forceType, aligndatsize);
    state.checkdatsize(symn);
}

// allocateSingleSymSections walks through the bucketed data symbols
// with type 'symn', creates a new section for each sym, and assigns
// the sym to a newly created section. Section name is set from the
// symbol name. "Seg" is the segment into which to place the new
// section, "forceType" is the new sym.SymKind to assign to the symbol
// within the section, and "rwx" holds section permissions.
private static void allocateSingleSymSections(this ptr<dodataState> _addr_state, ptr<sym.Segment> _addr_seg, sym.SymKind symn, sym.SymKind forceType, nint rwx) {
    ref dodataState state = ref _addr_state.val;
    ref sym.Segment seg = ref _addr_seg.val;

    var ldr = state.ctxt.loader;
    foreach (var (_, s) in state.data[symn]) {
        var sect = state.allocateDataSectionForSym(seg, s, rwx);
        ldr.SetSymSect(s, sect);
        state.setSymType(s, forceType);
        ldr.SetSymValue(s, int64(uint64(state.datsize) - sect.Vaddr));
        state.datsize += ldr.SymSize(s);
        sect.Length = uint64(state.datsize) - sect.Vaddr;
    }    state.checkdatsize(symn);
}

// allocateNamedSectionAndAssignSyms creates a new section with the
// specified name, then walks through the bucketed data symbols with
// type 'symn' and assigns each of them to this new section. "Seg" is
// the segment into which to place the new section, "secName" is the
// name to give to the new section, "forceType" (if non-zero) contains
// a new sym type to apply to each sym during the assignment, and
// "rwx" holds section permissions.
private static ptr<sym.Section> allocateNamedSectionAndAssignSyms(this ptr<dodataState> _addr_state, ptr<sym.Segment> _addr_seg, @string secName, sym.SymKind symn, sym.SymKind forceType, nint rwx) {
    ref dodataState state = ref _addr_state.val;
    ref sym.Segment seg = ref _addr_seg.val;

    var sect = state.allocateNamedDataSection(seg, secName, new slice<sym.SymKind>(new sym.SymKind[] { symn }), rwx);
    state.assignDsymsToSection(sect, state.data[symn], forceType, aligndatsize);
    return _addr_sect!;
}

// allocateDataSections allocates sym.Section objects for data/rodata
// (and related) symbols, and then assigns symbols to those sections.
private static void allocateDataSections(this ptr<dodataState> _addr_state, ptr<Link> _addr_ctxt) {
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

        foreach (var (_, __symn) in writable) {
            symn = __symn;
            state.allocateSingleSymSections(_addr_Segdata, symn, sym.SDATA, 06);
        }
        symn = symn__prev1;
    }

    var ldr = ctxt.loader; 

    // .got (and .toc on ppc64)
    if (len(state.data[sym.SELFGOT]) > 0) {
        var sect = state.allocateNamedSectionAndAssignSyms(_addr_Segdata, ".got", sym.SELFGOT, sym.SDATA, 06);
        if (ctxt.IsPPC64()) {
            {
                var s__prev1 = s;

                foreach (var (_, __s) in state.data[sym.SELFGOT]) {
                    s = __s; 
                    // Resolve .TOC. symbol for this object file (ppc64)

                    var toc = ldr.Lookup(".TOC.", int(ldr.SymVersion(s)));
                    if (toc != 0) {
                        ldr.SetSymSect(toc, sect);
                        ldr.AddInteriorSym(s, toc);
                        ldr.SetSymValue(toc, 0x8000);
                    }

                }

                s = s__prev1;
            }
        }
    }
    sect = state.allocateNamedSectionAndAssignSyms(_addr_Segdata, ".noptrdata", sym.SNOPTRDATA, sym.SDATA, 06);
    ldr.SetSymSect(ldr.LookupOrCreateSym("runtime.noptrdata", 0), sect);
    ldr.SetSymSect(ldr.LookupOrCreateSym("runtime.enoptrdata", 0), sect);

    var hasinitarr = ctxt.linkShared; 

    /* shared library initializer */

    if (ctxt.BuildMode == BuildModeCArchive || ctxt.BuildMode == BuildModeCShared || ctxt.BuildMode == BuildModeShared || ctxt.BuildMode == BuildModePlugin) 
        hasinitarr = true;
        if (ctxt.HeadType == objabi.Haix) {
        if (len(state.data[sym.SINITARR]) > 0) {
            Errorf(null, "XCOFF format doesn't allow .init_array section");
        }
    }
    if (hasinitarr && len(state.data[sym.SINITARR]) > 0) {
        state.allocateNamedSectionAndAssignSyms(_addr_Segdata, ".init_array", sym.SINITARR, sym.Sxxx, 06);
    }
    sect = state.allocateNamedSectionAndAssignSyms(_addr_Segdata, ".data", sym.SDATA, sym.SDATA, 06);
    ldr.SetSymSect(ldr.LookupOrCreateSym("runtime.data", 0), sect);
    ldr.SetSymSect(ldr.LookupOrCreateSym("runtime.edata", 0), sect);
    var dataGcEnd = state.datsize - int64(sect.Vaddr); 

    // On AIX, TOC entries must be the last of .data
    // These aren't part of gc as they won't change during the runtime.
    state.assignToSection(sect, sym.SXCOFFTOC, sym.SDATA);
    state.checkdatsize(sym.SDATA);
    sect.Length = uint64(state.datsize) - sect.Vaddr; 

    /* bss */
    sect = state.allocateNamedSectionAndAssignSyms(_addr_Segdata, ".bss", sym.SBSS, sym.Sxxx, 06);
    ldr.SetSymSect(ldr.LookupOrCreateSym("runtime.bss", 0), sect);
    ldr.SetSymSect(ldr.LookupOrCreateSym("runtime.ebss", 0), sect);
    var bssGcEnd = state.datsize - int64(sect.Vaddr); 

    // Emit gcdata for bss symbols now that symbol values have been assigned.
    foreach (var (_, g) in gcsToEmit) {
        GCProg gc = default;
        gc.Init(ctxt, g.symName);
        {
            var s__prev2 = s;

            foreach (var (_, __s) in state.data[g.symKind]) {
                s = __s;
                gc.AddSym(s);
            }

            s = s__prev2;
        }

        gc.End(g.gcEnd);

    }    sect = state.allocateNamedSectionAndAssignSyms(_addr_Segdata, ".noptrbss", sym.SNOPTRBSS, sym.Sxxx, 06);
    ldr.SetSymSect(ldr.LookupOrCreateSym("runtime.noptrbss", 0), sect);
    ldr.SetSymSect(ldr.LookupOrCreateSym("runtime.enoptrbss", 0), sect);
    ldr.SetSymSect(ldr.LookupOrCreateSym("runtime.end", 0), sect); 

    // Coverage instrumentation counters for libfuzzer.
    if (len(state.data[sym.SLIBFUZZER_EXTRA_COUNTER]) > 0) {
        state.allocateNamedSectionAndAssignSyms(_addr_Segdata, "__libfuzzer_extra_counters", sym.SLIBFUZZER_EXTRA_COUNTER, sym.Sxxx, 06);
    }
    if (len(state.data[sym.STLSBSS]) > 0) {
        sect = ; 
        // FIXME: not clear why it is sometimes necessary to suppress .tbss section creation.
        if ((ctxt.IsELF || ctxt.HeadType == objabi.Haix) && (ctxt.LinkMode == LinkExternal || !FlagD.val)) {
            sect = addsection(ldr, ctxt.Arch, _addr_Segdata, ".tbss", 06);
            sect.Align = int32(ctxt.Arch.PtrSize); 
            // FIXME: why does this need to be set to zero?
            sect.Vaddr = 0;

        }
        state.datsize = 0;

        {
            var s__prev1 = s;

            foreach (var (_, __s) in state.data[sym.STLSBSS]) {
                s = __s;
                state.datsize = aligndatsize(_addr_state, state.datsize, s);
                if (sect != null) {
                    ldr.SetSymSect(s, sect);
                }
                ldr.SetSymValue(s, state.datsize);
                state.datsize += ldr.SymSize(s);
            }

            s = s__prev1;
        }

        state.checkdatsize(sym.STLSBSS);

        if (sect != null) {
            sect.Length = uint64(state.datsize);
        }
    }
    ptr<sym.Segment> segro;
    if (ctxt.IsELF && ctxt.LinkMode == LinkInternal) {
        segro = _addr_Segrodata;
    }
    else if (ctxt.HeadType == objabi.Hwindows) {
        segro = _addr_Segrodata;
    }
    else
 {
        segro = _addr_Segtext;
    }
    state.datsize = 0; 

    /* read-only executable ELF, Mach-O sections */
    if (len(state.data[sym.STEXT]) != 0) {
        var culprit = ldr.SymName(state.data[sym.STEXT][0]);
        Errorf(null, "dodata found an sym.STEXT symbol: %s", culprit);
    }
    state.allocateSingleSymSections(_addr_Segtext, sym.SELFRXSECT, sym.SRODATA, 05);
    state.allocateSingleSymSections(_addr_Segtext, sym.SMACHOPLT, sym.SRODATA, 05); 

    /* read-only data */
    sect = state.allocateNamedDataSection(segro, ".rodata", sym.ReadOnly, 04);
    ldr.SetSymSect(ldr.LookupOrCreateSym("runtime.rodata", 0), sect);
    ldr.SetSymSect(ldr.LookupOrCreateSym("runtime.erodata", 0), sect);
    if (!ctxt.UseRelro()) {
        ldr.SetSymSect(ldr.LookupOrCreateSym("runtime.types", 0), sect);
        ldr.SetSymSect(ldr.LookupOrCreateSym("runtime.etypes", 0), sect);
    }
    {
        var symn__prev1 = symn;

        foreach (var (_, __symn) in sym.ReadOnly) {
            symn = __symn;
            var symnStartValue = state.datsize;
            state.assignToSection(sect, symn, sym.SRODATA);
            setCarrierSize(symn, state.datsize - symnStartValue);
            if (ctxt.HeadType == objabi.Haix) { 
                // Read-only symbols might be wrapped inside their outer
                // symbol.
                // XCOFF symbol table needs to know the size of
                // these outer symbols.
                xcoffUpdateOuterSize(ctxt, state.datsize - symnStartValue, symn);

            }

        }
        symn = symn__prev1;
    }

    state.allocateSingleSymSections(segro, sym.SELFROSECT, sym.SRODATA, 04); 

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
    const nint relroPerm = 06;

    const nint fallbackPerm = 04;

    var relroSecPerm = fallbackPerm;
    Func<@string, @string> genrelrosecname = suffix => {
        if (suffix == "") {
            return ".rodata";
        }
        return suffix;

    };
    var seg = segro;

    if (ctxt.UseRelro()) {
        var segrelro = _addr_Segrelrodata;
        if (ctxt.LinkMode == LinkExternal && !ctxt.IsAIX() && !ctxt.IsDarwin()) { 
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
            state.datsize = 0;

        }
        if (!ctxt.IsDarwin()) { // We don't need the special names on darwin.
            genrelrosecname = suffix => {
                return ".data.rel.ro" + suffix;
            }
;

        }
        sym.SymKind relroReadOnly = new slice<sym.SymKind>(new sym.SymKind[] {  });
        {
            var symnro__prev1 = symnro;

            foreach (var (_, __symnro) in sym.ReadOnly) {
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

        ldr.SetSymSect(ldr.LookupOrCreateSym("runtime.types", 0), sect);
        ldr.SetSymSect(ldr.LookupOrCreateSym("runtime.etypes", 0), sect);

        {
            var symnro__prev1 = symnro;

            foreach (var (__i, __symnro) in sym.ReadOnly) {
                i = __i;
                symnro = __symnro;
                if (i == 0 && symnro == sym.STYPE && ctxt.HeadType != objabi.Haix) { 
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

                    foreach (var (_, __s) in state.data[symn]) {
                        s = __s;
                        var outer = ldr.OuterSym(s);
                        if (s != 0 && ldr.SymSect(outer) != null && ldr.SymSect(outer) != sect) {
                            ctxt.Errorf(s, "s.Outer (%s) in different section from s, %s != %s", ldr.SymName(outer), ldr.SymSect(outer).Name, sect.Name);
                        }
                    }

                    s = s__prev2;
                }

                state.assignToSection(sect, symn, sym.SRODATA);
                setCarrierSize(symn, state.datsize - symnStartValue);
                if (ctxt.HeadType == objabi.Haix) { 
                    // Read-only symbols might be wrapped inside their outer
                    // symbol.
                    // XCOFF symbol table needs to know the size of
                    // these outer symbols.
                    xcoffUpdateOuterSize(ctxt, state.datsize - symnStartValue, symn);

                }

            }

            symnro = symnro__prev1;
        }

        sect.Length = uint64(state.datsize) - sect.Vaddr;

    }
    sect = state.allocateNamedDataSection(seg, genrelrosecname(".typelink"), new slice<sym.SymKind>(new sym.SymKind[] { sym.STYPELINK }), relroSecPerm);

    var typelink = ldr.CreateSymForUpdate("runtime.typelink", 0);
    ldr.SetSymSect(typelink.Sym(), sect);
    typelink.SetType(sym.SRODATA);
    state.datsize += typelink.Size();
    state.checkdatsize(sym.STYPELINK);
    sect.Length = uint64(state.datsize) - sect.Vaddr; 

    /* itablink */
    sect = state.allocateNamedDataSection(seg, genrelrosecname(".itablink"), new slice<sym.SymKind>(new sym.SymKind[] { sym.SITABLINK }), relroSecPerm);

    var itablink = ldr.CreateSymForUpdate("runtime.itablink", 0);
    ldr.SetSymSect(itablink.Sym(), sect);
    itablink.SetType(sym.SRODATA);
    state.datsize += itablink.Size();
    state.checkdatsize(sym.SITABLINK);
    sect.Length = uint64(state.datsize) - sect.Vaddr; 

    /* gosymtab */
    sect = state.allocateNamedSectionAndAssignSyms(seg, genrelrosecname(".gosymtab"), sym.SSYMTAB, sym.SRODATA, relroSecPerm);
    ldr.SetSymSect(ldr.LookupOrCreateSym("runtime.symtab", 0), sect);
    ldr.SetSymSect(ldr.LookupOrCreateSym("runtime.esymtab", 0), sect); 

    /* gopclntab */
    sect = state.allocateNamedSectionAndAssignSyms(seg, genrelrosecname(".gopclntab"), sym.SPCLNTAB, sym.SRODATA, relroSecPerm);
    ldr.SetSymSect(ldr.LookupOrCreateSym("runtime.pclntab", 0), sect);
    ldr.SetSymSect(ldr.LookupOrCreateSym("runtime.pcheader", 0), sect);
    ldr.SetSymSect(ldr.LookupOrCreateSym("runtime.funcnametab", 0), sect);
    ldr.SetSymSect(ldr.LookupOrCreateSym("runtime.cutab", 0), sect);
    ldr.SetSymSect(ldr.LookupOrCreateSym("runtime.filetab", 0), sect);
    ldr.SetSymSect(ldr.LookupOrCreateSym("runtime.pctab", 0), sect);
    ldr.SetSymSect(ldr.LookupOrCreateSym("runtime.functab", 0), sect);
    ldr.SetSymSect(ldr.LookupOrCreateSym("runtime.epclntab", 0), sect);
    setCarrierSize(sym.SPCLNTAB, int64(sect.Length));
    if (ctxt.HeadType == objabi.Haix) {
        xcoffUpdateOuterSize(ctxt, int64(sect.Length), sym.SPCLNTAB);
    }
    if (state.datsize != int64(uint32(state.datsize))) {
        Errorf(null, "read-only data segment too large: %d", state.datsize);
    }
    nint siz = 0;
    {
        var symn__prev1 = symn;

        for (symn = sym.SELFRXSECT; symn < sym.SXREF; symn++) {
            siz += len(state.data[symn]);
        }

        symn = symn__prev1;
    }
    ctxt.datap = make_slice<loader.Sym>(0, siz);
    {
        var symn__prev1 = symn;

        for (symn = sym.SELFRXSECT; symn < sym.SXREF; symn++) {
            ctxt.datap = append(ctxt.datap, state.data[symn]);
        }

        symn = symn__prev1;
    }

}

// allocateDwarfSections allocates sym.Section objects for DWARF
// symbols, and assigns symbols to sections.
private static void allocateDwarfSections(this ptr<dodataState> _addr_state, ptr<Link> _addr_ctxt) {
    ref dodataState state = ref _addr_state.val;
    ref Link ctxt = ref _addr_ctxt.val;

    Func<ptr<dodataState>, long, loader.Sym, long> alignOne = (state, datsize, s) => datsize;

    var ldr = ctxt.loader;
    for (nint i = 0; i < len(dwarfp); i++) { 
        // First the section symbol.
        var s = dwarfp[i].secSym();
        var sect = state.allocateNamedDataSection(_addr_Segdwarf, ldr.SymName(s), new slice<sym.SymKind>(new sym.SymKind[] {  }), 04);
        ldr.SetSymSect(s, sect);
        sect.Sym = sym.LoaderSym(s);
        var curType = ldr.SymType(s);
        state.setSymType(s, sym.SRODATA);
        ldr.SetSymValue(s, int64(uint64(state.datsize) - sect.Vaddr));
        state.datsize += ldr.SymSize(s); 

        // Then any sub-symbols for the section symbol.
        var subSyms = dwarfp[i].subSyms();
        state.assignDsymsToSection(sect, subSyms, sym.SRODATA, alignOne);

        for (nint j = 0; j < len(subSyms); j++) {
            s = subSyms[j];
            if (ctxt.HeadType == objabi.Haix && curType == sym.SDWARFLOC) { 
                // Update the size of .debug_loc for this symbol's
                // package.
                addDwsectCUSize(".debug_loc", ldr.SymPkg(s), uint64(ldr.SymSize(s)));

            }

        }
        sect.Length = uint64(state.datsize) - sect.Vaddr;
        state.checkdatsize(curType);

    }

}

private partial struct symNameSize {
    public @string name;
    public long sz;
    public long val;
    public loader.Sym sym;
}

private static (slice<loader.Sym>, int) dodataSect(this ptr<dodataState> _addr_state, ptr<Link> _addr_ctxt, sym.SymKind symn, slice<loader.Sym> syms) {
    slice<loader.Sym> result = default;
    int maxAlign = default;
    ref dodataState state = ref _addr_state.val;
    ref Link ctxt = ref _addr_ctxt.val;

    loader.Sym head = default;    loader.Sym tail = default;

    var ldr = ctxt.loader;
    var sl = make_slice<symNameSize>(len(syms));
    {
        var k__prev1 = k;
        var s__prev1 = s;

        foreach (var (__k, __s) in syms) {
            k = __k;
            s = __s;
            var ss = ldr.SymSize(s);
            sl[k] = new symNameSize(name:ldr.SymName(s),sz:ss,sym:s);
            var ds = int64(len(ldr.Data(s)));

            if (ss < ds) 
                ctxt.Errorf(s, "initialize bounds (%d < %d)", ss, ds);
            else if (ss < 0) 
                ctxt.Errorf(s, "negative size (%d bytes)", ss);
            else if (ss > cutoff) 
                ctxt.Errorf(s, "symbol too large (%d bytes)", ss);
            // If the usually-special section-marker symbols are being laid
            // out as regular symbols, put them either at the beginning or
            // end of their section.
            if ((ctxt.DynlinkingGo() && ctxt.HeadType == objabi.Hdarwin) || (ctxt.HeadType == objabi.Haix && ctxt.LinkMode == LinkExternal)) {
                switch (ldr.SymName(s)) {
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
        k = k__prev1;
        s = s__prev1;
    }

    var checkSize = symn != sym.SELFGOT; 

    // Perform the sort.
    if (symn != sym.SPCLNTAB) {
        sort.Slice(sl, (i, j) => {
            var si = sl[i].sym;
            var sj = sl[j].sym;

            if (si == head || sj == tail) 
                return true;
            else if (sj == head || si == tail) 
                return false;
                        if (checkSize) {
                var isz = sl[i].sz;
                var jsz = sl[j].sz;
                if (isz != jsz) {
                    return isz < jsz;
                }
            }

            var iname = sl[i].name;
            var jname = sl[j].name;
            if (iname != jname) {
                return iname < jname;
            }

            return si < sj;

        }
    else
);

    } { 
        // PCLNTAB was built internally, and has the proper order based on value.
        // Sort the symbols as such.
        {
            var k__prev1 = k;
            var s__prev1 = s;

            foreach (var (__k, __s) in syms) {
                k = __k;
                s = __s;
                sl[k].val = ldr.SymValue(s);
            }

            k = k__prev1;
            s = s__prev1;
        }

        sort.Slice(sl, (i, j) => sl[i].val < sl[j].val);

    }
    syms = syms[..(int)0];
    {
        var k__prev1 = k;

        foreach (var (__k) in sl) {
            k = __k;
            var s = sl[k].sym;
            if (s != head && s != tail) {
                var align = symalign(_addr_ldr, s);
                if (maxAlign < align) {
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
private static void textbuildid(this ptr<Link> _addr_ctxt) {
    ref Link ctxt = ref _addr_ctxt.val;

    if (ctxt.IsELF || ctxt.BuildMode == BuildModePlugin || flagBuildid == "".val) {
        return ;
    }
    var ldr = ctxt.loader;
    var s = ldr.CreateSymForUpdate("go.buildid", 0); 
    // The \xff is invalid UTF-8, meant to make it less likely
    // to find one of these accidentally.
    @string data = "\xff Go build ID: " + strconv.Quote(flagBuildid.val) + "\n \xff";
    s.SetType(sym.STEXT);
    s.SetData((slice<byte>)data);
    s.SetSize(int64(len(data)));

    ctxt.Textp = append(ctxt.Textp, 0);
    copy(ctxt.Textp[(int)1..], ctxt.Textp);
    ctxt.Textp[0] = s.Sym();

}

private static void buildinfo(this ptr<Link> _addr_ctxt) {
    ref Link ctxt = ref _addr_ctxt.val;

    if (ctxt.linkShared || ctxt.BuildMode == BuildModePlugin) { 
        // -linkshared and -buildmode=plugin get confused
        // about the relocations in go.buildinfo
        // pointing at the other data sections.
        // The version information is only available in executables.
        return ;

    }
    var ldr = ctxt.loader;
    var s = ldr.CreateSymForUpdate(".go.buildinfo", 0); 
    // On AIX, .go.buildinfo must be in the symbol table as
    // it has relocations.
    s.SetNotInSymbolTable(!ctxt.IsAIX());
    s.SetType(sym.SBUILDINFO);
    s.SetAlign(16); 
    // The \xff is invalid UTF-8, meant to make it less likely
    // to find one of these accidentally.
    const @string prefix = "\xff Go buildinf:"; // 14 bytes, plus 2 data bytes filled in below
 // 14 bytes, plus 2 data bytes filled in below
    var data = make_slice<byte>(32);
    copy(data, prefix);
    data[len(prefix)] = byte(ctxt.Arch.PtrSize);
    data[len(prefix) + 1] = 0;
    if (ctxt.Arch.ByteOrder == binary.BigEndian) {
        data[len(prefix) + 1] = 1;
    }
    s.SetData(data);
    s.SetSize(int64(len(data)));
    var (r, _) = s.AddRel(objabi.R_ADDR);
    r.SetOff(16);
    r.SetSiz(uint8(ctxt.Arch.PtrSize));
    r.SetSym(ldr.LookupOrCreateSym("runtime.buildVersion", 0));
    r, _ = s.AddRel(objabi.R_ADDR);
    r.SetOff(16 + int32(ctxt.Arch.PtrSize));
    r.SetSiz(uint8(ctxt.Arch.PtrSize));
    r.SetSym(ldr.LookupOrCreateSym("runtime.modinfo", 0));

}

// assign addresses to text
private static void textaddress(this ptr<Link> _addr_ctxt) {
    ref Link ctxt = ref _addr_ctxt.val;

    addsection(ctxt.loader, ctxt.Arch, _addr_Segtext, ".text", 05); 

    // Assign PCs in text segment.
    // Could parallelize, by assigning to text
    // and then letting threads copy down, but probably not worth it.
    var sect = Segtext.Sections[0];

    sect.Align = int32(Funcalign);

    var ldr = ctxt.loader;

    var text = ctxt.xdefine("runtime.text", sym.STEXT, 0);
    var etext = ctxt.xdefine("runtime.etext", sym.STEXT, 0);
    ldr.SetSymSect(text, sect);
    if (ctxt.IsAIX() && ctxt.IsExternal()) { 
        // Setting runtime.text has a real symbol prevents ld to
        // change its base address resulting in wrong offsets for
        // reflect methods.
        var u = ldr.MakeSymbolUpdater(text);
        u.SetAlign(sect.Align);
        u.SetSize(8);

    }
    if ((ctxt.DynlinkingGo() && ctxt.IsDarwin()) || (ctxt.IsAIX() && ctxt.IsExternal())) {
        ldr.SetSymSect(etext, sect);
        ctxt.Textp = append(ctxt.Textp, etext, 0);
        copy(ctxt.Textp[(int)1..], ctxt.Textp);
        ctxt.Textp[0] = text;
    }
    var start = uint64(Rnd(FlagTextAddr.val, int64(Funcalign)));
    var va = start;
    nint n = 1;
    sect.Vaddr = va;

    var limit = thearch.TrampLimit;
    if (limit == 0) {
        limit = 1 << 63; // unlimited
    }
    if (FlagDebugTextSize != 0.val) {
        limit = uint64(FlagDebugTextSize.val);
    }
    if (FlagDebugTramp > 1.val) {
        limit = 1; // debug mode, force generating trampolines for everything
    }
    if (ctxt.IsAIX() && ctxt.IsExternal()) { 
        // On AIX, normally we won't generate direct calls to external symbols,
        // except in one test, cmd/go/testdata/script/link_syso_issue33139.txt.
        // That test doesn't make much sense, and I'm not sure it ever works.
        // Just generate trampoline for now (which will turn a direct call to
        // an indirect call, which at least builds).
        limit = 1;

    }
    var big = false;
    {
        var s__prev1 = s;

        foreach (var (_, __s) in ctxt.Textp) {
            s = __s;
            sect, n, va = assignAddress(_addr_ctxt, _addr_sect, n, s, va, false, big);
            if (va - start >= limit) {
                big = true;
                break;
            }
        }
        s = s__prev1;
    }

    if (big) { 
        // reset addresses
        {
            var s__prev1 = s;

            foreach (var (_, __s) in ctxt.Textp) {
                s = __s;
                if (ldr.OuterSym(s) != 0 || s == text) {
                    continue;
                }
                var oldv = ldr.SymValue(s);
                {
                    var sub = s;

                    while (sub != 0) {
                        ldr.SetSymValue(sub, ldr.SymValue(sub) - oldv);
                        sub = ldr.SubSym(sub);
                    }

                }

            }

            s = s__prev1;
        }

        va = start;

        nint ntramps = 0;
        {
            var s__prev1 = s;

            foreach (var (_, __s) in ctxt.Textp) {
                s = __s;
                sect, n, va = assignAddress(_addr_ctxt, _addr_sect, n, s, va, false, big);

                trampoline(_addr_ctxt, s); // resolve jumps, may add trampolines if jump too far

                // lay down trampolines after each function
                while (ntramps < len(ctxt.tramps)) {
                    var tramp = ctxt.tramps[ntramps];
                    if (ctxt.IsAIX() && strings.HasPrefix(ldr.SymName(tramp), "runtime.text.")) { 
                        // Already set in assignAddress
                        continue;
                    ntramps++;
                    }

                    sect, n, va = assignAddress(_addr_ctxt, _addr_sect, n, tramp, va, true, big);

                }


            } 

            // merge tramps into Textp, keeping Textp in address order

            s = s__prev1;
        }

        if (ntramps != 0) {
            var newtextp = make_slice<loader.Sym>(0, len(ctxt.Textp) + ntramps);
            nint i = 0;
            {
                var s__prev1 = s;

                foreach (var (_, __s) in ctxt.Textp) {
                    s = __s;
                    while (i < ntramps && ldr.SymValue(ctxt.tramps[i]) < ldr.SymValue(s)) {
                        newtextp = append(newtextp, ctxt.tramps[i]);
                        i++;
                    }

                    newtextp = append(newtextp, s);

                }

                s = s__prev1;
            }

            newtextp = append(newtextp, ctxt.tramps[(int)i..(int)ntramps]);

            ctxt.Textp = newtextp;

        }
    }
    sect.Length = va - sect.Vaddr;
    ldr.SetSymSect(etext, sect);
    if (ldr.SymValue(etext) == 0) { 
        // Set the address of the start/end symbols, if not already
        // (i.e. not darwin+dynlink or AIX+external, see above).
        ldr.SetSymValue(etext, int64(va));
        ldr.SetSymValue(text, int64(Segtext.Sections[0].Vaddr));

    }
}

// assigns address for a text symbol, returns (possibly new) section, its number, and the address
private static (ptr<sym.Section>, nint, ulong) assignAddress(ptr<Link> _addr_ctxt, ptr<sym.Section> _addr_sect, nint n, loader.Sym s, ulong va, bool isTramp, bool big) => func((_, panic, _) => {
    ptr<sym.Section> _p0 = default!;
    nint _p0 = default;
    ulong _p0 = default;
    ref Link ctxt = ref _addr_ctxt.val;
    ref sym.Section sect = ref _addr_sect.val;

    var ldr = ctxt.loader;
    if (thearch.AssignAddress != null) {
        return _addr_thearch.AssignAddress(ldr, sect, n, s, va, isTramp)!;
    }
    ldr.SetSymSect(s, sect);
    if (ldr.AttrSubSymbol(s)) {
        return (_addr_sect!, n, va);
    }
    var align = ldr.SymAlign(s);
    if (align == 0) {
        align = int32(Funcalign);
    }
    va = uint64(Rnd(int64(va), int64(align)));
    if (sect.Align < align) {
        sect.Align = align;
    }
    var funcsize = uint64(MINFUNC); // spacing required for findfunctab
    if (ldr.SymSize(s) > MINFUNC) {
        funcsize = uint64(ldr.SymSize(s));
    }
    if (big && splitTextSections(_addr_ctxt) && ldr.OuterSym(s) == 0) { 
        // For debugging purposes, allow text size limit to be cranked down,
        // so as to stress test the code that handles multiple text sections.
        ulong textSizelimit = thearch.TrampLimit;
        if (FlagDebugTextSize != 0.val) {
            textSizelimit = uint64(FlagDebugTextSize.val);
        }
        if (funcsize > textSizelimit) {
            panic(fmt.Sprintf("error: text size limit %d less than text symbol %s size of %d", textSizelimit, ldr.SymName(s), funcsize));
        }
        if (va - sect.Vaddr + funcsize + maxSizeTrampolines(_addr_ctxt, _addr_ldr, s, isTramp) > textSizelimit) {
            var sectAlign = int32(thearch.Funcalign);
            if (ctxt.IsPPC64()) { 
                // Align the next text section to the worst case function alignment likely
                // to be encountered when processing function symbols. The start address
                // is rounded against the final alignment of the text section later on in
                // (*Link).address. This may happen due to usage of PCALIGN directives
                // larger than Funcalign, or usage of ISA 3.1 prefixed instructions
                // (see ISA 3.1 Book I 1.9).
                const nint ppc64maxFuncalign = 64;

                sectAlign = ppc64maxFuncalign;
                va = uint64(Rnd(int64(va), ppc64maxFuncalign));

            } 

            // Set the length for the previous text section
            sect.Length = va - sect.Vaddr; 

            // Create new section, set the starting Vaddr
            sect = addsection(ctxt.loader, ctxt.Arch, _addr_Segtext, ".text", 05);

            sect.Vaddr = va;
            sect.Align = sectAlign;
            ldr.SetSymSect(s, sect); 

            // Create a symbol for the start of the secondary text sections
            var ntext = ldr.CreateSymForUpdate(fmt.Sprintf("runtime.text.%d", n), 0);
            ntext.SetSect(sect);
            if (ctxt.IsAIX()) { 
                // runtime.text.X must be a real symbol on AIX.
                // Assign its address directly in order to be the
                // first symbol of this new section.
                ntext.SetType(sym.STEXT);
                ntext.SetSize(int64(MINFUNC));
                ntext.SetOnList(true);
                ntext.SetAlign(sectAlign);
                ctxt.tramps = append(ctxt.tramps, ntext.Sym());

                ntext.SetValue(int64(va));
                va += uint64(ntext.Size());

                {
                    var align__prev4 = align;

                    align = ldr.SymAlign(s);

                    if (align != 0) {
                        va = uint64(Rnd(int64(va), int64(align)));
                    }
                    else
 {
                        va = uint64(Rnd(int64(va), int64(Funcalign)));
                    }

                    align = align__prev4;

                }

            }

            n++;

        }
    }
    ldr.SetSymValue(s, 0);
    {
        var sub = s;

        while (sub != 0) {
            ldr.SetSymValue(sub, ldr.SymValue(sub) + int64(va));
            if (ctxt.Debugvlog > 2) {
                fmt.Println("assign text address:", ldr.SymName(sub), ldr.SymValue(sub));
            sub = ldr.SubSym(sub);
            }

        }
    }

    va += funcsize;

    return (_addr_sect!, n, va);

});

// Return whether we may need to split text sections.
//
// On PPC64x whem external linking a text section should not be larger than 2^25 bytes
// due to the size of call target offset field in the bl instruction.  Splitting into
// smaller text sections smaller than this limit allows the system linker to modify the long
// calls appropriately. The limit allows for the space needed for tables inserted by the
// linker.
//
// The same applies to Darwin/ARM64, with 2^27 byte threshold.
private static bool splitTextSections(ptr<Link> _addr_ctxt) {
    ref Link ctxt = ref _addr_ctxt.val;

    return (ctxt.IsPPC64() || (ctxt.IsARM64() && ctxt.IsDarwin())) && ctxt.IsExternal();
}

// On Wasm, we reserve 4096 bytes for zero page, then 8192 bytes for wasm_exec.js
// to store command line args and environment variables.
// Data sections starts from at least address 12288.
// Keep in sync with wasm_exec.js.
private static readonly nint wasmMinDataAddr = 4096 + 8192;

// address assigns virtual addresses to all segments and sections and
// returns all segments in file order.


// address assigns virtual addresses to all segments and sections and
// returns all segments in file order.
private static slice<ptr<sym.Segment>> address(this ptr<Link> _addr_ctxt) => func((_, panic, _) => {
    ref Link ctxt = ref _addr_ctxt.val;

    slice<ptr<sym.Segment>> order = default; // Layout order

    var va = uint64(FlagTextAddr.val);
    order = append(order, _addr_Segtext);
    Segtext.Rwx = 05;
    Segtext.Vaddr = va;
    {
        var i__prev1 = i;
        var s__prev1 = s;

        foreach (var (__i, __s) in Segtext.Sections) {
            i = __i;
            s = __s;
            va = uint64(Rnd(int64(va), int64(s.Align)));
            s.Vaddr = va;
            va += s.Length;

            if (ctxt.IsWasm() && i == 0 && va < wasmMinDataAddr) {
                va = wasmMinDataAddr;
            }
        }
        i = i__prev1;
        s = s__prev1;
    }

    Segtext.Length = va - uint64(FlagTextAddr.val);

    if (len(Segrodata.Sections) > 0) { 
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
        Segrodata.Rwx = 04;
        Segrodata.Vaddr = va;
        {
            var s__prev1 = s;

            foreach (var (_, __s) in Segrodata.Sections) {
                s = __s;
                va = uint64(Rnd(int64(va), int64(s.Align)));
                s.Vaddr = va;
                va += s.Length;
            }

            s = s__prev1;
        }

        Segrodata.Length = va - Segrodata.Vaddr;

    }
    if (len(Segrelrodata.Sections) > 0) { 
        // align to page boundary so as not to mix
        // rodata, rel-ro data, and executable text.
        va = uint64(Rnd(int64(va), int64(FlagRound.val)));
        if (ctxt.HeadType == objabi.Haix) { 
            // Relro data are inside data segment on AIX.
            va += uint64(XCOFFDATABASE) - uint64(XCOFFTEXTBASE);

        }
        order = append(order, _addr_Segrelrodata);
        Segrelrodata.Rwx = 06;
        Segrelrodata.Vaddr = va;
        {
            var s__prev1 = s;

            foreach (var (_, __s) in Segrelrodata.Sections) {
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
    if (ctxt.HeadType == objabi.Haix && len(Segrelrodata.Sections) == 0) { 
        // Data sections are moved to an unreachable segment
        // to ensure that they are position-independent.
        // Already done if relro sections exist.
        va += uint64(XCOFFDATABASE) - uint64(XCOFFTEXTBASE);

    }
    order = append(order, _addr_Segdata);
    Segdata.Rwx = 06;
    Segdata.Vaddr = va;
    ptr<sym.Section> data;
    ptr<sym.Section> noptr;
    ptr<sym.Section> bss;
    ptr<sym.Section> noptrbss;
    {
        var i__prev1 = i;
        var s__prev1 = s;

        foreach (var (__i, __s) in Segdata.Sections) {
            i = __i;
            s = __s;
            if ((ctxt.IsELF || ctxt.HeadType == objabi.Haix) && s.Name == ".tbss") {
                continue;
            }
            var vlen = int64(s.Length);
            if (i + 1 < len(Segdata.Sections) && !((ctxt.IsELF || ctxt.HeadType == objabi.Haix) && Segdata.Sections[i + 1].Name == ".tbss")) {
                vlen = int64(Segdata.Sections[i + 1].Vaddr - s.Vaddr);
            }
            s.Vaddr = va;
            va += uint64(vlen);
            Segdata.Length = va - Segdata.Vaddr;
            if (s.Name == ".data") {
                data = s;
            }
            if (s.Name == ".noptrdata") {
                noptr = s;
            }
            if (s.Name == ".bss") {
                bss = s;
            }
            if (s.Name == ".noptrbss") {
                noptrbss = s;
            }
        }
        i = i__prev1;
        s = s__prev1;
    }

    Segdata.Filelen = bss.Vaddr - Segdata.Vaddr;

    va = uint64(Rnd(int64(va), int64(FlagRound.val)));
    order = append(order, _addr_Segdwarf);
    Segdwarf.Rwx = 06;
    Segdwarf.Vaddr = va;
    {
        var i__prev1 = i;
        var s__prev1 = s;

        foreach (var (__i, __s) in Segdwarf.Sections) {
            i = __i;
            s = __s;
            vlen = int64(s.Length);
            if (i + 1 < len(Segdwarf.Sections)) {
                vlen = int64(Segdwarf.Sections[i + 1].Vaddr - s.Vaddr);
            }
            s.Vaddr = va;
            va += uint64(vlen);
            if (ctxt.HeadType == objabi.Hwindows) {
                va = uint64(Rnd(int64(va), PEFILEALIGN));
            }
            Segdwarf.Length = va - Segdwarf.Vaddr;
        }
        i = i__prev1;
        s = s__prev1;
    }

    var ldr = ctxt.loader;
    var rodata = ldr.SymSect(ldr.LookupOrCreateSym("runtime.rodata", 0));    var symtab = ldr.SymSect(ldr.LookupOrCreateSym("runtime.symtab", 0));    var pclntab = ldr.SymSect(ldr.LookupOrCreateSym("runtime.pclntab", 0));    var types = ldr.SymSect(ldr.LookupOrCreateSym("runtime.types", 0));

    {
        var s__prev1 = s;

        foreach (var (_, __s) in ctxt.datap) {
            s = __s;
            {
                var sect__prev1 = sect;

                var sect = ldr.SymSect(s);

                if (sect != null) {
                    ldr.AddToSymValue(s, int64(sect.Vaddr));
                }

                sect = sect__prev1;

            }

            var v = ldr.SymValue(s);
            {
                var sub__prev2 = sub;

                var sub = ldr.SubSym(s);

                while (sub != 0) {
                    ldr.AddToSymValue(sub, v);
                    sub = ldr.SubSym(sub);
                }


                sub = sub__prev2;
            }

        }
        s = s__prev1;
    }

    foreach (var (_, si) in dwarfp) {
        {
            var s__prev2 = s;

            foreach (var (_, __s) in si.syms) {
                s = __s;
                {
                    var sect__prev1 = sect;

                    sect = ldr.SymSect(s);

                    if (sect != null) {
                        ldr.AddToSymValue(s, int64(sect.Vaddr));
                    }

                    sect = sect__prev1;

                }

                sub = ldr.SubSym(s);
                if (sub != 0) {
                    panic(fmt.Sprintf("unexpected sub-sym for %s %s", ldr.SymName(s), ldr.SymType(s).String()));
                }

                v = ldr.SymValue(s);
                while (sub != 0) {
                    ldr.AddToSymValue(s, v);
                    sub = ldr.SubSym(sub);
                }


            }

            s = s__prev2;
        }
    }    if (ctxt.BuildMode == BuildModeShared) {
        var s = ldr.LookupOrCreateSym("go.link.abihashbytes", 0);
        sect = ldr.SymSect(ldr.LookupOrCreateSym(".note.go.abihash", 0));
        ldr.SetSymSect(s, sect);
        ldr.SetSymValue(s, int64(sect.Vaddr + 16));
    }
    nint n = 1;
    {
        var sect__prev1 = sect;

        foreach (var (_, __sect) in Segtext.Sections[(int)1..]) {
            sect = __sect;
            if (sect.Name != ".text") {
                break;
            }
            var symname = fmt.Sprintf("runtime.text.%d", n);
            if (ctxt.HeadType != objabi.Haix || ctxt.LinkMode != LinkExternal) { 
                // Addresses are already set on AIX with external linker
                // because these symbols are part of their sections.
                ctxt.xdefine(symname, sym.STEXT, int64(sect.Vaddr));

            }

            n++;

        }
        sect = sect__prev1;
    }

    ctxt.xdefine("runtime.rodata", sym.SRODATA, int64(rodata.Vaddr));
    ctxt.xdefine("runtime.erodata", sym.SRODATA, int64(rodata.Vaddr + rodata.Length));
    ctxt.xdefine("runtime.types", sym.SRODATA, int64(types.Vaddr));
    ctxt.xdefine("runtime.etypes", sym.SRODATA, int64(types.Vaddr + types.Length));

    s = ldr.Lookup("runtime.gcdata", 0);
    ldr.SetAttrLocal(s, true);
    ctxt.xdefine("runtime.egcdata", sym.SRODATA, ldr.SymAddr(s) + ldr.SymSize(s));
    ldr.SetSymSect(ldr.LookupOrCreateSym("runtime.egcdata", 0), ldr.SymSect(s));

    s = ldr.LookupOrCreateSym("runtime.gcbss", 0);
    ldr.SetAttrLocal(s, true);
    ctxt.xdefine("runtime.egcbss", sym.SRODATA, ldr.SymAddr(s) + ldr.SymSize(s));
    ldr.SetSymSect(ldr.LookupOrCreateSym("runtime.egcbss", 0), ldr.SymSect(s));

    ctxt.xdefine("runtime.symtab", sym.SRODATA, int64(symtab.Vaddr));
    ctxt.xdefine("runtime.esymtab", sym.SRODATA, int64(symtab.Vaddr + symtab.Length));
    ctxt.xdefine("runtime.pclntab", sym.SRODATA, int64(pclntab.Vaddr));
    ctxt.defineInternal("runtime.pcheader", sym.SRODATA);
    ctxt.defineInternal("runtime.funcnametab", sym.SRODATA);
    ctxt.defineInternal("runtime.cutab", sym.SRODATA);
    ctxt.defineInternal("runtime.filetab", sym.SRODATA);
    ctxt.defineInternal("runtime.pctab", sym.SRODATA);
    ctxt.defineInternal("runtime.functab", sym.SRODATA);
    ctxt.xdefine("runtime.epclntab", sym.SRODATA, int64(pclntab.Vaddr + pclntab.Length));
    ctxt.xdefine("runtime.noptrdata", sym.SNOPTRDATA, int64(noptr.Vaddr));
    ctxt.xdefine("runtime.enoptrdata", sym.SNOPTRDATA, int64(noptr.Vaddr + noptr.Length));
    ctxt.xdefine("runtime.bss", sym.SBSS, int64(bss.Vaddr));
    ctxt.xdefine("runtime.ebss", sym.SBSS, int64(bss.Vaddr + bss.Length));
    ctxt.xdefine("runtime.data", sym.SDATA, int64(data.Vaddr));
    ctxt.xdefine("runtime.edata", sym.SDATA, int64(data.Vaddr + data.Length));
    ctxt.xdefine("runtime.noptrbss", sym.SNOPTRBSS, int64(noptrbss.Vaddr));
    ctxt.xdefine("runtime.enoptrbss", sym.SNOPTRBSS, int64(noptrbss.Vaddr + noptrbss.Length));
    ctxt.xdefine("runtime.end", sym.SBSS, int64(Segdata.Vaddr + Segdata.Length));

    if (ctxt.IsSolaris()) { 
        // On Solaris, in the runtime it sets the external names of the
        // end symbols. Unset them and define separate symbols, so we
        // keep both.
        var etext = ldr.Lookup("runtime.etext", 0);
        var edata = ldr.Lookup("runtime.edata", 0);
        var end = ldr.Lookup("runtime.end", 0);
        ldr.SetSymExtname(etext, "runtime.etext");
        ldr.SetSymExtname(edata, "runtime.edata");
        ldr.SetSymExtname(end, "runtime.end");
        ctxt.xdefine("_etext", ldr.SymType(etext), ldr.SymValue(etext));
        ctxt.xdefine("_edata", ldr.SymType(edata), ldr.SymValue(edata));
        ctxt.xdefine("_end", ldr.SymType(end), ldr.SymValue(end));
        ldr.SetSymSect(ldr.Lookup("_etext", 0), ldr.SymSect(etext));
        ldr.SetSymSect(ldr.Lookup("_edata", 0), ldr.SymSect(edata));
        ldr.SetSymSect(ldr.Lookup("_end", 0), ldr.SymSect(end));

    }
    return order;

});

// layout assigns file offsets and lengths to the segments in order.
// Returns the file size containing all the segments.
private static ulong layout(this ptr<Link> _addr_ctxt, slice<ptr<sym.Segment>> order) {
    ref Link ctxt = ref _addr_ctxt.val;

    ptr<sym.Segment> prev;
    foreach (var (_, seg) in order) {
        if (prev == null) {
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
                if (seg.Vaddr % uint64(FlagRound.val) != seg.Fileoff % uint64(FlagRound.val)) {
                    Exitf("bad segment rounding (Vaddr=%#x Fileoff=%#x FlagRound=%#x)", seg.Vaddr, seg.Fileoff, FlagRound.val);
                }
            
        }
        if (seg != _addr_Segdata) { 
            // Link.address already set Segdata.Filelen to
            // account for BSS.
            seg.Filelen = seg.Length;

        }
        prev = seg;

    }    return prev.Fileoff + prev.Filelen;

}

// add a trampoline with symbol s (to be laid down after the current function)
private static void AddTramp(this ptr<Link> _addr_ctxt, ptr<loader.SymbolBuilder> _addr_s) {
    ref Link ctxt = ref _addr_ctxt.val;
    ref loader.SymbolBuilder s = ref _addr_s.val;

    s.SetType(sym.STEXT);
    s.SetReachable(true);
    s.SetOnList(true);
    ctxt.tramps = append(ctxt.tramps, s.Sym());
    if (FlagDebugTramp > 0 && ctxt.Debugvlog > 0.val) {
        ctxt.Logf("trampoline %s inserted\n", s.Name());
    }
}

// compressSyms compresses syms and returns the contents of the
// compressed section. If the section would get larger, it returns nil.
private static slice<byte> compressSyms(ptr<Link> _addr_ctxt, slice<loader.Sym> syms) {
    ref Link ctxt = ref _addr_ctxt.val;

    var ldr = ctxt.loader;
    long total = default;
    foreach (var (_, sym) in syms) {
        total += ldr.SymSize(sym);
    }    ref bytes.Buffer buf = ref heap(out ptr<bytes.Buffer> _addr_buf);
    buf.Write((slice<byte>)"ZLIB");
    array<byte> sizeBytes = new array<byte>(8);
    binary.BigEndian.PutUint64(sizeBytes[..], uint64(total));
    buf.Write(sizeBytes[..]);

    slice<byte> relocbuf = default; // temporary buffer for applying relocations

    // Using zlib.BestSpeed achieves very nearly the same
    // compression levels of zlib.DefaultCompression, but takes
    // substantially less time. This is important because DWARF
    // compression can be a significant fraction of link time.
    var (z, err) = zlib.NewWriterLevel(_addr_buf, zlib.BestSpeed);
    if (err != null) {
        log.Fatalf("NewWriterLevel failed: %s", err);
    }
    var st = ctxt.makeRelocSymState();
    foreach (var (_, s) in syms) { 
        // Symbol data may be read-only. Apply relocations in a
        // temporary buffer, and immediately write it out.
        var P = ldr.Data(s);
        var relocs = ldr.Relocs(s);
        if (relocs.Count() != 0) {
            relocbuf = append(relocbuf[..(int)0], P);
            P = relocbuf;
            st.relocsym(s, P);
        }
        {
            var (_, err) = z.Write(P);

            if (err != null) {
                log.Fatalf("compression failed: %s", err);
            }

        }

        {
            var i = ldr.SymSize(s) - int64(len(P));

            while (i > 0) {
                var b = zeros[..];
                if (i < int64(len(b))) {
                    b = b[..(int)i];
                }
                var (n, err) = z.Write(b);
                if (err != null) {
                    log.Fatalf("compression failed: %s", err);
                }
                i -= int64(n);
            }

        }

    }    {
        var err = z.Close();

        if (err != null) {
            log.Fatalf("compression failed: %s", err);
        }
    }

    if (int64(buf.Len()) >= total) { 
        // Compression didn't save any space.
        return null;

    }
    return buf.Bytes();

}

} // end ld_package
