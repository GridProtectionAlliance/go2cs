// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package httptrace provides mechanisms to trace the events within
// HTTP client requests.
// package httptrace -- go2cs converted at 2020 October 08 03:39:22 UTC
// import "net/http/httptrace" ==> using httptrace = go.net.http.httptrace_package
// Original source: C:\Go\src\net\http\httptrace\trace.go
using context = go.context_package;
using tls = go.crypto.tls_package;
using nettrace = go.@internal.nettrace_package;
using net = go.net_package;
using textproto = go.net.textproto_package;
using reflect = go.reflect_package;
using time = go.time_package;
using static go.builtin;
using System;

namespace go {
namespace net {
namespace http
{
    public static partial class httptrace_package
    {
        // unique type to prevent assignment.
        private partial struct clientEventContextKey
        {
        }

        // ContextClientTrace returns the ClientTrace associated with the
        // provided context. If none, it returns nil.
        public static ptr<ClientTrace> ContextClientTrace(context.Context ctx)
        {
            ptr<ClientTrace> (trace, _) = ctx.Value(new clientEventContextKey())._<ptr<ClientTrace>>();
            return _addr_trace!;
        }

        // WithClientTrace returns a new context based on the provided parent
        // ctx. HTTP client requests made with the returned context will use
        // the provided trace hooks, in addition to any previous hooks
        // registered with ctx. Any hooks defined in the provided trace will
        // be called first.
        public static context.Context WithClientTrace(context.Context ctx, ptr<ClientTrace> _addr_trace) => func((_, panic, __) =>
        {
            ref ClientTrace trace = ref _addr_trace.val;

            if (trace == null)
            {
                panic("nil trace");
            }

            var old = ContextClientTrace(ctx);
            trace.compose(old);

            ctx = context.WithValue(ctx, new clientEventContextKey(), trace);
            if (trace.hasNetHooks())
            {
                ptr<nettrace.Trace> nt = addr(new nettrace.Trace(ConnectStart:trace.ConnectStart,ConnectDone:trace.ConnectDone,));
                if (trace.DNSStart != null)
                {
                    nt.DNSStart = name =>
                    {
                        trace.DNSStart(new DNSStartInfo(Host:name));
                    }
;

                }

                if (trace.DNSDone != null)
                {
                    nt.DNSDone = (netIPs, coalesced, err) =>
                    {
                        var addrs = make_slice<net.IPAddr>(len(netIPs));
                        foreach (var (i, ip) in netIPs)
                        {
                            addrs[i] = ip._<net.IPAddr>();
                        }
                        trace.DNSDone(new DNSDoneInfo(Addrs:addrs,Coalesced:coalesced,Err:err,));

                    }
;

                }

                ctx = context.WithValue(ctx, new nettrace.TraceKey(), nt);

            }

            return ctx;

        });

