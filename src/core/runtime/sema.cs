// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Semaphore implementation exposed to Go.
// Intended use is provide a sleep and wakeup
// primitive that can be used in the contended case
// of other synchronization primitives.
// Thus it targets the same goal as Linux's futex,
// but it has much simpler semantics.
//
// That is, don't think of these as semaphores.
// Think of them as a way to implement sleep and wakeup
// such that every sleep is paired with a single wakeup,
// even if, due to races, the wakeup happens before the sleep.
//
// See Mullender and Cox, ``Semaphores in Plan 9,''
// https://swtch.com/semaphore.pdf
namespace go;

using cpu = @internal.cpu_package;
using atomic = @internal.runtime.atomic_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.runtime;

partial class runtime_package {

// Asynchronous semaphore for sync.Mutex.

// A semaRoot holds a balanced tree of sudog with distinct addresses (s.elem).
// Each of those sudog may in turn point (through s.waitlink) to a list
// of other sudogs waiting on the same address.
// The operations on the inner lists of sudogs with the same address
// are all O(1). The scanning of the top-level semaRoot list is O(log n),
// where n is the number of distinct addresses with goroutines blocked
// on them that hash to the given semaRoot.
// See golang.org/issue/17953 for a program that worked badly
// before we introduced the second level of list, and
// BenchmarkSemTable/OneAddrCollision/* for a benchmark that exercises this.
[GoType] partial struct semaRoot {
    internal mutex @lock;
    internal ж<sudog> treap;     // root of balanced tree of unique waiters.
    internal @internal.runtime.atomic_package.Uint32 nwait; // Number of waiters. Read w/o the lock.
}

internal static semTable semtable;

// Prime to not correlate with any user patterns.
internal static readonly UntypedInt semTabSize = 251;
// [...]struct {
	root	semaRoot
	pad	[cpu.CacheLinePadSize - unsafe.Sizeof(semaRoot{})]byte
}

[GoRecv] internal static ж<semaRoot> rootFor(this ref semTable t, ж<uint32> Ꮡaddr) {
    ref var addr = ref Ꮡaddr.val;

    return Ꮡt[(((uintptr)new @unsafe.Pointer(Ꮡaddr)) >> (int)(3)) % semTabSize].of(struct{root semaRoot; pad [40]byte}.Ꮡroot);
}

// sync_runtime_Semacquire should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - gvisor.dev/gvisor
//   - github.com/sagernet/gvisor
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname sync_runtime_Semacquire sync.runtime_Semacquire
internal static void sync_runtime_Semacquire(ж<uint32> Ꮡaddr) {
    ref var addr = ref Ꮡaddr.val;

    semacquire1(Ꮡaddr, false, semaBlockProfile, 0, waitReasonSemacquire);
}

//go:linkname poll_runtime_Semacquire internal/poll.runtime_Semacquire
internal static void poll_runtime_Semacquire(ж<uint32> Ꮡaddr) {
    ref var addr = ref Ꮡaddr.val;

    semacquire1(Ꮡaddr, false, semaBlockProfile, 0, waitReasonSemacquire);
}

// sync_runtime_Semrelease should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - gvisor.dev/gvisor
//   - github.com/sagernet/gvisor
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname sync_runtime_Semrelease sync.runtime_Semrelease
internal static void sync_runtime_Semrelease(ж<uint32> Ꮡaddr, bool handoff, nint skipframes) {
    ref var addr = ref Ꮡaddr.val;

    semrelease1(Ꮡaddr, handoff, skipframes);
}

//go:linkname sync_runtime_SemacquireMutex sync.runtime_SemacquireMutex
internal static void sync_runtime_SemacquireMutex(ж<uint32> Ꮡaddr, bool lifo, nint skipframes) {
    ref var addr = ref Ꮡaddr.val;

    semacquire1(Ꮡaddr, lifo, (semaProfileFlags)(semaBlockProfile | semaMutexProfile), skipframes, waitReasonSyncMutexLock);
}

//go:linkname sync_runtime_SemacquireRWMutexR sync.runtime_SemacquireRWMutexR
internal static void sync_runtime_SemacquireRWMutexR(ж<uint32> Ꮡaddr, bool lifo, nint skipframes) {
    ref var addr = ref Ꮡaddr.val;

    semacquire1(Ꮡaddr, lifo, (semaProfileFlags)(semaBlockProfile | semaMutexProfile), skipframes, waitReasonSyncRWMutexRLock);
}

