// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package socktest -- go2cs converted at 2020 August 29 08:36:26 UTC
// import "net/internal/socktest" ==> using socktest = go.net.@internal.socktest_package
// Original source: C:\Go\src\net\internal\socktest\sys_windows.go
using windows = go.@internal.syscall.windows_package;
using syscall = go.syscall_package;
using static go.builtin;

namespace go {
namespace net {
namespace @internal
{
    public static partial class socktest_package
    {
        // Socket wraps syscall.Socket.
        private static (syscall.Handle, error) Socket(this ref Switch _sw, long family, long sotype, long proto) => func(_sw, (ref Switch sw, Defer defer, Panic _, Recover __) =>
        {
            sw.once.Do(sw.init);

            Status so = ref new Status(Cookie:cookie(family,sotype,proto));
            sw.fmu.RLock();
            var (f, _) = sw.fltab[FilterSocket];
            sw.fmu.RUnlock();

            var (af, err) = f.apply(so);
            if (err != null)
            {
                return (syscall.InvalidHandle, err);
            }
            s, so.Err = syscall.Socket(family, sotype, proto);
            err = af.apply(so);

            if (err != null)
            {
                if (so.Err == null)
                {
                    syscall.Closesocket(s);
                }
                return (syscall.InvalidHandle, err);
            }
            sw.smu.Lock();
            defer(sw.smu.Unlock());
            if (so.Err != null)
            {
                sw.stats.getLocked(so.Cookie).OpenFailed;

                return (syscall.InvalidHandle, so.Err);
            }
            var nso = sw.addLocked(s, family, sotype, proto);
            sw.stats.getLocked(nso.Cookie).Opened;

            return (s, null);
        });

        // WSASocket wraps syscall.WSASocket.
        private static (syscall.Handle, error) WSASocket(this ref Switch _sw, int family, int sotype, int proto, ref syscall.WSAProtocolInfo _protinfo, uint group, uint flags) => func(_sw, _protinfo, (ref Switch sw, ref syscall.WSAProtocolInfo protinfo, Defer defer, Panic _, Recover __) =>
        {
            sw.once.Do(sw.init);

            Status so = ref new Status(Cookie:cookie(int(family),int(sotype),int(proto)));
            sw.fmu.RLock();
            var (f, _) = sw.fltab[FilterSocket];
            sw.fmu.RUnlock();

            var (af, err) = f.apply(so);
            if (err != null)
            {
                return (syscall.InvalidHandle, err);
            }
            s, so.Err = windows.WSASocket(family, sotype, proto, protinfo, group, flags);
            err = af.apply(so);

            if (err != null)
            {
                if (so.Err == null)
                {
                    syscall.Closesocket(s);
                }
                return (syscall.InvalidHandle, err);
            }
            sw.smu.Lock();
            defer(sw.smu.Unlock());
            if (so.Err != null)
            {
                sw.stats.getLocked(so.Cookie).OpenFailed;

                return (syscall.InvalidHandle, so.Err);
            }
            var nso = sw.addLocked(s, int(family), int(sotype), int(proto));
            sw.stats.getLocked(nso.Cookie).Opened;

            return (s, null);
        });

        // Closesocket wraps syscall.Closesocket.
        private static error Closesocket(this ref Switch _sw, syscall.Handle s) => func(_sw, (ref Switch sw, Defer defer, Panic _, Recover __) =>
        {
            var so = sw.sockso(s);
            if (so == null)
            {
                return error.As(syscall.Closesocket(s));
            }
            sw.fmu.RLock();
            var (f, _) = sw.fltab[FilterClose];
            sw.fmu.RUnlock();

            var (af, err) = f.apply(so);
            if (err != null)
            {
                return error.As(err);
            }
            so.Err = syscall.Closesocket(s);
            err = af.apply(so);

            if (err != null)
            {
                return error.As(err);
            }
            sw.smu.Lock();
            defer(sw.smu.Unlock());
            if (so.Err != null)
            {
                sw.stats.getLocked(so.Cookie).CloseFailed;

                return error.As(so.Err);
            }
            delete(sw.sotab, s);
            sw.stats.getLocked(so.Cookie).Closed;

            return error.As(null);
        });

