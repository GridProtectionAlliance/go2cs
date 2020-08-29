// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package net -- go2cs converted at 2020 August 29 08:26:45 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\iprawsock.go
using context = go.context_package;
using syscall = go.syscall_package;
using static go.builtin;

namespace go
{
    public static partial class net_package
    {
        // BUG(mikio): On every POSIX platform, reads from the "ip4" network
        // using the ReadFrom or ReadFromIP method might not return a complete
        // IPv4 packet, including its header, even if there is space
        // available. This can occur even in cases where Read or ReadMsgIP
        // could return a complete packet. For this reason, it is recommended
        // that you do not use these methods if it is important to receive a
        // full packet.
        //
        // The Go 1 compatibility guidelines make it impossible for us to
        // change the behavior of these methods; use Read or ReadMsgIP
        // instead.

        // BUG(mikio): On NaCl and Plan 9, the ReadMsgIP and
        // WriteMsgIP methods of IPConn are not implemented.

        // BUG(mikio): On Windows, the File method of IPConn is not
        // implemented.

        // IPAddr represents the address of an IP end point.
        public partial struct IPAddr
        {
            public IP IP;
            public @string Zone; // IPv6 scoped addressing zone
        }

        // Network returns the address's network name, "ip".
        private static @string Network(this ref IPAddr a)
        {
            return "ip";
        }

        private static @string String(this ref IPAddr a)
        {
            if (a == null)
            {
                return "<nil>";
            }
            var ip = ipEmptyString(a.IP);
            if (a.Zone != "")
            {
                return ip + "%" + a.Zone;
            }
            return ip;
        }

        private static bool isWildcard(this ref IPAddr a)
        {
            if (a == null || a.IP == null)
            {
                return true;
            }
            return a.IP.IsUnspecified();
        }

        private static Addr opAddr(this ref IPAddr a)
        {
            if (a == null)
            {
                return null;
            }
            return a;
        }

        // ResolveIPAddr returns an address of IP end point.
        //
        // The network must be an IP network name.
        //
        // If the host in the address parameter is not a literal IP address,
        // ResolveIPAddr resolves the address to an address of IP end point.
        // Otherwise, it parses the address as a literal IP address.
        // The address parameter can use a host name, but this is not
        // recommended, because it will return at most one of the host name's
        // IP addresses.
        //
        // See func Dial for a description of the network and address
        // parameters.
        public static (ref IPAddr, error) ResolveIPAddr(@string network, @string address)
        {
            if (network == "")
            { // a hint wildcard for Go 1.0 undocumented behavior
                network = "ip";
            }
            var (afnet, _, err) = parseNetwork(context.Background(), network, false);
            if (err != null)
            {
                return (null, err);
            }
            switch (afnet)
            {
                case "ip": 

                case "ip4": 

                case "ip6": 
                    break;
                default: 
                    return (null, UnknownNetworkError(network));
                    break;
            }
            var (addrs, err) = DefaultResolver.internetAddrList(context.Background(), afnet, address);
            if (err != null)
            {
                return (null, err);
            }
            return (addrs.forResolve(network, address)._<ref IPAddr>(), null);
        }

        // IPConn is the implementation of the Conn and PacketConn interfaces
        // for IP network connections.
        public partial struct IPConn
        {
            public ref conn conn => ref conn_val;
        }

        // SyscallConn returns a raw network connection.
        // This implements the syscall.Conn interface.
        private static (syscall.RawConn, error) SyscallConn(this ref IPConn c)
        {
            if (!c.ok())
            {
                return (null, syscall.EINVAL);
            }
            return newRawConn(c.fd);
        }

        // ReadFromIP acts like ReadFrom but returns an IPAddr.
        private static (long, ref IPAddr, error) ReadFromIP(this ref IPConn c, slice<byte> b)
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
        private static (long, Addr, error) ReadFrom(this ref IPConn c, slice<byte> b)
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

        // ReadMsgIP reads a message from c, copying the payload into b and
        // the associated out-of-band data into oob. It returns the number of
        // bytes copied into b, the number of bytes copied into oob, the flags
        // that were set on the message and the source address of the message.
        //
        // The packages golang.org/x/net/ipv4 and golang.org/x/net/ipv6 can be
        // used to manipulate IP-level socket options in oob.
        private static (long, long, long, ref IPAddr, error) ReadMsgIP(this ref IPConn c, slice<byte> b, slice<byte> oob)
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

        // WriteToIP acts like WriteTo but takes an IPAddr.
        private static (long, error) WriteToIP(this ref IPConn c, slice<byte> b, ref IPAddr addr)
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
        private static (long, error) WriteTo(this ref IPConn c, slice<byte> b, Addr addr)
        {
            if (!c.ok())
            {
                return (0L, syscall.EINVAL);
            }
            ref IPAddr (a, ok) = addr._<ref IPAddr>();
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

        // WriteMsgIP writes a message to addr via c, copying the payload from
        // b and the associated out-of-band data from oob. It returns the
        // number of payload and out-of-band bytes written.
        //
        // The packages golang.org/x/net/ipv4 and golang.org/x/net/ipv6 can be
        // used to manipulate IP-level socket options in oob.
        private static (long, long, error) WriteMsgIP(this ref IPConn c, slice<byte> b, slice<byte> oob, ref IPAddr addr)
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

        private static ref IPConn newIPConn(ref netFD fd)
        {
            return ref new IPConn(conn{fd});
        }

        // DialIP acts like Dial for IP networks.
        //
        // The network must be an IP network name; see func Dial for details.
        //
        // If laddr is nil, a local address is automatically chosen.
        // If the IP field of raddr is nil or an unspecified IP address, the
        // local system is assumed.
        public static (ref IPConn, error) DialIP(@string network, ref IPAddr laddr, ref IPAddr raddr)
        {
            var (c, err) = dialIP(context.Background(), network, laddr, raddr);
            if (err != null)
            {
                return (null, ref new OpError(Op:"dial",Net:network,Source:laddr.opAddr(),Addr:raddr.opAddr(),Err:err));
            }
            return (c, null);
        }

        // ListenIP acts like ListenPacket for IP networks.
        //
        // The network must be an IP network name; see func Dial for details.
        //
        // If the IP field of laddr is nil or an unspecified IP address,
        // ListenIP listens on all available IP addresses of the local system
        // except multicast IP addresses.
        public static (ref IPConn, error) ListenIP(@string network, ref IPAddr laddr)
        {
            var (c, err) = listenIP(context.Background(), network, laddr);
            if (err != null)
            {
                return (null, ref new OpError(Op:"listen",Net:network,Source:nil,Addr:laddr.opAddr(),Err:err));
            }
            return (c, null);
        }
    }
}
