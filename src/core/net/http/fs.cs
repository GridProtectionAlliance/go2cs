// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// HTTP file system request handler
namespace go.net;

using errors = errors_package;
using fmt = fmt_package;
using godebug = @internal.godebug_package;
using io = io_package;
using fs = io.fs_package;
using mime = mime_package;
using multipart = mime.multipart_package;
using textproto = net.textproto_package;
using url = net.url_package;
using os = os_package;
using path = path_package;
using filepath = path.filepath_package;
using sort = sort_package;
using strconv = strconv_package;
using strings = strings_package;
using time = time_package;
using @internal;
using io;
using mime;
using path;

partial class http_package {

[GoType("@string")] partial struct Dir;

// mapOpenError maps the provided non-nil error from opening name
// to a possibly better non-nil error. In particular, it turns OS-specific errors
// about opening files in non-directories into fs.ErrNotExist. See Issues 18984 and 49552.
internal static error mapOpenError(error originalErr, @string name, rune sep, fs.FileInfo, error) stat) {
    if (errors.Is(originalErr, fs.ErrNotExist) || errors.Is(originalErr, fs.ErrPermission)) {
        return originalErr;
    }
    var parts = strings.Split(name, ((@string)sep));
    foreach (var (i, _) in parts) {
        if (parts[i] == "") {
            continue;
        }
        (fi, err) = stat(strings.Join(parts[..(int)(i + 1)], ((@string)sep)));
        if (err != default!) {
            return originalErr;
        }
        if (!fi.IsDir()) {
            return fs.ErrNotExist;
        }
    }
    return originalErr;
}

// Open implements [FileSystem] using [os.Open], opening files for reading rooted
// and relative to the directory d.
public static (File, error) Open(this Dir d, @string name) {
    @string path = path.Clean("/"u8 + name)[1..];
    if (path == ""u8) {
        path = "."u8;
    }
    (path, err) = filepath.Localize(path);
    if (err != default!) {
        return (default!, errors.New("http: invalid or unsafe file path"u8));
    }
    @string dir = ((@string)d);
    if (dir == ""u8) {
        dir = "."u8;
    }
    @string fullName = filepath.Join(dir, path);
    (f, err) = os.Open(fullName);
    if (err != default!) {
        return (default!, mapOpenError(err, fullName, filepath.Separator, os.Stat));
    }
    return (~f, default!);
}

// A FileSystem implements access to a collection of named files.
// The elements in a file path are separated by slash ('/', U+002F)
// characters, regardless of host operating system convention.
// See the [FileServer] function to convert a FileSystem to a [Handler].
//
// This interface predates the [fs.FS] interface, which can be used instead:
// the [FS] adapter function converts an fs.FS to a FileSystem.
[GoType] partial interface FileSystem {
    (File, error) Open(@string name);
}

// A File is returned by a [FileSystem]'s Open method and can be
// served by the [FileServer] implementation.
//
// The methods should behave the same as those on an [*os.File].
[GoType] partial interface File :
    io.Closer,
    io.Reader,
    io.Seeker
{
    (slice<fs.FileInfo>, error) Readdir(nint count);
    (fs.FileInfo, error) Stat();
}

[GoType] partial interface anyDirs {
    nint len();
    @string name(nint i);
    bool isDir(nint i);
}

[GoType("[]fs")] partial struct fileInfoDirs;

internal static nint len(this fileInfoDirs d) {
    return len(d);
}

internal static bool isDir(this fileInfoDirs d, nint i) {
    return d[i].IsDir();
}

internal static @string name(this fileInfoDirs d, nint i) {
    return d[i].Name();
}

[GoType("[]fs")] partial struct dirEntryDirs;

internal static nint len(this dirEntryDirs d) {
    return len(d);
}

internal static bool isDir(this dirEntryDirs d, nint i) {
    return d[i].IsDir();
}

internal static @string name(this dirEntryDirs d, nint i) {
    return d[i].Name();
}

internal static void dirList(ResponseWriter w, ж<Request> Ꮡr, File f) {
    ref var r = ref Ꮡr.val;

    // Prefer to use ReadDir instead of Readdir,
    // because the former doesn't require calling
    // Stat on every entry of a directory on Unix.
    anyDirs dirs = default!;
    error err = default!;
    {
        var (d, ok) = f._<fs.ReadDirFile>(ᐧ); if (ok){
            dirEntryDirs listΔ1 = default!;
            (, err) = d.ReadDir(-1);
            dirs = listΔ1;
        } else {
            fileInfoDirs list = default!;
            (list, err) = f.Readdir(-1);
            dirs = list;
        }
    }
    if (err != default!) {
        logf(Ꮡr, "http: error reading directory: %v"u8, err);
        Error(w, "Error reading directory"u8, StatusInternalServerError);
        return;
    }
    sort.Slice(dirs, 
    var dirsʗ1 = dirs;
    (nint i, nint j) => dirsʗ1.name(iΔ1) < dirsʗ1.name(j));
    w.Header().Set("Content-Type"u8, "text/html; charset=utf-8"u8);
    fmt.Fprintf(w, "<!doctype html>\n"u8);
    fmt.Fprintf(w, "<meta name=\"viewport\" content=\"width=device-width\">\n"u8);
    fmt.Fprintf(w, "<pre>\n"u8);
    for (nint i = 0;nint n = dirs.len(); i < n; i++) {
        @string name = dirs.name(i);
        if (dirs.isDir(i)) {
            name += "/"u8;
        }
        // name may contain '?' or '#', which must be escaped to remain
        // part of the URL path, and not indicate the start of a query
        // string or fragment.
        var url = new url.URL(Path: name);
        fmt.Fprintf(w, "<a href=\"%s\">%s</a>\n"u8, url.String(), htmlReplacer.Replace(name));
    }
    fmt.Fprintf(w, "</pre>\n"u8);
}

