// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// HTTP Response reading and parsing.

// package http -- go2cs converted at 2020 October 08 03:40:17 UTC
// import "net/http" ==> using http = go.net.http_package
// Original source: C:\Go\src\net\http\response.go
using bufio = go.bufio_package;
using bytes = go.bytes_package;
using tls = go.crypto.tls_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using io = go.io_package;
using textproto = go.net.textproto_package;
using url = go.net.url_package;
using strconv = go.strconv_package;
using strings = go.strings_package;

using httpguts = go.golang.org.x.net.http.httpguts_package;
using static go.builtin;

namespace go {
namespace net
{
    public static partial class http_package
    {
        private static map respExcludeHeader = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, bool>{"Content-Length":true,"Transfer-Encoding":true,"Trailer":true,};

        // Response represents the response from an HTTP request.
        //
        // The Client and Transport return Responses from servers once
        // the response headers have been received. The response body
        // is streamed on demand as the Body field is read.
        public partial struct Response
        {
            public @string Status; // e.g. "200 OK"
            public long StatusCode; // e.g. 200
            public @string Proto; // e.g. "HTTP/1.0"
            public long ProtoMajor; // e.g. 1
            public long ProtoMinor; // e.g. 0

// Header maps header keys to values. If the response had multiple
// headers with the same key, they may be concatenated, with comma
// delimiters.  (RFC 7230, section 3.2.2 requires that multiple headers
// be semantically equivalent to a comma-delimited sequence.) When
// Header values are duplicated by other fields in this struct (e.g.,
// ContentLength, TransferEncoding, Trailer), the field values are
// authoritative.
//
// Keys in the map are canonicalized (see CanonicalHeaderKey).
            public Header Header; // Body represents the response body.
//
// The response body is streamed on demand as the Body field
// is read. If the network connection fails or the server
// terminates the response, Body.Read calls return an error.
//
// The http Client and Transport guarantee that Body is always
// non-nil, even on responses without a body or responses with
// a zero-length body. It is the caller's responsibility to
// close Body. The default HTTP client's Transport may not
// reuse HTTP/1.x "keep-alive" TCP connections if the Body is
// not read to completion and closed.
//
// The Body is automatically dechunked if the server replied
// with a "chunked" Transfer-Encoding.
//
// As of Go 1.12, the Body will also implement io.Writer
// on a successful "101 Switching Protocols" response,
// as used by WebSockets and HTTP/2's "h2c" mode.
            public io.ReadCloser Body; // ContentLength records the length of the associated content. The
// value -1 indicates that the length is unknown. Unless Request.Method
// is "HEAD", values >= 0 indicate that the given number of bytes may
// be read from Body.
            public long ContentLength; // Contains transfer encodings from outer-most to inner-most. Value is
// nil, means that "identity" encoding is used.
            public slice<@string> TransferEncoding; // Close records whether the header directed that the connection be
// closed after reading Body. The value is advice for clients: neither
// ReadResponse nor Response.Write ever closes a connection.
            public bool Close; // Uncompressed reports whether the response was sent compressed but
// was decompressed by the http package. When true, reading from
// Body yields the uncompressed content instead of the compressed
// content actually set from the server, ContentLength is set to -1,
// and the "Content-Length" and "Content-Encoding" fields are deleted
// from the responseHeader. To get the original response from
// the server, set Transport.DisableCompression to true.
            public bool Uncompressed; // Trailer maps trailer keys to values in the same
// format as Header.
//
// The Trailer initially contains only nil values, one for
// each key specified in the server's "Trailer" header
// value. Those values are not added to Header.
//
// Trailer must not be accessed concurrently with Read calls
// on the Body.
//
// After Body.Read has returned io.EOF, Trailer will contain
// any trailer values sent by the server.
            public Header Trailer; // Request is the request that was sent to obtain this Response.
// Request's Body is nil (having already been consumed).
// This is only populated for Client requests.
            public ptr<Request> Request; // TLS contains information about the TLS connection on which the
// response was received. It is nil for unencrypted responses.
// The pointer is shared between responses and should not be
// modified.
            public ptr<tls.ConnectionState> TLS;
        }

        // Cookies parses and returns the cookies set in the Set-Cookie headers.
        private static slice<ptr<Cookie>> Cookies(this ptr<Response> _addr_r)
        {
            ref Response r = ref _addr_r.val;

            return readSetCookies(r.Header);
        }

        // ErrNoLocation is returned by Response's Location method
        // when no Location header is present.
        public static var ErrNoLocation = errors.New("http: no Location header in response");

