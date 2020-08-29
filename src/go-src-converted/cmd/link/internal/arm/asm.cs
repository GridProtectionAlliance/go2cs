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

// package arm -- go2cs converted at 2020 August 29 10:02:25 UTC
// import "cmd/link/internal/arm" ==> using arm = go.cmd.link.@internal.arm_package
// Original source: C:\Go\src\cmd\link\internal\arm\asm.go
using objabi = go.cmd.@internal.objabi_package;
using sys = go.cmd.@internal.sys_package;
using ld = go.cmd.link.@internal.ld_package;
using sym = go.cmd.link.@internal.sym_package;
using elf = go.debug.elf_package;
using fmt = go.fmt_package;
using log = go.log_package;
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
        private static void gentext(ref ld.Link ctxt)
        {
            if (!ctxt.DynlinkingGo())
            {
                return;
            }
            var addmoduledata = ctxt.Syms.Lookup("runtime.addmoduledata", 0L);
            if (addmoduledata.Type == sym.STEXT && ctxt.BuildMode != ld.BuildModePlugin)
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

        private static bool adddynrel(ref ld.Link ctxt, ref sym.Symbol s, ref sym.Reloc r)
        {
            var targ = r.Sym;

            switch (r.Type)
            {
                case 256L + objabi.RelocType(elf.R_ARM_PLT32): 
                    r.Type = objabi.R_CALLARM;

                    if (targ.Type == sym.SDYNIMPORT)
                    {
                        addpltsym(ctxt, targ);
                        r.Sym = ctxt.Syms.Lookup(".plt", 0L);
                        r.Add = int64(braddoff(int32(r.Add), targ.Plt / 4L));
                    }
                    return true;
                    break;
                case 256L + objabi.RelocType(elf.R_ARM_THM_PC22): // R_ARM_THM_CALL
                    ld.Exitf("R_ARM_THM_CALL, are you using -marm?");
                    return false;
                    break;
                case 256L + objabi.RelocType(elf.R_ARM_GOT32): // R_ARM_GOT_BREL
                    if (targ.Type != sym.SDYNIMPORT)
                    {
                        addgotsyminternal(ctxt, targ);
                    }
                    else
                    {
                        addgotsym(ctxt, targ);
                    }
                    r.Type = objabi.R_CONST; // write r->add during relocsym
                    r.Sym = null;
                    r.Add += int64(targ.Got);
                    return true;
                    break;
                case 256L + objabi.RelocType(elf.R_ARM_GOT_PREL): // GOT(nil) + A - nil
                    if (targ.Type != sym.SDYNIMPORT)
                    {
                        addgotsyminternal(ctxt, targ);
                    }
                    else
                    {
                        addgotsym(ctxt, targ);
                    }
                    r.Type = objabi.R_PCREL;
                    r.Sym = ctxt.Syms.Lookup(".got", 0L);
                    r.Add += int64(targ.Got) + 4L;
                    return true;
                    break;
                case 256L + objabi.RelocType(elf.R_ARM_GOTOFF): // R_ARM_GOTOFF32
                    r.Type = objabi.R_GOTOFF;

                    return true;
                    break;
                case 256L + objabi.RelocType(elf.R_ARM_GOTPC): // R_ARM_BASE_PREL
                    r.Type = objabi.R_PCREL;

                    r.Sym = ctxt.Syms.Lookup(".got", 0L);
                    r.Add += 4L;
                    return true;
                    break;
                case 256L + objabi.RelocType(elf.R_ARM_CALL): 
                    r.Type = objabi.R_CALLARM;
                    if (targ.Type == sym.SDYNIMPORT)
                    {
                        addpltsym(ctxt, targ);
                        r.Sym = ctxt.Syms.Lookup(".plt", 0L);
                        r.Add = int64(braddoff(int32(r.Add), targ.Plt / 4L));
                    }
                    return true;
                    break;
                case 256L + objabi.RelocType(elf.R_ARM_REL32): // R_ARM_REL32
                    r.Type = objabi.R_PCREL;

                    r.Add += 4L;
                    return true;
                    break;
                case 256L + objabi.RelocType(elf.R_ARM_ABS32): 
                    if (targ.Type == sym.SDYNIMPORT)
                    {
                        ld.Errorf(s, "unexpected R_ARM_ABS32 relocation for dynamic symbol %s", targ.Name);
                    }
                    r.Type = objabi.R_ADDR;
                    return true; 

                    // we can just ignore this, because we are targeting ARM V5+ anyway
                    break;
                case 256L + objabi.RelocType(elf.R_ARM_V4BX): 
                    if (r.Sym != null)
                    { 
                        // R_ARM_V4BX is ABS relocation, so this symbol is a dummy symbol, ignore it
                        r.Sym.Type = 0L;
                    }
                    r.Sym = null;
                    return true;
                    break;
                case 256L + objabi.RelocType(elf.R_ARM_PC24): 

                case 256L + objabi.RelocType(elf.R_ARM_JUMP24): 
                    r.Type = objabi.R_CALLARM;
                    if (targ.Type == sym.SDYNIMPORT)
                    {
                        addpltsym(ctxt, targ);
                        r.Sym = ctxt.Syms.Lookup(".plt", 0L);
                        r.Add = int64(braddoff(int32(r.Add), targ.Plt / 4L));
                    }
                    return true;
                    break;
                default: 
                    if (r.Type >= 256L)
                    {
                        ld.Errorf(s, "unexpected relocation type %d (%s)", r.Type, sym.RelocName(ctxt.Arch, r.Type));
                        return false;
                    } 

                    // Handle relocations found in ELF object files.
                    break;
            } 

            // Handle references to ELF symbols from our own object files.
            if (targ.Type != sym.SDYNIMPORT)
            {
                return true;
            }

            if (r.Type == objabi.R_CALLARM) 
                addpltsym(ctxt, targ);
                r.Sym = ctxt.Syms.Lookup(".plt", 0L);
                r.Add = int64(targ.Plt);
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

        private static bool elfreloc1(ref ld.Link ctxt, ref sym.Reloc r, long sectoff)
        {
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

        private static void elfsetupplt(ref ld.Link ctxt)
        {
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

        private static bool machoreloc1(ref sys.Arch arch, ref ld.OutBuf @out, ref sym.Symbol s, ref sym.Reloc r, long sectoff)
        {
            uint v = default;

            var rs = r.Xsym;

            if (r.Type == objabi.R_PCREL)
            {
                if (rs.Type == sym.SHOSTOBJ)
                {
                    ld.Errorf(s, "pc-relative relocation of external symbol is not supported");
                    return false;
                }
                if (r.Siz != 4L)
                {
                    return false;
                } 

                // emit a pair of "scattered" relocations that
                // resolve to the difference of section addresses of
                // the symbol and the instruction
                // this value is added to the field being relocated
                var o1 = uint32(sectoff);
                o1 |= 1L << (int)(31L); // scattered bit
                o1 |= ld.MACHO_ARM_RELOC_SECTDIFF << (int)(24L);
                o1 |= 2L << (int)(28L); // size = 4

                var o2 = uint32(0L);
                o2 |= 1L << (int)(31L); // scattered bit
                o2 |= ld.MACHO_ARM_RELOC_PAIR << (int)(24L);
                o2 |= 2L << (int)(28L); // size = 4

                @out.Write32(o1);
                @out.Write32(uint32(ld.Symaddr(rs)));
                @out.Write32(o2);
                @out.Write32(uint32(s.Value + int64(r.Off)));
                return true;
            }
            if (rs.Type == sym.SHOSTOBJ || r.Type == objabi.R_CALLARM)
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
                v |= ld.MACHO_GENERIC_RELOC_VANILLA << (int)(28L);
            else if (r.Type == objabi.R_CALLARM) 
                v |= 1L << (int)(24L); // pc-relative bit
                v |= ld.MACHO_ARM_RELOC_BR24 << (int)(28L);
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
        private static void trampoline(ref ld.Link ctxt, ref sym.Reloc r, ref sym.Symbol s)
        {

            if (r.Type == objabi.R_CALLARM) 
                // r.Add is the instruction
                // low 24-bit encodes the target address
                var t = (ld.Symaddr(r.Sym) + int64(signext24(r.Add & 0xffffffUL) * 4L) - (s.Value + int64(r.Off))) / 4L;
                if (t > 0x7fffffUL || t < -0x800000UL || (ld.FlagDebugTramp > 1L && s.File != r.Sym.File.Value))
                { 
                    // direct call too far, need to insert trampoline.
                    // look up existing trampolines first. if we found one within the range
                    // of direct call, we can reuse it. otherwise create a new one.
                    var offset = (signext24(r.Add & 0xffffffUL) + 2L) * 4L;
                    ref sym.Symbol tramp = default;
                    for (long i = 0L; >>MARKER:FOREXPRESSION_LEVEL_1<<; i++)
                    {
                        var name = r.Sym.Name + fmt.Sprintf("%+d-tramp%d", offset, i);
                        tramp = ctxt.Syms.Lookup(name, int(r.Sym.Version));
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
                            gentrampdyn(ctxt.Arch, tramp, r.Sym, int64(offset));
                        }
                        else if (ctxt.BuildMode == ld.BuildModeCArchive || ctxt.BuildMode == ld.BuildModeCShared || ctxt.BuildMode == ld.BuildModePIE)
                        {
                            gentramppic(ctxt.Arch, tramp, r.Sym, int64(offset));
                        }
                        else
                        {
                            gentramp(ctxt.Arch, ctxt.LinkMode, tramp, r.Sym, int64(offset));
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
        private static void gentramp(ref sys.Arch arch, ld.LinkMode linkmode, ref sym.Symbol tramp, ref sym.Symbol target, long offset)
        {
            tramp.Size = 12L; // 3 instructions
            tramp.P = make_slice<byte>(tramp.Size);
            var t = ld.Symaddr(target) + int64(offset);
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
        private static void gentramppic(ref sys.Arch arch, ref sym.Symbol tramp, ref sym.Symbol target, long offset)
        {
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
        private static void gentrampdyn(ref sys.Arch arch, ref sym.Symbol tramp, ref sym.Symbol target, long offset)
        {
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
                o4 = uint32(0xe2800000UL | 11L << (int)(12L) | 11L << (int)(16L) | immrot(uint32(offset))); // ADD $offset, R11, R11
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

        private static bool archreloc(ref ld.Link ctxt, ref sym.Reloc r, ref sym.Symbol s, ref long val)
        {
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


                    if (rs.Type != sym.SHOSTOBJ && rs.Type != sym.SDYNIMPORT && rs.Sect == null)
                    {
                        ld.Errorf(s, "missing section for %s", rs.Name);
                    }
                    r.Xsym = rs; 

                    // ld64 for arm seems to want the symbol table to contain offset
                    // into the section rather than pseudo virtual address that contains
                    // the section load address.
                    // we need to compensate that by removing the instruction's address
                    // from addend.
                    if (ctxt.HeadType == objabi.Hdarwin)
                    {
                        r.Xadd -= ld.Symaddr(s) + int64(r.Off);
                    }
                    if (r.Xadd / 4L > 0x7fffffUL || r.Xadd / 4L < -0x800000UL)
                    {
                        ld.Errorf(s, "direct call too far %d", r.Xadd / 4L);
                    }
                    val.Value = int64(braddoff(int32(0xff000000UL & uint32(r.Add)), int32(0xffffffUL & uint32(r.Xadd / 4L))));
                    return true;
                                return false;
            }

            if (r.Type == objabi.R_CONST) 
                val.Value = r.Add;
                return true;
            else if (r.Type == objabi.R_GOTOFF) 
                val.Value = ld.Symaddr(r.Sym) + r.Add - ld.Symaddr(ctxt.Syms.Lookup(".got", 0L));
                return true; 

                // The following three arch specific relocations are only for generation of
                // Linux/ARM ELF's PLT entry (3 assembler instruction)
            else if (r.Type == objabi.R_PLT0) // add ip, pc, #0xXX00000
                if (ld.Symaddr(ctxt.Syms.Lookup(".got.plt", 0L)) < ld.Symaddr(ctxt.Syms.Lookup(".plt", 0L)))
                {
                    ld.Errorf(s, ".got.plt should be placed after .plt section.");
                }
                val.Value = 0xe28fc600UL + (0xffUL & (int64(uint32(ld.Symaddr(r.Sym) - (ld.Symaddr(ctxt.Syms.Lookup(".plt", 0L)) + int64(r.Off)) + r.Add)) >> (int)(20L)));
                return true;
            else if (r.Type == objabi.R_PLT1) // add ip, ip, #0xYY000
                val.Value = 0xe28cca00UL + (0xffUL & (int64(uint32(ld.Symaddr(r.Sym) - (ld.Symaddr(ctxt.Syms.Lookup(".plt", 0L)) + int64(r.Off)) + r.Add + 4L)) >> (int)(12L)));

                return true;
            else if (r.Type == objabi.R_PLT2) // ldr pc, [ip, #0xZZZ]!
                val.Value = 0xe5bcf000UL + (0xfffUL & int64(uint32(ld.Symaddr(r.Sym) - (ld.Symaddr(ctxt.Syms.Lookup(".plt", 0L)) + int64(r.Off)) + r.Add + 8L)));

                return true;
            else if (r.Type == objabi.R_CALLARM) // bl XXXXXX or b YYYYYY
                // r.Add is the instruction
                // low 24-bit encodes the target address
                var t = (ld.Symaddr(r.Sym) + int64(signext24(r.Add & 0xffffffUL) * 4L) - (s.Value + int64(r.Off))) / 4L;
                if (t > 0x7fffffUL || t < -0x800000UL)
                {
                    ld.Errorf(s, "direct call too far: %s %x", r.Sym.Name, t);
                }
                val.Value = int64(braddoff(int32(0xff000000UL & uint32(r.Add)), int32(0xffffffUL & t)));

                return true;
                        return false;
        }

        private static long archrelocvariant(ref ld.Link ctxt, ref sym.Reloc r, ref sym.Symbol s, long t)
        {
            log.Fatalf("unexpected relocation variant");
            return t;
        }

        private static ref sym.Reloc addpltreloc(ref ld.Link ctxt, ref sym.Symbol plt, ref sym.Symbol got, ref sym.Symbol s, objabi.RelocType typ)
        {
            var r = plt.AddRel();
            r.Sym = got;
            r.Off = int32(plt.Size);
            r.Siz = 4L;
            r.Type = typ;
            r.Add = int64(s.Got) - 8L;

            plt.Attr |= sym.AttrReachable;
            plt.Size += 4L;
            plt.Grow(plt.Size);

            return r;
        }

        private static void addpltsym(ref ld.Link ctxt, ref sym.Symbol s)
        {
            if (s.Plt >= 0L)
            {
                return;
            }
            ld.Adddynsym(ctxt, s);

            if (ctxt.IsELF)
            {
                var plt = ctxt.Syms.Lookup(".plt", 0L);
                var got = ctxt.Syms.Lookup(".got.plt", 0L);
                var rel = ctxt.Syms.Lookup(".rel.plt", 0L);
                if (plt.Size == 0L)
                {
                    elfsetupplt(ctxt);
                } 

                // .got entry
                s.Got = int32(got.Size); 

                // In theory, all GOT should point to the first PLT entry,
                // Linux/ARM's dynamic linker will do that for us, but FreeBSD/ARM's
                // dynamic linker won't, so we'd better do it ourselves.
                got.AddAddrPlus(ctxt.Arch, plt, 0L); 

                // .plt entry, this depends on the .got entry
                s.Plt = int32(plt.Size);

                addpltreloc(ctxt, plt, got, s, objabi.R_PLT0); // add lr, pc, #0xXX00000
                addpltreloc(ctxt, plt, got, s, objabi.R_PLT1); // add lr, lr, #0xYY000
                addpltreloc(ctxt, plt, got, s, objabi.R_PLT2); // ldr pc, [lr, #0xZZZ]!

                // rel
                rel.AddAddrPlus(ctxt.Arch, got, int64(s.Got));

                rel.AddUint32(ctxt.Arch, ld.ELF32_R_INFO(uint32(s.Dynid), uint32(elf.R_ARM_JUMP_SLOT)));
            }
            else
            {
                ld.Errorf(s, "addpltsym: unsupported binary format");
            }
        }

        private static void addgotsyminternal(ref ld.Link ctxt, ref sym.Symbol s)
        {
            if (s.Got >= 0L)
            {
                return;
            }
            var got = ctxt.Syms.Lookup(".got", 0L);
            s.Got = int32(got.Size);

            got.AddAddrPlus(ctxt.Arch, s, 0L);

            if (ctxt.IsELF)
            {
            }
            else
            {
                ld.Errorf(s, "addgotsyminternal: unsupported binary format");
            }
        }

        private static void addgotsym(ref ld.Link ctxt, ref sym.Symbol s)
        {
            if (s.Got >= 0L)
            {
                return;
            }
            ld.Adddynsym(ctxt, s);
            var got = ctxt.Syms.Lookup(".got", 0L);
            s.Got = int32(got.Size);
            got.AddUint32(ctxt.Arch, 0L);

            if (ctxt.IsELF)
            {
                var rel = ctxt.Syms.Lookup(".rel", 0L);
                rel.AddAddrPlus(ctxt.Arch, got, int64(s.Got));
                rel.AddUint32(ctxt.Arch, ld.ELF32_R_INFO(uint32(s.Dynid), uint32(elf.R_ARM_GLOB_DAT)));
            }
            else
            {
                ld.Errorf(s, "addgotsym: unsupported binary format");
            }
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
                ctxt.Out.Write32b(0x647UL); /* magic */
                ctxt.Out.Write32b(uint32(ld.Segtext.Filelen)); /* sizes */
                ctxt.Out.Write32b(uint32(ld.Segdata.Filelen));
                ctxt.Out.Write32b(uint32(ld.Segdata.Length - ld.Segdata.Filelen));
                ctxt.Out.Write32b(uint32(ld.Symsize)); /* nsyms */
                ctxt.Out.Write32b(uint32(ld.Entryvalue(ctxt))); /* va of entry */
                ctxt.Out.Write32b(0L);
                ctxt.Out.Write32b(uint32(ld.Lcsize));
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
