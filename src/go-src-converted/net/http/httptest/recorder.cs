// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package httptest -- go2cs converted at 2020 August 29 08:34:12 UTC
// import "net/http/httptest" ==> using httptest = go.net.http.httptest_package
// Original source: C:\Go\src\net\http\httptest\recorder.go
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using ioutil = go.io.ioutil_package;
using http = go.net.http_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using static go.builtin;

namespace go {
namespace net {
namespace http
{
    public static partial class httptest_package
    {
        // ResponseRecorder is an implementation of http.ResponseWriter that
        // records its mutations for later inspection in tests.
        public partial struct ResponseRecorder
        {
            public long Code; // HeaderMap contains the headers explicitly set by the Handler.
//
// To get the implicit headers set by the server (such as
// automatic Content-Type), use the Result method.
            public http.Header HeaderMap; // Body is the buffer to which the Handler's Write calls are sent.
// If nil, the Writes are silently discarded.
            public ptr<bytes.Buffer> Body; // Flushed is whether the Handler called Flush.
            public bool Flushed;
            public ptr<http.Response> result; // cache of Result's return value
            public http.Header snapHeader; // snapshot of HeaderMap at first Write
            public bool wroteHeader;
        }

        // NewRecorder returns an initialized ResponseRecorder.
        public static ref ResponseRecorder NewRecorder()
        {
            return ref new ResponseRecorder(HeaderMap:make(http.Header),Body:new(bytes.Buffer),Code:200,);
        }

        // DefaultRemoteAddr is the default remote address to return in RemoteAddr if
        // an explicit DefaultRemoteAddr isn't set on ResponseRecorder.
        public static readonly @string DefaultRemoteAddr = "1.2.3.4";

        // Header returns the response headers.


        // Header returns the response headers.
        private static http.Header Header(this ref ResponseRecorder rw)
        {
            var m = rw.HeaderMap;
            if (m == null)
            {
                m = make(http.Header);
                rw.HeaderMap = m;
            }
            return m;
        }

        // writeHeader writes a header if it was not written yet and
        // detects Content-Type if needed.
        //
        // bytes or str are the beginning of the response body.
        // We pass both to avoid unnecessarily generate garbage
        // in rw.WriteString which was created for performance reasons.
        // Non-nil bytes win.
        private static void writeHeader(this ref ResponseRecorder rw, slice<byte> b, @string str)
        {
            if (rw.wroteHeader)
            {
                return;
            }
            if (len(str) > 512L)
            {
                str = str[..512L];
            }
            var m = rw.Header();

            var (_, hasType) = m["Content-Type"];
            var hasTE = m.Get("Transfer-Encoding") != "";
            if (!hasType && !hasTE)
            {
                if (b == null)
                {
                    b = (slice<byte>)str;
                }
                m.Set("Content-Type", http.DetectContentType(b));
            }
            rw.WriteHeader(200L);
        }

        // Write always succeeds and writes to rw.Body, if not nil.
        private static (long, error) Write(this ref ResponseRecorder rw, slice<byte> buf)
        {
            rw.writeHeader(buf, "");
            if (rw.Body != null)
            {
                rw.Body.Write(buf);
            }
            return (len(buf), null);
        }

        // WriteString always succeeds and writes to rw.Body, if not nil.
        private static (long, error) WriteString(this ref ResponseRecorder rw, @string str)
        {
            rw.writeHeader(null, str);
            if (rw.Body != null)
            {
                rw.Body.WriteString(str);
            }
            return (len(str), null);
        }

