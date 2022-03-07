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

// package arm -- go2cs converted at 2022 March 06 23:19:57 UTC
// import "cmd/link/internal/arm" ==> using arm = go.cmd.link.@internal.arm_package
// Original source: C:\Program Files\Go\src\cmd\link\internal\arm\asm.go
using objabi = go.cmd.@internal.objabi_package;
using sys = go.cmd.@internal.sys_package;
using ld = go.cmd.link.@internal.ld_package;
using loader = go.cmd.link.@internal.loader_package;
using sym = go.cmd.link.@internal.sym_package;
using elf = go.debug.elf_package;
using fmt = go.fmt_package;
using log = go.log_package;
using System;


namespace go.cmd.link.@internal;

public static partial class arm_package {

    // This assembler:
    //
    //         .align 2
    // local.dso_init:
    //         ldr r0, .Lmoduledata
    // .Lloadfrom:
    //         ldr r0, [r0]
    //         b runtime.addmoduledata@plt
    // .align 2
    // .Lmoduledata:
    //         .word local.moduledata(GOT_PREL) + (. - (.Lloadfrom + 4))
    // assembles to:
    //
    // 00000000 <local.dso_init>:
    //    0:        e59f0004        ldr     r0, [pc, #4]    ; c <local.dso_init+0xc>
    //    4:        e5900000        ldr     r0, [r0]
    //    8:        eafffffe        b       0 <runtime.addmoduledata>
    //                      8: R_ARM_JUMP24 runtime.addmoduledata
    //    c:        00000004        .word   0x00000004
    //                      c: R_ARM_GOT_PREL       local.moduledata
private static void gentext(ptr<ld.Link> _addr_ctxt, ptr<loader.Loader> _addr_ldr) {
    ref ld.Link ctxt = ref _addr_ctxt.val;
    ref loader.Loader ldr = ref _addr_ldr.val;

    var (initfunc, addmoduledata) = ld.PrepareAddmoduledata(ctxt);
    if (initfunc == null) {
        return ;
    }
    Action<uint> o = op => {
        initfunc.AddUint32(ctxt.Arch, op);
    };
    o(0xe59f0004);
    o(0xe08f0000);

    o(0xeafffffe);
    var (rel, _) = initfunc.AddRel(objabi.R_CALLARM);
    rel.SetOff(8);
    rel.SetSiz(4);
    rel.SetSym(addmoduledata);
    rel.SetAdd(0xeafffffe); // vomit

    o(0x00000000);

    var (rel2, _) = initfunc.AddRel(objabi.R_PCREL);
    rel2.SetOff(12);
    rel2.SetSiz(4);
    rel2.SetSym(ctxt.Moduledata);
    rel2.SetAdd(4);

}

// Preserve highest 8 bits of a, and do addition to lower 24-bit
// of a and b; used to adjust ARM branch instruction's target
private static int braddoff(int a, int b) {
    return int32((uint32(a)) & 0xff000000 | 0x00ffffff & uint32(a + b));
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

    if (r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_ARM_PLT32)) 
        var su = ldr.MakeSymbolUpdater(s);
        su.SetRelocType(rIdx, objabi.R_CALLARM);

