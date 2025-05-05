// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file implements FindExportData.
namespace go.go.@internal;

using bufio = bufio_package;
using fmt = fmt_package;
using io = io_package;
using strconv = strconv_package;
using strings = strings_package;

partial class gcimporter_package {

internal static (@string name, nint size, error err) readGopackHeader(ж<bufio.Reader> Ꮡr) {
    @string name = default!;
    nint size = default!;
    error err = default!;

    ref var r = ref Ꮡr.val;
    // See $GOROOT/include/ar.h.
    var hdr = new slice<byte>(16 + 12 + 6 + 6 + 8 + 10 + 2);
    (_, err) = io.ReadFull(~r, hdr);
    if (err != default!) {
        return (name, size, err);
    }
    // leave for debugging
    if (false) {
        fmt.Printf("header: %s"u8, hdr);
    }
    @string s = strings.TrimSpace(((@string)(hdr[(int)(16 + 12 + 6 + 6 + 8)..][..10])));
    (size, err) = strconv.Atoi(s);
    if (err != default! || hdr[len(hdr) - 2] != (rune)'`' || hdr[len(hdr) - 1] != (rune)'\n') {
        err = fmt.Errorf("invalid archive header"u8);
        return (name, size, err);
    }
    name = strings.TrimSpace(((@string)(hdr[..16])));
    return (name, size, err);
}

// FindExportData positions the reader r at the beginning of the
// export data section of an underlying GC-created object/archive
// file by reading from it. The reader must be positioned at the
// start of the file before calling this function. The hdr result
// is the string before the export data, either "$$" or "$$B".
public static (@string hdr, nint size, error err) FindExportData(ж<bufio.Reader> Ꮡr) {
    @string hdr = default!;
    nint size = default!;
    error err = default!;

    ref var r = ref Ꮡr.val;
    // Read first line to make sure this is an object file.
    (line, err) = r.ReadSlice((rune)'\n');
    if (err != default!) {
        err = fmt.Errorf("can't find export data (%v)"u8, err);
        return (hdr, size, err);
    }
    if (((@string)line) == "!<arch>\n"u8) {
        // Archive file. Scan to __.PKGDEF.
        @string name = default!;
        {
            (name, size, err) = readGopackHeader(Ꮡr); if (err != default!) {
                return (hdr, size, err);
            }
        }
        // First entry should be __.PKGDEF.
        if (name != "__.PKGDEF"u8) {
            err = fmt.Errorf("go archive is missing __.PKGDEF"u8);
            return (hdr, size, err);
        }
        // Read first line of __.PKGDEF data, so that line
        // is once again the first line of the input.
        {
            (line, err) = r.ReadSlice((rune)'\n'); if (err != default!) {
                err = fmt.Errorf("can't find export data (%v)"u8, err);
                return (hdr, size, err);
            }
        }
    }
    // Now at __.PKGDEF in archive or still at beginning of file.
    // Either way, line should begin with "go object ".
    if (!strings.HasPrefix(((@string)line), "go object "u8)) {
        err = fmt.Errorf("not a Go object file"u8);
        return (hdr, size, err);
    }
    size -= len(line);
    // Skip over object header to export data.
    // Begins after first line starting with $$.
    while (line[0] != (rune)'$') {
        {
            (line, err) = r.ReadSlice((rune)'\n'); if (err != default!) {
                err = fmt.Errorf("can't find export data (%v)"u8, err);
                return (hdr, size, err);
            }
        }
        size -= len(line);
    }
    hdr = ((@string)line);
    return (hdr, size, err);
}

} // end gcimporter_package
