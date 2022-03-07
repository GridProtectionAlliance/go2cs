// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build dragonfly || freebsd || illumos || linux || netbsd || openbsd
// +build dragonfly freebsd illumos linux netbsd openbsd

// package socktest -- go2cs converted at 2022 March 06 22:25:42 UTC
// import "net/internal/socktest" ==> using socktest = go.net.@internal.socktest_package
// Original source: C:\Program Files\Go\src\net\internal\socktest\sys_cloexec.go
using syscall = go.syscall_package;

namespace go.net.@internal;

public static partial class socktest_package {

    // Accept4 wraps syscall.Accept4.
private static (nint, syscall.Sockaddr, error) Accept4(this ptr<Switch> _addr_sw, nint s, nint flags) => func((defer, _, _) => {
    nint ns = default;
    syscall.Sockaddr sa = default;
    error err = default!;
    ref Switch sw = ref _addr_sw.val;

    var so = sw.sockso(s);
    if (so == null) {
        return syscall.Accept4(s, flags);
    }
    sw.fmu.RLock();
    var f = sw.fltab[FilterAccept];
    sw.fmu.RUnlock();

    var (af, err) = f.apply(so);
    if (err != null) {
        return (-1, null, error.As(err)!);
    }
    ns, sa, so.Err = syscall.Accept4(s, flags);
    err = af.apply(so);

    if (err != null) {
        if (so.Err == null) {
            syscall.Close(ns);
        }
        return (-1, null, error.As(err)!);

    }
    sw.smu.Lock();
    defer(sw.smu.Unlock());
    if (so.Err != null) {
        sw.stats.getLocked(so.Cookie).AcceptFailed;

        return (-1, null, error.As(so.Err)!);

    }
    var nso = sw.addLocked(ns, so.Cookie.Family(), so.Cookie.Type(), so.Cookie.Protocol());
    sw.stats.getLocked(nso.Cookie).Accepted;

    return (ns, sa, error.As(null!)!);

});

} // end socktest_package
