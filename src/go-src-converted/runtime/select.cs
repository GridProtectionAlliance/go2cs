// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 August 29 08:19:55 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\select.go
// This file contains the implementation of Go select statements.

using sys = go.runtime.@internal.sys_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;

namespace go
{
    public static unsafe partial class runtime_package
    {
        private static readonly var debugSelect = false;



 
        // scase.kind
        private static readonly var caseNil = iota;
        private static readonly var caseRecv = 0;
        private static readonly var caseSend = 1;
        private static readonly var caseDefault = 2;

        // Select statement header.
        // Known to compiler.
        // Changes here must also be made in src/cmd/internal/gc/select.go's selecttype.
        private partial struct hselect
        {
            public ushort tcase; // total count of scase[]
            public ushort ncase; // currently filled scase[]
            public ptr<ushort> pollorder; // case poll order
            public ptr<ushort> lockorder; // channel lock order
            public array<scase> scase; // one per case (in order of appearance)
        }

        // Select case descriptor.
        // Known to compiler.
        // Changes here must also be made in src/cmd/internal/gc/select.go's selecttype.
        private partial struct scase
        {
            public unsafe.Pointer elem; // data element
            public ptr<hchan> c; // chan
            public System.UIntPtr pc; // return pc (for race detector / msan)
            public ushort kind;
            public ptr<bool> receivedp; // pointer to received bool, if any
            public long releasetime;
        }

        private static var chansendpc = funcPC(chansend);        private static var chanrecvpc = funcPC(chanrecv);

        private static System.UIntPtr selectsize(System.UIntPtr size)
        {
            var selsize = @unsafe.Sizeof(new hselect()) + (size - 1L) * @unsafe.Sizeof(new hselect().scase[0L]) + size * @unsafe.Sizeof(new hselect().lockorder.Value) + size * @unsafe.Sizeof(new hselect().pollorder.Value);
            return round(selsize, sys.Int64Align);
        }

        private static void newselect(ref hselect sel, long selsize, int size)
        {
            if (selsize != int64(selectsize(uintptr(size))))
            {
                print("runtime: bad select size ", selsize, ", want ", selectsize(uintptr(size)), "\n");
                throw("bad select size");
            }
            sel.tcase = uint16(size);
            sel.ncase = 0L;
            sel.lockorder = (uint16.Value)(add(@unsafe.Pointer(ref sel.scase), uintptr(size) * @unsafe.Sizeof(new hselect().scase[0L])));
            sel.pollorder = (uint16.Value)(add(@unsafe.Pointer(sel.lockorder), uintptr(size) * @unsafe.Sizeof(new hselect().lockorder.Value)));

            if (debugSelect)
            {
                print("newselect s=", sel, " size=", size, "\n");
            }
        }

        private static void selectsend(ref hselect sel, ref hchan c, unsafe.Pointer elem)
        {
            var pc = getcallerpc();
            var i = sel.ncase;
            if (i >= sel.tcase)
            {
                throw("selectsend: too many cases");
            }
            sel.ncase = i + 1L;
            if (c == null)
            {
                return;
            }
            var cas = (scase.Value)(add(@unsafe.Pointer(ref sel.scase), uintptr(i) * @unsafe.Sizeof(sel.scase[0L])));
            cas.pc = pc;
            cas.c = c;
            cas.kind = caseSend;
            cas.elem = elem;

            if (debugSelect)
            {
                print("selectsend s=", sel, " pc=", hex(cas.pc), " chan=", cas.c, "\n");
            }
        }

        private static void selectrecv(ref hselect sel, ref hchan c, unsafe.Pointer elem, ref bool received)
        {
            var pc = getcallerpc();
            var i = sel.ncase;
            if (i >= sel.tcase)
            {
                throw("selectrecv: too many cases");
            }
            sel.ncase = i + 1L;
            if (c == null)
            {
                return;
            }
            var cas = (scase.Value)(add(@unsafe.Pointer(ref sel.scase), uintptr(i) * @unsafe.Sizeof(sel.scase[0L])));
            cas.pc = pc;
            cas.c = c;
            cas.kind = caseRecv;
            cas.elem = elem;
            cas.receivedp = received;

            if (debugSelect)
            {
                print("selectrecv s=", sel, " pc=", hex(cas.pc), " chan=", cas.c, "\n");
            }
        }

