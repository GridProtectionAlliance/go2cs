// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package xcoff implements access to XCOFF (Extended Common Object File Format) files.
// package xcoff -- go2cs converted at 2022 March 06 22:41:04 UTC
// import "internal/xcoff" ==> using xcoff = go.@internal.xcoff_package
// Original source: C:\Program Files\Go\src\internal\xcoff\file.go
using dwarf = go.debug.dwarf_package;
using binary = go.encoding.binary_package;
using fmt = go.fmt_package;
using io = go.io_package;
using os = go.os_package;
using strings = go.strings_package;

namespace go.@internal;

public static partial class xcoff_package {

    // SectionHeader holds information about an XCOFF section header.
public partial struct SectionHeader {
    public @string Name;
    public ulong VirtualAddress;
    public ulong Size;
    public uint Type;
    public ulong Relptr;
    public uint Nreloc;
}

public partial struct Section : io.ReaderAt {
    public ref SectionHeader SectionHeader => ref SectionHeader_val;
    public slice<Reloc> Relocs;
    public ref io.ReaderAt ReaderAt => ref ReaderAt_val;
    public ptr<io.SectionReader> sr;
}

// AuxiliaryCSect holds information about an XCOFF symbol in an AUX_CSECT entry.
public partial struct AuxiliaryCSect {
    public long Length;
    public nint StorageMappingClass;
    public nint SymbolType;
}

// AuxiliaryFcn holds information about an XCOFF symbol in an AUX_FCN entry.
public partial struct AuxiliaryFcn {
    public long Size;
}

public partial struct Symbol {
    public @string Name;
    public ulong Value;
    public nint SectionNumber;
    public nint StorageClass;
    public AuxiliaryFcn AuxFcn;
    public AuxiliaryCSect AuxCSect;
}

public partial struct Reloc {
    public ulong VirtualAddress;
    public ptr<Symbol> Symbol;
    public bool Signed;
    public bool InstructionFixed;
    public byte Length;
    public byte Type;
}

// ImportedSymbol holds information about an imported XCOFF symbol.
public partial struct ImportedSymbol {
    public @string Name;
    public @string Library;
}

// FileHeader holds information about an XCOFF file header.
public partial struct FileHeader {
    public ushort TargetMachine;
}

// A File represents an open XCOFF file.
public partial struct File {
    public ref FileHeader FileHeader => ref FileHeader_val;
    public slice<ptr<Section>> Sections;
    public slice<ptr<Symbol>> Symbols;
    public slice<byte> StringTable;
    public slice<@string> LibraryPaths;
    public io.Closer closer;
}

// Open opens the named file using os.Open and prepares it for use as an XCOFF binary.
public static (ptr<File>, error) Open(@string name) {
    ptr<File> _p0 = default!;
    error _p0 = default!;

    var (f, err) = os.Open(name);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    var (ff, err) = NewFile(f);
    if (err != null) {
        f.Close();
        return (_addr_null!, error.As(err)!);
    }
    ff.closer = f;
    return (_addr_ff!, error.As(null!)!);

}

// Close closes the File.
// If the File was created using NewFile directly instead of Open,
// Close has no effect.
private static error Close(this ptr<File> _addr_f) {
    ref File f = ref _addr_f.val;

    error err = default!;
    if (f.closer != null) {
        err = error.As(f.closer.Close())!;
        f.closer = null;
    }
    return error.As(err)!;

}

// Section returns the first section with the given name, or nil if no such
// section exists.
// Xcoff have section's name limited to 8 bytes. Some sections like .gosymtab
// can be trunked but this method will still find them.
private static ptr<Section> Section(this ptr<File> _addr_f, @string name) {
    ref File f = ref _addr_f.val;

    foreach (var (_, s) in f.Sections) {
        if (s.Name == name || (len(name) > 8 && s.Name == name[..(int)8])) {
            return _addr_s!;
        }
    }    return _addr_null!;

}

// SectionByType returns the first section in f with the
// given type, or nil if there is no such section.
private static ptr<Section> SectionByType(this ptr<File> _addr_f, uint typ) {
    ref File f = ref _addr_f.val;

    foreach (var (_, s) in f.Sections) {
        if (s.Type == typ) {
            return _addr_s!;
        }
    }    return _addr_null!;

}

// cstring converts ASCII byte sequence b to string.
// It stops once it finds 0 or reaches end of b.
private static @string cstring(slice<byte> b) {
    nint i = default;
    for (i = 0; i < len(b) && b[i] != 0; i++)     }
    return string(b[..(int)i]);
}

