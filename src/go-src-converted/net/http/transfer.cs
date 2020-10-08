// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package http -- go2cs converted at 2020 October 08 03:40:38 UTC
// import "net/http" ==> using http = go.net.http_package
// Original source: C:\Go\src\net\http\transfer.go
using bufio = go.bufio_package;
using bytes = go.bytes_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using io = go.io_package;
using ioutil = go.io.ioutil_package;
using httptrace = go.net.http.httptrace_package;
using @internal = go.net.http.@internal_package;
using textproto = go.net.textproto_package;
using reflect = go.reflect_package;
using sort = go.sort_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using sync = go.sync_package;
using time = go.time_package;

using httpguts = go.golang.org.x.net.http.httpguts_package;
using static go.builtin;
using System;
using System.Threading;

namespace go {
namespace net
{
    public static partial class http_package
    {
        // ErrLineTooLong is returned when reading request or response bodies
        // with malformed chunked encoding.
        public static var ErrLineTooLong = @internal.ErrLineTooLong;

        private partial struct errorReader
        {
            public error err;
        }

        private static (long, error) Read(this errorReader r, slice<byte> p)
        {
            long n = default;
            error err = default!;

            return (0L, error.As(r.err)!);
        }

        private partial struct byteReader
        {
            public byte b;
            public bool done;
        }

        private static (long, error) Read(this ptr<byteReader> _addr_br, slice<byte> p)
        {
            long n = default;
            error err = default!;
            ref byteReader br = ref _addr_br.val;

            if (br.done)
            {
                return (0L, error.As(io.EOF)!);
            }

            if (len(p) == 0L)
            {
                return (0L, error.As(null!)!);
            }

            br.done = true;
            p[0L] = br.b;
            return (1L, error.As(io.EOF)!);

        }

        // transferWriter inspects the fields of a user-supplied Request or Response,
        // sanitizes them without changing the user object and provides methods for
        // writing the respective header, body and trailer in wire format.
        private partial struct transferWriter
        {
            public @string Method;
            public io.Reader Body;
            public io.Closer BodyCloser;
            public bool ResponseToHEAD;
            public long ContentLength; // -1 means unknown, 0 means exactly none
            public bool Close;
            public slice<@string> TransferEncoding;
            public Header Header;
            public Header Trailer;
            public bool IsResponse;
            public error bodyReadError; // any non-EOF error from reading Body

            public bool FlushHeaders; // flush headers to network before body
            public channel<readResult> ByteReadCh; // non-nil if probeRequestBody called
        }

        private static (ptr<transferWriter>, error) newTransferWriter(object r)
        {
            ptr<transferWriter> t = default!;
            error err = default!;

            t = addr(new transferWriter()); 

            // Extract relevant fields
            var atLeastHTTP11 = false;
            switch (r.type())
            {
                case ptr<Request> rr:
                    if (rr.ContentLength != 0L && rr.Body == null)
                    {
                        return (_addr_null!, error.As(fmt.Errorf("http: Request.ContentLength=%d with nil Body", rr.ContentLength))!);
                    }

                    t.Method = valueOrDefault(rr.Method, "GET");
                    t.Close = rr.Close;
                    t.TransferEncoding = rr.TransferEncoding;
                    t.Header = rr.Header;
                    t.Trailer = rr.Trailer;
                    t.Body = rr.Body;
                    t.BodyCloser = rr.Body;
                    t.ContentLength = rr.outgoingLength();
                    if (t.ContentLength < 0L && len(t.TransferEncoding) == 0L && t.shouldSendChunkedRequestBody())
                    {
                        t.TransferEncoding = new slice<@string>(new @string[] { "chunked" });
                    } 
                    // If there's a body, conservatively flush the headers
                    // to any bufio.Writer we're writing to, just in case
                    // the server needs the headers early, before we copy
                    // the body and possibly block. We make an exception
                    // for the common standard library in-memory types,
                    // though, to avoid unnecessary TCP packets on the
                    // wire. (Issue 22088.)
                    if (t.ContentLength != 0L && !isKnownInMemoryReader(t.Body))
                    {
                        t.FlushHeaders = true;
                    }

                    atLeastHTTP11 = true; // Transport requests are always 1.1 or 2.0
                    break;
                case ptr<Response> rr:
                    t.IsResponse = true;
                    if (rr.Request != null)
                    {
                        t.Method = rr.Request.Method;
                    }

                    t.Body = rr.Body;
                    t.BodyCloser = rr.Body;
                    t.ContentLength = rr.ContentLength;
                    t.Close = rr.Close;
                    t.TransferEncoding = rr.TransferEncoding;
                    t.Header = rr.Header;
                    t.Trailer = rr.Trailer;
                    atLeastHTTP11 = rr.ProtoAtLeast(1L, 1L);
                    t.ResponseToHEAD = noResponseBodyExpected(t.Method);
                    break; 

                // Sanitize Body,ContentLength,TransferEncoding
            } 

            // Sanitize Body,ContentLength,TransferEncoding
            if (t.ResponseToHEAD)
            {
                t.Body = null;
                if (chunked(t.TransferEncoding))
                {
                    t.ContentLength = -1L;
                }

            }
            else
            {
                if (!atLeastHTTP11 || t.Body == null)
                {
                    t.TransferEncoding = null;
                }

                if (chunked(t.TransferEncoding))
                {
                    t.ContentLength = -1L;
                }
                else if (t.Body == null)
                { // no chunking, no body
                    t.ContentLength = 0L;

                }

            } 

            // Sanitize Trailer
            if (!chunked(t.TransferEncoding))
            {
                t.Trailer = null;
            }

            return (_addr_t!, error.As(null!)!);

        }

