// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// HTTP server. See RFC 7230 through 7235.

// package http -- go2cs converted at 2020 October 09 04:58:01 UTC
// import "net/http" ==> using http = go.net.http_package
// Original source: C:\Go\src\net\http\server.go
using bufio = go.bufio_package;
using bytes = go.bytes_package;
using context = go.context_package;
using tls = go.crypto.tls_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using io = go.io_package;
using ioutil = go.io.ioutil_package;
using log = go.log_package;
using net = go.net_package;
using textproto = go.net.textproto_package;
using url = go.net.url_package;
using url = go.net.url_package;
using os = go.os_package;
using path = go.path_package;
using runtime = go.runtime_package;
using sort = go.sort_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using sync = go.sync_package;
using atomic = go.sync.atomic_package;
using time = go.time_package;

using httpguts = go.golang.org.x.net.http.httpguts_package;
using static go.builtin;
using System.Threading;
using System;

namespace go {
namespace net
{
    public static partial class http_package
    {
        // Errors used by the HTTP server.
 
        // ErrBodyNotAllowed is returned by ResponseWriter.Write calls
        // when the HTTP method or response code does not permit a
        // body.
        public static var ErrBodyNotAllowed = errors.New("http: request method or response status code does not allow body");        public static var ErrHijacked = errors.New("http: connection has been hijacked");        public static var ErrContentLength = errors.New("http: wrote more than the declared Content-Length");        public static var ErrWriteAfterFlush = errors.New("unused");

        // A Handler responds to an HTTP request.
        //
        // ServeHTTP should write reply headers and data to the ResponseWriter
        // and then return. Returning signals that the request is finished; it
        // is not valid to use the ResponseWriter or read from the
        // Request.Body after or concurrently with the completion of the
        // ServeHTTP call.
        //
        // Depending on the HTTP client software, HTTP protocol version, and
        // any intermediaries between the client and the Go server, it may not
        // be possible to read from the Request.Body after writing to the
        // ResponseWriter. Cautious handlers should read the Request.Body
        // first, and then reply.
        //
        // Except for reading the body, handlers should not modify the
        // provided Request.
        //
        // If ServeHTTP panics, the server (the caller of ServeHTTP) assumes
        // that the effect of the panic was isolated to the active request.
        // It recovers the panic, logs a stack trace to the server error log,
        // and either closes the network connection or sends an HTTP/2
        // RST_STREAM, depending on the HTTP protocol. To abort a handler so
        // the client sees an interrupted response but the server doesn't log
        // an error, panic with the value ErrAbortHandler.
        public partial interface Handler
        {
            void ServeHTTP(ResponseWriter _p0, ptr<Request> _p0);
        }

        // A ResponseWriter interface is used by an HTTP handler to
        // construct an HTTP response.
        //
        // A ResponseWriter may not be used after the Handler.ServeHTTP method
        // has returned.
        public partial interface ResponseWriter
        {
            (long, error) Header(); // Write writes the data to the connection as part of an HTTP reply.
//
// If WriteHeader has not yet been called, Write calls
// WriteHeader(http.StatusOK) before writing the data. If the Header
// does not contain a Content-Type line, Write adds a Content-Type set
// to the result of passing the initial 512 bytes of written data to
// DetectContentType. Additionally, if the total size of all written
// data is under a few KB and there are no Flush calls, the
// Content-Length header is added automatically.
//
// Depending on the HTTP protocol version and the client, calling
// Write or WriteHeader may prevent future reads on the
// Request.Body. For HTTP/1.x requests, handlers should read any
// needed request body data before writing the response. Once the
// headers have been flushed (due to either an explicit Flusher.Flush
// call or writing enough data to trigger a flush), the request body
// may be unavailable. For HTTP/2 requests, the Go HTTP server permits
// handlers to continue to read the request body while concurrently
// writing the response. However, such behavior may not be supported
// by all HTTP/2 clients. Handlers should read before writing if
// possible to maximize compatibility.
            (long, error) Write(slice<byte> _p0); // WriteHeader sends an HTTP response header with the provided
// status code.
//
// If WriteHeader is not called explicitly, the first call to Write
// will trigger an implicit WriteHeader(http.StatusOK).
// Thus explicit calls to WriteHeader are mainly used to
// send error codes.
//
// The provided code must be a valid HTTP 1xx-5xx status code.
// Only one header may be written. Go does not currently
// support sending user-defined 1xx informational headers,
// with the exception of 100-continue response header that the
// Server sends automatically when the Request.Body is read.
            (long, error) WriteHeader(long statusCode);
        }

        // The Flusher interface is implemented by ResponseWriters that allow
        // an HTTP handler to flush buffered data to the client.
        //
        // The default HTTP/1.x and HTTP/2 ResponseWriter implementations
        // support Flusher, but ResponseWriter wrappers may not. Handlers
        // should always test for this ability at runtime.
        //
        // Note that even for ResponseWriters that support Flush,
        // if the client is connected through an HTTP proxy,
        // the buffered data may not reach the client until the response
        // completes.
        public partial interface Flusher
        {
            void Flush();
        }

        // The Hijacker interface is implemented by ResponseWriters that allow
        // an HTTP handler to take over the connection.
        //
        // The default ResponseWriter for HTTP/1.x connections supports
        // Hijacker, but HTTP/2 connections intentionally do not.
        // ResponseWriter wrappers may also not support Hijacker. Handlers
        // should always test for this ability at runtime.
        public partial interface Hijacker
        {
            (net.Conn, ptr<bufio.ReadWriter>, error) Hijack();
        }

        // The CloseNotifier interface is implemented by ResponseWriters which
        // allow detecting when the underlying connection has gone away.
        //
        // This mechanism can be used to cancel long operations on the server
        // if the client has disconnected before the response is ready.
        //
        // Deprecated: the CloseNotifier interface predates Go's context package.
        // New code should use Request.Context instead.
        public partial interface CloseNotifier
        {
            channel<bool> CloseNotify();
        }

 
        // ServerContextKey is a context key. It can be used in HTTP
        // handlers with Context.Value to access the server that
        // started the handler. The associated value will be of
        // type *Server.
        public static ptr<contextKey> ServerContextKey = addr(new contextKey("http-server"));        public static ptr<contextKey> LocalAddrContextKey = addr(new contextKey("local-addr"));

        // A conn represents the server side of an HTTP connection.
        private partial struct conn
        {
            public ptr<Server> server; // cancelCtx cancels the connection-level context.
            public context.CancelFunc cancelCtx; // rwc is the underlying network connection.
// This is never wrapped by other types and is the value given out
// to CloseNotifier callers. It is usually of type *net.TCPConn or
// *tls.Conn.
            public net.Conn rwc; // remoteAddr is rwc.RemoteAddr().String(). It is not populated synchronously
// inside the Listener's Accept goroutine, as some implementations block.
// It is populated immediately inside the (*conn).serve goroutine.
// This is the value of a Handler's (*Request).RemoteAddr.
            public @string remoteAddr; // tlsState is the TLS connection state when using TLS.
// nil means not TLS.
            public ptr<tls.ConnectionState> tlsState; // werr is set to the first write error to rwc.
// It is set via checkConnErrorWriter{w}, where bufw writes.
            public error werr; // r is bufr's read source. It's a wrapper around rwc that provides
// io.LimitedReader-style limiting (while reading request headers)
// and functionality to support CloseNotifier. See *connReader docs.
            public ptr<connReader> r; // bufr reads from r.
            public ptr<bufio.Reader> bufr; // bufw writes to checkConnErrorWriter{c}, which populates werr on error.
            public ptr<bufio.Writer> bufw; // lastMethod is the method of the most recent request
// on this connection, if any.
            public @string lastMethod;
            public atomic.Value curReq; // of *response (which has a Request in it)

            public sync.Mutex mu; // hijackedv is whether this connection has been hijacked
// by a Handler with the Hijacker interface.
// It is guarded by mu.
            public bool hijackedv;
        }

        private static bool hijacked(this ptr<conn> _addr_c) => func((defer, _, __) =>
        {
            ref conn c = ref _addr_c.val;

            c.mu.Lock();
            defer(c.mu.Unlock());
            return c.hijackedv;
        });

        // c.mu must be held.
        private static (net.Conn, ptr<bufio.ReadWriter>, error) hijackLocked(this ptr<conn> _addr_c)
        {
            net.Conn rwc = default;
            ptr<bufio.ReadWriter> buf = default!;
            error err = default!;
            ref conn c = ref _addr_c.val;

            if (c.hijackedv)
            {
                return (null, _addr_null!, error.As(ErrHijacked)!);
            }

            c.r.abortPendingRead();

            c.hijackedv = true;
            rwc = c.rwc;
            rwc.SetDeadline(new time.Time());

            buf = bufio.NewReadWriter(c.bufr, bufio.NewWriter(rwc));
            if (c.r.hasByte)
            {
                {
                    var (_, err) = c.bufr.Peek(c.bufr.Buffered() + 1L);

                    if (err != null)
                    {
                        return (null, _addr_null!, error.As(fmt.Errorf("unexpected Peek failure reading buffered byte: %v", err))!);
                    }

                }

            }

            c.setState(rwc, StateHijacked);
            return ;

        }

        // This should be >= 512 bytes for DetectContentType,
        // but otherwise it's somewhat arbitrary.
        private static readonly long bufferBeforeChunkingSize = (long)2048L;

        // chunkWriter writes to a response's conn buffer, and is the writer
        // wrapped by the response.bufw buffered writer.
        //
        // chunkWriter also is responsible for finalizing the Header, including
        // conditionally setting the Content-Type and setting a Content-Length
        // in cases where the handler's final output is smaller than the buffer
        // size. It also conditionally adds chunk headers, when in chunking mode.
        //
        // See the comment above (*response).Write for the entire write flow.


        // chunkWriter writes to a response's conn buffer, and is the writer
        // wrapped by the response.bufw buffered writer.
        //
        // chunkWriter also is responsible for finalizing the Header, including
        // conditionally setting the Content-Type and setting a Content-Length
        // in cases where the handler's final output is smaller than the buffer
        // size. It also conditionally adds chunk headers, when in chunking mode.
        //
        // See the comment above (*response).Write for the entire write flow.
        private partial struct chunkWriter
        {
            public ptr<response> res; // header is either nil or a deep clone of res.handlerHeader
// at the time of res.writeHeader, if res.writeHeader is
// called and extra buffering is being done to calculate
// Content-Type and/or Content-Length.
            public Header header; // wroteHeader tells whether the header's been written to "the
// wire" (or rather: w.conn.buf). this is unlike
// (*response).wroteHeader, which tells only whether it was
// logically written.
            public bool wroteHeader; // set by the writeHeader method:
            public bool chunking; // using chunked transfer encoding for reply body
        }

        private static slice<byte> crlf = (slice<byte>)"\r\n";        private static slice<byte> colonSpace = (slice<byte>)": ";

        private static (long, error) Write(this ptr<chunkWriter> _addr_cw, slice<byte> p)
        {
            long n = default;
            error err = default!;
            ref chunkWriter cw = ref _addr_cw.val;

            if (!cw.wroteHeader)
            {
                cw.writeHeader(p);
            }

            if (cw.res.req.Method == "HEAD")
            { 
                // Eat writes.
                return (len(p), error.As(null!)!);

            }

            if (cw.chunking)
            {
                _, err = fmt.Fprintf(cw.res.conn.bufw, "%x\r\n", len(p));
                if (err != null)
                {
                    cw.res.conn.rwc.Close();
                    return ;
                }

            }

            n, err = cw.res.conn.bufw.Write(p);
            if (cw.chunking && err == null)
            {
                _, err = cw.res.conn.bufw.Write(crlf);
            }

            if (err != null)
            {
                cw.res.conn.rwc.Close();
            }

            return ;

        }

        private static void flush(this ptr<chunkWriter> _addr_cw)
        {
            ref chunkWriter cw = ref _addr_cw.val;

            if (!cw.wroteHeader)
            {
                cw.writeHeader(null);
            }

            cw.res.conn.bufw.Flush();

        }

        private static void close(this ptr<chunkWriter> _addr_cw)
        {
            ref chunkWriter cw = ref _addr_cw.val;

            if (!cw.wroteHeader)
            {
                cw.writeHeader(null);
            }

            if (cw.chunking)
            {
                var bw = cw.res.conn.bufw; // conn's bufio writer
                // zero chunk to mark EOF
                bw.WriteString("0\r\n");
                {
                    var trailers = cw.res.finalTrailers();

                    if (trailers != null)
                    {
                        trailers.Write(bw); // the writer handles noting errors
                    } 
                    // final blank line after the trailers (whether
                    // present or not)

                } 
                // final blank line after the trailers (whether
                // present or not)
                bw.WriteString("\r\n");

            }

        }

        // A response represents the server side of an HTTP response.
        private partial struct response
        {
            public ptr<conn> conn;
            public ptr<Request> req; // request for this response
            public io.ReadCloser reqBody;
            public context.CancelFunc cancelCtx; // when ServeHTTP exits
            public bool wroteHeader; // reply header has been (logically) written
            public bool wroteContinue; // 100 Continue response was written
            public bool wants10KeepAlive; // HTTP/1.0 w/ Connection "keep-alive"
            public bool wantsClose; // HTTP request has Connection "close"

// canWriteContinue is a boolean value accessed as an atomic int32
// that says whether or not a 100 Continue header can be written
// to the connection.
// writeContinueMu must be held while writing the header.
// These two fields together synchronize the body reader
// (the expectContinueReader, which wants to write 100 Continue)
// against the main writer.
            public atomicBool canWriteContinue;
            public sync.Mutex writeContinueMu;
            public ptr<bufio.Writer> w; // buffers output in chunks to chunkWriter
            public chunkWriter cw; // handlerHeader is the Header that Handlers get access to,
// which may be retained and mutated even after WriteHeader.
// handlerHeader is copied into cw.header at WriteHeader
// time, and privately mutated thereafter.
            public Header handlerHeader;
            public bool calledHeader; // handler accessed handlerHeader via Header

            public long written; // number of bytes written in body
            public long contentLength; // explicitly-declared Content-Length; or -1
            public long status; // status code passed to WriteHeader

// close connection after this reply.  set on request and
// updated after response from handler if there's a
// "Connection: keep-alive" response header and a
// Content-Length.
            public bool closeAfterReply; // requestBodyLimitHit is set by requestTooLarge when
// maxBytesReader hits its max size. It is checked in
// WriteHeader, to make sure we don't consume the
// remaining request body to try to advance to the next HTTP
// request. Instead, when this is set, we stop reading
// subsequent requests on this connection and stop reading
// input from it.
            public bool requestBodyLimitHit; // trailers are the headers to be sent after the handler
// finishes writing the body. This field is initialized from
// the Trailer response header when the response header is
// written.
            public slice<@string> trailers;
            public atomicBool handlerDone; // set true when the handler exits

// Buffers for Date, Content-Length, and status code
            public array<byte> dateBuf;
            public array<byte> clenBuf;
            public array<byte> statusBuf; // closeNotifyCh is the channel returned by CloseNotify.
// TODO(bradfitz): this is currently (for Go 1.8) always
// non-nil. Make this lazily-created again as it used to be?
            public channel<bool> closeNotifyCh;
            public int didCloseNotify; // atomic (only 0->1 winner should send)
        }

        // TrailerPrefix is a magic prefix for ResponseWriter.Header map keys
        // that, if present, signals that the map entry is actually for
        // the response trailers, and not the response headers. The prefix
        // is stripped after the ServeHTTP call finishes and the values are
        // sent in the trailers.
        //
        // This mechanism is intended only for trailers that are not known
        // prior to the headers being written. If the set of trailers is fixed
        // or known before the header is written, the normal Go trailers mechanism
        // is preferred:
        //    https://golang.org/pkg/net/http/#ResponseWriter
        //    https://golang.org/pkg/net/http/#example_ResponseWriter_trailers
        public static readonly @string TrailerPrefix = (@string)"Trailer:";

        // finalTrailers is called after the Handler exits and returns a non-nil
        // value if the Handler set any trailers.


        // finalTrailers is called after the Handler exits and returns a non-nil
        // value if the Handler set any trailers.
        private static Header finalTrailers(this ptr<response> _addr_w)
        {
            ref response w = ref _addr_w.val;

            Header t = default;
            {
                var k__prev1 = k;

                foreach (var (__k, __vv) in w.handlerHeader)
                {
                    k = __k;
                    vv = __vv;
                    if (strings.HasPrefix(k, TrailerPrefix))
                    {
                        if (t == null)
                        {
                            t = make(Header);
                        }

                        t[strings.TrimPrefix(k, TrailerPrefix)] = vv;

                    }

                }

                k = k__prev1;
            }

            {
                var k__prev1 = k;

                foreach (var (_, __k) in w.trailers)
                {
                    k = __k;
                    if (t == null)
                    {
                        t = make(Header);
                    }

                    foreach (var (_, v) in w.handlerHeader[k])
                    {
                        t.Add(k, v);
                    }

                }

                k = k__prev1;
            }

            return t;

        }

        private partial struct atomicBool // : int
        {
        }

