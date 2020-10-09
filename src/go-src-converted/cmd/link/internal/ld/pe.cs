// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// PE (Portable Executable) file writing
// https://www.microsoft.com/whdc/system/platform/firmware/PECOFF.mspx

// package ld -- go2cs converted at 2020 October 09 05:50:24 UTC
// import "cmd/link/internal/ld" ==> using ld = go.cmd.link.@internal.ld_package
// Original source: C:\Go\src\cmd\link\internal\ld\pe.go
using objabi = go.cmd.@internal.objabi_package;
using sys = go.cmd.@internal.sys_package;
using loader = go.cmd.link.@internal.loader_package;
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

        public static readonly ulong PEBASE = (ulong)0x00400000UL;


 
        // SectionAlignment must be greater than or equal to FileAlignment.
        // The default is the page size for the architecture.
        public static long PESECTALIGN = 0x1000UL;        public static long PEFILEALIGN = 2L << (int)(8L);

        public static readonly ulong IMAGE_SCN_CNT_CODE = (ulong)0x00000020UL;
        public static readonly ulong IMAGE_SCN_CNT_INITIALIZED_DATA = (ulong)0x00000040UL;
        public static readonly ulong IMAGE_SCN_CNT_UNINITIALIZED_DATA = (ulong)0x00000080UL;
        public static readonly ulong IMAGE_SCN_MEM_EXECUTE = (ulong)0x20000000UL;
        public static readonly ulong IMAGE_SCN_MEM_READ = (ulong)0x40000000UL;
        public static readonly ulong IMAGE_SCN_MEM_WRITE = (ulong)0x80000000UL;
        public static readonly ulong IMAGE_SCN_MEM_DISCARDABLE = (ulong)0x2000000UL;
        public static readonly ulong IMAGE_SCN_LNK_NRELOC_OVFL = (ulong)0x1000000UL;
        public static readonly ulong IMAGE_SCN_ALIGN_32BYTES = (ulong)0x600000UL;


        // TODO(crawshaw): add these constants to debug/pe.
 
        // TODO: the Microsoft doco says IMAGE_SYM_DTYPE_ARRAY is 3 and IMAGE_SYM_DTYPE_FUNCTION is 2
        public static readonly long IMAGE_SYM_TYPE_NULL = (long)0L;
        public static readonly long IMAGE_SYM_TYPE_STRUCT = (long)8L;
        public static readonly ulong IMAGE_SYM_DTYPE_FUNCTION = (ulong)0x20UL;
        public static readonly ulong IMAGE_SYM_DTYPE_ARRAY = (ulong)0x30UL;
        public static readonly long IMAGE_SYM_CLASS_EXTERNAL = (long)2L;
        public static readonly long IMAGE_SYM_CLASS_STATIC = (long)3L;

        public static readonly ulong IMAGE_REL_I386_DIR32 = (ulong)0x0006UL;
        public static readonly ulong IMAGE_REL_I386_SECREL = (ulong)0x000BUL;
        public static readonly ulong IMAGE_REL_I386_REL32 = (ulong)0x0014UL;

        public static readonly ulong IMAGE_REL_AMD64_ADDR64 = (ulong)0x0001UL;
        public static readonly ulong IMAGE_REL_AMD64_ADDR32 = (ulong)0x0002UL;
        public static readonly ulong IMAGE_REL_AMD64_REL32 = (ulong)0x0004UL;
        public static readonly ulong IMAGE_REL_AMD64_SECREL = (ulong)0x000BUL;

        public static readonly ulong IMAGE_REL_ARM_ABSOLUTE = (ulong)0x0000UL;
        public static readonly ulong IMAGE_REL_ARM_ADDR32 = (ulong)0x0001UL;
        public static readonly ulong IMAGE_REL_ARM_ADDR32NB = (ulong)0x0002UL;
        public static readonly ulong IMAGE_REL_ARM_BRANCH24 = (ulong)0x0003UL;
        public static readonly ulong IMAGE_REL_ARM_BRANCH11 = (ulong)0x0004UL;
        public static readonly ulong IMAGE_REL_ARM_SECREL = (ulong)0x000FUL;

        public static readonly long IMAGE_REL_BASED_HIGHLOW = (long)3L;
        public static readonly long IMAGE_REL_BASED_DIR64 = (long)10L;


        public static readonly long PeMinimumTargetMajorVersion = (long)6L;
        public static readonly long PeMinimumTargetMinorVersion = (long)1L;


        // DOS stub that prints out
        // "This program cannot be run in DOS mode."
        private static byte dosstub = new slice<byte>(new byte[] { 0x4d, 0x5a, 0x90, 0x00, 0x03, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0xff, 0xff, 0x00, 0x00, 0x8b, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x40, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x00, 0x00, 0x00, 0x0e, 0x1f, 0xba, 0x0e, 0x00, 0xb4, 0x09, 0xcd, 0x21, 0xb8, 0x01, 0x4c, 0xcd, 0x21, 0x54, 0x68, 0x69, 0x73, 0x20, 0x70, 0x72, 0x6f, 0x67, 0x72, 0x61, 0x6d, 0x20, 0x63, 0x61, 0x6e, 0x6e, 0x6f, 0x74, 0x20, 0x62, 0x65, 0x20, 0x72, 0x75, 0x6e, 0x20, 0x69, 0x6e, 0x20, 0x44, 0x4f, 0x53, 0x20, 0x6d, 0x6f, 0x64, 0x65, 0x2e, 0x0d, 0x0d, 0x0a, 0x24, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });

        public partial struct Imp
        {
            public loader.Sym s;
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

        private static loader.Sym rsrcsym = default;        public static int PESECTHEADR = default;        public static int PEFILEHEADR = default;        private static long pe64 = default;        private static ptr<Dll> dr;        private static var dexport = make_slice<loader.Sym>(0L, 1024L);

        // peStringTable is a COFF string table.
        private partial struct peStringTable
        {
            public slice<@string> strings;
            public long stringsLen;
        }

        // size returns size of string table t.
        private static long size(this ptr<peStringTable> _addr_t)
        {
            ref peStringTable t = ref _addr_t.val;
 
            // string table starts with 4-byte length at the beginning
            return t.stringsLen + 4L;

        }

        // add adds string str to string table t.
        private static long add(this ptr<peStringTable> _addr_t, @string str)
        {
            ref peStringTable t = ref _addr_t.val;

            var off = t.size();
            t.strings = append(t.strings, str);
            t.stringsLen += len(str) + 1L; // each string will have 0 appended to it
            return off;

        }

        // write writes string table t into the output file.
        private static void write(this ptr<peStringTable> _addr_t, ptr<OutBuf> _addr_@out)
        {
            ref peStringTable t = ref _addr_t.val;
            ref OutBuf @out = ref _addr_@out.val;

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
        private static void checkOffset(this ptr<peSection> _addr_sect, long off)
        {
            ref peSection sect = ref _addr_sect.val;

            if (off != int64(sect.pointerToRawData))
            {
                Errorf(null, "%s.PointerToRawData = %#x, want %#x", sect.name, uint64(int64(sect.pointerToRawData)), uint64(off));
                errorexit();
            }

        }

        // checkSegment verifies COFF section sect matches address
        // and file offset provided in segment seg.
        private static void checkSegment(this ptr<peSection> _addr_sect, ptr<sym.Segment> _addr_seg)
        {
            ref peSection sect = ref _addr_sect.val;
            ref sym.Segment seg = ref _addr_seg.val;

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
        private static void pad(this ptr<peSection> _addr_sect, ptr<OutBuf> _addr_@out, uint n)
        {
            ref peSection sect = ref _addr_sect.val;
            ref OutBuf @out = ref _addr_@out.val;

            @out.WriteStringN("", int(sect.sizeOfRawData - n));
        }

        // write writes COFF section sect into the output file.
        private static error write(this ptr<peSection> _addr_sect, ptr<OutBuf> _addr_@out, LinkMode linkmode)
        {
            ref peSection sect = ref _addr_sect.val;
            ref OutBuf @out = ref _addr_@out.val;

            pe.SectionHeader32 h = new pe.SectionHeader32(VirtualSize:sect.virtualSize,SizeOfRawData:sect.sizeOfRawData,PointerToRawData:sect.pointerToRawData,PointerToRelocations:sect.pointerToRelocations,NumberOfRelocations:sect.numberOfRelocations,Characteristics:sect.characteristics,);
            if (linkmode != LinkExternal)
            {
                h.VirtualAddress = sect.virtualAddress;
            }

            copy(h.Name[..], sect.shortName);
            return error.As(binary.Write(out, binary.LittleEndian, h))!;

        }

        // emitRelocations emits the relocation entries for the sect.
        // The actual relocations are emitted by relocfn.
        // This updates the corresponding PE section table entry
        // with the relocation offset and count.
        private static long emitRelocations(this ptr<peSection> _addr_sect, ptr<OutBuf> _addr_@out, Func<long> relocfn)
        {
            ref peSection sect = ref _addr_sect.val;
            ref OutBuf @out = ref _addr_@out.val;

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
            public slice<ptr<peSection>> sections;
            public peStringTable stringTable;
            public ptr<peSection> textSect;
            public ptr<peSection> rdataSect;
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
        private static ptr<peSection> addSection(this ptr<peFile> _addr_f, @string name, long sectsize, long filesize)
        {
            ref peFile f = ref _addr_f.val;

            ptr<peSection> sect = addr(new peSection(name:name,shortName:name,index:len(f.sections)+1,virtualSize:uint32(sectsize),virtualAddress:f.nextSectOffset,pointerToRawData:f.nextFileOffset,));
            f.nextSectOffset = uint32(Rnd(int64(f.nextSectOffset) + int64(sectsize), PESECTALIGN));
            if (filesize > 0L)
            {
                sect.sizeOfRawData = uint32(Rnd(int64(filesize), PEFILEALIGN));
                f.nextFileOffset += sect.sizeOfRawData;
            }

            f.sections = append(f.sections, sect);
            return _addr_sect!;

        }

        // addDWARFSection adds DWARF section to the COFF file f.
        // This function is similar to addSection, but DWARF section names are
        // longer than 8 characters, so they need to be stored in the string table.
        private static ptr<peSection> addDWARFSection(this ptr<peFile> _addr_f, @string name, long size)
        {
            ref peFile f = ref _addr_f.val;

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
            return _addr_h!;

        }

        // addDWARF adds DWARF information to the COFF file f.
        private static void addDWARF(this ptr<peFile> _addr_f)
        {
            ref peFile f = ref _addr_f.val;

            if (FlagS.val)
            { // disable symbol table
                return ;

            }

            if (FlagW.val)
            { // disable dwarf
                return ;

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
        private static ptr<peSection> addInitArray(this ptr<peFile> _addr_f, ptr<Link> _addr_ctxt)
        {
            ref peFile f = ref _addr_f.val;
            ref Link ctxt = ref _addr_ctxt.val;
 
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
                case "arm": 
                    size = 4L;
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

            var init_entry = ctxt.Syms.Lookup(flagEntrySymbol.val, 0L);
            var addr = uint64(init_entry.Value) - init_entry.Sect.Vaddr;
            switch (objabi.GOARCH)
            {
                case "386": 

                case "arm": 
                    ctxt.Out.Write32(uint32(addr));
                    break;
                case "amd64": 
                    ctxt.Out.Write64(addr);
                    break;
            }
            return _addr_sect!;

        }

        // emitRelocations emits relocation entries for go.o in external linking.
        private static void emitRelocations(this ptr<peFile> _addr_f, ptr<Link> _addr_ctxt) => func((_, panic, __) =>
        {
            ref peFile f = ref _addr_f.val;
            ref Link ctxt = ref _addr_ctxt.val;

            while (ctxt.Out.Offset() & 7L != 0L)
            {
                ctxt.Out.Write8(0L);
            } 

            // relocsect relocates symbols from first in section sect, and returns
            // the total number of relocations emitted.
 

            // relocsect relocates symbols from first in section sect, and returns
            // the total number of relocations emitted.
            Func<ptr<sym.Section>, slice<ptr<sym.Symbol>>, ulong, long> relocsect = (sect, syms, @base) =>
            { 
                // If main section has no bits, nothing to relocate.
                if (sect.Vaddr >= sect.Seg.Vaddr + sect.Seg.Filelen)
                {
                    return 0L;
                }

                long relocs = 0L;
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

                    foreach (var (ri) in sym.R)
                    {
                        var r = _addr_sym.R[ri];
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

                        if (!thearch.PEreloc1(ctxt.Arch, ctxt.Out, sym, r, int64(uint64(sym.Value + int64(r.Off)) - base)))
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

            {
                var s__prev1 = s;

                foreach (var (_, __s) in sects)
                {
                    s = __s;
                    s.peSect.emitRelocations(ctxt.Out, () =>
                    {
                        long n = default;
                        {
                            var sect__prev2 = sect;

                            foreach (var (_, __sect) in s.seg.Sections)
                            {
                                sect = __sect;
                                n += relocsect(sect, s.syms, s.seg.Vaddr);
                            }

                            sect = sect__prev2;
                        }

                        return n;

                    });

                }

                s = s__prev1;
            }

dwarfLoop:

            {
                var i__prev1 = i;

                for (long i = 0L; i < len(Segdwarf.Sections); i++)
                {
                    var sect = Segdwarf.Sections[i];
                    var si = dwarfp[i];
                    if (si.secSym() != sect.Sym || si.secSym().Sect != sect)
                    {
                        panic("inconsistency between dwarfp and Segdwarf");
                    }

                    foreach (var (_, pesect) in f.sections)
                    {
                        if (sect.Name == pesect.name)
                        {
                            pesect.emitRelocations(ctxt.Out, () =>
                            {
                                return relocsect(sect, si.syms, sect.Vaddr);
                            });
                            _continuedwarfLoop = true;
                            break;
                        }

                    }
                    Errorf(null, "emitRelocations: could not find %q section", sect.Name);

                }


                i = i__prev1;
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
                    case "arm": 
                        ctxt.Out.Write16(IMAGE_REL_ARM_ADDR32);
                        break;
                    default: 
                        Errorf(dottext, "unknown architecture for PE: %q\n", objabi.GOARCH);
                        break;
                }
                return 1L;

            });

        });

        // writeSymbol appends symbol s to file f symbol table.
        // It also sets s.Dynid to written symbol number.
        private static void writeSymbol(this ptr<peFile> _addr_f, ptr<OutBuf> _addr_@out, ptr<sym.Symbol> _addr_s, long value, long sectidx, ushort typ, byte @class)
        {
            ref peFile f = ref _addr_f.val;
            ref OutBuf @out = ref _addr_@out.val;
            ref sym.Symbol s = ref _addr_s.val;

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
        private static (long, long, error) mapToPESection(this ptr<peFile> _addr_f, ptr<sym.Symbol> _addr_s, LinkMode linkmode)
        {
            long pesectidx = default;
            long offset = default;
            error err = default!;
            ref peFile f = ref _addr_f.val;
            ref sym.Symbol s = ref _addr_s.val;

            if (s.Sect == null)
            {
                return (0L, 0L, error.As(fmt.Errorf("could not map %s symbol with no section", s.Name))!);
            }

            if (s.Sect.Seg == _addr_Segtext)
            {
                return (f.textSect.index, int64(uint64(s.Value) - Segtext.Vaddr), error.As(null!)!);
            }

            if (s.Sect.Seg == _addr_Segrodata)
            {
                return (f.rdataSect.index, int64(uint64(s.Value) - Segrodata.Vaddr), error.As(null!)!);
            }

            if (s.Sect.Seg != _addr_Segdata)
            {
                return (0L, 0L, error.As(fmt.Errorf("could not map %s symbol with non .text or .rdata or .data section", s.Name))!);
            }

            var v = uint64(s.Value) - Segdata.Vaddr;
            if (linkmode != LinkExternal)
            {
                return (f.dataSect.index, int64(v), error.As(null!)!);
            }

            if (s.Type == sym.SDATA)
            {
                return (f.dataSect.index, int64(v), error.As(null!)!);
            } 
            // Note: although address of runtime.edata (type sym.SDATA) is at the start of .bss section
            // it still belongs to the .data section, not the .bss section.
            if (v < Segdata.Filelen)
            {
                return (f.dataSect.index, int64(v), error.As(null!)!);
            }

            return (f.bssSect.index, int64(v - Segdata.Filelen), error.As(null!)!);

        }

        // writeSymbols writes all COFF symbol table records.
        private static void writeSymbols(this ptr<peFile> _addr_f, ptr<Link> _addr_ctxt)
        {
            ref peFile f = ref _addr_f.val;
            ref Link ctxt = ref _addr_ctxt.val;

            Action<ptr<Link>, ptr<sym.Symbol>, @string, SymbolType, long> put = (ctxt, s, name, type_, addr) =>
            {
                if (s == null)
                {
                    return ;
                }

                if (s.Sect == null && type_ != UndefinedSym)
                {
                    return ;
                }


                if (type_ == DataSym || type_ == BSSSym || type_ == TextSym || type_ == UndefinedSym)                 else 
                    return ;
                // Only windows/386 requires underscore prefix on external symbols.
                if (ctxt.Arch.Family == sys.I386 && ctxt.LinkMode == LinkExternal && (s.Type == sym.SHOSTOBJ || s.Type == sym.SUNDEFEXT || s.Attr.CgoExport()))
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
                if (s.IsFileLocal() || s.Attr.VisibilityHidden() || s.Attr.Local())
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
        private static void writeSymbolTableAndStringTable(this ptr<peFile> _addr_f, ptr<Link> _addr_ctxt)
        {
            ref peFile f = ref _addr_f.val;
            ref Link ctxt = ref _addr_ctxt.val;

            f.symtabOffset = ctxt.Out.Offset(); 

            // write COFF symbol table
            if (!FlagS || ctxt.LinkMode == LinkExternal.val)
            {
                f.writeSymbols(ctxt);
            } 

            // update COFF file header and section table
            var size = f.stringTable.size() + 18L * f.symbolCount;
            ptr<peSection> h;
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
        private static void writeFileHeader(this ptr<peFile> _addr_f, ptr<Link> _addr_ctxt)
        {
            ref peFile f = ref _addr_f.val;
            ref Link ctxt = ref _addr_ctxt.val;

            ref pe.FileHeader fh = ref heap(out ptr<pe.FileHeader> _addr_fh);


            if (ctxt.Arch.Family == sys.AMD64) 
                fh.Machine = pe.IMAGE_FILE_MACHINE_AMD64;
            else if (ctxt.Arch.Family == sys.I386) 
                fh.Machine = pe.IMAGE_FILE_MACHINE_I386;
            else if (ctxt.Arch.Family == sys.ARM) 
                fh.Machine = pe.IMAGE_FILE_MACHINE_ARMNT;
            else 
                Exitf("unknown PE architecture: %v", ctxt.Arch.Family);
                        fh.NumberOfSections = uint16(len(f.sections)); 

            // Being able to produce identical output for identical input is
            // much more beneficial than having build timestamp in the header.
            fh.TimeDateStamp = 0L;

            if (ctxt.LinkMode == LinkExternal)
            {
                fh.Characteristics = pe.IMAGE_FILE_LINE_NUMS_STRIPPED;
            }
            else
            {
                fh.Characteristics = pe.IMAGE_FILE_EXECUTABLE_IMAGE | pe.IMAGE_FILE_DEBUG_STRIPPED;

                if (ctxt.Arch.Family == sys.AMD64 || ctxt.Arch.Family == sys.I386) 
                    if (ctxt.BuildMode != BuildModePIE)
                    {
                        fh.Characteristics |= pe.IMAGE_FILE_RELOCS_STRIPPED;
                    }

                            }

            if (pe64 != 0L)
            {
                ref pe.OptionalHeader64 oh64 = ref heap(out ptr<pe.OptionalHeader64> _addr_oh64);
                fh.SizeOfOptionalHeader = uint16(binary.Size(_addr_oh64));
                fh.Characteristics |= pe.IMAGE_FILE_LARGE_ADDRESS_AWARE;
            }
            else
            {
                ref pe.OptionalHeader32 oh = ref heap(out ptr<pe.OptionalHeader32> _addr_oh);
                fh.SizeOfOptionalHeader = uint16(binary.Size(_addr_oh));
                fh.Characteristics |= pe.IMAGE_FILE_32BIT_MACHINE;
            }

            fh.PointerToSymbolTable = uint32(f.symtabOffset);
            fh.NumberOfSymbols = uint32(f.symbolCount);

            binary.Write(ctxt.Out, binary.LittleEndian, _addr_fh);

        }

        // writeOptionalHeader writes COFF optional header for peFile f.
        private static void writeOptionalHeader(this ptr<peFile> _addr_f, ptr<Link> _addr_ctxt)
        {
            ref peFile f = ref _addr_f.val;
            ref Link ctxt = ref _addr_ctxt.val;

            ref pe.OptionalHeader32 oh = ref heap(out ptr<pe.OptionalHeader32> _addr_oh);
            ref pe.OptionalHeader64 oh64 = ref heap(out ptr<pe.OptionalHeader64> _addr_oh64);

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
            oh64.MajorOperatingSystemVersion = PeMinimumTargetMajorVersion;
            oh.MajorOperatingSystemVersion = PeMinimumTargetMajorVersion;
            oh64.MinorOperatingSystemVersion = PeMinimumTargetMinorVersion;
            oh.MinorOperatingSystemVersion = PeMinimumTargetMinorVersion;
            oh64.MajorImageVersion = 1L;
            oh.MajorImageVersion = 1L;
            oh64.MinorImageVersion = 0L;
            oh.MinorImageVersion = 0L;
            oh64.MajorSubsystemVersion = PeMinimumTargetMajorVersion;
            oh.MajorSubsystemVersion = PeMinimumTargetMajorVersion;
            oh64.MinorSubsystemVersion = PeMinimumTargetMinorVersion;
            oh.MinorSubsystemVersion = PeMinimumTargetMinorVersion;
            oh64.SizeOfImage = f.nextSectOffset;
            oh.SizeOfImage = f.nextSectOffset;
            oh64.SizeOfHeaders = uint32(PEFILEHEADR);
            oh.SizeOfHeaders = uint32(PEFILEHEADR);
            if (windowsgui)
            {
                oh64.Subsystem = pe.IMAGE_SUBSYSTEM_WINDOWS_GUI;
                oh.Subsystem = pe.IMAGE_SUBSYSTEM_WINDOWS_GUI;
            }
            else
            {
                oh64.Subsystem = pe.IMAGE_SUBSYSTEM_WINDOWS_CUI;
                oh.Subsystem = pe.IMAGE_SUBSYSTEM_WINDOWS_CUI;
            } 

            // Mark as having awareness of terminal services, to avoid ancient compatibility hacks.
            oh64.DllCharacteristics |= pe.IMAGE_DLLCHARACTERISTICS_TERMINAL_SERVER_AWARE;
            oh.DllCharacteristics |= pe.IMAGE_DLLCHARACTERISTICS_TERMINAL_SERVER_AWARE; 

            // Enable DEP
            oh64.DllCharacteristics |= pe.IMAGE_DLLCHARACTERISTICS_NX_COMPAT;
            oh.DllCharacteristics |= pe.IMAGE_DLLCHARACTERISTICS_NX_COMPAT; 

            // The DLL can be relocated at load time.

            if (ctxt.Arch.Family == sys.AMD64 || ctxt.Arch.Family == sys.I386) 
                if (ctxt.BuildMode == BuildModePIE)
                {
                    oh64.DllCharacteristics |= pe.IMAGE_DLLCHARACTERISTICS_DYNAMIC_BASE;
                    oh.DllCharacteristics |= pe.IMAGE_DLLCHARACTERISTICS_DYNAMIC_BASE;
                }

            else if (ctxt.Arch.Family == sys.ARM) 
                oh64.DllCharacteristics |= pe.IMAGE_DLLCHARACTERISTICS_DYNAMIC_BASE;
                oh.DllCharacteristics |= pe.IMAGE_DLLCHARACTERISTICS_DYNAMIC_BASE;
            // Image can handle a high entropy 64-bit virtual address space.
            if (ctxt.BuildMode == BuildModePIE)
            {
                oh64.DllCharacteristics |= pe.IMAGE_DLLCHARACTERISTICS_HIGH_ENTROPY_VA;
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
            // The default stack reserve size directly affects only the main
            // thread, ctrlhandler thread, and profileloop thread. For
            // these, it must be greater than the stack size assumed by
            // externalthreadhandler.
            //
            // For other threads, the runtime explicitly asks the kernel
            // to use the default stack size so that all stacks are
            // consistent.
            //
            // At thread start, in minit, the runtime queries the OS for
            // the actual stack bounds so that the stack size doesn't need
            // to be hard-coded into the runtime.
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
                binary.Write(ctxt.Out, binary.LittleEndian, _addr_oh64);
            }
            else
            {
                binary.Write(ctxt.Out, binary.LittleEndian, _addr_oh);
            }

        }

        private static peFile pefile = default;

        public static void Peinit(ptr<Link> _addr_ctxt)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            long l = default;


            // 64-bit architectures
            if (ctxt.Arch.Family == sys.AMD64) 
                pe64 = 1L;
                ref pe.OptionalHeader64 oh64 = ref heap(out ptr<pe.OptionalHeader64> _addr_oh64);
                l = binary.Size(_addr_oh64); 

                // 32-bit architectures
            else 
                ref pe.OptionalHeader32 oh = ref heap(out ptr<pe.OptionalHeader32> _addr_oh);
                l = binary.Size(_addr_oh);
                        if (ctxt.LinkMode == LinkExternal)
            { 
                // .rdata section will contain "masks" and "shifts" symbols, and they
                // need to be aligned to 16-bytes. So make all sections aligned
                // to 32-byte and mark them all IMAGE_SCN_ALIGN_32BYTES so external
                // linker will honour that requirement.
                PESECTALIGN = 32L;
                PEFILEALIGN = 0L;

            }

            ref array<pe.SectionHeader32> sh = ref heap(new array<pe.SectionHeader32>(16L), out ptr<array<pe.SectionHeader32>> _addr_sh);
            ref pe.FileHeader fh = ref heap(out ptr<pe.FileHeader> _addr_fh);
            PEFILEHEADR = int32(Rnd(int64(len(dosstub) + binary.Size(_addr_fh) + l + binary.Size(_addr_sh)), PEFILEALIGN));
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
                foreach (var (_, name) in new array<@string>(new @string[] { "__image_base__", "_image_base__" }))
                {
                    var s = ctxt.loader.LookupOrCreateSym(name, 0L);
                    var sb = ctxt.loader.MakeSymbolUpdater(s);
                    sb.SetType(sym.SDATA);
                    sb.SetValue(PEBASE);
                    ctxt.loader.SetAttrReachable(s, true);
                    ctxt.loader.SetAttrSpecial(s, true);
                    ctxt.loader.SetAttrLocal(s, true);
                }

            }

            HEADR = PEFILEHEADR;
            if (FlagTextAddr == -1L.val)
            {
                FlagTextAddr.val = PEBASE + int64(PESECTHEADR);
            }

            if (FlagRound == -1L.val)
            {
                FlagRound.val = int(PESECTALIGN);
            }

        }

        private static void pewrite(ptr<Link> _addr_ctxt)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            ctxt.Out.SeekSet(0L);
            if (ctxt.LinkMode != LinkExternal)
            {
                ctxt.Out.Write(dosstub);
                ctxt.Out.WriteStringN("PE", 4L);
            }

            pefile.writeFileHeader(ctxt);

            pefile.writeOptionalHeader(ctxt);

            foreach (var (_, sect) in pefile.sections)
            {
                sect.write(ctxt.Out, ctxt.LinkMode);
            }

        }

        private static void strput(ptr<OutBuf> _addr_@out, @string s)
        {
            ref OutBuf @out = ref _addr_@out.val;

            @out.WriteString(s);
            @out.Write8(0L); 
            // string must be padded to even size
            if ((len(s) + 1L) % 2L != 0L)
            {
                @out.Write8(0L);
            }

        }

        private static ptr<Dll> initdynimport(ptr<Link> _addr_ctxt)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            var ldr = ctxt.loader;
            ptr<Dll> d;

            dr = null;
            ptr<Imp> m;
            for (var s = loader.Sym(1L); s < loader.Sym(ldr.NSym()); s++)
            {
                if (!ldr.AttrReachable(s) || ldr.SymType(s) != sym.SDYNIMPORT)
                {
                    continue;
                }

                var dynlib = ldr.SymDynimplib(s);
                d = dr;

                while (d != null)
                {
                    if (d.name == dynlib)
                    {
                        m = @new<Imp>();
                        break;
                    d = d.next;
                    }

                }


                if (d == null)
                {
                    d = @new<Dll>();
                    d.name = dynlib;
                    d.next = dr;
                    dr = d;
                    m = @new<Imp>();
                } 

                // Because external link requires properly stdcall decorated name,
                // all external symbols in runtime use %n to denote that the number
                // of uinptrs this function consumes. Store the argsize and discard
                // the %n suffix if any.
                m.argsize = -1L;
                var extName = ldr.SymExtname(s);
                {
                    var i = strings.IndexByte(extName, '%');

                    if (i >= 0L)
                    {
                        error err = default!;
                        m.argsize, err = strconv.Atoi(extName[i + 1L..]);
                        if (err != null)
                        {
                            ctxt.Errorf(s, "failed to parse stdcall decoration: %v", err);
                        }

                        m.argsize *= ctxt.Arch.PtrSize;
                        ldr.SetSymExtname(s, extName[..i]);

                    }

                }


                m.s = s;
                m.next = d.ms;
                d.ms = m;

            }


            if (ctxt.IsExternal())
            { 
                // Add real symbol name
                {
                    ptr<Dll> d__prev1 = d;

                    d = dr;

                    while (d != null)
                    {
                        m = d.ms;

                        while (m != null)
                        {
                            var sb = ldr.MakeSymbolUpdater(m.s);
                            sb.SetType(sym.SDATA);
                            sb.Grow(int64(ctxt.Arch.PtrSize));
                            var dynName = sb.Extname(); 
                            // only windows/386 requires stdcall decoration
                            if (ctxt.Is386() && m.argsize >= 0L)
                            {
                                dynName += fmt.Sprintf("@%d", m.argsize);
                            m = m.next;
                            }

                            var dynSym = ldr.CreateSymForUpdate(dynName, 0L);
                            dynSym.SetReachable(true);
                            dynSym.SetType(sym.SHOSTOBJ);
                            sb.AddReloc(new loader.Reloc(Sym:dynSym.Sym(),Type:objabi.R_ADDR,Off:0,Size:uint8(ctxt.Arch.PtrSize)));
                        d = d.next;
                        }
            else


                    }


                    d = d__prev1;
                }

            }            {
                var dynamic = ldr.CreateSymForUpdate(".windynamic", 0L);
                dynamic.SetReachable(true);
                dynamic.SetType(sym.SWINDOWS);
                {
                    ptr<Dll> d__prev1 = d;

                    d = dr;

                    while (d != null)
                    {
                        m = d.ms;

                        while (m != null)
                        {
                            sb = ldr.MakeSymbolUpdater(m.s);
                            sb.SetType(sym.SWINDOWS);
                            dynamic.PrependSub(m.s);
                            sb.SetValue(dynamic.Size());
                            dynamic.SetSize(dynamic.Size() + int64(ctxt.Arch.PtrSize));
                            m = m.next;
                        }


                        dynamic.SetSize(dynamic.Size() + int64(ctxt.Arch.PtrSize));
                        d = d.next;
                    }


                    d = d__prev1;
                }

            }

            return _addr_dr!;

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

        private static void addimports(ptr<Link> _addr_ctxt, ptr<peSection> _addr_datsect)
        {
            ref Link ctxt = ref _addr_ctxt.val;
            ref peSection datsect = ref _addr_datsect.val;

            var ldr = ctxt.loader;
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
            ctxt.Out.SeekSet(startoff + int64(binary.Size(addr(new IMAGE_IMPORT_DESCRIPTOR()))) * int64(n + 1L)); 

            // write dll names
            {
                var d__prev1 = d;

                d = dr;

                while (d != null)
                {
                    d.nameoff = uint64(ctxt.Out.Offset()) - uint64(startoff);
                    strput(_addr_ctxt.Out, d.name);
                    d = d.next;
                } 

                // write function names


                d = d__prev1;
            } 

            // write function names
            {
                var d__prev1 = d;

                d = dr;

                while (d != null)
                {
                    {
                        var m__prev2 = m;

                        var m = d.ms;

                        while (m != null)
                        {
                            m.off = uint64(pefile.nextSectOffset) + uint64(ctxt.Out.Offset()) - uint64(startoff);
                            ctxt.Out.Write16(0L); // hint
                            strput(_addr_ctxt.Out, ldr.Syms[m.s].Extname());
                            m = m.next;
                        }


                        m = m__prev2;
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
                    {
                        var m__prev2 = m;

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


                        m = m__prev2;
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
                    {
                        var m__prev2 = m;

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


                        m = m__prev2;
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
            pefile.dataDirectory[pe.IMAGE_DIRECTORY_ENTRY_IMPORT].VirtualAddress = isect.virtualAddress;
            pefile.dataDirectory[pe.IMAGE_DIRECTORY_ENTRY_IMPORT].Size = isect.virtualSize;
            pefile.dataDirectory[pe.IMAGE_DIRECTORY_ENTRY_IAT].VirtualAddress = uint32(dynamic.Value - PEBASE);
            pefile.dataDirectory[pe.IMAGE_DIRECTORY_ENTRY_IAT].Size = uint32(dynamic.Size);

            @out.SeekSet(endoff);

        }

        private static void initdynexport(ptr<Link> _addr_ctxt)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            var ldr = ctxt.loader;
            for (var s = loader.Sym(1L); s < loader.Sym(ldr.NSym()); s++)
            {
                if (!ldr.AttrReachable(s) || !ldr.AttrCgoExportDynamic(s))
                {
                    continue;
                }

                if (len(dexport) + 1L > cap(dexport))
                {
                    ctxt.Errorf(s, "pe dynexport table is full");
                    errorexit();
                }

                dexport = append(dexport, s);

            }


            sort.Slice(dexport, (i, j) => ldr.SymExtname(dexport[i]) < ldr.SymExtname(dexport[j]));

        }

        private static void addexports(ptr<Link> _addr_ctxt)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            var ldr = ctxt.loader;
            ref IMAGE_EXPORT_DIRECTORY e = ref heap(out ptr<IMAGE_EXPORT_DIRECTORY> _addr_e);

            var nexport = len(dexport);
            var size = binary.Size(_addr_e) + 10L * nexport + len(flagOutfile.val) + 1L;
            {
                var s__prev1 = s;

                foreach (var (_, __s) in dexport)
                {
                    s = __s;
                    size += len(ldr.Syms[s].Extname()) + 1L;
                }

                s = s__prev1;
            }

            if (nexport == 0L)
            {
                return ;
            }

            var sect = pefile.addSection(".edata", size, size);
            sect.characteristics = IMAGE_SCN_CNT_INITIALIZED_DATA | IMAGE_SCN_MEM_READ;
            sect.checkOffset(ctxt.Out.Offset());
            var va = int(sect.virtualAddress);
            pefile.dataDirectory[pe.IMAGE_DIRECTORY_ENTRY_EXPORT].VirtualAddress = uint32(va);
            pefile.dataDirectory[pe.IMAGE_DIRECTORY_ENTRY_EXPORT].Size = sect.virtualSize;

            var vaName = va + binary.Size(_addr_e) + nexport * 4L;
            var vaAddr = va + binary.Size(_addr_e);
            var vaNa = va + binary.Size(_addr_e) + nexport * 8L;

            e.Characteristics = 0L;
            e.MajorVersion = 0L;
            e.MinorVersion = 0L;
            e.NumberOfFunctions = uint32(nexport);
            e.NumberOfNames = uint32(nexport);
            e.Name = uint32(va + binary.Size(_addr_e)) + uint32(nexport) * 10L; // Program names.
            e.Base = 1L;
            e.AddressOfFunctions = uint32(vaAddr);
            e.AddressOfNames = uint32(vaName);
            e.AddressOfNameOrdinals = uint32(vaNa);

            var @out = ctxt.Out; 

            // put IMAGE_EXPORT_DIRECTORY
            binary.Write(out, binary.LittleEndian, _addr_e); 

            // put EXPORT Address Table
            {
                var s__prev1 = s;

                foreach (var (_, __s) in dexport)
                {
                    s = __s;
                    @out.Write32(uint32(ldr.Syms[s].Value - PEBASE));
                } 

                // put EXPORT Name Pointer Table

                s = s__prev1;
            }

            var v = int(e.Name + uint32(len(flagOutfile.val)) + 1L);

            {
                var s__prev1 = s;

                foreach (var (_, __s) in dexport)
                {
                    s = __s;
                    @out.Write32(uint32(v));
                    v += len(ldr.Syms[s].Extname()) + 1L;
                } 

                // put EXPORT Ordinal Table

                s = s__prev1;
            }

            for (long i = 0L; i < nexport; i++)
            {
                @out.Write16(uint16(i));
            } 

            // put Names
 

            // put Names
            @out.WriteStringN(flagOutfile.val, len(flagOutfile.val) + 1L);

            {
                var s__prev1 = s;

                foreach (var (_, __s) in dexport)
                {
                    s = __s;
                    var ss = ldr.Syms[s];
                    @out.WriteStringN(ss.Extname(), len(ss.Extname()) + 1L);
                }

                s = s__prev1;
            }

            sect.pad(out, uint32(size));

        }

        // peBaseRelocEntry represents a single relocation entry.
        private partial struct peBaseRelocEntry
        {
            public ushort typeOff;
            public ptr<sym.Reloc> rel;
            public ptr<sym.Symbol> sym; // For debug
        }

        // peBaseRelocBlock represents a Base Relocation Block. A block
        // is a collection of relocation entries in a page, where each
        // entry describes a single relocation.
        // The block page RVA (Relative Virtual Address) is the index
        // into peBaseRelocTable.blocks.
        private partial struct peBaseRelocBlock
        {
            public slice<peBaseRelocEntry> entries;
        }

        // pePages is a type used to store the list of pages for which there
        // are base relocation blocks. This is defined as a type so that
        // it can be sorted.
        private partial struct pePages // : slice<uint>
        {
        }

        private static long Len(this pePages p)
        {
            return len(p);
        }
        private static void Swap(this pePages p, long i, long j)
        {
            p[i] = p[j];
            p[j] = p[i];
        }
        private static bool Less(this pePages p, long i, long j)
        {
            return p[i] < p[j];
        }

        // A PE base relocation table is a list of blocks, where each block
        // contains relocation information for a single page. The blocks
        // must be emitted in order of page virtual address.
        // See https://docs.microsoft.com/en-us/windows/desktop/debug/pe-format#the-reloc-section-image-only
        private partial struct peBaseRelocTable
        {
            public map<uint, peBaseRelocBlock> blocks; // pePages is a list of keys into blocks map.
// It is stored separately for ease of sorting.
            public pePages pages;
        }

        private static void init(this ptr<peBaseRelocTable> _addr_rt, ptr<Link> _addr_ctxt)
        {
            ref peBaseRelocTable rt = ref _addr_rt.val;
            ref Link ctxt = ref _addr_ctxt.val;

            rt.blocks = make_map<uint, peBaseRelocBlock>();
        }

        private static void addentry(this ptr<peBaseRelocTable> _addr_rt, ptr<Link> _addr_ctxt, ptr<sym.Symbol> _addr_s, ptr<sym.Reloc> _addr_r)
        {
            ref peBaseRelocTable rt = ref _addr_rt.val;
            ref Link ctxt = ref _addr_ctxt.val;
            ref sym.Symbol s = ref _addr_s.val;
            ref sym.Reloc r = ref _addr_r.val;
 
            // pageSize is the size in bytes of a page
            // described by a base relocation block.
            const ulong pageSize = (ulong)0x1000UL;

            const var pageMask = pageSize - 1L;



            var addr = s.Value + int64(r.Off) - int64(PEBASE);
            var page = uint32(addr & ~pageMask);
            var off = uint32(addr & pageMask);

            var (b, ok) = rt.blocks[page];
            if (!ok)
            {
                rt.pages = append(rt.pages, page);
            }

            peBaseRelocEntry e = new peBaseRelocEntry(typeOff:uint16(off&0xFFF),rel:r,sym:s,); 

            // Set entry type
            switch (r.Siz)
            {
                case 4L: 
                    e.typeOff |= uint16(IMAGE_REL_BASED_HIGHLOW << (int)(12L));
                    break;
                case 8L: 
                    e.typeOff |= uint16(IMAGE_REL_BASED_DIR64 << (int)(12L));
                    break;
                default: 
                    Exitf("unsupported relocation size %d\n", r.Siz);
                    break;
            }

            b.entries = append(b.entries, e);
            rt.blocks[page] = b;

        }

        private static void write(this ptr<peBaseRelocTable> _addr_rt, ptr<Link> _addr_ctxt)
        {
            ref peBaseRelocTable rt = ref _addr_rt.val;
            ref Link ctxt = ref _addr_ctxt.val;

            var @out = ctxt.Out; 

            // sort the pages array
            sort.Sort(rt.pages);

            foreach (var (_, p) in rt.pages)
            {
                var b = rt.blocks[p];
                const long sizeOfPEbaseRelocBlock = (long)8L; // 2 * sizeof(uint32)
 // 2 * sizeof(uint32)
                var blockSize = uint32(sizeOfPEbaseRelocBlock + len(b.entries) * 2L);
                @out.Write32(p);
                @out.Write32(blockSize);

                foreach (var (_, e) in b.entries)
                {
                    @out.Write16(e.typeOff);
                }

            }

        }

        private static void addPEBaseRelocSym(ptr<Link> _addr_ctxt, ptr<sym.Symbol> _addr_s, ptr<peBaseRelocTable> _addr_rt)
        {
            ref Link ctxt = ref _addr_ctxt.val;
            ref sym.Symbol s = ref _addr_s.val;
            ref peBaseRelocTable rt = ref _addr_rt.val;

            for (long ri = 0L; ri < len(s.R); ri++)
            {
                var r = _addr_s.R[ri];

                if (r.Sym == null)
                {
                    continue;
                }

                if (!r.Sym.Attr.Reachable())
                {
                    continue;
                }

                if (r.Type >= objabi.ElfRelocOffset)
                {
                    continue;
                }

                if (r.Siz == 0L)
                { // informational relocation
                    continue;

                }

                if (r.Type == objabi.R_DWARFFILEREF)
                {
                    continue;
                }


                if (r.Type == objabi.R_ADDR) 
                    rt.addentry(ctxt, s, r);
                else                 
            }


        }

        private static void addPEBaseReloc(ptr<Link> _addr_ctxt)
        {
            ref Link ctxt = ref _addr_ctxt.val;
 
            // Arm does not work without base relocation table.
            // 386 and amd64 will only require the table for BuildModePIE.

            if (ctxt.Arch.Family == sys.I386 || ctxt.Arch.Family == sys.AMD64) 
                if (ctxt.BuildMode != BuildModePIE)
                {
                    return ;
                }

            else if (ctxt.Arch.Family == sys.ARM)             else 
                return ;
                        ref peBaseRelocTable rt = ref heap(out ptr<peBaseRelocTable> _addr_rt);
            rt.init(ctxt); 

            // Get relocation information
            {
                var s__prev1 = s;

                foreach (var (_, __s) in ctxt.Textp)
                {
                    s = __s;
                    addPEBaseRelocSym(_addr_ctxt, _addr_s, _addr_rt);
                }

                s = s__prev1;
            }

            {
                var s__prev1 = s;

                foreach (var (_, __s) in ctxt.datap)
                {
                    s = __s;
                    addPEBaseRelocSym(_addr_ctxt, _addr_s, _addr_rt);
                } 

                // Write relocation information

                s = s__prev1;
            }

            var startoff = ctxt.Out.Offset();
            rt.write(ctxt);
            var size = ctxt.Out.Offset() - startoff; 

            // Add a PE section and pad it at the end
            var rsect = pefile.addSection(".reloc", int(size), int(size));
            rsect.characteristics = IMAGE_SCN_CNT_INITIALIZED_DATA | IMAGE_SCN_MEM_READ | IMAGE_SCN_MEM_DISCARDABLE;
            rsect.checkOffset(startoff);
            rsect.pad(ctxt.Out, uint32(size));

            pefile.dataDirectory[pe.IMAGE_DIRECTORY_ENTRY_BASERELOC].VirtualAddress = rsect.virtualAddress;
            pefile.dataDirectory[pe.IMAGE_DIRECTORY_ENTRY_BASERELOC].Size = rsect.virtualSize;

        }

        private static void dope(this ptr<Link> _addr_ctxt)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            initdynimport(_addr_ctxt);
            initdynexport(_addr_ctxt);
        }

        private static void setpersrc(ptr<Link> _addr_ctxt, loader.Sym sym)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            if (rsrcsym != 0L)
            {
                Errorf(null, "too many .rsrc sections");
            }

            rsrcsym = sym;
            ctxt.loader.SetAttrReachable(rsrcsym, true);

        }

        private static void addpersrc(ptr<Link> _addr_ctxt)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            if (rsrcsym == 0L)
            {
                return ;
            }

            var rsrc = ctxt.loader.Syms[rsrcsym];
            var data = rsrc.P;
            var size = len(data);
            var h = pefile.addSection(".rsrc", size, size);
            h.characteristics = IMAGE_SCN_MEM_READ | IMAGE_SCN_MEM_WRITE | IMAGE_SCN_CNT_INITIALIZED_DATA;
            h.checkOffset(ctxt.Out.Offset()); 

            // relocation
            foreach (var (ri) in rsrc.R)
            {
                var r = _addr_rsrc.R[ri];
                var p = data[r.Off..];
                var val = uint32(int64(h.virtualAddress) + r.Add); 

                // 32-bit little-endian
                p[0L] = byte(val);

                p[1L] = byte(val >> (int)(8L));
                p[2L] = byte(val >> (int)(16L));
                p[3L] = byte(val >> (int)(24L));

            }
            ctxt.Out.Write(data);
            h.pad(ctxt.Out, uint32(size)); 

            // update data directory
            pefile.dataDirectory[pe.IMAGE_DIRECTORY_ENTRY_RESOURCE].VirtualAddress = h.virtualAddress;

            pefile.dataDirectory[pe.IMAGE_DIRECTORY_ENTRY_RESOURCE].Size = h.virtualSize;

        }

        public static void Asmbpe(ptr<Link> _addr_ctxt)
        {
            ref Link ctxt = ref _addr_ctxt.val;


            if (ctxt.Arch.Family == sys.AMD64 || ctxt.Arch.Family == sys.I386 || ctxt.Arch.Family == sys.ARM)             else 
                Exitf("unknown PE architecture: %v", ctxt.Arch.Family);
                        var t = pefile.addSection(".text", int(Segtext.Length), int(Segtext.Length));
            t.characteristics = IMAGE_SCN_CNT_CODE | IMAGE_SCN_CNT_INITIALIZED_DATA | IMAGE_SCN_MEM_EXECUTE | IMAGE_SCN_MEM_READ;
            if (ctxt.LinkMode == LinkExternal)
            { 
                // some data symbols (e.g. masks) end up in the .text section, and they normally
                // expect larger alignment requirement than the default text section alignment.
                t.characteristics |= IMAGE_SCN_ALIGN_32BYTES;

            }

            t.checkSegment(_addr_Segtext);
            pefile.textSect = t;

            var ro = pefile.addSection(".rdata", int(Segrodata.Length), int(Segrodata.Length));
            ro.characteristics = IMAGE_SCN_CNT_INITIALIZED_DATA | IMAGE_SCN_MEM_READ;
            if (ctxt.LinkMode == LinkExternal)
            { 
                // some data symbols (e.g. masks) end up in the .rdata section, and they normally
                // expect larger alignment requirement than the default text section alignment.
                ro.characteristics |= IMAGE_SCN_ALIGN_32BYTES;

            }

            ro.checkSegment(_addr_Segrodata);
            pefile.rdataSect = ro;

            ptr<peSection> d;
            if (ctxt.LinkMode != LinkExternal)
            {
                d = pefile.addSection(".data", int(Segdata.Length), int(Segdata.Filelen));
                d.characteristics = IMAGE_SCN_CNT_INITIALIZED_DATA | IMAGE_SCN_MEM_READ | IMAGE_SCN_MEM_WRITE;
                d.checkSegment(_addr_Segdata);
                pefile.dataSect = d;
            }
            else
            {
                d = pefile.addSection(".data", int(Segdata.Filelen), int(Segdata.Filelen));
                d.characteristics = IMAGE_SCN_CNT_INITIALIZED_DATA | IMAGE_SCN_MEM_READ | IMAGE_SCN_MEM_WRITE | IMAGE_SCN_ALIGN_32BYTES;
                d.checkSegment(_addr_Segdata);
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
                addimports(_addr_ctxt, d);
                addexports(_addr_ctxt);
                addPEBaseReloc(_addr_ctxt);
            }

            pefile.writeSymbolTableAndStringTable(ctxt);
            addpersrc(_addr_ctxt);
            if (ctxt.LinkMode == LinkExternal)
            {
                pefile.emitRelocations(ctxt);
            }

            pewrite(_addr_ctxt);

        }
    }
}}}}
