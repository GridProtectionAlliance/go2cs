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

// package s390x -- go2cs converted at 2022 March 06 23:20:19 UTC
// import "cmd/link/internal/s390x" ==> using s390x = go.cmd.link.@internal.s390x_package
// Original source: C:\Program Files\Go\src\cmd\link\internal\s390x\asm.go
using objabi = go.cmd.@internal.objabi_package;
using sys = go.cmd.@internal.sys_package;
using ld = go.cmd.link.@internal.ld_package;
using loader = go.cmd.link.@internal.loader_package;
using sym = go.cmd.link.@internal.sym_package;
using elf = go.debug.elf_package;

namespace go.cmd.link.@internal;

public static partial class s390x_package {

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
private static void gentext(ptr<ld.Link> _addr_ctxt, ptr<loader.Loader> _addr_ldr) {
    ref ld.Link ctxt = ref _addr_ctxt.val;
    ref loader.Loader ldr = ref _addr_ldr.val;

    var (initfunc, addmoduledata) = ld.PrepareAddmoduledata(ctxt);
    if (initfunc == null) {
        return ;
    }
    initfunc.AddUint8(0xc0);
    initfunc.AddUint8(0x20);
    initfunc.AddSymRef(ctxt.Arch, ctxt.Moduledata, 6, objabi.R_PCREL, 4);
    var r1 = initfunc.Relocs();
    ldr.SetRelocVariant(initfunc.Sym(), r1.Count() - 1, sym.RV_390_DBL); 

    // jg <runtime.addmoduledata[@plt]>
    initfunc.AddUint8(0xc0);
    initfunc.AddUint8(0xf4);
    initfunc.AddSymRef(ctxt.Arch, addmoduledata, 6, objabi.R_CALL, 4);
    var r2 = initfunc.Relocs();
    ldr.SetRelocVariant(initfunc.Sym(), r2.Count() - 1, sym.RV_390_DBL); 

    // undef (for debugging)
    initfunc.AddUint32(ctxt.Arch, 0);

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

    if (r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_390_12) || r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_390_GOT12)) 
        ldr.Errorf(s, "s390x 12-bit relocations have not been implemented (relocation type %d)", r.Type() - objabi.ElfRelocOffset);
        return false;
    else if (r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_390_8) || r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_390_16) || r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_390_32) || r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_390_64)) 
        if (targType == sym.SDYNIMPORT) {
            ldr.Errorf(s, "unexpected R_390_nn relocation for dynamic symbol %s", ldr.SymName(targ));
        }
        var su = ldr.MakeSymbolUpdater(s);
        su.SetRelocType(rIdx, objabi.R_ADDR);
        return true;
    else if (r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_390_PC16) || r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_390_PC32) || r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_390_PC64)) 
        if (targType == sym.SDYNIMPORT) {
            ldr.Errorf(s, "unexpected R_390_PCnn relocation for dynamic symbol %s", ldr.SymName(targ));
        }
        if ((targType == 0 || targType == sym.SXREF) && !ldr.AttrVisibilityHidden(targ)) {
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
        if (targType == sym.SDYNIMPORT) {
            addpltsym(_addr_target, _addr_ldr, _addr_syms, targ);
            r.SetSym(syms.PLT);
            su.SetRelocAdd(rIdx, r.Add() + int64(ldr.SymPlt(targ)));
        }
        return true;
    else if (r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_390_PLT32) || r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_390_PLT64)) 
        su = ldr.MakeSymbolUpdater(s);
        su.SetRelocType(rIdx, objabi.R_PCREL);
        su.SetRelocAdd(rIdx, r.Add() + int64(r.Siz()));
        if (targType == sym.SDYNIMPORT) {
            addpltsym(_addr_target, _addr_ldr, _addr_syms, targ);
            r.SetSym(syms.PLT);
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
        if (targType == sym.SDYNIMPORT) {
            ldr.Errorf(s, "unexpected R_390_GOTOFF relocation for dynamic symbol %s", ldr.SymName(targ));
        }
        su = ldr.MakeSymbolUpdater(s);
        su.SetRelocType(rIdx, objabi.R_GOTOFF);
        return true;
    else if (r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_390_GOTPC)) 
        su = ldr.MakeSymbolUpdater(s);
        su.SetRelocType(rIdx, objabi.R_PCREL);
        r.SetSym(syms.GOT);
        su.SetRelocAdd(rIdx, r.Add() + int64(r.Siz()));
        return true;
    else if (r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_390_PC16DBL) || r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_390_PC32DBL)) 
        su = ldr.MakeSymbolUpdater(s);
        su.SetRelocType(rIdx, objabi.R_PCREL);
        ldr.SetRelocVariant(s, rIdx, sym.RV_390_DBL);
        su.SetRelocAdd(rIdx, r.Add() + int64(r.Siz()));
        if (targType == sym.SDYNIMPORT) {
            ldr.Errorf(s, "unexpected R_390_PCnnDBL relocation for dynamic symbol %s", ldr.SymName(targ));
        }
        return true;
    else if (r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_390_GOTPCDBL)) 
        su = ldr.MakeSymbolUpdater(s);
        su.SetRelocType(rIdx, objabi.R_PCREL);
        ldr.SetRelocVariant(s, rIdx, sym.RV_390_DBL);
        r.SetSym(syms.GOT);
        su.SetRelocAdd(rIdx, r.Add() + int64(r.Siz()));
        return true;
    else if (r.Type() == objabi.ElfRelocOffset + objabi.RelocType(elf.R_390_GOTENT)) 
        ld.AddGotSym(target, ldr, syms, targ, uint32(elf.R_390_GLOB_DAT));
        su = ldr.MakeSymbolUpdater(s);
        su.SetRelocType(rIdx, objabi.R_PCREL);
        ldr.SetRelocVariant(s, rIdx, sym.RV_390_DBL);
        r.SetSym(syms.GOT);
        su.SetRelocAdd(rIdx, r.Add() + int64(ldr.SymGot(targ)) + int64(r.Siz()));
        return true;
    else 
        if (r.Type() >= objabi.ElfRelocOffset) {
            ldr.Errorf(s, "unexpected relocation type %d", r.Type());
            return false;
        }
    // Handle references to ELF symbols from our own object files.
    if (targType != sym.SDYNIMPORT) {
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

    if (r.Type == objabi.R_TLS_LE) 
        switch (siz) {
            case 4: 
                // WARNING - silently ignored by linker in ELF64
                @out.Write64(uint64(elf.R_390_TLS_LE32) | uint64(elfsym) << 32);
                break;
            case 8: 
                // WARNING - silently ignored by linker in ELF32
                @out.Write64(uint64(elf.R_390_TLS_LE64) | uint64(elfsym) << 32);
                break;
            default: 
                return false;
                break;
        }
    else if (r.Type == objabi.R_TLS_IE) 
        switch (siz) {
            case 4: 
                @out.Write64(uint64(elf.R_390_TLS_IEENT) | uint64(elfsym) << 32);
                break;
            default: 
                return false;
                break;
        }
    else if (r.Type == objabi.R_ADDR || r.Type == objabi.R_DWARFSECREF) 
        switch (siz) {
            case 4: 
                @out.Write64(uint64(elf.R_390_32) | uint64(elfsym) << 32);
                break;
            case 8: 
                @out.Write64(uint64(elf.R_390_64) | uint64(elfsym) << 32);
                break;
            default: 
                return false;
                break;
        }
    else if (r.Type == objabi.R_GOTPCREL) 
        if (siz == 4) {
            @out.Write64(uint64(elf.R_390_GOTENT) | uint64(elfsym) << 32);
        }
        else
 {
            return false;
        }
    else if (r.Type == objabi.R_PCREL || r.Type == objabi.R_PCRELDBL || r.Type == objabi.R_CALL) 
        var elfrel = elf.R_390_NONE;
        var rVariant = ldr.RelocVariant(s, ri);
        var isdbl = rVariant & sym.RV_TYPE_MASK == sym.RV_390_DBL; 
        // TODO(mundaym): all DBL style relocations should be
        // signalled using the variant - see issue 14218.

        if (r.Type == objabi.R_PCRELDBL || r.Type == objabi.R_CALL) 
            isdbl = true;
                if (ldr.SymType(r.Xsym) == sym.SDYNIMPORT && (ldr.SymElfType(r.Xsym) == elf.STT_FUNC || r.Type == objabi.R_CALL)) {
            if (isdbl) {
                switch (siz) {
                    case 2: 
                        elfrel = elf.R_390_PLT16DBL;
                        break;
                    case 4: 
                        elfrel = elf.R_390_PLT32DBL;
                        break;
                }

            }
            else
 {
                switch (siz) {
                    case 4: 
                        elfrel = elf.R_390_PLT32;
                        break;
                    case 8: 
                        elfrel = elf.R_390_PLT64;
                        break;
                }

            }

        }
        else
 {
            if (isdbl) {
                switch (siz) {
                    case 2: 
                        elfrel = elf.R_390_PC16DBL;
                        break;
                    case 4: 
                        elfrel = elf.R_390_PC32DBL;
                        break;
                }

            }
            else
 {
                switch (siz) {
                    case 2: 
                        elfrel = elf.R_390_PC16;
                        break;
                    case 4: 
                        elfrel = elf.R_390_PC32;
                        break;
                    case 8: 
                        elfrel = elf.R_390_PC64;
                        break;
                }

            }

        }
        if (elfrel == elf.R_390_NONE) {
            return false; // unsupported size/dbl combination
        }
        @out.Write64(uint64(elfrel) | uint64(elfsym) << 32);
    else 
        return false;
        @out.Write64(uint64(r.Xadd));
    return true;

}