        // shouldSendChunkedRequestBody reports whether we should try to send a
        // chunked request body to the server. In particular, the case we really
        // want to prevent is sending a GET or other typically-bodyless request to a
        // server with a chunked body when the body has zero bytes, since GETs with
        // bodies (while acceptable according to specs), even zero-byte chunked
        // bodies, are approximately never seen in the wild and confuse most
        // servers. See Issue 18257, as one example.
        //
        // The only reason we'd send such a request is if the user set the Body to a
        // non-nil value (say, ioutil.NopCloser(bytes.NewReader(nil))) and didn't
        // set ContentLength, or NewRequest set it to -1 (unknown), so then we assume
        // there's bytes to send.
        //
        // This code tries to read a byte from the Request.Body in such cases to see
        // whether the body actually has content (super rare) or is actually just
        // a non-nil content-less ReadCloser (the more common case). In that more
        // common case, we act as if their Body were nil instead, and don't send
        // a body.
        private static bool shouldSendChunkedRequestBody(this ptr<transferWriter> _addr_t)
        {
            ref transferWriter t = ref _addr_t.val;
 
            // Note that t.ContentLength is the corrected content length
            // from rr.outgoingLength, so 0 actually means zero, not unknown.
            if (t.ContentLength >= 0L || t.Body == null)
            { // redundant checks; caller did them
                return false;

            }

            if (t.Method == "CONNECT")
            {
                return false;
            }

            if (requestMethodUsuallyLacksBody(t.Method))
            { 
                // Only probe the Request.Body for GET/HEAD/DELETE/etc
                // requests, because it's only those types of requests
                // that confuse servers.
                t.probeRequestBody(); // adjusts t.Body, t.ContentLength
                return t.Body != null;

            } 
            // For all other request types (PUT, POST, PATCH, or anything
            // made-up we've never heard of), assume it's normal and the server
            // can deal with a chunked request body. Maybe we'll adjust this
            // later.
            return true;

        }

        // probeRequestBody reads a byte from t.Body to see whether it's empty
        // (returns io.EOF right away).
        //
        // But because we've had problems with this blocking users in the past
        // (issue 17480) when the body is a pipe (perhaps waiting on the response
        // headers before the pipe is fed data), we need to be careful and bound how
        // long we wait for it. This delay will only affect users if all the following
        // are true:
        //   * the request body blocks
        //   * the content length is not set (or set to -1)
        //   * the method doesn't usually have a body (GET, HEAD, DELETE, ...)
        //   * there is no transfer-encoding=chunked already set.
        // In other words, this delay will not normally affect anybody, and there
        // are workarounds if it does.
        private static void probeRequestBody(this ptr<transferWriter> _addr_t)
        {
            ref transferWriter t = ref _addr_t.val;

            t.ByteReadCh = make_channel<readResult>(1L);
            go_(() => body =>
            {
                array<byte> buf = new array<byte>(1L);
                readResult rres = default;
                rres.n, rres.err = body.Read(buf[..]);
                if (rres.n == 1L)
                {
                    rres.b = buf[0L];
                }

                t.ByteReadCh.Send(rres);

            }(t.Body));
            var timer = time.NewTimer(200L * time.Millisecond);
            timer.Stop();
            if (rres.n == 0L && rres.err == io.EOF)
            { 
                // It was empty.
                t.Body = null;
                t.ContentLength = 0L;

            }
            else if (rres.n == 1L)
            {
                if (rres.err != null)
                {
                    t.Body = io.MultiReader(addr(new byteReader(b:rres.b)), new errorReader(rres.err));
                }
                else
                {
                    t.Body = io.MultiReader(addr(new byteReader(b:rres.b)), t.Body);
                }

            }
            else if (rres.err != null)
            {
                t.Body = new errorReader(rres.err);
            }

            t.Body = io.MultiReader(new finishAsyncByteRead(t), t.Body); 
            // Request that Request.Write flush the headers to the
            // network before writing the body, since our body may not
            // become readable until it's seen the response headers.
            t.FlushHeaders = true;

        }

        private static bool noResponseBodyExpected(@string requestMethod)
        {
            return requestMethod == "HEAD";
        }

        private static bool shouldSendContentLength(this ptr<transferWriter> _addr_t)
        {
            ref transferWriter t = ref _addr_t.val;

            if (chunked(t.TransferEncoding))
            {
                return false;
            }

            if (t.ContentLength > 0L)
            {
                return true;
            }

            if (t.ContentLength < 0L)
            {
                return false;
            } 
            // Many servers expect a Content-Length for these methods
            if (t.Method == "POST" || t.Method == "PUT")
            {
                return true;
            }

            if (t.ContentLength == 0L && isIdentity(t.TransferEncoding))
            {
                if (t.Method == "GET" || t.Method == "HEAD")
                {
                    return false;
                }

                return true;

            }

            return false;

        }

