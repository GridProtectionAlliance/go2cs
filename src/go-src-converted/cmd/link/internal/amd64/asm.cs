// Inferno utils/6l/asm.c
// https://bitbucket.org/inferno-os/inferno-os/src/master/utils/6l/asm.c
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

// package amd64 -- go2cs converted at 2022 March 06 23:19:51 UTC
// import "cmd/link/internal/amd64" ==> using amd64 = go.cmd.link.@internal.amd64_package
// Original source: C:\Program Files\Go\src\cmd\link\internal\amd64\asm.go
using objabi = go.cmd.@internal.objabi_package;
using sys = go.cmd.@internal.sys_package;
using ld = go.cmd.link.@internal.ld_package;
using loader = go.cmd.link.@internal.loader_package;
using sym = go.cmd.link.@internal.sym_package;
using elf = go.debug.elf_package;
using log = go.log_package;
using System;


namespace go.cmd.link.@internal;

public static partial class amd64_package {

public static uint PADDR(uint x) {
    return x & ~0x80000000;
}

private static void gentext(ptr<ld.Link> _addr_ctxt, ptr<loader.Loader> _addr_ldr) {
    ref ld.Link ctxt = ref _addr_ctxt.val;
    ref loader.Loader ldr = ref _addr_ldr.val;

    var (initfunc, addmoduledata) = ld.PrepareAddmoduledata(ctxt);
    if (initfunc == null) {
        return ;
    }
    Action<byte[]> o = op => {
        foreach (var (_, op1) in op) {
            initfunc.AddUint8(op1);
        }
    }; 

    // 0000000000000000 <local.dso_init>:
    //    0:    48 8d 3d 00 00 00 00     lea    0x0(%rip),%rdi        # 7 <local.dso_init+0x7>
    //             3: R_X86_64_PC32    runtime.firstmoduledata-0x4
    o(0x48, 0x8d, 0x3d);
    initfunc.AddPCRelPlus(ctxt.Arch, ctxt.Moduledata, 0); 
    //    7:    e8 00 00 00 00           callq  c <local.dso_init+0xc>
    //             8: R_X86_64_PLT32    runtime.addmoduledata-0x4
    o(0xe8);
    initfunc.AddSymRef(ctxt.Arch, addmoduledata, 0, objabi.R_CALL, 4); 
    //    c:    c3                       retq
    o(0xc3);

}

private static bool adddynrel(ptr<ld.Target> _addr_target, ptr<loader.Loader> _addr_ldr, ptr<ld.ArchSyms> _addr_syms, loader.Sym s, loader.Reloc r, nint rIdx) {
    ref ld.Target target = ref _addr_target.val;
    ref loader.Loader ldr = ref _addr_ldr.val;
    ref ld.ArchSyms syms = ref _addr_syms.val;

    var targ = r.Sym();
    sym.SymKind targType = default;
    if (targ != 0) {
        targType = ldr.SymType(targ);
    }
    {
        var rt = r.Type();


        if (rt == objabi.ElfRelocOffset + objabi.RelocType(elf.R_X86_64_PC32))
        {
            if (targType == sym.SDYNIMPORT) {
                ldr.Errorf(s, "unexpected R_X86_64_PC32 relocation for dynamic symbol %s", ldr.SymName(targ));
            } 
            // TODO(mwhudson): the test of VisibilityHidden here probably doesn't make
            // sense and should be removed when someone has thought about it properly.
            if ((targType == 0 || targType == sym.SXREF) && !ldr.AttrVisibilityHidden(targ)) {
                ldr.Errorf(s, "unknown symbol %s in pcrel", ldr.SymName(targ));
            }

            var su = ldr.MakeSymbolUpdater(s);
            su.SetRelocType(rIdx, objabi.R_PCREL);
            su.SetRelocAdd(rIdx, r.Add() + 4);
            return true;
            goto __switch_break0;
        }
        if (rt == objabi.ElfRelocOffset + objabi.RelocType(elf.R_X86_64_PC64))
        {
            if (targType == sym.SDYNIMPORT) {
                ldr.Errorf(s, "unexpected R_X86_64_PC64 relocation for dynamic symbol %s", ldr.SymName(targ));
            }
            if (targType == 0 || targType == sym.SXREF) {
                ldr.Errorf(s, "unknown symbol %s in pcrel", ldr.SymName(targ));
            }
            su = ldr.MakeSymbolUpdater(s);
            su.SetRelocType(rIdx, objabi.R_PCREL);
            su.SetRelocAdd(rIdx, r.Add() + 8);
            return true;
            goto __switch_break0;
        }
        if (rt == objabi.ElfRelocOffset + objabi.RelocType(elf.R_X86_64_PLT32))
        {
            su = ldr.MakeSymbolUpdater(s);
            su.SetRelocType(rIdx, objabi.R_PCREL);
            su.SetRelocAdd(rIdx, r.Add() + 4);
            if (targType == sym.SDYNIMPORT) {
                addpltsym(_addr_target, _addr_ldr, _addr_syms, targ);
                su.SetRelocSym(rIdx, syms.PLT);
                su.SetRelocAdd(rIdx, r.Add() + int64(ldr.SymPlt(targ)));
            }
            return true;
            goto __switch_break0;
        }
        if (rt == objabi.ElfRelocOffset + objabi.RelocType(elf.R_X86_64_GOTPCREL) || rt == objabi.ElfRelocOffset + objabi.RelocType(elf.R_X86_64_GOTPCRELX) || rt == objabi.ElfRelocOffset + objabi.RelocType(elf.R_X86_64_REX_GOTPCRELX))
        {
            su = ldr.MakeSymbolUpdater(s);
            if (targType != sym.SDYNIMPORT) { 
                // have symbol
                var sData = ldr.Data(s);
                if (r.Off() >= 2 && sData[r.Off() - 2] == 0x8b) {
                    su.MakeWritable(); 
                    // turn MOVQ of GOT entry into LEAQ of symbol itself
                    var writeableData = su.Data();
                    writeableData[r.Off() - 2] = 0x8d;
                    su.SetRelocType(rIdx, objabi.R_PCREL);
                    su.SetRelocAdd(rIdx, r.Add() + 4);
                    return true;

                }

            } 

            // fall back to using GOT and hope for the best (CMOV*)
            // TODO: just needs relocation, no need to put in .dynsym
            ld.AddGotSym(target, ldr, syms, targ, uint32(elf.R_X86_64_GLOB_DAT));

            su.SetRelocType(rIdx, objabi.R_PCREL);
            su.SetRelocSym(rIdx, syms.GOT);
            su.SetRelocAdd(rIdx, r.Add() + 4 + int64(ldr.SymGot(targ)));
            return true;
            goto __switch_break0;
        }
        if (rt == objabi.ElfRelocOffset + objabi.RelocType(elf.R_X86_64_64))
        {
            if (targType == sym.SDYNIMPORT) {
                ldr.Errorf(s, "unexpected R_X86_64_64 relocation for dynamic symbol %s", ldr.SymName(targ));
            }
            su = ldr.MakeSymbolUpdater(s);
            su.SetRelocType(rIdx, objabi.R_ADDR);
            if (target.IsPIE() && target.IsInternal()) { 
                // For internal linking PIE, this R_ADDR relocation cannot
                // be resolved statically. We need to generate a dynamic
                // relocation. Let the code below handle it.
                break;

            }

            return true; 

            // Handle relocations found in Mach-O object files.
            goto __switch_break0;
        }
        if (rt == objabi.MachoRelocOffset + ld.MACHO_X86_64_RELOC_UNSIGNED * 2 + 0 || rt == objabi.MachoRelocOffset + ld.MACHO_X86_64_RELOC_SIGNED * 2 + 0 || rt == objabi.MachoRelocOffset + ld.MACHO_X86_64_RELOC_BRANCH * 2 + 0)
        {
            su = ldr.MakeSymbolUpdater(s);
            su.SetRelocType(rIdx, objabi.R_ADDR);

            if (targType == sym.SDYNIMPORT) {
                ldr.Errorf(s, "unexpected reloc for dynamic symbol %s", ldr.SymName(targ));
            }
            if (target.IsPIE() && target.IsInternal()) { 
                // For internal linking PIE, this R_ADDR relocation cannot
                // be resolved statically. We need to generate a dynamic
                // relocation. Let the code below handle it.
                if (rt == objabi.MachoRelocOffset + ld.MACHO_X86_64_RELOC_UNSIGNED * 2) {
                    break;
                }
                else
 { 
                    // MACHO_X86_64_RELOC_SIGNED or MACHO_X86_64_RELOC_BRANCH
                    // Can this happen? The object is expected to be PIC.
                    ldr.Errorf(s, "unsupported relocation for PIE: %v", rt);

                }

            }

            return true;
            goto __switch_break0;
        }
        if (rt == objabi.MachoRelocOffset + ld.MACHO_X86_64_RELOC_BRANCH * 2 + 1)
        {
            if (targType == sym.SDYNIMPORT) {
                addpltsym(_addr_target, _addr_ldr, _addr_syms, targ);
                su = ldr.MakeSymbolUpdater(s);
                su.SetRelocSym(rIdx, syms.PLT);
                su.SetRelocType(rIdx, objabi.R_PCREL);
                su.SetRelocAdd(rIdx, int64(ldr.SymPlt(targ)));
                return true;
            }
            fallthrough = true;

        }
        if (fallthrough || rt == objabi.MachoRelocOffset + ld.MACHO_X86_64_RELOC_UNSIGNED * 2 + 1 || rt == objabi.MachoRelocOffset + ld.MACHO_X86_64_RELOC_SIGNED * 2 + 1 || rt == objabi.MachoRelocOffset + ld.MACHO_X86_64_RELOC_SIGNED_1 * 2 + 1 || rt == objabi.MachoRelocOffset + ld.MACHO_X86_64_RELOC_SIGNED_2 * 2 + 1 || rt == objabi.MachoRelocOffset + ld.MACHO_X86_64_RELOC_SIGNED_4 * 2 + 1)
        {
            su = ldr.MakeSymbolUpdater(s);
            su.SetRelocType(rIdx, objabi.R_PCREL);

            if (targType == sym.SDYNIMPORT) {
                ldr.Errorf(s, "unexpected pc-relative reloc for dynamic symbol %s", ldr.SymName(targ));
            }
            return true;
            goto __switch_break0;
        }
        if (rt == objabi.MachoRelocOffset + ld.MACHO_X86_64_RELOC_GOT_LOAD * 2 + 1)
        {
            if (targType != sym.SDYNIMPORT) { 
                // have symbol
                // turn MOVQ of GOT entry into LEAQ of symbol itself
                var sdata = ldr.Data(s);
                if (r.Off() < 2 || sdata[r.Off() - 2] != 0x8b) {
                    ldr.Errorf(s, "unexpected GOT_LOAD reloc for non-dynamic symbol %s", ldr.SymName(targ));
                    return false;
                }

                su = ldr.MakeSymbolUpdater(s);
                su.MakeWritable();
                sdata = su.Data();
                sdata[r.Off() - 2] = 0x8d;
                su.SetRelocType(rIdx, objabi.R_PCREL);
                return true;

            }

            fallthrough = true;

        }
        if (fallthrough || rt == objabi.MachoRelocOffset + ld.MACHO_X86_64_RELOC_GOT * 2 + 1)
        {
            if (targType != sym.SDYNIMPORT) {
                ldr.Errorf(s, "unexpected GOT reloc for non-dynamic symbol %s", ldr.SymName(targ));
            }
            ld.AddGotSym(target, ldr, syms, targ, 0);
            su = ldr.MakeSymbolUpdater(s);
            su.SetRelocType(rIdx, objabi.R_PCREL);
            su.SetRelocSym(rIdx, syms.GOT);
            su.SetRelocAdd(rIdx, r.Add() + int64(ldr.SymGot(targ)));
            return true;
            goto __switch_break0;
        }
        // default: 
            if (rt >= objabi.ElfRelocOffset) {
                ldr.Errorf(s, "unexpected relocation type %d (%s)", r.Type(), sym.RelocName(target.Arch, r.Type()));
                return false;
            } 

            // Handle relocations found in ELF object files.

        __switch_break0:;
    } 

    // Reread the reloc to incorporate any changes in type above.
    var relocs = ldr.Relocs(s);
    r = relocs.At(rIdx);


    if (r.Type() == objabi.R_CALL || r.Type() == objabi.R_PCREL) 
        if (targType != sym.SDYNIMPORT) { 
            // nothing to do, the relocation will be laid out in reloc
            return true;

        }
        if (target.IsExternal()) { 
            // External linker will do this relocation.
            return true;

        }
        addpltsym(_addr_target, _addr_ldr, _addr_syms, targ);
        su = ldr.MakeSymbolUpdater(s);
        su.SetRelocSym(rIdx, syms.PLT);
        su.SetRelocAdd(rIdx, int64(ldr.SymPlt(targ)));
        return true;
    else if (r.Type() == objabi.R_ADDR) 
        if (ldr.SymType(s) == sym.STEXT && target.IsElf()) {
            su = ldr.MakeSymbolUpdater(s);
            if (target.IsSolaris()) {
                addpltsym(_addr_target, _addr_ldr, _addr_syms, targ);
                su.SetRelocSym(rIdx, syms.PLT);
                su.SetRelocAdd(rIdx, r.Add() + int64(ldr.SymPlt(targ)));
                return true;
            } 
            // The code is asking for the address of an external
            // function. We provide it with the address of the
            // correspondent GOT symbol.
            ld.AddGotSym(target, ldr, syms, targ, uint32(elf.R_X86_64_GLOB_DAT));

            su.SetRelocSym(rIdx, syms.GOT);
            su.SetRelocAdd(rIdx, r.Add() + int64(ldr.SymGot(targ)));
            return true;

        }
        if (target.IsPIE() && target.IsInternal()) { 
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
            switch (ldr.SymName(s)) {
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
            if (ldr.SymType(s) != sym.SDATA && ldr.SymType(s) != sym.SRODATA) {
                break;
            }

        }
        if (target.IsElf()) { 
            // Generate R_X86_64_RELATIVE relocations for best
            // efficiency in the dynamic linker.
            //
            // As noted above, symbol addresses have not been
            // assigned yet, so we can't generate the final reloc
            // entry yet. We ultimately want:
            //
            // r_offset = s + r.Off
            // r_info = R_X86_64_RELATIVE
            // r_addend = targ + r.Add
            //
            // The dynamic linker will set *offset = base address +
            // addend.
            //
            // AddAddrPlus is used for r_offset and r_addend to
            // generate new R_ADDR relocations that will update
            // these fields in the 'reloc' phase.
            var rela = ldr.MakeSymbolUpdater(syms.Rela);
            rela.AddAddrPlus(target.Arch, s, int64(r.Off()));
            if (r.Siz() == 8) {
                rela.AddUint64(target.Arch, elf.R_INFO(0, uint32(elf.R_X86_64_RELATIVE)));
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
        if (target.IsDarwin()) { 
            // Mach-O relocations are a royal pain to lay out.
            // They use a compact stateful bytecode representation.
            // Here we record what are needed and encode them later.
            ld.MachoAddRebase(s, int64(r.Off())); 
            // Not mark r done here. So we still apply it statically,
            // so in the file content we'll also have the right offset
            // to the relocation target. So it can be examined statically
            // (e.g. go version).
            return true;

        }
        return false;

}

private static bool elfreloc1(ptr<ld.Link> _addr_ctxt, ptr<ld.OutBuf> _addr_@out, ptr<loader.Loader> _addr_ldr, loader.Sym s, loader.ExtReloc r, nint ri, long sectoff) {
    ref ld.Link ctxt = ref _addr_ctxt.val;
    ref ld.OutBuf @out = ref _addr_@out.val;
    ref loader.Loader ldr = ref _addr_ldr.val;

    @out.Write64(uint64(sectoff));

    var elfsym = ld.ElfSymForReloc(ctxt, r.Xsym);
    var siz = r.Size;

    if (r.Type == objabi.R_ADDR || r.Type == objabi.R_DWARFSECREF) 
        if (siz == 4) {
            @out.Write64(uint64(elf.R_X86_64_32) | uint64(elfsym) << 32);
        }
        else if (siz == 8) {
            @out.Write64(uint64(elf.R_X86_64_64) | uint64(elfsym) << 32);
        }
        else
 {
            return false;
        }
    else if (r.Type == objabi.R_TLS_LE) 
        if (siz == 4) {
            @out.Write64(uint64(elf.R_X86_64_TPOFF32) | uint64(elfsym) << 32);
        }
        else
 {
            return false;
        }
    else if (r.Type == objabi.R_TLS_IE) 
        if (siz == 4) {
            @out.Write64(uint64(elf.R_X86_64_GOTTPOFF) | uint64(elfsym) << 32);
        }
        else
 {
            return false;
        }
    else if (r.Type == objabi.R_CALL) 
        if (siz == 4) {
            if (ldr.SymType(r.Xsym) == sym.SDYNIMPORT) {
                @out.Write64(uint64(elf.R_X86_64_PLT32) | uint64(elfsym) << 32);
            }
            else
 {
                @out.Write64(uint64(elf.R_X86_64_PC32) | uint64(elfsym) << 32);
            }

        }
        else
 {
            return false;
        }
    else if (r.Type == objabi.R_PCREL) 
        if (siz == 4) {
            if (ldr.SymType(r.Xsym) == sym.SDYNIMPORT && ldr.SymElfType(r.Xsym) == elf.STT_FUNC) {
                @out.Write64(uint64(elf.R_X86_64_PLT32) | uint64(elfsym) << 32);
            }
            else
 {
                @out.Write64(uint64(elf.R_X86_64_PC32) | uint64(elfsym) << 32);
            }

        }
        else
 {
            return false;
        }
    else if (r.Type == objabi.R_GOTPCREL) 
        if (siz == 4) {
            @out.Write64(uint64(elf.R_X86_64_GOTPCREL) | uint64(elfsym) << 32);
        }
        else
 {
            return false;
        }
    else 
        return false;
        @out.Write64(uint64(r.Xadd));
    return true;

}

private static bool machoreloc1(ptr<sys.Arch> _addr_arch, ptr<ld.OutBuf> _addr_@out, ptr<loader.Loader> _addr_ldr, loader.Sym s, loader.ExtReloc r, long sectoff) {
    ref sys.Arch arch = ref _addr_arch.val;
    ref ld.OutBuf @out = ref _addr_@out.val;
    ref loader.Loader ldr = ref _addr_ldr.val;

    uint v = default;

    var rs = r.Xsym;
    var rt = r.Type;

    if (ldr.SymType(rs) == sym.SHOSTOBJ || rt == objabi.R_PCREL || rt == objabi.R_GOTPCREL || rt == objabi.R_CALL) {
        if (ldr.SymDynid(rs) < 0) {
            ldr.Errorf(s, "reloc %d (%s) to non-macho symbol %s type=%d (%s)", rt, sym.RelocName(arch, rt), ldr.SymName(rs), ldr.SymType(rs), ldr.SymType(rs));
            return false;
        }
        v = uint32(ldr.SymDynid(rs));
        v |= 1 << 27; // external relocation
    }
    else
 {
        v = uint32(ldr.SymSect(rs).Extnum);
        if (v == 0) {
            ldr.Errorf(s, "reloc %d (%s) to symbol %s in non-macho section %s type=%d (%s)", rt, sym.RelocName(arch, rt), ldr.SymName(rs), ldr.SymSect(rs).Name, ldr.SymType(rs), ldr.SymType(rs));
            return false;
        }
    }

    if (rt == objabi.R_ADDR) 
        v |= ld.MACHO_X86_64_RELOC_UNSIGNED << 28;
    else if (rt == objabi.R_CALL) 
        v |= 1 << 24; // pc-relative bit
        v |= ld.MACHO_X86_64_RELOC_BRANCH << 28; 

        // NOTE: Only works with 'external' relocation. Forced above.
    else if (rt == objabi.R_PCREL) 
        v |= 1 << 24; // pc-relative bit
        v |= ld.MACHO_X86_64_RELOC_SIGNED << 28;
    else if (rt == objabi.R_GOTPCREL) 
        v |= 1 << 24; // pc-relative bit
        v |= ld.MACHO_X86_64_RELOC_GOT_LOAD << 28;
    else 
        return false;
        switch (r.Size) {
        case 1: 
            v |= 0 << 25;
            break;
        case 2: 
            v |= 1 << 25;
            break;
        case 4: 
            v |= 2 << 25;
            break;
        case 8: 
            v |= 3 << 25;
            break;
        default: 
            return false;
            break;
    }

    @out.Write32(uint32(sectoff));
    @out.Write32(v);
    return true;

}

private static bool pereloc1(ptr<sys.Arch> _addr_arch, ptr<ld.OutBuf> _addr_@out, ptr<loader.Loader> _addr_ldr, loader.Sym s, loader.ExtReloc r, long sectoff) {
    ref sys.Arch arch = ref _addr_arch.val;
    ref ld.OutBuf @out = ref _addr_@out.val;
    ref loader.Loader ldr = ref _addr_ldr.val;

    uint v = default;

    var rs = r.Xsym;
    var rt = r.Type;

    if (ldr.SymDynid(rs) < 0) {
        ldr.Errorf(s, "reloc %d (%s) to non-coff symbol %s type=%d (%s)", rt, sym.RelocName(arch, rt), ldr.SymName(rs), ldr.SymType(rs), ldr.SymType(rs));
        return false;
    }
    @out.Write32(uint32(sectoff));
    @out.Write32(uint32(ldr.SymDynid(rs)));


    if (rt == objabi.R_DWARFSECREF) 
        v = ld.IMAGE_REL_AMD64_SECREL;
    else if (rt == objabi.R_ADDR) 
        if (r.Size == 8) {
            v = ld.IMAGE_REL_AMD64_ADDR64;
        }
        else
 {
            v = ld.IMAGE_REL_AMD64_ADDR32;
        }
    else if (rt == objabi.R_CALL || rt == objabi.R_PCREL) 
        v = ld.IMAGE_REL_AMD64_REL32;
    else 
        return false;
        @out.Write16(uint16(v));

    return true;

}

private static (long, nint, bool) archreloc(ptr<ld.Target> _addr__p0, ptr<loader.Loader> _addr__p0, ptr<ld.ArchSyms> _addr__p0, loader.Reloc _p0, loader.Sym _p0, long _p0) {
    long _p0 = default;
    nint _p0 = default;
    bool _p0 = default;
    ref ld.Target _p0 = ref _addr__p0.val;
    ref loader.Loader _p0 = ref _addr__p0.val;
    ref ld.ArchSyms _p0 = ref _addr__p0.val;

    return (-1, 0, false);
}

private static long archrelocvariant(ptr<ld.Target> _addr__p0, ptr<loader.Loader> _addr__p0, loader.Reloc _p0, sym.RelocVariant _p0, loader.Sym _p0, long _p0, slice<byte> _p0) {
    ref ld.Target _p0 = ref _addr__p0.val;
    ref loader.Loader _p0 = ref _addr__p0.val;

    log.Fatalf("unexpected relocation variant");
    return -1;
}

private static void elfsetupplt(ptr<ld.Link> _addr_ctxt, ptr<loader.SymbolBuilder> _addr_plt, ptr<loader.SymbolBuilder> _addr_got, loader.Sym dynamic) {
    ref ld.Link ctxt = ref _addr_ctxt.val;
    ref loader.SymbolBuilder plt = ref _addr_plt.val;
    ref loader.SymbolBuilder got = ref _addr_got.val;

    if (plt.Size() == 0) { 
        // pushq got+8(IP)
        plt.AddUint8(0xff);

        plt.AddUint8(0x35);
        plt.AddPCRelPlus(ctxt.Arch, got.Sym(), 8); 

        // jmpq got+16(IP)
        plt.AddUint8(0xff);

        plt.AddUint8(0x25);
        plt.AddPCRelPlus(ctxt.Arch, got.Sym(), 16); 

        // nopl 0(AX)
        plt.AddUint32(ctxt.Arch, 0x00401f0f); 

        // assume got->size == 0 too
        got.AddAddrPlus(ctxt.Arch, dynamic, 0);

        got.AddUint64(ctxt.Arch, 0);
        got.AddUint64(ctxt.Arch, 0);

    }
}

private static void addpltsym(ptr<ld.Target> _addr_target, ptr<loader.Loader> _addr_ldr, ptr<ld.ArchSyms> _addr_syms, loader.Sym s) => func((_, panic, _) => {
    ref ld.Target target = ref _addr_target.val;
    ref loader.Loader ldr = ref _addr_ldr.val;
    ref ld.ArchSyms syms = ref _addr_syms.val;

    if (ldr.SymPlt(s) >= 0) {
        return ;
    }
    ld.Adddynsym(ldr, target, syms, s);

    if (target.IsElf()) {
        var plt = ldr.MakeSymbolUpdater(syms.PLT);
        var got = ldr.MakeSymbolUpdater(syms.GOTPLT);
        var rela = ldr.MakeSymbolUpdater(syms.RelaPLT);
        if (plt.Size() == 0) {
            panic("plt is not set up");
        }
        plt.AddUint8(0xff);

        plt.AddUint8(0x25);
        plt.AddPCRelPlus(target.Arch, got.Sym(), got.Size()); 

        // add to got: pointer to current pos in plt
        got.AddAddrPlus(target.Arch, plt.Sym(), plt.Size()); 

        // pushq $x
        plt.AddUint8(0x68);

        plt.AddUint32(target.Arch, uint32((got.Size() - 24 - 8) / 8)); 

        // jmpq .plt
        plt.AddUint8(0xe9);

        plt.AddUint32(target.Arch, uint32(-(plt.Size() + 4))); 

        // rela
        rela.AddAddrPlus(target.Arch, got.Sym(), got.Size() - 8);

        var sDynid = ldr.SymDynid(s);
        rela.AddUint64(target.Arch, elf.R_INFO(uint32(sDynid), uint32(elf.R_X86_64_JMP_SLOT)));
        rela.AddUint64(target.Arch, 0);

        ldr.SetPlt(s, int32(plt.Size() - 16));

    }
    else if (target.IsDarwin()) {
        ld.AddGotSym(target, ldr, syms, s, 0);

        sDynid = ldr.SymDynid(s);
        var lep = ldr.MakeSymbolUpdater(syms.LinkEditPLT);
        lep.AddUint32(target.Arch, uint32(sDynid));

        plt = ldr.MakeSymbolUpdater(syms.PLT);
        ldr.SetPlt(s, int32(plt.Size())); 

        // jmpq *got+size(IP)
        plt.AddUint8(0xff);
        plt.AddUint8(0x25);
        plt.AddPCRelPlus(target.Arch, syms.GOT, int64(ldr.SymGot(s)));

    }
    else
 {
        ldr.Errorf(s, "addpltsym: unsupported binary format");
    }
});

private static void tlsIEtoLE(slice<byte> P, nint off, nint size) { 
    // Transform the PC-relative instruction into a constant load.
    // That is,
    //
    //    MOVQ X(IP), REG  ->  MOVQ $Y, REG
    //
    // To determine the instruction and register, we study the op codes.
    // Consult an AMD64 instruction encoding guide to decipher this.
    if (off < 3) {
        log.Fatal("R_X86_64_GOTTPOFF reloc not preceded by MOVQ or ADDQ instruction");
    }
    var op = P[(int)off - 3..(int)off];
    var reg = op[2] >> 3;

    if (op[1] == 0x8b || reg == 4) { 
        // MOVQ
        if (op[0] == 0x4c) {
            op[0] = 0x49;
        }
        else if (size == 4 && op[0] == 0x44) {
            op[0] = 0x41;
        }
        if (op[1] == 0x8b) {
            op[1] = 0xc7;
        }
        else
 {
            op[1] = 0x81; // special case for SP
        }
        op[2] = 0xc0 | reg;

    }
    else
 { 
        // An alternate op is ADDQ. This is handled by GNU gold,
        // but right now is not generated by the Go compiler:
        //    ADDQ X(IP), REG  ->  ADDQ $Y, REG
        // Consider adding support for it here.
        log.Fatalf("expected TLS IE op to be MOVQ, got %v", op);

    }
}

} // end amd64_package
