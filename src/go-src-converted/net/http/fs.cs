// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// HTTP file system request handler

// package http -- go2cs converted at 2020 October 08 03:38:41 UTC
// import "net/http" ==> using http = go.net.http_package
// Original source: C:\Go\src\net\http\fs.go
using errors = go.errors_package;
using fmt = go.fmt_package;
using io = go.io_package;
using mime = go.mime_package;
using multipart = go.mime.multipart_package;
using textproto = go.net.textproto_package;
using url = go.net.url_package;
using os = go.os_package;
using path = go.path_package;
using filepath = go.path.filepath_package;
using sort = go.sort_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using time = go.time_package;
using static go.builtin;
using System;
using System.Threading;

namespace go {
namespace net
{
    public static partial class http_package
    {
        // A Dir implements FileSystem using the native file system restricted to a
        // specific directory tree.
        //
        // While the FileSystem.Open method takes '/'-separated paths, a Dir's string
        // value is a filename on the native file system, not a URL, so it is separated
        // by filepath.Separator, which isn't necessarily '/'.
        //
        // Note that Dir could expose sensitive files and directories. Dir will follow
        // symlinks pointing out of the directory tree, which can be especially dangerous
        // if serving from a directory in which users are able to create arbitrary symlinks.
        // Dir will also allow access to files and directories starting with a period,
        // which could expose sensitive directories like .git or sensitive files like
        // .htpasswd. To exclude files with a leading period, remove the files/directories
        // from the server or create a custom FileSystem implementation.
        //
        // An empty Dir is treated as ".".
        public partial struct Dir // : @string
        {
        }

        // mapDirOpenError maps the provided non-nil error from opening name
        // to a possibly better non-nil error. In particular, it turns OS-specific errors
        // about opening files in non-directories into os.ErrNotExist. See Issue 18984.
        private static error mapDirOpenError(error originalErr, @string name)
        {
            if (os.IsNotExist(originalErr) || os.IsPermission(originalErr))
            {
                return error.As(originalErr)!;
            }

            var parts = strings.Split(name, string(filepath.Separator));
            foreach (var (i) in parts)
            {
                if (parts[i] == "")
                {
                    continue;
                }

                var (fi, err) = os.Stat(strings.Join(parts[..i + 1L], string(filepath.Separator)));
                if (err != null)
                {
                    return error.As(originalErr)!;
                }

                if (!fi.IsDir())
                {
                    return error.As(os.ErrNotExist)!;
                }

            }
            return error.As(originalErr)!;

        }

        // Open implements FileSystem using os.Open, opening files for reading rooted
        // and relative to the directory d.
        public static (File, error) Open(this Dir d, @string name)
        {
            File _p0 = default;
            error _p0 = default!;

            if (filepath.Separator != '/' && strings.ContainsRune(name, filepath.Separator))
            {
                return (null, error.As(errors.New("http: invalid character in file path"))!);
            }

            var dir = string(d);
            if (dir == "")
            {
                dir = ".";
            }

            var fullName = filepath.Join(dir, filepath.FromSlash(path.Clean("/" + name)));
            var (f, err) = os.Open(fullName);
            if (err != null)
            {
                return (null, error.As(mapDirOpenError(err, fullName))!);
            }

            return (f, error.As(null!)!);

        }

        // A FileSystem implements access to a collection of named files.
        // The elements in a file path are separated by slash ('/', U+002F)
        // characters, regardless of host operating system convention.
        public partial interface FileSystem
        {
            (File, error) Open(@string name);
        }

        // A File is returned by a FileSystem's Open method and can be
        // served by the FileServer implementation.
        //
        // The methods should behave the same as those on an *os.File.
        public partial interface File : io.Closer, io.Reader, io.Seeker
        {
            (os.FileInfo, error) Readdir(long count);
            (os.FileInfo, error) Stat();
        }