        private static bool isSet(this ptr<atomicBool> _addr_b)
        {
            ref atomicBool b = ref _addr_b.val;

            return atomic.LoadInt32((int32.val)(b)) != 0L;
        }
        private static void setTrue(this ptr<atomicBool> _addr_b)
        {
            ref atomicBool b = ref _addr_b.val;

            atomic.StoreInt32((int32.val)(b), 1L);
        }
        private static void setFalse(this ptr<atomicBool> _addr_b)
        {
            ref atomicBool b = ref _addr_b.val;

            atomic.StoreInt32((int32.val)(b), 0L);
        }

        // declareTrailer is called for each Trailer header when the
        // response header is written. It notes that a header will need to be
        // written in the trailers at the end of the response.
        private static void declareTrailer(this ptr<response> _addr_w, @string k)
        {
            ref response w = ref _addr_w.val;

            k = CanonicalHeaderKey(k);
            if (!httpguts.ValidTrailerHeader(k))
            { 
                // Forbidden by RFC 7230, section 4.1.2
                return ;

            }

            w.trailers = append(w.trailers, k);

        }

        // requestTooLarge is called by maxBytesReader when too much input has
        // been read from the client.
        private static void requestTooLarge(this ptr<response> _addr_w)
        {
            ref response w = ref _addr_w.val;

            w.closeAfterReply = true;
            w.requestBodyLimitHit = true;
            if (!w.wroteHeader)
            {
                w.Header().Set("Connection", "close");
            }

        }

        // needsSniff reports whether a Content-Type still needs to be sniffed.
        private static bool needsSniff(this ptr<response> _addr_w)
        {
            ref response w = ref _addr_w.val;

            var (_, haveType) = w.handlerHeader["Content-Type"];
            return !w.cw.wroteHeader && !haveType && w.written < sniffLen;
        }

        // writerOnly hides an io.Writer value's optional ReadFrom method
        // from io.Copy.
        private partial struct writerOnly : io.Writer
        {
            public ref io.Writer Writer => ref Writer_val;
        }

        private static (bool, error) srcIsRegularFile(io.Reader src)
        {
            bool isRegular = default;
            error err = default!;

            switch (src.type())
            {
                case ptr<os.File> v:
                    var (fi, err) = v.Stat();
                    if (err != null)
                    {
                        return (false, error.As(err)!);
                    }

                    return (fi.Mode().IsRegular(), error.As(null!)!);
                    break;
                case ptr<io.LimitedReader> v:
                    return srcIsRegularFile(v.R);
                    break;
                default:
                {
                    var v = src.type();
                    return ;
                    break;
                }
            }

        }

        // ReadFrom is here to optimize copying from an *os.File regular file
        // to a *net.TCPConn with sendfile.
        private static (long, error) ReadFrom(this ptr<response> _addr_w, io.Reader src) => func((defer, _, __) =>
        {
            long n = default;
            error err = default!;
            ref response w = ref _addr_w.val;
 
            // Our underlying w.conn.rwc is usually a *TCPConn (with its
            // own ReadFrom method). If not, or if our src isn't a regular
            // file, just fall back to the normal copy method.
            io.ReaderFrom (rf, ok) = w.conn.rwc._<io.ReaderFrom>();
            var (regFile, err) = srcIsRegularFile(src);
            if (err != null)
            {
                return (0L, error.As(err)!);
            }

            if (!ok || !regFile)
            {
                ptr<slice<byte>> bufp = copyBufPool.Get()._<ptr<slice<byte>>>();
                defer(copyBufPool.Put(bufp));
                return io.CopyBuffer(new writerOnly(w), src, bufp.val);
            } 

            // sendfile path:
            if (!w.wroteHeader)
            {
                w.WriteHeader(StatusOK);
            }

            if (w.needsSniff())
            {
                var (n0, err) = io.Copy(new writerOnly(w), io.LimitReader(src, sniffLen));
                n += n0;
                if (err != null)
                {
                    return (n, error.As(err)!);
                }

            }

            w.w.Flush(); // get rid of any previous writes
            w.cw.flush(); // make sure Header is written; flush data to rwc

            // Now that cw has been flushed, its chunking field is guaranteed initialized.
            if (!w.cw.chunking && w.bodyAllowed())
            {
                (n0, err) = rf.ReadFrom(src);
                n += n0;
                w.written += n0;
                return (n, error.As(err)!);
            }

            (n0, err) = io.Copy(new writerOnly(w), src);
            n += n0;
            return (n, error.As(err)!);

        });

        // debugServerConnections controls whether all server connections are wrapped
        // with a verbose logging wrapper.
        private static readonly var debugServerConnections = false;

        // Create new connection from rwc.


        // Create new connection from rwc.
        private static ptr<conn> newConn(this ptr<Server> _addr_srv, net.Conn rwc)
        {
            ref Server srv = ref _addr_srv.val;

            ptr<conn> c = addr(new conn(server:srv,rwc:rwc,));
            if (debugServerConnections)
            {
                c.rwc = newLoggingConn("server", c.rwc);
            }

            return _addr_c!;

        }

        private partial struct readResult
        {
            public incomparable _;
            public long n;
            public error err;
            public byte b; // byte read, if n == 1
        }

        // connReader is the io.Reader wrapper used by *conn. It combines a
        // selectively-activated io.LimitedReader (to bound request header
        // read sizes) with support for selectively keeping an io.Reader.Read
        // call blocked in a background goroutine to wait for activity and
        // trigger a CloseNotifier channel.
        private partial struct connReader
        {
            public ptr<conn> conn;
            public sync.Mutex mu; // guards following
            public bool hasByte;
            public array<byte> byteBuf;
            public ptr<sync.Cond> cond;
            public bool inRead;
            public bool aborted; // set true before conn.rwc deadline is set to past
            public long remain; // bytes remaining
        }

        private static void @lock(this ptr<connReader> _addr_cr)
        {
            ref connReader cr = ref _addr_cr.val;

            cr.mu.Lock();
            if (cr.cond == null)
            {
                cr.cond = sync.NewCond(_addr_cr.mu);
            }

        }

        private static void unlock(this ptr<connReader> _addr_cr)
        {
            ref connReader cr = ref _addr_cr.val;

            cr.mu.Unlock();
        }

        private static void startBackgroundRead(this ptr<connReader> _addr_cr) => func((defer, panic, _) =>
        {
            ref connReader cr = ref _addr_cr.val;

            cr.@lock();
            defer(cr.unlock());
            if (cr.inRead)
            {
                panic("invalid concurrent Body.Read call");
            }

            if (cr.hasByte)
            {
                return ;
            }

            cr.inRead = true;
            cr.conn.rwc.SetReadDeadline(new time.Time());
            go_(() => cr.backgroundRead());

        });

        private static void backgroundRead(this ptr<connReader> _addr_cr)
        {
            ref connReader cr = ref _addr_cr.val;

            var (n, err) = cr.conn.rwc.Read(cr.byteBuf[..]);
            cr.@lock();
            if (n == 1L)
            {
                cr.hasByte = true; 
                // We were past the end of the previous request's body already
                // (since we wouldn't be in a background read otherwise), so
                // this is a pipelined HTTP request. Prior to Go 1.11 we used to
                // send on the CloseNotify channel and cancel the context here,
                // but the behavior was documented as only "may", and we only
                // did that because that's how CloseNotify accidentally behaved
                // in very early Go releases prior to context support. Once we
                // added context support, people used a Handler's
                // Request.Context() and passed it along. Having that context
                // cancel on pipelined HTTP requests caused problems.
                // Fortunately, almost nothing uses HTTP/1.x pipelining.
                // Unfortunately, apt-get does, or sometimes does.
                // New Go 1.11 behavior: don't fire CloseNotify or cancel
                // contexts on pipelined requests. Shouldn't affect people, but
                // fixes cases like Issue 23921. This does mean that a client
                // closing their TCP connection after sending a pipelined
                // request won't cancel the context, but we'll catch that on any
                // write failure (in checkConnErrorWriter.Write).
                // If the server never writes, yes, there are still contrived
                // server & client behaviors where this fails to ever cancel the
                // context, but that's kinda why HTTP/1.x pipelining died
                // anyway.
            }

            {
                net.Error (ne, ok) = err._<net.Error>();

                if (ok && cr.aborted && ne.Timeout())
                { 
                    // Ignore this error. It's the expected error from
                    // another goroutine calling abortPendingRead.
                }
                else if (err != null)
                {
                    cr.handleReadError(err);
                }


            }

            cr.aborted = false;
            cr.inRead = false;
            cr.unlock();
            cr.cond.Broadcast();

        }

        private static void abortPendingRead(this ptr<connReader> _addr_cr) => func((defer, _, __) =>
        {
            ref connReader cr = ref _addr_cr.val;

            cr.@lock();
            defer(cr.unlock());
            if (!cr.inRead)
            {
                return ;
            }

            cr.aborted = true;
            cr.conn.rwc.SetReadDeadline(aLongTimeAgo);
            while (cr.inRead)
            {
                cr.cond.Wait();
            }

            cr.conn.rwc.SetReadDeadline(new time.Time());

        });

        private static void setReadLimit(this ptr<connReader> _addr_cr, long remain)
        {
            ref connReader cr = ref _addr_cr.val;

            cr.remain = remain;
        }
        private static void setInfiniteReadLimit(this ptr<connReader> _addr_cr)
        {
            ref connReader cr = ref _addr_cr.val;

            cr.remain = maxInt64;
        }
        private static bool hitReadLimit(this ptr<connReader> _addr_cr)
        {
            ref connReader cr = ref _addr_cr.val;

            return cr.remain <= 0L;
        }

        // handleReadError is called whenever a Read from the client returns a
        // non-nil error.
        //
        // The provided non-nil err is almost always io.EOF or a "use of
        // closed network connection". In any case, the error is not
        // particularly interesting, except perhaps for debugging during
        // development. Any error means the connection is dead and we should
        // down its context.
        //
        // It may be called from multiple goroutines.
        private static void handleReadError(this ptr<connReader> _addr_cr, error _)
        {
            ref connReader cr = ref _addr_cr.val;

            cr.conn.cancelCtx();
            cr.closeNotify();
        }

        // may be called from multiple goroutines.
        private static void closeNotify(this ptr<connReader> _addr_cr)
        {
            ref connReader cr = ref _addr_cr.val;

            ptr<response> (res, _) = cr.conn.curReq.Load()._<ptr<response>>();
            if (res != null && atomic.CompareAndSwapInt32(_addr_res.didCloseNotify, 0L, 1L))
            {
                res.closeNotifyCh.Send(true);
            }

        }

        private static (long, error) Read(this ptr<connReader> _addr_cr, slice<byte> p) => func((_, panic, __) =>
        {
            long n = default;
            error err = default!;
            ref connReader cr = ref _addr_cr.val;

            cr.@lock();
            if (cr.inRead)
            {
                cr.unlock();
                if (cr.conn.hijacked())
                {
                    panic("invalid Body.Read call. After hijacked, the original Request must not be used");
                }

                panic("invalid concurrent Body.Read call");

            }

            if (cr.hitReadLimit())
            {
                cr.unlock();
                return (0L, error.As(io.EOF)!);
            }

            if (len(p) == 0L)
            {
                cr.unlock();
                return (0L, error.As(null!)!);
            }

            if (int64(len(p)) > cr.remain)
            {
                p = p[..cr.remain];
            }

            if (cr.hasByte)
            {
                p[0L] = cr.byteBuf[0L];
                cr.hasByte = false;
                cr.unlock();
                return (1L, error.As(null!)!);
            }

            cr.inRead = true;
            cr.unlock();
            n, err = cr.conn.rwc.Read(p);

            cr.@lock();
            cr.inRead = false;
            if (err != null)
            {
                cr.handleReadError(err);
            }

            cr.remain -= int64(n);
            cr.unlock();

            cr.cond.Broadcast();
            return (n, error.As(err)!);

        });

        private static sync.Pool bufioReaderPool = default;        private static sync.Pool bufioWriter2kPool = default;        private static sync.Pool bufioWriter4kPool = default;

        private static sync.Pool copyBufPool = new sync.Pool(New:func()interface{}{b:=make([]byte,32*1024)return&b},);

        private static ptr<sync.Pool> bufioWriterPool(long size)
        {
            switch (size)
            {
                case 2L << (int)(10L): 
                    return _addr__addr_bufioWriter2kPool!;
                    break;
                case 4L << (int)(10L): 
                    return _addr__addr_bufioWriter4kPool!;
                    break;
            }
            return _addr_null!;

        }

        private static ptr<bufio.Reader> newBufioReader(io.Reader r)
        {
            {
                var v = bufioReaderPool.Get();

                if (v != null)
                {
                    ptr<bufio.Reader> br = v._<ptr<bufio.Reader>>();
                    br.Reset(r);
                    return _addr_br!;
                } 
                // Note: if this reader size is ever changed, update
                // TestHandlerBodyClose's assumptions.

            } 
            // Note: if this reader size is ever changed, update
            // TestHandlerBodyClose's assumptions.
            return _addr_bufio.NewReader(r)!;

        }

        private static void putBufioReader(ptr<bufio.Reader> _addr_br)
        {
            ref bufio.Reader br = ref _addr_br.val;

            br.Reset(null);
            bufioReaderPool.Put(br);
        }

        private static ptr<bufio.Writer> newBufioWriterSize(io.Writer w, long size)
        {
            var pool = bufioWriterPool(size);
            if (pool != null)
            {
                {
                    var v = pool.Get();

                    if (v != null)
                    {
                        ptr<bufio.Writer> bw = v._<ptr<bufio.Writer>>();
                        bw.Reset(w);
                        return _addr_bw!;
                    }

                }

            }

            return _addr_bufio.NewWriterSize(w, size)!;

        }

        private static void putBufioWriter(ptr<bufio.Writer> _addr_bw)
        {
            ref bufio.Writer bw = ref _addr_bw.val;

            bw.Reset(null);
            {
                var pool = bufioWriterPool(bw.Available());

                if (pool != null)
                {
                    pool.Put(bw);
                }

            }

        }

        // DefaultMaxHeaderBytes is the maximum permitted size of the headers
        // in an HTTP request.
        // This can be overridden by setting Server.MaxHeaderBytes.
        public static readonly long DefaultMaxHeaderBytes = (long)1L << (int)(20L); // 1 MB

 // 1 MB

        private static long maxHeaderBytes(this ptr<Server> _addr_srv)
        {
            ref Server srv = ref _addr_srv.val;

            if (srv.MaxHeaderBytes > 0L)
            {
                return srv.MaxHeaderBytes;
            }

            return DefaultMaxHeaderBytes;

        }

        private static long initialReadLimitSize(this ptr<Server> _addr_srv)
        {
            ref Server srv = ref _addr_srv.val;

            return int64(srv.maxHeaderBytes()) + 4096L; // bufio slop
        }

        // wrapper around io.ReadCloser which on first read, sends an
        // HTTP/1.1 100 Continue header
        private partial struct expectContinueReader
        {
            public ptr<response> resp;
            public io.ReadCloser readCloser;
            public bool closed;
            public atomicBool sawEOF;
        }

        private static (long, error) Read(this ptr<expectContinueReader> _addr_ecr, slice<byte> p)
        {
            long n = default;
            error err = default!;
            ref expectContinueReader ecr = ref _addr_ecr.val;

            if (ecr.closed)
            {
                return (0L, error.As(ErrBodyReadAfterClose)!);
            }

            var w = ecr.resp;
            if (!w.wroteContinue && w.canWriteContinue.isSet() && !w.conn.hijacked())
            {
                w.wroteContinue = true;
                w.writeContinueMu.Lock();
                if (w.canWriteContinue.isSet())
                {
                    w.conn.bufw.WriteString("HTTP/1.1 100 Continue\r\n\r\n");
                    w.conn.bufw.Flush();
                    w.canWriteContinue.setFalse();
                }

                w.writeContinueMu.Unlock();

            }

            n, err = ecr.readCloser.Read(p);
            if (err == io.EOF)
            {
                ecr.sawEOF.setTrue();
            }

            return ;

        }

        private static error Close(this ptr<expectContinueReader> _addr_ecr)
        {
            ref expectContinueReader ecr = ref _addr_ecr.val;

            ecr.closed = true;
            return error.As(ecr.readCloser.Close())!;
        }

        // TimeFormat is the time format to use when generating times in HTTP
        // headers. It is like time.RFC1123 but hard-codes GMT as the time
        // zone. The time being formatted must be in UTC for Format to
        // generate the correct format.
        //
        // For parsing this time format, see ParseTime.
        public static readonly @string TimeFormat = (@string)"Mon, 02 Jan 2006 15:04:05 GMT";

        // appendTime is a non-allocating version of []byte(t.UTC().Format(TimeFormat))


        // appendTime is a non-allocating version of []byte(t.UTC().Format(TimeFormat))
        private static slice<byte> appendTime(slice<byte> b, time.Time t)
        {
            const @string days = (@string)"SunMonTueWedThuFriSat";

            const @string months = (@string)"JanFebMarAprMayJunJulAugSepOctNovDec";



            t = t.UTC();
            var (yy, mm, dd) = t.Date();
            var (hh, mn, ss) = t.Clock();
            var day = days[3L * t.Weekday()..];
            var mon = months[3L * (mm - 1L)..];

            return append(b, day[0L], day[1L], day[2L], ',', ' ', byte('0' + dd / 10L), byte('0' + dd % 10L), ' ', mon[0L], mon[1L], mon[2L], ' ', byte('0' + yy / 1000L), byte('0' + (yy / 100L) % 10L), byte('0' + (yy / 10L) % 10L), byte('0' + yy % 10L), ' ', byte('0' + hh / 10L), byte('0' + hh % 10L), ':', byte('0' + mn / 10L), byte('0' + mn % 10L), ':', byte('0' + ss / 10L), byte('0' + ss % 10L), ' ', 'G', 'M', 'T');
        }

