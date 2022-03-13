// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build dragonfly || freebsd || linux || netbsd || openbsd
// +build dragonfly freebsd linux netbsd openbsd

// package poll -- go2cs converted at 2022 March 13 05:27:49 UTC
// import "internal/poll" ==> using poll = go.@internal.poll_package
// Original source: C:\Program Files\Go\src\internal\poll\fcntl_syscall.go
namespace go.@internal;

using unix = @internal.syscall.unix_package;
using syscall = syscall_package;

public static partial class poll_package {

private static (nint, error) fcntl(nint fd, nint cmd, nint arg) {
    nint _p0 = default;
    error _p0 = default!;

    var (r, _, e) = syscall.Syscall(unix.FcntlSyscall, uintptr(fd), uintptr(cmd), uintptr(arg));
    if (e != 0) {
        return (int(r), error.As(syscall.Errno(e))!);
    }
    return (int(r), error.As(null!)!);
}

} // end poll_package