//go:linkname sync_runtime_SemacquireRWMutex sync.runtime_SemacquireRWMutex
internal static void sync_runtime_SemacquireRWMutex(ж<uint32> Ꮡaddr, bool lifo, nint skipframes) {
    ref var addr = ref Ꮡaddr.val;

    semacquire1(Ꮡaddr, lifo, (semaProfileFlags)(semaBlockProfile | semaMutexProfile), skipframes, waitReasonSyncRWMutexLock);
}

//go:linkname poll_runtime_Semrelease internal/poll.runtime_Semrelease
internal static void poll_runtime_Semrelease(ж<uint32> Ꮡaddr) {
    ref var addr = ref Ꮡaddr.val;

    semrelease(Ꮡaddr);
}

internal static void readyWithTime(ж<sudog> Ꮡs, nint traceskip) {
    ref var s = ref Ꮡs.val;

    if (s.releasetime != 0) {
        s.releasetime = cputicks();
    }
    goready(s.g, traceskip);
}

[GoType("num:nint")] partial struct semaProfileFlags;

internal static readonly semaProfileFlags semaBlockProfile = /* 1 << iota */ 1;
internal static readonly semaProfileFlags semaMutexProfile = 2;

// Called from runtime.
internal static void semacquire(ж<uint32> Ꮡaddr) {
    ref var addr = ref Ꮡaddr.val;

    semacquire1(Ꮡaddr, false, 0, 0, waitReasonSemacquire);
}

internal static void semacquire1(ж<uint32> Ꮡaddr, bool lifo, semaProfileFlags profile, nint skipframes, waitReason reason) {
    ref var addr = ref Ꮡaddr.val;

    var gp = getg();
    if (gp != (~(~gp).m).curg) {
        @throw("semacquire not on the G stack"u8);
    }
    // Easy case.
    if (cansemacquire(Ꮡaddr)) {
        return;
    }
    // Harder case:
    //	increment waiter count
    //	try cansemacquire one more time, return if succeeded
    //	enqueue itself as a waiter
    //	sleep
    //	(waiter descriptor is dequeued by signaler)
    var s = acquireSudog();
    var root = semtable.rootFor(Ꮡaddr);
    var t0 = ((int64)0);
    s.val.releasetime = 0;
    s.val.acquiretime = 0;
    s.val.ticket = 0;
    if ((semaProfileFlags)(profile & semaBlockProfile) != 0 && blockprofilerate > 0) {
        t0 = cputicks();
        s.val.releasetime = -1;
    }
    if ((semaProfileFlags)(profile & semaMutexProfile) != 0 && mutexprofilerate > 0) {
        if (t0 == 0) {
            t0 = cputicks();
        }
        s.val.acquiretime = t0;
    }
    while (ᐧ) {
        lockWithRank(Ꮡ((~root).@lock), lockRankRoot);
        // Add ourselves to nwait to disable "easy case" in semrelease.
        (~root).nwait.Add(1);
        // Check cansemacquire to avoid missed wakeup.
        if (cansemacquire(Ꮡaddr)) {
            (~root).nwait.Add(-1);
            unlock(Ꮡ((~root).@lock));
            break;
        }
        // Any semrelease after the cansemacquire knows we're waiting
        // (we set nwait above), so go to sleep.
        root.queue(Ꮡaddr, s, lifo);
        goparkunlock(Ꮡ((~root).@lock), reason, traceBlockSync, 4 + skipframes);
        if ((~s).ticket != 0 || cansemacquire(Ꮡaddr)) {
            break;
        }
    }
    if ((~s).releasetime > 0) {
        blockevent((~s).releasetime - t0, 3 + skipframes);
    }
    releaseSudog(s);
}

internal static void semrelease(ж<uint32> Ꮡaddr) {
    ref var addr = ref Ꮡaddr.val;

    semrelease1(Ꮡaddr, false, 0);
}

