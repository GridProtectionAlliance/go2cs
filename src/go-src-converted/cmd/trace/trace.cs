// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2020 August 29 10:05:07 UTC
// Original source: C:\Go\src\cmd\trace\trace.go
using json = go.encoding.json_package;
using fmt = go.fmt_package;
using trace = go.@internal.trace_package;
using log = go.log_package;
using http = go.net.http_package;
using filepath = go.path.filepath_package;
using runtime = go.runtime_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using time = go.time_package;
using static go.builtin;
using System.ComponentModel;
using System;

namespace go
{
    public static partial class main_package
    {
        private static void init()
        {
            http.HandleFunc("/trace", httpTrace);
            http.HandleFunc("/jsontrace", httpJsonTrace);
            http.HandleFunc("/trace_viewer_html", httpTraceViewerHTML);
        }

        // httpTrace serves either whole trace (goid==0) or trace for goid goroutine.
        private static void httpTrace(http.ResponseWriter w, ref http.Request r)
        {
            var (_, err) = parseTrace();
            if (err != null)
            {
                http.Error(w, err.Error(), http.StatusInternalServerError);
                return;
            }
            {
                var err = r.ParseForm();

                if (err != null)
                {
                    http.Error(w, err.Error(), http.StatusInternalServerError);
                    return;
                }

            }
            var html = strings.Replace(templTrace, "{{PARAMS}}", r.Form.Encode(), -1L);
            w.Write((slice<byte>)html);

        }

        // See https://github.com/catapult-project/catapult/blob/master/tracing/docs/embedding-trace-viewer.md
        // This is almost verbatim copy of:
        // https://github.com/catapult-project/catapult/blob/master/tracing/bin/index.html
        // on revision 5f9e4c3eaa555bdef18218a89f38c768303b7b6e.
        private static @string templTrace = "\n<html>\n<head>\n<link href=\"/trace_viewer_html\" rel=\"import\">\n<style type=\"text/cs" +
    "s\">\n  html, body {\n    box-sizing: border-box;\n    overflow: hidden;\n    margin:" +
    " 0px;\n    padding: 0;\n    width: 100%;\n    height: 100%;\n  }\n  #trace-viewer {\n " +
    "   width: 100%;\n    height: 100%;\n  }\n  #trace-viewer:focus {\n    outline: none;" +
    "\n  }\n</style>\n<script>\n\'use strict\';\n(function() {\n  var viewer;\n  var url;\n  va" +
    "r model;\n\n  function load() {\n    var req = new XMLHttpRequest();\n    var is_bin" +
    "ary = /[.]gz$/.test(url) || /[.]zip$/.test(url);\n    req.overrideMimeType(\'text/" +
    "plain; charset=x-user-defined\');\n    req.open(\'GET\', url, true);\n    if (is_bina" +
    "ry)\n      req.responseType = \'arraybuffer\';\n\n    req.onreadystatechange = functi" +
    "on(event) {\n      if (req.readyState !== 4)\n        return;\n\n      window.setTim" +
    "eout(function() {\n        if (req.status === 200)\n          onResult(is_binary ?" +
    " req.response : req.responseText);\n        else\n          onResultFail(req.statu" +
    "s);\n      }, 0);\n    };\n    req.send(null);\n  }\n\n  function onResultFail(err) {\n" +
    "    var overlay = new tr.ui.b.Overlay();\n    overlay.textContent = err + \': \' + " +
    "url + \' could not be loaded\';\n    overlay.title = \'Failed to fetch data\';\n    ov" +
    "erlay.visible = true;\n  }\n\n  function onResult(result) {\n    model = new tr.Mode" +
    "l();\n    var opts = new tr.importer.ImportOptions();\n    opts.shiftWorldToZero =" +
    " false;\n    var i = new tr.importer.Import(model, opts);\n    var p = i.importTra" +
    "cesWithProgressDialog([result]);\n    p.then(onModelLoaded, onImportFail);\n  }\n\n " +
    " function onModelLoaded() {\n    viewer.model = model;\n    viewer.viewTitle = \"tr" +
    "ace\";\n  }\n\n  function onImportFail(err) {\n    var overlay = new tr.ui.b.Overlay(" +
    ");\n    overlay.textContent = tr.b.normalizeException(err).message;\n    overlay.t" +
    "itle = \'Import error\';\n    overlay.visible = true;\n  }\n\n  document.addEventListe" +
    "ner(\'DOMContentLoaded\', function() {\n    var container = document.createElement(" +
    "\'track-view-container\');\n    container.id = \'track_view_container\';\n\n    viewer " +
    "= document.createElement(\'tr-ui-timeline-view\');\n    viewer.track_view_container" +
    " = container;\n    viewer.appendChild(container);\n\n    viewer.id = \'trace-viewer\'" +
    ";\n    viewer.globalMode = true;\n    document.body.appendChild(viewer);\n\n    url " +
    "= \'/jsontrace?{{PARAMS}}\';\n    load();\n  });\n}());\n</script>\n</head>\n<body>\n</bo" +
    "dy>\n</html>\n";