// GODEBUG=httpservecontentkeepheaders=1 restores the pre-1.23 behavior of not deleting
// Cache-Control, Content-Encoding, Etag, or Last-Modified headers on ServeContent errors.
internal static ж<godebug.Setting> httpservecontentkeepheaders = godebug.New("httpservecontentkeepheaders"u8);

// serveError serves an error from ServeFile, ServeFileFS, and ServeContent.
// Because those can all be configured by the caller by setting headers like
// Etag, Last-Modified, and Cache-Control to send on a successful response,
// the error path needs to clear them, since they may not be meant for errors.
internal static void serveError(ResponseWriter w, @string text, nint code) {
    var h = w.Header();
    var nonDefault = false;
    foreach (var (_, k) in new @string[]{
        "Cache-Control",
        "Content-Encoding",
        "Etag",
        "Last-Modified"
    }.slice()) {
        if (!h.has(k)) {
            continue;
        }
        if (httpservecontentkeepheaders.Value() == "1"u8){
            nonDefault = true;
        } else {
            h.Del(k);
        }
    }
    if (nonDefault) {
        httpservecontentkeepheaders.IncNonDefault();
    }
    Error(w, text, code);
}

// ServeContent replies to the request using the content in the
// provided ReadSeeker. The main benefit of ServeContent over [io.Copy]
// is that it handles Range requests properly, sets the MIME type, and
// handles If-Match, If-Unmodified-Since, If-None-Match, If-Modified-Since,
// and If-Range requests.
//
// If the response's Content-Type header is not set, ServeContent
// first tries to deduce the type from name's file extension and,
// if that fails, falls back to reading the first block of the content
// and passing it to [DetectContentType].
// The name is otherwise unused; in particular it can be empty and is
// never sent in the response.
//
// If modtime is not the zero time or Unix epoch, ServeContent
// includes it in a Last-Modified header in the response. If the
// request includes an If-Modified-Since header, ServeContent uses
// modtime to decide whether the content needs to be sent at all.
//
// The content's Seek method must work: ServeContent uses
// a seek to the end of the content to determine its size.
// Note that [*os.File] implements the [io.ReadSeeker] interface.
//
// If the caller has set w's ETag header formatted per RFC 7232, section 2.3,
// ServeContent uses it to handle requests using If-Match, If-None-Match, or If-Range.
//
// If an error occurs when serving the request (for example, when
// handling an invalid range request), ServeContent responds with an
// error message. By default, ServeContent strips the Cache-Control,
// Content-Encoding, ETag, and Last-Modified headers from error responses.
// The GODEBUG setting httpservecontentkeepheaders=1 causes ServeContent
// to preserve these headers.
public static void ServeContent(ResponseWriter w, ж<Request> Ꮡreq, @string name, time.Time modtime, io.ReadSeeker content) {
    ref var req = ref Ꮡreq.val;

    var sizeFunc = () => {
        var (size, err) = content.Seek(0, io.SeekEnd);
        if (err != default!) {
            return (0, errSeeker);
        }
        (_, err) = content.Seek(0, io.SeekStart);
        if (err != default!) {
            return (0, errSeeker);
        }
        return (size, default!);
    };
    serveContent(w, Ꮡreq, name, modtime, sizeFunc, content);
}

// errSeeker is returned by ServeContent's sizeFunc when the content
// doesn't seek properly. The underlying Seeker's error text isn't
// included in the sizeFunc reply so it's not sent over HTTP to end
// users.
internal static error errSeeker = errors.New("seeker can't seek"u8);

// errNoOverlap is returned by serveContent's parseRange if first-byte-pos of
// all of the byte-range-spec values is greater than the content size.
internal static error errNoOverlap = errors.New("invalid range: failed to overlap"u8);

