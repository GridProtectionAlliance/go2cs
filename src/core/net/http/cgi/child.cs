// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file implements CGI from the perspective of a child
// process.
namespace go.net.http;

using bufio = bufio_package;
using tls = crypto.tls_package;
using errors = errors_package;
using fmt = fmt_package;
using io = io_package;
using net = net_package;
using http = net.http_package;
using url = net.url_package;
using os = os_package;
using strconv = strconv_package;
using strings = strings_package;
using crypto;
using net;

partial class cgi_package {

// Request returns the HTTP request as represented in the current
// environment. This assumes the current program is being run
// by a web server in a CGI environment.
// The returned Request's Body is populated, if applicable.
public static (ж<http.Request>, error) Request() {
    (r, err) = RequestFromMap(envMap(os.Environ()));
    if (err != default!) {
        return (default!, err);
    }
    if ((~r).ContentLength > 0) {
        r.val.Body = io.NopCloser(io.LimitReader(~os.Stdin, (~r).ContentLength));
    }
    return (r, default!);
}

internal static map<@string, @string> envMap(slice<@string> env) {
    var m = new map<@string, @string>();
    foreach (var (_, kv) in env) {
        {
            var (k, v, ok) = strings.Cut(kv, "="u8); if (ok) {
                m[k] = v;
            }
        }
    }
    return m;
}

// RequestFromMap creates an [http.Request] from CGI variables.
// The returned Request's Body field is not populated.
public static (ж<http.Request>, error) RequestFromMap(map<@string, @string> @params) {
    var r = @new<http.Request>();
    r.val.Method = @params["REQUEST_METHOD"u8];
    if ((~r).Method == ""u8) {
        return (default!, errors.New("cgi: no REQUEST_METHOD in environment"u8));
    }
    r.val.Proto = @params["SERVER_PROTOCOL"u8];
    bool ok = default!;
    (r.val.ProtoMajor, r.val.ProtoMinor, ok) = http.ParseHTTPVersion((~r).Proto);
    if (!ok) {
        return (default!, errors.New("cgi: invalid SERVER_PROTOCOL version"u8));
    }
    r.val.Close = true;
    r.val.Trailer = new httpꓸHeader{nil};
    r.val.Header = new httpꓸHeader{nil};
    r.val.Host = @params["HTTP_HOST"u8];
    {
        @string lenstr = @params["CONTENT_LENGTH"u8]; if (lenstr != ""u8) {
            var (clen, err) = strconv.ParseInt(lenstr, 10, 64);
            if (err != default!) {
                return (default!, errors.New("cgi: bad CONTENT_LENGTH in environment: "u8 + lenstr));
            }
            r.val.ContentLength = clen;
        }
    }
    {
        @string ct = @params["CONTENT_TYPE"u8]; if (ct != ""u8) {
            (~r).Header.Set("Content-Type"u8, ct);
        }
    }
    // Copy "HTTP_FOO_BAR" variables to "Foo-Bar" Headers
    foreach (var (k, v) in @params) {
        if (k == "HTTP_HOST"u8) {
            continue;
        }
        {
            var (after, found) = strings.CutPrefix(k, "HTTP_"u8); if (found) {
                (~r).Header.Add(strings.ReplaceAll(after, "_"u8, "-"u8), v);
            }
        }
    }
    @string uriStr = @params["REQUEST_URI"u8];
    if (uriStr == ""u8) {
        // Fallback to SCRIPT_NAME, PATH_INFO and QUERY_STRING.
        uriStr = @params["SCRIPT_NAME"u8] + @params["PATH_INFO"u8];
        @string s = @params["QUERY_STRING"u8];
        if (s != ""u8) {
            uriStr += "?"u8 + s;
        }
    }
    // There's apparently a de-facto standard for this.
    // https://web.archive.org/web/20170105004655/http://docstore.mik.ua/orelly/linux/cgi/ch03_02.htm#ch03-35636
    {
        @string s = @params["HTTPS"u8]; if (s == "on"u8 || s == "ON"u8 || s == "1"u8) {
            r.val.TLS = Ꮡ(new tlsꓸConnectionState(HandshakeComplete: true));
        }
    }
    if ((~r).Host != ""u8) {
        // Hostname is provided, so we can reasonably construct a URL.
        @string rawurl = (~r).Host + uriStr;
        if ((~r).TLS == nil){
            rawurl = "http://"u8 + rawurl;
        } else {
            rawurl = "https://"u8 + rawurl;
        }
        (url, err) = url.Parse(rawurl);
        if (err != default!) {
            return (default!, errors.New("cgi: failed to parse host and REQUEST_URI into a URL: "u8 + rawurl));
        }
        r.val.URL = url;
    }
    // Fallback logic if we don't have a Host header or the URL
    // failed to parse
    if ((~r).URL == nil) {
        (url, err) = url.Parse(uriStr);
        if (err != default!) {
            return (default!, errors.New("cgi: failed to parse REQUEST_URI into a URL: "u8 + uriStr));
        }
        r.val.URL = url;
    }
    // Request.RemoteAddr has its port set by Go's standard http
    // server, so we do here too.
    var (remotePort, _) = strconv.Atoi(@params["REMOTE_PORT"u8]);
    // zero if unset or invalid
    r.val.RemoteAddr = net.JoinHostPort(@params["REMOTE_ADDR"u8], strconv.Itoa(remotePort));
    return (r, default!);
}

// Serve executes the provided [Handler] on the currently active CGI
// request, if any. If there's no current CGI environment
// an error is returned. The provided handler may be nil to use
// [http.DefaultServeMux].
public static error Serve(httpꓸHandler handler) {
    (req, err) = Request();
    if (err != default!) {
        return err;
    }
    if ((~req).Body == default!) {
        req.val.Body = http.NoBody;
    }
    if (handler == default!) {
        handler = ~http.DefaultServeMux;
    }
    var rw = Ꮡ(new response(
        req: req,
        header: new httpꓸHeader(),
        bufw: bufio.NewWriter(~os.Stdout)
    ));
    handler.ServeHTTP(~rw, req);
    rw.Write(default!);
    // make sure a response is sent
    {
        err = (~rw).bufw.Flush(); if (err != default!) {
            return err;
        }
    }
    return default!;
}

[GoType] partial struct response {
    internal ж<net.http_package.Request> req;
    internal net.http_package.ΔHeader header;
    internal nint code;
    internal bool wroteHeader;
    internal bool wroteCGIHeader;
    internal ж<bufio_package.Writer> bufw;
}

[GoRecv] internal static void Flush(this ref response r) {
    r.bufw.Flush();
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
    return r.bufw.Write(p);
}

[GoRecv] internal static void WriteHeader(this ref response r, nint code) {
    if (r.wroteHeader) {
        // Note: explicitly using Stderr, as Stdout is our HTTP output.
        fmt.Fprintf(~os.Stderr, "CGI attempted to write header twice on request for %s"u8, r.req.URL);
        return;
    }
    r.wroteHeader = true;
    r.code = code;
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
    fmt.Fprintf(~r.bufw, "Status: %d %s\r\n"u8, r.code, http.StatusText(r.code));
    {
        var _ = r.header["Content-Type"u8];
        var hasType = r.header["Content-Type"u8]; if (!hasType) {
            r.header.Set("Content-Type"u8, http.DetectContentType(p));
        }
    }
    r.header.Write(~r.bufw);
    r.bufw.WriteString("\r\n"u8);
    r.bufw.Flush();
}

} // end cgi_package
