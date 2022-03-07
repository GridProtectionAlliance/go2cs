// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package loadmacho implements a Mach-O file reader.
// package loadmacho -- go2cs converted at 2022 March 06 23:21:45 UTC
// import "cmd/link/internal/loadmacho" ==> using loadmacho = go.cmd.link.@internal.loadmacho_package
// Original source: C:\Program Files\Go\src\cmd\link\internal\loadmacho\ldmacho.go
using bytes = go.bytes_package;
using bio = go.cmd.@internal.bio_package;
using objabi = go.cmd.@internal.objabi_package;
using sys = go.cmd.@internal.sys_package;
using loader = go.cmd.link.@internal.loader_package;
using sym = go.cmd.link.@internal.sym_package;
using binary = go.encoding.binary_package;
using fmt = go.fmt_package;
using System;


namespace go.cmd.link.@internal;

public static partial class loadmacho_package {

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

    // TODO(crawshaw): de-duplicate these symbols with cmd/link/internal/ld
public static readonly nint MACHO_X86_64_RELOC_UNSIGNED = 0;
public static readonly nint MACHO_X86_64_RELOC_SIGNED = 1;
public static readonly nint MACHO_ARM64_RELOC_ADDEND = 10;


private partial struct ldMachoObj {
    public ptr<bio.Reader> f;
    public long @base; // off in f where Mach-O begins
    public long length; // length of Mach-O
    public bool is64;
    public @string name;
    public binary.ByteOrder e;
    public nuint cputype;
    public nuint subcputype;
    public uint filetype;
    public uint flags;
    public slice<ldMachoCmd> cmd;
    public nuint ncmd;
}

private partial struct ldMachoCmd {
    public nint type_;
    public uint off;
    public uint size;
    public ldMachoSeg seg;
    public ldMachoSymtab sym;
    public ldMachoDysymtab dsym;
}

private partial struct ldMachoSeg {
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

private partial struct ldMachoSect {
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
    public loader.Sym sym;
    public slice<ldMachoRel> rel;
}

private partial struct ldMachoRel {
    public uint addr;
    public uint symnum;
    public byte pcrel;
    public byte length;
    public byte extrn;
    public byte type_;
    public byte scattered;
    public uint value;
}

private partial struct ldMachoSymtab {
    public uint symoff;
    public uint nsym;
    public uint stroff;
    public uint strsize;
    public slice<byte> str;
    public slice<ldMachoSym> sym;
}

private partial struct ldMachoSym {
    public @string name;
    public byte type_;
    public byte sectnum;
    public ushort desc;
    public sbyte kind;
    public ulong value;
    public loader.Sym sym;
}

private partial struct ldMachoDysymtab {
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
public static readonly nuint N_EXT = 0x01;
public static readonly nuint N_TYPE = 0x1e;
public static readonly nuint N_STAB = 0xe0;


// ldMachoSym.desc
public static readonly nuint N_WEAK_REF = 0x40;
public static readonly nuint N_WEAK_DEF = 0x80;


public static readonly nint LdMachoCpuVax = 1;
public static readonly nint LdMachoCpu68000 = 6;
public static readonly nint LdMachoCpu386 = 7;
public static readonly nint LdMachoCpuAmd64 = 1 << 24 | 7;
public static readonly nint LdMachoCpuMips = 8;
public static readonly nint LdMachoCpu98000 = 10;
public static readonly nint LdMachoCpuHppa = 11;
public static readonly nint LdMachoCpuArm = 12;
public static readonly nint LdMachoCpuArm64 = 1 << 24 | 12;
public static readonly nint LdMachoCpu88000 = 13;
public static readonly nint LdMachoCpuSparc = 14;
public static readonly nint LdMachoCpu860 = 15;
public static readonly nint LdMachoCpuAlpha = 16;
public static readonly nint LdMachoCpuPower = 18;
public static readonly nint LdMachoCmdSegment = 1;
public static readonly nint LdMachoCmdSymtab = 2;
public static readonly nint LdMachoCmdSymseg = 3;
public static readonly nint LdMachoCmdThread = 4;
public static readonly nint LdMachoCmdDysymtab = 11;
public static readonly nint LdMachoCmdSegment64 = 25;
public static readonly nint LdMachoFileObject = 1;
public static readonly nint LdMachoFileExecutable = 2;
public static readonly nint LdMachoFileFvmlib = 3;
public static readonly nint LdMachoFileCore = 4;
public static readonly nint LdMachoFilePreload = 5;


private static nint unpackcmd(slice<byte> p, ptr<ldMachoObj> _addr_m, ptr<ldMachoCmd> _addr_c, nuint type_, nuint sz) {
    ref ldMachoObj m = ref _addr_m.val;
    ref ldMachoCmd c = ref _addr_c.val;

    var e4 = m.e.Uint32;
    var e8 = m.e.Uint64;

    c.type_ = int(type_);
    c.size = uint32(sz);

    if (type_ == LdMachoCmdSegment) 
        if (sz < 56) {
            return -1;
        }
        c.seg.name = cstring(p[(int)8..(int)24]);
        c.seg.vmaddr = uint64(e4(p[(int)24..]));
        c.seg.vmsize = uint64(e4(p[(int)28..]));
        c.seg.fileoff = e4(p[(int)32..]);
        c.seg.filesz = e4(p[(int)36..]);
        c.seg.maxprot = e4(p[(int)40..]);
        c.seg.initprot = e4(p[(int)44..]);
        c.seg.nsect = e4(p[(int)48..]);
        c.seg.flags = e4(p[(int)52..]);
        c.seg.sect = make_slice<ldMachoSect>(c.seg.nsect);
        if (uint32(sz) < 56 + c.seg.nsect * 68) {
            return -1;
        }
        p = p[(int)56..];
        ptr<ldMachoSect> s;
        {
            nint i__prev1 = i;

            for (nint i = 0; uint32(i) < c.seg.nsect; i++) {
                s = _addr_c.seg.sect[i];
                s.name = cstring(p[(int)0..(int)16]);
                s.segname = cstring(p[(int)16..(int)32]);
                s.addr = uint64(e4(p[(int)32..]));
                s.size = uint64(e4(p[(int)36..]));
                s.off = e4(p[(int)40..]);
                s.align = e4(p[(int)44..]);
                s.reloff = e4(p[(int)48..]);
                s.nreloc = e4(p[(int)52..]);
                s.flags = e4(p[(int)56..]);
                s.res1 = e4(p[(int)60..]);
                s.res2 = e4(p[(int)64..]);
                p = p[(int)68..];
            }


            i = i__prev1;
        }
    else if (type_ == LdMachoCmdSegment64) 
        if (sz < 72) {
            return -1;
        }
        c.seg.name = cstring(p[(int)8..(int)24]);
        c.seg.vmaddr = e8(p[(int)24..]);
        c.seg.vmsize = e8(p[(int)32..]);
        c.seg.fileoff = uint32(e8(p[(int)40..]));
        c.seg.filesz = uint32(e8(p[(int)48..]));
        c.seg.maxprot = e4(p[(int)56..]);
        c.seg.initprot = e4(p[(int)60..]);
        c.seg.nsect = e4(p[(int)64..]);
        c.seg.flags = e4(p[(int)68..]);
        c.seg.sect = make_slice<ldMachoSect>(c.seg.nsect);
        if (uint32(sz) < 72 + c.seg.nsect * 80) {
            return -1;
        }
        p = p[(int)72..];
        s = ;
        {
            nint i__prev1 = i;

            for (i = 0; uint32(i) < c.seg.nsect; i++) {
                s = _addr_c.seg.sect[i];
                s.name = cstring(p[(int)0..(int)16]);
                s.segname = cstring(p[(int)16..(int)32]);
                s.addr = e8(p[(int)32..]);
                s.size = e8(p[(int)40..]);
                s.off = e4(p[(int)48..]);
                s.align = e4(p[(int)52..]);
                s.reloff = e4(p[(int)56..]);
                s.nreloc = e4(p[(int)60..]);
                s.flags = e4(p[(int)64..]);
                s.res1 = e4(p[(int)68..]);
                s.res2 = e4(p[(int)72..]); 

                // p+76 is reserved
                p = p[(int)80..];

            }


            i = i__prev1;
        }
    else if (type_ == LdMachoCmdSymtab) 
        if (sz < 24) {
            return -1;
        }
        c.sym.symoff = e4(p[(int)8..]);
        c.sym.nsym = e4(p[(int)12..]);
        c.sym.stroff = e4(p[(int)16..]);
        c.sym.strsize = e4(p[(int)20..]);
    else if (type_ == LdMachoCmdDysymtab) 
        if (sz < 80) {
            return -1;
        }
        c.dsym.ilocalsym = e4(p[(int)8..]);
        c.dsym.nlocalsym = e4(p[(int)12..]);
        c.dsym.iextdefsym = e4(p[(int)16..]);
        c.dsym.nextdefsym = e4(p[(int)20..]);
        c.dsym.iundefsym = e4(p[(int)24..]);
        c.dsym.nundefsym = e4(p[(int)28..]);
        c.dsym.tocoff = e4(p[(int)32..]);
        c.dsym.ntoc = e4(p[(int)36..]);
        c.dsym.modtaboff = e4(p[(int)40..]);
        c.dsym.nmodtab = e4(p[(int)44..]);
        c.dsym.extrefsymoff = e4(p[(int)48..]);
        c.dsym.nextrefsyms = e4(p[(int)52..]);
        c.dsym.indirectsymoff = e4(p[(int)56..]);
        c.dsym.nindirectsyms = e4(p[(int)60..]);
        c.dsym.extreloff = e4(p[(int)64..]);
        c.dsym.nextrel = e4(p[(int)68..]);
        c.dsym.locreloff = e4(p[(int)72..]);
        c.dsym.nlocrel = e4(p[(int)76..]);
    else 
        return -1;
        return 0;

}

private static nint macholoadrel(ptr<ldMachoObj> _addr_m, ptr<ldMachoSect> _addr_sect) {
    ref ldMachoObj m = ref _addr_m.val;
    ref ldMachoSect sect = ref _addr_sect.val;

    if (sect.rel != null || sect.nreloc == 0) {
        return 0;
    }
    var rel = make_slice<ldMachoRel>(sect.nreloc);
    m.f.MustSeek(m.@base + int64(sect.reloff), 0);
    var (buf, _, err) = m.f.Slice(uint64(sect.nreloc * 8));
    if (err != null) {
        return -1;
    }
    for (var i = uint32(0); i < sect.nreloc; i++) {
        var r = _addr_rel[i];
        var p = buf[(int)i * 8..];
        r.addr = m.e.Uint32(p); 

        // TODO(rsc): Wrong interpretation for big-endian bitfields?
        if (r.addr & 0x80000000 != 0) { 
            // scatterbrained relocation
            r.scattered = 1;

            var v = r.addr >> 24;
            r.addr &= 0xFFFFFF;
            r.type_ = uint8(v & 0xF);
            v>>=4;
            r.length = 1 << (int)((v & 3));
            v>>=2;
            r.pcrel = uint8(v & 1);
            r.value = m.e.Uint32(p[(int)4..]);

        }
        else
 {
            v = m.e.Uint32(p[(int)4..]);
            r.symnum = v & 0xFFFFFF;
            v>>=24;
            r.pcrel = uint8(v & 1);
            v>>=1;
            r.length = 1 << (int)((v & 3));
            v>>=2;
            r.extrn = uint8(v & 1);
            v>>=1;
            r.type_ = uint8(v);
        }
    }

    sect.rel = rel;
    return 0;

}

private static nint macholoaddsym(ptr<ldMachoObj> _addr_m, ptr<ldMachoDysymtab> _addr_d) {
    ref ldMachoObj m = ref _addr_m.val;
    ref ldMachoDysymtab d = ref _addr_d.val;

    var n = int(d.nindirectsyms);
    m.f.MustSeek(m.@base + int64(d.indirectsymoff), 0);
    var (p, _, err) = m.f.Slice(uint64(n * 4));
    if (err != null) {
        return -1;
    }
    d.indir = make_slice<uint>(n);
    for (nint i = 0; i < n; i++) {
        d.indir[i] = m.e.Uint32(p[(int)4 * i..]);
    }
    return 0;

}

private static nint macholoadsym(ptr<ldMachoObj> _addr_m, ptr<ldMachoSymtab> _addr_symtab) {
    ref ldMachoObj m = ref _addr_m.val;
    ref ldMachoSymtab symtab = ref _addr_symtab.val;

    if (symtab.sym != null) {
        return 0;
    }
    m.f.MustSeek(m.@base + int64(symtab.stroff), 0);
    var (strbuf, _, err) = m.f.Slice(uint64(symtab.strsize));
    if (err != null) {
        return -1;
    }
    nint symsize = 12;
    if (m.is64) {
        symsize = 16;
    }
    var n = int(symtab.nsym * uint32(symsize));
    m.f.MustSeek(m.@base + int64(symtab.symoff), 0);
    var (symbuf, _, err) = m.f.Slice(uint64(n));
    if (err != null) {
        return -1;
    }
    var sym = make_slice<ldMachoSym>(symtab.nsym);
    var p = symbuf;
    for (var i = uint32(0); i < symtab.nsym; i++) {
        var s = _addr_sym[i];
        var v = m.e.Uint32(p);
        if (v >= symtab.strsize) {
            return -1;
        }
        s.name = cstring(strbuf[(int)v..]);
        s.type_ = p[4];
        s.sectnum = p[5];
        s.desc = m.e.Uint16(p[(int)6..]);
        if (m.is64) {
            s.value = m.e.Uint64(p[(int)8..]);
        }
        else
 {
            s.value = uint64(m.e.Uint32(p[(int)8..]));
        }
        p = p[(int)symsize..];

    }

    symtab.str = strbuf;
    symtab.sym = sym;
    return 0;

}

// Load the Mach-O file pn from f.
// Symbols are written into syms, and a slice of the text symbols is returned.
public static (slice<loader.Sym>, error) Load(ptr<loader.Loader> _addr_l, ptr<sys.Arch> _addr_arch, nint localSymVersion, ptr<bio.Reader> _addr_f, @string pkg, long length, @string pn) {
    slice<loader.Sym> textp = default;
    error err = default!;
    ref loader.Loader l = ref _addr_l.val;
    ref sys.Arch arch = ref _addr_arch.val;
    ref bio.Reader f = ref _addr_f.val;

    Func<@string, object[], (slice<loader.Sym>, error)> errorf = (str, args) => {
        return (null, error.As(fmt.Errorf("loadmacho: %v: %v", pn, fmt.Sprintf(str, args)))!);
    };

    var @base = f.Offset();

    var (hdr, _, err) = f.Slice(7 * 4);
    if (err != null) {
        return errorf("reading hdr: %v", err);
    }
    binary.ByteOrder e = default;
    if (binary.BigEndian.Uint32(hdr[..]) & ~1 == 0xFEEDFACE) {
        e = binary.BigEndian;
    }
    else if (binary.LittleEndian.Uint32(hdr[..]) & ~1 == 0xFEEDFACE) {
        e = binary.LittleEndian;
    }
    else
 {
        return errorf("bad magic - not mach-o file");
    }
    var is64 = e.Uint32(hdr[..]) == 0xFEEDFACF;
    var ncmd = e.Uint32(hdr[(int)4 * 4..]);
    var cmdsz = e.Uint32(hdr[(int)5 * 4..]);
    if (ncmd > 0x10000 || cmdsz >= 0x01000000) {
        return errorf("implausible mach-o header ncmd=%d cmdsz=%d", ncmd, cmdsz);
    }
    if (is64) {
        f.MustSeek(4, 1); // skip reserved word in header
    }
    ptr<ldMachoObj> m = addr(new ldMachoObj(f:f,e:e,cputype:uint(e.Uint32(hdr[1*4:])),subcputype:uint(e.Uint32(hdr[2*4:])),filetype:e.Uint32(hdr[3*4:]),ncmd:uint(ncmd),flags:e.Uint32(hdr[6*4:]),is64:is64,base:base,length:length,name:pn,));


    if (arch.Family == sys.AMD64) 
        if (e != binary.LittleEndian || m.cputype != LdMachoCpuAmd64) {
            return errorf("mach-o object but not amd64");
        }
    else if (arch.Family == sys.ARM64) 
        if (e != binary.LittleEndian || m.cputype != LdMachoCpuArm64) {
            return errorf("mach-o object but not arm64");
        }
    else 
        return errorf("mach-o %s unimplemented", arch.Name);
        m.cmd = make_slice<ldMachoCmd>(ncmd);
    var (cmdp, _, err) = f.Slice(uint64(cmdsz));
    if (err != null) {
        return errorf("reading cmds: %v", err);
    }
    ptr<ldMachoCmd> c;

    ptr<ldMachoSymtab> symtab;
    ptr<ldMachoDysymtab> dsymtab;

    var off = uint32(len(hdr));
    {
        var i__prev1 = i;

        for (var i = uint32(0); i < ncmd; i++) {
            var ty = e.Uint32(cmdp);
            var sz = e.Uint32(cmdp[(int)4..]);
            m.cmd[i].off = off;
            unpackcmd(cmdp, m, _addr_m.cmd[i], uint(ty), uint(sz));
            cmdp = cmdp[(int)sz..];
            off += sz;
            if (ty == LdMachoCmdSymtab) {
                if (symtab != null) {
                    return errorf("multiple symbol tables");
                }
                symtab = _addr_m.cmd[i].sym;
                macholoadsym(m, symtab);
            }
            if (ty == LdMachoCmdDysymtab) {
                dsymtab = _addr_m.cmd[i].dsym;
                macholoaddsym(m, dsymtab);
            }
            if ((is64 && ty == LdMachoCmdSegment64) || (!is64 && ty == LdMachoCmdSegment)) {
                if (c != null) {
                    return errorf("multiple load commands");
                }
                c = _addr_m.cmd[i];
            }
        }

        i = i__prev1;
    } 

    // load text and data segments into memory.
    // they are not as small as the load commands, but we'll need
    // the memory anyway for the symbol images, so we might
    // as well use one large chunk.
    if (c == null) {
        return errorf("no load command");
    }
    if (symtab == null) { 
        // our work is done here - no symbols means nothing can refer to this file
        return ;

    }
    if (int64(c.seg.fileoff + c.seg.filesz) >= length) {
        return errorf("load segment out of range");
    }
    f.MustSeek(m.@base + int64(c.seg.fileoff), 0);
    var (dat, readOnly, err) = f.Slice(uint64(c.seg.filesz));
    if (err != null) {
        return errorf("cannot load object data: %v", err);
    }
    {
        var i__prev1 = i;

        for (i = uint32(0); i < c.seg.nsect; i++) {
            var sect = _addr_c.seg.sect[i];
            if (sect.segname != "__TEXT" && sect.segname != "__DATA") {
                continue;
            }
            if (sect.name == "__eh_frame") {
                continue;
            }
            var name = fmt.Sprintf("%s(%s/%s)", pkg, sect.segname, sect.name);
            var s = l.LookupOrCreateSym(name, localSymVersion);
            var bld = l.MakeSymbolUpdater(s);
            if (bld.Type() != 0) {
                return errorf("duplicate %s/%s", sect.segname, sect.name);
            }
            if (sect.flags & 0xff == 1) { // S_ZEROFILL
                bld.SetData(make_slice<byte>(sect.size));

            }
            else
 {
                bld.SetReadOnly(readOnly);
                bld.SetData(dat[(int)sect.addr - c.seg.vmaddr..][..(int)sect.size]);
            }

            bld.SetSize(int64(len(bld.Data())));

            if (sect.segname == "__TEXT") {
                if (sect.name == "__text") {
                    bld.SetType(sym.STEXT);
                }
                else
 {
                    bld.SetType(sym.SRODATA);
                }

            }
            else
 {
                if (sect.name == "__bss") {
                    bld.SetType(sym.SNOPTRBSS);
                    bld.SetData(null);
                }
                else
 {
                    bld.SetType(sym.SNOPTRDATA);
                }

            }

            sect.sym = s;

        }

        i = i__prev1;
    } 

    // enter sub-symbols into symbol table.
    // have to guess sizes from next symbol.
    {
        var i__prev1 = i;

        for (i = uint32(0); i < symtab.nsym; i++) {
            var machsym = _addr_symtab.sym[i];
            if (machsym.type_ & N_STAB != 0) {
                continue;
            } 

            // TODO: check sym->type against outer->type.
            name = machsym.name;

            if (name[0] == '_' && name[1] != '\x00') {
                name = name[(int)1..];
            }

            nint v = 0;
            if (machsym.type_ & N_EXT == 0) {
                v = localSymVersion;
            }

            s = l.LookupOrCreateCgoExport(name, v);
            if (machsym.type_ & N_EXT == 0) {
                l.SetAttrDuplicateOK(s, true);
            }

            if (machsym.desc & (N_WEAK_REF | N_WEAK_DEF) != 0) {
                l.SetAttrDuplicateOK(s, true);
            }

            machsym.sym = s;
            if (machsym.sectnum == 0) { // undefined
                continue;

            }

            if (uint32(machsym.sectnum) > c.seg.nsect) {
                return errorf("reference to invalid section %d", machsym.sectnum);
            }

            sect = _addr_c.seg.sect[machsym.sectnum - 1];
            bld = l.MakeSymbolUpdater(s);
            var outer = sect.sym;
            if (outer == 0) {
                continue; // ignore reference to invalid section
            }

            {
                var osym = l.OuterSym(s);

                if (osym != 0) {
                    if (l.AttrDuplicateOK(s)) {
                        continue;
                    }
                    return errorf("duplicate symbol reference: %s in both %s and %s", l.SymName(s), l.SymName(osym), l.SymName(sect.sym));
                }

            }


            bld.SetType(l.SymType(outer));
            if (l.SymSize(outer) != 0) { // skip empty section (0-sized symbol)
                l.AddInteriorSym(outer, s);

            }

            bld.SetValue(int64(machsym.value - sect.addr));
            if (!l.AttrCgoExportDynamic(s)) {
                bld.SetDynimplib(""); // satisfy dynimport
            }

            if (l.SymType(outer) == sym.STEXT) {
                if (bld.External() && !bld.DuplicateOK()) {
                    return errorf("%v: duplicate symbol definition", s);
                }
                bld.SetExternal(true);
            }

        }

        i = i__prev1;
    } 

    // Sort outer lists by address, adding to textp.
    // This keeps textp in increasing address order.
    {
        var i__prev1 = i;

        for (i = 0; uint32(i) < c.seg.nsect; i++) {
            sect = _addr_c.seg.sect[i];
            s = sect.sym;
            if (s == 0) {
                continue;
            }
            bld = l.MakeSymbolUpdater(s);
            if (bld.SubSym() != 0) {
                bld.SortSub(); 

                // assign sizes, now that we know symbols in sorted order.
                {
                    var s1__prev2 = s1;

                    var s1 = bld.Sub();

                    while (s1 != 0) {
                        var s1Bld = l.MakeSymbolUpdater(s1);
                        {
                            var sub = l.SubSym(s1);

                            if (sub != 0) {
                                s1Bld.SetSize(l.SymValue(sub) - l.SymValue(s1));
                        s1 = l.SubSym(s1);
                            }
                            else
 {
                                var dlen = int64(len(l.Data(s)));
                                s1Bld.SetSize(l.SymValue(s) + dlen - l.SymValue(s1));
                            }

                        }

                    }


                    s1 = s1__prev2;
                }

            }

            if (bld.Type() == sym.STEXT) {
                if (bld.OnList()) {
                    return errorf("symbol %s listed multiple times", bld.Name());
                }
                bld.SetOnList(true);
                textp = append(textp, s);
                {
                    var s1__prev2 = s1;

                    s1 = bld.Sub();

                    while (s1 != 0) {
                        if (l.AttrOnList(s1)) {
                            return errorf("symbol %s listed multiple times", l.RawSymName(s1));
                        s1 = l.SubSym(s1);
                        }

                        l.SetAttrOnList(s1, true);
                        textp = append(textp, s1);

                    }


                    s1 = s1__prev2;
                }

            }

        }

        i = i__prev1;
    } 

    // load relocations
    {
        var i__prev1 = i;

        for (i = 0; uint32(i) < c.seg.nsect; i++) {
            sect = _addr_c.seg.sect[i];
            s = sect.sym;
            if (s == 0) {
                continue;
            }
            macholoadrel(m, _addr_sect);
            if (sect.rel == null) {
                continue;
            }
            var sb = l.MakeSymbolUpdater(sect.sym);
            long rAdd = default;
            for (var j = uint32(0); j < sect.nreloc; j++) {
                int rOff = default;                byte rSize = default;                objabi.RelocType rType = default;                loader.Sym rSym = default;
                var rel = _addr_sect.rel[j];
                if (rel.scattered != 0) { 
                    // mach-o only uses scattered relocation on 32-bit platforms,
                    // which are no longer supported.
                    return errorf("%v: unexpected scattered relocation", s);

                }

                if (arch.Family == sys.ARM64 && rel.type_ == MACHO_ARM64_RELOC_ADDEND) { 
                    // Two relocations. This addend will be applied to the next one.
                    rAdd = int64(rel.symnum) << 40 >> 40; // convert unsigned 24-bit to signed 24-bit
                    continue;

                }

                rSize = rel.length;
                rType = objabi.MachoRelocOffset + (objabi.RelocType(rel.type_) << 1) + objabi.RelocType(rel.pcrel);
                rOff = int32(rel.addr); 

                // Handle X86_64_RELOC_SIGNED referencing a section (rel.extrn == 0).
                var p = l.Data(s);
                if (arch.Family == sys.AMD64) {
                    if (rel.extrn == 0 && rel.type_ == MACHO_X86_64_RELOC_SIGNED) { 
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
                        var secaddr = c.seg.sect[rel.symnum - 1].addr;
                        rAdd = int64(uint64(int64(int32(e.Uint32(p[(int)rOff..]))) + int64(rOff) + 4) - secaddr);

                    }
                    else
 {
                        rAdd = int64(int32(e.Uint32(p[(int)rOff..])));
                    }

                } 

                // An unsigned internal relocation has a value offset
                // by the section address.
                if (arch.Family == sys.AMD64 && rel.extrn == 0 && rel.type_ == MACHO_X86_64_RELOC_UNSIGNED) {
                    secaddr = c.seg.sect[rel.symnum - 1].addr;
                    rAdd -= int64(secaddr);
                }

                if (rel.extrn == 0) {
                    if (rel.symnum < 1 || rel.symnum > c.seg.nsect) {
                        return errorf("invalid relocation: section reference out of range %d vs %d", rel.symnum, c.seg.nsect);
                    }
                    rSym = c.seg.sect[rel.symnum - 1].sym;
                    if (rSym == 0) {
                        return errorf("invalid relocation: %s", c.seg.sect[rel.symnum - 1].name);
                    }
                }
                else
 {
                    if (rel.symnum >= symtab.nsym) {
                        return errorf("invalid relocation: symbol reference out of range");
                    }
                    rSym = symtab.sym[rel.symnum].sym;
                }

                var (r, _) = sb.AddRel(rType);
                r.SetOff(rOff);
                r.SetSiz(rSize);
                r.SetSym(rSym);
                r.SetAdd(rAdd);

                rAdd = 0; // clear rAdd for next iteration
            }


            sb.SortRelocs();

        }

        i = i__prev1;
    }

    return (textp, error.As(null!)!);

}

private static @string cstring(slice<byte> x) {
    var i = bytes.IndexByte(x, '\x00');
    if (i >= 0) {
        x = x[..(int)i];
    }
    return string(x);

}

} // end loadmacho_package
