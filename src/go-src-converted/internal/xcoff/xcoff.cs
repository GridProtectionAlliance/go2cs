// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package xcoff -- go2cs converted at 2020 October 09 05:18:43 UTC
// import "internal/xcoff" ==> using xcoff = go.@internal.xcoff_package
// Original source: C:\Go\src\internal\xcoff\xcoff.go

using static go.builtin;

namespace go {
namespace @internal
{
    public static partial class xcoff_package
    {
        // File Header.
        public partial struct FileHeader32
        {
            public ushort Fmagic; // Target machine
            public ushort Fnscns; // Number of sections
            public int Ftimedat; // Time and date of file creation
            public uint Fsymptr; // Byte offset to symbol table start
            public int Fnsyms; // Number of entries in symbol table
            public ushort Fopthdr; // Number of bytes in optional header
            public ushort Fflags; // Flags
        }

        public partial struct FileHeader64
        {
            public ushort Fmagic; // Target machine
            public ushort Fnscns; // Number of sections
            public int Ftimedat; // Time and date of file creation
            public ulong Fsymptr; // Byte offset to symbol table start
            public ushort Fopthdr; // Number of bytes in optional header
            public ushort Fflags; // Flags
            public int Fnsyms; // Number of entries in symbol table
        }

        public static readonly long FILHSZ_32 = (long)20L;
        public static readonly long FILHSZ_64 = (long)24L;

        public static readonly long U802TOCMAGIC = (long)0737L; // AIX 32-bit XCOFF
        public static readonly long U64_TOCMAGIC = (long)0767L; // AIX 64-bit XCOFF

        // Flags that describe the type of the object file.
        public static readonly ulong F_RELFLG = (ulong)0x0001UL;
        public static readonly ulong F_EXEC = (ulong)0x0002UL;
        public static readonly ulong F_LNNO = (ulong)0x0004UL;
        public static readonly ulong F_FDPR_PROF = (ulong)0x0010UL;
        public static readonly ulong F_FDPR_OPTI = (ulong)0x0020UL;
        public static readonly ulong F_DSA = (ulong)0x0040UL;
        public static readonly ulong F_VARPG = (ulong)0x0100UL;
        public static readonly ulong F_DYNLOAD = (ulong)0x1000UL;
        public static readonly ulong F_SHROBJ = (ulong)0x2000UL;
        public static readonly ulong F_LOADONLY = (ulong)0x4000UL;


        // Section Header.
        public partial struct SectionHeader32
        {
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

        public partial struct SectionHeader64
        {
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
        public static readonly ulong STYP_DWARF = (ulong)0x0010UL;
        public static readonly ulong STYP_TEXT = (ulong)0x0020UL;
        public static readonly ulong STYP_DATA = (ulong)0x0040UL;
        public static readonly ulong STYP_BSS = (ulong)0x0080UL;
        public static readonly ulong STYP_EXCEPT = (ulong)0x0100UL;
        public static readonly ulong STYP_INFO = (ulong)0x0200UL;
        public static readonly ulong STYP_TDATA = (ulong)0x0400UL;
        public static readonly ulong STYP_TBSS = (ulong)0x0800UL;
        public static readonly ulong STYP_LOADER = (ulong)0x1000UL;
        public static readonly ulong STYP_DEBUG = (ulong)0x2000UL;
        public static readonly ulong STYP_TYPCHK = (ulong)0x4000UL;
        public static readonly ulong STYP_OVRFLO = (ulong)0x8000UL;

        public static readonly ulong SSUBTYP_DWINFO = (ulong)0x10000UL; // DWARF info section
        public static readonly ulong SSUBTYP_DWLINE = (ulong)0x20000UL; // DWARF line-number section
        public static readonly ulong SSUBTYP_DWPBNMS = (ulong)0x30000UL; // DWARF public names section
        public static readonly ulong SSUBTYP_DWPBTYP = (ulong)0x40000UL; // DWARF public types section
        public static readonly ulong SSUBTYP_DWARNGE = (ulong)0x50000UL; // DWARF aranges section
        public static readonly ulong SSUBTYP_DWABREV = (ulong)0x60000UL; // DWARF abbreviation section
        public static readonly ulong SSUBTYP_DWSTR = (ulong)0x70000UL; // DWARF strings section
        public static readonly ulong SSUBTYP_DWRNGES = (ulong)0x80000UL; // DWARF ranges section
        public static readonly ulong SSUBTYP_DWLOC = (ulong)0x90000UL; // DWARF location lists section
        public static readonly ulong SSUBTYP_DWFRAME = (ulong)0xA0000UL; // DWARF frames section
        public static readonly ulong SSUBTYP_DWMAC = (ulong)0xB0000UL; // DWARF macros section