        // httpTraceViewerHTML serves static part of trace-viewer.
        // This URL is queried from templTrace HTML.
        private static void httpTraceViewerHTML(http.ResponseWriter w, ref http.Request r)
        {
            http.ServeFile(w, r, filepath.Join(runtime.GOROOT(), "misc", "trace", "trace_viewer_full.html"));
        }

        // httpJsonTrace serves json trace, requested from within templTrace HTML.
        private static void httpJsonTrace(http.ResponseWriter w, ref http.Request r)
        { 
            // This is an AJAX handler, so instead of http.Error we use log.Printf to log errors.
            var (res, err) = parseTrace();
            if (err != null)
            {
                log.Printf("failed to parse trace: %v", err);
                return;
            }
            traceParams @params = ref new traceParams(parsed:res,endTime:int64(1<<63-1),);

            {
                var goids = r.FormValue("goid");

                if (goids != "")
                { 
                    // If goid argument is present, we are rendering a trace for this particular goroutine.
                    var (goid, err) = strconv.ParseUint(goids, 10L, 64L);
                    if (err != null)
                    {
                        log.Printf("failed to parse goid parameter '%v': %v", goids, err);
                        return;
                    }
                    analyzeGoroutines(res.Events);
                    var g = gs[goid];
                    @params.gtrace = true;
                    @params.startTime = g.StartTime;
                    @params.endTime = g.EndTime;
                    @params.maing = goid;
                    @params.gs = trace.RelatedGoroutines(res.Events, goid);
                }

            }

            var (data, err) = generateTrace(params);
            if (err != null)
            {
                log.Printf("failed to generate trace: %v", err);
                return;
            }
            {
                var startStr = r.FormValue("start");
                var endStr = r.FormValue("end");

                if (startStr != "" && endStr != "")
                { 
                    // If start/end arguments are present, we are rendering a range of the trace.
                    var (start, err) = strconv.ParseUint(startStr, 10L, 64L);
                    if (err != null)
                    {
                        log.Printf("failed to parse start parameter '%v': %v", startStr, err);
                        return;
                    }
                    var (end, err) = strconv.ParseUint(endStr, 10L, 64L);
                    if (err != null)
                    {
                        log.Printf("failed to parse end parameter '%v': %v", endStr, err);
                        return;
                    }
                    if (start >= uint64(len(data.Events)) || end <= start || end > uint64(len(data.Events)))
                    {
                        log.Printf("bogus start/end parameters: %v/%v, trace size %v", start, end, len(data.Events));
                        return;
                    }
                    data.Events = append(data.Events[start..end], data.Events[data.footer..]);
                }

            }
            err = json.NewEncoder(w).Encode(data);
            if (err != null)
            {
                log.Printf("failed to serialize trace: %v", err);
                return;
            }
        }

        public partial struct Range
        {
            public @string Name;
            public long Start;
            public long End;
        }

