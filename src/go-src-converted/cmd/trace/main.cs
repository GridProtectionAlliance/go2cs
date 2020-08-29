// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2020 August 29 10:05:00 UTC
// Original source: C:\Go\src\cmd\trace\main.go
using bufio = go.bufio_package;
using browser = go.cmd.@internal.browser_package;
using flag = go.flag_package;
using fmt = go.fmt_package;
using template = go.html.template_package;
using trace = go.@internal.trace_package;
using io = go.io_package;
using log = go.log_package;
using net = go.net_package;
using http = go.net.http_package;
using os = go.os_package;
using sync = go.sync_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class main_package
    {
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



        private static var httpFlag = flag.String("http", "localhost:0", "HTTP service address (e.g., ':6060')");        private static var pprofFlag = flag.String("pprof", "", "print a pprof-like profile instead");        private static var debugFlag = flag.Bool("d", false, "print debug information such as parsed events list");        private static @string programBinary = default;        private static @string traceFile = default;

        private static void Main()
        {
            flag.Usage = () =>
            {
                fmt.Fprintln(os.Stderr, usageMessage);
                os.Exit(2L);
            }
;
            flag.Parse(); 

            // Go 1.7 traces embed symbol info and does not require the binary.
            // But we optionally accept binary as first arg for Go 1.5 traces.
            switch (flag.NArg())
            {
                case 1L: 
                    traceFile = flag.Arg(0L);
                    break;
                case 2L: 
                    programBinary = flag.Arg(0L);
                    traceFile = flag.Arg(1L);
                    break;
                default: 
                    flag.Usage();
                    break;
            }

            Func<io.Writer, @string, error> pprofFunc = default;
            switch (pprofFlag.Value)
            {
                case "net": 
                    pprofFunc = pprofIO;
                    break;
                case "sync": 
                    pprofFunc = pprofBlock;
                    break;
                case "syscall": 
                    pprofFunc = pprofSyscall;
                    break;
                case "sched": 
                    pprofFunc = pprofSched;
                    break;
            }
            if (pprofFunc != null)
            {
                {
                    var err = pprofFunc(os.Stdout, "");

                    if (err != null)
                    {
                        dief("failed to generate pprof: %v\n", err);
                    }

                }
                os.Exit(0L);
            }
            if (pprofFlag != "".Value)
            {
                dief("unknown pprof type %s\n", pprofFlag.Value);
            }
            var (ln, err) = net.Listen("tcp", httpFlag.Value);
            if (err != null)
            {
                dief("failed to create server socket: %v\n", err);
            }
            log.Print("Parsing trace...");
            var (res, err) = parseTrace();
            if (err != null)
            {
                dief("%v\n", err);
            }
            if (debugFlag.Value)
            {
                trace.Print(res.Events);
                os.Exit(0L);
            }
            log.Print("Serializing trace...");
            traceParams @params = ref new traceParams(parsed:res,endTime:int64(1<<63-1),);
            var (data, err) = generateTrace(params);
            if (err != null)
            {
                dief("%v\n", err);
            }
            log.Print("Splitting trace...");
            ranges = splitTrace(data);

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
        private static (slice<ref trace.Event>, error) parseEvents()
        {
            var (res, err) = parseTrace();
            if (err != null)
            {
                return (null, err);
            }
            return (res.Events, err);
        }

        private static (trace.ParseResult, error) parseTrace() => func((defer, _, __) =>
        {
            loader.once.Do(() =>
            {
                var (tracef, err) = os.Open(traceFile);
                if (err != null)
                {
                    loader.err = fmt.Errorf("failed to open trace file: %v", err);
                    return;
                }
                defer(tracef.Close()); 

                // Parse and symbolize.
                var (res, err) = trace.Parse(bufio.NewReader(tracef), programBinary);
                if (err != null)
                {
                    loader.err = fmt.Errorf("failed to parse trace: %v", err);
                    return;
                }
                loader.res = res;
            });
            return (loader.res, loader.err);
        });

        // httpMain serves the starting page.
        private static void httpMain(http.ResponseWriter w, ref http.Request r)
        {
            {
                var err = templMain.Execute(w, ranges);

                if (err != null)
                {
                    http.Error(w, err.Error(), http.StatusInternalServerError);
                    return;
                }

            }
        }

        private static var templMain = template.Must(template.New("").Parse(@"
<html>
<body>
{{if $}}
	{{range $e := $}}
		<a href=""/trace?start={{$e.Start}}&end={{$e.End}}"">View trace ({{$e.Name}})</a><br>
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
</body>
</html>
"));

        private static void dief(@string msg, params object[] args)
        {
            args = args.Clone();

            fmt.Fprintf(os.Stderr, msg, args);
            os.Exit(1L);
        }
    }
}
