// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package os -- go2cs converted at 2022 March 06 22:12:39 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Program Files\Go\src\os\dirent_freebsd.go
using syscall = go.syscall_package;
using @unsafe = go.@unsafe_package;

namespace go;

public static partial class os_package {

private static (ulong, bool) direntIno(slice<byte> buf) {
    ulong _p0 = default;
    bool _p0 = default;

    return readInt(buf, @unsafe.Offsetof(new syscall.Dirent().Fileno), @unsafe.Sizeof(new syscall.Dirent().Fileno));
}

private static (ulong, bool) direntReclen(slice<byte> buf) {
    ulong _p0 = default;
    bool _p0 = default;

    return readInt(buf, @unsafe.Offsetof(new syscall.Dirent().Reclen), @unsafe.Sizeof(new syscall.Dirent().Reclen));
}

private static (ulong, bool) direntNamlen(slice<byte> buf) {
    ulong _p0 = default;
    bool _p0 = default;

    return readInt(buf, @unsafe.Offsetof(new syscall.Dirent().Namlen), @unsafe.Sizeof(new syscall.Dirent().Namlen));
}

private static FileMode direntType(slice<byte> buf) {
    var off = @unsafe.Offsetof(new syscall.Dirent().Type);
    if (off >= uintptr(len(buf))) {
        return ~FileMode(0); // unknown
    }
    var typ = buf[off];

    if (typ == syscall.DT_BLK) 
        return ModeDevice;
    else if (typ == syscall.DT_CHR) 
        return ModeDevice | ModeCharDevice;
    else if (typ == syscall.DT_DIR) 
        return ModeDir;
    else if (typ == syscall.DT_FIFO) 
        return ModeNamedPipe;
    else if (typ == syscall.DT_LNK) 
        return ModeSymlink;
    else if (typ == syscall.DT_REG) 
        return 0;
    else if (typ == syscall.DT_SOCK) 
        return ModeSocket;
        return ~FileMode(0); // unknown
}

} // end os_package
