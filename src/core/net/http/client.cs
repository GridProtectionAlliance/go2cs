// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// HTTP client. See RFC 7230 through 7235.
//
// This is the high-level Client interface.
// The low-level implementation is in transport.go.
namespace go.net;

using context = context_package;
using tls = crypto.tls_package;
using base64 = encoding.base64_package;
using errors = errors_package;
using fmt = fmt_package;
using io = io_package;
using log = log_package;
using ascii = net.http.@internal.ascii_package;
using url = net.url_package;
using reflect = reflect_package;
using slices = slices_package;
using strings = strings_package;
using sync = sync_package;
using atomic = sync.atomic_package;
using time = time_package;
using crypto;
using encoding;
using net.http.@internal;
using sync;

partial class http_package {

// A Client is an HTTP client. Its zero value ([DefaultClient]) is a
// usable client that uses [DefaultTransport].
//
// The [Client.Transport] typically has internal state (cached TCP
// connections), so Clients should be reused instead of created as
// needed. Clients are safe for concurrent use by multiple goroutines.
//
// A Client is higher-level than a [RoundTripper] (such as [Transport])
// and additionally handles HTTP details such as cookies and
// redirects.
//
// When following redirects, the Client will forward all headers set on the
// initial [Request] except:
//
//   - when forwarding sensitive headers like "Authorization",
//     "WWW-Authenticate", and "Cookie" to untrusted targets.
//     These headers will be ignored when following a redirect to a domain
//     that is not a subdomain match or exact match of the initial domain.
//     For example, a redirect from "foo.com" to either "foo.com" or "sub.foo.com"
//     will forward the sensitive headers, but a redirect to "bar.com" will not.
//   - when forwarding the "Cookie" header with a non-nil cookie Jar.
//     Since each redirect may mutate the state of the cookie jar,
//     a redirect may possibly alter a cookie set in the initial request.
//     When forwarding the "Cookie" header, any mutated cookies will be omitted,
//     with the expectation that the Jar will insert those mutated cookies
//     with the updated values (assuming the origin matches).
//     If Jar is nil, the initial cookies are forwarded without change.
[GoType] partial struct Client {
    // Transport specifies the mechanism by which individual
    // HTTP requests are made.
    // If nil, DefaultTransport is used.
    public RoundTripper Transport;
    // CheckRedirect specifies the policy for handling redirects.
    // If CheckRedirect is not nil, the client calls it before
    // following an HTTP redirect. The arguments req and via are
    // the upcoming request and the requests made already, oldest
    // first. If CheckRedirect returns an error, the Client's Get
    // method returns both the previous Response (with its Body
    // closed) and CheckRedirect's error (wrapped in a url.Error)
    // instead of issuing the Request req.
    // As a special case, if CheckRedirect returns ErrUseLastResponse,
    // then the most recent response is returned with its body
    // unclosed, along with a nil error.
    //
    // If CheckRedirect is nil, the Client uses its default policy,
    // which is to stop after 10 consecutive requests.
    public http.Request) error CheckRedirect;
    // Jar specifies the cookie jar.
    //
    // The Jar is used to insert relevant cookies into every
    // outbound Request and is updated with the cookie values
    // of every inbound Response. The Jar is consulted for every
    // redirect that the Client follows.
    //
    // If Jar is nil, cookies are only sent if they are explicitly
    // set on the Request.
    public CookieJar Jar;
    // Timeout specifies a time limit for requests made by this
    // Client. The timeout includes connection time, any
    // redirects, and reading the response body. The timer remains
    // running after Get, Head, Post, or Do return and will
    // interrupt reading of the Response.Body.
    //
    // A Timeout of zero means no timeout.
    //
    // The Client cancels requests to the underlying Transport
    // as if the Request's Context ended.
    //
    // For compatibility, the Client will also use the deprecated
    // CancelRequest method on Transport if found. New
    // RoundTripper implementations should use the Request's Context
    // for cancellation instead of implementing CancelRequest.
    public time_package.Duration Timeout;
}

// DefaultClient is the default [Client] and is used by [Get], [Head], and [Post].
public static ж<Client> DefaultClient = Ꮡ(new Client(nil));

// RoundTripper is an interface representing the ability to execute a
// single HTTP transaction, obtaining the [Response] for a given [Request].
//
// A RoundTripper must be safe for concurrent use by multiple
// goroutines.
[GoType] partial interface RoundTripper {
    // RoundTrip executes a single HTTP transaction, returning
    // a Response for the provided Request.
    //
    // RoundTrip should not attempt to interpret the response. In
    // particular, RoundTrip must return err == nil if it obtained
    // a response, regardless of the response's HTTP status code.
    // A non-nil err should be reserved for failure to obtain a
    // response. Similarly, RoundTrip should not attempt to
    // handle higher-level protocol details such as redirects,
    // authentication, or cookies.
    //
    // RoundTrip should not modify the request, except for
    // consuming and closing the Request's Body. RoundTrip may
    // read fields of the request in a separate goroutine. Callers
    // should not mutate or reuse the request until the Response's
    // Body has been closed.
    //
    // RoundTrip must always close the body, including on errors,
    // but depending on the implementation may do so in a separate
    // goroutine even after RoundTrip returns. This means that
    // callers wanting to reuse the body for subsequent requests
    // must arrange to wait for the Close call before doing so.
    //
    // The Request's URL and Header fields must be initialized.
    (ж<Response>, error) RoundTrip(ж<Request> _);
}

