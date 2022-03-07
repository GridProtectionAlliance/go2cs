// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package unix -- go2cs converted at 2022 March 06 22:12:50 UTC
// import "internal/syscall/unix" ==> using unix = go.@internal.syscall.unix_package
// Original source: C:\Program Files\Go\src\internal\syscall\unix\at_darwin.go
using syscall = go.syscall_package;
using _@unsafe_ = go.@unsafe_package;

namespace go.@internal.syscall;

public static partial class unix_package {

public static error Unlinkat(nint dirfd, @string path, nint flags) {
    return error.As(unlinkat(dirfd, path, flags))!;
}

public static (nint, error) Openat(nint dirfd, @string path, nint flags, uint perm) {
    nint _p0 = default;
    error _p0 = default!;

    return openat(dirfd, path, flags, perm);
}

public static error Fstatat(nint dirfd, @string path, ptr<syscall.Stat_t> _addr_stat, nint flags) {
    ref syscall.Stat_t stat = ref _addr_stat.val;

    return error.As(fstatat(dirfd, path, _addr_stat, flags))!;
}

//go:linkname unlinkat syscall.unlinkat
private static error unlinkat(nint dirfd, @string path, nint flags);

//go:linkname openat syscall.openat
private static (nint, error) openat(nint dirfd, @string path, nint flags, uint perm);

//go:linkname fstatat syscall.fstatat
private static error fstatat(nint dirfd, @string path, ptr<syscall.Stat_t> stat, nint flags);

} // end unix_package
