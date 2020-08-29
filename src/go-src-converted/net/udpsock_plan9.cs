// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package net -- go2cs converted at 2020 August 29 08:28:05 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\udpsock_plan9.go
using context = go.context_package;
using errors = go.errors_package;
using os = go.os_package;
using syscall = go.syscall_package;
using static go.builtin;

namespace go
{
    public static partial class net_package
    {
        private static (long, ref UDPAddr, error) readFrom(this ref UDPConn c, slice<byte> b)
        {
            var buf = make_slice<byte>(udpHeaderSize + len(b));
            var (m, err) = c.fd.Read(buf);
            if (err != null)
            {
                return (0L, null, err);
            }
            if (m < udpHeaderSize)
            {
                return (0L, null, errors.New("short read reading UDP header"));
            }
            buf = buf[..m];

            var (h, buf) = unmarshalUDPHeader(buf);
            n = copy(b, buf);
            return (n, ref new UDPAddr(IP:h.raddr,Port:int(h.rport)), null);
        }

        private static (long, long, long, ref UDPAddr, error) readMsg(this ref UDPConn c, slice<byte> b, slice<byte> oob)
        {
            return (0L, 0L, 0L, null, syscall.EPLAN9);
        }

        private static (long, error) writeTo(this ref UDPConn c, slice<byte> b, ref UDPAddr addr)
        {
            if (addr == null)
            {
                return (0L, errMissingAddress);
            }
            ptr<object> h = @new<udpHeader>();
            h.raddr = addr.IP.To16();
            h.laddr = c.fd.laddr._<ref UDPAddr>().IP.To16();
            h.ifcaddr = IPv6zero; // ignored (receive only)
            h.rport = uint16(addr.Port);
            h.lport = uint16(c.fd.laddr._<ref UDPAddr>().Port);

            var buf = make_slice<byte>(udpHeaderSize + len(b));
            var i = copy(buf, h.Bytes());
            copy(buf[i..], b);
            {
                var (_, err) = c.fd.Write(buf);

                if (err != null)
                {
                    return (0L, err);
                }

            }
            return (len(b), null);
        }

        private static (long, long, error) writeMsg(this ref UDPConn c, slice<byte> b, slice<byte> oob, ref UDPAddr addr)
        {
            return (0L, 0L, syscall.EPLAN9);
        }

        private static (ref UDPConn, error) dialUDP(context.Context ctx, @string net, ref UDPAddr laddr, ref UDPAddr raddr)
        {
            var (fd, err) = dialPlan9(ctx, net, laddr, raddr);
            if (err != null)
            {
                return (null, err);
            }
            return (newUDPConn(fd), null);
        }

        private static readonly long udpHeaderSize = 16L * 3L + 2L * 2L;



        private partial struct udpHeader
        {
            public IP raddr;
            public IP laddr;
            public IP ifcaddr;
            public ushort rport;
            public ushort lport;
        }

        private static slice<byte> Bytes(this ref udpHeader h)
        {
            var b = make_slice<byte>(udpHeaderSize);
            long i = 0L;
            i += copy(b[i..i + 16L], h.raddr);
            i += copy(b[i..i + 16L], h.laddr);
            i += copy(b[i..i + 16L], h.ifcaddr);
            b[i] = byte(h.rport >> (int)(8L));
            b[i + 1L] = byte(h.rport);
            i = i + 2L;
            b[i] = byte(h.lport >> (int)(8L));
            b[i + 1L] = byte(h.lport);
            i = i + 2L;
            return b;
        }

        private static (ref udpHeader, slice<byte>) unmarshalUDPHeader(slice<byte> b)
        {
            ptr<udpHeader> h = @new<udpHeader>();
            h.raddr = IP(b[..16L]);
            b = b[16L..];
            h.laddr = IP(b[..16L]);
            b = b[16L..];
            h.ifcaddr = IP(b[..16L]);
            b = b[16L..];
            h.rport = uint16(b[0L]) << (int)(8L) | uint16(b[1L]);
            b = b[2L..];
            h.lport = uint16(b[0L]) << (int)(8L) | uint16(b[1L]);
            b = b[2L..];
            return (h, b);
        }

        private static (ref UDPConn, error) listenUDP(context.Context ctx, @string network, ref UDPAddr laddr)
        {
            var (l, err) = listenPlan9(ctx, network, laddr);
            if (err != null)
            {
                return (null, err);
            }
            _, err = l.ctl.WriteString("headers");
            if (err != null)
            {
                return (null, err);
            }
            l.data, err = os.OpenFile(l.dir + "/data", os.O_RDWR, 0L);
            if (err != null)
            {
                return (null, err);
            }
            var (fd, err) = l.netFD();
            return (newUDPConn(fd), err);
        }

        private static (ref UDPConn, error) listenMulticastUDP(context.Context ctx, @string network, ref Interface ifi, ref UDPAddr gaddr)
        {
            var (l, err) = listenPlan9(ctx, network, gaddr);
            if (err != null)
            {
                return (null, err);
            }
            _, err = l.ctl.WriteString("headers");
            if (err != null)
            {
                return (null, err);
            }
            slice<Addr> addrs = default;
            if (ifi != null)
            {
                addrs, err = ifi.Addrs();
                if (err != null)
                {
                    return (null, err);
                }
            }
            else
            {
                addrs, err = InterfaceAddrs();
                if (err != null)
                {
                    return (null, err);
                }
            }
            foreach (var (_, addr) in addrs)
            {
                {
                    ref IPNet (ipnet, ok) = addr._<ref IPNet>();

                    if (ok)
                    {
                        _, err = l.ctl.WriteString("addmulti " + ipnet.IP.String() + " " + gaddr.IP.String());
                        if (err != null)
                        {
                            return (null, err);
                        }
                    }

                }
            }
            l.data, err = os.OpenFile(l.dir + "/data", os.O_RDWR, 0L);
            if (err != null)
            {
                return (null, err);
            }
            var (fd, err) = l.netFD();
            if (err != null)
            {
                return (null, err);
            }
            return (newUDPConn(fd), null);
        }
    }
}
