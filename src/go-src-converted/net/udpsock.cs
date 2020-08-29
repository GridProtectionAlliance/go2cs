// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package net -- go2cs converted at 2020 August 29 08:28:03 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\udpsock.go
using context = go.context_package;
using syscall = go.syscall_package;
using static go.builtin;

namespace go
{
    public static partial class net_package
    {
        // BUG(mikio): On NaCl and Plan 9, the ReadMsgUDP and
        // WriteMsgUDP methods of UDPConn are not implemented.

        // BUG(mikio): On Windows, the File method of UDPConn is not
        // implemented.

        // BUG(mikio): On NaCl, the ListenMulticastUDP function is not
        // implemented.

        // UDPAddr represents the address of a UDP end point.
        public partial struct UDPAddr
        {
            public IP IP;
            public long Port;
            public @string Zone; // IPv6 scoped addressing zone
        }

        // Network returns the address's network name, "udp".
        private static @string Network(this ref UDPAddr a)
        {
            return "udp";
        }

        private static @string String(this ref UDPAddr a)
        {
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

        private static bool isWildcard(this ref UDPAddr a)
        {
            if (a == null || a.IP == null)
            {
                return true;
            }
            return a.IP.IsUnspecified();
        }

        private static Addr opAddr(this ref UDPAddr a)
        {
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
        public static (ref UDPAddr, error) ResolveUDPAddr(@string network, @string address)
        {
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
                    return (null, UnknownNetworkError(network));
                    break;
            }
            var (addrs, err) = DefaultResolver.internetAddrList(context.Background(), network, address);
            if (err != null)
            {
                return (null, err);
            }
            return (addrs.forResolve(network, address)._<ref UDPAddr>(), null);
        }

        // UDPConn is the implementation of the Conn and PacketConn interfaces
        // for UDP network connections.
        public partial struct UDPConn
        {
            public ref conn conn => ref conn_val;
        }

        // SyscallConn returns a raw network connection.
        // This implements the syscall.Conn interface.
        private static (syscall.RawConn, error) SyscallConn(this ref UDPConn c)
        {
            if (!c.ok())
            {
                return (null, syscall.EINVAL);
            }
            return newRawConn(c.fd);
        }

        // ReadFromUDP acts like ReadFrom but returns a UDPAddr.
        private static (long, ref UDPAddr, error) ReadFromUDP(this ref UDPConn c, slice<byte> b)
        {
            if (!c.ok())
            {
                return (0L, null, syscall.EINVAL);
            }
            var (n, addr, err) = c.readFrom(b);
            if (err != null)
            {
                err = ref new OpError(Op:"read",Net:c.fd.net,Source:c.fd.laddr,Addr:c.fd.raddr,Err:err);
            }
            return (n, addr, err);
        }

        // ReadFrom implements the PacketConn ReadFrom method.
        private static (long, Addr, error) ReadFrom(this ref UDPConn c, slice<byte> b)
        {
            if (!c.ok())
            {
                return (0L, null, syscall.EINVAL);
            }
            var (n, addr, err) = c.readFrom(b);
            if (err != null)
            {
                err = ref new OpError(Op:"read",Net:c.fd.net,Source:c.fd.laddr,Addr:c.fd.raddr,Err:err);
            }
            if (addr == null)
            {
                return (n, null, err);
            }
            return (n, addr, err);
        }

        // ReadMsgUDP reads a message from c, copying the payload into b and
        // the associated out-of-band data into oob. It returns the number of
        // bytes copied into b, the number of bytes copied into oob, the flags
        // that were set on the message and the source address of the message.
        //
        // The packages golang.org/x/net/ipv4 and golang.org/x/net/ipv6 can be
        // used to manipulate IP-level socket options in oob.
        private static (long, long, long, ref UDPAddr, error) ReadMsgUDP(this ref UDPConn c, slice<byte> b, slice<byte> oob)
        {
            if (!c.ok())
            {
                return (0L, 0L, 0L, null, syscall.EINVAL);
            }
            n, oobn, flags, addr, err = c.readMsg(b, oob);
            if (err != null)
            {
                err = ref new OpError(Op:"read",Net:c.fd.net,Source:c.fd.laddr,Addr:c.fd.raddr,Err:err);
            }
            return;
        }

        // WriteToUDP acts like WriteTo but takes a UDPAddr.
        private static (long, error) WriteToUDP(this ref UDPConn c, slice<byte> b, ref UDPAddr addr)
        {
            if (!c.ok())
            {
                return (0L, syscall.EINVAL);
            }
            var (n, err) = c.writeTo(b, addr);
            if (err != null)
            {
                err = ref new OpError(Op:"write",Net:c.fd.net,Source:c.fd.laddr,Addr:addr.opAddr(),Err:err);
            }
            return (n, err);
        }

        // WriteTo implements the PacketConn WriteTo method.
        private static (long, error) WriteTo(this ref UDPConn c, slice<byte> b, Addr addr)
        {
            if (!c.ok())
            {
                return (0L, syscall.EINVAL);
            }
            ref UDPAddr (a, ok) = addr._<ref UDPAddr>();
            if (!ok)
            {
                return (0L, ref new OpError(Op:"write",Net:c.fd.net,Source:c.fd.laddr,Addr:addr,Err:syscall.EINVAL));
            }
            var (n, err) = c.writeTo(b, a);
            if (err != null)
            {
                err = ref new OpError(Op:"write",Net:c.fd.net,Source:c.fd.laddr,Addr:a.opAddr(),Err:err);
            }
            return (n, err);
        }

        // WriteMsgUDP writes a message to addr via c if c isn't connected, or
        // to c's remote address if c is connected (in which case addr must be
        // nil). The payload is copied from b and the associated out-of-band
        // data is copied from oob. It returns the number of payload and
        // out-of-band bytes written.
        //
        // The packages golang.org/x/net/ipv4 and golang.org/x/net/ipv6 can be
        // used to manipulate IP-level socket options in oob.
        private static (long, long, error) WriteMsgUDP(this ref UDPConn c, slice<byte> b, slice<byte> oob, ref UDPAddr addr)
        {
            if (!c.ok())
            {
                return (0L, 0L, syscall.EINVAL);
            }
            n, oobn, err = c.writeMsg(b, oob, addr);
            if (err != null)
            {
                err = ref new OpError(Op:"write",Net:c.fd.net,Source:c.fd.laddr,Addr:addr.opAddr(),Err:err);
            }
            return;
        }

        private static ref UDPConn newUDPConn(ref netFD fd)
        {
            return ref new UDPConn(conn{fd});
        }

        // DialUDP acts like Dial for UDP networks.
        //
        // The network must be a UDP network name; see func Dial for details.
        //
        // If laddr is nil, a local address is automatically chosen.
        // If the IP field of raddr is nil or an unspecified IP address, the
        // local system is assumed.
        public static (ref UDPConn, error) DialUDP(@string network, ref UDPAddr laddr, ref UDPAddr raddr)
        {
            switch (network)
            {
                case "udp": 

                case "udp4": 

                case "udp6": 
                    break;
                default: 
                    return (null, ref new OpError(Op:"dial",Net:network,Source:laddr.opAddr(),Addr:raddr.opAddr(),Err:UnknownNetworkError(network)));
                    break;
            }
            if (raddr == null)
            {
                return (null, ref new OpError(Op:"dial",Net:network,Source:laddr.opAddr(),Addr:nil,Err:errMissingAddress));
            }
            var (c, err) = dialUDP(context.Background(), network, laddr, raddr);
            if (err != null)
            {
                return (null, ref new OpError(Op:"dial",Net:network,Source:laddr.opAddr(),Addr:raddr.opAddr(),Err:err));
            }
            return (c, null);
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
        public static (ref UDPConn, error) ListenUDP(@string network, ref UDPAddr laddr)
        {
            switch (network)
            {
                case "udp": 

                case "udp4": 

                case "udp6": 
                    break;
                default: 
                    return (null, ref new OpError(Op:"listen",Net:network,Source:nil,Addr:laddr.opAddr(),Err:UnknownNetworkError(network)));
                    break;
            }
            if (laddr == null)
            {
                laddr = ref new UDPAddr();
            }
            var (c, err) = listenUDP(context.Background(), network, laddr);
            if (err != null)
            {
                return (null, ref new OpError(Op:"listen",Net:network,Source:nil,Addr:laddr.opAddr(),Err:err));
            }
            return (c, null);
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
        public static (ref UDPConn, error) ListenMulticastUDP(@string network, ref Interface ifi, ref UDPAddr gaddr)
        {
            switch (network)
            {
                case "udp": 

                case "udp4": 

                case "udp6": 
                    break;
                default: 
                    return (null, ref new OpError(Op:"listen",Net:network,Source:nil,Addr:gaddr.opAddr(),Err:UnknownNetworkError(network)));
                    break;
            }
            if (gaddr == null || gaddr.IP == null)
            {
                return (null, ref new OpError(Op:"listen",Net:network,Source:nil,Addr:gaddr.opAddr(),Err:errMissingAddress));
            }
            var (c, err) = listenMulticastUDP(context.Background(), network, ifi, gaddr);
            if (err != null)
            {
                return (null, ref new OpError(Op:"listen",Net:network,Source:nil,Addr:gaddr.opAddr(),Err:err));
            }
            return (c, null);
        }
    }
}