        // Symbol Table Entry.
        public partial struct SymEnt32
        {
            public array<byte> Nname; // Symbol name
            public uint Nvalue; // Symbol value
            public short Nscnum; // Section number of symbol
            public ushort Ntype; // Basic and derived type specification
            public sbyte Nsclass; // Storage class of symbol
            public sbyte Nnumaux; // Number of auxiliary entries
        }

        public partial struct SymEnt64
        {
            public ulong Nvalue; // Symbol value
            public uint Noffset; // Offset of the name in string table or .debug section
            public short Nscnum; // Section number of symbol
            public ushort Ntype; // Basic and derived type specification
            public sbyte Nsclass; // Storage class of symbol
            public sbyte Nnumaux; // Number of auxiliary entries
        }

        public static readonly long SYMESZ = (long)18L;



 
        // Nscnum
        public static readonly long N_DEBUG = (long)-2L;
        public static readonly long N_ABS = (long)-1L;
        public static readonly long N_UNDEF = (long)0L; 

        //Ntype
        public static readonly ulong SYM_V_INTERNAL = (ulong)0x1000UL;
        public static readonly ulong SYM_V_HIDDEN = (ulong)0x2000UL;
        public static readonly ulong SYM_V_PROTECTED = (ulong)0x3000UL;
        public static readonly ulong SYM_V_EXPORTED = (ulong)0x4000UL;
        public static readonly ulong SYM_TYPE_FUNC = (ulong)0x0020UL; // is function

        // Storage Class.
        public static readonly long C_NULL = (long)0L; // Symbol table entry marked for deletion
        public static readonly long C_EXT = (long)2L; // External symbol
        public static readonly long C_STAT = (long)3L; // Static symbol
        public static readonly long C_BLOCK = (long)100L; // Beginning or end of inner block
        public static readonly long C_FCN = (long)101L; // Beginning or end of function
        public static readonly long C_FILE = (long)103L; // Source file name and compiler information
        public static readonly long C_HIDEXT = (long)107L; // Unnamed external symbol
        public static readonly long C_BINCL = (long)108L; // Beginning of include file
        public static readonly long C_EINCL = (long)109L; // End of include file
        public static readonly long C_WEAKEXT = (long)111L; // Weak external symbol
        public static readonly long C_DWARF = (long)112L; // DWARF symbol
        public static readonly long C_GSYM = (long)128L; // Global variable
        public static readonly long C_LSYM = (long)129L; // Automatic variable allocated on stack
        public static readonly long C_PSYM = (long)130L; // Argument to subroutine allocated on stack
        public static readonly long C_RSYM = (long)131L; // Register variable
        public static readonly long C_RPSYM = (long)132L; // Argument to function or procedure stored in register
        public static readonly long C_STSYM = (long)133L; // Statically allocated symbol
        public static readonly long C_BCOMM = (long)135L; // Beginning of common block
        public static readonly long C_ECOML = (long)136L; // Local member of common block
        public static readonly long C_ECOMM = (long)137L; // End of common block
        public static readonly long C_DECL = (long)140L; // Declaration of object
        public static readonly long C_ENTRY = (long)141L; // Alternate entry
        public static readonly long C_FUN = (long)142L; // Function or procedure
        public static readonly long C_BSTAT = (long)143L; // Beginning of static block
        public static readonly long C_ESTAT = (long)144L; // End of static block
        public static readonly long C_GTLS = (long)145L; // Global thread-local variable
        public static readonly long C_STTLS = (long)146L; // Static thread-local variable

        // File Auxiliary Entry
        public partial struct AuxFile64
        {
            public array<byte> Xfname; // Name or offset inside string table
            public byte Xftype; // Source file string type
            public byte Xauxtype; // Type of auxiliary entry
        }