internal static void semrelease1(ж<uint32> Ꮡaddr, bool handoff, nint skipframes) {
    ref var addr = ref Ꮡaddr.val;

    var root = semtable.rootFor(Ꮡaddr);
    atomic.Xadd(Ꮡaddr, 1);
    // Easy case: no waiters?
    // This check must happen after the xadd, to avoid a missed wakeup
    // (see loop in semacquire).
    if ((~root).nwait.Load() == 0) {
        return;
    }
    // Harder case: search for a waiter and wake it.
    lockWithRank(Ꮡ((~root).@lock), lockRankRoot);
    if ((~root).nwait.Load() == 0) {
        // The count is already consumed by another goroutine,
        // so no need to wake up another goroutine.
        unlock(Ꮡ((~root).@lock));
        return;
    }
    var (s, t0, tailtime) = root.dequeue(Ꮡaddr);
    if (s != nil) {
        (~root).nwait.Add(-1);
    }
    unlock(Ꮡ((~root).@lock));
    if (s != nil) {
        // May be slow or even yield, so unlock first
        var acquiretime = s.val.acquiretime;
        if (acquiretime != 0) {
            // Charge contention that this (delayed) unlock caused.
            // If there are N more goroutines waiting beyond the
            // one that's waking up, charge their delay as well, so that
            // contention holding up many goroutines shows up as
            // more costly than contention holding up a single goroutine.
            // It would take O(N) time to calculate how long each goroutine
            // has been waiting, so instead we charge avg(head-wait, tail-wait)*N.
            // head-wait is the longest wait and tail-wait is the shortest.
            // (When we do a lifo insertion, we preserve this property by
            // copying the old head's acquiretime into the inserted new head.
            // In that case the overall average may be slightly high, but that's fine:
            // the average of the ends is only an approximation to the actual
            // average anyway.)
            // The root.dequeue above changed the head and tail acquiretime
            // to the current time, so the next unlock will not re-count this contention.
            var dt0 = t0 - acquiretime;
            var dt = dt0;
            if ((~s).waiters != 0) {
                var dtail = t0 - tailtime;
                dt += (dtail + dt0) / 2 * ((int64)(~s).waiters);
            }
            mutexevent(dt, 3 + skipframes);
        }
        if ((~s).ticket != 0) {
            @throw("corrupted semaphore ticket"u8);
        }
        if (handoff && cansemacquire(Ꮡaddr)) {
            s.val.ticket = 1;
        }
        readyWithTime(s, 5 + skipframes);
        if ((~s).ticket == 1 && (~(~getg()).m).locks == 0) {
            // Direct G handoff
            // readyWithTime has added the waiter G as runnext in the
            // current P; we now call the scheduler so that we start running
            // the waiter G immediately.
            // Note that waiter inherits our time slice: this is desirable
            // to avoid having a highly contended semaphore hog the P
            // indefinitely. goyield is like Gosched, but it emits a
            // "preempted" trace event instead and, more importantly, puts
            // the current G on the local runq instead of the global one.
            // We only do this in the starving regime (handoff=true), as in
            // the non-starving case it is possible for a different waiter
            // to acquire the semaphore while we are yielding/scheduling,
            // and this would be wasteful. We wait instead to enter starving
            // regime, and then we start to do direct handoffs of ticket and
            // P.
            // See issue 33747 for discussion.
            goyield();
        }
    }
}

internal static bool cansemacquire(ж<uint32> Ꮡaddr) {
    ref var addr = ref Ꮡaddr.val;

    while (ᐧ) {
        var v = atomic.Load(Ꮡaddr);
        if (v == 0) {
            return false;
        }
        if (atomic.Cas(Ꮡaddr, v, v - 1)) {
            return true;
        }
    }
}

