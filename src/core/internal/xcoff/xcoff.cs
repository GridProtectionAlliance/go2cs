// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

partial class xcoff_package {

// File Header.
[GoType] partial struct FileHeader32 {
    public uint16 Fmagic; // Target machine
    public uint16 Fnscns; // Number of sections
    public uint32 Ftimedat; // Time and date of file creation
    public uint32 Fsymptr; // Byte offset to symbol table start
    public uint32 Fnsyms; // Number of entries in symbol table
    public uint16 Fopthdr; // Number of bytes in optional header
    public uint16 Fflags; // Flags
}

[GoType] partial struct FileHeader64 {
    public uint16 Fmagic; // Target machine
    public uint16 Fnscns; // Number of sections
    public uint32 Ftimedat; // Time and date of file creation
    public uint64 Fsymptr; // Byte offset to symbol table start
    public uint16 Fopthdr; // Number of bytes in optional header
    public uint16 Fflags; // Flags
    public uint32 Fnsyms; // Number of entries in symbol table
}

public static readonly UntypedInt FILHSZ_32 = 20;
public static readonly UntypedInt FILHSZ_64 = 24;

public static readonly UntypedInt U802TOCMAGIC = /* 0737 */ 479; // AIX 32-bit XCOFF
public static readonly UntypedInt U64_TOCMAGIC = /* 0767 */ 503; // AIX 64-bit XCOFF

// Flags that describe the type of the object file.
public static readonly UntypedInt F_RELFLG = /* 0x0001 */ 1;

public static readonly UntypedInt F_EXEC = /* 0x0002 */ 2;

public static readonly UntypedInt F_LNNO = /* 0x0004 */ 4;

public static readonly UntypedInt F_FDPR_PROF = /* 0x0010 */ 16;

public static readonly UntypedInt F_FDPR_OPTI = /* 0x0020 */ 32;

public static readonly UntypedInt F_DSA = /* 0x0040 */ 64;

public static readonly UntypedInt F_VARPG = /* 0x0100 */ 256;

public static readonly UntypedInt F_DYNLOAD = /* 0x1000 */ 4096;

public static readonly UntypedInt F_SHROBJ = /* 0x2000 */ 8192;

public static readonly UntypedInt F_LOADONLY = /* 0x4000 */ 16384;

// Section Header.
[GoType] partial struct SectionHeader32 {
    public array<byte> Sname = new(8); // Section name
    public uint32 Spaddr;  // Physical address
    public uint32 Svaddr;  // Virtual address
    public uint32 Ssize;  // Section size
    public uint32 Sscnptr;  // Offset in file to raw data for section
    public uint32 Srelptr;  // Offset in file to relocation entries for section
    public uint32 Slnnoptr;  // Offset in file to line number entries for section
    public uint16 Snreloc;  // Number of relocation entries
    public uint16 Snlnno;  // Number of line number entries
    public uint32 Sflags;  // Flags to define the section type
}

[GoType] partial struct SectionHeader64 {
    public array<byte> Sname = new(8); // Section name
    public uint64 Spaddr;  // Physical address
    public uint64 Svaddr;  // Virtual address
    public uint64 Ssize;  // Section size
    public uint64 Sscnptr;  // Offset in file to raw data for section
    public uint64 Srelptr;  // Offset in file to relocation entries for section
    public uint64 Slnnoptr;  // Offset in file to line number entries for section
    public uint32 Snreloc;  // Number of relocation entries
    public uint32 Snlnno;  // Number of line number entries
    public uint32 Sflags;  // Flags to define the section type
    public uint32 Spad;  // Needs to be 72 bytes long
}

// Flags defining the section type.
public static readonly UntypedInt STYP_DWARF = /* 0x0010 */ 16;

public static readonly UntypedInt STYP_TEXT = /* 0x0020 */ 32;

public static readonly UntypedInt STYP_DATA = /* 0x0040 */ 64;

public static readonly UntypedInt STYP_BSS = /* 0x0080 */ 128;

public static readonly UntypedInt STYP_EXCEPT = /* 0x0100 */ 256;

public static readonly UntypedInt STYP_INFO = /* 0x0200 */ 512;

public static readonly UntypedInt STYP_TDATA = /* 0x0400 */ 1024;

public static readonly UntypedInt STYP_TBSS = /* 0x0800 */ 2048;

public static readonly UntypedInt STYP_LOADER = /* 0x1000 */ 4096;

public static readonly UntypedInt STYP_DEBUG = /* 0x2000 */ 8192;

public static readonly UntypedInt STYP_TYPCHK = /* 0x4000 */ 16384;

public static readonly UntypedInt STYP_OVRFLO = /* 0x8000 */ 32768;

public static readonly UntypedInt SSUBTYP_DWINFO = /* 0x10000 */ 65536; // DWARF info section
public static readonly UntypedInt SSUBTYP_DWLINE = /* 0x20000 */ 131072; // DWARF line-number section
public static readonly UntypedInt SSUBTYP_DWPBNMS = /* 0x30000 */ 196608; // DWARF public names section
public static readonly UntypedInt SSUBTYP_DWPBTYP = /* 0x40000 */ 262144; // DWARF public types section
public static readonly UntypedInt SSUBTYP_DWARNGE = /* 0x50000 */ 327680; // DWARF aranges section
public static readonly UntypedInt SSUBTYP_DWABREV = /* 0x60000 */ 393216; // DWARF abbreviation section
public static readonly UntypedInt SSUBTYP_DWSTR = /* 0x70000 */ 458752; // DWARF strings section
public static readonly UntypedInt SSUBTYP_DWRNGES = /* 0x80000 */ 524288; // DWARF ranges section
public static readonly UntypedInt SSUBTYP_DWLOC = /* 0x90000 */ 589824; // DWARF location lists section
public static readonly UntypedInt SSUBTYP_DWFRAME = /* 0xA0000 */ 655360; // DWARF frames section
public static readonly UntypedInt SSUBTYP_DWMAC = /* 0xB0000 */ 720896; // DWARF macros section

// Symbol Table Entry.
[GoType] partial struct SymEnt32 {
    public array<byte> Nname = new(8); // Symbol name
    public uint32 Nvalue;  // Symbol value
    public uint16 Nscnum;  // Section number of symbol
    public uint16 Ntype;  // Basic and derived type specification
    public uint8 Nsclass;   // Storage class of symbol
    public uint8 Nnumaux;   // Number of auxiliary entries
}

[GoType] partial struct SymEnt64 {
    public uint64 Nvalue; // Symbol value
    public uint32 Noffset; // Offset of the name in string table or .debug section
    public uint16 Nscnum; // Section number of symbol
    public uint16 Ntype; // Basic and derived type specification
    public uint8 Nsclass;  // Storage class of symbol
    public uint8 Nnumaux;  // Number of auxiliary entries
}

public static readonly UntypedInt SYMESZ = 18;

public static readonly UntypedInt N_DEBUG = -2;
public static readonly UntypedInt N_ABS = -1;
public static readonly UntypedInt N_UNDEF = 0;
public static readonly UntypedInt SYM_V_INTERNAL = /* 0x1000 */ 4096;
public static readonly UntypedInt SYM_V_HIDDEN = /* 0x2000 */ 8192;
public static readonly UntypedInt SYM_V_PROTECTED = /* 0x3000 */ 12288;
public static readonly UntypedInt SYM_V_EXPORTED = /* 0x4000 */ 16384;
public static readonly UntypedInt SYM_TYPE_FUNC = /* 0x0020 */ 32; // is function

// Storage Class.
public static readonly UntypedInt C_NULL = 0; // Symbol table entry marked for deletion

public static readonly UntypedInt C_EXT = 2; // External symbol

public static readonly UntypedInt C_STAT = 3; // Static symbol

public static readonly UntypedInt C_BLOCK = 100; // Beginning or end of inner block

public static readonly UntypedInt C_FCN = 101; // Beginning or end of function

public static readonly UntypedInt C_FILE = 103; // Source file name and compiler information

public static readonly UntypedInt C_HIDEXT = 107; // Unnamed external symbol

public static readonly UntypedInt C_BINCL = 108; // Beginning of include file

public static readonly UntypedInt C_EINCL = 109; // End of include file

public static readonly UntypedInt C_WEAKEXT = 111; // Weak external symbol

public static readonly UntypedInt C_DWARF = 112; // DWARF symbol

public static readonly UntypedInt C_GSYM = 128; // Global variable

public static readonly UntypedInt C_LSYM = 129; // Automatic variable allocated on stack

public static readonly UntypedInt C_PSYM = 130; // Argument to subroutine allocated on stack

public static readonly UntypedInt C_RSYM = 131; // Register variable

public static readonly UntypedInt C_RPSYM = 132; // Argument to function or procedure stored in register

public static readonly UntypedInt C_STSYM = 133; // Statically allocated symbol

public static readonly UntypedInt C_BCOMM = 135; // Beginning of common block

public static readonly UntypedInt C_ECOML = 136; // Local member of common block

public static readonly UntypedInt C_ECOMM = 137; // End of common block

public static readonly UntypedInt C_DECL = 140; // Declaration of object

public static readonly UntypedInt C_ENTRY = 141; // Alternate entry

public static readonly UntypedInt C_FUN = 142; // Function or procedure

public static readonly UntypedInt C_BSTAT = 143; // Beginning of static block

public static readonly UntypedInt C_ESTAT = 144; // End of static block

public static readonly UntypedInt C_GTLS = 145; // Global thread-local variable

public static readonly UntypedInt C_STTLS = 146; // Static thread-local variable

// File Auxiliary Entry
[GoType] partial struct AuxFile64 {
    public array<byte> Xfname = new(8); // Name or offset inside string table
    public uint8 Xftype;   // Source file string type
    public uint8 Xauxtype;   // Type of auxiliary entry
}

// Function Auxiliary Entry
[GoType] partial struct AuxFcn32 {
    public uint32 Xexptr; // File offset to exception table entry
    public uint32 Xfsize; // Size of function in bytes
    public uint32 Xlnnoptr; // File pointer to line number
    public uint32 Xendndx; // Symbol table index of next entry
    public uint16 Xpad; // Unused
}

[GoType] partial struct AuxFcn64 {
    public uint64 Xlnnoptr; // File pointer to line number
    public uint32 Xfsize; // Size of function in bytes
    public uint32 Xendndx; // Symbol table index of next entry
    public uint8 Xpad;  // Unused
    public uint8 Xauxtype;  // Type of auxiliary entry
}

[GoType] partial struct AuxSect64 {
    public uint64 Xscnlen; // section length
    public uint64 Xnreloc; // Num RLDs
    internal uint8 pad;
    public uint8 Xauxtype; // Type of auxiliary entry
}

// csect Auxiliary Entry.
[GoType] partial struct AuxCSect32 {
    public uint32 Xscnlen; // Length or symbol table index
    public uint32 Xparmhash; // Offset of parameter type-check string
    public uint16 Xsnhash; // .typchk section number
    public uint8 Xsmtyp;  // Symbol alignment and type
    public uint8 Xsmclas;  // Storage-mapping class
    public uint32 Xstab; // Reserved
    public uint16 Xsnstab; // Reserved
}

[GoType] partial struct AuxCSect64 {
    public uint32 Xscnlenlo; // Lower 4 bytes of length or symbol table index
    public uint32 Xparmhash; // Offset of parameter type-check string
    public uint16 Xsnhash; // .typchk section number
    public uint8 Xsmtyp;  // Symbol alignment and type
    public uint8 Xsmclas;  // Storage-mapping class
    public uint32 Xscnlenhi; // Upper 4 bytes of length or symbol table index
    public uint8 Xpad;  // Unused
    public uint8 Xauxtype;  // Type of auxiliary entry
}

// Auxiliary type
internal static readonly UntypedInt _AUX_EXCEPT = 255;

internal static readonly UntypedInt _AUX_FCN = 254;

internal static readonly UntypedInt _AUX_SYM = 253;

internal static readonly UntypedInt _AUX_FILE = 252;

internal static readonly UntypedInt _AUX_CSECT = 251;

internal static readonly UntypedInt _AUX_SECT = 250;

// Symbol type field.
public static readonly UntypedInt XTY_ER = 0; // External reference

public static readonly UntypedInt XTY_SD = 1; // Section definition

public static readonly UntypedInt XTY_LD = 2; // Label definition

public static readonly UntypedInt XTY_CM = 3; // Common csect definition

// Defines for File auxiliary definitions: x_ftype field of x_file
public static readonly UntypedInt XFT_FN = 0; // Source File Name

public static readonly UntypedInt XFT_CT = 1; // Compile Time Stamp

public static readonly UntypedInt XFT_CV = 2; // Compiler Version Number

public static readonly UntypedInt XFT_CD = 128; // Compiler Defined Information

// Storage-mapping class.
public static readonly UntypedInt XMC_PR = 0; // Program code

public static readonly UntypedInt XMC_RO = 1; // Read-only constant

public static readonly UntypedInt XMC_DB = 2; // Debug dictionary table

public static readonly UntypedInt XMC_TC = 3; // TOC entry

public static readonly UntypedInt XMC_UA = 4; // Unclassified

public static readonly UntypedInt XMC_RW = 5; // Read/Write data

public static readonly UntypedInt XMC_GL = 6; // Global linkage

public static readonly UntypedInt XMC_XO = 7; // Extended operation

public static readonly UntypedInt XMC_SV = 8; // 32-bit supervisor call descriptor

public static readonly UntypedInt XMC_BS = 9; // BSS class

public static readonly UntypedInt XMC_DS = 10; // Function descriptor

public static readonly UntypedInt XMC_UC = 11; // Unnamed FORTRAN common

public static readonly UntypedInt XMC_TC0 = 15; // TOC anchor

public static readonly UntypedInt XMC_TD = 16; // Scalar data entry in the TOC

public static readonly UntypedInt XMC_SV64 = 17; // 64-bit supervisor call descriptor

public static readonly UntypedInt XMC_SV3264 = 18; // Supervisor call descriptor for both 32-bit and 64-bit

public static readonly UntypedInt XMC_TL = 20; // Read/Write thread-local data

public static readonly UntypedInt XMC_UL = 21; // Read/Write thread-local data (.tbss)

public static readonly UntypedInt XMC_TE = 22; // TOC entry

// Loader Header.
[GoType] partial struct LoaderHeader32 {
    public uint32 Lversion; // Loader section version number
    public uint32 Lnsyms; // Number of symbol table entries
    public uint32 Lnreloc; // Number of relocation table entries
    public uint32 Listlen; // Length of import file ID string table
    public uint32 Lnimpid; // Number of import file IDs
    public uint32 Limpoff; // Offset to start of import file IDs
    public uint32 Lstlen; // Length of string table
    public uint32 Lstoff; // Offset to start of string table
}

[GoType] partial struct LoaderHeader64 {
    public uint32 Lversion; // Loader section version number
    public uint32 Lnsyms; // Number of symbol table entries
    public uint32 Lnreloc; // Number of relocation table entries
    public uint32 Listlen; // Length of import file ID string table
    public uint32 Lnimpid; // Number of import file IDs
    public uint32 Lstlen; // Length of string table
    public uint64 Limpoff; // Offset to start of import file IDs
    public uint64 Lstoff; // Offset to start of string table
    public uint64 Lsymoff; // Offset to start of symbol table
    public uint64 Lrldoff; // Offset to start of relocation entries
}

public static readonly UntypedInt LDHDRSZ_32 = 32;
public static readonly UntypedInt LDHDRSZ_64 = 56;

// Loader Symbol.
[GoType] partial struct LoaderSymbol32 {
    public array<byte> Lname = new(8); // Symbol name or byte offset into string table
    public uint32 Lvalue;  // Address field
    public uint16 Lscnum;  // Section number containing symbol
    public uint8 Lsmtype;   // Symbol type, export, import flags
    public uint8 Lsmclas;   // Symbol storage class
    public uint32 Lifile;  // Import file ID; ordinal of import file IDs
    public uint32 Lparm;  // Parameter type-check field
}

[GoType] partial struct LoaderSymbol64 {
    public uint64 Lvalue; // Address field
    public uint32 Loffset; // Byte offset into string table of symbol name
    public uint16 Lscnum; // Section number containing symbol
    public uint8 Lsmtype;  // Symbol type, export, import flags
    public uint8 Lsmclas;  // Symbol storage class
    public uint32 Lifile; // Import file ID; ordinal of import file IDs
    public uint32 Lparm; // Parameter type-check field
}

[GoType] partial struct Reloc32 {
    public uint32 Rvaddr; // (virtual) address of reference
    public uint32 Rsymndx; // Index into symbol table
    public uint8 Rsize;  // Sign and reloc bit len
    public uint8 Rtype;  // Toc relocation type
}

[GoType] partial struct Reloc64 {
    public uint64 Rvaddr; // (virtual) address of reference
    public uint32 Rsymndx; // Index into symbol table
    public uint8 Rsize;  // Sign and reloc bit len
    public uint8 Rtype;  // Toc relocation type
}

public static readonly UntypedInt R_POS = /* 0x00 */ 0; // A(sym) Positive Relocation
public static readonly UntypedInt R_NEG = /* 0x01 */ 1; // -A(sym) Negative Relocation
public static readonly UntypedInt R_REL = /* 0x02 */ 2; // A(sym-*) Relative to self
public static readonly UntypedInt R_TOC = /* 0x03 */ 3; // A(sym-TOC) Relative to TOC
public static readonly UntypedInt R_TRL = /* 0x12 */ 18; // A(sym-TOC) TOC Relative indirect load.
public static readonly UntypedInt R_TRLA = /* 0x13 */ 19; // A(sym-TOC) TOC Rel load address. modifiable inst
public static readonly UntypedInt R_GL = /* 0x05 */ 5; // A(external TOC of sym) Global Linkage
public static readonly UntypedInt R_TCL = /* 0x06 */ 6; // A(local TOC of sym) Local object TOC address
public static readonly UntypedInt R_RL = /* 0x0C */ 12; // A(sym) Pos indirect load. modifiable instruction
public static readonly UntypedInt R_RLA = /* 0x0D */ 13; // A(sym) Pos Load Address. modifiable instruction
public static readonly UntypedInt R_REF = /* 0x0F */ 15; // AL0(sym) Non relocating ref. No garbage collect
public static readonly UntypedInt R_BA = /* 0x08 */ 8; // A(sym) Branch absolute. Cannot modify instruction
public static readonly UntypedInt R_RBA = /* 0x18 */ 24; // A(sym) Branch absolute. modifiable instruction
public static readonly UntypedInt R_BR = /* 0x0A */ 10; // A(sym-*) Branch rel to self. non modifiable
public static readonly UntypedInt R_RBR = /* 0x1A */ 26; // A(sym-*) Branch rel to self. modifiable instr
public static readonly UntypedInt R_TLS = /* 0x20 */ 32; // General-dynamic reference to TLS symbol
public static readonly UntypedInt R_TLS_IE = /* 0x21 */ 33; // Initial-exec reference to TLS symbol
public static readonly UntypedInt R_TLS_LD = /* 0x22 */ 34; // Local-dynamic reference to TLS symbol
public static readonly UntypedInt R_TLS_LE = /* 0x23 */ 35; // Local-exec reference to TLS symbol
public static readonly UntypedInt R_TLSM = /* 0x24 */ 36; // Module reference to TLS symbol
public static readonly UntypedInt R_TLSML = /* 0x25 */ 37; // Module reference to local (own) module
public static readonly UntypedInt R_TOCU = /* 0x30 */ 48; // Relative to TOC - high order bits
public static readonly UntypedInt R_TOCL = /* 0x31 */ 49; // Relative to TOC - low order bits

} // end xcoff_package