// if name is empty, filename is unknown. (used for mime type, before sniffing)
// if modtime.IsZero(), modtime is unknown.
// content must be seeked to the beginning of the file.
// The sizeFunc is called at most once. Its error, if any, is sent in the HTTP response.
internal static void serveContent(ResponseWriter w, ж<Request> Ꮡr, @string name, time.Time modtime, Func<(int64, error)> sizeFunc, io.ReadSeeker content) => func((defer, _) => {
    ref var r = ref Ꮡr.val;

    setLastModified(w, modtime);
    var (done, rangeReq) = checkPreconditions(w, Ꮡr, modtime);
    if (done) {
        return;
    }
    nint code = StatusOK;
    // If Content-Type isn't set, use the file's extension to find it, but
    // if the Content-Type is unset explicitly, do not sniff the type.
    var ctypes = w.Header()["Content-Type"u8];
    var haveType = w.Header()["Content-Type"u8];
    @string ctype = default!;
    if (!haveType){
        ctype = mime.TypeByExtension(filepath.Ext(name));
        if (ctype == ""u8) {
            // read a chunk to decide between utf-8 text and binary
            array<byte> buf = new(512); /* sniffLen */
            var (n, _) = io.ReadFull(content, buf[..]);
            ctype = DetectContentType(buf[..(int)(n)]);
            var (_, errΔ1) = content.Seek(0, io.SeekStart);
            // rewind to output whole file
            if (errΔ1 != default!) {
                serveError(w, "seeker can't seek"u8, StatusInternalServerError);
                return;
            }
        }
        w.Header().Set("Content-Type"u8, ctype);
    } else 
    if (len(ctypes) > 0) {
        ctype = ctypes[0];
    }
    var (size, err) = sizeFunc();
    if (err != default!) {
        serveError(w, err.Error(), StatusInternalServerError);
        return;
    }
    if (size < 0) {
        // Should never happen but just to be sure
        serveError(w, "negative content size computed"u8, StatusInternalServerError);
        return;
    }
    // handle Content-Range header.
    var sendSize = size;
    io.Reader sendContent = content;
    (ranges, err) = parseRange(rangeReq, size);
    var exprᴛ1 = err;
    var matchᴛ1 = false;
    if (exprᴛ1 == default!) { matchᴛ1 = true;
    }
    else if (exprᴛ1 == errNoOverlap) { matchᴛ1 = true;
        if (size == 0) {
            // Some clients add a Range header to all requests to
            // limit the size of the response. If the file is empty,
            // ignore the range header and respond with a 200 rather
            // than a 416.
            ranges = default!;
            break;
        }
        w.Header().Set("Content-Range"u8, fmt.Sprintf("bytes */%d"u8, size));
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1) { /* default: */
        serveError(w, err.Error(), StatusRequestedRangeNotSatisfiable);
        return;
    }

    if (sumRangesSize(ranges) > size) {
        // The total number of bytes in all the ranges
        // is larger than the size of the file by
        // itself, so this is probably an attack, or a
        // dumb client. Ignore the range request.
        ranges = default!;
    }
    switch (ᐧ) {
    case {} when len(ranges) is 1: {
        var ra = ranges[0];
        {
            var (_, errΔ6) = content.Seek(ra.start, // RFC 7233, Section 4.1:
 // "If a single part is being transferred, the server
 // generating the 206 response MUST generate a
 // Content-Range header field, describing what range
 // of the selected representation is enclosed, and a
 // payload consisting of the range.
 // ...
 // A server MUST NOT generate a multipart response to
 // a request for a single range, since a client that
 // does not request multiple parts might not support
 // multipart responses."
 io.SeekStart); if (errΔ6 != default!) {
                serveError(w, errΔ6.Error(), StatusRequestedRangeNotSatisfiable);
                return;
            }
        }
        sendSize = ra.length;
        code = StatusPartialContent;
        w.Header().Set("Content-Range"u8, ra.contentRange(size));
        break;
    }
    case {} when len(ranges) is > 1: {
        sendSize = rangesMIMESize(ranges, ctype, size);
        code = StatusPartialContent;
        (pr, pw) = io.Pipe();
        var mw = multipart.NewWriter(~pw);
        w.Header().Set("Content-Type"u8, "multipart/byteranges; boundary="u8 + mw.Boundary());
        sendContent = ~pr;
        var prʗ1 = pr;
        defer(prʗ1.Close);
        var mwʗ1 = mw;
        var pwʗ1 = pw;
        var rangesʗ1 = ranges;
        goǃ(() => {
            // cause writing goroutine to fail and exit if CopyN doesn't finish.
            ref var ra = ref heap(new httpRange(), out var Ꮡra);

            foreach (var (_, ra) in rangesʗ1) {
                (part, errΔ7) = mwʗ1.CreatePart(ra.mimeHeader(ctype, size));
                if (errΔ7 != default!) {
                    pwʗ1.CloseWithError(errΔ7);
                    return;
                }
                {
                    var (_, errΔ8) = content.Seek(ra.start, io.SeekStart); if (errΔ8 != default!) {
                        pwʗ1.CloseWithError(errΔ8);
                        return;
                    }
                }
                {
                    var (_, errΔ9) = io.CopyN(part, content, ra.length); if (errΔ9 != default!) {
                        pwʗ1.CloseWithError(errΔ9);
                        return;
                    }
                }
            }
            mwʗ1.Close();
            pwʗ1.Close();
        });
        break;
    }}

    w.Header().Set("Accept-Ranges"u8, "bytes"u8);
    // We should be able to unconditionally set the Content-Length here.
    //
    // However, there is a pattern observed in the wild that this breaks:
    // The user wraps the ResponseWriter in one which gzips data written to it,
    // and sets "Content-Encoding: gzip".
    //
    // The user shouldn't be doing this; the serveContent path here depends
    // on serving seekable data with a known length. If you want to compress
    // on the fly, then you shouldn't be using ServeFile/ServeContent, or
    // you should compress the entire file up-front and provide a seekable
    // view of the compressed data.
    //
    // However, since we've observed this pattern in the wild, and since
    // setting Content-Length here breaks code that mostly-works today,
    // skip setting Content-Length if the user set Content-Encoding.
    //
    // If this is a range request, always set Content-Length.
    // If the user isn't changing the bytes sent in the ResponseWrite,
    // the Content-Length will be correct.
    // If the user is changing the bytes sent, then the range request wasn't
    // going to work properly anyway and we aren't worse off.
    //
    // A possible future improvement on this might be to look at the type
    // of the ResponseWriter, and always set Content-Length if it's one
    // that we recognize.
    if (len(ranges) > 0 || w.Header().Get("Content-Encoding"u8) == ""u8) {
        w.Header().Set("Content-Length"u8, strconv.FormatInt(sendSize, 10));
    }
    w.WriteHeader(code);
    if (r.Method != "HEAD"u8) {
        io.CopyN(w, sendContent, sendSize);
    }
});

