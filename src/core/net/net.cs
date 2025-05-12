// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

/*
Package net provides a portable interface for network I/O, including
TCP/IP, UDP, domain name resolution, and Unix domain sockets.

Although the package provides access to low-level networking
primitives, most clients will need only the basic interface provided
by the [Dial], [Listen], and Accept functions and the associated
[Conn] and [Listener] interfaces. The crypto/tls package uses
the same interfaces and similar Dial and Listen functions.

The Dial function connects to a server:

	conn, err := net.Dial("tcp", "golang.org:80")
	if err != nil {
		// handle error
	}
	fmt.Fprintf(conn, "GET / HTTP/1.0\r\n\r\n")
	status, err := bufio.NewReader(conn).ReadString('\n')
	// ...

The Listen function creates servers:

	ln, err := net.Listen("tcp", ":8080")
	if err != nil {
		// handle error
	}
	for {
		conn, err := ln.Accept()
		if err != nil {
			// handle error
		}
		go handleConnection(conn)
	}

# Name Resolution

The method for resolving domain names, whether indirectly with functions like Dial
or directly with functions like [LookupHost] and [LookupAddr], varies by operating system.

On Unix systems, the resolver has two options for resolving names.
It can use a pure Go resolver that sends DNS requests directly to the servers
listed in /etc/resolv.conf, or it can use a cgo-based resolver that calls C
library routines such as getaddrinfo and getnameinfo.

On Unix the pure Go resolver is preferred over the cgo resolver, because a blocked DNS
request consumes only a goroutine, while a blocked C call consumes an operating system thread.
When cgo is available, the cgo-based resolver is used instead under a variety of
conditions: on systems that do not let programs make direct DNS requests (OS X),
when the LOCALDOMAIN environment variable is present (even if empty),
when the RES_OPTIONS or HOSTALIASES environment variable is non-empty,
when the ASR_CONFIG environment variable is non-empty (OpenBSD only),
when /etc/resolv.conf or /etc/nsswitch.conf specify the use of features that the
Go resolver does not implement.

On all systems (except Plan 9), when the cgo resolver is being used
this package applies a concurrent cgo lookup limit to prevent the system
from running out of system threads. Currently, it is limited to 500 concurrent lookups.

The resolver decision can be overridden by setting the netdns value of the
GODEBUG environment variable (see package runtime) to go or cgo, as in:

	export GODEBUG=netdns=go    # force pure Go resolver
	export GODEBUG=netdns=cgo   # force native resolver (cgo, win32)

The decision can also be forced while building the Go source tree
by setting the netgo or netcgo build tag.

A numeric netdns setting, as in GODEBUG=netdns=1, causes the resolver
to print debugging information about its decisions.
To force a particular resolver while also printing debugging information,
join the two settings by a plus sign, as in GODEBUG=netdns=go+1.

The Go resolver will send an EDNS0 additional header with a DNS request,
to signal a willingness to accept a larger DNS packet size.
This can reportedly cause sporadic failures with the DNS server run
by some modems and routers. Setting GODEBUG=netedns0=0 will disable
sending the additional header.

On macOS, if Go code that uses the net package is built with
-buildmode=c-archive, linking the resulting archive into a C program
requires passing -lresolv when linking the C code.

On Plan 9, the resolver always accesses /net/cs and /net/dns.

On Windows, in Go 1.18.x and earlier, the resolver always used C
library functions, such as GetAddrInfo and DnsQuery.
*/
namespace go;

using context = context_package;
using errors = errors_package;
using poll = @internal.poll_package;
using io = io_package;
using os = os_package;
using sync = sync_package;
using syscall = syscall_package;
using time = time_package;
using _ = unsafe_package; // for linkname
using @internal;

