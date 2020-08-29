// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package net -- go2cs converted at 2020 August 29 08:26:23 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\hook.go
using context = go.context_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class net_package
    {
 
        // if non-nil, overrides dialTCP.
        private static Func<context.Context, @string, ref TCPAddr, ref TCPAddr, (ref TCPConn, error)> testHookDialTCP = default;        private static @string testHookHostsPath = "/etc/hosts";        private static Func<context.Context, Func<context.Context, @string, (slice<IPAddr>, error)>, @string, (slice<IPAddr>, error)> testHookLookupIP = (ctx, fn, host) =>
        {
            return fn(ctx, host);
        };        private static Action testHookSetKeepAlive = () =>
        {
        };
    }
}
