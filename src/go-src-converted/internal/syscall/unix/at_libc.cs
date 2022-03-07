// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build aix || solaris
// +build aix solaris

// package unix -- go2cs converted at 2022 March 06 22:12:51 UTC
// import "internal/syscall/unix" ==> using unix = go.@internal.syscall.unix_package
// Original source: C:\Program Files\Go\src\internal\syscall\unix\at_libc.go
using syscall = go.syscall_package;
using @unsafe = go.@unsafe_package;

namespace go.@internal.syscall;

public static partial class unix_package {

    //go:linkname procFstatat libc_fstatat
    //go:linkname procOpenat libc_openat
    //go:linkname procUnlinkat libc_unlinkat
private static System.UIntPtr procFstatat = default;private static System.UIntPtr procOpenat = default;private static System.UIntPtr procUnlinkat = default;


public static error Unlinkat(nint dirfd, @string path, nint flags) {
    var (p, err) = syscall.BytePtrFromString(path);
    if (err != null) {
        return error.As(err)!;
    }
    var (_, _, errno) = syscall6(uintptr(@unsafe.Pointer(_addr_procUnlinkat)), 3, uintptr(dirfd), uintptr(@unsafe.Pointer(p)), uintptr(flags), 0, 0, 0);
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
    var (fd, _, errno) = syscall6(uintptr(@unsafe.Pointer(_addr_procOpenat)), 4, uintptr(dirfd), uintptr(@unsafe.Pointer(p)), uintptr(flags), uintptr(perm), 0, 0);
    if (errno != 0) {
        return (0, error.As(errno)!);
    }
    return (int(fd), error.As(null!)!);

}

public static error Fstatat(nint dirfd, @string path, ptr<syscall.Stat_t> _addr_stat, nint flags) {
    ref syscall.Stat_t stat = ref _addr_stat.val;

    var (p, err) = syscall.BytePtrFromString(path);
    if (err != null) {
        return error.As(err)!;
    }
    var (_, _, errno) = syscall6(uintptr(@unsafe.Pointer(_addr_procFstatat)), 4, uintptr(dirfd), uintptr(@unsafe.Pointer(p)), uintptr(@unsafe.Pointer(stat)), uintptr(flags), 0, 0);
    if (errno != 0) {
        return error.As(errno)!;
    }
    return error.As(null!)!;

}

} // end unix_package
