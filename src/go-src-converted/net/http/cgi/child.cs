// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements CGI from the perspective of a child
// process.

// package cgi -- go2cs converted at 2020 August 29 08:34:01 UTC
// import "net/http/cgi" ==> using cgi = go.net.http.cgi_package
// Original source: C:\Go\src\net\http\cgi\child.go
using bufio = go.bufio_package;
using tls = go.crypto.tls_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using io = go.io_package;
using ioutil = go.io.ioutil_package;
using net = go.net_package;
using http = go.net.http_package;
using url = go.net.url_package;
using os = go.os_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using static go.builtin;

namespace go {
namespace net {
namespace http
{
    public static partial class cgi_package
    {
        // Request returns the HTTP request as represented in the current
        // environment. This assumes the current program is being run
        // by a web server in a CGI environment.
        // The returned Request's Body is populated, if applicable.
        public static (ref http.Request, error) Request()
        {
            var (r, err) = RequestFromMap(envMap(os.Environ()));
            if (err != null)
            {
                return (null, err);
            }
            if (r.ContentLength > 0L)
            {
                r.Body = ioutil.NopCloser(io.LimitReader(os.Stdin, r.ContentLength));
            }
            return (r, null);
        }

        private static map<@string, @string> envMap(slice<@string> env)
        {
            var m = make_map<@string, @string>();
            foreach (var (_, kv) in env)
            {
                {
                    var idx = strings.Index(kv, "=");

                    if (idx != -1L)
                    {
                        m[kv[..idx]] = kv[idx + 1L..];
                    }

                }
            }
            return m;
        }

        // RequestFromMap creates an http.Request from CGI variables.
        // The returned Request's Body field is not populated.
        public static (ref http.Request, error) RequestFromMap(map<@string, @string> @params)
        {
            ptr<http.Request> r = @new<http.Request>();
            r.Method = params["REQUEST_METHOD"];
            if (r.Method == "")
            {
                return (null, errors.New("cgi: no REQUEST_METHOD in environment"));
            }
            r.Proto = params["SERVER_PROTOCOL"];
            bool ok = default;
            r.ProtoMajor, r.ProtoMinor, ok = http.ParseHTTPVersion(r.Proto);
            if (!ok)
            {
                return (null, errors.New("cgi: invalid SERVER_PROTOCOL version"));
            }
            r.Close = true;
            r.Trailer = new http.Header();
            r.Header = new http.Header();

            r.Host = params["HTTP_HOST"];

            {
                var lenstr = params["CONTENT_LENGTH"];

                if (lenstr != "")
                {
                    var (clen, err) = strconv.ParseInt(lenstr, 10L, 64L);
                    if (err != null)
                    {
                        return (null, errors.New("cgi: bad CONTENT_LENGTH in environment: " + lenstr));
                    }
                    r.ContentLength = clen;
                }

            }

            {
                var ct = params["CONTENT_TYPE"];

                if (ct != "")
                {
                    r.Header.Set("Content-Type", ct);
                } 

                // Copy "HTTP_FOO_BAR" variables to "Foo-Bar" Headers

            } 

            // Copy "HTTP_FOO_BAR" variables to "Foo-Bar" Headers
            foreach (var (k, v) in params)
            {
                if (!strings.HasPrefix(k, "HTTP_") || k == "HTTP_HOST")
                {
                    continue;
                }
                r.Header.Add(strings.Replace(k[5L..], "_", "-", -1L), v);
            } 

            // TODO: cookies.  parsing them isn't exported, though.
            var uriStr = params["REQUEST_URI"];
            if (uriStr == "")
            { 
                // Fallback to SCRIPT_NAME, PATH_INFO and QUERY_STRING.
                uriStr = params["SCRIPT_NAME"] + params["PATH_INFO"];
                var s = params["QUERY_STRING"];
                if (s != "")
                {
                    uriStr += "?" + s;
                }
            } 

            // There's apparently a de-facto standard for this.
            // http://docstore.mik.ua/orelly/linux/cgi/ch03_02.htm#ch03-35636
            {
                var s__prev1 = s;

                s = params["HTTPS"];

                if (s == "on" || s == "ON" || s == "1")
                {
                    r.TLS = ref new tls.ConnectionState(HandshakeComplete:true);
                }

                s = s__prev1;

            }

            if (r.Host != "")
            { 
                // Hostname is provided, so we can reasonably construct a URL.
                var rawurl = r.Host + uriStr;
                if (r.TLS == null)
                {
                    rawurl = "http://" + rawurl;
                }
                else
                {
                    rawurl = "https://" + rawurl;
                }
                var (url, err) = url.Parse(rawurl);
                if (err != null)
                {
                    return (null, errors.New("cgi: failed to parse host and REQUEST_URI into a URL: " + rawurl));
                }
                r.URL = url;
            } 
            // Fallback logic if we don't have a Host header or the URL
            // failed to parse
            if (r.URL == null)
            {
                (url, err) = url.Parse(uriStr);
                if (err != null)
                {
                    return (null, errors.New("cgi: failed to parse REQUEST_URI into a URL: " + uriStr));
                }
                r.URL = url;
            } 

            // Request.RemoteAddr has its port set by Go's standard http
            // server, so we do here too.
            var (remotePort, _) = strconv.Atoi(params["REMOTE_PORT"]); // zero if unset or invalid
            r.RemoteAddr = net.JoinHostPort(params["REMOTE_ADDR"], strconv.Itoa(remotePort));

            return (r, null);
        }

        // Serve executes the provided Handler on the currently active CGI
        // request, if any. If there's no current CGI environment
        // an error is returned. The provided handler may be nil to use
        // http.DefaultServeMux.
        public static error Serve(http.Handler handler)
        {
            var (req, err) = Request();
            if (err != null)
            {
                return error.As(err);
            }
            if (handler == null)
            {
                handler = http.DefaultServeMux;
            }
            response rw = ref new response(req:req,header:make(http.Header),bufw:bufio.NewWriter(os.Stdout),);
            handler.ServeHTTP(rw, req);
            rw.Write(null); // make sure a response is sent
            err = rw.bufw.Flush();

            if (err != null)
            {
                return error.As(err);
            }
            return error.As(null);
        }

        private partial struct response
        {
            public ptr<http.Request> req;
            public http.Header header;
            public ptr<bufio.Writer> bufw;
            public bool headerSent;
        }

        private static void Flush(this ref response r)
        {
            r.bufw.Flush();
        }

        private static http.Header Header(this ref response r)
        {
            return r.header;
        }

        private static (long, error) Write(this ref response r, slice<byte> p)
        {
            if (!r.headerSent)
            {
                r.WriteHeader(http.StatusOK);
            }
            return r.bufw.Write(p);
        }

        private static void WriteHeader(this ref response r, long code)
        {
            if (r.headerSent)
            { 
                // Note: explicitly using Stderr, as Stdout is our HTTP output.
                fmt.Fprintf(os.Stderr, "CGI attempted to write header twice on request for %s", r.req.URL);
                return;
            }
            r.headerSent = true;
            fmt.Fprintf(r.bufw, "Status: %d %s\r\n", code, http.StatusText(code)); 

            // Set a default Content-Type
            {
                var (_, hasType) = r.header["Content-Type"];

                if (!hasType)
                {
                    r.header.Add("Content-Type", "text/html; charset=utf-8");
                }

            }

            r.header.Write(r.bufw);
            r.bufw.WriteString("\r\n");
            r.bufw.Flush();
        }
    }
}}}
