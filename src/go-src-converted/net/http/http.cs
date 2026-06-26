// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:generate bundle -o=h2_bundle.go -prefix=http2 -tags=!nethttpomithttp2 golang.org/x/net/http2
namespace go.net;

using io = io_package;
using strconv = strconv_package;
using strings = strings_package;
using time = time_package;
using utf8 = unicode.utf8_package;
using httpguts = golang.org.x.net.http.httpguts_package;
using golang.org.x.net.http;
using unicode;

partial class http_package {
// [...]func()

// maxInt64 is the effective "infinite" value for the Server and
// Transport's byte-limiting readers.
internal static readonly UntypedInt maxInt64 = /* 1<<63 - 1 */ 9223372036854775807;

// aLongTimeAgo is a non-zero time, far in the past, used for
// immediate cancellation of network operations.
internal static time.Time aLongTimeAgo = time.Unix(1, 0);

// omitBundledHTTP2 is set by omithttp2.go when the nethttpomithttp2
// build tag is set. That means h2_bundle.go isn't compiled in and we
// shouldn't try to use it.
internal static bool omitBundledHTTP2;

// TODO(bradfitz): move common stuff here. The other files have accumulated
// generic http stuff in random places.

// contextKey is a value for use with context.WithValue. It's used as
// a pointer so it fits in an interface{} without allocation.
[GoType] partial struct contextKey {
    internal @string name;
}

[GoRecv] internal static @string String(this ref contextKey k) {
    return "net/http context value "u8 + k.name;
}

// Given a string of the form "host", "host:port", or "[ipv6::address]:port",
// return true if the string includes a port.
internal static bool hasPort(@string s) {
    return strings.LastIndex(s, ":"u8) > strings.LastIndex(s, "]"u8);
}

// removeEmptyPort strips the empty port in ":port" to ""
// as mandated by RFC 3986 Section 6.2.3.
internal static @string removeEmptyPort(@string host) {
    if (hasPort(host)) {
        return strings.TrimSuffix(host, ":"u8);
    }
    return host;
}

internal static bool isNotToken(rune r) {
    return !httpguts.IsTokenRune(r);
}

// stringContainsCTLByte reports whether s contains any ASCII control character.
internal static bool stringContainsCTLByte(@string s) {
    for (nint i = 0; i < len(s); i++) {
        var b = s[i];
        if (b < (rune)' ' || b == 127) {
            return true;
        }
    }
    return false;
}

internal static @string hexEscapeNonASCII(@string s) {
    nint newLen = 0;
    for (nint i = 0; i < len(s); i++) {
        if (s[i] >= utf8.RuneSelf){
            newLen += 3;
        } else {
            newLen++;
        }
    }
    if (newLen == len(s)) {
        return s;
    }
    var b = new slice<byte>(0, newLen);
    nint pos = default!;
    for (nint i = 0; i < len(s); i++) {
        if (s[i] >= utf8.RuneSelf) {
            if (pos < i) {
                b = append(b, s[(int)(pos)..(int)(i)].ꓸꓸꓸ);
            }
            b = append(b, (rune)'%');
            b = strconv.AppendInt(b, ((int64)s[i]), 16);
            pos = i + 1;
        }
    }
    if (pos < len(s)) {
        b = append(b, s[(int)(pos)..].ꓸꓸꓸ);
    }
    return ((@string)b);
}

// NoBody is an [io.ReadCloser] with no bytes. Read always returns EOF
// and Close always returns nil. It can be used in an outgoing client
// request to explicitly signal that a request has zero bytes.
// An alternative, however, is to simply set [Request.Body] to nil.
public static noBody NoBody = new noBody(nil);

[GoType] partial struct noBody {
}

internal static (nint, error) Read(this noBody _, slice<byte> _) {
    return (0, io.EOF);
}

internal static error Close(this noBody _) {
    return default!;
}

internal static (int64, error) WriteTo(this noBody _, io.Writer _) {
    return (0, default!);
}

internal static io.WriterTo _ᴛ2ʗ = NoBody;
internal static io.ReadCloser _ᴛ3ʗ = NoBody;

// PushOptions describes options for [Pusher.Push].
[GoType] partial struct PushOptions {
    // Method specifies the HTTP method for the promised request.
    // If set, it must be "GET" or "HEAD". Empty means "GET".
    public @string Method;
    // Header specifies additional promised request headers. This cannot
    // include HTTP/2 pseudo header fields like ":path" and ":scheme",
    // which will be added automatically.
    public ΔHeader Header;
}

// Pusher is the interface implemented by ResponseWriters that support
// HTTP/2 server push. For more background, see
// https://tools.ietf.org/html/rfc7540#section-8.2.
[GoType] partial interface Pusher {
    // Push initiates an HTTP/2 server push. This constructs a synthetic
    // request using the given target and options, serializes that request
    // into a PUSH_PROMISE frame, then dispatches that request using the
    // server's request handler. If opts is nil, default options are used.
    //
    // The target must either be an absolute path (like "/path") or an absolute
    // URL that contains a valid host and the same scheme as the parent request.
    // If the target is a path, it will inherit the scheme and host of the
    // parent request.
    //
    // The HTTP/2 spec disallows recursive pushes and cross-authority pushes.
    // Push may or may not detect these invalid pushes; however, invalid
    // pushes will be detected and canceled by conforming clients.
    //
    // Handlers that wish to push URL X should call Push before sending any
    // data that may trigger a request for URL X. This avoids a race where the
    // client issues requests for X before receiving the PUSH_PROMISE for X.
    //
    // Push will run in a separate goroutine making the order of arrival
    // non-deterministic. Any required synchronization needs to be implemented
    // by the caller.
    //
    // Push returns ErrNotSupported if the client has disabled push or if push
    // is not supported on the underlying connection.
    error Push(@string target, ж<PushOptions> opts);
}

} // end http_package
