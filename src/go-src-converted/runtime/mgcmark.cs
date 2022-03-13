// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Garbage collector: marking and scanning

// package runtime -- go2cs converted at 2022 March 13 05:25:27 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\mgcmark.go
namespace go;

using atomic = runtime.@internal.atomic_package;
using sys = runtime.@internal.sys_package;
using @unsafe = @unsafe_package;
using System;

public static partial class runtime_package {

private static readonly var fixedRootFinalizers = iota;
private static readonly var fixedRootFreeGStacks = 0;
private static readonly rootBlockBytes fixedRootCount = 256 << 10; 

// maxObletBytes is the maximum bytes of an object to scan at
// once. Larger objects will be split up into "oblets" of at
// most this size. Since we can scan 1–2 MB/ms, 128 KB bounds
// scan preemption at ~100 µs.
//
// This must be > _MaxSmallSize so that the object base is the
// span base.
private static readonly nint maxObletBytes = 128 << 10; 

// drainCheckThreshold specifies how many units of work to do
// between self-preemption checks in gcDrain. Assuming a scan
// rate of 1 MB/ms, this is ~100 µs. Lower values have higher
// overhead in the scan loop (the scheduler check may perform
// a syscall, so its overhead is nontrivial). Higher values
// make the system less responsive to incoming work.
private static readonly nint drainCheckThreshold = 100000; 

// pagesPerSpanRoot indicates how many pages to scan from a span root
// at a time. Used by special root marking.
//
// Higher values improve throughput by increasing locality, but
// increase the minimum latency of a marking operation.
//
// Must be a multiple of the pageInUse bitmap element size and
// must also evenly divide pagesPerArena.
private static readonly nint pagesPerSpanRoot = 512;

// gcMarkRootPrepare queues root scanning jobs (stacks, globals, and
// some miscellany) and initializes scanning-related state.
//
// The world must be stopped.
private static void gcMarkRootPrepare() {
    assertWorldStopped(); 

    // Compute how many data and BSS root blocks there are.
    Func<System.UIntPtr, nint> nBlocks = bytes => int(divRoundUp(bytes, rootBlockBytes));

    work.nDataRoots = 0;
    work.nBSSRoots = 0; 

    // Scan globals.
    {
        var datap__prev1 = datap;

        foreach (var (_, __datap) in activeModules()) {
            datap = __datap;
            var nDataRoots = nBlocks(datap.edata - datap.data);
            if (nDataRoots > work.nDataRoots) {
                work.nDataRoots = nDataRoots;
            }
        }
        datap = datap__prev1;
    }

    {
        var datap__prev1 = datap;

        foreach (var (_, __datap) in activeModules()) {
            datap = __datap;
            var nBSSRoots = nBlocks(datap.ebss - datap.bss);
            if (nBSSRoots > work.nBSSRoots) {
                work.nBSSRoots = nBSSRoots;
            }
        }
        datap = datap__prev1;
    }

    mheap_.markArenas = mheap_.allArenas.slice(-1, len(mheap_.allArenas), len(mheap_.allArenas));
    work.nSpanRoots = len(mheap_.markArenas) * (pagesPerArena / pagesPerSpanRoot); 

    // Scan stacks.
    //
    // Gs may be created after this point, but it's okay that we
    // ignore them because they begin life without any roots, so
    // there's nothing to scan, and any roots they create during
    // the concurrent phase will be caught by the write barrier.
    work.nStackRoots = int(atomic.Loaduintptr(_addr_allglen));

    work.markrootNext = 0;
    work.markrootJobs = uint32(fixedRootCount + work.nDataRoots + work.nBSSRoots + work.nSpanRoots + work.nStackRoots); 

    // Calculate base indexes of each root type
    work.baseData = uint32(fixedRootCount);
    work.baseBSS = work.baseData + uint32(work.nDataRoots);
    work.baseSpans = work.baseBSS + uint32(work.nBSSRoots);
    work.baseStacks = work.baseSpans + uint32(work.nSpanRoots);
    work.baseEnd = work.baseStacks + uint32(work.nStackRoots);
}

// gcMarkRootCheck checks that all roots have been scanned. It is
// purely for debugging.
private static void gcMarkRootCheck() {
    if (work.markrootNext < work.markrootJobs) {
        print(work.markrootNext, " of ", work.markrootJobs, " markroot jobs done\n");
        throw("left over markroot jobs");
    }
    nint i = 0;
    forEachGRace(gp => {
        if (i >= work.nStackRoots) {
            return ;
        }
        if (!gp.gcscandone) {
            println("gp", gp, "goid", gp.goid, "status", readgstatus(gp), "gcscandone", gp.gcscandone);
            throw("scan missed a g");
        }
        i++;
    });
}

// ptrmask for an allocation containing a single pointer.
private static array<byte> oneptrmask = new array<byte>(new byte[] { 1 });

// markroot scans the i'th root.
//
// Preemption must be disabled (because this uses a gcWork).
//
// nowritebarrier is only advisory here.
//
//go:nowritebarrier
private static void markroot(ptr<gcWork> _addr_gcw, uint i) {
    ref gcWork gcw = ref _addr_gcw.val;
 
    // Note: if you add a case here, please also update heapdump.go:dumproots.

    if (work.baseData <= i && i < work.baseBSS) 
        {
            var datap__prev1 = datap;

            foreach (var (_, __datap) in activeModules()) {
                datap = __datap;
                markrootBlock(datap.data, datap.edata - datap.data, _addr_datap.gcdatamask.bytedata, _addr_gcw, int(i - work.baseData));
            }

            datap = datap__prev1;
        }
    else if (work.baseBSS <= i && i < work.baseSpans) 
        {
            var datap__prev1 = datap;

            foreach (var (_, __datap) in activeModules()) {
                datap = __datap;
                markrootBlock(datap.bss, datap.ebss - datap.bss, _addr_datap.gcbssmask.bytedata, _addr_gcw, int(i - work.baseBSS));
            }

            datap = datap__prev1;
        }
    else if (i == fixedRootFinalizers) 
        {
            var fb = allfin;

            while (fb != null) {
                var cnt = uintptr(atomic.Load(_addr_fb.cnt));
                scanblock(uintptr(@unsafe.Pointer(_addr_fb.fin[0])), cnt * @unsafe.Sizeof(fb.fin[0]), _addr_finptrmask[0], _addr_gcw, _addr_null);
                fb = fb.alllink;
            }

        }
    else if (i == fixedRootFreeGStacks) 
        // Switch to the system stack so we can call
        // stackfree.
        systemstack(markrootFreeGStacks);
    else if (work.baseSpans <= i && i < work.baseStacks) 
        // mark mspan.specials
        markrootSpans(_addr_gcw, int(i - work.baseSpans));
    else 
        // the rest is scanning goroutine stacks
        ptr<g> gp;
        if (work.baseStacks <= i && i < work.baseEnd) { 
            // N.B. Atomic read of allglen in gcMarkRootPrepare
            // acts as a barrier to ensure that allgs must be large
            // enough to contain all relevant Gs.
            gp = allgs[i - work.baseStacks];
        }
        else
 {
            throw("markroot: bad index");
        }
        var status = readgstatus(gp); // We are not in a scan state
        if ((status == _Gwaiting || status == _Gsyscall) && gp.waitsince == 0) {
            gp.waitsince = work.tstart;
        }
        systemstack(() => { 
            // If this is a self-scan, put the user G in
            // _Gwaiting to prevent self-deadlock. It may
            // already be in _Gwaiting if this is a mark
            // worker or we're in mark termination.
            var userG = getg().m.curg;
            var selfScan = gp == userG && readgstatus(userG) == _Grunning;
            if (selfScan) {
                casgstatus(userG, _Grunning, _Gwaiting);
                userG.waitreason = waitReasonGarbageCollectionScan;
            } 

            // TODO: suspendG blocks (and spins) until gp
            // stops, which may take a while for
            // running goroutines. Consider doing this in
            // two phases where the first is non-blocking:
            // we scan the stacks we can and ask running
            // goroutines to scan themselves; and the
            // second blocks.
            var stopped = suspendG(gp);
            if (stopped.dead) {
                gp.gcscandone = true;
                return ;
            }
            if (gp.gcscandone) {
                throw("g already scanned");
            }
            scanstack(gp, _addr_gcw);
            gp.gcscandone = true;
            resumeG(stopped);

            if (selfScan) {
                casgstatus(userG, _Gwaiting, _Grunning);
            }
        });
    }

// markrootBlock scans the shard'th shard of the block of memory [b0,
// b0+n0), with the given pointer mask.
//
//go:nowritebarrier
private static void markrootBlock(System.UIntPtr b0, System.UIntPtr n0, ptr<byte> _addr_ptrmask0, ptr<gcWork> _addr_gcw, nint shard) {
    ref byte ptrmask0 = ref _addr_ptrmask0.val;
    ref gcWork gcw = ref _addr_gcw.val;

    if (rootBlockBytes % (8 * sys.PtrSize) != 0) { 
        // This is necessary to pick byte offsets in ptrmask0.
        throw("rootBlockBytes must be a multiple of 8*ptrSize");
    }
    var off = uintptr(shard) * rootBlockBytes;
    if (off >= n0) {
        return ;
    }
    var b = b0 + off;
    var ptrmask = (uint8.val)(add(@unsafe.Pointer(ptrmask0), uintptr(shard) * (rootBlockBytes / (8 * sys.PtrSize))));
    var n = uintptr(rootBlockBytes);
    if (off + n > n0) {
        n = n0 - off;
    }
    scanblock(b, n, _addr_ptrmask, _addr_gcw, _addr_null);
}

// markrootFreeGStacks frees stacks of dead Gs.
//
// This does not free stacks of dead Gs cached on Ps, but having a few
// cached stacks around isn't a problem.
private static void markrootFreeGStacks() { 
    // Take list of dead Gs with stacks.
    lock(_addr_sched.gFree.@lock);
    var list = sched.gFree.stack;
    sched.gFree.stack = new gList();
    unlock(_addr_sched.gFree.@lock);
    if (list.empty()) {
        return ;
    }
    gQueue q = new gQueue(list.head,list.head);
    {
        var gp = list.head.ptr();

        while (gp != null) {
            stackfree(gp.stack);
            gp.stack.lo = 0;
            gp.stack.hi = 0; 
            // Manipulate the queue directly since the Gs are
            // already all linked the right way.
            q.tail.set(gp);
            gp = gp.schedlink.ptr();
        }
    } 

    // Put Gs back on the free list.
    lock(_addr_sched.gFree.@lock);
    sched.gFree.noStack.pushAll(q);
    unlock(_addr_sched.gFree.@lock);
}

// markrootSpans marks roots for one shard of markArenas.
//
//go:nowritebarrier
private static void markrootSpans(ptr<gcWork> _addr_gcw, nint shard) {
    ref gcWork gcw = ref _addr_gcw.val;
 
    // Objects with finalizers have two GC-related invariants:
    //
    // 1) Everything reachable from the object must be marked.
    // This ensures that when we pass the object to its finalizer,
    // everything the finalizer can reach will be retained.
    //
    // 2) Finalizer specials (which are not in the garbage
    // collected heap) are roots. In practice, this means the fn
    // field must be scanned.
    var sg = mheap_.sweepgen; 

    // Find the arena and page index into that arena for this shard.
    var ai = mheap_.markArenas[shard / (pagesPerArena / pagesPerSpanRoot)];
    var ha = mheap_.arenas[ai.l1()][ai.l2()];
    var arenaPage = uint(uintptr(shard) * pagesPerSpanRoot % pagesPerArena); 

    // Construct slice of bitmap which we'll iterate over.
    var specialsbits = ha.pageSpecials[(int)arenaPage / 8..];
    specialsbits = specialsbits[..(int)pagesPerSpanRoot / 8];
    foreach (var (i) in specialsbits) { 
        // Find set bits, which correspond to spans with specials.
        var specials = atomic.Load8(_addr_specialsbits[i]);
        if (specials == 0) {
            continue;
        }
        for (var j = uint(0); j < 8; j++) {
            if (specials & (1 << (int)(j)) == 0) {
                continue;
            } 
            // Find the span for this bit.
            //
            // This value is guaranteed to be non-nil because having
            // specials implies that the span is in-use, and since we're
            // currently marking we can be sure that we don't have to worry
            // about the span being freed and re-used.
            var s = ha.spans[arenaPage + uint(i) * 8 + j]; 

            // The state must be mSpanInUse if the specials bit is set, so
            // sanity check that.
            {
                var state = s.state.get();

                if (state != mSpanInUse) {
                    print("s.state = ", state, "\n");
                    throw("non in-use span found with specials bit set");
                } 
                // Check that this span was swept (it may be cached or uncached).

            } 
            // Check that this span was swept (it may be cached or uncached).
            if (!useCheckmark && !(s.sweepgen == sg || s.sweepgen == sg + 3)) { 
                // sweepgen was updated (+2) during non-checkmark GC pass
                print("sweep ", s.sweepgen, " ", sg, "\n");
                throw("gc: unswept span");
            } 

            // Lock the specials to prevent a special from being
            // removed from the list while we're traversing it.
            lock(_addr_s.speciallock);
            {
                var sp = s.specials;

                while (sp != null) {
                    if (sp.kind != _KindSpecialFinalizer) {
                        continue;
                    sp = sp.next;
                    } 
                    // don't mark finalized object, but scan it so we
                    // retain everything it points to.
                    var spf = (specialfinalizer.val)(@unsafe.Pointer(sp)); 
                    // A finalizer can be set for an inner byte of an object, find object beginning.
                    var p = s.@base() + uintptr(spf.special.offset) / s.elemsize * s.elemsize; 

                    // Mark everything that can be reached from
                    // the object (but *not* the object itself or
                    // we'll never collect it).
                    scanobject(p, _addr_gcw); 

                    // The special itself is a root.
                    scanblock(uintptr(@unsafe.Pointer(_addr_spf.fn)), sys.PtrSize, _addr_oneptrmask[0], _addr_gcw, _addr_null);
                }

            }
            unlock(_addr_s.speciallock);
        }
    }
}

// gcAssistAlloc performs GC work to make gp's assist debt positive.
// gp must be the calling user gorountine.
//
// This must be called with preemption enabled.
private static void gcAssistAlloc(ptr<g> _addr_gp) {
    ref g gp = ref _addr_gp.val;
 
    // Don't assist in non-preemptible contexts. These are
    // generally fragile and won't allow the assist to block.
    if (getg() == gp.m.g0) {
        return ;
    }
    {
        var mp = getg().m;

        if (mp.locks > 0 || mp.preemptoff != "") {
            return ;
        }
    }

    var traced = false;
retry:
    var assistWorkPerByte = float64frombits(atomic.Load64(_addr_gcController.assistWorkPerByte));
    var assistBytesPerWork = float64frombits(atomic.Load64(_addr_gcController.assistBytesPerWork));
    var debtBytes = -gp.gcAssistBytes;
    var scanWork = int64(assistWorkPerByte * float64(debtBytes));
    if (scanWork < gcOverAssistWork) {
        scanWork = gcOverAssistWork;
        debtBytes = int64(assistBytesPerWork * float64(scanWork));
    }
    var bgScanCredit = atomic.Loadint64(_addr_gcController.bgScanCredit);
    var stolen = int64(0);
    if (bgScanCredit > 0) {
        if (bgScanCredit < scanWork) {
            stolen = bgScanCredit;
            gp.gcAssistBytes += 1 + int64(assistBytesPerWork * float64(stolen));
        }
        else
 {
            stolen = scanWork;
            gp.gcAssistBytes += debtBytes;
        }
        atomic.Xaddint64(_addr_gcController.bgScanCredit, -stolen);

        scanWork -= stolen;

        if (scanWork == 0) { 
            // We were able to steal all of the credit we
            // needed.
            if (traced) {
                traceGCMarkAssistDone();
            }
            return ;
        }
    }
    if (trace.enabled && !traced) {
        traced = true;
        traceGCMarkAssistStart();
    }
    systemstack(() => {
        gcAssistAlloc1(_addr_gp, scanWork); 
        // The user stack may have moved, so this can't touch
        // anything on it until it returns from systemstack.
    });

    var completed = gp.param != null;
    gp.param = null;
    if (completed) {
        gcMarkDone();
    }
    if (gp.gcAssistBytes < 0) { 
        // We were unable steal enough credit or perform
        // enough work to pay off the assist debt. We need to
        // do one of these before letting the mutator allocate
        // more to prevent over-allocation.
        //
        // If this is because we were preempted, reschedule
        // and try some more.
        if (gp.preempt) {
            Gosched();
            goto retry;
        }
        if (!gcParkAssist()) {
            goto retry;
        }
    }
    if (traced) {
        traceGCMarkAssistDone();
    }
}

// gcAssistAlloc1 is the part of gcAssistAlloc that runs on the system
// stack. This is a separate function to make it easier to see that
// we're not capturing anything from the user stack, since the user
// stack may move while we're in this function.
//
// gcAssistAlloc1 indicates whether this assist completed the mark
// phase by setting gp.param to non-nil. This can't be communicated on
// the stack since it may move.
//
//go:systemstack
private static void gcAssistAlloc1(ptr<g> _addr_gp, long scanWork) {
    ref g gp = ref _addr_gp.val;
 
    // Clear the flag indicating that this assist completed the
    // mark phase.
    gp.param = null;

    if (atomic.Load(_addr_gcBlackenEnabled) == 0) { 
        // The gcBlackenEnabled check in malloc races with the
        // store that clears it but an atomic check in every malloc
        // would be a performance hit.
        // Instead we recheck it here on the non-preemptable system
        // stack to determine if we should perform an assist.

        // GC is done, so ignore any remaining debt.
        gp.gcAssistBytes = 0;
        return ;
    }
    var startTime = nanotime();

    var decnwait = atomic.Xadd(_addr_work.nwait, -1);
    if (decnwait == work.nproc) {
        println("runtime: work.nwait =", decnwait, "work.nproc=", work.nproc);
        throw("nwait > work.nprocs");
    }
    casgstatus(gp, _Grunning, _Gwaiting);
    gp.waitreason = waitReasonGCAssistMarking; 

    // drain own cached work first in the hopes that it
    // will be more cache friendly.
    var gcw = _addr_getg().m.p.ptr().gcw;
    var workDone = gcDrainN(_addr_gcw, scanWork);

    casgstatus(gp, _Gwaiting, _Grunning); 

    // Record that we did this much scan work.
    //
    // Back out the number of bytes of assist credit that
    // this scan work counts for. The "1+" is a poor man's
    // round-up, to ensure this adds credit even if
    // assistBytesPerWork is very low.
    var assistBytesPerWork = float64frombits(atomic.Load64(_addr_gcController.assistBytesPerWork));
    gp.gcAssistBytes += 1 + int64(assistBytesPerWork * float64(workDone)); 

    // If this is the last worker and we ran out of work,
    // signal a completion point.
    var incnwait = atomic.Xadd(_addr_work.nwait, +1);
    if (incnwait > work.nproc) {
        println("runtime: work.nwait=", incnwait, "work.nproc=", work.nproc);
        throw("work.nwait > work.nproc");
    }
    if (incnwait == work.nproc && !gcMarkWorkAvailable(null)) { 
        // This has reached a background completion point. Set
        // gp.param to a non-nil value to indicate this. It
        // doesn't matter what we set it to (it just has to be
        // a valid pointer).
        gp.param = @unsafe.Pointer(gp);
    }
    var duration = nanotime() - startTime;
    var _p_ = gp.m.p.ptr();
    _p_.gcAssistTime += duration;
    if (_p_.gcAssistTime > gcAssistTimeSlack) {
        atomic.Xaddint64(_addr_gcController.assistTime, _p_.gcAssistTime);
        _p_.gcAssistTime = 0;
    }
}

// gcWakeAllAssists wakes all currently blocked assists. This is used
// at the end of a GC cycle. gcBlackenEnabled must be false to prevent
// new assists from going to sleep after this point.
private static void gcWakeAllAssists() {
    lock(_addr_work.assistQueue.@lock);
    ref var list = ref heap(work.assistQueue.q.popList(), out ptr<var> _addr_list);
    injectglist(_addr_list);
    unlock(_addr_work.assistQueue.@lock);
}

// gcParkAssist puts the current goroutine on the assist queue and parks.
//
// gcParkAssist reports whether the assist is now satisfied. If it
// returns false, the caller must retry the assist.
//
//go:nowritebarrier
private static bool gcParkAssist() {
    lock(_addr_work.assistQueue.@lock); 
    // If the GC cycle finished while we were getting the lock,
    // exit the assist. The cycle can't finish while we hold the
    // lock.
    if (atomic.Load(_addr_gcBlackenEnabled) == 0) {
        unlock(_addr_work.assistQueue.@lock);
        return true;
    }
    var gp = getg();
    var oldList = work.assistQueue.q;
    work.assistQueue.q.pushBack(gp); 

    // Recheck for background credit now that this G is in
    // the queue, but can still back out. This avoids a
    // race in case background marking has flushed more
    // credit since we checked above.
    if (atomic.Loadint64(_addr_gcController.bgScanCredit) > 0) {
        work.assistQueue.q = oldList;
        if (oldList.tail != 0) {
            oldList.tail.ptr().schedlink.set(null);
        }
        unlock(_addr_work.assistQueue.@lock);
        return false;
    }
    goparkunlock(_addr_work.assistQueue.@lock, waitReasonGCAssistWait, traceEvGoBlockGC, 2);
    return true;
}

// gcFlushBgCredit flushes scanWork units of background scan work
// credit. This first satisfies blocked assists on the
// work.assistQueue and then flushes any remaining credit to
// gcController.bgScanCredit.
//
// Write barriers are disallowed because this is used by gcDrain after
// it has ensured that all work is drained and this must preserve that
// condition.
//
//go:nowritebarrierrec
private static void gcFlushBgCredit(long scanWork) {
    if (work.assistQueue.q.empty()) { 
        // Fast path; there are no blocked assists. There's a
        // small window here where an assist may add itself to
        // the blocked queue and park. If that happens, we'll
        // just get it on the next flush.
        atomic.Xaddint64(_addr_gcController.bgScanCredit, scanWork);
        return ;
    }
    var assistBytesPerWork = float64frombits(atomic.Load64(_addr_gcController.assistBytesPerWork));
    var scanBytes = int64(float64(scanWork) * assistBytesPerWork);

    lock(_addr_work.assistQueue.@lock);
    while (!work.assistQueue.q.empty() && scanBytes > 0) {
        var gp = work.assistQueue.q.pop(); 
        // Note that gp.gcAssistBytes is negative because gp
        // is in debt. Think carefully about the signs below.
        if (scanBytes + gp.gcAssistBytes >= 0) { 
            // Satisfy this entire assist debt.
            scanBytes += gp.gcAssistBytes;
            gp.gcAssistBytes = 0; 
            // It's important that we *not* put gp in
            // runnext. Otherwise, it's possible for user
            // code to exploit the GC worker's high
            // scheduler priority to get itself always run
            // before other goroutines and always in the
            // fresh quantum started by GC.
            ready(gp, 0, false);
        }
        else
 { 
            // Partially satisfy this assist.
            gp.gcAssistBytes += scanBytes;
            scanBytes = 0; 
            // As a heuristic, we move this assist to the
            // back of the queue so that large assists
            // can't clog up the assist queue and
            // substantially delay small assists.
            work.assistQueue.q.pushBack(gp);
            break;
        }
    }

    if (scanBytes > 0) { 
        // Convert from scan bytes back to work.
        var assistWorkPerByte = float64frombits(atomic.Load64(_addr_gcController.assistWorkPerByte));
        scanWork = int64(float64(scanBytes) * assistWorkPerByte);
        atomic.Xaddint64(_addr_gcController.bgScanCredit, scanWork);
    }
    unlock(_addr_work.assistQueue.@lock);
}

// scanstack scans gp's stack, greying all pointers found on the stack.
//
// scanstack will also shrink the stack if it is safe to do so. If it
// is not, it schedules a stack shrink for the next synchronous safe
// point.
//
// scanstack is marked go:systemstack because it must not be preempted
// while using a workbuf.
//
//go:nowritebarrier
//go:systemstack
private static void scanstack(ptr<g> _addr_gp, ptr<gcWork> _addr_gcw) {
    ref g gp = ref _addr_gp.val;
    ref gcWork gcw = ref _addr_gcw.val;

    if (readgstatus(gp) & _Gscan == 0) {
        print("runtime:scanstack: gp=", gp, ", goid=", gp.goid, ", gp->atomicstatus=", hex(readgstatus(gp)), "\n");
        throw("scanstack - bad status");
    }

    if (readgstatus(gp) & ~_Gscan == _Gdead) 
        return ;
    else if (readgstatus(gp) & ~_Gscan == _Grunning) 
        print("runtime: gp=", gp, ", goid=", gp.goid, ", gp->atomicstatus=", readgstatus(gp), "\n");
        throw("scanstack: goroutine not stopped");
    else if (readgstatus(gp) & ~_Gscan == _Grunnable || readgstatus(gp) & ~_Gscan == _Gsyscall || readgstatus(gp) & ~_Gscan == _Gwaiting)     else 
        print("runtime: gp=", gp, ", goid=", gp.goid, ", gp->atomicstatus=", readgstatus(gp), "\n");
        throw("mark - bad status");
        if (gp == getg()) {
        throw("can't scan our own stack");
    }
    if (isShrinkStackSafe(gp)) { 
        // Shrink the stack if not much of it is being used.
        shrinkstack(gp);
    }
    else
 { 
        // Otherwise, shrink the stack at the next sync safe point.
        gp.preemptShrink = true;
    }
    ref stackScanState state = ref heap(out ptr<stackScanState> _addr_state);
    state.stack = gp.stack;

    if (stackTraceDebug) {
        println("stack trace goroutine", gp.goid);
    }
    if (debugScanConservative && gp.asyncSafePoint) {
        print("scanning async preempted goroutine ", gp.goid, " stack [", hex(gp.stack.lo), ",", hex(gp.stack.hi), ")\n");
    }
    if (gp.sched.ctxt != null) {
        scanblock(uintptr(@unsafe.Pointer(_addr_gp.sched.ctxt)), sys.PtrSize, _addr_oneptrmask[0], _addr_gcw, _addr_state);
    }
    Func<ptr<stkframe>, unsafe.Pointer, bool> scanframe = (frame, unused) => {
        scanframeworker(_addr_frame, _addr_state, _addr_gcw);
        return true;
    };
    gentraceback(~uintptr(0), ~uintptr(0), 0, gp, 0, null, 0x7fffffff, scanframe, null, 0); 

    // Find additional pointers that point into the stack from the heap.
    // Currently this includes defers and panics. See also function copystack.

    // Find and trace all defer arguments.
    tracebackdefers(gp, scanframe, null); 

    // Find and trace other pointers in defer records.
    {
        ref var d = ref heap(gp._defer, out ptr<var> _addr_d);

        while (d != null) {
            if (d.fn != null) { 
                // tracebackdefers above does not scan the func value, which could
                // be a stack allocated closure. See issue 30453.
                scanblock(uintptr(@unsafe.Pointer(_addr_d.fn)), sys.PtrSize, _addr_oneptrmask[0], _addr_gcw, _addr_state);
            d = d.link;
            }
            if (d.link != null) { 
                // The link field of a stack-allocated defer record might point
                // to a heap-allocated defer record. Keep that heap record live.
                scanblock(uintptr(@unsafe.Pointer(_addr_d.link)), sys.PtrSize, _addr_oneptrmask[0], _addr_gcw, _addr_state);
            } 
            // Retain defers records themselves.
            // Defer records might not be reachable from the G through regular heap
            // tracing because the defer linked list might weave between the stack and the heap.
            if (d.heap) {
                scanblock(uintptr(@unsafe.Pointer(_addr_d)), sys.PtrSize, _addr_oneptrmask[0], _addr_gcw, _addr_state);
            }
        }
    }
    if (gp._panic != null) { 
        // Panics are always stack allocated.
        state.putPtr(uintptr(@unsafe.Pointer(gp._panic)), false);
    }
    state.buildIndex();
    while (true) {
        var (p, conservative) = state.getPtr();
        if (p == 0) {
            break;
        }
        var obj = state.findObject(p);
        if (obj == null) {
            continue;
        }
        var r = obj.r;
        if (r == null) { 
            // We've already scanned this object.
            continue;
        }
        obj.setRecord(null); // Don't scan it again.
        if (stackTraceDebug) {
            printlock();
            print("  live stkobj at", hex(state.stack.lo + uintptr(obj.off)), "of size", obj.size);
            if (conservative) {
                print(" (conservative)");
            }
            println();
            printunlock();
        }
        var gcdata = r.gcdata;
        ptr<mspan> s;
        if (r.useGCProg()) { 
            // This path is pretty unlikely, an object large enough
            // to have a GC program allocated on the stack.
            // We need some space to unpack the program into a straight
            // bitmask, which we allocate/free here.
            // TODO: it would be nice if there were a way to run a GC
            // program without having to store all its bits. We'd have
            // to change from a Lempel-Ziv style program to something else.
            // Or we can forbid putting objects on stacks if they require
            // a gc program (see issue 27447).
            s = materializeGCProg(r.ptrdata(), gcdata);
            gcdata = (byte.val)(@unsafe.Pointer(s.startAddr));
        }
        var b = state.stack.lo + uintptr(obj.off);
        if (conservative) {
            scanConservative(b, r.ptrdata(), _addr_gcdata, _addr_gcw, _addr_state);
        }
        else
 {
            scanblock(b, r.ptrdata(), _addr_gcdata, _addr_gcw, _addr_state);
        }
        if (s != null) {
            dematerializeGCProg(s);
        }
    } 

    // Deallocate object buffers.
    // (Pointer buffers were all deallocated in the loop above.)
    while (state.head != null) {
        var x = state.head;
        state.head = x.next;
        if (stackTraceDebug) {
            for (nint i = 0; i < x.nobj; i++) {
                obj = _addr_x.obj[i];
                if (obj.r == null) { // reachable
                    continue;
                }
                println("  dead stkobj at", hex(gp.stack.lo + uintptr(obj.off)), "of size", obj.r.size); 
                // Note: not necessarily really dead - only reachable-from-ptr dead.
            }
        }
        x.nobj = 0;
        putempty((workbuf.val)(@unsafe.Pointer(x)));
    }
    if (state.buf != null || state.cbuf != null || state.freeBuf != null) {
        throw("remaining pointer buffers");
    }
}

// Scan a stack frame: local variables and function arguments/results.
//go:nowritebarrier
private static void scanframeworker(ptr<stkframe> _addr_frame, ptr<stackScanState> _addr_state, ptr<gcWork> _addr_gcw) {
    ref stkframe frame = ref _addr_frame.val;
    ref stackScanState state = ref _addr_state.val;
    ref gcWork gcw = ref _addr_gcw.val;

    if (_DebugGC > 1 && frame.continpc != 0) {
        print("scanframe ", funcname(frame.fn), "\n");
    }
    var isAsyncPreempt = frame.fn.valid() && frame.fn.funcID == funcID_asyncPreempt;
    var isDebugCall = frame.fn.valid() && frame.fn.funcID == funcID_debugCallV2;
    if (state.conservative || isAsyncPreempt || isDebugCall) {
        if (debugScanConservative) {
            println("conservatively scanning function", funcname(frame.fn), "at PC", hex(frame.continpc));
        }
        if (frame.varp != 0) {
            var size = frame.varp - frame.sp;
            if (size > 0) {
                scanConservative(frame.sp, size, _addr_null, _addr_gcw, _addr_state);
            }
        }
        if (frame.arglen != 0) { 
            // TODO: We could pass the entry argument map
            // to narrow this down further.
            scanConservative(frame.argp, frame.arglen, _addr_null, _addr_gcw, _addr_state);
        }
        if (isAsyncPreempt || isDebugCall) { 
            // This function's frame contained the
            // registers for the asynchronously stopped
            // parent frame. Scan the parent
            // conservatively.
            state.conservative = true;
        }
        else
 { 
            // We only wanted to scan those two frames
            // conservatively. Clear the flag for future
            // frames.
            state.conservative = false;
        }
        return ;
    }
    var (locals, args, objs) = getStackMap(frame, _addr_state.cache, false); 

    // Scan local variables if stack frame has been allocated.
    if (locals.n > 0) {
        size = uintptr(locals.n) * sys.PtrSize;
        scanblock(frame.varp - size, size, _addr_locals.bytedata, _addr_gcw, _addr_state);
    }
    if (args.n > 0) {
        scanblock(frame.argp, uintptr(args.n) * sys.PtrSize, _addr_args.bytedata, _addr_gcw, _addr_state);
    }
    if (frame.varp != 0) { 
        // varp is 0 for defers, where there are no locals.
        // In that case, there can't be a pointer to its args, either.
        // (And all args would be scanned above anyway.)
        foreach (var (i, obj) in objs) {
            var off = obj.off;
            var @base = frame.varp; // locals base pointer
            if (off >= 0) {
                base = frame.argp; // arguments and return values base pointer
            }
            var ptr = base + uintptr(off);
            if (ptr < frame.sp) { 
                // object hasn't been allocated in the frame yet.
                continue;
            }
            if (stackTraceDebug) {
                println("stkobj at", hex(ptr), "of size", obj.size);
            }
            state.addObject(ptr, _addr_objs[i]);
        }
    }
}

private partial struct gcDrainFlags { // : nint
}

private static readonly gcDrainFlags gcDrainUntilPreempt = 1 << (int)(iota);
private static readonly var gcDrainFlushBgCredit = 0;
private static readonly var gcDrainIdle = 1;
private static readonly var gcDrainFractional = 2;

// gcDrain scans roots and objects in work buffers, blackening grey
// objects until it is unable to get more work. It may return before
// GC is done; it's the caller's responsibility to balance work from
// other Ps.
//
// If flags&gcDrainUntilPreempt != 0, gcDrain returns when g.preempt
// is set.
//
// If flags&gcDrainIdle != 0, gcDrain returns when there is other work
// to do.
//
// If flags&gcDrainFractional != 0, gcDrain self-preempts when
// pollFractionalWorkerExit() returns true. This implies
// gcDrainNoBlock.
//
// If flags&gcDrainFlushBgCredit != 0, gcDrain flushes scan work
// credit to gcController.bgScanCredit every gcCreditSlack units of
// scan work.
//
// gcDrain will always return if there is a pending STW.
//
//go:nowritebarrier
private static void gcDrain(ptr<gcWork> _addr_gcw, gcDrainFlags flags) {
    ref gcWork gcw = ref _addr_gcw.val;

    if (!writeBarrier.needed) {
        throw("gcDrain phase incorrect");
    }
    var gp = getg().m.curg;
    var preemptible = flags & gcDrainUntilPreempt != 0;
    var flushBgCredit = flags & gcDrainFlushBgCredit != 0;
    var idle = flags & gcDrainIdle != 0;

    var initScanWork = gcw.scanWork; 

    // checkWork is the scan work before performing the next
    // self-preempt check.
    var checkWork = int64(1 << 63 - 1);
    Func<bool> check = default;
    if (flags & (gcDrainIdle | gcDrainFractional) != 0) {
        checkWork = initScanWork + drainCheckThreshold;
        if (idle) {
            check = pollWork;
        }
        else if (flags & gcDrainFractional != 0) {
            check = pollFractionalWorkerExit;
        }
    }
    if (work.markrootNext < work.markrootJobs) { 
        // Stop if we're preemptible or if someone wants to STW.
        while (!(gp.preempt && (preemptible || atomic.Load(_addr_sched.gcwaiting) != 0))) {
            var job = atomic.Xadd(_addr_work.markrootNext, +1) - 1;
            if (job >= work.markrootJobs) {
                break;
            }
            markroot(_addr_gcw, job);
            if (check != null && check()) {
                goto done;
            }
        }
    }
    while (!(gp.preempt && (preemptible || atomic.Load(_addr_sched.gcwaiting) != 0))) { 
        // Try to keep work available on the global queue. We used to
        // check if there were waiting workers, but it's better to
        // just keep work available than to make workers wait. In the
        // worst case, we'll do O(log(_WorkbufSize)) unnecessary
        // balances.
        if (work.full == 0) {
            gcw.balance();
        }
        var b = gcw.tryGetFast();
        if (b == 0) {
            b = gcw.tryGet();
            if (b == 0) { 
                // Flush the write barrier
                // buffer; this may create
                // more work.
                wbBufFlush(null, 0);
                b = gcw.tryGet();
            }
        }
        if (b == 0) { 
            // Unable to get work.
            break;
        }
        scanobject(b, _addr_gcw); 

        // Flush background scan work credit to the global
        // account if we've accumulated enough locally so
        // mutator assists can draw on it.
        if (gcw.scanWork >= gcCreditSlack) {
            atomic.Xaddint64(_addr_gcController.scanWork, gcw.scanWork);
            if (flushBgCredit) {
                gcFlushBgCredit(gcw.scanWork - initScanWork);
                initScanWork = 0;
            }
            checkWork -= gcw.scanWork;
            gcw.scanWork = 0;

            if (checkWork <= 0) {
                checkWork += drainCheckThreshold;
                if (check != null && check()) {
                    break;
                }
            }
        }
    }

done:
    if (gcw.scanWork > 0) {
        atomic.Xaddint64(_addr_gcController.scanWork, gcw.scanWork);
        if (flushBgCredit) {
            gcFlushBgCredit(gcw.scanWork - initScanWork);
        }
        gcw.scanWork = 0;
    }
}

// gcDrainN blackens grey objects until it has performed roughly
// scanWork units of scan work or the G is preempted. This is
// best-effort, so it may perform less work if it fails to get a work
// buffer. Otherwise, it will perform at least n units of work, but
// may perform more because scanning is always done in whole object
// increments. It returns the amount of scan work performed.
//
// The caller goroutine must be in a preemptible state (e.g.,
// _Gwaiting) to prevent deadlocks during stack scanning. As a
// consequence, this must be called on the system stack.
//
//go:nowritebarrier
//go:systemstack
private static long gcDrainN(ptr<gcWork> _addr_gcw, long scanWork) {
    ref gcWork gcw = ref _addr_gcw.val;

    if (!writeBarrier.needed) {
        throw("gcDrainN phase incorrect");
    }
    var workFlushed = -gcw.scanWork;

    var gp = getg().m.curg;
    while (!gp.preempt && workFlushed + gcw.scanWork < scanWork) { 
        // See gcDrain comment.
        if (work.full == 0) {
            gcw.balance();
        }
        var b = gcw.tryGetFast();
        if (b == 0) {
            b = gcw.tryGet();
            if (b == 0) { 
                // Flush the write barrier buffer;
                // this may create more work.
                wbBufFlush(null, 0);
                b = gcw.tryGet();
            }
        }
        if (b == 0) { 
            // Try to do a root job.
            //
            // TODO: Assists should get credit for this
            // work.
            if (work.markrootNext < work.markrootJobs) {
                var job = atomic.Xadd(_addr_work.markrootNext, +1) - 1;
                if (job < work.markrootJobs) {
                    markroot(_addr_gcw, job);
                    continue;
                }
            } 
            // No heap or root jobs.
            break;
        }
        scanobject(b, _addr_gcw); 

        // Flush background scan work credit.
        if (gcw.scanWork >= gcCreditSlack) {
            atomic.Xaddint64(_addr_gcController.scanWork, gcw.scanWork);
            workFlushed += gcw.scanWork;
            gcw.scanWork = 0;
        }
    } 

    // Unlike gcDrain, there's no need to flush remaining work
    // here because this never flushes to bgScanCredit and
    // gcw.dispose will flush any remaining work to scanWork.

    return workFlushed + gcw.scanWork;
}

// scanblock scans b as scanobject would, but using an explicit
// pointer bitmap instead of the heap bitmap.
//
// This is used to scan non-heap roots, so it does not update
// gcw.bytesMarked or gcw.scanWork.
//
// If stk != nil, possible stack pointers are also reported to stk.putPtr.
//go:nowritebarrier
private static void scanblock(System.UIntPtr b0, System.UIntPtr n0, ptr<byte> _addr_ptrmask, ptr<gcWork> _addr_gcw, ptr<stackScanState> _addr_stk) {
    ref byte ptrmask = ref _addr_ptrmask.val;
    ref gcWork gcw = ref _addr_gcw.val;
    ref stackScanState stk = ref _addr_stk.val;
 
    // Use local copies of original parameters, so that a stack trace
    // due to one of the throws below shows the original block
    // base and extent.
    var b = b0;
    var n = n0;

    {
        var i = uintptr(0);

        while (i < n) { 
            // Find bits for the next word.
            var bits = uint32(addb(ptrmask, i / (sys.PtrSize * 8)).val);
            if (bits == 0) {
                i += sys.PtrSize * 8;
                continue;
            }
            for (nint j = 0; j < 8 && i < n; j++) {
                if (bits & 1 != 0) { 
                    // Same work as in scanobject; see comments there.
                    ptr<ptr<System.UIntPtr>> p = new ptr<ptr<ptr<System.UIntPtr>>>(@unsafe.Pointer(b + i));
                    if (p != 0) {
                        {
                            var (obj, span, objIndex) = findObject(p, b, i);

                            if (obj != 0) {
                                greyobject(obj, b, i, _addr_span, _addr_gcw, objIndex);
                            }
                            else if (stk != null && p >= stk.stack.lo && p < stk.stack.hi) {
                                stk.putPtr(p, false);
                            }

                        }
                    }
                }
                bits>>=1;
                i += sys.PtrSize;
            }
        }
    }
}

// scanobject scans the object starting at b, adding pointers to gcw.
// b must point to the beginning of a heap object or an oblet.
// scanobject consults the GC bitmap for the pointer mask and the
// spans for the size of the object.
//
//go:nowritebarrier
private static void scanobject(System.UIntPtr b, ptr<gcWork> _addr_gcw) {
    ref gcWork gcw = ref _addr_gcw.val;
 
    // Find the bits for b and the size of the object at b.
    //
    // b is either the beginning of an object, in which case this
    // is the size of the object to scan, or it points to an
    // oblet, in which case we compute the size to scan below.
    var hbits = heapBitsForAddr(b);
    var s = spanOfUnchecked(b);
    var n = s.elemsize;
    if (n == 0) {
        throw("scanobject n == 0");
    }
    if (n > maxObletBytes) { 
        // Large object. Break into oblets for better
        // parallelism and lower latency.
        if (b == s.@base()) { 
            // It's possible this is a noscan object (not
            // from greyobject, but from other code
            // paths), in which case we must *not* enqueue
            // oblets since their bitmaps will be
            // uninitialized.
            if (s.spanclass.noscan()) { 
                // Bypass the whole scan.
                gcw.bytesMarked += uint64(n);
                return ;
            } 

            // Enqueue the other oblets to scan later.
            // Some oblets may be in b's scalar tail, but
            // these will be marked as "no more pointers",
            // so we'll drop out immediately when we go to
            // scan those.
            {
                var oblet = b + maxObletBytes;

                while (oblet < s.@base() + s.elemsize) {
                    if (!gcw.putFast(oblet)) {
                        gcw.put(oblet);
                    oblet += maxObletBytes;
                    }
                }

            }
        }
        n = s.@base() + s.elemsize - b;
        if (n > maxObletBytes) {
            n = maxObletBytes;
        }
    }
    System.UIntPtr i = default;
    i = 0;

    while (i < n) { 
        // Load bits once. See CL 22712 and issue 16973 for discussion.
        var bits = hbits.bits();
        if (bits & bitScan == 0) {
            break; // no more pointers in this object
        (i, hbits) = (i + sys.PtrSize, hbits.next());
        }
        if (bits & bitPointer == 0) {
            continue; // not a pointer
        }
        ptr<ptr<System.UIntPtr>> obj = new ptr<ptr<ptr<System.UIntPtr>>>(@unsafe.Pointer(b + i)); 

        // At this point we have extracted the next potential pointer.
        // Quickly filter out nil and pointers back to the current object.
        if (obj != 0 && obj - b >= n) { 
            // Test if obj points into the Go heap and, if so,
            // mark the object.
            //
            // Note that it's possible for findObject to
            // fail if obj points to a just-allocated heap
            // object because of a race with growing the
            // heap. In this case, we know the object was
            // just allocated and hence will be marked by
            // allocation itself.
            {
                ptr<ptr<System.UIntPtr>> obj__prev2 = obj;

                var (obj, span, objIndex) = findObject(obj, b, i);

                if (obj != 0) {
                    greyobject(obj, b, i, _addr_span, _addr_gcw, objIndex);
                }

                obj = obj__prev2;

            }
        }
    }
    gcw.bytesMarked += uint64(n);
    gcw.scanWork += int64(i);
}

// scanConservative scans block [b, b+n) conservatively, treating any
// pointer-like value in the block as a pointer.
//
// If ptrmask != nil, only words that are marked in ptrmask are
// considered as potential pointers.
//
// If state != nil, it's assumed that [b, b+n) is a block in the stack
// and may contain pointers to stack objects.
private static void scanConservative(System.UIntPtr b, System.UIntPtr n, ptr<byte> _addr_ptrmask, ptr<gcWork> _addr_gcw, ptr<stackScanState> _addr_state) {
    ref byte ptrmask = ref _addr_ptrmask.val;
    ref gcWork gcw = ref _addr_gcw.val;
    ref stackScanState state = ref _addr_state.val;

    if (debugScanConservative) {
        printlock();
        print("conservatively scanning [", hex(b), ",", hex(b + n), ")\n");
        hexdumpWords(b, b + n, p => {
            if (ptrmask != null) {
                var word = (p - b) / sys.PtrSize;
                var bits = addb(ptrmask, word / 8).val;
                if ((bits >> (int)((word % 8))) & 1 == 0) {
                    return '$';
                }
            }
            ptr<ptr<System.UIntPtr>> val = new ptr<ptr<ptr<System.UIntPtr>>>(@unsafe.Pointer(p));
            if (state != null && state.stack.lo <= val && val < state.stack.hi) {
                return '@';
            }
            var span = spanOfHeap(val);
            if (span == null) {
                return ' ';
            }
            var idx = span.objIndex(val);
            if (span.isFree(idx)) {
                return ' ';
            }
            return '*';
        });
        printunlock();
    }
    {
        var i = uintptr(0);

        while (i < n) {
            if (ptrmask != null) {
                word = i / sys.PtrSize;
                bits = addb(ptrmask, word / 8).val;
                if (bits == 0) { 
                    // Skip 8 words (the loop increment will do the 8th)
                    //
                    // This must be the first time we've
                    // seen this word of ptrmask, so i
                    // must be 8-word-aligned, but check
                    // our reasoning just in case.
                    if (i % (sys.PtrSize * 8) != 0) {
                        throw("misaligned mask");
            i += sys.PtrSize;
                    }
                    i += sys.PtrSize * 8 - sys.PtrSize;
                    continue;
                }
                if ((bits >> (int)((word % 8))) & 1 == 0) {
                    continue;
                }
            }
            val = new ptr<ptr<ptr<System.UIntPtr>>>(@unsafe.Pointer(b + i)); 

            // Check if val points into the stack.
            if (state != null && state.stack.lo <= val && val < state.stack.hi) { 
                // val may point to a stack object. This
                // object may be dead from last cycle and
                // hence may contain pointers to unallocated
                // objects, but unlike heap objects we can't
                // tell if it's already dead. Hence, if all
                // pointers to this object are from
                // conservative scanning, we have to scan it
                // defensively, too.
                state.putPtr(val, true);
                continue;
            } 

            // Check if val points to a heap span.
            span = spanOfHeap(val);
            if (span == null) {
                continue;
            } 

            // Check if val points to an allocated object.
            idx = span.objIndex(val);
            if (span.isFree(idx)) {
                continue;
            } 

            // val points to an allocated object. Mark it.
            var obj = span.@base() + idx * span.elemsize;
            greyobject(obj, b, i, _addr_span, _addr_gcw, idx);
        }
    }
}

// Shade the object if it isn't already.
// The object is not nil and known to be in the heap.
// Preemption must be disabled.
//go:nowritebarrier
private static void shade(System.UIntPtr b) {
    {
        var (obj, span, objIndex) = findObject(b, 0, 0);

        if (obj != 0) {
            var gcw = _addr_getg().m.p.ptr().gcw;
            greyobject(obj, 0, 0, _addr_span, _addr_gcw, objIndex);
        }
    }
}

// obj is the start of an object with mark mbits.
// If it isn't already marked, mark it and enqueue into gcw.
// base and off are for debugging only and could be removed.
//
// See also wbBufFlush1, which partially duplicates this logic.
//
//go:nowritebarrierrec
private static void greyobject(System.UIntPtr obj, System.UIntPtr @base, System.UIntPtr off, ptr<mspan> _addr_span, ptr<gcWork> _addr_gcw, System.UIntPtr objIndex) {
    ref mspan span = ref _addr_span.val;
    ref gcWork gcw = ref _addr_gcw.val;
 
    // obj should be start of allocation, and so must be at least pointer-aligned.
    if (obj & (sys.PtrSize - 1) != 0) {
        throw("greyobject: obj not pointer-aligned");
    }
    var mbits = span.markBitsForIndex(objIndex);

    if (useCheckmark) {
        if (setCheckmark(obj, base, off, mbits)) { 
            // Already marked.
            return ;
        }
    }
    else
 {
        if (debug.gccheckmark > 0 && span.isFree(objIndex)) {
            print("runtime: marking free object ", hex(obj), " found at *(", hex(base), "+", hex(off), ")\n");
            gcDumpObject("base", base, off);
            gcDumpObject("obj", obj, ~uintptr(0));
            getg().m.traceback = 2;
            throw("marking free object");
        }
        if (mbits.isMarked()) {
            return ;
        }
        mbits.setMarked(); 

        // Mark span.
        var (arena, pageIdx, pageMask) = pageIndexOf(span.@base());
        if (arena.pageMarks[pageIdx] & pageMask == 0) {
            atomic.Or8(_addr_arena.pageMarks[pageIdx], pageMask);
        }
        if (span.spanclass.noscan()) {
            gcw.bytesMarked += uint64(span.elemsize);
            return ;
        }
    }
    if (!gcw.putFast(obj)) {
        gcw.put(obj);
    }
}

// gcDumpObject dumps the contents of obj for debugging and marks the
// field at byte offset off in obj.
private static void gcDumpObject(@string label, System.UIntPtr obj, System.UIntPtr off) {
    var s = spanOf(obj);
    print(label, "=", hex(obj));
    if (s == null) {
        print(" s=nil\n");
        return ;
    }
    print(" s.base()=", hex(s.@base()), " s.limit=", hex(s.limit), " s.spanclass=", s.spanclass, " s.elemsize=", s.elemsize, " s.state=");
    {
        var state = s.state.get();

        if (0 <= state && int(state) < len(mSpanStateNames)) {
            print(mSpanStateNames[state], "\n");
        }
        else
 {
            print("unknown(", state, ")\n");
        }
    }

    var skipped = false;
    var size = s.elemsize;
    if (s.state.get() == mSpanManual && size == 0) { 
        // We're printing something from a stack frame. We
        // don't know how big it is, so just show up to an
        // including off.
        size = off + sys.PtrSize;
    }
    {
        var i = uintptr(0);

        while (i < size) { 
            // For big objects, just print the beginning (because
            // that usually hints at the object's type) and the
            // fields around off.
            if (!(i < 128 * sys.PtrSize || off - 16 * sys.PtrSize < i && i < off + 16 * sys.PtrSize)) {
                skipped = true;
                continue;
            i += sys.PtrSize;
            }
            if (skipped) {
                print(" ...\n");
                skipped = false;
            }
            print(" *(", label, "+", i, ") = ", hex(new ptr<ptr<ptr<System.UIntPtr>>>(@unsafe.Pointer(obj + i))));
            if (i == off) {
                print(" <==");
            }
            print("\n");
        }
    }
    if (skipped) {
        print(" ...\n");
    }
}

// gcmarknewobject marks a newly allocated object black. obj must
// not contain any non-nil pointers.
//
// This is nosplit so it can manipulate a gcWork without preemption.
//
//go:nowritebarrier
//go:nosplit
private static void gcmarknewobject(ptr<mspan> _addr_span, System.UIntPtr obj, System.UIntPtr size, System.UIntPtr scanSize) {
    ref mspan span = ref _addr_span.val;

    if (useCheckmark) { // The world should be stopped so this should not happen.
        throw("gcmarknewobject called while doing checkmark");
    }
    var objIndex = span.objIndex(obj);
    span.markBitsForIndex(objIndex).setMarked(); 

    // Mark span.
    var (arena, pageIdx, pageMask) = pageIndexOf(span.@base());
    if (arena.pageMarks[pageIdx] & pageMask == 0) {
        atomic.Or8(_addr_arena.pageMarks[pageIdx], pageMask);
    }
    var gcw = _addr_getg().m.p.ptr().gcw;
    gcw.bytesMarked += uint64(size);
    gcw.scanWork += int64(scanSize);
}

// gcMarkTinyAllocs greys all active tiny alloc blocks.
//
// The world must be stopped.
private static void gcMarkTinyAllocs() {
    assertWorldStopped();

    foreach (var (_, p) in allp) {
        var c = p.mcache;
        if (c == null || c.tiny == 0) {
            continue;
        }
        var (_, span, objIndex) = findObject(c.tiny, 0, 0);
        var gcw = _addr_p.gcw;
        greyobject(c.tiny, 0, 0, _addr_span, _addr_gcw, objIndex);
    }
}

} // end runtime_package
