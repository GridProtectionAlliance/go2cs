// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package trace -- go2cs converted at 2020 October 08 04:42:25 UTC
// import "internal/trace" ==> using trace = go.@internal.trace_package
// Original source: C:\Go\src\internal\trace\goroutines.go
using sort = go.sort_package;
using static go.builtin;
using System;

namespace go {
namespace @internal
{
    public static partial class trace_package
    {
        // GDesc contains statistics and execution details of a single goroutine.
        public partial struct GDesc
        {
            public ulong ID;
            public @string Name;
            public ulong PC;
            public long CreationTime;
            public long StartTime;
            public long EndTime; // List of regions in the goroutine, sorted based on the start time.
            public slice<ptr<UserRegionDesc>> Regions; // Statistics of execution time during the goroutine execution.
            public ref GExecutionStat GExecutionStat => ref GExecutionStat_val;
            public ref ptr<gdesc> ptr<gdesc> => ref ptr<gdesc>_ptr; // private part.
        }

        // UserRegionDesc represents a region and goroutine execution stats
        // while the region was active.
        public partial struct UserRegionDesc
        {
            public ulong TaskID;
            public @string Name; // Region start event. Normally EvUserRegion start event or nil,
// but can be EvGoCreate event if the region is a synthetic
// region representing task inheritance from the parent goroutine.
            public ptr<Event> Start; // Region end event. Normally EvUserRegion end event or nil,
// but can be EvGoStop or EvGoEnd event if the goroutine
// terminated without explicitly ending the region.
            public ptr<Event> End;
            public ref GExecutionStat GExecutionStat => ref GExecutionStat_val;
        }

        // GExecutionStat contains statistics about a goroutine's execution
        // during a period of time.
        public partial struct GExecutionStat
        {
            public long ExecTime;
            public long SchedWaitTime;
            public long IOTime;
            public long BlockTime;
            public long SyscallTime;
            public long GCTime;
            public long SweepTime;
            public long TotalTime;
        }

        // sub returns the stats v-s.
        public static GExecutionStat sub(this GExecutionStat s, GExecutionStat v)
        {
            GExecutionStat r = default;

            r = s;
            r.ExecTime -= v.ExecTime;
            r.SchedWaitTime -= v.SchedWaitTime;
            r.IOTime -= v.IOTime;
            r.BlockTime -= v.BlockTime;
            r.SyscallTime -= v.SyscallTime;
            r.GCTime -= v.GCTime;
            r.SweepTime -= v.SweepTime;
            r.TotalTime -= v.TotalTime;
            return r;
        }

        // snapshotStat returns the snapshot of the goroutine execution statistics.
        // This is called as we process the ordered trace event stream. lastTs and
        // activeGCStartTime are used to process pending statistics if this is called
        // before any goroutine end event.
        private static GExecutionStat snapshotStat(this ptr<GDesc> _addr_g, long lastTs, long activeGCStartTime)
        {
            GExecutionStat ret = default;
            ref GDesc g = ref _addr_g.val;

            ret = g.GExecutionStat;

            if (g.gdesc == null)
            {
                return ret; // finalized GDesc. No pending state.
            }

            if (activeGCStartTime != 0L)
            { // terminating while GC is active
                if (g.CreationTime < activeGCStartTime)
                {
                    ret.GCTime += lastTs - activeGCStartTime;
                }
                else
                { 
                    // The goroutine's lifetime completely overlaps
                    // with a GC.
                    ret.GCTime += lastTs - g.CreationTime;

                }

            }

            if (g.TotalTime == 0L)
            {
                ret.TotalTime = lastTs - g.CreationTime;
            }

            if (g.lastStartTime != 0L)
            {
                ret.ExecTime += lastTs - g.lastStartTime;
            }

            if (g.blockNetTime != 0L)
            {
                ret.IOTime += lastTs - g.blockNetTime;
            }

            if (g.blockSyncTime != 0L)
            {
                ret.BlockTime += lastTs - g.blockSyncTime;
            }

            if (g.blockSyscallTime != 0L)
            {
                ret.SyscallTime += lastTs - g.blockSyscallTime;
            }

            if (g.blockSchedTime != 0L)
            {
                ret.SchedWaitTime += lastTs - g.blockSchedTime;
            }

            if (g.blockSweepTime != 0L)
            {
                ret.SweepTime += lastTs - g.blockSweepTime;
            }

            return ret;

        }

        // finalize is called when processing a goroutine end event or at
        // the end of trace processing. This finalizes the execution stat
        // and any active regions in the goroutine, in which case trigger is nil.
        private static void finalize(this ptr<GDesc> _addr_g, long lastTs, long activeGCStartTime, ptr<Event> _addr_trigger)
        {
            ref GDesc g = ref _addr_g.val;
            ref Event trigger = ref _addr_trigger.val;

            if (trigger != null)
            {
                g.EndTime = trigger.Ts;
            }

            var finalStat = g.snapshotStat(lastTs, activeGCStartTime);

            g.GExecutionStat = finalStat;
            foreach (var (_, s) in g.activeRegions)
            {
                s.End = trigger;
                s.GExecutionStat = finalStat.sub(s.GExecutionStat);
                g.Regions = append(g.Regions, s);
            }
            (g.gdesc).val = new gdesc();

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
            public slice<ptr<UserRegionDesc>> activeRegions; // stack of active regions
        }