        if (targType == sym.SDYNIMPORT) {
            addpltsym(_addr_target, _addr_ldr, _addr_syms, targ);
            su.SetRelocSym(rIdx, syms.PLT);
            su.SetRelocAdd(rIdx, int64(braddoff(int32(r.Add()), ldr.SymPlt(targ) / 4)));
        }
        return true;
    else if (r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_ARM_THM_PC22)) // R_ARM_THM_CALL
        ld.Exitf("R_ARM_THM_CALL, are you using -marm?");
        return false;
    else if (r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_ARM_GOT32)) // R_ARM_GOT_BREL
        if (targType != sym.SDYNIMPORT) {
            addgotsyminternal(_addr_target, _addr_ldr, _addr_syms, targ);
        }
        else
 {
            ld.AddGotSym(target, ldr, syms, targ, uint32(elf.R_ARM_GLOB_DAT));
        }
        su = ldr.MakeSymbolUpdater(s);
        su.SetRelocType(rIdx, objabi.R_CONST); // write r->add during relocsym
        su.SetRelocSym(rIdx, 0);
        su.SetRelocAdd(rIdx, r.Add() + int64(ldr.SymGot(targ)));
        return true;
    else if (r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_ARM_GOT_PREL)) // GOT(nil) + A - nil
        if (targType != sym.SDYNIMPORT) {
            addgotsyminternal(_addr_target, _addr_ldr, _addr_syms, targ);
        }
        else
 {
            ld.AddGotSym(target, ldr, syms, targ, uint32(elf.R_ARM_GLOB_DAT));
        }
        su = ldr.MakeSymbolUpdater(s);
        su.SetRelocType(rIdx, objabi.R_PCREL);
        su.SetRelocSym(rIdx, syms.GOT);
        su.SetRelocAdd(rIdx, r.Add() + 4 + int64(ldr.SymGot(targ)));
        return true;
    else if (r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_ARM_GOTOFF)) // R_ARM_GOTOFF32
        su = ldr.MakeSymbolUpdater(s);
        su.SetRelocType(rIdx, objabi.R_GOTOFF);
        return true;
    else if (r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_ARM_GOTPC)) // R_ARM_BASE_PREL
        su = ldr.MakeSymbolUpdater(s);
        su.SetRelocType(rIdx, objabi.R_PCREL);
        su.SetRelocSym(rIdx, syms.GOT);
        su.SetRelocAdd(rIdx, r.Add() + 4);
        return true;
    else if (r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_ARM_CALL)) 
        su = ldr.MakeSymbolUpdater(s);
        su.SetRelocType(rIdx, objabi.R_CALLARM);
        if (targType == sym.SDYNIMPORT) {
            addpltsym(_addr_target, _addr_ldr, _addr_syms, targ);
            su.SetRelocSym(rIdx, syms.PLT);
            su.SetRelocAdd(rIdx, int64(braddoff(int32(r.Add()), ldr.SymPlt(targ) / 4)));
        }
        return true;
    else if (r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_ARM_REL32)) // R_ARM_REL32
        su = ldr.MakeSymbolUpdater(s);
        su.SetRelocType(rIdx, objabi.R_PCREL);
        su.SetRelocAdd(rIdx, r.Add() + 4);
        return true;
    else if (r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_ARM_ABS32)) 
        if (targType == sym.SDYNIMPORT) {
            ldr.Errorf(s, "unexpected R_ARM_ABS32 relocation for dynamic symbol %s", ldr.SymName(targ));
        }
        su = ldr.MakeSymbolUpdater(s);
        su.SetRelocType(rIdx, objabi.R_ADDR);
        return true;
    else if (r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_ARM_PC24) || r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_ARM_JUMP24)) 
        su = ldr.MakeSymbolUpdater(s);
        su.SetRelocType(rIdx, objabi.R_CALLARM);
        if (targType == sym.SDYNIMPORT) {
            addpltsym(_addr_target, _addr_ldr, _addr_syms, targ);
            su.SetRelocSym(rIdx, syms.PLT);
            su.SetRelocAdd(rIdx, int64(braddoff(int32(r.Add()), ldr.SymPlt(targ) / 4)));
        }
        return true;
    else 
        if (r.Type() >= objabi.ElfRelocOffset) {
            ldr.Errorf(s, "unexpected relocation type %d (%s)", r.Type(), sym.RelocName(target.Arch, r.Type()));
            return false;
        }
    // Handle references to ELF symbols from our own object files.
    if (targType != sym.SDYNIMPORT) {
        return true;
    }
    var relocs = ldr.Relocs(s);
    r = relocs.At(rIdx);


    if (r.Type() == objabi.R_CALLARM) 
        if (target.IsExternal()) { 
            // External linker will do this relocation.
            return true;

        }
        addpltsym(_addr_target, _addr_ldr, _addr_syms, targ);
        su = ldr.MakeSymbolUpdater(s);
        su.SetRelocSym(rIdx, syms.PLT);
        su.SetRelocAdd(rIdx, int64(braddoff(int32(r.Add()), ldr.SymPlt(targ) / 4))); // TODO: don't use r.Add for instruction bytes (issue 19811)
        return true;
    else if (r.Type() == objabi.R_ADDR) 
        if (ldr.SymType(s) != sym.SDATA) {
            break;
        }
        if (target.IsElf()) {
            ld.Adddynsym(ldr, target, syms, targ);
            var rel = ldr.MakeSymbolUpdater(syms.Rel);
            rel.AddAddrPlus(target.Arch, s, int64(r.Off()));
            rel.AddUint32(target.Arch, elf.R_INFO32(uint32(ldr.SymDynid(targ)), uint32(elf.R_ARM_GLOB_DAT))); // we need a nil + A dynamic reloc
            su = ldr.MakeSymbolUpdater(s);
            su.SetRelocType(rIdx, objabi.R_CONST); // write r->add during relocsym
            su.SetRelocSym(rIdx, 0);
            return true;

        }
    else if (r.Type() == objabi.R_GOTPCREL) 
        if (target.IsExternal()) { 
            // External linker will do this relocation.
            return true;

        }
        if (targType != sym.SDYNIMPORT) {
            ldr.Errorf(s, "R_GOTPCREL target is not SDYNIMPORT symbol: %v", ldr.SymName(targ));
        }
        ld.AddGotSym(target, ldr, syms, targ, uint32(elf.R_ARM_GLOB_DAT));
        su = ldr.MakeSymbolUpdater(s);
        su.SetRelocType(rIdx, objabi.R_PCREL);
        su.SetRelocSym(rIdx, syms.GOT);
        su.SetRelocAdd(rIdx, r.Add() + int64(ldr.SymGot(targ)));
        return true;
        return false;

}

