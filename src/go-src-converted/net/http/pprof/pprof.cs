// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package pprof serves via its HTTP server runtime profiling data
// in the format expected by the pprof visualization tool.
//
// The package is typically only imported for the side effect of
// registering its HTTP handlers.
// The handled paths all begin with /debug/pprof/.
// As of Go 1.22, all the paths must be requested with GET.
//
// To use pprof, link this package into your program:
//
//	import _ "net/http/pprof"
//
// If your application is not already running an http server, you
// need to start one. Add "net/http" and "log" to your imports and
// the following code to your main function:
//
//	go func() {
//		log.Println(http.ListenAndServe("localhost:6060", nil))
//	}()
//
// By default, all the profiles listed in [runtime/pprof.Profile] are
// available (via [Handler]), in addition to the [Cmdline], [Profile], [Symbol],
// and [Trace] profiles defined in this package.
// If you are not using DefaultServeMux, you will have to register handlers
// with the mux you are using.
//
// # Parameters
//
// Parameters can be passed via GET query params:
//
//   - debug=N (all profiles): response format: N = 0: binary (default), N > 0: plaintext
//   - gc=N (heap profile): N > 0: run a garbage collection cycle before profiling
//   - seconds=N (allocs, block, goroutine, heap, mutex, threadcreate profiles): return a delta profile
//   - seconds=N (cpu (profile), trace profiles): profile for the given duration
//
// # Usage examples
//
// Use the pprof tool to look at the heap profile:
//
//	go tool pprof http://localhost:6060/debug/pprof/heap
//
// Or to look at a 30-second CPU profile:
//
//	go tool pprof http://localhost:6060/debug/pprof/profile?seconds=30
//
// Or to look at the goroutine blocking profile, after calling
// [runtime.SetBlockProfileRate] in your program:
//
//	go tool pprof http://localhost:6060/debug/pprof/block
//
// Or to look at the holders of contended mutexes, after calling
// [runtime.SetMutexProfileFraction] in your program:
//
//	go tool pprof http://localhost:6060/debug/pprof/mutex
//
// The package also exports a handler that serves execution trace data
// for the "go tool trace" command. To collect a 5-second execution trace:
//
//	curl -o trace.out http://localhost:6060/debug/pprof/trace?seconds=5
//	go tool trace trace.out
//
// To view all available profiles, open http://localhost:6060/debug/pprof/
// in your browser.
//
// For a study of the facility in action, visit
// https://blog.golang.org/2011/06/profiling-go-programs.html.
namespace go.net.http;

using bufio = bufio_package;
using bytes = bytes_package;
using context = context_package;
using fmt = fmt_package;
using html = html_package;
using godebug = @internal.godebug_package;
using profile = @internal.profile_package;
using io = io_package;
using log = log_package;
using http = net.http_package;
using url = net.url_package;
using os = os_package;
using runtime = runtime_package;
using pprof = runtime.pprof_package;
using trace = runtime.trace_package;
using sort = sort_package;
using strconv = strconv_package;
using strings = strings_package;
using time = time_package;
using @internal;
using net;
using runtime;

