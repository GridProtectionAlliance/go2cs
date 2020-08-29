// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build dragonfly freebsd linux

// package socktest -- go2cs converted at 2020 August 29 08:36:20 UTC
// import "net/internal/socktest" ==> using socktest = go.net.@internal.socktest_package
// Original source: C:\Go\src\net\internal\socktest\sys_cloexec.go
using syscall = go.syscall_package;
using static go.builtin;

namespace go {
namespace net {
namespace @internal
{
    public static partial class socktest_package
    {
        // Accept4 wraps syscall.Accept4.
        private static (long, syscall.Sockaddr, error) Accept4(this ref Switch _sw, long s, long flags) => func(_sw, (ref Switch sw, Defer defer, Panic _, Recover __) =>
        {
            var so = sw.sockso(s);
            if (so == null)
            {
                return syscall.Accept4(s, flags);
            }
            sw.fmu.RLock();
            var f = sw.fltab[FilterAccept];
            sw.fmu.RUnlock();

            var (af, err) = f.apply(so);
            if (err != null)
            {
                return (-1L, null, err);
            }
            ns, sa, so.Err = syscall.Accept4(s, flags);
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
    }
}}}
