// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package pprof -- go2cs converted at 2022 March 13 05:28:34 UTC
// import "runtime/pprof" ==> using pprof = go.runtime.pprof_package
// Original source: C:\Program Files\Go\src\runtime\pprof\elf.go
namespace go.runtime;

using binary = encoding.binary_package;
using errors = errors_package;
using fmt = fmt_package;
using os = os_package;

public static partial class pprof_package {

private static var errBadELF = errors.New("malformed ELF binary");private static var errNoBuildID = errors.New("no NT_GNU_BUILD_ID found in ELF binary");

// elfBuildID returns the GNU build ID of the named ELF binary,
// without introducing a dependency on debug/elf and its dependencies.
private static (@string, error) elfBuildID(@string file) => func((defer, _, _) => {
    @string _p0 = default;
    error _p0 = default!;

    var buf = make_slice<byte>(256);
    var (f, err) = os.Open(file);
    if (err != null) {
        return ("", error.As(err)!);
    }
    defer(f.Close());

    {
        var (_, err) = f.ReadAt(buf[..(int)64], 0);

        if (err != null) {
            return ("", error.As(err)!);
        }
    } 

    // ELF file begins with \x7F E L F.
    if (buf[0] != 0x7F || buf[1] != 'E' || buf[2] != 'L' || buf[3] != 'F') {
        return ("", error.As(errBadELF)!);
    }
    binary.ByteOrder byteOrder = default;
    switch (buf[5]) {
        case 1: // little-endian
            byteOrder = binary.LittleEndian;
            break;
        case 2: // big-endian
            byteOrder = binary.BigEndian;
            break;
        default: 
            return ("", error.As(errBadELF)!);
            break;
    }

    nint shnum = default;
    long shoff = default;    long shentsize = default;

    switch (buf[4]) {
        case 1: // 32-bit file header
            shoff = int64(byteOrder.Uint32(buf[(int)32..]));
            shentsize = int64(byteOrder.Uint16(buf[(int)46..]));
            if (shentsize != 40) {
                return ("", error.As(errBadELF)!);
            }
            shnum = int(byteOrder.Uint16(buf[(int)48..]));
            break;
        case 2: // 64-bit file header
            shoff = int64(byteOrder.Uint64(buf[(int)40..]));
            shentsize = int64(byteOrder.Uint16(buf[(int)58..]));
            if (shentsize != 64) {
                return ("", error.As(errBadELF)!);
            }
            shnum = int(byteOrder.Uint16(buf[(int)60..]));
            break;
        default: 
            return ("", error.As(errBadELF)!);
            break;
    }

    for (nint i = 0; i < shnum; i++) {
        {
            (_, err) = f.ReadAt(buf[..(int)shentsize], shoff + int64(i) * shentsize);

            if (err != null) {
                return ("", error.As(err)!);
            }

        }
        {
            var typ = byteOrder.Uint32(buf[(int)4..]);

            if (typ != 7) { // SHT_NOTE
                continue;
            }

        }
        long off = default;        long size = default;

        if (shentsize == 40) { 
            // 32-bit section header
            off = int64(byteOrder.Uint32(buf[(int)16..]));
            size = int64(byteOrder.Uint32(buf[(int)20..]));
        }
        else
 { 
            // 64-bit section header
            off = int64(byteOrder.Uint64(buf[(int)24..]));
            size = int64(byteOrder.Uint64(buf[(int)32..]));
        }
        size += off;
        while (off < size) {
            {
                (_, err) = f.ReadAt(buf[..(int)16], off);

                if (err != null) { // room for header + name GNU\x00
                    return ("", error.As(err)!);
                }

            }
            var nameSize = int(byteOrder.Uint32(buf[(int)0..]));
            var descSize = int(byteOrder.Uint32(buf[(int)4..]));
            var noteType = int(byteOrder.Uint32(buf[(int)8..]));
            var descOff = off + int64(12 + (nameSize + 3) & ~3);
            off = descOff + int64((descSize + 3) & ~3);
            if (nameSize != 4 || noteType != 3 || buf[12] != 'G' || buf[13] != 'N' || buf[14] != 'U' || buf[15] != '\x00') { // want name GNU\x00 type 3 (NT_GNU_BUILD_ID)
                continue;
            }
            if (descSize > len(buf)) {
                return ("", error.As(errBadELF)!);
            }
            {
                (_, err) = f.ReadAt(buf[..(int)descSize], descOff);

                if (err != null) {
                    return ("", error.As(err)!);
                }

            }
            return (fmt.Sprintf("%x", buf[..(int)descSize]), error.As(null!)!);
        }
    }
    return ("", error.As(errNoBuildID)!);
});

} // end pprof_package