private static bool elfreloc1(ptr<ld.Link> _addr_ctxt, ptr<ld.OutBuf> _addr_@out, ptr<loader.Loader> _addr_ldr, loader.Sym s, loader.ExtReloc r, nint ri, long sectoff) {
    ref ld.Link ctxt = ref _addr_ctxt.val;
    ref ld.OutBuf @out = ref _addr_@out.val;
    ref loader.Loader ldr = ref _addr_ldr.val;

    @out.Write32(uint32(sectoff));

    var elfsym = ld.ElfSymForReloc(ctxt, r.Xsym);
    var siz = r.Size;

    if (r.Type == objabi.R_ADDR || r.Type == objabi.R_DWARFSECREF) 
        if (siz == 4) {
            @out.Write32(uint32(elf.R_ARM_ABS32) | uint32(elfsym) << 8);
        }
        else
 {
            return false;
        }
    else if (r.Type == objabi.R_PCREL) 
        if (siz == 4) {
            @out.Write32(uint32(elf.R_ARM_REL32) | uint32(elfsym) << 8);
        }
        else
 {
            return false;
        }
    else if (r.Type == objabi.R_CALLARM) 
        if (siz == 4) {
            var relocs = ldr.Relocs(s);
            var r = relocs.At(ri);
            if (r.Add() & 0xff000000 == 0xeb000000) { // BL // TODO: using r.Add here is bad (issue 19811)
                @out.Write32(uint32(elf.R_ARM_CALL) | uint32(elfsym) << 8);

            }
            else
 {
                @out.Write32(uint32(elf.R_ARM_JUMP24) | uint32(elfsym) << 8);
            }

        }
        else
 {
            return false;
        }
    else if (r.Type == objabi.R_TLS_LE) 
        @out.Write32(uint32(elf.R_ARM_TLS_LE32) | uint32(elfsym) << 8);
    else if (r.Type == objabi.R_TLS_IE) 
        @out.Write32(uint32(elf.R_ARM_TLS_IE32) | uint32(elfsym) << 8);
    else if (r.Type == objabi.R_GOTPCREL) 
        if (siz == 4) {
            @out.Write32(uint32(elf.R_ARM_GOT_PREL) | uint32(elfsym) << 8);
        }
        else
 {
            return false;
        }
    else 
        return false;
        return true;

}

