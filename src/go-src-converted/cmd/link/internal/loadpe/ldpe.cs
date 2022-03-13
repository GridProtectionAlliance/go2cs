// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package loadpe implements a PE/COFF file reader.

// package loadpe -- go2cs converted at 2022 March 13 06:34:47 UTC
// import "cmd/link/internal/loadpe" ==> using loadpe = go.cmd.link.@internal.loadpe_package
// Original source: C:\Program Files\Go\src\cmd\link\internal\loadpe\ldpe.go
namespace go.cmd.link.@internal;

using bytes = bytes_package;
using bio = cmd.@internal.bio_package;
using objabi = cmd.@internal.objabi_package;
using sys = cmd.@internal.sys_package;
using loader = cmd.link.@internal.loader_package;
using sym = cmd.link.@internal.sym_package;
using pe = debug.pe_package;
using binary = encoding.binary_package;
using errors = errors_package;
using fmt = fmt_package;
using io = io_package;
using strings = strings_package;
using System;

public static partial class loadpe_package {

 
// TODO: the Microsoft doco says IMAGE_SYM_DTYPE_ARRAY is 3 (same with IMAGE_SYM_DTYPE_POINTER and IMAGE_SYM_DTYPE_FUNCTION)
public static readonly nint IMAGE_SYM_UNDEFINED = 0;
public static readonly nint IMAGE_SYM_ABSOLUTE = -1;
public static readonly nint IMAGE_SYM_DEBUG = -2;
public static readonly nint IMAGE_SYM_TYPE_NULL = 0;
public static readonly nint IMAGE_SYM_TYPE_VOID = 1;
public static readonly nint IMAGE_SYM_TYPE_CHAR = 2;
public static readonly nint IMAGE_SYM_TYPE_SHORT = 3;
public static readonly nint IMAGE_SYM_TYPE_INT = 4;
public static readonly nint IMAGE_SYM_TYPE_LONG = 5;
public static readonly nint IMAGE_SYM_TYPE_FLOAT = 6;
public static readonly nint IMAGE_SYM_TYPE_DOUBLE = 7;
public static readonly nint IMAGE_SYM_TYPE_STRUCT = 8;
public static readonly nint IMAGE_SYM_TYPE_UNION = 9;
public static readonly nint IMAGE_SYM_TYPE_ENUM = 10;
public static readonly nint IMAGE_SYM_TYPE_MOE = 11;
public static readonly nint IMAGE_SYM_TYPE_BYTE = 12;
public static readonly nint IMAGE_SYM_TYPE_WORD = 13;
public static readonly nint IMAGE_SYM_TYPE_UINT = 14;
public static readonly nint IMAGE_SYM_TYPE_DWORD = 15;
public static readonly nint IMAGE_SYM_TYPE_PCODE = 32768;
public static readonly nint IMAGE_SYM_DTYPE_NULL = 0;
public static readonly nuint IMAGE_SYM_DTYPE_POINTER = 0x10;
public static readonly nuint IMAGE_SYM_DTYPE_FUNCTION = 0x20;
public static readonly nuint IMAGE_SYM_DTYPE_ARRAY = 0x30;
public static readonly nint IMAGE_SYM_CLASS_END_OF_FUNCTION = -1;
public static readonly nint IMAGE_SYM_CLASS_NULL = 0;
public static readonly nint IMAGE_SYM_CLASS_AUTOMATIC = 1;
public static readonly nint IMAGE_SYM_CLASS_EXTERNAL = 2;
public static readonly nint IMAGE_SYM_CLASS_STATIC = 3;
public static readonly nint IMAGE_SYM_CLASS_REGISTER = 4;
public static readonly nint IMAGE_SYM_CLASS_EXTERNAL_DEF = 5;
public static readonly nint IMAGE_SYM_CLASS_LABEL = 6;
public static readonly nint IMAGE_SYM_CLASS_UNDEFINED_LABEL = 7;
public static readonly nint IMAGE_SYM_CLASS_MEMBER_OF_STRUCT = 8;
public static readonly nint IMAGE_SYM_CLASS_ARGUMENT = 9;
public static readonly nint IMAGE_SYM_CLASS_STRUCT_TAG = 10;
public static readonly nint IMAGE_SYM_CLASS_MEMBER_OF_UNION = 11;
public static readonly nint IMAGE_SYM_CLASS_UNION_TAG = 12;
public static readonly nint IMAGE_SYM_CLASS_TYPE_DEFINITION = 13;
public static readonly nint IMAGE_SYM_CLASS_UNDEFINED_STATIC = 14;
public static readonly nint IMAGE_SYM_CLASS_ENUM_TAG = 15;
public static readonly nint IMAGE_SYM_CLASS_MEMBER_OF_ENUM = 16;
public static readonly nint IMAGE_SYM_CLASS_REGISTER_PARAM = 17;
public static readonly nint IMAGE_SYM_CLASS_BIT_FIELD = 18;
public static readonly nint IMAGE_SYM_CLASS_FAR_EXTERNAL = 68; /* Not in PECOFF v8 spec */
public static readonly nint IMAGE_SYM_CLASS_BLOCK = 100;
public static readonly nint IMAGE_SYM_CLASS_FUNCTION = 101;
public static readonly nint IMAGE_SYM_CLASS_END_OF_STRUCT = 102;
public static readonly nint IMAGE_SYM_CLASS_FILE = 103;
public static readonly nint IMAGE_SYM_CLASS_SECTION = 104;
public static readonly nint IMAGE_SYM_CLASS_WEAK_EXTERNAL = 105;
public static readonly nint IMAGE_SYM_CLASS_CLR_TOKEN = 107;
public static readonly nuint IMAGE_REL_I386_ABSOLUTE = 0x0000;
public static readonly nuint IMAGE_REL_I386_DIR16 = 0x0001;
public static readonly nuint IMAGE_REL_I386_REL16 = 0x0002;
public static readonly nuint IMAGE_REL_I386_DIR32 = 0x0006;
public static readonly nuint IMAGE_REL_I386_DIR32NB = 0x0007;
public static readonly nuint IMAGE_REL_I386_SEG12 = 0x0009;
public static readonly nuint IMAGE_REL_I386_SECTION = 0x000A;
public static readonly nuint IMAGE_REL_I386_SECREL = 0x000B;
public static readonly nuint IMAGE_REL_I386_TOKEN = 0x000C;
public static readonly nuint IMAGE_REL_I386_SECREL7 = 0x000D;
public static readonly nuint IMAGE_REL_I386_REL32 = 0x0014;
public static readonly nuint IMAGE_REL_AMD64_ABSOLUTE = 0x0000;
public static readonly nuint IMAGE_REL_AMD64_ADDR64 = 0x0001;
public static readonly nuint IMAGE_REL_AMD64_ADDR32 = 0x0002;
public static readonly nuint IMAGE_REL_AMD64_ADDR32NB = 0x0003;
public static readonly nuint IMAGE_REL_AMD64_REL32 = 0x0004;
public static readonly nuint IMAGE_REL_AMD64_REL32_1 = 0x0005;
public static readonly nuint IMAGE_REL_AMD64_REL32_2 = 0x0006;
public static readonly nuint IMAGE_REL_AMD64_REL32_3 = 0x0007;
public static readonly nuint IMAGE_REL_AMD64_REL32_4 = 0x0008;
public static readonly nuint IMAGE_REL_AMD64_REL32_5 = 0x0009;
public static readonly nuint IMAGE_REL_AMD64_SECTION = 0x000A;
public static readonly nuint IMAGE_REL_AMD64_SECREL = 0x000B;
public static readonly nuint IMAGE_REL_AMD64_SECREL7 = 0x000C;
public static readonly nuint IMAGE_REL_AMD64_TOKEN = 0x000D;
public static readonly nuint IMAGE_REL_AMD64_SREL32 = 0x000E;
public static readonly nuint IMAGE_REL_AMD64_PAIR = 0x000F;
public static readonly nuint IMAGE_REL_AMD64_SSPAN32 = 0x0010;
public static readonly nuint IMAGE_REL_ARM_ABSOLUTE = 0x0000;
public static readonly nuint IMAGE_REL_ARM_ADDR32 = 0x0001;
public static readonly nuint IMAGE_REL_ARM_ADDR32NB = 0x0002;
public static readonly nuint IMAGE_REL_ARM_BRANCH24 = 0x0003;
public static readonly nuint IMAGE_REL_ARM_BRANCH11 = 0x0004;
public static readonly nuint IMAGE_REL_ARM_SECTION = 0x000E;
public static readonly nuint IMAGE_REL_ARM_SECREL = 0x000F;
public static readonly nuint IMAGE_REL_ARM_MOV32 = 0x0010;
public static readonly nuint IMAGE_REL_THUMB_MOV32 = 0x0011;
public static readonly nuint IMAGE_REL_THUMB_BRANCH20 = 0x0012;
public static readonly nuint IMAGE_REL_THUMB_BRANCH24 = 0x0014;
public static readonly nuint IMAGE_REL_THUMB_BLX23 = 0x0015;
public static readonly nuint IMAGE_REL_ARM_PAIR = 0x0016;
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

// TODO(crawshaw): de-duplicate these symbols with cmd/internal/ld, ideally in debug/pe.
public static readonly nuint IMAGE_SCN_CNT_CODE = 0x00000020;
public static readonly nuint IMAGE_SCN_CNT_INITIALIZED_DATA = 0x00000040;
public static readonly nuint IMAGE_SCN_CNT_UNINITIALIZED_DATA = 0x00000080;
public static readonly nuint IMAGE_SCN_MEM_DISCARDABLE = 0x02000000;
public static readonly nuint IMAGE_SCN_MEM_EXECUTE = 0x20000000;
public static readonly nuint IMAGE_SCN_MEM_READ = 0x40000000;
public static readonly nuint IMAGE_SCN_MEM_WRITE = 0x80000000;

// TODO(brainman): maybe just add ReadAt method to bio.Reader instead of creating peBiobuf

// peBiobuf makes bio.Reader look like io.ReaderAt.
private partial struct peBiobuf { // : bio.Reader
}

private static (nint, error) ReadAt(this ptr<peBiobuf> _addr_f, slice<byte> p, long off) {
    nint _p0 = default;
    error _p0 = default!;
    ref peBiobuf f = ref _addr_f.val;

    var ret = ((bio.Reader.val)(f)).MustSeek(off, 0);
    if (ret < 0) {
        return (0, error.As(errors.New("fail to seek"))!);
    }
    var (n, err) = f.Read(p);
    if (err != null) {
        return (0, error.As(err)!);
    }
    return (n, error.As(null!)!);
}

// makeUpdater creates a loader.SymbolBuilder if one hasn't been created previously.
// We use this to lazily make SymbolBuilders as we don't always need a builder, and creating them for all symbols might be an error.
private static ptr<loader.SymbolBuilder> makeUpdater(ptr<loader.Loader> _addr_l, ptr<loader.SymbolBuilder> _addr_bld, loader.Sym s) {
    ref loader.Loader l = ref _addr_l.val;
    ref loader.SymbolBuilder bld = ref _addr_bld.val;

    if (bld != null) {
        return _addr_bld!;
    }
    bld = l.MakeSymbolUpdater(s);
    return _addr_bld!;
}

// Load loads the PE file pn from input.
// Symbols are written into syms, and a slice of the text symbols is returned.
// If an .rsrc section or set of .rsrc$xx sections is found, its symbols are
// returned as rsrc.
public static (slice<loader.Sym>, slice<loader.Sym>, error) Load(ptr<loader.Loader> _addr_l, ptr<sys.Arch> _addr_arch, nint localSymVersion, ptr<bio.Reader> _addr_input, @string pkg, long length, @string pn) => func((defer, _, _) => {
    slice<loader.Sym> textp = default;
    slice<loader.Sym> rsrc = default;
    error err = default!;
    ref loader.Loader l = ref _addr_l.val;
    ref sys.Arch arch = ref _addr_arch.val;
    ref bio.Reader input = ref _addr_input.val;

    var lookup = l.LookupOrCreateCgoExport;
    var sectsyms = make_map<ptr<pe.Section>, loader.Sym>();
    var sectdata = make_map<ptr<pe.Section>, slice<byte>>(); 

    // Some input files are archives containing multiple of
    // object files, and pe.NewFile seeks to the start of
    // input file and get confused. Create section reader
    // to stop pe.NewFile looking before current position.
    var sr = io.NewSectionReader((peBiobuf.val)(input), input.Offset(), 1 << 63 - 1); 

    // TODO: replace pe.NewFile with pe.Load (grep for "add Load function" in debug/pe for details)
    var (f, err) = pe.NewFile(sr);
    if (err != null) {
        return (null, null, error.As(err)!);
    }
    defer(f.Close()); 

    // TODO return error if found .cormeta

    // create symbols for mapped sections
    {
        var sect__prev1 = sect;

        foreach (var (_, __sect) in f.Sections) {
            sect = __sect;
            if (sect.Characteristics & IMAGE_SCN_MEM_DISCARDABLE != 0) {
                continue;
            }
            if (sect.Characteristics & (IMAGE_SCN_CNT_CODE | IMAGE_SCN_CNT_INITIALIZED_DATA | IMAGE_SCN_CNT_UNINITIALIZED_DATA) == 0) { 
                // This has been seen for .idata sections, which we
                // want to ignore. See issues 5106 and 5273.
                continue;
            }
            var name = fmt.Sprintf("%s(%s)", pkg, sect.Name);
            var s = lookup(name, localSymVersion);
            var bld = l.MakeSymbolUpdater(s);


            if (sect.Characteristics & (IMAGE_SCN_CNT_UNINITIALIZED_DATA | IMAGE_SCN_CNT_INITIALIZED_DATA | IMAGE_SCN_MEM_READ | IMAGE_SCN_MEM_WRITE | IMAGE_SCN_CNT_CODE | IMAGE_SCN_MEM_EXECUTE) == IMAGE_SCN_CNT_INITIALIZED_DATA | IMAGE_SCN_MEM_READ) //.rdata
                bld.SetType(sym.SRODATA);
            else if (sect.Characteristics & (IMAGE_SCN_CNT_UNINITIALIZED_DATA | IMAGE_SCN_CNT_INITIALIZED_DATA | IMAGE_SCN_MEM_READ | IMAGE_SCN_MEM_WRITE | IMAGE_SCN_CNT_CODE | IMAGE_SCN_MEM_EXECUTE) == IMAGE_SCN_CNT_UNINITIALIZED_DATA | IMAGE_SCN_MEM_READ | IMAGE_SCN_MEM_WRITE) //.bss
                bld.SetType(sym.SNOPTRBSS);
            else if (sect.Characteristics & (IMAGE_SCN_CNT_UNINITIALIZED_DATA | IMAGE_SCN_CNT_INITIALIZED_DATA | IMAGE_SCN_MEM_READ | IMAGE_SCN_MEM_WRITE | IMAGE_SCN_CNT_CODE | IMAGE_SCN_MEM_EXECUTE) == IMAGE_SCN_CNT_INITIALIZED_DATA | IMAGE_SCN_MEM_READ | IMAGE_SCN_MEM_WRITE) //.data
                bld.SetType(sym.SNOPTRDATA);
            else if (sect.Characteristics & (IMAGE_SCN_CNT_UNINITIALIZED_DATA | IMAGE_SCN_CNT_INITIALIZED_DATA | IMAGE_SCN_MEM_READ | IMAGE_SCN_MEM_WRITE | IMAGE_SCN_CNT_CODE | IMAGE_SCN_MEM_EXECUTE) == IMAGE_SCN_CNT_CODE | IMAGE_SCN_MEM_EXECUTE | IMAGE_SCN_MEM_READ) //.text
                bld.SetType(sym.STEXT);
            else 
                return (null, null, error.As(fmt.Errorf("unexpected flags %#06x for PE section %s", sect.Characteristics, sect.Name))!);
                        if (bld.Type() != sym.SNOPTRBSS) {
                var (data, err) = sect.Data();
                if (err != null) {
                    return (null, null, error.As(err)!);
                }
                sectdata[sect] = data;
                bld.SetData(data);
            }
            bld.SetSize(int64(sect.Size));
            sectsyms[sect] = s;
            if (sect.Name == ".rsrc" || strings.HasPrefix(sect.Name, ".rsrc$")) {
                rsrc = append(rsrc, s);
            }
        }
        sect = sect__prev1;
    }

    foreach (var (_, rsect) in f.Sections) {
        {
            var (_, found) = sectsyms[rsect];

            if (!found) {
                continue;
            }

        }
        if (rsect.NumberOfRelocations == 0) {
            continue;
        }
        if (rsect.Characteristics & IMAGE_SCN_MEM_DISCARDABLE != 0) {
            continue;
        }
        if (rsect.Characteristics & (IMAGE_SCN_CNT_CODE | IMAGE_SCN_CNT_INITIALIZED_DATA | IMAGE_SCN_CNT_UNINITIALIZED_DATA) == 0) { 
            // This has been seen for .idata sections, which we
            // want to ignore. See issues 5106 and 5273.
            continue;
        }
        var splitResources = strings.HasPrefix(rsect.Name, ".rsrc$");
        var sb = l.MakeSymbolUpdater(sectsyms[rsect]);
        foreach (var (j, r) in rsect.Relocs) {
            if (int(r.SymbolTableIndex) >= len(f.COFFSymbols)) {
                return (null, null, error.As(fmt.Errorf("relocation number %d symbol index idx=%d cannot be large then number of symbols %d", j, r.SymbolTableIndex, len(f.COFFSymbols)))!);
            }
            var pesym = _addr_f.COFFSymbols[r.SymbolTableIndex];
            var (_, gosym, err) = readpesym(_addr_l, _addr_arch, lookup, _addr_f, _addr_pesym, sectsyms, localSymVersion);
            if (err != null) {
                return (null, null, error.As(err)!);
            }
            if (gosym == 0) {
                var (name, err) = pesym.FullName(f.StringTable);
                if (err != null) {
                    name = string(pesym.Name[..]);
                }
                return (null, null, error.As(fmt.Errorf("reloc of invalid sym %s idx=%d type=%d", name, r.SymbolTableIndex, pesym.Type))!);
            }
            var rSym = gosym;
            var rSize = uint8(4);
            var rOff = int32(r.VirtualAddress);
            long rAdd = default;
            objabi.RelocType rType = default;

            if (arch.Family == sys.I386 || arch.Family == sys.AMD64) 

                if (r.Type == IMAGE_REL_I386_REL32 || r.Type == IMAGE_REL_AMD64_REL32 || r.Type == IMAGE_REL_AMD64_ADDR32 || r.Type == IMAGE_REL_AMD64_ADDR32NB) 
                    rType = objabi.R_PCREL;

                    rAdd = int64(int32(binary.LittleEndian.Uint32(sectdata[rsect][(int)rOff..])));
                else if (r.Type == IMAGE_REL_I386_DIR32NB || r.Type == IMAGE_REL_I386_DIR32) 
                    rType = objabi.R_ADDR; 

                    // load addend from image
                    rAdd = int64(int32(binary.LittleEndian.Uint32(sectdata[rsect][(int)rOff..])));
                else if (r.Type == IMAGE_REL_AMD64_ADDR64) // R_X86_64_64
                    rSize = 8;

                    rType = objabi.R_ADDR; 

                    // load addend from image
                    rAdd = int64(binary.LittleEndian.Uint64(sectdata[rsect][(int)rOff..]));
                else 
                    return (null, null, error.As(fmt.Errorf("%s: %v: unknown relocation type %v", pn, sectsyms[rsect], r.Type))!);
                            else if (arch.Family == sys.ARM) 

                if (r.Type == IMAGE_REL_ARM_SECREL) 
                    rType = objabi.R_PCREL;

                    rAdd = int64(int32(binary.LittleEndian.Uint32(sectdata[rsect][(int)rOff..])));
                else if (r.Type == IMAGE_REL_ARM_ADDR32 || r.Type == IMAGE_REL_ARM_ADDR32NB) 
                    rType = objabi.R_ADDR;

                    rAdd = int64(int32(binary.LittleEndian.Uint32(sectdata[rsect][(int)rOff..])));
                else if (r.Type == IMAGE_REL_ARM_BRANCH24) 
                    rType = objabi.R_CALLARM;

                    rAdd = int64(int32(binary.LittleEndian.Uint32(sectdata[rsect][(int)rOff..])));
                else 
                    return (null, null, error.As(fmt.Errorf("%s: %v: unknown ARM relocation type %v", pn, sectsyms[rsect], r.Type))!);
                            else if (arch.Family == sys.ARM64) 

                if (r.Type == IMAGE_REL_ARM64_ADDR32 || r.Type == IMAGE_REL_ARM64_ADDR32NB) 
                    rType = objabi.R_ADDR;

                    rAdd = int64(int32(binary.LittleEndian.Uint32(sectdata[rsect][(int)rOff..])));
                else 
                    return (null, null, error.As(fmt.Errorf("%s: %v: unknown ARM64 relocation type %v", pn, sectsyms[rsect], r.Type))!);
                            else 
                return (null, null, error.As(fmt.Errorf("%s: unsupported arch %v", pn, arch.Family))!);
            // ld -r could generate multiple section symbols for the
            // same section but with different values, we have to take
            // that into account, or in the case of split resources,
            // the section and its symbols are split into two sections.
            if (issect(_addr_pesym) || splitResources) {
                rAdd += int64(pesym.Value);
            }
            var (rel, _) = sb.AddRel(rType);
            rel.SetOff(rOff);
            rel.SetSiz(rSize);
            rel.SetSym(rSym);
            rel.SetAdd(rAdd);
        }        sb.SortRelocs();
    }    {
        nint i = 0;
        nint numaux = 0;

        while (i < len(f.COFFSymbols)) {
            pesym = _addr_f.COFFSymbols[i];

            numaux = int(pesym.NumberOfAuxSymbols);

            (name, err) = pesym.FullName(f.StringTable);
            if (err != null) {
                return (null, null, error.As(err)!);
            i += numaux + 1;
            }
            if (name == "") {
                continue;
            }
            if (issect(_addr_pesym)) {
                continue;
            }
            if (int(pesym.SectionNumber) > len(f.Sections)) {
                continue;
            }
            if (pesym.SectionNumber == IMAGE_SYM_DEBUG) {
                continue;
            }
            if (pesym.SectionNumber == IMAGE_SYM_ABSOLUTE && bytes.Equal(pesym.Name[..], (slice<byte>)"@feat.00")) { 
                // Microsoft's linker looks at whether all input objects have an empty
                // section called @feat.00. If all of them do, then it enables SEH;
                // otherwise it doesn't enable that feature. So, since around the Windows
                // XP SP2 era, most tools that make PE objects just tack on that section,
                // so that it won't gimp Microsoft's linker logic. Go doesn't support SEH,
                // so in theory, none of this really matters to us. But actually, if the
                // linker tries to ingest an object with @feat.00 -- which are produced by
                // LLVM's resource compiler, for example -- it chokes because of the
                // IMAGE_SYM_ABSOLUTE section that it doesn't know how to deal with. Since
                // @feat.00 is just a marking anyway, skip IMAGE_SYM_ABSOLUTE sections that
                // are called @feat.00.
                continue;
            }
            ptr<pe.Section> sect;
            if (pesym.SectionNumber > 0) {
                sect = f.Sections[pesym.SectionNumber - 1];
                {
                    (_, found) = sectsyms[sect];

                    if (!found) {
                        continue;
                    }

                }
            }
            var (bld, s, err) = readpesym(_addr_l, _addr_arch, lookup, _addr_f, _addr_pesym, sectsyms, localSymVersion);
            if (err != null) {
                return (null, null, error.As(err)!);
            }
            if (pesym.SectionNumber == 0) { // extern
                if (l.SymType(s) == sym.SDYNIMPORT) {
                    bld = makeUpdater(_addr_l, _addr_bld, s);
                    bld.SetPlt(-2); // flag for dynimport in PE object files.
                }
                if (l.SymType(s) == sym.SXREF && pesym.Value > 0) { // global data
                    bld = makeUpdater(_addr_l, _addr_bld, s);
                    bld.SetType(sym.SNOPTRDATA);
                    bld.SetSize(int64(pesym.Value));
                }
                continue;
            }
            else if (pesym.SectionNumber > 0 && int(pesym.SectionNumber) <= len(f.Sections)) {
                sect = f.Sections[pesym.SectionNumber - 1];
                {
                    (_, found) = sectsyms[sect];

                    if (!found) {
                        return (null, null, error.As(fmt.Errorf("%s: %v: missing sect.sym", pn, s))!);
                    }

                }
            }
            else
 {
                return (null, null, error.As(fmt.Errorf("%s: %v: sectnum < 0!", pn, s))!);
            }
            if (sect == null) {
                return (null, null, error.As(null!)!);
            }
            if (l.OuterSym(s) != 0) {
                if (l.AttrDuplicateOK(s)) {
                    continue;
                }
                var outerName = l.SymName(l.OuterSym(s));
                var sectName = l.SymName(sectsyms[sect]);
                return (null, null, error.As(fmt.Errorf("%s: duplicate symbol reference: %s in both %s and %s", pn, l.SymName(s), outerName, sectName))!);
            }
            bld = makeUpdater(_addr_l, _addr_bld, s);
            var sectsym = sectsyms[sect];
            bld.SetType(l.SymType(sectsym));
            l.AddInteriorSym(sectsym, s);
            bld.SetValue(int64(pesym.Value));
            bld.SetSize(4);
            if (l.SymType(sectsym) == sym.STEXT) {
                if (bld.External() && !bld.DuplicateOK()) {
                    return (null, null, error.As(fmt.Errorf("%s: duplicate symbol definition", l.SymName(s)))!);
                }
                bld.SetExternal(true);
            }
        }
    } 

    // Sort outer lists by address, adding to textp.
    // This keeps textp in increasing address order.
    {
        var sect__prev1 = sect;

        foreach (var (_, __sect) in f.Sections) {
            sect = __sect;
            s = sectsyms[sect];
            if (s == 0) {
                continue;
            }
            l.SortSub(s);
            if (l.SymType(s) == sym.STEXT) {
                while (s != 0) {
                    if (l.AttrOnList(s)) {
                        return (null, null, error.As(fmt.Errorf("symbol %s listed multiple times", l.SymName(s)))!);
                    s = l.SubSym(s);
                    }
                    l.SetAttrOnList(s, true);
                    textp = append(textp, s);
                }
            }
        }
        sect = sect__prev1;
    }

    return (textp, rsrc, error.As(null!)!);
});

private static bool issect(ptr<pe.COFFSymbol> _addr_s) {
    ref pe.COFFSymbol s = ref _addr_s.val;

    return s.StorageClass == IMAGE_SYM_CLASS_STATIC && s.Type == 0 && s.Name[0] == '.';
}

private static (ptr<loader.SymbolBuilder>, loader.Sym, error) readpesym(ptr<loader.Loader> _addr_l, ptr<sys.Arch> _addr_arch, Func<@string, nint, loader.Sym> lookup, ptr<pe.File> _addr_f, ptr<pe.COFFSymbol> _addr_pesym, map<ptr<pe.Section>, loader.Sym> sectsyms, nint localSymVersion) {
    ptr<loader.SymbolBuilder> _p0 = default!;
    loader.Sym _p0 = default;
    error _p0 = default!;
    ref loader.Loader l = ref _addr_l.val;
    ref sys.Arch arch = ref _addr_arch.val;
    ref pe.File f = ref _addr_f.val;
    ref pe.COFFSymbol pesym = ref _addr_pesym.val;

    var (symname, err) = pesym.FullName(f.StringTable);
    if (err != null) {
        return (_addr_null!, 0, error.As(err)!);
    }
    @string name = default;
    if (issect(_addr_pesym)) {
        name = l.SymName(sectsyms[f.Sections[pesym.SectionNumber - 1]]);
    }
    else
 {
        name = symname;

        if (arch.Family == sys.AMD64) 
            if (name == "__imp___acrt_iob_func") { 
                // Do not rename __imp___acrt_iob_func into __acrt_iob_func,
                // because __imp___acrt_iob_func symbol is real
                // (see commit b295099 from git://git.code.sf.net/p/mingw-w64/mingw-w64 for details).
            }
            else
 {
                name = strings.TrimPrefix(name, "__imp_"); // __imp_Name => Name
            }
        else if (arch.Family == sys.I386) 
            if (name == "__imp____acrt_iob_func") { 
                // Do not rename __imp____acrt_iob_func into ___acrt_iob_func,
                // because __imp____acrt_iob_func symbol is real
                // (see commit b295099 from git://git.code.sf.net/p/mingw-w64/mingw-w64 for details).
            }
            else
 {
                name = strings.TrimPrefix(name, "__imp_"); // __imp_Name => Name
            }
            if (name[0] == '_') {
                name = name[(int)1..]; // _Name => Name
            }
            }
    {
        var i = strings.LastIndex(name, "@");

        if (i >= 0) {
            name = name[..(int)i];
        }
    }

    loader.Sym s = default;
    ptr<loader.SymbolBuilder> bld;

    if (pesym.Type == IMAGE_SYM_DTYPE_FUNCTION || pesym.Type == IMAGE_SYM_DTYPE_NULL) 

        if (pesym.StorageClass == IMAGE_SYM_CLASS_EXTERNAL) //global
            s = lookup(name, 0);
        else if (pesym.StorageClass == IMAGE_SYM_CLASS_NULL || pesym.StorageClass == IMAGE_SYM_CLASS_STATIC || pesym.StorageClass == IMAGE_SYM_CLASS_LABEL) 
            s = lookup(name, localSymVersion);
            bld = makeUpdater(_addr_l, bld, s);
            bld.SetDuplicateOK(true);
        else 
            return (_addr_null!, 0, error.As(fmt.Errorf("%s: invalid symbol binding %d", symname, pesym.StorageClass))!);
            else 
        return (_addr_null!, 0, error.As(fmt.Errorf("%s: invalid symbol type %d", symname, pesym.Type))!);
        if (s != 0 && l.SymType(s) == 0 && (pesym.StorageClass != IMAGE_SYM_CLASS_STATIC || pesym.Value != 0)) {
        bld = makeUpdater(_addr_l, bld, s);
        bld.SetType(sym.SXREF);
    }
    if (strings.HasPrefix(symname, "__imp_")) {
        bld = makeUpdater(_addr_l, bld, s);
        bld.SetGot(-2); // flag for __imp_
    }
    return (_addr_bld!, s, error.As(null!)!);
}

} // end loadpe_package
