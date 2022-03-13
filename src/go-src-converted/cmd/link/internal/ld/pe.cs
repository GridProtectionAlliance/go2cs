// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// PE (Portable Executable) file writing
// https://docs.microsoft.com/en-us/windows/win32/debug/pe-format

// package ld -- go2cs converted at 2022 March 13 06:35:21 UTC
// import "cmd/link/internal/ld" ==> using ld = go.cmd.link.@internal.ld_package
// Original source: C:\Program Files\Go\src\cmd\link\internal\ld\pe.go
namespace go.cmd.link.@internal;

using objabi = cmd.@internal.objabi_package;
using sys = cmd.@internal.sys_package;
using loader = cmd.link.@internal.loader_package;
using sym = cmd.link.@internal.sym_package;
using pe = debug.pe_package;
using binary = encoding.binary_package;
using fmt = fmt_package;
using buildcfg = @internal.buildcfg_package;
using sort = sort_package;
using strconv = strconv_package;
using strings = strings_package;
using System;

public static partial class ld_package {

public partial struct IMAGE_IMPORT_DESCRIPTOR {
    public uint OriginalFirstThunk;
    public uint TimeDateStamp;
    public uint ForwarderChain;
    public uint Name;
    public uint FirstThunk;
}

public partial struct IMAGE_EXPORT_DIRECTORY {
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

 
// PEBASE is the base address for the executable.
// It is small for 32-bit and large for 64-bit.
public static long PEBASE = default;public static long PESECTALIGN = 0x1000;public static long PEFILEALIGN = 2 << 8;

public static readonly nuint IMAGE_SCN_CNT_CODE = 0x00000020;
public static readonly nuint IMAGE_SCN_CNT_INITIALIZED_DATA = 0x00000040;
public static readonly nuint IMAGE_SCN_CNT_UNINITIALIZED_DATA = 0x00000080;
public static readonly nuint IMAGE_SCN_LNK_OTHER = 0x00000100;
public static readonly nuint IMAGE_SCN_LNK_INFO = 0x00000200;
public static readonly nuint IMAGE_SCN_LNK_REMOVE = 0x00000800;
public static readonly nuint IMAGE_SCN_LNK_COMDAT = 0x00001000;
public static readonly nuint IMAGE_SCN_GPREL = 0x00008000;
public static readonly nuint IMAGE_SCN_MEM_PURGEABLE = 0x00020000;
public static readonly nuint IMAGE_SCN_MEM_16BIT = 0x00020000;
public static readonly nuint IMAGE_SCN_MEM_LOCKED = 0x00040000;
public static readonly nuint IMAGE_SCN_MEM_PRELOAD = 0x00080000;
public static readonly nuint IMAGE_SCN_ALIGN_1BYTES = 0x00100000;
public static readonly nuint IMAGE_SCN_ALIGN_2BYTES = 0x00200000;
public static readonly nuint IMAGE_SCN_ALIGN_4BYTES = 0x00300000;
public static readonly nuint IMAGE_SCN_ALIGN_8BYTES = 0x00400000;
public static readonly nuint IMAGE_SCN_ALIGN_16BYTES = 0x00500000;
public static readonly nuint IMAGE_SCN_ALIGN_32BYTES = 0x00600000;
public static readonly nuint IMAGE_SCN_ALIGN_64BYTES = 0x00700000;
public static readonly nuint IMAGE_SCN_ALIGN_128BYTES = 0x00800000;
public static readonly nuint IMAGE_SCN_ALIGN_256BYTES = 0x00900000;
public static readonly nuint IMAGE_SCN_ALIGN_512BYTES = 0x00A00000;
public static readonly nuint IMAGE_SCN_ALIGN_1024BYTES = 0x00B00000;
public static readonly nuint IMAGE_SCN_ALIGN_2048BYTES = 0x00C00000;
public static readonly nuint IMAGE_SCN_ALIGN_4096BYTES = 0x00D00000;
public static readonly nuint IMAGE_SCN_ALIGN_8192BYTES = 0x00E00000;
public static readonly nuint IMAGE_SCN_LNK_NRELOC_OVFL = 0x01000000;
public static readonly nuint IMAGE_SCN_MEM_DISCARDABLE = 0x02000000;
public static readonly nuint IMAGE_SCN_MEM_NOT_CACHED = 0x04000000;
public static readonly nuint IMAGE_SCN_MEM_NOT_PAGED = 0x08000000;
public static readonly nuint IMAGE_SCN_MEM_SHARED = 0x10000000;
public static readonly nuint IMAGE_SCN_MEM_EXECUTE = 0x20000000;
public static readonly nuint IMAGE_SCN_MEM_READ = 0x40000000;
public static readonly nuint IMAGE_SCN_MEM_WRITE = 0x80000000;

// See https://docs.microsoft.com/en-us/windows/win32/debug/pe-format.
// TODO(crawshaw): add these constants to debug/pe.
 
// TODO: the Microsoft doco says IMAGE_SYM_DTYPE_ARRAY is 3 and IMAGE_SYM_DTYPE_FUNCTION is 2
public static readonly nint IMAGE_SYM_TYPE_NULL = 0;
public static readonly nint IMAGE_SYM_TYPE_STRUCT = 8;
public static readonly nuint IMAGE_SYM_DTYPE_FUNCTION = 0x20;
public static readonly nuint IMAGE_SYM_DTYPE_ARRAY = 0x30;
public static readonly nint IMAGE_SYM_CLASS_EXTERNAL = 2;
public static readonly nint IMAGE_SYM_CLASS_STATIC = 3;

public static readonly nuint IMAGE_REL_I386_DIR32 = 0x0006;
public static readonly nuint IMAGE_REL_I386_SECREL = 0x000B;
public static readonly nuint IMAGE_REL_I386_REL32 = 0x0014;

public static readonly nuint IMAGE_REL_AMD64_ADDR64 = 0x0001;
public static readonly nuint IMAGE_REL_AMD64_ADDR32 = 0x0002;
public static readonly nuint IMAGE_REL_AMD64_REL32 = 0x0004;
public static readonly nuint IMAGE_REL_AMD64_SECREL = 0x000B;

public static readonly nuint IMAGE_REL_ARM_ABSOLUTE = 0x0000;
public static readonly nuint IMAGE_REL_ARM_ADDR32 = 0x0001;
public static readonly nuint IMAGE_REL_ARM_ADDR32NB = 0x0002;
public static readonly nuint IMAGE_REL_ARM_BRANCH24 = 0x0003;
public static readonly nuint IMAGE_REL_ARM_BRANCH11 = 0x0004;
public static readonly nuint IMAGE_REL_ARM_SECREL = 0x000F;

public static readonly nuint IMAGE_REL_ARM64_ABSOLUTE = 0x0000;
public static readonly nuint IMAGE_REL_ARM64_ADDR32 = 0x0001;
public static readonly nuint IMAGE_REL_ARM64_ADDR32NB = 0x0002;
public static readonly nuint IMAGE_REL_ARM64_BRANCH26 = 0x0003;
public static readonly nuint IMAGE_REL_ARM64_PAGEBASE_REL21 = 0x0004;
public static readonly nuint IMAGE_REL_ARM64_REL21 = 0x0005;
public static readonly nuint IMAGE_REL_ARM64_PAGEOFFSET_12A = 0x0006;
public static readonly nuint IMAGE_REL_ARM64_PAGEOFFSET_12L = 0x0007;
public static readonly nuint IMAGE_REL_ARM64_SECREL = 0x0008;
public static readonly nuint IMAGE_REL_ARM64_SECREL_LOW12A = 0x0009;
public static readonly nuint IMAGE_REL_ARM64_SECREL_HIGH12A = 0x000A;
public static readonly nuint IMAGE_REL_ARM64_SECREL_LOW12L = 0x000B;
public static readonly nuint IMAGE_REL_ARM64_TOKEN = 0x000C;
public static readonly nuint IMAGE_REL_ARM64_SECTION = 0x000D;
public static readonly nuint IMAGE_REL_ARM64_ADDR64 = 0x000E;
public static readonly nuint IMAGE_REL_ARM64_BRANCH19 = 0x000F;
public static readonly nuint IMAGE_REL_ARM64_BRANCH14 = 0x0010;
public static readonly nuint IMAGE_REL_ARM64_REL32 = 0x0011;

public static readonly nint IMAGE_REL_BASED_HIGHLOW = 3;
public static readonly nint IMAGE_REL_BASED_DIR64 = 10;

public static readonly nint PeMinimumTargetMajorVersion = 6;
public static readonly nint PeMinimumTargetMinorVersion = 1;

// DOS stub that prints out
// "This program cannot be run in DOS mode."
private static byte dosstub = new slice<byte>(new byte[] { 0x4d, 0x5a, 0x90, 0x00, 0x03, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0xff, 0xff, 0x00, 0x00, 0x8b, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x40, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x00, 0x00, 0x00, 0x0e, 0x1f, 0xba, 0x0e, 0x00, 0xb4, 0x09, 0xcd, 0x21, 0xb8, 0x01, 0x4c, 0xcd, 0x21, 0x54, 0x68, 0x69, 0x73, 0x20, 0x70, 0x72, 0x6f, 0x67, 0x72, 0x61, 0x6d, 0x20, 0x63, 0x61, 0x6e, 0x6e, 0x6f, 0x74, 0x20, 0x62, 0x65, 0x20, 0x72, 0x75, 0x6e, 0x20, 0x69, 0x6e, 0x20, 0x44, 0x4f, 0x53, 0x20, 0x6d, 0x6f, 0x64, 0x65, 0x2e, 0x0d, 0x0d, 0x0a, 0x24, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });

public partial struct Imp {
    public loader.Sym s;
    public ulong off;
    public ptr<Imp> next;
    public nint argsize;
}

public partial struct Dll {
    public @string name;
    public ulong nameoff;
    public ulong thunkoff;
    public ptr<Imp> ms;
    public ptr<Dll> next;
}

private static slice<loader.Sym> rsrcsyms = default;public static int PESECTHEADR = default;public static int PEFILEHEADR = default;private static nint pe64 = default;private static ptr<Dll> dr;private static var dexport = make_slice<loader.Sym>(0, 1024);

// peStringTable is a COFF string table.
private partial struct peStringTable {
    public slice<@string> strings;
    public nint stringsLen;
}

// size returns size of string table t.
private static nint size(this ptr<peStringTable> _addr_t) {
    ref peStringTable t = ref _addr_t.val;
 
    // string table starts with 4-byte length at the beginning
    return t.stringsLen + 4;
}

// add adds string str to string table t.
private static nint add(this ptr<peStringTable> _addr_t, @string str) {
    ref peStringTable t = ref _addr_t.val;

    var off = t.size();
    t.strings = append(t.strings, str);
    t.stringsLen += len(str) + 1; // each string will have 0 appended to it
    return off;
}

// write writes string table t into the output file.
private static void write(this ptr<peStringTable> _addr_t, ptr<OutBuf> _addr_@out) {
    ref peStringTable t = ref _addr_t.val;
    ref OutBuf @out = ref _addr_@out.val;

    @out.Write32(uint32(t.size()));
    foreach (var (_, s) in t.strings) {
        @out.WriteString(s);
        @out.Write8(0);
    }
}

// peSection represents section from COFF section table.
private partial struct peSection {
    public @string name;
    public @string shortName;
    public nint index; // one-based index into the Section Table
    public uint virtualSize;
    public uint virtualAddress;
    public uint sizeOfRawData;
    public uint pointerToRawData;
    public uint pointerToRelocations;
    public ushort numberOfRelocations;
    public uint characteristics;
}

// checkOffset verifies COFF section sect offset in the file.
private static void checkOffset(this ptr<peSection> _addr_sect, long off) {
    ref peSection sect = ref _addr_sect.val;

    if (off != int64(sect.pointerToRawData)) {
        Errorf(null, "%s.PointerToRawData = %#x, want %#x", sect.name, uint64(int64(sect.pointerToRawData)), uint64(off));
        errorexit();
    }
}

// checkSegment verifies COFF section sect matches address
// and file offset provided in segment seg.
private static void checkSegment(this ptr<peSection> _addr_sect, ptr<sym.Segment> _addr_seg) {
    ref peSection sect = ref _addr_sect.val;
    ref sym.Segment seg = ref _addr_seg.val;

    if (seg.Vaddr - uint64(PEBASE) != uint64(sect.virtualAddress)) {
        Errorf(null, "%s.VirtualAddress = %#x, want %#x", sect.name, uint64(int64(sect.virtualAddress)), uint64(int64(seg.Vaddr - uint64(PEBASE))));
        errorexit();
    }
    if (seg.Fileoff != uint64(sect.pointerToRawData)) {
        Errorf(null, "%s.PointerToRawData = %#x, want %#x", sect.name, uint64(int64(sect.pointerToRawData)), uint64(int64(seg.Fileoff)));
        errorexit();
    }
}

// pad adds zeros to the section sect. It writes as many bytes
// as necessary to make section sect.SizeOfRawData bytes long.
// It assumes that n bytes are already written to the file.
private static void pad(this ptr<peSection> _addr_sect, ptr<OutBuf> _addr_@out, uint n) {
    ref peSection sect = ref _addr_sect.val;
    ref OutBuf @out = ref _addr_@out.val;

    @out.WriteStringN("", int(sect.sizeOfRawData - n));
}

// write writes COFF section sect into the output file.
private static error write(this ptr<peSection> _addr_sect, ptr<OutBuf> _addr_@out, LinkMode linkmode) {
    ref peSection sect = ref _addr_sect.val;
    ref OutBuf @out = ref _addr_@out.val;

    pe.SectionHeader32 h = new pe.SectionHeader32(VirtualSize:sect.virtualSize,SizeOfRawData:sect.sizeOfRawData,PointerToRawData:sect.pointerToRawData,PointerToRelocations:sect.pointerToRelocations,NumberOfRelocations:sect.numberOfRelocations,Characteristics:sect.characteristics,);
    if (linkmode != LinkExternal) {
        h.VirtualAddress = sect.virtualAddress;
    }
    copy(h.Name[..], sect.shortName);
    return error.As(binary.Write(out, binary.LittleEndian, h))!;
}

// emitRelocations emits the relocation entries for the sect.
// The actual relocations are emitted by relocfn.
// This updates the corresponding PE section table entry
// with the relocation offset and count.
private static nint emitRelocations(this ptr<peSection> _addr_sect, ptr<OutBuf> _addr_@out, Func<nint> relocfn) {
    ref peSection sect = ref _addr_sect.val;
    ref OutBuf @out = ref _addr_@out.val;

    sect.pointerToRelocations = uint32(@out.Offset()); 
    // first entry: extended relocs
    @out.Write32(0); // placeholder for number of relocation + 1
    @out.Write32(0);
    @out.Write16(0);

    var n = relocfn() + 1;

    var cpos = @out.Offset();
    @out.SeekSet(int64(sect.pointerToRelocations));
    @out.Write32(uint32(n));
    @out.SeekSet(cpos);
    if (n > 0x10000) {
        n = 0x10000;
        sect.characteristics |= IMAGE_SCN_LNK_NRELOC_OVFL;
    }
    else
 {
        sect.pointerToRelocations += 10; // skip the extend reloc entry
    }
    sect.numberOfRelocations = uint16(n - 1);
}

// peFile is used to build COFF file.
private partial struct peFile {
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
    public nint symbolCount; // number of symbol table records written
    public array<pe.DataDirectory> dataDirectory;
}

// addSection adds section to the COFF file f.
private static ptr<peSection> addSection(this ptr<peFile> _addr_f, @string name, nint sectsize, nint filesize) {
    ref peFile f = ref _addr_f.val;

    ptr<peSection> sect = addr(new peSection(name:name,shortName:name,index:len(f.sections)+1,virtualAddress:f.nextSectOffset,pointerToRawData:f.nextFileOffset,));
    f.nextSectOffset = uint32(Rnd(int64(f.nextSectOffset) + int64(sectsize), PESECTALIGN));
    if (filesize > 0) {
        sect.virtualSize = uint32(sectsize);
        sect.sizeOfRawData = uint32(Rnd(int64(filesize), PEFILEALIGN));
        f.nextFileOffset += sect.sizeOfRawData;
    }
    else
 {
        sect.sizeOfRawData = uint32(sectsize);
    }
    f.sections = append(f.sections, sect);
    return _addr_sect!;
}

// addDWARFSection adds DWARF section to the COFF file f.
// This function is similar to addSection, but DWARF section names are
// longer than 8 characters, so they need to be stored in the string table.
private static ptr<peSection> addDWARFSection(this ptr<peFile> _addr_f, @string name, nint size) {
    ref peFile f = ref _addr_f.val;

    if (size == 0) {
        Exitf("DWARF section %q is empty", name);
    }
    var off = f.stringTable.add(name);
    var h = f.addSection(name, size, size);
    h.shortName = fmt.Sprintf("/%d", off);
    h.characteristics = IMAGE_SCN_ALIGN_1BYTES | IMAGE_SCN_MEM_READ | IMAGE_SCN_MEM_DISCARDABLE | IMAGE_SCN_CNT_INITIALIZED_DATA;
    return _addr_h!;
}

// addDWARF adds DWARF information to the COFF file f.
private static void addDWARF(this ptr<peFile> _addr_f) {
    ref peFile f = ref _addr_f.val;

    if (FlagS.val) { // disable symbol table
        return ;
    }
    if (FlagW.val) { // disable dwarf
        return ;
    }
    foreach (var (_, sect) in Segdwarf.Sections) {
        var h = f.addDWARFSection(sect.Name, int(sect.Length));
        var fileoff = sect.Vaddr - Segdwarf.Vaddr + Segdwarf.Fileoff;
        if (uint64(h.pointerToRawData) != fileoff) {
            Exitf("%s.PointerToRawData = %#x, want %#x", sect.Name, h.pointerToRawData, fileoff);
        }
    }
}

// addInitArray adds .ctors COFF section to the file f.
private static ptr<peSection> addInitArray(this ptr<peFile> _addr_f, ptr<Link> _addr_ctxt) {
    ref peFile f = ref _addr_f.val;
    ref Link ctxt = ref _addr_ctxt.val;
 
    // The size below was determined by the specification for array relocations,
    // and by observing what GCC writes here. If the initarray section grows to
    // contain more than one constructor entry, the size will need to be 8 * constructor_count.
    // However, the entire Go runtime is initialized from just one function, so it is unlikely
    // that this will need to grow in the future.
    nint size = default;
    uint alignment = default;
    switch (buildcfg.GOARCH) {
        case "386": 

        case "arm": 
            size = 4;
            alignment = IMAGE_SCN_ALIGN_4BYTES;
            break;
        case "amd64": 

        case "arm64": 
            size = 8;
            alignment = IMAGE_SCN_ALIGN_8BYTES;
            break;
        default: 
            Exitf("peFile.addInitArray: unsupported GOARCH=%q\n", buildcfg.GOARCH);
            break;
    }
    var sect = f.addSection(".ctors", size, size);
    sect.characteristics = IMAGE_SCN_CNT_INITIALIZED_DATA | IMAGE_SCN_MEM_READ | IMAGE_SCN_MEM_WRITE | alignment;
    sect.sizeOfRawData = uint32(size);
    ctxt.Out.SeekSet(int64(sect.pointerToRawData));
    sect.checkOffset(ctxt.Out.Offset());

    var init_entry = ctxt.loader.Lookup(flagEntrySymbol.val, 0);
    var addr = uint64(ctxt.loader.SymValue(init_entry)) - ctxt.loader.SymSect(init_entry).Vaddr;
    switch (buildcfg.GOARCH) {
        case "386": 

        case "arm": 
            ctxt.Out.Write32(uint32(addr));
            break;
        case "amd64": 

        case "arm64": 
            ctxt.Out.Write64(addr);
            break;
    }
    return _addr_sect!;
}

// emitRelocations emits relocation entries for go.o in external linking.
private static void emitRelocations(this ptr<peFile> _addr_f, ptr<Link> _addr_ctxt) => func((_, panic, _) => {
    ref peFile f = ref _addr_f.val;
    ref Link ctxt = ref _addr_ctxt.val;

    while (ctxt.Out.Offset() & 7 != 0) {
        ctxt.Out.Write8(0);
    }

    var ldr = ctxt.loader; 

    // relocsect relocates symbols from first in section sect, and returns
    // the total number of relocations emitted.
    Func<ptr<sym.Section>, slice<loader.Sym>, ulong, nint> relocsect = (sect, syms, @base) => { 
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
                if (ldr.SymValue(s) >= eaddr) {
                    break;
                } 
                // Compute external relocations on the go, and pass to PEreloc1
                // to stream out.
                var relocs = ldr.Relocs(s);
                for (nint ri = 0; ri < relocs.Count(); ri++) {
                    var r = relocs.At(ri);
                    var (rr, ok) = extreloc(ctxt, ldr, s, r);
                    if (!ok) {
                        continue;
                    }
                    if (rr.Xsym == 0) {
                        ctxt.Errorf(s, "missing xsym in relocation");
                        continue;
                    }
                    if (ldr.SymDynid(rr.Xsym) < 0) {
                        ctxt.Errorf(s, "reloc %d to non-coff symbol %s (outer=%s) %d", r.Type(), ldr.SymName(r.Sym()), ldr.SymName(rr.Xsym), ldr.SymType(r.Sym()));
                    }
                    if (!thearch.PEreloc1(ctxt.Arch, ctxt.Out, ldr, s, rr, int64(uint64(ldr.SymValue(s) + int64(r.Off())) - base))) {
                        ctxt.Errorf(s, "unsupported obj reloc %v/%d to %s", r.Type(), r.Siz(), ldr.SymName(r.Sym()));
                    }
                }
            }

            s = s__prev1;
        }

        sect.Rellen = uint64(ctxt.Out.Offset()) - sect.Reloff;
        const nint relocLen = 4 + 4 + 2;

        return int(sect.Rellen / relocLen);
    };