private static void elfsetupplt(ptr<ld.Link> _addr_ctxt, ptr<loader.SymbolBuilder> _addr_plt, ptr<loader.SymbolBuilder> _addr_got, loader.Sym dynamic) {
    ref ld.Link ctxt = ref _addr_ctxt.val;
    ref loader.SymbolBuilder plt = ref _addr_plt.val;
    ref loader.SymbolBuilder got = ref _addr_got.val;

    if (plt.Size() == 0) { 
        // str lr, [sp, #-4]!
        plt.AddUint32(ctxt.Arch, 0xe52de004); 

        // ldr lr, [pc, #4]
        plt.AddUint32(ctxt.Arch, 0xe59fe004); 

        // add lr, pc, lr
        plt.AddUint32(ctxt.Arch, 0xe08fe00e); 

        // ldr pc, [lr, #8]!
        plt.AddUint32(ctxt.Arch, 0xe5bef008); 

        // .word &GLOBAL_OFFSET_TABLE[0] - .
        plt.AddPCRelPlus(ctxt.Arch, got.Sym(), 4); 

        // the first .plt entry requires 3 .plt.got entries
        got.AddUint32(ctxt.Arch, 0);

        got.AddUint32(ctxt.Arch, 0);
        got.AddUint32(ctxt.Arch, 0);

    }
}

private static bool machoreloc1(ptr<sys.Arch> _addr__p0, ptr<ld.OutBuf> _addr__p0, ptr<loader.Loader> _addr__p0, loader.Sym _p0, loader.ExtReloc _p0, long _p0) {
    ref sys.Arch _p0 = ref _addr__p0.val;
    ref ld.OutBuf _p0 = ref _addr__p0.val;
    ref loader.Loader _p0 = ref _addr__p0.val;

    return false;
}

private static bool pereloc1(ptr<sys.Arch> _addr_arch, ptr<ld.OutBuf> _addr_@out, ptr<loader.Loader> _addr_ldr, loader.Sym s, loader.ExtReloc r, long sectoff) {
    ref sys.Arch arch = ref _addr_arch.val;
    ref ld.OutBuf @out = ref _addr_@out.val;
    ref loader.Loader ldr = ref _addr_ldr.val;

    var rs = r.Xsym;
    var rt = r.Type;

    if (ldr.SymDynid(rs) < 0) {
        ldr.Errorf(s, "reloc %d (%s) to non-coff symbol %s type=%d (%s)", rt, sym.RelocName(arch, rt), ldr.SymName(rs), ldr.SymType(rs), ldr.SymType(rs));
        return false;
    }
    @out.Write32(uint32(sectoff));
    @out.Write32(uint32(ldr.SymDynid(rs)));

    uint v = default;

    if (rt == objabi.R_DWARFSECREF) 
        v = ld.IMAGE_REL_ARM_SECREL;
    else if (rt == objabi.R_ADDR) 
        v = ld.IMAGE_REL_ARM_ADDR32;
    else 
        // unsupported relocation type
        return false;
        @out.Write16(uint16(v));

    return true;

}

// sign extend a 24-bit integer
private static int signext24(long x) {
    return (int32(x) << 8) >> 8;
}

// encode an immediate in ARM's imm12 format. copied from ../../../internal/obj/arm/asm5.go
private static uint immrot(uint v) {
    for (nint i = 0; i < 16; i++) {
        if (v & ~0xff == 0) {
            return uint32(i << 8) | v | 1 << 25;
        }
        v = v << 2 | v >> 30;

    }
    return 0;

}

