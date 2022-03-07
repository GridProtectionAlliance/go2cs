// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package net -- go2cs converted at 2022 March 06 22:16:37 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Program Files\Go\src\net\sockopt_plan9.go
using syscall = go.syscall_package;

namespace go;

public static partial class net_package {

private static error setKeepAlive(ptr<netFD> _addr_fd, bool keepalive) {
    ref netFD fd = ref _addr_fd.val;

    if (keepalive) {
        var (_, e) = fd.ctl.WriteAt((slice<byte>)"keepalive", 0);
        return error.As(e)!;
    }
    return error.As(null!)!;

}

private static error setLinger(ptr<netFD> _addr_fd, nint sec) {
    ref netFD fd = ref _addr_fd.val;

    return error.As(syscall.EPLAN9)!;
}

} // end net_package
