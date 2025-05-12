// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// HTTP server. See RFC 7230 through 7235.
namespace go.net;

using bufio = bufio_package;
using bytes = bytes_package;
using context = context_package;
using tls = crypto.tls_package;
using errors = errors_package;
using fmt = fmt_package;
using godebug = @internal.godebug_package;
using io = io_package;
using log = log_package;
using maps = maps_package;
using rand = math.rand_package;
using net = net_package;
using textproto = net.textproto_package;
using url = net.url_package;
using urlpkg = net.url_package;
using path = path_package;
using runtime = runtime_package;
using slices = slices_package;
using strconv = strconv_package;
using strings = strings_package;
using sync = sync_package;
using atomic = sync.atomic_package;
using time = time_package;
using _ = unsafe_package; // for linkname
using httpguts = golang.org.x.net.http.httpguts_package;
using @internal;
using crypto;
using golang.org.x.net.http;
using math;
using sync;
using ꓸꓸꓸany = Span<any>;

partial class http_package {

// Errors used by the HTTP server.
public static error ErrBodyNotAllowed = errors.New("http: request method or response status code does not allow body"u8);

public static error ErrHijacked = errors.New("http: connection has been hijacked"u8);

public static error ErrContentLength = errors.New("http: wrote more than the declared Content-Length"u8);

public static error ErrWriteAfterFlush = errors.New("unused"u8);

// A Handler responds to an HTTP request.
//
// [Handler.ServeHTTP] should write reply headers and data to the [ResponseWriter]
// and then return. Returning signals that the request is finished; it
// is not valid to use the [ResponseWriter] or read from the
// [Request.Body] after or concurrently with the completion of the
// ServeHTTP call.
//
// Depending on the HTTP client software, HTTP protocol version, and
// any intermediaries between the client and the Go server, it may not
// be possible to read from the [Request.Body] after writing to the
// [ResponseWriter]. Cautious handlers should read the [Request.Body]
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
// an error, panic with the value [ErrAbortHandler].
[GoType] partial interface ΔHandler {
    void ServeHTTP(ResponseWriter _, ж<Request> _);
}

// A ResponseWriter interface is used by an HTTP handler to
// construct an HTTP response.
//
// A ResponseWriter may not be used after [Handler.ServeHTTP] has returned.
[GoType] partial interface ResponseWriter {
    // Header returns the header map that will be sent by
    // [ResponseWriter.WriteHeader]. The [Header] map also is the mechanism with which
    // [Handler] implementations can set HTTP trailers.
    //
    // Changing the header map after a call to [ResponseWriter.WriteHeader] (or
    // [ResponseWriter.Write]) has no effect unless the HTTP status code was of the
    // 1xx class or the modified headers are trailers.
    //
    // There are two ways to set Trailers. The preferred way is to
    // predeclare in the headers which trailers you will later
    // send by setting the "Trailer" header to the names of the
    // trailer keys which will come later. In this case, those
    // keys of the Header map are treated as if they were
    // trailers. See the example. The second way, for trailer
    // keys not known to the [Handler] until after the first [ResponseWriter.Write],
    // is to prefix the [Header] map keys with the [TrailerPrefix]
    // constant value.
    //
    // To suppress automatic response headers (such as "Date"), set
    // their value to nil.
    ΔHeader Header();
    // Write writes the data to the connection as part of an HTTP reply.
    //
    // If [ResponseWriter.WriteHeader] has not yet been called, Write calls
    // WriteHeader(http.StatusOK) before writing the data. If the Header
    // does not contain a Content-Type line, Write adds a Content-Type set
    // to the result of passing the initial 512 bytes of written data to
    // [DetectContentType]. Additionally, if the total size of all written
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
    (nint, error) Write(slice<byte> _);
    // WriteHeader sends an HTTP response header with the provided
    // status code.
    //
    // If WriteHeader is not called explicitly, the first call to Write
    // will trigger an implicit WriteHeader(http.StatusOK).
    // Thus explicit calls to WriteHeader are mainly used to
    // send error codes or 1xx informational responses.
    //
    // The provided code must be a valid HTTP 1xx-5xx status code.
    // Any number of 1xx headers may be written, followed by at most
    // one 2xx-5xx header. 1xx headers are sent immediately, but 2xx-5xx
    // headers may be buffered. Use the Flusher interface to send
    // buffered data. The header map is cleared when 2xx-5xx headers are
    // sent, but not with 1xx headers.
    //
    // The server will automatically send a 100 (Continue) header
    // on the first read from the request body if the request has
    // an "Expect: 100-continue" header.
    void WriteHeader(nint statusCode);
}

// The Flusher interface is implemented by ResponseWriters that allow
// an HTTP handler to flush buffered data to the client.
//
// The default HTTP/1.x and HTTP/2 [ResponseWriter] implementations
// support [Flusher], but ResponseWriter wrappers may not. Handlers
// should always test for this ability at runtime.
//
// Note that even for ResponseWriters that support Flush,
// if the client is connected through an HTTP proxy,
// the buffered data may not reach the client until the response
// completes.
[GoType] partial interface Flusher {
    // Flush sends any buffered data to the client.
    void Flush();
}

// The Hijacker interface is implemented by ResponseWriters that allow
// an HTTP handler to take over the connection.
//
// The default [ResponseWriter] for HTTP/1.x connections supports
// Hijacker, but HTTP/2 connections intentionally do not.
// ResponseWriter wrappers may also not support Hijacker. Handlers
// should always test for this ability at runtime.
[GoType] partial interface Hijacker {
    // Hijack lets the caller take over the connection.
    // After a call to Hijack the HTTP server library
    // will not do anything else with the connection.
    //
    // It becomes the caller's responsibility to manage
    // and close the connection.
    //
    // The returned net.Conn may have read or write deadlines
    // already set, depending on the configuration of the
    // Server. It is the caller's responsibility to set
    // or clear those deadlines as needed.
    //
    // The returned bufio.Reader may contain unprocessed buffered
    // data from the client.
    //
    // After a call to Hijack, the original Request.Body must not
    // be used. The original Request's Context remains valid and
    // is not canceled until the Request's ServeHTTP method
    // returns.
    (net.Conn, ж<bufio.ReadWriter>, error) Hijack();
}

// The CloseNotifier interface is implemented by ResponseWriters which
// allow detecting when the underlying connection has gone away.
//
// This mechanism can be used to cancel long operations on the server
// if the client has disconnected before the response is ready.
//
// Deprecated: the CloseNotifier interface predates Go's context package.
// New code should use [Request.Context] instead.
[GoType] partial interface CloseNotifier {
    // CloseNotify returns a channel that receives at most a
    // single value (true) when the client connection has gone
    // away.
    //
    // CloseNotify may wait to notify until Request.Body has been
    // fully read.
    //
    // After the Handler has returned, there is no guarantee
    // that the channel receives a value.
    //
    // If the protocol is HTTP/1.1 and CloseNotify is called while
    // processing an idempotent request (such as GET) while
    // HTTP/1.1 pipelining is in use, the arrival of a subsequent
    // pipelined request may cause a value to be sent on the
    // returned channel. In practice HTTP/1.1 pipelining is not
    // enabled in browsers and not seen often in the wild. If this
    // is a problem, use HTTP/2 or only use CloseNotify on methods
    // such as POST.
    /*<-*/channel<bool> CloseNotify();
}

public static ж<contextKey> ServerContextKey = Ꮡ(new contextKey("http-server"));
public static ж<contextKey> LocalAddrContextKey = Ꮡ(new contextKey("local-addr"));

// A conn represents the server side of an HTTP connection.
[GoType] partial struct conn {
    // server is the server on which the connection arrived.
    // Immutable; never nil.
    internal ж<Server> server;
    // cancelCtx cancels the connection-level context.
    internal context_package.CancelFunc cancelCtx;
    // rwc is the underlying network connection.
    // This is never wrapped by other types and is the value given out
    // to CloseNotifier callers. It is usually of type *net.TCPConn or
    // *tls.Conn.
    internal net_package.Conn rwc;
    // remoteAddr is rwc.RemoteAddr().String(). It is not populated synchronously
    // inside the Listener's Accept goroutine, as some implementations block.
    // It is populated immediately inside the (*conn).serve goroutine.
    // This is the value of a Handler's (*Request).RemoteAddr.
    internal @string remoteAddr;
    // tlsState is the TLS connection state when using TLS.
    // nil means not TLS.
    internal ж<crypto.tls_package.ΔConnectionState> tlsState;
    // werr is set to the first write error to rwc.
    // It is set via checkConnErrorWriter{w}, where bufw writes.
    internal error werr;
    // r is bufr's read source. It's a wrapper around rwc that provides
    // io.LimitedReader-style limiting (while reading request headers)
    // and functionality to support CloseNotifier. See *connReader docs.
    internal ж<connReader> r;
    // bufr reads from r.
    internal ж<bufio_package.Reader> bufr;
    // bufw writes to checkConnErrorWriter{c}, which populates werr on error.
    internal ж<bufio_package.Writer> bufw;
    // lastMethod is the method of the most recent request
    // on this connection, if any.
    internal @string lastMethod;
    internal sync.atomic_package.Pointer curReq; // (which has a Request in it)
    internal sync.atomic_package.Uint64 curState; // packed (unixtime<<8|uint8(ConnState))
    // mu guards hijackedv
    internal sync_package.Mutex mu;
    // hijackedv is whether this connection has been hijacked
    // by a Handler with the Hijacker interface.
    // It is guarded by mu.
    internal bool hijackedv;
}

[GoRecv] internal static bool hijacked(this ref conn c) => func((defer, _) => {
    c.mu.Lock();
    defer(c.mu.Unlock);
    return c.hijackedv;
});

// c.mu must be held.
[GoRecv] internal static (net.Conn rwc, ж<bufio.ReadWriter> buf, error err) hijackLocked(this ref conn c) {
    net.Conn rwc = default!;
    ж<bufio.ReadWriter> buf = default!;
    error err = default!;

    if (c.hijackedv) {
        return (default!, default!, ErrHijacked);
    }
    c.r.abortPendingRead();
    c.hijackedv = true;
    rwc = c.rwc;
    rwc.SetDeadline(new time.Time(nil));
    buf = bufio.NewReadWriter(c.bufr, bufio.NewWriter(rwc));
    if (c.r.hasByte) {
        {
            (_, errΔ1) = c.bufr.Peek(c.bufr.Buffered() + 1); if (errΔ1 != default!) {
                return (default!, default!, fmt.Errorf("unexpected Peek failure reading buffered byte: %v"u8, errΔ1));
            }
        }
    }
    c.setState(rwc, StateHijacked, runHooks);
    return (rwc, buf, err);
}

// This should be >= 512 bytes for DetectContentType,
// but otherwise it's somewhat arbitrary.
internal static readonly UntypedInt bufferBeforeChunkingSize = 2048;

// chunkWriter writes to a response's conn buffer, and is the writer
// wrapped by the response.w buffered writer.
//
// chunkWriter also is responsible for finalizing the Header, including
// conditionally setting the Content-Type and setting a Content-Length
// in cases where the handler's final output is smaller than the buffer
// size. It also conditionally adds chunk headers, when in chunking mode.
//
// See the comment above (*response).Write for the entire write flow.
[GoType] partial struct chunkWriter {
    internal ж<response> res;
    // header is either nil or a deep clone of res.handlerHeader
    // at the time of res.writeHeader, if res.writeHeader is
    // called and extra buffering is being done to calculate
    // Content-Type and/or Content-Length.
    internal ΔHeader header;
    // wroteHeader tells whether the header's been written to "the
    // wire" (or rather: w.conn.buf). this is unlike
    // (*response).wroteHeader, which tells only whether it was
    // logically written.
    internal bool wroteHeader;
    // set by the writeHeader method:
    internal bool chunking; // using chunked transfer encoding for reply body
}

internal static slice<byte> crlf = slice<byte>("\r\n");
internal static slice<byte> colonSpace = slice<byte>(": ");

[GoRecv] internal static (nint n, error err) Write(this ref chunkWriter cw, slice<byte> p) {
    nint n = default!;
    error err = default!;

    if (!cw.wroteHeader) {
        cw.writeHeader(p);
    }
    if (cw.res.req.Method == "HEAD"u8) {
        // Eat writes.
        return (len(p), default!);
    }
    if (cw.chunking) {
        (_, err) = fmt.Fprintf(~cw.res.conn.bufw, "%x\r\n"u8, len(p));
        if (err != default!) {
            cw.res.conn.rwc.Close();
            return (n, err);
        }
    }
    (n, err) = cw.res.conn.bufw.Write(p);
    if (cw.chunking && err == default!) {
        (_, err) = cw.res.conn.bufw.Write(crlf);
    }
    if (err != default!) {
        cw.res.conn.rwc.Close();
    }
    return (n, err);
}

[GoRecv] internal static error flush(this ref chunkWriter cw) {
    if (!cw.wroteHeader) {
        cw.writeHeader(default!);
    }
    return cw.res.conn.bufw.Flush();
}

[GoRecv] internal static void close(this ref chunkWriter cw) {
    if (!cw.wroteHeader) {
        cw.writeHeader(default!);
    }
    if (cw.chunking) {
        var bw = cw.res.conn.bufw;
        // conn's bufio writer
        // zero chunk to mark EOF
        bw.WriteString("0\r\n"u8);
        {
            var trailers = cw.res.finalTrailers(); if (trailers != default!) {
                trailers.Write(~bw);
            }
        }
        // the writer handles noting errors
        // final blank line after the trailers (whether
        // present or not)
        bw.WriteString("\r\n"u8);
    }
}

// A response represents the server side of an HTTP response.
[GoType] partial struct response {
    internal ж<conn> conn;
    internal ж<Request> req; // request for this response
    internal io_package.ReadCloser reqBody;
    internal context_package.CancelFunc cancelCtx; // when ServeHTTP exits
    internal bool wroteHeader;               // a non-1xx header has been (logically) written
    internal bool wants10KeepAlive;               // HTTP/1.0 w/ Connection "keep-alive"
    internal bool wantsClose;               // HTTP request has Connection "close"
    // canWriteContinue is an atomic boolean that says whether or
    // not a 100 Continue header can be written to the
    // connection.
    // writeContinueMu must be held while writing the header.
    // These two fields together synchronize the body reader (the
    // expectContinueReader, which wants to write 100 Continue)
    // against the main writer.
    internal sync_package.Mutex writeContinueMu;
    internal sync.atomic_package.Bool canWriteContinue;
    internal ж<bufio_package.Writer> w; // buffers output in chunks to chunkWriter
    internal chunkWriter cw;
    // handlerHeader is the Header that Handlers get access to,
    // which may be retained and mutated even after WriteHeader.
    // handlerHeader is copied into cw.header at WriteHeader
    // time, and privately mutated thereafter.
    internal ΔHeader handlerHeader;
    internal bool calledHeader; // handler accessed handlerHeader via Header
    internal int64 written; // number of bytes written in body
    internal int64 contentLength; // explicitly-declared Content-Length; or -1
    internal nint status;  // status code passed to WriteHeader
    // close connection after this reply.  set on request and
    // updated after response from handler if there's a
    // "Connection: keep-alive" response header and a
    // Content-Length.
    internal bool closeAfterReply;
    // When fullDuplex is false (the default), we consume any remaining
    // request body before starting to write a response.
    internal bool fullDuplex;
    // requestBodyLimitHit is set by requestTooLarge when
    // maxBytesReader hits its max size. It is checked in
    // WriteHeader, to make sure we don't consume the
    // remaining request body to try to advance to the next HTTP
    // request. Instead, when this is set, we stop reading
    // subsequent requests on this connection and stop reading
    // input from it.
    internal bool requestBodyLimitHit;
    // trailers are the headers to be sent after the handler
    // finishes writing the body. This field is initialized from
    // the Trailer response header when the response header is
    // written.
    internal slice<@string> trailers;
    internal sync.atomic_package.Bool handlerDone; // set true when the handler exits
    // Buffers for Date, Content-Length, and status code
    internal array<byte> dateBuf = new(len(TimeFormat));
    internal array<byte> clenBuf = new(10);
    internal array<byte> statusBuf = new(3);
    // closeNotifyCh is the channel returned by CloseNotify.
    // TODO(bradfitz): this is currently (for Go 1.8) always
    // non-nil. Make this lazily-created again as it used to be?
    internal channel<bool> closeNotifyCh;
    internal sync.atomic_package.Bool didCloseNotify; // atomic (only false->true winner should send)
}

[GoRecv] internal static error SetReadDeadline(this ref response c, time.Time deadline) {
    return c.conn.rwc.SetReadDeadline(deadline);
}

[GoRecv] internal static error SetWriteDeadline(this ref response c, time.Time deadline) {
    return c.conn.rwc.SetWriteDeadline(deadline);
}

[GoRecv] internal static error EnableFullDuplex(this ref response c) {
    c.fullDuplex = true;
    return default!;
}

// TrailerPrefix is a magic prefix for [ResponseWriter.Header] map keys
// that, if present, signals that the map entry is actually for
// the response trailers, and not the response headers. The prefix
// is stripped after the ServeHTTP call finishes and the values are
// sent in the trailers.
//
// This mechanism is intended only for trailers that are not known
// prior to the headers being written. If the set of trailers is fixed
// or known before the header is written, the normal Go trailers mechanism
// is preferred:
//
//	https://pkg.go.dev/net/http#ResponseWriter
//	https://pkg.go.dev/net/http#example-ResponseWriter-Trailers
public static readonly @string TrailerPrefix = "Trailer:"u8;

// finalTrailers is called after the Handler exits and returns a non-nil
// value if the Handler set any trailers.
[GoRecv] internal static ΔHeader finalTrailers(this ref response w) {
    ΔHeader t = default!;
    foreach (var (k, vv) in w.handlerHeader) {
        {
            var (kk, found) = strings.CutPrefix(k, TrailerPrefix); if (found) {
                if (t == default!) {
                    t = new ΔHeader();
                }
                t[kk] = vv;
            }
        }
    }
    foreach (var (_, k) in w.trailers) {
        if (t == default!) {
            t = new ΔHeader();
        }
        foreach (var (_, v) in w.handlerHeader[k]) {
            t.Add(k, v);
        }
    }
    return t;
}

// declareTrailer is called for each Trailer header when the
// response header is written. It notes that a header will need to be
// written in the trailers at the end of the response.
[GoRecv] internal static void declareTrailer(this ref response w, @string k) {
    k = CanonicalHeaderKey(k);
    if (!httpguts.ValidTrailerHeader(k)) {
        // Forbidden by RFC 7230, section 4.1.2
        return;
    }
    w.trailers = append(w.trailers, k);
}

// requestTooLarge is called by maxBytesReader when too much input has
// been read from the client.
[GoRecv] internal static void requestTooLarge(this ref response w) {
    w.closeAfterReply = true;
    w.requestBodyLimitHit = true;
    if (!w.wroteHeader) {
        w.Header().Set("Connection"u8, "close"u8);
    }
}

// disableWriteContinue stops Request.Body.Read from sending an automatic 100-Continue.
// If a 100-Continue is being written, it waits for it to complete before continuing.
[GoRecv] internal static void disableWriteContinue(this ref response w) {
    w.writeContinueMu.Lock();
    w.canWriteContinue.Store(false);
    w.writeContinueMu.Unlock();
}

// writerOnly hides an io.Writer value's optional ReadFrom method
// from io.Copy.
[GoType] partial struct writerOnly {
    public partial ref io_package.Writer Writer { get; }
}

// ReadFrom is here to optimize copying from an [*os.File] regular file
// to a [*net.TCPConn] with sendfile, or from a supported src type such
// as a *net.TCPConn on Linux with splice.
[GoRecv] internal static (int64 n, error err) ReadFrom(this ref response w, io.Reader src) => func((defer, _) => {
    int64 n = default!;
    error err = default!;

    var buf = getCopyBuf();
    deferǃ(putCopyBuf, buf, defer);
    // Our underlying w.conn.rwc is usually a *TCPConn (with its
    // own ReadFrom method). If not, just fall back to the normal
    // copy method.
    var (rf, ok) = w.conn.rwc._<io.ReaderFrom>(ᐧ);
    if (!ok) {
        return io.CopyBuffer(new writerOnly(w), src, buf);
    }
    // Copy the first sniffLen bytes before switching to ReadFrom.
    // This ensures we don't start writing the response before the
    // source is available (see golang.org/issue/5660) and provides
    // enough bytes to perform Content-Type sniffing when required.
    if (!w.cw.wroteHeader) {
        var (n0Δ1, errΔ1) = io.CopyBuffer(new writerOnly(w), io.LimitReader(src, sniffLen), buf);
        n += n0Δ1;
        if (errΔ1 != default! || n0Δ1 < sniffLen) {
            return (n, errΔ1);
        }
    }
    w.w.Flush();
    // get rid of any previous writes
    w.cw.flush();
    // make sure Header is written; flush data to rwc
    // Now that cw has been flushed, its chunking field is guaranteed initialized.
    if (!w.cw.chunking && w.bodyAllowed()) {
        var (n0Δ2, errΔ2) = rf.ReadFrom(src);
        n += n0Δ2;
        w.written += n0Δ2;
        return (n, errΔ2);
    }
    var (n0, err) = io.CopyBuffer(new writerOnly(w), src, buf);
    n += n0;
    return (n, err);
});

// debugServerConnections controls whether all server connections are wrapped
// with a verbose logging wrapper.
internal const bool debugServerConnections = false;

// Create new connection from rwc.
[GoRecv] internal static ж<conn> newConn(this ref Server srv, net.Conn rwc) {
    var c = Ꮡ(new conn(
        server: srv,
        rwc: rwc
    ));
    if (debugServerConnections) {
        c.val.rwc = newLoggingConn("server"u8, (~c).rwc);
    }
    return c;
}

[GoType] partial struct readResult {
    internal incomparable _;
    internal nint n;
    internal error err;
    internal byte b; // byte read, if n == 1
}

// connReader is the io.Reader wrapper used by *conn. It combines a
// selectively-activated io.LimitedReader (to bound request header
// read sizes) with support for selectively keeping an io.Reader.Read
// call blocked in a background goroutine to wait for activity and
// trigger a CloseNotifier channel.
[GoType] partial struct connReader {
    internal ж<conn> conn;
    internal sync_package.Mutex mu; // guards following
    internal bool hasByte;
    internal array<byte> byteBuf = new(1);
    internal ж<sync_package.Cond> cond;
    internal bool inRead;
    internal bool aborted;  // set true before conn.rwc deadline is set to past
    internal int64 remain; // bytes remaining
}

[GoRecv] internal static void @lock(this ref connReader cr) {
    cr.mu.Lock();
    if (cr.cond == nil) {
        cr.cond = sync.NewCond(cr.mu);
    }
}

[GoRecv] internal static void unlock(this ref connReader cr) {
    cr.mu.Unlock();
}

[GoRecv] internal static void startBackgroundRead(this ref connReader cr) => func((defer, _) => {
    cr.@lock();
    defer(cr.unlock);
    if (cr.inRead) {
        throw panic("invalid concurrent Body.Read call");
    }
    if (cr.hasByte) {
        return;
    }
    cr.inRead = true;
    cr.conn.rwc.SetReadDeadline(new time.Time(nil));
    goǃ(cr.backgroundRead);
});

[GoRecv] internal static void backgroundRead(this ref connReader cr) {
    var (n, err) = cr.conn.rwc.Read(cr.byteBuf[..]);
    cr.@lock();
    if (n == 1) {
        cr.hasByte = true;
    }
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
    {
        var (ne, ok) = err._<netꓸError>(ᐧ); if (ok && cr.aborted && ne.Timeout()){
        } else 
        if (err != default!) {
            // Ignore this error. It's the expected error from
            // another goroutine calling abortPendingRead.
            cr.handleReadError(err);
        }
    }
    cr.aborted = false;
    cr.inRead = false;
    cr.unlock();
    cr.cond.Broadcast();
}

[GoRecv] internal static void abortPendingRead(this ref connReader cr) => func((defer, _) => {
    cr.@lock();
    defer(cr.unlock);
    if (!cr.inRead) {
        return;
    }
    cr.aborted = true;
    cr.conn.rwc.SetReadDeadline(aLongTimeAgo);
    while (cr.inRead) {
        cr.cond.Wait();
    }
    cr.conn.rwc.SetReadDeadline(new time.Time(nil));
});

[GoRecv] internal static void setReadLimit(this ref connReader cr, int64 remain) {
    cr.remain = remain;
}

[GoRecv] internal static void setInfiniteReadLimit(this ref connReader cr) {
    cr.remain = maxInt64;
}

[GoRecv] internal static bool hitReadLimit(this ref connReader cr) {
    return cr.remain <= 0;
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
[GoRecv] internal static void handleReadError(this ref connReader cr, error _) {
    cr.conn.cancelCtx();
    cr.closeNotify();
}

// may be called from multiple goroutines.
[GoRecv] internal static void closeNotify(this ref connReader cr) {
    var res = cr.conn.curReq.Load();
    if (res != nil && !(~res).didCloseNotify.Swap(true)) {
        (~res).closeNotifyCh.ᐸꟷ(true);
    }
}

[GoRecv] internal static (nint n, error err) Read(this ref connReader cr, slice<byte> p) {
    nint n = default!;
    error err = default!;

    cr.@lock();
    if (cr.inRead) {
        cr.unlock();
        if (cr.conn.hijacked()) {
            throw panic("invalid Body.Read call. After hijacked, the original Request must not be used");
        }
        throw panic("invalid concurrent Body.Read call");
    }
    if (cr.hitReadLimit()) {
        cr.unlock();
        return (0, io.EOF);
    }
    if (len(p) == 0) {
        cr.unlock();
        return (0, default!);
    }
    if (((int64)len(p)) > cr.remain) {
        p = p[..(int)(cr.remain)];
    }
    if (cr.hasByte) {
        p[0] = cr.byteBuf[0];
        cr.hasByte = false;
        cr.unlock();
        return (1, default!);
    }
    cr.inRead = true;
    cr.unlock();
    (n, err) = cr.conn.rwc.Read(p);
    cr.@lock();
    cr.inRead = false;
    if (err != default!) {
        cr.handleReadError(err);
    }
    cr.remain -= ((int64)n);
    cr.unlock();
    cr.cond.Broadcast();
    return (n, err);
}

internal static sync.Pool bufioReaderPool;
internal static sync.Pool bufioWriter2kPool;
internal static sync.Pool bufioWriter4kPool;

internal static readonly UntypedInt copyBufPoolSize = /* 32 * 1024 */ 32768;

internal static sync.Pool copyBufPool = new sync.Pool(New: () => @new<array<byte>>());

internal static slice<byte> getCopyBuf() {
    return copyBufPool.Get()._<array<byte>.val>()[..];
}

internal static void putCopyBuf(slice<byte> b) {
    if (len(b) != copyBufPoolSize) {
        throw panic("trying to put back buffer of the wrong size in the copyBufPool");
    }
    copyBufPool.Put((ж<array<byte>>)(b));
}

internal static ж<sync.Pool> bufioWriterPool(nint size) {
    var exprᴛ1 = size;
    if (exprᴛ1 == 2 << (int)(10)) {
        return Ꮡ(bufioWriter2kPool);
    }
    if (exprᴛ1 == 4 << (int)(10)) {
        return Ꮡ(bufioWriter4kPool);
    }

    return default!;
}

// newBufioReader should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/gobwas/ws
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname newBufioReader
internal static ж<bufio.Reader> newBufioReader(io.Reader r) {
    {
        var v = bufioReaderPool.Get(); if (v != default!) {
            var br = v._<ж<bufio.Reader>>();
            br.Reset(r);
            return br;
        }
    }
    // Note: if this reader size is ever changed, update
    // TestHandlerBodyClose's assumptions.
    return bufio.NewReader(r);
}

// putBufioReader should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/gobwas/ws
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname putBufioReader
internal static void putBufioReader(ж<bufio.Reader> Ꮡbr) {
    ref var br = ref Ꮡbr.val;

    br.Reset(default!);
    bufioReaderPool.Put(br);
}

// newBufioWriterSize should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/gobwas/ws
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname newBufioWriterSize
internal static ж<bufio.Writer> newBufioWriterSize(io.Writer w, nint size) {
    var pool = bufioWriterPool(size);
    if (pool != nil) {
        {
            var v = pool.Get(); if (v != default!) {
                var bw = v._<ж<bufio.Writer>>();
                bw.Reset(w);
                return bw;
            }
        }
    }
    return bufio.NewWriterSize(w, size);
}

// putBufioWriter should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/gobwas/ws
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname putBufioWriter
internal static void putBufioWriter(ж<bufio.Writer> Ꮡbw) {
    ref var bw = ref Ꮡbw.val;

    bw.Reset(default!);
    {
        var pool = bufioWriterPool(bw.Available()); if (pool != nil) {
            pool.Put(bw);
        }
    }
}

// DefaultMaxHeaderBytes is the maximum permitted size of the headers
// in an HTTP request.
// This can be overridden by setting [Server.MaxHeaderBytes].
public static readonly UntypedInt DefaultMaxHeaderBytes = /* 1 << 20 */ 1048576; // 1 MB

[GoRecv] internal static nint maxHeaderBytes(this ref Server srv) {
    if (srv.MaxHeaderBytes > 0) {
        return srv.MaxHeaderBytes;
    }
    return DefaultMaxHeaderBytes;
}

[GoRecv] internal static int64 initialReadLimitSize(this ref Server srv) {
    return ((int64)srv.maxHeaderBytes()) + 4096;
}

// bufio slop

// tlsHandshakeTimeout returns the time limit permitted for the TLS
// handshake, or zero for unlimited.
//
// It returns the minimum of any positive ReadHeaderTimeout,
// ReadTimeout, or WriteTimeout.
[GoRecv] internal static time.Duration tlsHandshakeTimeout(this ref Server srv) {
    time.Duration ret = default!;
    foreach (var (_, v) in new time.Duration[]{
        srv.ReadHeaderTimeout,
        srv.ReadTimeout,
        srv.WriteTimeout
    }.array()) {
        if (v <= 0) {
            continue;
        }
        if (ret == 0 || v < ret) {
            ret = v;
        }
    }
    return ret;
}

// wrapper around io.ReadCloser which on first read, sends an
// HTTP/1.1 100 Continue header
[GoType] partial struct expectContinueReader {
    internal ж<response> resp;
    internal io_package.ReadCloser readCloser;
    internal sync.atomic_package.Bool closed;
    internal sync.atomic_package.Bool sawEOF;
}

[GoRecv] internal static (nint n, error err) Read(this ref expectContinueReader ecr, slice<byte> p) {
    nint n = default!;
    error err = default!;

    if (ecr.closed.Load()) {
        return (0, ErrBodyReadAfterClose);
    }
    var w = ecr.resp;
    if ((~w).canWriteContinue.Load()) {
        (~w).writeContinueMu.Lock();
        if ((~w).canWriteContinue.Load()) {
            (~(~w).conn).bufw.WriteString("HTTP/1.1 100 Continue\r\n\r\n"u8);
            (~(~w).conn).bufw.Flush();
            (~w).canWriteContinue.Store(false);
        }
        (~w).writeContinueMu.Unlock();
    }
    (n, err) = ecr.readCloser.Read(p);
    if (AreEqual(err, io.EOF)) {
        ecr.sawEOF.Store(true);
    }
    return (n, err);
}

[GoRecv] internal static error Close(this ref expectContinueReader ecr) {
    ecr.closed.Store(true);
    return ecr.readCloser.Close();
}

// TimeFormat is the time format to use when generating times in HTTP
// headers. It is like [time.RFC1123] but hard-codes GMT as the time
// zone. The time being formatted must be in UTC for Format to
// generate the correct format.
//
// For parsing this time format, see [ParseTime].
public static readonly @string TimeFormat = "Mon, 02 Jan 2006 15:04:05 GMT"u8;

// appendTime is a non-allocating version of []byte(t.UTC().Format(TimeFormat))
internal static slice<byte> appendTime(slice<byte> b, time.Time t) {
    @string days = "SunMonTueWedThuFriSat"u8;
    @string months = "JanFebMarAprMayJunJulAugSepOctNovDec"u8;
    t = t.UTC();
    var (yy, mm, dd) = t.Date();
    var (hh, mn, ss) = t.Clock();
    @string day = days[(int)(3 * t.Weekday())..];
    @string mon = months[(int)(3 * (mm - 1))..];
    return append(b,
        day[0], day[1], day[2], (rune)',', (rune)' ',
        ((byte)((rune)'0' + dd / 10)), ((byte)((rune)'0' + dd % 10)), (rune)' ',
        mon[0], mon[1], mon[2], (rune)' ',
        ((byte)((rune)'0' + yy / 1000)), ((byte)((rune)'0' + (yy / 100) % 10)), ((byte)((rune)'0' + (yy / 10) % 10)), ((byte)((rune)'0' + yy % 10)), (rune)' ',
        ((byte)((rune)'0' + hh / 10)), ((byte)((rune)'0' + hh % 10)), (rune)':',
        ((byte)((rune)'0' + mn / 10)), ((byte)((rune)'0' + mn % 10)), (rune)':',
        ((byte)((rune)'0' + ss / 10)), ((byte)((rune)'0' + ss % 10)), (rune)' ',
        (rune)'G', (rune)'M', (rune)'T');
}

internal static error errTooLarge = errors.New("http: request too large"u8);

// Read next request from connection.
[GoRecv] internal static (ж<response> w, error err) readRequest(this ref conn c, context.Context ctx) => func((defer, _) => {
    ж<response> w = default!;
    error err = default!;

    if (c.hijacked()) {
        return (default!, ErrHijacked);
    }
    time.Time wholeReqDeadline = default!;    // or zero if none
    time.Time hdrDeadline = default!;              // or zero if none
    var t0 = time.Now();
    {
        var d = c.server.readHeaderTimeout(); if (d > 0) {
            hdrDeadline = t0.Add(d);
        }
    }
    {
        var d = c.server.ReadTimeout; if (d > 0) {
            wholeReqDeadline = t0.Add(d);
        }
    }
    c.rwc.SetReadDeadline(hdrDeadline);
    {
        var d = c.server.WriteTimeout; if (d > 0) {
            defer(() => {
                c.rwc.SetWriteDeadline(time.Now().Add(d));
            });
        }
    }
    c.r.setReadLimit(c.server.initialReadLimitSize());
    if (c.lastMethod == "POST"u8) {
        // RFC 7230 section 3 tolerance for old buggy clients.
        (peek, _) = c.bufr.Peek(4);
        // ReadRequest will get err below
        c.bufr.Discard(numLeadingCRorLF(peek));
    }
    (req, err) = readRequest(c.bufr);
    if (err != default!) {
        if (c.r.hitReadLimit()) {
            return (default!, errTooLarge);
        }
        return (default!, err);
    }
    if (!http1ServerSupportsRequest(req)) {
        return (default!, new statusError(StatusHTTPVersionNotSupported, "unsupported protocol version"));
    }
    c.lastMethod = req.val.Method;
    c.r.setInfiniteReadLimit();
    var hosts = (~req).Header["Host"u8];
    var haveHost = (~req).Header["Host"u8];
    var isH2Upgrade = req.isH2Upgrade();
    if (req.ProtoAtLeast(1, 1) && (!haveHost || len(hosts) == 0) && !isH2Upgrade && (~req).Method != "CONNECT"u8) {
        return (default!, badRequestError("missing required Host header"u8));
    }
    if (len(hosts) == 1 && !httpguts.ValidHostHeader(hosts[0])) {
        return (default!, badRequestError("malformed Host header"u8));
    }
    foreach (var (k, vv) in (~req).Header) {
        if (!httpguts.ValidHeaderFieldName(k)) {
            return (default!, badRequestError("invalid header name"u8));
        }
        foreach (var (_, v) in vv) {
            if (!httpguts.ValidHeaderFieldValue(v)) {
                return (default!, badRequestError("invalid header value"u8));
            }
        }
    }
    delete((~req).Header, "Host"u8);
    (ctx, cancelCtx) = context.WithCancel(ctx);
    req.val.ctx = ctx;
    req.val.RemoteAddr = c.remoteAddr;
    req.val.TLS = c.tlsState;
    {
        var (body, ok) = (~req).Body._<body.val>(ᐧ); if (ok) {
            body.val.doEarlyClose = true;
        }
    }
    // Adjust the read deadline if necessary.
    if (!hdrDeadline.Equal(wholeReqDeadline)) {
        c.rwc.SetReadDeadline(wholeReqDeadline);
    }
    w = Ꮡ(new response(
        conn: c,
        cancelCtx: cancelCtx,
        req: req,
        reqBody: (~req).Body,
        handlerHeader: new ΔHeader(),
        contentLength: -1,
        closeNotifyCh: new channel<bool>(1), // We populate these ahead of time so we're not
 // reading from req.Header after their Handler starts
 // and maybe mutates it (Issue 14940)

        wants10KeepAlive: req.wantsHttp10KeepAlive(),
        wantsClose: req.wantsClose()
    ));
    if (isH2Upgrade) {
        w.val.closeAfterReply = true;
    }
    (~w).cw.res = w;
    w.val.w = newBufioWriterSize((~w).cw, bufferBeforeChunkingSize);
    return (w, default!);
});

// http1ServerSupportsRequest reports whether Go's HTTP/1.x server
// supports the given request.
internal static bool http1ServerSupportsRequest(ж<Request> Ꮡreq) {
    ref var req = ref Ꮡreq.val;

    if (req.ProtoMajor == 1) {
        return true;
    }
    // Accept "PRI * HTTP/2.0" upgrade requests, so Handlers can
    // wire up their own HTTP/2 upgrades.
    if (req.ProtoMajor == 2 && req.ProtoMinor == 0 && req.Method == "PRI"u8 && req.RequestURI == "*"u8) {
        return true;
    }
    // Reject HTTP/0.x, and all other HTTP/2+ requests (which
    // aren't encoded in ASCII anyway).
    return false;
}

[GoRecv] internal static ΔHeader Header(this ref response w) {
    if (w.cw.header == default! && w.wroteHeader && !w.cw.wroteHeader) {
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
// in order to keep a connection alive. If there are more bytes
// than this, the server, to be paranoid, instead sends a
// "Connection close" response.
//
// This number is approximately what a typical machine's TCP buffer
// size is anyway.  (if we have the bytes on the machine, we might as
// well read them)
internal static readonly UntypedInt maxPostHandlerReadBytes = /* 256 << 10 */ 262144;

internal static void checkWriteHeaderCode(nint code) {
    // Issue 22880: require valid WriteHeader status codes.
    // For now we only enforce that it's three digits.
    // In the future we might block things over 599 (600 and above aren't defined
    // at https://httpwg.org/specs/rfc7231.html#status.codes).
    // But for now any three digits.
    //
    // We used to send "HTTP/1.1 000 0" on the wire in responses but there's
    // no equivalent bogus thing we can realistically send in HTTP/2,
    // so we'll consistently panic instead and help people find their bugs
    // early. (We can't return an error from WriteHeader even if we wanted to.)
    if (code < 100 || code > 999) {
        throw panic(fmt.Sprintf("invalid WriteHeader code %v"u8, code));
    }
}

// relevantCaller searches the call stack for the first function outside of net/http.
// The purpose of this function is to provide more helpful error messages.
internal static runtime.Frame relevantCaller() {
    var pc = new slice<uintptr>(16);
    nint n = runtime.Callers(1, pc);
    var frames = runtime.CallersFrames(pc[..(int)(n)]);
    runtime.Frame frame = default!;
    while (ᐧ) {
        var (frameΔ1, more) = frames.Next();
        if (!strings.HasPrefix(frameΔ1.Function, "net/http."u8)) {
            return frameΔ1;
        }
        if (!more) {
            break;
        }
    }
    return frame;
}

[GoRecv] internal static void WriteHeader(this ref response w, nint code) {
    if (w.conn.hijacked()) {
        var caller = relevantCaller();
        w.conn.server.logf("http: response.WriteHeader on hijacked connection from %s (%s:%d)"u8, caller.Function, path.Base(caller.File), caller.Line);
        return;
    }
    if (w.wroteHeader) {
        var caller = relevantCaller();
        w.conn.server.logf("http: superfluous response.WriteHeader call from %s (%s:%d)"u8, caller.Function, path.Base(caller.File), caller.Line);
        return;
    }
    checkWriteHeaderCode(code);
    if (code < 101 || code > 199) {
        // Sending a 100 Continue or any non-1xx header disables the
        // automatically-sent 100 Continue from Request.Body.Read.
        w.disableWriteContinue();
    }
    // Handle informational headers.
    //
    // We shouldn't send any further headers after 101 Switching Protocols,
    // so it takes the non-informational path.
    if (code >= 100 && code <= 199 && code != StatusSwitchingProtocols) {
        writeStatusLine(w.conn.bufw, w.req.ProtoAtLeast(1, 1), code, w.statusBuf[..]);
        // Per RFC 8297 we must not clear the current header map
        w.handlerHeader.WriteSubset(~w.conn.bufw, excludedHeadersNoBody);
        w.conn.bufw.Write(crlf);
        w.conn.bufw.Flush();
        return;
    }
    w.wroteHeader = true;
    w.status = code;
    if (w.calledHeader && w.cw.header == default!) {
        w.cw.header = w.handlerHeader.Clone();
    }
    {
        @string cl = w.handlerHeader.get("Content-Length"u8); if (cl != ""u8) {
            var (v, err) = strconv.ParseInt(cl, 10, 64);
            if (err == default! && v >= 0){
                w.contentLength = v;
            } else {
                w.conn.server.logf("http: invalid Content-Length of %q"u8, cl);
                w.handlerHeader.Del("Content-Length"u8);
            }
        }
    }
}

// extraHeader is the set of headers sometimes added by chunkWriter.writeHeader.
// This type is used to avoid extra allocations from cloning and/or populating
// the response Header map and all its 1-element slices.
[GoType] partial struct extraHeader {
    internal @string contentType;
    internal @string connection;
    internal @string transferEncoding;
    internal slice<byte> date; // written if not nil
    internal slice<byte> contentLength; // written if not nil
}

// Sorted the same as extraHeader.Write's loop.
internal static slice<slice<byte>> extraHeaderKeys = new slice<byte>[]{
    slice<byte>("Content-Type"),
    slice<byte>("Connection"),
    slice<byte>("Transfer-Encoding")
}.slice();

internal static slice<byte> headerContentLength = slice<byte>("Content-Length: ");
internal static slice<byte> headerDate = slice<byte>("Date: ");

// Write writes the headers described in h to w.
//
// This method has a value receiver, despite the somewhat large size
// of h, because it prevents an allocation. The escape analysis isn't
// smart enough to realize this function doesn't mutate h.
internal static void Write(this extraHeader h, ж<bufio.Writer> Ꮡw) {
    ref var w = ref Ꮡw.val;

    if (h.date != default!) {
        w.Write(headerDate);
        w.Write(h.date);
        w.Write(crlf);
    }
    if (h.contentLength != default!) {
        w.Write(headerContentLength);
        w.Write(h.contentLength);
        w.Write(crlf);
    }
    foreach (var (i, v) in new @string[]{h.contentType, h.connection, h.transferEncoding}.slice()) {
        if (v != ""u8) {
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
[GoRecv] internal static void writeHeader(this ref chunkWriter cw, slice<byte> p) {
    if (cw.wroteHeader) {
        return;
    }
    cw.wroteHeader = true;
    var w = cw.res;
    var keepAlivesEnabled = (~(~w).conn).server.doKeepAlives();
    var isHEAD = (~(~w).req).Method == "HEAD"u8;
    // header is written out to w.conn.buf below. Depending on the
    // state of the handler, we either own the map or not. If we
    // don't own it, the exclude map is created lazily for
    // WriteSubset to remove headers. The setHeader struct holds
    // headers we need to add.
    var header = cw.header;
    var owned = header != default!;
    if (!owned) {
        header = w.val.handlerHeader;
    }
    map<@string, bool> excludeHeader = default!;
    var delHeader = 
    var excludeHeaderʗ1 = excludeHeader;
    var headerʗ1 = header;
    (@string key) => {
        if (owned) {
            headerʗ1.Del(key);
            return;
        }
        {
            var _ = headerʗ1[key];
            var ok = headerʗ1[key]; if (!ok) {
                return;
            }
        }
        if (excludeHeaderʗ1 == default!) {
            excludeHeaderʗ1 = new map<@string, bool>();
        }
        excludeHeaderʗ1[key] = true;
    };
    extraHeader setHeader = default!;
    // Don't write out the fake "Trailer:foo" keys. See TrailerPrefix.
    var trailers = false;
    foreach (var (k, _) in cw.header) {
        if (strings.HasPrefix(k, TrailerPrefix)) {
            if (excludeHeader == default!) {
                excludeHeader = new map<@string, bool>();
            }
            excludeHeader[k] = true;
            trailers = true;
        }
    }
    foreach (var (_, v) in cw.header["Trailer"u8]) {
        trailers = true;
        foreachHeaderElement(v, cw.res.declareTrailer);
    }
    @string te = header.get("Transfer-Encoding"u8);
    var hasTE = te != ""u8;
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
    if ((~w).handlerDone.Load() && !trailers && !hasTE && bodyAllowedForStatus((~w).status) && !header.has("Content-Length"u8) && (!isHEAD || len(p) > 0)) {
        w.val.contentLength = ((int64)len(p));
        setHeader.contentLength = strconv.AppendInt(cw.res.clenBuf[..0], ((int64)len(p)), 10);
    }
    // If this was an HTTP/1.0 request with keep-alive and we sent a
    // Content-Length back, we can make this a keep-alive response ...
    if ((~w).wants10KeepAlive && keepAlivesEnabled) {
        var sentLength = header.get("Content-Length"u8) != ""u8;
        if (sentLength && header.get("Connection"u8) == "keep-alive"u8) {
            w.val.closeAfterReply = false;
        }
    }
    // Check for an explicit (and valid) Content-Length header.
    var hasCL = (~w).contentLength != -1;
    if ((~w).wants10KeepAlive && (isHEAD || hasCL || !bodyAllowedForStatus((~w).status))){
        var _ = header["Connection"u8];
        var connectionHeaderSet = header["Connection"u8];
        if (!connectionHeaderSet) {
            setHeader.connection = "keep-alive"u8;
        }
    } else 
    if (!(~w).req.ProtoAtLeast(1, 1) || (~w).wantsClose) {
        w.val.closeAfterReply = true;
    }
    if (header.get("Connection"u8) == "close"u8 || !keepAlivesEnabled) {
        w.val.closeAfterReply = true;
    }
    // If the client wanted a 100-continue but we never sent it to
    // them (or, more strictly: we never finished reading their
    // request body), don't reuse this connection.
    //
    // This behavior was first added on the theory that we don't know
    // if the next bytes on the wire are going to be the remainder of
    // the request body or the subsequent request (see issue 11549),
    // but that's not correct: If we keep using the connection,
    // the client is required to send the request body whether we
    // asked for it or not.
    //
    // We probably do want to skip reusing the connection in most cases,
    // however. If the client is offering a large request body that we
    // don't intend to use, then it's better to close the connection
    // than to read the body. For now, assume that if we're sending
    // headers, the handler is done reading the body and we should
    // drop the connection if we haven't seen EOF.
    {
        var (ecr, ok) = (~(~w).req).Body._<expectContinueReader.val>(ᐧ); if (ok && !(~ecr).sawEOF.Load()) {
            w.val.closeAfterReply = true;
        }
    }
    // We do this by default because there are a number of clients that
    // send a full request before starting to read the response, and they
    // can deadlock if we start writing the response with unconsumed body
    // remaining. See Issue 15527 for some history.
    //
    // If full duplex mode has been enabled with ResponseController.EnableFullDuplex,
    // then leave the request body alone.
    //
    // We don't take this path when w.closeAfterReply is set.
    // We may not need to consume the request to get ready for the next one
    // (since we're closing the conn), but a client which sends a full request
    // before reading a response may deadlock in this case.
    // This behavior has been present since CL 5268043 (2011), however,
    // so it doesn't seem to be causing problems.
    if ((~(~w).req).ContentLength != 0 && !(~w).closeAfterReply && !(~w).fullDuplex) {
        bool discard = default!;
        bool tooBig = default!;
        switch ((~(~w).req).Body.type()) {
        case expectContinueReader.val bdy: {
            break;
        }
        case body.val bdy: {
            (~bdy).mu.Lock();
            switch (ᐧ) {
            case {} when (~bdy).closed: {
                if (!(~bdy).sawEOF) {
                    // We only get here if we have already fully consumed the request body
                    // (see above).
                    // Body was closed in handler with non-EOF error.
                    w.val.closeAfterReply = true;
                }
                break;
            }
            case {} when bdy.unreadDataSizeLocked() >= maxPostHandlerReadBytes: {
                tooBig = true;
                break;
            }
            default: {
                discard = true;
                break;
            }}

            (~bdy).mu.Unlock();
            break;
        }
        default: {
            var bdy = (~(~w).req).Body.type();
            discard = true;
            break;
        }}
        if (discard) {
            var (_, err) = io.CopyN(io.Discard, (~w).reqBody, maxPostHandlerReadBytes + 1);
            var exprᴛ1 = err;
            if (exprᴛ1 == default!) {
                tooBig = true;
            }
            else if (exprᴛ1 == ErrBodyReadAfterClose) {
            }
            else if (exprᴛ1 == io.EOF) {
                err = (~w).reqBody.Close();
                if (err != default!) {
                    // There must be even more data left over.
                    // Body was already consumed and closed.
                    // The remaining body was just consumed, close it.
                    w.val.closeAfterReply = true;
                }
            }
            else { /* default: */
                w.val.closeAfterReply = true;
            }

        }
        // Some other kind of error occurred, like a read timeout, or
        // corrupt chunked encoding. In any case, whatever remains
        // on the wire must not be parsed as another HTTP request.
        if (tooBig) {
            w.requestTooLarge();
            delHeader("Connection"u8);
            setHeader.connection = "close"u8;
        }
    }
    nint code = w.val.status;
    if (bodyAllowedForStatus(code)){
        // If no content type, apply sniffing algorithm to body.
        var _ = header["Content-Type"u8];
        var haveType = header["Content-Type"u8];
        // If the Content-Encoding was set and is non-blank,
        // we shouldn't sniff the body. See Issue 31753.
        @string ce = header.Get("Content-Encoding"u8);
        var hasCE = len(ce) > 0;
        if (!hasCE && !haveType && !hasTE && len(p) > 0) {
            setHeader.contentType = DetectContentType(p);
        }
    } else {
        foreach (var (_, k) in suppressedHeaders(code)) {
            delHeader(k);
        }
    }
    if (!header.has("Date"u8)) {
        setHeader.date = appendTime(cw.res.dateBuf[..0], time.Now());
    }
    if (hasCL && hasTE && te != "identity"u8) {
        // TODO: return an error if WriteHeader gets a return parameter
        // For now just ignore the Content-Length.
        (~(~w).conn).server.logf("http: WriteHeader called with both Transfer-Encoding of %q and a Content-Length of %d"u8,
            te, (~w).contentLength);
        delHeader("Content-Length"u8);
        hasCL = false;
    }
    if ((~(~w).req).Method == "HEAD"u8 || !bodyAllowedForStatus(code) || code == StatusNoContent){
        // Response has no body.
        delHeader("Transfer-Encoding"u8);
    } else 
    if (hasCL){
        // Content-Length has been provided, so no chunking is to be done.
        delHeader("Transfer-Encoding"u8);
    } else 
    if ((~w).req.ProtoAtLeast(1, 1)){
        // HTTP/1.1 or greater: Transfer-Encoding has been set to identity, and no
        // content-length has been provided. The connection must be closed after the
        // reply is written, and no chunking is to be done. This is the setup
        // recommended in the Server-Sent Events candidate recommendation 11,
        // section 8.
        if (hasTE && te == "identity"u8){
            cw.chunking = false;
            w.val.closeAfterReply = true;
            delHeader("Transfer-Encoding"u8);
        } else {
            // HTTP/1.1 or greater: use chunked transfer encoding
            // to avoid closing the connection at EOF.
            cw.chunking = true;
            setHeader.transferEncoding = "chunked"u8;
            if (hasTE && te == "chunked"u8) {
                // We will send the chunked Transfer-Encoding header later.
                delHeader("Transfer-Encoding"u8);
            }
        }
    } else {
        // HTTP version < 1.1: cannot do chunked transfer
        // encoding and we don't know the Content-Length so
        // signal EOF by closing connection.
        w.val.closeAfterReply = true;
        delHeader("Transfer-Encoding"u8);
    }
    // in case already set
    // Cannot use Content-Length with non-identity Transfer-Encoding.
    if (cw.chunking) {
        delHeader("Content-Length"u8);
    }
    if (!(~w).req.ProtoAtLeast(1, 0)) {
        return;
    }
    // Only override the Connection header if it is not a successful
    // protocol switch response and if KeepAlives are not enabled.
    // See https://golang.org/issue/36381.
    var delConnectionHeader = (~w).closeAfterReply && (!keepAlivesEnabled || !hasToken(cw.header.get("Connection"u8), "close"u8)) && !isProtocolSwitchResponse((~w).status, header);
    if (delConnectionHeader) {
        delHeader("Connection"u8);
        if ((~w).req.ProtoAtLeast(1, 1)) {
            setHeader.connection = "close"u8;
        }
    }
    writeStatusLine((~(~w).conn).bufw, (~w).req.ProtoAtLeast(1, 1), code, (~w).statusBuf[..]);
    cw.header.WriteSubset(~(~(~w).conn).bufw, excludeHeader);
    setHeader.Write((~(~w).conn).bufw);
    (~(~w).conn).bufw.Write(crlf);
}

// foreachHeaderElement splits v according to the "#rule" construction
// in RFC 7230 section 7 and calls fn for each non-empty element.
internal static void foreachHeaderElement(@string v, Action<@string> fn) {
    v = textproto.TrimString(v);
    if (v == ""u8) {
        return;
    }
    if (!strings.Contains(v, ","u8)) {
        fn(v);
        return;
    }
    foreach (var (_, f) in strings.Split(v, ","u8)) {
        {
            f = textproto.TrimString(f); if (f != ""u8) {
                fn(f);
            }
        }
    }
}

// writeStatusLine writes an HTTP/1.x Status-Line (RFC 7230 Section 3.1.2)
// to bw. is11 is whether the HTTP request is HTTP/1.1. false means HTTP/1.0.
// code is the response status code.
// scratch is an optional scratch buffer. If it has at least capacity 3, it's used.
internal static void writeStatusLine(ж<bufio.Writer> Ꮡbw, bool is11, nint code, slice<byte> scratch) {
    ref var bw = ref Ꮡbw.val;

    if (is11){
        bw.WriteString("HTTP/1.1 "u8);
    } else {
        bw.WriteString("HTTP/1.0 "u8);
    }
    {
        @string text = StatusText(code); if (text != ""u8){
            bw.Write(strconv.AppendInt(scratch[..0], ((int64)code), 10));
            bw.WriteByte((rune)' ');
            bw.WriteString(text);
            bw.WriteString("\r\n"u8);
        } else {
            // don't worry about performance
            fmt.Fprintf(~bw, "%03d status code %d\r\n"u8, code, code);
        }
    }
}

// bodyAllowed reports whether a Write is allowed for this response type.
// It's illegal to call this before the header has been flushed.
[GoRecv] internal static bool bodyAllowed(this ref response w) {
    if (!w.wroteHeader) {
        throw panic("");
    }
    return bodyAllowedForStatus(w.status);
}

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
//  1. *response (the ResponseWriter) ->
//  2. (*response).w, a [*bufio.Writer] of bufferBeforeChunkingSize bytes ->
//  3. chunkWriter.Writer (whose writeHeader finalizes Content-Length/Type)
//     and which writes the chunk headers, if needed ->
//  4. conn.bufw, a *bufio.Writer of default (4kB) bytes, writing to ->
//  5. checkConnErrorWriter{c}, which notes any non-nil error on Write
//     and populates c.werr with it if so, but otherwise writes to ->
//  6. the rwc, the [net.Conn].
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
[GoRecv] internal static (nint n, error err) Write(this ref response w, slice<byte> data) {
    nint n = default!;
    error err = default!;

    return w.write(len(data), data, ""u8);
}

[GoRecv] internal static (nint n, error err) WriteString(this ref response w, @string data) {
    nint n = default!;
    error err = default!;

    return w.write(len(data), default!, data);
}

// either dataB or dataS is non-zero.
[GoRecv] internal static (nint n, error err) write(this ref response w, nint lenData, slice<byte> dataB, @string dataS) {
    nint n = default!;
    error err = default!;

    if (w.conn.hijacked()) {
        if (lenData > 0) {
            var caller = relevantCaller();
            w.conn.server.logf("http: response.Write on hijacked connection from %s (%s:%d)"u8, caller.Function, path.Base(caller.File), caller.Line);
        }
        return (0, ErrHijacked);
    }
    if (w.canWriteContinue.Load()) {
        // Body reader wants to write 100 Continue but hasn't yet. Tell it not to.
        w.disableWriteContinue();
    }
    if (!w.wroteHeader) {
        w.WriteHeader(StatusOK);
    }
    if (lenData == 0) {
        return (0, default!);
    }
    if (!w.bodyAllowed()) {
        return (0, ErrBodyNotAllowed);
    }
    w.written += ((int64)lenData);
    // ignoring errors, for errorKludge
    if (w.contentLength != -1 && w.written > w.contentLength) {
        return (0, ErrContentLength);
    }
    if (dataB != default!){
        return w.w.Write(dataB);
    } else {
        return w.w.WriteString(dataS);
    }
}

[GoRecv] internal static void finishRequest(this ref response w) {
    w.handlerDone.Store(true);
    if (!w.wroteHeader) {
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
    if (w.req.MultipartForm != nil) {
        w.req.MultipartForm.RemoveAll();
    }
}

// shouldReuseConnection reports whether the underlying TCP connection can be reused.
// It must only be called after the handler is done executing.
[GoRecv] internal static bool shouldReuseConnection(this ref response w) {
    if (w.closeAfterReply) {
        // The request or something set while executing the
        // handler indicated we shouldn't reuse this
        // connection.
        return false;
    }
    if (w.req.Method != "HEAD"u8 && w.contentLength != -1 && w.bodyAllowed() && w.contentLength != w.written) {
        // Did not write enough. Avoid getting out of sync.
        return false;
    }
    // There was some error writing to the underlying connection
    // during the request, so don't re-use this conn.
    if (w.conn.werr != default!) {
        return false;
    }
    if (w.closedRequestBodyEarly()) {
        return false;
    }
    return true;
}

[GoRecv] internal static bool closedRequestBodyEarly(this ref response w) {
    var (body, ok) = w.req.Body._<body.val>(ᐧ);
    return ok && body.didEarlyClose();
}

[GoRecv] internal static void Flush(this ref response w) {
    w.FlushError();
}

[GoRecv] internal static error FlushError(this ref response w) {
    if (!w.wroteHeader) {
        w.WriteHeader(StatusOK);
    }
    var err = w.w.Flush();
    var e2 = w.cw.flush();
    if (err == default!) {
        err = e2;
    }
    return err;
}

[GoRecv] internal static void finalFlush(this ref conn c) {
    if (c.bufr != nil) {
        // Steal the bufio.Reader (~4KB worth of memory) and its associated
        // reader for a future connection.
        putBufioReader(c.bufr);
        c.bufr = default!;
    }
    if (c.bufw != nil) {
        c.bufw.Flush();
        // Steal the bufio.Writer (~4KB worth of memory) and its associated
        // writer for a future connection.
        putBufioWriter(c.bufw);
        c.bufw = default!;
    }
}

// Close the connection.
[GoRecv] internal static void close(this ref conn c) {
    c.finalFlush();
    c.rwc.Close();
}

// rstAvoidanceDelay is the amount of time we sleep after closing the
// write side of a TCP connection before closing the entire socket.
// By sleeping, we increase the chances that the client sees our FIN
// and processes its final data before they process the subsequent RST
// from closing a connection with known unread data.
// This RST seems to occur mostly on BSD systems. (And Windows?)
// This timeout is somewhat arbitrary (~latency around the planet),
// and may be modified by tests.
//
// TODO(bcmills): This should arguably be a server configuration parameter,
// not a hard-coded value.
internal static time.Duration rstAvoidanceDelay = 500 * time.Millisecond;

[GoType] partial interface closeWriter {
    error CloseWrite();
}

internal static closeWriter _ᴛ4ʗ = (ж<net.TCPConn>)(default!);

// closeWriteAndWait flushes any outstanding data and sends a FIN packet (if
// client is connected via TCP), signaling that we're done. We then
// pause for a bit, hoping the client processes it before any
// subsequent RST.
//
// See https://golang.org/issue/3595
[GoRecv] internal static void closeWriteAndWait(this ref conn c) {
    c.finalFlush();
    {
        var (tcp, ok) = c.rwc._<closeWriter>(ᐧ); if (ok) {
            tcp.CloseWrite();
        }
    }
    // When we return from closeWriteAndWait, the caller will fully close the
    // connection. If client is still writing to the connection, this will cause
    // the write to fail with ECONNRESET or similar. Unfortunately, many TCP
    // implementations will also drop unread packets from the client's read buffer
    // when a write fails, causing our final response to be truncated away too.
    //
    // As a result, https://www.rfc-editor.org/rfc/rfc7230#section-6.6 recommends
    // that “[t]he server … continues to read from the connection until it
    // receives a corresponding close by the client, or until the server is
    // reasonably certain that its own TCP stack has received the client's
    // acknowledgement of the packet(s) containing the server's last response.”
    //
    // Unfortunately, we have no straightforward way to be “reasonably certain”
    // that we have received the client's ACK, and at any rate we don't want to
    // allow a misbehaving client to soak up server connections indefinitely by
    // withholding an ACK, nor do we want to go through the complexity or overhead
    // of using low-level APIs to figure out when a TCP round-trip has completed.
    //
    // Instead, we declare that we are “reasonably certain” that we received the
    // ACK if maxRSTAvoidanceDelay has elapsed.
    time.Sleep(rstAvoidanceDelay);
}

// validNextProto reports whether the proto is a valid ALPN protocol name.
// Everything is valid except the empty string and built-in protocol types,
// so that those can't be overridden with alternate implementations.
internal static bool validNextProto(@string proto) {
    var exprᴛ1 = proto;
    if (exprᴛ1 == ""u8 || exprᴛ1 == "http/1.1"u8 || exprᴛ1 == "http/1.0"u8) {
        return false;
    }

    return true;
}

internal const bool runHooks = true;
internal const bool skipHooks = false;

[GoRecv] internal static void setState(this ref conn c, net.Conn nc, ConnState state, bool runHook) {
    var srv = c.server;
    var exprᴛ1 = state;
    if (exprᴛ1 == StateNew) {
        srv.trackConn(c, true);
    }
    else if (exprᴛ1 == StateHijacked || exprᴛ1 == StateClosed) {
        srv.trackConn(c, false);
    }

    if (state > 255 || state < 0) {
        throw panic("internal error");
    }
    var packedState = (uint64)(((uint64)(time.Now().Unix() << (int)(8))) | ((uint64)state));
    c.curState.Store(packedState);
    if (!runHook) {
        return;
    }
    {
        var hook = srv.val.ConnState; if (hook != default!) {
            hook(nc, state);
        }
    }
}

[GoRecv] internal static (ConnState state, int64 unixSec) getState(this ref conn c) {
    ConnState state = default!;
    int64 unixSec = default!;

    var packedState = c.curState.Load();
    return (((ConnState)((uint64)(packedState & 255))), ((int64)(packedState >> (int)(8))));
}

// badRequestError is a literal string (used by in the server in HTML,
// unescaped) to tell the user why their request was bad. It should
// be plain text without user info or other embedded errors.
internal static error badRequestError(@string e) {
    return new statusError(StatusBadRequest, e);
}

// statusError is an error used to respond to a request with an HTTP status.
// The text should be plain text without user info or other embedded errors.
[GoType] partial struct statusError {
    internal nint code;
    internal @string text;
}

internal static @string Error(this statusError e) {
    return StatusText(e.code) + ": "u8 + e.text;
}

// ErrAbortHandler is a sentinel panic value to abort a handler.
// While any panic from ServeHTTP aborts the response to the client,
// panicking with ErrAbortHandler also suppresses logging of a stack
// trace to the server's error log.
public static error ErrAbortHandler = errors.New("net/http: abort Handler"u8);

// isCommonNetReadError reports whether err is a common error
// encountered during reading a request off the network when the
// client has gone away or had its read fail somehow. This is used to
// determine which logs are interesting enough to log about.
internal static bool isCommonNetReadError(error err) {
    if (AreEqual(err, io.EOF)) {
        return true;
    }
    {
        var (neterr, ok) = err._<netꓸError>(ᐧ); if (ok && neterr.Timeout()) {
            return true;
        }
    }
    {
        var (oe, ok) = err._<ж<net.OpError>>(ᐧ); if (ok && (~oe).Op == "read"u8) {
            return true;
        }
    }
    return false;
}

// Serve a new connection.
[GoRecv] internal static void serve(this ref conn c, context.Context ctx) => func((defer, recover) => {
    {
        var ra = c.rwc.RemoteAddr(); if (ra != default!) {
            c.remoteAddr = ra.String();
        }
    }
    ctx = context.WithValue(ctx, LocalAddrContextKey, c.rwc.LocalAddr());
    ж<response> inFlightResponse = default!;
    var inFlightResponseʗ1 = inFlightResponse;
    defer(() => {
        {
            var err = recover(); if (err != default! && !AreEqual(err, ErrAbortHandler)) {
                static readonly UntypedInt size = /* 64 << 10 */ 65536;
                var buf = new slice<byte>(size);
                buf = buf[..(int)(runtime.Stack(buf, false))];
                c.server.logf("http: panic serving %v: %v\n%s"u8, c.remoteAddr, err, buf);
            }
        }
        if (inFlightResponseʗ1 != nil) {
            (~inFlightResponseʗ1).cancelCtx();
            inFlightResponseʗ1.disableWriteContinue();
        }
        if (!c.hijacked()) {
            if (inFlightResponseʗ1 != nil) {
                (~(~inFlightResponseʗ1).conn).r.abortPendingRead();
                (~inFlightResponseʗ1).reqBody.Close();
            }
            c.close();
            c.setState(c.rwc, StateClosed, runHooks);
        }
    });
    {
        var (tlsConn, ok) = c.rwc._<ж<tls.Conn>>(ᐧ); if (ok) {
            var tlsTO = c.server.tlsHandshakeTimeout();
            if (tlsTO > 0) {
                var dl = time.Now().Add(tlsTO);
                c.rwc.SetReadDeadline(dl);
                c.rwc.SetWriteDeadline(dl);
            }
            {
                var err = tlsConn.HandshakeContext(ctx); if (err != default!) {
                    // If the handshake failed due to the client not speaking
                    // TLS, assume they're speaking plaintext HTTP and write a
                    // 400 response on the TLS conn's underlying net.Conn.
                    @string reason = default!;
                    {
                        var (re, okΔ1) = err._<tls.RecordHeaderError>(ᐧ); if (okΔ1 && re.Conn != default! && tlsRecordHeaderLooksLikeHTTP(re.RecordHeader)){
                            io.WriteString(re.Conn, "HTTP/1.0 400 Bad Request\r\n\r\nClient sent an HTTP request to an HTTPS server.\n"u8);
                            re.Conn.Close();
                            reason = "client sent an HTTP request to an HTTPS server"u8;
                        } else {
                            reason = err.Error();
                        }
                    }
                    c.server.logf("http: TLS handshake error from %s: %v"u8, c.rwc.RemoteAddr(), reason);
                    return;
                }
            }
            // Restore Conn-level deadlines.
            if (tlsTO > 0) {
                c.rwc.SetReadDeadline(new time.Time(nil));
                c.rwc.SetWriteDeadline(new time.Time(nil));
            }
            c.tlsState = @new<tlsꓸConnectionState>();
            c.tlsState.val = tlsConn.ConnectionState();
            {
                @string proto = c.tlsState.NegotiatedProtocol; if (validNextProto(proto)) {
                    {
                        var fn = c.server.TLSNextProto[proto]; if (fn != default!) {
                            var h = new initALPNRequest(ctx, tlsConn, new serverHandler(c.server));
                            // Mark freshly created HTTP/2 as active and prevent any server state hooks
                            // from being run on these connections. This prevents closeIdleConns from
                            // closing such connections. See issue https://golang.org/issue/39776.
                            c.setState(c.rwc, StateActive, skipHooks);
                            fn(c.server, tlsConn, h);
                        }
                    }
                    return;
                }
            }
        }
    }
    // HTTP/1.x from here on.
    (ctx, cancelCtx) = context.WithCancel(ctx);
    c.cancelCtx = cancelCtx;
    var cancelCtxʗ1 = cancelCtx;
    defer(cancelCtxʗ1);
    c.r = Ꮡ(new connReader(conn: c));
    c.bufr = newBufioReader(~c.r);
    c.bufw = newBufioWriterSize(new checkConnErrorWriter(c), 4 << (int)(10));
    while (ᐧ) {
        (w, err) = c.readRequest(ctx);
        if (c.r.remain != c.server.initialReadLimitSize()) {
            // If we read any bytes off the wire, we're active.
            c.setState(c.rwc, StateActive, runHooks);
        }
        if (err != default!) {
            @string errorHeaders = "\r\nContent-Type: text/plain; charset=utf-8\r\nConnection: close\r\n\r\n"u8;
            switch (ᐧ) {
            case {} when err is errTooLarge: {
                // Their HTTP client may or may not be
                // able to read this if we're
                // responding to them and hanging up
                // while they're still writing their
                // request. Undefined behavior.
                @string publicErr = "431 Request Header Fields Too Large"u8;
                fmt.Fprintf(c.rwc, "HTTP/1.1 " + publicErr + errorHeaders + publicErr);
                c.closeWriteAndWait();
                return;
            }
            case {} when isUnsupportedTEError(err): {
                nint code = StatusNotImplemented;
                fmt.Fprintf(c.rwc, // Respond as per RFC 7230 Section 3.3.1 which says,
 //      A server that receives a request message with a
 //      transfer coding it does not understand SHOULD
 //      respond with 501 (Unimplemented).
 // We purposefully aren't echoing back the transfer-encoding's value,
 // so as to mitigate the risk of cross side scripting by an attacker.
 "HTTP/1.1 %d %s%sUnsupported transfer encoding"u8, code, StatusText(code), errorHeaders);
                return;
            }
            case {} when isCommonNetReadError(err): {
                return;
            }
            default: {
                {
                    var (v, ok) = err._<statusError>(ᐧ); if (ok) {
                        // don't reply
                        fmt.Fprintf(c.rwc, "HTTP/1.1 %d %s: %s%s%d %s: %s"u8, v.code, StatusText(v.code), v.text, errorHeaders, v.code, StatusText(v.code), v.text);
                        return;
                    }
                }
                @string publicErr = "400 Bad Request"u8;
                fmt.Fprintf(c.rwc, "HTTP/1.1 " + publicErr + errorHeaders + publicErr);
                return;
            }}

        }
        // Expect 100 Continue support
        var req = w.val.req;
        if (req.expectsContinue()){
            if (req.ProtoAtLeast(1, 1) && (~req).ContentLength != 0) {
                // Wrap the Body reader with one that replies on the connection
                req.val.Body = Ꮡ(new expectContinueReader(readCloser: (~req).Body, resp: w));
                (~w).canWriteContinue.Store(true);
            }
        } else 
        if ((~req).Header.get("Expect"u8) != ""u8) {
            w.sendExpectationFailed();
            return;
        }
        c.curReq.Store(w);
        if (requestBodyRemains((~req).Body)){
            registerOnHitEOF((~req).Body, (~(~w).conn).r.startBackgroundRead);
        } else {
            (~(~w).conn).r.startBackgroundRead();
        }
        // HTTP cannot have multiple simultaneous active requests.[*]
        // Until the server replies to this request, it can't read another,
        // so we might as well run the handler in this goroutine.
        // [*] Not strictly true: HTTP pipelining. We could let them all process
        // in parallel even if their responses need to be serialized.
        // But we're not going to implement HTTP pipelining because it
        // was never deployed in the wild and the answer is HTTP/2.
        inFlightResponse = w;
        new serverHandler(c.server).ServeHTTP(~w, (~w).req);
        inFlightResponse = default!;
        (~w).cancelCtx();
        if (c.hijacked()) {
            return;
        }
        w.finishRequest();
        c.rwc.SetWriteDeadline(new time.Time(nil));
        if (!w.shouldReuseConnection()) {
            if ((~w).requestBodyLimitHit || w.closedRequestBodyEarly()) {
                c.closeWriteAndWait();
            }
            return;
        }
        c.setState(c.rwc, StateIdle, runHooks);
        c.curReq.Store(nil);
        if (!(~(~w).conn).server.doKeepAlives()) {
            // We're in shutdown mode. We might've replied
            // to the user without "Connection: close" and
            // they might think they can send another
            // request, but such is life with HTTP/1.1.
            return;
        }
        {
            var d = c.server.idleTimeout(); if (d > 0){
                c.rwc.SetReadDeadline(time.Now().Add(d));
            } else {
                c.rwc.SetReadDeadline(new time.Time(nil));
            }
        }
        // Wait for the connection to become readable again before trying to
        // read the next request. This prevents a ReadHeaderTimeout or
        // ReadTimeout from starting until the first bytes of the next request
        // have been received.
        {
            (_, errΔ1) = c.bufr.Peek(4); if (errΔ1 != default!) {
                return;
            }
        }
        c.rwc.SetReadDeadline(new time.Time(nil));
    }
});

[GoRecv] internal static void sendExpectationFailed(this ref response w) {
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
    w.Header().Set("Connection"u8, "close"u8);
    w.WriteHeader(StatusExpectationFailed);
    w.finishRequest();
}

// Hijack implements the [Hijacker.Hijack] method. Our response is both a [ResponseWriter]
// and a [Hijacker].
[GoRecv] internal static (net.Conn rwc, ж<bufio.ReadWriter> buf, error err) Hijack(this ref response w) => func((defer, _) => {
    net.Conn rwc = default!;
    ж<bufio.ReadWriter> buf = default!;
    error err = default!;

    if (w.handlerDone.Load()) {
        throw panic("net/http: Hijack called after ServeHTTP finished");
    }
    w.disableWriteContinue();
    if (w.wroteHeader) {
        w.cw.flush();
    }
    var c = w.conn;
    (~c).mu.Lock();
    var cʗ1 = c;
    defer((~cʗ1).mu.Unlock);
    // Release the bufioWriter that writes to the chunk writer, it is not
    // used after a connection has been hijacked.
    (rwc, buf, err) = c.hijackLocked();
    if (err == default!) {
        putBufioWriter(w.w);
        w.w = default!;
    }
    return (rwc, buf, err);
});

[GoRecv] internal static /*<-*/channel<bool> CloseNotify(this ref response w) {
    if (w.handlerDone.Load()) {
        throw panic("net/http: CloseNotify called after ServeHTTP finished");
    }
    return w.closeNotifyCh;
}

internal static void registerOnHitEOF(io.ReadCloser rc, Action fn) {
    switch (rc.type()) {
    case expectContinueReader.val v: {
        registerOnHitEOF((~v).readCloser, fn);
        break;
    }
    case body.val v: {
        v.registerOnHitEOF(fn);
        break;
    }
    default: {
        var v = rc.type();
        throw panic("unexpected type "u8 + fmt.Sprintf("%T"u8, rc));
        break;
    }}
}

// requestBodyRemains reports whether future calls to Read
// on rc might yield more data.
internal static bool requestBodyRemains(io.ReadCloser rc) {
    if (rc == NoBody) {
        return false;
    }
    switch (rc.type()) {
    case expectContinueReader.val v: {
        return requestBodyRemains((~v).readCloser);
    }
    case body.val v: {
        return v.bodyRemains();
    }
    default: {
        var v = rc.type();
        throw panic("unexpected type "u8 + fmt.Sprintf("%T"u8, rc));
        break;
    }}
}

public delegate void HandlerFunc(ResponseWriter _, ж<Request> _);

// ServeHTTP calls f(w, r).
public static void ServeHTTP(this HandlerFunc f, ResponseWriter w, ж<Request> Ꮡr) {
    ref var r = ref Ꮡr.val;

    f(w, Ꮡr);
}

// Helper handlers

// Error replies to the request with the specified error message and HTTP code.
// It does not otherwise end the request; the caller should ensure no further
// writes are done to w.
// The error message should be plain text.
//
// Error deletes the Content-Length header,
// sets Content-Type to “text/plain; charset=utf-8”,
// and sets X-Content-Type-Options to “nosniff”.
// This configures the header properly for the error message,
// in case the caller had set it up expecting a successful output.
public static void Error(ResponseWriter w, @string error, nint code) {
    var h = w.Header();
    // Delete the Content-Length header, which might be for some other content.
    // Assuming the error string fits in the writer's buffer, we'll figure
    // out the correct Content-Length for it later.
    //
    // We don't delete Content-Encoding, because some middleware sets
    // Content-Encoding: gzip and wraps the ResponseWriter to compress on-the-fly.
    // See https://go.dev/issue/66343.
    h.Del("Content-Length"u8);
    // There might be content type already set, but we reset it to
    // text/plain for the error message.
    h.Set("Content-Type"u8, "text/plain; charset=utf-8"u8);
    h.Set("X-Content-Type-Options"u8, "nosniff"u8);
    w.WriteHeader(code);
    fmt.Fprintln(w, error);
}

// NotFound replies to the request with an HTTP 404 not found error.
public static void NotFound(ResponseWriter w, ж<Request> Ꮡr) {
    ref var r = ref Ꮡr.val;

    Error(w, "404 page not found"u8, StatusNotFound);
}

// NotFoundHandler returns a simple request handler
// that replies to each request with a “404 page not found” reply.
public static ΔHandler NotFoundHandler() {
    return ((HandlerFunc)NotFound);
}

// StripPrefix returns a handler that serves HTTP requests by removing the
// given prefix from the request URL's Path (and RawPath if set) and invoking
// the handler h. StripPrefix handles a request for a path that doesn't begin
// with prefix by replying with an HTTP 404 not found error. The prefix must
// match exactly: if the prefix in the request contains escaped characters
// the reply is also an HTTP 404 not found error.
public static ΔHandler StripPrefix(@string prefix, ΔHandler h) {
    if (prefix == ""u8) {
        return h;
    }
    return ((HandlerFunc)((ResponseWriter w, ж<Request> r) => {
        @string p = strings.TrimPrefix((~(~r).URL).Path, prefix);
        @string rp = strings.TrimPrefix((~(~r).URL).RawPath, prefix);
        if (len(p) < len((~(~r).URL).Path) && ((~(~r).URL).RawPath == ""u8 || len(rp) < len((~(~r).URL).RawPath))){
            var r2 = @new<Request>();
            r2.val = r.val;
            r2.val.URL = @new<url.URL>();
            (~r2).URL.val = (~r).URL.val;
            (~r2).URL.val.Path = p;
            (~r2).URL.val.RawPath = rp;
            h.ServeHTTP(w, r2);
        } else {
            NotFound(w, r);
        }
    }));
}

// Redirect replies to the request with a redirect to url,
// which may be a path relative to the request path.
//
// The provided code should be in the 3xx range and is usually
// [StatusMovedPermanently], [StatusFound] or [StatusSeeOther].
//
// If the Content-Type header has not been set, [Redirect] sets it
// to "text/html; charset=utf-8" and writes a small HTML body.
// Setting the Content-Type header to any value, including nil,
// disables that behavior.
public static void Redirect(ResponseWriter w, ж<Request> Ꮡr, @string url, nint code) {
    ref var r = ref Ꮡr.val;

    {
        (u, err) = urlpkg.Parse(url); if (err == default!) {
            // If url was relative, make its path absolute by
            // combining with request path.
            // The client would probably do this for us,
            // but doing it ourselves is more reliable.
            // See RFC 7231, section 7.1.2
            if ((~u).Scheme == ""u8 && (~u).Host == ""u8) {
                @string oldpath = r.URL.Path;
                if (oldpath == ""u8) {
                    // should not happen, but avoid a crash if it does
                    oldpath = "/"u8;
                }
                // no leading http://server
                if (url == ""u8 || url[0] != (rune)'/') {
                    // make relative path absolute
                    var (olddir, _) = path.Split(oldpath);
                    url = olddir + url;
                }
                @string query = default!;
                {
                    nint i = strings.Index(url, "?"u8); if (i != -1) {
                        (url, query) = (url[..(int)(i)], url[(int)(i)..]);
                    }
                }
                // clean up but preserve trailing slash
                var trailing = strings.HasSuffix(url, "/"u8);
                url = path.Clean(url);
                if (trailing && !strings.HasSuffix(url, "/"u8)) {
                    url += "/"u8;
                }
                url += query;
            }
        }
    }
    var h = w.Header();
    // RFC 7231 notes that a short HTML body is usually included in
    // the response because older user agents may not understand 301/307.
    // Do it only if the request didn't already have a Content-Type header.
    var _ = h["Content-Type"u8];
    var hadCT = h["Content-Type"u8];
    h.Set("Location"u8, hexEscapeNonASCII(url));
    if (!hadCT && (r.Method == "GET"u8 || r.Method == "HEAD"u8)) {
        h.Set("Content-Type"u8, "text/html; charset=utf-8"u8);
    }
    w.WriteHeader(code);
    // Shouldn't send the body for POST or HEAD; that leaves GET.
    if (!hadCT && r.Method == "GET"u8) {
        @string body = "<a href=\""u8 + htmlEscape(url) + "\">"u8 + StatusText(code) + "</a>.\n"u8;
        fmt.Fprintln(w, body);
    }
}

// "&#34;" is shorter than "&quot;".
// "&#39;" is shorter than "&apos;" and apos was not in HTML until HTML5.
internal static ж<strings.Replacer> htmlReplacer = strings.NewReplacer(
    "&"u8, "&amp;",
    "<", "&lt;",
    ">", "&gt;",
    @"""", "&#34;",
    "'", "&#39;");

internal static @string htmlEscape(@string s) {
    return htmlReplacer.Replace(s);
}

// Redirect to a fixed URL
[GoType] partial struct redirectHandler {
    internal @string url;
    internal nint code;
}

[GoRecv] internal static void ServeHTTP(this ref redirectHandler rh, ResponseWriter w, ж<Request> Ꮡr) {
    ref var r = ref Ꮡr.val;

    Redirect(w, Ꮡr, rh.url, rh.code);
}

// RedirectHandler returns a request handler that redirects
// each request it receives to the given url using the given
// status code.
//
// The provided code should be in the 3xx range and is usually
// [StatusMovedPermanently], [StatusFound] or [StatusSeeOther].
public static ΔHandler RedirectHandler(@string url, nint code) {
    return new redirectHandler(url, code);
}

// ServeMux is an HTTP request multiplexer.
// It matches the URL of each incoming request against a list of registered
// patterns and calls the handler for the pattern that
// most closely matches the URL.
//
// # Patterns
//
// Patterns can match the method, host and path of a request.
// Some examples:
//
//   - "/index.html" matches the path "/index.html" for any host and method.
//   - "GET /static/" matches a GET request whose path begins with "/static/".
//   - "example.com/" matches any request to the host "example.com".
//   - "example.com/{$}" matches requests with host "example.com" and path "/".
//   - "/b/{bucket}/o/{objectname...}" matches paths whose first segment is "b"
//     and whose third segment is "o". The name "bucket" denotes the second
//     segment and "objectname" denotes the remainder of the path.
//
// In general, a pattern looks like
//
//	[METHOD ][HOST]/[PATH]
//
// All three parts are optional; "/" is a valid pattern.
// If METHOD is present, it must be followed by at least one space or tab.
//
// Literal (that is, non-wildcard) parts of a pattern match
// the corresponding parts of a request case-sensitively.
//
// A pattern with no method matches every method. A pattern
// with the method GET matches both GET and HEAD requests.
// Otherwise, the method must match exactly.
//
// A pattern with no host matches every host.
// A pattern with a host matches URLs on that host only.
//
// A path can include wildcard segments of the form {NAME} or {NAME...}.
// For example, "/b/{bucket}/o/{objectname...}".
// The wildcard name must be a valid Go identifier.
// Wildcards must be full path segments: they must be preceded by a slash and followed by
// either a slash or the end of the string.
// For example, "/b_{bucket}" is not a valid pattern.
//
// Normally a wildcard matches only a single path segment,
// ending at the next literal slash (not %2F) in the request URL.
// But if the "..." is present, then the wildcard matches the remainder of the URL path, including slashes.
// (Therefore it is invalid for a "..." wildcard to appear anywhere but at the end of a pattern.)
// The match for a wildcard can be obtained by calling [Request.PathValue] with the wildcard's name.
// A trailing slash in a path acts as an anonymous "..." wildcard.
//
// The special wildcard {$} matches only the end of the URL.
// For example, the pattern "/{$}" matches only the path "/",
// whereas the pattern "/" matches every path.
//
// For matching, both pattern paths and incoming request paths are unescaped segment by segment.
// So, for example, the path "/a%2Fb/100%25" is treated as having two segments, "a/b" and "100%".
// The pattern "/a%2fb/" matches it, but the pattern "/a/b/" does not.
//
// # Precedence
//
// If two or more patterns match a request, then the most specific pattern takes precedence.
// A pattern P1 is more specific than P2 if P1 matches a strict subset of P2’s requests;
// that is, if P2 matches all the requests of P1 and more.
// If neither is more specific, then the patterns conflict.
// There is one exception to this rule, for backwards compatibility:
// if two patterns would otherwise conflict and one has a host while the other does not,
// then the pattern with the host takes precedence.
// If a pattern passed to [ServeMux.Handle] or [ServeMux.HandleFunc] conflicts with
// another pattern that is already registered, those functions panic.
//
// As an example of the general rule, "/images/thumbnails/" is more specific than "/images/",
// so both can be registered.
// The former matches paths beginning with "/images/thumbnails/"
// and the latter will match any other path in the "/images/" subtree.
//
// As another example, consider the patterns "GET /" and "/index.html":
// both match a GET request for "/index.html", but the former pattern
// matches all other GET and HEAD requests, while the latter matches any
// request for "/index.html" that uses a different method.
// The patterns conflict.
//
// # Trailing-slash redirection
//
// Consider a [ServeMux] with a handler for a subtree, registered using a trailing slash or "..." wildcard.
// If the ServeMux receives a request for the subtree root without a trailing slash,
// it redirects the request by adding the trailing slash.
// This behavior can be overridden with a separate registration for the path without
// the trailing slash or "..." wildcard. For example, registering "/images/" causes ServeMux
// to redirect a request for "/images" to "/images/", unless "/images" has
// been registered separately.
//
// # Request sanitizing
//
// ServeMux also takes care of sanitizing the URL request path and the Host
// header, stripping the port number and redirecting any request containing . or
// .. segments or repeated slashes to an equivalent, cleaner URL.
//
// # Compatibility
//
// The pattern syntax and matching behavior of ServeMux changed significantly
// in Go 1.22. To restore the old behavior, set the GODEBUG environment variable
// to "httpmuxgo121=1". This setting is read once, at program startup; changes
// during execution will be ignored.
//
// The backwards-incompatible changes include:
//   - Wildcards are just ordinary literal path segments in 1.21.
//     For example, the pattern "/{x}" will match only that path in 1.21,
//     but will match any one-segment path in 1.22.
//   - In 1.21, no pattern was rejected, unless it was empty or conflicted with an existing pattern.
//     In 1.22, syntactically invalid patterns will cause [ServeMux.Handle] and [ServeMux.HandleFunc] to panic.
//     For example, in 1.21, the patterns "/{"  and "/a{x}" match themselves,
//     but in 1.22 they are invalid and will cause a panic when registered.
//   - In 1.22, each segment of a pattern is unescaped; this was not done in 1.21.
//     For example, in 1.22 the pattern "/%61" matches the path "/a" ("%61" being the URL escape sequence for "a"),
//     but in 1.21 it would match only the path "/%2561" (where "%25" is the escape for the percent sign).
//   - When matching patterns to paths, in 1.22 each segment of the path is unescaped; in 1.21, the entire path is unescaped.
//     This change mostly affects how paths with %2F escapes adjacent to slashes are treated.
//     See https://go.dev/issue/21955 for details.
[GoType] partial struct ServeMux {
    internal sync_package.RWMutex mu;
    internal routingNode tree;
    internal routingIndex index;
    internal slice<ж<pattern>> patterns; // TODO(jba): remove if possible
    internal serveMux121 mux121; // used only when GODEBUG=httpmuxgo121=1
}

// NewServeMux allocates and returns a new [ServeMux].
public static ж<ServeMux> NewServeMux() {
    return Ꮡ(new ServeMux(nil));
}

// DefaultServeMux is the default [ServeMux] used by [Serve].
public static ж<ServeMux> DefaultServeMux = Ꮡ(defaultServeMux);

internal static ServeMux defaultServeMux;

// cleanPath returns the canonical path for p, eliminating . and .. elements.
internal static @string cleanPath(@string p) {
    if (p == ""u8) {
        return "/"u8;
    }
    if (p[0] != (rune)'/') {
        p = "/"u8 + p;
    }
    @string np = path.Clean(p);
    // path.Clean removes trailing slash except for root;
    // put the trailing slash back if necessary.
    if (p[len(p) - 1] == (rune)'/' && np != "/"u8) {
        // Fast path for common case of p being the string we want:
        if (len(p) == len(np) + 1 && strings.HasPrefix(p, np)){
            np = p;
        } else {
            np += "/"u8;
        }
    }
    return np;
}

// stripHostPort returns h without any trailing ":<port>".
internal static @string stripHostPort(@string h) {
    // If no port on host, return unchanged
    if (!strings.Contains(h, ":"u8)) {
        return h;
    }
    var (host, _, err) = net.SplitHostPort(h);
    if (err != default!) {
        return h;
    }
    // on error, return unchanged
    return host;
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
// the path that will match after following the redirect.
//
// If there is no registered handler that applies to the request,
// Handler returns a “page not found” handler and an empty pattern.
[GoRecv] public static (ΔHandler h, @string pattern) Handler(this ref ServeMux mux, ж<Request> Ꮡr) {
    ΔHandler h = default!;
    @string pattern = default!;

    ref var r = ref Ꮡr.val;
    if (use121) {
        return mux.mux121.findHandler(Ꮡr);
    }
    var (h, p, _, _) = mux.findHandler(Ꮡr);
    return (h, p);
}

// findHandler finds a handler for a request.
// If there is a matching handler, it returns it and the pattern that matched.
// Otherwise it returns a Redirect or NotFound handler with the path that would match
// after the redirect.
[GoRecv] public static (ΔHandler h, @string patStr, ж<pattern> _, slice<@string> matches) findHandler(this ref ServeMux mux, ж<Request> Ꮡr) {
    ΔHandler h = default!;
    @string patStr = default!;
    slice<@string> matches = default!;

    ref var r = ref Ꮡr.val;
    ж<routingNode> n = default!;
    @string host = r.URL.Host;
    @string escapedPath = r.URL.EscapedPath();
    @string path = escapedPath;
    // CONNECT requests are not canonicalized.
    if (r.Method == "CONNECT"u8){
        // If r.URL.Path is /tree and its handler is not registered,
        // the /tree -> /tree/ redirect applies to CONNECT requests
        // but the path canonicalization does not.
        (_, _, uΔ1) = mux.matchOrRedirect(host, r.Method, path, r.URL);
        if (uΔ1 != nil) {
            return (RedirectHandler(uΔ1.String(), StatusMovedPermanently), (~uΔ1).Path, default!, default!);
        }
        // Redo the match, this time with r.Host instead of r.URL.Host.
        // Pass a nil URL to skip the trailing-slash redirect logic.
        (n, matches, _) = mux.matchOrRedirect(r.Host, r.Method, path, nil);
    } else {
        // All other requests have any port stripped and path cleaned
        // before passing to mux.handler.
        host = stripHostPort(r.Host);
        path = cleanPath(path);
        // If the given path is /tree and its handler is not registered,
        // redirect for /tree/.
        ж<url.URL> u = default!;
        (n, matches, u) = mux.matchOrRedirect(host, r.Method, path, r.URL);
        if (u != nil) {
            return (RedirectHandler(u.String(), StatusMovedPermanently), (~u).Path, default!, default!);
        }
        if (path != escapedPath) {
            // Redirect to cleaned path.
            @string patStrΔ1 = ""u8;
            if (n != nil) {
                patStrΔ1 = (~n).pattern.String();
            }
            var u = Ꮡ(new url.URL(Path: path, RawQuery: r.URL.RawQuery));
            return (RedirectHandler(u.String(), StatusMovedPermanently), patStrΔ1, default!, default!);
        }
    }
    if (n == nil) {
        // We didn't find a match with the request method. To distinguish between
        // Not Found and Method Not Allowed, see if there is another pattern that
        // matches except for the method.
        var allowedMethods = mux.matchingMethods(host, path);
        if (len(allowedMethods) > 0) {
            return (((HandlerFunc)(
            var allowedMethodsʗ1 = allowedMethods;
            (ResponseWriter w, ж<Request> r) => {
                w.Header().Set("Allow"u8, strings.Join(allowedMethodsʗ1, ", "u8));
                Error(w, StatusText(StatusMethodNotAllowed), StatusMethodNotAllowed);
            })), "", default!, default!);
        }
        return (NotFoundHandler(), "", default!, default!);
    }
    return ((~n).handler, (~n).pattern.String(), (~n).pattern, matches);
}

// matchOrRedirect looks up a node in the tree that matches the host, method and path.
//
// If the url argument is non-nil, handler also deals with trailing-slash
// redirection: when a path doesn't match exactly, the match is tried again
// after appending "/" to the path. If that second match succeeds, the last
// return value is the URL to redirect to.
[GoRecv] public static (ж<routingNode> _, slice<@string> matches, ж<url.URL> redirectTo) matchOrRedirect(this ref ServeMux mux, @string host, @string method, @string path, ж<url.URL> Ꮡu) => func((defer, _) => {
    slice<@string> matches = default!;
    ж<url.URL> redirectTo = default!;

    ref var u = ref Ꮡu.val;
    mux.mu.RLock();
    defer(mux.mu.RUnlock);
    (n, matches) = mux.tree.match(host, method, path);
    // If we have an exact match, or we were asked not to try trailing-slash redirection,
    // or the URL already has a trailing slash, then we're done.
    if (!exactMatch(n, path) && u != nil && !strings.HasSuffix(path, "/"u8)) {
        // If there is an exact match with a trailing slash, then redirect.
        path += "/"u8;
        (n2, _) = mux.tree.match(host, method, path);
        if (exactMatch(n2, path)) {
            return (default!, default!, Ꮡ(new url.URL(Path: cleanPath(u.Path) + "/"u8, RawQuery: u.RawQuery)));
        }
    }
    return (n, matches, default!);
});

// exactMatch reports whether the node's pattern exactly matches the path.
// As a special case, if the node is nil, exactMatch return false.
//
// Before wildcards were introduced, it was clear that an exact match meant
// that the pattern and path were the same string. The only other possibility
// was that a trailing-slash pattern, like "/", matched a path longer than
// it, like "/a".
//
// With wildcards, we define an inexact match as any one where a multi wildcard
// matches a non-empty string. All other matches are exact.
// For example, these are all exact matches:
//
//	pattern   path
//	/a        /a
//	/{x}      /a
//	/a/{$}    /a/
//	/a/       /a/
//
// The last case has a multi wildcard (implicitly), but the match is exact because
// the wildcard matches the empty string.
//
// Examples of matches that are not exact:
//
//	pattern   path
//	/         /a
//	/a/{x...} /a/b
internal static bool exactMatch(ж<routingNode> Ꮡn, @string path) {
    ref var n = ref Ꮡn.val;

    if (n == nil) {
        return false;
    }
    // We can't directly implement the definition (empty match for multi
    // wildcard) because we don't record a match for anonymous multis.
    // If there is no multi, the match is exact.
    if (!n.pattern.lastSegment().multi) {
        return true;
    }
    // If the path doesn't end in a trailing slash, then the multi match
    // is non-empty.
    if (len(path) > 0 && path[len(path) - 1] != (rune)'/') {
        return false;
    }
    // Only patterns ending in {$} or a multi wildcard can
    // match a path with a trailing slash.
    // For the match to be exact, the number of pattern
    // segments should be the same as the number of slashes in the path.
    // E.g. "/a/b/{$}" and "/a/b/{...}" exactly match "/a/b/", but "/a/" does not.
    return len(n.pattern.segments) == strings.Count(path, "/"u8);
}

// matchingMethods return a sorted list of all methods that would match with the given host and path.
[GoRecv] internal static slice<@string> matchingMethods(this ref ServeMux mux, @string host, @string path) => func((defer, _) => {
    // Hold the read lock for the entire method so that the two matches are done
    // on the same set of registered patterns.
    mux.mu.RLock();
    defer(mux.mu.RUnlock);
    var ms = new map<@string, bool>{};
    mux.tree.matchingMethods(host, path, ms);
    // matchOrRedirect will try appending a trailing slash if there is no match.
    if (!strings.HasSuffix(path, "/"u8)) {
        mux.tree.matchingMethods(host, path + "/"u8, ms);
    }
    return slices.Sorted(maps.Keys<@string>(ms));
});

// ServeHTTP dispatches the request to the handler whose
// pattern most closely matches the request URL.
[GoRecv] public static void ServeHTTP(this ref ServeMux mux, ResponseWriter w, ж<Request> Ꮡr) {
    ref var r = ref Ꮡr.val;

    if (r.RequestURI == "*"u8) {
        if (r.ProtoAtLeast(1, 1)) {
            w.Header().Set("Connection"u8, "close"u8);
        }
        w.WriteHeader(StatusBadRequest);
        return;
    }
    ΔHandler h = default!;
    if (use121){
        (h, _) = mux.mux121.findHandler(Ꮡr);
    } else {
        (h, r.Pattern, r.pat, r.matches) = mux.findHandler(Ꮡr);
    }
    h.ServeHTTP(w, Ꮡr);
}

// The four functions below all call ServeMux.register so that callerLocation
// always refers to user code.

// Handle registers the handler for the given pattern.
// If the given pattern conflicts, with one that is already registered, Handle
// panics.
[GoRecv] public static void Handle(this ref ServeMux mux, @string pattern, ΔHandler handler) {
    if (use121){
        mux.mux121.handle(pattern, handler);
    } else {
        mux.register(pattern, handler);
    }
}

// HandleFunc registers the handler function for the given pattern.
// If the given pattern conflicts, with one that is already registered, HandleFunc
// panics.
[GoRecv] public static void HandleFunc(this ref ServeMux mux, @string pattern, http.Request) handler) {
    if (use121){
        mux.mux121.handleFunc(pattern, handler);
    } else {
        mux.register(pattern, ((HandlerFunc)handler));
    }
}

// Handle registers the handler for the given pattern in [DefaultServeMux].
// The documentation for [ServeMux] explains how patterns are matched.
public static void Handle(@string pattern, ΔHandler handler) {
    if (use121){
        (~DefaultServeMux).mux121.handle(pattern, handler);
    } else {
        DefaultServeMux.register(pattern, handler);
    }
}

// HandleFunc registers the handler function for the given pattern in [DefaultServeMux].
// The documentation for [ServeMux] explains how patterns are matched.
public static void HandleFunc(@string pattern, http.Request) handler) {
    if (use121){
        (~DefaultServeMux).mux121.handleFunc(pattern, handler);
    } else {
        DefaultServeMux.register(pattern, ((HandlerFunc)handler));
    }
}

[GoRecv] internal static void register(this ref ServeMux mux, @string pattern, ΔHandler handler) {
    {
        var err = mux.registerErr(pattern, handler); if (err != default!) {
            throw panic(err);
        }
    }
}

[GoRecv] internal static error registerErr(this ref ServeMux mux, @string patstr, ΔHandler handler) => func((defer, _) => {
    if (patstr == ""u8) {
        return errors.New("http: invalid pattern"u8);
    }
    if (handler == default!) {
        return errors.New("http: nil handler"u8);
    }
    {
        var (f, okΔ1) = handler._<HandlerFunc>(ᐧ); if (okΔ1 && f == default!) {
            return errors.New("http: nil handler"u8);
        }
    }
    (pat, err) = parsePattern(patstr);
    if (err != default!) {
        return fmt.Errorf("parsing %q: %w"u8, patstr, err);
    }
    // Get the caller's location, for better conflict error messages.
    // Skip register and whatever calls it.
    var (_, file, line, ok) = runtime.Caller(3);
    if (!ok){
        pat.val.loc = "unknown location"u8;
    } else {
        pat.val.loc = fmt.Sprintf("%s:%d"u8, file, line);
    }
    mux.mu.Lock();
    defer(mux.mu.Unlock);
    // Check for conflict.
    {
        var errΔ1 = mux.index.possiblyConflictingPatterns(pat, 
        var patʗ1 = pat;
        (ж<pattern> pat2) => {
            if (patʗ1.conflictsWith(pat2)) {
                @string d = describeConflict(patʗ1, pat2);
                return fmt.Errorf("pattern %q (registered at %s) conflicts with pattern %q (registered at %s):\n%s"u8,
                    patʗ1, (~patʗ1).loc, pat2, (~pat2).loc, d);
            }
            return default!;
        }); if (errΔ1 != default!) {
            return errΔ1;
        }
    }
    mux.tree.addPattern(pat, handler);
    mux.index.addPattern(pat);
    mux.patterns = append(mux.patterns, pat);
    return default!;
});

// Serve accepts incoming HTTP connections on the listener l,
// creating a new service goroutine for each. The service goroutines
// read requests and then call handler to reply to them.
//
// The handler is typically nil, in which case [DefaultServeMux] is used.
//
// HTTP/2 support is only enabled if the Listener returns [*tls.Conn]
// connections and they were configured with "h2" in the TLS
// Config.NextProtos.
//
// Serve always returns a non-nil error.
public static error Serve(net.Listener l, ΔHandler handler) {
    var srv = Ꮡ(new Server(ΔHandler: handler));
    return srv.Serve(l);
}

// ServeTLS accepts incoming HTTPS connections on the listener l,
// creating a new service goroutine for each. The service goroutines
// read requests and then call handler to reply to them.
//
// The handler is typically nil, in which case [DefaultServeMux] is used.
//
// Additionally, files containing a certificate and matching private key
// for the server must be provided. If the certificate is signed by a
// certificate authority, the certFile should be the concatenation
// of the server's certificate, any intermediates, and the CA's certificate.
//
// ServeTLS always returns a non-nil error.
public static error ServeTLS(net.Listener l, ΔHandler handler, @string certFile, @string keyFile) {
    var srv = Ꮡ(new Server(ΔHandler: handler));
    return srv.ServeTLS(l, certFile, keyFile);
}

// A Server defines parameters for running an HTTP server.
// The zero value for Server is a valid configuration.
[GoType] partial struct Server {
    // Addr optionally specifies the TCP address for the server to listen on,
    // in the form "host:port". If empty, ":http" (port 80) is used.
    // The service names are defined in RFC 6335 and assigned by IANA.
    // See net.Dial for details of the address format.
    public @string Addr;
    public ΔHandler Handler; // handler to invoke, http.DefaultServeMux if nil
    // DisableGeneralOptionsHandler, if true, passes "OPTIONS *" requests to the Handler,
    // otherwise responds with 200 OK and Content-Length: 0.
    public bool DisableGeneralOptionsHandler;
    // TLSConfig optionally provides a TLS configuration for use
    // by ServeTLS and ListenAndServeTLS. Note that this value is
    // cloned by ServeTLS and ListenAndServeTLS, so it's not
    // possible to modify the configuration with methods like
    // tls.Config.SetSessionTicketKeys. To use
    // SetSessionTicketKeys, use Server.Serve with a TLS Listener
    // instead.
    public ж<crypto.tls_package.Config> TLSConfig;
    // ReadTimeout is the maximum duration for reading the entire
    // request, including the body. A zero or negative value means
    // there will be no timeout.
    //
    // Because ReadTimeout does not let Handlers make per-request
    // decisions on each request body's acceptable deadline or
    // upload rate, most users will prefer to use
    // ReadHeaderTimeout. It is valid to use them both.
    public time_package.Duration ReadTimeout;
    // ReadHeaderTimeout is the amount of time allowed to read
    // request headers. The connection's read deadline is reset
    // after reading the headers and the Handler can decide what
    // is considered too slow for the body. If zero, the value of
    // ReadTimeout is used. If negative, or if zero and ReadTimeout
    // is zero or negative, there is no timeout.
    public time_package.Duration ReadHeaderTimeout;
    // WriteTimeout is the maximum duration before timing out
    // writes of the response. It is reset whenever a new
    // request's header is read. Like ReadTimeout, it does not
    // let Handlers make decisions on a per-request basis.
    // A zero or negative value means there will be no timeout.
    public time_package.Duration WriteTimeout;
    // IdleTimeout is the maximum amount of time to wait for the
    // next request when keep-alives are enabled. If zero, the value
    // of ReadTimeout is used. If negative, or if zero and ReadTimeout
    // is zero or negative, there is no timeout.
    public time_package.Duration IdleTimeout;
    // MaxHeaderBytes controls the maximum number of bytes the
    // server will read parsing the request header's keys and
    // values, including the request line. It does not limit the
    // size of the request body.
    // If zero, DefaultMaxHeaderBytes is used.
    public nint MaxHeaderBytes;
    // TLSNextProto optionally specifies a function to take over
    // ownership of the provided TLS connection when an ALPN
    // protocol upgrade has occurred. The map key is the protocol
    // name negotiated. The Handler argument should be used to
    // handle HTTP requests and will initialize the Request's TLS
    // and RemoteAddr if not already set. The connection is
    // automatically closed when the function returns.
    // If TLSNextProto is not nil, HTTP/2 support is not enabled
    // automatically.
    public http.Handler) TLSNextProto;
    // ConnState specifies an optional callback function that is
    // called when a client connection changes state. See the
    // ConnState type and associated constants for details.
    public Action<net.Conn, ConnState> ConnState;
    // ErrorLog specifies an optional logger for errors accepting
    // connections, unexpected behavior from handlers, and
    // underlying FileSystem errors.
    // If nil, logging is done via the log package's standard logger.
    public ж<log_package.Logger> ErrorLog;
    // BaseContext optionally specifies a function that returns
    // the base context for incoming requests on this server.
    // The provided Listener is the specific Listener that's
    // about to start accepting requests.
    // If BaseContext is nil, the default is context.Background().
    // If non-nil, it must return a non-nil context.
    public Func<net.Listener, context.Context> BaseContext;
    // ConnContext optionally specifies a function that modifies
    // the context used for a new connection c. The provided ctx
    // is derived from the base context and has a ServerContextKey
    // value.
    public Func<context.Context, net.Conn, context.Context> ConnContext;
    internal sync.atomic_package.Bool inShutdown; // true when server is in shutdown
    internal sync.atomic_package.Bool disableKeepAlives;
    internal sync_package.Once nextProtoOnce; // guards setupHTTP2_* init
    internal error nextProtoErr;     // result of http2.ConfigureServer if used
    internal sync_package.Mutex mu;
    internal map<ж<net.Listener>, EmptyStruct> listeners;
    internal map<ж<conn>, EmptyStruct> activeConn;
    internal slice<Action> onShutdown;
    internal sync_package.WaitGroup listenerGroup;
}

// Close immediately closes all active net.Listeners and any
// connections in state [StateNew], [StateActive], or [StateIdle]. For a
// graceful shutdown, use [Server.Shutdown].
//
// Close does not attempt to close (and does not even know about)
// any hijacked connections, such as WebSockets.
//
// Close returns any error returned from closing the [Server]'s
// underlying Listener(s).
[GoRecv] public static error Close(this ref Server srv) => func((defer, _) => {
    srv.inShutdown.Store(true);
    srv.mu.Lock();
    defer(srv.mu.Unlock);
    var err = srv.closeListenersLocked();
    // Unlock srv.mu while waiting for listenerGroup.
    // The group Add and Done calls are made with srv.mu held,
    // to avoid adding a new listener in the window between
    // us setting inShutdown above and waiting here.
    srv.mu.Unlock();
    srv.listenerGroup.Wait();
    srv.mu.Lock();
    foreach (var (c, _) in srv.activeConn) {
        (~c).rwc.Close();
        delete(srv.activeConn, c);
    }
    return err;
});

// shutdownPollIntervalMax is the max polling interval when checking
// quiescence during Server.Shutdown. Polling starts with a small
// interval and backs off to the max.
// Ideally we could find a solution that doesn't involve polling,
// but which also doesn't have a high runtime cost (and doesn't
// involve any contentious mutexes), but that is left as an
// exercise for the reader.
internal static readonly time.Duration shutdownPollIntervalMax = /* 500 * time.Millisecond */ 500000000;

// Shutdown gracefully shuts down the server without interrupting any
// active connections. Shutdown works by first closing all open
// listeners, then closing all idle connections, and then waiting
// indefinitely for connections to return to idle and then shut down.
// If the provided context expires before the shutdown is complete,
// Shutdown returns the context's error, otherwise it returns any
// error returned from closing the [Server]'s underlying Listener(s).
//
// When Shutdown is called, [Serve], [ListenAndServe], and
// [ListenAndServeTLS] immediately return [ErrServerClosed]. Make sure the
// program doesn't exit and waits instead for Shutdown to return.
//
// Shutdown does not attempt to close nor wait for hijacked
// connections such as WebSockets. The caller of Shutdown should
// separately notify such long-lived connections of shutdown and wait
// for them to close, if desired. See [Server.RegisterOnShutdown] for a way to
// register shutdown notification functions.
//
// Once Shutdown has been called on a server, it may not be reused;
// future calls to methods such as Serve will return ErrServerClosed.
[GoRecv] public static error Shutdown(this ref Server srv, context.Context ctx) => func((defer, _) => {
    srv.inShutdown.Store(true);
    srv.mu.Lock();
    var lnerr = srv.closeListenersLocked();
    foreach (var (_, f) in srv.onShutdown) {
        var fʗ1 = f;
        goǃ(fʗ1);
    }
    srv.mu.Unlock();
    srv.listenerGroup.Wait();
    var pollIntervalBase = time.Millisecond;
    var nextPollInterval = 
    () => {
        // Add 10% jitter.
        var interval = pollIntervalBase + ((time.Duration)rand.Intn(((nint)(pollIntervalBase / 10))));
        // Double and clamp for next time.
        pollIntervalBase *= 2;
        if (pollIntervalBase > shutdownPollIntervalMax) {
            pollIntervalBase = shutdownPollIntervalMax;
        }
        return interval;
    };
    var timer = time.NewTimer(nextPollInterval());
    var timerʗ1 = timer;
    defer(timerʗ1.Stop);
    while (ᐧ) {
        if (srv.closeIdleConns()) {
            return lnerr;
        }
        switch (select(ᐸꟷ(ctx.Done(), ꓸꓸꓸ), ᐸꟷ((~timer).C, ꓸꓸꓸ))) {
        case 0 when ctx.Done().ꟷᐳ(out _): {
            return ctx.Err();
        }
        case 1 when (~timer).C.ꟷᐳ(out _): {
            timer.Reset(nextPollInterval());
            break;
        }}
    }
});

// RegisterOnShutdown registers a function to call on [Server.Shutdown].
// This can be used to gracefully shutdown connections that have
// undergone ALPN protocol upgrade or that have been hijacked.
// This function should start protocol-specific graceful shutdown,
// but should not wait for shutdown to complete.
[GoRecv] public static void RegisterOnShutdown(this ref Server srv, Action f) {
    srv.mu.Lock();
    srv.onShutdown = append(srv.onShutdown, f);
    srv.mu.Unlock();
}

// closeIdleConns closes all idle connections and reports whether the
// server is quiescent.
[GoRecv] internal static bool closeIdleConns(this ref Server s) => func((defer, _) => {
    s.mu.Lock();
    defer(s.mu.Unlock);
    var quiescent = true;
    foreach (var (c, _) in s.activeConn) {
        var (st, unixSec) = c.getState();
        // Issue 22682: treat StateNew connections as if
        // they're idle if we haven't read the first request's
        // header in over 5 seconds.
        if (st == StateNew && unixSec < time.Now().Unix() - 5) {
            st = StateIdle;
        }
        if (st != StateIdle || unixSec == 0) {
            // Assume unixSec == 0 means it's a very new
            // connection, without state set yet.
            quiescent = false;
            continue;
        }
        (~c).rwc.Close();
        delete(s.activeConn, c);
    }
    return quiescent;
});

[GoRecv] internal static error closeListenersLocked(this ref Server s) {
    error err = default!;
    foreach (var (ln, _) in s.listeners) {
        {
            var cerr = (ж<ж<net.Listener>>).Close(); if (cerr != default! && err == default!) {
                err = cerr;
            }
        }
    }
    return err;
}

[GoType("num:nint")] partial struct ConnState;

public static readonly ConnState StateNew = /* iota */ 0;
public static readonly ConnState StateActive = 1;
public static readonly ConnState StateIdle = 2;
public static readonly ConnState StateHijacked = 3;
public static readonly ConnState StateClosed = 4;

internal static map<ConnState, @string> stateName = new map<ConnState, @string>{
    [StateNew] = "new"u8,
    [StateActive] = "active"u8,
    [StateIdle] = "idle"u8,
    [StateHijacked] = "hijacked"u8,
    [StateClosed] = "closed"u8
};

public static @string String(this ConnState c) {
    return stateName[c];
}

// serverHandler delegates to either the server's Handler or
// DefaultServeMux and also handles "OPTIONS *" requests.
[GoType] partial struct serverHandler {
    internal ж<Server> srv;
}

// ServeHTTP should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/erda-project/erda-infra
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname badServeHTTP net/http.serverHandler.ServeHTTP
internal static void ServeHTTP(this serverHandler sh, ResponseWriter rw, ж<Request> Ꮡreq) {
    ref var req = ref Ꮡreq.val;

    var handler = sh.srv.Handler;
    if (handler == default!) {
        handler = ~DefaultServeMux;
    }
    if (!sh.srv.DisableGeneralOptionsHandler && req.RequestURI == "*"u8 && req.Method == "OPTIONS"u8) {
        handler = new globalOptionsHandler(nil);
    }
    handler.ServeHTTP(rw, Ꮡreq);
}

internal static partial void badServeHTTP(serverHandler _, ResponseWriter _, ж<Request> _);

// AllowQuerySemicolons returns a handler that serves requests by converting any
// unescaped semicolons in the URL query to ampersands, and invoking the handler h.
//
// This restores the pre-Go 1.17 behavior of splitting query parameters on both
// semicolons and ampersands. (See golang.org/issue/25192). Note that this
// behavior doesn't match that of many proxies, and the mismatch can lead to
// security issues.
//
// AllowQuerySemicolons should be invoked before [Request.ParseForm] is called.
public static ΔHandler AllowQuerySemicolons(ΔHandler h) {
    return ((HandlerFunc)((ResponseWriter w, ж<Request> r) => {
        if (strings.Contains((~(~r).URL).RawQuery, ";"u8)){
            var r2 = @new<Request>();
            r2.val = r.val;
            r2.val.URL = @new<url.URL>();
            (~r2).URL.val = (~r).URL.val;
            (~r2).URL.val.RawQuery = strings.ReplaceAll((~(~r).URL).RawQuery, ";"u8, "&"u8);
            h.ServeHTTP(w, r2);
        } else {
            h.ServeHTTP(w, r);
        }
    }));
}

// ListenAndServe listens on the TCP network address srv.Addr and then
// calls [Serve] to handle requests on incoming connections.
// Accepted connections are configured to enable TCP keep-alives.
//
// If srv.Addr is blank, ":http" is used.
//
// ListenAndServe always returns a non-nil error. After [Server.Shutdown] or [Server.Close],
// the returned error is [ErrServerClosed].
[GoRecv] public static error ListenAndServe(this ref Server srv) {
    if (srv.shuttingDown()) {
        return ErrServerClosed;
    }
    @string addr = srv.Addr;
    if (addr == ""u8) {
        addr = ":http"u8;
    }
    (ln, err) = net.Listen("tcp"u8, addr);
    if (err != default!) {
        return err;
    }
    return srv.Serve(ln);
}

internal static Action<ж<Server>, net.Listener> testHookServerServe;                       // used if non-nil

// shouldConfigureHTTP2ForServe reports whether Server.Serve should configure
// automatic HTTP/2. (which sets up the srv.TLSNextProto map)
[GoRecv] internal static bool shouldConfigureHTTP2ForServe(this ref Server srv) {
    if (srv.TLSConfig == nil) {
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
    return slices.Contains(srv.TLSConfig.NextProtos, http2NextProtoTLS);
}

// ErrServerClosed is returned by the [Server.Serve], [ServeTLS], [ListenAndServe],
// and [ListenAndServeTLS] methods after a call to [Server.Shutdown] or [Server.Close].
public static error ErrServerClosed = errors.New("http: Server closed"u8);

// Serve accepts incoming connections on the Listener l, creating a
// new service goroutine for each. The service goroutines read requests and
// then call srv.Handler to reply to them.
//
// HTTP/2 support is only enabled if the Listener returns [*tls.Conn]
// connections and they were configured with "h2" in the TLS
// Config.NextProtos.
//
// Serve always returns a non-nil error and closes l.
// After [Server.Shutdown] or [Server.Close], the returned error is [ErrServerClosed].
[GoRecv] public static error Serve(this ref Server srv, net.Listener l) => func((defer, _) => {
    {
        var fn = testHookServerServe; if (fn != default!) {
            fn(srv, l);
        }
    }
    // call hook with unwrapped listener
    var origListener = l;
    Ꮡl = new onceCloseListener(Listener: l); l = ref Ꮡl.val;
    defer(l.Close);
    {
        var err = srv.setupHTTP2_Serve(); if (err != default!) {
            return err;
        }
    }
    if (!srv.trackListener(Ꮡ(l), true)) {
        return ErrServerClosed;
    }
    deferǃ(srv.trackListener, Ꮡ(l), false, defer);
    var baseCtx = context.Background();
    if (srv.BaseContext != default!) {
        baseCtx = srv.BaseContext(origListener);
        if (baseCtx == default!) {
            throw panic("BaseContext returned a nil context");
        }
    }
    time.Duration tempDelay = default!;                   // how long to sleep on accept failure
    var ctx = context.WithValue(baseCtx, ServerContextKey, srv);
    while (ᐧ) {
        (rw, err) = l.Accept();
        if (err != default!) {
            if (srv.shuttingDown()) {
                return ErrServerClosed;
            }
            {
                var (ne, ok) = err._<netꓸError>(ᐧ); if (ok && ne.Temporary()) {
                    if (tempDelay == 0){
                        tempDelay = 5 * time.Millisecond;
                    } else {
                        tempDelay *= 2;
                    }
                    {
                        var max = 1 * time.ΔSecond; if (tempDelay > max) {
                            tempDelay = max;
                        }
                    }
                    srv.logf("http: Accept error: %v; retrying in %v"u8, err, tempDelay);
                    time.Sleep(tempDelay);
                    continue;
                }
            }
            return err;
        }
        var connCtx = ctx;
        {
            var cc = srv.ConnContext; if (cc != default!) {
                connCtx = cc(connCtx, rw);
                if (connCtx == default!) {
                    throw panic("ConnContext returned nil");
                }
            }
        }
        tempDelay = 0;
        var c = srv.newConn(rw);
        c.setState((~c).rwc, StateNew, runHooks);
        // before Serve can return
        var cʗ1 = c;
        goǃ(cʗ1.serve, connCtx);
    }
});

// ServeTLS accepts incoming connections on the Listener l, creating a
// new service goroutine for each. The service goroutines perform TLS
// setup and then read requests, calling srv.Handler to reply to them.
//
// Files containing a certificate and matching private key for the
// server must be provided if neither the [Server]'s
// TLSConfig.Certificates, TLSConfig.GetCertificate nor
// config.GetConfigForClient are populated.
// If the certificate is signed by a certificate authority, the
// certFile should be the concatenation of the server's certificate,
// any intermediates, and the CA's certificate.
//
// ServeTLS always returns a non-nil error. After [Server.Shutdown] or [Server.Close], the
// returned error is [ErrServerClosed].
[GoRecv] public static error ServeTLS(this ref Server srv, net.Listener l, @string certFile, @string keyFile) {
    // Setup HTTP/2 before srv.Serve, to initialize srv.TLSConfig
    // before we clone it and create the TLS Listener.
    {
        var errΔ1 = srv.setupHTTP2_ServeTLS(); if (errΔ1 != default!) {
            return errΔ1;
        }
    }
    var config = cloneTLSConfig(srv.TLSConfig);
    if (!slices.Contains((~config).NextProtos, "http/1.1")) {
        config.val.NextProtos = append((~config).NextProtos, "http/1.1"u8);
    }
    var configHasCert = len((~config).Certificates) > 0 || (~config).GetCertificate != default! || (~config).GetConfigForClient != default!;
    if (!configHasCert || certFile != ""u8 || keyFile != ""u8) {
        error err = default!;
        config.val.Certificates = new slice<tls.Certificate>(1);
        ((~config).Certificates[0], err) = tls.LoadX509KeyPair(certFile, keyFile);
        if (err != default!) {
            return err;
        }
    }
    var tlsListener = tls.NewListener(l, config);
    return srv.Serve(tlsListener);
}

[GoType("dyn")] partial struct trackListener_s {
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
[GoRecv] public static bool trackListener(this ref Server s, ж<net.Listener> Ꮡln, bool add) => func((defer, _) => {
    ref var ln = ref Ꮡln.val;

    s.mu.Lock();
    defer(s.mu.Unlock);
    if (s.listeners == default!) {
        s.listeners = new map<ж<net.Listener>, EmptyStruct>();
    }
    if (add){
        if (s.shuttingDown()) {
            return false;
        }
        s.listeners[ln] = new trackListener_s();
        s.listenerGroup.Add(1);
    } else {
        delete(s.listeners, Ꮡln);
        s.listenerGroup.Done();
    }
    return true;
});

[GoType("dyn")] partial struct trackConn_s {
}

[GoRecv] public static void trackConn(this ref Server s, ж<conn> Ꮡc, bool add) => func((defer, _) => {
    ref var c = ref Ꮡc.val;

    s.mu.Lock();
    defer(s.mu.Unlock);
    if (s.activeConn == default!) {
        s.activeConn = new map<ж<conn>, EmptyStruct>();
    }
    if (add){
        s.activeConn[c] = new trackConn_s();
    } else {
        delete(s.activeConn, Ꮡc);
    }
});

[GoRecv] internal static time.Duration idleTimeout(this ref Server s) {
    if (s.IdleTimeout != 0) {
        return s.IdleTimeout;
    }
    return s.ReadTimeout;
}

[GoRecv] internal static time.Duration readHeaderTimeout(this ref Server s) {
    if (s.ReadHeaderTimeout != 0) {
        return s.ReadHeaderTimeout;
    }
    return s.ReadTimeout;
}

[GoRecv] internal static bool doKeepAlives(this ref Server s) {
    return !s.disableKeepAlives.Load() && !s.shuttingDown();
}

[GoRecv] internal static bool shuttingDown(this ref Server s) {
    return s.inShutdown.Load();
}

// SetKeepAlivesEnabled controls whether HTTP keep-alives are enabled.
// By default, keep-alives are always enabled. Only very
// resource-constrained environments or servers in the process of
// shutting down should disable them.
[GoRecv] public static void SetKeepAlivesEnabled(this ref Server srv, bool v) {
    if (v) {
        srv.disableKeepAlives.Store(false);
        return;
    }
    srv.disableKeepAlives.Store(true);
    // Close idle HTTP/1 conns:
    srv.closeIdleConns();
}

// TODO: Issue 26303: close HTTP/2 conns as soon as they become idle.
[GoRecv] internal static void logf(this ref Server s, @string format, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.slice();

    if (s.ErrorLog != nil){
        s.ErrorLog.Printf(format, args.ꓸꓸꓸ);
    } else {
        log.Printf(format, args.ꓸꓸꓸ);
    }
}

// logf prints to the ErrorLog of the *Server associated with request r
// via ServerContextKey. If there's no associated server, or if ErrorLog
// is nil, logging is done via the log package's standard logger.
internal static void logf(ж<Request> Ꮡr, @string format, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.slice();

    ref var r = ref Ꮡr.val;
    var (s, _) = r.Context().Value(ServerContextKey)._<Server.val>(ᐧ);
    if (s != nil && (~s).ErrorLog != nil){
        (~s).ErrorLog.Printf(format, args.ꓸꓸꓸ);
    } else {
        log.Printf(format, args.ꓸꓸꓸ);
    }
}

// ListenAndServe listens on the TCP network address addr and then calls
// [Serve] with handler to handle requests on incoming connections.
// Accepted connections are configured to enable TCP keep-alives.
//
// The handler is typically nil, in which case [DefaultServeMux] is used.
//
// ListenAndServe always returns a non-nil error.
public static error ListenAndServe(@string addr, ΔHandler handler) {
    var server = Ꮡ(new Server(Addr: addr, ΔHandler: handler));
    return server.ListenAndServe();
}

// ListenAndServeTLS acts identically to [ListenAndServe], except that it
// expects HTTPS connections. Additionally, files containing a certificate and
// matching private key for the server must be provided. If the certificate
// is signed by a certificate authority, the certFile should be the concatenation
// of the server's certificate, any intermediates, and the CA's certificate.
public static error ListenAndServeTLS(@string addr, @string certFile, @string keyFile, ΔHandler handler) {
    var server = Ꮡ(new Server(Addr: addr, ΔHandler: handler));
    return server.ListenAndServeTLS(certFile, keyFile);
}

// ListenAndServeTLS listens on the TCP network address srv.Addr and
// then calls [ServeTLS] to handle requests on incoming TLS connections.
// Accepted connections are configured to enable TCP keep-alives.
//
// Filenames containing a certificate and matching private key for the
// server must be provided if neither the [Server]'s TLSConfig.Certificates
// nor TLSConfig.GetCertificate are populated. If the certificate is
// signed by a certificate authority, the certFile should be the
// concatenation of the server's certificate, any intermediates, and
// the CA's certificate.
//
// If srv.Addr is blank, ":https" is used.
//
// ListenAndServeTLS always returns a non-nil error. After [Server.Shutdown] or
// [Server.Close], the returned error is [ErrServerClosed].
[GoRecv] public static error ListenAndServeTLS(this ref Server srv, @string certFile, @string keyFile) => func((defer, _) => {
    if (srv.shuttingDown()) {
        return ErrServerClosed;
    }
    @string addr = srv.Addr;
    if (addr == ""u8) {
        addr = ":https"u8;
    }
    (ln, err) = net.Listen("tcp"u8, addr);
    if (err != default!) {
        return err;
    }
    var lnʗ1 = ln;
    defer(lnʗ1.Close);
    return srv.ServeTLS(ln, certFile, keyFile);
});

// setupHTTP2_ServeTLS conditionally configures HTTP/2 on
// srv and reports whether there was an error setting it up. If it is
// not configured for policy reasons, nil is returned.
[GoRecv] internal static error setupHTTP2_ServeTLS(this ref Server srv) {
    srv.nextProtoOnce.Do(srv.onceSetNextProtoDefaults);
    return srv.nextProtoErr;
}

// setupHTTP2_Serve is called from (*Server).Serve and conditionally
// configures HTTP/2 on srv using a more conservative policy than
// setupHTTP2_ServeTLS because Serve is called after tls.Listen,
// and may be called concurrently. See shouldConfigureHTTP2ForServe.
//
// The tests named TestTransportAutomaticHTTP2* and
// TestConcurrentServerServe in server_test.go demonstrate some
// of the supported use cases and motivations.
[GoRecv] internal static error setupHTTP2_Serve(this ref Server srv) {
    srv.nextProtoOnce.Do(srv.onceSetNextProtoDefaults_Serve);
    return srv.nextProtoErr;
}

[GoRecv] internal static void onceSetNextProtoDefaults_Serve(this ref Server srv) {
    if (srv.shouldConfigureHTTP2ForServe()) {
        srv.onceSetNextProtoDefaults();
    }
}

internal static ж<godebug.Setting> http2server = godebug.New("http2server"u8);

// onceSetNextProtoDefaults configures HTTP/2, if the user hasn't
// configured otherwise. (by setting srv.TLSNextProto non-nil)
// It must only be called via srv.nextProtoOnce (use srv.setupHTTP2_*).
[GoRecv] internal static void onceSetNextProtoDefaults(this ref Server srv) {
    if (omitBundledHTTP2) {
        return;
    }
    if (http2server.Value() == "0"u8) {
        http2server.IncNonDefault();
        return;
    }
    // Enable HTTP/2 by default if the user hasn't otherwise
    // configured their TLSNextProto map.
    if (srv.TLSNextProto == default!) {
        var conf = Ꮡ(new http2Server(nil));
        srv.nextProtoErr = http2ConfigureServer(srv, conf);
    }
}

// TimeoutHandler returns a [Handler] that runs h with the given time limit.
//
// The new Handler calls h.ServeHTTP to handle each request, but if a
// call runs for longer than its time limit, the handler responds with
// a 503 Service Unavailable error and the given message in its body.
// (If msg is empty, a suitable default message will be sent.)
// After such a timeout, writes by h to its [ResponseWriter] will return
// [ErrHandlerTimeout].
//
// TimeoutHandler supports the [Pusher] interface but does not support
// the [Hijacker] or [Flusher] interfaces.
public static ΔHandler TimeoutHandler(ΔHandler h, time.Duration dt, @string msg) {
    return new timeoutHandler(
        handler: h,
        body: msg,
        dt: dt
    );
}

// ErrHandlerTimeout is returned on [ResponseWriter] Write calls
// in handlers which have timed out.
public static error ErrHandlerTimeout = errors.New("http: Handler timeout"u8);

[GoType] partial struct timeoutHandler {
    internal ΔHandler handler;
    internal @string body;
    internal time_package.Duration dt;
    // When set, no context will be created and this context will
    // be used instead.
    internal context_package.Context testContext;
}

[GoRecv] internal static @string errorBody(this ref timeoutHandler h) {
    if (h.body != ""u8) {
        return h.body;
    }
    return "<html><head><title>Timeout</title></head><body><h1>Timeout</h1></body></html>"u8;
}

[GoRecv] internal static void ServeHTTP(this ref timeoutHandler h, ResponseWriter w, ж<Request> Ꮡr) => func((defer, recover) => {
    ref var r = ref Ꮡr.val;

    var ctx = h.testContext;
    if (ctx == default!) {
        context.CancelFunc cancelCtx = default!;
        (ctx, cancelCtx) = context.WithTimeout(r.Context(), h.dt);
        var cancelCtxʗ1 = cancelCtx;
        defer(cancelCtxʗ1);
    }
    r = r.WithContext(ctx);
    var done = new channel<EmptyStruct>(1);
    var tw = Ꮡ(new timeoutWriter(
        w: w,
        h: new ΔHeader(),
        req: r
    ));
    var panicChan = new channel<any>(1);
    var doneʗ1 = done;
    var panicChanʗ1 = panicChan;
    var twʗ1 = tw;
    goǃ(() => {
        var panicChanʗ2 = panicChan;
        defer(() => {
            {
                var p = recover(); if (p != default!) {
                    panicChanʗ2.ᐸꟷ(p);
                }
            }
        });
        h.handler.ServeHTTP(~tw, Ꮡr);
        close(done);
    });
    switch (select(ᐸꟷ(panicChan, ꓸꓸꓸ), ᐸꟷ(done, ꓸꓸꓸ), ᐸꟷ(ctx.Done(), ꓸꓸꓸ))) {
    case 0 when panicChan.ꟷᐳ(out var p): {
        throw panic(p);
        break;
    }
    case 1 when done.ꟷᐳ(out _): {
        (~tw).mu.Lock();
        var twʗ2 = tw;
        defer((~twʗ2).mu.Unlock);
        var dst = w.Header();
        foreach (var (k, vv) in (~tw).h) {
            dst[k] = vv;
        }
        if (!(~tw).wroteHeader) {
            tw.val.code = StatusOK;
        }
        w.WriteHeader((~tw).code);
        w.Write((~tw).wbuf.Bytes());
        break;
    }
    case 2 when ctx.Done().ꟷᐳ(out _): {
        (~tw).mu.Lock();
        var twʗ3 = tw;
        defer((~twʗ3).mu.Unlock);
        {
            var err = ctx.Err();
            var exprᴛ1 = err;
            if (exprᴛ1 == context.DeadlineExceeded) {
                w.WriteHeader(StatusServiceUnavailable);
                io.WriteString(w, h.errorBody());
                tw.val.err = ErrHandlerTimeout;
            }
            else { /* default: */
                w.WriteHeader(StatusServiceUnavailable);
                tw.val.err = err;
            }
        }

        break;
    }}
});

[GoType] partial struct timeoutWriter {
    internal ResponseWriter w;
    internal ΔHeader h;
    internal bytes_package.Buffer wbuf;
    internal ж<Request> req;
    internal sync_package.Mutex mu;
    internal error err;
    internal bool wroteHeader;
    internal nint code;
}

internal static Pusher _ᴛ6ʗ = (ж<timeoutWriter>)(default!);

// Push implements the [Pusher] interface.
[GoRecv] internal static error Push(this ref timeoutWriter tw, @string target, ж<PushOptions> Ꮡopts) {
    ref var opts = ref Ꮡopts.val;

    {
        var (pusher, ok) = tw.w._<Pusher>(ᐧ); if (ok) {
            return pusher.Push(target, Ꮡopts);
        }
    }
    return ~ErrNotSupported;
}

[GoRecv] internal static ΔHeader Header(this ref timeoutWriter tw) {
    return tw.h;
}

[GoRecv] internal static (nint, error) Write(this ref timeoutWriter tw, slice<byte> p) => func((defer, _) => {
    tw.mu.Lock();
    defer(tw.mu.Unlock);
    if (tw.err != default!) {
        return (0, tw.err);
    }
    if (!tw.wroteHeader) {
        tw.writeHeaderLocked(StatusOK);
    }
    return tw.wbuf.Write(p);
});

[GoRecv] internal static void writeHeaderLocked(this ref timeoutWriter tw, nint code) {
    checkWriteHeaderCode(code);
    switch (ᐧ) {
    case {} when tw.err != default!: {
        return;
    }
    case {} when tw.wroteHeader: {
        if (tw.req != nil) {
            var caller = relevantCaller();
            logf(tw.req, "http: superfluous response.WriteHeader call from %s (%s:%d)"u8, caller.Function, path.Base(caller.File), caller.Line);
        }
        break;
    }
    default: {
        tw.wroteHeader = true;
        tw.code = code;
        break;
    }}

}

[GoRecv] internal static void WriteHeader(this ref timeoutWriter tw, nint code) => func((defer, _) => {
    tw.mu.Lock();
    defer(tw.mu.Unlock);
    tw.writeHeaderLocked(code);
});

// onceCloseListener wraps a net.Listener, protecting it from
// multiple Close calls.
[GoType] partial struct onceCloseListener {
    public partial ref net_package.Listener Listener { get; }
    internal sync_package.Once once;
    internal error closeErr;
}

[GoRecv] internal static error Close(this ref onceCloseListener oc) {
    oc.once.Do(oc.close);
    return oc.closeErr;
}

[GoRecv] internal static void close(this ref onceCloseListener oc) {
    oc.closeErr = oc.Listener.Close();
}

// globalOptionsHandler responds to "OPTIONS *" requests.
[GoType] partial struct globalOptionsHandler {
}

internal static void ServeHTTP(this globalOptionsHandler , ResponseWriter w, ж<Request> Ꮡr) {
    ref var r = ref Ꮡr.val;

    w.Header().Set("Content-Length"u8, "0"u8);
    if (r.ContentLength != 0) {
        // Read up to 4KB of OPTIONS body (as mentioned in the
        // spec as being reserved for future use), but anything
        // over that is considered a waste of server resources
        // (or an attack) and we abort and close the connection,
        // courtesy of MaxBytesReader's EOF behavior.
        var mb = MaxBytesReader(w, r.Body, 4 << (int)(10));
        io.Copy(io.Discard, mb);
    }
}

// initALPNRequest is an HTTP handler that initializes certain
// uninitialized fields in its *Request. Such partially-initialized
// Requests come from ALPN protocol handlers.
[GoType] partial struct initALPNRequest {
    internal context_package.Context ctx;
    internal ж<crypto.tls_package.Conn> c;
    internal serverHandler h;
}

// BaseContext is an exported but unadvertised [http.Handler] method
// recognized by x/net/http2 to pass down a context; the TLSNextProto
// API predates context support so we shoehorn through the only
// interface we have available.
internal static context.Context BaseContext(this initALPNRequest h) {
    return h.ctx;
}

internal static void ServeHTTP(this initALPNRequest h, ResponseWriter rw, ж<Request> Ꮡreq) {
    ref var req = ref Ꮡreq.val;

    if (req.TLS == nil) {
        req.TLS = Ꮡ(new tlsꓸConnectionState(nil));
        req.TLS = h.c.ConnectionState();
    }
    if (req.Body == default!) {
        req.Body = NoBody;
    }
    if (req.RemoteAddr == ""u8) {
        req.RemoteAddr = h.c.RemoteAddr().String();
    }
    h.h.ServeHTTP(rw, Ꮡreq);
}

// loggingConn is used for debugging.
[GoType] partial struct loggingConn {
    internal @string name;
    public partial ref net_package.Conn Conn { get; }
}

internal static sync.Mutex uniqNameMu;
internal static map<@string, nint> uniqNameNext = new map<@string, nint>();

internal static net.Conn newLoggingConn(@string baseName, net.Conn c) => func((defer, _) => {
    uniqNameMu.Lock();
    var uniqNameMuʗ1 = uniqNameMu;
    defer(uniqNameMuʗ1.Unlock);
    uniqNameNext[baseName]++;
    return new loggingConn(
        name: fmt.Sprintf("%s-%d"u8, baseName, uniqNameNext[baseName]),
        Conn: c
    );
});

[GoRecv] internal static (nint n, error err) Write(this ref loggingConn c, slice<byte> p) {
    nint n = default!;
    error err = default!;

    log.Printf("%s.Write(%d) = ...."u8, c.name, len(p));
    (n, err) = c.Conn.Write(p);
    log.Printf("%s.Write(%d) = %d, %v"u8, c.name, len(p), n, err);
    return (n, err);
}

[GoRecv] internal static (nint n, error err) Read(this ref loggingConn c, slice<byte> p) {
    nint n = default!;
    error err = default!;

    log.Printf("%s.Read(%d) = ...."u8, c.name, len(p));
    (n, err) = c.Conn.Read(p);
    log.Printf("%s.Read(%d) = %d, %v"u8, c.name, len(p), n, err);
    return (n, err);
}

[GoRecv] internal static error /*err*/ Close(this ref loggingConn c) {
    error err = default!;

    log.Printf("%s.Close() = ..."u8, c.name);
    err = c.Conn.Close();
    log.Printf("%s.Close() = %v"u8, c.name, err);
    return err;
}

// checkConnErrorWriter writes to c.rwc and records any write errors to c.werr.
// It only contains one field (and a pointer field at that), so it
// fits in an interface value without an extra allocation.
[GoType] partial struct checkConnErrorWriter {
    internal ж<conn> c;
}

internal static (nint n, error err) Write(this checkConnErrorWriter w, slice<byte> p) {
    nint n = default!;
    error err = default!;

    (n, err) = w.c.rwc.Write(p);
    if (err != default! && w.c.werr == default!) {
        w.c.werr = err;
        w.c.cancelCtx();
    }
    return (n, err);
}

internal static nint /*n*/ numLeadingCRorLF(slice<byte> v) {
    nint n = default!;

    foreach (var (_, b) in v) {
        if (b == (rune)'\r' || b == (rune)'\n') {
            n++;
            continue;
        }
        break;
    }
    return n;
}

// tlsRecordHeaderLooksLikeHTTP reports whether a TLS record header
// looks like it might've been a misdirected plaintext HTTP request.
internal static bool tlsRecordHeaderLooksLikeHTTP(array<byte> hdr) {
    hdr = hdr.Clone();

    var exprᴛ1 = ((@string)(hdr[..]));
    if (exprᴛ1 == "GET /"u8 || exprᴛ1 == "HEAD "u8 || exprᴛ1 == "POST "u8 || exprᴛ1 == "PUT /"u8 || exprᴛ1 == "OPTIO"u8) {
        return true;
    }

    return false;
}

// MaxBytesHandler returns a [Handler] that runs h with its [ResponseWriter] and [Request.Body] wrapped by a MaxBytesReader.
public static ΔHandler MaxBytesHandler(ΔHandler h, int64 n) {
    return ((HandlerFunc)((ResponseWriter w, ж<Request> r) => {
        ref var r2 = ref heap<Request>(out var Ꮡr2);
        r2 = r.val;
        r2.Body = MaxBytesReader(w, (~r).Body, n);
        h.ServeHTTP(w, Ꮡr2);
    }));
}

} // end http_package
