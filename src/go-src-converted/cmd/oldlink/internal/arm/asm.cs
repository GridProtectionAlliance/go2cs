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

// package arm -- go2cs converted at 2020 October 09 05:50:48 UTC
// import "cmd/oldlink/internal/arm" ==> using arm = go.cmd.oldlink.@internal.arm_package
// Original source: C:\Go\src\cmd\oldlink\internal\arm\asm.go
using objabi = go.cmd.@internal.objabi_package;
using sys = go.cmd.@internal.sys_package;
using ld = go.cmd.oldlink.@internal.ld_package;
using sym = go.cmd.oldlink.@internal.sym_package;
using elf = go.debug.elf_package;
using fmt = go.fmt_package;
using log = go.log_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace oldlink {
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
        private static void gentext(ptr<ld.Link> _addr_ctxt)
        {
            ref ld.Link ctxt = ref _addr_ctxt.val;

            if (!ctxt.DynlinkingGo())
            {
                return ;
            }
            var addmoduledata = ctxt.Syms.Lookup("runtime.addmoduledata", 0L);
            if (addmoduledata.Type == sym.STEXT && ctxt.BuildMode != ld.BuildModePlugin)
            { 
                // we're linking a module containing the runtime -> no need for
                // an init function
                return ;

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
            o(0xe59f0004UL);
            o(0xe08f0000UL);

            o(0xeafffffeUL);
            var rel = initfunc.AddRel();
            rel.Off = 8L;
            rel.Siz = 4L;
            rel.Sym = ctxt.Syms.Lookup("runtime.addmoduledata", 0L);
            rel.Type = objabi.R_CALLARM;
            rel.Add = 0xeafffffeUL; // vomit

            o(0x00000000UL);
            rel = initfunc.AddRel();
            rel.Off = 12L;
            rel.Siz = 4L;
            rel.Sym = ctxt.Moduledata;
            rel.Type = objabi.R_PCREL;
            rel.Add = 4L;

            if (ctxt.BuildMode == ld.BuildModePlugin)
            {
                ctxt.Textp = append(ctxt.Textp, addmoduledata);
            }
            ctxt.Textp = append(ctxt.Textp, initfunc);
            var initarray_entry = ctxt.Syms.Lookup("go.link.addmoduledatainit", 0L);
            initarray_entry.Attr |= sym.AttrReachable;
            initarray_entry.Attr |= sym.AttrLocal;
            initarray_entry.Type = sym.SINITARR;
            initarray_entry.AddAddr(ctxt.Arch, initfunc);

        }

        // Preserve highest 8 bits of a, and do addition to lower 24-bit
        // of a and b; used to adjust ARM branch instruction's target
        private static int braddoff(int a, int b)
        {
            return int32((uint32(a)) & 0xff000000UL | 0x00ffffffUL & uint32(a + b));
        }

        private static bool adddynrel(ptr<ld.Link> _addr_ctxt, ptr<sym.Symbol> _addr_s, ptr<sym.Reloc> _addr_r)
        {
            ref ld.Link ctxt = ref _addr_ctxt.val;
            ref sym.Symbol s = ref _addr_s.val;
            ref sym.Reloc r = ref _addr_r.val;

            var targ = r.Sym;


            if (r.Type == objabi.ElfRelocOffset + objabi.RelocType(elf.R_ARM_PLT32)) 
                r.Type = objabi.R_CALLARM;

                if (targ.Type == sym.SDYNIMPORT)
                {
                    addpltsym(_addr_ctxt, _addr_targ);
                    r.Sym = ctxt.Syms.Lookup(".plt", 0L);
                    r.Add = int64(braddoff(int32(r.Add), targ.Plt() / 4L));
                }

                return true;
            else if (r.Type == objabi.ElfRelocOffset + objabi.RelocType(elf.R_ARM_THM_PC22)) // R_ARM_THM_CALL
                ld.Exitf("R_ARM_THM_CALL, are you using -marm?");
                return false;
            else if (r.Type == objabi.ElfRelocOffset + objabi.RelocType(elf.R_ARM_GOT32)) // R_ARM_GOT_BREL
                if (targ.Type != sym.SDYNIMPORT)
                {
                    addgotsyminternal(_addr_ctxt, _addr_targ);
                }
                else
                {
                    addgotsym(_addr_ctxt, _addr_targ);
                }

                r.Type = objabi.R_CONST; // write r->add during relocsym
                r.Sym = null;
                r.Add += int64(targ.Got());
                return true;
            else if (r.Type == objabi.ElfRelocOffset + objabi.RelocType(elf.R_ARM_GOT_PREL)) // GOT(nil) + A - nil
                if (targ.Type != sym.SDYNIMPORT)
                {
                    addgotsyminternal(_addr_ctxt, _addr_targ);
                }
                else
                {
                    addgotsym(_addr_ctxt, _addr_targ);
                }

                r.Type = objabi.R_PCREL;
                r.Sym = ctxt.Syms.Lookup(".got", 0L);
                r.Add += int64(targ.Got()) + 4L;
                return true;
            else if (r.Type == objabi.ElfRelocOffset + objabi.RelocType(elf.R_ARM_GOTOFF)) // R_ARM_GOTOFF32
                r.Type = objabi.R_GOTOFF;

                return true;
            else if (r.Type == objabi.ElfRelocOffset + objabi.RelocType(elf.R_ARM_GOTPC)) // R_ARM_BASE_PREL
                r.Type = objabi.R_PCREL;

                r.Sym = ctxt.Syms.Lookup(".got", 0L);
                r.Add += 4L;
                return true;
            else if (r.Type == objabi.ElfRelocOffset + objabi.RelocType(elf.R_ARM_CALL)) 
                r.Type = objabi.R_CALLARM;
                if (targ.Type == sym.SDYNIMPORT)
                {
                    addpltsym(_addr_ctxt, _addr_targ);
                    r.Sym = ctxt.Syms.Lookup(".plt", 0L);
                    r.Add = int64(braddoff(int32(r.Add), targ.Plt() / 4L));
                }

                return true;
            else if (r.Type == objabi.ElfRelocOffset + objabi.RelocType(elf.R_ARM_REL32)) // R_ARM_REL32
                r.Type = objabi.R_PCREL;

                r.Add += 4L;
                return true;
            else if (r.Type == objabi.ElfRelocOffset + objabi.RelocType(elf.R_ARM_ABS32)) 
                if (targ.Type == sym.SDYNIMPORT)
                {
                    ld.Errorf(s, "unexpected R_ARM_ABS32 relocation for dynamic symbol %s", targ.Name);
                }

                r.Type = objabi.R_ADDR;
                return true; 

                // we can just ignore this, because we are targeting ARM V5+ anyway
            else if (r.Type == objabi.ElfRelocOffset + objabi.RelocType(elf.R_ARM_V4BX)) 
                if (r.Sym != null)
                { 
                    // R_ARM_V4BX is ABS relocation, so this symbol is a dummy symbol, ignore it
                    r.Sym.Type = 0L;

                }

                r.Sym = null;
                return true;
            else if (r.Type == objabi.ElfRelocOffset + objabi.RelocType(elf.R_ARM_PC24) || r.Type == objabi.ElfRelocOffset + objabi.RelocType(elf.R_ARM_JUMP24)) 
                r.Type = objabi.R_CALLARM;
                if (targ.Type == sym.SDYNIMPORT)
                {
                    addpltsym(_addr_ctxt, _addr_targ);
                    r.Sym = ctxt.Syms.Lookup(".plt", 0L);
                    r.Add = int64(braddoff(int32(r.Add), targ.Plt() / 4L));
                }

                return true;
            else 
                if (r.Type >= objabi.ElfRelocOffset)
                {
                    ld.Errorf(s, "unexpected relocation type %d (%s)", r.Type, sym.RelocName(ctxt.Arch, r.Type));
                    return false;
                } 

                // Handle relocations found in ELF object files.
            // Handle references to ELF symbols from our own object files.
            if (targ.Type != sym.SDYNIMPORT)
            {
                return true;
            }


            if (r.Type == objabi.R_CALLARM) 
                if (ctxt.LinkMode == ld.LinkExternal)
                { 
                    // External linker will do this relocation.
                    return true;

                }

                addpltsym(_addr_ctxt, _addr_targ);
                r.Sym = ctxt.Syms.Lookup(".plt", 0L);
                r.Add = int64(targ.Plt());
                return true;
            else if (r.Type == objabi.R_ADDR) 
                if (s.Type != sym.SDATA)
                {
                    break;
                }

                if (ctxt.IsELF)
                {
                    ld.Adddynsym(ctxt, targ);
                    var rel = ctxt.Syms.Lookup(".rel", 0L);
                    rel.AddAddrPlus(ctxt.Arch, s, int64(r.Off));
                    rel.AddUint32(ctxt.Arch, ld.ELF32_R_INFO(uint32(targ.Dynid), uint32(elf.R_ARM_GLOB_DAT))); // we need a nil + A dynamic reloc
                    r.Type = objabi.R_CONST; // write r->add during relocsym
                    r.Sym = null;
                    return true;

                }

                        return false;

        }

        private static bool elfreloc1(ptr<ld.Link> _addr_ctxt, ptr<sym.Reloc> _addr_r, long sectoff)
        {
            ref ld.Link ctxt = ref _addr_ctxt.val;
            ref sym.Reloc r = ref _addr_r.val;

            ctxt.Out.Write32(uint32(sectoff));

            var elfsym = r.Xsym.ElfsymForReloc();

            if (r.Type == objabi.R_ADDR) 
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

        private static void elfsetupplt(ptr<ld.Link> _addr_ctxt)
        {
            ref ld.Link ctxt = ref _addr_ctxt.val;

            var plt = ctxt.Syms.Lookup(".plt", 0L);
            var got = ctxt.Syms.Lookup(".got.plt", 0L);
            if (plt.Size == 0L)
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
                plt.AddPCRelPlus(ctxt.Arch, got, 4L); 

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
        private static void trampoline(ptr<ld.Link> _addr_ctxt, ptr<sym.Reloc> _addr_r, ptr<sym.Symbol> _addr_s)
        {
            ref ld.Link ctxt = ref _addr_ctxt.val;
            ref sym.Reloc r = ref _addr_r.val;
            ref sym.Symbol s = ref _addr_s.val;


            if (r.Type == objabi.R_CALLARM) 
                // r.Add is the instruction
                // low 24-bit encodes the target address
                var t = (ld.Symaddr(r.Sym) + int64(signext24(r.Add & 0xffffffUL) * 4L) - (s.Value + int64(r.Off))) / 4L;
                if (t > 0x7fffffUL || t < -0x800000UL || (ld.FlagDebugTramp > 1L && s.File != r.Sym.File.val))
                { 
                    // direct call too far, need to insert trampoline.
                    // look up existing trampolines first. if we found one within the range
                    // of direct call, we can reuse it. otherwise create a new one.
                    var offset = (signext24(r.Add & 0xffffffUL) + 2L) * 4L;
                    ptr<sym.Symbol> tramp;
                    for (long i = 0L; >>MARKER:FOREXPRESSION_LEVEL_1<<; i++)
                    {
                        var oName = r.Sym.Name;
                        var name = oName + fmt.Sprintf("%+d-tramp%d", offset, i);
                        tramp = ctxt.Syms.Lookup(name, int(r.Sym.Version));
                        if (oName == "runtime.deferreturn")
                        {
                            tramp.Attr.Set(sym.AttrDeferReturnTramp, true);
                        }

                        if (tramp.Type == sym.SDYNIMPORT)
                        { 
                            // don't reuse trampoline defined in other module
                            continue;

                        }

                        if (tramp.Value == 0L)
                        { 
                            // either the trampoline does not exist -- we need to create one,
                            // or found one the address which is not assigned -- this will be
                            // laid down immediately after the current function. use this one.
                            break;

                        }

                        t = (ld.Symaddr(tramp) - 8L - (s.Value + int64(r.Off))) / 4L;
                        if (t >= -0x800000UL && t < 0x7fffffUL)
                        { 
                            // found an existing trampoline that is not too far
                            // we can just use it
                            break;

                        }

                    }

                    if (tramp.Type == 0L)
                    { 
                        // trampoline does not exist, create one
                        ctxt.AddTramp(tramp);
                        if (ctxt.DynlinkingGo())
                        {
                            if (immrot(uint32(offset)) == 0L)
                            {
                                ld.Errorf(s, "odd offset in dynlink direct call: %v+%d", r.Sym, offset);
                            }

                            gentrampdyn(_addr_ctxt.Arch, tramp, _addr_r.Sym, int64(offset));

                        }
                        else if (ctxt.BuildMode == ld.BuildModeCArchive || ctxt.BuildMode == ld.BuildModeCShared || ctxt.BuildMode == ld.BuildModePIE)
                        {
                            gentramppic(_addr_ctxt.Arch, tramp, _addr_r.Sym, int64(offset));
                        }
                        else
                        {
                            gentramp(_addr_ctxt.Arch, ctxt.LinkMode, tramp, _addr_r.Sym, int64(offset));
                        }

                    } 
                    // modify reloc to point to tramp, which will be resolved later
                    r.Sym = tramp;
                    r.Add = r.Add & 0xff000000UL | 0xfffffeUL; // clear the offset embedded in the instruction
                    r.Done = false;

                }

            else 
                ld.Errorf(s, "trampoline called with non-jump reloc: %d (%s)", r.Type, sym.RelocName(ctxt.Arch, r.Type));
            
        }

        // generate a trampoline to target+offset
        private static void gentramp(ptr<sys.Arch> _addr_arch, ld.LinkMode linkmode, ptr<sym.Symbol> _addr_tramp, ptr<sym.Symbol> _addr_target, long offset)
        {
            ref sys.Arch arch = ref _addr_arch.val;
            ref sym.Symbol tramp = ref _addr_tramp.val;
            ref sym.Symbol target = ref _addr_target.val;

            tramp.Size = 12L; // 3 instructions
            tramp.P = make_slice<byte>(tramp.Size);
            var t = ld.Symaddr(target) + offset;
            var o1 = uint32(0xe5900000UL | 11L << (int)(12L) | 15L << (int)(16L)); // MOVW (R15), R11 // R15 is actual pc + 8
            var o2 = uint32(0xe12fff10UL | 11L); // JMP  (R11)
            var o3 = uint32(t); // WORD $target
            arch.ByteOrder.PutUint32(tramp.P, o1);
            arch.ByteOrder.PutUint32(tramp.P[4L..], o2);
            arch.ByteOrder.PutUint32(tramp.P[8L..], o3);

            if (linkmode == ld.LinkExternal)
            {
                var r = tramp.AddRel();
                r.Off = 8L;
                r.Type = objabi.R_ADDR;
                r.Siz = 4L;
                r.Sym = target;
                r.Add = offset;
            }

        }

        // generate a trampoline to target+offset in position independent code
        private static void gentramppic(ptr<sys.Arch> _addr_arch, ptr<sym.Symbol> _addr_tramp, ptr<sym.Symbol> _addr_target, long offset)
        {
            ref sys.Arch arch = ref _addr_arch.val;
            ref sym.Symbol tramp = ref _addr_tramp.val;
            ref sym.Symbol target = ref _addr_target.val;

            tramp.Size = 16L; // 4 instructions
            tramp.P = make_slice<byte>(tramp.Size);
            var o1 = uint32(0xe5900000UL | 11L << (int)(12L) | 15L << (int)(16L) | 4L); // MOVW 4(R15), R11 // R15 is actual pc + 8
            var o2 = uint32(0xe0800000UL | 11L << (int)(12L) | 15L << (int)(16L) | 11L); // ADD R15, R11, R11
            var o3 = uint32(0xe12fff10UL | 11L); // JMP  (R11)
            var o4 = uint32(0L); // WORD $(target-pc) // filled in with relocation
            arch.ByteOrder.PutUint32(tramp.P, o1);
            arch.ByteOrder.PutUint32(tramp.P[4L..], o2);
            arch.ByteOrder.PutUint32(tramp.P[8L..], o3);
            arch.ByteOrder.PutUint32(tramp.P[12L..], o4);

            var r = tramp.AddRel();
            r.Off = 12L;
            r.Type = objabi.R_PCREL;
            r.Siz = 4L;
            r.Sym = target;
            r.Add = offset + 4L;

        }

        // generate a trampoline to target+offset in dynlink mode (using GOT)
        private static void gentrampdyn(ptr<sys.Arch> _addr_arch, ptr<sym.Symbol> _addr_tramp, ptr<sym.Symbol> _addr_target, long offset)
        {
            ref sys.Arch arch = ref _addr_arch.val;
            ref sym.Symbol tramp = ref _addr_tramp.val;
            ref sym.Symbol target = ref _addr_target.val;

            tramp.Size = 20L; // 5 instructions
            var o1 = uint32(0xe5900000UL | 11L << (int)(12L) | 15L << (int)(16L) | 8L); // MOVW 8(R15), R11 // R15 is actual pc + 8
            var o2 = uint32(0xe0800000UL | 11L << (int)(12L) | 15L << (int)(16L) | 11L); // ADD R15, R11, R11
            var o3 = uint32(0xe5900000UL | 11L << (int)(12L) | 11L << (int)(16L)); // MOVW (R11), R11
            var o4 = uint32(0xe12fff10UL | 11L); // JMP  (R11)
            var o5 = uint32(0L); // WORD $target@GOT // filled in with relocation
            var o6 = uint32(0L);
            if (offset != 0L)
            { 
                // insert an instruction to add offset
                tramp.Size = 24L; // 6 instructions
                o6 = o5;
                o5 = o4;
                o4 = 0xe2800000UL | 11L << (int)(12L) | 11L << (int)(16L) | immrot(uint32(offset)); // ADD $offset, R11, R11
                o1 = uint32(0xe5900000UL | 11L << (int)(12L) | 15L << (int)(16L) | 12L); // MOVW 12(R15), R11
            }

            tramp.P = make_slice<byte>(tramp.Size);
            arch.ByteOrder.PutUint32(tramp.P, o1);
            arch.ByteOrder.PutUint32(tramp.P[4L..], o2);
            arch.ByteOrder.PutUint32(tramp.P[8L..], o3);
            arch.ByteOrder.PutUint32(tramp.P[12L..], o4);
            arch.ByteOrder.PutUint32(tramp.P[16L..], o5);
            if (offset != 0L)
            {
                arch.ByteOrder.PutUint32(tramp.P[20L..], o6);
            }

            var r = tramp.AddRel();
            r.Off = 16L;
            r.Type = objabi.R_GOTPCREL;
            r.Siz = 4L;
            r.Sym = target;
            r.Add = 8L;
            if (offset != 0L)
            { 
                // increase reloc offset by 4 as we inserted an ADD instruction
                r.Off = 20L;
                r.Add = 12L;

            }

        }

        private static (long, bool) archreloc(ptr<ld.Link> _addr_ctxt, ptr<sym.Reloc> _addr_r, ptr<sym.Symbol> _addr_s, long val)
        {
            long _p0 = default;
            bool _p0 = default;
            ref ld.Link ctxt = ref _addr_ctxt.val;
            ref sym.Reloc r = ref _addr_r.val;
            ref sym.Symbol s = ref _addr_s.val;

            if (ctxt.LinkMode == ld.LinkExternal)
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
                return (ld.Symaddr(r.Sym) + r.Add - ld.Symaddr(ctxt.Syms.Lookup(".got", 0L)), true); 

                // The following three arch specific relocations are only for generation of
                // Linux/ARM ELF's PLT entry (3 assembler instruction)
            else if (r.Type == objabi.R_PLT0) // add ip, pc, #0xXX00000
                if (ld.Symaddr(ctxt.Syms.Lookup(".got.plt", 0L)) < ld.Symaddr(ctxt.Syms.Lookup(".plt", 0L)))
                {
                    ld.Errorf(s, ".got.plt should be placed after .plt section.");
                }

                return (0xe28fc600UL + (0xffUL & (int64(uint32(ld.Symaddr(r.Sym) - (ld.Symaddr(ctxt.Syms.Lookup(".plt", 0L)) + int64(r.Off)) + r.Add)) >> (int)(20L))), true);
            else if (r.Type == objabi.R_PLT1) // add ip, ip, #0xYY000
                return (0xe28cca00UL + (0xffUL & (int64(uint32(ld.Symaddr(r.Sym) - (ld.Symaddr(ctxt.Syms.Lookup(".plt", 0L)) + int64(r.Off)) + r.Add + 4L)) >> (int)(12L))), true);
            else if (r.Type == objabi.R_PLT2) // ldr pc, [ip, #0xZZZ]!
                return (0xe5bcf000UL + (0xfffUL & int64(uint32(ld.Symaddr(r.Sym) - (ld.Symaddr(ctxt.Syms.Lookup(".plt", 0L)) + int64(r.Off)) + r.Add + 8L))), true);
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

        private static long archrelocvariant(ptr<ld.Link> _addr_ctxt, ptr<sym.Reloc> _addr_r, ptr<sym.Symbol> _addr_s, long t)
        {
            ref ld.Link ctxt = ref _addr_ctxt.val;
            ref sym.Reloc r = ref _addr_r.val;
            ref sym.Symbol s = ref _addr_s.val;

            log.Fatalf("unexpected relocation variant");
            return t;
        }

        private static void addpltreloc(ptr<ld.Link> _addr_ctxt, ptr<sym.Symbol> _addr_plt, ptr<sym.Symbol> _addr_got, ptr<sym.Symbol> _addr_s, objabi.RelocType typ)
        {
            ref ld.Link ctxt = ref _addr_ctxt.val;
            ref sym.Symbol plt = ref _addr_plt.val;
            ref sym.Symbol got = ref _addr_got.val;
            ref sym.Symbol s = ref _addr_s.val;

            var r = plt.AddRel();
            r.Sym = got;
            r.Off = int32(plt.Size);
            r.Siz = 4L;
            r.Type = typ;
            r.Add = int64(s.Got()) - 8L;

            plt.Attr |= sym.AttrReachable;
            plt.Size += 4L;
            plt.Grow(plt.Size);
        }

        private static void addpltsym(ptr<ld.Link> _addr_ctxt, ptr<sym.Symbol> _addr_s)
        {
            ref ld.Link ctxt = ref _addr_ctxt.val;
            ref sym.Symbol s = ref _addr_s.val;

            if (s.Plt() >= 0L)
            {
                return ;
            }

            ld.Adddynsym(ctxt, s);

            if (ctxt.IsELF)
            {
                var plt = ctxt.Syms.Lookup(".plt", 0L);
                var got = ctxt.Syms.Lookup(".got.plt", 0L);
                var rel = ctxt.Syms.Lookup(".rel.plt", 0L);
                if (plt.Size == 0L)
                {
                    elfsetupplt(_addr_ctxt);
                } 

                // .got entry
                s.SetGot(int32(got.Size)); 

                // In theory, all GOT should point to the first PLT entry,
                // Linux/ARM's dynamic linker will do that for us, but FreeBSD/ARM's
                // dynamic linker won't, so we'd better do it ourselves.
                got.AddAddrPlus(ctxt.Arch, plt, 0L); 

                // .plt entry, this depends on the .got entry
                s.SetPlt(int32(plt.Size));

                addpltreloc(_addr_ctxt, _addr_plt, _addr_got, _addr_s, objabi.R_PLT0); // add lr, pc, #0xXX00000
                addpltreloc(_addr_ctxt, _addr_plt, _addr_got, _addr_s, objabi.R_PLT1); // add lr, lr, #0xYY000
                addpltreloc(_addr_ctxt, _addr_plt, _addr_got, _addr_s, objabi.R_PLT2); // ldr pc, [lr, #0xZZZ]!

                // rel
                rel.AddAddrPlus(ctxt.Arch, got, int64(s.Got()));

                rel.AddUint32(ctxt.Arch, ld.ELF32_R_INFO(uint32(s.Dynid), uint32(elf.R_ARM_JUMP_SLOT)));

            }
            else
            {
                ld.Errorf(s, "addpltsym: unsupported binary format");
            }

        }

        private static void addgotsyminternal(ptr<ld.Link> _addr_ctxt, ptr<sym.Symbol> _addr_s)
        {
            ref ld.Link ctxt = ref _addr_ctxt.val;
            ref sym.Symbol s = ref _addr_s.val;

            if (s.Got() >= 0L)
            {
                return ;
            }

            var got = ctxt.Syms.Lookup(".got", 0L);
            s.SetGot(int32(got.Size));

            got.AddAddrPlus(ctxt.Arch, s, 0L);

            if (ctxt.IsELF)
            {
            }
            else
            {
                ld.Errorf(s, "addgotsyminternal: unsupported binary format");
            }

        }

        private static void addgotsym(ptr<ld.Link> _addr_ctxt, ptr<sym.Symbol> _addr_s)
        {
            ref ld.Link ctxt = ref _addr_ctxt.val;
            ref sym.Symbol s = ref _addr_s.val;

            if (s.Got() >= 0L)
            {
                return ;
            }

            ld.Adddynsym(ctxt, s);
            var got = ctxt.Syms.Lookup(".got", 0L);
            s.SetGot(int32(got.Size));
            got.AddUint32(ctxt.Arch, 0L);

            if (ctxt.IsELF)
            {
                var rel = ctxt.Syms.Lookup(".rel", 0L);
                rel.AddAddrPlus(ctxt.Arch, got, int64(s.Got()));
                rel.AddUint32(ctxt.Arch, ld.ELF32_R_INFO(uint32(s.Dynid), uint32(elf.R_ARM_GLOB_DAT)));
            }
            else
            {
                ld.Errorf(s, "addgotsym: unsupported binary format");
            }

        }

        private static void asmb(ptr<ld.Link> _addr_ctxt)
        {
            ref ld.Link ctxt = ref _addr_ctxt.val;

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
                ctxt.Out.SeekSet(int64(ld.Segrodata.Fileoff));
                ld.Datblk(ctxt, int64(ld.Segrodata.Vaddr), int64(ld.Segrodata.Filelen));
            }

            if (ld.Segrelrodata.Filelen > 0L)
            {
                ctxt.Out.SeekSet(int64(ld.Segrelrodata.Fileoff));
                ld.Datblk(ctxt, int64(ld.Segrelrodata.Vaddr), int64(ld.Segrelrodata.Filelen));
            }

            ctxt.Out.SeekSet(int64(ld.Segdata.Fileoff));
            ld.Datblk(ctxt, int64(ld.Segdata.Vaddr), int64(ld.Segdata.Filelen));

            ctxt.Out.SeekSet(int64(ld.Segdwarf.Fileoff));
            ld.Dwarfblk(ctxt, int64(ld.Segdwarf.Vaddr), int64(ld.Segdwarf.Filelen));

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
                    ctxt.Out.Flush();

                    var sym = ctxt.Syms.Lookup("pclntab", 0L);
                    if (sym != null)
                    {
                        ld.Lcsize = int32(len(sym.P));
                        ctxt.Out.Write(sym.P);
                        ctxt.Out.Flush();
                    }

                else if (ctxt.HeadType == objabi.Hwindows)                 else 
                    if (ctxt.IsELF)
                    {
                        ld.Asmelfsym(ctxt);
                        ctxt.Out.Flush();
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
            else                         ctxt.Out.Flush();
            if (ld.FlagC.val)
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
