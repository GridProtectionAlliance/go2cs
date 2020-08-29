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

// package ppc64 -- go2cs converted at 2020 August 29 10:02:42 UTC
// import "cmd/link/internal/ppc64" ==> using ppc64 = go.cmd.link.@internal.ppc64_package
// Original source: C:\Go\src\cmd\link\internal\ppc64\asm.go
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
    public static partial class ppc64_package
    {
        private static void genplt(ref ld.Link ctxt)
        { 
            // The ppc64 ABI PLT has similar concepts to other
            // architectures, but is laid out quite differently. When we
            // see an R_PPC64_REL24 relocation to a dynamic symbol
            // (indicating that the call needs to go through the PLT), we
            // generate up to three stubs and reserve a PLT slot.
            //
            // 1) The call site will be bl x; nop (where the relocation
            //    applies to the bl).  We rewrite this to bl x_stub; ld
            //    r2,24(r1).  The ld is necessary because x_stub will save
            //    r2 (the TOC pointer) at 24(r1) (the "TOC save slot").
            //
            // 2) We reserve space for a pointer in the .plt section (once
            //    per referenced dynamic function).  .plt is a data
            //    section filled solely by the dynamic linker (more like
            //    .plt.got on other architectures).  Initially, the
            //    dynamic linker will fill each slot with a pointer to the
            //    corresponding x@plt entry point.
            //
            // 3) We generate the "call stub" x_stub (once per dynamic
            //    function/object file pair).  This saves the TOC in the
            //    TOC save slot, reads the function pointer from x's .plt
            //    slot and calls it like any other global entry point
            //    (including setting r12 to the function address).
            //
            // 4) We generate the "symbol resolver stub" x@plt (once per
            //    dynamic function).  This is solely a branch to the glink
            //    resolver stub.
            //
            // 5) We generate the glink resolver stub (only once).  This
            //    computes which symbol resolver stub we came through and
            //    invokes the dynamic resolver via a pointer provided by
            //    the dynamic linker. This will patch up the .plt slot to
            //    point directly at the function so future calls go
            //    straight from the call stub to the real function, and
            //    then call the function.

            // NOTE: It's possible we could make ppc64 closer to other
            // architectures: ppc64's .plt is like .plt.got on other
            // platforms and ppc64's .glink is like .plt on other
            // platforms.

            // Find all R_PPC64_REL24 relocations that reference dynamic
            // imports. Reserve PLT entries for these symbols and
            // generate call stubs. The call stubs need to live in .text,
            // which is why we need to do this pass this early.
            //
            // This assumes "case 1" from the ABI, where the caller needs
            // us to save and restore the TOC pointer.
            slice<ref sym.Symbol> stubs = default;
            foreach (var (_, s) in ctxt.Textp)
            {
                foreach (var (i) in s.R)
                {
                    var r = ref s.R[i];
                    if (r.Type != 256L + objabi.RelocType(elf.R_PPC64_REL24) || r.Sym.Type != sym.SDYNIMPORT)
                    {
                        continue;
                    }
                    addpltsym(ctxt, r.Sym); 

                    // Generate call stub
                    var n = fmt.Sprintf("%s.%s", s.Name, r.Sym.Name);

                    var stub = ctxt.Syms.Lookup(n, 0L);
                    if (s.Attr.Reachable())
                    {
                        stub.Attr |= sym.AttrReachable;
                    }
                    if (stub.Size == 0L)
                    { 
                        // Need outer to resolve .TOC.
                        stub.Outer = s;
                        stubs = append(stubs, stub);
                        gencallstub(ctxt, 1L, stub, r.Sym);
                    }
                    r.Sym = stub; 

                    // Restore TOC after bl. The compiler put a
                    // nop here for us to overwrite.
                    const ulong o1 = 0xe8410018UL; // ld r2,24(r1)
 // ld r2,24(r1)
                    ctxt.Arch.ByteOrder.PutUint32(s.P[r.Off + 4L..], o1);
                }
            }            ctxt.Textp = append(stubs, ctxt.Textp);
        }

        private static void genaddmoduledata(ref ld.Link ctxt)
        {
            var addmoduledata = ctxt.Syms.ROLookup("runtime.addmoduledata", 0L);
            if (addmoduledata.Type == sym.STEXT && ctxt.BuildMode != ld.BuildModePlugin)
            {
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
            } 
            // addis r2, r12, .TOC.-func@ha
; 
            // addis r2, r12, .TOC.-func@ha
            var rel = initfunc.AddRel();
            rel.Off = int32(initfunc.Size);
            rel.Siz = 8L;
            rel.Sym = ctxt.Syms.Lookup(".TOC.", 0L);
            rel.Sym.Attr |= sym.AttrReachable;
            rel.Type = objabi.R_ADDRPOWER_PCREL;
            o(0x3c4c0000UL); 
            // addi r2, r2, .TOC.-func@l
            o(0x38420000UL); 
            // mflr r31
            o(0x7c0802a6UL); 
            // stdu r31, -32(r1)
            o(0xf801ffe1UL); 
            // addis r3, r2, local.moduledata@got@ha
            rel = initfunc.AddRel();
            rel.Off = int32(initfunc.Size);
            rel.Siz = 8L;
            {
                var s__prev1 = s;

                var s = ctxt.Syms.ROLookup("local.moduledata", 0L);

                if (s != null)
                {
                    rel.Sym = s;
                }                {
                    var s__prev2 = s;

                    s = ctxt.Syms.ROLookup("local.pluginmoduledata", 0L);


                    else if (s != null)
                    {
                        rel.Sym = s;
                    }
                    else
                    {
                        rel.Sym = ctxt.Syms.Lookup("runtime.firstmoduledata", 0L);
                    }

                    s = s__prev2;

                }

                s = s__prev1;

            }
            rel.Sym.Attr |= sym.AttrReachable;
            rel.Sym.Attr |= sym.AttrLocal;
            rel.Type = objabi.R_ADDRPOWER_GOT;
            o(0x3c620000UL); 
            // ld r3, local.moduledata@got@l(r3)
            o(0xe8630000UL); 
            // bl runtime.addmoduledata
            rel = initfunc.AddRel();
            rel.Off = int32(initfunc.Size);
            rel.Siz = 4L;
            rel.Sym = addmoduledata;
            rel.Type = objabi.R_CALLPOWER;
            o(0x48000001UL); 
            // nop
            o(0x60000000UL); 
            // ld r31, 0(r1)
            o(0xe8010000UL); 
            // mtlr r31
            o(0x7c0803a6UL); 
            // addi r1,r1,32
            o(0x38210020UL); 
            // blr
            o(0x4e800020UL);

            if (ctxt.BuildMode == ld.BuildModePlugin)
            {
                ctxt.Textp = append(ctxt.Textp, addmoduledata);
            }
            var initarray_entry = ctxt.Syms.Lookup("go.link.addmoduledatainit", 0L);
            ctxt.Textp = append(ctxt.Textp, initfunc);
            initarray_entry.Attr |= sym.AttrReachable;
            initarray_entry.Attr |= sym.AttrLocal;
            initarray_entry.Type = sym.SINITARR;
            initarray_entry.AddAddr(ctxt.Arch, initfunc);
        }

        private static void gentext(ref ld.Link ctxt)
        {
            if (ctxt.DynlinkingGo())
            {
                genaddmoduledata(ctxt);
            }
            if (ctxt.LinkMode == ld.LinkInternal)
            {
                genplt(ctxt);
            }
        }

        // Construct a call stub in stub that calls symbol targ via its PLT
        // entry.
        private static void gencallstub(ref ld.Link ctxt, long abicase, ref sym.Symbol stub, ref sym.Symbol targ)
        {
            if (abicase != 1L)
            { 
                // If we see R_PPC64_TOCSAVE or R_PPC64_REL24_NOTOC
                // relocations, we'll need to implement cases 2 and 3.
                log.Fatalf("gencallstub only implements case 1 calls");
            }
            var plt = ctxt.Syms.Lookup(".plt", 0L);

            stub.Type = sym.STEXT; 

            // Save TOC pointer in TOC save slot
            stub.AddUint32(ctxt.Arch, 0xf8410018UL); // std r2,24(r1)

            // Load the function pointer from the PLT.
            var r = stub.AddRel();

            r.Off = int32(stub.Size);
            r.Sym = plt;
            r.Add = int64(targ.Plt);
            r.Siz = 2L;
            if (ctxt.Arch.ByteOrder == binary.BigEndian)
            {
                r.Off += int32(r.Siz);
            }
            r.Type = objabi.R_POWER_TOC;
            r.Variant = sym.RV_POWER_HA;
            stub.AddUint32(ctxt.Arch, 0x3d820000UL); // addis r12,r2,targ@plt@toc@ha
            r = stub.AddRel();
            r.Off = int32(stub.Size);
            r.Sym = plt;
            r.Add = int64(targ.Plt);
            r.Siz = 2L;
            if (ctxt.Arch.ByteOrder == binary.BigEndian)
            {
                r.Off += int32(r.Siz);
            }
            r.Type = objabi.R_POWER_TOC;
            r.Variant = sym.RV_POWER_LO;
            stub.AddUint32(ctxt.Arch, 0xe98c0000UL); // ld r12,targ@plt@toc@l(r12)

            // Jump to the loaded pointer
            stub.AddUint32(ctxt.Arch, 0x7d8903a6UL); // mtctr r12
            stub.AddUint32(ctxt.Arch, 0x4e800420UL); // bctr
        }

        private static bool adddynrel(ref ld.Link ctxt, ref sym.Symbol s, ref sym.Reloc r)
        {
            var targ = r.Sym;

            switch (r.Type)
            {
                case 256L + objabi.RelocType(elf.R_PPC64_REL24): 
                    r.Type = objabi.R_CALLPOWER; 

                    // This is a local call, so the caller isn't setting
                    // up r12 and r2 is the same for the caller and
                    // callee. Hence, we need to go to the local entry
                    // point.  (If we don't do this, the callee will try
                    // to use r12 to compute r2.)
                    r.Add += int64(r.Sym.Localentry) * 4L;

                    if (targ.Type == sym.SDYNIMPORT)
                    { 
                        // Should have been handled in elfsetupplt
                        ld.Errorf(s, "unexpected R_PPC64_REL24 for dyn import");
                    }
                    return true;
                    break;
                case 256L + objabi.RelocType(elf.R_PPC_REL32): 
                    r.Type = objabi.R_PCREL;
                    r.Add += 4L;

                    if (targ.Type == sym.SDYNIMPORT)
                    {
                        ld.Errorf(s, "unexpected R_PPC_REL32 for dyn import");
                    }
                    return true;
                    break;
                case 256L + objabi.RelocType(elf.R_PPC64_ADDR64): 
                    r.Type = objabi.R_ADDR;
                    if (targ.Type == sym.SDYNIMPORT)
                    { 
                        // These happen in .toc sections
                        ld.Adddynsym(ctxt, targ);

                        var rela = ctxt.Syms.Lookup(".rela", 0L);
                        rela.AddAddrPlus(ctxt.Arch, s, int64(r.Off));
                        rela.AddUint64(ctxt.Arch, ld.ELF64_R_INFO(uint32(targ.Dynid), uint32(elf.R_PPC64_ADDR64)));
                        rela.AddUint64(ctxt.Arch, uint64(r.Add));
                        r.Type = 256L; // ignore during relocsym
                    }
                    return true;
                    break;
                case 256L + objabi.RelocType(elf.R_PPC64_TOC16): 
                    r.Type = objabi.R_POWER_TOC;
                    r.Variant = sym.RV_POWER_LO | sym.RV_CHECK_OVERFLOW;
                    return true;
                    break;
                case 256L + objabi.RelocType(elf.R_PPC64_TOC16_LO): 
                    r.Type = objabi.R_POWER_TOC;
                    r.Variant = sym.RV_POWER_LO;
                    return true;
                    break;
                case 256L + objabi.RelocType(elf.R_PPC64_TOC16_HA): 
                    r.Type = objabi.R_POWER_TOC;
                    r.Variant = sym.RV_POWER_HA | sym.RV_CHECK_OVERFLOW;
                    return true;
                    break;
                case 256L + objabi.RelocType(elf.R_PPC64_TOC16_HI): 
                    r.Type = objabi.R_POWER_TOC;
                    r.Variant = sym.RV_POWER_HI | sym.RV_CHECK_OVERFLOW;
                    return true;
                    break;
                case 256L + objabi.RelocType(elf.R_PPC64_TOC16_DS): 
                    r.Type = objabi.R_POWER_TOC;
                    r.Variant = sym.RV_POWER_DS | sym.RV_CHECK_OVERFLOW;
                    return true;
                    break;
                case 256L + objabi.RelocType(elf.R_PPC64_TOC16_LO_DS): 
                    r.Type = objabi.R_POWER_TOC;
                    r.Variant = sym.RV_POWER_DS;
                    return true;
                    break;
                case 256L + objabi.RelocType(elf.R_PPC64_REL16_LO): 
                    r.Type = objabi.R_PCREL;
                    r.Variant = sym.RV_POWER_LO;
                    r.Add += 2L; // Compensate for relocation size of 2
                    return true;
                    break;
                case 256L + objabi.RelocType(elf.R_PPC64_REL16_HI): 
                    r.Type = objabi.R_PCREL;
                    r.Variant = sym.RV_POWER_HI | sym.RV_CHECK_OVERFLOW;
                    r.Add += 2L;
                    return true;
                    break;
                case 256L + objabi.RelocType(elf.R_PPC64_REL16_HA): 
                    r.Type = objabi.R_PCREL;
                    r.Variant = sym.RV_POWER_HA | sym.RV_CHECK_OVERFLOW;
                    r.Add += 2L;
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

            // TODO(austin): Translate our relocations to ELF
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
                        ctxt.Out.Write64(uint64(elf.R_PPC64_ADDR32) | uint64(elfsym) << (int)(32L));
                        break;
                    case 8L: 
                        ctxt.Out.Write64(uint64(elf.R_PPC64_ADDR64) | uint64(elfsym) << (int)(32L));
                        break;
                    default: 
                        return false;
                        break;
                }
            else if (r.Type == objabi.R_POWER_TLS) 
                ctxt.Out.Write64(uint64(elf.R_PPC64_TLS) | uint64(elfsym) << (int)(32L));
            else if (r.Type == objabi.R_POWER_TLS_LE) 
                ctxt.Out.Write64(uint64(elf.R_PPC64_TPREL16) | uint64(elfsym) << (int)(32L));
            else if (r.Type == objabi.R_POWER_TLS_IE) 
                ctxt.Out.Write64(uint64(elf.R_PPC64_GOT_TPREL16_HA) | uint64(elfsym) << (int)(32L));
                ctxt.Out.Write64(uint64(r.Xadd));
                ctxt.Out.Write64(uint64(sectoff + 4L));
                ctxt.Out.Write64(uint64(elf.R_PPC64_GOT_TPREL16_LO_DS) | uint64(elfsym) << (int)(32L));
            else if (r.Type == objabi.R_ADDRPOWER) 
                ctxt.Out.Write64(uint64(elf.R_PPC64_ADDR16_HA) | uint64(elfsym) << (int)(32L));
                ctxt.Out.Write64(uint64(r.Xadd));
                ctxt.Out.Write64(uint64(sectoff + 4L));
                ctxt.Out.Write64(uint64(elf.R_PPC64_ADDR16_LO) | uint64(elfsym) << (int)(32L));
            else if (r.Type == objabi.R_ADDRPOWER_DS) 
                ctxt.Out.Write64(uint64(elf.R_PPC64_ADDR16_HA) | uint64(elfsym) << (int)(32L));
                ctxt.Out.Write64(uint64(r.Xadd));
                ctxt.Out.Write64(uint64(sectoff + 4L));
                ctxt.Out.Write64(uint64(elf.R_PPC64_ADDR16_LO_DS) | uint64(elfsym) << (int)(32L));
            else if (r.Type == objabi.R_ADDRPOWER_GOT) 
                ctxt.Out.Write64(uint64(elf.R_PPC64_GOT16_HA) | uint64(elfsym) << (int)(32L));
                ctxt.Out.Write64(uint64(r.Xadd));
                ctxt.Out.Write64(uint64(sectoff + 4L));
                ctxt.Out.Write64(uint64(elf.R_PPC64_GOT16_LO_DS) | uint64(elfsym) << (int)(32L));
            else if (r.Type == objabi.R_ADDRPOWER_PCREL) 
                ctxt.Out.Write64(uint64(elf.R_PPC64_REL16_HA) | uint64(elfsym) << (int)(32L));
                ctxt.Out.Write64(uint64(r.Xadd));
                ctxt.Out.Write64(uint64(sectoff + 4L));
                ctxt.Out.Write64(uint64(elf.R_PPC64_REL16_LO) | uint64(elfsym) << (int)(32L));
                r.Xadd += 4L;
            else if (r.Type == objabi.R_ADDRPOWER_TOCREL) 
                ctxt.Out.Write64(uint64(elf.R_PPC64_TOC16_HA) | uint64(elfsym) << (int)(32L));
                ctxt.Out.Write64(uint64(r.Xadd));
                ctxt.Out.Write64(uint64(sectoff + 4L));
                ctxt.Out.Write64(uint64(elf.R_PPC64_TOC16_LO) | uint64(elfsym) << (int)(32L));
            else if (r.Type == objabi.R_ADDRPOWER_TOCREL_DS) 
                ctxt.Out.Write64(uint64(elf.R_PPC64_TOC16_HA) | uint64(elfsym) << (int)(32L));
                ctxt.Out.Write64(uint64(r.Xadd));
                ctxt.Out.Write64(uint64(sectoff + 4L));
                ctxt.Out.Write64(uint64(elf.R_PPC64_TOC16_LO_DS) | uint64(elfsym) << (int)(32L));
            else if (r.Type == objabi.R_CALLPOWER) 
                if (r.Siz != 4L)
                {
                    return false;
                }
                ctxt.Out.Write64(uint64(elf.R_PPC64_REL24) | uint64(elfsym) << (int)(32L));
            else 
                return false;
                        ctxt.Out.Write64(uint64(r.Xadd));

            return true;
        }

        private static void elfsetupplt(ref ld.Link ctxt)
        {
            var plt = ctxt.Syms.Lookup(".plt", 0L);
            if (plt.Size == 0L)
            { 
                // The dynamic linker stores the address of the
                // dynamic resolver and the DSO identifier in the two
                // doublewords at the beginning of the .plt section
                // before the PLT array. Reserve space for these.
                plt.Size = 16L;
            }
        }

        private static bool machoreloc1(ref sys.Arch arch, ref ld.OutBuf @out, ref sym.Symbol s, ref sym.Reloc r, long sectoff)
        {
            return false;
        }

        // Return the value of .TOC. for symbol s
        private static long symtoc(ref ld.Link ctxt, ref sym.Symbol s)
        {
            ref sym.Symbol toc = default;

            if (s.Outer != null)
            {
                toc = ctxt.Syms.ROLookup(".TOC.", int(s.Outer.Version));
            }
            else
            {
                toc = ctxt.Syms.ROLookup(".TOC.", int(s.Version));
            }
            if (toc == null)
            {
                ld.Errorf(s, "TOC-relative relocation in object without .TOC.");
                return 0L;
            }
            return toc.Value;
        }

        private static bool archrelocaddr(ref ld.Link ctxt, ref sym.Reloc r, ref sym.Symbol s, ref long val)
        {
            uint o1 = default;            uint o2 = default;

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

            // We are spreading a 31-bit address across two instructions, putting the
            // high (adjusted) part in the low 16 bits of the first instruction and the
            // low part in the low 16 bits of the second instruction, or, in the DS case,
            // bits 15-2 (inclusive) of the address into bits 15-2 of the second
            // instruction (it is an error in this case if the low 2 bits of the address
            // are non-zero).
            var t = ld.Symaddr(r.Sym) + r.Add;
            if (t < 0L || t >= 1L << (int)(31L))
            {
                ld.Errorf(s, "relocation for %s is too big (>=2G): %d", s.Name, ld.Symaddr(r.Sym));
            }
            if (t & 0x8000UL != 0L)
            {
                t += 0x10000UL;
            }

            if (r.Type == objabi.R_ADDRPOWER) 
                o1 |= (uint32(t) >> (int)(16L)) & 0xffffUL;
                o2 |= uint32(t) & 0xffffUL;
            else if (r.Type == objabi.R_ADDRPOWER_DS) 
                o1 |= (uint32(t) >> (int)(16L)) & 0xffffUL;
                if (t & 3L != 0L)
                {
                    ld.Errorf(s, "bad DS reloc for %s: %d", s.Name, ld.Symaddr(r.Sym));
                }
                o2 |= uint32(t) & 0xfffcUL;
            else 
                return false;
                        if (ctxt.Arch.ByteOrder == binary.BigEndian)
            {
                val.Value = int64(o1) << (int)(32L) | int64(o2);
            }
            else
            {
                val.Value = int64(o2) << (int)(32L) | int64(o1);
            }
            return true;
        }

        // resolve direct jump relocation r in s, and add trampoline if necessary
        private static void trampoline(ref ld.Link ctxt, ref sym.Reloc r, ref sym.Symbol s)
        {
            // Trampolines are created if the branch offset is too large and the linker cannot insert a call stub to handle it.
            // For internal linking, trampolines are always created for long calls.
            // For external linking, the linker can insert a call stub to handle a long call, but depends on having the TOC address in
            // r2.  For those build modes with external linking where the TOC address is not maintained in r2, trampolines must be created.
            if (ctxt.LinkMode == ld.LinkExternal && (ctxt.DynlinkingGo() || ctxt.BuildMode == ld.BuildModeCArchive || ctxt.BuildMode == ld.BuildModeCShared || ctxt.BuildMode == ld.BuildModePIE))
            { 
                // No trampolines needed since r2 contains the TOC
                return;
            }
            var t = ld.Symaddr(r.Sym) + r.Add - (s.Value + int64(r.Off));

            if (r.Type == objabi.R_CALLPOWER) 

                // If branch offset is too far then create a trampoline.

                if ((ctxt.LinkMode == ld.LinkExternal && s.Sect != r.Sym.Sect) || (ctxt.LinkMode == ld.LinkInternal && int64(int32(t << (int)(6L)) >> (int)(6L)) != t) || (ld.FlagDebugTramp > 1L && s.File != r.Sym.File.Value))
                {
                    ref sym.Symbol tramp = default;
                    for (long i = 0L; >>MARKER:FOREXPRESSION_LEVEL_1<<; i++)
                    {
                        // Using r.Add as part of the name is significant in functions like duffzero where the call
                        // target is at some offset within the function.  Calls to duff+8 and duff+256 must appear as
                        // distinct trampolines.

                        var name = r.Sym.Name;
                        if (r.Add == 0L)
                        {
                            name = name + fmt.Sprintf("-tramp%d", i);
                        }
                        else
                        {
                            name = name + fmt.Sprintf("%+x-tramp%d", r.Add, i);
                        } 

                        // Look up the trampoline in case it already exists
                        tramp = ctxt.Syms.Lookup(name, int(r.Sym.Version));
                        if (tramp.Value == 0L)
                        {
                            break;
                        }
                        t = ld.Symaddr(tramp) + r.Add - (s.Value + int64(r.Off)); 

                        // With internal linking, the trampoline can be used if it is not too far.
                        // With external linking, the trampoline must be in this section for it to be reused.
                        if ((ctxt.LinkMode == ld.LinkInternal && int64(int32(t << (int)(6L)) >> (int)(6L)) == t) || (ctxt.LinkMode == ld.LinkExternal && s.Sect == tramp.Sect))
                        {
                            break;
                        }
                    }

                    if (tramp.Type == 0L)
                    {
                        if (ctxt.DynlinkingGo() || ctxt.BuildMode == ld.BuildModeCArchive || ctxt.BuildMode == ld.BuildModeCShared || ctxt.BuildMode == ld.BuildModePIE)
                        { 
                            // Should have returned for above cases
                            ld.Errorf(s, "unexpected trampoline for shared or dynamic linking\n");
                        }
                        else
                        {
                            ctxt.AddTramp(tramp);
                            gentramp(ctxt.Arch, ctxt.LinkMode, tramp, r.Sym, int64(r.Add));
                        }
                    }
                    r.Sym = tramp;
                    r.Add = 0L; // This was folded into the trampoline target address
                    r.Done = false;
                }
            else 
                ld.Errorf(s, "trampoline called with non-jump reloc: %d (%s)", r.Type, sym.RelocName(ctxt.Arch, r.Type));
                    }

        private static void gentramp(ref sys.Arch arch, ld.LinkMode linkmode, ref sym.Symbol tramp, ref sym.Symbol target, long offset)
        { 
            // Used for default build mode for an executable
            // Address of the call target is generated using
            // relocation and doesn't depend on r2 (TOC).
            tramp.Size = 16L; // 4 instructions
            tramp.P = make_slice<byte>(tramp.Size);
            var t = ld.Symaddr(target) + offset;
            var o1 = uint32(0x3fe00000UL); // lis r31,targetaddr hi
            var o2 = uint32(0x3bff0000UL); // addi r31,targetaddr lo
            // With external linking, the target address must be
            // relocated using LO and HA
            if (linkmode == ld.LinkExternal)
            {
                var tr = tramp.AddRel();
                tr.Off = 0L;
                tr.Type = objabi.R_ADDRPOWER;
                tr.Siz = 8L; // generates 2 relocations:  HA + LO
                tr.Sym = target;
                tr.Add = offset;
            }
            else
            { 
                // adjustment needed if lo has sign bit set
                // when using addi to compute address
                var val = uint32((t & 0xffff0000UL) >> (int)(16L));
                if (t & 0x8000UL != 0L)
                {
                    val += 1L;
                }
                o1 |= val; // hi part of addr
                o2 |= uint32(t & 0xffffUL); // lo part of addr
            }
            var o3 = uint32(0x7fe903a6UL); // mtctr r31
            var o4 = uint32(0x4e800420UL); // bctr
            arch.ByteOrder.PutUint32(tramp.P, o1);
            arch.ByteOrder.PutUint32(tramp.P[4L..], o2);
            arch.ByteOrder.PutUint32(tramp.P[8L..], o3);
            arch.ByteOrder.PutUint32(tramp.P[12L..], o4);
        }

        private static bool archreloc(ref ld.Link ctxt, ref sym.Reloc r, ref sym.Symbol s, ref long val)
        {
            if (ctxt.LinkMode == ld.LinkExternal)
            {

                if (r.Type == objabi.R_POWER_TLS || r.Type == objabi.R_POWER_TLS_LE || r.Type == objabi.R_POWER_TLS_IE) 
                    r.Done = false; 
                    // check Outer is nil, Type is TLSBSS?
                    r.Xadd = r.Add;
                    r.Xsym = r.Sym;
                    return true;
                else if (r.Type == objabi.R_ADDRPOWER || r.Type == objabi.R_ADDRPOWER_DS || r.Type == objabi.R_ADDRPOWER_TOCREL || r.Type == objabi.R_ADDRPOWER_TOCREL_DS || r.Type == objabi.R_ADDRPOWER_GOT || r.Type == objabi.R_ADDRPOWER_PCREL) 
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

                    return true;
                else if (r.Type == objabi.R_CALLPOWER) 
                    r.Done = false;
                    r.Xsym = r.Sym;
                    r.Xadd = r.Add;
                    return true;
                else 
                    return false;
                            }

            if (r.Type == objabi.R_CONST) 
                val.Value = r.Add;
                return true;
            else if (r.Type == objabi.R_GOTOFF) 
                val.Value = ld.Symaddr(r.Sym) + r.Add - ld.Symaddr(ctxt.Syms.Lookup(".got", 0L));
                return true;
            else if (r.Type == objabi.R_ADDRPOWER || r.Type == objabi.R_ADDRPOWER_DS) 
                return archrelocaddr(ctxt, r, s, val);
            else if (r.Type == objabi.R_CALLPOWER) 
                // Bits 6 through 29 = (S + A - P) >> 2

                var t = ld.Symaddr(r.Sym) + r.Add - (s.Value + int64(r.Off));

                if (t & 3L != 0L)
                {
                    ld.Errorf(s, "relocation for %s+%d is not aligned: %d", r.Sym.Name, r.Off, t);
                } 
                // If branch offset is too far then create a trampoline.
                if (int64(int32(t << (int)(6L)) >> (int)(6L)) != t)
                {
                    ld.Errorf(s, "direct call too far: %s %x", r.Sym.Name, t);
                }
                val.Value |= int64(uint32(t) & ~0xfc000003UL);
                return true;
            else if (r.Type == objabi.R_POWER_TOC) // S + A - .TOC.
                val.Value = ld.Symaddr(r.Sym) + r.Add - symtoc(ctxt, s);

                return true;
            else if (r.Type == objabi.R_POWER_TLS_LE) 
                // The thread pointer points 0x7000 bytes after the start of the the
                // thread local storage area as documented in section "3.7.2 TLS
                // Runtime Handling" of "Power Architecture 64-Bit ELF V2 ABI
                // Specification".
                var v = r.Sym.Value - 0x7000UL;
                if (int64(int16(v)) != v)
                {
                    ld.Errorf(s, "TLS offset out of range %d", v);
                }
                val.Value = (val & ~0xffffUL.Value) | (v & 0xffffUL);
                return true;
                        return false;
        }

        private static long archrelocvariant(ref ld.Link ctxt, ref sym.Reloc r, ref sym.Symbol s, long t)
        {

            if (r.Variant & sym.RV_TYPE_MASK == sym.RV_NONE)
            {
                return t;
                goto __switch_break0;
            }
            if (r.Variant & sym.RV_TYPE_MASK == sym.RV_POWER_LO)
            {
                if (r.Variant & sym.RV_CHECK_OVERFLOW != 0L)
                { 
                    // Whether to check for signed or unsigned
                    // overflow depends on the instruction
                    uint o1 = default;
                    if (ctxt.Arch.ByteOrder == binary.BigEndian)
                    {
                        o1 = ld.Be32(s.P[r.Off - 2L..]);
                    }
                    else
                    {
                        o1 = ld.Le32(s.P[r.Off..]);
                    }
                    switch (o1 >> (int)(26L))
                    {
                        case 24L: // andi

                        case 26L: // andi

                        case 28L: // andi
                            if (t >> (int)(16L) != 0L)
                            {
                                goto overflow;
                            }
                            break;
                        default: 
                            if (int64(int16(t)) != t)
                            {
                                goto overflow;
                            }
                            break;
                    }
                }
                return int64(int16(t));
                goto __switch_break0;
            }
            if (r.Variant & sym.RV_TYPE_MASK == sym.RV_POWER_HA)
            {
                t += 0x8000UL;
                fallthrough = true; 

                // Fallthrough
            }
            if (fallthrough || r.Variant & sym.RV_TYPE_MASK == sym.RV_POWER_HI)
            {
                t >>= 16L;

                if (r.Variant & sym.RV_CHECK_OVERFLOW != 0L)
                { 
                    // Whether to check for signed or unsigned
                    // overflow depends on the instruction
                    o1 = default;
                    if (ctxt.Arch.ByteOrder == binary.BigEndian)
                    {
                        o1 = ld.Be32(s.P[r.Off - 2L..]);
                    }
                    else
                    {
                        o1 = ld.Le32(s.P[r.Off..]);
                    }
                    switch (o1 >> (int)(26L))
                    {
                        case 25L: // andis

                        case 27L: // andis

                        case 29L: // andis
                            if (t >> (int)(16L) != 0L)
                            {
                                goto overflow;
                            }
                            break;
                        default: 
                            if (int64(int16(t)) != t)
                            {
                                goto overflow;
                            }
                            break;
                    }
                }
                return int64(int16(t));
                goto __switch_break0;
            }
            if (r.Variant & sym.RV_TYPE_MASK == sym.RV_POWER_DS)
            {
                o1 = default;
                if (ctxt.Arch.ByteOrder == binary.BigEndian)
                {
                    o1 = uint32(ld.Be16(s.P[r.Off..]));
                }
                else
                {
                    o1 = uint32(ld.Le16(s.P[r.Off..]));
                }
                if (t & 3L != 0L)
                {
                    ld.Errorf(s, "relocation for %s+%d is not aligned: %d", r.Sym.Name, r.Off, t);
                }
                if ((r.Variant & sym.RV_CHECK_OVERFLOW != 0L) && int64(int16(t)) != t)
                {
                    goto overflow;
                }
                return int64(o1) & 0x3UL | int64(int16(t));
                goto __switch_break0;
            }
            // default: 
                ld.Errorf(s, "unexpected relocation variant %d", r.Variant);

            __switch_break0:;

overflow:
            ld.Errorf(s, "relocation for %s+%d is too big: %d", r.Sym.Name, r.Off, t);
            return t;
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
                var rela = ctxt.Syms.Lookup(".rela.plt", 0L);
                if (plt.Size == 0L)
                {
                    elfsetupplt(ctxt);
                } 

                // Create the glink resolver if necessary
                var glink = ensureglinkresolver(ctxt); 

                // Write symbol resolver stub (just a branch to the
                // glink resolver stub)
                var r = glink.AddRel();

                r.Sym = glink;
                r.Off = int32(glink.Size);
                r.Siz = 4L;
                r.Type = objabi.R_CALLPOWER;
                glink.AddUint32(ctxt.Arch, 0x48000000UL); // b .glink

                // In the ppc64 ABI, the dynamic linker is responsible
                // for writing the entire PLT.  We just need to
                // reserve 8 bytes for each PLT entry and generate a
                // JMP_SLOT dynamic relocation for it.
                //
                // TODO(austin): ABI v1 is different
                s.Plt = int32(plt.Size);

                plt.Size += 8L;

                rela.AddAddrPlus(ctxt.Arch, plt, int64(s.Plt));
                rela.AddUint64(ctxt.Arch, ld.ELF64_R_INFO(uint32(s.Dynid), uint32(elf.R_PPC64_JMP_SLOT)));
                rela.AddUint64(ctxt.Arch, 0L);
            }
            else
            {
                ld.Errorf(s, "addpltsym: unsupported binary format");
            }
        }

        // Generate the glink resolver stub if necessary and return the .glink section
        private static ref sym.Symbol ensureglinkresolver(ref ld.Link ctxt)
        {
            var glink = ctxt.Syms.Lookup(".glink", 0L);
            if (glink.Size != 0L)
            {
                return glink;
            } 

            // This is essentially the resolver from the ppc64 ELF ABI.
            // At entry, r12 holds the address of the symbol resolver stub
            // for the target routine and the argument registers hold the
            // arguments for the target routine.
            //
            // This stub is PIC, so first get the PC of label 1 into r11.
            // Other things will be relative to this.
            glink.AddUint32(ctxt.Arch, 0x7c0802a6UL); // mflr r0
            glink.AddUint32(ctxt.Arch, 0x429f0005UL); // bcl 20,31,1f
            glink.AddUint32(ctxt.Arch, 0x7d6802a6UL); // 1: mflr r11
            glink.AddUint32(ctxt.Arch, 0x7c0803a6UL); // mtlf r0

            // Compute the .plt array index from the entry point address.
            // Because this is PIC, everything is relative to label 1b (in
            // r11):
            //   r0 = ((r12 - r11) - (res_0 - r11)) / 4 = (r12 - res_0) / 4
            glink.AddUint32(ctxt.Arch, 0x3800ffd0UL); // li r0,-(res_0-1b)=-48
            glink.AddUint32(ctxt.Arch, 0x7c006214UL); // add r0,r0,r12
            glink.AddUint32(ctxt.Arch, 0x7c0b0050UL); // sub r0,r0,r11
            glink.AddUint32(ctxt.Arch, 0x7800f082UL); // srdi r0,r0,2

            // r11 = address of the first byte of the PLT
            var r = glink.AddRel();

            r.Off = int32(glink.Size);
            r.Sym = ctxt.Syms.Lookup(".plt", 0L);
            r.Siz = 8L;
            r.Type = objabi.R_ADDRPOWER;

            glink.AddUint32(ctxt.Arch, 0x3d600000UL); // addis r11,0,.plt@ha
            glink.AddUint32(ctxt.Arch, 0x396b0000UL); // addi r11,r11,.plt@l

            // Load r12 = dynamic resolver address and r11 = DSO
            // identifier from the first two doublewords of the PLT.
            glink.AddUint32(ctxt.Arch, 0xe98b0000UL); // ld r12,0(r11)
            glink.AddUint32(ctxt.Arch, 0xe96b0008UL); // ld r11,8(r11)

            // Jump to the dynamic resolver
            glink.AddUint32(ctxt.Arch, 0x7d8903a6UL); // mtctr r12
            glink.AddUint32(ctxt.Arch, 0x4e800420UL); // bctr

            // The symbol resolvers must immediately follow.
            //   res_0:

            // Add DT_PPC64_GLINK .dynamic entry, which points to 32 bytes
            // before the first symbol resolver stub.
            var s = ctxt.Syms.Lookup(".dynamic", 0L);

            ld.Elfwritedynentsymplus(ctxt, s, ld.DT_PPC64_GLINK, glink, glink.Size - 32L);

            return glink;
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
            foreach (var (_, sect) in ld.Segtext.Sections)
            {
                ctxt.Out.SeekSet(int64(sect.Vaddr - ld.Segtext.Vaddr + ld.Segtext.Fileoff)); 
                // Handle additional text sections with Codeblk
                if (sect.Name == ".text")
                {
                    ld.Codeblk(ctxt, int64(sect.Vaddr), int64(sect.Length));
                }
                else
                {
                    ld.Datblk(ctxt, int64(sect.Vaddr), int64(sect.Length));
                }
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
