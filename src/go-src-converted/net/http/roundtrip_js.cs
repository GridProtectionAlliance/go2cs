// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build js,wasm

// package http -- go2cs converted at 2020 October 09 04:57:53 UTC
// import "net/http" ==> using http = go.net.http_package
// Original source: C:\Go\src\net\http\roundtrip_js.go
using errors = go.errors_package;
using fmt = go.fmt_package;
using io = go.io_package;
using ioutil = go.io.ioutil_package;
using strconv = go.strconv_package;
using js = go.syscall.js_package;
using static go.builtin;
using System;

namespace go {
namespace net
{
    public static partial class http_package
    {
        private static var uint8Array = js.Global().Get("Uint8Array");

        // jsFetchMode is a Request.Header map key that, if present,
        // signals that the map entry is actually an option to the Fetch API mode setting.
        // Valid values are: "cors", "no-cors", "same-origin", "navigate"
        // The default is "same-origin".
        //
        // Reference: https://developer.mozilla.org/en-US/docs/Web/API/WindowOrWorkerGlobalScope/fetch#Parameters
        private static readonly @string jsFetchMode = (@string)"js.fetch:mode";

        // jsFetchCreds is a Request.Header map key that, if present,
        // signals that the map entry is actually an option to the Fetch API credentials setting.
        // Valid values are: "omit", "same-origin", "include"
        // The default is "same-origin".
        //
        // Reference: https://developer.mozilla.org/en-US/docs/Web/API/WindowOrWorkerGlobalScope/fetch#Parameters


        // jsFetchCreds is a Request.Header map key that, if present,
        // signals that the map entry is actually an option to the Fetch API credentials setting.
        // Valid values are: "omit", "same-origin", "include"
        // The default is "same-origin".
        //
        // Reference: https://developer.mozilla.org/en-US/docs/Web/API/WindowOrWorkerGlobalScope/fetch#Parameters
        private static readonly @string jsFetchCreds = (@string)"js.fetch:credentials";

        // jsFetchRedirect is a Request.Header map key that, if present,
        // signals that the map entry is actually an option to the Fetch API redirect setting.
        // Valid values are: "follow", "error", "manual"
        // The default is "follow".
        //
        // Reference: https://developer.mozilla.org/en-US/docs/Web/API/WindowOrWorkerGlobalScope/fetch#Parameters


        // jsFetchRedirect is a Request.Header map key that, if present,
        // signals that the map entry is actually an option to the Fetch API redirect setting.
        // Valid values are: "follow", "error", "manual"
        // The default is "follow".
        //
        // Reference: https://developer.mozilla.org/en-US/docs/Web/API/WindowOrWorkerGlobalScope/fetch#Parameters
        private static readonly @string jsFetchRedirect = (@string)"js.fetch:redirect";



        private static var useFakeNetwork = js.Global().Get("fetch").IsUndefined();