// Convert the direct jump relocation r to refer to a trampoline if the target is too far
private static void trampoline(ptr<ld.Link> _addr_ctxt, ptr<loader.Loader> _addr_ldr, nint ri, loader.Sym rs, loader.Sym s) {
    ref ld.Link ctxt = ref _addr_ctxt.val;
    ref loader.Loader ldr = ref _addr_ldr.val;

    var relocs = ldr.Relocs(s);
    var r = relocs.At(ri);

    if (r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_ARM_CALL) || r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_ARM_PC24) || r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_ARM_JUMP24)) 
    {
        // Host object relocations that will be turned into a PLT call.
        // The PLT may be too far. Insert a trampoline for them.
        fallthrough = true;
    }
    if (fallthrough || r.Type() == objabi.R_CALLARM)
    {
        long t = default; 
        // ldr.SymValue(rs) == 0 indicates a cross-package jump to a function that is not yet
        // laid out. Conservatively use a trampoline. This should be rare, as we lay out packages
        // in dependency order.
        if (ldr.SymValue(rs) != 0) { 
            // r.Add is the instruction
            // low 24-bit encodes the target address
            t = (ldr.SymValue(rs) + int64(signext24(r.Add() & 0xffffff) * 4) - (ldr.SymValue(s) + int64(r.Off()))) / 4;

        }
        if (t > 0x7fffff || t < -0x800000 || ldr.SymValue(rs) == 0 || (ld.FlagDebugTramp > 1 && ldr.SymPkg(s) != ldr.SymPkg(rs).val)) { 
            // direct call too far, need to insert trampoline.
            // look up existing trampolines first. if we found one within the range
            // of direct call, we can reuse it. otherwise create a new one.
            var offset = (signext24(r.Add() & 0xffffff) + 2) * 4;
            loader.Sym tramp = default;
            for (nint i = 0; >>MARKER:FOREXPRESSION_LEVEL_1<<; i++) {
                var oName = ldr.SymName(rs);
                var name = oName + fmt.Sprintf("%+d-tramp%d", offset, i);
                tramp = ldr.LookupOrCreateSym(name, int(ldr.SymVersion(rs)));
                ldr.SetAttrReachable(tramp, true);
                if (ldr.SymType(tramp) == sym.SDYNIMPORT) { 
                    // don't reuse trampoline defined in other module
                    continue;

                }

                if (oName == "runtime.deferreturn") {
                    ldr.SetIsDeferReturnTramp(tramp, true);
                }

                if (ldr.SymValue(tramp) == 0) { 
                    // either the trampoline does not exist -- we need to create one,
                    // or found one the address which is not assigned -- this will be
                    // laid down immediately after the current function. use this one.
                    break;

                }

                t = (ldr.SymValue(tramp) - 8 - (ldr.SymValue(s) + int64(r.Off()))) / 4;
                if (t >= -0x800000 && t < 0x7fffff) { 
                    // found an existing trampoline that is not too far
                    // we can just use it
                    break;

                }

            }

            if (ldr.SymType(tramp) == 0) { 
                // trampoline does not exist, create one
                var trampb = ldr.MakeSymbolUpdater(tramp);
                ctxt.AddTramp(trampb);
                if (ctxt.DynlinkingGo() || ldr.SymType(rs) == sym.SDYNIMPORT) {
                    if (immrot(uint32(offset)) == 0) {
                        ctxt.Errorf(s, "odd offset in dynlink direct call: %v+%d", ldr.SymName(rs), offset);
                    }
                    gentrampdyn(_addr_ctxt.Arch, _addr_trampb, rs, int64(offset));
                }
                else if (ctxt.BuildMode == ld.BuildModeCArchive || ctxt.BuildMode == ld.BuildModeCShared || ctxt.BuildMode == ld.BuildModePIE) {
                    gentramppic(_addr_ctxt.Arch, _addr_trampb, rs, int64(offset));
                }
                else
 {
                    gentramp(_addr_ctxt.Arch, ctxt.LinkMode, _addr_ldr, _addr_trampb, rs, int64(offset));
                }

            } 
            // modify reloc to point to tramp, which will be resolved later
            var sb = ldr.MakeSymbolUpdater(s);
            relocs = sb.Relocs();
            r = relocs.At(ri);
            r.SetSym(tramp);
            r.SetAdd(r.Add() & 0xff000000 | 0xfffffe); // clear the offset embedded in the instruction
        }
        goto __switch_break0;
    }
    // default: 
        ctxt.Errorf(s, "trampoline called with non-jump reloc: %d (%s)", r.Type(), sym.RelocName(ctxt.Arch, r.Type()));

    __switch_break0:;

}

