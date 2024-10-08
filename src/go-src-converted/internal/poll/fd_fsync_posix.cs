// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build aix || dragonfly || freebsd || (js && wasm) || linux || netbsd || openbsd || solaris
// +build aix dragonfly freebsd js,wasm linux netbsd openbsd solaris

// package poll -- go2cs converted at 2022 March 13 05:27:50 UTC
// import "internal/poll" ==> using poll = go.@internal.poll_package
// Original source: C:\Program Files\Go\src\internal\poll\fd_fsync_posix.go
namespace go.@internal;

using syscall = syscall_package;
using System;

public static partial class poll_package {

// Fsync wraps syscall.Fsync.
private static error Fsync(this ptr<FD> _addr_fd) => func((defer, _, _) => {
    ref FD fd = ref _addr_fd.val;

    {
        var err = fd.incref();

        if (err != null) {
            return error.As(err)!;
        }
    }
    defer(fd.decref());
    return error.As(ignoringEINTR(() => error.As(syscall.Fsync(fd.Sysfd))!))!;
});

} // end poll_package
