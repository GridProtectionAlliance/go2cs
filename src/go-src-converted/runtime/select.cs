// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 October 08 03:22:56 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\select.go
// This file contains the implementation of Go select statements.

using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class runtime_package
    {
        private static readonly var debugSelect = (var)false;

        // scase.kind values.
        // Known to compiler.
        // Changes here must also be made in src/cmd/compile/internal/gc/select.go's walkselectcases.


        // scase.kind values.
        // Known to compiler.
        // Changes here must also be made in src/cmd/compile/internal/gc/select.go's walkselectcases.
        private static readonly var caseNil = (var)iota;
        private static readonly var caseRecv = (var)0;
        private static readonly var caseSend = (var)1;
        private static readonly var caseDefault = (var)2;


        // Select case descriptor.
        // Known to compiler.
        // Changes here must also be made in src/cmd/internal/gc/select.go's scasetype.
        private partial struct scase
        {
            public ptr<hchan> c; // chan
            public unsafe.Pointer elem; // data element
            public ushort kind;
            public System.UIntPtr pc; // race pc (for race detector / msan)
            public long releasetime;
        }

        private static var chansendpc = funcPC(chansend);        private static var chanrecvpc = funcPC(chanrecv);

        private static void selectsetpc(ptr<scase> _addr_cas)
        {
            ref scase cas = ref _addr_cas.val;

            cas.pc = getcallerpc();
        }

        private static void sellock(slice<scase> scases, slice<ushort> lockorder)
        {
            ptr<hchan> c;
            foreach (var (_, o) in lockorder)
            {
                var c0 = scases[o].c;
                if (c0 != null && c0 != c)
                {
                    c = c0;
                    lock(_addr_c.@lock);
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

                unlock(_addr_c.@lock);

            }


        }

        private static bool selparkcommit(ptr<g> _addr_gp, unsafe.Pointer _)
        {
            ref g gp = ref _addr_gp.val;
 
            // There are unlocked sudogs that point into gp's stack. Stack
            // copying must lock the channels of those sudogs.
            gp.activeStackChans = true; 
            // This must not access gp's stack (see gopark). In
            // particular, it must not access the *hselect. That's okay,
            // because by the time this is called, gp.waiting has all
            // channels in lock order.
            ptr<hchan> lastc;
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
                        unlock(_addr_lastc.@lock);
                    sg = sg.waitlink;
                    }

                    lastc = sg.c;

                }

            }
            if (lastc != null)
            {
                unlock(_addr_lastc.@lock);
            }

            return true;

        }

        private static void block()
        {
            gopark(null, null, waitReasonSelectNoCases, traceEvGoStop, 1L); // forever
        }

        // selectgo implements the select statement.
        //
        // cas0 points to an array of type [ncases]scase, and order0 points to
        // an array of type [2*ncases]uint16 where ncases must be <= 65536.
        // Both reside on the goroutine's stack (regardless of any escaping in
        // selectgo).
        //
        // selectgo returns the index of the chosen scase, which matches the
        // ordinal position of its respective select{recv,send,default} call.
        // Also, if the chosen scase was a receive operation, it reports whether
        // a value was received.
        private static (long, bool) selectgo(ptr<scase> _addr_cas0, ptr<ushort> _addr_order0, long ncases) => func((_, panic, __) =>
        {
            long _p0 = default;
            bool _p0 = default;
            ref scase cas0 = ref _addr_cas0.val;
            ref ushort order0 = ref _addr_order0.val;

            if (debugSelect)
            {
                print("select: cas0=", cas0, "\n");
            } 

            // NOTE: In order to maintain a lean stack size, the number of scases
            // is capped at 65536.
            ptr<array<scase>> cas1 = new ptr<ptr<array<scase>>>(@unsafe.Pointer(cas0));
            ptr<array<ushort>> order1 = new ptr<ptr<array<ushort>>>(@unsafe.Pointer(order0));

            var scases = cas1.slice(-1, ncases, ncases);
            var pollorder = order1.slice(-1, ncases, ncases);
            var lockorder = order1[ncases..].slice(-1, ncases, ncases); 

            // Replace send/receive cases involving nil channels with
            // caseNil so logic below can assume non-nil channel.
            {
                var i__prev1 = i;

                foreach (var (__i) in scases)
                {
                    i = __i;
                    var cas = _addr_scases[i];
                    if (cas.c == null && cas.kind != caseDefault)
                    {
                        cas.val = new scase();
                    }

                }

                i = i__prev1;
            }

            long t0 = default;
            if (blockprofilerate > 0L)
            {
                t0 = cputicks();
                {
                    var i__prev1 = i;

                    for (long i = 0L; i < ncases; i++)
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
            {
                var i__prev1 = i;

                for (i = 1L; i < ncases; i++)
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
            {
                var i__prev1 = i;

                for (i = 0L; i < ncases; i++)
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
                var i__prev1 = i;

                for (i = ncases - 1L; i >= 0L; i--)
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


                i = i__prev1;
            }

            if (debugSelect)
            {
                {
                    var i__prev1 = i;

                    for (i = 0L; i + 1L < ncases; i++)
                    {
                        if (scases[lockorder[i]].c.sortkey() > scases[lockorder[i + 1L]].c.sortkey())
                        {
                            print("i=", i, " x=", lockorder[i], " y=", lockorder[i + 1L], "\n");
                            throw("select: broken sort");
                        }

                    }


                    i = i__prev1;
                }

            } 

            // lock all the channels involved in the select
            sellock(scases, lockorder);

            ptr<g> gp;            ptr<sudog> sg;            c = ;            k = ;            ptr<sudog> sglist;            ptr<sudog> sgnext;            unsafe.Pointer qp = default;            ptr<ptr<sudog>> nextp;

loop:
            long dfli = default;
            ptr<scase> dfl;
            long casi = default;
            cas = ;
            bool recvOK = default;
            {
                var i__prev1 = i;

                for (i = 0L; i < ncases; i++)
                {
                    casi = int(pollorder[i]);
                    cas = _addr_scases[casi];
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
                            racereadpc(c.raceaddr(), cas.pc, chansendpc);
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

            nextp = _addr_gp.waiting;
            {
                var casei__prev1 = casei;

                foreach (var (_, __casei) in lockorder)
                {
                    casei = __casei;
                    casi = int(casei);
                    cas = _addr_scases[casi];
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
                    nextp.val = sg;
                    nextp = _addr_sg.waitlink;


                    if (cas.kind == caseRecv) 
                        c.recvq.enqueue(sg);
                    else if (cas.kind == caseSend) 
                        c.sendq.enqueue(sg);
                    
                } 

                // wait for someone to wake us up

                casei = casei__prev1;
            }

            gp.param = null;
            gopark(selparkcommit, null, waitReasonSelect, traceEvGoBlockSelect, 1L);
            gp.activeStackChans = false;

            sellock(scases, lockorder);

            gp.selectDone = 0L;
            sg = (sudog.val)(gp.param);
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
                    k = _addr_scases[casei];
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
                    sglist = addr(sgnext);

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
                print("wait-return: cas0=", cas0, " c=", c, " cas=", cas, " kind=", cas.kind, "\n");
            }

            if (cas.kind == caseRecv)
            {
                recvOK = true;
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

            recvOK = true;
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
                print("syncrecv: cas0=", cas0, " c=", c, "\n");
            }

            recvOK = true;
            goto retc;

rclose:
            selunlock(scases, lockorder);
            recvOK = false;
            if (cas.elem != null)
            {
                typedmemclr(c.elemtype, cas.elem);
            }

            if (raceenabled)
            {
                raceacquire(c.raceaddr());
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
                print("syncsend: cas0=", cas0, " c=", c, "\n");
            }

            goto retc;

retc:
            if (cas.releasetime > 0L)
            {
                blockevent(cas.releasetime - t0, 1L);
            }

            return (casi, recvOK);

sclose:
            selunlock(scases, lockorder);
            panic(plainError("send on closed channel"));

        });

        private static System.UIntPtr sortkey(this ptr<hchan> _addr_c)
        {
            ref hchan c = ref _addr_c.val;

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

        private static readonly selectDir _ = (selectDir)iota;
        private static readonly var selectSend = (var)0; // case Chan <- Send
        private static readonly var selectRecv = (var)1; // case <-Chan:
        private static readonly var selectDefault = (var)2; // default

        //go:linkname reflect_rselect reflect.rselect
        private static (long, bool) reflect_rselect(slice<runtimeSelect> cases)
        {
            long _p0 = default;
            bool _p0 = default;

            if (len(cases) == 0L)
            {
                block();
            }

            var sel = make_slice<scase>(len(cases));
            var order = make_slice<ushort>(2L * len(cases));
            foreach (var (i) in cases)
            {
                var rc = _addr_cases[i];

                if (rc.dir == selectDefault) 
                    sel[i] = new scase(kind:caseDefault);
                else if (rc.dir == selectSend) 
                    sel[i] = new scase(kind:caseSend,c:rc.ch,elem:rc.val);
                else if (rc.dir == selectRecv) 
                    sel[i] = new scase(kind:caseRecv,c:rc.ch,elem:rc.val);
                                if (raceenabled || msanenabled)
                {
                    selectsetpc(_addr_sel[i]);
                }

            }
            return selectgo(_addr_sel[0L], _addr_order[0L], len(cases));

        }

        private static void dequeueSudoG(this ptr<waitq> _addr_q, ptr<sudog> _addr_sgp)
        {
            ref waitq q = ref _addr_q.val;
            ref sudog sgp = ref _addr_sgp.val;

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
                    return ;

                } 
                // end of queue
                x.next = null;
                q.last = x;
                sgp.prev = null;
                return ;

            }

            if (y != null)
            { 
                // start of queue
                y.prev = null;
                q.first = y;
                sgp.next = null;
                return ;

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