// getString extracts a string from an XCOFF string table.
private static (@string, bool) getString(slice<byte> st, uint offset) {
    @string _p0 = default;
    bool _p0 = default;

    if (offset < 4 || int(offset) >= len(st)) {
        return ("", false);
    }
    return (cstring(st[(int)offset..]), true);

}

// NewFile creates a new File for accessing an XCOFF binary in an underlying reader.
public static (ptr<File>, error) NewFile(io.ReaderAt r) {
    ptr<File> _p0 = default!;
    error _p0 = default!;

    var sr = io.NewSectionReader(r, 0, 1 << 63 - 1); 
    // Read XCOFF target machine
    ref ushort magic = ref heap(out ptr<ushort> _addr_magic);
    {
        var err__prev1 = err;

        var err = binary.Read(sr, binary.BigEndian, _addr_magic);

        if (err != null) {
            return (_addr_null!, error.As(err)!);
        }
        err = err__prev1;

    }

    if (magic != U802TOCMAGIC && magic != U64_TOCMAGIC) {
        return (_addr_null!, error.As(fmt.Errorf("unrecognised XCOFF magic: 0x%x", magic))!);
    }
    ptr<File> f = @new<File>();
    f.TargetMachine = magic; 

    // Read XCOFF file header
    {
        var err__prev1 = err;

        var (_, err) = sr.Seek(0, os.SEEK_SET);

        if (err != null) {
            return (_addr_null!, error.As(err)!);
        }
        err = err__prev1;

    }

    ushort nscns = default;
    ulong symptr = default;
    int nsyms = default;
    ushort opthdr = default;
    nint hdrsz = default;

    if (f.TargetMachine == U802TOCMAGIC) 
        ptr<object> fhdr = @new<FileHeader32>();
        {
            var err__prev1 = err;

            err = binary.Read(sr, binary.BigEndian, fhdr);

            if (err != null) {
                return (_addr_null!, error.As(err)!);
            }

            err = err__prev1;

        }

        nscns = fhdr.Fnscns;
        symptr = uint64(fhdr.Fsymptr);
        nsyms = fhdr.Fnsyms;
        opthdr = fhdr.Fopthdr;
        hdrsz = FILHSZ_32;
    else if (f.TargetMachine == U64_TOCMAGIC) 
        fhdr = @new<FileHeader64>();
        {
            var err__prev1 = err;

            err = binary.Read(sr, binary.BigEndian, fhdr);

            if (err != null) {
                return (_addr_null!, error.As(err)!);
            }

            err = err__prev1;

        }

        nscns = fhdr.Fnscns;
        symptr = fhdr.Fsymptr;
        nsyms = fhdr.Fnsyms;
        opthdr = fhdr.Fopthdr;
        hdrsz = FILHSZ_64;
        if (symptr == 0 || nsyms <= 0) {
        return (_addr_null!, error.As(fmt.Errorf("no symbol table"))!);
    }
    var offset = symptr + uint64(nsyms) * SYMESZ;
    {
        var err__prev1 = err;

        (_, err) = sr.Seek(int64(offset), os.SEEK_SET);

        if (err != null) {
            return (_addr_null!, error.As(err)!);
        }
        err = err__prev1;

    } 
    // The first 4 bytes contain the length (in bytes).
    ref uint l = ref heap(out ptr<uint> _addr_l);
    {
        var err__prev1 = err;

        err = binary.Read(sr, binary.BigEndian, _addr_l);

        if (err != null) {
            return (_addr_null!, error.As(err)!);
        }
        err = err__prev1;

    }

    if (l > 4) {
        {
            var err__prev2 = err;

            (_, err) = sr.Seek(int64(offset), os.SEEK_SET);

            if (err != null) {
                return (_addr_null!, error.As(err)!);
            }

            err = err__prev2;

        }

        f.StringTable = make_slice<byte>(l);
        {
            var err__prev2 = err;

            (_, err) = io.ReadFull(sr, f.StringTable);

            if (err != null) {
                return (_addr_null!, error.As(err)!);
            }

            err = err__prev2;

        }

    }
    {
        var err__prev1 = err;

        (_, err) = sr.Seek(int64(hdrsz) + int64(opthdr), os.SEEK_SET);

        if (err != null) {
            return (_addr_null!, error.As(err)!);
        }
        err = err__prev1;

    }

    f.Sections = make_slice<ptr<Section>>(nscns);
    {
        nint i__prev1 = i;

        for (nint i = 0; i < int(nscns); i++) {
            ulong scnptr = default;
            ptr<Section> s = @new<Section>();

            if (f.TargetMachine == U802TOCMAGIC) 
                ptr<object> shdr = @new<SectionHeader32>();
                {
                    var err__prev1 = err;

                    err = binary.Read(sr, binary.BigEndian, shdr);

                    if (err != null) {
                        return (_addr_null!, error.As(err)!);
                    }

                    err = err__prev1;

                }

                s.Name = cstring(shdr.Sname[..]);
                s.VirtualAddress = uint64(shdr.Svaddr);
                s.Size = uint64(shdr.Ssize);
                scnptr = uint64(shdr.Sscnptr);
                s.Type = shdr.Sflags;
                s.Relptr = uint64(shdr.Srelptr);
                s.Nreloc = uint32(shdr.Snreloc);
            else if (f.TargetMachine == U64_TOCMAGIC) 
                shdr = @new<SectionHeader64>();
                {
                    var err__prev1 = err;

                    err = binary.Read(sr, binary.BigEndian, shdr);

                    if (err != null) {
                        return (_addr_null!, error.As(err)!);
                    }

                    err = err__prev1;

                }

                s.Name = cstring(shdr.Sname[..]);
                s.VirtualAddress = shdr.Svaddr;
                s.Size = shdr.Ssize;
                scnptr = shdr.Sscnptr;
                s.Type = shdr.Sflags;
                s.Relptr = shdr.Srelptr;
                s.Nreloc = shdr.Snreloc;
                        var r2 = r;
            if (scnptr == 0) { // .bss must have all 0s
                r2 = new zeroReaderAt();

            }

            s.sr = io.NewSectionReader(r2, int64(scnptr), int64(s.Size));
            s.ReaderAt = s.sr;
            f.Sections[i] = s;

        }

        i = i__prev1;
    } 

    // Symbol map needed by relocation
    var idxToSym = make_map<nint, ptr<Symbol>>(); 

    // Read symbol table
    {
        var err__prev1 = err;

        (_, err) = sr.Seek(int64(symptr), os.SEEK_SET);

        if (err != null) {
            return (_addr_null!, error.As(err)!);
        }
        err = err__prev1;

    }

    f.Symbols = make_slice<ptr<Symbol>>(0);
    {
        nint i__prev1 = i;

        for (i = 0; i < int(nsyms); i++) {
            nint numaux = default;
            bool ok = default;            bool needAuxFcn = default;

            ptr<Symbol> sym = @new<Symbol>();

            if (f.TargetMachine == U802TOCMAGIC) 
                ptr<object> se = @new<SymEnt32>();
                {
                    var err__prev1 = err;

                    err = binary.Read(sr, binary.BigEndian, se);

                    if (err != null) {
                        return (_addr_null!, error.As(err)!);
                    }

                    err = err__prev1;

                }

                numaux = int(se.Nnumaux);
                sym.SectionNumber = int(se.Nscnum);
                sym.StorageClass = int(se.Nsclass);
                sym.Value = uint64(se.Nvalue);
                needAuxFcn = se.Ntype & SYM_TYPE_FUNC != 0 && numaux > 1;
                var zeroes = binary.BigEndian.Uint32(se.Nname[..(int)4]);
                if (zeroes != 0) {
                    sym.Name = cstring(se.Nname[..]);
                }
                else
 {
                    offset = binary.BigEndian.Uint32(se.Nname[(int)4..]);
                    sym.Name, ok = getString(f.StringTable, offset);
                    if (!ok) {
                        goto skip;
                    }
                }

            else if (f.TargetMachine == U64_TOCMAGIC) 
                se = @new<SymEnt64>();
                {
                    var err__prev1 = err;

                    err = binary.Read(sr, binary.BigEndian, se);

                    if (err != null) {
                        return (_addr_null!, error.As(err)!);
                    }

                    err = err__prev1;

                }

                numaux = int(se.Nnumaux);
                sym.SectionNumber = int(se.Nscnum);
                sym.StorageClass = int(se.Nsclass);
                sym.Value = se.Nvalue;
                needAuxFcn = se.Ntype & SYM_TYPE_FUNC != 0 && numaux > 1;
                sym.Name, ok = getString(f.StringTable, se.Noffset);
                if (!ok) {
                    goto skip;
                }

                        if (sym.StorageClass != C_EXT && sym.StorageClass != C_WEAKEXT && sym.StorageClass != C_HIDEXT) {
                goto skip;
            } 
            // Must have at least one csect auxiliary entry.
            if (numaux < 1 || i + numaux >= int(nsyms)) {
                goto skip;
            }

            if (sym.SectionNumber > int(nscns)) {
                goto skip;
            }

            if (sym.SectionNumber == 0) {
                sym.Value = 0;
            }
            else
 {
                sym.Value -= f.Sections[sym.SectionNumber - 1].VirtualAddress;
            }

            idxToSym[i] = sym; 

            // If this symbol is a function, it must retrieve its size from
            // its AUX_FCN entry.
            // It can happen that a function symbol doesn't have any AUX_FCN.
            // In this case, needAuxFcn is false and their size will be set to 0.
            if (needAuxFcn) {

                if (f.TargetMachine == U802TOCMAGIC) 
                    ptr<object> aux = @new<AuxFcn32>();
                    {
                        var err__prev2 = err;

                        err = binary.Read(sr, binary.BigEndian, aux);

                        if (err != null) {
                            return (_addr_null!, error.As(err)!);
                        }

                        err = err__prev2;

                    }

                    sym.AuxFcn.Size = int64(aux.Xfsize);
                else if (f.TargetMachine == U64_TOCMAGIC) 
                    aux = @new<AuxFcn64>();
                    {
                        var err__prev2 = err;

                        err = binary.Read(sr, binary.BigEndian, aux);

                        if (err != null) {
                            return (_addr_null!, error.As(err)!);
                        }

                        err = err__prev2;

                    }

                    sym.AuxFcn.Size = int64(aux.Xfsize);
                
            } 

            // Read csect auxiliary entry (by convention, it is the last).
            if (!needAuxFcn) {
                {
                    var err__prev2 = err;

                    (_, err) = sr.Seek(int64(numaux - 1) * SYMESZ, os.SEEK_CUR);

                    if (err != null) {
                        return (_addr_null!, error.As(err)!);
                    }

                    err = err__prev2;

                }

            }

            i += numaux;
            numaux = 0;

            if (f.TargetMachine == U802TOCMAGIC) 
                aux = @new<AuxCSect32>();
                {
                    var err__prev1 = err;

                    err = binary.Read(sr, binary.BigEndian, aux);

                    if (err != null) {
                        return (_addr_null!, error.As(err)!);
                    }

                    err = err__prev1;

                }

                sym.AuxCSect.SymbolType = int(aux.Xsmtyp & 0x7);
                sym.AuxCSect.StorageMappingClass = int(aux.Xsmclas);
                sym.AuxCSect.Length = int64(aux.Xscnlen);
            else if (f.TargetMachine == U64_TOCMAGIC) 
                aux = @new<AuxCSect64>();
                {
                    var err__prev1 = err;

                    err = binary.Read(sr, binary.BigEndian, aux);

                    if (err != null) {
                        return (_addr_null!, error.As(err)!);
                    }

                    err = err__prev1;

                }

                sym.AuxCSect.SymbolType = int(aux.Xsmtyp & 0x7);
                sym.AuxCSect.StorageMappingClass = int(aux.Xsmclas);
                sym.AuxCSect.Length = int64(aux.Xscnlenhi) << 32 | int64(aux.Xscnlenlo);
                        f.Symbols = append(f.Symbols, sym);
skip: // Skip auxiliary entries
            i += numaux; // Skip auxiliary entries
            {
                var err__prev1 = err;

                (_, err) = sr.Seek(int64(numaux) * SYMESZ, os.SEEK_CUR);

                if (err != null) {
                    return (_addr_null!, error.As(err)!);
                }

                err = err__prev1;

            }

        }

        i = i__prev1;
    } 

    // Read relocations
    // Only for .data or .text section
    foreach (var (_, sect) in f.Sections) {
        if (sect.Type != STYP_TEXT && sect.Type != STYP_DATA) {
            continue;
        }
        sect.Relocs = make_slice<Reloc>(sect.Nreloc);
        if (sect.Relptr == 0) {
            continue;
        }
        {
            var err__prev1 = err;

            (_, err) = sr.Seek(int64(sect.Relptr), os.SEEK_SET);

            if (err != null) {
                return (_addr_null!, error.As(err)!);
            }

            err = err__prev1;

        }

        {
            nint i__prev2 = i;

            for (i = uint32(0); i < sect.Nreloc; i++) {

                if (f.TargetMachine == U802TOCMAGIC) 
                    ptr<object> rel = @new<Reloc32>();
                    {
                        var err__prev1 = err;

                        err = binary.Read(sr, binary.BigEndian, rel);

                        if (err != null) {
                            return (_addr_null!, error.As(err)!);
                        }

                        err = err__prev1;

                    }

                    sect.Relocs[i].VirtualAddress = uint64(rel.Rvaddr);
                    sect.Relocs[i].Symbol = idxToSym[int(rel.Rsymndx)];
                    sect.Relocs[i].Type = rel.Rtype;
                    sect.Relocs[i].Length = rel.Rsize & 0x3F + 1;

                    if (rel.Rsize & 0x80 != 0) {
                        sect.Relocs[i].Signed = true;
                    }

                    if (rel.Rsize & 0x40 != 0) {
                        sect.Relocs[i].InstructionFixed = true;
                    }

                else if (f.TargetMachine == U64_TOCMAGIC) 
                    rel = @new<Reloc64>();
                    {
                        var err__prev1 = err;

                        err = binary.Read(sr, binary.BigEndian, rel);

                        if (err != null) {
                            return (_addr_null!, error.As(err)!);
                        }

                        err = err__prev1;

                    }

                    sect.Relocs[i].VirtualAddress = rel.Rvaddr;
                    sect.Relocs[i].Symbol = idxToSym[int(rel.Rsymndx)];
                    sect.Relocs[i].Type = rel.Rtype;
                    sect.Relocs[i].Length = rel.Rsize & 0x3F + 1;
                    if (rel.Rsize & 0x80 != 0) {
                        sect.Relocs[i].Signed = true;
                    }

                    if (rel.Rsize & 0x40 != 0) {
                        sect.Relocs[i].InstructionFixed = true;
                    }

                            }


            i = i__prev2;
        }

    }    return (_addr_f!, error.As(null!)!);

}

