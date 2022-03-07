// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2022 March 06 22:08:25 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\chan.go
// This file contains the implementation of Go channels.

// Invariants:
//  At least one of c.sendq and c.recvq is empty,
//  except for the case of an unbuffered channel with a single goroutine
//  blocked on it for both sending and receiving using a select statement,
//  in which case the length of c.sendq and c.recvq is limited only by the
//  size of the select statement.
//
// For buffered channels, also:
//  c.qcount > 0 implies that c.recvq is empty.
//  c.qcount < c.dataqsiz implies that c.sendq is empty.

using atomic = go.runtime.@internal.atomic_package;
using math = go.runtime.@internal.math_package;
using @unsafe = go.@unsafe_package;
using System;


namespace go;

public static partial class runtime_package {

private static readonly nint maxAlign = 8;
private static readonly var hchanSize = @unsafe.Sizeof(new hchan()) + uintptr(-int(@unsafe.Sizeof(new hchan())) & (maxAlign - 1));
private static readonly var debugChan = false;


private partial struct hchan {
    public nuint qcount; // total data in the queue
    public nuint dataqsiz; // size of the circular queue
    public unsafe.Pointer buf; // points to an array of dataqsiz elements
    public ushort elemsize;
    public uint closed;
    public ptr<_type> elemtype; // element type
    public nuint sendx; // send index
    public nuint recvx; // receive index
    public waitq recvq; // list of recv waiters
    public waitq sendq; // list of send waiters

// lock protects all fields in hchan, as well as several
// fields in sudogs blocked on this channel.
//
// Do not change another G's status while holding this lock
// (in particular, do not ready a G), as this can deadlock
// with stack shrinking.
    public mutex @lock;
}

private partial struct waitq {
    public ptr<sudog> first;
    public ptr<sudog> last;
}

//go:linkname reflect_makechan reflect.makechan
private static ptr<hchan> reflect_makechan(ptr<chantype> _addr_t, nint size) {
    ref chantype t = ref _addr_t.val;

    return _addr_makechan(_addr_t, size)!;
}

private static ptr<hchan> makechan64(ptr<chantype> _addr_t, long size) => func((_, panic, _) => {
    ref chantype t = ref _addr_t.val;

    if (int64(int(size)) != size) {
        panic(plainError("makechan: size out of range"));
    }
    return _addr_makechan(_addr_t, int(size))!;

});

private static ptr<hchan> makechan(ptr<chantype> _addr_t, nint size) => func((_, panic, _) => {
    ref chantype t = ref _addr_t.val;

    var elem = t.elem; 

    // compiler checks this but be safe.
    if (elem.size >= 1 << 16) {
        throw("makechan: invalid channel element type");
    }
    if (hchanSize % maxAlign != 0 || elem.align > maxAlign) {
        throw("makechan: bad alignment");
    }
    var (mem, overflow) = math.MulUintptr(elem.size, uintptr(size));
    if (overflow || mem > maxAlloc - hchanSize || size < 0) {
        panic(plainError("makechan: size out of range"));
    }
    ptr<hchan> c;

    if (mem == 0) 
        // Queue or element size is zero.
        c = (hchan.val)(mallocgc(hchanSize, null, true)); 
        // Race detector uses this location for synchronization.
        c.buf = c.raceaddr();
    else if (elem.ptrdata == 0) 
        // Elements do not contain pointers.
        // Allocate hchan and buf in one call.
        c = (hchan.val)(mallocgc(hchanSize + mem, null, true));
        c.buf = add(@unsafe.Pointer(c), hchanSize);
    else 
        // Elements contain pointers.
        c = @new<hchan>();
        c.buf = mallocgc(mem, elem, true);
        c.elemsize = uint16(elem.size);
    c.elemtype = elem;
    c.dataqsiz = uint(size);
    lockInit(_addr_c.@lock, lockRankHchan);

    if (debugChan) {
        print("makechan: chan=", c, "; elemsize=", elem.size, "; dataqsiz=", size, "\n");
    }
    return _addr_c!;

});

// chanbuf(c, i) is pointer to the i'th slot in the buffer.
private static unsafe.Pointer chanbuf(ptr<hchan> _addr_c, nuint i) {
    ref hchan c = ref _addr_c.val;

    return add(c.buf, uintptr(i) * uintptr(c.elemsize));
}

// full reports whether a send on c would block (that is, the channel is full).
// It uses a single word-sized read of mutable state, so although
// the answer is instantaneously true, the correct answer may have changed
// by the time the calling function receives the return value.
private static bool full(ptr<hchan> _addr_c) {
    ref hchan c = ref _addr_c.val;
 
    // c.dataqsiz is immutable (never written after the channel is created)
    // so it is safe to read at any time during channel operation.
    if (c.dataqsiz == 0) { 
        // Assumes that a pointer read is relaxed-atomic.
        return c.recvq.first == null;

    }
    return c.qcount == c.dataqsiz;

}

// entry point for c <- x from compiled code
//go:nosplit
private static void chansend1(ptr<hchan> _addr_c, unsafe.Pointer elem) {
    ref hchan c = ref _addr_c.val;

    chansend(_addr_c, elem, true, getcallerpc());
}

/*
 * generic single channel send/recv
 * If block is not nil,
 * then the protocol will not
 * sleep but return if it could
 * not complete.
 *
 * sleep can wake up with g.param == nil
 * when a channel involved in the sleep has
 * been closed.  it is easiest to loop and re-run
 * the operation; we'll see that it's now closed.
 */
private static bool chansend(ptr<hchan> _addr_c, unsafe.Pointer ep, bool block, System.UIntPtr callerpc) => func((_, panic, _) => {
    ref hchan c = ref _addr_c.val;

    if (c == null) {
        if (!block) {
            return false;
        }
        gopark(null, null, waitReasonChanSendNilChan, traceEvGoStop, 2);
        throw("unreachable");

    }
    if (debugChan) {
        print("chansend: chan=", c, "\n");
    }
    if (raceenabled) {
        racereadpc(c.raceaddr(), callerpc, funcPC(chansend));
    }
    if (!block && c.closed == 0 && full(_addr_c)) {
        return false;
    }
    long t0 = default;
    if (blockprofilerate > 0) {
        t0 = cputicks();
    }
    lock(_addr_c.@lock);

    if (c.closed != 0) {
        unlock(_addr_c.@lock);
        panic(plainError("send on closed channel"));
    }
    {
        var sg = c.recvq.dequeue();

        if (sg != null) { 
            // Found a waiting receiver. We pass the value we want to send
            // directly to the receiver, bypassing the channel buffer (if any).
            send(_addr_c, _addr_sg, ep, () => {
                unlock(_addr_c.@lock);
            }, 3);
            return true;

        }
    }


    if (c.qcount < c.dataqsiz) { 
        // Space is available in the channel buffer. Enqueue the element to send.
        var qp = chanbuf(_addr_c, c.sendx);
        if (raceenabled) {
            racenotify(_addr_c, c.sendx, _addr_null);
        }
        typedmemmove(c.elemtype, qp, ep);
        c.sendx++;
        if (c.sendx == c.dataqsiz) {
            c.sendx = 0;
        }
        c.qcount++;
        unlock(_addr_c.@lock);
        return true;

    }
    if (!block) {
        unlock(_addr_c.@lock);
        return false;
    }
    var gp = getg();
    var mysg = acquireSudog();
    mysg.releasetime = 0;
    if (t0 != 0) {
        mysg.releasetime = -1;
    }
    mysg.elem = ep;
    mysg.waitlink = null;
    mysg.g = gp;
    mysg.isSelect = false;
    mysg.c = c;
    gp.waiting = mysg;
    gp.param = null;
    c.sendq.enqueue(mysg); 
    // Signal to anyone trying to shrink our stack that we're about
    // to park on a channel. The window between when this G's status
    // changes and when we set gp.activeStackChans is not safe for
    // stack shrinking.
    atomic.Store8(_addr_gp.parkingOnChan, 1);
    gopark(chanparkcommit, @unsafe.Pointer(_addr_c.@lock), waitReasonChanSend, traceEvGoBlockSend, 2); 
    // Ensure the value being sent is kept alive until the
    // receiver copies it out. The sudog has a pointer to the
    // stack object, but sudogs aren't considered as roots of the
    // stack tracer.
    KeepAlive(ep); 

    // someone woke us up.
    if (mysg != gp.waiting) {
        throw("G waiting list is corrupted");
    }
    gp.waiting = null;
    gp.activeStackChans = false;
    var closed = !mysg.success;
    gp.param = null;
    if (mysg.releasetime > 0) {
        blockevent(mysg.releasetime - t0, 2);
    }
    mysg.c = null;
    releaseSudog(mysg);
    if (closed) {
        if (c.closed == 0) {
            throw("chansend: spurious wakeup");
        }
        panic(plainError("send on closed channel"));

    }
    return true;

});

// send processes a send operation on an empty channel c.
// The value ep sent by the sender is copied to the receiver sg.
// The receiver is then woken up to go on its merry way.
// Channel c must be empty and locked.  send unlocks c with unlockf.
// sg must already be dequeued from c.
// ep must be non-nil and point to the heap or the caller's stack.
private static void send(ptr<hchan> _addr_c, ptr<sudog> _addr_sg, unsafe.Pointer ep, Action unlockf, nint skip) {
    ref hchan c = ref _addr_c.val;
    ref sudog sg = ref _addr_sg.val;

    if (raceenabled) {
        if (c.dataqsiz == 0) {
            racesync(_addr_c, _addr_sg);
        }
        else
 { 
            // Pretend we go through the buffer, even though
            // we copy directly. Note that we need to increment
            // the head/tail locations only when raceenabled.
            racenotify(_addr_c, c.recvx, _addr_null);
            racenotify(_addr_c, c.recvx, _addr_sg);
            c.recvx++;
            if (c.recvx == c.dataqsiz) {
                c.recvx = 0;
            }

            c.sendx = c.recvx; // c.sendx = (c.sendx+1) % c.dataqsiz
        }
    }
    if (sg.elem != null) {
        sendDirect(_addr_c.elemtype, _addr_sg, ep);
        sg.elem = null;
    }
    var gp = sg.g;
    unlockf();
    gp.param = @unsafe.Pointer(sg);
    sg.success = true;
    if (sg.releasetime != 0) {
        sg.releasetime = cputicks();
    }
    goready(gp, skip + 1);

}

// Sends and receives on unbuffered or empty-buffered channels are the
// only operations where one running goroutine writes to the stack of
// another running goroutine. The GC assumes that stack writes only
// happen when the goroutine is running and are only done by that
// goroutine. Using a write barrier is sufficient to make up for
// violating that assumption, but the write barrier has to work.
// typedmemmove will call bulkBarrierPreWrite, but the target bytes
// are not in the heap, so that will not help. We arrange to call
// memmove and typeBitsBulkBarrier instead.

private static void sendDirect(ptr<_type> _addr_t, ptr<sudog> _addr_sg, unsafe.Pointer src) {
    ref _type t = ref _addr_t.val;
    ref sudog sg = ref _addr_sg.val;
 
    // src is on our stack, dst is a slot on another stack.

    // Once we read sg.elem out of sg, it will no longer
    // be updated if the destination's stack gets copied (shrunk).
    // So make sure that no preemption points can happen between read & use.
    var dst = sg.elem;
    typeBitsBulkBarrier(t, uintptr(dst), uintptr(src), t.size); 
    // No need for cgo write barrier checks because dst is always
    // Go memory.
    memmove(dst, src, t.size);

}

private static void recvDirect(ptr<_type> _addr_t, ptr<sudog> _addr_sg, unsafe.Pointer dst) {
    ref _type t = ref _addr_t.val;
    ref sudog sg = ref _addr_sg.val;
 
    // dst is on our stack or the heap, src is on another stack.
    // The channel is locked, so src will not move during this
    // operation.
    var src = sg.elem;
    typeBitsBulkBarrier(t, uintptr(dst), uintptr(src), t.size);
    memmove(dst, src, t.size);

}

private static void closechan(ptr<hchan> _addr_c) => func((_, panic, _) => {
    ref hchan c = ref _addr_c.val;

    if (c == null) {
        panic(plainError("close of nil channel"));
    }
    lock(_addr_c.@lock);
    if (c.closed != 0) {
        unlock(_addr_c.@lock);
        panic(plainError("close of closed channel"));
    }
    if (raceenabled) {
        var callerpc = getcallerpc();
        racewritepc(c.raceaddr(), callerpc, funcPC(closechan));
        racerelease(c.raceaddr());
    }
    c.closed = 1;

    gList glist = default; 

    // release all readers
    while (true) {
        var sg = c.recvq.dequeue();
        if (sg == null) {
            break;
        }
        if (sg.elem != null) {
            typedmemclr(c.elemtype, sg.elem);
            sg.elem = null;
        }
        if (sg.releasetime != 0) {
            sg.releasetime = cputicks();
        }
        var gp = sg.g;
        gp.param = @unsafe.Pointer(sg);
        sg.success = false;
        if (raceenabled) {
            raceacquireg(gp, c.raceaddr());
        }
        glist.push(gp);

    } 

    // release all writers (they will panic)
    while (true) {
        sg = c.sendq.dequeue();
        if (sg == null) {
            break;
        }
        sg.elem = null;
        if (sg.releasetime != 0) {
            sg.releasetime = cputicks();
        }
        gp = sg.g;
        gp.param = @unsafe.Pointer(sg);
        sg.success = false;
        if (raceenabled) {
            raceacquireg(gp, c.raceaddr());
        }
        glist.push(gp);

    }
    unlock(_addr_c.@lock); 

    // Ready all Gs now that we've dropped the channel lock.
    while (!glist.empty()) {
        gp = glist.pop();
        gp.schedlink = 0;
        goready(gp, 3);
    }

});

// empty reports whether a read from c would block (that is, the channel is
// empty).  It uses a single atomic read of mutable state.
private static bool empty(ptr<hchan> _addr_c) {
    ref hchan c = ref _addr_c.val;
 
    // c.dataqsiz is immutable.
    if (c.dataqsiz == 0) {
        return atomic.Loadp(@unsafe.Pointer(_addr_c.sendq.first)) == null;
    }
    return atomic.Loaduint(_addr_c.qcount) == 0;

}

// entry points for <- c from compiled code
//go:nosplit
private static void chanrecv1(ptr<hchan> _addr_c, unsafe.Pointer elem) {
    ref hchan c = ref _addr_c.val;

    chanrecv(_addr_c, elem, true);
}

//go:nosplit
private static bool chanrecv2(ptr<hchan> _addr_c, unsafe.Pointer elem) {
    bool received = default;
    ref hchan c = ref _addr_c.val;

    _, received = chanrecv(_addr_c, elem, true);
    return ;
}

// chanrecv receives on channel c and writes the received data to ep.
// ep may be nil, in which case received data is ignored.
// If block == false and no elements are available, returns (false, false).
// Otherwise, if c is closed, zeros *ep and returns (true, false).
// Otherwise, fills in *ep with an element and returns (true, true).
// A non-nil ep must point to the heap or the caller's stack.
private static (bool, bool) chanrecv(ptr<hchan> _addr_c, unsafe.Pointer ep, bool block) {
    bool selected = default;
    bool received = default;
    ref hchan c = ref _addr_c.val;
 
    // raceenabled: don't need to check ep, as it is always on the stack
    // or is new memory allocated by reflect.

    if (debugChan) {
        print("chanrecv: chan=", c, "\n");
    }
    if (c == null) {
        if (!block) {
            return ;
        }
        gopark(null, null, waitReasonChanReceiveNilChan, traceEvGoStop, 2);
        throw("unreachable");

    }
    if (!block && empty(_addr_c)) { 
        // After observing that the channel is not ready for receiving, we observe whether the
        // channel is closed.
        //
        // Reordering of these checks could lead to incorrect behavior when racing with a close.
        // For example, if the channel was open and not empty, was closed, and then drained,
        // reordered reads could incorrectly indicate "open and empty". To prevent reordering,
        // we use atomic loads for both checks, and rely on emptying and closing to happen in
        // separate critical sections under the same lock.  This assumption fails when closing
        // an unbuffered channel with a blocked send, but that is an error condition anyway.
        if (atomic.Load(_addr_c.closed) == 0) { 
            // Because a channel cannot be reopened, the later observation of the channel
            // being not closed implies that it was also not closed at the moment of the
            // first observation. We behave as if we observed the channel at that moment
            // and report that the receive cannot proceed.
            return ;

        }
        if (empty(_addr_c)) { 
            // The channel is irreversibly closed and empty.
            if (raceenabled) {
                raceacquire(c.raceaddr());
            }

            if (ep != null) {
                typedmemclr(c.elemtype, ep);
            }

            return (true, false);

        }
    }
    long t0 = default;
    if (blockprofilerate > 0) {
        t0 = cputicks();
    }
    lock(_addr_c.@lock);

    if (c.closed != 0 && c.qcount == 0) {
        if (raceenabled) {
            raceacquire(c.raceaddr());
        }
        unlock(_addr_c.@lock);
        if (ep != null) {
            typedmemclr(c.elemtype, ep);
        }
        return (true, false);

    }
    {
        var sg = c.sendq.dequeue();

        if (sg != null) { 
            // Found a waiting sender. If buffer is size 0, receive value
            // directly from sender. Otherwise, receive from head of queue
            // and add sender's value to the tail of the queue (both map to
            // the same buffer slot because the queue is full).
            recv(_addr_c, _addr_sg, ep, () => {
                unlock(_addr_c.@lock);
            }, 3);
            return (true, true);

        }
    }


    if (c.qcount > 0) { 
        // Receive directly from queue
        var qp = chanbuf(_addr_c, c.recvx);
        if (raceenabled) {
            racenotify(_addr_c, c.recvx, _addr_null);
        }
        if (ep != null) {
            typedmemmove(c.elemtype, ep, qp);
        }
        typedmemclr(c.elemtype, qp);
        c.recvx++;
        if (c.recvx == c.dataqsiz) {
            c.recvx = 0;
        }
        c.qcount--;
        unlock(_addr_c.@lock);
        return (true, true);

    }
    if (!block) {
        unlock(_addr_c.@lock);
        return (false, false);
    }
    var gp = getg();
    var mysg = acquireSudog();
    mysg.releasetime = 0;
    if (t0 != 0) {
        mysg.releasetime = -1;
    }
    mysg.elem = ep;
    mysg.waitlink = null;
    gp.waiting = mysg;
    mysg.g = gp;
    mysg.isSelect = false;
    mysg.c = c;
    gp.param = null;
    c.recvq.enqueue(mysg); 
    // Signal to anyone trying to shrink our stack that we're about
    // to park on a channel. The window between when this G's status
    // changes and when we set gp.activeStackChans is not safe for
    // stack shrinking.
    atomic.Store8(_addr_gp.parkingOnChan, 1);
    gopark(chanparkcommit, @unsafe.Pointer(_addr_c.@lock), waitReasonChanReceive, traceEvGoBlockRecv, 2); 

    // someone woke us up
    if (mysg != gp.waiting) {
        throw("G waiting list is corrupted");
    }
    gp.waiting = null;
    gp.activeStackChans = false;
    if (mysg.releasetime > 0) {
        blockevent(mysg.releasetime - t0, 2);
    }
    var success = mysg.success;
    gp.param = null;
    mysg.c = null;
    releaseSudog(mysg);
    return (true, success);

}

// recv processes a receive operation on a full channel c.
// There are 2 parts:
// 1) The value sent by the sender sg is put into the channel
//    and the sender is woken up to go on its merry way.
// 2) The value received by the receiver (the current G) is
//    written to ep.
// For synchronous channels, both values are the same.
// For asynchronous channels, the receiver gets its data from
// the channel buffer and the sender's data is put in the
// channel buffer.
// Channel c must be full and locked. recv unlocks c with unlockf.
// sg must already be dequeued from c.
// A non-nil ep must point to the heap or the caller's stack.
private static void recv(ptr<hchan> _addr_c, ptr<sudog> _addr_sg, unsafe.Pointer ep, Action unlockf, nint skip) {
    ref hchan c = ref _addr_c.val;
    ref sudog sg = ref _addr_sg.val;

    if (c.dataqsiz == 0) {
        if (raceenabled) {
            racesync(_addr_c, _addr_sg);
        }
        if (ep != null) { 
            // copy data from sender
            recvDirect(_addr_c.elemtype, _addr_sg, ep);

        }
    }
    else
 { 
        // Queue is full. Take the item at the
        // head of the queue. Make the sender enqueue
        // its item at the tail of the queue. Since the
        // queue is full, those are both the same slot.
        var qp = chanbuf(_addr_c, c.recvx);
        if (raceenabled) {
            racenotify(_addr_c, c.recvx, _addr_null);
            racenotify(_addr_c, c.recvx, _addr_sg);
        }
        if (ep != null) {
            typedmemmove(c.elemtype, ep, qp);
        }
        typedmemmove(c.elemtype, qp, sg.elem);
        c.recvx++;
        if (c.recvx == c.dataqsiz) {
            c.recvx = 0;
        }
        c.sendx = c.recvx; // c.sendx = (c.sendx+1) % c.dataqsiz
    }
    sg.elem = null;
    var gp = sg.g;
    unlockf();
    gp.param = @unsafe.Pointer(sg);
    sg.success = true;
    if (sg.releasetime != 0) {
        sg.releasetime = cputicks();
    }
    goready(gp, skip + 1);

}

private static bool chanparkcommit(ptr<g> _addr_gp, unsafe.Pointer chanLock) {
    ref g gp = ref _addr_gp.val;
 
    // There are unlocked sudogs that point into gp's stack. Stack
    // copying must lock the channels of those sudogs.
    // Set activeStackChans here instead of before we try parking
    // because we could self-deadlock in stack growth on the
    // channel lock.
    gp.activeStackChans = true; 
    // Mark that it's safe for stack shrinking to occur now,
    // because any thread acquiring this G's stack for shrinking
    // is guaranteed to observe activeStackChans after this store.
    atomic.Store8(_addr_gp.parkingOnChan, 0); 
    // Make sure we unlock after setting activeStackChans and
    // unsetting parkingOnChan. The moment we unlock chanLock
    // we risk gp getting readied by a channel operation and
    // so gp could continue running before everything before
    // the unlock is visible (even to gp itself).
    unlock((mutex.val)(chanLock));
    return true;

}

// compiler implements
//
//    select {
//    case c <- v:
//        ... foo
//    default:
//        ... bar
//    }
//
// as
//
//    if selectnbsend(c, v) {
//        ... foo
//    } else {
//        ... bar
//    }
//
private static bool selectnbsend(ptr<hchan> _addr_c, unsafe.Pointer elem) {
    bool selected = default;
    ref hchan c = ref _addr_c.val;

    return chansend(_addr_c, elem, false, getcallerpc());
}

// compiler implements
//
//    select {
//    case v, ok = <-c:
//        ... foo
//    default:
//        ... bar
//    }
//
// as
//
//    if selected, ok = selectnbrecv(&v, c); selected {
//        ... foo
//    } else {
//        ... bar
//    }
//
private static (bool, bool) selectnbrecv(unsafe.Pointer elem, ptr<hchan> _addr_c) {
    bool selected = default;
    bool received = default;
    ref hchan c = ref _addr_c.val;

    return chanrecv(_addr_c, elem, false);
}

//go:linkname reflect_chansend reflect.chansend
private static bool reflect_chansend(ptr<hchan> _addr_c, unsafe.Pointer elem, bool nb) {
    bool selected = default;
    ref hchan c = ref _addr_c.val;

    return chansend(_addr_c, elem, !nb, getcallerpc());
}

//go:linkname reflect_chanrecv reflect.chanrecv
private static (bool, bool) reflect_chanrecv(ptr<hchan> _addr_c, bool nb, unsafe.Pointer elem) {
    bool selected = default;
    bool received = default;
    ref hchan c = ref _addr_c.val;

    return chanrecv(_addr_c, elem, !nb);
}

//go:linkname reflect_chanlen reflect.chanlen
private static nint reflect_chanlen(ptr<hchan> _addr_c) {
    ref hchan c = ref _addr_c.val;

    if (c == null) {
        return 0;
    }
    return int(c.qcount);

}

//go:linkname reflectlite_chanlen internal/reflectlite.chanlen
private static nint reflectlite_chanlen(ptr<hchan> _addr_c) {
    ref hchan c = ref _addr_c.val;

    if (c == null) {
        return 0;
    }
    return int(c.qcount);

}

//go:linkname reflect_chancap reflect.chancap
private static nint reflect_chancap(ptr<hchan> _addr_c) {
    ref hchan c = ref _addr_c.val;

    if (c == null) {
        return 0;
    }
    return int(c.dataqsiz);

}

//go:linkname reflect_chanclose reflect.chanclose
private static void reflect_chanclose(ptr<hchan> _addr_c) {
    ref hchan c = ref _addr_c.val;

    closechan(_addr_c);
}

private static void enqueue(this ptr<waitq> _addr_q, ptr<sudog> _addr_sgp) {
    ref waitq q = ref _addr_q.val;
    ref sudog sgp = ref _addr_sgp.val;

    sgp.next = null;
    var x = q.last;
    if (x == null) {
        sgp.prev = null;
        q.first = sgp;
        q.last = sgp;
        return ;
    }
    sgp.prev = x;
    x.next = sgp;
    q.last = sgp;

}

private static ptr<sudog> dequeue(this ptr<waitq> _addr_q) {
    ref waitq q = ref _addr_q.val;

    while (true) {
        var sgp = q.first;
        if (sgp == null) {
            return _addr_null!;
        }
        var y = sgp.next;
        if (y == null) {
            q.first = null;
            q.last = null;
        }
        else
 {
            y.prev = null;
            q.first = y;
            sgp.next = null; // mark as removed (see dequeueSudog)
        }
        if (sgp.isSelect && !atomic.Cas(_addr_sgp.g.selectDone, 0, 1)) {
            continue;
        }
        return _addr_sgp!;

    }

}

private static unsafe.Pointer raceaddr(this ptr<hchan> _addr_c) {
    ref hchan c = ref _addr_c.val;
 
    // Treat read-like and write-like operations on the channel to
    // happen at this address. Avoid using the address of qcount
    // or dataqsiz, because the len() and cap() builtins read
    // those addresses, and we don't want them racing with
    // operations like close().
    return @unsafe.Pointer(_addr_c.buf);

}

private static void racesync(ptr<hchan> _addr_c, ptr<sudog> _addr_sg) {
    ref hchan c = ref _addr_c.val;
    ref sudog sg = ref _addr_sg.val;

    racerelease(chanbuf(_addr_c, 0));
    raceacquireg(sg.g, chanbuf(_addr_c, 0));
    racereleaseg(sg.g, chanbuf(_addr_c, 0));
    raceacquire(chanbuf(_addr_c, 0));
}

// Notify the race detector of a send or receive involving buffer entry idx
// and a channel c or its communicating partner sg.
// This function handles the special case of c.elemsize==0.
private static void racenotify(ptr<hchan> _addr_c, nuint idx, ptr<sudog> _addr_sg) {
    ref hchan c = ref _addr_c.val;
    ref sudog sg = ref _addr_sg.val;
 
    // We could have passed the unsafe.Pointer corresponding to entry idx
    // instead of idx itself.  However, in a future version of this function,
    // we can use idx to better handle the case of elemsize==0.
    // A future improvement to the detector is to call TSan with c and idx:
    // this way, Go will continue to not allocating buffer entries for channels
    // of elemsize==0, yet the race detector can be made to handle multiple
    // sync objects underneath the hood (one sync object per idx)
    var qp = chanbuf(_addr_c, idx); 
    // When elemsize==0, we don't allocate a full buffer for the channel.
    // Instead of individual buffer entries, the race detector uses the
    // c.buf as the only buffer entry.  This simplification prevents us from
    // following the memory model's happens-before rules (rules that are
    // implemented in racereleaseacquire).  Instead, we accumulate happens-before
    // information in the synchronization object associated with c.buf.
    if (c.elemsize == 0) {
        if (sg == null) {
            raceacquire(qp);
            racerelease(qp);
        }
        else
 {
            raceacquireg(sg.g, qp);
            racereleaseg(sg.g, qp);
        }
    }
    else
 {
        if (sg == null) {
            racereleaseacquire(qp);
        }
        else
 {
            racereleaseacquireg(sg.g, qp);
        }
    }
}

} // end runtime_package