        // Location returns the URL of the response's "Location" header,
        // if present. Relative redirects are resolved relative to
        // the Response's Request. ErrNoLocation is returned if no
        // Location header is present.
        private static (ptr<url.URL>, error) Location(this ptr<Response> _addr_r)
        {
            ptr<url.URL> _p0 = default!;
            error _p0 = default!;
            ref Response r = ref _addr_r.val;

            var lv = r.Header.Get("Location");
            if (lv == "")
            {
                return (_addr_null!, error.As(ErrNoLocation)!);
            }

            if (r.Request != null && r.Request.URL != null)
            {
                return _addr_r.Request.URL.Parse(lv)!;
            }

            return _addr_url.Parse(lv)!;

        }

        // ReadResponse reads and returns an HTTP response from r.
        // The req parameter optionally specifies the Request that corresponds
        // to this Response. If nil, a GET request is assumed.
        // Clients must call resp.Body.Close when finished reading resp.Body.
        // After that call, clients can inspect resp.Trailer to find key/value
        // pairs included in the response trailer.
        public static (ptr<Response>, error) ReadResponse(ptr<bufio.Reader> _addr_r, ptr<Request> _addr_req)
        {
            ptr<Response> _p0 = default!;
            error _p0 = default!;
            ref bufio.Reader r = ref _addr_r.val;
            ref Request req = ref _addr_req.val;

            var tp = textproto.NewReader(r);
            ptr<Response> resp = addr(new Response(Request:req,)); 

            // Parse the first line of the response.
            var (line, err) = tp.ReadLine();
            if (err != null)
            {
                if (err == io.EOF)
                {
                    err = io.ErrUnexpectedEOF;
                }

                return (_addr_null!, error.As(err)!);

            }

            {
                var i__prev1 = i;

                var i = strings.IndexByte(line, ' ');

                if (i == -1L)
                {
                    return (_addr_null!, error.As(badStringError("malformed HTTP response", line))!);
                }
                else
                {
                    resp.Proto = line[..i];
                    resp.Status = strings.TrimLeft(line[i + 1L..], " ");
                }

                i = i__prev1;

            }

            var statusCode = resp.Status;
            {
                var i__prev1 = i;

                i = strings.IndexByte(resp.Status, ' ');

                if (i != -1L)
                {
                    statusCode = resp.Status[..i];
                }

                i = i__prev1;

            }

            if (len(statusCode) != 3L)
            {
                return (_addr_null!, error.As(badStringError("malformed HTTP status code", statusCode))!);
            }

            resp.StatusCode, err = strconv.Atoi(statusCode);
            if (err != null || resp.StatusCode < 0L)
            {
                return (_addr_null!, error.As(badStringError("malformed HTTP status code", statusCode))!);
            }

            bool ok = default;
            resp.ProtoMajor, resp.ProtoMinor, ok = ParseHTTPVersion(resp.Proto);

            if (!ok)
            {
                return (_addr_null!, error.As(badStringError("malformed HTTP version", resp.Proto))!);
            } 

            // Parse the response headers.
            var (mimeHeader, err) = tp.ReadMIMEHeader();
            if (err != null)
            {
                if (err == io.EOF)
                {
                    err = io.ErrUnexpectedEOF;
                }

                return (_addr_null!, error.As(err)!);

            }

            resp.Header = Header(mimeHeader);

            fixPragmaCacheControl(resp.Header);

            err = readTransfer(resp, r);
            if (err != null)
            {
                return (_addr_null!, error.As(err)!);
            }

            return (_addr_resp!, error.As(null!)!);

        }

        // RFC 7234, section 5.4: Should treat
        //    Pragma: no-cache
        // like
        //    Cache-Control: no-cache
        private static void fixPragmaCacheControl(Header header)
        {
            {
                var (hp, ok) = header["Pragma"];

                if (ok && len(hp) > 0L && hp[0L] == "no-cache")
                {
                    {
                        var (_, presentcc) = header["Cache-Control"];

                        if (!presentcc)
                        {
                            header["Cache-Control"] = new slice<@string>(new @string[] { "no-cache" });
                        }

                    }

                }

            }

        }

        // ProtoAtLeast reports whether the HTTP protocol used
        // in the response is at least major.minor.
        private static bool ProtoAtLeast(this ptr<Response> _addr_r, long major, long minor)
        {
            ref Response r = ref _addr_r.val;

            return r.ProtoMajor > major || r.ProtoMajor == major && r.ProtoMinor >= minor;
        }

