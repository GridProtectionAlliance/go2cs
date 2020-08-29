// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin dragonfly freebsd linux nacl netbsd openbsd solaris

// package socktest -- go2cs converted at 2020 August 29 08:36:23 UTC
// import "net/internal/socktest" ==> using socktest = go.net.@internal.socktest_package
// Original source: C:\Go\src\net\internal\socktest\sys_unix.go
using syscall = go.syscall_package;
using static go.builtin;

namespace go {
namespace net {
namespace @internal
{
    public static partial class socktest_package
    {
        // Socket wraps syscall.Socket.
        private static (long, error) Socket(this ref Switch _sw, long family, long sotype, long proto) => func(_sw, (ref Switch sw, Defer defer, Panic _, Recover __) =>
        {
            sw.once.Do(sw.init);

            Status so = ref new Status(Cookie:cookie(family,sotype,proto));
            sw.fmu.RLock();
            var f = sw.fltab[FilterSocket];
            sw.fmu.RUnlock();

            var (af, err) = f.apply(so);
            if (err != null)
            {
                return (-1L, err);
            }
            s, so.Err = syscall.Socket(family, sotype, proto);
            err = af.apply(so);

            if (err != null)
            {
                if (so.Err == null)
                {
                    syscall.Close(s);
                }
                return (-1L, err);
            }
            sw.smu.Lock();
            defer(sw.smu.Unlock());
            if (so.Err != null)
            {
                sw.stats.getLocked(so.Cookie).OpenFailed;

                return (-1L, so.Err);
            }
            var nso = sw.addLocked(s, family, sotype, proto);
            sw.stats.getLocked(nso.Cookie).Opened;

            return (s, null);
        });

        // Close wraps syscall.Close.
        private static error Close(this ref Switch _sw, long s) => func(_sw, (ref Switch sw, Defer defer, Panic _, Recover __) =>
        {
            var so = sw.sockso(s);
            if (so == null)
            {
                return error.As(syscall.Close(s));
            }
            sw.fmu.RLock();
            var f = sw.fltab[FilterClose];
            sw.fmu.RUnlock();

            var (af, err) = f.apply(so);
            if (err != null)
            {
                return error.As(err);
            }
            so.Err = syscall.Close(s);
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
        private static error Connect(this ref Switch _sw, long s, syscall.Sockaddr sa) => func(_sw, (ref Switch sw, Defer defer, Panic _, Recover __) =>
        {
            var so = sw.sockso(s);
            if (so == null)
            {
                return error.As(syscall.Connect(s, sa));
            }
            sw.fmu.RLock();
            var f = sw.fltab[FilterConnect];
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

        // Listen wraps syscall.Listen.
        private static error Listen(this ref Switch _sw, long s, long backlog) => func(_sw, (ref Switch sw, Defer defer, Panic _, Recover __) =>
        {
            var so = sw.sockso(s);
            if (so == null)
            {
                return error.As(syscall.Listen(s, backlog));
            }
            sw.fmu.RLock();
            var f = sw.fltab[FilterListen];
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

        // Accept wraps syscall.Accept.
        private static (long, syscall.Sockaddr, error) Accept(this ref Switch _sw, long s) => func(_sw, (ref Switch sw, Defer defer, Panic _, Recover __) =>
        {
            var so = sw.sockso(s);
            if (so == null)
            {
                return syscall.Accept(s);
            }
            sw.fmu.RLock();
            var f = sw.fltab[FilterAccept];
            sw.fmu.RUnlock();

            var (af, err) = f.apply(so);
            if (err != null)
            {
                return (-1L, null, err);
            }
            ns, sa, so.Err = syscall.Accept(s);
            err = af.apply(so);

            if (err != null)
            {
                if (so.Err == null)
                {
                    syscall.Close(ns);
                }
                return (-1L, null, err);
            }
            sw.smu.Lock();
            defer(sw.smu.Unlock());
            if (so.Err != null)
            {
                sw.stats.getLocked(so.Cookie).AcceptFailed;

                return (-1L, null, so.Err);
            }
            var nso = sw.addLocked(ns, so.Cookie.Family(), so.Cookie.Type(), so.Cookie.Protocol());
            sw.stats.getLocked(nso.Cookie).Accepted;

            return (ns, sa, null);
        });

        // GetsockoptInt wraps syscall.GetsockoptInt.
        private static (long, error) GetsockoptInt(this ref Switch sw, long s, long level, long opt)
        {
            var so = sw.sockso(s);
            if (so == null)
            {
                return syscall.GetsockoptInt(s, level, opt);
            }
            sw.fmu.RLock();
            var f = sw.fltab[FilterGetsockoptInt];
            sw.fmu.RUnlock();

            var (af, err) = f.apply(so);
            if (err != null)
            {
                return (-1L, err);
            }
            soerr, so.Err = syscall.GetsockoptInt(s, level, opt);
            so.SocketErr = syscall.Errno(soerr);
            err = af.apply(so);

            if (err != null)
            {
                return (-1L, err);
            }
            if (so.Err != null)
            {
                return (-1L, so.Err);
            }
            if (opt == syscall.SO_ERROR && (so.SocketErr == syscall.Errno(0L) || so.SocketErr == syscall.EISCONN))
            {
                sw.smu.Lock();
                sw.stats.getLocked(so.Cookie).Connected;

                sw.smu.Unlock();
            }
            return (soerr, null);
        }
    }
}}}
