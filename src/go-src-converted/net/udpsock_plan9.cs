// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package net -- go2cs converted at 2020 October 09 04:52:33 UTC
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
        private static (long, ptr<UDPAddr>, error) readFrom(this ptr<UDPConn> _addr_c, slice<byte> b)
        {
            long n = default;
            ptr<UDPAddr> addr = default!;
            error err = default!;
            ref UDPConn c = ref _addr_c.val;

            var buf = make_slice<byte>(udpHeaderSize + len(b));
            var (m, err) = c.fd.Read(buf);
            if (err != null)
            {
                return (0L, _addr_null!, error.As(err)!);
            }
            if (m < udpHeaderSize)
            {
                return (0L, _addr_null!, error.As(errors.New("short read reading UDP header"))!);
            }
            buf = buf[..m];

            var (h, buf) = unmarshalUDPHeader(buf);
            n = copy(b, buf);
            return (n, addr(new UDPAddr(IP:h.raddr,Port:int(h.rport))), error.As(null!)!);

        }

        private static (long, long, long, ptr<UDPAddr>, error) readMsg(this ptr<UDPConn> _addr_c, slice<byte> b, slice<byte> oob)
        {
            long n = default;
            long oobn = default;
            long flags = default;
            ptr<UDPAddr> addr = default!;
            error err = default!;
            ref UDPConn c = ref _addr_c.val;

            return (0L, 0L, 0L, _addr_null!, error.As(syscall.EPLAN9)!);
        }

        private static (long, error) writeTo(this ptr<UDPConn> _addr_c, slice<byte> b, ptr<UDPAddr> _addr_addr)
        {
            long _p0 = default;
            error _p0 = default!;
            ref UDPConn c = ref _addr_c.val;
            ref UDPAddr addr = ref _addr_addr.val;

            if (addr == null)
            {
                return (0L, error.As(errMissingAddress)!);
            }

            ptr<object> h = @new<udpHeader>();
            h.raddr = addr.IP.To16();
            h.laddr = c.fd.laddr._<ptr<UDPAddr>>().IP.To16();
            h.ifcaddr = IPv6zero; // ignored (receive only)
            h.rport = uint16(addr.Port);
            h.lport = uint16(c.fd.laddr._<ptr<UDPAddr>>().Port);

            var buf = make_slice<byte>(udpHeaderSize + len(b));
            var i = copy(buf, h.Bytes());
            copy(buf[i..], b);
            {
                var (_, err) = c.fd.Write(buf);

                if (err != null)
                {
                    return (0L, error.As(err)!);
                }

            }

            return (len(b), error.As(null!)!);

        }

        private static (long, long, error) writeMsg(this ptr<UDPConn> _addr_c, slice<byte> b, slice<byte> oob, ptr<UDPAddr> _addr_addr)
        {
            long n = default;
            long oobn = default;
            error err = default!;
            ref UDPConn c = ref _addr_c.val;
            ref UDPAddr addr = ref _addr_addr.val;

            return (0L, 0L, error.As(syscall.EPLAN9)!);
        }

        private static (ptr<UDPConn>, error) dialUDP(this ptr<sysDialer> _addr_sd, context.Context ctx, ptr<UDPAddr> _addr_laddr, ptr<UDPAddr> _addr_raddr)
        {
            ptr<UDPConn> _p0 = default!;
            error _p0 = default!;
            ref sysDialer sd = ref _addr_sd.val;
            ref UDPAddr laddr = ref _addr_laddr.val;
            ref UDPAddr raddr = ref _addr_raddr.val;

            var (fd, err) = dialPlan9(ctx, sd.network, laddr, raddr);
            if (err != null)
            {
                return (_addr_null!, error.As(err)!);
            }

            return (_addr_newUDPConn(fd)!, error.As(null!)!);

        }

        private static readonly long udpHeaderSize = (long)16L * 3L + 2L * 2L;



        private partial struct udpHeader
        {
            public IP raddr;
            public IP laddr;
            public IP ifcaddr;
            public ushort rport;
            public ushort lport;
        }

        private static slice<byte> Bytes(this ptr<udpHeader> _addr_h)
        {
            ref udpHeader h = ref _addr_h.val;

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

        private static (ptr<udpHeader>, slice<byte>) unmarshalUDPHeader(slice<byte> b)
        {
            ptr<udpHeader> _p0 = default!;
            slice<byte> _p0 = default;

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
            return (_addr_h!, b);

        }

        private static (ptr<UDPConn>, error) listenUDP(this ptr<sysListener> _addr_sl, context.Context ctx, ptr<UDPAddr> _addr_laddr)
        {
            ptr<UDPConn> _p0 = default!;
            error _p0 = default!;
            ref sysListener sl = ref _addr_sl.val;
            ref UDPAddr laddr = ref _addr_laddr.val;

            var (l, err) = listenPlan9(ctx, sl.network, laddr);
            if (err != null)
            {
                return (_addr_null!, error.As(err)!);
            }

            _, err = l.ctl.WriteString("headers");
            if (err != null)
            {
                return (_addr_null!, error.As(err)!);
            }

            l.data, err = os.OpenFile(l.dir + "/data", os.O_RDWR, 0L);
            if (err != null)
            {
                return (_addr_null!, error.As(err)!);
            }

            var (fd, err) = l.netFD();
            return (_addr_newUDPConn(fd)!, error.As(err)!);

        }

        private static (ptr<UDPConn>, error) listenMulticastUDP(this ptr<sysListener> _addr_sl, context.Context ctx, ptr<Interface> _addr_ifi, ptr<UDPAddr> _addr_gaddr)
        {
            ptr<UDPConn> _p0 = default!;
            error _p0 = default!;
            ref sysListener sl = ref _addr_sl.val;
            ref Interface ifi = ref _addr_ifi.val;
            ref UDPAddr gaddr = ref _addr_gaddr.val;
 
            // Plan 9 does not like announce command with a multicast address,
            // so do not specify an IP address when listening.
            var (l, err) = listenPlan9(ctx, sl.network, addr(new UDPAddr(IP:nil,Port:gaddr.Port,Zone:gaddr.Zone)));
            if (err != null)
            {
                return (_addr_null!, error.As(err)!);
            }

            _, err = l.ctl.WriteString("headers");
            if (err != null)
            {
                return (_addr_null!, error.As(err)!);
            }

            slice<Addr> addrs = default;
            if (ifi != null)
            {
                addrs, err = ifi.Addrs();
                if (err != null)
                {
                    return (_addr_null!, error.As(err)!);
                }

            }
            else
            {
                addrs, err = InterfaceAddrs();
                if (err != null)
                {
                    return (_addr_null!, error.As(err)!);
                }

            }

            var have4 = gaddr.IP.To4() != null;
            foreach (var (_, addr) in addrs)
            {
                {
                    ptr<IPNet> (ipnet, ok) = addr._<ptr<IPNet>>();

                    if (ok && (ipnet.IP.To4() != null) == have4)
                    {
                        _, err = l.ctl.WriteString("addmulti " + ipnet.IP.String() + " " + gaddr.IP.String());
                        if (err != null)
                        {
                            return (_addr_null!, error.As(addr(new OpError(Op:"addmulti",Net:"",Source:nil,Addr:ipnet,Err:err))!)!);
                        }

                    }

                }

            }
            l.data, err = os.OpenFile(l.dir + "/data", os.O_RDWR, 0L);
            if (err != null)
            {
                return (_addr_null!, error.As(err)!);
            }

            var (fd, err) = l.netFD();
            if (err != null)
            {
                return (_addr_null!, error.As(err)!);
            }

            return (_addr_newUDPConn(fd)!, error.As(null!)!);

        }
    }
}
