// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file records the static ranks of the locks in the runtime. If a lock
// is not given a rank, then it is assumed to be a leaf lock, which means no other
// lock can be acquired while it is held. Therefore, leaf locks do not need to be
// given an explicit rank. We list all of the architecture-independent leaf locks
// for documentation purposes, but don't list any of the architecture-dependent
// locks (which are all leaf locks). debugLock is ignored for ranking, since it is used
// when printing out lock ranking errors.
//
// lockInit(l *mutex, rank int) is used to set the rank of lock before it is used.
// If there is no clear place to initialize a lock, then the rank of a lock can be
// specified during the lock call itself via lockWithrank(l *mutex, rank int).
//
// Besides the static lock ranking (which is a total ordering of the locks), we
// also represent and enforce the actual partial order among the locks in the
// arcs[] array below. That is, if it is possible that lock B can be acquired when
// lock A is the previous acquired lock that is still held, then there should be
// an entry for A in arcs[B][]. We will currently fail not only if the total order
// (the lock ranking) is violated, but also if there is a missing entry in the
// partial order.

// package runtime -- go2cs converted at 2020 October 09 04:46:06 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\lockrank.go

using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        private partial struct lockRank // : long
        {
        }

        // Constants representing the lock rank of the architecture-independent locks in
        // the runtime. Locks with lower rank must be taken before locks with higher
        // rank.
        private static readonly lockRank lockRankDummy = (lockRank)iota; 

        // Locks held above sched
        private static readonly var lockRankSysmon = 0;
        private static readonly var lockRankScavenge = 1;
        private static readonly var lockRankForcegc = 2;
        private static readonly var lockRankSweepWaiters = 3;
        private static readonly var lockRankAssistQueue = 4;
        private static readonly var lockRankCpuprof = 5;
        private static readonly var lockRankSweep = 6;

        private static readonly var lockRankSched = 7;
        private static readonly var lockRankDeadlock = 8;
        private static readonly var lockRankPanic = 9;
        private static readonly var lockRankAllg = 10;
        private static readonly var lockRankAllp = 11;
        private static readonly var lockRankPollDesc = 12;

        private static readonly var lockRankTimers = 13; // Multiple timers locked simultaneously in destroy()
        private static readonly var lockRankItab = 14;
        private static readonly var lockRankReflectOffs = 15;
        private static readonly var lockRankHchan = 16; // Multiple hchans acquired in lock order in syncadjustsudogs()
        private static readonly var lockRankFin = 17;
        private static readonly var lockRankNotifyList = 18;
        private static readonly var lockRankTraceBuf = 19;
        private static readonly var lockRankTraceStrings = 20;
        private static readonly var lockRankMspanSpecial = 21;
        private static readonly var lockRankProf = 22;
        private static readonly var lockRankGcBitsArenas = 23;
        private static readonly var lockRankRoot = 24;
        private static readonly var lockRankTrace = 25;
        private static readonly var lockRankTraceStackTab = 26;
        private static readonly var lockRankNetpollInit = 27;

        private static readonly var lockRankRwmutexW = 28;
        private static readonly var lockRankRwmutexR = 29;

        private static readonly var lockRankMcentral = 30; // For !go115NewMCentralImpl
        private static readonly var lockRankSpine = 31; // For !go115NewMCentralImpl
        private static readonly var lockRankSpanSetSpine = 32;
        private static readonly var lockRankGscan = 33;
        private static readonly var lockRankStackpool = 34;
        private static readonly var lockRankStackLarge = 35;
        private static readonly var lockRankDefer = 36;
        private static readonly var lockRankSudog = 37; 

        // Memory-related non-leaf locks
        private static readonly var lockRankWbufSpans = 38;
        private static readonly var lockRankMheap = 39;
        private static readonly var lockRankMheapSpecial = 40; 

        // Memory-related leaf locks
        private static readonly var lockRankGlobalAlloc = 41; 

        // Other leaf locks
        private static readonly var lockRankGFree = 42; 
        // Generally, hchan must be acquired before gscan. But in one specific
        // case (in syncadjustsudogs from markroot after the g has been suspended
        // by suspendG), we allow gscan to be acquired, and then an hchan lock. To
        // allow this case, we get this lockRankHchanLeaf rank in
        // syncadjustsudogs(), rather than lockRankHchan. By using this special
        // rank, we don't allow any further locks to be acquired other than more
        // hchan locks.
        private static readonly var lockRankHchanLeaf = 43; 

        // Leaf locks with no dependencies, so these constants are not actually used anywhere.
        // There are other architecture-dependent leaf locks as well.
        private static readonly var lockRankNewmHandoff = 44;
        private static readonly var lockRankDebugPtrmask = 45;
        private static readonly var lockRankFaketimeState = 46;
        private static readonly var lockRankTicks = 47;
        private static readonly var lockRankRaceFini = 48;
        private static readonly var lockRankPollCache = 49;
        private static readonly var lockRankDebug = 50;


        // lockRankLeafRank is the rank of lock that does not have a declared rank, and hence is
        // a leaf lock.
        private static readonly lockRank lockRankLeafRank = (lockRank)1000L;

        // lockNames gives the names associated with each of the above ranks


        // lockNames gives the names associated with each of the above ranks
        private static @string lockNames = new slice<@string>(InitKeyedValues<@string>((lockRankDummy, ""), (lockRankSysmon, "sysmon"), (lockRankScavenge, "scavenge"), (lockRankForcegc, "forcegc"), (lockRankSweepWaiters, "sweepWaiters"), (lockRankAssistQueue, "assistQueue"), (lockRankCpuprof, "cpuprof"), (lockRankSweep, "sweep"), (lockRankSched, "sched"), (lockRankDeadlock, "deadlock"), (lockRankPanic, "panic"), (lockRankAllg, "allg"), (lockRankAllp, "allp"), (lockRankPollDesc, "pollDesc"), (lockRankTimers, "timers"), (lockRankItab, "itab"), (lockRankReflectOffs, "reflectOffs"), (lockRankHchan, "hchan"), (lockRankFin, "fin"), (lockRankNotifyList, "notifyList"), (lockRankTraceBuf, "traceBuf"), (lockRankTraceStrings, "traceStrings"), (lockRankMspanSpecial, "mspanSpecial"), (lockRankProf, "prof"), (lockRankGcBitsArenas, "gcBitsArenas"), (lockRankRoot, "root"), (lockRankTrace, "trace"), (lockRankTraceStackTab, "traceStackTab"), (lockRankNetpollInit, "netpollInit"), (lockRankRwmutexW, "rwmutexW"), (lockRankRwmutexR, "rwmutexR"), (lockRankMcentral, "mcentral"), (lockRankSpine, "spine"), (lockRankSpanSetSpine, "spanSetSpine"), (lockRankGscan, "gscan"), (lockRankStackpool, "stackpool"), (lockRankStackLarge, "stackLarge"), (lockRankDefer, "defer"), (lockRankSudog, "sudog"), (lockRankWbufSpans, "wbufSpans"), (lockRankMheap, "mheap"), (lockRankMheapSpecial, "mheapSpecial"), (lockRankGlobalAlloc, "globalAlloc.mutex"), (lockRankGFree, "gFree"), (lockRankHchanLeaf, "hchanLeaf"), (lockRankNewmHandoff, "newmHandoff.lock"), (lockRankDebugPtrmask, "debugPtrmask.lock"), (lockRankFaketimeState, "faketimeState.lock"), (lockRankTicks, "ticks.lock"), (lockRankRaceFini, "raceFiniLock"), (lockRankPollCache, "pollCache.lock"), (lockRankDebug, "debugLock")));

        private static @string String(this lockRank rank)
        {
            if (rank == 0L)
            {
                return "UNKNOWN";
            }

            if (rank == lockRankLeafRank)
            {
                return "LEAF";
            }

            return lockNames[rank];

        }

        // lockPartialOrder is a partial order among the various lock types, listing the immediate
        // ordering that has actually been observed in the runtime. Each entry (which
        // corresponds to a particular lock rank) specifies the list of locks that can be
        // already be held immediately "above" it.
        //
        // So, for example, the lockRankSched entry shows that all the locks preceding it in
        // rank can actually be held. The fin lock shows that only the sched, timers, or
        // hchan lock can be held immediately above it when it is acquired.
        private static slice<slice<lockRank>> lockPartialOrder = new slice<slice<lockRank>>(InitKeyedValues<slice<lockRank>>((lockRankDummy, {}), (lockRankSysmon, {}), (lockRankScavenge, {lockRankSysmon}), (lockRankForcegc, {lockRankSysmon}), (lockRankSweepWaiters, {}), (lockRankAssistQueue, {}), (lockRankCpuprof, {}), (lockRankSweep, {}), (lockRankSched, {lockRankSysmon,lockRankScavenge,lockRankForcegc,lockRankSweepWaiters,lockRankAssistQueue,lockRankCpuprof,lockRankSweep}), (lockRankDeadlock, {lockRankDeadlock}), (lockRankPanic, {lockRankDeadlock}), (lockRankAllg, {lockRankSysmon,lockRankSched,lockRankPanic}), (lockRankAllp, {lockRankSysmon,lockRankSched}), (lockRankPollDesc, {}), (lockRankTimers, {lockRankSysmon,lockRankScavenge,lockRankSched,lockRankAllp,lockRankPollDesc,lockRankTimers}), (lockRankItab, {}), (lockRankReflectOffs, {lockRankItab}), (lockRankHchan, {lockRankScavenge,lockRankSweep,lockRankHchan}), (lockRankFin, {lockRankSysmon,lockRankScavenge,lockRankSched,lockRankAllg,lockRankTimers,lockRankHchan}), (lockRankNotifyList, {}), (lockRankTraceBuf, {lockRankSysmon,lockRankScavenge}), (lockRankTraceStrings, {lockRankTraceBuf}), (lockRankMspanSpecial, {lockRankSysmon,lockRankScavenge,lockRankAssistQueue,lockRankCpuprof,lockRankSched,lockRankAllg,lockRankAllp,lockRankTimers,lockRankItab,lockRankReflectOffs,lockRankHchan,lockRankNotifyList,lockRankTraceBuf,lockRankTraceStrings}), (lockRankProf, {lockRankSysmon,lockRankScavenge,lockRankAssistQueue,lockRankCpuprof,lockRankSweep,lockRankSched,lockRankAllg,lockRankAllp,lockRankTimers,lockRankItab,lockRankReflectOffs,lockRankNotifyList,lockRankTraceBuf,lockRankTraceStrings,lockRankHchan}), (lockRankGcBitsArenas, {lockRankSysmon,lockRankScavenge,lockRankAssistQueue,lockRankCpuprof,lockRankSched,lockRankAllg,lockRankTimers,lockRankItab,lockRankReflectOffs,lockRankNotifyList,lockRankTraceBuf,lockRankTraceStrings,lockRankHchan}), (lockRankRoot, {}), (lockRankTrace, {lockRankSysmon,lockRankScavenge,lockRankForcegc,lockRankAssistQueue,lockRankSched,lockRankHchan,lockRankTraceBuf,lockRankTraceStrings,lockRankRoot,lockRankSweep}), (lockRankTraceStackTab, {lockRankScavenge,lockRankSweepWaiters,lockRankAssistQueue,lockRankSweep,lockRankSched,lockRankAllg,lockRankTimers,lockRankHchan,lockRankFin,lockRankNotifyList,lockRankTraceBuf,lockRankTraceStrings,lockRankRoot,lockRankTrace}), (lockRankNetpollInit, {lockRankTimers}), (lockRankRwmutexW, {}), (lockRankRwmutexR, {lockRankRwmutexW}), (lockRankMcentral, {lockRankSysmon,lockRankScavenge,lockRankForcegc,lockRankAssistQueue,lockRankCpuprof,lockRankSweep,lockRankSched,lockRankAllg,lockRankAllp,lockRankTimers,lockRankItab,lockRankReflectOffs,lockRankNotifyList,lockRankTraceBuf,lockRankTraceStrings,lockRankHchan}), (lockRankSpine, {lockRankSysmon,lockRankScavenge,lockRankAssistQueue,lockRankCpuprof,lockRankSched,lockRankAllg,lockRankTimers,lockRankItab,lockRankReflectOffs,lockRankNotifyList,lockRankTraceBuf,lockRankTraceStrings,lockRankHchan}), (lockRankSpanSetSpine, {lockRankSysmon,lockRankScavenge,lockRankForcegc,lockRankAssistQueue,lockRankCpuprof,lockRankSweep,lockRankSched,lockRankAllg,lockRankAllp,lockRankTimers,lockRankItab,lockRankReflectOffs,lockRankNotifyList,lockRankTraceBuf,lockRankTraceStrings,lockRankHchan}), (lockRankGscan, {lockRankSysmon,lockRankScavenge,lockRankForcegc,lockRankSweepWaiters,lockRankAssistQueue,lockRankCpuprof,lockRankSweep,lockRankSched,lockRankTimers,lockRankItab,lockRankReflectOffs,lockRankHchan,lockRankFin,lockRankTraceBuf,lockRankTraceStrings,lockRankRoot,lockRankNotifyList,lockRankProf,lockRankGcBitsArenas,lockRankTrace,lockRankTraceStackTab,lockRankNetpollInit,lockRankMcentral,lockRankSpine,lockRankSpanSetSpine}), (lockRankStackpool, {lockRankSysmon,lockRankScavenge,lockRankSweepWaiters,lockRankAssistQueue,lockRankCpuprof,lockRankSweep,lockRankSched,lockRankPollDesc,lockRankTimers,lockRankItab,lockRankReflectOffs,lockRankHchan,lockRankFin,lockRankNotifyList,lockRankTraceBuf,lockRankTraceStrings,lockRankProf,lockRankGcBitsArenas,lockRankRoot,lockRankTrace,lockRankTraceStackTab,lockRankNetpollInit,lockRankRwmutexR,lockRankMcentral,lockRankSpine,lockRankSpanSetSpine,lockRankGscan}), (lockRankStackLarge, {lockRankSysmon,lockRankAssistQueue,lockRankSched,lockRankItab,lockRankHchan,lockRankProf,lockRankGcBitsArenas,lockRankRoot,lockRankMcentral,lockRankSpanSetSpine,lockRankGscan}), (lockRankDefer, {}), (lockRankSudog, {lockRankNotifyList,lockRankHchan}), (lockRankWbufSpans, {lockRankSysmon,lockRankScavenge,lockRankSweepWaiters,lockRankAssistQueue,lockRankSweep,lockRankSched,lockRankAllg,lockRankPollDesc,lockRankTimers,lockRankItab,lockRankReflectOffs,lockRankHchan,lockRankNotifyList,lockRankTraceStrings,lockRankMspanSpecial,lockRankProf,lockRankRoot,lockRankGscan,lockRankDefer,lockRankSudog}), (lockRankMheap, {lockRankSysmon,lockRankScavenge,lockRankSweepWaiters,lockRankAssistQueue,lockRankCpuprof,lockRankSweep,lockRankSched,lockRankAllg,lockRankAllp,lockRankPollDesc,lockRankTimers,lockRankItab,lockRankReflectOffs,lockRankNotifyList,lockRankTraceBuf,lockRankTraceStrings,lockRankHchan,lockRankMspanSpecial,lockRankProf,lockRankGcBitsArenas,lockRankRoot,lockRankMcentral,lockRankGscan,lockRankStackpool,lockRankStackLarge,lockRankDefer,lockRankSudog,lockRankWbufSpans,lockRankSpanSetSpine}), (lockRankMheapSpecial, {lockRankSysmon,lockRankScavenge,lockRankAssistQueue,lockRankCpuprof,lockRankSweep,lockRankSched,lockRankAllg,lockRankAllp,lockRankTimers,lockRankItab,lockRankReflectOffs,lockRankNotifyList,lockRankTraceBuf,lockRankTraceStrings,lockRankHchan}), (lockRankGlobalAlloc, {lockRankProf,lockRankSpine,lockRankSpanSetSpine,lockRankMheap,lockRankMheapSpecial}), (lockRankGFree, {lockRankSched}), (lockRankHchanLeaf, {lockRankGscan,lockRankHchanLeaf}), (lockRankNewmHandoff, {}), (lockRankDebugPtrmask, {}), (lockRankFaketimeState, {}), (lockRankTicks, {}), (lockRankRaceFini, {}), (lockRankPollCache, {}), (lockRankDebug, {})));
    }
}