        private static void selectdefault(ref hselect sel)
        {
            var pc = getcallerpc();
            var i = sel.ncase;
            if (i >= sel.tcase)
            {
                throw("selectdefault: too many cases");
            }
            sel.ncase = i + 1L;
            var cas = (scase.Value)(add(@unsafe.Pointer(ref sel.scase), uintptr(i) * @unsafe.Sizeof(sel.scase[0L])));
            cas.pc = pc;
            cas.c = null;
            cas.kind = caseDefault;

            if (debugSelect)
            {
                print("selectdefault s=", sel, " pc=", hex(cas.pc), "\n");
            }
        }

        private static void sellock(slice<scase> scases, slice<ushort> lockorder)
        {
            ref hchan c = default;
            foreach (var (_, o) in lockorder)
            {
                var c0 = scases[o].c;
                if (c0 != null && c0 != c)
                {
                    c = c0;
                    lock(ref c.@lock);
                }
            }
        }

        private static void selunlock(slice<scase> scases, slice<ushort> lockorder)
        { 
            // We must be very careful here to not touch sel after we have unlocked
            // the last lock, because sel can be freed right after the last unlock.
            // Consider the following situation.
            // First M calls runtime·park() in runtime·selectgo() passing the sel.
            // Once runtime·park() has unlocked the last lock, another M makes
            // the G that calls select runnable again and schedules it for execution.
            // When the G runs on another M, it locks all the locks and frees sel.
            // Now if the first M touches sel, it will access freed memory.
            for (var i = len(scases) - 1L; i >= 0L; i--)
            {
                var c = scases[lockorder[i]].c;
                if (c == null)
                {
                    break;
                }
                if (i > 0L && c == scases[lockorder[i - 1L]].c)
                {
                    continue; // will unlock it on the next iteration
                }
                unlock(ref c.@lock);
            }

        }

        private static bool selparkcommit(ref g gp, unsafe.Pointer _)
        { 
            // This must not access gp's stack (see gopark). In
            // particular, it must not access the *hselect. That's okay,
            // because by the time this is called, gp.waiting has all
            // channels in lock order.
            ref hchan lastc = default;
            {
                var sg = gp.waiting;

                while (sg != null)
                {
                    if (sg.c != lastc && lastc != null)
                    { 
                        // As soon as we unlock the channel, fields in
                        // any sudog with that channel may change,
                        // including c and waitlink. Since multiple
                        // sudogs may have the same channel, we unlock
                        // only after we've passed the last instance
                        // of a channel.
                        unlock(ref lastc.@lock);
                    sg = sg.waitlink;
                    }
                    lastc = sg.c;
                }

            }
            if (lastc != null)
            {
                unlock(ref lastc.@lock);
            }
            return true;
        }

        private static void block()
        {
            gopark(null, null, "select (no cases)", traceEvGoStop, 1L); // forever
        }

