// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package net -- go2cs converted at 2022 March 06 22:16:11 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Program Files\Go\src\net\iprawsock_plan9.go
using context = go.context_package;
using syscall = go.syscall_package;

namespace go;

public static partial class net_package {

private static (nint, ptr<IPAddr>, error) readFrom(this ptr<IPConn> _addr_c, slice<byte> b) {
    nint _p0 = default;
    ptr<IPAddr> _p0 = default!;
    error _p0 = default!;
    ref IPConn c = ref _addr_c.val;

    return (0, _addr_null!, error.As(syscall.EPLAN9)!);
}

private static (nint, nint, nint, ptr<IPAddr>, error) readMsg(this ptr<IPConn> _addr_c, slice<byte> b, slice<byte> oob) {
    nint n = default;
    nint oobn = default;
    nint flags = default;
    ptr<IPAddr> addr = default!;
    error err = default!;
    ref IPConn c = ref _addr_c.val;

    return (0, 0, 0, _addr_null!, error.As(syscall.EPLAN9)!);
}

private static (nint, error) writeTo(this ptr<IPConn> _addr_c, slice<byte> b, ptr<IPAddr> _addr_addr) {
    nint _p0 = default;
    error _p0 = default!;
    ref IPConn c = ref _addr_c.val;
    ref IPAddr addr = ref _addr_addr.val;

    return (0, error.As(syscall.EPLAN9)!);
}

private static (nint, nint, error) writeMsg(this ptr<IPConn> _addr_c, slice<byte> b, slice<byte> oob, ptr<IPAddr> _addr_addr) {
    nint n = default;
    nint oobn = default;
    error err = default!;
    ref IPConn c = ref _addr_c.val;
    ref IPAddr addr = ref _addr_addr.val;

    return (0, 0, error.As(syscall.EPLAN9)!);
}

private static (ptr<IPConn>, error) dialIP(this ptr<sysDialer> _addr_sd, context.Context ctx, ptr<IPAddr> _addr_laddr, ptr<IPAddr> _addr_raddr) {
    ptr<IPConn> _p0 = default!;
    error _p0 = default!;
    ref sysDialer sd = ref _addr_sd.val;
    ref IPAddr laddr = ref _addr_laddr.val;
    ref IPAddr raddr = ref _addr_raddr.val;

    return (_addr_null!, error.As(syscall.EPLAN9)!);
}

private static (ptr<IPConn>, error) listenIP(this ptr<sysListener> _addr_sl, context.Context ctx, ptr<IPAddr> _addr_laddr) {
    ptr<IPConn> _p0 = default!;
    error _p0 = default!;
    ref sysListener sl = ref _addr_sl.val;
    ref IPAddr laddr = ref _addr_laddr.val;

    return (_addr_null!, error.As(syscall.EPLAN9)!);
}

} // end net_package
