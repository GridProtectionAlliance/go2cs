// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package net -- go2cs converted at 2020 August 29 08:26:46 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\iprawsock_plan9.go
using context = go.context_package;
using syscall = go.syscall_package;
using static go.builtin;

namespace go
{
    public static partial class net_package
    {
        private static (long, ref IPAddr, error) readFrom(this ref IPConn c, slice<byte> b)
        {
            return (0L, null, syscall.EPLAN9);
        }

        private static (long, long, long, ref IPAddr, error) readMsg(this ref IPConn c, slice<byte> b, slice<byte> oob)
        {
            return (0L, 0L, 0L, null, syscall.EPLAN9);
        }

        private static (long, error) writeTo(this ref IPConn c, slice<byte> b, ref IPAddr addr)
        {
            return (0L, syscall.EPLAN9);
        }

        private static (long, long, error) writeMsg(this ref IPConn c, slice<byte> b, slice<byte> oob, ref IPAddr addr)
        {
            return (0L, 0L, syscall.EPLAN9);
        }

        private static (ref IPConn, error) dialIP(context.Context ctx, @string netProto, ref IPAddr laddr, ref IPAddr raddr)
        {
            return (null, syscall.EPLAN9);
        }

        private static (ref IPConn, error) listenIP(context.Context ctx, @string netProto, ref IPAddr laddr)
        {
            return (null, syscall.EPLAN9);
        }
    }
}
