// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package os -- go2cs converted at 2022 March 06 22:12:41 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Program Files\Go\src\os\dirent_solaris.go
using syscall = go.syscall_package;
using @unsafe = go.@unsafe_package;

namespace go;

public static partial class os_package {

private static (ulong, bool) direntIno(slice<byte> buf) {
    ulong _p0 = default;
    bool _p0 = default;

    return readInt(buf, @unsafe.Offsetof(new syscall.Dirent().Ino), @unsafe.Sizeof(new syscall.Dirent().Ino));
}

private static (ulong, bool) direntReclen(slice<byte> buf) {
    ulong _p0 = default;
    bool _p0 = default;

    return readInt(buf, @unsafe.Offsetof(new syscall.Dirent().Reclen), @unsafe.Sizeof(new syscall.Dirent().Reclen));
}

private static (ulong, bool) direntNamlen(slice<byte> buf) {
    ulong _p0 = default;
    bool _p0 = default;

    var (reclen, ok) = direntReclen(buf);
    if (!ok) {
        return (0, false);
    }
    return (reclen - uint64(@unsafe.Offsetof(new syscall.Dirent().Name)), true);

}

private static FileMode direntType(slice<byte> buf) {
    return ~FileMode(0); // unknown
}

} // end os_package