        // selectgo implements the select statement.
        //
        // *sel is on the current goroutine's stack (regardless of any
        // escaping in selectgo).
        //
        // selectgo returns the index of the chosen scase, which matches the
        // ordinal position of its respective select{recv,send,default} call.
        private static long selectgo(ref hselect _sel) => func(_sel, (ref hselect sel, Defer _, Panic panic, Recover __) =>
        {
            if (debugSelect)
            {
                print("select: sel=", sel, "\n");
            }
            if (sel.ncase != sel.tcase)
            {
                throw("selectgo: case count mismatch");
            }
            slice scaseslice = new slice(unsafe.Pointer(&sel.scase),int(sel.ncase),int(sel.ncase));
            *(*slice<scase>) scases = @unsafe.Pointer(ref scaseslice).Value;

            long t0 = default;
            if (blockprofilerate > 0L)
            {
                t0 = cputicks();
                {
                    long i__prev1 = i;

                    for (long i = 0L; i < int(sel.ncase); i++)
                    {
                        scases[i].releasetime = -1L;
                    }


                    i = i__prev1;
                }
            } 

            // The compiler rewrites selects that statically have
            // only 0 or 1 cases plus default into simpler constructs.
            // The only way we can end up with such small sel.ncase
            // values here is for a larger select in which most channels
            // have been nilled out. The general code handles those
            // cases correctly, and they are rare enough not to bother
            // optimizing (and needing to test).

            // generate permuted order
            slice pollslice = new slice(unsafe.Pointer(sel.pollorder),int(sel.ncase),int(sel.ncase));
            *(*slice<ushort>) pollorder = @unsafe.Pointer(ref pollslice).Value;
            {
                long i__prev1 = i;

                for (i = 1L; i < int(sel.ncase); i++)
                {
                    var j = fastrandn(uint32(i + 1L));
                    pollorder[i] = pollorder[j];
                    pollorder[j] = uint16(i);
                } 

                // sort the cases by Hchan address to get the locking order.
                // simple heap sort, to guarantee n log n time and constant stack footprint.


                i = i__prev1;
            } 

            // sort the cases by Hchan address to get the locking order.
            // simple heap sort, to guarantee n log n time and constant stack footprint.
            slice lockslice = new slice(unsafe.Pointer(sel.lockorder),int(sel.ncase),int(sel.ncase));
            *(*slice<ushort>) lockorder = @unsafe.Pointer(ref lockslice).Value;
            {
                long i__prev1 = i;

                for (i = 0L; i < int(sel.ncase); i++)
                {
                    j = i; 
                    // Start with the pollorder to permute cases on the same channel.
                    var c = scases[pollorder[i]].c;
                    while (j > 0L && scases[lockorder[(j - 1L) / 2L]].c.sortkey() < c.sortkey())
                    {
                        var k = (j - 1L) / 2L;
                        lockorder[j] = lockorder[k];
                        j = k;
                    }

                    lockorder[j] = pollorder[i];
                }


                i = i__prev1;
            }
            {
                long i__prev1 = i;

                for (i = int(sel.ncase) - 1L; i >= 0L; i--)
                {
                    var o = lockorder[i];
                    c = scases[o].c;
                    lockorder[i] = lockorder[0L];
                    j = 0L;
                    while (true)
                    {
                        k = j * 2L + 1L;
                        if (k >= i)
                        {
                            break;
                        }
                        if (k + 1L < i && scases[lockorder[k]].c.sortkey() < scases[lockorder[k + 1L]].c.sortkey())
                        {
                            k++;
                        }
                        if (c.sortkey() < scases[lockorder[k]].c.sortkey())
                        {
                            lockorder[j] = lockorder[k];
                            j = k;
                            continue;
                        }
                        break;
                    }

                    lockorder[j] = o;
                }
                /*
                        for i := 0; i+1 < int(sel.ncase); i++ {
                            if scases[lockorder[i]].c.sortkey() > scases[lockorder[i+1]].c.sortkey() {
                                print("i=", i, " x=", lockorder[i], " y=", lockorder[i+1], "\n")
                                throw("select: broken sort")
                            }
                        }
                    */

                // lock all the channels involved in the select


                i = i__prev1;
            }
            /*
                    for i := 0; i+1 < int(sel.ncase); i++ {
                        if scases[lockorder[i]].c.sortkey() > scases[lockorder[i+1]].c.sortkey() {
                            print("i=", i, " x=", lockorder[i], " y=", lockorder[i+1], "\n")
                            throw("select: broken sort")
                        }
                    }
                */

            // lock all the channels involved in the select
            sellock(scases, lockorder);

            ref g gp = default;            ref sudog sg = default;            c = default;            k = default;            ref sudog sglist = default;            ref sudog sgnext = default;            unsafe.Pointer qp = default;            ptr<ptr<sudog>> nextp = default;

loop:
            long dfli = default;
            ref scase dfl = default;
            long casi = default;
            ref scase cas = default;
            {
                long i__prev1 = i;

                for (i = 0L; i < int(sel.ncase); i++)
                {
                    casi = int(pollorder[i]);
                    cas = ref scases[casi];
                    c = cas.c;


                    if (cas.kind == caseNil) 
                        continue;
                    else if (cas.kind == caseRecv) 
                        sg = c.sendq.dequeue();
                        if (sg != null)
                        {
                            goto recv;
                        }
                        if (c.qcount > 0L)
                        {
                            goto bufrecv;
                        }
                        if (c.closed != 0L)
                        {
                            goto rclose;
                        }
                    else if (cas.kind == caseSend) 
                        if (raceenabled)
                        {
                            racereadpc(@unsafe.Pointer(c), cas.pc, chansendpc);
                        }
                        if (c.closed != 0L)
                        {
                            goto sclose;
                        }
                        sg = c.recvq.dequeue();
                        if (sg != null)
                        {
                            goto send;
                        }
                        if (c.qcount < c.dataqsiz)
                        {
                            goto bufsend;
                        }
                    else if (cas.kind == caseDefault) 
                        dfli = casi;
                        dfl = cas;
                                    }


                i = i__prev1;
            }

            if (dfl != null)
            {
                selunlock(scases, lockorder);
                casi = dfli;
                cas = dfl;
                goto retc;
            } 

            // pass 2 - enqueue on all chans
            gp = getg();
            if (gp.waiting != null)
            {
                throw("gp.waiting != nil");
            }
            nextp = ref gp.waiting;
            {
                var casei__prev1 = casei;

                foreach (var (_, __casei) in lockorder)
                {
                    casei = __casei;
                    casi = int(casei);
                    cas = ref scases[casi];
                    if (cas.kind == caseNil)
                    {
                        continue;
                    }
                    c = cas.c;
                    sg = acquireSudog();
                    sg.g = gp;
                    sg.isSelect = true; 
                    // No stack splits between assigning elem and enqueuing
                    // sg on gp.waiting where copystack can find it.
                    sg.elem = cas.elem;
                    sg.releasetime = 0L;
                    if (t0 != 0L)
                    {
                        sg.releasetime = -1L;
                    }
                    sg.c = c; 
                    // Construct waiting list in lock order.
                    nextp.Value = sg;
                    nextp = ref sg.waitlink;


                    if (cas.kind == caseRecv) 
                        c.recvq.enqueue(sg);
                    else if (cas.kind == caseSend) 
                        c.sendq.enqueue(sg);
                                    } 

                // wait for someone to wake us up

                casei = casei__prev1;
            }

            gp.param = null;
            gopark(selparkcommit, null, "select", traceEvGoBlockSelect, 1L);

            sellock(scases, lockorder);

            gp.selectDone = 0L;
            sg = (sudog.Value)(gp.param);
            gp.param = null; 

            // pass 3 - dequeue from unsuccessful chans
            // otherwise they stack up on quiet channels
            // record the successful case, if any.
            // We singly-linked up the SudoGs in lock order.
            casi = -1L;
            cas = null;
            sglist = gp.waiting; 
            // Clear all elem before unlinking from gp.waiting.
            {
                var sg1 = gp.waiting;

                while (sg1 != null)
                {
                    sg1.isSelect = false;
                    sg1.elem = null;
                    sg1.c = null;
                    sg1 = sg1.waitlink;
                }

            }
            gp.waiting = null;

            {
                var casei__prev1 = casei;

                foreach (var (_, __casei) in lockorder)
                {
                    casei = __casei;
                    k = ref scases[casei];
                    if (k.kind == caseNil)
                    {
                        continue;
                    }
                    if (sglist.releasetime > 0L)
                    {
                        k.releasetime = sglist.releasetime;
                    }
                    if (sg == sglist)
                    { 
                        // sg has already been dequeued by the G that woke us up.
                        casi = int(casei);
                        cas = k;
                    }
                    else
                    {
                        c = k.c;
                        if (k.kind == caseSend)
                        {
                            c.sendq.dequeueSudoG(sglist);
                        }
                        else
                        {
                            c.recvq.dequeueSudoG(sglist);
                        }
                    }
                    sgnext = sglist.waitlink;
                    sglist.waitlink = null;
                    releaseSudog(sglist);
                    sglist = sgnext;
                }

                casei = casei__prev1;
            }

            if (cas == null)
            { 
                // We can wake up with gp.param == nil (so cas == nil)
                // when a channel involved in the select has been closed.
                // It is easiest to loop and re-run the operation;
                // we'll see that it's now closed.
                // Maybe some day we can signal the close explicitly,
                // but we'd have to distinguish close-on-reader from close-on-writer.
                // It's easiest not to duplicate the code and just recheck above.
                // We know that something closed, and things never un-close,
                // so we won't block again.
                goto loop;
            }
            c = cas.c;

            if (debugSelect)
            {
                print("wait-return: sel=", sel, " c=", c, " cas=", cas, " kind=", cas.kind, "\n");
            }
            if (cas.kind == caseRecv && cas.receivedp != null)
            {
                cas.receivedp.Value = true;
            }
            if (raceenabled)
            {
                if (cas.kind == caseRecv && cas.elem != null)
                {
                    raceWriteObjectPC(c.elemtype, cas.elem, cas.pc, chanrecvpc);
                }
                else if (cas.kind == caseSend)
                {
                    raceReadObjectPC(c.elemtype, cas.elem, cas.pc, chansendpc);
                }
            }
            if (msanenabled)
            {
                if (cas.kind == caseRecv && cas.elem != null)
                {
                    msanwrite(cas.elem, c.elemtype.size);
                }
                else if (cas.kind == caseSend)
                {
                    msanread(cas.elem, c.elemtype.size);
                }
            }
            selunlock(scases, lockorder);
            goto retc;

bufrecv:
            if (raceenabled)
            {
                if (cas.elem != null)
                {
                    raceWriteObjectPC(c.elemtype, cas.elem, cas.pc, chanrecvpc);
                }
                raceacquire(chanbuf(c, c.recvx));
                racerelease(chanbuf(c, c.recvx));
            }
            if (msanenabled && cas.elem != null)
            {
                msanwrite(cas.elem, c.elemtype.size);
            }
            if (cas.receivedp != null)
            {
                cas.receivedp.Value = true;
            }
            qp = chanbuf(c, c.recvx);
            if (cas.elem != null)
            {
                typedmemmove(c.elemtype, cas.elem, qp);
            }
            typedmemclr(c.elemtype, qp);
            c.recvx++;
            if (c.recvx == c.dataqsiz)
            {
                c.recvx = 0L;
            }
            c.qcount--;
            selunlock(scases, lockorder);
            goto retc;

bufsend:
            if (raceenabled)
            {
                raceacquire(chanbuf(c, c.sendx));
                racerelease(chanbuf(c, c.sendx));
                raceReadObjectPC(c.elemtype, cas.elem, cas.pc, chansendpc);
            }
            if (msanenabled)
            {
                msanread(cas.elem, c.elemtype.size);
            }
            typedmemmove(c.elemtype, chanbuf(c, c.sendx), cas.elem);
            c.sendx++;
            if (c.sendx == c.dataqsiz)
            {
                c.sendx = 0L;
            }
            c.qcount++;
            selunlock(scases, lockorder);
            goto retc;

recv:
            recv(c, sg, cas.elem, () =>
            {
                selunlock(scases, lockorder);

            }, 2L);
            if (debugSelect)
            {
                print("syncrecv: sel=", sel, " c=", c, "\n");
            }
            if (cas.receivedp != null)
            {
                cas.receivedp.Value = true;
            }
            goto retc;

rclose:
            selunlock(scases, lockorder);
            if (cas.receivedp != null)
            {
                cas.receivedp.Value = false;
            }
            if (cas.elem != null)
            {
                typedmemclr(c.elemtype, cas.elem);
            }
            if (raceenabled)
            {
                raceacquire(@unsafe.Pointer(c));
            }
            goto retc;

send:
            if (raceenabled)
            {
                raceReadObjectPC(c.elemtype, cas.elem, cas.pc, chansendpc);
            }
            if (msanenabled)
            {
                msanread(cas.elem, c.elemtype.size);
            }
            send(c, sg, cas.elem, () =>
            {
                selunlock(scases, lockorder);

            }, 2L);
            if (debugSelect)
            {
                print("syncsend: sel=", sel, " c=", c, "\n");
            }
            goto retc;

retc:
            if (cas.releasetime > 0L)
            {
                blockevent(cas.releasetime - t0, 1L);
            }
            return casi;

sclose:
            selunlock(scases, lockorder);
            panic(plainError("send on closed channel"));
        });

