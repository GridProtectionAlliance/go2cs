// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package loadelf implements an ELF file reader.
// package loadelf -- go2cs converted at 2020 October 09 05:52:03 UTC
// import "cmd/oldlink/internal/loadelf" ==> using loadelf = go.cmd.oldlink.@internal.loadelf_package
// Original source: C:\Go\src\cmd\oldlink\internal\loadelf\ldelf.go
using bytes = go.bytes_package;
using bio = go.cmd.@internal.bio_package;
using objabi = go.cmd.@internal.objabi_package;
using sys = go.cmd.@internal.sys_package;
using loader = go.cmd.oldlink.@internal.loader_package;
using sym = go.cmd.oldlink.@internal.sym_package;
using elf = go.debug.elf_package;
using binary = go.encoding.binary_package;
using fmt = go.fmt_package;
using io = go.io_package;
using log = go.log_package;
using sort = go.sort_package;
using strings = go.strings_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace oldlink {
namespace @internal
{
    public static partial class loadelf_package
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
        public static readonly long ElfClassNone = (long)0L;
        public static readonly long ElfClass32 = (long)1L;
        public static readonly long ElfClass64 = (long)2L;


        public static readonly long ElfDataNone = (long)0L;
        public static readonly long ElfDataLsb = (long)1L;
        public static readonly long ElfDataMsb = (long)2L;


        public static readonly long ElfTypeNone = (long)0L;
        public static readonly long ElfTypeRelocatable = (long)1L;
        public static readonly long ElfTypeExecutable = (long)2L;
        public static readonly long ElfTypeSharedObject = (long)3L;
        public static readonly long ElfTypeCore = (long)4L;


        public static readonly long ElfMachNone = (long)0L;
        public static readonly long ElfMach32100 = (long)1L;
        public static readonly long ElfMachSparc = (long)2L;
        public static readonly long ElfMach386 = (long)3L;
        public static readonly long ElfMach68000 = (long)4L;
        public static readonly long ElfMach88000 = (long)5L;
        public static readonly long ElfMach486 = (long)6L;
        public static readonly long ElfMach860 = (long)7L;
        public static readonly long ElfMachMips = (long)8L;
        public static readonly long ElfMachS370 = (long)9L;
        public static readonly long ElfMachMipsLe = (long)10L;
        public static readonly long ElfMachParisc = (long)15L;
        public static readonly long ElfMachVpp500 = (long)17L;
        public static readonly long ElfMachSparc32Plus = (long)18L;
        public static readonly long ElfMach960 = (long)19L;
        public static readonly long ElfMachPower = (long)20L;
        public static readonly long ElfMachPower64 = (long)21L;
        public static readonly long ElfMachS390 = (long)22L;
        public static readonly long ElfMachV800 = (long)36L;
        public static readonly long ElfMachFr20 = (long)37L;
        public static readonly long ElfMachRh32 = (long)38L;
        public static readonly long ElfMachRce = (long)39L;
        public static readonly long ElfMachArm = (long)40L;
        public static readonly long ElfMachAlpha = (long)41L;
        public static readonly long ElfMachSH = (long)42L;
        public static readonly long ElfMachSparc9 = (long)43L;
        public static readonly long ElfMachAmd64 = (long)62L;
        public static readonly long ElfMachArm64 = (long)183L;


        public static readonly long ElfAbiNone = (long)0L;
        public static readonly long ElfAbiSystemV = (long)0L;
        public static readonly long ElfAbiHPUX = (long)1L;
        public static readonly long ElfAbiNetBSD = (long)2L;
        public static readonly long ElfAbiLinux = (long)3L;
        public static readonly long ElfAbiSolaris = (long)6L;
        public static readonly long ElfAbiAix = (long)7L;
        public static readonly long ElfAbiIrix = (long)8L;
        public static readonly long ElfAbiFreeBSD = (long)9L;
        public static readonly long ElfAbiTru64 = (long)10L;
        public static readonly long ElfAbiModesto = (long)11L;
        public static readonly long ElfAbiOpenBSD = (long)12L;
        public static readonly long ElfAbiARM = (long)97L;
        public static readonly long ElfAbiEmbedded = (long)255L;


        public static readonly long ElfSectNone = (long)0L;
        public static readonly long ElfSectProgbits = (long)1L;
        public static readonly long ElfSectSymtab = (long)2L;
        public static readonly long ElfSectStrtab = (long)3L;
        public static readonly long ElfSectRela = (long)4L;
        public static readonly long ElfSectHash = (long)5L;
        public static readonly long ElfSectDynamic = (long)6L;
        public static readonly long ElfSectNote = (long)7L;
        public static readonly long ElfSectNobits = (long)8L;
        public static readonly long ElfSectRel = (long)9L;
        public static readonly long ElfSectShlib = (long)10L;
        public static readonly long ElfSectDynsym = (long)11L;
        public static readonly ulong ElfSectFlagWrite = (ulong)0x1UL;
        public static readonly ulong ElfSectFlagAlloc = (ulong)0x2UL;
        public static readonly ulong ElfSectFlagExec = (ulong)0x4UL;


        public static readonly long ElfSymBindLocal = (long)0L;
        public static readonly long ElfSymBindGlobal = (long)1L;
        public static readonly long ElfSymBindWeak = (long)2L;


        public static readonly long ElfSymTypeNone = (long)0L;
        public static readonly long ElfSymTypeObject = (long)1L;
        public static readonly long ElfSymTypeFunc = (long)2L;
        public static readonly long ElfSymTypeSection = (long)3L;
        public static readonly long ElfSymTypeFile = (long)4L;
        public static readonly long ElfSymTypeCommon = (long)5L;
        public static readonly long ElfSymTypeTLS = (long)6L;


        public static readonly long ElfSymShnNone = (long)0L;
        public static readonly ulong ElfSymShnAbs = (ulong)0xFFF1UL;
        public static readonly ulong ElfSymShnCommon = (ulong)0xFFF2UL;


        public static readonly long ElfProgNone = (long)0L;
        public static readonly long ElfProgLoad = (long)1L;
        public static readonly long ElfProgDynamic = (long)2L;
        public static readonly long ElfProgInterp = (long)3L;
        public static readonly long ElfProgNote = (long)4L;
        public static readonly long ElfProgShlib = (long)5L;
        public static readonly long ElfProgPhdr = (long)6L;
        public static readonly ulong ElfProgFlagExec = (ulong)0x1UL;
        public static readonly ulong ElfProgFlagWrite = (ulong)0x2UL;
        public static readonly ulong ElfProgFlagRead = (ulong)0x4UL;


        public static readonly long ElfNotePrStatus = (long)1L;
        public static readonly long ElfNotePrFpreg = (long)2L;
        public static readonly long ElfNotePrPsinfo = (long)3L;
        public static readonly long ElfNotePrTaskstruct = (long)4L;
        public static readonly long ElfNotePrAuxv = (long)6L;
        public static readonly ulong ElfNotePrXfpreg = (ulong)0x46e62b7fUL;


        // TODO(crawshaw): de-duplicate with cmd/oldlink/internal/ld/elf.go.
        public static readonly long ELF64SYMSIZE = (long)24L;
        public static readonly long ELF32SYMSIZE = (long)16L;

        public static readonly ulong SHT_ARM_ATTRIBUTES = (ulong)0x70000003UL;


        public partial struct ElfHdrBytes
        {
            public array<byte> Ident;
            public array<byte> Type;
            public array<byte> Machine;
            public array<byte> Version;
            public array<byte> Entry;
            public array<byte> Phoff;
            public array<byte> Shoff;
            public array<byte> Flags;
            public array<byte> Ehsize;
            public array<byte> Phentsize;
            public array<byte> Phnum;
            public array<byte> Shentsize;
            public array<byte> Shnum;
            public array<byte> Shstrndx;
        }

        public partial struct ElfSectBytes
        {
            public array<byte> Name;
            public array<byte> Type;
            public array<byte> Flags;
            public array<byte> Addr;
            public array<byte> Off;
            public array<byte> Size;
            public array<byte> Link;
            public array<byte> Info;
            public array<byte> Align;
            public array<byte> Entsize;
        }

        public partial struct ElfProgBytes
        {
        }

        public partial struct ElfSymBytes
        {
            public array<byte> Name;
            public array<byte> Value;
            public array<byte> Size;
            public byte Info;
            public byte Other;
            public array<byte> Shndx;
        }

        public partial struct ElfHdrBytes64
        {
            public array<byte> Ident;
            public array<byte> Type;
            public array<byte> Machine;
            public array<byte> Version;
            public array<byte> Entry;
            public array<byte> Phoff;
            public array<byte> Shoff;
            public array<byte> Flags;
            public array<byte> Ehsize;
            public array<byte> Phentsize;
            public array<byte> Phnum;
            public array<byte> Shentsize;
            public array<byte> Shnum;
            public array<byte> Shstrndx;
        }

        public partial struct ElfSectBytes64
        {
            public array<byte> Name;
            public array<byte> Type;
            public array<byte> Flags;
            public array<byte> Addr;
            public array<byte> Off;
            public array<byte> Size;
            public array<byte> Link;
            public array<byte> Info;
            public array<byte> Align;
            public array<byte> Entsize;
        }

        public partial struct ElfProgBytes64
        {
        }

        public partial struct ElfSymBytes64
        {
            public array<byte> Name;
            public byte Info;
            public byte Other;
            public array<byte> Shndx;
            public array<byte> Value;
            public array<byte> Size;
        }

        public partial struct ElfSect
        {
            public @string name;
            public uint nameoff;
            public uint type_;
            public ulong flags;
            public ulong addr;
            public ulong off;
            public ulong size;
            public uint link;
            public uint info;
            public ulong align;
            public ulong entsize;
            public slice<byte> @base;
            public ptr<sym.Symbol> sym;
        }

        public partial struct ElfObj
        {
            public ptr<bio.Reader> f;
            public long @base; // offset in f where ELF begins
            public long length; // length of ELF
            public long is64;
            public @string name;
            public binary.ByteOrder e;
            public slice<ElfSect> sect;
            public ulong nsect;
            public long nsymtab;
            public ptr<ElfSect> symtab;
            public ptr<ElfSect> symstr;
            public uint type_;
            public uint machine;
            public uint version;
            public ulong entry;
            public ulong phoff;
            public ulong shoff;
            public uint flags;
            public uint ehsize;
            public uint phentsize;
            public uint phnum;
            public uint shentsize;
            public uint shnum;
            public uint shstrndx;
        }

        public partial struct ElfSym
        {
            public @string name;
            public ulong value;
            public ulong size;
            public byte bind;
            public byte type_;
            public byte other;
            public ushort shndx;
            public ptr<sym.Symbol> sym;
        }

        public static array<byte> ElfMagic = new array<byte>(new byte[] { 0x7F, 'E', 'L', 'F' });

        public static readonly long TagFile = (long)1L;
        public static readonly long TagCPUName = (long)4L;
        public static readonly long TagCPURawName = (long)5L;
        public static readonly long TagCompatibility = (long)32L;
        public static readonly long TagNoDefaults = (long)64L;
        public static readonly long TagAlsoCompatibleWith = (long)65L;
        public static readonly long TagABIVFPArgs = (long)28L;


        private partial struct elfAttribute
        {
            public ulong tag;
            public @string sval;
            public ulong ival;
        }

        private partial struct elfAttributeList
        {
            public slice<byte> data;
            public error err;
        }

        private static @string @string(this ptr<elfAttributeList> _addr_a)
        {
            ref elfAttributeList a = ref _addr_a.val;

            if (a.err != null)
            {
                return "";
            }

            var nul = bytes.IndexByte(a.data, 0L);
            if (nul < 0L)
            {
                a.err = io.EOF;
                return "";
            }

            var s = string(a.data[..nul]);
            a.data = a.data[nul + 1L..];
            return s;

        }

        private static ulong uleb128(this ptr<elfAttributeList> _addr_a)
        {
            ref elfAttributeList a = ref _addr_a.val;

            if (a.err != null)
            {
                return 0L;
            }

            var (v, size) = binary.Uvarint(a.data);
            a.data = a.data[size..];
            return v;

        }

        // Read an elfAttribute from the list following the rules used on ARM systems.
        private static elfAttribute armAttr(this ptr<elfAttributeList> _addr_a)
        {
            ref elfAttributeList a = ref _addr_a.val;

            elfAttribute attr = new elfAttribute(tag:a.uleb128());

            if (attr.tag == TagCompatibility) 
                attr.ival = a.uleb128();
                attr.sval = a.@string();
            else if (attr.tag == 64L)             else if (attr.tag == 65L) // Tag_also_compatible_with
                // Not really, but we don't actually care about this tag.
                attr.sval = a.@string(); 

                // Tag with string argument
            else if (attr.tag == TagCPUName || attr.tag == TagCPURawName || (attr.tag >= 32L && attr.tag & 1L != 0L)) 
                attr.sval = a.@string();
            else // Tag with integer argument
                attr.ival = a.uleb128();
                        return attr;

        }

        private static bool done(this ptr<elfAttributeList> _addr_a)
        {
            ref elfAttributeList a = ref _addr_a.val;

            if (a.err != null || len(a.data) == 0L)
            {
                return true;
            }

            return false;

        }

        // Look for the attribute that indicates the object uses the hard-float ABI (a
        // file-level attribute with tag Tag_VFP_arch and value 1). Unfortunately the
        // format used means that we have to parse all of the file-level attributes to
        // find the one we are looking for. This format is slightly documented in "ELF
        // for the ARM Architecture" but mostly this is derived from reading the source
        // to gold and readelf.
        private static (bool, uint, error) parseArmAttributes(binary.ByteOrder e, slice<byte> data)
        {
            bool found = default;
            uint ehdrFlags = default;
            error err = default!;

            found = false;
            if (data[0L] != 'A')
            {
                return (false, 0L, error.As(fmt.Errorf(".ARM.attributes has unexpected format %c\n", data[0L]))!);
            }

            data = data[1L..];
            while (len(data) != 0L)
            {
                var sectionlength = e.Uint32(data);
                var sectiondata = data[4L..sectionlength];
                data = data[sectionlength..];

                var nulIndex = bytes.IndexByte(sectiondata, 0L);
                if (nulIndex < 0L)
                {
                    return (false, 0L, error.As(fmt.Errorf("corrupt .ARM.attributes (section name not NUL-terminated)\n"))!);
                }

                var name = string(sectiondata[..nulIndex]);
                sectiondata = sectiondata[nulIndex + 1L..];

                if (name != "aeabi")
                {
                    continue;
                }

                while (len(sectiondata) != 0L)
                {
                    var (subsectiontag, sz) = binary.Uvarint(sectiondata);
                    var subsectionsize = e.Uint32(sectiondata[sz..]);
                    var subsectiondata = sectiondata[sz + 4L..subsectionsize];
                    sectiondata = sectiondata[subsectionsize..];

                    if (subsectiontag != TagFile)
                    {
                        continue;
                    }

                    elfAttributeList attrList = new elfAttributeList(data:subsectiondata);
                    while (!attrList.done())
                    {
                        var attr = attrList.armAttr();
                        if (attr.tag == TagABIVFPArgs && attr.ival == 1L)
                        {
                            found = true;
                            ehdrFlags = 0x5000402UL; // has entry point, Version5 EABI, hard-float ABI
                        }

                    }

                    if (attrList.err != null)
                    {
                        return (false, 0L, error.As(fmt.Errorf("could not parse .ARM.attributes\n"))!);
                    }

                }


            }

            return (found, ehdrFlags, error.As(null!)!);

        }

        public static (slice<ptr<sym.Symbol>>, uint, error) Load(ptr<loader.Loader> _addr_l, ptr<sys.Arch> _addr_arch, ptr<sym.Symbols> _addr_syms, ptr<bio.Reader> _addr_f, @string pkg, long length, @string pn, uint flags)
        {
            slice<ptr<sym.Symbol>> _p0 = default;
            uint _p0 = default;
            error _p0 = default!;
            ref loader.Loader l = ref _addr_l.val;
            ref sys.Arch arch = ref _addr_arch.val;
            ref sym.Symbols syms = ref _addr_syms.val;
            ref bio.Reader f = ref _addr_f.val;

            Func<@string, long, ptr<sym.Symbol>> newSym = (name, version) =>
            {
                return l.Create(name, syms);
            }
;
            Func<@string, long, ptr<sym.Symbol>> lookup = (name, version) =>
            {
                return l.LookupOrCreate(name, version, syms);
            }
;
            return load(_addr_arch, syms.IncVersion(), newSym, lookup, _addr_f, pkg, length, pn, flags);

        }

        public static (slice<ptr<sym.Symbol>>, uint, error) LoadOld(ptr<sys.Arch> _addr_arch, ptr<sym.Symbols> _addr_syms, ptr<bio.Reader> _addr_f, @string pkg, long length, @string pn, uint flags)
        {
            slice<ptr<sym.Symbol>> _p0 = default;
            uint _p0 = default;
            error _p0 = default!;
            ref sys.Arch arch = ref _addr_arch.val;
            ref sym.Symbols syms = ref _addr_syms.val;
            ref bio.Reader f = ref _addr_f.val;

            return load(_addr_arch, syms.IncVersion(), syms.Newsym, syms.Lookup, _addr_f, pkg, length, pn, flags);
        }

        public delegate  ptr<sym.Symbol> lookupFunc(@string,  long);

        // load loads the ELF file pn from f.
        // Symbols are written into syms, and a slice of the text symbols is returned.
        //
        // On ARM systems, Load will attempt to determine what ELF header flags to
        // emit by scanning the attributes in the ELF file being loaded. The
        // parameter initEhdrFlags contains the current header flags for the output
        // object, and the returned ehdrFlags contains what this Load function computes.
        // TODO: find a better place for this logic.
        private static (slice<ptr<sym.Symbol>>, uint, error) load(ptr<sys.Arch> _addr_arch, long localSymVersion, lookupFunc newSym, lookupFunc lookup, ptr<bio.Reader> _addr_f, @string pkg, long length, @string pn, uint initEhdrFlags)
        {
            slice<ptr<sym.Symbol>> textp = default;
            uint ehdrFlags = default;
            error err = default!;
            ref sys.Arch arch = ref _addr_arch.val;
            ref bio.Reader f = ref _addr_f.val;

            Func<@string, object[], (slice<ptr<sym.Symbol>>, uint, error)> errorf = (str, args) =>
            {
                return (null, 0L, error.As(fmt.Errorf("loadelf: %s: %v", pn, fmt.Sprintf(str, args)))!);
            }
;

            var @base = f.Offset();

            array<byte> hdrbuf = new array<byte>(64L);
            {
                var err__prev1 = err;

                var (_, err) = io.ReadFull(f, hdrbuf[..]);

                if (err != null)
                {
                    return errorf("malformed elf file: %v", err);
                }

                err = err__prev1;

            }

            ptr<ElfHdrBytes> hdr = @new<ElfHdrBytes>();
            binary.Read(bytes.NewReader(hdrbuf[..]), binary.BigEndian, hdr); // only byte arrays; byte order doesn't matter
            if (string(hdr.Ident[..4L]) != "\x7FELF")
            {
                return errorf("malformed elf file, bad header");
            }

            binary.ByteOrder e = default;

            if (hdr.Ident[5L] == ElfDataLsb) 
                e = binary.LittleEndian;
            else if (hdr.Ident[5L] == ElfDataMsb) 
                e = binary.BigEndian;
            else 
                return errorf("malformed elf file, unknown header");
            // read header
            ptr<ElfObj> elfobj = @new<ElfObj>();

            elfobj.e = e;
            elfobj.f = f;
            elfobj.@base = base;
            elfobj.length = length;
            elfobj.name = pn;

            long is64 = 0L;
            if (hdr.Ident[4L] == ElfClass64)
            {
                is64 = 1L;
                hdr = @new<ElfHdrBytes64>();
                binary.Read(bytes.NewReader(hdrbuf[..]), binary.BigEndian, hdr); // only byte arrays; byte order doesn't matter
                elfobj.type_ = uint32(e.Uint16(hdr.Type[..]));
                elfobj.machine = uint32(e.Uint16(hdr.Machine[..]));
                elfobj.version = e.Uint32(hdr.Version[..]);
                elfobj.phoff = e.Uint64(hdr.Phoff[..]);
                elfobj.shoff = e.Uint64(hdr.Shoff[..]);
                elfobj.flags = e.Uint32(hdr.Flags[..]);
                elfobj.ehsize = uint32(e.Uint16(hdr.Ehsize[..]));
                elfobj.phentsize = uint32(e.Uint16(hdr.Phentsize[..]));
                elfobj.phnum = uint32(e.Uint16(hdr.Phnum[..]));
                elfobj.shentsize = uint32(e.Uint16(hdr.Shentsize[..]));
                elfobj.shnum = uint32(e.Uint16(hdr.Shnum[..]));
                elfobj.shstrndx = uint32(e.Uint16(hdr.Shstrndx[..]));

            }
            else
            {
                elfobj.type_ = uint32(e.Uint16(hdr.Type[..]));
                elfobj.machine = uint32(e.Uint16(hdr.Machine[..]));
                elfobj.version = e.Uint32(hdr.Version[..]);
                elfobj.entry = uint64(e.Uint32(hdr.Entry[..]));
                elfobj.phoff = uint64(e.Uint32(hdr.Phoff[..]));
                elfobj.shoff = uint64(e.Uint32(hdr.Shoff[..]));
                elfobj.flags = e.Uint32(hdr.Flags[..]);
                elfobj.ehsize = uint32(e.Uint16(hdr.Ehsize[..]));
                elfobj.phentsize = uint32(e.Uint16(hdr.Phentsize[..]));
                elfobj.phnum = uint32(e.Uint16(hdr.Phnum[..]));
                elfobj.shentsize = uint32(e.Uint16(hdr.Shentsize[..]));
                elfobj.shnum = uint32(e.Uint16(hdr.Shnum[..]));
                elfobj.shstrndx = uint32(e.Uint16(hdr.Shstrndx[..]));
            }

            elfobj.is64 = is64;

            {
                var v = uint32(hdr.Ident[6L]);

                if (v != elfobj.version)
                {
                    return errorf("malformed elf version: got %d, want %d", v, elfobj.version);
                }

            }


            if (e.Uint16(hdr.Type[..]) != ElfTypeRelocatable)
            {
                return errorf("elf but not elf relocatable object");
            }


            if (arch.Family == sys.MIPS) 
                if (elfobj.machine != ElfMachMips || hdr.Ident[4L] != ElfClass32)
                {
                    return errorf("elf object but not mips");
                }

            else if (arch.Family == sys.MIPS64) 
                if (elfobj.machine != ElfMachMips || hdr.Ident[4L] != ElfClass64)
                {
                    return errorf("elf object but not mips64");
                }

            else if (arch.Family == sys.ARM) 
                if (e != binary.LittleEndian || elfobj.machine != ElfMachArm || hdr.Ident[4L] != ElfClass32)
                {
                    return errorf("elf object but not arm");
                }

            else if (arch.Family == sys.AMD64) 
                if (e != binary.LittleEndian || elfobj.machine != ElfMachAmd64 || hdr.Ident[4L] != ElfClass64)
                {
                    return errorf("elf object but not amd64");
                }

            else if (arch.Family == sys.ARM64) 
                if (e != binary.LittleEndian || elfobj.machine != ElfMachArm64 || hdr.Ident[4L] != ElfClass64)
                {
                    return errorf("elf object but not arm64");
                }

            else if (arch.Family == sys.I386) 
                if (e != binary.LittleEndian || elfobj.machine != ElfMach386 || hdr.Ident[4L] != ElfClass32)
                {
                    return errorf("elf object but not 386");
                }

            else if (arch.Family == sys.PPC64) 
                if (elfobj.machine != ElfMachPower64 || hdr.Ident[4L] != ElfClass64)
                {
                    return errorf("elf object but not ppc64");
                }

            else if (arch.Family == sys.S390X) 
                if (elfobj.machine != ElfMachS390 || hdr.Ident[4L] != ElfClass64)
                {
                    return errorf("elf object but not s390x");
                }

            else 
                return errorf("elf %s unimplemented", arch.Name);
            // load section list into memory.
            elfobj.sect = make_slice<ElfSect>(elfobj.shnum);

            elfobj.nsect = uint(elfobj.shnum);
            {
                long i__prev1 = i;

                for (long i = 0L; uint(i) < elfobj.nsect; i++)
                {
                    f.MustSeek(int64(uint64(base) + elfobj.shoff + uint64(int64(i) * int64(elfobj.shentsize))), 0L);
                    var sect = _addr_elfobj.sect[i];
                    if (is64 != 0L)
                    {
                        ref ElfSectBytes64 b = ref heap(out ptr<ElfSectBytes64> _addr_b);

                        {
                            var err__prev2 = err;

                            var err = binary.Read(f, e, _addr_b);

                            if (err != null)
                            {
                                return errorf("malformed elf file: %v", err);
                            }

                            err = err__prev2;

                        }


                        sect.nameoff = e.Uint32(b.Name[..]);
                        sect.type_ = e.Uint32(b.Type[..]);
                        sect.flags = e.Uint64(b.Flags[..]);
                        sect.addr = e.Uint64(b.Addr[..]);
                        sect.off = e.Uint64(b.Off[..]);
                        sect.size = e.Uint64(b.Size[..]);
                        sect.link = e.Uint32(b.Link[..]);
                        sect.info = e.Uint32(b.Info[..]);
                        sect.align = e.Uint64(b.Align[..]);
                        sect.entsize = e.Uint64(b.Entsize[..]);

                    }
                    else
                    {
                        b = default;

                        {
                            var err__prev2 = err;

                            err = binary.Read(f, e, _addr_b);

                            if (err != null)
                            {
                                return errorf("malformed elf file: %v", err);
                            }

                            err = err__prev2;

                        }


                        sect.nameoff = e.Uint32(b.Name[..]);
                        sect.type_ = e.Uint32(b.Type[..]);
                        sect.flags = uint64(e.Uint32(b.Flags[..]));
                        sect.addr = uint64(e.Uint32(b.Addr[..]));
                        sect.off = uint64(e.Uint32(b.Off[..]));
                        sect.size = uint64(e.Uint32(b.Size[..]));
                        sect.link = e.Uint32(b.Link[..]);
                        sect.info = e.Uint32(b.Info[..]);
                        sect.align = uint64(e.Uint32(b.Align[..]));
                        sect.entsize = uint64(e.Uint32(b.Entsize[..]));

                    }

                } 

                // read section string table and translate names


                i = i__prev1;
            } 

            // read section string table and translate names
            if (elfobj.shstrndx >= uint32(elfobj.nsect))
            {
                return errorf("malformed elf file: shstrndx out of range %d >= %d", elfobj.shstrndx, elfobj.nsect);
            }

            sect = _addr_elfobj.sect[elfobj.shstrndx];
            {
                var err__prev1 = err;

                err = elfmap(elfobj, _addr_sect);

                if (err != null)
                {
                    return errorf("malformed elf file: %v", err);
                }

                err = err__prev1;

            }

            {
                long i__prev1 = i;

                for (i = 0L; uint(i) < elfobj.nsect; i++)
                {
                    if (elfobj.sect[i].nameoff != 0L)
                    {
                        elfobj.sect[i].name = cstring(sect.@base[elfobj.sect[i].nameoff..]);
                    }

                } 

                // load string table for symbols into memory.


                i = i__prev1;
            } 

            // load string table for symbols into memory.
            elfobj.symtab = section(elfobj, ".symtab");

            if (elfobj.symtab == null)
            { 
                // our work is done here - no symbols means nothing can refer to this file
                return ;

            }

            if (elfobj.symtab.link <= 0L || elfobj.symtab.link >= uint32(elfobj.nsect))
            {
                return errorf("elf object has symbol table with invalid string table link");
            }

            elfobj.symstr = _addr_elfobj.sect[elfobj.symtab.link];
            if (is64 != 0L)
            {
                elfobj.nsymtab = int(elfobj.symtab.size / ELF64SYMSIZE);
            }
            else
            {
                elfobj.nsymtab = int(elfobj.symtab.size / ELF32SYMSIZE);
            }

            {
                var err__prev1 = err;

                err = elfmap(elfobj, _addr_elfobj.symtab);

                if (err != null)
                {
                    return errorf("malformed elf file: %v", err);
                }

                err = err__prev1;

            }

            {
                var err__prev1 = err;

                err = elfmap(elfobj, _addr_elfobj.symstr);

                if (err != null)
                {
                    return errorf("malformed elf file: %v", err);
                } 

                // load text and data segments into memory.
                // they are not as small as the section lists, but we'll need
                // the memory anyway for the symbol images, so we might
                // as well use one large chunk.

                // create symbols for elfmapped sections

                err = err__prev1;

            } 

            // load text and data segments into memory.
            // they are not as small as the section lists, but we'll need
            // the memory anyway for the symbol images, so we might
            // as well use one large chunk.

            // create symbols for elfmapped sections
            var sectsymNames = make_map<@string, bool>();
            long counter = 0L;
            {
                long i__prev1 = i;

                for (i = 0L; uint(i) < elfobj.nsect; i++)
                {
                    sect = _addr_elfobj.sect[i];
                    if (sect.type_ == SHT_ARM_ATTRIBUTES && sect.name == ".ARM.attributes")
                    {
                        {
                            var err__prev2 = err;

                            err = elfmap(elfobj, _addr_sect);

                            if (err != null)
                            {
                                return errorf("%s: malformed elf file: %v", pn, err);
                            } 
                            // We assume the soft-float ABI unless we see a tag indicating otherwise.

                            err = err__prev2;

                        } 
                        // We assume the soft-float ABI unless we see a tag indicating otherwise.
                        if (initEhdrFlags == 0x5000002UL)
                        {
                            ehdrFlags = 0x5000202UL;
                        }
                        else
                        {
                            ehdrFlags = initEhdrFlags;
                        }

                        var (found, newEhdrFlags, err) = parseArmAttributes(e, sect.@base[..sect.size]);
                        if (err != null)
                        { 
                            // TODO(dfc) should this return an error?
                            log.Printf("%s: %v", pn, err);

                        }

                        if (found)
                        {
                            ehdrFlags = newEhdrFlags;
                        }

                    }

                    if ((sect.type_ != ElfSectProgbits && sect.type_ != ElfSectNobits) || sect.flags & ElfSectFlagAlloc == 0L)
                    {
                        continue;
                    }

                    if (sect.type_ != ElfSectNobits)
                    {
                        {
                            var err__prev2 = err;

                            err = elfmap(elfobj, _addr_sect);

                            if (err != null)
                            {
                                return errorf("%s: malformed elf file: %v", pn, err);
                            }

                            err = err__prev2;

                        }

                    }

                    var name = fmt.Sprintf("%s(%s)", pkg, sect.name);
                    while (sectsymNames[name])
                    {
                        counter++;
                        name = fmt.Sprintf("%s(%s%d)", pkg, sect.name, counter);
                    }

                    sectsymNames[name] = true;

                    var s = lookup(name, localSymVersion);


                    if (int(sect.flags) & (ElfSectFlagAlloc | ElfSectFlagWrite | ElfSectFlagExec) == ElfSectFlagAlloc) 
                        s.Type = sym.SRODATA;
                    else if (int(sect.flags) & (ElfSectFlagAlloc | ElfSectFlagWrite | ElfSectFlagExec) == ElfSectFlagAlloc + ElfSectFlagWrite) 
                        if (sect.type_ == ElfSectNobits)
                        {
                            s.Type = sym.SNOPTRBSS;
                        }
                        else
                        {
                            s.Type = sym.SNOPTRDATA;
                        }

                    else if (int(sect.flags) & (ElfSectFlagAlloc | ElfSectFlagWrite | ElfSectFlagExec) == ElfSectFlagAlloc + ElfSectFlagExec) 
                        s.Type = sym.STEXT;
                    else 
                        return errorf("%s: unexpected flags for ELF section %s", pn, sect.name);
                                        if (sect.name == ".got" || sect.name == ".toc")
                    {
                        s.Type = sym.SELFGOT;
                    }

                    if (sect.type_ == ElfSectProgbits)
                    {
                        s.P = sect.@base;
                        s.P = s.P[..sect.size];
                    }

                    s.Size = int64(sect.size);
                    s.Align = int32(sect.align);
                    sect.sym = s;

                } 

                // enter sub-symbols into symbol table.
                // symbol 0 is the null symbol.


                i = i__prev1;
            } 

            // enter sub-symbols into symbol table.
            // symbol 0 is the null symbol.
            var symbols = make_slice<ptr<sym.Symbol>>(elfobj.nsymtab);

            {
                long i__prev1 = i;

                for (i = 1L; i < elfobj.nsymtab; i++)
                {
                    ref ElfSym elfsym = ref heap(out ptr<ElfSym> _addr_elfsym);
                    {
                        var err__prev1 = err;

                        err = readelfsym(newSym, lookup, _addr_arch, elfobj, i, _addr_elfsym, 1L, localSymVersion);

                        if (err != null)
                        {
                            return errorf("%s: malformed elf file: %v", pn, err);
                        }

                        err = err__prev1;

                    }

                    symbols[i] = elfsym.sym;
                    if (elfsym.type_ != ElfSymTypeFunc && elfsym.type_ != ElfSymTypeObject && elfsym.type_ != ElfSymTypeNone && elfsym.type_ != ElfSymTypeCommon)
                    {
                        continue;
                    }

                    if (elfsym.shndx == ElfSymShnCommon || elfsym.type_ == ElfSymTypeCommon)
                    {
                        s = elfsym.sym;
                        if (uint64(s.Size) < elfsym.size)
                        {
                            s.Size = int64(elfsym.size);
                        }

                        if (s.Type == 0L || s.Type == sym.SXREF)
                        {
                            s.Type = sym.SNOPTRBSS;
                        }

                        continue;

                    }

                    if (uint(elfsym.shndx) >= elfobj.nsect || elfsym.shndx == 0L)
                    {
                        continue;
                    } 

                    // even when we pass needSym == 1 to readelfsym, it might still return nil to skip some unwanted symbols
                    if (elfsym.sym == null)
                    {
                        continue;
                    }

                    sect = _addr_elfobj.sect[elfsym.shndx];
                    if (sect.sym == null)
                    {
                        if (strings.HasPrefix(elfsym.name, ".Linfo_string"))
                        { // clang does this
                            continue;

                        }

                        if (elfsym.name == "" && elfsym.type_ == 0L && sect.name == ".debug_str")
                        { 
                            // This reportedly happens with clang 3.7 on ARM.
                            // See issue 13139.
                            continue;

                        }

                        if (strings.HasPrefix(elfsym.name, "$d") && elfsym.type_ == 0L && sect.name == ".debug_frame")
                        { 
                            // "$d" is a marker, not a real symbol.
                            // This happens with gcc on ARM64.
                            // See https://sourceware.org/bugzilla/show_bug.cgi?id=21809
                            continue;

                        }

                        if (strings.HasPrefix(elfsym.name, ".LASF"))
                        { // gcc on s390x does this
                            continue;

                        }

                        return errorf("%v: sym#%d: ignoring symbol in section %d (type %d)", elfsym.sym, i, elfsym.shndx, elfsym.type_);

                    }

                    s = elfsym.sym;
                    if (s.Outer != null)
                    {
                        if (s.Attr.DuplicateOK())
                        {
                            continue;
                        }

                        return errorf("duplicate symbol reference: %s in both %s and %s", s.Name, s.Outer.Name, sect.sym.Name);

                    }

                    s.Sub = sect.sym.Sub;
                    sect.sym.Sub = s;
                    s.Type = sect.sym.Type;
                    s.Attr |= sym.AttrSubSymbol;
                    if (!s.Attr.CgoExportDynamic())
                    {
                        s.SetDynimplib(""); // satisfy dynimport
                    }

                    s.Value = int64(elfsym.value);
                    s.Size = int64(elfsym.size);
                    s.Outer = sect.sym;
                    if (sect.sym.Type == sym.STEXT)
                    {
                        if (s.Attr.External() && !s.Attr.DuplicateOK())
                        {
                            return errorf("%v: duplicate symbol definition", s);
                        }

                        s.Attr |= sym.AttrExternal;

                    }

                    if (elfobj.machine == ElfMachPower64)
                    {
                        var flag = int(elfsym.other) >> (int)(5L);
                        if (2L <= flag && flag <= 6L)
                        {
                            s.SetLocalentry(1L << (int)(uint(flag - 2L)));
                        }
                        else if (flag == 7L)
                        {
                            return errorf("%v: invalid sym.other 0x%x", s, elfsym.other);
                        }

                    }

                } 

                // Sort outer lists by address, adding to textp.
                // This keeps textp in increasing address order.


                i = i__prev1;
            } 

            // Sort outer lists by address, adding to textp.
            // This keeps textp in increasing address order.
            {
                long i__prev1 = i;

                for (i = uint(0L); i < elfobj.nsect; i++)
                {
                    s = elfobj.sect[i].sym;
                    if (s == null)
                    {
                        continue;
                    }

                    if (s.Sub != null)
                    {
                        s.Sub = sym.SortSub(s.Sub);
                    }

                    if (s.Type == sym.STEXT)
                    {
                        if (s.Attr.OnList())
                        {
                            return errorf("symbol %s listed multiple times", s.Name);
                        }

                        s.Attr |= sym.AttrOnList;
                        textp = append(textp, s);
                        s = s.Sub;

                        while (s != null)
                        {
                            if (s.Attr.OnList())
                            {
                                return errorf("symbol %s listed multiple times", s.Name);
                            s = s.Sub;
                            }

                            s.Attr |= sym.AttrOnList;
                            textp = append(textp, s);

                        }


                    }

                } 

                // load relocations


                i = i__prev1;
            } 

            // load relocations
            {
                long i__prev1 = i;

                for (i = uint(0L); i < elfobj.nsect; i++)
                {
                    var rsect = _addr_elfobj.sect[i];
                    if (rsect.type_ != ElfSectRela && rsect.type_ != ElfSectRel)
                    {
                        continue;
                    }

                    if (rsect.info >= uint32(elfobj.nsect) || elfobj.sect[rsect.info].@base == null)
                    {
                        continue;
                    }

                    sect = _addr_elfobj.sect[rsect.info];
                    {
                        var err__prev1 = err;

                        err = elfmap(elfobj, _addr_rsect);

                        if (err != null)
                        {
                            return errorf("malformed elf file: %v", err);
                        }

                        err = err__prev1;

                    }

                    long rela = 0L;
                    if (rsect.type_ == ElfSectRela)
                    {
                        rela = 1L;
                    }

                    var n = int(rsect.size / uint64(4L + 4L * is64) / uint64(2L + rela));
                    var r = make_slice<sym.Reloc>(n);
                    var p = rsect.@base;
                    for (long j = 0L; j < n; j++)
                    {
                        ulong add = default;
                        long symIdx = default;
                        ulong relocType = default;

                        var rp = _addr_r[j];
                        if (is64 != 0L)
                        { 
                            // 64-bit rel/rela
                            rp.Off = int32(e.Uint64(p));

                            p = p[8L..];

                            if (arch.Family == sys.MIPS64) 
                                // https://www.linux-mips.org/pub/linux/mips/doc/ABI/elf64-2.4.pdf
                                // The doc shows it's different with general Linux ELF
                                symIdx = int(e.Uint32(p));
                                relocType = uint64(p[7L]);
                            else 
                                var info = e.Uint64(p);
                                relocType = info & 0xffffffffUL;
                                symIdx = int(info >> (int)(32L));
                                                        p = p[8L..];
                            if (rela != 0L)
                            {
                                add = e.Uint64(p);
                                p = p[8L..];
                            }

                        }
                        else
                        { 
                            // 32-bit rel/rela
                            rp.Off = int32(e.Uint32(p));

                            p = p[4L..];
                            info = e.Uint32(p);
                            relocType = uint64(info & 0xffUL);
                            symIdx = int(info >> (int)(8L));
                            p = p[4L..];
                            if (rela != 0L)
                            {
                                add = uint64(e.Uint32(p));
                                p = p[4L..];
                            }

                        }

                        if (relocType == 0L)
                        { // skip R_*_NONE relocation
                            j--;
                            n--;
                            continue;

                        }

                        if (symIdx == 0L)
                        { // absolute relocation, don't bother reading the null symbol
                            rp.Sym = null;

                        }
                        else
                        {
                            elfsym = default;
                            {
                                var err__prev2 = err;

                                err = readelfsym(newSym, lookup, _addr_arch, elfobj, symIdx, _addr_elfsym, 0L, 0L);

                                if (err != null)
                                {
                                    return errorf("malformed elf file: %v", err);
                                }

                                err = err__prev2;

                            }

                            elfsym.sym = symbols[symIdx];
                            if (elfsym.sym == null)
                            {
                                return errorf("malformed elf file: %s#%d: reloc of invalid sym #%d %s shndx=%d type=%d", sect.sym.Name, j, symIdx, elfsym.name, elfsym.shndx, elfsym.type_);
                            }

                            rp.Sym = elfsym.sym;

                        }

                        rp.Type = objabi.ElfRelocOffset + objabi.RelocType(relocType);
                        rp.Siz, err = relSize(_addr_arch, pn, uint32(relocType));
                        if (err != null)
                        {
                            return (null, 0L, error.As(err)!);
                        }

                        if (rela != 0L)
                        {
                            rp.Add = int64(add);
                        }
                        else
                        { 
                            // load addend from image
                            if (rp.Siz == 4L)
                            {
                                rp.Add = int64(e.Uint32(sect.@base[rp.Off..]));
                            }
                            else if (rp.Siz == 8L)
                            {
                                rp.Add = int64(e.Uint64(sect.@base[rp.Off..]));
                            }
                            else
                            {
                                return errorf("invalid rela size %d", rp.Siz);
                            }

                        }

                        if (rp.Siz == 2L)
                        {
                            rp.Add = int64(int16(rp.Add));
                        }

                        if (rp.Siz == 4L)
                        {
                            rp.Add = int64(int32(rp.Add));
                        }

                    } 

                    //print("rel %s %d %d %s %#llx\n", sect->sym->name, rp->type, rp->siz, rp->sym->name, rp->add);
 

                    //print("rel %s %d %d %s %#llx\n", sect->sym->name, rp->type, rp->siz, rp->sym->name, rp->add);
                    sort.Sort(sym.RelocByOff(r[..n])); 
                    // just in case

                    s = sect.sym;
                    s.R = r;
                    s.R = s.R[..n];

                }


                i = i__prev1;
            }

            return (textp, ehdrFlags, error.As(null!)!);

        }

        private static ptr<ElfSect> section(ptr<ElfObj> _addr_elfobj, @string name)
        {
            ref ElfObj elfobj = ref _addr_elfobj.val;

            for (long i = 0L; uint(i) < elfobj.nsect; i++)
            {
                if (elfobj.sect[i].name != "" && name != "" && elfobj.sect[i].name == name)
                {
                    return _addr__addr_elfobj.sect[i]!;
                }

            }

            return _addr_null!;

        }

        private static error elfmap(ptr<ElfObj> _addr_elfobj, ptr<ElfSect> _addr_sect)
        {
            error err = default!;
            ref ElfObj elfobj = ref _addr_elfobj.val;
            ref ElfSect sect = ref _addr_sect.val;

            if (sect.@base != null)
            {
                return error.As(null!)!;
            }

            if (sect.off + sect.size > uint64(elfobj.length))
            {
                err = fmt.Errorf("elf section past end of file");
                return error.As(err)!;
            }

            sect.@base = make_slice<byte>(sect.size);
            elfobj.f.MustSeek(int64(uint64(elfobj.@base) + sect.off), 0L);
            {
                var (_, err) = io.ReadFull(elfobj.f, sect.@base);

                if (err != null)
                {
                    return error.As(fmt.Errorf("short read: %v", err))!;
                }

            }


            return error.As(null!)!;

        }

        private static error readelfsym(lookupFunc newSym, lookupFunc lookup, ptr<sys.Arch> _addr_arch, ptr<ElfObj> _addr_elfobj, long i, ptr<ElfSym> _addr_elfsym, long needSym, long localSymVersion)
        {
            error err = default!;
            ref sys.Arch arch = ref _addr_arch.val;
            ref ElfObj elfobj = ref _addr_elfobj.val;
            ref ElfSym elfsym = ref _addr_elfsym.val;

            if (i >= elfobj.nsymtab || i < 0L)
            {
                err = fmt.Errorf("invalid elf symbol index");
                return error.As(err)!;
            }

            if (i == 0L)
            {
                return error.As(fmt.Errorf("readym: read null symbol!"))!;
            }

            if (elfobj.is64 != 0L)
            {
                ptr<ElfSymBytes64> b = @new<ElfSymBytes64>();
                binary.Read(bytes.NewReader(elfobj.symtab.@base[i * ELF64SYMSIZE..(i + 1L) * ELF64SYMSIZE]), elfobj.e, b);
                elfsym.name = cstring(elfobj.symstr.@base[elfobj.e.Uint32(b.Name[..])..]);
                elfsym.value = elfobj.e.Uint64(b.Value[..]);
                elfsym.size = elfobj.e.Uint64(b.Size[..]);
                elfsym.shndx = elfobj.e.Uint16(b.Shndx[..]);
                elfsym.bind = b.Info >> (int)(4L);
                elfsym.type_ = b.Info & 0xfUL;
                elfsym.other = b.Other;
            }
            else
            {
                b = @new<ElfSymBytes>();
                binary.Read(bytes.NewReader(elfobj.symtab.@base[i * ELF32SYMSIZE..(i + 1L) * ELF32SYMSIZE]), elfobj.e, b);
                elfsym.name = cstring(elfobj.symstr.@base[elfobj.e.Uint32(b.Name[..])..]);
                elfsym.value = uint64(elfobj.e.Uint32(b.Value[..]));
                elfsym.size = uint64(elfobj.e.Uint32(b.Size[..]));
                elfsym.shndx = elfobj.e.Uint16(b.Shndx[..]);
                elfsym.bind = b.Info >> (int)(4L);
                elfsym.type_ = b.Info & 0xfUL;
                elfsym.other = b.Other;
            }

            ptr<sym.Symbol> s;
            if (elfsym.name == "_GLOBAL_OFFSET_TABLE_")
            {
                elfsym.name = ".got";
            }

            if (elfsym.name == ".TOC.")
            { 
                // Magic symbol on ppc64.  Will be set to this object
                // file's .got+0x8000.
                elfsym.bind = ElfSymBindLocal;

            }


            if (elfsym.type_ == ElfSymTypeSection) 
                s = elfobj.sect[elfsym.shndx].sym;
            else if (elfsym.type_ == ElfSymTypeObject || elfsym.type_ == ElfSymTypeFunc || elfsym.type_ == ElfSymTypeNone || elfsym.type_ == ElfSymTypeCommon) 

                if (elfsym.bind == ElfSymBindGlobal) 
                    if (needSym != 0L)
                    {
                        s = lookup(elfsym.name, 0L); 

                        // for global scoped hidden symbols we should insert it into
                        // symbol hash table, but mark them as hidden.
                        // __i686.get_pc_thunk.bx is allowed to be duplicated, to
                        // workaround that we set dupok.
                        // TODO(minux): correctly handle __i686.get_pc_thunk.bx without
                        // set dupok generally. See https://golang.org/cl/5823055
                        // comment #5 for details.
                        if (s != null && elfsym.other == 2L)
                        {
                            s.Attr |= sym.AttrDuplicateOK | sym.AttrVisibilityHidden;
                        }

                    }

                else if (elfsym.bind == ElfSymBindLocal) 
                    if ((arch.Family == sys.ARM || arch.Family == sys.ARM64) && (strings.HasPrefix(elfsym.name, "$a") || strings.HasPrefix(elfsym.name, "$d") || strings.HasPrefix(elfsym.name, "$x")))
                    { 
                        // binutils for arm and arm64 generate these mapping
                        // symbols, ignore these
                        break;

                    }

                    if (elfsym.name == ".TOC.")
                    { 
                        // We need to be able to look this up,
                        // so put it in the hash table.
                        if (needSym != 0L)
                        {
                            s = lookup(elfsym.name, localSymVersion);
                            s.Attr |= sym.AttrVisibilityHidden;
                        }

                        break;

                    }

                    if (needSym != 0L)
                    { 
                        // local names and hidden global names are unique
                        // and should only be referenced by their index, not name, so we
                        // don't bother to add them into the hash table
                        // FIXME: pass empty string here for name? This would
                        // reduce mem use, but also (possibly) make it harder
                        // to debug problems.
                        s = newSym(elfsym.name, localSymVersion);

                        s.Attr |= sym.AttrVisibilityHidden;

                    }

                else if (elfsym.bind == ElfSymBindWeak) 
                    if (needSym != 0L)
                    {
                        s = lookup(elfsym.name, 0L);
                        if (elfsym.other == 2L)
                        {
                            s.Attr |= sym.AttrVisibilityHidden;
                        } 

                        // Allow weak symbols to be duplicated when already defined.
                        if (s.Outer != null)
                        {
                            s.Attr |= sym.AttrDuplicateOK;
                        }

                    }

                else 
                    err = fmt.Errorf("%s: invalid symbol binding %d", elfsym.name, elfsym.bind);
                    return error.As(err)!;
                            // TODO(mwhudson): the test of VisibilityHidden here probably doesn't make
            // sense and should be removed when someone has thought about it properly.
            if (s != null && s.Type == 0L && !s.Attr.VisibilityHidden() && elfsym.type_ != ElfSymTypeSection)
            {
                s.Type = sym.SXREF;
            }

            elfsym.sym = s;

            return error.As(null!)!;

        }

        private static (byte, error) relSize(ptr<sys.Arch> _addr_arch, @string pn, uint elftype)
        {
            byte _p0 = default;
            error _p0 = default!;
            ref sys.Arch arch = ref _addr_arch.val;
 
            // TODO(mdempsky): Replace this with a struct-valued switch statement
            // once golang.org/issue/15164 is fixed or found to not impair cmd/link
            // performance.

            const var AMD64 = uint32(sys.AMD64);
            const var ARM = uint32(sys.ARM);
            const var ARM64 = uint32(sys.ARM64);
            const var I386 = uint32(sys.I386);
            const var PPC64 = uint32(sys.PPC64);
            const var S390X = uint32(sys.S390X);
            const var MIPS = uint32(sys.MIPS);
            const var MIPS64 = uint32(sys.MIPS64);



            if (uint32(arch.Family) | elftype << (int)(16L) == MIPS | uint32(elf.R_MIPS_HI16) << (int)(16L) || uint32(arch.Family) | elftype << (int)(16L) == MIPS | uint32(elf.R_MIPS_LO16) << (int)(16L) || uint32(arch.Family) | elftype << (int)(16L) == MIPS | uint32(elf.R_MIPS_GOT16) << (int)(16L) || uint32(arch.Family) | elftype << (int)(16L) == MIPS | uint32(elf.R_MIPS_GPREL16) << (int)(16L) || uint32(arch.Family) | elftype << (int)(16L) == MIPS | uint32(elf.R_MIPS_GOT_PAGE) << (int)(16L) || uint32(arch.Family) | elftype << (int)(16L) == MIPS | uint32(elf.R_MIPS_JALR) << (int)(16L) || uint32(arch.Family) | elftype << (int)(16L) == MIPS | uint32(elf.R_MIPS_GOT_OFST) << (int)(16L) || uint32(arch.Family) | elftype << (int)(16L) == MIPS64 | uint32(elf.R_MIPS_HI16) << (int)(16L) || uint32(arch.Family) | elftype << (int)(16L) == MIPS64 | uint32(elf.R_MIPS_LO16) << (int)(16L) || uint32(arch.Family) | elftype << (int)(16L) == MIPS64 | uint32(elf.R_MIPS_GOT16) << (int)(16L) || uint32(arch.Family) | elftype << (int)(16L) == MIPS64 | uint32(elf.R_MIPS_GPREL16) << (int)(16L) || uint32(arch.Family) | elftype << (int)(16L) == MIPS64 | uint32(elf.R_MIPS_GOT_PAGE) << (int)(16L) || uint32(arch.Family) | elftype << (int)(16L) == MIPS64 | uint32(elf.R_MIPS_JALR) << (int)(16L) || uint32(arch.Family) | elftype << (int)(16L) == MIPS64 | uint32(elf.R_MIPS_GOT_OFST) << (int)(16L)) 
                return (4L, error.As(null!)!);
            else if (uint32(arch.Family) | elftype << (int)(16L) == S390X | uint32(elf.R_390_8) << (int)(16L)) 
                return (1L, error.As(null!)!);
            else if (uint32(arch.Family) | elftype << (int)(16L) == PPC64 | uint32(elf.R_PPC64_TOC16) << (int)(16L) || uint32(arch.Family) | elftype << (int)(16L) == PPC64 | uint32(elf.R_PPC64_TOC16_LO) << (int)(16L) || uint32(arch.Family) | elftype << (int)(16L) == PPC64 | uint32(elf.R_PPC64_TOC16_HI) << (int)(16L) || uint32(arch.Family) | elftype << (int)(16L) == PPC64 | uint32(elf.R_PPC64_TOC16_HA) << (int)(16L) || uint32(arch.Family) | elftype << (int)(16L) == PPC64 | uint32(elf.R_PPC64_TOC16_DS) << (int)(16L) || uint32(arch.Family) | elftype << (int)(16L) == PPC64 | uint32(elf.R_PPC64_TOC16_LO_DS) << (int)(16L) || uint32(arch.Family) | elftype << (int)(16L) == PPC64 | uint32(elf.R_PPC64_REL16_LO) << (int)(16L) || uint32(arch.Family) | elftype << (int)(16L) == PPC64 | uint32(elf.R_PPC64_REL16_HI) << (int)(16L) || uint32(arch.Family) | elftype << (int)(16L) == PPC64 | uint32(elf.R_PPC64_REL16_HA) << (int)(16L) || uint32(arch.Family) | elftype << (int)(16L) == S390X | uint32(elf.R_390_16) << (int)(16L) || uint32(arch.Family) | elftype << (int)(16L) == S390X | uint32(elf.R_390_GOT16) << (int)(16L) || uint32(arch.Family) | elftype << (int)(16L) == S390X | uint32(elf.R_390_PC16) << (int)(16L) || uint32(arch.Family) | elftype << (int)(16L) == S390X | uint32(elf.R_390_PC16DBL) << (int)(16L) || uint32(arch.Family) | elftype << (int)(16L) == S390X | uint32(elf.R_390_PLT16DBL) << (int)(16L)) 
                return (2L, error.As(null!)!);
            else if (uint32(arch.Family) | elftype << (int)(16L) == ARM | uint32(elf.R_ARM_ABS32) << (int)(16L) || uint32(arch.Family) | elftype << (int)(16L) == ARM | uint32(elf.R_ARM_GOT32) << (int)(16L) || uint32(arch.Family) | elftype << (int)(16L) == ARM | uint32(elf.R_ARM_PLT32) << (int)(16L) || uint32(arch.Family) | elftype << (int)(16L) == ARM | uint32(elf.R_ARM_GOTOFF) << (int)(16L) || uint32(arch.Family) | elftype << (int)(16L) == ARM | uint32(elf.R_ARM_GOTPC) << (int)(16L) || uint32(arch.Family) | elftype << (int)(16L) == ARM | uint32(elf.R_ARM_THM_PC22) << (int)(16L) || uint32(arch.Family) | elftype << (int)(16L) == ARM | uint32(elf.R_ARM_REL32) << (int)(16L) || uint32(arch.Family) | elftype << (int)(16L) == ARM | uint32(elf.R_ARM_CALL) << (int)(16L) || uint32(arch.Family) | elftype << (int)(16L) == ARM | uint32(elf.R_ARM_V4BX) << (int)(16L) || uint32(arch.Family) | elftype << (int)(16L) == ARM | uint32(elf.R_ARM_GOT_PREL) << (int)(16L) || uint32(arch.Family) | elftype << (int)(16L) == ARM | uint32(elf.R_ARM_PC24) << (int)(16L) || uint32(arch.Family) | elftype << (int)(16L) == ARM | uint32(elf.R_ARM_JUMP24) << (int)(16L) || uint32(arch.Family) | elftype << (int)(16L) == ARM64 | uint32(elf.R_AARCH64_CALL26) << (int)(16L) || uint32(arch.Family) | elftype << (int)(16L) == ARM64 | uint32(elf.R_AARCH64_ADR_GOT_PAGE) << (int)(16L) || uint32(arch.Family) | elftype << (int)(16L) == ARM64 | uint32(elf.R_AARCH64_LD64_GOT_LO12_NC) << (int)(16L) || uint32(arch.Family) | elftype << (int)(16L) == ARM64 | uint32(elf.R_AARCH64_ADR_PREL_PG_HI21) << (int)(16L) || uint32(arch.Family) | elftype << (int)(16L) == ARM64 | uint32(elf.R_AARCH64_ADD_ABS_LO12_NC) << (int)(16L) || uint32(arch.Family) | elftype << (int)(16L) == ARM64 | uint32(elf.R_AARCH64_LDST8_ABS_LO12_NC) << (int)(16L) || uint32(arch.Family) | elftype << (int)(16L) == ARM64 | uint32(elf.R_AARCH64_LDST32_ABS_LO12_NC) << (int)(16L) || uint32(arch.Family) | elftype << (int)(16L) == ARM64 | uint32(elf.R_AARCH64_LDST64_ABS_LO12_NC) << (int)(16L) || uint32(arch.Family) | elftype << (int)(16L) == ARM64 | uint32(elf.R_AARCH64_LDST128_ABS_LO12_NC) << (int)(16L) || uint32(arch.Family) | elftype << (int)(16L) == ARM64 | uint32(elf.R_AARCH64_PREL32) << (int)(16L) || uint32(arch.Family) | elftype << (int)(16L) == ARM64 | uint32(elf.R_AARCH64_JUMP26) << (int)(16L) || uint32(arch.Family) | elftype << (int)(16L) == AMD64 | uint32(elf.R_X86_64_PC32) << (int)(16L) || uint32(arch.Family) | elftype << (int)(16L) == AMD64 | uint32(elf.R_X86_64_PLT32) << (int)(16L) || uint32(arch.Family) | elftype << (int)(16L) == AMD64 | uint32(elf.R_X86_64_GOTPCREL) << (int)(16L) || uint32(arch.Family) | elftype << (int)(16L) == AMD64 | uint32(elf.R_X86_64_GOTPCRELX) << (int)(16L) || uint32(arch.Family) | elftype << (int)(16L) == AMD64 | uint32(elf.R_X86_64_REX_GOTPCRELX) << (int)(16L) || uint32(arch.Family) | elftype << (int)(16L) == I386 | uint32(elf.R_386_32) << (int)(16L) || uint32(arch.Family) | elftype << (int)(16L) == I386 | uint32(elf.R_386_PC32) << (int)(16L) || uint32(arch.Family) | elftype << (int)(16L) == I386 | uint32(elf.R_386_GOT32) << (int)(16L) || uint32(arch.Family) | elftype << (int)(16L) == I386 | uint32(elf.R_386_PLT32) << (int)(16L) || uint32(arch.Family) | elftype << (int)(16L) == I386 | uint32(elf.R_386_GOTOFF) << (int)(16L) || uint32(arch.Family) | elftype << (int)(16L) == I386 | uint32(elf.R_386_GOTPC) << (int)(16L) || uint32(arch.Family) | elftype << (int)(16L) == I386 | uint32(elf.R_386_GOT32X) << (int)(16L) || uint32(arch.Family) | elftype << (int)(16L) == PPC64 | uint32(elf.R_PPC64_REL24) << (int)(16L) || uint32(arch.Family) | elftype << (int)(16L) == PPC64 | uint32(elf.R_PPC_REL32) << (int)(16L) || uint32(arch.Family) | elftype << (int)(16L) == S390X | uint32(elf.R_390_32) << (int)(16L) || uint32(arch.Family) | elftype << (int)(16L) == S390X | uint32(elf.R_390_PC32) << (int)(16L) || uint32(arch.Family) | elftype << (int)(16L) == S390X | uint32(elf.R_390_GOT32) << (int)(16L) || uint32(arch.Family) | elftype << (int)(16L) == S390X | uint32(elf.R_390_PLT32) << (int)(16L) || uint32(arch.Family) | elftype << (int)(16L) == S390X | uint32(elf.R_390_PC32DBL) << (int)(16L) || uint32(arch.Family) | elftype << (int)(16L) == S390X | uint32(elf.R_390_PLT32DBL) << (int)(16L) || uint32(arch.Family) | elftype << (int)(16L) == S390X | uint32(elf.R_390_GOTPCDBL) << (int)(16L) || uint32(arch.Family) | elftype << (int)(16L) == S390X | uint32(elf.R_390_GOTENT) << (int)(16L)) 
                return (4L, error.As(null!)!);
            else if (uint32(arch.Family) | elftype << (int)(16L) == AMD64 | uint32(elf.R_X86_64_64) << (int)(16L) || uint32(arch.Family) | elftype << (int)(16L) == AMD64 | uint32(elf.R_X86_64_PC64) << (int)(16L) || uint32(arch.Family) | elftype << (int)(16L) == ARM64 | uint32(elf.R_AARCH64_ABS64) << (int)(16L) || uint32(arch.Family) | elftype << (int)(16L) == ARM64 | uint32(elf.R_AARCH64_PREL64) << (int)(16L) || uint32(arch.Family) | elftype << (int)(16L) == PPC64 | uint32(elf.R_PPC64_ADDR64) << (int)(16L) || uint32(arch.Family) | elftype << (int)(16L) == S390X | uint32(elf.R_390_GLOB_DAT) << (int)(16L) || uint32(arch.Family) | elftype << (int)(16L) == S390X | uint32(elf.R_390_RELATIVE) << (int)(16L) || uint32(arch.Family) | elftype << (int)(16L) == S390X | uint32(elf.R_390_GOTOFF) << (int)(16L) || uint32(arch.Family) | elftype << (int)(16L) == S390X | uint32(elf.R_390_GOTPC) << (int)(16L) || uint32(arch.Family) | elftype << (int)(16L) == S390X | uint32(elf.R_390_64) << (int)(16L) || uint32(arch.Family) | elftype << (int)(16L) == S390X | uint32(elf.R_390_PC64) << (int)(16L) || uint32(arch.Family) | elftype << (int)(16L) == S390X | uint32(elf.R_390_GOT64) << (int)(16L) || uint32(arch.Family) | elftype << (int)(16L) == S390X | uint32(elf.R_390_PLT64) << (int)(16L)) 
                return (8L, error.As(null!)!);
            else 
                return (0L, error.As(fmt.Errorf("%s: unknown relocation type %d; compiled without -fpic?", pn, elftype))!);
            
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
