// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package net -- go2cs converted at 2020 October 09 04:52:32 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\udpsock.go
using context = go.context_package;
using syscall = go.syscall_package;
using static go.builtin;

namespace go
{
    public static partial class net_package
    {
        // BUG(mikio): On Plan 9, the ReadMsgUDP and
        // WriteMsgUDP methods of UDPConn are not implemented.

        // BUG(mikio): On Windows, the File method of UDPConn is not
        // implemented.

        // BUG(mikio): On JS, methods and functions related to UDPConn are not
        // implemented.

        // UDPAddr represents the address of a UDP end point.
        public partial struct UDPAddr
        {
            public IP IP;
            public long Port;
            public @string Zone; // IPv6 scoped addressing zone
        }

        // Network returns the address's network name, "udp".
        private static @string Network(this ptr<UDPAddr> _addr_a)
        {
            ref UDPAddr a = ref _addr_a.val;

            return "udp";
        }

        private static @string String(this ptr<UDPAddr> _addr_a)
        {
            ref UDPAddr a = ref _addr_a.val;

            if (a == null)
            {
                return "<nil>";
            }

            var ip = ipEmptyString(a.IP);
            if (a.Zone != "")
            {
                return JoinHostPort(ip + "%" + a.Zone, itoa(a.Port));
            }

            return JoinHostPort(ip, itoa(a.Port));

        }

        private static bool isWildcard(this ptr<UDPAddr> _addr_a)
        {
            ref UDPAddr a = ref _addr_a.val;

            if (a == null || a.IP == null)
            {
                return true;
            }

            return a.IP.IsUnspecified();

        }

        private static Addr opAddr(this ptr<UDPAddr> _addr_a)
        {
            ref UDPAddr a = ref _addr_a.val;

            if (a == null)
            {
                return null;
            }

            return a;

        }

        // ResolveUDPAddr returns an address of UDP end point.
        //
        // The network must be a UDP network name.
        //
        // If the host in the address parameter is not a literal IP address or
        // the port is not a literal port number, ResolveUDPAddr resolves the
        // address to an address of UDP end point.
        // Otherwise, it parses the address as a pair of literal IP address
        // and port number.
        // The address parameter can use a host name, but this is not
        // recommended, because it will return at most one of the host name's
        // IP addresses.
        //
        // See func Dial for a description of the network and address
        // parameters.
        public static (ptr<UDPAddr>, error) ResolveUDPAddr(@string network, @string address)
        {
            ptr<UDPAddr> _p0 = default!;
            error _p0 = default!;

            switch (network)
            {
                case "udp": 

                case "udp4": 

                case "udp6": 
                    break;
                case "": // a hint wildcard for Go 1.0 undocumented behavior
                    network = "udp";
                    break;
                default: 
                    return (_addr_null!, error.As(UnknownNetworkError(network))!);
                    break;
            }
            var (addrs, err) = DefaultResolver.internetAddrList(context.Background(), network, address);
            if (err != null)
            {
                return (_addr_null!, error.As(err)!);
            }

            return (addrs.forResolve(network, address)._<ptr<UDPAddr>>(), error.As(null!)!);

        }

        // UDPConn is the implementation of the Conn and PacketConn interfaces
        // for UDP network connections.
        public partial struct UDPConn
        {
            public ref conn conn => ref conn_val;
        }

        // SyscallConn returns a raw network connection.
        // This implements the syscall.Conn interface.
        private static (syscall.RawConn, error) SyscallConn(this ptr<UDPConn> _addr_c)
        {
            syscall.RawConn _p0 = default;
            error _p0 = default!;
            ref UDPConn c = ref _addr_c.val;

            if (!c.ok())
            {
                return (null, error.As(syscall.EINVAL)!);
            }

            return newRawConn(c.fd);

        }

        // ReadFromUDP acts like ReadFrom but returns a UDPAddr.
        private static (long, ptr<UDPAddr>, error) ReadFromUDP(this ptr<UDPConn> _addr_c, slice<byte> b)
        {
            long _p0 = default;
            ptr<UDPAddr> _p0 = default!;
            error _p0 = default!;
            ref UDPConn c = ref _addr_c.val;

            if (!c.ok())
            {
                return (0L, _addr_null!, error.As(syscall.EINVAL)!);
            }

            var (n, addr, err) = c.readFrom(b);
            if (err != null)
            {
                err = addr(new OpError(Op:"read",Net:c.fd.net,Source:c.fd.laddr,Addr:c.fd.raddr,Err:err));
            }

            return (n, _addr_addr!, error.As(err)!);

        }

        // ReadFrom implements the PacketConn ReadFrom method.
        private static (long, Addr, error) ReadFrom(this ptr<UDPConn> _addr_c, slice<byte> b)
        {
            long _p0 = default;
            Addr _p0 = default;
            error _p0 = default!;
            ref UDPConn c = ref _addr_c.val;

            if (!c.ok())
            {
                return (0L, null, error.As(syscall.EINVAL)!);
            }

            var (n, addr, err) = c.readFrom(b);
            if (err != null)
            {
                err = addr(new OpError(Op:"read",Net:c.fd.net,Source:c.fd.laddr,Addr:c.fd.raddr,Err:err));
            }

            if (addr == null)
            {
                return (n, null, error.As(err)!);
            }

            return (n, addr, error.As(err)!);

        }

