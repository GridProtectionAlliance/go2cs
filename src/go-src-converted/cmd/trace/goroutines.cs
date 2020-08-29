// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Goroutine-related profiles.

// package main -- go2cs converted at 2020 August 29 10:04:51 UTC
// Original source: C:\Go\src\cmd\trace\goroutines.go
using fmt = go.fmt_package;
using template = go.html.template_package;
using trace = go.@internal.trace_package;
using http = go.net.http_package;
using sort = go.sort_package;
using strconv = go.strconv_package;
using sync = go.sync_package;
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

        private partial struct gtypeList // : slice<gtype>
        {
        }

        private static long Len(this gtypeList l)
        {
            return len(l);
        }

        private static bool Less(this gtypeList l, long i, long j)
        {
            return l[i].ExecTime > l[j].ExecTime;
        }

        private static void Swap(this gtypeList l, long i, long j)
        {
            l[i] = l[j];
            l[j] = l[i];
        }

        private partial struct gdescList // : slice<ref trace.GDesc>
        {
        }

        private static long Len(this gdescList l)
        {
            return len(l);
        }

        private static bool Less(this gdescList l, long i, long j)
        {
            return l[i].TotalTime > l[j].TotalTime;
        }

        private static void Swap(this gdescList l, long i, long j)
        {
            l[i] = l[j];
            l[j] = l[i];
        }

        private static sync.Once gsInit = default;        private static map<ulong, ref trace.GDesc> gs = default;

        // analyzeGoroutines generates statistics about execution of all goroutines and stores them in gs.
        private static void analyzeGoroutines(slice<ref trace.Event> events)
        {
            gsInit.Do(() =>
            {
                gs = trace.GoroutineStats(events);
            });
        }

        // httpGoroutines serves list of goroutine groups.
        private static void httpGoroutines(http.ResponseWriter w, ref http.Request r)
        {
            var (events, err) = parseEvents();
            if (err != null)
            {
                http.Error(w, err.Error(), http.StatusInternalServerError);
                return;
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
            gtypeList glist = default;
            foreach (var (k, v) in gss)
            {
                v.ID = k;
                glist = append(glist, v);
            }
            sort.Sort(glist);
            templGoroutines.Execute(w, glist);
        }

        private static var templGoroutines = template.Must(template.New("").Parse("\n<html>\n<body>\nGoroutines: <br>\n{{range $}}\n  <a href=\"/goroutine?id={{.ID}}\">{{." +
    "Name}}</a> N={{.N}} <br>\n{{end}}\n</body>\n</html>\n"));

        // httpGoroutine serves list of goroutines in a particular group.
        private static void httpGoroutine(http.ResponseWriter w, ref http.Request r)
        {
            var (events, err) = parseEvents();
            if (err != null)
            {
                http.Error(w, err.Error(), http.StatusInternalServerError);
                return;
            }
            var (pc, err) = strconv.ParseUint(r.FormValue("id"), 10L, 64L);
            if (err != null)
            {
                http.Error(w, fmt.Sprintf("failed to parse id parameter '%v': %v", r.FormValue("id"), err), http.StatusInternalServerError);
                return;
            }
            analyzeGoroutines(events);
            gdescList glist = default;
            foreach (var (_, g) in gs)
            {
                if (g.PC != pc)
                {
                    continue;
                }
                glist = append(glist, g);
            }
            sort.Sort(glist);
            err = templGoroutine.Execute(w, /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{PCuint64GListgdescList}{pc,glist});
            if (err != null)
            {
                http.Error(w, fmt.Sprintf("failed to execute template: %v", err), http.StatusInternalServerError);
                return;
            }
        }

        private static var templGoroutine = template.Must(template.New("").Parse(@"
<html>
<body>
<table border=""1"" sortable=""1"">
<tr>
<th> Goroutine </th>
<th> Total time, ns </th>
<th> Execution time, ns </th>
<th> <a href=""/io?id={{.PC}}"">Network wait time, ns</a><a href=""/io?id={{.PC}}&raw=1"" download=""io.profile"">⬇</a> </th>
<th> <a href=""/block?id={{.PC}}"">Sync block time, ns</a><a href=""/block?id={{.PC}}&raw=1"" download=""block.profile"">⬇</a> </th>
<th> <a href=""/syscall?id={{.PC}}"">Blocking syscall time, ns</a><a href=""/syscall?id={{.PC}}&raw=1"" download=""syscall.profile"">⬇</a> </th>
<th> <a href=""/sched?id={{.PC}}"">Scheduler wait time, ns</a><a href=""/sched?id={{.PC}}&raw=1"" download=""sched.profile"">⬇</a> </th>
<th> GC sweeping time, ns </th>
<th> GC pause time, ns </th>
</tr>
{{range .GList}}
  <tr>
    <td> <a href=""/trace?goid={{.ID}}"">{{.ID}}</a> </td>
    <td> {{.TotalTime}} </td>
    <td> {{.ExecTime}} </td>
    <td> {{.IOTime}} </td>
    <td> {{.BlockTime}} </td>
    <td> {{.SyscallTime}} </td>
    <td> {{.SchedWaitTime}} </td>
    <td> {{.SweepTime}} </td>
    <td> {{.GCTime}} </td>
  </tr>
{{end}}
</table>
</body>
</html>
"));
    }
}