        // splitTrace splits the trace into a number of ranges,
        // each resulting in approx 100MB of json output (trace viewer can hardly handle more).
        private static slice<Range> splitTrace(ViewerData data)
        {
            const long rangeSize = 100L << (int)(20L);

            slice<Range> ranges = default;
            ptr<object> cw = @new<countingWriter>();
            var enc = json.NewEncoder(cw); 
            // First calculate size of the mandatory part of the trace.
            // This includes stack traces and thread names.
            var data1 = data;
            data1.Events = data.Events[data.footer..];
            enc.Encode(data1);
            var auxSize = cw.size;
            cw.size = 0L; 
            // Then calculate size of each individual event and group them into ranges.
            for (long i = 0L;
            long start = 0L; i < data.footer; i++)
            {
                enc.Encode(data.Events[i]);
                if (cw.size + auxSize > rangeSize || i == data.footer - 1L)
                {
                    ranges = append(ranges, new Range(Name:fmt.Sprintf("%v-%v",time.Duration(data.Events[start].Time*1000),time.Duration(data.Events[i].Time*1000)),Start:start,End:i+1,));
                    start = i + 1L;
                    cw.size = 0L;
                }
            }

            if (len(ranges) == 1L)
            {
                ranges = null;
            }
            return ranges;
        }

        private partial struct countingWriter
        {
            public long size;
        }

        private static (long, error) Write(this ref countingWriter cw, slice<byte> data)
        {
            cw.size += len(data);
            return (len(data), null);
        }

        private partial struct traceParams
        {
            public trace.ParseResult parsed;
            public bool gtrace;
            public long startTime;
            public long endTime;
            public ulong maing;
            public map<ulong, bool> gs;
        }

        private partial struct traceContext
        {
            public ref traceParams traceParams => ref traceParams_ptr;
            public ViewerData data;
            public frameNode frameTree;
            public long frameSeq;
            public ulong arrowSeq;
            public ulong gcount;
            public heapStats heapStats;
            public heapStats prevHeapStats;
            public threadStats threadStats;
            public threadStats prevThreadStats;
            public array<long> gstates;
            public array<long> prevGstates;
        }

        private partial struct heapStats
        {
            public ulong heapAlloc;
            public ulong nextGC;
        }

        private partial struct threadStats
        {
            public ulong insyscallRuntime; // system goroutine in syscall
            public ulong insyscall; // user goroutine in syscall
            public ulong prunning; // thread running P
        }

        private partial struct frameNode
        {
            public long id;
            public map<ulong, frameNode> children;
        }

        private partial struct gState // : long
        {
        }

        private static readonly gState gDead = iota;
        private static readonly var gRunnable = 0;
        private static readonly var gRunning = 1;
        private static readonly var gWaiting = 2;
        private static readonly var gWaitingGC = 3;

        private static readonly var gStateCount = 4;

        private partial struct gInfo
        {
            public gState state; // current state
            public @string name; // name chosen for this goroutine at first EvGoStart
            public bool isSystemG;
            public ptr<trace.Event> start; // most recent EvGoStart
            public ptr<trace.Event> markAssist; // if non-nil, the mark assist currently running.
        }

        public partial struct ViewerData
        {
            [Description("json:\"traceEvents\"")]
            public slice<ref ViewerEvent> Events;
            [Description("json:\"stackFrames\"")]
            public map<@string, ViewerFrame> Frames;
            [Description("json:\"displayTimeUnit\"")]
            public @string TimeUnit; // This is where mandatory part of the trace starts (e.g. thread names)
            public long footer;
        }

        public partial struct ViewerEvent
        {
            [Description("json:\"name,omitempty\"")]
            public @string Name;
            [Description("json:\"ph\"")]
            public @string Phase;
            [Description("json:\"s,omitempty\"")]
            public @string Scope;
            [Description("json:\"ts\"")]
            public double Time;
            [Description("json:\"dur,omitempty\"")]
            public double Dur;
            [Description("json:\"pid\"")]
            public ulong Pid;
            [Description("json:\"tid\"")]
            public ulong Tid;
            [Description("json:\"id,omitempty\"")]
            public ulong ID;
            [Description("json:\"sf,omitempty\"")]
            public long Stack;
            [Description("json:\"esf,omitempty\"")]
            public long EndStack;
        }

        public partial struct ViewerFrame
        {
            [Description("json:\"name\"")]
            public @string Name;
            [Description("json:\"parent,omitempty\"")]
            public long Parent;
        }

