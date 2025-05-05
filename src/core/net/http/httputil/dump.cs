// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net.http;

using bufio = bufio_package;
using bytes = bytes_package;
using errors = errors_package;
using fmt = fmt_package;
using io = io_package;
using net = net_package;
using http = net.http_package;
using url = net.url_package;
using strings = strings_package;
using time = time_package;
using net;

partial class httputil_package {

// drainBody reads all of b to memory and then returns two equivalent
// ReadClosers yielding the same bytes.
//
// It returns an error if the initial slurp of all bytes fails. It does not attempt
// to make the returned ReadClosers have identical error-matching behavior.
internal static (io.ReadCloser r1, io.ReadCloser r2, error err) drainBody(io.ReadCloser b) {
    io.ReadCloser r1 = default!;
    io.ReadCloser r2 = default!;
    error err = default!;

    if (b == default! || b == http.NoBody) {
        // No copying needed. Preserve the magic sentinel meaning of NoBody.
        return (http.NoBody, http.NoBody, default!);
    }
    ref var buf = ref heap(new bytes_package.Buffer(), out var Ꮡbuf);
    {
        (_, err) = buf.ReadFrom(b); if (err != default!) {
            return (default!, b, err);
        }
    }
    {
        err = b.Close(); if (err != default!) {
            return (default!, b, err);
        }
    }
    return (io.NopCloser(~Ꮡbuf), io.NopCloser(~bytes.NewReader(buf.Bytes())), default!);
}

// dumpConn is a net.Conn which writes to Writer and reads from Reader
[GoType] partial struct dumpConn {
    public partial ref io_package.Writer Writer { get; }
    public partial ref io_package.Reader Reader { get; }
}

[GoRecv] internal static error Close(this ref dumpConn c) {
    return default!;
}

[GoRecv] internal static netꓸAddr LocalAddr(this ref dumpConn c) {
    return default!;
}

[GoRecv] internal static netꓸAddr RemoteAddr(this ref dumpConn c) {
    return default!;
}

[GoRecv] internal static error SetDeadline(this ref dumpConn c, time.Time t) {
    return default!;
}

[GoRecv] internal static error SetReadDeadline(this ref dumpConn c, time.Time t) {
    return default!;
}

[GoRecv] internal static error SetWriteDeadline(this ref dumpConn c, time.Time t) {
    return default!;
}

[GoType("num:byte")] partial struct neverEnding;

internal static (nint n, error err) Read(this neverEnding b, slice<byte> p) {
    nint n = default!;
    error err = default!;

    foreach (var (i, _) in p) {
        p[i] = ((byte)b);
    }
    return (len(p), default!);
}

// outgoingLength is a copy of the unexported
// (*http.Request).outgoingLength method.
internal static int64 outgoingLength(ж<http.Request> Ꮡreq) {
    ref var req = ref Ꮡreq.val;

    if (req.Body == default! || req.Body == http.NoBody) {
        return 0;
    }
    if (req.ContentLength != 0) {
        return req.ContentLength;
    }
    return -1;
}

// DumpRequestOut is like [DumpRequest] but for outgoing client requests. It
// includes any headers that the standard [http.Transport] adds, such as
// User-Agent.
public static (slice<byte>, error) DumpRequestOut(ж<http.Request> Ꮡreq, bool body) => func((defer, _) => {
    ref var req = ref Ꮡreq.val;

    var save = req.Body;
    var dummyBody = false;
    if (!body){
        var contentLength = outgoingLength(Ꮡreq);
        if (contentLength != 0) {
            req.Body = io.NopCloser(io.LimitReader(((neverEnding)(rune)'x'), contentLength));
            dummyBody = true;
        }
    } else {
        error err = default!;
        (save, req.Body, err) = drainBody(req.Body);
        if (err != default!) {
            return (default!, err);
        }
    }
    // Since we're using the actual Transport code to write the request,
    // switch to http so the Transport doesn't try to do an SSL
    // negotiation with our dumpConn and its bytes.Buffer & pipe.
    // The wire format for https and http are the same, anyway.
    var reqSend = req;
    if (req.URL.Scheme == "https"u8) {
        reqSend = @new<http.Request>();
        reqSend.val = req;
        reqSend.val.URL = @new<url.URL>();
        (~reqSend).URL.val = req.URL;
        (~reqSend).URL.val.Scheme = "http"u8;
    }
    // Use the actual Transport code to record what we would send
    // on the wire, but not using TCP.  Use a Transport with a
    // custom dialer that returns a fake net.Conn that waits
    // for the full input (and recording it), and then responds
    // with a dummy response.
    ref var buf = ref heap(new bytes_package.Buffer(), out var Ꮡbuf);                       // records the output
    (pr, pw) = io.Pipe();
    var prʗ1 = pr;
    defer(prʗ1.Close);
    var pwʗ1 = pw;
    defer(pwʗ1.Close);
    var dr = Ꮡ(new delegateReader(c: new channel<io.Reader>(1)));
    var t = Ꮡ(new http.Transport(
        Dial: 
        var bufʗ1 = buf;
        var drʗ1 = dr;
        var pwʗ2 = pw;
        (@string net, @string addr) => (Ꮡ(new dumpConn(io.MultiWriter(~Ꮡbufʗ1, pwʗ2), drʗ1)), default!)
    ));
    var tʗ1 = t;
    defer(tʗ1.CloseIdleConnections);
    // We need this channel to ensure that the reader
    // goroutine exits if t.RoundTrip returns an error.
    // See golang.org/issue/32571.
    var quitReadCh = new channel<struct{}>(1);
    // Wait for the request before replying with a dummy response:
    var drʗ2 = dr;
    var prʗ2 = pr;
    var quitReadChʗ1 = quitReadCh;
    goǃ(() => {
        (reqΔ1, err) = http.ReadRequest(bufio.NewReader(~prʗ2));
        if (err == default!) {
            // Ensure all the body is read; otherwise
            // we'll get a partial dump.
            io.Copy(io.Discard, (~reqΔ1).Body);
            (~reqΔ1).Body.Close();
        }
        switch (select((~drʗ2).c.ᐸꟷ(strings.NewReader("HTTP/1.1 204 No Content\r\nConnection: close\r\n\r\n"u8), ꓸꓸꓸ), ᐸꟷ(quitReadChʗ1, ꓸꓸꓸ))) {
        case 0: {
            break;
        }
        case 1 when quitReadChʗ1.ꟷᐳ(out _): {
            close((~drʗ2).c);
            break;
        }}
    });
    // Ensure delegateReader.Read doesn't block forever if we get an error.
    (_, err) = t.RoundTrip(reqSend);
    req.Body = save;
    if (err != default!) {
        pw.Close();
        dr.val.err = err;
        close(quitReadCh);
        return (default!, err);
    }
    var dump = buf.Bytes();
    // If we used a dummy body above, remove it now.
    // TODO: if the req.ContentLength is large, we allocate memory
    // unnecessarily just to slice it off here. But this is just
    // a debug function, so this is acceptable for now. We could
    // discard the body earlier if this matters.
    if (dummyBody) {
        {
            nint i = bytes.Index(dump, slice<byte>("\r\n\r\n")); if (i >= 0) {
                dump = dump[..(int)(i + 4)];
            }
        }
    }
    return (dump, default!);
});

// delegateReader is a reader that delegates to another reader,
// once it arrives on a channel.
[GoType] partial struct delegateReader {
    internal channel<io.Reader> c;
    internal error err;     // only used if r is nil and c is closed.
    internal io_package.Reader r; // nil until received from c
}

[GoRecv] internal static (nint, error) Read(this ref delegateReader r, slice<byte> p) {
    if (r.r == default!) {
        bool ok = default!;
        {
            var (r.r, ok) = ᐸꟷ(r.c, ꟷ); if (!ok) {
                return (0, r.err);
            }
        }
    }
    return r.r.Read(p);
}

// Return value if nonempty, def otherwise.
internal static @string valueOrDefault(@string value, @string def) {
    if (value != ""u8) {
        return value;
    }
    return def;
}

// not in Header map anyway
internal static map<@string, bool> reqWriteExcludeHeaderDump = new map<@string, bool>{
    ["Host"u8] = true,
    ["Transfer-Encoding"u8] = true,
    ["Trailer"u8] = true
};

// DumpRequest returns the given request in its HTTP/1.x wire
// representation. It should only be used by servers to debug client
// requests. The returned representation is an approximation only;
// some details of the initial request are lost while parsing it into
// an [http.Request]. In particular, the order and case of header field
// names are lost. The order of values in multi-valued headers is kept
// intact. HTTP/2 requests are dumped in HTTP/1.x form, not in their
// original binary representations.
//
// If body is true, DumpRequest also returns the body. To do so, it
// consumes req.Body and then replaces it with a new [io.ReadCloser]
// that yields the same bytes. If DumpRequest returns an error,
// the state of req is undefined.
//
// The documentation for [http.Request.Write] details which fields
// of req are included in the dump.
public static (slice<byte>, error) DumpRequest(ж<http.Request> Ꮡreq, bool body) {
    ref var req = ref Ꮡreq.val;

    error err = default!;
    var save = req.Body;
    if (!body || req.Body == default!){
        req.Body = default!;
    } else {
        (save, req.Body, err) = drainBody(req.Body);
        if (err != default!) {
            return (default!, err);
        }
    }
    ref var b = ref heap(new bytes_package.Buffer(), out var Ꮡb);
    // By default, print out the unmodified req.RequestURI, which
    // is always set for incoming server requests. But because we
    // previously used req.URL.RequestURI and the docs weren't
    // always so clear about when to use DumpRequest vs
    // DumpRequestOut, fall back to the old way if the caller
    // provides a non-server Request.
    @string reqURI = req.RequestURI;
    if (reqURI == ""u8) {
        reqURI = req.URL.RequestURI();
    }
    fmt.Fprintf(~Ꮡb, "%s %s HTTP/%d.%d\r\n"u8, valueOrDefault(req.Method, "GET"u8),
        reqURI, req.ProtoMajor, req.ProtoMinor);
    var absRequestURI = strings.HasPrefix(req.RequestURI, "http://"u8) || strings.HasPrefix(req.RequestURI, "https://"u8);
    if (!absRequestURI) {
        @string host = req.Host;
        if (host == ""u8 && req.URL != nil) {
            host = req.URL.Host;
        }
        if (host != ""u8) {
            fmt.Fprintf(~Ꮡb, "Host: %s\r\n"u8, host);
        }
    }
    var chunked = len(req.TransferEncoding) > 0 && req.TransferEncoding[0] == "chunked";
    if (len(req.TransferEncoding) > 0) {
        fmt.Fprintf(~Ꮡb, "Transfer-Encoding: %s\r\n"u8, strings.Join(req.TransferEncoding, ","u8));
    }
    err = req.Header.WriteSubset(~Ꮡb, reqWriteExcludeHeaderDump);
    if (err != default!) {
        return (default!, err);
    }
    io.WriteString(~Ꮡb, "\r\n"u8);
    if (req.Body != default!) {
        io.Writer dest = Ꮡb;
        if (chunked) {
            dest = NewChunkedWriter(dest);
        }
        (_, err) = io.Copy(dest, req.Body);
        if (chunked) {
            dest._<io.Closer>().Close();
            io.WriteString(~Ꮡb, "\r\n"u8);
        }
    }
    req.Body = save;
    if (err != default!) {
        return (default!, err);
    }
    return (b.Bytes(), default!);
}

// errNoBody is a sentinel error value used by failureToReadBody so we
// can detect that the lack of body was intentional.
internal static error errNoBody = errors.New("sentinel error value"u8);

// failureToReadBody is an io.ReadCloser that just returns errNoBody on
// Read. It's swapped in when we don't actually want to consume
// the body, but need a non-nil one, and want to distinguish the
// error from reading the dummy body.
[GoType] partial struct failureToReadBody {
}

internal static (nint, error) Read(this failureToReadBody _, slice<byte> _) {
    return (0, errNoBody);
}

internal static error Close(this failureToReadBody _) {
    return default!;
}

// emptyBody is an instance of empty reader.
internal static io.ReadCloser emptyBody = io.NopCloser(~strings.NewReader(""u8));

// DumpResponse is like DumpRequest but dumps a response.
public static (slice<byte>, error) DumpResponse(ж<http.Response> Ꮡresp, bool body) {
    ref var resp = ref Ꮡresp.val;

    ref var b = ref heap(new bytes_package.Buffer(), out var Ꮡb);
    error err = default!;
    var save = resp.Body;
    var savecl = resp.ContentLength;
    if (!body){
        // For content length of zero. Make sure the body is an empty
        // reader, instead of returning error through failureToReadBody{}.
        if (resp.ContentLength == 0){
            resp.Body = emptyBody;
        } else {
            resp.Body = new failureToReadBody(nil);
        }
    } else 
    if (resp.Body == default!){
        resp.Body = emptyBody;
    } else {
        (save, resp.Body, err) = drainBody(resp.Body);
        if (err != default!) {
            return (default!, err);
        }
    }
    err = resp.Write(~Ꮡb);
    if (AreEqual(err, errNoBody)) {
        err = default!;
    }
    resp.Body = save;
    resp.ContentLength = savecl;
    if (err != default!) {
        return (default!, err);
    }
    return (b.Bytes(), default!);
}

} // end httputil_package
