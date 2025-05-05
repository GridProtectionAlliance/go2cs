// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

/*
Package elf implements access to ELF object files.

# Security

This package is not designed to be hardened against adversarial inputs, and is
outside the scope of https://go.dev/security/policy. In particular, only basic
validation is done when parsing object files. As such, care should be taken when
parsing untrusted inputs, as parsing malformed files may consume significant
resources, or cause panics.
*/
namespace go.debug;

using bytes = bytes_package;
using zlib = compress.zlib_package;
using dwarf = debug.dwarf_package;
using binary = encoding.binary_package;
using errors = errors_package;
using fmt = fmt_package;
using saferio = @internal.saferio_package;
using zstd = @internal.zstd_package;
using io = io_package;
using os = os_package;
using strings = strings_package;
using @unsafe = unsafe_package;
using @internal;
using compress;
using encoding;

partial class elf_package {

// TODO: error reporting detail
/*
 * Internal ELF representation
 */

// A FileHeader represents an ELF file header.
[GoType] partial struct FileHeader {
    public Class Class;
    public ΔData Data;
    public Version Version;
    public OSABI OSABI;
    public uint8 ABIVersion;
    public encoding.binary_package.ByteOrder ByteOrder;
    public Type Type;
    public Machine Machine;
    public uint64 Entry;
}

// A File represents an open ELF file.
[GoType] partial struct File {
    public partial ref FileHeader FileHeader { get; }
    public slice<ж<ΔSection>> Sections;
    public slice<ж<Prog>> Progs;
    internal io_package.Closer closer;
    internal slice<verneed> gnuNeed;
    internal slice<byte> gnuVersym;
}

// A SectionHeader represents a single ELF section header.
[GoType] partial struct SectionHeader {
    public @string Name;
    public SectionType Type;
    public SectionFlag Flags;
    public uint64 Addr;
    public uint64 Offset;
    public uint64 Size;
    public uint32 Link;
    public uint32 Info;
    public uint64 Addralign;
    public uint64 Entsize;
    // FileSize is the size of this section in the file in bytes.
    // If a section is compressed, FileSize is the size of the
    // compressed data, while Size (above) is the size of the
    // uncompressed data.
    public uint64 FileSize;
}

// A Section represents a single section in an ELF file.
[GoType] partial struct ΔSection {
    public partial ref SectionHeader SectionHeader { get; }
    // Embed ReaderAt for ReadAt method.
    // Do not embed SectionReader directly
    // to avoid having Read and Seek.
    // If a client wants Read and Seek it must use
    // Open() to avoid fighting over the seek offset
    // with other clients.
    //
    // ReaderAt may be nil if the section is not easily available
    // in a random-access form. For example, a compressed section
    // may have a nil ReaderAt.
    public partial ref io_package.ReaderAt ReaderAt { get; }
    internal ж<io_package.SectionReader> sr;
    internal CompressionType compressionType;
    internal int64 compressionOffset;
}

// Data reads and returns the contents of the ELF section.
// Even if the section is stored compressed in the ELF file,
// Data returns uncompressed data.
//
// For an [SHT_NOBITS] section, Data always returns a non-nil error.
[GoRecv] public static (slice<byte>, error) Data(this ref ΔSection s) {
    return saferio.ReadData(s.Open(), s.Size);
}

// stringTable reads and returns the string table given by the
// specified link value.
[GoRecv] internal static (slice<byte>, error) stringTable(this ref File f, uint32 link) {
    if (link <= 0 || link >= ((uint32)len(f.Sections))) {
        return (default!, errors.New("section has invalid string table link"u8));
    }
    return f.Sections[link].Data();
}

// Open returns a new ReadSeeker reading the ELF section.
// Even if the section is stored compressed in the ELF file,
// the ReadSeeker reads uncompressed data.
//
// For an [SHT_NOBITS] section, all calls to the opened reader
// will return a non-nil error.
[GoRecv] public static io.ReadSeeker Open(this ref ΔSection s) {
    if (s.Type == SHT_NOBITS) {
        return ~io.NewSectionReader(new nobitsSectionReader(nil), 0, ((int64)s.Size));
    }
    Func<io.Reader, (io.ReadCloser, error)> zrd = default!;
    if ((SectionFlag)(s.Flags & SHF_COMPRESSED) == 0){
        if (!strings.HasPrefix(s.Name, ".zdebug"u8)) {
            return ~io.NewSectionReader(~s.sr, 0, 1 << (int)(63) - 1);
        }
        var b = new slice<byte>(12);
        var (n, _) = s.sr.ReadAt(b, 0);
        if (n != 12 || ((@string)(b[..4])) != "ZLIB"u8) {
            return ~io.NewSectionReader(~s.sr, 0, 1 << (int)(63) - 1);
        }
        s.compressionOffset = 12;
        s.compressionType = COMPRESS_ZLIB;
        s.Size = binary.BigEndian.Uint64(b[4..12]);
        zrd = zlib.NewReader;
    } else 
    if ((SectionFlag)(s.Flags & SHF_ALLOC) != 0) {
        return new errorReader(Ꮡ(new FormatError(((int64)s.Offset),
            "SHF_COMPRESSED applies only to non-allocable sections", s.compressionType))
        );
    }
    var exprᴛ1 = s.compressionType;
    if (exprᴛ1 == COMPRESS_ZLIB) {
        zrd = zlib.NewReader;
    }
    else if (exprᴛ1 == COMPRESS_ZSTD) {
        zrd = (io.Reader r) => (io.NopCloser(~zstd.NewReader(r)), default!);
    }

    if (zrd == default!) {
        return new errorReader(Ꮡ(new FormatError(((int64)s.Offset), "unknown compression type", s.compressionType)));
    }
    return new readSeekerFromReader(
        reset: 
        var zrdʗ1 = zrd;
        () => {
            var fr = io.NewSectionReader(~s.sr, s.compressionOffset, ((int64)s.FileSize) - s.compressionOffset);
            return zrdʗ1(~fr);
        },
        size: ((int64)s.Size)
    );
}

// A ProgHeader represents a single ELF program header.
[GoType] partial struct ProgHeader {
    public ProgType Type;
    public ProgFlag Flags;
    public uint64 Off;
    public uint64 Vaddr;
    public uint64 Paddr;
    public uint64 Filesz;
    public uint64 Memsz;
    public uint64 Align;
}

// A Prog represents a single ELF program header in an ELF binary.
[GoType] partial struct Prog {
    public partial ref ProgHeader ProgHeader { get; }
    // Embed ReaderAt for ReadAt method.
    // Do not embed SectionReader directly
    // to avoid having Read and Seek.
    // If a client wants Read and Seek it must use
    // Open() to avoid fighting over the seek offset
    // with other clients.
    public partial ref io_package.ReaderAt ReaderAt { get; }
    internal ж<io_package.SectionReader> sr;
}

// Open returns a new ReadSeeker reading the ELF program body.
[GoRecv] public static io.ReadSeeker Open(this ref Prog p) {
    return ~io.NewSectionReader(~p.sr, 0, 1 << (int)(63) - 1);
}

// A Symbol represents an entry in an ELF symbol table section.
[GoType] partial struct Symbol {
    public @string Name;
    public byte Info;
    public byte Other;
    public SectionIndex Section;
    public uint64 Value;
    public uint64 Size;
    // Version and Library are present only for the dynamic symbol
    // table.
    public @string Version;
    public @string Library;
}

/*
 * ELF reader
 */
[GoType] partial struct FormatError {
    internal int64 off;
    internal @string msg;
    internal any val;
}

[GoRecv] public static @string Error(this ref FormatError e) {
    @string msg = e.msg;
    if (e.val != default!) {
        msg += fmt.Sprintf(" '%v' "u8, e.val);
    }
    msg += fmt.Sprintf("in record at byte %#x"u8, e.off);
    return msg;
}

// Open opens the named file using [os.Open] and prepares it for use as an ELF binary.
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

// Close closes the [File].
// If the [File] was created using [NewFile] directly instead of [Open],
// Close has no effect.
[GoRecv] public static error Close(this ref File f) {
    error err = default!;
    if (f.closer != default!) {
        err = f.closer.Close();
        f.closer = default!;
    }
    return err;
}

// SectionByType returns the first section in f with the
// given type, or nil if there is no such section.
[GoRecv] public static ж<ΔSection> SectionByType(this ref File f, SectionType typ) {
    foreach (var (_, s) in f.Sections) {
        if (s.Type == typ) {
            return s;
        }
    }
    return default!;
}

// NewFile creates a new [File] for accessing an ELF binary in an underlying reader.
// The ELF binary is expected to start at position 0 in the ReaderAt.
public static (ж<File>, error) NewFile(io.ReaderAt r) {
    var sr = io.NewSectionReader(r, 0, 1 << (int)(63) - 1);
    // Read and decode ELF identifier
    array<uint8> ident = new(16);
    {
        var (_, errΔ1) = r.ReadAt(ident[0..], 0); if (errΔ1 != default!) {
            return (default!, errΔ1);
        }
    }
    if (ident[0] != (rune)'\x7f' || ident[1] != (rune)'E' || ident[2] != (rune)'L' || ident[3] != (rune)'F') {
        return (default!, new FormatError(0, "bad magic number", ident[0..4]));
    }
    var f = @new<File>();
    f.Class = ((Class)ident[EI_CLASS]);
    var exprᴛ1 = f.Class;
    if (exprᴛ1 == ELFCLASS32) {
    }
    else if (exprᴛ1 == ELFCLASS64) {
    }
    else { /* default: */
        return (default!, new FormatError( // ok
0, "unknown ELF class", f.Class));
    }

    f.Data = ((ΔData)ident[EI_DATA]);
    binary.ByteOrder bo = default!;
    var exprᴛ2 = f.Data;
    if (exprᴛ2 == ELFDATA2LSB) {
        bo = binary.LittleEndian;
    }
    else if (exprᴛ2 == ELFDATA2MSB) {
        bo = binary.BigEndian;
    }
    else { /* default: */
        return (default!, new FormatError(0, "unknown ELF data encoding", f.Data));
    }

    f.ByteOrder = bo;
    f.Version = ((Version)ident[EI_VERSION]);
    if (f.Version != EV_CURRENT) {
        return (default!, new FormatError(0, "unknown ELF version", f.Version));
    }
    f.OSABI = ((OSABI)ident[EI_OSABI]);
    f.ABIVersion = ident[EI_ABIVERSION];
    // Read ELF file header
    ref var phoff = ref heap(new int64(), out var Ꮡphoff);
    ref var phentsize = ref heap(new nint(), out var Ꮡphentsize);
    nint phnum = default!;
    ref var shoff = ref heap(new int64(), out var Ꮡshoff);
    ref var shentsize = ref heap(new nint(), out var Ꮡshentsize);
    ref var shnum = ref heap(new nint(), out var Ꮡshnum);
    ref var shstrndx = ref heap(new nint(), out var Ꮡshstrndx);
    var exprᴛ3 = f.Class;
    if (exprᴛ3 == ELFCLASS32) {
        Header32 hdrΔ2 = default!;
        var data = new slice<byte>(@unsafe.Sizeof(hdrΔ2));
        {
            var (_, errΔ4) = sr.ReadAt(data, 0); if (errΔ4 != default!) {
                return (default!, errΔ4);
            }
        }
        f.Type = ((Type)bo.Uint16(data[(int)(@unsafe.Offsetof(hdrΔ2.GetType(), "Type"))..]));
        f.Machine = ((Machine)bo.Uint16(data[(int)(@unsafe.Offsetof(hdrΔ2.GetType(), "Machine"))..]));
        f.Entry = ((uint64)bo.Uint32(data[(int)(@unsafe.Offsetof(hdrΔ2.GetType(), "Entry"))..]));
        {
            ref var v = ref heap<Version>(out var Ꮡv);
            v = ((Version)bo.Uint32(data[(int)(@unsafe.Offsetof(hdrΔ2.GetType(), "Version"))..])); if (v != f.Version) {
                return (default!, new FormatError(0, "mismatched ELF version", v));
            }
        }
        phoff = ((int64)bo.Uint32(data[(int)(@unsafe.Offsetof(hdrΔ2.GetType(), "Phoff"))..]));
        phentsize = ((nint)bo.Uint16(data[(int)(@unsafe.Offsetof(hdrΔ2.GetType(), "Phentsize"))..]));
        phnum = ((nint)bo.Uint16(data[(int)(@unsafe.Offsetof(hdrΔ2.GetType(), "Phnum"))..]));
        shoff = ((int64)bo.Uint32(data[(int)(@unsafe.Offsetof(hdrΔ2.GetType(), "Shoff"))..]));
        shentsize = ((nint)bo.Uint16(data[(int)(@unsafe.Offsetof(hdrΔ2.GetType(), "Shentsize"))..]));
        shnum = ((nint)bo.Uint16(data[(int)(@unsafe.Offsetof(hdrΔ2.GetType(), "Shnum"))..]));
        shstrndx = ((nint)bo.Uint16(data[(int)(@unsafe.Offsetof(hdrΔ2.GetType(), "Shstrndx"))..]));
    }
    else if (exprᴛ3 == ELFCLASS64) {
        Header64 hdr = default!;
        var data = new slice<byte>(@unsafe.Sizeof(hdr));
        {
            var (_, errΔ5) = sr.ReadAt(data, 0); if (errΔ5 != default!) {
                return (default!, errΔ5);
            }
        }
        f.Type = ((Type)bo.Uint16(data[(int)(@unsafe.Offsetof(hdr.GetType(), "Type"))..]));
        f.Machine = ((Machine)bo.Uint16(data[(int)(@unsafe.Offsetof(hdr.GetType(), "Machine"))..]));
        f.Entry = bo.Uint64(data[(int)(@unsafe.Offsetof(hdr.GetType(), "Entry"))..]);
        {
            ref var v = ref heap<Version>(out var Ꮡv);
            v = ((Version)bo.Uint32(data[(int)(@unsafe.Offsetof(hdr.GetType(), "Version"))..])); if (v != f.Version) {
                return (default!, new FormatError(0, "mismatched ELF version", v));
            }
        }
        phoff = ((int64)bo.Uint64(data[(int)(@unsafe.Offsetof(hdr.GetType(), "Phoff"))..]));
        phentsize = ((nint)bo.Uint16(data[(int)(@unsafe.Offsetof(hdr.GetType(), "Phentsize"))..]));
        phnum = ((nint)bo.Uint16(data[(int)(@unsafe.Offsetof(hdr.GetType(), "Phnum"))..]));
        shoff = ((int64)bo.Uint64(data[(int)(@unsafe.Offsetof(hdr.GetType(), "Shoff"))..]));
        shentsize = ((nint)bo.Uint16(data[(int)(@unsafe.Offsetof(hdr.GetType(), "Shentsize"))..]));
        shnum = ((nint)bo.Uint16(data[(int)(@unsafe.Offsetof(hdr.GetType(), "Shnum"))..]));
        shstrndx = ((nint)bo.Uint16(data[(int)(@unsafe.Offsetof(hdr.GetType(), "Shstrndx"))..]));
    }

    if (shoff < 0) {
        return (default!, new FormatError(0, "invalid shoff", shoff));
    }
    if (phoff < 0) {
        return (default!, new FormatError(0, "invalid phoff", phoff));
    }
    if (shoff == 0 && shnum != 0) {
        return (default!, new FormatError(0, "invalid ELF shnum for shoff=0", shnum));
    }
    if (shnum > 0 && shstrndx >= shnum) {
        return (default!, new FormatError(0, "invalid ELF shstrndx", shstrndx));
    }
    nint wantPhentsize = default!;
    nint wantShentsize = default!;
    var exprᴛ4 = f.Class;
    if (exprᴛ4 == ELFCLASS32) {
        wantPhentsize = 8 * 4;
        wantShentsize = 10 * 4;
    }
    else if (exprᴛ4 == ELFCLASS64) {
        wantPhentsize = 2 * 4 + 6 * 8;
        wantShentsize = 4 * 4 + 6 * 8;
    }

    if (phnum > 0 && phentsize < wantPhentsize) {
        return (default!, new FormatError(0, "invalid ELF phentsize", phentsize));
    }
    // Read program headers
    f.val.Progs = new slice<ж<Prog>>(phnum);
    (phdata, err) = saferio.ReadDataAt(~sr, ((uint64)phnum) * ((uint64)phentsize), phoff);
    if (err != default!) {
        return (default!, err);
    }
    for (nint i = 0; i < phnum; i++) {
        var off = ((uintptr)i) * ((uintptr)phentsize);
        var p = @new<Prog>();
        var exprᴛ5 = f.Class;
        if (exprᴛ5 == ELFCLASS32) {
            Prog32 phΔ2 = default!;
            p.val.ProgHeader = new ProgHeader(
                Type: ((ProgType)bo.Uint32(phdata[(int)(off + @unsafe.Offsetof(phΔ2.GetType(), "Type"))..])),
                Flags: ((ProgFlag)bo.Uint32(phdata[(int)(off + @unsafe.Offsetof(phΔ2.GetType(), "Flags"))..])),
                Off: ((uint64)bo.Uint32(phdata[(int)(off + @unsafe.Offsetof(phΔ2.GetType(), "Off"))..])),
                Vaddr: ((uint64)bo.Uint32(phdata[(int)(off + @unsafe.Offsetof(phΔ2.GetType(), "Vaddr"))..])),
                Paddr: ((uint64)bo.Uint32(phdata[(int)(off + @unsafe.Offsetof(phΔ2.GetType(), "Paddr"))..])),
                Filesz: ((uint64)bo.Uint32(phdata[(int)(off + @unsafe.Offsetof(phΔ2.GetType(), "Filesz"))..])),
                Memsz: ((uint64)bo.Uint32(phdata[(int)(off + @unsafe.Offsetof(phΔ2.GetType(), "Memsz"))..])),
                Align: ((uint64)bo.Uint32(phdata[(int)(off + @unsafe.Offsetof(phΔ2.GetType(), "Align"))..]))
            );
        }
        else if (exprᴛ5 == ELFCLASS64) {
            Prog64 ph = default!;
            p.val.ProgHeader = new ProgHeader(
                Type: ((ProgType)bo.Uint32(phdata[(int)(off + @unsafe.Offsetof(ph.GetType(), "Type"))..])),
                Flags: ((ProgFlag)bo.Uint32(phdata[(int)(off + @unsafe.Offsetof(ph.GetType(), "Flags"))..])),
                Off: bo.Uint64(phdata[(int)(off + @unsafe.Offsetof(ph.GetType(), "Off"))..]),
                Vaddr: bo.Uint64(phdata[(int)(off + @unsafe.Offsetof(ph.GetType(), "Vaddr"))..]),
                Paddr: bo.Uint64(phdata[(int)(off + @unsafe.Offsetof(ph.GetType(), "Paddr"))..]),
                Filesz: bo.Uint64(phdata[(int)(off + @unsafe.Offsetof(ph.GetType(), "Filesz"))..]),
                Memsz: bo.Uint64(phdata[(int)(off + @unsafe.Offsetof(ph.GetType(), "Memsz"))..]),
                Align: bo.Uint64(phdata[(int)(off + @unsafe.Offsetof(ph.GetType(), "Align"))..])
            );
        }

        if (((int64)p.Off) < 0) {
            return (default!, new FormatError(phoff + ((int64)off), "invalid program header offset", p.Off));
        }
        if (((int64)p.Filesz) < 0) {
            return (default!, new FormatError(phoff + ((int64)off), "invalid program header file size", p.Filesz));
        }
        p.val.sr = io.NewSectionReader(r, ((int64)p.Off), ((int64)p.Filesz));
        p.val.ReaderAt = p.val.sr;
        (~f).Progs[i] = p;
    }
    // If the number of sections is greater than or equal to SHN_LORESERVE
    // (0xff00), shnum has the value zero and the actual number of section
    // header table entries is contained in the sh_size field of the section
    // header at index 0.
    if (shoff > 0 && shnum == 0) {
        uint32 typ = default!;
        uint32 link = default!;
        sr.Seek(shoff, io.SeekStart);
        var exprᴛ6 = f.Class;
        if (exprᴛ6 == ELFCLASS32) {
            var shΔ2 = @new<Section32>();
            {
                var errΔ8 = binary.Read(~sr, bo, shΔ2); if (errΔ8 != default!) {
                    return (default!, errΔ8);
                }
            }
            shnum = ((nint)(~shΔ2).Size);
            typ = shΔ2.val.Type;
            link = shΔ2.val.Link;
        }
        else if (exprᴛ6 == ELFCLASS64) {
            var shΔ3 = @new<Section64>();
            {
                var errΔ9 = binary.Read(~sr, bo, shΔ3); if (errΔ9 != default!) {
                    return (default!, errΔ9);
                }
            }
            shnum = ((nint)(~shΔ3).Size);
            typ = shΔ3.val.Type;
            link = shΔ3.val.Link;
        }

        if (((SectionType)typ) != SHT_NULL) {
            return (default!, new FormatError(shoff, "invalid type of the initial section", ((SectionType)typ)));
        }
        if (shnum < ((nint)SHN_LORESERVE)) {
            return (default!, new FormatError(shoff, "invalid ELF shnum contained in sh_size", shnum));
        }
        // If the section name string table section index is greater than or
        // equal to SHN_LORESERVE (0xff00), this member has the value
        // SHN_XINDEX (0xffff) and the actual index of the section name
        // string table section is contained in the sh_link field of the
        // section header at index 0.
        if (shstrndx == ((nint)SHN_XINDEX)) {
            shstrndx = ((nint)link);
            if (shstrndx < ((nint)SHN_LORESERVE)) {
                return (default!, new FormatError(shoff, "invalid ELF shstrndx contained in sh_link", shstrndx));
            }
        }
    }
    if (shnum > 0 && shentsize < wantShentsize) {
        return (default!, new FormatError(0, "invalid ELF shentsize", shentsize));
    }
    // Read section headers
    nint c = saferio.SliceCap<ΔSection>(((uint64)shnum));
    if (c < 0) {
        return (default!, new FormatError(0, "too many sections", shnum));
    }
    f.val.Sections = new slice<ж<ΔSection>>(0, c);
    var names = new slice<uint32>(0, c);
    (shdata, err) = saferio.ReadDataAt(~sr, ((uint64)shnum) * ((uint64)shentsize), shoff);
    if (err != default!) {
        return (default!, err);
    }
    for (nint i = 0; i < shnum; i++) {
        var off = ((uintptr)i) * ((uintptr)shentsize);
        var s = @new<ΔSection>();
        var exprᴛ7 = f.Class;
        if (exprᴛ7 == ELFCLASS32) {
            Section32 shΔ5 = default!;
            names = append(names, bo.Uint32(shdata[(int)(off + @unsafe.Offsetof(shΔ5.GetType(), "Name"))..]));
            s.val.SectionHeader = new SectionHeader(
                Type: ((SectionType)bo.Uint32(shdata[(int)(off + @unsafe.Offsetof(shΔ5.GetType(), "Type"))..])),
                Flags: ((SectionFlag)bo.Uint32(shdata[(int)(off + @unsafe.Offsetof(shΔ5.GetType(), "Flags"))..])),
                Addr: ((uint64)bo.Uint32(shdata[(int)(off + @unsafe.Offsetof(shΔ5.GetType(), "Addr"))..])),
                Offset: ((uint64)bo.Uint32(shdata[(int)(off + @unsafe.Offsetof(shΔ5.GetType(), "Off"))..])),
                FileSize: ((uint64)bo.Uint32(shdata[(int)(off + @unsafe.Offsetof(shΔ5.GetType(), "Size"))..])),
                Link: bo.Uint32(shdata[(int)(off + @unsafe.Offsetof(shΔ5.GetType(), "Link"))..]),
                Info: bo.Uint32(shdata[(int)(off + @unsafe.Offsetof(shΔ5.GetType(), "Info"))..]),
                Addralign: ((uint64)bo.Uint32(shdata[(int)(off + @unsafe.Offsetof(shΔ5.GetType(), "Addralign"))..])),
                Entsize: ((uint64)bo.Uint32(shdata[(int)(off + @unsafe.Offsetof(shΔ5.GetType(), "Entsize"))..]))
            );
        }
        else if (exprᴛ7 == ELFCLASS64) {
            Section64 sh = default!;
            names = append(names, bo.Uint32(shdata[(int)(off + @unsafe.Offsetof(sh.GetType(), "Name"))..]));
            s.val.SectionHeader = new SectionHeader(
                Type: ((SectionType)bo.Uint32(shdata[(int)(off + @unsafe.Offsetof(sh.GetType(), "Type"))..])),
                Flags: ((SectionFlag)bo.Uint64(shdata[(int)(off + @unsafe.Offsetof(sh.GetType(), "Flags"))..])),
                Offset: bo.Uint64(shdata[(int)(off + @unsafe.Offsetof(sh.GetType(), "Off"))..]),
                FileSize: bo.Uint64(shdata[(int)(off + @unsafe.Offsetof(sh.GetType(), "Size"))..]),
                Addr: bo.Uint64(shdata[(int)(off + @unsafe.Offsetof(sh.GetType(), "Addr"))..]),
                Link: bo.Uint32(shdata[(int)(off + @unsafe.Offsetof(sh.GetType(), "Link"))..]),
                Info: bo.Uint32(shdata[(int)(off + @unsafe.Offsetof(sh.GetType(), "Info"))..]),
                Addralign: bo.Uint64(shdata[(int)(off + @unsafe.Offsetof(sh.GetType(), "Addralign"))..]),
                Entsize: bo.Uint64(shdata[(int)(off + @unsafe.Offsetof(sh.GetType(), "Entsize"))..])
            );
        }

        if (((int64)s.Offset) < 0) {
            return (default!, new FormatError(shoff + ((int64)off), "invalid section offset", ((int64)s.Offset)));
        }
        if (((int64)s.FileSize) < 0) {
            return (default!, new FormatError(shoff + ((int64)off), "invalid section size", ((int64)s.FileSize)));
        }
        s.val.sr = io.NewSectionReader(r, ((int64)s.Offset), ((int64)s.FileSize));
        if ((SectionFlag)(s.Flags & SHF_COMPRESSED) == 0){
            s.val.ReaderAt = s.val.sr;
            s.Size = s.FileSize;
        } else {
            // Read the compression header.
            var exprᴛ8 = f.Class;
            if (exprᴛ8 == ELFCLASS32) {
                Chdr32 chΔ2 = default!;
                var chdata = new slice<byte>(@unsafe.Sizeof(chΔ2));
                {
                    var (_, errΔ12) = (~s).sr.ReadAt(chdata, 0); if (errΔ12 != default!) {
                        return (default!, errΔ12);
                    }
                }
                s.val.compressionType = ((CompressionType)bo.Uint32(chdata[(int)(@unsafe.Offsetof(chΔ2.GetType(), "Type"))..]));
                s.Size = ((uint64)bo.Uint32(chdata[(int)(@unsafe.Offsetof(chΔ2.GetType(), "Size"))..]));
                s.Addralign = ((uint64)bo.Uint32(chdata[(int)(@unsafe.Offsetof(chΔ2.GetType(), "Addralign"))..]));
                s.val.compressionOffset = ((int64)@unsafe.Sizeof(chΔ2));
            }
            else if (exprᴛ8 == ELFCLASS64) {
                Chdr64 ch = default!;
                var chdata = new slice<byte>(@unsafe.Sizeof(ch));
                {
                    var (_, errΔ13) = (~s).sr.ReadAt(chdata, 0); if (errΔ13 != default!) {
                        return (default!, errΔ13);
                    }
                }
                s.val.compressionType = ((CompressionType)bo.Uint32(chdata[(int)(@unsafe.Offsetof(ch.GetType(), "Type"))..]));
                s.Size = bo.Uint64(chdata[(int)(@unsafe.Offsetof(ch.GetType(), "Size"))..]);
                s.Addralign = bo.Uint64(chdata[(int)(@unsafe.Offsetof(ch.GetType(), "Addralign"))..]);
                s.val.compressionOffset = ((int64)@unsafe.Sizeof(ch));
            }

        }
        f.val.Sections = append((~f).Sections, s);
    }
    if (len((~f).Sections) == 0) {
        return (f, default!);
    }
    // Load section header string table.
    if (shstrndx == 0) {
        // If the file has no section name string table,
        // shstrndx holds the value SHN_UNDEF (0).
        return (f, default!);
    }
    var shstr = (~f).Sections[shstrndx];
    if (shstr.Type != SHT_STRTAB) {
        return (default!, new FormatError(shoff + ((int64)(shstrndx * shentsize)), "invalid ELF section name string table type", shstr.Type));
    }
    (shstrtab, err) = shstr.Data();
    if (err != default!) {
        return (default!, err);
    }
    foreach (var (i, s) in (~f).Sections) {
        bool ok = default!;
        (s.Name, ok) = getString(shstrtab, ((nint)names[i]));
        if (!ok) {
            return (default!, new FormatError(shoff + ((int64)(i * shentsize)), "bad section name index", names[i]));
        }
    }
    return (f, default!);
}

// getSymbols returns a slice of Symbols from parsing the symbol table
// with the given type, along with the associated string table.
[GoRecv] internal static (slice<Symbol>, slice<byte>, error) getSymbols(this ref File f, SectionType typ) {
    var exprᴛ1 = f.Class;
    if (exprᴛ1 == ELFCLASS64) {
        return f.getSymbols64(typ);
    }
    if (exprᴛ1 == ELFCLASS32) {
        return f.getSymbols32(typ);
    }

    return (default!, default!, errors.New("not implemented"u8));
}

// ErrNoSymbols is returned by [File.Symbols] and [File.DynamicSymbols]
// if there is no such section in the File.
public static error ErrNoSymbols = errors.New("no symbol section"u8);

[GoRecv] internal static (slice<Symbol>, slice<byte>, error) getSymbols32(this ref File f, SectionType typ) {
    var symtabSection = f.SectionByType(typ);
    if (symtabSection == nil) {
        return (default!, default!, ErrNoSymbols);
    }
    (data, err) = symtabSection.Data();
    if (err != default!) {
        return (default!, default!, fmt.Errorf("cannot load symbol section: %w"u8, err));
    }
    if (len(data) == 0) {
        return (default!, default!, errors.New("symbol section is empty"u8));
    }
    if (len(data) % Sym32Size != 0) {
        return (default!, default!, errors.New("length of symbol section is not a multiple of SymSize"u8));
    }
    (strdata, err) = f.stringTable(symtabSection.Link);
    if (err != default!) {
        return (default!, default!, fmt.Errorf("cannot load string table section: %w"u8, err));
    }
    // The first entry is all zeros.
    data = data[(int)(Sym32Size)..];
    var symbols = new slice<Symbol>(len(data) / Sym32Size);
    nint i = 0;
    Sym32 sym = default!;
    while (len(data) > 0) {
        sym.Name = f.ByteOrder.Uint32(data[0..4]);
        sym.Value = f.ByteOrder.Uint32(data[4..8]);
        sym.Size = f.ByteOrder.Uint32(data[8..12]);
        sym.Info = data[12];
        sym.Other = data[13];
        sym.Shndx = f.ByteOrder.Uint16(data[14..16]);
        var (str, _) = getString(strdata, ((nint)sym.Name));
        symbols[i].Name = str;
        symbols[i].Info = sym.Info;
        symbols[i].Other = sym.Other;
        symbols[i].Section = ((SectionIndex)sym.Shndx);
        symbols[i].Value = ((uint64)sym.Value);
        symbols[i].Size = ((uint64)sym.Size);
        i++;
        data = data[(int)(Sym32Size)..];
    }
    return (symbols, strdata, default!);
}

[GoRecv] internal static (slice<Symbol>, slice<byte>, error) getSymbols64(this ref File f, SectionType typ) {
    var symtabSection = f.SectionByType(typ);
    if (symtabSection == nil) {
        return (default!, default!, ErrNoSymbols);
    }
    (data, err) = symtabSection.Data();
    if (err != default!) {
        return (default!, default!, fmt.Errorf("cannot load symbol section: %w"u8, err));
    }
    if (len(data) % Sym64Size != 0) {
        return (default!, default!, errors.New("length of symbol section is not a multiple of Sym64Size"u8));
    }
    (strdata, err) = f.stringTable(symtabSection.Link);
    if (err != default!) {
        return (default!, default!, fmt.Errorf("cannot load string table section: %w"u8, err));
    }
    // The first entry is all zeros.
    data = data[(int)(Sym64Size)..];
    var symbols = new slice<Symbol>(len(data) / Sym64Size);
    nint i = 0;
    Sym64 sym = default!;
    while (len(data) > 0) {
        sym.Name = f.ByteOrder.Uint32(data[0..4]);
        sym.Info = data[4];
        sym.Other = data[5];
        sym.Shndx = f.ByteOrder.Uint16(data[6..8]);
        sym.Value = f.ByteOrder.Uint64(data[8..16]);
        sym.Size = f.ByteOrder.Uint64(data[16..24]);
        var (str, _) = getString(strdata, ((nint)sym.Name));
        symbols[i].Name = str;
        symbols[i].Info = sym.Info;
        symbols[i].Other = sym.Other;
        symbols[i].Section = ((SectionIndex)sym.Shndx);
        symbols[i].Value = sym.Value;
        symbols[i].Size = sym.Size;
        i++;
        data = data[(int)(Sym64Size)..];
    }
    return (symbols, strdata, default!);
}

// getString extracts a string from an ELF string table.
internal static (@string, bool) getString(slice<byte> section, nint start) {
    if (start < 0 || start >= len(section)) {
        return ("", false);
    }
    for (nint end = start; end < len(section); end++) {
        if (section[end] == 0) {
            return (((@string)(section[(int)(start)..(int)(end)])), true);
        }
    }
    return ("", false);
}

// Section returns a section with the given name, or nil if no such
// section exists.
[GoRecv] public static ж<ΔSection> Section(this ref File f, @string name) {
    foreach (var (_, s) in f.Sections) {
        if (s.Name == name) {
            return s;
        }
    }
    return default!;
}

// applyRelocations applies relocations to dst. rels is a relocations section
// in REL or RELA format.
[GoRecv] internal static error applyRelocations(this ref File f, slice<byte> dst, slice<byte> rels) {
    switch (ᐧ) {
    case {} when f.Class == ELFCLASS64 && f.Machine == EM_X86_64: {
        return f.applyRelocationsAMD64(dst, rels);
    }
    case {} when f.Class == ELFCLASS32 && f.Machine == EM_386: {
        return f.applyRelocations386(dst, rels);
    }
    case {} when f.Class == ELFCLASS32 && f.Machine == EM_ARM: {
        return f.applyRelocationsARM(dst, rels);
    }
    case {} when f.Class == ELFCLASS64 && f.Machine == EM_AARCH64: {
        return f.applyRelocationsARM64(dst, rels);
    }
    case {} when f.Class == ELFCLASS32 && f.Machine == EM_PPC: {
        return f.applyRelocationsPPC(dst, rels);
    }
    case {} when f.Class == ELFCLASS64 && f.Machine == EM_PPC64: {
        return f.applyRelocationsPPC64(dst, rels);
    }
    case {} when f.Class == ELFCLASS32 && f.Machine == EM_MIPS: {
        return f.applyRelocationsMIPS(dst, rels);
    }
    case {} when f.Class == ELFCLASS64 && f.Machine == EM_MIPS: {
        return f.applyRelocationsMIPS64(dst, rels);
    }
    case {} when f.Class == ELFCLASS64 && f.Machine == EM_LOONGARCH: {
        return f.applyRelocationsLOONG64(dst, rels);
    }
    case {} when f.Class == ELFCLASS64 && f.Machine == EM_RISCV: {
        return f.applyRelocationsRISCV64(dst, rels);
    }
    case {} when f.Class == ELFCLASS64 && f.Machine == EM_S390: {
        return f.applyRelocationss390x(dst, rels);
    }
    case {} when f.Class == ELFCLASS64 && f.Machine == EM_SPARCV9: {
        return f.applyRelocationsSPARC64(dst, rels);
    }
    default: {
        return errors.New("applyRelocations: not implemented"u8);
    }}

}

// canApplyRelocation reports whether we should try to apply a
// relocation to a DWARF data section, given a pointer to the symbol
// targeted by the relocation.
// Most relocations in DWARF data tend to be section-relative, but
// some target non-section symbols (for example, low_PC attrs on
// subprogram or compilation unit DIEs that target function symbols).
internal static bool canApplyRelocation(ж<Symbol> Ꮡsym) {
    ref var sym = ref Ꮡsym.val;

    return sym.Section != SHN_UNDEF && sym.Section < SHN_LORESERVE;
}

[GoRecv] internal static error applyRelocationsAMD64(this ref File f, slice<byte> dst, slice<byte> rels) {
    // 24 is the size of Rela64.
    if (len(rels) % 24 != 0) {
        return errors.New("length of relocation section is not a multiple of 24"u8);
    }
    (symbols, _, err) = f.getSymbols(SHT_SYMTAB);
    if (err != default!) {
        return err;
    }
    var b = bytes.NewReader(rels);
    ref var rela = ref heap(new Rela64(), out var Ꮡrela);
    while (b.Len() > 0) {
        binary.Read(~b, f.ByteOrder, Ꮡrela);
        var symNo = rela.Info >> (int)(32);
        R_X86_64 t = ((R_X86_64)((uint64)(rela.Info & 65535)));
        if (symNo == 0 || symNo > ((uint64)len(symbols))) {
            continue;
        }
        var sym = Ꮡ(symbols, symNo - 1);
        if (!canApplyRelocation(sym)) {
            continue;
        }
        // There are relocations, so this must be a normal
        // object file.  The code below handles only basic relocations
        // of the form S + A (symbol plus addend).
        var exprᴛ1 = t;
        if (exprᴛ1 == R_X86_64_64) {
            if (rela.Off + 8 >= ((uint64)len(dst)) || rela.Addend < 0) {
                continue;
            }
            var val64 = (~sym).Value + ((uint64)rela.Addend);
            f.ByteOrder.PutUint64(dst[(int)(rela.Off)..(int)(rela.Off + 8)], val64);
        }
        else if (exprᴛ1 == R_X86_64_32) {
            if (rela.Off + 4 >= ((uint64)len(dst)) || rela.Addend < 0) {
                continue;
            }
            var val32 = ((uint32)(~sym).Value) + ((uint32)rela.Addend);
            f.ByteOrder.PutUint32(dst[(int)(rela.Off)..(int)(rela.Off + 4)], val32);
        }

    }
    return default!;
}

[GoRecv] internal static error applyRelocations386(this ref File f, slice<byte> dst, slice<byte> rels) {
    // 8 is the size of Rel32.
    if (len(rels) % 8 != 0) {
        return errors.New("length of relocation section is not a multiple of 8"u8);
    }
    (symbols, _, err) = f.getSymbols(SHT_SYMTAB);
    if (err != default!) {
        return err;
    }
    var b = bytes.NewReader(rels);
    ref var rel = ref heap(new Rel32(), out var Ꮡrel);
    while (b.Len() > 0) {
        binary.Read(~b, f.ByteOrder, Ꮡrel);
        var symNo = rel.Info >> (int)(8);
        R_386 t = ((R_386)((uint32)(rel.Info & 255)));
        if (symNo == 0 || symNo > ((uint32)len(symbols))) {
            continue;
        }
        var sym = Ꮡ(symbols, symNo - 1);
        if (t == R_386_32) {
            if (rel.Off + 4 >= ((uint32)len(dst))) {
                continue;
            }
            var val = f.ByteOrder.Uint32(dst[(int)(rel.Off)..(int)(rel.Off + 4)]);
            val += ((uint32)(~sym).Value);
            f.ByteOrder.PutUint32(dst[(int)(rel.Off)..(int)(rel.Off + 4)], val);
        }
    }
    return default!;
}

[GoRecv] internal static error applyRelocationsARM(this ref File f, slice<byte> dst, slice<byte> rels) {
    // 8 is the size of Rel32.
    if (len(rels) % 8 != 0) {
        return errors.New("length of relocation section is not a multiple of 8"u8);
    }
    (symbols, _, err) = f.getSymbols(SHT_SYMTAB);
    if (err != default!) {
        return err;
    }
    var b = bytes.NewReader(rels);
    ref var rel = ref heap(new Rel32(), out var Ꮡrel);
    while (b.Len() > 0) {
        binary.Read(~b, f.ByteOrder, Ꮡrel);
        var symNo = rel.Info >> (int)(8);
        R_ARM t = ((R_ARM)((uint32)(rel.Info & 255)));
        if (symNo == 0 || symNo > ((uint32)len(symbols))) {
            continue;
        }
        var sym = Ꮡ(symbols, symNo - 1);
        var exprᴛ1 = t;
        if (exprᴛ1 == R_ARM_ABS32) {
            if (rel.Off + 4 >= ((uint32)len(dst))) {
                continue;
            }
            var val = f.ByteOrder.Uint32(dst[(int)(rel.Off)..(int)(rel.Off + 4)]);
            val += ((uint32)(~sym).Value);
            f.ByteOrder.PutUint32(dst[(int)(rel.Off)..(int)(rel.Off + 4)], val);
        }

    }
    return default!;
}

[GoRecv] internal static error applyRelocationsARM64(this ref File f, slice<byte> dst, slice<byte> rels) {
    // 24 is the size of Rela64.
    if (len(rels) % 24 != 0) {
        return errors.New("length of relocation section is not a multiple of 24"u8);
    }
    (symbols, _, err) = f.getSymbols(SHT_SYMTAB);
    if (err != default!) {
        return err;
    }
    var b = bytes.NewReader(rels);
    ref var rela = ref heap(new Rela64(), out var Ꮡrela);
    while (b.Len() > 0) {
        binary.Read(~b, f.ByteOrder, Ꮡrela);
        var symNo = rela.Info >> (int)(32);
        R_AARCH64 t = ((R_AARCH64)((uint64)(rela.Info & 65535)));
        if (symNo == 0 || symNo > ((uint64)len(symbols))) {
            continue;
        }
        var sym = Ꮡ(symbols, symNo - 1);
        if (!canApplyRelocation(sym)) {
            continue;
        }
        // There are relocations, so this must be a normal
        // object file.  The code below handles only basic relocations
        // of the form S + A (symbol plus addend).
        var exprᴛ1 = t;
        if (exprᴛ1 == R_AARCH64_ABS64) {
            if (rela.Off + 8 >= ((uint64)len(dst)) || rela.Addend < 0) {
                continue;
            }
            var val64 = (~sym).Value + ((uint64)rela.Addend);
            f.ByteOrder.PutUint64(dst[(int)(rela.Off)..(int)(rela.Off + 8)], val64);
        }
        else if (exprᴛ1 == R_AARCH64_ABS32) {
            if (rela.Off + 4 >= ((uint64)len(dst)) || rela.Addend < 0) {
                continue;
            }
            var val32 = ((uint32)(~sym).Value) + ((uint32)rela.Addend);
            f.ByteOrder.PutUint32(dst[(int)(rela.Off)..(int)(rela.Off + 4)], val32);
        }

    }
    return default!;
}

[GoRecv] internal static error applyRelocationsPPC(this ref File f, slice<byte> dst, slice<byte> rels) {
    // 12 is the size of Rela32.
    if (len(rels) % 12 != 0) {
        return errors.New("length of relocation section is not a multiple of 12"u8);
    }
    (symbols, _, err) = f.getSymbols(SHT_SYMTAB);
    if (err != default!) {
        return err;
    }
    var b = bytes.NewReader(rels);
    ref var rela = ref heap(new Rela32(), out var Ꮡrela);
    while (b.Len() > 0) {
        binary.Read(~b, f.ByteOrder, Ꮡrela);
        var symNo = rela.Info >> (int)(8);
        R_PPC t = ((R_PPC)((uint32)(rela.Info & 255)));
        if (symNo == 0 || symNo > ((uint32)len(symbols))) {
            continue;
        }
        var sym = Ꮡ(symbols, symNo - 1);
        if (!canApplyRelocation(sym)) {
            continue;
        }
        var exprᴛ1 = t;
        if (exprᴛ1 == R_PPC_ADDR32) {
            if (rela.Off + 4 >= ((uint32)len(dst)) || rela.Addend < 0) {
                continue;
            }
            var val32 = ((uint32)(~sym).Value) + ((uint32)rela.Addend);
            f.ByteOrder.PutUint32(dst[(int)(rela.Off)..(int)(rela.Off + 4)], val32);
        }

    }
    return default!;
}

[GoRecv] internal static error applyRelocationsPPC64(this ref File f, slice<byte> dst, slice<byte> rels) {
    // 24 is the size of Rela64.
    if (len(rels) % 24 != 0) {
        return errors.New("length of relocation section is not a multiple of 24"u8);
    }
    (symbols, _, err) = f.getSymbols(SHT_SYMTAB);
    if (err != default!) {
        return err;
    }
    var b = bytes.NewReader(rels);
    ref var rela = ref heap(new Rela64(), out var Ꮡrela);
    while (b.Len() > 0) {
        binary.Read(~b, f.ByteOrder, Ꮡrela);
        var symNo = rela.Info >> (int)(32);
        R_PPC64 t = ((R_PPC64)((uint64)(rela.Info & 65535)));
        if (symNo == 0 || symNo > ((uint64)len(symbols))) {
            continue;
        }
        var sym = Ꮡ(symbols, symNo - 1);
        if (!canApplyRelocation(sym)) {
            continue;
        }
        var exprᴛ1 = t;
        if (exprᴛ1 == R_PPC64_ADDR64) {
            if (rela.Off + 8 >= ((uint64)len(dst)) || rela.Addend < 0) {
                continue;
            }
            var val64 = (~sym).Value + ((uint64)rela.Addend);
            f.ByteOrder.PutUint64(dst[(int)(rela.Off)..(int)(rela.Off + 8)], val64);
        }
        else if (exprᴛ1 == R_PPC64_ADDR32) {
            if (rela.Off + 4 >= ((uint64)len(dst)) || rela.Addend < 0) {
                continue;
            }
            var val32 = ((uint32)(~sym).Value) + ((uint32)rela.Addend);
            f.ByteOrder.PutUint32(dst[(int)(rela.Off)..(int)(rela.Off + 4)], val32);
        }

    }
    return default!;
}

[GoRecv] internal static error applyRelocationsMIPS(this ref File f, slice<byte> dst, slice<byte> rels) {
    // 8 is the size of Rel32.
    if (len(rels) % 8 != 0) {
        return errors.New("length of relocation section is not a multiple of 8"u8);
    }
    (symbols, _, err) = f.getSymbols(SHT_SYMTAB);
    if (err != default!) {
        return err;
    }
    var b = bytes.NewReader(rels);
    ref var rel = ref heap(new Rel32(), out var Ꮡrel);
    while (b.Len() > 0) {
        binary.Read(~b, f.ByteOrder, Ꮡrel);
        var symNo = rel.Info >> (int)(8);
        R_MIPS t = ((R_MIPS)((uint32)(rel.Info & 255)));
        if (symNo == 0 || symNo > ((uint32)len(symbols))) {
            continue;
        }
        var sym = Ꮡ(symbols, symNo - 1);
        var exprᴛ1 = t;
        if (exprᴛ1 == R_MIPS_32) {
            if (rel.Off + 4 >= ((uint32)len(dst))) {
                continue;
            }
            var val = f.ByteOrder.Uint32(dst[(int)(rel.Off)..(int)(rel.Off + 4)]);
            val += ((uint32)(~sym).Value);
            f.ByteOrder.PutUint32(dst[(int)(rel.Off)..(int)(rel.Off + 4)], val);
        }

    }
    return default!;
}

[GoRecv] internal static error applyRelocationsMIPS64(this ref File f, slice<byte> dst, slice<byte> rels) {
    // 24 is the size of Rela64.
    if (len(rels) % 24 != 0) {
        return errors.New("length of relocation section is not a multiple of 24"u8);
    }
    (symbols, _, err) = f.getSymbols(SHT_SYMTAB);
    if (err != default!) {
        return err;
    }
    var b = bytes.NewReader(rels);
    ref var rela = ref heap(new Rela64(), out var Ꮡrela);
    while (b.Len() > 0) {
        binary.Read(~b, f.ByteOrder, Ꮡrela);
        uint64 symNo = default!;
        R_MIPS t = default!;
        if (f.ByteOrder == binary.BigEndian){
            symNo = rela.Info >> (int)(32);
            t = ((R_MIPS)((uint64)(rela.Info & 255)));
        } else {
            symNo = (uint64)(rela.Info & (nint)4294967295L);
            t = ((R_MIPS)(rela.Info >> (int)(56)));
        }
        if (symNo == 0 || symNo > ((uint64)len(symbols))) {
            continue;
        }
        var sym = Ꮡ(symbols, symNo - 1);
        if (!canApplyRelocation(sym)) {
            continue;
        }
        var exprᴛ1 = t;
        if (exprᴛ1 == R_MIPS_64) {
            if (rela.Off + 8 >= ((uint64)len(dst)) || rela.Addend < 0) {
                continue;
            }
            var val64 = (~sym).Value + ((uint64)rela.Addend);
            f.ByteOrder.PutUint64(dst[(int)(rela.Off)..(int)(rela.Off + 8)], val64);
        }
        else if (exprᴛ1 == R_MIPS_32) {
            if (rela.Off + 4 >= ((uint64)len(dst)) || rela.Addend < 0) {
                continue;
            }
            var val32 = ((uint32)(~sym).Value) + ((uint32)rela.Addend);
            f.ByteOrder.PutUint32(dst[(int)(rela.Off)..(int)(rela.Off + 4)], val32);
        }

    }
    return default!;
}

[GoRecv] internal static error applyRelocationsLOONG64(this ref File f, slice<byte> dst, slice<byte> rels) {
    // 24 is the size of Rela64.
    if (len(rels) % 24 != 0) {
        return errors.New("length of relocation section is not a multiple of 24"u8);
    }
    (symbols, _, err) = f.getSymbols(SHT_SYMTAB);
    if (err != default!) {
        return err;
    }
    var b = bytes.NewReader(rels);
    ref var rela = ref heap(new Rela64(), out var Ꮡrela);
    while (b.Len() > 0) {
        binary.Read(~b, f.ByteOrder, Ꮡrela);
        uint64 symNo = default!;
        R_LARCH t = default!;
        symNo = rela.Info >> (int)(32);
        t = ((R_LARCH)((uint64)(rela.Info & 65535)));
        if (symNo == 0 || symNo > ((uint64)len(symbols))) {
            continue;
        }
        var sym = Ꮡ(symbols, symNo - 1);
        if (!canApplyRelocation(sym)) {
            continue;
        }
        var exprᴛ1 = t;
        if (exprᴛ1 == R_LARCH_64) {
            if (rela.Off + 8 >= ((uint64)len(dst)) || rela.Addend < 0) {
                continue;
            }
            var val64 = (~sym).Value + ((uint64)rela.Addend);
            f.ByteOrder.PutUint64(dst[(int)(rela.Off)..(int)(rela.Off + 8)], val64);
        }
        else if (exprᴛ1 == R_LARCH_32) {
            if (rela.Off + 4 >= ((uint64)len(dst)) || rela.Addend < 0) {
                continue;
            }
            var val32 = ((uint32)(~sym).Value) + ((uint32)rela.Addend);
            f.ByteOrder.PutUint32(dst[(int)(rela.Off)..(int)(rela.Off + 4)], val32);
        }

    }
    return default!;
}

[GoRecv] internal static error applyRelocationsRISCV64(this ref File f, slice<byte> dst, slice<byte> rels) {
    // 24 is the size of Rela64.
    if (len(rels) % 24 != 0) {
        return errors.New("length of relocation section is not a multiple of 24"u8);
    }
    (symbols, _, err) = f.getSymbols(SHT_SYMTAB);
    if (err != default!) {
        return err;
    }
    var b = bytes.NewReader(rels);
    ref var rela = ref heap(new Rela64(), out var Ꮡrela);
    while (b.Len() > 0) {
        binary.Read(~b, f.ByteOrder, Ꮡrela);
        var symNo = rela.Info >> (int)(32);
        R_RISCV t = ((R_RISCV)((uint64)(rela.Info & 65535)));
        if (symNo == 0 || symNo > ((uint64)len(symbols))) {
            continue;
        }
        var sym = Ꮡ(symbols, symNo - 1);
        if (!canApplyRelocation(sym)) {
            continue;
        }
        var exprᴛ1 = t;
        if (exprᴛ1 == R_RISCV_64) {
            if (rela.Off + 8 >= ((uint64)len(dst)) || rela.Addend < 0) {
                continue;
            }
            var val64 = (~sym).Value + ((uint64)rela.Addend);
            f.ByteOrder.PutUint64(dst[(int)(rela.Off)..(int)(rela.Off + 8)], val64);
        }
        else if (exprᴛ1 == R_RISCV_32) {
            if (rela.Off + 4 >= ((uint64)len(dst)) || rela.Addend < 0) {
                continue;
            }
            var val32 = ((uint32)(~sym).Value) + ((uint32)rela.Addend);
            f.ByteOrder.PutUint32(dst[(int)(rela.Off)..(int)(rela.Off + 4)], val32);
        }

    }
    return default!;
}

[GoRecv] internal static error applyRelocationss390x(this ref File f, slice<byte> dst, slice<byte> rels) {
    // 24 is the size of Rela64.
    if (len(rels) % 24 != 0) {
        return errors.New("length of relocation section is not a multiple of 24"u8);
    }
    (symbols, _, err) = f.getSymbols(SHT_SYMTAB);
    if (err != default!) {
        return err;
    }
    var b = bytes.NewReader(rels);
    ref var rela = ref heap(new Rela64(), out var Ꮡrela);
    while (b.Len() > 0) {
        binary.Read(~b, f.ByteOrder, Ꮡrela);
        var symNo = rela.Info >> (int)(32);
        R_390 t = ((R_390)((uint64)(rela.Info & 65535)));
        if (symNo == 0 || symNo > ((uint64)len(symbols))) {
            continue;
        }
        var sym = Ꮡ(symbols, symNo - 1);
        if (!canApplyRelocation(sym)) {
            continue;
        }
        var exprᴛ1 = t;
        if (exprᴛ1 == R_390_64) {
            if (rela.Off + 8 >= ((uint64)len(dst)) || rela.Addend < 0) {
                continue;
            }
            var val64 = (~sym).Value + ((uint64)rela.Addend);
            f.ByteOrder.PutUint64(dst[(int)(rela.Off)..(int)(rela.Off + 8)], val64);
        }
        else if (exprᴛ1 == R_390_32) {
            if (rela.Off + 4 >= ((uint64)len(dst)) || rela.Addend < 0) {
                continue;
            }
            var val32 = ((uint32)(~sym).Value) + ((uint32)rela.Addend);
            f.ByteOrder.PutUint32(dst[(int)(rela.Off)..(int)(rela.Off + 4)], val32);
        }

    }
    return default!;
}

[GoRecv] internal static error applyRelocationsSPARC64(this ref File f, slice<byte> dst, slice<byte> rels) {
    // 24 is the size of Rela64.
    if (len(rels) % 24 != 0) {
        return errors.New("length of relocation section is not a multiple of 24"u8);
    }
    (symbols, _, err) = f.getSymbols(SHT_SYMTAB);
    if (err != default!) {
        return err;
    }
    var b = bytes.NewReader(rels);
    ref var rela = ref heap(new Rela64(), out var Ꮡrela);
    while (b.Len() > 0) {
        binary.Read(~b, f.ByteOrder, Ꮡrela);
        var symNo = rela.Info >> (int)(32);
        R_SPARC t = ((R_SPARC)((uint64)(rela.Info & 255)));
        if (symNo == 0 || symNo > ((uint64)len(symbols))) {
            continue;
        }
        var sym = Ꮡ(symbols, symNo - 1);
        if (!canApplyRelocation(sym)) {
            continue;
        }
        var exprᴛ1 = t;
        if (exprᴛ1 == R_SPARC_64 || exprᴛ1 == R_SPARC_UA64) {
            if (rela.Off + 8 >= ((uint64)len(dst)) || rela.Addend < 0) {
                continue;
            }
            var val64 = (~sym).Value + ((uint64)rela.Addend);
            f.ByteOrder.PutUint64(dst[(int)(rela.Off)..(int)(rela.Off + 8)], val64);
        }
        else if (exprᴛ1 == R_SPARC_32 || exprᴛ1 == R_SPARC_UA32) {
            if (rela.Off + 4 >= ((uint64)len(dst)) || rela.Addend < 0) {
                continue;
            }
            var val32 = ((uint32)(~sym).Value) + ((uint32)rela.Addend);
            f.ByteOrder.PutUint32(dst[(int)(rela.Off)..(int)(rela.Off + 4)], val32);
        }

    }
    return default!;
}

[GoRecv] public static (ж<dwarf.Data>, error) DWARF(this ref File f) {
    var dwarfSuffix = (ж<ΔSection> s) => {
        switch (ᐧ) {
        case {} when strings.HasPrefix(s.Name, ".debug_"u8): {
            return s.Name[7..];
        }
        case {} when strings.HasPrefix(s.Name, ".zdebug_"u8): {
            return s.Name[8..];
        }
        default: {
            return ""u8;
        }}

    };
    // sectionData gets the data for s, checks its size, and
    // applies any applicable relations.
    var sectionData = (nint i, ж<ΔSection> s) => {
        (b, errΔ1) = s.Data();
        if (errΔ1 != default! && ((uint64)len(b)) < s.Size) {
            return (default!, errΔ1);
        }
        if (f.Type == ET_EXEC) {
            // Do not apply relocations to DWARF sections for ET_EXEC binaries.
            // Relocations should already be applied, and .rela sections may
            // contain incorrect data.
            return (b, default!);
        }
        foreach (var (_, r) in f.Sections) {
            if (r.Type != SHT_RELA && r.Type != SHT_REL) {
                continue;
            }
            if (((nint)r.Info) != i) {
                continue;
            }
            (rd, errΔ2) = r.Data();
            if (errΔ2 != default!) {
                return (default!, errΔ2);
            }
             = f.applyRelocations(b, rd);
            if (errΔ2 != default!) {
                return (default!, errΔ2);
            }
        }
        return (b, default!);
    };
    // There are many DWARf sections, but these are the ones
    // the debug/dwarf package started with.
    map<@string, slice<byte>> dat = new map<@string, slice<byte>>{["abbrev"u8] = default!, ["info"u8] = default!, ["str"u8] = default!, ["line"u8] = default!, ["ranges"u8] = default!};
    foreach (var (i, s) in f.Sections) {
        @string suffix = dwarfSuffix(s);
        if (suffix == ""u8) {
            continue;
        }
        {
            var _ = dat[suffix];
            var ok = dat[suffix]; if (!ok) {
                continue;
            }
        }
        (b, errΔ3) = sectionData(i, s);
        if (errΔ3 != default!) {
            return (default!, errΔ3);
        }
        dat[suffix] = b;
    }
    (d, err) = dwarf.New(dat["abbrev"u8], default!, default!, dat["info"u8], dat["line"u8], default!, dat["ranges"u8], dat["str"u8]);
    if (err != default!) {
        return (default!, err);
    }
    // Look for DWARF4 .debug_types sections and DWARF5 sections.
    foreach (var (i, s) in f.Sections) {
        @string suffix = dwarfSuffix(s);
        if (suffix == ""u8) {
            continue;
        }
        {
            var _ = dat[suffix];
            var ok = dat[suffix]; if (ok) {
                // Already handled.
                continue;
            }
        }
        (b, err) = sectionData(i, s);
        if (err != default!) {
            return (default!, err);
        }
        if (suffix == "types"u8){
            {
                var errΔ4 = d.AddTypes(fmt.Sprintf("types-%d"u8, i), b); if (errΔ4 != default!) {
                    return (default!, errΔ4);
                }
            }
        } else {
            {
                var errΔ5 = d.AddSection(".debug_"u8 + suffix, b); if (errΔ5 != default!) {
                    return (default!, errΔ5);
                }
            }
        }
    }
    return (d, default!);
}

// Symbols returns the symbol table for f. The symbols will be listed in the order
// they appear in f.
//
// For compatibility with Go 1.0, Symbols omits the null symbol at index 0.
// After retrieving the symbols as symtab, an externally supplied index x
// corresponds to symtab[x-1], not symtab[x].
[GoRecv] public static (slice<Symbol>, error) Symbols(this ref File f) {
    (sym, _, err) = f.getSymbols(SHT_SYMTAB);
    return (sym, err);
}

// DynamicSymbols returns the dynamic symbol table for f. The symbols
// will be listed in the order they appear in f.
//
// If f has a symbol version table, the returned [File.Symbols] will have
// initialized Version and Library fields.
//
// For compatibility with [File.Symbols], [File.DynamicSymbols] omits the null symbol at index 0.
// After retrieving the symbols as symtab, an externally supplied index x
// corresponds to symtab[x-1], not symtab[x].
[GoRecv] public static (slice<Symbol>, error) DynamicSymbols(this ref File f) {
    (sym, str, err) = f.getSymbols(SHT_DYNSYM);
    if (err != default!) {
        return (default!, err);
    }
    if (f.gnuVersionInit(str)) {
        foreach (var (i, _) in sym) {
            (sym[i].Library, sym[i].Version) = f.gnuVersion(i);
        }
    }
    return (sym, default!);
}

[GoType] partial struct ImportedSymbol {
    public @string Name;
    public @string Version;
    public @string Library;
}

// ImportedSymbols returns the names of all symbols
// referred to by the binary f that are expected to be
// satisfied by other libraries at dynamic load time.
// It does not return weak symbols.
[GoRecv] public static (slice<ImportedSymbol>, error) ImportedSymbols(this ref File f) {
    (sym, str, err) = f.getSymbols(SHT_DYNSYM);
    if (err != default!) {
        return (default!, err);
    }
    f.gnuVersionInit(str);
    slice<ImportedSymbol> all = default!;
    foreach (var (i, s) in sym) {
        if (ST_BIND(s.Info) == STB_GLOBAL && s.Section == SHN_UNDEF) {
            all = append(all, new ImportedSymbol(Name: s.Name));
            var symΔ1 = Ꮡ(all, len(all) - 1);
            (sym.val.Library, sym.val.Version) = f.gnuVersion(i);
        }
    }
    return (all, default!);
}

[GoType] partial struct verneed {
    public @string File;
    public @string Name;
}

// gnuVersionInit parses the GNU version tables
// for use by calls to gnuVersion.
[GoRecv] internal static bool gnuVersionInit(this ref File f, slice<byte> str) {
    if (f.gnuNeed != default!) {
        // Already initialized
        return true;
    }
    // Accumulate verneed information.
    var vn = f.SectionByType(SHT_GNU_VERNEED);
    if (vn == nil) {
        return false;
    }
    (d, _) = vn.Data();
    slice<verneed> need = default!;
    nint i = 0;
    while (ᐧ) {
        if (i + 16 > len(d)) {
            break;
        }
        var vers = f.ByteOrder.Uint16(d[(int)(i)..(int)(i + 2)]);
        if (vers != 1) {
            break;
        }
        var cnt = f.ByteOrder.Uint16(d[(int)(i + 2)..(int)(i + 4)]);
        var fileoff = f.ByteOrder.Uint32(d[(int)(i + 4)..(int)(i + 8)]);
        var aux = f.ByteOrder.Uint32(d[(int)(i + 8)..(int)(i + 12)]);
        var next = f.ByteOrder.Uint32(d[(int)(i + 12)..(int)(i + 16)]);
        var (file, _) = getString(str, ((nint)fileoff));
        @string name = default!;
        nint j = i + ((nint)aux);
        for (nint c = 0; c < ((nint)cnt); c++) {
            if (j + 16 > len(d)) {
                break;
            }
            // hash := f.ByteOrder.Uint32(d[j:j+4])
            // flags := f.ByteOrder.Uint16(d[j+4:j+6])
            var other = f.ByteOrder.Uint16(d[(int)(j + 6)..(int)(j + 8)]);
            var nameoff = f.ByteOrder.Uint32(d[(int)(j + 8)..(int)(j + 12)]);
            var nextΔ1 = f.ByteOrder.Uint32(d[(int)(j + 12)..(int)(j + 16)]);
            (name, _) = getString(str, ((nint)nameoff));
            nint ndx = ((nint)other);
            if (ndx >= len(need)) {
                var a = new slice<verneed>(2 * (ndx + 1));
                copy(a, need);
                need = a;
            }
            need[ndx] = new verneed(file, name);
            if (nextΔ1 == 0) {
                break;
            }
            j += ((nint)nextΔ1);
        }
        if (next == 0) {
            break;
        }
        i += ((nint)next);
    }
    // Versym parallels symbol table, indexing into verneed.
    var vs = f.SectionByType(SHT_GNU_VERSYM);
    if (vs == nil) {
        return false;
    }
    (d, _) = vs.Data();
    f.gnuNeed = need;
    f.gnuVersym = d;
    return true;
}

// gnuVersion adds Library and Version information to sym,
// which came from offset i of the symbol table.
[GoRecv] internal static (@string library, @string version) gnuVersion(this ref File f, nint i) {
    @string library = default!;
    @string version = default!;

    // Each entry is two bytes; skip undef entry at beginning.
    i = (i + 1) * 2;
    if (i >= len(f.gnuVersym)) {
        return (library, version);
    }
    var s = f.gnuVersym[(int)(i)..];
    if (len(s) < 2) {
        return (library, version);
    }
    nint j = ((nint)f.ByteOrder.Uint16(s));
    if (j < 2 || j >= len(f.gnuNeed)) {
        return (library, version);
    }
    var n = Ꮡ(f.gnuNeed[j]);
    return ((~n).File, (~n).Name);
}

// ImportedLibraries returns the names of all libraries
// referred to by the binary f that are expected to be
// linked with the binary at dynamic link time.
[GoRecv] public static (slice<@string>, error) ImportedLibraries(this ref File f) {
    return f.DynString(DT_NEEDED);
}

// DynString returns the strings listed for the given tag in the file's dynamic
// section.
//
// The tag must be one that takes string values: [DT_NEEDED], [DT_SONAME], [DT_RPATH], or
// [DT_RUNPATH].
[GoRecv] public static (slice<@string>, error) DynString(this ref File f, DynTag tag) {
    var exprᴛ1 = tag;
    if (exprᴛ1 == DT_NEEDED || exprᴛ1 == DT_SONAME || exprᴛ1 == DT_RPATH || exprᴛ1 == DT_RUNPATH) {
    }
    else { /* default: */
        return (default!, fmt.Errorf("non-string-valued tag %v"u8, tag));
    }

    var ds = f.SectionByType(SHT_DYNAMIC);
    if (ds == nil) {
        // not dynamic, so no libraries
        return (default!, default!);
    }
    (d, err) = ds.Data();
    if (err != default!) {
        return (default!, err);
    }
    nint dynSize = 8;
    if (f.Class == ELFCLASS64) {
        dynSize = 16;
    }
    if (len(d) % dynSize != 0) {
        return (default!, errors.New("length of dynamic section is not a multiple of dynamic entry size"u8));
    }
    (str, err) = f.stringTable(ds.Link);
    if (err != default!) {
        return (default!, err);
    }
    slice<@string> all = default!;
    while (len(d) > 0) {
        DynTag t = default!;
        uint64 v = default!;
        var exprᴛ2 = f.Class;
        if (exprᴛ2 == ELFCLASS32) {
            t = ((DynTag)f.ByteOrder.Uint32(d[0..4]));
            v = ((uint64)f.ByteOrder.Uint32(d[4..8]));
            d = d[8..];
        }
        else if (exprᴛ2 == ELFCLASS64) {
            t = ((DynTag)f.ByteOrder.Uint64(d[0..8]));
            v = f.ByteOrder.Uint64(d[8..16]);
            d = d[16..];
        }

        if (t == tag) {
            var (s, ok) = getString(str, ((nint)v));
            if (ok) {
                all = append(all, s);
            }
        }
    }
    return (all, default!);
}

// DynValue returns the values listed for the given tag in the file's dynamic
// section.
[GoRecv] public static (slice<uint64>, error) DynValue(this ref File f, DynTag tag) {
    var ds = f.SectionByType(SHT_DYNAMIC);
    if (ds == nil) {
        return (default!, default!);
    }
    (d, err) = ds.Data();
    if (err != default!) {
        return (default!, err);
    }
    nint dynSize = 8;
    if (f.Class == ELFCLASS64) {
        dynSize = 16;
    }
    if (len(d) % dynSize != 0) {
        return (default!, errors.New("length of dynamic section is not a multiple of dynamic entry size"u8));
    }
    // Parse the .dynamic section as a string of bytes.
    slice<uint64> vals = default!;
    while (len(d) > 0) {
        DynTag t = default!;
        uint64 v = default!;
        var exprᴛ1 = f.Class;
        if (exprᴛ1 == ELFCLASS32) {
            t = ((DynTag)f.ByteOrder.Uint32(d[0..4]));
            v = ((uint64)f.ByteOrder.Uint32(d[4..8]));
            d = d[8..];
        }
        else if (exprᴛ1 == ELFCLASS64) {
            t = ((DynTag)f.ByteOrder.Uint64(d[0..8]));
            v = f.ByteOrder.Uint64(d[8..16]);
            d = d[16..];
        }

        if (t == tag) {
            vals = append(vals, v);
        }
    }
    return (vals, default!);
}

[GoType] partial struct nobitsSectionReader {
}

[GoRecv] internal static (nint n, error err) ReadAt(this ref nobitsSectionReader _, slice<byte> p, int64 off) {
    nint n = default!;
    error err = default!;

    return (0, errors.New("unexpected read from SHT_NOBITS section"u8));
}

} // end elf_package
