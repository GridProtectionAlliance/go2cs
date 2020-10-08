// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package web defines minimal helper routines for accessing HTTP/HTTPS
// resources without requiring external dependencies on the net package.
//
// If the cmd_go_bootstrap build tag is present, web avoids the use of the net
// package and returns errors for all network operations.
// package web -- go2cs converted at 2020 October 08 04:33:56 UTC
// import "cmd/go/internal/web" ==> using web = go.cmd.go.@internal.web_package
// Original source: C:\Go\src\cmd\go\internal\web\api.go
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using io = go.io_package;
using ioutil = go.io.ioutil_package;
using url = go.net.url_package;
using os = go.os_package;
using strings = go.strings_package;
using unicode = go.unicode_package;
using utf8 = go.unicode.utf8_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class web_package
    {
        // SecurityMode specifies whether a function should make network
        // calls using insecure transports (eg, plain text HTTP).
        // The zero value is "secure".
        public partial struct SecurityMode // : long
        {
        }

        public static readonly SecurityMode SecureOnly = (SecurityMode)iota; // Reject plain HTTP; validate HTTPS.
        public static readonly var DefaultSecurity = (var)0; // Allow plain HTTP if explicit; validate HTTPS.
        public static readonly var Insecure = (var)1; // Allow plain HTTP if not explicitly HTTPS; skip HTTPS validation.

        // An HTTPError describes an HTTP error response (non-200 result).
        public partial struct HTTPError
        {
            public @string URL; // redacted
            public @string Status;
            public long StatusCode;
            public error Err; // underlying error, if known
            public @string Detail; // limited to maxErrorDetailLines and maxErrorDetailBytes
        }

        private static readonly long maxErrorDetailLines = (long)8L;
        private static readonly var maxErrorDetailBytes = (var)maxErrorDetailLines * 81L;


        private static @string Error(this ptr<HTTPError> _addr_e)
        {
            ref HTTPError e = ref _addr_e.val;

            if (e.Detail != "")
            {
                @string detailSep = " ";
                if (strings.ContainsRune(e.Detail, '\n'))
                {
                    detailSep = "\n\t";
                }

                return fmt.Sprintf("reading %s: %v\n\tserver response:%s%s", e.URL, e.Status, detailSep, e.Detail);

            }

            {
                var err = e.Err;

                if (err != null)
                {
                    {
                        ptr<os.PathError> (pErr, ok) = e.Err._<ptr<os.PathError>>();

                        if (ok && strings.HasSuffix(e.URL, pErr.Path))
                        { 
                            // Remove the redundant copy of the path.
                            err = pErr.Err;

                        }

                    }

                    return fmt.Sprintf("reading %s: %v", e.URL, err);

                }

            }


            return fmt.Sprintf("reading %s: %v", e.URL, e.Status);

        }

        private static bool Is(this ptr<HTTPError> _addr_e, error target)
        {
            ref HTTPError e = ref _addr_e.val;

            return target == os.ErrNotExist && (e.StatusCode == 404L || e.StatusCode == 410L);
        }

        private static error Unwrap(this ptr<HTTPError> _addr_e)
        {
            ref HTTPError e = ref _addr_e.val;

            return error.As(e.Err)!;
        }

        // GetBytes returns the body of the requested resource, or an error if the
        // response status was not http.StatusOK.
        //
        // GetBytes is a convenience wrapper around Get and Response.Err.
        public static (slice<byte>, error) GetBytes(ptr<url.URL> _addr_u) => func((defer, _, __) =>
        {
            slice<byte> _p0 = default;
            error _p0 = default!;
            ref url.URL u = ref _addr_u.val;

            var (resp, err) = Get(DefaultSecurity, _addr_u);
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            defer(resp.Body.Close());
            {
                var err = resp.Err();

                if (err != null)
                {
                    return (null, error.As(err)!);
                }

            }

            var (b, err) = ioutil.ReadAll(resp.Body);
            if (err != null)
            {
                return (null, error.As(fmt.Errorf("reading %s: %v", u.Redacted(), err))!);
            }

            return (b, error.As(null!)!);

        });

        public partial struct Response
        {
            public @string URL; // redacted
            public @string Status;
            public long StatusCode;
            public map<@string, slice<@string>> Header;
            public io.ReadCloser Body; // Either the original body or &errorDetail.

            public error fileErr;
            public errorDetailBuffer errorDetail;
        }

        // Err returns an *HTTPError corresponding to the response r.
        // If the response r has StatusCode 200 or 0 (unset), Err returns nil.
        // Otherwise, Err may read from r.Body in order to extract relevant error detail.
        private static error Err(this ptr<Response> _addr_r)
        {
            ref Response r = ref _addr_r.val;

            if (r.StatusCode == 200L || r.StatusCode == 0L)
            {
                return error.As(null!)!;
            }

            return error.As(addr(new HTTPError(URL:r.URL,Status:r.Status,StatusCode:r.StatusCode,Err:r.fileErr,Detail:r.formatErrorDetail(),))!)!;

        }

        // formatErrorDetail converts r.errorDetail (a prefix of the output of r.Body)
        // into a short, tab-indented summary.
        private static @string formatErrorDetail(this ptr<Response> _addr_r)
        {
            ref Response r = ref _addr_r.val;

            if (r.Body != _addr_r.errorDetail)
            {
                return ""; // Error detail collection not enabled.
            } 

            // Ensure that r.errorDetail has been populated.
            _, _ = io.Copy(ioutil.Discard, r.Body);

            var s = r.errorDetail.buf.String();
            if (!utf8.ValidString(s))
            {
                return ""; // Don't try to recover non-UTF-8 error messages.
            }

            foreach (var (_, r) in s)
            {
                if (!unicode.IsGraphic(r) && !unicode.IsSpace(r))
                {
                    return ""; // Don't let the server do any funny business with the user's terminal.
                }

            }
            strings.Builder detail = default;
            foreach (var (i, line) in strings.Split(s, "\n"))
            {
                if (strings.TrimSpace(line) == "")
                {
                    break; // Stop at the first blank line.
                }

                if (i > 0L)
                {
                    detail.WriteString("\n\t");
                }

                if (i >= maxErrorDetailLines)
                {
                    detail.WriteString("[Truncated: too many lines.]");
                    break;
                }

                if (detail.Len() + len(line) > maxErrorDetailBytes)
                {
                    detail.WriteString("[Truncated: too long.]");
                    break;
                }

                detail.WriteString(line);

            }
            return detail.String();

        }

        // Get returns the body of the HTTP or HTTPS resource specified at the given URL.
        //
        // If the URL does not include an explicit scheme, Get first tries "https".
        // If the server does not respond under that scheme and the security mode is
        // Insecure, Get then tries "http".
        // The URL included in the response indicates which scheme was actually used,
        // and it is a redacted URL suitable for use in error messages.
        //
        // For the "https" scheme only, credentials are attached using the
        // cmd/go/internal/auth package. If the URL itself includes a username and
        // password, it will not be attempted under the "http" scheme unless the
        // security mode is Insecure.
        //
        // Get returns a non-nil error only if the request did not receive a response
        // under any applicable scheme. (A non-2xx response does not cause an error.)
        public static (ptr<Response>, error) Get(SecurityMode security, ptr<url.URL> _addr_u)
        {
            ptr<Response> _p0 = default!;
            error _p0 = default!;
            ref url.URL u = ref _addr_u.val;

            return _addr_get(security, u)!;
        }

        // OpenBrowser attempts to open the requested URL in a web browser.
        public static bool OpenBrowser(@string url)
        {
            bool opened = default;

            return openBrowser(url);
        }

        // Join returns the result of adding the slash-separated
        // path elements to the end of u's path.
        public static ptr<url.URL> Join(ptr<url.URL> _addr_u, @string path)
        {
            ref url.URL u = ref _addr_u.val;

            ref url.URL j = ref heap(u, out ptr<url.URL> _addr_j);
            if (path == "")
            {
                return _addr__addr_j!;
            }

            j.Path = strings.TrimSuffix(u.Path, "/") + "/" + strings.TrimPrefix(path, "/");
            j.RawPath = strings.TrimSuffix(u.RawPath, "/") + "/" + strings.TrimPrefix(path, "/");
            return _addr__addr_j!;

        }

        // An errorDetailBuffer is an io.ReadCloser that copies up to
        // maxErrorDetailLines into a buffer for later inspection.
        private partial struct errorDetailBuffer
        {
            public io.ReadCloser r;
            public strings.Builder buf;
            public long bufLines;
        }

        private static error Close(this ptr<errorDetailBuffer> _addr_b)
        {
            ref errorDetailBuffer b = ref _addr_b.val;

            return error.As(b.r.Close())!;
        }

        private static (long, error) Read(this ptr<errorDetailBuffer> _addr_b, slice<byte> p)
        {
            long n = default;
            error err = default!;
            ref errorDetailBuffer b = ref _addr_b.val;

            n, err = b.r.Read(p); 

            // Copy the first maxErrorDetailLines+1 lines into b.buf,
            // discarding any further lines.
            //
            // Note that the read may begin or end in the middle of a UTF-8 character,
            // so don't try to do anything fancy with characters that encode to larger
            // than one byte.
            if (b.bufLines <= maxErrorDetailLines)
            {
                foreach (var (_, line) in bytes.SplitAfterN(p[..n], (slice<byte>)"\n", maxErrorDetailLines - b.bufLines))
                {
                    b.buf.Write(line);
                    if (len(line) > 0L && line[len(line) - 1L] == '\n')
                    {
                        b.bufLines++;
                        if (b.bufLines > maxErrorDetailLines)
                        {
                            break;
                        }

                    }

                }

            }

            return (n, error.As(err)!);

        }
    }
}}}}
