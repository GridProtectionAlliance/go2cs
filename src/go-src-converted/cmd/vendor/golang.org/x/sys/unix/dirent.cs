// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build aix || darwin || dragonfly || freebsd || linux || netbsd || openbsd || solaris
// +build aix darwin dragonfly freebsd linux netbsd openbsd solaris

// package unix -- go2cs converted at 2022 March 06 23:26:31 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\sys\unix\dirent.go
using @unsafe = go.@unsafe_package;

namespace go.cmd.vendor.golang.org.x.sys;

public static partial class unix_package {

    // readInt returns the size-bytes unsigned integer in native byte order at offset off.
private static (ulong, bool) readInt(slice<byte> b, System.UIntPtr off, System.UIntPtr size) {
    ulong u = default;
    bool ok = default;

    if (len(b) < int(off + size)) {
        return (0, false);
    }
    if (isBigEndian) {
        return (readIntBE(b[(int)off..], size), true);
    }
    return (readIntLE(b[(int)off..], size), true);

}

private static ulong readIntBE(slice<byte> b, System.UIntPtr size) => func((_, panic, _) => {
    switch (size) {
        case 1: 
            return uint64(b[0]);
            break;
        case 2: 
            _ = b[1]; // bounds check hint to compiler; see golang.org/issue/14808
            return uint64(b[1]) | uint64(b[0]) << 8;

            break;
        case 4: 
            _ = b[3]; // bounds check hint to compiler; see golang.org/issue/14808
            return uint64(b[3]) | uint64(b[2]) << 8 | uint64(b[1]) << 16 | uint64(b[0]) << 24;

            break;
        case 8: 
            _ = b[7]; // bounds check hint to compiler; see golang.org/issue/14808
            return uint64(b[7]) | uint64(b[6]) << 8 | uint64(b[5]) << 16 | uint64(b[4]) << 24 | uint64(b[3]) << 32 | uint64(b[2]) << 40 | uint64(b[1]) << 48 | uint64(b[0]) << 56;

            break;
        default: 
            panic("syscall: readInt with unsupported size");
            break;
    }

});

private static ulong readIntLE(slice<byte> b, System.UIntPtr size) => func((_, panic, _) => {
    switch (size) {
        case 1: 
            return uint64(b[0]);
            break;
        case 2: 
            _ = b[1]; // bounds check hint to compiler; see golang.org/issue/14808
            return uint64(b[0]) | uint64(b[1]) << 8;

            break;
        case 4: 
            _ = b[3]; // bounds check hint to compiler; see golang.org/issue/14808
            return uint64(b[0]) | uint64(b[1]) << 8 | uint64(b[2]) << 16 | uint64(b[3]) << 24;

            break;
        case 8: 
            _ = b[7]; // bounds check hint to compiler; see golang.org/issue/14808
            return uint64(b[0]) | uint64(b[1]) << 8 | uint64(b[2]) << 16 | uint64(b[3]) << 24 | uint64(b[4]) << 32 | uint64(b[5]) << 40 | uint64(b[6]) << 48 | uint64(b[7]) << 56;

            break;
        default: 
            panic("syscall: readInt with unsupported size");
            break;
    }

});

// ParseDirent parses up to max directory entries in buf,
// appending the names to names. It returns the number of
// bytes consumed from buf, the number of entries added
// to names, and the new names slice.
public static (nint, nint, slice<@string>) ParseDirent(slice<byte> buf, nint max, slice<@string> names) {
    nint consumed = default;
    nint count = default;
    slice<@string> newnames = default;

    var origlen = len(buf);
    count = 0;
    while (max != 0 && len(buf) > 0) {
        var (reclen, ok) = direntReclen(buf);
        if (!ok || reclen > uint64(len(buf))) {
            return (origlen, count, names);
        }
        var rec = buf[..(int)reclen];
        buf = buf[(int)reclen..];
        var (ino, ok) = direntIno(rec);
        if (!ok) {
            break;
        }
        if (ino == 0) { // File absent in directory.
            continue;

        }
        const var namoff = uint64(@unsafe.Offsetof(new Dirent().Name));

        var (namlen, ok) = direntNamlen(rec);
        if (!ok || namoff + namlen > uint64(len(rec))) {
            break;
        }
        var name = rec[(int)namoff..(int)namoff + namlen];
        foreach (var (i, c) in name) {
            if (c == 0) {
                name = name[..(int)i];
                break;
            }
        }        if (string(name) == "." || string(name) == "..") {
            continue;
        }
        max--;
        count++;
        names = append(names, string(name));

    }
    return (origlen - len(buf), count, names);

}

} // end unix_package