        private static error writeHeader(this ptr<transferWriter> _addr_t, io.Writer w, ptr<httptrace.ClientTrace> _addr_trace)
        {
            ref transferWriter t = ref _addr_t.val;
            ref httptrace.ClientTrace trace = ref _addr_trace.val;

            if (t.Close && !hasToken(t.Header.get("Connection"), "close"))
            {
                {
                    var (_, err) = io.WriteString(w, "Connection: close\r\n");

                    if (err != null)
                    {
                        return error.As(err)!;
                    }

                }

                if (trace != null && trace.WroteHeaderField != null)
                {
                    trace.WroteHeaderField("Connection", new slice<@string>(new @string[] { "close" }));
                }

            } 

            // Write Content-Length and/or Transfer-Encoding whose values are a
            // function of the sanitized field triple (Body, ContentLength,
            // TransferEncoding)
            if (t.shouldSendContentLength())
            {
                {
                    (_, err) = io.WriteString(w, "Content-Length: ");

                    if (err != null)
                    {
                        return error.As(err)!;
                    }

                }

                {
                    (_, err) = io.WriteString(w, strconv.FormatInt(t.ContentLength, 10L) + "\r\n");

                    if (err != null)
                    {
                        return error.As(err)!;
                    }

                }

                if (trace != null && trace.WroteHeaderField != null)
                {
                    trace.WroteHeaderField("Content-Length", new slice<@string>(new @string[] { strconv.FormatInt(t.ContentLength,10) }));
                }

            }
            else if (chunked(t.TransferEncoding))
            {
                {
                    (_, err) = io.WriteString(w, "Transfer-Encoding: chunked\r\n");

                    if (err != null)
                    {
                        return error.As(err)!;
                    }

                }

                if (trace != null && trace.WroteHeaderField != null)
                {
                    trace.WroteHeaderField("Transfer-Encoding", new slice<@string>(new @string[] { "chunked" }));
                }

            } 

            // Write Trailer header
            if (t.Trailer != null)
            {
                var keys = make_slice<@string>(0L, len(t.Trailer));
                foreach (var (k) in t.Trailer)
                {
                    k = CanonicalHeaderKey(k);
                    switch (k)
                    {
                        case "Transfer-Encoding": 

                        case "Trailer": 

                        case "Content-Length": 
                            return error.As(badStringError("invalid Trailer key", k))!;
                            break;
                    }
                    keys = append(keys, k);

                }
                if (len(keys) > 0L)
                {
                    sort.Strings(keys); 
                    // TODO: could do better allocation-wise here, but trailers are rare,
                    // so being lazy for now.
                    {
                        (_, err) = io.WriteString(w, "Trailer: " + strings.Join(keys, ",") + "\r\n");

                        if (err != null)
                        {
                            return error.As(err)!;
                        }

                    }

                    if (trace != null && trace.WroteHeaderField != null)
                    {
                        trace.WroteHeaderField("Trailer", keys);
                    }

                }

            }

            return error.As(null!)!;

        }

        private static error writeBody(this ptr<transferWriter> _addr_t, io.Writer w)
        {
            ref transferWriter t = ref _addr_t.val;

            error err = default!;
            long ncopy = default; 

            // Write body. We "unwrap" the body first if it was wrapped in a
            // nopCloser or readTrackingBody. This is to ensure that we can take advantage of
            // OS-level optimizations in the event that the body is an
            // *os.File.
            if (t.Body != null)
            {
                var body = t.unwrapBody();
                if (chunked(t.TransferEncoding))
                {
                    {
                        ptr<bufio.Writer> (bw, ok) = w._<ptr<bufio.Writer>>();

                        if (ok && !t.IsResponse)
                        {
                            w = addr(new internal.FlushAfterChunkWriter(Writer:bw));
                        }

                    }

                    var cw = @internal.NewChunkedWriter(w);
                    _, err = t.doBodyCopy(cw, body);
                    if (err == null)
                    {
                        err = error.As(cw.Close())!;
                    }

                }
                else if (t.ContentLength == -1L)
                {
                    var dst = w;
                    if (t.Method == "CONNECT")
                    {
                        dst = new bufioFlushWriter(dst);
                    }

                    ncopy, err = t.doBodyCopy(dst, body);

                }
                else
                {
                    ncopy, err = t.doBodyCopy(w, io.LimitReader(body, t.ContentLength));
                    if (err != null)
                    {
                        return error.As(err)!;
                    }

                    long nextra = default;
                    nextra, err = t.doBodyCopy(ioutil.Discard, body);
                    ncopy += nextra;

                }

                if (err != null)
                {
                    return error.As(err)!;
                }

            }

            if (t.BodyCloser != null)
            {
                {
                    error err__prev2 = err;

                    err = t.BodyCloser.Close();

                    if (err != null)
                    {
                        return error.As(err)!;
                    }

                    err = err__prev2;

                }

            }

            if (!t.ResponseToHEAD && t.ContentLength != -1L && t.ContentLength != ncopy)
            {
                return error.As(fmt.Errorf("http: ContentLength=%d with Body length %d", t.ContentLength, ncopy))!;
            }

            if (chunked(t.TransferEncoding))
            { 
                // Write Trailer header
                if (t.Trailer != null)
                {
                    {
                        error err__prev3 = err;

                        err = t.Trailer.Write(w);

                        if (err != null)
                        {
                            return error.As(err)!;
                        }

                        err = err__prev3;

                    }

                } 
                // Last chunk, empty trailer
                _, err = io.WriteString(w, "\r\n");

            }

            return error.As(err)!;

        }

        // doBodyCopy wraps a copy operation, with any resulting error also
        // being saved in bodyReadError.
        //
        // This function is only intended for use in writeBody.
        private static (long, error) doBodyCopy(this ptr<transferWriter> _addr_t, io.Writer dst, io.Reader src)
        {
            long n = default;
            error err = default!;
            ref transferWriter t = ref _addr_t.val;

            n, err = io.Copy(dst, src);
            if (err != null && err != io.EOF)
            {
                t.bodyReadError = err;
            }

            return ;

        }

