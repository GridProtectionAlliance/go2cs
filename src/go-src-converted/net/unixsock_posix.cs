// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build aix || darwin || dragonfly || freebsd || (js && wasm) || linux || netbsd || openbsd || solaris || windows
// +build aix darwin dragonfly freebsd js,wasm linux netbsd openbsd solaris windows

// package net -- go2cs converted at 2022 March 06 22:16:55 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Program Files\Go\src\net\unixsock_posix.go
using context = go.context_package;
using errors = go.errors_package;
using os = go.os_package;
using syscall = go.syscall_package;
using System;


namespace go;

public static partial class net_package {

private static (ptr<netFD>, error) unixSocket(context.Context ctx, @string net, sockaddr laddr, sockaddr raddr, @string mode, Func<@string, @string, syscall.RawConn, error> ctrlFn) {
    ptr<netFD> _p0 = default!;
    error _p0 = default!;

    nint sotype = default;
    switch (net) {
        case "unix": 
            sotype = syscall.SOCK_STREAM;
            break;
        case "unixgram": 
            sotype = syscall.SOCK_DGRAM;
            break;
        case "unixpacket": 
            sotype = syscall.SOCK_SEQPACKET;
            break;
        default: 
            return (_addr_null!, error.As(UnknownNetworkError(net))!);
            break;
    }

    switch (mode) {
        case "dial": 
            if (laddr != null && laddr.isWildcard()) {
                laddr = null;
            }
            if (raddr != null && raddr.isWildcard()) {
                raddr = null;
            }
            if (raddr == null && (sotype != syscall.SOCK_DGRAM || laddr == null)) {
                return (_addr_null!, error.As(errMissingAddress)!);
            }
            break;
        case "listen": 

            break;
        default: 
            return (_addr_null!, error.As(errors.New("unknown mode: " + mode))!);
            break;
    }

    var (fd, err) = socket(ctx, net, syscall.AF_UNIX, sotype, 0, false, laddr, raddr, ctrlFn);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    return (_addr_fd!, error.As(null!)!);

}

private static Addr sockaddrToUnix(syscall.Sockaddr sa) {
    {
        ptr<syscall.SockaddrUnix> (s, ok) = sa._<ptr<syscall.SockaddrUnix>>();

        if (ok) {
            return addr(new UnixAddr(Name:s.Name,Net:"unix"));
        }
    }

    return null;

}

private static Addr sockaddrToUnixgram(syscall.Sockaddr sa) {
    {
        ptr<syscall.SockaddrUnix> (s, ok) = sa._<ptr<syscall.SockaddrUnix>>();

        if (ok) {
            return addr(new UnixAddr(Name:s.Name,Net:"unixgram"));
        }
    }

    return null;

}

private static Addr sockaddrToUnixpacket(syscall.Sockaddr sa) {
    {
        ptr<syscall.SockaddrUnix> (s, ok) = sa._<ptr<syscall.SockaddrUnix>>();

        if (ok) {
            return addr(new UnixAddr(Name:s.Name,Net:"unixpacket"));
        }
    }

    return null;

}

private static @string sotypeToNet(nint sotype) => func((_, panic, _) => {

    if (sotype == syscall.SOCK_STREAM) 
        return "unix";
    else if (sotype == syscall.SOCK_DGRAM) 
        return "unixgram";
    else if (sotype == syscall.SOCK_SEQPACKET) 
        return "unixpacket";
    else 
        panic("sotypeToNet unknown socket type");
    
});

private static nint family(this ptr<UnixAddr> _addr_a) {
    ref UnixAddr a = ref _addr_a.val;

    return syscall.AF_UNIX;
}

private static (syscall.Sockaddr, error) sockaddr(this ptr<UnixAddr> _addr_a, nint family) {
    syscall.Sockaddr _p0 = default;
    error _p0 = default!;
    ref UnixAddr a = ref _addr_a.val;

    if (a == null) {
        return (null, error.As(null!)!);
    }
    return (addr(new syscall.SockaddrUnix(Name:a.Name)), error.As(null!)!);

}

private static sockaddr toLocal(this ptr<UnixAddr> _addr_a, @string net) {
    ref UnixAddr a = ref _addr_a.val;

    return a;
}

private static (nint, ptr<UnixAddr>, error) readFrom(this ptr<UnixConn> _addr_c, slice<byte> b) {
    nint _p0 = default;
    ptr<UnixAddr> _p0 = default!;
    error _p0 = default!;
    ref UnixConn c = ref _addr_c.val;

    ptr<UnixAddr> addr;
    var (n, sa, err) = c.fd.readFrom(b);
    switch (sa.type()) {
        case ptr<syscall.SockaddrUnix> sa:
            if (sa.Name != "") {
                addr = addr(new UnixAddr(Name:sa.Name,Net:sotypeToNet(c.fd.sotype)));
            }
            break;
    }
    return (n, _addr_addr!, error.As(err)!);

}

private static (nint, nint, nint, ptr<UnixAddr>, error) readMsg(this ptr<UnixConn> _addr_c, slice<byte> b, slice<byte> oob) {
    nint n = default;
    nint oobn = default;
    nint flags = default;
    ptr<UnixAddr> addr = default!;
    error err = default!;
    ref UnixConn c = ref _addr_c.val;

    syscall.Sockaddr sa = default;
    n, oobn, flags, sa, err = c.fd.readMsg(b, oob, readMsgFlags);
    if (readMsgFlags == 0 && err == null && oobn > 0) {
        setReadMsgCloseOnExec(oob[..(int)oobn]);
    }
    switch (sa.type()) {
        case ptr<syscall.SockaddrUnix> sa:
            if (sa.Name != "") {
                addr = addr(new UnixAddr(Name:sa.Name,Net:sotypeToNet(c.fd.sotype)));
            }
            break;
    }
    return ;

}

private static (nint, error) writeTo(this ptr<UnixConn> _addr_c, slice<byte> b, ptr<UnixAddr> _addr_addr) {
    nint _p0 = default;
    error _p0 = default!;
    ref UnixConn c = ref _addr_c.val;
    ref UnixAddr addr = ref _addr_addr.val;

    if (c.fd.isConnected) {
        return (0, error.As(ErrWriteToConnected)!);
    }
    if (addr == null) {
        return (0, error.As(errMissingAddress)!);
    }
    if (addr.Net != sotypeToNet(c.fd.sotype)) {
        return (0, error.As(syscall.EAFNOSUPPORT)!);
    }
    ptr<syscall.SockaddrUnix> sa = addr(new syscall.SockaddrUnix(Name:addr.Name));
    return c.fd.writeTo(b, sa);

}

private static (nint, nint, error) writeMsg(this ptr<UnixConn> _addr_c, slice<byte> b, slice<byte> oob, ptr<UnixAddr> _addr_addr) {
    nint n = default;
    nint oobn = default;
    error err = default!;
    ref UnixConn c = ref _addr_c.val;
    ref UnixAddr addr = ref _addr_addr.val;

    if (c.fd.sotype == syscall.SOCK_DGRAM && c.fd.isConnected) {
        return (0, 0, error.As(ErrWriteToConnected)!);
    }
    syscall.Sockaddr sa = default;
    if (addr != null) {
        if (addr.Net != sotypeToNet(c.fd.sotype)) {
            return (0, 0, error.As(syscall.EAFNOSUPPORT)!);
        }
        sa = addr(new syscall.SockaddrUnix(Name:addr.Name));

    }
    return c.fd.writeMsg(b, oob, sa);

}

private static (ptr<UnixConn>, error) dialUnix(this ptr<sysDialer> _addr_sd, context.Context ctx, ptr<UnixAddr> _addr_laddr, ptr<UnixAddr> _addr_raddr) {
    ptr<UnixConn> _p0 = default!;
    error _p0 = default!;
    ref sysDialer sd = ref _addr_sd.val;
    ref UnixAddr laddr = ref _addr_laddr.val;
    ref UnixAddr raddr = ref _addr_raddr.val;

    var (fd, err) = unixSocket(ctx, sd.network, laddr, raddr, "dial", sd.Dialer.Control);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    return (_addr_newUnixConn(fd)!, error.As(null!)!);

}

private static (ptr<UnixConn>, error) accept(this ptr<UnixListener> _addr_ln) {
    ptr<UnixConn> _p0 = default!;
    error _p0 = default!;
    ref UnixListener ln = ref _addr_ln.val;

    var (fd, err) = ln.fd.accept();
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    return (_addr_newUnixConn(fd)!, error.As(null!)!);

}

private static error close(this ptr<UnixListener> _addr_ln) {
    ref UnixListener ln = ref _addr_ln.val;
 
    // The operating system doesn't clean up
    // the file that announcing created, so
    // we have to clean it up ourselves.
    // There's a race here--we can't know for
    // sure whether someone else has come along
    // and replaced our socket name already--
    // but this sequence (remove then close)
    // is at least compatible with the auto-remove
    // sequence in ListenUnix. It's only non-Go
    // programs that can mess us up.
    // Even if there are racy calls to Close, we want to unlink only for the first one.
    ln.unlinkOnce.Do(() => {
        if (ln.path[0] != '@' && ln.unlink) {
            syscall.Unlink(ln.path);
        }
    });
    return error.As(ln.fd.Close())!;

}

private static (ptr<os.File>, error) file(this ptr<UnixListener> _addr_ln) {
    ptr<os.File> _p0 = default!;
    error _p0 = default!;
    ref UnixListener ln = ref _addr_ln.val;

    var (f, err) = ln.fd.dup();
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    return (_addr_f!, error.As(null!)!);

}

// SetUnlinkOnClose sets whether the underlying socket file should be removed
// from the file system when the listener is closed.
//
// The default behavior is to unlink the socket file only when package net created it.
// That is, when the listener and the underlying socket file were created by a call to
// Listen or ListenUnix, then by default closing the listener will remove the socket file.
// but if the listener was created by a call to FileListener to use an already existing
// socket file, then by default closing the listener will not remove the socket file.
private static void SetUnlinkOnClose(this ptr<UnixListener> _addr_l, bool unlink) {
    ref UnixListener l = ref _addr_l.val;

    l.unlink = unlink;
}

private static (ptr<UnixListener>, error) listenUnix(this ptr<sysListener> _addr_sl, context.Context ctx, ptr<UnixAddr> _addr_laddr) {
    ptr<UnixListener> _p0 = default!;
    error _p0 = default!;
    ref sysListener sl = ref _addr_sl.val;
    ref UnixAddr laddr = ref _addr_laddr.val;

    var (fd, err) = unixSocket(ctx, sl.network, laddr, null, "listen", sl.ListenConfig.Control);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    return (addr(new UnixListener(fd:fd,path:fd.laddr.String(),unlink:true)), error.As(null!)!);

}

private static (ptr<UnixConn>, error) listenUnixgram(this ptr<sysListener> _addr_sl, context.Context ctx, ptr<UnixAddr> _addr_laddr) {
    ptr<UnixConn> _p0 = default!;
    error _p0 = default!;
    ref sysListener sl = ref _addr_sl.val;
    ref UnixAddr laddr = ref _addr_laddr.val;

    var (fd, err) = unixSocket(ctx, sl.network, laddr, null, "listen", sl.ListenConfig.Control);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    return (_addr_newUnixConn(fd)!, error.As(null!)!);

}

} // end net_package
