// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// HTTP reverse proxy handler

// package httputil -- go2cs converted at 2020 October 08 03:41:37 UTC
// import "net/http/httputil" ==> using httputil = go.net.http.httputil_package
// Original source: C:\Go\src\net\http\httputil\reverseproxy.go
using context = go.context_package;
using fmt = go.fmt_package;
using io = go.io_package;
using log = go.log_package;
using net = go.net_package;
using http = go.net.http_package;
using textproto = go.net.textproto_package;
using url = go.net.url_package;
using strings = go.strings_package;
using sync = go.sync_package;
using time = go.time_package;

using httpguts = go.golang.org.x.net.http.httpguts_package;
using static go.builtin;
using System;
using System.Threading;

namespace go {
namespace net {
namespace http
{
    public static partial class httputil_package
    {
        // ReverseProxy is an HTTP Handler that takes an incoming request and
        // sends it to another server, proxying the response back to the
        // client.
        //
        // ReverseProxy by default sets the client IP as the value of the
        // X-Forwarded-For header.
        //
        // If an X-Forwarded-For header already exists, the client IP is
        // appended to the existing values. As a special case, if the header
        // exists in the Request.Header map but has a nil value (such as when
        // set by the Director func), the X-Forwarded-For header is
        // not modified.
        //
        // To prevent IP spoofing, be sure to delete any pre-existing
        // X-Forwarded-For header coming from the client or
        // an untrusted proxy.
        public partial struct ReverseProxy
        {
            public Action<ptr<http.Request>> Director; // The transport used to perform proxy requests.
// If nil, http.DefaultTransport is used.
            public http.RoundTripper Transport; // FlushInterval specifies the flush interval
// to flush to the client while copying the
// response body.
// If zero, no periodic flushing is done.
// A negative value means to flush immediately
// after each write to the client.
// The FlushInterval is ignored when ReverseProxy
// recognizes a response as a streaming response;
// for such responses, writes are flushed to the client
// immediately.
            public time.Duration FlushInterval; // ErrorLog specifies an optional logger for errors
// that occur when attempting to proxy the request.
// If nil, logging is done via the log package's standard logger.
            public ptr<log.Logger> ErrorLog; // BufferPool optionally specifies a buffer pool to
// get byte slices for use by io.CopyBuffer when
// copying HTTP response bodies.
            public BufferPool BufferPool; // ModifyResponse is an optional function that modifies the
// Response from the backend. It is called if the backend
// returns a response at all, with any HTTP status code.
// If the backend is unreachable, the optional ErrorHandler is
// called without any call to ModifyResponse.
//
// If ModifyResponse returns an error, ErrorHandler is called
// with its error value. If ErrorHandler is nil, its default
// implementation is used.
            public Func<ptr<http.Response>, error> ModifyResponse; // ErrorHandler is an optional function that handles errors
// reaching the backend or errors from ModifyResponse.
//
// If nil, the default is to log the provided error and return
// a 502 Status Bad Gateway response.
            public Action<http.ResponseWriter, ptr<http.Request>, error> ErrorHandler;
        }

        // A BufferPool is an interface for getting and returning temporary
        // byte slices for use by io.CopyBuffer.
        public partial interface BufferPool
        {
            slice<byte> Get();
            slice<byte> Put(slice<byte> _p0);
        }

        private static @string singleJoiningSlash(@string a, @string b)
        {
            var aslash = strings.HasSuffix(a, "/");
            var bslash = strings.HasPrefix(b, "/");

            if (aslash && bslash) 
                return a + b[1L..];
            else if (!aslash && !bslash) 
                return a + "/" + b;
                        return a + b;

        }

