// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package buildid -- go2cs converted at 2022 March 06 22:46:40 UTC
// import "cmd/internal/buildid" ==> using buildid = go.cmd.@internal.buildid_package
// Original source: C:\Program Files\Go\src\cmd\internal\buildid\buildid.go
using bytes = go.bytes_package;
using elf = go.debug.elf_package;
using fmt = go.fmt_package;
using xcoff = go.@internal.xcoff_package;
using io = go.io_package;
using fs = go.io.fs_package;
using os = go.os_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using System;


namespace go.cmd.@internal;

public static partial class buildid_package {

private static var errBuildIDMalformed = fmt.Errorf("malformed object file");private static slice<byte> bangArch = (slice<byte>)"!<arch>";private static slice<byte> pkgdef = (slice<byte>)"__.PKGDEF";private static slice<byte> goobject = (slice<byte>)"go object ";private static slice<byte> buildid = (slice<byte>)"build id ";

// ReadFile reads the build ID from an archive or executable file.
public static (@string, error) ReadFile(@string name) => func((defer, _, _) => {
    @string id = default;
    error err = default!;

    var (f, err) = os.Open(name);
    if (err != null) {
        return ("", error.As(err)!);
    }
    defer(f.Close());

    var buf = make_slice<byte>(8);
    {
        var (_, err) = f.ReadAt(buf, 0);

        if (err != null) {
            return ("", error.As(err)!);
        }
    }

    if (string(buf) != "!<arch>\n") {
        if (string(buf) == "<bigaf>\n") {
            return readGccgoBigArchive(name, _addr_f);
        }
        return readBinary(name, _addr_f);

    }
    var data = make_slice<byte>(1024);
    var (n, err) = io.ReadFull(f, data);
    if (err != null && n == 0) {
        return ("", error.As(err)!);
    }
    Func<(@string, error)> tryGccgo = () => {
        return readGccgoArchive(name, _addr_f);
    }; 

    // Archive header.
    for (nint i = 0; >>MARKER:FOREXPRESSION_LEVEL_1<<; i++) { // returns during i==3
        var j = bytes.IndexByte(data, '\n');
        if (j < 0) {
            return tryGccgo();
        }
        var line = data[..(int)j];
        data = data[(int)j + 1..];
        switch (i) {
            case 0: 
                if (!bytes.Equal(line, bangArch)) {
                    return tryGccgo();
                }
                break;
            case 1: 
                if (!bytes.HasPrefix(line, pkgdef)) {
                    return tryGccgo();
                }
                break;
            case 2: 
                if (!bytes.HasPrefix(line, goobject)) {
                    return tryGccgo();
                }
                break;
            case 3: 
                if (!bytes.HasPrefix(line, buildid)) { 
                    // Found the object header, just doesn't have a build id line.
                    // Treat as successful, with empty build id.
                    return ("", error.As(null!)!);

                }

                var (id, err) = strconv.Unquote(string(line[(int)len(buildid)..]));
                if (err != null) {
                    return tryGccgo();
                }

                return (id, error.As(null!)!);

                break;
        }

    }

});

// readGccgoArchive tries to parse the archive as a standard Unix
// archive file, and fetch the build ID from the _buildid.o entry.
// The _buildid.o entry is written by (*Builder).gccgoBuildIDELFFile
// in cmd/go/internal/work/exec.go.
private static (@string, error) readGccgoArchive(@string name, ptr<os.File> _addr_f) {
    @string _p0 = default;
    error _p0 = default!;
    ref os.File f = ref _addr_f.val;

    Func<(@string, error)> bad = () => {
        return ("", error.As(addr(new fs.PathError(Op:"parse",Path:name,Err:errBuildIDMalformed))!)!);
    };

    var off = int64(8);
    while (true) {
        {
            var (_, err) = f.Seek(off, io.SeekStart);

            if (err != null) {
                return ("", error.As(err)!);
            } 

            // TODO(iant): Make a debug/ar package, and use it
            // here and in cmd/link.

        } 

        // TODO(iant): Make a debug/ar package, and use it
        // here and in cmd/link.
        array<byte> hdr = new array<byte>(60);
        {
            (_, err) = io.ReadFull(f, hdr[..]);

            if (err != null) {
                if (err == io.EOF) { 
                    // No more entries, no build ID.
                    return ("", error.As(null!)!);

                }

                return ("", error.As(err)!);

            }

        }

        off += 60;

        var sizeStr = strings.TrimSpace(string(hdr[(int)48..(int)58]));
        var (size, err) = strconv.ParseInt(sizeStr, 0, 64);
        if (err != null) {
            return bad();
        }
        var name = strings.TrimSpace(string(hdr[..(int)16]));
        if (name == "_buildid.o/") {
            var sr = io.NewSectionReader(f, off, size);
            var (e, err) = elf.NewFile(sr);
            if (err != null) {
                return bad();
            }
            var s = e.Section(".go.buildid");
            if (s == null) {
                return bad();
            }
            var (data, err) = s.Data();
            if (err != null) {
                return bad();
            }
            return (string(data), error.As(null!)!);
        }
        off += size;
        if (off & 1 != 0) {
            off++;
        }
    }

}

// readGccgoBigArchive tries to parse the archive as an AIX big
// archive file, and fetch the build ID from the _buildid.o entry.
// The _buildid.o entry is written by (*Builder).gccgoBuildIDXCOFFFile
// in cmd/go/internal/work/exec.go.
private static (@string, error) readGccgoBigArchive(@string name, ptr<os.File> _addr_f) {
    @string _p0 = default;
    error _p0 = default!;
    ref os.File f = ref _addr_f.val;

    Func<(@string, error)> bad = () => {
        return ("", error.As(addr(new fs.PathError(Op:"parse",Path:name,Err:errBuildIDMalformed))!)!);
    }; 

    // Read fixed-length header.
    {
        var (_, err) = f.Seek(0, io.SeekStart);

        if (err != null) {
            return ("", error.As(err)!);
        }
    }

    array<byte> flhdr = new array<byte>(128);
    {
        (_, err) = io.ReadFull(f, flhdr[..]);

        if (err != null) {
            return ("", error.As(err)!);
        }
    } 
    // Read first member offset.
    var offStr = strings.TrimSpace(string(flhdr[(int)68..(int)88]));
    var (off, err) = strconv.ParseInt(offStr, 10, 64);
    if (err != null) {
        return bad();
    }
    while (true) {
        if (off == 0) { 
            // No more entries, no build ID.
            return ("", error.As(null!)!);

        }
        {
            (_, err) = f.Seek(off, io.SeekStart);

            if (err != null) {
                return ("", error.As(err)!);
            } 
            // Read member header.

        } 
        // Read member header.
        array<byte> hdr = new array<byte>(112);
        {
            (_, err) = io.ReadFull(f, hdr[..]);

            if (err != null) {
                return ("", error.As(err)!);
            } 
            // Read member name length.

        } 
        // Read member name length.
        var namLenStr = strings.TrimSpace(string(hdr[(int)108..(int)112]));
        var (namLen, err) = strconv.ParseInt(namLenStr, 10, 32);
        if (err != null) {
            return bad();
        }
        if (namLen == 10) {
            array<byte> nam = new array<byte>(10);
            {
                (_, err) = io.ReadFull(f, nam[..]);

                if (err != null) {
                    return ("", error.As(err)!);
                }

            }

            if (string(nam[..]) == "_buildid.o") {
                var sizeStr = strings.TrimSpace(string(hdr[(int)0..(int)20]));
                var (size, err) = strconv.ParseInt(sizeStr, 10, 64);
                if (err != null) {
                    return bad();
                }
                off += int64(len(hdr)) + namLen + 2;
                if (off & 1 != 0) {
                    off++;
                }
                var sr = io.NewSectionReader(f, off, size);
                var (x, err) = xcoff.NewFile(sr);
                if (err != null) {
                    return bad();
                }
                var data = x.CSect(".go.buildid");
                if (data == null) {
                    return bad();
                }
                return (string(data), error.As(null!)!);
            }

        }
        offStr = strings.TrimSpace(string(hdr[(int)20..(int)40]));
        off, err = strconv.ParseInt(offStr, 10, 64);
        if (err != null) {
            return bad();
        }
    }

}

private static slice<byte> goBuildPrefix = (slice<byte>)"\xff Go build ID: \"";private static slice<byte> goBuildEnd = (slice<byte>)"\"\n \xff";private static slice<byte> elfPrefix = (slice<byte>)"\x7fELF";private static slice<byte> machoPrefixes = new slice<slice<byte>>(new slice<byte>[] { {0xfe,0xed,0xfa,0xce}, {0xfe,0xed,0xfa,0xcf}, {0xce,0xfa,0xed,0xfe}, {0xcf,0xfa,0xed,0xfe} });

private static nint readSize = 32 * 1024; // changed for testing

// readBinary reads the build ID from a binary.
//
// ELF binaries store the build ID in a proper PT_NOTE section.
//
// Other binary formats are not so flexible. For those, the linker
// stores the build ID as non-instruction bytes at the very beginning
// of the text segment, which should appear near the beginning
// of the file. This is clumsy but fairly portable. Custom locations
// can be added for other binary types as needed, like we did for ELF.
private static (@string, error) readBinary(@string name, ptr<os.File> _addr_f) {
    @string id = default;
    error err = default!;
    ref os.File f = ref _addr_f.val;
 
    // Read the first 32 kB of the binary file.
    // That should be enough to find the build ID.
    // In ELF files, the build ID is in the leading headers,
    // which are typically less than 4 kB, not to mention 32 kB.
    // In Mach-O files, there's no limit, so we have to parse the file.
    // On other systems, we're trying to read enough that
    // we get the beginning of the text segment in the read.
    // The offset where the text segment begins in a hello
    // world compiled for each different object format today:
    //
    //    Plan 9: 0x20
    //    Windows: 0x600
    //
    var data = make_slice<byte>(readSize);
    _, err = io.ReadFull(f, data);
    if (err == io.ErrUnexpectedEOF) {
        err = null;
    }
    if (err != null) {
        return ("", error.As(err)!);
    }
    if (bytes.HasPrefix(data, elfPrefix)) {
        return readELF(name, f, data);
    }
    foreach (var (_, m) in machoPrefixes) {
        if (bytes.HasPrefix(data, m)) {
            return readMacho(name, f, data);
        }
    }    return readRaw(name, data);

}

// readRaw finds the raw build ID stored in text segment data.
private static (@string, error) readRaw(@string name, slice<byte> data) {
    @string id = default;
    error err = default!;

    var i = bytes.Index(data, goBuildPrefix);
    if (i < 0) { 
        // Missing. Treat as successful but build ID empty.
        return ("", error.As(null!)!);

    }
    var j = bytes.Index(data[(int)i + len(goBuildPrefix)..], goBuildEnd);
    if (j < 0) {
        return ("", error.As(addr(new fs.PathError(Op:"parse",Path:name,Err:errBuildIDMalformed))!)!);
    }
    var quoted = data[(int)i + len(goBuildPrefix) - 1..(int)i + len(goBuildPrefix) + j + 1];
    id, err = strconv.Unquote(string(quoted));
    if (err != null) {
        return ("", error.As(addr(new fs.PathError(Op:"parse",Path:name,Err:errBuildIDMalformed))!)!);
    }
    return (id, error.As(null!)!);

}

// HashToString converts the hash h to a string to be recorded
// in package archives and binaries as part of the build ID.
// We use the first 120 bits of the hash (5 chunks of 24 bits each) and encode
// it in base64, resulting in a 20-byte string. Because this is only used for
// detecting the need to rebuild installed files (not for lookups
// in the object file cache), 120 bits are sufficient to drive the
// probability of a false "do not need to rebuild" decision to effectively zero.
// We embed two different hashes in archives and four in binaries,
// so cutting to 20 bytes is a significant savings when build IDs are displayed.
// (20*4+3 = 83 bytes compared to 64*4+3 = 259 bytes for the
// more straightforward option of printing the entire h in base64).
public static @string HashToString(array<byte> h) {
    h = h.Clone();

    const @string b64 = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_";

    const nint chunks = 5;

    array<byte> dst = new array<byte>(chunks * 4);
    for (nint i = 0; i < chunks; i++) {
        var v = uint32(h[3 * i]) << 16 | uint32(h[3 * i + 1]) << 8 | uint32(h[3 * i + 2]);
        dst[4 * i + 0] = b64[(v >> 18) & 0x3F];
        dst[4 * i + 1] = b64[(v >> 12) & 0x3F];
        dst[4 * i + 2] = b64[(v >> 6) & 0x3F];
        dst[4 * i + 3] = b64[v & 0x3F];
    }
    return string(dst[..]);
}

} // end buildid_package
