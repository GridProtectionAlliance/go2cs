// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

/*
Package net provides a portable interface for network I/O, including
TCP/IP, UDP, domain name resolution, and Unix domain sockets.

Although the package provides access to low-level networking
primitives, most clients will need only the basic interface provided
by the Dial, Listen, and Accept functions and the associated
Conn and Listener interfaces. The crypto/tls package uses
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

Name Resolution

The method for resolving domain names, whether indirectly with functions like Dial
or directly with functions like LookupHost and LookupAddr, varies by operating system.

On Unix systems, the resolver has two options for resolving names.
It can use a pure Go resolver that sends DNS requests directly to the servers
listed in /etc/resolv.conf, or it can use a cgo-based resolver that calls C
library routines such as getaddrinfo and getnameinfo.

By default the pure Go resolver is used, because a blocked DNS request consumes
only a goroutine, while a blocked C call consumes an operating system thread.
When cgo is available, the cgo-based resolver is used instead under a variety of
conditions: on systems that do not let programs make direct DNS requests (OS X),
when the LOCALDOMAIN environment variable is present (even if empty),
when the RES_OPTIONS or HOSTALIASES environment variable is non-empty,
when the ASR_CONFIG environment variable is non-empty (OpenBSD only),
when /etc/resolv.conf or /etc/nsswitch.conf specify the use of features that the
Go resolver does not implement, and when the name being looked up ends in .local
or is an mDNS name.

The resolver decision can be overridden by setting the netdns value of the
GODEBUG environment variable (see package runtime) to go or cgo, as in:

    export GODEBUG=netdns=go    # force pure Go resolver
    export GODEBUG=netdns=cgo   # force cgo resolver

The decision can also be forced while building the Go source tree
by setting the netgo or netcgo build tag.

A numeric netdns setting, as in GODEBUG=netdns=1, causes the resolver
to print debugging information about its decisions.
To force a particular resolver while also printing debugging information,
join the two settings by a plus sign, as in GODEBUG=netdns=go+1.

On Plan 9, the resolver always accesses /net/cs and /net/dns.

On Windows, the resolver always uses C library functions, such as GetAddrInfo and DnsQuery.

*/
// package net -- go2cs converted at 2020 August 29 08:27:06 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\net.go
using context = go.context_package;
using errors = go.errors_package;
using poll = go.@internal.poll_package;
using io = go.io_package;
using os = go.os_package;
using syscall = go.syscall_package;
using time = go.time_package;
using static go.builtin;

namespace go
{
    public static partial class net_package
    {
        // netGo and netCgo contain the state of the build tags used
        // to build this binary, and whether cgo is available.
        // conf.go mirrors these into conf for easier testing.
        private static bool netGo = default;        private static bool netCgo = default;

        // Addr represents a network end point address.
        //
        // The two methods Network and String conventionally return strings
        // that can be passed as the arguments to Dial, but the exact form
        // and meaning of the strings is up to the implementation.
        public partial interface Addr
        {
            @string Network(); // name of the network (for example, "tcp", "udp")
            @string String(); // string form of address (for example, "192.0.2.1:25", "[2001:db8::1]:80")
        }

        // Conn is a generic stream-oriented network connection.
        //
        // Multiple goroutines may invoke methods on a Conn simultaneously.
        public partial interface Conn
        {
            error Read(slice<byte> b); // Write writes data to the connection.
// Write can be made to time out and return an Error with Timeout() == true
// after a fixed time limit; see SetDeadline and SetWriteDeadline.
            error Write(slice<byte> b); // Close closes the connection.
// Any blocked Read or Write operations will be unblocked and return errors.
            error Close(); // LocalAddr returns the local network address.
            error LocalAddr(); // RemoteAddr returns the remote network address.
            error RemoteAddr(); // SetDeadline sets the read and write deadlines associated
// with the connection. It is equivalent to calling both
// SetReadDeadline and SetWriteDeadline.
//
// A deadline is an absolute time after which I/O operations
// fail with a timeout (see type Error) instead of
// blocking. The deadline applies to all future and pending
// I/O, not just the immediately following call to Read or
// Write. After a deadline has been exceeded, the connection
// can be refreshed by setting a deadline in the future.
//
// An idle timeout can be implemented by repeatedly extending
// the deadline after successful Read or Write calls.
//
// A zero value for t means I/O operations will not time out.
            error SetDeadline(time.Time t); // SetReadDeadline sets the deadline for future Read calls
// and any currently-blocked Read call.
// A zero value for t means Read will not time out.
            error SetReadDeadline(time.Time t); // SetWriteDeadline sets the deadline for future Write calls
// and any currently-blocked Write call.
// Even if write times out, it may return n > 0, indicating that
// some of the data was successfully written.
// A zero value for t means Write will not time out.
            error SetWriteDeadline(time.Time t);
        }

