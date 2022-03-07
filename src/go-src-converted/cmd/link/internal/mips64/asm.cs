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

// package mips64 -- go2cs converted at 2022 March 06 23:20:08 UTC
// import "cmd/link/internal/mips64" ==> using mips64 = go.cmd.link.@internal.mips64_package
// Original source: C:\Program Files\Go\src\cmd\link\internal\mips64\asm.go
using objabi = go.cmd.@internal.objabi_package;
using sys = go.cmd.@internal.sys_package;
using ld = go.cmd.link.@internal.ld_package;
using loader = go.cmd.link.@internal.loader_package;
using sym = go.cmd.link.@internal.sym_package;
using elf = go.debug.elf_package;

namespace go.cmd.link.@internal;

public static partial class mips64_package {

private static void gentext(ptr<ld.Link> _addr_ctxt, ptr<loader.Loader> _addr_ldr) {
    ref ld.Link ctxt = ref _addr_ctxt.val;
    ref loader.Loader ldr = ref _addr_ldr.val;

}

private static bool elfreloc1(ptr<ld.Link> _addr_ctxt, ptr<ld.OutBuf> _addr_@out, ptr<loader.Loader> _addr_ldr, loader.Sym s, loader.ExtReloc r, nint ri, long sectoff) {
    ref ld.Link ctxt = ref _addr_ctxt.val;
    ref ld.OutBuf @out = ref _addr_@out.val;
    ref loader.Loader ldr = ref _addr_ldr.val;

    // mips64 ELF relocation (endian neutral)
    //        offset    uint64
    //        sym        uint32
    //        ssym    uint8
    //        type3    uint8
    //        type2    uint8
    //        type    uint8
    //        addend    int64

    var addend = r.Xadd;

    @out.Write64(uint64(sectoff));

    var elfsym = ld.ElfSymForReloc(ctxt, r.Xsym);
    @out.Write32(uint32(elfsym));
    @out.Write8(0);
    @out.Write8(0);
    @out.Write8(0);

    if (r.Type == objabi.R_ADDR || r.Type == objabi.R_DWARFSECREF) 
        switch (r.Size) {
            case 4: 
                @out.Write8(uint8(elf.R_MIPS_32));
                break;
            case 8: 
                @out.Write8(uint8(elf.R_MIPS_64));
                break;
            default: 
                return false;
                break;
        }
    else if (r.Type == objabi.R_ADDRMIPS) 
        @out.Write8(uint8(elf.R_MIPS_LO16));
    else if (r.Type == objabi.R_ADDRMIPSU) 
        @out.Write8(uint8(elf.R_MIPS_HI16));
    else if (r.Type == objabi.R_ADDRMIPSTLS) 
        @out.Write8(uint8(elf.R_MIPS_TLS_TPREL_LO16));
        if (ctxt.Target.IsOpenbsd()) { 
            // OpenBSD mips64 does not currently offset TLS by 0x7000,
            // as such we need to add this back to get the correct offset
            // via the external linker.
            addend += 0x7000;

        }
    else if (r.Type == objabi.R_CALLMIPS || r.Type == objabi.R_JMPMIPS) 
        @out.Write8(uint8(elf.R_MIPS_26));
    else 
        return false;
        @out.Write64(uint64(addend));

    return true;

}

private static void elfsetupplt(ptr<ld.Link> _addr_ctxt, ptr<loader.SymbolBuilder> _addr_plt, ptr<loader.SymbolBuilder> _addr_gotplt, loader.Sym dynamic) {
    ref ld.Link ctxt = ref _addr_ctxt.val;
    ref loader.SymbolBuilder plt = ref _addr_plt.val;
    ref loader.SymbolBuilder gotplt = ref _addr_gotplt.val;

    return ;
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

    if (target.IsExternal()) {

        if (r.Type() == objabi.R_ADDRMIPS || r.Type() == objabi.R_ADDRMIPSU || r.Type() == objabi.R_ADDRMIPSTLS || r.Type() == objabi.R_CALLMIPS || r.Type() == objabi.R_JMPMIPS) 
            return (val, 1, true);
        else 
            return (val, 0, false);
        
    }
    const var isOk = true;

    const nint noExtReloc = 0;

    var rs = r.Sym();
    rs = ldr.ResolveABIAlias(rs);

    if (r.Type() == objabi.R_ADDRMIPS || r.Type() == objabi.R_ADDRMIPSU) 
        var t = ldr.SymValue(rs) + r.Add();
        if (r.Type() == objabi.R_ADDRMIPS) {
            return (int64(val & 0xffff0000 | t & 0xffff), noExtReloc, isOk);
        }
        return (int64(val & 0xffff0000 | ((t + 1 << 15) >> 16) & 0xffff), noExtReloc, isOk);
    else if (r.Type() == objabi.R_ADDRMIPSTLS) 
        // thread pointer is at 0x7000 offset from the start of TLS data area
        t = ldr.SymValue(rs) + r.Add() - 0x7000;
        if (target.IsOpenbsd()) { 
            // OpenBSD mips64 does not currently offset TLS by 0x7000,
            // as such we need to add this back to get the correct offset.
            t += 0x7000;

        }
        if (t < -32768 || t >= 32678) {
            ldr.Errorf(s, "TLS offset out of range %d", t);
        }
        return (int64(val & 0xffff0000 | t & 0xffff), noExtReloc, isOk);
    else if (r.Type() == objabi.R_CALLMIPS || r.Type() == objabi.R_JMPMIPS) 
        // Low 26 bits = (S + A) >> 2
        t = ldr.SymValue(rs) + r.Add();
        return (int64(val & 0xfc000000 | (t >> 2) & ~0xfc000000), noExtReloc, isOk);
        return (val, 0, false);

}

private static long archrelocvariant(ptr<ld.Target> _addr__p0, ptr<loader.Loader> _addr__p0, loader.Reloc _p0, sym.RelocVariant _p0, loader.Sym _p0, long _p0, slice<byte> _p0) {
    ref ld.Target _p0 = ref _addr__p0.val;
    ref loader.Loader _p0 = ref _addr__p0.val;

    return -1;
}

private static (loader.ExtReloc, bool) extreloc(ptr<ld.Target> _addr_target, ptr<loader.Loader> _addr_ldr, loader.Reloc r, loader.Sym s) {
    loader.ExtReloc _p0 = default;
    bool _p0 = default;
    ref ld.Target target = ref _addr_target.val;
    ref loader.Loader ldr = ref _addr_ldr.val;


    if (r.Type() == objabi.R_ADDRMIPS || r.Type() == objabi.R_ADDRMIPSU) 
        return (ld.ExtrelocViaOuterSym(ldr, r, s), true);
    else if (r.Type() == objabi.R_ADDRMIPSTLS || r.Type() == objabi.R_CALLMIPS || r.Type() == objabi.R_JMPMIPS) 
        return (ld.ExtrelocSimple(ldr, r), true);
        return (new loader.ExtReloc(), false);

}

} // end mips64_package
