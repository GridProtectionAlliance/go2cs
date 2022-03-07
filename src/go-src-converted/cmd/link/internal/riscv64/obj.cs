// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package riscv64 -- go2cs converted at 2022 March 06 23:20:38 UTC
// import "cmd/link/internal/riscv64" ==> using riscv64 = go.cmd.link.@internal.riscv64_package
// Original source: C:\Program Files\Go\src\cmd\link\internal\riscv64\obj.go
using objabi = go.cmd.@internal.objabi_package;
using sys = go.cmd.@internal.sys_package;
using ld = go.cmd.link.@internal.ld_package;

namespace go.cmd.link.@internal;

public static partial class riscv64_package {

public static (ptr<sys.Arch>, ld.Arch) Init() {
    ptr<sys.Arch> _p0 = default!;
    ld.Arch _p0 = default;

    var arch = sys.ArchRISCV64;

    ld.Arch theArch = new ld.Arch(Funcalign:funcAlign,Maxalign:maxAlign,Minalign:minAlign,Dwarfregsp:dwarfRegSP,Dwarfreglr:dwarfRegLR,Archinit:archinit,Archreloc:archreloc,Archrelocvariant:archrelocvariant,Extreloc:extreloc,Elfreloc1:elfreloc1,ElfrelocSize:24,Elfsetupplt:elfsetupplt,Gentext:gentext,GenSymsLate:genSymsLate,Machoreloc1:machoreloc1,Linuxdynld:"/lib/ld.so.1",Freebsddynld:"XXX",Netbsddynld:"XXX",Openbsddynld:"XXX",Dragonflydynld:"XXX",Solarisdynld:"XXX",);

    return (_addr_arch!, theArch);
}

private static void archinit(ptr<ld.Link> _addr_ctxt) {
    ref ld.Link ctxt = ref _addr_ctxt.val;


    if (ctxt.HeadType == objabi.Hlinux) 
        ld.Elfinit(ctxt);
        ld.HEADR = ld.ELFRESERVE;
        if (ld.FlagTextAddr == -1.val) {
            ld.FlagTextAddr.val = 0x10000 + int64(ld.HEADR);
        }
        if (ld.FlagRound == -1.val) {
            ld.FlagRound.val = 0x10000;
        }
    else 
        ld.Exitf("unknown -H option: %v", ctxt.HeadType);
    
}

} // end riscv64_package