        private static void dirList(ResponseWriter w, ptr<Request> _addr_r, File f)
        {
            ref Request r = ref _addr_r.val;

            var (dirs, err) = f.Readdir(-1L);
            if (err != null)
            {
                logf(r, "http: error reading directory: %v", err);
                Error(w, "Error reading directory", StatusInternalServerError);
                return ;
            }

            sort.Slice(dirs, (i, j) => dirs[i].Name() < dirs[j].Name());

            w.Header().Set("Content-Type", "text/html; charset=utf-8");
            fmt.Fprintf(w, "<pre>\n");
            foreach (var (_, d) in dirs)
            {
                var name = d.Name();
                if (d.IsDir())
                {
                    name += "/";
                } 
                // name may contain '?' or '#', which must be escaped to remain
                // part of the URL path, and not indicate the start of a query
                // string or fragment.
                url.URL url = new url.URL(Path:name);
                fmt.Fprintf(w, "<a href=\"%s\">%s</a>\n", url.String(), htmlReplacer.Replace(name));

            }
            fmt.Fprintf(w, "</pre>\n");

        }

        // ServeContent replies to the request using the content in the
        // provided ReadSeeker. The main benefit of ServeContent over io.Copy
        // is that it handles Range requests properly, sets the MIME type, and
        // handles If-Match, If-Unmodified-Since, If-None-Match, If-Modified-Since,
        // and If-Range requests.
        //
        // If the response's Content-Type header is not set, ServeContent
        // first tries to deduce the type from name's file extension and,
        // if that fails, falls back to reading the first block of the content
        // and passing it to DetectContentType.
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
        //
        // If the caller has set w's ETag header formatted per RFC 7232, section 2.3,
        // ServeContent uses it to handle requests using If-Match, If-None-Match, or If-Range.
        //
        // Note that *os.File implements the io.ReadSeeker interface.
        public static void ServeContent(ResponseWriter w, ptr<Request> _addr_req, @string name, time.Time modtime, io.ReadSeeker content)
        {
            ref Request req = ref _addr_req.val;

            Func<(long, error)> sizeFunc = () =>
            {
                var (size, err) = content.Seek(0L, io.SeekEnd);
                if (err != null)
                {
                    return (0L, errSeeker);
                }

                _, err = content.Seek(0L, io.SeekStart);
                if (err != null)
                {
                    return (0L, errSeeker);
                }

                return (size, null);

            }
;
            serveContent(w, _addr_req, name, modtime, sizeFunc, content);

        }

        // errSeeker is returned by ServeContent's sizeFunc when the content
        // doesn't seek properly. The underlying Seeker's error text isn't
        // included in the sizeFunc reply so it's not sent over HTTP to end
        // users.
        private static var errSeeker = errors.New("seeker can't seek");

        // errNoOverlap is returned by serveContent's parseRange if first-byte-pos of
        // all of the byte-range-spec values is greater than the content size.
        private static var errNoOverlap = errors.New("invalid range: failed to overlap");

