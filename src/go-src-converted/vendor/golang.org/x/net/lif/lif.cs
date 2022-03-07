// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build solaris

// Package lif provides basic functions for the manipulation of
// logical network interfaces and interface addresses on Solaris.
//
// The package supports Solaris 11 or above.
// package lif -- go2cs converted at 2022 March 06 23:38:04 UTC
// import "vendor/golang.org/x/net/lif" ==> using lif = go.vendor.golang.org.x.net.lif_package
// Original source: C:\Program Files\Go\src\vendor\golang.org\x\net\lif\lif.go
using syscall = go.syscall_package;

namespace go.vendor.golang.org.x.net;

public static partial class lif_package {

private partial struct endpoint {
    public nint af;
    public System.UIntPtr s;
}

private static error close(this ptr<endpoint> _addr_ep) {
    ref endpoint ep = ref _addr_ep.val;

    return error.As(syscall.Close(int(ep.s)))!;
}

private static (slice<endpoint>, error) newEndpoints(nint af) {
    slice<endpoint> _p0 = default;
    error _p0 = default!;

    error lastErr = default!;
    slice<endpoint> eps = default;
    nint afs = new slice<nint>(new nint[] { sysAF_INET, sysAF_INET6 });
    if (af != sysAF_UNSPEC) {
        afs = new slice<nint>(new nint[] { af });
    }
    foreach (var (_, af) in afs) {
        var (s, err) = syscall.Socket(af, sysSOCK_DGRAM, 0);
        if (err != null) {
            lastErr = error.As(err)!;
            continue;
        }
        eps = append(eps, new endpoint(af:af,s:uintptr(s)));

    }    if (len(eps) == 0) {
        return (null, error.As(lastErr)!);
    }
    return (eps, error.As(null!)!);

}

} // end lif_package
