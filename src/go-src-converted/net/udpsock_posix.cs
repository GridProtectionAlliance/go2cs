// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build aix || darwin || dragonfly || freebsd || (js && wasm) || linux || netbsd || openbsd || solaris || windows
// +build aix darwin dragonfly freebsd js,wasm linux netbsd openbsd solaris windows

// package net -- go2cs converted at 2022 March 06 22:16:52 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Program Files\Go\src\net\udpsock_posix.go
using context = go.context_package;
using syscall = go.syscall_package;

namespace go;

public static partial class net_package {

private static Addr sockaddrToUDP(syscall.Sockaddr sa) {
    switch (sa.type()) {
        case ptr<syscall.SockaddrInet4> sa:
            return addr(new UDPAddr(IP:sa.Addr[0:],Port:sa.Port));
            break;
        case ptr<syscall.SockaddrInet6> sa:
            return addr(new UDPAddr(IP:sa.Addr[0:],Port:sa.Port,Zone:zoneCache.name(int(sa.ZoneId))));
            break;
    }
    return null;

}

private static nint family(this ptr<UDPAddr> _addr_a) {
    ref UDPAddr a = ref _addr_a.val;

    if (a == null || len(a.IP) <= IPv4len) {
        return syscall.AF_INET;
    }
    if (a.IP.To4() != null) {
        return syscall.AF_INET;
    }
    return syscall.AF_INET6;

}

private static (syscall.Sockaddr, error) sockaddr(this ptr<UDPAddr> _addr_a, nint family) {
    syscall.Sockaddr _p0 = default;
    error _p0 = default!;
    ref UDPAddr a = ref _addr_a.val;

    if (a == null) {
        return (null, error.As(null!)!);
    }
    return ipToSockaddr(family, a.IP, a.Port, a.Zone);

}

private static sockaddr toLocal(this ptr<UDPAddr> _addr_a, @string net) {
    ref UDPAddr a = ref _addr_a.val;

    return addr(new UDPAddr(loopbackIP(net),a.Port,a.Zone));
}

private static (nint, ptr<UDPAddr>, error) readFrom(this ptr<UDPConn> _addr_c, slice<byte> b, ptr<UDPAddr> _addr_addr) {
    nint _p0 = default;
    ptr<UDPAddr> _p0 = default!;
    error _p0 = default!;
    ref UDPConn c = ref _addr_c.val;
    ref UDPAddr addr = ref _addr_addr.val;

    var (n, sa, err) = c.fd.readFrom(b);
    switch (sa.type()) {
        case ptr<syscall.SockaddrInet4> sa:
            addr = new UDPAddr(IP:sa.Addr[0:],Port:sa.Port);
            break;
        case ptr<syscall.SockaddrInet6> sa:
            addr = new UDPAddr(IP:sa.Addr[0:],Port:sa.Port,Zone:zoneCache.name(int(sa.ZoneId)));
            break;
        default:
        {
            var sa = sa.type();
            addr = null;
            break;
        }
    }
    return (n, _addr_addr!, error.As(err)!);

}

private static (nint, nint, nint, ptr<UDPAddr>, error) readMsg(this ptr<UDPConn> _addr_c, slice<byte> b, slice<byte> oob) {
    nint n = default;
    nint oobn = default;
    nint flags = default;
    ptr<UDPAddr> addr = default!;
    error err = default!;
    ref UDPConn c = ref _addr_c.val;

    syscall.Sockaddr sa = default;
    n, oobn, flags, sa, err = c.fd.readMsg(b, oob, 0);
    switch (sa.type()) {
        case ptr<syscall.SockaddrInet4> sa:
            addr = addr(new UDPAddr(IP:sa.Addr[0:],Port:sa.Port));
            break;
        case ptr<syscall.SockaddrInet6> sa:
            addr = addr(new UDPAddr(IP:sa.Addr[0:],Port:sa.Port,Zone:zoneCache.name(int(sa.ZoneId))));
            break;
    }
    return ;

}

private static (nint, error) writeTo(this ptr<UDPConn> _addr_c, slice<byte> b, ptr<UDPAddr> _addr_addr) {
    nint _p0 = default;
    error _p0 = default!;
    ref UDPConn c = ref _addr_c.val;
    ref UDPAddr addr = ref _addr_addr.val;

    if (c.fd.isConnected) {
        return (0, error.As(ErrWriteToConnected)!);
    }
    if (addr == null) {
        return (0, error.As(errMissingAddress)!);
    }
    var (sa, err) = addr.sockaddr(c.fd.family);
    if (err != null) {
        return (0, error.As(err)!);
    }
    return c.fd.writeTo(b, sa);

}

private static (nint, nint, error) writeMsg(this ptr<UDPConn> _addr_c, slice<byte> b, slice<byte> oob, ptr<UDPAddr> _addr_addr) {
    nint n = default;
    nint oobn = default;
    error err = default!;
    ref UDPConn c = ref _addr_c.val;
    ref UDPAddr addr = ref _addr_addr.val;

    if (c.fd.isConnected && addr != null) {
        return (0, 0, error.As(ErrWriteToConnected)!);
    }
    if (!c.fd.isConnected && addr == null) {
        return (0, 0, error.As(errMissingAddress)!);
    }
    var (sa, err) = addr.sockaddr(c.fd.family);
    if (err != null) {
        return (0, 0, error.As(err)!);
    }
    return c.fd.writeMsg(b, oob, sa);

}

private static (ptr<UDPConn>, error) dialUDP(this ptr<sysDialer> _addr_sd, context.Context ctx, ptr<UDPAddr> _addr_laddr, ptr<UDPAddr> _addr_raddr) {
    ptr<UDPConn> _p0 = default!;
    error _p0 = default!;
    ref sysDialer sd = ref _addr_sd.val;
    ref UDPAddr laddr = ref _addr_laddr.val;
    ref UDPAddr raddr = ref _addr_raddr.val;

    var (fd, err) = internetSocket(ctx, sd.network, laddr, raddr, syscall.SOCK_DGRAM, 0, "dial", sd.Dialer.Control);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    return (_addr_newUDPConn(fd)!, error.As(null!)!);

}

private static (ptr<UDPConn>, error) listenUDP(this ptr<sysListener> _addr_sl, context.Context ctx, ptr<UDPAddr> _addr_laddr) {
    ptr<UDPConn> _p0 = default!;
    error _p0 = default!;
    ref sysListener sl = ref _addr_sl.val;
    ref UDPAddr laddr = ref _addr_laddr.val;

    var (fd, err) = internetSocket(ctx, sl.network, laddr, null, syscall.SOCK_DGRAM, 0, "listen", sl.ListenConfig.Control);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    return (_addr_newUDPConn(fd)!, error.As(null!)!);

}

private static (ptr<UDPConn>, error) listenMulticastUDP(this ptr<sysListener> _addr_sl, context.Context ctx, ptr<Interface> _addr_ifi, ptr<UDPAddr> _addr_gaddr) {
    ptr<UDPConn> _p0 = default!;
    error _p0 = default!;
    ref sysListener sl = ref _addr_sl.val;
    ref Interface ifi = ref _addr_ifi.val;
    ref UDPAddr gaddr = ref _addr_gaddr.val;

    var (fd, err) = internetSocket(ctx, sl.network, gaddr, null, syscall.SOCK_DGRAM, 0, "listen", sl.ListenConfig.Control);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    var c = newUDPConn(fd);
    {
        var ip4 = gaddr.IP.To4();

        if (ip4 != null) {
            {
                var err__prev2 = err;

                var err = listenIPv4MulticastUDP(_addr_c, _addr_ifi, ip4);

                if (err != null) {
                    c.Close();
                    return (_addr_null!, error.As(err)!);
                }

                err = err__prev2;

            }

        }
        else
 {
            {
                var err__prev2 = err;

                err = listenIPv6MulticastUDP(_addr_c, _addr_ifi, gaddr.IP);

                if (err != null) {
                    c.Close();
                    return (_addr_null!, error.As(err)!);
                }

                err = err__prev2;

            }

        }
    }

    return (_addr_c!, error.As(null!)!);

}

private static error listenIPv4MulticastUDP(ptr<UDPConn> _addr_c, ptr<Interface> _addr_ifi, IP ip) {
    ref UDPConn c = ref _addr_c.val;
    ref Interface ifi = ref _addr_ifi.val;

    if (ifi != null) {
        {
            var err__prev2 = err;

            var err = setIPv4MulticastInterface(c.fd, ifi);

            if (err != null) {
                return error.As(err)!;
            }

            err = err__prev2;

        }

    }
    {
        var err__prev1 = err;

        err = setIPv4MulticastLoopback(c.fd, false);

        if (err != null) {
            return error.As(err)!;
        }
        err = err__prev1;

    }

    {
        var err__prev1 = err;

        err = joinIPv4Group(c.fd, ifi, ip);

        if (err != null) {
            return error.As(err)!;
        }
        err = err__prev1;

    }

    return error.As(null!)!;

}

private static error listenIPv6MulticastUDP(ptr<UDPConn> _addr_c, ptr<Interface> _addr_ifi, IP ip) {
    ref UDPConn c = ref _addr_c.val;
    ref Interface ifi = ref _addr_ifi.val;

    if (ifi != null) {
        {
            var err__prev2 = err;

            var err = setIPv6MulticastInterface(c.fd, ifi);

            if (err != null) {
                return error.As(err)!;
            }

            err = err__prev2;

        }

    }
    {
        var err__prev1 = err;

        err = setIPv6MulticastLoopback(c.fd, false);

        if (err != null) {
            return error.As(err)!;
        }
        err = err__prev1;

    }

    {
        var err__prev1 = err;

        err = joinIPv6Group(c.fd, ifi, ip);

        if (err != null) {
            return error.As(err)!;
        }
        err = err__prev1;

    }

    return error.As(null!)!;

}

} // end net_package
