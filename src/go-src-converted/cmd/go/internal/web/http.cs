// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !cmd_go_bootstrap

// This code is compiled into the real 'go' binary, but it is not
// compiled into the binary that is built during all.bash, so as
// to avoid needing to build net (and thus use cgo) during the
// bootstrap process.

// package web -- go2cs converted at 2020 August 29 10:01:09 UTC
// import "cmd/go/internal/web" ==> using web = go.cmd.go.@internal.web_package
// Original source: C:\Go\src\cmd\go\internal\web\http.go
using tls = go.crypto.tls_package;
using fmt = go.fmt_package;
using io = go.io_package;
using ioutil = go.io.ioutil_package;
using log = go.log_package;
using http = go.net.http_package;
using url = go.net.url_package;
using time = go.time_package;

using cfg = go.cmd.go.@internal.cfg_package;
using browser = go.cmd.@internal.browser_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class web_package
    {
        // httpClient is the default HTTP client, but a variable so it can be
        // changed by tests, without modifying http.DefaultClient.
        private static var httpClient = http.DefaultClient;

        // impatientInsecureHTTPClient is used in -insecure mode,
        // when we're connecting to https servers that might not be there
        // or might be using self-signed certificates.
        private static http.Client impatientInsecureHTTPClient = ref new http.Client(Timeout:5*time.Second,Transport:&http.Transport{Proxy:http.ProxyFromEnvironment,TLSClientConfig:&tls.Config{InsecureSkipVerify:true,},},);

        public partial struct HTTPError
        {
            public @string status;
            public long StatusCode;
            public @string url;
        }

        private static @string Error(this ref HTTPError e)
        {
            return fmt.Sprintf("%s: %s", e.url, e.status);
        }

        // Get returns the data from an HTTP GET request for the given URL.
        public static (slice<byte>, error) Get(@string url) => func((defer, _, __) =>
        {
            var (resp, err) = httpClient.Get(url);
            if (err != null)
            {
                return (null, err);
            }
            defer(resp.Body.Close());
            if (resp.StatusCode != 200L)
            {
                HTTPError err = ref new HTTPError(status:resp.Status,StatusCode:resp.StatusCode,url:url);

                return (null, err);
            }
            var (b, err) = ioutil.ReadAll(resp.Body);
            if (err != null)
            {
                return (null, fmt.Errorf("%s: %v", url, err));
            }
            return (b, null);
        });

        // GetMaybeInsecure returns the body of either the importPath's
        // https resource or, if unavailable and permitted by the security mode, the http resource.
        public static (@string, io.ReadCloser, error) GetMaybeInsecure(@string importPath, SecurityMode security)
        {
            Func<@string, (@string, ref http.Response, error)> fetch = scheme =>
            {
                var (u, err) = url.Parse(scheme + "://" + importPath);
                if (err != null)
                {
                    return ("", null, err);
                }
                u.RawQuery = "go-get=1";
                urlStr = u.String();
                if (cfg.BuildV)
                {
                    log.Printf("Fetching %s", urlStr);
                }
                if (security == Insecure && scheme == "https")
                { // fail earlier
                    res, err = impatientInsecureHTTPClient.Get(urlStr);
                }
                else
                {
                    res, err = httpClient.Get(urlStr);
                }
                return;
            }
;
            Action<ref http.Response> closeBody = res =>
            {
                if (res != null)
                {
                    res.Body.Close();
                }
            }
;
            var (urlStr, res, err) = fetch("https");
            if (err != null)
            {
                if (cfg.BuildV)
                {
                    log.Printf("https fetch failed: %v", err);
                }
                if (security == Insecure)
                {
                    closeBody(res);
                    urlStr, res, err = fetch("http");
                }
            }
            if (err != null)
            {
                closeBody(res);
                return ("", null, err);
            } 
            // Note: accepting a non-200 OK here, so people can serve a
            // meta import in their http 404 page.
            if (cfg.BuildV)
            {
                log.Printf("Parsing meta tags from %s (status code %d)", urlStr, res.StatusCode);
            }
            return (urlStr, res.Body, null);
        }

        public static @string QueryEscape(@string s)
        {
            return url.QueryEscape(s);
        }
        public static bool OpenBrowser(@string url)
        {
            return browser.Open(url);
        }
    }
}}}}
