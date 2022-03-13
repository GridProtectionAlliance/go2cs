// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package httputil -- go2cs converted at 2022 March 13 05:38:28 UTC
// import "net/http/httputil" ==> using httputil = go.net.http.httputil_package
// Original source: C:\Program Files\Go\src\net\http\httputil\persist.go
namespace go.net.http;

using bufio = bufio_package;
using errors = errors_package;
using io = io_package;
using net = net_package;
using http = net.http_package;
using textproto = net.textproto_package;
using sync = sync_package;
using System;

public static partial class httputil_package {

 
// Deprecated: No longer used.
public static ptr<http.ProtocolError> ErrPersistEOF = addr(new http.ProtocolError(ErrorString:"persistent connection closed"));public static ptr<http.ProtocolError> ErrClosed = addr(new http.ProtocolError(ErrorString:"connection closed by user"));public static ptr<http.ProtocolError> ErrPipeline = addr(new http.ProtocolError(ErrorString:"pipeline error"));

// This is an API usage error - the local side is closed.
// ErrPersistEOF (above) reports that the remote side is closed.
private static var errClosed = errors.New("i/o operation on closed connection");

// ServerConn is an artifact of Go's early HTTP implementation.
// It is low-level, old, and unused by Go's current HTTP stack.
// We should have deleted it before Go 1.
//
// Deprecated: Use the Server in package net/http instead.
public partial struct ServerConn {
    public sync.Mutex mu; // read-write protects the following fields
    public net.Conn c;
    public ptr<bufio.Reader> r;
    public error re; // read/write errors
    public error we; // read/write errors
    public io.ReadCloser lastbody;
    public nint nread;
    public nint nwritten;
    public map<ptr<http.Request>, nuint> pipereq;
    public textproto.Pipeline pipe;
}

// NewServerConn is an artifact of Go's early HTTP implementation.
// It is low-level, old, and unused by Go's current HTTP stack.
// We should have deleted it before Go 1.
//
// Deprecated: Use the Server in package net/http instead.
public static ptr<ServerConn> NewServerConn(net.Conn c, ptr<bufio.Reader> _addr_r) {
    ref bufio.Reader r = ref _addr_r.val;

    if (r == null) {
        r = bufio.NewReader(c);
    }
    return addr(new ServerConn(c:c,r:r,pipereq:make(map[*http.Request]uint)));
}

// Hijack detaches the ServerConn and returns the underlying connection as well
// as the read-side bufio which may have some left over data. Hijack may be
// called before Read has signaled the end of the keep-alive logic. The user
// should not call Hijack while Read or Write is in progress.
private static (net.Conn, ptr<bufio.Reader>) Hijack(this ptr<ServerConn> _addr_sc) => func((defer, _, _) => {
    net.Conn _p0 = default;
    ptr<bufio.Reader> _p0 = default!;
    ref ServerConn sc = ref _addr_sc.val;

    sc.mu.Lock();
    defer(sc.mu.Unlock());
    var c = sc.c;
    var r = sc.r;
    sc.c = null;
    sc.r = null;
    return (c, _addr_r!);
});

// Close calls Hijack and then also closes the underlying connection.
private static error Close(this ptr<ServerConn> _addr_sc) {
    ref ServerConn sc = ref _addr_sc.val;

    var (c, _) = sc.Hijack();
    if (c != null) {
        return error.As(c.Close())!;
    }
    return error.As(null!)!;
}

// Read returns the next request on the wire. An ErrPersistEOF is returned if
// it is gracefully determined that there are no more requests (e.g. after the
// first request on an HTTP/1.0 connection, or after a Connection:close on a
// HTTP/1.1 connection).
private static (ptr<http.Request>, error) Read(this ptr<ServerConn> _addr_sc) => func((defer, _, _) => {
    ptr<http.Request> _p0 = default!;
    error _p0 = default!;
    ref ServerConn sc = ref _addr_sc.val;

    ptr<http.Request> req;
    error err = default!; 

    // Ensure ordered execution of Reads and Writes
    var id = sc.pipe.Next();
    sc.pipe.StartRequest(id);
    defer(() => {
        sc.pipe.EndRequest(id);
        if (req == null) {
            sc.pipe.StartResponse(id);
            sc.pipe.EndResponse(id);
        }
        else
 { 
            // Remember the pipeline id of this request
            sc.mu.Lock();
            sc.pipereq[req] = id;
            sc.mu.Unlock();
        }
    }());

    sc.mu.Lock();
    if (sc.we != null) { // no point receiving if write-side broken or closed
        defer(sc.mu.Unlock());
        return (_addr_null!, error.As(sc.we)!);
    }
    if (sc.re != null) {
        defer(sc.mu.Unlock());
        return (_addr_null!, error.As(sc.re)!);
    }
    if (sc.r == null) { // connection closed by user in the meantime
        defer(sc.mu.Unlock());
        return (_addr_null!, error.As(errClosed)!);
    }
    var r = sc.r;
    var lastbody = sc.lastbody;
    sc.lastbody = null;
    sc.mu.Unlock(); 

    // Make sure body is fully consumed, even if user does not call body.Close
    if (lastbody != null) { 
        // body.Close is assumed to be idempotent and multiple calls to
        // it should return the error that its first invocation
        // returned.
        err = error.As(lastbody.Close())!;
        if (err != null) {
            sc.mu.Lock();
            defer(sc.mu.Unlock());
            sc.re = err;
            return (_addr_null!, error.As(err)!);
        }
    }
    req, err = http.ReadRequest(r);
    sc.mu.Lock();
    defer(sc.mu.Unlock());
    if (err != null) {
        if (err == io.ErrUnexpectedEOF) { 
            // A close from the opposing client is treated as a
            // graceful close, even if there was some unparse-able
            // data before the close.
            sc.re = ErrPersistEOF;
            return (_addr_null!, error.As(sc.re)!);
        }
        else
 {
            sc.re = err;
            return (_addr_req!, error.As(err)!);
        }
    }
    sc.lastbody = req.Body;
    sc.nread++;
    if (req.Close) {
        sc.re = ErrPersistEOF;
        return (_addr_req!, error.As(sc.re)!);
    }
    return (_addr_req!, error.As(err)!);
});

// Pending returns the number of unanswered requests
// that have been received on the connection.
private static nint Pending(this ptr<ServerConn> _addr_sc) => func((defer, _, _) => {
    ref ServerConn sc = ref _addr_sc.val;

    sc.mu.Lock();
    defer(sc.mu.Unlock());
    return sc.nread - sc.nwritten;
});

// Write writes resp in response to req. To close the connection gracefully, set the
// Response.Close field to true. Write should be considered operational until
// it returns an error, regardless of any errors returned on the Read side.
private static error Write(this ptr<ServerConn> _addr_sc, ptr<http.Request> _addr_req, ptr<http.Response> _addr_resp) => func((defer, _, _) => {
    ref ServerConn sc = ref _addr_sc.val;
    ref http.Request req = ref _addr_req.val;
    ref http.Response resp = ref _addr_resp.val;

    // Retrieve the pipeline ID of this request/response pair
    sc.mu.Lock();
    var (id, ok) = sc.pipereq[req];
    delete(sc.pipereq, req);
    if (!ok) {
        sc.mu.Unlock();
        return error.As(ErrPipeline)!;
    }
    sc.mu.Unlock(); 

    // Ensure pipeline order
    sc.pipe.StartResponse(id);
    defer(sc.pipe.EndResponse(id));

    sc.mu.Lock();
    if (sc.we != null) {
        defer(sc.mu.Unlock());
        return error.As(sc.we)!;
    }
    if (sc.c == null) { // connection closed by user in the meantime
        defer(sc.mu.Unlock());
        return error.As(ErrClosed)!;
    }
    var c = sc.c;
    if (sc.nread <= sc.nwritten) {
        defer(sc.mu.Unlock());
        return error.As(errors.New("persist server pipe count"))!;
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
    defer(sc.mu.Unlock());
    if (err != null) {
        sc.we = err;
        return error.As(err)!;
    }
    sc.nwritten++;

    return error.As(null!)!;
});

// ClientConn is an artifact of Go's early HTTP implementation.
// It is low-level, old, and unused by Go's current HTTP stack.
// We should have deleted it before Go 1.
//
// Deprecated: Use Client or Transport in package net/http instead.
public partial struct ClientConn {
    public sync.Mutex mu; // read-write protects the following fields
    public net.Conn c;
    public ptr<bufio.Reader> r;
    public error re; // read/write errors
    public error we; // read/write errors
    public io.ReadCloser lastbody;
    public nint nread;
    public nint nwritten;
    public map<ptr<http.Request>, nuint> pipereq;
    public textproto.Pipeline pipe;
    public Func<ptr<http.Request>, io.Writer, error> writeReq;
}

// NewClientConn is an artifact of Go's early HTTP implementation.
// It is low-level, old, and unused by Go's current HTTP stack.
// We should have deleted it before Go 1.
//
// Deprecated: Use the Client or Transport in package net/http instead.
public static ptr<ClientConn> NewClientConn(net.Conn c, ptr<bufio.Reader> _addr_r) {
    ref bufio.Reader r = ref _addr_r.val;

    if (r == null) {
        r = bufio.NewReader(c);
    }
    return addr(new ClientConn(c:c,r:r,pipereq:make(map[*http.Request]uint),writeReq:(*http.Request).Write,));
}

// NewProxyClientConn is an artifact of Go's early HTTP implementation.
// It is low-level, old, and unused by Go's current HTTP stack.
// We should have deleted it before Go 1.
//
// Deprecated: Use the Client or Transport in package net/http instead.
public static ptr<ClientConn> NewProxyClientConn(net.Conn c, ptr<bufio.Reader> _addr_r) {
    ref bufio.Reader r = ref _addr_r.val;

    var cc = NewClientConn(c, _addr_r);
    cc.writeReq = (http.Request.val).WriteProxy;
    return _addr_cc!;
}

// Hijack detaches the ClientConn and returns the underlying connection as well
// as the read-side bufio which may have some left over data. Hijack may be
// called before the user or Read have signaled the end of the keep-alive
// logic. The user should not call Hijack while Read or Write is in progress.
private static (net.Conn, ptr<bufio.Reader>) Hijack(this ptr<ClientConn> _addr_cc) => func((defer, _, _) => {
    net.Conn c = default;
    ptr<bufio.Reader> r = default!;
    ref ClientConn cc = ref _addr_cc.val;

    cc.mu.Lock();
    defer(cc.mu.Unlock());
    c = cc.c;
    r = cc.r;
    cc.c = null;
    cc.r = null;
    return ;
});

// Close calls Hijack and then also closes the underlying connection.
private static error Close(this ptr<ClientConn> _addr_cc) {
    ref ClientConn cc = ref _addr_cc.val;

    var (c, _) = cc.Hijack();
    if (c != null) {
        return error.As(c.Close())!;
    }
    return error.As(null!)!;
}

// Write writes a request. An ErrPersistEOF error is returned if the connection
// has been closed in an HTTP keep-alive sense. If req.Close equals true, the
// keep-alive connection is logically closed after this request and the opposing
// server is informed. An ErrUnexpectedEOF indicates the remote closed the
// underlying TCP connection, which is usually considered as graceful close.
private static error Write(this ptr<ClientConn> _addr_cc, ptr<http.Request> _addr_req) => func((defer, _, _) => {
    ref ClientConn cc = ref _addr_cc.val;
    ref http.Request req = ref _addr_req.val;

    error err = default!; 

    // Ensure ordered execution of Writes
    var id = cc.pipe.Next();
    cc.pipe.StartRequest(id);
    defer(() => {
        cc.pipe.EndRequest(id);
        if (err != null) {
            cc.pipe.StartResponse(id);
            cc.pipe.EndResponse(id);
        }
        else
 { 
            // Remember the pipeline id of this request
            cc.mu.Lock();
            cc.pipereq[req] = id;
            cc.mu.Unlock();
        }
    }());

    cc.mu.Lock();
    if (cc.re != null) { // no point sending if read-side closed or broken
        defer(cc.mu.Unlock());
        return error.As(cc.re)!;
    }
    if (cc.we != null) {
        defer(cc.mu.Unlock());
        return error.As(cc.we)!;
    }
    if (cc.c == null) { // connection closed by user in the meantime
        defer(cc.mu.Unlock());
        return error.As(errClosed)!;
    }
    var c = cc.c;
    if (req.Close) { 
        // We write the EOF to the write-side error, because there
        // still might be some pipelined reads
        cc.we = ErrPersistEOF;
    }
    cc.mu.Unlock();

    err = error.As(cc.writeReq(req, c))!;
    cc.mu.Lock();
    defer(cc.mu.Unlock());
    if (err != null) {
        cc.we = err;
        return error.As(err)!;
    }
    cc.nwritten++;

    return error.As(null!)!;
});

// Pending returns the number of unanswered requests
// that have been sent on the connection.
private static nint Pending(this ptr<ClientConn> _addr_cc) => func((defer, _, _) => {
    ref ClientConn cc = ref _addr_cc.val;

    cc.mu.Lock();
    defer(cc.mu.Unlock());
    return cc.nwritten - cc.nread;
});

// Read reads the next response from the wire. A valid response might be
// returned together with an ErrPersistEOF, which means that the remote
// requested that this be the last request serviced. Read can be called
// concurrently with Write, but not with another Read.
private static (ptr<http.Response>, error) Read(this ptr<ClientConn> _addr_cc, ptr<http.Request> _addr_req) => func((defer, _, _) => {
    ptr<http.Response> resp = default!;
    error err = default!;
    ref ClientConn cc = ref _addr_cc.val;
    ref http.Request req = ref _addr_req.val;
 
    // Retrieve the pipeline ID of this request/response pair
    cc.mu.Lock();
    var (id, ok) = cc.pipereq[req];
    delete(cc.pipereq, req);
    if (!ok) {
        cc.mu.Unlock();
        return (_addr_null!, error.As(ErrPipeline)!);
    }
    cc.mu.Unlock(); 

    // Ensure pipeline order
    cc.pipe.StartResponse(id);
    defer(cc.pipe.EndResponse(id));

    cc.mu.Lock();
    if (cc.re != null) {
        defer(cc.mu.Unlock());
        return (_addr_null!, error.As(cc.re)!);
    }
    if (cc.r == null) { // connection closed by user in the meantime
        defer(cc.mu.Unlock());
        return (_addr_null!, error.As(errClosed)!);
    }
    var r = cc.r;
    var lastbody = cc.lastbody;
    cc.lastbody = null;
    cc.mu.Unlock(); 

    // Make sure body is fully consumed, even if user does not call body.Close
    if (lastbody != null) { 
        // body.Close is assumed to be idempotent and multiple calls to
        // it should return the error that its first invocation
        // returned.
        err = lastbody.Close();
        if (err != null) {
            cc.mu.Lock();
            defer(cc.mu.Unlock());
            cc.re = err;
            return (_addr_null!, error.As(err)!);
        }
    }
    resp, err = http.ReadResponse(r, req);
    cc.mu.Lock();
    defer(cc.mu.Unlock());
    if (err != null) {
        cc.re = err;
        return (_addr_resp!, error.As(err)!);
    }
    cc.lastbody = resp.Body;

    cc.nread++;

    if (resp.Close) {
        cc.re = ErrPersistEOF; // don't send any more requests
        return (_addr_resp!, error.As(cc.re)!);
    }
    return (_addr_resp!, error.As(err)!);
});

// Do is convenience method that writes a request and reads a response.
private static (ptr<http.Response>, error) Do(this ptr<ClientConn> _addr_cc, ptr<http.Request> _addr_req) {
    ptr<http.Response> _p0 = default!;
    error _p0 = default!;
    ref ClientConn cc = ref _addr_cc.val;
    ref http.Request req = ref _addr_req.val;

    var err = cc.Write(req);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    return _addr_cc.Read(req)!;
}

} // end httputil_package