// refererForURL returns a referer without any authentication info or
// an empty string if lastReq scheme is https and newReq scheme is http.
// If the referer was explicitly set, then it will continue to be used.
internal static @string refererForURL(ж<url.URL> ᏑlastReq, ж<url.URL> ᏑnewReq, @string explicitRef) {
    ref var lastReq = ref ᏑlastReq.val;
    ref var newReq = ref ᏑnewReq.val;

    // https://tools.ietf.org/html/rfc7231#section-5.5.2
    //   "Clients SHOULD NOT include a Referer header field in a
    //    (non-secure) HTTP request if the referring page was
    //    transferred with a secure protocol."
    if (lastReq.Scheme == "https"u8 && newReq.Scheme == "http"u8) {
        return ""u8;
    }
    if (explicitRef != ""u8) {
        return explicitRef;
    }
    @string referer = lastReq.String();
    if (lastReq.User != nil) {
        // This is not very efficient, but is the best we can
        // do without:
        // - introducing a new method on URL
        // - creating a race condition
        // - copying the URL struct manually, which would cause
        //   maintenance problems down the line
        @string auth = lastReq.User.String() + "@"u8;
        referer = strings.Replace(referer, auth, ""u8, 1);
    }
    return referer;
}

// didTimeout is non-nil only if err != nil.
[GoRecv] public static (ж<Response> resp, Func<bool> didTimeout, error err) send(this ref Client c, ж<Request> Ꮡreq, time.Time deadline) {
    ж<Response> resp = default!;
    Func<bool> didTimeout = default!;
    error err = default!;

    ref var req = ref Ꮡreq.val;
    if (c.Jar != default!) {
        foreach (var (_, cookie) in c.Jar.Cookies(req.URL)) {
            req.AddCookie(cookie);
        }
    }
    (resp, didTimeout, err) = send(Ꮡreq, c.transport(), deadline);
    if (err != default!) {
        return (default!, didTimeout, err);
    }
    if (c.Jar != default!) {
        {
            var rc = resp.Cookies(); if (len(rc) > 0) {
                c.Jar.SetCookies(req.URL, rc);
            }
        }
    }
    return (resp, default!, default!);
}

[GoRecv] internal static time.Time deadline(this ref Client c) {
    if (c.Timeout > 0) {
        return time.Now().Add(c.Timeout);
    }
    return new time.Time(nil);
}

[GoRecv] internal static RoundTripper transport(this ref Client c) {
    if (c.Transport != default!) {
        return c.Transport;
    }
    return DefaultTransport;
}

// ErrSchemeMismatch is returned when a server returns an HTTP response to an HTTPS client.
public static error ErrSchemeMismatch = errors.New("http: server gave HTTP response to HTTPS client"u8);

// send issues an HTTP request.
// Caller should close resp.Body when done reading from it.
internal static (ж<Response> resp, Func<bool> didTimeout, error err) send(ж<Request> Ꮡireq, RoundTripper rt, time.Time deadline) {
    ж<Response> resp = default!;
    Func<bool> didTimeout = default!;
    error err = default!;

    ref var ireq = ref Ꮡireq.val;
    var req = ireq;
    // req is either the original request, or a modified fork
    if (rt == default!) {
        req.closeBody();
        return (default!, alwaysFalse, errors.New("http: no Client.Transport or DefaultTransport"u8));
    }
    if ((~req).URL == nil) {
        req.closeBody();
        return (default!, alwaysFalse, errors.New("http: nil Request.URL"u8));
    }
    if ((~req).RequestURI != ""u8) {
        req.closeBody();
        return (default!, alwaysFalse, errors.New("http: Request.RequestURI can't be set in client requests"u8));
    }
    // forkReq forks req into a shallow clone of ireq the first
    // time it's called.
    var forkReq = 
    var reqʗ1 = req;
    () => {
        if (Ꮡireq == reqʗ1) {
            reqʗ1 = @new<Request>();
            reqʗ1.val = ireq;
        }
    };
    // shallow clone
    // Most the callers of send (Get, Post, et al) don't need
    // Headers, leaving it uninitialized. We guarantee to the
    // Transport that this has been initialized, though.
    if ((~req).Header == default!) {
        forkReq();
        req.val.Header = new ΔHeader();
    }
    {
        var u = (~req).URL.val.User; if (u != nil && (~req).Header.Get("Authorization"u8) == ""u8) {
            @string username = u.Username();
            var (password, _) = u.Password();
            forkReq();
            req.val.Header = cloneOrMakeHeader(ireq.Header);
            (~req).Header.Set("Authorization"u8, "Basic "u8 + basicAuth(username, password));
        }
    }
    if (!deadline.IsZero()) {
        forkReq();
    }
    (stopTimer, didTimeout) = setRequestCancel(req, rt, deadline);
    (resp, err) = rt.RoundTrip(req);
    if (err != default!) {
        stopTimer();
        if (resp != nil) {
            log.Printf("RoundTripper returned a response & error; ignoring response"u8);
        }
        {
            var (tlsErr, ok) = err._<tls.RecordHeaderError>(ᐧ); if (ok) {
                // If we get a bad TLS record header, check to see if the
                // response looks like HTTP and give a more helpful error.
                // See golang.org/issue/11111.
                if (((@string)(tlsErr.RecordHeader[..])) == "HTTP/"u8) {
                    err = ErrSchemeMismatch;
                }
            }
        }
        return (default!, didTimeout, err);
    }
    if (resp == nil) {
        return (default!, didTimeout, fmt.Errorf("http: RoundTripper implementation (%T) returned a nil *Response with a nil error"u8, rt));
    }
    if ((~resp).Body == default!) {
        // The documentation on the Body field says “The http Client and Transport
        // guarantee that Body is always non-nil, even on responses without a body
        // or responses with a zero-length body.” Unfortunately, we didn't document
        // that same constraint for arbitrary RoundTripper implementations, and
        // RoundTripper implementations in the wild (mostly in tests) assume that
        // they can use a nil Body to mean an empty one (similar to Request.Body).
        // (See https://golang.org/issue/38095.)
        //
        // If the ContentLength allows the Body to be empty, fill in an empty one
        // here to ensure that it is non-nil.
        if ((~resp).ContentLength > 0 && (~req).Method != "HEAD"u8) {
            return (default!, didTimeout, fmt.Errorf("http: RoundTripper implementation (%T) returned a *Response with content length %d but a nil Body"u8, rt, (~resp).ContentLength));
        }
        resp.val.Body = io.NopCloser(~strings.NewReader(""u8));
    }
    if (!deadline.IsZero()) {
        resp.val.Body = Ꮡ(new cancelTimerBody(
            stop: stopTimer,
            rc: (~resp).Body,
            reqDidTimeout: didTimeout
        ));
    }
    return (resp, default!, default!);
}

