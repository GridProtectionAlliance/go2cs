// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package pprof serves via its HTTP server runtime profiling data
// in the format expected by the pprof visualization tool.
//
// The package is typically only imported for the side effect of
// registering its HTTP handlers.
// The handled paths all begin with /debug/pprof/.
//
// To use pprof, link this package into your program:
//    import _ "net/http/pprof"
//
// If your application is not already running an http server, you
// need to start one. Add "net/http" and "log" to your imports and
// the following code to your main function:
//
//     go func() {
//         log.Println(http.ListenAndServe("localhost:6060", nil))
//     }()
//
// Then use the pprof tool to look at the heap profile:
//
//    go tool pprof http://localhost:6060/debug/pprof/heap
//
// Or to look at a 30-second CPU profile:
//
//    go tool pprof http://localhost:6060/debug/pprof/profile
//
// Or to look at the goroutine blocking profile, after calling
// runtime.SetBlockProfileRate in your program:
//
//    go tool pprof http://localhost:6060/debug/pprof/block
//
// Or to collect a 5-second execution trace:
//
//    wget http://localhost:6060/debug/pprof/trace?seconds=5
//
// Or to look at the holders of contended mutexes, after calling
// runtime.SetMutexProfileFraction in your program:
//
//    go tool pprof http://localhost:6060/debug/pprof/mutex
//
// To view all available profiles, open http://localhost:6060/debug/pprof/
// in your browser.
//
// For a study of the facility in action, visit
//
//    https://blog.golang.org/2011/06/profiling-go-programs.html
//
// package pprof -- go2cs converted at 2020 August 29 08:34:28 UTC
// import "net/http/pprof" ==> using pprof = go.net.http.pprof_package
// Original source: C:\Go\src\net\http\pprof\pprof.go
using bufio = go.bufio_package;
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using template = go.html.template_package;
using io = go.io_package;
using log = go.log_package;
using http = go.net.http_package;
using os = go.os_package;
using runtime = go.runtime_package;
using pprof = go.runtime.pprof_package;
using trace = go.runtime.trace_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using time = go.time_package;
using static go.builtin;

namespace go {
namespace net {
namespace http
{
    public static partial class pprof_package
    {
        private static void init()
        {
            http.HandleFunc("/debug/pprof/", Index);
            http.HandleFunc("/debug/pprof/cmdline", Cmdline);
            http.HandleFunc("/debug/pprof/profile", Profile);
            http.HandleFunc("/debug/pprof/symbol", Symbol);
            http.HandleFunc("/debug/pprof/trace", Trace);
        }

        // Cmdline responds with the running program's
        // command line, with arguments separated by NUL bytes.
        // The package initialization registers it as /debug/pprof/cmdline.
        public static void Cmdline(http.ResponseWriter w, ref http.Request r)
        {
            w.Header().Set("X-Content-Type-Options", "nosniff");
            w.Header().Set("Content-Type", "text/plain; charset=utf-8");
            fmt.Fprintf(w, strings.Join(os.Args, "\x00"));
        }

        private static void sleep(http.ResponseWriter w, time.Duration d)
        {
            channel<bool> clientGone = default;
            {
                http.CloseNotifier (cn, ok) = w._<http.CloseNotifier>();

                if (ok)
                {
                    clientGone = cn.CloseNotify();
                }

            }
        }

        private static bool durationExceedsWriteTimeout(ref http.Request r, double seconds)
        {
            ref http.Server (srv, ok) = r.Context().Value(http.ServerContextKey)._<ref http.Server>();
            return ok && srv.WriteTimeout != 0L && seconds >= srv.WriteTimeout.Seconds();
        }

        private static void serveError(http.ResponseWriter w, long status, @string txt)
        {
            w.Header().Set("Content-Type", "text/plain; charset=utf-8");
            w.Header().Set("X-Go-Pprof", "1");
            w.Header().Del("Content-Disposition");
            w.WriteHeader(status);
            fmt.Fprintln(w, txt);
        }

        // Profile responds with the pprof-formatted cpu profile.
        // The package initialization registers it as /debug/pprof/profile.
        public static void Profile(http.ResponseWriter w, ref http.Request r)
        {
            w.Header().Set("X-Content-Type-Options", "nosniff");
            var (sec, _) = strconv.ParseInt(r.FormValue("seconds"), 10L, 64L);
            if (sec == 0L)
            {
                sec = 30L;
            }
            if (durationExceedsWriteTimeout(r, float64(sec)))
            {
                serveError(w, http.StatusBadRequest, "profile duration exceeds server's WriteTimeout");
                return;
            } 

            // Set Content Type assuming StartCPUProfile will work,
            // because if it does it starts writing.
            w.Header().Set("Content-Type", "application/octet-stream");
            w.Header().Set("Content-Disposition", "attachment; filename=\"profile\"");
            {
                var err = pprof.StartCPUProfile(w);

                if (err != null)
                { 
                    // StartCPUProfile failed, so no writes yet.
                    serveError(w, http.StatusInternalServerError, fmt.Sprintf("Could not enable CPU profiling: %s", err));
                    return;
                }

            }
            sleep(w, time.Duration(sec) * time.Second);
            pprof.StopCPUProfile();
        }

