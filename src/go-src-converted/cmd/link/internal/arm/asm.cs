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

// package arm -- go2cs converted at 2020 October 08 04:37:14 UTC
// import "cmd/link/internal/arm" ==> using arm = go.cmd.link.@internal.arm_package
// Original source: C:\Go\src\cmd\link\internal\arm\asm.go
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
    public static partial class arm_package
    {
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
            o(0xe59f0004UL);
            o(0xe08f0000UL);

            o(0xeafffffeUL);
            loader.Reloc rel = new loader.Reloc(Off:8,Size:4,Type:objabi.R_CALLARM,Sym:addmoduledata,Add:0xeafffffe,);
            initfunc.AddReloc(rel);

            o(0x00000000UL);

            loader.Reloc rel2 = new loader.Reloc(Off:12,Size:4,Type:objabi.R_PCREL,Sym:ctxt.Moduledata2,Add:4,);
            initfunc.AddReloc(rel2);

        }

        // Preserve highest 8 bits of a, and do addition to lower 24-bit
        // of a and b; used to adjust ARM branch instruction's target
        private static int braddoff(int a, int b)
        {
            return int32((uint32(a)) & 0xff000000UL | 0x00ffffffUL & uint32(a + b));
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


            if (r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_ARM_PLT32)) 
                var su = ldr.MakeSymbolUpdater(s);
                su.SetRelocType(rIdx, objabi.R_CALLARM);

                if (targType == sym.SDYNIMPORT)
                {
                    addpltsym2(_addr_target, _addr_ldr, _addr_syms, targ);
                    su.SetRelocSym(rIdx, syms.PLT2);
                    su.SetRelocAdd(rIdx, int64(braddoff(int32(r.Add()), ldr.SymPlt(targ) / 4L)));
                }

                return true;
            else if (r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_ARM_THM_PC22)) // R_ARM_THM_CALL
                ld.Exitf("R_ARM_THM_CALL, are you using -marm?");
                return false;
            else if (r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_ARM_GOT32)) // R_ARM_GOT_BREL
                if (targType != sym.SDYNIMPORT)
                {
                    addgotsyminternal2(_addr_target, _addr_ldr, _addr_syms, targ);
                }
                else
                {
                    addgotsym2(_addr_target, _addr_ldr, _addr_syms, targ);
                }

                su = ldr.MakeSymbolUpdater(s);
                su.SetRelocType(rIdx, objabi.R_CONST); // write r->add during relocsym
                su.SetRelocSym(rIdx, 0L);
                su.SetRelocAdd(rIdx, r.Add() + int64(ldr.SymGot(targ)));
                return true;
            else if (r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_ARM_GOT_PREL)) // GOT(nil) + A - nil
                if (targType != sym.SDYNIMPORT)
                {
                    addgotsyminternal2(_addr_target, _addr_ldr, _addr_syms, targ);
                }
                else
                {
                    addgotsym2(_addr_target, _addr_ldr, _addr_syms, targ);
                }

                su = ldr.MakeSymbolUpdater(s);
                su.SetRelocType(rIdx, objabi.R_PCREL);
                su.SetRelocSym(rIdx, syms.GOT2);
                su.SetRelocAdd(rIdx, r.Add() + 4L + int64(ldr.SymGot(targ)));
                return true;
            else if (r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_ARM_GOTOFF)) // R_ARM_GOTOFF32
                su = ldr.MakeSymbolUpdater(s);
                su.SetRelocType(rIdx, objabi.R_GOTOFF);
                return true;
            else if (r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_ARM_GOTPC)) // R_ARM_BASE_PREL
                su = ldr.MakeSymbolUpdater(s);
                su.SetRelocType(rIdx, objabi.R_PCREL);
                su.SetRelocSym(rIdx, syms.GOT2);
                su.SetRelocAdd(rIdx, r.Add() + 4L);
                return true;
            else if (r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_ARM_CALL)) 
                su = ldr.MakeSymbolUpdater(s);
                su.SetRelocType(rIdx, objabi.R_CALLARM);
                if (targType == sym.SDYNIMPORT)
                {
                    addpltsym2(_addr_target, _addr_ldr, _addr_syms, targ);
                    su.SetRelocSym(rIdx, syms.PLT2);
                    su.SetRelocAdd(rIdx, int64(braddoff(int32(r.Add()), ldr.SymPlt(targ) / 4L)));
                }

                return true;
            else if (r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_ARM_REL32)) // R_ARM_REL32
                su = ldr.MakeSymbolUpdater(s);
                su.SetRelocType(rIdx, objabi.R_PCREL);
                su.SetRelocAdd(rIdx, r.Add() + 4L);
                return true;
            else if (r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_ARM_ABS32)) 
                if (targType == sym.SDYNIMPORT)
                {
                    ldr.Errorf(s, "unexpected R_ARM_ABS32 relocation for dynamic symbol %s", ldr.SymName(targ));
                }

                su = ldr.MakeSymbolUpdater(s);
                su.SetRelocType(rIdx, objabi.R_ADDR);
                return true;
            else if (r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_ARM_PC24) || r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_ARM_JUMP24)) 
                su = ldr.MakeSymbolUpdater(s);
                su.SetRelocType(rIdx, objabi.R_CALLARM);
                if (targType == sym.SDYNIMPORT)
                {
                    addpltsym2(_addr_target, _addr_ldr, _addr_syms, targ);
                    su.SetRelocSym(rIdx, syms.PLT2);
                    su.SetRelocAdd(rIdx, int64(braddoff(int32(r.Add()), ldr.SymPlt(targ) / 4L)));
                }

                return true;
            else 
                if (r.Type() >= objabi.ElfRelocOffset)
                {
                    ldr.Errorf(s, "unexpected relocation type %d (%s)", r.Type(), sym.RelocName(target.Arch, r.Type()));
                    return false;
                } 

                // Handle relocations found in ELF object files.
            // Handle references to ELF symbols from our own object files.
            if (targType != sym.SDYNIMPORT)
            {
                return true;
            } 

            // Reread the reloc to incorporate any changes in type above.
            var relocs = ldr.Relocs(s);
            r = relocs.At2(rIdx);


            if (r.Type() == objabi.R_CALLARM) 
                if (target.IsExternal())
                { 
                    // External linker will do this relocation.
                    return true;

                }

                addpltsym2(_addr_target, _addr_ldr, _addr_syms, targ);
                su = ldr.MakeSymbolUpdater(s);
                su.SetRelocSym(rIdx, syms.PLT2);
                su.SetRelocAdd(rIdx, int64(ldr.SymPlt(targ)));
                return true;
            else if (r.Type() == objabi.R_ADDR) 
                if (ldr.SymType(s) != sym.SDATA)
                {
                    break;
                }

                if (target.IsElf())
                {
                    ld.Adddynsym2(ldr, target, syms, targ);
                    var rel = ldr.MakeSymbolUpdater(syms.Rel2);
                    rel.AddAddrPlus(target.Arch, s, int64(r.Off()));
                    rel.AddUint32(target.Arch, ld.ELF32_R_INFO(uint32(ldr.SymDynid(targ)), uint32(elf.R_ARM_GLOB_DAT))); // we need a nil + A dynamic reloc
                    su = ldr.MakeSymbolUpdater(s);
                    su.SetRelocType(rIdx, objabi.R_CONST); // write r->add during relocsym
                    su.SetRelocSym(rIdx, 0L);
                    return true;

                }

                        return false;

        }

        private static bool elfreloc1(ptr<ld.Link> _addr_ctxt, ptr<sym.Reloc> _addr_r, long sectoff)
        {
            ref ld.Link ctxt = ref _addr_ctxt.val;
            ref sym.Reloc r = ref _addr_r.val;

            ctxt.Out.Write32(uint32(sectoff));

            var elfsym = ld.ElfSymForReloc(ctxt, r.Xsym);

            if (r.Type == objabi.R_ADDR || r.Type == objabi.R_DWARFSECREF) 
                if (r.Siz == 4L)
                {
                    ctxt.Out.Write32(uint32(elf.R_ARM_ABS32) | uint32(elfsym) << (int)(8L));
                }
                else
                {
                    return false;
                }

            else if (r.Type == objabi.R_PCREL) 
                if (r.Siz == 4L)
                {
                    ctxt.Out.Write32(uint32(elf.R_ARM_REL32) | uint32(elfsym) << (int)(8L));
                }
                else
                {
                    return false;
                }

            else if (r.Type == objabi.R_CALLARM) 
                if (r.Siz == 4L)
                {
                    if (r.Add & 0xff000000UL == 0xeb000000UL)
                    { // BL
                        ctxt.Out.Write32(uint32(elf.R_ARM_CALL) | uint32(elfsym) << (int)(8L));

                    }
                    else
                    {
                        ctxt.Out.Write32(uint32(elf.R_ARM_JUMP24) | uint32(elfsym) << (int)(8L));
                    }

                }
                else
                {
                    return false;
                }

            else if (r.Type == objabi.R_TLS_LE) 
                ctxt.Out.Write32(uint32(elf.R_ARM_TLS_LE32) | uint32(elfsym) << (int)(8L));
            else if (r.Type == objabi.R_TLS_IE) 
                ctxt.Out.Write32(uint32(elf.R_ARM_TLS_IE32) | uint32(elfsym) << (int)(8L));
            else if (r.Type == objabi.R_GOTPCREL) 
                if (r.Siz == 4L)
                {
                    ctxt.Out.Write32(uint32(elf.R_ARM_GOT_PREL) | uint32(elfsym) << (int)(8L));
                }
                else
                {
                    return false;
                }

            else 
                return false;
                        return true;

        }

        private static void elfsetupplt(ptr<ld.Link> _addr_ctxt, ptr<loader.SymbolBuilder> _addr_plt, ptr<loader.SymbolBuilder> _addr_got, loader.Sym dynamic)
        {
            ref ld.Link ctxt = ref _addr_ctxt.val;
            ref loader.SymbolBuilder plt = ref _addr_plt.val;
            ref loader.SymbolBuilder got = ref _addr_got.val;

            if (plt.Size() == 0L)
            { 
                // str lr, [sp, #-4]!
                plt.AddUint32(ctxt.Arch, 0xe52de004UL); 

                // ldr lr, [pc, #4]
                plt.AddUint32(ctxt.Arch, 0xe59fe004UL); 

                // add lr, pc, lr
                plt.AddUint32(ctxt.Arch, 0xe08fe00eUL); 

                // ldr pc, [lr, #8]!
                plt.AddUint32(ctxt.Arch, 0xe5bef008UL); 

                // .word &GLOBAL_OFFSET_TABLE[0] - .
                plt.AddPCRelPlus(ctxt.Arch, got.Sym(), 4L); 

                // the first .plt entry requires 3 .plt.got entries
                got.AddUint32(ctxt.Arch, 0L);

                got.AddUint32(ctxt.Arch, 0L);
                got.AddUint32(ctxt.Arch, 0L);

            }

        }

        private static bool machoreloc1(ptr<sys.Arch> _addr_arch, ptr<ld.OutBuf> _addr_@out, ptr<sym.Symbol> _addr_s, ptr<sym.Reloc> _addr_r, long sectoff)
        {
            ref sys.Arch arch = ref _addr_arch.val;
            ref ld.OutBuf @out = ref _addr_@out.val;
            ref sym.Symbol s = ref _addr_s.val;
            ref sym.Reloc r = ref _addr_r.val;

            return false;
        }

        private static bool pereloc1(ptr<sys.Arch> _addr_arch, ptr<ld.OutBuf> _addr_@out, ptr<sym.Symbol> _addr_s, ptr<sym.Reloc> _addr_r, long sectoff)
        {
            ref sys.Arch arch = ref _addr_arch.val;
            ref ld.OutBuf @out = ref _addr_@out.val;
            ref sym.Symbol s = ref _addr_s.val;
            ref sym.Reloc r = ref _addr_r.val;

            var rs = r.Xsym;

            if (rs.Dynid < 0L)
            {
                ld.Errorf(s, "reloc %d (%s) to non-coff symbol %s type=%d (%s)", r.Type, sym.RelocName(arch, r.Type), rs.Name, rs.Type, rs.Type);
                return false;
            }

            @out.Write32(uint32(sectoff));
            @out.Write32(uint32(rs.Dynid));

            uint v = default;

            if (r.Type == objabi.R_DWARFSECREF) 
                v = ld.IMAGE_REL_ARM_SECREL;
            else if (r.Type == objabi.R_ADDR) 
                v = ld.IMAGE_REL_ARM_ADDR32;
            else 
                // unsupported relocation type
                return false;
                        @out.Write16(uint16(v));

            return true;

        }

        // sign extend a 24-bit integer
        private static int signext24(long x)
        {
            return (int32(x) << (int)(8L)) >> (int)(8L);
        }

        // encode an immediate in ARM's imm12 format. copied from ../../../internal/obj/arm/asm5.go
        private static uint immrot(uint v)
        {
            for (long i = 0L; i < 16L; i++)
            {
                if (v & ~0xffUL == 0L)
                {
                    return uint32(i << (int)(8L)) | v | 1L << (int)(25L);
                }

                v = v << (int)(2L) | v >> (int)(30L);

            }

            return 0L;

        }

        // Convert the direct jump relocation r to refer to a trampoline if the target is too far
        private static void trampoline(ptr<ld.Link> _addr_ctxt, ptr<loader.Loader> _addr_ldr, long ri, loader.Sym rs, loader.Sym s)
        {
            ref ld.Link ctxt = ref _addr_ctxt.val;
            ref loader.Loader ldr = ref _addr_ldr.val;

            var relocs = ldr.Relocs(s);
            var r = relocs.At2(ri);

            if (r.Type() == objabi.R_CALLARM) 
                // r.Add is the instruction
                // low 24-bit encodes the target address
                var t = (ldr.SymValue(rs) + int64(signext24(r.Add() & 0xffffffUL) * 4L) - (ldr.SymValue(s) + int64(r.Off()))) / 4L;
                if (t > 0x7fffffUL || t < -0x800000UL || (ld.FlagDebugTramp > 1L && ldr.SymPkg(s) != ldr.SymPkg(rs).val))
                { 
                    // direct call too far, need to insert trampoline.
                    // look up existing trampolines first. if we found one within the range
                    // of direct call, we can reuse it. otherwise create a new one.
                    var offset = (signext24(r.Add() & 0xffffffUL) + 2L) * 4L;
                    loader.Sym tramp = default;
                    for (long i = 0L; >>MARKER:FOREXPRESSION_LEVEL_1<<; i++)
                    {
                        var oName = ldr.SymName(rs);
                        var name = oName + fmt.Sprintf("%+d-tramp%d", offset, i);
                        tramp = ldr.LookupOrCreateSym(name, int(ldr.SymVersion(rs)));
                        if (ldr.SymType(tramp) == sym.SDYNIMPORT)
                        { 
                            // don't reuse trampoline defined in other module
                            continue;

                        }

                        if (oName == "runtime.deferreturn")
                        {
                            ldr.SetIsDeferReturnTramp(tramp, true);
                        }

                        if (ldr.SymValue(tramp) == 0L)
                        { 
                            // either the trampoline does not exist -- we need to create one,
                            // or found one the address which is not assigned -- this will be
                            // laid down immediately after the current function. use this one.
                            break;

                        }

                        t = (ldr.SymValue(tramp) - 8L - (ldr.SymValue(s) + int64(r.Off()))) / 4L;
                        if (t >= -0x800000UL && t < 0x7fffffUL)
                        { 
                            // found an existing trampoline that is not too far
                            // we can just use it
                            break;

                        }

                    }

                    if (ldr.SymType(tramp) == 0L)
                    { 
                        // trampoline does not exist, create one
                        var trampb = ldr.MakeSymbolUpdater(tramp);
                        ctxt.AddTramp(trampb);
                        if (ctxt.DynlinkingGo())
                        {
                            if (immrot(uint32(offset)) == 0L)
                            {
                                ctxt.Errorf(s, "odd offset in dynlink direct call: %v+%d", ldr.SymName(rs), offset);
                            }

                            gentrampdyn(_addr_ctxt.Arch, _addr_trampb, rs, int64(offset));

                        }
                        else if (ctxt.BuildMode == ld.BuildModeCArchive || ctxt.BuildMode == ld.BuildModeCShared || ctxt.BuildMode == ld.BuildModePIE)
                        {
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
                    r = relocs.At2(ri);
                    r.SetSym(tramp);
                    r.SetAdd(r.Add() & 0xff000000UL | 0xfffffeUL); // clear the offset embedded in the instruction
                }

            else 
                ctxt.Errorf(s, "trampoline called with non-jump reloc: %d (%s)", r.Type(), sym.RelocName(ctxt.Arch, r.Type()));
            
        }

        // generate a trampoline to target+offset
        private static void gentramp(ptr<sys.Arch> _addr_arch, ld.LinkMode linkmode, ptr<loader.Loader> _addr_ldr, ptr<loader.SymbolBuilder> _addr_tramp, loader.Sym target, long offset)
        {
            ref sys.Arch arch = ref _addr_arch.val;
            ref loader.Loader ldr = ref _addr_ldr.val;
            ref loader.SymbolBuilder tramp = ref _addr_tramp.val;

            tramp.SetSize(12L); // 3 instructions
            var P = make_slice<byte>(tramp.Size());
            var t = ldr.SymValue(target) + offset;
            var o1 = uint32(0xe5900000UL | 11L << (int)(12L) | 15L << (int)(16L)); // MOVW (R15), R11 // R15 is actual pc + 8
            var o2 = uint32(0xe12fff10UL | 11L); // JMP  (R11)
            var o3 = uint32(t); // WORD $target
            arch.ByteOrder.PutUint32(P, o1);
            arch.ByteOrder.PutUint32(P[4L..], o2);
            arch.ByteOrder.PutUint32(P[8L..], o3);
            tramp.SetData(P);

            if (linkmode == ld.LinkExternal)
            {
                loader.Reloc r = new loader.Reloc(Off:8,Type:objabi.R_ADDR,Size:4,Sym:target,Add:offset,);
                tramp.AddReloc(r);
            }

        }

        // generate a trampoline to target+offset in position independent code
        private static void gentramppic(ptr<sys.Arch> _addr_arch, ptr<loader.SymbolBuilder> _addr_tramp, loader.Sym target, long offset)
        {
            ref sys.Arch arch = ref _addr_arch.val;
            ref loader.SymbolBuilder tramp = ref _addr_tramp.val;

            tramp.SetSize(16L); // 4 instructions
            var P = make_slice<byte>(tramp.Size());
            var o1 = uint32(0xe5900000UL | 11L << (int)(12L) | 15L << (int)(16L) | 4L); // MOVW 4(R15), R11 // R15 is actual pc + 8
            var o2 = uint32(0xe0800000UL | 11L << (int)(12L) | 15L << (int)(16L) | 11L); // ADD R15, R11, R11
            var o3 = uint32(0xe12fff10UL | 11L); // JMP  (R11)
            var o4 = uint32(0L); // WORD $(target-pc) // filled in with relocation
            arch.ByteOrder.PutUint32(P, o1);
            arch.ByteOrder.PutUint32(P[4L..], o2);
            arch.ByteOrder.PutUint32(P[8L..], o3);
            arch.ByteOrder.PutUint32(P[12L..], o4);
            tramp.SetData(P);

            loader.Reloc r = new loader.Reloc(Off:12,Type:objabi.R_PCREL,Size:4,Sym:target,Add:offset+4,);
            tramp.AddReloc(r);

        }

        // generate a trampoline to target+offset in dynlink mode (using GOT)
        private static void gentrampdyn(ptr<sys.Arch> _addr_arch, ptr<loader.SymbolBuilder> _addr_tramp, loader.Sym target, long offset)
        {
            ref sys.Arch arch = ref _addr_arch.val;
            ref loader.SymbolBuilder tramp = ref _addr_tramp.val;

            tramp.SetSize(20L); // 5 instructions
            var o1 = uint32(0xe5900000UL | 11L << (int)(12L) | 15L << (int)(16L) | 8L); // MOVW 8(R15), R11 // R15 is actual pc + 8
            var o2 = uint32(0xe0800000UL | 11L << (int)(12L) | 15L << (int)(16L) | 11L); // ADD R15, R11, R11
            var o3 = uint32(0xe5900000UL | 11L << (int)(12L) | 11L << (int)(16L)); // MOVW (R11), R11
            var o4 = uint32(0xe12fff10UL | 11L); // JMP  (R11)
            var o5 = uint32(0L); // WORD $target@GOT // filled in with relocation
            var o6 = uint32(0L);
            if (offset != 0L)
            { 
                // insert an instruction to add offset
                tramp.SetSize(24L); // 6 instructions
                o6 = o5;
                o5 = o4;
                o4 = 0xe2800000UL | 11L << (int)(12L) | 11L << (int)(16L) | immrot(uint32(offset)); // ADD $offset, R11, R11
                o1 = uint32(0xe5900000UL | 11L << (int)(12L) | 15L << (int)(16L) | 12L); // MOVW 12(R15), R11
            }

            var P = make_slice<byte>(tramp.Size());
            arch.ByteOrder.PutUint32(P, o1);
            arch.ByteOrder.PutUint32(P[4L..], o2);
            arch.ByteOrder.PutUint32(P[8L..], o3);
            arch.ByteOrder.PutUint32(P[12L..], o4);
            arch.ByteOrder.PutUint32(P[16L..], o5);
            if (offset != 0L)
            {
                arch.ByteOrder.PutUint32(P[20L..], o6);
            }

            tramp.SetData(P);

            loader.Reloc r = new loader.Reloc(Off:16,Type:objabi.R_GOTPCREL,Size:4,Sym:target,Add:8,);
            if (offset != 0L)
            { 
                // increase reloc offset by 4 as we inserted an ADD instruction
                r.Off = 20L;
                r.Add = 12L;

            }

            tramp.AddReloc(r);

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

                if (r.Type == objabi.R_CALLARM) 
                    r.Done = false; 

                    // set up addend for eventual relocation via outer symbol.
                    var rs = r.Sym;

                    r.Xadd = int64(signext24(r.Add & 0xffffffUL));
                    r.Xadd *= 4L;
                    while (rs.Outer != null)
                    {
                        r.Xadd += ld.Symaddr(rs) - ld.Symaddr(rs.Outer);
                        rs = rs.Outer;
                    }


                    if (rs.Type != sym.SHOSTOBJ && rs.Type != sym.SDYNIMPORT && rs.Type != sym.SUNDEFEXT && rs.Sect == null)
                    {
                        ld.Errorf(s, "missing section for %s", rs.Name);
                    }

                    r.Xsym = rs; 

                    // ld64 for arm seems to want the symbol table to contain offset
                    // into the section rather than pseudo virtual address that contains
                    // the section load address.
                    // we need to compensate that by removing the instruction's address
                    // from addend.
                    if (target.IsDarwin())
                    {
                        r.Xadd -= ld.Symaddr(s) + int64(r.Off);
                    }

                    if (r.Xadd / 4L > 0x7fffffUL || r.Xadd / 4L < -0x800000UL)
                    {
                        ld.Errorf(s, "direct call too far %d", r.Xadd / 4L);
                    }

                    return (int64(braddoff(int32(0xff000000UL & uint32(r.Add)), int32(0xffffffUL & uint32(r.Xadd / 4L)))), true);
                                return (-1L, false);

            }


            if (r.Type == objabi.R_CONST) 
                return (r.Add, true);
            else if (r.Type == objabi.R_GOTOFF) 
                return (ld.Symaddr(r.Sym) + r.Add - ld.Symaddr(syms.GOT), true); 

                // The following three arch specific relocations are only for generation of
                // Linux/ARM ELF's PLT entry (3 assembler instruction)
            else if (r.Type == objabi.R_PLT0) // add ip, pc, #0xXX00000
                if (ld.Symaddr(syms.GOTPLT) < ld.Symaddr(syms.PLT))
                {
                    ld.Errorf(s, ".got.plt should be placed after .plt section.");
                }

                return (0xe28fc600UL + (0xffUL & (int64(uint32(ld.Symaddr(r.Sym) - (ld.Symaddr(syms.PLT) + int64(r.Off)) + r.Add)) >> (int)(20L))), true);
            else if (r.Type == objabi.R_PLT1) // add ip, ip, #0xYY000
                return (0xe28cca00UL + (0xffUL & (int64(uint32(ld.Symaddr(r.Sym) - (ld.Symaddr(syms.PLT) + int64(r.Off)) + r.Add + 4L)) >> (int)(12L))), true);
            else if (r.Type == objabi.R_PLT2) // ldr pc, [ip, #0xZZZ]!
                return (0xe5bcf000UL + (0xfffUL & int64(uint32(ld.Symaddr(r.Sym) - (ld.Symaddr(syms.PLT) + int64(r.Off)) + r.Add + 8L))), true);
            else if (r.Type == objabi.R_CALLARM) // bl XXXXXX or b YYYYYY
                // r.Add is the instruction
                // low 24-bit encodes the target address
                var t = (ld.Symaddr(r.Sym) + int64(signext24(r.Add & 0xffffffUL) * 4L) - (s.Value + int64(r.Off))) / 4L;
                if (t > 0x7fffffUL || t < -0x800000UL)
                {
                    ld.Errorf(s, "direct call too far: %s %x", r.Sym.Name, t);
                }

                return (int64(braddoff(int32(0xff000000UL & uint32(r.Add)), int32(0xffffffUL & t))), true);
                        return (val, false);

        }

        private static long archrelocvariant(ptr<ld.Target> _addr_target, ptr<ld.ArchSyms> _addr_syms, ptr<sym.Reloc> _addr_r, ptr<sym.Symbol> _addr_s, long t)
        {
            ref ld.Target target = ref _addr_target.val;
            ref ld.ArchSyms syms = ref _addr_syms.val;
            ref sym.Reloc r = ref _addr_r.val;
            ref sym.Symbol s = ref _addr_s.val;

            log.Fatalf("unexpected relocation variant");
            return t;
        }

        private static void addpltreloc2(ptr<loader.Loader> _addr_ldr, ptr<loader.SymbolBuilder> _addr_plt, ptr<loader.SymbolBuilder> _addr_got, loader.Sym s, objabi.RelocType typ)
        {
            ref loader.Loader ldr = ref _addr_ldr.val;
            ref loader.SymbolBuilder plt = ref _addr_plt.val;
            ref loader.SymbolBuilder got = ref _addr_got.val;

            var (r, _) = plt.AddRel(typ);
            r.SetSym(got.Sym());
            r.SetOff(int32(plt.Size()));
            r.SetSiz(4L);
            r.SetAdd(int64(ldr.SymGot(s)) - 8L);

            plt.SetReachable(true);
            plt.SetSize(plt.Size() + 4L);
            plt.Grow(plt.Size());
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
                var got = ldr.MakeSymbolUpdater(syms.GOTPLT2);
                var rel = ldr.MakeSymbolUpdater(syms.RelPLT2);
                if (plt.Size() == 0L)
                {
                    panic("plt is not set up");
                } 

                // .got entry
                ldr.SetGot(s, int32(got.Size())); 

                // In theory, all GOT should point to the first PLT entry,
                // Linux/ARM's dynamic linker will do that for us, but FreeBSD/ARM's
                // dynamic linker won't, so we'd better do it ourselves.
                got.AddAddrPlus(target.Arch, plt.Sym(), 0L); 

                // .plt entry, this depends on the .got entry
                ldr.SetPlt(s, int32(plt.Size()));

                addpltreloc2(_addr_ldr, _addr_plt, _addr_got, s, objabi.R_PLT0); // add lr, pc, #0xXX00000
                addpltreloc2(_addr_ldr, _addr_plt, _addr_got, s, objabi.R_PLT1); // add lr, lr, #0xYY000
                addpltreloc2(_addr_ldr, _addr_plt, _addr_got, s, objabi.R_PLT2); // ldr pc, [lr, #0xZZZ]!

                // rel
                rel.AddAddrPlus(target.Arch, got.Sym(), int64(ldr.SymGot(s)));

                rel.AddUint32(target.Arch, ld.ELF32_R_INFO(uint32(ldr.SymDynid(s)), uint32(elf.R_ARM_JUMP_SLOT)));

            }
            else
            {
                ldr.Errorf(s, "addpltsym: unsupported binary format");
            }

        });

        private static void addgotsyminternal2(ptr<ld.Target> _addr_target, ptr<loader.Loader> _addr_ldr, ptr<ld.ArchSyms> _addr_syms, loader.Sym s)
        {
            ref ld.Target target = ref _addr_target.val;
            ref loader.Loader ldr = ref _addr_ldr.val;
            ref ld.ArchSyms syms = ref _addr_syms.val;

            if (ldr.SymGot(s) >= 0L)
            {
                return ;
            }

            var got = ldr.MakeSymbolUpdater(syms.GOT2);
            ldr.SetGot(s, int32(got.Size()));
            got.AddAddrPlus(target.Arch, s, 0L);

            if (target.IsElf())
            {
            }
            else
            {
                ldr.Errorf(s, "addgotsyminternal: unsupported binary format");
            }

        }

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
                var rel = ldr.MakeSymbolUpdater(syms.Rel2);
                rel.AddAddrPlus(target.Arch, got.Sym(), int64(ldr.SymGot(s)));
                rel.AddUint32(target.Arch, ld.ELF32_R_INFO(uint32(ldr.SymDynid(s)), uint32(elf.R_ARM_GLOB_DAT)));
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
 
            /* output symbol table */
            ld.Symsize = 0L;

            ld.Lcsize = 0L;
            var symo = uint32(0L);
            if (!ld.FlagS.val)
            { 
                // TODO: rationalize

                if (ctxt.HeadType == objabi.Hplan9) 
                    symo = uint32(ld.Segdata.Fileoff + ld.Segdata.Filelen);
                else if (ctxt.HeadType == objabi.Hwindows) 
                    symo = uint32(ld.Segdwarf.Fileoff + ld.Segdwarf.Filelen);
                    symo = uint32(ld.Rnd(int64(symo), ld.PEFILEALIGN));
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

                else if (ctxt.HeadType == objabi.Hwindows)                 else 
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
                ctxt.Out.Write32b(0x647UL); /* magic */
                ctxt.Out.Write32b(uint32(ld.Segtext.Filelen)); /* sizes */
                ctxt.Out.Write32b(uint32(ld.Segdata.Filelen));
                ctxt.Out.Write32b(uint32(ld.Segdata.Length - ld.Segdata.Filelen));
                ctxt.Out.Write32b(uint32(ld.Symsize)); /* nsyms */
                ctxt.Out.Write32b(uint32(ld.Entryvalue(ctxt))); /* va of entry */
                ctxt.Out.Write32b(0L);
                ctxt.Out.Write32b(uint32(ld.Lcsize));
            else if (ctxt.HeadType == objabi.Hlinux || ctxt.HeadType == objabi.Hfreebsd || ctxt.HeadType == objabi.Hnetbsd || ctxt.HeadType == objabi.Hopenbsd) 
                ld.Asmbelf(ctxt, int64(symo));
            else if (ctxt.HeadType == objabi.Hwindows) 
                ld.Asmbpe(ctxt);
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
