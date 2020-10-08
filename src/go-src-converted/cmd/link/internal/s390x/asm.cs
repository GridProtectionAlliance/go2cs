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

// package s390x -- go2cs converted at 2020 October 08 04:37:35 UTC
// import "cmd/link/internal/s390x" ==> using s390x = go.cmd.link.@internal.s390x_package
// Original source: C:\Go\src\cmd\link\internal\s390x\asm.go
using objabi = go.cmd.@internal.objabi_package;
using sys = go.cmd.@internal.sys_package;
using ld = go.cmd.link.@internal.ld_package;
using loader = go.cmd.link.@internal.loader_package;
using sym = go.cmd.link.@internal.sym_package;
using elf = go.debug.elf_package;
using fmt = go.fmt_package;
using sync = go.sync_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace link {
namespace @internal
{
    public static partial class s390x_package
    {
        // gentext generates assembly to append the local moduledata to the global
        // moduledata linked list at initialization time. This is only done if the runtime
        // is in a different module.
        //
        // <go.link.addmoduledata>:
        //     larl  %r2, <local.moduledata>
        //     jg    <runtime.addmoduledata@plt>
        //    undef
        //
        // The job of appending the moduledata is delegated to runtime.addmoduledata.
        private static void gentext2(ptr<ld.Link> _addr_ctxt, ptr<loader.Loader> _addr_ldr)
        {
            ref ld.Link ctxt = ref _addr_ctxt.val;
            ref loader.Loader ldr = ref _addr_ldr.val;

            var (initfunc, addmoduledata) = ld.PrepareAddmoduledata(ctxt);
            if (initfunc == null)
            {
                return ;
            }
            initfunc.AddUint8(0xc0UL);
            initfunc.AddUint8(0x20UL);
            initfunc.AddSymRef(ctxt.Arch, ctxt.Moduledata2, 6L, objabi.R_PCREL, 4L);
            var r1 = initfunc.Relocs();
            ldr.SetRelocVariant(initfunc.Sym(), r1.Count() - 1L, sym.RV_390_DBL); 

            // jg <runtime.addmoduledata[@plt]>
            initfunc.AddUint8(0xc0UL);
            initfunc.AddUint8(0xf4UL);
            initfunc.AddSymRef(ctxt.Arch, addmoduledata, 6L, objabi.R_CALL, 4L);
            var r2 = initfunc.Relocs();
            ldr.SetRelocVariant(initfunc.Sym(), r2.Count() - 1L, sym.RV_390_DBL); 

            // undef (for debugging)
            initfunc.AddUint32(ctxt.Arch, 0L);

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


            if (r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_390_12) || r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_390_GOT12)) 
                ldr.Errorf(s, "s390x 12-bit relocations have not been implemented (relocation type %d)", r.Type() - objabi.ElfRelocOffset);
                return false;
            else if (r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_390_8) || r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_390_16) || r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_390_32) || r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_390_64)) 
                if (targType == sym.SDYNIMPORT)
                {
                    ldr.Errorf(s, "unexpected R_390_nn relocation for dynamic symbol %s", ldr.SymName(targ));
                }

                var su = ldr.MakeSymbolUpdater(s);
                su.SetRelocType(rIdx, objabi.R_ADDR);
                return true;
            else if (r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_390_PC16) || r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_390_PC32) || r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_390_PC64)) 
                if (targType == sym.SDYNIMPORT)
                {
                    ldr.Errorf(s, "unexpected R_390_PCnn relocation for dynamic symbol %s", ldr.SymName(targ));
                } 
                // TODO(mwhudson): the test of VisibilityHidden here probably doesn't make
                // sense and should be removed when someone has thought about it properly.
                if ((targType == 0L || targType == sym.SXREF) && !ldr.AttrVisibilityHidden(targ))
                {
                    ldr.Errorf(s, "unknown symbol %s in pcrel", ldr.SymName(targ));
                }

                su = ldr.MakeSymbolUpdater(s);
                su.SetRelocType(rIdx, objabi.R_PCREL);
                su.SetRelocAdd(rIdx, r.Add() + int64(r.Siz()));
                return true;
            else if (r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_390_GOT16) || r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_390_GOT32) || r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_390_GOT64)) 
                ldr.Errorf(s, "unimplemented S390x relocation: %v", r.Type() - objabi.ElfRelocOffset);
                return true;
            else if (r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_390_PLT16DBL) || r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_390_PLT32DBL)) 
                su = ldr.MakeSymbolUpdater(s);
                su.SetRelocType(rIdx, objabi.R_PCREL);
                ldr.SetRelocVariant(s, rIdx, sym.RV_390_DBL);
                su.SetRelocAdd(rIdx, r.Add() + int64(r.Siz()));
                if (targType == sym.SDYNIMPORT)
                {
                    addpltsym2(_addr_target, _addr_ldr, _addr_syms, targ);
                    r.SetSym(syms.PLT2);
                    su.SetRelocAdd(rIdx, r.Add() + int64(ldr.SymPlt(targ)));
                }

                return true;
            else if (r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_390_PLT32) || r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_390_PLT64)) 
                su = ldr.MakeSymbolUpdater(s);
                su.SetRelocType(rIdx, objabi.R_PCREL);
                su.SetRelocAdd(rIdx, r.Add() + int64(r.Siz()));
                if (targType == sym.SDYNIMPORT)
                {
                    addpltsym2(_addr_target, _addr_ldr, _addr_syms, targ);
                    r.SetSym(syms.PLT2);
                    su.SetRelocAdd(rIdx, r.Add() + int64(ldr.SymPlt(targ)));
                }

                return true;
            else if (r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_390_COPY)) 
                ldr.Errorf(s, "unimplemented S390x relocation: %v", r.Type() - objabi.ElfRelocOffset);
                return false;
            else if (r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_390_GLOB_DAT)) 
                ldr.Errorf(s, "unimplemented S390x relocation: %v", r.Type() - objabi.ElfRelocOffset);
                return false;
            else if (r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_390_JMP_SLOT)) 
                ldr.Errorf(s, "unimplemented S390x relocation: %v", r.Type() - objabi.ElfRelocOffset);
                return false;
            else if (r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_390_RELATIVE)) 
                ldr.Errorf(s, "unimplemented S390x relocation: %v", r.Type() - objabi.ElfRelocOffset);
                return false;
            else if (r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_390_GOTOFF)) 
                if (targType == sym.SDYNIMPORT)
                {
                    ldr.Errorf(s, "unexpected R_390_GOTOFF relocation for dynamic symbol %s", ldr.SymName(targ));
                }

                su = ldr.MakeSymbolUpdater(s);
                su.SetRelocType(rIdx, objabi.R_GOTOFF);
                return true;
            else if (r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_390_GOTPC)) 
                su = ldr.MakeSymbolUpdater(s);
                su.SetRelocType(rIdx, objabi.R_PCREL);
                r.SetSym(syms.GOT2);
                su.SetRelocAdd(rIdx, r.Add() + int64(r.Siz()));
                return true;
            else if (r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_390_PC16DBL) || r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_390_PC32DBL)) 
                su = ldr.MakeSymbolUpdater(s);
                su.SetRelocType(rIdx, objabi.R_PCREL);
                ldr.SetRelocVariant(s, rIdx, sym.RV_390_DBL);
                su.SetRelocAdd(rIdx, r.Add() + int64(r.Siz()));
                if (targType == sym.SDYNIMPORT)
                {
                    ldr.Errorf(s, "unexpected R_390_PCnnDBL relocation for dynamic symbol %s", ldr.SymName(targ));
                }

                return true;
            else if (r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_390_GOTPCDBL)) 
                su = ldr.MakeSymbolUpdater(s);
                su.SetRelocType(rIdx, objabi.R_PCREL);
                ldr.SetRelocVariant(s, rIdx, sym.RV_390_DBL);
                r.SetSym(syms.GOT2);
                su.SetRelocAdd(rIdx, r.Add() + int64(r.Siz()));
                return true;
            else if (r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_390_GOTENT)) 
                addgotsym2(_addr_target, _addr_ldr, _addr_syms, targ);
                su = ldr.MakeSymbolUpdater(s);
                su.SetRelocType(rIdx, objabi.R_PCREL);
                ldr.SetRelocVariant(s, rIdx, sym.RV_390_DBL);
                r.SetSym(syms.GOT2);
                su.SetRelocAdd(rIdx, r.Add() + int64(ldr.SymGot(targ)) + int64(r.Siz()));
                return true;
            else 
                if (r.Type() >= objabi.ElfRelocOffset)
                {
                    ldr.Errorf(s, "unexpected relocation type %d", r.Type());
                    return false;
                } 

                // Handle relocations found in ELF object files.
            // Handle references to ELF symbols from our own object files.
            if (targType != sym.SDYNIMPORT)
            {
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

            if (r.Type == objabi.R_TLS_LE) 
                switch (r.Siz)
                {
                    case 4L: 
                        // WARNING - silently ignored by linker in ELF64
                        ctxt.Out.Write64(uint64(elf.R_390_TLS_LE32) | uint64(elfsym) << (int)(32L));
                        break;
                    case 8L: 
                        // WARNING - silently ignored by linker in ELF32
                        ctxt.Out.Write64(uint64(elf.R_390_TLS_LE64) | uint64(elfsym) << (int)(32L));
                        break;
                    default: 
                        return false;
                        break;
                }
            else if (r.Type == objabi.R_TLS_IE) 
                switch (r.Siz)
                {
                    case 4L: 
                        ctxt.Out.Write64(uint64(elf.R_390_TLS_IEENT) | uint64(elfsym) << (int)(32L));
                        break;
                    default: 
                        return false;
                        break;
                }
            else if (r.Type == objabi.R_ADDR || r.Type == objabi.R_DWARFSECREF) 
                switch (r.Siz)
                {
                    case 4L: 
                        ctxt.Out.Write64(uint64(elf.R_390_32) | uint64(elfsym) << (int)(32L));
                        break;
                    case 8L: 
                        ctxt.Out.Write64(uint64(elf.R_390_64) | uint64(elfsym) << (int)(32L));
                        break;
                    default: 
                        return false;
                        break;
                }
            else if (r.Type == objabi.R_GOTPCREL) 
                if (r.Siz == 4L)
                {
                    ctxt.Out.Write64(uint64(elf.R_390_GOTENT) | uint64(elfsym) << (int)(32L));
                }
                else
                {
                    return false;
                }

            else if (r.Type == objabi.R_PCREL || r.Type == objabi.R_PCRELDBL || r.Type == objabi.R_CALL) 
                var elfrel = elf.R_390_NONE;
                var isdbl = r.Variant & sym.RV_TYPE_MASK == sym.RV_390_DBL; 
                // TODO(mundaym): all DBL style relocations should be
                // signalled using the variant - see issue 14218.

                if (r.Type == objabi.R_PCRELDBL || r.Type == objabi.R_CALL) 
                    isdbl = true;
                                if (r.Xsym.Type == sym.SDYNIMPORT && (r.Xsym.ElfType() == elf.STT_FUNC || r.Type == objabi.R_CALL))
                {
                    if (isdbl)
                    {
                        switch (r.Siz)
                        {
                            case 2L: 
                                elfrel = elf.R_390_PLT16DBL;
                                break;
                            case 4L: 
                                elfrel = elf.R_390_PLT32DBL;
                                break;
                        }

                    }
                    else
                    {
                        switch (r.Siz)
                        {
                            case 4L: 
                                elfrel = elf.R_390_PLT32;
                                break;
                            case 8L: 
                                elfrel = elf.R_390_PLT64;
                                break;
                        }

                    }

                }
                else
                {
                    if (isdbl)
                    {
                        switch (r.Siz)
                        {
                            case 2L: 
                                elfrel = elf.R_390_PC16DBL;
                                break;
                            case 4L: 
                                elfrel = elf.R_390_PC32DBL;
                                break;
                        }

                    }
                    else
                    {
                        switch (r.Siz)
                        {
                            case 2L: 
                                elfrel = elf.R_390_PC16;
                                break;
                            case 4L: 
                                elfrel = elf.R_390_PC32;
                                break;
                            case 8L: 
                                elfrel = elf.R_390_PC64;
                                break;
                        }

                    }

                }

                if (elfrel == elf.R_390_NONE)
                {
                    return false; // unsupported size/dbl combination
                }

                ctxt.Out.Write64(uint64(elfrel) | uint64(elfsym) << (int)(32L));
            else 
                return false;
                        ctxt.Out.Write64(uint64(r.Xadd));
            return true;

        }

        private static void elfsetupplt(ptr<ld.Link> _addr_ctxt, ptr<loader.SymbolBuilder> _addr_plt, ptr<loader.SymbolBuilder> _addr_got, loader.Sym dynamic)
        {
            ref ld.Link ctxt = ref _addr_ctxt.val;
            ref loader.SymbolBuilder plt = ref _addr_plt.val;
            ref loader.SymbolBuilder got = ref _addr_got.val;

            if (plt.Size() == 0L)
            { 
                // stg     %r1,56(%r15)
                plt.AddUint8(0xe3UL);
                plt.AddUint8(0x10UL);
                plt.AddUint8(0xf0UL);
                plt.AddUint8(0x38UL);
                plt.AddUint8(0x00UL);
                plt.AddUint8(0x24UL); 
                // larl    %r1,_GLOBAL_OFFSET_TABLE_
                plt.AddUint8(0xc0UL);
                plt.AddUint8(0x10UL);
                plt.AddSymRef(ctxt.Arch, got.Sym(), 6L, objabi.R_PCRELDBL, 4L); 
                // mvc     48(8,%r15),8(%r1)
                plt.AddUint8(0xd2UL);
                plt.AddUint8(0x07UL);
                plt.AddUint8(0xf0UL);
                plt.AddUint8(0x30UL);
                plt.AddUint8(0x10UL);
                plt.AddUint8(0x08UL); 
                // lg      %r1,16(%r1)
                plt.AddUint8(0xe3UL);
                plt.AddUint8(0x10UL);
                plt.AddUint8(0x10UL);
                plt.AddUint8(0x10UL);
                plt.AddUint8(0x00UL);
                plt.AddUint8(0x04UL); 
                // br      %r1
                plt.AddUint8(0x07UL);
                plt.AddUint8(0xf1UL); 
                // nopr    %r0
                plt.AddUint8(0x07UL);
                plt.AddUint8(0x00UL); 
                // nopr    %r0
                plt.AddUint8(0x07UL);
                plt.AddUint8(0x00UL); 
                // nopr    %r0
                plt.AddUint8(0x07UL);
                plt.AddUint8(0x00UL); 

                // assume got->size == 0 too
                got.AddAddrPlus(ctxt.Arch, dynamic, 0L);

                got.AddUint64(ctxt.Arch, 0L);
                got.AddUint64(ctxt.Arch, 0L);

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
                return (val, false);
            }


            if (r.Type == objabi.R_CONST) 
                return (r.Add, true);
            else if (r.Type == objabi.R_GOTOFF) 
                return (ld.Symaddr(r.Sym) + r.Add - ld.Symaddr(syms.GOT), true);
                        return (val, false);

        }

        private static long archrelocvariant(ptr<ld.Target> _addr_target, ptr<ld.ArchSyms> _addr_syms, ptr<sym.Reloc> _addr_r, ptr<sym.Symbol> _addr_s, long t)
        {
            ref ld.Target target = ref _addr_target.val;
            ref ld.ArchSyms syms = ref _addr_syms.val;
            ref sym.Reloc r = ref _addr_r.val;
            ref sym.Symbol s = ref _addr_s.val;


            if (r.Variant & sym.RV_TYPE_MASK == sym.RV_NONE) 
                return t;
            else if (r.Variant & sym.RV_TYPE_MASK == sym.RV_390_DBL) 
                if ((t & 1L) != 0L)
                {
                    ld.Errorf(s, "%s+%v is not 2-byte aligned", r.Sym.Name, r.Sym.Value);
                }

                return t >> (int)(1L);
            else 
                ld.Errorf(s, "unexpected relocation variant %d", r.Variant);
                return t;
            
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
                var got = ldr.MakeSymbolUpdater(syms.GOT2);
                var rela = ldr.MakeSymbolUpdater(syms.RelaPLT2);
                if (plt.Size() == 0L)
                {
                    panic("plt is not set up");
                } 
                // larl    %r1,_GLOBAL_OFFSET_TABLE_+index
                plt.AddUint8(0xc0UL);
                plt.AddUint8(0x10UL);
                plt.AddPCRelPlus(target.Arch, got.Sym(), got.Size() + 6L);
                var pltrelocs = plt.Relocs();
                ldr.SetRelocVariant(plt.Sym(), pltrelocs.Count() - 1L, sym.RV_390_DBL); 

                // add to got: pointer to current pos in plt
                got.AddAddrPlus(target.Arch, plt.Sym(), plt.Size() + 8L); // weird but correct
                // lg      %r1,0(%r1)
                plt.AddUint8(0xe3UL);
                plt.AddUint8(0x10UL);
                plt.AddUint8(0x10UL);
                plt.AddUint8(0x00UL);
                plt.AddUint8(0x00UL);
                plt.AddUint8(0x04UL); 
                // br      %r1
                plt.AddUint8(0x07UL);
                plt.AddUint8(0xf1UL); 
                // basr    %r1,%r0
                plt.AddUint8(0x0dUL);
                plt.AddUint8(0x10UL); 
                // lgf     %r1,12(%r1)
                plt.AddUint8(0xe3UL);
                plt.AddUint8(0x10UL);
                plt.AddUint8(0x10UL);
                plt.AddUint8(0x0cUL);
                plt.AddUint8(0x00UL);
                plt.AddUint8(0x14UL); 
                // jg .plt
                plt.AddUint8(0xc0UL);
                plt.AddUint8(0xf4UL);

                plt.AddUint32(target.Arch, uint32(-((plt.Size() - 2L) >> (int)(1L)))); // roll-your-own relocation
                //.plt index
                plt.AddUint32(target.Arch, uint32(rela.Size())); // rela size before current entry

                // rela
                rela.AddAddrPlus(target.Arch, got.Sym(), got.Size() - 8L);

                var sDynid = ldr.SymDynid(s);
                rela.AddUint64(target.Arch, ld.ELF64_R_INFO(uint32(sDynid), uint32(elf.R_390_JMP_SLOT)));
                rela.AddUint64(target.Arch, 0L);

                ldr.SetPlt(s, int32(plt.Size() - 32L));


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
                rela.AddUint64(target.Arch, ld.ELF64_R_INFO(uint32(ldr.SymDynid(s)), uint32(elf.R_390_GLOB_DAT)));
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
 
            /* output symbol table */
            ld.Symsize = 0L;

            ld.Lcsize = 0L;
            var symo = uint32(0L);
            if (!ld.FlagS.val)
            {
                if (!ctxt.IsELF)
                {
                    ld.Errorf(null, "unsupported executable format");
                }

                symo = uint32(ld.Segdwarf.Fileoff + ld.Segdwarf.Filelen);
                symo = uint32(ld.Rnd(int64(symo), int64(ld.FlagRound.val)));

                ctxt.Out.SeekSet(int64(symo));
                ld.Asmelfsym(ctxt);
                ctxt.Out.Write(ld.Elfstrdat);

                if (ctxt.LinkMode == ld.LinkExternal)
                {
                    ld.Elfemitreloc(ctxt);
                }

            }

            ctxt.Out.SeekSet(0L);

            if (ctxt.HeadType == objabi.Hlinux) 
                ld.Asmbelf(ctxt, int64(symo));
            else 
                ld.Errorf(null, "unsupported operating system");
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
