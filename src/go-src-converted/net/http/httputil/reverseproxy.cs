// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// HTTP reverse proxy handler

// package httputil -- go2cs converted at 2020 August 29 08:34:26 UTC
// import "net/http/httputil" ==> using httputil = go.net.http.httputil_package
// Original source: C:\Go\src\net\http\httputil\reverseproxy.go
using context = go.context_package;
using io = go.io_package;
using log = go.log_package;
using net = go.net_package;
using http = go.net.http_package;
using url = go.net.url_package;
using strings = go.strings_package;
using sync = go.sync_package;
using time = go.time_package;
using static go.builtin;
using System;
using System.Threading;

namespace go {
namespace net {
namespace http
{
    public static partial class httputil_package
    {
        // onExitFlushLoop is a callback set by tests to detect the state of the
        // flushLoop() goroutine.
        private static Action onExitFlushLoop = default;

        // ReverseProxy is an HTTP Handler that takes an incoming request and
        // sends it to another server, proxying the response back to the
        // client.
        public partial struct ReverseProxy
        {
            public Action<ref http.Request> Director; // The transport used to perform proxy requests.
// If nil, http.DefaultTransport is used.
            public http.RoundTripper Transport; // FlushInterval specifies the flush interval
// to flush to the client while copying the
// response body.
// If zero, no periodic flushing is done.
            public time.Duration FlushInterval; // ErrorLog specifies an optional logger for errors
// that occur when attempting to proxy the request.
// If nil, logging goes to os.Stderr via the log package's
// standard logger.
            public ptr<log.Logger> ErrorLog; // BufferPool optionally specifies a buffer pool to
// get byte slices for use by io.CopyBuffer when
// copying HTTP response bodies.
            public BufferPool BufferPool; // ModifyResponse is an optional function that
// modifies the Response from the backend.
// If it returns an error, the proxy returns a StatusBadGateway error.
            public Func<ref http.Response, error> ModifyResponse;
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