        // Trace responds with the execution trace in binary form.
        // Tracing lasts for duration specified in seconds GET parameter, or for 1 second if not specified.
        // The package initialization registers it as /debug/pprof/trace.
        public static void Trace(http.ResponseWriter w, ref http.Request r)
        {
            w.Header().Set("X-Content-Type-Options", "nosniff");
            var (sec, err) = strconv.ParseFloat(r.FormValue("seconds"), 64L);
            if (sec <= 0L || err != null)
            {
                sec = 1L;
            }
            if (durationExceedsWriteTimeout(r, sec))
            {
                serveError(w, http.StatusBadRequest, "profile duration exceeds server's WriteTimeout");
                return;
            } 

            // Set Content Type assuming trace.Start will work,
            // because if it does it starts writing.
            w.Header().Set("Content-Type", "application/octet-stream");
            w.Header().Set("Content-Disposition", "attachment; filename=\"trace\"");
            {
                var err = trace.Start(w);

                if (err != null)
                { 
                    // trace.Start failed, so no writes yet.
                    serveError(w, http.StatusInternalServerError, fmt.Sprintf("Could not enable tracing: %s", err));
                    return;
                }

            }
            sleep(w, time.Duration(sec * float64(time.Second)));
            trace.Stop();
        }

        // Symbol looks up the program counters listed in the request,
        // responding with a table mapping program counters to function names.
        // The package initialization registers it as /debug/pprof/symbol.
        public static void Symbol(http.ResponseWriter w, ref http.Request r)
        {
            w.Header().Set("X-Content-Type-Options", "nosniff");
            w.Header().Set("Content-Type", "text/plain; charset=utf-8"); 

            // We have to read the whole POST body before
            // writing any output. Buffer the output here.
            bytes.Buffer buf = default; 

            // We don't know how many symbols we have, but we
            // do have symbol information. Pprof only cares whether
            // this number is 0 (no symbols available) or > 0.
            fmt.Fprintf(ref buf, "num_symbols: 1\n");

            ref bufio.Reader b = default;
            if (r.Method == "POST")
            {
                b = bufio.NewReader(r.Body);
            }
            else
            {
                b = bufio.NewReader(strings.NewReader(r.URL.RawQuery));
            }
            while (true)
            {
                var (word, err) = b.ReadSlice('+');
                if (err == null)
                {
                    word = word[0L..len(word) - 1L]; // trim +
                }
                var (pc, _) = strconv.ParseUint(string(word), 0L, 64L);
                if (pc != 0L)
                {
                    var f = runtime.FuncForPC(uintptr(pc));
                    if (f != null)
                    {
                        fmt.Fprintf(ref buf, "%#x %s\n", pc, f.Name());
                    }
                } 

                // Wait until here to check for err; the last
                // symbol will have an err because it doesn't end in +.
                if (err != null)
                {
                    if (err != io.EOF)
                    {
                        fmt.Fprintf(ref buf, "reading request: %v\n", err);
                    }
                    break;
                }
            }


            w.Write(buf.Bytes());
        }

        // Handler returns an HTTP handler that serves the named profile.
        public static http.Handler Handler(@string name)
        {
            return handler(name);
        }

        private partial struct handler // : @string
        {
        }

        private static void ServeHTTP(this handler name, http.ResponseWriter w, ref http.Request r)
        {
            w.Header().Set("X-Content-Type-Options", "nosniff");
            var p = pprof.Lookup(string(name));
            if (p == null)
            {
                serveError(w, http.StatusNotFound, "Unknown profile");
                return;
            }
            var (gc, _) = strconv.Atoi(r.FormValue("gc"));
            if (name == "heap" && gc > 0L)
            {
                runtime.GC();
            }
            var (debug, _) = strconv.Atoi(r.FormValue("debug"));
            if (debug != 0L)
            {
                w.Header().Set("Content-Type", "text/plain; charset=utf-8");
            }
            else
            {
                w.Header().Set("Content-Type", "application/octet-stream");
                w.Header().Set("Content-Disposition", fmt.Sprintf("attachment; filename=\"%s\"", name));
            }
            p.WriteTo(w, debug);
        }

        // Index responds with the pprof-formatted profile named by the request.
        // For example, "/debug/pprof/heap" serves the "heap" profile.
        // Index responds to a request for "/debug/pprof/" with an HTML page
        // listing the available profiles.
        public static void Index(http.ResponseWriter w, ref http.Request r)
        {
            if (strings.HasPrefix(r.URL.Path, "/debug/pprof/"))
            {
                var name = strings.TrimPrefix(r.URL.Path, "/debug/pprof/");
                if (name != "")
                {
                    handler(name).ServeHTTP(w, r);
                    return;
                }
            }
            var profiles = pprof.Profiles();
            {
                var err = indexTmpl.Execute(w, profiles);

                if (err != null)
                {
                    log.Print(err);
                }

            }
        }

        private static var indexTmpl = template.Must(template.New("index").Parse(@"<html>
<head>
<title>/debug/pprof/</title>
</head>
<body>
/debug/pprof/<br>
<br>
profiles:<br>
<table>
{{range .}}
<tr><td align=right>{{.Count}}<td><a href=""{{.Name}}?debug=1"">{{.Name}}</a>
{{end}}
</table>
<br>
<a href=""goroutine?debug=2"">full goroutine stack dump</a><br>
</body>
</html>
"));
    }
}}}
