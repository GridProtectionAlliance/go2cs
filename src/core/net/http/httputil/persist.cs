// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net.http;

using bufio = bufio_package;
using errors = errors_package;
using io = io_package;
using net = net_package;
using http = net.http_package;
using textproto = net.textproto_package;
using sync = sync_package;
using net;

partial class httputil_package {

public static ж<http.ProtocolError> ErrPersistEOF = Ꮡ(new http.ProtocolError(ErrorString: "persistent connection closed"u8));
public static ж<http.ProtocolError> ErrClosed = Ꮡ(new http.ProtocolError(ErrorString: "connection closed by user"u8));
public static ж<http.ProtocolError> ErrPipeline = Ꮡ(new http.ProtocolError(ErrorString: "pipeline error"u8));

// This is an API usage error - the local side is closed.
// ErrPersistEOF (above) reports that the remote side is closed.
internal static error errClosed = errors.New("i/o operation on closed connection"u8);

// ServerConn is an artifact of Go's early HTTP implementation.
// It is low-level, old, and unused by Go's current HTTP stack.
// We should have deleted it before Go 1.
//
// Deprecated: Use the Server in package [net/http] instead.
[GoType] partial struct ServerConn {
    internal sync_package.Mutex mu; // read-write protects the following fields
    internal net_package.Conn c;
    internal ж<bufio_package.Reader> r;
    internal error re; // read/write errors
    internal error we;
    internal io_package.ReadCloser lastbody;
    internal nint nread;
    internal nint nwritten;
    internal http.Request>uint pipereq;
    internal net.textproto_package.Pipeline pipe;
}

// NewServerConn is an artifact of Go's early HTTP implementation.
// It is low-level, old, and unused by Go's current HTTP stack.
// We should have deleted it before Go 1.
//
// Deprecated: Use the Server in package [net/http] instead.
public static ж<ServerConn> NewServerConn(net.Conn c, ж<bufio.Reader> Ꮡr) {
    ref var r = ref Ꮡr.val;

    if (r == nil) {
        r = bufio.NewReader(c);
    }
    return Ꮡ(new ServerConn(c: c, r: r, pipereq: new http.Request>uint()));
}

// Hijack detaches the [ServerConn] and returns the underlying connection as well
// as the read-side bufio which may have some left over data. Hijack may be
// called before Read has signaled the end of the keep-alive logic. The user
// should not call Hijack while [ServerConn.Read] or [ServerConn.Write] is in progress.
[GoRecv] public static (net.Conn, ж<bufio.Reader>) Hijack(this ref ServerConn sc) => func((defer, _) => {
    sc.mu.Lock();
    defer(sc.mu.Unlock);
    var c = sc.c;
    var r = sc.r;
    sc.c = default!;
    sc.r = default!;
    return (c, r);
});

// Close calls [ServerConn.Hijack] and then also closes the underlying connection.
[GoRecv] public static error Close(this ref ServerConn sc) {
    (c, _) = sc.Hijack();
    if (c != default!) {
        return c.Close();
    }
    return default!;
}

// Read returns the next request on the wire. An [ErrPersistEOF] is returned if
// it is gracefully determined that there are no more requests (e.g. after the
// first request on an HTTP/1.0 connection, or after a Connection:close on a
// HTTP/1.1 connection).
[GoRecv] public static (ж<http.Request>, error) Read(this ref ServerConn sc) => func((defer, _) => {
    ж<http.Request> req = default!;
    error err = default!;
    // Ensure ordered execution of Reads and Writes
    nuint id = sc.pipe.Next();
    sc.pipe.StartRequest(id);
    var reqʗ1 = req;
    defer(() => {
        sc.pipe.EndRequest(id);
        if (reqʗ1 == nil){
            sc.pipe.StartResponse(id);
            sc.pipe.EndResponse(id);
        } else {
            // Remember the pipeline id of this request
            sc.mu.Lock();
            sc.pipereq[reqʗ1] = id;
            sc.mu.Unlock();
        }
    });
    sc.mu.Lock();
    if (sc.we != default!) {
        // no point receiving if write-side broken or closed
        defer(sc.mu.Unlock);
        return (default!, sc.we);
    }
    if (sc.re != default!) {
        defer(sc.mu.Unlock);
        return (default!, sc.re);
    }
    if (sc.r == nil) {
        // connection closed by user in the meantime
        defer(sc.mu.Unlock);
        return (default!, errClosed);
    }
    var r = sc.r;
    var lastbody = sc.lastbody;
    sc.lastbody = default!;
    sc.mu.Unlock();
    // Make sure body is fully consumed, even if user does not call body.Close
    if (lastbody != default!) {
        // body.Close is assumed to be idempotent and multiple calls to
        // it should return the error that its first invocation
        // returned.
        err = lastbody.Close();
        if (err != default!) {
            sc.mu.Lock();
            defer(sc.mu.Unlock);
            sc.re = err;
            return (default!, err);
        }
    }
    (req, err) = http.ReadRequest(r);
    sc.mu.Lock();
    defer(sc.mu.Unlock);
    if (err != default!) {
        if (AreEqual(err, io.ErrUnexpectedEOF)){
            // A close from the opposing client is treated as a
            // graceful close, even if there was some unparse-able
            // data before the close.
            sc.re = ErrPersistEOF;
            return (default!, sc.re);
        } else {
            sc.re = err;
            return (req, err);
        }
    }
    sc.lastbody = req.val.Body;
    sc.nread++;
    if ((~req).Close) {
        sc.re = ErrPersistEOF;
        return (req, sc.re);
    }
    return (req, err);
});

// Pending returns the number of unanswered requests
// that have been received on the connection.
[GoRecv] public static nint Pending(this ref ServerConn sc) => func((defer, _) => {
    sc.mu.Lock();
    defer(sc.mu.Unlock);
    return sc.nread - sc.nwritten;
});

// Write writes resp in response to req. To close the connection gracefully, set the
// Response.Close field to true. Write should be considered operational until
// it returns an error, regardless of any errors returned on the [ServerConn.Read] side.
[GoRecv] public static error Write(this ref ServerConn sc, ж<http.Request> Ꮡreq, ж<http.Response> Ꮡresp) => func((defer, _) => {
    ref var req = ref Ꮡreq.val;
    ref var resp = ref Ꮡresp.val;

    // Retrieve the pipeline ID of this request/response pair
    sc.mu.Lock();
    nuint id = sc.pipereq[req];
    var ok = sc.pipereq[req];
    delete(sc.pipereq, Ꮡreq);
    if (!ok) {
        sc.mu.Unlock();
        return ~ErrPipeline;
    }
    sc.mu.Unlock();
    // Ensure pipeline order
    sc.pipe.StartResponse(id);
    deferǃ(sc.pipe.EndResponse, id, defer);
    sc.mu.Lock();
    if (sc.we != default!) {
        defer(sc.mu.Unlock);
        return sc.we;
    }
    if (sc.c == default!) {
        // connection closed by user in the meantime
        defer(sc.mu.Unlock);
        return ~ErrClosed;
    }
    var c = sc.c;
    if (sc.nread <= sc.nwritten) {
        defer(sc.mu.Unlock);
        return errors.New("persist server pipe count"u8);
    }
    if (resp.Close) {
        // After signaling a keep-alive close, any pipelined unread
        // requests will be lost. It is up to the user to drain them
        // before signaling.
        sc.re = ErrPersistEOF;
    }
    sc.mu.Unlock();
    var err = resp.Write(c);
    sc.mu.Lock();
    defer(sc.mu.Unlock);
    if (err != default!) {
        sc.we = err;
        return err;
    }
    sc.nwritten++;
    return default!;
});

// ClientConn is an artifact of Go's early HTTP implementation.
// It is low-level, old, and unused by Go's current HTTP stack.
// We should have deleted it before Go 1.
//
// Deprecated: Use Client or Transport in package [net/http] instead.
[GoType] partial struct ClientConn {
    internal sync_package.Mutex mu; // read-write protects the following fields
    internal net_package.Conn c;
    internal ж<bufio_package.Reader> r;
    internal error re; // read/write errors
    internal error we;
    internal io_package.ReadCloser lastbody;
    internal nint nread;
    internal nint nwritten;
    internal http.Request>uint pipereq;
    internal net.textproto_package.Pipeline pipe;
    internal http.Request, io.Writer) error writeReq;
}

