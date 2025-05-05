// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// HTTP Response reading and parsing.
namespace go.net;

using bufio = bufio_package;
using bytes = bytes_package;
using tls = crypto.tls_package;
using errors = errors_package;
using fmt = fmt_package;
using io = io_package;
using textproto = net.textproto_package;
using url = net.url_package;
using strconv = strconv_package;
using strings = strings_package;
using httpguts = golang.org.x.net.http.httpguts_package;
using crypto;
using golang.org.x.net.http;

partial class http_package {

internal static map<@string, bool> respExcludeHeader = new map<@string, bool>{
    ["Content-Length"u8] = true,
    ["Transfer-Encoding"u8] = true,
    ["Trailer"u8] = true
};

// Response represents the response from an HTTP request.
//
// The [Client] and [Transport] return Responses from servers once
// the response headers have been received. The response body
// is streamed on demand as the Body field is read.
[GoType] partial struct Response {
    public @string Status; // e.g. "200 OK"
    public nint StatusCode;   // e.g. 200
    public @string Proto; // e.g. "HTTP/1.0"
    public nint ProtoMajor;   // e.g. 1
    public nint ProtoMinor;   // e.g. 0
    // Header maps header keys to values. If the response had multiple
    // headers with the same key, they may be concatenated, with comma
    // delimiters.  (RFC 7230, section 3.2.2 requires that multiple headers
    // be semantically equivalent to a comma-delimited sequence.) When
    // Header values are duplicated by other fields in this struct (e.g.,
    // ContentLength, TransferEncoding, Trailer), the field values are
    // authoritative.
    //
    // Keys in the map are canonicalized (see CanonicalHeaderKey).
    public ΔHeader Header;
    // Body represents the response body.
    //
    // The response body is streamed on demand as the Body field
    // is read. If the network connection fails or the server
    // terminates the response, Body.Read calls return an error.
    //
    // The http Client and Transport guarantee that Body is always
    // non-nil, even on responses without a body or responses with
    // a zero-length body. It is the caller's responsibility to
    // close Body. The default HTTP client's Transport may not
    // reuse HTTP/1.x "keep-alive" TCP connections if the Body is
    // not read to completion and closed.
    //
    // The Body is automatically dechunked if the server replied
    // with a "chunked" Transfer-Encoding.
    //
    // As of Go 1.12, the Body will also implement io.Writer
    // on a successful "101 Switching Protocols" response,
    // as used by WebSockets and HTTP/2's "h2c" mode.
    public io_package.ReadCloser Body;
    // ContentLength records the length of the associated content. The
    // value -1 indicates that the length is unknown. Unless Request.Method
    // is "HEAD", values >= 0 indicate that the given number of bytes may
    // be read from Body.
    public int64 ContentLength;
    // Contains transfer encodings from outer-most to inner-most. Value is
    // nil, means that "identity" encoding is used.
    public slice<@string> TransferEncoding;
    // Close records whether the header directed that the connection be
    // closed after reading Body. The value is advice for clients: neither
    // ReadResponse nor Response.Write ever closes a connection.
    public bool Close;
    // Uncompressed reports whether the response was sent compressed but
    // was decompressed by the http package. When true, reading from
    // Body yields the uncompressed content instead of the compressed
    // content actually set from the server, ContentLength is set to -1,
    // and the "Content-Length" and "Content-Encoding" fields are deleted
    // from the responseHeader. To get the original response from
    // the server, set Transport.DisableCompression to true.
    public bool Uncompressed;
    // Trailer maps trailer keys to values in the same
    // format as Header.
    //
    // The Trailer initially contains only nil values, one for
    // each key specified in the server's "Trailer" header
    // value. Those values are not added to Header.
    //
    // Trailer must not be accessed concurrently with Read calls
    // on the Body.
    //
    // After Body.Read has returned io.EOF, Trailer will contain
    // any trailer values sent by the server.
    public ΔHeader Trailer;
    // Request is the request that was sent to obtain this Response.
    // Request's Body is nil (having already been consumed).
    // This is only populated for Client requests.
    public ж<Request> Request;
    // TLS contains information about the TLS connection on which the
    // response was received. It is nil for unencrypted responses.
    // The pointer is shared between responses and should not be
    // modified.
    public ж<crypto.tls_package.ΔConnectionState> TLS;
}

// Cookies parses and returns the cookies set in the Set-Cookie headers.
[GoRecv] public static slice<ж<ΔCookie>> Cookies(this ref Response r) {
    return readSetCookies(r.Header);
}

// ErrNoLocation is returned by the [Response.Location] method
// when no Location header is present.
public static error ErrNoLocation = errors.New("http: no Location header in response"u8);

// Location returns the URL of the response's "Location" header,
// if present. Relative redirects are resolved relative to
// [Response.Request]. [ErrNoLocation] is returned if no
// Location header is present.
[GoRecv] public static (ж<url.URL>, error) Location(this ref Response r) {
    @string lv = r.Header.Get("Location"u8);
    if (lv == ""u8) {
        return (default!, ErrNoLocation);
    }
    if (r.Request != nil && r.Request.URL != nil) {
        return r.Request.URL.Parse(lv);
    }
    return url.Parse(lv);
}

// ReadResponse reads and returns an HTTP response from r.
// The req parameter optionally specifies the [Request] that corresponds
// to this [Response]. If nil, a GET request is assumed.
// Clients must call resp.Body.Close when finished reading resp.Body.
// After that call, clients can inspect resp.Trailer to find key/value
// pairs included in the response trailer.
public static (ж<Response>, error) ReadResponse(ж<bufio.Reader> Ꮡr, ж<Request> Ꮡreq) {
    ref var r = ref Ꮡr.val;
    ref var req = ref Ꮡreq.val;

    var tp = textproto.NewReader(Ꮡr);
    var resp = Ꮡ(new Response(
        Request: req
    ));
    // Parse the first line of the response.
    var (line, err) = tp.ReadLine();
    if (err != default!) {
        if (AreEqual(err, io.EOF)) {
            err = io.ErrUnexpectedEOF;
        }
        return (default!, err);
    }
    var (proto, status, ok) = strings.Cut(line, " "u8);
    if (!ok) {
        return (default!, badStringError("malformed HTTP response"u8, line));
    }
    resp.val.Proto = proto;
    resp.val.Status = strings.TrimLeft(status, " "u8);
    var (statusCode, _, _) = strings.Cut((~resp).Status, " "u8);
    if (len(statusCode) != 3) {
        return (default!, badStringError("malformed HTTP status code"u8, statusCode));
    }
    (resp.val.StatusCode, err) = strconv.Atoi(statusCode);
    if (err != default! || (~resp).StatusCode < 0) {
        return (default!, badStringError("malformed HTTP status code"u8, statusCode));
    }
    {
        (resp.val.ProtoMajor, resp.val.ProtoMinor, ok) = ParseHTTPVersion((~resp).Proto); if (!ok) {
            return (default!, badStringError("malformed HTTP version"u8, (~resp).Proto));
        }
    }
    // Parse the response headers.
    (mimeHeader, err) = tp.ReadMIMEHeader();
    if (err != default!) {
        if (AreEqual(err, io.EOF)) {
            err = io.ErrUnexpectedEOF;
        }
        return (default!, err);
    }
    resp.val.Header = ((ΔHeader)mimeHeader);
    fixPragmaCacheControl((~resp).Header);
    err = readTransfer(resp, Ꮡr);
    if (err != default!) {
        return (default!, err);
    }
    return (resp, default!);
}

// RFC 7234, section 5.4: Should treat
//
//	Pragma: no-cache
//
// like
//
//	Cache-Control: no-cache
internal static void fixPragmaCacheControl(ΔHeader header) {
    {
        var hp = header["Pragma"u8];
        var ok = header["Pragma"u8]; if (ok && len(hp) > 0 && hp[0] == "no-cache") {
            {
                var _ = header["Cache-Control"u8];
                var presentcc = header["Cache-Control"u8]; if (!presentcc) {
                    header["Cache-Control"u8] = new @string[]{"no-cache"}.slice();
                }
            }
        }
    }
}

// ProtoAtLeast reports whether the HTTP protocol used
// in the response is at least major.minor.
[GoRecv] public static bool ProtoAtLeast(this ref Response r, nint major, nint minor) {
    return r.ProtoMajor > major || r.ProtoMajor == major && r.ProtoMinor >= minor;
}

[GoType("dyn")] partial struct Write_r1 {
    public partial ref io_package.Reader Reader { get; }
    public partial ref io_package.Closer Closer { get; }
}

// Write writes r to w in the HTTP/1.x server response format,
// including the status line, headers, body, and optional trailer.
//
// This method consults the following fields of the response r:
//
//	StatusCode
//	ProtoMajor
//	ProtoMinor
//	Request.Method
//	TransferEncoding
//	Trailer
//	Body
//	ContentLength
//	Header, values for non-canonical keys will have unpredictable behavior
//
// The Response Body is closed after it is sent.
[GoRecv] public static error Write(this ref Response r, io.Writer w) {
    // Status line
    @string text = r.Status;
    if (text == ""u8){
        text = StatusText(r.StatusCode);
        if (text == ""u8) {
            text = "status code "u8 + strconv.Itoa(r.StatusCode);
        }
    } else {
        // Just to reduce stutter, if user set r.Status to "200 OK" and StatusCode to 200.
        // Not important.
        text = strings.TrimPrefix(text, strconv.Itoa(r.StatusCode) + " "u8);
    }
    {
        var (_, errΔ1) = fmt.Fprintf(w, "HTTP/%d.%d %03d %s\r\n"u8, r.ProtoMajor, r.ProtoMinor, r.StatusCode, text); if (errΔ1 != default!) {
            return errΔ1;
        }
    }
    // Clone it, so we can modify r1 as needed.
    var r1 = @new<Response>();
    r1.val = r;
    if ((~r1).ContentLength == 0 && (~r1).Body != default!) {
        // Is it actually 0 length? Or just unknown?
        array<byte> buf = new(1);
        var (n, errΔ2) = (~r1).Body.Read(buf[..]);
        if (errΔ2 != default! && !AreEqual(errΔ2, io.EOF)) {
            return errΔ2;
        }
        if (n == 0){
            // Reset it to a known zero reader, in case underlying one
            // is unhappy being read repeatedly.
            r1.val.Body = NoBody;
        } else {
            r1.val.ContentLength = -1;
            r1.val.Body = new Write_r1(
                io.MultiReader(~bytes.NewReader(buf[..1]), r.Body),
                r.Body
            );
        }
    }
    // If we're sending a non-chunked HTTP/1.1 response without a
    // content-length, the only way to do that is the old HTTP/1.0
    // way, by noting the EOF with a connection close, so we need
    // to set Close.
    if ((~r1).ContentLength == -1 && !(~r1).Close && r1.ProtoAtLeast(1, 1) && !chunked((~r1).TransferEncoding) && !(~r1).Uncompressed) {
        r1.val.Close = true;
    }
    // Process Body,ContentLength,Close,Trailer
    (tw, err) = newTransferWriter(r1);
    if (err != default!) {
        return err;
    }
    err = tw.writeHeader(w, nil);
    if (err != default!) {
        return err;
    }
    // Rest of header
    err = r.Header.WriteSubset(w, respExcludeHeader);
    if (err != default!) {
        return err;
    }
    // contentLengthAlreadySent may have been already sent for
    // POST/PUT requests, even if zero length. See Issue 8180.
    var contentLengthAlreadySent = tw.shouldSendContentLength();
    if ((~r1).ContentLength == 0 && !chunked((~r1).TransferEncoding) && !contentLengthAlreadySent && bodyAllowedForStatus(r.StatusCode)) {
        {
            var (_, errΔ3) = io.WriteString(w, "Content-Length: 0\r\n"u8); if (errΔ3 != default!) {
                return errΔ3;
            }
        }
    }
    // End-of-header
    {
        var (_, errΔ4) = io.WriteString(w, "\r\n"u8); if (errΔ4 != default!) {
            return errΔ4;
        }
    }
    // Write body and trailer
    err = tw.writeBody(w);
    if (err != default!) {
        return err;
    }
    // Success
    return default!;
}

[GoRecv] internal static void closeBody(this ref Response r) {
    if (r.Body != default!) {
        r.Body.Close();
    }
}

// bodyIsWritable reports whether the Body supports writing. The
// Transport returns Writable bodies for 101 Switching Protocols
// responses.
// The Transport uses this method to determine whether a persistent
// connection is done being managed from its perspective. Once we
// return a writable response body to a user, the net/http package is
// done managing that connection.
[GoRecv] internal static bool bodyIsWritable(this ref Response r) {
    var (_, ok) = r.Body._<io.Writer>(ᐧ);
    return ok;
}

// isProtocolSwitch reports whether the response code and header
// indicate a successful protocol upgrade response.
[GoRecv] internal static bool isProtocolSwitch(this ref Response r) {
    return isProtocolSwitchResponse(r.StatusCode, r.Header);
}

// isProtocolSwitchResponse reports whether the response code and
// response header indicate a successful protocol upgrade response.
internal static bool isProtocolSwitchResponse(nint code, ΔHeader h) {
    return code == StatusSwitchingProtocols && isProtocolSwitchHeader(h);
}

// isProtocolSwitchHeader reports whether the request or response header
// is for a protocol switch.
internal static bool isProtocolSwitchHeader(ΔHeader h) {
    return h.Get("Upgrade"u8) != ""u8 && httpguts.HeaderValuesContainsToken(h["Connection"u8], "Upgrade"u8);
}

} // end http_package
