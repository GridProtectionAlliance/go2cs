// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package net -- go2cs converted at 2022 March 06 22:16:54 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Program Files\Go\src\net\unixsock_plan9.go
using context = go.context_package;
using os = go.os_package;
using syscall = go.syscall_package;

namespace go;

public static partial class net_package {

private static (nint, ptr<UnixAddr>, error) readFrom(this ptr<UnixConn> _addr_c, slice<byte> b) {
    nint _p0 = default;
    ptr<UnixAddr> _p0 = default!;
    error _p0 = default!;
    ref UnixConn c = ref _addr_c.val;

    return (0, _addr_null!, error.As(syscall.EPLAN9)!);
}

private static (nint, nint, nint, ptr<UnixAddr>, error) readMsg(this ptr<UnixConn> _addr_c, slice<byte> b, slice<byte> oob) {
    nint n = default;
    nint oobn = default;
    nint flags = default;
    ptr<UnixAddr> addr = default!;
    error err = default!;
    ref UnixConn c = ref _addr_c.val;

    return (0, 0, 0, _addr_null!, error.As(syscall.EPLAN9)!);
}

private static (nint, error) writeTo(this ptr<UnixConn> _addr_c, slice<byte> b, ptr<UnixAddr> _addr_addr) {
    nint _p0 = default;
    error _p0 = default!;
    ref UnixConn c = ref _addr_c.val;
    ref UnixAddr addr = ref _addr_addr.val;

    return (0, error.As(syscall.EPLAN9)!);
}

private static (nint, nint, error) writeMsg(this ptr<UnixConn> _addr_c, slice<byte> b, slice<byte> oob, ptr<UnixAddr> _addr_addr) {
    nint n = default;
    nint oobn = default;
    error err = default!;
    ref UnixConn c = ref _addr_c.val;
    ref UnixAddr addr = ref _addr_addr.val;

    return (0, 0, error.As(syscall.EPLAN9)!);
}

private static (ptr<UnixConn>, error) dialUnix(this ptr<sysDialer> _addr_sd, context.Context ctx, ptr<UnixAddr> _addr_laddr, ptr<UnixAddr> _addr_raddr) {
    ptr<UnixConn> _p0 = default!;
    error _p0 = default!;
    ref sysDialer sd = ref _addr_sd.val;
    ref UnixAddr laddr = ref _addr_laddr.val;
    ref UnixAddr raddr = ref _addr_raddr.val;

    return (_addr_null!, error.As(syscall.EPLAN9)!);
}

private static (ptr<UnixConn>, error) accept(this ptr<UnixListener> _addr_ln) {
    ptr<UnixConn> _p0 = default!;
    error _p0 = default!;
    ref UnixListener ln = ref _addr_ln.val;

    return (_addr_null!, error.As(syscall.EPLAN9)!);
}

private static error close(this ptr<UnixListener> _addr_ln) {
    ref UnixListener ln = ref _addr_ln.val;

    return error.As(syscall.EPLAN9)!;
}

private static (ptr<os.File>, error) file(this ptr<UnixListener> _addr_ln) {
    ptr<os.File> _p0 = default!;
    error _p0 = default!;
    ref UnixListener ln = ref _addr_ln.val;

    return (_addr_null!, error.As(syscall.EPLAN9)!);
}

private static (ptr<UnixListener>, error) listenUnix(this ptr<sysListener> _addr_sl, context.Context ctx, ptr<UnixAddr> _addr_laddr) {
    ptr<UnixListener> _p0 = default!;
    error _p0 = default!;
    ref sysListener sl = ref _addr_sl.val;
    ref UnixAddr laddr = ref _addr_laddr.val;

    return (_addr_null!, error.As(syscall.EPLAN9)!);
}

private static (ptr<UnixConn>, error) listenUnixgram(this ptr<sysListener> _addr_sl, context.Context ctx, ptr<UnixAddr> _addr_laddr) {
    ptr<UnixConn> _p0 = default!;
    error _p0 = default!;
    ref sysListener sl = ref _addr_sl.val;
    ref UnixAddr laddr = ref _addr_laddr.val;

    return (_addr_null!, error.As(syscall.EPLAN9)!);
}

} // end net_package
