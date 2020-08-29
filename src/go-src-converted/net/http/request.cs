// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// HTTP Request reading and parsing.

// package http -- go2cs converted at 2020 August 29 08:33:29 UTC
// import "net/http" ==> using http = go.net.http_package
// Original source: C:\Go\src\net\http\request.go
using bufio = go.bufio_package;
using bytes = go.bytes_package;
using context = go.context_package;
using tls = go.crypto.tls_package;
using base64 = go.encoding.base64_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using io = go.io_package;
using ioutil = go.io.ioutil_package;
using mime = go.mime_package;
using multipart = go.mime.multipart_package;
using net = go.net_package;
using httptrace = go.net.http.httptrace_package;
using textproto = go.net.textproto_package;
using url = go.net.url_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using sync = go.sync_package;

using idna = go.golang_org.x.net.idna_package;
using static go.builtin;
using System;

namespace go {
namespace net
{
    public static partial class http_package
    {
        private static readonly long defaultMaxMemory = 32L << (int)(20L); // 32 MB

        // ErrMissingFile is returned by FormFile when the provided file field name
        // is either not present in the request or not a file field.
        public static var ErrMissingFile = errors.New("http: no such file");

        // ProtocolError represents an HTTP protocol error.
        //
        // Deprecated: Not all errors in the http package related to protocol errors
        // are of type ProtocolError.
        public partial struct ProtocolError
        {
            public @string ErrorString;
        }

        private static @string Error(this ref ProtocolError pe)
        {
            return pe.ErrorString;
        }

 
        // ErrNotSupported is returned by the Push method of Pusher
        // implementations to indicate that HTTP/2 Push support is not
        // available.
        public static ProtocolError ErrNotSupported = ref new ProtocolError("feature not supported");        public static ProtocolError ErrUnexpectedTrailer = ref new ProtocolError("trailer header without chunked transfer encoding");        public static ProtocolError ErrMissingBoundary = ref new ProtocolError("no multipart boundary param in Content-Type");        public static ProtocolError ErrNotMultipart = ref new ProtocolError("request Content-Type isn't multipart/form-data");        public static ProtocolError ErrHeaderTooLong = ref new ProtocolError("header too long");        public static ProtocolError ErrShortBody = ref new ProtocolError("entity body too short");        public static ProtocolError ErrMissingContentLength = ref new ProtocolError("missing ContentLength in HEAD response");

        private partial struct badStringError
        {
            public @string what;
            public @string str;
        }

        private static @string Error(this ref badStringError e)
        {
            return fmt.Sprintf("%s %q", e.what, e.str);
        }

        // Headers that Request.Write handles itself and should be skipped.
        private static map reqWriteExcludeHeader = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, bool>{"Host":true,"User-Agent":true,"Content-Length":true,"Transfer-Encoding":true,"Trailer":true,};

        // A Request represents an HTTP request received by a server
        // or to be sent by a client.
        //
        // The field semantics differ slightly between client and server
        // usage. In addition to the notes on the fields below, see the
        // documentation for Request.Write and RoundTripper.
        public partial struct Request
        {
            public @string Method; // URL specifies either the URI being requested (for server
// requests) or the URL to access (for client requests).
//
// For server requests the URL is parsed from the URI
// supplied on the Request-Line as stored in RequestURI.  For
// most requests, fields other than Path and RawQuery will be
// empty. (See RFC 2616, Section 5.1.2)
//
// For client requests, the URL's Host specifies the server to
// connect to, while the Request's Host field optionally
// specifies the Host header value to send in the HTTP
// request.
            public ptr<url.URL> URL; // The protocol version for incoming server requests.
//
// For client requests these fields are ignored. The HTTP
// client code always uses either HTTP/1.1 or HTTP/2.
// See the docs on Transport for details.
            public @string Proto; // "HTTP/1.0"
            public long ProtoMajor; // 1
            public long ProtoMinor; // 0

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
// For client requests a nil body means the request has no
// body, such as a GET request. The HTTP Client's Transport
// is responsible for calling the Close method.
//
// For server requests the Request Body is always non-nil
// but will return EOF immediately when no body is present.
// The Server will close the request body. The ServeHTTP
// Handler does not need to.
            public io.ReadCloser Body; // GetBody defines an optional func to return a new copy of
// Body. It is used for client requests when a redirect requires
// reading the body more than once. Use of GetBody still
// requires setting Body.
//
// For server requests it is unused.
            public Func<(io.ReadCloser, error)> GetBody; // ContentLength records the length of the associated content.
// The value -1 indicates that the length is unknown.
// Values >= 0 indicate that the given number of bytes may
// be read from Body.
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
            public bool Close; // For server requests Host specifies the host on which the
// URL is sought. Per RFC 2616, this is either the value of
// the "Host" header or the host name given in the URL itself.
// It may be of the form "host:port". For international domain
// names, Host may be in Punycode or Unicode form. Use
// golang.org/x/net/idna to convert it to either format if
// needed.
//
// For client requests Host optionally overrides the Host
// header to send. If empty, the Request.Write method uses
// the value of URL.Host. Host may contain an international
// domain name.
            public @string Host; // Form contains the parsed form data, including both the URL
// field's query parameters and the POST or PUT form data.
// This field is only available after ParseForm is called.
// The HTTP client ignores Form and uses Body instead.
            public url.Values Form; // PostForm contains the parsed form data from POST, PATCH,
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
// For server requests the Trailer map initially contains only the
// trailer keys, with nil values. (The client declares which trailers it
// will later send.)  While the handler is reading from Body, it must
// not reference Trailer. After reading from Body returns EOF, Trailer
// can be read again and will contain non-nil values, if they were sent
// by the client.
//
// For client requests Trailer must be initialized to a map containing
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
            public @string RemoteAddr; // RequestURI is the unmodified Request-URI of the
// Request-Line (RFC 2616, Section 5.1) as sent by the client
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
// Deprecated: Use the Context and WithContext methods
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
        // For outgoing client requests, the context controls cancelation.
        //
        // For incoming server requests, the context is canceled when the
        // client's connection closes, the request is canceled (with HTTP/2),
        // or when the ServeHTTP method returns.
        private static context.Context Context(this ref Request r)
        {
            if (r.ctx != null)
            {
                return r.ctx;
            }
            return context.Background();
        }

