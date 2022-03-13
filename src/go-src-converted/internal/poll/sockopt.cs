// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build aix || darwin || dragonfly || freebsd || linux || netbsd || openbsd || solaris || windows
// +build aix darwin dragonfly freebsd linux netbsd openbsd solaris windows

// package poll -- go2cs converted at 2022 March 13 05:27:53 UTC
// import "internal/poll" ==> using poll = go.@internal.poll_package
// Original source: C:\Program Files\Go\src\internal\poll\sockopt.go
namespace go.@internal;

using syscall = syscall_package;

public static partial class poll_package {

// SetsockoptInt wraps the setsockopt network call with an int argument.
private static error SetsockoptInt(this ptr<FD> _addr_fd, nint level, nint name, nint arg) => func((defer, _, _) => {
    ref FD fd = ref _addr_fd.val;

    {
        var err = fd.incref();

        if (err != null) {
            return error.As(err)!;
        }
    }
    defer(fd.decref());
    return error.As(syscall.SetsockoptInt(fd.Sysfd, level, name, arg))!;
});

// SetsockoptInet4Addr wraps the setsockopt network call with an IPv4 address.
private static error SetsockoptInet4Addr(this ptr<FD> _addr_fd, nint level, nint name, array<byte> arg) => func((defer, _, _) => {
    arg = arg.Clone();
    ref FD fd = ref _addr_fd.val;

    {
        var err = fd.incref();

        if (err != null) {
            return error.As(err)!;
        }
    }
    defer(fd.decref());
    return error.As(syscall.SetsockoptInet4Addr(fd.Sysfd, level, name, arg))!;
});

// SetsockoptLinger wraps the setsockopt network call with a Linger argument.
private static error SetsockoptLinger(this ptr<FD> _addr_fd, nint level, nint name, ptr<syscall.Linger> _addr_l) => func((defer, _, _) => {
    ref FD fd = ref _addr_fd.val;
    ref syscall.Linger l = ref _addr_l.val;

    {
        var err = fd.incref();

        if (err != null) {
            return error.As(err)!;
        }
    }
    defer(fd.decref());
    return error.As(syscall.SetsockoptLinger(fd.Sysfd, level, name, l))!;
});

} // end poll_package