// generate a trampoline to target+offset
private static void gentramp(ptr<sys.Arch> _addr_arch, ld.LinkMode linkmode, ptr<loader.Loader> _addr_ldr, ptr<loader.SymbolBuilder> _addr_tramp, loader.Sym target, long offset) {
    ref sys.Arch arch = ref _addr_arch.val;
    ref loader.Loader ldr = ref _addr_ldr.val;
    ref loader.SymbolBuilder tramp = ref _addr_tramp.val;

    tramp.SetSize(12); // 3 instructions
    var P = make_slice<byte>(tramp.Size());
    var t = ldr.SymValue(target) + offset;
    var o1 = uint32(0xe5900000 | 12 << 12 | 15 << 16); // MOVW (R15), R12 // R15 is actual pc + 8
    var o2 = uint32(0xe12fff10 | 12); // JMP  (R12)
    var o3 = uint32(t); // WORD $target
    arch.ByteOrder.PutUint32(P, o1);
    arch.ByteOrder.PutUint32(P[(int)4..], o2);
    arch.ByteOrder.PutUint32(P[(int)8..], o3);
    tramp.SetData(P);

    if (linkmode == ld.LinkExternal || ldr.SymValue(target) == 0) {
        var (r, _) = tramp.AddRel(objabi.R_ADDR);
        r.SetOff(8);
        r.SetSiz(4);
        r.SetSym(target);
        r.SetAdd(offset);
    }
}

// generate a trampoline to target+offset in position independent code
private static void gentramppic(ptr<sys.Arch> _addr_arch, ptr<loader.SymbolBuilder> _addr_tramp, loader.Sym target, long offset) {
    ref sys.Arch arch = ref _addr_arch.val;
    ref loader.SymbolBuilder tramp = ref _addr_tramp.val;

    tramp.SetSize(16); // 4 instructions
    var P = make_slice<byte>(tramp.Size());
    var o1 = uint32(0xe5900000 | 12 << 12 | 15 << 16 | 4); // MOVW 4(R15), R12 // R15 is actual pc + 8
    var o2 = uint32(0xe0800000 | 12 << 12 | 15 << 16 | 12); // ADD R15, R12, R12
    var o3 = uint32(0xe12fff10 | 12); // JMP  (R12)
    var o4 = uint32(0); // WORD $(target-pc) // filled in with relocation
    arch.ByteOrder.PutUint32(P, o1);
    arch.ByteOrder.PutUint32(P[(int)4..], o2);
    arch.ByteOrder.PutUint32(P[(int)8..], o3);
    arch.ByteOrder.PutUint32(P[(int)12..], o4);
    tramp.SetData(P);

    var (r, _) = tramp.AddRel(objabi.R_PCREL);
    r.SetOff(12);
    r.SetSiz(4);
    r.SetSym(target);
    r.SetAdd(offset + 4);

}

// generate a trampoline to target+offset in dynlink mode (using GOT)
private static void gentrampdyn(ptr<sys.Arch> _addr_arch, ptr<loader.SymbolBuilder> _addr_tramp, loader.Sym target, long offset) {
    ref sys.Arch arch = ref _addr_arch.val;
    ref loader.SymbolBuilder tramp = ref _addr_tramp.val;

    tramp.SetSize(20); // 5 instructions
    var o1 = uint32(0xe5900000 | 12 << 12 | 15 << 16 | 8); // MOVW 8(R15), R12 // R15 is actual pc + 8
    var o2 = uint32(0xe0800000 | 12 << 12 | 15 << 16 | 12); // ADD R15, R12, R12
    var o3 = uint32(0xe5900000 | 12 << 12 | 12 << 16); // MOVW (R12), R12
    var o4 = uint32(0xe12fff10 | 12); // JMP  (R12)
    var o5 = uint32(0); // WORD $target@GOT // filled in with relocation
    var o6 = uint32(0);
    if (offset != 0) { 
        // insert an instruction to add offset
        tramp.SetSize(24); // 6 instructions
        o6 = o5;
        o5 = o4;
        o4 = 0xe2800000 | 12 << 12 | 12 << 16 | immrot(uint32(offset)); // ADD $offset, R12, R12
        o1 = uint32(0xe5900000 | 12 << 12 | 15 << 16 | 12); // MOVW 12(R15), R12
    }
    var P = make_slice<byte>(tramp.Size());
    arch.ByteOrder.PutUint32(P, o1);
    arch.ByteOrder.PutUint32(P[(int)4..], o2);
    arch.ByteOrder.PutUint32(P[(int)8..], o3);
    arch.ByteOrder.PutUint32(P[(int)12..], o4);
    arch.ByteOrder.PutUint32(P[(int)16..], o5);
    if (offset != 0) {
        arch.ByteOrder.PutUint32(P[(int)20..], o6);
    }
    tramp.SetData(P);

    var (r, _) = tramp.AddRel(objabi.R_GOTPCREL);
    r.SetOff(16);
    r.SetSiz(4);
    r.SetSym(target);
    r.SetAdd(8);
    if (offset != 0) { 
        // increase reloc offset by 4 as we inserted an ADD instruction
        r.SetOff(20);
        r.SetAdd(12);

    }
}