        // WithContext returns a shallow copy of r with its context changed
        // to ctx. The provided ctx must be non-nil.
        private static ref Request WithContext(this ref Request _r, context.Context ctx) => func(_r, (ref Request r, Defer _, Panic panic, Recover __) =>
        {
            if (ctx == null)
            {
                panic("nil context");
            }
            ptr<Request> r2 = @new<Request>();
            r2.Value = r.Value;
            r2.ctx = ctx; 

            // Deep copy the URL because it isn't
            // a map and the URL is mutable by users
            // of WithContext.
            if (r.URL != null)
            {
                ptr<url.URL> r2URL = @new<url.URL>();
                r2URL.Value = r.URL.Value;
                r2.URL = r2URL;
            }
            return r2;
        });

        // ProtoAtLeast reports whether the HTTP protocol used
        // in the request is at least major.minor.
        private static bool ProtoAtLeast(this ref Request r, long major, long minor)
        {
            return r.ProtoMajor > major || r.ProtoMajor == major && r.ProtoMinor >= minor;
        }

        // UserAgent returns the client's User-Agent, if sent in the request.
        private static @string UserAgent(this ref Request r)
        {
            return r.Header.Get("User-Agent");
        }

        // Cookies parses and returns the HTTP cookies sent with the request.
        private static slice<ref Cookie> Cookies(this ref Request r)
        {
            return readCookies(r.Header, "");
        }

        // ErrNoCookie is returned by Request's Cookie method when a cookie is not found.
        public static var ErrNoCookie = errors.New("http: named cookie not present");

        // Cookie returns the named cookie provided in the request or
        // ErrNoCookie if not found.
        // If multiple cookies match the given name, only one cookie will
        // be returned.
        private static (ref Cookie, error) Cookie(this ref Request r, @string name)
        {
            foreach (var (_, c) in readCookies(r.Header, name))
            {
                return (c, null);
            }
            return (null, ErrNoCookie);
        }