        // unwrapBodyReader unwraps the body's inner reader if it's a
        // nopCloser. This is to ensure that body writes sourced from local
        // files (*os.File types) are properly optimized.
        //
        // This function is only intended for use in writeBody.
        private static io.Reader unwrapBody(this ptr<transferWriter> _addr_t)
        {
            ref transferWriter t = ref _addr_t.val;

            if (reflect.TypeOf(t.Body) == nopCloserType)
            {
                return reflect.ValueOf(t.Body).Field(0L).Interface()._<io.Reader>();
            }

            {
                ptr<readTrackingBody> (r, ok) = t.Body._<ptr<readTrackingBody>>();

                if (ok)
                {
                    r.didRead = true;
                    return r.ReadCloser;
                }

            }

            return t.Body;

        }

        private partial struct transferReader
        {
            public Header Header;
            public long StatusCode;
            public @string RequestMethod;
            public long ProtoMajor;
            public long ProtoMinor; // Output
            public io.ReadCloser Body;
            public long ContentLength;
            public bool Chunked;
            public bool Close;
            public Header Trailer;
        }

        private static bool protoAtLeast(this ptr<transferReader> _addr_t, long m, long n)
        {
            ref transferReader t = ref _addr_t.val;

            return t.ProtoMajor > m || (t.ProtoMajor == m && t.ProtoMinor >= n);
        }

        // bodyAllowedForStatus reports whether a given response status code
        // permits a body. See RFC 7230, section 3.3.
        private static bool bodyAllowedForStatus(long status)
        {

            if (status >= 100L && status <= 199L) 
                return false;
            else if (status == 204L) 
                return false;
            else if (status == 304L) 
                return false;
                        return true;

        }

        private static @string suppressedHeaders304 = new slice<@string>(new @string[] { "Content-Type", "Content-Length", "Transfer-Encoding" });        private static @string suppressedHeadersNoBody = new slice<@string>(new @string[] { "Content-Length", "Transfer-Encoding" });

        private static slice<@string> suppressedHeaders(long status)
        {

            if (status == 304L) 
                // RFC 7232 section 4.1
                return suppressedHeaders304;
            else if (!bodyAllowedForStatus(status)) 
                return suppressedHeadersNoBody;
                        return null;

        }

        // msg is *Request or *Response.
        private static error readTransfer(object msg, ptr<bufio.Reader> _addr_r) => func((_, panic, __) =>
        {
            error err = default!;
            ref bufio.Reader r = ref _addr_r.val;

            ptr<transferReader> t = addr(new transferReader(RequestMethod:"GET")); 

            // Unify input
            var isResponse = false;
            switch (msg.type())
            {
                case ptr<Response> rr:
                    t.Header = rr.Header;
                    t.StatusCode = rr.StatusCode;
                    t.ProtoMajor = rr.ProtoMajor;
                    t.ProtoMinor = rr.ProtoMinor;
                    t.Close = shouldClose(t.ProtoMajor, t.ProtoMinor, t.Header, true);
                    isResponse = true;
                    if (rr.Request != null)
                    {
                        t.RequestMethod = rr.Request.Method;
                    }

                    break;
                case ptr<Request> rr:
                    t.Header = rr.Header;
                    t.RequestMethod = rr.Method;
                    t.ProtoMajor = rr.ProtoMajor;
                    t.ProtoMinor = rr.ProtoMinor; 
                    // Transfer semantics for Requests are exactly like those for
                    // Responses with status code 200, responding to a GET method
                    t.StatusCode = 200L;
                    t.Close = rr.Close;
                    break;
                default:
                {
                    var rr = msg.type();
                    panic("unexpected type");
                    break;
                } 

                // Default to HTTP/1.1
            } 

            // Default to HTTP/1.1
            if (t.ProtoMajor == 0L && t.ProtoMinor == 0L)
            {
                t.ProtoMajor = 1L;
                t.ProtoMinor = 1L;

            } 

            // Transfer-Encoding: chunked, and overriding Content-Length.
            {
                var err = t.parseTransferEncoding();

                if (err != null)
                {
                    return error.As(err)!;
                }

            }


            var (realLength, err) = fixLength(isResponse, t.StatusCode, t.RequestMethod, t.Header, t.Chunked);
            if (err != null)
            {
                return error.As(err)!;
            }

            if (isResponse && t.RequestMethod == "HEAD")
            {
                {
                    var (n, err) = parseContentLength(t.Header.get("Content-Length"));

                    if (err != null)
                    {
                        return error.As(err)!;
                    }
                    else
                    {
                        t.ContentLength = n;
                    }

                }

            }
            else
            {
                t.ContentLength = realLength;
            } 

            // Trailer
            t.Trailer, err = fixTrailer(t.Header, t.Chunked);
            if (err != null)
            {
                return error.As(err)!;
            } 

            // If there is no Content-Length or chunked Transfer-Encoding on a *Response
            // and the status is not 1xx, 204 or 304, then the body is unbounded.
            // See RFC 7230, section 3.3.
            switch (msg.type())
            {
                case ptr<Response> _:
                    if (realLength == -1L && !t.Chunked && bodyAllowedForStatus(t.StatusCode))
                    { 
                        // Unbounded body.
                        t.Close = true;

                    }

                    break; 

                // Prepare body reader. ContentLength < 0 means chunked encoding
                // or close connection when finished, since multipart is not supported yet
            } 

            // Prepare body reader. ContentLength < 0 means chunked encoding
            // or close connection when finished, since multipart is not supported yet

            if (t.Chunked) 
                if (noResponseBodyExpected(t.RequestMethod) || !bodyAllowedForStatus(t.StatusCode))
                {
                    t.Body = NoBody;
                }
                else
                {
                    t.Body = addr(new body(src:internal.NewChunkedReader(r),hdr:msg,r:r,closing:t.Close));
                }

            else if (realLength == 0L) 
                t.Body = NoBody;
            else if (realLength > 0L) 
                t.Body = addr(new body(src:io.LimitReader(r,realLength),closing:t.Close));
            else 
                // realLength < 0, i.e. "Content-Length" not mentioned in header
                if (t.Close)
                { 
                    // Close semantics (i.e. HTTP/1.0)
                    t.Body = addr(new body(src:r,closing:t.Close));

                }
                else
                { 
                    // Persistent connection (i.e. HTTP/1.1)
                    t.Body = NoBody;

                }

            // Unify output
            switch (msg.type())
            {
                case ptr<Request> rr:
                    rr.Body = t.Body;
                    rr.ContentLength = t.ContentLength;
                    if (t.Chunked)
                    {
                        rr.TransferEncoding = new slice<@string>(new @string[] { "chunked" });
                    }

                    rr.Close = t.Close;
                    rr.Trailer = t.Trailer;
                    break;
                case ptr<Response> rr:
                    rr.Body = t.Body;
                    rr.ContentLength = t.ContentLength;
                    if (t.Chunked)
                    {
                        rr.TransferEncoding = new slice<@string>(new @string[] { "chunked" });
                    }

                    rr.Close = t.Close;
                    rr.Trailer = t.Trailer;
                    break;

            }

            return error.As(null!)!;

        });