        private static var errTooLarge = errors.New("http: request too large");

        // Read next request from connection.
        private static (ptr<response>, error) readRequest(this ptr<conn> _addr_c, context.Context ctx) => func((defer, _, __) =>
        {
            ptr<response> w = default!;
            error err = default!;
            ref conn c = ref _addr_c.val;

            if (c.hijacked())
            {
                return (_addr_null!, error.As(ErrHijacked)!);
            }

            time.Time wholeReqDeadline = default;            time.Time hdrDeadline = default;
            var t0 = time.Now();
            {
                var d__prev1 = d;

                var d = c.server.readHeaderTimeout();

                if (d != 0L)
                {
                    hdrDeadline = t0.Add(d);
                }

                d = d__prev1;

            }

            {
                var d__prev1 = d;

                d = c.server.ReadTimeout;

                if (d != 0L)
                {
                    wholeReqDeadline = t0.Add(d);
                }

                d = d__prev1;

            }

            c.rwc.SetReadDeadline(hdrDeadline);
            {
                var d__prev1 = d;

                d = c.server.WriteTimeout;

                if (d != 0L)
                {
                    defer(() =>
                    {
                        c.rwc.SetWriteDeadline(time.Now().Add(d));
                    }());

                }

                d = d__prev1;

            }


            c.r.setReadLimit(c.server.initialReadLimitSize());
            if (c.lastMethod == "POST")
            { 
                // RFC 7230 section 3 tolerance for old buggy clients.
                var (peek, _) = c.bufr.Peek(4L); // ReadRequest will get err below
                c.bufr.Discard(numLeadingCRorLF(peek));

            }

            var (req, err) = readRequest(c.bufr, keepHostHeader);
            if (err != null)
            {
                if (c.r.hitReadLimit())
                {
                    return (_addr_null!, error.As(errTooLarge)!);
                }

                return (_addr_null!, error.As(err)!);

            }

            if (!http1ServerSupportsRequest(_addr_req))
            {
                return (_addr_null!, error.As(badRequestError("unsupported protocol version"))!);
            }

            c.lastMethod = req.Method;
            c.r.setInfiniteReadLimit();

            var (hosts, haveHost) = req.Header["Host"];
            var isH2Upgrade = req.isH2Upgrade();
            if (req.ProtoAtLeast(1L, 1L) && (!haveHost || len(hosts) == 0L) && !isH2Upgrade && req.Method != "CONNECT")
            {
                return (_addr_null!, error.As(badRequestError("missing required Host header"))!);
            }

            if (len(hosts) > 1L)
            {
                return (_addr_null!, error.As(badRequestError("too many Host headers"))!);
            }

            if (len(hosts) == 1L && !httpguts.ValidHostHeader(hosts[0L]))
            {
                return (_addr_null!, error.As(badRequestError("malformed Host header"))!);
            }

            foreach (var (k, vv) in req.Header)
            {
                if (!httpguts.ValidHeaderFieldName(k))
                {
                    return (_addr_null!, error.As(badRequestError("invalid header name"))!);
                }

                foreach (var (_, v) in vv)
                {
                    if (!httpguts.ValidHeaderFieldValue(v))
                    {
                        return (_addr_null!, error.As(badRequestError("invalid header value"))!);
                    }

                }

            }
            delete(req.Header, "Host");

            var (ctx, cancelCtx) = context.WithCancel(ctx);
            req.ctx = ctx;
            req.RemoteAddr = c.remoteAddr;
            req.TLS = c.tlsState;
            {
                ptr<body> (body, ok) = req.Body._<ptr<body>>();

                if (ok)
                {
                    body.doEarlyClose = true;
                } 

                // Adjust the read deadline if necessary.

            } 

            // Adjust the read deadline if necessary.
            if (!hdrDeadline.Equal(wholeReqDeadline))
            {
                c.rwc.SetReadDeadline(wholeReqDeadline);
            }

            w = addr(new response(conn:c,cancelCtx:cancelCtx,req:req,reqBody:req.Body,handlerHeader:make(Header),contentLength:-1,closeNotifyCh:make(chanbool,1),wants10KeepAlive:req.wantsHttp10KeepAlive(),wantsClose:req.wantsClose(),));
            if (isH2Upgrade)
            {
                w.closeAfterReply = true;
            }

            w.cw.res = w;
            w.w = newBufioWriterSize(_addr_w.cw, bufferBeforeChunkingSize);
            return (_addr_w!, error.As(null!)!);

        });

        // http1ServerSupportsRequest reports whether Go's HTTP/1.x server
        // supports the given request.
        private static bool http1ServerSupportsRequest(ptr<Request> _addr_req)
        {
            ref Request req = ref _addr_req.val;

            if (req.ProtoMajor == 1L)
            {
                return true;
            } 
            // Accept "PRI * HTTP/2.0" upgrade requests, so Handlers can
            // wire up their own HTTP/2 upgrades.
            if (req.ProtoMajor == 2L && req.ProtoMinor == 0L && req.Method == "PRI" && req.RequestURI == "*")
            {
                return true;
            } 
            // Reject HTTP/0.x, and all other HTTP/2+ requests (which
            // aren't encoded in ASCII anyway).
            return false;

        }

        private static Header Header(this ptr<response> _addr_w)
        {
            ref response w = ref _addr_w.val;

            if (w.cw.header == null && w.wroteHeader && !w.cw.wroteHeader)
            { 
                // Accessing the header between logically writing it
                // and physically writing it means we need to allocate
                // a clone to snapshot the logically written state.
                w.cw.header = w.handlerHeader.Clone();

            }

            w.calledHeader = true;
            return w.handlerHeader;

        }

        // maxPostHandlerReadBytes is the max number of Request.Body bytes not
        // consumed by a handler that the server will read from the client
        // in order to keep a connection alive. If there are more bytes than
        // this then the server to be paranoid instead sends a "Connection:
        // close" response.
        //
        // This number is approximately what a typical machine's TCP buffer
        // size is anyway.  (if we have the bytes on the machine, we might as
        // well read them)
        private static readonly long maxPostHandlerReadBytes = (long)256L << (int)(10L);



        private static void checkWriteHeaderCode(long code) => func((_, panic, __) =>
        { 
            // Issue 22880: require valid WriteHeader status codes.
            // For now we only enforce that it's three digits.
            // In the future we might block things over 599 (600 and above aren't defined
            // at https://httpwg.org/specs/rfc7231.html#status.codes)
            // and we might block under 200 (once we have more mature 1xx support).
            // But for now any three digits.
            //
            // We used to send "HTTP/1.1 000 0" on the wire in responses but there's
            // no equivalent bogus thing we can realistically send in HTTP/2,
            // so we'll consistently panic instead and help people find their bugs
            // early. (We can't return an error from WriteHeader even if we wanted to.)
            if (code < 100L || code > 999L)
            {
                panic(fmt.Sprintf("invalid WriteHeader code %v", code));
            }

        });

        // relevantCaller searches the call stack for the first function outside of net/http.
        // The purpose of this function is to provide more helpful error messages.
        private static runtime.Frame relevantCaller()
        {
            var pc = make_slice<System.UIntPtr>(16L);
            var n = runtime.Callers(1L, pc);
            var frames = runtime.CallersFrames(pc[..n]);
            runtime.Frame frame = default;
            while (true)
            {
                var (frame, more) = frames.Next();
                if (!strings.HasPrefix(frame.Function, "net/http."))
                {
                    return frame;
                }

                if (!more)
                {
                    break;
                }

            }

            return frame;

        }

        private static void WriteHeader(this ptr<response> _addr_w, long code)
        {
            ref response w = ref _addr_w.val;

            if (w.conn.hijacked())
            {
                var caller = relevantCaller();
                w.conn.server.logf("http: response.WriteHeader on hijacked connection from %s (%s:%d)", caller.Function, path.Base(caller.File), caller.Line);
                return ;
            }

            if (w.wroteHeader)
            {
                caller = relevantCaller();
                w.conn.server.logf("http: superfluous response.WriteHeader call from %s (%s:%d)", caller.Function, path.Base(caller.File), caller.Line);
                return ;
            }

            checkWriteHeaderCode(code);
            w.wroteHeader = true;
            w.status = code;

            if (w.calledHeader && w.cw.header == null)
            {
                w.cw.header = w.handlerHeader.Clone();
            }

            {
                var cl = w.handlerHeader.get("Content-Length");

                if (cl != "")
                {
                    var (v, err) = strconv.ParseInt(cl, 10L, 64L);
                    if (err == null && v >= 0L)
                    {
                        w.contentLength = v;
                    }
                    else
                    {
                        w.conn.server.logf("http: invalid Content-Length of %q", cl);
                        w.handlerHeader.Del("Content-Length");
                    }

                }

            }

        }

        // extraHeader is the set of headers sometimes added by chunkWriter.writeHeader.
        // This type is used to avoid extra allocations from cloning and/or populating
        // the response Header map and all its 1-element slices.
        private partial struct extraHeader
        {
            public @string contentType;
            public @string connection;
            public @string transferEncoding;
            public slice<byte> date; // written if not nil
            public slice<byte> contentLength; // written if not nil
        }

        // Sorted the same as extraHeader.Write's loop.
        private static slice<byte> extraHeaderKeys = new slice<slice<byte>>(new slice<byte>[] { []byte("Content-Type"), []byte("Connection"), []byte("Transfer-Encoding") });

        private static slice<byte> headerContentLength = (slice<byte>)"Content-Length: ";        private static slice<byte> headerDate = (slice<byte>)"Date: ";

        // Write writes the headers described in h to w.
        //
        // This method has a value receiver, despite the somewhat large size
        // of h, because it prevents an allocation. The escape analysis isn't
        // smart enough to realize this function doesn't mutate h.
        private static void Write(this extraHeader h, ptr<bufio.Writer> _addr_w)
        {
            ref bufio.Writer w = ref _addr_w.val;

            if (h.date != null)
            {
                w.Write(headerDate);
                w.Write(h.date);
                w.Write(crlf);
            }

            if (h.contentLength != null)
            {
                w.Write(headerContentLength);
                w.Write(h.contentLength);
                w.Write(crlf);
            }

            foreach (var (i, v) in new slice<@string>(new @string[] { h.contentType, h.connection, h.transferEncoding }))
            {
                if (v != "")
                {
                    w.Write(extraHeaderKeys[i]);
                    w.Write(colonSpace);
                    w.WriteString(v);
                    w.Write(crlf);
                }

            }

        }

        // writeHeader finalizes the header sent to the client and writes it
        // to cw.res.conn.bufw.
        //
        // p is not written by writeHeader, but is the first chunk of the body
        // that will be written. It is sniffed for a Content-Type if none is
        // set explicitly. It's also used to set the Content-Length, if the
        // total body size was small and the handler has already finished
        // running.
        private static void writeHeader(this ptr<chunkWriter> _addr_cw, slice<byte> p)
        {
            ref chunkWriter cw = ref _addr_cw.val;

            if (cw.wroteHeader)
            {
                return ;
            }

            cw.wroteHeader = true;

            var w = cw.res;
            var keepAlivesEnabled = w.conn.server.doKeepAlives();
            var isHEAD = w.req.Method == "HEAD"; 

            // header is written out to w.conn.buf below. Depending on the
            // state of the handler, we either own the map or not. If we
            // don't own it, the exclude map is created lazily for
            // WriteSubset to remove headers. The setHeader struct holds
            // headers we need to add.
            var header = cw.header;
            var owned = header != null;
            if (!owned)
            {
                header = w.handlerHeader;
            }

            map<@string, bool> excludeHeader = default;
            Action<@string> delHeader = key =>
            {
                if (owned)
                {
                    header.Del(key);
                    return ;
                }

                {
                    var (_, ok) = header[key];

                    if (!ok)
                    {
                        return ;
                    }

                }

                if (excludeHeader == null)
                {
                    excludeHeader = make_map<@string, bool>();
                }

                excludeHeader[key] = true;

            }
;
            extraHeader setHeader = default; 

            // Don't write out the fake "Trailer:foo" keys. See TrailerPrefix.
            var trailers = false;
            {
                var k__prev1 = k;

                foreach (var (__k) in cw.header)
                {
                    k = __k;
                    if (strings.HasPrefix(k, TrailerPrefix))
                    {
                        if (excludeHeader == null)
                        {
                            excludeHeader = make_map<@string, bool>();
                        }

                        excludeHeader[k] = true;
                        trailers = true;

                    }

                }

                k = k__prev1;
            }

            foreach (var (_, v) in cw.header["Trailer"])
            {
                trailers = true;
                foreachHeaderElement(v, cw.res.declareTrailer);
            }
            var te = header.get("Transfer-Encoding");
            var hasTE = te != ""; 

            // If the handler is done but never sent a Content-Length
            // response header and this is our first (and last) write, set
            // it, even to zero. This helps HTTP/1.0 clients keep their
            // "keep-alive" connections alive.
            // Exceptions: 304/204/1xx responses never get Content-Length, and if
            // it was a HEAD request, we don't know the difference between
            // 0 actual bytes and 0 bytes because the handler noticed it
            // was a HEAD request and chose not to write anything. So for
            // HEAD, the handler should either write the Content-Length or
            // write non-zero bytes. If it's actually 0 bytes and the
            // handler never looked at the Request.Method, we just don't
            // send a Content-Length header.
            // Further, we don't send an automatic Content-Length if they
            // set a Transfer-Encoding, because they're generally incompatible.
            if (w.handlerDone.isSet() && !trailers && !hasTE && bodyAllowedForStatus(w.status) && header.get("Content-Length") == "" && (!isHEAD || len(p) > 0L))
            {
                w.contentLength = int64(len(p));
                setHeader.contentLength = strconv.AppendInt(cw.res.clenBuf[..0L], int64(len(p)), 10L);
            } 

            // If this was an HTTP/1.0 request with keep-alive and we sent a
            // Content-Length back, we can make this a keep-alive response ...
            if (w.wants10KeepAlive && keepAlivesEnabled)
            {
                var sentLength = header.get("Content-Length") != "";
                if (sentLength && header.get("Connection") == "keep-alive")
                {
                    w.closeAfterReply = false;
                }

            } 

            // Check for an explicit (and valid) Content-Length header.
            var hasCL = w.contentLength != -1L;

            if (w.wants10KeepAlive && (isHEAD || hasCL || !bodyAllowedForStatus(w.status)))
            {
                var (_, connectionHeaderSet) = header["Connection"];
                if (!connectionHeaderSet)
                {
                    setHeader.connection = "keep-alive";
                }

            }
            else if (!w.req.ProtoAtLeast(1L, 1L) || w.wantsClose)
            {
                w.closeAfterReply = true;
            }

            if (header.get("Connection") == "close" || !keepAlivesEnabled)
            {
                w.closeAfterReply = true;
            } 

            // If the client wanted a 100-continue but we never sent it to
            // them (or, more strictly: we never finished reading their
            // request body), don't reuse this connection because it's now
            // in an unknown state: we might be sending this response at
            // the same time the client is now sending its request body
            // after a timeout.  (Some HTTP clients send Expect:
            // 100-continue but knowing that some servers don't support
            // it, the clients set a timer and send the body later anyway)
            // If we haven't seen EOF, we can't skip over the unread body
            // because we don't know if the next bytes on the wire will be
            // the body-following-the-timer or the subsequent request.
            // See Issue 11549.
            {
                ptr<expectContinueReader> (ecr, ok) = w.req.Body._<ptr<expectContinueReader>>();

                if (ok && !ecr.sawEOF.isSet())
                {
                    w.closeAfterReply = true;
                } 

                // Per RFC 2616, we should consume the request body before
                // replying, if the handler hasn't already done so. But we
                // don't want to do an unbounded amount of reading here for
                // DoS reasons, so we only try up to a threshold.
                // TODO(bradfitz): where does RFC 2616 say that? See Issue 15527
                // about HTTP/1.x Handlers concurrently reading and writing, like
                // HTTP/2 handlers can do. Maybe this code should be relaxed?

            } 

            // Per RFC 2616, we should consume the request body before
            // replying, if the handler hasn't already done so. But we
            // don't want to do an unbounded amount of reading here for
            // DoS reasons, so we only try up to a threshold.
            // TODO(bradfitz): where does RFC 2616 say that? See Issue 15527
            // about HTTP/1.x Handlers concurrently reading and writing, like
            // HTTP/2 handlers can do. Maybe this code should be relaxed?
            if (w.req.ContentLength != 0L && !w.closeAfterReply)
            {
                bool discard = default;                bool tooBig = default;



                switch (w.req.Body.type())
                {
                    case ptr<expectContinueReader> bdy:
                        if (bdy.resp.wroteContinue)
                        {
                            discard = true;
                        }

                        break;
                    case ptr<body> bdy:
                        bdy.mu.Lock();

                        if (bdy.closed) 
                            if (!bdy.sawEOF)
                            { 
                                // Body was closed in handler with non-EOF error.
                                w.closeAfterReply = true;

                            }

                        else if (bdy.unreadDataSizeLocked() >= maxPostHandlerReadBytes) 
                            tooBig = true;
                        else 
                            discard = true;
                                                bdy.mu.Unlock();
                        break;
                    default:
                    {
                        var bdy = w.req.Body.type();
                        discard = true;
                        break;
                    }

                }

                if (discard)
                {
                    var (_, err) = io.CopyN(ioutil.Discard, w.reqBody, maxPostHandlerReadBytes + 1L);

                    if (err == null) 
                        // There must be even more data left over.
                        tooBig = true;
                    else if (err == ErrBodyReadAfterClose)                     else if (err == io.EOF) 
                        // The remaining body was just consumed, close it.
                        err = w.reqBody.Close();
                        if (err != null)
                        {
                            w.closeAfterReply = true;
                        }

                    else 
                        // Some other kind of error occurred, like a read timeout, or
                        // corrupt chunked encoding. In any case, whatever remains
                        // on the wire must not be parsed as another HTTP request.
                        w.closeAfterReply = true;
                    
                }

                if (tooBig)
                {
                    w.requestTooLarge();
                    delHeader("Connection");
                    setHeader.connection = "close";
                }

            }

            var code = w.status;
            if (bodyAllowedForStatus(code))
            { 
                // If no content type, apply sniffing algorithm to body.
                var (_, haveType) = header["Content-Type"]; 

                // If the Content-Encoding was set and is non-blank,
                // we shouldn't sniff the body. See Issue 31753.
                var ce = header.Get("Content-Encoding");
                var hasCE = len(ce) > 0L;
                if (!hasCE && !haveType && !hasTE && len(p) > 0L)
                {
                    setHeader.contentType = DetectContentType(p);
                }

            }
            else
            {
                {
                    var k__prev1 = k;

                    foreach (var (_, __k) in suppressedHeaders(code))
                    {
                        k = __k;
                        delHeader(k);
                    }

                    k = k__prev1;
                }
            }

            if (!header.has("Date"))
            {
                setHeader.date = appendTime(cw.res.dateBuf[..0L], time.Now());
            }

            if (hasCL && hasTE && te != "identity")
            { 
                // TODO: return an error if WriteHeader gets a return parameter
                // For now just ignore the Content-Length.
                w.conn.server.logf("http: WriteHeader called with both Transfer-Encoding of %q and a Content-Length of %d", te, w.contentLength);
                delHeader("Content-Length");
                hasCL = false;

            }

            if (w.req.Method == "HEAD" || !bodyAllowedForStatus(code))
            { 
                // do nothing
            }
            else if (code == StatusNoContent)
            {
                delHeader("Transfer-Encoding");
            }
            else if (hasCL)
            {
                delHeader("Transfer-Encoding");
            }
            else if (w.req.ProtoAtLeast(1L, 1L))
            { 
                // HTTP/1.1 or greater: Transfer-Encoding has been set to identity, and no
                // content-length has been provided. The connection must be closed after the
                // reply is written, and no chunking is to be done. This is the setup
                // recommended in the Server-Sent Events candidate recommendation 11,
                // section 8.
                if (hasTE && te == "identity")
                {
                    cw.chunking = false;
                    w.closeAfterReply = true;
                }
                else
                { 
                    // HTTP/1.1 or greater: use chunked transfer encoding
                    // to avoid closing the connection at EOF.
                    cw.chunking = true;
                    setHeader.transferEncoding = "chunked";
                    if (hasTE && te == "chunked")
                    { 
                        // We will send the chunked Transfer-Encoding header later.
                        delHeader("Transfer-Encoding");

                    }

                }

            }
            else
            { 
                // HTTP version < 1.1: cannot do chunked transfer
                // encoding and we don't know the Content-Length so
                // signal EOF by closing connection.
                w.closeAfterReply = true;
                delHeader("Transfer-Encoding"); // in case already set
            } 

            // Cannot use Content-Length with non-identity Transfer-Encoding.
            if (cw.chunking)
            {
                delHeader("Content-Length");
            }

            if (!w.req.ProtoAtLeast(1L, 0L))
            {
                return ;
            }

            if (w.closeAfterReply && (!keepAlivesEnabled || !hasToken(cw.header.get("Connection"), "close")))
            {
                delHeader("Connection");
                if (w.req.ProtoAtLeast(1L, 1L))
                {
                    setHeader.connection = "close";
                }

            }

            writeStatusLine(_addr_w.conn.bufw, w.req.ProtoAtLeast(1L, 1L), code, w.statusBuf[..]);
            cw.header.WriteSubset(w.conn.bufw, excludeHeader);
            setHeader.Write(w.conn.bufw);
            w.conn.bufw.Write(crlf);

        }

