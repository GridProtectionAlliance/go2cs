// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ld -- go2cs converted at 2020 October 09 05:52:42 UTC
// import "cmd/oldlink/internal/ld" ==> using ld = go.cmd.oldlink.@internal.ld_package
// Original source: C:\Go\src\cmd\oldlink\internal\ld\xcoff.go
using bytes = go.bytes_package;
using objabi = go.cmd.@internal.objabi_package;
using sym = go.cmd.oldlink.@internal.sym_package;
using binary = go.encoding.binary_package;
using ioutil = go.io.ioutil_package;
using bits = go.math.bits_package;
using filepath = go.path.filepath_package;
using sort = go.sort_package;
using strings = go.strings_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace oldlink {
namespace @internal
{
    public static partial class ld_package
    {
        // This file handles all algorithms related to XCOFF files generation.
        // Most of them are adaptations of the ones in  cmd/oldlink/internal/pe.go
        // as PE and XCOFF are based on COFF files.
        // XCOFF files generated are 64 bits.
 
        // Total amount of space to reserve at the start of the file
        // for File Header, Auxiliary Header, and Section Headers.
        // May waste some.
        public static readonly var XCOFFHDRRESERVE = FILHSZ_64 + AOUTHSZ_EXEC64 + SCNHSZ_64 * 23L;
        public static readonly long XCOFFSECTALIGN = 32L; // base on dump -o

        // XCOFF binaries should normally have all its sections position-independent.
        // However, this is not yet possible for .text because of some R_ADDR relocations
        // inside RODATA symbols.
        // .data and .bss are position-independent so their address start inside a unreachable
        // segment during execution to force segfault if something is wrong.
        public static readonly ulong XCOFFTEXTBASE = (ulong)0x100000000UL; // Start of text address
        public static readonly ulong XCOFFDATABASE = (ulong)0x200000000UL; // Start of data address

        // File Header
        public partial struct XcoffFileHdr64
        {
            public ushort Fmagic; // Target machine
            public ushort Fnscns; // Number of sections
            public int Ftimedat; // Time and date of file creation
            public ulong Fsymptr; // Byte offset to symbol table start
            public ushort Fopthdr; // Number of bytes in optional header
            public ushort Fflags; // Flags
            public int Fnsyms; // Number of entries in symbol table
        }

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


        // Auxiliary Header
        public partial struct XcoffAoutHdr64
        {
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
        public partial struct XcoffScnHdr64
        {
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

        // Headers size
        public static readonly long FILHSZ_32 = (long)20L;
        public static readonly long FILHSZ_64 = (long)24L;
        public static readonly long AOUTHSZ_EXEC32 = (long)72L;
        public static readonly long AOUTHSZ_EXEC64 = (long)120L;
        public static readonly long SCNHSZ_32 = (long)40L;
        public static readonly long SCNHSZ_64 = (long)72L;
        public static readonly long LDHDRSZ_32 = (long)32L;
        public static readonly long LDHDRSZ_64 = (long)56L;
        public static readonly long LDSYMSZ_64 = (long)24L;
        public static readonly long RELSZ_64 = (long)14L;


        // Type representing all XCOFF symbols.
        private partial interface xcoffSym
        {
        }

        // Symbol Table Entry
        public partial struct XcoffSymEnt64
        {
            public ulong Nvalue; // Symbol value
            public uint Noffset; // Offset of the name in string table or .debug section
            public short Nscnum; // Section number of symbol
            public ushort Ntype; // Basic and derived type specification
            public byte Nsclass; // Storage class of symbol
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
        public partial struct XcoffAuxFile64
        {
            public uint Xzeroes; // The name is always in the string table
            public uint Xoffset; // Offset in the string table
            public array<byte> X_pad1;
            public byte Xftype; // Source file string type
            public array<byte> X_pad2;
            public byte Xauxtype; // Type of auxiliary entry
        }

        // Function Auxiliary Entry
        public partial struct XcoffAuxFcn64
        {
            public ulong Xlnnoptr; // File pointer to line number
            public uint Xfsize; // Size of function in bytes
            public uint Xendndx; // Symbol table index of next entry
            public byte Xpad; // Unused
            public byte Xauxtype; // Type of auxiliary entry
        }

        // csect Auxiliary Entry.
        public partial struct XcoffAuxCSect64
        {
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
        public partial struct XcoffAuxDWARF64
        {
            public ulong Xscnlen; // Length of this symbol section
            public array<byte> X_pad;
            public byte Xauxtype; // Type of auxiliary entry
        }

        // Auxiliary type
        private static readonly long _AUX_EXCEPT = (long)255L;
        private static readonly long _AUX_FCN = (long)254L;
        private static readonly long _AUX_SYM = (long)253L;
        private static readonly long _AUX_FILE = (long)252L;
        private static readonly long _AUX_CSECT = (long)251L;
        private static readonly long _AUX_SECT = (long)250L;


        // Xftype field
        public static readonly long XFT_FN = (long)0L; // Source File Name
        public static readonly long XFT_CT = (long)1L; // Compile Time Stamp
        public static readonly long XFT_CV = (long)2L; // Compiler Version Number
        public static readonly long XFT_CD = (long)128L; // Compiler Defined Information/


        // Symbol type field.
        public static readonly long XTY_ER = (long)0L; // External reference
        public static readonly long XTY_SD = (long)1L; // Section definition
        public static readonly long XTY_LD = (long)2L; // Label definition
        public static readonly long XTY_CM = (long)3L; // Common csect definition
        public static readonly ulong XTY_WK = (ulong)0x8UL; // Weak symbol
        public static readonly ulong XTY_EXP = (ulong)0x10UL; // Exported symbol
        public static readonly ulong XTY_ENT = (ulong)0x20UL; // Entry point symbol
        public static readonly ulong XTY_IMP = (ulong)0x40UL; // Imported symbol

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

        // Loader Header
        public partial struct XcoffLdHdr64
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

        // Loader Symbol
        public partial struct XcoffLdSym64
        {
            public ulong Lvalue; // Address field
            public uint Loffset; // Byte offset into string table of symbol name
            public short Lscnum; // Section number containing symbol
            public sbyte Lsmtype; // Symbol type, export, import flags
            public sbyte Lsmclas; // Symbol storage class
            public int Lifile; // Import file ID; ordinal of import file IDs
            public uint Lparm; // Parameter type-check field
        }

        private partial struct xcoffLoaderSymbol
        {
            public ptr<sym.Symbol> sym;
            public sbyte smtype;
            public sbyte smclas;
        }

        public partial struct XcoffLdImportFile64
        {
            public @string Limpidpath;
            public @string Limpidbase;
            public @string Limpidmem;
        }

        public partial struct XcoffLdRel64
        {
            public ulong Lvaddr; // Address Field
            public ushort Lrtype; // Relocation Size and Type
            public short Lrsecnm; // Section Number being relocated
            public int Lsymndx; // Loader-Section symbol table index
        }

        // xcoffLoaderReloc holds information about a relocation made by the loader.
        private partial struct xcoffLoaderReloc
        {
            public ptr<sym.Symbol> sym;
            public ptr<sym.Reloc> rel;
            public ushort rtype;
            public int symndx;
        }

        public static readonly ulong XCOFF_R_POS = (ulong)0x00UL; // A(sym) Positive Relocation
        public static readonly ulong XCOFF_R_NEG = (ulong)0x01UL; // -A(sym) Negative Relocation
        public static readonly ulong XCOFF_R_REL = (ulong)0x02UL; // A(sym-*) Relative to self
        public static readonly ulong XCOFF_R_TOC = (ulong)0x03UL; // A(sym-TOC) Relative to TOC
        public static readonly ulong XCOFF_R_TRL = (ulong)0x12UL; // A(sym-TOC) TOC Relative indirect load.

        public static readonly ulong XCOFF_R_TRLA = (ulong)0x13UL; // A(sym-TOC) TOC Rel load address. modifiable inst
        public static readonly ulong XCOFF_R_GL = (ulong)0x05UL; // A(external TOC of sym) Global Linkage
        public static readonly ulong XCOFF_R_TCL = (ulong)0x06UL; // A(local TOC of sym) Local object TOC address
        public static readonly ulong XCOFF_R_RL = (ulong)0x0CUL; // A(sym) Pos indirect load. modifiable instruction
        public static readonly ulong XCOFF_R_RLA = (ulong)0x0DUL; // A(sym) Pos Load Address. modifiable instruction
        public static readonly ulong XCOFF_R_REF = (ulong)0x0FUL; // AL0(sym) Non relocating ref. No garbage collect
        public static readonly ulong XCOFF_R_BA = (ulong)0x08UL; // A(sym) Branch absolute. Cannot modify instruction
        public static readonly ulong XCOFF_R_RBA = (ulong)0x18UL; // A(sym) Branch absolute. modifiable instruction
        public static readonly ulong XCOFF_R_BR = (ulong)0x0AUL; // A(sym-*) Branch rel to self. non modifiable
        public static readonly ulong XCOFF_R_RBR = (ulong)0x1AUL; // A(sym-*) Branch rel to self. modifiable instr

        public static readonly ulong XCOFF_R_TLS = (ulong)0x20UL; // General-dynamic reference to TLS symbol
        public static readonly ulong XCOFF_R_TLS_IE = (ulong)0x21UL; // Initial-exec reference to TLS symbol
        public static readonly ulong XCOFF_R_TLS_LD = (ulong)0x22UL; // Local-dynamic reference to TLS symbol
        public static readonly ulong XCOFF_R_TLS_LE = (ulong)0x23UL; // Local-exec reference to TLS symbol
        public static readonly ulong XCOFF_R_TLSM = (ulong)0x24UL; // Module reference to TLS symbol
        public static readonly ulong XCOFF_R_TLSML = (ulong)0x25UL; // Module reference to local (own) module

        public static readonly ulong XCOFF_R_TOCU = (ulong)0x30UL; // Relative to TOC - high order bits
        public static readonly ulong XCOFF_R_TOCL = (ulong)0x31UL; // Relative to TOC - low order bits

        public partial struct XcoffLdStr64
        {
            public ushort size;
            public @string name;
        }

        // xcoffFile is used to build XCOFF file.
        private partial struct xcoffFile
        {
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
            public map<@string, long> dynLibraries; // Dynamic libraries in .loader section. The integer represents its import file number (- 1)
            public slice<ptr<xcoffLoaderSymbol>> loaderSymbols; // symbols inside .loader symbol table
            public slice<ptr<xcoffLoaderReloc>> loaderReloc; // Reloc that must be made inside loader
        }

        // Var used by XCOFF Generation algorithms
        private static xcoffFile xfile = default;

        // xcoffStringTable is a XCOFF string table.
        private partial struct xcoffStringTable
        {
            public slice<@string> strings;
            public long stringsLen;
        }

        // size returns size of string table t.
        private static long size(this ptr<xcoffStringTable> _addr_t)
        {
            ref xcoffStringTable t = ref _addr_t.val;
 
            // string table starts with 4-byte length at the beginning
            return t.stringsLen + 4L;

        }

        // add adds string str to string table t.
        private static long add(this ptr<xcoffStringTable> _addr_t, @string str)
        {
            ref xcoffStringTable t = ref _addr_t.val;

            var off = t.size();
            t.strings = append(t.strings, str);
            t.stringsLen += len(str) + 1L; // each string will have 0 appended to it
            return off;

        }

        // write writes string table t into the output file.
        private static void write(this ptr<xcoffStringTable> _addr_t, ptr<OutBuf> _addr_@out)
        {
            ref xcoffStringTable t = ref _addr_t.val;
            ref OutBuf @out = ref _addr_@out.val;

            @out.Write32(uint32(t.size()));
            foreach (var (_, s) in t.strings)
            {
                @out.WriteString(s);
                @out.Write8(0L);
            }

        }

        // write writes XCOFF section sect into the output file.
        private static void write(this ptr<XcoffScnHdr64> _addr_sect, ptr<Link> _addr_ctxt)
        {
            ref XcoffScnHdr64 sect = ref _addr_sect.val;
            ref Link ctxt = ref _addr_ctxt.val;

            binary.Write(ctxt.Out, binary.BigEndian, sect);
            ctxt.Out.Write32(0L); // Add 4 empty bytes at the end to match alignment
        }

        // addSection adds section to the XCOFF file f.
        private static ptr<XcoffScnHdr64> addSection(this ptr<xcoffFile> _addr_f, @string name, ulong addr, ulong size, ulong fileoff, uint flags)
        {
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
        private static ptr<XcoffScnHdr64> addDwarfSection(this ptr<xcoffFile> _addr_f, ptr<sym.Section> _addr_s)
        {
            ref xcoffFile f = ref _addr_f.val;
            ref sym.Section s = ref _addr_s.val;

            var (newName, subtype) = xcoffGetDwarfSubtype(s.Name);
            return _addr_f.addSection(newName, 0L, s.Length, s.Seg.Fileoff + s.Vaddr - s.Seg.Vaddr, STYP_DWARF | subtype)!;
        }

        // xcoffGetDwarfSubtype returns the XCOFF name of the DWARF section str
        // and its subtype constant.
        private static (@string, uint) xcoffGetDwarfSubtype(@string str)
        {
            @string _p0 = default;
            uint _p0 = default;

            switch (str)
            {
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
            return ("", 0L);

        }

        // getXCOFFscnum returns the XCOFF section number of a Go section.
        private static short getXCOFFscnum(this ptr<xcoffFile> _addr_f, ptr<sym.Section> _addr_sect)
        {
            ref xcoffFile f = ref _addr_f.val;
            ref sym.Section sect = ref _addr_sect.val;


            if (sect.Seg == _addr_Segtext) 
                return f.sectNameToScnum[".text"];
            else if (sect.Seg == _addr_Segdata) 
                if (sect.Name == ".noptrbss" || sect.Name == ".bss")
                {
                    return f.sectNameToScnum[".bss"];
                }

                if (sect.Name == ".tbss")
                {
                    return f.sectNameToScnum[".tbss"];
                }

                return f.sectNameToScnum[".data"];
            else if (sect.Seg == _addr_Segdwarf) 
                var (name, _) = xcoffGetDwarfSubtype(sect.Name);
                return f.sectNameToScnum[name];
            else if (sect.Seg == _addr_Segrelrodata) 
                return f.sectNameToScnum[".data"];
                        Errorf(null, "getXCOFFscnum not implemented for section %s", sect.Name);
            return -1L;

        }

        // Xcoffinit initialised some internal value and setups
        // already known header information
        public static void Xcoffinit(ptr<Link> _addr_ctxt)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            xfile.dynLibraries = make_map<@string, long>();

            HEADR = int32(Rnd(XCOFFHDRRESERVE, XCOFFSECTALIGN));
            if (FlagTextAddr != -1L.val)
            {
                Errorf(null, "-T not available on AIX");
            }

            FlagTextAddr.val = XCOFFTEXTBASE + int64(HEADR);
            if (FlagRound != -1L.val)
            {
                Errorf(null, "-R not available on AIX");
            }

            FlagRound.val = int(XCOFFSECTALIGN);


        }

        // SYMBOL TABLE

        // type records C_FILE information needed for genasmsym in XCOFF.
        private partial struct xcoffSymSrcFile
        {
            public @string name;
            public ptr<XcoffSymEnt64> file; // Symbol of this C_FILE
            public ptr<XcoffAuxCSect64> csectAux; // Symbol for the current .csect
            public ulong csectSymNb; // Symbol number for the current .csect
            public long csectSize;
        }

        private static var currDwscnoff = make_map<@string, ulong>();        private static xcoffSymSrcFile currSymSrcFile = default;        private static var outerSymSize = make_map<@string, long>();

        // xcoffUpdateOuterSize stores the size of outer symbols in order to have it
        // in the symbol table.
        private static void xcoffUpdateOuterSize(ptr<Link> _addr_ctxt, long size, sym.SymKind stype)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            if (size == 0L)
            {
                return ;
            }


            if (stype == sym.SRODATA || stype == sym.SRODATARELRO || stype == sym.SFUNCTAB || stype == sym.SSTRING)
            {
                goto __switch_break0;
            }
            if (stype == sym.STYPERELRO)
            {
                if (ctxt.UseRelro() && (ctxt.BuildMode == BuildModeCArchive || ctxt.BuildMode == BuildModeCShared || ctxt.BuildMode == BuildModePIE))
                { 
                    // runtime.types size must be removed, as it's a real symbol.
                    outerSymSize["typerel.*"] = size - ctxt.Syms.ROLookup("runtime.types", 0L).Size;
                    return ;

                }

                fallthrough = true;
            }
            if (fallthrough || stype == sym.STYPE)
            {
                if (!ctxt.DynlinkingGo())
                { 
                    // runtime.types size must be removed, as it's a real symbol.
                    outerSymSize["type.*"] = size - ctxt.Syms.ROLookup("runtime.types", 0L).Size;

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
                if (!ctxt.DynlinkingGo())
                {
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
            if (stype == sym.SITABLINK)
            {
                outerSymSize["runtime.itablink"] = size;
                goto __switch_break0;
            }
            // default: 
                Errorf(null, "unknown XCOFF outer symbol for type %s", stype.String());

            __switch_break0:;


        }

        // addSymbol writes a symbol or an auxiliary symbol entry on ctxt.out.
        private static void addSymbol(this ptr<xcoffFile> _addr_f, xcoffSym sym)
        {
            ref xcoffFile f = ref _addr_f.val;

            f.symtabSym = append(f.symtabSym, sym);
            f.symbolCount++;
        }

        // xcoffAlign returns the log base 2 of the symbol's alignment.
        private static byte xcoffAlign(ptr<sym.Symbol> _addr_x, SymbolType t)
        {
            ref sym.Symbol x = ref _addr_x.val;

            var align = x.Align;
            if (align == 0L)
            {
                if (t == TextSym)
                {
                    align = int32(Funcalign);
                }
                else
                {
                    align = symalign(x);
                }

            }

            return logBase2(int(align));

        }

        // logBase2 returns the log in base 2 of a.
        private static byte logBase2(long a)
        {
            return uint8(bits.Len(uint(a)) - 1L);
        }

        // Write symbols needed when a new file appeared:
        // - a C_FILE with one auxiliary entry for its name
        // - C_DWARF symbols to provide debug information
        // - a C_HIDEXT which will be a csect containing all of its functions
        // It needs several parameters to create .csect symbols such as its entry point and its section number.
        //
        // Currently, a new file is in fact a new package. It seems to be OK, but it might change
        // in the future.
        private static void writeSymbolNewFile(this ptr<xcoffFile> _addr_f, ptr<Link> _addr_ctxt, @string name, ulong firstEntry, short extnum)
        {
            ref xcoffFile f = ref _addr_f.val;
            ref Link ctxt = ref _addr_ctxt.val;
 
            /* C_FILE */
            ptr<XcoffSymEnt64> s = addr(new XcoffSymEnt64(Noffset:uint32(f.stringTable.add(".file")),Nsclass:C_FILE,Nscnum:N_DEBUG,Ntype:0,Nnumaux:1,));
            f.addSymbol(s);
            currSymSrcFile.file = s; 

            // Auxiliary entry for file name.
            ptr<XcoffAuxFile64> auxf = addr(new XcoffAuxFile64(Xoffset:uint32(f.stringTable.add(name)),Xftype:XFT_FN,Xauxtype:_AUX_FILE,));
            f.addSymbol(auxf); 

            /* Dwarf */
            foreach (var (_, sect) in Segdwarf.Sections)
            {
                ulong dwsize = default;
                if (ctxt.LinkMode == LinkInternal)
                { 
                    // Find the size of this corresponding package DWARF compilation unit.
                    // This size is set during DWARF generation (see dwarf.go).
                    dwsize = getDwsectCUSize(sect.Name, name); 
                    // .debug_abbrev is common to all packages and not found with the previous function
                    if (sect.Name == ".debug_abbrev")
                    {
                        s = ctxt.Syms.ROLookup(sect.Name, 0L);
                        dwsize = uint64(s.Size);
                    }

                }
                else
                { 
                    // There is only one .FILE with external linking.
                    dwsize = sect.Length;

                } 

                // get XCOFF name
                var (name, _) = xcoffGetDwarfSubtype(sect.Name);
                s = addr(new XcoffSymEnt64(Nvalue:currDwscnoff[sect.Name],Noffset:uint32(f.stringTable.add(name)),Nsclass:C_DWARF,Nscnum:f.getXCOFFscnum(sect),Nnumaux:1,));

                if (currSymSrcFile.csectAux == null)
                { 
                    // Dwarf relocations need the symbol number of .dw* symbols.
                    // It doesn't need to know it for each package, one is enough.
                    // currSymSrcFile.csectAux == nil means first package.
                    var dws = ctxt.Syms.Lookup(sect.Name, 0L);
                    dws.Dynid = int32(f.symbolCount);

                    if (sect.Name == ".debug_frame" && ctxt.LinkMode != LinkExternal)
                    { 
                        // CIE size must be added to the first package.
                        dwsize += 48L;

                    }

                }

                f.addSymbol(s); 

                // update the DWARF section offset in this file
                if (sect.Name != ".debug_abbrev")
                {
                    currDwscnoff[sect.Name] += dwsize;
                } 

                // Auxiliary dwarf section
                ptr<XcoffAuxDWARF64> auxd = addr(new XcoffAuxDWARF64(Xscnlen:dwsize,Xauxtype:_AUX_SECT,));

                f.addSymbol(auxd);

            }

            /* .csect */
            // Check if extnum is in text.
            // This is temporary and only here to check if this algorithm is correct.
            if (extnum != 1L)
            {
                Exitf("XCOFF symtab: A new file was detected with its first symbol not in .text");
            }

            currSymSrcFile.csectSymNb = uint64(f.symbolCount); 

            // No offset because no name
            s = addr(new XcoffSymEnt64(Nvalue:firstEntry,Nscnum:extnum,Nsclass:C_HIDEXT,Ntype:0,Nnumaux:1,));
            f.addSymbol(s);

            ptr<XcoffAuxCSect64> aux = addr(new XcoffAuxCSect64(Xsmclas:XMC_PR,Xsmtyp:XTY_SD|logBase2(Funcalign)<<3,Xauxtype:_AUX_CSECT,));
            f.addSymbol(aux);

            currSymSrcFile.csectAux = aux;
            currSymSrcFile.csectSize = 0L;

        }

        // Update values for the previous package.
        //  - Svalue of the C_FILE symbol: if it is the last one, this Svalue must be -1
        //  - Xsclen of the csect symbol.
        private static void updatePreviousFile(this ptr<xcoffFile> _addr_f, ptr<Link> _addr_ctxt, bool last)
        {
            ref xcoffFile f = ref _addr_f.val;
            ref Link ctxt = ref _addr_ctxt.val;
 
            // first file
            if (currSymSrcFile.file == null)
            {
                return ;
            } 

            // Update C_FILE
            var cfile = currSymSrcFile.file;
            if (last)
            {
                cfile.Nvalue = 0xFFFFFFFFFFFFFFFFUL;
            }
            else
            {
                cfile.Nvalue = uint64(f.symbolCount);
            } 

            // update csect scnlen in this auxiliary entry
            var aux = currSymSrcFile.csectAux;
            aux.Xscnlenlo = uint32(currSymSrcFile.csectSize & 0xFFFFFFFFUL);
            aux.Xscnlenhi = uint32(currSymSrcFile.csectSize >> (int)(32L));

        }

        // Write symbol representing a .text function.
        // The symbol table is split with C_FILE corresponding to each package
        // and not to each source file as it should be.
        private static slice<xcoffSym> writeSymbolFunc(this ptr<xcoffFile> _addr_f, ptr<Link> _addr_ctxt, ptr<sym.Symbol> _addr_x)
        {
            ref xcoffFile f = ref _addr_f.val;
            ref Link ctxt = ref _addr_ctxt.val;
            ref sym.Symbol x = ref _addr_x.val;
 
            // New XCOFF symbols which will be written.
            xcoffSym syms = new slice<xcoffSym>(new xcoffSym[] {  }); 

            // Check if a new file is detected.
            if (strings.Contains(x.Name, "-tramp") || strings.HasPrefix(x.Name, "runtime.text."))
            { 
                // Trampoline don't have a FILE so there are considered
                // in the current file.
                // Same goes for runtime.text.X symbols.
            }
            else if (x.File == "")
            { // Undefined global symbol
                // If this happens, the algorithm must be redone.
                if (currSymSrcFile.name != "")
                {
                    Exitf("undefined global symbol found inside another file");
                }

            }
            else
            { 
                // Current file has changed. New C_FILE, C_DWARF, etc must be generated.
                if (currSymSrcFile.name != x.File)
                {
                    if (ctxt.LinkMode == LinkInternal)
                    { 
                        // update previous file values
                        xfile.updatePreviousFile(ctxt, false);
                        currSymSrcFile.name = x.File;
                        f.writeSymbolNewFile(ctxt, x.File, uint64(x.Value), xfile.getXCOFFscnum(x.Sect));

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
                        if (currSymSrcFile.name == "")
                        {
                            currSymSrcFile.name = x.File;
                            f.writeSymbolNewFile(ctxt, "go_functions", uint64(x.Value), xfile.getXCOFFscnum(x.Sect));
                        }

                    }

                }

            }

            ptr<XcoffSymEnt64> s = addr(new XcoffSymEnt64(Nsclass:C_EXT,Noffset:uint32(xfile.stringTable.add(x.Extname())),Nvalue:uint64(x.Value),Nscnum:f.getXCOFFscnum(x.Sect),Ntype:SYM_TYPE_FUNC,Nnumaux:2,));

            if (x.Version != 0L || x.Attr.VisibilityHidden() || x.Attr.Local())
            {
                s.Nsclass = C_HIDEXT;
            }

            x.Dynid = int32(xfile.symbolCount);
            syms = append(syms, s); 

            // Update current csect size
            currSymSrcFile.csectSize += x.Size; 

            // create auxiliary entries
            ptr<XcoffAuxFcn64> a2 = addr(new XcoffAuxFcn64(Xfsize:uint32(x.Size),Xlnnoptr:0,Xendndx:xfile.symbolCount+3,Xauxtype:_AUX_FCN,));
            syms = append(syms, a2);

            ptr<XcoffAuxCSect64> a4 = addr(new XcoffAuxCSect64(Xscnlenlo:uint32(currSymSrcFile.csectSymNb&0xFFFFFFFF),Xscnlenhi:uint32(currSymSrcFile.csectSymNb>>32),Xsmclas:XMC_PR,Xsmtyp:XTY_LD,Xauxtype:_AUX_CSECT,));
            a4.Xsmtyp |= uint8(xcoffAlign(_addr_x, TextSym) << (int)(3L));

            syms = append(syms, a4);
            return syms;

        }

        // put function used by genasmsym to write symbol table
        private static void putaixsym(ptr<Link> _addr_ctxt, ptr<sym.Symbol> _addr_x, @string str, SymbolType t, long addr, ptr<sym.Symbol> _addr_go_)
        {
            ref Link ctxt = ref _addr_ctxt.val;
            ref sym.Symbol x = ref _addr_x.val;
            ref sym.Symbol go_ = ref _addr_go_.val;

            // All XCOFF symbols generated by this GO symbols
            // Can be a symbol entry or a auxiliary entry
            xcoffSym syms = new slice<xcoffSym>(new xcoffSym[] {  });


            if (t == TextSym) 
                if (x.FuncInfo != null || strings.Contains(x.Name, "-tramp") || strings.HasPrefix(x.Name, "runtime.text."))
                { 
                    // Function within a file
                    syms = xfile.writeSymbolFunc(ctxt, x);

                }
                else
                { 
                    // Only runtime.text and runtime.etext come through this way
                    if (x.Name != "runtime.text" && x.Name != "runtime.etext" && x.Name != "go.buildid")
                    {
                        Exitf("putaixsym: unknown text symbol %s", x.Name);
                    }

                    ptr<XcoffSymEnt64> s = addr(new XcoffSymEnt64(Nsclass:C_HIDEXT,Noffset:uint32(xfile.stringTable.add(str)),Nvalue:uint64(x.Value),Nscnum:xfile.getXCOFFscnum(x.Sect),Ntype:SYM_TYPE_FUNC,Nnumaux:1,));
                    x.Dynid = int32(xfile.symbolCount);
                    syms = append(syms, s);

                    var size = uint64(x.Size);
                    ptr<XcoffAuxCSect64> a4 = addr(new XcoffAuxCSect64(Xauxtype:_AUX_CSECT,Xscnlenlo:uint32(size&0xFFFFFFFF),Xscnlenhi:uint32(size>>32),Xsmclas:XMC_PR,Xsmtyp:XTY_SD,));
                    a4.Xsmtyp |= uint8(xcoffAlign(_addr_x, TextSym) << (int)(3L));
                    syms = append(syms, a4);


                }

            else if (t == DataSym || t == BSSSym) 
                s = addr(new XcoffSymEnt64(Nsclass:C_EXT,Noffset:uint32(xfile.stringTable.add(str)),Nvalue:uint64(x.Value),Nscnum:xfile.getXCOFFscnum(x.Sect),Nnumaux:1,));

                if (x.Version != 0L || x.Attr.VisibilityHidden() || x.Attr.Local())
                { 
                    // There is more symbols in the case of a global data
                    // which are related to the assembly generated
                    // to access such symbols.
                    // But as Golang as its own way to check if a symbol is
                    // global or local (the capital letter), we don't need to
                    // implement them yet.
                    s.Nsclass = C_HIDEXT;

                }

                x.Dynid = int32(xfile.symbolCount);
                syms = append(syms, s); 

                // Create auxiliary entry

                // Normally, size should be the size of csect containing all
                // the data and bss symbols of one file/package.
                // However, it's easier to just have a csect for each symbol.
                // It might change
                size = uint64(x.Size);
                a4 = addr(new XcoffAuxCSect64(Xauxtype:_AUX_CSECT,Xscnlenlo:uint32(size&0xFFFFFFFF),Xscnlenhi:uint32(size>>32),));

                if (x.Type >= sym.STYPE && x.Type <= sym.SPCLNTAB)
                {
                    if (ctxt.LinkMode == LinkExternal && strings.HasPrefix(x.Sect.Name, ".data.rel.ro"))
                    { 
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
                else if (x.Type == sym.SDATA && strings.HasPrefix(x.Name, "TOC.") && ctxt.LinkMode == LinkExternal)
                {
                    a4.Xsmclas = XMC_TC;
                }
                else if (x.Name == "TOC")
                {
                    a4.Xsmclas = XMC_TC0;
                }
                else
                {
                    a4.Xsmclas = XMC_RW;
                }

                if (t == DataSym)
                {
                    a4.Xsmtyp |= XTY_SD;
                }
                else
                {
                    a4.Xsmtyp |= XTY_CM;
                }

                a4.Xsmtyp |= uint8(xcoffAlign(_addr_x, t) << (int)(3L));

                syms = append(syms, a4);
            else if (t == UndefinedSym) 
                if (x.Type != sym.SDYNIMPORT && x.Type != sym.SHOSTOBJ && x.Type != sym.SUNDEFEXT)
                {
                    return ;
                }

                s = addr(new XcoffSymEnt64(Nsclass:C_EXT,Noffset:uint32(xfile.stringTable.add(str)),Nnumaux:1,));
                x.Dynid = int32(xfile.symbolCount);
                syms = append(syms, s);

                a4 = addr(new XcoffAuxCSect64(Xauxtype:_AUX_CSECT,Xsmclas:XMC_DS,Xsmtyp:XTY_ER|XTY_IMP,));

                if (x.Name == "__n_pthreads")
                { 
                    // Currently, all imported symbols made by cgo_import_dynamic are
                    // syscall functions, except __n_pthreads which is a variable.
                    // TODO(aix): Find a way to detect variables imported by cgo.
                    a4.Xsmclas = XMC_RW;

                }

                syms = append(syms, a4);
            else if (t == TLSSym) 
                s = addr(new XcoffSymEnt64(Nsclass:C_EXT,Noffset:uint32(xfile.stringTable.add(str)),Nscnum:xfile.getXCOFFscnum(x.Sect),Nvalue:uint64(x.Value),Nnumaux:1,));

                x.Dynid = int32(xfile.symbolCount);
                syms = append(syms, s);

                size = uint64(x.Size);
                a4 = addr(new XcoffAuxCSect64(Xauxtype:_AUX_CSECT,Xsmclas:XMC_UL,Xsmtyp:XTY_CM,Xscnlenlo:uint32(size&0xFFFFFFFF),Xscnlenhi:uint32(size>>32),));

                syms = append(syms, a4);
            else 
                return ;
                        {
                ptr<XcoffSymEnt64> s__prev1 = s;

                foreach (var (_, __s) in syms)
                {
                    s = __s;
                    xfile.addSymbol(s);
                }

                s = s__prev1;
            }
        }

        // Generate XCOFF Symbol table.
        // It will be written in out file in Asmbxcoff, because it must be
        // at the very end, especially after relocation sections which needs symbols' index.
        private static void asmaixsym(this ptr<xcoffFile> _addr_f, ptr<Link> _addr_ctxt)
        {
            ref xcoffFile f = ref _addr_f.val;
            ref Link ctxt = ref _addr_ctxt.val;
 
            // Get correct size for symbols wrapping others symbols like go.string.*
            // sym.Size can be used directly as the symbols have already been written.
            foreach (var (name, size) in outerSymSize)
            {
                var sym = ctxt.Syms.ROLookup(name, 0L);
                if (sym == null)
                {
                    Errorf(null, "unknown outer symbol with name %s", name);
                }
                else
                {
                    sym.Size = size;
                }

            }
            genasmsym(ctxt, putaixsym);
            xfile.updatePreviousFile(ctxt, true);

        }

        private static void genDynSym(this ptr<xcoffFile> _addr_f, ptr<Link> _addr_ctxt)
        {
            ref xcoffFile f = ref _addr_f.val;
            ref Link ctxt = ref _addr_ctxt.val;

            slice<ptr<sym.Symbol>> dynsyms = default;
            {
                var s__prev1 = s;

                foreach (var (_, __s) in ctxt.Syms.Allsym)
                {
                    s = __s;
                    if (s.Type != sym.SHOSTOBJ && s.Type != sym.SDYNIMPORT)
                    {
                        continue;
                    }

                    dynsyms = append(dynsyms, s);

                }

                s = s__prev1;
            }

            {
                var s__prev1 = s;

                foreach (var (_, __s) in dynsyms)
                {
                    s = __s;
                    f.adddynimpsym(ctxt, s);

                    {
                        var (_, ok) = f.dynLibraries[s.Dynimplib()];

                        if (!ok)
                        {
                            f.dynLibraries[s.Dynimplib()] = len(f.dynLibraries);
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
        private static void adddynimpsym(this ptr<xcoffFile> _addr_f, ptr<Link> _addr_ctxt, ptr<sym.Symbol> _addr_s)
        {
            ref xcoffFile f = ref _addr_f.val;
            ref Link ctxt = ref _addr_ctxt.val;
            ref sym.Symbol s = ref _addr_s.val;
 
            // Check that library name is given.
            // Pattern is already checked when compiling.
            if (ctxt.LinkMode == LinkInternal && s.Dynimplib() == "")
            {
                Errorf(s, "imported symbol must have a given library");
            }

            s.Type = sym.SXCOFFTOC; 

            // Create new dynamic symbol
            var extsym = ctxt.Syms.Lookup(s.Extname(), 0L);
            extsym.Type = sym.SDYNIMPORT;
            extsym.Attr |= sym.AttrReachable;
            extsym.SetDynimplib(s.Dynimplib());
            extsym.SetExtname(s.Extname());
            extsym.SetDynimpvers(s.Dynimpvers()); 

            // Add loader symbol
            ptr<xcoffLoaderSymbol> lds = addr(new xcoffLoaderSymbol(sym:extsym,smtype:XTY_IMP,smclas:XMC_DS,));
            if (s.Name == "__n_pthreads")
            { 
                // Currently, all imported symbols made by cgo_import_dynamic are
                // syscall functions, except __n_pthreads which is a variable.
                // TODO(aix): Find a way to detect variables imported by cgo.
                lds.smclas = XMC_RW;

            }

            f.loaderSymbols = append(f.loaderSymbols, lds); 

            // Relocation to retrieve the external address
            s.AddBytes(make_slice<byte>(8L));
            s.SetAddr(ctxt.Arch, 0L, extsym);


        }

        // Xcoffadddynrel adds a dynamic relocation in a XCOFF file.
        // This relocation will be made by the loader.
        public static bool Xcoffadddynrel(ptr<Link> _addr_ctxt, ptr<sym.Symbol> _addr_s, ptr<sym.Reloc> _addr_r)
        {
            ref Link ctxt = ref _addr_ctxt.val;
            ref sym.Symbol s = ref _addr_s.val;
            ref sym.Reloc r = ref _addr_r.val;

            if (ctxt.LinkMode == LinkExternal)
            {
                return true;
            }

            if (s.Type <= sym.SPCLNTAB)
            {
                Errorf(s, "cannot have a relocation to %s in a text section symbol", r.Sym.Name);
                return false;
            }

            ptr<xcoffLoaderReloc> ldr = addr(new xcoffLoaderReloc(sym:s,rel:r,));


            if (r.Type == objabi.R_ADDR) 
                if (s.Type == sym.SXCOFFTOC && r.Sym.Type == sym.SDYNIMPORT)
                { 
                    // Imported symbol relocation
                    foreach (var (i, dynsym) in xfile.loaderSymbols)
                    {
                        if (dynsym.sym.Name == r.Sym.Name)
                        {
                            ldr.symndx = int32(i + 3L); // +3 because of 3 section symbols
                            break;

                        }

                    }

                }
                else if (s.Type == sym.SDATA)
                {

                    if (r.Sym.Sect.Seg == _addr_Segtext)                     else if (r.Sym.Sect.Seg == _addr_Segrodata) 
                        ldr.symndx = 0L; // .text
                    else if (r.Sym.Sect.Seg == _addr_Segdata) 
                        if (r.Sym.Type == sym.SBSS || r.Sym.Type == sym.SNOPTRBSS)
                        {
                            ldr.symndx = 2L; // .bss
                        }
                        else
                        {
                            ldr.symndx = 1L; // .data
                        }

                    else 
                        Errorf(s, "unknown segment for .loader relocation with symbol %s", r.Sym.Name);
                    
                }
                else
                {
                    Errorf(s, "unexpected type for .loader relocation R_ADDR for symbol %s: %s to %s", r.Sym.Name, s.Type, r.Sym.Type);
                    return false;
                }

                ldr.rtype = 0x3FUL << (int)(8L) + XCOFF_R_POS;
            else 
                Errorf(s, "unexpected .loader relocation to symbol: %s (type: %s)", r.Sym.Name, r.Type.String());
                return false;
                        xfile.loaderReloc = append(xfile.loaderReloc, ldr);
            return true;

        }

        private static void doxcoff(this ptr<Link> _addr_ctxt)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            if (FlagD.val)
            { 
                // All XCOFF files have dynamic symbols because of the syscalls.
                Exitf("-d is not available on AIX");

            } 

            // TOC
            var toc = ctxt.Syms.Lookup("TOC", 0L);
            toc.Type = sym.SXCOFFTOC;
            toc.Attr |= sym.AttrReachable;
            toc.Attr |= sym.AttrVisibilityHidden; 

            // Add entry point to .loader symbols.
            var ep = ctxt.Syms.ROLookup(flagEntrySymbol.val, 0L);
            if (!ep.Attr.Reachable())
            {
                Exitf("wrong entry point");
            }

            xfile.loaderSymbols = append(xfile.loaderSymbols, addr(new xcoffLoaderSymbol(sym:ep,smtype:XTY_ENT|XTY_SD,smclas:XMC_DS,)));

            xfile.genDynSym(ctxt);

            {
                var s__prev1 = s;

                foreach (var (_, __s) in ctxt.Syms.Allsym)
                {
                    s = __s;
                    if (strings.HasPrefix(s.Name, "TOC."))
                    {
                        s.Type = sym.SXCOFFTOC;
                    }

                }

                s = s__prev1;
            }

            if (ctxt.LinkMode == LinkExternal)
            { 
                // Change rt0_go name to match name in runtime/cgo:main().
                var rt0 = ctxt.Syms.ROLookup("runtime.rt0_go", 0L);
                ctxt.Syms.Rename(rt0.Name, "runtime_rt0_go", 0L, ctxt.Reachparent);

                {
                    var s__prev1 = s;

                    foreach (var (_, __s) in ctxt.Syms.Allsym)
                    {
                        s = __s;
                        if (!s.Attr.CgoExport())
                        {
                            continue;
                        }

                        var name = s.Extname();
                        if (s.Type == sym.STEXT)
                        { 
                            // On AIX, a exported function must have two symbols:
                            // - a .text symbol which must start with a ".".
                            // - a .data symbol which is a function descriptor.
                            ctxt.Syms.Rename(s.Name, "." + name, 0L, ctxt.Reachparent);

                            var desc = ctxt.Syms.Lookup(name, 0L);
                            desc.Type = sym.SNOPTRDATA;
                            desc.AddAddr(ctxt.Arch, s);
                            desc.AddAddr(ctxt.Arch, toc);
                            desc.AddUint64(ctxt.Arch, 0L);

                        }

                    }

                    s = s__prev1;
                }
            }

        }

        // Loader section
        // Currently, this section is created from scratch when assembling the XCOFF file
        // according to information retrieved in xfile object.

        // Create loader section and returns its size
        public static void Loaderblk(ptr<Link> _addr_ctxt, ulong off)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            xfile.writeLdrScn(ctxt, off);
        }

        private static void writeLdrScn(this ptr<xcoffFile> _addr_f, ptr<Link> _addr_ctxt, ulong globalOff)
        {
            ref xcoffFile f = ref _addr_f.val;
            ref Link ctxt = ref _addr_ctxt.val;

            slice<ptr<XcoffLdSym64>> symtab = default;
            slice<ptr<XcoffLdStr64>> strtab = default;
            slice<ptr<XcoffLdImportFile64>> importtab = default;
            slice<ptr<XcoffLdRel64>> reloctab = default;
            slice<ptr<XcoffLdRel64>> dynimpreloc = default; 

            // As the string table is updated in any loader subsection,
            //  its length must be computed at the same time.
            var stlen = uint32(0L); 

            // Loader Header
            ptr<XcoffLdHdr64> hdr = addr(new XcoffLdHdr64(Lversion:2,Lsymoff:LDHDRSZ_64,)); 

            /* Symbol table */
            {
                var s__prev1 = s;

                foreach (var (_, __s) in f.loaderSymbols)
                {
                    s = __s;
                    ptr<XcoffLdSym64> lds = addr(new XcoffLdSym64(Loffset:uint32(stlen+2),Lsmtype:s.smtype,Lsmclas:s.smclas,));

                    if (s.smtype == XTY_ENT | XTY_SD) 
                        lds.Lvalue = uint64(s.sym.Value);
                        lds.Lscnum = f.getXCOFFscnum(s.sym.Sect);
                    else if (s.smtype == XTY_IMP) 
                        lds.Lifile = int32(f.dynLibraries[s.sym.Dynimplib()] + 1L);
                    else 
                        Errorf(s.sym, "unexpected loader symbol type: 0x%x", s.smtype);
                                        ptr<XcoffLdStr64> ldstr = addr(new XcoffLdStr64(size:uint16(len(s.sym.Name)+1),name:s.sym.Name,));
                    stlen += uint32(2L + ldstr.size); // 2 = sizeof ldstr.size
                    symtab = append(symtab, lds);
                    strtab = append(strtab, ldstr);


                }

                s = s__prev1;
            }

            hdr.Lnsyms = int32(len(symtab));
            hdr.Lrldoff = hdr.Lsymoff + uint64(24L * hdr.Lnsyms); // 24 = sizeof one symbol
            var off = hdr.Lrldoff; // current offset is the same of reloc offset

            /* Reloc */
            var ep = ctxt.Syms.ROLookup(flagEntrySymbol.val, 0L);
            ptr<XcoffLdRel64> ldr = addr(new XcoffLdRel64(Lvaddr:uint64(ep.Value),Lrtype:0x3F00,Lrsecnm:f.getXCOFFscnum(ep.Sect),Lsymndx:0,));
            off += 16L;
            reloctab = append(reloctab, ldr);

            off += uint64(16L * len(f.loaderReloc));
            {
                var r__prev1 = r;

                foreach (var (_, __r) in f.loaderReloc)
                {
                    r = __r;
                    ldr = addr(new XcoffLdRel64(Lvaddr:uint64(r.sym.Value+int64(r.rel.Off)),Lrtype:r.rtype,Lsymndx:r.symndx,));

                    if (r.sym.Sect != null)
                    {
                        ldr.Lrsecnm = f.getXCOFFscnum(r.sym.Sect);
                    }

                    reloctab = append(reloctab, ldr);

                }

                r = r__prev1;
            }

            off += uint64(16L * len(dynimpreloc));
            reloctab = append(reloctab, dynimpreloc);

            hdr.Lnreloc = int32(len(reloctab));
            hdr.Limpoff = off;

            /* Import */
            // Default import: /usr/lib:/lib
            ptr<XcoffLdImportFile64> ldimpf = addr(new XcoffLdImportFile64(Limpidpath:"/usr/lib:/lib",));
            off += uint64(len(ldimpf.Limpidpath) + len(ldimpf.Limpidbase) + len(ldimpf.Limpidmem) + 3L); // + null delimiter
            importtab = append(importtab, ldimpf); 

            // The map created by adddynimpsym associates the name to a number
            // This number represents the librairie index (- 1) in this import files section
            // Therefore, they must be sorted before being put inside the section
            var libsOrdered = make_slice<@string>(len(f.dynLibraries));
            foreach (var (key, val) in f.dynLibraries)
            {
                if (libsOrdered[val] != "")
                {
                    continue;
                }

                libsOrdered[val] = key;

            }
            foreach (var (_, lib) in libsOrdered)
            { 
                // lib string is defined as base.a/mem.o or path/base.a/mem.o
                var n = strings.Split(lib, "/");
                @string path = "";
                var @base = n[len(n) - 2L];
                var mem = n[len(n) - 1L];
                if (len(n) > 2L)
                {
                    path = lib[..len(lib) - len(base) - len(mem) - 2L];
                }

                ldimpf = addr(new XcoffLdImportFile64(Limpidpath:path,Limpidbase:base,Limpidmem:mem,));
                off += uint64(len(ldimpf.Limpidpath) + len(ldimpf.Limpidbase) + len(ldimpf.Limpidmem) + 3L); // + null delimiter
                importtab = append(importtab, ldimpf);

            }
            hdr.Lnimpid = int32(len(importtab));
            hdr.Listlen = uint32(off - hdr.Limpoff);
            hdr.Lstoff = off;
            hdr.Lstlen = stlen; 

            /* Writing */
            ctxt.Out.SeekSet(int64(globalOff));
            binary.Write(ctxt.Out, ctxt.Arch.ByteOrder, hdr);

            {
                var s__prev1 = s;

                foreach (var (_, __s) in symtab)
                {
                    s = __s;
                    binary.Write(ctxt.Out, ctxt.Arch.ByteOrder, s);
                }

                s = s__prev1;
            }

            {
                var r__prev1 = r;

                foreach (var (_, __r) in reloctab)
                {
                    r = __r;
                    binary.Write(ctxt.Out, ctxt.Arch.ByteOrder, r);
                }

                r = r__prev1;
            }

            {
                var f__prev1 = f;

                foreach (var (_, __f) in importtab)
                {
                    f = __f;
                    ctxt.Out.WriteString(f.Limpidpath);
                    ctxt.Out.Write8(0L);
                    ctxt.Out.WriteString(f.Limpidbase);
                    ctxt.Out.Write8(0L);
                    ctxt.Out.WriteString(f.Limpidmem);
                    ctxt.Out.Write8(0L);
                }

                f = f__prev1;
            }

            {
                var s__prev1 = s;

                foreach (var (_, __s) in strtab)
                {
                    s = __s;
                    ctxt.Out.Write16(s.size);
                    ctxt.Out.WriteString(s.name);
                    ctxt.Out.Write8(0L); // null terminator
                }

                s = s__prev1;
            }

            f.loaderSize = off + uint64(stlen);
            ctxt.Out.Flush(); 

            /* again for printing */
            if (!flagA.val)
            {
                return ;
            }

            ctxt.Logf("\n.loader section"); 
            // write in buf
            ref bytes.Buffer buf = ref heap(out ptr<bytes.Buffer> _addr_buf);

            binary.Write(_addr_buf, ctxt.Arch.ByteOrder, hdr);
            {
                var s__prev1 = s;

                foreach (var (_, __s) in symtab)
                {
                    s = __s;
                    binary.Write(_addr_buf, ctxt.Arch.ByteOrder, s);
                }

                s = s__prev1;
            }

            {
                var f__prev1 = f;

                foreach (var (_, __f) in importtab)
                {
                    f = __f;
                    buf.WriteString(f.Limpidpath);
                    buf.WriteByte(0L);
                    buf.WriteString(f.Limpidbase);
                    buf.WriteByte(0L);
                    buf.WriteString(f.Limpidmem);
                    buf.WriteByte(0L);
                }

                f = f__prev1;
            }

            {
                var s__prev1 = s;

                foreach (var (_, __s) in strtab)
                {
                    s = __s;
                    binary.Write(_addr_buf, ctxt.Arch.ByteOrder, s.size);
                    buf.WriteString(s.name);
                    buf.WriteByte(0L); // null terminator
                } 

                // Log buffer

                s = s__prev1;
            }

            ctxt.Logf("\n\t%.8x|", globalOff);
            foreach (var (i, b) in buf.Bytes())
            {
                if (i > 0L && i % 16L == 0L)
                {
                    ctxt.Logf("\n\t%.8x|", uint64(globalOff) + uint64(i));
                }

                ctxt.Logf(" %.2x", b);

            }
            ctxt.Logf("\n");


        }

        // XCOFF assembling and writing file

        private static void writeFileHeader(this ptr<xcoffFile> _addr_f, ptr<Link> _addr_ctxt)
        {
            ref xcoffFile f = ref _addr_f.val;
            ref Link ctxt = ref _addr_ctxt.val;
 
            // File header
            f.xfhdr.Fmagic = U64_TOCMAGIC;
            f.xfhdr.Fnscns = uint16(len(f.sections));
            f.xfhdr.Ftimedat = 0L;

            if (!FlagS.val)
            {
                f.xfhdr.Fsymptr = uint64(f.symtabOffset);
                f.xfhdr.Fnsyms = int32(f.symbolCount);
            }

            if (ctxt.BuildMode == BuildModeExe && ctxt.LinkMode == LinkInternal)
            {
                f.xfhdr.Fopthdr = AOUTHSZ_EXEC64;
                f.xfhdr.Fflags = F_EXEC; 

                // auxiliary header
                f.xahdr.Ovstamp = 1L; // based on dump -o
                f.xahdr.Omagic = 0x10bUL;
                copy(f.xahdr.Omodtype[..], "1L");
                var entry = ctxt.Syms.ROLookup(flagEntrySymbol.val, 0L);
                f.xahdr.Oentry = uint64(entry.Value);
                f.xahdr.Osnentry = f.getXCOFFscnum(entry.Sect);
                var toc = ctxt.Syms.ROLookup("TOC", 0L);
                f.xahdr.Otoc = uint64(toc.Value);
                f.xahdr.Osntoc = f.getXCOFFscnum(toc.Sect);

                f.xahdr.Oalgntext = int16(logBase2(int(Funcalign)));
                f.xahdr.Oalgndata = 0x5UL;

                binary.Write(ctxt.Out, binary.BigEndian, _addr_f.xfhdr);
                binary.Write(ctxt.Out, binary.BigEndian, _addr_f.xahdr);

            }
            else
            {
                f.xfhdr.Fopthdr = 0L;
                binary.Write(ctxt.Out, binary.BigEndian, _addr_f.xfhdr);
            }

        }

        private static void xcoffwrite(ptr<Link> _addr_ctxt)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            ctxt.Out.SeekSet(0L);

            xfile.writeFileHeader(ctxt);

            foreach (var (_, sect) in xfile.sections)
            {
                sect.write(ctxt);
            }

        }

        // Generate XCOFF assembly file
        public static void Asmbxcoff(ptr<Link> _addr_ctxt, long fileoff)
        {
            ref Link ctxt = ref _addr_ctxt.val;

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
            if (len(Segrelrodata.Sections) > 0L)
            { 
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

            s = xfile.addSection(".bss", segdataVaddr + segdataFilelen, segbssFilelen, 0L, STYP_BSS);
            xfile.xahdr.Osnbss = xfile.sectNameToScnum[".bss"];
            xfile.xahdr.Obsize = s.Ssize;
            xfile.sectBss = s;

            if (ctxt.LinkMode == LinkExternal)
            {
                ptr<sym.Section> tbss;
                {
                    var s__prev1 = s;

                    foreach (var (_, __s) in Segdata.Sections)
                    {
                        s = __s;
                        if (s.Name == ".tbss")
                        {
                            tbss = s;
                            break;
                        }

                    }

                    s = s__prev1;
                }

                s = xfile.addSection(".tbss", tbss.Vaddr, tbss.Length, 0L, STYP_TBSS);

            } 

            // add dwarf sections
            foreach (var (_, sect) in Segdwarf.Sections)
            {
                xfile.addDwarfSection(sect);
            } 

            // add and write remaining sections
            if (ctxt.LinkMode == LinkInternal)
            { 
                // Loader section
                if (ctxt.BuildMode == BuildModeExe)
                {
                    Loaderblk(_addr_ctxt, uint64(fileoff));
                    s = xfile.addSection(".loader", 0L, xfile.loaderSize, uint64(fileoff), STYP_LOADER);
                    xfile.xahdr.Osnloader = xfile.sectNameToScnum[".loader"]; 

                    // Update fileoff for symbol table
                    fileoff += int64(xfile.loaderSize);

                }

            } 

            // Create Symbol table
            xfile.asmaixsym(ctxt);

            if (ctxt.LinkMode == LinkExternal)
            {
                xfile.emitRelocations(ctxt, fileoff);
            } 

            // Write Symbol table
            xfile.symtabOffset = ctxt.Out.Offset();
            {
                var s__prev1 = s;

                foreach (var (_, __s) in xfile.symtabSym)
                {
                    s = __s;
                    binary.Write(ctxt.Out, ctxt.Arch.ByteOrder, s);
                } 
                // write string table

                s = s__prev1;
            }

            xfile.stringTable.write(ctxt.Out);

            ctxt.Out.Flush(); 

            // write headers
            xcoffwrite(_addr_ctxt);

        }

        // byOffset is used to sort relocations by offset
        private partial struct byOffset // : slice<sym.Reloc>
        {
        }

        private static long Len(this byOffset x)
        {
            return len(x);
        }

        private static void Swap(this byOffset x, long i, long j)
        {
            x[i] = x[j];
            x[j] = x[i];

        }

        private static bool Less(this byOffset x, long i, long j)
        {
            return x[i].Off < x[j].Off;
        }

        // emitRelocations emits relocation entries for go.o in external linking.
        private static void emitRelocations(this ptr<xcoffFile> _addr_f, ptr<Link> _addr_ctxt, long fileoff)
        {
            ref xcoffFile f = ref _addr_f.val;
            ref Link ctxt = ref _addr_ctxt.val;

            ctxt.Out.SeekSet(fileoff);
            while (ctxt.Out.Offset() & 7L != 0L)
            {
                ctxt.Out.Write8(0L);
            } 

            // relocsect relocates symbols from first in section sect, and returns
            // the total number of relocations emitted.
 

            // relocsect relocates symbols from first in section sect, and returns
            // the total number of relocations emitted.
            Func<ptr<sym.Section>, slice<ptr<sym.Symbol>>, ulong, uint> relocsect = (sect, syms, @base) =>
            { 
                // ctxt.Logf("%s 0x%x\n", sect.Name, sect.Vaddr)
                // If main section has no bits, nothing to relocate.
                if (sect.Vaddr >= sect.Seg.Vaddr + sect.Seg.Filelen)
                {
                    return 0L;
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

                var eaddr = int64(sect.Vaddr + sect.Length);
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

                        // Relocation must be ordered by address, so s.R is ordered by Off.
                        sort.Sort(byOffset(s.R));

                        foreach (var (ri) in s.R)
                        {
                            var r = _addr_s.R[ri];

                            if (r.Done)
                            {
                                continue;
                            }

                            if (r.Xsym == null)
                            {
                                Errorf(s, "missing xsym in relocation");
                                continue;
                            }

                            if (r.Xsym.Dynid < 0L)
                            {
                                Errorf(s, "reloc %s to non-coff symbol %s (outer=%s) %d %d", r.Type.String(), r.Sym.Name, r.Xsym.Name, r.Sym.Type, r.Xsym.Dynid);
                            }

                            if (!thearch.Xcoffreloc1(ctxt.Arch, ctxt.Out, s, r, int64(uint64(s.Value + int64(r.Off)) - base)))
                            {
                                Errorf(s, "unsupported obj reloc %d(%s)/%d to %s", r.Type, r.Type.String(), r.Siz, r.Sym.Name);
                            }

                        }

                    }

                    s = s__prev1;
                }

                sect.Rellen = uint64(ctxt.Out.Offset()) - sect.Reloff;
                return uint32(sect.Rellen) / RELSZ_64;

            }
;
            {
                var s__prev1 = s;

                foreach (var (_, __s) in sects)
                {
                    s = __s;
                    s.xcoffSect.Srelptr = uint64(ctxt.Out.Offset());
                    var n = uint32(0L);
                    foreach (var (_, seg) in s.segs)
                    {
                        {
                            var sect__prev3 = sect;

                            foreach (var (_, __sect) in seg.Sections)
                            {
                                sect = __sect;
                                if (sect.Name == ".text")
                                {
                                    n += relocsect(sect, ctxt.Textp, 0L);
                                }
                                else
                                {
                                    n += relocsect(sect, datap, 0L);
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
                var sect__prev1 = sect;

                foreach (var (_, __sect) in Segdwarf.Sections)
                {
                    sect = __sect;
                    foreach (var (_, xcoffSect) in f.sections)
                    {
                        var (_, subtyp) = xcoffGetDwarfSubtype(sect.Name);
                        if (xcoffSect.Sflags & 0xF0000UL == subtyp)
                        {
                            xcoffSect.Srelptr = uint64(ctxt.Out.Offset());
                            xcoffSect.Snreloc = relocsect(sect, dwarfp, sect.Vaddr);
                            _continuedwarfLoop = true;
                            break;
                        }

                    }
                    Errorf(null, "emitRelocations: could not find %q section", sect.Name);

                }

                sect = sect__prev1;
            }
        }

        // xcoffCreateExportFile creates a file with exported symbols for
        // -Wl,-bE option.
        // ld won't export symbols unless they are listed in an export file.
        private static @string xcoffCreateExportFile(ptr<Link> _addr_ctxt)
        {
            @string fname = default;
            ref Link ctxt = ref _addr_ctxt.val;

            fname = filepath.Join(flagTmpdir.val, "export_file.exp");
            bytes.Buffer buf = default;

            foreach (var (_, s) in ctxt.Syms.Allsym)
            {
                if (!s.Attr.CgoExport())
                {
                    continue;
                }

                if (!strings.HasPrefix(s.String(), "_cgoexp_"))
                {
                    continue;
                } 

                // Retrieve the name of the initial symbol
                // exported by cgo.
                // The corresponding Go symbol is:
                // _cgoexp_hashcode_symname.
                var name = strings.SplitN(s.Extname(), "_", 4L)[3L];

                buf.Write((slice<byte>)name + "\n");

            }
            var err = ioutil.WriteFile(fname, buf.Bytes(), 0666L);
            if (err != null)
            {
                Errorf(null, "WriteFile %s failed: %v", fname, err);
            }

            return fname;


        }
    }
}}}}