        // AddCookie adds a cookie to the request. Per RFC 6265 section 5.4,
        // AddCookie does not attach more than one Cookie header field. That
        // means all cookies, if any, are written into the same line,
        // separated by semicolon.
        private static void AddCookie(this ref Request r, ref Cookie c)
        {
            var s = fmt.Sprintf("%s=%s", sanitizeCookieName(c.Name), sanitizeCookieValue(c.Value));
            {
                var c = r.Header.Get("Cookie");

                if (c != "")
                {
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
        private static @string Referer(this ref Request r)
        {
            return r.Header.Get("Referer");
        }

        // multipartByReader is a sentinel value.
        // Its presence in Request.MultipartForm indicates that parsing of the request
        // body has been handed off to a MultipartReader instead of ParseMultipartFrom.
        private static multipart.Form multipartByReader = ref new multipart.Form(Value:make(map[string][]string),File:make(map[string][]*multipart.FileHeader),);

        // MultipartReader returns a MIME multipart reader if this is a
        // multipart/form-data POST request, else returns nil and an error.
        // Use this function instead of ParseMultipartForm to
        // process the request body as a stream.
        private static (ref multipart.Reader, error) MultipartReader(this ref Request r)
        {
            if (r.MultipartForm == multipartByReader)
            {
                return (null, errors.New("http: MultipartReader called twice"));
            }
            if (r.MultipartForm != null)
            {
                return (null, errors.New("http: multipart handled by ParseMultipartForm"));
            }
            r.MultipartForm = multipartByReader;
            return r.multipartReader();
        }

        private static (ref multipart.Reader, error) multipartReader(this ref Request r)
        {
            var v = r.Header.Get("Content-Type");
            if (v == "")
            {
                return (null, ErrNotMultipart);
            }
            var (d, params, err) = mime.ParseMediaType(v);
            if (err != null || d != "multipart/form-data")
            {
                return (null, ErrNotMultipart);
            }
            var (boundary, ok) = params["boundary"];
            if (!ok)
            {
                return (null, ErrMissingBoundary);
            }
            return (multipart.NewReader(r.Body, boundary), null);
        }

        // isH2Upgrade reports whether r represents the http2 "client preface"
        // magic string.
        private static bool isH2Upgrade(this ref Request r)
        {
            return r.Method == "PRI" && len(r.Header) == 0L && r.URL.Path == "*" && r.Proto == "HTTP/2.0";
        }

        // Return value if nonempty, def otherwise.
        private static @string valueOrDefault(@string value, @string def)
        {
            if (value != "")
            {
                return value;
            }
            return def;
        }

        // NOTE: This is not intended to reflect the actual Go version being used.
        // It was changed at the time of Go 1.1 release because the former User-Agent
        // had ended up on a blacklist for some intrusion detection systems.
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
        private static error Write(this ref Request r, io.Writer w)
        {
            return error.As(r.write(w, false, null, null));
        }

        // WriteProxy is like Write but writes the request in the form
        // expected by an HTTP proxy. In particular, WriteProxy writes the
        // initial Request-URI line of the request with an absolute URI, per
        // section 5.1.2 of RFC 2616, including the scheme and host.
        // In either case, WriteProxy also writes a Host header, using
        // either r.Host or r.URL.Host.
        private static error WriteProxy(this ref Request r, io.Writer w)
        {
            return error.As(r.write(w, true, null, null));
        }

        // errMissingHost is returned by Write when there is no Host or URL present in
        // the Request.
        private static var errMissingHost = errors.New("http: Request.Write on Request with no Host or URL set");

        // extraHeaders may be nil
        // waitForContinue may be nil
        private static error write(this ref Request _r, io.Writer w, bool usingProxy, Header extraHeaders, Func<bool> waitForContinue) => func(_r, (ref Request r, Defer defer, Panic _, Recover __) =>
        {
            var trace = httptrace.ContextClientTrace(r.Context());
            if (trace != null && trace.WroteRequest != null)
            {
                defer(() =>
                {
                    trace.WroteRequest(new httptrace.WroteRequestInfo(Err:err,));
                }());
            } 

            // Find the target host. Prefer the Host: header, but if that
            // is not given, use the host from the request URL.
            //
            // Clean the host, in case it arrives with unexpected stuff in it.
            var host = cleanHost(r.Host);
            if (host == "")
            {
                if (r.URL == null)
                {
                    return error.As(errMissingHost);
                }
                host = cleanHost(r.URL.Host);
            } 

            // According to RFC 6874, an HTTP client, proxy, or other
            // intermediary must remove any IPv6 zone identifier attached
            // to an outgoing URI.
            host = removeZone(host);

            var ruri = r.URL.RequestURI();
            if (usingProxy && r.URL.Scheme != "" && r.URL.Opaque == "")
            {
                ruri = r.URL.Scheme + "://" + host + ruri;
            }
            else if (r.Method == "CONNECT" && r.URL.Path == "")
            { 
                // CONNECT requests normally give just the host and port, not a full URL.
                ruri = host;
            } 
            // TODO(bradfitz): escape at least newlines in ruri?

            // Wrap the writer in a bufio Writer if it's not already buffered.
            // Don't always call NewWriter, as that forces a bytes.Buffer
            // and other small bufio Writers to have a minimum 4k buffer
            // size.
            ref bufio.Writer bw = default;
            {
                io.ByteWriter (_, ok) = w._<io.ByteWriter>();

                if (!ok)
                {
                    bw = bufio.NewWriter(w);
                    w = bw;
                }

            }

            _, err = fmt.Fprintf(w, "%s %s HTTP/1.1\r\n", valueOrDefault(r.Method, "GET"), ruri);
            if (err != null)
            {
                return error.As(err);
            } 

            // Header lines
            _, err = fmt.Fprintf(w, "Host: %s\r\n", host);
            if (err != null)
            {
                return error.As(err);
            } 

            // Use the defaultUserAgent unless the Header contains one, which
            // may be blank to not send the header.
            var userAgent = defaultUserAgent;
            {
                (_, ok) = r.Header["User-Agent"];

                if (ok)
                {
                    userAgent = r.Header.Get("User-Agent");
                }

            }
            if (userAgent != "")
            {
                _, err = fmt.Fprintf(w, "User-Agent: %s\r\n", userAgent);
                if (err != null)
                {
                    return error.As(err);
                }
            } 

            // Process Body,ContentLength,Close,Trailer
            var (tw, err) = newTransferWriter(r);
            if (err != null)
            {
                return error.As(err);
            }
            err = tw.WriteHeader(w);
            if (err != null)
            {
                return error.As(err);
            }
            err = r.Header.WriteSubset(w, reqWriteExcludeHeader);
            if (err != null)
            {
                return error.As(err);
            }
            if (extraHeaders != null)
            {
                err = extraHeaders.Write(w);
                if (err != null)
                {
                    return error.As(err);
                }
            }
            _, err = io.WriteString(w, "\r\n");
            if (err != null)
            {
                return error.As(err);
            }
            if (trace != null && trace.WroteHeaders != null)
            {
                trace.WroteHeaders();
            } 

            // Flush and wait for 100-continue if expected.
            if (waitForContinue != null)
            {
                {
                    ref bufio.Writer bw__prev2 = bw;

                    ref bufio.Writer (bw, ok) = w._<ref bufio.Writer>();

                    if (ok)
                    {
                        err = bw.Flush();
                        if (err != null)
                        {
                            return error.As(err);
                        }
                    }

                    bw = bw__prev2;

                }
                if (trace != null && trace.Wait100Continue != null)
                {
                    trace.Wait100Continue();
                }
                if (!waitForContinue())
                {
                    r.closeBody();
                    return error.As(null);
                }
            }
            {
                ref bufio.Writer bw__prev1 = bw;

                (bw, ok) = w._<ref bufio.Writer>();

                if (ok && tw.FlushHeaders)
                {
                    {
                        var err = bw.Flush();

                        if (err != null)
                        {
                            return error.As(err);
                        }

                    }
                } 

                // Write body and trailer

                bw = bw__prev1;

            } 

            // Write body and trailer
            err = tw.WriteBody(w);
            if (err != null)
            {
                if (tw.bodyReadError == err)
                {
                    err = new requestBodyReadError(err);
                }
                return error.As(err);
            }
            if (bw != null)
            {
                return error.As(bw.Flush());
            }
            return error.As(null);
        });

        // requestBodyReadError wraps an error from (*Request).write to indicate
        // that the error came from a Read call on the Request.Body.
        // This error type should not escape the net/http package to users.
        private partial struct requestBodyReadError : error
        {
            public error error;
        }

        private static (@string, error) idnaASCII(@string v)
        { 
            // TODO: Consider removing this check after verifying performance is okay.
            // Right now punycode verification, length checks, context checks, and the
            // permissible character tests are all omitted. It also prevents the ToASCII
            // call from salvaging an invalid IDN, when possible. As a result it may be
            // possible to have two IDNs that appear identical to the user where the
            // ASCII-only version causes an error downstream whereas the non-ASCII
            // version does not.
            // Note that for correct ASCII IDNs ToASCII will only do considerably more
            // work, but it will not cause an allocation.
            if (isASCII(v))
            {
                return (v, null);
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
        private static @string cleanHost(@string @in)
        {
            {
                var i = strings.IndexAny(in, " /");

                if (i != -1L)
                {
                    in = in[..i];
                }

            }
            var (host, port, err) = net.SplitHostPort(in);
            if (err != null)
            { // input was just a host
                var (a, err) = idnaASCII(in);
                if (err != null)
                {
                    return in; // garbage in, garbage out
                }
                return a;
            }
            (a, err) = idnaASCII(host);
            if (err != null)
            {
                return in; // garbage in, garbage out
            }
            return net.JoinHostPort(a, port);
        }

        // removeZone removes IPv6 zone identifier from host.
        // E.g., "[fe80::1%en0]:8080" to "[fe80::1]:8080"
        private static @string removeZone(@string host)
        {
            if (!strings.HasPrefix(host, "["))
            {
                return host;
            }
            var i = strings.LastIndex(host, "]");
            if (i < 0L)
            {
                return host;
            }
            var j = strings.LastIndex(host[..i], "%");
            if (j < 0L)
            {
                return host;
            }
            return host[..j] + host[i..];
        }

        // ParseHTTPVersion parses a HTTP version string.
        // "HTTP/1.0" returns (1, 0, true).
        public static (long, long, bool) ParseHTTPVersion(@string vers)
        {
            const long Big = 1000000L; // arbitrary upper bound
 // arbitrary upper bound
            switch (vers)
            {
                case "HTTP/1.1": 
                    return (1L, 1L, true);
                    break;
                case "HTTP/1.0": 
                    return (1L, 0L, true);
                    break;
            }
            if (!strings.HasPrefix(vers, "HTTP/"))
            {
                return (0L, 0L, false);
            }
            var dot = strings.Index(vers, ".");
            if (dot < 0L)
            {
                return (0L, 0L, false);
            }
            var (major, err) = strconv.Atoi(vers[5L..dot]);
            if (err != null || major < 0L || major > Big)
            {
                return (0L, 0L, false);
            }
            minor, err = strconv.Atoi(vers[dot + 1L..]);
            if (err != null || minor < 0L || minor > Big)
            {
                return (0L, 0L, false);
            }
            return (major, minor, true);
        }

        private static bool validMethod(@string method)
        {
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
            return len(method) > 0L && strings.IndexFunc(method, isNotToken) == -1L;
        }

        // NewRequest returns a new Request given a method, URL, and optional body.
        //
        // If the provided body is also an io.Closer, the returned
        // Request.Body is set to body and will be closed by the Client
        // methods Do, Post, and PostForm, and Transport.RoundTrip.
        //
        // NewRequest returns a Request suitable for use with Client.Do or
        // Transport.RoundTrip. To create a request for use with testing a
        // Server Handler, either use the NewRequest function in the
        // net/http/httptest package, use ReadRequest, or manually update the
        // Request fields. See the Request type's documentation for the
        // difference between inbound and outbound request fields.
        //
        // If body is of type *bytes.Buffer, *bytes.Reader, or
        // *strings.Reader, the returned request's ContentLength is set to its
        // exact value (instead of -1), GetBody is populated (so 307 and 308
        // redirects can replay the body), and Body is set to NoBody if the
        // ContentLength is 0.
        public static (ref Request, error) NewRequest(@string method, @string url, io.Reader body)
        {
            if (method == "")
            { 
                // We document that "" means "GET" for Request.Method, and people have
                // relied on that from NewRequest, so keep that working.
                // We still enforce validMethod for non-empty methods.
                method = "GET";
            }
            if (!validMethod(method))
            {
                return (null, fmt.Errorf("net/http: invalid method %q", method));
            }
            var (u, err) = parseURL(url); // Just url.Parse (url is shadowed for godoc).
            if (err != null)
            {
                return (null, err);
            }
            io.ReadCloser (rc, ok) = body._<io.ReadCloser>();
            if (!ok && body != null)
            {
                rc = ioutil.NopCloser(body);
            } 
            // The host's colon:port should be normalized. See Issue 14836.
            u.Host = removeEmptyPort(u.Host);
            Request req = ref new Request(Method:method,URL:u,Proto:"HTTP/1.1",ProtoMajor:1,ProtoMinor:1,Header:make(Header),Body:rc,Host:u.Host,);
            if (body != null)
            {
                switch (body.type())
                {
                    case ref bytes.Buffer v:
                        req.ContentLength = int64(v.Len());
                        var buf = v.Bytes();
                        req.GetBody = () =>
                        {
                            var r = bytes.NewReader(buf);
                            return (ioutil.NopCloser(r), null);
                        }
;
                        break;
                    case ref bytes.Reader v:
                        req.ContentLength = int64(v.Len());
                        var snapshot = v.Value;
                        req.GetBody = () =>
                        {
                            r = snapshot;
                            return (ioutil.NopCloser(ref r), null);
                        }
;
                        break;
                    case ref strings.Reader v:
                        req.ContentLength = int64(v.Len());
                        snapshot = v.Value;
                        req.GetBody = () =>
                        {
                            r = snapshot;
                            return (ioutil.NopCloser(ref r), null);
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
                if (req.GetBody != null && req.ContentLength == 0L)
                {
                    req.Body = NoBody;
                    req.GetBody = () => (NoBody, null);
                }
            }
            return (req, null);
        }

        // BasicAuth returns the username and password provided in the request's
        // Authorization header, if the request uses HTTP Basic Authentication.
        // See RFC 2617, Section 2.
        private static (@string, @string, bool) BasicAuth(this ref Request r)
        {
            var auth = r.Header.Get("Authorization");
            if (auth == "")
            {
                return;
            }
            return parseBasicAuth(auth);
        }

        // parseBasicAuth parses an HTTP Basic Authentication string.
        // "Basic QWxhZGRpbjpvcGVuIHNlc2FtZQ==" returns ("Aladdin", "open sesame", true).
        private static (@string, @string, bool) parseBasicAuth(@string auth)
        {
            const @string prefix = "Basic ";

            if (!strings.HasPrefix(auth, prefix))
            {
                return;
            }
            var (c, err) = base64.StdEncoding.DecodeString(auth[len(prefix)..]);
            if (err != null)
            {
                return;
            }
            var cs = string(c);
            var s = strings.IndexByte(cs, ':');
            if (s < 0L)
            {
                return;
            }
            return (cs[..s], cs[s + 1L..], true);
        }

        // SetBasicAuth sets the request's Authorization header to use HTTP
        // Basic Authentication with the provided username and password.
        //
        // With HTTP Basic Authentication the provided username and password
        // are not encrypted.
        private static void SetBasicAuth(this ref Request r, @string username, @string password)
        {
            r.Header.Set("Authorization", "Basic " + basicAuth(username, password));
        }

        // parseRequestLine parses "GET /foo HTTP/1.1" into its three parts.
        private static (@string, @string, @string, bool) parseRequestLine(@string line)
        {
            var s1 = strings.Index(line, " ");
            var s2 = strings.Index(line[s1 + 1L..], " ");
            if (s1 < 0L || s2 < 0L)
            {
                return;
            }
            s2 += s1 + 1L;
            return (line[..s1], line[s1 + 1L..s2], line[s2 + 1L..], true);
        }

        private static sync.Pool textprotoReaderPool = default;

        private static ref textproto.Reader newTextprotoReader(ref bufio.Reader br)
        {
            {
                var v = textprotoReaderPool.Get();

                if (v != null)
                {
                    ref textproto.Reader tr = v._<ref textproto.Reader>();
                    tr.R = br;
                    return tr;
                }

            }
            return textproto.NewReader(br);
        }

        private static void putTextprotoReader(ref textproto.Reader r)
        {
            r.R = null;
            textprotoReaderPool.Put(r);
        }

        // ReadRequest reads and parses an incoming request from b.
        public static (ref Request, error) ReadRequest(ref bufio.Reader b)
        {
            return readRequest(b, deleteHostHeader);
        }

        // Constants for readRequest's deleteHostHeader parameter.
        private static readonly var deleteHostHeader = true;
        private static readonly var keepHostHeader = false;

        private static (ref Request, error) readRequest(ref bufio.Reader _b, bool deleteHostHeader) => func(_b, (ref bufio.Reader b, Defer defer, Panic _, Recover __) =>
        {
            var tp = newTextprotoReader(b);
            req = @new<Request>(); 

            // First line: GET /index.html HTTP/1.0
            @string s = default;
            s, err = tp.ReadLine();

            if (err != null)
            {
                return (null, err);
            }
            defer(() =>
            {
                putTextprotoReader(tp);
                if (err == io.EOF)
                {
                    err = io.ErrUnexpectedEOF;
                }
            }());

            bool ok = default;
            req.Method, req.RequestURI, req.Proto, ok = parseRequestLine(s);
            if (!ok)
            {
                return (null, ref new badStringError("malformed HTTP request",s));
            }
            if (!validMethod(req.Method))
            {
                return (null, ref new badStringError("invalid method",req.Method));
            }
            var rawurl = req.RequestURI;
            req.ProtoMajor, req.ProtoMinor, ok = ParseHTTPVersion(req.Proto);

            if (!ok)
            {
                return (null, ref new badStringError("malformed HTTP version",req.Proto));
            } 

            // CONNECT requests are used two different ways, and neither uses a full URL:
            // The standard use is to tunnel HTTPS through an HTTP proxy.
            // It looks like "CONNECT www.google.com:443 HTTP/1.1", and the parameter is
            // just the authority section of a URL. This information should go in req.URL.Host.
            //
            // The net/rpc package also uses CONNECT, but there the parameter is a path
            // that starts with a slash. It can be parsed with the regular URL parser,
            // and the path will end up in req.URL.Path, where it needs to be in order for
            // RPC to work.
            var justAuthority = req.Method == "CONNECT" && !strings.HasPrefix(rawurl, "/");
            if (justAuthority)
            {
                rawurl = "http://" + rawurl;
            }
            req.URL, err = url.ParseRequestURI(rawurl);

            if (err != null)
            {
                return (null, err);
            }
            if (justAuthority)
            { 
                // Strip the bogus "http://" back off.
                req.URL.Scheme = "";
            } 

            // Subsequent lines: Key: value.
            var (mimeHeader, err) = tp.ReadMIMEHeader();
            if (err != null)
            {
                return (null, err);
            }
            req.Header = Header(mimeHeader); 

            // RFC 2616: Must treat
            //    GET /index.html HTTP/1.1
            //    Host: www.google.com
            // and
            //    GET http://www.google.com/index.html HTTP/1.1
            //    Host: doesntmatter
            // the same. In the second case, any Host line is ignored.
            req.Host = req.URL.Host;
            if (req.Host == "")
            {
                req.Host = req.Header.get("Host");
            }
            if (deleteHostHeader)
            {
                delete(req.Header, "Host");
            }
            fixPragmaCacheControl(req.Header);

            req.Close = shouldClose(req.ProtoMajor, req.ProtoMinor, req.Header, false);

            err = readTransfer(req, b);
            if (err != null)
            {
                return (null, err);
            }
            if (req.isH2Upgrade())
            { 
                // Because it's neither chunked, nor declared:
                req.ContentLength = -1L; 

                // We want to give handlers a chance to hijack the
                // connection, but we need to prevent the Server from
                // dealing with the connection further if it's not
                // hijacked. Set Close to ensure that:
                req.Close = true;
            }
            return (req, null);
        });

        // MaxBytesReader is similar to io.LimitReader but is intended for
        // limiting the size of incoming request bodies. In contrast to
        // io.LimitReader, MaxBytesReader's result is a ReadCloser, returns a
        // non-EOF error for a Read beyond the limit, and closes the
        // underlying reader when its Close method is called.
        //
        // MaxBytesReader prevents clients from accidentally or maliciously
        // sending a large request and wasting server resources.
        public static io.ReadCloser MaxBytesReader(ResponseWriter w, io.ReadCloser r, long n)
        {
            return ref new maxBytesReader(w:w,r:r,n:n);
        }

        private partial struct maxBytesReader
        {
            public ResponseWriter w;
            public io.ReadCloser r; // underlying reader
            public long n; // max bytes remaining
            public error err; // sticky error
        }

        private static (long, error) Read(this ref maxBytesReader l, slice<byte> p)
        {
            if (l.err != null)
            {
                return (0L, l.err);
            }
            if (len(p) == 0L)
            {
                return (0L, null);
            } 
            // If they asked for a 32KB byte read but only 5 bytes are
            // remaining, no need to read 32KB. 6 bytes will answer the
            // question of the whether we hit the limit or go past it.
            if (int64(len(p)) > l.n + 1L)
            {
                p = p[..l.n + 1L];
            }
            n, err = l.r.Read(p);

            if (int64(n) <= l.n)
            {
                l.n -= int64(n);
                l.err = err;
                return (n, err);
            }
            n = int(l.n);
            l.n = 0L; 

            // The server code and client code both use
            // maxBytesReader. This "requestTooLarge" check is
            // only used by the server code. To prevent binaries
            // which only using the HTTP Client code (such as
            // cmd/go) from also linking in the HTTP server, don't
            // use a static type assertion to the server
            // "*response" type. Check this interface instead:
            private partial interface requestTooLarger
            {
                void requestTooLarge();
            }
            {
                requestTooLarger (res, ok) = l.w._<requestTooLarger>();

                if (ok)
                {
                    res.requestTooLarge();
                }

            }
            l.err = errors.New("http: request body too large");
            return (n, l.err);
        }

        private static error Close(this ref maxBytesReader l)
        {
            return error.As(l.r.Close());
        }

        private static void copyValues(url.Values dst, url.Values src)
        {
            foreach (var (k, vs) in src)
            {
                foreach (var (_, value) in vs)
                {
                    dst.Add(k, value);
                }
            }
        }

        private static (url.Values, error) parsePostForm(ref Request r)
        {
            if (r.Body == null)
            {
                err = errors.New("missing form body");
                return;
            }
            var ct = r.Header.Get("Content-Type"); 
            // RFC 2616, section 7.2.1 - empty type
            //   SHOULD be treated as application/octet-stream
            if (ct == "")
            {
                ct = "application/octet-stream";
            }
            ct, _, err = mime.ParseMediaType(ct);

            if (ct == "application/x-www-form-urlencoded") 
                io.Reader reader = r.Body;
                var maxFormSize = int64(1L << (int)(63L) - 1L);
                {
                    ref maxBytesReader (_, ok) = r.Body._<ref maxBytesReader>();

                    if (!ok)
                    {
                        maxFormSize = int64(10L << (int)(20L)); // 10 MB is a lot of text.
                        reader = io.LimitReader(r.Body, maxFormSize + 1L);
                    }

                }
                var (b, e) = ioutil.ReadAll(reader);
                if (e != null)
                {
                    if (err == null)
                    {
                        err = e;
                    }
                    break;
                }
                if (int64(len(b)) > maxFormSize)
                {
                    err = errors.New("http: POST too large");
                    return;
                }
                vs, e = url.ParseQuery(string(b));
                if (err == null)
                {
                    err = e;
                }
            else if (ct == "multipart/form-data")                         return;
        }

        // ParseForm populates r.Form and r.PostForm.
        //
        // For all requests, ParseForm parses the raw query from the URL and updates
        // r.Form.
        //
        // For POST, PUT, and PATCH requests, it also parses the request body as a form
        // and puts the results into both r.PostForm and r.Form. Request body parameters
        // take precedence over URL query string values in r.Form.
        //
        // For other HTTP methods, or when the Content-Type is not
        // application/x-www-form-urlencoded, the request Body is not read, and
        // r.PostForm is initialized to a non-nil, empty value.
        //
        // If the request Body's size has not already been limited by MaxBytesReader,
        // the size is capped at 10MB.
        //
        // ParseMultipartForm calls ParseForm automatically.
        // ParseForm is idempotent.
        private static error ParseForm(this ref Request r)
        {
            error err = default;
            if (r.PostForm == null)
            {
                if (r.Method == "POST" || r.Method == "PUT" || r.Method == "PATCH")
                {
                    r.PostForm, err = parsePostForm(r);
                }
                if (r.PostForm == null)
                {
                    r.PostForm = make(url.Values);
                }
            }
            if (r.Form == null)
            {
                if (len(r.PostForm) > 0L)
                {
                    r.Form = make(url.Values);
                    copyValues(r.Form, r.PostForm);
                }
                url.Values newValues = default;
                if (r.URL != null)
                {
                    error e = default;
                    newValues, e = url.ParseQuery(r.URL.RawQuery);
                    if (err == null)
                    {
                        err = error.As(e);
                    }
                }
                if (newValues == null)
                {
                    newValues = make(url.Values);
                }
                if (r.Form == null)
                {
                    r.Form = newValues;
                }
                else
                {
                    copyValues(r.Form, newValues);
                }
            }
            return error.As(err);
        }

        // ParseMultipartForm parses a request body as multipart/form-data.
        // The whole request body is parsed and up to a total of maxMemory bytes of
        // its file parts are stored in memory, with the remainder stored on
        // disk in temporary files.
        // ParseMultipartForm calls ParseForm if necessary.
        // After one call to ParseMultipartForm, subsequent calls have no effect.
        private static error ParseMultipartForm(this ref Request r, long maxMemory)
        {
            if (r.MultipartForm == multipartByReader)
            {
                return error.As(errors.New("http: multipart handled by MultipartReader"));
            }
            if (r.Form == null)
            {
                var err = r.ParseForm();
                if (err != null)
                {
                    return error.As(err);
                }
            }
            if (r.MultipartForm != null)
            {
                return error.As(null);
            }
            var (mr, err) = r.multipartReader();
            if (err != null)
            {
                return error.As(err);
            }
            var (f, err) = mr.ReadForm(maxMemory);
            if (err != null)
            {
                return error.As(err);
            }
            if (r.PostForm == null)
            {
                r.PostForm = make(url.Values);
            }
            foreach (var (k, v) in f.Value)
            {
                r.Form[k] = append(r.Form[k], v); 
                // r.PostForm should also be populated. See Issue 9305.
                r.PostForm[k] = append(r.PostForm[k], v);
            }
            r.MultipartForm = f;

            return error.As(null);
        }

        // FormValue returns the first value for the named component of the query.
        // POST and PUT body parameters take precedence over URL query string values.
        // FormValue calls ParseMultipartForm and ParseForm if necessary and ignores
        // any errors returned by these functions.
        // If key is not present, FormValue returns the empty string.
        // To access multiple values of the same key, call ParseForm and
        // then inspect Request.Form directly.
        private static @string FormValue(this ref Request r, @string key)
        {
            if (r.Form == null)
            {
                r.ParseMultipartForm(defaultMaxMemory);
            }
            {
                var vs = r.Form[key];

                if (len(vs) > 0L)
                {
                    return vs[0L];
                }

            }
            return "";
        }

        // PostFormValue returns the first value for the named component of the POST
        // or PUT request body. URL query parameters are ignored.
        // PostFormValue calls ParseMultipartForm and ParseForm if necessary and ignores
        // any errors returned by these functions.
        // If key is not present, PostFormValue returns the empty string.
        private static @string PostFormValue(this ref Request r, @string key)
        {
            if (r.PostForm == null)
            {
                r.ParseMultipartForm(defaultMaxMemory);
            }
            {
                var vs = r.PostForm[key];

                if (len(vs) > 0L)
                {
                    return vs[0L];
                }

            }
            return "";
        }

        // FormFile returns the first file for the provided form key.
        // FormFile calls ParseMultipartForm and ParseForm if necessary.
        private static (multipart.File, ref multipart.FileHeader, error) FormFile(this ref Request r, @string key)
        {
            if (r.MultipartForm == multipartByReader)
            {
                return (null, null, errors.New("http: multipart handled by MultipartReader"));
            }
            if (r.MultipartForm == null)
            {
                var err = r.ParseMultipartForm(defaultMaxMemory);
                if (err != null)
                {
                    return (null, null, err);
                }
            }
            if (r.MultipartForm != null && r.MultipartForm.File != null)
            {
                {
                    var fhs = r.MultipartForm.File[key];

                    if (len(fhs) > 0L)
                    {
                        var (f, err) = fhs[0L].Open();
                        return (f, fhs[0L], err);
                    }

                }
            }
            return (null, null, ErrMissingFile);
        }

        private static bool expectsContinue(this ref Request r)
        {
            return hasToken(r.Header.get("Expect"), "100-continue");
        }

        private static bool wantsHttp10KeepAlive(this ref Request r)
        {
            if (r.ProtoMajor != 1L || r.ProtoMinor != 0L)
            {
                return false;
            }
            return hasToken(r.Header.get("Connection"), "keep-alive");
        }

        private static bool wantsClose(this ref Request r)
        {
            return hasToken(r.Header.get("Connection"), "close");
        }

        private static void closeBody(this ref Request r)
        {
            if (r.Body != null)
            {
                r.Body.Close();
            }
        }

        private static bool isReplayable(this ref Request r)
        {
            if (r.Body == null || r.Body == NoBody || r.GetBody != null)
            {
                switch (valueOrDefault(r.Method, "GET"))
                {
                    case "GET": 

                    case "HEAD": 

                    case "OPTIONS": 

                    case "TRACE": 
                        return true;
                        break;
                }
            }
            return false;
        }

        // outgoingLength reports the Content-Length of this outgoing (Client) request.
        // It maps 0 into -1 (unknown) when the Body is non-nil.
        private static long outgoingLength(this ref Request r)
        {
            if (r.Body == null || r.Body == NoBody)
            {
                return 0L;
            }
            if (r.ContentLength != 0L)
            {
                return r.ContentLength;
            }
            return -1L;
        }

        // requestMethodUsuallyLacksBody reports whether the given request
        // method is one that typically does not involve a request body.
        // This is used by the Transport (via
        // transferWriter.shouldSendChunkedRequestBody) to determine whether
        // we try to test-read a byte from a non-nil Request.Body when
        // Request.outgoingLength() returns -1. See the comments in
        // shouldSendChunkedRequestBody.
        private static bool requestMethodUsuallyLacksBody(@string method)
        {
            switch (method)
            {
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
    }
}}
