// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net.@internal;

using windows = @internal.syscall.windows_package;
using syscall = syscall_package;
using @internal.syscall;

partial class socktest_package {

// WSASocket wraps [syscall.WSASocket].
[GoRecv] public static (syscallꓸHandle s, error err) WSASocket(this ref Switch sw, int32 family, int32 sotype, int32 proto, ж<syscall.WSAProtocolInfo> Ꮡprotinfo, uint32 group, uint32 flags) => func((defer, _) => {
    syscallꓸHandle s = default!;
    error err = default!;

    ref var protinfo = ref Ꮡprotinfo.val;
    sw.once.Do(sw.init);
    var so = Ꮡ(new Status(Cookie: cookie(((nint)family), ((nint)sotype), ((nint)proto))));
    sw.fmu.RLock();
    var f = sw.fltab[FilterSocket];
    var _ = sw.fltab[FilterSocket];
    sw.fmu.RUnlock();
    (af, err) = f.apply(so);
    if (err != default!) {
        return (syscall.InvalidHandle, err);
    }
    (s, so.val.Err) = windows.WSASocket(family, sotype, proto, Ꮡprotinfo, group, flags);
    {
        err = af.apply(so); if (err != default!) {
            if ((~so).Err == default!) {
                syscall.Closesocket(s);
            }
            return (syscall.InvalidHandle, err);
        }
    }
    sw.smu.Lock();
    defer(sw.smu.Unlock);
    if ((~so).Err != default!) {
        (~sw.stats.getLocked((~so).Cookie)).OpenFailed++;
        return (syscall.InvalidHandle, (~so).Err);
    }
    var nso = sw.addLocked(s, ((nint)family), ((nint)sotype), ((nint)proto));
    (~sw.stats.getLocked((~nso).Cookie)).Opened++;
    return (s, default!);
});

// Closesocket wraps [syscall.Closesocket].
[GoRecv] public static error /*err*/ Closesocket(this ref Switch sw, syscallꓸHandle s) => func((defer, _) => {
    error err = default!;

    var so = sw.sockso(s);
    if (so == nil) {
        return syscall.Closesocket(s);
    }
    sw.fmu.RLock();
    var f = sw.fltab[FilterClose];
    var _ = sw.fltab[FilterClose];
    sw.fmu.RUnlock();
    (af, err) = f.apply(so);
    if (err != default!) {
        return err;
    }
    so.val.Err = syscall.Closesocket(s);
    {
        err = af.apply(so); if (err != default!) {
            return err;
        }
    }
    sw.smu.Lock();
    defer(sw.smu.Unlock);
    if ((~so).Err != default!) {
        (~sw.stats.getLocked((~so).Cookie)).CloseFailed++;
        return (~so).Err;
    }
    delete(sw.sotab, s);
    (~sw.stats.getLocked((~so).Cookie)).Closed++;
    return default!;
});

// Connect wraps [syscall.Connect].
[GoRecv] public static error /*err*/ Connect(this ref Switch sw, syscallꓸHandle s, syscallꓸSockaddr sa) => func((defer, _) => {
    error err = default!;

    var so = sw.sockso(s);
    if (so == nil) {
        return syscall.Connect(s, sa);
    }
    sw.fmu.RLock();
    var f = sw.fltab[FilterConnect];
    var _ = sw.fltab[FilterConnect];
    sw.fmu.RUnlock();
    (af, err) = f.apply(so);
    if (err != default!) {
        return err;
    }
    so.val.Err = syscall.Connect(s, sa);
    {
        err = af.apply(so); if (err != default!) {
            return err;
        }
    }
    sw.smu.Lock();
    defer(sw.smu.Unlock);
    if ((~so).Err != default!) {
        (~sw.stats.getLocked((~so).Cookie)).ConnectFailed++;
        return (~so).Err;
    }
    (~sw.stats.getLocked((~so).Cookie)).Connected++;
    return default!;
});

// ConnectEx wraps [syscall.ConnectEx].
[GoRecv] public static error /*err*/ ConnectEx(this ref Switch sw, syscallꓸHandle s, syscallꓸSockaddr sa, ж<byte> Ꮡb, uint32 n, ж<uint32> Ꮡnwr, ж<syscall.Overlapped> Ꮡo) => func((defer, _) => {
    error err = default!;

    ref var b = ref Ꮡb.val;
    ref var nwr = ref Ꮡnwr.val;
    ref var o = ref Ꮡo.val;
    var so = sw.sockso(s);
    if (so == nil) {
        return syscall.ConnectEx(s, sa, Ꮡb, n, Ꮡnwr, Ꮡo);
    }
    sw.fmu.RLock();
    var f = sw.fltab[FilterConnect];
    var _ = sw.fltab[FilterConnect];
    sw.fmu.RUnlock();
    (af, err) = f.apply(so);
    if (err != default!) {
        return err;
    }
    so.val.Err = syscall.ConnectEx(s, sa, Ꮡb, n, Ꮡnwr, Ꮡo);
    {
        err = af.apply(so); if (err != default!) {
            return err;
        }
    }
    sw.smu.Lock();
    defer(sw.smu.Unlock);
    if ((~so).Err != default!) {
        (~sw.stats.getLocked((~so).Cookie)).ConnectFailed++;
        return (~so).Err;
    }
    (~sw.stats.getLocked((~so).Cookie)).Connected++;
    return default!;
});

// Listen wraps [syscall.Listen].
[GoRecv] public static error /*err*/ Listen(this ref Switch sw, syscallꓸHandle s, nint backlog) => func((defer, _) => {
    error err = default!;

    var so = sw.sockso(s);
    if (so == nil) {
        return syscall.Listen(s, backlog);
    }
    sw.fmu.RLock();
    var f = sw.fltab[FilterListen];
    var _ = sw.fltab[FilterListen];
    sw.fmu.RUnlock();
    (af, err) = f.apply(so);
    if (err != default!) {
        return err;
    }
    so.val.Err = syscall.Listen(s, backlog);
    {
        err = af.apply(so); if (err != default!) {
            return err;
        }
    }
    sw.smu.Lock();
    defer(sw.smu.Unlock);
    if ((~so).Err != default!) {
        (~sw.stats.getLocked((~so).Cookie)).ListenFailed++;
        return (~so).Err;
    }
    (~sw.stats.getLocked((~so).Cookie)).Listened++;
    return default!;
});

// AcceptEx wraps [syscall.AcceptEx].
[GoRecv] public static error AcceptEx(this ref Switch sw, syscallꓸHandle ls, syscallꓸHandle @as, ж<byte> Ꮡb, uint32 rxdatalen, uint32 laddrlen, uint32 raddrlen, ж<uint32> Ꮡrcvd, ж<syscall.Overlapped> Ꮡoverlapped) => func((defer, _) => {
    ref var b = ref Ꮡb.val;
    ref var rcvd = ref Ꮡrcvd.val;
    ref var overlapped = ref Ꮡoverlapped.val;

    var so = sw.sockso(ls);
    if (so == nil) {
        return syscall.AcceptEx(ls, @as, Ꮡb, rxdatalen, laddrlen, raddrlen, Ꮡrcvd, Ꮡoverlapped);
    }
    sw.fmu.RLock();
    var f = sw.fltab[FilterAccept];
    var _ = sw.fltab[FilterAccept];
    sw.fmu.RUnlock();
    (af, err) = f.apply(so);
    if (err != default!) {
        return err;
    }
    so.val.Err = syscall.AcceptEx(ls, @as, Ꮡb, rxdatalen, laddrlen, raddrlen, Ꮡrcvd, Ꮡoverlapped);
    {
        err = af.apply(so); if (err != default!) {
            return err;
        }
    }
    sw.smu.Lock();
    defer(sw.smu.Unlock);
    if ((~so).Err != default!) {
        (~sw.stats.getLocked((~so).Cookie)).AcceptFailed++;
        return (~so).Err;
    }
    var nso = sw.addLocked(@as, (~so).Cookie.Family(), (~so).Cookie.Type(), (~so).Cookie.Protocol());
    (~sw.stats.getLocked((~nso).Cookie)).Accepted++;
    return default!;
});

} // end socktest_package
