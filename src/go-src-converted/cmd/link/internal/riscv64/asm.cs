// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package riscv64 -- go2cs converted at 2022 March 06 23:20:16 UTC
// import "cmd/link/internal/riscv64" ==> using riscv64 = go.cmd.link.@internal.riscv64_package
// Original source: C:\Program Files\Go\src\cmd\link\internal\riscv64\asm.go
using riscv = go.cmd.@internal.obj.riscv_package;
using objabi = go.cmd.@internal.objabi_package;
using sys = go.cmd.@internal.sys_package;
using ld = go.cmd.link.@internal.ld_package;
using loader = go.cmd.link.@internal.loader_package;
using sym = go.cmd.link.@internal.sym_package;
using elf = go.debug.elf_package;
using fmt = go.fmt_package;
using log = go.log_package;
using sort = go.sort_package;
using System;


namespace go.cmd.link.@internal;

public static partial class riscv64_package {

    // fakeLabelName matches the RISCV_FAKE_LABEL_NAME from binutils.
private static readonly @string fakeLabelName = ".L0 ";



private static void gentext(ptr<ld.Link> _addr_ctxt, ptr<loader.Loader> _addr_ldr) {
    ref ld.Link ctxt = ref _addr_ctxt.val;
    ref loader.Loader ldr = ref _addr_ldr.val;

}

private static void genSymsLate(ptr<ld.Link> _addr_ctxt, ptr<loader.Loader> _addr_ldr) {
    ref ld.Link ctxt = ref _addr_ctxt.val;
    ref loader.Loader ldr = ref _addr_ldr.val;

    if (ctxt.LinkMode != ld.LinkExternal) {
        return ;
    }
    if (ctxt.Textp == null) {
        log.Fatal("genSymsLate called before Textp has been assigned");
    }
    slice<loader.Sym> hi20Syms = default;
    foreach (var (_, s) in ctxt.Textp) {
        var relocs = ldr.Relocs(s);
        for (nint ri = 0; ri < relocs.Count(); ri++) {
            var r = relocs.At(ri);
            if (r.Type() != objabi.R_RISCV_PCREL_ITYPE && r.Type() != objabi.R_RISCV_PCREL_STYPE && r.Type() != objabi.R_RISCV_TLS_IE_ITYPE && r.Type() != objabi.R_RISCV_TLS_IE_STYPE) {
                continue;
            }
            if (r.Off() == 0 && ldr.SymType(s) == sym.STEXT) { 
                // Use the symbol for the function instead of creating
                // an overlapping symbol.
                continue;

            } 

            // TODO(jsing): Consider generating ELF symbols without needing
            // loader symbols, in order to reduce memory consumption. This
            // would require changes to genelfsym so that it called
            // putelfsym and putelfsyment as appropriate.
            var sb = ldr.MakeSymbolBuilder(fakeLabelName);
            sb.SetType(sym.STEXT);
            sb.SetValue(ldr.SymValue(s) + int64(r.Off()));
            sb.SetLocal(true);
            sb.SetReachable(true);
            sb.SetVisibilityHidden(true);
            sb.SetSect(ldr.SymSect(s));
            {
                var outer = ldr.OuterSym(s);

                if (outer != 0) {
                    ldr.AddInteriorSym(outer, sb.Sym());
                }

            }

            hi20Syms = append(hi20Syms, sb.Sym());

        }

    }    ctxt.Textp = append(ctxt.Textp, hi20Syms);
    ldr.SortSyms(ctxt.Textp);

}

private static loader.Sym findHI20Symbol(ptr<ld.Link> _addr_ctxt, ptr<loader.Loader> _addr_ldr, long val) {
    ref ld.Link ctxt = ref _addr_ctxt.val;
    ref loader.Loader ldr = ref _addr_ldr.val;

    var idx = sort.Search(len(ctxt.Textp), i => ldr.SymValue(ctxt.Textp[i]) >= val);
    if (idx >= len(ctxt.Textp)) {
        return 0;
    }
    {
        var s = ctxt.Textp[idx];

        if (ldr.SymValue(s) == val && ldr.SymType(s) == sym.STEXT) {
            return s;
        }
    }

    return 0;

}

private static bool elfreloc1(ptr<ld.Link> _addr_ctxt, ptr<ld.OutBuf> _addr_@out, ptr<loader.Loader> _addr_ldr, loader.Sym s, loader.ExtReloc r, nint ri, long sectoff) {
    ref ld.Link ctxt = ref _addr_ctxt.val;
    ref ld.OutBuf @out = ref _addr_@out.val;
    ref loader.Loader ldr = ref _addr_ldr.val;

    var elfsym = ld.ElfSymForReloc(ctxt, r.Xsym);

    if (r.Type == objabi.R_ADDR || r.Type == objabi.R_DWARFSECREF) 
        @out.Write64(uint64(sectoff));
        switch (r.Size) {
            case 4: 
                @out.Write64(uint64(elf.R_RISCV_32) | uint64(elfsym) << 32);
                break;
            case 8: 
                @out.Write64(uint64(elf.R_RISCV_64) | uint64(elfsym) << 32);
                break;
            default: 
                ld.Errorf(null, "unknown size %d for %v relocation", r.Size, r.Type);
                return false;
                break;
        }
        @out.Write64(uint64(r.Xadd));
    else if (r.Type == objabi.R_CALLRISCV)     else if (r.Type == objabi.R_RISCV_PCREL_ITYPE || r.Type == objabi.R_RISCV_PCREL_STYPE || r.Type == objabi.R_RISCV_TLS_IE_ITYPE || r.Type == objabi.R_RISCV_TLS_IE_STYPE) 
        // Find the text symbol for the AUIPC instruction targeted
        // by this relocation.
        var relocs = ldr.Relocs(s);
        var offset = int64(relocs.At(ri).Off());
        var hi20Sym = findHI20Symbol(_addr_ctxt, _addr_ldr, ldr.SymValue(s) + offset);
        if (hi20Sym == 0) {
            ld.Errorf(null, "failed to find text symbol for HI20 relocation at %d (%x)", sectoff, ldr.SymValue(s) + offset);
            return false;
        }
        var hi20ElfSym = ld.ElfSymForReloc(ctxt, hi20Sym); 

        // Emit two relocations - a R_RISCV_PCREL_HI20 relocation and a
        // corresponding R_RISCV_PCREL_LO12_I or R_RISCV_PCREL_LO12_S relocation.
        // Note that the LO12 relocation must point to a target that has a valid
        // HI20 PC-relative relocation text symbol, which in turn points to the
        // given symbol. For further details see the ELF specification for RISC-V:
        //
        //   https://github.com/riscv/riscv-elf-psabi-doc/blob/master/riscv-elf.md#pc-relative-symbol-addresses
        //
        elf.R_RISCV hiRel = default;        elf.R_RISCV loRel = default;


        if (r.Type == objabi.R_RISCV_PCREL_ITYPE) 
            (hiRel, loRel) = (elf.R_RISCV_PCREL_HI20, elf.R_RISCV_PCREL_LO12_I);        else if (r.Type == objabi.R_RISCV_PCREL_STYPE) 
            (hiRel, loRel) = (elf.R_RISCV_PCREL_HI20, elf.R_RISCV_PCREL_LO12_S);        else if (r.Type == objabi.R_RISCV_TLS_IE_ITYPE) 
            (hiRel, loRel) = (elf.R_RISCV_TLS_GOT_HI20, elf.R_RISCV_PCREL_LO12_I);        else if (r.Type == objabi.R_RISCV_TLS_IE_STYPE) 
            (hiRel, loRel) = (elf.R_RISCV_TLS_GOT_HI20, elf.R_RISCV_PCREL_LO12_S);                @out.Write64(uint64(sectoff));
        @out.Write64(uint64(hiRel) | uint64(elfsym) << 32);
        @out.Write64(uint64(r.Xadd));
        @out.Write64(uint64(sectoff + 4));
        @out.Write64(uint64(loRel) | uint64(hi20ElfSym) << 32);
        @out.Write64(uint64(0));
    else 
        return false;
        return true;

}

private static void elfsetupplt(ptr<ld.Link> _addr_ctxt, ptr<loader.SymbolBuilder> _addr_plt, ptr<loader.SymbolBuilder> _addr_gotplt, loader.Sym dynamic) {
    ref ld.Link ctxt = ref _addr_ctxt.val;
    ref loader.SymbolBuilder plt = ref _addr_plt.val;
    ref loader.SymbolBuilder gotplt = ref _addr_gotplt.val;

    log.Fatalf("elfsetupplt");
}

private static bool machoreloc1(ptr<sys.Arch> _addr__p0, ptr<ld.OutBuf> _addr__p0, ptr<loader.Loader> _addr__p0, loader.Sym _p0, loader.ExtReloc _p0, long _p0) {
    ref sys.Arch _p0 = ref _addr__p0.val;
    ref ld.OutBuf _p0 = ref _addr__p0.val;
    ref loader.Loader _p0 = ref _addr__p0.val;

    log.Fatalf("machoreloc1 not implemented");
    return false;
}

private static (long, nint, bool) archreloc(ptr<ld.Target> _addr_target, ptr<loader.Loader> _addr_ldr, ptr<ld.ArchSyms> _addr_syms, loader.Reloc r, loader.Sym s, long val) => func((_, panic, _) => {
    long o = default;
    nint nExtReloc = default;
    bool ok = default;
    ref ld.Target target = ref _addr_target.val;
    ref loader.Loader ldr = ref _addr_ldr.val;
    ref ld.ArchSyms syms = ref _addr_syms.val;

    if (target.IsExternal()) {

        if (r.Type() == objabi.R_CALLRISCV) 
            return (val, 0, true);
        else if (r.Type() == objabi.R_RISCV_PCREL_ITYPE || r.Type() == objabi.R_RISCV_PCREL_STYPE || r.Type() == objabi.R_RISCV_TLS_IE_ITYPE || r.Type() == objabi.R_RISCV_TLS_IE_STYPE) 
            return (val, 2, true);
                return (val, 0, false);

    }
    var rs = ldr.ResolveABIAlias(r.Sym());


    if (r.Type() == objabi.R_CALLRISCV) 
        // Nothing to do.
        return (val, 0, true);
    else if (r.Type() == objabi.R_RISCV_TLS_IE_ITYPE || r.Type() == objabi.R_RISCV_TLS_IE_STYPE) 
        // TLS relocations are not currently handled for internal linking.
        // For now, TLS is only used when cgo is in use and cgo currently
        // requires external linking. However, we need to accept these
        // relocations so that code containing TLS variables will link,
        // even when they're not being used. For now, replace these
        // instructions with EBREAK to detect accidental use.
        const nuint ebreakIns = 0x00100073;

        return (ebreakIns << 32 | ebreakIns, 0, true);
    else if (r.Type() == objabi.R_RISCV_PCREL_ITYPE || r.Type() == objabi.R_RISCV_PCREL_STYPE) 
        var pc = ldr.SymValue(s) + int64(r.Off());
        var off = ldr.SymValue(rs) + r.Add() - pc; 

        // Generate AUIPC and second instruction immediates.
        var (low, high, err) = riscv.Split32BitImmediate(off);
        if (err != null) {
            ldr.Errorf(s, "R_RISCV_PCREL_ relocation does not fit in 32-bits: %d", off);
        }
        var (auipcImm, err) = riscv.EncodeUImmediate(high);
        if (err != null) {
            ldr.Errorf(s, "cannot encode R_RISCV_PCREL_ AUIPC relocation offset for %s: %v", ldr.SymName(rs), err);
        }
        long secondImm = default;        long secondImmMask = default;


        if (r.Type() == objabi.R_RISCV_PCREL_ITYPE) 
            secondImmMask = riscv.ITypeImmMask;
            secondImm, err = riscv.EncodeIImmediate(low);
            if (err != null) {
                ldr.Errorf(s, "cannot encode R_RISCV_PCREL_ITYPE I-type instruction relocation offset for %s: %v", ldr.SymName(rs), err);
            }
        else if (r.Type() == objabi.R_RISCV_PCREL_STYPE) 
            secondImmMask = riscv.STypeImmMask;
            secondImm, err = riscv.EncodeSImmediate(low);
            if (err != null) {
                ldr.Errorf(s, "cannot encode R_RISCV_PCREL_STYPE S-type instruction relocation offset for %s: %v", ldr.SymName(rs), err);
            }
        else 
            panic(fmt.Sprintf("Unknown relocation type: %v", r.Type()));
                var auipc = int64(uint32(val));
        var second = int64(uint32(val >> 32));

        auipc = (auipc & ~riscv.UTypeImmMask) | int64(uint32(auipcImm));
        second = (second & ~secondImmMask) | int64(uint32(secondImm));

        return (second << 32 | auipc, 0, true);
        return (val, 0, false);

});

private static long archrelocvariant(ptr<ld.Target> _addr__p0, ptr<loader.Loader> _addr__p0, loader.Reloc _p0, sym.RelocVariant _p0, loader.Sym _p0, long _p0, slice<byte> _p0) {
    ref ld.Target _p0 = ref _addr__p0.val;
    ref loader.Loader _p0 = ref _addr__p0.val;

    log.Fatalf("archrelocvariant");
    return -1;
}

private static (loader.ExtReloc, bool) extreloc(ptr<ld.Target> _addr_target, ptr<loader.Loader> _addr_ldr, loader.Reloc r, loader.Sym s) {
    loader.ExtReloc _p0 = default;
    bool _p0 = default;
    ref ld.Target target = ref _addr_target.val;
    ref loader.Loader ldr = ref _addr_ldr.val;


    if (r.Type() == objabi.R_RISCV_PCREL_ITYPE || r.Type() == objabi.R_RISCV_PCREL_STYPE || r.Type() == objabi.R_RISCV_TLS_IE_ITYPE || r.Type() == objabi.R_RISCV_TLS_IE_STYPE) 
        return (ld.ExtrelocViaOuterSym(ldr, r, s), true);
        return (new loader.ExtReloc(), false);

}

} // end riscv64_package
