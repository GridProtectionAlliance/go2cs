// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package httptest provides utilities for HTTP testing.
namespace go.net.http;

using bufio = bufio_package;
using bytes = bytes_package;
using context = context_package;
using tls = crypto.tls_package;
using io = io_package;
using http = net.http_package;
using strings = strings_package;
using crypto;
using net;

partial class httptest_package {

// NewRequest wraps NewRequestWithContext using context.Background.
public static ж<http.Request> NewRequest(@string method, @string target, io.Reader body) {
    return NewRequestWithContext(context.Background(), method, target, body);
}

// NewRequestWithContext returns a new incoming server Request, suitable
// for passing to an [http.Handler] for testing.
//
// The target is the RFC 7230 "request-target": it may be either a
// path or an absolute URL. If target is an absolute URL, the host name
// from the URL is used. Otherwise, "example.com" is used.
//
// The TLS field is set to a non-nil dummy value if target has scheme
// "https".
//
// The Request.Proto is always HTTP/1.1.
//
// An empty method means "GET".
//
// The provided body may be nil. If the body is of type *bytes.Reader,
// *strings.Reader, or *bytes.Buffer, the Request.ContentLength is
// set.
//
// NewRequest panics on error for ease of use in testing, where a
// panic is acceptable.
//
// To generate a client HTTP request instead of a server request, see
// the NewRequest function in the net/http package.
public static ж<http.Request> NewRequestWithContext(context.Context ctx, @string method, @string target, io.Reader body) {
    if (method == ""u8) {
        method = "GET"u8;
    }
    (req, err) = http.ReadRequest(bufio.NewReader(~strings.NewReader(method + " "u8 + target + " HTTP/1.0\r\n\r\n"u8)));
    if (err != default!) {
        throw panic("invalid NewRequest arguments; "u8 + err.Error());
    }
    req = req.WithContext(ctx);
    // HTTP/1.0 was used above to avoid needing a Host field. Change it to 1.1 here.
    req.val.Proto = "HTTP/1.1"u8;
    req.val.ProtoMinor = 1;
    req.val.Close = false;
    if (body != default!) {
        switch (body.type()) {
        case ж<bytes.Buffer> v: {
            req.val.ContentLength = ((int64)v.Len());
            break;
        }
        case ж<bytes.Reader> v: {
            req.val.ContentLength = ((int64)v.Len());
            break;
        }
        case ж<strings.Reader> v: {
            req.val.ContentLength = ((int64)v.Len());
            break;
        }
        default: {
            var v = body.type();
            req.val.ContentLength = -1;
            break;
        }}
        {
            var (rc, ok) = body._<io.ReadCloser>(ᐧ); if (ok){
                req.val.Body = rc;
            } else {
                req.val.Body = io.NopCloser(body);
            }
        }
    }
    // 192.0.2.0/24 is "TEST-NET" in RFC 5737 for use solely in
    // documentation and example source code and should not be
    // used publicly.
    req.val.RemoteAddr = "192.0.2.1:1234"u8;
    if ((~req).Host == ""u8) {
        req.val.Host = "example.com"u8;
    }
    if (strings.HasPrefix(target, "https://"u8)) {
        req.val.TLS = Ꮡ(new tlsꓸConnectionState(
            Version: tls.VersionTLS12,
            HandshakeComplete: true,
            ServerName: (~req).Host
        ));
    }
    return req;
}

} // end httptest_package
