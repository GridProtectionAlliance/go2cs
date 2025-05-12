// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

/*
Package plan9obj implements access to Plan 9 a.out object files.

# Security

This package is not designed to be hardened against adversarial inputs, and is
outside the scope of https://go.dev/security/policy. In particular, only basic
validation is done when parsing object files. As such, care should be taken when
parsing untrusted inputs, as parsing malformed files may consume significant
resources, or cause panics.
*/
namespace go.debug;

using binary = encoding.binary_package;
using errors = errors_package;
using fmt = fmt_package;
using saferio = @internal.saferio_package;
using io = io_package;
using os = os_package;
using @internal;
using encoding;

partial class plan9obj_package {

// A FileHeader represents a Plan 9 a.out file header.
[GoType] partial struct FileHeader {
    public uint32 Magic;
    public uint32 Bss;
    public uint64 Entry;
    public nint PtrSize;
    public uint64 LoadAddress;
    public uint64 HdrSize;
}

// A File represents an open Plan 9 a.out file.
[GoType] partial struct File {
    public partial ref FileHeader FileHeader { get; }
    public slice<ж<ΔSection>> Sections;
    internal io_package.Closer closer;
}

// A SectionHeader represents a single Plan 9 a.out section header.
// This structure doesn't exist on-disk, but eases navigation
// through the object file.
[GoType] partial struct SectionHeader {
    public @string Name;
    public uint32 Size;
    public uint32 Offset;
}

// A Section represents a single section in a Plan 9 a.out file.
[GoType] partial struct ΔSection {
    public partial ref SectionHeader SectionHeader { get; }
    // Embed ReaderAt for ReadAt method.
    // Do not embed SectionReader directly
    // to avoid having Read and Seek.
    // If a client wants Read and Seek it must use
    // Open() to avoid fighting over the seek offset
    // with other clients.
    public partial ref io_package.ReaderAt ReaderAt { get; }
    internal ж<io_package.SectionReader> sr;
}

// Data reads and returns the contents of the Plan 9 a.out section.
[GoRecv] public static (slice<byte>, error) Data(this ref ΔSection s) {
    return saferio.ReadDataAt(~s.sr, ((uint64)s.Size), 0);
}

// Open returns a new ReadSeeker reading the Plan 9 a.out section.
[GoRecv] public static io.ReadSeeker Open(this ref ΔSection s) {
    return ~io.NewSectionReader(~s.sr, 0, 1 << (int)(63) - 1);
}

// A Symbol represents an entry in a Plan 9 a.out symbol table section.
[GoType] partial struct Sym {
    public uint64 Value;
    public rune Type;
    public @string Name;
}

/*
 * Plan 9 a.out reader
 */

// formatError is returned by some operations if the data does
// not have the correct format for an object file.
[GoType] partial struct formatError {
    internal nint off;
    internal @string msg;
    internal any val;
}

[GoRecv] internal static @string Error(this ref formatError e) {
    @string msg = e.msg;
    if (e.val != default!) {
        msg += fmt.Sprintf(" '%v'"u8, e.val);
    }
    msg += fmt.Sprintf(" in record at byte %#x"u8, e.off);
    return msg;
}

// Open opens the named file using [os.Open] and prepares it for use as a Plan 9 a.out binary.
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

internal static (uint32, error) parseMagic(slice<byte> magic) {
    var m = binary.BigEndian.Uint32(magic);
    var exprᴛ1 = m;
    if (exprᴛ1 == Magic386 || exprᴛ1 == MagicAMD64 || exprᴛ1 == MagicARM) {
        return (m, default!);
    }

    return (0, new formatError(0, "bad magic number", magic));
}

[GoType("dyn")] partial struct NewFile_type {
    internal @string name;
    internal uint32 size;
}

// NewFile creates a new [File] for accessing a Plan 9 binary in an underlying reader.
// The Plan 9 binary is expected to start at position 0 in the ReaderAt.
public static (ж<File>, error) NewFile(io.ReaderAt r) {
    var sr = io.NewSectionReader(r, 0, 1 << (int)(63) - 1);
    // Read and decode Plan 9 magic
    array<byte> magic = new(4);
    {
        var (_, errΔ1) = r.ReadAt(magic[..], 0); if (errΔ1 != default!) {
            return (default!, errΔ1);
        }
    }
    var (_, err) = parseMagic(magic[..]);
    if (err != default!) {
        return (default!, err);
    }
    var ph = @new<prog>();
    {
        var errΔ2 = binary.Read(~sr, binary.BigEndian, ph); if (errΔ2 != default!) {
            return (default!, errΔ2);
        }
    }
    var f = Ꮡ(new File(FileHeader: new FileHeader(
        Magic: (~ph).Magic,
        Bss: (~ph).Bss,
        Entry: ((uint64)(~ph).Entry),
        PtrSize: 4,
        LoadAddress: 4096,
        HdrSize: 4 * 8
    )
    ));
    if ((uint32)((~ph).Magic & Magic64) != 0) {
        {
            var errΔ3 = binary.Read(~sr, binary.BigEndian, Ꮡ(f.Entry)); if (errΔ3 != default!) {
                return (default!, errΔ3);
            }
        }
        f.PtrSize = 8;
        f.LoadAddress = 2097152;
        f.HdrSize += 8;
    }
    slice<struct{name string; size uint32}> sects = new struct{name string; size uint32}[]{
        new("text"u8, (~ph).Text),
        new("data"u8, (~ph).Data),
        new("syms"u8, (~ph).Syms),
        new("spsz"u8, (~ph).Spsz),
        new("pcsz"u8, (~ph).Pcsz)
    }.slice();
    f.val.Sections = new slice<ж<ΔSection>>(5);
    var off = ((uint32)f.HdrSize);
    foreach (var (i, sect) in sects) {
        var s = @new<ΔSection>();
        s.val.SectionHeader = new SectionHeader(
            Name: sect.name,
            Size: sect.size,
            Offset: off
        );
        off += sect.size;
        s.val.sr = io.NewSectionReader(r, ((int64)s.Offset), ((int64)s.Size));
        s.val.ReaderAt = s.val.sr;
        (~f).Sections[i] = s;
    }
    return (f, default!);
}

internal static error walksymtab(slice<byte> data, nint ptrsz, Func<sym, error> fn) {
    binary.ByteOrder order = binary.BigEndian;
    sym s = default!;
    var p = data;
    while (len(p) >= 4) {
        // Symbol type, value.
        if (len(p) < ptrsz) {
            return new formatError(len(data), "unexpected EOF", default!);
        }
        // fixed-width value
        if (ptrsz == 8){
            s.value = order.Uint64(p[0..8]);
            p = p[8..];
        } else {
            s.value = ((uint64)order.Uint32(p[0..4]));
            p = p[4..];
        }
        if (len(p) < 1) {
            return new formatError(len(data), "unexpected EOF", default!);
        }
        var typ = (byte)(p[0] & 127);
        s.typ = typ;
        p = p[1..];
        // Name.
        nint i = default!;
        nint nnul = default!;
        for (i = 0; i < len(p); i++) {
            if (p[i] == 0) {
                nnul = 1;
                break;
            }
        }
        switch (typ) {
        case (rune)'z' or (rune)'Z': {
            p = p[(int)(i + nnul)..];
            for (i = 0; i + 2 <= len(p); i += 2) {
                if (p[i] == 0 && p[i + 1] == 0) {
                    nnul = 2;
                    break;
                }
            }
            break;
        }}

        if (len(p) < i + nnul) {
            return new formatError(len(data), "unexpected EOF", default!);
        }
        s.name = p[0..(int)(i)];
        i += nnul;
        p = p[(int)(i)..];
        fn(s);
    }
    return default!;
}

// newTable decodes the Go symbol table in data,
// returning an in-memory representation.
internal static (slice<Sym>, error) newTable(slice<byte> symtab, nint ptrsz) {
    nint n = default!;
    var err = walksymtab(symtab, ptrsz, (sym s) => {
        n++;
        return default!;
    });
    if (err != default!) {
        return (default!, err);
    }
    var fname = new map<uint16, @string>();
    var syms = new slice<Sym>(0, n);
    err = walksymtab(symtab, ptrsz, 
    var fnameʗ1 = fname;
    var symsʗ1 = syms;
    (sym s) => {
        nint nΔ1 = len(symsʗ1);
        symsʗ1 = symsʗ1[0..(int)(nΔ1 + 1)];
        var ts = Ꮡ(symsʗ1, nΔ1);
        ts.val.Type = ((rune)s.typ);
        ts.val.Value = s.value;
        switch (s.typ) {
        default: {
            ts.val.Name = ((@string)s.name);
            break;
        }
        case (rune)'z' or (rune)'Z': {
            for (nint i = 0; i < len(s.name); i += 2) {
                ref var eltIdx = ref heap<uint16>(out var ᏑeltIdx);
                eltIdx = binary.BigEndian.Uint16(s.name[(int)(i)..(int)(i + 2)]);
                @string elt = fnameʗ1[eltIdx];
                var ok = fnameʗ1[eltIdx];
                if (!ok) {
                    return Ꮡ(new formatError(-1, "bad filename code", eltIdx));
                }
                {
                    nint nΔ3 = len((~ts).Name); if (nΔ3 > 0 && (~ts).Name[nΔ3 - 1] != (rune)'/') {
                        ts.val.Name += "/"u8;
                    }
                }
                ts.val.Name += elt;
            }
            break;
        }}

        switch (s.typ) {
        case (rune)'f': {
            fnameʗ1[((uint16)s.value)] = ts.val.Name;
            break;
        }}

        return default!;
    });
    if (err != default!) {
        return (default!, err);
    }
    return (syms, default!);
}

// ErrNoSymbols is returned by [File.Symbols] if there is no such section
// in the File.
public static error ErrNoSymbols = errors.New("no symbol section"u8);

// Symbols returns the symbol table for f.
[GoRecv] public static (slice<Sym>, error) Symbols(this ref File f) {
    var symtabSection = f.Section("syms"u8);
    if (symtabSection == nil) {
        return (default!, ErrNoSymbols);
    }
    (symtab, err) = symtabSection.Data();
    if (err != default!) {
        return (default!, errors.New("cannot load symbol section"u8));
    }
    return newTable(symtab, f.PtrSize);
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

} // end plan9obj_package