// scanETag determines if a syntactically valid ETag is present at s. If so,
// the ETag and remaining text after consuming ETag is returned. Otherwise,
// it returns "", "".
internal static (@string etag, @string remain) scanETag(@string s) {
    @string etag = default!;
    @string remain = default!;

    s = textproto.TrimString(s);
    nint start = 0;
    if (strings.HasPrefix(s, "W/"u8)) {
        start = 2;
    }
    if (len(s[(int)(start)..]) < 2 || s[start] != (rune)'"') {
        return ("", "");
    }
    // ETag is either W/"text" or "text".
    // See RFC 7232 2.3.
    for (nint i = start + 1; i < len(s); i++) {
        var c = s[i];
        switch (ᐧ) {
        case {} when c == 33 || c >= 35 && c <= 126 || c >= 128: {
            break;
        }
        case {} when c is (rune)'"': {
            return (s[..(int)(i + 1)], s[(int)(i + 1)..]);
        }
        default: {
            return ("", "");
        }}

    }
    // Character values allowed in ETags.
    return ("", "");
}

// etagStrongMatch reports whether a and b match using strong ETag comparison.
// Assumes a and b are valid ETags.
internal static bool etagStrongMatch(@string a, @string b) {
    return a == b && a != ""u8 && a[0] == (rune)'"';
}

// etagWeakMatch reports whether a and b match using weak ETag comparison.
// Assumes a and b are valid ETags.
internal static bool etagWeakMatch(@string a, @string b) {
    return strings.TrimPrefix(a, "W/"u8) == strings.TrimPrefix(b, "W/"u8);
}

[GoType("num:nint")] partial struct condResult;

internal static readonly condResult condNone = /* iota */ 0;
internal static readonly condResult condTrue = 1;
internal static readonly condResult condFalse = 2;

internal static condResult checkIfMatch(ResponseWriter w, ж<Request> Ꮡr) {
    ref var r = ref Ꮡr.val;

    @string im = r.Header.Get("If-Match"u8);
    if (im == ""u8) {
        return condNone;
    }
    while (ᐧ) {
        im = textproto.TrimString(im);
        if (len(im) == 0) {
            break;
        }
        if (im[0] == (rune)',') {
            im = im[1..];
            continue;
        }
        if (im[0] == (rune)'*') {
            return condTrue;
        }
        var (etag, remain) = scanETag(im);
        if (etag == ""u8) {
            break;
        }
        if (etagStrongMatch(etag, w.Header().get("Etag"u8))) {
            return condTrue;
        }
        im = remain;
    }
    return condFalse;
}

internal static condResult checkIfUnmodifiedSince(ж<Request> Ꮡr, time.Time modtime) {
    ref var r = ref Ꮡr.val;

    @string ius = r.Header.Get("If-Unmodified-Since"u8);
    if (ius == ""u8 || isZeroTime(modtime)) {
        return condNone;
    }
    var (t, err) = ParseTime(ius);
    if (err != default!) {
        return condNone;
    }
    // The Last-Modified header truncates sub-second precision so
    // the modtime needs to be truncated too.
    modtime = modtime.Truncate(time.ΔSecond);
    {
        nint ret = modtime.Compare(t); if (ret <= 0) {
            return condTrue;
        }
    }
    return condFalse;
}

internal static condResult checkIfNoneMatch(ResponseWriter w, ж<Request> Ꮡr) {
    ref var r = ref Ꮡr.val;

    @string inm = r.Header.get("If-None-Match"u8);
    if (inm == ""u8) {
        return condNone;
    }
    @string buf = inm;
    while (ᐧ) {
        buf = textproto.TrimString(buf);
        if (len(buf) == 0) {
            break;
        }
        if (buf[0] == (rune)',') {
            buf = buf[1..];
            continue;
        }
        if (buf[0] == (rune)'*') {
            return condFalse;
        }
        var (etag, remain) = scanETag(buf);
        if (etag == ""u8) {
            break;
        }
        if (etagWeakMatch(etag, w.Header().get("Etag"u8))) {
            return condFalse;
        }
        buf = remain;
    }
    return condTrue;
}