        // foreachHeaderElement splits v according to the "#rule" construction
        // in RFC 7230 section 7 and calls fn for each non-empty element.
        private static void foreachHeaderElement(@string v, Action<@string> fn)
        {
            v = textproto.TrimString(v);
            if (v == "")
            {
                return ;
            }

            if (!strings.Contains(v, ","))
            {
                fn(v);
                return ;
            }

            foreach (var (_, f) in strings.Split(v, ","))
            {
                f = textproto.TrimString(f);

                if (f != "")
                {
                    fn(f);
                }

            }

        }

        // writeStatusLine writes an HTTP/1.x Status-Line (RFC 7230 Section 3.1.2)
        // to bw. is11 is whether the HTTP request is HTTP/1.1. false means HTTP/1.0.
        // code is the response status code.
        // scratch is an optional scratch buffer. If it has at least capacity 3, it's used.
        private static void writeStatusLine(ptr<bufio.Writer> _addr_bw, bool is11, long code, slice<byte> scratch)
        {
            ref bufio.Writer bw = ref _addr_bw.val;

            if (is11)
            {
                bw.WriteString("HTTP/1.1 ");
            }
            else
            {
                bw.WriteString("HTTP/1.0 ");
            }

            {
                var (text, ok) = statusText[code];

                if (ok)
                {
                    bw.Write(strconv.AppendInt(scratch[..0L], int64(code), 10L));
                    bw.WriteByte(' ');
                    bw.WriteString(text);
                    bw.WriteString("\r\n");
                }
                else
                { 
                    // don't worry about performance
                    fmt.Fprintf(bw, "%03d status code %d\r\n", code, code);

                }

            }

        }

        // bodyAllowed reports whether a Write is allowed for this response type.
        // It's illegal to call this before the header has been flushed.
        private static bool bodyAllowed(this ptr<response> _addr_w) => func((_, panic, __) =>
        {
            ref response w = ref _addr_w.val;

            if (!w.wroteHeader)
            {
                panic("");
            }

            return bodyAllowedForStatus(w.status);

        });

        // The Life Of A Write is like this:
        //
        // Handler starts. No header has been sent. The handler can either
        // write a header, or just start writing. Writing before sending a header
        // sends an implicitly empty 200 OK header.
        //
        // If the handler didn't declare a Content-Length up front, we either
        // go into chunking mode or, if the handler finishes running before
        // the chunking buffer size, we compute a Content-Length and send that
        // in the header instead.
        //
        // Likewise, if the handler didn't set a Content-Type, we sniff that
        // from the initial chunk of output.
        //
        // The Writers are wired together like:
        //
        // 1. *response (the ResponseWriter) ->
        // 2. (*response).w, a *bufio.Writer of bufferBeforeChunkingSize bytes
        // 3. chunkWriter.Writer (whose writeHeader finalizes Content-Length/Type)
        //    and which writes the chunk headers, if needed.
        // 4. conn.buf, a bufio.Writer of default (4kB) bytes, writing to ->
        // 5. checkConnErrorWriter{c}, which notes any non-nil error on Write
        //    and populates c.werr with it if so. but otherwise writes to:
        // 6. the rwc, the net.Conn.
        //
        // TODO(bradfitz): short-circuit some of the buffering when the
        // initial header contains both a Content-Type and Content-Length.
        // Also short-circuit in (1) when the header's been sent and not in
        // chunking mode, writing directly to (4) instead, if (2) has no
        // buffered data. More generally, we could short-circuit from (1) to
        // (3) even in chunking mode if the write size from (1) is over some
        // threshold and nothing is in (2).  The answer might be mostly making
        // bufferBeforeChunkingSize smaller and having bufio's fast-paths deal
        // with this instead.
        private static (long, error) Write(this ptr<response> _addr_w, slice<byte> data)
        {
            long n = default;
            error err = default!;
            ref response w = ref _addr_w.val;

            return w.write(len(data), data, "");
        }

        private static (long, error) WriteString(this ptr<response> _addr_w, @string data)
        {
            long n = default;
            error err = default!;
            ref response w = ref _addr_w.val;

            return w.write(len(data), null, data);
        }

        // either dataB or dataS is non-zero.
        private static (long, error) write(this ptr<response> _addr_w, long lenData, slice<byte> dataB, @string dataS)
        {
            long n = default;
            error err = default!;
            ref response w = ref _addr_w.val;

            if (w.conn.hijacked())
            {
                if (lenData > 0L)
                {
                    var caller = relevantCaller();
                    w.conn.server.logf("http: response.Write on hijacked connection from %s (%s:%d)", caller.Function, path.Base(caller.File), caller.Line);
                }

                return (0L, error.As(ErrHijacked)!);

            }

            if (w.canWriteContinue.isSet())
            { 
                // Body reader wants to write 100 Continue but hasn't yet.
                // Tell it not to. The store must be done while holding the lock
                // because the lock makes sure that there is not an active write
                // this very moment.
                w.writeContinueMu.Lock();
                w.canWriteContinue.setFalse();
                w.writeContinueMu.Unlock();

            }

            if (!w.wroteHeader)
            {
                w.WriteHeader(StatusOK);
            }

            if (lenData == 0L)
            {
                return (0L, error.As(null!)!);
            }

            if (!w.bodyAllowed())
            {
                return (0L, error.As(ErrBodyNotAllowed)!);
            }

            w.written += int64(lenData); // ignoring errors, for errorKludge
            if (w.contentLength != -1L && w.written > w.contentLength)
            {
                return (0L, error.As(ErrContentLength)!);
            }

            if (dataB != null)
            {
                return w.w.Write(dataB);
            }
            else
            {
                return w.w.WriteString(dataS);
            }

        }

        private static void finishRequest(this ptr<response> _addr_w)
        {
            ref response w = ref _addr_w.val;

            w.handlerDone.setTrue();

            if (!w.wroteHeader)
            {
                w.WriteHeader(StatusOK);
            }

            w.w.Flush();
            putBufioWriter(_addr_w.w);
            w.cw.close();
            w.conn.bufw.Flush();

            w.conn.r.abortPendingRead(); 

            // Close the body (regardless of w.closeAfterReply) so we can
            // re-use its bufio.Reader later safely.
            w.reqBody.Close();

            if (w.req.MultipartForm != null)
            {
                w.req.MultipartForm.RemoveAll();
            }

        }

        // shouldReuseConnection reports whether the underlying TCP connection can be reused.
        // It must only be called after the handler is done executing.
        private static bool shouldReuseConnection(this ptr<response> _addr_w)
        {
            ref response w = ref _addr_w.val;

            if (w.closeAfterReply)
            { 
                // The request or something set while executing the
                // handler indicated we shouldn't reuse this
                // connection.
                return false;

            }

            if (w.req.Method != "HEAD" && w.contentLength != -1L && w.bodyAllowed() && w.contentLength != w.written)
            { 
                // Did not write enough. Avoid getting out of sync.
                return false;

            } 

            // There was some error writing to the underlying connection
            // during the request, so don't re-use this conn.
            if (w.conn.werr != null)
            {
                return false;
            }

            if (w.closedRequestBodyEarly())
            {
                return false;
            }

            return true;

        }

        private static bool closedRequestBodyEarly(this ptr<response> _addr_w)
        {
            ref response w = ref _addr_w.val;

            ptr<body> (body, ok) = w.req.Body._<ptr<body>>();
            return ok && body.didEarlyClose();
        }

        private static void Flush(this ptr<response> _addr_w)
        {
            ref response w = ref _addr_w.val;

            if (!w.wroteHeader)
            {
                w.WriteHeader(StatusOK);
            }

            w.w.Flush();
            w.cw.flush();

        }

        private static void finalFlush(this ptr<conn> _addr_c)
        {
            ref conn c = ref _addr_c.val;

            if (c.bufr != null)
            { 
                // Steal the bufio.Reader (~4KB worth of memory) and its associated
                // reader for a future connection.
                putBufioReader(_addr_c.bufr);
                c.bufr = null;

            }

            if (c.bufw != null)
            {
                c.bufw.Flush(); 
                // Steal the bufio.Writer (~4KB worth of memory) and its associated
                // writer for a future connection.
                putBufioWriter(_addr_c.bufw);
                c.bufw = null;

            }

        }

        // Close the connection.
        private static void close(this ptr<conn> _addr_c)
        {
            ref conn c = ref _addr_c.val;

            c.finalFlush();
            c.rwc.Close();
        }

        // rstAvoidanceDelay is the amount of time we sleep after closing the
        // write side of a TCP connection before closing the entire socket.
        // By sleeping, we increase the chances that the client sees our FIN
        // and processes its final data before they process the subsequent RST
        // from closing a connection with known unread data.
        // This RST seems to occur mostly on BSD systems. (And Windows?)
        // This timeout is somewhat arbitrary (~latency around the planet).
        private static readonly long rstAvoidanceDelay = (long)500L * time.Millisecond;



        private partial interface closeWriter
        {
            error CloseWrite();
        }

        private static closeWriter _ = closeWriter.As((net.TCPConn.val)(null))!;

        // closeWrite flushes any outstanding data and sends a FIN packet (if
        // client is connected via TCP), signalling that we're done. We then
        // pause for a bit, hoping the client processes it before any
        // subsequent RST.
        //
        // See https://golang.org/issue/3595
        private static void closeWriteAndWait(this ptr<conn> _addr_c)
        {
            ref conn c = ref _addr_c.val;

            c.finalFlush();
            {
                closeWriter (tcp, ok) = closeWriter.As(c.rwc._<closeWriter>())!;

                if (ok)
                {
                    tcp.CloseWrite();
                }

            }

            time.Sleep(rstAvoidanceDelay);

        }

        // validNextProto reports whether the proto is a valid ALPN protocol name.
        // Everything is valid except the empty string and built-in protocol types,
        // so that those can't be overridden with alternate implementations.
        private static bool validNextProto(@string proto)
        {
            switch (proto)
            {
                case "": 

                case "http/1.1": 

                case "http/1.0": 
                    return false;
                    break;
            }
            return true;

        }

        private static void setState(this ptr<conn> _addr_c, net.Conn nc, ConnState state) => func((_, panic, __) =>
        {
            ref conn c = ref _addr_c.val;

            var srv = c.server;

            if (state == StateNew) 
                srv.trackConn(c, true);
            else if (state == StateHijacked || state == StateClosed) 
                srv.trackConn(c, false);
                        if (state > 0xffUL || state < 0L)
            {
                panic("internal error");
            }

            var packedState = uint64(time.Now().Unix() << (int)(8L)) | uint64(state);
            atomic.StoreUint64(_addr_c.curState.atomic, packedState);
            {
                var hook = srv.ConnState;

                if (hook != null)
                {
                    hook(nc, state);
                }

            }

        });

        private static (ConnState, long) getState(this ptr<conn> _addr_c)
        {
            ConnState state = default;
            long unixSec = default;
            ref conn c = ref _addr_c.val;

            var packedState = atomic.LoadUint64(_addr_c.curState.atomic);
            return (ConnState(packedState & 0xffUL), int64(packedState >> (int)(8L)));
        }

        // badRequestError is a literal string (used by in the server in HTML,
        // unescaped) to tell the user why their request was bad. It should
        // be plain text without user info or other embedded errors.
        private partial struct badRequestError // : @string
        {
        }

        private static @string Error(this badRequestError e)
        {
            return "Bad Request: " + string(e);
        }

        // ErrAbortHandler is a sentinel panic value to abort a handler.
        // While any panic from ServeHTTP aborts the response to the client,
        // panicking with ErrAbortHandler also suppresses logging of a stack
        // trace to the server's error log.
        public static var ErrAbortHandler = errors.New("net/http: abort Handler");

        // isCommonNetReadError reports whether err is a common error
        // encountered during reading a request off the network when the
        // client has gone away or had its read fail somehow. This is used to
        // determine which logs are interesting enough to log about.
        private static bool isCommonNetReadError(error err)
        {
            if (err == io.EOF)
            {
                return true;
            }

            {
                net.Error (neterr, ok) = err._<net.Error>();

                if (ok && neterr.Timeout())
                {
                    return true;
                }

            }

            {
                ptr<net.OpError> (oe, ok) = err._<ptr<net.OpError>>();

                if (ok && oe.Op == "read")
                {
                    return true;
                }

            }

            return false;

        }

