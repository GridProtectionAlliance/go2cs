// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package riscv64 -- go2cs converted at 2020 October 08 04:37:58 UTC
// import "cmd/link/internal/riscv64" ==> using riscv64 = go.cmd.link.@internal.riscv64_package
// Original source: C:\Go\src\cmd\link\internal\riscv64\obj.go
using objabi = go.cmd.@internal.objabi_package;
using sys = go.cmd.@internal.sys_package;
using ld = go.cmd.link.@internal.ld_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace link {
namespace @internal
{
    public static partial class riscv64_package
    {
        public static (ptr<sys.Arch>, ld.Arch) Init()
        {
            ptr<sys.Arch> _p0 = default!;
            ld.Arch _p0 = default;

            var arch = sys.ArchRISCV64;

            ld.Arch theArch = new ld.Arch(Funcalign:funcAlign,Maxalign:maxAlign,Minalign:minAlign,Dwarfregsp:dwarfRegSP,Dwarfreglr:dwarfRegLR,Adddynrel:adddynrel,Archinit:archinit,Archreloc:archreloc,Archrelocvariant:archrelocvariant,Asmb:asmb,Asmb2:asmb2,Elfreloc1:elfreloc1,Elfsetupplt:elfsetupplt,Gentext2:gentext2,Machoreloc1:machoreloc1,Linuxdynld:"/lib/ld.so.1",Freebsddynld:"XXX",Netbsddynld:"XXX",Openbsddynld:"XXX",Dragonflydynld:"XXX",Solarisdynld:"XXX",);

            return (_addr_arch!, theArch);
        }

        private static void archinit(ptr<ld.Link> _addr_ctxt)
        {
            ref ld.Link ctxt = ref _addr_ctxt.val;


            if (ctxt.HeadType == objabi.Hlinux) 
                ld.Elfinit(ctxt);
                ld.HEADR = ld.ELFRESERVE;
                if (ld.FlagTextAddr == -1L.val)
                {
                    ld.FlagTextAddr.val = 0x10000UL + int64(ld.HEADR);
                }

                if (ld.FlagRound == -1L.val)
                {
                    ld.FlagRound.val = 0x10000UL;
                }

            else 
                ld.Exitf("unknown -H option: %v", ctxt.HeadType);
            
        }
    }
}}}}