private static void elfsetupplt(ptr<ld.Link> _addr_ctxt, ptr<loader.SymbolBuilder> _addr_plt, ptr<loader.SymbolBuilder> _addr_got, loader.Sym dynamic) {
    ref ld.Link ctxt = ref _addr_ctxt.val;
    ref loader.SymbolBuilder plt = ref _addr_plt.val;
    ref loader.SymbolBuilder got = ref _addr_got.val;

    if (plt.Size() == 0) { 
        // stg     %r1,56(%r15)
        plt.AddUint8(0xe3);
        plt.AddUint8(0x10);
        plt.AddUint8(0xf0);
        plt.AddUint8(0x38);
        plt.AddUint8(0x00);
        plt.AddUint8(0x24); 
        // larl    %r1,_GLOBAL_OFFSET_TABLE_
        plt.AddUint8(0xc0);
        plt.AddUint8(0x10);
        plt.AddSymRef(ctxt.Arch, got.Sym(), 6, objabi.R_PCRELDBL, 4); 
        // mvc     48(8,%r15),8(%r1)
        plt.AddUint8(0xd2);
        plt.AddUint8(0x07);
        plt.AddUint8(0xf0);
        plt.AddUint8(0x30);
        plt.AddUint8(0x10);
        plt.AddUint8(0x08); 
        // lg      %r1,16(%r1)
        plt.AddUint8(0xe3);
        plt.AddUint8(0x10);
        plt.AddUint8(0x10);
        plt.AddUint8(0x10);
        plt.AddUint8(0x00);
        plt.AddUint8(0x04); 
        // br      %r1
        plt.AddUint8(0x07);
        plt.AddUint8(0xf1); 
        // nopr    %r0
        plt.AddUint8(0x07);
        plt.AddUint8(0x00); 
        // nopr    %r0
        plt.AddUint8(0x07);
        plt.AddUint8(0x00); 
        // nopr    %r0
        plt.AddUint8(0x07);
        plt.AddUint8(0x00); 

        // assume got->size == 0 too
        got.AddAddrPlus(ctxt.Arch, dynamic, 0);

        got.AddUint64(ctxt.Arch, 0);
        got.AddUint64(ctxt.Arch, 0);

    }
}