        // Serve a new connection.
        private static void serve(this ptr<conn> _addr_c, context.Context ctx) => func((defer, _, __) =>
        {
            ref conn c = ref _addr_c.val;

            c.remoteAddr = c.rwc.RemoteAddr().String();
            ctx = context.WithValue(ctx, LocalAddrContextKey, c.rwc.LocalAddr());
            defer(() =>
            {
                {
                    var err__prev1 = err;

                    var err = recover();

                    if (err != null && err != ErrAbortHandler)
                    {
                        const long size = (long)64L << (int)(10L);

                        var buf = make_slice<byte>(size);
                        buf = buf[..runtime.Stack(buf, false)];
                        c.server.logf("http: panic serving %v: %v\n%s", c.remoteAddr, err, buf);
                    }

                    err = err__prev1;

                }

                if (!c.hijacked())
                {
                    c.close();
                    c.setState(c.rwc, StateClosed);
                }

            }());

            {
                ptr<tls.Conn> (tlsConn, ok) = c.rwc._<ptr<tls.Conn>>();

                if (ok)
                {
                    {
                        var d__prev2 = d;

                        var d = c.server.ReadTimeout;

                        if (d != 0L)
                        {
                            c.rwc.SetReadDeadline(time.Now().Add(d));
                        }

                        d = d__prev2;

                    }

                    {
                        var d__prev2 = d;

                        d = c.server.WriteTimeout;

                        if (d != 0L)
                        {
                            c.rwc.SetWriteDeadline(time.Now().Add(d));
                        }

                        d = d__prev2;

                    }

                    {
                        var err__prev2 = err;

                        err = tlsConn.Handshake();

                        if (err != null)
                        { 
                            // If the handshake failed due to the client not speaking
                            // TLS, assume they're speaking plaintext HTTP and write a
                            // 400 response on the TLS conn's underlying net.Conn.
                            {
                                tls.RecordHeaderError (re, ok) = err._<tls.RecordHeaderError>();

                                if (ok && re.Conn != null && tlsRecordHeaderLooksLikeHTTP(re.RecordHeader))
                                {
                                    io.WriteString(re.Conn, "HTTP/1.0 400 Bad Request\r\n\r\nClient sent an HTTP request to an HTTPS server.\n");
                                    re.Conn.Close();
                                    return ;
                                }

                            }

                            c.server.logf("http: TLS handshake error from %s: %v", c.rwc.RemoteAddr(), err);
                            return ;

                        }

                        err = err__prev2;

                    }

                    c.tlsState = @new<tls.ConnectionState>();
                    c.tlsState.val = tlsConn.ConnectionState();
                    {
                        var proto = c.tlsState.NegotiatedProtocol;

                        if (validNextProto(proto))
                        {
                            {
                                var fn = c.server.TLSNextProto[proto];

                                if (fn != null)
                                {
                                    initALPNRequest h = new initALPNRequest(ctx,tlsConn,serverHandler{c.server});
                                    fn(c.server, tlsConn, h);
                                }

                            }

                            return ;

                        }

                    }

                } 

                // HTTP/1.x from here on.

            } 

            // HTTP/1.x from here on.

            var (ctx, cancelCtx) = context.WithCancel(ctx);
            c.cancelCtx = cancelCtx;
            defer(cancelCtx());

            c.r = addr(new connReader(conn:c));
            c.bufr = newBufioReader(c.r);
            c.bufw = newBufioWriterSize(new checkConnErrorWriter(c), 4L << (int)(10L));

            while (true)
            {
                var (w, err) = c.readRequest(ctx);
                if (c.r.remain != c.server.initialReadLimitSize())
                { 
                    // If we read any bytes off the wire, we're active.
                    c.setState(c.rwc, StateActive);

                }

                if (err != null)
                {
                    const @string errorHeaders = (@string)"\r\nContent-Type: text/plain; charset=utf-8\r\nConnection: close\r\n\r\n";




                    if (err == errTooLarge) 
                        // Their HTTP client may or may not be
                        // able to read this if we're
                        // responding to them and hanging up
                        // while they're still writing their
                        // request. Undefined behavior.
                        const @string publicErr = (@string)"431 Request Header Fields Too Large";

                        fmt.Fprintf(c.rwc, "HTTP/1.1 " + publicErr + errorHeaders + publicErr);
                        c.closeWriteAndWait();
                        return ;
                    else if (isUnsupportedTEError(err)) 
                        // Respond as per RFC 7230 Section 3.3.1 which says,
                        //      A server that receives a request message with a
                        //      transfer coding it does not understand SHOULD
                        //      respond with 501 (Unimplemented).
                        var code = StatusNotImplemented; 

                        // We purposefully aren't echoing back the transfer-encoding's value,
                        // so as to mitigate the risk of cross side scripting by an attacker.
                        fmt.Fprintf(c.rwc, "HTTP/1.1 %d %s%sUnsupported transfer encoding", code, StatusText(code), errorHeaders);
                        return ;
                    else if (isCommonNetReadError(err)) 
                        return ; // don't reply
                    else 
                        @string publicErr = "400 Bad Request";
                        {
                            badRequestError (v, ok) = err._<badRequestError>();

                            if (ok)
                            {
                                publicErr = publicErr + ": " + string(v);
                            }

                        }


                        fmt.Fprintf(c.rwc, "HTTP/1.1 " + publicErr + errorHeaders + publicErr);
                        return ;
                    
                } 

                // Expect 100 Continue support
                var req = w.req;
                if (req.expectsContinue())
                {
                    if (req.ProtoAtLeast(1L, 1L) && req.ContentLength != 0L)
                    { 
                        // Wrap the Body reader with one that replies on the connection
                        req.Body = addr(new expectContinueReader(readCloser:req.Body,resp:w));
                        w.canWriteContinue.setTrue();

                    }

                }
                else if (req.Header.get("Expect") != "")
                {
                    w.sendExpectationFailed();
                    return ;
                }

                c.curReq.Store(w);

                if (requestBodyRemains(req.Body))
                {
                    registerOnHitEOF(req.Body, w.conn.r.startBackgroundRead);
                }
                else
                {
                    w.conn.r.startBackgroundRead();
                } 

                // HTTP cannot have multiple simultaneous active requests.[*]
                // Until the server replies to this request, it can't read another,
                // so we might as well run the handler in this goroutine.
                // [*] Not strictly true: HTTP pipelining. We could let them all process
                // in parallel even if their responses need to be serialized.
                // But we're not going to implement HTTP pipelining because it
                // was never deployed in the wild and the answer is HTTP/2.
                new serverHandler(c.server).ServeHTTP(w, w.req);
                w.cancelCtx();
                if (c.hijacked())
                {
                    return ;
                }

                w.finishRequest();
                if (!w.shouldReuseConnection())
                {
                    if (w.requestBodyLimitHit || w.closedRequestBodyEarly())
                    {
                        c.closeWriteAndWait();
                    }

                    return ;

                }

                c.setState(c.rwc, StateIdle);
                c.curReq.Store((response.val)(null));

                if (!w.conn.server.doKeepAlives())
                { 
                    // We're in shutdown mode. We might've replied
                    // to the user without "Connection: close" and
                    // they might think they can send another
                    // request, but such is life with HTTP/1.1.
                    return ;

                }

                {
                    var d__prev1 = d;

                    d = c.server.idleTimeout();

                    if (d != 0L)
                    {
                        c.rwc.SetReadDeadline(time.Now().Add(d));
                        {
                            var err__prev2 = err;

                            var (_, err) = c.bufr.Peek(4L);

                            if (err != null)
                            {
                                return ;
                            }

                            err = err__prev2;

                        }

                    }

                    d = d__prev1;

                }

                c.rwc.SetReadDeadline(new time.Time());

            }


        });

        private static void sendExpectationFailed(this ptr<response> _addr_w)
        {
            ref response w = ref _addr_w.val;
 
            // TODO(bradfitz): let ServeHTTP handlers handle
            // requests with non-standard expectation[s]? Seems
            // theoretical at best, and doesn't fit into the
            // current ServeHTTP model anyway. We'd need to
            // make the ResponseWriter an optional
            // "ExpectReplier" interface or something.
            //
            // For now we'll just obey RFC 7231 5.1.1 which says
            // "A server that receives an Expect field-value other
            // than 100-continue MAY respond with a 417 (Expectation
            // Failed) status code to indicate that the unexpected
            // expectation cannot be met."
            w.Header().Set("Connection", "close");
            w.WriteHeader(StatusExpectationFailed);
            w.finishRequest();

        }

        // Hijack implements the Hijacker.Hijack method. Our response is both a ResponseWriter
        // and a Hijacker.
        private static (net.Conn, ptr<bufio.ReadWriter>, error) Hijack(this ptr<response> _addr_w) => func((defer, panic, _) =>
        {
            net.Conn rwc = default;
            ptr<bufio.ReadWriter> buf = default!;
            error err = default!;
            ref response w = ref _addr_w.val;

            if (w.handlerDone.isSet())
            {
                panic("net/http: Hijack called after ServeHTTP finished");
            }

            if (w.wroteHeader)
            {
                w.cw.flush();
            }

            var c = w.conn;
            c.mu.Lock();
            defer(c.mu.Unlock()); 

            // Release the bufioWriter that writes to the chunk writer, it is not
            // used after a connection has been hijacked.
            rwc, buf, err = c.hijackLocked();
            if (err == null)
            {
                putBufioWriter(_addr_w.w);
                w.w = null;
            }

            return (rwc, _addr_buf!, error.As(err)!);

        });

        private static channel<bool> CloseNotify(this ptr<response> _addr_w) => func((_, panic, __) =>
        {
            ref response w = ref _addr_w.val;

            if (w.handlerDone.isSet())
            {
                panic("net/http: CloseNotify called after ServeHTTP finished");
            }

            return w.closeNotifyCh;

        });

        private static void registerOnHitEOF(io.ReadCloser rc, Action fn) => func((_, panic, __) =>
        {
            switch (rc.type())
            {
                case ptr<expectContinueReader> v:
                    registerOnHitEOF(v.readCloser, fn);
                    break;
                case ptr<body> v:
                    v.registerOnHitEOF(fn);
                    break;
                default:
                {
                    var v = rc.type();
                    panic("unexpected type " + fmt.Sprintf("%T", rc));
                    break;
                }
            }

        });

        // requestBodyRemains reports whether future calls to Read
        // on rc might yield more data.
        private static bool requestBodyRemains(io.ReadCloser rc) => func((_, panic, __) =>
        {
            if (rc == NoBody)
            {
                return false;
            }

            switch (rc.type())
            {
                case ptr<expectContinueReader> v:
                    return requestBodyRemains(v.readCloser);
                    break;
                case ptr<body> v:
                    return v.bodyRemains();
                    break;
                default:
                {
                    var v = rc.type();
                    panic("unexpected type " + fmt.Sprintf("%T", rc));
                    break;
                }
            }

        });

        // The HandlerFunc type is an adapter to allow the use of
        // ordinary functions as HTTP handlers. If f is a function
        // with the appropriate signature, HandlerFunc(f) is a
        // Handler that calls f.
        public delegate void HandlerFunc(ResponseWriter, ptr<Request>);

        // ServeHTTP calls f(w, r).
        public static void ServeHTTP(this HandlerFunc f, ResponseWriter w, ptr<Request> _addr_r)
        {
            ref Request r = ref _addr_r.val;

            f(w, r);
        }

        // Helper handlers

        // Error replies to the request with the specified error message and HTTP code.
        // It does not otherwise end the request; the caller should ensure no further
        // writes are done to w.
        // The error message should be plain text.
        public static void Error(ResponseWriter w, @string error, long code)
        {
            w.Header().Set("Content-Type", "text/plain; charset=utf-8");
            w.Header().Set("X-Content-Type-Options", "nosniff");
            w.WriteHeader(code);
            fmt.Fprintln(w, error);
        }

        // NotFound replies to the request with an HTTP 404 not found error.
        public static void NotFound(ResponseWriter w, ptr<Request> _addr_r)
        {
            ref Request r = ref _addr_r.val;

            Error(w, "404 page not found", StatusNotFound);
        }

        // NotFoundHandler returns a simple request handler
        // that replies to each request with a ``404 page not found'' reply.
        public static Handler NotFoundHandler()
        {
            return HandlerFunc(NotFound);
        }

        // StripPrefix returns a handler that serves HTTP requests
        // by removing the given prefix from the request URL's Path
        // and invoking the handler h. StripPrefix handles a
        // request for a path that doesn't begin with prefix by
        // replying with an HTTP 404 not found error.
        public static Handler StripPrefix(@string prefix, Handler h)
        {
            if (prefix == "")
            {
                return h;
            }

            return HandlerFunc((w, r) =>
            {
                {
                    var p = strings.TrimPrefix(r.URL.Path, prefix);

                    if (len(p) < len(r.URL.Path))
                    {
                        ptr<Request> r2 = @new<Request>();
                        r2.val = r.val;
                        r2.URL = @new<url.URL>();
                        r2.URL.val = r.URL.val;
                        r2.URL.Path = p;
                        h.ServeHTTP(w, r2);
                    }
                    else
                    {
                        NotFound(w, _addr_r);
                    }

                }

            });

        }

        // Redirect replies to the request with a redirect to url,
        // which may be a path relative to the request path.
        //
        // The provided code should be in the 3xx range and is usually
        // StatusMovedPermanently, StatusFound or StatusSeeOther.
        //
        // If the Content-Type header has not been set, Redirect sets it
        // to "text/html; charset=utf-8" and writes a small HTML body.
        // Setting the Content-Type header to any value, including nil,
        // disables that behavior.
        public static void Redirect(ResponseWriter w, ptr<Request> _addr_r, @string url, long code)
        {
            ref Request r = ref _addr_r.val;

            {
                var (u, err) = urlpkg.Parse(url);

                if (err == null)
                { 
                    // If url was relative, make its path absolute by
                    // combining with request path.
                    // The client would probably do this for us,
                    // but doing it ourselves is more reliable.
                    // See RFC 7231, section 7.1.2
                    if (u.Scheme == "" && u.Host == "")
                    {
                        var oldpath = r.URL.Path;
                        if (oldpath == "")
                        { // should not happen, but avoid a crash if it does
                            oldpath = "/";

                        } 

                        // no leading http://server
                        if (url == "" || url[0L] != '/')
                        { 
                            // make relative path absolute
                            var (olddir, _) = path.Split(oldpath);
                            url = olddir + url;

                        }

                        @string query = default;
                        {
                            var i = strings.Index(url, "?");

                            if (i != -1L)
                            {
                                url = url[..i];
                                query = url[i..];

                            } 

                            // clean up but preserve trailing slash

                        } 

                        // clean up but preserve trailing slash
                        var trailing = strings.HasSuffix(url, "/");
                        url = path.Clean(url);
                        if (trailing && !strings.HasSuffix(url, "/"))
                        {
                            url += "/";
                        }

                        url += query;

                    }

                }

            }


            var h = w.Header(); 

            // RFC 7231 notes that a short HTML body is usually included in
            // the response because older user agents may not understand 301/307.
            // Do it only if the request didn't already have a Content-Type header.
            var (_, hadCT) = h["Content-Type"];

            h.Set("Location", hexEscapeNonASCII(url));
            if (!hadCT && (r.Method == "GET" || r.Method == "HEAD"))
            {
                h.Set("Content-Type", "text/html; charset=utf-8");
            }

            w.WriteHeader(code); 

            // Shouldn't send the body for POST or HEAD; that leaves GET.
            if (!hadCT && r.Method == "GET")
            {
                @string body = "<a href=\"" + htmlEscape(url) + "\">" + statusText[code] + "</a>.\n";
                fmt.Fprintln(w, body);
            }

        }

        private static var htmlReplacer = strings.NewReplacer("&", "&amp;", "<", "&lt;", ">", "&gt;", "\"", "&#34;", "'", "&#39;");

        private static @string htmlEscape(@string s)
        {
            return htmlReplacer.Replace(s);
        }

        // Redirect to a fixed URL
        private partial struct redirectHandler
        {
            public @string url;
            public long code;
        }

        private static void ServeHTTP(this ptr<redirectHandler> _addr_rh, ResponseWriter w, ptr<Request> _addr_r)
        {
            ref redirectHandler rh = ref _addr_rh.val;
            ref Request r = ref _addr_r.val;

            Redirect(w, _addr_r, rh.url, rh.code);
        }

        // RedirectHandler returns a request handler that redirects
        // each request it receives to the given url using the given
        // status code.
        //
        // The provided code should be in the 3xx range and is usually
        // StatusMovedPermanently, StatusFound or StatusSeeOther.
        public static Handler RedirectHandler(@string url, long code)
        {
            return addr(new redirectHandler(url,code));
        }

