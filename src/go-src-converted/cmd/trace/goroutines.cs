// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Goroutine-related profiles.

// package main -- go2cs converted at 2020 October 09 05:53:06 UTC
// Original source: C:\Go\src\cmd\trace\goroutines.go
using fmt = go.fmt_package;
using template = go.html.template_package;
using trace = go.@internal.trace_package;
using log = go.log_package;
using http = go.net.http_package;
using reflect = go.reflect_package;
using sort = go.sort_package;
using strconv = go.strconv_package;
using sync = go.sync_package;
using time = go.time_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class main_package
    {
        private static void init()
        {
            http.HandleFunc("/goroutines", httpGoroutines);
            http.HandleFunc("/goroutine", httpGoroutine);
        }

        // gtype describes a group of goroutines grouped by start PC.
        private partial struct gtype
        {
            public ulong ID; // Unique identifier (PC).
            public @string Name; // Start function.
            public long N; // Total number of goroutines in this group.
            public long ExecTime; // Total execution time of all goroutines in this group.
        }

        private static sync.Once gsInit = default;        private static map<ulong, ptr<trace.GDesc>> gs = default;

        // analyzeGoroutines generates statistics about execution of all goroutines and stores them in gs.
        private static void analyzeGoroutines(slice<ptr<trace.Event>> events)
        {
            gsInit.Do(() =>
            {
                gs = trace.GoroutineStats(events);
            });

        }

        // httpGoroutines serves list of goroutine groups.
        private static void httpGoroutines(http.ResponseWriter w, ptr<http.Request> _addr_r)
        {
            ref http.Request r = ref _addr_r.val;

            var (events, err) = parseEvents();
            if (err != null)
            {
                http.Error(w, err.Error(), http.StatusInternalServerError);
                return ;
            }

            analyzeGoroutines(events);
            var gss = make_map<ulong, gtype>();
            foreach (var (_, g) in gs)
            {
                var gs1 = gss[g.PC];
                gs1.ID = g.PC;
                gs1.Name = g.Name;
                gs1.N++;
                gs1.ExecTime += g.ExecTime;
                gss[g.PC] = gs1;
            }
            slice<gtype> glist = default;
            foreach (var (k, v) in gss)
            {
                v.ID = k;
                glist = append(glist, v);
            }
            sort.Slice(glist, (i, j) => glist[i].ExecTime > glist[j].ExecTime);
            w.Header().Set("Content-Type", "text/html;charset=utf-8");
            {
                var err = templGoroutines.Execute(w, glist);

                if (err != null)
                {
                    log.Printf("failed to execute template: %v", err);
                    return ;
                }

            }

        }

        private static var templGoroutines = template.Must(template.New("").Parse("\n<html>\n<body>\nGoroutines: <br>\n{{range $}}\n  <a href=\"/goroutine?id={{.ID}}\">{{." +
    "Name}}</a> N={{.N}} <br>\n{{end}}\n</body>\n</html>\n"));

        // httpGoroutine serves list of goroutines in a particular group.
        private static void httpGoroutine(http.ResponseWriter w, ptr<http.Request> _addr_r)
        {
            ref http.Request r = ref _addr_r.val;
 
            // TODO(hyangah): support format=csv (raw data)

            var (events, err) = parseEvents();
            if (err != null)
            {
                http.Error(w, err.Error(), http.StatusInternalServerError);
                return ;
            }

            var (pc, err) = strconv.ParseUint(r.FormValue("id"), 10L, 64L);
            if (err != null)
            {
                http.Error(w, fmt.Sprintf("failed to parse id parameter '%v': %v", r.FormValue("id"), err), http.StatusInternalServerError);
                return ;
            }

            analyzeGoroutines(events);
            slice<ptr<trace.GDesc>> glist = default;            @string name = default;            long totalExecTime = default;            long execTime = default;
            long maxTotalTime = default;

            foreach (var (_, g) in gs)
            {
                totalExecTime += g.ExecTime;

                if (g.PC != pc)
                {
                    continue;
                }

                glist = append(glist, g);
                name = g.Name;
                execTime += g.ExecTime;
                if (maxTotalTime < g.TotalTime)
                {
                    maxTotalTime = g.TotalTime;
                }

            }
            @string execTimePercent = "";
            if (totalExecTime > 0L)
            {
                execTimePercent = fmt.Sprintf("%.2f%%", float64(execTime) / float64(totalExecTime) * 100L);
            }

            var sortby = r.FormValue("sortby");
            var (_, ok) = reflect.TypeOf(new trace.GDesc()).FieldByNameFunc(s =>
            {
                return s == sortby;
            });
            if (!ok)
            {
                sortby = "TotalTime";
            }

            sort.Slice(glist, (i, j) =>
            {
                var ival = reflect.ValueOf(glist[i]).Elem().FieldByName(sortby).Int();
                var jval = reflect.ValueOf(glist[j]).Elem().FieldByName(sortby).Int();
                return ival > jval;
            });

            err = templGoroutine.Execute(w, /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{NamestringPCuint64NintExecTimePercentstringMaxTotalint64GList[]*trace.GDesc}{Name:name,PC:pc,N:len(glist),ExecTimePercent:execTimePercent,MaxTotal:maxTotalTime,GList:glist});
            if (err != null)
            {
                http.Error(w, fmt.Sprintf("failed to execute template: %v", err), http.StatusInternalServerError);
                return ;
            }

        }

        private static var templGoroutine = template.Must(template.New("").Funcs(new template.FuncMap("prettyDuration":func(nsecint64)template.HTML{d:=time.Duration(nsec)*time.Nanosecondreturntemplate.HTML(niceDuration(d))},"percent":func(dividend,divisorint64)template.HTML{ifdivisor==0{return""}returntemplate.HTML(fmt.Sprintf("(%.1f%%)",float64(dividend)/float64(divisor)*100))},"barLen":func(dividend,divisorint64)template.HTML{ifdivisor==0{return"0"}returntemplate.HTML(fmt.Sprintf("%.2f%%",float64(dividend)/float64(divisor)*100))},"unknownTime":func(desc*trace.GDesc)int64{sum:=desc.ExecTime+desc.IOTime+desc.BlockTime+desc.SyscallTime+desc.SchedWaitTimeifsum<desc.TotalTime{returndesc.TotalTime-sum}return0},)).Parse("\n<!DOCTYPE html>\n<title>Goroutine {{.Name}}</title>\n<style>\nth {\n  background-col" +
    "or: #050505;\n  color: #fff;\n}\nth.total-time,\nth.exec-time,\nth.io-time,\nth.block-" +
    "time,\nth.syscall-time,\nth.sched-time,\nth.sweep-time,\nth.pause-time {\n  cursor: p" +
    "ointer;\n}\ntable {\n  border-collapse: collapse;\n}\n.details tr:hover {\n  backgroun" +
    "d-color: #f2f2f2;\n}\n.details td {\n  text-align: right;\n  border: 1px solid black" +
    ";\n}\n.details td.id {\n  text-align: left;\n}\n.stacked-bar-graph {\n  width: 300px;\n" +
    "  height: 10px;\n  color: #414042;\n  white-space: nowrap;\n  font-size: 5px;\n}\n.st" +
    "acked-bar-graph span {\n  display: inline-block;\n  width: 100%;\n  height: 100%;\n " +
    " box-sizing: border-box;\n  float: left;\n  padding: 0;\n}\n.unknown-time { backgrou" +
    "nd-color: #636363; }\n.exec-time { background-color: #d7191c; }\n.io-time { backgr" +
    "ound-color: #fdae61; }\n.block-time { background-color: #d01c8b; }\n.syscall-time " +
    "{ background-color: #7b3294; }\n.sched-time { background-color: #2c7bb6; }\n</styl" +
    "e>\n\n<script>\nfunction reloadTable(key, value) {\n  let params = new URLSearchPara" +
    "ms(window.location.search);\n  params.set(key, value);\n  window.location.search =" +
    " params.toString();\n}\n</script>\n\n<table class=\"summary\">\n\t<tr><td>Goroutine Name" +
    ":</td><td>{{.Name}}</td></tr>\n\t<tr><td>Number of Goroutines:</td><td>{{.N}}</td>" +
    "</tr>\n\t<tr><td>Execution Time:</td><td>{{.ExecTimePercent}} of total program exe" +
    "cution time </td> </tr>\n\t<tr><td>Network Wait Time:</td><td> <a href=\"/io?id={{." +
    "PC}}\">graph</a><a href=\"/io?id={{.PC}}&raw=1\" download=\"io.profile\">(download)</" +
    "a></td></tr>\n\t<tr><td>Sync Block Time:</td><td> <a href=\"/block?id={{.PC}}\">grap" +
    "h</a><a href=\"/block?id={{.PC}}&raw=1\" download=\"block.profile\">(download)</a></" +
    "td></tr>\n\t<tr><td>Blocking Syscall Time:</td><td> <a href=\"/syscall?id={{.PC}}\">" +
    "graph</a><a href=\"/syscall?id={{.PC}}&raw=1\" download=\"syscall.profile\">(downloa" +
    "d)</a></td></tr>\n\t<tr><td>Scheduler Wait Time:</td><td> <a href=\"/sched?id={{.PC" +
    "}}\">graph</a><a href=\"/sched?id={{.PC}}&raw=1\" download=\"sched.profile\">(downloa" +
    "d)</a></td></tr>\n</table>\n<p>\n<table class=\"details\">\n<tr>\n<th> Goroutine</th>\n<" +
    "th onclick=\"reloadTable(\'sortby\', \'TotalTime\')\" class=\"total-time\"> Total</th>\n<" +
    "th></th>\n<th onclick=\"reloadTable(\'sortby\', \'ExecTime\')\" class=\"exec-time\"> Exec" +
    "ution</th>\n<th onclick=\"reloadTable(\'sortby\', \'IOTime\')\" class=\"io-time\"> Networ" +
    "k wait</th>\n<th onclick=\"reloadTable(\'sortby\', \'BlockTime\')\" class=\"block-time\">" +
    " Sync block </th>\n<th onclick=\"reloadTable(\'sortby\', \'SyscallTime\')\" class=\"sysc" +
    "all-time\"> Blocking syscall</th>\n<th onclick=\"reloadTable(\'sortby\', \'SchedWaitTi" +
    "me\')\" class=\"sched-time\"> Scheduler wait</th>\n<th onclick=\"reloadTable(\'sortby\'," +
    " \'SweepTime\')\" class=\"sweep-time\"> GC sweeping</th>\n<th onclick=\"reloadTable(\'so" +
    "rtby\', \'GCTime\')\" class=\"pause-time\"> GC pause</th>\n</tr>\n{{range .GList}}\n  <tr" +
    ">\n    <td> <a href=\"/trace?goid={{.ID}}\">{{.ID}}</a> </td>\n    <td> {{prettyDura" +
    "tion .TotalTime}} </td>\n    <td>\n\t<div class=\"stacked-bar-graph\">\n\t  {{if unknow" +
    "nTime .}}<span style=\"width:{{barLen (unknownTime .) $.MaxTotal}}\" class=\"unknow" +
    "n-time\">&nbsp;</span>{{end}}\n          {{if .ExecTime}}<span style=\"width:{{barL" +
    "en .ExecTime $.MaxTotal}}\" class=\"exec-time\">&nbsp;</span>{{end}}\n          {{if" +
    " .IOTime}}<span style=\"width:{{barLen .IOTime $.MaxTotal}}\" class=\"io-time\">&nbs" +
    "p;</span>{{end}}\n          {{if .BlockTime}}<span style=\"width:{{barLen .BlockTi" +
    "me $.MaxTotal}}\" class=\"block-time\">&nbsp;</span>{{end}}\n          {{if .Syscall" +
    "Time}}<span style=\"width:{{barLen .SyscallTime $.MaxTotal}}\" class=\"syscall-time" +
    "\">&nbsp;</span>{{end}}\n          {{if .SchedWaitTime}}<span style=\"width:{{barLe" +
    "n .SchedWaitTime $.MaxTotal}}\" class=\"sched-time\">&nbsp;</span>{{end}}\n        <" +
    "/div>\n    </td>\n    <td> {{prettyDuration .ExecTime}}</td>\n    <td> {{prettyDura" +
    "tion .IOTime}}</td>\n    <td> {{prettyDuration .BlockTime}}</td>\n    <td> {{prett" +
    "yDuration .SyscallTime}}</td>\n    <td> {{prettyDuration .SchedWaitTime}}</td>\n  " +
    "  <td> {{prettyDuration .SweepTime}} {{percent .SweepTime .TotalTime}}</td>\n    " +
    "<td> {{prettyDuration .GCTime}} {{percent .GCTime .TotalTime}}</td>\n  </tr>\n{{en" +
    "d}}\n</table>\n"));
    }
}