// zeroReaderAt is ReaderAt that reads 0s.
private partial struct zeroReaderAt {
}

// ReadAt writes len(p) 0s into p.
private static (nint, error) ReadAt(this zeroReaderAt w, slice<byte> p, long off) {
    nint n = default;
    error err = default!;

    foreach (var (i) in p) {
        p[i] = 0;
    }    return (len(p), error.As(null!)!);
}

// Data reads and returns the contents of the XCOFF section s.
private static (slice<byte>, error) Data(this ptr<Section> _addr_s) {
    slice<byte> _p0 = default;
    error _p0 = default!;
    ref Section s = ref _addr_s.val;

    var dat = make_slice<byte>(s.sr.Size());
    var (n, err) = s.sr.ReadAt(dat, 0);
    if (n == len(dat)) {
        err = null;
    }
    return (dat[..(int)n], error.As(err)!);

}

// CSect reads and returns the contents of a csect.
private static slice<byte> CSect(this ptr<File> _addr_f, @string name) {
    ref File f = ref _addr_f.val;

    foreach (var (_, sym) in f.Symbols) {
        if (sym.Name == name && sym.AuxCSect.SymbolType == XTY_SD) {
            {
                var i = sym.SectionNumber - 1;

                if (0 <= i && i < len(f.Sections)) {
                    var s = f.Sections[i];
                    if (sym.Value + uint64(sym.AuxCSect.Length) <= s.Size) {
                        var dat = make_slice<byte>(sym.AuxCSect.Length);
                        var (_, err) = s.sr.ReadAt(dat, int64(sym.Value));
                        if (err != null) {
                            return null;
                        }
                        return dat;
                    }
                }

            }

            break;

        }
    }    return null;

}

