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

// package ld -- go2cs converted at 2020 October 09 05:51:26 UTC
// import "cmd/oldlink/internal/ld" ==> using ld = go.cmd.oldlink.@internal.ld_package
// Original source: C:\Go\src\cmd\oldlink\internal\ld\data.go
using bufio = go.bufio_package;
using bytes = go.bytes_package;
using gcprog = go.cmd.@internal.gcprog_package;
using objabi = go.cmd.@internal.objabi_package;
using sys = go.cmd.@internal.sys_package;
using sym = go.cmd.oldlink.@internal.sym_package;
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
namespace oldlink {
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
        private static ulong maxSizeTrampolinesPPC64(ptr<sym.Symbol> _addr_s, bool isTramp)
        {
            ref sym.Symbol s = ref _addr_s.val;
 
            // If thearch.Trampoline is nil, then trampoline support is not available on this arch.
            // A trampoline does not need any dependent trampolines.
            if (thearch.Trampoline == null || isTramp)
            {
                return 0L;
            }

            var n = uint64(0L);
            foreach (var (ri) in s.R)
            {
                var r = _addr_s.R[ri];
                if (r.Type.IsDirectCallOrJump())
                {
                    n++;
                }

            } 
            // Trampolines in ppc64 are 4 instructions.
            return n * 16L;

        }

        // detect too-far jumps in function s, and add trampolines if necessary
        // ARM, PPC64 & PPC64LE support trampoline insertion for internal and external linking
        // On PPC64 & PPC64LE the text sections might be split but will still insert trampolines
        // where necessary.
        private static void trampoline(ptr<Link> _addr_ctxt, ptr<sym.Symbol> _addr_s)
        {
            ref Link ctxt = ref _addr_ctxt.val;
            ref sym.Symbol s = ref _addr_s.val;

            if (thearch.Trampoline == null)
            {
                return ; // no need or no support of trampolines on this arch
            }

            foreach (var (ri) in s.R)
            {
                var r = _addr_s.R[ri];
                if (!r.Type.IsDirectCallOrJump())
                {
                    continue;
                }

                if (Symaddr(r.Sym) == 0L && (r.Sym.Type != sym.SDYNIMPORT && r.Sym.Type != sym.SUNDEFEXT))
                {
                    if (r.Sym.File != s.File)
                    {
                        if (!isRuntimeDepPkg(s.File) || !isRuntimeDepPkg(r.Sym.File))
                        {
                            ctxt.ErrorUnresolved(s, r);
                        } 
                        // runtime and its dependent packages may call to each other.
                        // they are fine, as they will be laid down together.
                    }

                    continue;

                }

                thearch.Trampoline(ctxt, r, s);

            }

        }

