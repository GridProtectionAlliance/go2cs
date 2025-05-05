// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net.http;

using bytes = bytes_package;
using fmt = fmt_package;
using io = io_package;
using http = net.http_package;
using textproto = net.textproto_package;
using strconv = strconv_package;
using strings = strings_package;
using httpguts = golang.org.x.net.http.httpguts_package;
using golang.org.x.net.http;
using net;

partial class httptest_package {

// ResponseRecorder is an implementation of [http.ResponseWriter] that
// records its mutations for later inspection in tests.
[GoType] partial struct ResponseRecorder {
    // Code is the HTTP response code set by WriteHeader.
    //
    // Note that if a Handler never calls WriteHeader or Write,
    // this might end up being 0, rather than the implicit
    // http.StatusOK. To get the implicit value, use the Result
    // method.
    public nint Code;
    // HeaderMap contains the headers explicitly set by the Handler.
    // It is an internal detail.
    //
    // Deprecated: HeaderMap exists for historical compatibility
    // and should not be used. To access the headers returned by a handler,
    // use the Response.Header map as returned by the Result method.
    public net.http_package.ΔHeader HeaderMap;
    // Body is the buffer to which the Handler's Write calls are sent.
    // If nil, the Writes are silently discarded.
    public ж<bytes_package.Buffer> Body;
    // Flushed is whether the Handler called Flush.
    public bool Flushed;
    internal ж<net.http_package.Response> result; // cache of Result's return value
    internal net.http_package.ΔHeader snapHeader;  // snapshot of HeaderMap at first Write
    internal bool wroteHeader;
}

// NewRecorder returns an initialized [ResponseRecorder].
public static ж<ResponseRecorder> NewRecorder() {
    return Ꮡ(new ResponseRecorder(
        HeaderMap: new httpꓸHeader(),
        Body: @new<bytes.Buffer>(),
        Code: 200
    ));
}

// DefaultRemoteAddr is the default remote address to return in RemoteAddr if
// an explicit DefaultRemoteAddr isn't set on [ResponseRecorder].
public static readonly @string DefaultRemoteAddr = "1.2.3.4"u8;

// Header implements [http.ResponseWriter]. It returns the response
// headers to mutate within a handler. To test the headers that were
// written after a handler completes, use the [ResponseRecorder.Result] method and see
// the returned Response value's Header.
[GoRecv] public static httpꓸHeader Header(this ref ResponseRecorder rw) {
    var m = rw.HeaderMap;
    if (m == default!) {
        m = new httpꓸHeader();
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
[GoRecv] internal static void writeHeader(this ref ResponseRecorder rw, slice<byte> b, @string str) {
    if (rw.wroteHeader) {
        return;
    }
    if (len(str) > 512) {
        str = str[..512];
    }
    var m = rw.Header();
    var _ = m["Content-Type"u8];
    var hasType = m["Content-Type"u8];
    var hasTE = m.Get("Transfer-Encoding"u8) != ""u8;
    if (!hasType && !hasTE) {
        if (b == default!) {
            b = slice<byte>(str);
        }
        m.Set("Content-Type"u8, http.DetectContentType(b));
    }
    rw.WriteHeader(200);
}

// Write implements http.ResponseWriter. The data in buf is written to
// rw.Body, if not nil.
[GoRecv] public static (nint, error) Write(this ref ResponseRecorder rw, slice<byte> buf) {
    rw.writeHeader(buf, ""u8);
    if (rw.Body != nil) {
        rw.Body.Write(buf);
    }
    return (len(buf), default!);
}

// WriteString implements [io.StringWriter]. The data in str is written
// to rw.Body, if not nil.
[GoRecv] public static (nint, error) WriteString(this ref ResponseRecorder rw, @string str) {
    rw.writeHeader(default!, str);
    if (rw.Body != nil) {
        rw.Body.WriteString(str);
    }
    return (len(str), default!);
}

internal static void checkWriteHeaderCode(nint code) {
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
    if (code < 100 || code > 999) {
        throw panic(fmt.Sprintf("invalid WriteHeader code %v"u8, code));
    }
}

// WriteHeader implements [http.ResponseWriter].
[GoRecv] public static void WriteHeader(this ref ResponseRecorder rw, nint code) {
    if (rw.wroteHeader) {
        return;
    }
    checkWriteHeaderCode(code);
    rw.Code = code;
    rw.wroteHeader = true;
    if (rw.HeaderMap == default!) {
        rw.HeaderMap = new httpꓸHeader();
    }
    rw.snapHeader = rw.HeaderMap.Clone();
}

// Flush implements [http.Flusher]. To test whether Flush was
// called, see rw.Flushed.
[GoRecv] public static void Flush(this ref ResponseRecorder rw) {
    if (!rw.wroteHeader) {
        rw.WriteHeader(200);
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
// guaranteed to not return any error other than [io.EOF].
//
// Result must only be called after the handler has finished running.
[GoRecv] public static ж<http.Response> Result(this ref ResponseRecorder rw) {
    if (rw.result != nil) {
        return rw.result;
    }
    if (rw.snapHeader == default!) {
        rw.snapHeader = rw.HeaderMap.Clone();
    }
    var res = Ꮡ(new http.Response(
        Proto: "HTTP/1.1"u8,
        ProtoMajor: 1,
        ProtoMinor: 1,
        StatusCode: rw.Code,
        Header: rw.snapHeader
    ));
    rw.result = res;
    if ((~res).StatusCode == 0) {
        res.val.StatusCode = 200;
    }
    res.val.Status = fmt.Sprintf("%03d %s"u8, (~res).StatusCode, http.StatusText((~res).StatusCode));
    if (rw.Body != nil){
        res.val.Body = io.NopCloser(~bytes.NewReader(rw.Body.Bytes()));
    } else {
        res.val.Body = http.NoBody;
    }
    res.val.ContentLength = parseContentLength((~res).Header.Get("Content-Length"u8));
    {
        var trailers = rw.snapHeader["Trailer"u8];
        var ok = rw.snapHeader["Trailer"u8]; if (ok) {
            res.val.Trailer = new httpꓸHeader(len(trailers));
            foreach (var (_, k) in trailers) {
                foreach (var (_, kΔ1) in strings.Split(k, ","u8)) {
                    kΔ1 = http.CanonicalHeaderKey(textproto.TrimString(kΔ1));
                    if (!httpguts.ValidTrailerHeader(kΔ1)) {
                        // Ignore since forbidden by RFC 7230, section 4.1.2.
                        continue;
                    }
                    var vv = rw.HeaderMap[kΔ1];
                    var okΔ1 = rw.HeaderMap[kΔ1];
                    if (!okΔ1) {
                        continue;
                    }
                    var vv2 = new slice<@string>(len(vv));
                    copy(vv2, vv);
                    (~res).Trailer[kΔ1] = vv2;
                }
            }
        }
    }
    foreach (var (k, vv) in rw.HeaderMap) {
        if (!strings.HasPrefix(k, http.TrailerPrefix)) {
            continue;
        }
        if ((~res).Trailer == default!) {
            res.val.Trailer = new httpꓸHeader();
        }
        foreach (var (_, v) in vv) {
            (~res).Trailer.Add(strings.TrimPrefix(k, http.TrailerPrefix), v);
        }
    }
    return res;
}

// parseContentLength trims whitespace from s and returns -1 if no value
// is set, or the value if it's >= 0.
//
// This a modified version of same function found in net/http/transfer.go. This
// one just ignores an invalid header.
internal static int64 parseContentLength(@string cl) {
    cl = textproto.TrimString(cl);
    if (cl == ""u8) {
        return -1;
    }
    var (n, err) = strconv.ParseUint(cl, 10, 63);
    if (err != default!) {
        return -1;
    }
    return ((int64)n);
}

} // end httptest_package
