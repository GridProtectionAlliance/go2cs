// Inferno utils/5l/obj.c
// https://bitbucket.org/inferno-os/inferno-os/src/master/utils/5l/obj.c
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

// package arm -- go2cs converted at 2022 March 06 23:22:35 UTC
// import "cmd/link/internal/arm" ==> using arm = go.cmd.link.@internal.arm_package
// Original source: C:\Program Files\Go\src\cmd\link\internal\arm\obj.go
using objabi = go.cmd.@internal.objabi_package;
using sys = go.cmd.@internal.sys_package;
using ld = go.cmd.link.@internal.ld_package;

namespace go.cmd.link.@internal;

public static partial class arm_package {

public static (ptr<sys.Arch>, ld.Arch) Init() {
    ptr<sys.Arch> _p0 = default!;
    ld.Arch _p0 = default;

    var arch = sys.ArchARM;

    ld.Arch theArch = new ld.Arch(Funcalign:funcAlign,Maxalign:maxAlign,Minalign:minAlign,Dwarfregsp:dwarfRegSP,Dwarfreglr:dwarfRegLR,TrampLimit:0x1c00000,Plan9Magic:0x647,Adddynrel:adddynrel,Archinit:archinit,Archreloc:archreloc,Archrelocvariant:archrelocvariant,Extreloc:extreloc,Trampoline:trampoline,Elfreloc1:elfreloc1,ElfrelocSize:8,Elfsetupplt:elfsetupplt,Gentext:gentext,Machoreloc1:machoreloc1,PEreloc1:pereloc1,Linuxdynld:"/lib/ld-linux.so.3",Freebsddynld:"/usr/libexec/ld-elf.so.1",Openbsddynld:"/usr/libexec/ld.so",Netbsddynld:"/libexec/ld.elf_so",Dragonflydynld:"XXX",Solarisdynld:"XXX",);

    return (_addr_arch!, theArch);
}

private static void archinit(ptr<ld.Link> _addr_ctxt) {
    ref ld.Link ctxt = ref _addr_ctxt.val;


    if (ctxt.HeadType == objabi.Hplan9) /* plan 9 */
        ld.HEADR = 32;

        if (ld.FlagTextAddr == -1.val) {
            ld.FlagTextAddr.val = 4128;
        }
        if (ld.FlagRound == -1.val) {
            ld.FlagRound.val = 4096;
        }
    else if (ctxt.HeadType == objabi.Hlinux || ctxt.HeadType == objabi.Hfreebsd || ctxt.HeadType == objabi.Hnetbsd || ctxt.HeadType == objabi.Hopenbsd) 
        ld.FlagD.val = false; 
        // with dynamic linking
        ld.Elfinit(ctxt);
        ld.HEADR = ld.ELFRESERVE;
        if (ld.FlagTextAddr == -1.val) {
            ld.FlagTextAddr.val = 0x10000 + int64(ld.HEADR);
        }
        if (ld.FlagRound == -1.val) {
            ld.FlagRound.val = 0x10000;
        }
    else if (ctxt.HeadType == objabi.Hwindows) /* PE executable */
        // ld.HEADR, ld.FlagTextAddr, ld.FlagRound are set in ld.Peinit
        return ;
    else 
        ld.Exitf("unknown -H option: %v", ctxt.HeadType);
    
}

} // end arm_package
