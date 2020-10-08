// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Implementation of Server

// package httptest -- go2cs converted at 2020 October 08 03:41:27 UTC
// import "net/http/httptest" ==> using httptest = go.net.http.httptest_package
// Original source: C:\Go\src\net\http\httptest\server.go
using tls = go.crypto.tls_package;
using x509 = go.crypto.x509_package;
using flag = go.flag_package;
using fmt = go.fmt_package;
using log = go.log_package;
using net = go.net_package;
using http = go.net.http_package;
using @internal = go.net.http.@internal_package;
using os = go.os_package;
using strings = go.strings_package;
using sync = go.sync_package;
using time = go.time_package;
using static go.builtin;
using System.Threading;
using System;

namespace go {
namespace net {
namespace http
{
    public static partial class httptest_package
    {
        // A Server is an HTTP server listening on a system-chosen port on the
        // local loopback interface, for use in end-to-end HTTP tests.
        public partial struct Server
        {
            public @string URL; // base URL of form http://ipaddr:port with no trailing slash
            public net.Listener Listener; // EnableHTTP2 controls whether HTTP/2 is enabled
// on the server. It must be set between calling
// NewUnstartedServer and calling Server.StartTLS.
            public bool EnableHTTP2; // TLS is the optional TLS configuration, populated with a new config
// after TLS is started. If set on an unstarted server before StartTLS
// is called, existing fields are copied into the new config.
            public ptr<tls.Config> TLS; // Config may be changed after calling NewUnstartedServer and
// before Start or StartTLS.
            public ptr<http.Server> Config; // certificate is a parsed version of the TLS config certificate, if present.
            public ptr<x509.Certificate> certificate; // wg counts the number of outstanding HTTP requests on this server.
// Close blocks until all requests are finished.
            public sync.WaitGroup wg;
            public sync.Mutex mu; // guards closed and conns
            public bool closed;
            public map<net.Conn, http.ConnState> conns; // except terminal states

// client is configured for use with the server.
// Its transport is automatically closed when Close is called.
            public ptr<http.Client> client;
        }

        private static net.Listener newLocalListener() => func((_, panic, __) =>
        {
            if (serveFlag != "")
            {
                var (l, err) = net.Listen("tcp", serveFlag);
                if (err != null)
                {
                    panic(fmt.Sprintf("httptest: failed to listen on %v: %v", serveFlag, err));
                }

                return l;

            }

            (l, err) = net.Listen("tcp", "127.0.0.1:0");
            if (err != null)
            {
                l, err = net.Listen("tcp6", "[::1]:0");

                if (err != null)
                {
                    panic(fmt.Sprintf("httptest: failed to listen on a port: %v", err));
                }

            }

            return l;

        });

        // When debugging a particular http server-based test,
        // this flag lets you run
        //    go test -run=BrokenTest -httptest.serve=127.0.0.1:8000
        // to start the broken server so you can interact with it manually.
        // We only register this flag if it looks like the caller knows about it
        // and is trying to use it as we don't want to pollute flags and this
        // isn't really part of our API. Don't depend on this.
        private static @string serveFlag = default;

        private static void init()
        {
            if (strSliceContainsPrefix(os.Args, "-httptest.serve=") || strSliceContainsPrefix(os.Args, "--httptest.serve="))
            {
                flag.StringVar(_addr_serveFlag, "httptest.serve", "", "if non-empty, httptest.NewServer serves on this address and blocks.");
            }

        }

        private static bool strSliceContainsPrefix(slice<@string> v, @string pre)
        {
            foreach (var (_, s) in v)
            {
                if (strings.HasPrefix(s, pre))
                {
                    return true;
                }

            }
            return false;

        }

        // NewServer starts and returns a new Server.
        // The caller should call Close when finished, to shut it down.
        public static ptr<Server> NewServer(http.Handler handler)
        {
            var ts = NewUnstartedServer(handler);
            ts.Start();
            return _addr_ts!;
        }

        // NewUnstartedServer returns a new Server but doesn't start it.
        //
        // After changing its configuration, the caller should call Start or
        // StartTLS.
        //
        // The caller should call Close when finished, to shut it down.
        public static ptr<Server> NewUnstartedServer(http.Handler handler)
        {
            return addr(new Server(Listener:newLocalListener(),Config:&http.Server{Handler:handler},));
        }

        // Start starts a server from NewUnstartedServer.
        private static void Start(this ptr<Server> _addr_s) => func((_, panic, __) =>
        {
            ref Server s = ref _addr_s.val;

            if (s.URL != "")
            {
                panic("Server already started");
            }

            if (s.client == null)
            {
                s.client = addr(new http.Client(Transport:&http.Transport{}));
            }

            s.URL = "http://" + s.Listener.Addr().String();
            s.wrap();
            s.goServe();
            if (serveFlag != "")
            {
                fmt.Fprintln(os.Stderr, "httptest: serving on", s.URL);
            }

        });

