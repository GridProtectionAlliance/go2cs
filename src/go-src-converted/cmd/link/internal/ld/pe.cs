// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ld -- go2cs converted at 2020 August 29 10:04:34 UTC
// import "cmd/link/internal/ld" ==> using ld = go.cmd.link.@internal.ld_package
// Original source: C:\Go\src\cmd\link\internal\ld\pe.go
using objabi = go.cmd.@internal.objabi_package;
using sys = go.cmd.@internal.sys_package;
using sym = go.cmd.link.@internal.sym_package;
using pe = go.debug.pe_package;
using binary = go.encoding.binary_package;
using fmt = go.fmt_package;
using sort = go.sort_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace link {
namespace @internal
{
    public static partial class ld_package
    {
        public partial struct IMAGE_IMPORT_DESCRIPTOR
        {
            public uint OriginalFirstThunk;
            public uint TimeDateStamp;
            public uint ForwarderChain;
            public uint Name;
            public uint FirstThunk;
        }

        public partial struct IMAGE_EXPORT_DIRECTORY
        {
            public uint Characteristics;
            public uint TimeDateStamp;
            public ushort MajorVersion;
            public ushort MinorVersion;
            public uint Name;
            public uint Base;
            public uint NumberOfFunctions;
            public uint NumberOfNames;
            public uint AddressOfFunctions;
            public uint AddressOfNames;
            public uint AddressOfNameOrdinals;
        }

        public static readonly ulong PEBASE = 0x00400000UL;

 
        // SectionAlignment must be greater than or equal to FileAlignment.
        // The default is the page size for the architecture.
        public static long PESECTALIGN = 0x1000UL;        public static long PEFILEALIGN = 2L << (int)(8L);

        public static readonly ulong IMAGE_FILE_MACHINE_I386 = 0x14cUL;
        public static readonly ulong IMAGE_FILE_MACHINE_AMD64 = 0x8664UL;
        public static readonly ulong IMAGE_FILE_RELOCS_STRIPPED = 0x0001UL;
        public static readonly ulong IMAGE_FILE_EXECUTABLE_IMAGE = 0x0002UL;
        public static readonly ulong IMAGE_FILE_LINE_NUMS_STRIPPED = 0x0004UL;
        public static readonly ulong IMAGE_FILE_LARGE_ADDRESS_AWARE = 0x0020UL;
        public static readonly ulong IMAGE_FILE_32BIT_MACHINE = 0x0100UL;
        public static readonly ulong IMAGE_FILE_DEBUG_STRIPPED = 0x0200UL;
        public static readonly ulong IMAGE_SCN_CNT_CODE = 0x00000020UL;
        public static readonly ulong IMAGE_SCN_CNT_INITIALIZED_DATA = 0x00000040UL;
        public static readonly ulong IMAGE_SCN_CNT_UNINITIALIZED_DATA = 0x00000080UL;
        public static readonly ulong IMAGE_SCN_MEM_EXECUTE = 0x20000000UL;
        public static readonly ulong IMAGE_SCN_MEM_READ = 0x40000000UL;
        public static readonly ulong IMAGE_SCN_MEM_WRITE = 0x80000000UL;
        public static readonly ulong IMAGE_SCN_MEM_DISCARDABLE = 0x2000000UL;
        public static readonly ulong IMAGE_SCN_LNK_NRELOC_OVFL = 0x1000000UL;
        public static readonly ulong IMAGE_SCN_ALIGN_32BYTES = 0x600000UL;
        public static readonly long IMAGE_DIRECTORY_ENTRY_EXPORT = 0L;
        public static readonly long IMAGE_DIRECTORY_ENTRY_IMPORT = 1L;
        public static readonly long IMAGE_DIRECTORY_ENTRY_RESOURCE = 2L;
        public static readonly long IMAGE_DIRECTORY_ENTRY_EXCEPTION = 3L;
        public static readonly long IMAGE_DIRECTORY_ENTRY_SECURITY = 4L;
        public static readonly long IMAGE_DIRECTORY_ENTRY_BASERELOC = 5L;
        public static readonly long IMAGE_DIRECTORY_ENTRY_DEBUG = 6L;
        public static readonly long IMAGE_DIRECTORY_ENTRY_COPYRIGHT = 7L;
        public static readonly long IMAGE_DIRECTORY_ENTRY_ARCHITECTURE = 7L;
        public static readonly long IMAGE_DIRECTORY_ENTRY_GLOBALPTR = 8L;
        public static readonly long IMAGE_DIRECTORY_ENTRY_TLS = 9L;
        public static readonly long IMAGE_DIRECTORY_ENTRY_LOAD_CONFIG = 10L;
        public static readonly long IMAGE_DIRECTORY_ENTRY_BOUND_IMPORT = 11L;
        public static readonly long IMAGE_DIRECTORY_ENTRY_IAT = 12L;
        public static readonly long IMAGE_DIRECTORY_ENTRY_DELAY_IMPORT = 13L;
        public static readonly long IMAGE_DIRECTORY_ENTRY_COM_DESCRIPTOR = 14L;
        public static readonly long IMAGE_SUBSYSTEM_WINDOWS_GUI = 2L;
        public static readonly long IMAGE_SUBSYSTEM_WINDOWS_CUI = 3L;

        // TODO(crawshaw): add these constants to debug/pe.
 
        // TODO: the Microsoft doco says IMAGE_SYM_DTYPE_ARRAY is 3 and IMAGE_SYM_DTYPE_FUNCTION is 2
        public static readonly long IMAGE_SYM_TYPE_NULL = 0L;
        public static readonly long IMAGE_SYM_TYPE_STRUCT = 8L;
        public static readonly ulong IMAGE_SYM_DTYPE_FUNCTION = 0x20UL;
        public static readonly ulong IMAGE_SYM_DTYPE_ARRAY = 0x30UL;
        public static readonly long IMAGE_SYM_CLASS_EXTERNAL = 2L;
        public static readonly long IMAGE_SYM_CLASS_STATIC = 3L;

        public static readonly ulong IMAGE_REL_I386_DIR32 = 0x0006UL;
        public static readonly ulong IMAGE_REL_I386_SECREL = 0x000BUL;
        public static readonly ulong IMAGE_REL_I386_REL32 = 0x0014UL;

        public static readonly ulong IMAGE_REL_AMD64_ADDR64 = 0x0001UL;
        public static readonly ulong IMAGE_REL_AMD64_ADDR32 = 0x0002UL;
        public static readonly ulong IMAGE_REL_AMD64_REL32 = 0x0004UL;
        public static readonly ulong IMAGE_REL_AMD64_SECREL = 0x000BUL;

        // Copyright 2009 The Go Authors. All rights reserved.
        // Use of this source code is governed by a BSD-style
        // license that can be found in the LICENSE file.

        // PE (Portable Executable) file writing
        // http://www.microsoft.com/whdc/system/platform/firmware/PECOFF.mspx

        // DOS stub that prints out
        // "This program cannot be run in DOS mode."
        private static byte dosstub = new slice<byte>(new byte[] { 0x4d, 0x5a, 0x90, 0x00, 0x03, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0xff, 0xff, 0x00, 0x00, 0x8b, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x40, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x00, 0x00, 0x00, 0x0e, 0x1f, 0xba, 0x0e, 0x00, 0xb4, 0x09, 0xcd, 0x21, 0xb8, 0x01, 0x4c, 0xcd, 0x21, 0x54, 0x68, 0x69, 0x73, 0x20, 0x70, 0x72, 0x6f, 0x67, 0x72, 0x61, 0x6d, 0x20, 0x63, 0x61, 0x6e, 0x6e, 0x6f, 0x74, 0x20, 0x62, 0x65, 0x20, 0x72, 0x75, 0x6e, 0x20, 0x69, 0x6e, 0x20, 0x44, 0x4f, 0x53, 0x20, 0x6d, 0x6f, 0x64, 0x65, 0x2e, 0x0d, 0x0d, 0x0a, 0x24, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });

        public partial struct Imp
        {
            public ptr<sym.Symbol> s;
            public ulong off;
            public ptr<Imp> next;
            public long argsize;
        }

        public partial struct Dll
        {
            public @string name;
            public ulong nameoff;
            public ulong thunkoff;
            public ptr<Imp> ms;
            public ptr<Dll> next;
        }

        private static ref sym.Symbol rsrcsym = default;        public static int PESECTHEADR = default;        public static int PEFILEHEADR = default;        private static long pe64 = default;        private static ref Dll dr = default;        private static array<ref sym.Symbol> dexport = new array<ref sym.Symbol>(1024L);        private static long nexport = default;

        // peStringTable is a COFF string table.
        private partial struct peStringTable
        {
            public slice<@string> strings;
            public long stringsLen;
        }

        // size resturns size of string table t.
        private static long size(this ref peStringTable t)
        { 
            // string table starts with 4-byte length at the beginning
            return t.stringsLen + 4L;
        }

        // add adds string str to string table t.
        private static long add(this ref peStringTable t, @string str)
        {
            var off = t.size();
            t.strings = append(t.strings, str);
            t.stringsLen += len(str) + 1L; // each string will have 0 appended to it
            return off;
        }

        // write writes string table t into the output file.
        private static void write(this ref peStringTable t, ref OutBuf @out)
        {
            @out.Write32(uint32(t.size()));
            foreach (var (_, s) in t.strings)
            {
                @out.WriteString(s);
                @out.Write8(0L);
            }
        }

        // peSection represents section from COFF section table.
        private partial struct peSection
        {
            public @string name;
            public @string shortName;
            public long index; // one-based index into the Section Table
            public uint virtualSize;
            public uint virtualAddress;
            public uint sizeOfRawData;
            public uint pointerToRawData;
            public uint pointerToRelocations;
            public ushort numberOfRelocations;
            public uint characteristics;
        }

        // checkOffset verifies COFF section sect offset in the file.
        private static void checkOffset(this ref peSection sect, long off)
        {
            if (off != int64(sect.pointerToRawData))
            {
                Errorf(null, "%s.PointerToRawData = %#x, want %#x", sect.name, uint64(int64(sect.pointerToRawData)), uint64(off));
                errorexit();
            }
        }

        // checkSegment verifies COFF section sect matches address
        // and file offset provided in segment seg.
        private static void checkSegment(this ref peSection sect, ref sym.Segment seg)
        {
            if (seg.Vaddr - PEBASE != uint64(sect.virtualAddress))
            {
                Errorf(null, "%s.VirtualAddress = %#x, want %#x", sect.name, uint64(int64(sect.virtualAddress)), uint64(int64(seg.Vaddr - PEBASE)));
                errorexit();
            }
            if (seg.Fileoff != uint64(sect.pointerToRawData))
            {
                Errorf(null, "%s.PointerToRawData = %#x, want %#x", sect.name, uint64(int64(sect.pointerToRawData)), uint64(int64(seg.Fileoff)));
                errorexit();
            }
        }

        // pad adds zeros to the section sect. It writes as many bytes
        // as necessary to make section sect.SizeOfRawData bytes long.
        // It assumes that n bytes are already written to the file.
        private static void pad(this ref peSection sect, ref OutBuf @out, uint n)
        {
            @out.WriteStringN("", int(sect.sizeOfRawData - n));
        }

        // write writes COFF section sect into the output file.
        private static error write(this ref peSection sect, ref OutBuf @out, LinkMode linkmode)
        {
            pe.SectionHeader32 h = new pe.SectionHeader32(VirtualSize:sect.virtualSize,SizeOfRawData:sect.sizeOfRawData,PointerToRawData:sect.pointerToRawData,PointerToRelocations:sect.pointerToRelocations,NumberOfRelocations:sect.numberOfRelocations,Characteristics:sect.characteristics,);
            if (linkmode != LinkExternal)
            {
                h.VirtualAddress = sect.virtualAddress;
            }
            copy(h.Name[..], sect.shortName);
            return error.As(binary.Write(out, binary.LittleEndian, h));
        }

        // emitRelocations emits the relocation entries for the sect.
        // The actual relocations are emitted by relocfn.
        // This updates the corresponding PE section table entry
        // with the relocation offset and count.
        private static long emitRelocations(this ref peSection sect, ref OutBuf @out, Func<long> relocfn)
        {
            sect.pointerToRelocations = uint32(@out.Offset()); 
            // first entry: extended relocs
            @out.Write32(0L); // placeholder for number of relocation + 1
            @out.Write32(0L);
            @out.Write16(0L);

            var n = relocfn() + 1L;

            var cpos = @out.Offset();
            @out.SeekSet(int64(sect.pointerToRelocations));
            @out.Write32(uint32(n));
            @out.SeekSet(cpos);
            if (n > 0x10000UL)
            {
                n = 0x10000UL;
                sect.characteristics |= IMAGE_SCN_LNK_NRELOC_OVFL;
            }
            else
            {
                sect.pointerToRelocations += 10L; // skip the extend reloc entry
            }
            sect.numberOfRelocations = uint16(n - 1L);
        }

        // peFile is used to build COFF file.
        private partial struct peFile
        {
            public slice<ref peSection> sections;
            public peStringTable stringTable;
            public ptr<peSection> textSect;
            public ptr<peSection> dataSect;
            public ptr<peSection> bssSect;
            public ptr<peSection> ctorsSect;
            public uint nextSectOffset;
            public uint nextFileOffset;
            public long symtabOffset; // offset to the start of symbol table
            public long symbolCount; // number of symbol table records written
            public array<pe.DataDirectory> dataDirectory;
        }

        // addSection adds section to the COFF file f.
        private static ref peSection addSection(this ref peFile f, @string name, long sectsize, long filesize)
        {
            peSection sect = ref new peSection(name:name,shortName:name,index:len(f.sections)+1,virtualSize:uint32(sectsize),virtualAddress:f.nextSectOffset,pointerToRawData:f.nextFileOffset,);
            f.nextSectOffset = uint32(Rnd(int64(f.nextSectOffset) + int64(sectsize), PESECTALIGN));
            if (filesize > 0L)
            {
                sect.sizeOfRawData = uint32(Rnd(int64(filesize), PEFILEALIGN));
                f.nextFileOffset += sect.sizeOfRawData;
            }
            f.sections = append(f.sections, sect);
            return sect;
        }

        // addDWARFSection adds DWARF section to the COFF file f.
        // This function is similar to addSection, but DWARF section names are
        // longer than 8 characters, so they need to be stored in the string table.
        private static ref peSection addDWARFSection(this ref peFile f, @string name, long size)
        {
            if (size == 0L)
            {
                Exitf("DWARF section %q is empty", name);
            } 
            // DWARF section names are longer than 8 characters.
            // PE format requires such names to be stored in string table,
            // and section names replaced with slash (/) followed by
            // correspondent string table index.
            // see http://www.microsoft.com/whdc/system/platform/firmware/PECOFFdwn.mspx
            // for details
            var off = f.stringTable.add(name);
            var h = f.addSection(name, size, size);
            h.shortName = fmt.Sprintf("/%d", off);
            h.characteristics = IMAGE_SCN_MEM_READ | IMAGE_SCN_MEM_DISCARDABLE;
            return h;
        }

        // addDWARF adds DWARF information to the COFF file f.
        private static void addDWARF(this ref peFile f)
        {
            if (FlagS.Value)
            { // disable symbol table
                return;
            }
            if (FlagW.Value)
            { // disable dwarf
                return;
            }
            foreach (var (_, sect) in Segdwarf.Sections)
            {
                var h = f.addDWARFSection(sect.Name, int(sect.Length));
                var fileoff = sect.Vaddr - Segdwarf.Vaddr + Segdwarf.Fileoff;
                if (uint64(h.pointerToRawData) != fileoff)
                {
                    Exitf("%s.PointerToRawData = %#x, want %#x", sect.Name, h.pointerToRawData, fileoff);
                }
            }
        }

        // addInitArray adds .ctors COFF section to the file f.
        private static ref peSection addInitArray(this ref peFile f, ref Link ctxt)
        { 
            // The size below was determined by the specification for array relocations,
            // and by observing what GCC writes here. If the initarray section grows to
            // contain more than one constructor entry, the size will need to be 8 * constructor_count.
            // However, the entire Go runtime is initialized from just one function, so it is unlikely
            // that this will need to grow in the future.
            long size = default;
            switch (objabi.GOARCH)
            {
                case "386": 
                    size = 4L;
                    break;
                case "amd64": 
                    size = 8L;
                    break;
                default: 
                    Exitf("peFile.addInitArray: unsupported GOARCH=%q\n", objabi.GOARCH);
                    break;
            }
            var sect = f.addSection(".ctors", size, size);
            sect.characteristics = IMAGE_SCN_CNT_INITIALIZED_DATA | IMAGE_SCN_MEM_READ;
            sect.sizeOfRawData = uint32(size);
            ctxt.Out.SeekSet(int64(sect.pointerToRawData));
            sect.checkOffset(ctxt.Out.Offset());

            var init_entry = ctxt.Syms.Lookup(flagEntrySymbol.Value, 0L);
            var addr = uint64(init_entry.Value) - init_entry.Sect.Vaddr;
            switch (objabi.GOARCH)
            {
                case "386": 
                    ctxt.Out.Write32(uint32(addr));
                    break;
                case "amd64": 
                    ctxt.Out.Write64(addr);
                    break;
            }
            return sect;
        }

        // emitRelocations emits relocation entries for go.o in external linking.
        private static void emitRelocations(this ref peFile f, ref Link ctxt)
        {
            while (ctxt.Out.Offset() & 7L != 0L)
            {
                ctxt.Out.Write8(0L);
            } 

            // relocsect relocates symbols from first in section sect, and returns
            // the total number of relocations emitted.
 

            // relocsect relocates symbols from first in section sect, and returns
            // the total number of relocations emitted.
            Func<ref sym.Section, slice<ref sym.Symbol>, ulong, long> relocsect = (sect, syms, @base) =>
            { 
                // If main section has no bits, nothing to relocate.
                if (sect.Vaddr >= sect.Seg.Vaddr + sect.Seg.Filelen)
                {
                    return 0L;
                }
                long relocs = 0L;
                sect.Reloff = uint64(ctxt.Out.Offset());
                foreach (var (i, s) in syms)
                {
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
                var eaddr = int32(sect.Vaddr + sect.Length);
                foreach (var (_, sym) in syms)
                {
                    if (!sym.Attr.Reachable())
                    {
                        continue;
                    }
                    if (sym.Value >= int64(eaddr))
                    {
                        break;
                    }
                    for (long ri = 0L; ri < len(sym.R); ri++)
                    {
                        var r = ref sym.R[ri];
                        if (r.Done)
                        {
                            continue;
                        }
                        if (r.Xsym == null)
                        {
                            Errorf(sym, "missing xsym in relocation");
                            continue;
                        }
                        if (r.Xsym.Dynid < 0L)
                        {
                            Errorf(sym, "reloc %d to non-coff symbol %s (outer=%s) %d", r.Type, r.Sym.Name, r.Xsym.Name, r.Sym.Type);
                        }
                        if (!Thearch.PEreloc1(ctxt.Arch, ctxt.Out, sym, r, int64(uint64(sym.Value + int64(r.Off)) - base)))
                        {
                            Errorf(sym, "unsupported obj reloc %d/%d to %s", r.Type, r.Siz, r.Sym.Name);
                        }
                        relocs++;
                    }

                }
                sect.Rellen = uint64(ctxt.Out.Offset()) - sect.Reloff;
                return relocs;
            }
;

            f.textSect.emitRelocations(ctxt.Out, () =>
            {
                var n = relocsect(Segtext.Sections[0L], ctxt.Textp, Segtext.Vaddr);
                {
                    var sect__prev1 = sect;

                    foreach (var (_, __sect) in Segtext.Sections[1L..])
                    {
                        sect = __sect;
                        n += relocsect(sect, datap, Segtext.Vaddr);
                    }

                    sect = sect__prev1;
                }

                return n;
            });

            f.dataSect.emitRelocations(ctxt.Out, () =>
            {
                n = default;
                {
                    var sect__prev1 = sect;

                    foreach (var (_, __sect) in Segdata.Sections)
                    {
                        sect = __sect;
                        n += relocsect(sect, datap, Segdata.Vaddr);
                    }

                    sect = sect__prev1;
                }

                return n;
            });

dwarfLoop:

            {
                var sect__prev1 = sect;

                foreach (var (_, __sect) in Segdwarf.Sections)
                {
                    sect = __sect;
                    foreach (var (_, pesect) in f.sections)
                    {
                        if (sect.Name == pesect.name)
                        {
                            pesect.emitRelocations(ctxt.Out, () =>
                            {
                                return relocsect(sect, dwarfp, sect.Vaddr);
                            });
                            _continuedwarfLoop = true;
                            break;
                        }
                    }
                    Errorf(null, "emitRelocations: could not find %q section", sect.Name);
                }

                sect = sect__prev1;
            }
            f.ctorsSect.emitRelocations(ctxt.Out, () =>
            {
                var dottext = ctxt.Syms.Lookup(".text", 0L);
                ctxt.Out.Write32(0L);
                ctxt.Out.Write32(uint32(dottext.Dynid));
                switch (objabi.GOARCH)
                {
                    case "386": 
                        ctxt.Out.Write16(IMAGE_REL_I386_DIR32);
                        break;
                    case "amd64": 
                        ctxt.Out.Write16(IMAGE_REL_AMD64_ADDR64);
                        break;
                    default: 
                        Errorf(dottext, "unknown architecture for PE: %q\n", objabi.GOARCH);
                        break;
                }
                return 1L;
            });
        }

        // writeSymbol appends symbol s to file f symbol table.
        // It also sets s.Dynid to written symbol number.
        private static void writeSymbol(this ref peFile f, ref OutBuf @out, ref sym.Symbol s, long value, long sectidx, ushort typ, byte @class)
        {
            if (len(s.Name) > 8L)
            {
                @out.Write32(0L);
                @out.Write32(uint32(f.stringTable.add(s.Name)));
            }
            else
            {
                @out.WriteStringN(s.Name, 8L);
            }
            @out.Write32(uint32(value));
            @out.Write16(uint16(sectidx));
            @out.Write16(typ);
            @out.Write8(class);
            @out.Write8(0L); // no aux entries

            s.Dynid = int32(f.symbolCount);

            f.symbolCount++;
        }

        // mapToPESection searches peFile f for s symbol's location.
        // It returns PE section index, and offset within that section.
        private static (long, long, error) mapToPESection(this ref peFile f, ref sym.Symbol s, LinkMode linkmode)
        {
            if (s.Sect == null)
            {
                return (0L, 0L, fmt.Errorf("could not map %s symbol with no section", s.Name));
            }
            if (s.Sect.Seg == ref Segtext)
            {
                return (f.textSect.index, int64(uint64(s.Value) - Segtext.Vaddr), null);
            }
            if (s.Sect.Seg != ref Segdata)
            {
                return (0L, 0L, fmt.Errorf("could not map %s symbol with non .text or .data section", s.Name));
            }
            var v = uint64(s.Value) - Segdata.Vaddr;
            if (linkmode != LinkExternal)
            {
                return (f.dataSect.index, int64(v), null);
            }
            if (s.Type == sym.SDATA)
            {
                return (f.dataSect.index, int64(v), null);
            } 
            // Note: although address of runtime.edata (type sym.SDATA) is at the start of .bss section
            // it still belongs to the .data section, not the .bss section.
            if (v < Segdata.Filelen)
            {
                return (f.dataSect.index, int64(v), null);
            }
            return (f.bssSect.index, int64(v - Segdata.Filelen), null);
        }

        // writeSymbols writes all COFF symbol table records.
        private static void writeSymbols(this ref peFile f, ref Link ctxt)
        {
            Action<ref Link, ref sym.Symbol, @string, SymbolType, long, ref sym.Symbol> put = (ctxt, s, name, type_, addr, gotype) =>
            {
                if (s == null)
                {
                    return;
                }
                if (s.Sect == null && type_ != UndefinedSym)
                {
                    return;
                }

                if (type_ == DataSym || type_ == BSSSym || type_ == TextSym || type_ == UndefinedSym)                 else 
                    return;
                // Only windows/386 requires underscore prefix on external symbols.
                if (ctxt.Arch.Family == sys.I386 && ctxt.LinkMode == LinkExternal && (s.Type == sym.SHOSTOBJ || s.Attr.CgoExport()))
                {
                    s.Name = "_" + s.Name;
                }
                ushort typ = default;
                if (ctxt.LinkMode == LinkExternal)
                {
                    typ = IMAGE_SYM_TYPE_NULL;
                }
                else
                { 
                    // TODO: fix IMAGE_SYM_DTYPE_ARRAY value and use following expression, instead of 0x0308
                    typ = IMAGE_SYM_DTYPE_ARRAY << (int)(8L) + IMAGE_SYM_TYPE_STRUCT;
                    typ = 0x0308UL; // "array of structs"
                }
                var (sect, value, err) = f.mapToPESection(s, ctxt.LinkMode);
                if (err != null)
                {
                    if (type_ == UndefinedSym)
                    {
                        typ = IMAGE_SYM_DTYPE_FUNCTION;
                    }
                    else
                    {
                        Errorf(s, "addpesym: %v", err);
                    }
                }
                var @class = IMAGE_SYM_CLASS_EXTERNAL;
                if (s.Version != 0L || s.Attr.VisibilityHidden() || s.Attr.Local())
                {
                    class = IMAGE_SYM_CLASS_STATIC;
                }
                f.writeSymbol(ctxt.Out, s, value, sect, typ, uint8(class));
            }
;

            if (ctxt.LinkMode == LinkExternal)
            { 
                // Include section symbols as external, because
                // .ctors and .debug_* section relocations refer to it.
                foreach (var (_, pesect) in f.sections)
                {
                    var sym = ctxt.Syms.Lookup(pesect.name, 0L);
                    f.writeSymbol(ctxt.Out, sym, 0L, pesect.index, IMAGE_SYM_TYPE_NULL, IMAGE_SYM_CLASS_STATIC);
                }
            }
            genasmsym(ctxt, put);
        }

        // writeSymbolTableAndStringTable writes out symbol and string tables for peFile f.
        private static void writeSymbolTableAndStringTable(this ref peFile f, ref Link ctxt)
        {
            f.symtabOffset = ctxt.Out.Offset(); 

            // write COFF symbol table
            if (!FlagS || ctxt.LinkMode == LinkExternal.Value)
            {
                f.writeSymbols(ctxt);
            } 

            // update COFF file header and section table
            var size = f.stringTable.size() + 18L * f.symbolCount;
            ref peSection h = default;
            if (ctxt.LinkMode != LinkExternal)
            { 
                // We do not really need .symtab for go.o, and if we have one, ld
                // will also include it in the exe, and that will confuse windows.
                h = f.addSection(".symtab", size, size);
                h.characteristics = IMAGE_SCN_MEM_READ | IMAGE_SCN_MEM_DISCARDABLE;
                h.checkOffset(f.symtabOffset);
            } 

            // write COFF string table
            f.stringTable.write(ctxt.Out);
            if (ctxt.LinkMode != LinkExternal)
            {
                h.pad(ctxt.Out, uint32(size));
            }
        }

        // writeFileHeader writes COFF file header for peFile f.
        private static void writeFileHeader(this ref peFile f, ref sys.Arch arch, ref OutBuf @out, LinkMode linkmode)
        {
            pe.FileHeader fh = default;


            if (arch.Family == sys.AMD64) 
                fh.Machine = IMAGE_FILE_MACHINE_AMD64;
            else if (arch.Family == sys.I386) 
                fh.Machine = IMAGE_FILE_MACHINE_I386;
            else 
                Exitf("unknown PE architecture: %v", arch.Family);
                        fh.NumberOfSections = uint16(len(f.sections)); 

            // Being able to produce identical output for identical input is
            // much more beneficial than having build timestamp in the header.
            fh.TimeDateStamp = 0L;

            if (linkmode == LinkExternal)
            {
                fh.Characteristics = IMAGE_FILE_LINE_NUMS_STRIPPED;
            }
            else
            {
                fh.Characteristics = IMAGE_FILE_RELOCS_STRIPPED | IMAGE_FILE_EXECUTABLE_IMAGE | IMAGE_FILE_DEBUG_STRIPPED;
            }
            if (pe64 != 0L)
            {
                pe.OptionalHeader64 oh64 = default;
                fh.SizeOfOptionalHeader = uint16(binary.Size(ref oh64));
                fh.Characteristics |= IMAGE_FILE_LARGE_ADDRESS_AWARE;
            }
            else
            {
                pe.OptionalHeader32 oh = default;
                fh.SizeOfOptionalHeader = uint16(binary.Size(ref oh));
                fh.Characteristics |= IMAGE_FILE_32BIT_MACHINE;
            }
            fh.PointerToSymbolTable = uint32(f.symtabOffset);
            fh.NumberOfSymbols = uint32(f.symbolCount);

            binary.Write(out, binary.LittleEndian, ref fh);
        }

        // writeOptionalHeader writes COFF optional header for peFile f.
        private static void writeOptionalHeader(this ref peFile f, ref Link ctxt)
        {
            pe.OptionalHeader32 oh = default;
            pe.OptionalHeader64 oh64 = default;

            if (pe64 != 0L)
            {
                oh64.Magic = 0x20bUL; // PE32+
            }
            else
            {
                oh.Magic = 0x10bUL; // PE32
                oh.BaseOfData = f.dataSect.virtualAddress;
            } 

            // Fill out both oh64 and oh. We only use one. Oh well.
            oh64.MajorLinkerVersion = 3L;
            oh.MajorLinkerVersion = 3L;
            oh64.MinorLinkerVersion = 0L;
            oh.MinorLinkerVersion = 0L;
            oh64.SizeOfCode = f.textSect.sizeOfRawData;
            oh.SizeOfCode = f.textSect.sizeOfRawData;
            oh64.SizeOfInitializedData = f.dataSect.sizeOfRawData;
            oh.SizeOfInitializedData = f.dataSect.sizeOfRawData;
            oh64.SizeOfUninitializedData = 0L;
            oh.SizeOfUninitializedData = 0L;
            if (ctxt.LinkMode != LinkExternal)
            {
                oh64.AddressOfEntryPoint = uint32(Entryvalue(ctxt) - PEBASE);
                oh.AddressOfEntryPoint = uint32(Entryvalue(ctxt) - PEBASE);
            }
            oh64.BaseOfCode = f.textSect.virtualAddress;
            oh.BaseOfCode = f.textSect.virtualAddress;
            oh64.ImageBase = PEBASE;
            oh.ImageBase = PEBASE;
            oh64.SectionAlignment = uint32(PESECTALIGN);
            oh.SectionAlignment = uint32(PESECTALIGN);
            oh64.FileAlignment = uint32(PEFILEALIGN);
            oh.FileAlignment = uint32(PEFILEALIGN);
            oh64.MajorOperatingSystemVersion = 4L;
            oh.MajorOperatingSystemVersion = 4L;
            oh64.MinorOperatingSystemVersion = 0L;
            oh.MinorOperatingSystemVersion = 0L;
            oh64.MajorImageVersion = 1L;
            oh.MajorImageVersion = 1L;
            oh64.MinorImageVersion = 0L;
            oh.MinorImageVersion = 0L;
            oh64.MajorSubsystemVersion = 4L;
            oh.MajorSubsystemVersion = 4L;
            oh64.MinorSubsystemVersion = 0L;
            oh.MinorSubsystemVersion = 0L;
            oh64.SizeOfImage = f.nextSectOffset;
            oh.SizeOfImage = f.nextSectOffset;
            oh64.SizeOfHeaders = uint32(PEFILEHEADR);
            oh.SizeOfHeaders = uint32(PEFILEHEADR);
            if (windowsgui)
            {
                oh64.Subsystem = IMAGE_SUBSYSTEM_WINDOWS_GUI;
                oh.Subsystem = IMAGE_SUBSYSTEM_WINDOWS_GUI;
            }
            else
            {
                oh64.Subsystem = IMAGE_SUBSYSTEM_WINDOWS_CUI;
                oh.Subsystem = IMAGE_SUBSYSTEM_WINDOWS_CUI;
            } 

            // Disable stack growth as we don't want Windows to
            // fiddle with the thread stack limits, which we set
            // ourselves to circumvent the stack checks in the
            // Windows exception dispatcher.
            // Commit size must be strictly less than reserve
            // size otherwise reserve will be rounded up to a
            // larger size, as verified with VMMap.

            // On 64-bit, we always reserve 2MB stacks. "Pure" Go code is
            // okay with much smaller stacks, but the syscall package
            // makes it easy to call into arbitrary C code without cgo,
            // and system calls even in "pure" Go code are actually C
            // calls that may need more stack than we think.
            //
            // The default stack reserve size affects only the main
            // thread, ctrlhandler thread, and profileloop thread. For
            // these, it must be greater than the stack size assumed by
            // externalthreadhandler.
            //
            // For other threads we specify stack size in runtime explicitly.
            // For these, the reserve must match STACKSIZE in
            // runtime/cgo/gcc_windows_{386,amd64}.c and the correspondent
            // CreateThread parameter in runtime.newosproc.
            oh64.SizeOfStackReserve = 0x00200000UL;
            if (!iscgo)
            {
                oh64.SizeOfStackCommit = 0x00001000UL;
            }
            else
            { 
                // TODO(brainman): Maybe remove optional header writing altogether for cgo.
                // For cgo it is the external linker that is building final executable.
                // And it probably does not use any information stored in optional header.
                oh64.SizeOfStackCommit = 0x00200000UL - 0x2000UL; // account for 2 guard pages
            }
            oh.SizeOfStackReserve = 0x00100000UL;
            if (!iscgo)
            {
                oh.SizeOfStackCommit = 0x00001000UL;
            }
            else
            {
                oh.SizeOfStackCommit = 0x00100000UL - 0x2000UL; // account for 2 guard pages
            }
            oh64.SizeOfHeapReserve = 0x00100000UL;
            oh.SizeOfHeapReserve = 0x00100000UL;
            oh64.SizeOfHeapCommit = 0x00001000UL;
            oh.SizeOfHeapCommit = 0x00001000UL;
            oh64.NumberOfRvaAndSizes = 16L;
            oh.NumberOfRvaAndSizes = 16L;

            if (pe64 != 0L)
            {
                oh64.DataDirectory = f.dataDirectory;
            }
            else
            {
                oh.DataDirectory = f.dataDirectory;
            }
            if (pe64 != 0L)
            {
                binary.Write(ctxt.Out, binary.LittleEndian, ref oh64);
            }
            else
            {
                binary.Write(ctxt.Out, binary.LittleEndian, ref oh);
            }
        }

        private static peFile pefile = default;

        public static void Peinit(ref Link ctxt)
        {
            long l = default;


            // 64-bit architectures
            if (ctxt.Arch.Family == sys.AMD64) 
                pe64 = 1L;
                pe.OptionalHeader64 oh64 = default;
                l = binary.Size(ref oh64); 

                // 32-bit architectures
            else 
                pe.OptionalHeader32 oh = default;
                l = binary.Size(ref oh);
                        if (ctxt.LinkMode == LinkExternal)
            {
                PESECTALIGN = 0L;
                PEFILEALIGN = 0L;
            }
            array<pe.SectionHeader32> sh = new array<pe.SectionHeader32>(16L);
            pe.FileHeader fh = default;
            PEFILEHEADR = int32(Rnd(int64(len(dosstub) + binary.Size(ref fh) + l + binary.Size(ref sh)), PEFILEALIGN));
            if (ctxt.LinkMode != LinkExternal)
            {
                PESECTHEADR = int32(Rnd(int64(PEFILEHEADR), PESECTALIGN));
            }
            else
            {
                PESECTHEADR = 0L;
            }
            pefile.nextSectOffset = uint32(PESECTHEADR);
            pefile.nextFileOffset = uint32(PEFILEHEADR);

            if (ctxt.LinkMode == LinkInternal)
            { 
                // some mingw libs depend on this symbol, for example, FindPESectionByName
                ctxt.xdefine("__image_base__", sym.SDATA, PEBASE);
                ctxt.xdefine("_image_base__", sym.SDATA, PEBASE);
            }
            HEADR = PEFILEHEADR;
            if (FlagTextAddr == -1L.Value)
            {
                FlagTextAddr.Value = PEBASE + int64(PESECTHEADR);
            }
            if (FlagDataAddr == -1L.Value)
            {
                FlagDataAddr.Value = 0L;
            }
            if (FlagRound == -1L.Value)
            {
                FlagRound.Value = int(PESECTALIGN);
            }
            if (FlagDataAddr != 0L && FlagRound != 0L.Value.Value)
            {
                fmt.Printf("warning: -D0x%x is ignored because of -R0x%x\n", uint64(FlagDataAddr.Value), uint32(FlagRound.Value));
            }
        }

        private static void pewrite(ref Link ctxt)
        {
            ctxt.Out.SeekSet(0L);
            if (ctxt.LinkMode != LinkExternal)
            {
                ctxt.Out.Write(dosstub);
                ctxt.Out.WriteStringN("PE", 4L);
            }
            pefile.writeFileHeader(ctxt.Arch, ctxt.Out, ctxt.LinkMode);

            pefile.writeOptionalHeader(ctxt);

            foreach (var (_, sect) in pefile.sections)
            {
                sect.write(ctxt.Out, ctxt.LinkMode);
            }
        }

        private static void strput(ref OutBuf @out, @string s)
        {
            @out.WriteString(s);
            @out.Write8(0L); 
            // string must be padded to even size
            if ((len(s) + 1L) % 2L != 0L)
            {
                @out.Write8(0L);
            }
        }

        private static ref Dll initdynimport(ref Link ctxt)
        {
            ref Dll d = default;

            dr = null;
            ref Imp m = default;
            foreach (var (_, s) in ctxt.Syms.Allsym)
            {
                if (!s.Attr.Reachable() || s.Type != sym.SDYNIMPORT)
                {
                    continue;
                }
                d = dr;

                while (d != null)
                {
                    if (d.name == s.Dynimplib)
                    {
                        m = @new<Imp>();
                        break;
                    d = d.next;
                    }
                }


                if (d == null)
                {
                    d = @new<Dll>();
                    d.name = s.Dynimplib;
                    d.next = dr;
                    dr = d;
                    m = @new<Imp>();
                } 

                // Because external link requires properly stdcall decorated name,
                // all external symbols in runtime use %n to denote that the number
                // of uinptrs this function consumes. Store the argsize and discard
                // the %n suffix if any.
                m.argsize = -1L;
                {
                    var i = strings.IndexByte(s.Extname, '%');

                    if (i >= 0L)
                    {
                        error err = default;
                        m.argsize, err = strconv.Atoi(s.Extname[i + 1L..]);
                        if (err != null)
                        {
                            Errorf(s, "failed to parse stdcall decoration: %v", err);
                        }
                        m.argsize *= ctxt.Arch.PtrSize;
                        s.Extname = s.Extname[..i];
                    }

                }

                m.s = s;
                m.next = d.ms;
                d.ms = m;
            }
            if (ctxt.LinkMode == LinkExternal)
            { 
                // Add real symbol name
                {
                    ref Dll d__prev1 = d;

                    d = dr;

                    while (d != null)
                    {
                        m = d.ms;

                        while (m != null)
                        {
                            m.s.Type = sym.SDATA;
                            m.s.Grow(int64(ctxt.Arch.PtrSize));
                            var dynName = m.s.Extname; 
                            // only windows/386 requires stdcall decoration
                            if (ctxt.Arch.Family == sys.I386 && m.argsize >= 0L)
                            {
                                dynName += fmt.Sprintf("@%d", m.argsize);
                            m = m.next;
                            }
                            var dynSym = ctxt.Syms.Lookup(dynName, 0L);
                            dynSym.Attr |= sym.AttrReachable;
                            dynSym.Type = sym.SHOSTOBJ;
                            var r = m.s.AddRel();
                            r.Sym = dynSym;
                            r.Off = 0L;
                            r.Siz = uint8(ctxt.Arch.PtrSize);
                            r.Type = objabi.R_ADDR;
                        d = d.next;
                        }
            else

                    }


                    d = d__prev1;
                }
            }            {
                var dynamic = ctxt.Syms.Lookup(".windynamic", 0L);
                dynamic.Attr |= sym.AttrReachable;
                dynamic.Type = sym.SWINDOWS;
                {
                    ref Dll d__prev1 = d;

                    d = dr;

                    while (d != null)
                    {
                        m = d.ms;

                        while (m != null)
                        {
                            m.s.Type = sym.SWINDOWS;
                            m.s.Attr |= sym.AttrSubSymbol;
                            m.s.Sub = dynamic.Sub;
                            dynamic.Sub = m.s;
                            m.s.Value = dynamic.Size;
                            dynamic.Size += int64(ctxt.Arch.PtrSize);
                            m = m.next;
                        }


                        dynamic.Size += int64(ctxt.Arch.PtrSize);
                        d = d.next;
                    }


                    d = d__prev1;
                }
            }
            return dr;
        }

        // peimporteddlls returns the gcc command line argument to link all imported
        // DLLs.
        private static slice<@string> peimporteddlls()
        {
            slice<@string> dlls = default;

            {
                var d = dr;

                while (d != null)
                {
                    dlls = append(dlls, "-l" + strings.TrimSuffix(d.name, ".dll"));
                    d = d.next;
                }

            }

            return dlls;
        }

        private static void addimports(ref Link ctxt, ref peSection datsect)
        {
            var startoff = ctxt.Out.Offset();
            var dynamic = ctxt.Syms.Lookup(".windynamic", 0L); 

            // skip import descriptor table (will write it later)
            var n = uint64(0L);

            {
                var d__prev1 = d;

                var d = dr;

                while (d != null)
                {
                    n++;
                    d = d.next;
                }


                d = d__prev1;
            }
            ctxt.Out.SeekSet(startoff + int64(binary.Size(ref new IMAGE_IMPORT_DESCRIPTOR())) * int64(n + 1L)); 

            // write dll names
            {
                var d__prev1 = d;

                d = dr;

                while (d != null)
                {
                    d.nameoff = uint64(ctxt.Out.Offset()) - uint64(startoff);
                    strput(ctxt.Out, d.name);
                    d = d.next;
                } 

                // write function names


                d = d__prev1;
            } 

            // write function names
            ref Imp m = default;
            {
                var d__prev1 = d;

                d = dr;

                while (d != null)
                {
                    m = d.ms;

                    while (m != null)
                    {
                        m.off = uint64(pefile.nextSectOffset) + uint64(ctxt.Out.Offset()) - uint64(startoff);
                        ctxt.Out.Write16(0L); // hint
                        strput(ctxt.Out, m.s.Extname);
                        m = m.next;
                    }

                    d = d.next;
                } 

                // write OriginalFirstThunks


                d = d__prev1;
            } 

            // write OriginalFirstThunks
            var oftbase = uint64(ctxt.Out.Offset()) - uint64(startoff);

            n = uint64(ctxt.Out.Offset());
            {
                var d__prev1 = d;

                d = dr;

                while (d != null)
                {
                    d.thunkoff = uint64(ctxt.Out.Offset()) - n;
                    m = d.ms;

                    while (m != null)
                    {
                        if (pe64 != 0L)
                        {
                            ctxt.Out.Write64(m.off);
                        m = m.next;
                        }
                        else
                        {
                            ctxt.Out.Write32(uint32(m.off));
                    d = d.next;
                        }
                    }


                    if (pe64 != 0L)
                    {
                        ctxt.Out.Write64(0L);
                    }
                    else
                    {
                        ctxt.Out.Write32(0L);
                    }
                } 

                // add pe section and pad it at the end


                d = d__prev1;
            } 

            // add pe section and pad it at the end
            n = uint64(ctxt.Out.Offset()) - uint64(startoff);

            var isect = pefile.addSection(".idata", int(n), int(n));
            isect.characteristics = IMAGE_SCN_CNT_INITIALIZED_DATA | IMAGE_SCN_MEM_READ | IMAGE_SCN_MEM_WRITE;
            isect.checkOffset(startoff);
            isect.pad(ctxt.Out, uint32(n));
            var endoff = ctxt.Out.Offset(); 

            // write FirstThunks (allocated in .data section)
            var ftbase = uint64(dynamic.Value) - uint64(datsect.virtualAddress) - PEBASE;

            ctxt.Out.SeekSet(int64(uint64(datsect.pointerToRawData) + ftbase));
            {
                var d__prev1 = d;

                d = dr;

                while (d != null)
                {
                    m = d.ms;

                    while (m != null)
                    {
                        if (pe64 != 0L)
                        {
                            ctxt.Out.Write64(m.off);
                        m = m.next;
                        }
                        else
                        {
                            ctxt.Out.Write32(uint32(m.off));
                    d = d.next;
                        }
                    }


                    if (pe64 != 0L)
                    {
                        ctxt.Out.Write64(0L);
                    }
                    else
                    {
                        ctxt.Out.Write32(0L);
                    }
                } 

                // finally write import descriptor table


                d = d__prev1;
            } 

            // finally write import descriptor table
            var @out = ctxt.Out;
            @out.SeekSet(startoff);

            {
                var d__prev1 = d;

                d = dr;

                while (d != null)
                {
                    @out.Write32(uint32(uint64(isect.virtualAddress) + oftbase + d.thunkoff));
                    @out.Write32(0L);
                    @out.Write32(0L);
                    @out.Write32(uint32(uint64(isect.virtualAddress) + d.nameoff));
                    @out.Write32(uint32(uint64(datsect.virtualAddress) + ftbase + d.thunkoff));
                    d = d.next;
                }


                d = d__prev1;
            }

            @out.Write32(0L); //end
            @out.Write32(0L);
            @out.Write32(0L);
            @out.Write32(0L);
            @out.Write32(0L); 

            // update data directory
            pefile.dataDirectory[IMAGE_DIRECTORY_ENTRY_IMPORT].VirtualAddress = isect.virtualAddress;
            pefile.dataDirectory[IMAGE_DIRECTORY_ENTRY_IMPORT].Size = isect.virtualSize;
            pefile.dataDirectory[IMAGE_DIRECTORY_ENTRY_IAT].VirtualAddress = uint32(dynamic.Value - PEBASE);
            pefile.dataDirectory[IMAGE_DIRECTORY_ENTRY_IAT].Size = uint32(dynamic.Size);

            @out.SeekSet(endoff);
        }

        private partial struct byExtname // : slice<ref sym.Symbol>
        {
        }

        private static long Len(this byExtname s)
        {
            return len(s);
        }
        private static void Swap(this byExtname s, long i, long j)
        {
            s[i] = s[j];
            s[j] = s[i];

        }
        private static bool Less(this byExtname s, long i, long j)
        {
            return s[i].Extname < s[j].Extname;
        }

        private static void initdynexport(ref Link ctxt)
        {
            nexport = 0L;
            foreach (var (_, s) in ctxt.Syms.Allsym)
            {
                if (!s.Attr.Reachable() || !s.Attr.CgoExportDynamic())
                {
                    continue;
                }
                if (nexport + 1L > len(dexport))
                {
                    Errorf(s, "pe dynexport table is full");
                    errorexit();
                }
                dexport[nexport] = s;
                nexport++;
            }
            sort.Sort(byExtname(dexport[..nexport]));
        }

        private static void addexports(ref Link ctxt)
        {
            IMAGE_EXPORT_DIRECTORY e = default;

            var size = binary.Size(ref e) + 10L * nexport + len(flagOutfile.Value) + 1L;
            {
                long i__prev1 = i;

                for (long i = 0L; i < nexport; i++)
                {
                    size += len(dexport[i].Extname) + 1L;
                }


                i = i__prev1;
            }

            if (nexport == 0L)
            {
                return;
            }
            var sect = pefile.addSection(".edata", size, size);
            sect.characteristics = IMAGE_SCN_CNT_INITIALIZED_DATA | IMAGE_SCN_MEM_READ;
            sect.checkOffset(ctxt.Out.Offset());
            var va = int(sect.virtualAddress);
            pefile.dataDirectory[IMAGE_DIRECTORY_ENTRY_EXPORT].VirtualAddress = uint32(va);
            pefile.dataDirectory[IMAGE_DIRECTORY_ENTRY_EXPORT].Size = sect.virtualSize;

            var vaName = va + binary.Size(ref e) + nexport * 4L;
            var vaAddr = va + binary.Size(ref e);
            var vaNa = va + binary.Size(ref e) + nexport * 8L;

            e.Characteristics = 0L;
            e.MajorVersion = 0L;
            e.MinorVersion = 0L;
            e.NumberOfFunctions = uint32(nexport);
            e.NumberOfNames = uint32(nexport);
            e.Name = uint32(va + binary.Size(ref e)) + uint32(nexport) * 10L; // Program names.
            e.Base = 1L;
            e.AddressOfFunctions = uint32(vaAddr);
            e.AddressOfNames = uint32(vaName);
            e.AddressOfNameOrdinals = uint32(vaNa);

            var @out = ctxt.Out; 

            // put IMAGE_EXPORT_DIRECTORY
            binary.Write(out, binary.LittleEndian, ref e); 

            // put EXPORT Address Table
            {
                long i__prev1 = i;

                for (i = 0L; i < nexport; i++)
                {
                    @out.Write32(uint32(dexport[i].Value - PEBASE));
                } 

                // put EXPORT Name Pointer Table


                i = i__prev1;
            } 

            // put EXPORT Name Pointer Table
            var v = int(e.Name + uint32(len(flagOutfile.Value)) + 1L);

            {
                long i__prev1 = i;

                for (i = 0L; i < nexport; i++)
                {
                    @out.Write32(uint32(v));
                    v += len(dexport[i].Extname) + 1L;
                } 

                // put EXPORT Ordinal Table


                i = i__prev1;
            } 

            // put EXPORT Ordinal Table
            {
                long i__prev1 = i;

                for (i = 0L; i < nexport; i++)
                {
                    @out.Write16(uint16(i));
                } 

                // put Names


                i = i__prev1;
            } 

            // put Names
            @out.WriteStringN(flagOutfile.Value, len(flagOutfile.Value) + 1L);

            {
                long i__prev1 = i;

                for (i = 0L; i < nexport; i++)
                {
                    @out.WriteStringN(dexport[i].Extname, len(dexport[i].Extname) + 1L);
                }


                i = i__prev1;
            }
            sect.pad(out, uint32(size));
        }

        private static void dope(this ref Link ctxt)
        { 
            /* relocation table */
            var rel = ctxt.Syms.Lookup(".rel", 0L);

            rel.Attr |= sym.AttrReachable;
            rel.Type = sym.SELFROSECT;

            initdynimport(ctxt);
            initdynexport(ctxt);
        }

        private static void setpersrc(ref Link ctxt, ref sym.Symbol sym)
        {
            if (rsrcsym != null)
            {
                Errorf(sym, "too many .rsrc sections");
            }
            rsrcsym = sym;
        }

        private static void addpersrc(ref Link ctxt)
        {
            if (rsrcsym == null)
            {
                return;
            }
            var h = pefile.addSection(".rsrc", int(rsrcsym.Size), int(rsrcsym.Size));
            h.characteristics = IMAGE_SCN_MEM_READ | IMAGE_SCN_MEM_WRITE | IMAGE_SCN_CNT_INITIALIZED_DATA;
            h.checkOffset(ctxt.Out.Offset()); 

            // relocation
            slice<byte> p = default;
            ref sym.Reloc r = default;
            uint val = default;
            for (long ri = 0L; ri < len(rsrcsym.R); ri++)
            {
                r = ref rsrcsym.R[ri];
                p = rsrcsym.P[r.Off..];
                val = uint32(int64(h.virtualAddress) + r.Add); 

                // 32-bit little-endian
                p[0L] = byte(val);

                p[1L] = byte(val >> (int)(8L));
                p[2L] = byte(val >> (int)(16L));
                p[3L] = byte(val >> (int)(24L));
            }


            ctxt.Out.Write(rsrcsym.P);
            h.pad(ctxt.Out, uint32(rsrcsym.Size)); 

            // update data directory
            pefile.dataDirectory[IMAGE_DIRECTORY_ENTRY_RESOURCE].VirtualAddress = h.virtualAddress;

            pefile.dataDirectory[IMAGE_DIRECTORY_ENTRY_RESOURCE].Size = h.virtualSize;
        }

        public static void Asmbpe(ref Link ctxt)
        {

            if (ctxt.Arch.Family == sys.AMD64 || ctxt.Arch.Family == sys.I386)             else 
                Exitf("unknown PE architecture: %v", ctxt.Arch.Family);
                        var t = pefile.addSection(".text", int(Segtext.Length), int(Segtext.Length));
            t.characteristics = IMAGE_SCN_CNT_CODE | IMAGE_SCN_CNT_INITIALIZED_DATA | IMAGE_SCN_MEM_EXECUTE | IMAGE_SCN_MEM_READ;
            if (ctxt.LinkMode == LinkExternal)
            { 
                // some data symbols (e.g. masks) end up in the .text section, and they normally
                // expect larger alignment requirement than the default text section alignment.
                t.characteristics |= IMAGE_SCN_ALIGN_32BYTES;
            }
            t.checkSegment(ref Segtext);
            pefile.textSect = t;

            ref peSection d = default;
            if (ctxt.LinkMode != LinkExternal)
            {
                d = pefile.addSection(".data", int(Segdata.Length), int(Segdata.Filelen));
                d.characteristics = IMAGE_SCN_CNT_INITIALIZED_DATA | IMAGE_SCN_MEM_READ | IMAGE_SCN_MEM_WRITE;
                d.checkSegment(ref Segdata);
                pefile.dataSect = d;
            }
            else
            {
                d = pefile.addSection(".data", int(Segdata.Filelen), int(Segdata.Filelen));
                d.characteristics = IMAGE_SCN_CNT_INITIALIZED_DATA | IMAGE_SCN_MEM_READ | IMAGE_SCN_MEM_WRITE | IMAGE_SCN_ALIGN_32BYTES;
                d.checkSegment(ref Segdata);
                pefile.dataSect = d;

                var b = pefile.addSection(".bss", int(Segdata.Length - Segdata.Filelen), 0L);
                b.characteristics = IMAGE_SCN_CNT_UNINITIALIZED_DATA | IMAGE_SCN_MEM_READ | IMAGE_SCN_MEM_WRITE | IMAGE_SCN_ALIGN_32BYTES;
                b.pointerToRawData = 0L;
                pefile.bssSect = b;
            }
            pefile.addDWARF();

            if (ctxt.LinkMode == LinkExternal)
            {
                pefile.ctorsSect = pefile.addInitArray(ctxt);
            }
            ctxt.Out.SeekSet(int64(pefile.nextFileOffset));
            if (ctxt.LinkMode != LinkExternal)
            {
                addimports(ctxt, d);
                addexports(ctxt);
            }
            pefile.writeSymbolTableAndStringTable(ctxt);
            addpersrc(ctxt);
            if (ctxt.LinkMode == LinkExternal)
            {
                pefile.emitRelocations(ctxt);
            }
            pewrite(ctxt);
        }
    }
}}}}
