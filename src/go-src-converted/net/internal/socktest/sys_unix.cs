// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build aix || darwin || dragonfly || freebsd || (js && wasm) || linux || netbsd || openbsd || solaris
// +build aix darwin dragonfly freebsd js,wasm linux netbsd openbsd solaris

// package socktest -- go2cs converted at 2022 March 13 05:40:15 UTC
// import "net/internal/socktest" ==> using socktest = go.net.@internal.socktest_package
// Original source: C:\Program Files\Go\src\net\internal\socktest\sys_unix.go
namespace go.net.@internal;

using syscall = syscall_package;

public static partial class socktest_package {

// Socket wraps syscall.Socket.
private static (nint, error) Socket(this ptr<Switch> _addr_sw, nint family, nint sotype, nint proto) => func((defer, _, _) => {
    nint s = default;
    error err = default!;
    ref Switch sw = ref _addr_sw.val;

    sw.once.Do(sw.init);

    ptr<Status> so = addr(new Status(Cookie:cookie(family,sotype,proto)));
    sw.fmu.RLock();
    var f = sw.fltab[FilterSocket];
    sw.fmu.RUnlock();

    var (af, err) = f.apply(so);
    if (err != null) {
        return (-1, error.As(err)!);
    }
    s, so.Err = syscall.Socket(family, sotype, proto);
    err = af.apply(so);

    if (err != null) {
        if (so.Err == null) {
            syscall.Close(s);
        }
        return (-1, error.As(err)!);
    }
    sw.smu.Lock();
    defer(sw.smu.Unlock());
    if (so.Err != null) {
        sw.stats.getLocked;

        (so.Cookie).OpenFailed++;
        return (-1, error.As(so.Err)!);
    }
    var nso = sw.addLocked(s, family, sotype, proto);
    sw.stats.getLocked;

    (nso.Cookie).Opened++;
    return (s, error.As(null!)!);
});

// Close wraps syscall.Close.
private static error Close(this ptr<Switch> _addr_sw, nint s) => func((defer, _, _) => {
    error err = default!;
    ref Switch sw = ref _addr_sw.val;

    var so = sw.sockso(s);
    if (so == null) {
        return error.As(syscall.Close(s))!;
    }
    sw.fmu.RLock();
    var f = sw.fltab[FilterClose];
    sw.fmu.RUnlock();

    var (af, err) = f.apply(so);
    if (err != null) {
        return error.As(err)!;
    }
    so.Err = syscall.Close(s);
    err = af.apply(so);

    if (err != null) {
        return error.As(err)!;
    }
    sw.smu.Lock();
    defer(sw.smu.Unlock());
    if (so.Err != null) {
        sw.stats.getLocked;

        (so.Cookie).CloseFailed++;
        return error.As(so.Err)!;
    }
    delete(sw.sotab, s);
    sw.stats.getLocked;

    (so.Cookie).Closed++;
    return error.As(null!)!;
});

// Connect wraps syscall.Connect.
private static error Connect(this ptr<Switch> _addr_sw, nint s, syscall.Sockaddr sa) => func((defer, _, _) => {
    error err = default!;
    ref Switch sw = ref _addr_sw.val;

    var so = sw.sockso(s);
    if (so == null) {
        return error.As(syscall.Connect(s, sa))!;
    }
    sw.fmu.RLock();
    var f = sw.fltab[FilterConnect];
    sw.fmu.RUnlock();

    var (af, err) = f.apply(so);
    if (err != null) {
        return error.As(err)!;
    }
    so.Err = syscall.Connect(s, sa);
    err = af.apply(so);

    if (err != null) {
        return error.As(err)!;
    }
    sw.smu.Lock();
    defer(sw.smu.Unlock());
    if (so.Err != null) {
        sw.stats.getLocked;

        (so.Cookie).ConnectFailed++;
        return error.As(so.Err)!;
    }
    sw.stats.getLocked;

    (so.Cookie).Connected++;
    return error.As(null!)!;
});

// Listen wraps syscall.Listen.
private static error Listen(this ptr<Switch> _addr_sw, nint s, nint backlog) => func((defer, _, _) => {
    error err = default!;
    ref Switch sw = ref _addr_sw.val;

    var so = sw.sockso(s);
    if (so == null) {
        return error.As(syscall.Listen(s, backlog))!;
    }
    sw.fmu.RLock();
    var f = sw.fltab[FilterListen];
    sw.fmu.RUnlock();

    var (af, err) = f.apply(so);
    if (err != null) {
        return error.As(err)!;
    }
    so.Err = syscall.Listen(s, backlog);
    err = af.apply(so);

    if (err != null) {
        return error.As(err)!;
    }
    sw.smu.Lock();
    defer(sw.smu.Unlock());
    if (so.Err != null) {
        sw.stats.getLocked;

        (so.Cookie).ListenFailed++;
        return error.As(so.Err)!;
    }
    sw.stats.getLocked;

    (so.Cookie).Listened++;
    return error.As(null!)!;
});

// Accept wraps syscall.Accept.
private static (nint, syscall.Sockaddr, error) Accept(this ptr<Switch> _addr_sw, nint s) => func((defer, _, _) => {
    nint ns = default;
    syscall.Sockaddr sa = default;
    error err = default!;
    ref Switch sw = ref _addr_sw.val;

    var so = sw.sockso(s);
    if (so == null) {
        return syscall.Accept(s);
    }
    sw.fmu.RLock();
    var f = sw.fltab[FilterAccept];
    sw.fmu.RUnlock();

    var (af, err) = f.apply(so);
    if (err != null) {
        return (-1, null, error.As(err)!);
    }
    ns, sa, so.Err = syscall.Accept(s);
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
        sw.stats.getLocked;

        (so.Cookie).AcceptFailed++;
        return (-1, null, error.As(so.Err)!);
    }
    var nso = sw.addLocked(ns, so.Cookie.Family(), so.Cookie.Type(), so.Cookie.Protocol());
    sw.stats.getLocked;

    (nso.Cookie).Accepted++;
    return (ns, sa, error.As(null!)!);
});

// GetsockoptInt wraps syscall.GetsockoptInt.
private static (nint, error) GetsockoptInt(this ptr<Switch> _addr_sw, nint s, nint level, nint opt) {
    nint soerr = default;
    error err = default!;
    ref Switch sw = ref _addr_sw.val;

    var so = sw.sockso(s);
    if (so == null) {
        return syscall.GetsockoptInt(s, level, opt);
    }
    sw.fmu.RLock();
    var f = sw.fltab[FilterGetsockoptInt];
    sw.fmu.RUnlock();

    var (af, err) = f.apply(so);
    if (err != null) {
        return (-1, error.As(err)!);
    }
    soerr, so.Err = syscall.GetsockoptInt(s, level, opt);
    so.SocketErr = syscall.Errno(soerr);
    err = af.apply(so);

    if (err != null) {
        return (-1, error.As(err)!);
    }
    if (so.Err != null) {
        return (-1, error.As(so.Err)!);
    }
    if (opt == syscall.SO_ERROR && (so.SocketErr == syscall.Errno(0) || so.SocketErr == syscall.EISCONN)) {
        sw.smu.Lock();
        sw.stats.getLocked;

        (so.Cookie).Connected++;
        sw.smu.Unlock();
    }
    return (soerr, error.As(null!)!);
}

} // end socktest_package