// timeBeforeContextDeadline reports whether the non-zero Time t is
// before ctx's deadline, if any. If ctx does not have a deadline, it
// always reports true (the deadline is considered infinite).
internal static bool timeBeforeContextDeadline(time.Time t, context.Context ctx) {
    var (d, ok) = ctx.Deadline();
    if (!ok) {
        return true;
    }
    return t.Before(d);
}

// knownRoundTripperImpl reports whether rt is a RoundTripper that's
// maintained by the Go team and known to implement the latest
// optional semantics (notably contexts). The Request is used
// to check whether this particular request is using an alternate protocol,
// in which case we need to check the RoundTripper for that protocol.
internal static bool knownRoundTripperImpl(RoundTripper rt, ж<Request> Ꮡreq) {
    ref var req = ref Ꮡreq.val;

    switch (rt.type()) {
    case Transport.val t: {
        {
            var altRT = t.alternateRoundTripper(Ꮡreq); if (altRT != default!) {
                return knownRoundTripperImpl(altRT, Ꮡreq);
            }
        }
        return true;
    }
    case http2Transport.val t: {
        return true;
    }
    case http2noDialH2RoundTripper t: {
        return true;
    }}
    // There's a very minor chance of a false positive with this.
    // Instead of detecting our golang.org/x/net/http2.Transport,
    // it might detect a Transport type in a different http2
    // package. But I know of none, and the only problem would be
    // some temporarily leaked goroutines if the transport didn't
    // support contexts. So this is a good enough heuristic:
    if (reflect.TypeOf(rt).String() == "*http2.Transport"u8) {
        return true;
    }
    return false;
}

// The first way, used only for RoundTripper
// implementations written before Go 1.5 or Go 1.6.
[GoType("dyn")] partial interface setRequestCancel_canceler {
    void CancelRequest(ж<Request> _);
}

// setRequestCancel sets req.Cancel and adds a deadline context to req
// if deadline is non-zero. The RoundTripper's type is used to
// determine whether the legacy CancelRequest behavior should be used.
//
// As background, there are three ways to cancel a request:
// First was Transport.CancelRequest. (deprecated)
// Second was Request.Cancel.
// Third was Request.Context.
// This function populates the second and third, and uses the first if it really needs to.
internal static (Action stopTimer, Func<bool> didTimeout) setRequestCancel(ж<Request> Ꮡreq, RoundTripper rt, time.Time deadline) {
    Action stopTimer = default!;
    Func<bool> didTimeout = default!;

    ref var req = ref Ꮡreq.val;
    if (deadline.IsZero()) {
        return (nop, alwaysFalse);
    }
    var knownTransport = knownRoundTripperImpl(rt, Ꮡreq);
    var oldCtx = req.Context();
    if (req.Cancel == default! && knownTransport) {
        // If they already had a Request.Context that's
        // expiring sooner, do nothing:
        if (!timeBeforeContextDeadline(deadline, oldCtx)) {
            return (nop, alwaysFalse);
        }
        Action cancelCtxΔ1 = default!;
        (req.ctx, ) = context.WithDeadline(oldCtx, deadline);
        var deadlineʗ1 = deadline;
        return (cancelCtxΔ1, () => time.Now().After(deadlineʗ1));
    }
    var initialReqCancel = req.Cancel;
    // the user's original Request.Cancel, if any
    Action cancelCtx = default!;
    if (timeBeforeContextDeadline(deadline, oldCtx)) {
        (req.ctx, cancelCtx) = context.WithDeadline(oldCtx, deadline);
    }
    var cancel = new channel<EmptyStruct>(1);
    req.Cancel = cancel;
    var doCancel = 
    var cancelʗ1 = cancel;
    () => {
        // The second way in the func comment above:
        close(cancelʗ1);
        {
            var (v, ok) = rt._<canceler>(ᐧ); if (ok) {
                v.CancelRequest(Ꮡreq);
            }
        }
    };
    var stopTimerCh = new channel<EmptyStruct>(1);
    ref var once = ref heap(new sync_package.Once(), out var Ꮡonce);
    stopTimer = 
    var cancelCtxʗ1 = cancelCtx;
    var onceʗ1 = once;
    var stopTimerChʗ1 = stopTimerCh;
    () => {
        onceʗ1.Do(
        var cancelCtxʗ3 = cancelCtx;
        var stopTimerChʗ3 = stopTimerCh;
        () => {
            close(stopTimerChʗ3);
            if (cancelCtxʗ3 != default!) {
                cancelCtxʗ3();
            }
        });
    };
    var timer = time.NewTimer(time.Until(deadline));
    ref var timedOut = ref heap(new sync.atomic_package.Bool(), out var ᏑtimedOut);
    var doCancelʗ1 = doCancel;
    var initialReqCancelʗ1 = initialReqCancel;
    var stopTimerChʗ5 = stopTimerCh;
    var timedOutʗ1 = timedOut;
    var timerʗ1 = timer;
    goǃ(() => {
        switch (select(ᐸꟷ(initialReqCancelʗ1, ꓸꓸꓸ), ᐸꟷ((~timerʗ1).C, ꓸꓸꓸ), ᐸꟷ(stopTimerChʗ5, ꓸꓸꓸ))) {
        case 0 when initialReqCancelʗ1.ꟷᐳ(out _): {
            doCancelʗ1();
            timerʗ1.Stop();
            break;
        }
        case 1 when (~timerʗ1).C.ꟷᐳ(out _): {
            timedOutʗ1.Store(true);
            doCancelʗ1();
            break;
        }
        case 2 when stopTimerChʗ5.ꟷᐳ(out _): {
            timerʗ1.Stop();
            break;
        }}
    });
    return (stopTimer, timedOut.Load);
}