        private static (@string, @string) joinURLPath(ptr<url.URL> _addr_a, ptr<url.URL> _addr_b)
        {
            @string path = default;
            @string rawpath = default;
            ref url.URL a = ref _addr_a.val;
            ref url.URL b = ref _addr_b.val;

            if (a.RawPath == "" && b.RawPath == "")
            {
                return (singleJoiningSlash(a.Path, b.Path), "");
            } 
            // Same as singleJoiningSlash, but uses EscapedPath to determine
            // whether a slash should be added
            var apath = a.EscapedPath();
            var bpath = b.EscapedPath();

            var aslash = strings.HasSuffix(apath, "/");
            var bslash = strings.HasPrefix(bpath, "/");


            if (aslash && bslash) 
                return (a.Path + b.Path[1L..], apath + bpath[1L..]);
            else if (!aslash && !bslash) 
                return (a.Path + "/" + b.Path, apath + "/" + bpath);
                        return (a.Path + b.Path, apath + bpath);

        }

        // NewSingleHostReverseProxy returns a new ReverseProxy that routes
        // URLs to the scheme, host, and base path provided in target. If the
        // target's path is "/base" and the incoming request was for "/dir",
        // the target request will be for /base/dir.
        // NewSingleHostReverseProxy does not rewrite the Host header.
        // To rewrite Host headers, use ReverseProxy directly with a custom
        // Director policy.
        public static ptr<ReverseProxy> NewSingleHostReverseProxy(ptr<url.URL> _addr_target)
        {
            ref url.URL target = ref _addr_target.val;

            var targetQuery = target.RawQuery;
            Action<ptr<http.Request>> director = req =>
            {
                req.URL.Scheme = target.Scheme;
                req.URL.Host = target.Host;
                req.URL.Path, req.URL.RawPath = joinURLPath(_addr_target, _addr_req.URL);
                if (targetQuery == "" || req.URL.RawQuery == "")
                {
                    req.URL.RawQuery = targetQuery + req.URL.RawQuery;
                }
                else
                {
                    req.URL.RawQuery = targetQuery + "&" + req.URL.RawQuery;
                }

                {
                    var (_, ok) = req.Header["User-Agent"];

                    if (!ok)
                    { 
                        // explicitly disable User-Agent so it's not set to default value
                        req.Header.Set("User-Agent", "");

                    }

                }

            }
;
            return addr(new ReverseProxy(Director:director));

        }

        private static void copyHeader(http.Header dst, http.Header src)
        {
            foreach (var (k, vv) in src)
            {
                foreach (var (_, v) in vv)
                {
                    dst.Add(k, v);
                }

            }

        }

        // Hop-by-hop headers. These are removed when sent to the backend.
        // As of RFC 7230, hop-by-hop headers are required to appear in the
        // Connection header field. These are the headers defined by the
        // obsoleted RFC 2616 (section 13.5.1) and are used for backward
        // compatibility.
        private static @string hopHeaders = new slice<@string>(new @string[] { "Connection", "Proxy-Connection", "Keep-Alive", "Proxy-Authenticate", "Proxy-Authorization", "Te", "Trailer", "Transfer-Encoding", "Upgrade" });

        private static void defaultErrorHandler(this ptr<ReverseProxy> _addr_p, http.ResponseWriter rw, ptr<http.Request> _addr_req, error err)
        {
            ref ReverseProxy p = ref _addr_p.val;
            ref http.Request req = ref _addr_req.val;

            p.logf("http: proxy error: %v", err);
            rw.WriteHeader(http.StatusBadGateway);
        }

        private static Action<http.ResponseWriter, ptr<http.Request>, error> getErrorHandler(this ptr<ReverseProxy> _addr_p)
        {
            ref ReverseProxy p = ref _addr_p.val;

            if (p.ErrorHandler != null)
            {
                return p.ErrorHandler;
            }

            return p.defaultErrorHandler;

        }

        // modifyResponse conditionally runs the optional ModifyResponse hook
        // and reports whether the request should proceed.
        private static bool modifyResponse(this ptr<ReverseProxy> _addr_p, http.ResponseWriter rw, ptr<http.Response> _addr_res, ptr<http.Request> _addr_req)
        {
            ref ReverseProxy p = ref _addr_p.val;
            ref http.Response res = ref _addr_res.val;
            ref http.Request req = ref _addr_req.val;

            if (p.ModifyResponse == null)
            {
                return true;
            }

            {
                var err = p.ModifyResponse(res);

                if (err != null)
                {
                    res.Body.Close();
                    p.getErrorHandler()(rw, req, err);
                    return false;
                }

            }

            return true;

        }