        private partial struct conn
        {
            public ptr<netFD> fd;
        }

        private static bool ok(this ref conn c)
        {
            return c != null && c.fd != null;
        }

        // Implementation of the Conn interface.

        // Read implements the Conn Read method.
        private static (long, error) Read(this ref conn c, slice<byte> b)
        {
            if (!c.ok())
            {
                return (0L, syscall.EINVAL);
            }
            var (n, err) = c.fd.Read(b);
            if (err != null && err != io.EOF)
            {
                err = ref new OpError(Op:"read",Net:c.fd.net,Source:c.fd.laddr,Addr:c.fd.raddr,Err:err);
            }
            return (n, err);
        }

        // Write implements the Conn Write method.
        private static (long, error) Write(this ref conn c, slice<byte> b)
        {
            if (!c.ok())
            {
                return (0L, syscall.EINVAL);
            }
            var (n, err) = c.fd.Write(b);
            if (err != null)
            {
                err = ref new OpError(Op:"write",Net:c.fd.net,Source:c.fd.laddr,Addr:c.fd.raddr,Err:err);
            }
            return (n, err);
        }

        // Close closes the connection.
        private static error Close(this ref conn c)
        {
            if (!c.ok())
            {
                return error.As(syscall.EINVAL);
            }
            var err = c.fd.Close();
            if (err != null)
            {
                err = ref new OpError(Op:"close",Net:c.fd.net,Source:c.fd.laddr,Addr:c.fd.raddr,Err:err);
            }
            return error.As(err);
        }

        // LocalAddr returns the local network address.
        // The Addr returned is shared by all invocations of LocalAddr, so
        // do not modify it.
        private static Addr LocalAddr(this ref conn c)
        {
            if (!c.ok())
            {
                return null;
            }
            return c.fd.laddr;
        }

        // RemoteAddr returns the remote network address.
        // The Addr returned is shared by all invocations of RemoteAddr, so
        // do not modify it.
        private static Addr RemoteAddr(this ref conn c)
        {
            if (!c.ok())
            {
                return null;
            }
            return c.fd.raddr;
        }

        // SetDeadline implements the Conn SetDeadline method.
        private static error SetDeadline(this ref conn c, time.Time t)
        {
            if (!c.ok())
            {
                return error.As(syscall.EINVAL);
            }
            {
                var err = c.fd.pfd.SetDeadline(t);

                if (err != null)
                {
                    return error.As(ref new OpError(Op:"set",Net:c.fd.net,Source:nil,Addr:c.fd.laddr,Err:err));
                }

            }
            return error.As(null);
        }

        // SetReadDeadline implements the Conn SetReadDeadline method.
        private static error SetReadDeadline(this ref conn c, time.Time t)
        {
            if (!c.ok())
            {
                return error.As(syscall.EINVAL);
            }
            {
                var err = c.fd.pfd.SetReadDeadline(t);

                if (err != null)
                {
                    return error.As(ref new OpError(Op:"set",Net:c.fd.net,Source:nil,Addr:c.fd.laddr,Err:err));
                }

            }
            return error.As(null);
        }

        // SetWriteDeadline implements the Conn SetWriteDeadline method.
        private static error SetWriteDeadline(this ref conn c, time.Time t)
        {
            if (!c.ok())
            {
                return error.As(syscall.EINVAL);
            }
            {
                var err = c.fd.pfd.SetWriteDeadline(t);

                if (err != null)
                {
                    return error.As(ref new OpError(Op:"set",Net:c.fd.net,Source:nil,Addr:c.fd.laddr,Err:err));
                }

            }
            return error.As(null);
        }

        // SetReadBuffer sets the size of the operating system's
        // receive buffer associated with the connection.
        private static error SetReadBuffer(this ref conn c, long bytes)
        {
            if (!c.ok())
            {
                return error.As(syscall.EINVAL);
            }
            {
                var err = setReadBuffer(c.fd, bytes);

                if (err != null)
                {
                    return error.As(ref new OpError(Op:"set",Net:c.fd.net,Source:nil,Addr:c.fd.laddr,Err:err));
                }

            }
            return error.As(null);
        }