// See 2 (end of page 4) https://www.ietf.org/rfc/rfc2617.txt
// "To receive authorization, the client sends the userid and password,
// separated by a single colon (":") character, within a base64
// encoded string in the credentials."
// It is not meant to be urlencoded.
internal static @string basicAuth(@string username, @string password) {
    @string auth = username + ":"u8 + password;
    return base64.StdEncoding.EncodeToString(slice<byte>(auth));
}

// Get issues a GET to the specified URL. If the response is one of
// the following redirect codes, Get follows the redirect, up to a
// maximum of 10 redirects:
//
//	301 (Moved Permanently)
//	302 (Found)
//	303 (See Other)
//	307 (Temporary Redirect)
//	308 (Permanent Redirect)
//
// An error is returned if there were too many redirects or if there
// was an HTTP protocol error. A non-2xx response doesn't cause an
// error. Any returned error will be of type [*url.Error]. The url.Error
// value's Timeout method will report true if the request timed out.
//
// When err is nil, resp always contains a non-nil resp.Body.
// Caller should close resp.Body when done reading from it.
//
// Get is a wrapper around DefaultClient.Get.
//
// To make a request with custom headers, use [NewRequest] and
// DefaultClient.Do.
//
// To make a request with a specified context.Context, use [NewRequestWithContext]
// and DefaultClient.Do.
public static (ж<Response> resp, error err) Get(@string url) {
    ж<Response> resp = default!;
    error err = default!;

    return DefaultClient.Get(url);
}

// Get issues a GET to the specified URL. If the response is one of the
// following redirect codes, Get follows the redirect after calling the
// [Client.CheckRedirect] function:
//
//	301 (Moved Permanently)
//	302 (Found)
//	303 (See Other)
//	307 (Temporary Redirect)
//	308 (Permanent Redirect)
//
// An error is returned if the [Client.CheckRedirect] function fails
// or if there was an HTTP protocol error. A non-2xx response doesn't
// cause an error. Any returned error will be of type [*url.Error]. The
// url.Error value's Timeout method will report true if the request
// timed out.
//
// When err is nil, resp always contains a non-nil resp.Body.
// Caller should close resp.Body when done reading from it.
//
// To make a request with custom headers, use [NewRequest] and [Client.Do].
//
// To make a request with a specified context.Context, use [NewRequestWithContext]
// and Client.Do.
[GoRecv] public static (ж<Response> resp, error err) Get(this ref Client c, @string url) {
    ж<Response> resp = default!;
    error err = default!;

    (req, err) = NewRequest("GET"u8, url, default!);
    if (err != default!) {
        return (default!, err);
    }
    return c.Do(req);
}

internal static bool alwaysFalse() {
    return false;
}

// ErrUseLastResponse can be returned by Client.CheckRedirect hooks to
// control how redirects are processed. If returned, the next request
// is not sent and the most recent response is returned with its body
// unclosed.
public static error ErrUseLastResponse = errors.New("net/http: use last response"u8);

// checkRedirect calls either the user's configured CheckRedirect
// function, or the default.
[GoRecv] public static error checkRedirect(this ref Client c, ж<Request> Ꮡreq, slice<ж<Request>> via) {
    ref var req = ref Ꮡreq.val;

    var fn = c.CheckRedirect;
    if (fn == default!) {
        fn = defaultCheckRedirect;
    }
    return fn(Ꮡreq, via);
}