        // ServeMux is an HTTP request multiplexer.
        // It matches the URL of each incoming request against a list of registered
        // patterns and calls the handler for the pattern that
        // most closely matches the URL.
        //
        // Patterns name fixed, rooted paths, like "/favicon.ico",
        // or rooted subtrees, like "/images/" (note the trailing slash).
        // Longer patterns take precedence over shorter ones, so that
        // if there are handlers registered for both "/images/"
        // and "/images/thumbnails/", the latter handler will be
        // called for paths beginning "/images/thumbnails/" and the
        // former will receive requests for any other paths in the
        // "/images/" subtree.
        //
        // Note that since a pattern ending in a slash names a rooted subtree,
        // the pattern "/" matches all paths not matched by other registered
        // patterns, not just the URL with Path == "/".
        //
        // If a subtree has been registered and a request is received naming the
        // subtree root without its trailing slash, ServeMux redirects that
        // request to the subtree root (adding the trailing slash). This behavior can
        // be overridden with a separate registration for the path without
        // the trailing slash. For example, registering "/images/" causes ServeMux
        // to redirect a request for "/images" to "/images/", unless "/images" has
        // been registered separately.
        //
        // Patterns may optionally begin with a host name, restricting matches to
        // URLs on that host only. Host-specific patterns take precedence over
        // general patterns, so that a handler might register for the two patterns
        // "/codesearch" and "codesearch.google.com/" without also taking over
        // requests for "http://www.google.com/".
        //
        // ServeMux also takes care of sanitizing the URL request path and the Host
        // header, stripping the port number and redirecting any request containing . or
        // .. elements or repeated slashes to an equivalent, cleaner URL.
        public partial struct ServeMux
        {
            public sync.RWMutex mu;
            public map<@string, muxEntry> m;
            public slice<muxEntry> es; // slice of entries sorted from longest to shortest.
            public bool hosts; // whether any patterns contain hostnames
        }

        private partial struct muxEntry
        {
            public Handler h;
            public @string pattern;
        }

        // NewServeMux allocates and returns a new ServeMux.
        public static ptr<ServeMux> NewServeMux()
        {
            return @new<ServeMux>();
        }

        // DefaultServeMux is the default ServeMux used by Serve.
        public static var DefaultServeMux = _addr_defaultServeMux;

        private static ServeMux defaultServeMux = default;

        // cleanPath returns the canonical path for p, eliminating . and .. elements.
        private static @string cleanPath(@string p)
        {
            if (p == "")
            {
                return "/";
            }

            if (p[0L] != '/')
            {
                p = "/" + p;
            }

            var np = path.Clean(p); 
            // path.Clean removes trailing slash except for root;
            // put the trailing slash back if necessary.
            if (p[len(p) - 1L] == '/' && np != "/")
            { 
                // Fast path for common case of p being the string we want:
                if (len(p) == len(np) + 1L && strings.HasPrefix(p, np))
                {
                    np = p;
                }
                else
                {
                    np += "/";
                }

            }

            return np;

        }

        // stripHostPort returns h without any trailing ":<port>".
        private static @string stripHostPort(@string h)
        { 
            // If no port on host, return unchanged
            if (strings.IndexByte(h, ':') == -1L)
            {
                return h;
            }

            var (host, _, err) = net.SplitHostPort(h);
            if (err != null)
            {
                return h; // on error, return unchanged
            }

            return host;

        }

        // Find a handler on a handler map given a path string.
        // Most-specific (longest) pattern wins.
        private static (Handler, @string) match(this ptr<ServeMux> _addr_mux, @string path)
        {
            Handler h = default;
            @string pattern = default;
            ref ServeMux mux = ref _addr_mux.val;
 
            // Check for exact match first.
            var (v, ok) = mux.m[path];
            if (ok)
            {
                return (v.h, v.pattern);
            } 

            // Check for longest valid match.  mux.es contains all patterns
            // that end in / sorted from longest to shortest.
            foreach (var (_, e) in mux.es)
            {
                if (strings.HasPrefix(path, e.pattern))
                {
                    return (e.h, e.pattern);
                }

            }
            return (null, "");

        }

        // redirectToPathSlash determines if the given path needs appending "/" to it.
        // This occurs when a handler for path + "/" was already registered, but
        // not for path itself. If the path needs appending to, it creates a new
        // URL, setting the path to u.Path + "/" and returning true to indicate so.
        private static (ptr<url.URL>, bool) redirectToPathSlash(this ptr<ServeMux> _addr_mux, @string host, @string path, ptr<url.URL> _addr_u)
        {
            ptr<url.URL> _p0 = default!;
            bool _p0 = default;
            ref ServeMux mux = ref _addr_mux.val;
            ref url.URL u = ref _addr_u.val;

            mux.mu.RLock();
            var shouldRedirect = mux.shouldRedirectRLocked(host, path);
            mux.mu.RUnlock();
            if (!shouldRedirect)
            {
                return (_addr_u!, false);
            }

            path = path + "/";
            u = addr(new url.URL(Path:path,RawQuery:u.RawQuery));
            return (_addr_u!, true);

        }

        // shouldRedirectRLocked reports whether the given path and host should be redirected to
        // path+"/". This should happen if a handler is registered for path+"/" but
        // not path -- see comments at ServeMux.
        private static bool shouldRedirectRLocked(this ptr<ServeMux> _addr_mux, @string host, @string path)
        {
            ref ServeMux mux = ref _addr_mux.val;

            @string p = new slice<@string>(new @string[] { path, host+path });

            {
                var c__prev1 = c;

                foreach (var (_, __c) in p)
                {
                    c = __c;
                    {
                        var (_, exist) = mux.m[c];

                        if (exist)
                        {
                            return false;
                        }

                    }

                }

                c = c__prev1;
            }

            var n = len(path);
            if (n == 0L)
            {
                return false;
            }

            {
                var c__prev1 = c;

                foreach (var (_, __c) in p)
                {
                    c = __c;
                    {
                        (_, exist) = mux.m[c + "/"];

                        if (exist)
                        {
                            return path[n - 1L] != '/';
                        }

                    }

                }

                c = c__prev1;
            }

            return false;

        }

        // Handler returns the handler to use for the given request,
        // consulting r.Method, r.Host, and r.URL.Path. It always returns
        // a non-nil handler. If the path is not in its canonical form, the
        // handler will be an internally-generated handler that redirects
        // to the canonical path. If the host contains a port, it is ignored
        // when matching handlers.
        //
        // The path and host are used unchanged for CONNECT requests.
        //
        // Handler also returns the registered pattern that matches the
        // request or, in the case of internally-generated redirects,
        // the pattern that will match after following the redirect.
        //
        // If there is no registered handler that applies to the request,
        // Handler returns a ``page not found'' handler and an empty pattern.
        private static (Handler, @string) Handler(this ptr<ServeMux> _addr_mux, ptr<Request> _addr_r)
        {
            Handler h = default;
            @string pattern = default;
            ref ServeMux mux = ref _addr_mux.val;
            ref Request r = ref _addr_r.val;

            // CONNECT requests are not canonicalized.
            if (r.Method == "CONNECT")
            { 
                // If r.URL.Path is /tree and its handler is not registered,
                // the /tree -> /tree/ redirect applies to CONNECT requests
                // but the path canonicalization does not.
                {
                    var u__prev2 = u;

                    var (u, ok) = mux.redirectToPathSlash(r.URL.Host, r.URL.Path, r.URL);

                    if (ok)
                    {
                        return (RedirectHandler(u.String(), StatusMovedPermanently), u.Path);
                    }

                    u = u__prev2;

                }


                return mux.handler(r.Host, r.URL.Path);

            } 

            // All other requests have any port stripped and path cleaned
            // before passing to mux.handler.
            var host = stripHostPort(r.Host);
            var path = cleanPath(r.URL.Path); 

            // If the given path is /tree and its handler is not registered,
            // redirect for /tree/.
            {
                var u__prev1 = u;

                (u, ok) = mux.redirectToPathSlash(host, path, r.URL);

                if (ok)
                {
                    return (RedirectHandler(u.String(), StatusMovedPermanently), u.Path);
                }

                u = u__prev1;

            }


            if (path != r.URL.Path)
            {
                _, pattern = mux.handler(host, path);
                var url = r.URL.val;
                url.Path = path;
                return (RedirectHandler(url.String(), StatusMovedPermanently), pattern);
            }

            return mux.handler(host, r.URL.Path);

        }

        // handler is the main implementation of Handler.
        // The path is known to be in canonical form, except for CONNECT methods.
        private static (Handler, @string) handler(this ptr<ServeMux> _addr_mux, @string host, @string path) => func((defer, _, __) =>
        {
            Handler h = default;
            @string pattern = default;
            ref ServeMux mux = ref _addr_mux.val;

            mux.mu.RLock();
            defer(mux.mu.RUnlock()); 

            // Host-specific pattern takes precedence over generic ones
            if (mux.hosts)
            {
                h, pattern = mux.match(host + path);
            }

            if (h == null)
            {
                h, pattern = mux.match(path);
            }

            if (h == null)
            {
                h = NotFoundHandler();
                pattern = "";

            }

            return ;

        });

        // ServeHTTP dispatches the request to the handler whose
        // pattern most closely matches the request URL.
        private static void ServeHTTP(this ptr<ServeMux> _addr_mux, ResponseWriter w, ptr<Request> _addr_r)
        {
            ref ServeMux mux = ref _addr_mux.val;
            ref Request r = ref _addr_r.val;

            if (r.RequestURI == "*")
            {
                if (r.ProtoAtLeast(1L, 1L))
                {
                    w.Header().Set("Connection", "close");
                }

                w.WriteHeader(StatusBadRequest);
                return ;

            }

            var (h, _) = mux.Handler(r);
            h.ServeHTTP(w, r);

        }

        // Handle registers the handler for the given pattern.
        // If a handler already exists for pattern, Handle panics.
        private static void Handle(this ptr<ServeMux> _addr_mux, @string pattern, Handler handler) => func((defer, panic, _) =>
        {
            ref ServeMux mux = ref _addr_mux.val;

            mux.mu.Lock();
            defer(mux.mu.Unlock());

            if (pattern == "")
            {
                panic("http: invalid pattern");
            }

            if (handler == null)
            {
                panic("http: nil handler");
            }

            {
                var (_, exist) = mux.m[pattern];

                if (exist)
                {
                    panic("http: multiple registrations for " + pattern);
                }

            }


            if (mux.m == null)
            {
                mux.m = make_map<@string, muxEntry>();
            }

            muxEntry e = new muxEntry(h:handler,pattern:pattern);
            mux.m[pattern] = e;
            if (pattern[len(pattern) - 1L] == '/')
            {
                mux.es = appendSorted(mux.es, e);
            }

            if (pattern[0L] != '/')
            {
                mux.hosts = true;
            }

        });

        private static slice<muxEntry> appendSorted(slice<muxEntry> es, muxEntry e)
        {
            var n = len(es);
            var i = sort.Search(n, i =>
            {
                return len(es[i].pattern) < len(e.pattern);
            });
            if (i == n)
            {
                return append(es, e);
            } 
            // we now know that i points at where we want to insert
            es = append(es, new muxEntry()); // try to grow the slice in place, any entry works.
            copy(es[i + 1L..], es[i..]); // Move shorter entries down
            es[i] = e;
            return es;

        }

        // HandleFunc registers the handler function for the given pattern.
        private static void HandleFunc(this ptr<ServeMux> _addr_mux, @string pattern, Action<ResponseWriter, ptr<Request>> handler) => func((_, panic, __) =>
        {
            ref ServeMux mux = ref _addr_mux.val;

            if (handler == null)
            {
                panic("http: nil handler");
            }

            mux.Handle(pattern, HandlerFunc(handler));

        });

        // Handle registers the handler for the given pattern
        // in the DefaultServeMux.
        // The documentation for ServeMux explains how patterns are matched.
        public static void Handle(@string pattern, Handler handler)
        {
            DefaultServeMux.Handle(pattern, handler);
        }

        // HandleFunc registers the handler function for the given pattern
        // in the DefaultServeMux.
        // The documentation for ServeMux explains how patterns are matched.
        public static void HandleFunc(@string pattern, Action<ResponseWriter, ptr<Request>> handler)
        {
            DefaultServeMux.HandleFunc(pattern, handler);
        }

        // Serve accepts incoming HTTP connections on the listener l,
        // creating a new service goroutine for each. The service goroutines
        // read requests and then call handler to reply to them.
        //
        // The handler is typically nil, in which case the DefaultServeMux is used.
        //
        // HTTP/2 support is only enabled if the Listener returns *tls.Conn
        // connections and they were configured with "h2" in the TLS
        // Config.NextProtos.
        //
        // Serve always returns a non-nil error.
        public static error Serve(net.Listener l, Handler handler)
        {
            ptr<Server> srv = addr(new Server(Handler:handler));
            return error.As(srv.Serve(l))!;
        }

        // ServeTLS accepts incoming HTTPS connections on the listener l,
        // creating a new service goroutine for each. The service goroutines
        // read requests and then call handler to reply to them.
        //
        // The handler is typically nil, in which case the DefaultServeMux is used.
        //
        // Additionally, files containing a certificate and matching private key
        // for the server must be provided. If the certificate is signed by a
        // certificate authority, the certFile should be the concatenation
        // of the server's certificate, any intermediates, and the CA's certificate.
        //
        // ServeTLS always returns a non-nil error.
        public static error ServeTLS(net.Listener l, Handler handler, @string certFile, @string keyFile)
        {
            ptr<Server> srv = addr(new Server(Handler:handler));
            return error.As(srv.ServeTLS(l, certFile, keyFile))!;
        }

        // A Server defines parameters for running an HTTP server.
        // The zero value for Server is a valid configuration.
        public partial struct Server
        {
            public @string Addr;
            public Handler Handler; // handler to invoke, http.DefaultServeMux if nil

// TLSConfig optionally provides a TLS configuration for use
// by ServeTLS and ListenAndServeTLS. Note that this value is
// cloned by ServeTLS and ListenAndServeTLS, so it's not
// possible to modify the configuration with methods like
// tls.Config.SetSessionTicketKeys. To use
// SetSessionTicketKeys, use Server.Serve with a TLS Listener
// instead.
            public ptr<tls.Config> TLSConfig; // ReadTimeout is the maximum duration for reading the entire
// request, including the body.
//
// Because ReadTimeout does not let Handlers make per-request
// decisions on each request body's acceptable deadline or
// upload rate, most users will prefer to use
// ReadHeaderTimeout. It is valid to use them both.
            public time.Duration ReadTimeout; // ReadHeaderTimeout is the amount of time allowed to read
// request headers. The connection's read deadline is reset
// after reading the headers and the Handler can decide what
// is considered too slow for the body. If ReadHeaderTimeout
// is zero, the value of ReadTimeout is used. If both are
// zero, there is no timeout.
            public time.Duration ReadHeaderTimeout; // WriteTimeout is the maximum duration before timing out
// writes of the response. It is reset whenever a new
// request's header is read. Like ReadTimeout, it does not
// let Handlers make decisions on a per-request basis.
            public time.Duration WriteTimeout; // IdleTimeout is the maximum amount of time to wait for the
// next request when keep-alives are enabled. If IdleTimeout
// is zero, the value of ReadTimeout is used. If both are
// zero, there is no timeout.
            public time.Duration IdleTimeout; // MaxHeaderBytes controls the maximum number of bytes the
// server will read parsing the request header's keys and
// values, including the request line. It does not limit the
// size of the request body.
// If zero, DefaultMaxHeaderBytes is used.
            public long MaxHeaderBytes; // TLSNextProto optionally specifies a function to take over
// ownership of the provided TLS connection when an ALPN
// protocol upgrade has occurred. The map key is the protocol
// name negotiated. The Handler argument should be used to
// handle HTTP requests and will initialize the Request's TLS
// and RemoteAddr if not already set. The connection is
// automatically closed when the function returns.
// If TLSNextProto is not nil, HTTP/2 support is not enabled
// automatically.
            public map<@string, Action<ptr<Server>, ptr<tls.Conn>, Handler>> TLSNextProto; // ConnState specifies an optional callback function that is
// called when a client connection changes state. See the
// ConnState type and associated constants for details.
            public Action<net.Conn, ConnState> ConnState; // ErrorLog specifies an optional logger for errors accepting
// connections, unexpected behavior from handlers, and
// underlying FileSystem errors.
// If nil, logging is done via the log package's standard logger.
            public ptr<log.Logger> ErrorLog; // BaseContext optionally specifies a function that returns
// the base context for incoming requests on this server.
// The provided Listener is the specific Listener that's
// about to start accepting requests.
// If BaseContext is nil, the default is context.Background().
// If non-nil, it must return a non-nil context.
            public Func<net.Listener, context.Context> BaseContext; // ConnContext optionally specifies a function that modifies
// the context used for a new connection c. The provided ctx
// is derived from the base context and has a ServerContextKey
// value.
            public Func<context.Context, net.Conn, context.Context> ConnContext;
            public atomicBool inShutdown; // true when when server is in shutdown

            public int disableKeepAlives; // accessed atomically.
            public sync.Once nextProtoOnce; // guards setupHTTP2_* init
            public error nextProtoErr; // result of http2.ConfigureServer if used

            public sync.Mutex mu;
            public channel<object> doneChan;
            public slice<Action> onShutdown;
        }

        private static channel<object> getDoneChan(this ptr<Server> _addr_s) => func((defer, _, __) =>
        {
            ref Server s = ref _addr_s.val;

            s.mu.Lock();
            defer(s.mu.Unlock());
            return s.getDoneChanLocked();
        });

        private static channel<object> getDoneChanLocked(this ptr<Server> _addr_s)
        {
            ref Server s = ref _addr_s.val;

            if (s.doneChan == null)
            {
                s.doneChan = make_channel<object>();
            }

            return s.doneChan;

        }