        // relocsym resolve relocations in "s". The main loop walks through
        // the list of relocations attached to "s" and resolves them where
        // applicable. Relocations are often architecture-specific, requiring
        // calls into the 'archreloc' and/or 'archrelocvariant' functions for
        // the architecture. When external linking is in effect, it may not be
        // possible to completely resolve the address/offset for a symbol, in
        // which case the goal is to lay the groundwork for turning a given
        // relocation into an external reloc (to be applied by the external
        // linker). For more on how relocations work in general, see
        //
        //  "Linkers and Loaders", by John R. Levine (Morgan Kaufmann, 1999), ch. 7
        //
        // This is a performance-critical function for the linker; be careful
        // to avoid introducing unnecessary allocations in the main loop.
        private static void relocsym(ptr<Link> _addr_ctxt, ptr<sym.Symbol> _addr_s)
        {
            ref Link ctxt = ref _addr_ctxt.val;
            ref sym.Symbol s = ref _addr_s.val;

            if (len(s.R) == 0L)
            {
                return ;
            }

            if (s.Attr.ReadOnly())
            { 
                // The symbol's content is backed by read-only memory.
                // Copy it to writable memory to apply relocations.
                s.P = append((slice<byte>)null, s.P);
                s.Attr.Set(sym.AttrReadOnly, false);

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
                    if (ctxt.BuildMode == BuildModeShared || ctxt.BuildMode == BuildModePlugin)
                    {
                        if (r.Sym.Name == "main.main" || (ctxt.BuildMode != BuildModePlugin && r.Sym.Name == "main..inittask"))
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
                        ctxt.ErrorUnresolved(s, r);
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
                if (ctxt.HeadType != objabi.Hsolaris && ctxt.HeadType != objabi.Hdarwin && ctxt.HeadType != objabi.Haix && r.Sym != null && r.Sym.Type == sym.SDYNIMPORT && !ctxt.DynlinkingGo() && !r.Sym.Attr.SubSymbol())
                {
                    if (!(ctxt.Arch.Family == sys.PPC64 && ctxt.LinkMode == LinkExternal && r.Sym.Name == ".TOC."))
                    {
                        Errorf(s, "unhandled relocation for %s (type %d (%s) rtype %d (%s))", r.Sym.Name, r.Sym.Type, r.Sym.Type, r.Type, sym.RelocName(ctxt.Arch, r.Type));
                    }

                }

                if (r.Sym != null && r.Sym.Type != sym.STLSBSS && r.Type != objabi.R_WEAKADDROFF && !r.Sym.Attr.Reachable())
                {
                    Errorf(s, "unreachable sym in relocation: %s", r.Sym.Name);
                }

                if (ctxt.LinkMode == LinkExternal)
                {
                    r.InitExt();
                } 

                // TODO(mundaym): remove this special case - see issue 14218.
                if (ctxt.Arch.Family == sys.S390X)
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
                    if (ctxt.LinkMode == LinkExternal && ctxt.IsELF)
                    {
                        r.Done = false;
                        if (r.Sym == null)
                        {
                            r.Sym = ctxt.Tlsg;
                        }

                        r.Xsym = r.Sym;
                        r.Xadd = r.Add;
                        o = 0L;
                        if (ctxt.Arch.Family != sys.AMD64)
                        {
                            o = r.Add;
                        }

                        break;

                    }

                    if (ctxt.IsELF && ctxt.Arch.Family == sys.ARM)
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
                    else if (ctxt.IsELF || ctxt.HeadType == objabi.Hplan9 || ctxt.HeadType == objabi.Hdarwin)
                    {
                        o = int64(ctxt.Tlsoffset) + r.Add;
                    }
                    else if (ctxt.HeadType == objabi.Hwindows)
                    {
                        o = r.Add;
                    }
                    else
                    {
                        log.Fatalf("unexpected R_TLS_LE relocation for %v", ctxt.HeadType);
                    }

                    goto __switch_break0;
                }
                if (r.Type == objabi.R_TLS_IE)
                {
                    if (ctxt.LinkMode == LinkExternal && ctxt.IsELF)
                    {
                        r.Done = false;
                        if (r.Sym == null)
                        {
                            r.Sym = ctxt.Tlsg;
                        }

                        r.Xsym = r.Sym;
                        r.Xadd = r.Add;
                        o = 0L;
                        if (ctxt.Arch.Family != sys.AMD64)
                        {
                            o = r.Add;
                        }

                        break;

                    }

                    if (ctxt.BuildMode == BuildModePIE && ctxt.IsELF)
                    { 
                        // We are linking the final executable, so we
                        // can optimize any TLS IE relocation to LE.
                        if (thearch.TLSIEtoLE == null)
                        {
                            log.Fatalf("internal linking of TLS IE not supported on %v", ctxt.Arch.Family);
                        }

                        thearch.TLSIEtoLE(s, int(off), int(r.Siz));
                        o = int64(ctxt.Tlsoffset); 
                        // TODO: o += r.Add when ctxt.Arch.Family != sys.AMD64?
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
                    if (ctxt.LinkMode == LinkExternal && r.Sym.Type != sym.SCONST)
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
                        if (ctxt.IsELF)
                        {
                            if (ctxt.Arch.Family == sys.AMD64)
                            {
                                o = 0L;
                            }

                        }
                        else if (ctxt.HeadType == objabi.Hdarwin)
                        {
                            if (rs.Type != sym.SHOSTOBJ)
                            {
                                o += Symaddr(rs);
                            }

                        }
                        else if (ctxt.HeadType == objabi.Hwindows)
                        { 
                            // nothing to do
                        }
                        else if (ctxt.HeadType == objabi.Haix)
                        {
                            o = Symaddr(r.Sym) + r.Add;
                        }
                        else
                        {
                            Errorf(s, "unhandled pcrel relocation to %s on %v", rs.Name, ctxt.HeadType);
                        }

                        break;

                    } 

                    // On AIX, a second relocation must be done by the loader,
                    // as section addresses can change once loaded.
                    // The "default" symbol address is still needed by the loader so
                    // the current relocation can't be skipped.
                    if (ctxt.HeadType == objabi.Haix && r.Sym.Type != sym.SDYNIMPORT)
                    { 
                        // It's not possible to make a loader relocation in a
                        // symbol which is not inside .data section.
                        // FIXME: It should be forbidden to have R_ADDR from a
                        // symbol which isn't in .data. However, as .text has the
                        // same address once loaded, this is possible.
                        if (s.Sect.Seg == _addr_Segdata)
                        {
                            Xcoffadddynrel(ctxt, s, r);
                        }

                    }

                    o = Symaddr(r.Sym) + r.Add; 

                    // On amd64, 4-byte offsets will be sign-extended, so it is impossible to
                    // access more than 2GB of static data; fail at link time is better than
                    // fail at runtime. See https://golang.org/issue/7980.
                    // Instead of special casing only amd64, we treat this as an error on all
                    // 64-bit architectures so as to be future-proof.
                    if (int32(o) < 0L && ctxt.Arch.PtrSize > 4L && siz == 4L)
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

                    if (ctxt.LinkMode == LinkExternal)
                    {
                        r.Done = false; 

                        // On most platforms, the external linker needs to adjust DWARF references
                        // as it combines DWARF sections. However, on Darwin, dsymutil does the
                        // DWARF linking, and it understands how to follow section offsets.
                        // Leaving in the relocation records confuses it (see
                        // https://golang.org/issue/22068) so drop them for Darwin.
                        if (ctxt.HeadType == objabi.Hdarwin)
                        {
                            r.Done = true;
                        } 

                        // PE code emits IMAGE_REL_I386_SECREL and IMAGE_REL_AMD64_SECREL
                        // for R_DWARFSECREF relocations, while R_ADDR is replaced with
                        // IMAGE_REL_I386_DIR32, IMAGE_REL_AMD64_ADDR64 and IMAGE_REL_AMD64_ADDR32.
                        // Do not replace R_DWARFSECREF with R_ADDR for windows -
                        // let PE code emit correct relocations.
                        if (ctxt.HeadType != objabi.Hwindows)
                        {
                            r.Type = objabi.R_ADDR;
                        }

                        r.Xsym = ctxt.Syms.ROLookup(r.Sym.Sect.Name, 0L);
                        r.Xadd = r.Add + Symaddr(r.Sym) - int64(r.Sym.Sect.Vaddr);

                        o = r.Xadd;
                        if (ctxt.IsELF && ctxt.Arch.Family == sys.AMD64)
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
                    o = Symaddr(r.Sym) + r.Add - Symaddr(r.Sym.Unit.Textp[0L]); 

                    // r->sym can be null when CALL $(constant) is transformed from absolute PC to relative PC call.
                    goto __switch_break0;
                }
                if (r.Type == objabi.R_GOTPCREL)
                {
                    if (ctxt.DynlinkingGo() && ctxt.HeadType == objabi.Hdarwin && r.Sym != null && r.Sym.Type != sym.SCONST)
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
                    if (ctxt.LinkMode == LinkExternal && r.Sym != null && r.Sym.Type == sym.SUNDEFEXT)
                    { 
                        // pass through to the external linker.
                        r.Done = false;
                        r.Xadd = 0L;
                        if (ctxt.IsELF)
                        {
                            r.Xadd -= int64(r.Siz);
                        }

                        r.Xsym = r.Sym;
                        o = 0L;
                        break;

                    }

                    if (ctxt.LinkMode == LinkExternal && r.Sym != null && r.Sym.Type != sym.SCONST && (r.Sym.Sect != s.Sect || r.Type == objabi.R_GOTPCREL))
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
                        if (ctxt.IsELF)
                        {
                            if (ctxt.Arch.Family == sys.AMD64)
                            {
                                o = 0L;
                            }

                        }
                        else if (ctxt.HeadType == objabi.Hdarwin)
                        {
                            if (r.Type == objabi.R_CALL)
                            {
                                if (ctxt.LinkMode == LinkExternal && rs.Type == sym.SDYNIMPORT)
                                {
                                    if (ctxt.Arch.Family == sys.AMD64)
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
                        else if (ctxt.HeadType == objabi.Hwindows && ctxt.Arch.Family == sys.AMD64)
                        { // only amd64 needs PCREL
                            // PE/COFF's PC32 relocation uses the address after the relocated
                            // bytes as the base. Compensate by skewing the addend.
                            o += int64(r.Siz);

                        }
                        else
                        {
                            Errorf(s, "unhandled pcrel relocation to %s on %v", rs.Name, ctxt.HeadType);
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
                    if (ctxt.HeadType != objabi.Haix)
                    {
                        Errorf(s, "find XCOFF R_REF on non-XCOFF files");
                    }

                    if (ctxt.LinkMode != LinkExternal)
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
                            o = int64(ctxt.Arch.ByteOrder.Uint16(s.P[off..]));
                            break;
                        case 4L: 
                            o = int64(ctxt.Arch.ByteOrder.Uint32(s.P[off..]));
                            break;
                        case 8L: 
                            o = int64(ctxt.Arch.ByteOrder.Uint64(s.P[off..]));
                            break;
                        default: 
                            Errorf(s, "bad reloc size %#x for %s", uint32(siz), r.Sym.Name);
                            break;
                    }
                    {
                        var (offset, ok) = thearch.Archreloc(ctxt, r, s, o);

                        if (ok)
                        {
                            o = offset;
                        }
                        else
                        {
                            Errorf(s, "unknown reloc to %v: %d (%s)", r.Sym.Name, r.Type, sym.RelocName(ctxt.Arch, r.Type));
                        }

                    }


                __switch_break0:;

                if (ctxt.Arch.Family == sys.PPC64 || ctxt.Arch.Family == sys.S390X)
                {
                    r.InitExt();
                    if (r.Variant != sym.RV_NONE)
                    {
                        o = thearch.Archrelocvariant(ctxt, r, s, o);
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

                    fmt.Printf("relocate %s %#x (%#x+%#x, size %d) => %s %#x +%#x (xsym: %s +%#x) [type %d (%s)/%d, %x]\n", s.Name, s.Value + int64(off), s.Value, r.Off, r.Siz, nam, addr, r.Add, xnam, r.Xadd, r.Type, sym.RelocName(ctxt.Arch, r.Type), r.Variant, o);

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
                        ctxt.Arch.ByteOrder.PutUint16(s.P[off..], uint16(i16));
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
                        ctxt.Arch.ByteOrder.PutUint32(s.P[off..], uint32(fl));
                        break;
                    case 8L: 
                        ctxt.Arch.ByteOrder.PutUint64(s.P[off..], uint64(o));
                        break;
                    default: 
                        Errorf(s, "bad reloc size %#x for %s", uint32(siz), r.Sym.Name);
                        break;
                }

            }


        }

        private static void reloc(this ptr<Link> _addr_ctxt)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            {
                var s__prev1 = s;

                foreach (var (_, __s) in ctxt.Textp)
                {
                    s = __s;
                    relocsym(_addr_ctxt, _addr_s);
                }

                s = s__prev1;
            }

            {
                var s__prev1 = s;

                foreach (var (_, __s) in datap)
                {
                    s = __s;
                    relocsym(_addr_ctxt, _addr_s);
                }

                s = s__prev1;
            }

            {
                var s__prev1 = s;

                foreach (var (_, __s) in dwarfp)
                {
                    s = __s;
                    relocsym(_addr_ctxt, _addr_s);
                }

                s = s__prev1;
            }
        }

        private static void windynrelocsym(ptr<Link> _addr_ctxt, ptr<sym.Symbol> _addr_rel, ptr<sym.Symbol> _addr_s)
        {
            ref Link ctxt = ref _addr_ctxt.val;
            ref sym.Symbol rel = ref _addr_rel.val;
            ref sym.Symbol s = ref _addr_s.val;

            foreach (var (ri) in s.R)
            {
                var r = _addr_s.R[ri];
                var targ = r.Sym;
                if (targ == null)
                {
                    continue;
                }

                if (!targ.Attr.Reachable())
                {
                    if (r.Type == objabi.R_WEAKADDROFF)
                    {
                        continue;
                    }

                    Errorf(s, "dynamic relocation to unreachable symbol %s", targ.Name);

                }

                if (r.Sym.Plt() == -2L && r.Sym.Got() != -2L)
                { // make dynimport JMP table for PE object files.
                    targ.SetPlt(int32(rel.Size));
                    r.Sym = rel;
                    r.Add = int64(targ.Plt()); 

                    // jmp *addr

                    if (ctxt.Arch.Family == sys.I386) 
                        rel.AddUint8(0xffUL);
                        rel.AddUint8(0x25UL);
                        rel.AddAddr(ctxt.Arch, targ);
                        rel.AddUint8(0x90UL);
                        rel.AddUint8(0x90UL);
                    else if (ctxt.Arch.Family == sys.AMD64) 
                        rel.AddUint8(0xffUL);
                        rel.AddUint8(0x24UL);
                        rel.AddUint8(0x25UL);
                        rel.AddAddrPlus4(targ, 0L);
                        rel.AddUint8(0x90UL);
                    else 
                        Errorf(s, "unsupported arch %v", ctxt.Arch.Family);
                        return ;
                    
                }
                else if (r.Sym.Plt() >= 0L)
                {
                    r.Sym = rel;
                    r.Add = int64(targ.Plt());
                }

            }

        }

        // windynrelocsyms generates jump table to C library functions that will be
        // added later. windynrelocsyms writes the table into .rel symbol.
        private static void windynrelocsyms(this ptr<Link> _addr_ctxt)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            if (!(ctxt.HeadType == objabi.Hwindows && iscgo && ctxt.LinkMode == LinkInternal))
            {
                return ;
            } 

            /* relocation table */
            var rel = ctxt.Syms.Lookup(".rel", 0L);
            rel.Attr |= sym.AttrReachable;
            rel.Type = sym.STEXT;
            ctxt.Textp = append(ctxt.Textp, rel);

            foreach (var (_, s) in ctxt.Textp)
            {
                if (s == rel)
                {
                    continue;
                }

                windynrelocsym(_addr_ctxt, _addr_rel, _addr_s);

            }

        }

        private static void dynrelocsym(ptr<Link> _addr_ctxt, ptr<sym.Symbol> _addr_s)
        {
            ref Link ctxt = ref _addr_ctxt.val;
            ref sym.Symbol s = ref _addr_s.val;

            foreach (var (ri) in s.R)
            {
                var r = _addr_s.R[ri];
                if (ctxt.BuildMode == BuildModePIE && ctxt.LinkMode == LinkInternal)
                { 
                    // It's expected that some relocations will be done
                    // later by relocsym (R_TLS_LE, R_ADDROFF), so
                    // don't worry if Adddynrel returns false.
                    thearch.Adddynrel(ctxt, s, r);
                    continue;

                }

                if (r.Sym != null && r.Sym.Type == sym.SDYNIMPORT || r.Type >= objabi.ElfRelocOffset)
                {
                    if (r.Sym != null && !r.Sym.Attr.Reachable())
                    {
                        Errorf(s, "dynamic relocation to unreachable symbol %s", r.Sym.Name);
                    }

                    if (!thearch.Adddynrel(ctxt, s, r))
                    {
                        Errorf(s, "unsupported dynamic relocation for symbol %s (type=%d (%s) stype=%d (%s))", r.Sym.Name, r.Type, sym.RelocName(ctxt.Arch, r.Type), r.Sym.Type, r.Sym.Type);
                    }

                }

            }

        }

        private static void dynreloc(ptr<Link> _addr_ctxt, ptr<array<slice<ptr<sym.Symbol>>>> _addr_data)
        {
            ref Link ctxt = ref _addr_ctxt.val;
            ref array<slice<ptr<sym.Symbol>>> data = ref _addr_data.val;

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

                foreach (var (_, __s) in ctxt.Textp)
                {
                    s = __s;
                    dynrelocsym(_addr_ctxt, _addr_s);
                }

                s = s__prev1;
            }

            foreach (var (_, syms) in data)
            {
                {
                    var s__prev2 = s;

                    foreach (var (_, __s) in syms)
                    {
                        s = __s;
                        dynrelocsym(_addr_ctxt, _addr_s);
                    }

                    s = s__prev2;
                }
            }
            if (ctxt.IsELF)
            {
                elfdynhash(ctxt);
            }

        }

        public static void Codeblk(ptr<Link> _addr_ctxt, long addr, long size)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            CodeblkPad(_addr_ctxt, addr, size, zeros[..]);
        }
        public static void CodeblkPad(ptr<Link> _addr_ctxt, long addr, long size, slice<byte> pad)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            if (flagA.val)
            {
                ctxt.Logf("codeblk [%#x,%#x) at offset %#x\n", addr, addr + size, ctxt.Out.Offset());
            }

            blk(_addr_ctxt.Out, ctxt.Textp, addr, size, pad); 

            /* again for printing */
            if (!flagA.val)
            {
                return ;
            }

            var syms = ctxt.Textp;
            {
                var s__prev1 = s;

                foreach (var (__i, __s) in syms)
                {
                    i = __i;
                    s = __s;
                    if (!s.Attr.Reachable())
                    {
                        continue;
                    }

                    if (s.Value >= addr)
                    {
                        syms = syms[i..];
                        break;
                    }

                }

                s = s__prev1;
            }

            var eaddr = addr + size;
            {
                var s__prev1 = s;

                foreach (var (_, __s) in syms)
                {
                    s = __s;
                    if (!s.Attr.Reachable())
                    {
                        continue;
                    }

                    if (s.Value >= eaddr)
                    {
                        break;
                    }

                    if (addr < s.Value)
                    {
                        ctxt.Logf("%-20s %.8x|", "_", uint64(addr));
                        while (addr < s.Value)
                        {
                            ctxt.Logf(" %.2x", 0L);
                            addr++;
                        }

                        ctxt.Logf("\n");

                    }

                    ctxt.Logf("%.6x\t%-20s\n", uint64(addr), s.Name);
                    var q = s.P;

                    while (len(q) >= 16L)
                    {
                        ctxt.Logf("%.6x\t% x\n", uint64(addr), q[..16L]);
                        addr += 16L;
                        q = q[16L..];
                    }


                    if (len(q) > 0L)
                    {
                        ctxt.Logf("%.6x\t% x\n", uint64(addr), q);
                        addr += int64(len(q));
                    }

                }

                s = s__prev1;
            }

            if (addr < eaddr)
            {
                ctxt.Logf("%-20s %.8x|", "_", uint64(addr));
                while (addr < eaddr)
                {
                    ctxt.Logf(" %.2x", 0L);
                    addr++;
                }


            }

        }

        private static void blk(ptr<OutBuf> _addr_@out, slice<ptr<sym.Symbol>> syms, long addr, long size, slice<byte> pad)
        {
            ref OutBuf @out = ref _addr_@out.val;

            {
                var s__prev1 = s;

                foreach (var (__i, __s) in syms)
                {
                    i = __i;
                    s = __s;
                    if (!s.Attr.SubSymbol() && s.Value >= addr)
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
                    if (s.Attr.SubSymbol())
                    {
                        continue;
                    }

                    if (s.Value >= eaddr)
                    {
                        break;
                    }

                    if (s.Value < addr)
                    {
                        Errorf(s, "phase error: addr=%#x but sym=%#x type=%d", addr, s.Value, s.Type);
                        errorexit();
                    }

                    if (addr < s.Value)
                    {
                        @out.WriteStringPad("", int(s.Value - addr), pad);
                        addr = s.Value;
                    }

                    @out.WriteSym(s);
                    addr += int64(len(s.P));
                    if (addr < s.Value + s.Size)
                    {
                        @out.WriteStringPad("", int(s.Value + s.Size - addr), pad);
                        addr = s.Value + s.Size;
                    }

                    if (addr != s.Value + s.Size)
                    {
                        Errorf(s, "phase error: addr=%#x value+size=%#x", addr, s.Value + s.Size);
                        errorexit();
                    }

                    if (s.Value + s.Size >= eaddr)
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

            @out.Flush();

        }

        public static void Datblk(ptr<Link> _addr_ctxt, long addr, long size)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            writeDatblkToOutBuf(_addr_ctxt, _addr_ctxt.Out, addr, size);
        }

        // Used only on Wasm for now.
        public static slice<byte> DatblkBytes(ptr<Link> _addr_ctxt, long addr, long size)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            var buf = bytes.NewBuffer(make_slice<byte>(0L, size));
            ptr<OutBuf> @out = addr(new OutBuf(w:bufio.NewWriter(buf)));
            writeDatblkToOutBuf(_addr_ctxt, out, addr, size);
            @out.Flush();
            return buf.Bytes();
        }

        private static void writeDatblkToOutBuf(ptr<Link> _addr_ctxt, ptr<OutBuf> _addr_@out, long addr, long size)
        {
            ref Link ctxt = ref _addr_ctxt.val;
            ref OutBuf @out = ref _addr_@out.val;

            if (flagA.val)
            {
                ctxt.Logf("datblk [%#x,%#x) at offset %#x\n", addr, addr + size, ctxt.Out.Offset());
            }

            blk(_addr_out, datap, addr, size, zeros[..]); 

            /* again for printing */
            if (!flagA.val)
            {
                return ;
            }

            var syms = datap;
            {
                var i__prev1 = i;
                var sym__prev1 = sym;

                foreach (var (__i, __sym) in syms)
                {
                    i = __i;
                    sym = __sym;
                    if (sym.Value >= addr)
                    {
                        syms = syms[i..];
                        break;
                    }

                }

                i = i__prev1;
                sym = sym__prev1;
            }

            var eaddr = addr + size;
            {
                var sym__prev1 = sym;

                foreach (var (_, __sym) in syms)
                {
                    sym = __sym;
                    if (sym.Value >= eaddr)
                    {
                        break;
                    }

                    if (addr < sym.Value)
                    {
                        ctxt.Logf("\t%.8x| 00 ...\n", uint64(addr));
                        addr = sym.Value;
                    }

                    ctxt.Logf("%s\n\t%.8x|", sym.Name, uint64(addr));
                    {
                        var i__prev2 = i;

                        foreach (var (__i, __b) in sym.P)
                        {
                            i = __i;
                            b = __b;
                            if (i > 0L && i % 16L == 0L)
                            {
                                ctxt.Logf("\n\t%.8x|", uint64(addr) + uint64(i));
                            }

                            ctxt.Logf(" %.2x", b);

                        }

                        i = i__prev2;
                    }

                    addr += int64(len(sym.P));
                    while (addr < sym.Value + sym.Size)
                    {
                        ctxt.Logf(" %.2x", 0L);
                        addr++;
                    }

                    ctxt.Logf("\n");

                    if (ctxt.LinkMode != LinkExternal)
                    {
                        continue;
                    }

                    {
                        var i__prev2 = i;

                        foreach (var (__i) in sym.R)
                        {
                            i = __i;
                            var r = _addr_sym.R[i]; // Copying sym.Reloc has measurable impact on performance
                            @string rsname = "";
                            var rsval = int64(0L);
                            if (r.Sym != null)
                            {
                                rsname = r.Sym.Name;
                                rsval = r.Sym.Value;
                            }

                            @string typ = "?";

                            if (r.Type == objabi.R_ADDR) 
                                typ = "addr";
                            else if (r.Type == objabi.R_PCREL) 
                                typ = "pcrel";
                            else if (r.Type == objabi.R_CALL) 
                                typ = "call";
                                                        ctxt.Logf("\treloc %.8x/%d %s %s+%#x [%#x]\n", uint(sym.Value + int64(r.Off)), r.Siz, typ, rsname, r.Add, rsval + r.Add);

                        }

                        i = i__prev2;
                    }
                }

                sym = sym__prev1;
            }

            if (addr < eaddr)
            {
                ctxt.Logf("\t%.8x| 00 ...\n", uint(addr));
            }

            ctxt.Logf("\t%.8x|\n", uint(eaddr));

        }

        public static void Dwarfblk(ptr<Link> _addr_ctxt, long addr, long size)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            if (flagA.val)
            {
                ctxt.Logf("dwarfblk [%#x,%#x) at offset %#x\n", addr, addr + size, ctxt.Out.Offset());
            }

            blk(_addr_ctxt.Out, dwarfp, addr, size, zeros[..]);

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
        private static void addstrdata(ptr<Link> _addr_ctxt, @string name, @string value)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            var s = ctxt.Syms.ROLookup(name, 0L);
            if (s == null || s.Gotype == null)
            { 
                // Not defined in the loaded packages.
                return ;

            }

            if (s.Gotype.Name != "type.string")
            {
                Errorf(s, "cannot set with -X: not a var of type string (%s)", s.Gotype.Name);
                return ;
            }

            if (s.Type == sym.SBSS)
            {
                s.Type = sym.SDATA;
            }

            var p = fmt.Sprintf("%s.str", s.Name);
            var sp = ctxt.Syms.Lookup(p, 0L);

            Addstring(_addr_sp, value);
            sp.Type = sym.SRODATA;

            s.Size = 0L;
            s.P = s.P[..0L];
            if (s.Attr.ReadOnly())
            {
                s.P = make_slice<byte>(0L, ctxt.Arch.PtrSize * 2L);
                s.Attr.Set(sym.AttrReadOnly, false);
            }

            s.R = s.R[..0L];
            var reachable = s.Attr.Reachable();
            s.AddAddr(ctxt.Arch, sp);
            s.AddUint(ctxt.Arch, uint64(len(value))); 

            // addstring, addaddr, etc., mark the symbols as reachable.
            // In this case that is not necessarily true, so stick to what
            // we know before entering this function.
            s.Attr.Set(sym.AttrReachable, reachable);

            sp.Attr.Set(sym.AttrReachable, reachable);

        }