        private static System.UIntPtr sortkey(this ref hchan c)
        { 
            // TODO(khr): if we have a moving garbage collector, we'll need to
            // change this function.
            return uintptr(@unsafe.Pointer(c));
        }

        // A runtimeSelect is a single case passed to rselect.
        // This must match ../reflect/value.go:/runtimeSelect
        private partial struct runtimeSelect
        {
            public selectDir dir;
            public unsafe.Pointer typ; // channel type (not used here)
            public ptr<hchan> ch; // channel
            public unsafe.Pointer val; // ptr to data (SendDir) or ptr to receive buffer (RecvDir)
        }

        // These values must match ../reflect/value.go:/SelectDir.
        private partial struct selectDir // : long
        {
        }

        private static readonly selectDir _ = iota;
        private static readonly var selectSend = 0; // case Chan <- Send
        private static readonly var selectRecv = 1; // case <-Chan:
        private static readonly var selectDefault = 2; // default

        //go:linkname reflect_rselect reflect.rselect
        private static (long, bool) reflect_rselect(slice<runtimeSelect> cases)
        { 
            // flagNoScan is safe here, because all objects are also referenced from cases.
            var size = selectsize(uintptr(len(cases)));
            var sel = (hselect.Value)(mallocgc(size, null, true));
            newselect(sel, int64(size), int32(len(cases)));
            ptr<bool> r = @new<bool>();
            foreach (var (i) in cases)
            {
                var rc = ref cases[i];

                if (rc.dir == selectDefault) 
                    selectdefault(sel);
                else if (rc.dir == selectSend) 
                    selectsend(sel, rc.ch, rc.val);
                else if (rc.dir == selectRecv) 
                    selectrecv(sel, rc.ch, rc.val, r);
                            }
            chosen = selectgo(sel);
            recvOK = r.Value;
            return;
        }

        private static void dequeueSudoG(this ref waitq q, ref sudog sgp)
        {
            var x = sgp.prev;
            var y = sgp.next;
            if (x != null)
            {
                if (y != null)
                { 
                    // middle of queue
                    x.next = y;
                    y.prev = x;
                    sgp.next = null;
                    sgp.prev = null;
                    return;
                } 
                // end of queue
                x.next = null;
                q.last = x;
                sgp.prev = null;
                return;
            }
            if (y != null)
            { 
                // start of queue
                y.prev = null;
                q.first = y;
                sgp.next = null;
                return;
            } 

            // x==y==nil. Either sgp is the only element in the queue,
            // or it has already been removed. Use q.first to disambiguate.
            if (q.first == sgp)
            {
                q.first = null;
                q.last = null;
            }
        }
    }
}
