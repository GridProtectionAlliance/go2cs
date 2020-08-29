// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin dragonfly freebsd linux nacl netbsd openbsd solaris windows

// package net -- go2cs converted at 2020 August 29 08:28:06 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\udpsock_posix.go
using context = go.context_package;
using syscall = go.syscall_package;
using static go.builtin;

namespace go
{
    public static partial class net_package
    {
        private static Addr sockaddrToUDP(syscall.Sockaddr sa)
        {
            switch (sa.type())
            {
                case ref syscall.SockaddrInet4 sa:
                    return ref new UDPAddr(IP:sa.Addr[0:],Port:sa.Port);
                    break;
                case ref syscall.SockaddrInet6 sa:
                    return ref new UDPAddr(IP:sa.Addr[0:],Port:sa.Port,Zone:zoneCache.name(int(sa.ZoneId)));
                    break;
            }
            return null;
        }

        private static long family(this ref UDPAddr a)
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

        private static (syscall.Sockaddr, error) sockaddr(this ref UDPAddr a, long family)
        {
            if (a == null)
            {
                return (null, null);
            }
            return ipToSockaddr(family, a.IP, a.Port, a.Zone);
        }

        private static sockaddr toLocal(this ref UDPAddr a, @string net)
        {
            return ref new UDPAddr(loopbackIP(net),a.Port,a.Zone);
        }

        private static (long, ref UDPAddr, error) readFrom(this ref UDPConn c, slice<byte> b)
        {
            ref UDPAddr addr = default;
            var (n, sa, err) = c.fd.readFrom(b);
            switch (sa.type())
            {
                case ref syscall.SockaddrInet4 sa:
                    addr = ref new UDPAddr(IP:sa.Addr[0:],Port:sa.Port);
                    break;
                case ref syscall.SockaddrInet6 sa:
                    addr = ref new UDPAddr(IP:sa.Addr[0:],Port:sa.Port,Zone:zoneCache.name(int(sa.ZoneId)));
                    break;
            }
            return (n, addr, err);
        }

        private static (long, long, long, ref UDPAddr, error) readMsg(this ref UDPConn c, slice<byte> b, slice<byte> oob)
        {
            syscall.Sockaddr sa = default;
            n, oobn, flags, sa, err = c.fd.readMsg(b, oob);
            switch (sa.type())
            {
                case ref syscall.SockaddrInet4 sa:
                    addr = ref new UDPAddr(IP:sa.Addr[0:],Port:sa.Port);
                    break;
                case ref syscall.SockaddrInet6 sa:
                    addr = ref new UDPAddr(IP:sa.Addr[0:],Port:sa.Port,Zone:zoneCache.name(int(sa.ZoneId)));
                    break;
            }
            return;
        }

        private static (long, error) writeTo(this ref UDPConn c, slice<byte> b, ref UDPAddr addr)
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

        private static (long, long, error) writeMsg(this ref UDPConn c, slice<byte> b, slice<byte> oob, ref UDPAddr addr)
        {
            if (c.fd.isConnected && addr != null)
            {
                return (0L, 0L, ErrWriteToConnected);
            }
            if (!c.fd.isConnected && addr == null)
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

        private static (ref UDPConn, error) dialUDP(context.Context ctx, @string net, ref UDPAddr laddr, ref UDPAddr raddr)
        {
            var (fd, err) = internetSocket(ctx, net, laddr, raddr, syscall.SOCK_DGRAM, 0L, "dial");
            if (err != null)
            {
                return (null, err);
            }
            return (newUDPConn(fd), null);
        }

        private static (ref UDPConn, error) listenUDP(context.Context ctx, @string network, ref UDPAddr laddr)
        {
            var (fd, err) = internetSocket(ctx, network, laddr, null, syscall.SOCK_DGRAM, 0L, "listen");
            if (err != null)
            {
                return (null, err);
            }
            return (newUDPConn(fd), null);
        }

        private static (ref UDPConn, error) listenMulticastUDP(context.Context ctx, @string network, ref Interface ifi, ref UDPAddr gaddr)
        {
            var (fd, err) = internetSocket(ctx, network, gaddr, null, syscall.SOCK_DGRAM, 0L, "listen");
            if (err != null)
            {
                return (null, err);
            }
            var c = newUDPConn(fd);
            {
                var ip4 = gaddr.IP.To4();

                if (ip4 != null)
                {
                    {
                        var err__prev2 = err;

                        var err = listenIPv4MulticastUDP(c, ifi, ip4);

                        if (err != null)
                        {
                            c.Close();
                            return (null, err);
                        }

                        err = err__prev2;

                    }
                }
                else
                {
                    {
                        var err__prev2 = err;

                        err = listenIPv6MulticastUDP(c, ifi, gaddr.IP);

                        if (err != null)
                        {
                            c.Close();
                            return (null, err);
                        }

                        err = err__prev2;

                    }
                }

            }
            return (c, null);
        }

        private static error listenIPv4MulticastUDP(ref UDPConn c, ref Interface ifi, IP ip)
        {
            if (ifi != null)
            {
                {
                    var err__prev2 = err;

                    var err = setIPv4MulticastInterface(c.fd, ifi);

                    if (err != null)
                    {
                        return error.As(err);
                    }

                    err = err__prev2;

                }
            }
            {
                var err__prev1 = err;

                err = setIPv4MulticastLoopback(c.fd, false);

                if (err != null)
                {
                    return error.As(err);
                }

                err = err__prev1;

            }
            {
                var err__prev1 = err;

                err = joinIPv4Group(c.fd, ifi, ip);

                if (err != null)
                {
                    return error.As(err);
                }

                err = err__prev1;

            }
            return error.As(null);
        }

        private static error listenIPv6MulticastUDP(ref UDPConn c, ref Interface ifi, IP ip)
        {
            if (ifi != null)
            {
                {
                    var err__prev2 = err;

                    var err = setIPv6MulticastInterface(c.fd, ifi);

                    if (err != null)
                    {
                        return error.As(err);
                    }

                    err = err__prev2;

                }
            }
            {
                var err__prev1 = err;

                err = setIPv6MulticastLoopback(c.fd, false);

                if (err != null)
                {
                    return error.As(err);
                }

                err = err__prev1;

            }
            {
                var err__prev1 = err;

                err = joinIPv6Group(c.fd, ifi, ip);

                if (err != null)
                {
                    return error.As(err);
                }

                err = err__prev1;

            }
            return error.As(null);
        }
    }
}
