// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build aix || darwin || dragonfly || freebsd || linux || netbsd || openbsd || solaris
// +build aix darwin dragonfly freebsd linux netbsd openbsd solaris

// package poll -- go2cs converted at 2022 March 06 22:13:21 UTC
// import "internal/poll" ==> using poll = go.@internal.poll_package
// Original source: C:\Program Files\Go\src\internal\poll\sockopt_unix.go
using syscall = go.syscall_package;

namespace go.@internal;

public static partial class poll_package {

    // SetsockoptByte wraps the setsockopt network call with a byte argument.
private static error SetsockoptByte(this ptr<FD> _addr_fd, nint level, nint name, byte arg) => func((defer, _, _) => {
    ref FD fd = ref _addr_fd.val;

    {
        var err = fd.incref();

        if (err != null) {
            return error.As(err)!;
        }
    }

    defer(fd.decref());
    return error.As(syscall.SetsockoptByte(fd.Sysfd, level, name, arg))!;

});

} // end poll_package
