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

// package arm -- go2cs converted at 2020 October 08 04:42:07 UTC
// import "cmd/oldlink/internal/arm" ==> using arm = go.cmd.oldlink.@internal.arm_package
// Original source: C:\Go\src\cmd\oldlink\internal\arm\obj.go
using objabi = go.cmd.@internal.objabi_package;
using sys = go.cmd.@internal.sys_package;
using ld = go.cmd.oldlink.@internal.ld_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace oldlink {
namespace @internal
{
    public static partial class arm_package
    {
        public static (ptr<sys.Arch>, ld.Arch) Init()
        {
            ptr<sys.Arch> _p0 = default!;
            ld.Arch _p0 = default;

            var arch = sys.ArchARM;

            ld.Arch theArch = new ld.Arch(Funcalign:funcAlign,Maxalign:maxAlign,Minalign:minAlign,Dwarfregsp:dwarfRegSP,Dwarfreglr:dwarfRegLR,Adddynrel:adddynrel,Archinit:archinit,Archreloc:archreloc,Archrelocvariant:archrelocvariant,Trampoline:trampoline,Asmb:asmb,Asmb2:asmb2,Elfreloc1:elfreloc1,Elfsetupplt:elfsetupplt,Gentext:gentext,Machoreloc1:machoreloc1,PEreloc1:pereloc1,Linuxdynld:"/lib/ld-linux.so.3",Freebsddynld:"/usr/libexec/ld-elf.so.1",Openbsddynld:"/usr/libexec/ld.so",Netbsddynld:"/libexec/ld.elf_so",Dragonflydynld:"XXX",Solarisdynld:"XXX",);

            return (_addr_arch!, theArch);
        }

        private static void archinit(ptr<ld.Link> _addr_ctxt)
        {
            ref ld.Link ctxt = ref _addr_ctxt.val;


            if (ctxt.HeadType == objabi.Hplan9) /* plan 9 */
                ld.HEADR = 32L;

                if (ld.FlagTextAddr == -1L.val)
                {
                    ld.FlagTextAddr.val = 4128L;
                }

                if (ld.FlagRound == -1L.val)
                {
                    ld.FlagRound.val = 4096L;
                }

            else if (ctxt.HeadType == objabi.Hlinux || ctxt.HeadType == objabi.Hfreebsd || ctxt.HeadType == objabi.Hnetbsd || ctxt.HeadType == objabi.Hopenbsd) 
                ld.FlagD.val = false; 
                // with dynamic linking
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

            else if (ctxt.HeadType == objabi.Hwindows) /* PE executable */
                // ld.HEADR, ld.FlagTextAddr, ld.FlagRound are set in ld.Peinit
                return ;
            else 
                ld.Exitf("unknown -H option: %v", ctxt.HeadType);
            
        }
    }
}}}}
