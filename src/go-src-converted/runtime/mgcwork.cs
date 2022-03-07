// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2022 March 06 22:09:50 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\mgcwork.go
using atomic = go.runtime.@internal.atomic_package;
using sys = go.runtime.@internal.sys_package;
using @unsafe = go.@unsafe_package;
using System;


namespace go;

public static partial class runtime_package {

private static readonly nint _WorkbufSize = 2048; // in bytes; larger values result in less contention

// workbufAlloc is the number of bytes to allocate at a time
// for new workbufs. This must be a multiple of pageSize and
// should be a multiple of _WorkbufSize.
//
// Larger values reduce workbuf allocation overhead. Smaller
// values reduce heap fragmentation.
private static readonly nint workbufAlloc = 32 << 10;


private static void init() {
    if (workbufAlloc % pageSize != 0 || workbufAlloc % _WorkbufSize != 0) {
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
private partial struct gcWork {
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
    public bool flushedWork;
}

// Most of the methods of gcWork are go:nowritebarrierrec because the
// write barrier itself can invoke gcWork methods but the methods are
// not generally re-entrant. Hence, if a gcWork method invoked the
// write barrier while the gcWork was in an inconsistent state, and
// the write barrier in turn invoked a gcWork method, it could
// permanently corrupt the gcWork.

private static void init(this ptr<gcWork> _addr_w) {
    ref gcWork w = ref _addr_w.val;

    w.wbuf1 = getempty();
    var wbuf2 = trygetfull();
    if (wbuf2 == null) {
        wbuf2 = getempty();
    }
    w.wbuf2 = wbuf2;

}

// put enqueues a pointer for the garbage collector to trace.
// obj must point to the beginning of a heap object or an oblet.
//go:nowritebarrierrec
private static void put(this ptr<gcWork> _addr_w, System.UIntPtr obj) {
    ref gcWork w = ref _addr_w.val;

    var flushed = false;
    var wbuf = w.wbuf1; 
    // Record that this may acquire the wbufSpans or heap lock to
    // allocate a workbuf.
    lockWithRankMayAcquire(_addr_work.wbufSpans.@lock, lockRankWbufSpans);
    lockWithRankMayAcquire(_addr_mheap_.@lock, lockRankMheap);
    if (wbuf == null) {
        w.init();
        wbuf = w.wbuf1; 
        // wbuf is empty at this point.
    }
    else if (wbuf.nobj == len(wbuf.obj)) {
        (w.wbuf1, w.wbuf2) = (w.wbuf2, w.wbuf1);        wbuf = w.wbuf1;
        if (wbuf.nobj == len(wbuf.obj)) {
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
    if (flushed && gcphase == _GCmark) {
        gcController.enlistWorker();
    }
}

// putFast does a put and reports whether it can be done quickly
// otherwise it returns false and the caller needs to call put.
//go:nowritebarrierrec
private static bool putFast(this ptr<gcWork> _addr_w, System.UIntPtr obj) {
    ref gcWork w = ref _addr_w.val;

    var wbuf = w.wbuf1;
    if (wbuf == null) {
        return false;
    }
    else if (wbuf.nobj == len(wbuf.obj)) {
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
private static void putBatch(this ptr<gcWork> _addr_w, slice<System.UIntPtr> obj) {
    ref gcWork w = ref _addr_w.val;

    if (len(obj) == 0) {
        return ;
    }
    var flushed = false;
    var wbuf = w.wbuf1;
    if (wbuf == null) {
        w.init();
        wbuf = w.wbuf1;
    }
    while (len(obj) > 0) {
        while (wbuf.nobj == len(wbuf.obj)) {
            putfull(_addr_wbuf);
            w.flushedWork = true;
            (w.wbuf1, w.wbuf2) = (w.wbuf2, getempty());            wbuf = w.wbuf1;
            flushed = true;
        }
        var n = copy(wbuf.obj[(int)wbuf.nobj..], obj);
        wbuf.nobj += n;
        obj = obj[(int)n..];
    }

    if (flushed && gcphase == _GCmark) {
        gcController.enlistWorker();
    }
}

// tryGet dequeues a pointer for the garbage collector to trace.
//
// If there are no pointers remaining in this gcWork or in the global
// queue, tryGet returns 0.  Note that there may still be pointers in
// other gcWork instances or other caches.
//go:nowritebarrierrec
private static System.UIntPtr tryGet(this ptr<gcWork> _addr_w) {
    ref gcWork w = ref _addr_w.val;

    var wbuf = w.wbuf1;
    if (wbuf == null) {
        w.init();
        wbuf = w.wbuf1; 
        // wbuf is empty at this point.
    }
    if (wbuf.nobj == 0) {
        (w.wbuf1, w.wbuf2) = (w.wbuf2, w.wbuf1);        wbuf = w.wbuf1;
        if (wbuf.nobj == 0) {
            var owbuf = wbuf;
            wbuf = trygetfull();
            if (wbuf == null) {
                return 0;
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
private static System.UIntPtr tryGetFast(this ptr<gcWork> _addr_w) {
    ref gcWork w = ref _addr_w.val;

    var wbuf = w.wbuf1;
    if (wbuf == null) {
        return 0;
    }
    if (wbuf.nobj == 0) {
        return 0;
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
private static void dispose(this ptr<gcWork> _addr_w) {
    ref gcWork w = ref _addr_w.val;

    {
        var wbuf = w.wbuf1;

        if (wbuf != null) {
            if (wbuf.nobj == 0) {
                putempty(_addr_wbuf);
            }
            else
 {
                putfull(_addr_wbuf);
                w.flushedWork = true;
            }

            w.wbuf1 = null;

            wbuf = w.wbuf2;
            if (wbuf.nobj == 0) {
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

    if (w.bytesMarked != 0) { 
        // dispose happens relatively infrequently. If this
        // atomic becomes a problem, we should first try to
        // dispose less and if necessary aggregate in a per-P
        // counter.
        atomic.Xadd64(_addr_work.bytesMarked, int64(w.bytesMarked));
        w.bytesMarked = 0;

    }
    if (w.scanWork != 0) {
        atomic.Xaddint64(_addr_gcController.scanWork, w.scanWork);
        w.scanWork = 0;
    }
}

// balance moves some work that's cached in this gcWork back on the
// global queue.
//go:nowritebarrierrec
private static void balance(this ptr<gcWork> _addr_w) {
    ref gcWork w = ref _addr_w.val;

    if (w.wbuf1 == null) {
        return ;
    }
    {
        var wbuf__prev1 = wbuf;

        var wbuf = w.wbuf2;

        if (wbuf.nobj != 0) {
            putfull(_addr_wbuf);
            w.flushedWork = true;
            w.wbuf2 = getempty();
        }        {
            var wbuf__prev2 = wbuf;

            wbuf = w.wbuf1;


            else if (wbuf.nobj > 4) {
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
    if (gcphase == _GCmark) {
        gcController.enlistWorker();
    }
}

// empty reports whether w has no mark work available.
//go:nowritebarrierrec
private static bool empty(this ptr<gcWork> _addr_w) {
    ref gcWork w = ref _addr_w.val;

    return w.wbuf1 == null || (w.wbuf1.nobj == 0 && w.wbuf2.nobj == 0);
}

// Internally, the GC work pool is kept in arrays in work buffers.
// The gcWork interface caches a work buffer until full (or empty) to
// avoid contending on the global work buffer lists.

private partial struct workbufhdr {
    public lfnode node; // must be first
    public nint nobj;
}

//go:notinheap
private partial struct workbuf {
    public ref workbufhdr workbufhdr => ref workbufhdr_val; // account for the above fields
    public array<System.UIntPtr> obj;
}

// workbuf factory routines. These funcs are used to manage the
// workbufs.
// If the GC asks for some work these are the only routines that
// make wbufs available to the GC.

private static void checknonempty(this ptr<workbuf> _addr_b) {
    ref workbuf b = ref _addr_b.val;

    if (b.nobj == 0) {
        throw("workbuf is empty");
    }
}

private static void checkempty(this ptr<workbuf> _addr_b) {
    ref workbuf b = ref _addr_b.val;

    if (b.nobj != 0) {
        throw("workbuf is not empty");
    }
}

// getempty pops an empty work buffer off the work.empty list,
// allocating new buffers if none are available.
//go:nowritebarrier
private static ptr<workbuf> getempty() {
    ptr<workbuf> b;
    if (work.empty != 0) {
        b = (workbuf.val)(work.empty.pop());
        if (b != null) {
            b.checkempty();
        }
    }
    lockWithRankMayAcquire(_addr_work.wbufSpans.@lock, lockRankWbufSpans);
    lockWithRankMayAcquire(_addr_mheap_.@lock, lockRankMheap);
    if (b == null) { 
        // Allocate more workbufs.
        ptr<mspan> s;
        if (work.wbufSpans.free.first != null) {
            lock(_addr_work.wbufSpans.@lock);
            s = work.wbufSpans.free.first;
            if (s != null) {
                work.wbufSpans.free.remove(s);
                work.wbufSpans.busy.insert(s);
            }
            unlock(_addr_work.wbufSpans.@lock);
        }
        if (s == null) {
            systemstack(() => {
                s = mheap_.allocManual(workbufAlloc / pageSize, spanAllocWorkBuf);
            });
            if (s == null) {
                throw("out of memory");
            } 
            // Record the new span in the busy list.
            lock(_addr_work.wbufSpans.@lock);
            work.wbufSpans.busy.insert(s);
            unlock(_addr_work.wbufSpans.@lock);

        }
        {
            var i = uintptr(0);

            while (i + _WorkbufSize <= workbufAlloc) {
                var newb = (workbuf.val)(@unsafe.Pointer(s.@base() + i));
                newb.nobj = 0;
                lfnodeValidate(_addr_newb.node);
                if (i == 0) {
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
// Upon entry this goroutine owns b. The lfstack.push relinquishes ownership.
//go:nowritebarrier
private static void putempty(ptr<workbuf> _addr_b) {
    ref workbuf b = ref _addr_b.val;

    b.checkempty();
    work.empty.push(_addr_b.node);
}

// putfull puts the workbuf on the work.full list for the GC.
// putfull accepts partially full buffers so the GC can avoid competing
// with the mutators for ownership of partially full buffers.
//go:nowritebarrier
private static void putfull(ptr<workbuf> _addr_b) {
    ref workbuf b = ref _addr_b.val;

    b.checknonempty();
    work.full.push(_addr_b.node);
}

// trygetfull tries to get a full or partially empty workbuffer.
// If one is not immediately available return nil
//go:nowritebarrier
private static ptr<workbuf> trygetfull() {
    var b = (workbuf.val)(work.full.pop());
    if (b != null) {
        b.checknonempty();
        return _addr_b!;
    }
    return _addr_b!;

}

//go:nowritebarrier
private static ptr<workbuf> handoff(ptr<workbuf> _addr_b) {
    ref workbuf b = ref _addr_b.val;
 
    // Make new buffer with half of b's pointers.
    var b1 = getempty();
    var n = b.nobj / 2;
    b.nobj -= n;
    b1.nobj = n;
    memmove(@unsafe.Pointer(_addr_b1.obj[0]), @unsafe.Pointer(_addr_b.obj[b.nobj]), uintptr(n) * @unsafe.Sizeof(b1.obj[0])); 

    // Put b on full list - let first half of b get stolen.
    putfull(_addr_b);
    return _addr_b1!;

}

// prepareFreeWorkbufs moves busy workbuf spans to free list so they
// can be freed to the heap. This must only be called when all
// workbufs are on the empty list.
private static void prepareFreeWorkbufs() {
    lock(_addr_work.wbufSpans.@lock);
    if (work.full != 0) {
        throw("cannot free workbufs when work.full != 0");
    }
    work.empty = 0;
    work.wbufSpans.free.takeAll(_addr_work.wbufSpans.busy);
    unlock(_addr_work.wbufSpans.@lock);

}

// freeSomeWbufs frees some workbufs back to the heap and returns
// true if it should be called again to free more.
private static bool freeSomeWbufs(bool preemptible) {
    const nint batchSize = 64; // ~1–2 µs per span.
 // ~1–2 µs per span.
    lock(_addr_work.wbufSpans.@lock);
    if (gcphase != _GCoff || work.wbufSpans.free.isEmpty()) {
        unlock(_addr_work.wbufSpans.@lock);
        return false;
    }
    systemstack(() => {
        var gp = getg().m.curg;
        for (nint i = 0; i < batchSize && !(preemptible && gp.preempt); i++) {
            var span = work.wbufSpans.free.first;
            if (span == null) {
                break;
            }
            work.wbufSpans.free.remove(span);
            mheap_.freeManual(span, spanAllocWorkBuf);
        }
    });
    var more = !work.wbufSpans.free.isEmpty();
    unlock(_addr_work.wbufSpans.@lock);
    return more;

}

} // end runtime_package
