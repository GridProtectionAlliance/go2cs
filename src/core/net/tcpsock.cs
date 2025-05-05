// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using context = context_package;
using itoa = @internal.itoa_package;
using io = io_package;
using netip = net.netip_package;
using os = os_package;
using syscall = syscall_package;
using time = time_package;
using @internal;
using net;

partial class net_package {

// BUG(mikio): On JS and Windows, the File method of TCPConn and
// TCPListener is not implemented.

// TCPAddr represents the address of a TCP end point.
[GoType] partial struct TCPAddr {
    public IP IP;
    public nint Port;
    public @string Zone; // IPv6 scoped addressing zone
}

// AddrPort returns the [TCPAddr] a as a [netip.AddrPort].
//
// If a.Port does not fit in a uint16, it's silently truncated.
//
// If a is nil, a zero value is returned.
[GoRecv] public static netip.AddrPort AddrPort(this ref TCPAddr a) {
    if (a == nil) {
        return new netip.AddrPort(nil);
    }
    var (na, _) = netip.AddrFromSlice(a.IP);
    na = na.WithZone(a.Zone);
    return netip.AddrPortFrom(na, ((uint16)a.Port));
}

// Network returns the address's network name, "tcp".
[GoRecv] public static @string Network(this ref TCPAddr a) {
    return "tcp"u8;
}

[GoRecv] public static @string String(this ref TCPAddr a) {
    if (a == nil) {
        return "<nil>"u8;
    }
    @string ip = ipEmptyString(a.IP);
    if (a.Zone != ""u8) {
        return JoinHostPort(ip + "%"u8 + a.Zone, itoa.Itoa(a.Port));
    }
    return JoinHostPort(ip, itoa.Itoa(a.Port));
}

[GoRecv] internal static bool isWildcard(this ref TCPAddr a) {
    if (a == nil || a.IP == default!) {
        return true;
    }
    return a.IP.IsUnspecified();
}

[GoRecv("capture")] internal static ΔAddr opAddr(this ref TCPAddr a) {
    if (a == nil) {
        return default!;
    }
    return ~a;
}

// ResolveTCPAddr returns an address of TCP end point.
//
// The network must be a TCP network name.
//
// If the host in the address parameter is not a literal IP address or
// the port is not a literal port number, ResolveTCPAddr resolves the
// address to an address of TCP end point.
// Otherwise, it parses the address as a pair of literal IP address
// and port number.
// The address parameter can use a host name, but this is not
// recommended, because it will return at most one of the host name's
// IP addresses.
//
// See func [Dial] for a description of the network and address
// parameters.
public static (ж<TCPAddr>, error) ResolveTCPAddr(@string network, @string address) {
    var exprᴛ1 = network;
    if (exprᴛ1 == "tcp"u8 || exprᴛ1 == "tcp4"u8 || exprᴛ1 == "tcp6"u8) {
    }
    else if (exprᴛ1 == ""u8) {
        network = "tcp"u8;
    }
    else { /* default: */
        return (default!, ((UnknownNetworkError)network));
    }

    // a hint wildcard for Go 1.0 undocumented behavior
    (addrs, err) = DefaultResolver.internetAddrList(context.Background(), network, address);
    if (err != default!) {
        return (default!, err);
    }
    return (addrs.forResolve(network, address)._<TCPAddr.val>(), default!);
}

// TCPAddrFromAddrPort returns addr as a [TCPAddr]. If addr.IsValid() is false,
// then the returned TCPAddr will contain a nil IP field, indicating an
// address family-agnostic unspecified address.
public static ж<TCPAddr> TCPAddrFromAddrPort(netip.AddrPort addr) {
    return Ꮡ(new TCPAddr(
        IP: addr.Addr().AsSlice(),
        Zone: addr.Addr().Zone(),
        Port: ((nint)addr.Port())
    ));
}

// TCPConn is an implementation of the [Conn] interface for TCP network
// connections.
[GoType] partial struct TCPConn {
    internal partial ref conn conn { get; }
}

// KeepAliveConfig contains TCP keep-alive options.
//
// If the Idle, Interval, or Count fields are zero, a default value is chosen.
// If a field is negative, the corresponding socket-level option will be left unchanged.
//
// Note that prior to Windows 10 version 1709, neither setting Idle and Interval
// separately nor changing Count (which is usually 10) is supported.
// Therefore, it's recommended to set both Idle and Interval to non-negative values
// in conjunction with a -1 for Count on those old Windows if you intend to customize
// the TCP keep-alive settings.
// By contrast, if only one of Idle and Interval is set to a non-negative value,
// the other will be set to the system default value, and ultimately,
// set both Idle and Interval to negative values if you want to leave them unchanged.
//
// Note that Solaris and its derivatives do not support setting Interval to a non-negative value
// and Count to a negative value, or vice-versa.
[GoType] partial struct KeepAliveConfig {
    // If Enable is true, keep-alive probes are enabled.
    public bool Enable;
    // Idle is the time that the connection must be idle before
    // the first keep-alive probe is sent.
    // If zero, a default value of 15 seconds is used.
    public time_package.Duration Idle;
    // Interval is the time between keep-alive probes.
    // If zero, a default value of 15 seconds is used.
    public time_package.Duration Interval;
    // Count is the maximum number of keep-alive probes that
    // can go unanswered before dropping a connection.
    // If zero, a default value of 9 is used.
    public nint Count;
}

// SyscallConn returns a raw network connection.
// This implements the [syscall.Conn] interface.
[GoRecv] public static (syscall.RawConn, error) SyscallConn(this ref TCPConn c) {
    if (!c.ok()) {
        return (default!, syscall.EINVAL);
    }
    return (~newRawConn(c.fd), default!);
}

// ReadFrom implements the [io.ReaderFrom] ReadFrom method.
[GoRecv] public static (int64, error) ReadFrom(this ref TCPConn c, io.Reader r) {
    if (!c.ok()) {
        return (0, syscall.EINVAL);
    }
    var (n, err) = c.readFrom(r);
    if (err != default! && !AreEqual(err, io.EOF)) {
        Ꮡerr = new OpError(Op: "readfrom"u8, Net: c.fd.net, Source: c.fd.laddr, ΔAddr: c.fd.raddr, Err: err); err = ref Ꮡerr.val;
    }
    return (n, err);
}

// WriteTo implements the io.WriterTo WriteTo method.
[GoRecv] public static (int64, error) WriteTo(this ref TCPConn c, io.Writer w) {
    if (!c.ok()) {
        return (0, syscall.EINVAL);
    }
    var (n, err) = c.writeTo(w);
    if (err != default! && !AreEqual(err, io.EOF)) {
        Ꮡerr = new OpError(Op: "writeto"u8, Net: c.fd.net, Source: c.fd.laddr, ΔAddr: c.fd.raddr, Err: err); err = ref Ꮡerr.val;
    }
    return (n, err);
}

// CloseRead shuts down the reading side of the TCP connection.
// Most callers should just use Close.
[GoRecv] public static error CloseRead(this ref TCPConn c) {
    if (!c.ok()) {
        return syscall.EINVAL;
    }
    {
        var err = c.fd.closeRead(); if (err != default!) {
            return new OpError(Op: "close"u8, Net: c.fd.net, Source: c.fd.laddr, ΔAddr: c.fd.raddr, Err: err);
        }
    }
    return default!;
}

// CloseWrite shuts down the writing side of the TCP connection.
// Most callers should just use Close.
[GoRecv] public static error CloseWrite(this ref TCPConn c) {
    if (!c.ok()) {
        return syscall.EINVAL;
    }
    {
        var err = c.fd.closeWrite(); if (err != default!) {
            return new OpError(Op: "close"u8, Net: c.fd.net, Source: c.fd.laddr, ΔAddr: c.fd.raddr, Err: err);
        }
    }
    return default!;
}

// SetLinger sets the behavior of Close on a connection which still
// has data waiting to be sent or to be acknowledged.
//
// If sec < 0 (the default), the operating system finishes sending the
// data in the background.
//
// If sec == 0, the operating system discards any unsent or
// unacknowledged data.
//
// If sec > 0, the data is sent in the background as with sec < 0.
// On some operating systems including Linux, this may cause Close to block
// until all data has been sent or discarded.
// On some operating systems after sec seconds have elapsed any remaining
// unsent data may be discarded.
[GoRecv] public static error SetLinger(this ref TCPConn c, nint sec) {
    if (!c.ok()) {
        return syscall.EINVAL;
    }
    {
        var err = setLinger(c.fd, sec); if (err != default!) {
            return new OpError(Op: "set"u8, Net: c.fd.net, Source: c.fd.laddr, ΔAddr: c.fd.raddr, Err: err);
        }
    }
    return default!;
}

// SetKeepAlive sets whether the operating system should send
// keep-alive messages on the connection.
[GoRecv] public static error SetKeepAlive(this ref TCPConn c, bool keepalive) {
    if (!c.ok()) {
        return syscall.EINVAL;
    }
    {
        var err = setKeepAlive(c.fd, keepalive); if (err != default!) {
            return new OpError(Op: "set"u8, Net: c.fd.net, Source: c.fd.laddr, ΔAddr: c.fd.raddr, Err: err);
        }
    }
    return default!;
}

// SetKeepAlivePeriod sets the duration the connection needs to
// remain idle before TCP starts sending keepalive probes.
//
// Note that calling this method on Windows prior to Windows 10 version 1709
// will reset the KeepAliveInterval to the default system value, which is normally 1 second.
[GoRecv] public static error SetKeepAlivePeriod(this ref TCPConn c, time.Duration d) {
    if (!c.ok()) {
        return syscall.EINVAL;
    }
    {
        var err = setKeepAliveIdle(c.fd, d); if (err != default!) {
            return new OpError(Op: "set"u8, Net: c.fd.net, Source: c.fd.laddr, ΔAddr: c.fd.raddr, Err: err);
        }
    }
    return default!;
}

// SetNoDelay controls whether the operating system should delay
// packet transmission in hopes of sending fewer packets (Nagle's
// algorithm).  The default is true (no delay), meaning that data is
// sent as soon as possible after a Write.
[GoRecv] public static error SetNoDelay(this ref TCPConn c, bool noDelay) {
    if (!c.ok()) {
        return syscall.EINVAL;
    }
    {
        var err = setNoDelay(c.fd, noDelay); if (err != default!) {
            return new OpError(Op: "set"u8, Net: c.fd.net, Source: c.fd.laddr, ΔAddr: c.fd.raddr, Err: err);
        }
    }
    return default!;
}

// MultipathTCP reports whether the ongoing connection is using MPTCP.
//
// If Multipath TCP is not supported by the host, by the other peer or
// intentionally / accidentally filtered out by a device in between, a
// fallback to TCP will be done. This method does its best to check if
// MPTCP is still being used or not.
//
// On Linux, more conditions are verified on kernels >= v5.16, improving
// the results.
[GoRecv] public static (bool, error) MultipathTCP(this ref TCPConn c) {
    if (!c.ok()) {
        return (false, syscall.EINVAL);
    }
    return (isUsingMultipathTCP(c.fd), default!);
}

internal static ж<TCPConn> newTCPConn(ж<netFD> Ꮡfd, time.Duration keepAliveIdle, KeepAliveConfig keepAliveCfg, Action<ж<netFD>> preKeepAliveHook, Action<KeepAliveConfig> keepAliveHook) {
    ref var fd = ref Ꮡfd.val;

    setNoDelay(Ꮡfd, true);
    if (!keepAliveCfg.Enable && keepAliveIdle >= 0) {
        keepAliveCfg = new KeepAliveConfig(
            Enable: true,
            Idle: keepAliveIdle
        );
    }
    var c = Ꮡ(new TCPConn(new conn(Ꮡfd)));
    if (keepAliveCfg.Enable) {
        if (preKeepAliveHook != default!) {
            preKeepAliveHook(Ꮡfd);
        }
        c.SetKeepAliveConfig(keepAliveCfg);
        if (keepAliveHook != default!) {
            keepAliveHook(keepAliveCfg);
        }
    }
    return c;
}

// DialTCP acts like [Dial] for TCP networks.
//
// The network must be a TCP network name; see func Dial for details.
//
// If laddr is nil, a local address is automatically chosen.
// If the IP field of raddr is nil or an unspecified IP address, the
// local system is assumed.
public static (ж<TCPConn>, error) DialTCP(@string network, ж<TCPAddr> Ꮡladdr, ж<TCPAddr> Ꮡraddr) {
    ref var laddr = ref Ꮡladdr.val;
    ref var raddr = ref Ꮡraddr.val;

    var exprᴛ1 = network;
    if (exprᴛ1 == "tcp"u8 || exprᴛ1 == "tcp4"u8 || exprᴛ1 == "tcp6"u8) {
    }
    else { /* default: */
        return (default!, new OpError(Op: "dial"u8, Net: network, Source: laddr.opAddr(), ΔAddr: raddr.opAddr(), Err: ((UnknownNetworkError)network)));
    }

    if (raddr == nil) {
        return (default!, new OpError(Op: "dial"u8, Net: network, Source: laddr.opAddr(), ΔAddr: default!, Err: errMissingAddress));
    }
    var sd = Ꮡ(new sysDialer(network: network, address: raddr.String()));
    (c, err) = sd.dialTCP(context.Background(), Ꮡladdr, Ꮡraddr);
    if (err != default!) {
        return (default!, new OpError(Op: "dial"u8, Net: network, Source: laddr.opAddr(), ΔAddr: raddr.opAddr(), Err: err));
    }
    return (c, default!);
}

// TCPListener is a TCP network listener. Clients should typically
// use variables of type [Listener] instead of assuming TCP.
[GoType] partial struct TCPListener {
    internal ж<netFD> fd;
    internal ListenConfig lc;
}

// SyscallConn returns a raw network connection.
// This implements the [syscall.Conn] interface.
//
// The returned RawConn only supports calling Control. Read and
// Write return an error.
[GoRecv] public static (syscall.RawConn, error) SyscallConn(this ref TCPListener l) {
    if (!l.ok()) {
        return (default!, syscall.EINVAL);
    }
    return (~newRawListener(l.fd), default!);
}

// AcceptTCP accepts the next incoming call and returns the new
// connection.
[GoRecv] public static (ж<TCPConn>, error) AcceptTCP(this ref TCPListener l) {
    if (!l.ok()) {
        return (default!, syscall.EINVAL);
    }
    (c, err) = l.accept();
    if (err != default!) {
        return (default!, new OpError(Op: "accept"u8, Net: l.fd.net, Source: default!, ΔAddr: l.fd.laddr, Err: err));
    }
    return (c, default!);
}

// Accept implements the Accept method in the [Listener] interface; it
// waits for the next call and returns a generic [Conn].
[GoRecv] public static (Conn, error) Accept(this ref TCPListener l) {
    if (!l.ok()) {
        return (default!, syscall.EINVAL);
    }
    (c, err) = l.accept();
    if (err != default!) {
        return (default!, new OpError(Op: "accept"u8, Net: l.fd.net, Source: default!, ΔAddr: l.fd.laddr, Err: err));
    }
    return (~c, default!);
}

// Close stops listening on the TCP address.
// Already Accepted connections are not closed.
[GoRecv] public static error Close(this ref TCPListener l) {
    if (!l.ok()) {
        return syscall.EINVAL;
    }
    {
        var err = l.close(); if (err != default!) {
            return new OpError(Op: "close"u8, Net: l.fd.net, Source: default!, ΔAddr: l.fd.laddr, Err: err);
        }
    }
    return default!;
}

// Addr returns the listener's network address, a [*TCPAddr].
// The Addr returned is shared by all invocations of Addr, so
// do not modify it.
[GoRecv] public static ΔAddr Addr(this ref TCPListener l) {
    return l.fd.laddr;
}

// SetDeadline sets the deadline associated with the listener.
// A zero time value disables the deadline.
[GoRecv] public static error SetDeadline(this ref TCPListener l, time.Time t) {
    if (!l.ok()) {
        return syscall.EINVAL;
    }
    return l.fd.SetDeadline(t);
}

// File returns a copy of the underlying [os.File].
// It is the caller's responsibility to close f when finished.
// Closing l does not affect f, and closing f does not affect l.
//
// The returned os.File's file descriptor is different from the
// connection's. Attempting to change properties of the original
// using this duplicate may or may not have the desired effect.
[GoRecv] public static (ж<os.File> f, error err) File(this ref TCPListener l) {
    ж<os.File> f = default!;
    error err = default!;

    if (!l.ok()) {
        return (default!, syscall.EINVAL);
    }
    (f, err) = l.file();
    if (err != default!) {
        return (default!, new OpError(Op: "file"u8, Net: l.fd.net, Source: default!, ΔAddr: l.fd.laddr, Err: err));
    }
    return (f, err);
}

// ListenTCP acts like [Listen] for TCP networks.
//
// The network must be a TCP network name; see func Dial for details.
//
// If the IP field of laddr is nil or an unspecified IP address,
// ListenTCP listens on all available unicast and anycast IP addresses
// of the local system.
// If the Port field of laddr is 0, a port number is automatically
// chosen.
public static (ж<TCPListener>, error) ListenTCP(@string network, ж<TCPAddr> Ꮡladdr) {
    ref var laddr = ref Ꮡladdr.val;

    var exprᴛ1 = network;
    if (exprᴛ1 == "tcp"u8 || exprᴛ1 == "tcp4"u8 || exprᴛ1 == "tcp6"u8) {
    }
    else { /* default: */
        return (default!, new OpError(Op: "listen"u8, Net: network, Source: default!, ΔAddr: laddr.opAddr(), Err: ((UnknownNetworkError)network)));
    }

    if (laddr == nil) {
        Ꮡladdr = Ꮡ(new TCPAddr(nil)); laddr = ref Ꮡladdr.val;
    }
    var sl = Ꮡ(new sysListener(network: network, address: laddr.String()));
    (ln, err) = sl.listenTCP(context.Background(), Ꮡladdr);
    if (err != default!) {
        return (default!, new OpError(Op: "listen"u8, Net: network, Source: default!, ΔAddr: laddr.opAddr(), Err: err));
    }
    return (ln, default!);
}

// roundDurationUp rounds d to the next multiple of to.
internal static time.Duration roundDurationUp(time.Duration d, time.Duration to) {
    return (d + to - 1) / to;
}

} // end net_package