        // SetWriteBuffer sets the size of the operating system's
        // transmit buffer associated with the connection.
        private static error SetWriteBuffer(this ref conn c, long bytes)
        {
            if (!c.ok())
            {
                return error.As(syscall.EINVAL);
            }
            {
                var err = setWriteBuffer(c.fd, bytes);

                if (err != null)
                {
                    return error.As(ref new OpError(Op:"set",Net:c.fd.net,Source:nil,Addr:c.fd.laddr,Err:err));
                }

            }
            return error.As(null);
        }

        // File sets the underlying os.File to blocking mode and returns a copy.
        // It is the caller's responsibility to close f when finished.
        // Closing c does not affect f, and closing f does not affect c.
        //
        // The returned os.File's file descriptor is different from the connection's.
        // Attempting to change properties of the original using this duplicate
        // may or may not have the desired effect.
        //
        // On Unix systems this will cause the SetDeadline methods to stop working.
        private static (ref os.File, error) File(this ref conn c)
        {
            f, err = c.fd.dup();
            if (err != null)
            {
                err = ref new OpError(Op:"file",Net:c.fd.net,Source:c.fd.laddr,Addr:c.fd.raddr,Err:err);
            }
            return;
        }

        // PacketConn is a generic packet-oriented network connection.
        //
        // Multiple goroutines may invoke methods on a PacketConn simultaneously.
        public partial interface PacketConn
        {
            error ReadFrom(slice<byte> b); // WriteTo writes a packet with payload b to addr.
// WriteTo can be made to time out and return
// an Error with Timeout() == true after a fixed time limit;
// see SetDeadline and SetWriteDeadline.
// On packet-oriented connections, write timeouts are rare.
            error WriteTo(slice<byte> b, Addr addr); // Close closes the connection.
// Any blocked ReadFrom or WriteTo operations will be unblocked and return errors.
            error Close(); // LocalAddr returns the local network address.
            error LocalAddr(); // SetDeadline sets the read and write deadlines associated
// with the connection. It is equivalent to calling both
// SetReadDeadline and SetWriteDeadline.
//
// A deadline is an absolute time after which I/O operations
// fail with a timeout (see type Error) instead of
// blocking. The deadline applies to all future and pending
// I/O, not just the immediately following call to ReadFrom or
// WriteTo. After a deadline has been exceeded, the connection
// can be refreshed by setting a deadline in the future.
//
// An idle timeout can be implemented by repeatedly extending
// the deadline after successful ReadFrom or WriteTo calls.
//
// A zero value for t means I/O operations will not time out.
            error SetDeadline(time.Time t); // SetReadDeadline sets the deadline for future ReadFrom calls
// and any currently-blocked ReadFrom call.
// A zero value for t means ReadFrom will not time out.
            error SetReadDeadline(time.Time t); // SetWriteDeadline sets the deadline for future WriteTo calls
// and any currently-blocked WriteTo call.
// Even if write times out, it may return n > 0, indicating that
// some of the data was successfully written.
// A zero value for t means WriteTo will not time out.
            error SetWriteDeadline(time.Time t);
        }

        private static var listenerBacklog = maxListenerBacklog();

        // A Listener is a generic network listener for stream-oriented protocols.
        //
        // Multiple goroutines may invoke methods on a Listener simultaneously.
        public partial interface Listener
        {
            Addr Accept(); // Close closes the listener.
// Any blocked Accept operations will be unblocked and return errors.
            Addr Close(); // Addr returns the listener's network address.
            Addr Addr();
        }

        // An Error represents a network error.
        public partial interface Error : error
        {
            bool Timeout(); // Is the error a timeout?
            bool Temporary(); // Is the error temporary?
        }

        // Various errors contained in OpError.
 
        // For connection setup operations.
        private static var errNoSuitableAddress = errors.New("no suitable address found");        private static var errMissingAddress = errors.New("missing address");        private static var errCanceled = errors.New("operation was canceled");        public static var ErrWriteToConnected = errors.New("use of WriteTo with pre-connected connection");