        // ReadMsgUDP reads a message from c, copying the payload into b and
        // the associated out-of-band data into oob. It returns the number of
        // bytes copied into b, the number of bytes copied into oob, the flags
        // that were set on the message and the source address of the message.
        //
        // The packages golang.org/x/net/ipv4 and golang.org/x/net/ipv6 can be
        // used to manipulate IP-level socket options in oob.
        private static (long, long, long, ptr<UDPAddr>, error) ReadMsgUDP(this ptr<UDPConn> _addr_c, slice<byte> b, slice<byte> oob)
        {
            long n = default;
            long oobn = default;
            long flags = default;
            ptr<UDPAddr> addr = default!;
            error err = default!;
            ref UDPConn c = ref _addr_c.val;

            if (!c.ok())
            {
                return (0L, 0L, 0L, _addr_null!, error.As(syscall.EINVAL)!);
            }

            n, oobn, flags, addr, err = c.readMsg(b, oob);
            if (err != null)
            {
                err = addr(new OpError(Op:"read",Net:c.fd.net,Source:c.fd.laddr,Addr:c.fd.raddr,Err:err));
            }

            return ;

        }

        // WriteToUDP acts like WriteTo but takes a UDPAddr.
        private static (long, error) WriteToUDP(this ptr<UDPConn> _addr_c, slice<byte> b, ptr<UDPAddr> _addr_addr)
        {
            long _p0 = default;
            error _p0 = default!;
            ref UDPConn c = ref _addr_c.val;
            ref UDPAddr addr = ref _addr_addr.val;

            if (!c.ok())
            {
                return (0L, error.As(syscall.EINVAL)!);
            }

            var (n, err) = c.writeTo(b, addr);
            if (err != null)
            {
                err = addr(new OpError(Op:"write",Net:c.fd.net,Source:c.fd.laddr,Addr:addr.opAddr(),Err:err));
            }

            return (n, error.As(err)!);

        }

        // WriteTo implements the PacketConn WriteTo method.
        private static (long, error) WriteTo(this ptr<UDPConn> _addr_c, slice<byte> b, Addr addr)
        {
            long _p0 = default;
            error _p0 = default!;
            ref UDPConn c = ref _addr_c.val;

            if (!c.ok())
            {
                return (0L, error.As(syscall.EINVAL)!);
            }

            ptr<UDPAddr> (a, ok) = addr._<ptr<UDPAddr>>();
            if (!ok)
            {
                return (0L, error.As(addr(new OpError(Op:"write",Net:c.fd.net,Source:c.fd.laddr,Addr:addr,Err:syscall.EINVAL))!)!);
            }

            var (n, err) = c.writeTo(b, a);
            if (err != null)
            {
                err = addr(new OpError(Op:"write",Net:c.fd.net,Source:c.fd.laddr,Addr:a.opAddr(),Err:err));
            }

            return (n, error.As(err)!);

        }

        // WriteMsgUDP writes a message to addr via c if c isn't connected, or
        // to c's remote address if c is connected (in which case addr must be
        // nil). The payload is copied from b and the associated out-of-band
        // data is copied from oob. It returns the number of payload and
        // out-of-band bytes written.
        //
        // The packages golang.org/x/net/ipv4 and golang.org/x/net/ipv6 can be
        // used to manipulate IP-level socket options in oob.
        private static (long, long, error) WriteMsgUDP(this ptr<UDPConn> _addr_c, slice<byte> b, slice<byte> oob, ptr<UDPAddr> _addr_addr)
        {
            long n = default;
            long oobn = default;
            error err = default!;
            ref UDPConn c = ref _addr_c.val;
            ref UDPAddr addr = ref _addr_addr.val;

            if (!c.ok())
            {
                return (0L, 0L, error.As(syscall.EINVAL)!);
            }

            n, oobn, err = c.writeMsg(b, oob, addr);
            if (err != null)
            {
                err = addr(new OpError(Op:"write",Net:c.fd.net,Source:c.fd.laddr,Addr:addr.opAddr(),Err:err));
            }

            return ;

        }

        private static ptr<UDPConn> newUDPConn(ptr<netFD> _addr_fd)
        {
            ref netFD fd = ref _addr_fd.val;

            return addr(new UDPConn(conn{fd}));
        }