        // Checks whether chunked is part of the encodings stack
        private static bool chunked(slice<@string> te)
        {
            return len(te) > 0L && te[0L] == "chunked";
        }

        // Checks whether the encoding is explicitly "identity".
        private static bool isIdentity(slice<@string> te)
        {
            return len(te) == 1L && te[0L] == "identity";
        }

        // unsupportedTEError reports unsupported transfer-encodings.
        private partial struct unsupportedTEError
        {
            public @string err;
        }

        private static @string Error(this ptr<unsupportedTEError> _addr_uste)
        {
            ref unsupportedTEError uste = ref _addr_uste.val;

            return uste.err;
        }

        // isUnsupportedTEError checks if the error is of type
        // unsupportedTEError. It is usually invoked with a non-nil err.
        private static bool isUnsupportedTEError(error err)
        {
            ptr<unsupportedTEError> (_, ok) = err._<ptr<unsupportedTEError>>();
            return ok;
        }

        // parseTransferEncoding sets t.Chunked based on the Transfer-Encoding header.
        private static error parseTransferEncoding(this ptr<transferReader> _addr_t)
        {
            ref transferReader t = ref _addr_t.val;

            var (raw, present) = t.Header["Transfer-Encoding"];
            if (!present)
            {
                return error.As(null!)!;
            }

            delete(t.Header, "Transfer-Encoding"); 

            // Issue 12785; ignore Transfer-Encoding on HTTP/1.0 requests.
            if (!t.protoAtLeast(1L, 1L))
            {
                return error.As(null!)!;
            } 

            // Like nginx, we only support a single Transfer-Encoding header field, and
            // only if set to "chunked". This is one of the most security sensitive
            // surfaces in HTTP/1.1 due to the risk of request smuggling, so we keep it
            // strict and simple.
            if (len(raw) != 1L)
            {
                return error.As(addr(new unsupportedTEError(fmt.Sprintf("too many transfer encodings: %q",raw)))!)!;
            }

            if (strings.ToLower(textproto.TrimString(raw[0L])) != "chunked")
            {
                return error.As(addr(new unsupportedTEError(fmt.Sprintf("unsupported transfer encoding: %q",raw[0])))!)!;
            } 

            // RFC 7230 3.3.2 says "A sender MUST NOT send a Content-Length header field
            // in any message that contains a Transfer-Encoding header field."
            //
            // but also: "If a message is received with both a Transfer-Encoding and a
            // Content-Length header field, the Transfer-Encoding overrides the
            // Content-Length. Such a message might indicate an attempt to perform
            // request smuggling (Section 9.5) or response splitting (Section 9.4) and
            // ought to be handled as an error. A sender MUST remove the received
            // Content-Length field prior to forwarding such a message downstream."
            //
            // Reportedly, these appear in the wild.
            delete(t.Header, "Content-Length");

            t.Chunked = true;
            return error.As(null!)!;

        }

