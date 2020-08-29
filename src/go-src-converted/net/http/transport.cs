// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// HTTP client implementation. See RFC 2616.
//
// This is the low-level Transport implementation of RoundTripper.
// The high-level interface is in client.go.

// package http -- go2cs converted at 2020 August 29 08:33:59 UTC
// import "net/http" ==> using http = go.net.http_package
// Original source: C:\Go\src\net\http\transport.go
using bufio = go.bufio_package;
using gzip = go.compress.gzip_package;
using list = go.container.list_package;
using context = go.context_package;
using tls = go.crypto.tls_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using io = go.io_package;
using log = go.log_package;
using net = go.net_package;
using httptrace = go.net.http.httptrace_package;
using url = go.net.url_package;
using os = go.os_package;
using strings = go.strings_package;
using sync = go.sync_package;
using atomic = go.sync.atomic_package;
using time = go.time_package;

using httplex = go.golang_org.x.net.lex.httplex_package;
using proxy = go.golang_org.x.net.proxy_package;
using static go.builtin;
using System;
using System.Threading;

namespace go {
namespace net
{
    public static partial class http_package
    {
        // DefaultTransport is the default implementation of Transport and is
        // used by DefaultClient. It establishes network connections as needed
        // and caches them for reuse by subsequent calls. It uses HTTP proxies
        // as directed by the $HTTP_PROXY and $NO_PROXY (or $http_proxy and
        // $no_proxy) environment variables.
        public static RoundTripper DefaultTransport = ref new Transport(Proxy:ProxyFromEnvironment,DialContext:(&net.Dialer{Timeout:30*time.Second,KeepAlive:30*time.Second,DualStack:true,}).DialContext,MaxIdleConns:100,IdleConnTimeout:90*time.Second,TLSHandshakeTimeout:10*time.Second,ExpectContinueTimeout:1*time.Second,);

        // DefaultMaxIdleConnsPerHost is the default value of Transport's
        // MaxIdleConnsPerHost.
        public static readonly long DefaultMaxIdleConnsPerHost = 2L;

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
        // The Transport will send CONNECT requests to a proxy for its own use
        // when processing HTTPS requests, but Transport should generally not
        // be used to send a CONNECT request. That is, the Request passed to
        // the RoundTrip method should not have a Method of "CONNECT", as Go's
        // HTTP/1.x implementation does not support full-duplex request bodies
        // being written while the response body is streamed. Go's HTTP/2
        // implementation does support full duplex, but many CONNECT proxies speak
        // HTTP/1.x.


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
        // The Transport will send CONNECT requests to a proxy for its own use
        // when processing HTTPS requests, but Transport should generally not
        // be used to send a CONNECT request. That is, the Request passed to
        // the RoundTrip method should not have a Method of "CONNECT", as Go's
        // HTTP/1.x implementation does not support full-duplex request bodies
        // being written while the response body is streamed. Go's HTTP/2
        // implementation does support full duplex, but many CONNECT proxies speak
        // HTTP/1.x.
        public partial struct Transport
        {
            public sync.Mutex idleMu;
            public bool wantIdle; // user has requested to close all idle conns
            public map<connectMethodKey, slice<ref persistConn>> idleConn; // most recently used at end
            public map<connectMethodKey, channel<ref persistConn>> idleConnCh;
            public connLRU idleLRU;
            public sync.Mutex reqMu;
            public map<ref Request, Action<error>> reqCanceler;
            public sync.Mutex altMu; // guards changing altProto only
            public atomic.Value altProto; // of nil or map[string]RoundTripper, key is URI scheme

// Proxy specifies a function to return a proxy for a given
// Request. If the function returns a non-nil error, the
// request is aborted with the provided error.
//
// The proxy type is determined by the URL scheme. "http"
// and "socks5" are supported. If the scheme is empty,
// "http" is assumed.
//
// If Proxy is nil or returns a nil *URL, no proxy is used.
            public Func<ref Request, (ref url.URL, error)> Proxy; // DialContext specifies the dial function for creating unencrypted TCP connections.
// If DialContext is nil (and the deprecated Dial below is also nil),
// then the transport dials using package net.
            public Func<context.Context, @string, @string, (net.Conn, error)> DialContext; // Dial specifies the dial function for creating unencrypted TCP connections.
//
// Deprecated: Use DialContext instead, which allows the transport
// to cancel dials as soon as they are no longer needed.
// If both are set, DialContext takes priority.
            public Func<@string, @string, (net.Conn, error)> Dial; // DialTLS specifies an optional dial function for creating
// TLS connections for non-proxied HTTPS requests.
//
// If DialTLS is nil, Dial and TLSClientConfig are used.
//
// If DialTLS is set, the Dial hook is not used for HTTPS
// requests and the TLSClientConfig and TLSHandshakeTimeout
// are ignored. The returned net.Conn is assumed to already be
// past the TLS handshake.
            public Func<@string, @string, (net.Conn, error)> DialTLS; // TLSClientConfig specifies the TLS configuration to use with
// tls.Client.
// If nil, the default configuration is used.
// If non-nil, HTTP/2 support may not be enabled by default.
            public ptr<tls.Config> TLSClientConfig; // TLSHandshakeTimeout specifies the maximum amount of time waiting to
// wait for a TLS handshake. Zero means no timeout.
            public time.Duration TLSHandshakeTimeout; // DisableKeepAlives, if true, prevents re-use of TCP connections
// between different HTTP requests.
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
            public long MaxIdleConns; // MaxIdleConnsPerHost, if non-zero, controls the maximum idle
// (keep-alive) connections to keep per-host. If zero,
// DefaultMaxIdleConnsPerHost is used.
            public long MaxIdleConnsPerHost; // IdleConnTimeout is the maximum amount of time an idle
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
// alternate protocol (such as HTTP/2) after a TLS NPN/ALPN
// protocol negotiation. If Transport dials an TLS connection
// with a non-empty protocol name and TLSNextProto contains a
// map entry for that key (such as "h2"), then the func is
// called with the request's authority (such as "example.com"
// or "example.com:1234") and the TLS connection. The function
// must return a RoundTripper that then handles the request.
// If TLSNextProto is not nil, HTTP/2 support is not enabled
// automatically.
            public map<@string, Func<@string, ref tls.Conn, RoundTripper>> TLSNextProto; // ProxyConnectHeader optionally specifies headers to send to
// proxies during CONNECT requests.
            public Header ProxyConnectHeader; // MaxResponseHeaderBytes specifies a limit on how many
// response bytes are allowed in the server's response
// header.
//
// Zero means to use a default limit.
            public long MaxResponseHeaderBytes; // nextProtoOnce guards initialization of TLSNextProto and
// h2transport (via onceSetNextProtoDefaults)
            public sync.Once nextProtoOnce;
            public ptr<http2Transport> h2transport; // non-nil if http2 wired up

// TODO: tunable on max per-host TCP dials in flight (Issue 13957)
        }

