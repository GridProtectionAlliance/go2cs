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
// If you are not using DefaultServeMux, you will have to register handlers
// with the mux you are using.
//
// Then use the pprof tool to look at the heap profile:
//
//    go tool pprof http://localhost:6060/debug/pprof/heap
//
// Or to look at a 30-second CPU profile:
//
//    go tool pprof http://localhost:6060/debug/pprof/profile?seconds=30
//
// Or to look at the goroutine blocking profile, after calling
// runtime.SetBlockProfileRate in your program:
//
//    go tool pprof http://localhost:6060/debug/pprof/block
//
// Or to look at the holders of contended mutexes, after calling
// runtime.SetMutexProfileFraction in your program:
//
//    go tool pprof http://localhost:6060/debug/pprof/mutex
//
// The package also exports a handler that serves execution trace data
// for the "go tool trace" command. To collect a 5-second execution trace:
//
//    wget -O trace.out http://localhost:6060/debug/pprof/trace?seconds=5
//    go tool trace trace.out
//
// To view all available profiles, open http://localhost:6060/debug/pprof/
// in your browser.
//
// For a study of the facility in action, visit
//
//    https://blog.golang.org/2011/06/profiling-go-programs.html
//
// package pprof -- go2cs converted at 2020 October 08 03:41:38 UTC
// import "net/http/pprof" ==> using pprof = go.net.http.pprof_package
// Original source: C:\Go\src\net\http\pprof\pprof.go
using bufio = go.bufio_package;
using bytes = go.bytes_package;
using context = go.context_package;
using fmt = go.fmt_package;
using template = go.html.template_package;
using profile = go.@internal.profile_package;
using io = go.io_package;
using log = go.log_package;
using http = go.net.http_package;
using os = go.os_package;
using runtime = go.runtime_package;
using pprof = go.runtime.pprof_package;
using trace = go.runtime.trace_package;
using sort = go.sort_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using time = go.time_package;
using static go.builtin;
using System;

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
        public static void Cmdline(http.ResponseWriter w, ptr<http.Request> _addr_r)
        {
            ref http.Request r = ref _addr_r.val;

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

        private static bool durationExceedsWriteTimeout(ptr<http.Request> _addr_r, double seconds)
        {
            ref http.Request r = ref _addr_r.val;

            ptr<http.Server> (srv, ok) = r.Context().Value(http.ServerContextKey)._<ptr<http.Server>>();
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
        // Profiling lasts for duration specified in seconds GET parameter, or for 30 seconds if not specified.
        // The package initialization registers it as /debug/pprof/profile.
        public static void Profile(http.ResponseWriter w, ptr<http.Request> _addr_r)
        {
            ref http.Request r = ref _addr_r.val;

            w.Header().Set("X-Content-Type-Options", "nosniff");
            var (sec, err) = strconv.ParseInt(r.FormValue("seconds"), 10L, 64L);
            if (sec <= 0L || err != null)
            {
                sec = 30L;
            }

            if (durationExceedsWriteTimeout(_addr_r, float64(sec)))
            {
                serveError(w, http.StatusBadRequest, "profile duration exceeds server's WriteTimeout");
                return ;
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
                    return ;

                }

            }

            sleep(w, time.Duration(sec) * time.Second);
            pprof.StopCPUProfile();

        }

        // Trace responds with the execution trace in binary form.
        // Tracing lasts for duration specified in seconds GET parameter, or for 1 second if not specified.
        // The package initialization registers it as /debug/pprof/trace.
        public static void Trace(http.ResponseWriter w, ptr<http.Request> _addr_r)
        {
            ref http.Request r = ref _addr_r.val;

            w.Header().Set("X-Content-Type-Options", "nosniff");
            var (sec, err) = strconv.ParseFloat(r.FormValue("seconds"), 64L);
            if (sec <= 0L || err != null)
            {
                sec = 1L;
            }

            if (durationExceedsWriteTimeout(_addr_r, sec))
            {
                serveError(w, http.StatusBadRequest, "profile duration exceeds server's WriteTimeout");
                return ;
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
                    return ;

                }

            }

            sleep(w, time.Duration(sec * float64(time.Second)));
            trace.Stop();

        }

        // Symbol looks up the program counters listed in the request,
        // responding with a table mapping program counters to function names.
        // The package initialization registers it as /debug/pprof/symbol.
        public static void Symbol(http.ResponseWriter w, ptr<http.Request> _addr_r)
        {
            ref http.Request r = ref _addr_r.val;

            w.Header().Set("X-Content-Type-Options", "nosniff");
            w.Header().Set("Content-Type", "text/plain; charset=utf-8"); 

            // We have to read the whole POST body before
            // writing any output. Buffer the output here.
            ref bytes.Buffer buf = ref heap(out ptr<bytes.Buffer> _addr_buf); 

            // We don't know how many symbols we have, but we
            // do have symbol information. Pprof only cares whether
            // this number is 0 (no symbols available) or > 0.
            fmt.Fprintf(_addr_buf, "num_symbols: 1\n");

            ptr<bufio.Reader> b;
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
                        fmt.Fprintf(_addr_buf, "%#x %s\n", pc, f.Name());
                    }

                } 

                // Wait until here to check for err; the last
                // symbol will have an err because it doesn't end in +.
                if (err != null)
                {
                    if (err != io.EOF)
                    {
                        fmt.Fprintf(_addr_buf, "reading request: %v\n", err);
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

        private static void ServeHTTP(this handler name, http.ResponseWriter w, ptr<http.Request> _addr_r)
        {
            ref http.Request r = ref _addr_r.val;

            w.Header().Set("X-Content-Type-Options", "nosniff");
            var p = pprof.Lookup(string(name));
            if (p == null)
            {
                serveError(w, http.StatusNotFound, "Unknown profile");
                return ;
            }

            {
                var sec = r.FormValue("seconds");

                if (sec != "")
                {
                    name.serveDeltaProfile(w, r, p, sec);
                    return ;
                }

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

        private static void serveDeltaProfile(this handler name, http.ResponseWriter w, ptr<http.Request> _addr_r, ptr<pprof.Profile> _addr_p, @string secStr) => func((defer, _, __) =>
        {
            ref http.Request r = ref _addr_r.val;
            ref pprof.Profile p = ref _addr_p.val;

            var (sec, err) = strconv.ParseInt(secStr, 10L, 64L);
            if (err != null || sec <= 0L)
            {
                serveError(w, http.StatusBadRequest, "invalid value for \"seconds\" - must be a positive integer");
                return ;
            }

            if (!profileSupportsDelta[name])
            {
                serveError(w, http.StatusBadRequest, "\"seconds\" parameter is not supported for this profile type");
                return ;
            } 
            // 'name' should be a key in profileSupportsDelta.
            if (durationExceedsWriteTimeout(_addr_r, float64(sec)))
            {
                serveError(w, http.StatusBadRequest, "profile duration exceeds server's WriteTimeout");
                return ;
            }

            var (debug, _) = strconv.Atoi(r.FormValue("debug"));
            if (debug != 0L)
            {
                serveError(w, http.StatusBadRequest, "seconds and debug params are incompatible");
                return ;
            }

            var (p0, err) = collectProfile(_addr_p);
            if (err != null)
            {
                serveError(w, http.StatusInternalServerError, "failed to collect profile");
                return ;
            }

            var t = time.NewTimer(time.Duration(sec) * time.Second);
            defer(t.Stop());

            var err = r.Context().Err();
            if (err == context.DeadlineExceeded)
            {
                serveError(w, http.StatusRequestTimeout, err.Error());
            }
            else
            { // TODO: what's a good status code for cancelled requests? 400?
                serveError(w, http.StatusInternalServerError, err.Error());

            }

            return ;
            var (p1, err) = collectProfile(_addr_p);
            if (err != null)
            {
                serveError(w, http.StatusInternalServerError, "failed to collect profile");
                return ;
            }

            var ts = p1.TimeNanos;
            var dur = p1.TimeNanos - p0.TimeNanos;

            p0.Scale(-1L);

            p1, err = profile.Merge(new slice<ptr<profile.Profile>>(new ptr<profile.Profile>[] { p0, p1 }));
            if (err != null)
            {
                serveError(w, http.StatusInternalServerError, "failed to compute delta");
                return ;
            }

            p1.TimeNanos = ts; // set since we don't know what profile.Merge set for TimeNanos.
            p1.DurationNanos = dur;

            w.Header().Set("Content-Type", "application/octet-stream");
            w.Header().Set("Content-Disposition", fmt.Sprintf("attachment; filename=\"%s-delta\"", name));
            p1.Write(w);

        });

        private static (ptr<profile.Profile>, error) collectProfile(ptr<pprof.Profile> _addr_p)
        {
            ptr<profile.Profile> _p0 = default!;
            error _p0 = default!;
            ref pprof.Profile p = ref _addr_p.val;

            ref bytes.Buffer buf = ref heap(out ptr<bytes.Buffer> _addr_buf);
            {
                var err = p.WriteTo(_addr_buf, 0L);

                if (err != null)
                {
                    return (_addr_null!, error.As(err)!);
                }

            }

            var ts = time.Now().UnixNano();
            var (p0, err) = profile.Parse(_addr_buf);
            if (err != null)
            {
                return (_addr_null!, error.As(err)!);
            }

            p0.TimeNanos = ts;
            return (_addr_p0!, error.As(null!)!);

        }

        private static map profileSupportsDelta = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<handler, bool>{"allocs":true,"block":true,"goroutine":true,"heap":true,"mutex":true,"threadcreate":true,};

        private static map profileDescriptions = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, @string>{"allocs":"A sampling of all past memory allocations","block":"Stack traces that led to blocking on synchronization primitives","cmdline":"The command line invocation of the current program","goroutine":"Stack traces of all current goroutines","heap":"A sampling of memory allocations of live objects. You can specify the gc GET parameter to run GC before taking the heap sample.","mutex":"Stack traces of holders of contended mutexes","profile":"CPU profile. You can specify the duration in the seconds GET parameter. After you get the profile file, use the go tool pprof command to investigate the profile.","threadcreate":"Stack traces that led to the creation of new OS threads","trace":"A trace of execution of the current program. You can specify the duration in the seconds GET parameter. After you get the trace file, use the go tool trace command to investigate the trace.",};

        // Index responds with the pprof-formatted profile named by the request.
        // For example, "/debug/pprof/heap" serves the "heap" profile.
        // Index responds to a request for "/debug/pprof/" with an HTML page
        // listing the available profiles.
        public static void Index(http.ResponseWriter w, ptr<http.Request> _addr_r)
        {
            ref http.Request r = ref _addr_r.val;

            if (strings.HasPrefix(r.URL.Path, "/debug/pprof/"))
            {
                var name = strings.TrimPrefix(r.URL.Path, "/debug/pprof/");
                if (name != "")
                {
                    handler(name).ServeHTTP(w, r);
                    return ;
                }

            }

            w.Header().Set("X-Content-Type-Options", "nosniff");
            w.Header().Set("Content-Type", "text/html; charset=utf-8");

            private partial struct profile
            {
                public @string Name;
                public @string Href;
                public @string Desc;
                public long Count;
            }
            slice<profile> profiles = default;
            {
                var p__prev1 = p;

                foreach (var (_, __p) in pprof.Profiles())
                {
                    p = __p;
                    profiles = append(profiles, new profile(Name:p.Name(),Href:p.Name()+"?debug=1",Desc:profileDescriptions[p.Name()],Count:p.Count(),));
                } 

                // Adding other profiles exposed from within this package

                p = p__prev1;
            }

            {
                var p__prev1 = p;

                foreach (var (_, __p) in new slice<@string>(new @string[] { "cmdline", "profile", "trace" }))
                {
                    p = __p;
                    profiles = append(profiles, new profile(Name:p,Href:p,Desc:profileDescriptions[p],));
                }

                p = p__prev1;
            }

            sort.Slice(profiles, (i, j) =>
            {
                return profiles[i].Name < profiles[j].Name;
            });

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
<style>
.profile-name{
	display:inline-block;
	width:6rem;
}
</style>
</head>
<body>
/debug/pprof/<br>
<br>
Types of profiles available:
<table>
<thead><td>Count</td><td>Profile</td></thead>
{{range .}}
	<tr>
	<td>{{.Count}}</td><td><a href={{.Href}}>{{.Name}}</a></td>
	</tr>
{{end}}
</table>
<a href=""goroutine?debug=2"">full goroutine stack dump</a>
<br/>
<p>
Profile Descriptions:
<ul>
{{range .}}
<li><div class=profile-name>{{.Name}}:</div> {{.Desc}}</li>
{{end}}
</ul>
</p>
</body>
</html>
"));
    }
}}}
