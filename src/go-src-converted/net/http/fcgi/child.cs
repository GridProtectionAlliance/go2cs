// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package fcgi -- go2cs converted at 2022 March 13 05:38:17 UTC
// import "net/http/fcgi" ==> using fcgi = go.net.http.fcgi_package
// Original source: C:\Program Files\Go\src\net\http\fcgi\child.go
namespace go.net.http;
// This file implements FastCGI from the perspective of a child process.


using context = context_package;
using errors = errors_package;
using fmt = fmt_package;
using io = io_package;
using net = net_package;
using http = net.http_package;
using cgi = net.http.cgi_package;
using os = os_package;
using strings = strings_package;
using time = time_package;


// request holds the state for an in-progress request. As soon as it's complete,
// it's converted to an http.Request.

using System.Threading;
public static partial class fcgi_package {

private partial struct request {
    public ptr<io.PipeWriter> pw;
    public ushort reqId;
    public map<@string, @string> @params;
    public array<byte> buf;
    public slice<byte> rawParams;
    public bool keepConn;
}

// envVarsContextKey uniquely identifies a mapping of CGI
// environment variables to their values in a request context
private partial struct envVarsContextKey {
}

private static ptr<request> newRequest(ushort reqId, byte flags) {
    ptr<request> r = addr(new request(reqId:reqId,params:map[string]string{},keepConn:flags&flagKeepConn!=0,));
    r.rawParams = r.buf[..(int)0];
    return _addr_r!;
}

// parseParams reads an encoded []byte into Params.
private static void parseParams(this ptr<request> _addr_r) {
    ref request r = ref _addr_r.val;

    var text = r.rawParams;
    r.rawParams = null;
    while (len(text) > 0) {
        var (keyLen, n) = readSize(text);
        if (n == 0) {
            return ;
        }
        text = text[(int)n..];
        var (valLen, n) = readSize(text);
        if (n == 0) {
            return ;
        }
        text = text[(int)n..];
        if (int(keyLen) + int(valLen) > len(text)) {
            return ;
        }
        var key = readString(text, keyLen);
        text = text[(int)keyLen..];
        var val = readString(text, valLen);
        text = text[(int)valLen..];
        r.@params[key] = val;
    }
}

// response implements http.ResponseWriter.
private partial struct response {
    public ptr<request> req;
    public http.Header header;
    public nint code;
    public bool wroteHeader;
    public bool wroteCGIHeader;
    public ptr<bufWriter> w;
}

private static ptr<response> newResponse(ptr<child> _addr_c, ptr<request> _addr_req) {
    ref child c = ref _addr_c.val;
    ref request req = ref _addr_req.val;

    return addr(new response(req:req,header:http.Header{},w:newWriter(c.conn,typeStdout,req.reqId),));
}

private static http.Header Header(this ptr<response> _addr_r) {
    ref response r = ref _addr_r.val;

    return r.header;
}

private static (nint, error) Write(this ptr<response> _addr_r, slice<byte> p) {
    nint n = default;
    error err = default!;
    ref response r = ref _addr_r.val;

    if (!r.wroteHeader) {
        r.WriteHeader(http.StatusOK);
    }
    if (!r.wroteCGIHeader) {
        r.writeCGIHeader(p);
    }
    return r.w.Write(p);
}

private static void WriteHeader(this ptr<response> _addr_r, nint code) {
    ref response r = ref _addr_r.val;

    if (r.wroteHeader) {
        return ;
    }
    r.wroteHeader = true;
    r.code = code;
    if (code == http.StatusNotModified) { 
        // Must not have body.
        r.header.Del("Content-Type");
        r.header.Del("Content-Length");
        r.header.Del("Transfer-Encoding");
    }
    if (r.header.Get("Date") == "") {
        r.header.Set("Date", time.Now().UTC().Format(http.TimeFormat));
    }
}

// writeCGIHeader finalizes the header sent to the client and writes it to the output.
// p is not written by writeHeader, but is the first chunk of the body
// that will be written. It is sniffed for a Content-Type if none is
// set explicitly.
private static void writeCGIHeader(this ptr<response> _addr_r, slice<byte> p) {
    ref response r = ref _addr_r.val;

    if (r.wroteCGIHeader) {
        return ;
    }
    r.wroteCGIHeader = true;
    fmt.Fprintf(r.w, "Status: %d %s\r\n", r.code, http.StatusText(r.code));
    {
        var (_, hasType) = r.header["Content-Type"];

        if (r.code != http.StatusNotModified && !hasType) {
            r.header.Set("Content-Type", http.DetectContentType(p));
        }
    }
    r.header.Write(r.w);
    r.w.WriteString("\r\n");
    r.w.Flush();
}

private static void Flush(this ptr<response> _addr_r) {
    ref response r = ref _addr_r.val;

    if (!r.wroteHeader) {
        r.WriteHeader(http.StatusOK);
    }
    r.w.Flush();
}

private static error Close(this ptr<response> _addr_r) {
    ref response r = ref _addr_r.val;

    r.Flush();
    return error.As(r.w.Close())!;
}

private partial struct child {
    public ptr<conn> conn;
    public http.Handler handler;
    public map<ushort, ptr<request>> requests; // keyed by request ID
}

private static ptr<child> newChild(io.ReadWriteCloser rwc, http.Handler handler) {
    return addr(new child(conn:newConn(rwc),handler:handler,requests:make(map[uint16]*request),));
}

private static void serve(this ptr<child> _addr_c) => func((defer, _, _) => {
    ref child c = ref _addr_c.val;

    defer(c.conn.Close());
    defer(c.cleanUp());
    ref record rec = ref heap(out ptr<record> _addr_rec);
    while (true) {
        {
            var err__prev1 = err;

            var err = rec.read(c.conn.rwc);

            if (err != null) {
                return ;
            }

            err = err__prev1;

        }
        {
            var err__prev1 = err;

            err = c.handleRecord(_addr_rec);

            if (err != null) {
                return ;
            }

            err = err__prev1;

        }
    }
});

private static var errCloseConn = errors.New("fcgi: connection should be closed");

private static var emptyBody = io.NopCloser(strings.NewReader(""));

// ErrRequestAborted is returned by Read when a handler attempts to read the
// body of a request that has been aborted by the web server.
public static var ErrRequestAborted = errors.New("fcgi: request aborted by web server");

// ErrConnClosed is returned by Read when a handler attempts to read the body of
// a request after the connection to the web server has been closed.
public static var ErrConnClosed = errors.New("fcgi: connection to web server closed");

private static error handleRecord(this ptr<child> _addr_c, ptr<record> _addr_rec) {
    ref child c = ref _addr_c.val;
    ref record rec = ref _addr_rec.val;

    var (req, ok) = c.requests[rec.h.Id];
    if (!ok && rec.h.Type != typeBeginRequest && rec.h.Type != typeGetValues) { 
        // The spec says to ignore unknown request IDs.
        return error.As(null!)!;
    }

    if (rec.h.Type == typeBeginRequest) 
        if (req != null) { 
            // The server is trying to begin a request with the same ID
            // as an in-progress request. This is an error.
            return error.As(errors.New("fcgi: received ID that is already in-flight"))!;
        }
        beginRequest br = default;
        {
            var err = br.read(rec.content());

            if (err != null) {
                return error.As(err)!;
            }

        }
        if (br.role != roleResponder) {
            c.conn.writeEndRequest(rec.h.Id, 0, statusUnknownRole);
            return error.As(null!)!;
        }
        req = newRequest(rec.h.Id, br.flags);
        c.requests[rec.h.Id] = req;
        return error.As(null!)!;
    else if (rec.h.Type == typeParams) 
        // NOTE(eds): Technically a key-value pair can straddle the boundary
        // between two packets. We buffer until we've received all parameters.
        if (len(rec.content()) > 0) {
            req.rawParams = append(req.rawParams, rec.content());
            return error.As(null!)!;
        }
        req.parseParams();
        return error.As(null!)!;
    else if (rec.h.Type == typeStdin) 
        var content = rec.content();
        if (req.pw == null) {
            io.ReadCloser body = default;
            if (len(content) > 0) { 
                // body could be an io.LimitReader, but it shouldn't matter
                // as long as both sides are behaving.
                body, req.pw = io.Pipe();
            }
            else
 {
                body = emptyBody;
            }
            go_(() => c.serveRequest(req, body));
        }
        if (len(content) > 0) { 
            // TODO(eds): This blocks until the handler reads from the pipe.
            // If the handler takes a long time, it might be a problem.
            req.pw.Write(content);
        }
        else
 {
            delete(c.requests, req.reqId);
            if (req.pw != null) {
                req.pw.Close();
            }
        }
        return error.As(null!)!;
    else if (rec.h.Type == typeGetValues) 
        map values = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, @string>{"FCGI_MPXS_CONNS":"1"};
        c.conn.writePairs(typeGetValuesResult, 0, values);
        return error.As(null!)!;
    else if (rec.h.Type == typeData) 
        // If the filter role is implemented, read the data stream here.
        return error.As(null!)!;
    else if (rec.h.Type == typeAbortRequest) 
        delete(c.requests, rec.h.Id);
        c.conn.writeEndRequest(rec.h.Id, 0, statusRequestComplete);
        if (req.pw != null) {
            req.pw.CloseWithError(ErrRequestAborted);
        }
        if (!req.keepConn) { 
            // connection will close upon return
            return error.As(errCloseConn)!;
        }
        return error.As(null!)!;
    else 
        var b = make_slice<byte>(8);
        b[0] = byte(rec.h.Type);
        c.conn.writeRecord(typeUnknownType, 0, b);
        return error.As(null!)!;
    }

// filterOutUsedEnvVars returns a new map of env vars without the
// variables in the given envVars map that are read for creating each http.Request
private static map<@string, @string> filterOutUsedEnvVars(map<@string, @string> envVars) {
    var withoutUsedEnvVars = make_map<@string, @string>();
    foreach (var (k, v) in envVars) {
        if (addFastCGIEnvToContext(k)) {
            withoutUsedEnvVars[k] = v;
        }
    }    return withoutUsedEnvVars;
}

private static void serveRequest(this ptr<child> _addr_c, ptr<request> _addr_req, io.ReadCloser body) {
    ref child c = ref _addr_c.val;
    ref request req = ref _addr_req.val;

    var r = newResponse(_addr_c, _addr_req);
    var (httpReq, err) = cgi.RequestFromMap(req.@params);
    if (err != null) { 
        // there was an error reading the request
        r.WriteHeader(http.StatusInternalServerError);
        c.conn.writeRecord(typeStderr, req.reqId, (slice<byte>)err.Error());
    }
    else
 {
        httpReq.Body = body;
        var withoutUsedEnvVars = filterOutUsedEnvVars(req.@params);
        var envVarCtx = context.WithValue(httpReq.Context(), new envVarsContextKey(), withoutUsedEnvVars);
        httpReq = httpReq.WithContext(envVarCtx);
        c.handler.ServeHTTP(r, httpReq);
    }
    r.Write(null);
    r.Close();
    c.conn.writeEndRequest(req.reqId, 0, statusRequestComplete); 

    // Consume the entire body, so the host isn't still writing to
    // us when we close the socket below in the !keepConn case,
    // otherwise we'd send a RST. (golang.org/issue/4183)
    // TODO(bradfitz): also bound this copy in time. Or send
    // some sort of abort request to the host, so the host
    // can properly cut off the client sending all the data.
    // For now just bound it a little and
    io.CopyN(io.Discard, body, 100 << 20);
    body.Close();

    if (!req.keepConn) {
        c.conn.Close();
    }
}

private static void cleanUp(this ptr<child> _addr_c) {
    ref child c = ref _addr_c.val;

    foreach (var (_, req) in c.requests) {
        if (req.pw != null) { 
            // race with call to Close in c.serveRequest doesn't matter because
            // Pipe(Reader|Writer).Close are idempotent
            req.pw.CloseWithError(ErrConnClosed);
        }
    }
}

// Serve accepts incoming FastCGI connections on the listener l, creating a new
// goroutine for each. The goroutine reads requests and then calls handler
// to reply to them.
// If l is nil, Serve accepts connections from os.Stdin.
// If handler is nil, http.DefaultServeMux is used.
public static error Serve(net.Listener l, http.Handler handler) => func((defer, _, _) => {
    if (l == null) {
        error err = default!;
        l, err = net.FileListener(os.Stdin);
        if (err != null) {
            return error.As(err)!;
        }
        defer(l.Close());
    }
    if (handler == null) {
        handler = http.DefaultServeMux;
    }
    while (true) {
        var (rw, err) = l.Accept();
        if (err != null) {
            return error.As(err)!;
        }
        var c = newChild(rw, handler);
        go_(() => c.serve());
    }
});

// ProcessEnv returns FastCGI environment variables associated with the request r
// for which no effort was made to be included in the request itself - the data
// is hidden in the request's context. As an example, if REMOTE_USER is set for a
// request, it will not be found anywhere in r, but it will be included in
// ProcessEnv's response (via r's context).
public static map<@string, @string> ProcessEnv(ptr<http.Request> _addr_r) {
    ref http.Request r = ref _addr_r.val;

    map<@string, @string> (env, _) = r.Context().Value(new envVarsContextKey())._<map<@string, @string>>();
    return env;
}

// addFastCGIEnvToContext reports whether to include the FastCGI environment variable s
// in the http.Request.Context, accessible via ProcessEnv.
private static bool addFastCGIEnvToContext(@string s) { 
    // Exclude things supported by net/http natively:
    switch (s) {
        case "CONTENT_LENGTH": 

        case "CONTENT_TYPE": 

        case "HTTPS": 

        case "PATH_INFO": 

        case "QUERY_STRING": 

        case "REMOTE_ADDR": 

        case "REMOTE_HOST": 

        case "REMOTE_PORT": 

        case "REQUEST_METHOD": 

        case "REQUEST_URI": 

        case "SCRIPT_NAME": 

        case "SERVER_PROTOCOL": 
            return false;
            break;
    }
    if (strings.HasPrefix(s, "HTTP_")) {
        return false;
    }
    switch (s) {
        case "REMOTE_USER": 
            return true;
            break;
    } 
    // Unknown, so include it to be safe.
    return true;
}

} // end fcgi_package
