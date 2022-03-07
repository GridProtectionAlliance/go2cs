// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package socktest -- go2cs converted at 2022 March 06 22:25:45 UTC
// import "net/internal/socktest" ==> using socktest = go.net.@internal.socktest_package
// Original source: C:\Program Files\Go\src\net\internal\socktest\sys_windows.go
using windows = go.@internal.syscall.windows_package;
using syscall = go.syscall_package;

namespace go.net.@internal;

public static partial class socktest_package {

    // Socket wraps syscall.Socket.
private static (syscall.Handle, error) Socket(this ptr<Switch> _addr_sw, nint family, nint sotype, nint proto) => func((defer, _, _) => {
    syscall.Handle s = default;
    error err = default!;
    ref Switch sw = ref _addr_sw.val;

    sw.once.Do(sw.init);

    ptr<Status> so = addr(new Status(Cookie:cookie(family,sotype,proto)));
    sw.fmu.RLock();
    var (f, _) = sw.fltab[FilterSocket];
    sw.fmu.RUnlock();

    var (af, err) = f.apply(so);
    if (err != null) {
        return (syscall.InvalidHandle, error.As(err)!);
    }
    s, so.Err = syscall.Socket(family, sotype, proto);
    err = af.apply(so);

    if (err != null) {
        if (so.Err == null) {
            syscall.Closesocket(s);
        }
        return (syscall.InvalidHandle, error.As(err)!);

    }
    sw.smu.Lock();
    defer(sw.smu.Unlock());
    if (so.Err != null) {
        sw.stats.getLocked(so.Cookie).OpenFailed;

        return (syscall.InvalidHandle, error.As(so.Err)!);

    }
    var nso = sw.addLocked(s, family, sotype, proto);
    sw.stats.getLocked(nso.Cookie).Opened;

    return (s, error.As(null!)!);

});

// WSASocket wraps syscall.WSASocket.
private static (syscall.Handle, error) WSASocket(this ptr<Switch> _addr_sw, int family, int sotype, int proto, ptr<syscall.WSAProtocolInfo> _addr_protinfo, uint group, uint flags) => func((defer, _, _) => {
    syscall.Handle s = default;
    error err = default!;
    ref Switch sw = ref _addr_sw.val;
    ref syscall.WSAProtocolInfo protinfo = ref _addr_protinfo.val;

    sw.once.Do(sw.init);

    ptr<Status> so = addr(new Status(Cookie:cookie(int(family),int(sotype),int(proto))));
    sw.fmu.RLock();
    var (f, _) = sw.fltab[FilterSocket];
    sw.fmu.RUnlock();

    var (af, err) = f.apply(so);
    if (err != null) {
        return (syscall.InvalidHandle, error.As(err)!);
    }
    s, so.Err = windows.WSASocket(family, sotype, proto, protinfo, group, flags);
    err = af.apply(so);

    if (err != null) {
        if (so.Err == null) {
            syscall.Closesocket(s);
        }
        return (syscall.InvalidHandle, error.As(err)!);

    }
    sw.smu.Lock();
    defer(sw.smu.Unlock());
    if (so.Err != null) {
        sw.stats.getLocked(so.Cookie).OpenFailed;

        return (syscall.InvalidHandle, error.As(so.Err)!);

    }
    var nso = sw.addLocked(s, int(family), int(sotype), int(proto));
    sw.stats.getLocked(nso.Cookie).Opened;

    return (s, error.As(null!)!);

});

// Closesocket wraps syscall.Closesocket.
private static error Closesocket(this ptr<Switch> _addr_sw, syscall.Handle s) => func((defer, _, _) => {
    error err = default!;
    ref Switch sw = ref _addr_sw.val;

    var so = sw.sockso(s);
    if (so == null) {
        return error.As(syscall.Closesocket(s))!;
    }
    sw.fmu.RLock();
    var (f, _) = sw.fltab[FilterClose];
    sw.fmu.RUnlock();

    var (af, err) = f.apply(so);
    if (err != null) {
        return error.As(err)!;
    }
    so.Err = syscall.Closesocket(s);
    err = af.apply(so);

    if (err != null) {
        return error.As(err)!;
    }
    sw.smu.Lock();
    defer(sw.smu.Unlock());
    if (so.Err != null) {
        sw.stats.getLocked(so.Cookie).CloseFailed;

        return error.As(so.Err)!;

    }
    delete(sw.sotab, s);
    sw.stats.getLocked(so.Cookie).Closed;

    return error.As(null!)!;

});

// Connect wraps syscall.Connect.
private static error Connect(this ptr<Switch> _addr_sw, syscall.Handle s, syscall.Sockaddr sa) => func((defer, _, _) => {
    error err = default!;
    ref Switch sw = ref _addr_sw.val;

    var so = sw.sockso(s);
    if (so == null) {
        return error.As(syscall.Connect(s, sa))!;
    }
    sw.fmu.RLock();
    var (f, _) = sw.fltab[FilterConnect];
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
        sw.stats.getLocked(so.Cookie).ConnectFailed;

        return error.As(so.Err)!;

    }
    sw.stats.getLocked(so.Cookie).Connected;

    return error.As(null!)!;

});