// queue adds s to the blocked goroutines in semaRoot.
[GoRecv] internal static void queue(this ref semaRoot root, ж<uint32> Ꮡaddr, ж<sudog> Ꮡs, bool lifo) {
    ref var addr = ref Ꮡaddr.val;
    ref var s = ref Ꮡs.val;

    s.g = getg();
    s.elem = new @unsafe.Pointer(Ꮡaddr);
    s.next = default!;
    s.prev = default!;
    s.waiters = 0;
    ж<sudog> last = default!;
    var pt = Ꮡ(root.treap);
    for (var t = pt.val; t != nil; t = pt.val) {
        if ((~t).elem == new @unsafe.Pointer(Ꮡaddr)) {
            // Already have addr in list.
            if (lifo){
                // Substitute s in t's place in treap.
                pt.val = s;
                s.ticket = t.val.ticket;
                s.acquiretime = t.val.acquiretime;
                // preserve head acquiretime as oldest time
                s.parent = t.val.parent;
                s.prev = t.val.prev;
                s.next = t.val.next;
                if (s.prev != nil) {
                    s.prev.parent = s;
                }
                if (s.next != nil) {
                    s.next.parent = s;
                }
                // Add t first in s's wait list.
                s.waitlink = t;
                s.waittail = t.val.waittail;
                if (s.waittail == nil) {
                    s.waittail = t;
                }
                s.waiters = t.val.waiters;
                if (s.waiters + 1 != 0) {
                    s.waiters++;
                }
                t.val.parent = default!;
                t.val.prev = default!;
                t.val.next = default!;
                t.val.waittail = default!;
            } else {
                // Add s to end of t's wait list.
                if ((~t).waittail == nil){
                    t.val.waitlink = s;
                } else {
                    (~t).waittail.val.waitlink = s;
                }
                t.val.waittail = s;
                s.waitlink = default!;
                if ((~t).waiters + 1 != 0) {
                    (~t).waiters++;
                }
            }
            return;
        }
        last = t;
        if (((uintptr)new @unsafe.Pointer(Ꮡaddr)) < ((uintptr)(~t).elem)){
            pt = Ꮡ((~t).prev);
        } else {
            pt = Ꮡ((~t).next);
        }
    }
    // Add s as new leaf in tree of unique addrs.
    // The balanced tree is a treap using ticket as the random heap priority.
    // That is, it is a binary tree ordered according to the elem addresses,
    // but then among the space of possible binary trees respecting those
    // addresses, it is kept balanced on average by maintaining a heap ordering
    // on the ticket: s.ticket <= both s.prev.ticket and s.next.ticket.
    // https://en.wikipedia.org/wiki/Treap
    // https://faculty.washington.edu/aragon/pubs/rst89.pdf
    //
    // s.ticket compared with zero in couple of places, therefore set lowest bit.
    // It will not affect treap's quality noticeably.
    s.ticket = (uint32)(cheaprand() | 1);
    s.parent = last;
    pt.val = s;
    // Rotate up into tree according to ticket (priority).
    while (s.parent != nil && s.parent.ticket > s.ticket) {
        if (s.parent.prev == Ꮡs){
            root.rotateRight(s.parent);
        } else {
            if (s.parent.next != Ꮡs) {
                throw panic("semaRoot queue");
            }
            root.rotateLeft(s.parent);
        }
    }
}

// dequeue searches for and finds the first goroutine
// in semaRoot blocked on addr.
// If the sudog was being profiled, dequeue returns the time
// at which it was woken up as now. Otherwise now is 0.
// If there are additional entries in the wait list, dequeue
// returns tailtime set to the last entry's acquiretime.
// Otherwise tailtime is found.acquiretime.
[GoRecv] internal static (ж<sudog> found, int64 now, int64 tailtime) dequeue(this ref semaRoot root, ж<uint32> Ꮡaddr) {
    ж<sudog> found = default!;
    int64 now = default!;
    int64 tailtime = default!;

    ref var addr = ref Ꮡaddr.val;
    var ps = Ꮡ(root.treap);
    var s = ps.val;
    for (; s != nil; s = ps.val) {
        if ((~s).elem == new @unsafe.Pointer(Ꮡaddr)) {
            goto Found;
        }
        if (((uintptr)new @unsafe.Pointer(Ꮡaddr)) < ((uintptr)(~s).elem)){
            ps = Ꮡ((~s).prev);
        } else {
            ps = Ꮡ((~s).next);
        }
    }
    return (default!, 0, 0);
Found:
    now = ((int64)0);
    if ((~s).acquiretime != 0) {
        now = cputicks();
    }
    {
        var t = s.val.waitlink; if (t != nil){
            // Substitute t, also waiting on addr, for s in root tree of unique addrs.
            ps.val = t;
            t.val.ticket = s.val.ticket;
            t.val.parent = s.val.parent;
            t.val.prev = s.val.prev;
            if ((~t).prev != nil) {
                (~t).prev.val.parent = t;
            }
            t.val.next = s.val.next;
            if ((~t).next != nil) {
                (~t).next.val.parent = t;
            }
            if ((~t).waitlink != nil){
                t.val.waittail = s.val.waittail;
            } else {
                t.val.waittail = default!;
            }
            t.val.waiters = s.val.waiters;
            if ((~t).waiters > 1) {
                (~t).waiters--;
            }
            // Set head and tail acquire time to 'now',
            // because the caller will take care of charging
            // the delays before now for all entries in the list.
            t.val.acquiretime = now;
            tailtime = (~s).waittail.val.acquiretime;
            (~s).waittail.val.acquiretime = now;
            s.val.waitlink = default!;
            s.val.waittail = default!;
        } else {
            // Rotate s down to be leaf of tree for removal, respecting priorities.
            while ((~s).next != nil || (~s).prev != nil) {
                if ((~s).next == nil || (~s).prev != nil && (~(~s).prev).ticket < (~(~s).next).ticket){
                    root.rotateRight(s);
                } else {
                    root.rotateLeft(s);
                }
            }
            // Remove s, now a leaf.
            if ((~s).parent != nil){
                if ((~(~s).parent).prev == s){
                    (~s).parent.val.prev = default!;
                } else {
                    (~s).parent.val.next = default!;
                }
            } else {
                root.treap = default!;
            }
            tailtime = s.val.acquiretime;
        }
    }
    s.val.parent = default!;
    s.val.elem = default!;
    s.val.next = default!;
    s.val.prev = default!;
    s.val.ticket = 0;
    return (s, now, tailtime);
}