// redirectBehavior describes what should happen when the
// client encounters a 3xx status code from the server.
internal static (@string redirectMethod, bool shouldRedirect, bool includeBody) redirectBehavior(@string reqMethod, ж<Response> Ꮡresp, ж<Request> Ꮡireq) {
    @string redirectMethod = default!;
    bool shouldRedirect = default!;
    bool includeBody = default!;

    ref var resp = ref Ꮡresp.val;
    ref var ireq = ref Ꮡireq.val;
    switch (resp.StatusCode) {
    case 301 or 302 or 303: {
        redirectMethod = reqMethod;
        shouldRedirect = true;
        includeBody = false;
        if (reqMethod != "GET"u8 && reqMethod != "HEAD"u8) {
            // RFC 2616 allowed automatic redirection only with GET and
            // HEAD requests. RFC 7231 lifts this restriction, but we still
            // restrict other methods to GET to maintain compatibility.
            // See Issue 18570.
            redirectMethod = "GET"u8;
        }
        break;
    }
    case 307 or 308: {
        redirectMethod = reqMethod;
        shouldRedirect = true;
        includeBody = true;
        if (ireq.GetBody == default! && ireq.outgoingLength() != 0) {
            // We had a request body, and 307/308 require
            // re-sending it, but GetBody is not defined. So just
            // return this response to the user instead of an
            // error, like we did in Go 1.7 and earlier.
            shouldRedirect = false;
        }
        break;
    }}

    return (redirectMethod, shouldRedirect, includeBody);
}

// urlErrorOp returns the (*url.Error).Op value to use for the
// provided (*Request).Method value.
internal static @string urlErrorOp(@string method) {
    if (method == ""u8) {
        return "Get"u8;
    }
    {
        var (lowerMethod, ok) = ascii.ToLower(method); if (ok) {
            return method[..1] + lowerMethod[1..];
        }
    }
    return method;
}

// Do sends an HTTP request and returns an HTTP response, following
// policy (such as redirects, cookies, auth) as configured on the
// client.
//
// An error is returned if caused by client policy (such as
// CheckRedirect), or failure to speak HTTP (such as a network
// connectivity problem). A non-2xx status code doesn't cause an
// error.
//
// If the returned error is nil, the [Response] will contain a non-nil
// Body which the user is expected to close. If the Body is not both
// read to EOF and closed, the [Client]'s underlying [RoundTripper]
// (typically [Transport]) may not be able to re-use a persistent TCP
// connection to the server for a subsequent "keep-alive" request.
//
// The request Body, if non-nil, will be closed by the underlying
// Transport, even on errors. The Body may be closed asynchronously after
// Do returns.
//
// On error, any Response can be ignored. A non-nil Response with a
// non-nil error only occurs when CheckRedirect fails, and even then
// the returned [Response.Body] is already closed.
//
// Generally [Get], [Post], or [PostForm] will be used instead of Do.
//
// If the server replies with a redirect, the Client first uses the
// CheckRedirect function to determine whether the redirect should be
// followed. If permitted, a 301, 302, or 303 redirect causes
// subsequent requests to use HTTP method GET
// (or HEAD if the original request was HEAD), with no body.
// A 307 or 308 redirect preserves the original HTTP method and body,
// provided that the [Request.GetBody] function is defined.
// The [NewRequest] function automatically sets GetBody for common
// standard library body types.
//
// Any returned error will be of type [*url.Error]. The url.Error
// value's Timeout method will report true if the request timed out.
[GoRecv] public static (ж<Response>, error) Do(this ref Client c, ж<Request> Ꮡreq) {
    ref var req = ref Ꮡreq.val;

    return c.@do(Ꮡreq);
}

internal static Action<ж<Response>, error> testHookClientDoResult;

