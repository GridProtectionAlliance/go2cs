// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package loadmacho implements a Mach-O file reader.
// package loadmacho -- go2cs converted at 2020 October 09 05:52:07 UTC
// import "cmd/oldlink/internal/loadmacho" ==> using loadmacho = go.cmd.oldlink.@internal.loadmacho_package
// Original source: C:\Go\src\cmd\oldlink\internal\loadmacho\ldmacho.go
using bytes = go.bytes_package;
using bio = go.cmd.@internal.bio_package;
using objabi = go.cmd.@internal.objabi_package;
using sys = go.cmd.@internal.sys_package;
using loader = go.cmd.oldlink.@internal.loader_package;
using sym = go.cmd.oldlink.@internal.sym_package;
using binary = go.encoding.binary_package;
using fmt = go.fmt_package;
using io = go.io_package;
using sort = go.sort_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace oldlink {
namespace @internal
{
    public static partial class loadmacho_package
    {
        /*
        Derived from Plan 9 from User Space's src/libmach/elf.h, elf.c
        http://code.swtch.com/plan9port/src/tip/src/libmach/

            Copyright © 2004 Russ Cox.
            Portions Copyright © 2008-2010 Google Inc.
            Portions Copyright © 2010 The Go Authors.

        Permission is hereby granted, free of charge, to any person obtaining a copy
        of this software and associated documentation files (the "Software"), to deal
        in the Software without restriction, including without limitation the rights
        to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
        copies of the Software, and to permit persons to whom the Software is
        furnished to do so, subject to the following conditions:

        The above copyright notice and this permission notice shall be included in
        all copies or substantial portions of the Software.

        THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
        IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
        FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.  IN NO EVENT SHALL THE
        AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
        LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
        OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
        THE SOFTWARE.
        */

        // TODO(crawshaw): de-duplicate these symbols with cmd/internal/ld
        public static readonly long MACHO_X86_64_RELOC_UNSIGNED = (long)0L;
        public static readonly long MACHO_X86_64_RELOC_SIGNED = (long)1L;
        public static readonly long MACHO_FAKE_GOTPCREL = (long)100L;


        private partial struct ldMachoObj
        {
            public ptr<bio.Reader> f;
            public long @base; // off in f where Mach-O begins
            public long length; // length of Mach-O
            public bool is64;
            public @string name;
            public binary.ByteOrder e;
            public ulong cputype;
            public ulong subcputype;
            public uint filetype;
            public uint flags;
            public slice<ldMachoCmd> cmd;
            public ulong ncmd;
        }

        private partial struct ldMachoCmd
        {
            public long type_;
            public uint off;
            public uint size;
            public ldMachoSeg seg;
            public ldMachoSymtab sym;
            public ldMachoDysymtab dsym;
        }

        private partial struct ldMachoSeg
        {
            public @string name;
            public ulong vmaddr;
            public ulong vmsize;
            public uint fileoff;
            public uint filesz;
            public uint maxprot;
            public uint initprot;
            public uint nsect;
            public uint flags;
            public slice<ldMachoSect> sect;
        }

        private partial struct ldMachoSect
        {
            public @string name;
            public @string segname;
            public ulong addr;
            public ulong size;
            public uint off;
            public uint align;
            public uint reloff;
            public uint nreloc;
            public uint flags;
            public uint res1;
            public uint res2;
            public ptr<sym.Symbol> sym;
            public slice<ldMachoRel> rel;
        }

        private partial struct ldMachoRel
        {
            public uint addr;
            public uint symnum;
            public byte pcrel;
            public byte length;
            public byte extrn;
            public byte type_;
            public byte scattered;
            public uint value;
        }

        private partial struct ldMachoSymtab
        {
            public uint symoff;
            public uint nsym;
            public uint stroff;
            public uint strsize;
            public slice<byte> str;
            public slice<ldMachoSym> sym;
        }

        private partial struct ldMachoSym
        {
            public @string name;
            public byte type_;
            public byte sectnum;
            public ushort desc;
            public sbyte kind;
            public ulong value;
            public ptr<sym.Symbol> sym;
        }

        private partial struct ldMachoDysymtab
        {
            public uint ilocalsym;
            public uint nlocalsym;
            public uint iextdefsym;
            public uint nextdefsym;
            public uint iundefsym;
            public uint nundefsym;
            public uint tocoff;
            public uint ntoc;
            public uint modtaboff;
            public uint nmodtab;
            public uint extrefsymoff;
            public uint nextrefsyms;
            public uint indirectsymoff;
            public uint nindirectsyms;
            public uint extreloff;
            public uint nextrel;
            public uint locreloff;
            public uint nlocrel;
            public slice<uint> indir;
        }

        // ldMachoSym.type_
        public static readonly ulong N_EXT = (ulong)0x01UL;
        public static readonly ulong N_TYPE = (ulong)0x1eUL;
        public static readonly ulong N_STAB = (ulong)0xe0UL;


        // ldMachoSym.desc
        public static readonly ulong N_WEAK_REF = (ulong)0x40UL;
        public static readonly ulong N_WEAK_DEF = (ulong)0x80UL;


        public static readonly long LdMachoCpuVax = (long)1L;
        public static readonly long LdMachoCpu68000 = (long)6L;
        public static readonly long LdMachoCpu386 = (long)7L;
        public static readonly ulong LdMachoCpuAmd64 = (ulong)0x1000007UL;
        public static readonly long LdMachoCpuMips = (long)8L;
        public static readonly long LdMachoCpu98000 = (long)10L;
        public static readonly long LdMachoCpuHppa = (long)11L;
        public static readonly long LdMachoCpuArm = (long)12L;
        public static readonly long LdMachoCpu88000 = (long)13L;
        public static readonly long LdMachoCpuSparc = (long)14L;
        public static readonly long LdMachoCpu860 = (long)15L;
        public static readonly long LdMachoCpuAlpha = (long)16L;
        public static readonly long LdMachoCpuPower = (long)18L;
        public static readonly long LdMachoCmdSegment = (long)1L;
        public static readonly long LdMachoCmdSymtab = (long)2L;
        public static readonly long LdMachoCmdSymseg = (long)3L;
        public static readonly long LdMachoCmdThread = (long)4L;
        public static readonly long LdMachoCmdDysymtab = (long)11L;
        public static readonly long LdMachoCmdSegment64 = (long)25L;
        public static readonly long LdMachoFileObject = (long)1L;
        public static readonly long LdMachoFileExecutable = (long)2L;
        public static readonly long LdMachoFileFvmlib = (long)3L;
        public static readonly long LdMachoFileCore = (long)4L;
        public static readonly long LdMachoFilePreload = (long)5L;


        private static long unpackcmd(slice<byte> p, ptr<ldMachoObj> _addr_m, ptr<ldMachoCmd> _addr_c, ulong type_, ulong sz)
        {
            ref ldMachoObj m = ref _addr_m.val;
            ref ldMachoCmd c = ref _addr_c.val;

            var e4 = m.e.Uint32;
            var e8 = m.e.Uint64;

            c.type_ = int(type_);
            c.size = uint32(sz);

            if (type_ == LdMachoCmdSegment) 
                if (sz < 56L)
                {
                    return -1L;
                }

                c.seg.name = cstring(p[8L..24L]);
                c.seg.vmaddr = uint64(e4(p[24L..]));
                c.seg.vmsize = uint64(e4(p[28L..]));
                c.seg.fileoff = e4(p[32L..]);
                c.seg.filesz = e4(p[36L..]);
                c.seg.maxprot = e4(p[40L..]);
                c.seg.initprot = e4(p[44L..]);
                c.seg.nsect = e4(p[48L..]);
                c.seg.flags = e4(p[52L..]);
                c.seg.sect = make_slice<ldMachoSect>(c.seg.nsect);
                if (uint32(sz) < 56L + c.seg.nsect * 68L)
                {
                    return -1L;
                }

                p = p[56L..];
                ptr<ldMachoSect> s;
                {
                    long i__prev1 = i;

                    for (long i = 0L; uint32(i) < c.seg.nsect; i++)
                    {
                        s = _addr_c.seg.sect[i];
                        s.name = cstring(p[0L..16L]);
                        s.segname = cstring(p[16L..32L]);
                        s.addr = uint64(e4(p[32L..]));
                        s.size = uint64(e4(p[36L..]));
                        s.off = e4(p[40L..]);
                        s.align = e4(p[44L..]);
                        s.reloff = e4(p[48L..]);
                        s.nreloc = e4(p[52L..]);
                        s.flags = e4(p[56L..]);
                        s.res1 = e4(p[60L..]);
                        s.res2 = e4(p[64L..]);
                        p = p[68L..];
                    }


                    i = i__prev1;
                }
            else if (type_ == LdMachoCmdSegment64) 
                if (sz < 72L)
                {
                    return -1L;
                }

                c.seg.name = cstring(p[8L..24L]);
                c.seg.vmaddr = e8(p[24L..]);
                c.seg.vmsize = e8(p[32L..]);
                c.seg.fileoff = uint32(e8(p[40L..]));
                c.seg.filesz = uint32(e8(p[48L..]));
                c.seg.maxprot = e4(p[56L..]);
                c.seg.initprot = e4(p[60L..]);
                c.seg.nsect = e4(p[64L..]);
                c.seg.flags = e4(p[68L..]);
                c.seg.sect = make_slice<ldMachoSect>(c.seg.nsect);
                if (uint32(sz) < 72L + c.seg.nsect * 80L)
                {
                    return -1L;
                }

                p = p[72L..];
                s = ;
                {
                    long i__prev1 = i;

                    for (i = 0L; uint32(i) < c.seg.nsect; i++)
                    {
                        s = _addr_c.seg.sect[i];
                        s.name = cstring(p[0L..16L]);
                        s.segname = cstring(p[16L..32L]);
                        s.addr = e8(p[32L..]);
                        s.size = e8(p[40L..]);
                        s.off = e4(p[48L..]);
                        s.align = e4(p[52L..]);
                        s.reloff = e4(p[56L..]);
                        s.nreloc = e4(p[60L..]);
                        s.flags = e4(p[64L..]);
                        s.res1 = e4(p[68L..]);
                        s.res2 = e4(p[72L..]); 

                        // p+76 is reserved
                        p = p[80L..];

                    }


                    i = i__prev1;
                }
            else if (type_ == LdMachoCmdSymtab) 
                if (sz < 24L)
                {
                    return -1L;
                }

                c.sym.symoff = e4(p[8L..]);
                c.sym.nsym = e4(p[12L..]);
                c.sym.stroff = e4(p[16L..]);
                c.sym.strsize = e4(p[20L..]);
            else if (type_ == LdMachoCmdDysymtab) 
                if (sz < 80L)
                {
                    return -1L;
                }

                c.dsym.ilocalsym = e4(p[8L..]);
                c.dsym.nlocalsym = e4(p[12L..]);
                c.dsym.iextdefsym = e4(p[16L..]);
                c.dsym.nextdefsym = e4(p[20L..]);
                c.dsym.iundefsym = e4(p[24L..]);
                c.dsym.nundefsym = e4(p[28L..]);
                c.dsym.tocoff = e4(p[32L..]);
                c.dsym.ntoc = e4(p[36L..]);
                c.dsym.modtaboff = e4(p[40L..]);
                c.dsym.nmodtab = e4(p[44L..]);
                c.dsym.extrefsymoff = e4(p[48L..]);
                c.dsym.nextrefsyms = e4(p[52L..]);
                c.dsym.indirectsymoff = e4(p[56L..]);
                c.dsym.nindirectsyms = e4(p[60L..]);
                c.dsym.extreloff = e4(p[64L..]);
                c.dsym.nextrel = e4(p[68L..]);
                c.dsym.locreloff = e4(p[72L..]);
                c.dsym.nlocrel = e4(p[76L..]);
            else 
                return -1L;
                        return 0L;

        }

        private static long macholoadrel(ptr<ldMachoObj> _addr_m, ptr<ldMachoSect> _addr_sect)
        {
            ref ldMachoObj m = ref _addr_m.val;
            ref ldMachoSect sect = ref _addr_sect.val;

            if (sect.rel != null || sect.nreloc == 0L)
            {
                return 0L;
            }

            var rel = make_slice<ldMachoRel>(sect.nreloc);
            var n = int(sect.nreloc * 8L);
            var buf = make_slice<byte>(n);
            m.f.MustSeek(m.@base + int64(sect.reloff), 0L);
            {
                var (_, err) = io.ReadFull(m.f, buf);

                if (err != null)
                {
                    return -1L;
                }

            }

            for (var i = uint32(0L); i < sect.nreloc; i++)
            {
                var r = _addr_rel[i];
                var p = buf[i * 8L..];
                r.addr = m.e.Uint32(p); 

                // TODO(rsc): Wrong interpretation for big-endian bitfields?
                if (r.addr & 0x80000000UL != 0L)
                { 
                    // scatterbrained relocation
                    r.scattered = 1L;

                    var v = r.addr >> (int)(24L);
                    r.addr &= 0xFFFFFFUL;
                    r.type_ = uint8(v & 0xFUL);
                    v >>= 4L;
                    r.length = 1L << (int)((v & 3L));
                    v >>= 2L;
                    r.pcrel = uint8(v & 1L);
                    r.value = m.e.Uint32(p[4L..]);

                }
                else
                {
                    v = m.e.Uint32(p[4L..]);
                    r.symnum = v & 0xFFFFFFUL;
                    v >>= 24L;
                    r.pcrel = uint8(v & 1L);
                    v >>= 1L;
                    r.length = 1L << (int)((v & 3L));
                    v >>= 2L;
                    r.extrn = uint8(v & 1L);
                    v >>= 1L;
                    r.type_ = uint8(v);
                }

            }


            sect.rel = rel;
            return 0L;

        }

        private static long macholoaddsym(ptr<ldMachoObj> _addr_m, ptr<ldMachoDysymtab> _addr_d)
        {
            ref ldMachoObj m = ref _addr_m.val;
            ref ldMachoDysymtab d = ref _addr_d.val;

            var n = int(d.nindirectsyms);

            var p = make_slice<byte>(n * 4L);
            m.f.MustSeek(m.@base + int64(d.indirectsymoff), 0L);
            {
                var (_, err) = io.ReadFull(m.f, p);

                if (err != null)
                {
                    return -1L;
                }

            }


            d.indir = make_slice<uint>(n);
            for (long i = 0L; i < n; i++)
            {
                d.indir[i] = m.e.Uint32(p[4L * i..]);
            }

            return 0L;

        }

        private static long macholoadsym(ptr<ldMachoObj> _addr_m, ptr<ldMachoSymtab> _addr_symtab)
        {
            ref ldMachoObj m = ref _addr_m.val;
            ref ldMachoSymtab symtab = ref _addr_symtab.val;

            if (symtab.sym != null)
            {
                return 0L;
            }

            var strbuf = make_slice<byte>(symtab.strsize);
            m.f.MustSeek(m.@base + int64(symtab.stroff), 0L);
            {
                var (_, err) = io.ReadFull(m.f, strbuf);

                if (err != null)
                {
                    return -1L;
                }

            }


            long symsize = 12L;
            if (m.is64)
            {
                symsize = 16L;
            }

            var n = int(symtab.nsym * uint32(symsize));
            var symbuf = make_slice<byte>(n);
            m.f.MustSeek(m.@base + int64(symtab.symoff), 0L);
            {
                (_, err) = io.ReadFull(m.f, symbuf);

                if (err != null)
                {
                    return -1L;
                }

            }

            var sym = make_slice<ldMachoSym>(symtab.nsym);
            var p = symbuf;
            for (var i = uint32(0L); i < symtab.nsym; i++)
            {
                var s = _addr_sym[i];
                var v = m.e.Uint32(p);
                if (v >= symtab.strsize)
                {
                    return -1L;
                }

                s.name = cstring(strbuf[v..]);
                s.type_ = p[4L];
                s.sectnum = p[5L];
                s.desc = m.e.Uint16(p[6L..]);
                if (m.is64)
                {
                    s.value = m.e.Uint64(p[8L..]);
                }
                else
                {
                    s.value = uint64(m.e.Uint32(p[8L..]));
                }

                p = p[symsize..];

            }


            symtab.str = strbuf;
            symtab.sym = sym;
            return 0L;

        }

        public static (slice<ptr<sym.Symbol>>, error) Load(ptr<loader.Loader> _addr_l, ptr<sys.Arch> _addr_arch, ptr<sym.Symbols> _addr_syms, ptr<bio.Reader> _addr_f, @string pkg, long length, @string pn)
        {
            slice<ptr<sym.Symbol>> _p0 = default;
            error _p0 = default!;
            ref loader.Loader l = ref _addr_l.val;
            ref sys.Arch arch = ref _addr_arch.val;
            ref sym.Symbols syms = ref _addr_syms.val;
            ref bio.Reader f = ref _addr_f.val;

            Func<@string, long, ptr<sym.Symbol>> newSym = (name, version) =>
            {
                return l.LookupOrCreate(name, version, syms);
            }
;
            return load(_addr_arch, syms.IncVersion(), newSym, _addr_f, pkg, length, pn);

        }

        public static (slice<ptr<sym.Symbol>>, error) LoadOld(ptr<sys.Arch> _addr_arch, ptr<sym.Symbols> _addr_syms, ptr<bio.Reader> _addr_f, @string pkg, long length, @string pn)
        {
            slice<ptr<sym.Symbol>> textp = default;
            error err = default!;
            ref sys.Arch arch = ref _addr_arch.val;
            ref sym.Symbols syms = ref _addr_syms.val;
            ref bio.Reader f = ref _addr_f.val;

            return load(_addr_arch, syms.IncVersion(), syms.Lookup, _addr_f, pkg, length, pn);
        }

        // load the Mach-O file pn from f.
        // Symbols are written into syms, and a slice of the text symbols is returned.
        private static (slice<ptr<sym.Symbol>>, error) load(ptr<sys.Arch> _addr_arch, long localSymVersion, Func<@string, long, ptr<sym.Symbol>> lookup, ptr<bio.Reader> _addr_f, @string pkg, long length, @string pn)
        {
            slice<ptr<sym.Symbol>> textp = default;
            error err = default!;
            ref sys.Arch arch = ref _addr_arch.val;
            ref bio.Reader f = ref _addr_f.val;

            Func<@string, object[], (slice<ptr<sym.Symbol>>, error)> errorf = (str, args) =>
            {
                return (null, error.As(fmt.Errorf("loadmacho: %v: %v", pn, fmt.Sprintf(str, args)))!);
            }
;

            var @base = f.Offset();

            array<byte> hdr = new array<byte>(7L * 4L);
            {
                var (_, err) = io.ReadFull(f, hdr[..]);

                if (err != null)
                {
                    return errorf("reading hdr: %v", err);
                }

            }


            binary.ByteOrder e = default;
            if (binary.BigEndian.Uint32(hdr[..]) & ~1L == 0xFEEDFACEUL)
            {
                e = binary.BigEndian;
            }
            else if (binary.LittleEndian.Uint32(hdr[..]) & ~1L == 0xFEEDFACEUL)
            {
                e = binary.LittleEndian;
            }
            else
            {
                return errorf("bad magic - not mach-o file");
            }

            var is64 = e.Uint32(hdr[..]) == 0xFEEDFACFUL;
            var ncmd = e.Uint32(hdr[4L * 4L..]);
            var cmdsz = e.Uint32(hdr[5L * 4L..]);
            if (ncmd > 0x10000UL || cmdsz >= 0x01000000UL)
            {
                return errorf("implausible mach-o header ncmd=%d cmdsz=%d", ncmd, cmdsz);
            }

            if (is64)
            {
                f.MustSeek(4L, 1L); // skip reserved word in header
            }

            ptr<ldMachoObj> m = addr(new ldMachoObj(f:f,e:e,cputype:uint(e.Uint32(hdr[1*4:])),subcputype:uint(e.Uint32(hdr[2*4:])),filetype:e.Uint32(hdr[3*4:]),ncmd:uint(ncmd),flags:e.Uint32(hdr[6*4:]),is64:is64,base:base,length:length,name:pn,));


            if (arch.Family == sys.AMD64) 
                if (e != binary.LittleEndian || m.cputype != LdMachoCpuAmd64)
                {
                    return errorf("mach-o object but not amd64");
                }

            else 
                return errorf("mach-o %s unimplemented", arch.Name);
                        m.cmd = make_slice<ldMachoCmd>(ncmd);
            var cmdp = make_slice<byte>(cmdsz);
            {
                (_, err) = io.ReadFull(f, cmdp);

                if (err != null)
                {
                    return errorf("reading cmds: %v", err);
                } 

                // read and parse load commands

            } 

            // read and parse load commands
            ptr<ldMachoCmd> c;

            ptr<ldMachoSymtab> symtab;
            ptr<ldMachoDysymtab> dsymtab;

            var off = uint32(len(hdr));
            {
                var i__prev1 = i;

                for (var i = uint32(0L); i < ncmd; i++)
                {
                    var ty = e.Uint32(cmdp);
                    var sz = e.Uint32(cmdp[4L..]);
                    m.cmd[i].off = off;
                    unpackcmd(cmdp, m, _addr_m.cmd[i], uint(ty), uint(sz));
                    cmdp = cmdp[sz..];
                    off += sz;
                    if (ty == LdMachoCmdSymtab)
                    {
                        if (symtab != null)
                        {
                            return errorf("multiple symbol tables");
                        }

                        symtab = _addr_m.cmd[i].sym;
                        macholoadsym(m, symtab);

                    }

                    if (ty == LdMachoCmdDysymtab)
                    {
                        dsymtab = _addr_m.cmd[i].dsym;
                        macholoaddsym(m, dsymtab);
                    }

                    if ((is64 && ty == LdMachoCmdSegment64) || (!is64 && ty == LdMachoCmdSegment))
                    {
                        if (c != null)
                        {
                            return errorf("multiple load commands");
                        }

                        c = _addr_m.cmd[i];

                    }

                } 

                // load text and data segments into memory.
                // they are not as small as the load commands, but we'll need
                // the memory anyway for the symbol images, so we might
                // as well use one large chunk.


                i = i__prev1;
            } 

            // load text and data segments into memory.
            // they are not as small as the load commands, but we'll need
            // the memory anyway for the symbol images, so we might
            // as well use one large chunk.
            if (c == null)
            {
                return errorf("no load command");
            }

            if (symtab == null)
            { 
                // our work is done here - no symbols means nothing can refer to this file
                return ;

            }

            if (int64(c.seg.fileoff + c.seg.filesz) >= length)
            {
                return errorf("load segment out of range");
            }

            f.MustSeek(m.@base + int64(c.seg.fileoff), 0L);
            var dat = make_slice<byte>(c.seg.filesz);
            {
                (_, err) = io.ReadFull(f, dat);

                if (err != null)
                {
                    return errorf("cannot load object data: %v", err);
                }

            }


            {
                var i__prev1 = i;

                for (i = uint32(0L); i < c.seg.nsect; i++)
                {
                    var sect = _addr_c.seg.sect[i];
                    if (sect.segname != "__TEXT" && sect.segname != "__DATA")
                    {
                        continue;
                    }

                    if (sect.name == "__eh_frame")
                    {
                        continue;
                    }

                    var name = fmt.Sprintf("%s(%s/%s)", pkg, sect.segname, sect.name);
                    var s = lookup(name, localSymVersion);
                    if (s.Type != 0L)
                    {
                        return errorf("duplicate %s/%s", sect.segname, sect.name);
                    }

                    if (sect.flags & 0xffUL == 1L)
                    { // S_ZEROFILL
                        s.P = make_slice<byte>(sect.size);

                    }
                    else
                    {
                        s.P = dat[sect.addr - c.seg.vmaddr..][..sect.size];
                    }

                    s.Size = int64(len(s.P));

                    if (sect.segname == "__TEXT")
                    {
                        if (sect.name == "__text")
                        {
                            s.Type = sym.STEXT;
                        }
                        else
                        {
                            s.Type = sym.SRODATA;
                        }

                    }
                    else
                    {
                        if (sect.name == "__bss")
                        {
                            s.Type = sym.SNOPTRBSS;
                            s.P = s.P[..0L];
                        }
                        else
                        {
                            s.Type = sym.SNOPTRDATA;
                        }

                    }

                    sect.sym = s;

                } 

                // enter sub-symbols into symbol table.
                // have to guess sizes from next symbol.


                i = i__prev1;
            } 

            // enter sub-symbols into symbol table.
            // have to guess sizes from next symbol.
            {
                var i__prev1 = i;

                for (i = uint32(0L); i < symtab.nsym; i++)
                {
                    var machsym = _addr_symtab.sym[i];
                    if (machsym.type_ & N_STAB != 0L)
                    {
                        continue;
                    } 

                    // TODO: check sym->type against outer->type.
                    name = machsym.name;

                    if (name[0L] == '_' && name[1L] != '\x00')
                    {
                        name = name[1L..];
                    }

                    long v = 0L;
                    if (machsym.type_ & N_EXT == 0L)
                    {
                        v = localSymVersion;
                    }

                    s = lookup(name, v);
                    if (machsym.type_ & N_EXT == 0L)
                    {
                        s.Attr |= sym.AttrDuplicateOK;
                    }

                    if (machsym.desc & (N_WEAK_REF | N_WEAK_DEF) != 0L)
                    {
                        s.Attr |= sym.AttrDuplicateOK;
                    }

                    machsym.sym = s;
                    if (machsym.sectnum == 0L)
                    { // undefined
                        continue;

                    }

                    if (uint32(machsym.sectnum) > c.seg.nsect)
                    {
                        return errorf("reference to invalid section %d", machsym.sectnum);
                    }

                    sect = _addr_c.seg.sect[machsym.sectnum - 1L];
                    var outer = sect.sym;
                    if (outer == null)
                    {
                        continue; // ignore reference to invalid section
                    }

                    if (s.Outer != null)
                    {
                        if (s.Attr.DuplicateOK())
                        {
                            continue;
                        }

                        return errorf("duplicate symbol reference: %s in both %s and %s", s.Name, s.Outer.Name, sect.sym.Name);

                    }

                    s.Type = outer.Type;
                    s.Attr |= sym.AttrSubSymbol;
                    s.Sub = outer.Sub;
                    outer.Sub = s;
                    s.Outer = outer;
                    s.Value = int64(machsym.value - sect.addr);
                    if (!s.Attr.CgoExportDynamic())
                    {
                        s.SetDynimplib(""); // satisfy dynimport
                    }

                    if (outer.Type == sym.STEXT)
                    {
                        if (s.Attr.External() && !s.Attr.DuplicateOK())
                        {
                            return errorf("%v: duplicate symbol definition", s);
                        }

                        s.Attr |= sym.AttrExternal;

                    }

                    machsym.sym = s;

                } 

                // Sort outer lists by address, adding to textp.
                // This keeps textp in increasing address order.


                i = i__prev1;
            } 

            // Sort outer lists by address, adding to textp.
            // This keeps textp in increasing address order.
            {
                var i__prev1 = i;

                for (i = 0L; uint32(i) < c.seg.nsect; i++)
                {
                    sect = _addr_c.seg.sect[i];
                    s = sect.sym;
                    if (s == null)
                    {
                        continue;
                    }

                    if (s.Sub != null)
                    {
                        s.Sub = sym.SortSub(s.Sub); 

                        // assign sizes, now that we know symbols in sorted order.
                        {
                            var s1__prev2 = s1;

                            var s1 = s.Sub;

                            while (s1 != null)
                            {
                                if (s1.Sub != null)
                                {
                                    s1.Size = s1.Sub.Value - s1.Value;
                                s1 = s1.Sub;
                                }
                                else
                                {
                                    s1.Size = s.Value + s.Size - s1.Value;
                                }

                            }


                            s1 = s1__prev2;
                        }

                    }

                    if (s.Type == sym.STEXT)
                    {
                        if (s.Attr.OnList())
                        {
                            return errorf("symbol %s listed multiple times", s.Name);
                        }

                        s.Attr |= sym.AttrOnList;
                        textp = append(textp, s);
                        {
                            var s1__prev2 = s1;

                            s1 = s.Sub;

                            while (s1 != null)
                            {
                                if (s1.Attr.OnList())
                                {
                                    return errorf("symbol %s listed multiple times", s1.Name);
                                s1 = s1.Sub;
                                }

                                s1.Attr |= sym.AttrOnList;
                                textp = append(textp, s1);

                            }


                            s1 = s1__prev2;
                        }

                    }

                } 

                // load relocations


                i = i__prev1;
            } 

            // load relocations
            {
                var i__prev1 = i;

                for (i = 0L; uint32(i) < c.seg.nsect; i++)
                {
                    sect = _addr_c.seg.sect[i];
                    s = sect.sym;
                    if (s == null)
                    {
                        continue;
                    }

                    macholoadrel(m, _addr_sect);
                    if (sect.rel == null)
                    {
                        continue;
                    }

                    var r = make_slice<sym.Reloc>(sect.nreloc);
                    long rpi = 0L;
                    for (var j = uint32(0L); j < sect.nreloc; j++)
                    {
                        var rp = _addr_r[rpi];
                        var rel = _addr_sect.rel[j];
                        if (rel.scattered != 0L)
                        { 
                            // mach-o only uses scattered relocation on 32-bit platforms,
                            // which are no longer supported.
                            return errorf("%v: unexpected scattered relocation", s);

                        }

                        rp.Siz = rel.length;
                        rp.Type = objabi.MachoRelocOffset + (objabi.RelocType(rel.type_) << (int)(1L)) + objabi.RelocType(rel.pcrel);
                        rp.Off = int32(rel.addr); 

                        // Handle X86_64_RELOC_SIGNED referencing a section (rel->extrn == 0).
                        if (arch.Family == sys.AMD64 && rel.extrn == 0L && rel.type_ == MACHO_X86_64_RELOC_SIGNED)
                        { 
                            // Calculate the addend as the offset into the section.
                            //
                            // The rip-relative offset stored in the object file is encoded
                            // as follows:
                            //
                            //    movsd    0x00000360(%rip),%xmm0
                            //
                            // To get the absolute address of the value this rip-relative address is pointing
                            // to, we must add the address of the next instruction to it. This is done by
                            // taking the address of the relocation and adding 4 to it (since the rip-relative
                            // offset can at most be 32 bits long).  To calculate the offset into the section the
                            // relocation is referencing, we subtract the vaddr of the start of the referenced
                            // section found in the original object file.
                            //
                            // [For future reference, see Darwin's /usr/include/mach-o/x86_64/reloc.h]
                            var secaddr = c.seg.sect[rel.symnum - 1L].addr;

                            rp.Add = int64(uint64(int64(int32(e.Uint32(s.P[rp.Off..]))) + int64(rp.Off) + 4L) - secaddr);

                        }
                        else
                        {
                            rp.Add = int64(int32(e.Uint32(s.P[rp.Off..])));
                        } 

                        // An unsigned internal relocation has a value offset
                        // by the section address.
                        if (arch.Family == sys.AMD64 && rel.extrn == 0L && rel.type_ == MACHO_X86_64_RELOC_UNSIGNED)
                        {
                            secaddr = c.seg.sect[rel.symnum - 1L].addr;
                            rp.Add -= int64(secaddr);
                        }

                        if (rel.extrn == 0L)
                        {
                            if (rel.symnum < 1L || rel.symnum > c.seg.nsect)
                            {
                                return errorf("invalid relocation: section reference out of range %d vs %d", rel.symnum, c.seg.nsect);
                            }

                            rp.Sym = c.seg.sect[rel.symnum - 1L].sym;
                            if (rp.Sym == null)
                            {
                                return errorf("invalid relocation: %s", c.seg.sect[rel.symnum - 1L].name);
                            }

                        }
                        else
                        {
                            if (rel.symnum >= symtab.nsym)
                            {
                                return errorf("invalid relocation: symbol reference out of range");
                            }

                            rp.Sym = symtab.sym[rel.symnum].sym;

                        }

                        rpi++;

                    }


                    sort.Sort(sym.RelocByOff(r[..rpi]));
                    s.R = r;
                    s.R = s.R[..rpi];

                }


                i = i__prev1;
            }

            return (textp, error.As(null!)!);

        }

        private static @string cstring(slice<byte> x)
        {
            var i = bytes.IndexByte(x, '\x00');
            if (i >= 0L)
            {
                x = x[..i];
            }

            return string(x);

        }
    }
}}}}