        // Determine the expected body length, using RFC 7230 Section 3.3. This
        // function is not a method, because ultimately it should be shared by
        // ReadResponse and ReadRequest.
        private static (long, error) fixLength(bool isResponse, long status, @string requestMethod, Header header, bool chunked)
        {
            long _p0 = default;
            error _p0 = default!;

            var isRequest = !isResponse;
            var contentLens = header["Content-Length"]; 

            // Hardening against HTTP request smuggling
            if (len(contentLens) > 1L)
            { 
                // Per RFC 7230 Section 3.3.2, prevent multiple
                // Content-Length headers if they differ in value.
                // If there are dups of the value, remove the dups.
                // See Issue 16490.
                var first = textproto.TrimString(contentLens[0L]);
                foreach (var (_, ct) in contentLens[1L..])
                {
                    if (first != textproto.TrimString(ct))
                    {
                        return (0L, error.As(fmt.Errorf("http: message cannot contain multiple Content-Length headers; got %q", contentLens))!);
                    }

                } 

                // deduplicate Content-Length
                header.Del("Content-Length");
                header.Add("Content-Length", first);

                contentLens = header["Content-Length"];

            } 

            // Logic based on response type or status
            if (noResponseBodyExpected(requestMethod))
            { 
                // For HTTP requests, as part of hardening against request
                // smuggling (RFC 7230), don't allow a Content-Length header for
                // methods which don't permit bodies. As an exception, allow
                // exactly one Content-Length header if its value is "0".
                if (isRequest && len(contentLens) > 0L && !(len(contentLens) == 1L && contentLens[0L] == "0"))
                {
                    return (0L, error.As(fmt.Errorf("http: method cannot contain a Content-Length; got %q", contentLens))!);
                }

                return (0L, error.As(null!)!);

            }

            if (status / 100L == 1L)
            {
                return (0L, error.As(null!)!);
            }

            switch (status)
            {
                case 204L: 

                case 304L: 
                    return (0L, error.As(null!)!);
                    break;
            } 

            // Logic based on Transfer-Encoding
            if (chunked)
            {
                return (-1L, error.As(null!)!);
            } 

            // Logic based on Content-Length
            @string cl = default;
            if (len(contentLens) == 1L)
            {
                cl = textproto.TrimString(contentLens[0L]);
            }

            if (cl != "")
            {
                var (n, err) = parseContentLength(cl);
                if (err != null)
                {
                    return (-1L, error.As(err)!);
                }

                return (n, error.As(null!)!);

            }

            header.Del("Content-Length");

            if (isRequest)
            { 
                // RFC 7230 neither explicitly permits nor forbids an
                // entity-body on a GET request so we permit one if
                // declared, but we default to 0 here (not -1 below)
                // if there's no mention of a body.
                // Likewise, all other request methods are assumed to have
                // no body if neither Transfer-Encoding chunked nor a
                // Content-Length are set.
                return (0L, error.As(null!)!);

            } 

            // Body-EOF logic based on other methods (like closing, or chunked coding)
            return (-1L, error.As(null!)!);

        }

        // Determine whether to hang up after sending a request and body, or
        // receiving a response and body
        // 'header' is the request headers
        private static bool shouldClose(long major, long minor, Header header, bool removeCloseHeader)
        {
            if (major < 1L)
            {
                return true;
            }

            var conv = header["Connection"];
            var hasClose = httpguts.HeaderValuesContainsToken(conv, "close");
            if (major == 1L && minor == 0L)
            {
                return hasClose || !httpguts.HeaderValuesContainsToken(conv, "keep-alive");
            }

            if (hasClose && removeCloseHeader)
            {
                header.Del("Connection");
            }

            return hasClose;

        }

        // Parse the trailer header
        private static (Header, error) fixTrailer(Header header, bool chunked)
        {
            Header _p0 = default;
            error _p0 = default!;

            var (vv, ok) = header["Trailer"];
            if (!ok)
            {
                return (null, error.As(null!)!);
            }

            if (!chunked)
            { 
                // Trailer and no chunking:
                // this is an invalid use case for trailer header.
                // Nevertheless, no error will be returned and we
                // let users decide if this is a valid HTTP message.
                // The Trailer header will be kept in Response.Header
                // but not populate Response.Trailer.
                // See issue #27197.
                return (null, error.As(null!)!);

            }

            header.Del("Trailer");

            var trailer = make(Header);
            error err = default!;
            foreach (var (_, v) in vv)
            {
                foreachHeaderElement(v, key =>
                {
                    key = CanonicalHeaderKey(key);
                    switch (key)
                    {
                        case "Transfer-Encoding": 

                        case "Trailer": 

                        case "Content-Length": 
                            if (err == null)
                            {
                                err = error.As(badStringError("bad trailer key", key))!;
                                return ;
                            }

                            break;
                    }
                    trailer[key] = null;

                });

            }
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            if (len(trailer) == 0L)
            {
                return (null, error.As(null!)!);
            }

            return (trailer, error.As(null!)!);

        }

        // body turns a Reader into a ReadCloser.
        // Close ensures that the body has been fully read
        // and then reads the trailer if necessary.
        private partial struct body
        {
            public io.Reader src;
            public ptr<bufio.Reader> r; // underlying wire-format reader for the trailer
            public bool closing; // is the connection to be closed after reading body?
            public bool doEarlyClose; // whether Close should stop early

            public sync.Mutex mu; // guards following, and calls to Read and Close
            public bool sawEOF;
            public bool closed;
            public bool earlyClose; // Close called and we didn't read to the end of src
            public Action onHitEOF; // if non-nil, func to call when EOF is Read
        }

        // ErrBodyReadAfterClose is returned when reading a Request or Response
        // Body after the body has been closed. This typically happens when the body is
        // read after an HTTP Handler calls WriteHeader or Write on its
        // ResponseWriter.
        public static var ErrBodyReadAfterClose = errors.New("http: invalid Read on closed Body");

        private static (long, error) Read(this ptr<body> _addr_b, slice<byte> p) => func((defer, _, __) =>
        {
            long n = default;
            error err = default!;
            ref body b = ref _addr_b.val;

            b.mu.Lock();
            defer(b.mu.Unlock());
            if (b.closed)
            {
                return (0L, error.As(ErrBodyReadAfterClose)!);
            }

            return b.readLocked(p);

        });

