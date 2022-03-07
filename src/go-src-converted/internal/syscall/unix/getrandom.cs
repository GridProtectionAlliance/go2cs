// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build dragonfly || freebsd || linux
// +build dragonfly freebsd linux

// package unix -- go2cs converted at 2022 March 06 22:12:54 UTC
// import "internal/syscall/unix" ==> using unix = go.@internal.syscall.unix_package
// Original source: C:\Program Files\Go\src\internal\syscall\unix\getrandom.go
using atomic = go.sync.atomic_package;
using syscall = go.syscall_package;
using @unsafe = go.@unsafe_package;

namespace go.@internal.syscall;

public static partial class unix_package {

private static int getrandomUnsupported = default; // atomic

// GetRandomFlag is a flag supported by the getrandom system call.
public partial struct GetRandomFlag { // : System.UIntPtr
}

// GetRandom calls the getrandom system call.
public static (nint, error) GetRandom(slice<byte> p, GetRandomFlag flags) {
    nint n = default;
    error err = default!;

    if (len(p) == 0) {
        return (0, error.As(null!)!);
    }
    if (atomic.LoadInt32(_addr_getrandomUnsupported) != 0) {
        return (0, error.As(syscall.ENOSYS)!);
    }
    var (r1, _, errno) = syscall.Syscall(getrandomTrap, uintptr(@unsafe.Pointer(_addr_p[0])), uintptr(len(p)), uintptr(flags));
    if (errno != 0) {
        if (errno == syscall.ENOSYS) {
            atomic.StoreInt32(_addr_getrandomUnsupported, 1);
        }
        return (0, error.As(errno)!);

    }
    return (int(r1), error.As(null!)!);

}

} // end unix_package
