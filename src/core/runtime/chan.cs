// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

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
using abi = @internal.abi_package;
using atomic = @internal.runtime.atomic_package;
using math = runtime.@internal.math_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.runtime;
using runtime.@internal;

partial class runtime_package {

internal static readonly UntypedInt maxAlign = 8;
internal const uintptr hchanSize = /* unsafe.Sizeof(hchan{}) + uintptr(-int(unsafe.Sizeof(hchan{}))&(maxAlign-1)) */ 104;
internal const bool debugChan = false;

[GoType] partial struct Δhchan {
    internal nuint qcount;          // total data in the queue
    internal nuint dataqsiz;          // size of the circular queue
    internal @unsafe.Pointer buf; // points to an array of dataqsiz elements
    internal uint16 elemsize;
    internal uint32 closed;
    internal ж<timer> timer; // timer feeding this chan
    internal ж<_type> elemtype; // element type
    internal nuint sendx;  // send index
    internal nuint recvx;  // receive index
    internal waitq recvq;  // list of recv waiters
    internal waitq sendq;  // list of send waiters
    // lock protects all fields in hchan, as well as several
    // fields in sudogs blocked on this channel.
    //
    // Do not change another G's status while holding this lock
    // (in particular, do not ready a G), as this can deadlock
    // with stack shrinking.
    internal mutex @lock;
}

[GoType] partial struct waitq {
    internal ж<sudog> first;
    internal ж<sudog> last;
}

//go:linkname reflect_makechan reflect.makechan
internal static ж<Δhchan> reflect_makechan(ж<chantype> Ꮡt, nint size) {
    ref var t = ref Ꮡt.val;

    return makechan(Ꮡt, size);
}

internal static ж<Δhchan> makechan64(ж<chantype> Ꮡt, int64 size) {
    ref var t = ref Ꮡt.val;

    if (((int64)((nint)size)) != size) {
        throw panic(((plainError)"makechan: size out of range"u8));
    }
    return makechan(Ꮡt, ((nint)size));
}

internal static ж<Δhchan> makechan(ж<chantype> Ꮡt, nint size) {
    ref var t = ref Ꮡt.val;

    var elem = t.Elem;
    // compiler checks this but be safe.
    if ((~elem).Size_ >= 1 << (int)(16)) {
        @throw("makechan: invalid channel element type"u8);
    }
    if (hchanSize % maxAlign != 0 || (~elem).Align_ > maxAlign) {
        @throw("makechan: bad alignment"u8);
    }
    var (mem, overflow) = math.MulUintptr((~elem).Size_, ((uintptr)size));
    if (overflow || mem > maxAlloc - hchanSize || size < 0) {
        throw panic(((plainError)"makechan: size out of range"u8));
    }
    // Hchan does not contain pointers interesting for GC when elements stored in buf do not contain pointers.
    // buf points into the same allocation, elemtype is persistent.
    // SudoG's are referenced from their owning thread so they can't be collected.
    // TODO(dvyukov,rlh): Rethink when collector can move allocated objects.
    ж<Δhchan> c = default!;
    switch (ᐧ) {
    case {} when mem is 0: {
        c = (ж<Δhchan>)(uintptr)(mallocgc(hchanSize, nil, true));
        c.val.buf = (uintptr)c.raceaddr();
        break;
    }
    case {} when !elem.Pointers(): {
        c = (ж<Δhchan>)(uintptr)(mallocgc(hchanSize + mem, nil, true));
        c.val.buf = (uintptr)add(new @unsafe.Pointer(c), hchanSize);
        break;
    }
    default: {
        c = @new<Δhchan>();
        c.val.buf = (uintptr)mallocgc(mem, // Elements contain pointers.
 elem, true);
        break;
    }}

    c.val.elemsize = ((uint16)(~elem).Size_);
    c.val.elemtype = elem;
    c.val.dataqsiz = ((nuint)size);
    lockInit(Ꮡ((~c).@lock), lockRankHchan);
    if (debugChan) {
        print("makechan: chan=", c, "; elemsize=", (~elem).Size_, "; dataqsiz=", size, "\n");
    }
    return c;
}

// chanbuf(c, i) is pointer to the i'th slot in the buffer.
//
// chanbuf should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/fjl/memsize
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname chanbuf
internal static @unsafe.Pointer chanbuf(ж<Δhchan> Ꮡc, nuint i) {
    ref var c = ref Ꮡc.val;

    return (uintptr)add(c.buf, ((uintptr)i) * ((uintptr)c.elemsize));
}

// full reports whether a send on c would block (that is, the channel is full).
// It uses a single word-sized read of mutable state, so although
// the answer is instantaneously true, the correct answer may have changed
// by the time the calling function receives the return value.
internal static bool full(ж<Δhchan> Ꮡc) {
    ref var c = ref Ꮡc.val;

    // c.dataqsiz is immutable (never written after the channel is created)
    // so it is safe to read at any time during channel operation.
    if (c.dataqsiz == 0) {
        // Assumes that a pointer read is relaxed-atomic.
        return c.recvq.first == nil;
    }
    // Assumes that a uint read is relaxed-atomic.
    return c.qcount == c.dataqsiz;
}

// entry point for c <- x from compiled code.
//
//go:nosplit
internal static void chansend1(ж<Δhchan> Ꮡc, @unsafe.Pointer elem) {
    ref var c = ref Ꮡc.val;

    chansend(Ꮡc, elem.val, true, getcallerpc());
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
internal static bool chansend(ж<Δhchan> Ꮡc, @unsafe.Pointer ep, bool block, uintptr callerpc) {
    ref var c = ref Ꮡc.val;

    if (c == nil) {
        if (!block) {
            return false;
        }
        gopark(default!, nil, waitReasonChanSendNilChan, traceBlockForever, 2);
        @throw("unreachable"u8);
    }
    if (debugChan) {
        print("chansend: chan=", c, "\n");
    }
    if (raceenabled) {
        racereadpc((uintptr)c.raceaddr(), callerpc, abi.FuncPCABIInternal(chansend));
    }
    // Fast path: check for failed non-blocking operation without acquiring the lock.
    //
    // After observing that the channel is not closed, we observe that the channel is
    // not ready for sending. Each of these observations is a single word-sized read
    // (first c.closed and second full()).
    // Because a closed channel cannot transition from 'ready for sending' to
    // 'not ready for sending', even if the channel is closed between the two observations,
    // they imply a moment between the two when the channel was both not yet closed
    // and not ready for sending. We behave as if we observed the channel at that moment,
    // and report that the send cannot proceed.
    //
    // It is okay if the reads are reordered here: if we observe that the channel is not
    // ready for sending and then observe that it is not closed, that implies that the
    // channel wasn't closed during the first observation. However, nothing here
    // guarantees forward progress. We rely on the side effects of lock release in
    // chanrecv() and closechan() to update this thread's view of c.closed and full().
    if (!block && c.closed == 0 && full(Ꮡc)) {
        return false;
    }
    int64 t0 = default!;
    if (blockprofilerate > 0) {
        t0 = cputicks();
    }
    @lock(Ꮡ(c.@lock));
    if (c.closed != 0) {
        unlock(Ꮡ(c.@lock));
        throw panic(((plainError)"send on closed channel"u8));
    }
    {
        var sg = c.recvq.dequeue(); if (sg != nil) {
            // Found a waiting receiver. We pass the value we want to send
            // directly to the receiver, bypassing the channel buffer (if any).
            send(Ꮡc, sg, ep.val, () => {
                unlock(Ꮡ(c.@lock));
            }, 3);
            return true;
        }
    }
    if (c.qcount < c.dataqsiz) {
        // Space is available in the channel buffer. Enqueue the element to send.
        @unsafe.Pointer qp = (uintptr)chanbuf(Ꮡc, c.sendx);
        if (raceenabled) {
            racenotify(Ꮡc, c.sendx, nil);
        }
        typedmemmove(c.elemtype, qp, ep.val);
        c.sendx++;
        if (c.sendx == c.dataqsiz) {
            c.sendx = 0;
        }
        c.qcount++;
        unlock(Ꮡ(c.@lock));
        return true;
    }
    if (!block) {
        unlock(Ꮡ(c.@lock));
        return false;
    }
    // Block on the channel. Some receiver will complete our operation for us.
    var gp = getg();
    var mysg = acquireSudog();
    mysg.val.releasetime = 0;
    if (t0 != 0) {
        mysg.val.releasetime = -1;
    }
    // No stack splits between assigning elem and enqueuing mysg
    // on gp.waiting where copystack can find it.
    mysg.val.elem = ep;
    mysg.val.waitlink = default!;
    mysg.val.g = gp;
    mysg.val.isSelect = false;
    mysg.val.c = c;
    gp.val.waiting = mysg;
    gp.val.param = default!;
    c.sendq.enqueue(mysg);
    // Signal to anyone trying to shrink our stack that we're about
    // to park on a channel. The window between when this G's status
    // changes and when we set gp.activeStackChans is not safe for
    // stack shrinking.
    (~gp).parkingOnChan.Store(true);
    gopark(chanparkcommit, new @unsafe.Pointer(Ꮡ(c.@lock)), waitReasonChanSend, traceBlockChanSend, 2);
    // Ensure the value being sent is kept alive until the
    // receiver copies it out. The sudog has a pointer to the
    // stack object, but sudogs aren't considered as roots of the
    // stack tracer.
    KeepAlive(ep);
    // someone woke us up.
    if (mysg != (~gp).waiting) {
        @throw("G waiting list is corrupted"u8);
    }
    gp.val.waiting = default!;
    gp.val.activeStackChans = false;
    var closed = !(~mysg).success;
    gp.val.param = default!;
    if ((~mysg).releasetime > 0) {
        blockevent((~mysg).releasetime - t0, 2);
    }
    mysg.val.c = default!;
    releaseSudog(mysg);
    if (closed) {
        if (c.closed == 0) {
            @throw("chansend: spurious wakeup"u8);
        }
        throw panic(((plainError)"send on closed channel"u8));
    }
    return true;
}

// send processes a send operation on an empty channel c.
// The value ep sent by the sender is copied to the receiver sg.
// The receiver is then woken up to go on its merry way.
// Channel c must be empty and locked.  send unlocks c with unlockf.
// sg must already be dequeued from c.
// ep must be non-nil and point to the heap or the caller's stack.
internal static void send(ж<Δhchan> Ꮡc, ж<sudog> Ꮡsg, @unsafe.Pointer ep, Action unlockf, nint skip) {
    ref var c = ref Ꮡc.val;
    ref var sg = ref Ꮡsg.val;

    if (raceenabled) {
        if (c.dataqsiz == 0){
            racesync(Ꮡc, Ꮡsg);
        } else {
            // Pretend we go through the buffer, even though
            // we copy directly. Note that we need to increment
            // the head/tail locations only when raceenabled.
            racenotify(Ꮡc, c.recvx, nil);
            racenotify(Ꮡc, c.recvx, Ꮡsg);
            c.recvx++;
            if (c.recvx == c.dataqsiz) {
                c.recvx = 0;
            }
            c.sendx = c.recvx;
        }
    }
    // c.sendx = (c.sendx+1) % c.dataqsiz
    if (sg.elem != nil) {
        sendDirect(c.elemtype, Ꮡsg, ep.val);
        sg.elem = default!;
    }
    var gp = sg.g;
    unlockf();
    gp.val.param = new @unsafe.Pointer(Ꮡsg);
    sg.success = true;
    if (sg.releasetime != 0) {
        sg.releasetime = cputicks();
    }
    goready(gp, skip + 1);
}

// timerchandrain removes all elements in channel c's buffer.
// It reports whether any elements were removed.
// Because it is only intended for timers, it does not
// handle waiting senders at all (all timer channels
// use non-blocking sends to fill the buffer).
internal static bool timerchandrain(ж<Δhchan> Ꮡc) {
    ref var c = ref Ꮡc.val;

    // Note: Cannot use empty(c) because we are called
    // while holding c.timer.sendLock, and empty(c) will
    // call c.timer.maybeRunChan, which will deadlock.
    // We are emptying the channel, so we only care about
    // the count, not about potentially filling it up.
    if (atomic.Loaduint(Ꮡ(c.qcount)) == 0) {
        return false;
    }
    @lock(Ꮡ(c.@lock));
    var any = false;
    while (c.qcount > 0) {
        any = true;
        typedmemclr(c.elemtype, (uintptr)chanbuf(Ꮡc, c.recvx));
        c.recvx++;
        if (c.recvx == c.dataqsiz) {
            c.recvx = 0;
        }
        c.qcount--;
    }
    unlock(Ꮡ(c.@lock));
    return any;
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
internal static void sendDirect(ж<_type> Ꮡt, ж<sudog> Ꮡsg, @unsafe.Pointer src) {
    ref var t = ref Ꮡt.val;
    ref var sg = ref Ꮡsg.val;

    // src is on our stack, dst is a slot on another stack.
    // Once we read sg.elem out of sg, it will no longer
    // be updated if the destination's stack gets copied (shrunk).
    // So make sure that no preemption points can happen between read & use.
    @unsafe.Pointer dst = sg.elem;
    typeBitsBulkBarrier(Ꮡt, ((uintptr)dst), ((uintptr)src), t.Size_);
    // No need for cgo write barrier checks because dst is always
    // Go memory.
    memmove(dst, src.val, t.Size_);
}

internal static void recvDirect(ж<_type> Ꮡt, ж<sudog> Ꮡsg, @unsafe.Pointer dst) {
    ref var t = ref Ꮡt.val;
    ref var sg = ref Ꮡsg.val;

    // dst is on our stack or the heap, src is on another stack.
    // The channel is locked, so src will not move during this
    // operation.
    @unsafe.Pointer src = sg.elem;
    typeBitsBulkBarrier(Ꮡt, ((uintptr)dst), ((uintptr)src), t.Size_);
    memmove(dst.val, src, t.Size_);
}

internal static void closechan(ж<Δhchan> Ꮡc) {
    ref var c = ref Ꮡc.val;

    if (c == nil) {
        throw panic(((plainError)"close of nil channel"u8));
    }
    @lock(Ꮡ(c.@lock));
    if (c.closed != 0) {
        unlock(Ꮡ(c.@lock));
        throw panic(((plainError)"close of closed channel"u8));
    }
    if (raceenabled) {
        var callerpc = getcallerpc();
        racewritepc((uintptr)c.raceaddr(), callerpc, abi.FuncPCABIInternal(closechan));
        racerelease((uintptr)c.raceaddr());
    }
    c.closed = 1;
    gList glist = default!;
    // release all readers
    while (ᐧ) {
        var sg = c.recvq.dequeue();
        if (sg == nil) {
            break;
        }
        if ((~sg).elem != nil) {
            typedmemclr(c.elemtype, (~sg).elem);
            sg.val.elem = default!;
        }
        if ((~sg).releasetime != 0) {
            sg.val.releasetime = cputicks();
        }
        var gp = sg.val.g;
        gp.val.param = new @unsafe.Pointer(sg);
        sg.val.success = false;
        if (raceenabled) {
            raceacquireg(gp, (uintptr)c.raceaddr());
        }
        glist.push(gp);
    }
    // release all writers (they will panic)
    while (ᐧ) {
        var sg = c.sendq.dequeue();
        if (sg == nil) {
            break;
        }
        sg.val.elem = default!;
        if ((~sg).releasetime != 0) {
            sg.val.releasetime = cputicks();
        }
        var gp = sg.val.g;
        gp.val.param = new @unsafe.Pointer(sg);
        sg.val.success = false;
        if (raceenabled) {
            raceacquireg(gp, (uintptr)c.raceaddr());
        }
        glist.push(gp);
    }
    unlock(Ꮡ(c.@lock));
    // Ready all Gs now that we've dropped the channel lock.
    while (!glist.empty()) {
        var gp = glist.pop();
        gp.val.schedlink = 0;
        goready(gp, 3);
    }
}

// empty reports whether a read from c would block (that is, the channel is
// empty).  It is atomically correct and sequentially consistent at the moment
// it returns, but since the channel is unlocked, the channel may become
// non-empty immediately afterward.
internal static bool empty(ж<Δhchan> Ꮡc) {
    ref var c = ref Ꮡc.val;

    // c.dataqsiz is immutable.
    if (c.dataqsiz == 0) {
        return (uintptr)atomic.Loadp(((@unsafe.Pointer)(Ꮡc.sendq.of(waitq.Ꮡfirst)))) == nil;
    }
    // c.timer is also immutable (it is set after make(chan) but before any channel operations).
    // All timer channels have dataqsiz > 0.
    if (c.timer != nil) {
        c.timer.maybeRunChan();
    }
    return atomic.Loaduint(Ꮡ(c.qcount)) == 0;
}

// entry points for <- c from compiled code.
//
//go:nosplit
internal static void chanrecv1(ж<Δhchan> Ꮡc, @unsafe.Pointer elem) {
    ref var c = ref Ꮡc.val;

    chanrecv(Ꮡc, elem.val, true);
}

//go:nosplit
internal static bool /*received*/ chanrecv2(ж<Δhchan> Ꮡc, @unsafe.Pointer elem) {
    bool received = default!;

    ref var c = ref Ꮡc.val;
    (_, received) = chanrecv(Ꮡc, elem.val, true);
    return received;
}

// chanrecv receives on channel c and writes the received data to ep.
// ep may be nil, in which case received data is ignored.
// If block == false and no elements are available, returns (false, false).
// Otherwise, if c is closed, zeros *ep and returns (true, false).
// Otherwise, fills in *ep with an element and returns (true, true).
// A non-nil ep must point to the heap or the caller's stack.
internal static (bool selected, bool received) chanrecv(ж<Δhchan> Ꮡc, @unsafe.Pointer ep, bool block) {
    bool selected = default!;
    bool received = default!;

    ref var c = ref Ꮡc.val;
    // raceenabled: don't need to check ep, as it is always on the stack
    // or is new memory allocated by reflect.
    if (debugChan) {
        print("chanrecv: chan=", c, "\n");
    }
    if (c == nil) {
        if (!block) {
            return (selected, received);
        }
        gopark(default!, nil, waitReasonChanReceiveNilChan, traceBlockForever, 2);
        @throw("unreachable"u8);
    }
    if (c.timer != nil) {
        c.timer.maybeRunChan();
    }
    // Fast path: check for failed non-blocking operation without acquiring the lock.
    if (!block && empty(Ꮡc)) {
        // After observing that the channel is not ready for receiving, we observe whether the
        // channel is closed.
        //
        // Reordering of these checks could lead to incorrect behavior when racing with a close.
        // For example, if the channel was open and not empty, was closed, and then drained,
        // reordered reads could incorrectly indicate "open and empty". To prevent reordering,
        // we use atomic loads for both checks, and rely on emptying and closing to happen in
        // separate critical sections under the same lock.  This assumption fails when closing
        // an unbuffered channel with a blocked send, but that is an error condition anyway.
        if (atomic.Load(Ꮡ(c.closed)) == 0) {
            // Because a channel cannot be reopened, the later observation of the channel
            // being not closed implies that it was also not closed at the moment of the
            // first observation. We behave as if we observed the channel at that moment
            // and report that the receive cannot proceed.
            return (selected, received);
        }
        // The channel is irreversibly closed. Re-check whether the channel has any pending data
        // to receive, which could have arrived between the empty and closed checks above.
        // Sequential consistency is also required here, when racing with such a send.
        if (empty(Ꮡc)) {
            // The channel is irreversibly closed and empty.
            if (raceenabled) {
                raceacquire((uintptr)c.raceaddr());
            }
            if (ep != nil) {
                typedmemclr(c.elemtype, ep.val);
            }
            return (true, false);
        }
    }
    int64 t0 = default!;
    if (blockprofilerate > 0) {
        t0 = cputicks();
    }
    @lock(Ꮡ(c.@lock));
    if (c.closed != 0){
        if (c.qcount == 0) {
            if (raceenabled) {
                raceacquire((uintptr)c.raceaddr());
            }
            unlock(Ꮡ(c.@lock));
            if (ep != nil) {
                typedmemclr(c.elemtype, ep.val);
            }
            return (true, false);
        }
    } else {
        // The channel has been closed, but the channel's buffer have data.
        // Just found waiting sender with not closed.
        {
            var sg = c.sendq.dequeue(); if (sg != nil) {
                // Found a waiting sender. If buffer is size 0, receive value
                // directly from sender. Otherwise, receive from head of queue
                // and add sender's value to the tail of the queue (both map to
                // the same buffer slot because the queue is full).
                recv(Ꮡc, sg, ep.val, () => {
                    unlock(Ꮡ(c.@lock));
                }, 3);
                return (true, true);
            }
        }
    }
    if (c.qcount > 0) {
        // Receive directly from queue
        @unsafe.Pointer qp = (uintptr)chanbuf(Ꮡc, c.recvx);
        if (raceenabled) {
            racenotify(Ꮡc, c.recvx, nil);
        }
        if (ep != nil) {
            typedmemmove(c.elemtype, ep.val, qp);
        }
        typedmemclr(c.elemtype, qp);
        c.recvx++;
        if (c.recvx == c.dataqsiz) {
            c.recvx = 0;
        }
        c.qcount--;
        unlock(Ꮡ(c.@lock));
        return (true, true);
    }
    if (!block) {
        unlock(Ꮡ(c.@lock));
        return (false, false);
    }
    // no sender available: block on this channel.
    var gp = getg();
    var mysg = acquireSudog();
    mysg.val.releasetime = 0;
    if (t0 != 0) {
        mysg.val.releasetime = -1;
    }
    // No stack splits between assigning elem and enqueuing mysg
    // on gp.waiting where copystack can find it.
    mysg.val.elem = ep;
    mysg.val.waitlink = default!;
    gp.val.waiting = mysg;
    mysg.val.g = gp;
    mysg.val.isSelect = false;
    mysg.val.c = c;
    gp.val.param = default!;
    c.recvq.enqueue(mysg);
    if (c.timer != nil) {
        blockTimerChan(Ꮡc);
    }
    // Signal to anyone trying to shrink our stack that we're about
    // to park on a channel. The window between when this G's status
    // changes and when we set gp.activeStackChans is not safe for
    // stack shrinking.
    (~gp).parkingOnChan.Store(true);
    gopark(chanparkcommit, new @unsafe.Pointer(Ꮡ(c.@lock)), waitReasonChanReceive, traceBlockChanRecv, 2);
    // someone woke us up
    if (mysg != (~gp).waiting) {
        @throw("G waiting list is corrupted"u8);
    }
    if (c.timer != nil) {
        unblockTimerChan(Ꮡc);
    }
    gp.val.waiting = default!;
    gp.val.activeStackChans = false;
    if ((~mysg).releasetime > 0) {
        blockevent((~mysg).releasetime - t0, 2);
    }
    var success = mysg.val.success;
    gp.val.param = default!;
    mysg.val.c = default!;
    releaseSudog(mysg);
    return (true, success);
}

// recv processes a receive operation on a full channel c.
// There are 2 parts:
//  1. The value sent by the sender sg is put into the channel
//     and the sender is woken up to go on its merry way.
//  2. The value received by the receiver (the current G) is
//     written to ep.
//
// For synchronous channels, both values are the same.
// For asynchronous channels, the receiver gets its data from
// the channel buffer and the sender's data is put in the
// channel buffer.
// Channel c must be full and locked. recv unlocks c with unlockf.
// sg must already be dequeued from c.
// A non-nil ep must point to the heap or the caller's stack.
internal static void recv(ж<Δhchan> Ꮡc, ж<sudog> Ꮡsg, @unsafe.Pointer ep, Action unlockf, nint skip) {
    ref var c = ref Ꮡc.val;
    ref var sg = ref Ꮡsg.val;

    if (c.dataqsiz == 0){
        if (raceenabled) {
            racesync(Ꮡc, Ꮡsg);
        }
        if (ep != nil) {
            // copy data from sender
            recvDirect(c.elemtype, Ꮡsg, ep.val);
        }
    } else {
        // Queue is full. Take the item at the
        // head of the queue. Make the sender enqueue
        // its item at the tail of the queue. Since the
        // queue is full, those are both the same slot.
        @unsafe.Pointer qp = (uintptr)chanbuf(Ꮡc, c.recvx);
        if (raceenabled) {
            racenotify(Ꮡc, c.recvx, nil);
            racenotify(Ꮡc, c.recvx, Ꮡsg);
        }
        // copy data from queue to receiver
        if (ep != nil) {
            typedmemmove(c.elemtype, ep.val, qp);
        }
        // copy data from sender to queue
        typedmemmove(c.elemtype, qp, sg.elem);
        c.recvx++;
        if (c.recvx == c.dataqsiz) {
            c.recvx = 0;
        }
        c.sendx = c.recvx;
    }
    // c.sendx = (c.sendx+1) % c.dataqsiz
    sg.elem = default!;
    var gp = sg.g;
    unlockf();
    gp.val.param = new @unsafe.Pointer(Ꮡsg);
    sg.success = true;
    if (sg.releasetime != 0) {
        sg.releasetime = cputicks();
    }
    goready(gp, skip + 1);
}

internal static bool chanparkcommit(ж<g> Ꮡgp, @unsafe.Pointer chanLock) {
    ref var gp = ref Ꮡgp.val;

    // There are unlocked sudogs that point into gp's stack. Stack
    // copying must lock the channels of those sudogs.
    // Set activeStackChans here instead of before we try parking
    // because we could self-deadlock in stack growth on the
    // channel lock.
    gp.activeStackChans = true;
    // Mark that it's safe for stack shrinking to occur now,
    // because any thread acquiring this G's stack for shrinking
    // is guaranteed to observe activeStackChans after this store.
    gp.parkingOnChan.Store(false);
    // Make sure we unlock after setting activeStackChans and
    // unsetting parkingOnChan. The moment we unlock chanLock
    // we risk gp getting readied by a channel operation and
    // so gp could continue running before everything before
    // the unlock is visible (even to gp itself).
    unlock((ж<mutex>)(uintptr)(chanLock));
    return true;
}

// compiler implements
//
//	select {
//	case c <- v:
//		... foo
//	default:
//		... bar
//	}
//
// as
//
//	if selectnbsend(c, v) {
//		... foo
//	} else {
//		... bar
//	}
internal static bool /*selected*/ selectnbsend(ж<Δhchan> Ꮡc, @unsafe.Pointer elem) {
    bool selected = default!;

    ref var c = ref Ꮡc.val;
    return chansend(Ꮡc, elem.val, false, getcallerpc());
}

// compiler implements
//
//	select {
//	case v, ok = <-c:
//		... foo
//	default:
//		... bar
//	}
//
// as
//
//	if selected, ok = selectnbrecv(&v, c); selected {
//		... foo
//	} else {
//		... bar
//	}
internal static (bool selected, bool received) selectnbrecv(@unsafe.Pointer elem, ж<Δhchan> Ꮡc) {
    bool selected = default!;
    bool received = default!;

    ref var c = ref Ꮡc.val;
    return chanrecv(Ꮡc, elem.val, false);
}

//go:linkname reflect_chansend reflect.chansend0
internal static bool /*selected*/ reflect_chansend(ж<Δhchan> Ꮡc, @unsafe.Pointer elem, bool nb) {
    bool selected = default!;

    ref var c = ref Ꮡc.val;
    return chansend(Ꮡc, elem.val, !nb, getcallerpc());
}

//go:linkname reflect_chanrecv reflect.chanrecv
internal static (bool selected, bool received) reflect_chanrecv(ж<Δhchan> Ꮡc, bool nb, @unsafe.Pointer elem) {
    bool selected = default!;
    bool received = default!;

    ref var c = ref Ꮡc.val;
    return chanrecv(Ꮡc, elem.val, !nb);
}

internal static nint chanlen(ж<Δhchan> Ꮡc) {
    ref var c = ref Ꮡc.val;

    if (c == nil) {
        return 0;
    }
    var async = debug.asynctimerchan.Load() != 0;
    if (c.timer != nil && async) {
        c.timer.maybeRunChan();
    }
    if (c.timer != nil && !async) {
        // timer channels have a buffered implementation
        // but present to users as unbuffered, so that we can
        // undo sends without users noticing.
        return 0;
    }
    return ((nint)c.qcount);
}

internal static nint chancap(ж<Δhchan> Ꮡc) {
    ref var c = ref Ꮡc.val;

    if (c == nil) {
        return 0;
    }
    if (c.timer != nil) {
        var async = debug.asynctimerchan.Load() != 0;
        if (async) {
            return ((nint)c.dataqsiz);
        }
        // timer channels have a buffered implementation
        // but present to users as unbuffered, so that we can
        // undo sends without users noticing.
        return 0;
    }
    return ((nint)c.dataqsiz);
}

//go:linkname reflect_chanlen reflect.chanlen
internal static nint reflect_chanlen(ж<Δhchan> Ꮡc) {
    ref var c = ref Ꮡc.val;

    return chanlen(Ꮡc);
}

//go:linkname reflectlite_chanlen internal/reflectlite.chanlen
internal static nint reflectlite_chanlen(ж<Δhchan> Ꮡc) {
    ref var c = ref Ꮡc.val;

    return chanlen(Ꮡc);
}

//go:linkname reflect_chancap reflect.chancap
internal static nint reflect_chancap(ж<Δhchan> Ꮡc) {
    ref var c = ref Ꮡc.val;

    return chancap(Ꮡc);
}

//go:linkname reflect_chanclose reflect.chanclose
internal static void reflect_chanclose(ж<Δhchan> Ꮡc) {
    ref var c = ref Ꮡc.val;

    closechan(Ꮡc);
}

[GoRecv] internal static void enqueue(this ref waitq q, ж<sudog> Ꮡsgp) {
    ref var sgp = ref Ꮡsgp.val;

    sgp.next = default!;
    var x = q.last;
    if (x == nil) {
        sgp.prev = default!;
        q.first = sgp;
        q.last = sgp;
        return;
    }
    sgp.prev = x;
    x.val.next = sgp;
    q.last = sgp;
}

[GoRecv] internal static ж<sudog> dequeue(this ref waitq q) {
    while (ᐧ) {
        var sgp = q.first;
        if (sgp == nil) {
            return default!;
        }
        var y = sgp.val.next;
        if (y == nil){
            q.first = default!;
            q.last = default!;
        } else {
            y.val.prev = default!;
            q.first = y;
            sgp.val.next = default!;
        }
        // mark as removed (see dequeueSudoG)
        // if a goroutine was put on this queue because of a
        // select, there is a small window between the goroutine
        // being woken up by a different case and it grabbing the
        // channel locks. Once it has the lock
        // it removes itself from the queue, so we won't see it after that.
        // We use a flag in the G struct to tell us when someone
        // else has won the race to signal this goroutine but the goroutine
        // hasn't removed itself from the queue yet.
        if ((~sgp).isSelect && !(~(~sgp).g).selectDone.CompareAndSwap(0, 1)) {
            continue;
        }
        return sgp;
    }
}

[GoRecv] internal static @unsafe.Pointer raceaddr(this ref Δhchan c) {
    // Treat read-like and write-like operations on the channel to
    // happen at this address. Avoid using the address of qcount
    // or dataqsiz, because the len() and cap() builtins read
    // those addresses, and we don't want them racing with
    // operations like close().
    return ((@unsafe.Pointer)(Ꮡ(c.buf)));
}

internal static void racesync(ж<Δhchan> Ꮡc, ж<sudog> Ꮡsg) {
    ref var c = ref Ꮡc.val;
    ref var sg = ref Ꮡsg.val;

    racerelease((uintptr)chanbuf(Ꮡc, 0));
    raceacquireg(sg.g, (uintptr)chanbuf(Ꮡc, 0));
    racereleaseg(sg.g, (uintptr)chanbuf(Ꮡc, 0));
    raceacquire((uintptr)chanbuf(Ꮡc, 0));
}

// Notify the race detector of a send or receive involving buffer entry idx
// and a channel c or its communicating partner sg.
// This function handles the special case of c.elemsize==0.
internal static void racenotify(ж<Δhchan> Ꮡc, nuint idx, ж<sudog> Ꮡsg) {
    ref var c = ref Ꮡc.val;
    ref var sg = ref Ꮡsg.val;

    // We could have passed the unsafe.Pointer corresponding to entry idx
    // instead of idx itself.  However, in a future version of this function,
    // we can use idx to better handle the case of elemsize==0.
    // A future improvement to the detector is to call TSan with c and idx:
    // this way, Go will continue to not allocating buffer entries for channels
    // of elemsize==0, yet the race detector can be made to handle multiple
    // sync objects underneath the hood (one sync object per idx)
    @unsafe.Pointer qp = (uintptr)chanbuf(Ꮡc, idx);
    // When elemsize==0, we don't allocate a full buffer for the channel.
    // Instead of individual buffer entries, the race detector uses the
    // c.buf as the only buffer entry.  This simplification prevents us from
    // following the memory model's happens-before rules (rules that are
    // implemented in racereleaseacquire).  Instead, we accumulate happens-before
    // information in the synchronization object associated with c.buf.
    if (c.elemsize == 0){
        if (sg == nil){
            raceacquire(qp);
            racerelease(qp);
        } else {
            raceacquireg(sg.g, qp);
            racereleaseg(sg.g, qp);
        }
    } else {
        if (sg == nil){
            racereleaseacquire(qp);
        } else {
            racereleaseacquireg(sg.g, qp);
        }
    }
}

} // end runtime_package
