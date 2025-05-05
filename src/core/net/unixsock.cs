// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using context = context_package;
using os = os_package;
using sync = sync_package;
using syscall = syscall_package;
using time = time_package;

partial class net_package {

// BUG(mikio): On JS, WASIP1 and Plan 9, methods and functions related
// to UnixConn and UnixListener are not implemented.
// BUG(mikio): On Windows, methods and functions related to UnixConn
// and UnixListener don't work for "unixgram" and "unixpacket".

// UnixAddr represents the address of a Unix domain socket end point.
[GoType] partial struct UnixAddr {
    public @string Name;
    public @string Net;
}

// Network returns the address's network name, "unix", "unixgram" or
// "unixpacket".
[GoRecv] public static @string Network(this ref UnixAddr a) {
    return a.Net;
}

[GoRecv] public static @string String(this ref UnixAddr a) {
    if (a == nil) {
        return "<nil>"u8;
    }
    return a.Name;
}

[GoRecv] internal static bool isWildcard(this ref UnixAddr a) {
    return a == nil || a.Name == ""u8;
}

[GoRecv("capture")] internal static ΔAddr opAddr(this ref UnixAddr a) {
    if (a == nil) {
        return default!;
    }
    return ~a;
}

// ResolveUnixAddr returns an address of Unix domain socket end point.
//
// The network must be a Unix network name.
//
// See func [Dial] for a description of the network and address
// parameters.
public static (ж<UnixAddr>, error) ResolveUnixAddr(@string network, @string address) {
    var exprᴛ1 = network;
    if (exprᴛ1 == "unix"u8 || exprᴛ1 == "unixgram"u8 || exprᴛ1 == "unixpacket"u8) {
        return (Ꮡ(new UnixAddr(Name: address, Net: network)), default!);
    }
    { /* default: */
        return (default!, ((UnknownNetworkError)network));
    }

}

// UnixConn is an implementation of the [Conn] interface for connections
// to Unix domain sockets.
[GoType] partial struct UnixConn {
    internal partial ref conn conn { get; }
}

// SyscallConn returns a raw network connection.
// This implements the [syscall.Conn] interface.
[GoRecv] public static (syscall.RawConn, error) SyscallConn(this ref UnixConn c) {
    if (!c.ok()) {
        return (default!, syscall.EINVAL);
    }
    return (~newRawConn(c.fd), default!);
}

// CloseRead shuts down the reading side of the Unix domain connection.
// Most callers should just use Close.
[GoRecv] public static error CloseRead(this ref UnixConn c) {
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

// CloseWrite shuts down the writing side of the Unix domain connection.
// Most callers should just use Close.
[GoRecv] public static error CloseWrite(this ref UnixConn c) {
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

// ReadFromUnix acts like [UnixConn.ReadFrom] but returns a [UnixAddr].
[GoRecv] public static (nint, ж<UnixAddr>, error) ReadFromUnix(this ref UnixConn c, slice<byte> b) {
    if (!c.ok()) {
        return (0, default!, syscall.EINVAL);
    }
    var (n, addr, err) = c.readFrom(b);
    if (err != default!) {
        Ꮡerr = new OpError(Op: "read"u8, Net: c.fd.net, Source: c.fd.laddr, ΔAddr: c.fd.raddr, Err: err); err = ref Ꮡerr.val;
    }
    return (n, addr, err);
}

// ReadFrom implements the [PacketConn] ReadFrom method.
[GoRecv] public static (nint, ΔAddr, error) ReadFrom(this ref UnixConn c, slice<byte> b) {
    if (!c.ok()) {
        return (0, default!, syscall.EINVAL);
    }
    var (n, addr, err) = c.readFrom(b);
    if (err != default!) {
        Ꮡerr = new OpError(Op: "read"u8, Net: c.fd.net, Source: c.fd.laddr, ΔAddr: c.fd.raddr, Err: err); err = ref Ꮡerr.val;
    }
    if (addr == nil) {
        return (n, default!, err);
    }
    return (n, ~addr, err);
}

// ReadMsgUnix reads a message from c, copying the payload into b and
// the associated out-of-band data into oob. It returns the number of
// bytes copied into b, the number of bytes copied into oob, the flags
// that were set on the message and the source address of the message.
//
// Note that if len(b) == 0 and len(oob) > 0, this function will still
// read (and discard) 1 byte from the connection.
[GoRecv] public static (nint n, nint oobn, nint flags, ж<UnixAddr> addr, error err) ReadMsgUnix(this ref UnixConn c, slice<byte> b, slice<byte> oob) {
    nint n = default!;
    nint oobn = default!;
    nint flags = default!;
    ж<UnixAddr> addr = default!;
    error err = default!;

    if (!c.ok()) {
        return (0, 0, 0, default!, syscall.EINVAL);
    }
    (n, oobn, flags, addr, err) = c.readMsg(b, oob);
    if (err != default!) {
        Ꮡerr = new OpError(Op: "read"u8, Net: c.fd.net, Source: c.fd.laddr, ΔAddr: c.fd.raddr, Err: err); err = ref Ꮡerr.val;
    }
    return (n, oobn, flags, addr, err);
}

// WriteToUnix acts like [UnixConn.WriteTo] but takes a [UnixAddr].
[GoRecv] public static (nint, error) WriteToUnix(this ref UnixConn c, slice<byte> b, ж<UnixAddr> Ꮡaddr) {
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

// WriteTo implements the [PacketConn] WriteTo method.
[GoRecv] public static (nint, error) WriteTo(this ref UnixConn c, slice<byte> b, ΔAddr addr) {
    if (!c.ok()) {
        return (0, syscall.EINVAL);
    }
    var (a, ok) = addr._<UnixAddr.val>(ᐧ);
    if (!ok) {
        return (0, new OpError(Op: "write"u8, Net: c.fd.net, Source: c.fd.laddr, ΔAddr: addr, Err: syscall.EINVAL));
    }
    var (n, err) = c.writeTo(b, a);
    if (err != default!) {
        Ꮡerr = new OpError(Op: "write"u8, Net: c.fd.net, Source: c.fd.laddr, ΔAddr: a.opAddr(), Err: err); err = ref Ꮡerr.val;
    }
    return (n, err);
}

// WriteMsgUnix writes a message to addr via c, copying the payload
// from b and the associated out-of-band data from oob. It returns the
// number of payload and out-of-band bytes written.
//
// Note that if len(b) == 0 and len(oob) > 0, this function will still
// write 1 byte to the connection.
[GoRecv] public static (nint n, nint oobn, error err) WriteMsgUnix(this ref UnixConn c, slice<byte> b, slice<byte> oob, ж<UnixAddr> Ꮡaddr) {
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

internal static ж<UnixConn> newUnixConn(ж<netFD> Ꮡfd) {
    ref var fd = ref Ꮡfd.val;

    return Ꮡ(new UnixConn(new conn(Ꮡfd)));
}

// DialUnix acts like [Dial] for Unix networks.
//
// The network must be a Unix network name; see func Dial for details.
//
// If laddr is non-nil, it is used as the local address for the
// connection.
public static (ж<UnixConn>, error) DialUnix(@string network, ж<UnixAddr> Ꮡladdr, ж<UnixAddr> Ꮡraddr) {
    ref var laddr = ref Ꮡladdr.val;
    ref var raddr = ref Ꮡraddr.val;

    var exprᴛ1 = network;
    if (exprᴛ1 == "unix"u8 || exprᴛ1 == "unixgram"u8 || exprᴛ1 == "unixpacket"u8) {
    }
    else { /* default: */
        return (default!, new OpError(Op: "dial"u8, Net: network, Source: laddr.opAddr(), ΔAddr: raddr.opAddr(), Err: ((UnknownNetworkError)network)));
    }

    var sd = Ꮡ(new sysDialer(network: network, address: raddr.String()));
    (c, err) = sd.dialUnix(context.Background(), Ꮡladdr, Ꮡraddr);
    if (err != default!) {
        return (default!, new OpError(Op: "dial"u8, Net: network, Source: laddr.opAddr(), ΔAddr: raddr.opAddr(), Err: err));
    }
    return (c, default!);
}

// UnixListener is a Unix domain socket listener. Clients should
// typically use variables of type [Listener] instead of assuming Unix
// domain sockets.
[GoType] partial struct UnixListener {
    internal ж<netFD> fd;
    internal @string path;
    internal bool unlink;
    internal sync_package.Once unlinkOnce;
}

[GoRecv] internal static bool ok(this ref UnixListener ln) {
    return ln != nil && ln.fd != nil;
}

// SyscallConn returns a raw network connection.
// This implements the [syscall.Conn] interface.
//
// The returned RawConn only supports calling Control. Read and
// Write return an error.
[GoRecv] public static (syscall.RawConn, error) SyscallConn(this ref UnixListener l) {
    if (!l.ok()) {
        return (default!, syscall.EINVAL);
    }
    return (~newRawListener(l.fd), default!);
}

// AcceptUnix accepts the next incoming call and returns the new
// connection.
[GoRecv] public static (ж<UnixConn>, error) AcceptUnix(this ref UnixListener l) {
    if (!l.ok()) {
        return (default!, syscall.EINVAL);
    }
    (c, err) = l.accept();
    if (err != default!) {
        return (default!, new OpError(Op: "accept"u8, Net: l.fd.net, Source: default!, ΔAddr: l.fd.laddr, Err: err));
    }
    return (c, default!);
}

// Accept implements the Accept method in the [Listener] interface.
// Returned connections will be of type [*UnixConn].
[GoRecv] public static (Conn, error) Accept(this ref UnixListener l) {
    if (!l.ok()) {
        return (default!, syscall.EINVAL);
    }
    (c, err) = l.accept();
    if (err != default!) {
        return (default!, new OpError(Op: "accept"u8, Net: l.fd.net, Source: default!, ΔAddr: l.fd.laddr, Err: err));
    }
    return (~c, default!);
}

// Close stops listening on the Unix address. Already accepted
// connections are not closed.
[GoRecv] public static error Close(this ref UnixListener l) {
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

// Addr returns the listener's network address.
// The Addr returned is shared by all invocations of Addr, so
// do not modify it.
[GoRecv] public static ΔAddr Addr(this ref UnixListener l) {
    return l.fd.laddr;
}

// SetDeadline sets the deadline associated with the listener.
// A zero time value disables the deadline.
[GoRecv] public static error SetDeadline(this ref UnixListener l, time.Time t) {
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
[GoRecv] public static (ж<os.File> f, error err) File(this ref UnixListener l) {
    ж<os.File> f = default!;
    error err = default!;

    if (!l.ok()) {
        return (default!, syscall.EINVAL);
    }
    (f, err) = l.file();
    if (err != default!) {
        Ꮡerr = new OpError(Op: "file"u8, Net: l.fd.net, Source: default!, ΔAddr: l.fd.laddr, Err: err); err = ref Ꮡerr.val;
    }
    return (f, err);
}

// ListenUnix acts like [Listen] for Unix networks.
//
// The network must be "unix" or "unixpacket".
public static (ж<UnixListener>, error) ListenUnix(@string network, ж<UnixAddr> Ꮡladdr) {
    ref var laddr = ref Ꮡladdr.val;

    var exprᴛ1 = network;
    if (exprᴛ1 == "unix"u8 || exprᴛ1 == "unixpacket"u8) {
    }
    else { /* default: */
        return (default!, new OpError(Op: "listen"u8, Net: network, Source: default!, ΔAddr: laddr.opAddr(), Err: ((UnknownNetworkError)network)));
    }

    if (laddr == nil) {
        return (default!, new OpError(Op: "listen"u8, Net: network, Source: default!, ΔAddr: laddr.opAddr(), Err: errMissingAddress));
    }
    var sl = Ꮡ(new sysListener(network: network, address: laddr.String()));
    (ln, err) = sl.listenUnix(context.Background(), Ꮡladdr);
    if (err != default!) {
        return (default!, new OpError(Op: "listen"u8, Net: network, Source: default!, ΔAddr: laddr.opAddr(), Err: err));
    }
    return (ln, default!);
}

// ListenUnixgram acts like [ListenPacket] for Unix networks.
//
// The network must be "unixgram".
public static (ж<UnixConn>, error) ListenUnixgram(@string network, ж<UnixAddr> Ꮡladdr) {
    ref var laddr = ref Ꮡladdr.val;

    var exprᴛ1 = network;
    if (exprᴛ1 == "unixgram"u8) {
    }
    else { /* default: */
        return (default!, new OpError(Op: "listen"u8, Net: network, Source: default!, ΔAddr: laddr.opAddr(), Err: ((UnknownNetworkError)network)));
    }

    if (laddr == nil) {
        return (default!, new OpError(Op: "listen"u8, Net: network, Source: default!, ΔAddr: default!, Err: errMissingAddress));
    }
    var sl = Ꮡ(new sysListener(network: network, address: laddr.String()));
    (c, err) = sl.listenUnixgram(context.Background(), Ꮡladdr);
    if (err != default!) {
        return (default!, new OpError(Op: "listen"u8, Net: network, Source: default!, ΔAddr: laddr.opAddr(), Err: err));
    }
    return (c, default!);
}

} // end net_package