// NewClientConn is an artifact of Go's early HTTP implementation.
// It is low-level, old, and unused by Go's current HTTP stack.
// We should have deleted it before Go 1.
//
// Deprecated: Use the Client or Transport in package [net/http] instead.
public static ж<ClientConn> NewClientConn(net.Conn c, ж<bufio.Reader> Ꮡr) {
    ref var r = ref Ꮡr.val;

    if (r == nil) {
        r = bufio.NewReader(c);
    }
    return Ꮡ(new ClientConn(
        c: c,
        r: r,
        pipereq: new http.Request>uint(),
        writeReq: (ж<http.Request>).Write
    ));
}

// NewProxyClientConn is an artifact of Go's early HTTP implementation.
// It is low-level, old, and unused by Go's current HTTP stack.
// We should have deleted it before Go 1.
//
// Deprecated: Use the Client or Transport in package [net/http] instead.
public static ж<ClientConn> NewProxyClientConn(net.Conn c, ж<bufio.Reader> Ꮡr) {
    ref var r = ref Ꮡr.val;

    var cc = NewClientConn(c, Ꮡr);
    cc.val.writeReq = () => (ж<http.Request>).WriteProxy();
    return cc;
}

// Hijack detaches the [ClientConn] and returns the underlying connection as well
// as the read-side bufio which may have some left over data. Hijack may be
// called before the user or Read have signaled the end of the keep-alive
// logic. The user should not call Hijack while [ClientConn.Read] or ClientConn.Write is in progress.
[GoRecv] public static (net.Conn c, ж<bufio.Reader> r) Hijack(this ref ClientConn cc) => func((defer, _) => {
    net.Conn c = default!;
    ж<bufio.Reader> r = default!;

    cc.mu.Lock();
    defer(cc.mu.Unlock);
    c = cc.c;
    r = cc.r;
    cc.c = default!;
    cc.r = default!;
    return (c, r);
});

