// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net.@internal;

using windows = go.@internal.syscall.windows_package;
using syscall = syscall_package;
using go.@internal.syscall;

partial class socktest_package {

// WSASocket wraps [syscall.WSASocket].
public static (syscallꓸHandle s, error err) WSASocket(this ж<Switch> Ꮡsw, int32 family, int32 sotype, int32 proto, ж<syscall.WSAProtocolInfo> Ꮡprotinfo, uint32 group, uint32 flags) {
    syscallꓸHandle s = default!;
    error err = default!;
    func((defer, recover) => {
    ref var sw = ref Ꮡsw.Value;

        Ꮡsw.of(Switch.Ꮡonce).Do(Ꮡsw.init);
        var so = Ꮡ(new Status(Cookie: cookie((nint)family, (nint)sotype, (nint)proto)));
        Ꮡsw.of(Switch.Ꮡfmu).RLock();
        var (f, _) = sw.fltab[FilterSocket, ꟷ];
        Ꮡsw.of(Switch.Ꮡfmu).RUnlock();
        (var af, err) = f.apply(so);
        if (err != default!) {
            (s, err) = (syscall.InvalidHandle, err); return;
        }
        (s, so.Value.Err) = windows.WSASocket(family, sotype, proto, Ꮡprotinfo, group, flags);
        {
            err = af.apply(so); if (err != default!) {
                if ((~so).Err == default!) {
                    syscall.Closesocket(s);
                }
                (s, err) = (syscall.InvalidHandle, err); return;
            }
        }
        Ꮡsw.of(Switch.Ꮡsmu).Lock();
        defer(Ꮡsw.of(Switch.Ꮡsmu).Unlock);
        if ((~so).Err != default!) {
            sw.stats.getLocked((~so).Cookie).Value.OpenFailed++;
            (s, err) = (syscall.InvalidHandle, (~so).Err); return;
        }
        var nso = Ꮡsw.addLocked(s, (nint)family, (nint)sotype, (nint)proto);
        sw.stats.getLocked((~nso).Cookie).Value.Opened++;
        (s, err) = (s, default!);
    });
    return (s, err);
}

// Closesocket wraps [syscall.Closesocket].
public static error /*err*/ Closesocket(this ж<Switch> Ꮡsw, syscallꓸHandle s) {
    error err = default!;
    func((defer, recover) => {
    ref var sw = ref Ꮡsw.Value;

        var so = Ꮡsw.sockso(s);
        if (so == nil) {
            err = syscall.Closesocket(s); return;
        }
        Ꮡsw.of(Switch.Ꮡfmu).RLock();
        var (f, _) = sw.fltab[FilterClose, ꟷ];
        Ꮡsw.of(Switch.Ꮡfmu).RUnlock();
        (var af, err) = f.apply(so);
        if (err != default!) {
            return;
        }
        so.Value.Err = syscall.Closesocket(s);
        {
            err = af.apply(so); if (err != default!) {
                return;
            }
        }
        Ꮡsw.of(Switch.Ꮡsmu).Lock();
        defer(Ꮡsw.of(Switch.Ꮡsmu).Unlock);
        if ((~so).Err != default!) {
            sw.stats.getLocked((~so).Cookie).Value.CloseFailed++;
            err = (~so).Err; return;
        }
        delete(sw.sotab, s);
        sw.stats.getLocked((~so).Cookie).Value.Closed++;
        err = default!;
    });
    return err;
}

// Connect wraps [syscall.Connect].
public static error /*err*/ Connect(this ж<Switch> Ꮡsw, syscallꓸHandle s, syscallꓸSockaddr sa) {
    error err = default!;
    func((defer, recover) => {
    ref var sw = ref Ꮡsw.Value;

        var so = Ꮡsw.sockso(s);
        if (so == nil) {
            err = syscall.Connect(s, sa); return;
        }
        Ꮡsw.of(Switch.Ꮡfmu).RLock();
        var (f, _) = sw.fltab[FilterConnect, ꟷ];
        Ꮡsw.of(Switch.Ꮡfmu).RUnlock();
        (var af, err) = f.apply(so);
        if (err != default!) {
            return;
        }
        so.Value.Err = syscall.Connect(s, sa);
        {
            err = af.apply(so); if (err != default!) {
                return;
            }
        }
        Ꮡsw.of(Switch.Ꮡsmu).Lock();
        defer(Ꮡsw.of(Switch.Ꮡsmu).Unlock);
        if ((~so).Err != default!) {
            sw.stats.getLocked((~so).Cookie).Value.ConnectFailed++;
            err = (~so).Err; return;
        }
        sw.stats.getLocked((~so).Cookie).Value.Connected++;
        err = default!;
    });
    return err;
}

// ConnectEx wraps [syscall.ConnectEx].
public static error /*err*/ ConnectEx(this ж<Switch> Ꮡsw, syscallꓸHandle s, syscallꓸSockaddr sa, ж<byte> Ꮡb, uint32 n, ж<uint32> Ꮡnwr, ж<syscall.Overlapped> Ꮡo) {
    error err = default!;
    func((defer, recover) => {
    ref var sw = ref Ꮡsw.Value;

        var so = Ꮡsw.sockso(s);
        if (so == nil) {
            err = syscall.ConnectEx(s, sa, Ꮡb, n, Ꮡnwr, Ꮡo); return;
        }
        Ꮡsw.of(Switch.Ꮡfmu).RLock();
        var (f, _) = sw.fltab[FilterConnect, ꟷ];
        Ꮡsw.of(Switch.Ꮡfmu).RUnlock();
        (var af, err) = f.apply(so);
        if (err != default!) {
            return;
        }
        so.Value.Err = syscall.ConnectEx(s, sa, Ꮡb, n, Ꮡnwr, Ꮡo);
        {
            err = af.apply(so); if (err != default!) {
                return;
            }
        }
        Ꮡsw.of(Switch.Ꮡsmu).Lock();
        defer(Ꮡsw.of(Switch.Ꮡsmu).Unlock);
        if ((~so).Err != default!) {
            sw.stats.getLocked((~so).Cookie).Value.ConnectFailed++;
            err = (~so).Err; return;
        }
        sw.stats.getLocked((~so).Cookie).Value.Connected++;
        err = default!;
    });
    return err;
}

// Listen wraps [syscall.Listen].
public static error /*err*/ Listen(this ж<Switch> Ꮡsw, syscallꓸHandle s, nint backlog) {
    error err = default!;
    func((defer, recover) => {
    ref var sw = ref Ꮡsw.Value;

        var so = Ꮡsw.sockso(s);
        if (so == nil) {
            err = syscall.Listen(s, backlog); return;
        }
        Ꮡsw.of(Switch.Ꮡfmu).RLock();
        var (f, _) = sw.fltab[FilterListen, ꟷ];
        Ꮡsw.of(Switch.Ꮡfmu).RUnlock();
        (var af, err) = f.apply(so);
        if (err != default!) {
            return;
        }
        so.Value.Err = syscall.Listen(s, backlog);
        {
            err = af.apply(so); if (err != default!) {
                return;
            }
        }
        Ꮡsw.of(Switch.Ꮡsmu).Lock();
        defer(Ꮡsw.of(Switch.Ꮡsmu).Unlock);
        if ((~so).Err != default!) {
            sw.stats.getLocked((~so).Cookie).Value.ListenFailed++;
            err = (~so).Err; return;
        }
        sw.stats.getLocked((~so).Cookie).Value.Listened++;
        err = default!;
    });
    return err;
}

// AcceptEx wraps [syscall.AcceptEx].
public static error AcceptEx(this ж<Switch> Ꮡsw, syscallꓸHandle ls, syscallꓸHandle @as, ж<byte> Ꮡb, uint32 rxdatalen, uint32 laddrlen, uint32 raddrlen, ж<uint32> Ꮡrcvd, ж<syscall.Overlapped> Ꮡoverlapped) => func<error>((defer, recover) => {
    ref var sw = ref Ꮡsw.Value;

    var so = Ꮡsw.sockso(ls);
    if (so == nil) {
        return syscall.AcceptEx(ls, @as, Ꮡb, rxdatalen, laddrlen, raddrlen, Ꮡrcvd, Ꮡoverlapped);
    }
    Ꮡsw.of(Switch.Ꮡfmu).RLock();
    var (f, _) = sw.fltab[FilterAccept, ꟷ];
    Ꮡsw.of(Switch.Ꮡfmu).RUnlock();
    var (af, err) = f.apply(so);
    if (err != default!) {
        return err;
    }
    so.Value.Err = syscall.AcceptEx(ls, @as, Ꮡb, rxdatalen, laddrlen, raddrlen, Ꮡrcvd, Ꮡoverlapped);
    {
        err = af.apply(so); if (err != default!) {
            return err;
        }
    }
    Ꮡsw.of(Switch.Ꮡsmu).Lock();
    defer(Ꮡsw.of(Switch.Ꮡsmu).Unlock);
    if ((~so).Err != default!) {
        sw.stats.getLocked((~so).Cookie).Value.AcceptFailed++;
        return (~so).Err;
    }
    var nso = Ꮡsw.addLocked(@as, (~so).Cookie.Family(), (~so).Cookie.Type(), (~so).Cookie.Protocol());
    sw.stats.getLocked((~nso).Cookie).Value.Accepted++;
    return default!;
});

} // end socktest_package