private static bool machoreloc1(ptr<sys.Arch> _addr__p0, ptr<ld.OutBuf> _addr__p0, ptr<loader.Loader> _addr__p0, loader.Sym _p0, loader.ExtReloc _p0, long _p0) {
    ref sys.Arch _p0 = ref _addr__p0.val;
    ref ld.OutBuf _p0 = ref _addr__p0.val;
    ref loader.Loader _p0 = ref _addr__p0.val;

    return false;
}

private static (long, nint, bool) archreloc(ptr<ld.Target> _addr_target, ptr<loader.Loader> _addr_ldr, ptr<ld.ArchSyms> _addr_syms, loader.Reloc r, loader.Sym s, long val) {
    long o = default;
    nint nExtReloc = default;
    bool ok = default;
    ref ld.Target target = ref _addr_target.val;
    ref loader.Loader ldr = ref _addr_ldr.val;
    ref ld.ArchSyms syms = ref _addr_syms.val;

    return (val, 0, false);
}

private static long archrelocvariant(ptr<ld.Target> _addr_target, ptr<loader.Loader> _addr_ldr, loader.Reloc r, sym.RelocVariant rv, loader.Sym s, long t, slice<byte> p) {
    ref ld.Target target = ref _addr_target.val;
    ref loader.Loader ldr = ref _addr_ldr.val;


    if (rv & sym.RV_TYPE_MASK == sym.RV_NONE) 
        return t;
    else if (rv & sym.RV_TYPE_MASK == sym.RV_390_DBL) 
        if (t & 1 != 0) {
            ldr.Errorf(s, "%s+%v is not 2-byte aligned", ldr.SymName(r.Sym()), ldr.SymValue(r.Sym()));
        }
        return t >> 1;
    else 
        ldr.Errorf(s, "unexpected relocation variant %d", rv);
        return t;
    
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
        var got = ldr.MakeSymbolUpdater(syms.GOT);
        var rela = ldr.MakeSymbolUpdater(syms.RelaPLT);
        if (plt.Size() == 0) {
            panic("plt is not set up");
        }
        plt.AddUint8(0xc0);
        plt.AddUint8(0x10);
        plt.AddPCRelPlus(target.Arch, got.Sym(), got.Size() + 6);
        var pltrelocs = plt.Relocs();
        ldr.SetRelocVariant(plt.Sym(), pltrelocs.Count() - 1, sym.RV_390_DBL); 

        // add to got: pointer to current pos in plt
        got.AddAddrPlus(target.Arch, plt.Sym(), plt.Size() + 8); // weird but correct
        // lg      %r1,0(%r1)
        plt.AddUint8(0xe3);
        plt.AddUint8(0x10);
        plt.AddUint8(0x10);
        plt.AddUint8(0x00);
        plt.AddUint8(0x00);
        plt.AddUint8(0x04); 
        // br      %r1
        plt.AddUint8(0x07);
        plt.AddUint8(0xf1); 
        // basr    %r1,%r0
        plt.AddUint8(0x0d);
        plt.AddUint8(0x10); 
        // lgf     %r1,12(%r1)
        plt.AddUint8(0xe3);
        plt.AddUint8(0x10);
        plt.AddUint8(0x10);
        plt.AddUint8(0x0c);
        plt.AddUint8(0x00);
        plt.AddUint8(0x14); 
        // jg .plt
        plt.AddUint8(0xc0);
        plt.AddUint8(0xf4);

        plt.AddUint32(target.Arch, uint32(-((plt.Size() - 2) >> 1))); // roll-your-own relocation
        //.plt index
        plt.AddUint32(target.Arch, uint32(rela.Size())); // rela size before current entry

        // rela
        rela.AddAddrPlus(target.Arch, got.Sym(), got.Size() - 8);

        var sDynid = ldr.SymDynid(s);
        rela.AddUint64(target.Arch, elf.R_INFO(uint32(sDynid), uint32(elf.R_390_JMP_SLOT)));
        rela.AddUint64(target.Arch, 0);

        ldr.SetPlt(s, int32(plt.Size() - 32));


    }
    else
 {
        ldr.Errorf(s, "addpltsym: unsupported binary format");
    }
});

} // end s390x_package