internal static condResult checkIfModifiedSince(ж<Request> Ꮡr, time.Time modtime) {
    ref var r = ref Ꮡr.val;

    if (r.Method != "GET"u8 && r.Method != "HEAD"u8) {
        return condNone;
    }
    @string ims = r.Header.Get("If-Modified-Since"u8);
    if (ims == ""u8 || isZeroTime(modtime)) {
        return condNone;
    }
    var (t, err) = ParseTime(ims);
    if (err != default!) {
        return condNone;
    }
    // The Last-Modified header truncates sub-second precision so
    // the modtime needs to be truncated too.
    modtime = modtime.Truncate(time.ΔSecond);
    {
        nint ret = modtime.Compare(t); if (ret <= 0) {
            return condFalse;
        }
    }
    return condTrue;
}

internal static condResult checkIfRange(ResponseWriter w, ж<Request> Ꮡr, time.Time modtime) {
    ref var r = ref Ꮡr.val;

    if (r.Method != "GET"u8 && r.Method != "HEAD"u8) {
        return condNone;
    }
    @string ir = r.Header.get("If-Range"u8);
    if (ir == ""u8) {
        return condNone;
    }
    var (etag, _) = scanETag(ir);
    if (etag != ""u8) {
        if (etagStrongMatch(etag, w.Header().Get("Etag"u8))){
            return condTrue;
        } else {
            return condFalse;
        }
    }
    // The If-Range value is typically the ETag value, but it may also be
    // the modtime date. See golang.org/issue/8367.
    if (modtime.IsZero()) {
        return condFalse;
    }
    var (t, err) = ParseTime(ir);
    if (err != default!) {
        return condFalse;
    }
    if (t.Unix() == modtime.Unix()) {
        return condTrue;
    }
    return condFalse;
}

internal static time.Time unixEpochTime = time.Unix(0, 0);

// isZeroTime reports whether t is obviously unspecified (either zero or Unix()=0).
internal static bool isZeroTime(time.Time t) {
    return t.IsZero() || t.Equal(unixEpochTime);
}

internal static void setLastModified(ResponseWriter w, time.Time modtime) {
    if (!isZeroTime(modtime)) {
        w.Header().Set("Last-Modified"u8, modtime.UTC().Format(TimeFormat));
    }
}

internal static void writeNotModified(ResponseWriter w) {
    // RFC 7232 section 4.1:
    // a sender SHOULD NOT generate representation metadata other than the
    // above listed fields unless said metadata exists for the purpose of
    // guiding cache updates (e.g., Last-Modified might be useful if the
    // response does not have an ETag field).
    var h = w.Header();
    delete(h, "Content-Type"u8);
    delete(h, "Content-Length"u8);
    delete(h, "Content-Encoding"u8);
    if (h.Get("Etag"u8) != ""u8) {
        delete(h, "Last-Modified"u8);
    }
    w.WriteHeader(StatusNotModified);
}

// checkPreconditions evaluates request preconditions and reports whether a precondition
// resulted in sending StatusNotModified or StatusPreconditionFailed.
internal static (bool done, @string rangeHeader) checkPreconditions(ResponseWriter w, ж<Request> Ꮡr, time.Time modtime) {
    bool done = default!;
    @string rangeHeader = default!;

    ref var r = ref Ꮡr.val;
    // This function carefully follows RFC 7232 section 6.
    condResult ch = checkIfMatch(w, Ꮡr);
    if (ch == condNone) {
        ch = checkIfUnmodifiedSince(Ꮡr, modtime);
    }
    if (ch == condFalse) {
        w.WriteHeader(StatusPreconditionFailed);
        return (true, "");
    }
    var exprᴛ1 = checkIfNoneMatch(w, Ꮡr);
    if (exprᴛ1 == condFalse) {
        if (r.Method == "GET"u8 || r.Method == "HEAD"u8){
            writeNotModified(w);
            return (true, "");
        } else {
            w.WriteHeader(StatusPreconditionFailed);
            return (true, "");
        }
    }
    if (exprᴛ1 == condNone) {
        if (checkIfModifiedSince(Ꮡr, modtime) == condFalse) {
            writeNotModified(w);
            return (true, "");
        }
    }

    rangeHeader = r.Header.get("Range"u8);
    if (rangeHeader != ""u8 && checkIfRange(w, Ꮡr, modtime) == condFalse) {
        rangeHeader = ""u8;
    }
    return (false, rangeHeader);
}