        // StartTLS starts TLS on a server from NewUnstartedServer.
        private static void StartTLS(this ptr<Server> _addr_s) => func((_, panic, __) =>
        {
            ref Server s = ref _addr_s.val;

            if (s.URL != "")
            {
                panic("Server already started");
            }

            if (s.client == null)
            {
                s.client = addr(new http.Client(Transport:&http.Transport{}));
            }

            var (cert, err) = tls.X509KeyPair(@internal.LocalhostCert, @internal.LocalhostKey);
            if (err != null)
            {
                panic(fmt.Sprintf("httptest: NewTLSServer: %v", err));
            }

            var existingConfig = s.TLS;
            if (existingConfig != null)
            {
                s.TLS = existingConfig.Clone();
            }
            else
            {
                s.TLS = @new<tls.Config>();
            }

            if (s.TLS.NextProtos == null)
            {
                @string nextProtos = new slice<@string>(new @string[] { "http/1.1" });
                if (s.EnableHTTP2)
                {
                    nextProtos = new slice<@string>(new @string[] { "h2" });
                }

                s.TLS.NextProtos = nextProtos;

            }

            if (len(s.TLS.Certificates) == 0L)
            {
                s.TLS.Certificates = new slice<tls.Certificate>(new tls.Certificate[] { cert });
            }

            s.certificate, err = x509.ParseCertificate(s.TLS.Certificates[0L].Certificate[0L]);
            if (err != null)
            {
                panic(fmt.Sprintf("httptest: NewTLSServer: %v", err));
            }

            var certpool = x509.NewCertPool();
            certpool.AddCert(s.certificate);
            s.client.Transport = addr(new http.Transport(TLSClientConfig:&tls.Config{RootCAs:certpool,},ForceAttemptHTTP2:s.EnableHTTP2,));
            s.Listener = tls.NewListener(s.Listener, s.TLS);
            s.URL = "https://" + s.Listener.Addr().String();
            s.wrap();
            s.goServe();

        });

        // NewTLSServer starts and returns a new Server using TLS.
        // The caller should call Close when finished, to shut it down.
        public static ptr<Server> NewTLSServer(http.Handler handler)
        {
            var ts = NewUnstartedServer(handler);
            ts.StartTLS();
            return _addr_ts!;
        }

        private partial interface closeIdleTransport
        {
            void CloseIdleConnections();
        }

        // Close shuts down the server and blocks until all outstanding
        // requests on this server have completed.
        private static void Close(this ptr<Server> _addr_s) => func((defer, _, __) =>
        {
            ref Server s = ref _addr_s.val;

            s.mu.Lock();
            if (!s.closed)
            {
                s.closed = true;
                s.Listener.Close();
                s.Config.SetKeepAlivesEnabled(false);
                foreach (var (c, st) in s.conns)
                { 
                    // Force-close any idle connections (those between
                    // requests) and new connections (those which connected
                    // but never sent a request). StateNew connections are
                    // super rare and have only been seen (in
                    // previously-flaky tests) in the case of
                    // socket-late-binding races from the http Client
                    // dialing this server and then getting an idle
                    // connection before the dial completed. There is thus
                    // a connected connection in StateNew with no
                    // associated Request. We only close StateIdle and
                    // StateNew because they're not doing anything. It's
                    // possible StateNew is about to do something in a few
                    // milliseconds, but a previous CL to check again in a
                    // few milliseconds wasn't liked (early versions of
                    // https://golang.org/cl/15151) so now we just
                    // forcefully close StateNew. The docs for Server.Close say
                    // we wait for "outstanding requests", so we don't close things
                    // in StateActive.
                    if (st == http.StateIdle || st == http.StateNew)
                    {
                        s.closeConn(c);
                    }

                } 
                // If this server doesn't shut down in 5 seconds, tell the user why.
                var t = time.AfterFunc(5L * time.Second, s.logCloseHangDebugInfo);
                defer(t.Stop());

            }

            s.mu.Unlock(); 

            // Not part of httptest.Server's correctness, but assume most
            // users of httptest.Server will be using the standard
            // transport, so help them out and close any idle connections for them.
            {
                var t__prev1 = t;

                closeIdleTransport (t, ok) = closeIdleTransport.As(http.DefaultTransport._<closeIdleTransport>())!;

                if (ok)
                {
                    t.CloseIdleConnections();
                } 

                // Also close the client idle connections.

                t = t__prev1;

            } 

            // Also close the client idle connections.
            if (s.client != null)
            {
                {
                    var t__prev2 = t;

                    (t, ok) = closeIdleTransport.As(s.client.Transport._<closeIdleTransport>())!;

                    if (ok)
                    {
                        t.CloseIdleConnections();
                    }

                    t = t__prev2;

                }

            }

            s.wg.Wait();

        });

        private static void logCloseHangDebugInfo(this ptr<Server> _addr_s) => func((defer, _, __) =>
        {
            ref Server s = ref _addr_s.val;

            s.mu.Lock();
            defer(s.mu.Unlock());
            ref strings.Builder buf = ref heap(out ptr<strings.Builder> _addr_buf);
            buf.WriteString("httptest.Server blocked in Close after 5 seconds, waiting for connections:\n");
            foreach (var (c, st) in s.conns)
            {
                fmt.Fprintf(_addr_buf, "  %T %p %v in state %v\n", c, c, c.RemoteAddr(), st);
            }
            log.Print(buf.String());

        });