        // Must hold b.mu.
        private static (long, error) readLocked(this ptr<body> _addr_b, slice<byte> p)
        {
            long n = default;
            error err = default!;
            ref body b = ref _addr_b.val;

            if (b.sawEOF)
            {
                return (0L, error.As(io.EOF)!);
            }

            n, err = b.src.Read(p);

            if (err == io.EOF)
            {
                b.sawEOF = true; 
                // Chunked case. Read the trailer.
                if (b.hdr != null)
                {
                    {
                        var e = b.readTrailer();

                        if (e != null)
                        {
                            err = e; 
                            // Something went wrong in the trailer, we must not allow any
                            // further reads of any kind to succeed from body, nor any
                            // subsequent requests on the server connection. See
                            // golang.org/issue/12027
                            b.sawEOF = false;
                            b.closed = true;

                        }

                    }

                    b.hdr = null;

                }
                else
                { 
                    // If the server declared the Content-Length, our body is a LimitedReader
                    // and we need to check whether this EOF arrived early.
                    {
                        ptr<io.LimitedReader> lr__prev3 = lr;

                        ptr<io.LimitedReader> (lr, ok) = b.src._<ptr<io.LimitedReader>>();

                        if (ok && lr.N > 0L)
                        {
                            err = io.ErrUnexpectedEOF;
                        }

                        lr = lr__prev3;

                    }

                }

            } 

            // If we can return an EOF here along with the read data, do
            // so. This is optional per the io.Reader contract, but doing
            // so helps the HTTP transport code recycle its connection
            // earlier (since it will see this EOF itself), even if the
            // client doesn't do future reads or Close.
            if (err == null && n > 0L)
            {
                {
                    ptr<io.LimitedReader> lr__prev2 = lr;

                    (lr, ok) = b.src._<ptr<io.LimitedReader>>();

                    if (ok && lr.N == 0L)
                    {
                        err = io.EOF;
                        b.sawEOF = true;
                    }

                    lr = lr__prev2;

                }

            }

            if (b.sawEOF && b.onHitEOF != null)
            {
                b.onHitEOF();
            }

            return (n, error.As(err)!);

        }

        private static slice<byte> singleCRLF = (slice<byte>)"\r\n";        private static slice<byte> doubleCRLF = (slice<byte>)"\r\n\r\n";

        private static bool seeUpcomingDoubleCRLF(ptr<bufio.Reader> _addr_r)
        {
            ref bufio.Reader r = ref _addr_r.val;

            for (long peekSize = 4L; >>MARKER:FOREXPRESSION_LEVEL_1<<; peekSize++)
            { 
                // This loop stops when Peek returns an error,
                // which it does when r's buffer has been filled.
                var (buf, err) = r.Peek(peekSize);
                if (bytes.HasSuffix(buf, doubleCRLF))
                {
                    return true;
                }

                if (err != null)
                {
                    break;
                }

            }

            return false;

        }

        private static var errTrailerEOF = errors.New("http: unexpected EOF reading trailer");

        private static error readTrailer(this ptr<body> _addr_b)
        {
            ref body b = ref _addr_b.val;
 
            // The common case, since nobody uses trailers.
            var (buf, err) = b.r.Peek(2L);
            if (bytes.Equal(buf, singleCRLF))
            {
                b.r.Discard(2L);
                return error.As(null!)!;
            }

            if (len(buf) < 2L)
            {
                return error.As(errTrailerEOF)!;
            }

            if (err != null)
            {
                return error.As(err)!;
            } 

            // Make sure there's a header terminator coming up, to prevent
            // a DoS with an unbounded size Trailer. It's not easy to
            // slip in a LimitReader here, as textproto.NewReader requires
            // a concrete *bufio.Reader. Also, we can't get all the way
            // back up to our conn's LimitedReader that *might* be backing
            // this bufio.Reader. Instead, a hack: we iteratively Peek up
            // to the bufio.Reader's max size, looking for a double CRLF.
            // This limits the trailer to the underlying buffer size, typically 4kB.
            if (!seeUpcomingDoubleCRLF(_addr_b.r))
            {
                return error.As(errors.New("http: suspiciously long trailer after chunked body"))!;
            }

            var (hdr, err) = textproto.NewReader(b.r).ReadMIMEHeader();
            if (err != null)
            {
                if (err == io.EOF)
                {
                    return error.As(errTrailerEOF)!;
                }

                return error.As(err)!;

            }

            switch (b.hdr.type())
            {
                case ptr<Request> rr:
                    mergeSetHeader(_addr_rr.Trailer, Header(hdr));
                    break;
                case ptr<Response> rr:
                    mergeSetHeader(_addr_rr.Trailer, Header(hdr));
                    break;
            }
            return error.As(null!)!;

        }

        private static void mergeSetHeader(ptr<Header> _addr_dst, Header src)
        {
            ref Header dst = ref _addr_dst.val;

            if (dst == null.val)
            {
                dst = src;
                return ;
            }

            foreach (var (k, vv) in src)
            {
                (dst)[k] = vv;
            }

        }

        // unreadDataSizeLocked returns the number of bytes of unread input.
        // It returns -1 if unknown.
        // b.mu must be held.
        private static long unreadDataSizeLocked(this ptr<body> _addr_b)
        {
            ref body b = ref _addr_b.val;

            {
                ptr<io.LimitedReader> (lr, ok) = b.src._<ptr<io.LimitedReader>>();

                if (ok)
                {
                    return lr.N;
                }

            }

            return -1L;

        }