        // ClientTrace is a set of hooks to run at various stages of an outgoing
        // HTTP request. Any particular hook may be nil. Functions may be
        // called concurrently from different goroutines and some may be called
        // after the request has completed or failed.
        //
        // ClientTrace currently traces a single HTTP request & response
        // during a single round trip and has no hooks that span a series
        // of redirected requests.
        //
        // See https://blog.golang.org/http-tracing for more.
        public partial struct ClientTrace
        {
            public Action<@string> GetConn; // GotConn is called after a successful connection is
// obtained. There is no hook for failure to obtain a
// connection; instead, use the error from
// Transport.RoundTrip.
            public Action<GotConnInfo> GotConn; // PutIdleConn is called when the connection is returned to
// the idle pool. If err is nil, the connection was
// successfully returned to the idle pool. If err is non-nil,
// it describes why not. PutIdleConn is not called if
// connection reuse is disabled via Transport.DisableKeepAlives.
// PutIdleConn is called before the caller's Response.Body.Close
// call returns.
// For HTTP/2, this hook is not currently used.
            public Action<error> PutIdleConn; // GotFirstResponseByte is called when the first byte of the response
// headers is available.
            public Action GotFirstResponseByte; // Got100Continue is called if the server replies with a "100
// Continue" response.
            public Action Got100Continue; // Got1xxResponse is called for each 1xx informational response header
// returned before the final non-1xx response. Got1xxResponse is called
// for "100 Continue" responses, even if Got100Continue is also defined.
// If it returns an error, the client request is aborted with that error value.
            public Func<long, textproto.MIMEHeader, error> Got1xxResponse; // DNSStart is called when a DNS lookup begins.
            public Action<DNSStartInfo> DNSStart; // DNSDone is called when a DNS lookup ends.
            public Action<DNSDoneInfo> DNSDone; // ConnectStart is called when a new connection's Dial begins.
// If net.Dialer.DualStack (IPv6 "Happy Eyeballs") support is
// enabled, this may be called multiple times.
            public Action<@string, @string> ConnectStart; // ConnectDone is called when a new connection's Dial
// completes. The provided err indicates whether the
// connection completedly successfully.
// If net.Dialer.DualStack ("Happy Eyeballs") support is
// enabled, this may be called multiple times.
            public Action<@string, @string, error> ConnectDone; // TLSHandshakeStart is called when the TLS handshake is started. When
// connecting to an HTTPS site via an HTTP proxy, the handshake happens
// after the CONNECT request is processed by the proxy.
            public Action TLSHandshakeStart; // TLSHandshakeDone is called after the TLS handshake with either the
// successful handshake's connection state, or a non-nil error on handshake
// failure.
            public Action<tls.ConnectionState, error> TLSHandshakeDone; // WroteHeaderField is called after the Transport has written
// each request header. At the time of this call the values
// might be buffered and not yet written to the network.
            public Action<@string, slice<@string>> WroteHeaderField; // WroteHeaders is called after the Transport has written
// all request headers.
            public Action WroteHeaders; // Wait100Continue is called if the Request specified
// "Expect: 100-continue" and the Transport has written the
// request headers but is waiting for "100 Continue" from the
// server before writing the request body.
            public Action Wait100Continue; // WroteRequest is called with the result of writing the
// request and any body. It may be called multiple times
// in the case of retried requests.
            public Action<WroteRequestInfo> WroteRequest;
        }

        // WroteRequestInfo contains information provided to the WroteRequest
        // hook.
        public partial struct WroteRequestInfo
        {
            public error Err;
        }

        // compose modifies t such that it respects the previously-registered hooks in old,
        // subject to the composition policy requested in t.Compose.
        private static void compose(this ptr<ClientTrace> _addr_t, ptr<ClientTrace> _addr_old)
        {
            ref ClientTrace t = ref _addr_t.val;
            ref ClientTrace old = ref _addr_old.val;

            if (old == null)
            {
                return ;
            }

            var tv = reflect.ValueOf(t).Elem();
            var ov = reflect.ValueOf(old).Elem();
            var structType = tv.Type();
            for (long i = 0L; i < structType.NumField(); i++)
            {
                var tf = tv.Field(i);
                var hookType = tf.Type();
                if (hookType.Kind() != reflect.Func)
                {
                    continue;
                }

                var of = ov.Field(i);
                if (of.IsNil())
                {
                    continue;
                }

                if (tf.IsNil())
                {
                    tf.Set(of);
                    continue;
                } 

                // Make a copy of tf for tf to call. (Otherwise it
                // creates a recursive call cycle and stack overflows)
                var tfCopy = reflect.ValueOf(tf.Interface()); 

                // We need to call both tf and of in some order.
                var newFunc = reflect.MakeFunc(hookType, args =>
                {
                    tfCopy.Call(args);
                    return of.Call(args);
                });
                tv.Field(i).Set(newFunc);

            }


        }

        // DNSStartInfo contains information about a DNS request.
        public partial struct DNSStartInfo
        {
            public @string Host;
        }

        // DNSDoneInfo contains information about the results of a DNS lookup.
        public partial struct DNSDoneInfo
        {
            public slice<net.IPAddr> Addrs; // Err is any error that occurred during the DNS lookup.
            public error Err; // Coalesced is whether the Addrs were shared with another
// caller who was doing the same DNS lookup concurrently.
            public bool Coalesced;
        }

        private static bool hasNetHooks(this ptr<ClientTrace> _addr_t)
        {
            ref ClientTrace t = ref _addr_t.val;

            if (t == null)
            {
                return false;
            }

            return t.DNSStart != null || t.DNSDone != null || t.ConnectStart != null || t.ConnectDone != null;

        }

        // GotConnInfo is the argument to the ClientTrace.GotConn function and
        // contains information about the obtained connection.
        public partial struct GotConnInfo
        {
            public net.Conn Conn; // Reused is whether this connection has been previously
// used for another HTTP request.
            public bool Reused; // WasIdle is whether this connection was obtained from an
// idle pool.
            public bool WasIdle; // IdleTime reports how long the connection was previously
// idle, if WasIdle is true.
            public time.Duration IdleTime;
        }
    }
}}}
