// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package buildid -- go2cs converted at 2022 March 13 05:57:54 UTC
// import "cmd/internal/buildid" ==> using buildid = go.cmd.@internal.buildid_package
// Original source: C:\Program Files\Go\src\cmd\internal\buildid\note.go
namespace go.cmd.@internal;

using bytes = bytes_package;
using elf = debug.elf_package;
using macho = debug.macho_package;
using binary = encoding.binary_package;
using fmt = fmt_package;
using io = io_package;
using fs = io.fs_package;
using os = os_package;

public static partial class buildid_package {

private static (slice<byte>, error) readAligned4(io.Reader r, int sz) {
    slice<byte> _p0 = default;
    error _p0 = default!;

    var full = (sz + 3) & ~3;
    var data = make_slice<byte>(full);
    var (_, err) = io.ReadFull(r, data);
    if (err != null) {
        return (null, error.As(err)!);
    }
    data = data[..(int)sz];
    return (data, error.As(null!)!);
}

public static (slice<byte>, error) ReadELFNote(@string filename, @string name, int typ) => func((defer, _, _) => {
    slice<byte> _p0 = default;
    error _p0 = default!;

    var (f, err) = elf.Open(filename);
    if (err != null) {
        return (null, error.As(err)!);
    }
    defer(f.Close());
    foreach (var (_, sect) in f.Sections) {
        if (sect.Type != elf.SHT_NOTE) {
            continue;
        }
        var r = sect.Open();
        while (true) {
            ref int namesize = ref heap(out ptr<int> _addr_namesize);            ref int descsize = ref heap(out ptr<int> _addr_descsize);            ref int noteType = ref heap(out ptr<int> _addr_noteType);

            err = binary.Read(r, f.ByteOrder, _addr_namesize);
            if (err != null) {
                if (err == io.EOF) {
                    break;
                }
                return (null, error.As(fmt.Errorf("read namesize failed: %v", err))!);
            }
            err = binary.Read(r, f.ByteOrder, _addr_descsize);
            if (err != null) {
                return (null, error.As(fmt.Errorf("read descsize failed: %v", err))!);
            }
            err = binary.Read(r, f.ByteOrder, _addr_noteType);
            if (err != null) {
                return (null, error.As(fmt.Errorf("read type failed: %v", err))!);
            }
            var (noteName, err) = readAligned4(r, namesize);
            if (err != null) {
                return (null, error.As(fmt.Errorf("read name failed: %v", err))!);
            }
            var (desc, err) = readAligned4(r, descsize);
            if (err != null) {
                return (null, error.As(fmt.Errorf("read desc failed: %v", err))!);
            }
            if (name == string(noteName) && typ == noteType) {
                return (desc, error.As(null!)!);
            }
        }
    }    return (null, error.As(null!)!);
});

private static slice<byte> elfGoNote = (slice<byte>)"Go\x00\x00";
private static slice<byte> elfGNUNote = (slice<byte>)"GNU\x00";

// The Go build ID is stored in a note described by an ELF PT_NOTE prog
// header. The caller has already opened filename, to get f, and read
// at least 4 kB out, in data.
private static (@string, error) readELF(@string name, ptr<os.File> _addr_f, slice<byte> data) {
    @string buildid = default;
    error err = default!;
    ref os.File f = ref _addr_f.val;
 
    // Assume the note content is in the data, already read.
    // Rewrite the ELF header to set shnum to 0, so that we can pass
    // the data to elf.NewFile and it will decode the Prog list but not
    // try to read the section headers and the string table from disk.
    // That's a waste of I/O when all we care about is the Prog list
    // and the one ELF note.

    if (elf.Class(data[elf.EI_CLASS]) == elf.ELFCLASS32) 
        data[48] = 0;
        data[49] = 0;
    else if (elf.Class(data[elf.EI_CLASS]) == elf.ELFCLASS64) 
        data[60] = 0;
        data[61] = 0;
        const nint elfGoBuildIDTag = 4;

    const nint gnuBuildIDTag = 3;



    var (ef, err) = elf.NewFile(bytes.NewReader(data));
    if (err != null) {
        return ("", error.As(addr(new fs.PathError(Path:name,Op:"parse",Err:err))!)!);
    }
    @string gnu = default;
    foreach (var (_, p) in ef.Progs) {
        if (p.Type != elf.PT_NOTE || p.Filesz < 16) {
            continue;
        }
        slice<byte> note = default;
        if (p.Off + p.Filesz < uint64(len(data))) {
            note = data[(int)p.Off..(int)p.Off + p.Filesz];
        }
        else
 { 
            // For some linkers, such as the Solaris linker,
            // the buildid may not be found in data (which
            // likely contains the first 16kB of the file)
            // or even the first few megabytes of the file
            // due to differences in note segment placement;
            // in that case, extract the note data manually.
            _, err = f.Seek(int64(p.Off), io.SeekStart);
            if (err != null) {
                return ("", error.As(err)!);
            }
            note = make_slice<byte>(p.Filesz);
            _, err = io.ReadFull(f, note);
            if (err != null) {
                return ("", error.As(err)!);
            }
        }
        var filesz = p.Filesz;
        var off = p.Off;
        while (filesz >= 16) {
            var nameSize = ef.ByteOrder.Uint32(note);
            var valSize = ef.ByteOrder.Uint32(note[(int)4..]);
            var tag = ef.ByteOrder.Uint32(note[(int)8..]);
            var nname = note[(int)12..(int)16];
            if (nameSize == 4 && 16 + valSize <= uint32(len(note)) && tag == elfGoBuildIDTag && bytes.Equal(nname, elfGoNote)) {
                return (string(note[(int)16..(int)16 + valSize]), error.As(null!)!);
            }
            if (nameSize == 4 && 16 + valSize <= uint32(len(note)) && tag == gnuBuildIDTag && bytes.Equal(nname, elfGNUNote)) {
                gnu = string(note[(int)16..(int)16 + valSize]);
            }
            nameSize = (nameSize + 3) & ~3;
            valSize = (valSize + 3) & ~3;
            var notesz = uint64(12 + nameSize + valSize);
            if (filesz <= notesz) {
                break;
            }
            off += notesz;
            var align = p.Align;
            var alignedOff = (off + align - 1) & ~(align - 1);
            notesz += alignedOff - off;
            off = alignedOff;
            filesz -= notesz;
            note = note[(int)notesz..];
        }
    }    if (gnu != "") {
        return (gnu, error.As(null!)!);
    }
    return ("", error.As(null!)!);
}

// The Go build ID is stored at the beginning of the Mach-O __text segment.
// The caller has already opened filename, to get f, and read a few kB out, in data.
// Sadly, that's not guaranteed to hold the note, because there is an arbitrary amount
// of other junk placed in the file ahead of the main text.
private static (@string, error) readMacho(@string name, ptr<os.File> _addr_f, slice<byte> data) {
    @string buildid = default;
    error err = default!;
    ref os.File f = ref _addr_f.val;
 
    // If the data we want has already been read, don't worry about Mach-O parsing.
    // This is both an optimization and a hedge against the Mach-O parsing failing
    // in the future due to, for example, the name of the __text section changing.
    {
        var (b, err) = readRaw(name, data);

        if (b != "" && err == null) {
            return (b, error.As(err)!);
        }
    }

    var (mf, err) = macho.NewFile(f);
    if (err != null) {
        return ("", error.As(addr(new fs.PathError(Path:name,Op:"parse",Err:err))!)!);
    }
    var sect = mf.Section("__text");
    if (sect == null) { 
        // Every binary has a __text section. Something is wrong.
        return ("", error.As(addr(new fs.PathError(Path:name,Op:"parse",Err:fmt.Errorf("cannot find __text section")))!)!);
    }
    var n = sect.Size;
    if (n > uint64(readSize)) {
        n = uint64(readSize);
    }
    var buf = make_slice<byte>(n);
    {
        var (_, err) = f.ReadAt(buf, int64(sect.Offset));

        if (err != null) {
            return ("", error.As(err)!);
        }
    }

    return readRaw(name, buf);
}

} // end buildid_package