// name is '/'-separated, not filepath.Separator.
internal static void serveFile(ResponseWriter w, ж<Request> Ꮡr, FileSystem fs, @string name, bool redirect) => func((defer, _) => {
    ref var r = ref Ꮡr.val;

    @string indexPage = "/index.html"u8;
    // redirect .../index.html to .../
    // can't use Redirect() because that would make the path absolute,
    // which would be a problem running under StripPrefix
    if (strings.HasSuffix(r.URL.Path, indexPage)) {
        localRedirect(w, Ꮡr, "./"u8);
        return;
    }
    (f, err) = fs.Open(name);
    if (err != default!) {
        var (msg, code) = toHTTPError(err);
        serveError(w, msg, code);
        return;
    }
    var fʗ1 = f;
    defer(fʗ1.Close);
    (d, err) = f.Stat();
    if (err != default!) {
        var (msg, code) = toHTTPError(err);
        serveError(w, msg, code);
        return;
    }
    if (redirect) {
        // redirect to canonical path: / at end of directory url
        // r.URL.Path always begins with /
        @string url = r.URL.Path;
        if (d.IsDir()){
            if (url[len(url) - 1] != (rune)'/') {
                localRedirect(w, Ꮡr, path.Base(url) + "/"u8);
                return;
            }
        } else 
        if (url[len(url) - 1] == (rune)'/') {
            @string @base = path.Base(url);
            if (@base == "/"u8 || @base == "."u8) {
                // The FileSystem maps a path like "/" or "/./" to a file instead of a directory.
                @string msg = "http: attempting to traverse a non-directory"u8;
                serveError(w, msg, StatusInternalServerError);
                return;
            }
            localRedirect(w, Ꮡr, "../"u8 + @base);
            return;
        }
    }
    if (d.IsDir()) {
        @string url = r.URL.Path;
        // redirect if the directory name doesn't end in a slash
        if (url == ""u8 || url[len(url) - 1] != (rune)'/') {
            localRedirect(w, Ꮡr, path.Base(url) + "/"u8);
            return;
        }
        // use contents of index.html for directory, if present
        @string index = strings.TrimSuffix(name, "/"u8) + indexPage;
        (ff, err) = fs.Open(index);
        if (err == default!) {
            var ffʗ1 = ff;
            defer(ffʗ1.Close);
            (dd, errΔ1) = ff.Stat();
            if (errΔ1 == default!) {
                d = dd;
                f = ff;
            }
        }
    }
    // Still a directory? (we didn't find an index.html file)
    if (d.IsDir()) {
        if (checkIfModifiedSince(Ꮡr, d.ModTime()) == condFalse) {
            writeNotModified(w);
            return;
        }
        setLastModified(w, d.ModTime());
        dirList(w, Ꮡr, f);
        return;
    }
    // serveContent will check modification time
    var sizeFunc = 
    var dʗ1 = d;
    () => (dʗ1.Size(), default!);
    serveContent(w, Ꮡr, d.Name(), d.ModTime(), sizeFunc, f);
});

// toHTTPError returns a non-specific HTTP error message and status code
// for a given non-nil error value. It's important that toHTTPError does not
// actually return err.Error(), since msg and httpStatus are returned to users,
// and historically Go's ServeContent always returned just "404 Not Found" for
// all errors. We don't want to start leaking information in error messages.
internal static (@string msg, nint httpStatus) toHTTPError(error err) {
    @string msg = default!;
    nint httpStatus = default!;

    if (errors.Is(err, fs.ErrNotExist)) {
        return ("404 page not found", StatusNotFound);
    }
    if (errors.Is(err, fs.ErrPermission)) {
        return ("403 Forbidden", StatusForbidden);
    }
    // Default:
    return ("500 Internal Server Error", StatusInternalServerError);
}

// localRedirect gives a Moved Permanently response.
// It does not convert relative paths to absolute paths like Redirect does.
internal static void localRedirect(ResponseWriter w, ж<Request> Ꮡr, @string newPath) {
    ref var r = ref Ꮡr.val;

    {
        @string q = r.URL.RawQuery; if (q != ""u8) {
            newPath += "?"u8 + q;
        }
    }
    w.Header().Set("Location"u8, newPath);
    w.WriteHeader(StatusMovedPermanently);
}

// ServeFile replies to the request with the contents of the named
// file or directory.
//
// If the provided file or directory name is a relative path, it is
// interpreted relative to the current directory and may ascend to
// parent directories. If the provided name is constructed from user
// input, it should be sanitized before calling [ServeFile].
//
// As a precaution, ServeFile will reject requests where r.URL.Path
// contains a ".." path element; this protects against callers who
// might unsafely use [filepath.Join] on r.URL.Path without sanitizing
// it and then use that filepath.Join result as the name argument.
//
// As another special case, ServeFile redirects any request where r.URL.Path
// ends in "/index.html" to the same path, without the final
// "index.html". To avoid such redirects either modify the path or
// use [ServeContent].
//
// Outside of those two special cases, ServeFile does not use
// r.URL.Path for selecting the file or directory to serve; only the
// file or directory provided in the name argument is used.
public static void ServeFile(ResponseWriter w, ж<Request> Ꮡr, @string name) {
    ref var r = ref Ꮡr.val;

    if (containsDotDot(r.URL.Path)) {
        // Too many programs use r.URL.Path to construct the argument to
        // serveFile. Reject the request under the assumption that happened
        // here and ".." may not be wanted.
        // Note that name might not contain "..", for example if code (still
        // incorrectly) used filepath.Join(myDir, r.URL.Path).
        serveError(w, "invalid URL path"u8, StatusBadRequest);
        return;
    }
    var (dir, file) = filepath.Split(name);
    serveFile(w, Ꮡr, ((Dir)dir), file, false);
}

