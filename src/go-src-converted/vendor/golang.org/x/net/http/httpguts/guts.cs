// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package httpguts provides functions implementing various details
// of the HTTP specification.
//
// This package is shared by the standard library (which vendors it)
// and x/net/http2. It comes with no API stability promise.

// package httpguts -- go2cs converted at 2022 March 13 06:45:25 UTC
// import "vendor/golang.org/x/net/http/httpguts" ==> using httpguts = go.vendor.golang.org.x.net.http.httpguts_package
// Original source: C:\Program Files\Go\src\vendor\golang.org\x\net\http\httpguts\guts.go
namespace go.vendor.golang.org.x.net.http;

using textproto = net.textproto_package;
using strings = strings_package;


// ValidTrailerHeader reports whether name is a valid header field name to appear
// in trailers.
// See RFC 7230, Section 4.1.2

public static partial class httpguts_package {

public static bool ValidTrailerHeader(@string name) {
    name = textproto.CanonicalMIMEHeaderKey(name);
    if (strings.HasPrefix(name, "If-") || badTrailer[name]) {
        return false;
    }
    return true;
}

private static map badTrailer = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, bool>{"Authorization":true,"Cache-Control":true,"Connection":true,"Content-Encoding":true,"Content-Length":true,"Content-Range":true,"Content-Type":true,"Expect":true,"Host":true,"Keep-Alive":true,"Max-Forwards":true,"Pragma":true,"Proxy-Authenticate":true,"Proxy-Authorization":true,"Proxy-Connection":true,"Range":true,"Realm":true,"Te":true,"Trailer":true,"Transfer-Encoding":true,"Www-Authenticate":true,};

} // end httpguts_package
