// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net.http;

using fmt = fmt_package;
using io = io_package;
using http = go.net.http_package;
using os = os_package;
using path = path_package;
using slices = slices_package;
using strings = strings_package;
using time = time_package;
using go.net;
using url = go.net.url_package;

partial class cgi_package {

internal static void cgiMain() {
    var exprᴛ1 = path.Join(os.Getenv("SCRIPT_NAME"u8), os.Getenv("PATH_INFO"u8));
    if (exprᴛ1 == "/bar"u8 || exprᴛ1 == "/test.cgi"u8 || exprᴛ1 == "/myscript/bar"u8 || exprᴛ1 == "/test.cgi/extrapath"u8) {
        testCGI();
        return;
    }

    childCGIProcess();
}

// testCGI is a CGI program translated from a Perl program to complete host_test.
// test cases in host_test should be provided by testCGI.
internal static void testCGI() {
    var (req, err) = Request();
    if (err != default!) {
        throw panic(err);
    }
    err = req.ParseForm();
    if (err != default!) {
        throw panic(err);
    }
    var @params = req.Value.Form;
    if (@params.Get("loc"u8) != ""u8) {
        fmt.Printf("Location: %s\r\n\r\n"u8, @params.Get("loc"u8));
        return;
    }
    fmt.Printf("Content-Type: text/html\r\n"u8);
    fmt.Printf("X-CGI-Pid: %d\r\n"u8, os.Getpid());
    fmt.Printf("X-Test-Header: X-Test-Value\r\n"u8);
    fmt.Printf("\r\n"u8);
    if (@params.Get("writestderr"u8) != ""u8) {
        fmt.Fprintf(new os.FileжWriter(os.Stderr), "Hello, stderr!\n"u8);
    }
    if (@params.Get("bigresponse"u8) != ""u8) {
        // 17 MB, for OS X: golang.org/issue/4958
        @string line = strings.Repeat("A"u8, 1024);
        for (nint i = 0; i < 17 * 1024; i++) {
            fmt.Printf("%s\r\n"u8, line);
        }
        return;
    }
    fmt.Printf("test=Hello CGI\r\n"u8);
    var keys = new slice<@string>(0, len(@params));
    foreach (var (k, _) in @params) {
        keys = append(keys, k);
    }
    slices.Sort<slice<@string>, @string>(keys);
    foreach (var (_, key) in keys) {
        fmt.Printf("param-%s=%s\r\n"u8, key, @params.Get(key));
    }
    var envs = envMap(os.Environ());
    keys = new slice<@string>(0, len(envs));
    foreach (var (k, _) in envs) {
        keys = append(keys, k);
    }
    slices.Sort<slice<@string>, @string>(keys);
    foreach (var (_, key) in keys) {
        fmt.Printf("env-%s=%s\r\n"u8, key, envs[key]);
    }
    var (cwd, _) = os.Getwd();
    fmt.Printf("cwd=%s\r\n"u8, cwd);
}

[GoType("num:byte")] partial struct neverEnding;

internal static (nint n, error err) Read(this neverEnding b, slice<byte> p) {
    nint n = default!;
    error err = default!;

    foreach (var (i, _) in p) {
        p[i] = (byte)b;
    }
    return (len(p), default!);
}

// childCGIProcess is used by integration_test to complete unit tests.
internal static void childCGIProcess() {
    if (os.Getenv("REQUEST_METHOD"u8) == ""u8) {
        // Not in a CGI environment; skipping test.
        return;
    }
    var exprᴛ1 = os.Getenv("REQUEST_URI"u8);
    if (exprᴛ1 == "/immediate-disconnect"u8) {
        os.Exit(0);
    }
    else if (exprᴛ1 == "/no-content-type"u8) {
        fmt.Printf("Content-Length: 6\n\nHello\n"u8);
        os.Exit(0);
    }
    else if (exprᴛ1 == "/empty-headers"u8) {
        fmt.Printf("\nHello"u8);
        os.Exit(0);
    }

    Serve(new http_HandlerFuncᴠΔHandler(new http.HandlerFunc((http.ResponseWriter rw, ж<http.Request> req) => {
        if (req.FormValue("nil-request-body"u8) == "1"u8) {
            fmt.Fprintf(new http_ResponseWriterᴠWriter(rw), "nil-request-body=%v\n"u8, (~req).Body == default!);
            return;
        }
        rw.Header().Set("X-Test-Header"u8, "X-Test-Value"u8);
        req.ParseForm();
        if (req.FormValue("no-body"u8) == "1"u8) {
            return;
        }
        {
            var (eb, ok) = (~req).Form["exact-body"u8, ꟷ]; if (ok) {
                io.WriteString(new http_ResponseWriterᴠWriter(rw), eb[0]);
                return;
            }
        }
        if (req.FormValue("write-forever"u8) == "1"u8) {
            io.Copy(new http_ResponseWriterᴠWriter(rw), ((neverEnding)(rune)'a'));
            while (ᐧ) {
                time.Sleep(5000000000L);
            }
        }
        // hang forever, until killed
        fmt.Fprintf(new http_ResponseWriterᴠWriter(rw), "test=Hello CGI-in-CGI\n"u8);
        foreach (var (k, vv) in (~req).Form) {
            foreach (var (_, v) in vv) {
                fmt.Fprintf(new http_ResponseWriterᴠWriter(rw), "param-%s=%s\n"u8, k, v);
            }
        }
        foreach (var (_, kv) in os.Environ()) {
            fmt.Fprintf(new http_ResponseWriterᴠWriter(rw), "env-%s\n"u8, kv);
        }
    })));
    os.Exit(0);
}

} // end cgi_package
