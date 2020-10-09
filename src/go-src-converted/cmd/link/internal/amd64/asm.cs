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

// package amd64 -- go2cs converted at 2020 October 09 05:48:14 UTC
// import "cmd/link/internal/amd64" ==> using amd64 = go.cmd.link.@internal.amd64_package
// Original source: C:\Go\src\cmd\link\internal\amd64\asm.go
using objabi = go.cmd.@internal.objabi_package;
using sys = go.cmd.@internal.sys_package;
using ld = go.cmd.link.@internal.ld_package;
using loader = go.cmd.link.@internal.loader_package;
using sym = go.cmd.link.@internal.sym_package;
using elf = go.debug.elf_package;
using log = go.log_package;
using sync = go.sync_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace link {
namespace @internal
{
    public static partial class amd64_package
    {
        public static uint PADDR(uint x)
        {
            return x & ~0x80000000UL;
        }

        private static void gentext2(ptr<ld.Link> _addr_ctxt, ptr<loader.Loader> _addr_ldr)
        {
            ref ld.Link ctxt = ref _addr_ctxt.val;
            ref loader.Loader ldr = ref _addr_ldr.val;

            var (initfunc, addmoduledata) = ld.PrepareAddmoduledata(ctxt);
            if (initfunc == null)
            {
                return ;
            }

            Action<byte[]> o = op =>
            {
                foreach (var (_, op1) in op)
                {
                    initfunc.AddUint8(op1);
                }

            } 

            // 0000000000000000 <local.dso_init>:
            //    0:    48 8d 3d 00 00 00 00     lea    0x0(%rip),%rdi        # 7 <local.dso_init+0x7>
            //             3: R_X86_64_PC32    runtime.firstmoduledata-0x4
; 

            // 0000000000000000 <local.dso_init>:
            //    0:    48 8d 3d 00 00 00 00     lea    0x0(%rip),%rdi        # 7 <local.dso_init+0x7>
            //             3: R_X86_64_PC32    runtime.firstmoduledata-0x4
            o(0x48UL, 0x8dUL, 0x3dUL);
            initfunc.AddPCRelPlus(ctxt.Arch, ctxt.Moduledata2, 0L); 
            //    7:    e8 00 00 00 00           callq  c <local.dso_init+0xc>
            //             8: R_X86_64_PLT32    runtime.addmoduledata-0x4
            o(0xe8UL);
            initfunc.AddSymRef(ctxt.Arch, addmoduledata, 0L, objabi.R_CALL, 4L); 
            //    c:    c3                       retq
            o(0xc3UL);

        }

        // makeWritable makes a readonly symbol writable if we do opcode rewriting.
        private static void makeWritable(ptr<sym.Symbol> _addr_s)
        {
            ref sym.Symbol s = ref _addr_s.val;

            if (s.Attr.ReadOnly())
            {
                s.Attr.Set(sym.AttrReadOnly, false);
                s.P = append((slice<byte>)null, s.P);
            }

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


            if (r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_X86_64_PC32))
            {
                if (targType == sym.SDYNIMPORT)
                {
                    ldr.Errorf(s, "unexpected R_X86_64_PC32 relocation for dynamic symbol %s", ldr.SymName(targ));
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
                goto __switch_break0;
            }
            if (r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_X86_64_PC64))
            {
                if (targType == sym.SDYNIMPORT)
                {
                    ldr.Errorf(s, "unexpected R_X86_64_PC64 relocation for dynamic symbol %s", ldr.SymName(targ));
                }

                if (targType == 0L || targType == sym.SXREF)
                {
                    ldr.Errorf(s, "unknown symbol %s in pcrel", ldr.SymName(targ));
                }

                su = ldr.MakeSymbolUpdater(s);
                su.SetRelocType(rIdx, objabi.R_PCREL);
                su.SetRelocAdd(rIdx, r.Add() + 8L);
                return true;
                goto __switch_break0;
            }
            if (r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_X86_64_PLT32))
            {
                su = ldr.MakeSymbolUpdater(s);
                su.SetRelocType(rIdx, objabi.R_PCREL);
                su.SetRelocAdd(rIdx, r.Add() + 4L);
                if (targType == sym.SDYNIMPORT)
                {
                    addpltsym2(_addr_target, _addr_ldr, _addr_syms, targ);
                    su.SetRelocSym(rIdx, syms.PLT2);
                    su.SetRelocAdd(rIdx, r.Add() + int64(ldr.SymPlt(targ)));
                }

                return true;
                goto __switch_break0;
            }
            if (r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_X86_64_GOTPCREL) || r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_X86_64_GOTPCRELX) || r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_X86_64_REX_GOTPCRELX))
            {
                su = ldr.MakeSymbolUpdater(s);
                if (targType != sym.SDYNIMPORT)
                { 
                    // have symbol
                    var sData = ldr.Data(s);
                    if (r.Off() >= 2L && sData[r.Off() - 2L] == 0x8bUL)
                    {
                        su.MakeWritable(); 
                        // turn MOVQ of GOT entry into LEAQ of symbol itself
                        var writeableData = su.Data();
                        writeableData[r.Off() - 2L] = 0x8dUL;
                        su.SetRelocType(rIdx, objabi.R_PCREL);
                        su.SetRelocAdd(rIdx, r.Add() + 4L);
                        return true;

                    }

                } 

                // fall back to using GOT and hope for the best (CMOV*)
                // TODO: just needs relocation, no need to put in .dynsym
                addgotsym2(_addr_target, _addr_ldr, _addr_syms, targ);

                su.SetRelocType(rIdx, objabi.R_PCREL);
                su.SetRelocSym(rIdx, syms.GOT2);
                su.SetRelocAdd(rIdx, r.Add() + 4L + int64(ldr.SymGot(targ)));
                return true;
                goto __switch_break0;
            }
            if (r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_X86_64_64))
            {
                if (targType == sym.SDYNIMPORT)
                {
                    ldr.Errorf(s, "unexpected R_X86_64_64 relocation for dynamic symbol %s", ldr.SymName(targ));
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

                // Handle relocations found in Mach-O object files.
                goto __switch_break0;
            }
            if (r.Type() == objabi.MachoRelocOffset + ld.MACHO_X86_64_RELOC_UNSIGNED * 2L + 0L || r.Type() == objabi.MachoRelocOffset + ld.MACHO_X86_64_RELOC_SIGNED * 2L + 0L || r.Type() == objabi.MachoRelocOffset + ld.MACHO_X86_64_RELOC_BRANCH * 2L + 0L) 
            {
                // TODO: What is the difference between all these?
                su = ldr.MakeSymbolUpdater(s);
                su.SetRelocType(rIdx, objabi.R_ADDR);

                if (targType == sym.SDYNIMPORT)
                {
                    ldr.Errorf(s, "unexpected reloc for dynamic symbol %s", ldr.SymName(targ));
                }

                return true;
                goto __switch_break0;
            }
            if (r.Type() == objabi.MachoRelocOffset + ld.MACHO_X86_64_RELOC_BRANCH * 2L + 1L)
            {
                if (targType == sym.SDYNIMPORT)
                {
                    addpltsym2(_addr_target, _addr_ldr, _addr_syms, targ);
                    su = ldr.MakeSymbolUpdater(s);
                    su.SetRelocSym(rIdx, syms.PLT2);
                    su.SetRelocType(rIdx, objabi.R_PCREL);
                    su.SetRelocAdd(rIdx, int64(ldr.SymPlt(targ)));
                    return true;
                }

                fallthrough = true;

            }
            if (fallthrough || r.Type() == objabi.MachoRelocOffset + ld.MACHO_X86_64_RELOC_UNSIGNED * 2L + 1L || r.Type() == objabi.MachoRelocOffset + ld.MACHO_X86_64_RELOC_SIGNED * 2L + 1L || r.Type() == objabi.MachoRelocOffset + ld.MACHO_X86_64_RELOC_SIGNED_1 * 2L + 1L || r.Type() == objabi.MachoRelocOffset + ld.MACHO_X86_64_RELOC_SIGNED_2 * 2L + 1L || r.Type() == objabi.MachoRelocOffset + ld.MACHO_X86_64_RELOC_SIGNED_4 * 2L + 1L)
            {
                su = ldr.MakeSymbolUpdater(s);
                su.SetRelocType(rIdx, objabi.R_PCREL);

                if (targType == sym.SDYNIMPORT)
                {
                    ldr.Errorf(s, "unexpected pc-relative reloc for dynamic symbol %s", ldr.SymName(targ));
                }

                return true;
                goto __switch_break0;
            }
            if (r.Type() == objabi.MachoRelocOffset + ld.MACHO_X86_64_RELOC_GOT_LOAD * 2L + 1L)
            {
                if (targType != sym.SDYNIMPORT)
                { 
                    // have symbol
                    // turn MOVQ of GOT entry into LEAQ of symbol itself
                    var sdata = ldr.Data(s);
                    if (r.Off() < 2L || sdata[r.Off() - 2L] != 0x8bUL)
                    {
                        ldr.Errorf(s, "unexpected GOT_LOAD reloc for non-dynamic symbol %s", ldr.SymName(targ));
                        return false;
                    }

                    su = ldr.MakeSymbolUpdater(s);
                    su.MakeWritable();
                    sdata = su.Data();
                    sdata[r.Off() - 2L] = 0x8dUL;
                    su.SetRelocType(rIdx, objabi.R_PCREL);
                    return true;

                }

                fallthrough = true;

            }
            if (fallthrough || r.Type() == objabi.MachoRelocOffset + ld.MACHO_X86_64_RELOC_GOT * 2L + 1L)
            {
                if (targType != sym.SDYNIMPORT)
                {
                    ldr.Errorf(s, "unexpected GOT reloc for non-dynamic symbol %s", ldr.SymName(targ));
                }

                addgotsym2(_addr_target, _addr_ldr, _addr_syms, targ);
                su = ldr.MakeSymbolUpdater(s);
                su.SetRelocType(rIdx, objabi.R_PCREL);
                su.SetRelocSym(rIdx, syms.GOT2);
                su.SetRelocAdd(rIdx, r.Add() + int64(ldr.SymGot(targ)));
                return true;
                goto __switch_break0;
            }
            // default: 
                if (r.Type() >= objabi.ElfRelocOffset)
                {
                    ldr.Errorf(s, "unexpected relocation type %d (%s)", r.Type(), sym.RelocName(target.Arch, r.Type()));
                    return false;
                } 

                // Handle relocations found in ELF object files.

            __switch_break0:; 

            // Reread the reloc to incorporate any changes in type above.
            var relocs = ldr.Relocs(s);
            r = relocs.At2(rIdx);


            if (r.Type() == objabi.R_CALL || r.Type() == objabi.R_PCREL) 
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
                // Internal linking, for both ELF and Mach-O.
                // Build a PLT entry and change the relocation target to that entry.
                addpltsym2(_addr_target, _addr_ldr, _addr_syms, targ);
                su = ldr.MakeSymbolUpdater(s);
                su.SetRelocSym(rIdx, syms.PLT2);
                su.SetRelocAdd(rIdx, int64(ldr.SymPlt(targ)));
                return true;
            else if (r.Type() == objabi.R_ADDR) 
                if (ldr.SymType(s) == sym.STEXT && target.IsElf())
                {
                    su = ldr.MakeSymbolUpdater(s);
                    if (target.IsSolaris())
                    {
                        addpltsym2(_addr_target, _addr_ldr, _addr_syms, targ);
                        su.SetRelocSym(rIdx, syms.PLT2);
                        su.SetRelocAdd(rIdx, r.Add() + int64(ldr.SymPlt(targ)));
                        return true;
                    } 
                    // The code is asking for the address of an external
                    // function. We provide it with the address of the
                    // correspondent GOT symbol.
                    addgotsym2(_addr_target, _addr_ldr, _addr_syms, targ);

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
                    var rela = ldr.MakeSymbolUpdater(syms.Rela2);
                    rela.AddAddrPlus(target.Arch, s, int64(r.Off()));
                    if (r.Siz() == 8L)
                    {
                        rela.AddUint64(target.Arch, ld.ELF64_R_INFO(0L, uint32(elf.R_X86_64_RELATIVE)));
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

                if (target.IsDarwin() && ldr.SymSize(s) == int64(target.Arch.PtrSize) && r.Off() == 0L)
                { 
                    // Mach-O relocations are a royal pain to lay out.
                    // They use a compact stateful bytecode representation
                    // that is too much bother to deal with.
                    // Instead, interpret the C declaration
                    //    void *_Cvar_stderr = &stderr;
                    // as making _Cvar_stderr the name of a GOT entry
                    // for stderr. This is separate from the usual GOT entry,
                    // just in case the C code assigns to the variable,
                    // and of course it only works for single pointers,
                    // but we only need to support cgo and that's all it needs.
                    ld.Adddynsym2(ldr, target, syms, targ);

                    var got = ldr.MakeSymbolUpdater(syms.GOT2);
                    su = ldr.MakeSymbolUpdater(s);
                    su.SetType(got.Type());
                    got.PrependSub(s);
                    su.SetValue(got.Size());
                    got.AddUint64(target.Arch, 0L);
                    var leg = ldr.MakeSymbolUpdater(syms.LinkEditGOT2);
                    leg.AddUint32(target.Arch, uint32(ldr.SymDynid(targ)));
                    su.SetRelocType(rIdx, objabi.ElfRelocOffset); // ignore during relocsym
                    return true;

                }

                        return false;

        }

        private static bool elfreloc2(ptr<ld.Link> _addr_ctxt, ptr<loader.Loader> _addr_ldr, loader.Sym s, loader.ExtRelocView r, long sectoff)
        {
            ref ld.Link ctxt = ref _addr_ctxt.val;
            ref loader.Loader ldr = ref _addr_ldr.val;

            ctxt.Out.Write64(uint64(sectoff));

            var xsym = ldr.Syms[r.Xsym];
            var elfsym = ld.ElfSymForReloc(ctxt, xsym);
            var siz = r.Siz();

            if (r.Type() == objabi.R_ADDR || r.Type() == objabi.R_DWARFSECREF) 
                if (siz == 4L)
                {
                    ctxt.Out.Write64(uint64(elf.R_X86_64_32) | uint64(elfsym) << (int)(32L));
                }
                else if (siz == 8L)
                {
                    ctxt.Out.Write64(uint64(elf.R_X86_64_64) | uint64(elfsym) << (int)(32L));
                }
                else
                {
                    return false;
                }

            else if (r.Type() == objabi.R_TLS_LE) 
                if (siz == 4L)
                {
                    ctxt.Out.Write64(uint64(elf.R_X86_64_TPOFF32) | uint64(elfsym) << (int)(32L));
                }
                else
                {
                    return false;
                }

            else if (r.Type() == objabi.R_TLS_IE) 
                if (siz == 4L)
                {
                    ctxt.Out.Write64(uint64(elf.R_X86_64_GOTTPOFF) | uint64(elfsym) << (int)(32L));
                }
                else
                {
                    return false;
                }

            else if (r.Type() == objabi.R_CALL) 
                if (siz == 4L)
                {
                    if (xsym.Type == sym.SDYNIMPORT)
                    {
                        if (ctxt.DynlinkingGo())
                        {
                            ctxt.Out.Write64(uint64(elf.R_X86_64_PLT32) | uint64(elfsym) << (int)(32L));
                        }
                        else
                        {
                            ctxt.Out.Write64(uint64(elf.R_X86_64_GOTPCREL) | uint64(elfsym) << (int)(32L));
                        }

                    }
                    else
                    {
                        ctxt.Out.Write64(uint64(elf.R_X86_64_PC32) | uint64(elfsym) << (int)(32L));
                    }

                }
                else
                {
                    return false;
                }

            else if (r.Type() == objabi.R_PCREL) 
                if (siz == 4L)
                {
                    if (xsym.Type == sym.SDYNIMPORT && xsym.ElfType() == elf.STT_FUNC)
                    {
                        ctxt.Out.Write64(uint64(elf.R_X86_64_PLT32) | uint64(elfsym) << (int)(32L));
                    }
                    else
                    {
                        ctxt.Out.Write64(uint64(elf.R_X86_64_PC32) | uint64(elfsym) << (int)(32L));
                    }

                }
                else
                {
                    return false;
                }

            else if (r.Type() == objabi.R_GOTPCREL) 
                if (siz == 4L)
                {
                    ctxt.Out.Write64(uint64(elf.R_X86_64_GOTPCREL) | uint64(elfsym) << (int)(32L));
                }
                else
                {
                    return false;
                }

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

            if (rs.Type == sym.SHOSTOBJ || r.Type == objabi.R_PCREL || r.Type == objabi.R_GOTPCREL || r.Type == objabi.R_CALL)
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
                v |= ld.MACHO_X86_64_RELOC_UNSIGNED << (int)(28L);
            else if (r.Type == objabi.R_CALL) 
                v |= 1L << (int)(24L); // pc-relative bit
                v |= ld.MACHO_X86_64_RELOC_BRANCH << (int)(28L); 

                // NOTE: Only works with 'external' relocation. Forced above.
            else if (r.Type == objabi.R_PCREL) 
                v |= 1L << (int)(24L); // pc-relative bit
                v |= ld.MACHO_X86_64_RELOC_SIGNED << (int)(28L);
            else if (r.Type == objabi.R_GOTPCREL) 
                v |= 1L << (int)(24L); // pc-relative bit
                v |= ld.MACHO_X86_64_RELOC_GOT_LOAD << (int)(28L);
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

        private static bool pereloc1(ptr<sys.Arch> _addr_arch, ptr<ld.OutBuf> _addr_@out, ptr<sym.Symbol> _addr_s, ptr<sym.Reloc> _addr_r, long sectoff)
        {
            ref sys.Arch arch = ref _addr_arch.val;
            ref ld.OutBuf @out = ref _addr_@out.val;
            ref sym.Symbol s = ref _addr_s.val;
            ref sym.Reloc r = ref _addr_r.val;

            uint v = default;

            var rs = r.Xsym;

            if (rs.Dynid < 0L)
            {
                ld.Errorf(s, "reloc %d (%s) to non-coff symbol %s type=%d (%s)", r.Type, sym.RelocName(arch, r.Type), rs.Name, rs.Type, rs.Type);
                return false;
            }

            @out.Write32(uint32(sectoff));
            @out.Write32(uint32(rs.Dynid));


            if (r.Type == objabi.R_DWARFSECREF) 
                v = ld.IMAGE_REL_AMD64_SECREL;
            else if (r.Type == objabi.R_ADDR) 
                if (r.Siz == 8L)
                {
                    v = ld.IMAGE_REL_AMD64_ADDR64;
                }
                else
                {
                    v = ld.IMAGE_REL_AMD64_ADDR32;
                }

            else if (r.Type == objabi.R_CALL || r.Type == objabi.R_PCREL) 
                v = ld.IMAGE_REL_AMD64_REL32;
            else 
                return false;
                        @out.Write16(uint16(v));

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

        private static void elfsetupplt(ptr<ld.Link> _addr_ctxt, ptr<loader.SymbolBuilder> _addr_plt, ptr<loader.SymbolBuilder> _addr_got, loader.Sym dynamic)
        {
            ref ld.Link ctxt = ref _addr_ctxt.val;
            ref loader.SymbolBuilder plt = ref _addr_plt.val;
            ref loader.SymbolBuilder got = ref _addr_got.val;

            if (plt.Size() == 0L)
            { 
                // pushq got+8(IP)
                plt.AddUint8(0xffUL);

                plt.AddUint8(0x35UL);
                plt.AddPCRelPlus(ctxt.Arch, got.Sym(), 8L); 

                // jmpq got+16(IP)
                plt.AddUint8(0xffUL);

                plt.AddUint8(0x25UL);
                plt.AddPCRelPlus(ctxt.Arch, got.Sym(), 16L); 

                // nopl 0(AX)
                plt.AddUint32(ctxt.Arch, 0x00401f0fUL); 

                // assume got->size == 0 too
                got.AddAddrPlus(ctxt.Arch, dynamic, 0L);

                got.AddUint64(ctxt.Arch, 0L);
                got.AddUint64(ctxt.Arch, 0L);

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
                var got = ldr.MakeSymbolUpdater(syms.GOTPLT2);
                var rela = ldr.MakeSymbolUpdater(syms.RelaPLT2);
                if (plt.Size() == 0L)
                {
                    panic("plt is not set up");
                } 

                // jmpq *got+size(IP)
                plt.AddUint8(0xffUL);

                plt.AddUint8(0x25UL);
                plt.AddPCRelPlus(target.Arch, got.Sym(), got.Size()); 

                // add to got: pointer to current pos in plt
                got.AddAddrPlus(target.Arch, plt.Sym(), plt.Size()); 

                // pushq $x
                plt.AddUint8(0x68UL);

                plt.AddUint32(target.Arch, uint32((got.Size() - 24L - 8L) / 8L)); 

                // jmpq .plt
                plt.AddUint8(0xe9UL);

                plt.AddUint32(target.Arch, uint32(-(plt.Size() + 4L))); 

                // rela
                rela.AddAddrPlus(target.Arch, got.Sym(), got.Size() - 8L);

                var sDynid = ldr.SymDynid(s);
                rela.AddUint64(target.Arch, ld.ELF64_R_INFO(uint32(sDynid), uint32(elf.R_X86_64_JMP_SLOT)));
                rela.AddUint64(target.Arch, 0L);

                ldr.SetPlt(s, int32(plt.Size() - 16L));

            }
            else if (target.IsDarwin())
            { 
                // To do lazy symbol lookup right, we're supposed
                // to tell the dynamic loader which library each
                // symbol comes from and format the link info
                // section just so. I'm too lazy (ha!) to do that
                // so for now we'll just use non-lazy pointers,
                // which don't need to be told which library to use.
                //
                // https://networkpx.blogspot.com/2009/09/about-lcdyldinfoonly-command.html
                // has details about what we're avoiding.

                addgotsym2(_addr_target, _addr_ldr, _addr_syms, s);
                plt = ldr.MakeSymbolUpdater(syms.PLT2);

                sDynid = ldr.SymDynid(s);
                var lep = ldr.MakeSymbolUpdater(syms.LinkEditPLT2);
                lep.AddUint32(target.Arch, uint32(sDynid)); 

                // jmpq *got+size(IP)
                ldr.SetPlt(s, int32(plt.Size()));

                plt.AddUint8(0xffUL);
                plt.AddUint8(0x25UL);
                plt.AddPCRelPlus(target.Arch, syms.GOT2, int64(ldr.SymGot(s)));

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
                rela.AddUint64(target.Arch, ld.ELF64_R_INFO(uint32(ldr.SymDynid(s)), uint32(elf.R_X86_64_GLOB_DAT)));
                rela.AddUint64(target.Arch, 0L);
            }
            else if (target.IsDarwin())
            {
                var leg = ldr.MakeSymbolUpdater(syms.LinkEditGOT2);
                leg.AddUint32(target.Arch, uint32(ldr.SymDynid(s)));
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
            Action<ptr<ld.Link>, ptr<ld.OutBuf>, long, long> f = (ctxt, @out, start, length) =>
            { 
                // 0xCC is INT $3 - breakpoint instruction
                ld.CodeblkPad(ctxt, out, start, length, new slice<byte>(new byte[] { 0xCC }));

            }
;
            ld.WriteParallel(_addr_wg, f, ctxt, offset, sect.Vaddr, sect.Length);

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

            var machlink = int64(0L);
            if (ctxt.HeadType == objabi.Hdarwin)
            {
                machlink = ld.Domacholink(ctxt);
            }


            if (ctxt.HeadType == objabi.Hplan9) 
                break;
            else if (ctxt.HeadType == objabi.Hdarwin) 
                ld.Flag8 = true;                /* 64-bit addresses */
            else if (ctxt.HeadType == objabi.Hlinux || ctxt.HeadType == objabi.Hfreebsd || ctxt.HeadType == objabi.Hnetbsd || ctxt.HeadType == objabi.Hopenbsd || ctxt.HeadType == objabi.Hdragonfly || ctxt.HeadType == objabi.Hsolaris) 
                ld.Flag8 = true;                /* 64-bit addresses */
            else if (ctxt.HeadType == objabi.Hwindows) 
                break;
            else 
                ld.Errorf(null, "unknown header type %v", ctxt.HeadType);
                        ld.Symsize = 0L;
            ld.Spsize = 0L;
            ld.Lcsize = 0L;
            var symo = int64(0L);
            if (!ld.FlagS.val)
            {

                if (ctxt.HeadType == objabi.Hplan9) 
                    ld.FlagS.val = true;
                    symo = int64(ld.Segdata.Fileoff + ld.Segdata.Filelen);
                else if (ctxt.HeadType == objabi.Hdarwin) 
                    symo = int64(ld.Segdwarf.Fileoff + uint64(ld.Rnd(int64(ld.Segdwarf.Filelen), int64(ld.FlagRound.val))) + uint64(machlink));
                else if (ctxt.HeadType == objabi.Hlinux || ctxt.HeadType == objabi.Hfreebsd || ctxt.HeadType == objabi.Hnetbsd || ctxt.HeadType == objabi.Hopenbsd || ctxt.HeadType == objabi.Hdragonfly || ctxt.HeadType == objabi.Hsolaris) 
                    symo = int64(ld.Segdwarf.Fileoff + ld.Segdwarf.Filelen);
                    symo = ld.Rnd(symo, int64(ld.FlagRound.val));
                else if (ctxt.HeadType == objabi.Hwindows) 
                    symo = int64(ld.Segdwarf.Fileoff + ld.Segdwarf.Filelen);
                    symo = ld.Rnd(symo, ld.PEFILEALIGN);
                else                                 ctxt.Out.SeekSet(symo);

                if (ctxt.HeadType == objabi.Hplan9) 
                    ld.Asmplan9sym(ctxt);

                    var sym = ctxt.Syms.Lookup("pclntab", 0L);
                    if (sym != null)
                    {
                        ld.Lcsize = int32(len(sym.P));
                        ctxt.Out.Write(sym.P);
                    }

                else if (ctxt.HeadType == objabi.Hwindows)                 else if (ctxt.HeadType == objabi.Hdarwin) 
                    if (ctxt.LinkMode == ld.LinkExternal)
                    {
                        ld.Machoemitreloc(ctxt);
                    }

                else 
                    if (ctxt.IsELF)
                    {
                        ctxt.Out.SeekSet(symo);
                        ld.Asmelfsym(ctxt);
                        ctxt.Out.Write(ld.Elfstrdat);

                        if (ctxt.LinkMode == ld.LinkExternal)
                        {
                            ld.Elfemitreloc(ctxt);
                        }

                    }

                            }

            ctxt.Out.SeekSet(0L);

            if (ctxt.HeadType == objabi.Hplan9) /* plan9 */
                var magic = int32(4L * 26L * 26L + 7L);

                magic |= 0x00008000UL; /* fat header */
                ctxt.Out.Write32b(uint32(magic)); /* magic */
                ctxt.Out.Write32b(uint32(ld.Segtext.Filelen)); /* sizes */
                ctxt.Out.Write32b(uint32(ld.Segdata.Filelen));
                ctxt.Out.Write32b(uint32(ld.Segdata.Length - ld.Segdata.Filelen));
                ctxt.Out.Write32b(uint32(ld.Symsize)); /* nsyms */
                var vl = ld.Entryvalue(ctxt);
                ctxt.Out.Write32b(PADDR(uint32(vl))); /* va of entry */
                ctxt.Out.Write32b(uint32(ld.Spsize)); /* sp offsets */
                ctxt.Out.Write32b(uint32(ld.Lcsize)); /* line offsets */
                ctxt.Out.Write64b(uint64(vl));                /* va of entry */
            else if (ctxt.HeadType == objabi.Hdarwin) 
                ld.Asmbmacho(ctxt);
            else if (ctxt.HeadType == objabi.Hlinux || ctxt.HeadType == objabi.Hfreebsd || ctxt.HeadType == objabi.Hnetbsd || ctxt.HeadType == objabi.Hopenbsd || ctxt.HeadType == objabi.Hdragonfly || ctxt.HeadType == objabi.Hsolaris) 
                ld.Asmbelf(ctxt, symo);
            else if (ctxt.HeadType == objabi.Hwindows) 
                ld.Asmbpe(ctxt);
            else             
        }

        private static void tlsIEtoLE(slice<byte> P, long off, long size)
        { 
            // Transform the PC-relative instruction into a constant load.
            // That is,
            //
            //    MOVQ X(IP), REG  ->  MOVQ $Y, REG
            //
            // To determine the instruction and register, we study the op codes.
            // Consult an AMD64 instruction encoding guide to decipher this.
            if (off < 3L)
            {
                log.Fatal("R_X86_64_GOTTPOFF reloc not preceded by MOVQ or ADDQ instruction");
            }

            var op = P[off - 3L..off];
            var reg = op[2L] >> (int)(3L);

            if (op[1L] == 0x8bUL || reg == 4L)
            { 
                // MOVQ
                if (op[0L] == 0x4cUL)
                {
                    op[0L] = 0x49UL;
                }
                else if (size == 4L && op[0L] == 0x44UL)
                {
                    op[0L] = 0x41UL;
                }

                if (op[1L] == 0x8bUL)
                {
                    op[1L] = 0xc7UL;
                }
                else
                {
                    op[1L] = 0x81UL; // special case for SP
                }

                op[2L] = 0xc0UL | reg;

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
    }
}}}}
