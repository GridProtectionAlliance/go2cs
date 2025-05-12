// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Implementation of Server
namespace go.net.http;

using tls = crypto.tls_package;
using x509 = crypto.x509_package;
using flag = flag_package;
using fmt = fmt_package;
using log = log_package;
using net = net_package;
using http = net.http_package;
using testcert = net.http.@internal.testcert_package;
using os = os_package;
using strings = strings_package;
using sync = sync_package;
using time = time_package;
using crypto;
using net;
using net.http.@internal;

partial class httptest_package {

// A Server is an HTTP server listening on a system-chosen port on the
// local loopback interface, for use in end-to-end HTTP tests.
[GoType] partial struct Server {
    public @string URL; // base URL of form http://ipaddr:port with no trailing slash
    public net_package.Listener Listener;
    // EnableHTTP2 controls whether HTTP/2 is enabled
    // on the server. It must be set between calling
    // NewUnstartedServer and calling Server.StartTLS.
    public bool EnableHTTP2;
    // TLS is the optional TLS configuration, populated with a new config
    // after TLS is started. If set on an unstarted server before StartTLS
    // is called, existing fields are copied into the new config.
    public ж<crypto.tls_package.Config> TLS;
    // Config may be changed after calling NewUnstartedServer and
    // before Start or StartTLS.
    public ж<net.http_package.Server> Config;
    // certificate is a parsed version of the TLS config certificate, if present.
    internal ж<crypto.x509_package.Certificate> certificate;
    // wg counts the number of outstanding HTTP requests on this server.
    // Close blocks until all requests are finished.
    internal sync_package.WaitGroup wg;
    internal sync_package.Mutex mu; // guards closed and conns
    internal bool closed;
    internal http.ConnState conns; // except terminal states
    // client is configured for use with the server.
    // Its transport is automatically closed when Close is called.
    internal ж<net.http_package.Client> client;
}

internal static net.Listener newLocalListener() {
    if (serveFlag != ""u8) {
        (lΔ1, errΔ1) = net.Listen("tcp"u8, serveFlag);
        if (errΔ1 != default!) {
            throw panic(fmt.Sprintf("httptest: failed to listen on %v: %v"u8, serveFlag, errΔ1));
        }
        return lΔ1;
    }
    (l, err) = net.Listen("tcp"u8, "127.0.0.1:0"u8);
    if (err != default!) {
        {
            (l, err) = net.Listen("tcp6"u8, "[::1]:0"u8); if (err != default!) {
                throw panic(fmt.Sprintf("httptest: failed to listen on a port: %v"u8, err));
            }
        }
    }
    return l;
}

// When debugging a particular http server-based test,
// this flag lets you run
//
//	go test -run='^BrokenTest$' -httptest.serve=127.0.0.1:8000
//
// to start the broken server so you can interact with it manually.
// We only register this flag if it looks like the caller knows about it
// and is trying to use it as we don't want to pollute flags and this
// isn't really part of our API. Don't depend on this.
internal static @string serveFlag;

[GoInit] internal static void init() {
    if (strSliceContainsPrefix(os.Args, "-httptest.serve="u8) || strSliceContainsPrefix(os.Args, "--httptest.serve="u8)) {
        flag.StringVar(Ꮡ(serveFlag), "httptest.serve"u8, ""u8, "if non-empty, httptest.NewServer serves on this address and blocks."u8);
    }
}

internal static bool strSliceContainsPrefix(slice<@string> v, @string pre) {
    foreach (var (_, s) in v) {
        if (strings.HasPrefix(s, pre)) {
            return true;
        }
    }
    return false;
}

// NewServer starts and returns a new [Server].
// The caller should call Close when finished, to shut it down.
public static ж<Server> NewServer(httpꓸHandler handler) {
    var ts = NewUnstartedServer(handler);
    ts.Start();
    return ts;
}

// NewUnstartedServer returns a new [Server] but doesn't start it.
//
// After changing its configuration, the caller should call Start or
// StartTLS.
//
// The caller should call Close when finished, to shut it down.
public static ж<Server> NewUnstartedServer(httpꓸHandler handler) {
    return Ꮡ(new Server(
        Listener: newLocalListener(),
        Config: Ꮡ(new http.Server(Handler: handler))
    ));
}

// Start starts a server from NewUnstartedServer.
[GoRecv] public static void Start(this ref Server s) {
    if (s.URL != ""u8) {
        throw panic("Server already started");
    }
    if (s.client == nil) {
        s.client = Ꮡ(new http.Client(Transport: Ꮡ(new http.Transport(nil))));
    }
    s.URL = "http://"u8 + s.Listener.Addr().String();
    s.wrap();
    s.goServe();
    if (serveFlag != ""u8) {
        fmt.Fprintln(~os.Stderr, "httptest: serving on", s.URL);
        switch (select()) {
}
    }
}

// StartTLS starts TLS on a server from NewUnstartedServer.
[GoRecv] public static void StartTLS(this ref Server s) {
    if (s.URL != ""u8) {
        throw panic("Server already started");
    }
    if (s.client == nil) {
        s.client = Ꮡ(new http.Client(nil));
    }
    var (cert, err) = tls.X509KeyPair(testcert.LocalhostCert, testcert.LocalhostKey);
    if (err != default!) {
        throw panic(fmt.Sprintf("httptest: NewTLSServer: %v"u8, err));
    }
    var existingConfig = s.TLS;
    if (existingConfig != nil){
        s.TLS = existingConfig.Clone();
    } else {
        s.TLS = @new<tls.Config>();
    }
    if (s.TLS.NextProtos == default!) {
        var nextProtos = new @string[]{"http/1.1"}.slice();
        if (s.EnableHTTP2) {
            nextProtos = new @string[]{"h2"}.slice();
        }
        s.TLS.NextProtos = nextProtos;
    }
    if (len(s.TLS.Certificates) == 0) {
        s.TLS.Certificates = new tls.Certificate[]{cert}.slice();
    }
    (s.certificate, err) = x509.ParseCertificate(s.TLS.Certificates[0].Certificate[0]);
    if (err != default!) {
        throw panic(fmt.Sprintf("httptest: NewTLSServer: %v"u8, err));
    }
    var certpool = x509.NewCertPool();
    certpool.AddCert(s.certificate);
    s.client.Transport = Ꮡ(new http.Transport(
        TLSClientConfig: Ꮡ(new tls.Config(
            RootCAs: certpool
        )),
        ForceAttemptHTTP2: s.EnableHTTP2
    ));
    s.Listener = tls.NewListener(s.Listener, s.TLS);
    s.URL = "https://"u8 + s.Listener.Addr().String();
    s.wrap();
    s.goServe();
}

// NewTLSServer starts and returns a new [Server] using TLS.
// The caller should call Close when finished, to shut it down.
public static ж<Server> NewTLSServer(httpꓸHandler handler) {
    var ts = NewUnstartedServer(handler);
    ts.StartTLS();
    return ts;
}

[GoType] partial interface closeIdleTransport {
    void CloseIdleConnections();
}

// Close shuts down the server and blocks until all outstanding
// requests on this server have completed.
[GoRecv] public static void Close(this ref Server s) => func((defer, _) => {
    s.mu.Lock();
    if (!s.closed) {
        s.closed = true;
        s.Listener.Close();
        s.Config.SetKeepAlivesEnabled(false);
        foreach (var (c, st) in s.conns) {
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
            if (st == http.StateIdle || st == http.StateNew) {
                s.closeConn(c);
            }
        }
        // If this server doesn't shut down in 5 seconds, tell the user why.
        var t = time.AfterFunc(5 * time.ΔSecond, s.logCloseHangDebugInfo);
        var tʗ1 = t;
        defer(tʗ1.Stop);
    }
    s.mu.Unlock();
    // Not part of httptest.Server's correctness, but assume most
    // users of httptest.Server will be using the standard
    // transport, so help them out and close any idle connections for them.
    {
        var (t, ok) = http.DefaultTransport._<closeIdleTransport>(ᐧ); if (ok) {
            t.CloseIdleConnections();
        }
    }
    // Also close the client idle connections.
    if (s.client != nil) {
        {
            var (t, ok) = s.client.Transport._<closeIdleTransport>(ᐧ); if (ok) {
                t.CloseIdleConnections();
            }
        }
    }
    s.wg.Wait();
});

[GoRecv] internal static void logCloseHangDebugInfo(this ref Server s) => func((defer, _) => {
    s.mu.Lock();
    defer(s.mu.Unlock);
    ref var buf = ref heap(new strings_package.Builder(), out var Ꮡbuf);
    buf.WriteString("httptest.Server blocked in Close after 5 seconds, waiting for connections:\n"u8);
    foreach (var (c, st) in s.conns) {
        fmt.Fprintf(~Ꮡbuf, "  %T %p %v in state %v\n"u8, c, c, c.RemoteAddr(), st);
    }
    log.Print(buf.String());
});

// CloseClientConnections closes any open HTTP connections to the test Server.
[GoRecv] public static void CloseClientConnections(this ref Server s) => func((defer, _) => {
    s.mu.Lock();
    nint nconn = len(s.conns);
    var ch = new channel<EmptyStruct>(nconn);
    foreach (var (c, _) in s.conns) {
        goǃ(s.closeConnChan, c, ch);
    }
    s.mu.Unlock();
    // Wait for outstanding closes to finish.
    //
    // Out of paranoia for making a late change in Go 1.6, we
    // bound how long this can wait, since golang.org/issue/14291
    // isn't fully understood yet. At least this should only be used
    // in tests.
    var timer = time.NewTimer(5 * time.ΔSecond);
    var timerʗ1 = timer;
    defer(timerʗ1.Stop);
    for (nint i = 0; i < nconn; i++) {
        switch (select(ᐸꟷ(ch, ꓸꓸꓸ), ᐸꟷ((~timer).C, ꓸꓸꓸ))) {
        case 0 when ch.ꟷᐳ(out _): {
            break;
        }
        case 1 when (~timer).C.ꟷᐳ(out _): {
            return;
        }}
    }
});

// Too slow. Give up.

// Certificate returns the certificate used by the server, or nil if
// the server doesn't use TLS.
[GoRecv] public static ж<x509.Certificate> Certificate(this ref Server s) {
    return s.certificate;
}

// Client returns an HTTP client configured for making requests to the server.
// It is configured to trust the server's TLS test certificate and will
// close its idle connections on [Server.Close].
// Use Server.URL as the base URL to send requests to the server.
[GoRecv] public static ж<http.Client> Client(this ref Server s) {
    return s.client;
}

[GoRecv] internal static void goServe(this ref Server s) => func((defer, _) => {
    s.wg.Add(1);
    goǃ(() => {
        defer(s.wg.Done);
        s.Config.Serve(s.Listener);
    });
});

// wrap installs the connection state-tracking hook to know which
// connections are idle.
[GoRecv] internal static void wrap(this ref Server s) => func((defer, _) => {
    var oldHook = s.Config.ConnState;
    s.Config.ConnState = 
    var oldHookʗ1 = oldHook;
    (net.Conn c, http.ConnState cs) => {
        s.mu.Lock();
        defer(s.mu.Unlock);
        var exprᴛ1 = cs;
        if (exprᴛ1 == http.StateNew) {
            {
                http.ConnState _ = s.conns[c];
                var exists = s.conns[c]; if (exists) {
                    throw panic("invalid state transition");
                }
            }
            if (s.conns == default!) {
                s.conns = new http.ConnState();
            }
            s.wg.Add(1);
            s.conns[c] = cs;
            if (s.closed) {
                // Add c to the set of tracked conns and increment it to the
                // waitgroup.
                // Probably just a socket-late-binding dial from
                // the default transport that lost the race (and
                // thus this connection is now idle and will
                // never be used).
                s.closeConn(c);
            }
        }
        else if (exprᴛ1 == http.StateActive) {
            {
                http.ConnState oldState = s.conns[c];
                var ok = s.conns[c]; if (ok) {
                    if (oldState != http.StateNew && oldState != http.StateIdle) {
                        throw panic("invalid state transition");
                    }
                    s.conns[c] = cs;
                }
            }
        }
        else if (exprᴛ1 == http.StateIdle) {
            {
                http.ConnState oldState = s.conns[c];
                var ok = s.conns[c]; if (ok) {
                    if (oldState != http.StateActive) {
                        throw panic("invalid state transition");
                    }
                    s.conns[c] = cs;
                }
            }
            if (s.closed) {
                s.closeConn(c);
            }
        }
        else if (exprᴛ1 == http.StateHijacked || exprᴛ1 == http.StateClosed) {
            {
                http.ConnState _ = s.conns[c];
                var ok = s.conns[c]; if (ok) {
                    // Remove c from the set of tracked conns and decrement it from the
                    // waitgroup, unless it was previously removed.
                    delete(s.conns, c);
                    // Keep Close from returning until the user's ConnState hook
                    // (if any) finishes.
                    defer(s.wg.Done);
                }
            }
        }

        if (oldHook != default!) {
            oldHook(c, cs);
        }
    };
});

// closeConn closes c.
// s.mu must be held.
[GoRecv] internal static void closeConn(this ref Server s, net.Conn c) {
    s.closeConnChan(c, default!);
}

[GoType("dyn")] partial struct closeConnChan_type {
}

// closeConnChan is like closeConn, but takes an optional channel to receive a value
// when the goroutine closing c is done.
[GoRecv] internal static void closeConnChan(this ref Server s, net.Conn c, channel/*<-*/<EmptyStruct> done) {
    c.Close();
    if (done != default!) {
        done.ᐸꟷ(new closeConnChan_type());
    }
}

} // end httptest_package
