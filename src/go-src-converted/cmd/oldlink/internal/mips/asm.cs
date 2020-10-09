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
//    Portions Copyright © 2016 The Go Authors. All rights reserved.
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

// package mips -- go2cs converted at 2020 October 09 05:50:55 UTC
// import "cmd/oldlink/internal/mips" ==> using mips = go.cmd.oldlink.@internal.mips_package
// Original source: C:\Go\src\cmd\oldlink\internal\mips\asm.go
using objabi = go.cmd.@internal.objabi_package;
using sys = go.cmd.@internal.sys_package;
using ld = go.cmd.oldlink.@internal.ld_package;
using sym = go.cmd.oldlink.@internal.sym_package;
using elf = go.debug.elf_package;
using fmt = go.fmt_package;
using log = go.log_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace oldlink {
namespace @internal
{
    public static partial class mips_package
    {
        private static void gentext(ptr<ld.Link> _addr_ctxt)
        {
            ref ld.Link ctxt = ref _addr_ctxt.val;

            return ;
        }

        private static bool adddynrel(ptr<ld.Link> _addr_ctxt, ptr<sym.Symbol> _addr_s, ptr<sym.Reloc> _addr_r)
        {
            ref ld.Link ctxt = ref _addr_ctxt.val;
            ref sym.Symbol s = ref _addr_s.val;
            ref sym.Reloc r = ref _addr_r.val;

            log.Fatalf("adddynrel not implemented");
            return false;
        }

        private static bool elfreloc1(ptr<ld.Link> _addr_ctxt, ptr<sym.Reloc> _addr_r, long sectoff)
        {
            ref ld.Link ctxt = ref _addr_ctxt.val;
            ref sym.Reloc r = ref _addr_r.val;

            ctxt.Out.Write32(uint32(sectoff));

            var elfsym = r.Xsym.ElfsymForReloc();

            if (r.Type == objabi.R_ADDR) 
                if (r.Siz != 4L)
                {
                    return false;
                }

                ctxt.Out.Write32(uint32(elf.R_MIPS_32) | uint32(elfsym) << (int)(8L));
            else if (r.Type == objabi.R_ADDRMIPS) 
                ctxt.Out.Write32(uint32(elf.R_MIPS_LO16) | uint32(elfsym) << (int)(8L));
            else if (r.Type == objabi.R_ADDRMIPSU) 
                ctxt.Out.Write32(uint32(elf.R_MIPS_HI16) | uint32(elfsym) << (int)(8L));
            else if (r.Type == objabi.R_ADDRMIPSTLS) 
                ctxt.Out.Write32(uint32(elf.R_MIPS_TLS_TPREL_LO16) | uint32(elfsym) << (int)(8L));
            else if (r.Type == objabi.R_CALLMIPS || r.Type == objabi.R_JMPMIPS) 
                ctxt.Out.Write32(uint32(elf.R_MIPS_26) | uint32(elfsym) << (int)(8L));
            else 
                return false;
                        return true;

        }

        private static void elfsetupplt(ptr<ld.Link> _addr_ctxt)
        {
            ref ld.Link ctxt = ref _addr_ctxt.val;

            return ;
        }

        private static bool machoreloc1(ptr<sys.Arch> _addr_arch, ptr<ld.OutBuf> _addr_@out, ptr<sym.Symbol> _addr_s, ptr<sym.Reloc> _addr_r, long sectoff)
        {
            ref sys.Arch arch = ref _addr_arch.val;
            ref ld.OutBuf @out = ref _addr_@out.val;
            ref sym.Symbol s = ref _addr_s.val;
            ref sym.Reloc r = ref _addr_r.val;

            return false;
        }

        private static long applyrel(ptr<sys.Arch> _addr_arch, ptr<sym.Reloc> _addr_r, ptr<sym.Symbol> _addr_s, long val, long t)
        {
            ref sys.Arch arch = ref _addr_arch.val;
            ref sym.Reloc r = ref _addr_r.val;
            ref sym.Symbol s = ref _addr_s.val;

            var o = arch.ByteOrder.Uint32(s.P[r.Off..]);

            if (r.Type == objabi.R_ADDRMIPS || r.Type == objabi.R_ADDRMIPSTLS) 
                return int64(o & 0xffff0000UL | uint32(t) & 0xffffUL);
            else if (r.Type == objabi.R_ADDRMIPSU) 
                return int64(o & 0xffff0000UL | uint32((t + (1L << (int)(15L))) >> (int)(16L)) & 0xffffUL);
            else if (r.Type == objabi.R_CALLMIPS || r.Type == objabi.R_JMPMIPS) 
                return int64(o & 0xfc000000UL | uint32(t >> (int)(2L)) & ~0xfc000000UL);
            else 
                return val;
            
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

                if (r.Type == objabi.R_ADDRMIPS || r.Type == objabi.R_ADDRMIPSU) 
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
                    return (applyrel(_addr_ctxt.Arch, _addr_r, _addr_s, val, r.Xadd), true);
                else if (r.Type == objabi.R_ADDRMIPSTLS || r.Type == objabi.R_CALLMIPS || r.Type == objabi.R_JMPMIPS) 
                    r.Done = false;
                    r.Xsym = r.Sym;
                    r.Xadd = r.Add;
                    return (applyrel(_addr_ctxt.Arch, _addr_r, _addr_s, val, r.Add), true);
                else 
                    return (val, false);
                
            }


            if (r.Type == objabi.R_CONST) 
                return (r.Add, true);
            else if (r.Type == objabi.R_GOTOFF) 
                return (ld.Symaddr(r.Sym) + r.Add - ld.Symaddr(ctxt.Syms.Lookup(".got", 0L)), true);
            else if (r.Type == objabi.R_ADDRMIPS || r.Type == objabi.R_ADDRMIPSU) 
                var t = ld.Symaddr(r.Sym) + r.Add;
                return (applyrel(_addr_ctxt.Arch, _addr_r, _addr_s, val, t), true);
            else if (r.Type == objabi.R_CALLMIPS || r.Type == objabi.R_JMPMIPS) 
                t = ld.Symaddr(r.Sym) + r.Add;

                if (t & 3L != 0L)
                {
                    ld.Errorf(s, "direct call is not aligned: %s %x", r.Sym.Name, t);
                } 

                // check if target address is in the same 256 MB region as the next instruction
                if ((s.Value + int64(r.Off) + 4L) & 0xf0000000UL != (t & 0xf0000000UL))
                {
                    ld.Errorf(s, "direct call too far: %s %x", r.Sym.Name, t);
                }

                return (applyrel(_addr_ctxt.Arch, _addr_r, _addr_s, val, t), true);
            else if (r.Type == objabi.R_ADDRMIPSTLS) 
                // thread pointer is at 0x7000 offset from the start of TLS data area
                t = ld.Symaddr(r.Sym) + r.Add - 0x7000UL;
                if (t < -32768L || t >= 32678L)
                {
                    ld.Errorf(s, "TLS offset out of range %d", t);
                }

                return (applyrel(_addr_ctxt.Arch, _addr_r, _addr_s, val, t), true);
                        return (val, false);

        }

        private static long archrelocvariant(ptr<ld.Link> _addr_ctxt, ptr<sym.Reloc> _addr_r, ptr<sym.Symbol> _addr_s, long t)
        {
            ref ld.Link ctxt = ref _addr_ctxt.val;
            ref sym.Reloc r = ref _addr_r.val;
            ref sym.Symbol s = ref _addr_s.val;

            return -1L;
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