        // Function Auxiliary Entry
        public partial struct AuxFcn32
        {
            public uint Xexptr; // File offset to exception table entry
            public uint Xfsize; // Size of function in bytes
            public uint Xlnnoptr; // File pointer to line number
            public uint Xendndx; // Symbol table index of next entry
            public ushort Xpad; // Unused
        }
        public partial struct AuxFcn64
        {
            public ulong Xlnnoptr; // File pointer to line number
            public uint Xfsize; // Size of function in bytes
            public uint Xendndx; // Symbol table index of next entry
            public byte Xpad; // Unused
            public byte Xauxtype; // Type of auxiliary entry
        }

        public partial struct AuxSect64
        {
            public ulong Xscnlen; // section length
            public ulong Xnreloc; // Num RLDs
            public byte pad;
            public byte Xauxtype; // Type of auxiliary entry
        }

        // csect Auxiliary Entry.
        public partial struct AuxCSect32
        {
            public int Xscnlen; // Length or symbol table index
            public uint Xparmhash; // Offset of parameter type-check string
            public ushort Xsnhash; // .typchk section number
            public byte Xsmtyp; // Symbol alignment and type
            public byte Xsmclas; // Storage-mapping class
            public uint Xstab; // Reserved
            public ushort Xsnstab; // Reserved
        }

        public partial struct AuxCSect64
        {
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
        private static readonly long _AUX_EXCEPT = (long)255L;
        private static readonly long _AUX_FCN = (long)254L;
        private static readonly long _AUX_SYM = (long)253L;
        private static readonly long _AUX_FILE = (long)252L;
        private static readonly long _AUX_CSECT = (long)251L;
        private static readonly long _AUX_SECT = (long)250L;


        // Symbol type field.
        public static readonly long XTY_ER = (long)0L; // External reference
        public static readonly long XTY_SD = (long)1L; // Section definition
        public static readonly long XTY_LD = (long)2L; // Label definition
        public static readonly long XTY_CM = (long)3L; // Common csect definition

        // Defines for File auxiliary definitions: x_ftype field of x_file
        public static readonly long XFT_FN = (long)0L; // Source File Name
        public static readonly long XFT_CT = (long)1L; // Compile Time Stamp
        public static readonly long XFT_CV = (long)2L; // Compiler Version Number
        public static readonly long XFT_CD = (long)128L; // Compiler Defined Information

        // Storage-mapping class.
        public static readonly long XMC_PR = (long)0L; // Program code
        public static readonly long XMC_RO = (long)1L; // Read-only constant
        public static readonly long XMC_DB = (long)2L; // Debug dictionary table
        public static readonly long XMC_TC = (long)3L; // TOC entry
        public static readonly long XMC_UA = (long)4L; // Unclassified
        public static readonly long XMC_RW = (long)5L; // Read/Write data
        public static readonly long XMC_GL = (long)6L; // Global linkage
        public static readonly long XMC_XO = (long)7L; // Extended operation
        public static readonly long XMC_SV = (long)8L; // 32-bit supervisor call descriptor
        public static readonly long XMC_BS = (long)9L; // BSS class
        public static readonly long XMC_DS = (long)10L; // Function descriptor
        public static readonly long XMC_UC = (long)11L; // Unnamed FORTRAN common
        public static readonly long XMC_TC0 = (long)15L; // TOC anchor
        public static readonly long XMC_TD = (long)16L; // Scalar data entry in the TOC
        public static readonly long XMC_SV64 = (long)17L; // 64-bit supervisor call descriptor
        public static readonly long XMC_SV3264 = (long)18L; // Supervisor call descriptor for both 32-bit and 64-bit
        public static readonly long XMC_TL = (long)20L; // Read/Write thread-local data
        public static readonly long XMC_UL = (long)21L; // Read/Write thread-local data (.tbss)
        public static readonly long XMC_TE = (long)22L; // TOC entry

        // Loader Header.
        public partial struct LoaderHeader32
        {
            public int Lversion; // Loader section version number
            public int Lnsyms; // Number of symbol table entries
            public int Lnreloc; // Number of relocation table entries
            public uint Listlen; // Length of import file ID string table
            public int Lnimpid; // Number of import file IDs
            public uint Limpoff; // Offset to start of import file IDs
            public uint Lstlen; // Length of string table
            public uint Lstoff; // Offset to start of string table
        }