partial class net_package {

// Addr represents a network end point address.
//
// The two methods [Addr.Network] and [Addr.String] conventionally return strings
// that can be passed as the arguments to [Dial], but the exact form
// and meaning of the strings is up to the implementation.
[GoType] partial interface ΔAddr {
    @string Network(); // name of the network (for example, "tcp", "udp")
    @string String(); // string form of address (for example, "192.0.2.1:25", "[2001:db8::1]:80")
}

// Conn is a generic stream-oriented network connection.
//
// Multiple goroutines may invoke methods on a Conn simultaneously.
[GoType] partial interface Conn {
    // Read reads data from the connection.
    // Read can be made to time out and return an error after a fixed
    // time limit; see SetDeadline and SetReadDeadline.
    (nint n, error err) Read(slice<byte> b);
    // Write writes data to the connection.
    // Write can be made to time out and return an error after a fixed
    // time limit; see SetDeadline and SetWriteDeadline.
    (nint n, error err) Write(slice<byte> b);
    // Close closes the connection.
    // Any blocked Read or Write operations will be unblocked and return errors.
    error Close();
    // LocalAddr returns the local network address, if known.
    ΔAddr LocalAddr();
    // RemoteAddr returns the remote network address, if known.
    ΔAddr RemoteAddr();
    // SetDeadline sets the read and write deadlines associated
    // with the connection. It is equivalent to calling both
    // SetReadDeadline and SetWriteDeadline.
    //
    // A deadline is an absolute time after which I/O operations
    // fail instead of blocking. The deadline applies to all future
    // and pending I/O, not just the immediately following call to
    // Read or Write. After a deadline has been exceeded, the
    // connection can be refreshed by setting a deadline in the future.
    //
    // If the deadline is exceeded a call to Read or Write or to other
    // I/O methods will return an error that wraps os.ErrDeadlineExceeded.
    // This can be tested using errors.Is(err, os.ErrDeadlineExceeded).
    // The error's Timeout method will return true, but note that there
    // are other possible errors for which the Timeout method will
    // return true even if the deadline has not been exceeded.
    //
    // An idle timeout can be implemented by repeatedly extending
    // the deadline after successful Read or Write calls.
    //
    // A zero value for t means I/O operations will not time out.
    error SetDeadline(time.Time t);
    // SetReadDeadline sets the deadline for future Read calls
    // and any currently-blocked Read call.
    // A zero value for t means Read will not time out.
    error SetReadDeadline(time.Time t);
    // SetWriteDeadline sets the deadline for future Write calls
    // and any currently-blocked Write call.
    // Even if write times out, it may return n > 0, indicating that
    // some of the data was successfully written.
    // A zero value for t means Write will not time out.
    error SetWriteDeadline(time.Time t);
}

[GoType] partial struct conn {
    internal ж<netFD> fd;
}

[GoRecv] internal static bool ok(this ref conn c) {
    return c != nil && c.fd != nil;
}

// Implementation of the Conn interface.

// Read implements the Conn Read method.
[GoRecv] internal static (nint, error) Read(this ref conn c, slice<byte> b) {
    if (!c.ok()) {
        return (0, syscall.EINVAL);
    }
    var (n, err) = c.fd.Read(b);
    if (err != default! && !AreEqual(err, io.EOF)) {
        Ꮡerr = new OpError(Op: "read"u8, Net: c.fd.net, Source: c.fd.laddr, ΔAddr: c.fd.raddr, Err: err); err = ref Ꮡerr.val;
    }
    return (n, err);
}

// Write implements the Conn Write method.
[GoRecv] internal static (nint, error) Write(this ref conn c, slice<byte> b) {
    if (!c.ok()) {
        return (0, syscall.EINVAL);
    }
    var (n, err) = c.fd.Write(b);
    if (err != default!) {
        Ꮡerr = new OpError(Op: "write"u8, Net: c.fd.net, Source: c.fd.laddr, ΔAddr: c.fd.raddr, Err: err); err = ref Ꮡerr.val;
    }
    return (n, err);
}

// Close closes the connection.
[GoRecv] internal static error Close(this ref conn c) {
    if (!c.ok()) {
        return syscall.EINVAL;
    }
    var err = c.fd.Close();
    if (err != default!) {
        Ꮡerr = new OpError(Op: "close"u8, Net: c.fd.net, Source: c.fd.laddr, ΔAddr: c.fd.raddr, Err: err); err = ref Ꮡerr.val;
    }
    return err;
}

// LocalAddr returns the local network address.
// The Addr returned is shared by all invocations of LocalAddr, so
// do not modify it.
[GoRecv] internal static ΔAddr LocalAddr(this ref conn c) {
    if (!c.ok()) {
        return default!;
    }
    return c.fd.laddr;
}

// RemoteAddr returns the remote network address.
// The Addr returned is shared by all invocations of RemoteAddr, so
// do not modify it.
[GoRecv] internal static ΔAddr RemoteAddr(this ref conn c) {
    if (!c.ok()) {
        return default!;
    }
    return c.fd.raddr;
}

// SetDeadline implements the Conn SetDeadline method.
[GoRecv] internal static error SetDeadline(this ref conn c, time.Time t) {
    if (!c.ok()) {
        return syscall.EINVAL;
    }
    {
        var err = c.fd.SetDeadline(t); if (err != default!) {
            return new OpError(Op: "set"u8, Net: c.fd.net, Source: default!, ΔAddr: c.fd.laddr, Err: err);
        }
    }
    return default!;
}

// SetReadDeadline implements the Conn SetReadDeadline method.
[GoRecv] internal static error SetReadDeadline(this ref conn c, time.Time t) {
    if (!c.ok()) {
        return syscall.EINVAL;
    }
    {
        var err = c.fd.SetReadDeadline(t); if (err != default!) {
            return new OpError(Op: "set"u8, Net: c.fd.net, Source: default!, ΔAddr: c.fd.laddr, Err: err);
        }
    }
    return default!;
}

// SetWriteDeadline implements the Conn SetWriteDeadline method.
[GoRecv] internal static error SetWriteDeadline(this ref conn c, time.Time t) {
    if (!c.ok()) {
        return syscall.EINVAL;
    }
    {
        var err = c.fd.SetWriteDeadline(t); if (err != default!) {
            return new OpError(Op: "set"u8, Net: c.fd.net, Source: default!, ΔAddr: c.fd.laddr, Err: err);
        }
    }
    return default!;
}

// SetReadBuffer sets the size of the operating system's
// receive buffer associated with the connection.
[GoRecv] internal static error SetReadBuffer(this ref conn c, nint bytes) {
    if (!c.ok()) {
        return syscall.EINVAL;
    }
    {
        var err = setReadBuffer(c.fd, bytes); if (err != default!) {
            return new OpError(Op: "set"u8, Net: c.fd.net, Source: default!, ΔAddr: c.fd.laddr, Err: err);
        }
    }
    return default!;
}

// SetWriteBuffer sets the size of the operating system's
// transmit buffer associated with the connection.
[GoRecv] internal static error SetWriteBuffer(this ref conn c, nint bytes) {
    if (!c.ok()) {
        return syscall.EINVAL;
    }
    {
        var err = setWriteBuffer(c.fd, bytes); if (err != default!) {
            return new OpError(Op: "set"u8, Net: c.fd.net, Source: default!, ΔAddr: c.fd.laddr, Err: err);
        }
    }
    return default!;
}

// File returns a copy of the underlying [os.File].
// It is the caller's responsibility to close f when finished.
// Closing c does not affect f, and closing f does not affect c.
//
// The returned os.File's file descriptor is different from the connection's.
// Attempting to change properties of the original using this duplicate
// may or may not have the desired effect.
[GoRecv] internal static (ж<os.File> f, error err) File(this ref conn c) {
    ж<os.File> f = default!;
    error err = default!;

    (f, err) = c.fd.dup();
    if (err != default!) {
        Ꮡerr = new OpError(Op: "file"u8, Net: c.fd.net, Source: c.fd.laddr, ΔAddr: c.fd.raddr, Err: err); err = ref Ꮡerr.val;
    }
    return (f, err);
}

// PacketConn is a generic packet-oriented network connection.
//
// Multiple goroutines may invoke methods on a PacketConn simultaneously.
[GoType] partial interface PacketConn {
    // ReadFrom reads a packet from the connection,
    // copying the payload into p. It returns the number of
    // bytes copied into p and the return address that
    // was on the packet.
    // It returns the number of bytes read (0 <= n <= len(p))
    // and any error encountered. Callers should always process
    // the n > 0 bytes returned before considering the error err.
    // ReadFrom can be made to time out and return an error after a
    // fixed time limit; see SetDeadline and SetReadDeadline.
    (nint n, ΔAddr addr, error err) ReadFrom(slice<byte> p);
    // WriteTo writes a packet with payload p to addr.
    // WriteTo can be made to time out and return an Error after a
    // fixed time limit; see SetDeadline and SetWriteDeadline.
    // On packet-oriented connections, write timeouts are rare.
    (nint n, error err) WriteTo(slice<byte> p, ΔAddr addr);
    // Close closes the connection.
    // Any blocked ReadFrom or WriteTo operations will be unblocked and return errors.
    error Close();
    // LocalAddr returns the local network address, if known.
    ΔAddr LocalAddr();
    // SetDeadline sets the read and write deadlines associated
    // with the connection. It is equivalent to calling both
    // SetReadDeadline and SetWriteDeadline.
    //
    // A deadline is an absolute time after which I/O operations
    // fail instead of blocking. The deadline applies to all future
    // and pending I/O, not just the immediately following call to
    // Read or Write. After a deadline has been exceeded, the
    // connection can be refreshed by setting a deadline in the future.
    //
    // If the deadline is exceeded a call to Read or Write or to other
    // I/O methods will return an error that wraps os.ErrDeadlineExceeded.
    // This can be tested using errors.Is(err, os.ErrDeadlineExceeded).
    // The error's Timeout method will return true, but note that there
    // are other possible errors for which the Timeout method will
    // return true even if the deadline has not been exceeded.
    //
    // An idle timeout can be implemented by repeatedly extending
    // the deadline after successful ReadFrom or WriteTo calls.
    //
    // A zero value for t means I/O operations will not time out.
    error SetDeadline(time.Time t);
    // SetReadDeadline sets the deadline for future ReadFrom calls
    // and any currently-blocked ReadFrom call.
    // A zero value for t means ReadFrom will not time out.
    error SetReadDeadline(time.Time t);
    // SetWriteDeadline sets the deadline for future WriteTo calls
    // and any currently-blocked WriteTo call.
    // Even if write times out, it may return n > 0, indicating that
    // some of the data was successfully written.
    // A zero value for t means WriteTo will not time out.
    error SetWriteDeadline(time.Time t);
}


[GoType("dyn")] partial struct listenerBacklogCacheᴛ1 {
    public partial ref sync_package.Once Once { get; }
    internal nint val;
}
internal static listenerBacklogCacheᴛ1 listenerBacklogCache;

// listenerBacklog is a caching wrapper around maxListenerBacklog.
//
// listenerBacklog should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/database64128/tfo-go/v2
//   - github.com/metacubex/tfo-go
//   - github.com/sagernet/tfo-go
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname listenerBacklog
internal static nint listenerBacklog() {
    listenerBacklogCache.Do(
    var listenerBacklogCacheʗ2 = listenerBacklogCache;
    () => {
        listenerBacklogCacheʗ2.val = maxListenerBacklog();
    });
    return listenerBacklogCache.val;
}

// A Listener is a generic network listener for stream-oriented protocols.
//
// Multiple goroutines may invoke methods on a Listener simultaneously.
[GoType] partial interface Listener {
    // Accept waits for and returns the next connection to the listener.
    (Conn, error) Accept();
    // Close closes the listener.
    // Any blocked Accept operations will be unblocked and return errors.
    error Close();
    // Addr returns the listener's network address.
    ΔAddr Addr();
}

// An Error represents a network error.
[GoType] partial interface ΔError :
    error
{
    bool Timeout(); // Is the error a timeout?
    // Deprecated: Temporary errors are not well-defined.
    // Most "temporary" errors are timeouts, and the few exceptions are surprising.
    // Do not use this method.
    bool Temporary();
}

// Various errors contained in OpError.
internal static error errNoSuitableAddress = errors.New("no suitable address found"u8);

internal static error errMissingAddress = errors.New("missing address"u8);

internal static canceledError errCanceled = new canceledError(nil);

public static error ErrWriteToConnected = errors.New("use of WriteTo with pre-connected connection"u8);

// canceledError lets us return the same error string we have always
// returned, while still being Is context.Canceled.
[GoType] partial struct canceledError {
}

internal static @string Error(this canceledError _) {
    return "operation was canceled"u8;
}

internal static bool Is(this canceledError _, error err) {
    return AreEqual(err, context.Canceled);
}

// mapErr maps from the context errors to the historical internal net
// error values.
internal static error mapErr(error err) {
    var exprᴛ1 = err;
    if (exprᴛ1 == context.Canceled) {
        return errCanceled;
    }
    if (exprᴛ1 == context.DeadlineExceeded) {
        return errTimeout;
    }
    { /* default: */
        return err;
    }

}

// OpError is the error type usually returned by functions in the net
// package. It describes the operation, network type, and address of
// an error.
[GoType] partial struct OpError {
    // Op is the operation which caused the error, such as
    // "read" or "write".
    public @string Op;
    // Net is the network type on which this error occurred,
    // such as "tcp" or "udp6".
    public @string Net;
    // For operations involving a remote network connection, like
    // Dial, Read, or Write, Source is the corresponding local
    // network address.
    public ΔAddr Source;
    // Addr is the network address for which this error occurred.
    // For local operations, like Listen or SetDeadline, Addr is
    // the address of the local endpoint being manipulated.
    // For operations involving a remote network connection, like
    // Dial, Read, or Write, Addr is the remote address of that
    // connection.
    public ΔAddr Addr;
    // Err is the error that occurred during the operation.
    // The Error method panics if the error is nil.
    public error Err;
}

[GoRecv] public static error Unwrap(this ref OpError e) {
    return e.Err;
}

[GoRecv] public static @string Error(this ref OpError e) {
    if (e == nil) {
        return "<nil>"u8;
    }
    @string s = e.Op;
    if (e.Net != ""u8) {
        s += " "u8 + e.Net;
    }
    if (e.Source != default!) {
        s += " "u8 + e.Source.String();
    }
    if (e.Addr != default!) {
        if (e.Source != default!){
            s += "->"u8;
        } else {
            s += " "u8;
        }
        s += e.Addr.String();
    }
    s += ": "u8 + e.Err.Error();
    return s;
}

internal static time.Time aLongTimeAgo = time.Unix(1, 0);
internal static time.Time noDeadline = new time.Time(nil);
internal static channel<EmptyStruct> noCancel = (channel<EmptyStruct>)(default!);

[GoType] partial interface timeout {
    bool Timeout();
}

[GoRecv] public static bool Timeout(this ref OpError e) {
    {
        var (ne, okΔ1) = e.Err._<ж<os.SyscallError>>(ᐧ); if (okΔ1) {
            var (tΔ1, okΔ2) = (~ne).Err._<timeout>(ᐧ);
            return okΔ2 && tΔ1.Timeout();
        }
    }
    var (t, ok) = e.Err._<timeout>(ᐧ);
    return ok && t.Timeout();
}

[GoType] partial interface temporary {
    bool Temporary();
}

[GoRecv] public static bool Temporary(this ref OpError e) {
    // Treat ECONNRESET and ECONNABORTED as temporary errors when
    // they come from calling accept. See issue 6163.
    if (e.Op == "accept"u8 && isConnError(e.Err)) {
        return true;
    }
    {
        var (ne, okΔ1) = e.Err._<ж<os.SyscallError>>(ᐧ); if (okΔ1) {
            var (tΔ1, okΔ2) = (~ne).Err._<temporary>(ᐧ);
            return okΔ2 && tΔ1.Temporary();
        }
    }
    var (t, ok) = e.Err._<temporary>(ᐧ);
    return ok && t.Temporary();
}

// A ParseError is the error type of literal network address parsers.
[GoType] partial struct ParseError {
    // Type is the type of string that was expected, such as
    // "IP address", "CIDR address".
    public @string Type;
    // Text is the malformed text string.
    public @string Text;
}

[GoRecv] public static @string Error(this ref ParseError e) {
    return "invalid "u8 + e.Type + ": "u8 + e.Text;
}

[GoRecv] public static bool Timeout(this ref ParseError e) {
    return false;
}

[GoRecv] public static bool Temporary(this ref ParseError e) {
    return false;
}

[GoType] partial struct AddrError {
    public @string Err;
    public @string Addr;
}

[GoRecv] public static @string Error(this ref AddrError e) {
    if (e == nil) {
        return "<nil>"u8;
    }
    @string s = e.Err;
    if (e.Addr != ""u8) {
        s = "address "u8 + e.Addr + ": "u8 + s;
    }
    return s;
}

[GoRecv] public static bool Timeout(this ref AddrError e) {
    return false;
}

[GoRecv] public static bool Temporary(this ref AddrError e) {
    return false;
}

[GoType("@string")] partial struct UnknownNetworkError;

public static @string Error(this UnknownNetworkError e) {
    return "unknown network "u8 + ((@string)e);
}

public static bool Timeout(this UnknownNetworkError e) {
    return false;
}

public static bool Temporary(this UnknownNetworkError e) {
    return false;
}

[GoType("@string")] partial struct InvalidAddrError;

public static @string Error(this InvalidAddrError e) {
    return ((@string)e);
}

public static bool Timeout(this InvalidAddrError e) {
    return false;
}

public static bool Temporary(this InvalidAddrError e) {
    return false;
}

// errTimeout exists to return the historical "i/o timeout" string
// for context.DeadlineExceeded. See mapErr.
// It is also used when Dialer.Deadline is exceeded.
// error.Is(errTimeout, context.DeadlineExceeded) returns true.
//
// TODO(iant): We could consider changing this to os.ErrDeadlineExceeded
// in the future, if we make
//
//	errors.Is(os.ErrDeadlineExceeded, context.DeadlineExceeded)
//
// return true.
internal static error errTimeout = Ꮡ(new timeoutError(nil));

[GoType] partial struct timeoutError {
}

[GoRecv] internal static @string Error(this ref timeoutError e) {
    return "i/o timeout"u8;
}

[GoRecv] internal static bool Timeout(this ref timeoutError e) {
    return true;
}

[GoRecv] internal static bool Temporary(this ref timeoutError e) {
    return true;
}

[GoRecv] internal static bool Is(this ref timeoutError e, error err) {
    return AreEqual(err, context.DeadlineExceeded);
}

// DNSConfigError represents an error reading the machine's DNS configuration.
// (No longer used; kept for compatibility.)
[GoType] partial struct DNSConfigError {
    public error Err;
}

[GoRecv] public static error Unwrap(this ref DNSConfigError e) {
    return e.Err;
}

[GoRecv] public static @string Error(this ref DNSConfigError e) {
    return "error reading DNS config: "u8 + e.Err.Error();
}

[GoRecv] public static bool Timeout(this ref DNSConfigError e) {
    return false;
}

[GoRecv] public static bool Temporary(this ref DNSConfigError e) {
    return false;
}

// Various errors contained in DNSError.
internal static ж<notFoundError> errNoSuchHost = Ꮡ(new notFoundError("no such host"));

internal static ж<notFoundError> errUnknownPort = Ꮡ(new notFoundError("unknown port"));

// notFoundError is a special error understood by the newDNSError function,
// which causes a creation of a DNSError with IsNotFound field set to true.
[GoType] partial struct notFoundError {
    internal @string s;
}

[GoRecv] internal static @string Error(this ref notFoundError e) {
    return e.s;
}

// temporaryError is an error type that implements the [Error] interface.
// It returns true from the Temporary method.
[GoType] partial struct temporaryError {
    internal @string s;
}

[GoRecv] internal static @string Error(this ref temporaryError e) {
    return e.s;
}

[GoRecv] internal static bool Temporary(this ref temporaryError e) {
    return true;
}

[GoRecv] internal static bool Timeout(this ref temporaryError e) {
    return false;
}

// DNSError represents a DNS lookup error.
[GoType] partial struct DNSError {
    public error UnwrapErr;  // error returned by the [DNSError.Unwrap] method, might be nil
    public @string Err; // description of the error
    public @string Name; // name looked for
    public @string Server; // server used
    public bool IsTimeout;   // if true, timed out; not all timeouts set this
    public bool IsTemporary;   // if true, error is temporary; not all errors set this
    // IsNotFound is set to true when the requested name does not
    // contain any records of the requested type (data not found),
    // or the name itself was not found (NXDOMAIN).
    public bool IsNotFound;
}

// newDNSError creates a new *DNSError.
// Based on the err, it sets the UnwrapErr, IsTimeout, IsTemporary, IsNotFound fields.
internal static ж<DNSError> newDNSError(error err, @string name, @string server) {
    ref var isTimeout = ref heap(new bool(), out var ᏑisTimeout);
    ref var isTemporary = ref heap(new bool(), out var ᏑisTemporary);
    error unwrapErr = default!;
    {
        var (errΔ1, ok) = err._<ΔError>(ᐧ); if (ok) {
            isTimeout = errΔ1.Timeout();
            isTemporary = errΔ1.Temporary();
        }
    }
    // At this time, the only errors we wrap are context errors, to allow
    // users to check for canceled/timed out requests.
    if (errors.Is(err, context.DeadlineExceeded) || errors.Is(err, context.Canceled)) {
        unwrapErr = err;
    }
    (_, isNotFound) = err._<notFoundError.val>(ᐧ);
    return Ꮡ(new DNSError(
        UnwrapErr: unwrapErr,
        Err: err.Error(),
        Name: name,
        Server: server,
        IsTimeout: isTimeout,
        IsTemporary: isTemporary,
        IsNotFound: isNotFound
    ));
}

// Unwrap returns e.UnwrapErr.
[GoRecv] public static error Unwrap(this ref DNSError e) {
    return e.UnwrapErr;
}

[GoRecv] public static @string Error(this ref DNSError e) {
    if (e == nil) {
        return "<nil>"u8;
    }
    @string s = "lookup "u8 + e.Name;
    if (e.Server != ""u8) {
        s += " on "u8 + e.Server;
    }
    s += ": "u8 + e.Err;
    return s;
}

// Timeout reports whether the DNS lookup is known to have timed out.
// This is not always known; a DNS lookup may fail due to a timeout
// and return a [DNSError] for which Timeout returns false.
[GoRecv] public static bool Timeout(this ref DNSError e) {
    return e.IsTimeout;
}

// Temporary reports whether the DNS error is known to be temporary.
// This is not always known; a DNS lookup may fail due to a temporary
// error and return a [DNSError] for which Temporary returns false.
[GoRecv] public static bool Temporary(this ref DNSError e) {
    return e.IsTimeout || e.IsTemporary;
}

// errClosed exists just so that the docs for ErrClosed don't mention
// the internal package poll.
internal static poll.errNetClosing errClosed = poll.ErrNetClosing;

// ErrClosed is the error returned by an I/O call on a network
// connection that has already been closed, or that is closed by
// another goroutine before the I/O is completed. This may be wrapped
// in another error, and should normally be tested using
// errors.Is(err, net.ErrClosed).
public static error ErrClosed = errClosed;

// noReadFrom can be embedded alongside another type to
// hide the ReadFrom method of that other type.
[GoType] partial struct noReadFrom {
}

// ReadFrom hides another ReadFrom method.
// It should never be called.
internal static (int64, error) ReadFrom(this noReadFrom _, io.Reader _) {
    throw panic("can't happen");
}

// tcpConnWithoutReadFrom implements all the methods of *TCPConn other
// than ReadFrom. This is used to permit ReadFrom to call io.Copy
// without leading to a recursive call to ReadFrom.
[GoType] partial struct tcpConnWithoutReadFrom {
    internal partial ref noReadFrom noReadFrom { get; }
    public partial ref ж<TCPConn> TCPConn { get; }
}

// Fallback implementation of io.ReaderFrom's ReadFrom, when sendfile isn't
// applicable.
internal static (int64 n, error err) genericReadFrom(ж<TCPConn> Ꮡc, io.Reader r) {
    int64 n = default!;
    error err = default!;

    ref var c = ref Ꮡc.val;
    // Use wrapper to hide existing r.ReadFrom from io.Copy.
    return io.Copy(new tcpConnWithoutReadFrom(TCPConn: c), r);
}

// noWriteTo can be embedded alongside another type to
// hide the WriteTo method of that other type.
[GoType] partial struct noWriteTo {
}

// WriteTo hides another WriteTo method.
// It should never be called.
internal static (int64, error) WriteTo(this noWriteTo _, io.Writer _) {
    throw panic("can't happen");
}

// tcpConnWithoutWriteTo implements all the methods of *TCPConn other
// than WriteTo. This is used to permit WriteTo to call io.Copy
// without leading to a recursive call to WriteTo.
[GoType] partial struct tcpConnWithoutWriteTo {
    internal partial ref noWriteTo noWriteTo { get; }
    public partial ref ж<TCPConn> TCPConn { get; }
}

// Fallback implementation of io.WriterTo's WriteTo, when zero-copy isn't applicable.
internal static (int64 n, error err) genericWriteTo(ж<TCPConn> Ꮡc, io.Writer w) {
    int64 n = default!;
    error err = default!;

    ref var c = ref Ꮡc.val;
    // Use wrapper to hide existing w.WriteTo from io.Copy.
    return io.Copy(w, new tcpConnWithoutWriteTo(TCPConn: c));
}

// Limit the number of concurrent cgo-using goroutines, because
// each will block an entire operating system thread. The usual culprit
// is resolving many DNS names in separate goroutines but the DNS
// server is not responding. Then the many lookups each use a different
// thread, and the system or the program runs out of threads.
internal static channel<EmptyStruct> threadLimit;

internal static sync.Once threadOnce;

[GoType("dyn")] partial struct acquireThread_type {
}

internal static error acquireThread(context.Context ctx) {
    threadOnce.Do(
    var threadLimitʗ2 = threadLimit;
    () => {
        threadLimitʗ2 = new channel<EmptyStruct>(concurrentThreadsLimit());
    });
    switch (select(threadLimit.ᐸꟷ(new acquireThread_type(), ꓸꓸꓸ), ᐸꟷ(ctx.Done(), ꓸꓸꓸ))) {
    case 0: {
        return default!;
    }
    case 1 when ctx.Done().ꟷᐳ(out _): {
        return ctx.Err();
    }}
}

internal static void releaseThread() {
    ᐸꟷ(threadLimit);
}

// buffersWriter is the interface implemented by Conns that support a
// "writev"-like batch write optimization.
// writeBuffers should fully consume and write all chunks from the
// provided Buffers, else it should report a non-nil error.
[GoType] partial interface buffersWriter {
    (int64, error) writeBuffers(ж<Buffers> _);
}

[GoType("[]byte")] partial struct Buffers;

internal static io.WriterTo _ᴛ1ʗ = ((ж<Buffers>)default!);
internal static io.Reader _ᴛ2ʗ = ((ж<Buffers>)default!);

// WriteTo writes contents of the buffers to w.
//
// WriteTo implements [io.WriterTo] for [Buffers].
//
// WriteTo modifies the slice v as well as v[i] for 0 <= i < len(v),
// but does not modify v[i][j] for any i, j.
[GoRecv] public static (int64 n, error err) WriteTo(this ref Buffers v, io.Writer w) {
    int64 n = default!;
    error err = default!;

    {
        var (wv, ok) = w._<buffersWriter>(ᐧ); if (ok) {
            return wv.writeBuffers(v);
        }
    }
    foreach (var (_, b) in v) {
        var (nb, errΔ1) = w.Write(b);
        n += ((int64)nb);
        if (errΔ1 != default!) {
            v.consume(n);
            return (n, errΔ1);
        }
    }
    v.consume(n);
    return (n, default!);
}

// Read from the buffers.
//
// Read implements [io.Reader] for [Buffers].
//
// Read modifies the slice v as well as v[i] for 0 <= i < len(v),
// but does not modify v[i][j] for any i, j.
[GoRecv] public static (nint n, error err) Read(this ref Buffers v, slice<byte> p) {
    nint n = default!;
    error err = default!;

    while (len(p) > 0 && len(v) > 0) {
        nint n0 = copy(p, (ж<ж<Buffers>>)[0]);
        v.consume(((int64)n0));
        p = p[(int)(n0)..];
        n += n0;
    }
    if (len(v) == 0) {
        err = io.EOF;
    }
    return (n, err);
}

[GoRecv] internal static void consume(this ref Buffers v, int64 n) {
    while (len(v) > 0) {
        var ln0 = ((int64)len((ж<ж<Buffers>>)[0]));
        if (ln0 > n) {
            (ж<ж<Buffers>>)[0] = (ж<ж<Buffers>>)[0][(int)(n)..];
            return;
        }
        n -= ln0;
        (ж<ж<Buffers>>)[0] = default!;
        v = (ж<ж<Buffers>>)[1..];
    }
}

} // end net_package
