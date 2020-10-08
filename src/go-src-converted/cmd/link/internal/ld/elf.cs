// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ld -- go2cs converted at 2020 October 08 04:38:41 UTC
// import "cmd/link/internal/ld" ==> using ld = go.cmd.link.@internal.ld_package
// Original source: C:\Go\src\cmd\link\internal\ld\elf.go
using objabi = go.cmd.@internal.objabi_package;
using sys = go.cmd.@internal.sys_package;
using loader = go.cmd.link.@internal.loader_package;
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

        public static readonly long EI_MAG0 = (long)0L;
        public static readonly long EI_MAG1 = (long)1L;
        public static readonly long EI_MAG2 = (long)2L;
        public static readonly long EI_MAG3 = (long)3L;
        public static readonly long EI_CLASS = (long)4L;
        public static readonly long EI_DATA = (long)5L;
        public static readonly long EI_VERSION = (long)6L;
        public static readonly long EI_OSABI = (long)7L;
        public static readonly long EI_ABIVERSION = (long)8L;
        public static readonly long OLD_EI_BRAND = (long)8L;
        public static readonly long EI_PAD = (long)9L;
        public static readonly long EI_NIDENT = (long)16L;
        public static readonly ulong ELFMAG0 = (ulong)0x7fUL;
        public static readonly char ELFMAG1 = (char)'E';
        public static readonly char ELFMAG2 = (char)'L';
        public static readonly char ELFMAG3 = (char)'F';
        public static readonly long SELFMAG = (long)4L;
        public static readonly long EV_NONE = (long)0L;
        public static readonly long EV_CURRENT = (long)1L;
        public static readonly long ELFCLASSNONE = (long)0L;
        public static readonly long ELFCLASS32 = (long)1L;
        public static readonly long ELFCLASS64 = (long)2L;
        public static readonly long ELFDATANONE = (long)0L;
        public static readonly long ELFDATA2LSB = (long)1L;
        public static readonly long ELFDATA2MSB = (long)2L;
        public static readonly long ELFOSABI_NONE = (long)0L;
        public static readonly long ELFOSABI_HPUX = (long)1L;
        public static readonly long ELFOSABI_NETBSD = (long)2L;
        public static readonly long ELFOSABI_LINUX = (long)3L;
        public static readonly long ELFOSABI_HURD = (long)4L;
        public static readonly long ELFOSABI_86OPEN = (long)5L;
        public static readonly long ELFOSABI_SOLARIS = (long)6L;
        public static readonly long ELFOSABI_AIX = (long)7L;
        public static readonly long ELFOSABI_IRIX = (long)8L;
        public static readonly long ELFOSABI_FREEBSD = (long)9L;
        public static readonly long ELFOSABI_TRU64 = (long)10L;
        public static readonly long ELFOSABI_MODESTO = (long)11L;
        public static readonly long ELFOSABI_OPENBSD = (long)12L;
        public static readonly long ELFOSABI_OPENVMS = (long)13L;
        public static readonly long ELFOSABI_NSK = (long)14L;
        public static readonly long ELFOSABI_ARM = (long)97L;
        public static readonly long ELFOSABI_STANDALONE = (long)255L;
        public static readonly var ELFOSABI_SYSV = (var)ELFOSABI_NONE;
        public static readonly var ELFOSABI_MONTEREY = (var)ELFOSABI_AIX;
        public static readonly long ET_NONE = (long)0L;
        public static readonly long ET_REL = (long)1L;
        public static readonly long ET_EXEC = (long)2L;
        public static readonly long ET_DYN = (long)3L;
        public static readonly long ET_CORE = (long)4L;
        public static readonly ulong ET_LOOS = (ulong)0xfe00UL;
        public static readonly ulong ET_HIOS = (ulong)0xfeffUL;
        public static readonly ulong ET_LOPROC = (ulong)0xff00UL;
        public static readonly ulong ET_HIPROC = (ulong)0xffffUL;
        public static readonly long EM_NONE = (long)0L;
        public static readonly long EM_M32 = (long)1L;
        public static readonly long EM_SPARC = (long)2L;
        public static readonly long EM_386 = (long)3L;
        public static readonly long EM_68K = (long)4L;
        public static readonly long EM_88K = (long)5L;
        public static readonly long EM_860 = (long)7L;
        public static readonly long EM_MIPS = (long)8L;
        public static readonly long EM_S370 = (long)9L;
        public static readonly long EM_MIPS_RS3_LE = (long)10L;
        public static readonly long EM_PARISC = (long)15L;
        public static readonly long EM_VPP500 = (long)17L;
        public static readonly long EM_SPARC32PLUS = (long)18L;
        public static readonly long EM_960 = (long)19L;
        public static readonly long EM_PPC = (long)20L;
        public static readonly long EM_PPC64 = (long)21L;
        public static readonly long EM_S390 = (long)22L;
        public static readonly long EM_V800 = (long)36L;
        public static readonly long EM_FR20 = (long)37L;
        public static readonly long EM_RH32 = (long)38L;
        public static readonly long EM_RCE = (long)39L;
        public static readonly long EM_ARM = (long)40L;
        public static readonly long EM_SH = (long)42L;
        public static readonly long EM_SPARCV9 = (long)43L;
        public static readonly long EM_TRICORE = (long)44L;
        public static readonly long EM_ARC = (long)45L;
        public static readonly long EM_H8_300 = (long)46L;
        public static readonly long EM_H8_300H = (long)47L;
        public static readonly long EM_H8S = (long)48L;
        public static readonly long EM_H8_500 = (long)49L;
        public static readonly long EM_IA_64 = (long)50L;
        public static readonly long EM_MIPS_X = (long)51L;
        public static readonly long EM_COLDFIRE = (long)52L;
        public static readonly long EM_68HC12 = (long)53L;
        public static readonly long EM_MMA = (long)54L;
        public static readonly long EM_PCP = (long)55L;
        public static readonly long EM_NCPU = (long)56L;
        public static readonly long EM_NDR1 = (long)57L;
        public static readonly long EM_STARCORE = (long)58L;
        public static readonly long EM_ME16 = (long)59L;
        public static readonly long EM_ST100 = (long)60L;
        public static readonly long EM_TINYJ = (long)61L;
        public static readonly long EM_X86_64 = (long)62L;
        public static readonly long EM_AARCH64 = (long)183L;
        public static readonly long EM_486 = (long)6L;
        public static readonly long EM_MIPS_RS4_BE = (long)10L;
        public static readonly long EM_ALPHA_STD = (long)41L;
        public static readonly ulong EM_ALPHA = (ulong)0x9026UL;
        public static readonly long EM_RISCV = (long)243L;
        public static readonly long SHN_UNDEF = (long)0L;
        public static readonly ulong SHN_LORESERVE = (ulong)0xff00UL;
        public static readonly ulong SHN_LOPROC = (ulong)0xff00UL;
        public static readonly ulong SHN_HIPROC = (ulong)0xff1fUL;
        public static readonly ulong SHN_LOOS = (ulong)0xff20UL;
        public static readonly ulong SHN_HIOS = (ulong)0xff3fUL;
        public static readonly ulong SHN_ABS = (ulong)0xfff1UL;
        public static readonly ulong SHN_COMMON = (ulong)0xfff2UL;
        public static readonly ulong SHN_XINDEX = (ulong)0xffffUL;
        public static readonly ulong SHN_HIRESERVE = (ulong)0xffffUL;
        public static readonly long SHT_NULL = (long)0L;
        public static readonly long SHT_PROGBITS = (long)1L;
        public static readonly long SHT_SYMTAB = (long)2L;
        public static readonly long SHT_STRTAB = (long)3L;
        public static readonly long SHT_RELA = (long)4L;
        public static readonly long SHT_HASH = (long)5L;
        public static readonly long SHT_DYNAMIC = (long)6L;
        public static readonly long SHT_NOTE = (long)7L;
        public static readonly long SHT_NOBITS = (long)8L;
        public static readonly long SHT_REL = (long)9L;
        public static readonly long SHT_SHLIB = (long)10L;
        public static readonly long SHT_DYNSYM = (long)11L;
        public static readonly long SHT_INIT_ARRAY = (long)14L;
        public static readonly long SHT_FINI_ARRAY = (long)15L;
        public static readonly long SHT_PREINIT_ARRAY = (long)16L;
        public static readonly long SHT_GROUP = (long)17L;
        public static readonly long SHT_SYMTAB_SHNDX = (long)18L;
        public static readonly ulong SHT_LOOS = (ulong)0x60000000UL;
        public static readonly ulong SHT_HIOS = (ulong)0x6fffffffUL;
        public static readonly ulong SHT_GNU_VERDEF = (ulong)0x6ffffffdUL;
        public static readonly ulong SHT_GNU_VERNEED = (ulong)0x6ffffffeUL;
        public static readonly ulong SHT_GNU_VERSYM = (ulong)0x6fffffffUL;
        public static readonly ulong SHT_LOPROC = (ulong)0x70000000UL;
        public static readonly ulong SHT_ARM_ATTRIBUTES = (ulong)0x70000003UL;
        public static readonly ulong SHT_HIPROC = (ulong)0x7fffffffUL;
        public static readonly ulong SHT_LOUSER = (ulong)0x80000000UL;
        public static readonly ulong SHT_HIUSER = (ulong)0xffffffffUL;
        public static readonly ulong SHF_WRITE = (ulong)0x1UL;
        public static readonly ulong SHF_ALLOC = (ulong)0x2UL;
        public static readonly ulong SHF_EXECINSTR = (ulong)0x4UL;
        public static readonly ulong SHF_MERGE = (ulong)0x10UL;
        public static readonly ulong SHF_STRINGS = (ulong)0x20UL;
        public static readonly ulong SHF_INFO_LINK = (ulong)0x40UL;
        public static readonly ulong SHF_LINK_ORDER = (ulong)0x80UL;
        public static readonly ulong SHF_OS_NONCONFORMING = (ulong)0x100UL;
        public static readonly ulong SHF_GROUP = (ulong)0x200UL;
        public static readonly ulong SHF_TLS = (ulong)0x400UL;
        public static readonly ulong SHF_MASKOS = (ulong)0x0ff00000UL;
        public static readonly ulong SHF_MASKPROC = (ulong)0xf0000000UL;
        public static readonly long PT_NULL = (long)0L;
        public static readonly long PT_LOAD = (long)1L;
        public static readonly long PT_DYNAMIC = (long)2L;
        public static readonly long PT_INTERP = (long)3L;
        public static readonly long PT_NOTE = (long)4L;
        public static readonly long PT_SHLIB = (long)5L;
        public static readonly long PT_PHDR = (long)6L;
        public static readonly long PT_TLS = (long)7L;
        public static readonly ulong PT_LOOS = (ulong)0x60000000UL;
        public static readonly ulong PT_HIOS = (ulong)0x6fffffffUL;
        public static readonly ulong PT_LOPROC = (ulong)0x70000000UL;
        public static readonly ulong PT_HIPROC = (ulong)0x7fffffffUL;
        public static readonly ulong PT_GNU_STACK = (ulong)0x6474e551UL;
        public static readonly ulong PT_GNU_RELRO = (ulong)0x6474e552UL;
        public static readonly ulong PT_PAX_FLAGS = (ulong)0x65041580UL;
        public static readonly ulong PT_SUNWSTACK = (ulong)0x6ffffffbUL;
        public static readonly ulong PF_X = (ulong)0x1UL;
        public static readonly ulong PF_W = (ulong)0x2UL;
        public static readonly ulong PF_R = (ulong)0x4UL;
        public static readonly ulong PF_MASKOS = (ulong)0x0ff00000UL;
        public static readonly ulong PF_MASKPROC = (ulong)0xf0000000UL;
        public static readonly long DT_NULL = (long)0L;
        public static readonly long DT_NEEDED = (long)1L;
        public static readonly long DT_PLTRELSZ = (long)2L;
        public static readonly long DT_PLTGOT = (long)3L;
        public static readonly long DT_HASH = (long)4L;
        public static readonly long DT_STRTAB = (long)5L;
        public static readonly long DT_SYMTAB = (long)6L;
        public static readonly long DT_RELA = (long)7L;
        public static readonly long DT_RELASZ = (long)8L;
        public static readonly long DT_RELAENT = (long)9L;
        public static readonly long DT_STRSZ = (long)10L;
        public static readonly long DT_SYMENT = (long)11L;
        public static readonly long DT_INIT = (long)12L;
        public static readonly long DT_FINI = (long)13L;
        public static readonly long DT_SONAME = (long)14L;
        public static readonly long DT_RPATH = (long)15L;
        public static readonly long DT_SYMBOLIC = (long)16L;
        public static readonly long DT_REL = (long)17L;
        public static readonly long DT_RELSZ = (long)18L;
        public static readonly long DT_RELENT = (long)19L;
        public static readonly long DT_PLTREL = (long)20L;
        public static readonly long DT_DEBUG = (long)21L;
        public static readonly long DT_TEXTREL = (long)22L;
        public static readonly long DT_JMPREL = (long)23L;
        public static readonly long DT_BIND_NOW = (long)24L;
        public static readonly long DT_INIT_ARRAY = (long)25L;
        public static readonly long DT_FINI_ARRAY = (long)26L;
        public static readonly long DT_INIT_ARRAYSZ = (long)27L;
        public static readonly long DT_FINI_ARRAYSZ = (long)28L;
        public static readonly long DT_RUNPATH = (long)29L;
        public static readonly long DT_FLAGS = (long)30L;
        public static readonly long DT_ENCODING = (long)32L;
        public static readonly long DT_PREINIT_ARRAY = (long)32L;
        public static readonly long DT_PREINIT_ARRAYSZ = (long)33L;
        public static readonly ulong DT_LOOS = (ulong)0x6000000dUL;
        public static readonly ulong DT_HIOS = (ulong)0x6ffff000UL;
        public static readonly ulong DT_LOPROC = (ulong)0x70000000UL;
        public static readonly ulong DT_HIPROC = (ulong)0x7fffffffUL;
        public static readonly ulong DT_VERNEED = (ulong)0x6ffffffeUL;
        public static readonly ulong DT_VERNEEDNUM = (ulong)0x6fffffffUL;
        public static readonly ulong DT_VERSYM = (ulong)0x6ffffff0UL;
        public static readonly var DT_PPC64_GLINK = (var)DT_LOPROC + 0L;
        public static readonly var DT_PPC64_OPT = (var)DT_LOPROC + 3L;
        public static readonly ulong DF_ORIGIN = (ulong)0x0001UL;
        public static readonly ulong DF_SYMBOLIC = (ulong)0x0002UL;
        public static readonly ulong DF_TEXTREL = (ulong)0x0004UL;
        public static readonly ulong DF_BIND_NOW = (ulong)0x0008UL;
        public static readonly ulong DF_STATIC_TLS = (ulong)0x0010UL;
        public static readonly long NT_PRSTATUS = (long)1L;
        public static readonly long NT_FPREGSET = (long)2L;
        public static readonly long NT_PRPSINFO = (long)3L;
        public static readonly long STB_LOCAL = (long)0L;
        public static readonly long STB_GLOBAL = (long)1L;
        public static readonly long STB_WEAK = (long)2L;
        public static readonly long STB_LOOS = (long)10L;
        public static readonly long STB_HIOS = (long)12L;
        public static readonly long STB_LOPROC = (long)13L;
        public static readonly long STB_HIPROC = (long)15L;
        public static readonly long STT_NOTYPE = (long)0L;
        public static readonly long STT_OBJECT = (long)1L;
        public static readonly long STT_FUNC = (long)2L;
        public static readonly long STT_SECTION = (long)3L;
        public static readonly long STT_FILE = (long)4L;
        public static readonly long STT_COMMON = (long)5L;
        public static readonly long STT_TLS = (long)6L;
        public static readonly long STT_LOOS = (long)10L;
        public static readonly long STT_HIOS = (long)12L;
        public static readonly long STT_LOPROC = (long)13L;
        public static readonly long STT_HIPROC = (long)15L;
        public static readonly ulong STV_DEFAULT = (ulong)0x0UL;
        public static readonly ulong STV_INTERNAL = (ulong)0x1UL;
        public static readonly ulong STV_HIDDEN = (ulong)0x2UL;
        public static readonly ulong STV_PROTECTED = (ulong)0x3UL;
        public static readonly long STN_UNDEF = (long)0L;


        /* For accessing the fields of r_info. */

        /* For constructing r_info from field values. */

        /*
         * Relocation types.
         */
        public static readonly ulong ARM_MAGIC_TRAMP_NUMBER = (ulong)0x5c000003UL;


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
        public static readonly long ELF64HDRSIZE = (long)64L;
        public static readonly long ELF64PHDRSIZE = (long)56L;
        public static readonly long ELF64SHDRSIZE = (long)64L;
        public static readonly long ELF64RELSIZE = (long)16L;
        public static readonly long ELF64RELASIZE = (long)24L;
        public static readonly long ELF64SYMSIZE = (long)24L;
        public static readonly long ELF32HDRSIZE = (long)52L;
        public static readonly long ELF32PHDRSIZE = (long)32L;
        public static readonly long ELF32SHDRSIZE = (long)40L;
        public static readonly long ELF32SYMSIZE = (long)16L;
        public static readonly long ELF32RELSIZE = (long)8L;


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
        public static readonly long ELFRESERVE = (long)4096L;


        /*
         * We use the 64-bit data structures on both 32- and 64-bit machines
         * in order to write the code just once.  The 64-bit data structure is
         * written in the 32-bit format on the 32-bit machines.
         */
        public static readonly long NSECT = (long)400L;


        public static long Nelfsym = 1L;        private static bool elf64 = default;        private static @string elfRelType = default;        private static ElfEhdr ehdr = default;        private static array<ptr<ElfPhdr>> phdr = new array<ptr<ElfPhdr>>(NSECT);        private static array<ptr<ElfShdr>> shdr = new array<ptr<ElfShdr>>(NSECT);        private static @string interp = default;

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
        public static void Elfinit(ptr<Link> _addr_ctxt)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            ctxt.IsELF = true;

            if (ctxt.Arch.InFamily(sys.AMD64, sys.ARM64, sys.MIPS64, sys.PPC64, sys.RISCV64, sys.S390X))
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
            if (fallthrough || ctxt.Arch.Family == sys.AMD64 || ctxt.Arch.Family == sys.ARM64 || ctxt.Arch.Family == sys.MIPS64 || ctxt.Arch.Family == sys.RISCV64)
            {
                if (ctxt.Arch.Family == sys.MIPS64)
                {
                    ehdr.flags = 0x20000004UL; /* MIPS 3 CPIC */
                }

                elf64 = true;

                ehdr.phoff = ELF64HDRSIZE; /* Must be ELF64HDRSIZE: first PHdr must follow ELF header */
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
                /* Must be ELF32HDRSIZE: first PHdr must follow ELF header */
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
        private static void fixElfPhdr(ptr<ElfPhdr> _addr_e)
        {
            ref ElfPhdr e = ref _addr_e.val;

            var frag = int(e.vaddr & (e.align - 1L));

            e.off -= uint64(frag);
            e.vaddr -= uint64(frag);
            e.paddr -= uint64(frag);
            e.filesz += uint64(frag);
            e.memsz += uint64(frag);
        }

        private static void elf64phdr(ptr<OutBuf> _addr_@out, ptr<ElfPhdr> _addr_e)
        {
            ref OutBuf @out = ref _addr_@out.val;
            ref ElfPhdr e = ref _addr_e.val;

            if (e.type_ == PT_LOAD)
            {
                fixElfPhdr(_addr_e);
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

        private static void elf32phdr(ptr<OutBuf> _addr_@out, ptr<ElfPhdr> _addr_e)
        {
            ref OutBuf @out = ref _addr_@out.val;
            ref ElfPhdr e = ref _addr_e.val;

            if (e.type_ == PT_LOAD)
            {
                fixElfPhdr(_addr_e);
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

        private static void elf64shdr(ptr<OutBuf> _addr_@out, ptr<ElfShdr> _addr_e)
        {
            ref OutBuf @out = ref _addr_@out.val;
            ref ElfShdr e = ref _addr_e.val;

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

        private static void elf32shdr(ptr<OutBuf> _addr_@out, ptr<ElfShdr> _addr_e)
        {
            ref OutBuf @out = ref _addr_@out.val;
            ref ElfShdr e = ref _addr_e.val;

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

        private static uint elfwriteshdrs(ptr<OutBuf> _addr_@out)
        {
            ref OutBuf @out = ref _addr_@out.val;

            if (elf64)
            {
                {
                    long i__prev1 = i;

                    for (long i = 0L; i < int(ehdr.shnum); i++)
                    {
                        elf64shdr(_addr_out, _addr_shdr[i]);
                    }


                    i = i__prev1;
                }
                return uint32(ehdr.shnum) * ELF64SHDRSIZE;

            }

            {
                long i__prev1 = i;

                for (i = 0L; i < int(ehdr.shnum); i++)
                {
                    elf32shdr(_addr_out, _addr_shdr[i]);
                }


                i = i__prev1;
            }
            return uint32(ehdr.shnum) * ELF32SHDRSIZE;

        }

        private static void elfsetstring2(ptr<Link> _addr_ctxt, loader.Sym s, @string str, long off)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            if (nelfstr >= len(elfstr))
            {
                ctxt.Errorf(s, "too many elf strings");
                errorexit();
            }

            elfstr[nelfstr].s = str;
            elfstr[nelfstr].off = off;
            nelfstr++;

        }

        private static uint elfwritephdrs(ptr<OutBuf> _addr_@out)
        {
            ref OutBuf @out = ref _addr_@out.val;

            if (elf64)
            {
                {
                    long i__prev1 = i;

                    for (long i = 0L; i < int(ehdr.phnum); i++)
                    {
                        elf64phdr(_addr_out, _addr_phdr[i]);
                    }


                    i = i__prev1;
                }
                return uint32(ehdr.phnum) * ELF64PHDRSIZE;

            }

            {
                long i__prev1 = i;

                for (i = 0L; i < int(ehdr.phnum); i++)
                {
                    elf32phdr(_addr_out, _addr_phdr[i]);
                }


                i = i__prev1;
            }
            return uint32(ehdr.phnum) * ELF32PHDRSIZE;

        }

        private static ptr<ElfPhdr> newElfPhdr()
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

            return _addr_e!;

        }

        private static ptr<ElfShdr> newElfShdr(long name)
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

            return _addr_e!;

        }

        private static ptr<ElfEhdr> getElfEhdr()
        {
            return _addr__addr_ehdr!;
        }

        private static uint elf64writehdr(ptr<OutBuf> _addr_@out)
        {
            ref OutBuf @out = ref _addr_@out.val;

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

        private static uint elf32writehdr(ptr<OutBuf> _addr_@out)
        {
            ref OutBuf @out = ref _addr_@out.val;

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

        private static uint elfwritehdr(ptr<OutBuf> _addr_@out)
        {
            ref OutBuf @out = ref _addr_@out.val;

            if (elf64)
            {
                return elf64writehdr(_addr_out);
            }

            return elf32writehdr(_addr_out);

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

        private static void elfWriteDynEnt(ptr<sys.Arch> _addr_arch, ptr<sym.Symbol> _addr_s, long tag, ulong val)
        {
            ref sys.Arch arch = ref _addr_arch.val;
            ref sym.Symbol s = ref _addr_s.val;

            if (elf64)
            {
                s.AddUint64(arch, uint64(tag));
                s.AddUint64(arch, val);
            }
            else
            {
                s.AddUint32(arch, uint32(tag));
                s.AddUint32(arch, uint32(val));
            }

        }

        private static void elfWriteDynEntSym2(ptr<Link> _addr_ctxt, ptr<loader.SymbolBuilder> _addr_s, long tag, loader.Sym t)
        {
            ref Link ctxt = ref _addr_ctxt.val;
            ref loader.SymbolBuilder s = ref _addr_s.val;

            Elfwritedynentsymplus2(_addr_ctxt, _addr_s, tag, t, 0L);
        }

        public static void Elfwritedynentsymplus(ptr<sys.Arch> _addr_arch, ptr<sym.Symbol> _addr_s, long tag, ptr<sym.Symbol> _addr_t, long add)
        {
            ref sys.Arch arch = ref _addr_arch.val;
            ref sym.Symbol s = ref _addr_s.val;
            ref sym.Symbol t = ref _addr_t.val;

            if (elf64)
            {
                s.AddUint64(arch, uint64(tag));
            }
            else
            {
                s.AddUint32(arch, uint32(tag));
            }

            s.AddAddrPlus(arch, t, add);

        }

        private static void elfWriteDynEntSymSize(ptr<sys.Arch> _addr_arch, ptr<sym.Symbol> _addr_s, long tag, ptr<sym.Symbol> _addr_t)
        {
            ref sys.Arch arch = ref _addr_arch.val;
            ref sym.Symbol s = ref _addr_s.val;
            ref sym.Symbol t = ref _addr_t.val;

            if (elf64)
            {
                s.AddUint64(arch, uint64(tag));
            }
            else
            {
                s.AddUint32(arch, uint32(tag));
            }

            s.AddSize(arch, t);

        }

        // temporary
        public static void Elfwritedynent2(ptr<sys.Arch> _addr_arch, ptr<loader.SymbolBuilder> _addr_s, long tag, ulong val)
        {
            ref sys.Arch arch = ref _addr_arch.val;
            ref loader.SymbolBuilder s = ref _addr_s.val;

            if (elf64)
            {
                s.AddUint64(arch, uint64(tag));
                s.AddUint64(arch, val);
            }
            else
            {
                s.AddUint32(arch, uint32(tag));
                s.AddUint32(arch, uint32(val));
            }

        }

        private static void elfwritedynentsym2(ptr<Link> _addr_ctxt, ptr<loader.SymbolBuilder> _addr_s, long tag, loader.Sym t)
        {
            ref Link ctxt = ref _addr_ctxt.val;
            ref loader.SymbolBuilder s = ref _addr_s.val;

            Elfwritedynentsymplus2(_addr_ctxt, _addr_s, tag, t, 0L);
        }

        public static void Elfwritedynentsymplus2(ptr<Link> _addr_ctxt, ptr<loader.SymbolBuilder> _addr_s, long tag, loader.Sym t, long add)
        {
            ref Link ctxt = ref _addr_ctxt.val;
            ref loader.SymbolBuilder s = ref _addr_s.val;

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

        private static void elfwritedynentsymsize2(ptr<Link> _addr_ctxt, ptr<loader.SymbolBuilder> _addr_s, long tag, loader.Sym t)
        {
            ref Link ctxt = ref _addr_ctxt.val;
            ref loader.SymbolBuilder s = ref _addr_s.val;

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

        private static long elfinterp(ptr<ElfShdr> _addr_sh, ulong startva, ulong resoff, @string p)
        {
            ref ElfShdr sh = ref _addr_sh.val;

            interp = p;
            var n = len(interp) + 1L;
            sh.addr = startva + resoff - uint64(n);
            sh.off = resoff - uint64(n);
            sh.size = uint64(n);

            return n;
        }

        private static long elfwriteinterp(ptr<OutBuf> _addr_@out)
        {
            ref OutBuf @out = ref _addr_@out.val;

            var sh = elfshname(".interp");
            @out.SeekSet(int64(sh.off));
            @out.WriteString(interp);
            @out.Write8(0L);
            return int(sh.size);
        }

        private static long elfnote(ptr<ElfShdr> _addr_sh, ulong startva, ulong resoff, long sz)
        {
            ref ElfShdr sh = ref _addr_sh.val;

            long n = 3L * 4L + uint64(sz) + resoff % 4L;

            sh.type_ = SHT_NOTE;
            sh.flags = SHF_ALLOC;
            sh.addralign = 4L;
            sh.addr = startva + resoff - n;
            sh.off = resoff - n;
            sh.size = n - resoff % 4L;

            return int(n);
        }

        private static ptr<ElfShdr> elfwritenotehdr(ptr<OutBuf> _addr_@out, @string str, uint namesz, uint descsz, uint tag)
        {
            ref OutBuf @out = ref _addr_@out.val;

            var sh = elfshname(str); 

            // Write Elf_Note header.
            @out.SeekSet(int64(sh.off));

            @out.Write32(namesz);
            @out.Write32(descsz);
            @out.Write32(tag);

            return _addr_sh!;

        }

        // NetBSD Signature (as per sys/exec_elf.h)
        public static readonly long ELF_NOTE_NETBSD_NAMESZ = (long)7L;
        public static readonly long ELF_NOTE_NETBSD_DESCSZ = (long)4L;
        public static readonly long ELF_NOTE_NETBSD_TAG = (long)1L;
        public static readonly long ELF_NOTE_NETBSD_VERSION = (long)700000000L; /* NetBSD 7.0 */

        public static slice<byte> ELF_NOTE_NETBSD_NAME = (slice<byte>)"NetBSD\x00";

        private static long elfnetbsdsig(ptr<ElfShdr> _addr_sh, ulong startva, ulong resoff)
        {
            ref ElfShdr sh = ref _addr_sh.val;

            var n = int(Rnd(ELF_NOTE_NETBSD_NAMESZ, 4L) + Rnd(ELF_NOTE_NETBSD_DESCSZ, 4L));
            return elfnote(_addr_sh, startva, resoff, n);
        }

        private static long elfwritenetbsdsig(ptr<OutBuf> _addr_@out)
        {
            ref OutBuf @out = ref _addr_@out.val;
 
            // Write Elf_Note header.
            var sh = elfwritenotehdr(_addr_out, ".note.netbsd.ident", ELF_NOTE_NETBSD_NAMESZ, ELF_NOTE_NETBSD_DESCSZ, ELF_NOTE_NETBSD_TAG);

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

        // The race detector can't handle ASLR (address space layout randomization).
        // ASLR is on by default for NetBSD, so we turn the ASLR off explicitly
        // using a magic elf Note when building race binaries.

        private static long elfnetbsdpax(ptr<ElfShdr> _addr_sh, ulong startva, ulong resoff)
        {
            ref ElfShdr sh = ref _addr_sh.val;

            var n = int(Rnd(4L, 4L) + Rnd(4L, 4L));
            return elfnote(_addr_sh, startva, resoff, n);
        }

        private static long elfwritenetbsdpax(ptr<OutBuf> _addr_@out)
        {
            ref OutBuf @out = ref _addr_@out.val;

            var sh = elfwritenotehdr(_addr_out, ".note.netbsd.pax", 4L, 4L, 0x03UL);
            if (sh == null)
            {
                return 0L;
            }

            @out.Write((slice<byte>)"PaX\x00");
            @out.Write32(0x20UL); // 0x20 = Force disable ASLR
            return int(sh.size);

        }

        // OpenBSD Signature
        public static readonly long ELF_NOTE_OPENBSD_NAMESZ = (long)8L;
        public static readonly long ELF_NOTE_OPENBSD_DESCSZ = (long)4L;
        public static readonly long ELF_NOTE_OPENBSD_TAG = (long)1L;
        public static readonly long ELF_NOTE_OPENBSD_VERSION = (long)0L;


        public static slice<byte> ELF_NOTE_OPENBSD_NAME = (slice<byte>)"OpenBSD\x00";

        private static long elfopenbsdsig(ptr<ElfShdr> _addr_sh, ulong startva, ulong resoff)
        {
            ref ElfShdr sh = ref _addr_sh.val;

            var n = ELF_NOTE_OPENBSD_NAMESZ + ELF_NOTE_OPENBSD_DESCSZ;
            return elfnote(_addr_sh, startva, resoff, n);
        }

        private static long elfwriteopenbsdsig(ptr<OutBuf> _addr_@out)
        {
            ref OutBuf @out = ref _addr_@out.val;
 
            // Write Elf_Note header.
            var sh = elfwritenotehdr(_addr_out, ".note.openbsd.ident", ELF_NOTE_OPENBSD_NAMESZ, ELF_NOTE_OPENBSD_DESCSZ, ELF_NOTE_OPENBSD_TAG);

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

            const long maxLen = (long)32L;

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
        public static readonly long ELF_NOTE_BUILDINFO_NAMESZ = (long)4L;
        public static readonly long ELF_NOTE_BUILDINFO_TAG = (long)3L;


        public static slice<byte> ELF_NOTE_BUILDINFO_NAME = (slice<byte>)"GNU\x00";

        private static long elfbuildinfo(ptr<ElfShdr> _addr_sh, ulong startva, ulong resoff)
        {
            ref ElfShdr sh = ref _addr_sh.val;

            var n = int(ELF_NOTE_BUILDINFO_NAMESZ + Rnd(int64(len(buildinfo)), 4L));
            return elfnote(_addr_sh, startva, resoff, n);
        }

        private static long elfgobuildid(ptr<ElfShdr> _addr_sh, ulong startva, ulong resoff)
        {
            ref ElfShdr sh = ref _addr_sh.val;

            var n = len(ELF_NOTE_GO_NAME) + int(Rnd(int64(len(flagBuildid.val)), 4L));
            return elfnote(_addr_sh, startva, resoff, n);
        }

        private static long elfwritebuildinfo(ptr<OutBuf> _addr_@out)
        {
            ref OutBuf @out = ref _addr_@out.val;

            var sh = elfwritenotehdr(_addr_out, ".note.gnu.build-id", ELF_NOTE_BUILDINFO_NAMESZ, uint32(len(buildinfo)), ELF_NOTE_BUILDINFO_TAG);
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

        private static long elfwritegobuildid(ptr<OutBuf> _addr_@out)
        {
            ref OutBuf @out = ref _addr_@out.val;

            var sh = elfwritenotehdr(_addr_out, ".note.go.buildid", uint32(len(ELF_NOTE_GO_NAME)), uint32(len(flagBuildid.val)), ELF_NOTE_GOBUILDID_TAG);
            if (sh == null)
            {
                return 0L;
            }

            @out.Write(ELF_NOTE_GO_NAME);
            @out.Write((slice<byte>)flagBuildid.val);
            var zero = make_slice<byte>(4L);
            @out.Write(zero[..int(Rnd(int64(len(flagBuildid.val)), 4L) - int64(len(flagBuildid.val)))]);

            return int(sh.size);

        }

        // Go specific notes
        public static readonly long ELF_NOTE_GOPKGLIST_TAG = (long)1L;
        public static readonly long ELF_NOTE_GOABIHASH_TAG = (long)2L;
        public static readonly long ELF_NOTE_GODEPS_TAG = (long)3L;
        public static readonly long ELF_NOTE_GOBUILDID_TAG = (long)4L;


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

        private static ptr<Elfaux> addelflib(ptr<ptr<Elflib>> _addr_list, @string file, @string vers)
        {
            ref ptr<Elflib> list = ref _addr_list.val;

            ptr<Elflib> lib;

            lib = list.val;

            while (lib != null)
            {
                if (lib.file == file)
                {
                    goto havelib;
                lib = lib.next;
                }

            }

            lib = @new<Elflib>();
            lib.next = list.val;
            lib.file = file;
            list.val = addr(lib);

havelib:
            {
                var aux__prev1 = aux;

                var aux = lib.aux;

                while (aux != null)
                {
                    if (aux.vers == vers)
                    {
                        return _addr_aux!;
                    aux = aux.next;
                    }

                }


                aux = aux__prev1;
            }
            aux = @new<Elfaux>();
            aux.next = lib.aux;
            aux.vers = vers;
            lib.aux = aux;

            return _addr_aux!;

        }

        private static void elfdynhash2(ptr<Link> _addr_ctxt)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            if (!ctxt.IsELF)
            {
                return ;
            }

            var nsym = Nelfsym;
            var ldr = ctxt.loader;
            var s = ldr.CreateSymForUpdate(".hash", 0L);
            s.SetType(sym.SELFROSECT);
            s.SetReachable(true);

            var i = nsym;
            long nbucket = 1L;
            while (i > 0L)
            {
                nbucket++;
                i >>= 1L;
            }


            ptr<Elflib> needlib;
            var need = make_slice<ptr<Elfaux>>(nsym);
            var chain = make_slice<uint>(nsym);
            var buckets = make_slice<uint>(nbucket);

            {
                var sy__prev1 = sy;

                foreach (var (_, __sy) in ldr.DynidSyms())
                {
                    sy = __sy;
                    var dynid = ldr.SymDynid(sy);
                    if (ldr.SymDynimpvers(sy) != "")
                    {
                        need[dynid] = addelflib(_addr_needlib, ldr.SymDynimplib(sy), ldr.SymDynimpvers(sy));
                    }

                    var name = ldr.SymExtname(sy);
                    var hc = elfhash(name);

                    var b = hc % uint32(nbucket);
                    chain[dynid] = buckets[b];
                    buckets[b] = uint32(dynid);

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

            var dynstr = ldr.CreateSymForUpdate(".dynstr", 0L); 

            // version symbols
            var gnuVersionR = ldr.CreateSymForUpdate(".gnu.version_r", 0L);
            s = gnuVersionR;
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
                    s.AddUint32(ctxt.Arch, uint32(dynstr.Addstring(l.file))); // file string offset
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
                            s.AddUint32(ctxt.Arch, uint32(dynstr.Addstring(x.vers))); // version string offset
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
            var gnuVersion = ldr.CreateSymForUpdate(".gnu.version", 0L);
            s = gnuVersion;

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

            s = ldr.CreateSymForUpdate(".dynamic", 0L);
            elfverneed = nfile;
            if (elfverneed != 0L)
            {
                elfWriteDynEntSym2(_addr_ctxt, _addr_s, DT_VERNEED, gnuVersionR.Sym());
                Elfwritedynent2(_addr_ctxt.Arch, _addr_s, DT_VERNEEDNUM, uint64(nfile));
                elfWriteDynEntSym2(_addr_ctxt, _addr_s, DT_VERSYM, gnuVersion.Sym());
            }

            var sy = ldr.CreateSymForUpdate(elfRelType + ".plt", 0L);
            if (sy.Size() > 0L)
            {
                if (elfRelType == ".rela")
                {
                    Elfwritedynent2(_addr_ctxt.Arch, _addr_s, DT_PLTREL, DT_RELA);
                }
                else
                {
                    Elfwritedynent2(_addr_ctxt.Arch, _addr_s, DT_PLTREL, DT_REL);
                }

                elfwritedynentsymsize2(_addr_ctxt, _addr_s, DT_PLTRELSZ, sy.Sym());
                elfWriteDynEntSym2(_addr_ctxt, _addr_s, DT_JMPREL, sy.Sym());

            }

            Elfwritedynent2(_addr_ctxt.Arch, _addr_s, DT_NULL, 0L);

        }

        private static ptr<ElfPhdr> elfphload(ptr<sym.Segment> _addr_seg)
        {
            ref sym.Segment seg = ref _addr_seg.val;

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
            ph.align = uint64(FlagRound.val);

            return _addr_ph!;

        }

        private static void elfphrelro(ptr<sym.Segment> _addr_seg)
        {
            ref sym.Segment seg = ref _addr_seg.val;

            var ph = newElfPhdr();
            ph.type_ = PT_GNU_RELRO;
            ph.vaddr = seg.Vaddr;
            ph.paddr = seg.Vaddr;
            ph.memsz = seg.Length;
            ph.off = seg.Fileoff;
            ph.filesz = seg.Filelen;
            ph.align = uint64(FlagRound.val);
        }

        private static ptr<ElfShdr> elfshname(@string name)
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
                        return _addr_sh!;
                    }

                }

                return _addr_newElfShdr(int64(off))!;

            }

            Exitf("cannot find elf name %s", name);
            return _addr_null!;

        }

        // Create an ElfShdr for the section with name.
        // Create a duplicate if one already exists with that name
        private static ptr<ElfShdr> elfshnamedup(@string name)
        {
            for (long i = 0L; i < nelfstr; i++)
            {
                if (name == elfstr[i].s)
                {
                    var off = elfstr[i].off;
                    return _addr_newElfShdr(int64(off))!;
                }

            }


            Errorf(null, "cannot find elf name %s", name);
            errorexit();
            return _addr_null!;

        }

        private static ptr<ElfShdr> elfshalloc(ptr<sym.Section> _addr_sect)
        {
            ref sym.Section sect = ref _addr_sect.val;

            var sh = elfshname(sect.Name);
            sect.Elfsect = sh;
            return _addr_sh!;
        }

        private static ptr<ElfShdr> elfshbits(LinkMode linkmode, ptr<sym.Section> _addr_sect)
        {
            ref sym.Section sect = ref _addr_sect.val;

            ptr<ElfShdr> sh;

            if (sect.Name == ".text")
            {
                if (sect.Elfsect == null)
                {
                    sect.Elfsect = elfshnamedup(sect.Name);
                }

                sh = sect.Elfsect._<ptr<ElfShdr>>();

            }
            else
            {
                sh = elfshalloc(_addr_sect);
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
                return _addr_sh!;

            }

            if (sh.type_ > 0L)
            {
                return _addr_sh!;
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

            if (strings.HasPrefix(sect.Name, ".debug") || strings.HasPrefix(sect.Name, ".zdebug"))
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

            return _addr_sh!;

        }

        private static ptr<ElfShdr> elfshreloc(ptr<sys.Arch> _addr_arch, ptr<sym.Section> _addr_sect)
        {
            ref sys.Arch arch = ref _addr_arch.val;
            ref sym.Section sect = ref _addr_sect.val;
 
            // If main section is SHT_NOBITS, nothing to relocate.
            // Also nothing to relocate in .shstrtab or notes.
            if (sect.Vaddr >= sect.Seg.Vaddr + sect.Seg.Filelen)
            {
                return _addr_null!;
            }

            if (sect.Name == ".shstrtab" || sect.Name == ".tbss")
            {
                return _addr_null!;
            }

            if (sect.Elfsect._<ptr<ElfShdr>>().type_ == SHT_NOTE)
            {
                return _addr_null!;
            }

            var typ = SHT_REL;
            if (elfRelType == ".rela")
            {
                typ = SHT_RELA;
            }

            var sh = elfshname(elfRelType + sect.Name); 
            // There could be multiple text sections but each needs
            // its own .rela.text.

            if (sect.Name == ".text")
            {
                if (sh.info != 0L && sh.info != uint32(sect.Elfsect._<ptr<ElfShdr>>().shnum))
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
            sh.info = uint32(sect.Elfsect._<ptr<ElfShdr>>().shnum);
            sh.off = sect.Reloff;
            sh.size = sect.Rellen;
            sh.addralign = uint64(arch.RegSize);
            return _addr_sh!;

        }

        private static void elfrelocsect(ptr<Link> _addr_ctxt, ptr<sym.Section> _addr_sect, slice<ptr<sym.Symbol>> syms)
        {
            ref Link ctxt = ref _addr_ctxt.val;
            ref sym.Section sect = ref _addr_sect.val;

            if (!ctxt.IsAMD64())
            {
                elfrelocsect2(ctxt, sect, syms);
                return ;
            } 

            // If main section is SHT_NOBITS, nothing to relocate.
            // Also nothing to relocate in .shstrtab.
            if (sect.Vaddr >= sect.Seg.Vaddr + sect.Seg.Filelen)
            {
                return ;
            }

            if (sect.Name == ".shstrtab")
            {
                return ;
            }

            sect.Reloff = uint64(ctxt.Out.Offset());
            {
                var i__prev1 = i;
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

                i = i__prev1;
                s = s__prev1;
            }

            var ldr = ctxt.loader;
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

                    var i = loader.Sym(s.SymIdx);
                    var relocs = ldr.ExtRelocs(i);
                    for (long ri = 0L; ri < relocs.Count(); ri++)
                    {
                        var r = relocs.At(ri);
                        if (r.Xsym == 0L)
                        {
                            Errorf(s, "missing xsym in relocation %v", ldr.SymName(r.Sym()));
                            continue;
                        }

                        var esr = ElfSymForReloc(ctxt, ldr.Syms[r.Xsym]);
                        if (esr == 0L)
                        {
                            Errorf(s, "reloc %d (%s) to non-elf symbol %s (outer=%s) %d (%s)", r.Type(), sym.RelocName(ctxt.Arch, r.Type()), ldr.Syms[r.Sym()].Name, ldr.Syms[r.Xsym].Name, ldr.Syms[r.Sym()].Type, ldr.Syms[r.Sym()].Type);
                        }

                        if (!ldr.AttrReachable(r.Xsym))
                        {
                            Errorf(s, "unreachable reloc %d (%s) target %v", r.Type(), sym.RelocName(ctxt.Arch, r.Type()), ldr.Syms[r.Xsym].Name);
                        }

                        if (!thearch.Elfreloc2(ctxt, ldr, i, r, int64(uint64(s.Value + int64(r.Off())) - sect.Vaddr)))
                        {
                            Errorf(s, "unsupported obj reloc %d (%s)/%d to %s", r.Type(), sym.RelocName(ctxt.Arch, r.Type()), r.Siz(), ldr.Syms[r.Sym()].Name);
                        }

                    }


                }

                s = s__prev1;
            }

            sect.Rellen = uint64(ctxt.Out.Offset()) - sect.Reloff;

        }

        public static void Elfemitreloc(ptr<Link> _addr_ctxt) => func((_, panic, __) =>
        {
            ref Link ctxt = ref _addr_ctxt.val;

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
                        elfrelocsect(_addr_ctxt, _addr_sect, ctxt.Textp);
                    }
                    else
                    {
                        elfrelocsect(_addr_ctxt, _addr_sect, ctxt.datap);
                    }

                }

                sect = sect__prev1;
            }

            {
                var sect__prev1 = sect;

                foreach (var (_, __sect) in Segrodata.Sections)
                {
                    sect = __sect;
                    elfrelocsect(_addr_ctxt, _addr_sect, ctxt.datap);
                }

                sect = sect__prev1;
            }

            {
                var sect__prev1 = sect;

                foreach (var (_, __sect) in Segrelrodata.Sections)
                {
                    sect = __sect;
                    elfrelocsect(_addr_ctxt, _addr_sect, ctxt.datap);
                }

                sect = sect__prev1;
            }

            {
                var sect__prev1 = sect;

                foreach (var (_, __sect) in Segdata.Sections)
                {
                    sect = __sect;
                    elfrelocsect(_addr_ctxt, _addr_sect, ctxt.datap);
                }

                sect = sect__prev1;
            }

            for (long i = 0L; i < len(Segdwarf.Sections); i++)
            {
                var sect = Segdwarf.Sections[i];
                var si = dwarfp[i];
                if (si.secSym() != sect.Sym || si.secSym().Sect != sect)
                {
                    panic("inconsistency between dwarfp and Segdwarf");
                }

                elfrelocsect(_addr_ctxt, _addr_sect, si.syms);

            }


        });

        private static void addgonote(ptr<Link> _addr_ctxt, @string sectionName, uint tag, slice<byte> desc)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            var ldr = ctxt.loader;
            var s = ldr.CreateSymForUpdate(sectionName, 0L);
            s.SetReachable(true);
            s.SetType(sym.SELFROSECT); 
            // namesz
            s.AddUint32(ctxt.Arch, uint32(len(ELF_NOTE_GO_NAME))); 
            // descsz
            s.AddUint32(ctxt.Arch, uint32(len(desc))); 
            // tag
            s.AddUint32(ctxt.Arch, tag); 
            // name + padding
            s.AddBytes(ELF_NOTE_GO_NAME);
            while (len(s.Data()) % 4L != 0L)
            {
                s.AddUint8(0L);
            } 
            // desc + padding
 
            // desc + padding
            s.AddBytes(desc);
            while (len(s.Data()) % 4L != 0L)
            {
                s.AddUint8(0L);
            }

            s.SetSize(int64(len(s.Data())));
            s.SetAlign(4L);

        }

        private static void doelf(this ptr<Link> _addr_ctxt)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            var ldr = ctxt.loader; 

            /* predefine strings we need for section headers */
            var shstrtab = ldr.CreateSymForUpdate(".shstrtab", 0L);

            shstrtab.SetType(sym.SELFROSECT);
            shstrtab.SetReachable(true);

            shstrtab.Addstring("");
            shstrtab.Addstring(".text");
            shstrtab.Addstring(".noptrdata");
            shstrtab.Addstring(".data");
            shstrtab.Addstring(".bss");
            shstrtab.Addstring(".noptrbss");
            shstrtab.Addstring("__libfuzzer_extra_counters");
            shstrtab.Addstring(".go.buildinfo"); 

            // generate .tbss section for dynamic internal linker or external
            // linking, so that various binutils could correctly calculate
            // PT_TLS size. See https://golang.org/issue/5200.
            if (!FlagD || ctxt.IsExternal().val)
            {
                shstrtab.Addstring(".tbss");
            }

            if (ctxt.IsNetbsd())
            {
                shstrtab.Addstring(".note.netbsd.ident");
                if (flagRace.val)
                {
                    shstrtab.Addstring(".note.netbsd.pax");
                }

            }

            if (ctxt.IsOpenbsd())
            {
                shstrtab.Addstring(".note.openbsd.ident");
            }

            if (len(buildinfo) > 0L)
            {
                shstrtab.Addstring(".note.gnu.build-id");
            }

            if (flagBuildid != "".val)
            {
                shstrtab.Addstring(".note.go.buildid");
            }

            shstrtab.Addstring(".elfdata");
            shstrtab.Addstring(".rodata"); 
            // See the comment about data.rel.ro.FOO section names in data.go.
            @string relro_prefix = "";
            if (ctxt.UseRelro())
            {
                shstrtab.Addstring(".data.rel.ro");
                relro_prefix = ".data.rel.ro";
            }

            shstrtab.Addstring(relro_prefix + ".typelink");
            shstrtab.Addstring(relro_prefix + ".itablink");
            shstrtab.Addstring(relro_prefix + ".gosymtab");
            shstrtab.Addstring(relro_prefix + ".gopclntab");

            if (ctxt.IsExternal())
            {
                FlagD.val = true;

                shstrtab.Addstring(elfRelType + ".text");
                shstrtab.Addstring(elfRelType + ".rodata");
                shstrtab.Addstring(elfRelType + relro_prefix + ".typelink");
                shstrtab.Addstring(elfRelType + relro_prefix + ".itablink");
                shstrtab.Addstring(elfRelType + relro_prefix + ".gosymtab");
                shstrtab.Addstring(elfRelType + relro_prefix + ".gopclntab");
                shstrtab.Addstring(elfRelType + ".noptrdata");
                shstrtab.Addstring(elfRelType + ".data");
                if (ctxt.UseRelro())
                {
                    shstrtab.Addstring(elfRelType + ".data.rel.ro");
                }

                shstrtab.Addstring(elfRelType + ".go.buildinfo"); 

                // add a .note.GNU-stack section to mark the stack as non-executable
                shstrtab.Addstring(".note.GNU-stack");

                if (ctxt.IsShared())
                {
                    shstrtab.Addstring(".note.go.abihash");
                    shstrtab.Addstring(".note.go.pkg-list");
                    shstrtab.Addstring(".note.go.deps");
                }

            }

            var hasinitarr = ctxt.linkShared; 

            /* shared library initializer */

            if (ctxt.BuildMode == BuildModeCArchive || ctxt.BuildMode == BuildModeCShared || ctxt.BuildMode == BuildModeShared || ctxt.BuildMode == BuildModePlugin) 
                hasinitarr = true;
                        if (hasinitarr)
            {
                shstrtab.Addstring(".init_array");
                shstrtab.Addstring(elfRelType + ".init_array");
            }

            if (!FlagS.val)
            {
                shstrtab.Addstring(".symtab");
                shstrtab.Addstring(".strtab");
                dwarfaddshstrings(ctxt, shstrtab);
            }

            shstrtab.Addstring(".shstrtab");

            if (!FlagD.val)
            { /* -d suppresses dynamic loader format */
                shstrtab.Addstring(".interp");
                shstrtab.Addstring(".hash");
                shstrtab.Addstring(".got");
                if (ctxt.IsPPC64())
                {
                    shstrtab.Addstring(".glink");
                }

                shstrtab.Addstring(".got.plt");
                shstrtab.Addstring(".dynamic");
                shstrtab.Addstring(".dynsym");
                shstrtab.Addstring(".dynstr");
                shstrtab.Addstring(elfRelType);
                shstrtab.Addstring(elfRelType + ".plt");

                shstrtab.Addstring(".plt");
                shstrtab.Addstring(".gnu.version");
                shstrtab.Addstring(".gnu.version_r"); 

                /* dynamic symbol table - first entry all zeros */
                var dynsym = ldr.CreateSymForUpdate(".dynsym", 0L);

                dynsym.SetType(sym.SELFROSECT);
                dynsym.SetReachable(true);
                if (elf64)
                {
                    dynsym.SetSize(dynsym.Size() + ELF64SYMSIZE);
                }
                else
                {
                    dynsym.SetSize(dynsym.Size() + ELF32SYMSIZE);
                } 

                /* dynamic string table */
                var dynstr = ldr.CreateSymForUpdate(".dynstr", 0L);

                dynstr.SetType(sym.SELFROSECT);
                dynstr.SetReachable(true);
                if (dynstr.Size() == 0L)
                {
                    dynstr.Addstring("");
                } 

                /* relocation table */
                var s = ldr.CreateSymForUpdate(elfRelType, 0L);
                s.SetReachable(true);
                s.SetType(sym.SELFROSECT); 

                /* global offset table */
                var got = ldr.CreateSymForUpdate(".got", 0L);
                got.SetReachable(true);
                got.SetType(sym.SELFGOT); // writable

                /* ppc64 glink resolver */
                if (ctxt.IsPPC64())
                {
                    s = ldr.CreateSymForUpdate(".glink", 0L);
                    s.SetReachable(true);
                    s.SetType(sym.SELFRXSECT);
                } 

                /* hash */
                var hash = ldr.CreateSymForUpdate(".hash", 0L);
                hash.SetReachable(true);
                hash.SetType(sym.SELFROSECT);

                var gotplt = ldr.CreateSymForUpdate(".got.plt", 0L);
                gotplt.SetReachable(true);
                gotplt.SetType(sym.SELFSECT); // writable

                var plt = ldr.CreateSymForUpdate(".plt", 0L);
                plt.SetReachable(true);
                if (ctxt.IsPPC64())
                { 
                    // In the ppc64 ABI, .plt is a data section
                    // written by the dynamic linker.
                    plt.SetType(sym.SELFSECT);

                }
                else
                {
                    plt.SetType(sym.SELFRXSECT);
                }

                s = ldr.CreateSymForUpdate(elfRelType + ".plt", 0L);
                s.SetReachable(true);
                s.SetType(sym.SELFROSECT);

                s = ldr.CreateSymForUpdate(".gnu.version", 0L);
                s.SetReachable(true);
                s.SetType(sym.SELFROSECT);

                s = ldr.CreateSymForUpdate(".gnu.version_r", 0L);
                s.SetReachable(true);
                s.SetType(sym.SELFROSECT); 

                /* define dynamic elf table */
                var dynamic = ldr.CreateSymForUpdate(".dynamic", 0L);
                dynamic.SetReachable(true);
                dynamic.SetType(sym.SELFSECT); // writable

                if (ctxt.IsS390X())
                { 
                    // S390X uses .got instead of .got.plt
                    gotplt = got;

                }

                thearch.Elfsetupplt(ctxt, plt, gotplt, dynamic.Sym());

                /*
                         * .dynamic table
                         */
                elfwritedynentsym2(_addr_ctxt, _addr_dynamic, DT_HASH, hash.Sym());

                elfwritedynentsym2(_addr_ctxt, _addr_dynamic, DT_SYMTAB, dynsym.Sym());
                if (elf64)
                {
                    Elfwritedynent2(_addr_ctxt.Arch, _addr_dynamic, DT_SYMENT, ELF64SYMSIZE);
                }
                else
                {
                    Elfwritedynent2(_addr_ctxt.Arch, _addr_dynamic, DT_SYMENT, ELF32SYMSIZE);
                }

                elfwritedynentsym2(_addr_ctxt, _addr_dynamic, DT_STRTAB, dynstr.Sym());
                elfwritedynentsymsize2(_addr_ctxt, _addr_dynamic, DT_STRSZ, dynstr.Sym());
                if (elfRelType == ".rela")
                {
                    var rela = ldr.LookupOrCreateSym(".rela", 0L);
                    elfwritedynentsym2(_addr_ctxt, _addr_dynamic, DT_RELA, rela);
                    elfwritedynentsymsize2(_addr_ctxt, _addr_dynamic, DT_RELASZ, rela);
                    Elfwritedynent2(_addr_ctxt.Arch, _addr_dynamic, DT_RELAENT, ELF64RELASIZE);
                }
                else
                {
                    var rel = ldr.LookupOrCreateSym(".rel", 0L);
                    elfwritedynentsym2(_addr_ctxt, _addr_dynamic, DT_REL, rel);
                    elfwritedynentsymsize2(_addr_ctxt, _addr_dynamic, DT_RELSZ, rel);
                    Elfwritedynent2(_addr_ctxt.Arch, _addr_dynamic, DT_RELENT, ELF32RELSIZE);
                }

                if (rpath.val != "")
                {
                    Elfwritedynent2(_addr_ctxt.Arch, _addr_dynamic, DT_RUNPATH, uint64(dynstr.Addstring(rpath.val)));
                }

                if (ctxt.IsPPC64())
                {
                    elfwritedynentsym2(_addr_ctxt, _addr_dynamic, DT_PLTGOT, plt.Sym());
                }
                else
                {
                    elfwritedynentsym2(_addr_ctxt, _addr_dynamic, DT_PLTGOT, gotplt.Sym());
                }

                if (ctxt.IsPPC64())
                {
                    Elfwritedynent2(_addr_ctxt.Arch, _addr_dynamic, DT_PPC64_OPT, 0L);
                } 

                // Solaris dynamic linker can't handle an empty .rela.plt if
                // DT_JMPREL is emitted so we have to defer generation of DT_PLTREL,
                // DT_PLTRELSZ, and DT_JMPREL dynamic entries until after we know the
                // size of .rel(a).plt section.
                Elfwritedynent2(_addr_ctxt.Arch, _addr_dynamic, DT_DEBUG, 0L);

            }

            if (ctxt.IsShared())
            { 
                // The go.link.abihashbytes symbol will be pointed at the appropriate
                // part of the .note.go.abihash section in data.go:func address().
                s = ldr.LookupOrCreateSym("go.link.abihashbytes", 0L);
                var sb = ldr.MakeSymbolUpdater(s);
                ldr.SetAttrLocal(s, true);
                sb.SetType(sym.SRODATA);
                ldr.SetAttrSpecial(s, true);
                sb.SetReachable(true);
                sb.SetSize(sha1.Size);

                sort.Sort(byPkg(ctxt.Library));
                var h = sha1.New();
                foreach (var (_, l) in ctxt.Library)
                {
                    io.WriteString(h, l.Hash);
                }
                addgonote(_addr_ctxt, ".note.go.abihash", ELF_NOTE_GOABIHASH_TAG, h.Sum(new slice<byte>(new byte[] {  })));
                addgonote(_addr_ctxt, ".note.go.pkg-list", ELF_NOTE_GOPKGLIST_TAG, pkglistfornote);
                slice<@string> deplist = default;
                foreach (var (_, shlib) in ctxt.Shlibs)
                {
                    deplist = append(deplist, filepath.Base(shlib.Path));
                }
                addgonote(_addr_ctxt, ".note.go.deps", ELF_NOTE_GODEPS_TAG, (slice<byte>)strings.Join(deplist, "\n"));

            }

            if (ctxt.LinkMode == LinkExternal && flagBuildid != "".val)
            {
                addgonote(_addr_ctxt, ".note.go.buildid", ELF_NOTE_GOBUILDID_TAG, (slice<byte>)flagBuildid.val);
            }

        }

        // Do not write DT_NULL.  elfdynhash will finish it.
        private static void shsym(ptr<ElfShdr> _addr_sh, ptr<sym.Symbol> _addr_s)
        {
            ref ElfShdr sh = ref _addr_sh.val;
            ref sym.Symbol s = ref _addr_s.val;

            var addr = Symaddr(s);
            if (sh.flags & SHF_ALLOC != 0L)
            {
                sh.addr = uint64(addr);
            }

            sh.off = uint64(datoff(s, addr));
            sh.size = uint64(s.Size);

        }

        private static void phsh(ptr<ElfPhdr> _addr_ph, ptr<ElfShdr> _addr_sh)
        {
            ref ElfPhdr ph = ref _addr_ph.val;
            ref ElfShdr sh = ref _addr_sh.val;

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
                        elfshalloc(_addr_sect);
                    }

                }

                sect = sect__prev1;
            }

            {
                var sect__prev1 = sect;

                foreach (var (_, __sect) in Segrodata.Sections)
                {
                    sect = __sect;
                    elfshalloc(_addr_sect);
                }

                sect = sect__prev1;
            }

            {
                var sect__prev1 = sect;

                foreach (var (_, __sect) in Segrelrodata.Sections)
                {
                    sect = __sect;
                    elfshalloc(_addr_sect);
                }

                sect = sect__prev1;
            }

            {
                var sect__prev1 = sect;

                foreach (var (_, __sect) in Segdata.Sections)
                {
                    sect = __sect;
                    elfshalloc(_addr_sect);
                }

                sect = sect__prev1;
            }

            {
                var sect__prev1 = sect;

                foreach (var (_, __sect) in Segdwarf.Sections)
                {
                    sect = __sect;
                    elfshalloc(_addr_sect);
                }

                sect = sect__prev1;
            }
        }

        public static void Asmbelf(ptr<Link> _addr_ctxt, long symo)
        {
            ref Link ctxt = ref _addr_ctxt.val;

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
            else if (ctxt.Arch.Family == sys.RISCV64) 
                eh.machine = EM_RISCV;
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

            var startva = FlagTextAddr - int64(HEADR).val;
            var resoff = elfreserve;

            ptr<ElfPhdr> pph;
            ptr<ElfPhdr> pnote;
            if (flagRace && ctxt.IsNetbsd().val)
            {
                var sh = elfshname(".note.netbsd.pax");
                resoff -= int64(elfnetbsdpax(_addr_sh, uint64(startva), uint64(resoff)));
                pnote = newElfPhdr();
                pnote.type_ = PT_NOTE;
                pnote.flags = PF_R;
                phsh(pnote, _addr_sh);
            }

            if (ctxt.LinkMode == LinkExternal)
            { 
                /* skip program headers */
                eh.phoff = 0L;

                eh.phentsize = 0L;

                if (ctxt.BuildMode == BuildModeShared)
                {
                    sh = elfshname(".note.go.pkg-list");
                    sh.type_ = SHT_NOTE;
                    sh = elfshname(".note.go.abihash");
                    sh.type_ = SHT_NOTE;
                    sh.flags = SHF_ALLOC;
                    sh = elfshname(".note.go.deps");
                    sh.type_ = SHT_NOTE;
                }

                if (flagBuildid != "".val)
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
            pph.vaddr = uint64(FlagTextAddr.val) - uint64(HEADR) + pph.off;
            pph.paddr = uint64(FlagTextAddr.val) - uint64(HEADR) + pph.off;
            pph.align = uint64(FlagRound.val);

            /*
                 * PHDR must be in a loaded segment. Adjust the text
                 * segment boundaries downwards to include it.
                 */
            {
                var o = int64(Segtext.Vaddr - pph.vaddr);
                Segtext.Vaddr -= uint64(o);
                Segtext.Length += uint64(o);
                o = int64(Segtext.Fileoff - pph.off);
                Segtext.Fileoff -= uint64(o);
                Segtext.Filelen += uint64(o);
            }
            if (!FlagD.val)
            {                /* -d suppresses dynamic loader format */
                /* interpreter */
                sh = elfshname(".interp");

                sh.type_ = SHT_PROGBITS;
                sh.flags = SHF_ALLOC;
                sh.addralign = 1L;

                if (interpreter == "" && objabi.GO_LDSO != "")
                {
                    interpreter = objabi.GO_LDSO;
                }

                if (interpreter == "")
                {

                    if (ctxt.HeadType == objabi.Hlinux) 
                        if (objabi.GOOS == "android")
                        {
                            interpreter = thearch.Androiddynld;
                            if (interpreter == "")
                            {
                                Exitf("ELF interpreter not set");
                            }

                        }
                        else
                        {
                            interpreter = thearch.Linuxdynld;
                        }

                    else if (ctxt.HeadType == objabi.Hfreebsd) 
                        interpreter = thearch.Freebsddynld;
                    else if (ctxt.HeadType == objabi.Hnetbsd) 
                        interpreter = thearch.Netbsddynld;
                    else if (ctxt.HeadType == objabi.Hopenbsd) 
                        interpreter = thearch.Openbsddynld;
                    else if (ctxt.HeadType == objabi.Hdragonfly) 
                        interpreter = thearch.Dragonflydynld;
                    else if (ctxt.HeadType == objabi.Hsolaris) 
                        interpreter = thearch.Solarisdynld;
                    
                }

                resoff -= int64(elfinterp(_addr_sh, uint64(startva), uint64(resoff), interpreter));

                var ph = newElfPhdr();
                ph.type_ = PT_INTERP;
                ph.flags = PF_R;
                phsh(_addr_ph, _addr_sh);

            }

            pnote = null;
            if (ctxt.HeadType == objabi.Hnetbsd || ctxt.HeadType == objabi.Hopenbsd)
            {
                sh = ;

                if (ctxt.HeadType == objabi.Hnetbsd) 
                    sh = elfshname(".note.netbsd.ident");
                    resoff -= int64(elfnetbsdsig(_addr_sh, uint64(startva), uint64(resoff)));
                else if (ctxt.HeadType == objabi.Hopenbsd) 
                    sh = elfshname(".note.openbsd.ident");
                    resoff -= int64(elfopenbsdsig(_addr_sh, uint64(startva), uint64(resoff)));
                                pnote = newElfPhdr();
                pnote.type_ = PT_NOTE;
                pnote.flags = PF_R;
                phsh(pnote, _addr_sh);

            }

            if (len(buildinfo) > 0L)
            {
                sh = elfshname(".note.gnu.build-id");
                resoff -= int64(elfbuildinfo(_addr_sh, uint64(startva), uint64(resoff)));

                if (pnote == null)
                {
                    pnote = newElfPhdr();
                    pnote.type_ = PT_NOTE;
                    pnote.flags = PF_R;
                }

                phsh(pnote, _addr_sh);

            }

            if (flagBuildid != "".val)
            {
                sh = elfshname(".note.go.buildid");
                resoff -= int64(elfgobuildid(_addr_sh, uint64(startva), uint64(resoff)));

                pnote = newElfPhdr();
                pnote.type_ = PT_NOTE;
                pnote.flags = PF_R;
                phsh(pnote, _addr_sh);
            } 

            // Additions to the reserved area must be above this line.
            elfphload(_addr_Segtext);
            if (len(Segrodata.Sections) > 0L)
            {
                elfphload(_addr_Segrodata);
            }

            if (len(Segrelrodata.Sections) > 0L)
            {
                elfphload(_addr_Segrelrodata);
                elfphrelro(_addr_Segrelrodata);
            }

            elfphload(_addr_Segdata); 

            /* Dynamic linking sections */
            if (!FlagD.val)
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

                // sh.info is the index of first non-local symbol (number of local symbols)
                var s = ctxt.Syms.Lookup(".dynsym", 0L);
                var i = uint32(0L);
                {
                    var sub = s;

                    while (sub != null)
                    {
                        i++;
                        if (!sub.Attr.Local())
                        {
                            break;
                        sub = symSub(ctxt, sub);
                        }

                    }

                }
                sh.info = i;
                shsym(_addr_sh, _addr_s);

                sh = elfshname(".dynstr");
                sh.type_ = SHT_STRTAB;
                sh.flags = SHF_ALLOC;
                sh.addralign = 1L;
                shsym(_addr_sh, _addr_ctxt.Syms.Lookup(".dynstr", 0L));

                if (elfverneed != 0L)
                {
                    sh = elfshname(".gnu.version");
                    sh.type_ = SHT_GNU_VERSYM;
                    sh.flags = SHF_ALLOC;
                    sh.addralign = 2L;
                    sh.link = uint32(elfshname(".dynsym").shnum);
                    sh.entsize = 2L;
                    shsym(_addr_sh, _addr_ctxt.Syms.Lookup(".gnu.version", 0L));

                    sh = elfshname(".gnu.version_r");
                    sh.type_ = SHT_GNU_VERNEED;
                    sh.flags = SHF_ALLOC;
                    sh.addralign = uint64(ctxt.Arch.RegSize);
                    sh.info = uint32(elfverneed);
                    sh.link = uint32(elfshname(".dynstr").shnum);
                    shsym(_addr_sh, _addr_ctxt.Syms.Lookup(".gnu.version_r", 0L));
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
                    shsym(_addr_sh, _addr_ctxt.Syms.Lookup(".rela.plt", 0L));

                    sh = elfshname(".rela");
                    sh.type_ = SHT_RELA;
                    sh.flags = SHF_ALLOC;
                    sh.entsize = ELF64RELASIZE;
                    sh.addralign = 8L;
                    sh.link = uint32(elfshname(".dynsym").shnum);
                    shsym(_addr_sh, _addr_ctxt.Syms.Lookup(".rela", 0L));
                }
                else
                {
                    sh = elfshname(".rel.plt");
                    sh.type_ = SHT_REL;
                    sh.flags = SHF_ALLOC;
                    sh.entsize = ELF32RELSIZE;
                    sh.addralign = 4L;
                    sh.link = uint32(elfshname(".dynsym").shnum);
                    shsym(_addr_sh, _addr_ctxt.Syms.Lookup(".rel.plt", 0L));

                    sh = elfshname(".rel");
                    sh.type_ = SHT_REL;
                    sh.flags = SHF_ALLOC;
                    sh.entsize = ELF32RELSIZE;
                    sh.addralign = 4L;
                    sh.link = uint32(elfshname(".dynsym").shnum);
                    shsym(_addr_sh, _addr_ctxt.Syms.Lookup(".rel", 0L));
                }

                if (eh.machine == EM_PPC64)
                {
                    sh = elfshname(".glink");
                    sh.type_ = SHT_PROGBITS;
                    sh.flags = SHF_ALLOC + SHF_EXECINSTR;
                    sh.addralign = 4L;
                    shsym(_addr_sh, _addr_ctxt.Syms.Lookup(".glink", 0L));
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
                shsym(_addr_sh, _addr_ctxt.Syms.Lookup(".plt", 0L)); 

                // On ppc64, .got comes from the input files, so don't
                // create it here, and .got.plt is not used.
                if (eh.machine != EM_PPC64)
                {
                    sh = elfshname(".got");
                    sh.type_ = SHT_PROGBITS;
                    sh.flags = SHF_ALLOC + SHF_WRITE;
                    sh.entsize = uint64(ctxt.Arch.RegSize);
                    sh.addralign = uint64(ctxt.Arch.RegSize);
                    shsym(_addr_sh, _addr_ctxt.Syms.Lookup(".got", 0L));

                    sh = elfshname(".got.plt");
                    sh.type_ = SHT_PROGBITS;
                    sh.flags = SHF_ALLOC + SHF_WRITE;
                    sh.entsize = uint64(ctxt.Arch.RegSize);
                    sh.addralign = uint64(ctxt.Arch.RegSize);
                    shsym(_addr_sh, _addr_ctxt.Syms.Lookup(".got.plt", 0L));
                }

                sh = elfshname(".hash");
                sh.type_ = SHT_HASH;
                sh.flags = SHF_ALLOC;
                sh.entsize = 4L;
                sh.addralign = uint64(ctxt.Arch.RegSize);
                sh.link = uint32(elfshname(".dynsym").shnum);
                shsym(_addr_sh, _addr_ctxt.Syms.Lookup(".hash", 0L)); 

                /* sh and PT_DYNAMIC for .dynamic section */
                sh = elfshname(".dynamic");

                sh.type_ = SHT_DYNAMIC;
                sh.flags = SHF_ALLOC + SHF_WRITE;
                sh.entsize = 2L * uint64(ctxt.Arch.RegSize);
                sh.addralign = uint64(ctxt.Arch.RegSize);
                sh.link = uint32(elfshname(".dynstr").shnum);
                shsym(_addr_sh, _addr_ctxt.Syms.Lookup(".dynamic", 0L));
                ph = newElfPhdr();
                ph.type_ = PT_DYNAMIC;
                ph.flags = PF_R + PF_W;
                phsh(_addr_ph, _addr_sh);

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
            shsym(_addr_sh, _addr_ctxt.Syms.Lookup(".shstrtab", 0L));
            eh.shstrndx = uint16(sh.shnum); 

            // put these sections early in the list
            if (!FlagS.val)
            {
                elfshname(".symtab");
                elfshname(".strtab");
            }

            {
                var sect__prev1 = sect;

                foreach (var (_, __sect) in Segtext.Sections)
                {
                    sect = __sect;
                    elfshbits(ctxt.LinkMode, _addr_sect);
                }

                sect = sect__prev1;
            }

            {
                var sect__prev1 = sect;

                foreach (var (_, __sect) in Segrodata.Sections)
                {
                    sect = __sect;
                    elfshbits(ctxt.LinkMode, _addr_sect);
                }

                sect = sect__prev1;
            }

            {
                var sect__prev1 = sect;

                foreach (var (_, __sect) in Segrelrodata.Sections)
                {
                    sect = __sect;
                    elfshbits(ctxt.LinkMode, _addr_sect);
                }

                sect = sect__prev1;
            }

            {
                var sect__prev1 = sect;

                foreach (var (_, __sect) in Segdata.Sections)
                {
                    sect = __sect;
                    elfshbits(ctxt.LinkMode, _addr_sect);
                }

                sect = sect__prev1;
            }

            {
                var sect__prev1 = sect;

                foreach (var (_, __sect) in Segdwarf.Sections)
                {
                    sect = __sect;
                    elfshbits(ctxt.LinkMode, _addr_sect);
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
                        elfshreloc(_addr_ctxt.Arch, _addr_sect);
                    }

                    sect = sect__prev1;
                }

                {
                    var sect__prev1 = sect;

                    foreach (var (_, __sect) in Segrodata.Sections)
                    {
                        sect = __sect;
                        elfshreloc(_addr_ctxt.Arch, _addr_sect);
                    }

                    sect = sect__prev1;
                }

                {
                    var sect__prev1 = sect;

                    foreach (var (_, __sect) in Segrelrodata.Sections)
                    {
                        sect = __sect;
                        elfshreloc(_addr_ctxt.Arch, _addr_sect);
                    }

                    sect = sect__prev1;
                }

                {
                    var sect__prev1 = sect;

                    foreach (var (_, __sect) in Segdata.Sections)
                    {
                        sect = __sect;
                        elfshreloc(_addr_ctxt.Arch, _addr_sect);
                    }

                    sect = sect__prev1;
                }

                foreach (var (_, si) in dwarfp)
                {
                    s = si.secSym();
                    elfshreloc(_addr_ctxt.Arch, _addr_s.Sect);
                } 
                // add a .note.GNU-stack section to mark the stack as non-executable
                sh = elfshname(".note.GNU-stack");

                sh.type_ = SHT_PROGBITS;
                sh.addralign = 1L;
                sh.flags = 0L;

            }

            if (!FlagS.val)
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
            a += int64(elfwritehdr(_addr_ctxt.Out));
            a += int64(elfwritephdrs(_addr_ctxt.Out));
            a += int64(elfwriteshdrs(_addr_ctxt.Out));
            if (!FlagD.val)
            {
                a += int64(elfwriteinterp(_addr_ctxt.Out));
            }

            if (ctxt.LinkMode != LinkExternal)
            {
                if (ctxt.HeadType == objabi.Hnetbsd)
                {
                    a += int64(elfwritenetbsdsig(_addr_ctxt.Out));
                }

                if (ctxt.HeadType == objabi.Hopenbsd)
                {
                    a += int64(elfwriteopenbsdsig(_addr_ctxt.Out));
                }

                if (len(buildinfo) > 0L)
                {
                    a += int64(elfwritebuildinfo(_addr_ctxt.Out));
                }

                if (flagBuildid != "".val)
                {
                    a += int64(elfwritegobuildid(_addr_ctxt.Out));
                }

            }

            if (flagRace && ctxt.IsNetbsd().val)
            {
                a += int64(elfwritenetbsdpax(_addr_ctxt.Out));
            }

            if (a > elfreserve)
            {
                Errorf(null, "ELFRESERVE too small: %d > %d with %d text sections", a, elfreserve, numtext);
            }

        }

        private static void elfadddynsym2(ptr<loader.Loader> _addr_ldr, ptr<Target> _addr_target, ptr<ArchSyms> _addr_syms, loader.Sym s)
        {
            ref loader.Loader ldr = ref _addr_ldr.val;
            ref Target target = ref _addr_target.val;
            ref ArchSyms syms = ref _addr_syms.val;

            ldr.SetSymDynid(s, int32(Nelfsym));
            Nelfsym++;
            var d = ldr.MakeSymbolUpdater(syms.DynSym2);
            var name = ldr.SymExtname(s);
            var dstru = ldr.MakeSymbolUpdater(syms.DynStr2);
            var st = ldr.SymType(s);
            var cgoeStatic = ldr.AttrCgoExportStatic(s);
            var cgoeDynamic = ldr.AttrCgoExportDynamic(s);
            var cgoexp = (cgoeStatic || cgoeDynamic);

            d.AddUint32(target.Arch, uint32(dstru.Addstring(name)));

            if (elf64)
            {
                /* type */
                var t = STB_GLOBAL << (int)(4L);

                if (cgoexp && st == sym.STEXT)
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
                if (st == sym.SDYNIMPORT)
                {
                    d.AddUint16(target.Arch, SHN_UNDEF);
                }
                else
                {
                    d.AddUint16(target.Arch, 1L);
                } 

                /* value */
                if (st == sym.SDYNIMPORT)
                {
                    d.AddUint64(target.Arch, 0L);
                }
                else
                {
                    d.AddAddrPlus(target.Arch, s, 0L);
                } 

                /* size of object */
                d.AddUint64(target.Arch, uint64(len(ldr.Data(s))));

                var dil = ldr.SymDynimplib(s);

                if (target.Arch.Family == sys.AMD64 && !cgoeDynamic && dil != "" && !seenlib[dil])
                {
                    var du = ldr.MakeSymbolUpdater(syms.Dynamic2);
                    Elfwritedynent2(_addr_target.Arch, _addr_du, DT_NEEDED, uint64(dstru.Addstring(dil)));
                }

            }
            else
            {
                /* value */
                if (st == sym.SDYNIMPORT)
                {
                    d.AddUint32(target.Arch, 0L);
                }
                else
                {
                    d.AddAddrPlus(target.Arch, s, 0L);
                } 

                /* size of object */
                d.AddUint32(target.Arch, uint32(len(ldr.Data(s)))); 

                /* type */
                t = STB_GLOBAL << (int)(4L); 

                // TODO(mwhudson): presumably the behavior should actually be the same on both arm and 386.
                if (target.Arch.Family == sys.I386 && cgoexp && st == sym.STEXT)
                {
                    t |= STT_FUNC;
                }
                else if (target.Arch.Family == sys.ARM && cgoeDynamic && st == sym.STEXT)
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
                if (st == sym.SDYNIMPORT)
                {
                    d.AddUint16(target.Arch, SHN_UNDEF);
                }
                else
                {
                    d.AddUint16(target.Arch, 1L);
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
