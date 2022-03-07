// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build !cmd_go_bootstrap
// +build !cmd_go_bootstrap

// This code is compiled into the real 'go' binary, but it is not
// compiled into the binary that is built during all.bash, so as
// to avoid needing to build net (and thus use cgo) during the
// bootstrap process.

// package web -- go2cs converted at 2022 March 06 23:17:18 UTC
// import "cmd/go/internal/web" ==> using web = go.cmd.go.@internal.web_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\web\http.go
using tls = go.crypto.tls_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using mime = go.mime_package;
using http = go.net.http_package;
using urlpkg = go.net.url_package;
using os = go.os_package;
using strings = go.strings_package;
using time = go.time_package;

using auth = go.cmd.go.@internal.auth_package;
using cfg = go.cmd.go.@internal.cfg_package;
using browser = go.cmd.@internal.browser_package;
using System;


namespace go.cmd.go.@internal;

public static partial class web_package {

    // impatientInsecureHTTPClient is used with GOINSECURE,
    // when we're connecting to https servers that might not be there
    // or might be using self-signed certificates.
private static ptr<http.Client> impatientInsecureHTTPClient = addr(new http.Client(Timeout:5*time.Second,Transport:&http.Transport{Proxy:http.ProxyFromEnvironment,TLSClientConfig:&tls.Config{InsecureSkipVerify:true,},},));

// securityPreservingHTTPClient is like the default HTTP client, but rejects
// redirects to plain-HTTP URLs if the original URL was secure.
private static ptr<http.Client> securityPreservingHTTPClient = addr(new http.Client(CheckRedirect:func(req*http.Request,via[]*http.Request)error{iflen(via)>0&&via[0].URL.Scheme=="https"&&req.URL.Scheme!="https"{lastHop:=via[len(via)-1].URLreturnfmt.Errorf("redirected from secure URL %s to insecure URL %s",lastHop,req.URL)}iflen(via)>=10{returnerrors.New("stopped after 10 redirects")}returnnil},));

private static (ptr<Response>, error) get(SecurityMode security, ptr<urlpkg.URL> _addr_url) => func((_, panic, _) => {
    ptr<Response> _p0 = default!;
    error _p0 = default!;
    ref urlpkg.URL url = ref _addr_url.val;

    var start = time.Now();

    if (url.Scheme == "file") {
        return _addr_getFile(_addr_url)!;
    }
    if (os.Getenv("TESTGOPROXY404") == "1" && url.Host == "proxy.golang.org") {
        ptr<Response> res = addr(new Response(URL:url.Redacted(),Status:"404 testing",StatusCode:404,Header:make(map[string][]string),Body:http.NoBody,));
        if (cfg.BuildX) {
            fmt.Fprintf(os.Stderr, "# get %s: %v (%.3fs)\n", url.Redacted(), res.Status, time.Since(start).Seconds());
        }
        return (_addr_res!, error.As(null!)!);

    }
    if (url.Host == "localhost.localdev") {
        return (_addr_null!, error.As(fmt.Errorf("no such host localhost.localdev"))!);
    }
    if (os.Getenv("TESTGONETWORK") == "panic" && !strings.HasPrefix(url.Host, "127.0.0.1") && !strings.HasPrefix(url.Host, "0.0.0.0")) {
        panic("use of network: " + url.String());
    }
    Func<ptr<urlpkg.URL>, (ptr<urlpkg.URL>, ptr<http.Response>, error)> fetch = url => { 
        // Note: The -v build flag does not mean "print logging information",
        // despite its historical misuse for this in GOPATH-based go get.
        // We print extra logging in -x mode instead, which traces what
        // commands are executed.
        if (cfg.BuildX) {
            fmt.Fprintf(os.Stderr, "# get %s\n", url.Redacted());
        }
        var (req, err) = http.NewRequest("GET", url.String(), null);
        if (err != null) {
            return (_addr_null!, error.As(null!)!, err);
        }
        if (url.Scheme == "https") {
            auth.AddCredentials(req);
        }
        res = ;
        if (security == Insecure && url.Scheme == "https") { // fail earlier
            res, err = impatientInsecureHTTPClient.Do(req);

        }
        else
 {
            res, err = securityPreservingHTTPClient.Do(req);
        }
        return (_addr_url!, error.As(res)!, err);

    };

    ptr<urlpkg.URL> fetched;    res = ;    error err = default!;
    if (url.Scheme == "" || url.Scheme == "https") {
        ptr<urlpkg.URL> secure = @new<urlpkg.URL>();
        secure.val = url;
        secure.Scheme = "https";

        fetched, res, err = fetch(secure);
        if (err != null) {
            if (cfg.BuildX) {
                fmt.Fprintf(os.Stderr, "# get %s: %v\n", secure.Redacted(), err);
            }
            if (security != Insecure || url.Scheme == "https") { 
                // HTTPS failed, and we can't fall back to plain HTTP.
                // Report the error from the HTTPS attempt.
                return (_addr_null!, error.As(err)!);

            }

        }
    }
    if (res == null) {
        switch (url.Scheme) {
            case "http": 
                if (security == SecureOnly) {
                    if (cfg.BuildX) {
                        fmt.Fprintf(os.Stderr, "# get %s: insecure\n", url.Redacted());
                    }
                    return (_addr_null!, error.As(fmt.Errorf("insecure URL: %s", url.Redacted()))!);
                }
                break;
            case "": 
                if (security != Insecure) {
                    panic("should have returned after HTTPS failure");
                }
                break;
            default: 
                if (cfg.BuildX) {
                    fmt.Fprintf(os.Stderr, "# get %s: unsupported\n", url.Redacted());
                }
                return (_addr_null!, error.As(fmt.Errorf("unsupported scheme: %s", url.Redacted()))!);
                break;
        }

        ptr<urlpkg.URL> insecure = @new<urlpkg.URL>();
        insecure.val = url;
        insecure.Scheme = "http";
        if (insecure.User != null && security != Insecure) {
            if (cfg.BuildX) {
                fmt.Fprintf(os.Stderr, "# get %s: insecure credentials\n", insecure.Redacted());
            }
            return (_addr_null!, error.As(fmt.Errorf("refusing to pass credentials to insecure URL: %s", insecure.Redacted()))!);
        }
        fetched, res, err = fetch(insecure);
        if (err != null) {
            if (cfg.BuildX) {
                fmt.Fprintf(os.Stderr, "# get %s: %v\n", insecure.Redacted(), err);
            } 
            // HTTP failed, and we already tried HTTPS if applicable.
            // Report the error from the HTTP attempt.
            return (_addr_null!, error.As(err)!);

        }
    }
    if (cfg.BuildX) {
        fmt.Fprintf(os.Stderr, "# get %s: %v (%.3fs)\n", fetched.Redacted(), res.Status, time.Since(start).Seconds());
    }
    ptr<Response> r = addr(new Response(URL:fetched.Redacted(),Status:res.Status,StatusCode:res.StatusCode,Header:map[string][]string(res.Header),Body:res.Body,));

    if (res.StatusCode != http.StatusOK) {
        var contentType = res.Header.Get("Content-Type");
        {
            var (mediaType, params, _) = mime.ParseMediaType(contentType);

            if (mediaType == "text/plain") {
                {
                    var charset = strings.ToLower(params["charset"]);

                    switch (charset) {
                        case "us-ascii": 
                            // Body claims to be plain text in UTF-8 or a subset thereof.
                            // Try to extract a useful error message from it.

                        case "utf-8": 
                            // Body claims to be plain text in UTF-8 or a subset thereof.
                            // Try to extract a useful error message from it.

                        case "": 
                            // Body claims to be plain text in UTF-8 or a subset thereof.
                            // Try to extract a useful error message from it.
                            r.errorDetail.r = res.Body;
                            r.Body = _addr_r.errorDetail;
                            break;
                    }
                }

            }

        }

    }
    return (_addr_r!, error.As(null!)!);

});

private static (ptr<Response>, error) getFile(ptr<urlpkg.URL> _addr_u) {
    ptr<Response> _p0 = default!;
    error _p0 = default!;
    ref urlpkg.URL u = ref _addr_u.val;

    var (path, err) = urlToFilePath(u);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    var (f, err) = os.Open(path);

    if (os.IsNotExist(err)) {
        return (addr(new Response(URL:u.Redacted(),Status:http.StatusText(http.StatusNotFound),StatusCode:http.StatusNotFound,Body:http.NoBody,fileErr:err,)), error.As(null!)!);
    }
    if (os.IsPermission(err)) {
        return (addr(new Response(URL:u.Redacted(),Status:http.StatusText(http.StatusForbidden),StatusCode:http.StatusForbidden,Body:http.NoBody,fileErr:err,)), error.As(null!)!);
    }
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    return (addr(new Response(URL:u.Redacted(),Status:http.StatusText(http.StatusOK),StatusCode:http.StatusOK,Body:f,)), error.As(null!)!);

}

private static bool openBrowser(@string url) {
    return browser.Open(url);
}

} // end web_package
