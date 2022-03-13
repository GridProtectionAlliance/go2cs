// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2022 March 13 05:26:51 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\select.go
namespace go;
// This file contains the implementation of Go select statements.


using atomic = runtime.@internal.atomic_package;
using @unsafe = @unsafe_package;
using System;

public static partial class runtime_package {

private static readonly var debugSelect = false;

// Select case descriptor.
// Known to compiler.
// Changes here must also be made in src/cmd/compile/internal/walk/select.go's scasetype.


// Select case descriptor.
// Known to compiler.
// Changes here must also be made in src/cmd/compile/internal/walk/select.go's scasetype.
private partial struct scase {
    public ptr<hchan> c; // chan
    public unsafe.Pointer elem; // data element
}

private static var chansendpc = funcPC(chansend);private static var chanrecvpc = funcPC(chanrecv);

private static void selectsetpc(ptr<System.UIntPtr> _addr_pc) {
    ref System.UIntPtr pc = ref _addr_pc.val;

    pc = getcallerpc();
}

private static void sellock(slice<scase> scases, slice<ushort> lockorder) {
    ptr<hchan> c;
    foreach (var (_, o) in lockorder) {
        var c0 = scases[o].c;
        if (c0 != c) {
            c = c0;
            lock(_addr_c.@lock);
        }
    }
}

private static void selunlock(slice<scase> scases, slice<ushort> lockorder) { 
    // We must be very careful here to not touch sel after we have unlocked
    // the last lock, because sel can be freed right after the last unlock.
    // Consider the following situation.
    // First M calls runtime·park() in runtime·selectgo() passing the sel.
    // Once runtime·park() has unlocked the last lock, another M makes
    // the G that calls select runnable again and schedules it for execution.
    // When the G runs on another M, it locks all the locks and frees sel.
    // Now if the first M touches sel, it will access freed memory.
    for (var i = len(lockorder) - 1; i >= 0; i--) {
        var c = scases[lockorder[i]].c;
        if (i > 0 && c == scases[lockorder[i - 1]].c) {
            continue; // will unlock it on the next iteration
        }
        unlock(_addr_c.@lock);
    }
}

private static bool selparkcommit(ptr<g> _addr_gp, unsafe.Pointer _) {
    ref g gp = ref _addr_gp.val;
 
    // There are unlocked sudogs that point into gp's stack. Stack
    // copying must lock the channels of those sudogs.
    // Set activeStackChans here instead of before we try parking
    // because we could self-deadlock in stack growth on a
    // channel lock.
    gp.activeStackChans = true; 
    // Mark that it's safe for stack shrinking to occur now,
    // because any thread acquiring this G's stack for shrinking
    // is guaranteed to observe activeStackChans after this store.
    atomic.Store8(_addr_gp.parkingOnChan, 0); 
    // Make sure we unlock after setting activeStackChans and
    // unsetting parkingOnChan. The moment we unlock any of the
    // channel locks we risk gp getting readied by a channel operation
    // and so gp could continue running before everything before the
    // unlock is visible (even to gp itself).

    // This must not access gp's stack (see gopark). In
    // particular, it must not access the *hselect. That's okay,
    // because by the time this is called, gp.waiting has all
    // channels in lock order.
    ptr<hchan> lastc;
    {
        var sg = gp.waiting;

        while (sg != null) {
            if (sg.c != lastc && lastc != null) { 
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
    if (lastc != null) {
        unlock(_addr_lastc.@lock);
    }
    return true;
}

private static void block() {
    gopark(null, null, waitReasonSelectNoCases, traceEvGoStop, 1); // forever
}

// selectgo implements the select statement.
//
// cas0 points to an array of type [ncases]scase, and order0 points to
// an array of type [2*ncases]uint16 where ncases must be <= 65536.
// Both reside on the goroutine's stack (regardless of any escaping in
// selectgo).
//
// For race detector builds, pc0 points to an array of type
// [ncases]uintptr (also on the stack); for other builds, it's set to
// nil.
//
// selectgo returns the index of the chosen scase, which matches the
// ordinal position of its respective select{recv,send,default} call.
// Also, if the chosen scase was a receive operation, it reports whether
// a value was received.
private static (nint, bool) selectgo(ptr<scase> _addr_cas0, ptr<ushort> _addr_order0, ptr<System.UIntPtr> _addr_pc0, nint nsends, nint nrecvs, bool block) => func((_, panic, _) => {
    nint _p0 = default;
    bool _p0 = default;
    ref scase cas0 = ref _addr_cas0.val;
    ref ushort order0 = ref _addr_order0.val;
    ref System.UIntPtr pc0 = ref _addr_pc0.val;

    if (debugSelect) {
        print("select: cas0=", cas0, "\n");
    }
    ptr<array<scase>> cas1 = new ptr<ptr<array<scase>>>(@unsafe.Pointer(cas0));
    ptr<array<ushort>> order1 = new ptr<ptr<array<ushort>>>(@unsafe.Pointer(order0));

    var ncases = nsends + nrecvs;
    var scases = cas1.slice(-1, ncases, ncases);
    var pollorder = order1.slice(-1, ncases, ncases);
    var lockorder = order1[(int)ncases..].slice(-1, ncases, ncases); 
    // NOTE: pollorder/lockorder's underlying array was not zero-initialized by compiler.

    // Even when raceenabled is true, there might be select
    // statements in packages compiled without -race (e.g.,
    // ensureSigM in runtime/signal_unix.go).
    slice<System.UIntPtr> pcs = default;
    if (raceenabled && pc0 != null) {
        ptr<array<System.UIntPtr>> pc1 = new ptr<ptr<array<System.UIntPtr>>>(@unsafe.Pointer(pc0));
        pcs = pc1.slice(-1, ncases, ncases);
    }
    Func<nint, System.UIntPtr> casePC = casi => {
        if (pcs == null) {
            return 0;
        }
        return pcs[casi];
    };

    long t0 = default;
    if (blockprofilerate > 0) {
        t0 = cputicks();
    }
    nint norder = 0;
    {
        var i__prev1 = i;

        foreach (var (__i) in scases) {
            i = __i;
            var cas = _addr_scases[i]; 

            // Omit cases without channels from the poll and lock orders.
            if (cas.c == null) {
                cas.elem = null; // allow GC
                continue;
            }
            var j = fastrandn(uint32(norder + 1));
            pollorder[norder] = pollorder[j];
            pollorder[j] = uint16(i);
            norder++;
        }
        i = i__prev1;
    }

    pollorder = pollorder[..(int)norder];
    lockorder = lockorder[..(int)norder]; 

    // sort the cases by Hchan address to get the locking order.
    // simple heap sort, to guarantee n log n time and constant stack footprint.
    {
        var i__prev1 = i;

        foreach (var (__i) in lockorder) {
            i = __i;
            j = i; 
            // Start with the pollorder to permute cases on the same channel.
            var c = scases[pollorder[i]].c;
            while (j > 0 && scases[lockorder[(j - 1) / 2]].c.sortkey() < c.sortkey()) {
                var k = (j - 1) / 2;
                lockorder[j] = lockorder[k];
                j = k;
            }

            lockorder[j] = pollorder[i];
        }
        i = i__prev1;
    }

    {
        var i__prev1 = i;

        for (var i = len(lockorder) - 1; i >= 0; i--) {
            var o = lockorder[i];
            c = scases[o].c;
            lockorder[i] = lockorder[0];
            j = 0;
            while (true) {
                k = j * 2 + 1;
                if (k >= i) {
                    break;
                }
                if (k + 1 < i && scases[lockorder[k]].c.sortkey() < scases[lockorder[k + 1]].c.sortkey()) {
                    k++;
                }
                if (c.sortkey() < scases[lockorder[k]].c.sortkey()) {
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

    if (debugSelect) {
        {
            var i__prev1 = i;

            for (i = 0; i + 1 < len(lockorder); i++) {
                if (scases[lockorder[i]].c.sortkey() > scases[lockorder[i + 1]].c.sortkey()) {
                    print("i=", i, " x=", lockorder[i], " y=", lockorder[i + 1], "\n");
                    throw("select: broken sort");
                }
            }


            i = i__prev1;
        }
    }
    sellock(scases, lockorder);

    ptr<g> gp;    ptr<sudog> sg;    c = ;    k = ;    ptr<sudog> sglist;    ptr<sudog> sgnext;    unsafe.Pointer qp = default;    ptr<ptr<sudog>> nextp; 

    // pass 1 - look for something already waiting
    nint casi = default;
    cas = ;
    bool caseSuccess = default;
    long caseReleaseTime = -1;
    bool recvOK = default;
    {
        var casei__prev1 = casei;

        foreach (var (_, __casei) in pollorder) {
            casei = __casei;
            casi = int(casei);
            cas = _addr_scases[casi];
            c = cas.c;

            if (casi >= nsends) {
                sg = c.sendq.dequeue();
                if (sg != null) {
                    goto recv;
                }
                if (c.qcount > 0) {
                    goto bufrecv;
                }
                if (c.closed != 0) {
                    goto rclose;
                }
            }
            else
 {
                if (raceenabled) {
                    racereadpc(c.raceaddr(), casePC(casi), chansendpc);
                }
                if (c.closed != 0) {
                    goto sclose;
                }
                sg = c.recvq.dequeue();
                if (sg != null) {
                    goto send;
                }
                if (c.qcount < c.dataqsiz) {
                    goto bufsend;
                }
            }
        }
        casei = casei__prev1;
    }

    if (!block) {
        selunlock(scases, lockorder);
        casi = -1;
        goto retc;
    }
    gp = getg();
    if (gp.waiting != null) {
        throw("gp.waiting != nil");
    }
    nextp = _addr_gp.waiting;
    {
        var casei__prev1 = casei;

        foreach (var (_, __casei) in lockorder) {
            casei = __casei;
            casi = int(casei);
            cas = _addr_scases[casi];
            c = cas.c;
            sg = acquireSudog();
            sg.g = gp;
            sg.isSelect = true; 
            // No stack splits between assigning elem and enqueuing
            // sg on gp.waiting where copystack can find it.
            sg.elem = cas.elem;
            sg.releasetime = 0;
            if (t0 != 0) {
                sg.releasetime = -1;
            }
            sg.c = c; 
            // Construct waiting list in lock order.
            nextp.val = sg;
            nextp = _addr_sg.waitlink;

            if (casi < nsends) {
                c.sendq.enqueue(sg);
            }
            else
 {
                c.recvq.enqueue(sg);
            }
        }
        casei = casei__prev1;
    }

    gp.param = null; 
    // Signal to anyone trying to shrink our stack that we're about
    // to park on a channel. The window between when this G's status
    // changes and when we set gp.activeStackChans is not safe for
    // stack shrinking.
    atomic.Store8(_addr_gp.parkingOnChan, 1);
    gopark(selparkcommit, null, waitReasonSelect, traceEvGoBlockSelect, 1);
    gp.activeStackChans = false;

    sellock(scases, lockorder);

    gp.selectDone = 0;
    sg = (sudog.val)(gp.param);
    gp.param = null; 

    // pass 3 - dequeue from unsuccessful chans
    // otherwise they stack up on quiet channels
    // record the successful case, if any.
    // We singly-linked up the SudoGs in lock order.
    casi = -1;
    cas = null;
    caseSuccess = false;
    sglist = gp.waiting; 
    // Clear all elem before unlinking from gp.waiting.
    {
        var sg1 = gp.waiting;

        while (sg1 != null) {
            sg1.isSelect = false;
            sg1.elem = null;
            sg1.c = null;
            sg1 = sg1.waitlink;
        }
    }
    gp.waiting = null;

    {
        var casei__prev1 = casei;

        foreach (var (_, __casei) in lockorder) {
            casei = __casei;
            k = _addr_scases[casei];
            if (sg == sglist) { 
                // sg has already been dequeued by the G that woke us up.
                casi = int(casei);
                cas = k;
                caseSuccess = sglist.success;
                if (sglist.releasetime > 0) {
                    caseReleaseTime = sglist.releasetime;
                }
            }
            else
 {
                c = k.c;
                if (int(casei) < nsends) {
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

    if (cas == null) {
        throw("selectgo: bad wakeup");
    }
    c = cas.c;

    if (debugSelect) {
        print("wait-return: cas0=", cas0, " c=", c, " cas=", cas, " send=", casi < nsends, "\n");
    }
    if (casi < nsends) {
        if (!caseSuccess) {
            goto sclose;
        }
    }
    else
 {
        recvOK = caseSuccess;
    }
    if (raceenabled) {
        if (casi < nsends) {
            raceReadObjectPC(c.elemtype, cas.elem, casePC(casi), chansendpc);
        }
        else if (cas.elem != null) {
            raceWriteObjectPC(c.elemtype, cas.elem, casePC(casi), chanrecvpc);
        }
    }
    if (msanenabled) {
        if (casi < nsends) {
            msanread(cas.elem, c.elemtype.size);
        }
        else if (cas.elem != null) {
            msanwrite(cas.elem, c.elemtype.size);
        }
    }
    selunlock(scases, lockorder);
    goto retc;

bufrecv:
    if (raceenabled) {
        if (cas.elem != null) {
            raceWriteObjectPC(c.elemtype, cas.elem, casePC(casi), chanrecvpc);
        }
        racenotify(c, c.recvx, null);
    }
    if (msanenabled && cas.elem != null) {
        msanwrite(cas.elem, c.elemtype.size);
    }
    recvOK = true;
    qp = chanbuf(c, c.recvx);
    if (cas.elem != null) {
        typedmemmove(c.elemtype, cas.elem, qp);
    }
    typedmemclr(c.elemtype, qp);
    c.recvx++;
    if (c.recvx == c.dataqsiz) {
        c.recvx = 0;
    }
    c.qcount--;
    selunlock(scases, lockorder);
    goto retc;

bufsend:
    if (raceenabled) {
        racenotify(c, c.sendx, null);
        raceReadObjectPC(c.elemtype, cas.elem, casePC(casi), chansendpc);
    }
    if (msanenabled) {
        msanread(cas.elem, c.elemtype.size);
    }
    typedmemmove(c.elemtype, chanbuf(c, c.sendx), cas.elem);
    c.sendx++;
    if (c.sendx == c.dataqsiz) {
        c.sendx = 0;
    }
    c.qcount++;
    selunlock(scases, lockorder);
    goto retc;

recv:
    recv(c, sg, cas.elem, () => {
        selunlock(scases, lockorder);
    }, 2);
    if (debugSelect) {
        print("syncrecv: cas0=", cas0, " c=", c, "\n");
    }
    recvOK = true;
    goto retc;

rclose:
    selunlock(scases, lockorder);
    recvOK = false;
    if (cas.elem != null) {
        typedmemclr(c.elemtype, cas.elem);
    }
    if (raceenabled) {
        raceacquire(c.raceaddr());
    }
    goto retc;

send:
    if (raceenabled) {
        raceReadObjectPC(c.elemtype, cas.elem, casePC(casi), chansendpc);
    }
    if (msanenabled) {
        msanread(cas.elem, c.elemtype.size);
    }
    send(c, sg, cas.elem, () => {
        selunlock(scases, lockorder);
    }, 2);
    if (debugSelect) {
        print("syncsend: cas0=", cas0, " c=", c, "\n");
    }
    goto retc;

retc:
    if (caseReleaseTime > 0) {
        blockevent(caseReleaseTime - t0, 1);
    }
    return (casi, recvOK);

sclose:
    selunlock(scases, lockorder);
    panic(plainError("send on closed channel"));
});

private static System.UIntPtr sortkey(this ptr<hchan> _addr_c) {
    ref hchan c = ref _addr_c.val;

    return uintptr(@unsafe.Pointer(c));
}

// A runtimeSelect is a single case passed to rselect.
// This must match ../reflect/value.go:/runtimeSelect
private partial struct runtimeSelect {
    public selectDir dir;
    public unsafe.Pointer typ; // channel type (not used here)
    public ptr<hchan> ch; // channel
    public unsafe.Pointer val; // ptr to data (SendDir) or ptr to receive buffer (RecvDir)
}

// These values must match ../reflect/value.go:/SelectDir.
private partial struct selectDir { // : nint
}

private static readonly selectDir _ = iota;
private static readonly var selectSend = 0; // case Chan <- Send
private static readonly var selectRecv = 1; // case <-Chan:
private static readonly var selectDefault = 2; // default

//go:linkname reflect_rselect reflect.rselect
private static (nint, bool) reflect_rselect(slice<runtimeSelect> cases) {
    nint _p0 = default;
    bool _p0 = default;

    if (len(cases) == 0) {
        block();
    }
    var sel = make_slice<scase>(len(cases));
    var orig = make_slice<nint>(len(cases));
    nint nsends = 0;
    nint nrecvs = 0;
    nint dflt = -1;
    {
        var i__prev1 = i;

        foreach (var (__i, __rc) in cases) {
            i = __i;
            rc = __rc;
            nint j = default;

            if (rc.dir == selectDefault) 
                dflt = i;
                continue;
            else if (rc.dir == selectSend) 
                j = nsends;
                nsends++;
            else if (rc.dir == selectRecv) 
                nrecvs++;
                j = len(cases) - nrecvs;
                        sel[j] = new scase(c:rc.ch,elem:rc.val);
            orig[j] = i;
        }
        i = i__prev1;
    }

    if (nsends + nrecvs == 0) {
        return (dflt, false);
    }
    if (nsends + nrecvs < len(cases)) {
        copy(sel[(int)nsends..], sel[(int)len(cases) - nrecvs..]);
        copy(orig[(int)nsends..], orig[(int)len(cases) - nrecvs..]);
    }
    var order = make_slice<ushort>(2 * (nsends + nrecvs));
    ptr<System.UIntPtr> pc0;
    if (raceenabled) {
        var pcs = make_slice<System.UIntPtr>(nsends + nrecvs);
        {
            var i__prev1 = i;

            foreach (var (__i) in pcs) {
                i = __i;
                selectsetpc(_addr_pcs[i]);
            }

            i = i__prev1;
        }

        pc0 = _addr_pcs[0];
    }
    var (chosen, recvOK) = selectgo(_addr_sel[0], _addr_order[0], pc0, nsends, nrecvs, dflt == -1); 

    // Translate chosen back to caller's ordering.
    if (chosen < 0) {
        chosen = dflt;
    }
    else
 {
        chosen = orig[chosen];
    }
    return (chosen, recvOK);
}

private static void dequeueSudoG(this ptr<waitq> _addr_q, ptr<sudog> _addr_sgp) {
    ref waitq q = ref _addr_q.val;
    ref sudog sgp = ref _addr_sgp.val;

    var x = sgp.prev;
    var y = sgp.next;
    if (x != null) {
        if (y != null) { 
            // middle of queue
            x.next = y;
            y.prev = x;
            sgp.next = null;
            sgp.prev = null;
            return ;
        }
        x.next = null;
        q.last = x;
        sgp.prev = null;
        return ;
    }
    if (y != null) { 
        // start of queue
        y.prev = null;
        q.first = y;
        sgp.next = null;
        return ;
    }
    if (q.first == sgp) {
        q.first = null;
        q.last = null;
    }
}

} // end runtime_package
