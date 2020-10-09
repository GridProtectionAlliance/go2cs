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

// package s390x -- go2cs converted at 2020 October 09 05:51:06 UTC
// import "cmd/oldlink/internal/s390x" ==> using s390x = go.cmd.oldlink.@internal.s390x_package
// Original source: C:\Go\src\cmd\oldlink\internal\s390x\asm.go
using objabi = go.cmd.@internal.objabi_package;
using sys = go.cmd.@internal.sys_package;
using ld = go.cmd.oldlink.@internal.ld_package;
using sym = go.cmd.oldlink.@internal.sym_package;
using elf = go.debug.elf_package;
using fmt = go.fmt_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace oldlink {
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

            // larl %r2, <local.moduledata>
            initfunc.AddUint8(0xc0UL);
            initfunc.AddUint8(0x20UL);
            var lmd = initfunc.AddRel();
            lmd.InitExt();
            lmd.Off = int32(initfunc.Size);
            lmd.Siz = 4L;
            lmd.Sym = ctxt.Moduledata;
            lmd.Type = objabi.R_PCREL;
            lmd.Variant = sym.RV_390_DBL;
            lmd.Add = 2L + int64(lmd.Siz);
            initfunc.AddUint32(ctxt.Arch, 0L); 

            // jg <runtime.addmoduledata[@plt]>
            initfunc.AddUint8(0xc0UL);
            initfunc.AddUint8(0xf4UL);
            var rel = initfunc.AddRel();
            rel.InitExt();
            rel.Off = int32(initfunc.Size);
            rel.Siz = 4L;
            rel.Sym = ctxt.Syms.Lookup("runtime.addmoduledata", 0L);
            rel.Type = objabi.R_CALL;
            rel.Variant = sym.RV_390_DBL;
            rel.Add = 2L + int64(rel.Siz);
            initfunc.AddUint32(ctxt.Arch, 0L); 

            // undef (for debugging)
            initfunc.AddUint32(ctxt.Arch, 0L);
            if (ctxt.BuildMode == ld.BuildModePlugin)
            {
                ctxt.Textp = append(ctxt.Textp, addmoduledata);
            }
            ctxt.Textp = append(ctxt.Textp, initfunc);
            var initarray_entry = ctxt.Syms.Lookup("go.link.addmoduledatainit", 0L);
            initarray_entry.Attr |= sym.AttrLocal;
            initarray_entry.Attr |= sym.AttrReachable;
            initarray_entry.Type = sym.SINITARR;
            initarray_entry.AddAddr(ctxt.Arch, initfunc);

        }

        private static bool adddynrel(ptr<ld.Link> _addr_ctxt, ptr<sym.Symbol> _addr_s, ptr<sym.Reloc> _addr_r)
        {
            ref ld.Link ctxt = ref _addr_ctxt.val;
            ref sym.Symbol s = ref _addr_s.val;
            ref sym.Reloc r = ref _addr_r.val;

            var targ = r.Sym;
            r.InitExt();


            if (r.Type == objabi.ElfRelocOffset + objabi.RelocType(elf.R_390_12) || r.Type == objabi.ElfRelocOffset + objabi.RelocType(elf.R_390_GOT12)) 
                ld.Errorf(s, "s390x 12-bit relocations have not been implemented (relocation type %d)", r.Type - objabi.ElfRelocOffset);
                return false;
            else if (r.Type == objabi.ElfRelocOffset + objabi.RelocType(elf.R_390_8) || r.Type == objabi.ElfRelocOffset + objabi.RelocType(elf.R_390_16) || r.Type == objabi.ElfRelocOffset + objabi.RelocType(elf.R_390_32) || r.Type == objabi.ElfRelocOffset + objabi.RelocType(elf.R_390_64)) 
                if (targ.Type == sym.SDYNIMPORT)
                {
                    ld.Errorf(s, "unexpected R_390_nn relocation for dynamic symbol %s", targ.Name);
                }

                r.Type = objabi.R_ADDR;
                return true;
            else if (r.Type == objabi.ElfRelocOffset + objabi.RelocType(elf.R_390_PC16) || r.Type == objabi.ElfRelocOffset + objabi.RelocType(elf.R_390_PC32) || r.Type == objabi.ElfRelocOffset + objabi.RelocType(elf.R_390_PC64)) 
                if (targ.Type == sym.SDYNIMPORT)
                {
                    ld.Errorf(s, "unexpected R_390_PCnn relocation for dynamic symbol %s", targ.Name);
                } 
                // TODO(mwhudson): the test of VisibilityHidden here probably doesn't make
                // sense and should be removed when someone has thought about it properly.
                if ((targ.Type == 0L || targ.Type == sym.SXREF) && !targ.Attr.VisibilityHidden())
                {
                    ld.Errorf(s, "unknown symbol %s in pcrel", targ.Name);
                }

                r.Type = objabi.R_PCREL;
                r.Add += int64(r.Siz);
                return true;
            else if (r.Type == objabi.ElfRelocOffset + objabi.RelocType(elf.R_390_GOT16) || r.Type == objabi.ElfRelocOffset + objabi.RelocType(elf.R_390_GOT32) || r.Type == objabi.ElfRelocOffset + objabi.RelocType(elf.R_390_GOT64)) 
                ld.Errorf(s, "unimplemented S390x relocation: %v", r.Type - objabi.ElfRelocOffset);
                return true;
            else if (r.Type == objabi.ElfRelocOffset + objabi.RelocType(elf.R_390_PLT16DBL) || r.Type == objabi.ElfRelocOffset + objabi.RelocType(elf.R_390_PLT32DBL)) 
                r.Type = objabi.R_PCREL;
                r.Variant = sym.RV_390_DBL;
                r.Add += int64(r.Siz);
                if (targ.Type == sym.SDYNIMPORT)
                {
                    addpltsym(_addr_ctxt, _addr_targ);
                    r.Sym = ctxt.Syms.Lookup(".plt", 0L);
                    r.Add += int64(targ.Plt());
                }

                return true;
            else if (r.Type == objabi.ElfRelocOffset + objabi.RelocType(elf.R_390_PLT32) || r.Type == objabi.ElfRelocOffset + objabi.RelocType(elf.R_390_PLT64)) 
                r.Type = objabi.R_PCREL;
                r.Add += int64(r.Siz);
                if (targ.Type == sym.SDYNIMPORT)
                {
                    addpltsym(_addr_ctxt, _addr_targ);
                    r.Sym = ctxt.Syms.Lookup(".plt", 0L);
                    r.Add += int64(targ.Plt());
                }

                return true;
            else if (r.Type == objabi.ElfRelocOffset + objabi.RelocType(elf.R_390_COPY)) 
                ld.Errorf(s, "unimplemented S390x relocation: %v", r.Type - objabi.ElfRelocOffset);
                return false;
            else if (r.Type == objabi.ElfRelocOffset + objabi.RelocType(elf.R_390_GLOB_DAT)) 
                ld.Errorf(s, "unimplemented S390x relocation: %v", r.Type - objabi.ElfRelocOffset);
                return false;
            else if (r.Type == objabi.ElfRelocOffset + objabi.RelocType(elf.R_390_JMP_SLOT)) 
                ld.Errorf(s, "unimplemented S390x relocation: %v", r.Type - objabi.ElfRelocOffset);
                return false;
            else if (r.Type == objabi.ElfRelocOffset + objabi.RelocType(elf.R_390_RELATIVE)) 
                ld.Errorf(s, "unimplemented S390x relocation: %v", r.Type - objabi.ElfRelocOffset);
                return false;
            else if (r.Type == objabi.ElfRelocOffset + objabi.RelocType(elf.R_390_GOTOFF)) 
                if (targ.Type == sym.SDYNIMPORT)
                {
                    ld.Errorf(s, "unexpected R_390_GOTOFF relocation for dynamic symbol %s", targ.Name);
                }

                r.Type = objabi.R_GOTOFF;
                return true;
            else if (r.Type == objabi.ElfRelocOffset + objabi.RelocType(elf.R_390_GOTPC)) 
                r.Type = objabi.R_PCREL;
                r.Sym = ctxt.Syms.Lookup(".got", 0L);
                r.Add += int64(r.Siz);
                return true;
            else if (r.Type == objabi.ElfRelocOffset + objabi.RelocType(elf.R_390_PC16DBL) || r.Type == objabi.ElfRelocOffset + objabi.RelocType(elf.R_390_PC32DBL)) 
                r.Type = objabi.R_PCREL;
                r.Variant = sym.RV_390_DBL;
                r.Add += int64(r.Siz);
                if (targ.Type == sym.SDYNIMPORT)
                {
                    ld.Errorf(s, "unexpected R_390_PCnnDBL relocation for dynamic symbol %s", targ.Name);
                }

                return true;
            else if (r.Type == objabi.ElfRelocOffset + objabi.RelocType(elf.R_390_GOTPCDBL)) 
                r.Type = objabi.R_PCREL;
                r.Variant = sym.RV_390_DBL;
                r.Sym = ctxt.Syms.Lookup(".got", 0L);
                r.Add += int64(r.Siz);
                return true;
            else if (r.Type == objabi.ElfRelocOffset + objabi.RelocType(elf.R_390_GOTENT)) 
                addgotsym(_addr_ctxt, _addr_targ);

                r.Type = objabi.R_PCREL;
                r.Variant = sym.RV_390_DBL;
                r.Sym = ctxt.Syms.Lookup(".got", 0L);
                r.Add += int64(targ.Got());
                r.Add += int64(r.Siz);
                return true;
            else 
                if (r.Type >= objabi.ElfRelocOffset)
                {
                    ld.Errorf(s, "unexpected relocation type %d", r.Type);
                    return false;
                } 

                // Handle relocations found in ELF object files.
            // Handle references to ELF symbols from our own object files.
            if (targ.Type != sym.SDYNIMPORT)
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

            var elfsym = r.Xsym.ElfsymForReloc();

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
            else if (r.Type == objabi.R_ADDR) 
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

        private static void elfsetupplt(ptr<ld.Link> _addr_ctxt)
        {
            ref ld.Link ctxt = ref _addr_ctxt.val;

            var plt = ctxt.Syms.Lookup(".plt", 0L);
            var got = ctxt.Syms.Lookup(".got", 0L);
            if (plt.Size == 0L)
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
                plt.AddPCRelPlus(ctxt.Arch, got, 6L); 
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
                got.AddAddrPlus(ctxt.Arch, ctxt.Syms.Lookup(".dynamic", 0L), 0L);

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

        private static (long, bool) archreloc(ptr<ld.Link> _addr_ctxt, ptr<sym.Reloc> _addr_r, ptr<sym.Symbol> _addr_s, long val)
        {
            long _p0 = default;
            bool _p0 = default;
            ref ld.Link ctxt = ref _addr_ctxt.val;
            ref sym.Reloc r = ref _addr_r.val;
            ref sym.Symbol s = ref _addr_s.val;

            if (ctxt.LinkMode == ld.LinkExternal)
            {
                return (val, false);
            }


            if (r.Type == objabi.R_CONST) 
                return (r.Add, true);
            else if (r.Type == objabi.R_GOTOFF) 
                return (ld.Symaddr(r.Sym) + r.Add - ld.Symaddr(ctxt.Syms.Lookup(".got", 0L)), true);
                        return (val, false);

        }

        private static long archrelocvariant(ptr<ld.Link> _addr_ctxt, ptr<sym.Reloc> _addr_r, ptr<sym.Symbol> _addr_s, long t)
        {
            ref ld.Link ctxt = ref _addr_ctxt.val;
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
                var got = ctxt.Syms.Lookup(".got", 0L);
                var rela = ctxt.Syms.Lookup(".rela.plt", 0L);
                if (plt.Size == 0L)
                {
                    elfsetupplt(_addr_ctxt);
                } 
                // larl    %r1,_GLOBAL_OFFSET_TABLE_+index
                plt.AddUint8(0xc0UL);
                plt.AddUint8(0x10UL);
                plt.AddPCRelPlus(ctxt.Arch, got, got.Size + 6L); // need variant?

                // add to got: pointer to current pos in plt
                got.AddAddrPlus(ctxt.Arch, plt, plt.Size + 8L); // weird but correct
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

                plt.AddUint32(ctxt.Arch, uint32(-((plt.Size - 2L) >> (int)(1L)))); // roll-your-own relocation
                //.plt index
                plt.AddUint32(ctxt.Arch, uint32(rela.Size)); // rela size before current entry

                // rela
                rela.AddAddrPlus(ctxt.Arch, got, got.Size - 8L);

                rela.AddUint64(ctxt.Arch, ld.ELF64_R_INFO(uint32(s.Dynid), uint32(elf.R_390_JMP_SLOT)));
                rela.AddUint64(ctxt.Arch, 0L);

                s.SetPlt(int32(plt.Size - 32L));


            }
            else
            {
                ld.Errorf(s, "addpltsym: unsupported binary format");
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
            got.AddUint64(ctxt.Arch, 0L);

            if (ctxt.IsELF)
            {
                var rela = ctxt.Syms.Lookup(".rela", 0L);
                rela.AddAddrPlus(ctxt.Arch, got, int64(s.Got()));
                rela.AddUint64(ctxt.Arch, ld.ELF64_R_INFO(uint32(s.Dynid), uint32(elf.R_390_GLOB_DAT)));
                rela.AddUint64(ctxt.Arch, 0L);
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
                if (!ctxt.IsELF)
                {
                    ld.Errorf(null, "unsupported executable format");
                }

                symo = uint32(ld.Segdwarf.Fileoff + ld.Segdwarf.Filelen);
                symo = uint32(ld.Rnd(int64(symo), int64(ld.FlagRound.val)));

                ctxt.Out.SeekSet(int64(symo));
                ld.Asmelfsym(ctxt);
                ctxt.Out.Flush();
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
                        ctxt.Out.Flush();
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