        // if name is empty, filename is unknown. (used for mime type, before sniffing)
        // if modtime.IsZero(), modtime is unknown.
        // content must be seeked to the beginning of the file.
        // The sizeFunc is called at most once. Its error, if any, is sent in the HTTP response.
        private static (long, error) serveContent(ResponseWriter w, ptr<Request> _addr_r, @string name, time.Time modtime, Func<(long, error)> sizeFunc, io.ReadSeeker content) => func((defer, _, __) =>
        {
            long _p0 = default;
            error _p0 = default!;
            ref Request r = ref _addr_r.val;

            setLastModified(w, modtime);
            var (done, rangeReq) = checkPreconditions(w, _addr_r, modtime);
            if (done)
            {
                return ;
            }

            var code = StatusOK; 

            // If Content-Type isn't set, use the file's extension to find it, but
            // if the Content-Type is unset explicitly, do not sniff the type.
            var (ctypes, haveType) = w.Header()["Content-Type"];
            @string ctype = default;
            if (!haveType)
            {
                ctype = mime.TypeByExtension(filepath.Ext(name));
                if (ctype == "")
                { 
                    // read a chunk to decide between utf-8 text and binary
                    array<byte> buf = new array<byte>(sniffLen);
                    var (n, _) = io.ReadFull(content, buf[..]);
                    ctype = DetectContentType(buf[..n]);
                    var (_, err) = content.Seek(0L, io.SeekStart); // rewind to output whole file
                    if (err != null)
                    {
                        Error(w, "seeker can't seek", StatusInternalServerError);
                        return ;
                    }

                }

                w.Header().Set("Content-Type", ctype);

            }
            else if (len(ctypes) > 0L)
            {
                ctype = ctypes[0L];
            }

            var (size, err) = sizeFunc();
            if (err != null)
            {
                Error(w, err.Error(), StatusInternalServerError);
                return ;
            } 

            // handle Content-Range header.
            var sendSize = size;
            io.Reader sendContent = content;
            if (size >= 0L)
            {
                var (ranges, err) = parseRange(rangeReq, size);
                if (err != null)
                {
                    if (err == errNoOverlap)
                    {
                        w.Header().Set("Content-Range", fmt.Sprintf("bytes */%d", size));
                    }

                    Error(w, err.Error(), StatusRequestedRangeNotSatisfiable);
                    return ;

                }

                if (sumRangesSize(ranges) > size)
                { 
                    // The total number of bytes in all the ranges
                    // is larger than the size of the file by
                    // itself, so this is probably an attack, or a
                    // dumb client. Ignore the range request.
                    ranges = null;

                }


                if (len(ranges) == 1L) 
                    // RFC 7233, Section 4.1:
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
                    var ra = ranges[0L];
                    {
                        (_, err) = content.Seek(ra.start, io.SeekStart);

                        if (err != null)
                        {
                            Error(w, err.Error(), StatusRequestedRangeNotSatisfiable);
                            return ;
                        }

                    }

                    sendSize = ra.length;
                    code = StatusPartialContent;
                    w.Header().Set("Content-Range", ra.contentRange(size));
                else if (len(ranges) > 1L) 
                    sendSize = rangesMIMESize(ranges, ctype, size);
                    code = StatusPartialContent;

                    var (pr, pw) = io.Pipe();
                    var mw = multipart.NewWriter(pw);
                    w.Header().Set("Content-Type", "multipart/byteranges; boundary=" + mw.Boundary());
                    sendContent = pr;
                    defer(pr.Close()); // cause writing goroutine to fail and exit if CopyN doesn't finish.
                    go_(() => () =>
                    {
                        {
                            var ra__prev1 = ra;

                            foreach (var (_, __ra) in ranges)
                            {
                                ra = __ra;
                                var (part, err) = mw.CreatePart(ra.mimeHeader(ctype, size));
                                if (err != null)
                                {
                                    pw.CloseWithError(err);
                                    return ;
                                }

                                {
                                    (_, err) = content.Seek(ra.start, io.SeekStart);

                                    if (err != null)
                                    {
                                        pw.CloseWithError(err);
                                        return ;
                                    }

                                }

                                {
                                    (_, err) = io.CopyN(part, content, ra.length);

                                    if (err != null)
                                    {
                                        pw.CloseWithError(err);
                                        return ;
                                    }

                                }

                            }

                            ra = ra__prev1;
                        }

                        mw.Close();
                        pw.Close();

                    }());
                                w.Header().Set("Accept-Ranges", "bytes");
                if (w.Header().Get("Content-Encoding") == "")
                {
                    w.Header().Set("Content-Length", strconv.FormatInt(sendSize, 10L));
                }

            }

            w.WriteHeader(code);

            if (r.Method != "HEAD")
            {
                io.CopyN(w, sendContent, sendSize);
            }

        });

