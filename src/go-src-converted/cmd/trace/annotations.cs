// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2020 October 08 04:42:21 UTC
// Original source: C:\Go\src\cmd\trace\annotations.go
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using template = go.html.template_package;
using trace = go.@internal.trace_package;
using log = go.log_package;
using math = go.math_package;
using http = go.net.http_package;
using url = go.net.url_package;
using reflect = go.reflect_package;
using sort = go.sort_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using time = go.time_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class main_package
    {
        private static void init()
        {
            http.HandleFunc("/usertasks", httpUserTasks);
            http.HandleFunc("/usertask", httpUserTask);
            http.HandleFunc("/userregions", httpUserRegions);
            http.HandleFunc("/userregion", httpUserRegion);
        }

        // httpUserTasks reports all tasks found in the trace.
        private static void httpUserTasks(http.ResponseWriter w, ptr<http.Request> _addr_r)
        {
            ref http.Request r = ref _addr_r.val;

            var (res, err) = analyzeAnnotations();
            if (err != null)
            {
                http.Error(w, err.Error(), http.StatusInternalServerError);
                return ;
            }

            var tasks = res.tasks;
            var summary = make_map<@string, taskStats>();
            foreach (var (_, task) in tasks)
            {
                var (stats, ok) = summary[task.name];
                if (!ok)
                {
                    stats.Type = task.name;
                }

                stats.add(task);
                summary[task.name] = stats;

            } 

            // Sort tasks by type.
            var userTasks = make_slice<taskStats>(0L, len(summary));
            {
                var stats__prev1 = stats;

                foreach (var (_, __stats) in summary)
                {
                    stats = __stats;
                    userTasks = append(userTasks, stats);
                }

                stats = stats__prev1;
            }

            sort.Slice(userTasks, (i, j) =>
            {
                return userTasks[i].Type < userTasks[j].Type;
            }); 

            // Emit table.
            err = templUserTaskTypes.Execute(w, userTasks);
            if (err != null)
            {
                http.Error(w, fmt.Sprintf("failed to execute template: %v", err), http.StatusInternalServerError);
                return ;
            }

        }

        private static void httpUserRegions(http.ResponseWriter w, ptr<http.Request> _addr_r)
        {
            ref http.Request r = ref _addr_r.val;

            var (res, err) = analyzeAnnotations();
            if (err != null)
            {
                http.Error(w, err.Error(), http.StatusInternalServerError);
                return ;
            }

            var allRegions = res.regions;

            var summary = make_map<regionTypeID, regionStats>();
            foreach (var (id, regions) in allRegions)
            {
                var (stats, ok) = summary[id];
                if (!ok)
                {
                    stats.regionTypeID = id;
                }

                foreach (var (_, s) in regions)
                {
                    stats.add(s);
                }
                summary[id] = stats;

            } 
            // Sort regions by pc and name
            var userRegions = make_slice<regionStats>(0L, len(summary));
            {
                var stats__prev1 = stats;

                foreach (var (_, __stats) in summary)
                {
                    stats = __stats;
                    userRegions = append(userRegions, stats);
                }

                stats = stats__prev1;
            }

            sort.Slice(userRegions, (i, j) =>
            {
                if (userRegions[i].Type != userRegions[j].Type)
                {
                    return userRegions[i].Type < userRegions[j].Type;
                }

                return userRegions[i].Frame.PC < userRegions[j].Frame.PC;

            }); 
            // Emit table.
            err = templUserRegionTypes.Execute(w, userRegions);
            if (err != null)
            {
                http.Error(w, fmt.Sprintf("failed to execute template: %v", err), http.StatusInternalServerError);
                return ;
            }

        }

        private static void httpUserRegion(http.ResponseWriter w, ptr<http.Request> _addr_r)
        {
            ref http.Request r = ref _addr_r.val;

            var (filter, err) = newRegionFilter(_addr_r);
            if (err != null)
            {
                http.Error(w, err.Error(), http.StatusBadRequest);
                return ;
            }

            var (res, err) = analyzeAnnotations();
            if (err != null)
            {
                http.Error(w, err.Error(), http.StatusInternalServerError);
                return ;
            }

            var allRegions = res.regions;

            slice<regionDesc> data = default;

            long maxTotal = default;
            foreach (var (id, regions) in allRegions)
            {
                foreach (var (_, s) in regions)
                {
                    if (!filter.match(id, s))
                    {
                        continue;
                    }

                    data = append(data, s);
                    if (maxTotal < s.TotalTime)
                    {
                        maxTotal = s.TotalTime;
                    }

                }

            }
            var sortby = r.FormValue("sortby");
            var (_, ok) = reflect.TypeOf(new regionDesc()).FieldByNameFunc(s =>
            {
                return s == sortby;
            });
            if (!ok)
            {
                sortby = "TotalTime";
            }

            sort.Slice(data, (i, j) =>
            {
                var ival = reflect.ValueOf(data[i]).FieldByName(sortby).Int();
                var jval = reflect.ValueOf(data[j]).FieldByName(sortby).Int();
                return ival > jval;
            });

            err = templUserRegionType.Execute(w, /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{MaxTotalint64Data[]regionDescNamestringFilter*regionFilter}{MaxTotal:maxTotal,Data:data,Name:filter.name,Filter:filter,});
            if (err != null)
            {
                http.Error(w, fmt.Sprintf("failed to execute template: %v", err), http.StatusInternalServerError);
                return ;
            }

        }

        // httpUserTask presents the details of the selected tasks.
        private static void httpUserTask(http.ResponseWriter w, ptr<http.Request> _addr_r)
        {
            ref http.Request r = ref _addr_r.val;

            var (filter, err) = newTaskFilter(_addr_r);
            if (err != null)
            {
                http.Error(w, err.Error(), http.StatusBadRequest);
                return ;
            }

            var (res, err) = analyzeAnnotations();
            if (err != null)
            {
                http.Error(w, err.Error(), http.StatusInternalServerError);
                return ;
            }

            var tasks = res.tasks;

            private partial struct @event
            {
                public @string WhenString;
                public time.Duration Elapsed;
                public ulong Go;
                public @string What; // TODO: include stack trace of creation time
            }
            private partial struct entry
            {
                public @string WhenString;
                public ulong ID;
                public time.Duration Duration;
                public bool Complete;
                public slice<event> Events;
                public time.Duration Start; // Time since the beginning of the trace
                public time.Duration End; // Time since the beginning of the trace
                public time.Duration GCTime;
            }

            var @base = time.Duration(firstTimestamp()) * time.Nanosecond; // trace start

            slice<entry> data = default;

            foreach (var (_, task) in tasks)
            {
                if (!filter.match(task))
                {
                    continue;
                } 
                // merge events in the task.events and task.regions.Start
                var rawEvents = append(new slice<ptr<trace.Event>>(new ptr<trace.Event>[] {  }), task.events);
                foreach (var (_, s) in task.regions)
                {
                    if (s.Start != null)
                    {
                        rawEvents = append(rawEvents, s.Start);
                    }

                }
                sort.SliceStable(rawEvents, (i, j) => rawEvents[i].Ts < rawEvents[j].Ts);

                slice<event> events = default;
                time.Duration last = default;
                foreach (var (i, ev) in rawEvents)
                {
                    var when = time.Duration(ev.Ts) * time.Nanosecond - base;
                    var elapsed = time.Duration(ev.Ts) * time.Nanosecond - last;
                    if (i == 0L)
                    {
                        elapsed = 0L;
                    }

                    var what = describeEvent(_addr_ev);
                    if (what != "")
                    {
                        events = append(events, new event(WhenString:fmt.Sprintf("%2.9f",when.Seconds()),Elapsed:elapsed,What:what,Go:ev.G,));
                        last = time.Duration(ev.Ts) * time.Nanosecond;
                    }

                }
                data = append(data, new entry(WhenString:fmt.Sprintf("%2.9fs",(time.Duration(task.firstTimestamp())*time.Nanosecond-base).Seconds()),Duration:task.duration(),ID:task.id,Complete:task.complete(),Events:events,Start:time.Duration(task.firstTimestamp())*time.Nanosecond,End:time.Duration(task.endTimestamp())*time.Nanosecond,GCTime:task.overlappingGCDuration(res.gcEvents),));

            }
            sort.Slice(data, (i, j) =>
            {
                return data[i].Duration < data[j].Duration;
            }); 

            // Emit table.
            err = templUserTaskType.Execute(w, /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{NamestringEntry[]entry}{Name:filter.name,Entry:data,});
            if (err != null)
            {
                log.Printf("failed to execute template: %v", err);
                http.Error(w, fmt.Sprintf("failed to execute template: %v", err), http.StatusInternalServerError);
                return ;
            }

        }

        private partial struct annotationAnalysisResult
        {
            public map<ulong, ptr<taskDesc>> tasks; // tasks
            public map<regionTypeID, slice<regionDesc>> regions; // regions
            public slice<ptr<trace.Event>> gcEvents; // GCStartevents, sorted
        }

        private partial struct regionTypeID
        {
            public trace.Frame Frame; // top frame
            public @string Type;
        }

        // analyzeAnnotations analyzes user annotation events and
        // returns the task descriptors keyed by internal task id.
        private static (annotationAnalysisResult, error) analyzeAnnotations()
        {
            annotationAnalysisResult _p0 = default;
            error _p0 = default!;

            var (res, err) = parseTrace();
            if (err != null)
            {
                return (new annotationAnalysisResult(), error.As(fmt.Errorf("failed to parse trace: %v", err))!);
            }

            var events = res.Events;
            if (len(events) == 0L)
            {
                return (new annotationAnalysisResult(), error.As(fmt.Errorf("empty trace"))!);
            }

            allTasks tasks = new allTasks();
            map regions = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<regionTypeID, slice<regionDesc>>{};
            slice<ptr<trace.Event>> gcEvents = default;

            foreach (var (_, ev) in events)
            {
                {
                    var typ = ev.Type;


                    if (typ == trace.EvUserTaskCreate || typ == trace.EvUserTaskEnd || typ == trace.EvUserLog) 
                        var taskid = ev.Args[0L];
                        var task = tasks.task(taskid);
                        task.addEvent(ev); 

                        // retrieve parent task information
                        if (typ == trace.EvUserTaskCreate)
                        {
                            {
                                var parentID = ev.Args[1L];

                                if (parentID != 0L)
                                {
                                    var parentTask = tasks.task(parentID);
                                    task.parent = parentTask;
                                    if (parentTask != null)
                                    {
                                        parentTask.children = append(parentTask.children, task);
                                    }

                                }

                            }

                        }

                    else if (typ == trace.EvGCStart) 
                        gcEvents = append(gcEvents, ev);

                }

            } 
            // combine region info.
            analyzeGoroutines(events);
            foreach (var (goid, stats) in gs)
            { 
                // gs is a global var defined in goroutines.go as a result
                // of analyzeGoroutines. TODO(hyangah): fix this not to depend
                // on a 'global' var.
                foreach (var (_, s) in stats.Regions)
                {
                    if (s.TaskID != 0L)
                    {
                        task = tasks.task(s.TaskID);
                        task.goroutines[goid] = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{};
                        task.regions = append(task.regions, new regionDesc(UserRegionDesc:s,G:goid));
                    }

                    trace.Frame frame = default;
                    if (s.Start != null)
                    {
                        frame = s.Start.Stk[0L].val;
                    }

                    regionTypeID id = new regionTypeID(Frame:frame,Type:s.Name);
                    regions[id] = append(regions[id], new regionDesc(UserRegionDesc:s,G:goid));

                }

            } 

            // sort regions in tasks based on the timestamps.
            {
                var task__prev1 = task;

                foreach (var (_, __task) in tasks)
                {
                    task = __task;
                    sort.SliceStable(task.regions, (i, j) =>
                    {
                        var si = task.regions[i].firstTimestamp();
                        var sj = task.regions[j].firstTimestamp();
                        if (si != sj)
                        {
                            return si < sj;
                        }

                        return task.regions[i].lastTimestamp() < task.regions[j].lastTimestamp();

                    });

                }

                task = task__prev1;
            }

            return (new annotationAnalysisResult(tasks:tasks,regions:regions,gcEvents:gcEvents), error.As(null!)!);

        }

        // taskDesc represents a task.
        private partial struct taskDesc
        {
            public @string name; // user-provided task name
            public ulong id; // internal task id
            public slice<ptr<trace.Event>> events; // sorted based on timestamp.
            public slice<regionDesc> regions; // associated regions, sorted based on the start timestamp and then the last timestamp.
            public ptr<trace.Event> create; // Task create event
            public ptr<trace.Event> end; // Task end event

            public ptr<taskDesc> parent;
            public slice<ptr<taskDesc>> children;
        }

        private static ptr<taskDesc> newTaskDesc(ulong id)
        {
            return addr(new taskDesc(id:id,goroutines:make(map[uint64]struct{}),));
        }

        private static @string String(this ptr<taskDesc> _addr_task)
        {
            ref taskDesc task = ref _addr_task.val;

            if (task == null)
            {
                return "task <nil>";
            }

            ptr<object> wb = @new<bytes.Buffer>();
            fmt.Fprintf(wb, "task %d:\t%s\n", task.id, task.name);
            fmt.Fprintf(wb, "\tstart: %v end: %v complete: %t\n", task.firstTimestamp(), task.endTimestamp(), task.complete());
            fmt.Fprintf(wb, "\t%d goroutines\n", len(task.goroutines));
            fmt.Fprintf(wb, "\t%d regions:\n", len(task.regions));
            foreach (var (_, s) in task.regions)
            {
                fmt.Fprintf(wb, "\t\t%s(goid=%d)\n", s.Name, s.G);
            }
            if (task.parent != null)
            {
                fmt.Fprintf(wb, "\tparent: %s\n", task.parent.name);
            }

            fmt.Fprintf(wb, "\t%d children:\n", len(task.children));
            foreach (var (_, c) in task.children)
            {
                fmt.Fprintf(wb, "\t\t%s\n", c.name);
            }
            return wb.String();

        }

        // regionDesc represents a region.
        private partial struct regionDesc
        {
            public ref ptr<trace.UserRegionDesc> UserRegionDesc> => ref UserRegionDesc>_ptr;
            public ulong G; // id of goroutine where the region was defined
        }

        private partial struct allTasks // : map<ulong, ptr<taskDesc>>
        {
        }

        private static ptr<taskDesc> task(this allTasks tasks, ulong taskID)
        {
            if (taskID == 0L)
            {
                return _addr_null!; // notask
            }

            var (t, ok) = tasks[taskID];
            if (ok)
            {
                return _addr_t!;
            }

            t = addr(new taskDesc(id:taskID,goroutines:make(map[uint64]struct{}),));
            tasks[taskID] = t;
            return _addr_t!;

        }

        private static void addEvent(this ptr<taskDesc> _addr_task, ptr<trace.Event> _addr_ev)
        {
            ref taskDesc task = ref _addr_task.val;
            ref trace.Event ev = ref _addr_ev.val;

            if (task == null)
            {
                return ;
            }

            task.events = append(task.events, ev);
            task.goroutines[ev.G] = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{};

            {
                var typ = ev.Type;


                if (typ == trace.EvUserTaskCreate) 
                    task.name = ev.SArgs[0L];
                    task.create = ev;
                else if (typ == trace.EvUserTaskEnd) 
                    task.end = ev;

            }

        }

        // complete is true only if both start and end events of this task
        // are present in the trace.
        private static bool complete(this ptr<taskDesc> _addr_task)
        {
            ref taskDesc task = ref _addr_task.val;

            if (task == null)
            {
                return false;
            }

            return task.create != null && task.end != null;

        }

        // descendants returns all the task nodes in the subtree rooted from this task.
        private static slice<ptr<taskDesc>> descendants(this ptr<taskDesc> _addr_task)
        {
            ref taskDesc task = ref _addr_task.val;

            if (task == null)
            {
                return null;
            }

            ptr<taskDesc> res = new slice<ptr<taskDesc>>(new ptr<taskDesc>[] { task });
            for (long i = 0L; len(res[i..]) > 0L; i++)
            {
                var t = res[i];
                foreach (var (_, c) in t.children)
                {
                    res = append(res, c);
                }

            }

            return res;

        }

        // firstTimestamp returns the first timestamp of this task found in
        // this trace. If the trace does not contain the task creation event,
        // the first timestamp of the trace will be returned.
        private static long firstTimestamp(this ptr<taskDesc> _addr_task)
        {
            ref taskDesc task = ref _addr_task.val;

            if (task != null && task.create != null)
            {
                return task.create.Ts;
            }

            return firstTimestamp();

        }

        // lastTimestamp returns the last timestamp of this task in this
        // trace. If the trace does not contain the task end event, the last
        // timestamp of the trace will be returned.
        private static long lastTimestamp(this ptr<taskDesc> _addr_task)
        {
            ref taskDesc task = ref _addr_task.val;

            var endTs = task.endTimestamp();
            {
                var last = task.lastEvent();

                if (last != null && last.Ts > endTs)
                {
                    return last.Ts;
                }

            }

            return endTs;

        }

        // endTimestamp returns the timestamp of this task's end event.
        // If the trace does not contain the task end event, the last
        // timestamp of the trace will be returned.
        private static long endTimestamp(this ptr<taskDesc> _addr_task)
        {
            ref taskDesc task = ref _addr_task.val;

            if (task != null && task.end != null)
            {
                return task.end.Ts;
            }

            return lastTimestamp();

        }

        private static time.Duration duration(this ptr<taskDesc> _addr_task)
        {
            ref taskDesc task = ref _addr_task.val;

            return time.Duration(task.endTimestamp() - task.firstTimestamp()) * time.Nanosecond;
        }

        private static time.Duration duration(this ptr<regionDesc> _addr_region)
        {
            ref regionDesc region = ref _addr_region.val;

            return time.Duration(region.lastTimestamp() - region.firstTimestamp()) * time.Nanosecond;
        }

        // overlappingGCDuration returns the sum of GC period overlapping with the task's lifetime.
        private static time.Duration overlappingGCDuration(this ptr<taskDesc> _addr_task, slice<ptr<trace.Event>> evs)
        {
            time.Duration overlapping = default;
            ref taskDesc task = ref _addr_task.val;

            foreach (var (_, ev) in evs)
            { 
                // make sure we only consider the global GC events.
                {
                    var typ = ev.Type;

                    if (typ != trace.EvGCStart && typ != trace.EvGCSTWStart)
                    {
                        continue;
                    }

                }


                {
                    var (o, overlapped) = task.overlappingDuration(ev);

                    if (overlapped)
                    {
                        overlapping += o;
                    }

                }

            }
            return overlapping;

        }

        // overlappingInstant reports whether the instantaneous event, ev, occurred during
        // any of the task's region if ev is a goroutine-local event, or overlaps with the
        // task's lifetime if ev is a global event.
        private static bool overlappingInstant(this ptr<taskDesc> _addr_task, ptr<trace.Event> _addr_ev)
        {
            ref taskDesc task = ref _addr_task.val;
            ref trace.Event ev = ref _addr_ev.val;

            {
                var (_, ok) = isUserAnnotationEvent(_addr_ev);

                if (ok && task.id != ev.Args[0L])
                {
                    return false; // not this task's user event.
                }

            }


            var ts = ev.Ts;
            var taskStart = task.firstTimestamp();
            var taskEnd = task.endTimestamp();
            if (ts < taskStart || taskEnd < ts)
            {
                return false;
            }

            if (ev.P == trace.GCP)
            {
                return true;
            } 

            // Goroutine local event. Check whether there are regions overlapping with the event.
            var goid = ev.G;
            foreach (var (_, region) in task.regions)
            {
                if (region.G != goid)
                {
                    continue;
                }

                if (region.firstTimestamp() <= ts && ts <= region.lastTimestamp())
                {
                    return true;
                }

            }
            return false;

        }

        // overlappingDuration reports whether the durational event, ev, overlaps with
        // any of the task's region if ev is a goroutine-local event, or overlaps with
        // the task's lifetime if ev is a global event. It returns the overlapping time
        // as well.
        private static (time.Duration, bool) overlappingDuration(this ptr<taskDesc> _addr_task, ptr<trace.Event> _addr_ev)
        {
            time.Duration _p0 = default;
            bool _p0 = default;
            ref taskDesc task = ref _addr_task.val;
            ref trace.Event ev = ref _addr_ev.val;

            var start = ev.Ts;
            var end = lastTimestamp();
            if (ev.Link != null)
            {
                end = ev.Link.Ts;
            }

            if (start > end)
            {
                return (0L, false);
            }

            var goid = ev.G;
            var goid2 = ev.G;
            if (ev.Link != null)
            {
                goid2 = ev.Link.G;
            } 

            // This event is a global GC event
            if (ev.P == trace.GCP)
            {
                var taskStart = task.firstTimestamp();
                var taskEnd = task.endTimestamp();
                var o = overlappingDuration(taskStart, taskEnd, start, end);
                return (o, o > 0L);
            } 

            // Goroutine local event. Check whether there are regions overlapping with the event.
            time.Duration overlapping = default;
            long lastRegionEnd = default; // the end of previous overlapping region
            foreach (var (_, region) in task.regions)
            {
                if (region.G != goid && region.G != goid2)
                {
                    continue;
                }

                var regionStart = region.firstTimestamp();
                var regionEnd = region.lastTimestamp();
                if (regionStart < lastRegionEnd)
                { // skip nested regions
                    continue;

                }

                {
                    var o__prev1 = o;

                    o = overlappingDuration(regionStart, regionEnd, start, end);

                    if (o > 0L)
                    { 
                        // overlapping.
                        lastRegionEnd = regionEnd;
                        overlapping += o;

                    }

                    o = o__prev1;

                }

            }
            return (overlapping, overlapping > 0L);

        }

        // overlappingDuration returns the overlapping time duration between
        // two time intervals [start1, end1] and [start2, end2] where
        // start, end parameters are all int64 representing nanoseconds.
        private static time.Duration overlappingDuration(long start1, long end1, long start2, long end2)
        { 
            // assume start1 <= end1 and start2 <= end2
            if (end1 < start2 || end2 < start1)
            {
                return 0L;
            }

            if (start1 < start2)
            { // choose the later one
                start1 = start2;

            }

            if (end1 > end2)
            { // choose the earlier one
                end1 = end2;

            }

            return time.Duration(end1 - start1);

        }

        private static ptr<trace.Event> lastEvent(this ptr<taskDesc> _addr_task)
        {
            ref taskDesc task = ref _addr_task.val;

            if (task == null)
            {
                return _addr_null!;
            }

            {
                var n = len(task.events);

                if (n > 0L)
                {
                    return _addr_task.events[n - 1L]!;
                }

            }

            return _addr_null!;

        }

        // firstTimestamp returns the timestamp of region start event.
        // If the region's start event is not present in the trace,
        // the first timestamp of the trace will be returned.
        private static long firstTimestamp(this ptr<regionDesc> _addr_region)
        {
            ref regionDesc region = ref _addr_region.val;

            if (region.Start != null)
            {
                return region.Start.Ts;
            }

            return firstTimestamp();

        }

        // lastTimestamp returns the timestamp of region end event.
        // If the region's end event is not present in the trace,
        // the last timestamp of the trace will be returned.
        private static long lastTimestamp(this ptr<regionDesc> _addr_region)
        {
            ref regionDesc region = ref _addr_region.val;

            if (region.End != null)
            {
                return region.End.Ts;
            }

            return lastTimestamp();

        }

        // RelatedGoroutines returns IDs of goroutines related to the task. A goroutine
        // is related to the task if user annotation activities for the task occurred.
        // If non-zero depth is provided, this searches all events with BFS and includes
        // goroutines unblocked any of related goroutines to the result.
        private static map<ulong, bool> RelatedGoroutines(this ptr<taskDesc> _addr_task, slice<ptr<trace.Event>> events, long depth)
        {
            ref taskDesc task = ref _addr_task.val;

            var start = task.firstTimestamp();
            var end = task.endTimestamp();

            map gmap = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ulong, bool>{};
            foreach (var (k) in task.goroutines)
            {
                gmap[k] = true;
            }
            for (long i = 0L; i < depth; i++)
            {
                var gmap1 = make_map<ulong, bool>();
                foreach (var (g) in gmap)
                {
                    gmap1[g] = true;
                }
                foreach (var (_, ev) in events)
                {
                    if (ev.Ts < start || ev.Ts > end)
                    {
                        continue;
                    }

                    if (ev.Type == trace.EvGoUnblock && gmap[ev.Args[0L]])
                    {
                        gmap1[ev.G] = true;
                    }

                    gmap = gmap1;

                }

            }

            gmap[0L] = true; // for GC events (goroutine id = 0)
            return gmap;

        }

        private partial struct taskFilter
        {
            public @string name;
            public slice<Func<ptr<taskDesc>, bool>> cond;
        }

        private static bool match(this ptr<taskFilter> _addr_f, ptr<taskDesc> _addr_t)
        {
            ref taskFilter f = ref _addr_f.val;
            ref taskDesc t = ref _addr_t.val;

            if (t == null)
            {
                return false;
            }

            foreach (var (_, c) in f.cond)
            {
                if (!c(t))
                {
                    return false;
                }

            }
            return true;

        }

        private static (ptr<taskFilter>, error) newTaskFilter(ptr<http.Request> _addr_r)
        {
            ptr<taskFilter> _p0 = default!;
            error _p0 = default!;
            ref http.Request r = ref _addr_r.val;

            {
                var err = r.ParseForm();

                if (err != null)
                {
                    return (_addr_null!, error.As(err)!);
                }

            }


            slice<@string> name = default;
            slice<Func<ptr<taskDesc>, bool>> conditions = default;

            var param = r.Form;
            {
                var (typ, ok) = param["type"];

                if (ok && len(typ) > 0L)
                {
                    name = append(name, "type=" + typ[0L]);
                    conditions = append(conditions, t =>
                    {
                        return _addr_t.name == typ[0L]!;
                    });

                }

            }

            {
                var complete = r.FormValue("complete");

                if (complete == "1")
                {
                    name = append(name, "complete");
                    conditions = append(conditions, t =>
                    {
                        return _addr_t.complete()!;
                    });

                }
                else if (complete == "0")
                {
                    name = append(name, "incomplete");
                    conditions = append(conditions, t =>
                    {
                        return _addr_!t.complete()!;
                    });

                }


            }

            {
                var lat__prev1 = lat;

                var (lat, err) = time.ParseDuration(r.FormValue("latmin"));

                if (err == null)
                {
                    name = append(name, fmt.Sprintf("latency >= %s", lat));
                    conditions = append(conditions, t =>
                    {
                        return _addr_t.complete() && t.duration() >= lat!;
                    });

                }

                lat = lat__prev1;

            }

            {
                var lat__prev1 = lat;

                (lat, err) = time.ParseDuration(r.FormValue("latmax"));

                if (err == null)
                {
                    name = append(name, fmt.Sprintf("latency <= %s", lat));
                    conditions = append(conditions, t =>
                    {
                        return _addr_t.complete() && t.duration() <= lat!;
                    });

                }

                lat = lat__prev1;

            }

            {
                var text = r.FormValue("logtext");

                if (text != "")
                {
                    name = append(name, fmt.Sprintf("log contains %q", text));
                    conditions = append(conditions, t =>
                    {
                        return _addr_taskMatches(_addr_t, text)!;
                    });

                }

            }


            return (addr(new taskFilter(name:strings.Join(name,","),cond:conditions)), error.As(null!)!);

        }

        private static bool taskMatches(ptr<taskDesc> _addr_t, @string text)
        {
            ref taskDesc t = ref _addr_t.val;

            foreach (var (_, ev) in t.events)
            {

                if (ev.Type == trace.EvUserTaskCreate || ev.Type == trace.EvUserRegion || ev.Type == trace.EvUserLog) 
                    foreach (var (_, s) in ev.SArgs)
                    {
                        if (strings.Contains(s, text))
                        {
                            return true;
                        }

                    }
                
            }
            return false;

        }

        private partial struct regionFilter
        {
            public @string name;
            public url.Values @params;
            public slice<Func<regionTypeID, regionDesc, bool>> cond;
        }

        private static bool match(this ptr<regionFilter> _addr_f, regionTypeID id, regionDesc s)
        {
            ref regionFilter f = ref _addr_f.val;

            foreach (var (_, c) in f.cond)
            {
                if (!c(id, s))
                {
                    return false;
                }

            }
            return true;

        }

        private static (ptr<regionFilter>, error) newRegionFilter(ptr<http.Request> _addr_r)
        {
            ptr<regionFilter> _p0 = default!;
            error _p0 = default!;
            ref http.Request r = ref _addr_r.val;

            {
                var err = r.ParseForm();

                if (err != null)
                {
                    return (_addr_null!, error.As(err)!);
                }

            }


            slice<@string> name = default;
            slice<Func<regionTypeID, regionDesc, bool>> conditions = default;
            var filterParams = make(url.Values);

            var param = r.Form;
            {
                var (typ, ok) = param["type"];

                if (ok && len(typ) > 0L)
                {
                    name = append(name, "type=" + typ[0L]);
                    conditions = append(conditions, (id, s) =>
                    {
                        return _addr_id.Type == typ[0L]!;
                    });
                    filterParams.Add("type", typ[0L]);

                }

            }

            {
                var (pc, err) = strconv.ParseUint(r.FormValue("pc"), 16L, 64L);

                if (err == null)
                {
                    var encPC = fmt.Sprintf("%x", pc);
                    name = append(name, "pc=" + encPC);
                    conditions = append(conditions, (id, s) =>
                    {
                        return _addr_id.Frame.PC == pc!;
                    });
                    filterParams.Add("pc", encPC);

                }

            }


            {
                var lat__prev1 = lat;

                var (lat, err) = time.ParseDuration(r.FormValue("latmin"));

                if (err == null)
                {
                    name = append(name, fmt.Sprintf("latency >= %s", lat));
                    conditions = append(conditions, (_, s) =>
                    {
                        return _addr_s.duration() >= lat!;
                    });
                    filterParams.Add("latmin", lat.String());

                }

                lat = lat__prev1;

            }

            {
                var lat__prev1 = lat;

                (lat, err) = time.ParseDuration(r.FormValue("latmax"));

                if (err == null)
                {
                    name = append(name, fmt.Sprintf("latency <= %s", lat));
                    conditions = append(conditions, (_, s) =>
                    {
                        return _addr_s.duration() <= lat!;
                    });
                    filterParams.Add("latmax", lat.String());

                }

                lat = lat__prev1;

            }


            return (addr(new regionFilter(name:strings.Join(name,","),cond:conditions,params:filterParams,)), error.As(null!)!);

        }

        private partial struct durationHistogram
        {
            public long Count;
            public slice<long> Buckets;
            public long MinBucket;
            public long MaxBucket;
        }

        // Five buckets for every power of 10.
        private static var logDiv = math.Log(math.Pow(10L, 1.0F / 5L));

        private static void add(this ptr<durationHistogram> _addr_h, time.Duration d)
        {
            ref durationHistogram h = ref _addr_h.val;

            long bucket = default;
            if (d > 0L)
            {
                bucket = int(math.Log(float64(d)) / logDiv);
            }

            if (len(h.Buckets) <= bucket)
            {
                h.Buckets = append(h.Buckets, make_slice<long>(bucket - len(h.Buckets) + 1L));
                h.Buckets = h.Buckets[..cap(h.Buckets)];
            }

            h.Buckets[bucket]++;
            if (bucket < h.MinBucket || h.MaxBucket == 0L)
            {
                h.MinBucket = bucket;
            }

            if (bucket > h.MaxBucket)
            {
                h.MaxBucket = bucket;
            }

            h.Count++;

        }

        private static time.Duration BucketMin(this ptr<durationHistogram> _addr_h, long bucket)
        {
            ref durationHistogram h = ref _addr_h.val;

            return time.Duration(math.Exp(float64(bucket) * logDiv));
        }

        private static @string niceDuration(time.Duration d)
        {
            time.Duration rnd = default;
            @string unit = default;

            if (d < 10L * time.Microsecond) 
                rnd = time.Nanosecond;
                unit = "ns";
            else if (d < 10L * time.Millisecond) 
                rnd = time.Microsecond;
                unit = "µs";
            else if (d < 10L * time.Second) 
                rnd = time.Millisecond;
                unit = "ms";
            else 
                rnd = time.Second;
                unit = "s ";
                        return fmt.Sprintf("%d%s", d / rnd, unit);

        }

        private static template.HTML ToHTML(this ptr<durationHistogram> _addr_h, Func<time.Duration, time.Duration, @string> urlmaker)
        {
            ref durationHistogram h = ref _addr_h.val;

            if (h == null || h.Count == 0L)
            {
                return template.HTML("");
            }

            const long barWidth = (long)400L;



            long maxCount = 0L;
            foreach (var (_, count) in h.Buckets)
            {
                if (count > maxCount)
                {
                    maxCount = count;
                }

            }
            ptr<object> w = @new<bytes.Buffer>();
            fmt.Fprintf(w, "<table>");
            for (var i = h.MinBucket; i <= h.MaxBucket; i++)
            { 
                // Tick label.
                if (h.Buckets[i] > 0L)
                {
                    fmt.Fprintf(w, "<tr><td class=\"histoTime\" align=\"right\"><a href=%s>%s</a></td>", urlmaker(h.BucketMin(i), h.BucketMin(i + 1L)), niceDuration(h.BucketMin(i)));
                }
                else
                {
                    fmt.Fprintf(w, "<tr><td class=\"histoTime\" align=\"right\">%s</td>", niceDuration(h.BucketMin(i)));
                } 
                // Bucket bar.
                var width = h.Buckets[i] * barWidth / maxCount;
                fmt.Fprintf(w, "<td><div style=\"width:%dpx;background:blue;position:relative\">&nbsp;</div></td>", width); 
                // Bucket count.
                fmt.Fprintf(w, "<td align=\"right\"><div style=\"position:relative\">%d</div></td>", h.Buckets[i]);
                fmt.Fprintf(w, "</tr>\n");


            } 
            // Final tick label.
 
            // Final tick label.
            fmt.Fprintf(w, "<tr><td align=\"right\">%s</td></tr>", niceDuration(h.BucketMin(h.MaxBucket + 1L)));
            fmt.Fprintf(w, "</table>");
            return template.HTML(w.String());

        }

        private static @string String(this ptr<durationHistogram> _addr_h)
        {
            ref durationHistogram h = ref _addr_h.val;

            const long barWidth = (long)40L;



            @string labels = new slice<@string>(new @string[] {  });
            long maxLabel = 0L;
            long maxCount = 0L;
            {
                var i__prev1 = i;

                for (var i = h.MinBucket; i <= h.MaxBucket; i++)
                { 
                    // TODO: This formatting is pretty awful.
                    var label = fmt.Sprintf("[%-12s%-11s)", h.BucketMin(i).String() + ",", h.BucketMin(i + 1L));
                    labels = append(labels, label);
                    if (len(label) > maxLabel)
                    {
                        maxLabel = len(label);
                    }

                    var count = h.Buckets[i];
                    if (count > maxCount)
                    {
                        maxCount = count;
                    }

                }


                i = i__prev1;
            }

            ptr<object> w = @new<bytes.Buffer>();
            {
                var i__prev1 = i;

                for (i = h.MinBucket; i <= h.MaxBucket; i++)
                {
                    count = h.Buckets[i];
                    var bar = count * barWidth / maxCount;
                    fmt.Fprintf(w, "%*s %-*s %d\n", maxLabel, labels[i - h.MinBucket], barWidth, strings.Repeat("█", bar), count);
                }


                i = i__prev1;
            }
            return w.String();

        }

        private partial struct regionStats
        {
            public ref regionTypeID regionTypeID => ref regionTypeID_val;
            public durationHistogram Histogram;
        }

        private static Func<time.Duration, time.Duration, @string> UserRegionURL(this ptr<regionStats> _addr_s)
        {
            ref regionStats s = ref _addr_s.val;

            return (min, max) =>
            {
                return fmt.Sprintf("/userregion?type=%s&pc=%x&latmin=%v&latmax=%v", template.URLQueryEscaper(s.Type), s.Frame.PC, template.URLQueryEscaper(min), template.URLQueryEscaper(max));
            };

        }

        private static void add(this ptr<regionStats> _addr_s, regionDesc region)
        {
            ref regionStats s = ref _addr_s.val;

            s.Histogram.add(region.duration());
        }

        private static var templUserRegionTypes = template.Must(template.New("").Parse(@"
<html>
<style type=""text/css"">
.histoTime {
   width: 20%;
   white-space:nowrap;
}

</style>
<body>
<table border=""1"" sortable=""1"">
<tr>
<th>Region type</th>
<th>Count</th>
<th>Duration distribution (complete tasks)</th>
</tr>
{{range $}}
  <tr>
    <td>{{.Type}}<br>{{.Frame.Fn}}<br>{{.Frame.File}}:{{.Frame.Line}}</td>
    <td><a href=""/userregion?type={{.Type}}&pc={{.Frame.PC | printf ""%x""}}"">{{.Histogram.Count}}</a></td>
    <td>{{.Histogram.ToHTML (.UserRegionURL)}}</td>
  </tr>
{{end}}
</table>
</body>
</html>
"));

        private partial struct taskStats
        {
            public @string Type;
            public long Count; // Complete + incomplete tasks
            public durationHistogram Histogram; // Complete tasks only
        }

        private static Func<time.Duration, time.Duration, @string> UserTaskURL(this ptr<taskStats> _addr_s, bool complete)
        {
            ref taskStats s = ref _addr_s.val;

            return (min, max) =>
            {
                return fmt.Sprintf("/usertask?type=%s&complete=%v&latmin=%v&latmax=%v", template.URLQueryEscaper(s.Type), template.URLQueryEscaper(complete), template.URLQueryEscaper(min), template.URLQueryEscaper(max));
            };

        }

        private static void add(this ptr<taskStats> _addr_s, ptr<taskDesc> _addr_task)
        {
            ref taskStats s = ref _addr_s.val;
            ref taskDesc task = ref _addr_task.val;

            s.Count++;
            if (task.complete())
            {
                s.Histogram.add(task.duration());
            }

        }

        private static var templUserTaskTypes = template.Must(template.New("").Parse(@"
<html>
<style type=""text/css"">
.histoTime {
   width: 20%;
   white-space:nowrap;
}

</style>
<body>
Search log text: <form action=""/usertask""><input name=""logtext"" type=""text""><input type=""submit""></form><br>
<table border=""1"" sortable=""1"">
<tr>
<th>Task type</th>
<th>Count</th>
<th>Duration distribution (complete tasks)</th>
</tr>
{{range $}}
  <tr>
    <td>{{.Type}}</td>
    <td><a href=""/usertask?type={{.Type}}"">{{.Count}}</a></td>
    <td>{{.Histogram.ToHTML (.UserTaskURL true)}}</td>
  </tr>
{{end}}
</table>
</body>
</html>
"));

        private static var templUserTaskType = template.Must(template.New("userTask").Funcs(new template.FuncMap("elapsed":elapsed,"asMillisecond":asMillisecond,"trimSpace":strings.TrimSpace,)).Parse("\n<html>\n<head> <title>User Task: {{.Name}} </title> </head>\n        <style type=\"" +
    "text/css\">\n                body {\n                        font-family: sans-seri" +
    "f;\n                }\n                table#req-status td.family {\n              " +
    "          padding-right: 2em;\n                }\n                table#req-status" +
    " td.active {\n                        padding-right: 1em;\n                }\n     " +
    "           table#req-status td.empty {\n                        color: #aaa;\n    " +
    "            }\n                table#reqs {\n                        margin-top: 1" +
    "em;\n                }\n                table#reqs tr.first {\n                    " +
    "    font-weight: bold;\n                }\n                table#reqs td {\n       " +
    "                 font-family: monospace;\n                }\n                table" +
    "#reqs td.when {\n                        text-align: right;\n                     " +
    "   white-space: nowrap;\n                }\n                table#reqs td.elapsed " +
    "{\n                        padding: 0 0.5em;\n                        text-align: " +
    "right;\n                        white-space: pre;\n                        width: " +
    "10em;\n                }\n                address {\n                        font-s" +
    "ize: smaller;\n                        margin-top: 5em;\n                }\n       " +
    " </style>\n<body>\n\n<h2>User Task: {{.Name}}</h2>\n\nSearch log text: <form onsubmit" +
    "=\"window.location.search+=\'&logtext=\'+window.logtextinput.value; return false\">\n" +
    "<input name=\"logtext\" id=\"logtextinput\" type=\"text\"><input type=\"submit\">\n</form" +
    "><br>\n\n<table id=\"reqs\">\n<tr><th>When</th><th>Elapsed</th><th>Goroutine ID</th><" +
    "th>Events</th></tr>\n     {{range $el := $.Entry}}\n        <tr class=\"first\">\n   " +
    "             <td class=\"when\">{{$el.WhenString}}</td>\n                <td class=" +
    "\"elapsed\">{{$el.Duration}}</td>\n\t\t<td></td>\n                <td>\n<a href=\"/trace" +
    "?focustask={{$el.ID}}#{{asMillisecond $el.Start}}:{{asMillisecond $el.End}}\">Tas" +
    "k {{$el.ID}}</a>\n<a href=\"/trace?taskid={{$el.ID}}#{{asMillisecond $el.Start}}:{" +
    "{asMillisecond $el.End}}\">(goroutine view)</a>\n({{if .Complete}}complete{{else}}" +
    "incomplete{{end}})</td>\n        </tr>\n        {{range $el.Events}}\n        <tr>\n" +
    "                <td class=\"when\">{{.WhenString}}</td>\n                <td class=" +
    "\"elapsed\">{{elapsed .Elapsed}}</td>\n\t\t<td class=\"goid\">{{.Go}}</td>\n            " +
    "    <td>{{.What}}</td>\n        </tr>\n        {{end}}\n\t<tr>\n\t\t<td></td>\n\t\t<td></t" +
    "d>\n\t\t<td></td>\n\t\t<td>GC:{{$el.GCTime}}</td>\n    {{end}}\n</body>\n</html>\n"));

        private static @string elapsed(time.Duration d)
        {
            slice<byte> b = (slice<byte>)fmt.Sprintf("%.9f", d.Seconds()); 

            // For subsecond durations, blank all zeros before decimal point,
            // and all zeros between the decimal point and the first non-zero digit.
            if (d < time.Second)
            {
                var dot = bytes.IndexByte(b, '.');
                {
                    long i__prev1 = i;

                    for (long i = 0L; i < dot; i++)
                    {
                        b[i] = ' ';
                    }


                    i = i__prev1;
                }
                {
                    long i__prev1 = i;

                    for (i = dot + 1L; i < len(b); i++)
                    {
                        if (b[i] == '0')
                        {
                            b[i] = ' ';
                        }
                        else
                        {
                            break;
                        }

                    }


                    i = i__prev1;
                }

            }

            return string(b);

        }

        private static double asMillisecond(time.Duration d)
        {
            return float64(d.Nanoseconds()) / 1e6F;
        }

        private static @string formatUserLog(ptr<trace.Event> _addr_ev)
        {
            ref trace.Event ev = ref _addr_ev.val;

            var k = ev.SArgs[0L];
            var v = ev.SArgs[1L];
            if (k == "")
            {
                return v;
            }

            if (v == "")
            {
                return k;
            }

            return fmt.Sprintf("%v=%v", k, v);

        }

        private static @string describeEvent(ptr<trace.Event> _addr_ev)
        {
            ref trace.Event ev = ref _addr_ev.val;


            if (ev.Type == trace.EvGoCreate) 
                var goid = ev.Args[0L];
                return fmt.Sprintf("new goroutine %d: %s", goid, gs[goid].Name);
            else if (ev.Type == trace.EvGoEnd || ev.Type == trace.EvGoStop) 
                return "goroutine stopped";
            else if (ev.Type == trace.EvUserLog) 
                return formatUserLog(_addr_ev);
            else if (ev.Type == trace.EvUserRegion) 
                if (ev.Args[1L] == 0L)
                {
                    @string duration = "unknown";
                    if (ev.Link != null)
                    {
                        duration = (time.Duration(ev.Link.Ts - ev.Ts) * time.Nanosecond).String();
                    }

                    return fmt.Sprintf("region %s started (duration: %v)", ev.SArgs[0L], duration);

                }

                return fmt.Sprintf("region %s ended", ev.SArgs[0L]);
            else if (ev.Type == trace.EvUserTaskCreate) 
                return fmt.Sprintf("task %v (id %d, parent %d) created", ev.SArgs[0L], ev.Args[0L], ev.Args[1L]); 
                // TODO: add child task creation events into the parent task events
            else if (ev.Type == trace.EvUserTaskEnd) 
                return "task end";
                        return "";

        }

        private static (ulong, bool) isUserAnnotationEvent(ptr<trace.Event> _addr_ev)
        {
            ulong taskID = default;
            bool ok = default;
            ref trace.Event ev = ref _addr_ev.val;


            if (ev.Type == trace.EvUserLog || ev.Type == trace.EvUserRegion || ev.Type == trace.EvUserTaskCreate || ev.Type == trace.EvUserTaskEnd) 
                return (ev.Args[0L], true);
                        return (0L, false);

        }

        private static var templUserRegionType = template.Must(template.New("").Funcs(new template.FuncMap("prettyDuration":func(nsecint64)template.HTML{d:=time.Duration(nsec)*time.Nanosecondreturntemplate.HTML(niceDuration(d))},"percent":func(dividend,divisorint64)template.HTML{ifdivisor==0{return""}returntemplate.HTML(fmt.Sprintf("(%.1f%%)",float64(dividend)/float64(divisor)*100))},"barLen":func(dividend,divisorint64)template.HTML{ifdivisor==0{return"0"}returntemplate.HTML(fmt.Sprintf("%.2f%%",float64(dividend)/float64(divisor)*100))},"unknownTime":func(descregionDesc)int64{sum:=desc.ExecTime+desc.IOTime+desc.BlockTime+desc.SyscallTime+desc.SchedWaitTimeifsum<desc.TotalTime{returndesc.TotalTime-sum}return0},"filterParams":func(f*regionFilter)template.URL{returntemplate.URL(f.params.Encode())},)).Parse("\n<!DOCTYPE html>\n<title>User Region {{.Name}}</title>\n<style>\nth {\n  background-c" +
    "olor: #050505;\n  color: #fff;\n}\nth.total-time,\nth.exec-time,\nth.io-time,\nth.bloc" +
    "k-time,\nth.syscall-time,\nth.sched-time,\nth.sweep-time,\nth.pause-time {\n  cursor:" +
    " pointer;\n}\ntable {\n  border-collapse: collapse;\n}\n.details tr:hover {\n  backgro" +
    "und-color: #f2f2f2;\n}\n.details td {\n  text-align: right;\n  border: 1px solid #00" +
    "0;\n}\n.details td.id {\n  text-align: left;\n}\n.stacked-bar-graph {\n  width: 300px;" +
    "\n  height: 10px;\n  color: #414042;\n  white-space: nowrap;\n  font-size: 5px;\n}\n.s" +
    "tacked-bar-graph span {\n  display: inline-block;\n  width: 100%;\n  height: 100%;\n" +
    "  box-sizing: border-box;\n  float: left;\n  padding: 0;\n}\n.unknown-time { backgro" +
    "und-color: #636363; }\n.exec-time { background-color: #d7191c; }\n.io-time { backg" +
    "round-color: #fdae61; }\n.block-time { background-color: #d01c8b; }\n.syscall-time" +
    " { background-color: #7b3294; }\n.sched-time { background-color: #2c7bb6; }\n</sty" +
    "le>\n\n<script>\nfunction reloadTable(key, value) {\n  let params = new URLSearchPar" +
    "ams(window.location.search);\n  params.set(key, value);\n  window.location.search " +
    "= params.toString();\n}\n</script>\n\n<h2>{{.Name}}</h2>\n\n{{ with $p := filterParams" +
    " .Filter}}\n<table class=\"summary\">\n\t<tr><td>Network Wait Time:</td><td> <a href=" +
    "\"/regionio?{{$p}}\">graph</a><a href=\"/regionio?{{$p}}&raw=1\" download=\"io.profil" +
    "e\">(download)</a></td></tr>\n\t<tr><td>Sync Block Time:</td><td> <a href=\"/regionb" +
    "lock?{{$p}}\">graph</a><a href=\"/regionblock?{{$p}}&raw=1\" download=\"block.profil" +
    "e\">(download)</a></td></tr>\n\t<tr><td>Blocking Syscall Time:</td><td> <a href=\"/r" +
    "egionsyscall?{{$p}}\">graph</a><a href=\"/regionsyscall?{{$p}}&raw=1\" download=\"sy" +
    "scall.profile\">(download)</a></td></tr>\n\t<tr><td>Scheduler Wait Time:</td><td> <" +
    "a href=\"/regionsched?{{$p}}\">graph</a><a href=\"/regionsched?{{$p}}&raw=1\" downlo" +
    "ad=\"sched.profile\">(download)</a></td></tr>\n</table>\n{{ end }}\n<p>\n<table class=" +
    "\"details\">\n<tr>\n<th> Goroutine </th>\n<th> Task </th>\n<th onclick=\"reloadTable(\'s" +
    "ortby\', \'TotalTime\')\" class=\"total-time\"> Total</th>\n<th></th>\n<th onclick=\"relo" +
    "adTable(\'sortby\', \'ExecTime\')\" class=\"exec-time\"> Execution</th>\n<th onclick=\"re" +
    "loadTable(\'sortby\', \'IOTime\')\" class=\"io-time\"> Network wait</th>\n<th onclick=\"r" +
    "eloadTable(\'sortby\', \'BlockTime\')\" class=\"block-time\"> Sync block </th>\n<th oncl" +
    "ick=\"reloadTable(\'sortby\', \'SyscallTime\')\" class=\"syscall-time\"> Blocking syscal" +
    "l</th>\n<th onclick=\"reloadTable(\'sortby\', \'SchedWaitTime\')\" class=\"sched-time\"> " +
    "Scheduler wait</th>\n<th onclick=\"reloadTable(\'sortby\', \'SweepTime\')\" class=\"swee" +
    "p-time\"> GC sweeping</th>\n<th onclick=\"reloadTable(\'sortby\', \'GCTime\')\" class=\"p" +
    "ause-time\"> GC pause</th>\n</tr>\n{{range .Data}}\n  <tr>\n    <td> <a href=\"/trace?" +
    "goid={{.G}}\">{{.G}}</a> </td>\n    <td> {{if .TaskID}}<a href=\"/trace?focustask={" +
    "{.TaskID}}\">{{.TaskID}}</a>{{end}} </td>\n    <td> {{prettyDuration .TotalTime}} " +
    "</td>\n    <td>\n        <div class=\"stacked-bar-graph\">\n          {{if unknownTim" +
    "e .}}<span style=\"width:{{barLen (unknownTime .) $.MaxTotal}}\" class=\"unknown-ti" +
    "me\">&nbsp;</span>{{end}}\n          {{if .ExecTime}}<span style=\"width:{{barLen ." +
    "ExecTime $.MaxTotal}}\" class=\"exec-time\">&nbsp;</span>{{end}}\n          {{if .IO" +
    "Time}}<span style=\"width:{{barLen .IOTime $.MaxTotal}}\" class=\"io-time\">&nbsp;</" +
    "span>{{end}}\n          {{if .BlockTime}}<span style=\"width:{{barLen .BlockTime $" +
    ".MaxTotal}}\" class=\"block-time\">&nbsp;</span>{{end}}\n          {{if .SyscallTime" +
    "}}<span style=\"width:{{barLen .SyscallTime $.MaxTotal}}\" class=\"syscall-time\">&n" +
    "bsp;</span>{{end}}\n          {{if .SchedWaitTime}}<span style=\"width:{{barLen .S" +
    "chedWaitTime $.MaxTotal}}\" class=\"sched-time\">&nbsp;</span>{{end}}\n        </div" +
    ">\n    </td>\n    <td> {{prettyDuration .ExecTime}}</td>\n    <td> {{prettyDuration" +
    " .IOTime}}</td>\n    <td> {{prettyDuration .BlockTime}}</td>\n    <td> {{prettyDur" +
    "ation .SyscallTime}}</td>\n    <td> {{prettyDuration .SchedWaitTime}}</td>\n    <t" +
    "d> {{prettyDuration .SweepTime}} {{percent .SweepTime .TotalTime}}</td>\n    <td>" +
    " {{prettyDuration .GCTime}} {{percent .GCTime .TotalTime}}</td>\n  </tr>\n{{end}}\n" +
    "</table>\n</p>\n"));
    }
}
