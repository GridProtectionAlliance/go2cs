// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build aix || darwin || dragonfly || freebsd || (js && wasm) || linux || netbsd || openbsd || solaris || windows
// +build aix darwin dragonfly freebsd js,wasm linux netbsd openbsd solaris windows

// package poll -- go2cs converted at 2022 March 13 05:27:51 UTC
// import "internal/poll" ==> using poll = go.@internal.poll_package
// Original source: C:\Program Files\Go\src\internal\poll\fd_posix.go
namespace go.@internal;

using io = io_package;
using syscall = syscall_package;


// eofError returns io.EOF when fd is available for reading end of
// file.

using System;
public static partial class poll_package {

private static error eofError(this ptr<FD> _addr_fd, nint n, error err) {
    ref FD fd = ref _addr_fd.val;

    if (n == 0 && err == null && fd.ZeroReadIsEOF) {
        return error.As(io.EOF)!;
    }
    return error.As(err)!;
}

// Shutdown wraps syscall.Shutdown.
private static error Shutdown(this ptr<FD> _addr_fd, nint how) => func((defer, _, _) => {
    ref FD fd = ref _addr_fd.val;

    {
        var err = fd.incref();

        if (err != null) {
            return error.As(err)!;
        }
    }
    defer(fd.decref());
    return error.As(syscall.Shutdown(fd.Sysfd, how))!;
});

// Fchown wraps syscall.Fchown.
private static error Fchown(this ptr<FD> _addr_fd, nint uid, nint gid) => func((defer, _, _) => {
    ref FD fd = ref _addr_fd.val;

    {
        var err = fd.incref();

        if (err != null) {
            return error.As(err)!;
        }
    }
    defer(fd.decref());
    return error.As(ignoringEINTR(() => error.As(syscall.Fchown(fd.Sysfd, uid, gid))!))!;
});

// Ftruncate wraps syscall.Ftruncate.
private static error Ftruncate(this ptr<FD> _addr_fd, long size) => func((defer, _, _) => {
    ref FD fd = ref _addr_fd.val;

    {
        var err = fd.incref();

        if (err != null) {
            return error.As(err)!;
        }
    }
    defer(fd.decref());
    return error.As(ignoringEINTR(() => error.As(syscall.Ftruncate(fd.Sysfd, size))!))!;
});

// RawControl invokes the user-defined function f for a non-IO
// operation.
private static error RawControl(this ptr<FD> _addr_fd, Action<System.UIntPtr> f) => func((defer, _, _) => {
    ref FD fd = ref _addr_fd.val;

    {
        var err = fd.incref();

        if (err != null) {
            return error.As(err)!;
        }
    }
    defer(fd.decref());
    f(uintptr(fd.Sysfd));
    return error.As(null!)!;
});

// ignoringEINTR makes a function call and repeats it if it returns
// an EINTR error. This appears to be required even though we install all
// signal handlers with SA_RESTART: see #22838, #38033, #38836, #40846.
// Also #20400 and #36644 are issues in which a signal handler is
// installed without setting SA_RESTART. None of these are the common case,
// but there are enough of them that it seems that we can't avoid
// an EINTR loop.
private static error ignoringEINTR(Func<error> fn) {
    while (true) {
        var err = fn();
        if (err != syscall.EINTR) {
            return error.As(err)!;
        }
    }
}

} // end poll_package