partial class pprof_package {

[GoInit] internal static void init() {
    @string prefix = ""u8;
    if (godebug.New("httpmuxgo121"u8).Value() != "1"u8) {
        prefix = "GET "u8;
    }
    http.HandleFunc(prefix + "/debug/pprof/"u8, Index);
    http.HandleFunc(prefix + "/debug/pprof/cmdline"u8, Cmdline);
    http.HandleFunc(prefix + "/debug/pprof/profile"u8, Profile);
    http.HandleFunc(prefix + "/debug/pprof/symbol"u8, Symbol);
    http.HandleFunc(prefix + "/debug/pprof/trace"u8, Trace);
}

// Cmdline responds with the running program's
// command line, with arguments separated by NUL bytes.
// The package initialization registers it as /debug/pprof/cmdline.
public static void Cmdline(http.ResponseWriter w, ж<http.Request> Ꮡr) {
    ref var r = ref Ꮡr.val;

    w.Header().Set("X-Content-Type-Options"u8, "nosniff"u8);
    w.Header().Set("Content-Type"u8, "text/plain; charset=utf-8"u8);
    fmt.Fprint(w, strings.Join(os.Args, "\x00"u8));
}

internal static void sleep(ж<http.Request> Ꮡr, time.Duration d) {
    ref var r = ref Ꮡr.val;

    switch (select(ᐸꟷ(time.After(d), ꓸꓸꓸ), ᐸꟷ(r.Context().Done(), ꓸꓸꓸ))) {
    case 0 when time.After(d).ꟷᐳ(out _): {
        break;
    }
    case 1 when r.Context().Done().ꟷᐳ(out _): {
        break;
    }}
}

internal static void configureWriteDeadline(http.ResponseWriter w, ж<http.Request> Ꮡr, float64 seconds) {
    ref var r = ref Ꮡr.val;

    var (srv, ok) = r.Context().Value(http.ServerContextKey)._<ж<http.Server>>(ᐧ);
    if (ok && (~srv).WriteTimeout > 0) {
        var timeout = (~srv).WriteTimeout + ((time.Duration)(seconds * ((float64)time.ΔSecond)));
        var rc = http.NewResponseController(w);
        rc.SetWriteDeadline(time.Now().Add(timeout));
    }
}

internal static void serveError(http.ResponseWriter w, nint status, @string txt) {
    w.Header().Set("Content-Type"u8, "text/plain; charset=utf-8"u8);
    w.Header().Set("X-Go-Pprof"u8, "1"u8);
    w.Header().Del("Content-Disposition"u8);
    w.WriteHeader(status);
    fmt.Fprintln(w, txt);
}

// Profile responds with the pprof-formatted cpu profile.
// Profiling lasts for duration specified in seconds GET parameter, or for 30 seconds if not specified.
// The package initialization registers it as /debug/pprof/profile.
public static void Profile(http.ResponseWriter w, ж<http.Request> Ꮡr) {
    ref var r = ref Ꮡr.val;

    w.Header().Set("X-Content-Type-Options"u8, "nosniff"u8);
    var (sec, err) = strconv.ParseInt(r.FormValue("seconds"u8), 10, 64);
    if (sec <= 0 || err != default!) {
        sec = 30;
    }
    configureWriteDeadline(w, Ꮡr, ((float64)sec));
    // Set Content Type assuming StartCPUProfile will work,
    // because if it does it starts writing.
    w.Header().Set("Content-Type"u8, "application/octet-stream"u8);
    w.Header().Set("Content-Disposition"u8, @"attachment; filename=""profile"""u8);
    {
        var errΔ1 = pprof.StartCPUProfile(w); if (errΔ1 != default!) {
            // StartCPUProfile failed, so no writes yet.
            serveError(w, http.StatusInternalServerError,
                fmt.Sprintf("Could not enable CPU profiling: %s"u8, errΔ1));
            return;
        }
    }
    sleep(Ꮡr, ((time.Duration)sec) * time.ΔSecond);
    pprof.StopCPUProfile();
}

// Trace responds with the execution trace in binary form.
// Tracing lasts for duration specified in seconds GET parameter, or for 1 second if not specified.
// The package initialization registers it as /debug/pprof/trace.
public static void Trace(http.ResponseWriter w, ж<http.Request> Ꮡr) {
    ref var r = ref Ꮡr.val;

    w.Header().Set("X-Content-Type-Options"u8, "nosniff"u8);
    var (sec, err) = strconv.ParseFloat(r.FormValue("seconds"u8), 64);
    if (sec <= 0 || err != default!) {
        sec = 1;
    }
    configureWriteDeadline(w, Ꮡr, sec);
    // Set Content Type assuming trace.Start will work,
    // because if it does it starts writing.
    w.Header().Set("Content-Type"u8, "application/octet-stream"u8);
    w.Header().Set("Content-Disposition"u8, @"attachment; filename=""trace"""u8);
    {
        var errΔ1 = trace.Start(w); if (errΔ1 != default!) {
            // trace.Start failed, so no writes yet.
            serveError(w, http.StatusInternalServerError,
                fmt.Sprintf("Could not enable tracing: %s"u8, errΔ1));
            return;
        }
    }
    sleep(Ꮡr, ((time.Duration)(sec * ((float64)time.ΔSecond))));
    trace.Stop();
}

// Symbol looks up the program counters listed in the request,
// responding with a table mapping program counters to function names.
// The package initialization registers it as /debug/pprof/symbol.
public static void Symbol(http.ResponseWriter w, ж<http.Request> Ꮡr) {
    ref var r = ref Ꮡr.val;

    w.Header().Set("X-Content-Type-Options"u8, "nosniff"u8);
    w.Header().Set("Content-Type"u8, "text/plain; charset=utf-8"u8);
    // We have to read the whole POST body before
    // writing any output. Buffer the output here.
    ref var buf = ref heap(new bytes_package.Buffer(), out var Ꮡbuf);
    // We don't know how many symbols we have, but we
    // do have symbol information. Pprof only cares whether
    // this number is 0 (no symbols available) or > 0.
    fmt.Fprintf(~Ꮡbuf, "num_symbols: 1\n"u8);
    ж<bufio.Reader> b = default!;
    if (r.Method == "POST"u8){
        b = bufio.NewReader(r.Body);
    } else {
        b = bufio.NewReader(~strings.NewReader(r.URL.RawQuery));
    }
    while (ᐧ) {
        (word, err) = b.ReadSlice((rune)'+');
        if (err == default!) {
            word = word[0..(int)(len(word) - 1)];
        }
        // trim +
        var (pc, _) = strconv.ParseUint(((@string)word), 0, 64);
        if (pc != 0) {
            var f = runtime.FuncForPC(((uintptr)pc));
            if (f != nil) {
                fmt.Fprintf(~Ꮡbuf, "%#x %s\n"u8, pc, f.Name());
            }
        }
        // Wait until here to check for err; the last
        // symbol will have an err because it doesn't end in +.
        if (err != default!) {
            if (!AreEqual(err, io.EOF)) {
                fmt.Fprintf(~Ꮡbuf, "reading request: %v\n"u8, err);
            }
            break;
        }
    }
    w.Write(buf.Bytes());
}

// Handler returns an HTTP handler that serves the named profile.
// Available profiles can be found in [runtime/pprof.Profile].
public static httpꓸHandler Handler(@string name) {
    return ((handler)name);
}

[GoType("@string")] partial struct handler;

internal static void ServeHTTP(this handler name, http.ResponseWriter w, ж<http.Request> Ꮡr) {
    ref var r = ref Ꮡr.val;

    w.Header().Set("X-Content-Type-Options"u8, "nosniff"u8);
    var p = pprof.Lookup(((@string)name));
    if (p == nil) {
        serveError(w, http.StatusNotFound, "Unknown profile"u8);
        return;
    }
    {
        @string sec = r.FormValue("seconds"u8); if (sec != ""u8) {
            name.serveDeltaProfile(w, Ꮡr, p, sec);
            return;
        }
    }
    var (gc, _) = strconv.Atoi(r.FormValue("gc"u8));
    if (name == "heap"u8 && gc > 0) {
        runtime.GC();
    }
    var (debug, _) = strconv.Atoi(r.FormValue("debug"u8));
    if (debug != 0){
        w.Header().Set("Content-Type"u8, "text/plain; charset=utf-8"u8);
    } else {
        w.Header().Set("Content-Type"u8, "application/octet-stream"u8);
        w.Header().Set("Content-Disposition"u8, fmt.Sprintf(@"attachment; filename=""%s"""u8, name));
    }
    p.WriteTo(w, debug);
}

internal static void serveDeltaProfile(this handler name, http.ResponseWriter w, ж<http.Request> Ꮡr, ж<pprof.Profile> Ꮡp, @string secStr) => func((defer, _) => {
    ref var r = ref Ꮡr.val;
    ref var p = ref Ꮡp.val;

    var (sec, err) = strconv.ParseInt(secStr, 10, 64);
    if (err != default! || sec <= 0) {
        serveError(w, http.StatusBadRequest, @"invalid value for ""seconds"" - must be a positive integer"u8);
        return;
    }
    // 'name' should be a key in profileSupportsDelta.
    if (!profileSupportsDelta[name]) {
        serveError(w, http.StatusBadRequest, @"""seconds"" parameter is not supported for this profile type"u8);
        return;
    }
    configureWriteDeadline(w, Ꮡr, ((float64)sec));
    var (debug, _) = strconv.Atoi(r.FormValue("debug"u8));
    if (debug != 0) {
        serveError(w, http.StatusBadRequest, "seconds and debug params are incompatible"u8);
        return;
    }
    (p0, err) = collectProfile(Ꮡp);
    if (err != default!) {
        serveError(w, http.StatusInternalServerError, "failed to collect profile"u8);
        return;
    }
    var t = time.NewTimer(((time.Duration)sec) * time.ΔSecond);
    var tʗ1 = t;
    defer(tʗ1.Stop);
    switch (select(ᐸꟷ(r.Context().Done(), ꓸꓸꓸ), ᐸꟷ((~t).C, ꓸꓸꓸ))) {
    case 0 when r.Context().Done().ꟷᐳ(out _): {
        var errΔ1 = r.Context().Err();
        if (AreEqual(errΔ1, context.DeadlineExceeded)){
            serveError(w, http.StatusRequestTimeout, errΔ1.Error());
        } else {
            // TODO: what's a good status code for canceled requests? 400?
            serveError(w, http.StatusInternalServerError, errΔ1.Error());
        }
        return;
    }
    case 1 when (~t).C.ꟷᐳ(out _): {
    }}
    (p1, err) = collectProfile(Ꮡp);
    if (err != default!) {
        serveError(w, http.StatusInternalServerError, "failed to collect profile"u8);
        return;
    }
    var ts = p1.val.TimeNanos;
    var dur = (~p1).TimeNanos - (~p0).TimeNanos;
    p0.Scale(-1);
    (p1, err) = profile.Merge(new profile.Profile[]{p0, p1}.slice());
    if (err != default!) {
        serveError(w, http.StatusInternalServerError, "failed to compute delta"u8);
        return;
    }
    p1.val.TimeNanos = ts;
    // set since we don't know what profile.Merge set for TimeNanos.
    p1.val.DurationNanos = dur;
    w.Header().Set("Content-Type"u8, "application/octet-stream"u8);
    w.Header().Set("Content-Disposition"u8, fmt.Sprintf(@"attachment; filename=""%s-delta"""u8, name));
    p1.Write(w);
});

internal static (ж<profile.Profile>, error) collectProfile(ж<pprof.Profile> Ꮡp) {
    ref var p = ref Ꮡp.val;

    ref var buf = ref heap(new bytes_package.Buffer(), out var Ꮡbuf);
    {
        var errΔ1 = p.WriteTo(~Ꮡbuf, 0); if (errΔ1 != default!) {
            return (default!, errΔ1);
        }
    }
    var ts = time.Now().UnixNano();
    (p0, err) = profile.Parse(~Ꮡbuf);
    if (err != default!) {
        return (default!, err);
    }
    p0.val.TimeNanos = ts;
    return (p0, default!);
}

internal static map<handler, bool> profileSupportsDelta = new map<handler, bool>{
    ["allocs"u8] = true,
    ["block"u8] = true,
    ["goroutine"u8] = true,
    ["heap"u8] = true,
    ["mutex"u8] = true,
    ["threadcreate"u8] = true
};

internal static map<@string, @string> profileDescriptions = new map<@string, @string>{
    ["allocs"u8] = "A sampling of all past memory allocations"u8,
    ["block"u8] = "Stack traces that led to blocking on synchronization primitives"u8,
    ["cmdline"u8] = "The command line invocation of the current program"u8,
    ["goroutine"u8] = "Stack traces of all current goroutines. Use debug=2 as a query parameter to export in the same format as an unrecovered panic."u8,
    ["heap"u8] = "A sampling of memory allocations of live objects. You can specify the gc GET parameter to run GC before taking the heap sample."u8,
    ["mutex"u8] = "Stack traces of holders of contended mutexes"u8,
    ["profile"u8] = "CPU profile. You can specify the duration in the seconds GET parameter. After you get the profile file, use the go tool pprof command to investigate the profile."u8,
    ["threadcreate"u8] = "Stack traces that led to the creation of new OS threads"u8,
    ["trace"u8] = "A trace of execution of the current program. You can specify the duration in the seconds GET parameter. After you get the trace file, use the go tool trace command to investigate the trace."u8
};

[GoType] partial struct profileEntry {
    public @string Name;
    public @string Href;
    public @string Desc;
    public nint Count;
}

// Index responds with the pprof-formatted profile named by the request.
// For example, "/debug/pprof/heap" serves the "heap" profile.
// Index responds to a request for "/debug/pprof/" with an HTML page
// listing the available profiles.
public static void Index(http.ResponseWriter w, ж<http.Request> Ꮡr) {
    ref var r = ref Ꮡr.val;

    {
        var (name, found) = strings.CutPrefix(r.URL.Path, "/debug/pprof/"u8); if (found) {
            if (name != ""u8) {
                ((handler)name).ServeHTTP(w, Ꮡr);
                return;
            }
        }
    }
    w.Header().Set("X-Content-Type-Options"u8, "nosniff"u8);
    w.Header().Set("Content-Type"u8, "text/html; charset=utf-8"u8);
    slice<profileEntry> profiles = default!;
    foreach (var (_, p) in pprof.Profiles()) {
        profiles = append(profiles, new profileEntry(
            Name: p.Name(),
            Href: p.Name(),
            Desc: profileDescriptions[p.Name()],
            Count: p.Count()
        ));
    }
    // Adding other profiles exposed from within this package
    foreach (var (_, p) in new @string[]{"cmdline", "profile", "trace"}.slice()) {
        profiles = append(profiles, new profileEntry(
            Name: p,
            Href: p,
            Desc: profileDescriptions[p]
        ));
    }
    sort.Slice(profiles, 
    var profilesʗ1 = profiles;
    (nint i, nint j) => profilesʗ1[i].Name < profilesʗ1[j].Name);
    {
        var err = indexTmplExecute(w, profiles); if (err != default!) {
            log.Print(err);
        }
    }
}

internal static error indexTmplExecute(io.Writer w, slice<profileEntry> profiles) {
    ref var b = ref heap(new bytes_package.Buffer(), out var Ꮡb);
    b.WriteString("""
<html>
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
/debug/pprof/
<br>
<p>Set debug=1 as a query parameter to export in legacy text format</p>
<br>
Types of profiles available:
<table>
<thead><td>Count</td><td>Profile</td></thead>

"""u8);
    foreach (var (_, profile) in profiles) {
        var link = Ꮡ(new url.URL(Path: profile.Href, RawQuery: "debug=1"u8));
        fmt.Fprintf(~Ꮡb, "<tr><td>%d</td><td><a href='%s'>%s</a></td></tr>\n"u8, profile.Count, link, html.EscapeString(profile.Name));
    }
    b.WriteString("""
</table>
<a href="goroutine?debug=2">full goroutine stack dump</a>
<br>
<p>
Profile Descriptions:
<ul>

"""u8);
    foreach (var (_, profile) in profiles) {
        fmt.Fprintf(~Ꮡb, "<li><div class=profile-name>%s: </div> %s</li>\n"u8, html.EscapeString(profile.Name), html.EscapeString(profile.Desc));
    }
    b.WriteString("""
</ul>
</p>
</body>
</html>
"""u8);
    var (_, err) = w.Write(b.Bytes());
    return err;
}

} // end pprof_package
