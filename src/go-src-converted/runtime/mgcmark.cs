// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Garbage collector: marking and scanning

// package runtime -- go2cs converted at 2020 August 29 08:18:09 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\mgcmark.go
using atomic = go.runtime.@internal.atomic_package;
using sys = go.runtime.@internal.sys_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;

namespace go
{
    public static unsafe partial class runtime_package
    {
        private static readonly var fixedRootFinalizers = iota;
        private static readonly var fixedRootFreeGStacks = 0;
        private static readonly rootBlockBytes fixedRootCount = 256L << (int)(10L); 

        // rootBlockSpans is the number of spans to scan per span
        // root.
        private static readonly long rootBlockSpans = 8L * 1024L; // 64MB worth of spans

        // maxObletBytes is the maximum bytes of an object to scan at
        // once. Larger objects will be split up into "oblets" of at
        // most this size. Since we can scan 1–2 MB/ms, 128 KB bounds
        // scan preemption at ~100 µs.
        //
        // This must be > _MaxSmallSize so that the object base is the
        // span base.
        private static readonly long maxObletBytes = 128L << (int)(10L); 

        // drainCheckThreshold specifies how many units of work to do
        // between self-preemption checks in gcDrain. Assuming a scan
        // rate of 1 MB/ms, this is ~100 µs. Lower values have higher
        // overhead in the scan loop (the scheduler check may perform
        // a syscall, so its overhead is nontrivial). Higher values
        // make the system less responsive to incoming work.
        private static readonly long drainCheckThreshold = 100000L;

        // gcMarkRootPrepare queues root scanning jobs (stacks, globals, and
        // some miscellany) and initializes scanning-related state.
        //
        // The caller must have call gcCopySpans().
        //
        // The world must be stopped.
        //
        //go:nowritebarrier
        private static void gcMarkRootPrepare()
        {
            if (gcphase == _GCmarktermination)
            {
                work.nFlushCacheRoots = int(gomaxprocs);
            }
            else
            {
                work.nFlushCacheRoots = 0L;
            } 

            // Compute how many data and BSS root blocks there are.
            Func<System.UIntPtr, long> nBlocks = bytes =>
            {
                return int((bytes + rootBlockBytes - 1L) / rootBlockBytes);
            }
;

            work.nDataRoots = 0L;
            work.nBSSRoots = 0L; 

            // Only scan globals once per cycle; preferably concurrently.
            if (!work.markrootDone)
            {
                {
                    var datap__prev1 = datap;

                    foreach (var (_, __datap) in activeModules())
                    {
                        datap = __datap;
                        var nDataRoots = nBlocks(datap.edata - datap.data);
                        if (nDataRoots > work.nDataRoots)
                        {
                            work.nDataRoots = nDataRoots;
                        }
                    }

                    datap = datap__prev1;
                }

                {
                    var datap__prev1 = datap;

                    foreach (var (_, __datap) in activeModules())
                    {
                        datap = __datap;
                        var nBSSRoots = nBlocks(datap.ebss - datap.bss);
                        if (nBSSRoots > work.nBSSRoots)
                        {
                            work.nBSSRoots = nBSSRoots;
                        }
                    }

                    datap = datap__prev1;
                }

            }
            if (!work.markrootDone)
            { 
                // On the first markroot, we need to scan span roots.
                // In concurrent GC, this happens during concurrent
                // mark and we depend on addfinalizer to ensure the
                // above invariants for objects that get finalizers
                // after concurrent mark. In STW GC, this will happen
                // during mark termination.
                //
                // We're only interested in scanning the in-use spans,
                // which will all be swept at this point. More spans
                // may be added to this list during concurrent GC, but
                // we only care about spans that were allocated before
                // this mark phase.
                work.nSpanRoots = mheap_.sweepSpans[mheap_.sweepgen / 2L % 2L].numBlocks(); 

                // On the first markroot, we need to scan all Gs. Gs
                // may be created after this point, but it's okay that
                // we ignore them because they begin life without any
                // roots, so there's nothing to scan, and any roots
                // they create during the concurrent phase will be
                // scanned during mark termination. During mark
                // termination, allglen isn't changing, so we'll scan
                // all Gs.
                work.nStackRoots = int(atomic.Loaduintptr(ref allglen));
            }
            else
            { 
                // We've already scanned span roots and kept the scan
                // up-to-date during concurrent mark.
                work.nSpanRoots = 0L; 

                // The hybrid barrier ensures that stacks can't
                // contain pointers to unmarked objects, so on the
                // second markroot, there's no need to scan stacks.
                work.nStackRoots = 0L;

                if (debug.gcrescanstacks > 0L)
                { 
                    // Scan stacks anyway for debugging.
                    work.nStackRoots = int(atomic.Loaduintptr(ref allglen));
                }
            }
            work.markrootNext = 0L;
            work.markrootJobs = uint32(fixedRootCount + work.nFlushCacheRoots + work.nDataRoots + work.nBSSRoots + work.nSpanRoots + work.nStackRoots);
        }