        private static void closeDoneChanLocked(this ptr<Server> _addr_s)
        {
            ref Server s = ref _addr_s.val;

            var ch = s.getDoneChanLocked();
            close(ch);
        }

        // Close immediately closes all active net.Listeners and any
        // connections in state StateNew, StateActive, or StateIdle. For a
        // graceful shutdown, use Shutdown.
        //
        // Close does not attempt to close (and does not even know about)
        // any hijacked connections, such as WebSockets.
        //
        // Close returns any error returned from closing the Server's
        // underlying Listener(s).
        private static error Close(this ptr<Server> _addr_srv) => func((defer, _, __) =>
        {
            ref Server srv = ref _addr_srv.val;

            srv.inShutdown.setTrue();
            srv.mu.Lock();
            defer(srv.mu.Unlock());
            srv.closeDoneChanLocked();
            var err = srv.closeListenersLocked();
            foreach (var (c) in srv.activeConn)
            {
                c.rwc.Close();
                delete(srv.activeConn, c);
            }
            return error.As(err)!;

        });

        // shutdownPollInterval is how often we poll for quiescence
        // during Server.Shutdown. This is lower during tests, to
        // speed up tests.
        // Ideally we could find a solution that doesn't involve polling,
        // but which also doesn't have a high runtime cost (and doesn't
        // involve any contentious mutexes), but that is left as an
        // exercise for the reader.
        private static long shutdownPollInterval = 500L * time.Millisecond;

        // Shutdown gracefully shuts down the server without interrupting any
        // active connections. Shutdown works by first closing all open
        // listeners, then closing all idle connections, and then waiting
        // indefinitely for connections to return to idle and then shut down.
        // If the provided context expires before the shutdown is complete,
        // Shutdown returns the context's error, otherwise it returns any
        // error returned from closing the Server's underlying Listener(s).
        //
        // When Shutdown is called, Serve, ListenAndServe, and
        // ListenAndServeTLS immediately return ErrServerClosed. Make sure the
        // program doesn't exit and waits instead for Shutdown to return.
        //
        // Shutdown does not attempt to close nor wait for hijacked
        // connections such as WebSockets. The caller of Shutdown should
        // separately notify such long-lived connections of shutdown and wait
        // for them to close, if desired. See RegisterOnShutdown for a way to
        // register shutdown notification functions.
        //
        // Once Shutdown has been called on a server, it may not be reused;
        // future calls to methods such as Serve will return ErrServerClosed.
        private static error Shutdown(this ptr<Server> _addr_srv, context.Context ctx) => func((defer, _, __) =>
        {
            ref Server srv = ref _addr_srv.val;

            srv.inShutdown.setTrue();

            srv.mu.Lock();
            var lnerr = srv.closeListenersLocked();
            srv.closeDoneChanLocked();
            foreach (var (_, f) in srv.onShutdown)
            {
                go_(() => f());
            }
            srv.mu.Unlock();

            var ticker = time.NewTicker(shutdownPollInterval);
            defer(ticker.Stop());
            while (true)
            {
                if (srv.closeIdleConns() && srv.numListeners() == 0L)
                {
                    return error.As(lnerr)!;
                }

                return error.As(ctx.Err())!;

            }


        });

        // RegisterOnShutdown registers a function to call on Shutdown.
        // This can be used to gracefully shutdown connections that have
        // undergone ALPN protocol upgrade or that have been hijacked.
        // This function should start protocol-specific graceful shutdown,
        // but should not wait for shutdown to complete.
        private static void RegisterOnShutdown(this ptr<Server> _addr_srv, Action f)
        {
            ref Server srv = ref _addr_srv.val;

            srv.mu.Lock();
            srv.onShutdown = append(srv.onShutdown, f);
            srv.mu.Unlock();
        }

        private static long numListeners(this ptr<Server> _addr_s) => func((defer, _, __) =>
        {
            ref Server s = ref _addr_s.val;

            s.mu.Lock();
            defer(s.mu.Unlock());
            return len(s.listeners);
        });

        // closeIdleConns closes all idle connections and reports whether the
        // server is quiescent.
        private static bool closeIdleConns(this ptr<Server> _addr_s) => func((defer, _, __) =>
        {
            ref Server s = ref _addr_s.val;

            s.mu.Lock();
            defer(s.mu.Unlock());
            var quiescent = true;
            foreach (var (c) in s.activeConn)
            {
                var (st, unixSec) = c.getState(); 
                // Issue 22682: treat StateNew connections as if
                // they're idle if we haven't read the first request's
                // header in over 5 seconds.
                if (st == StateNew && unixSec < time.Now().Unix() - 5L)
                {
                    st = StateIdle;
                }

                if (st != StateIdle || unixSec == 0L)
                { 
                    // Assume unixSec == 0 means it's a very new
                    // connection, without state set yet.
                    quiescent = false;
                    continue;

                }

                c.rwc.Close();
                delete(s.activeConn, c);

            }
            return quiescent;

        });

        private static error closeListenersLocked(this ptr<Server> _addr_s)
        {
            ref Server s = ref _addr_s.val;

            error err = default!;
            foreach (var (ln) in s.listeners)
            {
                {
                    object cerr = ptr<ln>();

                    if (cerr != null && err == null)
                    {
                        err = error.As(cerr)!;
                    }

                }

            }
            return error.As(err)!;

        }

        // A ConnState represents the state of a client connection to a server.
        // It's used by the optional Server.ConnState hook.
        public partial struct ConnState // : long
        {
        }

 
        // StateNew represents a new connection that is expected to
        // send a request immediately. Connections begin at this
        // state and then transition to either StateActive or
        // StateClosed.
        public static readonly ConnState StateNew = (ConnState)iota; 

        // StateActive represents a connection that has read 1 or more
        // bytes of a request. The Server.ConnState hook for
        // StateActive fires before the request has entered a handler
        // and doesn't fire again until the request has been
        // handled. After the request is handled, the state
        // transitions to StateClosed, StateHijacked, or StateIdle.
        // For HTTP/2, StateActive fires on the transition from zero
        // to one active request, and only transitions away once all
        // active requests are complete. That means that ConnState
        // cannot be used to do per-request work; ConnState only notes
        // the overall state of the connection.
        public static readonly var StateActive = 0; 

        // StateIdle represents a connection that has finished
        // handling a request and is in the keep-alive state, waiting
        // for a new request. Connections transition from StateIdle
        // to either StateActive or StateClosed.
        public static readonly var StateIdle = 1; 

        // StateHijacked represents a hijacked connection.
        // This is a terminal state. It does not transition to StateClosed.
        public static readonly var StateHijacked = 2; 

        // StateClosed represents a closed connection.
        // This is a terminal state. Hijacked connections do not
        // transition to StateClosed.
        public static readonly var StateClosed = 3;


        private static map stateName = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ConnState, @string>{StateNew:"new",StateActive:"active",StateIdle:"idle",StateHijacked:"hijacked",StateClosed:"closed",};

        public static @string String(this ConnState c)
        {
            return stateName[c];
        }

        // serverHandler delegates to either the server's Handler or
        // DefaultServeMux and also handles "OPTIONS *" requests.
        private partial struct serverHandler
        {
            public ptr<Server> srv;
        }

        private static void ServeHTTP(this serverHandler sh, ResponseWriter rw, ptr<Request> _addr_req)
        {
            ref Request req = ref _addr_req.val;

            var handler = sh.srv.Handler;
            if (handler == null)
            {
                handler = DefaultServeMux;
            }

            if (req.RequestURI == "*" && req.Method == "OPTIONS")
            {
                handler = new globalOptionsHandler();
            }

            handler.ServeHTTP(rw, req);

        }

        // ListenAndServe listens on the TCP network address srv.Addr and then
        // calls Serve to handle requests on incoming connections.
        // Accepted connections are configured to enable TCP keep-alives.
        //
        // If srv.Addr is blank, ":http" is used.
        //
        // ListenAndServe always returns a non-nil error. After Shutdown or Close,
        // the returned error is ErrServerClosed.
        private static error ListenAndServe(this ptr<Server> _addr_srv)
        {
            ref Server srv = ref _addr_srv.val;

            if (srv.shuttingDown())
            {
                return error.As(ErrServerClosed)!;
            }

            var addr = srv.Addr;
            if (addr == "")
            {
                addr = ":http";
            }

            var (ln, err) = net.Listen("tcp", addr);
            if (err != null)
            {
                return error.As(err)!;
            }

            return error.As(srv.Serve(ln))!;

        }

        private static Action<ptr<Server>, net.Listener> testHookServerServe = default; // used if non-nil

        // shouldDoServeHTTP2 reports whether Server.Serve should configure
        // automatic HTTP/2. (which sets up the srv.TLSNextProto map)
        private static bool shouldConfigureHTTP2ForServe(this ptr<Server> _addr_srv)
        {
            ref Server srv = ref _addr_srv.val;

            if (srv.TLSConfig == null)
            { 
                // Compatibility with Go 1.6:
                // If there's no TLSConfig, it's possible that the user just
                // didn't set it on the http.Server, but did pass it to
                // tls.NewListener and passed that listener to Serve.
                // So we should configure HTTP/2 (to set up srv.TLSNextProto)
                // in case the listener returns an "h2" *tls.Conn.
                return true;

            } 
            // The user specified a TLSConfig on their http.Server.
            // In this, case, only configure HTTP/2 if their tls.Config
            // explicitly mentions "h2". Otherwise http2.ConfigureServer
            // would modify the tls.Config to add it, but they probably already
            // passed this tls.Config to tls.NewListener. And if they did,
            // it's too late anyway to fix it. It would only be potentially racy.
            // See Issue 15908.
            return strSliceContains(srv.TLSConfig.NextProtos, http2NextProtoTLS);

        }

        // ErrServerClosed is returned by the Server's Serve, ServeTLS, ListenAndServe,
        // and ListenAndServeTLS methods after a call to Shutdown or Close.
        public static var ErrServerClosed = errors.New("http: Server closed");

        // Serve accepts incoming connections on the Listener l, creating a
        // new service goroutine for each. The service goroutines read requests and
        // then call srv.Handler to reply to them.
        //
        // HTTP/2 support is only enabled if the Listener returns *tls.Conn
        // connections and they were configured with "h2" in the TLS
        // Config.NextProtos.
        //
        // Serve always returns a non-nil error and closes l.
        // After Shutdown or Close, the returned error is ErrServerClosed.
        private static error Serve(this ptr<Server> _addr_srv, net.Listener l) => func((defer, panic, _) =>
        {
            ref Server srv = ref _addr_srv.val;

            {
                var fn = testHookServerServe;

                if (fn != null)
                {
                    fn(srv, l); // call hook with unwrapped listener
                }

            }


            var origListener = l;
            l = addr(new onceCloseListener(Listener:l));
            defer(l.Close());

            {
                var err = srv.setupHTTP2_Serve();

                if (err != null)
                {
                    return error.As(err)!;
                }

            }


            if (!srv.trackListener(_addr_l, true))
            {
                return error.As(ErrServerClosed)!;
            }

            defer(srv.trackListener(_addr_l, false));

            var baseCtx = context.Background();
            if (srv.BaseContext != null)
            {
                baseCtx = srv.BaseContext(origListener);
                if (baseCtx == null)
                {
                    panic("BaseContext returned a nil context");
                }

            }

            time.Duration tempDelay = default; // how long to sleep on accept failure

            var ctx = context.WithValue(baseCtx, ServerContextKey, srv);
            while (true)
            {
                var (rw, err) = l.Accept();
                if (err != null)
                {
                    return error.As(ErrServerClosed)!;
                    {
                        net.Error (ne, ok) = err._<net.Error>();

                        if (ok && ne.Temporary())
                        {
                            if (tempDelay == 0L)
                            {
                                tempDelay = 5L * time.Millisecond;
                            }
                            else
                            {
                                tempDelay *= 2L;
                            }

                            {
                                long max = 1L * time.Second;

                                if (tempDelay > max)
                                {
                                    tempDelay = max;
                                }

                            }

                            srv.logf("http: Accept error: %v; retrying in %v", err, tempDelay);
                            time.Sleep(tempDelay);
                            continue;

                        }

                    }

                    return error.As(err)!;

                }

                var connCtx = ctx;
                {
                    var cc = srv.ConnContext;

                    if (cc != null)
                    {
                        connCtx = cc(connCtx, rw);
                        if (connCtx == null)
                        {
                            panic("ConnContext returned nil");
                        }

                    }

                }

                tempDelay = 0L;
                var c = srv.newConn(rw);
                c.setState(c.rwc, StateNew); // before Serve can return
                go_(() => c.serve(connCtx));

            }


        });

        // ServeTLS accepts incoming connections on the Listener l, creating a
        // new service goroutine for each. The service goroutines perform TLS
        // setup and then read requests, calling srv.Handler to reply to them.
        //
        // Files containing a certificate and matching private key for the
        // server must be provided if neither the Server's
        // TLSConfig.Certificates nor TLSConfig.GetCertificate are populated.
        // If the certificate is signed by a certificate authority, the
        // certFile should be the concatenation of the server's certificate,
        // any intermediates, and the CA's certificate.
        //
        // ServeTLS always returns a non-nil error. After Shutdown or Close, the
        // returned error is ErrServerClosed.
        private static error ServeTLS(this ptr<Server> _addr_srv, net.Listener l, @string certFile, @string keyFile)
        {
            ref Server srv = ref _addr_srv.val;
 
            // Setup HTTP/2 before srv.Serve, to initialize srv.TLSConfig
            // before we clone it and create the TLS Listener.
            {
                var err__prev1 = err;

                var err = srv.setupHTTP2_ServeTLS();

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }


            var config = cloneTLSConfig(srv.TLSConfig);
            if (!strSliceContains(config.NextProtos, "http/1.1"))
            {
                config.NextProtos = append(config.NextProtos, "http/1.1");
            }

            var configHasCert = len(config.Certificates) > 0L || config.GetCertificate != null;
            if (!configHasCert || certFile != "" || keyFile != "")
            {
                err = default!;
                config.Certificates = make_slice<tls.Certificate>(1L);
                config.Certificates[0L], err = tls.LoadX509KeyPair(certFile, keyFile);
                if (err != null)
                {
                    return error.As(err)!;
                }

            }

            var tlsListener = tls.NewListener(l, config);
            return error.As(srv.Serve(tlsListener))!;

        }

        // trackListener adds or removes a net.Listener to the set of tracked
        // listeners.
        //
        // We store a pointer to interface in the map set, in case the
        // net.Listener is not comparable. This is safe because we only call
        // trackListener via Serve and can track+defer untrack the same
        // pointer to local variable there. We never need to compare a
        // Listener from another caller.
        //
        // It reports whether the server is still up (not Shutdown or Closed).
        private static bool trackListener(this ptr<Server> _addr_s, ptr<net.Listener> _addr_ln, bool add) => func((defer, _, __) =>
        {
            ref Server s = ref _addr_s.val;
            ref net.Listener ln = ref _addr_ln.val;

            s.mu.Lock();
            defer(s.mu.Unlock());
            if (s.listeners == null)
            {
                s.listeners = make();
            }

            if (add)
            {
                if (s.shuttingDown())
                {
                    return false;
                }

                s.listeners[ln] = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{};

            }
            else
            {
                delete(s.listeners, ln);
            }

            return true;

        });

        private static void trackConn(this ptr<Server> _addr_s, ptr<conn> _addr_c, bool add) => func((defer, _, __) =>
        {
            ref Server s = ref _addr_s.val;
            ref conn c = ref _addr_c.val;

            s.mu.Lock();
            defer(s.mu.Unlock());
            if (s.activeConn == null)
            {
                s.activeConn = make();
            }

            if (add)
            {
                s.activeConn[c] = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{};
            }
            else
            {
                delete(s.activeConn, c);
            }

        });

        private static time.Duration idleTimeout(this ptr<Server> _addr_s)
        {
            ref Server s = ref _addr_s.val;

            if (s.IdleTimeout != 0L)
            {
                return s.IdleTimeout;
            }

            return s.ReadTimeout;

        }

        private static time.Duration readHeaderTimeout(this ptr<Server> _addr_s)
        {
            ref Server s = ref _addr_s.val;

            if (s.ReadHeaderTimeout != 0L)
            {
                return s.ReadHeaderTimeout;
            }

            return s.ReadTimeout;

        }

        private static bool doKeepAlives(this ptr<Server> _addr_s)
        {
            ref Server s = ref _addr_s.val;

            return atomic.LoadInt32(_addr_s.disableKeepAlives) == 0L && !s.shuttingDown();
        }

        private static bool shuttingDown(this ptr<Server> _addr_s)
        {
            ref Server s = ref _addr_s.val;

            return s.inShutdown.isSet();
        }

        // SetKeepAlivesEnabled controls whether HTTP keep-alives are enabled.
        // By default, keep-alives are always enabled. Only very
        // resource-constrained environments or servers in the process of
        // shutting down should disable them.
        private static void SetKeepAlivesEnabled(this ptr<Server> _addr_srv, bool v)
        {
            ref Server srv = ref _addr_srv.val;

            if (v)
            {
                atomic.StoreInt32(_addr_srv.disableKeepAlives, 0L);
                return ;
            }

            atomic.StoreInt32(_addr_srv.disableKeepAlives, 1L); 

            // Close idle HTTP/1 conns:
            srv.closeIdleConns(); 

            // TODO: Issue 26303: close HTTP/2 conns as soon as they become idle.
        }