        // scanETag determines if a syntactically valid ETag is present at s. If so,
        // the ETag and remaining text after consuming ETag is returned. Otherwise,
        // it returns "", "".
        private static (@string, @string) scanETag(@string s)
        {
            @string etag = default;
            @string remain = default;

            s = textproto.TrimString(s);
            long start = 0L;
            if (strings.HasPrefix(s, "W/"))
            {
                start = 2L;
            }

            if (len(s[start..]) < 2L || s[start] != '"')
            {
                return ("", "");
            } 
            // ETag is either W/"text" or "text".
            // See RFC 7232 2.3.
            for (var i = start + 1L; i < len(s); i++)
            {
                var c = s[i];

                // Character values allowed in ETags.
                if (c == 0x21UL || c >= 0x23UL && c <= 0x7EUL || c >= 0x80UL)                 else if (c == '"') 
                    return (s[..i + 1L], s[i + 1L..]);
                else 
                    return ("", "");
                
            }

            return ("", "");

        }

        // etagStrongMatch reports whether a and b match using strong ETag comparison.
        // Assumes a and b are valid ETags.
        private static bool etagStrongMatch(@string a, @string b)
        {
            return a == b && a != "" && a[0L] == '"';
        }

        // etagWeakMatch reports whether a and b match using weak ETag comparison.
        // Assumes a and b are valid ETags.
        private static bool etagWeakMatch(@string a, @string b)
        {
            return strings.TrimPrefix(a, "W/") == strings.TrimPrefix(b, "W/");
        }

        // condResult is the result of an HTTP request precondition check.
        // See https://tools.ietf.org/html/rfc7232 section 3.
        private partial struct condResult // : long
        {
        }

        private static readonly condResult condNone = (condResult)iota;
        private static readonly var condTrue = (var)0;
        private static readonly var condFalse = (var)1;


        private static condResult checkIfMatch(ResponseWriter w, ptr<Request> _addr_r)
        {
            ref Request r = ref _addr_r.val;

            var im = r.Header.Get("If-Match");
            if (im == "")
            {
                return condNone;
            }

            while (true)
            {
                im = textproto.TrimString(im);
                if (len(im) == 0L)
                {
                    break;
                }

                if (im[0L] == ',')
                {
                    im = im[1L..];
                    continue;
                }

                if (im[0L] == '*')
                {
                    return condTrue;
                }

                var (etag, remain) = scanETag(im);
                if (etag == "")
                {
                    break;
                }

                if (etagStrongMatch(etag, w.Header().get("Etag")))
                {
                    return condTrue;
                }

                im = remain;

            }


            return condFalse;

        }

        private static condResult checkIfUnmodifiedSince(ptr<Request> _addr_r, time.Time modtime)
        {
            ref Request r = ref _addr_r.val;

            var ius = r.Header.Get("If-Unmodified-Since");
            if (ius == "" || isZeroTime(modtime))
            {
                return condNone;
            }

            var (t, err) = ParseTime(ius);
            if (err != null)
            {
                return condNone;
            } 

            // The Last-Modified header truncates sub-second precision so
            // the modtime needs to be truncated too.
            modtime = modtime.Truncate(time.Second);
            if (modtime.Before(t) || modtime.Equal(t))
            {
                return condTrue;
            }

            return condFalse;

        }

        private static condResult checkIfNoneMatch(ResponseWriter w, ptr<Request> _addr_r)
        {
            ref Request r = ref _addr_r.val;

            var inm = r.Header.get("If-None-Match");
            if (inm == "")
            {
                return condNone;
            }

            var buf = inm;
            while (true)
            {
                buf = textproto.TrimString(buf);
                if (len(buf) == 0L)
                {
                    break;
                }

                if (buf[0L] == ',')
                {
                    buf = buf[1L..];
                    continue;
                }

                if (buf[0L] == '*')
                {
                    return condFalse;
                }

                var (etag, remain) = scanETag(buf);
                if (etag == "")
                {
                    break;
                }

                if (etagWeakMatch(etag, w.Header().get("Etag")))
                {
                    return condFalse;
                }

                buf = remain;

            }

            return condTrue;

        }