private static (ptr<dwarf.Data>, error) DWARF(this ptr<File> _addr_f) {
    ptr<dwarf.Data> _p0 = default!;
    error _p0 = default!;
    ref File f = ref _addr_f.val;
 
    // There are many other DWARF sections, but these
    // are the ones the debug/dwarf package uses.
    // Don't bother loading others.
    array<uint> subtypes = new array<uint>(new uint[] { SSUBTYP_DWABREV, SSUBTYP_DWINFO, SSUBTYP_DWLINE, SSUBTYP_DWRNGES, SSUBTYP_DWSTR });
    array<slice<byte>> dat = new array<slice<byte>>(len(subtypes));
    foreach (var (i, subtype) in subtypes) {
        var s = f.SectionByType(STYP_DWARF | subtype);
        if (s != null) {
            var (b, err) = s.Data();
            if (err != null && uint64(len(b)) < s.Size) {
                return (_addr_null!, error.As(err)!);
            }
            dat[i] = b;
        }
    }    var abbrev = dat[0];
    var info = dat[1];
    var line = dat[2];
    var ranges = dat[3];
    var str = dat[4];
    return _addr_dwarf.New(abbrev, null, null, info, line, null, ranges, str)!;

}

// readImportID returns the import file IDs stored inside the .loader section.
// Library name pattern is either path/base/member or base/member
private static (slice<@string>, error) readImportIDs(this ptr<File> _addr_f, ptr<Section> _addr_s) {
    slice<@string> _p0 = default;
    error _p0 = default!;
    ref File f = ref _addr_f.val;
    ref Section s = ref _addr_s.val;
 
    // Read loader header
    {
        var err__prev1 = err;

        var (_, err) = s.sr.Seek(0, os.SEEK_SET);

        if (err != null) {
            return (null, error.As(err)!);
        }
        err = err__prev1;

    }

    uint istlen = default;
    int nimpid = default;
    ulong impoff = default;

    if (f.TargetMachine == U802TOCMAGIC) 
        ptr<object> lhdr = @new<LoaderHeader32>();
        {
            var err__prev1 = err;

            var err = binary.Read(s.sr, binary.BigEndian, lhdr);

            if (err != null) {
                return (null, error.As(err)!);
            }

            err = err__prev1;

        }

        istlen = lhdr.Listlen;
        nimpid = lhdr.Lnimpid;
        impoff = uint64(lhdr.Limpoff);
    else if (f.TargetMachine == U64_TOCMAGIC) 
        lhdr = @new<LoaderHeader64>();
        {
            var err__prev1 = err;

            err = binary.Read(s.sr, binary.BigEndian, lhdr);

            if (err != null) {
                return (null, error.As(err)!);
            }

            err = err__prev1;

        }

        istlen = lhdr.Listlen;
        nimpid = lhdr.Lnimpid;
        impoff = lhdr.Limpoff;
    // Read loader import file ID table
    {
        var err__prev1 = err;

        (_, err) = s.sr.Seek(int64(impoff), os.SEEK_SET);

        if (err != null) {
            return (null, error.As(err)!);
        }
        err = err__prev1;

    }

    var table = make_slice<byte>(istlen);
    {
        var err__prev1 = err;

        (_, err) = io.ReadFull(s.sr, table);

        if (err != null) {
            return (null, error.As(err)!);
        }
        err = err__prev1;

    }


    nint offset = 0; 
    // First import file ID is the default LIBPATH value
    var libpath = cstring(table[(int)offset..]);
    f.LibraryPaths = strings.Split(libpath, ":");
    offset += len(libpath) + 3; // 3 null bytes
    var all = make_slice<@string>(0);
    for (nint i = 1; i < int(nimpid); i++) {
        var impidpath = cstring(table[(int)offset..]);
        offset += len(impidpath) + 1;
        var impidbase = cstring(table[(int)offset..]);
        offset += len(impidbase) + 1;
        var impidmem = cstring(table[(int)offset..]);
        offset += len(impidmem) + 1;
        @string path = default;
        if (len(impidpath) > 0) {
            path = impidpath + "/" + impidbase + "/" + impidmem;
        }
        else
 {
            path = impidbase + "/" + impidmem;
        }
        all = append(all, path);

    }

    return (all, error.As(null!)!);

}

