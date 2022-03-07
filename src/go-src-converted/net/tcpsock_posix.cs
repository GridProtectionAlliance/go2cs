// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build aix || darwin || dragonfly || freebsd || (js && wasm) || linux || netbsd || openbsd || solaris || windows
// +build aix darwin dragonfly freebsd js,wasm linux netbsd openbsd solaris windows

// package net -- go2cs converted at 2022 March 06 22:16:50 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Program Files\Go\src\net\tcpsock_posix.go
using context = go.context_package;
using io = go.io_package;
using os = go.os_package;
using syscall = go.syscall_package;

namespace go;

public static partial class net_package {

private static Addr sockaddrToTCP(syscall.Sockaddr sa) {
    switch (sa.type()) {
        case ptr<syscall.SockaddrInet4> sa:
            return addr(new TCPAddr(IP:sa.Addr[0:],Port:sa.Port));
            break;
        case ptr<syscall.SockaddrInet6> sa:
            return addr(new TCPAddr(IP:sa.Addr[0:],Port:sa.Port,Zone:zoneCache.name(int(sa.ZoneId))));
            break;
    }
    return null;

}

private static nint family(this ptr<TCPAddr> _addr_a) {
    ref TCPAddr a = ref _addr_a.val;

    if (a == null || len(a.IP) <= IPv4len) {
        return syscall.AF_INET;
    }
    if (a.IP.To4() != null) {
        return syscall.AF_INET;
    }
    return syscall.AF_INET6;

}

private static (syscall.Sockaddr, error) sockaddr(this ptr<TCPAddr> _addr_a, nint family) {
    syscall.Sockaddr _p0 = default;
    error _p0 = default!;
    ref TCPAddr a = ref _addr_a.val;

    if (a == null) {
        return (null, error.As(null!)!);
    }
    return ipToSockaddr(family, a.IP, a.Port, a.Zone);

}

private static sockaddr toLocal(this ptr<TCPAddr> _addr_a, @string net) {
    ref TCPAddr a = ref _addr_a.val;

    return addr(new TCPAddr(loopbackIP(net),a.Port,a.Zone));
}

private static (long, error) readFrom(this ptr<TCPConn> _addr_c, io.Reader r) {
    long _p0 = default;
    error _p0 = default!;
    ref TCPConn c = ref _addr_c.val;

    {
        var n__prev1 = n;

        var (n, err, handled) = splice(c.fd, r);

        if (handled) {
            return (n, error.As(err)!);
        }
        n = n__prev1;

    }

    {
        var n__prev1 = n;

        (n, err, handled) = sendFile(c.fd, r);

        if (handled) {
            return (n, error.As(err)!);
        }
        n = n__prev1;

    }

    return genericReadFrom(c, r);

}

private static (ptr<TCPConn>, error) dialTCP(this ptr<sysDialer> _addr_sd, context.Context ctx, ptr<TCPAddr> _addr_laddr, ptr<TCPAddr> _addr_raddr) {
    ptr<TCPConn> _p0 = default!;
    error _p0 = default!;
    ref sysDialer sd = ref _addr_sd.val;
    ref TCPAddr laddr = ref _addr_laddr.val;
    ref TCPAddr raddr = ref _addr_raddr.val;

    if (testHookDialTCP != null) {
        return _addr_testHookDialTCP(ctx, sd.network, laddr, raddr)!;
    }
    return _addr_sd.doDialTCP(ctx, laddr, raddr)!;

}

private static (ptr<TCPConn>, error) doDialTCP(this ptr<sysDialer> _addr_sd, context.Context ctx, ptr<TCPAddr> _addr_laddr, ptr<TCPAddr> _addr_raddr) {
    ptr<TCPConn> _p0 = default!;
    error _p0 = default!;
    ref sysDialer sd = ref _addr_sd.val;
    ref TCPAddr laddr = ref _addr_laddr.val;
    ref TCPAddr raddr = ref _addr_raddr.val;

    var (fd, err) = internetSocket(ctx, sd.network, laddr, raddr, syscall.SOCK_STREAM, 0, "dial", sd.Dialer.Control); 

    // TCP has a rarely used mechanism called a 'simultaneous connection' in
    // which Dial("tcp", addr1, addr2) run on the machine at addr1 can
    // connect to a simultaneous Dial("tcp", addr2, addr1) run on the machine
    // at addr2, without either machine executing Listen. If laddr == nil,
    // it means we want the kernel to pick an appropriate originating local
    // address. Some Linux kernels cycle blindly through a fixed range of
    // local ports, regardless of destination port. If a kernel happens to
    // pick local port 50001 as the source for a Dial("tcp", "", "localhost:50001"),
    // then the Dial will succeed, having simultaneously connected to itself.
    // This can only happen when we are letting the kernel pick a port (laddr == nil)
    // and when there is no listener for the destination address.
    // It's hard to argue this is anything other than a kernel bug. If we
    // see this happen, rather than expose the buggy effect to users, we
    // close the fd and try again. If it happens twice more, we relent and
    // use the result. See also:
    //    https://golang.org/issue/2690
    //    https://stackoverflow.com/questions/4949858/
    //
    // The opposite can also happen: if we ask the kernel to pick an appropriate
    // originating local address, sometimes it picks one that is already in use.
    // So if the error is EADDRNOTAVAIL, we have to try again too, just for
    // a different reason.
    //
    // The kernel socket code is no doubt enjoying watching us squirm.
    for (nint i = 0; i < 2 && (laddr == null || laddr.Port == 0) && (selfConnect(_addr_fd, err) || spuriousENOTAVAIL(err)); i++) {
        if (err == null) {
            fd.Close();
        }
        fd, err = internetSocket(ctx, sd.network, laddr, raddr, syscall.SOCK_STREAM, 0, "dial", sd.Dialer.Control);

    }

    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    return (_addr_newTCPConn(fd)!, error.As(null!)!);

}

private static bool selfConnect(ptr<netFD> _addr_fd, error err) {
    ref netFD fd = ref _addr_fd.val;
 
    // If the connect failed, we clearly didn't connect to ourselves.
    if (err != null) {
        return false;
    }
    if (fd.laddr == null || fd.raddr == null) {
        return true;
    }
    ptr<TCPAddr> l = fd.laddr._<ptr<TCPAddr>>();
    ptr<TCPAddr> r = fd.raddr._<ptr<TCPAddr>>();
    return l.Port == r.Port && l.IP.Equal(r.IP);

}

private static bool spuriousENOTAVAIL(error err) {
    {
        ptr<OpError> (op, ok) = err._<ptr<OpError>>();

        if (ok) {
            err = op.Err;
        }
    }

    {
        ptr<os.SyscallError> (sys, ok) = err._<ptr<os.SyscallError>>();

        if (ok) {
            err = sys.Err;
        }
    }

    return err == syscall.EADDRNOTAVAIL;

}

private static bool ok(this ptr<TCPListener> _addr_ln) {
    ref TCPListener ln = ref _addr_ln.val;

    return ln != null && ln.fd != null;
}

private static (ptr<TCPConn>, error) accept(this ptr<TCPListener> _addr_ln) {
    ptr<TCPConn> _p0 = default!;
    error _p0 = default!;
    ref TCPListener ln = ref _addr_ln.val;

    var (fd, err) = ln.fd.accept();
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    var tc = newTCPConn(fd);
    if (ln.lc.KeepAlive >= 0) {
        setKeepAlive(fd, true);
        var ka = ln.lc.KeepAlive;
        if (ln.lc.KeepAlive == 0) {
            ka = defaultTCPKeepAlive;
        }
        setKeepAlivePeriod(fd, ka);

    }
    return (_addr_tc!, error.As(null!)!);

}

private static error close(this ptr<TCPListener> _addr_ln) {
    ref TCPListener ln = ref _addr_ln.val;

    return error.As(ln.fd.Close())!;
}

private static (ptr<os.File>, error) file(this ptr<TCPListener> _addr_ln) {
    ptr<os.File> _p0 = default!;
    error _p0 = default!;
    ref TCPListener ln = ref _addr_ln.val;

    var (f, err) = ln.fd.dup();
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    return (_addr_f!, error.As(null!)!);

}

private static (ptr<TCPListener>, error) listenTCP(this ptr<sysListener> _addr_sl, context.Context ctx, ptr<TCPAddr> _addr_laddr) {
    ptr<TCPListener> _p0 = default!;
    error _p0 = default!;
    ref sysListener sl = ref _addr_sl.val;
    ref TCPAddr laddr = ref _addr_laddr.val;

    var (fd, err) = internetSocket(ctx, sl.network, laddr, null, syscall.SOCK_STREAM, 0, "listen", sl.ListenConfig.Control);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    return (addr(new TCPListener(fd:fd,lc:sl.ListenConfig)), error.As(null!)!);

}

} // end net_package
