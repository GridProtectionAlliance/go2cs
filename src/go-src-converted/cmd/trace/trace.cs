// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2022 March 06 23:23:13 UTC
// Original source: C:\Program Files\Go\src\cmd\trace\trace.go
using traceviewer = go.cmd.@internal.traceviewer_package;
using json = go.encoding.json_package;
using fmt = go.fmt_package;
using trace = go.@internal.trace_package;
using io = go.io_package;
using log = go.log_package;
using math = go.math_package;
using http = go.net.http_package;
using filepath = go.path.filepath_package;
using runtime = go.runtime_package;
using debug = go.runtime.debug_package;
using sort = go.sort_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using time = go.time_package;
using System;
using System.ComponentModel;


namespace go;

public static partial class main_package {

private static void init() {
    http.HandleFunc("/trace", httpTrace);
    http.HandleFunc("/jsontrace", httpJsonTrace);
    http.HandleFunc("/trace_viewer_html", httpTraceViewerHTML);
    http.HandleFunc("/webcomponents.min.js", webcomponentsJS);
}

// httpTrace serves either whole trace (goid==0) or trace for goid goroutine.
private static void httpTrace(http.ResponseWriter w, ptr<http.Request> _addr_r) {
    ref http.Request r = ref _addr_r.val;

    var (_, err) = parseTrace();
    if (err != null) {
        http.Error(w, err.Error(), http.StatusInternalServerError);
        return ;
    }
    {
        var err = r.ParseForm();

        if (err != null) {
            http.Error(w, err.Error(), http.StatusInternalServerError);
            return ;
        }
    }

    var html = strings.ReplaceAll(templTrace, "{{PARAMS}}", r.Form.Encode());
    w.Write((slice<byte>)html);


}

// https://chromium.googlesource.com/catapult/+/9508452e18f130c98499cb4c4f1e1efaedee8962/tracing/docs/embedding-trace-viewer.md
// This is almost verbatim copy of https://chromium-review.googlesource.com/c/catapult/+/2062938/2/tracing/bin/index.html
private static @string templTrace = "\n<html>\n<head>\n<script src=\"/webcomponents.min.js\"></script>\n<script>\n\'use strict" +
    "\';\n\nfunction onTraceViewerImportFail() {\n  document.addEventListener(\'DOMContent" +
    "Loaded\', function() {\n    document.body.textContent =\n    \'/trace_viewer_full.ht" +
    "ml is missing. File a bug in https://golang.org/issue\';\n  });\n}\n</script>\n\n<link" +
    " rel=\"import\" href=\"/trace_viewer_html\"\n      onerror=\"onTraceViewerImportFail(e" +
    "vent)\">\n\n<style type=\"text/css\">\n  html, body {\n    box-sizing: border-box;\n    " +
    "overflow: hidden;\n    margin: 0px;\n    padding: 0;\n    width: 100%;\n    height: " +
    "100%;\n  }\n  #trace-viewer {\n    width: 100%;\n    height: 100%;\n  }\n  #trace-view" +
    "er:focus {\n    outline: none;\n  }\n</style>\n<script>\n\'use strict\';\n(function() {\n" +
    "  var viewer;\n  var url;\n  var model;\n\n  function load() {\n    var req = new XML" +
    "HttpRequest();\n    var isBinary = /[.]gz$/.test(url) || /[.]zip$/.test(url);\n   " +
    " req.overrideMimeType(\'text/plain; charset=x-user-defined\');\n    req.open(\'GET\'," +
    " url, true);\n    if (isBinary)\n      req.responseType = \'arraybuffer\';\n\n    req." +
    "onreadystatechange = function(event) {\n      if (req.readyState !== 4)\n        r" +
    "eturn;\n\n      window.setTimeout(function() {\n        if (req.status === 200)\n   " +
    "       onResult(isBinary ? req.response : req.responseText);\n        else\n      " +
    "    onResultFail(req.status);\n      }, 0);\n    };\n    req.send(null);\n  }\n\n  fun" +
    "ction onResultFail(err) {\n    var overlay = new tr.ui.b.Overlay();\n    overlay.t" +
    "extContent = err + \': \' + url + \' could not be loaded\';\n    overlay.title = \'Fai" +
    "led to fetch data\';\n    overlay.visible = true;\n  }\n\n  function onResult(result)" +
    " {\n    model = new tr.Model();\n    var opts = new tr.importer.ImportOptions();\n " +
    "   opts.shiftWorldToZero = false;\n    var i = new tr.importer.Import(model, opts" +
    ");\n    var p = i.importTracesWithProgressDialog([result]);\n    p.then(onModelLoa" +
    "ded, onImportFail);\n  }\n\n  function onModelLoaded() {\n    viewer.model = model;\n" +
    "    viewer.viewTitle = \"trace\";\n\n    if (!model || model.bounds.isEmpty)\n      r" +
    "eturn;\n    var sel = window.location.hash.substr(1);\n    if (sel === \'\')\n      r" +
    "eturn;\n    var parts = sel.split(\':\');\n    var range = new (tr.b.Range || tr.b.m" +
    "ath.Range)();\n    range.addValue(parseFloat(parts[0]));\n    range.addValue(parse" +
    "Float(parts[1]));\n    viewer.trackView.viewport.interestRange.set(range);\n  }\n\n " +
    " function onImportFail(err) {\n    var overlay = new tr.ui.b.Overlay();\n    overl" +
    "ay.textContent = tr.b.normalizeException(err).message;\n    overlay.title = \'Impo" +
    "rt error\';\n    overlay.visible = true;\n  }\n\n  document.addEventListener(\'WebComp" +
    "onentsReady\', function() {\n    var container = document.createElement(\'track-vie" +
    "w-container\');\n    container.id = \'track_view_container\';\n\n    viewer = document" +
    ".createElement(\'tr-ui-timeline-view\');\n    viewer.track_view_container = contain" +
    "er;\n    Polymer.dom(viewer).appendChild(container);\n\n    viewer.id = \'trace-view" +
    "er\';\n    viewer.globalMode = true;\n    Polymer.dom(document.body).appendChild(vi" +
    "ewer);\n\n    url = \'/jsontrace?{{PARAMS}}\';\n    load();\n  });\n}());\n</script>\n</h" +
    "ead>\n<body>\n</body>\n</html>\n";

// httpTraceViewerHTML serves static part of trace-viewer.
// This URL is queried from templTrace HTML.
private static void httpTraceViewerHTML(http.ResponseWriter w, ptr<http.Request> _addr_r) {
    ref http.Request r = ref _addr_r.val;

    http.ServeFile(w, r, filepath.Join(runtime.GOROOT(), "misc", "trace", "trace_viewer_full.html"));
}

private static void webcomponentsJS(http.ResponseWriter w, ptr<http.Request> _addr_r) {
    ref http.Request r = ref _addr_r.val;

    http.ServeFile(w, r, filepath.Join(runtime.GOROOT(), "misc", "trace", "webcomponents.min.js"));
}

// httpJsonTrace serves json trace, requested from within templTrace HTML.
private static void httpJsonTrace(http.ResponseWriter w, ptr<http.Request> _addr_r) => func((defer, _, _) => {
    ref http.Request r = ref _addr_r.val;

    defer(debug.FreeOSMemory());
    defer(reportMemoryUsage("after httpJsonTrace")); 
    // This is an AJAX handler, so instead of http.Error we use log.Printf to log errors.
    var (res, err) = parseTrace();
    if (err != null) {
        log.Printf("failed to parse trace: %v", err);
        return ;
    }
    ptr<traceParams> @params = addr(new traceParams(parsed:res,endTime:math.MaxInt64,));

    {
        var goids = r.FormValue("goid");

        if (goids != "") { 
            // If goid argument is present, we are rendering a trace for this particular goroutine.
            var (goid, err) = strconv.ParseUint(goids, 10, 64);
            if (err != null) {
                log.Printf("failed to parse goid parameter %q: %v", goids, err);
                return ;
            }

            analyzeGoroutines(res.Events);
            var (g, ok) = gs[goid];
            if (!ok) {
                log.Printf("failed to find goroutine %d", goid);
                return ;
            }

            @params.mode = modeGoroutineOriented;
            @params.startTime = g.StartTime;
            if (g.EndTime != 0) {
                @params.endTime = g.EndTime;
            }
            else
 { // The goroutine didn't end.
                @params.endTime = lastTimestamp();

            }

            @params.maing = goid;
            @params.gs = trace.RelatedGoroutines(res.Events, goid);

        }        {
            var taskids__prev2 = taskids;

            var taskids = r.FormValue("taskid");


            else if (taskids != "") {
                var (taskid, err) = strconv.ParseUint(taskids, 10, 64);
                if (err != null) {
                    log.Printf("failed to parse taskid parameter %q: %v", taskids, err);
                    return ;
                }
                var (annotRes, _) = analyzeAnnotations();
                var (task, ok) = annotRes.tasks[taskid];
                if (!ok || len(task.events) == 0) {
                    log.Printf("failed to find task with id %d", taskid);
                    return ;
                }
                var goid = task.events[0].G;
                @params.mode = modeGoroutineOriented | modeTaskOriented;
                @params.startTime = task.firstTimestamp() - 1;
                @params.endTime = task.lastTimestamp() + 1;
                @params.maing = goid;
                @params.tasks = task.descendants();
                map gs = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ulong, bool>{};
                foreach (var (_, t) in @params.tasks) { 
                    // find only directly involved goroutines
                    foreach (var (k, v) in t.RelatedGoroutines(res.Events, 0)) {
                        gs[k] = v;
                    }

                }
                @params.gs = gs;

            }            {
                var taskids__prev3 = taskids;

                taskids = r.FormValue("focustask");


                else if (taskids != "") {
                    (taskid, err) = strconv.ParseUint(taskids, 10, 64);
                    if (err != null) {
                        log.Printf("failed to parse focustask parameter %q: %v", taskids, err);
                        return ;
                    }
                    (annotRes, _) = analyzeAnnotations();
                    (task, ok) = annotRes.tasks[taskid];
                    if (!ok || len(task.events) == 0) {
                        log.Printf("failed to find task with id %d", taskid);
                        return ;
                    }
                    @params.mode = modeTaskOriented;
                    @params.startTime = task.firstTimestamp() - 1;
                    @params.endTime = task.lastTimestamp() + 1;
                    @params.tasks = task.descendants();
                }

                taskids = taskids__prev3;

            }



            taskids = taskids__prev2;

        }



    }


    var start = int64(0);
    var end = int64(math.MaxInt64);
    {
        var startStr = r.FormValue("start");
        var endStr = r.FormValue("end");

        if (startStr != "" && endStr != "") { 
            // If start/end arguments are present, we are rendering a range of the trace.
            start, err = strconv.ParseInt(startStr, 10, 64);
            if (err != null) {
                log.Printf("failed to parse start parameter %q: %v", startStr, err);
                return ;
            }

            end, err = strconv.ParseInt(endStr, 10, 64);
            if (err != null) {
                log.Printf("failed to parse end parameter %q: %v", endStr, err);
                return ;
            }

        }
    }


    var c = viewerDataTraceConsumer(w, start, end);
    {
        var err = generateTrace(params, c);

        if (err != null) {
            log.Printf("failed to generate trace: %v", err);
            return ;
        }
    }

});

public partial struct Range {
    public @string Name;
    public nint Start;
    public nint End;
    public long StartTime;
    public long EndTime;
}

public static @string URL(this Range r) {
    return fmt.Sprintf("/trace?start=%d&end=%d", r.Start, r.End);
}

// splitTrace splits the trace into a number of ranges,
// each resulting in approx 100MB of json output
// (trace viewer can hardly handle more).
private static slice<Range> splitTrace(trace.ParseResult res) {
    ptr<traceParams> @params = addr(new traceParams(parsed:res,endTime:math.MaxInt64,));
    var (s, c) = splittingTraceConsumer(100 << 20); // 100M
    {
        var err = generateTrace(params, c);

        if (err != null) {
            dief("%v\n", err);
        }
    }

    return s.Ranges;

}

private partial struct splitter {
    public slice<Range> Ranges;
}

private static (ptr<splitter>, traceConsumer) splittingTraceConsumer(nint max) {
    ptr<splitter> _p0 = default!;
    traceConsumer _p0 = default;

    private partial struct eventSz {
        public double Time;
        public nint Sz;
    }

    traceviewer.Data data = new traceviewer.Data(Frames:make(map[string]traceviewer.Frame));    slice<eventSz> sizes = default;    ref countingWriter cw = ref heap(out ptr<countingWriter> _addr_cw);

    ptr<splitter> s = @new<splitter>();

    return (_addr_s!, new traceConsumer(consumeTimeUnit:func(unitstring){data.TimeUnit=unit},consumeViewerEvent:func(v*traceviewer.Event,requiredbool){ifrequired{data.Events=append(data.Events,v)return}enc:=json.NewEncoder(&cw)enc.Encode(v)sizes=append(sizes,eventSz{v.Time,cw.size+1})cw.size=0},consumeViewerFrame:func(kstring,vtraceviewer.Frame){data.Frames[k]=v},flush:func(){cw.size=0enc:=json.NewEncoder(&cw)enc.Encode(data)minSize:=cw.sizesum:=minSizestart:=0fori,ev:=rangesizes{ifsum+ev.Sz>max{startTime:=time.Duration(sizes[start].Time*1000)endTime:=time.Duration(ev.Time*1000)ranges=append(ranges,Range{Name:fmt.Sprintf("%v-%v",startTime,endTime),Start:start,End:i+1,StartTime:int64(startTime),EndTime:int64(endTime),})start=i+1sum=minSize}else{sum+=ev.Sz+1}}iflen(ranges)<=1{s.Ranges=nilreturn}ifend:=len(sizes)-1;start<end{ranges=append(ranges,Range{Name:fmt.Sprintf("%v-%v",time.Duration(sizes[start].Time*1000),time.Duration(sizes[end].Time*1000)),Start:start,End:end,StartTime:int64(sizes[start].Time*1000),EndTime:int64(sizes[end].Time*1000),})}s.Ranges=ranges},));

}

private partial struct countingWriter {
    public nint size;
}

private static (nint, error) Write(this ptr<countingWriter> _addr_cw, slice<byte> data) {
    nint _p0 = default;
    error _p0 = default!;
    ref countingWriter cw = ref _addr_cw.val;

    cw.size += len(data);
    return (len(data), error.As(null!)!);
}

private partial struct traceParams {
    public trace.ParseResult parsed;
    public traceviewMode mode;
    public long startTime;
    public long endTime;
    public ulong maing; // for goroutine-oriented view, place this goroutine on the top row
    public map<ulong, bool> gs; // Goroutines to be displayed for goroutine-oriented or task-oriented view
    public slice<ptr<taskDesc>> tasks; // Tasks to be displayed. tasks[0] is the top-most task
}

private partial struct traceviewMode { // : nuint
}

private static readonly traceviewMode modeGoroutineOriented = 1 << (int)(iota);
private static readonly var modeTaskOriented = 0;


private partial struct traceContext {
    public ref ptr<traceParams> ptr<traceParams> => ref ptr<traceParams>_ptr;
    public traceConsumer consumer;
    public frameNode frameTree;
    public nint frameSeq;
    public ulong arrowSeq;
    public ulong gcount;
    public heapStats heapStats;
    public heapStats prevHeapStats;
    public threadStats threadStats;
    public threadStats prevThreadStats;
    public array<long> gstates;
    public array<long> prevGstates;
    public nint regionID; // last emitted region id. incremented in each emitRegion call.
}

private partial struct heapStats {
    public ulong heapAlloc;
    public ulong nextGC;
}

private partial struct threadStats {
    public long insyscallRuntime; // system goroutine in syscall
    public long insyscall; // user goroutine in syscall
    public long prunning; // thread running P
}

private partial struct frameNode {
    public nint id;
    public map<ulong, frameNode> children;
}

private partial struct gState { // : nint
}

private static readonly gState gDead = iota;
private static readonly var gRunnable = 0;
private static readonly var gRunning = 1;
private static readonly var gWaiting = 2;
private static readonly var gWaitingGC = 3;

private static readonly var gStateCount = 4;


private partial struct gInfo {
    public gState state; // current state
    public @string name; // name chosen for this goroutine at first EvGoStart
    public bool isSystemG;
    public ptr<trace.Event> start; // most recent EvGoStart
    public ptr<trace.Event> markAssist; // if non-nil, the mark assist currently running.
}

public partial struct NameArg {
    [Description("json:\"name\"")]
    public @string Name;
}

public partial struct TaskArg {
    [Description("json:\"id\"")]
    public ulong ID;
    [Description("json:\"start_g,omitempty\"")]
    public ulong StartG;
    [Description("json:\"end_g,omitempty\"")]
    public ulong EndG;
}

public partial struct RegionArg {
    [Description("json:\"taskid,omitempty\"")]
    public ulong TaskID;
}

public partial struct SortIndexArg {
    [Description("json:\"sort_index\"")]
    public nint Index;
}

private partial struct traceConsumer {
    public Action<@string> consumeTimeUnit;
    public Action<ptr<traceviewer.Event>, bool> consumeViewerEvent;
    public Action<@string, traceviewer.Frame> consumeViewerFrame;
    public Action flush;
}

private static readonly nint procsSection = 0; // where Goroutines or per-P timelines are presented.
private static readonly nint statsSection = 1; // where counters are presented.
private static readonly nint tasksSection = 2; // where Task hierarchy & timeline is presented.

// generateTrace generates json trace for trace-viewer:
// https://github.com/google/trace-viewer
// Trace format is described at:
// https://docs.google.com/document/d/1CvAClvFfyA5R-PhYUmn5OOQtYMH4h6I0nSsKchNAySU/view
// If mode==goroutineMode, generate trace for goroutine goid, otherwise whole trace.
// startTime, endTime determine part of the trace that we are interested in.
// gset restricts goroutines that are included in the resulting trace.
private static error generateTrace(ptr<traceParams> _addr_@params, traceConsumer consumer) => func((defer, _, _) => {
    ref traceParams @params = ref _addr_@params.val;

    defer(consumer.flush());

    ptr<traceContext> ctx = addr(new traceContext(traceParams:params));
    ctx.frameTree.children = make_map<ulong, frameNode>();
    ctx.consumer = consumer;

    ctx.consumer.consumeTimeUnit("ns");
    nint maxProc = 0;
    var ginfos = make_map<ulong, ptr<gInfo>>();
    var stacks = @params.parsed.Stacks;

    Func<ulong, ptr<gInfo>> getGInfo = g => {
        var (info, ok) = ginfos[g];
        if (!ok) {
            info = addr(new gInfo());
            ginfos[g] = info;
        }
        return error.As(info)!;

    }; 

    // Since we make many calls to setGState, we record a sticky
    // error in setGStateErr and check it after every event.
    error setGStateErr = default!;
    Action<ptr<trace.Event>, ulong, gState, gState> setGState = (ev, g, oldState, newState) => {
        var info = getGInfo(g);
        if (oldState == gWaiting && info.state == gWaitingGC) { 
            // For checking, gWaiting counts as any gWaiting*.
            oldState = info.state;

        }
        if (info.state != oldState && setGStateErr == null) {
            setGStateErr = error.As(fmt.Errorf("expected G %d to be in state %d, but got state %d", g, oldState, newState))!;
        }
        ctx.gstates[info.state]--;
        ctx.gstates[newState]++;
        info.state = newState;

    };

    foreach (var (_, ev) in ctx.parsed.Events) { 
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
            var newG = ev.Args[0];
            info = getGInfo(newG);
            if (info.name != "") {
                return error.As(fmt.Errorf("duplicate go create event for go id=%d detected at offset %d", newG, ev.Off))!;
            }
            var (stk, ok) = stacks[ev.Args[1]];
            if (!ok || len(stk) == 0) {
                return error.As(fmt.Errorf("invalid go create event: missing stack information for go id=%d at offset %d", newG, ev.Off))!;
            }
            var fname = stk[0].Fn;
            info.name = fmt.Sprintf("G%v %s", newG, fname);
            info.isSystemG = isSystemGoroutine(fname);

            ctx.gcount++;
            setGState(ev, newG, gDead, gRunnable);
        else if (ev.Type == trace.EvGoEnd) 
            ctx.gcount--;
            setGState(ev, ev.G, gRunning, gDead);
        else if (ev.Type == trace.EvGoUnblock) 
            setGState(ev, ev.Args[0], gWaiting, gRunnable);
        else if (ev.Type == trace.EvGoSysExit) 
            setGState(ev, ev.G, gWaiting, gRunnable);
            if (getGInfo(ev.G).isSystemG) {
                ctx.threadStats.insyscallRuntime--;
            }
            else
 {
                ctx.threadStats.insyscall--;
            }

        else if (ev.Type == trace.EvGoSysBlock) 
            setGState(ev, ev.G, gRunning, gWaiting);
            if (getGInfo(ev.G).isSystemG) {
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
            if (getGInfo(ev.G).isSystemG) {
                ctx.threadStats.insyscallRuntime++;
            }
            else
 {
                ctx.threadStats.insyscall++;
            }

        else if (ev.Type == trace.EvHeapAlloc) 
            ctx.heapStats.heapAlloc = ev.Args[0];
        else if (ev.Type == trace.EvHeapGoal) 
            ctx.heapStats.nextGC = ev.Args[0];
                if (setGStateErr != null) {
            return error.As(setGStateErr)!;
        }
        if (ctx.gstates[gRunnable] < 0 || ctx.gstates[gRunning] < 0 || ctx.threadStats.insyscall < 0 || ctx.threadStats.insyscallRuntime < 0) {
            return error.As(fmt.Errorf("invalid state after processing %v: runnable=%d running=%d insyscall=%d insyscallRuntime=%d", ev, ctx.gstates[gRunnable], ctx.gstates[gRunning], ctx.threadStats.insyscall, ctx.threadStats.insyscallRuntime))!;
        }
        if (ctx.gs != null && ev.P < trace.FakeP && !ctx.gs[ev.G]) {
            continue;
        }
        if (!withinTimeRange(_addr_ev, ctx.startTime, ctx.endTime)) {
            continue;
        }
        if (ev.P < trace.FakeP && ev.P > maxProc) {
            maxProc = ev.P;
        }

        if (ev.Type == trace.EvProcStart) 
            if (ctx.mode & modeGoroutineOriented != 0) {
                continue;
            }
            ctx.emitInstant(ev, "proc start", "");
        else if (ev.Type == trace.EvProcStop) 
            if (ctx.mode & modeGoroutineOriented != 0) {
                continue;
            }
            ctx.emitInstant(ev, "proc stop", "");
        else if (ev.Type == trace.EvGCStart) 
            ctx.emitSlice(ev, "GC");
        else if (ev.Type == trace.EvGCDone)         else if (ev.Type == trace.EvGCSTWStart) 
            if (ctx.mode & modeGoroutineOriented != 0) {
                continue;
            }
            ctx.emitSlice(ev, fmt.Sprintf("STW (%s)", ev.SArgs[0]));
        else if (ev.Type == trace.EvGCSTWDone)         else if (ev.Type == trace.EvGCMarkAssistStart) 
            // Mark assists can continue past preemptions, so truncate to the
            // whichever comes first. We'll synthesize another slice if
            // necessary in EvGoStart.
            var markFinish = ev.Link;
            var goFinish = getGInfo(ev.G).start.Link;
            ref var fakeMarkStart = ref heap(ev.val, out ptr<var> _addr_fakeMarkStart);
            @string text = "MARK ASSIST";
            if (markFinish == null || markFinish.Ts > goFinish.Ts) {
                fakeMarkStart.Link = goFinish;
                text = "MARK ASSIST (unfinished)";
            }
            ctx.emitSlice(_addr_fakeMarkStart, text);
        else if (ev.Type == trace.EvGCSweepStart) 
            var slice = ctx.makeSlice(ev, "SWEEP");
            {
                var done = ev.Link;

                if (done != null && done.Args[0] != 0) {
                    slice.Arg = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{Sweptuint64`json:"Swept bytes"`Reclaimeduint64`json:"Reclaimed bytes"`}{done.Args[0],done.Args[1]};
                }

            }

            ctx.emit(slice);
        else if (ev.Type == trace.EvGoStart || ev.Type == trace.EvGoStartLabel) 
            info = getGInfo(ev.G);
            if (ev.Type == trace.EvGoStartLabel) {
                ctx.emitSlice(ev, ev.SArgs[0]);
            }
            else
 {
                ctx.emitSlice(ev, info.name);
            }

            if (info.markAssist != null) { 
                // If we're in a mark assist, synthesize a new slice, ending
                // either when the mark assist ends or when we're descheduled.
                markFinish = info.markAssist.Link;
                goFinish = ev.Link;
                fakeMarkStart = ev.val;
                text = "MARK ASSIST (resumed, unfinished)";
                if (markFinish != null && markFinish.Ts < goFinish.Ts) {
                    fakeMarkStart.Link = markFinish;
                    text = "MARK ASSIST (resumed)";
                }

                ctx.emitSlice(_addr_fakeMarkStart, text);

            }

        else if (ev.Type == trace.EvGoCreate) 
            ctx.emitArrow(ev, "go");
        else if (ev.Type == trace.EvGoUnblock) 
            ctx.emitArrow(ev, "unblock");
        else if (ev.Type == trace.EvGoSysCall) 
            ctx.emitInstant(ev, "syscall", "");
        else if (ev.Type == trace.EvGoSysExit) 
            ctx.emitArrow(ev, "sysexit");
        else if (ev.Type == trace.EvUserLog) 
            ctx.emitInstant(ev, formatUserLog(ev), "user event");
        else if (ev.Type == trace.EvUserTaskCreate) 
            ctx.emitInstant(ev, "task start", "user event");
        else if (ev.Type == trace.EvUserTaskEnd) 
            ctx.emitInstant(ev, "task end", "user event");
        // Emit any counter updates.
        ctx.emitThreadCounters(ev);
        ctx.emitHeapCounters(ev);
        ctx.emitGoroutineCounters(ev);

    }    ctx.emitSectionFooter(statsSection, "STATS", 0);

    if (ctx.mode & modeTaskOriented != 0) {
        ctx.emitSectionFooter(tasksSection, "TASKS", 1);
    }
    if (ctx.mode & modeGoroutineOriented != 0) {
        ctx.emitSectionFooter(procsSection, "G", 2);
    }
    else
 {
        ctx.emitSectionFooter(procsSection, "PROCS", 2);
    }
    ctx.emitFooter(addr(new traceviewer.Event(Name:"thread_name",Phase:"M",PID:procsSection,TID:trace.GCP,Arg:&NameArg{"GC"})));
    ctx.emitFooter(addr(new traceviewer.Event(Name:"thread_sort_index",Phase:"M",PID:procsSection,TID:trace.GCP,Arg:&SortIndexArg{-6})));

    ctx.emitFooter(addr(new traceviewer.Event(Name:"thread_name",Phase:"M",PID:procsSection,TID:trace.NetpollP,Arg:&NameArg{"Network"})));
    ctx.emitFooter(addr(new traceviewer.Event(Name:"thread_sort_index",Phase:"M",PID:procsSection,TID:trace.NetpollP,Arg:&SortIndexArg{-5})));

    ctx.emitFooter(addr(new traceviewer.Event(Name:"thread_name",Phase:"M",PID:procsSection,TID:trace.TimerP,Arg:&NameArg{"Timers"})));
    ctx.emitFooter(addr(new traceviewer.Event(Name:"thread_sort_index",Phase:"M",PID:procsSection,TID:trace.TimerP,Arg:&SortIndexArg{-4})));

    ctx.emitFooter(addr(new traceviewer.Event(Name:"thread_name",Phase:"M",PID:procsSection,TID:trace.SyscallP,Arg:&NameArg{"Syscalls"})));
    ctx.emitFooter(addr(new traceviewer.Event(Name:"thread_sort_index",Phase:"M",PID:procsSection,TID:trace.SyscallP,Arg:&SortIndexArg{-3}))); 

    // Display rows for Ps if we are in the default trace view mode (not goroutine-oriented presentation)
    if (ctx.mode & modeGoroutineOriented == 0) {
        {
            nint i__prev1 = i;

            for (nint i = 0; i <= maxProc; i++) {
                ctx.emitFooter(addr(new traceviewer.Event(Name:"thread_name",Phase:"M",PID:procsSection,TID:uint64(i),Arg:&NameArg{fmt.Sprintf("Proc %v",i)})));
                ctx.emitFooter(addr(new traceviewer.Event(Name:"thread_sort_index",Phase:"M",PID:procsSection,TID:uint64(i),Arg:&SortIndexArg{i})));
            }


            i = i__prev1;
        }

    }
    if (ctx.mode & modeTaskOriented != 0) { 
        // sort tasks based on the task start time.
        var sortedTask = make_slice<ptr<taskDesc>>(0, len(ctx.tasks));
        {
            var task__prev1 = task;

            foreach (var (_, __task) in ctx.tasks) {
                task = __task;
                sortedTask = append(sortedTask, task);
            }

            task = task__prev1;
        }

        sort.SliceStable(sortedTask, (i, j) => {
            var ti = sortedTask[i];
            var tj = sortedTask[j];
            if (ti.firstTimestamp() == tj.firstTimestamp()) {
                return error.As(ti.lastTimestamp() < tj.lastTimestamp())!;
            }

            return error.As(ti.firstTimestamp() < tj.firstTimestamp())!;

        });

        {
            nint i__prev1 = i;
            var task__prev1 = task;

            foreach (var (__i, __task) in sortedTask) {
                i = __i;
                task = __task;
                ctx.emitTask(task, i); 

                // If we are in goroutine-oriented mode, we draw regions.
                // TODO(hyangah): add this for task/P-oriented mode (i.e., focustask view) too.
                if (ctx.mode & modeGoroutineOriented != 0) {
                    foreach (var (_, s) in task.regions) {
                        ctx.emitRegion(s);
                    }
                }

            }

            i = i__prev1;
            task = task__prev1;
        }
    }
    if (ctx.mode & modeGoroutineOriented != 0) {
        foreach (var (k, v) in ginfos) {
            if (!ctx.gs[k]) {
                continue;
            }
            ctx.emitFooter(addr(new traceviewer.Event(Name:"thread_name",Phase:"M",PID:procsSection,TID:k,Arg:&NameArg{v.name})));
        }        ctx.emitFooter(addr(new traceviewer.Event(Name:"thread_sort_index",Phase:"M",PID:procsSection,TID:ctx.maing,Arg:&SortIndexArg{-2}))); 
        // Row for GC or global state (specified with G=0)
        ctx.emitFooter(addr(new traceviewer.Event(Name:"thread_sort_index",Phase:"M",PID:procsSection,TID:0,Arg:&SortIndexArg{-1})));

    }
    return error.As(null!)!;

});

private static void emit(this ptr<traceContext> _addr_ctx, ptr<traceviewer.Event> _addr_e) {
    ref traceContext ctx = ref _addr_ctx.val;
    ref traceviewer.Event e = ref _addr_e.val;

    ctx.consumer.consumeViewerEvent(e, false);
}

private static void emitFooter(this ptr<traceContext> _addr_ctx, ptr<traceviewer.Event> _addr_e) {
    ref traceContext ctx = ref _addr_ctx.val;
    ref traceviewer.Event e = ref _addr_e.val;

    ctx.consumer.consumeViewerEvent(e, true);
}
private static void emitSectionFooter(this ptr<traceContext> _addr_ctx, ulong sectionID, @string name, nint priority) {
    ref traceContext ctx = ref _addr_ctx.val;

    ctx.emitFooter(addr(new traceviewer.Event(Name:"process_name",Phase:"M",PID:sectionID,Arg:&NameArg{name})));
    ctx.emitFooter(addr(new traceviewer.Event(Name:"process_sort_index",Phase:"M",PID:sectionID,Arg:&SortIndexArg{priority})));
}

private static double time(this ptr<traceContext> _addr_ctx, ptr<trace.Event> _addr_ev) {
    ref traceContext ctx = ref _addr_ctx.val;
    ref trace.Event ev = ref _addr_ev.val;
 
    // Trace viewer wants timestamps in microseconds.
    return float64(ev.Ts) / 1000;

}

private static bool withinTimeRange(ptr<trace.Event> _addr_ev, long s, long e) {
    ref trace.Event ev = ref _addr_ev.val;

    {
        var evEnd = ev.Link;

        if (evEnd != null) {
            return ev.Ts <= e && evEnd.Ts >= s;
        }
    }

    return ev.Ts >= s && ev.Ts <= e;

}

private static bool tsWithinRange(long ts, long s, long e) {
    return s <= ts && ts <= e;
}

private static ulong proc(this ptr<traceContext> _addr_ctx, ptr<trace.Event> _addr_ev) {
    ref traceContext ctx = ref _addr_ctx.val;
    ref trace.Event ev = ref _addr_ev.val;

    if (ctx.mode & modeGoroutineOriented != 0 && ev.P < trace.FakeP) {
        return ev.G;
    }
    else
 {
        return uint64(ev.P);
    }
}

private static void emitSlice(this ptr<traceContext> _addr_ctx, ptr<trace.Event> _addr_ev, @string name) {
    ref traceContext ctx = ref _addr_ctx.val;
    ref trace.Event ev = ref _addr_ev.val;

    ctx.emit(ctx.makeSlice(ev, name));
}

private static ptr<traceviewer.Event> makeSlice(this ptr<traceContext> _addr_ctx, ptr<trace.Event> _addr_ev, @string name) {
    ref traceContext ctx = ref _addr_ctx.val;
    ref trace.Event ev = ref _addr_ev.val;
 
    // If ViewerEvent.Dur is not a positive value,
    // trace viewer handles it as a non-terminating time interval.
    // Avoid it by setting the field with a small value.
    var durationUsec = ctx.time(ev.Link) - ctx.time(ev);
    if (ev.Link.Ts - ev.Ts <= 0) {
        durationUsec = 0.0001F; // 0.1 nanoseconds
    }
    ptr<traceviewer.Event> sl = addr(new traceviewer.Event(Name:name,Phase:"X",Time:ctx.time(ev),Dur:durationUsec,TID:ctx.proc(ev),Stack:ctx.stack(ev.Stk),EndStack:ctx.stack(ev.Link.Stk),)); 

    // grey out non-overlapping events if the event is not a global event (ev.G == 0)
    if (ctx.mode & modeTaskOriented != 0 && ev.G != 0) { 
        // include P information.
        {
            var t = ev.Type;

            if (t == trace.EvGoStart || t == trace.EvGoStartLabel) {
                public partial struct Arg {
                    public nint P;
                }
                sl.Arg = addr(new Arg(P:ev.P));

            } 
            // grey out non-overlapping events.

        } 
        // grey out non-overlapping events.
        var overlapping = false;
        foreach (var (_, task) in ctx.tasks) {
            {
                var (_, overlapped) = task.overlappingDuration(ev);

                if (overlapped) {
                    overlapping = true;
                    break;
                }

            }

        }        if (!overlapping) {
            sl.Cname = colorLightGrey;
        }
    }
    return _addr_sl!;

}

private static void emitTask(this ptr<traceContext> _addr_ctx, ptr<taskDesc> _addr_task, nint sortIndex) {
    ref traceContext ctx = ref _addr_ctx.val;
    ref taskDesc task = ref _addr_task.val;

    var taskRow = uint64(task.id);
    var taskName = task.name;
    var durationUsec = float64(task.lastTimestamp() - task.firstTimestamp()) / 1e3F;

    ctx.emitFooter(addr(new traceviewer.Event(Name:"thread_name",Phase:"M",PID:tasksSection,TID:taskRow,Arg:&NameArg{fmt.Sprintf("T%d %s",task.id,taskName)})));
    ctx.emit(addr(new traceviewer.Event(Name:"thread_sort_index",Phase:"M",PID:tasksSection,TID:taskRow,Arg:&SortIndexArg{sortIndex})));
    var ts = float64(task.firstTimestamp()) / 1e3F;
    ptr<traceviewer.Event> sl = addr(new traceviewer.Event(Name:taskName,Phase:"X",Time:ts,Dur:durationUsec,PID:tasksSection,TID:taskRow,Cname:pickTaskColor(task.id),));
    TaskArg targ = new TaskArg(ID:task.id);
    if (task.create != null) {
        sl.Stack = ctx.stack(task.create.Stk);
        targ.StartG = task.create.G;
    }
    if (task.end != null) {
        sl.EndStack = ctx.stack(task.end.Stk);
        targ.EndG = task.end.G;
    }
    sl.Arg = targ;
    ctx.emit(sl);

    if (task.create != null && task.create.Type == trace.EvUserTaskCreate && task.create.Args[1] != 0) {
        ctx.arrowSeq++;
        ctx.emit(addr(new traceviewer.Event(Name:"newTask",Phase:"s",TID:task.create.Args[1],ID:ctx.arrowSeq,Time:ts,PID:tasksSection)));
        ctx.emit(addr(new traceviewer.Event(Name:"newTask",Phase:"t",TID:taskRow,ID:ctx.arrowSeq,Time:ts,PID:tasksSection)));
    }
}

private static void emitRegion(this ptr<traceContext> _addr_ctx, regionDesc s) {
    ref traceContext ctx = ref _addr_ctx.val;

    if (s.Name == "") {
        return ;
    }
    if (!tsWithinRange(s.firstTimestamp(), ctx.startTime, ctx.endTime) && !tsWithinRange(s.lastTimestamp(), ctx.startTime, ctx.endTime)) {
        return ;
    }
    ctx.regionID++;
    var regionID = ctx.regionID;

    var id = s.TaskID;
    var scopeID = fmt.Sprintf("%x", id);
    var name = s.Name;

    ptr<traceviewer.Event> sl0 = addr(new traceviewer.Event(Category:"Region",Name:name,Phase:"b",Time:float64(s.firstTimestamp())/1e3,TID:s.G,ID:uint64(regionID),Scope:scopeID,Cname:pickTaskColor(s.TaskID),));
    if (s.Start != null) {
        sl0.Stack = ctx.stack(s.Start.Stk);
    }
    ctx.emit(sl0);

    ptr<traceviewer.Event> sl1 = addr(new traceviewer.Event(Category:"Region",Name:name,Phase:"e",Time:float64(s.lastTimestamp())/1e3,TID:s.G,ID:uint64(regionID),Scope:scopeID,Cname:pickTaskColor(s.TaskID),Arg:RegionArg{TaskID:s.TaskID},));
    if (s.End != null) {
        sl1.Stack = ctx.stack(s.End.Stk);
    }
    ctx.emit(sl1);

}

private partial struct heapCountersArg {
    public ulong Allocated;
    public ulong NextGC;
}

private static void emitHeapCounters(this ptr<traceContext> _addr_ctx, ptr<trace.Event> _addr_ev) {
    ref traceContext ctx = ref _addr_ctx.val;
    ref trace.Event ev = ref _addr_ev.val;

    if (ctx.prevHeapStats == ctx.heapStats) {
        return ;
    }
    var diff = uint64(0);
    if (ctx.heapStats.nextGC > ctx.heapStats.heapAlloc) {
        diff = ctx.heapStats.nextGC - ctx.heapStats.heapAlloc;
    }
    if (tsWithinRange(ev.Ts, ctx.startTime, ctx.endTime)) {
        ctx.emit(addr(new traceviewer.Event(Name:"Heap",Phase:"C",Time:ctx.time(ev),PID:1,Arg:&heapCountersArg{ctx.heapStats.heapAlloc,diff})));
    }
    ctx.prevHeapStats = ctx.heapStats;

}

private partial struct goroutineCountersArg {
    public ulong Running;
    public ulong Runnable;
    public ulong GCWaiting;
}

private static void emitGoroutineCounters(this ptr<traceContext> _addr_ctx, ptr<trace.Event> _addr_ev) {
    ref traceContext ctx = ref _addr_ctx.val;
    ref trace.Event ev = ref _addr_ev.val;

    if (ctx.prevGstates == ctx.gstates) {
        return ;
    }
    if (tsWithinRange(ev.Ts, ctx.startTime, ctx.endTime)) {
        ctx.emit(addr(new traceviewer.Event(Name:"Goroutines",Phase:"C",Time:ctx.time(ev),PID:1,Arg:&goroutineCountersArg{uint64(ctx.gstates[gRunning]),uint64(ctx.gstates[gRunnable]),uint64(ctx.gstates[gWaitingGC])})));
    }
    ctx.prevGstates = ctx.gstates;

}

private partial struct threadCountersArg {
    public long Running;
    public long InSyscall;
}

private static void emitThreadCounters(this ptr<traceContext> _addr_ctx, ptr<trace.Event> _addr_ev) {
    ref traceContext ctx = ref _addr_ctx.val;
    ref trace.Event ev = ref _addr_ev.val;

    if (ctx.prevThreadStats == ctx.threadStats) {
        return ;
    }
    if (tsWithinRange(ev.Ts, ctx.startTime, ctx.endTime)) {
        ctx.emit(addr(new traceviewer.Event(Name:"Threads",Phase:"C",Time:ctx.time(ev),PID:1,Arg:&threadCountersArg{Running:ctx.threadStats.prunning,InSyscall:ctx.threadStats.insyscall})));
    }
    ctx.prevThreadStats = ctx.threadStats;

}

private static void emitInstant(this ptr<traceContext> _addr_ctx, ptr<trace.Event> _addr_ev, @string name, @string category) {
    ref traceContext ctx = ref _addr_ctx.val;
    ref trace.Event ev = ref _addr_ev.val;

    if (!tsWithinRange(ev.Ts, ctx.startTime, ctx.endTime)) {
        return ;
    }
    @string cname = "";
    if (ctx.mode & modeTaskOriented != 0) {
        var (taskID, isUserAnnotation) = isUserAnnotationEvent(ev);

        var show = false;
        foreach (var (_, task) in ctx.tasks) {
            if (isUserAnnotation && task.id == taskID || task.overlappingInstant(ev)) {
                show = true;
                break;
            }
        }        if (!show) {
            if (isUserAnnotation) {
                return ; // don't display unrelated user annotation events.
            }

            cname = colorLightGrey;

        }
    }
    var arg = default;
    if (ev.Type == trace.EvProcStart) {
        public partial struct Arg {
            public nint P;
        }
        arg = addr(new Arg(ev.Args[0]));

    }
    ctx.emit(addr(new traceviewer.Event(Name:name,Category:category,Phase:"I",Scope:"t",Time:ctx.time(ev),TID:ctx.proc(ev),Stack:ctx.stack(ev.Stk),Cname:cname,Arg:arg)));

}

private static void emitArrow(this ptr<traceContext> _addr_ctx, ptr<trace.Event> _addr_ev, @string name) {
    ref traceContext ctx = ref _addr_ctx.val;
    ref trace.Event ev = ref _addr_ev.val;

    if (ev.Link == null) { 
        // The other end of the arrow is not captured in the trace.
        // For example, a goroutine was unblocked but was not scheduled before trace stop.
        return ;

    }
    if (ctx.mode & modeGoroutineOriented != 0 && (!ctx.gs[ev.Link.G] || ev.Link.Ts < ctx.startTime || ev.Link.Ts > ctx.endTime)) {
        return ;
    }
    if (ev.P == trace.NetpollP || ev.P == trace.TimerP || ev.P == trace.SyscallP) { 
        // Trace-viewer discards arrows if they don't start/end inside of a slice or instant.
        // So emit a fake instant at the start of the arrow.
        ctx.emitInstant(addr(new trace.Event(P:ev.P,Ts:ev.Ts)), "unblock", "");

    }
    @string color = "";
    if (ctx.mode & modeTaskOriented != 0) {
        var overlapping = false; 
        // skip non-overlapping arrows.
        foreach (var (_, task) in ctx.tasks) {
            {
                var (_, overlapped) = task.overlappingDuration(ev);

                if (overlapped) {
                    overlapping = true;
                    break;
                }

            }

        }        if (!overlapping) {
            return ;
        }
    }
    ctx.arrowSeq++;
    ctx.emit(addr(new traceviewer.Event(Name:name,Phase:"s",TID:ctx.proc(ev),ID:ctx.arrowSeq,Time:ctx.time(ev),Stack:ctx.stack(ev.Stk),Cname:color)));
    ctx.emit(addr(new traceviewer.Event(Name:name,Phase:"t",TID:ctx.proc(ev.Link),ID:ctx.arrowSeq,Time:ctx.time(ev.Link),Cname:color)));

}

private static nint stack(this ptr<traceContext> _addr_ctx, slice<ptr<trace.Frame>> stk) {
    ref traceContext ctx = ref _addr_ctx.val;

    return ctx.buildBranch(ctx.frameTree, stk);
}

// buildBranch builds one branch in the prefix tree rooted at ctx.frameTree.
private static nint buildBranch(this ptr<traceContext> _addr_ctx, frameNode parent, slice<ptr<trace.Frame>> stk) {
    ref traceContext ctx = ref _addr_ctx.val;

    if (len(stk) == 0) {
        return parent.id;
    }
    var last = len(stk) - 1;
    var frame = stk[last];
    stk = stk[..(int)last];

    var (node, ok) = parent.children[frame.PC];
    if (!ok) {
        ctx.frameSeq++;
        node.id = ctx.frameSeq;
        node.children = make_map<ulong, frameNode>();
        parent.children[frame.PC] = node;
        ctx.consumer.consumeViewerFrame(strconv.Itoa(node.id), new traceviewer.Frame(Name:fmt.Sprintf("%v:%v",frame.Fn,frame.Line),Parent:parent.id));
    }
    return ctx.buildBranch(node, stk);

}

private static bool isSystemGoroutine(@string entryFn) { 
    // This mimics runtime.isSystemGoroutine as closely as
    // possible.
    return entryFn != "runtime.main" && strings.HasPrefix(entryFn, "runtime.");

}

// firstTimestamp returns the timestamp of the first event record.
private static long firstTimestamp() {
    var (res, _) = parseTrace();
    if (len(res.Events) > 0) {
        return res.Events[0].Ts;
    }
    return 0;

}

// lastTimestamp returns the timestamp of the last event record.
private static long lastTimestamp() {
    var (res, _) = parseTrace();
    {
        var n = len(res.Events);

        if (n > 1) {
            return res.Events[n - 1].Ts;
        }
    }

    return 0;

}

private partial struct jsonWriter {
    public io.Writer w;
    public ptr<json.Encoder> enc;
}

private static traceConsumer viewerDataTraceConsumer(io.Writer w, long start, long end) {
    var frames = make_map<@string, traceviewer.Frame>();
    var enc = json.NewEncoder(w);
    nint written = 0;
    var index = int64(-1);

    io.WriteString(w, "{");
    return new traceConsumer(consumeTimeUnit:func(unitstring){io.WriteString(w,`"displayTimeUnit":`)enc.Encode(unit)io.WriteString(w,",")},consumeViewerEvent:func(v*traceviewer.Event,requiredbool){index++if!required&&(index<start||index>end){return}ifwritten==0{io.WriteString(w,`"traceEvents": [`)}ifwritten>0{io.WriteString(w,",")}enc.Encode(v)written++},consumeViewerFrame:func(kstring,vtraceviewer.Frame){frames[k]=v},flush:func(){io.WriteString(w,`], "stackFrames":`)enc.Encode(frames)io.WriteString(w,`}`)},);
}

// Mapping from more reasonable color names to the reserved color names in
// https://github.com/catapult-project/catapult/blob/master/tracing/tracing/base/color_scheme.html#L50
// The chrome trace viewer allows only those as cname values.
private static readonly @string colorLightMauve = "thread_state_uninterruptible"; // 182, 125, 143
private static readonly @string colorOrange = "thread_state_iowait"; // 255, 140, 0
private static readonly @string colorSeafoamGreen = "thread_state_running"; // 126, 200, 148
private static readonly @string colorVistaBlue = "thread_state_runnable"; // 133, 160, 210
private static readonly @string colorTan = "thread_state_unknown"; // 199, 155, 125
private static readonly @string colorIrisBlue = "background_memory_dump"; // 0, 180, 180
private static readonly @string colorMidnightBlue = "light_memory_dump"; // 0, 0, 180
private static readonly @string colorDeepMagenta = "detailed_memory_dump"; // 180, 0, 180
private static readonly @string colorBlue = "vsync_highlight_color"; // 0, 0, 255
private static readonly @string colorGrey = "generic_work"; // 125, 125, 125
private static readonly @string colorGreen = "good"; // 0, 125, 0
private static readonly @string colorDarkGoldenrod = "bad"; // 180, 125, 0
private static readonly @string colorPeach = "terrible"; // 180, 0, 0
private static readonly @string colorBlack = "black"; // 0, 0, 0
private static readonly @string colorLightGrey = "grey"; // 221, 221, 221
private static readonly @string colorWhite = "white"; // 255, 255, 255
private static readonly @string colorYellow = "yellow"; // 255, 255, 0
private static readonly @string colorOlive = "olive"; // 100, 100, 0
private static readonly @string colorCornflowerBlue = "rail_response"; // 67, 135, 253
private static readonly @string colorSunsetOrange = "rail_animation"; // 244, 74, 63
private static readonly @string colorTangerine = "rail_idle"; // 238, 142, 0
private static readonly @string colorShamrockGreen = "rail_load"; // 13, 168, 97
private static readonly @string colorGreenishYellow = "startup"; // 230, 230, 0
private static readonly @string colorDarkGrey = "heap_dump_stack_frame"; // 128, 128, 128
private static readonly @string colorTawny = "heap_dump_child_node_arrow"; // 204, 102, 0
private static readonly @string colorLemon = "cq_build_running"; // 255, 255, 119
private static readonly @string colorLime = "cq_build_passed"; // 153, 238, 102
private static readonly @string colorPink = "cq_build_failed"; // 238, 136, 136
private static readonly @string colorSilver = "cq_build_abandoned"; // 187, 187, 187
private static readonly @string colorManzGreen = "cq_build_attempt_runnig"; // 222, 222, 75
private static readonly @string colorKellyGreen = "cq_build_attempt_passed"; // 108, 218, 35
private static readonly @string colorAnotherGrey = "cq_build_attempt_failed"; // 187, 187, 187

private static @string colorForTask = new slice<@string>(new @string[] { colorLightMauve, colorOrange, colorSeafoamGreen, colorVistaBlue, colorTan, colorMidnightBlue, colorIrisBlue, colorDeepMagenta, colorGreen, colorDarkGoldenrod, colorPeach, colorOlive, colorCornflowerBlue, colorSunsetOrange, colorTangerine, colorShamrockGreen, colorTawny, colorLemon, colorLime, colorPink, colorSilver, colorManzGreen, colorKellyGreen });

private static @string pickTaskColor(ulong id) {
    var idx = id % uint64(len(colorForTask));
    return colorForTask[idx];
}

} // end main_package
