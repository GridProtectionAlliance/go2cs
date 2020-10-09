// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ld -- go2cs converted at 2020 October 09 05:49:14 UTC
// import "cmd/link/internal/ld" ==> using ld = go.cmd.link.@internal.ld_package
// Original source: C:\Go\src\cmd\link\internal\ld\data2.go
using objabi = go.cmd.@internal.objabi_package;
using loader = go.cmd.link.@internal.loader_package;
using sym = go.cmd.link.@internal.sym_package;
using fmt = go.fmt_package;
using log = go.log_package;
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
        // Temporary dumping around for sym.Symbol version of helper
        // functions in dodata(), still being used for some archs/oses.
        // FIXME: get rid of this file when dodata() is completely
        // converted.
        public static long Addstring(ptr<sym.Symbol> _addr_s, @string str)
        {
            ref sym.Symbol s = ref _addr_s.val;

            if (s.Type == 0L)
            {
                s.Type = sym.SNOPTRDATA;
            }
            s.Attr |= sym.AttrReachable;
            var r = s.Size;
            if (s.Name == ".shstrtab")
            {
                elfsetstring(s, str, int(r));
            }
            s.P = append(s.P, str);
            s.P = append(s.P, 0L);
            s.Size = int64(len(s.P));
            return r;

        }

        // symalign returns the required alignment for the given symbol s.
        private static int symalign(ptr<sym.Symbol> _addr_s)
        {
            ref sym.Symbol s = ref _addr_s.val;

            var min = int32(thearch.Minalign);
            if (s.Align >= min)
            {
                return s.Align;
            }
            else if (s.Align != 0L)
            {
                return min;
            }

            if (strings.HasPrefix(s.Name, "go.string.") || strings.HasPrefix(s.Name, "type..namedata."))
            { 
                // String data is just bytes.
                // If we align it, we waste a lot of space to padding.
                return min;

            }

            var align = int32(thearch.Maxalign);
            while (int64(align) > s.Size && align > min)
            {
                align >>= 1L;
            }

            s.Align = align;
            return align;

        }

        private static void relocsym2(ptr<Target> _addr_target, ptr<loader.Loader> _addr_ldr, ptr<ErrorReporter> _addr_err, ptr<ArchSyms> _addr_syms, ptr<sym.Symbol> _addr_s)
        {
            ref Target target = ref _addr_target.val;
            ref loader.Loader ldr = ref _addr_ldr.val;
            ref ErrorReporter err = ref _addr_err.val;
            ref ArchSyms syms = ref _addr_syms.val;
            ref sym.Symbol s = ref _addr_s.val;

            if (len(s.R) == 0L)
            {
                return ;
            }

            for (var ri = int32(0L); ri < int32(len(s.R)); ri++)
            {
                var r = _addr_s.R[ri];
                if (r.Done)
                { 
                    // Relocation already processed by an earlier phase.
                    continue;

                }

                r.Done = true;
                var off = r.Off;
                var siz = int32(r.Siz);
                if (off < 0L || off + siz > int32(len(s.P)))
                {
                    @string rname = "";
                    if (r.Sym != null)
                    {
                        rname = r.Sym.Name;
                    }

                    Errorf(s, "invalid relocation %s: %d+%d not in [%d,%d)", rname, off, siz, 0L, len(s.P));
                    continue;

                }

                if (r.Sym != null && ((r.Sym.Type == sym.Sxxx && !r.Sym.Attr.VisibilityHidden()) || r.Sym.Type == sym.SXREF))
                { 
                    // When putting the runtime but not main into a shared library
                    // these symbols are undefined and that's OK.
                    if (target.IsShared() || target.IsPlugin())
                    {
                        if (r.Sym.Name == "main.main" || (!target.IsPlugin() && r.Sym.Name == "main..inittask"))
                        {
                            r.Sym.Type = sym.SDYNIMPORT;
                        }
                        else if (strings.HasPrefix(r.Sym.Name, "go.info."))
                        { 
                            // Skip go.info symbols. They are only needed to communicate
                            // DWARF info between the compiler and linker.
                            continue;

                        }

                    }
                    else
                    {
                        err.errorUnresolved2(s, r);
                        continue;
                    }

                }

                if (r.Type >= objabi.ElfRelocOffset)
                {
                    continue;
                }

                if (r.Siz == 0L)
                { // informational relocation - no work to do
                    continue;

                } 

                // We need to be able to reference dynimport symbols when linking against
                // shared libraries, and Solaris, Darwin and AIX need it always
                if (!target.IsSolaris() && !target.IsDarwin() && !target.IsAIX() && r.Sym != null && r.Sym.Type == sym.SDYNIMPORT && !target.IsDynlinkingGo() && !r.Sym.Attr.SubSymbol())
                {
                    if (!(target.IsPPC64() && target.IsExternal() && r.Sym.Name == ".TOC."))
                    {
                        Errorf(s, "unhandled relocation for %s (type %d (%s) rtype %d (%s))", r.Sym.Name, r.Sym.Type, r.Sym.Type, r.Type, sym.RelocName(target.Arch, r.Type));
                    }

                }

                if (r.Sym != null && r.Sym.Type != sym.STLSBSS && r.Type != objabi.R_WEAKADDROFF && !r.Sym.Attr.Reachable())
                {
                    Errorf(s, "unreachable sym in relocation: %s", r.Sym.Name);
                }

                if (target.IsExternal())
                {
                    r.InitExt();
                } 

                // TODO(mundaym): remove this special case - see issue 14218.
                if (target.IsS390X())
                {

                    if (r.Type == objabi.R_PCRELDBL) 
                        r.InitExt();
                        r.Type = objabi.R_PCREL;
                        r.Variant = sym.RV_390_DBL;
                    else if (r.Type == objabi.R_CALL) 
                        r.InitExt();
                        r.Variant = sym.RV_390_DBL;
                    
                }

                long o = default;

                if (r.Type == objabi.R_TLS_LE)
                {
                    if (target.IsExternal() && target.IsElf())
                    {
                        r.Done = false;
                        if (r.Sym == null)
                        {
                            r.Sym = syms.Tlsg;
                        }

                        r.Xsym = r.Sym;
                        r.Xadd = r.Add;
                        o = 0L;
                        if (!target.IsAMD64())
                        {
                            o = r.Add;
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
                        o = 8L + r.Sym.Value;

                    }
                    else if (target.IsElf() || target.IsPlan9() || target.IsDarwin())
                    {
                        o = int64(syms.Tlsoffset) + r.Add;
                    }
                    else if (target.IsWindows())
                    {
                        o = r.Add;
                    }
                    else
                    {
                        log.Fatalf("unexpected R_TLS_LE relocation for %v", target.HeadType);
                    }

                    goto __switch_break0;
                }
                if (r.Type == objabi.R_TLS_IE)
                {
                    if (target.IsExternal() && target.IsElf())
                    {
                        r.Done = false;
                        if (r.Sym == null)
                        {
                            r.Sym = syms.Tlsg;
                        }

                        r.Xsym = r.Sym;
                        r.Xadd = r.Add;
                        o = 0L;
                        if (!target.IsAMD64())
                        {
                            o = r.Add;
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

                        thearch.TLSIEtoLE(s.P, int(off), int(r.Siz));
                        o = int64(syms.Tlsoffset); 
                        // TODO: o += r.Add when !target.IsAmd64()?
                        // Why do we treat r.Add differently on AMD64?
                        // Is the external linker using Xadd at all?
                    }
                    else
                    {
                        log.Fatalf("cannot handle R_TLS_IE (sym %s) when linking internally", s.Name);
                    }

                    goto __switch_break0;
                }
                if (r.Type == objabi.R_ADDR)
                {
                    if (target.IsExternal() && r.Sym.Type != sym.SCONST)
                    {
                        r.Done = false; 

                        // set up addend for eventual relocation via outer symbol.
                        var rs = r.Sym;

                        r.Xadd = r.Add;
                        while (rs.Outer != null)
                        {
                            r.Xadd += Symaddr(rs) - Symaddr(rs.Outer);
                            rs = rs.Outer;
                        }


                        if (rs.Type != sym.SHOSTOBJ && rs.Type != sym.SDYNIMPORT && rs.Type != sym.SUNDEFEXT && rs.Sect == null)
                        {
                            Errorf(s, "missing section for relocation target %s", rs.Name);
                        }

                        r.Xsym = rs;

                        o = r.Xadd;
                        if (target.IsElf())
                        {
                            if (target.IsAMD64())
                            {
                                o = 0L;
                            }

                        }
                        else if (target.IsDarwin())
                        {
                            if (rs.Type != sym.SHOSTOBJ)
                            {
                                o += Symaddr(rs);
                            }

                        }
                        else if (target.IsWindows())
                        { 
                            // nothing to do
                        }
                        else if (target.IsAIX())
                        {
                            o = Symaddr(r.Sym) + r.Add;
                        }
                        else
                        {
                            Errorf(s, "unhandled pcrel relocation to %s on %v", rs.Name, target.HeadType);
                        }

                        break;

                    } 

                    // On AIX, a second relocation must be done by the loader,
                    // as section addresses can change once loaded.
                    // The "default" symbol address is still needed by the loader so
                    // the current relocation can't be skipped.
                    if (target.IsAIX() && r.Sym.Type != sym.SDYNIMPORT)
                    { 
                        // It's not possible to make a loader relocation in a
                        // symbol which is not inside .data section.
                        // FIXME: It should be forbidden to have R_ADDR from a
                        // symbol which isn't in .data. However, as .text has the
                        // same address once loaded, this is possible.
                        if (s.Sect.Seg == _addr_Segdata)
                        {
                            Xcoffadddynrel(target, ldr, s, r);
                        }

                    }

                    o = Symaddr(r.Sym) + r.Add; 

                    // On amd64, 4-byte offsets will be sign-extended, so it is impossible to
                    // access more than 2GB of static data; fail at link time is better than
                    // fail at runtime. See https://golang.org/issue/7980.
                    // Instead of special casing only amd64, we treat this as an error on all
                    // 64-bit architectures so as to be future-proof.
                    if (int32(o) < 0L && target.Arch.PtrSize > 4L && siz == 4L)
                    {
                        Errorf(s, "non-pc-relative relocation address for %s is too big: %#x (%#x + %#x)", r.Sym.Name, uint64(o), Symaddr(r.Sym), r.Add);
                        errorexit();
                    }

                    goto __switch_break0;
                }
                if (r.Type == objabi.R_DWARFSECREF)
                {
                    if (r.Sym.Sect == null)
                    {
                        Errorf(s, "missing DWARF section for relocation target %s", r.Sym.Name);
                    }

                    if (target.IsExternal())
                    {
                        r.Done = false; 

                        // On most platforms, the external linker needs to adjust DWARF references
                        // as it combines DWARF sections. However, on Darwin, dsymutil does the
                        // DWARF linking, and it understands how to follow section offsets.
                        // Leaving in the relocation records confuses it (see
                        // https://golang.org/issue/22068) so drop them for Darwin.
                        if (target.IsDarwin())
                        {
                            r.Done = true;
                        } 

                        // PE code emits IMAGE_REL_I386_SECREL and IMAGE_REL_AMD64_SECREL
                        // for R_DWARFSECREF relocations, while R_ADDR is replaced with
                        // IMAGE_REL_I386_DIR32, IMAGE_REL_AMD64_ADDR64 and IMAGE_REL_AMD64_ADDR32.
                        // Do not replace R_DWARFSECREF with R_ADDR for windows -
                        // let PE code emit correct relocations.
                        if (!target.IsWindows())
                        {
                            r.Type = objabi.R_ADDR;
                        }

                        r.Xsym = r.Sym.Sect.Sym;
                        r.Xadd = r.Add + Symaddr(r.Sym) - int64(r.Sym.Sect.Vaddr);

                        o = r.Xadd;
                        if (target.IsElf() && target.IsAMD64())
                        {
                            o = 0L;
                        }

                        break;

                    }

                    o = Symaddr(r.Sym) + r.Add - int64(r.Sym.Sect.Vaddr);
                    goto __switch_break0;
                }
                if (r.Type == objabi.R_WEAKADDROFF)
                {
                    if (!r.Sym.Attr.Reachable())
                    {
                        continue;
                    }

                    fallthrough = true;
                }
                if (fallthrough || r.Type == objabi.R_ADDROFF) 
                {
                    // The method offset tables using this relocation expect the offset to be relative
                    // to the start of the first text section, even if there are multiple.
                    if (r.Sym.Sect.Name == ".text")
                    {
                        o = Symaddr(r.Sym) - int64(Segtext.Sections[0L].Vaddr) + r.Add;
                    }
                    else
                    {
                        o = Symaddr(r.Sym) - int64(r.Sym.Sect.Vaddr) + r.Add;
                    }

                    goto __switch_break0;
                }
                if (r.Type == objabi.R_ADDRCUOFF) 
                {
                    // debug_range and debug_loc elements use this relocation type to get an
                    // offset from the start of the compile unit.
                    var u = ldr.SymUnit(loader.Sym(r.Sym.SymIdx));
                    o = Symaddr(r.Sym) + r.Add - Symaddr(ldr.Syms[u.Textp2[0L]]); 

                    // r->sym can be null when CALL $(constant) is transformed from absolute PC to relative PC call.
                    goto __switch_break0;
                }
                if (r.Type == objabi.R_GOTPCREL)
                {
                    if (target.IsDynlinkingGo() && target.IsDarwin() && r.Sym != null && r.Sym.Type != sym.SCONST)
                    {
                        r.Done = false;
                        r.Xadd = r.Add;
                        r.Xadd -= int64(r.Siz); // relative to address after the relocated chunk
                        r.Xsym = r.Sym;

                        o = r.Xadd;
                        o += int64(r.Siz);
                        break;

                    }

                    fallthrough = true;
                }
                if (fallthrough || r.Type == objabi.R_CALL || r.Type == objabi.R_PCREL)
                {
                    if (target.IsExternal() && r.Sym != null && r.Sym.Type == sym.SUNDEFEXT)
                    { 
                        // pass through to the external linker.
                        r.Done = false;
                        r.Xadd = 0L;
                        if (target.IsElf())
                        {
                            r.Xadd -= int64(r.Siz);
                        }

                        r.Xsym = r.Sym;
                        o = 0L;
                        break;

                    }

                    if (target.IsExternal() && r.Sym != null && r.Sym.Type != sym.SCONST && (r.Sym.Sect != s.Sect || r.Type == objabi.R_GOTPCREL))
                    {
                        r.Done = false; 

                        // set up addend for eventual relocation via outer symbol.
                        rs = r.Sym;

                        r.Xadd = r.Add;
                        while (rs.Outer != null)
                        {
                            r.Xadd += Symaddr(rs) - Symaddr(rs.Outer);
                            rs = rs.Outer;
                        }


                        r.Xadd -= int64(r.Siz); // relative to address after the relocated chunk
                        if (rs.Type != sym.SHOSTOBJ && rs.Type != sym.SDYNIMPORT && rs.Sect == null)
                        {
                            Errorf(s, "missing section for relocation target %s", rs.Name);
                        }

                        r.Xsym = rs;

                        o = r.Xadd;
                        if (target.IsElf())
                        {
                            if (target.IsAMD64())
                            {
                                o = 0L;
                            }

                        }
                        else if (target.IsDarwin())
                        {
                            if (r.Type == objabi.R_CALL)
                            {
                                if (target.IsExternal() && rs.Type == sym.SDYNIMPORT)
                                {
                                    if (target.IsAMD64())
                                    { 
                                        // AMD64 dynamic relocations are relative to the end of the relocation.
                                        o += int64(r.Siz);

                                    }

                                }
                                else
                                {
                                    if (rs.Type != sym.SHOSTOBJ)
                                    {
                                        o += int64(uint64(Symaddr(rs)) - rs.Sect.Vaddr);
                                    }

                                    o -= int64(r.Off); // relative to section offset, not symbol
                                }

                            }
                            else
                            {
                                o += int64(r.Siz);
                            }

                        }
                        else if (target.IsWindows() && target.IsAMD64())
                        { // only amd64 needs PCREL
                            // PE/COFF's PC32 relocation uses the address after the relocated
                            // bytes as the base. Compensate by skewing the addend.
                            o += int64(r.Siz);

                        }
                        else
                        {
                            Errorf(s, "unhandled pcrel relocation to %s on %v", rs.Name, target.HeadType);
                        }

                        break;

                    }

                    o = 0L;
                    if (r.Sym != null)
                    {
                        o += Symaddr(r.Sym);
                    }

                    o += r.Add - (s.Value + int64(r.Off) + int64(r.Siz));
                    goto __switch_break0;
                }
                if (r.Type == objabi.R_SIZE)
                {
                    o = r.Sym.Size + r.Add;
                    goto __switch_break0;
                }
                if (r.Type == objabi.R_XCOFFREF)
                {
                    if (!target.IsAIX())
                    {
                        Errorf(s, "find XCOFF R_REF on non-XCOFF files");
                    }

                    if (!target.IsExternal())
                    {
                        Errorf(s, "find XCOFF R_REF with internal linking");
                    }

                    r.Xsym = r.Sym;
                    r.Xadd = r.Add;
                    r.Done = false; 

                    // This isn't a real relocation so it must not update
                    // its offset value.
                    continue;
                    goto __switch_break0;
                }
                if (r.Type == objabi.R_DWARFFILEREF) 
                {
                    // The final file index is saved in r.Add in dwarf.go:writelines.
                    o = r.Add;
                    goto __switch_break0;
                }
                // default: 
                    switch (siz)
                    {
                        case 1L: 
                            o = int64(s.P[off]);
                            break;
                        case 2L: 
                            o = int64(target.Arch.ByteOrder.Uint16(s.P[off..]));
                            break;
                        case 4L: 
                            o = int64(target.Arch.ByteOrder.Uint32(s.P[off..]));
                            break;
                        case 8L: 
                            o = int64(target.Arch.ByteOrder.Uint64(s.P[off..]));
                            break;
                        default: 
                            Errorf(s, "bad reloc size %#x for %s", uint32(siz), r.Sym.Name);
                            break;
                    }
                    {
                        var (offset, ok) = thearch.Archreloc(target, syms, r, s, o);

                        if (ok)
                        {
                            o = offset;
                        }
                        else
                        {
                            Errorf(s, "unknown reloc to %v: %d (%s)", r.Sym.Name, r.Type, sym.RelocName(target.Arch, r.Type));
                        }

                    }


                __switch_break0:;

                if (target.IsPPC64() || target.IsS390X())
                {
                    r.InitExt();
                    if (r.Variant != sym.RV_NONE)
                    {
                        o = thearch.Archrelocvariant(target, syms, r, s, o);
                    }

                }

                if (false)
                {
                    @string nam = "<nil>";
                    long addr = default;
                    if (r.Sym != null)
                    {
                        nam = r.Sym.Name;
                        addr = Symaddr(r.Sym);
                    }

                    @string xnam = "<nil>";
                    if (r.Xsym != null)
                    {
                        xnam = r.Xsym.Name;
                    }

                    fmt.Printf("relocate %s %#x (%#x+%#x, size %d) => %s %#x +%#x (xsym: %s +%#x) [type %d (%s)/%d, %x]\n", s.Name, s.Value + int64(off), s.Value, r.Off, r.Siz, nam, addr, r.Add, xnam, r.Xadd, r.Type, sym.RelocName(target.Arch, r.Type), r.Variant, o);

                }

                switch (siz)
                {
                    case 1L: 
                        s.P[off] = byte(int8(o));
                        break;
                    case 2L: 
                        if (o != int64(int16(o)))
                        {
                            Errorf(s, "relocation address for %s is too big: %#x", r.Sym.Name, o);
                        }

                        var i16 = int16(o);
                        target.Arch.ByteOrder.PutUint16(s.P[off..], uint16(i16));
                        break;
                    case 4L: 
                        if (r.Type == objabi.R_PCREL || r.Type == objabi.R_CALL)
                        {
                            if (o != int64(int32(o)))
                            {
                                Errorf(s, "pc-relative relocation address for %s is too big: %#x", r.Sym.Name, o);
                            }

                        }
                        else
                        {
                            if (o != int64(int32(o)) && o != int64(uint32(o)))
                            {
                                Errorf(s, "non-pc-relative relocation address for %s is too big: %#x", r.Sym.Name, uint64(o));
                            }

                        }

                        var fl = int32(o);
                        target.Arch.ByteOrder.PutUint32(s.P[off..], uint32(fl));
                        break;
                    case 8L: 
                        target.Arch.ByteOrder.PutUint64(s.P[off..], uint64(o));
                        break;
                    default: 
                        Errorf(s, "bad reloc size %#x for %s", uint32(siz), r.Sym.Name);
                        break;
                }

            }


        }

        private static void reloc2(this ptr<Link> _addr_ctxt)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            sync.WaitGroup wg = default;
            var target = _addr_ctxt.Target;
            var ldr = ctxt.loader;
            var reporter = _addr_ctxt.ErrorReporter;
            var syms = _addr_ctxt.ArchSyms;
            wg.Add(3L);
            go_(() => () =>
            {
                if (!ctxt.IsWasm())
                { // On Wasm, text relocations are applied in Asmb2.
                    {
                        var s__prev1 = s;

                        foreach (var (_, __s) in ctxt.Textp)
                        {
                            s = __s;
                            relocsym2(_addr_target, _addr_ldr, _addr_reporter, _addr_syms, _addr_s);
                        }

                        s = s__prev1;
                    }
                }

                wg.Done();

            }());
            go_(() => () =>
            {
                {
                    var s__prev1 = s;

                    foreach (var (_, __s) in ctxt.datap)
                    {
                        s = __s;
                        relocsym2(_addr_target, _addr_ldr, _addr_reporter, _addr_syms, _addr_s);
                    }

                    s = s__prev1;
                }

                wg.Done();

            }());
            go_(() => () =>
            {
                foreach (var (_, si) in dwarfp)
                {
                    {
                        var s__prev2 = s;

                        foreach (var (_, __s) in si.syms)
                        {
                            s = __s;
                            relocsym2(_addr_target, _addr_ldr, _addr_reporter, _addr_syms, _addr_s);
                        }

                        s = s__prev2;
                    }
                }
                wg.Done();

            }());
            wg.Wait();

        }
    }
}}}}