    {
        var s__prev1 = s;

        foreach (var (_, __s) in sects) {
            s = __s;
            s.peSect.emitRelocations(ctxt.Out, () => {
                nint n = default;
                {
                    var sect__prev2 = sect;

                    foreach (var (_, __sect) in s.seg.Sections) {
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

        for (nint i = 0; i < len(Segdwarf.Sections); i++) {
            var sect = Segdwarf.Sections[i];
            var si = dwarfp[i];
            if (si.secSym() != loader.Sym(sect.Sym) || ldr.SymSect(si.secSym()) != sect) {
                panic("inconsistency between dwarfp and Segdwarf");
            }
            foreach (var (_, pesect) in f.sections) {
                if (sect.Name == pesect.name) {
                    pesect.emitRelocations(ctxt.Out, () => relocsect(sect, si.syms, sect.Vaddr));
                    _continuedwarfLoop = true;
                    break;
                }
            }
            Errorf(null, "emitRelocations: could not find %q section", sect.Name);
        }

        i = i__prev1;
    }
    if (f.ctorsSect == null) {
        return ;
    }
    f.ctorsSect.emitRelocations(ctxt.Out, () => {
        var dottext = ldr.Lookup(".text", 0);
        ctxt.Out.Write32(0);
        ctxt.Out.Write32(uint32(ldr.SymDynid(dottext)));
        switch (buildcfg.GOARCH) {
            case "386": 
                ctxt.Out.Write16(IMAGE_REL_I386_DIR32);
                break;
            case "amd64": 
                ctxt.Out.Write16(IMAGE_REL_AMD64_ADDR64);
                break;
            case "arm": 
                ctxt.Out.Write16(IMAGE_REL_ARM_ADDR32);
                break;
            case "arm64": 
                ctxt.Out.Write16(IMAGE_REL_ARM64_ADDR64);
                break;
            default: 
                ctxt.Errorf(dottext, "unknown architecture for PE: %q\n", buildcfg.GOARCH);
                break;
        }
        return 1;
    });
});

// writeSymbol appends symbol s to file f symbol table.
// It also sets s.Dynid to written symbol number.
private static void writeSymbol(this ptr<peFile> _addr_f, ptr<OutBuf> _addr_@out, ptr<loader.Loader> _addr_ldr, loader.Sym s, @string name, long value, nint sectidx, ushort typ, byte @class) {
    ref peFile f = ref _addr_f.val;
    ref OutBuf @out = ref _addr_@out.val;
    ref loader.Loader ldr = ref _addr_ldr.val;

    if (len(name) > 8) {
        @out.Write32(0);
        @out.Write32(uint32(f.stringTable.add(name)));
    }
    else
 {
        @out.WriteStringN(name, 8);
    }
    @out.Write32(uint32(value));
    @out.Write16(uint16(sectidx));
    @out.Write16(typ);
    @out.Write8(class);
    @out.Write8(0); // no aux entries

    ldr.SetSymDynid(s, int32(f.symbolCount));

    f.symbolCount++;
}

// mapToPESection searches peFile f for s symbol's location.
// It returns PE section index, and offset within that section.
private static (nint, long, error) mapToPESection(this ptr<peFile> _addr_f, ptr<loader.Loader> _addr_ldr, loader.Sym s, LinkMode linkmode) {
    nint pesectidx = default;
    long offset = default;
    error err = default!;
    ref peFile f = ref _addr_f.val;
    ref loader.Loader ldr = ref _addr_ldr.val;

    var sect = ldr.SymSect(s);
    if (sect == null) {
        return (0, 0, error.As(fmt.Errorf("could not map %s symbol with no section", ldr.SymName(s)))!);
    }
    if (sect.Seg == _addr_Segtext) {
        return (f.textSect.index, int64(uint64(ldr.SymValue(s)) - Segtext.Vaddr), error.As(null!)!);
    }
    if (sect.Seg == _addr_Segrodata) {
        return (f.rdataSect.index, int64(uint64(ldr.SymValue(s)) - Segrodata.Vaddr), error.As(null!)!);
    }
    if (sect.Seg != _addr_Segdata) {
        return (0, 0, error.As(fmt.Errorf("could not map %s symbol with non .text or .rdata or .data section", ldr.SymName(s)))!);
    }
    var v = uint64(ldr.SymValue(s)) - Segdata.Vaddr;
    if (linkmode != LinkExternal) {
        return (f.dataSect.index, int64(v), error.As(null!)!);
    }
    if (ldr.SymType(s) == sym.SDATA) {
        return (f.dataSect.index, int64(v), error.As(null!)!);
    }
    if (v < Segdata.Filelen) {
        return (f.dataSect.index, int64(v), error.As(null!)!);
    }
    return (f.bssSect.index, int64(v - Segdata.Filelen), error.As(null!)!);
}

private static var isLabel = make_map<loader.Sym, bool>();

public static void AddPELabelSym(ptr<loader.Loader> _addr_ldr, loader.Sym s) {
    ref loader.Loader ldr = ref _addr_ldr.val;

    isLabel[s] = true;
}

// writeSymbols writes all COFF symbol table records.
private static void writeSymbols(this ptr<peFile> _addr_f, ptr<Link> _addr_ctxt) {
    ref peFile f = ref _addr_f.val;
    ref Link ctxt = ref _addr_ctxt.val;

    var ldr = ctxt.loader;
    Action<loader.Sym> addsym = s => {
        var t = ldr.SymType(s);
        if (ldr.SymSect(s) == null && t != sym.SDYNIMPORT && t != sym.SHOSTOBJ && t != sym.SUNDEFEXT) {
            return ;
        }
        var name = ldr.SymName(s); 

        // Only windows/386 requires underscore prefix on external symbols.
        if (ctxt.Is386() && ctxt.IsExternal() && (t == sym.SHOSTOBJ || t == sym.SUNDEFEXT || ldr.AttrCgoExport(s))) {
            name = "_" + name;
        }
        name = mangleABIName(ctxt, ldr, s, name);

        ushort peSymType = default;
        if (ctxt.IsExternal()) {
            peSymType = IMAGE_SYM_TYPE_NULL;
        }
        else
 { 
            // TODO: fix IMAGE_SYM_DTYPE_ARRAY value and use following expression, instead of 0x0308
            // peSymType = IMAGE_SYM_DTYPE_ARRAY<<8 + IMAGE_SYM_TYPE_STRUCT
            peSymType = 0x0308; // "array of structs"
        }
        var (sect, value, err) = f.mapToPESection(ldr, s, ctxt.LinkMode);
        if (err != null) {
            if (t == sym.SDYNIMPORT || t == sym.SHOSTOBJ || t == sym.SUNDEFEXT) {
                peSymType = IMAGE_SYM_DTYPE_FUNCTION;
            }
            else
 {
                ctxt.Errorf(s, "addpesym: %v", err);
            }
        }
        var @class = IMAGE_SYM_CLASS_EXTERNAL;
        if (ldr.IsFileLocal(s) || ldr.AttrVisibilityHidden(s) || ldr.AttrLocal(s)) {
            class = IMAGE_SYM_CLASS_STATIC;
        }
        f.writeSymbol(ctxt.Out, ldr, s, name, value, sect, peSymType, uint8(class));
    };

    if (ctxt.LinkMode == LinkExternal) { 
        // Include section symbols as external, because
        // .ctors and .debug_* section relocations refer to it.
        foreach (var (_, pesect) in f.sections) {
            var s = ldr.LookupOrCreateSym(pesect.name, 0);
            f.writeSymbol(ctxt.Out, ldr, s, pesect.name, 0, pesect.index, IMAGE_SYM_TYPE_NULL, IMAGE_SYM_CLASS_STATIC);
        }
    }
    s = ldr.Lookup("runtime.text", 0);
    if (ldr.SymType(s) == sym.STEXT) {
        addsym(s);
    }
    s = ldr.Lookup("runtime.etext", 0);
    if (ldr.SymType(s) == sym.STEXT) {
        addsym(s);
    }
    {
        var s__prev1 = s;

        foreach (var (_, __s) in ctxt.Textp) {
            s = __s;
            addsym(s);
        }
        s = s__prev1;
    }

    Func<loader.Sym, bool> shouldBeInSymbolTable = s => {
        if (ldr.AttrNotInSymbolTable(s)) {
            return false;
        }
        name = ldr.RawSymName(s); // TODO: try not to read the name
        if (name == "" || name[0] == '.') {
            return false;
        }
        return true;
    }; 

    // Add data symbols and external references.
    {
        var s__prev1 = s;

        for (s = loader.Sym(1); s < loader.Sym(ldr.NSym()); s++) {
            if (!ldr.AttrReachable(s)) {
                continue;
            }
            t = ldr.SymType(s);
            if (t >= sym.SELFRXSECT && t < sym.SXREF) { // data sections handled in dodata
                if (t == sym.STLSBSS) {
                    continue;
                }
                if (!shouldBeInSymbolTable(s)) {
                    continue;
                }
                addsym(s);
            }

            if (t == sym.SDYNIMPORT || t == sym.SHOSTOBJ || t == sym.SUNDEFEXT) 
                addsym(s);
            else 
                if (len(isLabel) > 0 && isLabel[s]) {
                    addsym(s);
                }
                    }

        s = s__prev1;
    }
}

// writeSymbolTableAndStringTable writes out symbol and string tables for peFile f.
private static void writeSymbolTableAndStringTable(this ptr<peFile> _addr_f, ptr<Link> _addr_ctxt) {
    ref peFile f = ref _addr_f.val;
    ref Link ctxt = ref _addr_ctxt.val;

    f.symtabOffset = ctxt.Out.Offset(); 

    // write COFF symbol table
    if (!FlagS || ctxt.LinkMode == LinkExternal.val) {
        f.writeSymbols(ctxt);
    }
    var size = f.stringTable.size() + 18 * f.symbolCount;
    ptr<peSection> h;
    if (ctxt.LinkMode != LinkExternal) { 
        // We do not really need .symtab for go.o, and if we have one, ld
        // will also include it in the exe, and that will confuse windows.
        h = f.addSection(".symtab", size, size);
        h.characteristics = IMAGE_SCN_MEM_READ | IMAGE_SCN_MEM_DISCARDABLE;
        h.checkOffset(f.symtabOffset);
    }
    f.stringTable.write(ctxt.Out);
    if (ctxt.LinkMode != LinkExternal) {
        h.pad(ctxt.Out, uint32(size));
    }
}

// writeFileHeader writes COFF file header for peFile f.
private static void writeFileHeader(this ptr<peFile> _addr_f, ptr<Link> _addr_ctxt) {
    ref peFile f = ref _addr_f.val;
    ref Link ctxt = ref _addr_ctxt.val;

    ref pe.FileHeader fh = ref heap(out ptr<pe.FileHeader> _addr_fh);


    if (ctxt.Arch.Family == sys.AMD64) 
        fh.Machine = pe.IMAGE_FILE_MACHINE_AMD64;
    else if (ctxt.Arch.Family == sys.I386) 
        fh.Machine = pe.IMAGE_FILE_MACHINE_I386;
    else if (ctxt.Arch.Family == sys.ARM) 
        fh.Machine = pe.IMAGE_FILE_MACHINE_ARMNT;
    else if (ctxt.Arch.Family == sys.ARM64) 
        fh.Machine = pe.IMAGE_FILE_MACHINE_ARM64;
    else 
        Exitf("unknown PE architecture: %v", ctxt.Arch.Family);
        fh.NumberOfSections = uint16(len(f.sections)); 

    // Being able to produce identical output for identical input is
    // much more beneficial than having build timestamp in the header.
    fh.TimeDateStamp = 0;

    if (ctxt.LinkMode == LinkExternal) {
        fh.Characteristics = pe.IMAGE_FILE_LINE_NUMS_STRIPPED;
    }
    else
 {
        fh.Characteristics = pe.IMAGE_FILE_EXECUTABLE_IMAGE | pe.IMAGE_FILE_DEBUG_STRIPPED;

        if (ctxt.Arch.Family == sys.AMD64 || ctxt.Arch.Family == sys.I386) 
            if (ctxt.BuildMode != BuildModePIE) {
                fh.Characteristics |= pe.IMAGE_FILE_RELOCS_STRIPPED;
            }
            }
    if (pe64 != 0) {
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
private static void writeOptionalHeader(this ptr<peFile> _addr_f, ptr<Link> _addr_ctxt) {
    ref peFile f = ref _addr_f.val;
    ref Link ctxt = ref _addr_ctxt.val;

    ref pe.OptionalHeader32 oh = ref heap(out ptr<pe.OptionalHeader32> _addr_oh);
    ref pe.OptionalHeader64 oh64 = ref heap(out ptr<pe.OptionalHeader64> _addr_oh64);

    if (pe64 != 0) {
        oh64.Magic = 0x20b; // PE32+
    }
    else
 {
        oh.Magic = 0x10b; // PE32
        oh.BaseOfData = f.dataSect.virtualAddress;
    }
    oh64.MajorLinkerVersion = 3;
    oh.MajorLinkerVersion = 3;
    oh64.MinorLinkerVersion = 0;
    oh.MinorLinkerVersion = 0;
    oh64.SizeOfCode = f.textSect.sizeOfRawData;
    oh.SizeOfCode = f.textSect.sizeOfRawData;
    oh64.SizeOfInitializedData = f.dataSect.sizeOfRawData;
    oh.SizeOfInitializedData = f.dataSect.sizeOfRawData;
    oh64.SizeOfUninitializedData = 0;
    oh.SizeOfUninitializedData = 0;
    if (ctxt.LinkMode != LinkExternal) {
        oh64.AddressOfEntryPoint = uint32(Entryvalue(ctxt) - PEBASE);
        oh.AddressOfEntryPoint = uint32(Entryvalue(ctxt) - PEBASE);
    }
    oh64.BaseOfCode = f.textSect.virtualAddress;
    oh.BaseOfCode = f.textSect.virtualAddress;
    oh64.ImageBase = uint64(PEBASE);
    oh.ImageBase = uint32(PEBASE);
    oh64.SectionAlignment = uint32(PESECTALIGN);
    oh.SectionAlignment = uint32(PESECTALIGN);
    oh64.FileAlignment = uint32(PEFILEALIGN);
    oh.FileAlignment = uint32(PEFILEALIGN);
    oh64.MajorOperatingSystemVersion = PeMinimumTargetMajorVersion;
    oh.MajorOperatingSystemVersion = PeMinimumTargetMajorVersion;
    oh64.MinorOperatingSystemVersion = PeMinimumTargetMinorVersion;
    oh.MinorOperatingSystemVersion = PeMinimumTargetMinorVersion;
    oh64.MajorImageVersion = 1;
    oh.MajorImageVersion = 1;
    oh64.MinorImageVersion = 0;
    oh.MinorImageVersion = 0;
    oh64.MajorSubsystemVersion = PeMinimumTargetMajorVersion;
    oh.MajorSubsystemVersion = PeMinimumTargetMajorVersion;
    oh64.MinorSubsystemVersion = PeMinimumTargetMinorVersion;
    oh.MinorSubsystemVersion = PeMinimumTargetMinorVersion;
    oh64.SizeOfImage = f.nextSectOffset;
    oh.SizeOfImage = f.nextSectOffset;
    oh64.SizeOfHeaders = uint32(PEFILEHEADR);
    oh.SizeOfHeaders = uint32(PEFILEHEADR);
    if (windowsgui) {
        oh64.Subsystem = pe.IMAGE_SUBSYSTEM_WINDOWS_GUI;
        oh.Subsystem = pe.IMAGE_SUBSYSTEM_WINDOWS_GUI;
    }
    else
 {
        oh64.Subsystem = pe.IMAGE_SUBSYSTEM_WINDOWS_CUI;
        oh.Subsystem = pe.IMAGE_SUBSYSTEM_WINDOWS_CUI;
    }
    oh64.DllCharacteristics |= pe.IMAGE_DLLCHARACTERISTICS_TERMINAL_SERVER_AWARE;
    oh.DllCharacteristics |= pe.IMAGE_DLLCHARACTERISTICS_TERMINAL_SERVER_AWARE; 

    // Enable DEP
    oh64.DllCharacteristics |= pe.IMAGE_DLLCHARACTERISTICS_NX_COMPAT;
    oh.DllCharacteristics |= pe.IMAGE_DLLCHARACTERISTICS_NX_COMPAT; 

    // The DLL can be relocated at load time.
    if (needPEBaseReloc(_addr_ctxt)) {
        oh64.DllCharacteristics |= pe.IMAGE_DLLCHARACTERISTICS_DYNAMIC_BASE;
        oh.DllCharacteristics |= pe.IMAGE_DLLCHARACTERISTICS_DYNAMIC_BASE;
    }
    if (ctxt.BuildMode == BuildModePIE) {
        oh64.DllCharacteristics |= pe.IMAGE_DLLCHARACTERISTICS_HIGH_ENTROPY_VA;
    }
    oh64.SizeOfStackReserve = 0x00200000;
    if (!iscgo) {
        oh64.SizeOfStackCommit = 0x00001000;
    }
    else
 { 
        // TODO(brainman): Maybe remove optional header writing altogether for cgo.
        // For cgo it is the external linker that is building final executable.
        // And it probably does not use any information stored in optional header.
        oh64.SizeOfStackCommit = 0x00200000 - 0x2000; // account for 2 guard pages
    }
    oh.SizeOfStackReserve = 0x00100000;
    if (!iscgo) {
        oh.SizeOfStackCommit = 0x00001000;
    }
    else
 {
        oh.SizeOfStackCommit = 0x00100000 - 0x2000; // account for 2 guard pages
    }
    oh64.SizeOfHeapReserve = 0x00100000;
    oh.SizeOfHeapReserve = 0x00100000;
    oh64.SizeOfHeapCommit = 0x00001000;
    oh.SizeOfHeapCommit = 0x00001000;
    oh64.NumberOfRvaAndSizes = 16;
    oh.NumberOfRvaAndSizes = 16;

    if (pe64 != 0) {
        oh64.DataDirectory = f.dataDirectory;
    }
    else
 {
        oh.DataDirectory = f.dataDirectory;
    }
    if (pe64 != 0) {
        binary.Write(ctxt.Out, binary.LittleEndian, _addr_oh64);
    }
    else
 {
        binary.Write(ctxt.Out, binary.LittleEndian, _addr_oh);
    }
}

private static peFile pefile = default;

public static void Peinit(ptr<Link> _addr_ctxt) {
    ref Link ctxt = ref _addr_ctxt.val;

    nint l = default;

    if (ctxt.Arch.PtrSize == 8) { 
        // 64-bit architectures
        pe64 = 1;
        PEBASE = 1 << 32;
        if (ctxt.Arch.Family == sys.AMD64) { 
            // TODO(rsc): For cgo we currently use 32-bit relocations
            // that fail when PEBASE is too large.
            // We need to fix this, but for now, use a smaller PEBASE.
            PEBASE = 1 << 22;
        }
        ref pe.OptionalHeader64 oh64 = ref heap(out ptr<pe.OptionalHeader64> _addr_oh64);
        l = binary.Size(_addr_oh64);
    }
    else
 { 
        // 32-bit architectures
        PEBASE = 1 << 22;
        ref pe.OptionalHeader32 oh = ref heap(out ptr<pe.OptionalHeader32> _addr_oh);
        l = binary.Size(_addr_oh);
    }
    if (ctxt.LinkMode == LinkExternal) { 
        // .rdata section will contain "masks" and "shifts" symbols, and they
        // need to be aligned to 16-bytes. So make all sections aligned
        // to 32-byte and mark them all IMAGE_SCN_ALIGN_32BYTES so external
        // linker will honour that requirement.
        PESECTALIGN = 32;
        PEFILEALIGN = 0;
    }
    ref array<pe.SectionHeader32> sh = ref heap(new array<pe.SectionHeader32>(16), out ptr<array<pe.SectionHeader32>> _addr_sh);
    ref pe.FileHeader fh = ref heap(out ptr<pe.FileHeader> _addr_fh);
    PEFILEHEADR = int32(Rnd(int64(len(dosstub) + binary.Size(_addr_fh) + l + binary.Size(_addr_sh)), PEFILEALIGN));
    if (ctxt.LinkMode != LinkExternal) {
        PESECTHEADR = int32(Rnd(int64(PEFILEHEADR), PESECTALIGN));
    }
    else
 {
        PESECTHEADR = 0;
    }
    pefile.nextSectOffset = uint32(PESECTHEADR);
    pefile.nextFileOffset = uint32(PEFILEHEADR);

    if (ctxt.LinkMode == LinkInternal) { 
        // some mingw libs depend on this symbol, for example, FindPESectionByName
        foreach (var (_, name) in new array<@string>(new @string[] { "__image_base__", "_image_base__" })) {
            var sb = ctxt.loader.CreateSymForUpdate(name, 0);
            sb.SetType(sym.SDATA);
            sb.SetValue(PEBASE);
            ctxt.loader.SetAttrSpecial(sb.Sym(), true);
            ctxt.loader.SetAttrLocal(sb.Sym(), true);
        }
    }
    HEADR = PEFILEHEADR;
    if (FlagTextAddr == -1.val) {
        FlagTextAddr.val = PEBASE + int64(PESECTHEADR);
    }
    if (FlagRound == -1.val) {
        FlagRound.val = int(PESECTALIGN);
    }
}

private static void pewrite(ptr<Link> _addr_ctxt) {
    ref Link ctxt = ref _addr_ctxt.val;

    ctxt.Out.SeekSet(0);
    if (ctxt.LinkMode != LinkExternal) {
        ctxt.Out.Write(dosstub);
        ctxt.Out.WriteStringN("PE", 4);
    }
    pefile.writeFileHeader(ctxt);

    pefile.writeOptionalHeader(ctxt);

    foreach (var (_, sect) in pefile.sections) {
        sect.write(ctxt.Out, ctxt.LinkMode);
    }
}

private static void strput(ptr<OutBuf> _addr_@out, @string s) {
    ref OutBuf @out = ref _addr_@out.val;

    @out.WriteString(s);
    @out.Write8(0); 
    // string must be padded to even size
    if ((len(s) + 1) % 2 != 0) {
        @out.Write8(0);
    }
}

private static ptr<Dll> initdynimport(ptr<Link> _addr_ctxt) {
    ref Link ctxt = ref _addr_ctxt.val;

    var ldr = ctxt.loader;
    ptr<Dll> d;

    dr = null;
    ptr<Imp> m;
    for (var s = loader.Sym(1); s < loader.Sym(ldr.NSym()); s++) {
        if (!ldr.AttrReachable(s) || ldr.SymType(s) != sym.SDYNIMPORT) {
            continue;
        }
        var dynlib = ldr.SymDynimplib(s);
        d = dr;

        while (d != null) {
            if (d.name == dynlib) {
                m = @new<Imp>();
                break;
            d = d.next;
            }
        }

        if (d == null) {
            d = @new<Dll>();
            d.name = dynlib;
            d.next = dr;
            dr = d;
            m = @new<Imp>();
        }
        m.argsize = -1;
        var extName = ldr.SymExtname(s);
        {
            var i = strings.IndexByte(extName, '%');

            if (i >= 0) {
                error err = default!;
                m.argsize, err = strconv.Atoi(extName[(int)i + 1..]);
                if (err != null) {
                    ctxt.Errorf(s, "failed to parse stdcall decoration: %v", err);
                }
                m.argsize *= ctxt.Arch.PtrSize;
                ldr.SetSymExtname(s, extName[..(int)i]);
            }

        }

        m.s = s;
        m.next = d.ms;
        d.ms = m;
    }

    if (ctxt.IsExternal()) { 
        // Add real symbol name
        {
            ptr<Dll> d__prev1 = d;

            d = dr;

            while (d != null) {
                m = d.ms;

                while (m != null) {
                    var sb = ldr.MakeSymbolUpdater(m.s);
                    sb.SetType(sym.SDATA);
                    sb.Grow(int64(ctxt.Arch.PtrSize));
                    var dynName = sb.Extname(); 
                    // only windows/386 requires stdcall decoration
                    if (ctxt.Is386() && m.argsize >= 0) {
                        dynName += fmt.Sprintf("@%d", m.argsize);
                    m = m.next;
                    }
                    var dynSym = ldr.CreateSymForUpdate(dynName, 0);
                    dynSym.SetType(sym.SHOSTOBJ);
                    var (r, _) = sb.AddRel(objabi.R_ADDR);
                    r.SetSym(dynSym.Sym());
                    r.SetSiz(uint8(ctxt.Arch.PtrSize));
                d = d.next;
                }
    else
            }


            d = d__prev1;
        }
    } {
        var dynamic = ldr.CreateSymForUpdate(".windynamic", 0);
        dynamic.SetType(sym.SWINDOWS);
        {
            ptr<Dll> d__prev1 = d;

            d = dr;

            while (d != null) {
                m = d.ms;

                while (m != null) {
                    sb = ldr.MakeSymbolUpdater(m.s);
                    sb.SetType(sym.SWINDOWS);
                    sb.SetValue(dynamic.Size());
                    dynamic.SetSize(dynamic.Size() + int64(ctxt.Arch.PtrSize));
                    dynamic.AddInteriorSym(m.s);
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
private static slice<@string> peimporteddlls() {
    slice<@string> dlls = default;

    {
        var d = dr;

        while (d != null) {
            dlls = append(dlls, "-l" + strings.TrimSuffix(d.name, ".dll"));
            d = d.next;
        }
    }

    return dlls;
}

private static void addimports(ptr<Link> _addr_ctxt, ptr<peSection> _addr_datsect) {
    ref Link ctxt = ref _addr_ctxt.val;
    ref peSection datsect = ref _addr_datsect.val;

    var ldr = ctxt.loader;
    var startoff = ctxt.Out.Offset();
    var dynamic = ldr.LookupOrCreateSym(".windynamic", 0); 

    // skip import descriptor table (will write it later)
    var n = uint64(0);

    {
        var d__prev1 = d;

        var d = dr;

        while (d != null) {
            n++;
            d = d.next;
        }

        d = d__prev1;
    }
    ctxt.Out.SeekSet(startoff + int64(binary.Size(addr(new IMAGE_IMPORT_DESCRIPTOR()))) * int64(n + 1)); 

    // write dll names
    {
        var d__prev1 = d;

        d = dr;

        while (d != null) {
            d.nameoff = uint64(ctxt.Out.Offset()) - uint64(startoff);
            strput(_addr_ctxt.Out, d.name);
            d = d.next;
        }

        d = d__prev1;
    } 

    // write function names
    {
        var d__prev1 = d;

        d = dr;

        while (d != null) {
            {
                var m__prev2 = m;

                var m = d.ms;

                while (m != null) {
                    m.off = uint64(pefile.nextSectOffset) + uint64(ctxt.Out.Offset()) - uint64(startoff);
                    ctxt.Out.Write16(0); // hint
                    strput(_addr_ctxt.Out, ldr.SymExtname(m.s));
                    m = m.next;
                }


                m = m__prev2;
            }
            d = d.next;
        }

        d = d__prev1;
    } 

    // write OriginalFirstThunks
    var oftbase = uint64(ctxt.Out.Offset()) - uint64(startoff);

    n = uint64(ctxt.Out.Offset());
    {
        var d__prev1 = d;

        d = dr;

        while (d != null) {
            d.thunkoff = uint64(ctxt.Out.Offset()) - n;
            {
                var m__prev2 = m;

                m = d.ms;

                while (m != null) {
                    if (pe64 != 0) {
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

            if (pe64 != 0) {
                ctxt.Out.Write64(0);
            }
            else
 {
                ctxt.Out.Write32(0);
            }
        }

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
    var ftbase = uint64(ldr.SymValue(dynamic)) - uint64(datsect.virtualAddress) - uint64(PEBASE);

    ctxt.Out.SeekSet(int64(uint64(datsect.pointerToRawData) + ftbase));
    {
        var d__prev1 = d;

        d = dr;

        while (d != null) {
            {
                var m__prev2 = m;

                m = d.ms;

                while (m != null) {
                    if (pe64 != 0) {
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

            if (pe64 != 0) {
                ctxt.Out.Write64(0);
            }
            else
 {
                ctxt.Out.Write32(0);
            }
        }

        d = d__prev1;
    } 

    // finally write import descriptor table
    var @out = ctxt.Out;
    @out.SeekSet(startoff);

    {
        var d__prev1 = d;

        d = dr;

        while (d != null) {
            @out.Write32(uint32(uint64(isect.virtualAddress) + oftbase + d.thunkoff));
            @out.Write32(0);
            @out.Write32(0);
            @out.Write32(uint32(uint64(isect.virtualAddress) + d.nameoff));
            @out.Write32(uint32(uint64(datsect.virtualAddress) + ftbase + d.thunkoff));
            d = d.next;
        }

        d = d__prev1;
    }

    @out.Write32(0); //end
    @out.Write32(0);
    @out.Write32(0);
    @out.Write32(0);
    @out.Write32(0); 

    // update data directory
    pefile.dataDirectory[pe.IMAGE_DIRECTORY_ENTRY_IMPORT].VirtualAddress = isect.virtualAddress;
    pefile.dataDirectory[pe.IMAGE_DIRECTORY_ENTRY_IMPORT].Size = isect.virtualSize;
    pefile.dataDirectory[pe.IMAGE_DIRECTORY_ENTRY_IAT].VirtualAddress = uint32(ldr.SymValue(dynamic) - PEBASE);
    pefile.dataDirectory[pe.IMAGE_DIRECTORY_ENTRY_IAT].Size = uint32(ldr.SymSize(dynamic));

    @out.SeekSet(endoff);
}

private static void initdynexport(ptr<Link> _addr_ctxt) {
    ref Link ctxt = ref _addr_ctxt.val;

    var ldr = ctxt.loader;
    for (var s = loader.Sym(1); s < loader.Sym(ldr.NSym()); s++) {
        if (!ldr.AttrReachable(s) || !ldr.AttrCgoExportDynamic(s)) {
            continue;
        }
        if (len(dexport) + 1 > cap(dexport)) {
            ctxt.Errorf(s, "pe dynexport table is full");
            errorexit();
        }
        dexport = append(dexport, s);
    }

    sort.Slice(dexport, (i, j) => ldr.SymExtname(dexport[i]) < ldr.SymExtname(dexport[j]));
}

private static void addexports(ptr<Link> _addr_ctxt) {
    ref Link ctxt = ref _addr_ctxt.val;

    var ldr = ctxt.loader;
    ref IMAGE_EXPORT_DIRECTORY e = ref heap(out ptr<IMAGE_EXPORT_DIRECTORY> _addr_e);

    var nexport = len(dexport);
    var size = binary.Size(_addr_e) + 10 * nexport + len(flagOutfile.val) + 1;
    {
        var s__prev1 = s;

        foreach (var (_, __s) in dexport) {
            s = __s;
            size += len(ldr.SymExtname(s)) + 1;
        }
        s = s__prev1;
    }

    if (nexport == 0) {
        return ;
    }
    var sect = pefile.addSection(".edata", size, size);
    sect.characteristics = IMAGE_SCN_CNT_INITIALIZED_DATA | IMAGE_SCN_MEM_READ;
    sect.checkOffset(ctxt.Out.Offset());
    var va = int(sect.virtualAddress);
    pefile.dataDirectory[pe.IMAGE_DIRECTORY_ENTRY_EXPORT].VirtualAddress = uint32(va);
    pefile.dataDirectory[pe.IMAGE_DIRECTORY_ENTRY_EXPORT].Size = sect.virtualSize;

    var vaName = va + binary.Size(_addr_e) + nexport * 4;
    var vaAddr = va + binary.Size(_addr_e);
    var vaNa = va + binary.Size(_addr_e) + nexport * 8;

    e.Characteristics = 0;
    e.MajorVersion = 0;
    e.MinorVersion = 0;
    e.NumberOfFunctions = uint32(nexport);
    e.NumberOfNames = uint32(nexport);
    e.Name = uint32(va + binary.Size(_addr_e)) + uint32(nexport) * 10; // Program names.
    e.Base = 1;
    e.AddressOfFunctions = uint32(vaAddr);
    e.AddressOfNames = uint32(vaName);
    e.AddressOfNameOrdinals = uint32(vaNa);

    var @out = ctxt.Out; 

    // put IMAGE_EXPORT_DIRECTORY
    binary.Write(out, binary.LittleEndian, _addr_e); 

    // put EXPORT Address Table
    {
        var s__prev1 = s;

        foreach (var (_, __s) in dexport) {
            s = __s;
            @out.Write32(uint32(ldr.SymValue(s) - PEBASE));
        }
        s = s__prev1;
    }

    var v = int(e.Name + uint32(len(flagOutfile.val)) + 1);

    {
        var s__prev1 = s;

        foreach (var (_, __s) in dexport) {
            s = __s;
            @out.Write32(uint32(v));
            v += len(ldr.SymExtname(s)) + 1;
        }
        s = s__prev1;
    }

    for (nint i = 0; i < nexport; i++) {
        @out.Write16(uint16(i));
    } 

    // put Names
    @out.WriteStringN(flagOutfile.val, len(flagOutfile.val) + 1);

    {
        var s__prev1 = s;

        foreach (var (_, __s) in dexport) {
            s = __s;
            var name = ldr.SymExtname(s);
            @out.WriteStringN(name, len(name) + 1);
        }
        s = s__prev1;
    }

    sect.pad(out, uint32(size));
}

// peBaseRelocEntry represents a single relocation entry.
private partial struct peBaseRelocEntry {
    public ushort typeOff;
}

// peBaseRelocBlock represents a Base Relocation Block. A block
// is a collection of relocation entries in a page, where each
// entry describes a single relocation.
// The block page RVA (Relative Virtual Address) is the index
// into peBaseRelocTable.blocks.
private partial struct peBaseRelocBlock {
    public slice<peBaseRelocEntry> entries;
}

// pePages is a type used to store the list of pages for which there
// are base relocation blocks. This is defined as a type so that
// it can be sorted.
private partial struct pePages { // : slice<uint>
}

private static nint Len(this pePages p) {
    return len(p);
}
private static void Swap(this pePages p, nint i, nint j) {
    (p[i], p[j]) = (p[j], p[i]);
}
private static bool Less(this pePages p, nint i, nint j) {
    return p[i] < p[j];
}

// A PE base relocation table is a list of blocks, where each block
// contains relocation information for a single page. The blocks
// must be emitted in order of page virtual address.
// See https://docs.microsoft.com/en-us/windows/desktop/debug/pe-format#the-reloc-section-image-only
private partial struct peBaseRelocTable {
    public map<uint, peBaseRelocBlock> blocks; // pePages is a list of keys into blocks map.
// It is stored separately for ease of sorting.
    public pePages pages;
}

private static void init(this ptr<peBaseRelocTable> _addr_rt, ptr<Link> _addr_ctxt) {
    ref peBaseRelocTable rt = ref _addr_rt.val;
    ref Link ctxt = ref _addr_ctxt.val;

    rt.blocks = make_map<uint, peBaseRelocBlock>();
}

private static void addentry(this ptr<peBaseRelocTable> _addr_rt, ptr<loader.Loader> _addr_ldr, loader.Sym s, ptr<loader.Reloc> _addr_r) {
    ref peBaseRelocTable rt = ref _addr_rt.val;
    ref loader.Loader ldr = ref _addr_ldr.val;
    ref loader.Reloc r = ref _addr_r.val;
 
    // pageSize is the size in bytes of a page
    // described by a base relocation block.
    const nuint pageSize = 0x1000;

    const var pageMask = pageSize - 1;



    var addr = ldr.SymValue(s) + int64(r.Off()) - int64(PEBASE);
    var page = uint32(addr & ~pageMask);
    var off = uint32(addr & pageMask);

    var (b, ok) = rt.blocks[page];
    if (!ok) {
        rt.pages = append(rt.pages, page);
    }
    peBaseRelocEntry e = new peBaseRelocEntry(typeOff:uint16(off&0xFFF),); 

    // Set entry type
    switch (r.Siz()) {
        case 4: 
            e.typeOff |= uint16(IMAGE_REL_BASED_HIGHLOW << 12);
            break;
        case 8: 
            e.typeOff |= uint16(IMAGE_REL_BASED_DIR64 << 12);
            break;
        default: 
            Exitf("unsupported relocation size %d\n", r.Siz);
            break;
    }

    b.entries = append(b.entries, e);
    rt.blocks[page] = b;
}

private static void write(this ptr<peBaseRelocTable> _addr_rt, ptr<Link> _addr_ctxt) {
    ref peBaseRelocTable rt = ref _addr_rt.val;
    ref Link ctxt = ref _addr_ctxt.val;

    var @out = ctxt.Out; 

    // sort the pages array
    sort.Sort(rt.pages);

    foreach (var (_, p) in rt.pages) {
        var b = rt.blocks[p];
        const nint sizeOfPEbaseRelocBlock = 8; // 2 * sizeof(uint32)
 // 2 * sizeof(uint32)
        var blockSize = uint32(sizeOfPEbaseRelocBlock + len(b.entries) * 2);
        @out.Write32(p);
        @out.Write32(blockSize);

        foreach (var (_, e) in b.entries) {
            @out.Write16(e.typeOff);
        }
    }
}

private static void addPEBaseRelocSym(ptr<loader.Loader> _addr_ldr, loader.Sym s, ptr<peBaseRelocTable> _addr_rt) {
    ref loader.Loader ldr = ref _addr_ldr.val;
    ref peBaseRelocTable rt = ref _addr_rt.val;

    var relocs = ldr.Relocs(s);
    for (nint ri = 0; ri < relocs.Count(); ri++) {
        ref var r = ref heap(relocs.At(ri), out ptr<var> _addr_r);
        if (r.Type() >= objabi.ElfRelocOffset) {
            continue;
        }
        if (r.Siz() == 0) { // informational relocation
            continue;
        }
        if (r.Type() == objabi.R_DWARFFILEREF) {
            continue;
        }
        var rs = r.Sym();
        rs = ldr.ResolveABIAlias(rs);
        if (rs == 0) {
            continue;
        }
        if (!ldr.AttrReachable(s)) {
            continue;
        }

        if (r.Type() == objabi.R_ADDR) 
            rt.addentry(ldr, s, _addr_r);
        else         
    }
}

private static bool needPEBaseReloc(ptr<Link> _addr_ctxt) {
    ref Link ctxt = ref _addr_ctxt.val;
 
    // Non-PIE x86 binaries don't need the base relocation table.
    // Everyone else does.
    if ((ctxt.Arch.Family == sys.I386 || ctxt.Arch.Family == sys.AMD64) && ctxt.BuildMode != BuildModePIE) {
        return false;
    }
    return true;
}

private static void addPEBaseReloc(ptr<Link> _addr_ctxt) {
    ref Link ctxt = ref _addr_ctxt.val;

    if (!needPEBaseReloc(_addr_ctxt)) {
        return ;
    }
    ref peBaseRelocTable rt = ref heap(out ptr<peBaseRelocTable> _addr_rt);
    rt.init(ctxt); 

    // Get relocation information
    var ldr = ctxt.loader;
    {
        var s__prev1 = s;

        foreach (var (_, __s) in ctxt.Textp) {
            s = __s;
            addPEBaseRelocSym(_addr_ldr, s, _addr_rt);
        }
        s = s__prev1;
    }

    {
        var s__prev1 = s;

        foreach (var (_, __s) in ctxt.datap) {
            s = __s;
            addPEBaseRelocSym(_addr_ldr, s, _addr_rt);
        }
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

private static void dope(this ptr<Link> _addr_ctxt) {
    ref Link ctxt = ref _addr_ctxt.val;

    initdynimport(_addr_ctxt);
    initdynexport(_addr_ctxt);
}

private static void setpersrc(ptr<Link> _addr_ctxt, slice<loader.Sym> syms) {
    ref Link ctxt = ref _addr_ctxt.val;

    if (len(rsrcsyms) != 0) {
        Errorf(null, "too many .rsrc sections");
    }
    rsrcsyms = syms;
}

private static void addpersrc(ptr<Link> _addr_ctxt) {
    ref Link ctxt = ref _addr_ctxt.val;

    if (len(rsrcsyms) == 0) {
        return ;
    }
    long size = default;
    {
        var rsrcsym__prev1 = rsrcsym;

        foreach (var (_, __rsrcsym) in rsrcsyms) {
            rsrcsym = __rsrcsym;
            size += ctxt.loader.SymSize(rsrcsym);
        }
        rsrcsym = rsrcsym__prev1;
    }

    var h = pefile.addSection(".rsrc", int(size), int(size));
    h.characteristics = IMAGE_SCN_MEM_READ | IMAGE_SCN_CNT_INITIALIZED_DATA;
    h.checkOffset(ctxt.Out.Offset());

    {
        var rsrcsym__prev1 = rsrcsym;

        foreach (var (_, __rsrcsym) in rsrcsyms) {
            rsrcsym = __rsrcsym; 
            // A split resource happens when the actual resource data and its relocations are
            // split across multiple sections, denoted by a $01 or $02 at the end of the .rsrc
            // section name.
            var splitResources = strings.Contains(ctxt.loader.SymName(rsrcsym), ".rsrc$");
            var relocs = ctxt.loader.Relocs(rsrcsym);
            var data = ctxt.loader.Data(rsrcsym);
            for (nint ri = 0; ri < relocs.Count(); ri++) {
                var r = relocs.At(ri);
                var p = data[(int)r.Off()..];
                var val = uint32(int64(h.virtualAddress) + r.Add());
                if (splitResources) { 
                    // If we're a split resource section, and that section has relocation
                    // symbols, then the data that it points to doesn't actually begin at
                    // the virtual address listed in this current section, but rather
                    // begins at the section immediately after this one. So, in order to
                    // calculate the proper virtual address of the data it's pointing to,
                    // we have to add the length of this section to the virtual address.
                    // This works because .rsrc sections are divided into two (but not more)
                    // of these sections.
                    val += uint32(len(data));
                }
                binary.LittleEndian.PutUint32(p, val);
            }

            ctxt.Out.Write(data);
        }
        rsrcsym = rsrcsym__prev1;
    }

    h.pad(ctxt.Out, uint32(size)); 

    // update data directory
    pefile.dataDirectory[pe.IMAGE_DIRECTORY_ENTRY_RESOURCE].VirtualAddress = h.virtualAddress;
    pefile.dataDirectory[pe.IMAGE_DIRECTORY_ENTRY_RESOURCE].Size = h.virtualSize;
}

private static void asmbPe(ptr<Link> _addr_ctxt) {
    ref Link ctxt = ref _addr_ctxt.val;

    var t = pefile.addSection(".text", int(Segtext.Length), int(Segtext.Length));
    t.characteristics = IMAGE_SCN_CNT_CODE | IMAGE_SCN_MEM_EXECUTE | IMAGE_SCN_MEM_READ;
    if (ctxt.LinkMode == LinkExternal) { 
        // some data symbols (e.g. masks) end up in the .text section, and they normally
        // expect larger alignment requirement than the default text section alignment.
        t.characteristics |= IMAGE_SCN_ALIGN_32BYTES;
    }
    t.checkSegment(_addr_Segtext);
    pefile.textSect = t;

    var ro = pefile.addSection(".rdata", int(Segrodata.Length), int(Segrodata.Length));
    ro.characteristics = IMAGE_SCN_CNT_INITIALIZED_DATA | IMAGE_SCN_MEM_READ;
    if (ctxt.LinkMode == LinkExternal) { 
        // some data symbols (e.g. masks) end up in the .rdata section, and they normally
        // expect larger alignment requirement than the default text section alignment.
        ro.characteristics |= IMAGE_SCN_ALIGN_32BYTES;
    }
    ro.checkSegment(_addr_Segrodata);
    pefile.rdataSect = ro;

    ptr<peSection> d;
    if (ctxt.LinkMode != LinkExternal) {
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

        var b = pefile.addSection(".bss", int(Segdata.Length - Segdata.Filelen), 0);
        b.characteristics = IMAGE_SCN_CNT_UNINITIALIZED_DATA | IMAGE_SCN_MEM_READ | IMAGE_SCN_MEM_WRITE | IMAGE_SCN_ALIGN_32BYTES;
        b.pointerToRawData = 0;
        pefile.bssSect = b;
    }
    pefile.addDWARF();

    if (ctxt.LinkMode == LinkExternal) {
        pefile.ctorsSect = pefile.addInitArray(ctxt);
    }
    ctxt.Out.SeekSet(int64(pefile.nextFileOffset));
    if (ctxt.LinkMode != LinkExternal) {
        addimports(_addr_ctxt, d);
        addexports(_addr_ctxt);
        addPEBaseReloc(_addr_ctxt);
    }
    pefile.writeSymbolTableAndStringTable(ctxt);
    addpersrc(_addr_ctxt);
    if (ctxt.LinkMode == LinkExternal) {
        pefile.emitRelocations(ctxt);
    }
    pewrite(_addr_ctxt);
}

} // end ld_package