        private static condResult checkIfModifiedSince(ptr<Request> _addr_r, time.Time modtime)
        {
            ref Request r = ref _addr_r.val;

            if (r.Method != "GET" && r.Method != "HEAD")
            {
                return condNone;
            }

            var ims = r.Header.Get("If-Modified-Since");
            if (ims == "" || isZeroTime(modtime))
            {
                return condNone;
            }

            var (t, err) = ParseTime(ims);
            if (err != null)
            {
                return condNone;
            } 
            // The Last-Modified header truncates sub-second precision so
            // the modtime needs to be truncated too.
            modtime = modtime.Truncate(time.Second);
            if (modtime.Before(t) || modtime.Equal(t))
            {
                return condFalse;
            }

            return condTrue;

        }

        private static condResult checkIfRange(ResponseWriter w, ptr<Request> _addr_r, time.Time modtime)
        {
            ref Request r = ref _addr_r.val;

            if (r.Method != "GET" && r.Method != "HEAD")
            {
                return condNone;
            }

            var ir = r.Header.get("If-Range");
            if (ir == "")
            {
                return condNone;
            }

            var (etag, _) = scanETag(ir);
            if (etag != "")
            {
                if (etagStrongMatch(etag, w.Header().Get("Etag")))
                {
                    return condTrue;
                }
                else
                {
                    return condFalse;
                }

            } 
            // The If-Range value is typically the ETag value, but it may also be
            // the modtime date. See golang.org/issue/8367.
            if (modtime.IsZero())
            {
                return condFalse;
            }

            var (t, err) = ParseTime(ir);
            if (err != null)
            {
                return condFalse;
            }

            if (t.Unix() == modtime.Unix())
            {
                return condTrue;
            }

            return condFalse;

        }

        private static var unixEpochTime = time.Unix(0L, 0L);

        // isZeroTime reports whether t is obviously unspecified (either zero or Unix()=0).
        private static bool isZeroTime(time.Time t)
        {
            return t.IsZero() || t.Equal(unixEpochTime);
        }

        private static void setLastModified(ResponseWriter w, time.Time modtime)
        {
            if (!isZeroTime(modtime))
            {
                w.Header().Set("Last-Modified", modtime.UTC().Format(TimeFormat));
            }

        }

        private static void writeNotModified(ResponseWriter w)
        { 
            // RFC 7232 section 4.1:
            // a sender SHOULD NOT generate representation metadata other than the
            // above listed fields unless said metadata exists for the purpose of
            // guiding cache updates (e.g., Last-Modified might be useful if the
            // response does not have an ETag field).
            var h = w.Header();
            delete(h, "Content-Type");
            delete(h, "Content-Length");
            if (h.Get("Etag") != "")
            {
                delete(h, "Last-Modified");
            }

            w.WriteHeader(StatusNotModified);

        }

        // checkPreconditions evaluates request preconditions and reports whether a precondition
        // resulted in sending StatusNotModified or StatusPreconditionFailed.
        private static (bool, @string) checkPreconditions(ResponseWriter w, ptr<Request> _addr_r, time.Time modtime)
        {
            bool done = default;
            @string rangeHeader = default;
            ref Request r = ref _addr_r.val;
 
            // This function carefully follows RFC 7232 section 6.
            var ch = checkIfMatch(w, _addr_r);
            if (ch == condNone)
            {
                ch = checkIfUnmodifiedSince(_addr_r, modtime);
            }

            if (ch == condFalse)
            {
                w.WriteHeader(StatusPreconditionFailed);
                return (true, "");
            }


            if (checkIfNoneMatch(w, _addr_r) == condFalse) 
                if (r.Method == "GET" || r.Method == "HEAD")
                {
                    writeNotModified(w);
                    return (true, "");
                }
                else
                {
                    w.WriteHeader(StatusPreconditionFailed);
                    return (true, "");
                }

            else if (checkIfNoneMatch(w, _addr_r) == condNone) 
                if (checkIfModifiedSince(_addr_r, modtime) == condFalse)
                {
                    writeNotModified(w);
                    return (true, "");
                }

                        rangeHeader = r.Header.get("Range");
            if (rangeHeader != "" && checkIfRange(w, _addr_r, modtime) == condFalse)
            {
                rangeHeader = "";
            }

            return (false, rangeHeader);

        }