        // GoroutineStats generates statistics for all goroutines in the trace.
        public static map<ulong, ptr<GDesc>> GoroutineStats(slice<ptr<Event>> events)
        {
            var gs = make_map<ulong, ptr<GDesc>>();
            long lastTs = default;
            long gcStartTime = default; // gcStartTime == 0 indicates gc is inactive.
            foreach (var (_, ev) in events)
            {
                lastTs = ev.Ts;

                if (ev.Type == EvGoCreate) 
                    ptr<GDesc> g = addr(new GDesc(ID:ev.Args[0],CreationTime:ev.Ts,gdesc:new(gdesc)));
                    g.blockSchedTime = ev.Ts; 
                    // When a goroutine is newly created, inherit the
                    // task of the active region. For ease handling of
                    // this case, we create a fake region description with
                    // the task id.
                    {
                        var creatorG = gs[ev.G];

                        if (creatorG != null && len(creatorG.gdesc.activeRegions) > 0L)
                        {
                            var regions = creatorG.gdesc.activeRegions;
                            var s = regions[len(regions) - 1L];
                            if (s.TaskID != 0L)
                            {
                                g.gdesc.activeRegions = new slice<ptr<UserRegionDesc>>(new ptr<UserRegionDesc>[] { {TaskID:s.TaskID,Start:ev} });
                            }

                        }

                    }

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
                    g.finalize(ev.Ts, gcStartTime, ev);
                else if (ev.Type == EvGoBlockSend || ev.Type == EvGoBlockRecv || ev.Type == EvGoBlockSelect || ev.Type == EvGoBlockSync || ev.Type == EvGoBlockCond) 
                    g = gs[ev.G];
                    g.ExecTime += ev.Ts - g.lastStartTime;
                    g.lastStartTime = 0L;
                    g.blockSyncTime = ev.Ts;
                else if (ev.Type == EvGoSched || ev.Type == EvGoPreempt) 
                    g = gs[ev.G];
                    g.ExecTime += ev.Ts - g.lastStartTime;
                    g.lastStartTime = 0L;
                    g.blockSchedTime = ev.Ts;
                else if (ev.Type == EvGoSleep || ev.Type == EvGoBlock) 
                    g = gs[ev.G];
                    g.ExecTime += ev.Ts - g.lastStartTime;
                    g.lastStartTime = 0L;
                else if (ev.Type == EvGoBlockNet) 
                    g = gs[ev.G];
                    g.ExecTime += ev.Ts - g.lastStartTime;
                    g.lastStartTime = 0L;
                    g.blockNetTime = ev.Ts;
                else if (ev.Type == EvGoBlockGC) 
                    g = gs[ev.G];
                    g.ExecTime += ev.Ts - g.lastStartTime;
                    g.lastStartTime = 0L;
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
                    g.lastStartTime = 0L;
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
                            if (g.EndTime != 0L)
                            {
                                continue;
                            }

                            if (gcStartTime < g.CreationTime)
                            {
                                g.GCTime += ev.Ts - g.CreationTime;
                            }
                            else
                            {
                                g.GCTime += ev.Ts - gcStartTime;
                            }

                        }

                        g = g__prev2;
                    }

                    gcStartTime = 0L; // indicates gc is inactive.
                else if (ev.Type == EvUserRegion) 
                    g = gs[ev.G];
                    {
                        var mode = ev.Args[1L];

                        switch (mode)
                        {
                            case 0L: // region start
                                g.activeRegions = append(g.activeRegions, addr(new UserRegionDesc(Name:ev.SArgs[0],TaskID:ev.Args[0],Start:ev,GExecutionStat:g.snapshotStat(lastTs,gcStartTime),)));
                                break;
                            case 1L: // region end
                                ptr<UserRegionDesc> sd;
                                {
                                    var regionStk = g.activeRegions;

                                    if (len(regionStk) > 0L)
                                    {
                                        var n = len(regionStk);
                                        sd = regionStk[n - 1L];
                                        regionStk = regionStk[..n - 1L]; // pop
                                        g.activeRegions = regionStk;

                                    }
                                    else
                                    {
                                        sd = addr(new UserRegionDesc(Name:ev.SArgs[0],TaskID:ev.Args[0],));
                                    }

                                }

                                sd.GExecutionStat = g.snapshotStat(lastTs, gcStartTime).sub(sd.GExecutionStat);
                                sd.End = ev;
                                g.Regions = append(g.Regions, sd);
                                break;
                        }
                    }
                
            }
            {
                GDesc g__prev1 = g;

                foreach (var (_, __g) in gs)
                {
                    g = __g;
                    g.finalize(lastTs, gcStartTime, null); 

                    // sort based on region start time
                    sort.Slice(g.Regions, (i, j) =>
                    {
                        var x = g.Regions[i].Start;
                        var y = g.Regions[j].Start;
                        if (x == null)
                        {
                            return true;
                        }

                        if (y == null)
                        {
                            return false;
                        }

                        return x.Ts < y.Ts;

                    });

                    g.gdesc = null;

                }

                g = g__prev1;
            }

            return gs;

        }

        // RelatedGoroutines finds a set of goroutines related to goroutine goid.
        public static map<ulong, bool> RelatedGoroutines(slice<ptr<Event>> events, ulong goid)
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