private static (long, nint, bool) archreloc(ptr<ld.Target> _addr_target, ptr<loader.Loader> _addr_ldr, ptr<ld.ArchSyms> _addr_syms, loader.Reloc r, loader.Sym s, long val) {
    long o = default;
    nint nExtReloc = default;
    bool ok = default;
    ref ld.Target target = ref _addr_target.val;
    ref loader.Loader ldr = ref _addr_ldr.val;
    ref ld.ArchSyms syms = ref _addr_syms.val;

    var rs = r.Sym();
    rs = ldr.ResolveABIAlias(rs);
    if (target.IsExternal()) {

        if (r.Type() == objabi.R_CALLARM) 
            // set up addend for eventual relocation via outer symbol.
            var (_, off) = ld.FoldSubSymbolOffset(ldr, rs);
            var xadd = int64(signext24(r.Add() & 0xffffff)) * 4 + off;
            if (xadd / 4 > 0x7fffff || xadd / 4 < -0x800000) {
                ldr.Errorf(s, "direct call too far %d", xadd / 4);
            }
            return (int64(braddoff(int32(0xff000000 & uint32(r.Add())), int32(0xffffff & uint32(xadd / 4)))), 1, true);
                return (-1, 0, false);

    }
    const var isOk = true;

    const nint noExtReloc = 0;


    // The following three arch specific relocations are only for generation of
    // Linux/ARM ELF's PLT entry (3 assembler instruction)
    if (r.Type() == objabi.R_PLT0) // add ip, pc, #0xXX00000
        if (ldr.SymValue(syms.GOTPLT) < ldr.SymValue(syms.PLT)) {
            ldr.Errorf(s, ".got.plt should be placed after .plt section.");
        }
        return (0xe28fc600 + (0xff & (int64(uint32(ldr.SymValue(rs) - (ldr.SymValue(syms.PLT) + int64(r.Off())) + r.Add())) >> 20)), noExtReloc, isOk);
    else if (r.Type() == objabi.R_PLT1) // add ip, ip, #0xYY000
        return (0xe28cca00 + (0xff & (int64(uint32(ldr.SymValue(rs) - (ldr.SymValue(syms.PLT) + int64(r.Off())) + r.Add() + 4)) >> 12)), noExtReloc, isOk);
    else if (r.Type() == objabi.R_PLT2) // ldr pc, [ip, #0xZZZ]!
        return (0xe5bcf000 + (0xfff & int64(uint32(ldr.SymValue(rs) - (ldr.SymValue(syms.PLT) + int64(r.Off())) + r.Add() + 8))), noExtReloc, isOk);
    else if (r.Type() == objabi.R_CALLARM) // bl XXXXXX or b YYYYYY
        // r.Add is the instruction
        // low 24-bit encodes the target address
        var t = (ldr.SymValue(rs) + int64(signext24(r.Add() & 0xffffff) * 4) - (ldr.SymValue(s) + int64(r.Off()))) / 4;
        if (t > 0x7fffff || t < -0x800000) {
            ldr.Errorf(s, "direct call too far: %s %x", ldr.SymName(rs), t);
        }
        return (int64(braddoff(int32(0xff000000 & uint32(r.Add())), int32(0xffffff & t))), noExtReloc, isOk);
        return (val, 0, false);

}

private static long archrelocvariant(ptr<ld.Target> _addr__p0, ptr<loader.Loader> _addr__p0, loader.Reloc _p0, sym.RelocVariant _p0, loader.Sym _p0, long _p0, slice<byte> _p0) {
    ref ld.Target _p0 = ref _addr__p0.val;
    ref loader.Loader _p0 = ref _addr__p0.val;

    log.Fatalf("unexpected relocation variant");
    return -1;
}

