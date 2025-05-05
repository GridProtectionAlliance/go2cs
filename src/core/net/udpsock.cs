// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using context = context_package;
using itoa = @internal.itoa_package;
using netip = net.netip_package;
using syscall = syscall_package;
using @internal;
using net;

partial class net_package {

// BUG(mikio): On Plan 9, the ReadMsgUDP and
// WriteMsgUDP methods of UDPConn are not implemented.
// BUG(mikio): On Windows, the File method of UDPConn is not
// implemented.
// BUG(mikio): On JS, methods and functions related to UDPConn are not
// implemented.

// UDPAddr represents the address of a UDP end point.
[GoType] partial struct UDPAddr {
    public IP IP;
    public nint Port;
    public @string Zone; // IPv6 scoped addressing zone
}

// AddrPort returns the UDPAddr a as a netip.AddrPort.
//
// If a.Port does not fit in a uint16, it's silently truncated.
//
// If a is nil, a zero value is returned.
[GoRecv] public static netip.AddrPort AddrPort(this ref UDPAddr a) {
    if (a == nil) {
        return new netip.AddrPort(nil);
    }
    var (na, _) = netip.AddrFromSlice(a.IP);
    na = na.WithZone(a.Zone);
    return netip.AddrPortFrom(na, ((uint16)a.Port));
}

// Network returns the address's network name, "udp".
[GoRecv] public static @string Network(this ref UDPAddr a) {
    return "udp"u8;
}

[GoRecv] public static @string String(this ref UDPAddr a) {
    if (a == nil) {
        return "<nil>"u8;
    }
    @string ip = ipEmptyString(a.IP);
    if (a.Zone != ""u8) {
        return JoinHostPort(ip + "%"u8 + a.Zone, itoa.Itoa(a.Port));
    }
    return JoinHostPort(ip, itoa.Itoa(a.Port));
}

[GoRecv] internal static bool isWildcard(this ref UDPAddr a) {
    if (a == nil || a.IP == default!) {
        return true;
    }
    return a.IP.IsUnspecified();
}

[GoRecv("capture")] internal static ΔAddr opAddr(this ref UDPAddr a) {
    if (a == nil) {
        return default!;
    }
    return ~a;
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
public static (ж<UDPAddr>, error) ResolveUDPAddr(@string network, @string address) {
    var exprᴛ1 = network;
    if (exprᴛ1 == "udp"u8 || exprᴛ1 == "udp4"u8 || exprᴛ1 == "udp6"u8) {
    }
    else if (exprᴛ1 == ""u8) {
        network = "udp"u8;
    }
    else { /* default: */
        return (default!, ((UnknownNetworkError)network));
    }

    // a hint wildcard for Go 1.0 undocumented behavior
    (addrs, err) = DefaultResolver.internetAddrList(context.Background(), network, address);
    if (err != default!) {
        return (default!, err);
    }
    return (addrs.forResolve(network, address)._<UDPAddr.val>(), default!);
}

// UDPAddrFromAddrPort returns addr as a UDPAddr. If addr.IsValid() is false,
// then the returned UDPAddr will contain a nil IP field, indicating an
// address family-agnostic unspecified address.
public static ж<UDPAddr> UDPAddrFromAddrPort(netip.AddrPort addr) {
    return Ꮡ(new UDPAddr(
        IP: addr.Addr().AsSlice(),
        Zone: addr.Addr().Zone(),
        Port: ((nint)addr.Port())
    ));
}

// An addrPortUDPAddr is a netip.AddrPort-based UDP address that satisfies the Addr interface.
[GoType] partial struct addrPortUDPAddr {
    public partial ref net.netip_package.AddrPort AddrPort { get; }
}

internal static @string Network(this addrPortUDPAddr _) {
    return "udp"u8;
}

// UDPConn is the implementation of the Conn and PacketConn interfaces
// for UDP network connections.
[GoType] partial struct UDPConn {
    internal partial ref conn conn { get; }
}

// SyscallConn returns a raw network connection.
// This implements the syscall.Conn interface.
[GoRecv] public static (syscall.RawConn, error) SyscallConn(this ref UDPConn c) {
    if (!c.ok()) {
        return (default!, syscall.EINVAL);
    }
    return (~newRawConn(c.fd), default!);
}

// ReadFromUDP acts like ReadFrom but returns a UDPAddr.
[GoRecv] public static (nint n, ж<UDPAddr> addr, error err) ReadFromUDP(this ref UDPConn c, slice<byte> b) {
    nint n = default!;
    ж<UDPAddr> addr = default!;
    error err = default!;

    // This function is designed to allow the caller to control the lifetime
    // of the returned *UDPAddr and thereby prevent an allocation.
    // See https://blog.filippo.io/efficient-go-apis-with-the-inliner/.
    // The real work is done by readFromUDP, below.
    return c.readFromUDP(b, Ꮡ(new UDPAddr(nil)));
}

// readFromUDP implements ReadFromUDP.
[GoRecv] public static (nint, ж<UDPAddr>, error) readFromUDP(this ref UDPConn c, slice<byte> b, ж<UDPAddr> Ꮡaddr) {
    ref var addr = ref Ꮡaddr.val;

    if (!c.ok()) {
        return (0, default!, syscall.EINVAL);
    }
    var (n, addr, err) = c.readFrom(b, Ꮡaddr);
    if (err != default!) {
        Ꮡerr = new OpError(Op: "read"u8, Net: c.fd.net, Source: c.fd.laddr, ΔAddr: c.fd.raddr, Err: err); err = ref Ꮡerr.val;
    }
    return (n, Ꮡaddr, err);
}

// ReadFrom implements the PacketConn ReadFrom method.
[GoRecv] public static (nint, ΔAddr, error) ReadFrom(this ref UDPConn c, slice<byte> b) {
    var (n, addr, err) = c.readFromUDP(b, Ꮡ(new UDPAddr(nil)));
    if (addr == nil) {
        // Return Addr(nil), not Addr(*UDPConn(nil)).
        return (n, default!, err);
    }
    return (n, ~addr, err);
}

// ReadFromUDPAddrPort acts like ReadFrom but returns a netip.AddrPort.
//
// If c is bound to an unspecified address, the returned
// netip.AddrPort's address might be an IPv4-mapped IPv6 address.
// Use netip.Addr.Unmap to get the address without the IPv6 prefix.
[GoRecv] public static (nint n, netip.AddrPort addr, error err) ReadFromUDPAddrPort(this ref UDPConn c, slice<byte> b) {
    nint n = default!;
    netip.AddrPort addr = default!;
    error err = default!;

    if (!c.ok()) {
        return (0, new netip.AddrPort(nil), syscall.EINVAL);
    }
    (n, addr, err) = c.readFromAddrPort(b);
    if (err != default!) {
        Ꮡerr = new OpError(Op: "read"u8, Net: c.fd.net, Source: c.fd.laddr, ΔAddr: c.fd.raddr, Err: err); err = ref Ꮡerr.val;
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
[GoRecv] public static (nint n, nint oobn, nint flags, ж<UDPAddr> addr, error err) ReadMsgUDP(this ref UDPConn c, slice<byte> b, slice<byte> oob) {
    nint n = default!;
    nint oobn = default!;
    nint flags = default!;
    ж<UDPAddr> addr = default!;
    error err = default!;

    netip.AddrPort ap = default!;
    (n, oobn, flags, ap, err) = c.ReadMsgUDPAddrPort(b, oob);
    if (ap.IsValid()) {
        addr = UDPAddrFromAddrPort(ap);
    }
    return (n, oobn, flags, addr, err);
}

// ReadMsgUDPAddrPort is like ReadMsgUDP but returns an netip.AddrPort instead of a UDPAddr.
[GoRecv] public static (nint n, nint oobn, nint flags, netip.AddrPort addr, error err) ReadMsgUDPAddrPort(this ref UDPConn c, slice<byte> b, slice<byte> oob) {
    nint n = default!;
    nint oobn = default!;
    nint flags = default!;
    netip.AddrPort addr = default!;
    error err = default!;

    if (!c.ok()) {
        return (0, 0, 0, new netip.AddrPort(nil), syscall.EINVAL);
    }
    (n, oobn, flags, addr, err) = c.readMsg(b, oob);
    if (err != default!) {
        Ꮡerr = new OpError(Op: "read"u8, Net: c.fd.net, Source: c.fd.laddr, ΔAddr: c.fd.raddr, Err: err); err = ref Ꮡerr.val;
    }
    return (n, oobn, flags, addr, err);
}

// WriteToUDP acts like WriteTo but takes a UDPAddr.
[GoRecv] public static (nint, error) WriteToUDP(this ref UDPConn c, slice<byte> b, ж<UDPAddr> Ꮡaddr) {
    ref var addr = ref Ꮡaddr.val;

    if (!c.ok()) {
        return (0, syscall.EINVAL);
    }
    var (n, err) = c.writeTo(b, Ꮡaddr);
    if (err != default!) {
        Ꮡerr = new OpError(Op: "write"u8, Net: c.fd.net, Source: c.fd.laddr, ΔAddr: addr.opAddr(), Err: err); err = ref Ꮡerr.val;
    }
    return (n, err);
}

// WriteToUDPAddrPort acts like WriteTo but takes a netip.AddrPort.
[GoRecv] public static (nint, error) WriteToUDPAddrPort(this ref UDPConn c, slice<byte> b, netip.AddrPort addr) {
    if (!c.ok()) {
        return (0, syscall.EINVAL);
    }
    var (n, err) = c.writeToAddrPort(b, addr);
    if (err != default!) {
        Ꮡerr = new OpError(Op: "write"u8, Net: c.fd.net, Source: c.fd.laddr, ΔAddr: new addrPortUDPAddr(addr), Err: err); err = ref Ꮡerr.val;
    }
    return (n, err);
}

// WriteTo implements the PacketConn WriteTo method.
[GoRecv] public static (nint, error) WriteTo(this ref UDPConn c, slice<byte> b, ΔAddr addr) {
    if (!c.ok()) {
        return (0, syscall.EINVAL);
    }
    var (a, ok) = addr._<UDPAddr.val>(ᐧ);
    if (!ok) {
        return (0, new OpError(Op: "write"u8, Net: c.fd.net, Source: c.fd.laddr, ΔAddr: addr, Err: syscall.EINVAL));
    }
    var (n, err) = c.writeTo(b, a);
    if (err != default!) {
        Ꮡerr = new OpError(Op: "write"u8, Net: c.fd.net, Source: c.fd.laddr, ΔAddr: a.opAddr(), Err: err); err = ref Ꮡerr.val;
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
[GoRecv] public static (nint n, nint oobn, error err) WriteMsgUDP(this ref UDPConn c, slice<byte> b, slice<byte> oob, ж<UDPAddr> Ꮡaddr) {
    nint n = default!;
    nint oobn = default!;
    error err = default!;

    ref var addr = ref Ꮡaddr.val;
    if (!c.ok()) {
        return (0, 0, syscall.EINVAL);
    }
    (n, oobn, err) = c.writeMsg(b, oob, Ꮡaddr);
    if (err != default!) {
        Ꮡerr = new OpError(Op: "write"u8, Net: c.fd.net, Source: c.fd.laddr, ΔAddr: addr.opAddr(), Err: err); err = ref Ꮡerr.val;
    }
    return (n, oobn, err);
}

// WriteMsgUDPAddrPort is like WriteMsgUDP but takes a netip.AddrPort instead of a UDPAddr.
[GoRecv] public static (nint n, nint oobn, error err) WriteMsgUDPAddrPort(this ref UDPConn c, slice<byte> b, slice<byte> oob, netip.AddrPort addr) {
    nint n = default!;
    nint oobn = default!;
    error err = default!;

    if (!c.ok()) {
        return (0, 0, syscall.EINVAL);
    }
    (n, oobn, err) = c.writeMsgAddrPort(b, oob, addr);
    if (err != default!) {
        Ꮡerr = new OpError(Op: "write"u8, Net: c.fd.net, Source: c.fd.laddr, ΔAddr: new addrPortUDPAddr(addr), Err: err); err = ref Ꮡerr.val;
    }
    return (n, oobn, err);
}

internal static ж<UDPConn> newUDPConn(ж<netFD> Ꮡfd) {
    ref var fd = ref Ꮡfd.val;

    return Ꮡ(new UDPConn(new conn(Ꮡfd)));
}

// DialUDP acts like Dial for UDP networks.
//
// The network must be a UDP network name; see func Dial for details.
//
// If laddr is nil, a local address is automatically chosen.
// If the IP field of raddr is nil or an unspecified IP address, the
// local system is assumed.
public static (ж<UDPConn>, error) DialUDP(@string network, ж<UDPAddr> Ꮡladdr, ж<UDPAddr> Ꮡraddr) {
    ref var laddr = ref Ꮡladdr.val;
    ref var raddr = ref Ꮡraddr.val;

    var exprᴛ1 = network;
    if (exprᴛ1 == "udp"u8 || exprᴛ1 == "udp4"u8 || exprᴛ1 == "udp6"u8) {
    }
    else { /* default: */
        return (default!, new OpError(Op: "dial"u8, Net: network, Source: laddr.opAddr(), ΔAddr: raddr.opAddr(), Err: ((UnknownNetworkError)network)));
    }

    if (raddr == nil) {
        return (default!, new OpError(Op: "dial"u8, Net: network, Source: laddr.opAddr(), ΔAddr: default!, Err: errMissingAddress));
    }
    var sd = Ꮡ(new sysDialer(network: network, address: raddr.String()));
    (c, err) = sd.dialUDP(context.Background(), Ꮡladdr, Ꮡraddr);
    if (err != default!) {
        return (default!, new OpError(Op: "dial"u8, Net: network, Source: laddr.opAddr(), ΔAddr: raddr.opAddr(), Err: err));
    }
    return (c, default!);
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
public static (ж<UDPConn>, error) ListenUDP(@string network, ж<UDPAddr> Ꮡladdr) {
    ref var laddr = ref Ꮡladdr.val;

    var exprᴛ1 = network;
    if (exprᴛ1 == "udp"u8 || exprᴛ1 == "udp4"u8 || exprᴛ1 == "udp6"u8) {
    }
    else { /* default: */
        return (default!, new OpError(Op: "listen"u8, Net: network, Source: default!, ΔAddr: laddr.opAddr(), Err: ((UnknownNetworkError)network)));
    }

    if (laddr == nil) {
        Ꮡladdr = Ꮡ(new UDPAddr(nil)); laddr = ref Ꮡladdr.val;
    }
    var sl = Ꮡ(new sysListener(network: network, address: laddr.String()));
    (c, err) = sl.listenUDP(context.Background(), Ꮡladdr);
    if (err != default!) {
        return (default!, new OpError(Op: "listen"u8, Net: network, Source: default!, ΔAddr: laddr.opAddr(), Err: err));
    }
    return (c, default!);
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
//
// Note that ListenMulticastUDP will set the IP_MULTICAST_LOOP socket option
// to 0 under IPPROTO_IP, to disable loopback of multicast packets.
public static (ж<UDPConn>, error) ListenMulticastUDP(@string network, ж<Interface> Ꮡifi, ж<UDPAddr> Ꮡgaddr) {
    ref var ifi = ref Ꮡifi.val;
    ref var gaddr = ref Ꮡgaddr.val;

    var exprᴛ1 = network;
    if (exprᴛ1 == "udp"u8 || exprᴛ1 == "udp4"u8 || exprᴛ1 == "udp6"u8) {
    }
    else { /* default: */
        return (default!, new OpError(Op: "listen"u8, Net: network, Source: default!, ΔAddr: gaddr.opAddr(), Err: ((UnknownNetworkError)network)));
    }

    if (gaddr == nil || gaddr.IP == default!) {
        return (default!, new OpError(Op: "listen"u8, Net: network, Source: default!, ΔAddr: gaddr.opAddr(), Err: errMissingAddress));
    }
    var sl = Ꮡ(new sysListener(network: network, address: gaddr.String()));
    (c, err) = sl.listenMulticastUDP(context.Background(), Ꮡifi, Ꮡgaddr);
    if (err != default!) {
        return (default!, new OpError(Op: "listen"u8, Net: network, Source: default!, ΔAddr: gaddr.opAddr(), Err: err));
    }
    return (c, default!);
}

} // end net_package