        private static void dostrdata(this ptr<Link> _addr_ctxt)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            foreach (var (_, name) in strnames)
            {
                addstrdata(_addr_ctxt, name, strdata[name]);
            }

        }

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

        // addgostring adds str, as a Go string value, to s. symname is the name of the
        // symbol used to define the string data and must be unique per linked object.
        private static void addgostring(ptr<Link> _addr_ctxt, ptr<sym.Symbol> _addr_s, @string symname, @string str)
        {
            ref Link ctxt = ref _addr_ctxt.val;
            ref sym.Symbol s = ref _addr_s.val;

            var sdata = ctxt.Syms.Lookup(symname, 0L);
            if (sdata.Type != sym.Sxxx)
            {
                Errorf(s, "duplicate symname in addgostring: %s", symname);
            }

            sdata.Attr |= sym.AttrReachable;
            sdata.Attr |= sym.AttrLocal;
            sdata.Type = sym.SRODATA;
            sdata.Size = int64(len(str));
            sdata.P = (slice<byte>)str;
            s.AddAddr(ctxt.Arch, sdata);
            s.AddUint(ctxt.Arch, uint64(len(str)));

        }

        private static void addinitarrdata(ptr<Link> _addr_ctxt, ptr<sym.Symbol> _addr_s)
        {
            ref Link ctxt = ref _addr_ctxt.val;
            ref sym.Symbol s = ref _addr_s.val;

            var p = s.Name + ".ptr";
            var sp = ctxt.Syms.Lookup(p, 0L);
            sp.Type = sym.SINITARR;
            sp.Size = 0L;
            sp.Attr |= sym.AttrDuplicateOK;
            sp.AddAddr(ctxt.Arch, s);
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

        private static long aligndatsize(long datsize, ptr<sym.Symbol> _addr_s)
        {
            ref sym.Symbol s = ref _addr_s.val;

            return Rnd(datsize, int64(symalign(_addr_s)));
        }

        private static readonly var debugGCProg = false;



        public partial struct GCProg
        {
            public ptr<Link> ctxt;
            public ptr<sym.Symbol> sym;
            public gcprog.Writer w;
        }

        private static void Init(this ptr<GCProg> _addr_p, ptr<Link> _addr_ctxt, @string name)
        {
            ref GCProg p = ref _addr_p.val;
            ref Link ctxt = ref _addr_ctxt.val;

            p.ctxt = ctxt;
            p.sym = ctxt.Syms.Lookup(name, 0L);
            p.w.Init(p.writeByte(ctxt));
            if (debugGCProg)
            {
                fmt.Fprintf(os.Stderr, "ld: start GCProg %s\n", name);
                p.w.Debug(os.Stderr);
            }

        }

        private static Action<byte> writeByte(this ptr<GCProg> _addr_p, ptr<Link> _addr_ctxt)
        {
            ref GCProg p = ref _addr_p.val;
            ref Link ctxt = ref _addr_ctxt.val;

            return x =>
            {
                p.sym.AddUint8(x);
            };

        }

        private static void End(this ptr<GCProg> _addr_p, long size)
        {
            ref GCProg p = ref _addr_p.val;

            p.w.ZeroUntil(size / int64(p.ctxt.Arch.PtrSize));
            p.w.End();
            if (debugGCProg)
            {
                fmt.Fprintf(os.Stderr, "ld: end GCProg\n");
            }

        }

        private static void AddSym(this ptr<GCProg> _addr_p, ptr<sym.Symbol> _addr_s)
        {
            ref GCProg p = ref _addr_p.val;
            ref sym.Symbol s = ref _addr_s.val;

            var typ = s.Gotype; 
            // Things without pointers should be in sym.SNOPTRDATA or sym.SNOPTRBSS;
            // everything we see should have pointers and should therefore have a type.
            if (typ == null)
            {
                switch (s.Name)
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
                Errorf(s, "missing Go type information for global symbol: size %d", s.Size);
                return ;

            }

            var ptrsize = int64(p.ctxt.Arch.PtrSize);
            var nptr = decodetypePtrdata(p.ctxt.Arch, typ.P) / ptrsize;

            if (debugGCProg)
            {
                fmt.Fprintf(os.Stderr, "gcprog sym: %s at %d (ptr=%d+%d)\n", s.Name, s.Value, s.Value / ptrsize, nptr);
            }

            if (decodetypeUsegcprog(p.ctxt.Arch, typ.P) == 0L)
            { 
                // Copy pointers from mask into program.
                var mask = decodetypeGcmask(p.ctxt, typ);
                for (var i = int64(0L); i < nptr; i++)
                {
                    if ((mask[i / 8L] >> (int)(uint(i % 8L))) & 1L != 0L)
                    {
                        p.w.Ptr(s.Value / ptrsize + i);
                    }

                }

                return ;

            } 

            // Copy program.
            var prog = decodetypeGcprog(p.ctxt, typ);
            p.w.ZeroUntil(s.Value / ptrsize);
            p.w.Append(prog[4L..], nptr);

        }

        // dataSortKey is used to sort a slice of data symbol *sym.Symbol pointers.
        // The sort keys are kept inline to improve cache behavior while sorting.
        private partial struct dataSortKey
        {
            public long size;
            public @string name;
            public ptr<sym.Symbol> sym;
        }

        private partial struct bySizeAndName // : slice<dataSortKey>
        {
        }

        private static long Len(this bySizeAndName d)
        {
            return len(d);
        }
        private static void Swap(this bySizeAndName d, long i, long j)
        {
            d[i] = d[j];
            d[j] = d[i];
        }
        private static bool Less(this bySizeAndName d, long i, long j)
        {
            var s1 = d[i];
            var s2 = d[j];
            if (s1.size != s2.size)
            {
                return s1.size < s2.size;
            }

            return s1.name < s2.name;

        }

        // cutoff is the maximum data section size permitted by the linker
        // (see issue #9862).
        private static readonly float cutoff = (float)2e9F; // 2 GB (or so; looks better in errors than 2^31)

 // 2 GB (or so; looks better in errors than 2^31)

        private static void checkdatsize(ptr<Link> _addr_ctxt, long datsize, sym.SymKind symn)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            if (datsize > cutoff)
            {
                Errorf(null, "too much data in section %v (over %v bytes)", symn, cutoff);
            }

        }

        // datap is a collection of reachable data symbols in address order.
        // Generated by dodata.
        private static slice<ptr<sym.Symbol>> datap = default;

        private static void dodata(this ptr<Link> _addr_ctxt) => func((_, panic, __) =>
        {
            ref Link ctxt = ref _addr_ctxt.val;

            if ((ctxt.DynlinkingGo() && ctxt.HeadType == objabi.Hdarwin) || (ctxt.HeadType == objabi.Haix && ctxt.LinkMode == LinkExternal))
            { 
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
                var bss = ctxt.Syms.Lookup("runtime.bss", 0L);
                bss.Size = 8L;
                bss.Attr.Set(sym.AttrSpecial, false);

                ctxt.Syms.Lookup("runtime.ebss", 0L).Attr.Set(sym.AttrSpecial, false);

                ref var data = ref heap(ctxt.Syms.Lookup("runtime.data", 0L), out ptr<var> _addr_data);
                data.Size = 8L;
                data.Attr.Set(sym.AttrSpecial, false);

                var edata = ctxt.Syms.Lookup("runtime.edata", 0L);
                edata.Attr.Set(sym.AttrSpecial, false);
                if (ctxt.HeadType == objabi.Haix)
                { 
                    // XCOFFTOC symbols are part of .data section.
                    edata.Type = sym.SXCOFFTOC;

                }

                var types = ctxt.Syms.Lookup("runtime.types", 0L);
                types.Type = sym.STYPE;
                types.Size = 8L;
                types.Attr.Set(sym.AttrSpecial, false);

                var etypes = ctxt.Syms.Lookup("runtime.etypes", 0L);
                etypes.Type = sym.SFUNCTAB;
                etypes.Attr.Set(sym.AttrSpecial, false);

                if (ctxt.HeadType == objabi.Haix)
                {
                    var rodata = ctxt.Syms.Lookup("runtime.rodata", 0L);
                    rodata.Type = sym.SSTRING;
                    rodata.Size = 8L;
                    rodata.Attr.Set(sym.AttrSpecial, false);

                    ctxt.Syms.Lookup("runtime.erodata", 0L).Attr.Set(sym.AttrSpecial, false);
                }

            } 

            // Collect data symbols by type into data.
            data = new array<slice<ptr<sym.Symbol>>>(sym.SXREF);
            {
                var s__prev1 = s;

                foreach (var (_, __s) in ctxt.Syms.Allsym)
                {
                    s = __s;
                    if (!s.Attr.Reachable() || s.Attr.Special() || s.Attr.SubSymbol())
                    {
                        continue;
                    }

                    if (s.Type <= sym.STEXT || s.Type >= sym.SXREF)
                    {
                        continue;
                    }

                    data[s.Type] = append(data[s.Type], s);

                } 

                // Now that we have the data symbols, but before we start
                // to assign addresses, record all the necessary
                // dynamic relocations. These will grow the relocation
                // symbol, which is itself data.
                //
                // On darwin, we need the symbol table numbers for dynreloc.

                s = s__prev1;
            }

            if (ctxt.HeadType == objabi.Hdarwin)
            {
                machosymorder(ctxt);
            }

            dynreloc(_addr_ctxt, _addr_data);

            if (ctxt.UseRelro())
            { 
                // "read only" data with relocations needs to go in its own section
                // when building a shared library. We do this by boosting objects of
                // type SXXX with relocations to type SXXXRELRO.
                {
                    var symnro__prev1 = symnro;

                    foreach (var (_, __symnro) in sym.ReadOnly)
                    {
                        symnro = __symnro;
                        var symnrelro = sym.RelROMap[symnro];

                        ptr<sym.Symbol> ro = new slice<ptr<sym.Symbol>>(new ptr<sym.Symbol>[] {  });
                        var relro = data[symnrelro];

                        {
                            var s__prev2 = s;

                            foreach (var (_, __s) in data[symnro])
                            {
                                s = __s;
                                var isRelro = len(s.R) > 0L;

                                if (s.Type == sym.STYPE || s.Type == sym.STYPERELRO || s.Type == sym.SGOFUNCRELRO) 
                                    // Symbols are not sorted yet, so it is possible
                                    // that an Outer symbol has been changed to a
                                    // relro Type before it reaches here.
                                    isRelro = true;
                                else if (s.Type == sym.SFUNCTAB) 
                                    if (ctxt.HeadType == objabi.Haix && s.Name == "runtime.etypes")
                                    { 
                                        // runtime.etypes must be at the end of
                                        // the relro datas.
                                        isRelro = true;

                                    }

                                                                if (isRelro)
                                {
                                    s.Type = symnrelro;
                                    if (s.Outer != null)
                                    {
                                        s.Outer.Type = s.Type;
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
                                if (s.Outer != null && s.Outer.Type != s.Type)
                                {
                                    Errorf(s, "inconsistent types for symbol and its Outer %s (%v != %v)", s.Outer.Name, s.Type, s.Outer.Type);
                                }

                            }

                            s = s__prev2;
                        }

                        data[symnro] = ro;
                        data[symnrelro] = relro;

                    }

                    symnro = symnro__prev1;
                }
            } 

            // Sort symbols.
            array<int> dataMaxAlign = new array<int>(sym.SXREF);
            sync.WaitGroup wg = default;
            {
                var symn__prev1 = symn;

                foreach (var (__symn) in data)
                {
                    symn = __symn;
                    var symn = sym.SymKind(symn);
                    wg.Add(1L);
                    go_(() => () =>
                    {
                        data[symn], dataMaxAlign[symn] = dodataSect(_addr_ctxt, symn, data[symn]);
                        wg.Done();
                    }());

                }

                symn = symn__prev1;
            }

            wg.Wait();

            if (ctxt.HeadType == objabi.Haix && ctxt.LinkMode == LinkExternal)
            { 
                // These symbols must have the same alignment as their section.
                // Otherwize, ld might change the layout of Go sections.
                ctxt.Syms.ROLookup("runtime.data", 0L).Align = dataMaxAlign[sym.SDATA];
                ctxt.Syms.ROLookup("runtime.bss", 0L).Align = dataMaxAlign[sym.SBSS];

            } 

            // Allocate sections.
            // Data is processed before segtext, because we need
            // to see all symbols in the .data and .bss sections in order
            // to generate garbage collection information.
            var datsize = int64(0L); 

            // Writable data sections that do not need any specialized handling.
            sym.SymKind writable = new slice<sym.SymKind>(new sym.SymKind[] { sym.SBUILDINFO, sym.SELFSECT, sym.SMACHO, sym.SMACHOGOT, sym.SWINDOWS });
            {
                var symn__prev1 = symn;

                foreach (var (_, __symn) in writable)
                {
                    symn = __symn;
                    {
                        var s__prev2 = s;

                        foreach (var (_, __s) in data[symn])
                        {
                            s = __s;
                            var sect = addsection(ctxt.Arch, _addr_Segdata, s.Name, 06L);
                            sect.Align = symalign(_addr_s);
                            datsize = Rnd(datsize, int64(sect.Align));
                            sect.Vaddr = uint64(datsize);
                            s.Sect = sect;
                            s.Type = sym.SDATA;
                            s.Value = int64(uint64(datsize) - sect.Vaddr);
                            datsize += s.Size;
                            sect.Length = uint64(datsize) - sect.Vaddr;
                        }

                        s = s__prev2;
                    }

                    checkdatsize(_addr_ctxt, datsize, symn);

                } 

                // .got (and .toc on ppc64)

                symn = symn__prev1;
            }

            if (len(data[sym.SELFGOT]) > 0L)
            {
                sect = addsection(ctxt.Arch, _addr_Segdata, ".got", 06L);
                sect.Align = dataMaxAlign[sym.SELFGOT];
                datsize = Rnd(datsize, int64(sect.Align));
                sect.Vaddr = uint64(datsize);
                {
                    var s__prev1 = s;

                    foreach (var (_, __s) in data[sym.SELFGOT])
                    {
                        s = __s;
                        datsize = aligndatsize(datsize, _addr_s);
                        s.Sect = sect;
                        s.Type = sym.SDATA;
                        s.Value = int64(uint64(datsize) - sect.Vaddr); 

                        // Resolve .TOC. symbol for this object file (ppc64)
                        var toc = ctxt.Syms.ROLookup(".TOC.", int(s.Version));
                        if (toc != null)
                        {
                            toc.Sect = sect;
                            toc.Outer = s;
                            toc.Sub = s.Sub;
                            s.Sub = toc;

                            toc.Value = 0x8000UL;
                        }

                        datsize += s.Size;

                    }

                    s = s__prev1;
                }

                checkdatsize(_addr_ctxt, datsize, sym.SELFGOT);
                sect.Length = uint64(datsize) - sect.Vaddr;

            } 

            /* pointer-free data */
            sect = addsection(ctxt.Arch, _addr_Segdata, ".noptrdata", 06L);
            sect.Align = dataMaxAlign[sym.SNOPTRDATA];
            datsize = Rnd(datsize, int64(sect.Align));
            sect.Vaddr = uint64(datsize);
            ctxt.Syms.Lookup("runtime.noptrdata", 0L).Sect = sect;
            ctxt.Syms.Lookup("runtime.enoptrdata", 0L).Sect = sect;
            {
                var s__prev1 = s;

                foreach (var (_, __s) in data[sym.SNOPTRDATA])
                {
                    s = __s;
                    datsize = aligndatsize(datsize, _addr_s);
                    s.Sect = sect;
                    s.Type = sym.SDATA;
                    s.Value = int64(uint64(datsize) - sect.Vaddr);
                    datsize += s.Size;
                }

                s = s__prev1;
            }

            checkdatsize(_addr_ctxt, datsize, sym.SNOPTRDATA);
            sect.Length = uint64(datsize) - sect.Vaddr;

            var hasinitarr = ctxt.linkShared; 

            /* shared library initializer */

            if (ctxt.BuildMode == BuildModeCArchive || ctxt.BuildMode == BuildModeCShared || ctxt.BuildMode == BuildModeShared || ctxt.BuildMode == BuildModePlugin) 
                hasinitarr = true;
                        if (ctxt.HeadType == objabi.Haix)
            {
                if (len(data[sym.SINITARR]) > 0L)
                {
                    Errorf(null, "XCOFF format doesn't allow .init_array section");
                }

            }

            if (hasinitarr && len(data[sym.SINITARR]) > 0L)
            {
                sect = addsection(ctxt.Arch, _addr_Segdata, ".init_array", 06L);
                sect.Align = dataMaxAlign[sym.SINITARR];
                datsize = Rnd(datsize, int64(sect.Align));
                sect.Vaddr = uint64(datsize);
                {
                    var s__prev1 = s;

                    foreach (var (_, __s) in data[sym.SINITARR])
                    {
                        s = __s;
                        datsize = aligndatsize(datsize, _addr_s);
                        s.Sect = sect;
                        s.Value = int64(uint64(datsize) - sect.Vaddr);
                        datsize += s.Size;
                    }

                    s = s__prev1;
                }

                sect.Length = uint64(datsize) - sect.Vaddr;
                checkdatsize(_addr_ctxt, datsize, sym.SINITARR);

            } 

            /* data */
            sect = addsection(ctxt.Arch, _addr_Segdata, ".data", 06L);
            sect.Align = dataMaxAlign[sym.SDATA];
            datsize = Rnd(datsize, int64(sect.Align));
            sect.Vaddr = uint64(datsize);
            ctxt.Syms.Lookup("runtime.data", 0L).Sect = sect;
            ctxt.Syms.Lookup("runtime.edata", 0L).Sect = sect;
            GCProg gc = default;
            gc.Init(ctxt, "runtime.gcdata");
            {
                var s__prev1 = s;

                foreach (var (_, __s) in data[sym.SDATA])
                {
                    s = __s;
                    s.Sect = sect;
                    s.Type = sym.SDATA;
                    datsize = aligndatsize(datsize, _addr_s);
                    s.Value = int64(uint64(datsize) - sect.Vaddr);
                    gc.AddSym(s);
                    datsize += s.Size;
                }

                s = s__prev1;
            }

            gc.End(datsize - int64(sect.Vaddr)); 
            // On AIX, TOC entries must be the last of .data
            // These aren't part of gc as they won't change during the runtime.
            {
                var s__prev1 = s;

                foreach (var (_, __s) in data[sym.SXCOFFTOC])
                {
                    s = __s;
                    s.Sect = sect;
                    s.Type = sym.SDATA;
                    datsize = aligndatsize(datsize, _addr_s);
                    s.Value = int64(uint64(datsize) - sect.Vaddr);
                    datsize += s.Size;
                }

                s = s__prev1;
            }

            checkdatsize(_addr_ctxt, datsize, sym.SDATA);
            sect.Length = uint64(datsize) - sect.Vaddr; 

            /* bss */
            sect = addsection(ctxt.Arch, _addr_Segdata, ".bss", 06L);
            sect.Align = dataMaxAlign[sym.SBSS];
            datsize = Rnd(datsize, int64(sect.Align));
            sect.Vaddr = uint64(datsize);
            ctxt.Syms.Lookup("runtime.bss", 0L).Sect = sect;
            ctxt.Syms.Lookup("runtime.ebss", 0L).Sect = sect;
            gc = new GCProg();
            gc.Init(ctxt, "runtime.gcbss");
            {
                var s__prev1 = s;

                foreach (var (_, __s) in data[sym.SBSS])
                {
                    s = __s;
                    s.Sect = sect;
                    datsize = aligndatsize(datsize, _addr_s);
                    s.Value = int64(uint64(datsize) - sect.Vaddr);
                    gc.AddSym(s);
                    datsize += s.Size;
                }

                s = s__prev1;
            }

            checkdatsize(_addr_ctxt, datsize, sym.SBSS);
            sect.Length = uint64(datsize) - sect.Vaddr;
            gc.End(int64(sect.Length)); 

            /* pointer-free bss */
            sect = addsection(ctxt.Arch, _addr_Segdata, ".noptrbss", 06L);
            sect.Align = dataMaxAlign[sym.SNOPTRBSS];
            datsize = Rnd(datsize, int64(sect.Align));
            sect.Vaddr = uint64(datsize);
            ctxt.Syms.Lookup("runtime.noptrbss", 0L).Sect = sect;
            ctxt.Syms.Lookup("runtime.enoptrbss", 0L).Sect = sect;
            {
                var s__prev1 = s;

                foreach (var (_, __s) in data[sym.SNOPTRBSS])
                {
                    s = __s;
                    datsize = aligndatsize(datsize, _addr_s);
                    s.Sect = sect;
                    s.Value = int64(uint64(datsize) - sect.Vaddr);
                    datsize += s.Size;
                }

                s = s__prev1;
            }

            sect.Length = uint64(datsize) - sect.Vaddr;
            ctxt.Syms.Lookup("runtime.end", 0L).Sect = sect;
            checkdatsize(_addr_ctxt, datsize, sym.SNOPTRBSS); 

            // Coverage instrumentation counters for libfuzzer.
            if (len(data[sym.SLIBFUZZER_EXTRA_COUNTER]) > 0L)
            {
                sect = addsection(ctxt.Arch, _addr_Segdata, "__libfuzzer_extra_counters", 06L);
                sect.Align = dataMaxAlign[sym.SLIBFUZZER_EXTRA_COUNTER];
                datsize = Rnd(datsize, int64(sect.Align));
                sect.Vaddr = uint64(datsize);
                {
                    var s__prev1 = s;

                    foreach (var (_, __s) in data[sym.SLIBFUZZER_EXTRA_COUNTER])
                    {
                        s = __s;
                        datsize = aligndatsize(datsize, _addr_s);
                        s.Sect = sect;
                        s.Value = int64(uint64(datsize) - sect.Vaddr);
                        datsize += s.Size;
                    }

                    s = s__prev1;
                }

                sect.Length = uint64(datsize) - sect.Vaddr;
                checkdatsize(_addr_ctxt, datsize, sym.SLIBFUZZER_EXTRA_COUNTER);

            }

            if (len(data[sym.STLSBSS]) > 0L)
            {
                sect = ;
                if ((ctxt.IsELF || ctxt.HeadType == objabi.Haix) && (ctxt.LinkMode == LinkExternal || !FlagD.val))
                {
                    sect = addsection(ctxt.Arch, _addr_Segdata, ".tbss", 06L);
                    sect.Align = int32(ctxt.Arch.PtrSize);
                    sect.Vaddr = 0L;
                }

                datsize = 0L;

                {
                    var s__prev1 = s;

                    foreach (var (_, __s) in data[sym.STLSBSS])
                    {
                        s = __s;
                        datsize = aligndatsize(datsize, _addr_s);
                        s.Sect = sect;
                        s.Value = datsize;
                        datsize += s.Size;
                    }

                    s = s__prev1;
                }

                checkdatsize(_addr_ctxt, datsize, sym.STLSBSS);

                if (sect != null)
                {
                    sect.Length = uint64(datsize);
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

            datsize = 0L; 

            /* read-only executable ELF, Mach-O sections */
            if (len(data[sym.STEXT]) != 0L)
            {
                Errorf(null, "dodata found an sym.STEXT symbol: %s", data[sym.STEXT][0L].Name);
            }

            {
                var s__prev1 = s;

                foreach (var (_, __s) in data[sym.SELFRXSECT])
                {
                    s = __s;
                    sect = addsection(ctxt.Arch, _addr_Segtext, s.Name, 04L);
                    sect.Align = symalign(_addr_s);
                    datsize = Rnd(datsize, int64(sect.Align));
                    sect.Vaddr = uint64(datsize);
                    s.Sect = sect;
                    s.Type = sym.SRODATA;
                    s.Value = int64(uint64(datsize) - sect.Vaddr);
                    datsize += s.Size;
                    sect.Length = uint64(datsize) - sect.Vaddr;
                    checkdatsize(_addr_ctxt, datsize, sym.SELFRXSECT);
                } 

                /* read-only data */

                s = s__prev1;
            }

            sect = addsection(ctxt.Arch, segro, ".rodata", 04L);

            sect.Vaddr = 0L;
            ctxt.Syms.Lookup("runtime.rodata", 0L).Sect = sect;
            ctxt.Syms.Lookup("runtime.erodata", 0L).Sect = sect;
            if (!ctxt.UseRelro())
            {
                ctxt.Syms.Lookup("runtime.types", 0L).Sect = sect;
                ctxt.Syms.Lookup("runtime.etypes", 0L).Sect = sect;
            }

            {
                var symn__prev1 = symn;

                foreach (var (_, __symn) in sym.ReadOnly)
                {
                    symn = __symn;
                    var align = dataMaxAlign[symn];
                    if (sect.Align < align)
                    {
                        sect.Align = align;
                    }

                }

                symn = symn__prev1;
            }

            datsize = Rnd(datsize, int64(sect.Align));
            {
                var symn__prev1 = symn;

                foreach (var (_, __symn) in sym.ReadOnly)
                {
                    symn = __symn;
                    var symnStartValue = datsize;
                    {
                        var s__prev2 = s;

                        foreach (var (_, __s) in data[symn])
                        {
                            s = __s;
                            datsize = aligndatsize(datsize, _addr_s);
                            s.Sect = sect;
                            s.Type = sym.SRODATA;
                            s.Value = int64(uint64(datsize) - sect.Vaddr);
                            datsize += s.Size;
                        }

                        s = s__prev2;
                    }

                    checkdatsize(_addr_ctxt, datsize, symn);
                    if (ctxt.HeadType == objabi.Haix)
                    { 
                        // Read-only symbols might be wrapped inside their outer
                        // symbol.
                        // XCOFF symbol table needs to know the size of
                        // these outer symbols.
                        xcoffUpdateOuterSize(ctxt, datsize - symnStartValue, symn);

                    }

                }

                symn = symn__prev1;
            }

            sect.Length = uint64(datsize) - sect.Vaddr; 

            /* read-only ELF, Mach-O sections */
            {
                var s__prev1 = s;

                foreach (var (_, __s) in data[sym.SELFROSECT])
                {
                    s = __s;
                    sect = addsection(ctxt.Arch, segro, s.Name, 04L);
                    sect.Align = symalign(_addr_s);
                    datsize = Rnd(datsize, int64(sect.Align));
                    sect.Vaddr = uint64(datsize);
                    s.Sect = sect;
                    s.Type = sym.SRODATA;
                    s.Value = int64(uint64(datsize) - sect.Vaddr);
                    datsize += s.Size;
                    sect.Length = uint64(datsize) - sect.Vaddr;
                }

                s = s__prev1;
            }

            checkdatsize(_addr_ctxt, datsize, sym.SELFROSECT);

            {
                var s__prev1 = s;

                foreach (var (_, __s) in data[sym.SMACHOPLT])
                {
                    s = __s;
                    sect = addsection(ctxt.Arch, segro, s.Name, 04L);
                    sect.Align = symalign(_addr_s);
                    datsize = Rnd(datsize, int64(sect.Align));
                    sect.Vaddr = uint64(datsize);
                    s.Sect = sect;
                    s.Type = sym.SRODATA;
                    s.Value = int64(uint64(datsize) - sect.Vaddr);
                    datsize += s.Size;
                    sect.Length = uint64(datsize) - sect.Vaddr;
                }

                s = s__prev1;
            }

            checkdatsize(_addr_ctxt, datsize, sym.SMACHOPLT); 

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
            Func<@string, ptr<sym.Section>> addrelrosection = suffix =>
            {
                return addsection(ctxt.Arch, segro, suffix, 04L);
            }
;

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
                    datsize = 0L;

                }

                addrelrosection = suffix =>
                {
                    return addsection(ctxt.Arch, segrelro, ".data.rel.ro" + suffix, 06L);
                } 

                /* data only written by relocations */
; 

                /* data only written by relocations */
                sect = addrelrosection("");

                ctxt.Syms.Lookup("runtime.types", 0L).Sect = sect;
                ctxt.Syms.Lookup("runtime.etypes", 0L).Sect = sect;

                {
                    var symnro__prev1 = symnro;

                    foreach (var (_, __symnro) in sym.ReadOnly)
                    {
                        symnro = __symnro;
                        symn = sym.RelROMap[symnro];
                        align = dataMaxAlign[symn];
                        if (sect.Align < align)
                        {
                            sect.Align = align;
                        }

                    }

                    symnro = symnro__prev1;
                }

                datsize = Rnd(datsize, int64(sect.Align));
                sect.Vaddr = uint64(datsize);

                {
                    var i__prev1 = i;
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
                            datsize++;

                        }

                        symn = sym.RelROMap[symnro];
                        symnStartValue = datsize;
                        {
                            var s__prev2 = s;

                            foreach (var (_, __s) in data[symn])
                            {
                                s = __s;
                                datsize = aligndatsize(datsize, _addr_s);
                                if (s.Outer != null && s.Outer.Sect != null && s.Outer.Sect != sect)
                                {
                                    Errorf(s, "s.Outer (%s) in different section from s, %s != %s", s.Outer.Name, s.Outer.Sect.Name, sect.Name);
                                }

                                s.Sect = sect;
                                s.Type = sym.SRODATA;
                                s.Value = int64(uint64(datsize) - sect.Vaddr);
                                datsize += s.Size;

                            }

                            s = s__prev2;
                        }

                        checkdatsize(_addr_ctxt, datsize, symn);
                        if (ctxt.HeadType == objabi.Haix)
                        { 
                            // Read-only symbols might be wrapped inside their outer
                            // symbol.
                            // XCOFF symbol table needs to know the size of
                            // these outer symbols.
                            xcoffUpdateOuterSize(ctxt, datsize - symnStartValue, symn);

                        }

                    }

                    i = i__prev1;
                    symnro = symnro__prev1;
                }

                sect.Length = uint64(datsize) - sect.Vaddr;

            } 

            /* typelink */
            sect = addrelrosection(".typelink");
            sect.Align = dataMaxAlign[sym.STYPELINK];
            datsize = Rnd(datsize, int64(sect.Align));
            sect.Vaddr = uint64(datsize);
            var typelink = ctxt.Syms.Lookup("runtime.typelink", 0L);
            typelink.Sect = sect;
            typelink.Type = sym.SRODATA;
            datsize += typelink.Size;
            checkdatsize(_addr_ctxt, datsize, sym.STYPELINK);
            sect.Length = uint64(datsize) - sect.Vaddr; 

            /* itablink */
            sect = addrelrosection(".itablink");
            sect.Align = dataMaxAlign[sym.SITABLINK];
            datsize = Rnd(datsize, int64(sect.Align));
            sect.Vaddr = uint64(datsize);
            ctxt.Syms.Lookup("runtime.itablink", 0L).Sect = sect;
            ctxt.Syms.Lookup("runtime.eitablink", 0L).Sect = sect;
            {
                var s__prev1 = s;

                foreach (var (_, __s) in data[sym.SITABLINK])
                {
                    s = __s;
                    datsize = aligndatsize(datsize, _addr_s);
                    s.Sect = sect;
                    s.Type = sym.SRODATA;
                    s.Value = int64(uint64(datsize) - sect.Vaddr);
                    datsize += s.Size;
                }

                s = s__prev1;
            }

            checkdatsize(_addr_ctxt, datsize, sym.SITABLINK);
            sect.Length = uint64(datsize) - sect.Vaddr;
            if (ctxt.HeadType == objabi.Haix)
            { 
                // Store .itablink size because its symbols are wrapped
                // under an outer symbol: runtime.itablink.
                xcoffUpdateOuterSize(ctxt, int64(sect.Length), sym.SITABLINK);

            } 

            /* gosymtab */
            sect = addrelrosection(".gosymtab");
            sect.Align = dataMaxAlign[sym.SSYMTAB];
            datsize = Rnd(datsize, int64(sect.Align));
            sect.Vaddr = uint64(datsize);
            ctxt.Syms.Lookup("runtime.symtab", 0L).Sect = sect;
            ctxt.Syms.Lookup("runtime.esymtab", 0L).Sect = sect;
            {
                var s__prev1 = s;

                foreach (var (_, __s) in data[sym.SSYMTAB])
                {
                    s = __s;
                    datsize = aligndatsize(datsize, _addr_s);
                    s.Sect = sect;
                    s.Type = sym.SRODATA;
                    s.Value = int64(uint64(datsize) - sect.Vaddr);
                    datsize += s.Size;
                }

                s = s__prev1;
            }

            checkdatsize(_addr_ctxt, datsize, sym.SSYMTAB);
            sect.Length = uint64(datsize) - sect.Vaddr; 

            /* gopclntab */
            sect = addrelrosection(".gopclntab");
            sect.Align = dataMaxAlign[sym.SPCLNTAB];
            datsize = Rnd(datsize, int64(sect.Align));
            sect.Vaddr = uint64(datsize);
            ctxt.Syms.Lookup("runtime.pclntab", 0L).Sect = sect;
            ctxt.Syms.Lookup("runtime.epclntab", 0L).Sect = sect;
            {
                var s__prev1 = s;

                foreach (var (_, __s) in data[sym.SPCLNTAB])
                {
                    s = __s;
                    datsize = aligndatsize(datsize, _addr_s);
                    s.Sect = sect;
                    s.Type = sym.SRODATA;
                    s.Value = int64(uint64(datsize) - sect.Vaddr);
                    datsize += s.Size;
                }

                s = s__prev1;
            }

            checkdatsize(_addr_ctxt, datsize, sym.SRODATA);
            sect.Length = uint64(datsize) - sect.Vaddr; 

            // 6g uses 4-byte relocation offsets, so the entire segment must fit in 32 bits.
            if (datsize != int64(uint32(datsize)))
            {
                Errorf(null, "read-only data segment too large: %d", datsize);
            }

            {
                var symn__prev1 = symn;

                for (symn = sym.SELFRXSECT; symn < sym.SXREF; symn++)
                {
                    datap = append(datap, data[symn]);
                }


                symn = symn__prev1;
            }

            dwarfGenerateDebugSyms(ctxt);

            long i = default;
            while (i < len(dwarfp))
            {
                var s = dwarfp[i];
                if (s.Type != sym.SDWARFSECT)
                {
                    break;
                i++;
                }

                sect = addsection(ctxt.Arch, _addr_Segdwarf, s.Name, 04L);
                sect.Align = 1L;
                datsize = Rnd(datsize, int64(sect.Align));
                sect.Vaddr = uint64(datsize);
                s.Sect = sect;
                s.Type = sym.SRODATA;
                s.Value = int64(uint64(datsize) - sect.Vaddr);
                datsize += s.Size;
                sect.Length = uint64(datsize) - sect.Vaddr;

            }

            checkdatsize(_addr_ctxt, datsize, sym.SDWARFSECT);

            while (i < len(dwarfp))
            {
                var curType = dwarfp[i].Type;
                sect = ;

                if (curType == sym.SDWARFINFO) 
                    sect = addsection(ctxt.Arch, _addr_Segdwarf, ".debug_info", 04L);
                else if (curType == sym.SDWARFRANGE) 
                    sect = addsection(ctxt.Arch, _addr_Segdwarf, ".debug_ranges", 04L);
                else if (curType == sym.SDWARFLOC) 
                    sect = addsection(ctxt.Arch, _addr_Segdwarf, ".debug_loc", 04L);
                else 
                    // Error is unrecoverable, so panic.
                    panic(fmt.Sprintf("unknown DWARF section %v", curType));
                                sect.Align = 1L;
                datsize = Rnd(datsize, int64(sect.Align));
                sect.Vaddr = uint64(datsize);
                while (i < len(dwarfp))
                {
                    s = dwarfp[i];
                    if (s.Type != curType)
                    {
                        break;
                    i++;
                    }

                    s.Sect = sect;
                    s.Type = sym.SRODATA;
                    s.Value = int64(uint64(datsize) - sect.Vaddr);
                    s.Attr |= sym.AttrLocal;
                    datsize += s.Size;

                    if (ctxt.HeadType == objabi.Haix && curType == sym.SDWARFLOC)
                    { 
                        // Update the size of .debug_loc for this symbol's
                        // package.
                        addDwsectCUSize(".debug_loc", s.File, uint64(s.Size));

                    }

                }

                sect.Length = uint64(datsize) - sect.Vaddr;
                checkdatsize(_addr_ctxt, datsize, curType);

            } 

            /* number the sections */
 

            /* number the sections */
            var n = int32(1L);

            {
                var sect__prev1 = sect;

                foreach (var (_, __sect) in Segtext.Sections)
                {
                    sect = __sect;
                    sect.Extnum = int16(n);
                    n++;
                }

                sect = sect__prev1;
            }

            {
                var sect__prev1 = sect;

                foreach (var (_, __sect) in Segrodata.Sections)
                {
                    sect = __sect;
                    sect.Extnum = int16(n);
                    n++;
                }

                sect = sect__prev1;
            }

            {
                var sect__prev1 = sect;

                foreach (var (_, __sect) in Segrelrodata.Sections)
                {
                    sect = __sect;
                    sect.Extnum = int16(n);
                    n++;
                }

                sect = sect__prev1;
            }

            {
                var sect__prev1 = sect;

                foreach (var (_, __sect) in Segdata.Sections)
                {
                    sect = __sect;
                    sect.Extnum = int16(n);
                    n++;
                }

                sect = sect__prev1;
            }

            {
                var sect__prev1 = sect;

                foreach (var (_, __sect) in Segdwarf.Sections)
                {
                    sect = __sect;
                    sect.Extnum = int16(n);
                    n++;
                }

                sect = sect__prev1;
            }
        });

        private static (slice<ptr<sym.Symbol>>, int) dodataSect(ptr<Link> _addr_ctxt, sym.SymKind symn, slice<ptr<sym.Symbol>> syms)
        {
            slice<ptr<sym.Symbol>> result = default;
            int maxAlign = default;
            ref Link ctxt = ref _addr_ctxt.val;

            if (ctxt.HeadType == objabi.Hdarwin)
            { 
                // Some symbols may no longer belong in syms
                // due to movement in machosymorder.
                var newSyms = make_slice<ptr<sym.Symbol>>(0L, len(syms));
                {
                    var s__prev1 = s;

                    foreach (var (_, __s) in syms)
                    {
                        s = __s;
                        if (s.Type == symn)
                        {
                            newSyms = append(newSyms, s);
                        }

                    }

                    s = s__prev1;
                }

                syms = newSyms;

            }

            ptr<sym.Symbol> head;            ptr<sym.Symbol> tail;

            var symsSort = make_slice<dataSortKey>(0L, len(syms));
            {
                var s__prev1 = s;

                foreach (var (_, __s) in syms)
                {
                    s = __s;
                    if (s.Attr.OnList())
                    {
                        log.Fatalf("symbol %s listed multiple times", s.Name);
                    }

                    s.Attr |= sym.AttrOnList;

                    if (s.Size < int64(len(s.P))) 
                        Errorf(s, "initialize bounds (%d < %d)", s.Size, len(s.P));
                    else if (s.Size < 0L) 
                        Errorf(s, "negative size (%d bytes)", s.Size);
                    else if (s.Size > cutoff) 
                        Errorf(s, "symbol too large (%d bytes)", s.Size);
                    // If the usually-special section-marker symbols are being laid
                    // out as regular symbols, put them either at the beginning or
                    // end of their section.
                    if ((ctxt.DynlinkingGo() && ctxt.HeadType == objabi.Hdarwin) || (ctxt.HeadType == objabi.Haix && ctxt.LinkMode == LinkExternal))
                    {
                        switch (s.Name)
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

                    dataSortKey key = new dataSortKey(size:s.Size,name:s.Name,sym:s,);


                    if (s.Type == sym.SELFGOT) 
                        // For ppc64, we want to interleave the .got and .toc sections
                        // from input files. Both are type sym.SELFGOT, so in that case
                        // we skip size comparison and fall through to the name
                        // comparison (conveniently, .got sorts before .toc).
                        key.size = 0L;
                                        symsSort = append(symsSort, key);

                }

                s = s__prev1;
            }

            sort.Sort(bySizeAndName(symsSort));

            long off = 0L;
            if (head != null)
            {
                syms[0L] = head;
                off++;
            }

            {
                var i__prev1 = i;

                foreach (var (__i, __symSort) in symsSort)
                {
                    i = __i;
                    symSort = __symSort;
                    syms[i + off] = symSort.sym;
                    var align = symalign(_addr_symSort.sym);
                    if (maxAlign < align)
                    {
                        maxAlign = align;
                    }

                }

                i = i__prev1;
            }

            if (tail != null)
            {
                syms[len(syms) - 1L] = tail;
            }

            if (ctxt.IsELF && symn == sym.SELFROSECT)
            { 
                // Make .rela and .rela.plt contiguous, the ELF ABI requires this
                // and Solaris actually cares.
                long reli = -1L;
                long plti = -1L;
                {
                    var i__prev1 = i;
                    var s__prev1 = s;

                    foreach (var (__i, __s) in syms)
                    {
                        i = __i;
                        s = __s;
                        switch (s.Name)
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

                    i = i__prev1;
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
                    rel.Align = int32(ctxt.Arch.RegSize);
                    plt.Align = int32(ctxt.Arch.RegSize);

                }

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

            var s = ctxt.Syms.Lookup("go.buildid", 0L);
            s.Attr |= sym.AttrReachable; 
            // The \xff is invalid UTF-8, meant to make it less likely
            // to find one of these accidentally.
            @string data = "\xff Go build ID: " + strconv.Quote(flagBuildid.val) + "\n \xff";
            s.Type = sym.STEXT;
            s.P = (slice<byte>)data;
            s.Size = int64(len(s.P));

            ctxt.Textp = append(ctxt.Textp, null);
            copy(ctxt.Textp[1L..], ctxt.Textp);
            ctxt.Textp[0L] = s;

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

            var s = ctxt.Syms.Lookup(".go.buildinfo", 0L);
            s.Attr |= sym.AttrReachable;
            s.Type = sym.SBUILDINFO;
            s.Align = 16L; 
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

            s.P = data;
            s.Size = int64(len(s.P));
            var s1 = ctxt.Syms.Lookup("runtime.buildVersion", 0L);
            var s2 = ctxt.Syms.Lookup("runtime.modinfo", 0L);
            s.R = new slice<sym.Reloc>(new sym.Reloc[] { {Off:16,Siz:uint8(ctxt.Arch.PtrSize),Type:objabi.R_ADDR,Sym:s1}, {Off:16+int32(ctxt.Arch.PtrSize),Siz:uint8(ctxt.Arch.PtrSize),Type:objabi.R_ADDR,Sym:s2} });

        }

        // assign addresses to text
        private static void textaddress(this ptr<Link> _addr_ctxt)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            addsection(ctxt.Arch, _addr_Segtext, ".text", 05L); 

            // Assign PCs in text segment.
            // Could parallelize, by assigning to text
            // and then letting threads copy down, but probably not worth it.
            var sect = Segtext.Sections[0L];

            sect.Align = int32(Funcalign);

            var text = ctxt.Syms.Lookup("runtime.text", 0L);
            text.Sect = sect;
            if (ctxt.HeadType == objabi.Haix && ctxt.LinkMode == LinkExternal)
            { 
                // Setting runtime.text has a real symbol prevents ld to
                // change its base address resulting in wrong offsets for
                // reflect methods.
                text.Align = sect.Align;
                text.Size = 0x8UL;

            }

            if ((ctxt.DynlinkingGo() && ctxt.HeadType == objabi.Hdarwin) || (ctxt.HeadType == objabi.Haix && ctxt.LinkMode == LinkExternal))
            {
                var etext = ctxt.Syms.Lookup("runtime.etext", 0L);
                etext.Sect = sect;

                ctxt.Textp = append(ctxt.Textp, etext, null);
                copy(ctxt.Textp[1L..], ctxt.Textp);
                ctxt.Textp[0L] = text;
            }

            var va = uint64(FlagTextAddr.val);
            long n = 1L;
            sect.Vaddr = va;
            long ntramps = 0L;
            {
                var s__prev1 = s;

                foreach (var (_, __s) in ctxt.Textp)
                {
                    s = __s;
                    sect, n, va = assignAddress(_addr_ctxt, _addr_sect, n, _addr_s, va, false);

                    trampoline(_addr_ctxt, _addr_s); // resolve jumps, may add trampolines if jump too far

                    // lay down trampolines after each function
                    while (ntramps < len(ctxt.tramps))
                    {
                        var tramp = ctxt.tramps[ntramps];
                        if (ctxt.HeadType == objabi.Haix && strings.HasPrefix(tramp.Name, "runtime.text."))
                        { 
                            // Already set in assignAddress
                            continue;
                        ntramps++;
                        }

                        sect, n, va = assignAddress(_addr_ctxt, _addr_sect, n, _addr_tramp, va, true);

                    }


                }

                s = s__prev1;
            }

            sect.Length = va - sect.Vaddr;
            ctxt.Syms.Lookup("runtime.etext", 0L).Sect = sect; 

            // merge tramps into Textp, keeping Textp in address order
            if (ntramps != 0L)
            {
                var newtextp = make_slice<ptr<sym.Symbol>>(0L, len(ctxt.Textp) + ntramps);
                long i = 0L;
                {
                    var s__prev1 = s;

                    foreach (var (_, __s) in ctxt.Textp)
                    {
                        s = __s;
                        while (i < ntramps && ctxt.tramps[i].Value < s.Value)
                        {
                            newtextp = append(newtextp, ctxt.tramps[i]);
                            i++;
                        }

                        newtextp = append(newtextp, s);

                    }

                    s = s__prev1;
                }

                newtextp = append(newtextp, ctxt.tramps[i..ntramps]);

                ctxt.Textp = newtextp;

            }

        }

        // assigns address for a text symbol, returns (possibly new) section, its number, and the address
        // Note: once we have trampoline insertion support for external linking, this function
        // will not need to create new text sections, and so no need to return sect and n.
        private static (ptr<sym.Section>, long, ulong) assignAddress(ptr<Link> _addr_ctxt, ptr<sym.Section> _addr_sect, long n, ptr<sym.Symbol> _addr_s, ulong va, bool isTramp)
        {
            ptr<sym.Section> _p0 = default!;
            long _p0 = default;
            ulong _p0 = default;
            ref Link ctxt = ref _addr_ctxt.val;
            ref sym.Section sect = ref _addr_sect.val;
            ref sym.Symbol s = ref _addr_s.val;

            if (thearch.AssignAddress != null)
            {
                return _addr_thearch.AssignAddress(ctxt, sect, n, s, va, isTramp)!;
            }

            s.Sect = sect;
            if (s.Attr.SubSymbol())
            {
                return (_addr_sect!, n, va);
            }

            if (s.Align != 0L)
            {
                va = uint64(Rnd(int64(va), int64(s.Align)));
            }
            else
            {
                va = uint64(Rnd(int64(va), int64(Funcalign)));
            }

            var funcsize = uint64(MINFUNC); // spacing required for findfunctab
            if (s.Size > MINFUNC)
            {
                funcsize = uint64(s.Size);
            }

            if (sect.Align < s.Align)
            {
                sect.Align = s.Align;
            } 

            // On ppc64x a text section should not be larger than 2^26 bytes due to the size of
            // call target offset field in the bl instruction.  Splitting into smaller text
            // sections smaller than this limit allows the GNU linker to modify the long calls
            // appropriately.  The limit allows for the space needed for tables inserted by the linker.

            // If this function doesn't fit in the current text section, then create a new one.

            // Only break at outermost syms.
            if (ctxt.Arch.InFamily(sys.PPC64) && s.Outer == null && ctxt.LinkMode == LinkExternal && va - sect.Vaddr + funcsize + maxSizeTrampolinesPPC64(_addr_s, isTramp) > 0x1c00000UL)
            { 
                // Set the length for the previous text section
                sect.Length = va - sect.Vaddr; 

                // Create new section, set the starting Vaddr
                sect = addsection(ctxt.Arch, _addr_Segtext, ".text", 05L);
                sect.Vaddr = va;
                s.Sect = sect; 

                // Create a symbol for the start of the secondary text sections
                var ntext = ctxt.Syms.Lookup(fmt.Sprintf("runtime.text.%d", n), 0L);
                ntext.Sect = sect;
                if (ctxt.HeadType == objabi.Haix)
                { 
                    // runtime.text.X must be a real symbol on AIX.
                    // Assign its address directly in order to be the
                    // first symbol of this new section.
                    ntext.Type = sym.STEXT;
                    ntext.Size = int64(MINFUNC);
                    ntext.Attr |= sym.AttrReachable;
                    ntext.Attr |= sym.AttrOnList;
                    ctxt.tramps = append(ctxt.tramps, ntext);

                    ntext.Value = int64(va);
                    va += uint64(ntext.Size);

                    if (s.Align != 0L)
                    {
                        va = uint64(Rnd(int64(va), int64(s.Align)));
                    }
                    else
                    {
                        va = uint64(Rnd(int64(va), int64(Funcalign)));
                    }

                }

                n++;

            }

            s.Value = 0L;
            {
                var sub = s;

                while (sub != null)
                {
                    sub.Value += int64(va);
                    sub = sub.Sub;
                }

            }

            va += funcsize;

            return (_addr_sect!, n, va);

        }

        // address assigns virtual addresses to all segments and sections and
        // returns all segments in file order.
        private static slice<ptr<sym.Segment>> address(this ptr<Link> _addr_ctxt)
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

            var text = Segtext.Sections[0L];            var rodata = ctxt.Syms.Lookup("runtime.rodata", 0L).Sect;            var itablink = ctxt.Syms.Lookup("runtime.itablink", 0L).Sect;            var symtab = ctxt.Syms.Lookup("runtime.symtab", 0L).Sect;            var pclntab = ctxt.Syms.Lookup("runtime.pclntab", 0L).Sect;            var types = ctxt.Syms.Lookup("runtime.types", 0L).Sect;
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

                foreach (var (_, __s) in datap)
                {
                    s = __s;
                    if (s.Sect != null)
                    {
                        s.Value += int64(s.Sect.Vaddr);
                    }

                    {
                        var sub__prev2 = sub;

                        var sub = s.Sub;

                        while (sub != null)
                        {
                            sub.Value += s.Value;
                            sub = sub.Sub;
                        }


                        sub = sub__prev2;
                    }

                }

                s = s__prev1;
            }

            {
                var s__prev1 = s;

                foreach (var (_, __s) in dwarfp)
                {
                    s = __s;
                    if (s.Sect != null)
                    {
                        s.Value += int64(s.Sect.Vaddr);
                    }

                    {
                        var sub__prev2 = sub;

                        sub = s.Sub;

                        while (sub != null)
                        {
                            sub.Value += s.Value;
                            sub = sub.Sub;
                        }


                        sub = sub__prev2;
                    }

                }

                s = s__prev1;
            }

            if (ctxt.BuildMode == BuildModeShared)
            {
                var s = ctxt.Syms.Lookup("go.link.abihashbytes", 0L);
                var sectSym = ctxt.Syms.Lookup(".note.go.abihash", 0L);
                s.Sect = sectSym.Sect;
                s.Value = int64(sectSym.Sect.Vaddr + 16L);
            }

            ctxt.xdefine("runtime.text", sym.STEXT, int64(text.Vaddr));
            ctxt.xdefine("runtime.etext", sym.STEXT, int64(lasttext.Vaddr + lasttext.Length)); 

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
            ctxt.xdefine("runtime.itablink", sym.SRODATA, int64(itablink.Vaddr));
            ctxt.xdefine("runtime.eitablink", sym.SRODATA, int64(itablink.Vaddr + itablink.Length));

            s = ctxt.Syms.Lookup("runtime.gcdata", 0L);
            s.Attr |= sym.AttrLocal;
            ctxt.xdefine("runtime.egcdata", sym.SRODATA, Symaddr(s) + s.Size);
            ctxt.Syms.Lookup("runtime.egcdata", 0L).Sect = s.Sect;

            s = ctxt.Syms.Lookup("runtime.gcbss", 0L);
            s.Attr |= sym.AttrLocal;
            ctxt.xdefine("runtime.egcbss", sym.SRODATA, Symaddr(s) + s.Size);
            ctxt.Syms.Lookup("runtime.egcbss", 0L).Sect = s.Sect;

            ctxt.xdefine("runtime.symtab", sym.SRODATA, int64(symtab.Vaddr));
            ctxt.xdefine("runtime.esymtab", sym.SRODATA, int64(symtab.Vaddr + symtab.Length));
            ctxt.xdefine("runtime.pclntab", sym.SRODATA, int64(pclntab.Vaddr));
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

            return order;

        }

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
        private static void AddTramp(this ptr<Link> _addr_ctxt, ptr<sym.Symbol> _addr_s)
        {
            ref Link ctxt = ref _addr_ctxt.val;
            ref sym.Symbol s = ref _addr_s.val;

            s.Type = sym.STEXT;
            s.Attr |= sym.AttrReachable;
            s.Attr |= sym.AttrOnList;
            ctxt.tramps = append(ctxt.tramps, s);
            if (FlagDebugTramp > 0L && ctxt.Debugvlog > 0L.val)
            {
                ctxt.Logf("trampoline %s inserted\n", s);
            }

        }

        // compressSyms compresses syms and returns the contents of the
        // compressed section. If the section would get larger, it returns nil.
        private static slice<byte> compressSyms(ptr<Link> _addr_ctxt, slice<ptr<sym.Symbol>> syms)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            long total = default;
            foreach (var (_, sym) in syms)
            {
                total += sym.Size;
            }
            ref bytes.Buffer buf = ref heap(out ptr<bytes.Buffer> _addr_buf);
            buf.Write((slice<byte>)"ZLIB");
            array<byte> sizeBytes = new array<byte>(8L);
            binary.BigEndian.PutUint64(sizeBytes[..], uint64(total));
            buf.Write(sizeBytes[..]); 

            // Using zlib.BestSpeed achieves very nearly the same
            // compression levels of zlib.DefaultCompression, but takes
            // substantially less time. This is important because DWARF
            // compression can be a significant fraction of link time.
            var (z, err) = zlib.NewWriterLevel(_addr_buf, zlib.BestSpeed);
            if (err != null)
            {
                log.Fatalf("NewWriterLevel failed: %s", err);
            }

            foreach (var (_, s) in syms)
            { 
                // s.P may be read-only. Apply relocations in a
                // temporary buffer, and immediately write it out.
                var oldP = s.P;
                var wasReadOnly = s.Attr.ReadOnly();
                if (len(s.R) != 0L && wasReadOnly)
                {
                    ctxt.relocbuf = append(ctxt.relocbuf[..0L], s.P);
                    s.P = ctxt.relocbuf;
                    s.Attr.Set(sym.AttrReadOnly, false);
                }

                relocsym(_addr_ctxt, _addr_s);
                {
                    var (_, err) = z.Write(s.P);

                    if (err != null)
                    {
                        log.Fatalf("compression failed: %s", err);
                    }

                }

                {
                    var i__prev2 = i;

                    var i = s.Size - int64(len(s.P));

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
                    // Restore s.P if a temporary buffer was used. If compression
                    // is not beneficial, we'll go back to use the uncompressed
                    // contents, in which case we still need s.P.


                    i = i__prev2;
                } 
                // Restore s.P if a temporary buffer was used. If compression
                // is not beneficial, we'll go back to use the uncompressed
                // contents, in which case we still need s.P.
                if (len(s.R) != 0L && wasReadOnly)
                {
                    s.P = oldP;
                    s.Attr.Set(sym.AttrReadOnly, wasReadOnly);
                    {
                        var i__prev2 = i;

                        foreach (var (__i) in s.R)
                        {
                            i = __i;
                            s.R[i].Done = false;
                        }

                        i = i__prev2;
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