        // RoundTrip implements the RoundTripper interface using the WHATWG Fetch API.
        private static (ptr<Response>, error) RoundTrip(this ptr<Transport> _addr_t, ptr<Request> _addr_req)
        {
            ptr<Response> _p0 = default!;
            error _p0 = default!;
            ref Transport t = ref _addr_t.val;
            ref Request req = ref _addr_req.val;

            if (useFakeNetwork)
            {
                return _addr_t.roundTrip(req)!;
            }

            var ac = js.Global().Get("AbortController");
            if (!ac.IsUndefined())
            { 
                // Some browsers that support WASM don't necessarily support
                // the AbortController. See
                // https://developer.mozilla.org/en-US/docs/Web/API/AbortController#Browser_compatibility.
                ac = ac.New();

            }

            var opt = js.Global().Get("Object").New(); 
            // See https://developer.mozilla.org/en-US/docs/Web/API/WindowOrWorkerGlobalScope/fetch
            // for options available.
            opt.Set("method", req.Method);
            opt.Set("credentials", "same-origin");
            {
                var h__prev1 = h;

                var h = req.Header.Get(jsFetchCreds);

                if (h != "")
                {
                    opt.Set("credentials", h);
                    req.Header.Del(jsFetchCreds);
                }

                h = h__prev1;

            }

            {
                var h__prev1 = h;

                h = req.Header.Get(jsFetchMode);

                if (h != "")
                {
                    opt.Set("mode", h);
                    req.Header.Del(jsFetchMode);
                }

                h = h__prev1;

            }

            {
                var h__prev1 = h;

                h = req.Header.Get(jsFetchRedirect);

                if (h != "")
                {
                    opt.Set("redirect", h);
                    req.Header.Del(jsFetchRedirect);
                }

                h = h__prev1;

            }

            if (!ac.IsUndefined())
            {
                opt.Set("signal", ac.Get("signal"));
            }

            var headers = js.Global().Get("Headers").New();
            {
                var key__prev1 = key;

                foreach (var (__key, __values) in req.Header)
                {
                    key = __key;
                    values = __values;
                    {
                        var value__prev2 = value;

                        foreach (var (_, __value) in values)
                        {
                            value = __value;
                            headers.Call("append", key, value);
                        }

                        value = value__prev2;
                    }
                }

                key = key__prev1;
            }

            opt.Set("headers", headers);

            if (req.Body != null)
            { 
                // TODO(johanbrandhorst): Stream request body when possible.
                // See https://bugs.chromium.org/p/chromium/issues/detail?id=688906 for Blink issue.
                // See https://bugzilla.mozilla.org/show_bug.cgi?id=1387483 for Firefox issue.
                // See https://github.com/web-platform-tests/wpt/issues/7693 for WHATWG tests issue.
                // See https://developer.mozilla.org/en-US/docs/Web/API/Streams_API for more details on the Streams API
                // and browser support.
                var (body, err) = ioutil.ReadAll(req.Body);
                if (err != null)
                {
                    req.Body.Close(); // RoundTrip must always close the body, including on errors.
                    return (_addr_null!, error.As(err)!);

                }

                req.Body.Close();
                var buf = uint8Array.New(len(body));
                js.CopyBytesToJS(buf, body);
                opt.Set("body", buf);

            }

            var fetchPromise = js.Global().Call("fetch", req.URL.String(), opt);
            var respCh = make_channel<ptr<Response>>(1L);            var errCh = make_channel<error>(1L);            js.Func success = default;            js.Func failure = default;

            success = js.FuncOf((@this, args) =>
            {
                success.Release();
                failure.Release();

                var result = args[0L];
                Header header = new Header(); 
                // https://developer.mozilla.org/en-US/docs/Web/API/Headers/entries
                var headersIt = result.Get("headers").Call("entries");
                while (true)
                {
                    var n = headersIt.Call("next");
                    if (n.Get("done").Bool())
                    {
                        break;
                    }

                    var pair = n.Get("value");
                    var key = pair.Index(0L).String();
                    var value = pair.Index(1L).String();
                    var ck = CanonicalHeaderKey(key);
                    header[ck] = append(header[ck], value);

                }


                var contentLength = int64(0L);
                {
                    var (cl, err) = strconv.ParseInt(header.Get("Content-Length"), 10L, 64L);

                    if (err == null)
                    {
                        contentLength = cl;
                    }

                }


                var b = result.Get("body");
                io.ReadCloser body = default; 
                // The body is undefined when the browser does not support streaming response bodies (Firefox),
                // and null in certain error cases, i.e. when the request is blocked because of CORS settings.
                if (!b.IsUndefined() && !b.IsNull())
                {
                    body = addr(new streamReader(stream:b.Call("getReader")));
                }
                else
                { 
                    // Fall back to using ArrayBuffer
                    // https://developer.mozilla.org/en-US/docs/Web/API/Body/arrayBuffer
                    body = addr(new arrayReader(arrayPromise:result.Call("arrayBuffer")));

                }

                var code = result.Get("status").Int();
                respCh.Send(addr(new Response(Status:fmt.Sprintf("%d %s",code,StatusText(code)),StatusCode:code,Header:header,ContentLength:contentLength,Body:body,Request:req,)));

                return _addr_null!;

            });
            failure = js.FuncOf((@this, args) =>
            {
                success.Release();
                failure.Release();
                errCh.Send(fmt.Errorf("net/http: fetch() failed: %s", args[0L].Get("message").String()));
                return _addr_null!;
            });

            fetchPromise.Call("then", success, failure);
            if (!ac.IsUndefined())
            { 
                // Abort the Fetch request.
                ac.Call("abort");

            }

            return (_addr_null!, error.As(req.Context().Err())!);
            return (_addr_resp!, error.As(null!)!);
            return (_addr_null!, error.As(err)!);

        }

        private static var errClosed = errors.New("net/http: reader is closed");