        // gcMarkRootCheck checks that all roots have been scanned. It is
        // purely for debugging.
        private static void gcMarkRootCheck()
        {
            if (work.markrootNext < work.markrootJobs)
            {
                print(work.markrootNext, " of ", work.markrootJobs, " markroot jobs done\n");
                throw("left over markroot jobs");
            }
            lock(ref allglock); 
            // Check that stacks have been scanned.
            ref g gp = default;
            if (gcphase == _GCmarktermination && debug.gcrescanstacks > 0L)
            {
                {
                    long i__prev1 = i;

                    for (long i = 0L; i < len(allgs); i++)
                    {
                        gp = allgs[i];
                        if (!(gp.gcscandone && gp.gcscanvalid) && readgstatus(gp) != _Gdead)
                        {
                            goto fail;
                        }
                    }
            else


                    i = i__prev1;
                }
            }            {
                {
                    long i__prev1 = i;

                    for (i = 0L; i < work.nStackRoots; i++)
                    {
                        gp = allgs[i];
                        if (!gp.gcscandone)
                        {
                            goto fail;
                        }
                    }


                    i = i__prev1;
                }
            }
            unlock(ref allglock);
            return;

fail:
            println("gp", gp, "goid", gp.goid, "status", readgstatus(gp), "gcscandone", gp.gcscandone, "gcscanvalid", gp.gcscanvalid);
            unlock(ref allglock); // Avoid self-deadlock with traceback.
            throw("scan missed a g");
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
        private static void markroot(ref gcWork gcw, uint i)
        { 
            // TODO(austin): This is a bit ridiculous. Compute and store
            // the bases in gcMarkRootPrepare instead of the counts.
            var baseFlushCache = uint32(fixedRootCount);
            var baseData = baseFlushCache + uint32(work.nFlushCacheRoots);
            var baseBSS = baseData + uint32(work.nDataRoots);
            var baseSpans = baseBSS + uint32(work.nBSSRoots);
            var baseStacks = baseSpans + uint32(work.nSpanRoots);
            var end = baseStacks + uint32(work.nStackRoots); 

            // Note: if you add a case here, please also update heapdump.go:dumproots.

            if (baseFlushCache <= i && i < baseData) 
                flushmcache(int(i - baseFlushCache));
            else if (baseData <= i && i < baseBSS) 
                {
                    var datap__prev1 = datap;

                    foreach (var (_, __datap) in activeModules())
                    {
                        datap = __datap;
                        markrootBlock(datap.data, datap.edata - datap.data, datap.gcdatamask.bytedata, gcw, int(i - baseData));
                    }

                    datap = datap__prev1;
                }
            else if (baseBSS <= i && i < baseSpans) 
                {
                    var datap__prev1 = datap;

                    foreach (var (_, __datap) in activeModules())
                    {
                        datap = __datap;
                        markrootBlock(datap.bss, datap.ebss - datap.bss, datap.gcbssmask.bytedata, gcw, int(i - baseBSS));
                    }

                    datap = datap__prev1;
                }
            else if (i == fixedRootFinalizers) 
                // Only do this once per GC cycle since we don't call
                // queuefinalizer during marking.
                if (work.markrootDone)
                {
                    break;
                }
                {
                    var fb = allfin;

                    while (fb != null)
                    {
                        var cnt = uintptr(atomic.Load(ref fb.cnt));
                        scanblock(uintptr(@unsafe.Pointer(ref fb.fin[0L])), cnt * @unsafe.Sizeof(fb.fin[0L]), ref finptrmask[0L], gcw);
                        fb = fb.alllink;
                    }

                }
            else if (i == fixedRootFreeGStacks) 
                // Only do this once per GC cycle; preferably
                // concurrently.
                if (!work.markrootDone)
                { 
                    // Switch to the system stack so we can call
                    // stackfree.
                    systemstack(markrootFreeGStacks);
                }
            else if (baseSpans <= i && i < baseStacks) 
                // mark MSpan.specials
                markrootSpans(gcw, int(i - baseSpans));
            else 
                // the rest is scanning goroutine stacks
                ref g gp = default;
                if (baseStacks <= i && i < end)
                {
                    gp = allgs[i - baseStacks];
                }
                else
                {
                    throw("markroot: bad index");
                } 

                // remember when we've first observed the G blocked
                // needed only to output in traceback
                var status = readgstatus(gp); // We are not in a scan state
                if ((status == _Gwaiting || status == _Gsyscall) && gp.waitsince == 0L)
                {
                    gp.waitsince = work.tstart;
                } 

                // scang must be done on the system stack in case
                // we're trying to scan our own stack.
                systemstack(() =>
                { 
                    // If this is a self-scan, put the user G in
                    // _Gwaiting to prevent self-deadlock. It may
                    // already be in _Gwaiting if this is a mark
                    // worker or we're in mark termination.
                    var userG = getg().m.curg;
                    var selfScan = gp == userG && readgstatus(userG) == _Grunning;
                    if (selfScan)
                    {
                        casgstatus(userG, _Grunning, _Gwaiting);
                        userG.waitreason = "garbage collection scan";
                    } 

                    // TODO: scang blocks until gp's stack has
                    // been scanned, which may take a while for
                    // running goroutines. Consider doing this in
                    // two phases where the first is non-blocking:
                    // we scan the stacks we can and ask running
                    // goroutines to scan themselves; and the
                    // second blocks.
                    scang(gp, gcw);

                    if (selfScan)
                    {
                        casgstatus(userG, _Gwaiting, _Grunning);
                    }
                });
                    }

        // markrootBlock scans the shard'th shard of the block of memory [b0,
        // b0+n0), with the given pointer mask.
        //
        //go:nowritebarrier
        private static void markrootBlock(System.UIntPtr b0, System.UIntPtr n0, ref byte ptrmask0, ref gcWork gcw, long shard)
        {
            if (rootBlockBytes % (8L * sys.PtrSize) != 0L)
            { 
                // This is necessary to pick byte offsets in ptrmask0.
                throw("rootBlockBytes must be a multiple of 8*ptrSize");
            }
            var b = b0 + uintptr(shard) * rootBlockBytes;
            if (b >= b0 + n0)
            {
                return;
            }
            var ptrmask = (uint8.Value)(add(@unsafe.Pointer(ptrmask0), uintptr(shard) * (rootBlockBytes / (8L * sys.PtrSize))));
            var n = uintptr(rootBlockBytes);
            if (b + n > b0 + n0)
            {
                n = b0 + n0 - b;
            } 

            // Scan this shard.
            scanblock(b, n, ptrmask, gcw);
        }

        // markrootFreeGStacks frees stacks of dead Gs.
        //
        // This does not free stacks of dead Gs cached on Ps, but having a few
        // cached stacks around isn't a problem.
        //
        //TODO go:nowritebarrier
        private static void markrootFreeGStacks()
        { 
            // Take list of dead Gs with stacks.
            lock(ref sched.gflock);
            var list = sched.gfreeStack;
            sched.gfreeStack = null;
            unlock(ref sched.gflock);
            if (list == null)
            {
                return;
            } 

            // Free stacks.
            var tail = list;
            {
                var gp = list;

                while (gp != null)
                {
                    shrinkstack(gp);
                    tail = gp;
                    gp = gp.schedlink.ptr();
                } 

                // Put Gs back on the free list.

            } 

            // Put Gs back on the free list.
            lock(ref sched.gflock);
            tail.schedlink.set(sched.gfreeNoStack);
            sched.gfreeNoStack = list;
            unlock(ref sched.gflock);
        }

        // markrootSpans marks roots for one shard of work.spans.
        //
        //go:nowritebarrier
        private static void markrootSpans(ref gcWork gcw, long shard)
        { 
            // Objects with finalizers have two GC-related invariants:
            //
            // 1) Everything reachable from the object must be marked.
            // This ensures that when we pass the object to its finalizer,
            // everything the finalizer can reach will be retained.
            //
            // 2) Finalizer specials (which are not in the garbage
            // collected heap) are roots. In practice, this means the fn
            // field must be scanned.
            //
            // TODO(austin): There are several ideas for making this more
            // efficient in issue #11485.

            if (work.markrootDone)
            {
                throw("markrootSpans during second markroot");
            }
            var sg = mheap_.sweepgen;
            var spans = mheap_.sweepSpans[mheap_.sweepgen / 2L % 2L].block(shard); 
            // Note that work.spans may not include spans that were
            // allocated between entering the scan phase and now. This is
            // okay because any objects with finalizers in those spans
            // must have been allocated and given finalizers after we
            // entered the scan phase, so addfinalizer will have ensured
            // the above invariants for them.
            foreach (var (_, s) in spans)
            {
                if (s.state != mSpanInUse)
                {
                    continue;
                }
                if (!useCheckmark && s.sweepgen != sg)
                { 
                    // sweepgen was updated (+2) during non-checkmark GC pass
                    print("sweep ", s.sweepgen, " ", sg, "\n");
                    throw("gc: unswept span");
                } 

                // Speculatively check if there are any specials
                // without acquiring the span lock. This may race with
                // adding the first special to a span, but in that
                // case addfinalizer will observe that the GC is
                // active (which is globally synchronized) and ensure
                // the above invariants. We may also ensure the
                // invariants, but it's okay to scan an object twice.
                if (s.specials == null)
                {
                    continue;
                } 

                // Lock the specials to prevent a special from being
                // removed from the list while we're traversing it.
                lock(ref s.speciallock);

                {
                    var sp = s.specials;

                    while (sp != null)
                    {
                        if (sp.kind != _KindSpecialFinalizer)
                        {
                            continue;
                        sp = sp.next;
                        } 
                        // don't mark finalized object, but scan it so we
                        // retain everything it points to.
                        var spf = (specialfinalizer.Value)(@unsafe.Pointer(sp)); 
                        // A finalizer can be set for an inner byte of an object, find object beginning.
                        var p = s.@base() + uintptr(spf.special.offset) / s.elemsize * s.elemsize; 

                        // Mark everything that can be reached from
                        // the object (but *not* the object itself or
                        // we'll never collect it).
                        scanobject(p, gcw); 

                        // The special itself is a root.
                        scanblock(uintptr(@unsafe.Pointer(ref spf.fn)), sys.PtrSize, ref oneptrmask[0L], gcw);
                    }

                }

                unlock(ref s.speciallock);
            }
        }

        // gcAssistAlloc performs GC work to make gp's assist debt positive.
        // gp must be the calling user gorountine.
        //
        // This must be called with preemption enabled.
        private static void gcAssistAlloc(ref g gp)
        { 
            // Don't assist in non-preemptible contexts. These are
            // generally fragile and won't allow the assist to block.
            if (getg() == gp.m.g0)
            {
                return;
            }
            {
                var mp = getg().m;

                if (mp.locks > 0L || mp.preemptoff != "")
                {
                    return;
                }

            }

            var traced = false;
retry:
            var debtBytes = -gp.gcAssistBytes;
            var scanWork = int64(gcController.assistWorkPerByte * float64(debtBytes));
            if (scanWork < gcOverAssistWork)
            {
                scanWork = gcOverAssistWork;
                debtBytes = int64(gcController.assistBytesPerWork * float64(scanWork));
            } 

            // Steal as much credit as we can from the background GC's
            // scan credit. This is racy and may drop the background
            // credit below 0 if two mutators steal at the same time. This
            // will just cause steals to fail until credit is accumulated
            // again, so in the long run it doesn't really matter, but we
            // do have to handle the negative credit case.
            var bgScanCredit = atomic.Loadint64(ref gcController.bgScanCredit);
            var stolen = int64(0L);
            if (bgScanCredit > 0L)
            {
                if (bgScanCredit < scanWork)
                {
                    stolen = bgScanCredit;
                    gp.gcAssistBytes += 1L + int64(gcController.assistBytesPerWork * float64(stolen));
                }
                else
                {
                    stolen = scanWork;
                    gp.gcAssistBytes += debtBytes;
                }
                atomic.Xaddint64(ref gcController.bgScanCredit, -stolen);

                scanWork -= stolen;

                if (scanWork == 0L)
                { 
                    // We were able to steal all of the credit we
                    // needed.
                    if (traced)
                    {
                        traceGCMarkAssistDone();
                    }
                    return;
                }
            }
            if (trace.enabled && !traced)
            {
                traced = true;
                traceGCMarkAssistStart();
            } 

            // Perform assist work
            systemstack(() =>
            {
                gcAssistAlloc1(gp, scanWork); 
                // The user stack may have moved, so this can't touch
                // anything on it until it returns from systemstack.
            });

            var completed = gp.param != null;
            gp.param = null;
            if (completed)
            {
                gcMarkDone();
            }
            if (gp.gcAssistBytes < 0L)
            { 
                // We were unable steal enough credit or perform
                // enough work to pay off the assist debt. We need to
                // do one of these before letting the mutator allocate
                // more to prevent over-allocation.
                //
                // If this is because we were preempted, reschedule
                // and try some more.
                if (gp.preempt)
                {
                    Gosched();
                    goto retry;
                } 

                // Add this G to an assist queue and park. When the GC
                // has more background credit, it will satisfy queued
                // assists before flushing to the global credit pool.
                //
                // Note that this does *not* get woken up when more
                // work is added to the work list. The theory is that
                // there wasn't enough work to do anyway, so we might
                // as well let background marking take care of the
                // work that is available.
                if (!gcParkAssist())
                {
                    goto retry;
                } 

                // At this point either background GC has satisfied
                // this G's assist debt, or the GC cycle is over.
            }
            if (traced)
            {
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
        private static void gcAssistAlloc1(ref g gp, long scanWork)
        { 
            // Clear the flag indicating that this assist completed the
            // mark phase.
            gp.param = null;

            if (atomic.Load(ref gcBlackenEnabled) == 0L)
            { 
                // The gcBlackenEnabled check in malloc races with the
                // store that clears it but an atomic check in every malloc
                // would be a performance hit.
                // Instead we recheck it here on the non-preemptable system
                // stack to determine if we should preform an assist.

                // GC is done, so ignore any remaining debt.
                gp.gcAssistBytes = 0L;
                return;
            } 
            // Track time spent in this assist. Since we're on the
            // system stack, this is non-preemptible, so we can
            // just measure start and end time.
            var startTime = nanotime();

            var decnwait = atomic.Xadd(ref work.nwait, -1L);
            if (decnwait == work.nproc)
            {
                println("runtime: work.nwait =", decnwait, "work.nproc=", work.nproc);
                throw("nwait > work.nprocs");
            } 

            // gcDrainN requires the caller to be preemptible.
            casgstatus(gp, _Grunning, _Gwaiting);
            gp.waitreason = "GC assist marking"; 

            // drain own cached work first in the hopes that it
            // will be more cache friendly.
            var gcw = ref getg().m.p.ptr().gcw;
            var workDone = gcDrainN(gcw, scanWork); 
            // If we are near the end of the mark phase
            // dispose of the gcw.
            if (gcBlackenPromptly)
            {
                gcw.dispose();
            }
            casgstatus(gp, _Gwaiting, _Grunning); 

            // Record that we did this much scan work.
            //
            // Back out the number of bytes of assist credit that
            // this scan work counts for. The "1+" is a poor man's
            // round-up, to ensure this adds credit even if
            // assistBytesPerWork is very low.
            gp.gcAssistBytes += 1L + int64(gcController.assistBytesPerWork * float64(workDone)); 

            // If this is the last worker and we ran out of work,
            // signal a completion point.
            var incnwait = atomic.Xadd(ref work.nwait, +1L);
            if (incnwait > work.nproc)
            {
                println("runtime: work.nwait=", incnwait, "work.nproc=", work.nproc, "gcBlackenPromptly=", gcBlackenPromptly);
                throw("work.nwait > work.nproc");
            }
            if (incnwait == work.nproc && !gcMarkWorkAvailable(null))
            { 
                // This has reached a background completion point. Set
                // gp.param to a non-nil value to indicate this. It
                // doesn't matter what we set it to (it just has to be
                // a valid pointer).
                gp.param = @unsafe.Pointer(gp);
            }
            var duration = nanotime() - startTime;
            var _p_ = gp.m.p.ptr();
            _p_.gcAssistTime += duration;
            if (_p_.gcAssistTime > gcAssistTimeSlack)
            {
                atomic.Xaddint64(ref gcController.assistTime, _p_.gcAssistTime);
                _p_.gcAssistTime = 0L;
            }
        }

        // gcWakeAllAssists wakes all currently blocked assists. This is used
        // at the end of a GC cycle. gcBlackenEnabled must be false to prevent
        // new assists from going to sleep after this point.
        private static void gcWakeAllAssists()
        {
            lock(ref work.assistQueue.@lock);
            injectglist(work.assistQueue.head.ptr());
            work.assistQueue.head.set(null);
            work.assistQueue.tail.set(null);
            unlock(ref work.assistQueue.@lock);
        }

        // gcParkAssist puts the current goroutine on the assist queue and parks.
        //
        // gcParkAssist returns whether the assist is now satisfied. If it
        // returns false, the caller must retry the assist.
        //
        //go:nowritebarrier
        private static bool gcParkAssist()
        {
            lock(ref work.assistQueue.@lock); 
            // If the GC cycle finished while we were getting the lock,
            // exit the assist. The cycle can't finish while we hold the
            // lock.
            if (atomic.Load(ref gcBlackenEnabled) == 0L)
            {
                unlock(ref work.assistQueue.@lock);
                return true;
            }
            var gp = getg();
            var oldHead = work.assistQueue.head;
            var oldTail = work.assistQueue.tail;
            if (oldHead == 0L)
            {
                work.assistQueue.head.set(gp);
            }
            else
            {
                oldTail.ptr().schedlink.set(gp);
            }
            work.assistQueue.tail.set(gp);
            gp.schedlink.set(null); 

            // Recheck for background credit now that this G is in
            // the queue, but can still back out. This avoids a
            // race in case background marking has flushed more
            // credit since we checked above.
            if (atomic.Loadint64(ref gcController.bgScanCredit) > 0L)
            {
                work.assistQueue.head = oldHead;
                work.assistQueue.tail = oldTail;
                if (oldTail != 0L)
                {
                    oldTail.ptr().schedlink.set(null);
                }
                unlock(ref work.assistQueue.@lock);
                return false;
            } 
            // Park.
            goparkunlock(ref work.assistQueue.@lock, "GC assist wait", traceEvGoBlockGC, 2L);
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
        private static void gcFlushBgCredit(long scanWork)
        {
            if (work.assistQueue.head == 0L)
            { 
                // Fast path; there are no blocked assists. There's a
                // small window here where an assist may add itself to
                // the blocked queue and park. If that happens, we'll
                // just get it on the next flush.
                atomic.Xaddint64(ref gcController.bgScanCredit, scanWork);
                return;
            }
            var scanBytes = int64(float64(scanWork) * gcController.assistBytesPerWork);

            lock(ref work.assistQueue.@lock);
            var gp = work.assistQueue.head.ptr();
            while (gp != null && scanBytes > 0L)
            { 
                // Note that gp.gcAssistBytes is negative because gp
                // is in debt. Think carefully about the signs below.
                if (scanBytes + gp.gcAssistBytes >= 0L)
                { 
                    // Satisfy this entire assist debt.
                    scanBytes += gp.gcAssistBytes;
                    gp.gcAssistBytes = 0L;
                    var xgp = gp;
                    gp = gp.schedlink.ptr(); 
                    // It's important that we *not* put xgp in
                    // runnext. Otherwise, it's possible for user
                    // code to exploit the GC worker's high
                    // scheduler priority to get itself always run
                    // before other goroutines and always in the
                    // fresh quantum started by GC.
                    ready(xgp, 0L, false);
                }
                else
                { 
                    // Partially satisfy this assist.
                    gp.gcAssistBytes += scanBytes;
                    scanBytes = 0L; 
                    // As a heuristic, we move this assist to the
                    // back of the queue so that large assists
                    // can't clog up the assist queue and
                    // substantially delay small assists.
                    xgp = gp;
                    gp = gp.schedlink.ptr();
                    if (gp == null)
                    { 
                        // gp is the only assist in the queue.
                        gp = xgp;
                    }
                    else
                    {
                        xgp.schedlink = 0L;
                        work.assistQueue.tail.ptr().schedlink.set(xgp);
                        work.assistQueue.tail.set(xgp);
                    }
                    break;
                }
            }

            work.assistQueue.head.set(gp);
            if (gp == null)
            {
                work.assistQueue.tail.set(null);
            }
            if (scanBytes > 0L)
            { 
                // Convert from scan bytes back to work.
                scanWork = int64(float64(scanBytes) * gcController.assistWorkPerByte);
                atomic.Xaddint64(ref gcController.bgScanCredit, scanWork);
            }
            unlock(ref work.assistQueue.@lock);
        }

        // scanstack scans gp's stack, greying all pointers found on the stack.
        //
        // scanstack is marked go:systemstack because it must not be preempted
        // while using a workbuf.
        //
        //go:nowritebarrier
        //go:systemstack
        private static void scanstack(ref g gp, ref gcWork gcw)
        {
            if (gp.gcscanvalid)
            {
                return;
            }
            if (readgstatus(gp) & _Gscan == 0L)
            {
                print("runtime:scanstack: gp=", gp, ", goid=", gp.goid, ", gp->atomicstatus=", hex(readgstatus(gp)), "\n");
                throw("scanstack - bad status");
            }

            if (readgstatus(gp) & ~_Gscan == _Gdead) 
                return;
            else if (readgstatus(gp) & ~_Gscan == _Grunning) 
                print("runtime: gp=", gp, ", goid=", gp.goid, ", gp->atomicstatus=", readgstatus(gp), "\n");
                throw("scanstack: goroutine not stopped");
            else if (readgstatus(gp) & ~_Gscan == _Grunnable || readgstatus(gp) & ~_Gscan == _Gsyscall || readgstatus(gp) & ~_Gscan == _Gwaiting)             else 
                print("runtime: gp=", gp, ", goid=", gp.goid, ", gp->atomicstatus=", readgstatus(gp), "\n");
                throw("mark - bad status");
                        if (gp == getg())
            {
                throw("can't scan our own stack");
            }
            var mp = gp.m;
            if (mp != null && mp.helpgc != 0L)
            {
                throw("can't scan gchelper stack");
            } 

            // Shrink the stack if not much of it is being used. During
            // concurrent GC, we can do this during concurrent mark.
            if (!work.markrootDone)
            {
                shrinkstack(gp);
            } 

            // Scan the saved context register. This is effectively a live
            // register that gets moved back and forth between the
            // register and sched.ctxt without a write barrier.
            if (gp.sched.ctxt != null)
            {
                scanblock(uintptr(@unsafe.Pointer(ref gp.sched.ctxt)), sys.PtrSize, ref oneptrmask[0L], gcw);
            } 

            // Scan the stack.
            pcvalueCache cache = default;
            Func<ref stkframe, unsafe.Pointer, bool> scanframe = (frame, unused) =>
            {
                scanframeworker(frame, ref cache, gcw);
                return true;
            }
;
            gentraceback(~uintptr(0L), ~uintptr(0L), 0L, gp, 0L, null, 0x7fffffffUL, scanframe, null, 0L);
            tracebackdefers(gp, scanframe, null);
            gp.gcscanvalid = true;
        }

        // Scan a stack frame: local variables and function arguments/results.
        //go:nowritebarrier
        private static void scanframeworker(ref stkframe frame, ref pcvalueCache cache, ref gcWork gcw)
        {
            var f = frame.fn;
            var targetpc = frame.continpc;
            if (targetpc == 0L)
            { 
                // Frame is dead.
                return;
            }
            if (_DebugGC > 1L)
            {
                print("scanframe ", funcname(f), "\n");
            }
            if (targetpc != f.entry)
            {
                targetpc--;
            }
            var pcdata = pcdatavalue(f, _PCDATA_StackMapIndex, targetpc, cache);
            if (pcdata == -1L)
            { 
                // We do not have a valid pcdata value but there might be a
                // stackmap for this function. It is likely that we are looking
                // at the function prologue, assume so and hope for the best.
                pcdata = 0L;
            } 

            // Scan local variables if stack frame has been allocated.
            var size = frame.varp - frame.sp;
            System.UIntPtr minsize = default;

            if (sys.ArchFamily == sys.ARM64) 
                minsize = sys.SpAlign;
            else 
                minsize = sys.MinFrameSize;
                        if (size > minsize)
            {
                var stkmap = (stackmap.Value)(funcdata(f, _FUNCDATA_LocalsPointerMaps));
                if (stkmap == null || stkmap.n <= 0L)
                {
                    print("runtime: frame ", funcname(f), " untyped locals ", hex(frame.varp - size), "+", hex(size), "\n");
                    throw("missing stackmap");
                } 

                // Locals bitmap information, scan just the pointers in locals.
                if (pcdata < 0L || pcdata >= stkmap.n)
                { 
                    // don't know where we are
                    print("runtime: pcdata is ", pcdata, " and ", stkmap.n, " locals stack map entries for ", funcname(f), " (targetpc=", targetpc, ")\n");
                    throw("scanframe: bad symbol table");
                }
                var bv = stackmapdata(stkmap, pcdata);
                size = uintptr(bv.n) * sys.PtrSize;
                scanblock(frame.varp - size, size, bv.bytedata, gcw);
            } 

            // Scan arguments.
            if (frame.arglen > 0L)
            {
                bv = default;
                if (frame.argmap != null)
                {
                    bv = frame.argmap.Value;
                }
                else
                {
                    stkmap = (stackmap.Value)(funcdata(f, _FUNCDATA_ArgsPointerMaps));
                    if (stkmap == null || stkmap.n <= 0L)
                    {
                        print("runtime: frame ", funcname(f), " untyped args ", hex(frame.argp), "+", hex(frame.arglen), "\n");
                        throw("missing stackmap");
                    }
                    if (pcdata < 0L || pcdata >= stkmap.n)
                    { 
                        // don't know where we are
                        print("runtime: pcdata is ", pcdata, " and ", stkmap.n, " args stack map entries for ", funcname(f), " (targetpc=", targetpc, ")\n");
                        throw("scanframe: bad symbol table");
                    }
                    bv = stackmapdata(stkmap, pcdata);
                }
                scanblock(frame.argp, uintptr(bv.n) * sys.PtrSize, bv.bytedata, gcw);
            }
        }

        private partial struct gcDrainFlags // : long
        {
        }

        private static readonly gcDrainFlags gcDrainUntilPreempt = 1L << (int)(iota);
        private static readonly var gcDrainNoBlock = 0;
        private static readonly var gcDrainFlushBgCredit = 1;
        private static readonly var gcDrainIdle = 2;
        private static readonly var gcDrainFractional = 3; 

        // gcDrainBlock means neither gcDrainUntilPreempt or
        // gcDrainNoBlock. It is the default, but callers should use
        // the constant for documentation purposes.
        private static readonly gcDrainFlags gcDrainBlock = 0L;

        // gcDrain scans roots and objects in work buffers, blackening grey
        // objects until all roots and work buffers have been drained.
        //
        // If flags&gcDrainUntilPreempt != 0, gcDrain returns when g.preempt
        // is set. This implies gcDrainNoBlock.
        //
        // If flags&gcDrainIdle != 0, gcDrain returns when there is other work
        // to do. This implies gcDrainNoBlock.
        //
        // If flags&gcDrainFractional != 0, gcDrain self-preempts when
        // pollFractionalWorkerExit() returns true. This implies
        // gcDrainNoBlock.
        //
        // If flags&gcDrainNoBlock != 0, gcDrain returns as soon as it is
        // unable to get more work. Otherwise, it will block until all
        // blocking calls are blocked in gcDrain.
        //
        // If flags&gcDrainFlushBgCredit != 0, gcDrain flushes scan work
        // credit to gcController.bgScanCredit every gcCreditSlack units of
        // scan work.
        //
        //go:nowritebarrier
        private static void gcDrain(ref gcWork gcw, gcDrainFlags flags)
        {
            if (!writeBarrier.needed)
            {
                throw("gcDrain phase incorrect");
            }
            var gp = getg().m.curg;
            var preemptible = flags & gcDrainUntilPreempt != 0L;
            var blocking = flags & (gcDrainUntilPreempt | gcDrainIdle | gcDrainFractional | gcDrainNoBlock) == 0L;
            var flushBgCredit = flags & gcDrainFlushBgCredit != 0L;
            var idle = flags & gcDrainIdle != 0L;

            var initScanWork = gcw.scanWork; 

            // checkWork is the scan work before performing the next
            // self-preempt check.
            var checkWork = int64(1L << (int)(63L) - 1L);
            Func<bool> check = default;
            if (flags & (gcDrainIdle | gcDrainFractional) != 0L)
            {
                checkWork = initScanWork + drainCheckThreshold;
                if (idle)
                {
                    check = pollWork;
                }
                else if (flags & gcDrainFractional != 0L)
                {
                    check = pollFractionalWorkerExit;
                }
            } 

            // Drain root marking jobs.
            if (work.markrootNext < work.markrootJobs)
            {
                while (!(preemptible && gp.preempt))
                {
                    var job = atomic.Xadd(ref work.markrootNext, +1L) - 1L;
                    if (job >= work.markrootJobs)
                    {
                        break;
                    }
                    markroot(gcw, job);
                    if (check != null && check())
                    {
                        goto done;
                    }
                }

            } 

            // Drain heap marking jobs.
            while (!(preemptible && gp.preempt))
            { 
                // Try to keep work available on the global queue. We used to
                // check if there were waiting workers, but it's better to
                // just keep work available than to make workers wait. In the
                // worst case, we'll do O(log(_WorkbufSize)) unnecessary
                // balances.
                if (work.full == 0L)
                {
                    gcw.balance();
                }
                System.UIntPtr b = default;
                if (blocking)
                {
                    b = gcw.get();
                }
                else
                {
                    b = gcw.tryGetFast();
                    if (b == 0L)
                    {
                        b = gcw.tryGet();
                    }
                }
                if (b == 0L)
                { 
                    // work barrier reached or tryGet failed.
                    break;
                }
                scanobject(b, gcw); 

                // Flush background scan work credit to the global
                // account if we've accumulated enough locally so
                // mutator assists can draw on it.
                if (gcw.scanWork >= gcCreditSlack)
                {
                    atomic.Xaddint64(ref gcController.scanWork, gcw.scanWork);
                    if (flushBgCredit)
                    {
                        gcFlushBgCredit(gcw.scanWork - initScanWork);
                        initScanWork = 0L;
                    }
                    checkWork -= gcw.scanWork;
                    gcw.scanWork = 0L;

                    if (checkWork <= 0L)
                    {
                        checkWork += drainCheckThreshold;
                        if (check != null && check())
                        {
                            break;
                        }
                    }
                }
            } 

            // In blocking mode, write barriers are not allowed after this
            // point because we must preserve the condition that the work
            // buffers are empty.
 

            // In blocking mode, write barriers are not allowed after this
            // point because we must preserve the condition that the work
            // buffers are empty.

done:
            if (gcw.scanWork > 0L)
            {
                atomic.Xaddint64(ref gcController.scanWork, gcw.scanWork);
                if (flushBgCredit)
                {
                    gcFlushBgCredit(gcw.scanWork - initScanWork);
                }
                gcw.scanWork = 0L;
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
        private static long gcDrainN(ref gcWork gcw, long scanWork)
        {
            if (!writeBarrier.needed)
            {
                throw("gcDrainN phase incorrect");
            } 

            // There may already be scan work on the gcw, which we don't
            // want to claim was done by this call.
            var workFlushed = -gcw.scanWork;

            var gp = getg().m.curg;
            while (!gp.preempt && workFlushed + gcw.scanWork < scanWork)
            { 
                // See gcDrain comment.
                if (work.full == 0L)
                {
                    gcw.balance();
                } 

                // This might be a good place to add prefetch code...
                // if(wbuf.nobj > 4) {
                //         PREFETCH(wbuf->obj[wbuf.nobj - 3];
                //  }
                //
                var b = gcw.tryGetFast();
                if (b == 0L)
                {
                    b = gcw.tryGet();
                }
                if (b == 0L)
                { 
                    // Try to do a root job.
                    //
                    // TODO: Assists should get credit for this
                    // work.
                    if (work.markrootNext < work.markrootJobs)
                    {
                        var job = atomic.Xadd(ref work.markrootNext, +1L) - 1L;
                        if (job < work.markrootJobs)
                        {
                            markroot(gcw, job);
                            continue;
                        }
                    } 
                    // No heap or root jobs.
                    break;
                }
                scanobject(b, gcw); 

                // Flush background scan work credit.
                if (gcw.scanWork >= gcCreditSlack)
                {
                    atomic.Xaddint64(ref gcController.scanWork, gcw.scanWork);
                    workFlushed += gcw.scanWork;
                    gcw.scanWork = 0L;
                }
            } 

            // Unlike gcDrain, there's no need to flush remaining work
            // here because this never flushes to bgScanCredit and
            // gcw.dispose will flush any remaining work to scanWork.
 

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
        //go:nowritebarrier
        private static void scanblock(System.UIntPtr b0, System.UIntPtr n0, ref byte ptrmask, ref gcWork gcw)
        { 
            // Use local copies of original parameters, so that a stack trace
            // due to one of the throws below shows the original block
            // base and extent.
            var b = b0;
            var n = n0;

            var arena_start = mheap_.arena_start;
            var arena_used = mheap_.arena_used;

            {
                var i = uintptr(0L);

                while (i < n)
                { 
                    // Find bits for the next word.
                    var bits = uint32(addb(ptrmask, i / (sys.PtrSize * 8L)).Value);
                    if (bits == 0L)
                    {
                        i += sys.PtrSize * 8L;
                        continue;
                    }
                    for (long j = 0L; j < 8L && i < n; j++)
                    {
                        if (bits & 1L != 0L)
                        { 
                            // Same work as in scanobject; see comments there.
                            *(*System.UIntPtr) obj = @unsafe.Pointer(b + i).Value;
                            if (obj != 0L && arena_start <= obj && obj < arena_used)
                            {
                                {
                                    *(*System.UIntPtr) obj__prev3 = obj;

                                    var (obj, hbits, span, objIndex) = heapBitsForObject(obj, b, i);

                                    if (obj != 0L)
                                    {
                                        greyobject(obj, b, i, hbits, span, gcw, objIndex);
                                    }

                                    obj = obj__prev3;

                                }
                            }
                        }
                        bits >>= 1L;
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
        private static void scanobject(System.UIntPtr b, ref gcWork gcw)
        { 
            // Note that arena_used may change concurrently during
            // scanobject and hence scanobject may encounter a pointer to
            // a newly allocated heap object that is *not* in
            // [start,used). It will not mark this object; however, we
            // know that it was just installed by a mutator, which means
            // that mutator will execute a write barrier and take care of
            // marking it. This is even more pronounced on relaxed memory
            // architectures since we access arena_used without barriers
            // or synchronization, but the same logic applies.
            var arena_start = mheap_.arena_start;
            var arena_used = mheap_.arena_used; 

            // Find the bits for b and the size of the object at b.
            //
            // b is either the beginning of an object, in which case this
            // is the size of the object to scan, or it points to an
            // oblet, in which case we compute the size to scan below.
            var hbits = heapBitsForAddr(b);
            var s = spanOfUnchecked(b);
            var n = s.elemsize;
            if (n == 0L)
            {
                throw("scanobject n == 0");
            }
            if (n > maxObletBytes)
            { 
                // Large object. Break into oblets for better
                // parallelism and lower latency.
                if (b == s.@base())
                { 
                    // It's possible this is a noscan object (not
                    // from greyobject, but from other code
                    // paths), in which case we must *not* enqueue
                    // oblets since their bitmaps will be
                    // uninitialized.
                    if (s.spanclass.noscan())
                    { 
                        // Bypass the whole scan.
                        gcw.bytesMarked += uint64(n);
                        return;
                    } 

                    // Enqueue the other oblets to scan later.
                    // Some oblets may be in b's scalar tail, but
                    // these will be marked as "no more pointers",
                    // so we'll drop out immediately when we go to
                    // scan those.
                    {
                        var oblet = b + maxObletBytes;

                        while (oblet < s.@base() + s.elemsize)
                        {
                            if (!gcw.putFast(oblet))
                            {
                                gcw.put(oblet);
                            oblet += maxObletBytes;
                            }
                        }

                    }
                } 

                // Compute the size of the oblet. Since this object
                // must be a large object, s.base() is the beginning
                // of the object.
                n = s.@base() + s.elemsize - b;
                if (n > maxObletBytes)
                {
                    n = maxObletBytes;
                }
            }
            System.UIntPtr i = default;
            i = 0L;

            while (i < n)
            { 
                // Find bits for this word.
                if (i != 0L)
                { 
                    // Avoid needless hbits.next() on last iteration.
                    hbits = hbits.next();
                i += sys.PtrSize;
                } 
                // Load bits once. See CL 22712 and issue 16973 for discussion.
                var bits = hbits.bits(); 
                // During checkmarking, 1-word objects store the checkmark
                // in the type bit for the one word. The only one-word objects
                // are pointers, or else they'd be merged with other non-pointer
                // data into larger allocations.
                if (i != 1L * sys.PtrSize && bits & bitScan == 0L)
                {
                    break; // no more pointers in this object
                }
                if (bits & bitPointer == 0L)
                {
                    continue; // not a pointer
                } 

                // Work here is duplicated in scanblock and above.
                // If you make changes here, make changes there too.
                *(*System.UIntPtr) obj = @unsafe.Pointer(b + i).Value; 

                // At this point we have extracted the next potential pointer.
                // Check if it points into heap and not back at the current object.
                if (obj != 0L && arena_start <= obj && obj < arena_used && obj - b >= n)
                { 
                    // Mark the object.
                    {
                        *(*System.UIntPtr) obj__prev2 = obj;

                        var (obj, hbits, span, objIndex) = heapBitsForObject(obj, b, i);

                        if (obj != 0L)
                        {
                            greyobject(obj, b, i, hbits, span, gcw, objIndex);
                        }

                        obj = obj__prev2;

                    }
                }
            }

            gcw.bytesMarked += uint64(n);
            gcw.scanWork += int64(i);
        }

        // Shade the object if it isn't already.
        // The object is not nil and known to be in the heap.
        // Preemption must be disabled.
        //go:nowritebarrier
        private static void shade(System.UIntPtr b)
        {
            {
                var (obj, hbits, span, objIndex) = heapBitsForObject(b, 0L, 0L);

                if (obj != 0L)
                {
                    var gcw = ref getg().m.p.ptr().gcw;
                    greyobject(obj, 0L, 0L, hbits, span, gcw, objIndex);
                    if (gcphase == _GCmarktermination || gcBlackenPromptly)
                    { 
                        // Ps aren't allowed to cache work during mark
                        // termination.
                        gcw.dispose();
                    }
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
        private static void greyobject(System.UIntPtr obj, System.UIntPtr @base, System.UIntPtr off, heapBits hbits, ref mspan span, ref gcWork gcw, System.UIntPtr objIndex)
        { 
            // obj should be start of allocation, and so must be at least pointer-aligned.
            if (obj & (sys.PtrSize - 1L) != 0L)
            {
                throw("greyobject: obj not pointer-aligned");
            }
            var mbits = span.markBitsForIndex(objIndex);

            if (useCheckmark)
            {
                if (!mbits.isMarked())
                {
                    printlock();
                    print("runtime:greyobject: checkmarks finds unexpected unmarked object obj=", hex(obj), "\n");
                    print("runtime: found obj at *(", hex(base), "+", hex(off), ")\n"); 

                    // Dump the source (base) object
                    gcDumpObject("base", base, off); 

                    // Dump the object
                    gcDumpObject("obj", obj, ~uintptr(0L));

                    getg().m.traceback = 2L;
                    throw("checkmark found unmarked object");
                }
                if (hbits.isCheckmarked(span.elemsize))
                {
                    return;
                }
                hbits.setCheckmarked(span.elemsize);
                if (!hbits.isCheckmarked(span.elemsize))
                {
                    throw("setCheckmarked and isCheckmarked disagree");
                }
            }
            else
            {
                if (debug.gccheckmark > 0L && span.isFree(objIndex))
                {
                    print("runtime: marking free object ", hex(obj), " found at *(", hex(base), "+", hex(off), ")\n");
                    gcDumpObject("base", base, off);
                    gcDumpObject("obj", obj, ~uintptr(0L));
                    getg().m.traceback = 2L;
                    throw("marking free object");
                } 

                // If marked we have nothing to do.
                if (mbits.isMarked())
                {
                    return;
                } 
                // mbits.setMarked() // Avoid extra call overhead with manual inlining.
                atomic.Or8(mbits.bytep, mbits.mask); 
                // If this is a noscan object, fast-track it to black
                // instead of greying it.
                if (span.spanclass.noscan())
                {
                    gcw.bytesMarked += uint64(span.elemsize);
                    return;
                }
            } 

            // Queue the obj for scanning. The PREFETCH(obj) logic has been removed but
            // seems like a nice optimization that can be added back in.
            // There needs to be time between the PREFETCH and the use.
            // Previously we put the obj in an 8 element buffer that is drained at a rate
            // to give the PREFETCH time to do its work.
            // Use of PREFETCHNTA might be more appropriate than PREFETCH
            if (!gcw.putFast(obj))
            {
                gcw.put(obj);
            }
        }

        // gcDumpObject dumps the contents of obj for debugging and marks the
        // field at byte offset off in obj.
        private static void gcDumpObject(@string label, System.UIntPtr obj, System.UIntPtr off)
        {
            if (obj < mheap_.arena_start || obj >= mheap_.arena_used)
            {
                print(label, "=", hex(obj), " is not in the Go heap\n");
                return;
            }
            var k = obj >> (int)(_PageShift);
            var x = k;
            x -= mheap_.arena_start >> (int)(_PageShift);
            var s = mheap_.spans[x];
            print(label, "=", hex(obj), " k=", hex(k));
            if (s == null)
            {
                print(" s=nil\n");
                return;
            }
            print(" s.base()=", hex(s.@base()), " s.limit=", hex(s.limit), " s.spanclass=", s.spanclass, " s.elemsize=", s.elemsize, " s.state=");
            if (0L <= s.state && int(s.state) < len(mSpanStateNames))
            {
                print(mSpanStateNames[s.state], "\n");
            }
            else
            {
                print("unknown(", s.state, ")\n");
            }
            var skipped = false;
            var size = s.elemsize;
            if (s.state == _MSpanManual && size == 0L)
            { 
                // We're printing something from a stack frame. We
                // don't know how big it is, so just show up to an
                // including off.
                size = off + sys.PtrSize;
            }
            {
                var i = uintptr(0L);

                while (i < size)
                { 
                    // For big objects, just print the beginning (because
                    // that usually hints at the object's type) and the
                    // fields around off.
                    if (!(i < 128L * sys.PtrSize || off - 16L * sys.PtrSize < i && i < off + 16L * sys.PtrSize))
                    {
                        skipped = true;
                        continue;
                    i += sys.PtrSize;
                    }
                    if (skipped)
                    {
                        print(" ...\n");
                        skipped = false;
                    }
                    print(" *(", label, "+", i, ") = ", hex(@unsafe.Pointer(obj + i).Value));
                    if (i == off)
                    {
                        print(" <==");
                    }
                    print("\n");
                }

            }
            if (skipped)
            {
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
        private static void gcmarknewobject(System.UIntPtr obj, System.UIntPtr size, System.UIntPtr scanSize)
        {
            if (useCheckmark && !gcBlackenPromptly)
            { // The world should be stopped so this should not happen.
                throw("gcmarknewobject called while doing checkmark");
            }
            markBitsForAddr(obj).setMarked();
            var gcw = ref getg().m.p.ptr().gcw;
            gcw.bytesMarked += uint64(size);
            gcw.scanWork += int64(scanSize);
            if (gcBlackenPromptly)
            { 
                // There shouldn't be anything in the work queue, but
                // we still need to flush stats.
                gcw.dispose();
            }
        }

        // gcMarkTinyAllocs greys all active tiny alloc blocks.
        //
        // The world must be stopped.
        private static void gcMarkTinyAllocs()
        {
            foreach (var (_, p) in allp)
            {
                var c = p.mcache;
                if (c == null || c.tiny == 0L)
                {
                    continue;
                }
                var (_, hbits, span, objIndex) = heapBitsForObject(c.tiny, 0L, 0L);
                var gcw = ref p.gcw;
                greyobject(c.tiny, 0L, 0L, hbits, span, gcw, objIndex);
                if (gcBlackenPromptly)
                {
                    gcw.dispose();
                }
            }
        }

        // Checkmarking

        // To help debug the concurrent GC we remark with the world
        // stopped ensuring that any object encountered has their normal
        // mark bit set. To do this we use an orthogonal bit
        // pattern to indicate the object is marked. The following pattern
        // uses the upper two bits in the object's boundary nibble.
        // 01: scalar  not marked
        // 10: pointer not marked
        // 11: pointer     marked
        // 00: scalar      marked
        // Xoring with 01 will flip the pattern from marked to unmarked and vica versa.
        // The higher bit is 1 for pointers and 0 for scalars, whether the object
        // is marked or not.
        // The first nibble no longer holds the typeDead pattern indicating that the
        // there are no more pointers in the object. This information is held
        // in the second nibble.

        // If useCheckmark is true, marking of an object uses the
        // checkmark bits (encoding above) instead of the standard
        // mark bits.
        private static var useCheckmark = false;

        //go:nowritebarrier
        private static void initCheckmarks()
        {
            useCheckmark = true;
            foreach (var (_, s) in mheap_.allspans)
            {
                if (s.state == _MSpanInUse)
                {
                    heapBitsForSpan(s.@base()).initCheckmarkSpan(s.layout());
                }
            }
        }

        private static void clearCheckmarks()
        {
            useCheckmark = false;
            foreach (var (_, s) in mheap_.allspans)
            {
                if (s.state == _MSpanInUse)
                {
                    heapBitsForSpan(s.@base()).clearCheckmarkSpan(s.layout());
                }
            }
        }
    }
}
