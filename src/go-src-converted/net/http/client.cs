// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// HTTP client. See RFC 7230 through 7235.
//
// This is the high-level Client interface.
// The low-level implementation is in transport.go.

// package http -- go2cs converted at 2022 March 13 05:30:17 UTC
// import "net/http" ==> using http = go.net.http_package
// Original source: C:\Program Files\Go\src\net\http\client.go
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
using sort = sort_package;
using strings = strings_package;
using sync = sync_package;
using time = time_package;


// A Client is an HTTP client. Its zero value (DefaultClient) is a
// usable client that uses DefaultTransport.
//
// The Client's Transport typically has internal state (cached TCP
// connections), so Clients should be reused instead of created as
// needed. Clients are safe for concurrent use by multiple goroutines.
//
// A Client is higher-level than a RoundTripper (such as Transport)
// and additionally handles HTTP details such as cookies and
// redirects.
//
// When following redirects, the Client will forward all headers set on the
// initial Request except:
//
// • when forwarding sensitive headers like "Authorization",
// "WWW-Authenticate", and "Cookie" to untrusted targets.
// These headers will be ignored when following a redirect to a domain
// that is not a subdomain match or exact match of the initial domain.
// For example, a redirect from "foo.com" to either "foo.com" or "sub.foo.com"
// will forward the sensitive headers, but a redirect to "bar.com" will not.
//
// • when forwarding the "Cookie" header with a non-nil cookie Jar.
// Since each redirect may mutate the state of the cookie jar,
// a redirect may possibly alter a cookie set in the initial request.
// When forwarding the "Cookie" header, any mutated cookies will be omitted,
// with the expectation that the Jar will insert those mutated cookies
// with the updated values (assuming the origin matches).
// If Jar is nil, the initial cookies are forwarded without change.
//

