// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build aix darwin dragonfly freebsd js,wasm linux netbsd openbsd solaris windows

// package net -- go2cs converted at 2020 October 08 03:33:45 UTC
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
                case ptr<syscall.SockaddrInet4> sa:
                    return addr(new IPAddr(IP:sa.Addr[0:]));
                    break;
                case ptr<syscall.SockaddrInet6> sa:
                    return addr(new IPAddr(IP:sa.Addr[0:],Zone:zoneCache.name(int(sa.ZoneId))));
                    break;
            }
            return null;

        }

        private static long family(this ptr<IPAddr> _addr_a)
        {
            ref IPAddr a = ref _addr_a.val;

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

        private static (syscall.Sockaddr, error) sockaddr(this ptr<IPAddr> _addr_a, long family)
        {
            syscall.Sockaddr _p0 = default;
            error _p0 = default!;
            ref IPAddr a = ref _addr_a.val;

            if (a == null)
            {
                return (null, error.As(null!)!);
            }

            return ipToSockaddr(family, a.IP, 0L, a.Zone);

        }

        private static sockaddr toLocal(this ptr<IPAddr> _addr_a, @string net)
        {
            ref IPAddr a = ref _addr_a.val;

            return addr(new IPAddr(loopbackIP(net),a.Zone));
        }

        private static (long, ptr<IPAddr>, error) readFrom(this ptr<IPConn> _addr_c, slice<byte> b)
        {
            long _p0 = default;
            ptr<IPAddr> _p0 = default!;
            error _p0 = default!;
            ref IPConn c = ref _addr_c.val;
 
            // TODO(cw,rsc): consider using readv if we know the family
            // type to avoid the header trim/copy
            ptr<IPAddr> addr;
            var (n, sa, err) = c.fd.readFrom(b);
            switch (sa.type())
            {
                case ptr<syscall.SockaddrInet4> sa:
                    addr = addr(new IPAddr(IP:sa.Addr[0:]));
                    n = stripIPv4Header(n, b);
                    break;
                case ptr<syscall.SockaddrInet6> sa:
                    addr = addr(new IPAddr(IP:sa.Addr[0:],Zone:zoneCache.name(int(sa.ZoneId))));
                    break;
            }
            return (n, _addr_addr!, error.As(err)!);

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

        private static (long, long, long, ptr<IPAddr>, error) readMsg(this ptr<IPConn> _addr_c, slice<byte> b, slice<byte> oob)
        {
            long n = default;
            long oobn = default;
            long flags = default;
            ptr<IPAddr> addr = default!;
            error err = default!;
            ref IPConn c = ref _addr_c.val;

            syscall.Sockaddr sa = default;
            n, oobn, flags, sa, err = c.fd.readMsg(b, oob);
            switch (sa.type())
            {
                case ptr<syscall.SockaddrInet4> sa:
                    addr = addr(new IPAddr(IP:sa.Addr[0:]));
                    break;
                case ptr<syscall.SockaddrInet6> sa:
                    addr = addr(new IPAddr(IP:sa.Addr[0:],Zone:zoneCache.name(int(sa.ZoneId))));
                    break;
            }
            return ;

        }

        private static (long, error) writeTo(this ptr<IPConn> _addr_c, slice<byte> b, ptr<IPAddr> _addr_addr)
        {
            long _p0 = default;
            error _p0 = default!;
            ref IPConn c = ref _addr_c.val;
            ref IPAddr addr = ref _addr_addr.val;

            if (c.fd.isConnected)
            {
                return (0L, error.As(ErrWriteToConnected)!);
            }

            if (addr == null)
            {
                return (0L, error.As(errMissingAddress)!);
            }

            var (sa, err) = addr.sockaddr(c.fd.family);
            if (err != null)
            {
                return (0L, error.As(err)!);
            }

            return c.fd.writeTo(b, sa);

        }

        private static (long, long, error) writeMsg(this ptr<IPConn> _addr_c, slice<byte> b, slice<byte> oob, ptr<IPAddr> _addr_addr)
        {
            long n = default;
            long oobn = default;
            error err = default!;
            ref IPConn c = ref _addr_c.val;
            ref IPAddr addr = ref _addr_addr.val;

            if (c.fd.isConnected)
            {
                return (0L, 0L, error.As(ErrWriteToConnected)!);
            }

            if (addr == null)
            {
                return (0L, 0L, error.As(errMissingAddress)!);
            }

            var (sa, err) = addr.sockaddr(c.fd.family);
            if (err != null)
            {
                return (0L, 0L, error.As(err)!);
            }

            return c.fd.writeMsg(b, oob, sa);

        }

        private static (ptr<IPConn>, error) dialIP(this ptr<sysDialer> _addr_sd, context.Context ctx, ptr<IPAddr> _addr_laddr, ptr<IPAddr> _addr_raddr)
        {
            ptr<IPConn> _p0 = default!;
            error _p0 = default!;
            ref sysDialer sd = ref _addr_sd.val;
            ref IPAddr laddr = ref _addr_laddr.val;
            ref IPAddr raddr = ref _addr_raddr.val;

            var (network, proto, err) = parseNetwork(ctx, sd.network, true);
            if (err != null)
            {
                return (_addr_null!, error.As(err)!);
            }

            switch (network)
            {
                case "ip": 

                case "ip4": 

                case "ip6": 
                    break;
                default: 
                    return (_addr_null!, error.As(UnknownNetworkError(sd.network))!);
                    break;
            }
            var (fd, err) = internetSocket(ctx, network, laddr, raddr, syscall.SOCK_RAW, proto, "dial", sd.Dialer.Control);
            if (err != null)
            {
                return (_addr_null!, error.As(err)!);
            }

            return (_addr_newIPConn(fd)!, error.As(null!)!);

        }

        private static (ptr<IPConn>, error) listenIP(this ptr<sysListener> _addr_sl, context.Context ctx, ptr<IPAddr> _addr_laddr)
        {
            ptr<IPConn> _p0 = default!;
            error _p0 = default!;
            ref sysListener sl = ref _addr_sl.val;
            ref IPAddr laddr = ref _addr_laddr.val;

            var (network, proto, err) = parseNetwork(ctx, sl.network, true);
            if (err != null)
            {
                return (_addr_null!, error.As(err)!);
            }

            switch (network)
            {
                case "ip": 

                case "ip4": 

                case "ip6": 
                    break;
                default: 
                    return (_addr_null!, error.As(UnknownNetworkError(sl.network))!);
                    break;
            }
            var (fd, err) = internetSocket(ctx, network, laddr, null, syscall.SOCK_RAW, proto, "listen", sl.ListenConfig.Control);
            if (err != null)
            {
                return (_addr_null!, error.As(err)!);
            }

            return (_addr_newIPConn(fd)!, error.As(null!)!);

        }
    }
}
