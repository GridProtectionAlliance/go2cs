// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// HTTP client implementation. See RFC 7230 through 7235.
//
// This is the low-level Transport implementation of RoundTripper.
// The high-level interface is in client.go.

// package http -- go2cs converted at 2022 March 13 05:37:49 UTC
// import "net/http" ==> using http = go.net.http_package
// Original source: C:\Program Files\Go\src\net\http\transport.go
namespace go.net;

using bufio = bufio_package;
using gzip = compress.gzip_package;
using list = container.list_package;
using context = context_package;
using tls = crypto.tls_package;
using errors = errors_package;
using fmt = fmt_package;
using io = io_package;
using log = log_package;
using net = net_package;
using httptrace = net.http.httptrace_package;
using ascii = net.http.@internal.ascii_package;
using textproto = net.textproto_package;
using url = net.url_package;
using os = os_package;
using reflect = reflect_package;
using strings = strings_package;
using sync = sync_package;
using atomic = sync.atomic_package;
using time = time_package;

using httpguts = golang.org.x.net.http.httpguts_package;
using httpproxy = golang.org.x.net.http.httpproxy_package;


// DefaultTransport is the default implementation of Transport and is
// used by DefaultClient. It establishes network connections as needed
// and caches them for reuse by subsequent calls. It uses HTTP proxies
// as directed by the $HTTP_PROXY and $NO_PROXY (or $http_proxy and
// $no_proxy) environment variables.

using System;
using System.Threading;
public static partial class http_package {

public static RoundTripper DefaultTransport = addr(new Transport(Proxy:ProxyFromEnvironment,DialContext:(&net.Dialer{Timeout:30*time.Second,KeepAlive:30*time.Second,}).DialContext,ForceAttemptHTTP2:true,MaxIdleConns:100,IdleConnTimeout:90*time.Second,TLSHandshakeTimeout:10*time.Second,ExpectContinueTimeout:1*time.Second,));

// DefaultMaxIdleConnsPerHost is the default value of Transport's
// MaxIdleConnsPerHost.
public static readonly nint DefaultMaxIdleConnsPerHost = 2;

// Transport is an implementation of RoundTripper that supports HTTP,
// HTTPS, and HTTP proxies (for either HTTP or HTTPS with CONNECT).
//
// By default, Transport caches connections for future re-use.
// This may leave many open connections when accessing many hosts.
// This behavior can be managed using Transport's CloseIdleConnections method
// and the MaxIdleConnsPerHost and DisableKeepAlives fields.
//
// Transports should be reused instead of created as needed.
// Transports are safe for concurrent use by multiple goroutines.
//
// A Transport is a low-level primitive for making HTTP and HTTPS requests.
// For high-level functionality, such as cookies and redirects, see Client.
//
// Transport uses HTTP/1.1 for HTTP URLs and either HTTP/1.1 or HTTP/2
// for HTTPS URLs, depending on whether the server supports HTTP/2,
// and how the Transport is configured. The DefaultTransport supports HTTP/2.
// To explicitly enable HTTP/2 on a transport, use golang.org/x/net/http2
// and call ConfigureTransport. See the package docs for more about HTTP/2.
//
// Responses with status codes in the 1xx range are either handled
// automatically (100 expect-continue) or ignored. The one
// exception is HTTP status code 101 (Switching Protocols), which is
// considered a terminal status and returned by RoundTrip. To see the
// ignored 1xx responses, use the httptrace trace package's
// ClientTrace.Got1xxResponse.
//
// Transport only retries a request upon encountering a network error
// if the request is idempotent and either has no body or has its
// Request.GetBody defined. HTTP requests are considered idempotent if
// they have HTTP methods GET, HEAD, OPTIONS, or TRACE; or if their
// Header map contains an "Idempotency-Key" or "X-Idempotency-Key"
// entry. If the idempotency key value is a zero-length slice, the
// request is treated as idempotent but the header is not sent on the
// wire.


// Transport is an implementation of RoundTripper that supports HTTP,
// HTTPS, and HTTP proxies (for either HTTP or HTTPS with CONNECT).
//
// By default, Transport caches connections for future re-use.
// This may leave many open connections when accessing many hosts.
// This behavior can be managed using Transport's CloseIdleConnections method
// and the MaxIdleConnsPerHost and DisableKeepAlives fields.
//
// Transports should be reused instead of created as needed.
// Transports are safe for concurrent use by multiple goroutines.
//
// A Transport is a low-level primitive for making HTTP and HTTPS requests.
// For high-level functionality, such as cookies and redirects, see Client.
//
// Transport uses HTTP/1.1 for HTTP URLs and either HTTP/1.1 or HTTP/2
// for HTTPS URLs, depending on whether the server supports HTTP/2,
// and how the Transport is configured. The DefaultTransport supports HTTP/2.
// To explicitly enable HTTP/2 on a transport, use golang.org/x/net/http2
// and call ConfigureTransport. See the package docs for more about HTTP/2.
//
// Responses with status codes in the 1xx range are either handled
// automatically (100 expect-continue) or ignored. The one
// exception is HTTP status code 101 (Switching Protocols), which is
// considered a terminal status and returned by RoundTrip. To see the
// ignored 1xx responses, use the httptrace trace package's
// ClientTrace.Got1xxResponse.
//
// Transport only retries a request upon encountering a network error
// if the request is idempotent and either has no body or has its
// Request.GetBody defined. HTTP requests are considered idempotent if
// they have HTTP methods GET, HEAD, OPTIONS, or TRACE; or if their
// Header map contains an "Idempotency-Key" or "X-Idempotency-Key"
// entry. If the idempotency key value is a zero-length slice, the
// request is treated as idempotent but the header is not sent on the
// wire.
public partial struct Transport {
    public sync.Mutex idleMu;
    public bool closeIdle; // user has requested to close all idle conns
    public map<connectMethodKey, slice<ptr<persistConn>>> idleConn; // most recently used at end
    public map<connectMethodKey, wantConnQueue> idleConnWait; // waiting getConns
    public connLRU idleLRU;
    public sync.Mutex reqMu;
    public map<cancelKey, Action<error>> reqCanceler;
    public sync.Mutex altMu; // guards changing altProto only
    public atomic.Value altProto; // of nil or map[string]RoundTripper, key is URI scheme