[GoRecv] public static (ж<Response> retres, error reterr) @do(this ref Client c, ж<Request> Ꮡreq) => func((defer, _) => {
    ж<Response> retres = default!;
    error reterr = default!;

    ref var req = ref Ꮡreq.val;
    if (testHookClientDoResult != default!) {
        defer(() => {
            testHookClientDoResult(retres, reterr);
        });
    }
    if (req.URL == nil) {
        req.closeBody();
        return (default!, new urlꓸError(
            Op: urlErrorOp(req.Method),
            Err: errors.New("http: nil Request.URL"u8)
        ));
    }
    _ = c;
    // panic early if c is nil; see go.dev/issue/53521
    time.Time deadline = c.deadline();
    slice<ж<Request>> reqs = default!;
    ж<Response> resp = default!;
    Action<ж<Request>> copyHeaders = c.makeHeadersCopier(Ꮡreq);
    bool reqBodyClosed = false; // have we closed the current req.Body?
    ref var redirectMethod = ref heap(new @string(), out var ᏑredirectMethod);
    bool includeBody = default!;
    var uerr = 
    var reqsʗ1 = reqs;
    var respʗ1 = resp;
    (error err) => {
        // the body may have been closed already by c.send()
        if (!reqBodyClosed) {
            req.closeBody();
        }
        ref var urlStr = ref heap(new @string(), out var ᏑurlStr);
        if (respʗ1 != nil && (~respʗ1).Request != nil){
            urlStr = stripPassword((~(~respʗ1).Request).URL);
        } else {
            urlStr = stripPassword(req.URL);
        }
        return Ꮡ(new urlꓸError(
            Op: urlErrorOp((~reqsʗ1[0]).Method),
            URL: urlStr,
            Err: errΔ1
        ));
    };
    while (ᐧ) {
        // For all but the first request, create the next
        // request hop and replace req.
        if (len(reqs) > 0) {
            @string loc = (~resp).Header.Get("Location"u8);
            if (loc == ""u8) {
                // While most 3xx responses include a Location, it is not
                // required and 3xx responses without a Location have been
                // observed in the wild. See issues #17773 and #49281.
                return (resp, default!);
            }
            (u, errΔ2) = req.URL.Parse(loc);
            if (errΔ2 != default!) {
                resp.closeBody();
                return (default!, uerr(fmt.Errorf("failed to parse Location header %q: %v"u8, loc, errΔ2)));
            }
            @string host = ""u8;
            if (req.Host != ""u8 && req.Host != req.URL.Host) {
                // If the caller specified a custom Host header and the
                // redirect location is relative, preserve the Host header
                // through the redirect. See issue #22233.
                {
                    (uΔ1, _) = url.Parse(loc); if (uΔ1 != nil && !uΔ1.IsAbs()) {
                        host = req.Host;
                    }
                }
            }
            var ireq = reqs[0];
            Ꮡreq = Ꮡ(new Request(
                Method: redirectMethod,
                Response: resp,
                URL: u,
                ΔHeader: new ΔHeader(),
                Host: host,
                Cancel: (~ireq).Cancel,
                ctx: (~ireq).ctx
            )); req = ref Ꮡreq.val;
            if (includeBody && (~ireq).GetBody != default!) {
                (req.Body, ) = (~ireq).GetBody();
                if (errΔ2 != default!) {
                    resp.closeBody();
                    return (default!, uerr(errΔ2));
                }
                req.ContentLength = ireq.val.ContentLength;
            }
            // Copy original headers before setting the Referer,
            // in case the user set Referer on their first request.
            // If they really want to override, they can do it in
            // their CheckRedirect func.
            copyHeaders(Ꮡreq);
            // Add the Referer header from the most recent
            // request URL to the new one, if it's not https->http:
            {
                @string @ref = refererForURL((~reqs[len(reqs) - 1]).URL, req.URL, req.Header.Get("Referer"u8)); if (@ref != ""u8) {
                    req.Header.Set("Referer"u8, @ref);
                }
            }
             = c.checkRedirect(Ꮡreq, reqs);
            // Sentinel error to let users select the
            // previous response, without closing its
            // body. See Issue 10069.
            if (AreEqual(errΔ2, ErrUseLastResponse)) {
                return (resp, default!);
            }
            // Close the previous response's body. But
            // read at least some of the body so if it's
            // small the underlying TCP connection will be
            // re-used. No need to check for errors: if it
            // fails, the Transport won't reuse it anyway.
            static readonly UntypedInt maxBodySlurpSize = /* 2 << 10 */ 2048;
            if ((~resp).ContentLength == -1 || (~resp).ContentLength <= maxBodySlurpSize) {
                io.CopyN(io.Discard, (~resp).Body, maxBodySlurpSize);
            }
            (~resp).Body.Close();
            if (errΔ2 != default!) {
                // Special case for Go 1 compatibility: return both the response
                // and an error if the CheckRedirect function failed.
                // See https://golang.org/issue/3795
                // The resp.Body has already been closed.
                var ue = uerr(errΔ2);
                ue._<ж<urlꓸError>>().URL = loc;
                return (resp, ue);
            }
        }
        reqs = append(reqs, Ꮡreq);
        error err = default!;
        Func<bool> didTimeout = default!;
        {
            (resp, didTimeout, err) = c.send(Ꮡreq, deadline); if (err != default!) {
                // c.send() always closes req.Body
                reqBodyClosed = true;
                if (!deadline.IsZero() && didTimeout()) {
                    Ꮡerr = new timeoutError(err.Error() + " (Client.Timeout exceeded while awaiting headers)"u8); err = ref Ꮡerr.val;
                }
                return (default!, uerr(err));
            }
        }
        bool shouldRedirect = default!;
        (redirectMethod, shouldRedirect, includeBody) = redirectBehavior(req.Method, resp, reqs[0]);
        if (!shouldRedirect) {
            return (resp, default!);
        }
        req.closeBody();
    }
});