        private static void ServeHTTP(this ptr<ReverseProxy> _addr_p, http.ResponseWriter rw, ptr<http.Request> _addr_req) => func((defer, panic, _) =>
        {
            ref ReverseProxy p = ref _addr_p.val;
            ref http.Request req = ref _addr_req.val;

            var transport = p.Transport;
            if (transport == null)
            {
                transport = http.DefaultTransport;
            }

            var ctx = req.Context();
            {
                http.CloseNotifier (cn, ok) = rw._<http.CloseNotifier>();

                if (ok)
                {
                    context.CancelFunc cancel = default;
                    ctx, cancel = context.WithCancel(ctx);
                    defer(cancel());
                    var notifyChan = cn.CloseNotify();
                    go_(() => () =>
                    {
                        cancel();
                    }());

                }

            }


            var outreq = req.Clone(ctx);
            if (req.ContentLength == 0L)
            {
                outreq.Body = null; // Issue 16036: nil Body for http.Transport retries
            }

            if (outreq.Header == null)
            {
                outreq.Header = make(http.Header); // Issue 33142: historical behavior was to always allocate
            }

            p.Director(outreq);
            outreq.Close = false;

            var reqUpType = upgradeType(outreq.Header);
            removeConnectionHeaders(outreq.Header); 

            // Remove hop-by-hop headers to the backend. Especially
            // important is "Connection" because we want a persistent
            // connection, regardless of what the client sent to us.
            {
                var h__prev1 = h;

                foreach (var (_, __h) in hopHeaders)
                {
                    h = __h;
                    var hv = outreq.Header.Get(h);
                    if (hv == "")
                    {
                        continue;
                    }

                    if (h == "Te" && hv == "trailers")
                    { 
                        // Issue 21096: tell backend applications that
                        // care about trailer support that we support
                        // trailers. (We do, but we don't go out of
                        // our way to advertise that unless the
                        // incoming client request thought it was
                        // worth mentioning)
                        continue;

                    }

                    outreq.Header.Del(h);

                } 

                // After stripping all the hop-by-hop connection headers above, add back any
                // necessary for protocol upgrades, such as for websockets.

                h = h__prev1;
            }

            if (reqUpType != "")
            {
                outreq.Header.Set("Connection", "Upgrade");
                outreq.Header.Set("Upgrade", reqUpType);
            }

            {
                var (clientIP, _, err) = net.SplitHostPort(req.RemoteAddr);

                if (err == null)
                { 
                    // If we aren't the first proxy retain prior
                    // X-Forwarded-For information as a comma+space
                    // separated list and fold multiple headers into one.
                    var (prior, ok) = outreq.Header["X-Forwarded-For"];
                    var omit = ok && prior == null; // Issue 38079: nil now means don't populate the header
                    if (len(prior) > 0L)
                    {
                        clientIP = strings.Join(prior, ", ") + ", " + clientIP;
                    }

                    if (!omit)
                    {
                        outreq.Header.Set("X-Forwarded-For", clientIP);
                    }

                }

            }


            var (res, err) = transport.RoundTrip(outreq);
            if (err != null)
            {
                p.getErrorHandler()(rw, outreq, err);
                return ;
            } 

            // Deal with 101 Switching Protocols responses: (WebSocket, h2c, etc)
            if (res.StatusCode == http.StatusSwitchingProtocols)
            {
                if (!p.modifyResponse(rw, res, outreq))
                {
                    return ;
                }

                p.handleUpgradeResponse(rw, outreq, res);
                return ;

            }

            removeConnectionHeaders(res.Header);

            {
                var h__prev1 = h;

                foreach (var (_, __h) in hopHeaders)
                {
                    h = __h;
                    res.Header.Del(h);
                }

                h = h__prev1;
            }

            if (!p.modifyResponse(rw, res, outreq))
            {
                return ;
            }

            copyHeader(rw.Header(), res.Header); 

            // The "Trailer" header isn't included in the Transport's response,
            // at least for *http.Transport. Build it up from Trailer.
            var announcedTrailers = len(res.Trailer);
            if (announcedTrailers > 0L)
            {
                var trailerKeys = make_slice<@string>(0L, len(res.Trailer));
                {
                    var k__prev1 = k;

                    foreach (var (__k) in res.Trailer)
                    {
                        k = __k;
                        trailerKeys = append(trailerKeys, k);
                    }

                    k = k__prev1;
                }

                rw.Header().Add("Trailer", strings.Join(trailerKeys, ", "));

            }

            rw.WriteHeader(res.StatusCode);

            err = p.copyResponse(rw, res.Body, p.flushInterval(req, res));
            if (err != null)
            {
                defer(res.Body.Close()); 
                // Since we're streaming the response, if we run into an error all we can do
                // is abort the request. Issue 23643: ReverseProxy should use ErrAbortHandler
                // on read error while copying body.
                if (!shouldPanicOnCopyError(_addr_req))
                {
                    p.logf("suppressing panic for copyResponse error in test; copy error: %v", err);
                    return ;
                }

                panic(http.ErrAbortHandler);

            }

            res.Body.Close(); // close now, instead of defer, to populate res.Trailer

            if (len(res.Trailer) > 0L)
            { 
                // Force chunking if we saw a response trailer.
                // This prevents net/http from calculating the length for short
                // bodies and adding a Content-Length.
                {
                    http.Flusher (fl, ok) = rw._<http.Flusher>();

                    if (ok)
                    {
                        fl.Flush();
                    }

                }

            }

            if (len(res.Trailer) == announcedTrailers)
            {
                copyHeader(rw.Header(), res.Trailer);
                return ;
            }

            {
                var k__prev1 = k;

                foreach (var (__k, __vv) in res.Trailer)
                {
                    k = __k;
                    vv = __vv;
                    k = http.TrailerPrefix + k;
                    foreach (var (_, v) in vv)
                    {
                        rw.Header().Add(k, v);
                    }

                }

                k = k__prev1;
            }
        });