        // DialUDP acts like Dial for UDP networks.
        //
        // The network must be a UDP network name; see func Dial for details.
        //
        // If laddr is nil, a local address is automatically chosen.
        // If the IP field of raddr is nil or an unspecified IP address, the
        // local system is assumed.
        public static (ptr<UDPConn>, error) DialUDP(@string network, ptr<UDPAddr> _addr_laddr, ptr<UDPAddr> _addr_raddr)
        {
            ptr<UDPConn> _p0 = default!;
            error _p0 = default!;
            ref UDPAddr laddr = ref _addr_laddr.val;
            ref UDPAddr raddr = ref _addr_raddr.val;

            switch (network)
            {
                case "udp": 

                case "udp4": 

                case "udp6": 
                    break;
                default: 
                    return (_addr_null!, error.As(addr(new OpError(Op:"dial",Net:network,Source:laddr.opAddr(),Addr:raddr.opAddr(),Err:UnknownNetworkError(network)))!)!);
                    break;
            }
            if (raddr == null)
            {
                return (_addr_null!, error.As(addr(new OpError(Op:"dial",Net:network,Source:laddr.opAddr(),Addr:nil,Err:errMissingAddress))!)!);
            }

            ptr<sysDialer> sd = addr(new sysDialer(network:network,address:raddr.String()));
            var (c, err) = sd.dialUDP(context.Background(), laddr, raddr);
            if (err != null)
            {
                return (_addr_null!, error.As(addr(new OpError(Op:"dial",Net:network,Source:laddr.opAddr(),Addr:raddr.opAddr(),Err:err))!)!);
            }

            return (_addr_c!, error.As(null!)!);

        }

        // ListenUDP acts like ListenPacket for UDP networks.
        //
        // The network must be a UDP network name; see func Dial for details.
        //
        // If the IP field of laddr is nil or an unspecified IP address,
        // ListenUDP listens on all available IP addresses of the local system
        // except multicast IP addresses.
        // If the Port field of laddr is 0, a port number is automatically
        // chosen.
        public static (ptr<UDPConn>, error) ListenUDP(@string network, ptr<UDPAddr> _addr_laddr)
        {
            ptr<UDPConn> _p0 = default!;
            error _p0 = default!;
            ref UDPAddr laddr = ref _addr_laddr.val;

            switch (network)
            {
                case "udp": 

                case "udp4": 

                case "udp6": 
                    break;
                default: 
                    return (_addr_null!, error.As(addr(new OpError(Op:"listen",Net:network,Source:nil,Addr:laddr.opAddr(),Err:UnknownNetworkError(network)))!)!);
                    break;
            }
            if (laddr == null)
            {
                laddr = addr(new UDPAddr());
            }

            ptr<sysListener> sl = addr(new sysListener(network:network,address:laddr.String()));
            var (c, err) = sl.listenUDP(context.Background(), laddr);
            if (err != null)
            {
                return (_addr_null!, error.As(addr(new OpError(Op:"listen",Net:network,Source:nil,Addr:laddr.opAddr(),Err:err))!)!);
            }

            return (_addr_c!, error.As(null!)!);

        }

        // ListenMulticastUDP acts like ListenPacket for UDP networks but
        // takes a group address on a specific network interface.
        //
        // The network must be a UDP network name; see func Dial for details.
        //
        // ListenMulticastUDP listens on all available IP addresses of the
        // local system including the group, multicast IP address.
        // If ifi is nil, ListenMulticastUDP uses the system-assigned
        // multicast interface, although this is not recommended because the
        // assignment depends on platforms and sometimes it might require
        // routing configuration.
        // If the Port field of gaddr is 0, a port number is automatically
        // chosen.
        //
        // ListenMulticastUDP is just for convenience of simple, small
        // applications. There are golang.org/x/net/ipv4 and
        // golang.org/x/net/ipv6 packages for general purpose uses.
        public static (ptr<UDPConn>, error) ListenMulticastUDP(@string network, ptr<Interface> _addr_ifi, ptr<UDPAddr> _addr_gaddr)
        {
            ptr<UDPConn> _p0 = default!;
            error _p0 = default!;
            ref Interface ifi = ref _addr_ifi.val;
            ref UDPAddr gaddr = ref _addr_gaddr.val;

            switch (network)
            {
                case "udp": 

                case "udp4": 

                case "udp6": 
                    break;
                default: 
                    return (_addr_null!, error.As(addr(new OpError(Op:"listen",Net:network,Source:nil,Addr:gaddr.opAddr(),Err:UnknownNetworkError(network)))!)!);
                    break;
            }
            if (gaddr == null || gaddr.IP == null)
            {
                return (_addr_null!, error.As(addr(new OpError(Op:"listen",Net:network,Source:nil,Addr:gaddr.opAddr(),Err:errMissingAddress))!)!);
            }

            ptr<sysListener> sl = addr(new sysListener(network:network,address:gaddr.String()));
            var (c, err) = sl.listenMulticastUDP(context.Background(), ifi, gaddr);
            if (err != null)
            {
                return (_addr_null!, error.As(addr(new OpError(Op:"listen",Net:network,Source:nil,Addr:gaddr.opAddr(),Err:err))!)!);
            }

            return (_addr_c!, error.As(null!)!);

        }
    }
}
