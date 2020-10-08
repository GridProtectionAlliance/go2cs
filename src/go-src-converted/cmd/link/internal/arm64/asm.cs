// Inferno utils/5l/asm.c
// https://bitbucket.org/inferno-os/inferno-os/src/master/utils/5l/asm.c
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

// package arm64 -- go2cs converted at 2020 October 08 04:37:20 UTC
// import "cmd/link/internal/arm64" ==> using arm64 = go.cmd.link.@internal.arm64_package
// Original source: C:\Go\src\cmd\link\internal\arm64\asm.go
using objabi = go.cmd.@internal.objabi_package;
using sys = go.cmd.@internal.sys_package;
using ld = go.cmd.link.@internal.ld_package;
using loader = go.cmd.link.@internal.loader_package;
using sym = go.cmd.link.@internal.sym_package;
using elf = go.debug.elf_package;
using fmt = go.fmt_package;
using log = go.log_package;
using sync = go.sync_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace link {
namespace @internal
{
    public static partial class arm64_package
    {
        private static void gentext2(ptr<ld.Link> _addr_ctxt, ptr<loader.Loader> _addr_ldr)
        {
            ref ld.Link ctxt = ref _addr_ctxt.val;
            ref loader.Loader ldr = ref _addr_ldr.val;

            var (initfunc, addmoduledata) = ld.PrepareAddmoduledata(ctxt);
            if (initfunc == null)
            {
                return ;
            }
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
            loader.Reloc rel = new loader.Reloc(Off:0,Size:8,Type:objabi.R_ADDRARM64,Sym:ctxt.Moduledata2,);
            initfunc.AddReloc(rel); 

            // 8:    14000000     b    0 <runtime.addmoduledata>
            //     8: R_AARCH64_CALL26    runtime.addmoduledata
            o(0x14000000UL);
            loader.Reloc rel2 = new loader.Reloc(Off:8,Size:4,Type:objabi.R_CALLARM64,Sym:addmoduledata,);
            initfunc.AddReloc(rel2);

        }

        private static bool adddynrel2(ptr<ld.Target> _addr_target, ptr<loader.Loader> _addr_ldr, ptr<ld.ArchSyms> _addr_syms, loader.Sym s, loader.Reloc2 r, long rIdx)
        {
            ref ld.Target target = ref _addr_target.val;
            ref loader.Loader ldr = ref _addr_ldr.val;
            ref ld.ArchSyms syms = ref _addr_syms.val;

            var targ = r.Sym();
            sym.SymKind targType = default;
            if (targ != 0L)
            {
                targType = ldr.SymType(targ);
            }


            if (r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_AARCH64_PREL32)) 
                if (targType == sym.SDYNIMPORT)
                {
                    ldr.Errorf(s, "unexpected R_AARCH64_PREL32 relocation for dynamic symbol %s", ldr.SymName(targ));
                } 
                // TODO(mwhudson): the test of VisibilityHidden here probably doesn't make
                // sense and should be removed when someone has thought about it properly.
                if ((targType == 0L || targType == sym.SXREF) && !ldr.AttrVisibilityHidden(targ))
                {
                    ldr.Errorf(s, "unknown symbol %s in pcrel", ldr.SymName(targ));
                }

                var su = ldr.MakeSymbolUpdater(s);
                su.SetRelocType(rIdx, objabi.R_PCREL);
                su.SetRelocAdd(rIdx, r.Add() + 4L);
                return true;
            else if (r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_AARCH64_PREL64)) 
                if (targType == sym.SDYNIMPORT)
                {
                    ldr.Errorf(s, "unexpected R_AARCH64_PREL64 relocation for dynamic symbol %s", ldr.SymName(targ));
                }

                if (targType == 0L || targType == sym.SXREF)
                {
                    ldr.Errorf(s, "unknown symbol %s in pcrel", ldr.SymName(targ));
                }

                su = ldr.MakeSymbolUpdater(s);
                su.SetRelocType(rIdx, objabi.R_PCREL);
                su.SetRelocAdd(rIdx, r.Add() + 8L);
                return true;
            else if (r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_AARCH64_CALL26) || r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_AARCH64_JUMP26)) 
                if (targType == sym.SDYNIMPORT)
                {
                    addpltsym2(_addr_target, _addr_ldr, _addr_syms, targ);
                    su = ldr.MakeSymbolUpdater(s);
                    su.SetRelocSym(rIdx, syms.PLT2);
                    su.SetRelocAdd(rIdx, r.Add() + int64(ldr.SymPlt(targ)));
                }

                if ((targType == 0L || targType == sym.SXREF) && !ldr.AttrVisibilityHidden(targ))
                {
                    ldr.Errorf(s, "unknown symbol %s in callarm64", ldr.SymName(targ));
                }

                su = ldr.MakeSymbolUpdater(s);
                su.SetRelocType(rIdx, objabi.R_CALLARM64);
                return true;
            else if (r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_AARCH64_ADR_GOT_PAGE) || r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_AARCH64_LD64_GOT_LO12_NC)) 
                if (targType != sym.SDYNIMPORT)
                { 
                    // have symbol
                    // TODO: turn LDR of GOT entry into ADR of symbol itself
                } 

                // fall back to using GOT
                // TODO: just needs relocation, no need to put in .dynsym
                addgotsym2(_addr_target, _addr_ldr, _addr_syms, targ);
                su = ldr.MakeSymbolUpdater(s);
                su.SetRelocType(rIdx, objabi.R_ARM64_GOT);
                su.SetRelocSym(rIdx, syms.GOT2);
                su.SetRelocAdd(rIdx, r.Add() + int64(ldr.SymGot(targ)));
                return true;
            else if (r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_AARCH64_ADR_PREL_PG_HI21) || r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_AARCH64_ADD_ABS_LO12_NC)) 
                if (targType == sym.SDYNIMPORT)
                {
                    ldr.Errorf(s, "unexpected relocation for dynamic symbol %s", ldr.SymName(targ));
                }

                if (targType == 0L || targType == sym.SXREF)
                {
                    ldr.Errorf(s, "unknown symbol %s", ldr.SymName(targ));
                }

                su = ldr.MakeSymbolUpdater(s);
                su.SetRelocType(rIdx, objabi.R_ARM64_PCREL);
                return true;
            else if (r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_AARCH64_ABS64)) 
                if (targType == sym.SDYNIMPORT)
                {
                    ldr.Errorf(s, "unexpected R_AARCH64_ABS64 relocation for dynamic symbol %s", ldr.SymName(targ));
                }

                su = ldr.MakeSymbolUpdater(s);
                su.SetRelocType(rIdx, objabi.R_ADDR);
                if (target.IsPIE() && target.IsInternal())
                { 
                    // For internal linking PIE, this R_ADDR relocation cannot
                    // be resolved statically. We need to generate a dynamic
                    // relocation. Let the code below handle it.
                    break;

                }

                return true;
            else if (r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_AARCH64_LDST8_ABS_LO12_NC)) 
                if (targType == sym.SDYNIMPORT)
                {
                    ldr.Errorf(s, "unexpected relocation for dynamic symbol %s", ldr.SymName(targ));
                }

                su = ldr.MakeSymbolUpdater(s);
                su.SetRelocType(rIdx, objabi.R_ARM64_LDST8);
                return true;
            else if (r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_AARCH64_LDST32_ABS_LO12_NC)) 
                if (targType == sym.SDYNIMPORT)
                {
                    ldr.Errorf(s, "unexpected relocation for dynamic symbol %s", ldr.SymName(targ));
                }

                su = ldr.MakeSymbolUpdater(s);
                su.SetRelocType(rIdx, objabi.R_ARM64_LDST32);
                return true;
            else if (r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_AARCH64_LDST64_ABS_LO12_NC)) 
                if (targType == sym.SDYNIMPORT)
                {
                    ldr.Errorf(s, "unexpected relocation for dynamic symbol %s", ldr.SymName(targ));
                }

                su = ldr.MakeSymbolUpdater(s);
                su.SetRelocType(rIdx, objabi.R_ARM64_LDST64);

                return true;
            else if (r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_AARCH64_LDST128_ABS_LO12_NC)) 
                if (targType == sym.SDYNIMPORT)
                {
                    ldr.Errorf(s, "unexpected relocation for dynamic symbol %s", ldr.SymName(targ));
                }

                su = ldr.MakeSymbolUpdater(s);
                su.SetRelocType(rIdx, objabi.R_ARM64_LDST128);
                return true;
            else 
                if (r.Type() >= objabi.ElfRelocOffset)
                {
                    ldr.Errorf(s, "unexpected relocation type %d (%s)", r.Type(), sym.RelocName(target.Arch, r.Type()));
                    return false;
                } 

                // Handle relocations found in ELF object files.
            // Reread the reloc to incorporate any changes in type above.
            var relocs = ldr.Relocs(s);
            r = relocs.At2(rIdx);


            if (r.Type() == objabi.R_CALL || r.Type() == objabi.R_PCREL || r.Type() == objabi.R_CALLARM64) 
                if (targType != sym.SDYNIMPORT)
                { 
                    // nothing to do, the relocation will be laid out in reloc
                    return true;

                }

                if (target.IsExternal())
                { 
                    // External linker will do this relocation.
                    return true;

                }

            else if (r.Type() == objabi.R_ADDR) 
                if (ldr.SymType(s) == sym.STEXT && target.IsElf())
                { 
                    // The code is asking for the address of an external
                    // function. We provide it with the address of the
                    // correspondent GOT symbol.
                    addgotsym2(_addr_target, _addr_ldr, _addr_syms, targ);
                    su = ldr.MakeSymbolUpdater(s);
                    su.SetRelocSym(rIdx, syms.GOT2);
                    su.SetRelocAdd(rIdx, r.Add() + int64(ldr.SymGot(targ)));
                    return true;

                } 

                // Process dynamic relocations for the data sections.
                if (target.IsPIE() && target.IsInternal())
                { 
                    // When internally linking, generate dynamic relocations
                    // for all typical R_ADDR relocations. The exception
                    // are those R_ADDR that are created as part of generating
                    // the dynamic relocations and must be resolved statically.
                    //
                    // There are three phases relevant to understanding this:
                    //
                    //    dodata()  // we are here
                    //    address() // symbol address assignment
                    //    reloc()   // resolution of static R_ADDR relocs
                    //
                    // At this point symbol addresses have not been
                    // assigned yet (as the final size of the .rela section
                    // will affect the addresses), and so we cannot write
                    // the Elf64_Rela.r_offset now. Instead we delay it
                    // until after the 'address' phase of the linker is
                    // complete. We do this via Addaddrplus, which creates
                    // a new R_ADDR relocation which will be resolved in
                    // the 'reloc' phase.
                    //
                    // These synthetic static R_ADDR relocs must be skipped
                    // now, or else we will be caught in an infinite loop
                    // of generating synthetic relocs for our synthetic
                    // relocs.
                    //
                    // Furthermore, the rela sections contain dynamic
                    // relocations with R_ADDR relocations on
                    // Elf64_Rela.r_offset. This field should contain the
                    // symbol offset as determined by reloc(), not the
                    // final dynamically linked address as a dynamic
                    // relocation would provide.
                    switch (ldr.SymName(s))
                    {
                        case ".dynsym": 

                        case ".rela": 

                        case ".rela.plt": 

                        case ".got.plt": 

                        case ".dynamic": 
                            return false;
                            break;
                    }

                }
                else
                { 
                    // Either internally linking a static executable,
                    // in which case we can resolve these relocations
                    // statically in the 'reloc' phase, or externally
                    // linking, in which case the relocation will be
                    // prepared in the 'reloc' phase and passed to the
                    // external linker in the 'asmb' phase.
                    if (ldr.SymType(s) != sym.SDATA && ldr.SymType(s) != sym.SRODATA)
                    {
                        break;
                    }

                }

                if (target.IsElf())
                { 
                    // Generate R_AARCH64_RELATIVE relocations for best
                    // efficiency in the dynamic linker.
                    //
                    // As noted above, symbol addresses have not been
                    // assigned yet, so we can't generate the final reloc
                    // entry yet. We ultimately want:
                    //
                    // r_offset = s + r.Off
                    // r_info = R_AARCH64_RELATIVE
                    // r_addend = targ + r.Add
                    //
                    // The dynamic linker will set *offset = base address +
                    // addend.
                    //
                    // AddAddrPlus is used for r_offset and r_addend to
                    // generate new R_ADDR relocations that will update
                    // these fields in the 'reloc' phase.
                    var rela = ldr.MakeSymbolUpdater(syms.Rela2);
                    rela.AddAddrPlus(target.Arch, s, int64(r.Off()));
                    if (r.Siz() == 8L)
                    {
                        rela.AddUint64(target.Arch, ld.ELF64_R_INFO(0L, uint32(elf.R_AARCH64_RELATIVE)));
                    }
                    else
                    {
                        ldr.Errorf(s, "unexpected relocation for dynamic symbol %s", ldr.SymName(targ));
                    }

                    rela.AddAddrPlus(target.Arch, targ, int64(r.Add())); 
                    // Not mark r done here. So we still apply it statically,
                    // so in the file content we'll also have the right offset
                    // to the relocation target. So it can be examined statically
                    // (e.g. go version).
                    return true;

                }

                        return false;

        }

        private static bool elfreloc1(ptr<ld.Link> _addr_ctxt, ptr<sym.Reloc> _addr_r, long sectoff)
        {
            ref ld.Link ctxt = ref _addr_ctxt.val;
            ref sym.Reloc r = ref _addr_r.val;

            ctxt.Out.Write64(uint64(sectoff));

            var elfsym = ld.ElfSymForReloc(ctxt, r.Xsym);

            if (r.Type == objabi.R_ADDR || r.Type == objabi.R_DWARFSECREF) 
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

        private static bool machoreloc1(ptr<sys.Arch> _addr_arch, ptr<ld.OutBuf> _addr_@out, ptr<sym.Symbol> _addr_s, ptr<sym.Reloc> _addr_r, long sectoff)
        {
            ref sys.Arch arch = ref _addr_arch.val;
            ref ld.OutBuf @out = ref _addr_@out.val;
            ref sym.Symbol s = ref _addr_s.val;
            ref sym.Reloc r = ref _addr_r.val;

            uint v = default;

            var rs = r.Xsym;

            if (rs.Type == sym.SHOSTOBJ || r.Type == objabi.R_CALLARM64 || r.Type == objabi.R_ADDRARM64)
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

        private static (long, bool) archreloc(ptr<ld.Target> _addr_target, ptr<ld.ArchSyms> _addr_syms, ptr<sym.Reloc> _addr_r, ptr<sym.Symbol> _addr_s, long val)
        {
            long _p0 = default;
            bool _p0 = default;
            ref ld.Target target = ref _addr_target.val;
            ref ld.ArchSyms syms = ref _addr_syms.val;
            ref sym.Reloc r = ref _addr_r.val;
            ref sym.Symbol s = ref _addr_s.val;

            if (target.IsExternal())
            {

                if (r.Type == objabi.R_ARM64_GOTPCREL)
                {
                    uint o1 = default;                    uint o2 = default;

                    if (target.IsBigEndian())
                    {
                        o1 = uint32(val >> (int)(32L));
                        o2 = uint32(val);
                    }
                    else
                    {
                        o1 = uint32(val);
                        o2 = uint32(val >> (int)(32L));
                    } 
                    // Any relocation against a function symbol is redirected to
                    // be against a local symbol instead (see putelfsym in
                    // symtab.go) but unfortunately the system linker was buggy
                    // when confronted with a R_AARCH64_ADR_GOT_PAGE relocation
                    // against a local symbol until May 2015
                    // (https://sourceware.org/bugzilla/show_bug.cgi?id=18270). So
                    // we convert the adrp; ld64 + R_ARM64_GOTPCREL into adrp;
                    // add + R_ADDRARM64.
                    if (!(r.Sym.IsFileLocal() || r.Sym.Attr.VisibilityHidden() || r.Sym.Attr.Local()) && r.Sym.Type == sym.STEXT && target.IsDynlinkingGo())
                    {
                        if (o2 & 0xffc00000UL != 0xf9400000UL)
                        {
                            ld.Errorf(s, "R_ARM64_GOTPCREL against unexpected instruction %x", o2);
                        }

                        o2 = 0x91000000UL | (o2 & 0x000003ffUL);
                        r.Type = objabi.R_ADDRARM64;

                    }

                    if (target.IsBigEndian())
                    {
                        val = int64(o1) << (int)(32L) | int64(o2);
                    }
                    else
                    {
                        val = int64(o2) << (int)(32L) | int64(o1);
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
                    if (false && target.IsDarwin())
                    {
                        uint o0 = default;                        o1 = default;



                        if (target.IsBigEndian())
                        {
                            o0 = uint32(val >> (int)(32L));
                            o1 = uint32(val);
                        }
                        else
                        {
                            o0 = uint32(val);
                            o1 = uint32(val >> (int)(32L));
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
                        if (target.IsBigEndian())
                        {
                            val = int64(o0) << (int)(32L) | int64(o1);
                        }
                        else
                        {
                            val = int64(o1) << (int)(32L) | int64(o0);
                        }

                    }

                    return (val, true);
                    goto __switch_break0;
                }
                if (r.Type == objabi.R_CALLARM64 || r.Type == objabi.R_ARM64_TLS_LE || r.Type == objabi.R_ARM64_TLS_IE)
                {
                    r.Done = false;
                    r.Xsym = r.Sym;
                    r.Xadd = r.Add;
                    return (val, true);
                    goto __switch_break0;
                }
                // default: 
                    return (val, false);

                __switch_break0:;

            }


            if (r.Type == objabi.R_CONST) 
                return (r.Add, true);
            else if (r.Type == objabi.R_GOTOFF) 
                return (ld.Symaddr(r.Sym) + r.Add - ld.Symaddr(syms.GOT), true);
            else if (r.Type == objabi.R_ADDRARM64) 
                var t = ld.Symaddr(r.Sym) + r.Add - ((s.Value + int64(r.Off)) & ~0xfffUL);
                if (t >= 1L << (int)(32L) || t < -1L << (int)(32L))
                {
                    ld.Errorf(s, "program too large, address relocation distance = %d", t);
                }

                o0 = default;                o1 = default;



                if (target.IsBigEndian())
                {
                    o0 = uint32(val >> (int)(32L));
                    o1 = uint32(val);
                }
                else
                {
                    o0 = uint32(val);
                    o1 = uint32(val >> (int)(32L));
                }

                o0 |= (uint32((t >> (int)(12L)) & 3L) << (int)(29L)) | (uint32((t >> (int)(12L) >> (int)(2L)) & 0x7ffffUL) << (int)(5L));
                o1 |= uint32(t & 0xfffUL) << (int)(10L); 

                // when laid out, the instruction order must always be o1, o2.
                if (target.IsBigEndian())
                {
                    return (int64(o0) << (int)(32L) | int64(o1), true);
                }

                return (int64(o1) << (int)(32L) | int64(o0), true);
            else if (r.Type == objabi.R_ARM64_TLS_LE) 
                r.Done = false;
                if (target.IsDarwin())
                {
                    ld.Errorf(s, "TLS reloc on unsupported OS %v", target.HeadType);
                } 
                // The TCB is two pointers. This is not documented anywhere, but is
                // de facto part of the ABI.
                var v = r.Sym.Value + int64(2L * target.Arch.PtrSize);
                if (v < 0L || v >= 32678L)
                {
                    ld.Errorf(s, "TLS offset out of range %d", v);
                }

                return (val | (v << (int)(5L)), true);
            else if (r.Type == objabi.R_ARM64_TLS_IE) 
                if (target.IsPIE() && target.IsElf())
                { 
                    // We are linking the final executable, so we
                    // can optimize any TLS IE relocation to LE.
                    r.Done = false;
                    if (!target.IsLinux())
                    {
                        ld.Errorf(s, "TLS reloc on unsupported OS %v", target.HeadType);
                    } 

                    // The TCB is two pointers. This is not documented anywhere, but is
                    // de facto part of the ABI.
                    v = ld.Symaddr(r.Sym) + int64(2L * target.Arch.PtrSize) + r.Add;
                    if (v < 0L || v >= 32678L)
                    {
                        ld.Errorf(s, "TLS offset out of range %d", v);
                    }

                    o0 = default;                    o1 = default;

                    if (target.IsBigEndian())
                    {
                        o0 = uint32(val >> (int)(32L));
                        o1 = uint32(val);
                    }
                    else
                    {
                        o0 = uint32(val);
                        o1 = uint32(val >> (int)(32L));
                    } 

                    // R_AARCH64_TLSIE_ADR_GOTTPREL_PAGE21
                    // turn ADRP to MOVZ
                    o0 = 0xd2a00000UL | uint32(o0 & 0x1fUL) | (uint32((v >> (int)(16L)) & 0xffffUL) << (int)(5L)); 
                    // R_AARCH64_TLSIE_LD64_GOTTPREL_LO12_NC
                    // turn LD64 to MOVK
                    if (v & 3L != 0L)
                    {
                        ld.Errorf(s, "invalid address: %x for relocation type: R_AARCH64_TLSIE_LD64_GOTTPREL_LO12_NC", v);
                    }

                    o1 = 0xf2800000UL | uint32(o1 & 0x1fUL) | (uint32(v & 0xffffUL) << (int)(5L)); 

                    // when laid out, the instruction order must always be o0, o1.
                    if (target.IsBigEndian())
                    {
                        return (int64(o0) << (int)(32L) | int64(o1), true);
                    }

                    return (int64(o1) << (int)(32L) | int64(o0), true);

                }
                else
                {
                    log.Fatalf("cannot handle R_ARM64_TLS_IE (sym %s) when linking internally", s.Name);
                }

            else if (r.Type == objabi.R_CALLARM64) 
                t = default;
                if (r.Sym.Type == sym.SDYNIMPORT)
                {
                    t = (ld.Symaddr(syms.PLT) + r.Add) - (s.Value + int64(r.Off));
                }
                else
                {
                    t = (ld.Symaddr(r.Sym) + r.Add) - (s.Value + int64(r.Off));
                }

                if (t >= 1L << (int)(27L) || t < -1L << (int)(27L))
                {
                    ld.Errorf(s, "program too large, call relocation distance = %d", t);
                }

                return (val | ((t >> (int)(2L)) & 0x03ffffffUL), true);
            else if (r.Type == objabi.R_ARM64_GOT) 
                if (s.P[r.Off + 3L] & 0x9fUL == 0x90UL)
                { 
                    // R_AARCH64_ADR_GOT_PAGE
                    // patch instruction: adrp
                    t = ld.Symaddr(r.Sym) + r.Add - ((s.Value + int64(r.Off)) & ~0xfffUL);
                    if (t >= 1L << (int)(32L) || t < -1L << (int)(32L))
                    {
                        ld.Errorf(s, "program too large, address relocation distance = %d", t);
                    }

                    o0 = default;
                    o0 |= (uint32((t >> (int)(12L)) & 3L) << (int)(29L)) | (uint32((t >> (int)(12L) >> (int)(2L)) & 0x7ffffUL) << (int)(5L));
                    return (val | int64(o0), true);

                }
                else if (s.P[r.Off + 3L] == 0xf9UL)
                { 
                    // R_AARCH64_LD64_GOT_LO12_NC
                    // patch instruction: ldr
                    t = ld.Symaddr(r.Sym) + r.Add - ((s.Value + int64(r.Off)) & ~0xfffUL);
                    if (t & 7L != 0L)
                    {
                        ld.Errorf(s, "invalid address: %x for relocation type: R_AARCH64_LD64_GOT_LO12_NC", t);
                    }

                    o1 = default;
                    o1 |= uint32(t & 0xfffUL) << (int)((10L - 3L));
                    return (val | int64(uint64(o1)), true);

                }
                else
                {
                    ld.Errorf(s, "unsupported instruction for %v R_GOTARM64", s.P[r.Off..r.Off + 4L]);
                }

            else if (r.Type == objabi.R_ARM64_PCREL) 
                if (s.P[r.Off + 3L] & 0x9fUL == 0x90UL)
                { 
                    // R_AARCH64_ADR_PREL_PG_HI21
                    // patch instruction: adrp
                    t = ld.Symaddr(r.Sym) + r.Add - ((s.Value + int64(r.Off)) & ~0xfffUL);
                    if (t >= 1L << (int)(32L) || t < -1L << (int)(32L))
                    {
                        ld.Errorf(s, "program too large, address relocation distance = %d", t);
                    }

                    o0 = (uint32((t >> (int)(12L)) & 3L) << (int)(29L)) | (uint32((t >> (int)(12L) >> (int)(2L)) & 0x7ffffUL) << (int)(5L));
                    return (val | int64(o0), true);

                }
                else if (s.P[r.Off + 3L] & 0x91UL == 0x91UL)
                { 
                    // R_AARCH64_ADD_ABS_LO12_NC
                    // patch instruction: add
                    t = ld.Symaddr(r.Sym) + r.Add - ((s.Value + int64(r.Off)) & ~0xfffUL);
                    o1 = uint32(t & 0xfffUL) << (int)(10L);
                    return (val | int64(o1), true);

                }
                else
                {
                    ld.Errorf(s, "unsupported instruction for %v R_PCRELARM64", s.P[r.Off..r.Off + 4L]);
                }

            else if (r.Type == objabi.R_ARM64_LDST8) 
                t = ld.Symaddr(r.Sym) + r.Add - ((s.Value + int64(r.Off)) & ~0xfffUL);
                o0 = uint32(t & 0xfffUL) << (int)(10L);
                return (val | int64(o0), true);
            else if (r.Type == objabi.R_ARM64_LDST32) 
                t = ld.Symaddr(r.Sym) + r.Add - ((s.Value + int64(r.Off)) & ~0xfffUL);
                if (t & 3L != 0L)
                {
                    ld.Errorf(s, "invalid address: %x for relocation type: R_AARCH64_LDST32_ABS_LO12_NC", t);
                }

                o0 = (uint32(t & 0xfffUL) >> (int)(2L)) << (int)(10L);
                return (val | int64(o0), true);
            else if (r.Type == objabi.R_ARM64_LDST64) 
                t = ld.Symaddr(r.Sym) + r.Add - ((s.Value + int64(r.Off)) & ~0xfffUL);
                if (t & 7L != 0L)
                {
                    ld.Errorf(s, "invalid address: %x for relocation type: R_AARCH64_LDST64_ABS_LO12_NC", t);
                }

                o0 = (uint32(t & 0xfffUL) >> (int)(3L)) << (int)(10L);
                return (val | int64(o0), true);
            else if (r.Type == objabi.R_ARM64_LDST128) 
                t = ld.Symaddr(r.Sym) + r.Add - ((s.Value + int64(r.Off)) & ~0xfffUL);
                if (t & 15L != 0L)
                {
                    ld.Errorf(s, "invalid address: %x for relocation type: R_AARCH64_LDST128_ABS_LO12_NC", t);
                }

                o0 = (uint32(t & 0xfffUL) >> (int)(4L)) << (int)(10L);
                return (val | int64(o0), true);
                        return (val, false);

        }

        private static long archrelocvariant(ptr<ld.Target> _addr_target, ptr<ld.ArchSyms> _addr_syms, ptr<sym.Reloc> _addr_r, ptr<sym.Symbol> _addr_s, long t)
        {
            ref ld.Target target = ref _addr_target.val;
            ref ld.ArchSyms syms = ref _addr_syms.val;
            ref sym.Reloc r = ref _addr_r.val;
            ref sym.Symbol s = ref _addr_s.val;

            log.Fatalf("unexpected relocation variant");
            return -1L;
        }

        private static void elfsetupplt(ptr<ld.Link> _addr_ctxt, ptr<loader.SymbolBuilder> _addr_plt, ptr<loader.SymbolBuilder> _addr_gotplt, loader.Sym dynamic)
        {
            ref ld.Link ctxt = ref _addr_ctxt.val;
            ref loader.SymbolBuilder plt = ref _addr_plt.val;
            ref loader.SymbolBuilder gotplt = ref _addr_gotplt.val;

            if (plt.Size() == 0L)
            { 
                // stp     x16, x30, [sp, #-16]!
                // identifying information
                plt.AddUint32(ctxt.Arch, 0xa9bf7bf0UL); 

                // the following two instructions (adrp + ldr) load *got[2] into x17
                // adrp    x16, &got[0]
                plt.AddSymRef(ctxt.Arch, gotplt.Sym(), 16L, objabi.R_ARM64_GOT, 4L);
                plt.SetUint32(ctxt.Arch, plt.Size() - 4L, 0x90000010UL); 

                // <imm> is the offset value of &got[2] to &got[0], the same below
                // ldr     x17, [x16, <imm>]
                plt.AddSymRef(ctxt.Arch, gotplt.Sym(), 16L, objabi.R_ARM64_GOT, 4L);
                plt.SetUint32(ctxt.Arch, plt.Size() - 4L, 0xf9400211UL); 

                // add     x16, x16, <imm>
                plt.AddSymRef(ctxt.Arch, gotplt.Sym(), 16L, objabi.R_ARM64_PCREL, 4L);
                plt.SetUint32(ctxt.Arch, plt.Size() - 4L, 0x91000210UL); 

                // br      x17
                plt.AddUint32(ctxt.Arch, 0xd61f0220UL); 

                // 3 nop for place holder
                plt.AddUint32(ctxt.Arch, 0xd503201fUL);
                plt.AddUint32(ctxt.Arch, 0xd503201fUL);
                plt.AddUint32(ctxt.Arch, 0xd503201fUL); 

                // check gotplt.size == 0
                if (gotplt.Size() != 0L)
                {
                    ctxt.Errorf(gotplt.Sym(), "got.plt is not empty at the very beginning");
                }

                gotplt.AddAddrPlus(ctxt.Arch, dynamic, 0L);

                gotplt.AddUint64(ctxt.Arch, 0L);
                gotplt.AddUint64(ctxt.Arch, 0L);

            }

        }

        private static void addpltsym2(ptr<ld.Target> _addr_target, ptr<loader.Loader> _addr_ldr, ptr<ld.ArchSyms> _addr_syms, loader.Sym s) => func((_, panic, __) =>
        {
            ref ld.Target target = ref _addr_target.val;
            ref loader.Loader ldr = ref _addr_ldr.val;
            ref ld.ArchSyms syms = ref _addr_syms.val;

            if (ldr.SymPlt(s) >= 0L)
            {
                return ;
            }

            ld.Adddynsym2(ldr, target, syms, s);

            if (target.IsElf())
            {
                var plt = ldr.MakeSymbolUpdater(syms.PLT2);
                var gotplt = ldr.MakeSymbolUpdater(syms.GOTPLT2);
                var rela = ldr.MakeSymbolUpdater(syms.RelaPLT2);
                if (plt.Size() == 0L)
                {
                    panic("plt is not set up");
                } 

                // adrp    x16, &got.plt[0]
                plt.AddAddrPlus4(target.Arch, gotplt.Sym(), gotplt.Size());
                plt.SetUint32(target.Arch, plt.Size() - 4L, 0x90000010UL);
                var relocs = plt.Relocs();
                plt.SetRelocType(relocs.Count() - 1L, objabi.R_ARM64_GOT); 

                // <offset> is the offset value of &got.plt[n] to &got.plt[0]
                // ldr     x17, [x16, <offset>]
                plt.AddAddrPlus4(target.Arch, gotplt.Sym(), gotplt.Size());
                plt.SetUint32(target.Arch, plt.Size() - 4L, 0xf9400211UL);
                relocs = plt.Relocs();
                plt.SetRelocType(relocs.Count() - 1L, objabi.R_ARM64_GOT); 

                // add     x16, x16, <offset>
                plt.AddAddrPlus4(target.Arch, gotplt.Sym(), gotplt.Size());
                plt.SetUint32(target.Arch, plt.Size() - 4L, 0x91000210UL);
                relocs = plt.Relocs();
                plt.SetRelocType(relocs.Count() - 1L, objabi.R_ARM64_PCREL); 

                // br      x17
                plt.AddUint32(target.Arch, 0xd61f0220UL); 

                // add to got.plt: pointer to plt[0]
                gotplt.AddAddrPlus(target.Arch, plt.Sym(), 0L); 

                // rela
                rela.AddAddrPlus(target.Arch, gotplt.Sym(), gotplt.Size() - 8L);
                var sDynid = ldr.SymDynid(s);

                rela.AddUint64(target.Arch, ld.ELF64_R_INFO(uint32(sDynid), uint32(elf.R_AARCH64_JUMP_SLOT)));
                rela.AddUint64(target.Arch, 0L);

                ldr.SetPlt(s, int32(plt.Size() - 16L));

            }
            else
            {
                ldr.Errorf(s, "addpltsym: unsupported binary format");
            }

        });

        private static void addgotsym2(ptr<ld.Target> _addr_target, ptr<loader.Loader> _addr_ldr, ptr<ld.ArchSyms> _addr_syms, loader.Sym s)
        {
            ref ld.Target target = ref _addr_target.val;
            ref loader.Loader ldr = ref _addr_ldr.val;
            ref ld.ArchSyms syms = ref _addr_syms.val;

            if (ldr.SymGot(s) >= 0L)
            {
                return ;
            }

            ld.Adddynsym2(ldr, target, syms, s);
            var got = ldr.MakeSymbolUpdater(syms.GOT2);
            ldr.SetGot(s, int32(got.Size()));
            got.AddUint64(target.Arch, 0L);

            if (target.IsElf())
            {
                var rela = ldr.MakeSymbolUpdater(syms.Rela2);
                rela.AddAddrPlus(target.Arch, got.Sym(), int64(ldr.SymGot(s)));
                rela.AddUint64(target.Arch, ld.ELF64_R_INFO(uint32(ldr.SymDynid(s)), uint32(elf.R_AARCH64_GLOB_DAT)));
                rela.AddUint64(target.Arch, 0L);
            }
            else
            {
                ldr.Errorf(s, "addgotsym: unsupported binary format");
            }

        }

        private static void asmb(ptr<ld.Link> _addr_ctxt, ptr<loader.Loader> _addr__)
        {
            ref ld.Link ctxt = ref _addr_ctxt.val;
            ref loader.Loader _ = ref _addr__.val;

            if (ctxt.IsELF)
            {
                ld.Asmbelfsetup();
            }

            ref sync.WaitGroup wg = ref heap(out ptr<sync.WaitGroup> _addr_wg);
            var sect = ld.Segtext.Sections[0L];
            var offset = sect.Vaddr - ld.Segtext.Vaddr + ld.Segtext.Fileoff;
            ld.WriteParallel(_addr_wg, ld.Codeblk, ctxt, offset, sect.Vaddr, sect.Length);

            {
                var sect__prev1 = sect;

                foreach (var (_, __sect) in ld.Segtext.Sections[1L..])
                {
                    sect = __sect;
                    offset = sect.Vaddr - ld.Segtext.Vaddr + ld.Segtext.Fileoff;
                    ld.WriteParallel(_addr_wg, ld.Datblk, ctxt, offset, sect.Vaddr, sect.Length);
                }

                sect = sect__prev1;
            }

            if (ld.Segrodata.Filelen > 0L)
            {
                ld.WriteParallel(_addr_wg, ld.Datblk, ctxt, ld.Segrodata.Fileoff, ld.Segrodata.Vaddr, ld.Segrodata.Filelen);
            }

            if (ld.Segrelrodata.Filelen > 0L)
            {
                ld.WriteParallel(_addr_wg, ld.Datblk, ctxt, ld.Segrelrodata.Fileoff, ld.Segrelrodata.Vaddr, ld.Segrelrodata.Filelen);
            }

            ld.WriteParallel(_addr_wg, ld.Datblk, ctxt, ld.Segdata.Fileoff, ld.Segdata.Vaddr, ld.Segdata.Filelen);

            ld.WriteParallel(_addr_wg, ld.Dwarfblk, ctxt, ld.Segdwarf.Fileoff, ld.Segdwarf.Vaddr, ld.Segdwarf.Filelen);
            wg.Wait();

        }

        private static void asmb2(ptr<ld.Link> _addr_ctxt)
        {
            ref ld.Link ctxt = ref _addr_ctxt.val;

            var machlink = uint32(0L);
            if (ctxt.HeadType == objabi.Hdarwin)
            {
                machlink = uint32(ld.Domacholink(ctxt));
            } 

            /* output symbol table */
            ld.Symsize = 0L;

            ld.Lcsize = 0L;
            var symo = uint32(0L);
            if (!ld.FlagS.val)
            { 
                // TODO: rationalize

                if (ctxt.HeadType == objabi.Hplan9) 
                    symo = uint32(ld.Segdata.Fileoff + ld.Segdata.Filelen);
                else if (ctxt.HeadType == objabi.Hdarwin) 
                    symo = uint32(ld.Segdwarf.Fileoff + uint64(ld.Rnd(int64(ld.Segdwarf.Filelen), int64(ld.FlagRound.val))) + uint64(machlink));
                else 
                    if (ctxt.IsELF)
                    {
                        symo = uint32(ld.Segdwarf.Fileoff + ld.Segdwarf.Filelen);
                        symo = uint32(ld.Rnd(int64(symo), int64(ld.FlagRound.val)));
                    }

                                ctxt.Out.SeekSet(int64(symo));

                if (ctxt.HeadType == objabi.Hplan9) 
                    ld.Asmplan9sym(ctxt);

                    var sym = ctxt.Syms.Lookup("pclntab", 0L);
                    if (sym != null)
                    {
                        ld.Lcsize = int32(len(sym.P));
                        ctxt.Out.Write(sym.P);
                    }

                else if (ctxt.HeadType == objabi.Hdarwin) 
                    if (ctxt.LinkMode == ld.LinkExternal)
                    {
                        ld.Machoemitreloc(ctxt);
                    }

                else 
                    if (ctxt.IsELF)
                    {
                        ld.Asmelfsym(ctxt);
                        ctxt.Out.Write(ld.Elfstrdat);

                        if (ctxt.LinkMode == ld.LinkExternal)
                        {
                            ld.Elfemitreloc(ctxt);
                        }

                    }

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
            else if (ctxt.HeadType == objabi.Hlinux || ctxt.HeadType == objabi.Hfreebsd || ctxt.HeadType == objabi.Hnetbsd || ctxt.HeadType == objabi.Hopenbsd) 
                ld.Asmbelf(ctxt, int64(symo));
            else if (ctxt.HeadType == objabi.Hdarwin) 
                ld.Asmbmacho(ctxt);
            else                         if (ld.FlagC.val)
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
