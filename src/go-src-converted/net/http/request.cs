// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// HTTP Request reading and parsing.

// package http -- go2cs converted at 2022 March 13 05:37:18 UTC
// import "net/http" ==> using http = go.net.http_package
// Original source: C:\Program Files\Go\src\net\http\request.go
namespace go.net;

using bufio = bufio_package;
using bytes = bytes_package;
using context = context_package;
using tls = crypto.tls_package;
using base64 = encoding.base64_package;
using errors = errors_package;
using fmt = fmt_package;
using io = io_package;
using mime = mime_package;
using multipart = mime.multipart_package;
using net = net_package;
using httptrace = net.http.httptrace_package;
using ascii = net.http.@internal.ascii_package;
using textproto = net.textproto_package;
using url = net.url_package;
using url = net.url_package;
using strconv = strconv_package;
using strings = strings_package;
using sync = sync_package;

using idna = golang.org.x.net.idna_package;
using System;

public static partial class http_package {

private static readonly nint defaultMaxMemory = 32 << 20; // 32 MB

// ErrMissingFile is returned by FormFile when the provided file field name
// is either not present in the request or not a file field.
public static var ErrMissingFile = errors.New("http: no such file");

// ProtocolError represents an HTTP protocol error.
//
// Deprecated: Not all errors in the http package related to protocol errors
// are of type ProtocolError.
public partial struct ProtocolError {
    public @string ErrorString;
}

private static @string Error(this ptr<ProtocolError> _addr_pe) {
    ref ProtocolError pe = ref _addr_pe.val;

    return pe.ErrorString;
}

 
// ErrNotSupported is returned by the Push method of Pusher
// implementations to indicate that HTTP/2 Push support is not
// available.
public static ptr<ProtocolError> ErrNotSupported = addr(new ProtocolError("feature not supported"));public static ptr<ProtocolError> ErrUnexpectedTrailer = addr(new ProtocolError("trailer header without chunked transfer encoding"));public static ptr<ProtocolError> ErrMissingBoundary = addr(new ProtocolError("no multipart boundary param in Content-Type"));public static ptr<ProtocolError> ErrNotMultipart = addr(new ProtocolError("request Content-Type isn't multipart/form-data"));public static ptr<ProtocolError> ErrHeaderTooLong = addr(new ProtocolError("header too long"));public static ptr<ProtocolError> ErrShortBody = addr(new ProtocolError("entity body too short"));public static ptr<ProtocolError> ErrMissingContentLength = addr(new ProtocolError("missing ContentLength in HEAD response"));

private static error badStringError(@string what, @string val) {
    return error.As(fmt.Errorf("%s %q", what, val))!;
}

// Headers that Request.Write handles itself and should be skipped.
private static map reqWriteExcludeHeader = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, bool>{"Host":true,"User-Agent":true,"Content-Length":true,"Transfer-Encoding":true,"Trailer":true,};

// A Request represents an HTTP request received by a server
// or to be sent by a client.
//
// The field semantics differ slightly between client and server
// usage. In addition to the notes on the fields below, see the
// documentation for Request.Write and RoundTripper.
public partial struct Request {
    public @string Method; // URL specifies either the URI being requested (for server
// requests) or the URL to access (for client requests).
//
// For server requests, the URL is parsed from the URI
// supplied on the Request-Line as stored in RequestURI.  For
// most requests, fields other than Path and RawQuery will be
// empty. (See RFC 7230, Section 5.3)
//
// For client requests, the URL's Host specifies the server to
// connect to, while the Request's Host field optionally
// specifies the Host header value to send in the HTTP
// request.
    public ptr<url.URL> URL; // The protocol version for incoming server requests.
//
// For client requests, these fields are ignored. The HTTP
// client code always uses either HTTP/1.1 or HTTP/2.
// See the docs on Transport for details.
    public @string Proto; // "HTTP/1.0"
    public nint ProtoMajor; // 1
    public nint ProtoMinor; // 0

// Header contains the request header fields either received
// by the server or to be sent by the client.
//
// If a server received a request with header lines,
//
//    Host: example.com
//    accept-encoding: gzip, deflate
//    Accept-Language: en-us
//    fOO: Bar
//    foo: two
//
// then
//
//    Header = map[string][]string{
//        "Accept-Encoding": {"gzip, deflate"},
//        "Accept-Language": {"en-us"},
//        "Foo": {"Bar", "two"},
//    }
//
// For incoming requests, the Host header is promoted to the
// Request.Host field and removed from the Header map.
//
// HTTP defines that header names are case-insensitive. The
// request parser implements this by using CanonicalHeaderKey,
// making the first character and any characters following a
// hyphen uppercase and the rest lowercase.
//
// For client requests, certain headers such as Content-Length
// and Connection are automatically written when needed and
// values in Header may be ignored. See the documentation
// for the Request.Write method.
    public Header Header; // Body is the request's body.
//
// For client requests, a nil body means the request has no
// body, such as a GET request. The HTTP Client's Transport
// is responsible for calling the Close method.
//
// For server requests, the Request Body is always non-nil
// but will return EOF immediately when no body is present.
// The Server will close the request body. The ServeHTTP
// Handler does not need to.
//
// Body must allow Read to be called concurrently with Close.
// In particular, calling Close should unblock a Read waiting
// for input.
    public io.ReadCloser Body; // GetBody defines an optional func to return a new copy of
// Body. It is used for client requests when a redirect requires
// reading the body more than once. Use of GetBody still
// requires setting Body.
//
// For server requests, it is unused.
    public Func<(io.ReadCloser, error)> GetBody; // ContentLength records the length of the associated content.
// The value -1 indicates that the length is unknown.
// Values >= 0 indicate that the given number of bytes may
// be read from Body.
//
// For client requests, a value of 0 with a non-nil Body is
// also treated as unknown.
    public long ContentLength; // TransferEncoding lists the transfer encodings from outermost to
// innermost. An empty list denotes the "identity" encoding.
// TransferEncoding can usually be ignored; chunked encoding is
// automatically added and removed as necessary when sending and
// receiving requests.
    public slice<@string> TransferEncoding; // Close indicates whether to close the connection after
// replying to this request (for servers) or after sending this
// request and reading its response (for clients).
//
// For server requests, the HTTP server handles this automatically
// and this field is not needed by Handlers.
//
// For client requests, setting this field prevents re-use of
// TCP connections between requests to the same hosts, as if
// Transport.DisableKeepAlives were set.
    public bool Close; // For server requests, Host specifies the host on which the
// URL is sought. For HTTP/1 (per RFC 7230, section 5.4), this
// is either the value of the "Host" header or the host name
// given in the URL itself. For HTTP/2, it is the value of the
// ":authority" pseudo-header field.
// It may be of the form "host:port". For international domain
// names, Host may be in Punycode or Unicode form. Use
// golang.org/x/net/idna to convert it to either format if
// needed.
// To prevent DNS rebinding attacks, server Handlers should
// validate that the Host header has a value for which the
// Handler considers itself authoritative. The included
// ServeMux supports patterns registered to particular host
// names and thus protects its registered Handlers.
//
// For client requests, Host optionally overrides the Host
// header to send. If empty, the Request.Write method uses
// the value of URL.Host. Host may contain an international
// domain name.
    public @string Host; // Form contains the parsed form data, including both the URL
// field's query parameters and the PATCH, POST, or PUT form data.
// This field is only available after ParseForm is called.
// The HTTP client ignores Form and uses Body instead.
    public url.Values Form; // PostForm contains the parsed form data from PATCH, POST
// or PUT body parameters.
//
// This field is only available after ParseForm is called.
// The HTTP client ignores PostForm and uses Body instead.
    public url.Values PostForm; // MultipartForm is the parsed multipart form, including file uploads.
// This field is only available after ParseMultipartForm is called.
// The HTTP client ignores MultipartForm and uses Body instead.
    public ptr<multipart.Form> MultipartForm; // Trailer specifies additional headers that are sent after the request
// body.
//
// For server requests, the Trailer map initially contains only the
// trailer keys, with nil values. (The client declares which trailers it
// will later send.)  While the handler is reading from Body, it must
// not reference Trailer. After reading from Body returns EOF, Trailer
// can be read again and will contain non-nil values, if they were sent
// by the client.
//
// For client requests, Trailer must be initialized to a map containing
// the trailer keys to later send. The values may be nil or their final
// values. The ContentLength must be 0 or -1, to send a chunked request.
// After the HTTP request is sent the map values can be updated while
// the request body is read. Once the body returns EOF, the caller must
// not mutate Trailer.
//
// Few HTTP clients, servers, or proxies support HTTP trailers.
    public Header Trailer; // RemoteAddr allows HTTP servers and other software to record
// the network address that sent the request, usually for
// logging. This field is not filled in by ReadRequest and
// has no defined format. The HTTP server in this package
// sets RemoteAddr to an "IP:port" address before invoking a
// handler.
// This field is ignored by the HTTP client.
    public @string RemoteAddr; // RequestURI is the unmodified request-target of the
// Request-Line (RFC 7230, Section 3.1.1) as sent by the client
// to a server. Usually the URL field should be used instead.
// It is an error to set this field in an HTTP client request.
    public @string RequestURI; // TLS allows HTTP servers and other software to record
// information about the TLS connection on which the request
// was received. This field is not filled in by ReadRequest.
// The HTTP server in this package sets the field for
// TLS-enabled connections before invoking a handler;
// otherwise it leaves the field nil.
// This field is ignored by the HTTP client.
    public ptr<tls.ConnectionState> TLS; // Cancel is an optional channel whose closure indicates that the client
// request should be regarded as canceled. Not all implementations of
// RoundTripper may support Cancel.
//
// For server requests, this field is not applicable.
//
// Deprecated: Set the Request's context with NewRequestWithContext
// instead. If a Request's Cancel field and context are both
// set, it is undefined whether Cancel is respected.
    public channel<object> Cancel; // Response is the redirect response which caused this request
// to be created. This field is only populated during client
// redirects.
    public ptr<Response> Response; // ctx is either the client or server context. It should only
// be modified via copying the whole Request using WithContext.
// It is unexported to prevent people from using Context wrong
// and mutating the contexts held by callers of the same request.
    public context.Context ctx;
}

// Context returns the request's context. To change the context, use
// WithContext.
//
// The returned context is always non-nil; it defaults to the
// background context.
//
// For outgoing client requests, the context controls cancellation.
//
// For incoming server requests, the context is canceled when the
// client's connection closes, the request is canceled (with HTTP/2),
// or when the ServeHTTP method returns.
private static context.Context Context(this ptr<Request> _addr_r) {
    ref Request r = ref _addr_r.val;

    if (r.ctx != null) {
        return r.ctx;
    }
    return context.Background();
}

// WithContext returns a shallow copy of r with its context changed
// to ctx. The provided ctx must be non-nil.
//
// For outgoing client request, the context controls the entire
// lifetime of a request and its response: obtaining a connection,
// sending the request, and reading the response headers and body.
//
// To create a new request with a context, use NewRequestWithContext.
// To change the context of a request, such as an incoming request you
// want to modify before sending back out, use Request.Clone. Between
// those two uses, it's rare to need WithContext.
private static ptr<Request> WithContext(this ptr<Request> _addr_r, context.Context ctx) => func((_, panic, _) => {
    ref Request r = ref _addr_r.val;

    if (ctx == null) {
        panic("nil context");
    }
    ptr<Request> r2 = @new<Request>();
    r2.val = r.val;
    r2.ctx = ctx;
    r2.URL = cloneURL(r.URL); // legacy behavior; TODO: try to remove. Issue 23544
    return _addr_r2!;
});

// Clone returns a deep copy of r with its context changed to ctx.
// The provided ctx must be non-nil.
//
// For an outgoing client request, the context controls the entire
// lifetime of a request and its response: obtaining a connection,
// sending the request, and reading the response headers and body.
private static ptr<Request> Clone(this ptr<Request> _addr_r, context.Context ctx) => func((_, panic, _) => {
    ref Request r = ref _addr_r.val;

    if (ctx == null) {
        panic("nil context");
    }
    ptr<Request> r2 = @new<Request>();
    r2.val = r.val;
    r2.ctx = ctx;
    r2.URL = cloneURL(r.URL);
    if (r.Header != null) {
        r2.Header = r.Header.Clone();
    }
    if (r.Trailer != null) {
        r2.Trailer = r.Trailer.Clone();
    }
    {
        var s = r.TransferEncoding;

        if (s != null) {
            var s2 = make_slice<@string>(len(s));
            copy(s2, s);
            r2.TransferEncoding = s2;
        }
    }
    r2.Form = cloneURLValues(r.Form);
    r2.PostForm = cloneURLValues(r.PostForm);
    r2.MultipartForm = cloneMultipartForm(r.MultipartForm);
    return _addr_r2!;
});

// ProtoAtLeast reports whether the HTTP protocol used
// in the request is at least major.minor.
private static bool ProtoAtLeast(this ptr<Request> _addr_r, nint major, nint minor) {
    ref Request r = ref _addr_r.val;

    return r.ProtoMajor > major || r.ProtoMajor == major && r.ProtoMinor >= minor;
}

// UserAgent returns the client's User-Agent, if sent in the request.
private static @string UserAgent(this ptr<Request> _addr_r) {
    ref Request r = ref _addr_r.val;

    return r.Header.Get("User-Agent");
}

// Cookies parses and returns the HTTP cookies sent with the request.
private static slice<ptr<Cookie>> Cookies(this ptr<Request> _addr_r) {
    ref Request r = ref _addr_r.val;

    return readCookies(r.Header, "");
}

// ErrNoCookie is returned by Request's Cookie method when a cookie is not found.
public static var ErrNoCookie = errors.New("http: named cookie not present");

// Cookie returns the named cookie provided in the request or
// ErrNoCookie if not found.
// If multiple cookies match the given name, only one cookie will
// be returned.
private static (ptr<Cookie>, error) Cookie(this ptr<Request> _addr_r, @string name) {
    ptr<Cookie> _p0 = default!;
    error _p0 = default!;
    ref Request r = ref _addr_r.val;

    foreach (var (_, c) in readCookies(r.Header, name)) {
        return (_addr_c!, error.As(null!)!);
    }    return (_addr_null!, error.As(ErrNoCookie)!);
}

// AddCookie adds a cookie to the request. Per RFC 6265 section 5.4,
// AddCookie does not attach more than one Cookie header field. That
// means all cookies, if any, are written into the same line,
// separated by semicolon.
// AddCookie only sanitizes c's name and value, and does not sanitize
// a Cookie header already present in the request.
private static void AddCookie(this ptr<Request> _addr_r, ptr<Cookie> _addr_c) {
    ref Request r = ref _addr_r.val;
    ref Cookie c = ref _addr_c.val;

    var s = fmt.Sprintf("%s=%s", sanitizeCookieName(c.Name), sanitizeCookieValue(c.Value));
    {
        var c = r.Header.Get("Cookie");

        if (c != "") {
            r.Header.Set("Cookie", c + "; " + s);
        }
        else
 {
            r.Header.Set("Cookie", s);
        }
    }
}

// Referer returns the referring URL, if sent in the request.
//
// Referer is misspelled as in the request itself, a mistake from the
// earliest days of HTTP.  This value can also be fetched from the
// Header map as Header["Referer"]; the benefit of making it available
// as a method is that the compiler can diagnose programs that use the
// alternate (correct English) spelling req.Referrer() but cannot
// diagnose programs that use Header["Referrer"].
private static @string Referer(this ptr<Request> _addr_r) {
    ref Request r = ref _addr_r.val;

    return r.Header.Get("Referer");
}

// multipartByReader is a sentinel value.
// Its presence in Request.MultipartForm indicates that parsing of the request
// body has been handed off to a MultipartReader instead of ParseMultipartForm.
private static ptr<multipart.Form> multipartByReader = addr(new multipart.Form(Value:make(map[string][]string),File:make(map[string][]*multipart.FileHeader),));

// MultipartReader returns a MIME multipart reader if this is a
// multipart/form-data or a multipart/mixed POST request, else returns nil and an error.
// Use this function instead of ParseMultipartForm to
// process the request body as a stream.
private static (ptr<multipart.Reader>, error) MultipartReader(this ptr<Request> _addr_r) {
    ptr<multipart.Reader> _p0 = default!;
    error _p0 = default!;
    ref Request r = ref _addr_r.val;

    if (r.MultipartForm == multipartByReader) {
        return (_addr_null!, error.As(errors.New("http: MultipartReader called twice"))!);
    }
    if (r.MultipartForm != null) {
        return (_addr_null!, error.As(errors.New("http: multipart handled by ParseMultipartForm"))!);
    }
    r.MultipartForm = multipartByReader;
    return _addr_r.multipartReader(true)!;
}

private static (ptr<multipart.Reader>, error) multipartReader(this ptr<Request> _addr_r, bool allowMixed) {
    ptr<multipart.Reader> _p0 = default!;
    error _p0 = default!;
    ref Request r = ref _addr_r.val;

    var v = r.Header.Get("Content-Type");
    if (v == "") {
        return (_addr_null!, error.As(ErrNotMultipart)!);
    }
    var (d, params, err) = mime.ParseMediaType(v);
    if (err != null || !(d == "multipart/form-data" || allowMixed && d == "multipart/mixed")) {
        return (_addr_null!, error.As(ErrNotMultipart)!);
    }
    var (boundary, ok) = params["boundary"];
    if (!ok) {
        return (_addr_null!, error.As(ErrMissingBoundary)!);
    }
    return (_addr_multipart.NewReader(r.Body, boundary)!, error.As(null!)!);
}

// isH2Upgrade reports whether r represents the http2 "client preface"
// magic string.
private static bool isH2Upgrade(this ptr<Request> _addr_r) {
    ref Request r = ref _addr_r.val;

    return r.Method == "PRI" && len(r.Header) == 0 && r.URL.Path == "*" && r.Proto == "HTTP/2.0";
}

// Return value if nonempty, def otherwise.
private static @string valueOrDefault(@string value, @string def) {
    if (value != "") {
        return value;
    }
    return def;
}

// NOTE: This is not intended to reflect the actual Go version being used.
// It was changed at the time of Go 1.1 release because the former User-Agent
// had ended up blocked by some intrusion detection systems.
// See https://codereview.appspot.com/7532043.
private static readonly @string defaultUserAgent = "Go-http-client/1.1";

// Write writes an HTTP/1.1 request, which is the header and body, in wire format.
// This method consults the following fields of the request:
//    Host
//    URL
//    Method (defaults to "GET")
//    Header
//    ContentLength
//    TransferEncoding
//    Body
//
// If Body is present, Content-Length is <= 0 and TransferEncoding
// hasn't been set to "identity", Write adds "Transfer-Encoding:
// chunked" to the header. Body is closed after it is sent.


// Write writes an HTTP/1.1 request, which is the header and body, in wire format.
// This method consults the following fields of the request:
//    Host
//    URL
//    Method (defaults to "GET")
//    Header
//    ContentLength
//    TransferEncoding
//    Body
//
// If Body is present, Content-Length is <= 0 and TransferEncoding
// hasn't been set to "identity", Write adds "Transfer-Encoding:
// chunked" to the header. Body is closed after it is sent.
private static error Write(this ptr<Request> _addr_r, io.Writer w) {
    ref Request r = ref _addr_r.val;

    return error.As(r.write(w, false, null, null))!;
}

// WriteProxy is like Write but writes the request in the form
// expected by an HTTP proxy. In particular, WriteProxy writes the
// initial Request-URI line of the request with an absolute URI, per
// section 5.3 of RFC 7230, including the scheme and host.
// In either case, WriteProxy also writes a Host header, using
// either r.Host or r.URL.Host.
private static error WriteProxy(this ptr<Request> _addr_r, io.Writer w) {
    ref Request r = ref _addr_r.val;

    return error.As(r.write(w, true, null, null))!;
}

// errMissingHost is returned by Write when there is no Host or URL present in
// the Request.
private static var errMissingHost = errors.New("http: Request.Write on Request with no Host or URL set");

// extraHeaders may be nil
// waitForContinue may be nil
// always closes body
private static error write(this ptr<Request> _addr_r, io.Writer w, bool usingProxy, Header extraHeaders, Func<bool> waitForContinue) => func((defer, _, _) => {
    error err = default!;
    ref Request r = ref _addr_r.val;

    var trace = httptrace.ContextClientTrace(r.Context());
    if (trace != null && trace.WroteRequest != null) {
        defer(() => {
            trace.WroteRequest(new httptrace.WroteRequestInfo(Err:err,));
        }());
    }
    var closed = false;
    defer(() => {
        if (closed) {
            return ;
        }
        {
            var closeErr = r.closeBody();

            if (closeErr != null && err == null) {
                err = closeErr;
            }

        }
    }()); 

    // Find the target host. Prefer the Host: header, but if that
    // is not given, use the host from the request URL.
    //
    // Clean the host, in case it arrives with unexpected stuff in it.
    var host = cleanHost(r.Host);
    if (host == "") {
        if (r.URL == null) {
            return error.As(errMissingHost)!;
        }
        host = cleanHost(r.URL.Host);
    }
    host = removeZone(host);

    var ruri = r.URL.RequestURI();
    if (usingProxy && r.URL.Scheme != "" && r.URL.Opaque == "") {
        ruri = r.URL.Scheme + "://" + host + ruri;
    }
    else if (r.Method == "CONNECT" && r.URL.Path == "") { 
        // CONNECT requests normally give just the host and port, not a full URL.
        ruri = host;
        if (r.URL.Opaque != "") {
            ruri = r.URL.Opaque;
        }
    }
    if (stringContainsCTLByte(ruri)) {
        return error.As(errors.New("net/http: can't write control character in Request.URL"))!;
    }
    ptr<bufio.Writer> bw;
    {
        io.ByteWriter (_, ok) = w._<io.ByteWriter>();

        if (!ok) {
            bw = bufio.NewWriter(w);
            w = bw;
        }
    }

    _, err = fmt.Fprintf(w, "%s %s HTTP/1.1\r\n", valueOrDefault(r.Method, "GET"), ruri);
    if (err != null) {
        return error.As(err)!;
    }
    _, err = fmt.Fprintf(w, "Host: %s\r\n", host);
    if (err != null) {
        return error.As(err)!;
    }
    if (trace != null && trace.WroteHeaderField != null) {
        trace.WroteHeaderField("Host", new slice<@string>(new @string[] { host }));
    }
    var userAgent = defaultUserAgent;
    if (r.Header.has("User-Agent")) {
        userAgent = r.Header.Get("User-Agent");
    }
    if (userAgent != "") {
        _, err = fmt.Fprintf(w, "User-Agent: %s\r\n", userAgent);
        if (err != null) {
            return error.As(err)!;
        }
        if (trace != null && trace.WroteHeaderField != null) {
            trace.WroteHeaderField("User-Agent", new slice<@string>(new @string[] { userAgent }));
        }
    }
    var (tw, err) = newTransferWriter(r);
    if (err != null) {
        return error.As(err)!;
    }
    err = tw.writeHeader(w, trace);
    if (err != null) {
        return error.As(err)!;
    }
    err = r.Header.writeSubset(w, reqWriteExcludeHeader, trace);
    if (err != null) {
        return error.As(err)!;
    }
    if (extraHeaders != null) {
        err = extraHeaders.write(w, trace);
        if (err != null) {
            return error.As(err)!;
        }
    }
    _, err = io.WriteString(w, "\r\n");
    if (err != null) {
        return error.As(err)!;
    }
    if (trace != null && trace.WroteHeaders != null) {
        trace.WroteHeaders();
    }
    if (waitForContinue != null) {
        {
            ptr<bufio.Writer> bw__prev2 = bw;

            ptr<bufio.Writer> (bw, ok) = w._<ptr<bufio.Writer>>();

            if (ok) {
                err = bw.Flush();
                if (err != null) {
                    return error.As(err)!;
                }
            }

            bw = bw__prev2;

        }
        if (trace != null && trace.Wait100Continue != null) {
            trace.Wait100Continue();
        }
        if (!waitForContinue()) {
            closed = true;
            r.closeBody();
            return error.As(null!)!;
        }
    }
    {
        ptr<bufio.Writer> bw__prev1 = bw;

        (bw, ok) = w._<ptr<bufio.Writer>>();

        if (ok && tw.FlushHeaders) {
            {
                var err = bw.Flush();

                if (err != null) {
                    return error.As(err)!;
                }

            }
        }
        bw = bw__prev1;

    } 

    // Write body and trailer
    closed = true;
    err = tw.writeBody(w);
    if (err != null) {
        if (tw.bodyReadError == err) {
            err = new requestBodyReadError(err);
        }
        return error.As(err)!;
    }
    if (bw != null) {
        return error.As(bw.Flush())!;
    }
    return error.As(null!)!;
});

// requestBodyReadError wraps an error from (*Request).write to indicate
// that the error came from a Read call on the Request.Body.
// This error type should not escape the net/http package to users.
private partial struct requestBodyReadError : error {
    public error error;
}

private static (@string, error) idnaASCII(@string v) {
    @string _p0 = default;
    error _p0 = default!;
 
    // TODO: Consider removing this check after verifying performance is okay.
    // Right now punycode verification, length checks, context checks, and the
    // permissible character tests are all omitted. It also prevents the ToASCII
    // call from salvaging an invalid IDN, when possible. As a result it may be
    // possible to have two IDNs that appear identical to the user where the
    // ASCII-only version causes an error downstream whereas the non-ASCII
    // version does not.
    // Note that for correct ASCII IDNs ToASCII will only do considerably more
    // work, but it will not cause an allocation.
    if (ascii.Is(v)) {
        return (v, error.As(null!)!);
    }
    return idna.Lookup.ToASCII(v);
}

// cleanHost cleans up the host sent in request's Host header.
//
// It both strips anything after '/' or ' ', and puts the value
// into Punycode form, if necessary.
//
// Ideally we'd clean the Host header according to the spec:
//   https://tools.ietf.org/html/rfc7230#section-5.4 (Host = uri-host [ ":" port ]")
//   https://tools.ietf.org/html/rfc7230#section-2.7 (uri-host -> rfc3986's host)
//   https://tools.ietf.org/html/rfc3986#section-3.2.2 (definition of host)
// But practically, what we are trying to avoid is the situation in
// issue 11206, where a malformed Host header used in the proxy context
// would create a bad request. So it is enough to just truncate at the
// first offending character.
private static @string cleanHost(@string @in) {
    {
        var i = strings.IndexAny(in, " /");

        if (i != -1) {
            in = in[..(int)i];
        }
    }
    var (host, port, err) = net.SplitHostPort(in);
    if (err != null) { // input was just a host
        var (a, err) = idnaASCII(in);
        if (err != null) {
            return in; // garbage in, garbage out
        }
        return a;
    }
    (a, err) = idnaASCII(host);
    if (err != null) {
        return in; // garbage in, garbage out
    }
    return net.JoinHostPort(a, port);
}

// removeZone removes IPv6 zone identifier from host.
// E.g., "[fe80::1%en0]:8080" to "[fe80::1]:8080"
private static @string removeZone(@string host) {
    if (!strings.HasPrefix(host, "[")) {
        return host;
    }
    var i = strings.LastIndex(host, "]");
    if (i < 0) {
        return host;
    }
    var j = strings.LastIndex(host[..(int)i], "%");
    if (j < 0) {
        return host;
    }
    return host[..(int)j] + host[(int)i..];
}

// ParseHTTPVersion parses an HTTP version string.
// "HTTP/1.0" returns (1, 0, true). Note that strings without
// a minor version, such as "HTTP/2", are not valid.
public static (nint, nint, bool) ParseHTTPVersion(@string vers) {
    nint major = default;
    nint minor = default;
    bool ok = default;

    const nint Big = 1000000; // arbitrary upper bound
 // arbitrary upper bound
    switch (vers) {
        case "HTTP/1.1": 
            return (1, 1, true);
            break;
        case "HTTP/1.0": 
            return (1, 0, true);
            break;
    }
    if (!strings.HasPrefix(vers, "HTTP/")) {
        return (0, 0, false);
    }
    var dot = strings.Index(vers, ".");
    if (dot < 0) {
        return (0, 0, false);
    }
    var (major, err) = strconv.Atoi(vers[(int)5..(int)dot]);
    if (err != null || major < 0 || major > Big) {
        return (0, 0, false);
    }
    minor, err = strconv.Atoi(vers[(int)dot + 1..]);
    if (err != null || minor < 0 || minor > Big) {
        return (0, 0, false);
    }
    return (major, minor, true);
}

private static bool validMethod(@string method) {
    /*
             Method         = "OPTIONS"                ; Section 9.2
                            | "GET"                    ; Section 9.3
                            | "HEAD"                   ; Section 9.4
                            | "POST"                   ; Section 9.5
                            | "PUT"                    ; Section 9.6
                            | "DELETE"                 ; Section 9.7
                            | "TRACE"                  ; Section 9.8
                            | "CONNECT"                ; Section 9.9
                            | extension-method
           extension-method = token
             token          = 1*<any CHAR except CTLs or separators>
        */
    return len(method) > 0 && strings.IndexFunc(method, isNotToken) == -1;
}

// NewRequest wraps NewRequestWithContext using context.Background.
public static (ptr<Request>, error) NewRequest(@string method, @string url, io.Reader body) {
    ptr<Request> _p0 = default!;
    error _p0 = default!;

    return _addr_NewRequestWithContext(context.Background(), method, url, body)!;
}

// NewRequestWithContext returns a new Request given a method, URL, and
// optional body.
//
// If the provided body is also an io.Closer, the returned
// Request.Body is set to body and will be closed by the Client
// methods Do, Post, and PostForm, and Transport.RoundTrip.
//
// NewRequestWithContext returns a Request suitable for use with
// Client.Do or Transport.RoundTrip. To create a request for use with
// testing a Server Handler, either use the NewRequest function in the
// net/http/httptest package, use ReadRequest, or manually update the
// Request fields. For an outgoing client request, the context
// controls the entire lifetime of a request and its response:
// obtaining a connection, sending the request, and reading the
// response headers and body. See the Request type's documentation for
// the difference between inbound and outbound request fields.
//
// If body is of type *bytes.Buffer, *bytes.Reader, or
// *strings.Reader, the returned request's ContentLength is set to its
// exact value (instead of -1), GetBody is populated (so 307 and 308
// redirects can replay the body), and Body is set to NoBody if the
// ContentLength is 0.
public static (ptr<Request>, error) NewRequestWithContext(context.Context ctx, @string method, @string url, io.Reader body) {
    ptr<Request> _p0 = default!;
    error _p0 = default!;

    if (method == "") { 
        // We document that "" means "GET" for Request.Method, and people have
        // relied on that from NewRequest, so keep that working.
        // We still enforce validMethod for non-empty methods.
        method = "GET";
    }
    if (!validMethod(method)) {
        return (_addr_null!, error.As(fmt.Errorf("net/http: invalid method %q", method))!);
    }
    if (ctx == null) {
        return (_addr_null!, error.As(errors.New("net/http: nil Context"))!);
    }
    var (u, err) = urlpkg.Parse(url);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    io.ReadCloser (rc, ok) = body._<io.ReadCloser>();
    if (!ok && body != null) {
        rc = io.NopCloser(body);
    }
    u.Host = removeEmptyPort(u.Host);
    ptr<Request> req = addr(new Request(ctx:ctx,Method:method,URL:u,Proto:"HTTP/1.1",ProtoMajor:1,ProtoMinor:1,Header:make(Header),Body:rc,Host:u.Host,));
    if (body != null) {
        switch (body.type()) {
            case ptr<bytes.Buffer> v:
                req.ContentLength = int64(v.Len());
                var buf = v.Bytes();
                req.GetBody = () => {
                    ref var r = ref heap(bytes.NewReader(buf), out ptr<var> _addr_r);
                    return (_addr_io.NopCloser(r)!, error.As(null!)!);
                }
;
                break;
            case ptr<bytes.Reader> v:
                req.ContentLength = int64(v.Len());
                var snapshot = v.val;
                req.GetBody = () => {
                    r = snapshot;
                    return (_addr_io.NopCloser(_addr_r)!, error.As(null!)!);
                }
;
                break;
            case ptr<strings.Reader> v:
                req.ContentLength = int64(v.Len());
                snapshot = v.val;
                req.GetBody = () => {
                    r = snapshot;
                    return (_addr_io.NopCloser(_addr_r)!, error.As(null!)!);
                }
;
                break;
            default:
            {
                var v = body.type();
                break;
            } 
            // For client requests, Request.ContentLength of 0
            // means either actually 0, or unknown. The only way
            // to explicitly say that the ContentLength is zero is
            // to set the Body to nil. But turns out too much code
            // depends on NewRequest returning a non-nil Body,
            // so we use a well-known ReadCloser variable instead
            // and have the http package also treat that sentinel
            // variable to mean explicitly zero.
        } 
        // For client requests, Request.ContentLength of 0
        // means either actually 0, or unknown. The only way
        // to explicitly say that the ContentLength is zero is
        // to set the Body to nil. But turns out too much code
        // depends on NewRequest returning a non-nil Body,
        // so we use a well-known ReadCloser variable instead
        // and have the http package also treat that sentinel
        // variable to mean explicitly zero.
        if (req.GetBody != null && req.ContentLength == 0) {
            req.Body = NoBody;
            req.GetBody = () => (_addr_NoBody!, error.As(null!)!);
        }
    }
    return (_addr_req!, error.As(null!)!);
}

// BasicAuth returns the username and password provided in the request's
// Authorization header, if the request uses HTTP Basic Authentication.
// See RFC 2617, Section 2.
private static (@string, @string, bool) BasicAuth(this ptr<Request> _addr_r) {
    @string username = default;
    @string password = default;
    bool ok = default;
    ref Request r = ref _addr_r.val;

    var auth = r.Header.Get("Authorization");
    if (auth == "") {
        return ;
    }
    return parseBasicAuth(auth);
}

// parseBasicAuth parses an HTTP Basic Authentication string.
// "Basic QWxhZGRpbjpvcGVuIHNlc2FtZQ==" returns ("Aladdin", "open sesame", true).
private static (@string, @string, bool) parseBasicAuth(@string auth) {
    @string username = default;
    @string password = default;
    bool ok = default;

    const @string prefix = "Basic "; 
    // Case insensitive prefix match. See Issue 22736.
 
    // Case insensitive prefix match. See Issue 22736.
    if (len(auth) < len(prefix) || !ascii.EqualFold(auth[..(int)len(prefix)], prefix)) {
        return ;
    }
    var (c, err) = base64.StdEncoding.DecodeString(auth[(int)len(prefix)..]);
    if (err != null) {
        return ;
    }
    var cs = string(c);
    var s = strings.IndexByte(cs, ':');
    if (s < 0) {
        return ;
    }
    return (cs[..(int)s], cs[(int)s + 1..], true);
}

// SetBasicAuth sets the request's Authorization header to use HTTP
// Basic Authentication with the provided username and password.
//
// With HTTP Basic Authentication the provided username and password
// are not encrypted.
//
// Some protocols may impose additional requirements on pre-escaping the
// username and password. For instance, when used with OAuth2, both arguments
// must be URL encoded first with url.QueryEscape.
private static void SetBasicAuth(this ptr<Request> _addr_r, @string username, @string password) {
    ref Request r = ref _addr_r.val;

    r.Header.Set("Authorization", "Basic " + basicAuth(username, password));
}

// parseRequestLine parses "GET /foo HTTP/1.1" into its three parts.
private static (@string, @string, @string, bool) parseRequestLine(@string line) {
    @string method = default;
    @string requestURI = default;
    @string proto = default;
    bool ok = default;

    var s1 = strings.Index(line, " ");
    var s2 = strings.Index(line[(int)s1 + 1..], " ");
    if (s1 < 0 || s2 < 0) {
        return ;
    }
    s2 += s1 + 1;
    return (line[..(int)s1], line[(int)s1 + 1..(int)s2], line[(int)s2 + 1..], true);
}

private static sync.Pool textprotoReaderPool = default;

private static ptr<textproto.Reader> newTextprotoReader(ptr<bufio.Reader> _addr_br) {
    ref bufio.Reader br = ref _addr_br.val;

    {
        var v = textprotoReaderPool.Get();

        if (v != null) {
            ptr<textproto.Reader> tr = v._<ptr<textproto.Reader>>();
            tr.R = br;
            return _addr_tr!;
        }
    }
    return _addr_textproto.NewReader(br)!;
}

private static void putTextprotoReader(ptr<textproto.Reader> _addr_r) {
    ref textproto.Reader r = ref _addr_r.val;

    r.R = null;
    textprotoReaderPool.Put(r);
}

// ReadRequest reads and parses an incoming request from b.
//
// ReadRequest is a low-level function and should only be used for
// specialized applications; most code should use the Server to read
// requests and handle them via the Handler interface. ReadRequest
// only supports HTTP/1.x requests. For HTTP/2, use golang.org/x/net/http2.
public static (ptr<Request>, error) ReadRequest(ptr<bufio.Reader> _addr_b) {
    ptr<Request> _p0 = default!;
    error _p0 = default!;
    ref bufio.Reader b = ref _addr_b.val;

    var (req, err) = readRequest(_addr_b);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    delete(req.Header, "Host");
    return (_addr_req!, error.As(err)!);
}

private static (ptr<Request>, error) readRequest(ptr<bufio.Reader> _addr_b) => func((defer, _, _) => {
    ptr<Request> req = default!;
    error err = default!;
    ref bufio.Reader b = ref _addr_b.val;

    var tp = newTextprotoReader(_addr_b);
    req = @new<Request>(); 

    // First line: GET /index.html HTTP/1.0
    @string s = default;
    s, err = tp.ReadLine();

    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    defer(() => {
        putTextprotoReader(_addr_tp);
        if (err == io.EOF) {
            err = io.ErrUnexpectedEOF;
        }
    }());

    bool ok = default;
    req.Method, req.RequestURI, req.Proto, ok = parseRequestLine(s);
    if (!ok) {
        return (_addr_null!, error.As(badStringError("malformed HTTP request", s))!);
    }
    if (!validMethod(req.Method)) {
        return (_addr_null!, error.As(badStringError("invalid method", req.Method))!);
    }
    var rawurl = req.RequestURI;
    req.ProtoMajor, req.ProtoMinor, ok = ParseHTTPVersion(req.Proto);

    if (!ok) {
        return (_addr_null!, error.As(badStringError("malformed HTTP version", req.Proto))!);
    }
    var justAuthority = req.Method == "CONNECT" && !strings.HasPrefix(rawurl, "/");
    if (justAuthority) {
        rawurl = "http://" + rawurl;
    }
    req.URL, err = url.ParseRequestURI(rawurl);

    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    if (justAuthority) { 
        // Strip the bogus "http://" back off.
        req.URL.Scheme = "";
    }
    var (mimeHeader, err) = tp.ReadMIMEHeader();
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    req.Header = Header(mimeHeader);
    if (len(req.Header["Host"]) > 1) {
        return (_addr_null!, error.As(fmt.Errorf("too many Host headers"))!);
    }
    req.Host = req.URL.Host;
    if (req.Host == "") {
        req.Host = req.Header.get("Host");
    }
    fixPragmaCacheControl(req.Header);

    req.Close = shouldClose(req.ProtoMajor, req.ProtoMinor, req.Header, false);

    err = readTransfer(req, b);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    if (req.isH2Upgrade()) { 
        // Because it's neither chunked, nor declared:
        req.ContentLength = -1; 

        // We want to give handlers a chance to hijack the
        // connection, but we need to prevent the Server from
        // dealing with the connection further if it's not
        // hijacked. Set Close to ensure that:
        req.Close = true;
    }
    return (_addr_req!, error.As(null!)!);
});

// MaxBytesReader is similar to io.LimitReader but is intended for
// limiting the size of incoming request bodies. In contrast to
// io.LimitReader, MaxBytesReader's result is a ReadCloser, returns a
// non-EOF error for a Read beyond the limit, and closes the
// underlying reader when its Close method is called.
//
// MaxBytesReader prevents clients from accidentally or maliciously
// sending a large request and wasting server resources.
public static io.ReadCloser MaxBytesReader(ResponseWriter w, io.ReadCloser r, long n) {
    if (n < 0) { // Treat negative limits as equivalent to 0.
        n = 0;
    }
    return addr(new maxBytesReader(w:w,r:r,n:n));
}

private partial struct maxBytesReader {
    public ResponseWriter w;
    public io.ReadCloser r; // underlying reader
    public long n; // max bytes remaining
    public error err; // sticky error
}

private static (nint, error) Read(this ptr<maxBytesReader> _addr_l, slice<byte> p) {
    nint n = default;
    error err = default!;
    ref maxBytesReader l = ref _addr_l.val;

    if (l.err != null) {
        return (0, error.As(l.err)!);
    }
    if (len(p) == 0) {
        return (0, error.As(null!)!);
    }
    if (int64(len(p)) > l.n + 1) {
        p = p[..(int)l.n + 1];
    }
    n, err = l.r.Read(p);

    if (int64(n) <= l.n) {
        l.n -= int64(n);
        l.err = err;
        return (n, error.As(err)!);
    }
    n = int(l.n);
    l.n = 0; 

    // The server code and client code both use
    // maxBytesReader. This "requestTooLarge" check is
    // only used by the server code. To prevent binaries
    // which only using the HTTP Client code (such as
    // cmd/go) from also linking in the HTTP server, don't
    // use a static type assertion to the server
    // "*response" type. Check this interface instead:
    private partial interface requestTooLarger {
        void requestTooLarge();
    }
    {
        requestTooLarger (res, ok) = requestTooLarger.As(l.w._<requestTooLarger>())!;

        if (ok) {
            res.requestTooLarge();
        }
    }
    l.err = errors.New("http: request body too large");
    return (n, error.As(l.err)!);
}

private static error Close(this ptr<maxBytesReader> _addr_l) {
    ref maxBytesReader l = ref _addr_l.val;

    return error.As(l.r.Close())!;
}

private static void copyValues(url.Values dst, url.Values src) {
    foreach (var (k, vs) in src) {
        dst[k] = append(dst[k], vs);
    }
}

private static (url.Values, error) parsePostForm(ptr<Request> _addr_r) {
    url.Values vs = default;
    error err = default!;
    ref Request r = ref _addr_r.val;

    if (r.Body == null) {
        err = errors.New("missing form body");
        return ;
    }
    var ct = r.Header.Get("Content-Type"); 
    // RFC 7231, section 3.1.1.5 - empty type
    //   MAY be treated as application/octet-stream
    if (ct == "") {
        ct = "application/octet-stream";
    }
    ct, _, err = mime.ParseMediaType(ct);

    if (ct == "application/x-www-form-urlencoded") 
        io.Reader reader = r.Body;
        var maxFormSize = int64(1 << 63 - 1);
        {
            ptr<maxBytesReader> (_, ok) = r.Body._<ptr<maxBytesReader>>();

            if (!ok) {
                maxFormSize = int64(10 << 20); // 10 MB is a lot of text.
                reader = io.LimitReader(r.Body, maxFormSize + 1);
            }

        }
        var (b, e) = io.ReadAll(reader);
        if (e != null) {
            if (err == null) {
                err = e;
            }
            break;
        }
        if (int64(len(b)) > maxFormSize) {
            err = errors.New("http: POST too large");
            return ;
        }
        vs, e = url.ParseQuery(string(b));
        if (err == null) {
            err = e;
        }
    else if (ct == "multipart/form-data")         return ;
}

// ParseForm populates r.Form and r.PostForm.
//
// For all requests, ParseForm parses the raw query from the URL and updates
// r.Form.
//
// For POST, PUT, and PATCH requests, it also reads the request body, parses it
// as a form and puts the results into both r.PostForm and r.Form. Request body
// parameters take precedence over URL query string values in r.Form.
//
// If the request Body's size has not already been limited by MaxBytesReader,
// the size is capped at 10MB.
//
// For other HTTP methods, or when the Content-Type is not
// application/x-www-form-urlencoded, the request Body is not read, and
// r.PostForm is initialized to a non-nil, empty value.
//
// ParseMultipartForm calls ParseForm automatically.
// ParseForm is idempotent.
private static error ParseForm(this ptr<Request> _addr_r) {
    ref Request r = ref _addr_r.val;

    error err = default!;
    if (r.PostForm == null) {
        if (r.Method == "POST" || r.Method == "PUT" || r.Method == "PATCH") {
            r.PostForm, err = parsePostForm(_addr_r);
        }
        if (r.PostForm == null) {
            r.PostForm = make(url.Values);
        }
    }
    if (r.Form == null) {
        if (len(r.PostForm) > 0) {
            r.Form = make(url.Values);
            copyValues(r.Form, r.PostForm);
        }
        url.Values newValues = default;
        if (r.URL != null) {
            error e = default!;
            newValues, e = url.ParseQuery(r.URL.RawQuery);
            if (err == null) {
                err = error.As(e)!;
            }
        }
        if (newValues == null) {
            newValues = make(url.Values);
        }
        if (r.Form == null) {
            r.Form = newValues;
        }
        else
 {
            copyValues(r.Form, newValues);
        }
    }
    return error.As(err)!;
}

// ParseMultipartForm parses a request body as multipart/form-data.
// The whole request body is parsed and up to a total of maxMemory bytes of
// its file parts are stored in memory, with the remainder stored on
// disk in temporary files.
// ParseMultipartForm calls ParseForm if necessary.
// If ParseForm returns an error, ParseMultipartForm returns it but also
// continues parsing the request body.
// After one call to ParseMultipartForm, subsequent calls have no effect.
private static error ParseMultipartForm(this ptr<Request> _addr_r, long maxMemory) {
    ref Request r = ref _addr_r.val;

    if (r.MultipartForm == multipartByReader) {
        return error.As(errors.New("http: multipart handled by MultipartReader"))!;
    }
    error parseFormErr = default!;
    if (r.Form == null) { 
        // Let errors in ParseForm fall through, and just
        // return it at the end.
        parseFormErr = error.As(r.ParseForm())!;
    }
    if (r.MultipartForm != null) {
        return error.As(null!)!;
    }
    var (mr, err) = r.multipartReader(false);
    if (err != null) {
        return error.As(err)!;
    }
    var (f, err) = mr.ReadForm(maxMemory);
    if (err != null) {
        return error.As(err)!;
    }
    if (r.PostForm == null) {
        r.PostForm = make(url.Values);
    }
    foreach (var (k, v) in f.Value) {
        r.Form[k] = append(r.Form[k], v); 
        // r.PostForm should also be populated. See Issue 9305.
        r.PostForm[k] = append(r.PostForm[k], v);
    }    r.MultipartForm = f;

    return error.As(parseFormErr)!;
}

// FormValue returns the first value for the named component of the query.
// POST and PUT body parameters take precedence over URL query string values.
// FormValue calls ParseMultipartForm and ParseForm if necessary and ignores
// any errors returned by these functions.
// If key is not present, FormValue returns the empty string.
// To access multiple values of the same key, call ParseForm and
// then inspect Request.Form directly.
private static @string FormValue(this ptr<Request> _addr_r, @string key) {
    ref Request r = ref _addr_r.val;

    if (r.Form == null) {
        r.ParseMultipartForm(defaultMaxMemory);
    }
    {
        var vs = r.Form[key];

        if (len(vs) > 0) {
            return vs[0];
        }
    }
    return "";
}

// PostFormValue returns the first value for the named component of the POST,
// PATCH, or PUT request body. URL query parameters are ignored.
// PostFormValue calls ParseMultipartForm and ParseForm if necessary and ignores
// any errors returned by these functions.
// If key is not present, PostFormValue returns the empty string.
private static @string PostFormValue(this ptr<Request> _addr_r, @string key) {
    ref Request r = ref _addr_r.val;

    if (r.PostForm == null) {
        r.ParseMultipartForm(defaultMaxMemory);
    }
    {
        var vs = r.PostForm[key];

        if (len(vs) > 0) {
            return vs[0];
        }
    }
    return "";
}

// FormFile returns the first file for the provided form key.
// FormFile calls ParseMultipartForm and ParseForm if necessary.
private static (multipart.File, ptr<multipart.FileHeader>, error) FormFile(this ptr<Request> _addr_r, @string key) {
    multipart.File _p0 = default;
    ptr<multipart.FileHeader> _p0 = default!;
    error _p0 = default!;
    ref Request r = ref _addr_r.val;

    if (r.MultipartForm == multipartByReader) {
        return (null, _addr_null!, error.As(errors.New("http: multipart handled by MultipartReader"))!);
    }
    if (r.MultipartForm == null) {
        var err = r.ParseMultipartForm(defaultMaxMemory);
        if (err != null) {
            return (null, _addr_null!, error.As(err)!);
        }
    }
    if (r.MultipartForm != null && r.MultipartForm.File != null) {
        {
            var fhs = r.MultipartForm.File[key];

            if (len(fhs) > 0) {
                var (f, err) = fhs[0].Open();
                return (f, _addr_fhs[0]!, error.As(err)!);
            }

        }
    }
    return (null, _addr_null!, error.As(ErrMissingFile)!);
}

private static bool expectsContinue(this ptr<Request> _addr_r) {
    ref Request r = ref _addr_r.val;

    return hasToken(r.Header.get("Expect"), "100-continue");
}

private static bool wantsHttp10KeepAlive(this ptr<Request> _addr_r) {
    ref Request r = ref _addr_r.val;

    if (r.ProtoMajor != 1 || r.ProtoMinor != 0) {
        return false;
    }
    return hasToken(r.Header.get("Connection"), "keep-alive");
}

private static bool wantsClose(this ptr<Request> _addr_r) {
    ref Request r = ref _addr_r.val;

    if (r.Close) {
        return true;
    }
    return hasToken(r.Header.get("Connection"), "close");
}

private static error closeBody(this ptr<Request> _addr_r) {
    ref Request r = ref _addr_r.val;

    if (r.Body == null) {
        return error.As(null!)!;
    }
    return error.As(r.Body.Close())!;
}

private static bool isReplayable(this ptr<Request> _addr_r) {
    ref Request r = ref _addr_r.val;

    if (r.Body == null || r.Body == NoBody || r.GetBody != null) {
        switch (valueOrDefault(r.Method, "GET")) {
            case "GET": 

            case "HEAD": 

            case "OPTIONS": 

            case "TRACE": 
                return true;
                break;
        } 
        // The Idempotency-Key, while non-standard, is widely used to
        // mean a POST or other request is idempotent. See
        // https://golang.org/issue/19943#issuecomment-421092421
        if (r.Header.has("Idempotency-Key") || r.Header.has("X-Idempotency-Key")) {
            return true;
        }
    }
    return false;
}

// outgoingLength reports the Content-Length of this outgoing (Client) request.
// It maps 0 into -1 (unknown) when the Body is non-nil.
private static long outgoingLength(this ptr<Request> _addr_r) {
    ref Request r = ref _addr_r.val;

    if (r.Body == null || r.Body == NoBody) {
        return 0;
    }
    if (r.ContentLength != 0) {
        return r.ContentLength;
    }
    return -1;
}

// requestMethodUsuallyLacksBody reports whether the given request
// method is one that typically does not involve a request body.
// This is used by the Transport (via
// transferWriter.shouldSendChunkedRequestBody) to determine whether
// we try to test-read a byte from a non-nil Request.Body when
// Request.outgoingLength() returns -1. See the comments in
// shouldSendChunkedRequestBody.
private static bool requestMethodUsuallyLacksBody(@string method) {
    switch (method) {
        case "GET": 

        case "HEAD": 

        case "DELETE": 

        case "OPTIONS": 

        case "PROPFIND": 

        case "SEARCH": 
            return true;
            break;
    }
    return false;
}

// requiresHTTP1 reports whether this request requires being sent on
// an HTTP/1 connection.
private static bool requiresHTTP1(this ptr<Request> _addr_r) {
    ref Request r = ref _addr_r.val;

    return hasToken(r.Header.Get("Connection"), "upgrade") && ascii.EqualFold(r.Header.Get("Upgrade"), "websocket");
}

} // end http_package
