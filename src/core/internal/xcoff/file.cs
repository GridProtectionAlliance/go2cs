// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package xcoff implements access to XCOFF (Extended Common Object File Format) files.
namespace go.@internal;

using dwarf = debug.dwarf_package;
using binary = encoding.binary_package;
using errors = errors_package;
using fmt = fmt_package;
using saferio = @internal.saferio_package;
using io = io_package;
using os = os_package;
using strings = strings_package;
using debug;
using encoding;

partial class xcoff_package {

// SectionHeader holds information about an XCOFF section header.
[GoType] partial struct SectionHeader {
    public @string Name;
    public uint64 VirtualAddress;
    public uint64 Size;
    public uint32 Type;
    public uint64 Relptr;
    public uint32 Nreloc;
}

[GoType] partial struct ΔSection {
    public partial ref SectionHeader SectionHeader { get; }
    public slice<Reloc> Relocs;
    public partial ref io_package.ReaderAt ReaderAt { get; }
    internal ж<io_package.SectionReader> sr;
}

// AuxiliaryCSect holds information about an XCOFF symbol in an AUX_CSECT entry.
[GoType] partial struct AuxiliaryCSect {
    public int64 Length;
    public nint StorageMappingClass;
    public nint SymbolType;
}

// AuxiliaryFcn holds information about an XCOFF symbol in an AUX_FCN entry.
[GoType] partial struct AuxiliaryFcn {
    public int64 Size;
}

[GoType] partial struct Symbol {
    public @string Name;
    public uint64 Value;
    public nint SectionNumber;
    public nint StorageClass;
    public AuxiliaryFcn AuxFcn;
    public AuxiliaryCSect AuxCSect;
}

[GoType] partial struct Reloc {
    public uint64 VirtualAddress;
    public ж<Symbol> Symbol;
    public bool Signed;
    public bool InstructionFixed;
    public uint8 Length;
    public uint8 Type;
}

// ImportedSymbol holds information about an imported XCOFF symbol.
[GoType] partial struct ImportedSymbol {
    public @string Name;
    public @string Library;
}

// FileHeader holds information about an XCOFF file header.
[GoType] partial struct FileHeader {
    public uint16 TargetMachine;
}

// A File represents an open XCOFF file.
[GoType] partial struct File {
    public partial ref FileHeader FileHeader { get; }
    public slice<ж<ΔSection>> Sections;
    public slice<ж<Symbol>> Symbols;
    public slice<byte> StringTable;
    public slice<@string> LibraryPaths;
    internal io_package.Closer closer;
}

// Open opens the named file using os.Open and prepares it for use as an XCOFF binary.
public static (ж<File>, error) Open(@string name) {
    (f, err) = os.Open(name);
    if (err != default!) {
        return (default!, err);
    }
    (ff, err) = NewFile(~f);
    if (err != default!) {
        f.Close();
        return (default!, err);
    }
    ff.val.closer = f;
    return (ff, default!);
}

// Close closes the File.
// If the File was created using NewFile directly instead of Open,
// Close has no effect.
[GoRecv] public static error Close(this ref File f) {
    error err = default!;
    if (f.closer != default!) {
        err = f.closer.Close();
        f.closer = default!;
    }
    return err;
}

// Section returns the first section with the given name, or nil if no such
// section exists.
// Xcoff have section's name limited to 8 bytes. Some sections like .gosymtab
// can be trunked but this method will still find them.
[GoRecv] public static ж<ΔSection> Section(this ref File f, @string name) {
    foreach (var (_, s) in f.Sections) {
        if (s.Name == name || (len(name) > 8 && s.Name == name[..8])) {
            return s;
        }
    }
    return default!;
}

// SectionByType returns the first section in f with the
// given type, or nil if there is no such section.
[GoRecv] public static ж<ΔSection> SectionByType(this ref File f, uint32 typ) {
    foreach (var (_, s) in f.Sections) {
        if (s.Type == typ) {
            return s;
        }
    }
    return default!;
}

// cstring converts ASCII byte sequence b to string.
// It stops once it finds 0 or reaches end of b.
internal static @string cstring(slice<byte> b) {
    nint i = default!;
    for (i = 0; i < len(b) && b[i] != 0; i++) {
    }
    return ((@string)(b[..(int)(i)]));
}

// getString extracts a string from an XCOFF string table.
internal static (@string, bool) getString(slice<byte> st, uint32 offset) {
    if (offset < 4 || ((nint)offset) >= len(st)) {
        return ("", false);
    }
    return (cstring(st[(int)(offset)..]), true);
}

// NewFile creates a new File for accessing an XCOFF binary in an underlying reader.
public static (ж<File>, error) NewFile(io.ReaderAt r) {
    var sr = io.NewSectionReader(r, 0, 1 << (int)(63) - 1);
    // Read XCOFF target machine
    ref var magic = ref heap(new uint16(), out var Ꮡmagic);
    {
        var err = binary.Read(~sr, binary.BigEndian, Ꮡmagic); if (err != default!) {
            return (default!, err);
        }
    }
    if (magic != U802TOCMAGIC && magic != U64_TOCMAGIC) {
        return (default!, fmt.Errorf("unrecognised XCOFF magic: 0x%x"u8, magic));
    }
    var f = @new<File>();
    f.TargetMachine = magic;
    // Read XCOFF file header
    {
        var (_, err) = sr.Seek(0, io.SeekStart); if (err != default!) {
            return (default!, err);
        }
    }
    uint16 nscns = default!;
    uint64 symptr = default!;
    uint32 nsyms = default!;
    uint16 opthdr = default!;
    nint hdrsz = default!;
    switch (f.TargetMachine) {
    case U802TOCMAGIC: {
        var fhdr = @new<FileHeader32>();
        {
            var err = binary.Read(~sr, binary.BigEndian, fhdr); if (err != default!) {
                return (default!, err);
            }
        }
        nscns = fhdr.val.Fnscns;
        symptr = ((uint64)(~fhdr).Fsymptr);
        nsyms = fhdr.val.Fnsyms;
        opthdr = fhdr.val.Fopthdr;
        hdrsz = FILHSZ_32;
        break;
    }
    case U64_TOCMAGIC: {
        var fhdr = @new<FileHeader64>();
        {
            var err = binary.Read(~sr, binary.BigEndian, fhdr); if (err != default!) {
                return (default!, err);
            }
        }
        nscns = fhdr.val.Fnscns;
        symptr = fhdr.val.Fsymptr;
        nsyms = fhdr.val.Fnsyms;
        opthdr = fhdr.val.Fopthdr;
        hdrsz = FILHSZ_64;
        break;
    }}

    if (symptr == 0 || nsyms <= 0) {
        return (default!, fmt.Errorf("no symbol table"u8));
    }
    // Read string table (located right after symbol table).
    var offset = symptr + ((uint64)nsyms) * SYMESZ;
    {
        var (_, err) = sr.Seek(((int64)offset), io.SeekStart); if (err != default!) {
            return (default!, err);
        }
    }
    // The first 4 bytes contain the length (in bytes).
    ref var l = ref heap(new uint32(), out var Ꮡl);
    {
        var err = binary.Read(~sr, binary.BigEndian, Ꮡl); if (err != default!) {
            return (default!, err);
        }
    }
    if (l > 4) {
        (st, err) = saferio.ReadDataAt(~sr, ((uint64)l), ((int64)offset));
        if (err != default!) {
            return (default!, err);
        }
        f.val.StringTable = st;
    }
    // Read section headers
    {
        var (_, err) = sr.Seek(((int64)hdrsz) + ((int64)opthdr), io.SeekStart); if (err != default!) {
            return (default!, err);
        }
    }
    nint c = saferio.SliceCap[ΔSection.val](((uint64)nscns));
    if (c < 0) {
        return (default!, fmt.Errorf("too many XCOFF sections (%d)"u8, nscns));
    }
    f.val.Sections = new slice<ж<ΔSection>>(0, c);
    for (nint i = 0; i < ((nint)nscns); i++) {
        uint64 scnptr = default!;
        var s = @new<ΔSection>();
        switch (f.TargetMachine) {
        case U802TOCMAGIC: {
            var shdr = @new<SectionHeader32>();
            {
                var err = binary.Read(~sr, binary.BigEndian, shdr); if (err != default!) {
                    return (default!, err);
                }
            }
            s.Name = cstring((~shdr).Sname[..]);
            s.VirtualAddress = ((uint64)(~shdr).Svaddr);
            s.Size = ((uint64)(~shdr).Ssize);
            scnptr = ((uint64)(~shdr).Sscnptr);
            s.Type = shdr.val.Sflags;
            s.Relptr = ((uint64)(~shdr).Srelptr);
            s.Nreloc = ((uint32)(~shdr).Snreloc);
            break;
        }
        case U64_TOCMAGIC: {
            var shdr = @new<SectionHeader64>();
            {
                var err = binary.Read(~sr, binary.BigEndian, shdr); if (err != default!) {
                    return (default!, err);
                }
            }
            s.Name = cstring((~shdr).Sname[..]);
            s.VirtualAddress = shdr.val.Svaddr;
            s.Size = shdr.val.Ssize;
            scnptr = shdr.val.Sscnptr;
            s.Type = shdr.val.Sflags;
            s.Relptr = shdr.val.Srelptr;
            s.Nreloc = shdr.val.Snreloc;
            break;
        }}

        var r2 = r;
        if (scnptr == 0) {
            // .bss must have all 0s
            Ꮡr2 = new nobitsSectionReader(nil); r2 = ref Ꮡr2.val;
        }
        s.val.sr = io.NewSectionReader(r2, ((int64)scnptr), ((int64)s.Size));
        s.val.ReaderAt = s.val.sr;
        f.val.Sections = append((~f).Sections, s);
    }
    // Symbol map needed by relocation
    map<nint, ж<Symbol>> idxToSym = new map<nint, ж<Symbol>>();
    // Read symbol table
    {
        var (_, err) = sr.Seek(((int64)symptr), io.SeekStart); if (err != default!) {
            return (default!, err);
        }
    }
    f.val.Symbols = new slice<ж<Symbol>>(0);
    for (nint i = 0; i < ((nint)nsyms); i++) {
        nint numaux = default!;
        bool ok = default!;
        bool needAuxFcn = default!;
        var sym = @new<Symbol>();
        switch (f.TargetMachine) {
        case U802TOCMAGIC: {
            var se = @new<SymEnt32>();
            {
                var err = binary.Read(~sr, binary.BigEndian, se); if (err != default!) {
                    return (default!, err);
                }
            }
            numaux = ((nint)(~se).Nnumaux);
            sym.val.SectionNumber = ((nint)(~se).Nscnum);
            sym.val.StorageClass = ((nint)(~se).Nsclass);
            sym.val.Value = ((uint64)(~se).Nvalue);
            needAuxFcn = (uint16)((~se).Ntype & SYM_TYPE_FUNC) != 0 && numaux > 1;
            var zeroes = binary.BigEndian.Uint32((~se).Nname[..4]);
            if (zeroes != 0){
                sym.val.Name = cstring((~se).Nname[..]);
            } else {
                var offsetΔ2 = binary.BigEndian.Uint32((~se).Nname[4..]);
                (sym.val.Name, ok) = getString((~f).StringTable, offsetΔ2);
                if (!ok) {
                    goto skip;
                }
            }
            break;
        }
        case U64_TOCMAGIC: {
            var se = @new<SymEnt64>();
            {
                var err = binary.Read(~sr, binary.BigEndian, se); if (err != default!) {
                    return (default!, err);
                }
            }
            numaux = ((nint)(~se).Nnumaux);
            sym.val.SectionNumber = ((nint)(~se).Nscnum);
            sym.val.StorageClass = ((nint)(~se).Nsclass);
            sym.val.Value = se.val.Nvalue;
            needAuxFcn = (uint16)((~se).Ntype & SYM_TYPE_FUNC) != 0 && numaux > 1;
            (sym.val.Name, ok) = getString((~f).StringTable, (~se).Noffset);
            if (!ok) {
                goto skip;
            }
            break;
        }}

        if ((~sym).StorageClass != C_EXT && (~sym).StorageClass != C_WEAKEXT && (~sym).StorageClass != C_HIDEXT) {
            goto skip;
        }
        // Must have at least one csect auxiliary entry.
        if (numaux < 1 || i + numaux >= ((nint)nsyms)) {
            goto skip;
        }
        if ((~sym).SectionNumber > ((nint)nscns)) {
            goto skip;
        }
        if ((~sym).SectionNumber == 0){
            sym.val.Value = 0;
        } else {
            sym.val.Value -= (~f).Sections[(~sym).SectionNumber - 1].VirtualAddress;
        }
        idxToSym[i] = sym;
        // If this symbol is a function, it must retrieve its size from
        // its AUX_FCN entry.
        // It can happen that a function symbol doesn't have any AUX_FCN.
        // In this case, needAuxFcn is false and their size will be set to 0.
        if (needAuxFcn) {
            switch (f.TargetMachine) {
            case U802TOCMAGIC: {
                var aux = @new<AuxFcn32>();
                {
                    var err = binary.Read(~sr, binary.BigEndian, aux); if (err != default!) {
                        return (default!, err);
                    }
                }
                (~sym).AuxFcn.Size = ((int64)(~aux).Xfsize);
                break;
            }
            case U64_TOCMAGIC: {
                var aux = @new<AuxFcn64>();
                {
                    var err = binary.Read(~sr, binary.BigEndian, aux); if (err != default!) {
                        return (default!, err);
                    }
                }
                (~sym).AuxFcn.Size = ((int64)(~aux).Xfsize);
                break;
            }}

        }
        // Read csect auxiliary entry (by convention, it is the last).
        if (!needAuxFcn) {
            {
                var (_, err) = sr.Seek(((int64)(numaux - 1)) * SYMESZ, io.SeekCurrent); if (err != default!) {
                    return (default!, err);
                }
            }
        }
        i += numaux;
        numaux = 0;
        switch (f.TargetMachine) {
        case U802TOCMAGIC: {
            var aux = @new<AuxCSect32>();
            {
                var err = binary.Read(~sr, binary.BigEndian, aux); if (err != default!) {
                    return (default!, err);
                }
            }
            (~sym).AuxCSect.SymbolType = ((nint)((uint8)((~aux).Xsmtyp & 7)));
            (~sym).AuxCSect.StorageMappingClass = ((nint)(~aux).Xsmclas);
            (~sym).AuxCSect.Length = ((int64)(~aux).Xscnlen);
            break;
        }
        case U64_TOCMAGIC: {
            var aux = @new<AuxCSect64>();
            {
                var err = binary.Read(~sr, binary.BigEndian, aux); if (err != default!) {
                    return (default!, err);
                }
            }
            (~sym).AuxCSect.SymbolType = ((nint)((uint8)((~aux).Xsmtyp & 7)));
            (~sym).AuxCSect.StorageMappingClass = ((nint)(~aux).Xsmclas);
            (~sym).AuxCSect.Length = (int64)(((int64)(~aux).Xscnlenhi) << (int)(32) | ((int64)(~aux).Xscnlenlo));
            break;
        }}

        f.val.Symbols = append((~f).Symbols, sym);
skip:
        i += numaux;
        // Skip auxiliary entries
        {
            var (_, err) = sr.Seek(((int64)numaux) * SYMESZ, io.SeekCurrent); if (err != default!) {
                return (default!, err);
            }
        }
    }
    // Read relocations
    // Only for .data or .text section
    foreach (var (sectNum, sect) in (~f).Sections) {
        if (sect.Type != STYP_TEXT && sect.Type != STYP_DATA) {
            continue;
        }
        if (sect.Relptr == 0) {
            continue;
        }
        nint c = saferio.SliceCap<Reloc>(((uint64)sect.Nreloc));
        if (c < 0) {
            return (default!, fmt.Errorf("too many relocs (%d) for section %d"u8, sect.Nreloc, sectNum));
        }
        sect.val.Relocs = new slice<Reloc>(0, c);
        {
            var (_, err) = sr.Seek(((int64)sect.Relptr), io.SeekStart); if (err != default!) {
                return (default!, err);
            }
        }
        for (var i = ((uint32)0); i < sect.Nreloc; i++) {
            Reloc reloc = default!;
            switch (f.TargetMachine) {
            case U802TOCMAGIC: {
                var rel = @new<Reloc32>();
                {
                    var err = binary.Read(~sr, binary.BigEndian, rel); if (err != default!) {
                        return (default!, err);
                    }
                }
                reloc.VirtualAddress = ((uint64)(~rel).Rvaddr);
                reloc.Symbol = idxToSym[((nint)(~rel).Rsymndx)];
                reloc.Type = rel.val.Rtype;
                reloc.Length = (uint8)((~rel).Rsize & 63) + 1;
                if ((uint8)((~rel).Rsize & 128) != 0) {
                    reloc.Signed = true;
                }
                if ((uint8)((~rel).Rsize & 64) != 0) {
                    reloc.InstructionFixed = true;
                }
                break;
            }
            case U64_TOCMAGIC: {
                var rel = @new<Reloc64>();
                {
                    var err = binary.Read(~sr, binary.BigEndian, rel); if (err != default!) {
                        return (default!, err);
                    }
                }
                reloc.VirtualAddress = rel.val.Rvaddr;
                reloc.Symbol = idxToSym[((nint)(~rel).Rsymndx)];
                reloc.Type = rel.val.Rtype;
                reloc.Length = (uint8)((~rel).Rsize & 63) + 1;
                if ((uint8)((~rel).Rsize & 128) != 0) {
                    reloc.Signed = true;
                }
                if ((uint8)((~rel).Rsize & 64) != 0) {
                    reloc.InstructionFixed = true;
                }
                break;
            }}

            sect.val.Relocs = append((~sect).Relocs, reloc);
        }
    }
    return (f, default!);
}

[GoType] partial struct nobitsSectionReader {
}

[GoRecv] internal static (nint n, error err) ReadAt(this ref nobitsSectionReader _, slice<byte> p, int64 off) {
    nint n = default!;
    error err = default!;

    return (0, errors.New("unexpected read from section with uninitialized data"u8));
}

// Data reads and returns the contents of the XCOFF section s.
[GoRecv] public static (slice<byte>, error) Data(this ref ΔSection s) {
    var dat = new slice<byte>(s.sr.Size());
    var (n, err) = s.sr.ReadAt(dat, 0);
    if (n == len(dat)) {
        err = default!;
    }
    return (dat[..(int)(n)], err);
}

// CSect reads and returns the contents of a csect.
[GoRecv] public static slice<byte> CSect(this ref File f, @string name) {
    foreach (var (_, sym) in f.Symbols) {
        if ((~sym).Name == name && (~sym).AuxCSect.SymbolType == XTY_SD) {
            {
                nint i = (~sym).SectionNumber - 1; if (0 <= i && i < len(f.Sections)) {
                    var s = f.Sections[i];
                    if ((~sym).Value + ((uint64)(~sym).AuxCSect.Length) <= s.Size) {
                        var dat = new slice<byte>((~sym).AuxCSect.Length);
                        var (_, err) = (~s).sr.ReadAt(dat, ((int64)(~sym).Value));
                        if (err != default!) {
                            return default!;
                        }
                        return dat;
                    }
                }
            }
            break;
        }
    }
    return default!;
}

[GoRecv] public static (ж<dwarf.Data>, error) DWARF(this ref File f) {
    // There are many other DWARF sections, but these
    // are the ones the debug/dwarf package uses.
    // Don't bother loading others.
    array<uint32> subtypes = new uint32[]{SSUBTYP_DWABREV, SSUBTYP_DWINFO, SSUBTYP_DWLINE, SSUBTYP_DWRNGES, SSUBTYP_DWSTR}.array();
    array<slice<byte>> dat = new(5); /* len(subtypes) */
    foreach (var (i, subtype) in subtypes) {
        var s = f.SectionByType((uint32)(STYP_DWARF | subtype));
        if (s != nil) {
            (b, err) = s.Data();
            if (err != default! && ((uint64)len(b)) < s.Size) {
                return (default!, err);
            }
            dat[i] = b;
        }
    }
    var abbrev = dat[0];
    var info = dat[1];
    var line = dat[2];
    var ranges = dat[3];
    var str = dat[4];
    return dwarf.New(abbrev, default!, default!, info, line, default!, ranges, str);
}

// readImportID returns the import file IDs stored inside the .loader section.
// Library name pattern is either path/base/member or base/member
[GoRecv] public static (slice<@string>, error) readImportIDs(this ref File f, ж<ΔSection> Ꮡs) {
    ref var s = ref Ꮡs.val;

    // Read loader header
    {
        var (_, err) = s.sr.Seek(0, io.SeekStart); if (err != default!) {
            return (default!, err);
        }
    }
    uint32 istlen = default!;
    uint32 nimpid = default!;
    uint64 impoff = default!;
    switch (f.TargetMachine) {
    case U802TOCMAGIC: {
        var lhdr = @new<LoaderHeader32>();
        {
            var err = binary.Read(~s.sr, binary.BigEndian, lhdr); if (err != default!) {
                return (default!, err);
            }
        }
        istlen = lhdr.val.Listlen;
        nimpid = lhdr.val.Lnimpid;
        impoff = ((uint64)(~lhdr).Limpoff);
        break;
    }
    case U64_TOCMAGIC: {
        var lhdr = @new<LoaderHeader64>();
        {
            var err = binary.Read(~s.sr, binary.BigEndian, lhdr); if (err != default!) {
                return (default!, err);
            }
        }
        istlen = lhdr.val.Listlen;
        nimpid = lhdr.val.Lnimpid;
        impoff = lhdr.val.Limpoff;
        break;
    }}

    // Read loader import file ID table
    {
        var (_, err) = s.sr.Seek(((int64)impoff), io.SeekStart); if (err != default!) {
            return (default!, err);
        }
    }
    var table = new slice<byte>(istlen);
    {
        var (_, err) = io.ReadFull(~s.sr, table); if (err != default!) {
            return (default!, err);
        }
    }
    nint offset = 0;
    // First import file ID is the default LIBPATH value
    @string libpath = cstring(table[(int)(offset)..]);
    f.LibraryPaths = strings.Split(libpath, ":"u8);
    offset += len(libpath) + 3;
    // 3 null bytes
    var all = new slice<@string>(0);
    for (nint i = 1; i < ((nint)nimpid); i++) {
        @string impidpath = cstring(table[(int)(offset)..]);
        offset += len(impidpath) + 1;
        @string impidbase = cstring(table[(int)(offset)..]);
        offset += len(impidbase) + 1;
        @string impidmem = cstring(table[(int)(offset)..]);
        offset += len(impidmem) + 1;
        @string path = default!;
        if (len(impidpath) > 0){
            path = impidpath + "/"u8 + impidbase + "/"u8 + impidmem;
        } else {
            path = impidbase + "/"u8 + impidmem;
        }
        all = append(all, path);
    }
    return (all, default!);
}

// ImportedSymbols returns the names of all symbols
// referred to by the binary f that are expected to be
// satisfied by other libraries at dynamic load time.
// It does not return weak symbols.
[GoRecv] public static (slice<ImportedSymbol>, error) ImportedSymbols(this ref File f) {
    var s = f.SectionByType(STYP_LOADER);
    if (s == nil) {
        return (default!, default!);
    }
    // Read loader header
    {
        var (_, errΔ1) = (~s).sr.Seek(0, io.SeekStart); if (errΔ1 != default!) {
            return (default!, errΔ1);
        }
    }
    uint32 stlen = default!;
    uint64 stoff = default!;
    uint32 nsyms = default!;
    uint64 symoff = default!;
    switch (f.TargetMachine) {
    case U802TOCMAGIC: {
        var lhdr = @new<LoaderHeader32>();
        {
            var errΔ4 = binary.Read(~(~s).sr, binary.BigEndian, lhdr); if (errΔ4 != default!) {
                return (default!, errΔ4);
            }
        }
        stlen = lhdr.val.Lstlen;
        stoff = ((uint64)(~lhdr).Lstoff);
        nsyms = lhdr.val.Lnsyms;
        symoff = LDHDRSZ_32;
        break;
    }
    case U64_TOCMAGIC: {
        var lhdr = @new<LoaderHeader64>();
        {
            var errΔ5 = binary.Read(~(~s).sr, binary.BigEndian, lhdr); if (errΔ5 != default!) {
                return (default!, errΔ5);
            }
        }
        stlen = lhdr.val.Lstlen;
        stoff = lhdr.val.Lstoff;
        nsyms = lhdr.val.Lnsyms;
        symoff = lhdr.val.Lsymoff;
        break;
    }}

    // Read loader section string table
    {
        var (_, errΔ6) = (~s).sr.Seek(((int64)stoff), io.SeekStart); if (errΔ6 != default!) {
            return (default!, errΔ6);
        }
    }
    var st = new slice<byte>(stlen);
    {
        var (_, errΔ7) = io.ReadFull(~(~s).sr, st); if (errΔ7 != default!) {
            return (default!, errΔ7);
        }
    }
    // Read imported libraries
    (libs, err) = f.readImportIDs(s);
    if (err != default!) {
        return (default!, err);
    }
    // Read loader symbol table
    {
        var (_, errΔ8) = (~s).sr.Seek(((int64)symoff), io.SeekStart); if (errΔ8 != default!) {
            return (default!, errΔ8);
        }
    }
    var all = new slice<ImportedSymbol>(0);
    for (nint i = 0; i < ((nint)nsyms); i++) {
        @string name = default!;
        uint32 ifile = default!;
        bool ok = default!;
        switch (f.TargetMachine) {
        case U802TOCMAGIC: {
            var ldsym = @new<LoaderSymbol32>();
            {
                var errΔ11 = binary.Read(~(~s).sr, binary.BigEndian, ldsym); if (errΔ11 != default!) {
                    return (default!, errΔ11);
                }
            }
            if ((uint8)((~ldsym).Lsmtype & 64) == 0) {
                continue;
            }
            var zeroes = binary.BigEndian.Uint32((~ldsym).Lname[..4]);
            if (zeroes != 0){
                // Imported symbols only
                name = cstring((~ldsym).Lname[..]);
            } else {
                var offset = binary.BigEndian.Uint32((~ldsym).Lname[4..]);
                (name, ok) = getString(st, offset);
                if (!ok) {
                    continue;
                }
            }
            ifile = ldsym.val.Lifile;
            break;
        }
        case U64_TOCMAGIC: {
            var ldsym = @new<LoaderSymbol64>();
            {
                var errΔ12 = binary.Read(~(~s).sr, binary.BigEndian, ldsym); if (errΔ12 != default!) {
                    return (default!, errΔ12);
                }
            }
            if ((uint8)((~ldsym).Lsmtype & 64) == 0) {
                continue;
            }
            (name, ok) = getString(st, // Imported symbols only
 (~ldsym).Loffset);
            if (!ok) {
                continue;
            }
            ifile = ldsym.val.Lifile;
            break;
        }}

        ImportedSymbol sym = default!;
        sym.Name = name;
        if (ifile >= 1 && ((nint)ifile) <= len(libs)) {
            sym.Library = libs[ifile - 1];
        }
        all = append(all, sym);
    }
    return (all, default!);
}

// ImportedLibraries returns the names of all libraries
// referred to by the binary f that are expected to be
// linked with the binary at dynamic link time.
[GoRecv] public static (slice<@string>, error) ImportedLibraries(this ref File f) {
    var s = f.SectionByType(STYP_LOADER);
    if (s == nil) {
        return (default!, default!);
    }
    (all, err) = f.readImportIDs(s);
    return (all, err);
}

} // end xcoff_package
