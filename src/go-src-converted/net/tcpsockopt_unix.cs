// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build aix || freebsd || linux || netbsd
// +build aix freebsd linux netbsd

// package net -- go2cs converted at 2022 March 13 05:30:09 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Program Files\Go\src\net\tcpsockopt_unix.go
namespace go;

using runtime = runtime_package;
using syscall = syscall_package;
using time = time_package;

public static partial class net_package {

private static error setKeepAlivePeriod(ptr<netFD> _addr_fd, time.Duration d) {
    ref netFD fd = ref _addr_fd.val;
 
    // The kernel expects seconds so round to next highest second.
    var secs = int(roundDurationUp(d, time.Second));
    {
        var err__prev1 = err;

        var err = fd.pfd.SetsockoptInt(syscall.IPPROTO_TCP, syscall.TCP_KEEPINTVL, secs);

        if (err != null) {
            return error.As(wrapSyscallError("setsockopt", err))!;
        }
        err = err__prev1;

    }
    err = fd.pfd.SetsockoptInt(syscall.IPPROTO_TCP, syscall.TCP_KEEPIDLE, secs);
    runtime.KeepAlive(fd);
    return error.As(wrapSyscallError("setsockopt", err))!;
}

} // end net_package
