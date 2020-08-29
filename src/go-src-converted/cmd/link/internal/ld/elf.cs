// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ld -- go2cs converted at 2020 August 29 10:03:41 UTC
// import "cmd/link/internal/ld" ==> using ld = go.cmd.link.@internal.ld_package
// Original source: C:\Go\src\cmd\link\internal\ld\elf.go
using objabi = go.cmd.@internal.objabi_package;
using sys = go.cmd.@internal.sys_package;
using sym = go.cmd.link.@internal.sym_package;
using sha1 = go.crypto.sha1_package;
using binary = go.encoding.binary_package;
using hex = go.encoding.hex_package;
using io = go.io_package;
using filepath = go.path.filepath_package;
using sort = go.sort_package;
using strings = go.strings_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace link {
namespace @internal
{
    public static partial class ld_package
    {
        /*
         * Derived from:
         * $FreeBSD: src/sys/sys/elf32.h,v 1.8.14.1 2005/12/30 22:13:58 marcel Exp $
         * $FreeBSD: src/sys/sys/elf64.h,v 1.10.14.1 2005/12/30 22:13:58 marcel Exp $
         * $FreeBSD: src/sys/sys/elf_common.h,v 1.15.8.1 2005/12/30 22:13:58 marcel Exp $
         * $FreeBSD: src/sys/alpha/include/elf.h,v 1.14 2003/09/25 01:10:22 peter Exp $
         * $FreeBSD: src/sys/amd64/include/elf.h,v 1.18 2004/08/03 08:21:48 dfr Exp $
         * $FreeBSD: src/sys/arm/include/elf.h,v 1.5.2.1 2006/06/30 21:42:52 cognet Exp $
         * $FreeBSD: src/sys/i386/include/elf.h,v 1.16 2004/08/02 19:12:17 dfr Exp $
         * $FreeBSD: src/sys/powerpc/include/elf.h,v 1.7 2004/11/02 09:47:01 ssouhlal Exp $
         * $FreeBSD: src/sys/sparc64/include/elf.h,v 1.12 2003/09/25 01:10:26 peter Exp $
         *
         * Copyright (c) 1996-1998 John D. Polstra.  All rights reserved.
         * Copyright (c) 2001 David E. O'Brien
         * Portions Copyright 2009 The Go Authors. All rights reserved.
         *
         * Redistribution and use in source and binary forms, with or without
         * modification, are permitted provided that the following conditions
         * are met:
         * 1. Redistributions of source code must retain the above copyright
         *    notice, this list of conditions and the following disclaimer.
         * 2. Redistributions in binary form must reproduce the above copyright
         *    notice, this list of conditions and the following disclaimer in the
         *    documentation and/or other materials provided with the distribution.
         *
         * THIS SOFTWARE IS PROVIDED BY THE AUTHOR AND CONTRIBUTORS ``AS IS'' AND
         * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
         * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
         * ARE DISCLAIMED.  IN NO EVENT SHALL THE AUTHOR OR CONTRIBUTORS BE LIABLE
         * FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
         * DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS
         * OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
         * HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT
         * LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY
         * OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF
         * SUCH DAMAGE.
         *
         */

        /*
         * ELF definitions that are independent of architecture or word size.
         */

        /*
         * Note header.  The ".note" section contains an array of notes.  Each
         * begins with this header, aligned to a word boundary.  Immediately
         * following the note header is n_namesz bytes of name, padded to the
         * next word boundary.  Then comes n_descsz bytes of descriptor, again
         * padded to a word boundary.  The values of n_namesz and n_descsz do
         * not include the padding.
         */
        private partial struct elfNote
        {
            public uint nNamesz;
            public uint nDescsz;
            public uint nType;
        }

        public static readonly long EI_MAG0 = 0L;
        public static readonly long EI_MAG1 = 1L;
        public static readonly long EI_MAG2 = 2L;
        public static readonly long EI_MAG3 = 3L;
        public static readonly long EI_CLASS = 4L;
        public static readonly long EI_DATA = 5L;
        public static readonly long EI_VERSION = 6L;
        public static readonly long EI_OSABI = 7L;
        public static readonly long EI_ABIVERSION = 8L;
        public static readonly long OLD_EI_BRAND = 8L;
        public static readonly long EI_PAD = 9L;
        public static readonly long EI_NIDENT = 16L;
        public static readonly ulong ELFMAG0 = 0x7fUL;
        public static readonly char ELFMAG1 = 'E';
        public static readonly char ELFMAG2 = 'L';
        public static readonly char ELFMAG3 = 'F';
        public static readonly long SELFMAG = 4L;
        public static readonly long EV_NONE = 0L;
        public static readonly long EV_CURRENT = 1L;
        public static readonly long ELFCLASSNONE = 0L;
        public static readonly long ELFCLASS32 = 1L;
        public static readonly long ELFCLASS64 = 2L;
        public static readonly long ELFDATANONE = 0L;
        public static readonly long ELFDATA2LSB = 1L;
        public static readonly long ELFDATA2MSB = 2L;
        public static readonly long ELFOSABI_NONE = 0L;
        public static readonly long ELFOSABI_HPUX = 1L;
        public static readonly long ELFOSABI_NETBSD = 2L;
        public static readonly long ELFOSABI_LINUX = 3L;
        public static readonly long ELFOSABI_HURD = 4L;
        public static readonly long ELFOSABI_86OPEN = 5L;
        public static readonly long ELFOSABI_SOLARIS = 6L;
        public static readonly long ELFOSABI_AIX = 7L;
        public static readonly long ELFOSABI_IRIX = 8L;
        public static readonly long ELFOSABI_FREEBSD = 9L;
        public static readonly long ELFOSABI_TRU64 = 10L;
        public static readonly long ELFOSABI_MODESTO = 11L;
        public static readonly long ELFOSABI_OPENBSD = 12L;
        public static readonly long ELFOSABI_OPENVMS = 13L;
        public static readonly long ELFOSABI_NSK = 14L;
        public static readonly long ELFOSABI_ARM = 97L;
        public static readonly long ELFOSABI_STANDALONE = 255L;
        public static readonly var ELFOSABI_SYSV = ELFOSABI_NONE;
        public static readonly var ELFOSABI_MONTEREY = ELFOSABI_AIX;
        public static readonly long ET_NONE = 0L;
        public static readonly long ET_REL = 1L;
        public static readonly long ET_EXEC = 2L;
        public static readonly long ET_DYN = 3L;
        public static readonly long ET_CORE = 4L;
        public static readonly ulong ET_LOOS = 0xfe00UL;
        public static readonly ulong ET_HIOS = 0xfeffUL;
        public static readonly ulong ET_LOPROC = 0xff00UL;
        public static readonly ulong ET_HIPROC = 0xffffUL;
        public static readonly long EM_NONE = 0L;
        public static readonly long EM_M32 = 1L;
        public static readonly long EM_SPARC = 2L;
        public static readonly long EM_386 = 3L;
        public static readonly long EM_68K = 4L;
        public static readonly long EM_88K = 5L;
        public static readonly long EM_860 = 7L;
        public static readonly long EM_MIPS = 8L;
        public static readonly long EM_S370 = 9L;
        public static readonly long EM_MIPS_RS3_LE = 10L;
        public static readonly long EM_PARISC = 15L;
        public static readonly long EM_VPP500 = 17L;
        public static readonly long EM_SPARC32PLUS = 18L;
        public static readonly long EM_960 = 19L;
        public static readonly long EM_PPC = 20L;
        public static readonly long EM_PPC64 = 21L;
        public static readonly long EM_S390 = 22L;
        public static readonly long EM_V800 = 36L;
        public static readonly long EM_FR20 = 37L;
        public static readonly long EM_RH32 = 38L;
        public static readonly long EM_RCE = 39L;
        public static readonly long EM_ARM = 40L;
        public static readonly long EM_SH = 42L;
        public static readonly long EM_SPARCV9 = 43L;
        public static readonly long EM_TRICORE = 44L;
        public static readonly long EM_ARC = 45L;
        public static readonly long EM_H8_300 = 46L;
        public static readonly long EM_H8_300H = 47L;
        public static readonly long EM_H8S = 48L;
        public static readonly long EM_H8_500 = 49L;
        public static readonly long EM_IA_64 = 50L;
        public static readonly long EM_MIPS_X = 51L;
        public static readonly long EM_COLDFIRE = 52L;
        public static readonly long EM_68HC12 = 53L;
        public static readonly long EM_MMA = 54L;
        public static readonly long EM_PCP = 55L;
        public static readonly long EM_NCPU = 56L;
        public static readonly long EM_NDR1 = 57L;
        public static readonly long EM_STARCORE = 58L;
        public static readonly long EM_ME16 = 59L;
        public static readonly long EM_ST100 = 60L;
        public static readonly long EM_TINYJ = 61L;
        public static readonly long EM_X86_64 = 62L;
        public static readonly long EM_AARCH64 = 183L;
        public static readonly long EM_486 = 6L;
        public static readonly long EM_MIPS_RS4_BE = 10L;
        public static readonly long EM_ALPHA_STD = 41L;
        public static readonly ulong EM_ALPHA = 0x9026UL;
        public static readonly long SHN_UNDEF = 0L;
        public static readonly ulong SHN_LORESERVE = 0xff00UL;
        public static readonly ulong SHN_LOPROC = 0xff00UL;
        public static readonly ulong SHN_HIPROC = 0xff1fUL;
        public static readonly ulong SHN_LOOS = 0xff20UL;
        public static readonly ulong SHN_HIOS = 0xff3fUL;
        public static readonly ulong SHN_ABS = 0xfff1UL;
        public static readonly ulong SHN_COMMON = 0xfff2UL;
        public static readonly ulong SHN_XINDEX = 0xffffUL;
        public static readonly ulong SHN_HIRESERVE = 0xffffUL;
        public static readonly long SHT_NULL = 0L;
        public static readonly long SHT_PROGBITS = 1L;
        public static readonly long SHT_SYMTAB = 2L;
        public static readonly long SHT_STRTAB = 3L;
        public static readonly long SHT_RELA = 4L;
        public static readonly long SHT_HASH = 5L;
        public static readonly long SHT_DYNAMIC = 6L;
        public static readonly long SHT_NOTE = 7L;
        public static readonly long SHT_NOBITS = 8L;
        public static readonly long SHT_REL = 9L;
        public static readonly long SHT_SHLIB = 10L;
        public static readonly long SHT_DYNSYM = 11L;
        public static readonly long SHT_INIT_ARRAY = 14L;
        public static readonly long SHT_FINI_ARRAY = 15L;
        public static readonly long SHT_PREINIT_ARRAY = 16L;
        public static readonly long SHT_GROUP = 17L;
        public static readonly long SHT_SYMTAB_SHNDX = 18L;
        public static readonly ulong SHT_LOOS = 0x60000000UL;
        public static readonly ulong SHT_HIOS = 0x6fffffffUL;
        public static readonly ulong SHT_GNU_VERDEF = 0x6ffffffdUL;
        public static readonly ulong SHT_GNU_VERNEED = 0x6ffffffeUL;
        public static readonly ulong SHT_GNU_VERSYM = 0x6fffffffUL;
        public static readonly ulong SHT_LOPROC = 0x70000000UL;
        public static readonly ulong SHT_ARM_ATTRIBUTES = 0x70000003UL;
        public static readonly ulong SHT_HIPROC = 0x7fffffffUL;
        public static readonly ulong SHT_LOUSER = 0x80000000UL;
        public static readonly ulong SHT_HIUSER = 0xffffffffUL;
        public static readonly ulong SHF_WRITE = 0x1UL;
        public static readonly ulong SHF_ALLOC = 0x2UL;
        public static readonly ulong SHF_EXECINSTR = 0x4UL;
        public static readonly ulong SHF_MERGE = 0x10UL;
        public static readonly ulong SHF_STRINGS = 0x20UL;
        public static readonly ulong SHF_INFO_LINK = 0x40UL;
        public static readonly ulong SHF_LINK_ORDER = 0x80UL;
        public static readonly ulong SHF_OS_NONCONFORMING = 0x100UL;
        public static readonly ulong SHF_GROUP = 0x200UL;
        public static readonly ulong SHF_TLS = 0x400UL;
        public static readonly ulong SHF_MASKOS = 0x0ff00000UL;
        public static readonly ulong SHF_MASKPROC = 0xf0000000UL;
        public static readonly long PT_NULL = 0L;
        public static readonly long PT_LOAD = 1L;
        public static readonly long PT_DYNAMIC = 2L;
        public static readonly long PT_INTERP = 3L;
        public static readonly long PT_NOTE = 4L;
        public static readonly long PT_SHLIB = 5L;
        public static readonly long PT_PHDR = 6L;
        public static readonly long PT_TLS = 7L;
        public static readonly ulong PT_LOOS = 0x60000000UL;
        public static readonly ulong PT_HIOS = 0x6fffffffUL;
        public static readonly ulong PT_LOPROC = 0x70000000UL;
        public static readonly ulong PT_HIPROC = 0x7fffffffUL;
        public static readonly ulong PT_GNU_STACK = 0x6474e551UL;
        public static readonly ulong PT_GNU_RELRO = 0x6474e552UL;
        public static readonly ulong PT_PAX_FLAGS = 0x65041580UL;
        public static readonly ulong PT_SUNWSTACK = 0x6ffffffbUL;
        public static readonly ulong PF_X = 0x1UL;
        public static readonly ulong PF_W = 0x2UL;
        public static readonly ulong PF_R = 0x4UL;
        public static readonly ulong PF_MASKOS = 0x0ff00000UL;
        public static readonly ulong PF_MASKPROC = 0xf0000000UL;
        public static readonly long DT_NULL = 0L;
        public static readonly long DT_NEEDED = 1L;
        public static readonly long DT_PLTRELSZ = 2L;
        public static readonly long DT_PLTGOT = 3L;
        public static readonly long DT_HASH = 4L;
        public static readonly long DT_STRTAB = 5L;
        public static readonly long DT_SYMTAB = 6L;
        public static readonly long DT_RELA = 7L;
        public static readonly long DT_RELASZ = 8L;
        public static readonly long DT_RELAENT = 9L;
        public static readonly long DT_STRSZ = 10L;
        public static readonly long DT_SYMENT = 11L;
        public static readonly long DT_INIT = 12L;
        public static readonly long DT_FINI = 13L;
        public static readonly long DT_SONAME = 14L;
        public static readonly long DT_RPATH = 15L;
        public static readonly long DT_SYMBOLIC = 16L;
        public static readonly long DT_REL = 17L;
        public static readonly long DT_RELSZ = 18L;
        public static readonly long DT_RELENT = 19L;
        public static readonly long DT_PLTREL = 20L;
        public static readonly long DT_DEBUG = 21L;
        public static readonly long DT_TEXTREL = 22L;
        public static readonly long DT_JMPREL = 23L;
        public static readonly long DT_BIND_NOW = 24L;
        public static readonly long DT_INIT_ARRAY = 25L;
        public static readonly long DT_FINI_ARRAY = 26L;
        public static readonly long DT_INIT_ARRAYSZ = 27L;
        public static readonly long DT_FINI_ARRAYSZ = 28L;
        public static readonly long DT_RUNPATH = 29L;
        public static readonly long DT_FLAGS = 30L;
        public static readonly long DT_ENCODING = 32L;
        public static readonly long DT_PREINIT_ARRAY = 32L;
        public static readonly long DT_PREINIT_ARRAYSZ = 33L;
        public static readonly ulong DT_LOOS = 0x6000000dUL;
        public static readonly ulong DT_HIOS = 0x6ffff000UL;
        public static readonly ulong DT_LOPROC = 0x70000000UL;
        public static readonly ulong DT_HIPROC = 0x7fffffffUL;
        public static readonly ulong DT_VERNEED = 0x6ffffffeUL;
        public static readonly ulong DT_VERNEEDNUM = 0x6fffffffUL;
        public static readonly ulong DT_VERSYM = 0x6ffffff0UL;
        public static readonly var DT_PPC64_GLINK = DT_LOPROC + 0L;
        public static readonly var DT_PPC64_OPT = DT_LOPROC + 3L;
        public static readonly ulong DF_ORIGIN = 0x0001UL;
        public static readonly ulong DF_SYMBOLIC = 0x0002UL;
        public static readonly ulong DF_TEXTREL = 0x0004UL;
        public static readonly ulong DF_BIND_NOW = 0x0008UL;
        public static readonly ulong DF_STATIC_TLS = 0x0010UL;
        public static readonly long NT_PRSTATUS = 1L;
        public static readonly long NT_FPREGSET = 2L;
        public static readonly long NT_PRPSINFO = 3L;
        public static readonly long STB_LOCAL = 0L;
        public static readonly long STB_GLOBAL = 1L;
        public static readonly long STB_WEAK = 2L;
        public static readonly long STB_LOOS = 10L;
        public static readonly long STB_HIOS = 12L;
        public static readonly long STB_LOPROC = 13L;
        public static readonly long STB_HIPROC = 15L;
        public static readonly long STT_NOTYPE = 0L;
        public static readonly long STT_OBJECT = 1L;
        public static readonly long STT_FUNC = 2L;
        public static readonly long STT_SECTION = 3L;
        public static readonly long STT_FILE = 4L;
        public static readonly long STT_COMMON = 5L;
        public static readonly long STT_TLS = 6L;
        public static readonly long STT_LOOS = 10L;
        public static readonly long STT_HIOS = 12L;
        public static readonly long STT_LOPROC = 13L;
        public static readonly long STT_HIPROC = 15L;
        public static readonly ulong STV_DEFAULT = 0x0UL;
        public static readonly ulong STV_INTERNAL = 0x1UL;
        public static readonly ulong STV_HIDDEN = 0x2UL;
        public static readonly ulong STV_PROTECTED = 0x3UL;
        public static readonly long STN_UNDEF = 0L;

        /* For accessing the fields of r_info. */

        /* For constructing r_info from field values. */

        /*
         * Relocation types.
         */
        public static readonly ulong ARM_MAGIC_TRAMP_NUMBER = 0x5c000003UL;

        /*
         * Symbol table entries.
         */

        /* For accessing the fields of st_info. */

        /* For constructing st_info from field values. */

        /* For accessing the fields of st_other. */

        /*
         * ELF header.
         */
        public partial struct ElfEhdr
        {
            public array<byte> ident;
            public ushort type_;
            public ushort machine;
            public uint version;
            public ulong entry;
            public ulong phoff;
            public ulong shoff;
            public uint flags;
            public ushort ehsize;
            public ushort phentsize;
            public ushort phnum;
            public ushort shentsize;
            public ushort shnum;
            public ushort shstrndx;
        }

        /*
         * Section header.
         */
        public partial struct ElfShdr
        {
            public uint name;
            public uint type_;
            public ulong flags;
            public ulong addr;
            public ulong off;
            public ulong size;
            public uint link;
            public uint info;
            public ulong addralign;
            public ulong entsize;
            public long shnum;
        }

        /*
         * Program header.
         */
        public partial struct ElfPhdr
        {
            public uint type_;
            public uint flags;
            public ulong off;
            public ulong vaddr;
            public ulong paddr;
            public ulong filesz;
            public ulong memsz;
            public ulong align;
        }

        /* For accessing the fields of r_info. */

        /* For constructing r_info from field values. */

        /*
         * Symbol table entries.
         */

        /* For accessing the fields of st_info. */

        /* For constructing st_info from field values. */

        /* For accessing the fields of st_other. */

        /*
         * Go linker interface
         */
        public static readonly long ELF64HDRSIZE = 64L;
        public static readonly long ELF64PHDRSIZE = 56L;
        public static readonly long ELF64SHDRSIZE = 64L;
        public static readonly long ELF64RELSIZE = 16L;
        public static readonly long ELF64RELASIZE = 24L;
        public static readonly long ELF64SYMSIZE = 24L;
        public static readonly long ELF32HDRSIZE = 52L;
        public static readonly long ELF32PHDRSIZE = 32L;
        public static readonly long ELF32SHDRSIZE = 40L;
        public static readonly long ELF32SYMSIZE = 16L;
        public static readonly long ELF32RELSIZE = 8L;

        /*
         * The interface uses the 64-bit structures always,
         * to avoid code duplication.  The writers know how to
         * marshal a 32-bit representation from the 64-bit structure.
         */

        public static slice<byte> Elfstrdat = default;

        /*
         * Total amount of space to reserve at the start of the file
         * for Header, PHeaders, SHeaders, and interp.
         * May waste some.
         * On FreeBSD, cannot be larger than a page.
         */
        public static readonly long ELFRESERVE = 4096L;

        /*
         * We use the 64-bit data structures on both 32- and 64-bit machines
         * in order to write the code just once.  The 64-bit data structure is
         * written in the 32-bit format on the 32-bit machines.
         */
        public static readonly long NSECT = 400L;

        public static long Nelfsym = 1L;        private static bool elf64 = default;        private static @string elfRelType = default;        private static ElfEhdr ehdr = default;        private static array<ref ElfPhdr> phdr = new array<ref ElfPhdr>(NSECT);        private static array<ref ElfShdr> shdr = new array<ref ElfShdr>(NSECT);        private static @string interp = default;

        public partial struct Elfstring
        {
            public @string s;
            public long off;
        }

        private static array<Elfstring> elfstr = new array<Elfstring>(100L);

        private static long nelfstr = default;

        private static slice<byte> buildinfo = default;

        /*
         Initialize the global variable that describes the ELF header. It will be updated as
         we write section and prog headers.
        */
        public static void Elfinit(ref Link ctxt)
        {
            ctxt.IsELF = true;

            if (ctxt.Arch.InFamily(sys.AMD64, sys.ARM64, sys.MIPS64, sys.PPC64, sys.S390X))
            {
                elfRelType = ".rela";
            }
            else
            {
                elfRelType = ".rel";
            }

            // 64-bit architectures
            if (ctxt.Arch.Family == sys.PPC64 || ctxt.Arch.Family == sys.S390X)
            {
                if (ctxt.Arch.ByteOrder == binary.BigEndian)
                {
                    ehdr.flags = 1L; /* Version 1 ABI */
                }
                else
                {
                    ehdr.flags = 2L; /* Version 2 ABI */
                }
                fallthrough = true;
            }
            if (fallthrough || ctxt.Arch.Family == sys.AMD64 || ctxt.Arch.Family == sys.ARM64 || ctxt.Arch.Family == sys.MIPS64)
            {
                if (ctxt.Arch.Family == sys.MIPS64)
                {
                    ehdr.flags = 0x20000004UL; /* MIPS 3 CPIC */
                }
                elf64 = true;

                ehdr.phoff = ELF64HDRSIZE; /* Must be be ELF64HDRSIZE: first PHdr must follow ELF header */
                ehdr.shoff = ELF64HDRSIZE; /* Will move as we add PHeaders */
                ehdr.ehsize = ELF64HDRSIZE; /* Must be ELF64HDRSIZE */
                ehdr.phentsize = ELF64PHDRSIZE; /* Must be ELF64PHDRSIZE */
                ehdr.shentsize = ELF64SHDRSIZE;                /* Must be ELF64SHDRSIZE */

                // 32-bit architectures
                goto __switch_break0;
            }
            if (ctxt.Arch.Family == sys.ARM || ctxt.Arch.Family == sys.MIPS)
            {
                if (ctxt.Arch.Family == sys.ARM)
                { 
                    // we use EABI on linux/arm, freebsd/arm, netbsd/arm.
                    if (ctxt.HeadType == objabi.Hlinux || ctxt.HeadType == objabi.Hfreebsd || ctxt.HeadType == objabi.Hnetbsd)
                    { 
                        // We set a value here that makes no indication of which
                        // float ABI the object uses, because this is information
                        // used by the dynamic linker to compare executables and
                        // shared libraries -- so it only matters for cgo calls, and
                        // the information properly comes from the object files
                        // produced by the host C compiler. parseArmAttributes in
                        // ldelf.go reads that information and updates this field as
                        // appropriate.
                        ehdr.flags = 0x5000002UL; // has entry point, Version5 EABI
                    }
                }
                else if (ctxt.Arch.Family == sys.MIPS)
                {
                    ehdr.flags = 0x50001004UL; /* MIPS 32 CPIC O32*/
                }
            }
            // default: 
                ehdr.phoff = ELF32HDRSIZE; 
                /* Must be be ELF32HDRSIZE: first PHdr must follow ELF header */
                ehdr.shoff = ELF32HDRSIZE; /* Will move as we add PHeaders */
                ehdr.ehsize = ELF32HDRSIZE; /* Must be ELF32HDRSIZE */
                ehdr.phentsize = ELF32PHDRSIZE; /* Must be ELF32PHDRSIZE */
                ehdr.shentsize = ELF32SHDRSIZE; /* Must be ELF32SHDRSIZE */

            __switch_break0:;
        }

        // Make sure PT_LOAD is aligned properly and
        // that there is no gap,
        // correct ELF loaders will do this implicitly,
        // but buggy ELF loaders like the one in some
        // versions of QEMU and UPX won't.
        private static void fixElfPhdr(ref ElfPhdr e)
        {
            var frag = int(e.vaddr & (e.align - 1L));

            e.off -= uint64(frag);
            e.vaddr -= uint64(frag);
            e.paddr -= uint64(frag);
            e.filesz += uint64(frag);
            e.memsz += uint64(frag);
        }

        private static void elf64phdr(ref OutBuf @out, ref ElfPhdr e)
        {
            if (e.type_ == PT_LOAD)
            {
                fixElfPhdr(e);
            }
            @out.Write32(e.type_);
            @out.Write32(e.flags);
            @out.Write64(e.off);
            @out.Write64(e.vaddr);
            @out.Write64(e.paddr);
            @out.Write64(e.filesz);
            @out.Write64(e.memsz);
            @out.Write64(e.align);
        }

        private static void elf32phdr(ref OutBuf @out, ref ElfPhdr e)
        {
            if (e.type_ == PT_LOAD)
            {
                fixElfPhdr(e);
            }
            @out.Write32(e.type_);
            @out.Write32(uint32(e.off));
            @out.Write32(uint32(e.vaddr));
            @out.Write32(uint32(e.paddr));
            @out.Write32(uint32(e.filesz));
            @out.Write32(uint32(e.memsz));
            @out.Write32(e.flags);
            @out.Write32(uint32(e.align));
        }

        private static void elf64shdr(ref OutBuf @out, ref ElfShdr e)
        {
            @out.Write32(e.name);
            @out.Write32(e.type_);
            @out.Write64(e.flags);
            @out.Write64(e.addr);
            @out.Write64(e.off);
            @out.Write64(e.size);
            @out.Write32(e.link);
            @out.Write32(e.info);
            @out.Write64(e.addralign);
            @out.Write64(e.entsize);
        }

        private static void elf32shdr(ref OutBuf @out, ref ElfShdr e)
        {
            @out.Write32(e.name);
            @out.Write32(e.type_);
            @out.Write32(uint32(e.flags));
            @out.Write32(uint32(e.addr));
            @out.Write32(uint32(e.off));
            @out.Write32(uint32(e.size));
            @out.Write32(e.link);
            @out.Write32(e.info);
            @out.Write32(uint32(e.addralign));
            @out.Write32(uint32(e.entsize));
        }

        private static uint elfwriteshdrs(ref OutBuf @out)
        {
            if (elf64)
            {
                {
                    long i__prev1 = i;

                    for (long i = 0L; i < int(ehdr.shnum); i++)
                    {
                        elf64shdr(out, shdr[i]);
                    }


                    i = i__prev1;
                }
                return uint32(ehdr.shnum) * ELF64SHDRSIZE;
            }
            {
                long i__prev1 = i;

                for (i = 0L; i < int(ehdr.shnum); i++)
                {
                    elf32shdr(out, shdr[i]);
                }


                i = i__prev1;
            }
            return uint32(ehdr.shnum) * ELF32SHDRSIZE;
        }

        private static void elfsetstring(ref sym.Symbol s, @string str, long off)
        {
            if (nelfstr >= len(elfstr))
            {
                Errorf(s, "too many elf strings");
                errorexit();
            }
            elfstr[nelfstr].s = str;
            elfstr[nelfstr].off = off;
            nelfstr++;
        }

        private static uint elfwritephdrs(ref OutBuf @out)
        {
            if (elf64)
            {
                {
                    long i__prev1 = i;

                    for (long i = 0L; i < int(ehdr.phnum); i++)
                    {
                        elf64phdr(out, phdr[i]);
                    }


                    i = i__prev1;
                }
                return uint32(ehdr.phnum) * ELF64PHDRSIZE;
            }
            {
                long i__prev1 = i;

                for (i = 0L; i < int(ehdr.phnum); i++)
                {
                    elf32phdr(out, phdr[i]);
                }


                i = i__prev1;
            }
            return uint32(ehdr.phnum) * ELF32PHDRSIZE;
        }

        private static ref ElfPhdr newElfPhdr()
        {
            ptr<ElfPhdr> e = @new<ElfPhdr>();
            if (ehdr.phnum >= NSECT)
            {
                Errorf(null, "too many phdrs");
            }
            else
            {
                phdr[ehdr.phnum] = e;
                ehdr.phnum++;
            }
            if (elf64)
            {
                ehdr.shoff += ELF64PHDRSIZE;
            }
            else
            {
                ehdr.shoff += ELF32PHDRSIZE;
            }
            return e;
        }

        private static ref ElfShdr newElfShdr(long name)
        {
            ptr<ElfShdr> e = @new<ElfShdr>();
            e.name = uint32(name);
            e.shnum = int(ehdr.shnum);
            if (ehdr.shnum >= NSECT)
            {
                Errorf(null, "too many shdrs");
            }
            else
            {
                shdr[ehdr.shnum] = e;
                ehdr.shnum++;
            }
            return e;
        }

        private static ref ElfEhdr getElfEhdr()
        {
            return ref ehdr;
        }

        private static uint elf64writehdr(ref OutBuf @out)
        {
            @out.Write(ehdr.ident[..]);
            @out.Write16(ehdr.type_);
            @out.Write16(ehdr.machine);
            @out.Write32(ehdr.version);
            @out.Write64(ehdr.entry);
            @out.Write64(ehdr.phoff);
            @out.Write64(ehdr.shoff);
            @out.Write32(ehdr.flags);
            @out.Write16(ehdr.ehsize);
            @out.Write16(ehdr.phentsize);
            @out.Write16(ehdr.phnum);
            @out.Write16(ehdr.shentsize);
            @out.Write16(ehdr.shnum);
            @out.Write16(ehdr.shstrndx);
            return ELF64HDRSIZE;
        }

        private static uint elf32writehdr(ref OutBuf @out)
        {
            @out.Write(ehdr.ident[..]);
            @out.Write16(ehdr.type_);
            @out.Write16(ehdr.machine);
            @out.Write32(ehdr.version);
            @out.Write32(uint32(ehdr.entry));
            @out.Write32(uint32(ehdr.phoff));
            @out.Write32(uint32(ehdr.shoff));
            @out.Write32(ehdr.flags);
            @out.Write16(ehdr.ehsize);
            @out.Write16(ehdr.phentsize);
            @out.Write16(ehdr.phnum);
            @out.Write16(ehdr.shentsize);
            @out.Write16(ehdr.shnum);
            @out.Write16(ehdr.shstrndx);
            return ELF32HDRSIZE;
        }

        private static uint elfwritehdr(ref OutBuf @out)
        {
            if (elf64)
            {
                return elf64writehdr(out);
            }
            return elf32writehdr(out);
        }

        /* Taken directly from the definition document for ELF64 */
        private static uint elfhash(@string name)
        {
            uint h = default;
            for (long i = 0L; i < len(name); i++)
            {
                h = (h << (int)(4L)) + uint32(name[i]);
                {
                    var g = h & 0xf0000000UL;

                    if (g != 0L)
                    {
                        h ^= g >> (int)(24L);
                    }

                }
                h &= 0x0fffffffUL;
            }

            return h;
        }

        public static void Elfwritedynent(ref Link ctxt, ref sym.Symbol s, long tag, ulong val)
        {
            if (elf64)
            {
                s.AddUint64(ctxt.Arch, uint64(tag));
                s.AddUint64(ctxt.Arch, val);
            }
            else
            {
                s.AddUint32(ctxt.Arch, uint32(tag));
                s.AddUint32(ctxt.Arch, uint32(val));
            }
        }

        private static void elfwritedynentsym(ref Link ctxt, ref sym.Symbol s, long tag, ref sym.Symbol t)
        {
            Elfwritedynentsymplus(ctxt, s, tag, t, 0L);
        }

        public static void Elfwritedynentsymplus(ref Link ctxt, ref sym.Symbol s, long tag, ref sym.Symbol t, long add)
        {
            if (elf64)
            {
                s.AddUint64(ctxt.Arch, uint64(tag));
            }
            else
            {
                s.AddUint32(ctxt.Arch, uint32(tag));
            }
            s.AddAddrPlus(ctxt.Arch, t, add);
        }

        private static void elfwritedynentsymsize(ref Link ctxt, ref sym.Symbol s, long tag, ref sym.Symbol t)
        {
            if (elf64)
            {
                s.AddUint64(ctxt.Arch, uint64(tag));
            }
            else
            {
                s.AddUint32(ctxt.Arch, uint32(tag));
            }
            s.AddSize(ctxt.Arch, t);
        }

        private static long elfinterp(ref ElfShdr sh, ulong startva, ulong resoff, @string p)
        {
            interp = p;
            var n = len(interp) + 1L;
            sh.addr = startva + resoff - uint64(n);
            sh.off = resoff - uint64(n);
            sh.size = uint64(n);

            return n;
        }

        private static long elfwriteinterp(ref OutBuf @out)
        {
            var sh = elfshname(".interp");
            @out.SeekSet(int64(sh.off));
            @out.WriteString(interp);
            @out.Write8(0L);
            return int(sh.size);
        }

        private static long elfnote(ref ElfShdr sh, ulong startva, ulong resoff, long sz, bool alloc)
        {
            long n = 3L * 4L + uint64(sz) + resoff % 4L;

            sh.type_ = SHT_NOTE;
            if (alloc)
            {
                sh.flags = SHF_ALLOC;
            }
            sh.addralign = 4L;
            sh.addr = startva + resoff - n;
            sh.off = resoff - n;
            sh.size = n - resoff % 4L;

            return int(n);
        }

        private static ref ElfShdr elfwritenotehdr(ref OutBuf @out, @string str, uint namesz, uint descsz, uint tag)
        {
            var sh = elfshname(str); 

            // Write Elf_Note header.
            @out.SeekSet(int64(sh.off));

            @out.Write32(namesz);
            @out.Write32(descsz);
            @out.Write32(tag);

            return sh;
        }

        // NetBSD Signature (as per sys/exec_elf.h)
        public static readonly long ELF_NOTE_NETBSD_NAMESZ = 7L;
        public static readonly long ELF_NOTE_NETBSD_DESCSZ = 4L;
        public static readonly long ELF_NOTE_NETBSD_TAG = 1L;
        public static readonly long ELF_NOTE_NETBSD_VERSION = 599000000L; /* NetBSD 5.99 */

        public static slice<byte> ELF_NOTE_NETBSD_NAME = (slice<byte>)"NetBSD\x00";

        private static long elfnetbsdsig(ref ElfShdr sh, ulong startva, ulong resoff)
        {
            var n = int(Rnd(ELF_NOTE_NETBSD_NAMESZ, 4L) + Rnd(ELF_NOTE_NETBSD_DESCSZ, 4L));
            return elfnote(sh, startva, resoff, n, true);
        }

        private static long elfwritenetbsdsig(ref OutBuf @out)
        { 
            // Write Elf_Note header.
            var sh = elfwritenotehdr(out, ".note.netbsd.ident", ELF_NOTE_NETBSD_NAMESZ, ELF_NOTE_NETBSD_DESCSZ, ELF_NOTE_NETBSD_TAG);

            if (sh == null)
            {
                return 0L;
            } 

            // Followed by NetBSD string and version.
            @out.Write(ELF_NOTE_NETBSD_NAME);
            @out.Write8(0L);
            @out.Write32(ELF_NOTE_NETBSD_VERSION);

            return int(sh.size);
        }

        // OpenBSD Signature
        public static readonly long ELF_NOTE_OPENBSD_NAMESZ = 8L;
        public static readonly long ELF_NOTE_OPENBSD_DESCSZ = 4L;
        public static readonly long ELF_NOTE_OPENBSD_TAG = 1L;
        public static readonly long ELF_NOTE_OPENBSD_VERSION = 0L;

        public static slice<byte> ELF_NOTE_OPENBSD_NAME = (slice<byte>)"OpenBSD\x00";

        private static long elfopenbsdsig(ref ElfShdr sh, ulong startva, ulong resoff)
        {
            var n = ELF_NOTE_OPENBSD_NAMESZ + ELF_NOTE_OPENBSD_DESCSZ;
            return elfnote(sh, startva, resoff, n, true);
        }

        private static long elfwriteopenbsdsig(ref OutBuf @out)
        { 
            // Write Elf_Note header.
            var sh = elfwritenotehdr(out, ".note.openbsd.ident", ELF_NOTE_OPENBSD_NAMESZ, ELF_NOTE_OPENBSD_DESCSZ, ELF_NOTE_OPENBSD_TAG);

            if (sh == null)
            {
                return 0L;
            } 

            // Followed by OpenBSD string and version.
            @out.Write(ELF_NOTE_OPENBSD_NAME);

            @out.Write32(ELF_NOTE_OPENBSD_VERSION);

            return int(sh.size);
        }

        private static void addbuildinfo(@string val)
        {
            if (!strings.HasPrefix(val, "0x"))
            {
                Exitf("-B argument must start with 0x: %s", val);
            }
            var ov = val;
            val = val[2L..];

            const long maxLen = 32L;

            if (hex.DecodedLen(len(val)) > maxLen)
            {
                Exitf("-B option too long (max %d digits): %s", maxLen, ov);
            }
            var (b, err) = hex.DecodeString(val);
            if (err != null)
            {
                if (err == hex.ErrLength)
                {
                    Exitf("-B argument must have even number of digits: %s", ov);
                }
                {
                    hex.InvalidByteError (inv, ok) = err._<hex.InvalidByteError>();

                    if (ok)
                    {
                        Exitf("-B argument contains invalid hex digit %c: %s", byte(inv), ov);
                    }

                }
                Exitf("-B argument contains invalid hex: %s", ov);
            }
            buildinfo = b;
        }

        // Build info note
        public static readonly long ELF_NOTE_BUILDINFO_NAMESZ = 4L;
        public static readonly long ELF_NOTE_BUILDINFO_TAG = 3L;

        public static slice<byte> ELF_NOTE_BUILDINFO_NAME = (slice<byte>)"GNU\x00";

        private static long elfbuildinfo(ref ElfShdr sh, ulong startva, ulong resoff)
        {
            var n = int(ELF_NOTE_BUILDINFO_NAMESZ + Rnd(int64(len(buildinfo)), 4L));
            return elfnote(sh, startva, resoff, n, true);
        }

        private static long elfgobuildid(ref ElfShdr sh, ulong startva, ulong resoff)
        {
            var n = len(ELF_NOTE_GO_NAME) + int(Rnd(int64(len(flagBuildid.Value)), 4L));
            return elfnote(sh, startva, resoff, n, true);
        }

        private static long elfwritebuildinfo(ref OutBuf @out)
        {
            var sh = elfwritenotehdr(out, ".note.gnu.build-id", ELF_NOTE_BUILDINFO_NAMESZ, uint32(len(buildinfo)), ELF_NOTE_BUILDINFO_TAG);
            if (sh == null)
            {
                return 0L;
            }
            @out.Write(ELF_NOTE_BUILDINFO_NAME);
            @out.Write(buildinfo);
            var zero = make_slice<byte>(4L);
            @out.Write(zero[..int(Rnd(int64(len(buildinfo)), 4L) - int64(len(buildinfo)))]);

            return int(sh.size);
        }

        private static long elfwritegobuildid(ref OutBuf @out)
        {
            var sh = elfwritenotehdr(out, ".note.go.buildid", uint32(len(ELF_NOTE_GO_NAME)), uint32(len(flagBuildid.Value)), ELF_NOTE_GOBUILDID_TAG);
            if (sh == null)
            {
                return 0L;
            }
            @out.Write(ELF_NOTE_GO_NAME);
            @out.Write((slice<byte>)flagBuildid.Value);
            var zero = make_slice<byte>(4L);
            @out.Write(zero[..int(Rnd(int64(len(flagBuildid.Value)), 4L) - int64(len(flagBuildid.Value)))]);

            return int(sh.size);
        }

        // Go specific notes
        public static readonly long ELF_NOTE_GOPKGLIST_TAG = 1L;
        public static readonly long ELF_NOTE_GOABIHASH_TAG = 2L;
        public static readonly long ELF_NOTE_GODEPS_TAG = 3L;
        public static readonly long ELF_NOTE_GOBUILDID_TAG = 4L;

        public static slice<byte> ELF_NOTE_GO_NAME = (slice<byte>)"Go\x00\x00";

        private static long elfverneed = default;

        public partial struct Elfaux
        {
            public ptr<Elfaux> next;
            public long num;
            public @string vers;
        }

        public partial struct Elflib
        {
            public ptr<Elflib> next;
            public ptr<Elfaux> aux;
            public @string file;
        }

        private static ref Elfaux addelflib(ptr<ptr<Elflib>> list, @string file, @string vers)
        {
            ref Elflib lib = default;

            lib = list.Value;

            while (lib != null)
            {
                if (lib.file == file)
                {
                    goto havelib;
                lib = lib.next;
                }
            }

            lib = @new<Elflib>();
            lib.next = list.Value;
            lib.file = file;
            list.Value = lib;

havelib:
            {
                var aux__prev1 = aux;

                var aux = lib.aux;

                while (aux != null)
                {
                    if (aux.vers == vers)
                    {
                        return aux;
                    aux = aux.next;
                    }
                }


                aux = aux__prev1;
            }
            aux = @new<Elfaux>();
            aux.next = lib.aux;
            aux.vers = vers;
            lib.aux = aux;

            return aux;
        }

        private static void elfdynhash(ref Link ctxt)
        {
            if (!ctxt.IsELF)
            {
                return;
            }
            var nsym = Nelfsym;
            var s = ctxt.Syms.Lookup(".hash", 0L);
            s.Type = sym.SELFROSECT;
            s.Attr |= sym.AttrReachable;

            var i = nsym;
            long nbucket = 1L;
            while (i > 0L)
            {
                nbucket++;
                i >>= 1L;
            }


            ref Elflib needlib = default;
            var need = make_slice<ref Elfaux>(nsym);
            var chain = make_slice<uint>(nsym);
            var buckets = make_slice<uint>(nbucket);

            {
                var sy__prev1 = sy;

                foreach (var (_, __sy) in ctxt.Syms.Allsym)
                {
                    sy = __sy;
                    if (sy.Dynid <= 0L)
                    {
                        continue;
                    }
                    if (sy.Dynimpvers != "")
                    {
                        need[sy.Dynid] = addelflib(ref needlib, sy.Dynimplib, sy.Dynimpvers);
                    }
                    var name = sy.Extname;
                    var hc = elfhash(name);

                    var b = hc % uint32(nbucket);
                    chain[sy.Dynid] = buckets[b];
                    buckets[b] = uint32(sy.Dynid);
                } 

                // s390x (ELF64) hash table entries are 8 bytes

                sy = sy__prev1;
            }

            if (ctxt.Arch.Family == sys.S390X)
            {
                s.AddUint64(ctxt.Arch, uint64(nbucket));
                s.AddUint64(ctxt.Arch, uint64(nsym));
                {
                    var i__prev1 = i;

                    for (i = 0L; i < nbucket; i++)
                    {
                        s.AddUint64(ctxt.Arch, uint64(buckets[i]));
                    }
            else


                    i = i__prev1;
                }
                {
                    var i__prev1 = i;

                    for (i = 0L; i < nsym; i++)
                    {
                        s.AddUint64(ctxt.Arch, uint64(chain[i]));
                    }


                    i = i__prev1;
                }
            }            {
                s.AddUint32(ctxt.Arch, uint32(nbucket));
                s.AddUint32(ctxt.Arch, uint32(nsym));
                {
                    var i__prev1 = i;

                    for (i = 0L; i < nbucket; i++)
                    {
                        s.AddUint32(ctxt.Arch, buckets[i]);
                    }


                    i = i__prev1;
                }
                {
                    var i__prev1 = i;

                    for (i = 0L; i < nsym; i++)
                    {
                        s.AddUint32(ctxt.Arch, chain[i]);
                    }


                    i = i__prev1;
                }
            } 

            // version symbols
            var dynstr = ctxt.Syms.Lookup(".dynstr", 0L);

            s = ctxt.Syms.Lookup(".gnu.version_r", 0L);
            i = 2L;
            long nfile = 0L;
            {
                var l = needlib;

                while (l != null)
                {
                    nfile++; 

                    // header
                    s.AddUint16(ctxt.Arch, 1L); // table version
                    long j = 0L;
                    {
                        var x__prev2 = x;

                        var x = l.aux;

                        while (x != null)
                        {
                            j++;
                            x = x.next;
                        }


                        x = x__prev2;
                    }
                    s.AddUint16(ctxt.Arch, uint16(j)); // aux count
                    s.AddUint32(ctxt.Arch, uint32(Addstring(dynstr, l.file))); // file string offset
                    s.AddUint32(ctxt.Arch, 16L); // offset from header to first aux
                    if (l.next != null)
                    {
                        s.AddUint32(ctxt.Arch, 16L + uint32(j) * 16L); // offset from this header to next
                    l = l.next;
                    }
                    else
                    {
                        s.AddUint32(ctxt.Arch, 0L);
                    }
                    {
                        var x__prev2 = x;

                        x = l.aux;

                        while (x != null)
                        {
                            x.num = i;
                            i++; 

                            // aux struct
                            s.AddUint32(ctxt.Arch, elfhash(x.vers)); // hash
                            s.AddUint16(ctxt.Arch, 0L); // flags
                            s.AddUint16(ctxt.Arch, uint16(x.num)); // other - index we refer to this by
                            s.AddUint32(ctxt.Arch, uint32(Addstring(dynstr, x.vers))); // version string offset
                            if (x.next != null)
                            {
                                s.AddUint32(ctxt.Arch, 16L); // offset from this aux to next
                            x = x.next;
                            }
                            else
                            {
                                s.AddUint32(ctxt.Arch, 0L);
                            }
                        }


                        x = x__prev2;
                    }
                } 

                // version references

            } 

            // version references
            s = ctxt.Syms.Lookup(".gnu.version", 0L);

            {
                var i__prev1 = i;

                for (i = 0L; i < nsym; i++)
                {
                    if (i == 0L)
                    {
                        s.AddUint16(ctxt.Arch, 0L); // first entry - no symbol
                    }
                    else if (need[i] == null)
                    {
                        s.AddUint16(ctxt.Arch, 1L); // global
                    }
                    else
                    {
                        s.AddUint16(ctxt.Arch, uint16(need[i].num));
                    }
                }


                i = i__prev1;
            }

            s = ctxt.Syms.Lookup(".dynamic", 0L);
            elfverneed = nfile;
            if (elfverneed != 0L)
            {
                elfwritedynentsym(ctxt, s, DT_VERNEED, ctxt.Syms.Lookup(".gnu.version_r", 0L));
                Elfwritedynent(ctxt, s, DT_VERNEEDNUM, uint64(nfile));
                elfwritedynentsym(ctxt, s, DT_VERSYM, ctxt.Syms.Lookup(".gnu.version", 0L));
            }
            var sy = ctxt.Syms.Lookup(elfRelType + ".plt", 0L);
            if (sy.Size > 0L)
            {
                if (elfRelType == ".rela")
                {
                    Elfwritedynent(ctxt, s, DT_PLTREL, DT_RELA);
                }
                else
                {
                    Elfwritedynent(ctxt, s, DT_PLTREL, DT_REL);
                }
                elfwritedynentsymsize(ctxt, s, DT_PLTRELSZ, sy);
                elfwritedynentsym(ctxt, s, DT_JMPREL, sy);
            }
            Elfwritedynent(ctxt, s, DT_NULL, 0L);
        }

        private static ref ElfPhdr elfphload(ref sym.Segment seg)
        {
            var ph = newElfPhdr();
            ph.type_ = PT_LOAD;
            if (seg.Rwx & 4L != 0L)
            {
                ph.flags |= PF_R;
            }
            if (seg.Rwx & 2L != 0L)
            {
                ph.flags |= PF_W;
            }
            if (seg.Rwx & 1L != 0L)
            {
                ph.flags |= PF_X;
            }
            ph.vaddr = seg.Vaddr;
            ph.paddr = seg.Vaddr;
            ph.memsz = seg.Length;
            ph.off = seg.Fileoff;
            ph.filesz = seg.Filelen;
            ph.align = uint64(FlagRound.Value);

            return ph;
        }

        private static void elfphrelro(ref sym.Segment seg)
        {
            var ph = newElfPhdr();
            ph.type_ = PT_GNU_RELRO;
            ph.vaddr = seg.Vaddr;
            ph.paddr = seg.Vaddr;
            ph.memsz = seg.Length;
            ph.off = seg.Fileoff;
            ph.filesz = seg.Filelen;
            ph.align = uint64(FlagRound.Value);
        }

        private static ref ElfShdr elfshname(@string name)
        {
            for (long i = 0L; i < nelfstr; i++)
            {
                if (name != elfstr[i].s)
                {
                    continue;
                }
                var off = elfstr[i].off;
                for (i = 0L; i < int(ehdr.shnum); i++)
                {
                    var sh = shdr[i];
                    if (sh.name == uint32(off))
                    {
                        return sh;
                    }
                }

                return newElfShdr(int64(off));
            }

            Exitf("cannot find elf name %s", name);
            return null;
        }

        // Create an ElfShdr for the section with name.
        // Create a duplicate if one already exists with that name
        private static ref ElfShdr elfshnamedup(@string name)
        {
            for (long i = 0L; i < nelfstr; i++)
            {
                if (name == elfstr[i].s)
                {
                    var off = elfstr[i].off;
                    return newElfShdr(int64(off));
                }
            }


            Errorf(null, "cannot find elf name %s", name);
            errorexit();
            return null;
        }

        private static ref ElfShdr elfshalloc(ref sym.Section sect)
        {
            var sh = elfshname(sect.Name);
            sect.Elfsect = sh;
            return sh;
        }

        private static ref ElfShdr elfshbits(LinkMode linkmode, ref sym.Section sect)
        {
            ref ElfShdr sh = default;

            if (sect.Name == ".text")
            {
                if (sect.Elfsect == null)
                {
                    sect.Elfsect = elfshnamedup(sect.Name);
                }
                sh = sect.Elfsect._<ref ElfShdr>();
            }
            else
            {
                sh = elfshalloc(sect);
            } 

            // If this section has already been set up as a note, we assume type_ and
            // flags are already correct, but the other fields still need filling in.
            if (sh.type_ == SHT_NOTE)
            {
                if (linkmode != LinkExternal)
                { 
                    // TODO(mwhudson): the approach here will work OK when
                    // linking internally for notes that we want to be included
                    // in a loadable segment (e.g. the abihash note) but not for
                    // notes that we do not want to be mapped (e.g. the package
                    // list note). The real fix is probably to define new values
                    // for Symbol.Type corresponding to mapped and unmapped notes
                    // and handle them in dodata().
                    Errorf(null, "sh.type_ == SHT_NOTE in elfshbits when linking internally");
                }
                sh.addralign = uint64(sect.Align);
                sh.size = sect.Length;
                sh.off = sect.Seg.Fileoff + sect.Vaddr - sect.Seg.Vaddr;
                return sh;
            }
            if (sh.type_ > 0L)
            {
                return sh;
            }
            if (sect.Vaddr < sect.Seg.Vaddr + sect.Seg.Filelen)
            {
                sh.type_ = SHT_PROGBITS;
            }
            else
            {
                sh.type_ = SHT_NOBITS;
            }
            sh.flags = SHF_ALLOC;
            if (sect.Rwx & 1L != 0L)
            {
                sh.flags |= SHF_EXECINSTR;
            }
            if (sect.Rwx & 2L != 0L)
            {
                sh.flags |= SHF_WRITE;
            }
            if (sect.Name == ".tbss")
            {
                sh.flags |= SHF_TLS;
                sh.type_ = SHT_NOBITS;
            }
            if (strings.HasPrefix(sect.Name, ".debug"))
            {
                sh.flags = 0L;
            }
            if (linkmode != LinkExternal)
            {
                sh.addr = sect.Vaddr;
            }
            sh.addralign = uint64(sect.Align);
            sh.size = sect.Length;
            if (sect.Name != ".tbss")
            {
                sh.off = sect.Seg.Fileoff + sect.Vaddr - sect.Seg.Vaddr;
            }
            return sh;
        }

        private static ref ElfShdr elfshreloc(ref sys.Arch arch, ref sym.Section sect)
        { 
            // If main section is SHT_NOBITS, nothing to relocate.
            // Also nothing to relocate in .shstrtab or notes.
            if (sect.Vaddr >= sect.Seg.Vaddr + sect.Seg.Filelen)
            {
                return null;
            }
            if (sect.Name == ".shstrtab" || sect.Name == ".tbss")
            {
                return null;
            }
            if (sect.Elfsect._<ref ElfShdr>().type_ == SHT_NOTE)
            {
                return null;
            }
            long typ = default;
            if (elfRelType == ".rela")
            {
                typ = SHT_RELA;
            }
            else
            {
                typ = SHT_REL;
            }
            var sh = elfshname(elfRelType + sect.Name); 
            // There could be multiple text sections but each needs
            // its own .rela.text.

            if (sect.Name == ".text")
            {
                if (sh.info != 0L && sh.info != uint32(sect.Elfsect._<ref ElfShdr>().shnum))
                {
                    sh = elfshnamedup(elfRelType + sect.Name);
                }
            }
            sh.type_ = uint32(typ);
            sh.entsize = uint64(arch.RegSize) * 2L;
            if (typ == SHT_RELA)
            {
                sh.entsize += uint64(arch.RegSize);
            }
            sh.link = uint32(elfshname(".symtab").shnum);
            sh.info = uint32(sect.Elfsect._<ref ElfShdr>().shnum);
            sh.off = sect.Reloff;
            sh.size = sect.Rellen;
            sh.addralign = uint64(arch.RegSize);
            return sh;
        }

        private static void elfrelocsect(ref Link ctxt, ref sym.Section sect, slice<ref sym.Symbol> syms)
        { 
            // If main section is SHT_NOBITS, nothing to relocate.
            // Also nothing to relocate in .shstrtab.
            if (sect.Vaddr >= sect.Seg.Vaddr + sect.Seg.Filelen)
            {
                return;
            }
            if (sect.Name == ".shstrtab")
            {
                return;
            }
            sect.Reloff = uint64(ctxt.Out.Offset());
            {
                var s__prev1 = s;

                foreach (var (__i, __s) in syms)
                {
                    i = __i;
                    s = __s;
                    if (!s.Attr.Reachable())
                    {
                        continue;
                    }
                    if (uint64(s.Value) >= sect.Vaddr)
                    {
                        syms = syms[i..];
                        break;
                    }
                }

                s = s__prev1;
            }

            var eaddr = int32(sect.Vaddr + sect.Length);
            {
                var s__prev1 = s;

                foreach (var (_, __s) in syms)
                {
                    s = __s;
                    if (!s.Attr.Reachable())
                    {
                        continue;
                    }
                    if (s.Value >= int64(eaddr))
                    {
                        break;
                    }
                    for (long ri = 0L; ri < len(s.R); ri++)
                    {
                        var r = ref s.R[ri];
                        if (r.Done)
                        {
                            continue;
                        }
                        if (r.Xsym == null)
                        {
                            Errorf(s, "missing xsym in relocation %#v %#v", r.Sym.Name, s);
                            continue;
                        }
                        if (r.Xsym.ElfsymForReloc() == 0L)
                        {
                            Errorf(s, "reloc %d (%s) to non-elf symbol %s (outer=%s) %d (%s)", r.Type, sym.RelocName(ctxt.Arch, r.Type), r.Sym.Name, r.Xsym.Name, r.Sym.Type, r.Sym.Type);
                        }
                        if (!r.Xsym.Attr.Reachable())
                        {
                            Errorf(s, "unreachable reloc %d (%s) target %v", r.Type, sym.RelocName(ctxt.Arch, r.Type), r.Xsym.Name);
                        }
                        if (!Thearch.Elfreloc1(ctxt, r, int64(uint64(s.Value + int64(r.Off)) - sect.Vaddr)))
                        {
                            Errorf(s, "unsupported obj reloc %d (%s)/%d to %s", r.Type, sym.RelocName(ctxt.Arch, r.Type), r.Siz, r.Sym.Name);
                        }
                    }

                }

                s = s__prev1;
            }

            sect.Rellen = uint64(ctxt.Out.Offset()) - sect.Reloff;
        }

        public static void Elfemitreloc(ref Link ctxt)
        {
            while (ctxt.Out.Offset() & 7L != 0L)
            {
                ctxt.Out.Write8(0L);
            }


            {
                var sect__prev1 = sect;

                foreach (var (_, __sect) in Segtext.Sections)
                {
                    sect = __sect;
                    if (sect.Name == ".text")
                    {
                        elfrelocsect(ctxt, sect, ctxt.Textp);
                    }
                    else
                    {
                        elfrelocsect(ctxt, sect, datap);
                    }
                }

                sect = sect__prev1;
            }

            {
                var sect__prev1 = sect;

                foreach (var (_, __sect) in Segrodata.Sections)
                {
                    sect = __sect;
                    elfrelocsect(ctxt, sect, datap);
                }

                sect = sect__prev1;
            }

            {
                var sect__prev1 = sect;

                foreach (var (_, __sect) in Segrelrodata.Sections)
                {
                    sect = __sect;
                    elfrelocsect(ctxt, sect, datap);
                }

                sect = sect__prev1;
            }

            {
                var sect__prev1 = sect;

                foreach (var (_, __sect) in Segdata.Sections)
                {
                    sect = __sect;
                    elfrelocsect(ctxt, sect, datap);
                }

                sect = sect__prev1;
            }

            {
                var sect__prev1 = sect;

                foreach (var (_, __sect) in Segdwarf.Sections)
                {
                    sect = __sect;
                    elfrelocsect(ctxt, sect, dwarfp);
                }

                sect = sect__prev1;
            }

        }

        private static void addgonote(ref Link ctxt, @string sectionName, uint tag, slice<byte> desc)
        {
            var s = ctxt.Syms.Lookup(sectionName, 0L);
            s.Attr |= sym.AttrReachable;
            s.Type = sym.SELFROSECT; 
            // namesz
            s.AddUint32(ctxt.Arch, uint32(len(ELF_NOTE_GO_NAME))); 
            // descsz
            s.AddUint32(ctxt.Arch, uint32(len(desc))); 
            // tag
            s.AddUint32(ctxt.Arch, tag); 
            // name + padding
            s.P = append(s.P, ELF_NOTE_GO_NAME);
            while (len(s.P) % 4L != 0L)
            {
                s.P = append(s.P, 0L);
            } 
            // desc + padding
 
            // desc + padding
            s.P = append(s.P, desc);
            while (len(s.P) % 4L != 0L)
            {
                s.P = append(s.P, 0L);
            }

            s.Size = int64(len(s.P));
            s.Align = 4L;
        }

        private static void doelf(this ref Link ctxt)
        {
            if (!ctxt.IsELF)
            {
                return;
            } 

            /* predefine strings we need for section headers */
            var shstrtab = ctxt.Syms.Lookup(".shstrtab", 0L);

            shstrtab.Type = sym.SELFROSECT;
            shstrtab.Attr |= sym.AttrReachable;

            Addstring(shstrtab, "");
            Addstring(shstrtab, ".text");
            Addstring(shstrtab, ".noptrdata");
            Addstring(shstrtab, ".data");
            Addstring(shstrtab, ".bss");
            Addstring(shstrtab, ".noptrbss"); 

            // generate .tbss section for dynamic internal linker or external
            // linking, so that various binutils could correctly calculate
            // PT_TLS size. See https://golang.org/issue/5200.
            if (!FlagD || ctxt.LinkMode == LinkExternal.Value)
            {
                Addstring(shstrtab, ".tbss");
            }
            if (ctxt.HeadType == objabi.Hnetbsd)
            {
                Addstring(shstrtab, ".note.netbsd.ident");
            }
            if (ctxt.HeadType == objabi.Hopenbsd)
            {
                Addstring(shstrtab, ".note.openbsd.ident");
            }
            if (len(buildinfo) > 0L)
            {
                Addstring(shstrtab, ".note.gnu.build-id");
            }
            if (flagBuildid != "".Value)
            {
                Addstring(shstrtab, ".note.go.buildid");
            }
            Addstring(shstrtab, ".elfdata");
            Addstring(shstrtab, ".rodata"); 
            // See the comment about data.rel.ro.FOO section names in data.go.
            @string relro_prefix = "";
            if (ctxt.UseRelro())
            {
                Addstring(shstrtab, ".data.rel.ro");
                relro_prefix = ".data.rel.ro";
            }
            Addstring(shstrtab, relro_prefix + ".typelink");
            Addstring(shstrtab, relro_prefix + ".itablink");
            Addstring(shstrtab, relro_prefix + ".gosymtab");
            Addstring(shstrtab, relro_prefix + ".gopclntab");

            if (ctxt.LinkMode == LinkExternal)
            {
                FlagD.Value = true;

                Addstring(shstrtab, elfRelType + ".text");
                Addstring(shstrtab, elfRelType + ".rodata");
                Addstring(shstrtab, elfRelType + relro_prefix + ".typelink");
                Addstring(shstrtab, elfRelType + relro_prefix + ".itablink");
                Addstring(shstrtab, elfRelType + relro_prefix + ".gosymtab");
                Addstring(shstrtab, elfRelType + relro_prefix + ".gopclntab");
                Addstring(shstrtab, elfRelType + ".noptrdata");
                Addstring(shstrtab, elfRelType + ".data");
                if (ctxt.UseRelro())
                {
                    Addstring(shstrtab, elfRelType + ".data.rel.ro");
                } 

                // add a .note.GNU-stack section to mark the stack as non-executable
                Addstring(shstrtab, ".note.GNU-stack");

                if (ctxt.BuildMode == BuildModeShared)
                {
                    Addstring(shstrtab, ".note.go.abihash");
                    Addstring(shstrtab, ".note.go.pkg-list");
                    Addstring(shstrtab, ".note.go.deps");
                }
            }
            var hasinitarr = ctxt.linkShared; 

            /* shared library initializer */

            if (ctxt.BuildMode == BuildModeCArchive || ctxt.BuildMode == BuildModeCShared || ctxt.BuildMode == BuildModeShared || ctxt.BuildMode == BuildModePlugin) 
                hasinitarr = true;
                        if (hasinitarr)
            {
                Addstring(shstrtab, ".init_array");
                Addstring(shstrtab, elfRelType + ".init_array");
            }
            if (!FlagS.Value)
            {
                Addstring(shstrtab, ".symtab");
                Addstring(shstrtab, ".strtab");
                dwarfaddshstrings(ctxt, shstrtab);
            }
            Addstring(shstrtab, ".shstrtab");

            if (!FlagD.Value)
            { /* -d suppresses dynamic loader format */
                Addstring(shstrtab, ".interp");
                Addstring(shstrtab, ".hash");
                Addstring(shstrtab, ".got");
                if (ctxt.Arch.Family == sys.PPC64)
                {
                    Addstring(shstrtab, ".glink");
                }
                Addstring(shstrtab, ".got.plt");
                Addstring(shstrtab, ".dynamic");
                Addstring(shstrtab, ".dynsym");
                Addstring(shstrtab, ".dynstr");
                Addstring(shstrtab, elfRelType);
                Addstring(shstrtab, elfRelType + ".plt");

                Addstring(shstrtab, ".plt");
                Addstring(shstrtab, ".gnu.version");
                Addstring(shstrtab, ".gnu.version_r"); 

                /* dynamic symbol table - first entry all zeros */
                var s = ctxt.Syms.Lookup(".dynsym", 0L);

                s.Type = sym.SELFROSECT;
                s.Attr |= sym.AttrReachable;
                if (elf64)
                {
                    s.Size += ELF64SYMSIZE;
                }
                else
                {
                    s.Size += ELF32SYMSIZE;
                } 

                /* dynamic string table */
                s = ctxt.Syms.Lookup(".dynstr", 0L);

                s.Type = sym.SELFROSECT;
                s.Attr |= sym.AttrReachable;
                if (s.Size == 0L)
                {
                    Addstring(s, "");
                }
                var dynstr = s; 

                /* relocation table */
                s = ctxt.Syms.Lookup(elfRelType, 0L);
                s.Attr |= sym.AttrReachable;
                s.Type = sym.SELFROSECT; 

                /* global offset table */
                s = ctxt.Syms.Lookup(".got", 0L);

                s.Attr |= sym.AttrReachable;
                s.Type = sym.SELFGOT; // writable

                /* ppc64 glink resolver */
                if (ctxt.Arch.Family == sys.PPC64)
                {
                    s = ctxt.Syms.Lookup(".glink", 0L);
                    s.Attr |= sym.AttrReachable;
                    s.Type = sym.SELFRXSECT;
                } 

                /* hash */
                s = ctxt.Syms.Lookup(".hash", 0L);

                s.Attr |= sym.AttrReachable;
                s.Type = sym.SELFROSECT;

                s = ctxt.Syms.Lookup(".got.plt", 0L);
                s.Attr |= sym.AttrReachable;
                s.Type = sym.SELFSECT; // writable

                s = ctxt.Syms.Lookup(".plt", 0L);

                s.Attr |= sym.AttrReachable;
                if (ctxt.Arch.Family == sys.PPC64)
                { 
                    // In the ppc64 ABI, .plt is a data section
                    // written by the dynamic linker.
                    s.Type = sym.SELFSECT;
                }
                else
                {
                    s.Type = sym.SELFRXSECT;
                }
                Thearch.Elfsetupplt(ctxt);

                s = ctxt.Syms.Lookup(elfRelType + ".plt", 0L);
                s.Attr |= sym.AttrReachable;
                s.Type = sym.SELFROSECT;

                s = ctxt.Syms.Lookup(".gnu.version", 0L);
                s.Attr |= sym.AttrReachable;
                s.Type = sym.SELFROSECT;

                s = ctxt.Syms.Lookup(".gnu.version_r", 0L);
                s.Attr |= sym.AttrReachable;
                s.Type = sym.SELFROSECT; 

                /* define dynamic elf table */
                s = ctxt.Syms.Lookup(".dynamic", 0L);

                s.Attr |= sym.AttrReachable;
                s.Type = sym.SELFSECT; // writable

                /*
                         * .dynamic table
                         */
                elfwritedynentsym(ctxt, s, DT_HASH, ctxt.Syms.Lookup(".hash", 0L));

                elfwritedynentsym(ctxt, s, DT_SYMTAB, ctxt.Syms.Lookup(".dynsym", 0L));
                if (elf64)
                {
                    Elfwritedynent(ctxt, s, DT_SYMENT, ELF64SYMSIZE);
                }
                else
                {
                    Elfwritedynent(ctxt, s, DT_SYMENT, ELF32SYMSIZE);
                }
                elfwritedynentsym(ctxt, s, DT_STRTAB, ctxt.Syms.Lookup(".dynstr", 0L));
                elfwritedynentsymsize(ctxt, s, DT_STRSZ, ctxt.Syms.Lookup(".dynstr", 0L));
                if (elfRelType == ".rela")
                {
                    elfwritedynentsym(ctxt, s, DT_RELA, ctxt.Syms.Lookup(".rela", 0L));
                    elfwritedynentsymsize(ctxt, s, DT_RELASZ, ctxt.Syms.Lookup(".rela", 0L));
                    Elfwritedynent(ctxt, s, DT_RELAENT, ELF64RELASIZE);
                }
                else
                {
                    elfwritedynentsym(ctxt, s, DT_REL, ctxt.Syms.Lookup(".rel", 0L));
                    elfwritedynentsymsize(ctxt, s, DT_RELSZ, ctxt.Syms.Lookup(".rel", 0L));
                    Elfwritedynent(ctxt, s, DT_RELENT, ELF32RELSIZE);
                }
                if (rpath.val != "")
                {
                    Elfwritedynent(ctxt, s, DT_RUNPATH, uint64(Addstring(dynstr, rpath.val)));
                }
                if (ctxt.Arch.Family == sys.PPC64)
                {
                    elfwritedynentsym(ctxt, s, DT_PLTGOT, ctxt.Syms.Lookup(".plt", 0L));
                }
                else if (ctxt.Arch.Family == sys.S390X)
                {
                    elfwritedynentsym(ctxt, s, DT_PLTGOT, ctxt.Syms.Lookup(".got", 0L));
                }
                else
                {
                    elfwritedynentsym(ctxt, s, DT_PLTGOT, ctxt.Syms.Lookup(".got.plt", 0L));
                }
                if (ctxt.Arch.Family == sys.PPC64)
                {
                    Elfwritedynent(ctxt, s, DT_PPC64_OPT, 0L);
                } 

                // Solaris dynamic linker can't handle an empty .rela.plt if
                // DT_JMPREL is emitted so we have to defer generation of DT_PLTREL,
                // DT_PLTRELSZ, and DT_JMPREL dynamic entries until after we know the
                // size of .rel(a).plt section.
                Elfwritedynent(ctxt, s, DT_DEBUG, 0L);
            }
            if (ctxt.BuildMode == BuildModeShared)
            { 
                // The go.link.abihashbytes symbol will be pointed at the appropriate
                // part of the .note.go.abihash section in data.go:func address().
                s = ctxt.Syms.Lookup("go.link.abihashbytes", 0L);
                s.Attr |= sym.AttrLocal;
                s.Type = sym.SRODATA;
                s.Attr |= sym.AttrSpecial;
                s.Attr |= sym.AttrReachable;
                s.Size = int64(sha1.Size);

                sort.Sort(byPkg(ctxt.Library));
                var h = sha1.New();
                foreach (var (_, l) in ctxt.Library)
                {
                    io.WriteString(h, l.Hash);
                }
                addgonote(ctxt, ".note.go.abihash", ELF_NOTE_GOABIHASH_TAG, h.Sum(new slice<byte>(new byte[] {  })));
                addgonote(ctxt, ".note.go.pkg-list", ELF_NOTE_GOPKGLIST_TAG, pkglistfornote);
                slice<@string> deplist = default;
                foreach (var (_, shlib) in ctxt.Shlibs)
                {
                    deplist = append(deplist, filepath.Base(shlib.Path));
                }
                addgonote(ctxt, ".note.go.deps", ELF_NOTE_GODEPS_TAG, (slice<byte>)strings.Join(deplist, "\n"));
            }
            if (ctxt.LinkMode == LinkExternal && flagBuildid != "".Value)
            {
                addgonote(ctxt, ".note.go.buildid", ELF_NOTE_GOBUILDID_TAG, (slice<byte>)flagBuildid.Value);
            }
        }

        // Do not write DT_NULL.  elfdynhash will finish it.
        private static void shsym(ref ElfShdr sh, ref sym.Symbol s)
        {
            var addr = Symaddr(s);
            if (sh.flags & SHF_ALLOC != 0L)
            {
                sh.addr = uint64(addr);
            }
            sh.off = uint64(datoff(s, addr));
            sh.size = uint64(s.Size);
        }

        private static void phsh(ref ElfPhdr ph, ref ElfShdr sh)
        {
            ph.vaddr = sh.addr;
            ph.paddr = ph.vaddr;
            ph.off = sh.off;
            ph.filesz = sh.size;
            ph.memsz = sh.size;
            ph.align = sh.addralign;
        }

        public static void Asmbelfsetup()
        { 
            /* This null SHdr must appear before all others */
            elfshname("");

            {
                var sect__prev1 = sect;

                foreach (var (_, __sect) in Segtext.Sections)
                {
                    sect = __sect; 
                    // There could be multiple .text sections. Instead check the Elfsect
                    // field to determine if already has an ElfShdr and if not, create one.
                    if (sect.Name == ".text")
                    {
                        if (sect.Elfsect == null)
                        {
                            sect.Elfsect = elfshnamedup(sect.Name);
                        }
                    }
                    else
                    {
                        elfshalloc(sect);
                    }
                }

                sect = sect__prev1;
            }

            {
                var sect__prev1 = sect;

                foreach (var (_, __sect) in Segrodata.Sections)
                {
                    sect = __sect;
                    elfshalloc(sect);
                }

                sect = sect__prev1;
            }

            {
                var sect__prev1 = sect;

                foreach (var (_, __sect) in Segrelrodata.Sections)
                {
                    sect = __sect;
                    elfshalloc(sect);
                }

                sect = sect__prev1;
            }

            {
                var sect__prev1 = sect;

                foreach (var (_, __sect) in Segdata.Sections)
                {
                    sect = __sect;
                    elfshalloc(sect);
                }

                sect = sect__prev1;
            }

            {
                var sect__prev1 = sect;

                foreach (var (_, __sect) in Segdwarf.Sections)
                {
                    sect = __sect;
                    elfshalloc(sect);
                }

                sect = sect__prev1;
            }

        }

        public static void Asmbelf(ref Link ctxt, long symo)
        {
            var eh = getElfEhdr();

            if (ctxt.Arch.Family == sys.MIPS || ctxt.Arch.Family == sys.MIPS64) 
                eh.machine = EM_MIPS;
            else if (ctxt.Arch.Family == sys.ARM) 
                eh.machine = EM_ARM;
            else if (ctxt.Arch.Family == sys.AMD64) 
                eh.machine = EM_X86_64;
            else if (ctxt.Arch.Family == sys.ARM64) 
                eh.machine = EM_AARCH64;
            else if (ctxt.Arch.Family == sys.I386) 
                eh.machine = EM_386;
            else if (ctxt.Arch.Family == sys.PPC64) 
                eh.machine = EM_PPC64;
            else if (ctxt.Arch.Family == sys.S390X) 
                eh.machine = EM_S390;
            else 
                Exitf("unknown architecture in asmbelf: %v", ctxt.Arch.Family);
                        var elfreserve = int64(ELFRESERVE);

            var numtext = int64(0L);
            {
                var sect__prev1 = sect;

                foreach (var (_, __sect) in Segtext.Sections)
                {
                    sect = __sect;
                    if (sect.Name == ".text")
                    {
                        numtext++;
                    }
                } 

                // If there are multiple text sections, extra space is needed
                // in the elfreserve for the additional .text and .rela.text
                // section headers.  It can handle 4 extra now. Headers are
                // 64 bytes.

                sect = sect__prev1;
            }

            if (numtext > 4L)
            {
                elfreserve += elfreserve + numtext * 64L * 2L;
            }
            var startva = FlagTextAddr - int64(HEADR).Value;
            var resoff = elfreserve;

            ref ElfPhdr pph = default;
            ref ElfPhdr pnote = default;
            if (ctxt.LinkMode == LinkExternal)
            { 
                /* skip program headers */
                eh.phoff = 0L;

                eh.phentsize = 0L;

                if (ctxt.BuildMode == BuildModeShared)
                {
                    var sh = elfshname(".note.go.pkg-list");
                    sh.type_ = SHT_NOTE;
                    sh = elfshname(".note.go.abihash");
                    sh.type_ = SHT_NOTE;
                    sh.flags = SHF_ALLOC;
                    sh = elfshname(".note.go.deps");
                    sh.type_ = SHT_NOTE;
                }
                if (flagBuildid != "".Value)
                {
                    sh = elfshname(".note.go.buildid");
                    sh.type_ = SHT_NOTE;
                    sh.flags = SHF_ALLOC;
                }
                goto elfobj;
            } 

            /* program header info */
            pph = newElfPhdr();

            pph.type_ = PT_PHDR;
            pph.flags = PF_R;
            pph.off = uint64(eh.ehsize);
            pph.vaddr = uint64(FlagTextAddr.Value) - uint64(HEADR) + pph.off;
            pph.paddr = uint64(FlagTextAddr.Value) - uint64(HEADR) + pph.off;
            pph.align = uint64(FlagRound.Value);

            /*
                 * PHDR must be in a loaded segment. Adjust the text
                 * segment boundaries downwards to include it.
                 * Except on NaCl where it must not be loaded.
                 */
            if (ctxt.HeadType != objabi.Hnacl)
            {
                var o = int64(Segtext.Vaddr - pph.vaddr);
                Segtext.Vaddr -= uint64(o);
                Segtext.Length += uint64(o);
                o = int64(Segtext.Fileoff - pph.off);
                Segtext.Fileoff -= uint64(o);
                Segtext.Filelen += uint64(o);
            }
            if (!FlagD.Value)
            {                /* -d suppresses dynamic loader format */
                /* interpreter */
                sh = elfshname(".interp");

                sh.type_ = SHT_PROGBITS;
                sh.flags = SHF_ALLOC;
                sh.addralign = 1L;
                if (interpreter == "")
                {

                    if (ctxt.HeadType == objabi.Hlinux) 
                        interpreter = Thearch.Linuxdynld;
                    else if (ctxt.HeadType == objabi.Hfreebsd) 
                        interpreter = Thearch.Freebsddynld;
                    else if (ctxt.HeadType == objabi.Hnetbsd) 
                        interpreter = Thearch.Netbsddynld;
                    else if (ctxt.HeadType == objabi.Hopenbsd) 
                        interpreter = Thearch.Openbsddynld;
                    else if (ctxt.HeadType == objabi.Hdragonfly) 
                        interpreter = Thearch.Dragonflydynld;
                    else if (ctxt.HeadType == objabi.Hsolaris) 
                        interpreter = Thearch.Solarisdynld;
                                    }
                resoff -= int64(elfinterp(sh, uint64(startva), uint64(resoff), interpreter));

                var ph = newElfPhdr();
                ph.type_ = PT_INTERP;
                ph.flags = PF_R;
                phsh(ph, sh);
            }
            pnote = null;
            if (ctxt.HeadType == objabi.Hnetbsd || ctxt.HeadType == objabi.Hopenbsd)
            {
                sh = default;

                if (ctxt.HeadType == objabi.Hnetbsd) 
                    sh = elfshname(".note.netbsd.ident");
                    resoff -= int64(elfnetbsdsig(sh, uint64(startva), uint64(resoff)));
                else if (ctxt.HeadType == objabi.Hopenbsd) 
                    sh = elfshname(".note.openbsd.ident");
                    resoff -= int64(elfopenbsdsig(sh, uint64(startva), uint64(resoff)));
                                pnote = newElfPhdr();
                pnote.type_ = PT_NOTE;
                pnote.flags = PF_R;
                phsh(pnote, sh);
            }
            if (len(buildinfo) > 0L)
            {
                sh = elfshname(".note.gnu.build-id");
                resoff -= int64(elfbuildinfo(sh, uint64(startva), uint64(resoff)));

                if (pnote == null)
                {
                    pnote = newElfPhdr();
                    pnote.type_ = PT_NOTE;
                    pnote.flags = PF_R;
                }
                phsh(pnote, sh);
            }
            if (flagBuildid != "".Value)
            {
                sh = elfshname(".note.go.buildid");
                resoff -= int64(elfgobuildid(sh, uint64(startva), uint64(resoff)));

                pnote = newElfPhdr();
                pnote.type_ = PT_NOTE;
                pnote.flags = PF_R;
                phsh(pnote, sh);
            } 

            // Additions to the reserved area must be above this line.
            elfphload(ref Segtext);
            if (len(Segrodata.Sections) > 0L)
            {
                elfphload(ref Segrodata);
            }
            if (len(Segrelrodata.Sections) > 0L)
            {
                elfphload(ref Segrelrodata);
                elfphrelro(ref Segrelrodata);
            }
            elfphload(ref Segdata); 

            /* Dynamic linking sections */
            if (!FlagD.Value)
            {
                sh = elfshname(".dynsym");
                sh.type_ = SHT_DYNSYM;
                sh.flags = SHF_ALLOC;
                if (elf64)
                {
                    sh.entsize = ELF64SYMSIZE;
                }
                else
                {
                    sh.entsize = ELF32SYMSIZE;
                }
                sh.addralign = uint64(ctxt.Arch.RegSize);
                sh.link = uint32(elfshname(".dynstr").shnum); 

                // sh->info = index of first non-local symbol (number of local symbols)
                shsym(sh, ctxt.Syms.Lookup(".dynsym", 0L));

                sh = elfshname(".dynstr");
                sh.type_ = SHT_STRTAB;
                sh.flags = SHF_ALLOC;
                sh.addralign = 1L;
                shsym(sh, ctxt.Syms.Lookup(".dynstr", 0L));

                if (elfverneed != 0L)
                {
                    sh = elfshname(".gnu.version");
                    sh.type_ = SHT_GNU_VERSYM;
                    sh.flags = SHF_ALLOC;
                    sh.addralign = 2L;
                    sh.link = uint32(elfshname(".dynsym").shnum);
                    sh.entsize = 2L;
                    shsym(sh, ctxt.Syms.Lookup(".gnu.version", 0L));

                    sh = elfshname(".gnu.version_r");
                    sh.type_ = SHT_GNU_VERNEED;
                    sh.flags = SHF_ALLOC;
                    sh.addralign = uint64(ctxt.Arch.RegSize);
                    sh.info = uint32(elfverneed);
                    sh.link = uint32(elfshname(".dynstr").shnum);
                    shsym(sh, ctxt.Syms.Lookup(".gnu.version_r", 0L));
                }
                if (elfRelType == ".rela")
                {
                    sh = elfshname(".rela.plt");
                    sh.type_ = SHT_RELA;
                    sh.flags = SHF_ALLOC;
                    sh.entsize = ELF64RELASIZE;
                    sh.addralign = uint64(ctxt.Arch.RegSize);
                    sh.link = uint32(elfshname(".dynsym").shnum);
                    sh.info = uint32(elfshname(".plt").shnum);
                    shsym(sh, ctxt.Syms.Lookup(".rela.plt", 0L));

                    sh = elfshname(".rela");
                    sh.type_ = SHT_RELA;
                    sh.flags = SHF_ALLOC;
                    sh.entsize = ELF64RELASIZE;
                    sh.addralign = 8L;
                    sh.link = uint32(elfshname(".dynsym").shnum);
                    shsym(sh, ctxt.Syms.Lookup(".rela", 0L));
                }
                else
                {
                    sh = elfshname(".rel.plt");
                    sh.type_ = SHT_REL;
                    sh.flags = SHF_ALLOC;
                    sh.entsize = ELF32RELSIZE;
                    sh.addralign = 4L;
                    sh.link = uint32(elfshname(".dynsym").shnum);
                    shsym(sh, ctxt.Syms.Lookup(".rel.plt", 0L));

                    sh = elfshname(".rel");
                    sh.type_ = SHT_REL;
                    sh.flags = SHF_ALLOC;
                    sh.entsize = ELF32RELSIZE;
                    sh.addralign = 4L;
                    sh.link = uint32(elfshname(".dynsym").shnum);
                    shsym(sh, ctxt.Syms.Lookup(".rel", 0L));
                }
                if (eh.machine == EM_PPC64)
                {
                    sh = elfshname(".glink");
                    sh.type_ = SHT_PROGBITS;
                    sh.flags = SHF_ALLOC + SHF_EXECINSTR;
                    sh.addralign = 4L;
                    shsym(sh, ctxt.Syms.Lookup(".glink", 0L));
                }
                sh = elfshname(".plt");
                sh.type_ = SHT_PROGBITS;
                sh.flags = SHF_ALLOC + SHF_EXECINSTR;
                if (eh.machine == EM_X86_64)
                {
                    sh.entsize = 16L;
                }
                else if (eh.machine == EM_S390)
                {
                    sh.entsize = 32L;
                }
                else if (eh.machine == EM_PPC64)
                { 
                    // On ppc64, this is just a table of addresses
                    // filled by the dynamic linker
                    sh.type_ = SHT_NOBITS;

                    sh.flags = SHF_ALLOC + SHF_WRITE;
                    sh.entsize = 8L;
                }
                else
                {
                    sh.entsize = 4L;
                }
                sh.addralign = sh.entsize;
                shsym(sh, ctxt.Syms.Lookup(".plt", 0L)); 

                // On ppc64, .got comes from the input files, so don't
                // create it here, and .got.plt is not used.
                if (eh.machine != EM_PPC64)
                {
                    sh = elfshname(".got");
                    sh.type_ = SHT_PROGBITS;
                    sh.flags = SHF_ALLOC + SHF_WRITE;
                    sh.entsize = uint64(ctxt.Arch.RegSize);
                    sh.addralign = uint64(ctxt.Arch.RegSize);
                    shsym(sh, ctxt.Syms.Lookup(".got", 0L));

                    sh = elfshname(".got.plt");
                    sh.type_ = SHT_PROGBITS;
                    sh.flags = SHF_ALLOC + SHF_WRITE;
                    sh.entsize = uint64(ctxt.Arch.RegSize);
                    sh.addralign = uint64(ctxt.Arch.RegSize);
                    shsym(sh, ctxt.Syms.Lookup(".got.plt", 0L));
                }
                sh = elfshname(".hash");
                sh.type_ = SHT_HASH;
                sh.flags = SHF_ALLOC;
                sh.entsize = 4L;
                sh.addralign = uint64(ctxt.Arch.RegSize);
                sh.link = uint32(elfshname(".dynsym").shnum);
                shsym(sh, ctxt.Syms.Lookup(".hash", 0L)); 

                /* sh and PT_DYNAMIC for .dynamic section */
                sh = elfshname(".dynamic");

                sh.type_ = SHT_DYNAMIC;
                sh.flags = SHF_ALLOC + SHF_WRITE;
                sh.entsize = 2L * uint64(ctxt.Arch.RegSize);
                sh.addralign = uint64(ctxt.Arch.RegSize);
                sh.link = uint32(elfshname(".dynstr").shnum);
                shsym(sh, ctxt.Syms.Lookup(".dynamic", 0L));
                ph = newElfPhdr();
                ph.type_ = PT_DYNAMIC;
                ph.flags = PF_R + PF_W;
                phsh(ph, sh);

                /*
                         * Thread-local storage segment (really just size).
                         */
                var tlssize = uint64(0L);
                {
                    var sect__prev1 = sect;

                    foreach (var (_, __sect) in Segdata.Sections)
                    {
                        sect = __sect;
                        if (sect.Name == ".tbss")
                        {
                            tlssize = sect.Length;
                        }
                    }

                    sect = sect__prev1;
                }

                if (tlssize != 0L)
                {
                    ph = newElfPhdr();
                    ph.type_ = PT_TLS;
                    ph.flags = PF_R;
                    ph.memsz = tlssize;
                    ph.align = uint64(ctxt.Arch.RegSize);
                }
            }
            if (ctxt.HeadType == objabi.Hlinux)
            {
                ph = newElfPhdr();
                ph.type_ = PT_GNU_STACK;
                ph.flags = PF_W + PF_R;
                ph.align = uint64(ctxt.Arch.RegSize);

                ph = newElfPhdr();
                ph.type_ = PT_PAX_FLAGS;
                ph.flags = 0x2a00UL; // mprotect, randexec, emutramp disabled
                ph.align = uint64(ctxt.Arch.RegSize);
            }
            else if (ctxt.HeadType == objabi.Hsolaris)
            {
                ph = newElfPhdr();
                ph.type_ = PT_SUNWSTACK;
                ph.flags = PF_W + PF_R;
            }
elfobj:
            sh = elfshname(".shstrtab");
            sh.type_ = SHT_STRTAB;
            sh.addralign = 1L;
            shsym(sh, ctxt.Syms.Lookup(".shstrtab", 0L));
            eh.shstrndx = uint16(sh.shnum); 

            // put these sections early in the list
            if (!FlagS.Value)
            {
                elfshname(".symtab");
                elfshname(".strtab");
            }
            {
                var sect__prev1 = sect;

                foreach (var (_, __sect) in Segtext.Sections)
                {
                    sect = __sect;
                    elfshbits(ctxt.LinkMode, sect);
                }

                sect = sect__prev1;
            }

            {
                var sect__prev1 = sect;

                foreach (var (_, __sect) in Segrodata.Sections)
                {
                    sect = __sect;
                    elfshbits(ctxt.LinkMode, sect);
                }

                sect = sect__prev1;
            }

            {
                var sect__prev1 = sect;

                foreach (var (_, __sect) in Segrelrodata.Sections)
                {
                    sect = __sect;
                    elfshbits(ctxt.LinkMode, sect);
                }

                sect = sect__prev1;
            }

            {
                var sect__prev1 = sect;

                foreach (var (_, __sect) in Segdata.Sections)
                {
                    sect = __sect;
                    elfshbits(ctxt.LinkMode, sect);
                }

                sect = sect__prev1;
            }

            {
                var sect__prev1 = sect;

                foreach (var (_, __sect) in Segdwarf.Sections)
                {
                    sect = __sect;
                    elfshbits(ctxt.LinkMode, sect);
                }

                sect = sect__prev1;
            }

            if (ctxt.LinkMode == LinkExternal)
            {
                {
                    var sect__prev1 = sect;

                    foreach (var (_, __sect) in Segtext.Sections)
                    {
                        sect = __sect;
                        elfshreloc(ctxt.Arch, sect);
                    }

                    sect = sect__prev1;
                }

                {
                    var sect__prev1 = sect;

                    foreach (var (_, __sect) in Segrodata.Sections)
                    {
                        sect = __sect;
                        elfshreloc(ctxt.Arch, sect);
                    }

                    sect = sect__prev1;
                }

                {
                    var sect__prev1 = sect;

                    foreach (var (_, __sect) in Segrelrodata.Sections)
                    {
                        sect = __sect;
                        elfshreloc(ctxt.Arch, sect);
                    }

                    sect = sect__prev1;
                }

                {
                    var sect__prev1 = sect;

                    foreach (var (_, __sect) in Segdata.Sections)
                    {
                        sect = __sect;
                        elfshreloc(ctxt.Arch, sect);
                    }

                    sect = sect__prev1;
                }

                foreach (var (_, s) in dwarfp)
                {
                    if (len(s.R) > 0L || s.Type == sym.SDWARFINFO || s.Type == sym.SDWARFLOC)
                    {
                        elfshreloc(ctxt.Arch, s.Sect);
                    }
                } 
                // add a .note.GNU-stack section to mark the stack as non-executable
                sh = elfshname(".note.GNU-stack");

                sh.type_ = SHT_PROGBITS;
                sh.addralign = 1L;
                sh.flags = 0L;
            }
            if (!FlagS.Value)
            {
                sh = elfshname(".symtab");
                sh.type_ = SHT_SYMTAB;
                sh.off = uint64(symo);
                sh.size = uint64(Symsize);
                sh.addralign = uint64(ctxt.Arch.RegSize);
                sh.entsize = 8L + 2L * uint64(ctxt.Arch.RegSize);
                sh.link = uint32(elfshname(".strtab").shnum);
                sh.info = uint32(elfglobalsymndx);

                sh = elfshname(".strtab");
                sh.type_ = SHT_STRTAB;
                sh.off = uint64(symo) + uint64(Symsize);
                sh.size = uint64(len(Elfstrdat));
                sh.addralign = 1L;
            } 

            /* Main header */
            eh.ident[EI_MAG0] = '';

            eh.ident[EI_MAG1] = 'E';
            eh.ident[EI_MAG2] = 'L';
            eh.ident[EI_MAG3] = 'F';
            if (ctxt.HeadType == objabi.Hfreebsd)
            {
                eh.ident[EI_OSABI] = ELFOSABI_FREEBSD;
            }
            else if (ctxt.HeadType == objabi.Hnetbsd)
            {
                eh.ident[EI_OSABI] = ELFOSABI_NETBSD;
            }
            else if (ctxt.HeadType == objabi.Hopenbsd)
            {
                eh.ident[EI_OSABI] = ELFOSABI_OPENBSD;
            }
            else if (ctxt.HeadType == objabi.Hdragonfly)
            {
                eh.ident[EI_OSABI] = ELFOSABI_NONE;
            }
            if (elf64)
            {
                eh.ident[EI_CLASS] = ELFCLASS64;
            }
            else
            {
                eh.ident[EI_CLASS] = ELFCLASS32;
            }
            if (ctxt.Arch.ByteOrder == binary.BigEndian)
            {
                eh.ident[EI_DATA] = ELFDATA2MSB;
            }
            else
            {
                eh.ident[EI_DATA] = ELFDATA2LSB;
            }
            eh.ident[EI_VERSION] = EV_CURRENT;

            if (ctxt.LinkMode == LinkExternal)
            {
                eh.type_ = ET_REL;
            }
            else if (ctxt.BuildMode == BuildModePIE)
            {
                eh.type_ = ET_DYN;
            }
            else
            {
                eh.type_ = ET_EXEC;
            }
            if (ctxt.LinkMode != LinkExternal)
            {
                eh.entry = uint64(Entryvalue(ctxt));
            }
            eh.version = EV_CURRENT;

            if (pph != null)
            {
                pph.filesz = uint64(eh.phnum) * uint64(eh.phentsize);
                pph.memsz = pph.filesz;
            }
            ctxt.Out.SeekSet(0L);
            var a = int64(0L);
            a += int64(elfwritehdr(ctxt.Out));
            a += int64(elfwritephdrs(ctxt.Out));
            a += int64(elfwriteshdrs(ctxt.Out));
            if (!FlagD.Value)
            {
                a += int64(elfwriteinterp(ctxt.Out));
            }
            if (ctxt.LinkMode != LinkExternal)
            {
                if (ctxt.HeadType == objabi.Hnetbsd)
                {
                    a += int64(elfwritenetbsdsig(ctxt.Out));
                }
                if (ctxt.HeadType == objabi.Hopenbsd)
                {
                    a += int64(elfwriteopenbsdsig(ctxt.Out));
                }
                if (len(buildinfo) > 0L)
                {
                    a += int64(elfwritebuildinfo(ctxt.Out));
                }
                if (flagBuildid != "".Value)
                {
                    a += int64(elfwritegobuildid(ctxt.Out));
                }
            }
            if (a > elfreserve)
            {
                Errorf(null, "ELFRESERVE too small: %d > %d with %d text sections", a, elfreserve, numtext);
            }
        }

        private static void elfadddynsym(ref Link ctxt, ref sym.Symbol s)
        {
            if (elf64)
            {
                s.Dynid = int32(Nelfsym);
                Nelfsym++;

                var d = ctxt.Syms.Lookup(".dynsym", 0L);

                var name = s.Extname;
                d.AddUint32(ctxt.Arch, uint32(Addstring(ctxt.Syms.Lookup(".dynstr", 0L), name))); 

                /* type */
                var t = STB_GLOBAL << (int)(4L);

                if (s.Attr.CgoExport() && s.Type == sym.STEXT)
                {
                    t |= STT_FUNC;
                }
                else
                {
                    t |= STT_OBJECT;
                }
                d.AddUint8(uint8(t)); 

                /* reserved */
                d.AddUint8(0L); 

                /* section where symbol is defined */
                if (s.Type == sym.SDYNIMPORT)
                {
                    d.AddUint16(ctxt.Arch, SHN_UNDEF);
                }
                else
                {
                    d.AddUint16(ctxt.Arch, 1L);
                } 

                /* value */
                if (s.Type == sym.SDYNIMPORT)
                {
                    d.AddUint64(ctxt.Arch, 0L);
                }
                else
                {
                    d.AddAddr(ctxt.Arch, s);
                } 

                /* size of object */
                d.AddUint64(ctxt.Arch, uint64(s.Size));

                if (ctxt.Arch.Family == sys.AMD64 && !s.Attr.CgoExportDynamic() && s.Dynimplib != "" && !seenlib[s.Dynimplib])
                {
                    Elfwritedynent(ctxt, ctxt.Syms.Lookup(".dynamic", 0L), DT_NEEDED, uint64(Addstring(ctxt.Syms.Lookup(".dynstr", 0L), s.Dynimplib)));
                }
            }
            else
            {
                s.Dynid = int32(Nelfsym);
                Nelfsym++;

                d = ctxt.Syms.Lookup(".dynsym", 0L); 

                /* name */
                name = s.Extname;

                d.AddUint32(ctxt.Arch, uint32(Addstring(ctxt.Syms.Lookup(".dynstr", 0L), name))); 

                /* value */
                if (s.Type == sym.SDYNIMPORT)
                {
                    d.AddUint32(ctxt.Arch, 0L);
                }
                else
                {
                    d.AddAddr(ctxt.Arch, s);
                } 

                /* size of object */
                d.AddUint32(ctxt.Arch, uint32(s.Size)); 

                /* type */
                t = STB_GLOBAL << (int)(4L); 

                // TODO(mwhudson): presumably the behavior should actually be the same on both arm and 386.
                if (ctxt.Arch.Family == sys.I386 && s.Attr.CgoExport() && s.Type == sym.STEXT)
                {
                    t |= STT_FUNC;
                }
                else if (ctxt.Arch.Family == sys.ARM && s.Attr.CgoExportDynamic() && s.Type == sym.STEXT)
                {
                    t |= STT_FUNC;
                }
                else
                {
                    t |= STT_OBJECT;
                }
                d.AddUint8(uint8(t));
                d.AddUint8(0L); 

                /* shndx */
                if (s.Type == sym.SDYNIMPORT)
                {
                    d.AddUint16(ctxt.Arch, SHN_UNDEF);
                }
                else
                {
                    d.AddUint16(ctxt.Arch, 1L);
                }
            }
        }

        public static uint ELF32_R_SYM(uint info)
        {
            return info >> (int)(8L);
        }

        public static uint ELF32_R_TYPE(uint info)
        {
            return uint32(uint8(info));
        }

        public static uint ELF32_R_INFO(uint sym, uint type_)
        {
            return sym << (int)(8L) | type_;
        }

        public static byte ELF32_ST_BIND(byte info)
        {
            return info >> (int)(4L);
        }

        public static byte ELF32_ST_TYPE(byte info)
        {
            return info & 0xfUL;
        }

        public static byte ELF32_ST_INFO(byte bind, byte type_)
        {
            return bind << (int)(4L) | type_ & 0xfUL;
        }

        public static byte ELF32_ST_VISIBILITY(byte oth)
        {
            return oth & 3L;
        }

        public static uint ELF64_R_SYM(ulong info)
        {
            return uint32(info >> (int)(32L));
        }

        public static uint ELF64_R_TYPE(ulong info)
        {
            return uint32(info);
        }

        public static ulong ELF64_R_INFO(uint sym, uint type_)
        {
            return uint64(sym) << (int)(32L) | uint64(type_);
        }

        public static byte ELF64_ST_BIND(byte info)
        {
            return info >> (int)(4L);
        }

        public static byte ELF64_ST_TYPE(byte info)
        {
            return info & 0xfUL;
        }

        public static byte ELF64_ST_INFO(byte bind, byte type_)
        {
            return bind << (int)(4L) | type_ & 0xfUL;
        }

        public static byte ELF64_ST_VISIBILITY(byte oth)
        {
            return oth & 3L;
        }
    }
}}}}