        // name is '/'-separated, not filepath.Separator.
        private static void serveFile(ResponseWriter w, ptr<Request> _addr_r, FileSystem fs, @string name, bool redirect) => func((defer, _, __) =>
        {
            ref Request r = ref _addr_r.val;

            const @string indexPage = (@string)"/index.html"; 

            // redirect .../index.html to .../
            // can't use Redirect() because that would make the path absolute,
            // which would be a problem running under StripPrefix
 

            // redirect .../index.html to .../
            // can't use Redirect() because that would make the path absolute,
            // which would be a problem running under StripPrefix
            if (strings.HasSuffix(r.URL.Path, indexPage))
            {
                localRedirect(w, _addr_r, "./");
                return ;
            }

            var (f, err) = fs.Open(name);
            if (err != null)
            {
                var (msg, code) = toHTTPError(err);
                Error(w, msg, code);
                return ;
            }

            defer(f.Close());

            var (d, err) = f.Stat();
            if (err != null)
            {
                (msg, code) = toHTTPError(err);
                Error(w, msg, code);
                return ;
            }

            if (redirect)
            { 
                // redirect to canonical path: / at end of directory url
                // r.URL.Path always begins with /
                var url = r.URL.Path;
                if (d.IsDir())
                {
                    if (url[len(url) - 1L] != '/')
                    {
                        localRedirect(w, _addr_r, path.Base(url) + "/");
                        return ;
                    }

                }
                else
                {
                    if (url[len(url) - 1L] == '/')
                    {
                        localRedirect(w, _addr_r, "../" + path.Base(url));
                        return ;
                    }

                }

            }

            if (d.IsDir())
            {
                url = r.URL.Path; 
                // redirect if the directory name doesn't end in a slash
                if (url == "" || url[len(url) - 1L] != '/')
                {
                    localRedirect(w, _addr_r, path.Base(url) + "/");
                    return ;
                } 

                // use contents of index.html for directory, if present
                var index = strings.TrimSuffix(name, "/") + indexPage;
                var (ff, err) = fs.Open(index);
                if (err == null)
                {
                    defer(ff.Close());
                    var (dd, err) = ff.Stat();
                    if (err == null)
                    {
                        name = index;
                        d = dd;
                        f = ff;
                    }

                }

            } 

            // Still a directory? (we didn't find an index.html file)
            if (d.IsDir())
            {
                if (checkIfModifiedSince(_addr_r, d.ModTime()) == condFalse)
                {
                    writeNotModified(w);
                    return ;
                }

                setLastModified(w, d.ModTime());
                dirList(w, _addr_r, f);
                return ;

            } 

            // serveContent will check modification time
            Func<(long, error)> sizeFunc = () => (d.Size(), null);
            serveContent(w, _addr_r, d.Name(), d.ModTime(), sizeFunc, f);

        });

        // toHTTPError returns a non-specific HTTP error message and status code
        // for a given non-nil error value. It's important that toHTTPError does not
        // actually return err.Error(), since msg and httpStatus are returned to users,
        // and historically Go's ServeContent always returned just "404 Not Found" for
        // all errors. We don't want to start leaking information in error messages.
        private static (@string, long) toHTTPError(error err)
        {
            @string msg = default;
            long httpStatus = default;

            if (os.IsNotExist(err))
            {
                return ("404 page not found", StatusNotFound);
            }

            if (os.IsPermission(err))
            {
                return ("403 Forbidden", StatusForbidden);
            } 
            // Default:
            return ("500 Internal Server Error", StatusInternalServerError);

        }