        private static bool inOurTests = default; // whether we're in our own tests

        // shouldPanicOnCopyError reports whether the reverse proxy should
        // panic with http.ErrAbortHandler. This is the right thing to do by
        // default, but Go 1.10 and earlier did not, so existing unit tests
        // weren't expecting panics. Only panic in our own tests, or when
        // running under the HTTP server.
        private static bool shouldPanicOnCopyError(ptr<http.Request> _addr_req)
        {
            ref http.Request req = ref _addr_req.val;

            if (inOurTests)
            { 
                // Our tests know to handle this panic.
                return true;

            }

            if (req.Context().Value(http.ServerContextKey) != null)
            { 
                // We seem to be running under an HTTP server, so
                // it'll recover the panic.
                return true;

            } 
            // Otherwise act like Go 1.10 and earlier to not break
            // existing tests.
            return false;

        }

        // removeConnectionHeaders removes hop-by-hop headers listed in the "Connection" header of h.
        // See RFC 7230, section 6.1
        private static void removeConnectionHeaders(http.Header h)
        {
            foreach (var (_, f) in h["Connection"])
            {
                foreach (var (_, sf) in strings.Split(f, ","))
                {
                    sf = textproto.TrimString(sf);

                    if (sf != "")
                    {
                        h.Del(sf);
                    }

                }

            }

        }

        // flushInterval returns the p.FlushInterval value, conditionally
        // overriding its value for a specific request/response.
        private static time.Duration flushInterval(this ptr<ReverseProxy> _addr_p, ptr<http.Request> _addr_req, ptr<http.Response> _addr_res)
        {
            ref ReverseProxy p = ref _addr_p.val;
            ref http.Request req = ref _addr_req.val;
            ref http.Response res = ref _addr_res.val;

            var resCT = res.Header.Get("Content-Type"); 

            // For Server-Sent Events responses, flush immediately.
            // The MIME type is defined in https://www.w3.org/TR/eventsource/#text-event-stream
            if (resCT == "text/event-stream")
            {
                return -1L; // negative means immediately
            } 

            // TODO: more specific cases? e.g. res.ContentLength == -1?
            return p.FlushInterval;

        }

