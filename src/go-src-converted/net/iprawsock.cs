// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package net -- go2cs converted at 2022 March 06 22:16:11 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Program Files\Go\src\net\iprawsock.go
using context = go.context_package;
using syscall = go.syscall_package;

namespace go;

public static partial class net_package {

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

    // BUG(mikio): On JS and Plan 9, methods and functions related
    // to IPConn are not implemented.

    // BUG(mikio): On Windows, the File method of IPConn is not
    // implemented.

    // IPAddr represents the address of an IP end point.
public partial struct IPAddr {
    public IP IP;
    public @string Zone; // IPv6 scoped addressing zone
}

// Network returns the address's network name, "ip".
private static @string Network(this ptr<IPAddr> _addr_a) {
    ref IPAddr a = ref _addr_a.val;

    return "ip";
}

private static @string String(this ptr<IPAddr> _addr_a) {
    ref IPAddr a = ref _addr_a.val;

    if (a == null) {
        return "<nil>";
    }
    var ip = ipEmptyString(a.IP);
    if (a.Zone != "") {
        return ip + "%" + a.Zone;
    }
    return ip;

}

private static bool isWildcard(this ptr<IPAddr> _addr_a) {
    ref IPAddr a = ref _addr_a.val;

    if (a == null || a.IP == null) {
        return true;
    }
    return a.IP.IsUnspecified();

}

private static Addr opAddr(this ptr<IPAddr> _addr_a) {
    ref IPAddr a = ref _addr_a.val;

    if (a == null) {
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
public static (ptr<IPAddr>, error) ResolveIPAddr(@string network, @string address) {
    ptr<IPAddr> _p0 = default!;
    error _p0 = default!;

    if (network == "") { // a hint wildcard for Go 1.0 undocumented behavior
        network = "ip";

    }
    var (afnet, _, err) = parseNetwork(context.Background(), network, false);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    switch (afnet) {
        case "ip": 

        case "ip4": 

        case "ip6": 

            break;
        default: 
            return (_addr_null!, error.As(UnknownNetworkError(network))!);
            break;
    }
    var (addrs, err) = DefaultResolver.internetAddrList(context.Background(), afnet, address);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    return (addrs.forResolve(network, address)._<ptr<IPAddr>>(), error.As(null!)!);

}

// IPConn is the implementation of the Conn and PacketConn interfaces
// for IP network connections.
public partial struct IPConn {
    public ref conn conn => ref conn_val;
}

// SyscallConn returns a raw network connection.
// This implements the syscall.Conn interface.
private static (syscall.RawConn, error) SyscallConn(this ptr<IPConn> _addr_c) {
    syscall.RawConn _p0 = default;
    error _p0 = default!;
    ref IPConn c = ref _addr_c.val;

    if (!c.ok()) {
        return (null, error.As(syscall.EINVAL)!);
    }
    return newRawConn(c.fd);

}

// ReadFromIP acts like ReadFrom but returns an IPAddr.
private static (nint, ptr<IPAddr>, error) ReadFromIP(this ptr<IPConn> _addr_c, slice<byte> b) {
    nint _p0 = default;
    ptr<IPAddr> _p0 = default!;
    error _p0 = default!;
    ref IPConn c = ref _addr_c.val;

    if (!c.ok()) {
        return (0, _addr_null!, error.As(syscall.EINVAL)!);
    }
    var (n, addr, err) = c.readFrom(b);
    if (err != null) {
        err = addr(new OpError(Op:"read",Net:c.fd.net,Source:c.fd.laddr,Addr:c.fd.raddr,Err:err));
    }
    return (n, _addr_addr!, error.As(err)!);

}

// ReadFrom implements the PacketConn ReadFrom method.
private static (nint, Addr, error) ReadFrom(this ptr<IPConn> _addr_c, slice<byte> b) {
    nint _p0 = default;
    Addr _p0 = default;
    error _p0 = default!;
    ref IPConn c = ref _addr_c.val;

    if (!c.ok()) {
        return (0, null, error.As(syscall.EINVAL)!);
    }
    var (n, addr, err) = c.readFrom(b);
    if (err != null) {
        err = addr(new OpError(Op:"read",Net:c.fd.net,Source:c.fd.laddr,Addr:c.fd.raddr,Err:err));
    }
    if (addr == null) {
        return (n, null, error.As(err)!);
    }
    return (n, addr, error.As(err)!);

}

// ReadMsgIP reads a message from c, copying the payload into b and
// the associated out-of-band data into oob. It returns the number of
// bytes copied into b, the number of bytes copied into oob, the flags
// that were set on the message and the source address of the message.
//
// The packages golang.org/x/net/ipv4 and golang.org/x/net/ipv6 can be
// used to manipulate IP-level socket options in oob.
private static (nint, nint, nint, ptr<IPAddr>, error) ReadMsgIP(this ptr<IPConn> _addr_c, slice<byte> b, slice<byte> oob) {
    nint n = default;
    nint oobn = default;
    nint flags = default;
    ptr<IPAddr> addr = default!;
    error err = default!;
    ref IPConn c = ref _addr_c.val;

    if (!c.ok()) {
        return (0, 0, 0, _addr_null!, error.As(syscall.EINVAL)!);
    }
    n, oobn, flags, addr, err = c.readMsg(b, oob);
    if (err != null) {
        err = addr(new OpError(Op:"read",Net:c.fd.net,Source:c.fd.laddr,Addr:c.fd.raddr,Err:err));
    }
    return ;

}

// WriteToIP acts like WriteTo but takes an IPAddr.
private static (nint, error) WriteToIP(this ptr<IPConn> _addr_c, slice<byte> b, ptr<IPAddr> _addr_addr) {
    nint _p0 = default;
    error _p0 = default!;
    ref IPConn c = ref _addr_c.val;
    ref IPAddr addr = ref _addr_addr.val;

    if (!c.ok()) {
        return (0, error.As(syscall.EINVAL)!);
    }
    var (n, err) = c.writeTo(b, addr);
    if (err != null) {
        err = addr(new OpError(Op:"write",Net:c.fd.net,Source:c.fd.laddr,Addr:addr.opAddr(),Err:err));
    }
    return (n, error.As(err)!);

}

// WriteTo implements the PacketConn WriteTo method.
private static (nint, error) WriteTo(this ptr<IPConn> _addr_c, slice<byte> b, Addr addr) {
    nint _p0 = default;
    error _p0 = default!;
    ref IPConn c = ref _addr_c.val;

    if (!c.ok()) {
        return (0, error.As(syscall.EINVAL)!);
    }
    ptr<IPAddr> (a, ok) = addr._<ptr<IPAddr>>();
    if (!ok) {
        return (0, error.As(addr(new OpError(Op:"write",Net:c.fd.net,Source:c.fd.laddr,Addr:addr,Err:syscall.EINVAL))!)!);
    }
    var (n, err) = c.writeTo(b, a);
    if (err != null) {
        err = addr(new OpError(Op:"write",Net:c.fd.net,Source:c.fd.laddr,Addr:a.opAddr(),Err:err));
    }
    return (n, error.As(err)!);

}

// WriteMsgIP writes a message to addr via c, copying the payload from
// b and the associated out-of-band data from oob. It returns the
// number of payload and out-of-band bytes written.
//
// The packages golang.org/x/net/ipv4 and golang.org/x/net/ipv6 can be
// used to manipulate IP-level socket options in oob.
private static (nint, nint, error) WriteMsgIP(this ptr<IPConn> _addr_c, slice<byte> b, slice<byte> oob, ptr<IPAddr> _addr_addr) {
    nint n = default;
    nint oobn = default;
    error err = default!;
    ref IPConn c = ref _addr_c.val;
    ref IPAddr addr = ref _addr_addr.val;

    if (!c.ok()) {
        return (0, 0, error.As(syscall.EINVAL)!);
    }
    n, oobn, err = c.writeMsg(b, oob, addr);
    if (err != null) {
        err = addr(new OpError(Op:"write",Net:c.fd.net,Source:c.fd.laddr,Addr:addr.opAddr(),Err:err));
    }
    return ;

}

private static ptr<IPConn> newIPConn(ptr<netFD> _addr_fd) {
    ref netFD fd = ref _addr_fd.val;

    return addr(new IPConn(conn{fd}));
}

// DialIP acts like Dial for IP networks.
//
// The network must be an IP network name; see func Dial for details.
//
// If laddr is nil, a local address is automatically chosen.
// If the IP field of raddr is nil or an unspecified IP address, the
// local system is assumed.
public static (ptr<IPConn>, error) DialIP(@string network, ptr<IPAddr> _addr_laddr, ptr<IPAddr> _addr_raddr) {
    ptr<IPConn> _p0 = default!;
    error _p0 = default!;
    ref IPAddr laddr = ref _addr_laddr.val;
    ref IPAddr raddr = ref _addr_raddr.val;

    if (raddr == null) {
        return (_addr_null!, error.As(addr(new OpError(Op:"dial",Net:network,Source:laddr.opAddr(),Addr:nil,Err:errMissingAddress))!)!);
    }
    ptr<sysDialer> sd = addr(new sysDialer(network:network,address:raddr.String()));
    var (c, err) = sd.dialIP(context.Background(), laddr, raddr);
    if (err != null) {
        return (_addr_null!, error.As(addr(new OpError(Op:"dial",Net:network,Source:laddr.opAddr(),Addr:raddr.opAddr(),Err:err))!)!);
    }
    return (_addr_c!, error.As(null!)!);

}

// ListenIP acts like ListenPacket for IP networks.
//
// The network must be an IP network name; see func Dial for details.
//
// If the IP field of laddr is nil or an unspecified IP address,
// ListenIP listens on all available IP addresses of the local system
// except multicast IP addresses.
public static (ptr<IPConn>, error) ListenIP(@string network, ptr<IPAddr> _addr_laddr) {
    ptr<IPConn> _p0 = default!;
    error _p0 = default!;
    ref IPAddr laddr = ref _addr_laddr.val;

    if (laddr == null) {
        laddr = addr(new IPAddr());
    }
    ptr<sysListener> sl = addr(new sysListener(network:network,address:laddr.String()));
    var (c, err) = sl.listenIP(context.Background(), laddr);
    if (err != null) {
        return (_addr_null!, error.As(addr(new OpError(Op:"listen",Net:network,Source:nil,Addr:laddr.opAddr(),Err:err))!)!);
    }
    return (_addr_c!, error.As(null!)!);

}

} // end net_package
