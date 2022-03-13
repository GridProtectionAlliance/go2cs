// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2022 March 13 06:36:07 UTC
// Original source: C:\Program Files\Go\src\cmd\trace\main.go
namespace go;

using bufio = bufio_package;
using browser = cmd.@internal.browser_package;
using flag = flag_package;
using fmt = fmt_package;
using template = html.template_package;
using trace = @internal.trace_package;
using io = io_package;
using log = log_package;
using net = net_package;
using http = net.http_package;
using os = os_package;
using runtime = runtime_package;
using debug = runtime.debug_package;
using sync = sync_package;

using _pprof_ = net.http.pprof_package; // Required to use pprof



using System;public static partial class main_package {

private static readonly @string usageMessage = "" + @"Usage of 'go tool trace':
Given a trace file produced by 'go test':
	go test -trace=trace.out pkg

Open a web browser displaying trace:
	go tool trace [flags] [pkg.test] trace.out

Generate a pprof-like profile from the trace:
    go tool trace -pprof=TYPE [pkg.test] trace.out

[pkg.test] argument is required for traces produced by Go 1.6 and below.
Go 1.7 does not require the binary argument.

Supported profile types are:
    - net: network blocking profile
    - sync: synchronization blocking profile
    - syscall: syscall blocking profile
    - sched: scheduler latency profile

Flags:
	-http=addr: HTTP service address (e.g., ':6060')
	-pprof=type: print a pprof-like profile instead
	-d: print debug info such as parsed events

Note that while the various profiles available when launching
'go tool trace' work on every browser, the trace viewer itself
(the 'view trace' page) comes from the Chrome/Chromium project
and is only actively tested on that browser.
";



private static var httpFlag = flag.String("http", "localhost:0", "HTTP service address (e.g., ':6060')");private static var pprofFlag = flag.String("pprof", "", "print a pprof-like profile instead");private static var debugFlag = flag.Bool("d", false, "print debug information such as parsed events list");private static @string programBinary = default;private static @string traceFile = default;

private static void Main() {
    flag.Usage = () => {
        fmt.Fprintln(os.Stderr, usageMessage);
        os.Exit(2);
    };
    flag.Parse(); 

    // Go 1.7 traces embed symbol info and does not require the binary.
    // But we optionally accept binary as first arg for Go 1.5 traces.
    switch (flag.NArg()) {
        case 1: 
            traceFile = flag.Arg(0);
            break;
        case 2: 
            programBinary = flag.Arg(0);
            traceFile = flag.Arg(1);
            break;
        default: 
            flag.Usage();
            break;
    }

    Func<io.Writer, ptr<http.Request>, error> pprofFunc = default;
    switch (pprofFlag.val) {
        case "net": 
            pprofFunc = pprofByGoroutine(computePprofIO);
            break;
        case "sync": 
            pprofFunc = pprofByGoroutine(computePprofBlock);
            break;
        case "syscall": 
            pprofFunc = pprofByGoroutine(computePprofSyscall);
            break;
        case "sched": 
            pprofFunc = pprofByGoroutine(computePprofSched);
            break;
    }
    if (pprofFunc != null) {
        {
            var err = pprofFunc(os.Stdout, addr(new http.Request()));

            if (err != null) {
                dief("failed to generate pprof: %v\n", err);
            }

        }
        os.Exit(0);
    }
    if (pprofFlag != "".val) {
        dief("unknown pprof type %s\n", pprofFlag.val);
    }
    var (ln, err) = net.Listen("tcp", httpFlag.val);
    if (err != null) {
        dief("failed to create server socket: %v\n", err);
    }
    log.Print("Parsing trace...");
    var (res, err) = parseTrace();
    if (err != null) {
        dief("%v\n", err);
    }
    if (debugFlag.val) {
        trace.Print(res.Events);
        os.Exit(0);
    }
    reportMemoryUsage("after parsing trace");
    debug.FreeOSMemory();

    log.Print("Splitting trace...");
    ranges = splitTrace(res);
    reportMemoryUsage("after spliting trace");
    debug.FreeOSMemory();

    @string addr = "http://" + ln.Addr().String();
    log.Printf("Opening browser. Trace viewer is listening on %s", addr);
    browser.Open(addr); 

    // Start http server.
    http.HandleFunc("/", httpMain);
    err = http.Serve(ln, null);
    dief("failed to start http server: %v\n", err);
}

private static slice<Range> ranges = default;

private static var loader = default;

// parseEvents is a compatibility wrapper that returns only
// the Events part of trace.ParseResult returned by parseTrace.
private static (slice<ptr<trace.Event>>, error) parseEvents() {
    slice<ptr<trace.Event>> _p0 = default;
    error _p0 = default!;

    var (res, err) = parseTrace();
    if (err != null) {
        return (null, error.As(err)!);
    }
    return (res.Events, error.As(err)!);
}

private static (trace.ParseResult, error) parseTrace() => func((defer, _, _) => {
    trace.ParseResult _p0 = default;
    error _p0 = default!;

    loader.once.Do(() => {
        var (tracef, err) = os.Open(traceFile);
        if (err != null) {
            loader.err = fmt.Errorf("failed to open trace file: %v", err);
            return ;
        }
        defer(tracef.Close()); 

        // Parse and symbolize.
        var (res, err) = trace.Parse(bufio.NewReader(tracef), programBinary);
        if (err != null) {
            loader.err = fmt.Errorf("failed to parse trace: %v", err);
            return ;
        }
        loader.res = res;
    });
    return (loader.res, error.As(loader.err)!);
});

// httpMain serves the starting page.
private static void httpMain(http.ResponseWriter w, ptr<http.Request> _addr_r) {
    ref http.Request r = ref _addr_r.val;

    {
        var err = templMain.Execute(w, ranges);

        if (err != null) {
            http.Error(w, err.Error(), http.StatusInternalServerError);
            return ;
        }
    }
}

private static var templMain = template.Must(template.New("").Parse(@"
<html>
<body>
{{if $}}
	{{range $e := $}}
		<a href=""{{$e.URL}}"">View trace ({{$e.Name}})</a><br>
	{{end}}
	<br>
{{else}}
	<a href=""/trace"">View trace</a><br>
{{end}}
<a href=""/goroutines"">Goroutine analysis</a><br>
<a href=""/io"">Network blocking profile</a> (<a href=""/io?raw=1"" download=""io.profile"">⬇</a>)<br>
<a href=""/block"">Synchronization blocking profile</a> (<a href=""/block?raw=1"" download=""block.profile"">⬇</a>)<br>
<a href=""/syscall"">Syscall blocking profile</a> (<a href=""/syscall?raw=1"" download=""syscall.profile"">⬇</a>)<br>
<a href=""/sched"">Scheduler latency profile</a> (<a href=""/sche?raw=1"" download=""sched.profile"">⬇</a>)<br>
<a href=""/usertasks"">User-defined tasks</a><br>
<a href=""/userregions"">User-defined regions</a><br>
<a href=""/mmu"">Minimum mutator utilization</a><br>
</body>
</html>
"));

private static void dief(@string msg, params object[] args) {
    args = args.Clone();

    fmt.Fprintf(os.Stderr, msg, args);
    os.Exit(1);
}

private static bool debugMemoryUsage = default;

private static void init() {
    var v = os.Getenv("DEBUG_MEMORY_USAGE");
    debugMemoryUsage = v != "";
}

private static void reportMemoryUsage(@string msg) {
    if (!debugMemoryUsage) {
        return ;
    }
    ref runtime.MemStats s = ref heap(out ptr<runtime.MemStats> _addr_s);
    runtime.ReadMemStats(_addr_s);
    var w = os.Stderr;
    fmt.Fprintf(w, "%s\n", msg);
    fmt.Fprintf(w, " Alloc:\t%d Bytes\n", s.Alloc);
    fmt.Fprintf(w, " Sys:\t%d Bytes\n", s.Sys);
    fmt.Fprintf(w, " HeapReleased:\t%d Bytes\n", s.HeapReleased);
    fmt.Fprintf(w, " HeapSys:\t%d Bytes\n", s.HeapSys);
    fmt.Fprintf(w, " HeapInUse:\t%d Bytes\n", s.HeapInuse);
    fmt.Fprintf(w, " HeapAlloc:\t%d Bytes\n", s.HeapAlloc);
    ref @string dummy = ref heap(out ptr<@string> _addr_dummy);
    fmt.Printf("Enter to continue...");
    fmt.Scanf("%s", _addr_dummy);
}

} // end main_package