        private static error copyResponse(this ptr<ReverseProxy> _addr_p, io.Writer dst, io.Reader src, time.Duration flushInterval) => func((defer, _, __) =>
        {
            ref ReverseProxy p = ref _addr_p.val;

            if (flushInterval != 0L)
            {
                {
                    writeFlusher (wf, ok) = writeFlusher.As(dst._<writeFlusher>())!;

                    if (ok)
                    {
                        ptr<maxLatencyWriter> mlw = addr(new maxLatencyWriter(dst:wf,latency:flushInterval,));
                        defer(mlw.stop()); 

                        // set up initial timer so headers get flushed even if body writes are delayed
                        mlw.flushPending = true;
                        mlw.t = time.AfterFunc(flushInterval, mlw.delayedFlush);

                        dst = mlw;

                    }

                }

            }

            slice<byte> buf = default;
            if (p.BufferPool != null)
            {
                buf = p.BufferPool.Get();
                defer(p.BufferPool.Put(buf));
            }

            var (_, err) = p.copyBuffer(dst, src, buf);
            return error.As(err)!;

        });

        // copyBuffer returns any write errors or non-EOF read errors, and the amount
        // of bytes written.
        private static (long, error) copyBuffer(this ptr<ReverseProxy> _addr_p, io.Writer dst, io.Reader src, slice<byte> buf)
        {
            long _p0 = default;
            error _p0 = default!;
            ref ReverseProxy p = ref _addr_p.val;

            if (len(buf) == 0L)
            {
                buf = make_slice<byte>(32L * 1024L);
            }

            long written = default;
            while (true)
            {
                var (nr, rerr) = src.Read(buf);
                if (rerr != null && rerr != io.EOF && rerr != context.Canceled)
                {
                    p.logf("httputil: ReverseProxy read error during body copy: %v", rerr);
                }

                if (nr > 0L)
                {
                    var (nw, werr) = dst.Write(buf[..nr]);
                    if (nw > 0L)
                    {
                        written += int64(nw);
                    }

                    if (werr != null)
                    {
                        return (written, error.As(werr)!);
                    }

                    if (nr != nw)
                    {
                        return (written, error.As(io.ErrShortWrite)!);
                    }

                }

                if (rerr != null)
                {
                    if (rerr == io.EOF)
                    {
                        rerr = null;
                    }

                    return (written, error.As(rerr)!);

                }

            }


        }

        private static void logf(this ptr<ReverseProxy> _addr_p, @string format, params object[] args)
        {
            args = args.Clone();
            ref ReverseProxy p = ref _addr_p.val;

            if (p.ErrorLog != null)
            {
                p.ErrorLog.Printf(format, args);
            }
            else
            {
                log.Printf(format, args);
            }

        }

        private partial interface writeFlusher : io.Writer, http.Flusher
        {
        }

        private partial struct maxLatencyWriter
        {
            public writeFlusher dst;
            public time.Duration latency; // non-zero; negative means to flush immediately

            public sync.Mutex mu; // protects t, flushPending, and dst.Flush
            public ptr<time.Timer> t;
            public bool flushPending;
        }

        private static (long, error) Write(this ptr<maxLatencyWriter> _addr_m, slice<byte> p) => func((defer, _, __) =>
        {
            long n = default;
            error err = default!;
            ref maxLatencyWriter m = ref _addr_m.val;

            m.mu.Lock();
            defer(m.mu.Unlock());
            n, err = m.dst.Write(p);
            if (m.latency < 0L)
            {
                m.dst.Flush();
                return ;
            }

            if (m.flushPending)
            {
                return ;
            }

            if (m.t == null)
            {
                m.t = time.AfterFunc(m.latency, m.delayedFlush);
            }
            else
            {
                m.t.Reset(m.latency);
            }

            m.flushPending = true;
            return ;

        });