// ServeFileFS replies to the request with the contents
// of the named file or directory from the file system fsys.
// The files provided by fsys must implement [io.Seeker].
//
// If the provided name is constructed from user input, it should be
// sanitized before calling [ServeFileFS].
//
// As a precaution, ServeFileFS will reject requests where r.URL.Path
// contains a ".." path element; this protects against callers who
// might unsafely use [filepath.Join] on r.URL.Path without sanitizing
// it and then use that filepath.Join result as the name argument.
//
// As another special case, ServeFileFS redirects any request where r.URL.Path
// ends in "/index.html" to the same path, without the final
// "index.html". To avoid such redirects either modify the path or
// use [ServeContent].
//
// Outside of those two special cases, ServeFileFS does not use
// r.URL.Path for selecting the file or directory to serve; only the
// file or directory provided in the name argument is used.
public static void ServeFileFS(ResponseWriter w, ж<Request> Ꮡr, fs.FS fsys, @string name) {
    ref var r = ref Ꮡr.val;

    if (containsDotDot(r.URL.Path)) {
        // Too many programs use r.URL.Path to construct the argument to
        // serveFile. Reject the request under the assumption that happened
        // here and ".." may not be wanted.
        // Note that name might not contain "..", for example if code (still
        // incorrectly) used filepath.Join(myDir, r.URL.Path).
        serveError(w, "invalid URL path"u8, StatusBadRequest);
        return;
    }
    serveFile(w, Ꮡr, FS(fsys), name, false);
}

internal static bool containsDotDot(@string v) {
    if (!strings.Contains(v, ".."u8)) {
        return false;
    }
    foreach (var (_, ent) in strings.FieldsFunc(v, isSlashRune)) {
        if (ent == ".."u8) {
            return true;
        }
    }
    return false;
}

internal static bool isSlashRune(rune r) {
    return r == (rune)'/' || r == (rune)'\\';
}

[GoType] partial struct fileHandler {
    internal FileSystem root;
}

[GoType] partial struct ioFS {
    internal io.fs_package.FS fsys;
}

[GoType] partial struct ioFile {
    internal io.fs_package.File file;
}

internal static (File, error) Open(this ioFS f, @string name) {
    if (name == "/"u8){
        name = "."u8;
    } else {
        name = strings.TrimPrefix(name, "/"u8);
    }
    (file, err) = f.fsys.Open(name);
    if (err != default!) {
        return (default!, mapOpenError(err, name, (rune)'/', 
        var fʗ1 = f;
        (@string path) => fs.Stat(fʗ1.fsys, path)));
    }
    return (new ioFile(file), default!);
}

internal static error Close(this ioFile f) {
    return f.file.Close();
}

internal static (nint, error) Read(this ioFile f, slice<byte> b) {
    return f.file.Read(b);
}

internal static (fs.FileInfo, error) Stat(this ioFile f) {
    return f.file.Stat();
}

internal static error errMissingSeek = errors.New("io.File missing Seek method"u8);

internal static error errMissingReadDir = errors.New("io.File directory missing ReadDir method"u8);

internal static (int64, error) Seek(this ioFile f, int64 offset, nint whence) {
    var (s, ok) = f.file._<io.Seeker>(ᐧ);
    if (!ok) {
        return (0, errMissingSeek);
    }
    return s.Seek(offset, whence);
}

internal static (slice<fs.DirEntry>, error) ReadDir(this ioFile f, nint count) {
    var (d, ok) = f.file._<fs.ReadDirFile>(ᐧ);
    if (!ok) {
        return (default!, errMissingReadDir);
    }
    return d.ReadDir(count);
}

internal static (slice<fs.FileInfo>, error) Readdir(this ioFile f, nint count) {
    var (d, ok) = f.file._<fs.ReadDirFile>(ᐧ);
    if (!ok) {
        return (default!, errMissingReadDir);
    }
    slice<fs.FileInfo> list = default!;
    while (ᐧ) {
        (dirs, err) = d.ReadDir(count - len(list));
        foreach (var (_, dir) in dirs) {
            (info, errΔ1) = dir.Info();
            if (errΔ1 != default!) {
                // Pretend it doesn't exist, like (*os.File).Readdir does.
                continue;
            }
            list = append(list, info);
        }
        if (err != default!) {
            return (list, err);
        }
        if (count < 0 || len(list) >= count) {
            break;
        }
    }
    return (list, default!);
}

// FS converts fsys to a [FileSystem] implementation,
// for use with [FileServer] and [NewFileTransport].
// The files provided by fsys must implement [io.Seeker].
public static FileSystem FS(fs.FS fsys) {
    return new ioFS(fsys);
}

// FileServer returns a handler that serves HTTP requests
// with the contents of the file system rooted at root.
//
// As a special case, the returned file server redirects any request
// ending in "/index.html" to the same path, without the final
// "index.html".
//
// To use the operating system's file system implementation,
// use [http.Dir]:
//
//	http.Handle("/", http.FileServer(http.Dir("/tmp")))
//
// To use an [fs.FS] implementation, use [http.FileServerFS] instead.
public static ΔHandler FileServer(FileSystem root) {
    return new fileHandler(root);
}