    public sync.Mutex connsPerHostMu;
    public map<connectMethodKey, nint> connsPerHost;
    public map<connectMethodKey, wantConnQueue> connsPerHostWait; // waiting getConns

// Proxy specifies a function to return a proxy for a given
// Request. If the function returns a non-nil error, the
// request is aborted with the provided error.
//
// The proxy type is determined by the URL scheme. "http",
// "https", and "socks5" are supported. If the scheme is empty,
// "http" is assumed.
//
// If Proxy is nil or returns a nil *URL, no proxy is used.
    public Func<ptr<Request>, (ptr<url.URL>, error)> Proxy; // DialContext specifies the dial function for creating unencrypted TCP connections.
// If DialContext is nil (and the deprecated Dial below is also nil),
// then the transport dials using package net.
//
// DialContext runs concurrently with calls to RoundTrip.
// A RoundTrip call that initiates a dial may end up using
// a connection dialed previously when the earlier connection
// becomes idle before the later DialContext completes.
    public Func<context.Context, @string, @string, (net.Conn, error)> DialContext; // Dial specifies the dial function for creating unencrypted TCP connections.
//
// Dial runs concurrently with calls to RoundTrip.
// A RoundTrip call that initiates a dial may end up using
// a connection dialed previously when the earlier connection
// becomes idle before the later Dial completes.
//
// Deprecated: Use DialContext instead, which allows the transport
// to cancel dials as soon as they are no longer needed.
// If both are set, DialContext takes priority.
    public Func<@string, @string, (net.Conn, error)> Dial; // DialTLSContext specifies an optional dial function for creating
// TLS connections for non-proxied HTTPS requests.
//
// If DialTLSContext is nil (and the deprecated DialTLS below is also nil),
// DialContext and TLSClientConfig are used.
//
// If DialTLSContext is set, the Dial and DialContext hooks are not used for HTTPS
// requests and the TLSClientConfig and TLSHandshakeTimeout
// are ignored. The returned net.Conn is assumed to already be
// past the TLS handshake.
    public Func<context.Context, @string, @string, (net.Conn, error)> DialTLSContext; // DialTLS specifies an optional dial function for creating
// TLS connections for non-proxied HTTPS requests.
//
// Deprecated: Use DialTLSContext instead, which allows the transport
// to cancel dials as soon as they are no longer needed.
// If both are set, DialTLSContext takes priority.
    public Func<@string, @string, (net.Conn, error)> DialTLS; // TLSClientConfig specifies the TLS configuration to use with
// tls.Client.
// If nil, the default configuration is used.
// If non-nil, HTTP/2 support may not be enabled by default.
    public ptr<tls.Config> TLSClientConfig; // TLSHandshakeTimeout specifies the maximum amount of time waiting to
// wait for a TLS handshake. Zero means no timeout.
    public time.Duration TLSHandshakeTimeout; // DisableKeepAlives, if true, disables HTTP keep-alives and
// will only use the connection to the server for a single
// HTTP request.
//
// This is unrelated to the similarly named TCP keep-alives.
    public bool DisableKeepAlives; // DisableCompression, if true, prevents the Transport from
// requesting compression with an "Accept-Encoding: gzip"
// request header when the Request contains no existing
// Accept-Encoding value. If the Transport requests gzip on
// its own and gets a gzipped response, it's transparently
// decoded in the Response.Body. However, if the user
// explicitly requested gzip it is not automatically
// uncompressed.
    public bool DisableCompression; // MaxIdleConns controls the maximum number of idle (keep-alive)
// connections across all hosts. Zero means no limit.
    public nint MaxIdleConns; // MaxIdleConnsPerHost, if non-zero, controls the maximum idle
// (keep-alive) connections to keep per-host. If zero,
// DefaultMaxIdleConnsPerHost is used.
    public nint MaxIdleConnsPerHost; // MaxConnsPerHost optionally limits the total number of
// connections per host, including connections in the dialing,
// active, and idle states. On limit violation, dials will block.
//
// Zero means no limit.
    public nint MaxConnsPerHost; // IdleConnTimeout is the maximum amount of time an idle
// (keep-alive) connection will remain idle before closing
// itself.
// Zero means no limit.
    public time.Duration IdleConnTimeout; // ResponseHeaderTimeout, if non-zero, specifies the amount of
// time to wait for a server's response headers after fully
// writing the request (including its body, if any). This
// time does not include the time to read the response body.
    public time.Duration ResponseHeaderTimeout; // ExpectContinueTimeout, if non-zero, specifies the amount of
// time to wait for a server's first response headers after fully
// writing the request headers if the request has an
// "Expect: 100-continue" header. Zero means no timeout and
// causes the body to be sent immediately, without
// waiting for the server to approve.
// This time does not include the time to send the request header.
    public time.Duration ExpectContinueTimeout; // TLSNextProto specifies how the Transport switches to an
// alternate protocol (such as HTTP/2) after a TLS ALPN
// protocol negotiation. If Transport dials an TLS connection
// with a non-empty protocol name and TLSNextProto contains a
// map entry for that key (such as "h2"), then the func is
// called with the request's authority (such as "example.com"
// or "example.com:1234") and the TLS connection. The function
// must return a RoundTripper that then handles the request.
// If TLSNextProto is not nil, HTTP/2 support is not enabled
// automatically.
    public map<@string, Func<@string, ptr<tls.Conn>, RoundTripper>> TLSNextProto; // ProxyConnectHeader optionally specifies headers to send to
// proxies during CONNECT requests.
// To set the header dynamically, see GetProxyConnectHeader.
    public Header ProxyConnectHeader; // GetProxyConnectHeader optionally specifies a func to return
// headers to send to proxyURL during a CONNECT request to the
// ip:port target.
// If it returns an error, the Transport's RoundTrip fails with
// that error. It can return (nil, nil) to not add headers.
// If GetProxyConnectHeader is non-nil, ProxyConnectHeader is
// ignored.
    public Func<context.Context, ptr<url.URL>, @string, (Header, error)> GetProxyConnectHeader; // MaxResponseHeaderBytes specifies a limit on how many
// response bytes are allowed in the server's response
// header.
//
// Zero means to use a default limit.
    public long MaxResponseHeaderBytes; // WriteBufferSize specifies the size of the write buffer used
// when writing to the transport.
// If zero, a default (currently 4KB) is used.
    public nint WriteBufferSize; // ReadBufferSize specifies the size of the read buffer used
// when reading from the transport.
// If zero, a default (currently 4KB) is used.
    public nint ReadBufferSize; // nextProtoOnce guards initialization of TLSNextProto and
// h2transport (via onceSetNextProtoDefaults)
    public sync.Once nextProtoOnce;
    public h2Transport h2transport; // non-nil if http2 wired up
    public bool tlsNextProtoWasNil; // whether TLSNextProto was nil when the Once fired

// ForceAttemptHTTP2 controls whether HTTP/2 is enabled when a non-zero
// Dial, DialTLS, or DialContext func or TLSClientConfig is provided.
// By default, use of any those fields conservatively disables HTTP/2.
// To use a custom dialer or TLS config and still attempt HTTP/2
// upgrades, set this to true.
    public bool ForceAttemptHTTP2;
}

// A cancelKey is the key of the reqCanceler map.
// We wrap the *Request in this type since we want to use the original request,
// not any transient one created by roundTrip.
private partial struct cancelKey {
    public ptr<Request> req;
}

private static nint writeBufferSize(this ptr<Transport> _addr_t) {
    ref Transport t = ref _addr_t.val;

    if (t.WriteBufferSize > 0) {
        return t.WriteBufferSize;
    }
    return 4 << 10;
}

private static nint readBufferSize(this ptr<Transport> _addr_t) {
    ref Transport t = ref _addr_t.val;

    if (t.ReadBufferSize > 0) {
        return t.ReadBufferSize;
    }
    return 4 << 10;
}

// Clone returns a deep copy of t's exported fields.
private static ptr<Transport> Clone(this ptr<Transport> _addr_t) {
    ref Transport t = ref _addr_t.val;

    t.nextProtoOnce.Do(t.onceSetNextProtoDefaults);
    ptr<Transport> t2 = addr(new Transport(Proxy:t.Proxy,DialContext:t.DialContext,Dial:t.Dial,DialTLS:t.DialTLS,DialTLSContext:t.DialTLSContext,TLSHandshakeTimeout:t.TLSHandshakeTimeout,DisableKeepAlives:t.DisableKeepAlives,DisableCompression:t.DisableCompression,MaxIdleConns:t.MaxIdleConns,MaxIdleConnsPerHost:t.MaxIdleConnsPerHost,MaxConnsPerHost:t.MaxConnsPerHost,IdleConnTimeout:t.IdleConnTimeout,ResponseHeaderTimeout:t.ResponseHeaderTimeout,ExpectContinueTimeout:t.ExpectContinueTimeout,ProxyConnectHeader:t.ProxyConnectHeader.Clone(),GetProxyConnectHeader:t.GetProxyConnectHeader,MaxResponseHeaderBytes:t.MaxResponseHeaderBytes,ForceAttemptHTTP2:t.ForceAttemptHTTP2,WriteBufferSize:t.WriteBufferSize,ReadBufferSize:t.ReadBufferSize,));
    if (t.TLSClientConfig != null) {
        t2.TLSClientConfig = t.TLSClientConfig.Clone();
    }
    if (!t.tlsNextProtoWasNil) {
        map npm = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, Func<@string, ptr<tls.Conn>, RoundTripper>>{};
        foreach (var (k, v) in t.TLSNextProto) {
            npm[k] = v;
        }        t2.TLSNextProto = npm;
    }
    return _addr_t2!;
}

// h2Transport is the interface we expect to be able to call from
// net/http against an *http2.Transport that's either bundled into
// h2_bundle.go or supplied by the user via x/net/http2.
//
// We name it with the "h2" prefix to stay out of the "http2" prefix
// namespace used by x/tools/cmd/bundle for h2_bundle.go.
private partial interface h2Transport {
    void CloseIdleConnections();
}

private static bool hasCustomTLSDialer(this ptr<Transport> _addr_t) {
    ref Transport t = ref _addr_t.val;

    return t.DialTLS != null || t.DialTLSContext != null;
}

// onceSetNextProtoDefaults initializes TLSNextProto.
// It must be called via t.nextProtoOnce.Do.
private static void onceSetNextProtoDefaults(this ptr<Transport> _addr_t) {
    ref Transport t = ref _addr_t.val;

    t.tlsNextProtoWasNil = (t.TLSNextProto == null);
    if (strings.Contains(os.Getenv("GODEBUG"), "http2client=0")) {
        return ;
    }
    map<@string, RoundTripper> (altProto, _) = t.altProto.Load()._<map<@string, RoundTripper>>();
    {
        var rv = reflect.ValueOf(altProto["https"]);

        if (rv.IsValid() && rv.Type().Kind() == reflect.Struct && rv.Type().NumField() == 1) {
            {
                var v = rv.Field(0);

                if (v.CanInterface()) {
                    {
                        h2Transport (h2i, ok) = h2Transport.As(v.Interface()._<h2Transport>())!;

                        if (ok) {
                            t.h2transport = h2i;
                            return ;
                        }

                    }
                }

            }
        }
    }

    if (t.TLSNextProto != null) { 
        // This is the documented way to disable http2 on a
        // Transport.
        return ;
    }
    if (!t.ForceAttemptHTTP2 && (t.TLSClientConfig != null || t.Dial != null || t.DialContext != null || t.hasCustomTLSDialer())) { 
        // Be conservative and don't automatically enable
        // http2 if they've specified a custom TLS config or
        // custom dialers. Let them opt-in themselves via
        // http2.ConfigureTransport so we don't surprise them
        // by modifying their tls.Config. Issue 14275.
        // However, if ForceAttemptHTTP2 is true, it overrides the above checks.
        return ;
    }
    if (omitBundledHTTP2) {
        return ;
    }
    var (t2, err) = http2configureTransports(t);
    if (err != null) {
        log.Printf("Error enabling Transport HTTP/2 support: %v", err);
        return ;
    }
    t.h2transport = t2; 

    // Auto-configure the http2.Transport's MaxHeaderListSize from
    // the http.Transport's MaxResponseHeaderBytes. They don't
    // exactly mean the same thing, but they're close.
    //
    // TODO: also add this to x/net/http2.Configure Transport, behind
    // a +build go1.7 build tag:
    {
        var limit1 = t.MaxResponseHeaderBytes;

        if (limit1 != 0 && t2.MaxHeaderListSize == 0) {
            const nint h2max = 1 << 32 - 1;

            if (limit1 >= h2max) {
                t2.MaxHeaderListSize = h2max;
            }
            else
 {
                t2.MaxHeaderListSize = uint32(limit1);
            }
        }
    }
}

// ProxyFromEnvironment returns the URL of the proxy to use for a
// given request, as indicated by the environment variables
// HTTP_PROXY, HTTPS_PROXY and NO_PROXY (or the lowercase versions
// thereof). HTTPS_PROXY takes precedence over HTTP_PROXY for https
// requests.
//
// The environment values may be either a complete URL or a
// "host[:port]", in which case the "http" scheme is assumed.
// The schemes "http", "https", and "socks5" are supported.
// An error is returned if the value is a different form.
//
// A nil URL and nil error are returned if no proxy is defined in the
// environment, or a proxy should not be used for the given request,
// as defined by NO_PROXY.
//
// As a special case, if req.URL.Host is "localhost" (with or without
// a port number), then a nil URL and nil error will be returned.
public static (ptr<url.URL>, error) ProxyFromEnvironment(ptr<Request> _addr_req) {
    ptr<url.URL> _p0 = default!;
    error _p0 = default!;
    ref Request req = ref _addr_req.val;

    return _addr_envProxyFunc()(req.URL)!;
}

// ProxyURL returns a proxy function (for use in a Transport)
// that always returns the same URL.
public static Func<ptr<Request>, (ptr<url.URL>, error)> ProxyURL(ptr<url.URL> _addr_fixedURL) {
    ref url.URL fixedURL = ref _addr_fixedURL.val;

    return _p0 => (fixedURL, null);
}

// transportRequest is a wrapper around a *Request that adds
// optional extra headers to write and stores any error to return
// from roundTrip.
private partial struct transportRequest {
    public ref ptr<Request> ptr<Request> => ref ptr<Request>_ptr; // original request, not to be mutated
    public Header extra; // extra headers to write, or nil
    public ptr<httptrace.ClientTrace> trace; // optional
    public cancelKey cancelKey;
    public sync.Mutex mu; // guards err
    public error err; // first setError value for mapRoundTripError to consider
}

private static Header extraHeaders(this ptr<transportRequest> _addr_tr) {
    ref transportRequest tr = ref _addr_tr.val;

    if (tr.extra == null) {
        tr.extra = make(Header);
    }
    return tr.extra;
}

private static void setError(this ptr<transportRequest> _addr_tr, error err) {
    ref transportRequest tr = ref _addr_tr.val;

    tr.mu.Lock();
    if (tr.err == null) {
        tr.err = err;
    }
    tr.mu.Unlock();
}

// useRegisteredProtocol reports whether an alternate protocol (as registered
// with Transport.RegisterProtocol) should be respected for this request.
private static bool useRegisteredProtocol(this ptr<Transport> _addr_t, ptr<Request> _addr_req) {
    ref Transport t = ref _addr_t.val;
    ref Request req = ref _addr_req.val;

    if (req.URL.Scheme == "https" && req.requiresHTTP1()) { 
        // If this request requires HTTP/1, don't use the
        // "https" alternate protocol, which is used by the
        // HTTP/2 code to take over requests if there's an
        // existing cached HTTP/2 connection.
        return false;
    }
    return true;
}

// alternateRoundTripper returns the alternate RoundTripper to use
// for this request if the Request's URL scheme requires one,
// or nil for the normal case of using the Transport.
private static RoundTripper alternateRoundTripper(this ptr<Transport> _addr_t, ptr<Request> _addr_req) {
    ref Transport t = ref _addr_t.val;
    ref Request req = ref _addr_req.val;

    if (!t.useRegisteredProtocol(req)) {
        return null;
    }
    map<@string, RoundTripper> (altProto, _) = t.altProto.Load()._<map<@string, RoundTripper>>();
    return altProto[req.URL.Scheme];
}

// roundTrip implements a RoundTripper over HTTP.
private static (ptr<Response>, error) roundTrip(this ptr<Transport> _addr_t, ptr<Request> _addr_req) {
    ptr<Response> _p0 = default!;
    error _p0 = default!;
    ref Transport t = ref _addr_t.val;
    ref Request req = ref _addr_req.val;

    t.nextProtoOnce.Do(t.onceSetNextProtoDefaults);
    var ctx = req.Context();
    var trace = httptrace.ContextClientTrace(ctx);

    if (req.URL == null) {
        req.closeBody();
        return (_addr_null!, error.As(errors.New("http: nil Request.URL"))!);
    }
    if (req.Header == null) {
        req.closeBody();
        return (_addr_null!, error.As(errors.New("http: nil Request.Header"))!);
    }
    var scheme = req.URL.Scheme;
    var isHTTP = scheme == "http" || scheme == "https";
    if (isHTTP) {
        foreach (var (k, vv) in req.Header) {
            if (!httpguts.ValidHeaderFieldName(k)) {
                req.closeBody();
                return (_addr_null!, error.As(fmt.Errorf("net/http: invalid header field name %q", k))!);
            }
            foreach (var (_, v) in vv) {
                if (!httpguts.ValidHeaderFieldValue(v)) {
                    req.closeBody();
                    return (_addr_null!, error.As(fmt.Errorf("net/http: invalid header field value %q for key %v", v, k))!);
                }
            }
        }
    }
    var origReq = req;
    cancelKey cancelKey = new cancelKey(origReq);
    req = setupRewindBody(_addr_req);

    {
        var altRT = t.alternateRoundTripper(req);

        if (altRT != null) {
            {
                var resp__prev2 = resp;

                var (resp, err) = altRT.RoundTrip(req);

                if (err != ErrSkipAltProtocol) {
                    return (_addr_resp!, error.As(err)!);
                }

                resp = resp__prev2;

            }
            error err = default!;
            req, err = rewindBody(_addr_req);
            if (err != null) {
                return (_addr_null!, error.As(err)!);
            }
        }
    }
    if (!isHTTP) {
        req.closeBody();
        return (_addr_null!, error.As(badStringError("unsupported protocol scheme", scheme))!);
    }
    if (req.Method != "" && !validMethod(req.Method)) {
        req.closeBody();
        return (_addr_null!, error.As(fmt.Errorf("net/http: invalid method %q", req.Method))!);
    }
    if (req.URL.Host == "") {
        req.closeBody();
        return (_addr_null!, error.As(errors.New("http: no Host in request URL"))!);
    }
    while (true) {
        req.closeBody();
        return (_addr_null!, error.As(ctx.Err())!);
        ptr<transportRequest> treq = addr(new transportRequest(Request:req,trace:trace,cancelKey:cancelKey));
        var (cm, err) = t.connectMethodForRequest(treq);
        if (err != null) {
            req.closeBody();
            return (_addr_null!, error.As(err)!);
        }
        var (pconn, err) = t.getConn(treq, cm);
        if (err != null) {
            t.setReqCanceler(cancelKey, null);
            req.closeBody();
            return (_addr_null!, error.As(err)!);
        }
        ptr<Response> resp;
        if (pconn.alt != null) { 
            // HTTP/2 path.
            t.setReqCanceler(cancelKey, null); // not cancelable with CancelRequest
            resp, err = pconn.alt.RoundTrip(req);
        }
        else
 {
            resp, err = pconn.roundTrip(treq);
        }
        if (err == null) {
            resp.Request = origReq;
            return (_addr_resp!, error.As(null!)!);
        }
        if (http2isNoCachedConnError(err)) {
            if (t.removeIdleConn(pconn)) {
                t.decConnsPerHost(pconn.cacheKey);
            }
        }
        else if (!pconn.shouldRetryRequest(req, err)) { 
            // Issue 16465: return underlying net.Conn.Read error from peek,
            // as we've historically done.
            {
                transportReadFromServerError (e, ok) = err._<transportReadFromServerError>();

                if (ok) {
                    err = error.As(e.err)!;
                }

            }
            return (_addr_null!, error.As(err)!);
        }
        testHookRoundTripRetried(); 

        // Rewind the body if we're able to.
        req, err = rewindBody(_addr_req);
        if (err != null) {
            return (_addr_null!, error.As(err)!);
        }
    }
}

private static var errCannotRewind = errors.New("net/http: cannot rewind body after connection loss");

private partial struct readTrackingBody : io.ReadCloser {
    public ref io.ReadCloser ReadCloser => ref ReadCloser_val;
    public bool didRead;
    public bool didClose;
}

private static (nint, error) Read(this ptr<readTrackingBody> _addr_r, slice<byte> data) {
    nint _p0 = default;
    error _p0 = default!;
    ref readTrackingBody r = ref _addr_r.val;

    r.didRead = true;
    return r.ReadCloser.Read(data);
}

private static error Close(this ptr<readTrackingBody> _addr_r) {
    ref readTrackingBody r = ref _addr_r.val;

    r.didClose = true;
    return error.As(r.ReadCloser.Close())!;
}

// setupRewindBody returns a new request with a custom body wrapper
// that can report whether the body needs rewinding.
// This lets rewindBody avoid an error result when the request
// does not have GetBody but the body hasn't been read at all yet.
private static ptr<Request> setupRewindBody(ptr<Request> _addr_req) {
    ref Request req = ref _addr_req.val;

    if (req.Body == null || req.Body == NoBody) {
        return _addr_req!;
    }
    ref Request newReq = ref heap(req, out ptr<Request> _addr_newReq);
    newReq.Body = addr(new readTrackingBody(ReadCloser:req.Body));
    return _addr__addr_newReq!;
}

// rewindBody returns a new request with the body rewound.
// It returns req unmodified if the body does not need rewinding.
// rewindBody takes care of closing req.Body when appropriate
// (in all cases except when rewindBody returns req unmodified).
private static (ptr<Request>, error) rewindBody(ptr<Request> _addr_req) {
    ptr<Request> rewound = default!;
    error err = default!;
    ref Request req = ref _addr_req.val;

    if (req.Body == null || req.Body == NoBody || (!req.Body._<ptr<readTrackingBody>>().didRead && !req.Body._<ptr<readTrackingBody>>().didClose)) {
        return (_addr_req!, error.As(null!)!); // nothing to rewind
    }
    if (!req.Body._<ptr<readTrackingBody>>().didClose) {
        req.closeBody();
    }
    if (req.GetBody == null) {
        return (_addr_null!, error.As(errCannotRewind)!);
    }
    var (body, err) = req.GetBody();
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    ref Request newReq = ref heap(req, out ptr<Request> _addr_newReq);
    newReq.Body = addr(new readTrackingBody(ReadCloser:body));
    return (_addr__addr_newReq!, error.As(null!)!);
}

// shouldRetryRequest reports whether we should retry sending a failed
// HTTP request on a new connection. The non-nil input error is the
// error from roundTrip.
private static bool shouldRetryRequest(this ptr<persistConn> _addr_pc, ptr<Request> _addr_req, error err) {
    ref persistConn pc = ref _addr_pc.val;
    ref Request req = ref _addr_req.val;

    if (http2isNoCachedConnError(err)) { 
        // Issue 16582: if the user started a bunch of
        // requests at once, they can all pick the same conn
        // and violate the server's max concurrent streams.
        // Instead, match the HTTP/1 behavior for now and dial
        // again to get a new TCP connection, rather than failing
        // this request.
        return true;
    }
    if (err == errMissingHost) { 
        // User error.
        return false;
    }
    if (!pc.isReused()) { 
        // This was a fresh connection. There's no reason the server
        // should've hung up on us.
        //
        // Also, if we retried now, we could loop forever
        // creating new connections and retrying if the server
        // is just hanging up on us because it doesn't like
        // our request (as opposed to sending an error).
        return false;
    }
    {
        nothingWrittenError (_, ok) = err._<nothingWrittenError>();

        if (ok) { 
            // We never wrote anything, so it's safe to retry, if there's no body or we
            // can "rewind" the body with GetBody.
            return req.outgoingLength() == 0 || req.GetBody != null;
        }
    }
    if (!req.isReplayable()) { 
        // Don't retry non-idempotent requests.
        return false;
    }
    {
        (_, ok) = err._<transportReadFromServerError>();

        if (ok) { 
            // We got some non-EOF net.Conn.Read failure reading
            // the 1st response byte from the server.
            return true;
        }
    }
    if (err == errServerClosedIdle) { 
        // The server replied with io.EOF while we were trying to
        // read the response. Probably an unfortunately keep-alive
        // timeout, just as the client was writing a request.
        return true;
    }
    return false; // conservatively
}

// ErrSkipAltProtocol is a sentinel error value defined by Transport.RegisterProtocol.
public static var ErrSkipAltProtocol = errors.New("net/http: skip alternate protocol");

// RegisterProtocol registers a new protocol with scheme.
// The Transport will pass requests using the given scheme to rt.
// It is rt's responsibility to simulate HTTP request semantics.
//
// RegisterProtocol can be used by other packages to provide
// implementations of protocol schemes like "ftp" or "file".
//
// If rt.RoundTrip returns ErrSkipAltProtocol, the Transport will
// handle the RoundTrip itself for that one request, as if the
// protocol were not registered.
private static void RegisterProtocol(this ptr<Transport> _addr_t, @string scheme, RoundTripper rt) => func((defer, panic, _) => {
    ref Transport t = ref _addr_t.val;

    t.altMu.Lock();
    defer(t.altMu.Unlock());
    map<@string, RoundTripper> (oldMap, _) = t.altProto.Load()._<map<@string, RoundTripper>>();
    {
        var (_, exists) = oldMap[scheme];

        if (exists) {
            panic("protocol " + scheme + " already registered");
        }
    }
    var newMap = make_map<@string, RoundTripper>();
    foreach (var (k, v) in oldMap) {
        newMap[k] = v;
    }    newMap[scheme] = rt;
    t.altProto.Store(newMap);
});

// CloseIdleConnections closes any connections which were previously
// connected from previous requests but are now sitting idle in
// a "keep-alive" state. It does not interrupt any connections currently
// in use.
private static void CloseIdleConnections(this ptr<Transport> _addr_t) {
    ref Transport t = ref _addr_t.val;

    t.nextProtoOnce.Do(t.onceSetNextProtoDefaults);
    t.idleMu.Lock();
    var m = t.idleConn;
    t.idleConn = null;
    t.closeIdle = true; // close newly idle connections
    t.idleLRU = new connLRU();
    t.idleMu.Unlock();
    foreach (var (_, conns) in m) {
        foreach (var (_, pconn) in conns) {
            pconn.close(errCloseIdleConns);
        }
    }    {
        var t2 = t.h2transport;

        if (t2 != null) {
            t2.CloseIdleConnections();
        }
    }
}

// CancelRequest cancels an in-flight request by closing its connection.
// CancelRequest should only be called after RoundTrip has returned.
//
// Deprecated: Use Request.WithContext to create a request with a
// cancelable context instead. CancelRequest cannot cancel HTTP/2
// requests.
private static void CancelRequest(this ptr<Transport> _addr_t, ptr<Request> _addr_req) {
    ref Transport t = ref _addr_t.val;
    ref Request req = ref _addr_req.val;

    t.cancelRequest(new cancelKey(req), errRequestCanceled);
}

// Cancel an in-flight request, recording the error value.
// Returns whether the request was canceled.
private static bool cancelRequest(this ptr<Transport> _addr_t, cancelKey key, error err) => func((defer, _, _) => {
    ref Transport t = ref _addr_t.val;
 
    // This function must not return until the cancel func has completed.
    // See: https://golang.org/issue/34658
    t.reqMu.Lock();
    defer(t.reqMu.Unlock());
    var cancel = t.reqCanceler[key];
    delete(t.reqCanceler, key);
    if (cancel != null) {
        cancel(err);
    }
    return cancel != null;
});

//
// Private implementation past this point.
//

 
// proxyConfigOnce guards proxyConfig
private static sync.Once envProxyOnce = default;private static Func<ptr<url.URL>, (ptr<url.URL>, error)> envProxyFuncValue = default;

// defaultProxyConfig returns a ProxyConfig value looked up
// from the environment. This mitigates expensive lookups
// on some platforms (e.g. Windows).
private static Func<ptr<url.URL>, (ptr<url.URL>, error)> envProxyFunc() {
    envProxyOnce.Do(() => {
        envProxyFuncValue = httpproxy.FromEnvironment().ProxyFunc();
    });
    return envProxyFuncValue;
}

// resetProxyConfig is used by tests.
private static void resetProxyConfig() {
    envProxyOnce = new sync.Once();
    envProxyFuncValue = null;
}

private static (connectMethod, error) connectMethodForRequest(this ptr<Transport> _addr_t, ptr<transportRequest> _addr_treq) {
    connectMethod cm = default;
    error err = default!;
    ref Transport t = ref _addr_t.val;
    ref transportRequest treq = ref _addr_treq.val;

    cm.targetScheme = treq.URL.Scheme;
    cm.targetAddr = canonicalAddr(_addr_treq.URL);
    if (t.Proxy != null) {
        cm.proxyURL, err = t.Proxy(treq.Request);
    }
    cm.onlyH1 = treq.requiresHTTP1();
    return (cm, error.As(err)!);
}

// proxyAuth returns the Proxy-Authorization header to set
// on requests, if applicable.
private static @string proxyAuth(this ptr<connectMethod> _addr_cm) {
    ref connectMethod cm = ref _addr_cm.val;

    if (cm.proxyURL == null) {
        return "";
    }
    {
        var u = cm.proxyURL.User;

        if (u != null) {
            var username = u.Username();
            var (password, _) = u.Password();
            return "Basic " + basicAuth(username, password);
        }
    }
    return "";
}

// error values for debugging and testing, not seen by users.
private static var errKeepAlivesDisabled = errors.New("http: putIdleConn: keep alives disabled");private static var errConnBroken = errors.New("http: putIdleConn: connection is in bad state");private static var errCloseIdle = errors.New("http: putIdleConn: CloseIdleConnections was called");private static var errTooManyIdle = errors.New("http: putIdleConn: too many idle connections");private static var errTooManyIdleHost = errors.New("http: putIdleConn: too many idle connections for host");private static var errCloseIdleConns = errors.New("http: CloseIdleConnections called");private static var errReadLoopExiting = errors.New("http: persistConn.readLoop exiting");private static var errIdleConnTimeout = errors.New("http: idle connection timeout");private static var errServerClosedIdle = errors.New("http: server closed idle connection");

// transportReadFromServerError is used by Transport.readLoop when the
// 1 byte peek read fails and we're actually anticipating a response.
// Usually this is just due to the inherent keep-alive shut down race,
// where the server closed the connection at the same time the client
// wrote. The underlying err field is usually io.EOF or some
// ECONNRESET sort of thing which varies by platform. But it might be
// the user's custom net.Conn.Read error too, so we carry it along for
// them to return from Transport.RoundTrip.
private partial struct transportReadFromServerError {
    public error err;
}

private static error Unwrap(this transportReadFromServerError e) {
    return error.As(e.err)!;
}

private static @string Error(this transportReadFromServerError e) {
    return fmt.Sprintf("net/http: Transport failed to read from server: %v", e.err);
}

private static void putOrCloseIdleConn(this ptr<Transport> _addr_t, ptr<persistConn> _addr_pconn) {
    ref Transport t = ref _addr_t.val;
    ref persistConn pconn = ref _addr_pconn.val;

    {
        var err = t.tryPutIdleConn(pconn);

        if (err != null) {
            pconn.close(err);
        }
    }
}

private static nint maxIdleConnsPerHost(this ptr<Transport> _addr_t) {
    ref Transport t = ref _addr_t.val;

    {
        var v = t.MaxIdleConnsPerHost;

        if (v != 0) {
            return v;
        }
    }
    return DefaultMaxIdleConnsPerHost;
}

// tryPutIdleConn adds pconn to the list of idle persistent connections awaiting
// a new request.
// If pconn is no longer needed or not in a good state, tryPutIdleConn returns
// an error explaining why it wasn't registered.
// tryPutIdleConn does not close pconn. Use putOrCloseIdleConn instead for that.
private static error tryPutIdleConn(this ptr<Transport> _addr_t, ptr<persistConn> _addr_pconn) => func((defer, _, _) => {
    ref Transport t = ref _addr_t.val;
    ref persistConn pconn = ref _addr_pconn.val;

    if (t.DisableKeepAlives || t.MaxIdleConnsPerHost < 0) {
        return error.As(errKeepAlivesDisabled)!;
    }
    if (pconn.isBroken()) {
        return error.As(errConnBroken)!;
    }
    pconn.markReused();

    t.idleMu.Lock();
    defer(t.idleMu.Unlock()); 

    // HTTP/2 (pconn.alt != nil) connections do not come out of the idle list,
    // because multiple goroutines can use them simultaneously.
    // If this is an HTTP/2 connection being “returned,” we're done.
    if (pconn.alt != null && t.idleLRU.m[pconn] != null) {
        return error.As(null!)!;
    }
    var key = pconn.cacheKey;
    {
        var (q, ok) = t.idleConnWait[key];

        if (ok) {
            var done = false;
            if (pconn.alt == null) { 
                // HTTP/1.
                // Loop over the waiting list until we find a w that isn't done already, and hand it pconn.
                while (q.len() > 0) {
                    var w = q.popFront();
                    if (w.tryDeliver(pconn, null)) {
                        done = true;
                        break;
                    }
                }
            else
            } { 
                // HTTP/2.
                // Can hand the same pconn to everyone in the waiting list,
                // and we still won't be done: we want to put it in the idle
                // list unconditionally, for any future clients too.
                while (q.len() > 0) {
                    w = q.popFront();
                    w.tryDeliver(pconn, null);
                }
            }
            if (q.len() == 0) {
                delete(t.idleConnWait, key);
            }
            else
 {
                t.idleConnWait[key] = q;
            }
            if (done) {
                return error.As(null!)!;
            }
        }
    }

    if (t.closeIdle) {
        return error.As(errCloseIdle)!;
    }
    if (t.idleConn == null) {
        t.idleConn = make_map<connectMethodKey, slice<ptr<persistConn>>>();
    }
    var idles = t.idleConn[key];
    if (len(idles) >= t.maxIdleConnsPerHost()) {
        return error.As(errTooManyIdleHost)!;
    }
    foreach (var (_, exist) in idles) {
        if (exist == pconn) {
            log.Fatalf("dup idle pconn %p in freelist", pconn);
        }
    }    t.idleConn[key] = append(idles, pconn);
    t.idleLRU.add(pconn);
    if (t.MaxIdleConns != 0 && t.idleLRU.len() > t.MaxIdleConns) {
        var oldest = t.idleLRU.removeOldest();
        oldest.close(errTooManyIdle);
        t.removeIdleConnLocked(oldest);
    }
    if (t.IdleConnTimeout > 0 && pconn.alt == null) {
        if (pconn.idleTimer != null) {
            pconn.idleTimer.Reset(t.IdleConnTimeout);
        }
        else
 {
            pconn.idleTimer = time.AfterFunc(t.IdleConnTimeout, pconn.closeConnIfStillIdle);
        }
    }
    pconn.idleAt = time.Now();
    return error.As(null!)!;
});

// queueForIdleConn queues w to receive the next idle connection for w.cm.
// As an optimization hint to the caller, queueForIdleConn reports whether
// it successfully delivered an already-idle connection.
private static bool queueForIdleConn(this ptr<Transport> _addr_t, ptr<wantConn> _addr_w) => func((defer, _, _) => {
    bool delivered = default;
    ref Transport t = ref _addr_t.val;
    ref wantConn w = ref _addr_w.val;

    if (t.DisableKeepAlives) {
        return false;
    }
    t.idleMu.Lock();
    defer(t.idleMu.Unlock()); 

    // Stop closing connections that become idle - we might want one.
    // (That is, undo the effect of t.CloseIdleConnections.)
    t.closeIdle = false;

    if (w == null) { 
        // Happens in test hook.
        return false;
    }
    time.Time oldTime = default;
    if (t.IdleConnTimeout > 0) {
        oldTime = time.Now().Add(-t.IdleConnTimeout);
    }
    {
        var (list, ok) = t.idleConn[w.key];

        if (ok) {
            var stop = false;
            var delivered = false;
            while (len(list) > 0 && !stop) {
                var pconn = list[len(list) - 1]; 

                // See whether this connection has been idle too long, considering
                // only the wall time (the Round(0)), in case this is a laptop or VM
                // coming out of suspend with previously cached idle connections.
                var tooOld = !oldTime.IsZero() && pconn.idleAt.Round(0).Before(oldTime);
                if (tooOld) { 
                    // Async cleanup. Launch in its own goroutine (as if a
                    // time.AfterFunc called it); it acquires idleMu, which we're
                    // holding, and does a synchronous net.Conn.Close.
                    go_(() => pconn.closeConnIfStillIdle());
                }
                if (pconn.isBroken() || tooOld) { 
                    // If either persistConn.readLoop has marked the connection
                    // broken, but Transport.removeIdleConn has not yet removed it
                    // from the idle list, or if this persistConn is too old (it was
                    // idle too long), then ignore it and look for another. In both
                    // cases it's already in the process of being closed.
                    list = list[..(int)len(list) - 1];
                    continue;
                }
                delivered = w.tryDeliver(pconn, null);
                if (delivered) {
                    if (pconn.alt != null) { 
                        // HTTP/2: multiple clients can share pconn.
                        // Leave it in the list.
                    }
                    else
 { 
                        // HTTP/1: only one client can use pconn.
                        // Remove it from the list.
                        t.idleLRU.remove(pconn);
                        list = list[..(int)len(list) - 1];
                    }
                }
                stop = true;
            }

            if (len(list) > 0) {
                t.idleConn[w.key] = list;
            }
            else
 {
                delete(t.idleConn, w.key);
            }
            if (stop) {
                return delivered;
            }
        }
    } 

    // Register to receive next connection that becomes idle.
    if (t.idleConnWait == null) {
        t.idleConnWait = make_map<connectMethodKey, wantConnQueue>();
    }
    var q = t.idleConnWait[w.key];
    q.cleanFront();
    q.pushBack(w);
    t.idleConnWait[w.key] = q;
    return false;
});

// removeIdleConn marks pconn as dead.
private static bool removeIdleConn(this ptr<Transport> _addr_t, ptr<persistConn> _addr_pconn) => func((defer, _, _) => {
    ref Transport t = ref _addr_t.val;
    ref persistConn pconn = ref _addr_pconn.val;

    t.idleMu.Lock();
    defer(t.idleMu.Unlock());
    return t.removeIdleConnLocked(pconn);
});

// t.idleMu must be held.
private static bool removeIdleConnLocked(this ptr<Transport> _addr_t, ptr<persistConn> _addr_pconn) {
    ref Transport t = ref _addr_t.val;
    ref persistConn pconn = ref _addr_pconn.val;

    if (pconn.idleTimer != null) {
        pconn.idleTimer.Stop();
    }
    t.idleLRU.remove(pconn);
    var key = pconn.cacheKey;
    var pconns = t.idleConn[key];
    bool removed = default;
    switch (len(pconns)) {
        case 0: 

            break;
        case 1: 
            if (pconns[0] == pconn) {
                delete(t.idleConn, key);
                removed = true;
            }
            break;
        default: 
            foreach (var (i, v) in pconns) {
                if (v != pconn) {
                    continue;
                } 
                // Slide down, keeping most recently-used
                // conns at the end.
                copy(pconns[(int)i..], pconns[(int)i + 1..]);
                t.idleConn[key] = pconns[..(int)len(pconns) - 1];
                removed = true;
                break;
            }
            break;
    }
    return removed;
}

private static void setReqCanceler(this ptr<Transport> _addr_t, cancelKey key, Action<error> fn) => func((defer, _, _) => {
    ref Transport t = ref _addr_t.val;

    t.reqMu.Lock();
    defer(t.reqMu.Unlock());
    if (t.reqCanceler == null) {
        t.reqCanceler = make_map<cancelKey, Action<error>>();
    }
    if (fn != null) {
        t.reqCanceler[key] = fn;
    }
    else
 {
        delete(t.reqCanceler, key);
    }
});

// replaceReqCanceler replaces an existing cancel function. If there is no cancel function
// for the request, we don't set the function and return false.
// Since CancelRequest will clear the canceler, we can use the return value to detect if
// the request was canceled since the last setReqCancel call.
private static bool replaceReqCanceler(this ptr<Transport> _addr_t, cancelKey key, Action<error> fn) => func((defer, _, _) => {
    ref Transport t = ref _addr_t.val;

    t.reqMu.Lock();
    defer(t.reqMu.Unlock());
    var (_, ok) = t.reqCanceler[key];
    if (!ok) {
        return false;
    }
    if (fn != null) {
        t.reqCanceler[key] = fn;
    }
    else
 {
        delete(t.reqCanceler, key);
    }
    return true;
});

private static net.Dialer zeroDialer = default;

private static (net.Conn, error) dial(this ptr<Transport> _addr_t, context.Context ctx, @string network, @string addr) {
    net.Conn _p0 = default;
    error _p0 = default!;
    ref Transport t = ref _addr_t.val;

    if (t.DialContext != null) {
        return t.DialContext(ctx, network, addr);
    }
    if (t.Dial != null) {
        var (c, err) = t.Dial(network, addr);
        if (c == null && err == null) {
            err = errors.New("net/http: Transport.Dial hook returned (nil, nil)");
        }
        return (c, error.As(err)!);
    }
    return zeroDialer.DialContext(ctx, network, addr);
}

// A wantConn records state about a wanted connection
// (that is, an active call to getConn).
// The conn may be gotten by dialing or by finding an idle connection,
// or a cancellation may make the conn no longer wanted.
// These three options are racing against each other and use
// wantConn to coordinate and agree about the winning outcome.
private partial struct wantConn {
    public connectMethod cm;
    public connectMethodKey key; // cm.key()
    public context.Context ctx; // context for dial
    public channel<object> ready; // closed when pc, err pair is delivered

// hooks for testing to know when dials are done
// beforeDial is called in the getConn goroutine when the dial is queued.
// afterDial is called when the dial is completed or canceled.
    public Action beforeDial;
    public Action afterDial;
    public sync.Mutex mu; // protects pc, err, close(ready)
    public ptr<persistConn> pc;
    public error err;
}

// waiting reports whether w is still waiting for an answer (connection or error).
private static bool waiting(this ptr<wantConn> _addr_w) {
    ref wantConn w = ref _addr_w.val;

    return false;
    return true;
}

// tryDeliver attempts to deliver pc, err to w and reports whether it succeeded.
private static bool tryDeliver(this ptr<wantConn> _addr_w, ptr<persistConn> _addr_pc, error err) => func((defer, panic, _) => {
    ref wantConn w = ref _addr_w.val;
    ref persistConn pc = ref _addr_pc.val;

    w.mu.Lock();
    defer(w.mu.Unlock());

    if (w.pc != null || w.err != null) {
        return false;
    }
    w.pc = pc;
    w.err = err;
    if (w.pc == null && w.err == null) {
        panic("net/http: internal error: misuse of tryDeliver");
    }
    close(w.ready);
    return true;
});

// cancel marks w as no longer wanting a result (for example, due to cancellation).
// If a connection has been delivered already, cancel returns it with t.putOrCloseIdleConn.
private static void cancel(this ptr<wantConn> _addr_w, ptr<Transport> _addr_t, error err) {
    ref wantConn w = ref _addr_w.val;
    ref Transport t = ref _addr_t.val;

    w.mu.Lock();
    if (w.pc == null && w.err == null) {
        close(w.ready); // catch misbehavior in future delivery
    }
    var pc = w.pc;
    w.pc = null;
    w.err = err;
    w.mu.Unlock();

    if (pc != null) {
        t.putOrCloseIdleConn(pc);
    }
}

// A wantConnQueue is a queue of wantConns.
private partial struct wantConnQueue {
    public slice<ptr<wantConn>> head;
    public nint headPos;
    public slice<ptr<wantConn>> tail;
}

// len returns the number of items in the queue.
private static nint len(this ptr<wantConnQueue> _addr_q) {
    ref wantConnQueue q = ref _addr_q.val;

    return len(q.head) - q.headPos + len(q.tail);
}

// pushBack adds w to the back of the queue.
private static void pushBack(this ptr<wantConnQueue> _addr_q, ptr<wantConn> _addr_w) {
    ref wantConnQueue q = ref _addr_q.val;
    ref wantConn w = ref _addr_w.val;

    q.tail = append(q.tail, w);
}

// popFront removes and returns the wantConn at the front of the queue.
private static ptr<wantConn> popFront(this ptr<wantConnQueue> _addr_q) {
    ref wantConnQueue q = ref _addr_q.val;

    if (q.headPos >= len(q.head)) {
        if (len(q.tail) == 0) {
            return _addr_null!;
        }
        (q.head, q.headPos, q.tail) = (q.tail, 0, q.head[..(int)0]);
    }
    var w = q.head[q.headPos];
    q.head[q.headPos] = null;
    q.headPos++;
    return _addr_w!;
}

// peekFront returns the wantConn at the front of the queue without removing it.
private static ptr<wantConn> peekFront(this ptr<wantConnQueue> _addr_q) {
    ref wantConnQueue q = ref _addr_q.val;

    if (q.headPos < len(q.head)) {
        return _addr_q.head[q.headPos]!;
    }
    if (len(q.tail) > 0) {
        return _addr_q.tail[0]!;
    }
    return _addr_null!;
}

// cleanFront pops any wantConns that are no longer waiting from the head of the
// queue, reporting whether any were popped.
private static bool cleanFront(this ptr<wantConnQueue> _addr_q) {
    bool cleaned = default;
    ref wantConnQueue q = ref _addr_q.val;

    while (true) {
        var w = q.peekFront();
        if (w == null || w.waiting()) {
            return cleaned;
        }
        q.popFront();
        cleaned = true;
    }
}

private static (net.Conn, error) customDialTLS(this ptr<Transport> _addr_t, context.Context ctx, @string network, @string addr) {
    net.Conn conn = default;
    error err = default!;
    ref Transport t = ref _addr_t.val;

    if (t.DialTLSContext != null) {
        conn, err = t.DialTLSContext(ctx, network, addr);
    }
    else
 {
        conn, err = t.DialTLS(network, addr);
    }
    if (conn == null && err == null) {
        err = errors.New("net/http: Transport.DialTLS or DialTLSContext returned (nil, nil)");
    }
    return ;
}

// getConn dials and creates a new persistConn to the target as
// specified in the connectMethod. This includes doing a proxy CONNECT
// and/or setting up TLS.  If this doesn't return an error, the persistConn
// is ready to write requests to.
private static (ptr<persistConn>, error) getConn(this ptr<Transport> _addr_t, ptr<transportRequest> _addr_treq, connectMethod cm) => func((defer, _, _) => {
    ptr<persistConn> pc = default!;
    error err = default!;
    ref Transport t = ref _addr_t.val;
    ref transportRequest treq = ref _addr_treq.val;

    var req = treq.Request;
    var trace = treq.trace;
    var ctx = req.Context();
    if (trace != null && trace.GetConn != null) {
        trace.GetConn(cm.addr());
    }
    ptr<wantConn> w = addr(new wantConn(cm:cm,key:cm.key(),ctx:ctx,ready:make(chanstruct{},1),beforeDial:testHookPrePendingDial,afterDial:testHookPostPendingDial,));
    defer(() => {
        if (err != null) {
            w.cancel(t, err);
        }
    }()); 

    // Queue for idle connection.
    {
        var delivered = t.queueForIdleConn(w);

        if (delivered) {
            var pc = w.pc; 
            // Trace only for HTTP/1.
            // HTTP/2 calls trace.GotConn itself.
            if (pc.alt == null && trace != null && trace.GotConn != null) {
                trace.GotConn(pc.gotIdleConnTrace(pc.idleAt));
            } 
            // set request canceler to some non-nil function so we
            // can detect whether it was cleared between now and when
            // we enter roundTrip
            t.setReqCanceler(treq.cancelKey, _p0 => {
            });
            return (_addr_pc!, error.As(null!)!);
        }
    }

    var cancelc = make_channel<error>(1);
    t.setReqCanceler(treq.cancelKey, err => {
        cancelc.Send(err);
    }); 

    // Queue for permission to dial.
    t.queueForDial(w); 

    // Wait for completion or cancellation.
    if (w.pc != null && w.pc.alt == null && trace != null && trace.GotConn != null) {
        trace.GotConn(new httptrace.GotConnInfo(Conn:w.pc.conn,Reused:w.pc.isReused()));
    }
    if (w.err != null) { 
        // If the request has been canceled, that's probably
        // what caused w.err; if so, prefer to return the
        // cancellation error (see golang.org/issue/16049).
        return (_addr_null!, error.As(errRequestCanceledConn)!);
        return (_addr_null!, error.As(req.Context().Err())!);
        if (err == errRequestCanceled) {
            err = errRequestCanceledConn;
        }
        return (_addr_null!, error.As(err)!);
    }
    return (_addr_w.pc!, error.As(w.err)!);
    return (_addr_null!, error.As(errRequestCanceledConn)!);
    return (_addr_null!, error.As(req.Context().Err())!);
    if (err == errRequestCanceled) {
        err = errRequestCanceledConn;
    }
    return (_addr_null!, error.As(err)!);
});

// queueForDial queues w to wait for permission to begin dialing.
// Once w receives permission to dial, it will do so in a separate goroutine.
private static void queueForDial(this ptr<Transport> _addr_t, ptr<wantConn> _addr_w) => func((defer, _, _) => {
    ref Transport t = ref _addr_t.val;
    ref wantConn w = ref _addr_w.val;

    w.beforeDial();
    if (t.MaxConnsPerHost <= 0) {
        go_(() => t.dialConnFor(w));
        return ;
    }
    t.connsPerHostMu.Lock();
    defer(t.connsPerHostMu.Unlock());

    {
        var n = t.connsPerHost[w.key];

        if (n < t.MaxConnsPerHost) {
            if (t.connsPerHost == null) {
                t.connsPerHost = make_map<connectMethodKey, nint>();
            }
            t.connsPerHost[w.key] = n + 1;
            go_(() => t.dialConnFor(w));
            return ;
        }
    }

    if (t.connsPerHostWait == null) {
        t.connsPerHostWait = make_map<connectMethodKey, wantConnQueue>();
    }
    var q = t.connsPerHostWait[w.key];
    q.cleanFront();
    q.pushBack(w);
    t.connsPerHostWait[w.key] = q;
});

// dialConnFor dials on behalf of w and delivers the result to w.
// dialConnFor has received permission to dial w.cm and is counted in t.connCount[w.cm.key()].
// If the dial is canceled or unsuccessful, dialConnFor decrements t.connCount[w.cm.key()].
private static void dialConnFor(this ptr<Transport> _addr_t, ptr<wantConn> _addr_w) => func((defer, _, _) => {
    ref Transport t = ref _addr_t.val;
    ref wantConn w = ref _addr_w.val;

    defer(w.afterDial());

    var (pc, err) = t.dialConn(w.ctx, w.cm);
    var delivered = w.tryDeliver(pc, err);
    if (err == null && (!delivered || pc.alt != null)) { 
        // pconn was not passed to w,
        // or it is HTTP/2 and can be shared.
        // Add to the idle connection pool.
        t.putOrCloseIdleConn(pc);
    }
    if (err != null) {
        t.decConnsPerHost(w.key);
    }
});

// decConnsPerHost decrements the per-host connection count for key,
// which may in turn give a different waiting goroutine permission to dial.
private static void decConnsPerHost(this ptr<Transport> _addr_t, connectMethodKey key) => func((defer, panic, _) => {
    ref Transport t = ref _addr_t.val;

    if (t.MaxConnsPerHost <= 0) {
        return ;
    }
    t.connsPerHostMu.Lock();
    defer(t.connsPerHostMu.Unlock());
    var n = t.connsPerHost[key];
    if (n == 0) { 
        // Shouldn't happen, but if it does, the counting is buggy and could
        // easily lead to a silent deadlock, so report the problem loudly.
        panic("net/http: internal error: connCount underflow");
    }
    {
        var q = t.connsPerHostWait[key];

        if (q.len() > 0) {
            var done = false;
            while (q.len() > 0) {
                var w = q.popFront();
                if (w.waiting()) {
                    go_(() => t.dialConnFor(w));
                    done = true;
                    break;
                }
            }

            if (q.len() == 0) {
                delete(t.connsPerHostWait, key);
            }
            else
 { 
                // q is a value (like a slice), so we have to store
                // the updated q back into the map.
                t.connsPerHostWait[key] = q;
            }
            if (done) {
                return ;
            }
        }
    } 

    // Otherwise, decrement the recorded count.
    n--;

    if (n == 0) {
        delete(t.connsPerHost, key);
    }
    else
 {
        t.connsPerHost[key] = n;
    }
});

// Add TLS to a persistent connection, i.e. negotiate a TLS session. If pconn is already a TLS
// tunnel, this function establishes a nested TLS session inside the encrypted channel.
// The remote endpoint's name may be overridden by TLSClientConfig.ServerName.
private static error addTLS(this ptr<persistConn> _addr_pconn, context.Context ctx, @string name, ptr<httptrace.ClientTrace> _addr_trace) {
    ref persistConn pconn = ref _addr_pconn.val;
    ref httptrace.ClientTrace trace = ref _addr_trace.val;
 
    // Initiate TLS and check remote host name against certificate.
    var cfg = cloneTLSConfig(_addr_pconn.t.TLSClientConfig);
    if (cfg.ServerName == "") {
        cfg.ServerName = name;
    }
    if (pconn.cacheKey.onlyH1) {
        cfg.NextProtos = null;
    }
    var plainConn = pconn.conn;
    var tlsConn = tls.Client(plainConn, cfg);
    var errc = make_channel<error>(2);
    ptr<time.Timer> timer; // for canceling TLS handshake
    {
        var d = pconn.t.TLSHandshakeTimeout;

        if (d != 0) {
            timer = time.AfterFunc(d, () => {
                errc.Send(new tlsHandshakeTimeoutError());
            });
        }
    }
    go_(() => () => {
        if (trace != null && trace.TLSHandshakeStart != null) {
            trace.TLSHandshakeStart();
        }
        var err = tlsConn.HandshakeContext(ctx);
        if (timer != null) {
            timer.Stop();
        }
        errc.Send(err);
    }());
    {
        var err__prev1 = err;

        err = errc.Receive();

        if (err != null) {
            plainConn.Close();
            if (trace != null && trace.TLSHandshakeDone != null) {
                trace.TLSHandshakeDone(new tls.ConnectionState(), err);
            }
            return error.As(err)!;
        }
        err = err__prev1;

    }
    ref var cs = ref heap(tlsConn.ConnectionState(), out ptr<var> _addr_cs);
    if (trace != null && trace.TLSHandshakeDone != null) {
        trace.TLSHandshakeDone(cs, null);
    }
    _addr_pconn.tlsState = _addr_cs;
    pconn.tlsState = ref _addr_pconn.tlsState.val;
    pconn.conn = tlsConn;
    return error.As(null!)!;
}

private partial interface erringRoundTripper {
    error RoundTripErr();
}

private static (ptr<persistConn>, error) dialConn(this ptr<Transport> _addr_t, context.Context ctx, connectMethod cm) => func((defer, _, _) => {
    ptr<persistConn> pconn = default!;
    error err = default!;
    ref Transport t = ref _addr_t.val;

    pconn = addr(new persistConn(t:t,cacheKey:cm.key(),reqch:make(chanrequestAndChan,1),writech:make(chanwriteRequest,1),closech:make(chanstruct{}),writeErrCh:make(chanerror,1),writeLoopDone:make(chanstruct{}),));
    var trace = httptrace.ContextClientTrace(ctx);
    Func<error, error> wrapErr = err => {
        if (cm.proxyURL != null) { 
            // Return a typed error, per Issue 16997
            return addr(new net.OpError(Op:"proxyconnect",Net:"tcp",Err:err));
        }
        return _addr_err!;
    };
    if (cm.scheme() == "https" && t.hasCustomTLSDialer()) {
        error err = default!;
        pconn.conn, err = t.customDialTLS(ctx, "tcp", cm.addr());
        if (err != null) {
            return (_addr_null!, error.As(wrapErr(err))!);
        }
        {
            ptr<tls.Conn> (tc, ok) = pconn.conn._<ptr<tls.Conn>>();

            if (ok) { 
                // Handshake here, in case DialTLS didn't. TLSNextProto below
                // depends on it for knowing the connection state.
                if (trace != null && trace.TLSHandshakeStart != null) {
                    trace.TLSHandshakeStart();
                }
                {
                    error err__prev3 = err;

                    err = tc.HandshakeContext(ctx);

                    if (err != null) {
                        go_(() => pconn.conn.Close());
                        if (trace != null && trace.TLSHandshakeDone != null) {
                            trace.TLSHandshakeDone(new tls.ConnectionState(), err);
                        }
                        return (_addr_null!, error.As(err)!);
                    }

                    err = err__prev3;

                }
                ref var cs = ref heap(tc.ConnectionState(), out ptr<var> _addr_cs);
                if (trace != null && trace.TLSHandshakeDone != null) {
                    trace.TLSHandshakeDone(cs, null);
                }
                _addr_pconn.tlsState = _addr_cs;
                pconn.tlsState = ref _addr_pconn.tlsState.val;
            }

        }
    }
    else
 {
        var (conn, err) = t.dial(ctx, "tcp", cm.addr());
        if (err != null) {
            return (_addr_null!, error.As(wrapErr(err))!);
        }
        pconn.conn = conn;
        if (cm.scheme() == "https") {
            @string firstTLSHost = default;
            firstTLSHost, _, err = net.SplitHostPort(cm.addr());

            if (err != null) {
                return (_addr_null!, error.As(wrapErr(err))!);
            }
            err = error.As(pconn.addTLS(ctx, firstTLSHost, trace))!;

            if (err != null) {
                return (_addr_null!, error.As(wrapErr(err))!);
            }
        }
    }

    if (cm.proxyURL == null)     else if (cm.proxyURL.Scheme == "socks5") 
        var conn = pconn.conn;
        var d = socksNewDialer("tcp", conn.RemoteAddr().String());
        {
            var u = cm.proxyURL.User;

            if (u != null) {
                ptr<socksUsernamePassword> auth = addr(new socksUsernamePassword(Username:u.Username(),));
                auth.Password, _ = u.Password();
                d.AuthMethods = new slice<socksAuthMethod>(new socksAuthMethod[] { socksAuthMethodNotRequired, socksAuthMethodUsernamePassword });
                d.Authenticate = auth.Authenticate;
            }

        }
        {
            error err__prev1 = err;

            var (_, err) = d.DialWithConn(ctx, conn, "tcp", cm.targetAddr);

            if (err != null) {
                conn.Close();
                return (_addr_null!, error.As(err)!);
            }

            err = err__prev1;

        }
    else if (cm.targetScheme == "http") 
        pconn.isProxy = true;
        {
            var pa__prev1 = pa;

            var pa = cm.proxyAuth();

            if (pa != "") {
                pconn.mutateHeaderFunc = h => {
                    h.Set("Proxy-Authorization", pa);
                }
;
            }

            pa = pa__prev1;

        }
    else if (cm.targetScheme == "https") 
        conn = pconn.conn;
        Header hdr = default;
        if (t.GetProxyConnectHeader != null) {
            err = default!;
            hdr, err = t.GetProxyConnectHeader(ctx, cm.proxyURL, cm.targetAddr);
            if (err != null) {
                conn.Close();
                return (_addr_null!, error.As(err)!);
            }
        }
        else
 {
            hdr = t.ProxyConnectHeader;
        }
        if (hdr == null) {
            hdr = make(Header);
        }
        {
            var pa__prev1 = pa;

            pa = cm.proxyAuth();

            if (pa != "") {
                hdr = hdr.Clone();
                hdr.Set("Proxy-Authorization", pa);
            }

            pa = pa__prev1;

        }
        ptr<Request> connectReq = addr(new Request(Method:"CONNECT",URL:&url.URL{Opaque:cm.targetAddr},Host:cm.targetAddr,Header:hdr,)); 

        // If there's no done channel (no deadline or cancellation
        // from the caller possible), at least set some (long)
        // timeout here. This will make sure we don't block forever
        // and leak a goroutine if the connection stops replying
        // after the TCP connect.
        var connectCtx = ctx;
        if (ctx.Done() == null) {
            var (newCtx, cancel) = context.WithTimeout(ctx, 1 * time.Minute);
            defer(cancel());
            connectCtx = newCtx;
        }
        var didReadResponse = make_channel<object>(); // closed after CONNECT write+read is done or fails
        ptr<Response> resp;        err = default!; 
        // Write the CONNECT request & read the response.
        go_(() => () => {
            defer(close(didReadResponse));
            err = error.As(connectReq.Write(conn))!;
            if (err != null) {
                return ;
            } 
            // Okay to use and discard buffered reader here, because
            // TLS server will not speak until spoken to.
            var br = bufio.NewReader(conn);
            resp, err = ReadResponse(br, connectReq);
        }());
        conn.Close().Send(didReadResponse);
        return (_addr_null!, error.As(connectCtx.Err())!);
        if (err != null) {
            conn.Close();
            return (_addr_null!, error.As(err)!);
        }
        if (resp.StatusCode != 200) {
            var f = strings.SplitN(resp.Status, " ", 2);
            conn.Close();
            if (len(f) < 2) {
                return (_addr_null!, error.As(errors.New("unknown status code"))!);
            }
            return (_addr_null!, error.As(errors.New(f[1]))!);
        }
        if (cm.proxyURL != null && cm.targetScheme == "https") {
        {
            error err__prev2 = err;

            err = pconn.addTLS(ctx, cm.tlsHost(), trace);

            if (err != null) {
                return (_addr_null!, error.As(err)!);
            }

            err = err__prev2;

        }
    }
    {
        var s = pconn.tlsState;

        if (s != null && s.NegotiatedProtocolIsMutual && s.NegotiatedProtocol != "") {
            {
                var (next, ok) = t.TLSNextProto[s.NegotiatedProtocol];

                if (ok) {
                    var alt = next(cm.targetAddr, pconn.conn._<ptr<tls.Conn>>());
                    {
                        erringRoundTripper (e, ok) = erringRoundTripper.As(alt._<erringRoundTripper>())!;

                        if (ok) { 
                            // pconn.conn was closed by next (http2configureTransports.upgradeFn).
                            return (_addr_null!, error.As(e.RoundTripErr())!);
                        }

                    }
                    return (addr(new persistConn(t:t,cacheKey:pconn.cacheKey,alt:alt)), error.As(null!)!);
                }

            }
        }
    }

    pconn.br = bufio.NewReaderSize(pconn, t.readBufferSize());
    pconn.bw = bufio.NewWriterSize(new persistConnWriter(pconn), t.writeBufferSize());

    go_(() => pconn.readLoop());
    go_(() => pconn.writeLoop());
    return (_addr_pconn!, error.As(null!)!);
});

// persistConnWriter is the io.Writer written to by pc.bw.
// It accumulates the number of bytes written to the underlying conn,
// so the retry logic can determine whether any bytes made it across
// the wire.
// This is exactly 1 pointer field wide so it can go into an interface
// without allocation.
private partial struct persistConnWriter {
    public ptr<persistConn> pc;
}

private static (nint, error) Write(this persistConnWriter w, slice<byte> p) {
    nint n = default;
    error err = default!;

    n, err = w.pc.conn.Write(p);
    w.pc.nwrite += int64(n);
    return ;
}

// ReadFrom exposes persistConnWriter's underlying Conn to io.Copy and if
// the Conn implements io.ReaderFrom, it can take advantage of optimizations
// such as sendfile.
private static (long, error) ReadFrom(this persistConnWriter w, io.Reader r) {
    long n = default;
    error err = default!;

    n, err = io.Copy(w.pc.conn, r);
    w.pc.nwrite += n;
    return ;
}

private static io.ReaderFrom _ = (persistConnWriter.val)(null);

// connectMethod is the map key (in its String form) for keeping persistent
// TCP connections alive for subsequent HTTP requests.
//
// A connect method may be of the following types:
//
//    connectMethod.key().String()      Description
//    ------------------------------    -------------------------
//    |http|foo.com                     http directly to server, no proxy
//    |https|foo.com                    https directly to server, no proxy
//    |https,h1|foo.com                 https directly to server w/o HTTP/2, no proxy
//    http://proxy.com|https|foo.com    http to proxy, then CONNECT to foo.com
//    http://proxy.com|http             http to proxy, http to anywhere after that
//    socks5://proxy.com|http|foo.com   socks5 to proxy, then http to foo.com
//    socks5://proxy.com|https|foo.com  socks5 to proxy, then https to foo.com
//    https://proxy.com|https|foo.com   https to proxy, then CONNECT to foo.com
//    https://proxy.com|http            https to proxy, http to anywhere after that
//
private partial struct connectMethod {
    public incomparable _;
    public ptr<url.URL> proxyURL; // nil for no proxy, else full proxy URL
    public @string targetScheme; // "http" or "https"
// If proxyURL specifies an http or https proxy, and targetScheme is http (not https),
// then targetAddr is not included in the connect method key, because the socket can
// be reused for different targetAddr values.
    public @string targetAddr;
    public bool onlyH1; // whether to disable HTTP/2 and force HTTP/1
}

private static connectMethodKey key(this ptr<connectMethod> _addr_cm) {
    ref connectMethod cm = ref _addr_cm.val;

    @string proxyStr = "";
    var targetAddr = cm.targetAddr;
    if (cm.proxyURL != null) {
        proxyStr = cm.proxyURL.String();
        if ((cm.proxyURL.Scheme == "http" || cm.proxyURL.Scheme == "https") && cm.targetScheme == "http") {
            targetAddr = "";
        }
    }
    return new connectMethodKey(proxy:proxyStr,scheme:cm.targetScheme,addr:targetAddr,onlyH1:cm.onlyH1,);
}

// scheme returns the first hop scheme: http, https, or socks5
private static @string scheme(this ptr<connectMethod> _addr_cm) {
    ref connectMethod cm = ref _addr_cm.val;

    if (cm.proxyURL != null) {
        return cm.proxyURL.Scheme;
    }
    return cm.targetScheme;
}

// addr returns the first hop "host:port" to which we need to TCP connect.
private static @string addr(this ptr<connectMethod> _addr_cm) {
    ref connectMethod cm = ref _addr_cm.val;

    if (cm.proxyURL != null) {
        return canonicalAddr(_addr_cm.proxyURL);
    }
    return cm.targetAddr;
}

// tlsHost returns the host name to match against the peer's
// TLS certificate.
private static @string tlsHost(this ptr<connectMethod> _addr_cm) {
    ref connectMethod cm = ref _addr_cm.val;

    var h = cm.targetAddr;
    if (hasPort(h)) {
        h = h[..(int)strings.LastIndex(h, ":")];
    }
    return h;
}

// connectMethodKey is the map key version of connectMethod, with a
// stringified proxy URL (or the empty string) instead of a pointer to
// a URL.
private partial struct connectMethodKey {
    public @string proxy;
    public @string scheme;
    public @string addr;
    public bool onlyH1;
}

private static @string String(this connectMethodKey k) { 
    // Only used by tests.
    @string h1 = default;
    if (k.onlyH1) {
        h1 = ",h1";
    }
    return fmt.Sprintf("%s|%s%s|%s", k.proxy, k.scheme, h1, k.addr);
}

// persistConn wraps a connection, usually a persistent one
// (but may be used for non-keep-alive requests as well)
private partial struct persistConn {
    public RoundTripper alt;
    public ptr<Transport> t;
    public connectMethodKey cacheKey;
    public net.Conn conn;
    public ptr<tls.ConnectionState> tlsState;
    public ptr<bufio.Reader> br; // from conn
    public ptr<bufio.Writer> bw; // to conn
    public long nwrite; // bytes written
    public channel<requestAndChan> reqch; // written by roundTrip; read by readLoop
    public channel<writeRequest> writech; // written by roundTrip; read by writeLoop
    public channel<object> closech; // closed when conn closed
    public bool isProxy;
    public bool sawEOF; // whether we've seen EOF from conn; owned by readLoop
    public long readLimit; // bytes allowed to be read; owned by readLoop
// writeErrCh passes the request write error (usually nil)
// from the writeLoop goroutine to the readLoop which passes
// it off to the res.Body reader, which then uses it to decide
// whether or not a connection can be reused. Issue 7569.
    public channel<error> writeErrCh;
    public channel<object> writeLoopDone; // closed when write loop ends

// Both guarded by Transport.idleMu:
    public time.Time idleAt; // time it last become idle
    public ptr<time.Timer> idleTimer; // holding an AfterFunc to close it