        // streamReader implements an io.ReadCloser wrapper for ReadableStream.
        // See https://fetch.spec.whatwg.org/#readablestream for more information.
        private partial struct streamReader
        {
            public slice<byte> pending;
            public js.Value stream;
            public error err; // sticky read error
        }

        private static (long, error) Read(this ptr<streamReader> _addr_r, slice<byte> p) => func((defer, _, __) =>
        {
            long n = default;
            error err = default!;
            ref streamReader r = ref _addr_r.val;

            if (r.err != null)
            {
                return (0L, error.As(r.err)!);
            }

            if (len(r.pending) == 0L)
            {
                var bCh = make_channel<slice<byte>>(1L);                var errCh = make_channel<error>(1L);
                var success = js.FuncOf((@this, args) =>
                {
                    var result = args[0L];
                    if (result.Get("done").Bool())
                    {
                        errCh.Send(io.EOF);
                        return null;
                    }

                    var value = make_slice<byte>(result.Get("value").Get("byteLength").Int());
                    js.CopyBytesToGo(value, result.Get("value"));
                    bCh.Send(value);
                    return null;

                });
                defer(success.Release());
                var failure = js.FuncOf((@this, args) =>
                { 
                    // Assumes it's a TypeError. See
                    // https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/TypeError
                    // for more information on this type. See
                    // https://streams.spec.whatwg.org/#byob-reader-read for the spec on
                    // the read method.
                    errCh.Send(errors.New(args[0L].Get("message").String()));
                    return null;

                });
                defer(failure.Release());
                r.stream.Call("read").Call("then", success, failure);
                r.pending = b;
                r.err = err;
                return (0L, error.As(err)!);

            }

            n = copy(p, r.pending);
            r.pending = r.pending[n..];
            return (n, error.As(null!)!);

        });

        private static error Close(this ptr<streamReader> _addr_r)
        {
            ref streamReader r = ref _addr_r.val;
 
            // This ignores any error returned from cancel method. So far, I did not encounter any concrete
            // situation where reporting the error is meaningful. Most users ignore error from resp.Body.Close().
            // If there's a need to report error here, it can be implemented and tested when that need comes up.
            r.stream.Call("cancel");
            if (r.err == null)
            {
                r.err = errClosed;
            }

            return error.As(null!)!;

        }

        // arrayReader implements an io.ReadCloser wrapper for ArrayBuffer.
        // https://developer.mozilla.org/en-US/docs/Web/API/Body/arrayBuffer.
        private partial struct arrayReader
        {
            public js.Value arrayPromise;
            public slice<byte> pending;
            public bool read;
            public error err; // sticky read error
        }

        private static (long, error) Read(this ptr<arrayReader> _addr_r, slice<byte> p) => func((defer, _, __) =>
        {
            long n = default;
            error err = default!;
            ref arrayReader r = ref _addr_r.val;

            if (r.err != null)
            {
                return (0L, error.As(r.err)!);
            }

            if (!r.read)
            {
                r.read = true;
                var bCh = make_channel<slice<byte>>(1L);                var errCh = make_channel<error>(1L);
                var success = js.FuncOf((@this, args) =>
                { 
                    // Wrap the input ArrayBuffer with a Uint8Array
                    var uint8arrayWrapper = uint8Array.New(args[0L]);
                    var value = make_slice<byte>(uint8arrayWrapper.Get("byteLength").Int());
                    js.CopyBytesToGo(value, uint8arrayWrapper);
                    bCh.Send(value);
                    return null;

                });
                defer(success.Release());
                var failure = js.FuncOf((@this, args) =>
                { 
                    // Assumes it's a TypeError. See
                    // https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/TypeError
                    // for more information on this type.
                    // See https://fetch.spec.whatwg.org/#concept-body-consume-body for reasons this might error.
                    errCh.Send(errors.New(args[0L].Get("message").String()));
                    return null;

                });
                defer(failure.Release());
                r.arrayPromise.Call("then", success, failure);
                r.pending = b;
                return (0L, error.As(err)!);

            }

            if (len(r.pending) == 0L)
            {
                return (0L, error.As(io.EOF)!);
            }

            n = copy(p, r.pending);
            r.pending = r.pending[n..];
            return (n, error.As(null!)!);

        });

        private static error Close(this ptr<arrayReader> _addr_r)
        {
            ref arrayReader r = ref _addr_r.val;

            if (r.err == null)
            {
                r.err = errClosed;
            }

            return error.As(null!)!;

        }
    }
}}
