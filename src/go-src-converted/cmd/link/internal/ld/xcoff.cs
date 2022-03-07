// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ld -- go2cs converted at 2022 March 06 23:22:33 UTC
// import "cmd/link/internal/ld" ==> using ld = go.cmd.link.@internal.ld_package
// Original source: C:\Program Files\Go\src\cmd\link\internal\ld\xcoff.go
using bytes = go.bytes_package;
using objabi = go.cmd.@internal.objabi_package;
using loader = go.cmd.link.@internal.loader_package;
using sym = go.cmd.link.@internal.sym_package;
using binary = go.encoding.binary_package;
using fmt = go.fmt_package;
using ioutil = go.io.ioutil_package;
using bits = go.math.bits_package;
using filepath = go.path.filepath_package;
using sort = go.sort_package;
using strings = go.strings_package;
using sync = go.sync_package;
using System;


namespace go.cmd.link.@internal;

public static partial class ld_package {

    // This file handles all algorithms related to XCOFF files generation.
    // Most of them are adaptations of the ones in  cmd/link/internal/pe.go
    // as PE and XCOFF are based on COFF files.
    // XCOFF files generated are 64 bits.
 
// Total amount of space to reserve at the start of the file
// for File Header, Auxiliary Header, and Section Headers.
// May waste some.
public static readonly var XCOFFHDRRESERVE = FILHSZ_64 + AOUTHSZ_EXEC64 + SCNHSZ_64 * 23; 

// base on dump -o, then rounded from 32B to 64B to
// match worst case elf text section alignment on ppc64.
public static readonly long XCOFFSECTALIGN = 64; 

// XCOFF binaries should normally have all its sections position-independent.
// However, this is not yet possible for .text because of some R_ADDR relocations
// inside RODATA symbols.
// .data and .bss are position-independent so their address start inside a unreachable
// segment during execution to force segfault if something is wrong.
public static readonly nuint XCOFFTEXTBASE = 0x100000000; // Start of text address
public static readonly nuint XCOFFDATABASE = 0x200000000; // Start of data address

// File Header
public partial struct XcoffFileHdr64 {
    public ushort Fmagic; // Target machine
    public ushort Fnscns; // Number of sections
    public int Ftimedat; // Time and date of file creation
    public ulong Fsymptr; // Byte offset to symbol table start
    public ushort Fopthdr; // Number of bytes in optional header
    public ushort Fflags; // Flags
    public int Fnsyms; // Number of entries in symbol table
}

public static readonly nint U64_TOCMAGIC = 0767; // AIX 64-bit XCOFF

// Flags that describe the type of the object file.
public static readonly nuint F_RELFLG = 0x0001;
public static readonly nuint F_EXEC = 0x0002;
public static readonly nuint F_LNNO = 0x0004;
public static readonly nuint F_FDPR_PROF = 0x0010;
public static readonly nuint F_FDPR_OPTI = 0x0020;
public static readonly nuint F_DSA = 0x0040;
public static readonly nuint F_VARPG = 0x0100;
public static readonly nuint F_DYNLOAD = 0x1000;
public static readonly nuint F_SHROBJ = 0x2000;
public static readonly nuint F_LOADONLY = 0x4000;


// Auxiliary Header
public partial struct XcoffAoutHdr64 {
    public short Omagic; // Flags - Ignored If Vstamp Is 1
    public short Ovstamp; // Version
    public uint Odebugger; // Reserved For Debugger
    public ulong Otextstart; // Virtual Address Of Text
    public ulong Odatastart; // Virtual Address Of Data
    public ulong Otoc; // Toc Address
    public short Osnentry; // Section Number For Entry Point
    public short Osntext; // Section Number For Text
    public short Osndata; // Section Number For Data
    public short Osntoc; // Section Number For Toc
    public short Osnloader; // Section Number For Loader
    public short Osnbss; // Section Number For Bss
    public short Oalgntext; // Max Text Alignment
    public short Oalgndata; // Max Data Alignment
    public array<byte> Omodtype; // Module Type Field
    public byte Ocpuflag; // Bit Flags - Cputypes Of Objects
    public byte Ocputype; // Reserved for CPU type
    public byte Otextpsize; // Requested text page size
    public byte Odatapsize; // Requested data page size
    public byte Ostackpsize; // Requested stack page size
    public byte Oflags; // Flags And TLS Alignment
    public ulong Otsize; // Text Size In Bytes
    public ulong Odsize; // Data Size In Bytes
    public ulong Obsize; // Bss Size In Bytes
    public ulong Oentry; // Entry Point Address
    public ulong Omaxstack; // Max Stack Size Allowed
    public ulong Omaxdata; // Max Data Size Allowed
    public short Osntdata; // Section Number For Tdata Section
    public short Osntbss; // Section Number For Tbss Section
    public ushort Ox64flags; // Additional Flags For 64-Bit Objects
    public short Oresv3a; // Reserved
    public array<int> Oresv3; // Reserved
}

// Section Header
public partial struct XcoffScnHdr64 {
    public array<byte> Sname; // Section Name
    public ulong Spaddr; // Physical Address
    public ulong Svaddr; // Virtual Address
    public ulong Ssize; // Section Size
    public ulong Sscnptr; // File Offset To Raw Data
    public ulong Srelptr; // File Offset To Relocation
    public ulong Slnnoptr; // File Offset To Line Numbers
    public uint Snreloc; // Number Of Relocation Entries
    public uint Snlnno; // Number Of Line Number Entries
    public uint Sflags; // flags
}

// Flags defining the section type.
public static readonly nuint STYP_DWARF = 0x0010;
public static readonly nuint STYP_TEXT = 0x0020;
public static readonly nuint STYP_DATA = 0x0040;
public static readonly nuint STYP_BSS = 0x0080;
public static readonly nuint STYP_EXCEPT = 0x0100;
public static readonly nuint STYP_INFO = 0x0200;
public static readonly nuint STYP_TDATA = 0x0400;
public static readonly nuint STYP_TBSS = 0x0800;
public static readonly nuint STYP_LOADER = 0x1000;
public static readonly nuint STYP_DEBUG = 0x2000;
public static readonly nuint STYP_TYPCHK = 0x4000;
public static readonly nuint STYP_OVRFLO = 0x8000;

public static readonly nuint SSUBTYP_DWINFO = 0x10000; // DWARF info section
public static readonly nuint SSUBTYP_DWLINE = 0x20000; // DWARF line-number section
public static readonly nuint SSUBTYP_DWPBNMS = 0x30000; // DWARF public names section
public static readonly nuint SSUBTYP_DWPBTYP = 0x40000; // DWARF public types section
public static readonly nuint SSUBTYP_DWARNGE = 0x50000; // DWARF aranges section
public static readonly nuint SSUBTYP_DWABREV = 0x60000; // DWARF abbreviation section
public static readonly nuint SSUBTYP_DWSTR = 0x70000; // DWARF strings section
public static readonly nuint SSUBTYP_DWRNGES = 0x80000; // DWARF ranges section
public static readonly nuint SSUBTYP_DWLOC = 0x90000; // DWARF location lists section
public static readonly nuint SSUBTYP_DWFRAME = 0xA0000; // DWARF frames section
public static readonly nuint SSUBTYP_DWMAC = 0xB0000; // DWARF macros section

// Headers size
public static readonly nint FILHSZ_32 = 20;
public static readonly nint FILHSZ_64 = 24;
public static readonly nint AOUTHSZ_EXEC32 = 72;
public static readonly nint AOUTHSZ_EXEC64 = 120;
public static readonly nint SCNHSZ_32 = 40;
public static readonly nint SCNHSZ_64 = 72;
public static readonly nint LDHDRSZ_32 = 32;
public static readonly nint LDHDRSZ_64 = 56;
public static readonly nint LDSYMSZ_64 = 24;
public static readonly nint RELSZ_64 = 14;


// Type representing all XCOFF symbols.
private partial interface xcoffSym {
}

// Symbol Table Entry
public partial struct XcoffSymEnt64 {
    public ulong Nvalue; // Symbol value
    public uint Noffset; // Offset of the name in string table or .debug section
    public short Nscnum; // Section number of symbol
    public ushort Ntype; // Basic and derived type specification
    public byte Nsclass; // Storage class of symbol
    public sbyte Nnumaux; // Number of auxiliary entries
}

public static readonly nint SYMESZ = 18;



 
// Nscnum
public static readonly nint N_DEBUG = -2;
public static readonly nint N_ABS = -1;
public static readonly nint N_UNDEF = 0; 

//Ntype
public static readonly nuint SYM_V_INTERNAL = 0x1000;
public static readonly nuint SYM_V_HIDDEN = 0x2000;
public static readonly nuint SYM_V_PROTECTED = 0x3000;
public static readonly nuint SYM_V_EXPORTED = 0x4000;
public static readonly nuint SYM_TYPE_FUNC = 0x0020; // is function

// Storage Class.
public static readonly nint C_NULL = 0; // Symbol table entry marked for deletion
public static readonly nint C_EXT = 2; // External symbol
public static readonly nint C_STAT = 3; // Static symbol
public static readonly nint C_BLOCK = 100; // Beginning or end of inner block
public static readonly nint C_FCN = 101; // Beginning or end of function
public static readonly nint C_FILE = 103; // Source file name and compiler information
public static readonly nint C_HIDEXT = 107; // Unnamed external symbol
public static readonly nint C_BINCL = 108; // Beginning of include file
public static readonly nint C_EINCL = 109; // End of include file
public static readonly nint C_WEAKEXT = 111; // Weak external symbol
public static readonly nint C_DWARF = 112; // DWARF symbol
public static readonly nint C_GSYM = 128; // Global variable
public static readonly nint C_LSYM = 129; // Automatic variable allocated on stack
public static readonly nint C_PSYM = 130; // Argument to subroutine allocated on stack
public static readonly nint C_RSYM = 131; // Register variable
public static readonly nint C_RPSYM = 132; // Argument to function or procedure stored in register
public static readonly nint C_STSYM = 133; // Statically allocated symbol
public static readonly nint C_BCOMM = 135; // Beginning of common block
public static readonly nint C_ECOML = 136; // Local member of common block
public static readonly nint C_ECOMM = 137; // End of common block
public static readonly nint C_DECL = 140; // Declaration of object
public static readonly nint C_ENTRY = 141; // Alternate entry
public static readonly nint C_FUN = 142; // Function or procedure
public static readonly nint C_BSTAT = 143; // Beginning of static block
public static readonly nint C_ESTAT = 144; // End of static block
public static readonly nint C_GTLS = 145; // Global thread-local variable
public static readonly nint C_STTLS = 146; // Static thread-local variable

// File Auxiliary Entry
public partial struct XcoffAuxFile64 {
    public uint Xzeroes; // The name is always in the string table
    public uint Xoffset; // Offset in the string table
    public array<byte> X_pad1;
    public byte Xftype; // Source file string type
    public array<byte> X_pad2;
    public byte Xauxtype; // Type of auxiliary entry
}

// Function Auxiliary Entry
public partial struct XcoffAuxFcn64 {
    public ulong Xlnnoptr; // File pointer to line number
    public uint Xfsize; // Size of function in bytes
    public uint Xendndx; // Symbol table index of next entry
    public byte Xpad; // Unused
    public byte Xauxtype; // Type of auxiliary entry
}

// csect Auxiliary Entry.
public partial struct XcoffAuxCSect64 {
    public uint Xscnlenlo; // Lower 4 bytes of length or symbol table index
    public uint Xparmhash; // Offset of parameter type-check string
    public ushort Xsnhash; // .typchk section number
    public byte Xsmtyp; // Symbol alignment and type
    public byte Xsmclas; // Storage-mapping class
    public uint Xscnlenhi; // Upper 4 bytes of length or symbol table index
    public byte Xpad; // Unused
    public byte Xauxtype; // Type of auxiliary entry
}

// DWARF Auxiliary Entry
public partial struct XcoffAuxDWARF64 {
    public ulong Xscnlen; // Length of this symbol section
    public array<byte> X_pad;
    public byte Xauxtype; // Type of auxiliary entry
}

// Auxiliary type
private static readonly nint _AUX_EXCEPT = 255;
private static readonly nint _AUX_FCN = 254;
private static readonly nint _AUX_SYM = 253;
private static readonly nint _AUX_FILE = 252;
private static readonly nint _AUX_CSECT = 251;
private static readonly nint _AUX_SECT = 250;


// Xftype field
public static readonly nint XFT_FN = 0; // Source File Name
public static readonly nint XFT_CT = 1; // Compile Time Stamp
public static readonly nint XFT_CV = 2; // Compiler Version Number
public static readonly nint XFT_CD = 128; // Compiler Defined Information/


// Symbol type field.
public static readonly nint XTY_ER = 0; // External reference
public static readonly nint XTY_SD = 1; // Section definition
public static readonly nint XTY_LD = 2; // Label definition
public static readonly nint XTY_CM = 3; // Common csect definition
public static readonly nuint XTY_WK = 0x8; // Weak symbol
public static readonly nuint XTY_EXP = 0x10; // Exported symbol
public static readonly nuint XTY_ENT = 0x20; // Entry point symbol
public static readonly nuint XTY_IMP = 0x40; // Imported symbol

// Storage-mapping class.
public static readonly nint XMC_PR = 0; // Program code
public static readonly nint XMC_RO = 1; // Read-only constant
public static readonly nint XMC_DB = 2; // Debug dictionary table
public static readonly nint XMC_TC = 3; // TOC entry
public static readonly nint XMC_UA = 4; // Unclassified
public static readonly nint XMC_RW = 5; // Read/Write data
public static readonly nint XMC_GL = 6; // Global linkage
public static readonly nint XMC_XO = 7; // Extended operation
public static readonly nint XMC_SV = 8; // 32-bit supervisor call descriptor
public static readonly nint XMC_BS = 9; // BSS class
public static readonly nint XMC_DS = 10; // Function descriptor
public static readonly nint XMC_UC = 11; // Unnamed FORTRAN common
public static readonly nint XMC_TC0 = 15; // TOC anchor
public static readonly nint XMC_TD = 16; // Scalar data entry in the TOC
public static readonly nint XMC_SV64 = 17; // 64-bit supervisor call descriptor
public static readonly nint XMC_SV3264 = 18; // Supervisor call descriptor for both 32-bit and 64-bit
public static readonly nint XMC_TL = 20; // Read/Write thread-local data
public static readonly nint XMC_UL = 21; // Read/Write thread-local data (.tbss)
public static readonly nint XMC_TE = 22; // TOC entry

// Loader Header
public partial struct XcoffLdHdr64 {
    public int Lversion; // Loader section version number
    public int Lnsyms; // Number of symbol table entries
    public int Lnreloc; // Number of relocation table entries
    public uint Listlen; // Length of import file ID string table
    public int Lnimpid; // Number of import file IDs
    public uint Lstlen; // Length of string table
    public ulong Limpoff; // Offset to start of import file IDs
    public ulong Lstoff; // Offset to start of string table
    public ulong Lsymoff; // Offset to start of symbol table
    public ulong Lrldoff; // Offset to start of relocation entries
}

// Loader Symbol
public partial struct XcoffLdSym64 {
    public ulong Lvalue; // Address field
    public uint Loffset; // Byte offset into string table of symbol name
    public short Lscnum; // Section number containing symbol
    public sbyte Lsmtype; // Symbol type, export, import flags
    public sbyte Lsmclas; // Symbol storage class
    public int Lifile; // Import file ID; ordinal of import file IDs
    public uint Lparm; // Parameter type-check field
}

private partial struct xcoffLoaderSymbol {
    public loader.Sym sym;
    public sbyte smtype;
    public sbyte smclas;
}

public partial struct XcoffLdImportFile64 {
    public @string Limpidpath;
    public @string Limpidbase;
    public @string Limpidmem;
}

public partial struct XcoffLdRel64 {
    public ulong Lvaddr; // Address Field
    public ushort Lrtype; // Relocation Size and Type
    public short Lrsecnm; // Section Number being relocated
    public int Lsymndx; // Loader-Section symbol table index
}

// xcoffLoaderReloc holds information about a relocation made by the loader.
private partial struct xcoffLoaderReloc {
    public loader.Sym sym;
    public int roff;
    public ushort rtype;
    public int symndx;
}

public static readonly nuint XCOFF_R_POS = 0x00; // A(sym) Positive Relocation
public static readonly nuint XCOFF_R_NEG = 0x01; // -A(sym) Negative Relocation
public static readonly nuint XCOFF_R_REL = 0x02; // A(sym-*) Relative to self
public static readonly nuint XCOFF_R_TOC = 0x03; // A(sym-TOC) Relative to TOC
public static readonly nuint XCOFF_R_TRL = 0x12; // A(sym-TOC) TOC Relative indirect load.

public static readonly nuint XCOFF_R_TRLA = 0x13; // A(sym-TOC) TOC Rel load address. modifiable inst
public static readonly nuint XCOFF_R_GL = 0x05; // A(external TOC of sym) Global Linkage
public static readonly nuint XCOFF_R_TCL = 0x06; // A(local TOC of sym) Local object TOC address
public static readonly nuint XCOFF_R_RL = 0x0C; // A(sym) Pos indirect load. modifiable instruction
public static readonly nuint XCOFF_R_RLA = 0x0D; // A(sym) Pos Load Address. modifiable instruction
public static readonly nuint XCOFF_R_REF = 0x0F; // AL0(sym) Non relocating ref. No garbage collect
public static readonly nuint XCOFF_R_BA = 0x08; // A(sym) Branch absolute. Cannot modify instruction
public static readonly nuint XCOFF_R_RBA = 0x18; // A(sym) Branch absolute. modifiable instruction
public static readonly nuint XCOFF_R_BR = 0x0A; // A(sym-*) Branch rel to self. non modifiable
public static readonly nuint XCOFF_R_RBR = 0x1A; // A(sym-*) Branch rel to self. modifiable instr

public static readonly nuint XCOFF_R_TLS = 0x20; // General-dynamic reference to TLS symbol
public static readonly nuint XCOFF_R_TLS_IE = 0x21; // Initial-exec reference to TLS symbol
public static readonly nuint XCOFF_R_TLS_LD = 0x22; // Local-dynamic reference to TLS symbol
public static readonly nuint XCOFF_R_TLS_LE = 0x23; // Local-exec reference to TLS symbol
public static readonly nuint XCOFF_R_TLSM = 0x24; // Module reference to TLS symbol
public static readonly nuint XCOFF_R_TLSML = 0x25; // Module reference to local (own) module

public static readonly nuint XCOFF_R_TOCU = 0x30; // Relative to TOC - high order bits
public static readonly nuint XCOFF_R_TOCL = 0x31; // Relative to TOC - low order bits

public partial struct XcoffLdStr64 {
    public ushort size;
    public @string name;
}

// xcoffFile is used to build XCOFF file.
private partial struct xcoffFile {
    public XcoffFileHdr64 xfhdr;
    public XcoffAoutHdr64 xahdr;
    public slice<ptr<XcoffScnHdr64>> sections;
    public ptr<XcoffScnHdr64> sectText;
    public ptr<XcoffScnHdr64> sectData;
    public ptr<XcoffScnHdr64> sectBss;
    public xcoffStringTable stringTable;
    public map<@string, short> sectNameToScnum;
    public ulong loaderSize;
    public long symtabOffset; // offset to the start of symbol table
    public uint symbolCount; // number of symbol table records written
    public slice<xcoffSym> symtabSym; // XCOFF symbols for the symbol table
    public map<@string, nint> dynLibraries; // Dynamic libraries in .loader section. The integer represents its import file number (- 1)
    public slice<ptr<xcoffLoaderSymbol>> loaderSymbols; // symbols inside .loader symbol table
    public slice<ptr<xcoffLoaderReloc>> loaderReloc; // Reloc that must be made inside loader
    public ref sync.Mutex Mutex => ref Mutex_val; // currently protect loaderReloc
}

// Var used by XCOFF Generation algorithms
private static xcoffFile xfile = default;

// xcoffStringTable is a XCOFF string table.
private partial struct xcoffStringTable {
    public slice<@string> strings;
    public nint stringsLen;
}

// size returns size of string table t.
private static nint size(this ptr<xcoffStringTable> _addr_t) {
    ref xcoffStringTable t = ref _addr_t.val;
 
    // string table starts with 4-byte length at the beginning
    return t.stringsLen + 4;

}

// add adds string str to string table t.
private static nint add(this ptr<xcoffStringTable> _addr_t, @string str) {
    ref xcoffStringTable t = ref _addr_t.val;

    var off = t.size();
    t.strings = append(t.strings, str);
    t.stringsLen += len(str) + 1; // each string will have 0 appended to it
    return off;

}

// write writes string table t into the output file.
private static void write(this ptr<xcoffStringTable> _addr_t, ptr<OutBuf> _addr_@out) {
    ref xcoffStringTable t = ref _addr_t.val;
    ref OutBuf @out = ref _addr_@out.val;

    @out.Write32(uint32(t.size()));
    foreach (var (_, s) in t.strings) {
        @out.WriteString(s);
        @out.Write8(0);
    }
}

// write writes XCOFF section sect into the output file.
private static void write(this ptr<XcoffScnHdr64> _addr_sect, ptr<Link> _addr_ctxt) {
    ref XcoffScnHdr64 sect = ref _addr_sect.val;
    ref Link ctxt = ref _addr_ctxt.val;

    binary.Write(ctxt.Out, binary.BigEndian, sect);
    ctxt.Out.Write32(0); // Add 4 empty bytes at the end to match alignment
}

// addSection adds section to the XCOFF file f.
private static ptr<XcoffScnHdr64> addSection(this ptr<xcoffFile> _addr_f, @string name, ulong addr, ulong size, ulong fileoff, uint flags) {
    ref xcoffFile f = ref _addr_f.val;

    ptr<XcoffScnHdr64> sect = addr(new XcoffScnHdr64(Spaddr:addr,Svaddr:addr,Ssize:size,Sscnptr:fileoff,Sflags:flags,));
    copy(sect.Sname[..], name); // copy string to [8]byte
    f.sections = append(f.sections, sect);
    f.sectNameToScnum[name] = int16(len(f.sections));
    return _addr_sect!;

}

// addDwarfSection adds a dwarf section to the XCOFF file f.
// This function is similar to addSection, but Dwarf section names
// must be modified to conventional names and they are various subtypes.
private static ptr<XcoffScnHdr64> addDwarfSection(this ptr<xcoffFile> _addr_f, ptr<sym.Section> _addr_s) {
    ref xcoffFile f = ref _addr_f.val;
    ref sym.Section s = ref _addr_s.val;

    var (newName, subtype) = xcoffGetDwarfSubtype(s.Name);
    return _addr_f.addSection(newName, 0, s.Length, s.Seg.Fileoff + s.Vaddr - s.Seg.Vaddr, STYP_DWARF | subtype)!;
}

// xcoffGetDwarfSubtype returns the XCOFF name of the DWARF section str
// and its subtype constant.
private static (@string, uint) xcoffGetDwarfSubtype(@string str) {
    @string _p0 = default;
    uint _p0 = default;

    switch (str) {
        case ".debug_abbrev": 
            return (".dwabrev", SSUBTYP_DWABREV);
            break;
        case ".debug_info": 
            return (".dwinfo", SSUBTYP_DWINFO);
            break;
        case ".debug_frame": 
            return (".dwframe", SSUBTYP_DWFRAME);
            break;
        case ".debug_line": 
            return (".dwline", SSUBTYP_DWLINE);
            break;
        case ".debug_loc": 
            return (".dwloc", SSUBTYP_DWLOC);
            break;
        case ".debug_pubnames": 
            return (".dwpbnms", SSUBTYP_DWPBNMS);
            break;
        case ".debug_pubtypes": 
            return (".dwpbtyp", SSUBTYP_DWPBTYP);
            break;
        case ".debug_ranges": 
            return (".dwrnges", SSUBTYP_DWRNGES);
            break;
        default: 
            Exitf("unknown DWARF section name for XCOFF: %s", str);
            break;
    } 
    // never used
    return ("", 0);

}

// getXCOFFscnum returns the XCOFF section number of a Go section.
private static short getXCOFFscnum(this ptr<xcoffFile> _addr_f, ptr<sym.Section> _addr_sect) {
    ref xcoffFile f = ref _addr_f.val;
    ref sym.Section sect = ref _addr_sect.val;


    if (sect.Seg == _addr_Segtext) 
        return f.sectNameToScnum[".text"];
    else if (sect.Seg == _addr_Segdata) 
        if (sect.Name == ".noptrbss" || sect.Name == ".bss") {
            return f.sectNameToScnum[".bss"];
        }
        if (sect.Name == ".tbss") {
            return f.sectNameToScnum[".tbss"];
        }
        return f.sectNameToScnum[".data"];
    else if (sect.Seg == _addr_Segdwarf) 
        var (name, _) = xcoffGetDwarfSubtype(sect.Name);
        return f.sectNameToScnum[name];
    else if (sect.Seg == _addr_Segrelrodata) 
        return f.sectNameToScnum[".data"];
        Errorf(null, "getXCOFFscnum not implemented for section %s", sect.Name);
    return -1;

}

// Xcoffinit initialised some internal value and setups
// already known header information
public static void Xcoffinit(ptr<Link> _addr_ctxt) {
    ref Link ctxt = ref _addr_ctxt.val;

    xfile.dynLibraries = make_map<@string, nint>();

    HEADR = int32(Rnd(XCOFFHDRRESERVE, XCOFFSECTALIGN));
    if (FlagTextAddr != -1.val) {
        Errorf(null, "-T not available on AIX");
    }
    FlagTextAddr.val = XCOFFTEXTBASE + int64(HEADR);
    if (FlagRound != -1.val) {
        Errorf(null, "-R not available on AIX");
    }
    FlagRound.val = int(XCOFFSECTALIGN);


}

// SYMBOL TABLE

// type records C_FILE information needed for genasmsym in XCOFF.
private partial struct xcoffSymSrcFile {
    public @string name;
    public ptr<XcoffSymEnt64> file; // Symbol of this C_FILE
    public ptr<XcoffAuxCSect64> csectAux; // Symbol for the current .csect
    public ulong csectSymNb; // Symbol number for the current .csect
    public long csectVAStart;
    public long csectVAEnd;
}

private static var currDwscnoff = make_map<@string, ulong>();private static xcoffSymSrcFile currSymSrcFile = default;private static var outerSymSize = make_map<@string, long>();

// xcoffUpdateOuterSize stores the size of outer symbols in order to have it
// in the symbol table.
private static void xcoffUpdateOuterSize(ptr<Link> _addr_ctxt, long size, sym.SymKind stype) {
    ref Link ctxt = ref _addr_ctxt.val;

    if (size == 0) {
        return ;
    }
    var ldr = ctxt.loader;

    if (stype == sym.SRODATA || stype == sym.SRODATARELRO || stype == sym.SFUNCTAB || stype == sym.SSTRING)
    {
        goto __switch_break0;
    }
    if (stype == sym.STYPERELRO)
    {
        if (ctxt.UseRelro() && (ctxt.BuildMode == BuildModeCArchive || ctxt.BuildMode == BuildModeCShared || ctxt.BuildMode == BuildModePIE)) { 
            // runtime.types size must be removed, as it's a real symbol.
            var tsize = ldr.SymSize(ldr.Lookup("runtime.types", 0));
            outerSymSize["typerel.*"] = size - tsize;
            return ;

        }
        fallthrough = true;
    }
    if (fallthrough || stype == sym.STYPE)
    {
        if (!ctxt.DynlinkingGo()) { 
            // runtime.types size must be removed, as it's a real symbol.
            tsize = ldr.SymSize(ldr.Lookup("runtime.types", 0));
            outerSymSize["type.*"] = size - tsize;

        }
        goto __switch_break0;
    }
    if (stype == sym.SGOSTRING)
    {
        outerSymSize["go.string.*"] = size;
        goto __switch_break0;
    }
    if (stype == sym.SGOFUNC)
    {
        if (!ctxt.DynlinkingGo()) {
            outerSymSize["go.func.*"] = size;
        }
        goto __switch_break0;
    }
    if (stype == sym.SGOFUNCRELRO)
    {
        outerSymSize["go.funcrel.*"] = size;
        goto __switch_break0;
    }
    if (stype == sym.SGCBITS)
    {
        outerSymSize["runtime.gcbits.*"] = size;
        goto __switch_break0;
    }
    if (stype == sym.SPCLNTAB)
    {
        outerSymSize["runtime.pclntab"] = size;
        goto __switch_break0;
    }
    // default: 
        Errorf(null, "unknown XCOFF outer symbol for type %s", stype.String());

    __switch_break0:;

}

// addSymbol writes a symbol or an auxiliary symbol entry on ctxt.out.
private static void addSymbol(this ptr<xcoffFile> _addr_f, xcoffSym sym) {
    ref xcoffFile f = ref _addr_f.val;

    f.symtabSym = append(f.symtabSym, sym);
    f.symbolCount++;
}

// xcoffAlign returns the log base 2 of the symbol's alignment.
private static byte xcoffAlign(ptr<loader.Loader> _addr_ldr, loader.Sym x, SymbolType t) {
    ref loader.Loader ldr = ref _addr_ldr.val;

    var align = ldr.SymAlign(x);
    if (align == 0) {
        if (t == TextSym) {
            align = int32(Funcalign);
        }
        else
 {
            align = symalign(ldr, x);
        }
    }
    return logBase2(int(align));

}

// logBase2 returns the log in base 2 of a.
private static byte logBase2(nint a) {
    return uint8(bits.Len(uint(a)) - 1);
}

// Write symbols needed when a new file appeared:
// - a C_FILE with one auxiliary entry for its name
// - C_DWARF symbols to provide debug information
// - a C_HIDEXT which will be a csect containing all of its functions
// It needs several parameters to create .csect symbols such as its entry point and its section number.
//
// Currently, a new file is in fact a new package. It seems to be OK, but it might change
// in the future.
private static void writeSymbolNewFile(this ptr<xcoffFile> _addr_f, ptr<Link> _addr_ctxt, @string name, ulong firstEntry, short extnum) {
    ref xcoffFile f = ref _addr_f.val;
    ref Link ctxt = ref _addr_ctxt.val;

    var ldr = ctxt.loader; 
    /* C_FILE */
    ptr<XcoffSymEnt64> s = addr(new XcoffSymEnt64(Noffset:uint32(f.stringTable.add(".file")),Nsclass:C_FILE,Nscnum:N_DEBUG,Ntype:0,Nnumaux:1,));
    f.addSymbol(s);
    currSymSrcFile.file = s; 

    // Auxiliary entry for file name.
    ptr<XcoffAuxFile64> auxf = addr(new XcoffAuxFile64(Xoffset:uint32(f.stringTable.add(name)),Xftype:XFT_FN,Xauxtype:_AUX_FILE,));
    f.addSymbol(auxf); 

    /* Dwarf */
    foreach (var (_, sect) in Segdwarf.Sections) {
        ulong dwsize = default;
        if (ctxt.LinkMode == LinkInternal) { 
            // Find the size of this corresponding package DWARF compilation unit.
            // This size is set during DWARF generation (see dwarf.go).
            dwsize = getDwsectCUSize(sect.Name, name); 
            // .debug_abbrev is common to all packages and not found with the previous function
            if (sect.Name == ".debug_abbrev") {
                dwsize = uint64(ldr.SymSize(loader.Sym(sect.Sym)));
            }

        }
        else
 { 
            // There is only one .FILE with external linking.
            dwsize = sect.Length;

        }
        var (name, _) = xcoffGetDwarfSubtype(sect.Name);
        s = addr(new XcoffSymEnt64(Nvalue:currDwscnoff[sect.Name],Noffset:uint32(f.stringTable.add(name)),Nsclass:C_DWARF,Nscnum:f.getXCOFFscnum(sect),Nnumaux:1,));

        if (currSymSrcFile.csectAux == null) { 
            // Dwarf relocations need the symbol number of .dw* symbols.
            // It doesn't need to know it for each package, one is enough.
            // currSymSrcFile.csectAux == nil means first package.
            ldr.SetSymDynid(loader.Sym(sect.Sym), int32(f.symbolCount));

            if (sect.Name == ".debug_frame" && ctxt.LinkMode != LinkExternal) { 
                // CIE size must be added to the first package.
                dwsize += 48;

            }

        }
        f.addSymbol(s); 

        // update the DWARF section offset in this file
        if (sect.Name != ".debug_abbrev") {
            currDwscnoff[sect.Name] += dwsize;
        }
        ptr<XcoffAuxDWARF64> auxd = addr(new XcoffAuxDWARF64(Xscnlen:dwsize,Xauxtype:_AUX_SECT,));

        f.addSymbol(auxd);

    }    if (extnum != 1) {
        Exitf("XCOFF symtab: A new file was detected with its first symbol not in .text");
    }
    currSymSrcFile.csectSymNb = uint64(f.symbolCount); 

    // No offset because no name
    s = addr(new XcoffSymEnt64(Nvalue:firstEntry,Nscnum:extnum,Nsclass:C_HIDEXT,Ntype:0,Nnumaux:1,));
    f.addSymbol(s);

    ptr<XcoffAuxCSect64> aux = addr(new XcoffAuxCSect64(Xsmclas:XMC_PR,Xsmtyp:XTY_SD|logBase2(Funcalign)<<3,Xauxtype:_AUX_CSECT,));
    f.addSymbol(aux);

    currSymSrcFile.csectAux = aux;
    currSymSrcFile.csectVAStart = int64(firstEntry);
    currSymSrcFile.csectVAEnd = int64(firstEntry);

}

// Update values for the previous package.
//  - Svalue of the C_FILE symbol: if it is the last one, this Svalue must be -1
//  - Xsclen of the csect symbol.
private static void updatePreviousFile(this ptr<xcoffFile> _addr_f, ptr<Link> _addr_ctxt, bool last) {
    ref xcoffFile f = ref _addr_f.val;
    ref Link ctxt = ref _addr_ctxt.val;
 
    // first file
    if (currSymSrcFile.file == null) {
        return ;
    }
    var cfile = currSymSrcFile.file;
    if (last) {
        cfile.Nvalue = 0xFFFFFFFFFFFFFFFF;
    }
    else
 {
        cfile.Nvalue = uint64(f.symbolCount);
    }
    var aux = currSymSrcFile.csectAux;
    var csectSize = currSymSrcFile.csectVAEnd - currSymSrcFile.csectVAStart;
    aux.Xscnlenlo = uint32(csectSize & 0xFFFFFFFF);
    aux.Xscnlenhi = uint32(csectSize >> 32);

}

// Write symbol representing a .text function.
// The symbol table is split with C_FILE corresponding to each package
// and not to each source file as it should be.
private static slice<xcoffSym> writeSymbolFunc(this ptr<xcoffFile> _addr_f, ptr<Link> _addr_ctxt, loader.Sym x) {
    ref xcoffFile f = ref _addr_f.val;
    ref Link ctxt = ref _addr_ctxt.val;
 
    // New XCOFF symbols which will be written.
    xcoffSym syms = new slice<xcoffSym>(new xcoffSym[] {  }); 

    // Check if a new file is detected.
    var ldr = ctxt.loader;
    var name = ldr.SymName(x);
    if (strings.Contains(name, "-tramp") || strings.HasPrefix(name, "runtime.text.")) { 
        // Trampoline don't have a FILE so there are considered
        // in the current file.
        // Same goes for runtime.text.X symbols.
    }
    else if (ldr.SymPkg(x) == "") { // Undefined global symbol
        // If this happens, the algorithm must be redone.
        if (currSymSrcFile.name != "") {
            Exitf("undefined global symbol found inside another file");
        }
    }
    else
 { 
        // Current file has changed. New C_FILE, C_DWARF, etc must be generated.
        if (currSymSrcFile.name != ldr.SymPkg(x)) {
            if (ctxt.LinkMode == LinkInternal) { 
                // update previous file values
                xfile.updatePreviousFile(ctxt, false);
                currSymSrcFile.name = ldr.SymPkg(x);
                f.writeSymbolNewFile(ctxt, ldr.SymPkg(x), uint64(ldr.SymValue(x)), xfile.getXCOFFscnum(ldr.SymSect(x)));

            }
            else
 { 
                // With external linking, ld will crash if there is several
                // .FILE and DWARF debugging enable, somewhere during
                // the relocation phase.
                // Therefore, all packages are merged under a fake .FILE
                // "go_functions".
                // TODO(aix); remove once ld has been fixed or the triggering
                // relocation has been found and fixed.
                if (currSymSrcFile.name == "") {
                    currSymSrcFile.name = ldr.SymPkg(x);
                    f.writeSymbolNewFile(ctxt, "go_functions", uint64(ldr.SymValue(x)), xfile.getXCOFFscnum(ldr.SymSect(x)));
                }

            }

        }
    }
    ptr<XcoffSymEnt64> s = addr(new XcoffSymEnt64(Nsclass:C_EXT,Noffset:uint32(xfile.stringTable.add(ldr.SymExtname(x))),Nvalue:uint64(ldr.SymValue(x)),Nscnum:f.getXCOFFscnum(ldr.SymSect(x)),Ntype:SYM_TYPE_FUNC,Nnumaux:2,));

    if (ldr.IsFileLocal(x) || ldr.AttrVisibilityHidden(x) || ldr.AttrLocal(x)) {
        s.Nsclass = C_HIDEXT;
    }
    ldr.SetSymDynid(x, int32(xfile.symbolCount));
    syms = append(syms, s); 

    // Keep track of the section size by tracking the VA range. Individual
    // alignment differences may introduce a few extra bytes of padding
    // which are not fully accounted for by ldr.SymSize(x).
    var sv = ldr.SymValue(x) + ldr.SymSize(x);
    if (currSymSrcFile.csectVAEnd < sv) {
        currSymSrcFile.csectVAEnd = sv;
    }
    ptr<XcoffAuxFcn64> a2 = addr(new XcoffAuxFcn64(Xfsize:uint32(ldr.SymSize(x)),Xlnnoptr:0,Xendndx:xfile.symbolCount+3,Xauxtype:_AUX_FCN,));
    syms = append(syms, a2);

    ptr<XcoffAuxCSect64> a4 = addr(new XcoffAuxCSect64(Xscnlenlo:uint32(currSymSrcFile.csectSymNb&0xFFFFFFFF),Xscnlenhi:uint32(currSymSrcFile.csectSymNb>>32),Xsmclas:XMC_PR,Xsmtyp:XTY_LD,Xauxtype:_AUX_CSECT,));
    a4.Xsmtyp |= uint8(xcoffAlign(_addr_ldr, x, TextSym) << 3);

    syms = append(syms, a4);
    return syms;

}

// put function used by genasmsym to write symbol table
private static void putaixsym(ptr<Link> _addr_ctxt, loader.Sym x, SymbolType t) {
    ref Link ctxt = ref _addr_ctxt.val;
 
    // All XCOFF symbols generated by this GO symbols
    // Can be a symbol entry or a auxiliary entry
    xcoffSym syms = new slice<xcoffSym>(new xcoffSym[] {  });

    var ldr = ctxt.loader;
    var name = ldr.SymName(x);
    if (t == UndefinedSym) {
        name = ldr.SymExtname(x);
    }

    if (t == TextSym) 
        if (ldr.SymPkg(x) != "" || strings.Contains(name, "-tramp") || strings.HasPrefix(name, "runtime.text.")) { 
            // Function within a file
            syms = xfile.writeSymbolFunc(ctxt, x);

        }
        else
 { 
            // Only runtime.text and runtime.etext come through this way
            if (name != "runtime.text" && name != "runtime.etext" && name != "go.buildid") {
                Exitf("putaixsym: unknown text symbol %s", name);
            }

            ptr<XcoffSymEnt64> s = addr(new XcoffSymEnt64(Nsclass:C_HIDEXT,Noffset:uint32(xfile.stringTable.add(name)),Nvalue:uint64(ldr.SymValue(x)),Nscnum:xfile.getXCOFFscnum(ldr.SymSect(x)),Ntype:SYM_TYPE_FUNC,Nnumaux:1,));
            ldr.SetSymDynid(x, int32(xfile.symbolCount));
            syms = append(syms, s);

            var size = uint64(ldr.SymSize(x));
            ptr<XcoffAuxCSect64> a4 = addr(new XcoffAuxCSect64(Xauxtype:_AUX_CSECT,Xscnlenlo:uint32(size&0xFFFFFFFF),Xscnlenhi:uint32(size>>32),Xsmclas:XMC_PR,Xsmtyp:XTY_SD,));
            a4.Xsmtyp |= uint8(xcoffAlign(_addr_ldr, x, TextSym) << 3);
            syms = append(syms, a4);

        }
    else if (t == DataSym || t == BSSSym) 
        s = addr(new XcoffSymEnt64(Nsclass:C_EXT,Noffset:uint32(xfile.stringTable.add(name)),Nvalue:uint64(ldr.SymValue(x)),Nscnum:xfile.getXCOFFscnum(ldr.SymSect(x)),Nnumaux:1,));

        if (ldr.IsFileLocal(x) || ldr.AttrVisibilityHidden(x) || ldr.AttrLocal(x)) { 
            // There is more symbols in the case of a global data
            // which are related to the assembly generated
            // to access such symbols.
            // But as Golang as its own way to check if a symbol is
            // global or local (the capital letter), we don't need to
            // implement them yet.
            s.Nsclass = C_HIDEXT;

        }
        ldr.SetSymDynid(x, int32(xfile.symbolCount));
        syms = append(syms, s); 

        // Create auxiliary entry

        // Normally, size should be the size of csect containing all
        // the data and bss symbols of one file/package.
        // However, it's easier to just have a csect for each symbol.
        // It might change
        size = uint64(ldr.SymSize(x));
        a4 = addr(new XcoffAuxCSect64(Xauxtype:_AUX_CSECT,Xscnlenlo:uint32(size&0xFFFFFFFF),Xscnlenhi:uint32(size>>32),));

        {
            var ty__prev1 = ty;

            var ty = ldr.SymType(x);

            if (ty >= sym.STYPE && ty <= sym.SPCLNTAB) {
                if (ctxt.IsExternal() && strings.HasPrefix(ldr.SymSect(x).Name, ".data.rel.ro")) { 
                    // During external linking, read-only datas with relocation
                    // must be in .data.
                    a4.Xsmclas = XMC_RW;

                }
                else
 { 
                    // Read only data
                    a4.Xsmclas = XMC_RO;

                }

            }
            else if (strings.HasPrefix(ldr.SymName(x), "TOC.") && ctxt.IsExternal()) {
                a4.Xsmclas = XMC_TC;
            }
            else if (ldr.SymName(x) == "TOC") {
                a4.Xsmclas = XMC_TC0;
            }
            else
 {
                a4.Xsmclas = XMC_RW;
            }


            ty = ty__prev1;

        }

        if (t == DataSym) {
            a4.Xsmtyp |= XTY_SD;
        }
        else
 {
            a4.Xsmtyp |= XTY_CM;
        }
        a4.Xsmtyp |= uint8(xcoffAlign(_addr_ldr, x, t) << 3);

        syms = append(syms, a4);
    else if (t == UndefinedSym) 
        {
            var ty__prev1 = ty;

            ty = ldr.SymType(x);

            if (ty != sym.SDYNIMPORT && ty != sym.SHOSTOBJ && ty != sym.SUNDEFEXT) {
                return ;
            }

            ty = ty__prev1;

        }

        s = addr(new XcoffSymEnt64(Nsclass:C_EXT,Noffset:uint32(xfile.stringTable.add(name)),Nnumaux:1,));
        ldr.SetSymDynid(x, int32(xfile.symbolCount));
        syms = append(syms, s);

        a4 = addr(new XcoffAuxCSect64(Xauxtype:_AUX_CSECT,Xsmclas:XMC_DS,Xsmtyp:XTY_ER|XTY_IMP,));

        if (ldr.SymName(x) == "__n_pthreads") { 
            // Currently, all imported symbols made by cgo_import_dynamic are
            // syscall functions, except __n_pthreads which is a variable.
            // TODO(aix): Find a way to detect variables imported by cgo.
            a4.Xsmclas = XMC_RW;

        }
        syms = append(syms, a4);
    else if (t == TLSSym) 
        s = addr(new XcoffSymEnt64(Nsclass:C_EXT,Noffset:uint32(xfile.stringTable.add(name)),Nscnum:xfile.getXCOFFscnum(ldr.SymSect(x)),Nvalue:uint64(ldr.SymValue(x)),Nnumaux:1,));

        ldr.SetSymDynid(x, int32(xfile.symbolCount));
        syms = append(syms, s);

        size = uint64(ldr.SymSize(x));
        a4 = addr(new XcoffAuxCSect64(Xauxtype:_AUX_CSECT,Xsmclas:XMC_UL,Xsmtyp:XTY_CM,Xscnlenlo:uint32(size&0xFFFFFFFF),Xscnlenhi:uint32(size>>32),));

        syms = append(syms, a4);
    else 
        return ;
        {
        ptr<XcoffSymEnt64> s__prev1 = s;

        foreach (var (_, __s) in syms) {
            s = __s;
            xfile.addSymbol(s);
        }
        s = s__prev1;
    }
}

// Generate XCOFF Symbol table.
// It will be written in out file in Asmbxcoff, because it must be
// at the very end, especially after relocation sections which needs symbols' index.
private static void asmaixsym(this ptr<xcoffFile> _addr_f, ptr<Link> _addr_ctxt) {
    ref xcoffFile f = ref _addr_f.val;
    ref Link ctxt = ref _addr_ctxt.val;

    var ldr = ctxt.loader; 
    // Get correct size for symbols wrapping others symbols like go.string.*
    // sym.Size can be used directly as the symbols have already been written.
    foreach (var (name, size) in outerSymSize) {
        var sym = ldr.Lookup(name, 0);
        if (sym == 0) {
            Errorf(null, "unknown outer symbol with name %s", name);
        }
        else
 {
            var s = ldr.MakeSymbolUpdater(sym);
            s.SetSize(size);
        }
    }    s = ldr.Lookup("runtime.text", 0);
    if (ldr.SymType(s) == sym.STEXT) { 
        // We've already included this symbol in ctxt.Textp on AIX with external linker.
        // See data.go:/textaddress
        if (!ctxt.IsExternal()) {
            putaixsym(_addr_ctxt, s, TextSym);
        }
    }
    nint n = 1; 
    // Generate base addresses for all text sections if there are multiple
    foreach (var (_, sect) in Segtext.Sections[(int)1..]) {
        if (sect.Name != ".text" || ctxt.IsExternal()) { 
            // On AIX, runtime.text.X are symbols already in the symtab.
            break;

        }
        s = ldr.Lookup(fmt.Sprintf("runtime.text.%d", n), 0);
        if (s == 0) {
            break;
        }
        if (ldr.SymType(s) == sym.STEXT) {
            putaixsym(_addr_ctxt, s, TextSym);
        }
        n++;

    }    s = ldr.Lookup("runtime.etext", 0);
    if (ldr.SymType(s) == sym.STEXT) { 
        // We've already included this symbol in ctxt.Textp
        // on AIX with external linker.
        // See data.go:/textaddress
        if (!ctxt.IsExternal()) {
            putaixsym(_addr_ctxt, s, TextSym);
        }
    }
    Func<loader.Sym, @string, bool> shouldBeInSymbolTable = (s, name) => {
        if (name == ".go.buildinfo") { 
            // On AIX, .go.buildinfo must be in the symbol table as
            // it has relocations.
            return true;

        }
        if (ldr.AttrNotInSymbolTable(s)) {
            return false;
        }
        if ((name == "" || name[0] == '.') && !ldr.IsFileLocal(s) && name != ".TOC.") {
            return false;
        }
        return true;

    };

    {
        var s__prev1 = s;

        for (s = loader.Sym(1);
        var nsym = loader.Sym(ldr.NSym()); s < nsym; s++) {
            if (!shouldBeInSymbolTable(s, ldr.SymName(s))) {
                continue;
            }
            var st = ldr.SymType(s);

            if (st == sym.STLSBSS) 
                if (ctxt.IsExternal()) {
                    putaixsym(_addr_ctxt, s, TLSSym);
                }
            else if (st == sym.SBSS || st == sym.SNOPTRBSS || st == sym.SLIBFUZZER_EXTRA_COUNTER) 
                if (ldr.AttrReachable(s)) {
                    var data = ldr.Data(s);
                    if (len(data) > 0) {
                        ldr.Errorf(s, "should not be bss (size=%d type=%v special=%v)", len(data), ldr.SymType(s), ldr.AttrSpecial(s));
                    }
                    putaixsym(_addr_ctxt, s, BSSSym);
                }
            else if (st >= sym.SELFRXSECT && st < sym.SXREF) // data sections handled in dodata
                if (ldr.AttrReachable(s)) {
                    putaixsym(_addr_ctxt, s, DataSym);
                }
            else if (st == sym.SUNDEFEXT) 
                putaixsym(_addr_ctxt, s, UndefinedSym);
            else if (st == sym.SDYNIMPORT) 
                if (ldr.AttrReachable(s)) {
                    putaixsym(_addr_ctxt, s, UndefinedSym);
                }
            
        }

        s = s__prev1;
    }

    {
        var s__prev1 = s;

        foreach (var (_, __s) in ctxt.Textp) {
            s = __s;
            putaixsym(_addr_ctxt, s, TextSym);
        }
        s = s__prev1;
    }

    if (ctxt.Debugvlog != 0 || flagN.val) {
        ctxt.Logf("symsize = %d\n", uint32(symSize));
    }
    xfile.updatePreviousFile(ctxt, true);

}

private static void genDynSym(this ptr<xcoffFile> _addr_f, ptr<Link> _addr_ctxt) {
    ref xcoffFile f = ref _addr_f.val;
    ref Link ctxt = ref _addr_ctxt.val;

    var ldr = ctxt.loader;
    slice<loader.Sym> dynsyms = default;
    {
        var s__prev1 = s;

        for (var s = loader.Sym(1); s < loader.Sym(ldr.NSym()); s++) {
            if (!ldr.AttrReachable(s)) {
                continue;
            }
            {
                var t = ldr.SymType(s);

                if (t != sym.SHOSTOBJ && t != sym.SDYNIMPORT) {
                    continue;
                }

            }

            dynsyms = append(dynsyms, s);

        }

        s = s__prev1;
    }

    {
        var s__prev1 = s;

        foreach (var (_, __s) in dynsyms) {
            s = __s;
            f.adddynimpsym(ctxt, s);

            {
                var (_, ok) = f.dynLibraries[ldr.SymDynimplib(s)];

                if (!ok) {
                    f.dynLibraries[ldr.SymDynimplib(s)] = len(f.dynLibraries);
                }

            }

        }
        s = s__prev1;
    }
}

// (*xcoffFile)adddynimpsym adds the dynamic symbol "s" to a XCOFF file.
// A new symbol named s.Extname() is created to be the actual dynamic symbol
// in the .loader section and in the symbol table as an External Reference.
// The symbol "s" is transformed to SXCOFFTOC to end up in .data section.
// However, there is no writing protection on those symbols and
// it might need to be added.
// TODO(aix): Handles dynamic symbols without library.
private static void adddynimpsym(this ptr<xcoffFile> _addr_f, ptr<Link> _addr_ctxt, loader.Sym s) {
    ref xcoffFile f = ref _addr_f.val;
    ref Link ctxt = ref _addr_ctxt.val;
 
    // Check that library name is given.
    // Pattern is already checked when compiling.
    var ldr = ctxt.loader;
    if (ctxt.IsInternal() && ldr.SymDynimplib(s) == "") {
        ctxt.Errorf(s, "imported symbol must have a given library");
    }
    var sb = ldr.MakeSymbolUpdater(s);
    sb.SetReachable(true);
    sb.SetType(sym.SXCOFFTOC); 

    // Create new dynamic symbol
    var extsym = ldr.CreateSymForUpdate(ldr.SymExtname(s), 0);
    extsym.SetType(sym.SDYNIMPORT);
    extsym.SetDynimplib(ldr.SymDynimplib(s));
    extsym.SetExtname(ldr.SymExtname(s));
    extsym.SetDynimpvers(ldr.SymDynimpvers(s)); 

    // Add loader symbol
    ptr<xcoffLoaderSymbol> lds = addr(new xcoffLoaderSymbol(sym:extsym.Sym(),smtype:XTY_IMP,smclas:XMC_DS,));
    if (ldr.SymName(s) == "__n_pthreads") { 
        // Currently, all imported symbols made by cgo_import_dynamic are
        // syscall functions, except __n_pthreads which is a variable.
        // TODO(aix): Find a way to detect variables imported by cgo.
        lds.smclas = XMC_RW;

    }
    f.loaderSymbols = append(f.loaderSymbols, lds); 

    // Relocation to retrieve the external address
    sb.AddBytes(make_slice<byte>(8));
    var (r, _) = sb.AddRel(objabi.R_ADDR);
    r.SetSym(extsym.Sym());
    r.SetSiz(uint8(ctxt.Arch.PtrSize)); 
    // TODO: maybe this could be
    // sb.SetSize(0)
    // sb.SetData(nil)
    // sb.AddAddr(ctxt.Arch, extsym.Sym())
    // If the size is not 0 to begin with, I don't think the added 8 bytes
    // of zeros are necessary.
}

// Xcoffadddynrel adds a dynamic relocation in a XCOFF file.
// This relocation will be made by the loader.
public static bool Xcoffadddynrel(ptr<Target> _addr_target, ptr<loader.Loader> _addr_ldr, ptr<ArchSyms> _addr_syms, loader.Sym s, loader.Reloc r, nint rIdx) {
    ref Target target = ref _addr_target.val;
    ref loader.Loader ldr = ref _addr_ldr.val;
    ref ArchSyms syms = ref _addr_syms.val;

    if (target.IsExternal()) {
        return true;
    }
    if (ldr.SymType(s) <= sym.SPCLNTAB) {
        ldr.Errorf(s, "cannot have a relocation to %s in a text section symbol", ldr.SymName(r.Sym()));
        return false;
    }
    ptr<xcoffLoaderReloc> xldr = addr(new xcoffLoaderReloc(sym:s,roff:r.Off(),));
    var targ = ldr.ResolveABIAlias(r.Sym());
    sym.SymKind targType = default;
    if (targ != 0) {
        targType = ldr.SymType(targ);
    }

    if (r.Type() == objabi.R_ADDR) 
        if (ldr.SymType(s) == sym.SXCOFFTOC && targType == sym.SDYNIMPORT) { 
            // Imported symbol relocation
            foreach (var (i, dynsym) in xfile.loaderSymbols) {
                if (ldr.SymName(dynsym.sym) == ldr.SymName(targ)) {
                    xldr.symndx = int32(i + 3); // +3 because of 3 section symbols
                    break;

                }

            }

        }        {
            var t = ldr.SymType(s);


            else if (t == sym.SDATA || t == sym.SNOPTRDATA || t == sym.SBUILDINFO || t == sym.SXCOFFTOC) {

                if (ldr.SymSect(targ).Seg == _addr_Segtext)                 else if (ldr.SymSect(targ).Seg == _addr_Segrodata) 
                    xldr.symndx = 0; // .text
                else if (ldr.SymSect(targ).Seg == _addr_Segdata) 
                    if (targType == sym.SBSS || targType == sym.SNOPTRBSS) {
                        xldr.symndx = 2; // .bss
                    }
                    else
 {
                        xldr.symndx = 1; // .data
                    }

                else 
                    ldr.Errorf(s, "unknown segment for .loader relocation with symbol %s", ldr.SymName(targ));
                
            }
            else
 {
                ldr.Errorf(s, "unexpected type for .loader relocation R_ADDR for symbol %s: %s to %s", ldr.SymName(targ), ldr.SymType(s), ldr.SymType(targ));
                return false;
            }

        }


        xldr.rtype = 0x3F << 8 + XCOFF_R_POS;
    else 
        ldr.Errorf(s, "unexpected .loader relocation to symbol: %s (type: %s)", ldr.SymName(targ), r.Type().String());
        return false;
        xfile.Lock();
    xfile.loaderReloc = append(xfile.loaderReloc, xldr);
    xfile.Unlock();
    return true;

}

private static void doxcoff(this ptr<Link> _addr_ctxt) => func((_, panic, _) => {
    ref Link ctxt = ref _addr_ctxt.val;

    if (FlagD.val) { 
        // All XCOFF files have dynamic symbols because of the syscalls.
        Exitf("-d is not available on AIX");

    }
    var ldr = ctxt.loader; 

    // TOC
    var toc = ldr.CreateSymForUpdate("TOC", 0);
    toc.SetType(sym.SXCOFFTOC);
    toc.SetVisibilityHidden(true); 

    // Add entry point to .loader symbols.
    var ep = ldr.Lookup(flagEntrySymbol.val, 0);
    if (ep == 0 || !ldr.AttrReachable(ep)) {
        Exitf("wrong entry point");
    }
    xfile.loaderSymbols = append(xfile.loaderSymbols, addr(new xcoffLoaderSymbol(sym:ep,smtype:XTY_ENT|XTY_SD,smclas:XMC_DS,)));

    xfile.genDynSym(ctxt);

    {
        var s__prev1 = s;

        for (var s = loader.Sym(1); s < loader.Sym(ldr.NSym()); s++) {
            if (strings.HasPrefix(ldr.SymName(s), "TOC.")) {
                var sb = ldr.MakeSymbolUpdater(s);
                sb.SetType(sym.SXCOFFTOC);
            }
        }

        s = s__prev1;
    }

    if (ctxt.IsExternal()) { 
        // Change rt0_go name to match name in runtime/cgo:main().
        var rt0 = ldr.Lookup("runtime.rt0_go", 0);
        ldr.SetSymExtname(rt0, "runtime_rt0_go");

        var nsym = loader.Sym(ldr.NSym());
        {
            var s__prev1 = s;

            for (s = loader.Sym(1); s < nsym; s++) {
                if (!ldr.AttrCgoExport(s)) {
                    continue;
                }
                if (ldr.IsFileLocal(s)) {
                    panic("cgo_export on static symbol");
                }
                if (ldr.SymType(s) == sym.STEXT || ldr.SymType(s) == sym.SABIALIAS) { 
                    // On AIX, a exported function must have two symbols:
                    // - a .text symbol which must start with a ".".
                    // - a .data symbol which is a function descriptor.
                    var name = ldr.SymExtname(s);
                    ldr.SetSymExtname(s, "." + name);

                    var desc = ldr.MakeSymbolUpdater(ldr.CreateExtSym(name, 0));
                    desc.SetReachable(true);
                    desc.SetType(sym.SNOPTRDATA);
                    desc.AddAddr(ctxt.Arch, s);
                    desc.AddAddr(ctxt.Arch, toc.Sym());
                    desc.AddUint64(ctxt.Arch, 0);

                }

            }


            s = s__prev1;
        }

    }
});

// Loader section
// Currently, this section is created from scratch when assembling the XCOFF file
// according to information retrieved in xfile object.

// Create loader section and returns its size
public static void Loaderblk(ptr<Link> _addr_ctxt, ulong off) {
    ref Link ctxt = ref _addr_ctxt.val;

    xfile.writeLdrScn(ctxt, off);
}

private static void writeLdrScn(this ptr<xcoffFile> _addr_f, ptr<Link> _addr_ctxt, ulong globalOff) => func((_, panic, _) => {
    ref xcoffFile f = ref _addr_f.val;
    ref Link ctxt = ref _addr_ctxt.val;

    slice<ptr<XcoffLdSym64>> symtab = default;
    slice<ptr<XcoffLdStr64>> strtab = default;
    slice<ptr<XcoffLdImportFile64>> importtab = default;
    slice<ptr<XcoffLdRel64>> reloctab = default;
    slice<ptr<XcoffLdRel64>> dynimpreloc = default; 

    // As the string table is updated in any loader subsection,
    //  its length must be computed at the same time.
    var stlen = uint32(0); 

    // Loader Header
    ptr<XcoffLdHdr64> hdr = addr(new XcoffLdHdr64(Lversion:2,Lsymoff:LDHDRSZ_64,));

    var ldr = ctxt.loader; 
    /* Symbol table */
    {
        var s__prev1 = s;

        foreach (var (_, __s) in f.loaderSymbols) {
            s = __s;
            ptr<XcoffLdSym64> lds = addr(new XcoffLdSym64(Loffset:uint32(stlen+2),Lsmtype:s.smtype,Lsmclas:s.smclas,));
            var sym = s.sym;

            if (s.smtype == XTY_ENT | XTY_SD) 
                lds.Lvalue = uint64(ldr.SymValue(sym));
                lds.Lscnum = f.getXCOFFscnum(ldr.SymSect(sym));
            else if (s.smtype == XTY_IMP) 
                lds.Lifile = int32(f.dynLibraries[ldr.SymDynimplib(sym)] + 1);
            else 
                ldr.Errorf(sym, "unexpected loader symbol type: 0x%x", s.smtype);
                        ptr<XcoffLdStr64> ldstr = addr(new XcoffLdStr64(size:uint16(len(ldr.SymName(sym))+1),name:ldr.SymName(sym),));
            stlen += uint32(2 + ldstr.size); // 2 = sizeof ldstr.size
            symtab = append(symtab, lds);
            strtab = append(strtab, ldstr);


        }
        s = s__prev1;
    }

    hdr.Lnsyms = int32(len(symtab));
    hdr.Lrldoff = hdr.Lsymoff + uint64(24 * hdr.Lnsyms); // 24 = sizeof one symbol
    var off = hdr.Lrldoff; // current offset is the same of reloc offset

    /* Reloc */
    // Ensure deterministic order
    sort.Slice(f.loaderReloc, (i, j) => {
        var r1 = f.loaderReloc[i];
        var r2 = f.loaderReloc[j];
        if (r1.sym != r2.sym) {
            return r1.sym < r2.sym;
        }
        if (r1.roff != r2.roff) {
            return r1.roff < r2.roff;
        }
        if (r1.rtype != r2.rtype) {
            return r1.rtype < r2.rtype;
        }
        return r1.symndx < r2.symndx;

    });

    var ep = ldr.Lookup(flagEntrySymbol.val, 0);
    ptr<XcoffLdRel64> xldr = addr(new XcoffLdRel64(Lvaddr:uint64(ldr.SymValue(ep)),Lrtype:0x3F00,Lrsecnm:f.getXCOFFscnum(ldr.SymSect(ep)),Lsymndx:0,));
    off += 16;
    reloctab = append(reloctab, xldr);

    off += uint64(16 * len(f.loaderReloc));
    {
        var r__prev1 = r;

        foreach (var (_, __r) in f.loaderReloc) {
            r = __r;
            var symp = r.sym;
            if (symp == 0) {
                panic("unexpected 0 sym value");
            }
            xldr = addr(new XcoffLdRel64(Lvaddr:uint64(ldr.SymValue(symp)+int64(r.roff)),Lrtype:r.rtype,Lsymndx:r.symndx,));

            if (ldr.SymSect(symp) != null) {
                xldr.Lrsecnm = f.getXCOFFscnum(ldr.SymSect(symp));
            }
            reloctab = append(reloctab, xldr);
        }
        r = r__prev1;
    }

    off += uint64(16 * len(dynimpreloc));
    reloctab = append(reloctab, dynimpreloc);

    hdr.Lnreloc = int32(len(reloctab));
    hdr.Limpoff = off;

    /* Import */
    // Default import: /usr/lib:/lib
    ptr<XcoffLdImportFile64> ldimpf = addr(new XcoffLdImportFile64(Limpidpath:"/usr/lib:/lib",));
    off += uint64(len(ldimpf.Limpidpath) + len(ldimpf.Limpidbase) + len(ldimpf.Limpidmem) + 3); // + null delimiter
    importtab = append(importtab, ldimpf); 

    // The map created by adddynimpsym associates the name to a number
    // This number represents the librairie index (- 1) in this import files section
    // Therefore, they must be sorted before being put inside the section
    var libsOrdered = make_slice<@string>(len(f.dynLibraries));
    foreach (var (key, val) in f.dynLibraries) {
        if (libsOrdered[val] != "") {
            continue;
        }
        libsOrdered[val] = key;

    }    foreach (var (_, lib) in libsOrdered) { 
        // lib string is defined as base.a/mem.o or path/base.a/mem.o
        var n = strings.Split(lib, "/");
        @string path = "";
        var @base = n[len(n) - 2];
        var mem = n[len(n) - 1];
        if (len(n) > 2) {
            path = lib[..(int)len(lib) - len(base) - len(mem) - 2];
        }
        ldimpf = addr(new XcoffLdImportFile64(Limpidpath:path,Limpidbase:base,Limpidmem:mem,));
        off += uint64(len(ldimpf.Limpidpath) + len(ldimpf.Limpidbase) + len(ldimpf.Limpidmem) + 3); // + null delimiter
        importtab = append(importtab, ldimpf);

    }    hdr.Lnimpid = int32(len(importtab));
    hdr.Listlen = uint32(off - hdr.Limpoff);
    hdr.Lstoff = off;
    hdr.Lstlen = stlen; 

    /* Writing */
    ctxt.Out.SeekSet(int64(globalOff));
    binary.Write(ctxt.Out, ctxt.Arch.ByteOrder, hdr);

    {
        var s__prev1 = s;

        foreach (var (_, __s) in symtab) {
            s = __s;
            binary.Write(ctxt.Out, ctxt.Arch.ByteOrder, s);
        }
        s = s__prev1;
    }

    {
        var r__prev1 = r;

        foreach (var (_, __r) in reloctab) {
            r = __r;
            binary.Write(ctxt.Out, ctxt.Arch.ByteOrder, r);
        }
        r = r__prev1;
    }

    foreach (var (_, f) in importtab) {
        ctxt.Out.WriteString(f.Limpidpath);
        ctxt.Out.Write8(0);
        ctxt.Out.WriteString(f.Limpidbase);
        ctxt.Out.Write8(0);
        ctxt.Out.WriteString(f.Limpidmem);
        ctxt.Out.Write8(0);
    }    {
        var s__prev1 = s;

        foreach (var (_, __s) in strtab) {
            s = __s;
            ctxt.Out.Write16(s.size);
            ctxt.Out.WriteString(s.name);
            ctxt.Out.Write8(0); // null terminator
        }
        s = s__prev1;
    }

    f.loaderSize = off + uint64(stlen);

});

// XCOFF assembling and writing file

private static void writeFileHeader(this ptr<xcoffFile> _addr_f, ptr<Link> _addr_ctxt) {
    ref xcoffFile f = ref _addr_f.val;
    ref Link ctxt = ref _addr_ctxt.val;
 
    // File header
    f.xfhdr.Fmagic = U64_TOCMAGIC;
    f.xfhdr.Fnscns = uint16(len(f.sections));
    f.xfhdr.Ftimedat = 0;

    if (!FlagS.val) {
        f.xfhdr.Fsymptr = uint64(f.symtabOffset);
        f.xfhdr.Fnsyms = int32(f.symbolCount);
    }
    if (ctxt.BuildMode == BuildModeExe && ctxt.LinkMode == LinkInternal) {
        var ldr = ctxt.loader;
        f.xfhdr.Fopthdr = AOUTHSZ_EXEC64;
        f.xfhdr.Fflags = F_EXEC; 

        // auxiliary header
        f.xahdr.Ovstamp = 1; // based on dump -o
        f.xahdr.Omagic = 0x10b;
        copy(f.xahdr.Omodtype[..], "1L");
        var entry = ldr.Lookup(flagEntrySymbol.val, 0);
        f.xahdr.Oentry = uint64(ldr.SymValue(entry));
        f.xahdr.Osnentry = f.getXCOFFscnum(ldr.SymSect(entry));
        var toc = ldr.Lookup("TOC", 0);
        f.xahdr.Otoc = uint64(ldr.SymValue(toc));
        f.xahdr.Osntoc = f.getXCOFFscnum(ldr.SymSect(toc));

        f.xahdr.Oalgntext = int16(logBase2(int(XCOFFSECTALIGN)));
        f.xahdr.Oalgndata = 0x5;

        binary.Write(ctxt.Out, binary.BigEndian, _addr_f.xfhdr);
        binary.Write(ctxt.Out, binary.BigEndian, _addr_f.xahdr);

    }
    else
 {
        f.xfhdr.Fopthdr = 0;
        binary.Write(ctxt.Out, binary.BigEndian, _addr_f.xfhdr);
    }
}

private static void xcoffwrite(ptr<Link> _addr_ctxt) {
    ref Link ctxt = ref _addr_ctxt.val;

    ctxt.Out.SeekSet(0);

    xfile.writeFileHeader(ctxt);

    foreach (var (_, sect) in xfile.sections) {
        sect.write(ctxt);
    }
}

// Generate XCOFF assembly file
private static void asmbXcoff(ptr<Link> _addr_ctxt) {
    ref Link ctxt = ref _addr_ctxt.val;

    ctxt.Out.SeekSet(0);
    var fileoff = int64(Segdwarf.Fileoff + Segdwarf.Filelen);
    fileoff = int64(Rnd(int64(fileoff), int64(FlagRound.val)));

    xfile.sectNameToScnum = make_map<@string, short>(); 

    // Add sections
    var s = xfile.addSection(".text", Segtext.Vaddr, Segtext.Length, Segtext.Fileoff, STYP_TEXT);
    xfile.xahdr.Otextstart = s.Svaddr;
    xfile.xahdr.Osntext = xfile.sectNameToScnum[".text"];
    xfile.xahdr.Otsize = s.Ssize;
    xfile.sectText = s;

    var segdataVaddr = Segdata.Vaddr;
    var segdataFilelen = Segdata.Filelen;
    var segdataFileoff = Segdata.Fileoff;
    var segbssFilelen = Segdata.Length - Segdata.Filelen;
    if (len(Segrelrodata.Sections) > 0) { 
        // Merge relro segment to data segment as
        // relro data are inside data segment on AIX.
        segdataVaddr = Segrelrodata.Vaddr;
        segdataFileoff = Segrelrodata.Fileoff;
        segdataFilelen = Segdata.Vaddr + Segdata.Filelen - Segrelrodata.Vaddr;

    }
    s = xfile.addSection(".data", segdataVaddr, segdataFilelen, segdataFileoff, STYP_DATA);
    xfile.xahdr.Odatastart = s.Svaddr;
    xfile.xahdr.Osndata = xfile.sectNameToScnum[".data"];
    xfile.xahdr.Odsize = s.Ssize;
    xfile.sectData = s;

    s = xfile.addSection(".bss", segdataVaddr + segdataFilelen, segbssFilelen, 0, STYP_BSS);
    xfile.xahdr.Osnbss = xfile.sectNameToScnum[".bss"];
    xfile.xahdr.Obsize = s.Ssize;
    xfile.sectBss = s;

    if (ctxt.LinkMode == LinkExternal) {
        ptr<sym.Section> tbss;
        {
            var s__prev1 = s;

            foreach (var (_, __s) in Segdata.Sections) {
                s = __s;
                if (s.Name == ".tbss") {
                    tbss = s;
                    break;
                }
            }

            s = s__prev1;
        }

        s = xfile.addSection(".tbss", tbss.Vaddr, tbss.Length, 0, STYP_TBSS);

    }
    foreach (var (_, sect) in Segdwarf.Sections) {
        xfile.addDwarfSection(sect);
    }    if (ctxt.LinkMode == LinkInternal) { 
        // Loader section
        if (ctxt.BuildMode == BuildModeExe) {
            Loaderblk(_addr_ctxt, uint64(fileoff));
            s = xfile.addSection(".loader", 0, xfile.loaderSize, uint64(fileoff), STYP_LOADER);
            xfile.xahdr.Osnloader = xfile.sectNameToScnum[".loader"]; 

            // Update fileoff for symbol table
            fileoff += int64(xfile.loaderSize);

        }
    }
    xfile.asmaixsym(ctxt);

    if (ctxt.LinkMode == LinkExternal) {
        xfile.emitRelocations(ctxt, fileoff);
    }
    xfile.symtabOffset = ctxt.Out.Offset();
    {
        var s__prev1 = s;

        foreach (var (_, __s) in xfile.symtabSym) {
            s = __s;
            binary.Write(ctxt.Out, ctxt.Arch.ByteOrder, s);
        }
        s = s__prev1;
    }

    xfile.stringTable.write(ctxt.Out); 

    // write headers
    xcoffwrite(_addr_ctxt);

}

// emitRelocations emits relocation entries for go.o in external linking.
private static void emitRelocations(this ptr<xcoffFile> _addr_f, ptr<Link> _addr_ctxt, long fileoff) => func((_, panic, _) => {
    ref xcoffFile f = ref _addr_f.val;
    ref Link ctxt = ref _addr_ctxt.val;

    ctxt.Out.SeekSet(fileoff);
    while (ctxt.Out.Offset() & 7 != 0) {
        ctxt.Out.Write8(0);
    }

    var ldr = ctxt.loader; 
    // relocsect relocates symbols from first in section sect, and returns
    // the total number of relocations emitted.
    Func<ptr<sym.Section>, slice<loader.Sym>, ulong, uint> relocsect = (sect, syms, @base) => { 
        // ctxt.Logf("%s 0x%x\n", sect.Name, sect.Vaddr)
        // If main section has no bits, nothing to relocate.
        if (sect.Vaddr >= sect.Seg.Vaddr + sect.Seg.Filelen) {
            return 0;
        }
        sect.Reloff = uint64(ctxt.Out.Offset());
        {
            var i__prev1 = i;
            var s__prev1 = s;

            foreach (var (__i, __s) in syms) {
                i = __i;
                s = __s;
                if (!ldr.AttrReachable(s)) {
                    continue;
                }
                if (uint64(ldr.SymValue(s)) >= sect.Vaddr) {
                    syms = syms[(int)i..];
                    break;
                }
            }

            i = i__prev1;
            s = s__prev1;
        }

        var eaddr = int64(sect.Vaddr + sect.Length);
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

                // Compute external relocations on the go, and pass to Xcoffreloc1 to stream out.
                // Relocation must be ordered by address, so create a list of sorted indices.
                var relocs = ldr.Relocs(s);
                var sorted = make_slice<nint>(relocs.Count());
                {
                    var i__prev2 = i;

                    for (nint i = 0; i < relocs.Count(); i++) {
                        sorted[i] = i;
                    }


                    i = i__prev2;
                }
                sort.Slice(sorted, (i, j) => {
                    return relocs.At(sorted[i]).Off() < relocs.At(sorted[j]).Off();
                });

                foreach (var (_, ri) in sorted) {
                    var r = relocs.At(ri);
                    var (rr, ok) = extreloc(ctxt, ldr, s, r);
                    if (!ok) {
                        continue;
                    }
                    if (rr.Xsym == 0) {
                        ldr.Errorf(s, "missing xsym in relocation");
                        continue;
                    }
                    if (ldr.SymDynid(rr.Xsym) < 0) {
                        ldr.Errorf(s, "reloc %s to non-coff symbol %s (outer=%s) %d %d", r.Type(), ldr.SymName(r.Sym()), ldr.SymName(rr.Xsym), ldr.SymType(r.Sym()), ldr.SymDynid(rr.Xsym));
                    }
                    if (!thearch.Xcoffreloc1(ctxt.Arch, ctxt.Out, ldr, s, rr, int64(uint64(ldr.SymValue(s) + int64(r.Off())) - base))) {
                        ldr.Errorf(s, "unsupported obj reloc %d(%s)/%d to %s", r.Type(), r.Type(), r.Siz(), ldr.SymName(r.Sym()));
                    }
                }

            }

            s = s__prev1;
        }

        sect.Rellen = uint64(ctxt.Out.Offset()) - sect.Reloff;
        return uint32(sect.Rellen) / RELSZ_64;

    };
    {
        var s__prev1 = s;

        foreach (var (_, __s) in sects) {
            s = __s;
            s.xcoffSect.Srelptr = uint64(ctxt.Out.Offset());
            var n = uint32(0);
            foreach (var (_, seg) in s.segs) {
                {
                    var sect__prev3 = sect;

                    foreach (var (_, __sect) in seg.Sections) {
                        sect = __sect;
                        if (sect.Name == ".text") {
                            n += relocsect(sect, ctxt.Textp, 0);
                        }
                        else
 {
                            n += relocsect(sect, ctxt.datap, 0);
                        }

                    }

                    sect = sect__prev3;
                }
            }
            s.xcoffSect.Snreloc += n;

        }
        s = s__prev1;
    }

dwarfLoop:
    {
        var i__prev1 = i;

        for (i = 0; i < len(Segdwarf.Sections); i++) {
            var sect = Segdwarf.Sections[i];
            var si = dwarfp[i];
            if (si.secSym() != loader.Sym(sect.Sym) || ldr.SymSect(si.secSym()) != sect) {
                panic("inconsistency between dwarfp and Segdwarf");
            }
            foreach (var (_, xcoffSect) in f.sections) {
                var (_, subtyp) = xcoffGetDwarfSubtype(sect.Name);
                if (xcoffSect.Sflags & 0xF0000 == subtyp) {
                    xcoffSect.Srelptr = uint64(ctxt.Out.Offset());
                    xcoffSect.Snreloc = relocsect(sect, si.syms, sect.Vaddr);
                    _continuedwarfLoop = true;
                    break;
                }

            }
            Errorf(null, "emitRelocations: could not find %q section", sect.Name);

        }

        i = i__prev1;
    }

});

// xcoffCreateExportFile creates a file with exported symbols for
// -Wl,-bE option.
// ld won't export symbols unless they are listed in an export file.
private static @string xcoffCreateExportFile(ptr<Link> _addr_ctxt) {
    @string fname = default;
    ref Link ctxt = ref _addr_ctxt.val;

    fname = filepath.Join(flagTmpdir.val, "export_file.exp");
    bytes.Buffer buf = default;

    var ldr = ctxt.loader;
    for (var s = loader.Sym(1);
    var nsym = loader.Sym(ldr.NSym()); s < nsym; s++) {
        if (!ldr.AttrCgoExport(s)) {
            continue;
        }
        var extname = ldr.SymExtname(s);
        if (!strings.HasPrefix(extname, "._cgoexp_")) {
            continue;
        }
        if (ldr.IsFileLocal(s)) {
            continue; // Only export non-static symbols
        }
        var name = strings.SplitN(extname, "_", 4)[3];

        buf.Write((slice<byte>)name + "\n");

    }

    var err = ioutil.WriteFile(fname, buf.Bytes(), 0666);
    if (err != null) {
        Errorf(null, "WriteFile %s failed: %v", fname, err);
    }
    return fname;

}

} // end ld_package
