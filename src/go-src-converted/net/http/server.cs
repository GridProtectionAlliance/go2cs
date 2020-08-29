// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// HTTP server. See RFC 2616.

// package http -- go2cs converted at 2020 August 29 08:33:42 UTC
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
using os = go.os_package;
using path = go.path_package;
using runtime = go.runtime_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using sync = go.sync_package;
using atomic = go.sync.atomic_package;
using time = go.time_package;

using httplex = go.golang_org.x.net.lex.httplex_package;
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
            void ServeHTTP(ResponseWriter _p0, ref Request _p0);
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
// DetectContentType.
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
            (net.Conn, ref bufio.ReadWriter, error) Hijack();
        }

        // The CloseNotifier interface is implemented by ResponseWriters which
        // allow detecting when the underlying connection has gone away.
        //
        // This mechanism can be used to cancel long operations on the server
        // if the client has disconnected before the response is ready.
        public partial interface CloseNotifier
        {
            channel<bool> CloseNotify();
        }

 
        // ServerContextKey is a context key. It can be used in HTTP
        // handlers with context.WithValue to access the server that
        // started the handler. The associated value will be of
        // type *Server.
        public static contextKey ServerContextKey = ref new contextKey("http-server");        public static contextKey LocalAddrContextKey = ref new contextKey("local-addr");

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

            public atomic.Value curState; // of ConnState

// mu guards hijackedv
            public sync.Mutex mu; // hijackedv is whether this connection has been hijacked
