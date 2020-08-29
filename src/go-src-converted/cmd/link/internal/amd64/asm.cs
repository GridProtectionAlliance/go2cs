// Inferno utils/6l/asm.c
// https://bitbucket.org/inferno-os/inferno-os/src/default/utils/6l/asm.c
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

// package amd64 -- go2cs converted at 2020 August 29 10:02:18 UTC
// import "cmd/link/internal/amd64" ==> using amd64 = go.cmd.link.@internal.amd64_package
// Original source: C:\Go\src\cmd\link\internal\amd64\asm.go
using objabi = go.cmd.@internal.objabi_package;
using sys = go.cmd.@internal.sys_package;
using ld = go.cmd.link.@internal.ld_package;
using sym = go.cmd.link.@internal.sym_package;
using elf = go.debug.elf_package;
using log = go.log_package;
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

        public static long Addcall(ref ld.Link ctxt, ref sym.Symbol s, ref sym.Symbol t)
        {
            s.Attr |= sym.AttrReachable;
            var i = s.Size;
            s.Size += 4L;
            s.Grow(s.Size);
            var r = s.AddRel();
            r.Sym = t;
            r.Off = int32(i);
            r.Type = objabi.R_CALL;
            r.Siz = 4L;
            return i + int64(r.Siz);
        }

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
            initfunc.AddPCRelPlus(ctxt.Arch, ctxt.Moduledata, 0L); 
            //    7:    e8 00 00 00 00           callq  c <local.dso_init+0xc>
            //             8: R_X86_64_PLT32    runtime.addmoduledata-0x4
            o(0xe8UL);
            Addcall(ctxt, initfunc, addmoduledata); 
            //    c:    c3                       retq
            o(0xc3UL);
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

        private static bool adddynrel(ref ld.Link ctxt, ref sym.Symbol s, ref sym.Reloc r)
        {
            var targ = r.Sym;


            if (r.Type == 256L + objabi.RelocType(elf.R_X86_64_PC32))
            {
                if (targ.Type == sym.SDYNIMPORT)
                {
                    ld.Errorf(s, "unexpected R_X86_64_PC32 relocation for dynamic symbol %s", targ.Name);
                } 
                // TODO(mwhudson): the test of VisibilityHidden here probably doesn't make
                // sense and should be removed when someone has thought about it properly.
                if ((targ.Type == 0L || targ.Type == sym.SXREF) && !targ.Attr.VisibilityHidden())
                {
                    ld.Errorf(s, "unknown symbol %s in pcrel", targ.Name);
                }
                r.Type = objabi.R_PCREL;
                r.Add += 4L;
                return true;
                goto __switch_break0;
            }
            if (r.Type == 256L + objabi.RelocType(elf.R_X86_64_PC64))
            {
                if (targ.Type == sym.SDYNIMPORT)
                {
                    ld.Errorf(s, "unexpected R_X86_64_PC64 relocation for dynamic symbol %s", targ.Name);
                }
                if (targ.Type == 0L || targ.Type == sym.SXREF)
                {
                    ld.Errorf(s, "unknown symbol %s in pcrel", targ.Name);
                }
                r.Type = objabi.R_PCREL;
                r.Add += 8L;
                return true;
                goto __switch_break0;
            }
            if (r.Type == 256L + objabi.RelocType(elf.R_X86_64_PLT32))
            {
                r.Type = objabi.R_PCREL;
                r.Add += 4L;
                if (targ.Type == sym.SDYNIMPORT)
                {
                    addpltsym(ctxt, targ);
                    r.Sym = ctxt.Syms.Lookup(".plt", 0L);
                    r.Add += int64(targ.Plt);
                }
                return true;
                goto __switch_break0;
            }
            if (r.Type == 256L + objabi.RelocType(elf.R_X86_64_GOTPCREL) || r.Type == 256L + objabi.RelocType(elf.R_X86_64_GOTPCRELX) || r.Type == 256L + objabi.RelocType(elf.R_X86_64_REX_GOTPCRELX))
            {
                if (targ.Type != sym.SDYNIMPORT)
                { 
                    // have symbol
                    if (r.Off >= 2L && s.P[r.Off - 2L] == 0x8bUL)
                    { 
                        // turn MOVQ of GOT entry into LEAQ of symbol itself
                        s.P[r.Off - 2L] = 0x8dUL;

                        r.Type = objabi.R_PCREL;
                        r.Add += 4L;
                        return true;
                    }
                } 

                // fall back to using GOT and hope for the best (CMOV*)
                // TODO: just needs relocation, no need to put in .dynsym
                addgotsym(ctxt, targ);

                r.Type = objabi.R_PCREL;
                r.Sym = ctxt.Syms.Lookup(".got", 0L);
                r.Add += 4L;
                r.Add += int64(targ.Got);
                return true;
                goto __switch_break0;
            }
            if (r.Type == 256L + objabi.RelocType(elf.R_X86_64_64))
            {
                if (targ.Type == sym.SDYNIMPORT)
                {
                    ld.Errorf(s, "unexpected R_X86_64_64 relocation for dynamic symbol %s", targ.Name);
                }
                r.Type = objabi.R_ADDR;
                return true; 

                // Handle relocations found in Mach-O object files.
                goto __switch_break0;
            }
            if (r.Type == 512L + ld.MACHO_X86_64_RELOC_UNSIGNED * 2L + 0L || r.Type == 512L + ld.MACHO_X86_64_RELOC_SIGNED * 2L + 0L || r.Type == 512L + ld.MACHO_X86_64_RELOC_BRANCH * 2L + 0L) 
            {
                // TODO: What is the difference between all these?
                r.Type = objabi.R_ADDR;

                if (targ.Type == sym.SDYNIMPORT)
                {
                    ld.Errorf(s, "unexpected reloc for dynamic symbol %s", targ.Name);
                }
                return true;
                goto __switch_break0;
            }
            if (r.Type == 512L + ld.MACHO_X86_64_RELOC_BRANCH * 2L + 1L)
            {
                if (targ.Type == sym.SDYNIMPORT)
                {
                    addpltsym(ctxt, targ);
                    r.Sym = ctxt.Syms.Lookup(".plt", 0L);
                    r.Add = int64(targ.Plt);
                    r.Type = objabi.R_PCREL;
                    return true;
                }
                fallthrough = true; 

                // fall through
            }
            if (fallthrough || r.Type == 512L + ld.MACHO_X86_64_RELOC_UNSIGNED * 2L + 1L || r.Type == 512L + ld.MACHO_X86_64_RELOC_SIGNED * 2L + 1L || r.Type == 512L + ld.MACHO_X86_64_RELOC_SIGNED_1 * 2L + 1L || r.Type == 512L + ld.MACHO_X86_64_RELOC_SIGNED_2 * 2L + 1L || r.Type == 512L + ld.MACHO_X86_64_RELOC_SIGNED_4 * 2L + 1L)
            {
                r.Type = objabi.R_PCREL;

                if (targ.Type == sym.SDYNIMPORT)
                {
                    ld.Errorf(s, "unexpected pc-relative reloc for dynamic symbol %s", targ.Name);
                }
                return true;
                goto __switch_break0;
            }
            if (r.Type == 512L + ld.MACHO_X86_64_RELOC_GOT_LOAD * 2L + 1L)
            {
                if (targ.Type != sym.SDYNIMPORT)
                { 
                    // have symbol
                    // turn MOVQ of GOT entry into LEAQ of symbol itself
                    if (r.Off < 2L || s.P[r.Off - 2L] != 0x8bUL)
                    {
                        ld.Errorf(s, "unexpected GOT_LOAD reloc for non-dynamic symbol %s", targ.Name);
                        return false;
                    }
                    s.P[r.Off - 2L] = 0x8dUL;
                    r.Type = objabi.R_PCREL;
                    return true;
                }
                fallthrough = true; 

                // fall through
            }
            if (fallthrough || r.Type == 512L + ld.MACHO_X86_64_RELOC_GOT * 2L + 1L)
            {
                if (targ.Type != sym.SDYNIMPORT)
                {
                    ld.Errorf(s, "unexpected GOT reloc for non-dynamic symbol %s", targ.Name);
                }
                addgotsym(ctxt, targ);
                r.Type = objabi.R_PCREL;
                r.Sym = ctxt.Syms.Lookup(".got", 0L);
                r.Add += int64(targ.Got);
                return true;
                goto __switch_break0;
            }
            // default: 
                if (r.Type >= 256L)
                {
                    ld.Errorf(s, "unexpected relocation type %d (%s)", r.Type, sym.RelocName(ctxt.Arch, r.Type));
                    return false;
                } 

                // Handle relocations found in ELF object files.

            __switch_break0:;


            if (r.Type == objabi.R_CALL || r.Type == objabi.R_PCREL) 
                if (targ.Type != sym.SDYNIMPORT)
                { 
                    // nothing to do, the relocation will be laid out in reloc
                    return true;
                } 
                // for both ELF and Mach-O
                addpltsym(ctxt, targ);
                r.Sym = ctxt.Syms.Lookup(".plt", 0L);
                r.Add = int64(targ.Plt);
                return true;
            else if (r.Type == objabi.R_ADDR) 
                if (s.Type == sym.STEXT && ctxt.IsELF)
                {
                    if (ctxt.HeadType == objabi.Hsolaris)
                    {
                        addpltsym(ctxt, targ);
                        r.Sym = ctxt.Syms.Lookup(".plt", 0L);
                        r.Add += int64(targ.Plt);
                        return true;
                    } 
                    // The code is asking for the address of an external
                    // function. We provide it with the address of the
                    // correspondent GOT symbol.
                    addgotsym(ctxt, targ);

                    r.Sym = ctxt.Syms.Lookup(".got", 0L);
                    r.Add += int64(targ.Got);
                    return true;
                } 

                // Process dynamic relocations for the data sections.
                if (ctxt.BuildMode == ld.BuildModePIE && ctxt.LinkMode == ld.LinkInternal)
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
                    switch (s.Name)
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
                    if (s.Type != sym.SDATA && s.Type != sym.SRODATA)
                    {
                        break;
                    }
                }
                if (ctxt.IsELF)
                { 
                    // TODO: We generate a R_X86_64_64 relocation for every R_ADDR, even
                    // though it would be more efficient (for the dynamic linker) if we
                    // generated R_X86_RELATIVE instead.
                    ld.Adddynsym(ctxt, targ);
                    var rela = ctxt.Syms.Lookup(".rela", 0L);
                    rela.AddAddrPlus(ctxt.Arch, s, int64(r.Off));
                    if (r.Siz == 8L)
                    {
                        rela.AddUint64(ctxt.Arch, ld.ELF64_R_INFO(uint32(targ.Dynid), uint32(elf.R_X86_64_64)));
                    }
                    else
                    { 
                        // TODO: never happens, remove.
                        rela.AddUint64(ctxt.Arch, ld.ELF64_R_INFO(uint32(targ.Dynid), uint32(elf.R_X86_64_32)));
                    }
                    rela.AddUint64(ctxt.Arch, uint64(r.Add));
                    r.Type = 256L; // ignore during relocsym
                    return true;
                }
                if (ctxt.HeadType == objabi.Hdarwin && s.Size == int64(ctxt.Arch.PtrSize) && r.Off == 0L)
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
                    ld.Adddynsym(ctxt, targ);

                    var got = ctxt.Syms.Lookup(".got", 0L);
                    s.Type = got.Type;
                    s.Attr |= sym.AttrSubSymbol;
                    s.Outer = got;
                    s.Sub = got.Sub;
                    got.Sub = s;
                    s.Value = got.Size;
                    got.AddUint64(ctxt.Arch, 0L);
                    ctxt.Syms.Lookup(".linkedit.got", 0L).AddUint32(ctxt.Arch, uint32(targ.Dynid));
                    r.Type = 256L; // ignore during relocsym
                    return true;
                }
                        return false;
        }

        private static bool elfreloc1(ref ld.Link ctxt, ref sym.Reloc r, long sectoff)
        {
            ctxt.Out.Write64(uint64(sectoff));

            var elfsym = r.Xsym.ElfsymForReloc();

            if (r.Type == objabi.R_ADDR) 
                if (r.Siz == 4L)
                {
                    ctxt.Out.Write64(uint64(elf.R_X86_64_32) | uint64(elfsym) << (int)(32L));
                }
                else if (r.Siz == 8L)
                {
                    ctxt.Out.Write64(uint64(elf.R_X86_64_64) | uint64(elfsym) << (int)(32L));
                }
                else
                {
                    return false;
                }
            else if (r.Type == objabi.R_TLS_LE) 
                if (r.Siz == 4L)
                {
                    ctxt.Out.Write64(uint64(elf.R_X86_64_TPOFF32) | uint64(elfsym) << (int)(32L));
                }
                else
                {
                    return false;
                }
            else if (r.Type == objabi.R_TLS_IE) 
                if (r.Siz == 4L)
                {
                    ctxt.Out.Write64(uint64(elf.R_X86_64_GOTTPOFF) | uint64(elfsym) << (int)(32L));
                }
                else
                {
                    return false;
                }
            else if (r.Type == objabi.R_CALL) 
                if (r.Siz == 4L)
                {
                    if (r.Xsym.Type == sym.SDYNIMPORT)
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
            else if (r.Type == objabi.R_PCREL) 
                if (r.Siz == 4L)
                {
                    if (r.Xsym.Type == sym.SDYNIMPORT && r.Xsym.ElfType == elf.STT_FUNC)
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
            else if (r.Type == objabi.R_GOTPCREL) 
                if (r.Siz == 4L)
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

        private static bool machoreloc1(ref sys.Arch arch, ref ld.OutBuf @out, ref sym.Symbol s, ref sym.Reloc r, long sectoff)
        {
            uint v = default;

            var rs = r.Xsym;

            if (rs.Type == sym.SHOSTOBJ || r.Type == objabi.R_PCREL || r.Type == objabi.R_GOTPCREL)
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

        private static bool pereloc1(ref sys.Arch arch, ref ld.OutBuf @out, ref sym.Symbol s, ref sym.Reloc r, long sectoff)
        {
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

        private static bool archreloc(ref ld.Link ctxt, ref sym.Reloc r, ref sym.Symbol s, ref long val)
        {
            return false;
        }

        private static long archrelocvariant(ref ld.Link ctxt, ref sym.Reloc r, ref sym.Symbol s, long t)
        {
            log.Fatalf("unexpected relocation variant");
            return t;
        }

        private static void elfsetupplt(ref ld.Link ctxt)
        {
            var plt = ctxt.Syms.Lookup(".plt", 0L);
            var got = ctxt.Syms.Lookup(".got.plt", 0L);
            if (plt.Size == 0L)
            { 
                // pushq got+8(IP)
                plt.AddUint8(0xffUL);

                plt.AddUint8(0x35UL);
                plt.AddPCRelPlus(ctxt.Arch, got, 8L); 

                // jmpq got+16(IP)
                plt.AddUint8(0xffUL);

                plt.AddUint8(0x25UL);
                plt.AddPCRelPlus(ctxt.Arch, got, 16L); 

                // nopl 0(AX)
                plt.AddUint32(ctxt.Arch, 0x00401f0fUL); 

                // assume got->size == 0 too
                got.AddAddrPlus(ctxt.Arch, ctxt.Syms.Lookup(".dynamic", 0L), 0L);

                got.AddUint64(ctxt.Arch, 0L);
                got.AddUint64(ctxt.Arch, 0L);
            }
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
                var rela = ctxt.Syms.Lookup(".rela.plt", 0L);
                if (plt.Size == 0L)
                {
                    elfsetupplt(ctxt);
                } 

                // jmpq *got+size(IP)
                plt.AddUint8(0xffUL);

                plt.AddUint8(0x25UL);
                plt.AddPCRelPlus(ctxt.Arch, got, got.Size); 

                // add to got: pointer to current pos in plt
                got.AddAddrPlus(ctxt.Arch, plt, plt.Size); 

                // pushq $x
                plt.AddUint8(0x68UL);

                plt.AddUint32(ctxt.Arch, uint32((got.Size - 24L - 8L) / 8L)); 

                // jmpq .plt
                plt.AddUint8(0xe9UL);

                plt.AddUint32(ctxt.Arch, uint32(-(plt.Size + 4L))); 

                // rela
                rela.AddAddrPlus(ctxt.Arch, got, got.Size - 8L);

                rela.AddUint64(ctxt.Arch, ld.ELF64_R_INFO(uint32(s.Dynid), uint32(elf.R_X86_64_JMP_SLOT)));
                rela.AddUint64(ctxt.Arch, 0L);

                s.Plt = int32(plt.Size - 16L);
            }
            else if (ctxt.HeadType == objabi.Hdarwin)
            { 
                // To do lazy symbol lookup right, we're supposed
                // to tell the dynamic loader which library each
                // symbol comes from and format the link info
                // section just so. I'm too lazy (ha!) to do that
                // so for now we'll just use non-lazy pointers,
                // which don't need to be told which library to use.
                //
                // http://networkpx.blogspot.com/2009/09/about-lcdyldinfoonly-command.html
                // has details about what we're avoiding.

                addgotsym(ctxt, s);
                plt = ctxt.Syms.Lookup(".plt", 0L);

                ctxt.Syms.Lookup(".linkedit.plt", 0L).AddUint32(ctxt.Arch, uint32(s.Dynid)); 

                // jmpq *got+size(IP)
                s.Plt = int32(plt.Size);

                plt.AddUint8(0xffUL);
                plt.AddUint8(0x25UL);
                plt.AddPCRelPlus(ctxt.Arch, ctxt.Syms.Lookup(".got", 0L), int64(s.Got));
            }
            else
            {
                ld.Errorf(s, "addpltsym: unsupported binary format");
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
            got.AddUint64(ctxt.Arch, 0L);

            if (ctxt.IsELF)
            {
                var rela = ctxt.Syms.Lookup(".rela", 0L);
                rela.AddAddrPlus(ctxt.Arch, got, int64(s.Got));
                rela.AddUint64(ctxt.Arch, ld.ELF64_R_INFO(uint32(s.Dynid), uint32(elf.R_X86_64_GLOB_DAT)));
                rela.AddUint64(ctxt.Arch, 0L);
            }
            else if (ctxt.HeadType == objabi.Hdarwin)
            {
                ctxt.Syms.Lookup(".linkedit.got", 0L).AddUint32(ctxt.Arch, uint32(s.Dynid));
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
            if (ctxt.Debugvlog != 0L)
            {
                ctxt.Logf("%5.2f codeblk\n", ld.Cputime());
            }
            if (ctxt.IsELF)
            {
                ld.Asmbelfsetup();
            }
            var sect = ld.Segtext.Sections[0L];
            ctxt.Out.SeekSet(int64(sect.Vaddr - ld.Segtext.Vaddr + ld.Segtext.Fileoff)); 
            // 0xCC is INT $3 - breakpoint instruction
            ld.CodeblkPad(ctxt, int64(sect.Vaddr), int64(sect.Length), new slice<byte>(new byte[] { 0xCC }));
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
            else if (ctxt.HeadType == objabi.Hnacl || ctxt.HeadType == objabi.Hwindows) 
                break;
            else 
                ld.Errorf(null, "unknown header type %v", ctxt.HeadType);
                        ld.Symsize = 0L;
            ld.Spsize = 0L;
            ld.Lcsize = 0L;
            var symo = int64(0L);
            if (!ld.FlagS.Value)
            {
                if (ctxt.Debugvlog != 0L)
                {
                    ctxt.Logf("%5.2f sym\n", ld.Cputime());
                }

                if (ctxt.HeadType == objabi.Hplan9) 
                    ld.FlagS.Value = true;
                    symo = int64(ld.Segdata.Fileoff + ld.Segdata.Filelen);
                else if (ctxt.HeadType == objabi.Hdarwin) 
                    symo = int64(ld.Segdwarf.Fileoff + uint64(ld.Rnd(int64(ld.Segdwarf.Filelen), int64(ld.FlagRound.Value))) + uint64(machlink));
                else if (ctxt.HeadType == objabi.Hlinux || ctxt.HeadType == objabi.Hfreebsd || ctxt.HeadType == objabi.Hnetbsd || ctxt.HeadType == objabi.Hopenbsd || ctxt.HeadType == objabi.Hdragonfly || ctxt.HeadType == objabi.Hsolaris || ctxt.HeadType == objabi.Hnacl) 
                    symo = int64(ld.Segdwarf.Fileoff + ld.Segdwarf.Filelen);
                    symo = ld.Rnd(symo, int64(ld.FlagRound.Value));
                else if (ctxt.HeadType == objabi.Hwindows) 
                    symo = int64(ld.Segdwarf.Fileoff + ld.Segdwarf.Filelen);
                    symo = ld.Rnd(symo, ld.PEFILEALIGN);
                else                                 ctxt.Out.SeekSet(symo);

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
                else if (ctxt.HeadType == objabi.Hwindows) 
                    if (ctxt.Debugvlog != 0L)
                    {
                        ctxt.Logf("%5.2f dwarf\n", ld.Cputime());
                    }
                else if (ctxt.HeadType == objabi.Hdarwin) 
                    if (ctxt.LinkMode == ld.LinkExternal)
                    {
                        ld.Machoemitreloc(ctxt);
                    }
                else 
                    if (ctxt.IsELF)
                    {
                        ctxt.Out.SeekSet(symo);
                        ld.Asmelfsym(ctxt);
                        ctxt.Out.Flush();
                        ctxt.Out.Write(ld.Elfstrdat);

                        if (ctxt.Debugvlog != 0L)
                        {
                            ctxt.Logf("%5.2f dwarf\n", ld.Cputime());
                        }
                        if (ctxt.LinkMode == ld.LinkExternal)
                        {
                            ld.Elfemitreloc(ctxt);
                        }
                    }
                            }
            if (ctxt.Debugvlog != 0L)
            {
                ctxt.Logf("%5.2f headr\n", ld.Cputime());
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
            else if (ctxt.HeadType == objabi.Hlinux || ctxt.HeadType == objabi.Hfreebsd || ctxt.HeadType == objabi.Hnetbsd || ctxt.HeadType == objabi.Hopenbsd || ctxt.HeadType == objabi.Hdragonfly || ctxt.HeadType == objabi.Hsolaris || ctxt.HeadType == objabi.Hnacl) 
                ld.Asmbelf(ctxt, symo);
            else if (ctxt.HeadType == objabi.Hwindows) 
                ld.Asmbpe(ctxt);
            else                         ctxt.Out.Flush();
        }

        private static void tlsIEtoLE(ref sym.Symbol s, long off, long size)
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
            var op = s.P[off - 3L..off];
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