        // localRedirect gives a Moved Permanently response.
        // It does not convert relative paths to absolute paths like Redirect does.
        private static void localRedirect(ResponseWriter w, ptr<Request> _addr_r, @string newPath)
        {
            ref Request r = ref _addr_r.val;

            {
                var q = r.URL.RawQuery;

                if (q != "")
                {
                    newPath += "?" + q;
                }

            }

            w.Header().Set("Location", newPath);
            w.WriteHeader(StatusMovedPermanently);

        }

        // ServeFile replies to the request with the contents of the named
        // file or directory.
        //
        // If the provided file or directory name is a relative path, it is
        // interpreted relative to the current directory and may ascend to
        // parent directories. If the provided name is constructed from user
        // input, it should be sanitized before calling ServeFile.
        //
        // As a precaution, ServeFile will reject requests where r.URL.Path
        // contains a ".." path element; this protects against callers who
        // might unsafely use filepath.Join on r.URL.Path without sanitizing
        // it and then use that filepath.Join result as the name argument.
        //
        // As another special case, ServeFile redirects any request where r.URL.Path
        // ends in "/index.html" to the same path, without the final
        // "index.html". To avoid such redirects either modify the path or
        // use ServeContent.
        //
        // Outside of those two special cases, ServeFile does not use
        // r.URL.Path for selecting the file or directory to serve; only the
        // file or directory provided in the name argument is used.
        public static void ServeFile(ResponseWriter w, ptr<Request> _addr_r, @string name)
        {
            ref Request r = ref _addr_r.val;

            if (containsDotDot(r.URL.Path))
            { 
                // Too many programs use r.URL.Path to construct the argument to
                // serveFile. Reject the request under the assumption that happened
                // here and ".." may not be wanted.
                // Note that name might not contain "..", for example if code (still
                // incorrectly) used filepath.Join(myDir, r.URL.Path).
                Error(w, "invalid URL path", StatusBadRequest);
                return ;

            }

            var (dir, file) = filepath.Split(name);
            serveFile(w, _addr_r, Dir(dir), file, false);

        }

        private static bool containsDotDot(@string v)
        {
            if (!strings.Contains(v, ".."))
            {
                return false;
            }

            foreach (var (_, ent) in strings.FieldsFunc(v, isSlashRune))
            {
                if (ent == "..")
                {
                    return true;
                }

            }
            return false;

        }

        private static bool isSlashRune(int r)
        {
            return r == '/' || r == '\\';
        }

        private partial struct fileHandler
        {
            public FileSystem root;
        }

        // FileServer returns a handler that serves HTTP requests
        // with the contents of the file system rooted at root.
        //
        // To use the operating system's file system implementation,
        // use http.Dir:
        //
        //     http.Handle("/", http.FileServer(http.Dir("/tmp")))
        //
        // As a special case, the returned file server redirects any request
        // ending in "/index.html" to the same path, without the final
        // "index.html".
        public static Handler FileServer(FileSystem root)
        {
            return addr(new fileHandler(root));
        }

        private static void ServeHTTP(this ptr<fileHandler> _addr_f, ResponseWriter w, ptr<Request> _addr_r)
        {
            ref fileHandler f = ref _addr_f.val;
            ref Request r = ref _addr_r.val;

            var upath = r.URL.Path;
            if (!strings.HasPrefix(upath, "/"))
            {
                upath = "/" + upath;
                r.URL.Path = upath;
            }

            serveFile(w, _addr_r, f.root, path.Clean(upath), true);

        }

        // httpRange specifies the byte range to be sent to the client.
        private partial struct httpRange
        {
            public long start;
            public long length;
        }

        private static @string contentRange(this httpRange r, long size)
        {
            return fmt.Sprintf("bytes %d-%d/%d", r.start, r.start + r.length - 1L, size);
        }

