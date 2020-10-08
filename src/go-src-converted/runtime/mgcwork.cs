// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 October 08 03:21:08 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\mgcwork.go
using atomic = go.runtime.@internal.atomic_package;
using sys = go.runtime.@internal.sys_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class runtime_package
    {
        private static readonly long _WorkbufSize = (long)2048L; // in bytes; larger values result in less contention

        // workbufAlloc is the number of bytes to allocate at a time
        // for new workbufs. This must be a multiple of pageSize and
        // should be a multiple of _WorkbufSize.
        //
        // Larger values reduce workbuf allocation overhead. Smaller
        // values reduce heap fragmentation.
        private static readonly long workbufAlloc = (long)32L << (int)(10L);


        // throwOnGCWork causes any operations that add pointers to a gcWork
        // buffer to throw.
        //
        // TODO(austin): This is a temporary debugging measure for issue
        // #27993. To be removed before release.
        private static bool throwOnGCWork = default;

        private static void init()
        {
            if (workbufAlloc % pageSize != 0L || workbufAlloc % _WorkbufSize != 0L)
            {
                throw("bad workbufAlloc");
            }

        }

        // Garbage collector work pool abstraction.
        //
        // This implements a producer/consumer model for pointers to grey
        // objects. A grey object is one that is marked and on a work
        // queue. A black object is marked and not on a work queue.
        //
        // Write barriers, root discovery, stack scanning, and object scanning
        // produce pointers to grey objects. Scanning consumes pointers to
        // grey objects, thus blackening them, and then scans them,
        // potentially producing new pointers to grey objects.

        // A gcWork provides the interface to produce and consume work for the
        // garbage collector.
        //
        // A gcWork can be used on the stack as follows:
        //
        //     (preemption must be disabled)
        //     gcw := &getg().m.p.ptr().gcw
        //     .. call gcw.put() to produce and gcw.tryGet() to consume ..
        //
        // It's important that any use of gcWork during the mark phase prevent
        // the garbage collector from transitioning to mark termination since
        // gcWork may locally hold GC work buffers. This can be done by
        // disabling preemption (systemstack or acquirem).
        private partial struct gcWork
        {
            public ptr<workbuf> wbuf1; // Bytes marked (blackened) on this gcWork. This is aggregated
// into work.bytesMarked by dispose.
            public ptr<workbuf> wbuf2; // Bytes marked (blackened) on this gcWork. This is aggregated
// into work.bytesMarked by dispose.
            public ulong bytesMarked; // Scan work performed on this gcWork. This is aggregated into
// gcController by dispose and may also be flushed by callers.
            public long scanWork; // flushedWork indicates that a non-empty work buffer was
// flushed to the global work list since the last gcMarkDone
// termination check. Specifically, this indicates that this
// gcWork may have communicated work to another gcWork.
            public bool flushedWork; // pauseGen causes put operations to spin while pauseGen ==
// gcWorkPauseGen if debugCachedWork is true.
            public uint pauseGen; // putGen is the pauseGen of the last putGen.
            public uint putGen; // pauseStack is the stack at which this P was paused if
// debugCachedWork is true.
            public array<System.UIntPtr> pauseStack;
        }

        // Most of the methods of gcWork are go:nowritebarrierrec because the
        // write barrier itself can invoke gcWork methods but the methods are
        // not generally re-entrant. Hence, if a gcWork method invoked the
        // write barrier while the gcWork was in an inconsistent state, and
        // the write barrier in turn invoked a gcWork method, it could
        // permanently corrupt the gcWork.

        private static void init(this ptr<gcWork> _addr_w)
        {
            ref gcWork w = ref _addr_w.val;

            w.wbuf1 = getempty();
            var wbuf2 = trygetfull();
            if (wbuf2 == null)
            {
                wbuf2 = getempty();
            }

            w.wbuf2 = wbuf2;

        }

        private static void checkPut(this ptr<gcWork> _addr_w, System.UIntPtr ptr, slice<System.UIntPtr> ptrs)
        {
            ref gcWork w = ref _addr_w.val;

            if (debugCachedWork)
            {
                var alreadyFailed = w.putGen == w.pauseGen;
                w.putGen = w.pauseGen;
                if (!canPreemptM(getg().m))
                { 
                    // If we were to spin, the runtime may
                    // deadlock. Since we can't be preempted, the
                    // spin could prevent gcMarkDone from
                    // finishing the ragged barrier, which is what
                    // releases us from the spin.
                    return ;

                }

                while (atomic.Load(_addr_gcWorkPauseGen) == w.pauseGen)
                {
                }

                if (throwOnGCWork)
                {
                    printlock();
                    if (alreadyFailed)
                    {
                        println("runtime: checkPut already failed at this generation");
                    }

                    println("runtime: late gcWork put");
                    if (ptr != 0L)
                    {
                        gcDumpObject("ptr", ptr, ~uintptr(0L));
                    }

                    foreach (var (_, ptr) in ptrs)
                    {
                        gcDumpObject("ptrs", ptr, ~uintptr(0L));
                    }
                    println("runtime: paused at");
                    foreach (var (_, pc) in w.pauseStack)
                    {
                        if (pc == 0L)
                        {
                            break;
                        }

                        var f = findfunc(pc);
                        if (f.valid())
                        { 
                            // Obviously this doesn't
                            // relate to ancestor
                            // tracebacks, but this
                            // function prints what we
                            // want.
                            printAncestorTracebackFuncInfo(f, pc);

                        }
                        else
                        {
                            println("\tunknown PC ", hex(pc), "\n");
                        }

                    }
                    throw("throwOnGCWork");

                }

            }

        }

        // put enqueues a pointer for the garbage collector to trace.
        // obj must point to the beginning of a heap object or an oblet.
        //go:nowritebarrierrec
        private static void put(this ptr<gcWork> _addr_w, System.UIntPtr obj)
        {
            ref gcWork w = ref _addr_w.val;

            w.checkPut(obj, null);

            var flushed = false;
            var wbuf = w.wbuf1; 
            // Record that this may acquire the wbufSpans or heap lock to
            // allocate a workbuf.
            lockWithRankMayAcquire(_addr_work.wbufSpans.@lock, lockRankWbufSpans);
            lockWithRankMayAcquire(_addr_mheap_.@lock, lockRankMheap);
            if (wbuf == null)
            {
                w.init();
                wbuf = w.wbuf1; 
                // wbuf is empty at this point.
            }
            else if (wbuf.nobj == len(wbuf.obj))
            {
                w.wbuf1 = w.wbuf2;
                w.wbuf2 = w.wbuf1;
                wbuf = w.wbuf1;
                if (wbuf.nobj == len(wbuf.obj))
                {
                    putfull(_addr_wbuf);
                    w.flushedWork = true;
                    wbuf = getempty();
                    w.wbuf1 = wbuf;
                    flushed = true;
                }

            }

            wbuf.obj[wbuf.nobj] = obj;
            wbuf.nobj++; 

            // If we put a buffer on full, let the GC controller know so
            // it can encourage more workers to run. We delay this until
            // the end of put so that w is in a consistent state, since
            // enlistWorker may itself manipulate w.
            if (flushed && gcphase == _GCmark)
            {
                gcController.enlistWorker();
            }

        }

        // putFast does a put and reports whether it can be done quickly
        // otherwise it returns false and the caller needs to call put.
        //go:nowritebarrierrec
        private static bool putFast(this ptr<gcWork> _addr_w, System.UIntPtr obj)
        {
            ref gcWork w = ref _addr_w.val;

            w.checkPut(obj, null);

            var wbuf = w.wbuf1;
            if (wbuf == null)
            {
                return false;
            }
            else if (wbuf.nobj == len(wbuf.obj))
            {
                return false;
            }

            wbuf.obj[wbuf.nobj] = obj;
            wbuf.nobj++;
            return true;

        }

        // putBatch performs a put on every pointer in obj. See put for
        // constraints on these pointers.
        //
        //go:nowritebarrierrec
        private static void putBatch(this ptr<gcWork> _addr_w, slice<System.UIntPtr> obj)
        {
            ref gcWork w = ref _addr_w.val;

            if (len(obj) == 0L)
            {
                return ;
            }

            w.checkPut(0L, obj);

            var flushed = false;
            var wbuf = w.wbuf1;
            if (wbuf == null)
            {
                w.init();
                wbuf = w.wbuf1;
            }

            while (len(obj) > 0L)
            {
                while (wbuf.nobj == len(wbuf.obj))
                {
                    putfull(_addr_wbuf);
                    w.flushedWork = true;
                    w.wbuf1 = w.wbuf2;
                    w.wbuf2 = getempty();
                    wbuf = w.wbuf1;
                    flushed = true;

                }

                var n = copy(wbuf.obj[wbuf.nobj..], obj);
                wbuf.nobj += n;
                obj = obj[n..];

            }


            if (flushed && gcphase == _GCmark)
            {
                gcController.enlistWorker();
            }

        }

        // tryGet dequeues a pointer for the garbage collector to trace.
        //
        // If there are no pointers remaining in this gcWork or in the global
        // queue, tryGet returns 0.  Note that there may still be pointers in
        // other gcWork instances or other caches.
        //go:nowritebarrierrec
        private static System.UIntPtr tryGet(this ptr<gcWork> _addr_w)
        {
            ref gcWork w = ref _addr_w.val;

            var wbuf = w.wbuf1;
            if (wbuf == null)
            {
                w.init();
                wbuf = w.wbuf1; 
                // wbuf is empty at this point.
            }

            if (wbuf.nobj == 0L)
            {
                w.wbuf1 = w.wbuf2;
                w.wbuf2 = w.wbuf1;
                wbuf = w.wbuf1;
                if (wbuf.nobj == 0L)
                {
                    var owbuf = wbuf;
                    wbuf = trygetfull();
                    if (wbuf == null)
                    {
                        return 0L;
                    }

                    putempty(_addr_owbuf);
                    w.wbuf1 = wbuf;

                }

            }

            wbuf.nobj--;
            return wbuf.obj[wbuf.nobj];

        }

        // tryGetFast dequeues a pointer for the garbage collector to trace
        // if one is readily available. Otherwise it returns 0 and
        // the caller is expected to call tryGet().
        //go:nowritebarrierrec
        private static System.UIntPtr tryGetFast(this ptr<gcWork> _addr_w)
        {
            ref gcWork w = ref _addr_w.val;

            var wbuf = w.wbuf1;
            if (wbuf == null)
            {
                return 0L;
            }

            if (wbuf.nobj == 0L)
            {
                return 0L;
            }

            wbuf.nobj--;
            return wbuf.obj[wbuf.nobj];

        }

        // dispose returns any cached pointers to the global queue.
        // The buffers are being put on the full queue so that the
        // write barriers will not simply reacquire them before the
        // GC can inspect them. This helps reduce the mutator's
        // ability to hide pointers during the concurrent mark phase.
        //
        //go:nowritebarrierrec
        private static void dispose(this ptr<gcWork> _addr_w)
        {
            ref gcWork w = ref _addr_w.val;

            {
                var wbuf = w.wbuf1;

                if (wbuf != null)
                {
                    if (wbuf.nobj == 0L)
                    {
                        putempty(_addr_wbuf);
                    }
                    else
                    {
                        putfull(_addr_wbuf);
                        w.flushedWork = true;
                    }

                    w.wbuf1 = null;

                    wbuf = w.wbuf2;
                    if (wbuf.nobj == 0L)
                    {
                        putempty(_addr_wbuf);
                    }
                    else
                    {
                        putfull(_addr_wbuf);
                        w.flushedWork = true;
                    }

                    w.wbuf2 = null;

                }

            }

            if (w.bytesMarked != 0L)
            { 
                // dispose happens relatively infrequently. If this
                // atomic becomes a problem, we should first try to
                // dispose less and if necessary aggregate in a per-P
                // counter.
                atomic.Xadd64(_addr_work.bytesMarked, int64(w.bytesMarked));
                w.bytesMarked = 0L;

            }

            if (w.scanWork != 0L)
            {
                atomic.Xaddint64(_addr_gcController.scanWork, w.scanWork);
                w.scanWork = 0L;
            }

        }

        // balance moves some work that's cached in this gcWork back on the
        // global queue.
        //go:nowritebarrierrec
        private static void balance(this ptr<gcWork> _addr_w)
        {
            ref gcWork w = ref _addr_w.val;

            if (w.wbuf1 == null)
            {
                return ;
            }

            {
                var wbuf__prev1 = wbuf;

                var wbuf = w.wbuf2;

                if (wbuf.nobj != 0L)
                {
                    w.checkPut(0L, wbuf.obj[..wbuf.nobj]);
                    putfull(_addr_wbuf);
                    w.flushedWork = true;
                    w.wbuf2 = getempty();
                }                {
                    var wbuf__prev2 = wbuf;

                    wbuf = w.wbuf1;


                    else if (wbuf.nobj > 4L)
                    {
                        w.checkPut(0L, wbuf.obj[..wbuf.nobj]);
                        w.wbuf1 = handoff(_addr_wbuf);
                        w.flushedWork = true; // handoff did putfull
                    }
                    else
                    {
                        return ;
                    } 
                    // We flushed a buffer to the full list, so wake a worker.

                    wbuf = wbuf__prev2;

                } 
                // We flushed a buffer to the full list, so wake a worker.

                wbuf = wbuf__prev1;

            } 
            // We flushed a buffer to the full list, so wake a worker.
            if (gcphase == _GCmark)
            {
                gcController.enlistWorker();
            }

        }

        // empty reports whether w has no mark work available.
        //go:nowritebarrierrec
        private static bool empty(this ptr<gcWork> _addr_w)
        {
            ref gcWork w = ref _addr_w.val;

            return w.wbuf1 == null || (w.wbuf1.nobj == 0L && w.wbuf2.nobj == 0L);
        }

        // Internally, the GC work pool is kept in arrays in work buffers.
        // The gcWork interface caches a work buffer until full (or empty) to
        // avoid contending on the global work buffer lists.

        private partial struct workbufhdr
        {
            public lfnode node; // must be first
            public long nobj;
        }

        //go:notinheap
        private partial struct workbuf
        {
            public ref workbufhdr workbufhdr => ref workbufhdr_val; // account for the above fields
            public array<System.UIntPtr> obj;
        }

        // workbuf factory routines. These funcs are used to manage the
        // workbufs.
        // If the GC asks for some work these are the only routines that
        // make wbufs available to the GC.

        private static void checknonempty(this ptr<workbuf> _addr_b)
        {
            ref workbuf b = ref _addr_b.val;

            if (b.nobj == 0L)
            {
                throw("workbuf is empty");
            }

        }

        private static void checkempty(this ptr<workbuf> _addr_b)
        {
            ref workbuf b = ref _addr_b.val;

            if (b.nobj != 0L)
            {
                throw("workbuf is not empty");
            }

        }

        // getempty pops an empty work buffer off the work.empty list,
        // allocating new buffers if none are available.
        //go:nowritebarrier
        private static ptr<workbuf> getempty()
        {
            ptr<workbuf> b;
            if (work.empty != 0L)
            {
                b = (workbuf.val)(work.empty.pop());
                if (b != null)
                {
                    b.checkempty();
                }

            } 
            // Record that this may acquire the wbufSpans or heap lock to
            // allocate a workbuf.
            lockWithRankMayAcquire(_addr_work.wbufSpans.@lock, lockRankWbufSpans);
            lockWithRankMayAcquire(_addr_mheap_.@lock, lockRankMheap);
            if (b == null)
            { 
                // Allocate more workbufs.
                ptr<mspan> s;
                if (work.wbufSpans.free.first != null)
                {
                    lock(_addr_work.wbufSpans.@lock);
                    s = work.wbufSpans.free.first;
                    if (s != null)
                    {
                        work.wbufSpans.free.remove(s);
                        work.wbufSpans.busy.insert(s);
                    }

                    unlock(_addr_work.wbufSpans.@lock);

                }

                if (s == null)
                {
                    systemstack(() =>
                    {
                        s = mheap_.allocManual(workbufAlloc / pageSize, _addr_memstats.gc_sys);
                    });
                    if (s == null)
                    {
                        throw("out of memory");
                    } 
                    // Record the new span in the busy list.
                    lock(_addr_work.wbufSpans.@lock);
                    work.wbufSpans.busy.insert(s);
                    unlock(_addr_work.wbufSpans.@lock);

                } 
                // Slice up the span into new workbufs. Return one and
                // put the rest on the empty list.
                {
                    var i = uintptr(0L);

                    while (i + _WorkbufSize <= workbufAlloc)
                    {
                        var newb = (workbuf.val)(@unsafe.Pointer(s.@base() + i));
                        newb.nobj = 0L;
                        lfnodeValidate(_addr_newb.node);
                        if (i == 0L)
                        {
                            b = newb;
                        i += _WorkbufSize;
                        }
                        else
                        {
                            putempty(_addr_newb);
                        }

                    }

                }

            }

            return _addr_b!;

        }

        // putempty puts a workbuf onto the work.empty list.
        // Upon entry this go routine owns b. The lfstack.push relinquishes ownership.
        //go:nowritebarrier
        private static void putempty(ptr<workbuf> _addr_b)
        {
            ref workbuf b = ref _addr_b.val;

            b.checkempty();
            work.empty.push(_addr_b.node);
        }

        // putfull puts the workbuf on the work.full list for the GC.
        // putfull accepts partially full buffers so the GC can avoid competing
        // with the mutators for ownership of partially full buffers.
        //go:nowritebarrier
        private static void putfull(ptr<workbuf> _addr_b)
        {
            ref workbuf b = ref _addr_b.val;

            b.checknonempty();
            work.full.push(_addr_b.node);
        }

        // trygetfull tries to get a full or partially empty workbuffer.
        // If one is not immediately available return nil
        //go:nowritebarrier
        private static ptr<workbuf> trygetfull()
        {
            var b = (workbuf.val)(work.full.pop());
            if (b != null)
            {
                b.checknonempty();
                return _addr_b!;
            }

            return _addr_b!;

        }

        //go:nowritebarrier
        private static ptr<workbuf> handoff(ptr<workbuf> _addr_b)
        {
            ref workbuf b = ref _addr_b.val;
 
            // Make new buffer with half of b's pointers.
            var b1 = getempty();
            var n = b.nobj / 2L;
            b.nobj -= n;
            b1.nobj = n;
            memmove(@unsafe.Pointer(_addr_b1.obj[0L]), @unsafe.Pointer(_addr_b.obj[b.nobj]), uintptr(n) * @unsafe.Sizeof(b1.obj[0L])); 

            // Put b on full list - let first half of b get stolen.
            putfull(_addr_b);
            return _addr_b1!;

        }

        // prepareFreeWorkbufs moves busy workbuf spans to free list so they
        // can be freed to the heap. This must only be called when all
        // workbufs are on the empty list.
        private static void prepareFreeWorkbufs()
        {
            lock(_addr_work.wbufSpans.@lock);
            if (work.full != 0L)
            {
                throw("cannot free workbufs when work.full != 0");
            } 
            // Since all workbufs are on the empty list, we don't care
            // which ones are in which spans. We can wipe the entire empty
            // list and move all workbuf spans to the free list.
            work.empty = 0L;
            work.wbufSpans.free.takeAll(_addr_work.wbufSpans.busy);
            unlock(_addr_work.wbufSpans.@lock);

        }

        // freeSomeWbufs frees some workbufs back to the heap and returns
        // true if it should be called again to free more.
        private static bool freeSomeWbufs(bool preemptible)
        {
            const long batchSize = (long)64L; // ~1–2 µs per span.
 // ~1–2 µs per span.
            lock(_addr_work.wbufSpans.@lock);
            if (gcphase != _GCoff || work.wbufSpans.free.isEmpty())
            {
                unlock(_addr_work.wbufSpans.@lock);
                return false;
            }

            systemstack(() =>
            {
                var gp = getg().m.curg;
                for (long i = 0L; i < batchSize && !(preemptible && gp.preempt); i++)
                {
                    var span = work.wbufSpans.free.first;
                    if (span == null)
                    {
                        break;
                    }

                    work.wbufSpans.free.remove(span);
                    mheap_.freeManual(span, _addr_memstats.gc_sys);

                }


            });
            var more = !work.wbufSpans.free.isEmpty();
            unlock(_addr_work.wbufSpans.@lock);
            return more;

        }
    }
}