        private static void delayedFlush(this ptr<maxLatencyWriter> _addr_m) => func((defer, _, __) =>
        {
            ref maxLatencyWriter m = ref _addr_m.val;

            m.mu.Lock();
            defer(m.mu.Unlock());
            if (!m.flushPending)
            { // if stop was called but AfterFunc already started this goroutine
                return ;

            }

            m.dst.Flush();
            m.flushPending = false;

        });

        private static void stop(this ptr<maxLatencyWriter> _addr_m) => func((defer, _, __) =>
        {
            ref maxLatencyWriter m = ref _addr_m.val;

            m.mu.Lock();
            defer(m.mu.Unlock());
            m.flushPending = false;
            if (m.t != null)
            {
                m.t.Stop();
            }

        });

        private static @string upgradeType(http.Header h)
        {
            if (!httpguts.HeaderValuesContainsToken(h["Connection"], "Upgrade"))
            {
                return "";
            }

            return strings.ToLower(h.Get("Upgrade"));

        }

        private static void handleUpgradeResponse(this ptr<ReverseProxy> _addr_p, http.ResponseWriter rw, ptr<http.Request> _addr_req, ptr<http.Response> _addr_res) => func((defer, _, __) =>
        {
            ref ReverseProxy p = ref _addr_p.val;
            ref http.Request req = ref _addr_req.val;
            ref http.Response res = ref _addr_res.val;

            var reqUpType = upgradeType(req.Header);
            var resUpType = upgradeType(res.Header);
            if (reqUpType != resUpType)
            {
                p.getErrorHandler()(rw, req, fmt.Errorf("backend tried to switch protocol %q when %q was requested", resUpType, reqUpType));
                return ;
            }

            copyHeader(res.Header, rw.Header());

            http.Hijacker (hj, ok) = rw._<http.Hijacker>();
            if (!ok)
            {
                p.getErrorHandler()(rw, req, fmt.Errorf("can't switch protocols using non-Hijacker ResponseWriter type %T", rw));
                return ;
            }

            io.ReadWriteCloser (backConn, ok) = res.Body._<io.ReadWriteCloser>();
            if (!ok)
            {
                p.getErrorHandler()(rw, req, fmt.Errorf("internal error: 101 switching protocols response with non-writable body"));
                return ;
            }

            var backConnCloseCh = make_channel<bool>();
            go_(() => () =>
            { 
                // Ensure that the cancelation of a request closes the backend.
                // See issue https://golang.org/issue/35559.
                backConn.Close();

            }());

            defer(close(backConnCloseCh));

            var (conn, brw, err) = hj.Hijack();
            if (err != null)
            {
                p.getErrorHandler()(rw, req, fmt.Errorf("Hijack failed on protocol switch: %v", err));
                return ;
            }

            defer(conn.Close());
            res.Body = null; // so res.Write only writes the headers; we have res.Body in backConn above
            {
                var err__prev1 = err;

                var err = res.Write(brw);

                if (err != null)
                {
                    p.getErrorHandler()(rw, req, fmt.Errorf("response write: %v", err));
                    return ;
                }

                err = err__prev1;

            }

            {
                var err__prev1 = err;

                err = brw.Flush();

                if (err != null)
                {
                    p.getErrorHandler()(rw, req, fmt.Errorf("response flush: %v", err));
                    return ;
                }

                err = err__prev1;

            }

            var errc = make_channel<error>(1L);
            switchProtocolCopier spc = new switchProtocolCopier(user:conn,backend:backConn);
            go_(() => spc.copyToBackend(errc));
            go_(() => spc.copyFromBackend(errc));
            errc.Receive();
            return ;

        });

        // switchProtocolCopier exists so goroutines proxying data back and
        // forth have nice names in stacks.
        private partial struct switchProtocolCopier
        {
            public io.ReadWriter user;
            public io.ReadWriter backend;
        }

        private static void copyFromBackend(this switchProtocolCopier c, channel<error> errc)
        {
            var (_, err) = io.Copy(c.user, c.backend);
            errc.Send(err);
        }

        private static void copyToBackend(this switchProtocolCopier c, channel<error> errc)
        {
            var (_, err) = io.Copy(c.backend, c.user);
            errc.Send(err);
        }
    }
}}}
