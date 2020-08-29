// Derived from Inferno utils/6l/obj.c and utils/6l/span.c
// https://bitbucket.org/inferno-os/inferno-os/src/default/utils/6l/obj.c
// https://bitbucket.org/inferno-os/inferno-os/src/default/utils/6l/span.c
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

// package ld -- go2cs converted at 2020 August 29 10:03:15 UTC
// import "cmd/link/internal/ld" ==> using ld = go.cmd.link.@internal.ld_package
// Original source: C:\Go\src\cmd\link\internal\ld\data.go
using gcprog = go.cmd.@internal.gcprog_package;
using objabi = go.cmd.@internal.objabi_package;
using sys = go.cmd.@internal.sys_package;
using sym = go.cmd.link.@internal.sym_package;
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
        // isRuntimeDepPkg returns whether pkg is the runtime package or its dependency
        private static bool isRuntimeDepPkg(@string pkg)
        {
            switch (pkg)
            {
                case "runtime": // runtime may call to sync/atomic, due to go:linkname

                case "sync/atomic": // runtime may call to sync/atomic, due to go:linkname
                    return true;
                    break;
            }
            return strings.HasPrefix(pkg, "runtime/internal/") && !strings.HasSuffix(pkg, "_test");
        }

        // Estimate the max size needed to hold any new trampolines created for this function. This
        // is used to determine when the section can be split if it becomes too large, to ensure that
        // the trampolines are in the same section as the function that uses them.
        private static ulong maxSizeTrampolinesPPC64(ref sym.Symbol s, bool isTramp)
        { 
            // If Thearch.Trampoline is nil, then trampoline support is not available on this arch.
            // A trampoline does not need any dependent trampolines.
            if (Thearch.Trampoline == null || isTramp)
            {
                return 0L;
            }
            var n = uint64(0L);
            foreach (var (ri) in s.R)
            {
                var r = ref s.R[ri];
                if (r.Type.IsDirectJump())
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
        private static void trampoline(ref Link ctxt, ref sym.Symbol s)
        {
            if (Thearch.Trampoline == null)
            {
                return; // no need or no support of trampolines on this arch
            }
            foreach (var (ri) in s.R)
            {
                var r = ref s.R[ri];
                if (!r.Type.IsDirectJump())
                {
                    continue;
                }
                if (Symaddr(r.Sym) == 0L && r.Sym.Type != sym.SDYNIMPORT)
                {
                    if (r.Sym.File != s.File)
                    {
                        if (!isRuntimeDepPkg(s.File) || !isRuntimeDepPkg(r.Sym.File))
                        {
                            Errorf(s, "unresolved inter-package jump to %s(%s)", r.Sym, r.Sym.File);
                        } 
                        // runtime and its dependent packages may call to each other.
                        // they are fine, as they will be laid down together.
                    }
                    continue;
                }
                Thearch.Trampoline(ctxt, r, s);
            }
        }

        // resolve relocations in s.
        private static void relocsym(ref Link ctxt, ref sym.Symbol s)
        {
            for (var ri = int32(0L); ri < int32(len(s.R)); ri++)
            {
                var r = ref s.R[ri];
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
                if (r.Sym != null && ((r.Sym.Type == 0L && !r.Sym.Attr.VisibilityHidden()) || r.Sym.Type == sym.SXREF))
                { 
                    // When putting the runtime but not main into a shared library
                    // these symbols are undefined and that's OK.
                    if (ctxt.BuildMode == BuildModeShared)
                    {
                        if (r.Sym.Name == "main.main" || r.Sym.Name == "main.init")
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
                        Errorf(s, "relocation target %s not defined", r.Sym.Name);
                        continue;
                    }
                }
                if (r.Type >= 256L)
                {
                    continue;
                }
                if (r.Siz == 0L)
                { // informational relocation - no work to do
                    continue;
                }
                if (r.Type == objabi.R_DWARFFILEREF)
                { 
                    // These should have been processed previously during
                    // line table writing.
                    Errorf(s, "orphan R_DWARFFILEREF reloc to %v", r.Sym.Name);
                    continue;
                } 

                // We need to be able to reference dynimport symbols when linking against
                // shared libraries, and Solaris needs it always
                if (ctxt.HeadType != objabi.Hsolaris && r.Sym != null && r.Sym.Type == sym.SDYNIMPORT && !ctxt.DynlinkingGo() && !r.Sym.Attr.SubSymbol())
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

                // TODO(mundaym): remove this special case - see issue 14218.
                if (ctxt.Arch.Family == sys.S390X)
                {

                    if (r.Type == objabi.R_PCRELDBL) 
                        r.Type = objabi.R_PCREL;
                        r.Variant = sym.RV_390_DBL;
                    else if (r.Type == objabi.R_CALL) 
                        r.Variant = sym.RV_390_DBL;
                                    }
                long o = default;

                if (r.Type == objabi.R_TLS_LE)
                {
                    var isAndroidX86 = objabi.GOOS == "android" && (ctxt.Arch.InFamily(sys.AMD64, sys.I386));

                    if (ctxt.LinkMode == LinkExternal && ctxt.IsELF && !isAndroidX86)
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
                    else if (ctxt.IsELF || ctxt.HeadType == objabi.Hplan9 || ctxt.HeadType == objabi.Hdarwin || isAndroidX86)
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
                    isAndroidX86 = objabi.GOOS == "android" && (ctxt.Arch.InFamily(sys.AMD64, sys.I386));

                    if (ctxt.LinkMode == LinkExternal && ctxt.IsELF && !isAndroidX86)
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
                        if (Thearch.TLSIEtoLE == null)
                        {
                            log.Fatalf("internal linking of TLS IE not supported on %v", ctxt.Arch.Family);
                        }
                        Thearch.TLSIEtoLE(s, int(off), int(r.Siz));
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
                            // ld64 for arm64 has a bug where if the address pointed to by o exists in the
                            // symbol table (dynid >= 0), or is inside a symbol that exists in the symbol
                            // table, then it will add o twice into the relocated value.
                            // The workaround is that on arm64 don't ever add symaddr to o and always use
                            // extern relocation by requiring rs->dynid >= 0.
                            if (rs.Type != sym.SHOSTOBJ)
                            {
                                if (ctxt.Arch.Family == sys.ARM64 && rs.Dynid < 0L)
                                {
                                    Errorf(s, "R_ADDR reloc to %s+%d is not supported on darwin/arm64", rs.Name, o);
                                }
                                if (ctxt.Arch.Family != sys.ARM64)
                                {
                                    o += Symaddr(rs);
                                }
                            }
                        }
                        else if (ctxt.HeadType == objabi.Hwindows)
                        { 
                            // nothing to do
                        }
                        else
                        {
                            Errorf(s, "unhandled pcrel relocation to %s on %v", rs.Name, ctxt.HeadType);
                        }
                        break;
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
                    o = Symaddr(r.Sym) + r.Add - Symaddr(r.Sym.Lib.Textp[0L]); 

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
                                if (rs.Type != sym.SHOSTOBJ)
                                {
                                    o += int64(uint64(Symaddr(rs)) - rs.Sect.Vaddr);
                                }
                                o -= int64(r.Off); // relative to section offset, not symbol
                            }
                            else if (ctxt.Arch.Family == sys.ARM)
                            { 
                                // see ../arm/asm.go:/machoreloc1
                                o += Symaddr(rs) - int64(s.Value) - int64(r.Off);
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
                    if (!Thearch.Archreloc(ctxt, r, s, ref o))
                    {
                        Errorf(s, "unknown reloc to %v: %d (%s)", r.Sym.Name, r.Type, sym.RelocName(ctxt.Arch, r.Type));
                    }

                __switch_break0:;

                if (r.Variant != sym.RV_NONE)
                {
                    o = Thearch.Archrelocvariant(ctxt, r, s, o);
                }
                if (false)
                {
                    @string nam = "<nil>";
                    if (r.Sym != null)
                    {
                        nam = r.Sym.Name;
                    }
                    fmt.Printf("relocate %s %#x (%#x+%#x, size %d) => %s %#x +%#x [type %d (%s)/%d, %x]\n", s.Name, s.Value + int64(off), s.Value, r.Off, r.Siz, nam, Symaddr(r.Sym), r.Add, r.Type, sym.RelocName(ctxt.Arch, r.Type), r.Variant, o);
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

        private static void reloc(this ref Link ctxt)
        {
            if (ctxt.Debugvlog != 0L)
            {
                ctxt.Logf("%5.2f reloc\n", Cputime());
            }
            {
                var s__prev1 = s;

                foreach (var (_, __s) in ctxt.Textp)
                {
                    s = __s;
                    relocsym(ctxt, s);
                }

                s = s__prev1;
            }

            {
                var s__prev1 = s;

                foreach (var (_, __s) in datap)
                {
                    s = __s;
                    relocsym(ctxt, s);
                }

                s = s__prev1;
            }

            {
                var s__prev1 = s;

                foreach (var (_, __s) in dwarfp)
                {
                    s = __s;
                    relocsym(ctxt, s);
                }

                s = s__prev1;
            }

        }

        private static void windynrelocsym(ref Link ctxt, ref sym.Symbol s)
        {
            var rel = ctxt.Syms.Lookup(".rel", 0L);
            if (s == rel)
            {
                return;
            }
            for (long ri = 0L; ri < len(s.R); ri++)
            {
                var r = ref s.R[ri];
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
                if (r.Sym.Plt == -2L && r.Sym.Got != -2L)
                { // make dynimport JMP table for PE object files.
                    targ.Plt = int32(rel.Size);
                    r.Sym = rel;
                    r.Add = int64(targ.Plt); 

                    // jmp *addr
                    if (ctxt.Arch.Family == sys.I386)
                    {
                        rel.AddUint8(0xffUL);
                        rel.AddUint8(0x25UL);
                        rel.AddAddr(ctxt.Arch, targ);
                        rel.AddUint8(0x90UL);
                        rel.AddUint8(0x90UL);
                    }
                    else
                    {
                        rel.AddUint8(0xffUL);
                        rel.AddUint8(0x24UL);
                        rel.AddUint8(0x25UL);
                        rel.AddAddrPlus4(targ, 0L);
                        rel.AddUint8(0x90UL);
                    }
                }
                else if (r.Sym.Plt >= 0L)
                {
                    r.Sym = rel;
                    r.Add = int64(targ.Plt);
                }
            }

        }

        private static void dynrelocsym(ref Link ctxt, ref sym.Symbol s)
        {
            if (ctxt.HeadType == objabi.Hwindows)
            {
                if (ctxt.LinkMode == LinkInternal)
                {
                    windynrelocsym(ctxt, s);
                }
                return;
            }
            for (long ri = 0L; ri < len(s.R); ri++)
            {
                var r = ref s.R[ri];
                if (ctxt.BuildMode == BuildModePIE && ctxt.LinkMode == LinkInternal)
                { 
                    // It's expected that some relocations will be done
                    // later by relocsym (R_TLS_LE, R_ADDROFF), so
                    // don't worry if Adddynrel returns false.
                    Thearch.Adddynrel(ctxt, s, r);
                    continue;
                }
                if (r.Sym != null && r.Sym.Type == sym.SDYNIMPORT || r.Type >= 256L)
                {
                    if (r.Sym != null && !r.Sym.Attr.Reachable())
                    {
                        Errorf(s, "dynamic relocation to unreachable symbol %s", r.Sym.Name);
                    }
                    if (!Thearch.Adddynrel(ctxt, s, r))
                    {
                        Errorf(s, "unsupported dynamic relocation for symbol %s (type=%d (%s) stype=%d (%s))", r.Sym.Name, r.Type, sym.RelocName(ctxt.Arch, r.Type), r.Sym.Type, r.Sym.Type);
                    }
                }
            }

        }

        private static void dynreloc(ref Link ctxt, ref array<slice<ref sym.Symbol>> data)
        { 
            // -d suppresses dynamic loader format, so we may as well not
            // compute these sections or mark their symbols as reachable.
            if (FlagD && ctxt.HeadType != objabi.Hwindows.Value)
            {
                return;
            }
            if (ctxt.Debugvlog != 0L)
            {
                ctxt.Logf("%5.2f dynreloc\n", Cputime());
            }
            {
                var s__prev1 = s;

                foreach (var (_, __s) in ctxt.Textp)
                {
                    s = __s;
                    dynrelocsym(ctxt, s);
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
                        dynrelocsym(ctxt, s);
                    }

                    s = s__prev2;
                }

            }
            if (ctxt.IsELF)
            {
                elfdynhash(ctxt);
            }
        }

        public static void Codeblk(ref Link ctxt, long addr, long size)
        {
            CodeblkPad(ctxt, addr, size, zeros[..]);
        }
        public static void CodeblkPad(ref Link ctxt, long addr, long size, slice<byte> pad)
        {
            if (flagA.Value)
            {
                ctxt.Logf("codeblk [%#x,%#x) at offset %#x\n", addr, addr + size, ctxt.Out.Offset());
            }
            blk(ctxt, ctxt.Textp, addr, size, pad); 

            /* again for printing */
            if (!flagA.Value)
            {
                return;
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
            slice<byte> q = default;
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
                    q = s.P;

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

        private static void blk(ref Link ctxt, slice<ref sym.Symbol> syms, long addr, long size, slice<byte> pad)
        {
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
                        ctxt.Out.WriteStringPad("", int(s.Value - addr), pad);
                        addr = s.Value;
                    }
                    ctxt.Out.Write(s.P);
                    addr += int64(len(s.P));
                    if (addr < s.Value + s.Size)
                    {
                        ctxt.Out.WriteStringPad("", int(s.Value + s.Size - addr), pad);
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
                ctxt.Out.WriteStringPad("", int(eaddr - addr), pad);
            }
            ctxt.Out.Flush();
        }

        public static void Datblk(ref Link ctxt, long addr, long size)
        {
            if (flagA.Value)
            {
                ctxt.Logf("datblk [%#x,%#x) at offset %#x\n", addr, addr + size, ctxt.Out.Offset());
            }
            blk(ctxt, datap, addr, size, zeros[..]); 

            /* again for printing */
            if (!flagA.Value)
            {
                return;
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
                    foreach (var (_, r) in sym.R)
                    {
                        @string rsname = "";
                        if (r.Sym != null)
                        {
                            rsname = r.Sym.Name;
                        }
                        @string typ = "?";

                        if (r.Type == objabi.R_ADDR) 
                            typ = "addr";
                        else if (r.Type == objabi.R_PCREL) 
                            typ = "pcrel";
                        else if (r.Type == objabi.R_CALL) 
                            typ = "call";
                                                ctxt.Logf("\treloc %.8x/%d %s %s+%#x [%#x]\n", uint(sym.Value + int64(r.Off)), r.Siz, typ, rsname, r.Add, r.Sym.Value + r.Add);
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

        public static void Dwarfblk(ref Link ctxt, long addr, long size)
        {
            if (flagA.Value)
            {
                ctxt.Logf("dwarfblk [%#x,%#x) at offset %#x\n", addr, addr + size, ctxt.Out.Offset());
            }
            blk(ctxt, dwarfp, addr, size, zeros[..]);
        }

        private static array<byte> zeros = new array<byte>(512L);

        private static var strdata = make_map<@string, @string>();        private static slice<@string> strnames = default;

        private static void addstrdata1(ref Link ctxt, @string arg)
        {
            var eq = strings.Index(arg, "=");
            var dot = strings.LastIndex(arg[..eq + 1L], ".");
            if (eq < 0L || dot < 0L)
            {
                Exitf("-X flag requires argument of the form importpath.name=value");
            }
            var pkg = arg[..dot];
            if (ctxt.BuildMode == BuildModePlugin && pkg == "main")
            {
                pkg = flagPluginPath.Value;
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

        private static void addstrdata(ref Link ctxt, @string name, @string value)
        {
            var s = ctxt.Syms.ROLookup(name, 0L);
            if (s == null || s.Gotype == null)
            { 
                // Not defined in the loaded packages.
                return;
            }
            if (s.Gotype.Name != "type.string")
            {
                Errorf(s, "cannot set with -X: not a var of type string (%s)", s.Gotype.Name);
                return;
            }
            if (s.Type == sym.SBSS)
            {
                s.Type = sym.SDATA;
            }
            var p = fmt.Sprintf("%s.str", s.Name);
            var sp = ctxt.Syms.Lookup(p, 0L);

            Addstring(sp, value);
            sp.Type = sym.SRODATA;

            s.Size = 0L;
            s.P = s.P[..0L];
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

        private static void dostrdata(this ref Link ctxt)
        {
            foreach (var (_, name) in strnames)
            {
                addstrdata(ctxt, name, strdata[name]);
            }
        }

        public static long Addstring(ref sym.Symbol s, @string str)
        {
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
        private static void addgostring(ref Link ctxt, ref sym.Symbol s, @string symname, @string str)
        {
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

        private static void addinitarrdata(ref Link ctxt, ref sym.Symbol s)
        {
            var p = s.Name + ".ptr";
            var sp = ctxt.Syms.Lookup(p, 0L);
            sp.Type = sym.SINITARR;
            sp.Size = 0L;
            sp.Attr |= sym.AttrDuplicateOK;
            sp.AddAddr(ctxt.Arch, s);
        }

        private static void dosymtype(ref Link ctxt)
        {

            if (ctxt.BuildMode == BuildModeCArchive || ctxt.BuildMode == BuildModeCShared) 
                foreach (var (_, s) in ctxt.Syms.Allsym)
                { 
                    // Create a new entry in the .init_array section that points to the
                    // library initializer function.
                    if (s.Name == flagEntrySymbol.Value)
                    {
                        addinitarrdata(ctxt, s);
                    }
                }
                    }

        // symalign returns the required alignment for the given symbol s.
        private static int symalign(ref sym.Symbol s)
        {
            var min = int32(Thearch.Minalign);
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
            var align = int32(Thearch.Maxalign);
            while (int64(align) > s.Size && align > min)
            {
                align >>= 1L;
            }

            return align;
        }

        private static long aligndatsize(long datsize, ref sym.Symbol s)
        {
            return Rnd(datsize, int64(symalign(s)));
        }

        private static readonly var debugGCProg = false;



        public partial struct GCProg
        {
            public ptr<Link> ctxt;
            public ptr<sym.Symbol> sym;
            public gcprog.Writer w;
        }

        private static void Init(this ref GCProg p, ref Link ctxt, @string name)
        {
            p.ctxt = ctxt;
            p.sym = ctxt.Syms.Lookup(name, 0L);
            p.w.Init(p.writeByte(ctxt));
            if (debugGCProg)
            {
                fmt.Fprintf(os.Stderr, "ld: start GCProg %s\n", name);
                p.w.Debug(os.Stderr);
            }
        }

        private static Action<byte> writeByte(this ref GCProg p, ref Link ctxt)
        {
            return x =>
            {
                p.sym.AddUint8(x);
            }
;
        }

        private static void End(this ref GCProg p, long size)
        {
            p.w.ZeroUntil(size / int64(p.ctxt.Arch.PtrSize));
            p.w.End();
            if (debugGCProg)
            {
                fmt.Fprintf(os.Stderr, "ld: end GCProg\n");
            }
        }

        private static void AddSym(this ref GCProg p, ref sym.Symbol s)
        {
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
                        return;
                        break;
                }
                Errorf(s, "missing Go type information for global symbol: size %d", s.Size);
                return;
            }
            var ptrsize = int64(p.ctxt.Arch.PtrSize);
            var nptr = decodetypePtrdata(p.ctxt.Arch, typ) / ptrsize;

            if (debugGCProg)
            {
                fmt.Fprintf(os.Stderr, "gcprog sym: %s at %d (ptr=%d+%d)\n", s.Name, s.Value, s.Value / ptrsize, nptr);
            }
            if (decodetypeUsegcprog(p.ctxt.Arch, typ) == 0L)
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

                return;
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
        private static readonly float cutoff = 2e9F; // 2 GB (or so; looks better in errors than 2^31)

 // 2 GB (or so; looks better in errors than 2^31)

        private static void checkdatsize(ref Link ctxt, long datsize, sym.SymKind symn)
        {
            if (datsize > cutoff)
            {
                Errorf(null, "too much data in section %v (over %v bytes)", symn, cutoff);
            }
        }

        // datap is a collection of reachable data symbols in address order.
        // Generated by dodata.
        private static slice<ref sym.Symbol> datap = default;

        private static void dodata(this ref Link ctxt)
        {
            if (ctxt.Debugvlog != 0L)
            {
                ctxt.Logf("%5.2f dodata\n", Cputime());
            }
            if (ctxt.DynlinkingGo() && ctxt.HeadType == objabi.Hdarwin)
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
                var bss = ctxt.Syms.Lookup("runtime.bss", 0L);
                bss.Size = 8L;
                bss.Attr.Set(sym.AttrSpecial, false);

                ctxt.Syms.Lookup("runtime.ebss", 0L).Attr.Set(sym.AttrSpecial, false);

                var data = ctxt.Syms.Lookup("runtime.data", 0L);
                data.Size = 8L;
                data.Attr.Set(sym.AttrSpecial, false);

                ctxt.Syms.Lookup("runtime.edata", 0L).Attr.Set(sym.AttrSpecial, false);

                var types = ctxt.Syms.Lookup("runtime.types", 0L);
                types.Type = sym.STYPE;
                types.Size = 8L;
                types.Attr.Set(sym.AttrSpecial, false);

                var etypes = ctxt.Syms.Lookup("runtime.etypes", 0L);
                etypes.Type = sym.SFUNCTAB;
                etypes.Attr.Set(sym.AttrSpecial, false);
            } 

            // Collect data symbols by type into data.
            data = new array<slice<ref sym.Symbol>>(sym.SXREF);
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
            dynreloc(ctxt, ref data);

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

                        ref sym.Symbol ro = new slice<ref sym.Symbol>(new ref sym.Symbol[] {  });
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
                        data[symn], dataMaxAlign[symn] = dodataSect(ctxt, symn, data[symn]);
                        wg.Done();
                    }());
                }

                symn = symn__prev1;
            }

            wg.Wait(); 

            // Allocate sections.
            // Data is processed before segtext, because we need
            // to see all symbols in the .data and .bss sections in order
            // to generate garbage collection information.
            var datsize = int64(0L); 

            // Writable data sections that do not need any specialized handling.
            sym.SymKind writable = new slice<sym.SymKind>(new sym.SymKind[] { sym.SELFSECT, sym.SMACHO, sym.SMACHOGOT, sym.SWINDOWS });
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
                            var sect = addsection(ctxt.Arch, ref Segdata, s.Name, 06L);
                            sect.Align = symalign(s);
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

                    checkdatsize(ctxt, datsize, symn);
                } 

                // .got (and .toc on ppc64)

                symn = symn__prev1;
            }

            if (len(data[sym.SELFGOT]) > 0L)
            {
                sect = addsection(ctxt.Arch, ref Segdata, ".got", 06L);
                sect.Align = dataMaxAlign[sym.SELFGOT];
                datsize = Rnd(datsize, int64(sect.Align));
                sect.Vaddr = uint64(datsize);
                ref sym.Symbol toc = default;
                {
                    var s__prev1 = s;

                    foreach (var (_, __s) in data[sym.SELFGOT])
                    {
                        s = __s;
                        datsize = aligndatsize(datsize, s);
                        s.Sect = sect;
                        s.Type = sym.SDATA;
                        s.Value = int64(uint64(datsize) - sect.Vaddr); 

                        // Resolve .TOC. symbol for this object file (ppc64)
                        toc = ctxt.Syms.ROLookup(".TOC.", int(s.Version));
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

                checkdatsize(ctxt, datsize, sym.SELFGOT);
                sect.Length = uint64(datsize) - sect.Vaddr;
            } 

            /* pointer-free data */
            sect = addsection(ctxt.Arch, ref Segdata, ".noptrdata", 06L);
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
                    datsize = aligndatsize(datsize, s);
                    s.Sect = sect;
                    s.Type = sym.SDATA;
                    s.Value = int64(uint64(datsize) - sect.Vaddr);
                    datsize += s.Size;
                }

                s = s__prev1;
            }

            checkdatsize(ctxt, datsize, sym.SNOPTRDATA);
            sect.Length = uint64(datsize) - sect.Vaddr;

            var hasinitarr = ctxt.linkShared; 

            /* shared library initializer */

            if (ctxt.BuildMode == BuildModeCArchive || ctxt.BuildMode == BuildModeCShared || ctxt.BuildMode == BuildModeShared || ctxt.BuildMode == BuildModePlugin) 
                hasinitarr = true;
                        if (hasinitarr)
            {
                sect = addsection(ctxt.Arch, ref Segdata, ".init_array", 06L);
                sect.Align = dataMaxAlign[sym.SINITARR];
                datsize = Rnd(datsize, int64(sect.Align));
                sect.Vaddr = uint64(datsize);
                {
                    var s__prev1 = s;

                    foreach (var (_, __s) in data[sym.SINITARR])
                    {
                        s = __s;
                        datsize = aligndatsize(datsize, s);
                        s.Sect = sect;
                        s.Value = int64(uint64(datsize) - sect.Vaddr);
                        datsize += s.Size;
                    }

                    s = s__prev1;
                }

                sect.Length = uint64(datsize) - sect.Vaddr;
                checkdatsize(ctxt, datsize, sym.SINITARR);
            } 

            /* data */
            sect = addsection(ctxt.Arch, ref Segdata, ".data", 06L);
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
                    datsize = aligndatsize(datsize, s);
                    s.Value = int64(uint64(datsize) - sect.Vaddr);
                    gc.AddSym(s);
                    datsize += s.Size;
                }

                s = s__prev1;
            }

            checkdatsize(ctxt, datsize, sym.SDATA);
            sect.Length = uint64(datsize) - sect.Vaddr;
            gc.End(int64(sect.Length)); 

            /* bss */
            sect = addsection(ctxt.Arch, ref Segdata, ".bss", 06L);
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
                    datsize = aligndatsize(datsize, s);
                    s.Value = int64(uint64(datsize) - sect.Vaddr);
                    gc.AddSym(s);
                    datsize += s.Size;
                }

                s = s__prev1;
            }

            checkdatsize(ctxt, datsize, sym.SBSS);
            sect.Length = uint64(datsize) - sect.Vaddr;
            gc.End(int64(sect.Length)); 

            /* pointer-free bss */
            sect = addsection(ctxt.Arch, ref Segdata, ".noptrbss", 06L);
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
                    datsize = aligndatsize(datsize, s);
                    s.Sect = sect;
                    s.Value = int64(uint64(datsize) - sect.Vaddr);
                    datsize += s.Size;
                }

                s = s__prev1;
            }

            sect.Length = uint64(datsize) - sect.Vaddr;
            ctxt.Syms.Lookup("runtime.end", 0L).Sect = sect;
            checkdatsize(ctxt, datsize, sym.SNOPTRBSS);

            if (len(data[sym.STLSBSS]) > 0L)
            {
                sect = default;
                if (ctxt.IsELF && (ctxt.LinkMode == LinkExternal || !FlagD.Value))
                {
                    sect = addsection(ctxt.Arch, ref Segdata, ".tbss", 06L);
                    sect.Align = int32(ctxt.Arch.PtrSize);
                    sect.Vaddr = 0L;
                }
                datsize = 0L;

                {
                    var s__prev1 = s;

                    foreach (var (_, __s) in data[sym.STLSBSS])
                    {
                        s = __s;
                        datsize = aligndatsize(datsize, s);
                        s.Sect = sect;
                        s.Value = datsize;
                        datsize += s.Size;
                    }

                    s = s__prev1;
                }

                checkdatsize(ctxt, datsize, sym.STLSBSS);

                if (sect != null)
                {
                    sect.Length = uint64(datsize);
                }
            }

            /*
                 * We finished data, begin read-only data.
                 * Not all systems support a separate read-only non-executable data section.
                 * ELF systems do.
                 * OS X and Plan 9 do not.
                 * Windows PE may, but if so we have not implemented it.
                 * And if we're using external linking mode, the point is moot,
                 * since it's not our decision; that code expects the sections in
                 * segtext.
                 */
            ref sym.Segment segro = default;
            if (ctxt.IsELF && ctxt.LinkMode == LinkInternal)
            {
                segro = ref Segrodata;
            }
            else
            {
                segro = ref Segtext;
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
                    sect = addsection(ctxt.Arch, ref Segtext, s.Name, 04L);
                    sect.Align = symalign(s);
                    datsize = Rnd(datsize, int64(sect.Align));
                    sect.Vaddr = uint64(datsize);
                    s.Sect = sect;
                    s.Type = sym.SRODATA;
                    s.Value = int64(uint64(datsize) - sect.Vaddr);
                    datsize += s.Size;
                    sect.Length = uint64(datsize) - sect.Vaddr;
                    checkdatsize(ctxt, datsize, sym.SELFRXSECT);
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
                    {
                        var s__prev2 = s;

                        foreach (var (_, __s) in data[symn])
                        {
                            s = __s;
                            datsize = aligndatsize(datsize, s);
                            s.Sect = sect;
                            s.Type = sym.SRODATA;
                            s.Value = int64(uint64(datsize) - sect.Vaddr);
                            datsize += s.Size;
                        }

                        s = s__prev2;
                    }

                    checkdatsize(ctxt, datsize, symn);
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
                    sect.Align = symalign(s);
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

            checkdatsize(ctxt, datsize, sym.SELFROSECT);

            {
                var s__prev1 = s;

                foreach (var (_, __s) in data[sym.SMACHOPLT])
                {
                    s = __s;
                    sect = addsection(ctxt.Arch, segro, s.Name, 04L);
                    sect.Align = symalign(s);
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

            checkdatsize(ctxt, datsize, sym.SMACHOPLT); 

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
            Func<@string, ref sym.Section> addrelrosection = suffix =>
            {
                return addsection(ctxt.Arch, segro, suffix, 04L);
            }
;

            if (ctxt.UseRelro())
            {
                addrelrosection = suffix =>
                {
                    var seg = ref Segrelrodata;
                    if (ctxt.LinkMode == LinkExternal)
                    { 
                        // Using a separate segment with an external
                        // linker results in some programs moving
                        // their data sections unexpectedly, which
                        // corrupts the moduledata. So we use the
                        // rodata segment and let the external linker
                        // sort out a rel.ro segment.
                        seg = ref Segrodata;
                    }
                    return addsection(ctxt.Arch, seg, ".data.rel.ro" + suffix, 06L);
                } 
                /* data only written by relocations */
; 
                /* data only written by relocations */
                sect = addrelrosection("");

                sect.Vaddr = 0L;
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
                {
                    var symnro__prev1 = symnro;

                    foreach (var (_, __symnro) in sym.ReadOnly)
                    {
                        symnro = __symnro;
                        symn = sym.RelROMap[symnro];
                        {
                            var s__prev2 = s;

                            foreach (var (_, __s) in data[symn])
                            {
                                s = __s;
                                datsize = aligndatsize(datsize, s);
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

                        checkdatsize(ctxt, datsize, symn);
                    }

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
            checkdatsize(ctxt, datsize, sym.STYPELINK);
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
                    datsize = aligndatsize(datsize, s);
                    s.Sect = sect;
                    s.Type = sym.SRODATA;
                    s.Value = int64(uint64(datsize) - sect.Vaddr);
                    datsize += s.Size;
                }

                s = s__prev1;
            }

            checkdatsize(ctxt, datsize, sym.SITABLINK);
            sect.Length = uint64(datsize) - sect.Vaddr; 

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
                    datsize = aligndatsize(datsize, s);
                    s.Sect = sect;
                    s.Type = sym.SRODATA;
                    s.Value = int64(uint64(datsize) - sect.Vaddr);
                    datsize += s.Size;
                }

                s = s__prev1;
            }

            checkdatsize(ctxt, datsize, sym.SSYMTAB);
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
                    datsize = aligndatsize(datsize, s);
                    s.Sect = sect;
                    s.Type = sym.SRODATA;
                    s.Value = int64(uint64(datsize) - sect.Vaddr);
                    datsize += s.Size;
                }

                s = s__prev1;
            }

            checkdatsize(ctxt, datsize, sym.SRODATA);
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

            dwarfgeneratedebugsyms(ctxt);

            long i = default;
            while (i < len(dwarfp))
            {
                var s = dwarfp[i];
                if (s.Type != sym.SDWARFSECT)
                {
                    break;
                i++;
                }
                sect = addsection(ctxt.Arch, ref Segdwarf, s.Name, 04L);
                sect.Align = 1L;
                datsize = Rnd(datsize, int64(sect.Align));
                sect.Vaddr = uint64(datsize);
                s.Sect = sect;
                s.Type = sym.SRODATA;
                s.Value = int64(uint64(datsize) - sect.Vaddr);
                datsize += s.Size;
                sect.Length = uint64(datsize) - sect.Vaddr;
            }

            checkdatsize(ctxt, datsize, sym.SDWARFSECT);

            while (i < len(dwarfp))
            {
                var curType = dwarfp[i].Type;
                sect = default;

                if (curType == sym.SDWARFINFO) 
                    sect = addsection(ctxt.Arch, ref Segdwarf, ".debug_info", 04L);
                else if (curType == sym.SDWARFRANGE) 
                    sect = addsection(ctxt.Arch, ref Segdwarf, ".debug_ranges", 04L);
                else if (curType == sym.SDWARFLOC) 
                    sect = addsection(ctxt.Arch, ref Segdwarf, ".debug_loc", 04L);
                else 
                    Errorf(dwarfp[i], "unknown DWARF section %v", curType);
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
                }

                sect.Length = uint64(datsize) - sect.Vaddr;
                checkdatsize(ctxt, datsize, curType);
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

        }

        private static (slice<ref sym.Symbol>, int) dodataSect(ref Link ctxt, sym.SymKind symn, slice<ref sym.Symbol> syms)
        {
            if (ctxt.HeadType == objabi.Hdarwin)
            { 
                // Some symbols may no longer belong in syms
                // due to movement in machosymorder.
                var newSyms = make_slice<ref sym.Symbol>(0L, len(syms));
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
            ref sym.Symbol head = default;            ref sym.Symbol tail = default;

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
                    if (ctxt.DynlinkingGo() && ctxt.HeadType == objabi.Hdarwin)
                    {
                        switch (s.Name)
                        {
                            case "runtime.text": 

                            case "runtime.bss": 

                            case "runtime.data": 

                            case "runtime.types": 
                                head = s;
                                continue;
                                break;
                            case "runtime.etext": 

                            case "runtime.ebss": 

                            case "runtime.edata": 

                            case "runtime.etypes": 
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
                    var align = symalign(symSort.sym);
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
        private static void textbuildid(this ref Link ctxt)
        {
            if (ctxt.IsELF || ctxt.BuildMode == BuildModePlugin || flagBuildid == "".Value)
            {
                return;
            }
            var s = ctxt.Syms.Lookup("go.buildid", 0L);
            s.Attr |= sym.AttrReachable; 
            // The \xff is invalid UTF-8, meant to make it less likely
            // to find one of these accidentally.
            @string data = "\xff Go build ID: " + strconv.Quote(flagBuildid.Value) + "\n \xff";
            s.Type = sym.STEXT;
            s.P = (slice<byte>)data;
            s.Size = int64(len(s.P));

            ctxt.Textp = append(ctxt.Textp, null);
            copy(ctxt.Textp[1L..], ctxt.Textp);
            ctxt.Textp[0L] = s;
        }

        // assign addresses to text
        private static void textaddress(this ref Link ctxt)
        {
            addsection(ctxt.Arch, ref Segtext, ".text", 05L); 

            // Assign PCs in text segment.
            // Could parallelize, by assigning to text
            // and then letting threads copy down, but probably not worth it.
            var sect = Segtext.Sections[0L];

            sect.Align = int32(Funcalign);

            var text = ctxt.Syms.Lookup("runtime.text", 0L);
            text.Sect = sect;

            if (ctxt.DynlinkingGo() && ctxt.HeadType == objabi.Hdarwin)
            {
                var etext = ctxt.Syms.Lookup("runtime.etext", 0L);
                etext.Sect = sect;

                ctxt.Textp = append(ctxt.Textp, etext, null);
                copy(ctxt.Textp[1L..], ctxt.Textp);
                ctxt.Textp[0L] = text;
            }
            var va = uint64(FlagTextAddr.Value);
            long n = 1L;
            sect.Vaddr = va;
            long ntramps = 0L;
            {
                var s__prev1 = s;

                foreach (var (_, __s) in ctxt.Textp)
                {
                    s = __s;
                    sect, n, va = assignAddress(ctxt, sect, n, s, va, false);

                    trampoline(ctxt, s); // resolve jumps, may add trampolines if jump too far

                    // lay down trampolines after each function
                    while (ntramps < len(ctxt.tramps))
                    {
                        var tramp = ctxt.tramps[ntramps];
                        sect, n, va = assignAddress(ctxt, sect, n, tramp, va, true);
                        ntramps++;
                    }

                }

                s = s__prev1;
            }

            sect.Length = va - sect.Vaddr;
            ctxt.Syms.Lookup("runtime.etext", 0L).Sect = sect; 

            // merge tramps into Textp, keeping Textp in address order
            if (ntramps != 0L)
            {
                var newtextp = make_slice<ref sym.Symbol>(0L, len(ctxt.Textp) + ntramps);
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
        private static (ref sym.Section, long, ulong) assignAddress(ref Link ctxt, ref sym.Section sect, long n, ref sym.Symbol s, ulong va, bool isTramp)
        {
            s.Sect = sect;
            if (s.Attr.SubSymbol())
            {
                return (sect, n, va);
            }
            if (s.Align != 0L)
            {
                va = uint64(Rnd(int64(va), int64(s.Align)));
            }
            else
            {
                va = uint64(Rnd(int64(va), int64(Funcalign)));
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

            var funcsize = uint64(MINFUNC); // spacing required for findfunctab
            if (s.Size > MINFUNC)
            {
                funcsize = uint64(s.Size);
            } 

            // On ppc64x a text section should not be larger than 2^26 bytes due to the size of
            // call target offset field in the bl instruction.  Splitting into smaller text
            // sections smaller than this limit allows the GNU linker to modify the long calls
            // appropriately.  The limit allows for the space needed for tables inserted by the linker.

            // If this function doesn't fit in the current text section, then create a new one.

            // Only break at outermost syms.
            if (ctxt.Arch.InFamily(sys.PPC64) && s.Outer == null && ctxt.IsELF && ctxt.LinkMode == LinkExternal && va - sect.Vaddr + funcsize + maxSizeTrampolinesPPC64(s, isTramp) > 0x1c00000UL)
            {
                // Set the length for the previous text section
                sect.Length = va - sect.Vaddr; 

                // Create new section, set the starting Vaddr
                sect = addsection(ctxt.Arch, ref Segtext, ".text", 05L);
                sect.Vaddr = va;
                s.Sect = sect; 

                // Create a symbol for the start of the secondary text sections
                ctxt.Syms.Lookup(fmt.Sprintf("runtime.text.%d", n), 0L).Sect = sect;
                n++;
            }
            va += funcsize;

            return (sect, n, va);
        }

        // assign addresses
        private static void address(this ref Link ctxt)
        {
            var va = uint64(FlagTextAddr.Value);
            Segtext.Rwx = 05L;
            Segtext.Vaddr = va;
            Segtext.Fileoff = uint64(HEADR);
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

            Segtext.Length = va - uint64(FlagTextAddr.Value);
            Segtext.Filelen = Segtext.Length;
            if (ctxt.HeadType == objabi.Hnacl)
            {
                va += 32L; // room for the "halt sled"
            }
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
                va = uint64(Rnd(int64(va), int64(FlagRound.Value)));

                Segrodata.Rwx = 04L;
                Segrodata.Vaddr = va;
                Segrodata.Fileoff = va - Segtext.Vaddr + Segtext.Fileoff;
                Segrodata.Filelen = 0L;
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
                Segrodata.Filelen = Segrodata.Length;
            }
            if (len(Segrelrodata.Sections) > 0L)
            { 
                // align to page boundary so as not to mix
                // rodata, rel-ro data, and executable text.
                va = uint64(Rnd(int64(va), int64(FlagRound.Value)));

                Segrelrodata.Rwx = 06L;
                Segrelrodata.Vaddr = va;
                Segrelrodata.Fileoff = va - Segrodata.Vaddr + Segrodata.Fileoff;
                Segrelrodata.Filelen = 0L;
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
                Segrelrodata.Filelen = Segrelrodata.Length;
            }
            va = uint64(Rnd(int64(va), int64(FlagRound.Value)));
            Segdata.Rwx = 06L;
            Segdata.Vaddr = va;
            Segdata.Fileoff = va - Segtext.Vaddr + Segtext.Fileoff;
            Segdata.Filelen = 0L;
            if (ctxt.HeadType == objabi.Hwindows)
            {
                Segdata.Fileoff = Segtext.Fileoff + uint64(Rnd(int64(Segtext.Length), PEFILEALIGN));
            }
            if (ctxt.HeadType == objabi.Hplan9)
            {
                Segdata.Fileoff = Segtext.Fileoff + Segtext.Filelen;
            }
            ref sym.Section data = default;
            ref sym.Section noptr = default;
            ref sym.Section bss = default;
            ref sym.Section noptrbss = default;
            {
                var i__prev1 = i;
                var s__prev1 = s;

                foreach (var (__i, __s) in Segdata.Sections)
                {
                    i = __i;
                    s = __s;
                    if (ctxt.IsELF && s.Name == ".tbss")
                    {
                        continue;
                    }
                    var vlen = int64(s.Length);
                    if (i + 1L < len(Segdata.Sections) && !(ctxt.IsELF && Segdata.Sections[i + 1L].Name == ".tbss"))
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

                i = i__prev1;
                s = s__prev1;
            }

            Segdata.Filelen = bss.Vaddr - Segdata.Vaddr;

            va = uint64(Rnd(int64(va), int64(FlagRound.Value)));
            Segdwarf.Rwx = 06L;
            Segdwarf.Vaddr = va;
            Segdwarf.Fileoff = Segdata.Fileoff + uint64(Rnd(int64(Segdata.Filelen), int64(FlagRound.Value)));
            Segdwarf.Filelen = 0L;
            if (ctxt.HeadType == objabi.Hwindows)
            {
                Segdwarf.Fileoff = Segdata.Fileoff + uint64(Rnd(int64(Segdata.Filelen), int64(PEFILEALIGN)));
            }
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

            Segdwarf.Filelen = va - Segdwarf.Vaddr;

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
                    ctxt.xdefine(symname, sym.STEXT, int64(sect.Vaddr));
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
        }

        // add a trampoline with symbol s (to be laid down after the current function)
        private static void AddTramp(this ref Link ctxt, ref sym.Symbol s)
        {
            s.Type = sym.STEXT;
            s.Attr |= sym.AttrReachable;
            s.Attr |= sym.AttrOnList;
            ctxt.tramps = append(ctxt.tramps, s);
            if (FlagDebugTramp > 0L && ctxt.Debugvlog > 0L.Value)
            {
                ctxt.Logf("trampoline %s inserted\n", s);
            }
        }
    }
}}}}
