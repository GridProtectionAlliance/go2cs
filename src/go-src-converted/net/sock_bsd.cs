// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build darwin || dragonfly || freebsd || netbsd || openbsd
// +build darwin dragonfly freebsd netbsd openbsd

// package net -- go2cs converted at 2022 March 13 05:30:06 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Program Files\Go\src\net\sock_bsd.go
namespace go;

using runtime = runtime_package;
using syscall = syscall_package;

public static partial class net_package {

private static nint maxListenerBacklog() {
    uint n = default;    error err = default!;
    switch (runtime.GOOS) {
        case "darwin": 

        case "ios": 
            n, err = syscall.SysctlUint32("kern.ipc.somaxconn");
            break;
        case "freebsd": 
            n, err = syscall.SysctlUint32("kern.ipc.soacceptqueue");
            break;
        case "netbsd": 

            break;
        case "openbsd": 
            n, err = syscall.SysctlUint32("kern.somaxconn");
            break;
    }
    if (n == 0 || err != null) {
        return syscall.SOMAXCONN;
    }
    if (n > 1 << 16 - 1) {
        n = 1 << 16 - 1;
    }
    return int(n);
}

} // end net_package