        // WriteHeader sets rw.Code. After it is called, changing rw.Header
        // will not affect rw.HeaderMap.
        private static void WriteHeader(this ref ResponseRecorder rw, long code)
        {
            if (rw.wroteHeader)
            {
                return;
            }
            rw.Code = code;
            rw.wroteHeader = true;
            if (rw.HeaderMap == null)
            {
                rw.HeaderMap = make(http.Header);
            }
            rw.snapHeader = cloneHeader(rw.HeaderMap);
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

        // Flush sets rw.Flushed to true.
        private static void Flush(this ref ResponseRecorder rw)
        {
            if (!rw.wroteHeader)
            {
                rw.WriteHeader(200L);
            }
            rw.Flushed = true;
        }

        // Result returns the response generated by the handler.
        //
        // The returned Response will have at least its StatusCode,
        // Header, Body, and optionally Trailer populated.
        // More fields may be populated in the future, so callers should
        // not DeepEqual the result in tests.
        //
        // The Response.Header is a snapshot of the headers at the time of the
        // first write call, or at the time of this call, if the handler never
        // did a write.
        //
        // The Response.Body is guaranteed to be non-nil and Body.Read call is
        // guaranteed to not return any error other than io.EOF.
        //
        // Result must only be called after the handler has finished running.
        private static ref http.Response Result(this ref ResponseRecorder rw)
        {
            if (rw.result != null)
            {
                return rw.result;
            }
            if (rw.snapHeader == null)
            {
                rw.snapHeader = cloneHeader(rw.HeaderMap);
            }
            http.Response res = ref new http.Response(Proto:"HTTP/1.1",ProtoMajor:1,ProtoMinor:1,StatusCode:rw.Code,Header:rw.snapHeader,);
            rw.result = res;
            if (res.StatusCode == 0L)
            {
                res.StatusCode = 200L;
            }
            res.Status = fmt.Sprintf("%03d %s", res.StatusCode, http.StatusText(res.StatusCode));
            if (rw.Body != null)
            {
                res.Body = ioutil.NopCloser(bytes.NewReader(rw.Body.Bytes()));
            }
            res.ContentLength = parseContentLength(res.Header.Get("Content-Length"));

            {
                var (trailers, ok) = rw.snapHeader["Trailer"];

                if (ok)
                {
                    res.Trailer = make(http.Header, len(trailers));
                    {
                        var k__prev1 = k;

                        foreach (var (_, __k) in trailers)
                        {
                            k = __k; 
                            // TODO: use http2.ValidTrailerHeader, but we can't
                            // get at it easily because it's bundled into net/http
                            // unexported. This is good enough for now:
                            switch (k)
                            {
                                case "Transfer-Encoding": 
                                    // Ignore since forbidden by RFC 2616 14.40.

                                case "Content-Length": 
                                    // Ignore since forbidden by RFC 2616 14.40.

                                case "Trailer": 
                                    // Ignore since forbidden by RFC 2616 14.40.
                                    continue;
                                    break;
                            }
                            k = http.CanonicalHeaderKey(k);
                            var (vv, ok) = rw.HeaderMap[k];
                            if (!ok)
                            {
                                continue;
                            }
                            var vv2 = make_slice<@string>(len(vv));
                            copy(vv2, vv);
                            res.Trailer[k] = vv2;
                        }

                        k = k__prev1;
                    }

                }

            }
            {
                var k__prev1 = k;
                var vv__prev1 = vv;

                foreach (var (__k, __vv) in rw.HeaderMap)
                {
                    k = __k;
                    vv = __vv;
                    if (!strings.HasPrefix(k, http.TrailerPrefix))
                    {
                        continue;
                    }
                    if (res.Trailer == null)
                    {
                        res.Trailer = make(http.Header);
                    }
                    foreach (var (_, v) in vv)
                    {
                        res.Trailer.Add(strings.TrimPrefix(k, http.TrailerPrefix), v);
                    }
                }

                k = k__prev1;
                vv = vv__prev1;
            }

            return res;
        }

        // parseContentLength trims whitespace from s and returns -1 if no value
        // is set, or the value if it's >= 0.
        //
        // This a modified version of same function found in net/http/transfer.go. This
        // one just ignores an invalid header.
        private static long parseContentLength(@string cl)
        {
            cl = strings.TrimSpace(cl);
            if (cl == "")
            {
                return -1L;
            }
            var (n, err) = strconv.ParseInt(cl, 10L, 64L);
            if (err != null)
            {
                return -1L;
            }
            return n;
        }
    }
}}}
