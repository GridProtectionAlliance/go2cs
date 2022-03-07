// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Serving of pprof-like profiles.

// package main -- go2cs converted at 2022 March 06 23:23:07 UTC
// Original source: C:\Program Files\Go\src\cmd\trace\pprof.go
using bufio = go.bufio_package;
using fmt = go.fmt_package;
using exec = go.@internal.execabs_package;
using trace = go.@internal.trace_package;
using io = go.io_package;
using http = go.net.http_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using runtime = go.runtime_package;
using sort = go.sort_package;
using strconv = go.strconv_package;
using time = go.time_package;

using profile = go.github.com.google.pprof.profile_package;
using System;


namespace go;

public static partial class main_package {

private static @string goCmd() {
    @string exeSuffix = default;
    if (runtime.GOOS == "windows") {
        exeSuffix = ".exe";
    }
    var path = filepath.Join(runtime.GOROOT(), "bin", "go" + exeSuffix);
    {
        var (_, err) = os.Stat(path);

        if (err == null) {
            return path;
        }
    }

    return "go";

}

private static void init() {
    http.HandleFunc("/io", serveSVGProfile(pprofByGoroutine(computePprofIO)));
    http.HandleFunc("/block", serveSVGProfile(pprofByGoroutine(computePprofBlock)));
    http.HandleFunc("/syscall", serveSVGProfile(pprofByGoroutine(computePprofSyscall)));
    http.HandleFunc("/sched", serveSVGProfile(pprofByGoroutine(computePprofSched)));

    http.HandleFunc("/regionio", serveSVGProfile(pprofByRegion(computePprofIO)));
    http.HandleFunc("/regionblock", serveSVGProfile(pprofByRegion(computePprofBlock)));
    http.HandleFunc("/regionsyscall", serveSVGProfile(pprofByRegion(computePprofSyscall)));
    http.HandleFunc("/regionsched", serveSVGProfile(pprofByRegion(computePprofSched)));
}

// Record represents one entry in pprof-like profiles.
public partial struct Record {
    public slice<ptr<trace.Frame>> stk;
    public ulong n;
    public long time;
}

// interval represents a time interval in the trace.
private partial struct interval {
    public long begin; // nanoseconds.
    public long end; // nanoseconds.
}

private static Func<io.Writer, ptr<http.Request>, error> pprofByGoroutine(Func<io.Writer, map<ulong, slice<interval>>, slice<ptr<trace.Event>>, error> compute) {
    return (w, r) => {
        var id = r.FormValue("id");
        var (events, err) = parseEvents();
        if (err != null) {
            return err;
        }
        var (gToIntervals, err) = pprofMatchingGoroutines(id, events);
        if (err != null) {
            return err;
        }
        return compute(w, gToIntervals, events);

    };

}

private static Func<io.Writer, ptr<http.Request>, error> pprofByRegion(Func<io.Writer, map<ulong, slice<interval>>, slice<ptr<trace.Event>>, error> compute) {
    return (w, r) => {
        var (filter, err) = newRegionFilter(r);
        if (err != null) {
            return err;
        }
        var (gToIntervals, err) = pprofMatchingRegions(_addr_filter);
        if (err != null) {
            return err;
        }
        var (events, _) = parseEvents();

        return compute(w, gToIntervals, events);

    };

}

// pprofMatchingGoroutines parses the goroutine type id string (i.e. pc)
// and returns the ids of goroutines of the matching type and its interval.
// If the id string is empty, returns nil without an error.
private static (map<ulong, slice<interval>>, error) pprofMatchingGoroutines(@string id, slice<ptr<trace.Event>> events) {
    map<ulong, slice<interval>> _p0 = default;
    error _p0 = default!;

    if (id == "") {
        return (null, error.As(null!)!);
    }
    var (pc, err) = strconv.ParseUint(id, 10, 64); // id is string
    if (err != null) {
        return (null, error.As(fmt.Errorf("invalid goroutine type: %v", id))!);
    }
    analyzeGoroutines(events);
    map<ulong, slice<interval>> res = default;
    foreach (var (_, g) in gs) {
        if (g.PC != pc) {
            continue;
        }
        if (res == null) {
            res = make_map<ulong, slice<interval>>();
        }
        var endTime = g.EndTime;
        if (g.EndTime == 0) {
            endTime = lastTimestamp(); // the trace doesn't include the goroutine end event. Use the trace end time.
        }
        res[g.ID] = new slice<interval>(new interval[] { {begin:g.StartTime,end:endTime} });

    }    if (len(res) == 0 && id != "") {
        return (null, error.As(fmt.Errorf("failed to find matching goroutines for id: %s", id))!);
    }
    return (res, error.As(null!)!);

}

// pprofMatchingRegions returns the time intervals of matching regions
// grouped by the goroutine id. If the filter is nil, returns nil without an error.
private static (map<ulong, slice<interval>>, error) pprofMatchingRegions(ptr<regionFilter> _addr_filter) {
    map<ulong, slice<interval>> _p0 = default;
    error _p0 = default!;
    ref regionFilter filter = ref _addr_filter.val;

    var (res, err) = analyzeAnnotations();
    if (err != null) {
        return (null, error.As(err)!);
    }
    if (filter == null) {
        return (null, error.As(null!)!);
    }
    var gToIntervals = make_map<ulong, slice<interval>>();
    foreach (var (id, regions) in res.regions) {
        foreach (var (_, s) in regions) {
            if (filter.match(id, s)) {
                gToIntervals[s.G] = append(gToIntervals[s.G], new interval(begin:s.firstTimestamp(),end:s.lastTimestamp()));
            }
        }
    }    foreach (var (g, intervals) in gToIntervals) { 
        // in order to remove nested regions and
        // consider only the outermost regions,
        // first, we sort based on the start time
        // and then scan through to select only the outermost regions.
        sort.Slice(intervals, (i, j) => {
            var x = intervals[i].begin;
            var y = intervals[j].begin;
            if (x == y) {
                return intervals[i].end < intervals[j].end;
            }
            return x < y;
        });
        long lastTimestamp = default;
        nint n = default; 
        // select only the outermost regions.
        foreach (var (_, i) in intervals) {
            if (lastTimestamp <= i.begin) {
                intervals[n] = i; // new non-overlapping region starts.
                lastTimestamp = i.end;
                n++;

            } // otherwise, skip because this region overlaps with a previous region.
        }        gToIntervals[g] = intervals[..(int)n];

    }    return (gToIntervals, error.As(null!)!);

}

// computePprofIO generates IO pprof-like profile (time spent in IO wait, currently only network blocking event).
private static error computePprofIO(io.Writer w, map<ulong, slice<interval>> gToIntervals, slice<ptr<trace.Event>> events) {
    var prof = make_map<ulong, Record>();
    foreach (var (_, ev) in events) {
        if (ev.Type != trace.EvGoBlockNet || ev.Link == null || ev.StkID == 0 || len(ev.Stk) == 0) {
            continue;
        }
        var overlapping = pprofOverlappingDuration(gToIntervals, _addr_ev);
        if (overlapping > 0) {
            var rec = prof[ev.StkID];
            rec.stk = ev.Stk;
            rec.n++;
            rec.time += overlapping.Nanoseconds();
            prof[ev.StkID] = rec;
        }
    }    return error.As(buildProfile(prof).Write(w))!;

}

// computePprofBlock generates blocking pprof-like profile (time spent blocked on synchronization primitives).
private static error computePprofBlock(io.Writer w, map<ulong, slice<interval>> gToIntervals, slice<ptr<trace.Event>> events) {
    var prof = make_map<ulong, Record>();
    foreach (var (_, ev) in events) {

        if (ev.Type == trace.EvGoBlockSend || ev.Type == trace.EvGoBlockRecv || ev.Type == trace.EvGoBlockSelect || ev.Type == trace.EvGoBlockSync || ev.Type == trace.EvGoBlockCond || ev.Type == trace.EvGoBlockGC)         else 
            continue;
                if (ev.Link == null || ev.StkID == 0 || len(ev.Stk) == 0) {
            continue;
        }
        var overlapping = pprofOverlappingDuration(gToIntervals, _addr_ev);
        if (overlapping > 0) {
            var rec = prof[ev.StkID];
            rec.stk = ev.Stk;
            rec.n++;
            rec.time += overlapping.Nanoseconds();
            prof[ev.StkID] = rec;
        }
    }    return error.As(buildProfile(prof).Write(w))!;

}

// computePprofSyscall generates syscall pprof-like profile (time spent blocked in syscalls).
private static error computePprofSyscall(io.Writer w, map<ulong, slice<interval>> gToIntervals, slice<ptr<trace.Event>> events) {
    var prof = make_map<ulong, Record>();
    foreach (var (_, ev) in events) {
        if (ev.Type != trace.EvGoSysCall || ev.Link == null || ev.StkID == 0 || len(ev.Stk) == 0) {
            continue;
        }
        var overlapping = pprofOverlappingDuration(gToIntervals, _addr_ev);
        if (overlapping > 0) {
            var rec = prof[ev.StkID];
            rec.stk = ev.Stk;
            rec.n++;
            rec.time += overlapping.Nanoseconds();
            prof[ev.StkID] = rec;
        }
    }    return error.As(buildProfile(prof).Write(w))!;

}

// computePprofSched generates scheduler latency pprof-like profile
// (time between a goroutine become runnable and actually scheduled for execution).
private static error computePprofSched(io.Writer w, map<ulong, slice<interval>> gToIntervals, slice<ptr<trace.Event>> events) {
    var prof = make_map<ulong, Record>();
    foreach (var (_, ev) in events) {
        if ((ev.Type != trace.EvGoUnblock && ev.Type != trace.EvGoCreate) || ev.Link == null || ev.StkID == 0 || len(ev.Stk) == 0) {
            continue;
        }
        var overlapping = pprofOverlappingDuration(gToIntervals, _addr_ev);
        if (overlapping > 0) {
            var rec = prof[ev.StkID];
            rec.stk = ev.Stk;
            rec.n++;
            rec.time += overlapping.Nanoseconds();
            prof[ev.StkID] = rec;
        }
    }    return error.As(buildProfile(prof).Write(w))!;

}

// pprofOverlappingDuration returns the overlapping duration between
// the time intervals in gToIntervals and the specified event.
// If gToIntervals is nil, this simply returns the event's duration.
private static time.Duration pprofOverlappingDuration(map<ulong, slice<interval>> gToIntervals, ptr<trace.Event> _addr_ev) {
    ref trace.Event ev = ref _addr_ev.val;

    if (gToIntervals == null) { // No filtering.
        return time.Duration(ev.Link.Ts - ev.Ts) * time.Nanosecond;

    }
    var intervals = gToIntervals[ev.G];
    if (len(intervals) == 0) {
        return 0;
    }
    time.Duration overlapping = default;
    foreach (var (_, i) in intervals) {
        {
            var o = overlappingDuration(i.begin, i.end, ev.Ts, ev.Link.Ts);

            if (o > 0) {
                overlapping += o;
            }

        }

    }    return overlapping;

}

// serveSVGProfile serves pprof-like profile generated by prof as svg.
private static http.HandlerFunc serveSVGProfile(Func<io.Writer, ptr<http.Request>, error> prof) => func((defer, _, _) => {
    return (w, r) => {
        if (r.FormValue("raw") != "") {
            w.Header().Set("Content-Type", "application/octet-stream");
            {
                var err__prev2 = err;

                var err = prof(w, r);

                if (err != null) {
                    w.Header().Set("Content-Type", "text/plain; charset=utf-8");
                    w.Header().Set("X-Go-Pprof", "1");
                    http.Error(w, fmt.Sprintf("failed to get profile: %v", err), http.StatusInternalServerError);
                    return ;
                }

                err = err__prev2;

            }

            return ;

        }
        var (blockf, err) = os.CreateTemp("", "block");
        if (err != null) {
            http.Error(w, fmt.Sprintf("failed to create temp file: %v", err), http.StatusInternalServerError);
            return ;
        }
        defer(() => {
            blockf.Close();
            os.Remove(blockf.Name());
        }());
        var blockb = bufio.NewWriter(blockf);
        {
            var err__prev1 = err;

            err = prof(blockb, r);

            if (err != null) {
                http.Error(w, fmt.Sprintf("failed to generate profile: %v", err), http.StatusInternalServerError);
                return ;
            }

            err = err__prev1;

        }

        {
            var err__prev1 = err;

            err = blockb.Flush();

            if (err != null) {
                http.Error(w, fmt.Sprintf("failed to flush temp file: %v", err), http.StatusInternalServerError);
                return ;
            }

            err = err__prev1;

        }

        {
            var err__prev1 = err;

            err = blockf.Close();

            if (err != null) {
                http.Error(w, fmt.Sprintf("failed to close temp file: %v", err), http.StatusInternalServerError);
                return ;
            }

            err = err__prev1;

        }

        var svgFilename = blockf.Name() + ".svg";
        {
            var err__prev1 = err;

            var (output, err) = exec.Command(goCmd(), "tool", "pprof", "-svg", "-output", svgFilename, blockf.Name()).CombinedOutput();

            if (err != null) {
                http.Error(w, fmt.Sprintf("failed to execute go tool pprof: %v\n%s", err, output), http.StatusInternalServerError);
                return ;
            }

            err = err__prev1;

        }

        defer(os.Remove(svgFilename));
        w.Header().Set("Content-Type", "image/svg+xml");
        http.ServeFile(w, r, svgFilename);

    };

});

private static ptr<profile.Profile> buildProfile(map<ulong, Record> prof) {
    ptr<profile.Profile> p = addr(new profile.Profile(PeriodType:&profile.ValueType{Type:"trace",Unit:"count"},Period:1,SampleType:[]*profile.ValueType{{Type:"contentions",Unit:"count"},{Type:"delay",Unit:"nanoseconds"},},));
    var locs = make_map<ulong, ptr<profile.Location>>();
    var funcs = make_map<@string, ptr<profile.Function>>();
    foreach (var (_, rec) in prof) {
        slice<ptr<profile.Location>> sloc = default;
        foreach (var (_, frame) in rec.stk) {
            var loc = locs[frame.PC];
            if (loc == null) {
                var fn = funcs[frame.File + frame.Fn];
                if (fn == null) {
                    fn = addr(new profile.Function(ID:uint64(len(p.Function)+1),Name:frame.Fn,SystemName:frame.Fn,Filename:frame.File,));
                    p.Function = append(p.Function, fn);
                    funcs[frame.File + frame.Fn] = fn;
                }
                loc = addr(new profile.Location(ID:uint64(len(p.Location)+1),Address:frame.PC,Line:[]profile.Line{{Function:fn,Line:int64(frame.Line),},},));
                p.Location = append(p.Location, loc);
                locs[frame.PC] = loc;
            }
            sloc = append(sloc, loc);
        }        p.Sample = append(p.Sample, addr(new profile.Sample(Value:[]int64{int64(rec.n),rec.time},Location:sloc,)));
    }    return _addr_p!;
}

} // end main_package
