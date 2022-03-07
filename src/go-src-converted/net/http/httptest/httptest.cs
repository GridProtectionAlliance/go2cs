// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package httptest provides utilities for HTTP testing.
// package httptest -- go2cs converted at 2022 March 06 22:23:53 UTC
// import "net/http/httptest" ==> using httptest = go.net.http.httptest_package
// Original source: C:\Program Files\Go\src\net\http\httptest\httptest.go
using bufio = go.bufio_package;
using bytes = go.bytes_package;
using tls = go.crypto.tls_package;
using io = go.io_package;
using http = go.net.http_package;
using strings = go.strings_package;

namespace go.net.http;

public static partial class httptest_package {

    // NewRequest returns a new incoming server Request, suitable
    // for passing to an http.Handler for testing.
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
public static ptr<http.Request> NewRequest(@string method, @string target, io.Reader body) => func((_, panic, _) => {
    if (method == "") {
        method = "GET";
    }
    var (req, err) = http.ReadRequest(bufio.NewReader(strings.NewReader(method + " " + target + " HTTP/1.0\r\n\r\n")));
    if (err != null) {
        panic("invalid NewRequest arguments; " + err.Error());
    }
    req.Proto = "HTTP/1.1";
    req.ProtoMinor = 1;
    req.Close = false;

    if (body != null) {
        switch (body.type()) {
            case ptr<bytes.Buffer> v:
                req.ContentLength = int64(v.Len());
                break;
            case ptr<bytes.Reader> v:
                req.ContentLength = int64(v.Len());
                break;
            case ptr<strings.Reader> v:
                req.ContentLength = int64(v.Len());
                break;
            default:
            {
                var v = body.type();
                req.ContentLength = -1;
                break;
            }
        }
        {
            io.ReadCloser (rc, ok) = body._<io.ReadCloser>();

            if (ok) {
                req.Body = rc;
            }
            else
 {
                req.Body = io.NopCloser(body);
            }
        }

    }
    req.RemoteAddr = "192.0.2.1:1234";

    if (req.Host == "") {
        req.Host = "example.com";
    }
    if (strings.HasPrefix(target, "https://")) {
        req.TLS = addr(new tls.ConnectionState(Version:tls.VersionTLS12,HandshakeComplete:true,ServerName:req.Host,));
    }
    return _addr_req!;

});

} // end httptest_package