// Close calls [ClientConn.Hijack] and then also closes the underlying connection.
[GoRecv] public static error Close(this ref ClientConn cc) {
    (c, _) = cc.Hijack();
    if (c != default!) {
        return c.Close();
    }
    return default!;
}

// Write writes a request. An [ErrPersistEOF] error is returned if the connection
// has been closed in an HTTP keep-alive sense. If req.Close equals true, the
// keep-alive connection is logically closed after this request and the opposing
// server is informed. An ErrUnexpectedEOF indicates the remote closed the
// underlying TCP connection, which is usually considered as graceful close.
[GoRecv] public static error Write(this ref ClientConn cc, ж<http.Request> Ꮡreq) => func((defer, _) => {
    ref var req = ref Ꮡreq.val;

    error err = default!;
    // Ensure ordered execution of Writes
    nuint id = cc.pipe.Next();
    cc.pipe.StartRequest(id);
    var errʗ1 = err;
    defer(() => {
        cc.pipe.EndRequest(id);
        if (errʗ1 != default!){
            cc.pipe.StartResponse(id);
            cc.pipe.EndResponse(id);
        } else {
            // Remember the pipeline id of this request
            cc.mu.Lock();
            cc.pipereq[req] = id;
            cc.mu.Unlock();
        }
    });
    cc.mu.Lock();
    if (cc.re != default!) {
        // no point sending if read-side closed or broken
        defer(cc.mu.Unlock);
        return cc.re;
    }
    if (cc.we != default!) {
        defer(cc.mu.Unlock);
        return cc.we;
    }
    if (cc.c == default!) {
        // connection closed by user in the meantime
        defer(cc.mu.Unlock);
        return errClosed;
    }
    var c = cc.c;
    if (req.Close) {
        // We write the EOF to the write-side error, because there
        // still might be some pipelined reads
        cc.we = ErrPersistEOF;
    }
    cc.mu.Unlock();
    err = cc.writeReq(req, c);
    cc.mu.Lock();
    defer(cc.mu.Unlock);
    if (err != default!) {
        cc.we = err;
        return err;
    }
    cc.nwritten++;
    return default!;
});

// Pending returns the number of unanswered requests
// that have been sent on the connection.
[GoRecv] public static nint Pending(this ref ClientConn cc) => func((defer, _) => {
    cc.mu.Lock();
    defer(cc.mu.Unlock);
    return cc.nwritten - cc.nread;
});

// Read reads the next response from the wire. A valid response might be
// returned together with an [ErrPersistEOF], which means that the remote
// requested that this be the last request serviced. Read can be called
// concurrently with [ClientConn.Write], but not with another Read.
[GoRecv] public static (ж<http.Response> resp, error err) Read(this ref ClientConn cc, ж<http.Request> Ꮡreq) => func((defer, _) => {
    ж<http.Response> resp = default!;
    error err = default!;

    ref var req = ref Ꮡreq.val;
    // Retrieve the pipeline ID of this request/response pair
    cc.mu.Lock();
    nuint id = cc.pipereq[req];
    var ok = cc.pipereq[req];
    delete(cc.pipereq, Ꮡreq);
    if (!ok) {
        cc.mu.Unlock();
        return (default!, ~ErrPipeline);
    }
    cc.mu.Unlock();
    // Ensure pipeline order
    cc.pipe.StartResponse(id);
    deferǃ(cc.pipe.EndResponse, id, defer);
    cc.mu.Lock();
    if (cc.re != default!) {
        defer(cc.mu.Unlock);
        return (default!, cc.re);
    }
    if (cc.r == nil) {
        // connection closed by user in the meantime
        defer(cc.mu.Unlock);
        return (default!, errClosed);
    }
    var r = cc.r;
    var lastbody = cc.lastbody;
    cc.lastbody = default!;
    cc.mu.Unlock();
    // Make sure body is fully consumed, even if user does not call body.Close
    if (lastbody != default!) {
        // body.Close is assumed to be idempotent and multiple calls to
        // it should return the error that its first invocation
        // returned.
        err = lastbody.Close();
        if (err != default!) {
            cc.mu.Lock();
            defer(cc.mu.Unlock);
            cc.re = err;
            return (default!, err);
        }
    }
    (resp, err) = http.ReadResponse(r, Ꮡreq);
    cc.mu.Lock();
    defer(cc.mu.Unlock);
    if (err != default!) {
        cc.re = err;
        return (resp, err);
    }
    cc.lastbody = resp.val.Body;
    cc.nread++;
    if ((~resp).Close) {
        cc.re = ErrPersistEOF;
        // don't send any more requests
        return (resp, cc.re);
    }
    return (resp, err);
});

// Do is convenience method that writes a request and reads a response.
[GoRecv] public static (ж<http.Response>, error) Do(this ref ClientConn cc, ж<http.Request> Ꮡreq) {
    ref var req = ref Ꮡreq.val;

    var err = cc.Write(Ꮡreq);
    if (err != default!) {
        return (default!, err);
    }
    return cc.Read(Ꮡreq);
}

} // end httputil_package
