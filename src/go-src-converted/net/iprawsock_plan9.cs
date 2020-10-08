// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package net -- go2cs converted at 2020 October 08 03:33:43 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\iprawsock_plan9.go
using context = go.context_package;
using syscall = go.syscall_package;
using static go.builtin;

namespace go
{
    public static partial class net_package
    {
        private static (long, ptr<IPAddr>, error) readFrom(this ptr<IPConn> _addr_c, slice<byte> b)
        {
            long _p0 = default;
            ptr<IPAddr> _p0 = default!;
            error _p0 = default!;
            ref IPConn c = ref _addr_c.val;

            return (0L, _addr_null!, error.As(syscall.EPLAN9)!);
        }

        private static (long, long, long, ptr<IPAddr>, error) readMsg(this ptr<IPConn> _addr_c, slice<byte> b, slice<byte> oob)
        {
            long n = default;
            long oobn = default;
            long flags = default;
            ptr<IPAddr> addr = default!;
            error err = default!;
            ref IPConn c = ref _addr_c.val;

            return (0L, 0L, 0L, _addr_null!, error.As(syscall.EPLAN9)!);
        }

        private static (long, error) writeTo(this ptr<IPConn> _addr_c, slice<byte> b, ptr<IPAddr> _addr_addr)
        {
            long _p0 = default;
            error _p0 = default!;
            ref IPConn c = ref _addr_c.val;
            ref IPAddr addr = ref _addr_addr.val;

            return (0L, error.As(syscall.EPLAN9)!);
        }

        private static (long, long, error) writeMsg(this ptr<IPConn> _addr_c, slice<byte> b, slice<byte> oob, ptr<IPAddr> _addr_addr)
        {
            long n = default;
            long oobn = default;
            error err = default!;
            ref IPConn c = ref _addr_c.val;
            ref IPAddr addr = ref _addr_addr.val;

            return (0L, 0L, error.As(syscall.EPLAN9)!);
        }

        private static (ptr<IPConn>, error) dialIP(this ptr<sysDialer> _addr_sd, context.Context ctx, ptr<IPAddr> _addr_laddr, ptr<IPAddr> _addr_raddr)
        {
            ptr<IPConn> _p0 = default!;
            error _p0 = default!;
            ref sysDialer sd = ref _addr_sd.val;
            ref IPAddr laddr = ref _addr_laddr.val;
            ref IPAddr raddr = ref _addr_raddr.val;

            return (_addr_null!, error.As(syscall.EPLAN9)!);
        }

        private static (ptr<IPConn>, error) listenIP(this ptr<sysListener> _addr_sl, context.Context ctx, ptr<IPAddr> _addr_laddr)
        {
            ptr<IPConn> _p0 = default!;
            error _p0 = default!;
            ref sysListener sl = ref _addr_sl.val;
            ref IPAddr laddr = ref _addr_laddr.val;

            return (_addr_null!, error.As(syscall.EPLAN9)!);
        }
    }
}