        // NewSingleHostReverseProxy returns a new ReverseProxy that routes
        // URLs to the scheme, host, and base path provided in target. If the
        // target's path is "/base" and the incoming request was for "/dir",
        // the target request will be for /base/dir.
        // NewSingleHostReverseProxy does not rewrite the Host header.
        // To rewrite Host headers, use ReverseProxy directly with a custom
        // Director policy.
        public static ref ReverseProxy NewSingleHostReverseProxy(ref url.URL target)
        {
            var targetQuery = target.RawQuery;
            Action<ref http.Request> director = req =>
            {
                req.URL.Scheme = target.Scheme;
                req.URL.Host = target.Host;
                req.URL.Path = singleJoiningSlash(target.Path, req.URL.Path);
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
            return ref new ReverseProxy(Director:director);
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

        private static http.Header cloneHeader(http.Header h)
        {
            var h2 = make(http.Header, len(h));
            foreach (var (k, vv) in h)
            {
                var vv2 = make_slice<@string>(len(vv));
                copy(vv2, vv);
                h2[k] = vv2;
            }
            return h2;
        }

        // Hop-by-hop headers. These are removed when sent to the backend.
        // http://www.w3.org/Protocols/rfc2616/rfc2616-sec13.html
        private static @string hopHeaders = new slice<@string>(new @string[] { "Connection", "Proxy-Connection", "Keep-Alive", "Proxy-Authenticate", "Proxy-Authorization", "Te", "Trailer", "Transfer-Encoding", "Upgrade" });

        private static void ServeHTTP(this ref ReverseProxy _p, http.ResponseWriter rw, ref http.Request _req) => func(_p, _req, (ref ReverseProxy p, ref http.Request req, Defer defer, Panic _, Recover __) =>
        {
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

            var outreq = req.WithContext(ctx); // includes shallow copies of maps, but okay
            if (req.ContentLength == 0L)
            {
                outreq.Body = null; // Issue 16036: nil Body for http.Transport retries
            }
            outreq.Header = cloneHeader(req.Header);

            p.Director(outreq);
            outreq.Close = false;

            removeConnectionHeaders(outreq.Header); 

            // Remove hop-by-hop headers to the backend. Especially
            // important is "Connection" because we want a persistent
            // connection, regardless of what the client sent to us.
            {
                var h__prev1 = h;

                foreach (var (_, __h) in hopHeaders)
                {
                    h = __h;
                    if (outreq.Header.Get(h) != "")
                    {
                        outreq.Header.Del(h);
                    }
                }

                h = h__prev1;
            }

            {
                var (clientIP, _, err) = net.SplitHostPort(req.RemoteAddr);

                if (err == null)
                { 
                    // If we aren't the first proxy retain prior
                    // X-Forwarded-For information as a comma+space
                    // separated list and fold multiple headers into one.
                    {
                        var (prior, ok) = outreq.Header["X-Forwarded-For"];

                        if (ok)
                        {
                            clientIP = strings.Join(prior, ", ") + ", " + clientIP;
                        }

                    }
                    outreq.Header.Set("X-Forwarded-For", clientIP);
                }

            }

            var (res, err) = transport.RoundTrip(outreq);
            if (err != null)
            {
                p.logf("http: proxy error: %v", err);
                rw.WriteHeader(http.StatusBadGateway);
                return;
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

            if (p.ModifyResponse != null)
            {
                {
                    var err = p.ModifyResponse(res);

                    if (err != null)
                    {
                        p.logf("http: proxy error: %v", err);
                        rw.WriteHeader(http.StatusBadGateway);
                        res.Body.Close();
                        return;
                    }

                }
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
            p.copyResponse(rw, res.Body);
            res.Body.Close(); // close now, instead of defer, to populate res.Trailer

            if (len(res.Trailer) == announcedTrailers)
            {
                copyHeader(rw.Header(), res.Trailer);
                return;
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

        // removeConnectionHeaders removes hop-by-hop headers listed in the "Connection" header of h.
        // See RFC 2616, section 14.10.
        private static void removeConnectionHeaders(http.Header h)
        {
            {
                var c = h.Get("Connection");

                if (c != "")
                {
                    foreach (var (_, f) in strings.Split(c, ","))
                    {
                        f = strings.TrimSpace(f);

                        if (f != "")
                        {
                            h.Del(f);
                        }
                    }
                }

            }
        }

        private static void copyResponse(this ref ReverseProxy _p, io.Writer dst, io.Reader src) => func(_p, (ref ReverseProxy p, Defer defer, Panic _, Recover __) =>
        {
            if (p.FlushInterval != 0L)
            {
                {
                    writeFlusher (wf, ok) = dst._<writeFlusher>();

                    if (ok)
                    {
                        maxLatencyWriter mlw = ref new maxLatencyWriter(dst:wf,latency:p.FlushInterval,done:make(chanbool),);
                        go_(() => mlw.flushLoop());
                        defer(mlw.stop());
                        dst = mlw;
                    }

                }
            }
            slice<byte> buf = default;
            if (p.BufferPool != null)
            {
                buf = p.BufferPool.Get();
            }
            p.copyBuffer(dst, src, buf);
            if (p.BufferPool != null)
            {
                p.BufferPool.Put(buf);
            }
        });

        private static (long, error) copyBuffer(this ref ReverseProxy p, io.Writer dst, io.Reader src, slice<byte> buf)
        {
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
                        return (written, werr);
                    }
                    if (nr != nw)
                    {
                        return (written, io.ErrShortWrite);
                    }
                }
                if (rerr != null)
                {
                    return (written, rerr);
                }
            }

        }

        private static void logf(this ref ReverseProxy p, @string format, params object[] args)
        {
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
            public time.Duration latency;
            public sync.Mutex mu; // protects Write + Flush
            public channel<bool> done;
        }

        private static (long, error) Write(this ref maxLatencyWriter _m, slice<byte> p) => func(_m, (ref maxLatencyWriter m, Defer defer, Panic _, Recover __) =>
        {
            m.mu.Lock();
            defer(m.mu.Unlock());
            return m.dst.Write(p);
        });

        private static void flushLoop(this ref maxLatencyWriter _m) => func(_m, (ref maxLatencyWriter m, Defer defer, Panic _, Recover __) =>
        {
            var t = time.NewTicker(m.latency);
            defer(t.Stop());
            while (true)
            {
                if (onExitFlushLoop != null)
                {
                    onExitFlushLoop();
                }
                return;
                m.mu.Lock();
                m.dst.Flush();
                m.mu.Unlock();
            }

        });

        private static void stop(this ref maxLatencyWriter m)
        {
            m.done.Send(true);

        }
    }
}}}