private static (loader.ExtReloc, bool) extreloc(ptr<ld.Target> _addr_target, ptr<loader.Loader> _addr_ldr, loader.Reloc r, loader.Sym s) {
    loader.ExtReloc _p0 = default;
    bool _p0 = default;
    ref ld.Target target = ref _addr_target.val;
    ref loader.Loader ldr = ref _addr_ldr.val;

    var rs = ldr.ResolveABIAlias(r.Sym());
    loader.ExtReloc rr = default;

    if (r.Type() == objabi.R_CALLARM) 
        // set up addend for eventual relocation via outer symbol.
        var (rs, off) = ld.FoldSubSymbolOffset(ldr, rs);
        rr.Xadd = int64(signext24(r.Add() & 0xffffff)) * 4 + off;
        var rst = ldr.SymType(rs);
        if (rst != sym.SHOSTOBJ && rst != sym.SDYNIMPORT && rst != sym.SUNDEFEXT && ldr.SymSect(rs) == null) {
            ldr.Errorf(s, "missing section for %s", ldr.SymName(rs));
        }
        rr.Xsym = rs;
        rr.Type = r.Type();
        rr.Size = r.Siz();
        return (rr, true);
        return (rr, false);

}

private static void addpltreloc(ptr<loader.Loader> _addr_ldr, ptr<loader.SymbolBuilder> _addr_plt, ptr<loader.SymbolBuilder> _addr_got, loader.Sym s, objabi.RelocType typ) {
    ref loader.Loader ldr = ref _addr_ldr.val;
    ref loader.SymbolBuilder plt = ref _addr_plt.val;
    ref loader.SymbolBuilder got = ref _addr_got.val;

    var (r, _) = plt.AddRel(typ);
    r.SetSym(got.Sym());
    r.SetOff(int32(plt.Size()));
    r.SetSiz(4);
    r.SetAdd(int64(ldr.SymGot(s)) - 8);

    plt.SetReachable(true);
    plt.SetSize(plt.Size() + 4);
    plt.Grow(plt.Size());
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
        var rel = ldr.MakeSymbolUpdater(syms.RelPLT);
        if (plt.Size() == 0) {
            panic("plt is not set up");
        }
        ldr.SetGot(s, int32(got.Size())); 

        // In theory, all GOT should point to the first PLT entry,
        // Linux/ARM's dynamic linker will do that for us, but FreeBSD/ARM's
        // dynamic linker won't, so we'd better do it ourselves.
        got.AddAddrPlus(target.Arch, plt.Sym(), 0); 

        // .plt entry, this depends on the .got entry
        ldr.SetPlt(s, int32(plt.Size()));

        addpltreloc(_addr_ldr, _addr_plt, _addr_got, s, objabi.R_PLT0); // add lr, pc, #0xXX00000
        addpltreloc(_addr_ldr, _addr_plt, _addr_got, s, objabi.R_PLT1); // add lr, lr, #0xYY000
        addpltreloc(_addr_ldr, _addr_plt, _addr_got, s, objabi.R_PLT2); // ldr pc, [lr, #0xZZZ]!

        // rel
        rel.AddAddrPlus(target.Arch, got.Sym(), int64(ldr.SymGot(s)));

        rel.AddUint32(target.Arch, elf.R_INFO32(uint32(ldr.SymDynid(s)), uint32(elf.R_ARM_JUMP_SLOT)));

    }
    else
 {
        ldr.Errorf(s, "addpltsym: unsupported binary format");
    }
});

private static void addgotsyminternal(ptr<ld.Target> _addr_target, ptr<loader.Loader> _addr_ldr, ptr<ld.ArchSyms> _addr_syms, loader.Sym s) {
    ref ld.Target target = ref _addr_target.val;
    ref loader.Loader ldr = ref _addr_ldr.val;
    ref ld.ArchSyms syms = ref _addr_syms.val;

    if (ldr.SymGot(s) >= 0) {
        return ;
    }
    var got = ldr.MakeSymbolUpdater(syms.GOT);
    ldr.SetGot(s, int32(got.Size()));
    got.AddAddrPlus(target.Arch, s, 0);

    if (target.IsElf())     }
    else
 {
        ldr.Errorf(s, "addgotsyminternal: unsupported binary format");
    }
}

} // end arm_package
