// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin dragonfly freebsd linux nacl netbsd openbsd solaris windows

// package net -- go2cs converted at 2020 August 29 08:26:48 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\iprawsock_posix.go
using context = go.context_package;
using syscall = go.syscall_package;
using static go.builtin;

namespace go
{
    public static partial class net_package
    {
        private static Addr sockaddrToIP(syscall.Sockaddr sa)
        {
            switch (sa.type())
            {
                case ref syscall.SockaddrInet4 sa:
                    return ref new IPAddr(IP:sa.Addr[0:]);
                    break;
                case ref syscall.SockaddrInet6 sa:
                    return ref new IPAddr(IP:sa.Addr[0:],Zone:zoneCache.name(int(sa.ZoneId)));
                    break;
            }
            return null;
        }

        private static long family(this ref IPAddr a)
        {
            if (a == null || len(a.IP) <= IPv4len)
            {
                return syscall.AF_INET;
            }
            if (a.IP.To4() != null)
            {
                return syscall.AF_INET;
            }
            return syscall.AF_INET6;
        }

        private static (syscall.Sockaddr, error) sockaddr(this ref IPAddr a, long family)
        {
            if (a == null)
            {
                return (null, null);
            }
            return ipToSockaddr(family, a.IP, 0L, a.Zone);
        }

        private static sockaddr toLocal(this ref IPAddr a, @string net)
        {
            return ref new IPAddr(loopbackIP(net),a.Zone);
        }

        private static (long, ref IPAddr, error) readFrom(this ref IPConn c, slice<byte> b)
        { 
            // TODO(cw,rsc): consider using readv if we know the family
            // type to avoid the header trim/copy
            ref IPAddr addr = default;
            var (n, sa, err) = c.fd.readFrom(b);
            switch (sa.type())
            {
                case ref syscall.SockaddrInet4 sa:
                    addr = ref new IPAddr(IP:sa.Addr[0:]);
                    n = stripIPv4Header(n, b);
                    break;
                case ref syscall.SockaddrInet6 sa:
                    addr = ref new IPAddr(IP:sa.Addr[0:],Zone:zoneCache.name(int(sa.ZoneId)));
                    break;
            }
            return (n, addr, err);
        }

        private static long stripIPv4Header(long n, slice<byte> b)
        {
            if (len(b) < 20L)
            {
                return n;
            }
            var l = int(b[0L] & 0x0fUL) << (int)(2L);
            if (20L > l || l > len(b))
            {
                return n;
            }
            if (b[0L] >> (int)(4L) != 4L)
            {
                return n;
            }
            copy(b, b[l..]);
            return n - l;
        }

        private static (long, long, long, ref IPAddr, error) readMsg(this ref IPConn c, slice<byte> b, slice<byte> oob)
        {
            syscall.Sockaddr sa = default;
            n, oobn, flags, sa, err = c.fd.readMsg(b, oob);
            switch (sa.type())
            {
                case ref syscall.SockaddrInet4 sa:
                    addr = ref new IPAddr(IP:sa.Addr[0:]);
                    break;
                case ref syscall.SockaddrInet6 sa:
                    addr = ref new IPAddr(IP:sa.Addr[0:],Zone:zoneCache.name(int(sa.ZoneId)));
                    break;
            }
            return;
        }

        private static (long, error) writeTo(this ref IPConn c, slice<byte> b, ref IPAddr addr)
        {
            if (c.fd.isConnected)
            {
                return (0L, ErrWriteToConnected);
            }
            if (addr == null)
            {
                return (0L, errMissingAddress);
            }
            var (sa, err) = addr.sockaddr(c.fd.family);
            if (err != null)
            {
                return (0L, err);
            }
            return c.fd.writeTo(b, sa);
        }

        private static (long, long, error) writeMsg(this ref IPConn c, slice<byte> b, slice<byte> oob, ref IPAddr addr)
        {
            if (c.fd.isConnected)
            {
                return (0L, 0L, ErrWriteToConnected);
            }
            if (addr == null)
            {
                return (0L, 0L, errMissingAddress);
            }
            var (sa, err) = addr.sockaddr(c.fd.family);
            if (err != null)
            {
                return (0L, 0L, err);
            }
            return c.fd.writeMsg(b, oob, sa);
        }

        private static (ref IPConn, error) dialIP(context.Context ctx, @string netProto, ref IPAddr laddr, ref IPAddr raddr)
        {
            var (network, proto, err) = parseNetwork(ctx, netProto, true);
            if (err != null)
            {
                return (null, err);
            }
            switch (network)
            {
                case "ip": 

                case "ip4": 

                case "ip6": 
                    break;
                default: 
                    return (null, UnknownNetworkError(netProto));
                    break;
            }
            if (raddr == null)
            {
                return (null, errMissingAddress);
            }
            var (fd, err) = internetSocket(ctx, network, laddr, raddr, syscall.SOCK_RAW, proto, "dial");
            if (err != null)
            {
                return (null, err);
            }
            return (newIPConn(fd), null);
        }

        private static (ref IPConn, error) listenIP(context.Context ctx, @string netProto, ref IPAddr laddr)
        {
            var (network, proto, err) = parseNetwork(ctx, netProto, true);
            if (err != null)
            {
                return (null, err);
            }
            switch (network)
            {
                case "ip": 

                case "ip4": 

                case "ip6": 
                    break;
                default: 
                    return (null, UnknownNetworkError(netProto));
                    break;
            }
            var (fd, err) = internetSocket(ctx, network, laddr, null, syscall.SOCK_RAW, proto, "listen");
            if (err != null)
            {
                return (null, err);
            }
            return (newIPConn(fd), null);
        }
    }
}
