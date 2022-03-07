// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package poll -- go2cs converted at 2022 March 06 22:12:59 UTC
// import "internal/poll" ==> using poll = go.@internal.poll_package
// Original source: C:\Program Files\Go\src\internal\poll\fd_fsync_windows.go
using syscall = go.syscall_package;

namespace go.@internal;

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
    return error.As(syscall.Fsync(fd.Sysfd))!;

});

} // end poll_package