// makeHeadersCopier makes a function that copies headers from the
// initial Request, ireq. For every redirect, this function must be called
// so that it can copy headers into the upcoming Request.
[GoRecv] public static Action<ж<Request>> makeHeadersCopier(this ref Client c, ж<Request> Ꮡireq) {
    ref var ireq = ref Ꮡireq.val;

    // The headers to copy are from the very initial request.
    // We use a closured callback to keep a reference to these original headers.
    ΔHeader ireqhdr = cloneOrMakeHeader(ireq.Header);
    
    map<@string, slice<ж<ΔCookie>>> icookies = default!;
    if (c.Jar != default! && ireq.Header.Get("Cookie"u8) != ""u8) {
        icookies = new map<@string, slice<ж<ΔCookie>>>();
        foreach (var (_, cΔ1) in ireq.Cookies()) {
            icookies[(~cΔ1).Name] = append(icookies[(~cΔ1).Name], cΔ1);
        }
    }
    var preq = ireq;
    // The previous request
    var icookiesʗ1 = icookies;
    var ireqhdrʗ1 = ireqhdr;
    var preqʗ1 = preq;
    return (ж<Request> req) => {
        // If Jar is present and there was some initial cookies provided
        // via the request header, then we may need to alter the initial
        // cookies as we follow redirects since each redirect may end up
        // modifying a pre-existing cookie.
        //
        // Since cookies already set in the request header do not contain
        // information about the original domain and path, the logic below
        // assumes any new set cookies override the original cookie
        // regardless of domain or path.
        //
        // See https://golang.org/issue/17494
        if (c.Jar != default! && icookiesʗ1 != default!) {
            bool changed = default!;
            var resp = req.val.Response;
            // The response that caused the upcoming redirect
            foreach (var (_, cΔ2) in resp.Cookies()) {
                {
                    var _ = icookiesʗ1[(~cΔ2).Name];
                    var ok = icookiesʗ1[(~cΔ2).Name]; if (ok) {
                        delete(icookiesʗ1, (~cΔ2).Name);
                        changed = true;
                    }
                }
            }
            if (changed) {
                ireqhdrʗ1.Del("Cookie"u8);
                slice<@string> ss = default!;
                foreach (var (_, cs) in icookiesʗ1) {
                    foreach (var (_, cΔ3) in cs) {
                        ss = append(ss, (~cΔ3).Name + "="u8 + (~cΔ3).Value);
                    }
                }
                slices.Sort(ss);
                // Ensure deterministic headers
                ireqhdrʗ1.Set("Cookie"u8, strings.Join(ss, "; "u8));
            }
        }
        // Copy the initial request's Header values
        // (at least the safe ones).
        foreach (var (k, vv) in ireqhdrʗ1) {
            if (shouldCopyHeaderOnRedirect(k, (~preqʗ1).URL, (~req).URL)) {
                (~req).Header[k] = vv;
            }
        }
        preqʗ1 = req;
    };
}

// Update previous Request with the current request
internal static error defaultCheckRedirect(ж<Request> Ꮡreq, slice<ж<Request>> via) {
    ref var req = ref Ꮡreq.val;

    if (len(via) >= 10) {
        return errors.New("stopped after 10 redirects"u8);
    }
    return default!;
}

// Post issues a POST to the specified URL.
//
// Caller should close resp.Body when done reading from it.
//
// If the provided body is an [io.Closer], it is closed after the
// request.
//
// Post is a wrapper around DefaultClient.Post.
//
// To set custom headers, use [NewRequest] and DefaultClient.Do.
//
// See the [Client.Do] method documentation for details on how redirects
// are handled.
//
// To make a request with a specified context.Context, use [NewRequestWithContext]
// and DefaultClient.Do.
public static (ж<Response> resp, error err) Post(@string url, @string contentType, io.Reader body) {
    ж<Response> resp = default!;
    error err = default!;

    return DefaultClient.Post(url, contentType, body);
}

// Post issues a POST to the specified URL.
//
// Caller should close resp.Body when done reading from it.
//
// If the provided body is an [io.Closer], it is closed after the
// request.
//
// To set custom headers, use [NewRequest] and [Client.Do].
//
// To make a request with a specified context.Context, use [NewRequestWithContext]
// and [Client.Do].
//
// See the Client.Do method documentation for details on how redirects
// are handled.
[GoRecv] public static (ж<Response> resp, error err) Post(this ref Client c, @string url, @string contentType, io.Reader body) {
    ж<Response> resp = default!;
    error err = default!;

    (req, err) = NewRequest("POST"u8, url, body);
    if (err != default!) {
        return (default!, err);
    }
    (~req).Header.Set("Content-Type"u8, contentType);
    return c.Do(req);
}

// PostForm issues a POST to the specified URL, with data's keys and
// values URL-encoded as the request body.
//
// The Content-Type header is set to application/x-www-form-urlencoded.
// To set other headers, use [NewRequest] and DefaultClient.Do.
//
// When err is nil, resp always contains a non-nil resp.Body.
// Caller should close resp.Body when done reading from it.
//
// PostForm is a wrapper around DefaultClient.PostForm.
//
// See the [Client.Do] method documentation for details on how redirects
// are handled.
//
// To make a request with a specified [context.Context], use [NewRequestWithContext]
// and DefaultClient.Do.
public static (ж<Response> resp, error err) PostForm(@string url, url.Values data) {
    ж<Response> resp = default!;
    error err = default!;

    return DefaultClient.PostForm(url, data);
}

// PostForm issues a POST to the specified URL,
// with data's keys and values URL-encoded as the request body.
//
// The Content-Type header is set to application/x-www-form-urlencoded.
// To set other headers, use [NewRequest] and [Client.Do].
//
// When err is nil, resp always contains a non-nil resp.Body.
// Caller should close resp.Body when done reading from it.
//
// See the Client.Do method documentation for details on how redirects
// are handled.
//
// To make a request with a specified context.Context, use [NewRequestWithContext]
// and Client.Do.
[GoRecv] public static (ж<Response> resp, error err) PostForm(this ref Client c, @string url, url.Values data) {
    ж<Response> resp = default!;
    error err = default!;

    return c.Post(url, "application/x-www-form-urlencoded"u8, ~strings.NewReader(data.Encode()));
}