        public partial struct NameArg
        {
            [Description("json:\"name\"")]
            public @string Name;
        }

        public partial struct SortIndexArg
        {
            [Description("json:\"sort_index\"")]
            public long Index;
        }

        // generateTrace generates json trace for trace-viewer:
        // https://github.com/google/trace-viewer
        // Trace format is described at:
        // https://docs.google.com/document/d/1CvAClvFfyA5R-PhYUmn5OOQtYMH4h6I0nSsKchNAySU/view
        // If gtrace=true, generate trace for goroutine goid, otherwise whole trace.
        // startTime, endTime determine part of the trace that we are interested in.
        // gset restricts goroutines that are included in the resulting trace.
        private static (ViewerData, error) generateTrace(ref traceParams @params)
        {
            traceContext ctx = ref new traceContext(traceParams:params);
            ctx.frameTree.children = make_map<ulong, frameNode>();
            ctx.data.Frames = make_map<@string, ViewerFrame>();
            ctx.data.TimeUnit = "ns";
            long maxProc = 0L;
            var ginfos = make_map<ulong, ref gInfo>();
            var stacks = @params.parsed.Stacks;

            Func<ulong, ref gInfo> getGInfo = g =>
            {
                var (info, ok) = ginfos[g];
                if (!ok)
                {
                    info = ref new gInfo();
                    ginfos[g] = info;
                }
                return info;
            } 

            // Since we make many calls to setGState, we record a sticky
            // error in setGStateErr and check it after every event.
; 

            // Since we make many calls to setGState, we record a sticky
            // error in setGStateErr and check it after every event.
            error setGStateErr = default;
            Action<ref trace.Event, ulong, gState, gState> setGState = (ev, g, oldState, newState) =>
            {
                var info = getGInfo(g);
                if (oldState == gWaiting && info.state == gWaitingGC)
                { 
                    // For checking, gWaiting counts as any gWaiting*.
                    oldState = info.state;
                }
                if (info.state != oldState && setGStateErr == null)
                {
                    setGStateErr = error.As(fmt.Errorf("expected G %d to be in state %d, but got state %d", g, oldState, newState));
                }
                ctx.gstates[info.state]--;
                ctx.gstates[newState]++;
                info.state = newState;
            }
;

            foreach (var (_, ev) in ctx.parsed.Events)
            { 
                // Handle state transitions before we filter out events.

                if (ev.Type == trace.EvGoStart || ev.Type == trace.EvGoStartLabel) 
                    setGState(ev, ev.G, gRunnable, gRunning);
                    info = getGInfo(ev.G);
                    info.start = ev;
                else if (ev.Type == trace.EvProcStart) 
                    ctx.threadStats.prunning++;
                else if (ev.Type == trace.EvProcStop) 
                    ctx.threadStats.prunning--;
                else if (ev.Type == trace.EvGoCreate) 
                    var newG = ev.Args[0L];
                    info = getGInfo(newG);
                    if (info.name != "")
                    {
                        return (ctx.data, fmt.Errorf("duplicate go create event for go id=%d detected at offset %d", newG, ev.Off));
                    }
                    var (stk, ok) = stacks[ev.Args[1L]];
                    if (!ok || len(stk) == 0L)
                    {
                        return (ctx.data, fmt.Errorf("invalid go create event: missing stack information for go id=%d at offset %d", newG, ev.Off));
                    }
                    var fname = stk[0L].Fn;
                    info.name = fmt.Sprintf("G%v %s", newG, fname);
                    info.isSystemG = strings.HasPrefix(fname, "runtime.") && fname != "runtime.main";

                    ctx.gcount++;
                    setGState(ev, newG, gDead, gRunnable);
                else if (ev.Type == trace.EvGoEnd) 
                    ctx.gcount--;
                    setGState(ev, ev.G, gRunning, gDead);
                else if (ev.Type == trace.EvGoUnblock) 
                    setGState(ev, ev.Args[0L], gWaiting, gRunnable);
                else if (ev.Type == trace.EvGoSysExit) 
                    setGState(ev, ev.G, gWaiting, gRunnable);
                    if (getGInfo(ev.G).isSystemG)
                    {
                        ctx.threadStats.insyscallRuntime--;
                    }
                    else
                    {
                        ctx.threadStats.insyscall--;
                    }
                else if (ev.Type == trace.EvGoSysBlock) 
                    setGState(ev, ev.G, gRunning, gWaiting);
                    if (getGInfo(ev.G).isSystemG)
                    {
                        ctx.threadStats.insyscallRuntime++;
                    }
                    else
                    {
                        ctx.threadStats.insyscall++;
                    }
                else if (ev.Type == trace.EvGoSched || ev.Type == trace.EvGoPreempt) 
                    setGState(ev, ev.G, gRunning, gRunnable);
                else if (ev.Type == trace.EvGoStop || ev.Type == trace.EvGoSleep || ev.Type == trace.EvGoBlock || ev.Type == trace.EvGoBlockSend || ev.Type == trace.EvGoBlockRecv || ev.Type == trace.EvGoBlockSelect || ev.Type == trace.EvGoBlockSync || ev.Type == trace.EvGoBlockCond || ev.Type == trace.EvGoBlockNet) 
                    setGState(ev, ev.G, gRunning, gWaiting);
                else if (ev.Type == trace.EvGoBlockGC) 
                    setGState(ev, ev.G, gRunning, gWaitingGC);
                else if (ev.Type == trace.EvGCMarkAssistStart) 
                    getGInfo(ev.G).markAssist;

                    ev;
                else if (ev.Type == trace.EvGCMarkAssistDone) 
                    getGInfo(ev.G).markAssist;

                    null;
                else if (ev.Type == trace.EvGoWaiting) 
                    setGState(ev, ev.G, gRunnable, gWaiting);
                else if (ev.Type == trace.EvGoInSyscall) 
                    // Cancel out the effect of EvGoCreate at the beginning.
                    setGState(ev, ev.G, gRunnable, gWaiting);
                    if (getGInfo(ev.G).isSystemG)
                    {
                        ctx.threadStats.insyscallRuntime++;
                    }
                    else
                    {
                        ctx.threadStats.insyscall++;
                    }
                else if (ev.Type == trace.EvHeapAlloc) 
                    ctx.heapStats.heapAlloc = ev.Args[0L];
                else if (ev.Type == trace.EvNextGC) 
                    ctx.heapStats.nextGC = ev.Args[0L];
                                if (setGStateErr != null)
                {
                    return (ctx.data, setGStateErr);
                }
                if (ctx.gstates[gRunnable] < 0L || ctx.gstates[gRunning] < 0L || ctx.threadStats.insyscall < 0L || ctx.threadStats.insyscallRuntime < 0L)
                {
                    return (ctx.data, fmt.Errorf("invalid state after processing %v: runnable=%d running=%d insyscall=%d insyscallRuntime=%d", ev, ctx.gstates[gRunnable], ctx.gstates[gRunning], ctx.threadStats.insyscall, ctx.threadStats.insyscallRuntime));
                } 

                // Ignore events that are from uninteresting goroutines
                // or outside of the interesting timeframe.
                if (ctx.gs != null && ev.P < trace.FakeP && !ctx.gs[ev.G])
                {
                    continue;
                }
                if (ev.Ts < ctx.startTime || ev.Ts > ctx.endTime)
                {
                    continue;
                }
                if (ev.P < trace.FakeP && ev.P > maxProc)
                {
                    maxProc = ev.P;
                } 

                // Emit trace objects.

                if (ev.Type == trace.EvProcStart) 
                    if (ctx.gtrace)
                    {
                        continue;
                    }
                    ctx.emitInstant(ev, "proc start");
                else if (ev.Type == trace.EvProcStop) 
                    if (ctx.gtrace)
                    {
                        continue;
                    }
                    ctx.emitInstant(ev, "proc stop");
                else if (ev.Type == trace.EvGCStart) 
                    ctx.emitSlice(ev, "GC");
                else if (ev.Type == trace.EvGCDone)                 else if (ev.Type == trace.EvGCSTWStart) 
                    if (ctx.gtrace)
                    {
                        continue;
                    }
                    ctx.emitSlice(ev, fmt.Sprintf("STW (%s)", ev.SArgs[0L]));
                else if (ev.Type == trace.EvGCSTWDone)                 else if (ev.Type == trace.EvGCMarkAssistStart) 
                    // Mark assists can continue past preemptions, so truncate to the
                    // whichever comes first. We'll synthesize another slice if
                    // necessary in EvGoStart.
                    var markFinish = ev.Link;
                    var goFinish = getGInfo(ev.G).start.Link;
                    var fakeMarkStart = ev.Value;
                    @string text = "MARK ASSIST";
                    if (markFinish == null || markFinish.Ts > goFinish.Ts)
                    {
                        fakeMarkStart.Link = goFinish;
                        text = "MARK ASSIST (unfinished)";
                    }
                    ctx.emitSlice(ref fakeMarkStart, text);
                else if (ev.Type == trace.EvGCSweepStart) 
                    var slice = ctx.emitSlice(ev, "SWEEP");
                    {
                        var done = ev.Link;

                        if (done != null && done.Args[0L] != 0L)
                        {
                            slice.Arg = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{Sweptuint64`json:"Swept bytes"`Reclaimeduint64`json:"Reclaimed bytes"`}{done.Args[0],done.Args[1]};
                        }

                    }
                else if (ev.Type == trace.EvGoStart || ev.Type == trace.EvGoStartLabel) 
                    info = getGInfo(ev.G);
                    if (ev.Type == trace.EvGoStartLabel)
                    {
                        ctx.emitSlice(ev, ev.SArgs[0L]);
                    }
                    else
                    {
                        ctx.emitSlice(ev, info.name);
                    }
                    if (info.markAssist != null)
                    { 
                        // If we're in a mark assist, synthesize a new slice, ending
                        // either when the mark assist ends or when we're descheduled.
                        markFinish = info.markAssist.Link;
                        goFinish = ev.Link;
                        fakeMarkStart = ev.Value;
                        text = "MARK ASSIST (resumed, unfinished)";
                        if (markFinish != null && markFinish.Ts < goFinish.Ts)
                        {
                            fakeMarkStart.Link = markFinish;
                            text = "MARK ASSIST (resumed)";
                        }
                        ctx.emitSlice(ref fakeMarkStart, text);
                    }
                else if (ev.Type == trace.EvGoCreate) 
                    ctx.emitArrow(ev, "go");
                else if (ev.Type == trace.EvGoUnblock) 
                    ctx.emitArrow(ev, "unblock");
                else if (ev.Type == trace.EvGoSysCall) 
                    ctx.emitInstant(ev, "syscall");
                else if (ev.Type == trace.EvGoSysExit) 
                    ctx.emitArrow(ev, "sysexit");
                // Emit any counter updates.
                ctx.emitThreadCounters(ev);
                ctx.emitHeapCounters(ev);
                ctx.emitGoroutineCounters(ev);
            }
            ctx.data.footer = len(ctx.data.Events);
            ctx.emit(ref new ViewerEvent(Name:"process_name",Phase:"M",Pid:0,Arg:&NameArg{"PROCS"}));
            ctx.emit(ref new ViewerEvent(Name:"process_sort_index",Phase:"M",Pid:0,Arg:&SortIndexArg{1}));

            ctx.emit(ref new ViewerEvent(Name:"process_name",Phase:"M",Pid:1,Arg:&NameArg{"STATS"}));
            ctx.emit(ref new ViewerEvent(Name:"process_sort_index",Phase:"M",Pid:1,Arg:&SortIndexArg{0}));

            ctx.emit(ref new ViewerEvent(Name:"thread_name",Phase:"M",Pid:0,Tid:trace.GCP,Arg:&NameArg{"GC"}));
            ctx.emit(ref new ViewerEvent(Name:"thread_sort_index",Phase:"M",Pid:0,Tid:trace.GCP,Arg:&SortIndexArg{-6}));

            ctx.emit(ref new ViewerEvent(Name:"thread_name",Phase:"M",Pid:0,Tid:trace.NetpollP,Arg:&NameArg{"Network"}));
            ctx.emit(ref new ViewerEvent(Name:"thread_sort_index",Phase:"M",Pid:0,Tid:trace.NetpollP,Arg:&SortIndexArg{-5}));

            ctx.emit(ref new ViewerEvent(Name:"thread_name",Phase:"M",Pid:0,Tid:trace.TimerP,Arg:&NameArg{"Timers"}));
            ctx.emit(ref new ViewerEvent(Name:"thread_sort_index",Phase:"M",Pid:0,Tid:trace.TimerP,Arg:&SortIndexArg{-4}));

            ctx.emit(ref new ViewerEvent(Name:"thread_name",Phase:"M",Pid:0,Tid:trace.SyscallP,Arg:&NameArg{"Syscalls"}));
            ctx.emit(ref new ViewerEvent(Name:"thread_sort_index",Phase:"M",Pid:0,Tid:trace.SyscallP,Arg:&SortIndexArg{-3}));

            if (!ctx.gtrace)
            {
                for (long i = 0L; i <= maxProc; i++)
                {
                    ctx.emit(ref new ViewerEvent(Name:"thread_name",Phase:"M",Pid:0,Tid:uint64(i),Arg:&NameArg{fmt.Sprintf("Proc %v",i)}));
                    ctx.emit(ref new ViewerEvent(Name:"thread_sort_index",Phase:"M",Pid:0,Tid:uint64(i),Arg:&SortIndexArg{i}));
                }

            }
            if (ctx.gtrace && ctx.gs != null)
            {
                foreach (var (k, v) in ginfos)
                {
                    if (!ctx.gs[k])
                    {
                        continue;
                    }
                    ctx.emit(ref new ViewerEvent(Name:"thread_name",Phase:"M",Pid:0,Tid:k,Arg:&NameArg{v.name}));
                }
                ctx.emit(ref new ViewerEvent(Name:"thread_sort_index",Phase:"M",Pid:0,Tid:ctx.maing,Arg:&SortIndexArg{-2}));
                ctx.emit(ref new ViewerEvent(Name:"thread_sort_index",Phase:"M",Pid:0,Tid:0,Arg:&SortIndexArg{-1}));
            }
            return (ctx.data, null);
        }