        private static void logf(this ptr<Server> _addr_s, @string format, params object[] args)
        {
            args = args.Clone();
            ref Server s = ref _addr_s.val;

            if (s.ErrorLog != null)
            {
                s.ErrorLog.Printf(format, args);
            }
            else
            {
                log.Printf(format, args);
            }

        }

        // logf prints to the ErrorLog of the *Server associated with request r
        // via ServerContextKey. If there's no associated server, or if ErrorLog
        // is nil, logging is done via the log package's standard logger.
        private static void logf(ptr<Request> _addr_r, @string format, params object[] args)
        {
            args = args.Clone();
            ref Request r = ref _addr_r.val;

            ptr<Server> (s, _) = r.Context().Value(ServerContextKey)._<ptr<Server>>();
            if (s != null && s.ErrorLog != null)
            {
                s.ErrorLog.Printf(format, args);
            }
            else
            {
                log.Printf(format, args);
            }

        }

        // ListenAndServe listens on the TCP network address addr and then calls
        // Serve with handler to handle requests on incoming connections.
        // Accepted connections are configured to enable TCP keep-alives.
        //
        // The handler is typically nil, in which case the DefaultServeMux is used.
        //
        // ListenAndServe always returns a non-nil error.
        public static error ListenAndServe(@string addr, Handler handler)
        {
            ptr<Server> server = addr(new Server(Addr:addr,Handler:handler));
            return error.As(server.ListenAndServe())!;
        }

        // ListenAndServeTLS acts identically to ListenAndServe, except that it
        // expects HTTPS connections. Additionally, files containing a certificate and
        // matching private key for the server must be provided. If the certificate
        // is signed by a certificate authority, the certFile should be the concatenation
        // of the server's certificate, any intermediates, and the CA's certificate.
        public static error ListenAndServeTLS(@string addr, @string certFile, @string keyFile, Handler handler)
        {
            ptr<Server> server = addr(new Server(Addr:addr,Handler:handler));
            return error.As(server.ListenAndServeTLS(certFile, keyFile))!;
        }

        // ListenAndServeTLS listens on the TCP network address srv.Addr and
        // then calls ServeTLS to handle requests on incoming TLS connections.
        // Accepted connections are configured to enable TCP keep-alives.
        //
        // Filenames containing a certificate and matching private key for the
        // server must be provided if neither the Server's TLSConfig.Certificates
        // nor TLSConfig.GetCertificate are populated. If the certificate is
        // signed by a certificate authority, the certFile should be the
        // concatenation of the server's certificate, any intermediates, and
        // the CA's certificate.
        //
        // If srv.Addr is blank, ":https" is used.
        //
        // ListenAndServeTLS always returns a non-nil error. After Shutdown or
        // Close, the returned error is ErrServerClosed.
        private static error ListenAndServeTLS(this ptr<Server> _addr_srv, @string certFile, @string keyFile) => func((defer, _, __) =>
        {
            ref Server srv = ref _addr_srv.val;

            if (srv.shuttingDown())
            {
                return error.As(ErrServerClosed)!;
            }

            var addr = srv.Addr;
            if (addr == "")
            {
                addr = ":https";
            }

            var (ln, err) = net.Listen("tcp", addr);
            if (err != null)
            {
                return error.As(err)!;
            }

            defer(ln.Close());

            return error.As(srv.ServeTLS(ln, certFile, keyFile))!;

        });

        // setupHTTP2_ServeTLS conditionally configures HTTP/2 on
        // srv and reports whether there was an error setting it up. If it is
        // not configured for policy reasons, nil is returned.
        private static error setupHTTP2_ServeTLS(this ptr<Server> _addr_srv)
        {
            ref Server srv = ref _addr_srv.val;

            srv.nextProtoOnce.Do(srv.onceSetNextProtoDefaults);
            return error.As(srv.nextProtoErr)!;
        }

        // setupHTTP2_Serve is called from (*Server).Serve and conditionally
        // configures HTTP/2 on srv using a more conservative policy than
        // setupHTTP2_ServeTLS because Serve is called after tls.Listen,
        // and may be called concurrently. See shouldConfigureHTTP2ForServe.
        //
        // The tests named TestTransportAutomaticHTTP2* and
        // TestConcurrentServerServe in server_test.go demonstrate some
        // of the supported use cases and motivations.
        private static error setupHTTP2_Serve(this ptr<Server> _addr_srv)
        {
            ref Server srv = ref _addr_srv.val;

            srv.nextProtoOnce.Do(srv.onceSetNextProtoDefaults_Serve);
            return error.As(srv.nextProtoErr)!;
        }

        private static void onceSetNextProtoDefaults_Serve(this ptr<Server> _addr_srv)
        {
            ref Server srv = ref _addr_srv.val;

            if (srv.shouldConfigureHTTP2ForServe())
            {
                srv.onceSetNextProtoDefaults();
            }

        }

        // onceSetNextProtoDefaults configures HTTP/2, if the user hasn't
        // configured otherwise. (by setting srv.TLSNextProto non-nil)
        // It must only be called via srv.nextProtoOnce (use srv.setupHTTP2_*).
        private static void onceSetNextProtoDefaults(this ptr<Server> _addr_srv)
        {
            ref Server srv = ref _addr_srv.val;

            if (omitBundledHTTP2 || strings.Contains(os.Getenv("GODEBUG"), "http2server=0"))
            {
                return ;
            } 
            // Enable HTTP/2 by default if the user hasn't otherwise
            // configured their TLSNextProto map.
            if (srv.TLSNextProto == null)
            {
                ptr<http2Server> conf = addr(new http2Server(NewWriteScheduler:func()http2WriteScheduler{returnhttp2NewPriorityWriteScheduler(nil)},));
                srv.nextProtoErr = http2ConfigureServer(srv, conf);
            }

        }

        // TimeoutHandler returns a Handler that runs h with the given time limit.
        //
        // The new Handler calls h.ServeHTTP to handle each request, but if a
        // call runs for longer than its time limit, the handler responds with
        // a 503 Service Unavailable error and the given message in its body.
        // (If msg is empty, a suitable default message will be sent.)
        // After such a timeout, writes by h to its ResponseWriter will return
        // ErrHandlerTimeout.
        //
        // TimeoutHandler supports the Pusher interface but does not support
        // the Hijacker or Flusher interfaces.
        public static Handler TimeoutHandler(Handler h, time.Duration dt, @string msg)
        {
            return addr(new timeoutHandler(handler:h,body:msg,dt:dt,));
        }

        // ErrHandlerTimeout is returned on ResponseWriter Write calls
        // in handlers which have timed out.
        public static var ErrHandlerTimeout = errors.New("http: Handler timeout");

        private partial struct timeoutHandler
        {
            public Handler handler;
            public @string body;
            public time.Duration dt; // When set, no context will be created and this context will
// be used instead.
            public context.Context testContext;
        }

        private static @string errorBody(this ptr<timeoutHandler> _addr_h)
        {
            ref timeoutHandler h = ref _addr_h.val;

            if (h.body != "")
            {
                return h.body;
            }

            return "<html><head><title>Timeout</title></head><body><h1>Timeout</h1></body></html>";

        }

        private static void ServeHTTP(this ptr<timeoutHandler> _addr_h, ResponseWriter w, ptr<Request> _addr_r) => func((defer, panic, _) =>
        {
            ref timeoutHandler h = ref _addr_h.val;
            ref Request r = ref _addr_r.val;

            var ctx = h.testContext;
            if (ctx == null)
            {
                context.CancelFunc cancelCtx = default;
                ctx, cancelCtx = context.WithTimeout(r.Context(), h.dt);
                defer(cancelCtx());
            }

            r = r.WithContext(ctx);
            var done = make_channel<object>();
            ptr<timeoutWriter> tw = addr(new timeoutWriter(w:w,h:make(Header),req:r,));
            var panicChan = make_channel<object>(1L);
            go_(() => () =>
            {
                defer(() =>
                {
                    {
                        var p__prev1 = p;

                        var p = recover();

                        if (p != null)
                        {
                            panicChan.Send(p);
                        }

                        p = p__prev1;

                    }

                }());
                h.handler.ServeHTTP(tw, r);
                close(done);

            }());
            panic(p);
            tw.mu.Lock();
            defer(tw.mu.Unlock());
            var dst = w.Header();
            foreach (var (k, vv) in tw.h)
            {
                dst[k] = vv;
            }
            if (!tw.wroteHeader)
            {
                tw.code = StatusOK;
            }

            w.WriteHeader(tw.code);
            w.Write(tw.wbuf.Bytes());
            tw.mu.Lock();
            defer(tw.mu.Unlock());
            w.WriteHeader(StatusServiceUnavailable);
            io.WriteString(w, h.errorBody());
            tw.timedOut = true;

        });

        private partial struct timeoutWriter
        {
            public ResponseWriter w;
            public Header h;
            public bytes.Buffer wbuf;
            public ptr<Request> req;
            public sync.Mutex mu;
            public bool timedOut;
            public bool wroteHeader;
            public long code;
        }

        private static Pusher _ = (timeoutWriter.val)(null);

        // Push implements the Pusher interface.
        private static error Push(this ptr<timeoutWriter> _addr_tw, @string target, ptr<PushOptions> _addr_opts)
        {
            ref timeoutWriter tw = ref _addr_tw.val;
            ref PushOptions opts = ref _addr_opts.val;

            {
                Pusher (pusher, ok) = tw.w._<Pusher>();

                if (ok)
                {
                    return error.As(pusher.Push(target, opts))!;
                }

            }

            return error.As(ErrNotSupported)!;

        }

        private static Header Header(this ptr<timeoutWriter> _addr_tw)
        {
            ref timeoutWriter tw = ref _addr_tw.val;

            return tw.h;
        }

        private static (long, error) Write(this ptr<timeoutWriter> _addr_tw, slice<byte> p) => func((defer, _, __) =>
        {
            long _p0 = default;
            error _p0 = default!;
            ref timeoutWriter tw = ref _addr_tw.val;

            tw.mu.Lock();
            defer(tw.mu.Unlock());
            if (tw.timedOut)
            {
                return (0L, error.As(ErrHandlerTimeout)!);
            }

            if (!tw.wroteHeader)
            {
                tw.writeHeaderLocked(StatusOK);
            }

            return tw.wbuf.Write(p);

        });

        private static void writeHeaderLocked(this ptr<timeoutWriter> _addr_tw, long code)
        {
            ref timeoutWriter tw = ref _addr_tw.val;

            checkWriteHeaderCode(code);


            if (tw.timedOut) 
                return ;
            else if (tw.wroteHeader) 
                if (tw.req != null)
                {
                    var caller = relevantCaller();
                    logf(_addr_tw.req, "http: superfluous response.WriteHeader call from %s (%s:%d)", caller.Function, path.Base(caller.File), caller.Line);
                }

            else 
                tw.wroteHeader = true;
                tw.code = code;
            
        }

        private static void WriteHeader(this ptr<timeoutWriter> _addr_tw, long code) => func((defer, _, __) =>
        {
            ref timeoutWriter tw = ref _addr_tw.val;

            tw.mu.Lock();
            defer(tw.mu.Unlock());
            tw.writeHeaderLocked(code);
        });

        // onceCloseListener wraps a net.Listener, protecting it from
        // multiple Close calls.
        private partial struct onceCloseListener : net.Listener
        {
            public ref net.Listener Listener => ref Listener_val;
            public sync.Once once;
            public error closeErr;
        }

        private static error Close(this ptr<onceCloseListener> _addr_oc)
        {
            ref onceCloseListener oc = ref _addr_oc.val;

            oc.once.Do(oc.close);
            return error.As(oc.closeErr)!;
        }

        private static void close(this ptr<onceCloseListener> _addr_oc)
        {
            ref onceCloseListener oc = ref _addr_oc.val;

            oc.closeErr = oc.Listener.Close();
        }

        // globalOptionsHandler responds to "OPTIONS *" requests.
        private partial struct globalOptionsHandler
        {
        }

        private static void ServeHTTP(this globalOptionsHandler _p0, ResponseWriter w, ptr<Request> _addr_r)
        {
            ref Request r = ref _addr_r.val;

            w.Header().Set("Content-Length", "0");
            if (r.ContentLength != 0L)
            { 
                // Read up to 4KB of OPTIONS body (as mentioned in the
                // spec as being reserved for future use), but anything
                // over that is considered a waste of server resources
                // (or an attack) and we abort and close the connection,
                // courtesy of MaxBytesReader's EOF behavior.
                var mb = MaxBytesReader(w, r.Body, 4L << (int)(10L));
                io.Copy(ioutil.Discard, mb);

            }

        }

        // initALPNRequest is an HTTP handler that initializes certain
        // uninitialized fields in its *Request. Such partially-initialized
        // Requests come from ALPN protocol handlers.
        private partial struct initALPNRequest
        {
            public context.Context ctx;
            public ptr<tls.Conn> c;
            public serverHandler h;
        }

        // BaseContext is an exported but unadvertised http.Handler method
        // recognized by x/net/http2 to pass down a context; the TLSNextProto
        // API predates context support so we shoehorn through the only
        // interface we have available.
        private static context.Context BaseContext(this initALPNRequest h)
        {
            return h.ctx;
        }

        private static void ServeHTTP(this initALPNRequest h, ResponseWriter rw, ptr<Request> _addr_req)
        {
            ref Request req = ref _addr_req.val;

            if (req.TLS == null)
            {
                req.TLS = addr(new tls.ConnectionState());
                req.TLS.val = h.c.ConnectionState();
            }

            if (req.Body == null)
            {
                req.Body = NoBody;
            }

            if (req.RemoteAddr == "")
            {
                req.RemoteAddr = h.c.RemoteAddr().String();
            }

            h.h.ServeHTTP(rw, req);

        }

        // loggingConn is used for debugging.
        private partial struct loggingConn : net.Conn
        {
            public @string name;
            public ref net.Conn Conn => ref Conn_val;
        }

        private static sync.Mutex uniqNameMu = default;        private static var uniqNameNext = make_map<@string, long>();

        private static net.Conn newLoggingConn(@string baseName, net.Conn c) => func((defer, _, __) =>
        {
            uniqNameMu.Lock();
            defer(uniqNameMu.Unlock());
            uniqNameNext[baseName]++;
            return addr(new loggingConn(name:fmt.Sprintf("%s-%d",baseName,uniqNameNext[baseName]),Conn:c,));
        });

        private static (long, error) Write(this ptr<loggingConn> _addr_c, slice<byte> p)
        {
            long n = default;
            error err = default!;
            ref loggingConn c = ref _addr_c.val;

            log.Printf("%s.Write(%d) = ....", c.name, len(p));
            n, err = c.Conn.Write(p);
            log.Printf("%s.Write(%d) = %d, %v", c.name, len(p), n, err);
            return ;
        }

        private static (long, error) Read(this ptr<loggingConn> _addr_c, slice<byte> p)
        {
            long n = default;
            error err = default!;
            ref loggingConn c = ref _addr_c.val;

            log.Printf("%s.Read(%d) = ....", c.name, len(p));
            n, err = c.Conn.Read(p);
            log.Printf("%s.Read(%d) = %d, %v", c.name, len(p), n, err);
            return ;
        }

        private static error Close(this ptr<loggingConn> _addr_c)
        {
            error err = default!;
            ref loggingConn c = ref _addr_c.val;

            log.Printf("%s.Close() = ...", c.name);
            err = c.Conn.Close();
            log.Printf("%s.Close() = %v", c.name, err);
            return ;
        }

        // checkConnErrorWriter writes to c.rwc and records any write errors to c.werr.
        // It only contains one field (and a pointer field at that), so it
        // fits in an interface value without an extra allocation.
        private partial struct checkConnErrorWriter
        {
            public ptr<conn> c;
        }

        private static (long, error) Write(this checkConnErrorWriter w, slice<byte> p)
        {
            long n = default;
            error err = default!;

            n, err = w.c.rwc.Write(p);
            if (err != null && w.c.werr == null)
            {
                w.c.werr = err;
                w.c.cancelCtx();
            }

            return ;

        }

        private static long numLeadingCRorLF(slice<byte> v)
        {
            long n = default;

            foreach (var (_, b) in v)
            {
                if (b == '\r' || b == '\n')
                {
                    n++;
                    continue;
                }

                break;

            }
            return ;


        }

        private static bool strSliceContains(slice<@string> ss, @string s)
        {
            foreach (var (_, v) in ss)
            {
                if (v == s)
                {
                    return true;
                }

            }
            return false;

        }

        // tlsRecordHeaderLooksLikeHTTP reports whether a TLS record header
        // looks like it might've been a misdirected plaintext HTTP request.
        private static bool tlsRecordHeaderLooksLikeHTTP(array<byte> hdr)
        {
            hdr = hdr.Clone();

            switch (string(hdr[..]))
            {
                case "GET /": 

                case "HEAD ": 

                case "POST ": 

                case "PUT /": 

                case "OPTIO": 
                    return true;
                    break;
            }
            return false;

        }
    }
}}
