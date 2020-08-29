// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 August 29 08:16:38 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\chan.go
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
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class runtime_package
    {
        private static readonly long maxAlign = 8L;
        private static readonly var hchanSize = @unsafe.Sizeof(new hchan()) + uintptr(-int(@unsafe.Sizeof(new hchan())) & (maxAlign - 1L));
        private static readonly var debugChan = false;

        private partial struct hchan
        {
            public ulong qcount; // total data in the queue
            public ulong dataqsiz; // size of the circular queue
            public unsafe.Pointer buf; // points to an array of dataqsiz elements
            public ushort elemsize;
            public uint closed;
            public ptr<_type> elemtype; // element type
            public ulong sendx; // send index
            public ulong recvx; // receive index
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

        private partial struct waitq
        {
            public ptr<sudog> first;
            public ptr<sudog> last;
        }

        //go:linkname reflect_makechan reflect.makechan
        private static ref hchan reflect_makechan(ref chantype t, long size)
        {
            return makechan(t, size);
        }

        private static ref hchan makechan64(ref chantype _t, long size) => func(_t, (ref chantype t, Defer _, Panic panic, Recover __) =>
        {
            if (int64(int(size)) != size)
            {
                panic(plainError("makechan: size out of range"));
            }
            return makechan(t, int(size));
        });

        private static ref hchan makechan(ref chantype _t, long size) => func(_t, (ref chantype t, Defer _, Panic panic, Recover __) =>
        {
            var elem = t.elem; 

            // compiler checks this but be safe.
            if (elem.size >= 1L << (int)(16L))
            {
                throw("makechan: invalid channel element type");
            }
            if (hchanSize % maxAlign != 0L || elem.align > maxAlign)
            {
                throw("makechan: bad alignment");
            }
            if (size < 0L || uintptr(size) > maxSliceCap(elem.size) || uintptr(size) * elem.size > _MaxMem - hchanSize)
            {
                panic(plainError("makechan: size out of range"));
            } 

            // Hchan does not contain pointers interesting for GC when elements stored in buf do not contain pointers.
            // buf points into the same allocation, elemtype is persistent.
            // SudoG's are referenced from their owning thread so they can't be collected.
            // TODO(dvyukov,rlh): Rethink when collector can move allocated objects.
            ref hchan c = default;

            if (size == 0L || elem.size == 0L) 
                // Queue or element size is zero.
                c = (hchan.Value)(mallocgc(hchanSize, null, true)); 
                // Race detector uses this location for synchronization.
                c.buf = @unsafe.Pointer(c);
            else if (elem.kind & kindNoPointers != 0L) 
                // Elements do not contain pointers.
                // Allocate hchan and buf in one call.
                c = (hchan.Value)(mallocgc(hchanSize + uintptr(size) * elem.size, null, true));
                c.buf = add(@unsafe.Pointer(c), hchanSize);
            else 
                // Elements contain pointers.
                c = @new<hchan>();
                c.buf = mallocgc(uintptr(size) * elem.size, elem, true);
                        c.elemsize = uint16(elem.size);
            c.elemtype = elem;
            c.dataqsiz = uint(size);

            if (debugChan)
            {
                print("makechan: chan=", c, "; elemsize=", elem.size, "; elemalg=", elem.alg, "; dataqsiz=", size, "\n");
            }
            return c;
        });

        // chanbuf(c, i) is pointer to the i'th slot in the buffer.
        private static unsafe.Pointer chanbuf(ref hchan c, ulong i)
        {
            return add(c.buf, uintptr(i) * uintptr(c.elemsize));
        }

        // entry point for c <- x from compiled code
        //go:nosplit
        private static void chansend1(ref hchan c, unsafe.Pointer elem)
        {
            chansend(c, elem, true, getcallerpc());
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
        private static bool chansend(ref hchan _c, unsafe.Pointer ep, bool block, System.UIntPtr callerpc) => func(_c, (ref hchan c, Defer _, Panic panic, Recover __) =>
        {
            if (c == null)
            {
                if (!block)
                {
                    return false;
                }
                gopark(null, null, "chan send (nil chan)", traceEvGoStop, 2L);
                throw("unreachable");
            }
            if (debugChan)
            {
                print("chansend: chan=", c, "\n");
            }
            if (raceenabled)
            {
                racereadpc(@unsafe.Pointer(c), callerpc, funcPC(chansend));
            } 

            // Fast path: check for failed non-blocking operation without acquiring the lock.
            //
            // After observing that the channel is not closed, we observe that the channel is
            // not ready for sending. Each of these observations is a single word-sized read
            // (first c.closed and second c.recvq.first or c.qcount depending on kind of channel).
            // Because a closed channel cannot transition from 'ready for sending' to
            // 'not ready for sending', even if the channel is closed between the two observations,
            // they imply a moment between the two when the channel was both not yet closed
            // and not ready for sending. We behave as if we observed the channel at that moment,
            // and report that the send cannot proceed.
            //
            // It is okay if the reads are reordered here: if we observe that the channel is not
            // ready for sending and then observe that it is not closed, that implies that the
            // channel wasn't closed during the first observation.
            if (!block && c.closed == 0L && ((c.dataqsiz == 0L && c.recvq.first == null) || (c.dataqsiz > 0L && c.qcount == c.dataqsiz)))
            {
                return false;
            }
            long t0 = default;
            if (blockprofilerate > 0L)
            {
                t0 = cputicks();
            }
            lock(ref c.@lock);

            if (c.closed != 0L)
            {
                unlock(ref c.@lock);
                panic(plainError("send on closed channel"));
            }
            {
                var sg = c.recvq.dequeue();

                if (sg != null)
                { 
                    // Found a waiting receiver. We pass the value we want to send
                    // directly to the receiver, bypassing the channel buffer (if any).
                    send(c, sg, ep, () =>
                    {
                        unlock(ref c.@lock);

                    }, 3L);
                    return true;
                }

            }

            if (c.qcount < c.dataqsiz)
            { 
                // Space is available in the channel buffer. Enqueue the element to send.
                var qp = chanbuf(c, c.sendx);
                if (raceenabled)
                {
                    raceacquire(qp);
                    racerelease(qp);
                }
                typedmemmove(c.elemtype, qp, ep);
                c.sendx++;
                if (c.sendx == c.dataqsiz)
                {
                    c.sendx = 0L;
                }
                c.qcount++;
                unlock(ref c.@lock);
                return true;
            }
            if (!block)
            {
                unlock(ref c.@lock);
                return false;
            } 

            // Block on the channel. Some receiver will complete our operation for us.
            var gp = getg();
            var mysg = acquireSudog();
            mysg.releasetime = 0L;
            if (t0 != 0L)
            {
                mysg.releasetime = -1L;
            } 
            // No stack splits between assigning elem and enqueuing mysg
            // on gp.waiting where copystack can find it.
            mysg.elem = ep;
            mysg.waitlink = null;
            mysg.g = gp;
            mysg.isSelect = false;
            mysg.c = c;
            gp.waiting = mysg;
            gp.param = null;
            c.sendq.enqueue(mysg);
            goparkunlock(ref c.@lock, "chan send", traceEvGoBlockSend, 3L); 

            // someone woke us up.
            if (mysg != gp.waiting)
            {
                throw("G waiting list is corrupted");
            }
            gp.waiting = null;
            if (gp.param == null)
            {
                if (c.closed == 0L)
                {
                    throw("chansend: spurious wakeup");
                }
                panic(plainError("send on closed channel"));
            }
            gp.param = null;
            if (mysg.releasetime > 0L)
            {
                blockevent(mysg.releasetime - t0, 2L);
            }
            mysg.c = null;
            releaseSudog(mysg);
            return true;
        });

        // send processes a send operation on an empty channel c.
        // The value ep sent by the sender is copied to the receiver sg.
        // The receiver is then woken up to go on its merry way.
        // Channel c must be empty and locked.  send unlocks c with unlockf.
        // sg must already be dequeued from c.
        // ep must be non-nil and point to the heap or the caller's stack.
        private static void send(ref hchan c, ref sudog sg, unsafe.Pointer ep, Action unlockf, long skip)
        {
            if (raceenabled)
            {
                if (c.dataqsiz == 0L)
                {
                    racesync(c, sg);
                }
                else
                { 
                    // Pretend we go through the buffer, even though
                    // we copy directly. Note that we need to increment
                    // the head/tail locations only when raceenabled.
                    var qp = chanbuf(c, c.recvx);
                    raceacquire(qp);
                    racerelease(qp);
                    raceacquireg(sg.g, qp);
                    racereleaseg(sg.g, qp);
                    c.recvx++;
                    if (c.recvx == c.dataqsiz)
                    {
                        c.recvx = 0L;
                    }
                    c.sendx = c.recvx; // c.sendx = (c.sendx+1) % c.dataqsiz
                }
            }
            if (sg.elem != null)
            {
                sendDirect(c.elemtype, sg, ep);
                sg.elem = null;
            }
            var gp = sg.g;
            unlockf();
            gp.param = @unsafe.Pointer(sg);
            if (sg.releasetime != 0L)
            {
                sg.releasetime = cputicks();
            }
            goready(gp, skip + 1L);
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

        private static void sendDirect(ref _type t, ref sudog sg, unsafe.Pointer src)
        { 
            // src is on our stack, dst is a slot on another stack.

            // Once we read sg.elem out of sg, it will no longer
            // be updated if the destination's stack gets copied (shrunk).
            // So make sure that no preemption points can happen between read & use.
            var dst = sg.elem;
            typeBitsBulkBarrier(t, uintptr(dst), uintptr(src), t.size);
            memmove(dst, src, t.size);
        }

        private static void recvDirect(ref _type t, ref sudog sg, unsafe.Pointer dst)
        { 
            // dst is on our stack or the heap, src is on another stack.
            // The channel is locked, so src will not move during this
            // operation.
            var src = sg.elem;
            typeBitsBulkBarrier(t, uintptr(dst), uintptr(src), t.size);
            memmove(dst, src, t.size);
        }

        private static void closechan(ref hchan _c) => func(_c, (ref hchan c, Defer _, Panic panic, Recover __) =>
        {
            if (c == null)
            {
                panic(plainError("close of nil channel"));
            }
            lock(ref c.@lock);
            if (c.closed != 0L)
            {
                unlock(ref c.@lock);
                panic(plainError("close of closed channel"));
            }
            if (raceenabled)
            {
                var callerpc = getcallerpc();
                racewritepc(@unsafe.Pointer(c), callerpc, funcPC(closechan));
                racerelease(@unsafe.Pointer(c));
            }
            c.closed = 1L;

            ref g glist = default; 

            // release all readers
            while (true)
            {
                var sg = c.recvq.dequeue();
                if (sg == null)
                {
                    break;
                }
                if (sg.elem != null)
                {
                    typedmemclr(c.elemtype, sg.elem);
                    sg.elem = null;
                }
                if (sg.releasetime != 0L)
                {
                    sg.releasetime = cputicks();
                }
                var gp = sg.g;
                gp.param = null;
                if (raceenabled)
                {
                    raceacquireg(gp, @unsafe.Pointer(c));
                }
                gp.schedlink.set(glist);
                glist = gp;
            } 

            // release all writers (they will panic)
 

            // release all writers (they will panic)
            while (true)
            {
                sg = c.sendq.dequeue();
                if (sg == null)
                {
                    break;
                }
                sg.elem = null;
                if (sg.releasetime != 0L)
                {
                    sg.releasetime = cputicks();
                }
                gp = sg.g;
                gp.param = null;
                if (raceenabled)
                {
                    raceacquireg(gp, @unsafe.Pointer(c));
                }
                gp.schedlink.set(glist);
                glist = gp;
            }

            unlock(ref c.@lock); 

            // Ready all Gs now that we've dropped the channel lock.
            while (glist != null)
            {
                gp = glist;
                glist = glist.schedlink.ptr();
                gp.schedlink = 0L;
                goready(gp, 3L);
            }

        });

        // entry points for <- c from compiled code
        //go:nosplit
        private static void chanrecv1(ref hchan c, unsafe.Pointer elem)
        {
            chanrecv(c, elem, true);
        }

        //go:nosplit
        private static bool chanrecv2(ref hchan c, unsafe.Pointer elem)
        {
            _, received = chanrecv(c, elem, true);
            return;
        }

        // chanrecv receives on channel c and writes the received data to ep.
        // ep may be nil, in which case received data is ignored.
        // If block == false and no elements are available, returns (false, false).
        // Otherwise, if c is closed, zeros *ep and returns (true, false).
        // Otherwise, fills in *ep with an element and returns (true, true).
        // A non-nil ep must point to the heap or the caller's stack.
        private static (bool, bool) chanrecv(ref hchan c, unsafe.Pointer ep, bool block)
        { 
            // raceenabled: don't need to check ep, as it is always on the stack
            // or is new memory allocated by reflect.

            if (debugChan)
            {
                print("chanrecv: chan=", c, "\n");
            }
            if (c == null)
            {
                if (!block)
                {
                    return;
                }
                gopark(null, null, "chan receive (nil chan)", traceEvGoStop, 2L);
                throw("unreachable");
            } 

            // Fast path: check for failed non-blocking operation without acquiring the lock.
            //
            // After observing that the channel is not ready for receiving, we observe that the
            // channel is not closed. Each of these observations is a single word-sized read
            // (first c.sendq.first or c.qcount, and second c.closed).
            // Because a channel cannot be reopened, the later observation of the channel
            // being not closed implies that it was also not closed at the moment of the
            // first observation. We behave as if we observed the channel at that moment
            // and report that the receive cannot proceed.
            //
            // The order of operations is important here: reversing the operations can lead to
            // incorrect behavior when racing with a close.
            if (!block && (c.dataqsiz == 0L && c.sendq.first == null || c.dataqsiz > 0L && atomic.Loaduint(ref c.qcount) == 0L) && atomic.Load(ref c.closed) == 0L)
            {
                return;
            }
            long t0 = default;
            if (blockprofilerate > 0L)
            {
                t0 = cputicks();
            }
            lock(ref c.@lock);

            if (c.closed != 0L && c.qcount == 0L)
            {
                if (raceenabled)
                {
                    raceacquire(@unsafe.Pointer(c));
                }
                unlock(ref c.@lock);
                if (ep != null)
                {
                    typedmemclr(c.elemtype, ep);
                }
                return (true, false);
            }
            {
                var sg = c.sendq.dequeue();

                if (sg != null)
                { 
                    // Found a waiting sender. If buffer is size 0, receive value
                    // directly from sender. Otherwise, receive from head of queue
                    // and add sender's value to the tail of the queue (both map to
                    // the same buffer slot because the queue is full).
                    recv(c, sg, ep, () =>
                    {
                        unlock(ref c.@lock);

                    }, 3L);
                    return (true, true);
                }

            }

            if (c.qcount > 0L)
            { 
                // Receive directly from queue
                var qp = chanbuf(c, c.recvx);
                if (raceenabled)
                {
                    raceacquire(qp);
                    racerelease(qp);
                }
                if (ep != null)
                {
                    typedmemmove(c.elemtype, ep, qp);
                }
                typedmemclr(c.elemtype, qp);
                c.recvx++;
                if (c.recvx == c.dataqsiz)
                {
                    c.recvx = 0L;
                }
                c.qcount--;
                unlock(ref c.@lock);
                return (true, true);
            }
            if (!block)
            {
                unlock(ref c.@lock);
                return (false, false);
            } 

            // no sender available: block on this channel.
            var gp = getg();
            var mysg = acquireSudog();
            mysg.releasetime = 0L;
            if (t0 != 0L)
            {
                mysg.releasetime = -1L;
            } 
            // No stack splits between assigning elem and enqueuing mysg
            // on gp.waiting where copystack can find it.
            mysg.elem = ep;
            mysg.waitlink = null;
            gp.waiting = mysg;
            mysg.g = gp;
            mysg.isSelect = false;
            mysg.c = c;
            gp.param = null;
            c.recvq.enqueue(mysg);
            goparkunlock(ref c.@lock, "chan receive", traceEvGoBlockRecv, 3L); 

            // someone woke us up
            if (mysg != gp.waiting)
            {
                throw("G waiting list is corrupted");
            }
            gp.waiting = null;
            if (mysg.releasetime > 0L)
            {
                blockevent(mysg.releasetime - t0, 2L);
            }
            var closed = gp.param == null;
            gp.param = null;
            mysg.c = null;
            releaseSudog(mysg);
            return (true, !closed);
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
        private static void recv(ref hchan c, ref sudog sg, unsafe.Pointer ep, Action unlockf, long skip)
        {
            if (c.dataqsiz == 0L)
            {
                if (raceenabled)
                {
                    racesync(c, sg);
                }
                if (ep != null)
                { 
                    // copy data from sender
                    recvDirect(c.elemtype, sg, ep);
                }
            }
            else
            { 
                // Queue is full. Take the item at the
                // head of the queue. Make the sender enqueue
                // its item at the tail of the queue. Since the
                // queue is full, those are both the same slot.
                var qp = chanbuf(c, c.recvx);
                if (raceenabled)
                {
                    raceacquire(qp);
                    racerelease(qp);
                    raceacquireg(sg.g, qp);
                    racereleaseg(sg.g, qp);
                } 
                // copy data from queue to receiver
                if (ep != null)
                {
                    typedmemmove(c.elemtype, ep, qp);
                } 
                // copy data from sender to queue
                typedmemmove(c.elemtype, qp, sg.elem);
                c.recvx++;
                if (c.recvx == c.dataqsiz)
                {
                    c.recvx = 0L;
                }
                c.sendx = c.recvx; // c.sendx = (c.sendx+1) % c.dataqsiz
            }
            sg.elem = null;
            var gp = sg.g;
            unlockf();
            gp.param = @unsafe.Pointer(sg);
            if (sg.releasetime != 0L)
            {
                sg.releasetime = cputicks();
            }
            goready(gp, skip + 1L);
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
        private static bool selectnbsend(ref hchan c, unsafe.Pointer elem)
        {
            return chansend(c, elem, false, getcallerpc());
        }

        // compiler implements
        //
        //    select {
        //    case v = <-c:
        //        ... foo
        //    default:
        //        ... bar
        //    }
        //
        // as
        //
        //    if selectnbrecv(&v, c) {
        //        ... foo
        //    } else {
        //        ... bar
        //    }
        //
        private static bool selectnbrecv(unsafe.Pointer elem, ref hchan c)
        {
            selected, _ = chanrecv(c, elem, false);
            return;
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
        //    if c != nil && selectnbrecv2(&v, &ok, c) {
        //        ... foo
        //    } else {
        //        ... bar
        //    }
        //
        private static bool selectnbrecv2(unsafe.Pointer elem, ref bool received, ref hchan c)
        { 
            // TODO(khr): just return 2 values from this function, now that it is in Go.
            selected, received.Value = chanrecv(c, elem, false);
            return;
        }

        //go:linkname reflect_chansend reflect.chansend
        private static bool reflect_chansend(ref hchan c, unsafe.Pointer elem, bool nb)
        {
            return chansend(c, elem, !nb, getcallerpc());
        }

        //go:linkname reflect_chanrecv reflect.chanrecv
        private static (bool, bool) reflect_chanrecv(ref hchan c, bool nb, unsafe.Pointer elem)
        {
            return chanrecv(c, elem, !nb);
        }

        //go:linkname reflect_chanlen reflect.chanlen
        private static long reflect_chanlen(ref hchan c)
        {
            if (c == null)
            {
                return 0L;
            }
            return int(c.qcount);
        }

        //go:linkname reflect_chancap reflect.chancap
        private static long reflect_chancap(ref hchan c)
        {
            if (c == null)
            {
                return 0L;
            }
            return int(c.dataqsiz);
        }

        //go:linkname reflect_chanclose reflect.chanclose
        private static void reflect_chanclose(ref hchan c)
        {
            closechan(c);
        }

        private static void enqueue(this ref waitq q, ref sudog sgp)
        {
            sgp.next = null;
            var x = q.last;
            if (x == null)
            {
                sgp.prev = null;
                q.first = sgp;
                q.last = sgp;
                return;
            }
            sgp.prev = x;
            x.next = sgp;
            q.last = sgp;
        }

        private static ref sudog dequeue(this ref waitq q)
        {
            while (true)
            {
                var sgp = q.first;
                if (sgp == null)
                {
                    return null;
                }
                var y = sgp.next;
                if (y == null)
                {
                    q.first = null;
                    q.last = null;
                }
                else
                {
                    y.prev = null;
                    q.first = y;
                    sgp.next = null; // mark as removed (see dequeueSudog)
                } 

                // if a goroutine was put on this queue because of a
                // select, there is a small window between the goroutine
                // being woken up by a different case and it grabbing the
                // channel locks. Once it has the lock
                // it removes itself from the queue, so we won't see it after that.
                // We use a flag in the G struct to tell us when someone
                // else has won the race to signal this goroutine but the goroutine
                // hasn't removed itself from the queue yet.
                if (sgp.isSelect)
                {
                    if (!atomic.Cas(ref sgp.g.selectDone, 0L, 1L))
                    {
                        continue;
                    }
                }
                return sgp;
            }

        }

        private static void racesync(ref hchan c, ref sudog sg)
        {
            racerelease(chanbuf(c, 0L));
            raceacquireg(sg.g, chanbuf(c, 0L));
            racereleaseg(sg.g, chanbuf(c, 0L));
            raceacquire(chanbuf(c, 0L));
        }
    }
}
