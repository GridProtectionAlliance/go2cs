// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Serving of pprof-like profiles.

// package main -- go2cs converted at 2020 August 29 10:05:03 UTC
// Original source: C:\Go\src\cmd\trace\pprof.go
using bufio = go.bufio_package;
using fmt = go.fmt_package;
using trace = go.@internal.trace_package;
using io = go.io_package;
using ioutil = go.io.ioutil_package;
using http = go.net.http_package;
using os = go.os_package;
using exec = go.os.exec_package;
using filepath = go.path.filepath_package;
using runtime = go.runtime_package;
using strconv = go.strconv_package;

using profile = go.github.com.google.pprof.profile_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class main_package
    {
        private static @string goCmd()
        {
            @string exeSuffix = default;
            if (runtime.GOOS == "windows")
            {
                exeSuffix = ".exe";
            }
            var path = filepath.Join(runtime.GOROOT(), "bin", "go" + exeSuffix);
            {
                var (_, err) = os.Stat(path);

                if (err == null)
                {
                    return path;
                }
            }
            return "go";
        }

        private static void init()
        {
            http.HandleFunc("/io", serveSVGProfile(pprofIO));
            http.HandleFunc("/block", serveSVGProfile(pprofBlock));
            http.HandleFunc("/syscall", serveSVGProfile(pprofSyscall));
            http.HandleFunc("/sched", serveSVGProfile(pprofSched));
        }

        // Record represents one entry in pprof-like profiles.
        public partial struct Record
        {
            public slice<ref trace.Frame> stk;
            public ulong n;
            public long time;
        }

        // pprofMatchingGoroutines parses the goroutine type id string (i.e. pc)
        // and returns the ids of goroutines of the matching type.
        // If the id string is empty, returns nil without an error.
        private static (map<ulong, bool>, error) pprofMatchingGoroutines(@string id, slice<ref trace.Event> events)
        {
            if (id == "")
            {
                return (null, null);
            }
            var (pc, err) = strconv.ParseUint(id, 10L, 64L); // id is string
            if (err != null)
            {
                return (null, fmt.Errorf("invalid goroutine type: %v", id));
            }
            analyzeGoroutines(events);
            map<ulong, bool> res = default;
            foreach (var (_, g) in gs)
            {
                if (g.PC != pc)
                {
                    continue;
                }
                if (res == null)
                {
                    res = make_map<ulong, bool>();
                }
                res[g.ID] = true;
            }
            if (len(res) == 0L && id != "")
            {
                return (null, fmt.Errorf("failed to find matching goroutines for id: %s", id));
            }
            return (res, null);
        }

        // pprofIO generates IO pprof-like profile (time spent in IO wait,
        // currently only network blocking event).
        private static error pprofIO(io.Writer w, @string id)
        {
            var (events, err) = parseEvents();
            if (err != null)
            {
                return error.As(err);
            }
            var (goroutines, err) = pprofMatchingGoroutines(id, events);
            if (err != null)
            {
                return error.As(err);
            }
            var prof = make_map<ulong, Record>();
            foreach (var (_, ev) in events)
            {
                if (ev.Type != trace.EvGoBlockNet || ev.Link == null || ev.StkID == 0L || len(ev.Stk) == 0L)
                {
                    continue;
                }
                if (goroutines != null && !goroutines[ev.G])
                {
                    continue;
                }
                var rec = prof[ev.StkID];
                rec.stk = ev.Stk;
                rec.n++;
                rec.time += ev.Link.Ts - ev.Ts;
                prof[ev.StkID] = rec;
            }
            return error.As(buildProfile(prof).Write(w));
        }

        // pprofBlock generates blocking pprof-like profile (time spent blocked on synchronization primitives).
        private static error pprofBlock(io.Writer w, @string id)
        {
            var (events, err) = parseEvents();
            if (err != null)
            {
                return error.As(err);
            }
            var (goroutines, err) = pprofMatchingGoroutines(id, events);
            if (err != null)
            {
                return error.As(err);
            }
            var prof = make_map<ulong, Record>();
            foreach (var (_, ev) in events)
            {

                if (ev.Type == trace.EvGoBlockSend || ev.Type == trace.EvGoBlockRecv || ev.Type == trace.EvGoBlockSelect || ev.Type == trace.EvGoBlockSync || ev.Type == trace.EvGoBlockCond || ev.Type == trace.EvGoBlockGC)                 else 
                    continue;
                                if (ev.Link == null || ev.StkID == 0L || len(ev.Stk) == 0L)
                {
                    continue;
                }
                if (goroutines != null && !goroutines[ev.G])
                {
                    continue;
                }
                var rec = prof[ev.StkID];
                rec.stk = ev.Stk;
                rec.n++;
                rec.time += ev.Link.Ts - ev.Ts;
                prof[ev.StkID] = rec;
            }
            return error.As(buildProfile(prof).Write(w));
        }

        // pprofSyscall generates syscall pprof-like profile (time spent blocked in syscalls).
        private static error pprofSyscall(io.Writer w, @string id)
        {
            var (events, err) = parseEvents();
            if (err != null)
            {
                return error.As(err);
            }
            var (goroutines, err) = pprofMatchingGoroutines(id, events);
            if (err != null)
            {
                return error.As(err);
            }
            var prof = make_map<ulong, Record>();
            foreach (var (_, ev) in events)
            {
                if (ev.Type != trace.EvGoSysCall || ev.Link == null || ev.StkID == 0L || len(ev.Stk) == 0L)
                {
                    continue;
                }
                if (goroutines != null && !goroutines[ev.G])
                {
                    continue;
                }
                var rec = prof[ev.StkID];
                rec.stk = ev.Stk;
                rec.n++;
                rec.time += ev.Link.Ts - ev.Ts;
                prof[ev.StkID] = rec;
            }
            return error.As(buildProfile(prof).Write(w));
        }

        // pprofSched generates scheduler latency pprof-like profile
        // (time between a goroutine become runnable and actually scheduled for execution).
        private static error pprofSched(io.Writer w, @string id)
        {
            var (events, err) = parseEvents();
            if (err != null)
            {
                return error.As(err);
            }
            var (goroutines, err) = pprofMatchingGoroutines(id, events);
            if (err != null)
            {
                return error.As(err);
            }
            var prof = make_map<ulong, Record>();
            foreach (var (_, ev) in events)
            {
                if ((ev.Type != trace.EvGoUnblock && ev.Type != trace.EvGoCreate) || ev.Link == null || ev.StkID == 0L || len(ev.Stk) == 0L)
                {
                    continue;
                }
                if (goroutines != null && !goroutines[ev.G])
                {
                    continue;
                }
                var rec = prof[ev.StkID];
                rec.stk = ev.Stk;
                rec.n++;
                rec.time += ev.Link.Ts - ev.Ts;
                prof[ev.StkID] = rec;
            }
            return error.As(buildProfile(prof).Write(w));
        }

        // serveSVGProfile serves pprof-like profile generated by prof as svg.
        private static http.HandlerFunc serveSVGProfile(Func<io.Writer, @string, error> prof) => func((defer, _, __) =>
        {
            return (w, r) =>
            {
                if (r.FormValue("raw") != "")
                {
                    w.Header().Set("Content-Type", "application/octet-stream");
                    {
                        var err__prev2 = err;

                        var err = prof(w, r.FormValue("id"));

                        if (err != null)
                        {
                            w.Header().Set("Content-Type", "text/plain; charset=utf-8");
                            w.Header().Set("X-Go-Pprof", "1");
                            http.Error(w, fmt.Sprintf("failed to get profile: %v", err), http.StatusInternalServerError);
                            return;
                        }

                        err = err__prev2;

                    }
                    return;
                }
                var (blockf, err) = ioutil.TempFile("", "block");
                if (err != null)
                {
                    http.Error(w, fmt.Sprintf("failed to create temp file: %v", err), http.StatusInternalServerError);
                    return;
                }
                defer(() =>
                {
                    blockf.Close();
                    os.Remove(blockf.Name());
                }());
                var blockb = bufio.NewWriter(blockf);
                {
                    var err__prev1 = err;

                    err = prof(blockb, r.FormValue("id"));

                    if (err != null)
                    {
                        http.Error(w, fmt.Sprintf("failed to generate profile: %v", err), http.StatusInternalServerError);
                        return;
                    }

                    err = err__prev1;

                }
                {
                    var err__prev1 = err;

                    err = blockb.Flush();

                    if (err != null)
                    {
                        http.Error(w, fmt.Sprintf("failed to flush temp file: %v", err), http.StatusInternalServerError);
                        return;
                    }

                    err = err__prev1;

                }
                {
                    var err__prev1 = err;

                    err = blockf.Close();

                    if (err != null)
                    {
                        http.Error(w, fmt.Sprintf("failed to close temp file: %v", err), http.StatusInternalServerError);
                        return;
                    }

                    err = err__prev1;

                }
                var svgFilename = blockf.Name() + ".svg";
                {
                    var err__prev1 = err;

                    var (output, err) = exec.Command(goCmd(), "tool", "pprof", "-svg", "-output", svgFilename, blockf.Name()).CombinedOutput();

                    if (err != null)
                    {
                        http.Error(w, fmt.Sprintf("failed to execute go tool pprof: %v\n%s", err, output), http.StatusInternalServerError);
                        return;
                    }

                    err = err__prev1;

                }
                defer(os.Remove(svgFilename));
                w.Header().Set("Content-Type", "image/svg+xml");
                http.ServeFile(w, r, svgFilename);
            }
;
        });

        private static ref profile.Profile buildProfile(map<ulong, Record> prof)
        {
            profile.Profile p = ref new profile.Profile(PeriodType:&profile.ValueType{Type:"trace",Unit:"count"},Period:1,SampleType:[]*profile.ValueType{{Type:"contentions",Unit:"count"},{Type:"delay",Unit:"nanoseconds"},},);
            var locs = make_map<ulong, ref profile.Location>();
            var funcs = make_map<@string, ref profile.Function>();
            foreach (var (_, rec) in prof)
            {
                slice<ref profile.Location> sloc = default;
                foreach (var (_, frame) in rec.stk)
                {
                    var loc = locs[frame.PC];
                    if (loc == null)
                    {
                        var fn = funcs[frame.File + frame.Fn];
                        if (fn == null)
                        {
                            fn = ref new profile.Function(ID:uint64(len(p.Function)+1),Name:frame.Fn,SystemName:frame.Fn,Filename:frame.File,);
                            p.Function = append(p.Function, fn);
                            funcs[frame.File + frame.Fn] = fn;
                        }
                        loc = ref new profile.Location(ID:uint64(len(p.Location)+1),Address:frame.PC,Line:[]profile.Line{profile.Line{Function:fn,Line:int64(frame.Line),},},);
                        p.Location = append(p.Location, loc);
                        locs[frame.PC] = loc;
                    }
                    sloc = append(sloc, loc);
                }
                p.Sample = append(p.Sample, ref new profile.Sample(Value:[]int64{int64(rec.n),rec.time},Location:sloc,));
            }
            return p;
        }
    }
}
