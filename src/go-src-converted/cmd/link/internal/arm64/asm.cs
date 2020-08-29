// Inferno utils/5l/asm.c
// https://bitbucket.org/inferno-os/inferno-os/src/default/utils/5l/asm.c
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

// package arm64 -- go2cs converted at 2020 August 29 10:02:29 UTC
// import "cmd/link/internal/arm64" ==> using arm64 = go.cmd.link.@internal.arm64_package
// Original source: C:\Go\src\cmd\link\internal\arm64\asm.go
using objabi = go.cmd.@internal.objabi_package;
using sys = go.cmd.@internal.sys_package;
using ld = go.cmd.link.@internal.ld_package;
using sym = go.cmd.link.@internal.sym_package;
using elf = go.debug.elf_package;
using binary = go.encoding.binary_package;
using fmt = go.fmt_package;
using log = go.log_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace link {
namespace @internal
{
    public static partial class arm64_package
    {
        private static void gentext(ref ld.Link ctxt)
        {
            if (!ctxt.DynlinkingGo())
            {
                return;
            }
            var addmoduledata = ctxt.Syms.Lookup("runtime.addmoduledata", 0L);
            if (addmoduledata.Type == sym.STEXT)
            { 
                // we're linking a module containing the runtime -> no need for
                // an init function
                return;
            }
            addmoduledata.Attr |= sym.AttrReachable;
            var initfunc = ctxt.Syms.Lookup("go.link.addmoduledata", 0L);
            initfunc.Type = sym.STEXT;
            initfunc.Attr |= sym.AttrLocal;
            initfunc.Attr |= sym.AttrReachable;
            Action<uint> o = op =>
            {
                initfunc.AddUint32(ctxt.Arch, op);
            }; 
            // 0000000000000000 <local.dso_init>:
            // 0:    90000000     adrp    x0, 0 <runtime.firstmoduledata>
            //     0: R_AARCH64_ADR_PREL_PG_HI21    local.moduledata
            // 4:    91000000     add    x0, x0, #0x0
            //     4: R_AARCH64_ADD_ABS_LO12_NC    local.moduledata
            o(0x90000000UL);
            o(0x91000000UL);
            var rel = initfunc.AddRel();
            rel.Off = 0L;
            rel.Siz = 8L;
            rel.Sym = ctxt.Moduledata;
            rel.Type = objabi.R_ADDRARM64; 

            // 8:    14000000     bl    0 <runtime.addmoduledata>
            //     8: R_AARCH64_CALL26    runtime.addmoduledata
            o(0x14000000UL);
            rel = initfunc.AddRel();
            rel.Off = 8L;
            rel.Siz = 4L;
            rel.Sym = ctxt.Syms.Lookup("runtime.addmoduledata", 0L);
            rel.Type = objabi.R_CALLARM64; // Really should be R_AARCH64_JUMP26 but doesn't seem to make any difference

            ctxt.Textp = append(ctxt.Textp, initfunc);
            var initarray_entry = ctxt.Syms.Lookup("go.link.addmoduledatainit", 0L);
            initarray_entry.Attr |= sym.AttrReachable;
            initarray_entry.Attr |= sym.AttrLocal;
            initarray_entry.Type = sym.SINITARR;
            initarray_entry.AddAddr(ctxt.Arch, initfunc);
        }

        private static bool adddynrel(ref ld.Link ctxt, ref sym.Symbol s, ref sym.Reloc r)
        {
            log.Fatalf("adddynrel not implemented");
            return false;
        }

        private static bool elfreloc1(ref ld.Link ctxt, ref sym.Reloc r, long sectoff)
        {
            ctxt.Out.Write64(uint64(sectoff));

            var elfsym = r.Xsym.ElfsymForReloc();

            if (r.Type == objabi.R_ADDR) 
                switch (r.Siz)
                {
                    case 4L: 
                        ctxt.Out.Write64(uint64(elf.R_AARCH64_ABS32) | uint64(elfsym) << (int)(32L));
                        break;
                    case 8L: 
                        ctxt.Out.Write64(uint64(elf.R_AARCH64_ABS64) | uint64(elfsym) << (int)(32L));
                        break;
                    default: 
                        return false;
                        break;
                }
            else if (r.Type == objabi.R_ADDRARM64) 
                // two relocations: R_AARCH64_ADR_PREL_PG_HI21 and R_AARCH64_ADD_ABS_LO12_NC
                ctxt.Out.Write64(uint64(elf.R_AARCH64_ADR_PREL_PG_HI21) | uint64(elfsym) << (int)(32L));
                ctxt.Out.Write64(uint64(r.Xadd));
                ctxt.Out.Write64(uint64(sectoff + 4L));
                ctxt.Out.Write64(uint64(elf.R_AARCH64_ADD_ABS_LO12_NC) | uint64(elfsym) << (int)(32L));
            else if (r.Type == objabi.R_ARM64_TLS_LE) 
                ctxt.Out.Write64(uint64(elf.R_AARCH64_TLSLE_MOVW_TPREL_G0) | uint64(elfsym) << (int)(32L));
            else if (r.Type == objabi.R_ARM64_TLS_IE) 
                ctxt.Out.Write64(uint64(elf.R_AARCH64_TLSIE_ADR_GOTTPREL_PAGE21) | uint64(elfsym) << (int)(32L));
                ctxt.Out.Write64(uint64(r.Xadd));
                ctxt.Out.Write64(uint64(sectoff + 4L));
                ctxt.Out.Write64(uint64(elf.R_AARCH64_TLSIE_LD64_GOTTPREL_LO12_NC) | uint64(elfsym) << (int)(32L));
            else if (r.Type == objabi.R_ARM64_GOTPCREL) 
                ctxt.Out.Write64(uint64(elf.R_AARCH64_ADR_GOT_PAGE) | uint64(elfsym) << (int)(32L));
                ctxt.Out.Write64(uint64(r.Xadd));
                ctxt.Out.Write64(uint64(sectoff + 4L));
                ctxt.Out.Write64(uint64(elf.R_AARCH64_LD64_GOT_LO12_NC) | uint64(elfsym) << (int)(32L));
            else if (r.Type == objabi.R_CALLARM64) 
                if (r.Siz != 4L)
                {
                    return false;
                }
                ctxt.Out.Write64(uint64(elf.R_AARCH64_CALL26) | uint64(elfsym) << (int)(32L));
            else 
                return false;
                        ctxt.Out.Write64(uint64(r.Xadd));

            return true;
        }

        private static void elfsetupplt(ref ld.Link ctxt)
        { 
            // TODO(aram)
            return;
        }

        private static bool machoreloc1(ref sys.Arch arch, ref ld.OutBuf @out, ref sym.Symbol s, ref sym.Reloc r, long sectoff)
        {
            uint v = default;

            var rs = r.Xsym; 

            // ld64 has a bug handling MACHO_ARM64_RELOC_UNSIGNED with !extern relocation.
            // see cmd/internal/ld/data.go for details. The workaround is that don't use !extern
            // UNSIGNED relocation at all.
            if (rs.Type == sym.SHOSTOBJ || r.Type == objabi.R_CALLARM64 || r.Type == objabi.R_ADDRARM64 || r.Type == objabi.R_ADDR)
            {
                if (rs.Dynid < 0L)
                {
                    ld.Errorf(s, "reloc %d (%s) to non-macho symbol %s type=%d (%s)", r.Type, sym.RelocName(arch, r.Type), rs.Name, rs.Type, rs.Type);
                    return false;
                }
                v = uint32(rs.Dynid);
                v |= 1L << (int)(27L); // external relocation
            }
            else
            {
                v = uint32(rs.Sect.Extnum);
                if (v == 0L)
                {
                    ld.Errorf(s, "reloc %d (%s) to symbol %s in non-macho section %s type=%d (%s)", r.Type, sym.RelocName(arch, r.Type), rs.Name, rs.Sect.Name, rs.Type, rs.Type);
                    return false;
                }
            }

            if (r.Type == objabi.R_ADDR) 
                v |= ld.MACHO_ARM64_RELOC_UNSIGNED << (int)(28L);
            else if (r.Type == objabi.R_CALLARM64) 
                if (r.Xadd != 0L)
                {
                    ld.Errorf(s, "ld64 doesn't allow BR26 reloc with non-zero addend: %s+%d", rs.Name, r.Xadd);
                }
                v |= 1L << (int)(24L); // pc-relative bit
                v |= ld.MACHO_ARM64_RELOC_BRANCH26 << (int)(28L);
            else if (r.Type == objabi.R_ADDRARM64) 
                r.Siz = 4L; 
                // Two relocation entries: MACHO_ARM64_RELOC_PAGEOFF12 MACHO_ARM64_RELOC_PAGE21
                // if r.Xadd is non-zero, add two MACHO_ARM64_RELOC_ADDEND.
                if (r.Xadd != 0L)
                {
                    @out.Write32(uint32(sectoff + 4L));
                    @out.Write32((ld.MACHO_ARM64_RELOC_ADDEND << (int)(28L)) | (2L << (int)(25L)) | uint32(r.Xadd & 0xffffffUL));
                }
                @out.Write32(uint32(sectoff + 4L));
                @out.Write32(v | (ld.MACHO_ARM64_RELOC_PAGEOFF12 << (int)(28L)) | (2L << (int)(25L)));
                if (r.Xadd != 0L)
                {
                    @out.Write32(uint32(sectoff));
                    @out.Write32((ld.MACHO_ARM64_RELOC_ADDEND << (int)(28L)) | (2L << (int)(25L)) | uint32(r.Xadd & 0xffffffUL));
                }
                v |= 1L << (int)(24L); // pc-relative bit
                v |= ld.MACHO_ARM64_RELOC_PAGE21 << (int)(28L);
            else 
                return false;
                        switch (r.Siz)
            {
                case 1L: 
                    v |= 0L << (int)(25L);
                    break;
                case 2L: 
                    v |= 1L << (int)(25L);
                    break;
                case 4L: 
                    v |= 2L << (int)(25L);
                    break;
                case 8L: 
                    v |= 3L << (int)(25L);
                    break;
                default: 
                    return false;
                    break;
            }

            @out.Write32(uint32(sectoff));
            @out.Write32(v);
            return true;
        }

        private static bool archreloc(ref ld.Link ctxt, ref sym.Reloc r, ref sym.Symbol s, ref long val)
        {
            if (ctxt.LinkMode == ld.LinkExternal)
            {

                if (r.Type == objabi.R_ARM64_GOTPCREL)
                {
                    uint o1 = default;                    uint o2 = default;

                    if (ctxt.Arch.ByteOrder == binary.BigEndian)
                    {
                        o1 = uint32(val >> (int)(32L).Value);
                        o2 = uint32(val.Value);
                    }
                    else
                    {
                        o1 = uint32(val.Value);
                        o2 = uint32(val >> (int)(32L).Value);
                    } 
                    // Any relocation against a function symbol is redirected to
                    // be against a local symbol instead (see putelfsym in
                    // symtab.go) but unfortunately the system linker was buggy
                    // when confronted with a R_AARCH64_ADR_GOT_PAGE relocation
                    // against a local symbol until May 2015
                    // (https://sourceware.org/bugzilla/show_bug.cgi?id=18270). So
                    // we convert the adrp; ld64 + R_ARM64_GOTPCREL into adrp;
                    // add + R_ADDRARM64.
                    if (!(r.Sym.Version != 0L || r.Sym.Attr.VisibilityHidden() || r.Sym.Attr.Local()) && r.Sym.Type == sym.STEXT && ctxt.DynlinkingGo())
                    {
                        if (o2 & 0xffc00000UL != 0xf9400000UL)
                        {
                            ld.Errorf(s, "R_ARM64_GOTPCREL against unexpected instruction %x", o2);
                        }
                        o2 = 0x91000000UL | (o2 & 0x000003ffUL);
                        r.Type = objabi.R_ADDRARM64;
                    }
                    if (ctxt.Arch.ByteOrder == binary.BigEndian)
                    {
                        val.Value = int64(o1) << (int)(32L) | int64(o2);
                    }
                    else
                    {
                        val.Value = int64(o2) << (int)(32L) | int64(o1);
                    }
                    fallthrough = true;
                }
                if (fallthrough || r.Type == objabi.R_ADDRARM64)
                {
                    r.Done = false; 

                    // set up addend for eventual relocation via outer symbol.
                    var rs = r.Sym;
                    r.Xadd = r.Add;
                    while (rs.Outer != null)
                    {
                        r.Xadd += ld.Symaddr(rs) - ld.Symaddr(rs.Outer);
                        rs = rs.Outer;
                    }


                    if (rs.Type != sym.SHOSTOBJ && rs.Type != sym.SDYNIMPORT && rs.Sect == null)
                    {
                        ld.Errorf(s, "missing section for %s", rs.Name);
                    }
                    r.Xsym = rs; 

                    // Note: ld64 currently has a bug that any non-zero addend for BR26 relocation
                    // will make the linking fail because it thinks the code is not PIC even though
                    // the BR26 relocation should be fully resolved at link time.
                    // That is the reason why the next if block is disabled. When the bug in ld64
                    // is fixed, we can enable this block and also enable duff's device in cmd/7g.
                    if (false && ctxt.HeadType == objabi.Hdarwin)
                    {
                        uint o0 = default;                        o1 = default;



                        if (ctxt.Arch.ByteOrder == binary.BigEndian)
                        {
                            o0 = uint32(val >> (int)(32L).Value);
                            o1 = uint32(val.Value);
                        }
                        else
                        {
                            o0 = uint32(val.Value);
                            o1 = uint32(val >> (int)(32L).Value);
                        } 
                        // Mach-O wants the addend to be encoded in the instruction
                        // Note that although Mach-O supports ARM64_RELOC_ADDEND, it
                        // can only encode 24-bit of signed addend, but the instructions
                        // supports 33-bit of signed addend, so we always encode the
                        // addend in place.
                        o0 |= (uint32((r.Xadd >> (int)(12L)) & 3L) << (int)(29L)) | (uint32((r.Xadd >> (int)(12L) >> (int)(2L)) & 0x7ffffUL) << (int)(5L));
                        o1 |= uint32(r.Xadd & 0xfffUL) << (int)(10L);
                        r.Xadd = 0L; 

                        // when laid out, the instruction order must always be o1, o2.
                        if (ctxt.Arch.ByteOrder == binary.BigEndian)
                        {
                            val.Value = int64(o0) << (int)(32L) | int64(o1);
                        }
                        else
                        {
                            val.Value = int64(o1) << (int)(32L) | int64(o0);
                        }
                    }
                    return true;
                    goto __switch_break0;
                }
                if (r.Type == objabi.R_CALLARM64 || r.Type == objabi.R_ARM64_TLS_LE || r.Type == objabi.R_ARM64_TLS_IE)
                {
                    r.Done = false;
                    r.Xsym = r.Sym;
                    r.Xadd = r.Add;
                    return true;
                    goto __switch_break0;
                }
                // default: 
                    return false;

                __switch_break0:;
            }

            if (r.Type == objabi.R_CONST) 
                val.Value = r.Add;
                return true;
            else if (r.Type == objabi.R_GOTOFF) 
                val.Value = ld.Symaddr(r.Sym) + r.Add - ld.Symaddr(ctxt.Syms.Lookup(".got", 0L));
                return true;
            else if (r.Type == objabi.R_ADDRARM64) 
                var t = ld.Symaddr(r.Sym) + r.Add - ((s.Value + int64(r.Off)) & ~0xfffUL);
                if (t >= 1L << (int)(32L) || t < -1L << (int)(32L))
                {
                    ld.Errorf(s, "program too large, address relocation distance = %d", t);
                }
                o0 = default;                o1 = default;



                if (ctxt.Arch.ByteOrder == binary.BigEndian)
                {
                    o0 = uint32(val >> (int)(32L).Value);
                    o1 = uint32(val.Value);
                }
                else
                {
                    o0 = uint32(val.Value);
                    o1 = uint32(val >> (int)(32L).Value);
                }
                o0 |= (uint32((t >> (int)(12L)) & 3L) << (int)(29L)) | (uint32((t >> (int)(12L) >> (int)(2L)) & 0x7ffffUL) << (int)(5L));
                o1 |= uint32(t & 0xfffUL) << (int)(10L); 

                // when laid out, the instruction order must always be o1, o2.
                if (ctxt.Arch.ByteOrder == binary.BigEndian)
                {
                    val.Value = int64(o0) << (int)(32L) | int64(o1);
                }
                else
                {
                    val.Value = int64(o1) << (int)(32L) | int64(o0);
                }
                return true;
            else if (r.Type == objabi.R_ARM64_TLS_LE) 
                r.Done = false;
                if (ctxt.HeadType != objabi.Hlinux)
                {
                    ld.Errorf(s, "TLS reloc on unsupported OS %v", ctxt.HeadType);
                } 
                // The TCB is two pointers. This is not documented anywhere, but is
                // de facto part of the ABI.
                var v = r.Sym.Value + int64(2L * ctxt.Arch.PtrSize);
                if (v < 0L || v >= 32678L)
                {
                    ld.Errorf(s, "TLS offset out of range %d", v);
                }
                val.Value |= v << (int)(5L);
                return true;
            else if (r.Type == objabi.R_CALLARM64) 
                t = (ld.Symaddr(r.Sym) + r.Add) - (s.Value + int64(r.Off));
                if (t >= 1L << (int)(27L) || t < -1L << (int)(27L))
                {
                    ld.Errorf(s, "program too large, call relocation distance = %d", t);
                }
                val.Value |= (t >> (int)(2L)) & 0x03ffffffUL;
                return true;
                        return false;
        }

        private static long archrelocvariant(ref ld.Link ctxt, ref sym.Reloc r, ref sym.Symbol s, long t)
        {
            log.Fatalf("unexpected relocation variant");
            return -1L;
        }

        private static void asmb(ref ld.Link ctxt)
        {
            if (ctxt.Debugvlog != 0L)
            {
                ctxt.Logf("%5.2f asmb\n", ld.Cputime());
            }
            if (ctxt.IsELF)
            {
                ld.Asmbelfsetup();
            }
            var sect = ld.Segtext.Sections[0L];
            ctxt.Out.SeekSet(int64(sect.Vaddr - ld.Segtext.Vaddr + ld.Segtext.Fileoff));
            ld.Codeblk(ctxt, int64(sect.Vaddr), int64(sect.Length));
            foreach (var (_, __sect) in ld.Segtext.Sections[1L..])
            {
                sect = __sect;
                ctxt.Out.SeekSet(int64(sect.Vaddr - ld.Segtext.Vaddr + ld.Segtext.Fileoff));
                ld.Datblk(ctxt, int64(sect.Vaddr), int64(sect.Length));
            }

            if (ld.Segrodata.Filelen > 0L)
            {
                if (ctxt.Debugvlog != 0L)
                {
                    ctxt.Logf("%5.2f rodatblk\n", ld.Cputime());
                }
                ctxt.Out.SeekSet(int64(ld.Segrodata.Fileoff));
                ld.Datblk(ctxt, int64(ld.Segrodata.Vaddr), int64(ld.Segrodata.Filelen));
            }
            if (ld.Segrelrodata.Filelen > 0L)
            {
                if (ctxt.Debugvlog != 0L)
                {
                    ctxt.Logf("%5.2f relrodatblk\n", ld.Cputime());
                }
                ctxt.Out.SeekSet(int64(ld.Segrelrodata.Fileoff));
                ld.Datblk(ctxt, int64(ld.Segrelrodata.Vaddr), int64(ld.Segrelrodata.Filelen));
            }
            if (ctxt.Debugvlog != 0L)
            {
                ctxt.Logf("%5.2f datblk\n", ld.Cputime());
            }
            ctxt.Out.SeekSet(int64(ld.Segdata.Fileoff));
            ld.Datblk(ctxt, int64(ld.Segdata.Vaddr), int64(ld.Segdata.Filelen));

            ctxt.Out.SeekSet(int64(ld.Segdwarf.Fileoff));
            ld.Dwarfblk(ctxt, int64(ld.Segdwarf.Vaddr), int64(ld.Segdwarf.Filelen));

            var machlink = uint32(0L);
            if (ctxt.HeadType == objabi.Hdarwin)
            {
                machlink = uint32(ld.Domacholink(ctxt));
            } 

            /* output symbol table */
            ld.Symsize = 0L;

            ld.Lcsize = 0L;
            var symo = uint32(0L);
            if (!ld.FlagS.Value)
            { 
                // TODO: rationalize
                if (ctxt.Debugvlog != 0L)
                {
                    ctxt.Logf("%5.2f sym\n", ld.Cputime());
                }

                if (ctxt.HeadType == objabi.Hplan9) 
                    symo = uint32(ld.Segdata.Fileoff + ld.Segdata.Filelen);
                else if (ctxt.HeadType == objabi.Hdarwin) 
                    symo = uint32(ld.Segdwarf.Fileoff + uint64(ld.Rnd(int64(ld.Segdwarf.Filelen), int64(ld.FlagRound.Value))) + uint64(machlink));
                else 
                    if (ctxt.IsELF)
                    {
                        symo = uint32(ld.Segdwarf.Fileoff + ld.Segdwarf.Filelen);
                        symo = uint32(ld.Rnd(int64(symo), int64(ld.FlagRound.Value)));
                    }
                                ctxt.Out.SeekSet(int64(symo));

                if (ctxt.HeadType == objabi.Hplan9) 
                    ld.Asmplan9sym(ctxt);
                    ctxt.Out.Flush();

                    var sym = ctxt.Syms.Lookup("pclntab", 0L);
                    if (sym != null)
                    {
                        ld.Lcsize = int32(len(sym.P));
                        ctxt.Out.Write(sym.P);
                        ctxt.Out.Flush();
                    }
                else if (ctxt.HeadType == objabi.Hdarwin) 
                    if (ctxt.LinkMode == ld.LinkExternal)
                    {
                        ld.Machoemitreloc(ctxt);
                    }
                else 
                    if (ctxt.IsELF)
                    {
                        if (ctxt.Debugvlog != 0L)
                        {
                            ctxt.Logf("%5.2f elfsym\n", ld.Cputime());
                        }
                        ld.Asmelfsym(ctxt);
                        ctxt.Out.Flush();
                        ctxt.Out.Write(ld.Elfstrdat);

                        if (ctxt.LinkMode == ld.LinkExternal)
                        {
                            ld.Elfemitreloc(ctxt);
                        }
                    }
                            }
            if (ctxt.Debugvlog != 0L)
            {
                ctxt.Logf("%5.2f header\n", ld.Cputime());
            }
            ctxt.Out.SeekSet(0L);

            if (ctxt.HeadType == objabi.Hplan9) /* plan 9 */
                ctxt.Out.Write32(0x647UL); /* magic */
                ctxt.Out.Write32(uint32(ld.Segtext.Filelen)); /* sizes */
                ctxt.Out.Write32(uint32(ld.Segdata.Filelen));
                ctxt.Out.Write32(uint32(ld.Segdata.Length - ld.Segdata.Filelen));
                ctxt.Out.Write32(uint32(ld.Symsize)); /* nsyms */
                ctxt.Out.Write32(uint32(ld.Entryvalue(ctxt))); /* va of entry */
                ctxt.Out.Write32(0L);
                ctxt.Out.Write32(uint32(ld.Lcsize));
            else if (ctxt.HeadType == objabi.Hlinux || ctxt.HeadType == objabi.Hfreebsd || ctxt.HeadType == objabi.Hnetbsd || ctxt.HeadType == objabi.Hopenbsd || ctxt.HeadType == objabi.Hnacl) 
                ld.Asmbelf(ctxt, int64(symo));
            else if (ctxt.HeadType == objabi.Hdarwin) 
                ld.Asmbmacho(ctxt);
            else                         ctxt.Out.Flush();
            if (ld.FlagC.Value)
            {
                fmt.Printf("textsize=%d\n", ld.Segtext.Filelen);
                fmt.Printf("datsize=%d\n", ld.Segdata.Filelen);
                fmt.Printf("bsssize=%d\n", ld.Segdata.Length - ld.Segdata.Filelen);
                fmt.Printf("symsize=%d\n", ld.Symsize);
                fmt.Printf("lcsize=%d\n", ld.Lcsize);
                fmt.Printf("total=%d\n", ld.Segtext.Filelen + ld.Segdata.Length + uint64(ld.Symsize) + uint64(ld.Lcsize));
            }
        }
    }
}}}}
