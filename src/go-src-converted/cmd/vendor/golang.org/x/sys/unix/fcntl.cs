// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build dragonfly || freebsd || linux || netbsd || openbsd
// +build dragonfly freebsd linux netbsd openbsd

// package unix -- go2cs converted at 2022 March 13 06:41:18 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\sys\unix\fcntl.go
namespace go.cmd.vendor.golang.org.x.sys;

using @unsafe = @unsafe_package;

public static partial class unix_package {

// fcntl64Syscall is usually SYS_FCNTL, but is overridden on 32-bit Linux
// systems by fcntl_linux_32bit.go to be SYS_FCNTL64.
private static System.UIntPtr fcntl64Syscall = SYS_FCNTL;

private static (nint, error) fcntl(nint fd, nint cmd, nint arg) {
    nint _p0 = default;
    error _p0 = default!;

    var (valptr, _, errno) = Syscall(fcntl64Syscall, uintptr(fd), uintptr(cmd), uintptr(arg));
    error err = default!;
    if (errno != 0) {
        err = error.As(errno)!;
    }
    return (int(valptr), error.As(err)!);
}

// FcntlInt performs a fcntl syscall on fd with the provided command and argument.
public static (nint, error) FcntlInt(System.UIntPtr fd, nint cmd, nint arg) {
    nint _p0 = default;
    error _p0 = default!;

    return fcntl(int(fd), cmd, arg);
}

// FcntlFlock performs a fcntl syscall for the F_GETLK, F_SETLK or F_SETLKW command.
public static error FcntlFlock(System.UIntPtr fd, nint cmd, ptr<Flock_t> _addr_lk) {
    ref Flock_t lk = ref _addr_lk.val;

    var (_, _, errno) = Syscall(fcntl64Syscall, fd, uintptr(cmd), uintptr(@unsafe.Pointer(lk)));
    if (errno == 0) {
        return error.As(null!)!;
    }
    return error.As(errno)!;
}

} // end unix_package