// rotateLeft rotates the tree rooted at node x.
// turning (x a (y b c)) into (y (x a b) c).
[GoRecv] internal static void rotateLeft(this ref semaRoot root, ж<sudog> Ꮡx) {
    ref var x = ref Ꮡx.val;

    // p -> (x a (y b c))
    var Δp = x.parent;
    var y = x.next;
    var b = y.val.prev;
    y.val.prev = x;
    x.parent = y;
    x.next = b;
    if (b != nil) {
        b.val.parent = x;
    }
    y.val.parent = Δp;
    if (Δp == nil){
        root.treap = y;
    } else 
    if ((~Δp).prev == Ꮡx){
        Δp.val.prev = y;
    } else {
        if ((~Δp).next != Ꮡx) {
            @throw("semaRoot rotateLeft"u8);
        }
        Δp.val.next = y;
    }
}

// rotateRight rotates the tree rooted at node y.
// turning (y (x a b) c) into (x a (y b c)).
[GoRecv] internal static void rotateRight(this ref semaRoot root, ж<sudog> Ꮡy) {
    ref var y = ref Ꮡy.val;

    // p -> (y (x a b) c)
    var Δp = y.parent;
    var x = y.prev;
    var b = x.val.next;
    x.val.next = y;
    y.parent = x;
    y.prev = b;
    if (b != nil) {
        b.val.parent = y;
    }
    x.val.parent = Δp;
    if (Δp == nil){
        root.treap = x;
    } else 
    if ((~Δp).prev == Ꮡy){
        Δp.val.prev = x;
    } else {
        if ((~Δp).next != Ꮡy) {
            @throw("semaRoot rotateRight"u8);
        }
        Δp.val.next = x;
    }
}

// notifyList is a ticket-based notification list used to implement sync.Cond.
//
// It must be kept in sync with the sync package.
[GoType] partial struct notifyList {
    // wait is the ticket number of the next waiter. It is atomically
    // incremented outside the lock.
    internal @internal.runtime.atomic_package.Uint32 wait;
    // notify is the ticket number of the next waiter to be notified. It can
    // be read outside the lock, but is only written to with lock held.
    //
    // Both wait & notify can wrap around, and such cases will be correctly
    // handled as long as their "unwrapped" difference is bounded by 2^31.
    // For this not to be the case, we'd need to have 2^31+ goroutines
    // blocked on the same condvar, which is currently not possible.
    internal uint32 notify;
    // List of parked waiters.
    internal mutex @lock;
    internal ж<sudog> head;
    internal ж<sudog> tail;
}

// less checks if a < b, considering a & b running counts that may overflow the
// 32-bit range, and that their "unwrapped" difference is always less than 2^31.
internal static bool less(uint32 a, uint32 b) {
    return ((int32)(a - b)) < 0;
}

// notifyListAdd adds the caller to a notify list such that it can receive
// notifications. The caller must eventually call notifyListWait to wait for
// such a notification, passing the returned ticket number.
//
//go:linkname notifyListAdd sync.runtime_notifyListAdd
internal static uint32 notifyListAdd(ж<notifyList> Ꮡl) {
    ref var l = ref Ꮡl.val;

    // This may be called concurrently, for example, when called from
    // sync.Cond.Wait while holding a RWMutex in read mode.
    return l.wait.Add(1) - 1;
}

