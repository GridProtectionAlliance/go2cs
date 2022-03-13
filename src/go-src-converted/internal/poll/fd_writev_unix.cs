// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build dragonfly || freebsd || linux || netbsd || openbsd
// +build dragonfly freebsd linux netbsd openbsd

// package poll -- go2cs converted at 2022 March 13 05:27:53 UTC
// import "internal/poll" ==> using poll = go.@internal.poll_package
// Original source: C:\Program Files\Go\src\internal\poll\fd_writev_unix.go
namespace go.@internal;

using syscall = syscall_package;
using @unsafe = @unsafe_package;

public static partial class poll_package {

private static (System.UIntPtr, error) writev(nint fd, slice<syscall.Iovec> iovecs) {
    System.UIntPtr _p0 = default;
    error _p0 = default!;

    System.UIntPtr r = default;    syscall.Errno e = default;
    while (true) {
        r, _, e = syscall.Syscall(syscall.SYS_WRITEV, uintptr(fd), uintptr(@unsafe.Pointer(_addr_iovecs[0])), uintptr(len(iovecs)));
        if (e != syscall.EINTR) {
            break;
        }
    }
    if (e != 0) {
        return (r, error.As(e)!);
    }
    return (r, error.As(null!)!);
}

} // end poll_package
