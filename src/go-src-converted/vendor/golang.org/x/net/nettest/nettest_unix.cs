// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build aix || darwin || dragonfly || freebsd || linux || netbsd || openbsd || solaris || zos
// +build aix darwin dragonfly freebsd linux netbsd openbsd solaris zos

// package nettest -- go2cs converted at 2022 March 06 23:38:10 UTC
// import "vendor/golang.org/x/net/nettest" ==> using nettest = go.vendor.golang.org.x.net.nettest_package
// Original source: C:\Program Files\Go\src\vendor\golang.org\x\net\nettest\nettest_unix.go
using syscall = go.syscall_package;

namespace go.vendor.golang.org.x.net;

public static partial class nettest_package {

private static bool supportsRawSocket() {
    foreach (var (_, af) in new slice<nint>(new nint[] { syscall.AF_INET, syscall.AF_INET6 })) {
        var (s, err) = syscall.Socket(af, syscall.SOCK_RAW, 0);
        if (err != null) {
            continue;
        }
        syscall.Close(s);
        return true;

    }    return false;

}

} // end nettest_package