        public partial struct LoaderHeader64
        {
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

        public static readonly long LDHDRSZ_32 = (long)32L;
        public static readonly long LDHDRSZ_64 = (long)56L;


        // Loader Symbol.
        public partial struct LoaderSymbol32
        {
            public array<byte> Lname; // Symbol name or byte offset into string table
            public uint Lvalue; // Address field
            public short Lscnum; // Section number containing symbol
            public sbyte Lsmtype; // Symbol type, export, import flags
            public sbyte Lsmclas; // Symbol storage class
            public int Lifile; // Import file ID; ordinal of import file IDs
            public uint Lparm; // Parameter type-check field
        }

        public partial struct LoaderSymbol64
        {
            public ulong Lvalue; // Address field
            public uint Loffset; // Byte offset into string table of symbol name
            public short Lscnum; // Section number containing symbol
            public sbyte Lsmtype; // Symbol type, export, import flags
            public sbyte Lsmclas; // Symbol storage class
            public int Lifile; // Import file ID; ordinal of import file IDs
            public uint Lparm; // Parameter type-check field
        }

        public partial struct Reloc32
        {
            public uint Rvaddr; // (virtual) address of reference
            public uint Rsymndx; // Index into symbol table
            public byte Rsize; // Sign and reloc bit len
            public byte Rtype; // Toc relocation type
        }

        public partial struct Reloc64
        {
            public ulong Rvaddr; // (virtual) address of reference
            public uint Rsymndx; // Index into symbol table
            public byte Rsize; // Sign and reloc bit len
            public byte Rtype; // Toc relocation type
        }

        public static readonly ulong R_POS = (ulong)0x00UL; // A(sym) Positive Relocation
        public static readonly ulong R_NEG = (ulong)0x01UL; // -A(sym) Negative Relocation
        public static readonly ulong R_REL = (ulong)0x02UL; // A(sym-*) Relative to self
        public static readonly ulong R_TOC = (ulong)0x03UL; // A(sym-TOC) Relative to TOC
        public static readonly ulong R_TRL = (ulong)0x12UL; // A(sym-TOC) TOC Relative indirect load.

        public static readonly ulong R_TRLA = (ulong)0x13UL; // A(sym-TOC) TOC Rel load address. modifiable inst
        public static readonly ulong R_GL = (ulong)0x05UL; // A(external TOC of sym) Global Linkage
        public static readonly ulong R_TCL = (ulong)0x06UL; // A(local TOC of sym) Local object TOC address
        public static readonly ulong R_RL = (ulong)0x0CUL; // A(sym) Pos indirect load. modifiable instruction
        public static readonly ulong R_RLA = (ulong)0x0DUL; // A(sym) Pos Load Address. modifiable instruction
        public static readonly ulong R_REF = (ulong)0x0FUL; // AL0(sym) Non relocating ref. No garbage collect
        public static readonly ulong R_BA = (ulong)0x08UL; // A(sym) Branch absolute. Cannot modify instruction
        public static readonly ulong R_RBA = (ulong)0x18UL; // A(sym) Branch absolute. modifiable instruction
        public static readonly ulong R_BR = (ulong)0x0AUL; // A(sym-*) Branch rel to self. non modifiable
        public static readonly ulong R_RBR = (ulong)0x1AUL; // A(sym-*) Branch rel to self. modifiable instr

        public static readonly ulong R_TLS = (ulong)0x20UL; // General-dynamic reference to TLS symbol
        public static readonly ulong R_TLS_IE = (ulong)0x21UL; // Initial-exec reference to TLS symbol
        public static readonly ulong R_TLS_LD = (ulong)0x22UL; // Local-dynamic reference to TLS symbol
        public static readonly ulong R_TLS_LE = (ulong)0x23UL; // Local-exec reference to TLS symbol
        public static readonly ulong R_TLSM = (ulong)0x24UL; // Module reference to TLS symbol
        public static readonly ulong R_TLSML = (ulong)0x25UL; // Module reference to local (own) module

        public static readonly ulong R_TOCU = (ulong)0x30UL; // Relative to TOC - high order bits
        public static readonly ulong R_TOCL = (ulong)0x31UL; // Relative to TOC - low order bits
    }
}}