        private static error Close(this ptr<body> _addr_b) => func((defer, _, __) =>
        {
            ref body b = ref _addr_b.val;

            b.mu.Lock();
            defer(b.mu.Unlock());
            if (b.closed)
            {
                return error.As(null!)!;
            }

            error err = default!;

            if (b.sawEOF)             else if (b.hdr == null && b.closing)             else if (b.doEarlyClose) 
                // Read up to maxPostHandlerReadBytes bytes of the body, looking
                // for EOF (and trailers), so we can re-use this connection.
                {
                    ptr<io.LimitedReader> (lr, ok) = b.src._<ptr<io.LimitedReader>>();

                    if (ok && lr.N > maxPostHandlerReadBytes)
                    { 
                        // There was a declared Content-Length, and we have more bytes remaining
                        // than our maxPostHandlerReadBytes tolerance. So, give up.
                        b.earlyClose = true;

                    }
                    else
                    {
                        long n = default; 
                        // Consume the body, or, which will also lead to us reading
                        // the trailer headers after the body, if present.
                        n, err = io.CopyN(ioutil.Discard, new bodyLocked(b), maxPostHandlerReadBytes);
                        if (err == io.EOF)
                        {
                            err = error.As(null)!;
                        }

                        if (n == maxPostHandlerReadBytes)
                        {
                            b.earlyClose = true;
                        }

                    }

                }

            else 
                // Fully consume the body, which will also lead to us reading
                // the trailer headers after the body, if present.
                _, err = io.Copy(ioutil.Discard, new bodyLocked(b));
                        b.closed = true;
            return error.As(err)!;

        });

        private static bool didEarlyClose(this ptr<body> _addr_b) => func((defer, _, __) =>
        {
            ref body b = ref _addr_b.val;

            b.mu.Lock();
            defer(b.mu.Unlock());
            return b.earlyClose;
        });

        // bodyRemains reports whether future Read calls might
        // yield data.
        private static bool bodyRemains(this ptr<body> _addr_b) => func((defer, _, __) =>
        {
            ref body b = ref _addr_b.val;

            b.mu.Lock();
            defer(b.mu.Unlock());
            return !b.sawEOF;
        });

        private static void registerOnHitEOF(this ptr<body> _addr_b, Action fn) => func((defer, _, __) =>
        {
            ref body b = ref _addr_b.val;

            b.mu.Lock();
            defer(b.mu.Unlock());
            b.onHitEOF = fn;
        });

        // bodyLocked is a io.Reader reading from a *body when its mutex is
        // already held.
        private partial struct bodyLocked
        {
            public ptr<body> b;
        }

        private static (long, error) Read(this bodyLocked bl, slice<byte> p)
        {
            long n = default;
            error err = default!;

            if (bl.b.closed)
            {
                return (0L, error.As(ErrBodyReadAfterClose)!);
            }

            return bl.b.readLocked(p);

        }

        // parseContentLength trims whitespace from s and returns -1 if no value
        // is set, or the value if it's >= 0.
        private static (long, error) parseContentLength(@string cl)
        {
            long _p0 = default;
            error _p0 = default!;

            cl = textproto.TrimString(cl);
            if (cl == "")
            {
                return (-1L, error.As(null!)!);
            }

            var (n, err) = strconv.ParseUint(cl, 10L, 63L);
            if (err != null)
            {
                return (0L, error.As(badStringError("bad Content-Length", cl))!);
            }

            return (int64(n), error.As(null!)!);


        }

        // finishAsyncByteRead finishes reading the 1-byte sniff
        // from the ContentLength==0, Body!=nil case.
        private partial struct finishAsyncByteRead
        {
            public ptr<transferWriter> tw;
        }

        private static (long, error) Read(this finishAsyncByteRead fr, slice<byte> p)
        {
            long n = default;
            error err = default!;

            if (len(p) == 0L)
            {
                return ;
            }

            var rres = fr.tw.ByteReadCh.Receive();
            n = rres.n;
            err = rres.err;
            if (n == 1L)
            {
                p[0L] = rres.b;
            }

            return ;

        }

        private static var nopCloserType = reflect.TypeOf(ioutil.NopCloser(null));

        // isKnownInMemoryReader reports whether r is a type known to not
        // block on Read. Its caller uses this as an optional optimization to
        // send fewer TCP packets.
        private static bool isKnownInMemoryReader(io.Reader r)
        {
            switch (r.type())
            {
                case ptr<bytes.Reader> _:
                    return true;
                    break;
                case ptr<bytes.Buffer> _:
                    return true;
                    break;
                case ptr<strings.Reader> _:
                    return true;
                    break;
            }
            if (reflect.TypeOf(r) == nopCloserType)
            {
                return isKnownInMemoryReader(reflect.ValueOf(r).Field(0L).Interface()._<io.Reader>());
            }

            {
                ptr<readTrackingBody> (r, ok) = r._<ptr<readTrackingBody>>();

                if (ok)
                {
                    return isKnownInMemoryReader(r.ReadCloser);
                }

            }

            return false;

        }

        // bufioFlushWriter is an io.Writer wrapper that flushes all writes
        // on its wrapped writer if it's a *bufio.Writer.
        private partial struct bufioFlushWriter
        {
            public io.Writer w;
        }

        private static (long, error) Write(this bufioFlushWriter fw, slice<byte> p)
        {
            long n = default;
            error err = default!;

            n, err = fw.w.Write(p);
            {
                ptr<bufio.Writer> (bw, ok) = fw.w._<ptr<bufio.Writer>>();

                if (n > 0L && ok)
                {
                    var ferr = bw.Flush();
                    if (ferr != null && err == null)
                    {
                        err = ferr;
                    }

                }

            }

            return ;

        }
    }
}}
