// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package xcoff -- go2cs converted at 2022 March 06 22:41:04 UTC
// import "internal/xcoff" ==> using xcoff = go.@internal.xcoff_package
// Original source: C:\Program Files\Go\src\internal\xcoff\xcoff.go


namespace go.@internal;

public static partial class xcoff_package {

    // File Header.
public partial struct FileHeader32 {
    public ushort Fmagic; // Target machine
    public ushort Fnscns; // Number of sections
    public int Ftimedat; // Time and date of file creation
    public uint Fsymptr; // Byte offset to symbol table start
    public int Fnsyms; // Number of entries in symbol table
    public ushort Fopthdr; // Number of bytes in optional header
    public ushort Fflags; // Flags
}

public partial struct FileHeader64 {
    public ushort Fmagic; // Target machine
    public ushort Fnscns; // Number of sections
    public int Ftimedat; // Time and date of file creation
    public ulong Fsymptr; // Byte offset to symbol table start
    public ushort Fopthdr; // Number of bytes in optional header
    public ushort Fflags; // Flags
    public int Fnsyms; // Number of entries in symbol table
}

public static readonly nint FILHSZ_32 = 20;
public static readonly nint FILHSZ_64 = 24;

public static readonly nint U802TOCMAGIC = 0737; // AIX 32-bit XCOFF
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


// Section Header.
public partial struct SectionHeader32 {
    public array<byte> Sname; // Section name
    public uint Spaddr; // Physical address
    public uint Svaddr; // Virtual address
    public uint Ssize; // Section size
    public uint Sscnptr; // Offset in file to raw data for section
    public uint Srelptr; // Offset in file to relocation entries for section
    public uint Slnnoptr; // Offset in file to line number entries for section
    public ushort Snreloc; // Number of relocation entries
    public ushort Snlnno; // Number of line number entries
    public uint Sflags; // Flags to define the section type
}

public partial struct SectionHeader64 {
    public array<byte> Sname; // Section name
    public ulong Spaddr; // Physical address
    public ulong Svaddr; // Virtual address
    public ulong Ssize; // Section size
    public ulong Sscnptr; // Offset in file to raw data for section
    public ulong Srelptr; // Offset in file to relocation entries for section
    public ulong Slnnoptr; // Offset in file to line number entries for section
    public uint Snreloc; // Number of relocation entries
    public uint Snlnno; // Number of line number entries
    public uint Sflags; // Flags to define the section type
    public uint Spad; // Needs to be 72 bytes long
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

// Symbol Table Entry.
public partial struct SymEnt32 {
    public array<byte> Nname; // Symbol name
    public uint Nvalue; // Symbol value
    public short Nscnum; // Section number of symbol
    public ushort Ntype; // Basic and derived type specification
    public sbyte Nsclass; // Storage class of symbol
    public sbyte Nnumaux; // Number of auxiliary entries
}

public partial struct SymEnt64 {
    public ulong Nvalue; // Symbol value
    public uint Noffset; // Offset of the name in string table or .debug section
    public short Nscnum; // Section number of symbol
    public ushort Ntype; // Basic and derived type specification
    public sbyte Nsclass; // Storage class of symbol
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
public partial struct AuxFile64 {
    public array<byte> Xfname; // Name or offset inside string table
    public byte Xftype; // Source file string type
    public byte Xauxtype; // Type of auxiliary entry
}

// Function Auxiliary Entry
public partial struct AuxFcn32 {
    public uint Xexptr; // File offset to exception table entry
    public uint Xfsize; // Size of function in bytes
    public uint Xlnnoptr; // File pointer to line number
    public uint Xendndx; // Symbol table index of next entry
    public ushort Xpad; // Unused
}
public partial struct AuxFcn64 {
    public ulong Xlnnoptr; // File pointer to line number
    public uint Xfsize; // Size of function in bytes
    public uint Xendndx; // Symbol table index of next entry
    public byte Xpad; // Unused
    public byte Xauxtype; // Type of auxiliary entry
}

public partial struct AuxSect64 {
    public ulong Xscnlen; // section length
    public ulong Xnreloc; // Num RLDs
    public byte pad;
    public byte Xauxtype; // Type of auxiliary entry
}

// csect Auxiliary Entry.
public partial struct AuxCSect32 {
    public int Xscnlen; // Length or symbol table index
    public uint Xparmhash; // Offset of parameter type-check string
    public ushort Xsnhash; // .typchk section number
    public byte Xsmtyp; // Symbol alignment and type
    public byte Xsmclas; // Storage-mapping class
    public uint Xstab; // Reserved
    public ushort Xsnstab; // Reserved
}

public partial struct AuxCSect64 {
    public uint Xscnlenlo; // Lower 4 bytes of length or symbol table index
    public uint Xparmhash; // Offset of parameter type-check string
    public ushort Xsnhash; // .typchk section number
    public byte Xsmtyp; // Symbol alignment and type
    public byte Xsmclas; // Storage-mapping class
    public int Xscnlenhi; // Upper 4 bytes of length or symbol table index
    public byte Xpad; // Unused
    public byte Xauxtype; // Type of auxiliary entry
}

// Auxiliary type
private static readonly nint _AUX_EXCEPT = 255;
private static readonly nint _AUX_FCN = 254;
private static readonly nint _AUX_SYM = 253;
private static readonly nint _AUX_FILE = 252;
private static readonly nint _AUX_CSECT = 251;
private static readonly nint _AUX_SECT = 250;


// Symbol type field.
public static readonly nint XTY_ER = 0; // External reference
public static readonly nint XTY_SD = 1; // Section definition
public static readonly nint XTY_LD = 2; // Label definition
public static readonly nint XTY_CM = 3; // Common csect definition

// Defines for File auxiliary definitions: x_ftype field of x_file
public static readonly nint XFT_FN = 0; // Source File Name
public static readonly nint XFT_CT = 1; // Compile Time Stamp
public static readonly nint XFT_CV = 2; // Compiler Version Number
public static readonly nint XFT_CD = 128; // Compiler Defined Information

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

// Loader Header.
public partial struct LoaderHeader32 {
    public int Lversion; // Loader section version number
    public int Lnsyms; // Number of symbol table entries
    public int Lnreloc; // Number of relocation table entries
    public uint Listlen; // Length of import file ID string table
    public int Lnimpid; // Number of import file IDs
    public uint Limpoff; // Offset to start of import file IDs
    public uint Lstlen; // Length of string table
    public uint Lstoff; // Offset to start of string table
}

public partial struct LoaderHeader64 {
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

public static readonly nint LDHDRSZ_32 = 32;
public static readonly nint LDHDRSZ_64 = 56;


// Loader Symbol.
public partial struct LoaderSymbol32 {
    public array<byte> Lname; // Symbol name or byte offset into string table
    public uint Lvalue; // Address field
    public short Lscnum; // Section number containing symbol
    public sbyte Lsmtype; // Symbol type, export, import flags
    public sbyte Lsmclas; // Symbol storage class
    public int Lifile; // Import file ID; ordinal of import file IDs
    public uint Lparm; // Parameter type-check field
}

public partial struct LoaderSymbol64 {
    public ulong Lvalue; // Address field
    public uint Loffset; // Byte offset into string table of symbol name
    public short Lscnum; // Section number containing symbol
    public sbyte Lsmtype; // Symbol type, export, import flags
    public sbyte Lsmclas; // Symbol storage class
    public int Lifile; // Import file ID; ordinal of import file IDs
    public uint Lparm; // Parameter type-check field
}

public partial struct Reloc32 {
    public uint Rvaddr; // (virtual) address of reference
    public uint Rsymndx; // Index into symbol table
    public byte Rsize; // Sign and reloc bit len
    public byte Rtype; // Toc relocation type
}

public partial struct Reloc64 {
    public ulong Rvaddr; // (virtual) address of reference
    public uint Rsymndx; // Index into symbol table
    public byte Rsize; // Sign and reloc bit len
    public byte Rtype; // Toc relocation type
}

public static readonly nuint R_POS = 0x00; // A(sym) Positive Relocation
public static readonly nuint R_NEG = 0x01; // -A(sym) Negative Relocation
public static readonly nuint R_REL = 0x02; // A(sym-*) Relative to self
public static readonly nuint R_TOC = 0x03; // A(sym-TOC) Relative to TOC
public static readonly nuint R_TRL = 0x12; // A(sym-TOC) TOC Relative indirect load.

public static readonly nuint R_TRLA = 0x13; // A(sym-TOC) TOC Rel load address. modifiable inst
public static readonly nuint R_GL = 0x05; // A(external TOC of sym) Global Linkage
public static readonly nuint R_TCL = 0x06; // A(local TOC of sym) Local object TOC address
public static readonly nuint R_RL = 0x0C; // A(sym) Pos indirect load. modifiable instruction
public static readonly nuint R_RLA = 0x0D; // A(sym) Pos Load Address. modifiable instruction
public static readonly nuint R_REF = 0x0F; // AL0(sym) Non relocating ref. No garbage collect
public static readonly nuint R_BA = 0x08; // A(sym) Branch absolute. Cannot modify instruction
public static readonly nuint R_RBA = 0x18; // A(sym) Branch absolute. modifiable instruction
public static readonly nuint R_BR = 0x0A; // A(sym-*) Branch rel to self. non modifiable
public static readonly nuint R_RBR = 0x1A; // A(sym-*) Branch rel to self. modifiable instr

public static readonly nuint R_TLS = 0x20; // General-dynamic reference to TLS symbol
public static readonly nuint R_TLS_IE = 0x21; // Initial-exec reference to TLS symbol
public static readonly nuint R_TLS_LD = 0x22; // Local-dynamic reference to TLS symbol
public static readonly nuint R_TLS_LE = 0x23; // Local-exec reference to TLS symbol
public static readonly nuint R_TLSM = 0x24; // Module reference to TLS symbol
public static readonly nuint R_TLSML = 0x25; // Module reference to local (own) module

public static readonly nuint R_TOCU = 0x30; // Relative to TOC - high order bits
public static readonly nuint R_TOCL = 0x31; // Relative to TOC - low order bits

} // end xcoff_package
