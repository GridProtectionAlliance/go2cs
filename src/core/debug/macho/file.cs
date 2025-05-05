// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

/*
Package macho implements access to Mach-O object files.

# Security

This package is not designed to be hardened against adversarial inputs, and is
outside the scope of https://go.dev/security/policy. In particular, only basic
validation is done when parsing object files. As such, care should be taken when
parsing untrusted inputs, as parsing malformed files may consume significant
resources, or cause panics.
*/
namespace go.debug;

// High level access to low level data structures.
using bytes = bytes_package;
using zlib = compress.zlib_package;
using dwarf = debug.dwarf_package;
using binary = encoding.binary_package;
using fmt = fmt_package;
using saferio = @internal.saferio_package;
using io = io_package;
using os = os_package;
using strings = strings_package;
using @internal;
using compress;
using encoding;

partial class macho_package {

// A File represents an open Mach-O file.
[GoType] partial struct File {
    public partial ref FileHeader FileHeader { get; }
    public encoding.binary_package.ByteOrder ByteOrder;
    public slice<Load> Loads;
    public slice<ж<ΔSection>> Sections;
    public ж<Symtab> Symtab;
    public ж<Dysymtab> Dysymtab;
    internal io_package.Closer closer;
}

// A Load represents any Mach-O load command.
[GoType] partial interface Load {
    slice<byte> Raw();
}

[GoType("[]byte")] partial struct LoadBytes;

public static slice<byte> Raw(this LoadBytes b) {
    return b;
}

// A SegmentHeader is the header for a Mach-O 32-bit or 64-bit load segment command.
[GoType] partial struct SegmentHeader {
    public LoadCmd Cmd;
    public uint32 Len;
    public @string Name;
    public uint64 Addr;
    public uint64 Memsz;
    public uint64 Offset;
    public uint64 Filesz;
    public uint32 Maxprot;
    public uint32 Prot;
    public uint32 Nsect;
    public uint32 Flag;
}

// A Segment represents a Mach-O 32-bit or 64-bit load segment command.
[GoType] partial struct ΔSegment {
    public partial ref LoadBytes LoadBytes { get; }
    public partial ref SegmentHeader SegmentHeader { get; }
    // Embed ReaderAt for ReadAt method.
    // Do not embed SectionReader directly
    // to avoid having Read and Seek.
    // If a client wants Read and Seek it must use
    // Open() to avoid fighting over the seek offset
    // with other clients.
    public partial ref io_package.ReaderAt ReaderAt { get; }
    internal ж<io_package.SectionReader> sr;
}

// Data reads and returns the contents of the segment.
[GoRecv] public static (slice<byte>, error) Data(this ref ΔSegment s) {
    return saferio.ReadDataAt(~s.sr, s.Filesz, 0);
}

// Open returns a new ReadSeeker reading the segment.
[GoRecv] public static io.ReadSeeker Open(this ref ΔSegment s) {
    return ~io.NewSectionReader(~s.sr, 0, 1 << (int)(63) - 1);
}

[GoType] partial struct SectionHeader {
    public @string Name;
    public @string Seg;
    public uint64 Addr;
    public uint64 Size;
    public uint32 Offset;
    public uint32 Align;
    public uint32 Reloff;
    public uint32 Nreloc;
    public uint32 Flags;
}

// A Reloc represents a Mach-O relocation.
[GoType] partial struct Reloc {
    public uint32 Addr;
    public uint32 Value;
    // when Scattered == false && Extern == true, Value is the symbol number.
    // when Scattered == false && Extern == false, Value is the section number.
    // when Scattered == true, Value is the value that this reloc refers to.
    public uint8 Type;
    public uint8 Len; // 0=byte, 1=word, 2=long, 3=quad
    public bool Pcrel;
    public bool Extern; // valid if Scattered == false
    public bool Scattered;
}

[GoType] partial struct ΔSection {
    public partial ref SectionHeader SectionHeader { get; }
    public slice<Reloc> Relocs;
    // Embed ReaderAt for ReadAt method.
    // Do not embed SectionReader directly
    // to avoid having Read and Seek.
    // If a client wants Read and Seek it must use
    // Open() to avoid fighting over the seek offset
    // with other clients.
    public partial ref io_package.ReaderAt ReaderAt { get; }
    internal ж<io_package.SectionReader> sr;
}

// Data reads and returns the contents of the Mach-O section.
[GoRecv] public static (slice<byte>, error) Data(this ref ΔSection s) {
    return saferio.ReadDataAt(~s.sr, s.Size, 0);
}

// Open returns a new ReadSeeker reading the Mach-O section.
[GoRecv] public static io.ReadSeeker Open(this ref ΔSection s) {
    return ~io.NewSectionReader(~s.sr, 0, 1 << (int)(63) - 1);
}

// A Dylib represents a Mach-O load dynamic library command.
[GoType] partial struct Dylib {
    public partial ref LoadBytes LoadBytes { get; }
    public @string Name;
    public uint32 Time;
    public uint32 CurrentVersion;
    public uint32 CompatVersion;
}

// A Symtab represents a Mach-O symbol table command.
[GoType] partial struct Symtab {
    public partial ref LoadBytes LoadBytes { get; }
    public partial ref SymtabCmd SymtabCmd { get; }
    public slice<Symbol> Syms;
}

// A Dysymtab represents a Mach-O dynamic symbol table command.
[GoType] partial struct Dysymtab {
    public partial ref LoadBytes LoadBytes { get; }
    public partial ref DysymtabCmd DysymtabCmd { get; }
    public slice<uint32> IndirectSyms; // indices into Symtab.Syms
}

// A Rpath represents a Mach-O rpath command.
[GoType] partial struct Rpath {
    public partial ref LoadBytes LoadBytes { get; }
    public @string Path;
}

// A Symbol is a Mach-O 32-bit or 64-bit symbol table entry.
[GoType] partial struct Symbol {
    public @string Name;
    public uint8 Type;
    public uint8 Sect;
    public uint16 Desc;
    public uint64 Value;
}

/*
 * Mach-O reader
 */

// FormatError is returned by some operations if the data does
// not have the correct format for an object file.
[GoType] partial struct FormatError {
    internal int64 off;
    internal @string msg;
    internal any val;
}

[GoRecv] public static @string Error(this ref FormatError e) {
    @string msg = e.msg;
    if (e.val != default!) {
        msg += fmt.Sprintf(" '%v'"u8, e.val);
    }
    msg += fmt.Sprintf(" in record at byte %#x"u8, e.off);
    return msg;
}

// Open opens the named file using [os.Open] and prepares it for use as a Mach-O binary.
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

// NewFile creates a new [File] for accessing a Mach-O binary in an underlying reader.
// The Mach-O binary is expected to start at position 0 in the ReaderAt.
public static (ж<File>, error) NewFile(io.ReaderAt r) {
    var f = @new<File>();
    var sr = io.NewSectionReader(r, 0, 1 << (int)(63) - 1);
    // Read and decode Mach magic to determine byte order, size.
    // Magic32 and Magic64 differ only in the bottom bit.
    array<byte> ident = new(4);
    {
        var (_, errΔ1) = r.ReadAt(ident[0..], 0); if (errΔ1 != default!) {
            return (default!, errΔ1);
        }
    }
    var be = binary.BigEndian.Uint32(ident[0..]);
    var le = binary.LittleEndian.Uint32(ident[0..]);
    switch ((uint32)(Magic32 & ~1)) {
    case (uint32)(be & ~1): {
        f.val.ByteOrder = binary.BigEndian;
        f.Magic = be;
        break;
    }
    case (uint32)(le & ~1): {
        f.val.ByteOrder = binary.LittleEndian;
        f.Magic = le;
        break;
    }
    default: {
        return (default!, new FormatError(0, "invalid magic number", default!));
    }}

    // Read entire file header.
    {
        var errΔ2 = binary.Read(~sr, (~f).ByteOrder, Ꮡ((~f).FileHeader)); if (errΔ2 != default!) {
            return (default!, errΔ2);
        }
    }
    // Then load commands.
    ref var offset = ref heap<int64>(out var Ꮡoffset);
    offset = ((int64)fileHeaderSize32);
    if (f.Magic == Magic64) {
        offset = fileHeaderSize64;
    }
    (dat, err) = saferio.ReadDataAt(r, ((uint64)f.Cmdsz), offset);
    if (err != default!) {
        return (default!, err);
    }
    nint c = saferio.SliceCap<Load>(((uint64)f.Ncmd));
    if (c < 0) {
        return (default!, new FormatError(offset, "too many load commands", default!));
    }
    f.val.Loads = new slice<Load>(0, c);
    var bo = f.val.ByteOrder;
    for (var i = ((uint32)0); i < f.Ncmd; i++) {
        // Each load command begins with uint32 command and length.
        if (len(dat) < 8) {
            return (default!, new FormatError(offset, "command block too small", default!));
        }
        var (cmd, siz) = (((LoadCmd)bo.Uint32(dat[0..4])), bo.Uint32(dat[4..8]));
        if (siz < 8 || siz > ((uint32)len(dat))) {
            return (default!, new FormatError(offset, "invalid command block size", default!));
        }
        slice<byte> cmddat = default!;
        (cmddat, dat) = (dat[0..(int)(siz)], dat[(int)(siz)..]);
        offset += ((int64)siz);
        ж<ΔSegment> s = default!;
        var exprᴛ1 = cmd;
        { /* default: */
            f.val.Loads = append((~f).Loads, ((LoadBytes)cmddat));
        }
        else if (exprᴛ1 == LoadCmdRpath) {
            ref var hdrΔ4 = ref heap(new RpathCmd(), out var ᏑhdrΔ4);
            var b = bytes.NewReader(cmddat);
            {
                var errΔ14 = binary.Read(~b, bo, ᏑhdrΔ4); if (errΔ14 != default!) {
                    return (default!, errΔ14);
                }
            }
            var l = @new<Rpath>();
            if (hdrΔ4.Path >= ((uint32)len(cmddat))) {
                return (default!, new FormatError(offset, "invalid path in rpath command", hdrΔ4.Path));
            }
            l.val.Path = cstring(cmddat[(int)(hdrΔ4.Path)..]);
            l.val.LoadBytes = ((LoadBytes)cmddat);
            f.val.Loads = append((~f).Loads, ~l);
        }
        else if (exprᴛ1 == LoadCmdDylib) {
            ref var hdrΔ5 = ref heap(new DylibCmd(), out var ᏑhdrΔ5);
            var b = bytes.NewReader(cmddat);
            {
                var errΔ15 = binary.Read(~b, bo, ᏑhdrΔ5); if (errΔ15 != default!) {
                    return (default!, errΔ15);
                }
            }
            var l = @new<Dylib>();
            if (hdrΔ5.Name >= ((uint32)len(cmddat))) {
                return (default!, new FormatError(offset, "invalid name in dynamic library command", hdrΔ5.Name));
            }
            l.val.Name = cstring(cmddat[(int)(hdrΔ5.Name)..]);
            l.val.Time = hdrΔ5.Time;
            l.val.CurrentVersion = hdrΔ5.CurrentVersion;
            l.val.CompatVersion = hdrΔ5.CompatVersion;
            l.val.LoadBytes = ((LoadBytes)cmddat);
            f.val.Loads = append((~f).Loads, ~l);
        }
        else if (exprᴛ1 == LoadCmdSymtab) {
            ref var hdrΔ6 = ref heap(new SymtabCmd(), out var ᏑhdrΔ6);
            var b = bytes.NewReader(cmddat);
            {
                var errΔ16 = binary.Read(~b, bo, ᏑhdrΔ6); if (errΔ16 != default!) {
                    return (default!, errΔ16);
                }
            }
            (strtab, err) = saferio.ReadDataAt(r, ((uint64)hdrΔ6.Strsize), ((int64)hdrΔ6.Stroff));
            if (err != default!) {
                return (default!, err);
            }
            nint symsz = default!;
            if (f.Magic == Magic64){
                symsz = 16;
            } else {
                symsz = 12;
            }
            (symdat, err) = saferio.ReadDataAt(r, ((uint64)hdrΔ6.Nsyms) * ((uint64)symsz), ((int64)hdrΔ6.Symoff));
            if (err != default!) {
                return (default!, err);
            }
            (st, err) = f.parseSymtab(symdat, strtab, cmddat, ᏑhdrΔ6, offset);
            if (err != default!) {
                return (default!, err);
            }
            f.val.Loads = append((~f).Loads, ~st);
            f.val.Symtab = st;
        }
        else if (exprᴛ1 == LoadCmdDysymtab) {
            ref var hdr = ref heap(new DysymtabCmd(), out var Ꮡhdr);
            var b = bytes.NewReader(cmddat);
            {
                var errΔ17 = binary.Read(~b, bo, Ꮡhdr); if (errΔ17 != default!) {
                    return (default!, errΔ17);
                }
            }
            if ((~f).Symtab == nil){
                return (default!, new FormatError(offset, "dynamic symbol table seen before any ordinary symbol table", default!));
            } else 
            if (hdr.Iundefsym > ((uint32)len((~(~f).Symtab).Syms))){
                return (default!, new FormatError(offset, fmt.Sprintf(
                    "undefined symbols index in dynamic symbol table command is greater than symbol table length (%d > %d)"u8,
                    hdr.Iundefsym, len((~(~f).Symtab).Syms)), default!));
            } else 
            if (hdr.Iundefsym + hdr.Nundefsym > ((uint32)len((~(~f).Symtab).Syms))) {
                return (default!, new FormatError(offset, fmt.Sprintf(
                    "number of undefined symbols after index in dynamic symbol table command is greater than symbol table length (%d > %d)"u8,
                    hdr.Iundefsym + hdr.Nundefsym, len((~(~f).Symtab).Syms)), default!));
            }
            (dat, err) = saferio.ReadDataAt(r, ((uint64)hdr.Nindirectsyms) * 4, ((int64)hdr.Indirectsymoff));
            if (err != default!) {
                return (default!, err);
            }
            var x = new slice<uint32>(hdr.Nindirectsyms);
            {
                var errΔ18 = binary.Read(~bytes.NewReader(dat), bo, x); if (errΔ18 != default!) {
                    return (default!, errΔ18);
                }
            }
            var st = @new<Dysymtab>();
            st.val.LoadBytes = ((LoadBytes)cmddat);
            st.val.DysymtabCmd = hdr;
            st.val.IndirectSyms = x;
            f.val.Loads = append((~f).Loads, ~st);
            f.val.Dysymtab = st;
        }
        else if (exprᴛ1 == LoadCmdSegment) {
            ref var seg32 = ref heap(new Segment32(), out var Ꮡseg32);
            var b = bytes.NewReader(cmddat);
            {
                var errΔ19 = binary.Read(~b, bo, Ꮡseg32); if (errΔ19 != default!) {
                    return (default!, errΔ19);
                }
            }
            s = @new<ΔSegment>();
            s.val.LoadBytes = cmddat;
            s.Cmd = cmd;
            s.Len = siz;
            s.Name = cstring(seg32.Name[0..]);
            s.Addr = ((uint64)seg32.Addr);
            s.Memsz = ((uint64)seg32.Memsz);
            s.Offset = ((uint64)seg32.Offset);
            s.Filesz = ((uint64)seg32.Filesz);
            s.Maxprot = seg32.Maxprot;
            s.Prot = seg32.Prot;
            s.Nsect = seg32.Nsect;
            s.Flag = seg32.Flag;
            f.val.Loads = append((~f).Loads, ~s);
            for (nint iΔ3 = 0; iΔ3 < ((nint)s.Nsect); iΔ3++) {
                ref var sh32 = ref heap(new Section32(), out var Ꮡsh32);
                {
                    var errΔ20 = binary.Read(~b, bo, Ꮡsh32); if (errΔ20 != default!) {
                        return (default!, errΔ20);
                    }
                }
                var sh = @new<ΔSection>();
                sh.Name = cstring(sh32.Name[0..]);
                sh.Seg = cstring(sh32.Seg[0..]);
                sh.Addr = ((uint64)sh32.Addr);
                sh.Size = ((uint64)sh32.Size);
                sh.Offset = sh32.Offset;
                sh.Align = sh32.Align;
                sh.Reloff = sh32.Reloff;
                sh.Nreloc = sh32.Nreloc;
                sh.Flags = sh32.Flags;
                {
                    var errΔ21 = f.pushSection(sh, r); if (errΔ21 != default!) {
                        return (default!, errΔ21);
                    }
                }
            }
        }
        if (exprᴛ1 == LoadCmdSegment64) {
            ref var seg64 = ref heap(new Segment64(), out var Ꮡseg64);
            var b = bytes.NewReader(cmddat);
            {
                var errΔ22 = binary.Read(~b, bo, Ꮡseg64); if (errΔ22 != default!) {
                    return (default!, errΔ22);
                }
            }
            s = @new<ΔSegment>();
            s.val.LoadBytes = cmddat;
            s.Cmd = cmd;
            s.Len = siz;
            s.Name = cstring(seg64.Name[0..]);
            s.Addr = seg64.Addr;
            s.Memsz = seg64.Memsz;
            s.Offset = seg64.Offset;
            s.Filesz = seg64.Filesz;
            s.Maxprot = seg64.Maxprot;
            s.Prot = seg64.Prot;
            s.Nsect = seg64.Nsect;
            s.Flag = seg64.Flag;
            f.val.Loads = append((~f).Loads, ~s);
            for (nint iΔ4 = 0; iΔ4 < ((nint)s.Nsect); iΔ4++) {
                ref var sh64 = ref heap(new Section64(), out var Ꮡsh64);
                {
                    var errΔ23 = binary.Read(~b, bo, Ꮡsh64); if (errΔ23 != default!) {
                        return (default!, errΔ23);
                    }
                }
                var sh = @new<ΔSection>();
                sh.Name = cstring(sh64.Name[0..]);
                sh.Seg = cstring(sh64.Seg[0..]);
                sh.Addr = sh64.Addr;
                sh.Size = sh64.Size;
                sh.Offset = sh64.Offset;
                sh.Align = sh64.Align;
                sh.Reloff = sh64.Reloff;
                sh.Nreloc = sh64.Nreloc;
                sh.Flags = sh64.Flags;
                {
                    var errΔ24 = f.pushSection(sh, r); if (errΔ24 != default!) {
                        return (default!, errΔ24);
                    }
                }
            }
        }

        if (s != nil) {
            if (((int64)s.Offset) < 0) {
                return (default!, new FormatError(offset, "invalid section offset", s.Offset));
            }
            if (((int64)s.Filesz) < 0) {
                return (default!, new FormatError(offset, "invalid section file size", s.Filesz));
            }
            s.val.sr = io.NewSectionReader(r, ((int64)s.Offset), ((int64)s.Filesz));
            s.val.ReaderAt = s.val.sr;
        }
    }
    return (f, default!);
}

[GoRecv] public static (ж<Symtab>, error) parseSymtab(this ref File f, slice<byte> symdat, slice<byte> strtab, slice<byte> cmddat, ж<SymtabCmd> Ꮡhdr, int64 offset) {
    ref var hdr = ref Ꮡhdr.val;

    var bo = f.ByteOrder;
    nint c = saferio.SliceCap<Symbol>(((uint64)hdr.Nsyms));
    if (c < 0) {
        return (default!, new FormatError(offset, "too many symbols", default!));
    }
    var symtab = new slice<Symbol>(0, c);
    var b = bytes.NewReader(symdat);
    for (nint i = 0; i < ((nint)hdr.Nsyms); i++) {
        ref var n = ref heap(new Nlist64(), out var Ꮡn);
        if (f.Magic == Magic64){
            {
                var err = binary.Read(~b, bo, Ꮡn); if (err != default!) {
                    return (default!, err);
                }
            }
        } else {
            ref var n32 = ref heap(new Nlist32(), out var Ꮡn32);
            {
                var err = binary.Read(~b, bo, Ꮡn32); if (err != default!) {
                    return (default!, err);
                }
            }
            n.Name = n32.Name;
            n.Type = n32.Type;
            n.Sect = n32.Sect;
            n.Desc = n32.Desc;
            n.Value = ((uint64)n32.Value);
        }
        if (n.Name >= ((uint32)len(strtab))) {
            return (default!, new FormatError(offset, "invalid name in symbol table", n.Name));
        }
        // We add "_" to Go symbols. Strip it here. See issue 33808.
        @string name = cstring(strtab[(int)(n.Name)..]);
        if (strings.Contains(name, "."u8) && name[0] == (rune)'_') {
            name = name[1..];
        }
        symtab = append(symtab, new Symbol(
            Name: name,
            Type: n.Type,
            Sect: n.Sect,
            Desc: n.Desc,
            Value: n.Value
        ));
    }
    var st = @new<Symtab>();
    st.val.LoadBytes = ((LoadBytes)cmddat);
    st.val.Syms = symtab;
    return (st, default!);
}

[GoType] partial struct relocInfo {
    public uint32 Addr;
    public uint32 Symnum;
}

[GoRecv] public static error pushSection(this ref File f, ж<ΔSection> Ꮡsh, io.ReaderAt r) {
    ref var sh = ref Ꮡsh.val;

    f.Sections = append(f.Sections, Ꮡsh);
    sh.sr = io.NewSectionReader(r, ((int64)sh.Offset), ((int64)sh.Size));
    sh.ReaderAt = sh.sr;
    if (sh.Nreloc > 0) {
        (reldat, err) = saferio.ReadDataAt(r, ((uint64)sh.Nreloc) * 8, ((int64)sh.Reloff));
        if (err != default!) {
            return err;
        }
        var b = bytes.NewReader(reldat);
        var bo = f.ByteOrder;
        sh.Relocs = new slice<Reloc>(sh.Nreloc);
        foreach (var (i, _) in sh.Relocs) {
            var rel = Ꮡ(sh.Relocs, i);
            ref var ri = ref heap(new relocInfo(), out var Ꮡri);
            {
                var errΔ1 = binary.Read(~b, bo, Ꮡri); if (errΔ1 != default!) {
                    return errΔ1;
                }
            }
            if ((uint32)(ri.Addr & (1 << (int)(31))) != 0){
                // scattered
                rel.val.Addr = (uint32)(ri.Addr & (1 << (int)(24) - 1));
                rel.val.Type = ((uint8)((uint32)((ri.Addr >> (int)(24)) & (1 << (int)(4) - 1))));
                rel.val.Len = ((uint8)((uint32)((ri.Addr >> (int)(28)) & (1 << (int)(2) - 1))));
                rel.val.Pcrel = (uint32)(ri.Addr & (1 << (int)(30))) != 0;
                rel.val.Value = ri.Symnum;
                rel.val.Scattered = true;
            } else {
                var exprᴛ1 = bo;
                if (exprᴛ1 == binary.LittleEndian) {
                    rel.val.Addr = ri.Addr;
                    rel.val.Value = (uint32)(ri.Symnum & (1 << (int)(24) - 1));
                    rel.val.Pcrel = (uint32)(ri.Symnum & (1 << (int)(24))) != 0;
                    rel.val.Len = ((uint8)((uint32)((ri.Symnum >> (int)(25)) & (1 << (int)(2) - 1))));
                    rel.val.Extern = (uint32)(ri.Symnum & (1 << (int)(27))) != 0;
                    rel.val.Type = ((uint8)((uint32)((ri.Symnum >> (int)(28)) & (1 << (int)(4) - 1))));
                }
                else if (exprᴛ1 == binary.BigEndian) {
                    rel.val.Addr = ri.Addr;
                    rel.val.Value = ri.Symnum >> (int)(8);
                    rel.val.Pcrel = (uint32)(ri.Symnum & (1 << (int)(7))) != 0;
                    rel.val.Len = ((uint8)((uint32)((ri.Symnum >> (int)(5)) & (1 << (int)(2) - 1))));
                    rel.val.Extern = (uint32)(ri.Symnum & (1 << (int)(4))) != 0;
                    rel.val.Type = ((uint8)((uint32)(ri.Symnum & (1 << (int)(4) - 1))));
                }
                else { /* default: */
                    throw panic("unreachable");
                }

            }
        }
    }
    return default!;
}

internal static @string cstring(slice<byte> b) {
    nint i = bytes.IndexByte(b, 0);
    if (i == -1) {
        i = len(b);
    }
    return ((@string)(b[0..(int)(i)]));
}

// Segment returns the first Segment with the given name, or nil if no such segment exists.
[GoRecv] public static ж<ΔSegment> Segment(this ref File f, @string name) {
    foreach (var (_, l) in f.Loads) {
        {
            var (s, ok) = l._<ΔSegment.val>(ᐧ); if (ok && s.Name == name) {
                return s;
            }
        }
    }
    return default!;
}

// Section returns the first section with the given name, or nil if no such
// section exists.
[GoRecv] public static ж<ΔSection> Section(this ref File f, @string name) {
    foreach (var (_, s) in f.Sections) {
        if (s.Name == name) {
            return s;
        }
    }
    return default!;
}

// DWARF returns the DWARF debug information for the Mach-O file.
[GoRecv] public static (ж<dwarf.Data>, error) DWARF(this ref File f) {
    var dwarfSuffix = (ж<ΔSection> s) => {
        switch (ᐧ) {
        case {} when strings.HasPrefix(s.Name, "__debug_"u8): {
            return s.Name[8..];
        }
        case {} when strings.HasPrefix(s.Name, "__zdebug_"u8): {
            return s.Name[9..];
        }
        default: {
            return ""u8;
        }}

    };
    var sectionData = (ж<ΔSection> s) => {
        (b, errΔ1) = s.Data();
        if (errΔ1 != default! && ((uint64)len(b)) < s.Size) {
            return (default!, errΔ1);
        }
        if (len(b) >= 12 && ((@string)(b[..4])) == "ZLIB"u8) {
            var dlen = binary.BigEndian.Uint64(b[4..12]);
            var dbuf = new slice<byte>(dlen);
            (r, errΔ2) = zlib.NewReader(~bytes.NewBuffer(b[12..]));
            if (errΔ2 != default!) {
                return (default!, errΔ2);
            }
            {
                var (_, errΔ3) = io.ReadFull(r, dbuf); if (errΔ3 != default!) {
                    return (default!, errΔ3);
                }
            }
            {
                var errΔ4 = r.Close(); if (errΔ4 != default!) {
                    return (default!, errΔ4);
                }
            }
            b = dbuf;
        }
        return (b, default!);
    };
    // There are many other DWARF sections, but these
    // are the ones the debug/dwarf package uses.
    // Don't bother loading others.
    map<@string, slice<byte>> dat = new map<@string, slice<byte>>{["abbrev"u8] = default!, ["info"u8] = default!, ["str"u8] = default!, ["line"u8] = default!, ["ranges"u8] = default!};
    foreach (var (_, s) in f.Sections) {
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
        (b, errΔ5) = sectionData(s);
        if (errΔ5 != default!) {
            return (default!, errΔ5);
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
        (b, err) = sectionData(s);
        if (err != default!) {
            return (default!, err);
        }
        if (suffix == "types"u8){
            err = d.AddTypes(fmt.Sprintf("types-%d"u8, i), b);
        } else {
            err = d.AddSection(".debug_"u8 + suffix, b);
        }
        if (err != default!) {
            return (default!, err);
        }
    }
    return (d, default!);
}

// ImportedSymbols returns the names of all symbols
// referred to by the binary f that are expected to be
// satisfied by other libraries at dynamic load time.
[GoRecv] public static (slice<@string>, error) ImportedSymbols(this ref File f) {
    if (f.Dysymtab == nil || f.Symtab == nil) {
        return (default!, new FormatError(0, "missing symbol table", default!));
    }
    var st = f.Symtab;
    var dt = f.Dysymtab;
    slice<@string> all = default!;
    foreach (var (_, s) in (~st).Syms[(int)(dt.Iundefsym)..(int)(dt.Iundefsym + dt.Nundefsym)]) {
        all = append(all, s.Name);
    }
    return (all, default!);
}

// ImportedLibraries returns the paths of all libraries
// referred to by the binary f that are expected to be
// linked with the binary at dynamic link time.
[GoRecv] public static (slice<@string>, error) ImportedLibraries(this ref File f) {
    slice<@string> all = default!;
    foreach (var (_, l) in f.Loads) {
        {
            var (lib, ok) = l._<Dylib.val>(ᐧ); if (ok) {
                all = append(all, (~lib).Name);
            }
        }
    }
    return (all, default!);
}

} // end macho_package
