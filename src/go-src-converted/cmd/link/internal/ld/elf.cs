// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ld -- go2cs converted at 2022 March 13 06:34:21 UTC
// import "cmd/link/internal/ld" ==> using ld = go.cmd.link.@internal.ld_package
// Original source: C:\Program Files\Go\src\cmd\link\internal\ld\elf.go
namespace go.cmd.link.@internal;

using objabi = cmd.@internal.objabi_package;
using sys = cmd.@internal.sys_package;
using loader = cmd.link.@internal.loader_package;
using sym = cmd.link.@internal.sym_package;
using sha1 = crypto.sha1_package;
using elf = debug.elf_package;
using binary = encoding.binary_package;
using hex = encoding.hex_package;
using fmt = fmt_package;
using buildcfg = @internal.buildcfg_package;
using filepath = path.filepath_package;
using runtime = runtime_package;
using sort = sort_package;
using strings = strings_package;


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

public static partial class ld_package {

private partial struct elfNote {
    public uint nNamesz;
    public uint nDescsz;
    public uint nType;
}

/* For accessing the fields of r_info. */

/* For constructing r_info from field values. */

/*
 * Relocation types.
 */
public static readonly nuint ARM_MAGIC_TRAMP_NUMBER = 0x5c000003;

/*
 * Symbol table entries.
 */

/* For accessing the fields of st_info. */

/* For constructing st_info from field values. */

/* For accessing the fields of st_other. */

/*
 * ELF header.
 */
public partial struct ElfEhdr { // : elf.Header64
}

/*
 * Section header.
 */
public partial struct ElfShdr {
    public ref elf.Section64 Section64 => ref Section64_val;
    public elf.SectionIndex shnum;
}

/*
 * Program header.
 */
public partial struct ElfPhdr { // : elf.ProgHeader
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
public static readonly nint ELF64HDRSIZE = 64;
public static readonly nint ELF64PHDRSIZE = 56;
public static readonly nint ELF64SHDRSIZE = 64;
public static readonly nint ELF64RELSIZE = 16;
public static readonly nint ELF64RELASIZE = 24;
public static readonly nint ELF64SYMSIZE = 24;
public static readonly nint ELF32HDRSIZE = 52;
public static readonly nint ELF32PHDRSIZE = 32;
public static readonly nint ELF32SHDRSIZE = 40;
public static readonly nint ELF32SYMSIZE = 16;
public static readonly nint ELF32RELSIZE = 8;

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
public static readonly nint ELFRESERVE = 4096;

/*
 * We use the 64-bit data structures on both 32- and 64-bit machines
 * in order to write the code just once.  The 64-bit data structure is
 * written in the 32-bit format on the 32-bit machines.
 */
public static readonly nint NSECT = 400;

public static nint Nelfsym = 1;private static bool elf64 = default;private static @string elfRelType = default;private static ElfEhdr ehdr = default;private static array<ptr<ElfPhdr>> phdr = new array<ptr<ElfPhdr>>(NSECT);private static array<ptr<ElfShdr>> shdr = new array<ptr<ElfShdr>>(NSECT);private static @string interp = default;

public partial struct Elfstring {
    public @string s;
    public nint off;
}

private static array<Elfstring> elfstr = new array<Elfstring>(100);

private static nint nelfstr = default;

private static slice<byte> buildinfo = default;

/*
 Initialize the global variable that describes the ELF header. It will be updated as
 we write section and prog headers.
*/
public static void Elfinit(ptr<Link> _addr_ctxt) {
    ref Link ctxt = ref _addr_ctxt.val;

    ctxt.IsELF = true;

    if (ctxt.Arch.InFamily(sys.AMD64, sys.ARM64, sys.MIPS64, sys.PPC64, sys.RISCV64, sys.S390X)) {
        elfRelType = ".rela";
    }
    else
 {
        elfRelType = ".rel";
    }

    // 64-bit architectures
    if (ctxt.Arch.Family == sys.PPC64 || ctxt.Arch.Family == sys.S390X)
    {
        if (ctxt.Arch.ByteOrder == binary.BigEndian) {
            ehdr.Flags = 1; /* Version 1 ABI */
        }
        else
 {
            ehdr.Flags = 2; /* Version 2 ABI */
        }
        fallthrough = true;
    }
    if (fallthrough || ctxt.Arch.Family == sys.AMD64 || ctxt.Arch.Family == sys.ARM64 || ctxt.Arch.Family == sys.MIPS64 || ctxt.Arch.Family == sys.RISCV64)
    {
        if (ctxt.Arch.Family == sys.MIPS64) {
            ehdr.Flags = 0x20000004; /* MIPS 3 CPIC */
        }
        if (ctxt.Arch.Family == sys.RISCV64) {
            ehdr.Flags = 0x4; /* RISCV Float ABI Double */
        }
        elf64 = true;

        ehdr.Phoff = ELF64HDRSIZE; /* Must be ELF64HDRSIZE: first PHdr must follow ELF header */
        ehdr.Shoff = ELF64HDRSIZE; /* Will move as we add PHeaders */
        ehdr.Ehsize = ELF64HDRSIZE; /* Must be ELF64HDRSIZE */
        ehdr.Phentsize = ELF64PHDRSIZE; /* Must be ELF64PHDRSIZE */
        ehdr.Shentsize = ELF64SHDRSIZE;        /* Must be ELF64SHDRSIZE */

        // 32-bit architectures
        goto __switch_break0;
    }
    if (ctxt.Arch.Family == sys.ARM || ctxt.Arch.Family == sys.MIPS)
    {
        if (ctxt.Arch.Family == sys.ARM) { 
            // we use EABI on linux/arm, freebsd/arm, netbsd/arm.
            if (ctxt.HeadType == objabi.Hlinux || ctxt.HeadType == objabi.Hfreebsd || ctxt.HeadType == objabi.Hnetbsd) { 
                // We set a value here that makes no indication of which
                // float ABI the object uses, because this is information
                // used by the dynamic linker to compare executables and
                // shared libraries -- so it only matters for cgo calls, and
                // the information properly comes from the object files
                // produced by the host C compiler. parseArmAttributes in
                // ldelf.go reads that information and updates this field as
                // appropriate.
                ehdr.Flags = 0x5000002; // has entry point, Version5 EABI
            }
        }
        else if (ctxt.Arch.Family == sys.MIPS) {
            ehdr.Flags = 0x50001004; /* MIPS 32 CPIC O32*/
        }
    }
    // default: 
        ehdr.Phoff = ELF32HDRSIZE; 
        /* Must be ELF32HDRSIZE: first PHdr must follow ELF header */
        ehdr.Shoff = ELF32HDRSIZE; /* Will move as we add PHeaders */
        ehdr.Ehsize = ELF32HDRSIZE; /* Must be ELF32HDRSIZE */
        ehdr.Phentsize = ELF32PHDRSIZE; /* Must be ELF32PHDRSIZE */
        ehdr.Shentsize = ELF32SHDRSIZE; /* Must be ELF32SHDRSIZE */

    __switch_break0:;
}

// Make sure PT_LOAD is aligned properly and
// that there is no gap,
// correct ELF loaders will do this implicitly,
// but buggy ELF loaders like the one in some
// versions of QEMU and UPX won't.
private static void fixElfPhdr(ptr<ElfPhdr> _addr_e) {
    ref ElfPhdr e = ref _addr_e.val;

    var frag = int(e.Vaddr & (e.Align - 1));

    e.Off -= uint64(frag);
    e.Vaddr -= uint64(frag);
    e.Paddr -= uint64(frag);
    e.Filesz += uint64(frag);
    e.Memsz += uint64(frag);
}

private static void elf64phdr(ptr<OutBuf> _addr_@out, ptr<ElfPhdr> _addr_e) {
    ref OutBuf @out = ref _addr_@out.val;
    ref ElfPhdr e = ref _addr_e.val;

    if (e.Type == elf.PT_LOAD) {
        fixElfPhdr(_addr_e);
    }
    @out.Write32(uint32(e.Type));
    @out.Write32(uint32(e.Flags));
    @out.Write64(e.Off);
    @out.Write64(e.Vaddr);
    @out.Write64(e.Paddr);
    @out.Write64(e.Filesz);
    @out.Write64(e.Memsz);
    @out.Write64(e.Align);
}

private static void elf32phdr(ptr<OutBuf> _addr_@out, ptr<ElfPhdr> _addr_e) {
    ref OutBuf @out = ref _addr_@out.val;
    ref ElfPhdr e = ref _addr_e.val;

    if (e.Type == elf.PT_LOAD) {
        fixElfPhdr(_addr_e);
    }
    @out.Write32(uint32(e.Type));
    @out.Write32(uint32(e.Off));
    @out.Write32(uint32(e.Vaddr));
    @out.Write32(uint32(e.Paddr));
    @out.Write32(uint32(e.Filesz));
    @out.Write32(uint32(e.Memsz));
    @out.Write32(uint32(e.Flags));
    @out.Write32(uint32(e.Align));
}

private static void elf64shdr(ptr<OutBuf> _addr_@out, ptr<ElfShdr> _addr_e) {
    ref OutBuf @out = ref _addr_@out.val;
    ref ElfShdr e = ref _addr_e.val;

    @out.Write32(e.Name);
    @out.Write32(uint32(e.Type));
    @out.Write64(uint64(e.Flags));
    @out.Write64(e.Addr);
    @out.Write64(e.Off);
    @out.Write64(e.Size);
    @out.Write32(e.Link);
    @out.Write32(e.Info);
    @out.Write64(e.Addralign);
    @out.Write64(e.Entsize);
}

private static void elf32shdr(ptr<OutBuf> _addr_@out, ptr<ElfShdr> _addr_e) {
    ref OutBuf @out = ref _addr_@out.val;
    ref ElfShdr e = ref _addr_e.val;

    @out.Write32(e.Name);
    @out.Write32(uint32(e.Type));
    @out.Write32(uint32(e.Flags));
    @out.Write32(uint32(e.Addr));
    @out.Write32(uint32(e.Off));
    @out.Write32(uint32(e.Size));
    @out.Write32(e.Link);
    @out.Write32(e.Info);
    @out.Write32(uint32(e.Addralign));
    @out.Write32(uint32(e.Entsize));
}

private static uint elfwriteshdrs(ptr<OutBuf> _addr_@out) {
    ref OutBuf @out = ref _addr_@out.val;

    if (elf64) {
        {
            nint i__prev1 = i;

            for (nint i = 0; i < int(ehdr.Shnum); i++) {
                elf64shdr(_addr_out, _addr_shdr[i]);
            }


            i = i__prev1;
        }
        return uint32(ehdr.Shnum) * ELF64SHDRSIZE;
    }
    {
        nint i__prev1 = i;

        for (i = 0; i < int(ehdr.Shnum); i++) {
            elf32shdr(_addr_out, _addr_shdr[i]);
        }

        i = i__prev1;
    }
    return uint32(ehdr.Shnum) * ELF32SHDRSIZE;
}

private static void elfsetstring(ptr<Link> _addr_ctxt, loader.Sym s, @string str, nint off) {
    ref Link ctxt = ref _addr_ctxt.val;

    if (nelfstr >= len(elfstr)) {
        ctxt.Errorf(s, "too many elf strings");
        errorexit();
    }
    elfstr[nelfstr].s = str;
    elfstr[nelfstr].off = off;
    nelfstr++;
}

private static uint elfwritephdrs(ptr<OutBuf> _addr_@out) {
    ref OutBuf @out = ref _addr_@out.val;

    if (elf64) {
        {
            nint i__prev1 = i;

            for (nint i = 0; i < int(ehdr.Phnum); i++) {
                elf64phdr(_addr_out, _addr_phdr[i]);
            }


            i = i__prev1;
        }
        return uint32(ehdr.Phnum) * ELF64PHDRSIZE;
    }
    {
        nint i__prev1 = i;

        for (i = 0; i < int(ehdr.Phnum); i++) {
            elf32phdr(_addr_out, _addr_phdr[i]);
        }

        i = i__prev1;
    }
    return uint32(ehdr.Phnum) * ELF32PHDRSIZE;
}

private static ptr<ElfPhdr> newElfPhdr() {
    ptr<ElfPhdr> e = @new<ElfPhdr>();
    if (ehdr.Phnum >= NSECT) {
        Errorf(null, "too many phdrs");
    }
    else
 {
        phdr[ehdr.Phnum] = e;
        ehdr.Phnum++;
    }
    if (elf64) {
        ehdr.Shoff += ELF64PHDRSIZE;
    }
    else
 {
        ehdr.Shoff += ELF32PHDRSIZE;
    }
    return _addr_e!;
}

private static ptr<ElfShdr> newElfShdr(long name) {
    ptr<ElfShdr> e = @new<ElfShdr>();
    e.Name = uint32(name);
    e.shnum = elf.SectionIndex(ehdr.Shnum);
    if (ehdr.Shnum >= NSECT) {
        Errorf(null, "too many shdrs");
    }
    else
 {
        shdr[ehdr.Shnum] = e;
        ehdr.Shnum++;
    }
    return _addr_e!;
}

private static ptr<ElfEhdr> getElfEhdr() {
    return _addr__addr_ehdr!;
}

private static uint elf64writehdr(ptr<OutBuf> _addr_@out) {
    ref OutBuf @out = ref _addr_@out.val;

    @out.Write(ehdr.Ident[..]);
    @out.Write16(uint16(ehdr.Type));
    @out.Write16(uint16(ehdr.Machine));
    @out.Write32(uint32(ehdr.Version));
    @out.Write64(ehdr.Entry);
    @out.Write64(ehdr.Phoff);
    @out.Write64(ehdr.Shoff);
    @out.Write32(ehdr.Flags);
    @out.Write16(ehdr.Ehsize);
    @out.Write16(ehdr.Phentsize);
    @out.Write16(ehdr.Phnum);
    @out.Write16(ehdr.Shentsize);
    @out.Write16(ehdr.Shnum);
    @out.Write16(ehdr.Shstrndx);
    return ELF64HDRSIZE;
}

private static uint elf32writehdr(ptr<OutBuf> _addr_@out) {
    ref OutBuf @out = ref _addr_@out.val;

    @out.Write(ehdr.Ident[..]);
    @out.Write16(uint16(ehdr.Type));
    @out.Write16(uint16(ehdr.Machine));
    @out.Write32(uint32(ehdr.Version));
    @out.Write32(uint32(ehdr.Entry));
    @out.Write32(uint32(ehdr.Phoff));
    @out.Write32(uint32(ehdr.Shoff));
    @out.Write32(ehdr.Flags);
    @out.Write16(ehdr.Ehsize);
    @out.Write16(ehdr.Phentsize);
    @out.Write16(ehdr.Phnum);
    @out.Write16(ehdr.Shentsize);
    @out.Write16(ehdr.Shnum);
    @out.Write16(ehdr.Shstrndx);
    return ELF32HDRSIZE;
}

private static uint elfwritehdr(ptr<OutBuf> _addr_@out) {
    ref OutBuf @out = ref _addr_@out.val;

    if (elf64) {
        return elf64writehdr(_addr_out);
    }
    return elf32writehdr(_addr_out);
}

/* Taken directly from the definition document for ELF64 */
private static uint elfhash(@string name) {
    uint h = default;
    for (nint i = 0; i < len(name); i++) {
        h = (h << 4) + uint32(name[i]);
        {
            var g = h & 0xf0000000;

            if (g != 0) {
                h ^= g >> 24;
            }

        }
        h &= 0x0fffffff;
    }
    return h;
}

private static void elfWriteDynEntSym(ptr<Link> _addr_ctxt, ptr<loader.SymbolBuilder> _addr_s, elf.DynTag tag, loader.Sym t) {
    ref Link ctxt = ref _addr_ctxt.val;
    ref loader.SymbolBuilder s = ref _addr_s.val;

    Elfwritedynentsymplus(_addr_ctxt, _addr_s, tag, t, 0);
}

public static void Elfwritedynent(ptr<sys.Arch> _addr_arch, ptr<loader.SymbolBuilder> _addr_s, elf.DynTag tag, ulong val) {
    ref sys.Arch arch = ref _addr_arch.val;
    ref loader.SymbolBuilder s = ref _addr_s.val;

    if (elf64) {
        s.AddUint64(arch, uint64(tag));
        s.AddUint64(arch, val);
    }
    else
 {
        s.AddUint32(arch, uint32(tag));
        s.AddUint32(arch, uint32(val));
    }
}

private static void elfwritedynentsym(ptr<Link> _addr_ctxt, ptr<loader.SymbolBuilder> _addr_s, elf.DynTag tag, loader.Sym t) {
    ref Link ctxt = ref _addr_ctxt.val;
    ref loader.SymbolBuilder s = ref _addr_s.val;

    Elfwritedynentsymplus(_addr_ctxt, _addr_s, tag, t, 0);
}

public static void Elfwritedynentsymplus(ptr<Link> _addr_ctxt, ptr<loader.SymbolBuilder> _addr_s, elf.DynTag tag, loader.Sym t, long add) {
    ref Link ctxt = ref _addr_ctxt.val;
    ref loader.SymbolBuilder s = ref _addr_s.val;

    if (elf64) {
        s.AddUint64(ctxt.Arch, uint64(tag));
    }
    else
 {
        s.AddUint32(ctxt.Arch, uint32(tag));
    }
    s.AddAddrPlus(ctxt.Arch, t, add);
}

private static void elfwritedynentsymsize(ptr<Link> _addr_ctxt, ptr<loader.SymbolBuilder> _addr_s, elf.DynTag tag, loader.Sym t) {
    ref Link ctxt = ref _addr_ctxt.val;
    ref loader.SymbolBuilder s = ref _addr_s.val;

    if (elf64) {
        s.AddUint64(ctxt.Arch, uint64(tag));
    }
    else
 {
        s.AddUint32(ctxt.Arch, uint32(tag));
    }
    s.AddSize(ctxt.Arch, t);
}

private static nint elfinterp(ptr<ElfShdr> _addr_sh, ulong startva, ulong resoff, @string p) {
    ref ElfShdr sh = ref _addr_sh.val;

    interp = p;
    var n = len(interp) + 1;
    sh.Addr = startva + resoff - uint64(n);
    sh.Off = resoff - uint64(n);
    sh.Size = uint64(n);

    return n;
}

private static nint elfwriteinterp(ptr<OutBuf> _addr_@out) {
    ref OutBuf @out = ref _addr_@out.val;

    var sh = elfshname(".interp");
    @out.SeekSet(int64(sh.Off));
    @out.WriteString(interp);
    @out.Write8(0);
    return int(sh.Size);
}

// member of .gnu.attributes of MIPS for fpAbi
 
// No floating point is present in the module (default)
public static readonly nint MIPS_FPABI_NONE = 0; 
// FP code in the module uses the FP32 ABI for a 32-bit ABI
public static readonly nint MIPS_FPABI_ANY = 1; 
// FP code in the module only uses single precision ABI
public static readonly nint MIPS_FPABI_SINGLE = 2; 
// FP code in the module uses soft-float ABI
public static readonly nint MIPS_FPABI_SOFT = 3; 
// FP code in the module assumes an FPU with FR=1 and has 12
// callee-saved doubles. Historic, no longer supported.
public static readonly nint MIPS_FPABI_HIST = 4; 
// FP code in the module uses the FPXX  ABI
public static readonly nint MIPS_FPABI_FPXX = 5; 
// FP code in the module uses the FP64  ABI
public static readonly nint MIPS_FPABI_FP64 = 6; 
// FP code in the module uses the FP64A ABI
public static readonly nint MIPS_FPABI_FP64A = 7;

private static nint elfMipsAbiFlags(ptr<ElfShdr> _addr_sh, ulong startva, ulong resoff) {
    ref ElfShdr sh = ref _addr_sh.val;

    nint n = 24;
    sh.Addr = startva + resoff - uint64(n);
    sh.Off = resoff - uint64(n);
    sh.Size = uint64(n);
    sh.Type = uint32(elf.SHT_MIPS_ABIFLAGS);
    sh.Flags = uint64(elf.SHF_ALLOC);

    return n;
}

//typedef struct
//{
//  /* Version of flags structure.  */
//  uint16_t version;
//  /* The level of the ISA: 1-5, 32, 64.  */
//  uint8_t isa_level;
//  /* The revision of ISA: 0 for MIPS V and below, 1-n otherwise.  */
//  uint8_t isa_rev;
//  /* The size of general purpose registers.  */
//  uint8_t gpr_size;
//  /* The size of co-processor 1 registers.  */
//  uint8_t cpr1_size;
//  /* The size of co-processor 2 registers.  */
//  uint8_t cpr2_size;
//  /* The floating-point ABI.  */
//  uint8_t fp_abi;
//  /* Processor-specific extension.  */
//  uint32_t isa_ext;
//  /* Mask of ASEs used.  */
//  uint32_t ases;
//  /* Mask of general flags.  */
//  uint32_t flags1;
//  uint32_t flags2;
//} Elf_Internal_ABIFlags_v0;
private static nint elfWriteMipsAbiFlags(ptr<Link> _addr_ctxt) {
    ref Link ctxt = ref _addr_ctxt.val;

    var sh = elfshname(".MIPS.abiflags");
    ctxt.Out.SeekSet(int64(sh.Off));
    ctxt.Out.Write16(0); // version
    ctxt.Out.Write8(32); // isaLevel
    ctxt.Out.Write8(1); // isaRev
    ctxt.Out.Write8(1); // gprSize
    ctxt.Out.Write8(1); // cpr1Size
    ctxt.Out.Write8(0); // cpr2Size
    if (buildcfg.GOMIPS == "softfloat") {
        ctxt.Out.Write8(MIPS_FPABI_SOFT); // fpAbi
    }
    else
 { 
        // Go cannot make sure non odd-number-fpr is used (ie, in load a double from memory).
        // So, we mark the object is MIPS I style paired float/double register scheme,
        // aka MIPS_FPABI_ANY. If we mark the object as FPXX, the kernel may use FR=1 mode,
        // then we meet some problem.
        // Note: MIPS_FPABI_ANY is bad naming: in fact it is MIPS I style FPR usage.
        //       It is not for 'ANY'.
        // TODO: switch to FPXX after be sure that no odd-number-fpr is used.
        ctxt.Out.Write8(MIPS_FPABI_ANY); // fpAbi
    }
    ctxt.Out.Write32(0); // isaExt
    ctxt.Out.Write32(0); // ases
    ctxt.Out.Write32(0); // flags1
    ctxt.Out.Write32(0); // flags2
    return int(sh.Size);
}

private static nint elfnote(ptr<ElfShdr> _addr_sh, ulong startva, ulong resoff, nint sz) {
    ref ElfShdr sh = ref _addr_sh.val;

    nint n = 3 * 4 + uint64(sz) + resoff % 4;

    sh.Type = uint32(elf.SHT_NOTE);
    sh.Flags = uint64(elf.SHF_ALLOC);
    sh.Addralign = 4;
    sh.Addr = startva + resoff - n;
    sh.Off = resoff - n;
    sh.Size = n - resoff % 4;

    return int(n);
}

private static ptr<ElfShdr> elfwritenotehdr(ptr<OutBuf> _addr_@out, @string str, uint namesz, uint descsz, uint tag) {
    ref OutBuf @out = ref _addr_@out.val;

    var sh = elfshname(str); 

    // Write Elf_Note header.
    @out.SeekSet(int64(sh.Off));

    @out.Write32(namesz);
    @out.Write32(descsz);
    @out.Write32(tag);

    return _addr_sh!;
}

// NetBSD Signature (as per sys/exec_elf.h)
public static readonly nint ELF_NOTE_NETBSD_NAMESZ = 7;
public static readonly nint ELF_NOTE_NETBSD_DESCSZ = 4;
public static readonly nint ELF_NOTE_NETBSD_TAG = 1;
public static readonly nint ELF_NOTE_NETBSD_VERSION = 700000000; /* NetBSD 7.0 */

public static slice<byte> ELF_NOTE_NETBSD_NAME = (slice<byte>)"NetBSD\x00";

private static nint elfnetbsdsig(ptr<ElfShdr> _addr_sh, ulong startva, ulong resoff) {
    ref ElfShdr sh = ref _addr_sh.val;

    var n = int(Rnd(ELF_NOTE_NETBSD_NAMESZ, 4) + Rnd(ELF_NOTE_NETBSD_DESCSZ, 4));
    return elfnote(_addr_sh, startva, resoff, n);
}

private static nint elfwritenetbsdsig(ptr<OutBuf> _addr_@out) {
    ref OutBuf @out = ref _addr_@out.val;
 
    // Write Elf_Note header.
    var sh = elfwritenotehdr(_addr_out, ".note.netbsd.ident", ELF_NOTE_NETBSD_NAMESZ, ELF_NOTE_NETBSD_DESCSZ, ELF_NOTE_NETBSD_TAG);

    if (sh == null) {
        return 0;
    }
    @out.Write(ELF_NOTE_NETBSD_NAME);
    @out.Write8(0);
    @out.Write32(ELF_NOTE_NETBSD_VERSION);

    return int(sh.Size);
}

// The race detector can't handle ASLR (address space layout randomization).
// ASLR is on by default for NetBSD, so we turn the ASLR off explicitly
// using a magic elf Note when building race binaries.

private static nint elfnetbsdpax(ptr<ElfShdr> _addr_sh, ulong startva, ulong resoff) {
    ref ElfShdr sh = ref _addr_sh.val;

    var n = int(Rnd(4, 4) + Rnd(4, 4));
    return elfnote(_addr_sh, startva, resoff, n);
}

private static nint elfwritenetbsdpax(ptr<OutBuf> _addr_@out) {
    ref OutBuf @out = ref _addr_@out.val;

    var sh = elfwritenotehdr(_addr_out, ".note.netbsd.pax", 4, 4, 0x03);
    if (sh == null) {
        return 0;
    }
    @out.Write((slice<byte>)"PaX\x00");
    @out.Write32(0x20); // 0x20 = Force disable ASLR
    return int(sh.Size);
}

// OpenBSD Signature
public static readonly nint ELF_NOTE_OPENBSD_NAMESZ = 8;
public static readonly nint ELF_NOTE_OPENBSD_DESCSZ = 4;
public static readonly nint ELF_NOTE_OPENBSD_TAG = 1;
public static readonly nint ELF_NOTE_OPENBSD_VERSION = 0;

public static slice<byte> ELF_NOTE_OPENBSD_NAME = (slice<byte>)"OpenBSD\x00";

private static nint elfopenbsdsig(ptr<ElfShdr> _addr_sh, ulong startva, ulong resoff) {
    ref ElfShdr sh = ref _addr_sh.val;

    var n = ELF_NOTE_OPENBSD_NAMESZ + ELF_NOTE_OPENBSD_DESCSZ;
    return elfnote(_addr_sh, startva, resoff, n);
}

private static nint elfwriteopenbsdsig(ptr<OutBuf> _addr_@out) {
    ref OutBuf @out = ref _addr_@out.val;
 
    // Write Elf_Note header.
    var sh = elfwritenotehdr(_addr_out, ".note.openbsd.ident", ELF_NOTE_OPENBSD_NAMESZ, ELF_NOTE_OPENBSD_DESCSZ, ELF_NOTE_OPENBSD_TAG);

    if (sh == null) {
        return 0;
    }
    @out.Write(ELF_NOTE_OPENBSD_NAME);

    @out.Write32(ELF_NOTE_OPENBSD_VERSION);

    return int(sh.Size);
}

private static void addbuildinfo(@string val) {
    if (!strings.HasPrefix(val, "0x")) {
        Exitf("-B argument must start with 0x: %s", val);
    }
    var ov = val;
    val = val[(int)2..];

    const nint maxLen = 32;

    if (hex.DecodedLen(len(val)) > maxLen) {
        Exitf("-B option too long (max %d digits): %s", maxLen, ov);
    }
    var (b, err) = hex.DecodeString(val);
    if (err != null) {
        if (err == hex.ErrLength) {
            Exitf("-B argument must have even number of digits: %s", ov);
        }
        {
            hex.InvalidByteError (inv, ok) = err._<hex.InvalidByteError>();

            if (ok) {
                Exitf("-B argument contains invalid hex digit %c: %s", byte(inv), ov);
            }

        }
        Exitf("-B argument contains invalid hex: %s", ov);
    }
    buildinfo = b;
}

// Build info note
public static readonly nint ELF_NOTE_BUILDINFO_NAMESZ = 4;
public static readonly nint ELF_NOTE_BUILDINFO_TAG = 3;

public static slice<byte> ELF_NOTE_BUILDINFO_NAME = (slice<byte>)"GNU\x00";

private static nint elfbuildinfo(ptr<ElfShdr> _addr_sh, ulong startva, ulong resoff) {
    ref ElfShdr sh = ref _addr_sh.val;

    var n = int(ELF_NOTE_BUILDINFO_NAMESZ + Rnd(int64(len(buildinfo)), 4));
    return elfnote(_addr_sh, startva, resoff, n);
}

private static nint elfgobuildid(ptr<ElfShdr> _addr_sh, ulong startva, ulong resoff) {
    ref ElfShdr sh = ref _addr_sh.val;

    var n = len(ELF_NOTE_GO_NAME) + int(Rnd(int64(len(flagBuildid.val)), 4));
    return elfnote(_addr_sh, startva, resoff, n);
}

private static nint elfwritebuildinfo(ptr<OutBuf> _addr_@out) {
    ref OutBuf @out = ref _addr_@out.val;

    var sh = elfwritenotehdr(_addr_out, ".note.gnu.build-id", ELF_NOTE_BUILDINFO_NAMESZ, uint32(len(buildinfo)), ELF_NOTE_BUILDINFO_TAG);
    if (sh == null) {
        return 0;
    }
    @out.Write(ELF_NOTE_BUILDINFO_NAME);
    @out.Write(buildinfo);
    var zero = make_slice<byte>(4);
    @out.Write(zero[..(int)int(Rnd(int64(len(buildinfo)), 4) - int64(len(buildinfo)))]);

    return int(sh.Size);
}

private static nint elfwritegobuildid(ptr<OutBuf> _addr_@out) {
    ref OutBuf @out = ref _addr_@out.val;

    var sh = elfwritenotehdr(_addr_out, ".note.go.buildid", uint32(len(ELF_NOTE_GO_NAME)), uint32(len(flagBuildid.val)), ELF_NOTE_GOBUILDID_TAG);
    if (sh == null) {
        return 0;
    }
    @out.Write(ELF_NOTE_GO_NAME);
    @out.Write((slice<byte>)flagBuildid.val);
    var zero = make_slice<byte>(4);
    @out.Write(zero[..(int)int(Rnd(int64(len(flagBuildid.val)), 4) - int64(len(flagBuildid.val)))]);

    return int(sh.Size);
}

// Go specific notes
public static readonly nint ELF_NOTE_GOPKGLIST_TAG = 1;
public static readonly nint ELF_NOTE_GOABIHASH_TAG = 2;
public static readonly nint ELF_NOTE_GODEPS_TAG = 3;
public static readonly nint ELF_NOTE_GOBUILDID_TAG = 4;

public static slice<byte> ELF_NOTE_GO_NAME = (slice<byte>)"Go\x00\x00";

private static nint elfverneed = default;

public partial struct Elfaux {
    public ptr<Elfaux> next;
    public nint num;
    public @string vers;
}

public partial struct Elflib {
    public ptr<Elflib> next;
    public ptr<Elfaux> aux;
    public @string file;
}

private static ptr<Elfaux> addelflib(ptr<ptr<Elflib>> _addr_list, @string file, @string vers) {
    ref ptr<Elflib> list = ref _addr_list.val;

    ptr<Elflib> lib;

    lib = list.val;

    while (lib != null) {
        if (lib.file == file) {
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

        while (aux != null) {
            if (aux.vers == vers) {
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

private static void elfdynhash(ptr<Link> _addr_ctxt) {
    ref Link ctxt = ref _addr_ctxt.val;

    if (!ctxt.IsELF) {
        return ;
    }
    var nsym = Nelfsym;
    var ldr = ctxt.loader;
    var s = ldr.CreateSymForUpdate(".hash", 0);
    s.SetType(sym.SELFROSECT);

    var i = nsym;
    nint nbucket = 1;
    while (i > 0) {
        nbucket++;
        i>>=1;
    }

    ptr<Elflib> needlib;
    var need = make_slice<ptr<Elfaux>>(nsym);
    var chain = make_slice<uint>(nsym);
    var buckets = make_slice<uint>(nbucket);

    {
        var sy__prev1 = sy;

        foreach (var (_, __sy) in ldr.DynidSyms()) {
            sy = __sy;
            var dynid = ldr.SymDynid(sy);
            if (ldr.SymDynimpvers(sy) != "") {
                need[dynid] = addelflib(_addr_needlib, ldr.SymDynimplib(sy), ldr.SymDynimpvers(sy));
            }
            var name = ldr.SymExtname(sy);
            var hc = elfhash(name);

            var b = hc % uint32(nbucket);
            chain[dynid] = buckets[b];
            buckets[b] = uint32(dynid);
        }
        sy = sy__prev1;
    }

    if (ctxt.Arch.Family == sys.S390X) {
        s.AddUint64(ctxt.Arch, uint64(nbucket));
        s.AddUint64(ctxt.Arch, uint64(nsym));
        {
            var i__prev1 = i;

            for (i = 0; i < nbucket; i++) {
                s.AddUint64(ctxt.Arch, uint64(buckets[i]));
            }
    else


            i = i__prev1;
        }
        {
            var i__prev1 = i;

            for (i = 0; i < nsym; i++) {
                s.AddUint64(ctxt.Arch, uint64(chain[i]));
            }


            i = i__prev1;
        }
    } {
        s.AddUint32(ctxt.Arch, uint32(nbucket));
        s.AddUint32(ctxt.Arch, uint32(nsym));
        {
            var i__prev1 = i;

            for (i = 0; i < nbucket; i++) {
                s.AddUint32(ctxt.Arch, buckets[i]);
            }


            i = i__prev1;
        }
        {
            var i__prev1 = i;

            for (i = 0; i < nsym; i++) {
                s.AddUint32(ctxt.Arch, chain[i]);
            }


            i = i__prev1;
        }
    }
    var dynstr = ldr.CreateSymForUpdate(".dynstr", 0); 

    // version symbols
    var gnuVersionR = ldr.CreateSymForUpdate(".gnu.version_r", 0);
    s = gnuVersionR;
    i = 2;
    nint nfile = 0;
    {
        var l = needlib;

        while (l != null) {
            nfile++; 

            // header
            s.AddUint16(ctxt.Arch, 1); // table version
            nint j = 0;
            {
                var x__prev2 = x;

                var x = l.aux;

                while (x != null) {
                    j++;
                    x = x.next;
                }


                x = x__prev2;
            }
            s.AddUint16(ctxt.Arch, uint16(j)); // aux count
            s.AddUint32(ctxt.Arch, uint32(dynstr.Addstring(l.file))); // file string offset
            s.AddUint32(ctxt.Arch, 16); // offset from header to first aux
            if (l.next != null) {
                s.AddUint32(ctxt.Arch, 16 + uint32(j) * 16); // offset from this header to next
            l = l.next;
            }
            else
 {
                s.AddUint32(ctxt.Arch, 0);
            }
            {
                var x__prev2 = x;

                x = l.aux;

                while (x != null) {
                    x.num = i;
                    i++; 

                    // aux struct
                    s.AddUint32(ctxt.Arch, elfhash(x.vers)); // hash
                    s.AddUint16(ctxt.Arch, 0); // flags
                    s.AddUint16(ctxt.Arch, uint16(x.num)); // other - index we refer to this by
                    s.AddUint32(ctxt.Arch, uint32(dynstr.Addstring(x.vers))); // version string offset
                    if (x.next != null) {
                        s.AddUint32(ctxt.Arch, 16); // offset from this aux to next
                    x = x.next;
                    }
                    else
 {
                        s.AddUint32(ctxt.Arch, 0);
                    }
                }


                x = x__prev2;
            }
        }
    } 

    // version references
    var gnuVersion = ldr.CreateSymForUpdate(".gnu.version", 0);
    s = gnuVersion;

    {
        var i__prev1 = i;

        for (i = 0; i < nsym; i++) {
            if (i == 0) {
                s.AddUint16(ctxt.Arch, 0); // first entry - no symbol
            }
            else if (need[i] == null) {
                s.AddUint16(ctxt.Arch, 1); // global
            }
            else
 {
                s.AddUint16(ctxt.Arch, uint16(need[i].num));
            }
        }

        i = i__prev1;
    }

    s = ldr.CreateSymForUpdate(".dynamic", 0);
    if (ctxt.BuildMode == BuildModePIE) { 
        // https://github.com/bminor/glibc/blob/895ef79e04a953cac1493863bcae29ad85657ee1/elf/elf.h#L986
        const nuint DTFLAGS_1_PIE = 0x08000000;

        Elfwritedynent(_addr_ctxt.Arch, _addr_s, elf.DT_FLAGS_1, uint64(DTFLAGS_1_PIE));
    }
    elfverneed = nfile;
    if (elfverneed != 0) {
        elfWriteDynEntSym(_addr_ctxt, _addr_s, elf.DT_VERNEED, gnuVersionR.Sym());
        Elfwritedynent(_addr_ctxt.Arch, _addr_s, elf.DT_VERNEEDNUM, uint64(nfile));
        elfWriteDynEntSym(_addr_ctxt, _addr_s, elf.DT_VERSYM, gnuVersion.Sym());
    }
    var sy = ldr.CreateSymForUpdate(elfRelType + ".plt", 0);
    if (sy.Size() > 0) {
        if (elfRelType == ".rela") {
            Elfwritedynent(_addr_ctxt.Arch, _addr_s, elf.DT_PLTREL, uint64(elf.DT_RELA));
        }
        else
 {
            Elfwritedynent(_addr_ctxt.Arch, _addr_s, elf.DT_PLTREL, uint64(elf.DT_REL));
        }
        elfwritedynentsymsize(_addr_ctxt, _addr_s, elf.DT_PLTRELSZ, sy.Sym());
        elfWriteDynEntSym(_addr_ctxt, _addr_s, elf.DT_JMPREL, sy.Sym());
    }
    Elfwritedynent(_addr_ctxt.Arch, _addr_s, elf.DT_NULL, 0);
}

private static ptr<ElfPhdr> elfphload(ptr<sym.Segment> _addr_seg) {
    ref sym.Segment seg = ref _addr_seg.val;

    var ph = newElfPhdr();
    ph.Type = elf.PT_LOAD;
    if (seg.Rwx & 4 != 0) {
        ph.Flags |= elf.PF_R;
    }
    if (seg.Rwx & 2 != 0) {
        ph.Flags |= elf.PF_W;
    }
    if (seg.Rwx & 1 != 0) {
        ph.Flags |= elf.PF_X;
    }
    ph.Vaddr = seg.Vaddr;
    ph.Paddr = seg.Vaddr;
    ph.Memsz = seg.Length;
    ph.Off = seg.Fileoff;
    ph.Filesz = seg.Filelen;
    ph.Align = uint64(FlagRound.val);

    return _addr_ph!;
}

private static void elfphrelro(ptr<sym.Segment> _addr_seg) {
    ref sym.Segment seg = ref _addr_seg.val;

    var ph = newElfPhdr();
    ph.Type = elf.PT_GNU_RELRO;
    ph.Vaddr = seg.Vaddr;
    ph.Paddr = seg.Vaddr;
    ph.Memsz = seg.Length;
    ph.Off = seg.Fileoff;
    ph.Filesz = seg.Filelen;
    ph.Align = uint64(FlagRound.val);
}

private static ptr<ElfShdr> elfshname(@string name) {
    for (nint i = 0; i < nelfstr; i++) {
        if (name != elfstr[i].s) {
            continue;
        }
        var off = elfstr[i].off;
        for (i = 0; i < int(ehdr.Shnum); i++) {
            var sh = shdr[i];
            if (sh.Name == uint32(off)) {
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
private static ptr<ElfShdr> elfshnamedup(@string name) {
    for (nint i = 0; i < nelfstr; i++) {
        if (name == elfstr[i].s) {
            var off = elfstr[i].off;
            return _addr_newElfShdr(int64(off))!;
        }
    }

    Errorf(null, "cannot find elf name %s", name);
    errorexit();
    return _addr_null!;
}

private static ptr<ElfShdr> elfshalloc(ptr<sym.Section> _addr_sect) {
    ref sym.Section sect = ref _addr_sect.val;

    var sh = elfshname(sect.Name);
    sect.Elfsect = sh;
    return _addr_sh!;
}

private static ptr<ElfShdr> elfshbits(LinkMode linkmode, ptr<sym.Section> _addr_sect) {
    ref sym.Section sect = ref _addr_sect.val;

    ptr<ElfShdr> sh;

    if (sect.Name == ".text") {
        if (sect.Elfsect == null) {
            sect.Elfsect = elfshnamedup(sect.Name);
        }
        sh = sect.Elfsect._<ptr<ElfShdr>>();
    }
    else
 {
        sh = elfshalloc(_addr_sect);
    }
    if (sh.Type == uint32(elf.SHT_NOTE)) {
        if (linkmode != LinkExternal) { 
            // TODO(mwhudson): the approach here will work OK when
            // linking internally for notes that we want to be included
            // in a loadable segment (e.g. the abihash note) but not for
            // notes that we do not want to be mapped (e.g. the package
            // list note). The real fix is probably to define new values
            // for Symbol.Type corresponding to mapped and unmapped notes
            // and handle them in dodata().
            Errorf(null, "sh.Type == SHT_NOTE in elfshbits when linking internally");
        }
        sh.Addralign = uint64(sect.Align);
        sh.Size = sect.Length;
        sh.Off = sect.Seg.Fileoff + sect.Vaddr - sect.Seg.Vaddr;
        return _addr_sh!;
    }
    if (sh.Type > 0) {
        return _addr_sh!;
    }
    if (sect.Vaddr < sect.Seg.Vaddr + sect.Seg.Filelen) {
        sh.Type = uint32(elf.SHT_PROGBITS);
    }
    else
 {
        sh.Type = uint32(elf.SHT_NOBITS);
    }
    sh.Flags = uint64(elf.SHF_ALLOC);
    if (sect.Rwx & 1 != 0) {
        sh.Flags |= uint64(elf.SHF_EXECINSTR);
    }
    if (sect.Rwx & 2 != 0) {
        sh.Flags |= uint64(elf.SHF_WRITE);
    }
    if (sect.Name == ".tbss") {
        sh.Flags |= uint64(elf.SHF_TLS);
        sh.Type = uint32(elf.SHT_NOBITS);
    }
    if (strings.HasPrefix(sect.Name, ".debug") || strings.HasPrefix(sect.Name, ".zdebug")) {
        sh.Flags = 0;
    }
    if (linkmode != LinkExternal) {
        sh.Addr = sect.Vaddr;
    }
    sh.Addralign = uint64(sect.Align);
    sh.Size = sect.Length;
    if (sect.Name != ".tbss") {
        sh.Off = sect.Seg.Fileoff + sect.Vaddr - sect.Seg.Vaddr;
    }
    return _addr_sh!;
}

private static ptr<ElfShdr> elfshreloc(ptr<sys.Arch> _addr_arch, ptr<sym.Section> _addr_sect) {
    ref sys.Arch arch = ref _addr_arch.val;
    ref sym.Section sect = ref _addr_sect.val;
 
    // If main section is SHT_NOBITS, nothing to relocate.
    // Also nothing to relocate in .shstrtab or notes.
    if (sect.Vaddr >= sect.Seg.Vaddr + sect.Seg.Filelen) {
        return _addr_null!;
    }
    if (sect.Name == ".shstrtab" || sect.Name == ".tbss") {
        return _addr_null!;
    }
    if (sect.Elfsect._<ptr<ElfShdr>>().Type == uint32(elf.SHT_NOTE)) {
        return _addr_null!;
    }
    var typ = elf.SHT_REL;
    if (elfRelType == ".rela") {
        typ = elf.SHT_RELA;
    }
    var sh = elfshname(elfRelType + sect.Name); 
    // There could be multiple text sections but each needs
    // its own .rela.text.

    if (sect.Name == ".text") {
        if (sh.Info != 0 && sh.Info != uint32(sect.Elfsect._<ptr<ElfShdr>>().shnum)) {
            sh = elfshnamedup(elfRelType + sect.Name);
        }
    }
    sh.Type = uint32(typ);
    sh.Entsize = uint64(arch.RegSize) * 2;
    if (typ == elf.SHT_RELA) {
        sh.Entsize += uint64(arch.RegSize);
    }
    sh.Link = uint32(elfshname(".symtab").shnum);
    sh.Info = uint32(sect.Elfsect._<ptr<ElfShdr>>().shnum);
    sh.Off = sect.Reloff;
    sh.Size = sect.Rellen;
    sh.Addralign = uint64(arch.RegSize);
    return _addr_sh!;
}

private static void elfrelocsect(ptr<Link> _addr_ctxt, ptr<OutBuf> _addr_@out, ptr<sym.Section> _addr_sect, slice<loader.Sym> syms) => func((_, panic, _) => {
    ref Link ctxt = ref _addr_ctxt.val;
    ref OutBuf @out = ref _addr_@out.val;
    ref sym.Section sect = ref _addr_sect.val;
 
    // If main section is SHT_NOBITS, nothing to relocate.
    // Also nothing to relocate in .shstrtab.
    if (sect.Vaddr >= sect.Seg.Vaddr + sect.Seg.Filelen) {
        return ;
    }
    if (sect.Name == ".shstrtab") {
        return ;
    }
    var ldr = ctxt.loader;
    {
        var s__prev1 = s;

        foreach (var (__i, __s) in syms) {
            i = __i;
            s = __s;
            if (!ldr.AttrReachable(s)) {
                panic("should never happen");
            }
            if (uint64(ldr.SymValue(s)) >= sect.Vaddr) {
                syms = syms[(int)i..];
                break;
            }
        }
        s = s__prev1;
    }

    var eaddr = sect.Vaddr + sect.Length;
    {
        var s__prev1 = s;

        foreach (var (_, __s) in syms) {
            s = __s;
            if (!ldr.AttrReachable(s)) {
                continue;
            }
            if (ldr.SymValue(s) >= int64(eaddr)) {
                break;
            } 

            // Compute external relocations on the go, and pass to Elfreloc1
            // to stream out.
            var relocs = ldr.Relocs(s);
            for (nint ri = 0; ri < relocs.Count(); ri++) {
                var r = relocs.At(ri);
                var (rr, ok) = extreloc(ctxt, ldr, s, r);
                if (!ok) {
                    continue;
                }
                if (rr.Xsym == 0) {
                    ldr.Errorf(s, "missing xsym in relocation");
                    continue;
                }
                var esr = ElfSymForReloc(ctxt, rr.Xsym);
                if (esr == 0) {
                    ldr.Errorf(s, "reloc %d (%s) to non-elf symbol %s (outer=%s) %d (%s)", r.Type(), sym.RelocName(ctxt.Arch, r.Type()), ldr.SymName(r.Sym()), ldr.SymName(rr.Xsym), ldr.SymType(r.Sym()), ldr.SymType(r.Sym()).String());
                }
                if (!ldr.AttrReachable(rr.Xsym)) {
                    ldr.Errorf(s, "unreachable reloc %d (%s) target %v", r.Type(), sym.RelocName(ctxt.Arch, r.Type()), ldr.SymName(rr.Xsym));
                }
                if (!thearch.Elfreloc1(ctxt, out, ldr, s, rr, ri, int64(uint64(ldr.SymValue(s) + int64(r.Off())) - sect.Vaddr))) {
                    ldr.Errorf(s, "unsupported obj reloc %d (%s)/%d to %s", r.Type(), sym.RelocName(ctxt.Arch, r.Type()), r.Siz(), ldr.SymName(r.Sym()));
                }
            }
        }
        s = s__prev1;
    }

    if (uint64(@out.Offset()) != sect.Reloff + sect.Rellen) {
        panic(fmt.Sprintf("elfrelocsect: size mismatch %d != %d + %d", @out.Offset(), sect.Reloff, sect.Rellen));
    }
});

private static void elfEmitReloc(ptr<Link> _addr_ctxt) => func((_, panic, _) => {
    ref Link ctxt = ref _addr_ctxt.val;

    while (ctxt.Out.Offset() & 7 != 0) {
        ctxt.Out.Write8(0);
    }

    sizeExtRelocs(ctxt, thearch.ElfrelocSize);
    var (relocSect, wg) = relocSectFn(ctxt, elfrelocsect);

    {
        var sect__prev1 = sect;

        foreach (var (_, __sect) in Segtext.Sections) {
            sect = __sect;
            if (sect.Name == ".text") {
                relocSect(ctxt, sect, ctxt.Textp);
            }
            else
 {
                relocSect(ctxt, sect, ctxt.datap);
            }
        }
        sect = sect__prev1;
    }

    {
        var sect__prev1 = sect;

        foreach (var (_, __sect) in Segrodata.Sections) {
            sect = __sect;
            relocSect(ctxt, sect, ctxt.datap);
        }
        sect = sect__prev1;
    }

    {
        var sect__prev1 = sect;

        foreach (var (_, __sect) in Segrelrodata.Sections) {
            sect = __sect;
            relocSect(ctxt, sect, ctxt.datap);
        }
        sect = sect__prev1;
    }

    {
        var sect__prev1 = sect;

        foreach (var (_, __sect) in Segdata.Sections) {
            sect = __sect;
            relocSect(ctxt, sect, ctxt.datap);
        }
        sect = sect__prev1;
    }

    for (nint i = 0; i < len(Segdwarf.Sections); i++) {
        var sect = Segdwarf.Sections[i];
        var si = dwarfp[i];
        if (si.secSym() != loader.Sym(sect.Sym) || ctxt.loader.SymSect(si.secSym()) != sect) {
            panic("inconsistency between dwarfp and Segdwarf");
        }
        relocSect(ctxt, sect, si.syms);
    }
    wg.Wait();
});

private static void addgonote(ptr<Link> _addr_ctxt, @string sectionName, uint tag, slice<byte> desc) {
    ref Link ctxt = ref _addr_ctxt.val;

    var ldr = ctxt.loader;
    var s = ldr.CreateSymForUpdate(sectionName, 0);
    s.SetType(sym.SELFROSECT); 
    // namesz
    s.AddUint32(ctxt.Arch, uint32(len(ELF_NOTE_GO_NAME))); 
    // descsz
    s.AddUint32(ctxt.Arch, uint32(len(desc))); 
    // tag
    s.AddUint32(ctxt.Arch, tag); 
    // name + padding
    s.AddBytes(ELF_NOTE_GO_NAME);
    while (len(s.Data()) % 4 != 0) {
        s.AddUint8(0);
    } 
    // desc + padding
    s.AddBytes(desc);
    while (len(s.Data()) % 4 != 0) {
        s.AddUint8(0);
    }
    s.SetSize(int64(len(s.Data())));
    s.SetAlign(4);
}

private static void doelf(this ptr<Link> _addr_ctxt) {
    ref Link ctxt = ref _addr_ctxt.val;

    var ldr = ctxt.loader; 

    /* predefine strings we need for section headers */
    var shstrtab = ldr.CreateSymForUpdate(".shstrtab", 0);

    shstrtab.SetType(sym.SELFROSECT);

    shstrtab.Addstring("");
    shstrtab.Addstring(".text");
    shstrtab.Addstring(".noptrdata");
    shstrtab.Addstring(".data");
    shstrtab.Addstring(".bss");
    shstrtab.Addstring(".noptrbss");
    shstrtab.Addstring("__libfuzzer_extra_counters");
    shstrtab.Addstring(".go.buildinfo");
    if (ctxt.IsMIPS()) {
        shstrtab.Addstring(".MIPS.abiflags");
        shstrtab.Addstring(".gnu.attributes");
    }
    if (!FlagD || ctxt.IsExternal().val) {
        shstrtab.Addstring(".tbss");
    }
    if (ctxt.IsNetbsd()) {
        shstrtab.Addstring(".note.netbsd.ident");
        if (flagRace.val) {
            shstrtab.Addstring(".note.netbsd.pax");
        }
    }
    if (ctxt.IsOpenbsd()) {
        shstrtab.Addstring(".note.openbsd.ident");
    }
    if (len(buildinfo) > 0) {
        shstrtab.Addstring(".note.gnu.build-id");
    }
    if (flagBuildid != "".val) {
        shstrtab.Addstring(".note.go.buildid");
    }
    shstrtab.Addstring(".elfdata");
    shstrtab.Addstring(".rodata"); 
    // See the comment about data.rel.ro.FOO section names in data.go.
    @string relro_prefix = "";
    if (ctxt.UseRelro()) {
        shstrtab.Addstring(".data.rel.ro");
        relro_prefix = ".data.rel.ro";
    }
    shstrtab.Addstring(relro_prefix + ".typelink");
    shstrtab.Addstring(relro_prefix + ".itablink");
    shstrtab.Addstring(relro_prefix + ".gosymtab");
    shstrtab.Addstring(relro_prefix + ".gopclntab");

    if (ctxt.IsExternal()) {
        FlagD.val = true;

        shstrtab.Addstring(elfRelType + ".text");
        shstrtab.Addstring(elfRelType + ".rodata");
        shstrtab.Addstring(elfRelType + relro_prefix + ".typelink");
        shstrtab.Addstring(elfRelType + relro_prefix + ".itablink");
        shstrtab.Addstring(elfRelType + relro_prefix + ".gosymtab");
        shstrtab.Addstring(elfRelType + relro_prefix + ".gopclntab");
        shstrtab.Addstring(elfRelType + ".noptrdata");
        shstrtab.Addstring(elfRelType + ".data");
        if (ctxt.UseRelro()) {
            shstrtab.Addstring(elfRelType + ".data.rel.ro");
        }
        shstrtab.Addstring(elfRelType + ".go.buildinfo");
        if (ctxt.IsMIPS()) {
            shstrtab.Addstring(elfRelType + ".MIPS.abiflags");
            shstrtab.Addstring(elfRelType + ".gnu.attributes");
        }
        shstrtab.Addstring(".note.GNU-stack");

        if (ctxt.IsShared()) {
            shstrtab.Addstring(".note.go.abihash");
            shstrtab.Addstring(".note.go.pkg-list");
            shstrtab.Addstring(".note.go.deps");
        }
    }
    var hasinitarr = ctxt.linkShared; 

    /* shared library initializer */

    if (ctxt.BuildMode == BuildModeCArchive || ctxt.BuildMode == BuildModeCShared || ctxt.BuildMode == BuildModeShared || ctxt.BuildMode == BuildModePlugin) 
        hasinitarr = true;
        if (hasinitarr) {
        shstrtab.Addstring(".init_array");
        shstrtab.Addstring(elfRelType + ".init_array");
    }
    if (!FlagS.val) {
        shstrtab.Addstring(".symtab");
        shstrtab.Addstring(".strtab");
        dwarfaddshstrings(ctxt, shstrtab);
    }
    shstrtab.Addstring(".shstrtab");

    if (!FlagD.val) { /* -d suppresses dynamic loader format */
        shstrtab.Addstring(".interp");
        shstrtab.Addstring(".hash");
        shstrtab.Addstring(".got");
        if (ctxt.IsPPC64()) {
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
        var dynsym = ldr.CreateSymForUpdate(".dynsym", 0);

        dynsym.SetType(sym.SELFROSECT);
        if (elf64) {
            dynsym.SetSize(dynsym.Size() + ELF64SYMSIZE);
        }
        else
 {
            dynsym.SetSize(dynsym.Size() + ELF32SYMSIZE);
        }
        var dynstr = ldr.CreateSymForUpdate(".dynstr", 0);

        dynstr.SetType(sym.SELFROSECT);
        if (dynstr.Size() == 0) {
            dynstr.Addstring("");
        }
        var s = ldr.CreateSymForUpdate(elfRelType, 0);
        s.SetType(sym.SELFROSECT); 

        /* global offset table */
        var got = ldr.CreateSymForUpdate(".got", 0);
        got.SetType(sym.SELFGOT); // writable

        /* ppc64 glink resolver */
        if (ctxt.IsPPC64()) {
            s = ldr.CreateSymForUpdate(".glink", 0);
            s.SetType(sym.SELFRXSECT);
        }
        var hash = ldr.CreateSymForUpdate(".hash", 0);
        hash.SetType(sym.SELFROSECT);

        var gotplt = ldr.CreateSymForUpdate(".got.plt", 0);
        gotplt.SetType(sym.SELFSECT); // writable

        var plt = ldr.CreateSymForUpdate(".plt", 0);
        if (ctxt.IsPPC64()) { 
            // In the ppc64 ABI, .plt is a data section
            // written by the dynamic linker.
            plt.SetType(sym.SELFSECT);
        }
        else
 {
            plt.SetType(sym.SELFRXSECT);
        }
        s = ldr.CreateSymForUpdate(elfRelType + ".plt", 0);
        s.SetType(sym.SELFROSECT);

        s = ldr.CreateSymForUpdate(".gnu.version", 0);
        s.SetType(sym.SELFROSECT);

        s = ldr.CreateSymForUpdate(".gnu.version_r", 0);
        s.SetType(sym.SELFROSECT); 

        /* define dynamic elf table */
        var dynamic = ldr.CreateSymForUpdate(".dynamic", 0);
        dynamic.SetType(sym.SELFSECT); // writable

        if (ctxt.IsS390X()) { 
            // S390X uses .got instead of .got.plt
            gotplt = got;
        }
        thearch.Elfsetupplt(ctxt, plt, gotplt, dynamic.Sym());

        /*
                 * .dynamic table
                 */
        elfwritedynentsym(_addr_ctxt, _addr_dynamic, elf.DT_HASH, hash.Sym());

        elfwritedynentsym(_addr_ctxt, _addr_dynamic, elf.DT_SYMTAB, dynsym.Sym());
        if (elf64) {
            Elfwritedynent(_addr_ctxt.Arch, _addr_dynamic, elf.DT_SYMENT, ELF64SYMSIZE);
        }
        else
 {
            Elfwritedynent(_addr_ctxt.Arch, _addr_dynamic, elf.DT_SYMENT, ELF32SYMSIZE);
        }
        elfwritedynentsym(_addr_ctxt, _addr_dynamic, elf.DT_STRTAB, dynstr.Sym());
        elfwritedynentsymsize(_addr_ctxt, _addr_dynamic, elf.DT_STRSZ, dynstr.Sym());
        if (elfRelType == ".rela") {
            var rela = ldr.LookupOrCreateSym(".rela", 0);
            elfwritedynentsym(_addr_ctxt, _addr_dynamic, elf.DT_RELA, rela);
            elfwritedynentsymsize(_addr_ctxt, _addr_dynamic, elf.DT_RELASZ, rela);
            Elfwritedynent(_addr_ctxt.Arch, _addr_dynamic, elf.DT_RELAENT, ELF64RELASIZE);
        }
        else
 {
            var rel = ldr.LookupOrCreateSym(".rel", 0);
            elfwritedynentsym(_addr_ctxt, _addr_dynamic, elf.DT_REL, rel);
            elfwritedynentsymsize(_addr_ctxt, _addr_dynamic, elf.DT_RELSZ, rel);
            Elfwritedynent(_addr_ctxt.Arch, _addr_dynamic, elf.DT_RELENT, ELF32RELSIZE);
        }
        if (rpath.val != "") {
            Elfwritedynent(_addr_ctxt.Arch, _addr_dynamic, elf.DT_RUNPATH, uint64(dynstr.Addstring(rpath.val)));
        }
        if (ctxt.IsPPC64()) {
            elfwritedynentsym(_addr_ctxt, _addr_dynamic, elf.DT_PLTGOT, plt.Sym());
        }
        else
 {
            elfwritedynentsym(_addr_ctxt, _addr_dynamic, elf.DT_PLTGOT, gotplt.Sym());
        }
        if (ctxt.IsPPC64()) {
            Elfwritedynent(_addr_ctxt.Arch, _addr_dynamic, elf.DT_PPC64_OPT, 0);
        }
        Elfwritedynent(_addr_ctxt.Arch, _addr_dynamic, elf.DT_DEBUG, 0);
    }
    if (ctxt.IsShared()) { 
        // The go.link.abihashbytes symbol will be pointed at the appropriate
        // part of the .note.go.abihash section in data.go:func address().
        s = ldr.LookupOrCreateSym("go.link.abihashbytes", 0);
        var sb = ldr.MakeSymbolUpdater(s);
        ldr.SetAttrLocal(s, true);
        sb.SetType(sym.SRODATA);
        ldr.SetAttrSpecial(s, true);
        sb.SetReachable(true);
        sb.SetSize(sha1.Size);

        sort.Sort(byPkg(ctxt.Library));
        var h = sha1.New();
        foreach (var (_, l) in ctxt.Library) {
            h.Write(l.Fingerprint[..]);
        }        addgonote(_addr_ctxt, ".note.go.abihash", ELF_NOTE_GOABIHASH_TAG, h.Sum(new slice<byte>(new byte[] {  })));
        addgonote(_addr_ctxt, ".note.go.pkg-list", ELF_NOTE_GOPKGLIST_TAG, pkglistfornote);
        slice<@string> deplist = default;
        foreach (var (_, shlib) in ctxt.Shlibs) {
            deplist = append(deplist, filepath.Base(shlib.Path));
        }        addgonote(_addr_ctxt, ".note.go.deps", ELF_NOTE_GODEPS_TAG, (slice<byte>)strings.Join(deplist, "\n"));
    }
    if (ctxt.LinkMode == LinkExternal && flagBuildid != "".val) {
        addgonote(_addr_ctxt, ".note.go.buildid", ELF_NOTE_GOBUILDID_TAG, (slice<byte>)flagBuildid.val);
    }
    if (ctxt.IsMIPS()) {
        var gnuattributes = ldr.CreateSymForUpdate(".gnu.attributes", 0);
        gnuattributes.SetType(sym.SELFROSECT);
        gnuattributes.SetReachable(true);
        gnuattributes.AddUint8('A'); // version 'A'
        gnuattributes.AddUint32(ctxt.Arch, 15); // length 15 including itself
        gnuattributes.AddBytes((slice<byte>)"gnu\x00"); // "gnu\0"
        gnuattributes.AddUint8(1); // 1:file, 2: section, 3: symbol, 1 here
        gnuattributes.AddUint32(ctxt.Arch, 7); // tag length, including tag, 7 here
        gnuattributes.AddUint8(4); // 4 for FP, 8 for MSA
        if (buildcfg.GOMIPS == "softfloat") {
            gnuattributes.AddUint8(MIPS_FPABI_SOFT);
        }
        else
 { 
            // Note: MIPS_FPABI_ANY is bad naming: in fact it is MIPS I style FPR usage.
            //       It is not for 'ANY'.
            // TODO: switch to FPXX after be sure that no odd-number-fpr is used.
            gnuattributes.AddUint8(MIPS_FPABI_ANY);
        }
    }
}

// Do not write DT_NULL.  elfdynhash will finish it.
private static void shsym(ptr<ElfShdr> _addr_sh, ptr<loader.Loader> _addr_ldr, loader.Sym s) => func((_, panic, _) => {
    ref ElfShdr sh = ref _addr_sh.val;
    ref loader.Loader ldr = ref _addr_ldr.val;

    if (s == 0) {
        panic("bad symbol in shsym2");
    }
    var addr = ldr.SymValue(s);
    if (sh.Flags & uint64(elf.SHF_ALLOC) != 0) {
        sh.Addr = uint64(addr);
    }
    sh.Off = uint64(datoff(ldr, s, addr));
    sh.Size = uint64(ldr.SymSize(s));
});

private static void phsh(ptr<ElfPhdr> _addr_ph, ptr<ElfShdr> _addr_sh) {
    ref ElfPhdr ph = ref _addr_ph.val;
    ref ElfShdr sh = ref _addr_sh.val;

    ph.Vaddr = sh.Addr;
    ph.Paddr = ph.Vaddr;
    ph.Off = sh.Off;
    ph.Filesz = sh.Size;
    ph.Memsz = sh.Size;
    ph.Align = sh.Addralign;
}

public static void Asmbelfsetup() { 
    /* This null SHdr must appear before all others */
    elfshname("");

    {
        var sect__prev1 = sect;

        foreach (var (_, __sect) in Segtext.Sections) {
            sect = __sect; 
            // There could be multiple .text sections. Instead check the Elfsect
            // field to determine if already has an ElfShdr and if not, create one.
            if (sect.Name == ".text") {
                if (sect.Elfsect == null) {
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

        foreach (var (_, __sect) in Segrodata.Sections) {
            sect = __sect;
            elfshalloc(_addr_sect);
        }
        sect = sect__prev1;
    }

    {
        var sect__prev1 = sect;

        foreach (var (_, __sect) in Segrelrodata.Sections) {
            sect = __sect;
            elfshalloc(_addr_sect);
        }
        sect = sect__prev1;
    }

    {
        var sect__prev1 = sect;

        foreach (var (_, __sect) in Segdata.Sections) {
            sect = __sect;
            elfshalloc(_addr_sect);
        }
        sect = sect__prev1;
    }

    {
        var sect__prev1 = sect;

        foreach (var (_, __sect) in Segdwarf.Sections) {
            sect = __sect;
            elfshalloc(_addr_sect);
        }
        sect = sect__prev1;
    }
}

private static void asmbElf(ptr<Link> _addr_ctxt) {
    ref Link ctxt = ref _addr_ctxt.val;

    long symo = default;
    if (!FlagS.val) {
        symo = int64(Segdwarf.Fileoff + Segdwarf.Filelen);
        symo = Rnd(symo, int64(ctxt.Arch.PtrSize));
        ctxt.Out.SeekSet(symo);
        asmElfSym(ctxt);
        ctxt.Out.Write(Elfstrdat);
        if (ctxt.IsExternal()) {
            elfEmitReloc(_addr_ctxt);
        }
    }
    ctxt.Out.SeekSet(0);

    var ldr = ctxt.loader;
    var eh = getElfEhdr();

    if (ctxt.Arch.Family == sys.MIPS || ctxt.Arch.Family == sys.MIPS64) 
        eh.Machine = uint16(elf.EM_MIPS);
    else if (ctxt.Arch.Family == sys.ARM) 
        eh.Machine = uint16(elf.EM_ARM);
    else if (ctxt.Arch.Family == sys.AMD64) 
        eh.Machine = uint16(elf.EM_X86_64);
    else if (ctxt.Arch.Family == sys.ARM64) 
        eh.Machine = uint16(elf.EM_AARCH64);
    else if (ctxt.Arch.Family == sys.I386) 
        eh.Machine = uint16(elf.EM_386);
    else if (ctxt.Arch.Family == sys.PPC64) 
        eh.Machine = uint16(elf.EM_PPC64);
    else if (ctxt.Arch.Family == sys.RISCV64) 
        eh.Machine = uint16(elf.EM_RISCV);
    else if (ctxt.Arch.Family == sys.S390X) 
        eh.Machine = uint16(elf.EM_S390);
    else 
        Exitf("unknown architecture in asmbelf: %v", ctxt.Arch.Family);
        var elfreserve = int64(ELFRESERVE);

    var numtext = int64(0);
    {
        var sect__prev1 = sect;

        foreach (var (_, __sect) in Segtext.Sections) {
            sect = __sect;
            if (sect.Name == ".text") {
                numtext++;
            }
        }
        sect = sect__prev1;
    }

    if (numtext > 4) {
        elfreserve += elfreserve + numtext * 64 * 2;
    }
    var startva = FlagTextAddr - int64(HEADR).val;
    var resoff = elfreserve;

    ptr<ElfPhdr> pph;
    ptr<ElfPhdr> pnote;
    if (flagRace && ctxt.IsNetbsd().val) {
        var sh = elfshname(".note.netbsd.pax");
        resoff -= int64(elfnetbsdpax(_addr_sh, uint64(startva), uint64(resoff)));
        pnote = newElfPhdr();
        pnote.Type = elf.PT_NOTE;
        pnote.Flags = elf.PF_R;
        phsh(pnote, _addr_sh);
    }
    if (ctxt.LinkMode == LinkExternal) { 
        /* skip program headers */
        eh.Phoff = 0;

        eh.Phentsize = 0;

        if (ctxt.BuildMode == BuildModeShared) {
            sh = elfshname(".note.go.pkg-list");
            sh.Type = uint32(elf.SHT_NOTE);
            sh = elfshname(".note.go.abihash");
            sh.Type = uint32(elf.SHT_NOTE);
            sh.Flags = uint64(elf.SHF_ALLOC);
            sh = elfshname(".note.go.deps");
            sh.Type = uint32(elf.SHT_NOTE);
        }
        if (flagBuildid != "".val) {
            sh = elfshname(".note.go.buildid");
            sh.Type = uint32(elf.SHT_NOTE);
            sh.Flags = uint64(elf.SHF_ALLOC);
        }
        goto elfobj;
    }
    pph = newElfPhdr();

    pph.Type = elf.PT_PHDR;
    pph.Flags = elf.PF_R;
    pph.Off = uint64(eh.Ehsize);
    pph.Vaddr = uint64(FlagTextAddr.val) - uint64(HEADR) + pph.Off;
    pph.Paddr = uint64(FlagTextAddr.val) - uint64(HEADR) + pph.Off;
    pph.Align = uint64(FlagRound.val);

    /*
         * PHDR must be in a loaded segment. Adjust the text
         * segment boundaries downwards to include it.
         */
 {
        var o = int64(Segtext.Vaddr - pph.Vaddr);
        Segtext.Vaddr -= uint64(o);
        Segtext.Length += uint64(o);
        o = int64(Segtext.Fileoff - pph.Off);
        Segtext.Fileoff -= uint64(o);
        Segtext.Filelen += uint64(o);
    }    if (!FlagD.val) {        /* -d suppresses dynamic loader format */
        /* interpreter */
        sh = elfshname(".interp");

        sh.Type = uint32(elf.SHT_PROGBITS);
        sh.Flags = uint64(elf.SHF_ALLOC);
        sh.Addralign = 1;

        if (interpreter == "" && buildcfg.GOOS == runtime.GOOS && buildcfg.GOARCH == runtime.GOARCH && buildcfg.GO_LDSO != "") {
            interpreter = buildcfg.GO_LDSO;
        }
        if (interpreter == "") {

            if (ctxt.HeadType == objabi.Hlinux) 
                if (buildcfg.GOOS == "android") {
                    interpreter = thearch.Androiddynld;
                    if (interpreter == "") {
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
        ph.Type = elf.PT_INTERP;
        ph.Flags = elf.PF_R;
        phsh(_addr_ph, _addr_sh);
    }
    pnote = null;
    if (ctxt.HeadType == objabi.Hnetbsd || ctxt.HeadType == objabi.Hopenbsd) {
        sh = ;

        if (ctxt.HeadType == objabi.Hnetbsd) 
            sh = elfshname(".note.netbsd.ident");
            resoff -= int64(elfnetbsdsig(_addr_sh, uint64(startva), uint64(resoff)));
        else if (ctxt.HeadType == objabi.Hopenbsd) 
            sh = elfshname(".note.openbsd.ident");
            resoff -= int64(elfopenbsdsig(_addr_sh, uint64(startva), uint64(resoff)));
                pnote = newElfPhdr();
        pnote.Type = elf.PT_NOTE;
        pnote.Flags = elf.PF_R;
        phsh(pnote, _addr_sh);
    }
    if (len(buildinfo) > 0) {
        sh = elfshname(".note.gnu.build-id");
        resoff -= int64(elfbuildinfo(_addr_sh, uint64(startva), uint64(resoff)));

        if (pnote == null) {
            pnote = newElfPhdr();
            pnote.Type = elf.PT_NOTE;
            pnote.Flags = elf.PF_R;
        }
        phsh(pnote, _addr_sh);
    }
    if (flagBuildid != "".val) {
        sh = elfshname(".note.go.buildid");
        resoff -= int64(elfgobuildid(_addr_sh, uint64(startva), uint64(resoff)));

        pnote = newElfPhdr();
        pnote.Type = elf.PT_NOTE;
        pnote.Flags = elf.PF_R;
        phsh(pnote, _addr_sh);
    }
    elfphload(_addr_Segtext);
    if (len(Segrodata.Sections) > 0) {
        elfphload(_addr_Segrodata);
    }
    if (len(Segrelrodata.Sections) > 0) {
        elfphload(_addr_Segrelrodata);
        elfphrelro(_addr_Segrelrodata);
    }
    elfphload(_addr_Segdata); 

    /* Dynamic linking sections */
    if (!FlagD.val) {
        sh = elfshname(".dynsym");
        sh.Type = uint32(elf.SHT_DYNSYM);
        sh.Flags = uint64(elf.SHF_ALLOC);
        if (elf64) {
            sh.Entsize = ELF64SYMSIZE;
        }
        else
 {
            sh.Entsize = ELF32SYMSIZE;
        }
        sh.Addralign = uint64(ctxt.Arch.RegSize);
        sh.Link = uint32(elfshname(".dynstr").shnum); 

        // sh.info is the index of first non-local symbol (number of local symbols)
        var s = ldr.Lookup(".dynsym", 0);
        var i = uint32(0);
        {
            var sub = s;

            while (sub != 0) {
                i++;
                if (!ldr.AttrLocal(sub)) {
                    break;
                sub = ldr.SubSym(sub);
                }
            }

        }
        sh.Info = i;
        shsym(_addr_sh, _addr_ldr, s);

        sh = elfshname(".dynstr");
        sh.Type = uint32(elf.SHT_STRTAB);
        sh.Flags = uint64(elf.SHF_ALLOC);
        sh.Addralign = 1;
        shsym(_addr_sh, _addr_ldr, ldr.Lookup(".dynstr", 0));

        if (elfverneed != 0) {
            sh = elfshname(".gnu.version");
            sh.Type = uint32(elf.SHT_GNU_VERSYM);
            sh.Flags = uint64(elf.SHF_ALLOC);
            sh.Addralign = 2;
            sh.Link = uint32(elfshname(".dynsym").shnum);
            sh.Entsize = 2;
            shsym(_addr_sh, _addr_ldr, ldr.Lookup(".gnu.version", 0));

            sh = elfshname(".gnu.version_r");
            sh.Type = uint32(elf.SHT_GNU_VERNEED);
            sh.Flags = uint64(elf.SHF_ALLOC);
            sh.Addralign = uint64(ctxt.Arch.RegSize);
            sh.Info = uint32(elfverneed);
            sh.Link = uint32(elfshname(".dynstr").shnum);
            shsym(_addr_sh, _addr_ldr, ldr.Lookup(".gnu.version_r", 0));
        }
        if (elfRelType == ".rela") {
            sh = elfshname(".rela.plt");
            sh.Type = uint32(elf.SHT_RELA);
            sh.Flags = uint64(elf.SHF_ALLOC);
            sh.Entsize = ELF64RELASIZE;
            sh.Addralign = uint64(ctxt.Arch.RegSize);
            sh.Link = uint32(elfshname(".dynsym").shnum);
            sh.Info = uint32(elfshname(".plt").shnum);
            shsym(_addr_sh, _addr_ldr, ldr.Lookup(".rela.plt", 0));

            sh = elfshname(".rela");
            sh.Type = uint32(elf.SHT_RELA);
            sh.Flags = uint64(elf.SHF_ALLOC);
            sh.Entsize = ELF64RELASIZE;
            sh.Addralign = 8;
            sh.Link = uint32(elfshname(".dynsym").shnum);
            shsym(_addr_sh, _addr_ldr, ldr.Lookup(".rela", 0));
        }
        else
 {
            sh = elfshname(".rel.plt");
            sh.Type = uint32(elf.SHT_REL);
            sh.Flags = uint64(elf.SHF_ALLOC);
            sh.Entsize = ELF32RELSIZE;
            sh.Addralign = 4;
            sh.Link = uint32(elfshname(".dynsym").shnum);
            shsym(_addr_sh, _addr_ldr, ldr.Lookup(".rel.plt", 0));

            sh = elfshname(".rel");
            sh.Type = uint32(elf.SHT_REL);
            sh.Flags = uint64(elf.SHF_ALLOC);
            sh.Entsize = ELF32RELSIZE;
            sh.Addralign = 4;
            sh.Link = uint32(elfshname(".dynsym").shnum);
            shsym(_addr_sh, _addr_ldr, ldr.Lookup(".rel", 0));
        }
        if (elf.Machine(eh.Machine) == elf.EM_PPC64) {
            sh = elfshname(".glink");
            sh.Type = uint32(elf.SHT_PROGBITS);
            sh.Flags = uint64(elf.SHF_ALLOC + elf.SHF_EXECINSTR);
            sh.Addralign = 4;
            shsym(_addr_sh, _addr_ldr, ldr.Lookup(".glink", 0));
        }
        sh = elfshname(".plt");
        sh.Type = uint32(elf.SHT_PROGBITS);
        sh.Flags = uint64(elf.SHF_ALLOC + elf.SHF_EXECINSTR);
        if (elf.Machine(eh.Machine) == elf.EM_X86_64) {
            sh.Entsize = 16;
        }
        else if (elf.Machine(eh.Machine) == elf.EM_S390) {
            sh.Entsize = 32;
        }
        else if (elf.Machine(eh.Machine) == elf.EM_PPC64) { 
            // On ppc64, this is just a table of addresses
            // filled by the dynamic linker
            sh.Type = uint32(elf.SHT_NOBITS);

            sh.Flags = uint64(elf.SHF_ALLOC + elf.SHF_WRITE);
            sh.Entsize = 8;
        }
        else
 {
            sh.Entsize = 4;
        }
        sh.Addralign = sh.Entsize;
        shsym(_addr_sh, _addr_ldr, ldr.Lookup(".plt", 0)); 

        // On ppc64, .got comes from the input files, so don't
        // create it here, and .got.plt is not used.
        if (elf.Machine(eh.Machine) != elf.EM_PPC64) {
            sh = elfshname(".got");
            sh.Type = uint32(elf.SHT_PROGBITS);
            sh.Flags = uint64(elf.SHF_ALLOC + elf.SHF_WRITE);
            sh.Entsize = uint64(ctxt.Arch.RegSize);
            sh.Addralign = uint64(ctxt.Arch.RegSize);
            shsym(_addr_sh, _addr_ldr, ldr.Lookup(".got", 0));

            sh = elfshname(".got.plt");
            sh.Type = uint32(elf.SHT_PROGBITS);
            sh.Flags = uint64(elf.SHF_ALLOC + elf.SHF_WRITE);
            sh.Entsize = uint64(ctxt.Arch.RegSize);
            sh.Addralign = uint64(ctxt.Arch.RegSize);
            shsym(_addr_sh, _addr_ldr, ldr.Lookup(".got.plt", 0));
        }
        sh = elfshname(".hash");
        sh.Type = uint32(elf.SHT_HASH);
        sh.Flags = uint64(elf.SHF_ALLOC);
        sh.Entsize = 4;
        sh.Addralign = uint64(ctxt.Arch.RegSize);
        sh.Link = uint32(elfshname(".dynsym").shnum);
        shsym(_addr_sh, _addr_ldr, ldr.Lookup(".hash", 0)); 

        /* sh and elf.PT_DYNAMIC for .dynamic section */
        sh = elfshname(".dynamic");

        sh.Type = uint32(elf.SHT_DYNAMIC);
        sh.Flags = uint64(elf.SHF_ALLOC + elf.SHF_WRITE);
        sh.Entsize = 2 * uint64(ctxt.Arch.RegSize);
        sh.Addralign = uint64(ctxt.Arch.RegSize);
        sh.Link = uint32(elfshname(".dynstr").shnum);
        shsym(_addr_sh, _addr_ldr, ldr.Lookup(".dynamic", 0));
        ph = newElfPhdr();
        ph.Type = elf.PT_DYNAMIC;
        ph.Flags = elf.PF_R + elf.PF_W;
        phsh(_addr_ph, _addr_sh);

        /*
                 * Thread-local storage segment (really just size).
                 */
        var tlssize = uint64(0);
        {
            var sect__prev1 = sect;

            foreach (var (_, __sect) in Segdata.Sections) {
                sect = __sect;
                if (sect.Name == ".tbss") {
                    tlssize = sect.Length;
                }
            }

            sect = sect__prev1;
        }

        if (tlssize != 0) {
            ph = newElfPhdr();
            ph.Type = elf.PT_TLS;
            ph.Flags = elf.PF_R;
            ph.Memsz = tlssize;
            ph.Align = uint64(ctxt.Arch.RegSize);
        }
    }
    if (ctxt.HeadType == objabi.Hlinux) {
        ph = newElfPhdr();
        ph.Type = elf.PT_GNU_STACK;
        ph.Flags = elf.PF_W + elf.PF_R;
        ph.Align = uint64(ctxt.Arch.RegSize);

        ph = newElfPhdr();
        ph.Type = elf.PT_PAX_FLAGS;
        ph.Flags = 0x2a00; // mprotect, randexec, emutramp disabled
        ph.Align = uint64(ctxt.Arch.RegSize);
    }
    else if (ctxt.HeadType == objabi.Hsolaris) {
        ph = newElfPhdr();
        ph.Type = elf.PT_SUNWSTACK;
        ph.Flags = elf.PF_W + elf.PF_R;
    }
elfobj:
    sh = elfshname(".shstrtab");
    sh.Type = uint32(elf.SHT_STRTAB);
    sh.Addralign = 1;
    shsym(_addr_sh, _addr_ldr, ldr.Lookup(".shstrtab", 0));
    eh.Shstrndx = uint16(sh.shnum);

    if (ctxt.IsMIPS()) {
        sh = elfshname(".MIPS.abiflags");
        sh.Type = uint32(elf.SHT_MIPS_ABIFLAGS);
        sh.Flags = uint64(elf.SHF_ALLOC);
        sh.Addralign = 8;
        resoff -= int64(elfMipsAbiFlags(_addr_sh, uint64(startva), uint64(resoff)));

        ph = newElfPhdr();
        ph.Type = elf.PT_MIPS_ABIFLAGS;
        ph.Flags = elf.PF_R;
        phsh(_addr_ph, _addr_sh);

        sh = elfshname(".gnu.attributes");
        sh.Type = uint32(elf.SHT_GNU_ATTRIBUTES);
        sh.Addralign = 1;
        ldr = ctxt.loader;
        shsym(_addr_sh, _addr_ldr, ldr.Lookup(".gnu.attributes", 0));
    }
    if (!FlagS.val) {
        elfshname(".symtab");
        elfshname(".strtab");
    }
    {
        var sect__prev1 = sect;

        foreach (var (_, __sect) in Segtext.Sections) {
            sect = __sect;
            elfshbits(ctxt.LinkMode, _addr_sect);
        }
        sect = sect__prev1;
    }

    {
        var sect__prev1 = sect;

        foreach (var (_, __sect) in Segrodata.Sections) {
            sect = __sect;
            elfshbits(ctxt.LinkMode, _addr_sect);
        }
        sect = sect__prev1;
    }

    {
        var sect__prev1 = sect;

        foreach (var (_, __sect) in Segrelrodata.Sections) {
            sect = __sect;
            elfshbits(ctxt.LinkMode, _addr_sect);
        }
        sect = sect__prev1;
    }

    {
        var sect__prev1 = sect;

        foreach (var (_, __sect) in Segdata.Sections) {
            sect = __sect;
            elfshbits(ctxt.LinkMode, _addr_sect);
        }
        sect = sect__prev1;
    }

    {
        var sect__prev1 = sect;

        foreach (var (_, __sect) in Segdwarf.Sections) {
            sect = __sect;
            elfshbits(ctxt.LinkMode, _addr_sect);
        }
        sect = sect__prev1;
    }

    if (ctxt.LinkMode == LinkExternal) {
        {
            var sect__prev1 = sect;

            foreach (var (_, __sect) in Segtext.Sections) {
                sect = __sect;
                elfshreloc(_addr_ctxt.Arch, _addr_sect);
            }

            sect = sect__prev1;
        }

        {
            var sect__prev1 = sect;

            foreach (var (_, __sect) in Segrodata.Sections) {
                sect = __sect;
                elfshreloc(_addr_ctxt.Arch, _addr_sect);
            }

            sect = sect__prev1;
        }

        {
            var sect__prev1 = sect;

            foreach (var (_, __sect) in Segrelrodata.Sections) {
                sect = __sect;
                elfshreloc(_addr_ctxt.Arch, _addr_sect);
            }

            sect = sect__prev1;
        }

        {
            var sect__prev1 = sect;

            foreach (var (_, __sect) in Segdata.Sections) {
                sect = __sect;
                elfshreloc(_addr_ctxt.Arch, _addr_sect);
            }

            sect = sect__prev1;
        }

        foreach (var (_, si) in dwarfp) {
            var sect = ldr.SymSect(si.secSym());
            elfshreloc(_addr_ctxt.Arch, _addr_sect);
        }        sh = elfshname(".note.GNU-stack");

        sh.Type = uint32(elf.SHT_PROGBITS);
        sh.Addralign = 1;
        sh.Flags = 0;
    }
    if (!FlagS.val) {
        sh = elfshname(".symtab");
        sh.Type = uint32(elf.SHT_SYMTAB);
        sh.Off = uint64(symo);
        sh.Size = uint64(symSize);
        sh.Addralign = uint64(ctxt.Arch.RegSize);
        sh.Entsize = 8 + 2 * uint64(ctxt.Arch.RegSize);
        sh.Link = uint32(elfshname(".strtab").shnum);
        sh.Info = uint32(elfglobalsymndx);

        sh = elfshname(".strtab");
        sh.Type = uint32(elf.SHT_STRTAB);
        sh.Off = uint64(symo) + uint64(symSize);
        sh.Size = uint64(len(Elfstrdat));
        sh.Addralign = 1;
    }
    copy(eh.Ident[..], elf.ELFMAG);

    elf.OSABI osabi = default;

    if (ctxt.HeadType == objabi.Hfreebsd) 
        osabi = elf.ELFOSABI_FREEBSD;
    else if (ctxt.HeadType == objabi.Hnetbsd) 
        osabi = elf.ELFOSABI_NETBSD;
    else if (ctxt.HeadType == objabi.Hopenbsd) 
        osabi = elf.ELFOSABI_OPENBSD;
    else if (ctxt.HeadType == objabi.Hdragonfly) 
        osabi = elf.ELFOSABI_NONE;
        eh.Ident[elf.EI_OSABI] = byte(osabi);

    if (elf64) {
        eh.Ident[elf.EI_CLASS] = byte(elf.ELFCLASS64);
    }
    else
 {
        eh.Ident[elf.EI_CLASS] = byte(elf.ELFCLASS32);
    }
    if (ctxt.Arch.ByteOrder == binary.BigEndian) {
        eh.Ident[elf.EI_DATA] = byte(elf.ELFDATA2MSB);
    }
    else
 {
        eh.Ident[elf.EI_DATA] = byte(elf.ELFDATA2LSB);
    }
    eh.Ident[elf.EI_VERSION] = byte(elf.EV_CURRENT);

    if (ctxt.LinkMode == LinkExternal) {
        eh.Type = uint16(elf.ET_REL);
    }
    else if (ctxt.BuildMode == BuildModePIE) {
        eh.Type = uint16(elf.ET_DYN);
    }
    else
 {
        eh.Type = uint16(elf.ET_EXEC);
    }
    if (ctxt.LinkMode != LinkExternal) {
        eh.Entry = uint64(Entryvalue(ctxt));
    }
    eh.Version = uint32(elf.EV_CURRENT);

    if (pph != null) {
        pph.Filesz = uint64(eh.Phnum) * uint64(eh.Phentsize);
        pph.Memsz = pph.Filesz;
    }
    ctxt.Out.SeekSet(0);
    var a = int64(0);
    a += int64(elfwritehdr(_addr_ctxt.Out));
    a += int64(elfwritephdrs(_addr_ctxt.Out));
    a += int64(elfwriteshdrs(_addr_ctxt.Out));
    if (!FlagD.val) {
        a += int64(elfwriteinterp(_addr_ctxt.Out));
    }
    if (ctxt.IsMIPS()) {
        a += int64(elfWriteMipsAbiFlags(_addr_ctxt));
    }
    if (ctxt.LinkMode != LinkExternal) {
        if (ctxt.HeadType == objabi.Hnetbsd) {
            a += int64(elfwritenetbsdsig(_addr_ctxt.Out));
        }
        if (ctxt.HeadType == objabi.Hopenbsd) {
            a += int64(elfwriteopenbsdsig(_addr_ctxt.Out));
        }
        if (len(buildinfo) > 0) {
            a += int64(elfwritebuildinfo(_addr_ctxt.Out));
        }
        if (flagBuildid != "".val) {
            a += int64(elfwritegobuildid(_addr_ctxt.Out));
        }
    }
    if (flagRace && ctxt.IsNetbsd().val) {
        a += int64(elfwritenetbsdpax(_addr_ctxt.Out));
    }
    if (a > elfreserve) {
        Errorf(null, "ELFRESERVE too small: %d > %d with %d text sections", a, elfreserve, numtext);
    }
    if (a > int64(HEADR)) {
        Errorf(null, "HEADR too small: %d > %d with %d text sections", a, HEADR, numtext);
    }
}

private static void elfadddynsym(ptr<loader.Loader> _addr_ldr, ptr<Target> _addr_target, ptr<ArchSyms> _addr_syms, loader.Sym s) {
    ref loader.Loader ldr = ref _addr_ldr.val;
    ref Target target = ref _addr_target.val;
    ref ArchSyms syms = ref _addr_syms.val;

    ldr.SetSymDynid(s, int32(Nelfsym));
    Nelfsym++;
    var d = ldr.MakeSymbolUpdater(syms.DynSym);
    var name = ldr.SymExtname(s);
    var dstru = ldr.MakeSymbolUpdater(syms.DynStr);
    var st = ldr.SymType(s);
    var cgoeStatic = ldr.AttrCgoExportStatic(s);
    var cgoeDynamic = ldr.AttrCgoExportDynamic(s);
    var cgoexp = (cgoeStatic || cgoeDynamic);

    d.AddUint32(target.Arch, uint32(dstru.Addstring(name)));

    if (elf64) {
        /* type */
        byte t = default;

        if (cgoexp && st == sym.STEXT) {
            t = elf.ST_INFO(elf.STB_GLOBAL, elf.STT_FUNC);
        }
        else
 {
            t = elf.ST_INFO(elf.STB_GLOBAL, elf.STT_OBJECT);
        }
        d.AddUint8(t); 

        /* reserved */
        d.AddUint8(0); 

        /* section where symbol is defined */
        if (st == sym.SDYNIMPORT) {
            d.AddUint16(target.Arch, uint16(elf.SHN_UNDEF));
        }
        else
 {
            d.AddUint16(target.Arch, 1);
        }
        if (st == sym.SDYNIMPORT) {
            d.AddUint64(target.Arch, 0);
        }
        else
 {
            d.AddAddrPlus(target.Arch, s, 0);
        }
        d.AddUint64(target.Arch, uint64(len(ldr.Data(s))));

        var dil = ldr.SymDynimplib(s);

        if (target.Arch.Family == sys.AMD64 && !cgoeDynamic && dil != "" && !seenlib[dil]) {
            var du = ldr.MakeSymbolUpdater(syms.Dynamic);
            Elfwritedynent(_addr_target.Arch, _addr_du, elf.DT_NEEDED, uint64(dstru.Addstring(dil)));
            seenlib[dil] = true;
        }
    }
    else
 {
        /* value */
        if (st == sym.SDYNIMPORT) {
            d.AddUint32(target.Arch, 0);
        }
        else
 {
            d.AddAddrPlus(target.Arch, s, 0);
        }
        d.AddUint32(target.Arch, uint32(len(ldr.Data(s)))); 

        /* type */
        t = default; 

        // TODO(mwhudson): presumably the behavior should actually be the same on both arm and 386.
        if (target.Arch.Family == sys.I386 && cgoexp && st == sym.STEXT) {
            t = elf.ST_INFO(elf.STB_GLOBAL, elf.STT_FUNC);
        }
        else if (target.Arch.Family == sys.ARM && cgoeDynamic && st == sym.STEXT) {
            t = elf.ST_INFO(elf.STB_GLOBAL, elf.STT_FUNC);
        }
        else
 {
            t = elf.ST_INFO(elf.STB_GLOBAL, elf.STT_OBJECT);
        }
        d.AddUint8(t);
        d.AddUint8(0); 

        /* shndx */
        if (st == sym.SDYNIMPORT) {
            d.AddUint16(target.Arch, uint16(elf.SHN_UNDEF));
        }
        else
 {
            d.AddUint16(target.Arch, 1);
        }
    }
}

} // end ld_package