// by a Handler with the Hijacker interface.
// It is guarded by mu.
            public bool hijackedv;
        }

        private static bool hijacked(this ref conn _c) => func(_c, (ref conn c, Defer defer, Panic _, Recover __) =>
        {
            c.mu.Lock();
            defer(c.mu.Unlock());
            return c.hijackedv;
        });

        // c.mu must be held.
        private static (net.Conn, ref bufio.ReadWriter, error) hijackLocked(this ref conn c)
        {
            if (c.hijackedv)
            {
                return (null, null, ErrHijacked);
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
                        return (null, null, fmt.Errorf("unexpected Peek failure reading buffered byte: %v", err));
                    }

                }
            }
            c.setState(rwc, StateHijacked);
            return;
        }

        // This should be >= 512 bytes for DetectContentType,
        // but otherwise it's somewhat arbitrary.
        private static readonly long bufferBeforeChunkingSize = 2048L;

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
// at the time of res.WriteHeader, if res.WriteHeader is
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

        private static (long, error) Write(this ref chunkWriter cw, slice<byte> p)
        {
            if (!cw.wroteHeader)
            {
                cw.writeHeader(p);
            }
            if (cw.res.req.Method == "HEAD")
            { 
                // Eat writes.
                return (len(p), null);
            }
            if (cw.chunking)
            {
                _, err = fmt.Fprintf(cw.res.conn.bufw, "%x\r\n", len(p));
                if (err != null)
                {
                    cw.res.conn.rwc.Close();
                    return;
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
            return;
        }

        private static void flush(this ref chunkWriter cw)
        {
            if (!cw.wroteHeader)
            {
                cw.writeHeader(null);
            }
            cw.res.conn.bufw.Flush();
        }

        private static void close(this ref chunkWriter cw)
        {
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
        public static readonly @string TrailerPrefix = "Trailer:";

        // finalTrailers is called after the Handler exits and returns a non-nil
        // value if the Handler set any trailers.


        // finalTrailers is called after the Handler exits and returns a non-nil
        // value if the Handler set any trailers.
        private static Header finalTrailers(this ref response w)
        {
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

        private static bool isSet(this ref atomicBool b)
        {
            return atomic.LoadInt32((int32.Value)(b)) != 0L;
        }
        private static void setTrue(this ref atomicBool b)
        {
            atomic.StoreInt32((int32.Value)(b), 1L);

        }

        // declareTrailer is called for each Trailer header when the
        // response header is written. It notes that a header will need to be
        // written in the trailers at the end of the response.
        private static void declareTrailer(this ref response w, @string k)
        {
            k = CanonicalHeaderKey(k);
            switch (k)
            {
                case "Transfer-Encoding": 
                    // Forbidden by RFC 2616 14.40.

                case "Content-Length": 
                    // Forbidden by RFC 2616 14.40.

                case "Trailer": 
                    // Forbidden by RFC 2616 14.40.
                    return;
                    break;
            }
            w.trailers = append(w.trailers, k);
        }

        // requestTooLarge is called by maxBytesReader when too much input has
        // been read from the client.
        private static void requestTooLarge(this ref response w)
        {
            w.closeAfterReply = true;
            w.requestBodyLimitHit = true;
            if (!w.wroteHeader)
            {
                w.Header().Set("Connection", "close");
            }
        }

        // needsSniff reports whether a Content-Type still needs to be sniffed.
        private static bool needsSniff(this ref response w)
        {
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
            switch (src.type())
            {
                case ref os.File v:
                    var (fi, err) = v.Stat();
                    if (err != null)
                    {
                        return (false, err);
                    }
                    return (fi.Mode().IsRegular(), null);
                    break;
                case ref io.LimitedReader v:
                    return srcIsRegularFile(v.R);
                    break;
                default:
                {
                    var v = src.type();
                    return;
                    break;
                }
            }
        }

        // ReadFrom is here to optimize copying from an *os.File regular file
        // to a *net.TCPConn with sendfile.
        private static (long, error) ReadFrom(this ref response _w, io.Reader src) => func(_w, (ref response w, Defer defer, Panic _, Recover __) =>
        { 
            // Our underlying w.conn.rwc is usually a *TCPConn (with its
            // own ReadFrom method). If not, or if our src isn't a regular
            // file, just fall back to the normal copy method.
            io.ReaderFrom (rf, ok) = w.conn.rwc._<io.ReaderFrom>();
            var (regFile, err) = srcIsRegularFile(src);
            if (err != null)
            {
                return (0L, err);
            }
            if (!ok || !regFile)
            {
                ref slice<byte> bufp = copyBufPool.Get()._<ref slice<byte>>();
                defer(copyBufPool.Put(bufp));
                return io.CopyBuffer(new writerOnly(w), src, bufp.Value);
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
                    return (n, err);
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
                return (n, err);
            }
            (n0, err) = io.Copy(new writerOnly(w), src);
            n += n0;
            return (n, err);
        });

        // debugServerConnections controls whether all server connections are wrapped
        // with a verbose logging wrapper.
        private static readonly var debugServerConnections = false;

        // Create new connection from rwc.


        // Create new connection from rwc.
        private static ref conn newConn(this ref Server srv, net.Conn rwc)
        {
            conn c = ref new conn(server:srv,rwc:rwc,);
            if (debugServerConnections)
            {
                c.rwc = newLoggingConn("server", c.rwc);
            }
            return c;
        }

        private partial struct readResult
        {
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

        private static void @lock(this ref connReader cr)
        {
            cr.mu.Lock();
            if (cr.cond == null)
            {
                cr.cond = sync.NewCond(ref cr.mu);
            }
        }

        private static void unlock(this ref connReader cr)
        {
            cr.mu.Unlock();

        }

        private static void startBackgroundRead(this ref connReader _cr) => func(_cr, (ref connReader cr, Defer defer, Panic panic, Recover _) =>
        {
            cr.@lock();
            defer(cr.unlock());
            if (cr.inRead)
            {
                panic("invalid concurrent Body.Read call");
            }
            if (cr.hasByte)
            {
                return;
            }
            cr.inRead = true;
            cr.conn.rwc.SetReadDeadline(new time.Time());
            go_(() => cr.backgroundRead());
        });

        private static void backgroundRead(this ref connReader cr)
        {
            var (n, err) = cr.conn.rwc.Read(cr.byteBuf[..]);
            cr.@lock();
            if (n == 1L)
            {
                cr.hasByte = true; 
                // We were at EOF already (since we wouldn't be in a
                // background read otherwise), so this is a pipelined
                // HTTP request.
                cr.closeNotifyFromPipelinedRequest();
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

        private static void abortPendingRead(this ref connReader _cr) => func(_cr, (ref connReader cr, Defer defer, Panic _, Recover __) =>
        {
            cr.@lock();
            defer(cr.unlock());
            if (!cr.inRead)
            {
                return;
            }
            cr.aborted = true;
            cr.conn.rwc.SetReadDeadline(aLongTimeAgo);
            while (cr.inRead)
            {
                cr.cond.Wait();
            }

            cr.conn.rwc.SetReadDeadline(new time.Time());
        });

        private static void setReadLimit(this ref connReader cr, long remain)
        {
            cr.remain = remain;

        }
        private static void setInfiniteReadLimit(this ref connReader cr)
        {
            cr.remain = maxInt64;

        }
        private static bool hitReadLimit(this ref connReader cr)
        {
            return cr.remain <= 0L;
        }

        // may be called from multiple goroutines.
        private static void handleReadError(this ref connReader cr, error err)
        {
            cr.conn.cancelCtx();
            cr.closeNotify();
        }

        // closeNotifyFromPipelinedRequest simply calls closeNotify.
        //
        // This method wrapper is here for documentation. The callers are the
        // cases where we send on the closenotify channel because of a
        // pipelined HTTP request, per the previous Go behavior and
        // documentation (that this "MAY" happen).
        //
        // TODO: consider changing this behavior and making context
        // cancelation and closenotify work the same.
        private static void closeNotifyFromPipelinedRequest(this ref connReader cr)
        {
            cr.closeNotify();
        }

        // may be called from multiple goroutines.
        private static void closeNotify(this ref connReader cr)
        {
            ref response (res, _) = cr.conn.curReq.Load()._<ref response>();
            if (res != null)
            {
                if (atomic.CompareAndSwapInt32(ref res.didCloseNotify, 0L, 1L))
                {
                    res.closeNotifyCh.Send(true);
                }
            }
        }

        private static (long, error) Read(this ref connReader _cr, slice<byte> p) => func(_cr, (ref connReader cr, Defer _, Panic panic, Recover __) =>
        {
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
                return (0L, io.EOF);
            }
            if (len(p) == 0L)
            {
                cr.unlock();
                return (0L, null);
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
                return (1L, null);
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
            return (n, err);
        });

        private static sync.Pool bufioReaderPool = default;        private static sync.Pool bufioWriter2kPool = default;        private static sync.Pool bufioWriter4kPool = default;

        private static sync.Pool copyBufPool = new sync.Pool(New:func()interface{}{b:=make([]byte,32*1024)return&b},);

        private static ref sync.Pool bufioWriterPool(long size)
        {
            switch (size)
            {
                case 2L << (int)(10L): 
                    return ref bufioWriter2kPool;
                    break;
                case 4L << (int)(10L): 
                    return ref bufioWriter4kPool;
                    break;
            }
            return null;
        }

        private static ref bufio.Reader newBufioReader(io.Reader r)
        {
            {
                var v = bufioReaderPool.Get();

                if (v != null)
                {
                    ref bufio.Reader br = v._<ref bufio.Reader>();
                    br.Reset(r);
                    return br;
                } 
                // Note: if this reader size is ever changed, update
                // TestHandlerBodyClose's assumptions.

            } 
            // Note: if this reader size is ever changed, update
            // TestHandlerBodyClose's assumptions.
            return bufio.NewReader(r);
        }

        private static void putBufioReader(ref bufio.Reader br)
        {
            br.Reset(null);
            bufioReaderPool.Put(br);
        }

        private static ref bufio.Writer newBufioWriterSize(io.Writer w, long size)
        {
            var pool = bufioWriterPool(size);
            if (pool != null)
            {
                {
                    var v = pool.Get();

                    if (v != null)
                    {
                        ref bufio.Writer bw = v._<ref bufio.Writer>();
                        bw.Reset(w);
                        return bw;
                    }

                }
            }
            return bufio.NewWriterSize(w, size);
        }

        private static void putBufioWriter(ref bufio.Writer bw)
        {
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
        public static readonly long DefaultMaxHeaderBytes = 1L << (int)(20L); // 1 MB

 // 1 MB

        private static long maxHeaderBytes(this ref Server srv)
        {
            if (srv.MaxHeaderBytes > 0L)
            {
                return srv.MaxHeaderBytes;
            }
            return DefaultMaxHeaderBytes;
        }

        private static long initialReadLimitSize(this ref Server srv)
        {
            return int64(srv.maxHeaderBytes()) + 4096L; // bufio slop
        }

        // wrapper around io.ReadCloser which on first read, sends an
        // HTTP/1.1 100 Continue header
        private partial struct expectContinueReader
        {
            public ptr<response> resp;
            public io.ReadCloser readCloser;
            public bool closed;
            public bool sawEOF;
        }

        private static (long, error) Read(this ref expectContinueReader ecr, slice<byte> p)
        {
            if (ecr.closed)
            {
                return (0L, ErrBodyReadAfterClose);
            }
            if (!ecr.resp.wroteContinue && !ecr.resp.conn.hijacked())
            {
                ecr.resp.wroteContinue = true;
                ecr.resp.conn.bufw.WriteString("HTTP/1.1 100 Continue\r\n\r\n");
                ecr.resp.conn.bufw.Flush();
            }
            n, err = ecr.readCloser.Read(p);
            if (err == io.EOF)
            {
                ecr.sawEOF = true;
            }
            return;
        }

        private static error Close(this ref expectContinueReader ecr)
        {
            ecr.closed = true;
            return error.As(ecr.readCloser.Close());
        }

        // TimeFormat is the time format to use when generating times in HTTP
        // headers. It is like time.RFC1123 but hard-codes GMT as the time
        // zone. The time being formatted must be in UTC for Format to
        // generate the correct format.
        //
        // For parsing this time format, see ParseTime.
        public static readonly @string TimeFormat = "Mon, 02 Jan 2006 15:04:05 GMT";

        // appendTime is a non-allocating version of []byte(t.UTC().Format(TimeFormat))


        // appendTime is a non-allocating version of []byte(t.UTC().Format(TimeFormat))
        private static slice<byte> appendTime(slice<byte> b, time.Time t)
        {
            const @string days = "SunMonTueWedThuFriSat";

            const @string months = "JanFebMarAprMayJunJulAugSepOctNovDec";



            t = t.UTC();
            var (yy, mm, dd) = t.Date();
            var (hh, mn, ss) = t.Clock();
            var day = days[3L * t.Weekday()..];
            var mon = months[3L * (mm - 1L)..];

            return append(b, day[0L], day[1L], day[2L], ',', ' ', byte('0' + dd / 10L), byte('0' + dd % 10L), ' ', mon[0L], mon[1L], mon[2L], ' ', byte('0' + yy / 1000L), byte('0' + (yy / 100L) % 10L), byte('0' + (yy / 10L) % 10L), byte('0' + yy % 10L), ' ', byte('0' + hh / 10L), byte('0' + hh % 10L), ':', byte('0' + mn / 10L), byte('0' + mn % 10L), ':', byte('0' + ss / 10L), byte('0' + ss % 10L), ' ', 'G', 'M', 'T');
        }

        private static var errTooLarge = errors.New("http: request too large");

        // Read next request from connection.
        private static (ref response, error) readRequest(this ref conn _c, context.Context ctx) => func(_c, (ref conn c, Defer defer, Panic _, Recover __) =>
        {
            if (c.hijacked())
            {
                return (null, ErrHijacked);
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
                // RFC 2616 section 4.1 tolerance for old buggy clients.
                var (peek, _) = c.bufr.Peek(4L); // ReadRequest will get err below
                c.bufr.Discard(numLeadingCRorLF(peek));
            }
            var (req, err) = readRequest(c.bufr, keepHostHeader);
            if (err != null)
            {
                if (c.r.hitReadLimit())
                {
                    return (null, errTooLarge);
                }
                return (null, err);
            }
            if (!http1ServerSupportsRequest(req))
            {
                return (null, badRequestError("unsupported protocol version"));
            }
            c.lastMethod = req.Method;
            c.r.setInfiniteReadLimit();

            var (hosts, haveHost) = req.Header["Host"];
            var isH2Upgrade = req.isH2Upgrade();
            if (req.ProtoAtLeast(1L, 1L) && (!haveHost || len(hosts) == 0L) && !isH2Upgrade && req.Method != "CONNECT")
            {
                return (null, badRequestError("missing required Host header"));
            }
            if (len(hosts) > 1L)
            {
                return (null, badRequestError("too many Host headers"));
            }
            if (len(hosts) == 1L && !httplex.ValidHostHeader(hosts[0L]))
            {
                return (null, badRequestError("malformed Host header"));
            }
            foreach (var (k, vv) in req.Header)
            {
                if (!httplex.ValidHeaderFieldName(k))
                {
                    return (null, badRequestError("invalid header name"));
                }
                foreach (var (_, v) in vv)
                {
                    if (!httplex.ValidHeaderFieldValue(v))
                    {
                        return (null, badRequestError("invalid header value"));
                    }
                }
            }
            delete(req.Header, "Host");

            var (ctx, cancelCtx) = context.WithCancel(ctx);
            req.ctx = ctx;
            req.RemoteAddr = c.remoteAddr;
            req.TLS = c.tlsState;
            {
                ref body (body, ok) = req.Body._<ref body>();

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
            w = ref new response(conn:c,cancelCtx:cancelCtx,req:req,reqBody:req.Body,handlerHeader:make(Header),contentLength:-1,closeNotifyCh:make(chanbool,1),wants10KeepAlive:req.wantsHttp10KeepAlive(),wantsClose:req.wantsClose(),);
            if (isH2Upgrade)
            {
                w.closeAfterReply = true;
            }
            w.cw.res = w;
            w.w = newBufioWriterSize(ref w.cw, bufferBeforeChunkingSize);
            return (w, null);
        });

        // http1ServerSupportsRequest reports whether Go's HTTP/1.x server
        // supports the given request.
        private static bool http1ServerSupportsRequest(ref Request req)
        {
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

        private static Header Header(this ref response w)
        {
            if (w.cw.header == null && w.wroteHeader && !w.cw.wroteHeader)
            { 
                // Accessing the header between logically writing it
                // and physically writing it means we need to allocate
                // a clone to snapshot the logically written state.
                w.cw.header = w.handlerHeader.clone();
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
        private static readonly long maxPostHandlerReadBytes = 256L << (int)(10L);



        private static void checkWriteHeaderCode(long code) => func((_, panic, __) =>
        { 
            // Issue 22880: require valid WriteHeader status codes.
            // For now we only enforce that it's three digits.
            // In the future we might block things over 599 (600 and above aren't defined
            // at http://httpwg.org/specs/rfc7231.html#status.codes)
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

        private static void WriteHeader(this ref response w, long code)
        {
            if (w.conn.hijacked())
            {
                w.conn.server.logf("http: response.WriteHeader on hijacked connection");
                return;
            }
            if (w.wroteHeader)
            {
                w.conn.server.logf("http: multiple response.WriteHeader calls");
                return;
            }
            checkWriteHeaderCode(code);
            w.wroteHeader = true;
            w.status = code;

            if (w.calledHeader && w.cw.header == null)
            {
                w.cw.header = w.handlerHeader.clone();
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
        private static void Write(this extraHeader h, ref bufio.Writer w)
        {
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
        private static void writeHeader(this ref chunkWriter cw, slice<byte> p)
        {
            if (cw.wroteHeader)
            {
                return;
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
                    return;
                }
                {
                    var (_, ok) = header[key];

                    if (!ok)
                    {
                        return;
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
                ref expectContinueReader (ecr, ok) = w.req.Body._<ref expectContinueReader>();

                if (ok && !ecr.sawEOF)
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
                    case ref expectContinueReader bdy:
                        if (bdy.resp.wroteContinue)
                        {
                            discard = true;
                        }
                        break;
                    case ref body bdy:
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
                if (!haveType && !hasTE && len(p) > 0L)
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
            {
                (_, ok) = header["Date"];

                if (!ok)
                {
                    setHeader.date = appendTime(cw.res.dateBuf[..0L], time.Now());
                }

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
                return;
            }
            if (w.closeAfterReply && (!keepAlivesEnabled || !hasToken(cw.header.get("Connection"), "close")))
            {
                delHeader("Connection");
                if (w.req.ProtoAtLeast(1L, 1L))
                {
                    setHeader.connection = "close";
                }
            }
            writeStatusLine(w.conn.bufw, w.req.ProtoAtLeast(1L, 1L), code, w.statusBuf[..]);
            cw.header.WriteSubset(w.conn.bufw, excludeHeader);
            setHeader.Write(w.conn.bufw);
            w.conn.bufw.Write(crlf);
        }

        // foreachHeaderElement splits v according to the "#rule" construction
        // in RFC 2616 section 2.1 and calls fn for each non-empty element.
        private static void foreachHeaderElement(@string v, Action<@string> fn)
        {
            v = textproto.TrimString(v);
            if (v == "")
            {
                return;
            }
            if (!strings.Contains(v, ","))
            {
                fn(v);
                return;
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

        // writeStatusLine writes an HTTP/1.x Status-Line (RFC 2616 Section 6.1)
        // to bw. is11 is whether the HTTP request is HTTP/1.1. false means HTTP/1.0.
        // code is the response status code.
        // scratch is an optional scratch buffer. If it has at least capacity 3, it's used.
        private static void writeStatusLine(ref bufio.Writer bw, bool is11, long code, slice<byte> scratch)
        {
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
        private static bool bodyAllowed(this ref response _w) => func(_w, (ref response w, Defer _, Panic panic, Recover __) =>
        {
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
        private static (long, error) Write(this ref response w, slice<byte> data)
        {
            return w.write(len(data), data, "");
        }

        private static (long, error) WriteString(this ref response w, @string data)
        {
            return w.write(len(data), null, data);
        }

        // either dataB or dataS is non-zero.
        private static (long, error) write(this ref response w, long lenData, slice<byte> dataB, @string dataS)
        {
            if (w.conn.hijacked())
            {
                if (lenData > 0L)
                {
                    w.conn.server.logf("http: response.Write on hijacked connection");
                }
                return (0L, ErrHijacked);
            }
            if (!w.wroteHeader)
            {
                w.WriteHeader(StatusOK);
            }
            if (lenData == 0L)
            {
                return (0L, null);
            }
            if (!w.bodyAllowed())
            {
                return (0L, ErrBodyNotAllowed);
            }
            w.written += int64(lenData); // ignoring errors, for errorKludge
            if (w.contentLength != -1L && w.written > w.contentLength)
            {
                return (0L, ErrContentLength);
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

        private static void finishRequest(this ref response w)
        {
            w.handlerDone.setTrue();

            if (!w.wroteHeader)
            {
                w.WriteHeader(StatusOK);
            }
            w.w.Flush();
            putBufioWriter(w.w);
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
        private static bool shouldReuseConnection(this ref response w)
        {
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

        private static bool closedRequestBodyEarly(this ref response w)
        {
            ref body (body, ok) = w.req.Body._<ref body>();
            return ok && body.didEarlyClose();
        }

        private static void Flush(this ref response w)
        {
            if (!w.wroteHeader)
            {
                w.WriteHeader(StatusOK);
            }
            w.w.Flush();
            w.cw.flush();
        }

        private static void finalFlush(this ref conn c)
        {
            if (c.bufr != null)
            { 
                // Steal the bufio.Reader (~4KB worth of memory) and its associated
                // reader for a future connection.
                putBufioReader(c.bufr);
                c.bufr = null;
            }
            if (c.bufw != null)
            {
                c.bufw.Flush(); 
                // Steal the bufio.Writer (~4KB worth of memory) and its associated
                // writer for a future connection.
                putBufioWriter(c.bufw);
                c.bufw = null;
            }
        }

        // Close the connection.
        private static void close(this ref conn c)
        {
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
        private static readonly long rstAvoidanceDelay = 500L * time.Millisecond;



        private partial interface closeWriter
        {
            error CloseWrite();
        }

        private static closeWriter _ = closeWriter.As((net.TCPConn.Value)(null));

        // closeWrite flushes any outstanding data and sends a FIN packet (if
        // client is connected via TCP), signalling that we're done. We then
        // pause for a bit, hoping the client processes it before any
        // subsequent RST.
        //
        // See https://golang.org/issue/3595
        private static void closeWriteAndWait(this ref conn c)
        {
            c.finalFlush();
            {
                closeWriter (tcp, ok) = c.rwc._<closeWriter>();

                if (ok)
                {
                    tcp.CloseWrite();
                }

            }
            time.Sleep(rstAvoidanceDelay);
        }

        // validNPN reports whether the proto is not a blacklisted Next
        // Protocol Negotiation protocol. Empty and built-in protocol types
        // are blacklisted and can't be overridden with alternate
        // implementations.
        private static bool validNPN(@string proto)
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

        private static void setState(this ref conn c, net.Conn nc, ConnState state)
        {
            var srv = c.server;

            if (state == StateNew) 
                srv.trackConn(c, true);
            else if (state == StateHijacked || state == StateClosed) 
                srv.trackConn(c, false);
                        c.curState.Store(connStateInterface[state]);
            {
                var hook = srv.ConnState;

                if (hook != null)
                {
                    hook(nc, state);
                }

            }
        }

        // connStateInterface is an array of the interface{} versions of
        // ConnState values, so we can use them in atomic.Values later without
        // paying the cost of shoving their integers in an interface{}.


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
                ref net.OpError (oe, ok) = err._<ref net.OpError>();

                if (ok && oe.Op == "read")
                {
                    return true;
                }

            }
            return false;
        }

        // Serve a new connection.
        private static void serve(this ref conn _c, context.Context ctx) => func(_c, (ref conn c, Defer defer, Panic _, Recover __) =>
        {
            c.remoteAddr = c.rwc.RemoteAddr().String();
            ctx = context.WithValue(ctx, LocalAddrContextKey, c.rwc.LocalAddr());
            defer(() =>
            {
                {
                    var err__prev1 = err;

                    var err = recover();

                    if (err != null && err != ErrAbortHandler)
                    {
                        const long size = 64L << (int)(10L);

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
                ref tls.Conn (tlsConn, ok) = c.rwc._<ref tls.Conn>();

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
                            c.server.logf("http: TLS handshake error from %s: %v", c.rwc.RemoteAddr(), err);
                            return;
                        }

                        err = err__prev2;

                    }
                    c.tlsState = @new<tls.ConnectionState>();
                    c.tlsState.Value = tlsConn.ConnectionState();
                    {
                        var proto = c.tlsState.NegotiatedProtocol;

                        if (validNPN(proto))
                        {
                            {
                                var fn = c.server.TLSNextProto[proto];

                                if (fn != null)
                                {
                                    initNPNRequest h = new initNPNRequest(tlsConn,serverHandler{c.server});
                                    fn(c.server, tlsConn, h);
                                }

                            }
                            return;
                        }

                    }
                } 

                // HTTP/1.x from here on.

            } 

            // HTTP/1.x from here on.

            var (ctx, cancelCtx) = context.WithCancel(ctx);
            c.cancelCtx = cancelCtx;
            defer(cancelCtx());

            c.r = ref new connReader(conn:c);
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
                    const @string errorHeaders = "\r\nContent-Type: text/plain; charset=utf-8\r\nConnection: close\r\n\r\n";



                    if (err == errTooLarge)
                    { 
                        // Their HTTP client may or may not be
                        // able to read this if we're
                        // responding to them and hanging up
                        // while they're still writing their
                        // request. Undefined behavior.
                        const @string publicErr = "431 Request Header Fields Too Large";

                        fmt.Fprintf(c.rwc, "HTTP/1.1 " + publicErr + errorHeaders + publicErr);
                        c.closeWriteAndWait();
                        return;
                    }
                    if (isCommonNetReadError(err))
                    {
                        return; // don't reply
                    }
                    @string publicErr = "400 Bad Request";
                    {
                        badRequestError (v, ok) = err._<badRequestError>();

                        if (ok)
                        {
                            publicErr = publicErr + ": " + string(v);
                        }

                    }

                    fmt.Fprintf(c.rwc, "HTTP/1.1 " + publicErr + errorHeaders + publicErr);
                    return;
                } 

                // Expect 100 Continue support
                var req = w.req;
                if (req.expectsContinue())
                {
                    if (req.ProtoAtLeast(1L, 1L) && req.ContentLength != 0L)
                    { 
                        // Wrap the Body reader with one that replies on the connection
                        req.Body = ref new expectContinueReader(readCloser:req.Body,resp:w);
                    }
                }
                else if (req.Header.get("Expect") != "")
                {
                    w.sendExpectationFailed();
                    return;
                }
                c.curReq.Store(w);

                if (requestBodyRemains(req.Body))
                {
                    registerOnHitEOF(req.Body, w.conn.r.startBackgroundRead);
                }
                else
                {
                    if (w.conn.bufr.Buffered() > 0L)
                    {
                        w.conn.r.closeNotifyFromPipelinedRequest();
                    }
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
                    return;
                }
                w.finishRequest();
                if (!w.shouldReuseConnection())
                {
                    if (w.requestBodyLimitHit || w.closedRequestBodyEarly())
                    {
                        c.closeWriteAndWait();
                    }
                    return;
                }
                c.setState(c.rwc, StateIdle);
                c.curReq.Store((response.Value)(null));

                if (!w.conn.server.doKeepAlives())
                { 
                    // We're in shutdown mode. We might've replied
                    // to the user without "Connection: close" and
                    // they might think they can send another
                    // request, but such is life with HTTP/1.1.
                    return;
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
                                return;
                            }

                            err = err__prev2;

                        }
                    }

                    d = d__prev1;

                }
                c.rwc.SetReadDeadline(new time.Time());
            }

        });

        private static void sendExpectationFailed(this ref response w)
        { 
            // TODO(bradfitz): let ServeHTTP handlers handle
            // requests with non-standard expectation[s]? Seems
            // theoretical at best, and doesn't fit into the
            // current ServeHTTP model anyway. We'd need to
            // make the ResponseWriter an optional
            // "ExpectReplier" interface or something.
            //
            // For now we'll just obey RFC 2616 14.20 which says
            // "If a server receives a request containing an
            // Expect field that includes an expectation-
            // extension that it does not support, it MUST
            // respond with a 417 (Expectation Failed) status."
            w.Header().Set("Connection", "close");
            w.WriteHeader(StatusExpectationFailed);
            w.finishRequest();
        }

        // Hijack implements the Hijacker.Hijack method. Our response is both a ResponseWriter
        // and a Hijacker.
        private static (net.Conn, ref bufio.ReadWriter, error) Hijack(this ref response _w) => func(_w, (ref response w, Defer defer, Panic panic, Recover _) =>
        {
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
                putBufioWriter(w.w);
                w.w = null;
            }
            return (rwc, buf, err);
        });

        private static channel<bool> CloseNotify(this ref response _w) => func(_w, (ref response w, Defer _, Panic panic, Recover __) =>
        {
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
                case ref expectContinueReader v:
                    registerOnHitEOF(v.readCloser, fn);
                    break;
                case ref body v:
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
                case ref expectContinueReader v:
                    return requestBodyRemains(v.readCloser);
                    break;
                case ref body v:
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
        public delegate void HandlerFunc(ResponseWriter, ref Request);

        // ServeHTTP calls f(w, r).
        public static void ServeHTTP(this HandlerFunc f, ResponseWriter w, ref Request r)
        {
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
        public static void NotFound(ResponseWriter w, ref Request r)
        {
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
                        r2.Value = r.Value;
                        r2.URL = @new<url.URL>();
                        r2.URL.Value = r.URL.Value;
                        r2.URL.Path = p;
                        h.ServeHTTP(w, r2);
                    }
                    else
                    {
                        NotFound(w, r);
                    }

                }
            });
        }

        // Redirect replies to the request with a redirect to url,
        // which may be a path relative to the request path.
        //
        // The provided code should be in the 3xx range and is usually
        // StatusMovedPermanently, StatusFound or StatusSeeOther.
        public static void Redirect(ResponseWriter w, ref Request r, @string url, long code)
        { 
            // parseURL is just url.Parse (url is shadowed for godoc).
            {
                var (u, err) = parseURL(url);

                if (err == null)
                { 
                    // If url was relative, make absolute by
                    // combining with request path.
                    // The browser would probably do this for us,
                    // but doing it ourselves is more reliable.

                    // NOTE(rsc): RFC 2616 says that the Location
                    // line must be an absolute URI, like
                    // "http://www.google.com/redirect/",
                    // not a path like "/redirect/".
                    // Unfortunately, we don't know what to
                    // put in the host name section to get the
                    // client to connect to us again, so we can't
                    // know the right absolute URI to send back.
                    // Because of this problem, no one pays attention
                    // to the RFC; they all send back just a new path.
                    // So do we.
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

            w.Header().Set("Location", hexEscapeNonASCII(url));
            if (r.Method == "GET" || r.Method == "HEAD")
            {
                w.Header().Set("Content-Type", "text/html; charset=utf-8");
            }
            w.WriteHeader(code); 

            // RFC 2616 recommends that a short note "SHOULD" be included in the
            // response because older user agents may not understand 301/307.
            // Shouldn't send the response for POST or HEAD; that leaves GET.
            if (r.Method == "GET")
            {
                @string note = "<a href=\"" + htmlEscape(url) + "\">" + statusText[code] + "</a>.\n";
                fmt.Fprintln(w, note);
            }
        }

        // parseURL is just url.Parse. It exists only so that url.Parse can be called
        // in places where url is shadowed for godoc. See https://golang.org/cl/49930.
        private static var parseURL = url.Parse;

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

        private static void ServeHTTP(this ref redirectHandler rh, ResponseWriter w, ref Request r)
        {
            Redirect(w, r, rh.url, rh.code);
        }

        // RedirectHandler returns a request handler that redirects
        // each request it receives to the given url using the given
        // status code.
        //
        // The provided code should be in the 3xx range and is usually
        // StatusMovedPermanently, StatusFound or StatusSeeOther.
        public static Handler RedirectHandler(@string url, long code)
        {
            return ref new redirectHandler(url,code);
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
        // ServeMux also takes care of sanitizing the URL request path,
        // redirecting any request containing . or .. elements or repeated slashes
        // to an equivalent, cleaner URL.
        public partial struct ServeMux
        {
            public sync.RWMutex mu;
            public map<@string, muxEntry> m;
            public bool hosts; // whether any patterns contain hostnames
        }

        private partial struct muxEntry
        {
            public Handler h;
            public @string pattern;
        }

        // NewServeMux allocates and returns a new ServeMux.
        public static ref ServeMux NewServeMux()
        {
            return @new<ServeMux>();
        }

        // DefaultServeMux is the default ServeMux used by Serve.
        public static var DefaultServeMux = ref defaultServeMux;

        private static ServeMux defaultServeMux = default;

        // Does path match pattern?
        private static bool pathMatch(@string pattern, @string path)
        {
            if (len(pattern) == 0L)
            { 
                // should not happen
                return false;
            }
            var n = len(pattern);
            if (pattern[n - 1L] != '/')
            {
                return pattern == path;
            }
            return len(path) >= n && path[0L..n] == pattern;
        }

        // Return the canonical path for p, eliminating . and .. elements.
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
                np += "/";
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
        private static (Handler, @string) match(this ref ServeMux mux, @string path)
        { 
            // Check for exact match first.
            var (v, ok) = mux.m[path];
            if (ok)
            {
                return (v.h, v.pattern);
            } 

            // Check for longest valid match.
            long n = 0L;
            {
                var v__prev1 = v;

                foreach (var (__k, __v) in mux.m)
                {
                    k = __k;
                    v = __v;
                    if (!pathMatch(k, path))
                    {
                        continue;
                    }
                    if (h == null || len(k) > n)
                    {
                        n = len(k);
                        h = v.h;
                        pattern = v.pattern;
                    }
                }

                v = v__prev1;
            }

            return;
        }

        // redirectToPathSlash determines if the given path needs appending "/" to it.
        // This occurs when a handler for path + "/" was already registered, but
        // not for path itself. If the path needs appending to, it creates a new
        // URL, setting the path to u.Path + "/" and returning true to indicate so.
        private static (ref url.URL, bool) redirectToPathSlash(this ref ServeMux mux, @string host, @string path, ref url.URL u)
        {
            if (!mux.shouldRedirect(host, path))
            {
                return (u, false);
            }
            path = path + "/";
            u = ref new url.URL(Path:path,RawQuery:u.RawQuery);
            return (u, true);
        }

        // shouldRedirect reports whether the given path and host should be redirected to
        // path+"/". This should happen if a handler is registered for path+"/" but
        // not path -- see comments at ServeMux.
        private static bool shouldRedirect(this ref ServeMux mux, @string host, @string path)
        {
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
        private static (Handler, @string) Handler(this ref ServeMux mux, ref Request r)
        {
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
                var url = r.URL.Value;
                url.Path = path;
                return (RedirectHandler(url.String(), StatusMovedPermanently), pattern);
            }
            return mux.handler(host, r.URL.Path);
        }

        // handler is the main implementation of Handler.
        // The path is known to be in canonical form, except for CONNECT methods.
        private static (Handler, @string) handler(this ref ServeMux _mux, @string host, @string path) => func(_mux, (ref ServeMux mux, Defer defer, Panic _, Recover __) =>
        {
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
            return;
        });

        // ServeHTTP dispatches the request to the handler whose
        // pattern most closely matches the request URL.
        private static void ServeHTTP(this ref ServeMux mux, ResponseWriter w, ref Request r)
        {
            if (r.RequestURI == "*")
            {
                if (r.ProtoAtLeast(1L, 1L))
                {
                    w.Header().Set("Connection", "close");
                }
                w.WriteHeader(StatusBadRequest);
                return;
            }
            var (h, _) = mux.Handler(r);
            h.ServeHTTP(w, r);
        }

        // Handle registers the handler for the given pattern.
        // If a handler already exists for pattern, Handle panics.
        private static void Handle(this ref ServeMux _mux, @string pattern, Handler handler) => func(_mux, (ref ServeMux mux, Defer defer, Panic panic, Recover _) =>
        {
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
            mux.m[pattern] = new muxEntry(h:handler,pattern:pattern);

            if (pattern[0L] != '/')
            {
                mux.hosts = true;
            }
        });

        // HandleFunc registers the handler function for the given pattern.
        private static void HandleFunc(this ref ServeMux mux, @string pattern, Action<ResponseWriter, ref Request> handler)
        {
            mux.Handle(pattern, HandlerFunc(handler));
        }

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
        public static void HandleFunc(@string pattern, Action<ResponseWriter, ref Request> handler)
        {
            DefaultServeMux.HandleFunc(pattern, handler);
        }

        // Serve accepts incoming HTTP connections on the listener l,
        // creating a new service goroutine for each. The service goroutines
        // read requests and then call handler to reply to them.
        // Handler is typically nil, in which case the DefaultServeMux is used.
        public static error Serve(net.Listener l, Handler handler)
        {
            Server srv = ref new Server(Handler:handler);
            return error.As(srv.Serve(l));
        }

        // ServeTLS accepts incoming HTTPS connections on the listener l,
        // creating a new service goroutine for each. The service goroutines
        // read requests and then call handler to reply to them.
        //
        // Handler is typically nil, in which case the DefaultServeMux is used.
        //
        // Additionally, files containing a certificate and matching private key
        // for the server must be provided. If the certificate is signed by a
        // certificate authority, the certFile should be the concatenation
        // of the server's certificate, any intermediates, and the CA's certificate.
        public static error ServeTLS(net.Listener l, Handler handler, @string certFile, @string keyFile)
        {
            Server srv = ref new Server(Handler:handler);
            return error.As(srv.ServeTLS(l, certFile, keyFile));
        }

        // A Server defines parameters for running an HTTP server.
        // The zero value for Server is a valid configuration.
        public partial struct Server
        {
            public @string Addr; // TCP address to listen on, ":http" if empty
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
// is considered too slow for the body.
            public time.Duration ReadHeaderTimeout; // WriteTimeout is the maximum duration before timing out
// writes of the response. It is reset whenever a new
// request's header is read. Like ReadTimeout, it does not
// let Handlers make decisions on a per-request basis.
            public time.Duration WriteTimeout; // IdleTimeout is the maximum amount of time to wait for the
// next request when keep-alives are enabled. If IdleTimeout
// is zero, the value of ReadTimeout is used. If both are
// zero, ReadHeaderTimeout is used.
            public time.Duration IdleTimeout; // MaxHeaderBytes controls the maximum number of bytes the
// server will read parsing the request header's keys and
// values, including the request line. It does not limit the
// size of the request body.
// If zero, DefaultMaxHeaderBytes is used.
            public long MaxHeaderBytes; // TLSNextProto optionally specifies a function to take over
// ownership of the provided TLS connection when an NPN/ALPN
// protocol upgrade has occurred. The map key is the protocol
// name negotiated. The Handler argument should be used to
// handle HTTP requests and will initialize the Request's TLS
// and RemoteAddr if not already set. The connection is
// automatically closed when the function returns.
// If TLSNextProto is not nil, HTTP/2 support is not enabled
// automatically.
            public map<@string, Action<ref Server, ref tls.Conn, Handler>> TLSNextProto; // ConnState specifies an optional callback function that is
// called when a client connection changes state. See the
// ConnState type and associated constants for details.
            public Action<net.Conn, ConnState> ConnState; // ErrorLog specifies an optional logger for errors accepting
// connections, unexpected behavior from handlers, and
// underlying FileSystem errors.
// If nil, logging is done via the log package's standard logger.
            public ptr<log.Logger> ErrorLog;
            public int disableKeepAlives; // accessed atomically.
            public int inShutdown; // accessed atomically (non-zero means we're in Shutdown)
            public sync.Once nextProtoOnce; // guards setupHTTP2_* init
            public error nextProtoErr; // result of http2.ConfigureServer if used

            public sync.Mutex mu;
            public channel<object> doneChan;
            public slice<Action> onShutdown;
        }

        private static channel<object> getDoneChan(this ref Server _s) => func(_s, (ref Server s, Defer defer, Panic _, Recover __) =>
        {
            s.mu.Lock();
            defer(s.mu.Unlock());
            return s.getDoneChanLocked();
        });

        private static channel<object> getDoneChanLocked(this ref Server s)
        {
            if (s.doneChan == null)
            {
                s.doneChan = make_channel<object>();
            }
            return s.doneChan;
        }

        private static void closeDoneChanLocked(this ref Server s)
        {
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
        private static error Close(this ref Server _srv) => func(_srv, (ref Server srv, Defer defer, Panic _, Recover __) =>
        {
            srv.mu.Lock();
            defer(srv.mu.Unlock());
            srv.closeDoneChanLocked();
            var err = srv.closeListenersLocked();
            foreach (var (c) in srv.activeConn)
            {
                c.rwc.Close();
                delete(srv.activeConn, c);
            }
            return error.As(err);
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
        private static error Shutdown(this ref Server _srv, context.Context ctx) => func(_srv, (ref Server srv, Defer defer, Panic _, Recover __) =>
        {
            atomic.AddInt32(ref srv.inShutdown, 1L);
            defer(atomic.AddInt32(ref srv.inShutdown, -1L));

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
                if (srv.closeIdleConns())
                {
                    return error.As(lnerr);
                }
                return error.As(ctx.Err());
            }

        });

        // RegisterOnShutdown registers a function to call on Shutdown.
        // This can be used to gracefully shutdown connections that have
        // undergone NPN/ALPN protocol upgrade or that have been hijacked.
        // This function should start protocol-specific graceful shutdown,
        // but should not wait for shutdown to complete.
        private static void RegisterOnShutdown(this ref Server srv, Action f)
        {
            srv.mu.Lock();
            srv.onShutdown = append(srv.onShutdown, f);
            srv.mu.Unlock();
        }

        // closeIdleConns closes all idle connections and reports whether the
        // server is quiescent.
        private static bool closeIdleConns(this ref Server _s) => func(_s, (ref Server s, Defer defer, Panic _, Recover __) =>
        {
            s.mu.Lock();
            defer(s.mu.Unlock());
            var quiescent = true;
            foreach (var (c) in s.activeConn)
            {
                ConnState (st, ok) = c.curState.Load()._<ConnState>();
                if (!ok || st != StateIdle)
                {
                    quiescent = false;
                    continue;
                }
                c.rwc.Close();
                delete(s.activeConn, c);
            }
            return quiescent;
        });

        private static error closeListenersLocked(this ref Server s)
        {
            error err = default;
            foreach (var (ln) in s.listeners)
            {
                {
                    var cerr = ln.Close();

                    if (cerr != null && err == null)
                    {
                        err = error.As(cerr);
                    }

                }
                delete(s.listeners, ln);
            }
            return error.As(err);
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
        public static readonly ConnState StateNew = iota; 

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

        private static void ServeHTTP(this serverHandler sh, ResponseWriter rw, ref Request req)
        {
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
        // If srv.Addr is blank, ":http" is used.
        // ListenAndServe always returns a non-nil error.
        private static error ListenAndServe(this ref Server srv)
        {
            var addr = srv.Addr;
            if (addr == "")
            {
                addr = ":http";
            }
            var (ln, err) = net.Listen("tcp", addr);
            if (err != null)
            {
                return error.As(err);
            }
            return error.As(srv.Serve(new tcpKeepAliveListener(ln.(*net.TCPListener))));
        }

        private static Action<ref Server, net.Listener> testHookServerServe = default; // used if non-nil

        // shouldDoServeHTTP2 reports whether Server.Serve should configure
        // automatic HTTP/2. (which sets up the srv.TLSNextProto map)
        private static bool shouldConfigureHTTP2ForServe(this ref Server srv)
        {
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
        // For HTTP/2 support, srv.TLSConfig should be initialized to the
        // provided listener's TLS Config before calling Serve. If
        // srv.TLSConfig is non-nil and doesn't include the string "h2" in
        // Config.NextProtos, HTTP/2 support is not enabled.
        //
        // Serve always returns a non-nil error. After Shutdown or Close, the
        // returned error is ErrServerClosed.
        private static error Serve(this ref Server _srv, net.Listener l) => func(_srv, (ref Server srv, Defer defer, Panic _, Recover __) =>
        {
            defer(l.Close());
            {
                var fn = testHookServerServe;

                if (fn != null)
                {
                    fn(srv, l);
                }

            }
            time.Duration tempDelay = default; // how long to sleep on accept failure

            {
                var err = srv.setupHTTP2_Serve();

                if (err != null)
                {
                    return error.As(err);
                }

            }

            srv.trackListener(l, true);
            defer(srv.trackListener(l, false));

            var baseCtx = context.Background(); // base is always background, per Issue 16220
            var ctx = context.WithValue(baseCtx, ServerContextKey, srv);
            while (true)
            {
                var (rw, e) = l.Accept();
                if (e != null)
                {
                    return error.As(ErrServerClosed);
                    {
                        net.Error (ne, ok) = e._<net.Error>();

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
                            srv.logf("http: Accept error: %v; retrying in %v", e, tempDelay);
                            time.Sleep(tempDelay);
                            continue;
                        }

                    }
                    return error.As(e);
                }
                tempDelay = 0L;
                var c = srv.newConn(rw);
                c.setState(c.rwc, StateNew); // before Serve can return
                go_(() => c.serve(ctx));
            }

        });

        // ServeTLS accepts incoming connections on the Listener l, creating a
        // new service goroutine for each. The service goroutines read requests and
        // then call srv.Handler to reply to them.
        //
        // Additionally, files containing a certificate and matching private key for
        // the server must be provided if neither the Server's TLSConfig.Certificates
        // nor TLSConfig.GetCertificate are populated.. If the certificate is signed by
        // a certificate authority, the certFile should be the concatenation of the
        // server's certificate, any intermediates, and the CA's certificate.
        //
        // For HTTP/2 support, srv.TLSConfig should be initialized to the
        // provided listener's TLS Config before calling ServeTLS. If
        // srv.TLSConfig is non-nil and doesn't include the string "h2" in
        // Config.NextProtos, HTTP/2 support is not enabled.
        //
        // ServeTLS always returns a non-nil error. After Shutdown or Close, the
        // returned error is ErrServerClosed.
        private static error ServeTLS(this ref Server srv, net.Listener l, @string certFile, @string keyFile)
        { 
            // Setup HTTP/2 before srv.Serve, to initialize srv.TLSConfig
            // before we clone it and create the TLS Listener.
            {
                var err__prev1 = err;

                var err = srv.setupHTTP2_ServeTLS();

                if (err != null)
                {
                    return error.As(err);
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
                err = default;
                config.Certificates = make_slice<tls.Certificate>(1L);
                config.Certificates[0L], err = tls.LoadX509KeyPair(certFile, keyFile);
                if (err != null)
                {
                    return error.As(err);
                }
            }
            var tlsListener = tls.NewListener(l, config);
            return error.As(srv.Serve(tlsListener));
        }

        private static void trackListener(this ref Server _s, net.Listener ln, bool add) => func(_s, (ref Server s, Defer defer, Panic _, Recover __) =>
        {
            s.mu.Lock();
            defer(s.mu.Unlock());
            if (s.listeners == null)
            {
                s.listeners = make();
            }
            if (add)
            { 
                // If the *Server is being reused after a previous
                // Close or Shutdown, reset its doneChan:
                if (len(s.listeners) == 0L && len(s.activeConn) == 0L)
                {
                    s.doneChan = null;
                }
                s.listeners[ln] = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{};
            }
            else
            {
                delete(s.listeners, ln);
            }
        });

        private static void trackConn(this ref Server _s, ref conn _c, bool add) => func(_s, _c, (ref Server s, ref conn c, Defer defer, Panic _, Recover __) =>
        {
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

        private static time.Duration idleTimeout(this ref Server s)
        {
            if (s.IdleTimeout != 0L)
            {
                return s.IdleTimeout;
            }
            return s.ReadTimeout;
        }

        private static time.Duration readHeaderTimeout(this ref Server s)
        {
            if (s.ReadHeaderTimeout != 0L)
            {
                return s.ReadHeaderTimeout;
            }
            return s.ReadTimeout;
        }

        private static bool doKeepAlives(this ref Server s)
        {
            return atomic.LoadInt32(ref s.disableKeepAlives) == 0L && !s.shuttingDown();
        }

        private static bool shuttingDown(this ref Server s)
        {
            return atomic.LoadInt32(ref s.inShutdown) != 0L;
        }

        // SetKeepAlivesEnabled controls whether HTTP keep-alives are enabled.
        // By default, keep-alives are always enabled. Only very
        // resource-constrained environments or servers in the process of
        // shutting down should disable them.
        private static void SetKeepAlivesEnabled(this ref Server _srv, bool v) => func(_srv, (ref Server srv, Defer defer, Panic _, Recover __) =>
        {
            if (v)
            {
                atomic.StoreInt32(ref srv.disableKeepAlives, 0L);
                return;
            }
            atomic.StoreInt32(ref srv.disableKeepAlives, 1L); 

            // Close idle HTTP/1 conns:
            srv.closeIdleConns(); 

            // Close HTTP/2 conns, as soon as they become idle, but reset
            // the chan so future conns (if the listener is still active)
            // still work and don't get a GOAWAY immediately, before their
            // first request:
            srv.mu.Lock();
            defer(srv.mu.Unlock());
            srv.closeDoneChanLocked(); // closes http2 conns
            srv.doneChan = null;
        });

        private static void logf(this ref Server s, @string format, params object[] args)
        {
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
        private static void logf(ref Request r, @string format, params object[] args)
        {
            args = args.Clone();

            ref Server (s, _) = r.Context().Value(ServerContextKey)._<ref Server>();
            if (s != null && s.ErrorLog != null)
            {
                s.ErrorLog.Printf(format, args);
            }
            else
            {
                log.Printf(format, args);
            }
        }

        // ListenAndServe listens on the TCP network address addr
        // and then calls Serve with handler to handle requests
        // on incoming connections.
        // Accepted connections are configured to enable TCP keep-alives.
        // Handler is typically nil, in which case the DefaultServeMux is
        // used.
        //
        // A trivial example server is:
        //
        //    package main
        //
        //    import (
        //        "io"
        //        "net/http"
        //        "log"
        //    )
        //
        //    // hello world, the web server
        //    func HelloServer(w http.ResponseWriter, req *http.Request) {
        //        io.WriteString(w, "hello, world!\n")
        //    }
        //
        //    func main() {
        //        http.HandleFunc("/hello", HelloServer)
        //        log.Fatal(http.ListenAndServe(":12345", nil))
        //    }
        //
        // ListenAndServe always returns a non-nil error.
        public static error ListenAndServe(@string addr, Handler handler)
        {
            Server server = ref new Server(Addr:addr,Handler:handler);
            return error.As(server.ListenAndServe());
        }

        // ListenAndServeTLS acts identically to ListenAndServe, except that it
        // expects HTTPS connections. Additionally, files containing a certificate and
        // matching private key for the server must be provided. If the certificate
        // is signed by a certificate authority, the certFile should be the concatenation
        // of the server's certificate, any intermediates, and the CA's certificate.
        //
        // A trivial example server is:
        //
        //    import (
        //        "log"
        //        "net/http"
        //    )
        //
        //    func handler(w http.ResponseWriter, req *http.Request) {
        //        w.Header().Set("Content-Type", "text/plain")
        //        w.Write([]byte("This is an example server.\n"))
        //    }
        //
        //    func main() {
        //        http.HandleFunc("/", handler)
        //        log.Printf("About to listen on 10443. Go to https://127.0.0.1:10443/")
        //        err := http.ListenAndServeTLS(":10443", "cert.pem", "key.pem", nil)
        //        log.Fatal(err)
        //    }
        //
        // One can use generate_cert.go in crypto/tls to generate cert.pem and key.pem.
        //
        // ListenAndServeTLS always returns a non-nil error.
        public static error ListenAndServeTLS(@string addr, @string certFile, @string keyFile, Handler handler)
        {
            Server server = ref new Server(Addr:addr,Handler:handler);
            return error.As(server.ListenAndServeTLS(certFile, keyFile));
        }

        // ListenAndServeTLS listens on the TCP network address srv.Addr and
        // then calls Serve to handle requests on incoming TLS connections.
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
        // ListenAndServeTLS always returns a non-nil error.
        private static error ListenAndServeTLS(this ref Server _srv, @string certFile, @string keyFile) => func(_srv, (ref Server srv, Defer defer, Panic _, Recover __) =>
        {
            var addr = srv.Addr;
            if (addr == "")
            {
                addr = ":https";
            }
            var (ln, err) = net.Listen("tcp", addr);
            if (err != null)
            {
                return error.As(err);
            }
            defer(ln.Close());

            return error.As(srv.ServeTLS(new tcpKeepAliveListener(ln.(*net.TCPListener)), certFile, keyFile));
        });

        // setupHTTP2_ServeTLS conditionally configures HTTP/2 on
        // srv and returns whether there was an error setting it up. If it is
        // not configured for policy reasons, nil is returned.
        private static error setupHTTP2_ServeTLS(this ref Server srv)
        {
            srv.nextProtoOnce.Do(srv.onceSetNextProtoDefaults);
            return error.As(srv.nextProtoErr);
        }

        // setupHTTP2_Serve is called from (*Server).Serve and conditionally
        // configures HTTP/2 on srv using a more conservative policy than
        // setupHTTP2_ServeTLS because Serve may be called
        // concurrently.
        //
        // The tests named TestTransportAutomaticHTTP2* and
        // TestConcurrentServerServe in server_test.go demonstrate some
        // of the supported use cases and motivations.
        private static error setupHTTP2_Serve(this ref Server srv)
        {
            srv.nextProtoOnce.Do(srv.onceSetNextProtoDefaults_Serve);
            return error.As(srv.nextProtoErr);
        }

        private static void onceSetNextProtoDefaults_Serve(this ref Server srv)
        {
            if (srv.shouldConfigureHTTP2ForServe())
            {
                srv.onceSetNextProtoDefaults();
            }
        }

        // onceSetNextProtoDefaults configures HTTP/2, if the user hasn't
        // configured otherwise. (by setting srv.TLSNextProto non-nil)
        // It must only be called via srv.nextProtoOnce (use srv.setupHTTP2_*).
        private static void onceSetNextProtoDefaults(this ref Server srv)
        {
            if (strings.Contains(os.Getenv("GODEBUG"), "http2server=0"))
            {
                return;
            } 
            // Enable HTTP/2 by default if the user hasn't otherwise
            // configured their TLSNextProto map.
            if (srv.TLSNextProto == null)
            {
                http2Server conf = ref new http2Server(NewWriteScheduler:func()http2WriteScheduler{returnhttp2NewPriorityWriteScheduler(nil)},);
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
        // TimeoutHandler buffers all Handler writes to memory and does not
        // support the Hijacker or Flusher interfaces.
        public static Handler TimeoutHandler(Handler h, time.Duration dt, @string msg)
        {
            return ref new timeoutHandler(handler:h,body:msg,dt:dt,);
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

        private static @string errorBody(this ref timeoutHandler h)
        {
            if (h.body != "")
            {
                return h.body;
            }
            return "<html><head><title>Timeout</title></head><body><h1>Timeout</h1></body></html>";
        }

        private static void ServeHTTP(this ref timeoutHandler _h, ResponseWriter w, ref Request _r) => func(_h, _r, (ref timeoutHandler h, ref Request r, Defer defer, Panic panic, Recover _) =>
        {
            var ctx = h.testContext;
            if (ctx == null)
            {
                context.CancelFunc cancelCtx = default;
                ctx, cancelCtx = context.WithTimeout(r.Context(), h.dt);
                defer(cancelCtx());
            }
            r = r.WithContext(ctx);
            var done = make_channel<object>();
            timeoutWriter tw = ref new timeoutWriter(w:w,h:make(Header),);
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
            return;
        });

        private partial struct timeoutWriter
        {
            public ResponseWriter w;
            public Header h;
            public bytes.Buffer wbuf;
            public sync.Mutex mu;
            public bool timedOut;
            public bool wroteHeader;
            public long code;
        }

        private static Header Header(this ref timeoutWriter tw)
        {
            return tw.h;
        }

        private static (long, error) Write(this ref timeoutWriter _tw, slice<byte> p) => func(_tw, (ref timeoutWriter tw, Defer defer, Panic _, Recover __) =>
        {
            tw.mu.Lock();
            defer(tw.mu.Unlock());
            if (tw.timedOut)
            {
                return (0L, ErrHandlerTimeout);
            }
            if (!tw.wroteHeader)
            {
                tw.writeHeader(StatusOK);
            }
            return tw.wbuf.Write(p);
        });

        private static void WriteHeader(this ref timeoutWriter _tw, long code) => func(_tw, (ref timeoutWriter tw, Defer defer, Panic _, Recover __) =>
        {
            checkWriteHeaderCode(code);
            tw.mu.Lock();
            defer(tw.mu.Unlock());
            if (tw.timedOut || tw.wroteHeader)
            {
                return;
            }
            tw.writeHeader(code);
        });

        private static void writeHeader(this ref timeoutWriter tw, long code)
        {
            tw.wroteHeader = true;
            tw.code = code;
        }

        // tcpKeepAliveListener sets TCP keep-alive timeouts on accepted
        // connections. It's used by ListenAndServe and ListenAndServeTLS so
        // dead TCP connections (e.g. closing laptop mid-download) eventually
        // go away.
        private partial struct tcpKeepAliveListener
        {
            public ref net.TCPListener TCPListener => ref TCPListener_ptr;
        }

        private static (net.Conn, error) Accept(this tcpKeepAliveListener ln)
        {
            var (tc, err) = ln.AcceptTCP();
            if (err != null)
            {
                return (null, err);
            }
            tc.SetKeepAlive(true);
            tc.SetKeepAlivePeriod(3L * time.Minute);
            return (tc, null);
        }

        // globalOptionsHandler responds to "OPTIONS *" requests.
        private partial struct globalOptionsHandler
        {
        }

        private static void ServeHTTP(this globalOptionsHandler _p0, ResponseWriter w, ref Request r)
        {
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

        // initNPNRequest is an HTTP handler that initializes certain
        // uninitialized fields in its *Request. Such partially-initialized
        // Requests come from NPN protocol handlers.
        private partial struct initNPNRequest
        {
            public ptr<tls.Conn> c;
            public serverHandler h;
        }

        private static void ServeHTTP(this initNPNRequest h, ResponseWriter rw, ref Request req)
        {
            if (req.TLS == null)
            {
                req.TLS = ref new tls.ConnectionState();
                req.TLS.Value = h.c.ConnectionState();
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
            return ref new loggingConn(name:fmt.Sprintf("%s-%d",baseName,uniqNameNext[baseName]),Conn:c,);
        });

        private static (long, error) Write(this ref loggingConn c, slice<byte> p)
        {
            log.Printf("%s.Write(%d) = ....", c.name, len(p));
            n, err = c.Conn.Write(p);
            log.Printf("%s.Write(%d) = %d, %v", c.name, len(p), n, err);
            return;
        }

        private static (long, error) Read(this ref loggingConn c, slice<byte> p)
        {
            log.Printf("%s.Read(%d) = ....", c.name, len(p));
            n, err = c.Conn.Read(p);
            log.Printf("%s.Read(%d) = %d, %v", c.name, len(p), n, err);
            return;
        }

        private static error Close(this ref loggingConn c)
        {
            log.Printf("%s.Close() = ...", c.name);
            err = c.Conn.Close();
            log.Printf("%s.Close() = %v", c.name, err);
            return;
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
            n, err = w.c.rwc.Write(p);
            if (err != null && w.c.werr == null)
            {
                w.c.werr = err;
                w.c.cancelCtx();
            }
            return;
        }

        private static long numLeadingCRorLF(slice<byte> v)
        {
            foreach (var (_, b) in v)
            {
                if (b == '\r' || b == '\n')
                {
                    n++;
                    continue;
                }
                break;
            }
            return;

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
    }
}}