        private static void emit(this ref traceContext ctx, ref ViewerEvent e)
        {
            ctx.data.Events = append(ctx.data.Events, e);
        }

        private static double time(this ref traceContext ctx, ref trace.Event ev)
        { 
            // Trace viewer wants timestamps in microseconds.
            return float64(ev.Ts - ctx.startTime) / 1000L;
        }

        private static ulong proc(this ref traceContext ctx, ref trace.Event ev)
        {
            if (ctx.gtrace && ev.P < trace.FakeP)
            {
                return ev.G;
            }
            else
            {
                return uint64(ev.P);
            }
        }

        private static ref ViewerEvent emitSlice(this ref traceContext ctx, ref trace.Event ev, @string name)
        {
            ViewerEvent sl = ref new ViewerEvent(Name:name,Phase:"X",Time:ctx.time(ev),Dur:ctx.time(ev.Link)-ctx.time(ev),Tid:ctx.proc(ev),Stack:ctx.stack(ev.Stk),EndStack:ctx.stack(ev.Link.Stk),);
            ctx.emit(sl);
            return sl;
        }

        private partial struct heapCountersArg
        {
            public ulong Allocated;
            public ulong NextGC;
        }

        private static void emitHeapCounters(this ref traceContext ctx, ref trace.Event ev)
        {
            if (ctx.gtrace)
            {
                return;
            }
            if (ctx.prevHeapStats == ctx.heapStats)
            {
                return;
            }
            var diff = uint64(0L);
            if (ctx.heapStats.nextGC > ctx.heapStats.heapAlloc)
            {
                diff = ctx.heapStats.nextGC - ctx.heapStats.heapAlloc;
            }
            ctx.emit(ref new ViewerEvent(Name:"Heap",Phase:"C",Time:ctx.time(ev),Pid:1,Arg:&heapCountersArg{ctx.heapStats.heapAlloc,diff}));
            ctx.prevHeapStats = ctx.heapStats;
        }