        // mapErr maps from the context errors to the historical internal net
        // error values.
        //
        // TODO(bradfitz): get rid of this after adjusting tests and making
        // context.DeadlineExceeded implement net.Error?
        private static error mapErr(error err)
        {

            if (err == context.Canceled) 
                return error.As(errCanceled);
            else if (err == context.DeadlineExceeded) 
                return error.As(poll.ErrTimeout);
            else 
                return error.As(err);
                    }

        // OpError is the error type usually returned by functions in the net
        // package. It describes the operation, network type, and address of
        // an error.
        public partial struct OpError
        {
            public @string Op; // Net is the network type on which this error occurred,
// such as "tcp" or "udp6".
            public @string Net; // For operations involving a remote network connection, like
// Dial, Read, or Write, Source is the corresponding local
// network address.
            public Addr Source; // Addr is the network address for which this error occurred.
// For local operations, like Listen or SetDeadline, Addr is
// the address of the local endpoint being manipulated.
// For operations involving a remote network connection, like
// Dial, Read, or Write, Addr is the remote address of that
// connection.
            public Addr Addr; // Err is the error that occurred during the operation.
            public error Err;
        }

        private static @string Error(this ref OpError e)
        {
            if (e == null)
            {
                return "<nil>";
            }
            var s = e.Op;
            if (e.Net != "")
            {
                s += " " + e.Net;
            }
            if (e.Source != null)
            {
                s += " " + e.Source.String();
            }
            if (e.Addr != null)
            {
                if (e.Source != null)
                {
                    s += "->";
                }
                else
                {
                    s += " ";
                }
                s += e.Addr.String();
            }
            s += ": " + e.Err.Error();
            return s;
        }

 
        // aLongTimeAgo is a non-zero time, far in the past, used for
        // immediate cancelation of dials.
        private static var aLongTimeAgo = time.Unix(1L, 0L);        private static time.Time noDeadline = new time.Time();        private static channel<object> noCancel = (channel<object>)null;

        private partial interface timeout
        {
            bool Timeout();
        }

        private static bool Timeout(this ref OpError e)
        {
            {
                ref os.SyscallError (ne, ok) = e.Err._<ref os.SyscallError>();

                if (ok)
                {
                    timeout (t, ok) = ne.Err._<timeout>();
                    return ok && t.Timeout();
                }

            }
            (t, ok) = e.Err._<timeout>();
            return ok && t.Timeout();
        }

        private partial interface temporary
        {
            bool Temporary();
        }

        private static bool Temporary(this ref OpError e)
        {
            {
                ref os.SyscallError (ne, ok) = e.Err._<ref os.SyscallError>();

                if (ok)
                {
                    temporary (t, ok) = ne.Err._<temporary>();
                    return ok && t.Temporary();
                }

            }
            (t, ok) = e.Err._<temporary>();
            return ok && t.Temporary();
        }

        // A ParseError is the error type of literal network address parsers.
        public partial struct ParseError
        {
            public @string Type; // Text is the malformed text string.
            public @string Text;
        }

        private static @string Error(this ref ParseError e)
        {
            return "invalid " + e.Type + ": " + e.Text;
        }

        public partial struct AddrError
        {
            public @string Err;
            public @string Addr;
        }

        private static @string Error(this ref AddrError e)
        {
            if (e == null)
            {
                return "<nil>";
            }
            var s = e.Err;
            if (e.Addr != "")
            {
                s = "address " + e.Addr + ": " + s;
            }
            return s;
        }

        private static bool Timeout(this ref AddrError e)
        {
            return false;
        }
        private static bool Temporary(this ref AddrError e)
        {
            return false;
        }

        public partial struct UnknownNetworkError // : @string
        {
        }

        public static @string Error(this UnknownNetworkError e)
        {
            return "unknown network " + string(e);
        }
        public static bool Timeout(this UnknownNetworkError e)
        {
            return false;
        }
        public static bool Temporary(this UnknownNetworkError e)
        {
            return false;
        }

        public partial struct InvalidAddrError // : @string
        {
        }

        public static @string Error(this InvalidAddrError e)
        {
            return string(e);
        }
        public static bool Timeout(this InvalidAddrError e)
        {
            return false;
        }
        public static bool Temporary(this InvalidAddrError e)
        {
            return false;
        }

        // DNSConfigError represents an error reading the machine's DNS configuration.
        // (No longer used; kept for compatibility.)
        public partial struct DNSConfigError
        {
            public error Err;
        }

