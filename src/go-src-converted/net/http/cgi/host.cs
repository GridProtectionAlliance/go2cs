// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements the host side of CGI (being the webserver
// parent process).

// Package cgi implements CGI (Common Gateway Interface) as specified
// in RFC 3875.
//
// Note that using CGI means starting a new process to handle each
// request, which is typically less efficient than using a
// long-running server. This package is intended primarily for
// compatibility with existing systems.
// package cgi -- go2cs converted at 2022 March 06 22:23:27 UTC
// import "net/http/cgi" ==> using cgi = go.net.http.cgi_package
// Original source: C:\Program Files\Go\src\net\http\cgi\host.go
using bufio = go.bufio_package;
using fmt = go.fmt_package;
using io = go.io_package;
using log = go.log_package;
using net = go.net_package;
using http = go.net.http_package;
using textproto = go.net.textproto_package;
using os = go.os_package;
using exec = go.os.exec_package;
using filepath = go.path.filepath_package;
using regexp = go.regexp_package;
using runtime = go.runtime_package;
using strconv = go.strconv_package;
using strings = go.strings_package;

using httpguts = go.golang.org.x.net.http.httpguts_package;
using System;


namespace go.net.http;

public static partial class cgi_package {

private static var trailingPort = regexp.MustCompile(":([0-9]+)$");

private static Func<slice<@string>> osDefaultInheritEnv = () => {
    switch (runtime.GOOS) {
        case "darwin": 

        case "ios": 
            return new slice<@string>(new @string[] { "DYLD_LIBRARY_PATH" });
            break;
        case "linux": 

        case "freebsd": 

        case "netbsd": 

        case "openbsd": 
            return new slice<@string>(new @string[] { "LD_LIBRARY_PATH" });
            break;
        case "hpux": 
            return new slice<@string>(new @string[] { "LD_LIBRARY_PATH", "SHLIB_PATH" });
            break;
        case "irix": 
            return new slice<@string>(new @string[] { "LD_LIBRARY_PATH", "LD_LIBRARYN32_PATH", "LD_LIBRARY64_PATH" });
            break;
        case "illumos": 

        case "solaris": 
            return new slice<@string>(new @string[] { "LD_LIBRARY_PATH", "LD_LIBRARY_PATH_32", "LD_LIBRARY_PATH_64" });
            break;
        case "windows": 
            return new slice<@string>(new @string[] { "SystemRoot", "COMSPEC", "PATHEXT", "WINDIR" });
            break;
    }
    return null;

}();

// Handler runs an executable in a subprocess with a CGI environment.
public partial struct Handler {
    public @string Path; // path to the CGI executable
    public @string Root; // root URI prefix of handler or empty for "/"

// Dir specifies the CGI executable's working directory.
// If Dir is empty, the base directory of Path is used.
// If Path has no base directory, the current working
// directory is used.
    public @string Dir;
    public slice<@string> Env; // extra environment variables to set, if any, as "key=value"
    public slice<@string> InheritEnv; // environment variables to inherit from host, as "key"
    public ptr<log.Logger> Logger; // optional log for errors or nil to use log.Print
    public slice<@string> Args; // optional arguments to pass to child process
    public io.Writer Stderr; // optional stderr for the child process; nil means os.Stderr

// PathLocationHandler specifies the root http Handler that
// should handle internal redirects when the CGI process
// returns a Location header value starting with a "/", as
// specified in RFC 3875 § 6.3.2. This will likely be
// http.DefaultServeMux.
//
// If nil, a CGI response with a local URI path is instead sent
// back to the client and not redirected internally.
    public http.Handler PathLocationHandler;
}

private static io.Writer stderr(this ptr<Handler> _addr_h) {
    ref Handler h = ref _addr_h.val;

    if (h.Stderr != null) {
        return h.Stderr;
    }
    return os.Stderr;

}

// removeLeadingDuplicates remove leading duplicate in environments.
// It's possible to override environment like following.
//    cgi.Handler{
//      ...
//      Env: []string{"SCRIPT_FILENAME=foo.php"},
//    }
private static slice<@string> removeLeadingDuplicates(slice<@string> env) {
    slice<@string> ret = default;

    foreach (var (i, e) in env) {
        var found = false;
        {
            var eq = strings.IndexByte(e, '=');

            if (eq != -1) {
                var keq = e[..(int)eq + 1]; // "key="
                foreach (var (_, e2) in env[(int)i + 1..]) {
                    if (strings.HasPrefix(e2, keq)) {
                        found = true;
                        break;
                    }
                }

            }

        }

        if (!found) {
            ret = append(ret, e);
        }
    }    return ;

}

private static void ServeHTTP(this ptr<Handler> _addr_h, http.ResponseWriter rw, ptr<http.Request> _addr_req) => func((defer, _, _) => {
    ref Handler h = ref _addr_h.val;
    ref http.Request req = ref _addr_req.val;

    var root = h.Root;
    if (root == "") {
        root = "/";
    }
    if (len(req.TransferEncoding) > 0 && req.TransferEncoding[0] == "chunked") {
        rw.WriteHeader(http.StatusBadRequest);
        rw.Write((slice<byte>)"Chunked request bodies are not supported by CGI.");
        return ;
    }
    var pathInfo = req.URL.Path;
    if (root != "/" && strings.HasPrefix(pathInfo, root)) {
        pathInfo = pathInfo[(int)len(root)..];
    }
    @string port = "80";
    {
        var matches = trailingPort.FindStringSubmatch(req.Host);

        if (len(matches) != 0) {
            port = matches[1];
        }
    }


    @string env = new slice<@string>(new @string[] { "SERVER_SOFTWARE=go", "SERVER_NAME="+req.Host, "SERVER_PROTOCOL=HTTP/1.1", "HTTP_HOST="+req.Host, "GATEWAY_INTERFACE=CGI/1.1", "REQUEST_METHOD="+req.Method, "QUERY_STRING="+req.URL.RawQuery, "REQUEST_URI="+req.URL.RequestURI(), "PATH_INFO="+pathInfo, "SCRIPT_NAME="+root, "SCRIPT_FILENAME="+h.Path, "SERVER_PORT="+port });

    {
        var (remoteIP, remotePort, err) = net.SplitHostPort(req.RemoteAddr);

        if (err == null) {
            env = append(env, "REMOTE_ADDR=" + remoteIP, "REMOTE_HOST=" + remoteIP, "REMOTE_PORT=" + remotePort);
        }
        else
 { 
            // could not parse ip:port, let's use whole RemoteAddr and leave REMOTE_PORT undefined
            env = append(env, "REMOTE_ADDR=" + req.RemoteAddr, "REMOTE_HOST=" + req.RemoteAddr);

        }
    }


    if (req.TLS != null) {
        env = append(env, "HTTPS=on");
    }
    {
        var k__prev1 = k;
        var v__prev1 = v;

        foreach (var (__k, __v) in req.Header) {
            k = __k;
            v = __v;
            k = strings.Map(upperCaseAndUnderscore, k);
            if (k == "PROXY") { 
                // See Issue 16405
                continue;

            }

            @string joinStr = ", ";
            if (k == "COOKIE") {
                joinStr = "; ";
            }

            env = append(env, "HTTP_" + k + "=" + strings.Join(v, joinStr));

        }
        k = k__prev1;
        v = v__prev1;
    }

    if (req.ContentLength > 0) {
        env = append(env, fmt.Sprintf("CONTENT_LENGTH=%d", req.ContentLength));
    }
    {
        var ctype = req.Header.Get("Content-Type");

        if (ctype != "") {
            env = append(env, "CONTENT_TYPE=" + ctype);
        }
    }


    var envPath = os.Getenv("PATH");
    if (envPath == "") {
        envPath = "/bin:/usr/bin:/usr/ucb:/usr/bsd:/usr/local/bin";
    }
    env = append(env, "PATH=" + envPath);

    {
        var e__prev1 = e;

        foreach (var (_, __e) in h.InheritEnv) {
            e = __e;
            {
                var v__prev1 = v;

                var v = os.Getenv(e);

                if (v != "") {
                    env = append(env, e + "=" + v);
                }

                v = v__prev1;

            }

        }
        e = e__prev1;
    }

    {
        var e__prev1 = e;

        foreach (var (_, __e) in osDefaultInheritEnv) {
            e = __e;
            {
                var v__prev1 = v;

                v = os.Getenv(e);

                if (v != "") {
                    env = append(env, e + "=" + v);
                }

                v = v__prev1;

            }

        }
        e = e__prev1;
    }

    if (h.Env != null) {
        env = append(env, h.Env);
    }
    env = removeLeadingDuplicates(env);

    @string cwd = default;    @string path = default;

    if (h.Dir != "") {
        path = h.Path;
        cwd = h.Dir;
    }
    else
 {
        cwd, path = filepath.Split(h.Path);
    }
    if (cwd == "") {
        cwd = ".";
    }
    Action<error> internalError = err => {
        rw.WriteHeader(http.StatusInternalServerError);
        h.printf("CGI error: %v", err);
    };

    ptr<exec.Cmd> cmd = addr(new exec.Cmd(Path:path,Args:append([]string{h.Path},h.Args...),Dir:cwd,Env:env,Stderr:h.stderr(),));
    if (req.ContentLength != 0) {
        cmd.Stdin = req.Body;
    }
    var (stdoutRead, err) = cmd.StdoutPipe();
    if (err != null) {
        internalError(err);
        return ;
    }
    err = cmd.Start();
    if (err != null) {
        internalError(err);
        return ;
    }
    {
        var hook = testHookStartProcess;

        if (hook != null) {
            hook(cmd.Process);
        }
    }

    defer(cmd.Wait());
    defer(stdoutRead.Close());

    var linebody = bufio.NewReaderSize(stdoutRead, 1024);
    var headers = make(http.Header);
    nint statusCode = 0;
    nint headerLines = 0;
    var sawBlankLine = false;
    while (true) {
        var (line, isPrefix, err) = linebody.ReadLine();
        if (isPrefix) {
            rw.WriteHeader(http.StatusInternalServerError);
            h.printf("cgi: long header line from subprocess.");
            return ;
        }
        if (err == io.EOF) {
            break;
        }
        if (err != null) {
            rw.WriteHeader(http.StatusInternalServerError);
            h.printf("cgi: error reading headers: %v", err);
            return ;
        }
        if (len(line) == 0) {
            sawBlankLine = true;
            break;
        }
        headerLines++;
        var parts = strings.SplitN(string(line), ":", 2);
        if (len(parts) < 2) {
            h.printf("cgi: bogus header line: %s", string(line));
            continue;
        }
        var header = parts[0];
        var val = parts[1];
        if (!httpguts.ValidHeaderFieldName(header)) {
            h.printf("cgi: invalid header name: %q", header);
            continue;
        }
        val = textproto.TrimString(val);

        if (header == "Status") 
            if (len(val) < 3) {
                h.printf("cgi: bogus status (short): %q", val);
                return ;
            }
            var (code, err) = strconv.Atoi(val[(int)0..(int)3]);
            if (err != null) {
                h.printf("cgi: bogus status: %q", val);
                h.printf("cgi: line was %q", line);
                return ;
            }
            statusCode = code;
        else 
            headers.Add(header, val);
        
    }
    if (headerLines == 0 || !sawBlankLine) {
        rw.WriteHeader(http.StatusInternalServerError);
        h.printf("cgi: no headers");
        return ;
    }
    {
        var loc = headers.Get("Location");

        if (loc != "") {
            if (strings.HasPrefix(loc, "/") && h.PathLocationHandler != null) {
                h.handleInternalRedirect(rw, req, loc);
                return ;
            }
            if (statusCode == 0) {
                statusCode = http.StatusFound;
            }
        }
    }


    if (statusCode == 0 && headers.Get("Content-Type") == "") {
        rw.WriteHeader(http.StatusInternalServerError);
        h.printf("cgi: missing required Content-Type in headers");
        return ;
    }
    if (statusCode == 0) {
        statusCode = http.StatusOK;
    }
    {
        var k__prev1 = k;

        foreach (var (__k, __vv) in headers) {
            k = __k;
            vv = __vv;
            {
                var v__prev2 = v;

                foreach (var (_, __v) in vv) {
                    v = __v;
                    rw.Header().Add(k, v);
                }

                v = v__prev2;
            }
        }
        k = k__prev1;
    }

    rw.WriteHeader(statusCode);

    _, err = io.Copy(rw, linebody);
    if (err != null) {
        h.printf("cgi: copy error: %v", err); 
        // And kill the child CGI process so we don't hang on
        // the deferred cmd.Wait above if the error was just
        // the client (rw) going away. If it was a read error
        // (because the child died itself), then the extra
        // kill of an already-dead process is harmless (the PID
        // won't be reused until the Wait above).
        cmd.Process.Kill();

    }
});

private static void printf(this ptr<Handler> _addr_h, @string format, params object[] v) {
    v = v.Clone();
    ref Handler h = ref _addr_h.val;

    if (h.Logger != null) {
        h.Logger.Printf(format, v);
    }
    else
 {
        log.Printf(format, v);
    }
}

private static void handleInternalRedirect(this ptr<Handler> _addr_h, http.ResponseWriter rw, ptr<http.Request> _addr_req, @string path) {
    ref Handler h = ref _addr_h.val;
    ref http.Request req = ref _addr_req.val;

    var (url, err) = req.URL.Parse(path);
    if (err != null) {
        rw.WriteHeader(http.StatusInternalServerError);
        h.printf("cgi: error resolving local URI path %q: %v", path, err);
        return ;
    }
    ptr<http.Request> newReq = addr(new http.Request(Method:"GET",URL:url,Proto:"HTTP/1.1",ProtoMajor:1,ProtoMinor:1,Header:make(http.Header),Host:url.Host,RemoteAddr:req.RemoteAddr,TLS:req.TLS,));
    h.PathLocationHandler.ServeHTTP(rw, newReq);

}

private static int upperCaseAndUnderscore(int r) {

    if (r >= 'a' && r <= 'z') 
        return r - ('a' - 'A');
    else if (r == '-') 
        return '_';
    else if (r == '=') 
        // Maybe not part of the CGI 'spec' but would mess up
        // the environment in any case, as Go represents the
        // environment as a slice of "key=value" strings.
        return '_';
    // TODO: other transformations in spec or practice?
    return r;

}

private static Action<ptr<os.Process>> testHookStartProcess = default; // nil except for some tests

} // end cgi_package
