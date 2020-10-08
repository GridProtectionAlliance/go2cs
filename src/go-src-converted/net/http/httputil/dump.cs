// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package httputil -- go2cs converted at 2020 October 08 03:41:32 UTC
// import "net/http/httputil" ==> using httputil = go.net.http.httputil_package
// Original source: C:\Go\src\net\http\httputil\dump.go
using bufio = go.bufio_package;
using bytes = go.bytes_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using io = go.io_package;
using ioutil = go.io.ioutil_package;
using net = go.net_package;
using http = go.net.http_package;
using url = go.net.url_package;
using strings = go.strings_package;
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
        // drainBody reads all of b to memory and then returns two equivalent
        // ReadClosers yielding the same bytes.
        //
        // It returns an error if the initial slurp of all bytes fails. It does not attempt
        // to make the returned ReadClosers have identical error-matching behavior.
        private static (io.ReadCloser, io.ReadCloser, error) drainBody(io.ReadCloser b)
        {
            io.ReadCloser r1 = default;
            io.ReadCloser r2 = default;
            error err = default!;

            if (b == null || b == http.NoBody)
            { 
                // No copying needed. Preserve the magic sentinel meaning of NoBody.
                return (http.NoBody, http.NoBody, error.As(null!)!);

            }
            ref bytes.Buffer buf = ref heap(out ptr<bytes.Buffer> _addr_buf);
            _, err = buf.ReadFrom(b);

            if (err != null)
            {
                return (null, b, error.As(err)!);
            }
            err = b.Close();

            if (err != null)
            {
                return (null, b, error.As(err)!);
            }
            return (ioutil.NopCloser(_addr_buf), ioutil.NopCloser(bytes.NewReader(buf.Bytes())), error.As(null!)!);

        }

        // dumpConn is a net.Conn which writes to Writer and reads from Reader
        private partial struct dumpConn : io.Writer, io.Reader
        {
            public ref io.Writer Writer => ref Writer_val;
            public ref io.Reader Reader => ref Reader_val;
        }

        private static error Close(this ptr<dumpConn> _addr_c)
        {
            ref dumpConn c = ref _addr_c.val;

            return error.As(null!)!;
        }
        private static net.Addr LocalAddr(this ptr<dumpConn> _addr_c)
        {
            ref dumpConn c = ref _addr_c.val;

            return null;
        }
        private static net.Addr RemoteAddr(this ptr<dumpConn> _addr_c)
        {
            ref dumpConn c = ref _addr_c.val;

            return null;
        }
        private static error SetDeadline(this ptr<dumpConn> _addr_c, time.Time t)
        {
            ref dumpConn c = ref _addr_c.val;

            return error.As(null!)!;
        }
        private static error SetReadDeadline(this ptr<dumpConn> _addr_c, time.Time t)
        {
            ref dumpConn c = ref _addr_c.val;

            return error.As(null!)!;
        }
        private static error SetWriteDeadline(this ptr<dumpConn> _addr_c, time.Time t)
        {
            ref dumpConn c = ref _addr_c.val;

            return error.As(null!)!;
        }

        private partial struct neverEnding // : byte
        {
        }

        private static (long, error) Read(this neverEnding b, slice<byte> p)
        {
            long n = default;
            error err = default!;

            foreach (var (i) in p)
            {
                p[i] = byte(b);
            }
            return (len(p), error.As(null!)!);

        }

        // outGoingLength is a copy of the unexported
        // (*http.Request).outgoingLength method.
        private static long outgoingLength(ptr<http.Request> _addr_req)
        {
            ref http.Request req = ref _addr_req.val;

            if (req.Body == null || req.Body == http.NoBody)
            {
                return 0L;
            }

            if (req.ContentLength != 0L)
            {
                return req.ContentLength;
            }

            return -1L;

        }

        // DumpRequestOut is like DumpRequest but for outgoing client requests. It
        // includes any headers that the standard http.Transport adds, such as
        // User-Agent.
        public static (slice<byte>, error) DumpRequestOut(ptr<http.Request> _addr_req, bool body) => func((defer, _, __) =>
        {
            slice<byte> _p0 = default;
            error _p0 = default!;
            ref http.Request req = ref _addr_req.val;

            var save = req.Body;
            var dummyBody = false;
            if (!body)
            {
                var contentLength = outgoingLength(_addr_req);
                if (contentLength != 0L)
                {
                    req.Body = ioutil.NopCloser(io.LimitReader(neverEnding('x'), contentLength));
                    dummyBody = true;
                }

            }
            else
            {
                error err = default!;
                save, req.Body, err = drainBody(req.Body);
                if (err != null)
                {
                    return (null, error.As(err)!);
                }

            } 

            // Since we're using the actual Transport code to write the request,
            // switch to http so the Transport doesn't try to do an SSL
            // negotiation with our dumpConn and its bytes.Buffer & pipe.
            // The wire format for https and http are the same, anyway.
            var reqSend = req;
            if (req.URL.Scheme == "https")
            {
                reqSend = @new<http.Request>();
                reqSend.val = req;
                reqSend.URL = @new<url.URL>();
                reqSend.URL.val = req.URL.val;
                reqSend.URL.Scheme = "http";
            } 

            // Use the actual Transport code to record what we would send
            // on the wire, but not using TCP.  Use a Transport with a
            // custom dialer that returns a fake net.Conn that waits
            // for the full input (and recording it), and then responds
            // with a dummy response.
            ref bytes.Buffer buf = ref heap(out ptr<bytes.Buffer> _addr_buf); // records the output
            var (pr, pw) = io.Pipe();
            defer(pr.Close());
            defer(pw.Close());
            ptr<delegateReader> dr = addr(new delegateReader(c:make(chanio.Reader)));

            ptr<http.Transport> t = addr(new http.Transport(Dial:func(net,addrstring)(net.Conn,error){return&dumpConn{io.MultiWriter(&buf,pw),dr},nil},));
            defer(t.CloseIdleConnections()); 

            // We need this channel to ensure that the reader
            // goroutine exits if t.RoundTrip returns an error.
            // See golang.org/issue/32571.
            var quitReadCh = make_channel<object>(); 
            // Wait for the request before replying with a dummy response:
            go_(() => () =>
            {
                var (req, err) = http.ReadRequest(bufio.NewReader(pr));
                if (err == null)
                { 
                    // Ensure all the body is read; otherwise
                    // we'll get a partial dump.
                    io.Copy(ioutil.Discard, req.Body);
                    req.Body.Close();

                }

            }());

            var (_, err) = t.RoundTrip(reqSend);

            req.Body = save;
            if (err != null)
            {
                pw.Close();
                quitReadCh.Send(/* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{});
                return (null, error.As(err)!);
            }

            var dump = buf.Bytes(); 

            // If we used a dummy body above, remove it now.
            // TODO: if the req.ContentLength is large, we allocate memory
            // unnecessarily just to slice it off here. But this is just
            // a debug function, so this is acceptable for now. We could
            // discard the body earlier if this matters.
            if (dummyBody)
            {
                {
                    var i = bytes.Index(dump, (slice<byte>)"\r\n\r\n");

                    if (i >= 0L)
                    {
                        dump = dump[..i + 4L];
                    }

                }

            }

            return (dump, error.As(null!)!);

        });

        // delegateReader is a reader that delegates to another reader,
        // once it arrives on a channel.
        private partial struct delegateReader
        {
            public channel<io.Reader> c;
            public io.Reader r; // nil until received from c
        }

        private static (long, error) Read(this ptr<delegateReader> _addr_r, slice<byte> p)
        {
            long _p0 = default;
            error _p0 = default!;
            ref delegateReader r = ref _addr_r.val;

            if (r.r == null)
            {
                r.r = r.c.Receive();
            }

            return r.r.Read(p);

        }

        // Return value if nonempty, def otherwise.
        private static @string valueOrDefault(@string value, @string def)
        {
            if (value != "")
            {
                return value;
            }

            return def;

        }

        private static map reqWriteExcludeHeaderDump = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, bool>{"Host":true,"Transfer-Encoding":true,"Trailer":true,};

        // DumpRequest returns the given request in its HTTP/1.x wire
        // representation. It should only be used by servers to debug client
        // requests. The returned representation is an approximation only;
        // some details of the initial request are lost while parsing it into
        // an http.Request. In particular, the order and case of header field
        // names are lost. The order of values in multi-valued headers is kept
        // intact. HTTP/2 requests are dumped in HTTP/1.x form, not in their
        // original binary representations.
        //
        // If body is true, DumpRequest also returns the body. To do so, it
        // consumes req.Body and then replaces it with a new io.ReadCloser
        // that yields the same bytes. If DumpRequest returns an error,
        // the state of req is undefined.
        //
        // The documentation for http.Request.Write details which fields
        // of req are included in the dump.
        public static (slice<byte>, error) DumpRequest(ptr<http.Request> _addr_req, bool body)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;
            ref http.Request req = ref _addr_req.val;

            error err = default!;
            var save = req.Body;
            if (!body || req.Body == null)
            {
                req.Body = null;
            }
            else
            {
                save, req.Body, err = drainBody(req.Body);
                if (err != null)
                {
                    return (null, error.As(err)!);
                }

            }

            ref bytes.Buffer b = ref heap(out ptr<bytes.Buffer> _addr_b); 

            // By default, print out the unmodified req.RequestURI, which
            // is always set for incoming server requests. But because we
            // previously used req.URL.RequestURI and the docs weren't
            // always so clear about when to use DumpRequest vs
            // DumpRequestOut, fall back to the old way if the caller
            // provides a non-server Request.
            var reqURI = req.RequestURI;
            if (reqURI == "")
            {
                reqURI = req.URL.RequestURI();
            }

            fmt.Fprintf(_addr_b, "%s %s HTTP/%d.%d\r\n", valueOrDefault(req.Method, "GET"), reqURI, req.ProtoMajor, req.ProtoMinor);

            var absRequestURI = strings.HasPrefix(req.RequestURI, "http://") || strings.HasPrefix(req.RequestURI, "https://");
            if (!absRequestURI)
            {
                var host = req.Host;
                if (host == "" && req.URL != null)
                {
                    host = req.URL.Host;
                }

                if (host != "")
                {
                    fmt.Fprintf(_addr_b, "Host: %s\r\n", host);
                }

            }

            var chunked = len(req.TransferEncoding) > 0L && req.TransferEncoding[0L] == "chunked";
            if (len(req.TransferEncoding) > 0L)
            {
                fmt.Fprintf(_addr_b, "Transfer-Encoding: %s\r\n", strings.Join(req.TransferEncoding, ","));
            }

            if (req.Close)
            {
                fmt.Fprintf(_addr_b, "Connection: close\r\n");
            }

            err = error.As(req.Header.WriteSubset(_addr_b, reqWriteExcludeHeaderDump))!;
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            io.WriteString(_addr_b, "\r\n");

            if (req.Body != null)
            {
                io.Writer dest = _addr_b;
                if (chunked)
                {
                    dest = NewChunkedWriter(dest);
                }

                _, err = io.Copy(dest, req.Body);
                if (chunked)
                {
                    dest._<io.Closer>().Close();
                    io.WriteString(_addr_b, "\r\n");
                }

            }

            req.Body = save;
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            return (b.Bytes(), error.As(null!)!);

        }

        // errNoBody is a sentinel error value used by failureToReadBody so we
        // can detect that the lack of body was intentional.
        private static var errNoBody = errors.New("sentinel error value");

        // failureToReadBody is a io.ReadCloser that just returns errNoBody on
        // Read. It's swapped in when we don't actually want to consume
        // the body, but need a non-nil one, and want to distinguish the
        // error from reading the dummy body.
        private partial struct failureToReadBody
        {
        }

        private static (long, error) Read(this failureToReadBody _p0, slice<byte> _p0)
        {
            long _p0 = default;
            error _p0 = default!;

            return (0L, error.As(errNoBody)!);
        }
        private static error Close(this failureToReadBody _p0)
        {
            return error.As(null!)!;
        }

        // emptyBody is an instance of empty reader.
        private static var emptyBody = ioutil.NopCloser(strings.NewReader(""));

        // DumpResponse is like DumpRequest but dumps a response.
        public static (slice<byte>, error) DumpResponse(ptr<http.Response> _addr_resp, bool body)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;
            ref http.Response resp = ref _addr_resp.val;

            ref bytes.Buffer b = ref heap(out ptr<bytes.Buffer> _addr_b);
            error err = default!;
            var save = resp.Body;
            var savecl = resp.ContentLength;

            if (!body)
            { 
                // For content length of zero. Make sure the body is an empty
                // reader, instead of returning error through failureToReadBody{}.
                if (resp.ContentLength == 0L)
                {
                    resp.Body = emptyBody;
                }
                else
                {
                    resp.Body = new failureToReadBody();
                }

            }
            else if (resp.Body == null)
            {
                resp.Body = emptyBody;
            }
            else
            {
                save, resp.Body, err = drainBody(resp.Body);
                if (err != null)
                {
                    return (null, error.As(err)!);
                }

            }

            err = error.As(resp.Write(_addr_b))!;
            if (err == errNoBody)
            {
                err = error.As(null)!;
            }

            resp.Body = save;
            resp.ContentLength = savecl;
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            return (b.Bytes(), error.As(null!)!);

        }
    }
}}}
