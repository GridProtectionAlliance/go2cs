// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:generate bundle -o=h2_bundle.go -prefix=http2 -tags=!nethttpomithttp2 golang.org/x/net/http2

// package http -- go2cs converted at 2022 March 13 05:37:15 UTC
// import "net/http" ==> using http = go.net.http_package
// Original source: C:\Program Files\Go\src\net\http\http.go
namespace go.net;

using io = io_package;
using strconv = strconv_package;
using strings = strings_package;
using time = time_package;
using utf8 = unicode.utf8_package;

using httpguts = golang.org.x.net.http.httpguts_package;


// incomparable is a zero-width, non-comparable type. Adding it to a struct
// makes that struct also non-comparable, and generally doesn't add
// any size (as long as it's first).

using System;
public static partial class http_package {

private partial struct incomparable { // : array<Action>
}

// maxInt64 is the effective "infinite" value for the Server and
// Transport's byte-limiting readers.
private static readonly nint maxInt64 = 1 << 63 - 1;

// aLongTimeAgo is a non-zero time, far in the past, used for
// immediate cancellation of network operations.


// aLongTimeAgo is a non-zero time, far in the past, used for
// immediate cancellation of network operations.
private static var aLongTimeAgo = time.Unix(1, 0);

// omitBundledHTTP2 is set by omithttp2.go when the nethttpomithttp2
// build tag is set. That means h2_bundle.go isn't compiled in and we
// shouldn't try to use it.
private static bool omitBundledHTTP2 = default;

// TODO(bradfitz): move common stuff here. The other files have accumulated
// generic http stuff in random places.

// contextKey is a value for use with context.WithValue. It's used as
// a pointer so it fits in an interface{} without allocation.
private partial struct contextKey {
    public @string name;
}

private static @string String(this ptr<contextKey> _addr_k) {
    ref contextKey k = ref _addr_k.val;

    return "net/http context value " + k.name;
}

// Given a string of the form "host", "host:port", or "[ipv6::address]:port",
// return true if the string includes a port.
private static bool hasPort(@string s) {
    return strings.LastIndex(s, ":") > strings.LastIndex(s, "]");
}

// removeEmptyPort strips the empty port in ":port" to ""
// as mandated by RFC 3986 Section 6.2.3.
private static @string removeEmptyPort(@string host) {
    if (hasPort(host)) {
        return strings.TrimSuffix(host, ":");
    }
    return host;
}

private static bool isNotToken(int r) {
    return !httpguts.IsTokenRune(r);
}

// stringContainsCTLByte reports whether s contains any ASCII control character.
private static bool stringContainsCTLByte(@string s) {
    for (nint i = 0; i < len(s); i++) {
        var b = s[i];
        if (b < ' ' || b == 0x7f) {
            return true;
        }
    }
    return false;
}

private static @string hexEscapeNonASCII(@string s) {
    nint newLen = 0;
    {
        nint i__prev1 = i;

        for (nint i = 0; i < len(s); i++) {
            if (s[i] >= utf8.RuneSelf) {
                newLen += 3;
            }
            else
 {
                newLen++;
            }
        }

        i = i__prev1;
    }
    if (newLen == len(s)) {
        return s;
    }
    var b = make_slice<byte>(0, newLen);
    {
        nint i__prev1 = i;

        for (i = 0; i < len(s); i++) {
            if (s[i] >= utf8.RuneSelf) {
                b = append(b, '%');
                b = strconv.AppendInt(b, int64(s[i]), 16);
            }
            else
 {
                b = append(b, s[i]);
            }
        }

        i = i__prev1;
    }
    return string(b);
}

// NoBody is an io.ReadCloser with no bytes. Read always returns EOF
// and Close always returns nil. It can be used in an outgoing client
// request to explicitly signal that a request has zero bytes.
// An alternative, however, is to simply set Request.Body to nil.
public static noBody NoBody = new noBody();

private partial struct noBody {
}

private static (nint, error) Read(this noBody _p0, slice<byte> _p0) {
    nint _p0 = default;
    error _p0 = default!;

    return (0, error.As(io.EOF)!);
}
private static error Close(this noBody _p0) {
    return error.As(null!)!;
}
private static (long, error) WriteTo(this noBody _p0, io.Writer _p0) {
    long _p0 = default;
    error _p0 = default!;

    return (0, error.As(null!)!);
}

 
// verify that an io.Copy from NoBody won't require a buffer:
private static io.WriterTo _ = NoBody;private static io.ReadCloser _ = NoBody;

// PushOptions describes options for Pusher.Push.
public partial struct PushOptions {
    public @string Method; // Header specifies additional promised request headers. This cannot
// include HTTP/2 pseudo header fields like ":path" and ":scheme",
// which will be added automatically.
    public Header Header;
}

// Pusher is the interface implemented by ResponseWriters that support
// HTTP/2 server push. For more background, see
// https://tools.ietf.org/html/rfc7540#section-8.2.
public partial interface Pusher {
    error Push(@string target, ptr<PushOptions> opts);
}

} // end http_package