        private static textproto.MIMEHeader mimeHeader(this httpRange r, @string contentType, long size)
        {
            return new textproto.MIMEHeader("Content-Range":{r.contentRange(size)},"Content-Type":{contentType},);
        }

        // parseRange parses a Range header string as per RFC 7233.
        // errNoOverlap is returned if none of the ranges overlap.
        private static (slice<httpRange>, error) parseRange(@string s, long size)
        {
            slice<httpRange> _p0 = default;
            error _p0 = default!;

            if (s == "")
            {
                return (null, error.As(null!)!); // header not present
            }

            const @string b = (@string)"bytes=";

            if (!strings.HasPrefix(s, b))
            {
                return (null, error.As(errors.New("invalid range"))!);
            }

            slice<httpRange> ranges = default;
            var noOverlap = false;
            foreach (var (_, ra) in strings.Split(s[len(b)..], ","))
            {
                ra = textproto.TrimString(ra);
                if (ra == "")
                {
                    continue;
                }

                var i = strings.Index(ra, "-");
                if (i < 0L)
                {
                    return (null, error.As(errors.New("invalid range"))!);
                }

                var start = textproto.TrimString(ra[..i]);
                var end = textproto.TrimString(ra[i + 1L..]);
                httpRange r = default;
                if (start == "")
                { 
                    // If no start is specified, end specifies the
                    // range start relative to the end of the file.
                    var (i, err) = strconv.ParseInt(end, 10L, 64L);
                    if (err != null)
                    {
                        return (null, error.As(errors.New("invalid range"))!);
                    }

                    if (i > size)
                    {
                        i = size;
                    }

                    r.start = size - i;
                    r.length = size - r.start;

                }
                else
                {
                    (i, err) = strconv.ParseInt(start, 10L, 64L);
                    if (err != null || i < 0L)
                    {
                        return (null, error.As(errors.New("invalid range"))!);
                    }

                    if (i >= size)
                    { 
                        // If the range begins after the size of the content,
                        // then it does not overlap.
                        noOverlap = true;
                        continue;

                    }

                    r.start = i;
                    if (end == "")
                    { 
                        // If no end is specified, range extends to end of the file.
                        r.length = size - r.start;

                    }
                    else
                    {
                        (i, err) = strconv.ParseInt(end, 10L, 64L);
                        if (err != null || r.start > i)
                        {
                            return (null, error.As(errors.New("invalid range"))!);
                        }

                        if (i >= size)
                        {
                            i = size - 1L;
                        }

                        r.length = i - r.start + 1L;

                    }

                }

                ranges = append(ranges, r);

            }
            if (noOverlap && len(ranges) == 0L)
            { 
                // The specified ranges did not overlap with the content.
                return (null, error.As(errNoOverlap)!);

            }

            return (ranges, error.As(null!)!);

        }

        // countingWriter counts how many bytes have been written to it.
        private partial struct countingWriter // : long
        {
        }

        private static (long, error) Write(this ptr<countingWriter> _addr_w, slice<byte> p)
        {
            long n = default;
            error err = default!;
            ref countingWriter w = ref _addr_w.val;

            w.val += countingWriter(len(p));
            return (len(p), error.As(null!)!);
        }

        // rangesMIMESize returns the number of bytes it takes to encode the
        // provided ranges as a multipart response.
        private static long rangesMIMESize(slice<httpRange> ranges, @string contentType, long contentSize)
        {
            long encSize = default;

            ref countingWriter w = ref heap(out ptr<countingWriter> _addr_w);
            var mw = multipart.NewWriter(_addr_w);
            foreach (var (_, ra) in ranges)
            {
                mw.CreatePart(ra.mimeHeader(contentType, contentSize));
                encSize += ra.length;
            }
            mw.Close();
            encSize += int64(w);
            return ;

        }

        private static long sumRangesSize(slice<httpRange> ranges)
        {
            long size = default;

            foreach (var (_, ra) in ranges)
            {
                size += ra.length;
            }
            return ;

        }
    }
}}