        // Connect wraps syscall.Connect.
        private static error Connect(this ref Switch _sw, syscall.Handle s, syscall.Sockaddr sa) => func(_sw, (ref Switch sw, Defer defer, Panic _, Recover __) =>
        {
            var so = sw.sockso(s);
            if (so == null)
            {
                return error.As(syscall.Connect(s, sa));
            }
            sw.fmu.RLock();
            var (f, _) = sw.fltab[FilterConnect];
            sw.fmu.RUnlock();

            var (af, err) = f.apply(so);
            if (err != null)
            {
                return error.As(err);
            }
            so.Err = syscall.Connect(s, sa);
            err = af.apply(so);

            if (err != null)
            {
                return error.As(err);
            }
            sw.smu.Lock();
            defer(sw.smu.Unlock());
            if (so.Err != null)
            {
                sw.stats.getLocked(so.Cookie).ConnectFailed;

                return error.As(so.Err);
            }
            sw.stats.getLocked(so.Cookie).Connected;

            return error.As(null);
        });

        // ConnectEx wraps syscall.ConnectEx.
        private static error ConnectEx(this ref Switch _sw, syscall.Handle s, syscall.Sockaddr sa, ref byte _b, uint n, ref uint _nwr, ref syscall.Overlapped _o) => func(_sw, _b, _nwr, _o, (ref Switch sw, ref byte b, ref uint nwr, ref syscall.Overlapped o, Defer defer, Panic _, Recover __) =>
        {
            var so = sw.sockso(s);
            if (so == null)
            {
                return error.As(syscall.ConnectEx(s, sa, b, n, nwr, o));
            }
            sw.fmu.RLock();
            var (f, _) = sw.fltab[FilterConnect];
            sw.fmu.RUnlock();

            var (af, err) = f.apply(so);
            if (err != null)
            {
                return error.As(err);
            }
            so.Err = syscall.ConnectEx(s, sa, b, n, nwr, o);
            err = af.apply(so);

            if (err != null)
            {
                return error.As(err);
            }
            sw.smu.Lock();
            defer(sw.smu.Unlock());
            if (so.Err != null)
            {
                sw.stats.getLocked(so.Cookie).ConnectFailed;

                return error.As(so.Err);
            }
            sw.stats.getLocked(so.Cookie).Connected;

            return error.As(null);
        });

        // Listen wraps syscall.Listen.
        private static error Listen(this ref Switch _sw, syscall.Handle s, long backlog) => func(_sw, (ref Switch sw, Defer defer, Panic _, Recover __) =>
        {
            var so = sw.sockso(s);
            if (so == null)
            {
                return error.As(syscall.Listen(s, backlog));
            }
            sw.fmu.RLock();
            var (f, _) = sw.fltab[FilterListen];
            sw.fmu.RUnlock();

            var (af, err) = f.apply(so);
            if (err != null)
            {
                return error.As(err);
            }
            so.Err = syscall.Listen(s, backlog);
            err = af.apply(so);

            if (err != null)
            {
                return error.As(err);
            }
            sw.smu.Lock();
            defer(sw.smu.Unlock());
            if (so.Err != null)
            {
                sw.stats.getLocked(so.Cookie).ListenFailed;

                return error.As(so.Err);
            }
            sw.stats.getLocked(so.Cookie).Listened;

            return error.As(null);
        });

        // AcceptEx wraps syscall.AcceptEx.
        private static error AcceptEx(this ref Switch _sw, syscall.Handle ls, syscall.Handle @as, ref byte _b, uint rxdatalen, uint laddrlen, uint raddrlen, ref uint _rcvd, ref syscall.Overlapped _overlapped) => func(_sw, _b, _rcvd, _overlapped, (ref Switch sw, ref byte b, ref uint rcvd, ref syscall.Overlapped overlapped, Defer defer, Panic _, Recover __) =>
        {
            var so = sw.sockso(ls);
            if (so == null)
            {
                return error.As(syscall.AcceptEx(ls, as, b, rxdatalen, laddrlen, raddrlen, rcvd, overlapped));
            }
            sw.fmu.RLock();
            var (f, _) = sw.fltab[FilterAccept];
            sw.fmu.RUnlock();

            var (af, err) = f.apply(so);
            if (err != null)
            {
                return error.As(err);
            }
            so.Err = syscall.AcceptEx(ls, as, b, rxdatalen, laddrlen, raddrlen, rcvd, overlapped);
            err = af.apply(so);

            if (err != null)
            {
                return error.As(err);
            }
            sw.smu.Lock();
            defer(sw.smu.Unlock());
            if (so.Err != null)
            {
                sw.stats.getLocked(so.Cookie).AcceptFailed;

                return error.As(so.Err);
            }
            var nso = sw.addLocked(as, so.Cookie.Family(), so.Cookie.Type(), so.Cookie.Protocol());
            sw.stats.getLocked(nso.Cookie).Accepted;

            return error.As(null);
        });
    }
}}}
