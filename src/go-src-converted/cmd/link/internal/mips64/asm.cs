// Inferno utils/5l/asm.c
// https://bitbucket.org/inferno-os/inferno-os/src/default/utils/5l/asm.c
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

// package mips64 -- go2cs converted at 2020 August 29 10:02:35 UTC
// import "cmd/link/internal/mips64" ==> using mips64 = go.cmd.link.@internal.mips64_package
// Original source: C:\Go\src\cmd\link\internal\mips64\asm.go
using objabi = go.cmd.@internal.objabi_package;
using sys = go.cmd.@internal.sys_package;
using ld = go.cmd.link.@internal.ld_package;
using sym = go.cmd.link.@internal.sym_package;
using elf = go.debug.elf_package;
using fmt = go.fmt_package;
using log = go.log_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace link {
namespace @internal
{
    public static partial class mips64_package
    {
        private static void gentext(ref ld.Link ctxt)
        {
        }

        private static bool adddynrel(ref ld.Link ctxt, ref sym.Symbol s, ref sym.Reloc r)
        {
            log.Fatalf("adddynrel not implemented");
            return false;
        }

        private static bool elfreloc1(ref ld.Link ctxt, ref sym.Reloc r, long sectoff)
        { 
            // mips64 ELF relocation (endian neutral)
            //        offset    uint64
            //        sym        uint32
            //        ssym    uint8
            //        type3    uint8
            //        type2    uint8
            //        type    uint8
            //        addend    int64

            ctxt.Out.Write64(uint64(sectoff));

            var elfsym = r.Xsym.ElfsymForReloc();
            ctxt.Out.Write32(uint32(elfsym));
            ctxt.Out.Write8(0L);
            ctxt.Out.Write8(0L);
            ctxt.Out.Write8(0L);

            if (r.Type == objabi.R_ADDR) 
                switch (r.Siz)
                {
                    case 4L: 
                        ctxt.Out.Write8(uint8(elf.R_MIPS_32));
                        break;
                    case 8L: 
                        ctxt.Out.Write8(uint8(elf.R_MIPS_64));
                        break;
                    default: 
                        return false;
                        break;
                }
            else if (r.Type == objabi.R_ADDRMIPS) 
                ctxt.Out.Write8(uint8(elf.R_MIPS_LO16));
            else if (r.Type == objabi.R_ADDRMIPSU) 
                ctxt.Out.Write8(uint8(elf.R_MIPS_HI16));
            else if (r.Type == objabi.R_ADDRMIPSTLS) 
                ctxt.Out.Write8(uint8(elf.R_MIPS_TLS_TPREL_LO16));
            else if (r.Type == objabi.R_CALLMIPS || r.Type == objabi.R_JMPMIPS) 
                ctxt.Out.Write8(uint8(elf.R_MIPS_26));
            else 
                return false;
                        ctxt.Out.Write64(uint64(r.Xadd));

            return true;
        }

        private static void elfsetupplt(ref ld.Link ctxt)
        {
            return;
        }

        private static bool machoreloc1(ref sys.Arch arch, ref ld.OutBuf @out, ref sym.Symbol s, ref sym.Reloc r, long sectoff)
        {
            return false;
        }

        private static bool archreloc(ref ld.Link ctxt, ref sym.Reloc r, ref sym.Symbol s, ref long val)
        {
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

                    return true;
                else if (r.Type == objabi.R_ADDRMIPSTLS || r.Type == objabi.R_CALLMIPS || r.Type == objabi.R_JMPMIPS) 
                    r.Done = false;
                    r.Xsym = r.Sym;
                    r.Xadd = r.Add;
                    return true;
                else 
                    return false;
                            }

            if (r.Type == objabi.R_CONST) 
                val.Value = r.Add;
                return true;
            else if (r.Type == objabi.R_GOTOFF) 
                val.Value = ld.Symaddr(r.Sym) + r.Add - ld.Symaddr(ctxt.Syms.Lookup(".got", 0L));
                return true;
            else if (r.Type == objabi.R_ADDRMIPS || r.Type == objabi.R_ADDRMIPSU) 
                var t = ld.Symaddr(r.Sym) + r.Add;
                var o1 = ctxt.Arch.ByteOrder.Uint32(s.P[r.Off..]);
                if (r.Type == objabi.R_ADDRMIPS)
                {
                    val.Value = int64(o1 & 0xffff0000UL | uint32(t) & 0xffffUL);
                }
                else
                {
                    val.Value = int64(o1 & 0xffff0000UL | uint32((t + 1L << (int)(15L)) >> (int)(16L)) & 0xffffUL);
                }
                return true;
            else if (r.Type == objabi.R_ADDRMIPSTLS) 
                // thread pointer is at 0x7000 offset from the start of TLS data area
                t = ld.Symaddr(r.Sym) + r.Add - 0x7000UL;
                if (t < -32768L || t >= 32678L)
                {
                    ld.Errorf(s, "TLS offset out of range %d", t);
                }
                o1 = ctxt.Arch.ByteOrder.Uint32(s.P[r.Off..]);
                val.Value = int64(o1 & 0xffff0000UL | uint32(t) & 0xffffUL);
                return true;
            else if (r.Type == objabi.R_CALLMIPS || r.Type == objabi.R_JMPMIPS) 
                // Low 26 bits = (S + A) >> 2
                t = ld.Symaddr(r.Sym) + r.Add;
                o1 = ctxt.Arch.ByteOrder.Uint32(s.P[r.Off..]);
                val.Value = int64(o1 & 0xfc000000UL | uint32(t >> (int)(2L)) & ~0xfc000000UL);
                return true;
                        return false;
        }

        private static long archrelocvariant(ref ld.Link ctxt, ref sym.Reloc r, ref sym.Symbol s, long t)
        {
            return -1L;
        }

        private static void asmb(ref ld.Link ctxt)
        {
            if (ctxt.Debugvlog != 0L)
            {
                ctxt.Logf("%5.2f asmb\n", ld.Cputime());
            }
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
                if (ctxt.Debugvlog != 0L)
                {
                    ctxt.Logf("%5.2f rodatblk\n", ld.Cputime());
                }
                ctxt.Out.SeekSet(int64(ld.Segrodata.Fileoff));
                ld.Datblk(ctxt, int64(ld.Segrodata.Vaddr), int64(ld.Segrodata.Filelen));
            }
            if (ld.Segrelrodata.Filelen > 0L)
            {
                if (ctxt.Debugvlog != 0L)
                {
                    ctxt.Logf("%5.2f rodatblk\n", ld.Cputime());
                }
                ctxt.Out.SeekSet(int64(ld.Segrelrodata.Fileoff));
                ld.Datblk(ctxt, int64(ld.Segrelrodata.Vaddr), int64(ld.Segrelrodata.Filelen));
            }
            if (ctxt.Debugvlog != 0L)
            {
                ctxt.Logf("%5.2f datblk\n", ld.Cputime());
            }
            ctxt.Out.SeekSet(int64(ld.Segdata.Fileoff));
            ld.Datblk(ctxt, int64(ld.Segdata.Vaddr), int64(ld.Segdata.Filelen));

            ctxt.Out.SeekSet(int64(ld.Segdwarf.Fileoff));
            ld.Dwarfblk(ctxt, int64(ld.Segdwarf.Vaddr), int64(ld.Segdwarf.Filelen)); 

            /* output symbol table */
            ld.Symsize = 0L;

            ld.Lcsize = 0L;
            var symo = uint32(0L);
            if (!ld.FlagS.Value)
            { 
                // TODO: rationalize
                if (ctxt.Debugvlog != 0L)
                {
                    ctxt.Logf("%5.2f sym\n", ld.Cputime());
                }

                if (ctxt.HeadType == objabi.Hplan9) 
                    symo = uint32(ld.Segdata.Fileoff + ld.Segdata.Filelen);
                else 
                    if (ctxt.IsELF)
                    {
                        symo = uint32(ld.Segdwarf.Fileoff + ld.Segdwarf.Filelen);
                        symo = uint32(ld.Rnd(int64(symo), int64(ld.FlagRound.Value)));
                    }
                                ctxt.Out.SeekSet(int64(symo));

                if (ctxt.HeadType == objabi.Hplan9) 
                    ld.Asmplan9sym(ctxt);
                    ctxt.Out.Flush();

                    var sym = ctxt.Syms.Lookup("pclntab", 0L);
                    if (sym != null)
                    {
                        ld.Lcsize = int32(len(sym.P));
                        ctxt.Out.Write(sym.P);
                        ctxt.Out.Flush();
                    }
                else 
                    if (ctxt.IsELF)
                    {
                        if (ctxt.Debugvlog != 0L)
                        {
                            ctxt.Logf("%5.2f elfsym\n", ld.Cputime());
                        }
                        ld.Asmelfsym(ctxt);
                        ctxt.Out.Flush();
                        ctxt.Out.Write(ld.Elfstrdat);

                        if (ctxt.LinkMode == ld.LinkExternal)
                        {
                            ld.Elfemitreloc(ctxt);
                        }
                    }
                            }
            if (ctxt.Debugvlog != 0L)
            {
                ctxt.Logf("%5.2f header\n", ld.Cputime());
            }
            ctxt.Out.SeekSet(0L);

            if (ctxt.HeadType == objabi.Hplan9) /* plan 9 */
                var magic = uint32(4L * 18L * 18L + 7L);
                if (ctxt.Arch == sys.ArchMIPS64LE)
                {
                    magic = uint32(4L * 26L * 26L + 7L);
                }
                ctxt.Out.Write32(magic); /* magic */
                ctxt.Out.Write32(uint32(ld.Segtext.Filelen)); /* sizes */
                ctxt.Out.Write32(uint32(ld.Segdata.Filelen));
                ctxt.Out.Write32(uint32(ld.Segdata.Length - ld.Segdata.Filelen));
                ctxt.Out.Write32(uint32(ld.Symsize)); /* nsyms */
                ctxt.Out.Write32(uint32(ld.Entryvalue(ctxt))); /* va of entry */
                ctxt.Out.Write32(0L);
                ctxt.Out.Write32(uint32(ld.Lcsize));
            else if (ctxt.HeadType == objabi.Hlinux || ctxt.HeadType == objabi.Hfreebsd || ctxt.HeadType == objabi.Hnetbsd || ctxt.HeadType == objabi.Hopenbsd || ctxt.HeadType == objabi.Hnacl) 
                ld.Asmbelf(ctxt, int64(symo));
            else                         ctxt.Out.Flush();
            if (ld.FlagC.Value)
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