    public sync.Mutex mu; // guards following fields
    public nint numExpectedResponses;
    public error closed; // set non-nil when conn is closed, before closech is closed
    public error canceledErr; // set non-nil if conn is canceled
    public bool broken; // an error has happened on this connection; marked broken so it's not reused.
    public bool reused; // whether conn has had successful request/response and is being reused.
// mutateHeaderFunc is an optional func to modify extra
// headers on each outbound request before it's written. (the
// original Request given to RoundTrip is not modified)
    public Action<Header> mutateHeaderFunc;
}

private static long maxHeaderResponseSize(this ptr<persistConn> _addr_pc) {
    ref persistConn pc = ref _addr_pc.val;

    {
        var v = pc.t.MaxResponseHeaderBytes;

        if (v != 0) {
            return v;
        }
    }
    return 10 << 20; // conservative default; same as http2
}

private static (nint, error) Read(this ptr<persistConn> _addr_pc, slice<byte> p) {
    nint n = default;
    error err = default!;
    ref persistConn pc = ref _addr_pc.val;

    if (pc.readLimit <= 0) {
        return (0, error.As(fmt.Errorf("read limit of %d bytes exhausted", pc.maxHeaderResponseSize()))!);
    }
    if (int64(len(p)) > pc.readLimit) {
        p = p[..(int)pc.readLimit];
    }
    n, err = pc.conn.Read(p);
    if (err == io.EOF) {
        pc.sawEOF = true;
    }
    pc.readLimit -= int64(n);
    return ;
}

// isBroken reports whether this connection is in a known broken state.
private static bool isBroken(this ptr<persistConn> _addr_pc) {
    ref persistConn pc = ref _addr_pc.val;

    pc.mu.Lock();
    var b = pc.closed != null;
    pc.mu.Unlock();
    return b;
}

// canceled returns non-nil if the connection was closed due to
// CancelRequest or due to context cancellation.
private static error canceled(this ptr<persistConn> _addr_pc) => func((defer, _, _) => {
    ref persistConn pc = ref _addr_pc.val;

    pc.mu.Lock();
    defer(pc.mu.Unlock());
    return error.As(pc.canceledErr)!;
});

// isReused reports whether this connection has been used before.
private static bool isReused(this ptr<persistConn> _addr_pc) {
    ref persistConn pc = ref _addr_pc.val;

    pc.mu.Lock();
    var r = pc.reused;
    pc.mu.Unlock();
    return r;
}

private static httptrace.GotConnInfo gotIdleConnTrace(this ptr<persistConn> _addr_pc, time.Time idleAt) => func((defer, _, _) => {
    httptrace.GotConnInfo t = default;
    ref persistConn pc = ref _addr_pc.val;

    pc.mu.Lock();
    defer(pc.mu.Unlock());
    t.Reused = pc.reused;
    t.Conn = pc.conn;
    t.WasIdle = true;
    if (!idleAt.IsZero()) {
        t.IdleTime = time.Since(idleAt);
    }
    return ;
});

private static void cancelRequest(this ptr<persistConn> _addr_pc, error err) => func((defer, _, _) => {
    ref persistConn pc = ref _addr_pc.val;

    pc.mu.Lock();
    defer(pc.mu.Unlock());
    pc.canceledErr = err;
    pc.closeLocked(errRequestCanceled);
});

// closeConnIfStillIdle closes the connection if it's still sitting idle.
// This is what's called by the persistConn's idleTimer, and is run in its
// own goroutine.
private static void closeConnIfStillIdle(this ptr<persistConn> _addr_pc) => func((defer, _, _) => {
    ref persistConn pc = ref _addr_pc.val;

    var t = pc.t;
    t.idleMu.Lock();
    defer(t.idleMu.Unlock());
    {
        var (_, ok) = t.idleLRU.m[pc];

        if (!ok) { 
            // Not idle.
            return ;
        }
    }
    t.removeIdleConnLocked(pc);
    pc.close(errIdleConnTimeout);
});

// mapRoundTripError returns the appropriate error value for
// persistConn.roundTrip.
//
// The provided err is the first error that (*persistConn).roundTrip
// happened to receive from its select statement.
//
// The startBytesWritten value should be the value of pc.nwrite before the roundTrip
// started writing the request.
private static error mapRoundTripError(this ptr<persistConn> _addr_pc, ptr<transportRequest> _addr_req, long startBytesWritten, error err) {
    ref persistConn pc = ref _addr_pc.val;
    ref transportRequest req = ref _addr_req.val;

    if (err == null) {
        return error.As(null!)!;
    }
    pc.writeLoopDone.Receive(); 

    // If the request was canceled, that's better than network
    // failures that were likely the result of tearing down the
    // connection.
    {
        var cerr = pc.canceled();

        if (cerr != null) {
            return error.As(cerr)!;
        }
    } 

    // See if an error was set explicitly.
    req.mu.Lock();
    var reqErr = req.err;
    req.mu.Unlock();
    if (reqErr != null) {
        return error.As(reqErr)!;
    }
    if (err == errServerClosedIdle) { 
        // Don't decorate
        return error.As(err)!;
    }
    {
        transportReadFromServerError (_, ok) = err._<transportReadFromServerError>();

        if (ok) { 
            // Don't decorate
            return error.As(err)!;
        }
    }
    if (pc.isBroken()) {
        if (pc.nwrite == startBytesWritten) {
            return error.As(new nothingWrittenError(err))!;
        }
        return error.As(fmt.Errorf("net/http: HTTP/1.x transport connection broken: %v", err))!;
    }
    return error.As(err)!;
}

// errCallerOwnsConn is an internal sentinel error used when we hand
// off a writable response.Body to the caller. We use this to prevent
// closing a net.Conn that is now owned by the caller.
private static var errCallerOwnsConn = errors.New("read loop ending; caller owns writable underlying conn");

private static void readLoop(this ptr<persistConn> _addr_pc) => func((defer, _, _) => {
    ref persistConn pc = ref _addr_pc.val;

    var closeErr = errReadLoopExiting; // default value, if not changed below
    defer(() => {
        pc.close(closeErr);
        pc.t.removeIdleConn(pc);
    }());

    Func<ptr<httptrace.ClientTrace>, bool> tryPutIdleConn = trace => {
        {
            var err = pc.t.tryPutIdleConn(pc);

            if (err != null) {
                closeErr = err;
                if (trace != null && trace.PutIdleConn != null && err != errKeepAlivesDisabled) {
                    trace.PutIdleConn(err);
                }
                return false;
            }

        }
        if (trace != null && trace.PutIdleConn != null) {
            trace.PutIdleConn(null);
        }
        return true;
    }; 

    // eofc is used to block caller goroutines reading from Response.Body
    // at EOF until this goroutines has (potentially) added the connection
    // back to the idle pool.
    var eofc = make_channel<object>();
    defer(close(eofc)); // unblock reader on errors

    // Read this once, before loop starts. (to avoid races in tests)
    testHookMu.Lock();
    var testHookReadLoopBeforeNextRead = testHookReadLoopBeforeNextRead;
    testHookMu.Unlock();

    var alive = true;
    while (alive) {
        pc.readLimit = pc.maxHeaderResponseSize();
        var (_, err) = pc.br.Peek(1);

        pc.mu.Lock();
        if (pc.numExpectedResponses == 0) {
            pc.readLoopPeekFailLocked(err);
            pc.mu.Unlock();
            return ;
        }
        pc.mu.Unlock();

        var rc = pc.reqch.Receive();
        var trace = httptrace.ContextClientTrace(rc.req.Context());

        ptr<Response> resp;
        if (err == null) {
            resp, err = pc.readResponse(rc, trace);
        }
        else
 {
            err = new transportReadFromServerError(err);
            closeErr = err;
        }
        if (err != null) {
            if (pc.readLimit <= 0) {
                err = fmt.Errorf("net/http: server response headers exceeded %d bytes; aborted", pc.maxHeaderResponseSize());
            }
            return ;
            return ;
        }
        pc.readLimit = maxInt64; // effectively no limit for response bodies

        pc.mu.Lock();
        pc.numExpectedResponses--;
        pc.mu.Unlock();

        var bodyWritable = resp.bodyIsWritable();
        var hasBody = rc.req.Method != "HEAD" && resp.ContentLength != 0;

        if (resp.Close || rc.req.Close || resp.StatusCode <= 199 || bodyWritable) { 
            // Don't do keep-alive on error if either party requested a close
            // or we get an unexpected informational (1xx) response.
            // StatusCode 100 is already handled above.
            alive = false;
        }
        if (!hasBody || bodyWritable) {
            var replaced = pc.t.replaceReqCanceler(rc.cancelKey, null); 

            // Put the idle conn back into the pool before we send the response
            // so if they process it quickly and make another request, they'll
            // get this same conn. But we use the unbuffered channel 'rc'
            // to guarantee that persistConn.roundTrip got out of its select
            // potentially waiting for this persistConn to close.
            alive = alive && !pc.sawEOF && pc.wroteRequest() && replaced && tryPutIdleConn(trace);

            if (bodyWritable) {
                closeErr = errCallerOwnsConn;
            }
            return ;
            testHookReadLoopBeforeNextRead();
            continue;
        }
        var waitForBodyRead = make_channel<bool>(2);
        ptr<bodyEOFSignal> body = addr(new bodyEOFSignal(body:resp.Body,earlyCloseFn:func()error{waitForBodyRead<-false<-eofcreturnnil},fn:func(errerror)error{isEOF:=err==io.EOFwaitForBodyRead<-isEOFifisEOF{<-eofc}elseiferr!=nil{ifcerr:=pc.canceled();cerr!=nil{returncerr}}returnerr},));

        resp.Body = body;
        if (rc.addedGzip && ascii.EqualFold(resp.Header.Get("Content-Encoding"), "gzip")) {
            resp.Body = addr(new gzipReader(body:body));
            resp.Header.Del("Content-Encoding");
            resp.Header.Del("Content-Length");
            resp.ContentLength = -1;
            resp.Uncompressed = true;
        }
        return ;
        replaced = pc.t.replaceReqCanceler(rc.cancelKey, null); // before pc might return to idle pool
        alive = alive && bodyEOF && !pc.sawEOF && pc.wroteRequest() && replaced && tryPutIdleConn(trace);
        if (bodyEOF) {
            eofc.Send(/* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{});
        }
        alive = false;
        pc.t.CancelRequest(rc.req);
        alive = false;
        pc.t.cancelRequest(rc.cancelKey, rc.req.Context().Err());
        alive = false;
        testHookReadLoopBeforeNextRead();
    }
});

private static void readLoopPeekFailLocked(this ptr<persistConn> _addr_pc, error peekErr) {
    ref persistConn pc = ref _addr_pc.val;

    if (pc.closed != null) {
        return ;
    }
    {
        var n = pc.br.Buffered();

        if (n > 0) {
            var (buf, _) = pc.br.Peek(n);
            if (is408Message(buf)) {
                pc.closeLocked(errServerClosedIdle);
                return ;
            }
            else
 {
                log.Printf("Unsolicited response received on idle HTTP channel starting with %q; err=%v", buf, peekErr);
            }
        }
    }
    if (peekErr == io.EOF) { 
        // common case.
        pc.closeLocked(errServerClosedIdle);
    }
    else
 {
        pc.closeLocked(fmt.Errorf("readLoopPeekFailLocked: %v", peekErr));
    }
}

// is408Message reports whether buf has the prefix of an
// HTTP 408 Request Timeout response.
// See golang.org/issue/32310.
private static bool is408Message(slice<byte> buf) {
    if (len(buf) < len("HTTP/1.x 408")) {
        return false;
    }
    if (string(buf[..(int)7]) != "HTTP/1.") {
        return false;
    }
    return string(buf[(int)8..(int)12]) == " 408";
}

// readResponse reads an HTTP response (or two, in the case of "Expect:
// 100-continue") from the server. It returns the final non-100 one.
// trace is optional.
private static (ptr<Response>, error) readResponse(this ptr<persistConn> _addr_pc, requestAndChan rc, ptr<httptrace.ClientTrace> _addr_trace) {
    ptr<Response> resp = default!;
    error err = default!;
    ref persistConn pc = ref _addr_pc.val;
    ref httptrace.ClientTrace trace = ref _addr_trace.val;

    if (trace != null && trace.GotFirstResponseByte != null) {
        {
            var (peek, err) = pc.br.Peek(1);

            if (err == null && len(peek) == 1) {
                trace.GotFirstResponseByte();
            }

        }
    }
    nint num1xx = 0; // number of informational 1xx headers received
    const nint max1xxResponses = 5; // arbitrary bound on number of informational responses

 // arbitrary bound on number of informational responses

    var continueCh = rc.continueCh;
    while (true) {
        resp, err = ReadResponse(pc.br, rc.req);
        if (err != null) {
            return ;
        }
        var resCode = resp.StatusCode;
        if (continueCh != null) {
            if (resCode == 100) {
                if (trace != null && trace.Got100Continue != null) {
                    trace.Got100Continue();
                }
                continueCh.Send(/* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{});
                continueCh = null;
            }
            else if (resCode >= 200) {
                close(continueCh);
                continueCh = null;
            }
        }
        nint is1xx = 100 <= resCode && resCode <= 199; 
        // treat 101 as a terminal status, see issue 26161
        var is1xxNonTerminal = is1xx && resCode != StatusSwitchingProtocols;
        if (is1xxNonTerminal) {
            num1xx++;
            if (num1xx > max1xxResponses) {
                return (_addr_null!, error.As(errors.New("net/http: too many 1xx informational responses"))!);
            }
            pc.readLimit = pc.maxHeaderResponseSize(); // reset the limit
            if (trace != null && trace.Got1xxResponse != null) {
                {
                    var err = trace.Got1xxResponse(resCode, textproto.MIMEHeader(resp.Header));

                    if (err != null) {
                        return (_addr_null!, error.As(err)!);
                    }

                }
            }
            continue;
        }
        break;
    }
    if (resp.isProtocolSwitch()) {
        resp.Body = newReadWriteCloserBody(_addr_pc.br, pc.conn);
    }
    resp.TLS = pc.tlsState;
    return ;
}

// waitForContinue returns the function to block until
// any response, timeout or connection close. After any of them,
// the function returns a bool which indicates if the body should be sent.
private static Func<bool> waitForContinue(this ptr<persistConn> _addr_pc, channel<object> continueCh) => func((defer, _, _) => {
    ref persistConn pc = ref _addr_pc.val;

    if (continueCh == null) {
        return null;
    }
    return () => {
        var timer = time.NewTimer(pc.t.ExpectContinueTimeout);
        defer(timer.Stop());

        return ok;
        return true;
        return false;
    };
});

private static io.ReadWriteCloser newReadWriteCloserBody(ptr<bufio.Reader> _addr_br, io.ReadWriteCloser rwc) {
    ref bufio.Reader br = ref _addr_br.val;

    ptr<readWriteCloserBody> body = addr(new readWriteCloserBody(ReadWriteCloser:rwc));
    if (br.Buffered() != 0) {
        body.br = br;
    }
    return body;
}

// readWriteCloserBody is the Response.Body type used when we want to
// give users write access to the Body through the underlying
// connection (TCP, unless using custom dialers). This is then
// the concrete type for a Response.Body on the 101 Switching
// Protocols response, as used by WebSockets, h2c, etc.
private partial struct readWriteCloserBody : io.ReadWriteCloser {
    public incomparable _;
    public ptr<bufio.Reader> br; // used until empty
    public ref io.ReadWriteCloser ReadWriteCloser => ref ReadWriteCloser_val;
}

private static (nint, error) Read(this ptr<readWriteCloserBody> _addr_b, slice<byte> p) {
    nint n = default;
    error err = default!;
    ref readWriteCloserBody b = ref _addr_b.val;

    if (b.br != null) {
        {
            var n = b.br.Buffered();

            if (len(p) > n) {
                p = p[..(int)n];
            }

        }
        n, err = b.br.Read(p);
        if (b.br.Buffered() == 0) {
            b.br = null;
        }
        return (n, error.As(err)!);
    }
    return b.ReadWriteCloser.Read(p);
}

// nothingWrittenError wraps a write errors which ended up writing zero bytes.
private partial struct nothingWrittenError : error {
    public error error;
}

private static void writeLoop(this ptr<persistConn> _addr_pc) => func((defer, _, _) => {
    ref persistConn pc = ref _addr_pc.val;

    defer(close(pc.writeLoopDone));
    while (true) {
        var startBytesWritten = pc.nwrite;
        var err = wr.req.Request.write(pc.bw, pc.isProxy, wr.req.extra, pc.waitForContinue(wr.continueCh));
        {
            requestBodyReadError (bre, ok) = err._<requestBodyReadError>();

            if (ok) {
                err = bre.error; 
                // Errors reading from the user's
                // Request.Body are high priority.
                // Set it here before sending on the
                // channels below or calling
                // pc.close() which tears down
                // connections and causes other
                // errors.
                wr.req.setError(err);
            }

        }
        if (err == null) {
            err = pc.bw.Flush();
        }
        if (err != null) {
            if (pc.nwrite == startBytesWritten) {
                err = new nothingWrittenError(err);
            }
        }
        pc.writeErrCh.Send(err); // to the body reader, which might recycle us
        wr.ch.Send(err); // to the roundTrip function
        if (err != null) {
            pc.close(err);
            return ;
        }
        return ;
    }
});

// maxWriteWaitBeforeConnReuse is how long the a Transport RoundTrip
// will wait to see the Request's Body.Write result after getting a
// response from the server. See comments in (*persistConn).wroteRequest.
private static readonly nint maxWriteWaitBeforeConnReuse = 50 * time.Millisecond;

// wroteRequest is a check before recycling a connection that the previous write
// (from writeLoop above) happened and was successful.


// wroteRequest is a check before recycling a connection that the previous write
// (from writeLoop above) happened and was successful.
private static bool wroteRequest(this ptr<persistConn> _addr_pc) => func((defer, _, _) => {
    ref persistConn pc = ref _addr_pc.val;

    return err == null;
    var t = time.NewTimer(maxWriteWaitBeforeConnReuse);
    defer(t.Stop());
    return err == null;
    return false;
});

// responseAndError is how the goroutine reading from an HTTP/1 server
// communicates with the goroutine doing the RoundTrip.
private partial struct responseAndError {
    public incomparable _;
    public ptr<Response> res; // else use this response (see res method)
    public error err;
}

private partial struct requestAndChan {
    public incomparable _;
    public ptr<Request> req;
    public cancelKey cancelKey;
    public channel<responseAndError> ch; // unbuffered; always send in select on callerGone

// whether the Transport (as opposed to the user client code)
// added the Accept-Encoding gzip header. If the Transport
// set it, only then do we transparently decode the gzip.
    public bool addedGzip; // Optional blocking chan for Expect: 100-continue (for send).
// If the request has an "Expect: 100-continue" header and
// the server responds 100 Continue, readLoop send a value
// to writeLoop via this chan.
    public channel<object> continueCh;
    public channel<object> callerGone; // closed when roundTrip caller has returned
}

// A writeRequest is sent by the readLoop's goroutine to the
// writeLoop's goroutine to write a request while the read loop
// concurrently waits on both the write response and the server's
// reply.
private partial struct writeRequest {
    public ptr<transportRequest> req;
    public channel<error> ch; // Optional blocking chan for Expect: 100-continue (for receive).
// If not nil, writeLoop blocks sending request body until
// it receives from this chan.
    public channel<object> continueCh;
}

private partial struct httpError {
    public @string err;
    public bool timeout;
}

private static @string Error(this ptr<httpError> _addr_e) {
    ref httpError e = ref _addr_e.val;

    return e.err;
}
private static bool Timeout(this ptr<httpError> _addr_e) {
    ref httpError e = ref _addr_e.val;

    return e.timeout;
}
private static bool Temporary(this ptr<httpError> _addr_e) {
    ref httpError e = ref _addr_e.val;

    return true;
}

private static error errTimeout = error.As(addr(new httpError(err:"net/http: timeout awaiting response headers",timeout:true)))!;

// errRequestCanceled is set to be identical to the one from h2 to facilitate
// testing.
private static var errRequestCanceled = http2errRequestCanceled;
private static var errRequestCanceledConn = errors.New("net/http: request canceled while waiting for connection"); // TODO: unify?

private static void nop() {
}

// testHooks. Always non-nil.
private static var testHookEnterRoundTrip = nop;private static var testHookWaitResLoop = nop;private static var testHookRoundTripRetried = nop;private static var testHookPrePendingDial = nop;private static var testHookPostPendingDial = nop;private static sync.Locker testHookMu = new fakeLocker();private static var testHookReadLoopBeforeNextRead = nop;

private static (ptr<Response>, error) roundTrip(this ptr<persistConn> _addr_pc, ptr<transportRequest> _addr_req) => func((defer, panic, _) => {
    ptr<Response> resp = default!;
    error err = default!;
    ref persistConn pc = ref _addr_pc.val;
    ref transportRequest req = ref _addr_req.val;

    testHookEnterRoundTrip();
    if (!pc.t.replaceReqCanceler(req.cancelKey, pc.cancelRequest)) {
        pc.t.putOrCloseIdleConn(pc);
        return (_addr_null!, error.As(errRequestCanceled)!);
    }
    pc.mu.Lock();
    pc.numExpectedResponses++;
    var headerFn = pc.mutateHeaderFunc;
    pc.mu.Unlock();

    if (headerFn != null) {
        headerFn(req.extraHeaders());
    }
    var requestedGzip = false;
    if (!pc.t.DisableCompression && req.Header.Get("Accept-Encoding") == "" && req.Header.Get("Range") == "" && req.Method != "HEAD") { 
        // Request gzip only, not deflate. Deflate is ambiguous and
        // not as universally supported anyway.
        // See: https://zlib.net/zlib_faq.html#faq39
        //
        // Note that we don't request this for HEAD requests,
        // due to a bug in nginx:
        //   https://trac.nginx.org/nginx/ticket/358
        //   https://golang.org/issue/5522
        //
        // We don't request gzip if the request is for a range, since
        // auto-decoding a portion of a gzipped document will just fail
        // anyway. See https://golang.org/issue/8923
        requestedGzip = true;
        req.extraHeaders().Set("Accept-Encoding", "gzip");
    }
    channel<object> continueCh = default;
    if (req.ProtoAtLeast(1, 1) && req.Body != null && req.expectsContinue()) {
        continueCh = make_channel<object>(1);
    }
    if (pc.t.DisableKeepAlives && !req.wantsClose() && !isProtocolSwitchHeader(req.Header)) {
        req.extraHeaders().Set("Connection", "close");
    }
    var gone = make_channel<object>();
    defer(close(gone));

    defer(() => {
        if (err != null) {
            pc.t.setReqCanceler(req.cancelKey, null);
        }
    }());

    const var debugRoundTrip = false; 

    // Write the request concurrently with waiting for a response,
    // in case the server decides to reply before reading our full
    // request body.
 

    // Write the request concurrently with waiting for a response,
    // in case the server decides to reply before reading our full
    // request body.
    var startBytesWritten = pc.nwrite;
    var writeErrCh = make_channel<error>(1);
    pc.writech.Send(new writeRequest(req,writeErrCh,continueCh));

    var resc = make_channel<responseAndError>();
    pc.reqch.Send(new requestAndChan(req:req.Request,cancelKey:req.cancelKey,ch:resc,addedGzip:requestedGzip,continueCh:continueCh,callerGone:gone,));

    channel<time.Time> respHeaderTimer = default;
    var cancelChan = req.Request.Cancel;
    var ctxDoneChan = req.Context().Done();
    var pcClosed = pc.closech;
    var canceled = false;
    while (true) {
        testHookWaitResLoop();
        if (debugRoundTrip) {
            req.logf("writeErrCh resv: %T/%#v", err, err);
        }
        if (err != null) {
            pc.close(fmt.Errorf("write error: %v", err));
            return (_addr_null!, error.As(pc.mapRoundTripError(req, startBytesWritten, err))!);
        }
        {
            var d = pc.t.ResponseHeaderTimeout;

            if (d > 0) {
                if (debugRoundTrip) {
                    req.logf("starting timer for %v", d);
                }
                var timer = time.NewTimer(d);
                defer(timer.Stop()); // prevent leaks
                respHeaderTimer = timer.C;
            }

        }
        pcClosed = null;
        if (canceled || pc.t.replaceReqCanceler(req.cancelKey, null)) {
            if (debugRoundTrip) {
                req.logf("closech recv: %T %#v", pc.closed, pc.closed);
            }
            return (_addr_null!, error.As(pc.mapRoundTripError(req, startBytesWritten, pc.closed))!);
        }
        if (debugRoundTrip) {
            req.logf("timeout waiting for response headers.");
        }
        pc.close(errTimeout);
        return (_addr_null!, error.As(errTimeout)!);
        if ((re.res == null) == (re.err == null)) {
            panic(fmt.Sprintf("internal error: exactly one of res or err should be set; nil=%v", re.res == null));
        }
        if (debugRoundTrip) {
            req.logf("resc recv: %p, %T/%#v", re.res, re.err, re.err);
        }
        if (re.err != null) {
            return (_addr_null!, error.As(pc.mapRoundTripError(req, startBytesWritten, re.err))!);
        }
        return (_addr_re.res!, error.As(null!)!);
        canceled = pc.t.cancelRequest(req.cancelKey, errRequestCanceled);
        cancelChan = null;
        canceled = pc.t.cancelRequest(req.cancelKey, req.Context().Err());
        cancelChan = null;
        ctxDoneChan = null;
    }
});

// tLogKey is a context WithValue key for test debugging contexts containing
// a t.Logf func. See export_test.go's Request.WithT method.
private partial struct tLogKey {
}

private static void logf(this ptr<transportRequest> _addr_tr, @string format, params object[] args) {
    args = args.Clone();
    ref transportRequest tr = ref _addr_tr.val;

    {
        Action<@string, object> (logf, ok) = tr.Request.Context().Value(new tLogKey())._<Action<@string, object>>();

        if (ok) {
            logf(time.Now().Format(time.RFC3339Nano) + ": " + format, args);
        }
    }
}

// markReused marks this connection as having been successfully used for a
// request and response.
private static void markReused(this ptr<persistConn> _addr_pc) {
    ref persistConn pc = ref _addr_pc.val;

    pc.mu.Lock();
    pc.reused = true;
    pc.mu.Unlock();
}

// close closes the underlying TCP connection and closes
// the pc.closech channel.
//
// The provided err is only for testing and debugging; in normal
// circumstances it should never be seen by users.
private static void close(this ptr<persistConn> _addr_pc, error err) => func((defer, _, _) => {
    ref persistConn pc = ref _addr_pc.val;

    pc.mu.Lock();
    defer(pc.mu.Unlock());
    pc.closeLocked(err);
});

private static void closeLocked(this ptr<persistConn> _addr_pc, error err) => func((_, panic, _) => {
    ref persistConn pc = ref _addr_pc.val;

    if (err == null) {
        panic("nil error");
    }
    pc.broken = true;
    if (pc.closed == null) {
        pc.closed = err;
        pc.t.decConnsPerHost(pc.cacheKey); 
        // Close HTTP/1 (pc.alt == nil) connection.
        // HTTP/2 closes its connection itself.
        if (pc.alt == null) {
            if (err != errCallerOwnsConn) {
                pc.conn.Close();
            }
            close(pc.closech);
        }
    }
    pc.mutateHeaderFunc = null;
});

private static map portMap = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, @string>{"http":"80","https":"443","socks5":"1080",};

// canonicalAddr returns url.Host but always with a ":port" suffix
private static @string canonicalAddr(ptr<url.URL> _addr_url) {
    ref url.URL url = ref _addr_url.val;

    var addr = url.Hostname();
    {
        var (v, err) = idnaASCII(addr);

        if (err == null) {
            addr = v;
        }
    }
    var port = url.Port();
    if (port == "") {
        port = portMap[url.Scheme];
    }
    return net.JoinHostPort(addr, port);
}

// bodyEOFSignal is used by the HTTP/1 transport when reading response
// bodies to make sure we see the end of a response body before
// proceeding and reading on the connection again.
//
// It wraps a ReadCloser but runs fn (if non-nil) at most
// once, right before its final (error-producing) Read or Close call
// returns. fn should return the new error to return from Read or Close.
//
// If earlyCloseFn is non-nil and Close is called before io.EOF is
// seen, earlyCloseFn is called instead of fn, and its return value is
// the return value from Close.
private partial struct bodyEOFSignal {
    public io.ReadCloser body;
    public sync.Mutex mu; // guards following 4 fields
    public bool closed; // whether Close has been called
    public error rerr; // sticky Read error
    public Func<error, error> fn; // err will be nil on Read io.EOF
    public Func<error> earlyCloseFn; // optional alt Close func used if io.EOF not seen
}

private static var errReadOnClosedResBody = errors.New("http: read on closed response body");

private static (nint, error) Read(this ptr<bodyEOFSignal> _addr_es, slice<byte> p) => func((defer, _, _) => {
    nint n = default;
    error err = default!;
    ref bodyEOFSignal es = ref _addr_es.val;

    es.mu.Lock();
    var closed = es.closed;
    var rerr = es.rerr;
    es.mu.Unlock();
    if (closed) {
        return (0, error.As(errReadOnClosedResBody)!);
    }
    if (rerr != null) {
        return (0, error.As(rerr)!);
    }
    n, err = es.body.Read(p);
    if (err != null) {
        es.mu.Lock();
        defer(es.mu.Unlock());
        if (es.rerr == null) {
            es.rerr = err;
        }
        err = es.condfn(err);
    }
    return ;
});

private static error Close(this ptr<bodyEOFSignal> _addr_es) => func((defer, _, _) => {
    ref bodyEOFSignal es = ref _addr_es.val;

    es.mu.Lock();
    defer(es.mu.Unlock());
    if (es.closed) {
        return error.As(null!)!;
    }
    es.closed = true;
    if (es.earlyCloseFn != null && es.rerr != io.EOF) {
        return error.As(es.earlyCloseFn())!;
    }
    var err = es.body.Close();
    return error.As(es.condfn(err))!;
});

// caller must hold es.mu.
private static error condfn(this ptr<bodyEOFSignal> _addr_es, error err) {
    ref bodyEOFSignal es = ref _addr_es.val;

    if (es.fn == null) {
        return error.As(err)!;
    }
    err = es.fn(err);
    es.fn = null;
    return error.As(err)!;
}

// gzipReader wraps a response body so it can lazily
// call gzip.NewReader on the first call to Read
private partial struct gzipReader {
    public incomparable _;
    public ptr<bodyEOFSignal> body; // underlying HTTP/1 response body framing
    public ptr<gzip.Reader> zr; // lazily-initialized gzip reader
    public error zerr; // any error from gzip.NewReader; sticky
}

private static (nint, error) Read(this ptr<gzipReader> _addr_gz, slice<byte> p) {
    nint n = default;
    error err = default!;
    ref gzipReader gz = ref _addr_gz.val;

    if (gz.zr == null) {
        if (gz.zerr == null) {
            gz.zr, gz.zerr = gzip.NewReader(gz.body);
        }
        if (gz.zerr != null) {
            return (0, error.As(gz.zerr)!);
        }
    }
    gz.body.mu.Lock();
    if (gz.body.closed) {
        err = errReadOnClosedResBody;
    }
    gz.body.mu.Unlock();

    if (err != null) {
        return (0, error.As(err)!);
    }
    return gz.zr.Read(p);
}

private static error Close(this ptr<gzipReader> _addr_gz) {
    ref gzipReader gz = ref _addr_gz.val;

    return error.As(gz.body.Close())!;
}

private partial struct tlsHandshakeTimeoutError {
}

private static bool Timeout(this tlsHandshakeTimeoutError _p0) {
    return true;
}
private static bool Temporary(this tlsHandshakeTimeoutError _p0) {
    return true;
}
private static @string Error(this tlsHandshakeTimeoutError _p0) {
    return "net/http: TLS handshake timeout";
}

// fakeLocker is a sync.Locker which does nothing. It's used to guard
// test-only fields when not under test, to avoid runtime atomic
// overhead.
private partial struct fakeLocker {
}

private static void Lock(this fakeLocker _p0) {
}
private static void Unlock(this fakeLocker _p0) {
}

// cloneTLSConfig returns a shallow clone of cfg, or a new zero tls.Config if
// cfg is nil. This is safe to call even if cfg is in active use by a TLS
// client or server.
private static ptr<tls.Config> cloneTLSConfig(ptr<tls.Config> _addr_cfg) {
    ref tls.Config cfg = ref _addr_cfg.val;

    if (cfg == null) {
        return addr(new tls.Config());
    }
    return _addr_cfg.Clone()!;
}

private partial struct connLRU {
    public ptr<list.List> ll; // list.Element.Value type is of *persistConn
    public map<ptr<persistConn>, ptr<list.Element>> m;
}

// add adds pc to the head of the linked list.
private static void add(this ptr<connLRU> _addr_cl, ptr<persistConn> _addr_pc) => func((_, panic, _) => {
    ref connLRU cl = ref _addr_cl.val;
    ref persistConn pc = ref _addr_pc.val;

    if (cl.ll == null) {
        cl.ll = list.New();
        cl.m = make_map<ptr<persistConn>, ptr<list.Element>>();
    }
    var ele = cl.ll.PushFront(pc);
    {
        var (_, ok) = cl.m[pc];

        if (ok) {
            panic("persistConn was already in LRU");
        }
    }
    cl.m[pc] = ele;
});

private static ptr<persistConn> removeOldest(this ptr<connLRU> _addr_cl) {
    ref connLRU cl = ref _addr_cl.val;

    var ele = cl.ll.Back();
    ptr<persistConn> pc = ele.Value._<ptr<persistConn>>();
    cl.ll.Remove(ele);
    delete(cl.m, pc);
    return _addr_pc!;
}

// remove removes pc from cl.
private static void remove(this ptr<connLRU> _addr_cl, ptr<persistConn> _addr_pc) {
    ref connLRU cl = ref _addr_cl.val;
    ref persistConn pc = ref _addr_pc.val;

    {
        var (ele, ok) = cl.m[pc];

        if (ok) {
            cl.ll.Remove(ele);
            delete(cl.m, pc);
        }
    }
}

// len returns the number of items in the cache.
private static nint len(this ptr<connLRU> _addr_cl) {
    ref connLRU cl = ref _addr_cl.val;

    return len(cl.m);
}

} // end http_package