// ImportedSymbols returns the names of all symbols
// referred to by the binary f that are expected to be
// satisfied by other libraries at dynamic load time.
// It does not return weak symbols.
private static (slice<ImportedSymbol>, error) ImportedSymbols(this ptr<File> _addr_f) {
    slice<ImportedSymbol> _p0 = default;
    error _p0 = default!;
    ref File f = ref _addr_f.val;

    var s = f.SectionByType(STYP_LOADER);
    if (s == null) {
        return (null, error.As(null!)!);
    }
    {
        var err__prev1 = err;

        var (_, err) = s.sr.Seek(0, os.SEEK_SET);

        if (err != null) {
            return (null, error.As(err)!);
        }
        err = err__prev1;

    }

    uint stlen = default;
    ulong stoff = default;
    int nsyms = default;
    ulong symoff = default;

    if (f.TargetMachine == U802TOCMAGIC) 
        ptr<object> lhdr = @new<LoaderHeader32>();
        {
            var err__prev1 = err;

            var err = binary.Read(s.sr, binary.BigEndian, lhdr);

            if (err != null) {
                return (null, error.As(err)!);
            }

            err = err__prev1;

        }

        stlen = lhdr.Lstlen;
        stoff = uint64(lhdr.Lstoff);
        nsyms = lhdr.Lnsyms;
        symoff = LDHDRSZ_32;
    else if (f.TargetMachine == U64_TOCMAGIC) 
        lhdr = @new<LoaderHeader64>();
        {
            var err__prev1 = err;

            err = binary.Read(s.sr, binary.BigEndian, lhdr);

            if (err != null) {
                return (null, error.As(err)!);
            }

            err = err__prev1;

        }

        stlen = lhdr.Lstlen;
        stoff = lhdr.Lstoff;
        nsyms = lhdr.Lnsyms;
        symoff = lhdr.Lsymoff;
    // Read loader section string table
    {
        var err__prev1 = err;

        (_, err) = s.sr.Seek(int64(stoff), os.SEEK_SET);

        if (err != null) {
            return (null, error.As(err)!);
        }
        err = err__prev1;

    }

    var st = make_slice<byte>(stlen);
    {
        var err__prev1 = err;

        (_, err) = io.ReadFull(s.sr, st);

        if (err != null) {
            return (null, error.As(err)!);
        }
        err = err__prev1;

    } 

    // Read imported libraries
    var (libs, err) = f.readImportIDs(s);
    if (err != null) {
        return (null, error.As(err)!);
    }
    {
        var err__prev1 = err;

        (_, err) = s.sr.Seek(int64(symoff), os.SEEK_SET);

        if (err != null) {
            return (null, error.As(err)!);
        }
        err = err__prev1;

    }

    var all = make_slice<ImportedSymbol>(0);
    for (nint i = 0; i < int(nsyms); i++) {
        @string name = default;
        int ifile = default;
        bool ok = default;

        if (f.TargetMachine == U802TOCMAGIC) 
            ptr<object> ldsym = @new<LoaderSymbol32>();
            {
                var err__prev1 = err;

                err = binary.Read(s.sr, binary.BigEndian, ldsym);

                if (err != null) {
                    return (null, error.As(err)!);
                }

                err = err__prev1;

            }

            if (ldsym.Lsmtype & 0x40 == 0) {
                continue; // Imported symbols only
            }

            var zeroes = binary.BigEndian.Uint32(ldsym.Lname[..(int)4]);
            if (zeroes != 0) {
                name = cstring(ldsym.Lname[..]);
            }
            else
 {
                var offset = binary.BigEndian.Uint32(ldsym.Lname[(int)4..]);
                name, ok = getString(st, offset);
                if (!ok) {
                    continue;
                }
            }

            ifile = ldsym.Lifile;
        else if (f.TargetMachine == U64_TOCMAGIC) 
            ldsym = @new<LoaderSymbol64>();
            {
                var err__prev1 = err;

                err = binary.Read(s.sr, binary.BigEndian, ldsym);

                if (err != null) {
                    return (null, error.As(err)!);
                }

                err = err__prev1;

            }

            if (ldsym.Lsmtype & 0x40 == 0) {
                continue; // Imported symbols only
            }

            name, ok = getString(st, ldsym.Loffset);
            if (!ok) {
                continue;
            }

            ifile = ldsym.Lifile;
                ImportedSymbol sym = default;
        sym.Name = name;
        if (ifile >= 1 && int(ifile) <= len(libs)) {
            sym.Library = libs[ifile - 1];
        }
        all = append(all, sym);

    }

    return (all, error.As(null!)!);

}

// ImportedLibraries returns the names of all libraries
// referred to by the binary f that are expected to be
// linked with the binary at dynamic link time.
private static (slice<@string>, error) ImportedLibraries(this ptr<File> _addr_f) {
    slice<@string> _p0 = default;
    error _p0 = default!;
    ref File f = ref _addr_f.val;

    var s = f.SectionByType(STYP_LOADER);
    if (s == null) {
        return (null, error.As(null!)!);
    }
    var (all, err) = f.readImportIDs(s);
    return (all, error.As(err)!);

}

} // end xcoff_package