// notifyListWait waits for a notification. If one has been sent since
// notifyListAdd was called, it returns immediately. Otherwise, it blocks.
//
//go:linkname notifyListWait sync.runtime_notifyListWait
internal static void notifyListWait(ж<notifyList> Ꮡl, uint32 t) {
    ref var l = ref Ꮡl.val;

    lockWithRank(Ꮡ(l.@lock), lockRankNotifyList);
    // Return right away if this ticket has already been notified.
    if (less(t, l.notify)) {
        unlock(Ꮡ(l.@lock));
        return;
    }
    // Enqueue itself.
    var s = acquireSudog();
    s.val.g = getg();
    s.val.ticket = t;
    s.val.releasetime = 0;
    var t0 = ((int64)0);
    if (blockprofilerate > 0) {
        t0 = cputicks();
        s.val.releasetime = -1;
    }
    if (l.tail == nil){
        l.head = s;
    } else {
        l.tail.next = s;
    }
    l.tail = s;
    goparkunlock(Ꮡ(l.@lock), waitReasonSyncCondWait, traceBlockCondWait, 3);
    if (t0 != 0) {
        blockevent((~s).releasetime - t0, 2);
    }
    releaseSudog(s);
}

// notifyListNotifyAll notifies all entries in the list.
//
//go:linkname notifyListNotifyAll sync.runtime_notifyListNotifyAll
internal static void notifyListNotifyAll(ж<notifyList> Ꮡl) {
    ref var l = ref Ꮡl.val;

    // Fast-path: if there are no new waiters since the last notification
    // we don't need to acquire the lock.
    if (l.wait.Load() == atomic.Load(Ꮡ(l.notify))) {
        return;
    }
    // Pull the list out into a local variable, waiters will be readied
    // outside the lock.
    lockWithRank(Ꮡ(l.@lock), lockRankNotifyList);
    var s = l.head;
    l.head = default!;
    l.tail = default!;
    // Update the next ticket to be notified. We can set it to the current
    // value of wait because any previous waiters are already in the list
    // or will notice that they have already been notified when trying to
    // add themselves to the list.
    atomic.Store(Ꮡ(l.notify), l.wait.Load());
    unlock(Ꮡ(l.@lock));
    // Go through the local list and ready all waiters.
    while (s != nil) {
        var next = s.val.next;
        s.val.next = default!;
        readyWithTime(s, 4);
        s = next;
    }
}

// notifyListNotifyOne notifies one entry in the list.
//
//go:linkname notifyListNotifyOne sync.runtime_notifyListNotifyOne
internal static void notifyListNotifyOne(ж<notifyList> Ꮡl) {
    ref var l = ref Ꮡl.val;

    // Fast-path: if there are no new waiters since the last notification
    // we don't need to acquire the lock at all.
    if (l.wait.Load() == atomic.Load(Ꮡ(l.notify))) {
        return;
    }
    lockWithRank(Ꮡ(l.@lock), lockRankNotifyList);
    // Re-check under the lock if we need to do anything.
    var t = l.notify;
    if (t == l.wait.Load()) {
        unlock(Ꮡ(l.@lock));
        return;
    }
    // Update the next notify ticket number.
    atomic.Store(Ꮡ(l.notify), t + 1);
    // Try to find the g that needs to be notified.
    // If it hasn't made it to the list yet we won't find it,
    // but it won't park itself once it sees the new notify number.
    //
    // This scan looks linear but essentially always stops quickly.
    // Because g's queue separately from taking numbers,
    // there may be minor reorderings in the list, but we
    // expect the g we're looking for to be near the front.
    // The g has others in front of it on the list only to the
    // extent that it lost the race, so the iteration will not
    // be too long. This applies even when the g is missing:
    // it hasn't yet gotten to sleep and has lost the race to
    // the (few) other g's that we find on the list.
    for (var Δp = (ж<sudog>)(default!);var s = l.head; s != nil; (Δp, s) = (s, s.val.next)) {
        if ((~s).ticket == t) {
            var n = s.val.next;
            if (Δp != nil){
                Δp.val.next = n;
            } else {
                l.head = n;
            }
            if (n == nil) {
                l.tail = Δp;
            }
            unlock(Ꮡ(l.@lock));
            s.val.next = default!;
            readyWithTime(s, 4);
            return;
        }
    }
    unlock(Ꮡ(l.@lock));
}

//go:linkname notifyListCheck sync.runtime_notifyListCheck
internal static void notifyListCheck(uintptr sz) {
    if (sz != @unsafe.Sizeof(new notifyList(nil))) {
        print("runtime: bad notifyList size - sync=", sz, " runtime=", @unsafe.Sizeof(new notifyList(nil)), "\n");
        @throw("bad notifyList size"u8);
    }
}

//go:linkname sync_nanotime sync.runtime_nanotime
internal static int64 sync_nanotime() {
    return nanotime();
}

} // end runtime_package
