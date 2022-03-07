// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package net -- go2cs converted at 2022 March 06 22:16:52 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Program Files\Go\src\net\udpsock_plan9.go
using context = go.context_package;
using errors = go.errors_package;
using os = go.os_package;
using syscall = go.syscall_package;

namespace go;

public static partial class net_package {

private static (nint, ptr<UDPAddr>, error) readFrom(this ptr<UDPConn> _addr_c, slice<byte> b, ptr<UDPAddr> _addr_addr) {
    nint _p0 = default;
    ptr<UDPAddr> _p0 = default!;
    error _p0 = default!;
    ref UDPConn c = ref _addr_c.val;
    ref UDPAddr addr = ref _addr_addr.val;

    var buf = make_slice<byte>(udpHeaderSize + len(b));
    var (m, err) = c.fd.Read(buf);
    if (err != null) {
        return (0, _addr_null!, error.As(err)!);
    }
    if (m < udpHeaderSize) {
        return (0, _addr_null!, error.As(errors.New("short read reading UDP header"))!);
    }
    buf = buf[..(int)m];

    var (h, buf) = unmarshalUDPHeader(buf);
    var n = copy(b, buf);
    addr = new UDPAddr(IP:h.raddr,Port:int(h.rport));
    return (n, _addr_addr!, error.As(null!)!);

}

private static (nint, nint, nint, ptr<UDPAddr>, error) readMsg(this ptr<UDPConn> _addr_c, slice<byte> b, slice<byte> oob) {
    nint n = default;
    nint oobn = default;
    nint flags = default;
    ptr<UDPAddr> addr = default!;
    error err = default!;
    ref UDPConn c = ref _addr_c.val;

    return (0, 0, 0, _addr_null!, error.As(syscall.EPLAN9)!);
}

private static (nint, error) writeTo(this ptr<UDPConn> _addr_c, slice<byte> b, ptr<UDPAddr> _addr_addr) {
    nint _p0 = default;
    error _p0 = default!;
    ref UDPConn c = ref _addr_c.val;
    ref UDPAddr addr = ref _addr_addr.val;

    if (addr == null) {
        return (0, error.As(errMissingAddress)!);
    }
    ptr<object> h = @new<udpHeader>();
    h.raddr = addr.IP.To16();
    h.laddr = c.fd.laddr._<ptr<UDPAddr>>().IP.To16();
    h.ifcaddr = IPv6zero; // ignored (receive only)
    h.rport = uint16(addr.Port);
    h.lport = uint16(c.fd.laddr._<ptr<UDPAddr>>().Port);

    var buf = make_slice<byte>(udpHeaderSize + len(b));
    var i = copy(buf, h.Bytes());
    copy(buf[(int)i..], b);
    {
        var (_, err) = c.fd.Write(buf);

        if (err != null) {
            return (0, error.As(err)!);
        }
    }

    return (len(b), error.As(null!)!);

}

private static (nint, nint, error) writeMsg(this ptr<UDPConn> _addr_c, slice<byte> b, slice<byte> oob, ptr<UDPAddr> _addr_addr) {
    nint n = default;
    nint oobn = default;
    error err = default!;
    ref UDPConn c = ref _addr_c.val;
    ref UDPAddr addr = ref _addr_addr.val;

    return (0, 0, error.As(syscall.EPLAN9)!);
}

private static (ptr<UDPConn>, error) dialUDP(this ptr<sysDialer> _addr_sd, context.Context ctx, ptr<UDPAddr> _addr_laddr, ptr<UDPAddr> _addr_raddr) {
    ptr<UDPConn> _p0 = default!;
    error _p0 = default!;
    ref sysDialer sd = ref _addr_sd.val;
    ref UDPAddr laddr = ref _addr_laddr.val;
    ref UDPAddr raddr = ref _addr_raddr.val;

    var (fd, err) = dialPlan9(ctx, sd.network, laddr, raddr);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    return (_addr_newUDPConn(fd)!, error.As(null!)!);

}

private static readonly nint udpHeaderSize = 16 * 3 + 2 * 2;



private partial struct udpHeader {
    public IP raddr;
    public IP laddr;
    public IP ifcaddr;
    public ushort rport;
    public ushort lport;
}

private static slice<byte> Bytes(this ptr<udpHeader> _addr_h) {
    ref udpHeader h = ref _addr_h.val;

    var b = make_slice<byte>(udpHeaderSize);
    nint i = 0;
    i += copy(b[(int)i..(int)i + 16], h.raddr);
    i += copy(b[(int)i..(int)i + 16], h.laddr);
    i += copy(b[(int)i..(int)i + 16], h.ifcaddr);
    (b[i], b[i + 1], i) = (byte(h.rport >> 8), byte(h.rport), i + 2);    (b[i], b[i + 1], i) = (byte(h.lport >> 8), byte(h.lport), i + 2);    return b;
}

private static (ptr<udpHeader>, slice<byte>) unmarshalUDPHeader(slice<byte> b) {
    ptr<udpHeader> _p0 = default!;
    slice<byte> _p0 = default;

    ptr<udpHeader> h = @new<udpHeader>();
    (h.raddr, b) = (IP(b[..(int)16]), b[(int)16..]);    (h.laddr, b) = (IP(b[..(int)16]), b[(int)16..]);    (h.ifcaddr, b) = (IP(b[..(int)16]), b[(int)16..]);    (h.rport, b) = (uint16(b[0]) << 8 | uint16(b[1]), b[(int)2..]);    (h.lport, b) = (uint16(b[0]) << 8 | uint16(b[1]), b[(int)2..]);    return (_addr_h!, b);
}

private static (ptr<UDPConn>, error) listenUDP(this ptr<sysListener> _addr_sl, context.Context ctx, ptr<UDPAddr> _addr_laddr) {
    ptr<UDPConn> _p0 = default!;
    error _p0 = default!;
    ref sysListener sl = ref _addr_sl.val;
    ref UDPAddr laddr = ref _addr_laddr.val;

    var (l, err) = listenPlan9(ctx, sl.network, laddr);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    _, err = l.ctl.WriteString("headers");
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    l.data, err = os.OpenFile(l.dir + "/data", os.O_RDWR, 0);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    var (fd, err) = l.netFD();
    return (_addr_newUDPConn(fd)!, error.As(err)!);

}

private static (ptr<UDPConn>, error) listenMulticastUDP(this ptr<sysListener> _addr_sl, context.Context ctx, ptr<Interface> _addr_ifi, ptr<UDPAddr> _addr_gaddr) {
    ptr<UDPConn> _p0 = default!;
    error _p0 = default!;
    ref sysListener sl = ref _addr_sl.val;
    ref Interface ifi = ref _addr_ifi.val;
    ref UDPAddr gaddr = ref _addr_gaddr.val;
 
    // Plan 9 does not like announce command with a multicast address,
    // so do not specify an IP address when listening.
    var (l, err) = listenPlan9(ctx, sl.network, addr(new UDPAddr(IP:nil,Port:gaddr.Port,Zone:gaddr.Zone)));
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    _, err = l.ctl.WriteString("headers");
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    slice<Addr> addrs = default;
    if (ifi != null) {
        addrs, err = ifi.Addrs();
        if (err != null) {
            return (_addr_null!, error.As(err)!);
        }
    }
    else
 {
        addrs, err = InterfaceAddrs();
        if (err != null) {
            return (_addr_null!, error.As(err)!);
        }
    }
    var have4 = gaddr.IP.To4() != null;
    foreach (var (_, addr) in addrs) {
        {
            ptr<IPNet> (ipnet, ok) = addr._<ptr<IPNet>>();

            if (ok && (ipnet.IP.To4() != null) == have4) {
                _, err = l.ctl.WriteString("addmulti " + ipnet.IP.String() + " " + gaddr.IP.String());
                if (err != null) {
                    return (_addr_null!, error.As(addr(new OpError(Op:"addmulti",Net:"",Source:nil,Addr:ipnet,Err:err))!)!);
                }
            }

        }

    }    l.data, err = os.OpenFile(l.dir + "/data", os.O_RDWR, 0);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    var (fd, err) = l.netFD();
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    return (_addr_newUDPConn(fd)!, error.As(null!)!);

}

} // end net_package
