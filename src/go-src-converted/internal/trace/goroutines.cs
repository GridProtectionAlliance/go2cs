// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package trace -- go2cs converted at 2020 August 29 10:04:52 UTC
// import "internal/trace" ==> using trace = go.@internal.trace_package
// Original source: C:\Go\src\internal\trace\goroutines.go

using static go.builtin;

namespace go {
namespace @internal
{
    public static partial class trace_package
    {
        // GDesc contains statistics about execution of a single goroutine.
        public partial struct GDesc
        {
            public ulong ID;
            public @string Name;
            public ulong PC;
            public long CreationTime;
            public long StartTime;
            public long EndTime;
            public long ExecTime;
            public long SchedWaitTime;
            public long IOTime;
            public long BlockTime;
            public long SyscallTime;
            public long GCTime;
            public long SweepTime;
            public long TotalTime;
            public ref gdesc gdesc => ref gdesc_ptr; // private part
        }

        // gdesc is a private part of GDesc that is required only during analysis.
        private partial struct gdesc
        {
            public long lastStartTime;
            public long blockNetTime;
            public long blockSyncTime;
            public long blockSyscallTime;
            public long blockSweepTime;
            public long blockGCTime;
            public long blockSchedTime;
        }

        // GoroutineStats generates statistics for all goroutines in the trace.
        public static map<ulong, ref GDesc> GoroutineStats(slice<ref Event> events)
        {
            var gs = make_map<ulong, ref GDesc>();
            long lastTs = default;
            long gcStartTime = default;
            foreach (var (_, ev) in events)
            {
                lastTs = ev.Ts;

                if (ev.Type == EvGoCreate) 
                    GDesc g = ref new GDesc(ID:ev.Args[0],CreationTime:ev.Ts,gdesc:new(gdesc));
                    g.blockSchedTime = ev.Ts;
                    gs[g.ID] = g;
                else if (ev.Type == EvGoStart || ev.Type == EvGoStartLabel) 
                    g = gs[ev.G];
                    if (g.PC == 0L)
                    {
                        g.PC = ev.Stk[0L].PC;
                        g.Name = ev.Stk[0L].Fn;
                    }
                    g.lastStartTime = ev.Ts;
                    if (g.StartTime == 0L)
                    {
                        g.StartTime = ev.Ts;
                    }
                    if (g.blockSchedTime != 0L)
                    {
                        g.SchedWaitTime += ev.Ts - g.blockSchedTime;
                        g.blockSchedTime = 0L;
                    }
                else if (ev.Type == EvGoEnd || ev.Type == EvGoStop) 
                    g = gs[ev.G];
                    g.ExecTime += ev.Ts - g.lastStartTime;
                    g.TotalTime = ev.Ts - g.CreationTime;
                    g.EndTime = ev.Ts;
                else if (ev.Type == EvGoBlockSend || ev.Type == EvGoBlockRecv || ev.Type == EvGoBlockSelect || ev.Type == EvGoBlockSync || ev.Type == EvGoBlockCond) 
                    g = gs[ev.G];
                    g.ExecTime += ev.Ts - g.lastStartTime;
                    g.blockSyncTime = ev.Ts;
                else if (ev.Type == EvGoSched || ev.Type == EvGoPreempt) 
                    g = gs[ev.G];
                    g.ExecTime += ev.Ts - g.lastStartTime;
                    g.blockSchedTime = ev.Ts;
                else if (ev.Type == EvGoSleep || ev.Type == EvGoBlock) 
                    g = gs[ev.G];
                    g.ExecTime += ev.Ts - g.lastStartTime;
                else if (ev.Type == EvGoBlockNet) 
                    g = gs[ev.G];
                    g.ExecTime += ev.Ts - g.lastStartTime;
                    g.blockNetTime = ev.Ts;
                else if (ev.Type == EvGoBlockGC) 
                    g = gs[ev.G];
                    g.ExecTime += ev.Ts - g.lastStartTime;
                    g.blockGCTime = ev.Ts;
                else if (ev.Type == EvGoUnblock) 
                    g = gs[ev.Args[0L]];
                    if (g.blockNetTime != 0L)
                    {
                        g.IOTime += ev.Ts - g.blockNetTime;
                        g.blockNetTime = 0L;
                    }
                    if (g.blockSyncTime != 0L)
                    {
                        g.BlockTime += ev.Ts - g.blockSyncTime;
                        g.blockSyncTime = 0L;
                    }
                    g.blockSchedTime = ev.Ts;
                else if (ev.Type == EvGoSysBlock) 
                    g = gs[ev.G];
                    g.ExecTime += ev.Ts - g.lastStartTime;
                    g.blockSyscallTime = ev.Ts;
                else if (ev.Type == EvGoSysExit) 
                    g = gs[ev.G];
                    if (g.blockSyscallTime != 0L)
                    {
                        g.SyscallTime += ev.Ts - g.blockSyscallTime;
                        g.blockSyscallTime = 0L;
                    }
                    g.blockSchedTime = ev.Ts;
                else if (ev.Type == EvGCSweepStart) 
                    g = gs[ev.G];
                    if (g != null)
                    { 
                        // Sweep can happen during GC on system goroutine.
                        g.blockSweepTime = ev.Ts;
                    }
                else if (ev.Type == EvGCSweepDone) 
                    g = gs[ev.G];
                    if (g != null && g.blockSweepTime != 0L)
                    {
                        g.SweepTime += ev.Ts - g.blockSweepTime;
                        g.blockSweepTime = 0L;
                    }
                else if (ev.Type == EvGCStart) 
                    gcStartTime = ev.Ts;
                else if (ev.Type == EvGCDone) 
                    {
                        GDesc g__prev2 = g;

                        foreach (var (_, __g) in gs)
                        {
                            g = __g;
                            if (g.EndTime == 0L)
                            {
                                g.GCTime += ev.Ts - gcStartTime;
                            }
                        }

                        g = g__prev2;
                    }
                            }
            {
                GDesc g__prev1 = g;

                foreach (var (_, __g) in gs)
                {
                    g = __g;
                    if (g.TotalTime == 0L)
                    {
                        g.TotalTime = lastTs - g.CreationTime;
                    }
                    if (g.EndTime == 0L)
                    {
                        g.EndTime = lastTs;
                    }
                    if (g.blockNetTime != 0L)
                    {
                        g.IOTime += lastTs - g.blockNetTime;
                        g.blockNetTime = 0L;
                    }
                    if (g.blockSyncTime != 0L)
                    {
                        g.BlockTime += lastTs - g.blockSyncTime;
                        g.blockSyncTime = 0L;
                    }
                    if (g.blockSyscallTime != 0L)
                    {
                        g.SyscallTime += lastTs - g.blockSyscallTime;
                        g.blockSyscallTime = 0L;
                    }
                    if (g.blockSchedTime != 0L)
                    {
                        g.SchedWaitTime += lastTs - g.blockSchedTime;
                        g.blockSchedTime = 0L;
                    }
                    g.gdesc = null;
                }

                g = g__prev1;
            }

            return gs;
        }

        // RelatedGoroutines finds a set of goroutines related to goroutine goid.
        public static map<ulong, bool> RelatedGoroutines(slice<ref Event> events, ulong goid)
        { 
            // BFS of depth 2 over "unblock" edges
            // (what goroutines unblock goroutine goid?).
            var gmap = make_map<ulong, bool>();
            gmap[goid] = true;
            for (long i = 0L; i < 2L; i++)
            {
                var gmap1 = make_map<ulong, bool>();
                foreach (var (g) in gmap)
                {
                    gmap1[g] = true;
                }
                foreach (var (_, ev) in events)
                {
                    if (ev.Type == EvGoUnblock && gmap[ev.Args[0L]])
                    {
                        gmap1[ev.G] = true;
                    }
                }
                gmap = gmap1;
            }

            gmap[0L] = true; // for GC events
            return gmap;
        }
    }
}}