using System;
using System.Threading;
public static partial class http_package {

public partial struct Client {
    public RoundTripper Transport; // CheckRedirect specifies the policy for handling redirects.
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
    public Func<ptr<Request>, slice<ptr<Request>>, error> CheckRedirect; // Jar specifies the cookie jar.
//
// The Jar is used to insert relevant cookies into every
// outbound Request and is updated with the cookie values
// of every inbound Response. The Jar is consulted for every
// redirect that the Client follows.
//
// If Jar is nil, cookies are only sent if they are explicitly
// set on the Request.
    public CookieJar Jar; // Timeout specifies a time limit for requests made by this
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
    public time.Duration Timeout;
}

// DefaultClient is the default Client and is used by Get, Head, and Post.
public static ptr<Client> DefaultClient = addr(new Client());

// RoundTripper is an interface representing the ability to execute a
// single HTTP transaction, obtaining the Response for a given Request.
//
// A RoundTripper must be safe for concurrent use by multiple
// goroutines.
public partial interface RoundTripper {
    (ptr<Response>, error) RoundTrip(ptr<Request> _p0);
}

// refererForURL returns a referer without any authentication info or
// an empty string if lastReq scheme is https and newReq scheme is http.
private static @string refererForURL(ptr<url.URL> _addr_lastReq, ptr<url.URL> _addr_newReq) {
    ref url.URL lastReq = ref _addr_lastReq.val;
    ref url.URL newReq = ref _addr_newReq.val;
 
    // https://tools.ietf.org/html/rfc7231#section-5.5.2
    //   "Clients SHOULD NOT include a Referer header field in a
    //    (non-secure) HTTP request if the referring page was
    //    transferred with a secure protocol."
    if (lastReq.Scheme == "https" && newReq.Scheme == "http") {
        return "";
    }
    var referer = lastReq.String();
    if (lastReq.User != null) { 
        // This is not very efficient, but is the best we can
        // do without:
        // - introducing a new method on URL
        // - creating a race condition
        // - copying the URL struct manually, which would cause
        //   maintenance problems down the line
        var auth = lastReq.User.String() + "@";
        referer = strings.Replace(referer, auth, "", 1);
    }
    return referer;
}

// didTimeout is non-nil only if err != nil.
private static (ptr<Response>, Func<bool>, error) send(this ptr<Client> _addr_c, ptr<Request> _addr_req, time.Time deadline) {
    ptr<Response> resp = default!;
    Func<bool> didTimeout = default;
    error err = default!;
    ref Client c = ref _addr_c.val;
    ref Request req = ref _addr_req.val;

    if (c.Jar != null) {
        foreach (var (_, cookie) in c.Jar.Cookies(req.URL)) {
            req.AddCookie(cookie);
        }
    }
    resp, didTimeout, err = send(_addr_req, c.transport(), deadline);
    if (err != null) {
        return (_addr_null!, didTimeout, error.As(err)!);
    }
    if (c.Jar != null) {
        {
            var rc = resp.Cookies();

            if (len(rc) > 0) {
                c.Jar.SetCookies(req.URL, rc);
            }

        }
    }
    return (_addr_resp!, null, error.As(null!)!);
}

private static time.Time deadline(this ptr<Client> _addr_c) {
    ref Client c = ref _addr_c.val;

    if (c.Timeout > 0) {
        return time.Now().Add(c.Timeout);
    }
    return new time.Time();
}

private static RoundTripper transport(this ptr<Client> _addr_c) {
    ref Client c = ref _addr_c.val;

    if (c.Transport != null) {
        return c.Transport;
    }
    return DefaultTransport;
}

// send issues an HTTP request.
// Caller should close resp.Body when done reading from it.
private static (ptr<Response>, Func<bool>, error) send(ptr<Request> _addr_ireq, RoundTripper rt, time.Time deadline) {
    ptr<Response> resp = default!;
    Func<bool> didTimeout = default;
    error err = default!;
    ref Request ireq = ref _addr_ireq.val;

    var req = ireq; // req is either the original request, or a modified fork

    if (rt == null) {
        req.closeBody();
        return (_addr_null!, alwaysFalse, error.As(errors.New("http: no Client.Transport or DefaultTransport"))!);
    }
    if (req.URL == null) {
        req.closeBody();
        return (_addr_null!, alwaysFalse, error.As(errors.New("http: nil Request.URL"))!);
    }
    if (req.RequestURI != "") {
        req.closeBody();
        return (_addr_null!, alwaysFalse, error.As(errors.New("http: Request.RequestURI can't be set in client requests"))!);
    }
    Action forkReq = () => {
        if (ireq == req) {
            req = @new<Request>();
            req.val = ireq; // shallow clone
        }
    }; 

    // Most the callers of send (Get, Post, et al) don't need
    // Headers, leaving it uninitialized. We guarantee to the
    // Transport that this has been initialized, though.
    if (req.Header == null) {
        forkReq();
        req.Header = make(Header);
    }
    {
        var u = req.URL.User;

        if (u != null && req.Header.Get("Authorization") == "") {
            var username = u.Username();
            var (password, _) = u.Password();
            forkReq();
            req.Header = cloneOrMakeHeader(ireq.Header);
            req.Header.Set("Authorization", "Basic " + basicAuth(username, password));
        }
    }

    if (!deadline.IsZero()) {
        forkReq();
    }
    var (stopTimer, didTimeout) = setRequestCancel(_addr_req, rt, deadline);

    resp, err = rt.RoundTrip(req);
    if (err != null) {
        stopTimer();
        if (resp != null) {
            log.Printf("RoundTripper returned a response & error; ignoring response");
        }
        {
            tls.RecordHeaderError (tlsErr, ok) = err._<tls.RecordHeaderError>();

            if (ok) { 
                // If we get a bad TLS record header, check to see if the
                // response looks like HTTP and give a more helpful error.
                // See golang.org/issue/11111.
                if (string(tlsErr.RecordHeader[..]) == "HTTP/") {
                    err = errors.New("http: server gave HTTP response to HTTPS client");
                }
            }

        }
        return (_addr_null!, didTimeout, error.As(err)!);
    }
    if (resp == null) {
        return (_addr_null!, didTimeout, error.As(fmt.Errorf("http: RoundTripper implementation (%T) returned a nil *Response with a nil error", rt))!);
    }
    if (resp.Body == null) { 
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
        if (resp.ContentLength > 0 && req.Method != "HEAD") {
            return (_addr_null!, didTimeout, error.As(fmt.Errorf("http: RoundTripper implementation (%T) returned a *Response with content length %d but a nil Body", rt, resp.ContentLength))!);
        }
        resp.Body = io.NopCloser(strings.NewReader(""));
    }
    if (!deadline.IsZero()) {
        resp.Body = addr(new cancelTimerBody(stop:stopTimer,rc:resp.Body,reqDidTimeout:didTimeout,));
    }
    return (_addr_resp!, null, error.As(null!)!);
}

// timeBeforeContextDeadline reports whether the non-zero Time t is
// before ctx's deadline, if any. If ctx does not have a deadline, it
// always reports true (the deadline is considered infinite).
private static bool timeBeforeContextDeadline(time.Time t, context.Context ctx) {
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
private static bool knownRoundTripperImpl(RoundTripper rt, ptr<Request> _addr_req) {
    ref Request req = ref _addr_req.val;

    switch (rt.type()) {
        case ptr<Transport> t:
            {
                var altRT = t.alternateRoundTripper(req);

                if (altRT != null) {
                    return knownRoundTripperImpl(altRT, _addr_req);
                }

            }
            return true;
            break;
        case ptr<http2Transport> t:
            return true;
            break;
        case http2noDialH2RoundTripper t:
            return true;
            break; 
        // There's a very minor chance of a false positive with this.
        // Instead of detecting our golang.org/x/net/http2.Transport,
        // it might detect a Transport type in a different http2
        // package. But I know of none, and the only problem would be
        // some temporarily leaked goroutines if the transport didn't
        // support contexts. So this is a good enough heuristic:
    } 
    // There's a very minor chance of a false positive with this.
    // Instead of detecting our golang.org/x/net/http2.Transport,
    // it might detect a Transport type in a different http2
    // package. But I know of none, and the only problem would be
    // some temporarily leaked goroutines if the transport didn't
    // support contexts. So this is a good enough heuristic:
    if (reflect.TypeOf(rt).String() == "*http2.Transport") {
        return true;
    }
    return false;
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
private static (Action, Func<bool>) setRequestCancel(ptr<Request> _addr_req, RoundTripper rt, time.Time deadline) {
    Action stopTimer = default;
    Func<bool> didTimeout = default;
    ref Request req = ref _addr_req.val;

    if (deadline.IsZero()) {
        return (nop, alwaysFalse);
    }
    var knownTransport = knownRoundTripperImpl(rt, _addr_req);
    var oldCtx = req.Context();

    if (req.Cancel == null && knownTransport) { 
        // If they already had a Request.Context that's
        // expiring sooner, do nothing:
        if (!timeBeforeContextDeadline(deadline, oldCtx)) {
            return (nop, alwaysFalse);
        }
        Action cancelCtx = default;
        req.ctx, cancelCtx = context.WithDeadline(oldCtx, deadline);
        return (cancelCtx, () => time.Now().After(deadline));
    }
    var initialReqCancel = req.Cancel; // the user's original Request.Cancel, if any

    cancelCtx = default;
    {
        var oldCtx__prev1 = oldCtx;

        oldCtx = req.Context();

        if (timeBeforeContextDeadline(deadline, oldCtx)) {
            req.ctx, cancelCtx = context.WithDeadline(oldCtx, deadline);
        }
        oldCtx = oldCtx__prev1;

    }

    var cancel = make_channel<object>();
    req.Cancel = cancel;

    Action doCancel = () => { 
        // The second way in the func comment above:
        close(cancel); 
        // The first way, used only for RoundTripper
        // implementations written before Go 1.5 or Go 1.6.
        private partial interface canceler {
            void CancelRequest(ptr<Request> _p0);
        }
        {
            canceler (v, ok) = canceler.As(rt._<canceler>())!;

            if (ok) {
                v.CancelRequest(req);
            }

        }
    };

    var stopTimerCh = make_channel<object>();
    sync.Once once = default;
    stopTimer = () => {
        once.Do(() => {
            close(stopTimerCh);
            if (cancelCtx != null) {
                cancelCtx();
            }
        });
    };

    var timer = time.NewTimer(time.Until(deadline));
    atomicBool timedOut = default;

    go_(() => () => {
        doCancel();
        timer.Stop();
        timedOut.setTrue();
        doCancel();
        timer.Stop();
    }());

    return (stopTimer, timedOut.isSet);
}

// See 2 (end of page 4) https://www.ietf.org/rfc/rfc2617.txt
// "To receive authorization, the client sends the userid and password,
// separated by a single colon (":") character, within a base64
// encoded string in the credentials."
// It is not meant to be urlencoded.
private static @string basicAuth(@string username, @string password) {
    var auth = username + ":" + password;
    return base64.StdEncoding.EncodeToString((slice<byte>)auth);
}

// Get issues a GET to the specified URL. If the response is one of
// the following redirect codes, Get follows the redirect, up to a
// maximum of 10 redirects:
//
//    301 (Moved Permanently)
//    302 (Found)
//    303 (See Other)
//    307 (Temporary Redirect)
//    308 (Permanent Redirect)
//
// An error is returned if there were too many redirects or if there
// was an HTTP protocol error. A non-2xx response doesn't cause an
// error. Any returned error will be of type *url.Error. The url.Error
// value's Timeout method will report true if the request timed out.
//
// When err is nil, resp always contains a non-nil resp.Body.
// Caller should close resp.Body when done reading from it.
//
// Get is a wrapper around DefaultClient.Get.
//
// To make a request with custom headers, use NewRequest and
// DefaultClient.Do.
//
// To make a request with a specified context.Context, use NewRequestWithContext
// and DefaultClient.Do.
public static (ptr<Response>, error) Get(@string url) {
    ptr<Response> resp = default!;
    error err = default!;

    return _addr_DefaultClient.Get(url)!;
}

// Get issues a GET to the specified URL. If the response is one of the
// following redirect codes, Get follows the redirect after calling the
// Client's CheckRedirect function:
//
//    301 (Moved Permanently)
//    302 (Found)
//    303 (See Other)
//    307 (Temporary Redirect)
//    308 (Permanent Redirect)
//
// An error is returned if the Client's CheckRedirect function fails
// or if there was an HTTP protocol error. A non-2xx response doesn't
// cause an error. Any returned error will be of type *url.Error. The
// url.Error value's Timeout method will report true if the request
// timed out.
//
// When err is nil, resp always contains a non-nil resp.Body.
// Caller should close resp.Body when done reading from it.
//
// To make a request with custom headers, use NewRequest and Client.Do.
//
// To make a request with a specified context.Context, use NewRequestWithContext
// and Client.Do.
private static (ptr<Response>, error) Get(this ptr<Client> _addr_c, @string url) {
    ptr<Response> resp = default!;
    error err = default!;
    ref Client c = ref _addr_c.val;

    var (req, err) = NewRequest("GET", url, null);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    return _addr_c.Do(req)!;
}

private static bool alwaysFalse() {
    return false;
}

// ErrUseLastResponse can be returned by Client.CheckRedirect hooks to
// control how redirects are processed. If returned, the next request
// is not sent and the most recent response is returned with its body
// unclosed.
public static var ErrUseLastResponse = errors.New("net/http: use last response");

// checkRedirect calls either the user's configured CheckRedirect
// function, or the default.
private static error checkRedirect(this ptr<Client> _addr_c, ptr<Request> _addr_req, slice<ptr<Request>> via) {
    ref Client c = ref _addr_c.val;
    ref Request req = ref _addr_req.val;

    var fn = c.CheckRedirect;
    if (fn == null) {
        fn = defaultCheckRedirect;
    }
    return error.As(fn(req, via))!;
}

// redirectBehavior describes what should happen when the
// client encounters a 3xx status code from the server
private static (@string, bool, bool) redirectBehavior(@string reqMethod, ptr<Response> _addr_resp, ptr<Request> _addr_ireq) {
    @string redirectMethod = default;
    bool shouldRedirect = default;
    bool includeBody = default;
    ref Response resp = ref _addr_resp.val;
    ref Request ireq = ref _addr_ireq.val;

    switch (resp.StatusCode) {
        case 301: 

        case 302: 

        case 303: 
            redirectMethod = reqMethod;
            shouldRedirect = true;
            includeBody = false; 

            // RFC 2616 allowed automatic redirection only with GET and
            // HEAD requests. RFC 7231 lifts this restriction, but we still
            // restrict other methods to GET to maintain compatibility.
            // See Issue 18570.
            if (reqMethod != "GET" && reqMethod != "HEAD") {
                redirectMethod = "GET";
            }
            break;
        case 307: 

        case 308: 
            redirectMethod = reqMethod;
            shouldRedirect = true;
            includeBody = true; 

            // Treat 307 and 308 specially, since they're new in
            // Go 1.8, and they also require re-sending the request body.
            if (resp.Header.Get("Location") == "") { 
                // 308s have been observed in the wild being served
                // without Location headers. Since Go 1.7 and earlier
                // didn't follow these codes, just stop here instead
                // of returning an error.
                // See Issue 17773.
                shouldRedirect = false;
                break;
            }
            if (ireq.GetBody == null && ireq.outgoingLength() != 0) { 
                // We had a request body, and 307/308 require
                // re-sending it, but GetBody is not defined. So just
                // return this response to the user instead of an
                // error, like we did in Go 1.7 and earlier.
                shouldRedirect = false;
            }
            break;
    }
    return (redirectMethod, shouldRedirect, includeBody);
}

// urlErrorOp returns the (*url.Error).Op value to use for the
// provided (*Request).Method value.
private static @string urlErrorOp(@string method) {
    if (method == "") {
        return "Get";
    }
    {
        var (lowerMethod, ok) = ascii.ToLower(method);

        if (ok) {
            return method[..(int)1] + lowerMethod[(int)1..];
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
// If the returned error is nil, the Response will contain a non-nil
// Body which the user is expected to close. If the Body is not both
// read to EOF and closed, the Client's underlying RoundTripper
// (typically Transport) may not be able to re-use a persistent TCP
// connection to the server for a subsequent "keep-alive" request.
//
// The request Body, if non-nil, will be closed by the underlying
// Transport, even on errors.
//
// On error, any Response can be ignored. A non-nil Response with a
// non-nil error only occurs when CheckRedirect fails, and even then
// the returned Response.Body is already closed.
//
// Generally Get, Post, or PostForm will be used instead of Do.
//
// If the server replies with a redirect, the Client first uses the
// CheckRedirect function to determine whether the redirect should be
// followed. If permitted, a 301, 302, or 303 redirect causes
// subsequent requests to use HTTP method GET
// (or HEAD if the original request was HEAD), with no body.
// A 307 or 308 redirect preserves the original HTTP method and body,
// provided that the Request.GetBody function is defined.
// The NewRequest function automatically sets GetBody for common
// standard library body types.
//
// Any returned error will be of type *url.Error. The url.Error
// value's Timeout method will report true if the request timed out.
private static (ptr<Response>, error) Do(this ptr<Client> _addr_c, ptr<Request> _addr_req) {
    ptr<Response> _p0 = default!;
    error _p0 = default!;
    ref Client c = ref _addr_c.val;
    ref Request req = ref _addr_req.val;

    return _addr_c.@do(req)!;
}

private static Action<ptr<Response>, error> testHookClientDoResult = default;

private static (ptr<Response>, error) @do(this ptr<Client> _addr_c, ptr<Request> _addr_req) => func((defer, _, _) => {
    ptr<Response> retres = default!;
    error reterr = default!;
    ref Client c = ref _addr_c.val;
    ref Request req = ref _addr_req.val;

    if (testHookClientDoResult != null) {
        defer(() => {
            testHookClientDoResult(retres, reterr);
        }());
    }
    if (req.URL == null) {
        req.closeBody();
        return (_addr_null!, error.As(addr(new url.Error(Op:urlErrorOp(req.Method),Err:errors.New("http: nil Request.URL"),))!)!);
    }
    var deadline = c.deadline();    slice<ptr<Request>> reqs = default;    ptr<Response> resp;    var copyHeaders = c.makeHeadersCopier(req);    var reqBodyClosed = false;    @string redirectMethod = default;    bool includeBody = default;
    Func<error, error> uerr = err => { 
        // the body may have been closed already by c.send()
        if (!reqBodyClosed) {
            req.closeBody();
        }
        @string urlStr = default;
        if (resp != null && resp.Request != null) {
            urlStr = stripPassword(_addr_resp.Request.URL);
        }
        else
 {
            urlStr = stripPassword(_addr_req.URL);
        }
        return addr(new url.Error(Op:urlErrorOp(reqs[0].Method),URL:urlStr,Err:err,));
    };
    while (true) { 
        // For all but the first request, create the next
        // request hop and replace req.
        if (len(reqs) > 0) {
            var loc = resp.Header.Get("Location");
            if (loc == "") {
                resp.closeBody();
                return (_addr_null!, error.As(uerr(fmt.Errorf("%d response missing Location header", resp.StatusCode)))!);
            }
            var (u, err) = req.URL.Parse(loc);
            if (err != null) {
                resp.closeBody();
                return (_addr_null!, error.As(uerr(fmt.Errorf("failed to parse Location header %q: %v", loc, err)))!);
            }
            @string host = "";
            if (req.Host != "" && req.Host != req.URL.Host) { 
                // If the caller specified a custom Host header and the
                // redirect location is relative, preserve the Host header
                // through the redirect. See issue #22233.
                {
                    var u__prev3 = u;

                    var (u, _) = url.Parse(loc);

                    if (u != null && !u.IsAbs()) {
                        host = req.Host;
                    }

                    u = u__prev3;

                }
            }
            var ireq = reqs[0];
            req = addr(new Request(Method:redirectMethod,Response:resp,URL:u,Header:make(Header),Host:host,Cancel:ireq.Cancel,ctx:ireq.ctx,));
            if (includeBody && ireq.GetBody != null) {
                req.Body, err = ireq.GetBody();
                if (err != null) {
                    resp.closeBody();
                    return (_addr_null!, error.As(uerr(err))!);
                }
                req.ContentLength = ireq.ContentLength;
            } 

            // Copy original headers before setting the Referer,
            // in case the user set Referer on their first request.
            // If they really want to override, they can do it in
            // their CheckRedirect func.
            copyHeaders(req); 

            // Add the Referer header from the most recent
            // request URL to the new one, if it's not https->http:
            {
                var @ref = refererForURL(_addr_reqs[len(reqs) - 1].URL, _addr_req.URL);

                if (ref != "") {
                    req.Header.Set("Referer", ref);
                }

            }
            err = c.checkRedirect(req, reqs); 

            // Sentinel error to let users select the
            // previous response, without closing its
            // body. See Issue 10069.
            if (err == ErrUseLastResponse) {
                return (_addr_resp!, error.As(null!)!);
            } 

            // Close the previous response's body. But
            // read at least some of the body so if it's
            // small the underlying TCP connection will be
            // re-used. No need to check for errors: if it
            // fails, the Transport won't reuse it anyway.
            const nint maxBodySlurpSize = 2 << 10;

            if (resp.ContentLength == -1 || resp.ContentLength <= maxBodySlurpSize) {
                io.CopyN(io.Discard, resp.Body, maxBodySlurpSize);
            }
            resp.Body.Close();

            if (err != null) { 
                // Special case for Go 1 compatibility: return both the response
                // and an error if the CheckRedirect function failed.
                // See https://golang.org/issue/3795
                // The resp.Body has already been closed.
                var ue = uerr(err);
                ue._<ptr<url.Error>>().URL = loc;
                return (_addr_resp!, error.As(ue)!);
            }
        }
        reqs = append(reqs, req);
        error err = default!;
        Func<bool> didTimeout = default;
        resp, didTimeout, err = c.send(req, deadline);

        if (err != null) { 
            // c.send() always closes req.Body
            reqBodyClosed = true;
            if (!deadline.IsZero() && didTimeout()) {
                err = error.As(addr(new httpError(err:err.Error()+" (Client.Timeout exceeded while awaiting headers)",timeout:true,)))!;
            }
            return (_addr_null!, error.As(uerr(err))!);
        }
        bool shouldRedirect = default;
        redirectMethod, shouldRedirect, includeBody = redirectBehavior(req.Method, resp, _addr_reqs[0]);
        if (!shouldRedirect) {
            return (_addr_resp!, error.As(null!)!);
        }
        req.closeBody();
    }
});

// makeHeadersCopier makes a function that copies headers from the
// initial Request, ireq. For every redirect, this function must be called
// so that it can copy headers into the upcoming Request.
private static Action<ptr<Request>> makeHeadersCopier(this ptr<Client> _addr_c, ptr<Request> _addr_ireq) {
    ref Client c = ref _addr_c.val;
    ref Request ireq = ref _addr_ireq.val;
 
    // The headers to copy are from the very initial request.
    // We use a closured callback to keep a reference to these original headers.
    var ireqhdr = cloneOrMakeHeader(ireq.Header);    map<@string, slice<ptr<Cookie>>> icookies = default;
    if (c.Jar != null && ireq.Header.Get("Cookie") != "") {
        icookies = make_map<@string, slice<ptr<Cookie>>>();
        {
            var c__prev1 = c;

            foreach (var (_, __c) in ireq.Cookies()) {
                c = __c;
                icookies[c.Name] = append(icookies[c.Name], c);
            }

            c = c__prev1;
        }
    }
    var preq = ireq; // The previous request
    return req => { 
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
        if (c.Jar != null && icookies != null) {
            bool changed = default;
            var resp = req.Response; // The response that caused the upcoming redirect
            {
                var c__prev1 = c;

                foreach (var (_, __c) in resp.Cookies()) {
                    c = __c;
                    {
                        var (_, ok) = icookies[c.Name];

                        if (ok) {
                            delete(icookies, c.Name);
                            changed = true;
                        }

                    }
                }

                c = c__prev1;
            }

            if (changed) {
                ireqhdr.Del("Cookie");
                slice<@string> ss = default;
                foreach (var (_, cs) in icookies) {
                    {
                        var c__prev2 = c;

                        foreach (var (_, __c) in cs) {
                            c = __c;
                            ss = append(ss, c.Name + "=" + c.Value);
                        }

                        c = c__prev2;
                    }
                }
                sort.Strings(ss); // Ensure deterministic headers
                ireqhdr.Set("Cookie", strings.Join(ss, "; "));
            }
        }
        foreach (var (k, vv) in ireqhdr) {
            if (shouldCopyHeaderOnRedirect(k, _addr_preq.URL, _addr_req.URL)) {
                req.Header[k] = vv;
            }
        }        preq = req; // Update previous Request with the current request
    };
}

private static error defaultCheckRedirect(ptr<Request> _addr_req, slice<ptr<Request>> via) {
    ref Request req = ref _addr_req.val;

    if (len(via) >= 10) {
        return error.As(errors.New("stopped after 10 redirects"))!;
    }
    return error.As(null!)!;
}

// Post issues a POST to the specified URL.
//
// Caller should close resp.Body when done reading from it.
//
// If the provided body is an io.Closer, it is closed after the
// request.
//
// Post is a wrapper around DefaultClient.Post.
//
// To set custom headers, use NewRequest and DefaultClient.Do.
//
// See the Client.Do method documentation for details on how redirects
// are handled.
//
// To make a request with a specified context.Context, use NewRequestWithContext
// and DefaultClient.Do.
public static (ptr<Response>, error) Post(@string url, @string contentType, io.Reader body) {
    ptr<Response> resp = default!;
    error err = default!;

    return _addr_DefaultClient.Post(url, contentType, body)!;
}

// Post issues a POST to the specified URL.
//
// Caller should close resp.Body when done reading from it.
//
// If the provided body is an io.Closer, it is closed after the
// request.
//
// To set custom headers, use NewRequest and Client.Do.
//
// To make a request with a specified context.Context, use NewRequestWithContext
// and Client.Do.
//
// See the Client.Do method documentation for details on how redirects
// are handled.
private static (ptr<Response>, error) Post(this ptr<Client> _addr_c, @string url, @string contentType, io.Reader body) {
    ptr<Response> resp = default!;
    error err = default!;
    ref Client c = ref _addr_c.val;

    var (req, err) = NewRequest("POST", url, body);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    req.Header.Set("Content-Type", contentType);
    return _addr_c.Do(req)!;
}

// PostForm issues a POST to the specified URL, with data's keys and
// values URL-encoded as the request body.
//
// The Content-Type header is set to application/x-www-form-urlencoded.
// To set other headers, use NewRequest and DefaultClient.Do.
//
// When err is nil, resp always contains a non-nil resp.Body.
// Caller should close resp.Body when done reading from it.
//
// PostForm is a wrapper around DefaultClient.PostForm.
//
// See the Client.Do method documentation for details on how redirects
// are handled.
//
// To make a request with a specified context.Context, use NewRequestWithContext
// and DefaultClient.Do.
public static (ptr<Response>, error) PostForm(@string url, url.Values data) {
    ptr<Response> resp = default!;
    error err = default!;

    return _addr_DefaultClient.PostForm(url, data)!;
}

// PostForm issues a POST to the specified URL,
// with data's keys and values URL-encoded as the request body.
//
// The Content-Type header is set to application/x-www-form-urlencoded.
// To set other headers, use NewRequest and Client.Do.
//
// When err is nil, resp always contains a non-nil resp.Body.
// Caller should close resp.Body when done reading from it.
//
// See the Client.Do method documentation for details on how redirects
// are handled.
//
// To make a request with a specified context.Context, use NewRequestWithContext
// and Client.Do.
private static (ptr<Response>, error) PostForm(this ptr<Client> _addr_c, @string url, url.Values data) {
    ptr<Response> resp = default!;
    error err = default!;
    ref Client c = ref _addr_c.val;

    return _addr_c.Post(url, "application/x-www-form-urlencoded", strings.NewReader(data.Encode()))!;
}

// Head issues a HEAD to the specified URL. If the response is one of
// the following redirect codes, Head follows the redirect, up to a
// maximum of 10 redirects:
//
//    301 (Moved Permanently)
//    302 (Found)
//    303 (See Other)
//    307 (Temporary Redirect)
//    308 (Permanent Redirect)
//
// Head is a wrapper around DefaultClient.Head
//
// To make a request with a specified context.Context, use NewRequestWithContext
// and DefaultClient.Do.
public static (ptr<Response>, error) Head(@string url) {
    ptr<Response> resp = default!;
    error err = default!;

    return _addr_DefaultClient.Head(url)!;
}

// Head issues a HEAD to the specified URL. If the response is one of the
// following redirect codes, Head follows the redirect after calling the
// Client's CheckRedirect function:
//
//    301 (Moved Permanently)
//    302 (Found)
//    303 (See Other)
//    307 (Temporary Redirect)
//    308 (Permanent Redirect)
//
// To make a request with a specified context.Context, use NewRequestWithContext
// and Client.Do.
private static (ptr<Response>, error) Head(this ptr<Client> _addr_c, @string url) {
    ptr<Response> resp = default!;
    error err = default!;
    ref Client c = ref _addr_c.val;

    var (req, err) = NewRequest("HEAD", url, null);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    return _addr_c.Do(req)!;
}

// CloseIdleConnections closes any connections on its Transport which
// were previously connected from previous requests but are now
// sitting idle in a "keep-alive" state. It does not interrupt any
// connections currently in use.
//
// If the Client's Transport does not have a CloseIdleConnections method
// then this method does nothing.
private static void CloseIdleConnections(this ptr<Client> _addr_c) {
    ref Client c = ref _addr_c.val;

    private partial interface closeIdler {
        void CloseIdleConnections();
    }
    {
        closeIdler (tr, ok) = closeIdler.As(c.transport()._<closeIdler>())!;

        if (ok) {
            tr.CloseIdleConnections();
        }
    }
}

// cancelTimerBody is an io.ReadCloser that wraps rc with two features:
// 1) On Read error or close, the stop func is called.
// 2) On Read failure, if reqDidTimeout is true, the error is wrapped and
//    marked as net.Error that hit its timeout.
private partial struct cancelTimerBody {
    public Action stop; // stops the time.Timer waiting to cancel the request
    public io.ReadCloser rc;
    public Func<bool> reqDidTimeout;
}

private static (nint, error) Read(this ptr<cancelTimerBody> _addr_b, slice<byte> p) {
    nint n = default;
    error err = default!;
    ref cancelTimerBody b = ref _addr_b.val;

    n, err = b.rc.Read(p);
    if (err == null) {
        return (n, error.As(null!)!);
    }
    b.stop();
    if (err == io.EOF) {
        return (n, error.As(err)!);
    }
    if (b.reqDidTimeout()) {
        err = addr(new httpError(err:err.Error()+" (Client.Timeout or context cancellation while reading body)",timeout:true,));
    }
    return (n, error.As(err)!);
}

private static error Close(this ptr<cancelTimerBody> _addr_b) {
    ref cancelTimerBody b = ref _addr_b.val;

    var err = b.rc.Close();
    b.stop();
    return error.As(err)!;
}

private static bool shouldCopyHeaderOnRedirect(@string headerKey, ptr<url.URL> _addr_initial, ptr<url.URL> _addr_dest) {
    ref url.URL initial = ref _addr_initial.val;
    ref url.URL dest = ref _addr_dest.val;

    switch (CanonicalHeaderKey(headerKey)) {
        case "Authorization": 
            // Permit sending auth/cookie headers from "foo.com"
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


        case "Www-Authenticate": 
            // Permit sending auth/cookie headers from "foo.com"
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


        case "Cookie": 
            // Permit sending auth/cookie headers from "foo.com"
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


        case "Cookie2": 
            // Permit sending auth/cookie headers from "foo.com"
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

            var ihost = canonicalAddr(initial);
            var dhost = canonicalAddr(dest);
            return isDomainOrSubdomain(dhost, ihost);
            break;
    } 
    // All other headers are copied:
    return true;
}

// isDomainOrSubdomain reports whether sub is a subdomain (or exact
// match) of the parent domain.
//
// Both domains must already be in canonical form.
private static bool isDomainOrSubdomain(@string sub, @string parent) {
    if (sub == parent) {
        return true;
    }
    if (!strings.HasSuffix(sub, parent)) {
        return false;
    }
    return sub[len(sub) - len(parent) - 1] == '.';
}

private static @string stripPassword(ptr<url.URL> _addr_u) {
    ref url.URL u = ref _addr_u.val;

    var (_, passSet) = u.User.Password();
    if (passSet) {
        return strings.Replace(u.String(), u.User.String() + "@", u.User.Username() + ":***@", 1);
    }
    return u.String();
}

} // end http_package