        // onceSetNextProtoDefaults initializes TLSNextProto.
        // It must be called via t.nextProtoOnce.Do.
        private static void onceSetNextProtoDefaults(this ref Transport t)
        {
            if (strings.Contains(os.Getenv("GODEBUG"), "http2client=0"))
            {
                return;
            }
            if (t.TLSNextProto != null)
            { 
                // This is the documented way to disable http2 on a
                // Transport.
                return;
            }
            if (t.TLSClientConfig != null || t.Dial != null || t.DialTLS != null)
            { 
                // Be conservative and don't automatically enable
                // http2 if they've specified a custom TLS config or
                // custom dialers. Let them opt-in themselves via
                // http2.ConfigureTransport so we don't surprise them
                // by modifying their tls.Config. Issue 14275.
                return;
            }
            var (t2, err) = http2configureTransport(t);
            if (err != null)
            {
                log.Printf("Error enabling Transport HTTP/2 support: %v", err);
                return;
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

                if (limit1 != 0L && t2.MaxHeaderListSize == 0L)
                {
                    const long h2max = 1L << (int)(32L) - 1L;

                    if (limit1 >= h2max)
                    {
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
        // An error is returned if the value is a different form.
        //
        // A nil URL and nil error are returned if no proxy is defined in the
        // environment, or a proxy should not be used for the given request,
        // as defined by NO_PROXY.
        //
        // As a special case, if req.URL.Host is "localhost" (with or without
        // a port number), then a nil URL and nil error will be returned.
        public static (ref url.URL, error) ProxyFromEnvironment(ref Request req)
        {
            @string proxy = default;
            if (req.URL.Scheme == "https")
            {
                proxy = httpsProxyEnv.Get();
            }
            if (proxy == "")
            {
                proxy = httpProxyEnv.Get();
                if (proxy != "" && os.Getenv("REQUEST_METHOD") != "")
                {
                    return (null, errors.New("net/http: refusing to use HTTP_PROXY value in CGI environment; see golang.org/s/cgihttpproxy"));
                }
            }
            if (proxy == "")
            {
                return (null, null);
            }
            if (!useProxy(canonicalAddr(req.URL)))
            {
                return (null, null);
            }
            var (proxyURL, err) = url.Parse(proxy);
            if (err != null || (proxyURL.Scheme != "http" && proxyURL.Scheme != "https" && proxyURL.Scheme != "socks5"))
            { 
                // proxy was bogus. Try prepending "http://" to it and
                // see if that parses correctly. If not, we fall
                // through and complain about the original one.
                {
                    var proxyURL__prev2 = proxyURL;

                    (proxyURL, err) = url.Parse("http://" + proxy);

                    if (err == null)
                    {
                        return (proxyURL, null);
                    }

                    proxyURL = proxyURL__prev2;

                }

            }
            if (err != null)
            {
                return (null, fmt.Errorf("invalid proxy address %q: %v", proxy, err));
            }
            return (proxyURL, null);
        }

        // ProxyURL returns a proxy function (for use in a Transport)
        // that always returns the same URL.
        public static Func<ref Request, (ref url.URL, error)> ProxyURL(ref url.URL fixedURL)
        {
            return _p0 =>
            {
                return (fixedURL, null);
            }
;
        }

        // transportRequest is a wrapper around a *Request that adds
        // optional extra headers to write and stores any error to return
        // from roundTrip.
        private partial struct transportRequest
        {
            public ref Request Request => ref Request_ptr; // original request, not to be mutated
            public Header extra; // extra headers to write, or nil
            public ptr<httptrace.ClientTrace> trace; // optional

            public sync.Mutex mu; // guards err
            public error err; // first setError value for mapRoundTripError to consider
        }

        private static Header extraHeaders(this ref transportRequest tr)
        {
            if (tr.extra == null)
            {
                tr.extra = make(Header);
            }
            return tr.extra;
        }

        private static void setError(this ref transportRequest tr, error err)
        {
            tr.mu.Lock();
            if (tr.err == null)
            {
                tr.err = err;
            }
            tr.mu.Unlock();
        }

        // RoundTrip implements the RoundTripper interface.
        //
        // For higher-level HTTP client support (such as handling of cookies
        // and redirects), see Get, Post, and the Client type.
        private static (ref Response, error) RoundTrip(this ref Transport t, ref Request req)
        {
            t.nextProtoOnce.Do(t.onceSetNextProtoDefaults);
            var ctx = req.Context();
            var trace = httptrace.ContextClientTrace(ctx);

            if (req.URL == null)
            {
                req.closeBody();
                return (null, errors.New("http: nil Request.URL"));
            }
            if (req.Header == null)
            {
                req.closeBody();
                return (null, errors.New("http: nil Request.Header"));
            }
            var scheme = req.URL.Scheme;
            var isHTTP = scheme == "http" || scheme == "https";
            if (isHTTP)
            {
                foreach (var (k, vv) in req.Header)
                {
                    if (!httplex.ValidHeaderFieldName(k))
                    {
                        return (null, fmt.Errorf("net/http: invalid header field name %q", k));
                    }
                    foreach (var (_, v) in vv)
                    {
                        if (!httplex.ValidHeaderFieldValue(v))
                        {
                            return (null, fmt.Errorf("net/http: invalid header field value %q for key %v", v, k));
                        }
                    }
                }
            }
            map<@string, RoundTripper> (altProto, _) = t.altProto.Load()._<map<@string, RoundTripper>>();
            {
                var altRT = altProto[scheme];

                if (altRT != null)
                {
                    {
                        var resp__prev2 = resp;

                        var (resp, err) = altRT.RoundTrip(req);

                        if (err != ErrSkipAltProtocol)
                        {
                            return (resp, err);
                        }

                        resp = resp__prev2;

                    }
                }

            }
            if (!isHTTP)
            {
                req.closeBody();
                return (null, ref new badStringError("unsupported protocol scheme",scheme));
            }
            if (req.Method != "" && !validMethod(req.Method))
            {
                return (null, fmt.Errorf("net/http: invalid method %q", req.Method));
            }
            if (req.URL.Host == "")
            {
                req.closeBody();
                return (null, errors.New("http: no Host in request URL"));
            }
            while (true)
            { 
                // treq gets modified by roundTrip, so we need to recreate for each retry.
                transportRequest treq = ref new transportRequest(Request:req,trace:trace);
                var (cm, err) = t.connectMethodForRequest(treq);
                if (err != null)
                {
                    req.closeBody();
                    return (null, err);
                } 

                // Get the cached or newly-created connection to either the
                // host (for http or https), the http proxy, or the http proxy
                // pre-CONNECTed to https server. In any case, we'll be ready
                // to send it requests.
                var (pconn, err) = t.getConn(treq, cm);
                if (err != null)
                {
                    t.setReqCanceler(req, null);
                    req.closeBody();
                    return (null, err);
                }
                ref Response resp = default;
                if (pconn.alt != null)
                { 
                    // HTTP/2 path.
                    t.setReqCanceler(req, null); // not cancelable with CancelRequest
                    resp, err = pconn.alt.RoundTrip(req);
                }
                else
                {
                    resp, err = pconn.roundTrip(treq);
                }
                if (err == null)
                {
                    return (resp, null);
                }
                if (!pconn.shouldRetryRequest(req, err))
                { 
                    // Issue 16465: return underlying net.Conn.Read error from peek,
                    // as we've historically done.
                    {
                        transportReadFromServerError (e, ok) = err._<transportReadFromServerError>();

                        if (ok)
                        {
                            err = e.err;
                        }

                    }
                    return (null, err);
                }
                testHookRoundTripRetried(); 

                // Rewind the body if we're able to.  (HTTP/2 does this itself so we only
                // need to do it for HTTP/1.1 connections.)
                if (req.GetBody != null && pconn.alt == null)
                {
                    var newReq = req.Value;
                    error err = default;
                    newReq.Body, err = req.GetBody();
                    if (err != null)
                    {
                        return (null, err);
                    }
                    req = ref newReq;
                }
            }

        }

        // shouldRetryRequest reports whether we should retry sending a failed
        // HTTP request on a new connection. The non-nil input error is the
        // error from roundTrip.
        private static bool shouldRetryRequest(this ref persistConn pc, ref Request req, error err)
        {
            if (http2isNoCachedConnError(err))
            { 
                // Issue 16582: if the user started a bunch of
                // requests at once, they can all pick the same conn
                // and violate the server's max concurrent streams.
                // Instead, match the HTTP/1 behavior for now and dial
                // again to get a new TCP connection, rather than failing
                // this request.
                return true;
            }
            if (err == errMissingHost)
            { 
                // User error.
                return false;
            }
            if (!pc.isReused())
            { 
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

                if (ok)
                { 
                    // We never wrote anything, so it's safe to retry, if there's no body or we
                    // can "rewind" the body with GetBody.
                    return req.outgoingLength() == 0L || req.GetBody != null;
                }

            }
            if (!req.isReplayable())
            { 
                // Don't retry non-idempotent requests.
                return false;
            }
            {
                (_, ok) = err._<transportReadFromServerError>();

                if (ok)
                { 
                    // We got some non-EOF net.Conn.Read failure reading
                    // the 1st response byte from the server.
                    return true;
                }

            }
            if (err == errServerClosedIdle)
            { 
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
        private static void RegisterProtocol(this ref Transport _t, @string scheme, RoundTripper rt) => func(_t, (ref Transport t, Defer defer, Panic panic, Recover _) =>
        {
            t.altMu.Lock();
            defer(t.altMu.Unlock());
            map<@string, RoundTripper> (oldMap, _) = t.altProto.Load()._<map<@string, RoundTripper>>();
            {
                var (_, exists) = oldMap[scheme];

                if (exists)
                {
                    panic("protocol " + scheme + " already registered");
                }

            }
            var newMap = make_map<@string, RoundTripper>();
            foreach (var (k, v) in oldMap)
            {
                newMap[k] = v;
            }
            newMap[scheme] = rt;
            t.altProto.Store(newMap);
        });

        // CloseIdleConnections closes any connections which were previously
        // connected from previous requests but are now sitting idle in
        // a "keep-alive" state. It does not interrupt any connections currently
        // in use.
        private static void CloseIdleConnections(this ref Transport t)
        {
            t.nextProtoOnce.Do(t.onceSetNextProtoDefaults);
            t.idleMu.Lock();
            var m = t.idleConn;
            t.idleConn = null;
            t.idleConnCh = null;
            t.wantIdle = true;
            t.idleLRU = new connLRU();
            t.idleMu.Unlock();
            foreach (var (_, conns) in m)
            {
                foreach (var (_, pconn) in conns)
                {
                    pconn.close(errCloseIdleConns);
                }
            }
            {
                var t2 = t.h2transport;

                if (t2 != null)
                {
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
        private static void CancelRequest(this ref Transport t, ref Request req)
        {
            t.cancelRequest(req, errRequestCanceled);
        }

        // Cancel an in-flight request, recording the error value.
        private static void cancelRequest(this ref Transport t, ref Request req, error err)
        {
            t.reqMu.Lock();
            var cancel = t.reqCanceler[req];
            delete(t.reqCanceler, req);
            t.reqMu.Unlock();
            if (cancel != null)
            {
                cancel(err);
            }
        }

        //
        // Private implementation past this point.
        //

        private static envOnce httpProxyEnv = ref new envOnce(names:[]string{"HTTP_PROXY","http_proxy"},);        private static envOnce httpsProxyEnv = ref new envOnce(names:[]string{"HTTPS_PROXY","https_proxy"},);        private static envOnce noProxyEnv = ref new envOnce(names:[]string{"NO_PROXY","no_proxy"},);

        // envOnce looks up an environment variable (optionally by multiple
        // names) once. It mitigates expensive lookups on some platforms
        // (e.g. Windows).
        private partial struct envOnce
        {
            public slice<@string> names;
            public sync.Once once;
            public @string val;
        }

        private static @string Get(this ref envOnce e)
        {
            e.once.Do(e.init);
            return e.val;
        }

        private static void init(this ref envOnce e)
        {
            foreach (var (_, n) in e.names)
            {
                e.val = os.Getenv(n);
                if (e.val != "")
                {
                    return;
                }
            }
        }

        // reset is used by tests
        private static void reset(this ref envOnce e)
        {
            e.once = new sync.Once();
            e.val = "";
        }

        private static (connectMethod, error) connectMethodForRequest(this ref Transport t, ref transportRequest treq)
        {
            {
                var port__prev1 = port;

                var port = treq.URL.Port();

                if (!validPort(port))
                {
                    return (cm, fmt.Errorf("invalid URL port %q", port));
                }

                port = port__prev1;

            }
            cm.targetScheme = treq.URL.Scheme;
            cm.targetAddr = canonicalAddr(treq.URL);
            if (t.Proxy != null)
            {
                cm.proxyURL, err = t.Proxy(treq.Request);
                if (err == null && cm.proxyURL != null)
                {
                    {
                        var port__prev3 = port;

                        port = cm.proxyURL.Port();

                        if (!validPort(port))
                        {
                            return (cm, fmt.Errorf("invalid proxy URL port %q", port));
                        }

                        port = port__prev3;

                    }
                }
            }
            return (cm, err);
        }

        // proxyAuth returns the Proxy-Authorization header to set
        // on requests, if applicable.
        private static @string proxyAuth(this ref connectMethod cm)
        {
            if (cm.proxyURL == null)
            {
                return "";
            }
            {
                var u = cm.proxyURL.User;

                if (u != null)
                {
                    var username = u.Username();
                    var (password, _) = u.Password();
                    return "Basic " + basicAuth(username, password);
                }

            }
            return "";
        }

        // error values for debugging and testing, not seen by users.
        private static var errKeepAlivesDisabled = errors.New("http: putIdleConn: keep alives disabled");        private static var errConnBroken = errors.New("http: putIdleConn: connection is in bad state");        private static var errWantIdle = errors.New("http: putIdleConn: CloseIdleConnections was called");        private static var errTooManyIdle = errors.New("http: putIdleConn: too many idle connections");        private static var errTooManyIdleHost = errors.New("http: putIdleConn: too many idle connections for host");        private static var errCloseIdleConns = errors.New("http: CloseIdleConnections called");        private static var errReadLoopExiting = errors.New("http: persistConn.readLoop exiting");        private static var errIdleConnTimeout = errors.New("http: idle connection timeout");        private static var errNotCachingH2Conn = errors.New("http: not caching alternate protocol's connections");        private static var errServerClosedIdle = errors.New("http: server closed idle connection");

        // transportReadFromServerError is used by Transport.readLoop when the
        // 1 byte peek read fails and we're actually anticipating a response.
        // Usually this is just due to the inherent keep-alive shut down race,
        // where the server closed the connection at the same time the client
        // wrote. The underlying err field is usually io.EOF or some
        // ECONNRESET sort of thing which varies by platform. But it might be
        // the user's custom net.Conn.Read error too, so we carry it along for
        // them to return from Transport.RoundTrip.
        private partial struct transportReadFromServerError
        {
            public error err;
        }

        private static @string Error(this transportReadFromServerError e)
        {
            return fmt.Sprintf("net/http: Transport failed to read from server: %v", e.err);
        }

        private static void putOrCloseIdleConn(this ref Transport t, ref persistConn pconn)
        {
            {
                var err = t.tryPutIdleConn(pconn);

                if (err != null)
                {
                    pconn.close(err);
                }

            }
        }

        private static long maxIdleConnsPerHost(this ref Transport t)
        {
            {
                var v = t.MaxIdleConnsPerHost;

                if (v != 0L)
                {
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
        private static error tryPutIdleConn(this ref Transport _t, ref persistConn _pconn) => func(_t, _pconn, (ref Transport t, ref persistConn pconn, Defer defer, Panic _, Recover __) =>
        {
            if (t.DisableKeepAlives || t.MaxIdleConnsPerHost < 0L)
            {
                return error.As(errKeepAlivesDisabled);
            }
            if (pconn.isBroken())
            {
                return error.As(errConnBroken);
            }
            if (pconn.alt != null)
            {
                return error.As(errNotCachingH2Conn);
            }
            pconn.markReused();
            var key = pconn.cacheKey;

            t.idleMu.Lock();
            defer(t.idleMu.Unlock());

            var waitingDialer = t.idleConnCh[key];
            return error.As(null);
            if (waitingDialer != null)
            { 
                // They had populated this, but their dial won
                // first, so we can clean up this map entry.
                delete(t.idleConnCh, key);
            }
            if (t.wantIdle)
            {
                return error.As(errWantIdle);
            }
            if (t.idleConn == null)
            {
                t.idleConn = make_map<connectMethodKey, slice<ref persistConn>>();
            }
            var idles = t.idleConn[key];
            if (len(idles) >= t.maxIdleConnsPerHost())
            {
                return error.As(errTooManyIdleHost);
            }
            foreach (var (_, exist) in idles)
            {
                if (exist == pconn)
                {
                    log.Fatalf("dup idle pconn %p in freelist", pconn);
                }
            }
            t.idleConn[key] = append(idles, pconn);
            t.idleLRU.add(pconn);
            if (t.MaxIdleConns != 0L && t.idleLRU.len() > t.MaxIdleConns)
            {
                var oldest = t.idleLRU.removeOldest();
                oldest.close(errTooManyIdle);
                t.removeIdleConnLocked(oldest);
            }
            if (t.IdleConnTimeout > 0L)
            {
                if (pconn.idleTimer != null)
                {
                    pconn.idleTimer.Reset(t.IdleConnTimeout);
                }
                else
                {
                    pconn.idleTimer = time.AfterFunc(t.IdleConnTimeout, pconn.closeConnIfStillIdle);
                }
            }
            pconn.idleAt = time.Now();
            return error.As(null);
        });

        // getIdleConnCh returns a channel to receive and return idle
        // persistent connection for the given connectMethod.
        // It may return nil, if persistent connections are not being used.
        private static channel<ref persistConn> getIdleConnCh(this ref Transport _t, connectMethod cm) => func(_t, (ref Transport t, Defer defer, Panic _, Recover __) =>
        {
            if (t.DisableKeepAlives)
            {
                return null;
            }
            var key = cm.key();
            t.idleMu.Lock();
            defer(t.idleMu.Unlock());
            t.wantIdle = false;
            if (t.idleConnCh == null)
            {
                t.idleConnCh = make_map<connectMethodKey, channel<ref persistConn>>();
            }
            var (ch, ok) = t.idleConnCh[key];
            if (!ok)
            {
                ch = make_channel<ref persistConn>();
                t.idleConnCh[key] = ch;
            }
            return ch;
        });

        private static (ref persistConn, time.Time) getIdleConn(this ref Transport _t, connectMethod cm) => func(_t, (ref Transport t, Defer defer, Panic _, Recover __) =>
        {
            var key = cm.key();
            t.idleMu.Lock();
            defer(t.idleMu.Unlock());
            while (true)
            {
                var (pconns, ok) = t.idleConn[key];
                if (!ok)
                {
                    return (null, new time.Time());
                }
                if (len(pconns) == 1L)
                {
                    pconn = pconns[0L];
                    delete(t.idleConn, key);
                }
                else
                { 
                    // 2 or more cached connections; use the most
                    // recently used one at the end.
                    pconn = pconns[len(pconns) - 1L];
                    t.idleConn[key] = pconns[..len(pconns) - 1L];
                }
                t.idleLRU.remove(pconn);
                if (pconn.isBroken())
                { 
                    // There is a tiny window where this is
                    // possible, between the connecting dying and
                    // the persistConn readLoop calling
                    // Transport.removeIdleConn. Just skip it and
                    // carry on.
                    continue;
                }
                if (pconn.idleTimer != null && !pconn.idleTimer.Stop())
                { 
                    // We picked this conn at the ~same time it
                    // was expiring and it's trying to close
                    // itself in another goroutine. Don't use it.
                    continue;
                }
                return (pconn, pconn.idleAt);
            }

        });

        // removeIdleConn marks pconn as dead.
        private static void removeIdleConn(this ref Transport _t, ref persistConn _pconn) => func(_t, _pconn, (ref Transport t, ref persistConn pconn, Defer defer, Panic _, Recover __) =>
        {
            t.idleMu.Lock();
            defer(t.idleMu.Unlock());
            t.removeIdleConnLocked(pconn);
        });

        // t.idleMu must be held.
        private static void removeIdleConnLocked(this ref Transport t, ref persistConn pconn)
        {
            if (pconn.idleTimer != null)
            {
                pconn.idleTimer.Stop();
            }
            t.idleLRU.remove(pconn);
            var key = pconn.cacheKey;
            var pconns = t.idleConn[key];
            switch (len(pconns))
            {
                case 0L: 
                    break;
                case 1L: 
                    if (pconns[0L] == pconn)
                    {
                        delete(t.idleConn, key);
                    }
                    break;
                default: 
                    foreach (var (i, v) in pconns)
                    {
                        if (v != pconn)
                        {
                            continue;
                        } 
                        // Slide down, keeping most recently-used
                        // conns at the end.
                        copy(pconns[i..], pconns[i + 1L..]);
                        t.idleConn[key] = pconns[..len(pconns) - 1L];
                        break;
                    }
                    break;
            }
        }

        private static void setReqCanceler(this ref Transport _t, ref Request _r, Action<error> fn) => func(_t, _r, (ref Transport t, ref Request r, Defer defer, Panic _, Recover __) =>
        {
            t.reqMu.Lock();
            defer(t.reqMu.Unlock());
            if (t.reqCanceler == null)
            {
                t.reqCanceler = make_map<ref Request, Action<error>>();
            }
            if (fn != null)
            {
                t.reqCanceler[r] = fn;
            }
            else
            {
                delete(t.reqCanceler, r);
            }
        });

        // replaceReqCanceler replaces an existing cancel function. If there is no cancel function
        // for the request, we don't set the function and return false.
        // Since CancelRequest will clear the canceler, we can use the return value to detect if
        // the request was canceled since the last setReqCancel call.
        private static bool replaceReqCanceler(this ref Transport _t, ref Request _r, Action<error> fn) => func(_t, _r, (ref Transport t, ref Request r, Defer defer, Panic _, Recover __) =>
        {
            t.reqMu.Lock();
            defer(t.reqMu.Unlock());
            var (_, ok) = t.reqCanceler[r];
            if (!ok)
            {
                return false;
            }
            if (fn != null)
            {
                t.reqCanceler[r] = fn;
            }
            else
            {
                delete(t.reqCanceler, r);
            }
            return true;
        });

        private static net.Dialer zeroDialer = default;

        private static (net.Conn, error) dial(this ref Transport t, context.Context ctx, @string network, @string addr)
        {
            if (t.DialContext != null)
            {
                return t.DialContext(ctx, network, addr);
            }
            if (t.Dial != null)
            {
                var (c, err) = t.Dial(network, addr);
                if (c == null && err == null)
                {
                    err = errors.New("net/http: Transport.Dial hook returned (nil, nil)");
                }
                return (c, err);
            }
            return zeroDialer.DialContext(ctx, network, addr);
        }

        // getConn dials and creates a new persistConn to the target as
        // specified in the connectMethod. This includes doing a proxy CONNECT
        // and/or setting up TLS.  If this doesn't return an error, the persistConn
        // is ready to write requests to.
        private static (ref persistConn, error) getConn(this ref Transport t, ref transportRequest treq, connectMethod cm)
        {
            var req = treq.Request;
            var trace = treq.trace;
            var ctx = req.Context();
            if (trace != null && trace.GetConn != null)
            {
                trace.GetConn(cm.addr());
            }
            {
                var pc__prev1 = pc;

                var (pc, idleSince) = t.getIdleConn(cm);

                if (pc != null)
                {
                    if (trace != null && trace.GotConn != null)
                    {
                        trace.GotConn(pc.gotIdleConnTrace(idleSince));
                    } 
                    // set request canceler to some non-nil function so we
                    // can detect whether it was cleared between now and when
                    // we enter roundTrip
                    t.setReqCanceler(req, _p0 =>
                    {
                    });
                    return (pc, null);
                }

                pc = pc__prev1;

            }

            private partial struct dialRes
            {
                public ptr<persistConn> pc;
                public error err;
            }
            var dialc = make_channel<dialRes>(); 

            // Copy these hooks so we don't race on the postPendingDial in
            // the goroutine we launch. Issue 11136.
            var testHookPrePendingDial = testHookPrePendingDial;
            var testHookPostPendingDial = testHookPostPendingDial;

            Action handlePendingDial = () =>
            {
                testHookPrePendingDial();
                go_(() => () =>
                {
                    {
                        var v__prev1 = v;

                        var v = dialc.Receive();

                        if (v.err == null)
                        {
                            t.putOrCloseIdleConn(v.pc);
                        }

                        v = v__prev1;

                    }
                    testHookPostPendingDial();
                }());
            }
;

            var cancelc = make_channel<error>(1L);
            t.setReqCanceler(req, err =>
            {
                cancelc.Send(err);

            });

            go_(() => () =>
            {
                var (pc, err) = t.dialConn(ctx, cm);
                dialc.Send(new dialRes(pc,err));
            }());

            var idleConnCh = t.getIdleConnCh(cm);
            if (v.pc != null)
            {
                if (trace != null && trace.GotConn != null && v.pc.alt == null)
                {
                    trace.GotConn(new httptrace.GotConnInfo(Conn:v.pc.conn));
                }
                return (v.pc, null);
            } 
            // Our dial failed. See why to return a nicer error
            // value.
            return (null, errRequestCanceledConn);
            return (null, req.Context().Err());
            if (err == errRequestCanceled)
            {
                err = errRequestCanceledConn;
            }
            return (null, err);
            return (null, v.err);
            handlePendingDial();
            if (trace != null && trace.GotConn != null)
            {
                trace.GotConn(new httptrace.GotConnInfo(Conn:pc.conn,Reused:pc.isReused()));
            }
            return (pc, null);
            handlePendingDial();
            return (null, errRequestCanceledConn);
            handlePendingDial();
            return (null, req.Context().Err());
            handlePendingDial();
            if (err == errRequestCanceled)
            {
                err = errRequestCanceledConn;
            }
            return (null, err);
        }

        private partial struct oneConnDialer // : channel<net.Conn>
        {
        }

        private static proxy.Dialer newOneConnDialer(net.Conn c)
        {
            var ch = make_channel<net.Conn>(1L);
            ch.Send(c);
            return oneConnDialer(ch);
        }

        private static (net.Conn, error) Dial(this oneConnDialer d, @string network, @string addr)
        {
            return (c, null);
            return (null, io.EOF);
        }

        // The connect method and the transport can both specify a TLS
        // Host name.  The transport's name takes precedence if present.
        private static @string chooseTLSHost(connectMethod cm, ref Transport t)
        {
            @string tlsHost = "";
            if (t.TLSClientConfig != null)
            {
                tlsHost = t.TLSClientConfig.ServerName;
            }
            if (tlsHost == "")
            {
                tlsHost = cm.tlsHost();
            }
            return tlsHost;
        }

        // Add TLS to a persistent connection, i.e. negotiate a TLS session. If pconn is already a TLS
        // tunnel, this function establishes a nested TLS session inside the encrypted channel.
        // The remote endpoint's name may be overridden by TLSClientConfig.ServerName.
        private static error addTLS(this ref persistConn pconn, @string name, ref httptrace.ClientTrace trace)
        { 
            // Initiate TLS and check remote host name against certificate.
            var cfg = cloneTLSConfig(pconn.t.TLSClientConfig);
            if (cfg.ServerName == "")
            {
                cfg.ServerName = name;
            }
            var plainConn = pconn.conn;
            var tlsConn = tls.Client(plainConn, cfg);
            var errc = make_channel<error>(2L);
            ref time.Timer timer = default; // for canceling TLS handshake
            {
                var d = pconn.t.TLSHandshakeTimeout;

                if (d != 0L)
                {
                    timer = time.AfterFunc(d, () =>
                    {
                        errc.Send(new tlsHandshakeTimeoutError());
                    });
                }

            }
            go_(() => () =>
            {
                if (trace != null && trace.TLSHandshakeStart != null)
                {
                    trace.TLSHandshakeStart();
                }
                var err = tlsConn.Handshake();
                if (timer != null)
                {
                    timer.Stop();
                }
                errc.Send(err);
            }());
            {
                var err__prev1 = err;

                err = errc.Receive();

                if (err != null)
                {
                    plainConn.Close();
                    if (trace != null && trace.TLSHandshakeDone != null)
                    {
                        trace.TLSHandshakeDone(new tls.ConnectionState(), err);
                    }
                    return error.As(err);
                }

                err = err__prev1;

            }
            if (!cfg.InsecureSkipVerify)
            {
                {
                    var err__prev2 = err;

                    err = tlsConn.VerifyHostname(cfg.ServerName);

                    if (err != null)
                    {
                        plainConn.Close();
                        return error.As(err);
                    }

                    err = err__prev2;

                }
            }
            var cs = tlsConn.ConnectionState();
            if (trace != null && trace.TLSHandshakeDone != null)
            {
                trace.TLSHandshakeDone(cs, null);
            }
            pconn.tlsState = ref cs;
            pconn.conn = tlsConn;
            return error.As(null);
        }

        private static (ref persistConn, error) dialConn(this ref Transport t, context.Context ctx, connectMethod cm)
        {
            persistConn pconn = ref new persistConn(t:t,cacheKey:cm.key(),reqch:make(chanrequestAndChan,1),writech:make(chanwriteRequest,1),closech:make(chanstruct{}),writeErrCh:make(chanerror,1),writeLoopDone:make(chanstruct{}),);
            var trace = httptrace.ContextClientTrace(ctx);
            Func<error, error> wrapErr = err =>
            {
                if (cm.proxyURL != null)
                { 
                    // Return a typed error, per Issue 16997
                    return ref new net.OpError(Op:"proxyconnect",Net:"tcp",Err:err);
                }
                return err;
            }
;
            if (cm.scheme() == "https" && t.DialTLS != null)
            {
                error err = default;
                pconn.conn, err = t.DialTLS("tcp", cm.addr());
                if (err != null)
                {
                    return (null, wrapErr(err));
                }
                if (pconn.conn == null)
                {
                    return (null, wrapErr(errors.New("net/http: Transport.DialTLS returned (nil, nil)")));
                }
                {
                    ref tls.Conn (tc, ok) = pconn.conn._<ref tls.Conn>();

                    if (ok)
                    { 
                        // Handshake here, in case DialTLS didn't. TLSNextProto below
                        // depends on it for knowing the connection state.
                        if (trace != null && trace.TLSHandshakeStart != null)
                        {
                            trace.TLSHandshakeStart();
                        }
                        {
                            error err__prev3 = err;

                            err = tc.Handshake();

                            if (err != null)
                            {
                                go_(() => pconn.conn.Close());
                                if (trace != null && trace.TLSHandshakeDone != null)
                                {
                                    trace.TLSHandshakeDone(new tls.ConnectionState(), err);
                                }
                                return (null, err);
                            }

                            err = err__prev3;

                        }
                        var cs = tc.ConnectionState();
                        if (trace != null && trace.TLSHandshakeDone != null)
                        {
                            trace.TLSHandshakeDone(cs, null);
                        }
                        pconn.tlsState = ref cs;
                    }

                }
            }
            else
            {
                var (conn, err) = t.dial(ctx, "tcp", cm.addr());
                if (err != null)
                {
                    return (null, wrapErr(err));
                }
                pconn.conn = conn;
                if (cm.scheme() == "https")
                {
                    @string firstTLSHost = default;
                    firstTLSHost, _, err = net.SplitHostPort(cm.addr());

                    if (err != null)
                    {
                        return (null, wrapErr(err));
                    }
                    err = error.As(pconn.addTLS(firstTLSHost, trace));

                    if (err != null)
                    {
                        return (null, wrapErr(err));
                    }
                }
            } 

            // Proxy setup.

            if (cm.proxyURL == null)             else if (cm.proxyURL.Scheme == "socks5") 
                var conn = pconn.conn;
                ref proxy.Auth auth = default;
                {
                    var u = cm.proxyURL.User;

                    if (u != null)
                    {
                        auth = ref new proxy.Auth();
                        auth.User = u.Username();
                        auth.Password, _ = u.Password();
                    }

                }
                var (p, err) = proxy.SOCKS5("", cm.addr(), auth, newOneConnDialer(conn));
                if (err != null)
                {
                    conn.Close();
                    return (null, err);
                }
                {
                    error err__prev1 = err;

                    var (_, err) = p.Dial("tcp", cm.targetAddr);

                    if (err != null)
                    {
                        conn.Close();
                        return (null, err);
                    }

                    err = err__prev1;

                }
            else if (cm.targetScheme == "http") 
                pconn.isProxy = true;
                {
                    var pa__prev1 = pa;

                    var pa = cm.proxyAuth();

                    if (pa != "")
                    {
                        pconn.mutateHeaderFunc = h =>
                        {
                            h.Set("Proxy-Authorization", pa);
                        }
;
                    }

                    pa = pa__prev1;

                }
            else if (cm.targetScheme == "https") 
                conn = pconn.conn;
                var hdr = t.ProxyConnectHeader;
                if (hdr == null)
                {
                    hdr = make(Header);
                }
                Request connectReq = ref new Request(Method:"CONNECT",URL:&url.URL{Opaque:cm.targetAddr},Host:cm.targetAddr,Header:hdr,);
                {
                    var pa__prev1 = pa;

                    pa = cm.proxyAuth();

                    if (pa != "")
                    {
                        connectReq.Header.Set("Proxy-Authorization", pa);
                    }

                    pa = pa__prev1;

                }
                connectReq.Write(conn); 

                // Read response.
                // Okay to use and discard buffered reader here, because
                // TLS server will not speak until spoken to.
                var br = bufio.NewReader(conn);
                var (resp, err) = ReadResponse(br, connectReq);
                if (err != null)
                {
                    conn.Close();
                    return (null, err);
                }
                if (resp.StatusCode != 200L)
                {
                    var f = strings.SplitN(resp.Status, " ", 2L);
                    conn.Close();
                    if (len(f) < 2L)
                    {
                        return (null, errors.New("unknown status code"));
                    }
                    return (null, errors.New(f[1L]));
                }
                        if (cm.proxyURL != null && cm.targetScheme == "https")
            {
                {
                    error err__prev2 = err;

                    err = pconn.addTLS(cm.tlsHost(), trace);

                    if (err != null)
                    {
                        return (null, err);
                    }

                    err = err__prev2;

                }
            }
            {
                var s = pconn.tlsState;

                if (s != null && s.NegotiatedProtocolIsMutual && s.NegotiatedProtocol != "")
                {
                    {
                        var (next, ok) = t.TLSNextProto[s.NegotiatedProtocol];

                        if (ok)
                        {
                            return (ref new persistConn(alt:next(cm.targetAddr,pconn.conn.(*tls.Conn))), null);
                        }

                    }
                }

            }

            pconn.br = bufio.NewReader(pconn);
            pconn.bw = bufio.NewWriter(new persistConnWriter(pconn));
            go_(() => pconn.readLoop());
            go_(() => pconn.writeLoop());
            return (pconn, null);
        }

        // persistConnWriter is the io.Writer written to by pc.bw.
        // It accumulates the number of bytes written to the underlying conn,
        // so the retry logic can determine whether any bytes made it across
        // the wire.
        // This is exactly 1 pointer field wide so it can go into an interface
        // without allocation.
        private partial struct persistConnWriter
        {
            public ptr<persistConn> pc;
        }

        private static (long, error) Write(this persistConnWriter w, slice<byte> p)
        {
            n, err = w.pc.conn.Write(p);
            w.pc.nwrite += int64(n);
            return;
        }

        // useProxy reports whether requests to addr should use a proxy,
        // according to the NO_PROXY or no_proxy environment variable.
        // addr is always a canonicalAddr with a host and port.
        private static bool useProxy(@string addr)
        {
            if (len(addr) == 0L)
            {
                return true;
            }
            var (host, _, err) = net.SplitHostPort(addr);
            if (err != null)
            {
                return false;
            }
            if (host == "localhost")
            {
                return false;
            }
            {
                var ip = net.ParseIP(host);

                if (ip != null)
                {
                    if (ip.IsLoopback())
                    {
                        return false;
                    }
                }

            }

            var noProxy = noProxyEnv.Get();
            if (noProxy == "*")
            {
                return false;
            }
            addr = strings.ToLower(strings.TrimSpace(addr));
            if (hasPort(addr))
            {
                addr = addr[..strings.LastIndex(addr, ":")];
            }
            foreach (var (_, p) in strings.Split(noProxy, ","))
            {
                p = strings.ToLower(strings.TrimSpace(p));
                if (len(p) == 0L)
                {
                    continue;
                }
                if (hasPort(p))
                {
                    p = p[..strings.LastIndex(p, ":")];
                }
                if (addr == p)
                {
                    return false;
                }
                if (len(p) == 0L)
                { 
                    // There is no host part, likely the entry is malformed; ignore.
                    continue;
                }
                if (p[0L] == '.' && (strings.HasSuffix(addr, p) || addr == p[1L..]))
                { 
                    // no_proxy ".foo.com" matches "bar.foo.com" or "foo.com"
                    return false;
                }
                if (p[0L] != '.' && strings.HasSuffix(addr, p) && addr[len(addr) - len(p) - 1L] == '.')
                { 
                    // no_proxy "foo.com" matches "bar.foo.com"
                    return false;
                }
            }
            return true;
        }

        // connectMethod is the map key (in its String form) for keeping persistent
        // TCP connections alive for subsequent HTTP requests.
        //
        // A connect method may be of the following types:
        //
        //    Cache key form                    Description
        //    -----------------                 -------------------------
        //    |http|foo.com                     http directly to server, no proxy
        //    |https|foo.com                    https directly to server, no proxy
        //    http://proxy.com|https|foo.com    http to proxy, then CONNECT to foo.com
        //    http://proxy.com|http             http to proxy, http to anywhere after that
        //    socks5://proxy.com|http|foo.com   socks5 to proxy, then http to foo.com
        //    socks5://proxy.com|https|foo.com  socks5 to proxy, then https to foo.com
        //    https://proxy.com|https|foo.com   https to proxy, then CONNECT to foo.com
        //    https://proxy.com|http            https to proxy, http to anywhere after that
        //
        private partial struct connectMethod
        {
            public ptr<url.URL> proxyURL; // nil for no proxy, else full proxy URL
            public @string targetScheme; // "http" or "https"
// If proxyURL specifies an http or https proxy, and targetScheme is http (not https),
// then targetAddr is not included in the connect method key, because the socket can
// be reused for different targetAddr values.
            public @string targetAddr;
        }

        private static connectMethodKey key(this ref connectMethod cm)
        {
            @string proxyStr = "";
            var targetAddr = cm.targetAddr;
            if (cm.proxyURL != null)
            {
                proxyStr = cm.proxyURL.String();
                if ((cm.proxyURL.Scheme == "http" || cm.proxyURL.Scheme == "https") && cm.targetScheme == "http")
                {
                    targetAddr = "";
                }
            }
            return new connectMethodKey(proxy:proxyStr,scheme:cm.targetScheme,addr:targetAddr,);
        }

        // scheme returns the first hop scheme: http, https, or socks5
        private static @string scheme(this ref connectMethod cm)
        {
            if (cm.proxyURL != null)
            {
                return cm.proxyURL.Scheme;
            }
            return cm.targetScheme;
        }

        // addr returns the first hop "host:port" to which we need to TCP connect.
        private static @string addr(this ref connectMethod cm)
        {
            if (cm.proxyURL != null)
            {
                return canonicalAddr(cm.proxyURL);
            }
            return cm.targetAddr;
        }

        // tlsHost returns the host name to match against the peer's
        // TLS certificate.
        private static @string tlsHost(this ref connectMethod cm)
        {
            var h = cm.targetAddr;
            if (hasPort(h))
            {
                h = h[..strings.LastIndex(h, ":")];
            }
            return h;
        }

        // connectMethodKey is the map key version of connectMethod, with a
        // stringified proxy URL (or the empty string) instead of a pointer to
        // a URL.
        private partial struct connectMethodKey
        {
            public @string proxy;
            public @string scheme;
            public @string addr;
        }

        private static @string String(this connectMethodKey k)
        { 
            // Only used by tests.
            return fmt.Sprintf("%s|%s|%s", k.proxy, k.scheme, k.addr);
        }

        // persistConn wraps a connection, usually a persistent one
        // (but may be used for non-keep-alive requests as well)
        private partial struct persistConn
        {
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
            public long numExpectedResponses;
            public error closed; // set non-nil when conn is closed, before closech is closed
            public error canceledErr; // set non-nil if conn is canceled
            public bool broken; // an error has happened on this connection; marked broken so it's not reused.
            public bool reused; // whether conn has had successful request/response and is being reused.
// mutateHeaderFunc is an optional func to modify extra
// headers on each outbound request before it's written. (the
// original Request given to RoundTrip is not modified)
            public Action<Header> mutateHeaderFunc;
        }

        private static long maxHeaderResponseSize(this ref persistConn pc)
        {
            {
                var v = pc.t.MaxResponseHeaderBytes;

                if (v != 0L)
                {
                    return v;
                }

            }
            return 10L << (int)(20L); // conservative default; same as http2
        }

        private static (long, error) Read(this ref persistConn pc, slice<byte> p)
        {
            if (pc.readLimit <= 0L)
            {
                return (0L, fmt.Errorf("read limit of %d bytes exhausted", pc.maxHeaderResponseSize()));
            }
            if (int64(len(p)) > pc.readLimit)
            {
                p = p[..pc.readLimit];
            }
            n, err = pc.conn.Read(p);
            if (err == io.EOF)
            {
                pc.sawEOF = true;
            }
            pc.readLimit -= int64(n);
            return;
        }

        // isBroken reports whether this connection is in a known broken state.
        private static bool isBroken(this ref persistConn pc)
        {
            pc.mu.Lock();
            var b = pc.closed != null;
            pc.mu.Unlock();
            return b;
        }

        // canceled returns non-nil if the connection was closed due to
        // CancelRequest or due to context cancelation.
        private static error canceled(this ref persistConn _pc) => func(_pc, (ref persistConn pc, Defer defer, Panic _, Recover __) =>
        {
            pc.mu.Lock();
            defer(pc.mu.Unlock());
            return error.As(pc.canceledErr);
        });

        // isReused reports whether this connection is in a known broken state.
        private static bool isReused(this ref persistConn pc)
        {
            pc.mu.Lock();
            var r = pc.reused;
            pc.mu.Unlock();
            return r;
        }

        private static httptrace.GotConnInfo gotIdleConnTrace(this ref persistConn _pc, time.Time idleAt) => func(_pc, (ref persistConn pc, Defer defer, Panic _, Recover __) =>
        {
            pc.mu.Lock();
            defer(pc.mu.Unlock());
            t.Reused = pc.reused;
            t.Conn = pc.conn;
            t.WasIdle = true;
            if (!idleAt.IsZero())
            {
                t.IdleTime = time.Since(idleAt);
            }
            return;
        });

        private static void cancelRequest(this ref persistConn _pc, error err) => func(_pc, (ref persistConn pc, Defer defer, Panic _, Recover __) =>
        {
            pc.mu.Lock();
            defer(pc.mu.Unlock());
            pc.canceledErr = err;
            pc.closeLocked(errRequestCanceled);
        });

        // closeConnIfStillIdle closes the connection if it's still sitting idle.
        // This is what's called by the persistConn's idleTimer, and is run in its
        // own goroutine.
        private static void closeConnIfStillIdle(this ref persistConn _pc) => func(_pc, (ref persistConn pc, Defer defer, Panic _, Recover __) =>
        {
            var t = pc.t;
            t.idleMu.Lock();
            defer(t.idleMu.Unlock());
            {
                var (_, ok) = t.idleLRU.m[pc];

                if (!ok)
                { 
                    // Not idle.
                    return;
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
        private static error mapRoundTripError(this ref persistConn pc, ref transportRequest req, long startBytesWritten, error err)
        {
            if (err == null)
            {
                return error.As(null);
            } 

            // If the request was canceled, that's better than network
            // failures that were likely the result of tearing down the
            // connection.
            {
                var cerr = pc.canceled();

                if (cerr != null)
                {
                    return error.As(cerr);
                } 

                // See if an error was set explicitly.

            } 

            // See if an error was set explicitly.
            req.mu.Lock();
            var reqErr = req.err;
            req.mu.Unlock();
            if (reqErr != null)
            {
                return error.As(reqErr);
            }
            if (err == errServerClosedIdle)
            { 
                // Don't decorate
                return error.As(err);
            }
            {
                transportReadFromServerError (_, ok) = err._<transportReadFromServerError>();

                if (ok)
                { 
                    // Don't decorate
                    return error.As(err);
                }

            }
            if (pc.isBroken())
            {
                pc.writeLoopDone.Receive();
                if (pc.nwrite == startBytesWritten)
                {
                    return error.As(new nothingWrittenError(err));
                }
                return error.As(fmt.Errorf("net/http: HTTP/1.x transport connection broken: %v", err));
            }
            return error.As(err);
        }

        private static void readLoop(this ref persistConn _pc) => func(_pc, (ref persistConn pc, Defer defer, Panic _, Recover __) =>
        {
            var closeErr = errReadLoopExiting; // default value, if not changed below
            defer(() =>
            {
                pc.close(closeErr);
                pc.t.removeIdleConn(pc);
            }());

            Func<ref httptrace.ClientTrace, bool> tryPutIdleConn = trace =>
            {
                {
                    var err = pc.t.tryPutIdleConn(pc);

                    if (err != null)
                    {
                        closeErr = err;
                        if (trace != null && trace.PutIdleConn != null && err != errKeepAlivesDisabled)
                        {
                            trace.PutIdleConn(err);
                        }
                        return false;
                    }

                }
                if (trace != null && trace.PutIdleConn != null)
                {
                    trace.PutIdleConn(null);
                }
                return true;
            } 

            // eofc is used to block caller goroutines reading from Response.Body
            // at EOF until this goroutines has (potentially) added the connection
            // back to the idle pool.
; 

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
            while (alive)
            {
                pc.readLimit = pc.maxHeaderResponseSize();
                var (_, err) = pc.br.Peek(1L);

                pc.mu.Lock();
                if (pc.numExpectedResponses == 0L)
                {
                    pc.readLoopPeekFailLocked(err);
                    pc.mu.Unlock();
                    return;
                }
                pc.mu.Unlock();

                var rc = pc.reqch.Receive();
                var trace = httptrace.ContextClientTrace(rc.req.Context());

                ref Response resp = default;
                if (err == null)
                {
                    resp, err = pc.readResponse(rc, trace);
                }
                else
                {
                    err = new transportReadFromServerError(err);
                    closeErr = err;
                }
                if (err != null)
                {
                    if (pc.readLimit <= 0L)
                    {
                        err = fmt.Errorf("net/http: server response headers exceeded %d bytes; aborted", pc.maxHeaderResponseSize());
                    }
                    return;
                    return;
                }
                pc.readLimit = maxInt64; // effictively no limit for response bodies

                pc.mu.Lock();
                pc.numExpectedResponses--;
                pc.mu.Unlock();

                var hasBody = rc.req.Method != "HEAD" && resp.ContentLength != 0L;

                if (resp.Close || rc.req.Close || resp.StatusCode <= 199L)
                { 
                    // Don't do keep-alive on error if either party requested a close
                    // or we get an unexpected informational (1xx) response.
                    // StatusCode 100 is already handled above.
                    alive = false;
                }
                if (!hasBody)
                {
                    pc.t.setReqCanceler(rc.req, null); 

                    // Put the idle conn back into the pool before we send the response
                    // so if they process it quickly and make another request, they'll
                    // get this same conn. But we use the unbuffered channel 'rc'
                    // to guarantee that persistConn.roundTrip got out of its select
                    // potentially waiting for this persistConn to close.
                    // but after
                    alive = alive && !pc.sawEOF && pc.wroteRequest() && tryPutIdleConn(trace);

                    return;
                    testHookReadLoopBeforeNextRead();
                    continue;
                }
                var waitForBodyRead = make_channel<bool>(2L);
                bodyEOFSignal body = ref new bodyEOFSignal(body:resp.Body,earlyCloseFn:func()error{waitForBodyRead<-false<-eofcreturnnil},fn:func(errerror)error{isEOF:=err==io.EOFwaitForBodyRead<-isEOFifisEOF{<-eofc}elseiferr!=nil{ifcerr:=pc.canceled();cerr!=nil{returncerr}}returnerr},);

                resp.Body = body;
                if (rc.addedGzip && strings.EqualFold(resp.Header.Get("Content-Encoding"), "gzip"))
                {
                    resp.Body = ref new gzipReader(body:body);
                    resp.Header.Del("Content-Encoding");
                    resp.Header.Del("Content-Length");
                    resp.ContentLength = -1L;
                    resp.Uncompressed = true;
                }
                return;
                pc.t.setReqCanceler(rc.req, null); // before pc might return to idle pool
                alive = alive && bodyEOF && !pc.sawEOF && pc.wroteRequest() && tryPutIdleConn(trace);
                if (bodyEOF)
                {
                    eofc.Send(/* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{});
                }
                alive = false;
                pc.t.CancelRequest(rc.req);
                alive = false;
                pc.t.cancelRequest(rc.req, rc.req.Context().Err());
                alive = false;
                testHookReadLoopBeforeNextRead();
            }

        });

        private static void readLoopPeekFailLocked(this ref persistConn pc, error peekErr)
        {
            if (pc.closed != null)
            {
                return;
            }
            {
                var n = pc.br.Buffered();

                if (n > 0L)
                {
                    var (buf, _) = pc.br.Peek(n);
                    log.Printf("Unsolicited response received on idle HTTP channel starting with %q; err=%v", buf, peekErr);
                }

            }
            if (peekErr == io.EOF)
            { 
                // common case.
                pc.closeLocked(errServerClosedIdle);
            }
            else
            {
                pc.closeLocked(fmt.Errorf("readLoopPeekFailLocked: %v", peekErr));
            }
        }

        // readResponse reads an HTTP response (or two, in the case of "Expect:
        // 100-continue") from the server. It returns the final non-100 one.
        // trace is optional.
        private static (ref Response, error) readResponse(this ref persistConn pc, requestAndChan rc, ref httptrace.ClientTrace trace)
        {
            if (trace != null && trace.GotFirstResponseByte != null)
            {
                {
                    var (peek, err) = pc.br.Peek(1L);

                    if (err == null && len(peek) == 1L)
                    {
                        trace.GotFirstResponseByte();
                    }

                }
            }
            resp, err = ReadResponse(pc.br, rc.req);
            if (err != null)
            {
                return;
            }
            if (rc.continueCh != null)
            {
                if (resp.StatusCode == 100L)
                {
                    if (trace != null && trace.Got100Continue != null)
                    {
                        trace.Got100Continue();
                    }
                    rc.continueCh.Send(/* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{});
                }
                else
                {
                    close(rc.continueCh);
                }
            }
            if (resp.StatusCode == 100L)
            {
                pc.readLimit = pc.maxHeaderResponseSize(); // reset the limit
                resp, err = ReadResponse(pc.br, rc.req);
                if (err != null)
                {
                    return;
                }
            }
            resp.TLS = pc.tlsState;
            return;
        }

        // waitForContinue returns the function to block until
        // any response, timeout or connection close. After any of them,
        // the function returns a bool which indicates if the body should be sent.
        private static Func<bool> waitForContinue(this ref persistConn _pc, channel<object> continueCh) => func(_pc, (ref persistConn pc, Defer defer, Panic _, Recover __) =>
        {
            if (continueCh == null)
            {
                return null;
            }
            return () =>
            {
                var timer = time.NewTimer(pc.t.ExpectContinueTimeout);
                defer(timer.Stop());

                return ok;
                return true;
                return false;
            }
;
        });

        // nothingWrittenError wraps a write errors which ended up writing zero bytes.
        private partial struct nothingWrittenError : error
        {
            public error error;
        }

        private static void writeLoop(this ref persistConn _pc) => func(_pc, (ref persistConn pc, Defer defer, Panic _, Recover __) =>
        {
            defer(close(pc.writeLoopDone));
            while (true)
            {
                var startBytesWritten = pc.nwrite;
                var err = wr.req.Request.write(pc.bw, pc.isProxy, wr.req.extra, pc.waitForContinue(wr.continueCh));
                {
                    requestBodyReadError (bre, ok) = err._<requestBodyReadError>();

                    if (ok)
                    {
                        err = bre.error; 
                        // Errors reading from the user's
                        // Request.Body are high priority.
                        // Set it here before sending on the
                        // channels below or calling
                        // pc.close() which tears town
                        // connections and causes other
                        // errors.
                        wr.req.setError(err);
                    }

                }
                if (err == null)
                {
                    err = pc.bw.Flush();
                }
                if (err != null)
                {
                    wr.req.Request.closeBody();
                    if (pc.nwrite == startBytesWritten)
                    {
                        err = new nothingWrittenError(err);
                    }
                }
                pc.writeErrCh.Send(err); // to the body reader, which might recycle us
                wr.ch.Send(err); // to the roundTrip function
                if (err != null)
                {
                    pc.close(err);
                    return;
                }
                return;
            }

        });

        // wroteRequest is a check before recycling a connection that the previous write
        // (from writeLoop above) happened and was successful.
        private static bool wroteRequest(this ref persistConn pc)
        {
            return err == null;
            return err == null;
            return false;
        }

        // responseAndError is how the goroutine reading from an HTTP/1 server
        // communicates with the goroutine doing the RoundTrip.
        private partial struct responseAndError
        {
            public ptr<Response> res; // else use this response (see res method)
            public error err;
        }

        private partial struct requestAndChan
        {
            public ptr<Request> req;
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
        private partial struct writeRequest
        {
            public ptr<transportRequest> req;
            public channel<error> ch; // Optional blocking chan for Expect: 100-continue (for receive).
// If not nil, writeLoop blocks sending request body until
// it receives from this chan.
            public channel<object> continueCh;
        }

        private partial struct httpError
        {
            public @string err;
            public bool timeout;
        }

        private static @string Error(this ref httpError e)
        {
            return e.err;
        }
        private static bool Timeout(this ref httpError e)
        {
            return e.timeout;
        }
        private static bool Temporary(this ref httpError e)
        {
            return true;
        }

        private static error errTimeout = error.As(ref new httpError(err:"net/http: timeout awaiting response headers",timeout:true));
        private static var errRequestCanceled = errors.New("net/http: request canceled");
        private static var errRequestCanceledConn = errors.New("net/http: request canceled while waiting for connection"); // TODO: unify?

        private static void nop()
        {
        }

        // testHooks. Always non-nil.
        private static var testHookEnterRoundTrip = nop;        private static var testHookWaitResLoop = nop;        private static var testHookRoundTripRetried = nop;        private static var testHookPrePendingDial = nop;        private static var testHookPostPendingDial = nop;        private static sync.Locker testHookMu = new fakeLocker();        private static var testHookReadLoopBeforeNextRead = nop;

        private static (ref Response, error) roundTrip(this ref persistConn _pc, ref transportRequest _req) => func(_pc, _req, (ref persistConn pc, ref transportRequest req, Defer defer, Panic panic, Recover _) =>
        {
            testHookEnterRoundTrip();
            if (!pc.t.replaceReqCanceler(req.Request, pc.cancelRequest))
            {
                pc.t.putOrCloseIdleConn(pc);
                return (null, errRequestCanceled);
            }
            pc.mu.Lock();
            pc.numExpectedResponses++;
            var headerFn = pc.mutateHeaderFunc;
            pc.mu.Unlock();

            if (headerFn != null)
            {
                headerFn(req.extraHeaders());
            } 

            // Ask for a compressed version if the caller didn't set their
            // own value for Accept-Encoding. We only attempt to
            // uncompress the gzip stream if we were the layer that
            // requested it.
            var requestedGzip = false;
            if (!pc.t.DisableCompression && req.Header.Get("Accept-Encoding") == "" && req.Header.Get("Range") == "" && req.Method != "HEAD")
            { 
                // Request gzip only, not deflate. Deflate is ambiguous and
                // not as universally supported anyway.
                // See: http://www.gzip.org/zlib/zlib_faq.html#faq38
                //
                // Note that we don't request this for HEAD requests,
                // due to a bug in nginx:
                //   http://trac.nginx.org/nginx/ticket/358
                //   https://golang.org/issue/5522
                //
                // We don't request gzip if the request is for a range, since
                // auto-decoding a portion of a gzipped document will just fail
                // anyway. See https://golang.org/issue/8923
                requestedGzip = true;
                req.extraHeaders().Set("Accept-Encoding", "gzip");
            }
            channel<object> continueCh = default;
            if (req.ProtoAtLeast(1L, 1L) && req.Body != null && req.expectsContinue())
            {
                continueCh = make_channel<object>(1L);
            }
            if (pc.t.DisableKeepAlives)
            {
                req.extraHeaders().Set("Connection", "close");
            }
            var gone = make_channel<object>();
            defer(close(gone));

            defer(() =>
            {
                if (err != null)
                {
                    pc.t.setReqCanceler(req.Request, null);
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
            var writeErrCh = make_channel<error>(1L);
            pc.writech.Send(new writeRequest(req,writeErrCh,continueCh));

            var resc = make_channel<responseAndError>();
            pc.reqch.Send(new requestAndChan(req:req.Request,ch:resc,addedGzip:requestedGzip,continueCh:continueCh,callerGone:gone,));

            channel<time.Time> respHeaderTimer = default;
            var cancelChan = req.Request.Cancel;
            var ctxDoneChan = req.Context().Done();
            while (true)
            {
                testHookWaitResLoop();
                if (debugRoundTrip)
                {
                    req.logf("writeErrCh resv: %T/%#v", err, err);
                }
                if (err != null)
                {
                    pc.close(fmt.Errorf("write error: %v", err));
                    return (null, pc.mapRoundTripError(req, startBytesWritten, err));
                }
                {
                    var d = pc.t.ResponseHeaderTimeout;

                    if (d > 0L)
                    {
                        if (debugRoundTrip)
                        {
                            req.logf("starting timer for %v", d);
                        }
                        var timer = time.NewTimer(d);
                        defer(timer.Stop()); // prevent leaks
                        respHeaderTimer = timer.C;
                    }

                }
                if (debugRoundTrip)
                {
                    req.logf("closech recv: %T %#v", pc.closed, pc.closed);
                }
                return (null, pc.mapRoundTripError(req, startBytesWritten, pc.closed));
                if (debugRoundTrip)
                {
                    req.logf("timeout waiting for response headers.");
                }
                pc.close(errTimeout);
                return (null, errTimeout);
                if ((re.res == null) == (re.err == null))
                {
                    panic(fmt.Sprintf("internal error: exactly one of res or err should be set; nil=%v", re.res == null));
                }
                if (debugRoundTrip)
                {
                    req.logf("resc recv: %p, %T/%#v", re.res, re.err, re.err);
                }
                if (re.err != null)
                {
                    return (null, pc.mapRoundTripError(req, startBytesWritten, re.err));
                }
                return (re.res, null);
                pc.t.CancelRequest(req.Request);
                cancelChan = null;
                pc.t.cancelRequest(req.Request, req.Context().Err());
                cancelChan = null;
                ctxDoneChan = null;
            }

        });

        // tLogKey is a context WithValue key for test debugging contexts containing
        // a t.Logf func. See export_test.go's Request.WithT method.
        private partial struct tLogKey
        {
        }

        private static void logf(this ref transportRequest tr, @string format, params object[] args)
        {
            {
                Action<@string, object> (logf, ok) = tr.Request.Context().Value(new tLogKey())._<Action<@string, object>>();

                if (ok)
                {
                    logf(time.Now().Format(time.RFC3339Nano) + ": " + format, args);
                }

            }
        }

        // markReused marks this connection as having been successfully used for a
        // request and response.
        private static void markReused(this ref persistConn pc)
        {
            pc.mu.Lock();
            pc.reused = true;
            pc.mu.Unlock();
        }

        // close closes the underlying TCP connection and closes
        // the pc.closech channel.
        //
        // The provided err is only for testing and debugging; in normal
        // circumstances it should never be seen by users.
        private static void close(this ref persistConn _pc, error err) => func(_pc, (ref persistConn pc, Defer defer, Panic _, Recover __) =>
        {
            pc.mu.Lock();
            defer(pc.mu.Unlock());
            pc.closeLocked(err);
        });

        private static void closeLocked(this ref persistConn _pc, error err) => func(_pc, (ref persistConn pc, Defer _, Panic panic, Recover __) =>
        {
            if (err == null)
            {
                panic("nil error");
            }
            pc.broken = true;
            if (pc.closed == null)
            {
                pc.closed = err;
                if (pc.alt != null)
                { 
                    // Do nothing; can only get here via getConn's
                    // handlePendingDial's putOrCloseIdleConn when
                    // it turns out the abandoned connection in
                    // flight ended up negotiating an alternate
                    // protocol. We don't use the connection
                    // freelist for http2. That's done by the
                    // alternate protocol's RoundTripper.
                }
                else
                {
                    pc.conn.Close();
                    close(pc.closech);
                }
            }
            pc.mutateHeaderFunc = null;
        });

        private static map portMap = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, @string>{"http":"80","https":"443","socks5":"1080",};

        // canonicalAddr returns url.Host but always with a ":port" suffix
        private static @string canonicalAddr(ref url.URL url)
        {
            var addr = url.Hostname();
            {
                var (v, err) = idnaASCII(addr);

                if (err == null)
                {
                    addr = v;
                }

            }
            var port = url.Port();
            if (port == "")
            {
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
        private partial struct bodyEOFSignal
        {
            public io.ReadCloser body;
            public sync.Mutex mu; // guards following 4 fields
            public bool closed; // whether Close has been called
            public error rerr; // sticky Read error
            public Func<error, error> fn; // err will be nil on Read io.EOF
            public Func<error> earlyCloseFn; // optional alt Close func used if io.EOF not seen
        }

        private static var errReadOnClosedResBody = errors.New("http: read on closed response body");

        private static (long, error) Read(this ref bodyEOFSignal _es, slice<byte> p) => func(_es, (ref bodyEOFSignal es, Defer defer, Panic _, Recover __) =>
        {
            es.mu.Lock();
            var closed = es.closed;
            var rerr = es.rerr;
            es.mu.Unlock();
            if (closed)
            {
                return (0L, errReadOnClosedResBody);
            }
            if (rerr != null)
            {
                return (0L, rerr);
            }
            n, err = es.body.Read(p);
            if (err != null)
            {
                es.mu.Lock();
                defer(es.mu.Unlock());
                if (es.rerr == null)
                {
                    es.rerr = err;
                }
                err = es.condfn(err);
            }
            return;
        });

        private static error Close(this ref bodyEOFSignal _es) => func(_es, (ref bodyEOFSignal es, Defer defer, Panic _, Recover __) =>
        {
            es.mu.Lock();
            defer(es.mu.Unlock());
            if (es.closed)
            {
                return error.As(null);
            }
            es.closed = true;
            if (es.earlyCloseFn != null && es.rerr != io.EOF)
            {
                return error.As(es.earlyCloseFn());
            }
            var err = es.body.Close();
            return error.As(es.condfn(err));
        });

        // caller must hold es.mu.
        private static error condfn(this ref bodyEOFSignal es, error err)
        {
            if (es.fn == null)
            {
                return error.As(err);
            }
            err = es.fn(err);
            es.fn = null;
            return error.As(err);
        }

        // gzipReader wraps a response body so it can lazily
        // call gzip.NewReader on the first call to Read
        private partial struct gzipReader
        {
            public ptr<bodyEOFSignal> body; // underlying HTTP/1 response body framing
            public ptr<gzip.Reader> zr; // lazily-initialized gzip reader
            public error zerr; // any error from gzip.NewReader; sticky
        }

        private static (long, error) Read(this ref gzipReader gz, slice<byte> p)
        {
            if (gz.zr == null)
            {
                if (gz.zerr == null)
                {
                    gz.zr, gz.zerr = gzip.NewReader(gz.body);
                }
                if (gz.zerr != null)
                {
                    return (0L, gz.zerr);
                }
            }
            gz.body.mu.Lock();
            if (gz.body.closed)
            {
                err = errReadOnClosedResBody;
            }
            gz.body.mu.Unlock();

            if (err != null)
            {
                return (0L, err);
            }
            return gz.zr.Read(p);
        }

        private static error Close(this ref gzipReader gz)
        {
            return error.As(gz.body.Close());
        }

        private partial struct readerAndCloser : io.Reader, io.Closer
        {
            public ref io.Reader Reader => ref Reader_val;
            public ref io.Closer Closer => ref Closer_val;
        }

        private partial struct tlsHandshakeTimeoutError
        {
        }

        private static bool Timeout(this tlsHandshakeTimeoutError _p0)
        {
            return true;
        }
        private static bool Temporary(this tlsHandshakeTimeoutError _p0)
        {
            return true;
        }
        private static @string Error(this tlsHandshakeTimeoutError _p0)
        {
            return "net/http: TLS handshake timeout";
        }

        // fakeLocker is a sync.Locker which does nothing. It's used to guard
        // test-only fields when not under test, to avoid runtime atomic
        // overhead.
        private partial struct fakeLocker
        {
        }

        private static void Lock(this fakeLocker _p0)
        {
        }
        private static void Unlock(this fakeLocker _p0)
        {
        }

        // clneTLSConfig returns a shallow clone of cfg, or a new zero tls.Config if
        // cfg is nil. This is safe to call even if cfg is in active use by a TLS
        // client or server.
        private static ref tls.Config cloneTLSConfig(ref tls.Config cfg)
        {
            if (cfg == null)
            {
                return ref new tls.Config();
            }
            return cfg.Clone();
        }

        private partial struct connLRU
        {
            public ptr<list.List> ll; // list.Element.Value type is of *persistConn
            public map<ref persistConn, ref list.Element> m;
        }

        // add adds pc to the head of the linked list.
        private static void add(this ref connLRU _cl, ref persistConn _pc) => func(_cl, _pc, (ref connLRU cl, ref persistConn pc, Defer _, Panic panic, Recover __) =>
        {
            if (cl.ll == null)
            {
                cl.ll = list.New();
                cl.m = make_map<ref persistConn, ref list.Element>();
            }
            var ele = cl.ll.PushFront(pc);
            {
                var (_, ok) = cl.m[pc];

                if (ok)
                {
                    panic("persistConn was already in LRU");
                }

            }
            cl.m[pc] = ele;
        });

        private static ref persistConn removeOldest(this ref connLRU cl)
        {
            var ele = cl.ll.Back();
            ref persistConn pc = ele.Value._<ref persistConn>();
            cl.ll.Remove(ele);
            delete(cl.m, pc);
            return pc;
        }

        // remove removes pc from cl.
        private static void remove(this ref connLRU cl, ref persistConn pc)
        {
            {
                var (ele, ok) = cl.m[pc];

                if (ok)
                {
                    cl.ll.Remove(ele);
                    delete(cl.m, pc);
                }

            }
        }

        // len returns the number of items in the cache.
        private static long len(this ref connLRU cl)
        {
            return len(cl.m);
        }

        // validPort reports whether p (without the colon) is a valid port in
        // a URL, per RFC 3986 Section 3.2.3, which says the port may be
        // empty, or only contain digits.
        private static bool validPort(@string p)
        {
            foreach (var (_, r) in (slice<byte>)p)
            {
                if (r < '0' || r > '9')
                {
                    return false;
                }
            }
            return true;
        }
    }
}}
