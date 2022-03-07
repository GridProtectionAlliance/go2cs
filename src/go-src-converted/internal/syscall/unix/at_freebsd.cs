// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package unix -- go2cs converted at 2022 March 06 22:12:50 UTC
// import "internal/syscall/unix" ==> using unix = go.@internal.syscall.unix_package
// Original source: C:\Program Files\Go\src\internal\syscall\unix\at_freebsd.go
using syscall = go.syscall_package;
using @unsafe = go.@unsafe_package;

namespace go.@internal.syscall;

public static partial class unix_package {

public static readonly nuint AT_REMOVEDIR = 0x800;
public static readonly nuint AT_SYMLINK_NOFOLLOW = 0x200;


public static error Unlinkat(nint dirfd, @string path, nint flags) {
    var (p, err) = syscall.BytePtrFromString(path);
    if (err != null) {
        return error.As(err)!;
    }
    var (_, _, errno) = syscall.Syscall(syscall.SYS_UNLINKAT, uintptr(dirfd), uintptr(@unsafe.Pointer(p)), uintptr(flags));
    if (errno != 0) {
        return error.As(errno)!;
    }
    return error.As(null!)!;

}

public static (nint, error) Openat(nint dirfd, @string path, nint flags, uint perm) {
    nint _p0 = default;
    error _p0 = default!;

    var (p, err) = syscall.BytePtrFromString(path);
    if (err != null) {
        return (0, error.As(err)!);
    }
    var (fd, _, errno) = syscall.Syscall6(syscall.SYS_OPENAT, uintptr(dirfd), uintptr(@unsafe.Pointer(p)), uintptr(flags), uintptr(perm), 0, 0);
    if (errno != 0) {
        return (0, error.As(errno)!);
    }
    return (int(fd), error.As(null!)!);

}

public static error Fstatat(nint dirfd, @string path, ptr<syscall.Stat_t> _addr_stat, nint flags) {
    ref syscall.Stat_t stat = ref _addr_stat.val;

    return error.As(syscall.Fstatat(dirfd, path, stat, flags))!;
}

} // end unix_package