        private partial struct goroutineCountersArg
        {
            public ulong Running;
            public ulong Runnable;
            public ulong GCWaiting;
        }

        private static void emitGoroutineCounters(this ref traceContext ctx, ref trace.Event ev)
        {
            if (ctx.gtrace)
            {
                return;
            }
            if (ctx.prevGstates == ctx.gstates)
            {
                return;
            }
            ctx.emit(ref new ViewerEvent(Name:"Goroutines",Phase:"C",Time:ctx.time(ev),Pid:1,Arg:&goroutineCountersArg{uint64(ctx.gstates[gRunning]),uint64(ctx.gstates[gRunnable]),uint64(ctx.gstates[gWaitingGC])}));
            ctx.prevGstates = ctx.gstates;
        }

        private partial struct threadCountersArg
        {
            public ulong Running;
            public ulong InSyscall;
        }

        private static void emitThreadCounters(this ref traceContext ctx, ref trace.Event ev)
        {
            if (ctx.gtrace)
            {
                return;
            }
            if (ctx.prevThreadStats == ctx.threadStats)
            {
                return;
            }
            ctx.emit(ref new ViewerEvent(Name:"Threads",Phase:"C",Time:ctx.time(ev),Pid:1,Arg:&threadCountersArg{Running:ctx.threadStats.prunning,InSyscall:ctx.threadStats.insyscall}));
            ctx.prevThreadStats = ctx.threadStats;
        }

