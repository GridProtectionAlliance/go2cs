// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 August 29 08:18:15 UTC
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
        private static readonly long _WorkbufSize = 2048L; // in bytes; larger values result in less contention

        // workbufAlloc is the number of bytes to allocate at a time
        // for new workbufs. This must be a multiple of pageSize and
        // should be a multiple of _WorkbufSize.
        //
        // Larger values reduce workbuf allocation overhead. Smaller
        // values reduce heap fragmentation.
        private static readonly long workbufAlloc = 32L << (int)(10L);

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
        //     .. call gcw.put() to produce and gcw.get() to consume ..
        //     if gcBlackenPromptly {
        //         gcw.dispose()
        //     }
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
            public long scanWork;
        }

        // Most of the methods of gcWork are go:nowritebarrierrec because the
        // write barrier itself can invoke gcWork methods but the methods are
        // not generally re-entrant. Hence, if a gcWork method invoked the
        // write barrier while the gcWork was in an inconsistent state, and
        // the write barrier in turn invoked a gcWork method, it could
        // permanently corrupt the gcWork.

        private static void init(this ref gcWork w)
        {
            w.wbuf1 = getempty();
            var wbuf2 = trygetfull();
            if (wbuf2 == null)
            {
                wbuf2 = getempty();
            }
            w.wbuf2 = wbuf2;
        }

        // put enqueues a pointer for the garbage collector to trace.
        // obj must point to the beginning of a heap object or an oblet.
        //go:nowritebarrierrec
        private static void put(this ref gcWork w, System.UIntPtr obj)
        {
            var flushed = false;
            var wbuf = w.wbuf1;
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
                    putfull(wbuf);
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

        // putFast does a put and returns true if it can be done quickly
        // otherwise it returns false and the caller needs to call put.
        //go:nowritebarrierrec
        private static bool putFast(this ref gcWork w, System.UIntPtr obj)
        {
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
        private static void putBatch(this ref gcWork w, slice<System.UIntPtr> obj)
        {
            if (len(obj) == 0L)
            {
                return;
            }
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
                    putfull(wbuf);
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
        private static System.UIntPtr tryGet(this ref gcWork w)
        {
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
                    putempty(owbuf);
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
        private static System.UIntPtr tryGetFast(this ref gcWork w)
        {
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

        // get dequeues a pointer for the garbage collector to trace, blocking
        // if necessary to ensure all pointers from all queues and caches have
        // been retrieved.  get returns 0 if there are no pointers remaining.
        //go:nowritebarrierrec
        private static System.UIntPtr get(this ref gcWork w)
        {
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
                    wbuf = getfull();
                    if (wbuf == null)
                    {
                        return 0L;
                    }
                    putempty(owbuf);
                    w.wbuf1 = wbuf;
                }
            } 

            // TODO: This might be a good place to add prefetch code
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
        private static void dispose(this ref gcWork w)
        {
            {
                var wbuf = w.wbuf1;

                if (wbuf != null)
                {
                    if (wbuf.nobj == 0L)
                    {
                        putempty(wbuf);
                    }
                    else
                    {
                        putfull(wbuf);
                    }
                    w.wbuf1 = null;

                    wbuf = w.wbuf2;
                    if (wbuf.nobj == 0L)
                    {
                        putempty(wbuf);
                    }
                    else
                    {
                        putfull(wbuf);
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
                atomic.Xadd64(ref work.bytesMarked, int64(w.bytesMarked));
                w.bytesMarked = 0L;
            }
            if (w.scanWork != 0L)
            {
                atomic.Xaddint64(ref gcController.scanWork, w.scanWork);
                w.scanWork = 0L;
            }
        }

        // balance moves some work that's cached in this gcWork back on the
        // global queue.
        //go:nowritebarrierrec
        private static void balance(this ref gcWork w)
        {
            if (w.wbuf1 == null)
            {
                return;
            }
            {
                var wbuf__prev1 = wbuf;

                var wbuf = w.wbuf2;

                if (wbuf.nobj != 0L)
                {
                    putfull(wbuf);
                    w.wbuf2 = getempty();
                }                {
                    var wbuf__prev2 = wbuf;

                    wbuf = w.wbuf1;


                    else if (wbuf.nobj > 4L)
                    {
                        w.wbuf1 = handoff(wbuf);
                    }
                    else
                    {
                        return;
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

        // empty returns true if w has no mark work available.
        //go:nowritebarrierrec
        private static bool empty(this ref gcWork w)
        {
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

        private static void checknonempty(this ref workbuf b)
        {
            if (b.nobj == 0L)
            {
                throw("workbuf is empty");
            }
        }

        private static void checkempty(this ref workbuf b)
        {
            if (b.nobj != 0L)
            {
                throw("workbuf is not empty");
            }
        }

        // getempty pops an empty work buffer off the work.empty list,
        // allocating new buffers if none are available.
        //go:nowritebarrier
        private static ref workbuf getempty()
        {
            ref workbuf b = default;
            if (work.empty != 0L)
            {
                b = (workbuf.Value)(work.empty.pop());
                if (b != null)
                {
                    b.checkempty();
                }
            }
            if (b == null)
            { 
                // Allocate more workbufs.
                ref mspan s = default;
                if (work.wbufSpans.free.first != null)
                {
                    lock(ref work.wbufSpans.@lock);
                    s = work.wbufSpans.free.first;
                    if (s != null)
                    {
                        work.wbufSpans.free.remove(s);
                        work.wbufSpans.busy.insert(s);
                    }
                    unlock(ref work.wbufSpans.@lock);
                }
                if (s == null)
                {
                    systemstack(() =>
                    {
                        s = mheap_.allocManual(workbufAlloc / pageSize, ref memstats.gc_sys);
                    });
                    if (s == null)
                    {
                        throw("out of memory");
                    } 
                    // Record the new span in the busy list.
                    lock(ref work.wbufSpans.@lock);
                    work.wbufSpans.busy.insert(s);
                    unlock(ref work.wbufSpans.@lock);
                } 
                // Slice up the span into new workbufs. Return one and
                // put the rest on the empty list.
                {
                    var i = uintptr(0L);

                    while (i + _WorkbufSize <= workbufAlloc)
                    {
                        var newb = (workbuf.Value)(@unsafe.Pointer(s.@base() + i));
                        newb.nobj = 0L;
                        if (i == 0L)
                        {
                            b = newb;
                        i += _WorkbufSize;
                        }
                        else
                        {
                            putempty(newb);
                        }
                    }

                }
            }
            return b;
        }

        // putempty puts a workbuf onto the work.empty list.
        // Upon entry this go routine owns b. The lfstack.push relinquishes ownership.
        //go:nowritebarrier
        private static void putempty(ref workbuf b)
        {
            b.checkempty();
            work.empty.push(ref b.node);
        }

        // putfull puts the workbuf on the work.full list for the GC.
        // putfull accepts partially full buffers so the GC can avoid competing
        // with the mutators for ownership of partially full buffers.
        //go:nowritebarrier
        private static void putfull(ref workbuf b)
        {
            b.checknonempty();
            work.full.push(ref b.node);
        }

        // trygetfull tries to get a full or partially empty workbuffer.
        // If one is not immediately available return nil
        //go:nowritebarrier
        private static ref workbuf trygetfull()
        {
            var b = (workbuf.Value)(work.full.pop());
            if (b != null)
            {
                b.checknonempty();
                return b;
            }
            return b;
        }

        // Get a full work buffer off the work.full list.
        // If nothing is available wait until all the other gc helpers have
        // finished and then return nil.
        // getfull acts as a barrier for work.nproc helpers. As long as one
        // gchelper is actively marking objects it
        // may create a workbuffer that the other helpers can work on.
        // The for loop either exits when a work buffer is found
        // or when _all_ of the work.nproc GC helpers are in the loop
        // looking for work and thus not capable of creating new work.
        // This is in fact the termination condition for the STW mark
        // phase.
        //go:nowritebarrier
        private static ref workbuf getfull()
        {
            var b = (workbuf.Value)(work.full.pop());
            if (b != null)
            {
                b.checknonempty();
                return b;
            }
            var incnwait = atomic.Xadd(ref work.nwait, +1L);
            if (incnwait > work.nproc)
            {
                println("runtime: work.nwait=", incnwait, "work.nproc=", work.nproc);
                throw("work.nwait > work.nproc");
            }
            for (long i = 0L; >>MARKER:FOREXPRESSION_LEVEL_1<<; i++)
            {
                if (work.full != 0L)
                {
                    var decnwait = atomic.Xadd(ref work.nwait, -1L);
                    if (decnwait == work.nproc)
                    {
                        println("runtime: work.nwait=", decnwait, "work.nproc=", work.nproc);
                        throw("work.nwait > work.nproc");
                    }
                    b = (workbuf.Value)(work.full.pop());
                    if (b != null)
                    {
                        b.checknonempty();
                        return b;
                    }
                    incnwait = atomic.Xadd(ref work.nwait, +1L);
                    if (incnwait > work.nproc)
                    {
                        println("runtime: work.nwait=", incnwait, "work.nproc=", work.nproc);
                        throw("work.nwait > work.nproc");
                    }
                }
                if (work.nwait == work.nproc && work.markrootNext >= work.markrootJobs)
                {
                    return null;
                }
                if (i < 10L)
                {
                    procyield(20L);
                }
                else if (i < 20L)
                {
                    osyield();
                }
                else
                {
                    usleep(100L);
                }
            }

        }

        //go:nowritebarrier
        private static ref workbuf handoff(ref workbuf b)
        { 
            // Make new buffer with half of b's pointers.
            var b1 = getempty();
            var n = b.nobj / 2L;
            b.nobj -= n;
            b1.nobj = n;
            memmove(@unsafe.Pointer(ref b1.obj[0L]), @unsafe.Pointer(ref b.obj[b.nobj]), uintptr(n) * @unsafe.Sizeof(b1.obj[0L])); 

            // Put b on full list - let first half of b get stolen.
            putfull(b);
            return b1;
        }

        // prepareFreeWorkbufs moves busy workbuf spans to free list so they
        // can be freed to the heap. This must only be called when all
        // workbufs are on the empty list.
        private static void prepareFreeWorkbufs()
        {
            lock(ref work.wbufSpans.@lock);
            if (work.full != 0L)
            {
                throw("cannot free workbufs when work.full != 0");
            } 
            // Since all workbufs are on the empty list, we don't care
            // which ones are in which spans. We can wipe the entire empty
            // list and move all workbuf spans to the free list.
            work.empty = 0L;
            work.wbufSpans.free.takeAll(ref work.wbufSpans.busy);
            unlock(ref work.wbufSpans.@lock);
        }

        // freeSomeWbufs frees some workbufs back to the heap and returns
        // true if it should be called again to free more.
        private static bool freeSomeWbufs(bool preemptible)
        {
            const long batchSize = 64L; // ~1–2 µs per span.
 // ~1–2 µs per span.
            lock(ref work.wbufSpans.@lock);
            if (gcphase != _GCoff || work.wbufSpans.free.isEmpty())
            {
                unlock(ref work.wbufSpans.@lock);
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
                    mheap_.freeManual(span, ref memstats.gc_sys);
                }

            });
            var more = !work.wbufSpans.free.isEmpty();
            unlock(ref work.wbufSpans.@lock);
            return more;
        }
    }
}