        // CloseClientConnections closes any open HTTP connections to the test Server.
        private static void CloseClientConnections(this ptr<Server> _addr_s) => func((defer, _, __) =>
        {
            ref Server s = ref _addr_s.val;

            s.mu.Lock();
            var nconn = len(s.conns);
            var ch = make_channel<object>(nconn);
            foreach (var (c) in s.conns)
            {
                go_(() => s.closeConnChan(c, ch));
            }
            s.mu.Unlock(); 

            // Wait for outstanding closes to finish.
            //
            // Out of paranoia for making a late change in Go 1.6, we
            // bound how long this can wait, since golang.org/issue/14291
            // isn't fully understood yet. At least this should only be used
            // in tests.
            var timer = time.NewTimer(5L * time.Second);
            defer(timer.Stop());
            for (long i = 0L; i < nconn; i++)
            {
                return ;
            }


        });

        // Certificate returns the certificate used by the server, or nil if
        // the server doesn't use TLS.
        private static ptr<x509.Certificate> Certificate(this ptr<Server> _addr_s)
        {
            ref Server s = ref _addr_s.val;

            return _addr_s.certificate!;
        }

        // Client returns an HTTP client configured for making requests to the server.
        // It is configured to trust the server's TLS test certificate and will
        // close its idle connections on Server.Close.
        private static ptr<http.Client> Client(this ptr<Server> _addr_s)
        {
            ref Server s = ref _addr_s.val;

            return _addr_s.client!;
        }

        private static void goServe(this ptr<Server> _addr_s) => func((defer, _, __) =>
        {
            ref Server s = ref _addr_s.val;

            s.wg.Add(1L);
            go_(() => () =>
            {
                defer(s.wg.Done());
                s.Config.Serve(s.Listener);
            }());

        });

        // wrap installs the connection state-tracking hook to know which
        // connections are idle.
        private static void wrap(this ptr<Server> _addr_s) => func((defer, panic, _) =>
        {
            ref Server s = ref _addr_s.val;

            var oldHook = s.Config.ConnState;
            s.Config.ConnState = (c, cs) =>
            {
                s.mu.Lock();
                defer(s.mu.Unlock());

                if (cs == http.StateNew) 
                    s.wg.Add(1L);
                    {
                        var (_, exists) = s.conns[c];

                        if (exists)
                        {
                            panic("invalid state transition");
                        }

                    }

                    if (s.conns == null)
                    {
                        s.conns = make_map<net.Conn, http.ConnState>();
                    }

                    s.conns[c] = cs;
                    if (s.closed)
                    { 
                        // Probably just a socket-late-binding dial from
                        // the default transport that lost the race (and
                        // thus this connection is now idle and will
                        // never be used).
                        s.closeConn(c);

                    }

                else if (cs == http.StateActive) 
                    {
                        var oldState__prev1 = oldState;

                        var (oldState, ok) = s.conns[c];

                        if (ok)
                        {
                            if (oldState != http.StateNew && oldState != http.StateIdle)
                            {
                                panic("invalid state transition");
                            }

                            s.conns[c] = cs;

                        }

                        oldState = oldState__prev1;

                    }

                else if (cs == http.StateIdle) 
                    {
                        var oldState__prev1 = oldState;

                        (oldState, ok) = s.conns[c];

                        if (ok)
                        {
                            if (oldState != http.StateActive)
                            {
                                panic("invalid state transition");
                            }

                            s.conns[c] = cs;

                        }

                        oldState = oldState__prev1;

                    }

                    if (s.closed)
                    {
                        s.closeConn(c);
                    }

                else if (cs == http.StateHijacked || cs == http.StateClosed) 
                    s.forgetConn(c);
                                if (oldHook != null)
                {
                    oldHook(c, cs);
                }

            }
;

        });

        // closeConn closes c.
        // s.mu must be held.
        private static void closeConn(this ptr<Server> _addr_s, net.Conn c)
        {
            ref Server s = ref _addr_s.val;

            s.closeConnChan(c, null);
        }

        // closeConnChan is like closeConn, but takes an optional channel to receive a value
        // when the goroutine closing c is done.
        private static void closeConnChan(this ptr<Server> _addr_s, net.Conn c, channel<object> done)
        {
            ref Server s = ref _addr_s.val;

            c.Close();
            if (done != null)
            {
                done.Send(/* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{});
            }

        }

        // forgetConn removes c from the set of tracked conns and decrements it from the
        // waitgroup, unless it was previously removed.
        // s.mu must be held.
        private static void forgetConn(this ptr<Server> _addr_s, net.Conn c)
        {
            ref Server s = ref _addr_s.val;

            {
                var (_, ok) = s.conns[c];

                if (ok)
                {
                    delete(s.conns, c);
                    s.wg.Done();
                }

            }

        }
    }
}}}