// FileServerFS returns a handler that serves HTTP requests
// with the contents of the file system fsys.
// The files provided by fsys must implement [io.Seeker].
//
// As a special case, the returned file server redirects any request
// ending in "/index.html" to the same path, without the final
// "index.html".
//
//	http.Handle("/", http.FileServerFS(fsys))
public static ΔHandler FileServerFS(fs.FS root) {
    return FileServer(FS(root));
}

[GoRecv] internal static void ServeHTTP(this ref fileHandler f, ResponseWriter w, ж<Request> Ꮡr) {
    ref var r = ref Ꮡr.val;

    @string upath = r.URL.Path;
    if (!strings.HasPrefix(upath, "/"u8)) {
        upath = "/"u8 + upath;
        r.URL.Path = upath;
    }
    serveFile(w, Ꮡr, f.root, path.Clean(upath), true);
}

// httpRange specifies the byte range to be sent to the client.
[GoType] partial struct httpRange {
    internal int64 start;
    internal int64 length;
}

internal static @string contentRange(this httpRange r, int64 size) {
    return fmt.Sprintf("bytes %d-%d/%d"u8, r.start, r.start + r.length - 1, size);
}

internal static textproto.MIMEHeader mimeHeader(this httpRange r, @string contentType, int64 size) {
    return new textproto.MIMEHeader{
        "Content-Range"u8: new(r.contentRange(size)),
        "Content-Type"u8: new(contentType)
    };
}

// parseRange parses a Range header string as per RFC 7233.
// errNoOverlap is returned if none of the ranges overlap.
internal static (slice<httpRange>, error) parseRange(@string s, int64 size) {
    if (s == ""u8) {
        return (default!, default!);
    }
    // header not present
    @string b = "bytes="u8;
    if (!strings.HasPrefix(s, b)) {
        return (default!, errors.New("invalid range"u8));
    }
    slice<httpRange> ranges = default!;
    var noOverlap = false;
    foreach (var (_, ra) in strings.Split(s[(int)(len(b))..], ","u8)) {
        ra = textproto.TrimString(ra);
        if (ra == ""u8) {
            continue;
        }
        var (start, end, ok) = strings.Cut(ra, "-"u8);
        if (!ok) {
            return (default!, errors.New("invalid range"u8));
        }
        (start, end) = (textproto.TrimString(start), textproto.TrimString(end));
        httpRange r = default!;
        if (start == ""u8){
            // If no start is specified, end specifies the
            // range start relative to the end of the file,
            // and we are dealing with <suffix-length>
            // which has to be a non-negative integer as per
            // RFC 7233 Section 2.1 "Byte-Ranges".
            if (end == ""u8 || end[0] == (rune)'-') {
                return (default!, errors.New("invalid range"u8));
            }
            var (i, err) = strconv.ParseInt(end, 10, 64);
            if (i < 0 || err != default!) {
                return (default!, errors.New("invalid range"u8));
            }
            if (i > size) {
                i = size;
            }
            r.start = size - i;
            r.length = size - r.start;
        } else {
            var (i, err) = strconv.ParseInt(start, 10, 64);
            if (err != default! || i < 0) {
                return (default!, errors.New("invalid range"u8));
            }
            if (i >= size) {
                // If the range begins after the size of the content,
                // then it does not overlap.
                noOverlap = true;
                continue;
            }
            r.start = i;
            if (end == ""u8){
                // If no end is specified, range extends to end of the file.
                r.length = size - r.start;
            } else {
                var (iΔ1, errΔ1) = strconv.ParseInt(end, 10, 64);
                if (errΔ1 != default! || r.start > iΔ1) {
                    return (default!, errors.New("invalid range"u8));
                }
                if (iΔ1 >= size) {
                    iΔ1 = size - 1;
                }
                r.length = iΔ1 - r.start + 1;
            }
        }
        ranges = append(ranges, r);
    }
    if (noOverlap && len(ranges) == 0) {
        // The specified ranges did not overlap with the content.
        return (default!, errNoOverlap);
    }
    return (ranges, default!);
}

[GoType("num:int64")] partial struct countingWriter;

[GoRecv] internal static (nint n, error err) Write(this ref countingWriter w, slice<byte> p) {
    nint n = default!;
    error err = default!;

    w += ((countingWriter)len(p));
    return (len(p), default!);
}

// rangesMIMESize returns the number of bytes it takes to encode the
// provided ranges as a multipart response.
internal static int64 /*encSize*/ rangesMIMESize(slice<httpRange> ranges, @string contentType, int64 contentSize) {
    int64 encSize = default!;

    ref var w = ref heap(new countingWriter(), out var Ꮡw);
    var mw = multipart.NewWriter(~Ꮡw);
    foreach (var (_, ra) in ranges) {
        mw.CreatePart(ra.mimeHeader(contentType, contentSize));
        encSize += ra.length;
    }
    mw.Close();
    encSize += ((int64)w);
    return encSize;
}

internal static int64 /*size*/ sumRangesSize(slice<httpRange> ranges) {
    int64 size = default!;

    foreach (var (_, ra) in ranges) {
        size += ra.length;
    }
    return size;
}

} // end http_package