// ConnectEx wraps syscall.ConnectEx.
private static error ConnectEx(this ptr<Switch> _addr_sw, syscall.Handle s, syscall.Sockaddr sa, ptr<byte> _addr_b, uint n, ptr<uint> _addr_nwr, ptr<syscall.Overlapped> _addr_o) => func((defer, _, _) => {
    error err = default!;
    ref Switch sw = ref _addr_sw.val;
    ref byte b = ref _addr_b.val;
    ref uint nwr = ref _addr_nwr.val;
    ref syscall.Overlapped o = ref _addr_o.val;

    var so = sw.sockso(s);
    if (so == null) {
        return error.As(syscall.ConnectEx(s, sa, b, n, nwr, o))!;
    }
    sw.fmu.RLock();
    var (f, _) = sw.fltab[FilterConnect];
    sw.fmu.RUnlock();

    var (af, err) = f.apply(so);
    if (err != null) {
        return error.As(err)!;
    }
    so.Err = syscall.ConnectEx(s, sa, b, n, nwr, o);
    err = af.apply(so);

    if (err != null) {
        return error.As(err)!;
    }
    sw.smu.Lock();
    defer(sw.smu.Unlock());
    if (so.Err != null) {
        sw.stats.getLocked(so.Cookie).ConnectFailed;

        return error.As(so.Err)!;

    }
    sw.stats.getLocked(so.Cookie).Connected;

    return error.As(null!)!;

});

// Listen wraps syscall.Listen.
private static error Listen(this ptr<Switch> _addr_sw, syscall.Handle s, nint backlog) => func((defer, _, _) => {
    error err = default!;
    ref Switch sw = ref _addr_sw.val;

    var so = sw.sockso(s);
    if (so == null) {
        return error.As(syscall.Listen(s, backlog))!;
    }
    sw.fmu.RLock();
    var (f, _) = sw.fltab[FilterListen];
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
        sw.stats.getLocked(so.Cookie).ListenFailed;

        return error.As(so.Err)!;

    }
    sw.stats.getLocked(so.Cookie).Listened;

    return error.As(null!)!;

});

// AcceptEx wraps syscall.AcceptEx.
private static error AcceptEx(this ptr<Switch> _addr_sw, syscall.Handle ls, syscall.Handle @as, ptr<byte> _addr_b, uint rxdatalen, uint laddrlen, uint raddrlen, ptr<uint> _addr_rcvd, ptr<syscall.Overlapped> _addr_overlapped) => func((defer, _, _) => {
    ref Switch sw = ref _addr_sw.val;
    ref byte b = ref _addr_b.val;
    ref uint rcvd = ref _addr_rcvd.val;
    ref syscall.Overlapped overlapped = ref _addr_overlapped.val;

    var so = sw.sockso(ls);
    if (so == null) {
        return error.As(syscall.AcceptEx(ls, as, b, rxdatalen, laddrlen, raddrlen, rcvd, overlapped))!;
    }
    sw.fmu.RLock();
    var (f, _) = sw.fltab[FilterAccept];
    sw.fmu.RUnlock();

    var (af, err) = f.apply(so);
    if (err != null) {
        return error.As(err)!;
    }
    so.Err = syscall.AcceptEx(ls, as, b, rxdatalen, laddrlen, raddrlen, rcvd, overlapped);
    err = af.apply(so);

    if (err != null) {
        return error.As(err)!;
    }
    sw.smu.Lock();
    defer(sw.smu.Unlock());
    if (so.Err != null) {
        sw.stats.getLocked(so.Cookie).AcceptFailed;

        return error.As(so.Err)!;

    }
    var nso = sw.addLocked(as, so.Cookie.Family(), so.Cookie.Type(), so.Cookie.Protocol());
    sw.stats.getLocked(nso.Cookie).Accepted;

    return error.As(null!)!;

});

} // end socktest_package