        private static @string Error(this ref DNSConfigError e)
        {
            return "error reading DNS config: " + e.Err.Error();
        }
        private static bool Timeout(this ref DNSConfigError e)
        {
            return false;
        }
        private static bool Temporary(this ref DNSConfigError e)
        {
            return false;
        }

        // Various errors contained in DNSError.
        private static var errNoSuchHost = errors.New("no such host");

        // DNSError represents a DNS lookup error.
        public partial struct DNSError
        {
            public @string Err; // description of the error
            public @string Name; // name looked for
            public @string Server; // server used
            public bool IsTimeout; // if true, timed out; not all timeouts set this
            public bool IsTemporary; // if true, error is temporary; not all errors set this
        }

        private static @string Error(this ref DNSError e)
        {
            if (e == null)
            {
                return "<nil>";
            }
            @string s = "lookup " + e.Name;
            if (e.Server != "")
            {
                s += " on " + e.Server;
            }
            s += ": " + e.Err;
            return s;
        }

        // Timeout reports whether the DNS lookup is known to have timed out.
        // This is not always known; a DNS lookup may fail due to a timeout
        // and return a DNSError for which Timeout returns false.
        private static bool Timeout(this ref DNSError e)
        {
            return e.IsTimeout;
        }

        // Temporary reports whether the DNS error is known to be temporary.
        // This is not always known; a DNS lookup may fail due to a temporary
        // error and return a DNSError for which Temporary returns false.
        private static bool Temporary(this ref DNSError e)
        {
            return e.IsTimeout || e.IsTemporary;
        }

        private partial struct writerOnly : io.Writer
        {
            public ref io.Writer Writer => ref Writer_val;
        }

        // Fallback implementation of io.ReaderFrom's ReadFrom, when sendfile isn't
        // applicable.
        private static (long, error) genericReadFrom(io.Writer w, io.Reader r)
        { 
            // Use wrapper to hide existing r.ReadFrom from io.Copy.
            return io.Copy(new writerOnly(w), r);
        }

        // Limit the number of concurrent cgo-using goroutines, because
        // each will block an entire operating system thread. The usual culprit
        // is resolving many DNS names in separate goroutines but the DNS
        // server is not responding. Then the many lookups each use a different
        // thread, and the system or the program runs out of threads.

        private static var threadLimit = make_channel<object>(500L);

        private static void acquireThread()
        {
            threadLimit.Send(/* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{});
        }

        private static void releaseThread()
        {
            threadLimit.Receive();
        }

        // buffersWriter is the interface implemented by Conns that support a
        // "writev"-like batch write optimization.
        // writeBuffers should fully consume and write all chunks from the
        // provided Buffers, else it should report a non-nil error.
        private partial interface buffersWriter
        {
            (long, error) writeBuffers(ref Buffers _p0);
        }

        // Buffers contains zero or more runs of bytes to write.
        //
        // On certain machines, for certain types of connections, this is
        // optimized into an OS-specific batch write operation (such as
        // "writev").
        public partial struct Buffers // : slice<slice<byte>>
        {
        }

        private static io.WriterTo _ = (Buffers.Value)(null);        private static io.Reader _ = (Buffers.Value)(null);

        private static (long, error) WriteTo(this ref Buffers v, io.Writer w)
        {
            {
                buffersWriter (wv, ok) = w._<buffersWriter>();

                if (ok)
                {
                    return wv.writeBuffers(v);
                }

            }
            foreach (var (_, b) in v.Value)
            {
                var (nb, err) = w.Write(b);
                n += int64(nb);
                if (err != null)
                {
                    v.consume(n);
                    return (n, err);
                }
            }
            v.consume(n);
            return (n, null);
        }

        private static (long, error) Read(this ref Buffers v, slice<byte> p)
        {
            while (len(p) > 0L && len(v.Value) > 0L)
            {
                var n0 = copy(p, (v.Value)[0L]);
                v.consume(int64(n0));
                p = p[n0..];
                n += n0;
            }

            if (len(v.Value) == 0L)
            {
                err = io.EOF;
            }
            return;
        }

        private static void consume(this ref Buffers v, long n)
        {
            while (len(v.Value) > 0L)
            {
                var ln0 = int64(len((v.Value)[0L]));
                if (ln0 > n)
                {
                    (v.Value)[0L] = (v.Value)[0L][n..];
                    return;
                }
                n -= ln0;
                v.Value = (v.Value)[1L..];
            }

        }
    }
}
