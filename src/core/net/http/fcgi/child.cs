// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
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
using net;

partial class fcgi_package {

// request holds the state for an in-progress request. As soon as it's complete,
// it's converted to an http.Request.
[GoType] partial struct request {
    internal ж<io_package.PipeWriter> pw;
    internal uint16 reqId;
    internal map<@string, @string> @params;
    internal array<byte> buf = new(1024);
    internal slice<byte> rawParams;
    internal bool keepConn;
}

// envVarsContextKey uniquely identifies a mapping of CGI
// environment variables to their values in a request context
[GoType] partial struct envVarsContextKey {
}

internal static ж<request> newRequest(uint16 reqId, uint8 flags) {
    var r = Ꮡ(new request(
        reqId: reqId,
        @params: new map<@string, @string>{},
        keepConn: (uint8)(flags & flagKeepConn) != 0
    ));
    r.val.rawParams = (~r).buf[..0];
    return r;
}

// parseParams reads an encoded []byte into Params.
[GoRecv] internal static void parseParams(this ref request r) {
    var text = r.rawParams;
    r.rawParams = default!;
    while (len(text) > 0) {
        var (keyLen, n) = readSize(text);
        if (n == 0) {
            return;
        }
        text = text[(int)(n)..];
        var (valLen, n) = readSize(text);
        if (n == 0) {
            return;
        }
        text = text[(int)(n)..];
        if (((nint)keyLen) + ((nint)valLen) > len(text)) {
            return;
        }
        @string key = readString(text, keyLen);
        text = text[(int)(keyLen)..];
        @string val = readString(text, valLen);
        text = text[(int)(valLen)..];
        r.@params[key] = val;
    }
}

// response implements http.ResponseWriter.
[GoType] partial struct response {
    internal ж<request> req;
    internal net.http_package.ΔHeader header;
    internal nint code;
    internal bool wroteHeader;
    internal bool wroteCGIHeader;
    internal ж<bufWriter> w;
}

internal static ж<response> newResponse(ж<child> Ꮡc, ж<request> Ꮡreq) {
    ref var c = ref Ꮡc.val;
    ref var req = ref Ꮡreq.val;

    return Ꮡ(new response(
        req: req,
        header: new httpꓸHeader{nil},
        w: newWriter(c.conn, typeStdout, req.reqId)
    ));
}

[GoRecv] internal static httpꓸHeader Header(this ref response r) {
    return r.header;
}

[GoRecv] internal static (nint n, error err) Write(this ref response r, slice<byte> p) {
    nint n = default!;
    error err = default!;

    if (!r.wroteHeader) {
        r.WriteHeader(http.StatusOK);
    }
    if (!r.wroteCGIHeader) {
        r.writeCGIHeader(p);
    }
    return r.w.Write(p);
}

[GoRecv] internal static void WriteHeader(this ref response r, nint code) {
    if (r.wroteHeader) {
        return;
    }
    r.wroteHeader = true;
    r.code = code;
    if (code == http.StatusNotModified) {
        // Must not have body.
        r.header.Del("Content-Type"u8);
        r.header.Del("Content-Length"u8);
        r.header.Del("Transfer-Encoding"u8);
    }
    if (r.header.Get("Date"u8) == ""u8) {
        r.header.Set("Date"u8, time.Now().UTC().Format(http.TimeFormat));
    }
}

// writeCGIHeader finalizes the header sent to the client and writes it to the output.
// p is not written by writeHeader, but is the first chunk of the body
// that will be written. It is sniffed for a Content-Type if none is
// set explicitly.
[GoRecv] internal static void writeCGIHeader(this ref response r, slice<byte> p) {
    if (r.wroteCGIHeader) {
        return;
    }
    r.wroteCGIHeader = true;
    fmt.Fprintf(~r.w, "Status: %d %s\r\n"u8, r.code, http.StatusText(r.code));
    {
        var _ = r.header["Content-Type"u8];
        var hasType = r.header["Content-Type"u8]; if (r.code != http.StatusNotModified && !hasType) {
            r.header.Set("Content-Type"u8, http.DetectContentType(p));
        }
    }
    r.header.Write(~r.w);
    r.w.WriteString("\r\n"u8);
    r.w.Flush();
}

[GoRecv] internal static void Flush(this ref response r) {
    if (!r.wroteHeader) {
        r.WriteHeader(http.StatusOK);
    }
    r.w.Flush();
}

[GoRecv] internal static error Close(this ref response r) {
    r.Flush();
    return r.w.Close();
}

[GoType] partial struct child {
    internal ж<conn> conn;
    internal net.http_package.ΔHandler handler;
    internal map<uint16, ж<request>> requests; // keyed by request ID
}

internal static ж<child> newChild(io.ReadWriteCloser rwc, httpꓸHandler handler) {
    return Ꮡ(new child(
        conn: newConn(rwc),
        handler: handler,
        requests: new map<uint16, ж<request>>()
    ));
}

[GoRecv] internal static void serve(this ref child c) => func((defer, _) => {
    defer(c.conn.Close);
    defer(c.cleanUp);
    ref var rec = ref heap(new record(), out var Ꮡrec);
    while (ᐧ) {
        {
            var err = rec.read(c.conn.rwc); if (err != default!) {
                return;
            }
        }
        {
            var err = c.handleRecord(Ꮡrec); if (err != default!) {
                return;
            }
        }
    }
});

internal static error errCloseConn = errors.New("fcgi: connection should be closed"u8);

internal static io.ReadCloser emptyBody = io.NopCloser(~strings.NewReader(""u8));

// ErrRequestAborted is returned by Read when a handler attempts to read the
// body of a request that has been aborted by the web server.
public static error ErrRequestAborted = errors.New("fcgi: request aborted by web server"u8);

// ErrConnClosed is returned by Read when a handler attempts to read the body of
// a request after the connection to the web server has been closed.
public static error ErrConnClosed = errors.New("fcgi: connection to web server closed"u8);

[GoRecv] internal static error handleRecord(this ref child c, ж<record> Ꮡrec) {
    ref var rec = ref Ꮡrec.val;

    var req = c.requests[rec.h.Id];
    var ok = c.requests[rec.h.Id];
    if (!ok && rec.h.Type != typeBeginRequest && rec.h.Type != typeGetValues) {
        // The spec says to ignore unknown request IDs.
        return default!;
    }
    var exprᴛ1 = rec.h.Type;
    if (exprᴛ1 == typeBeginRequest) {
        if (req != nil) {
            // The server is trying to begin a request with the same ID
            // as an in-progress request. This is an error.
            return errors.New("fcgi: received ID that is already in-flight"u8);
        }
        beginRequest br = default!;
        {
            var err = br.read(rec.content()); if (err != default!) {
                return err;
            }
        }
        if (br.role != roleResponder) {
            c.conn.writeEndRequest(rec.h.Id, 0, statusUnknownRole);
            return default!;
        }
        req = newRequest(rec.h.Id, br.flags);
        c.requests[rec.h.Id] = req;
        return default!;
    }
    if (exprᴛ1 == typeParams) {
        if (len(rec.content()) > 0) {
            // NOTE(eds): Technically a key-value pair can straddle the boundary
            // between two packets. We buffer until we've received all parameters.
            req.val.rawParams = append((~req).rawParams, rec.content().ꓸꓸꓸ);
            return default!;
        }
        req.parseParams();
        return default!;
    }
    if (exprᴛ1 == typeStdin) {
        var content = rec.content();
        if ((~req).pw == nil) {
            io.ReadCloser body = default!;
            if (len(content) > 0){
                // body could be an io.LimitReader, but it shouldn't matter
                // as long as both sides are behaving.
                (body, req.val.pw) = io.Pipe();
            } else {
                body = emptyBody;
            }
            goǃ(c.serveRequest, req, body);
        }
        if (len(content) > 0){
            // TODO(eds): This blocks until the handler reads from the pipe.
            // If the handler takes a long time, it might be a problem.
            (~req).pw.Write(content);
        } else {
            delete(c.requests, (~req).reqId);
            if ((~req).pw != nil) {
                (~req).pw.Close();
            }
        }
        return default!;
    }
    if (exprᴛ1 == typeGetValues) {
        var values = new map<@string, @string>{["FCGI_MPXS_CONNS"u8] = "1"u8};
        c.conn.writePairs(typeGetValuesResult, 0, values);
        return default!;
    }
    if (exprᴛ1 == typeData) {
        return default!;
    }
    if (exprᴛ1 == typeAbortRequest) {
        delete(c.requests, // If the filter role is implemented, read the data stream here.
 rec.h.Id);
        c.conn.writeEndRequest(rec.h.Id, 0, statusRequestComplete);
        if ((~req).pw != nil) {
            (~req).pw.CloseWithError(ErrRequestAborted);
        }
        if (!(~req).keepConn) {
            // connection will close upon return
            return errCloseConn;
        }
        return default!;
    }
    { /* default: */
        var b = new slice<byte>(8);
        b[0] = ((byte)rec.h.Type);
        c.conn.writeRecord(typeUnknownType, 0, b);
        return default!;
    }

}

// filterOutUsedEnvVars returns a new map of env vars without the
// variables in the given envVars map that are read for creating each http.Request
internal static map<@string, @string> filterOutUsedEnvVars(map<@string, @string> envVars) {
    var withoutUsedEnvVars = new map<@string, @string>();
    foreach (var (k, v) in envVars) {
        if (addFastCGIEnvToContext(k)) {
            withoutUsedEnvVars[k] = v;
        }
    }
    return withoutUsedEnvVars;
}

[GoRecv] internal static void serveRequest(this ref child c, ж<request> Ꮡreq, io.ReadCloser body) {
    ref var req = ref Ꮡreq.val;

    var r = newResponse(c, Ꮡreq);
    (httpReq, err) = cgi.RequestFromMap(req.@params);
    if (err != default!){
        // there was an error reading the request
        r.WriteHeader(http.StatusInternalServerError);
        c.conn.writeRecord(typeStderr, req.reqId, slice<byte>(err.Error()));
    } else {
        httpReq.val.Body = body;
        var withoutUsedEnvVars = filterOutUsedEnvVars(req.@params);
        var envVarCtx = context.WithValue(httpReq.Context(), new envVarsContextKey(nil), withoutUsedEnvVars);
        httpReq = httpReq.WithContext(envVarCtx);
        c.handler.ServeHTTP(~r, httpReq);
    }
    // Make sure we serve something even if nothing was written to r
    r.Write(default!);
    r.Close();
    c.conn.writeEndRequest(req.reqId, 0, statusRequestComplete);
    // Consume the entire body, so the host isn't still writing to
    // us when we close the socket below in the !keepConn case,
    // otherwise we'd send a RST. (golang.org/issue/4183)
    // TODO(bradfitz): also bound this copy in time. Or send
    // some sort of abort request to the host, so the host
    // can properly cut off the client sending all the data.
    // For now just bound it a little and
    io.CopyN(io.Discard, body, 100 << (int)(20));
    body.Close();
    if (!req.keepConn) {
        c.conn.Close();
    }
}

[GoRecv] internal static void cleanUp(this ref child c) {
    foreach (var (_, req) in c.requests) {
        if ((~req).pw != nil) {
            // race with call to Close in c.serveRequest doesn't matter because
            // Pipe(Reader|Writer).Close are idempotent
            (~req).pw.CloseWithError(ErrConnClosed);
        }
    }
}

// Serve accepts incoming FastCGI connections on the listener l, creating a new
// goroutine for each. The goroutine reads requests and then calls handler
// to reply to them.
// If l is nil, Serve accepts connections from os.Stdin.
// If handler is nil, [http.DefaultServeMux] is used.
public static error Serve(net.Listener l, httpꓸHandler handler) => func((defer, _) => {
    if (l == default!) {
        error err = default!;
        (l, err) = net.FileListener(os.Stdin);
        if (err != default!) {
            return err;
        }
        defer(l.Close);
    }
    if (handler == default!) {
        handler = ~http.DefaultServeMux;
    }
    while (ᐧ) {
        (rw, err) = l.Accept();
        if (err != default!) {
            return err;
        }
        var c = newChild(rw, handler);
        var cʗ1 = c;
        goǃ(cʗ1.serve);
    }
});

// ProcessEnv returns FastCGI environment variables associated with the request r
// for which no effort was made to be included in the request itself - the data
// is hidden in the request's context. As an example, if REMOTE_USER is set for a
// request, it will not be found anywhere in r, but it will be included in
// ProcessEnv's response (via r's context).
public static map<@string, @string> ProcessEnv(ж<http.Request> Ꮡr) {
    ref var r = ref Ꮡr.val;

    var (env, _) = r.Context().Value(new envVarsContextKey(nil))._<map<@string, @string, >>(ᐧ);
    return env;
}

// addFastCGIEnvToContext reports whether to include the FastCGI environment variable s
// in the http.Request.Context, accessible via ProcessEnv.
internal static bool addFastCGIEnvToContext(@string s) {
    // Exclude things supported by net/http natively:
    var exprᴛ1 = s;
    if (exprᴛ1 == "CONTENT_LENGTH"u8 || exprᴛ1 == "CONTENT_TYPE"u8 || exprᴛ1 == "HTTPS"u8 || exprᴛ1 == "PATH_INFO"u8 || exprᴛ1 == "QUERY_STRING"u8 || exprᴛ1 == "REMOTE_ADDR"u8 || exprᴛ1 == "REMOTE_HOST"u8 || exprᴛ1 == "REMOTE_PORT"u8 || exprᴛ1 == "REQUEST_METHOD"u8 || exprᴛ1 == "REQUEST_URI"u8 || exprᴛ1 == "SCRIPT_NAME"u8 || exprᴛ1 == "SERVER_PROTOCOL"u8) {
        return false;
    }

    if (strings.HasPrefix(s, "HTTP_"u8)) {
        return false;
    }
    // Explicitly include FastCGI-specific things.
    // This list is redundant with the default "return true" below.
    // Consider this documentation of the sorts of things we expect
    // to maybe see.
    var exprᴛ2 = s;
    if (exprᴛ2 == "REMOTE_USER"u8) {
        return true;
    }

    // Unknown, so include it to be safe.
    return true;
}

} // end fcgi_package