// Head issues a HEAD to the specified URL. If the response is one of
// the following redirect codes, Head follows the redirect, up to a
// maximum of 10 redirects:
//
//	301 (Moved Permanently)
//	302 (Found)
//	303 (See Other)
//	307 (Temporary Redirect)
//	308 (Permanent Redirect)
//
// Head is a wrapper around DefaultClient.Head.
//
// To make a request with a specified [context.Context], use [NewRequestWithContext]
// and DefaultClient.Do.
public static (ж<Response> resp, error err) Head(@string url) {
    ж<Response> resp = default!;
    error err = default!;

    return DefaultClient.Head(url);
}

// Head issues a HEAD to the specified URL. If the response is one of the
// following redirect codes, Head follows the redirect after calling the
// [Client.CheckRedirect] function:
//
//	301 (Moved Permanently)
//	302 (Found)
//	303 (See Other)
//	307 (Temporary Redirect)
//	308 (Permanent Redirect)
//
// To make a request with a specified [context.Context], use [NewRequestWithContext]
// and [Client.Do].
[GoRecv] public static (ж<Response> resp, error err) Head(this ref Client c, @string url) {
    ж<Response> resp = default!;
    error err = default!;

    (req, err) = NewRequest("HEAD"u8, url, default!);
    if (err != default!) {
        return (default!, err);
    }
    return c.Do(req);
}

[GoType("dyn")] partial interface CloseIdleConnections_closeIdler {
    void CloseIdleConnections();
}

// CloseIdleConnections closes any connections on its [Transport] which
// were previously connected from previous requests but are now
// sitting idle in a "keep-alive" state. It does not interrupt any
// connections currently in use.
//
// If [Client.Transport] does not have a [Client.CloseIdleConnections] method
// then this method does nothing.
[GoRecv] public static void CloseIdleConnections(this ref Client c) {
    {
        var (tr, ok) = c.transport()._<closeIdler>(ᐧ); if (ok) {
            tr.CloseIdleConnections();
        }
    }
}

// cancelTimerBody is an io.ReadCloser that wraps rc with two features:
//  1. On Read error or close, the stop func is called.
//  2. On Read failure, if reqDidTimeout is true, the error is wrapped and
//     marked as net.Error that hit its timeout.
[GoType] partial struct cancelTimerBody {
    internal Action stop; // stops the time.Timer waiting to cancel the request
    internal io_package.ReadCloser rc;
    internal Func<bool> reqDidTimeout;
}

[GoRecv] internal static (nint n, error err) Read(this ref cancelTimerBody b, slice<byte> p) {
    nint n = default!;
    error err = default!;

    (n, err) = b.rc.Read(p);
    if (err == default!) {
        return (n, default!);
    }
    if (AreEqual(err, io.EOF)) {
        return (n, err);
    }
    if (b.reqDidTimeout()) {
        Ꮡerr = new timeoutError(err.Error() + " (Client.Timeout or context cancellation while reading body)"u8); err = ref Ꮡerr.val;
    }
    return (n, err);
}

[GoRecv] internal static error Close(this ref cancelTimerBody b) {
    var err = b.rc.Close();
    b.stop();
    return err;
}

internal static bool shouldCopyHeaderOnRedirect(@string headerKey, ж<url.URL> Ꮡinitial, ж<url.URL> Ꮡdest) {
    ref var initial = ref Ꮡinitial.val;
    ref var dest = ref Ꮡdest.val;

    var exprᴛ1 = CanonicalHeaderKey(headerKey);
    if (exprᴛ1 == "Authorization"u8 || exprᴛ1 == "Www-Authenticate"u8 || exprᴛ1 == "Cookie"u8 || exprᴛ1 == "Cookie2"u8) {
        @string ihost = idnaASCIIFromURL(Ꮡinitial);
        @string dhost = idnaASCIIFromURL(Ꮡdest);
        return isDomainOrSubdomain(dhost, // Permit sending auth/cookie headers from "foo.com"
 // to "sub.foo.com".
 // Note that we don't send all cookies to subdomains
 // automatically. This function is only used for
 // Cookies set explicitly on the initial outgoing
 // client request. Cookies automatically added via the
 // CookieJar mechanism continue to follow each
 // cookie's scope as set by Set-Cookie. But for
 // outgoing requests with the Cookie header set
 // directly, we don't know their scope, so we assume
 // it's for *.domain.com.
 ihost);
    }

    // All other headers are copied:
    return true;
}

// isDomainOrSubdomain reports whether sub is a subdomain (or exact
// match) of the parent domain.
//
// Both domains must already be in canonical form.
internal static bool isDomainOrSubdomain(@string sub, @string parent) {
    if (sub == parent) {
        return true;
    }
    // If sub contains a :, it's probably an IPv6 address (and is definitely not a hostname).
    // Don't check the suffix in this case, to avoid matching the contents of a IPv6 zone.
    // For example, "::1%.www.example.com" is not a subdomain of "www.example.com".
    if (strings.ContainsAny(sub, ":%"u8)) {
        return false;
    }
    // If sub is "foo.example.com" and parent is "example.com",
    // that means sub must end in "."+parent.
    // Do it without allocating.
    if (!strings.HasSuffix(sub, parent)) {
        return false;
    }
    return sub[len(sub) - len(parent) - 1] == (rune)'.';
}

internal static @string stripPassword(ж<url.URL> Ꮡu) {
    ref var u = ref Ꮡu.val;

    var (_, passSet) = u.User.Password();
    if (passSet) {
        return strings.Replace(u.String(), u.User.String() + "@"u8, u.User.Username() + ":***@"u8, 1);
    }
    return u.String();
}

} // end http_package