        private static void emitInstant(this ref traceContext ctx, ref trace.Event ev, @string name)
        {
            var arg = default;
            if (ev.Type == trace.EvProcStart)
            {
                public partial struct Arg
                {
                    public ulong ThreadID;
                }
                arg = ref new Arg(ev.Args[0]);
            }
            ctx.emit(ref new ViewerEvent(Name:name,Phase:"I",Scope:"t",Time:ctx.time(ev),Tid:ctx.proc(ev),Stack:ctx.stack(ev.Stk),Arg:arg));
        }

        private static void emitArrow(this ref traceContext ctx, ref trace.Event ev, @string name)
        {
            if (ev.Link == null)
            { 
                // The other end of the arrow is not captured in the trace.
                // For example, a goroutine was unblocked but was not scheduled before trace stop.
                return;
            }
            if (ctx.gtrace && (!ctx.gs[ev.Link.G] || ev.Link.Ts < ctx.startTime || ev.Link.Ts > ctx.endTime))
            {
                return;
            }
            if (ev.P == trace.NetpollP || ev.P == trace.TimerP || ev.P == trace.SyscallP)
            { 
                // Trace-viewer discards arrows if they don't start/end inside of a slice or instant.
                // So emit a fake instant at the start of the arrow.
                ctx.emitInstant(ref new trace.Event(P:ev.P,Ts:ev.Ts), "unblock");
            }
            ctx.arrowSeq++;
            ctx.emit(ref new ViewerEvent(Name:name,Phase:"s",Tid:ctx.proc(ev),ID:ctx.arrowSeq,Time:ctx.time(ev),Stack:ctx.stack(ev.Stk)));
            ctx.emit(ref new ViewerEvent(Name:name,Phase:"t",Tid:ctx.proc(ev.Link),ID:ctx.arrowSeq,Time:ctx.time(ev.Link)));
        }

        private static long stack(this ref traceContext ctx, slice<ref trace.Frame> stk)
        {
            return ctx.buildBranch(ctx.frameTree, stk);
        }

        // buildBranch builds one branch in the prefix tree rooted at ctx.frameTree.
        private static long buildBranch(this ref traceContext ctx, frameNode parent, slice<ref trace.Frame> stk)
        {
            if (len(stk) == 0L)
            {
                return parent.id;
            }
            var last = len(stk) - 1L;
            var frame = stk[last];
            stk = stk[..last];

            var (node, ok) = parent.children[frame.PC];
            if (!ok)
            {
                ctx.frameSeq++;
                node.id = ctx.frameSeq;
                node.children = make_map<ulong, frameNode>();
                parent.children[frame.PC] = node;
                ctx.data.Frames[strconv.Itoa(node.id)] = new ViewerFrame(fmt.Sprintf("%v:%v",frame.Fn,frame.Line),parent.id);
            }
            return ctx.buildBranch(node, stk);
        }
    }
}
