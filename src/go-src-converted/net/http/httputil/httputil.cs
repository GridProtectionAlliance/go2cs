// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package httputil provides HTTP utility functions, complementing the
// more common ones in the net/http package.
// package httputil -- go2cs converted at 2022 March 06 22:24:01 UTC
// import "net/http/httputil" ==> using httputil = go.net.http.httputil_package
// Original source: C:\Program Files\Go\src\net\http\httputil\httputil.go
using io = go.io_package;
using @internal = go.net.http.@internal_package;

namespace go.net.http;

public static partial class httputil_package {

    // NewChunkedReader returns a new chunkedReader that translates the data read from r
    // out of HTTP "chunked" format before returning it.
    // The chunkedReader returns io.EOF when the final 0-length chunk is read.
    //
    // NewChunkedReader is not needed by normal applications. The http package
    // automatically decodes chunking when reading response bodies.
public static io.Reader NewChunkedReader(io.Reader r) {
    return @internal.NewChunkedReader(r);
}

// NewChunkedWriter returns a new chunkedWriter that translates writes into HTTP
// "chunked" format before writing them to w. Closing the returned chunkedWriter
// sends the final 0-length chunk that marks the end of the stream but does
// not send the final CRLF that appears after trailers; trailers and the last
// CRLF must be written separately.
//
// NewChunkedWriter is not needed by normal applications. The http
// package adds chunking automatically if handlers don't set a
// Content-Length header. Using NewChunkedWriter inside a handler
// would result in double chunking or chunking with a Content-Length
// length, both of which are wrong.
public static io.WriteCloser NewChunkedWriter(io.Writer w) {
    return @internal.NewChunkedWriter(w);
}

// ErrLineTooLong is returned when reading malformed chunked data
// with lines that are too long.
public static var ErrLineTooLong = @internal.ErrLineTooLong;

} // end httputil_package