        // Write writes r to w in the HTTP/1.x server response format,
        // including the status line, headers, body, and optional trailer.
        //
        // This method consults the following fields of the response r:
        //
        //  StatusCode
        //  ProtoMajor
        //  ProtoMinor
        //  Request.Method
        //  TransferEncoding
        //  Trailer
        //  Body
        //  ContentLength
        //  Header, values for non-canonical keys will have unpredictable behavior
        //
        // The Response Body is closed after it is sent.
        private static error Write(this ptr<Response> _addr_r, io.Writer w)
        {
            ref Response r = ref _addr_r.val;
 
            // Status line
            var text = r.Status;
            if (text == "")
            {
                bool ok = default;
                text, ok = statusText[r.StatusCode];
                if (!ok)
                {
                    text = "status code " + strconv.Itoa(r.StatusCode);
                }

            }
            else
            { 
                // Just to reduce stutter, if user set r.Status to "200 OK" and StatusCode to 200.
                // Not important.
                text = strings.TrimPrefix(text, strconv.Itoa(r.StatusCode) + " ");

            }

            {
                var (_, err) = fmt.Fprintf(w, "HTTP/%d.%d %03d %s\r\n", r.ProtoMajor, r.ProtoMinor, r.StatusCode, text);

                if (err != null)
                {
                    return error.As(err)!;
                } 

                // Clone it, so we can modify r1 as needed.

            } 

            // Clone it, so we can modify r1 as needed.
            ptr<Response> r1 = @new<Response>();
            r1.val = r.val;
            if (r1.ContentLength == 0L && r1.Body != null)
            { 
                // Is it actually 0 length? Or just unknown?
                array<byte> buf = new array<byte>(1L);
                var (n, err) = r1.Body.Read(buf[..]);
                if (err != null && err != io.EOF)
                {
                    return error.As(err)!;
                }

                if (n == 0L)
                { 
                    // Reset it to a known zero reader, in case underlying one
                    // is unhappy being read repeatedly.
                    r1.Body = NoBody;

                }
                else
                {
                    r1.ContentLength = -1L;
                    r1.Body = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{io.Readerio.Closer}{io.MultiReader(bytes.NewReader(buf[:1]),r.Body),r.Body,};
                }

            } 
            // If we're sending a non-chunked HTTP/1.1 response without a
            // content-length, the only way to do that is the old HTTP/1.0
            // way, by noting the EOF with a connection close, so we need
            // to set Close.
            if (r1.ContentLength == -1L && !r1.Close && r1.ProtoAtLeast(1L, 1L) && !chunked(r1.TransferEncoding) && !r1.Uncompressed)
            {
                r1.Close = true;
            } 

            // Process Body,ContentLength,Close,Trailer
            var (tw, err) = newTransferWriter(r1);
            if (err != null)
            {
                return error.As(err)!;
            }

            err = tw.writeHeader(w, null);
            if (err != null)
            {
                return error.As(err)!;
            } 

            // Rest of header
            err = r.Header.WriteSubset(w, respExcludeHeader);
            if (err != null)
            {
                return error.As(err)!;
            } 

            // contentLengthAlreadySent may have been already sent for
            // POST/PUT requests, even if zero length. See Issue 8180.
            var contentLengthAlreadySent = tw.shouldSendContentLength();
            if (r1.ContentLength == 0L && !chunked(r1.TransferEncoding) && !contentLengthAlreadySent && bodyAllowedForStatus(r.StatusCode))
            {
                {
                    (_, err) = io.WriteString(w, "Content-Length: 0\r\n");

                    if (err != null)
                    {
                        return error.As(err)!;
                    }

                }

            } 

            // End-of-header
            {
                (_, err) = io.WriteString(w, "\r\n");

                if (err != null)
                {
                    return error.As(err)!;
                } 

                // Write body and trailer

            } 

            // Write body and trailer
            err = tw.writeBody(w);
            if (err != null)
            {
                return error.As(err)!;
            } 

            // Success
            return error.As(null!)!;

        }

        private static void closeBody(this ptr<Response> _addr_r)
        {
            ref Response r = ref _addr_r.val;

            if (r.Body != null)
            {
                r.Body.Close();
            }

        }

        // bodyIsWritable reports whether the Body supports writing. The
        // Transport returns Writable bodies for 101 Switching Protocols
        // responses.
        // The Transport uses this method to determine whether a persistent
        // connection is done being managed from its perspective. Once we
        // return a writable response body to a user, the net/http package is
        // done managing that connection.
        private static bool bodyIsWritable(this ptr<Response> _addr_r)
        {
            ref Response r = ref _addr_r.val;

            io.Writer (_, ok) = r.Body._<io.Writer>();
            return ok;
        }

        // isProtocolSwitch reports whether r is a response to a successful
        // protocol upgrade.
        private static bool isProtocolSwitch(this ptr<Response> _addr_r)
        {
            ref Response r = ref _addr_r.val;

            return r.StatusCode == StatusSwitchingProtocols && r.Header.Get("Upgrade") != "" && httpguts.HeaderValuesContainsToken(r.Header["Connection"], "Upgrade");
        }
    }
}}
