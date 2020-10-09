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

// package runtime -- go2cs converted at 2020 October 09 04:48:12 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\sema.go
using cpu = go.@internal.cpu_package;
using atomic = go.runtime.@internal.atomic_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        // Asynchronous semaphore for sync.Mutex.

        // A semaRoot holds a balanced tree of sudog with distinct addresses (s.elem).
        // Each of those sudog may in turn point (through s.waitlink) to a list
        // of other sudogs waiting on the same address.
        // The operations on the inner lists of sudogs with the same address
        // are all O(1). The scanning of the top-level semaRoot list is O(log n),
        // where n is the number of distinct addresses with goroutines blocked
        // on them that hash to the given semaRoot.
        // See golang.org/issue/17953 for a program that worked badly
        // before we introduced the second level of list, and test/locklinear.go
        // for a test that exercises this.
        private partial struct semaRoot
        {
            public mutex @lock;
            public ptr<sudog> treap; // root of balanced tree of unique waiters.
            public uint nwait; // Number of waiters. Read w/o the lock.
        }

        // Prime to not correlate with any user patterns.
        private static readonly long semTabSize = (long)251L;



        private static var semtable = default;

        //go:linkname sync_runtime_Semacquire sync.runtime_Semacquire
        private static void sync_runtime_Semacquire(ptr<uint> _addr_addr)
        {
            ref uint addr = ref _addr_addr.val;

            semacquire1(_addr_addr, false, semaBlockProfile, 0L);
        }

        //go:linkname poll_runtime_Semacquire internal/poll.runtime_Semacquire
        private static void poll_runtime_Semacquire(ptr<uint> _addr_addr)
        {
            ref uint addr = ref _addr_addr.val;

            semacquire1(_addr_addr, false, semaBlockProfile, 0L);
        }

        //go:linkname sync_runtime_Semrelease sync.runtime_Semrelease
        private static void sync_runtime_Semrelease(ptr<uint> _addr_addr, bool handoff, long skipframes)
        {
            ref uint addr = ref _addr_addr.val;

            semrelease1(_addr_addr, handoff, skipframes);
        }

        //go:linkname sync_runtime_SemacquireMutex sync.runtime_SemacquireMutex
        private static void sync_runtime_SemacquireMutex(ptr<uint> _addr_addr, bool lifo, long skipframes)
        {
            ref uint addr = ref _addr_addr.val;

            semacquire1(_addr_addr, lifo, semaBlockProfile | semaMutexProfile, skipframes);
        }

        //go:linkname poll_runtime_Semrelease internal/poll.runtime_Semrelease
        private static void poll_runtime_Semrelease(ptr<uint> _addr_addr)
        {
            ref uint addr = ref _addr_addr.val;

            semrelease(_addr_addr);
        }

        private static void readyWithTime(ptr<sudog> _addr_s, long traceskip)
        {
            ref sudog s = ref _addr_s.val;

            if (s.releasetime != 0L)
            {
                s.releasetime = cputicks();
            }

            goready(s.g, traceskip);

        }

        private partial struct semaProfileFlags // : long
        {
        }

        private static readonly semaProfileFlags semaBlockProfile = (semaProfileFlags)1L << (int)(iota);
        private static readonly var semaMutexProfile = 0;


        // Called from runtime.
        private static void semacquire(ptr<uint> _addr_addr)
        {
            ref uint addr = ref _addr_addr.val;

            semacquire1(_addr_addr, false, 0L, 0L);
        }

        private static void semacquire1(ptr<uint> _addr_addr, bool lifo, semaProfileFlags profile, long skipframes)
        {
            ref uint addr = ref _addr_addr.val;

            var gp = getg();
            if (gp != gp.m.curg)
            {
                throw("semacquire not on the G stack");
            } 

            // Easy case.
            if (cansemacquire(_addr_addr))
            {
                return ;
            } 

            // Harder case:
            //    increment waiter count
            //    try cansemacquire one more time, return if succeeded
            //    enqueue itself as a waiter
            //    sleep
            //    (waiter descriptor is dequeued by signaler)
            var s = acquireSudog();
            var root = semroot(_addr_addr);
            var t0 = int64(0L);
            s.releasetime = 0L;
            s.acquiretime = 0L;
            s.ticket = 0L;
            if (profile & semaBlockProfile != 0L && blockprofilerate > 0L)
            {
                t0 = cputicks();
                s.releasetime = -1L;
            }

            if (profile & semaMutexProfile != 0L && mutexprofilerate > 0L)
            {
                if (t0 == 0L)
                {
                    t0 = cputicks();
                }

                s.acquiretime = t0;

            }

            while (true)
            {
                lockWithRank(_addr_root.@lock, lockRankRoot); 
                // Add ourselves to nwait to disable "easy case" in semrelease.
                atomic.Xadd(_addr_root.nwait, 1L); 
                // Check cansemacquire to avoid missed wakeup.
                if (cansemacquire(_addr_addr))
                {
                    atomic.Xadd(_addr_root.nwait, -1L);
                    unlock(_addr_root.@lock);
                    break;
                } 
                // Any semrelease after the cansemacquire knows we're waiting
                // (we set nwait above), so go to sleep.
                root.queue(addr, s, lifo);
                goparkunlock(_addr_root.@lock, waitReasonSemacquire, traceEvGoBlockSync, 4L + skipframes);
                if (s.ticket != 0L || cansemacquire(_addr_addr))
                {
                    break;
                }

            }

            if (s.releasetime > 0L)
            {
                blockevent(s.releasetime - t0, 3L + skipframes);
            }

            releaseSudog(s);

        }

        private static void semrelease(ptr<uint> _addr_addr)
        {
            ref uint addr = ref _addr_addr.val;

            semrelease1(_addr_addr, false, 0L);
        }

        private static void semrelease1(ptr<uint> _addr_addr, bool handoff, long skipframes)
        {
            ref uint addr = ref _addr_addr.val;

            var root = semroot(_addr_addr);
            atomic.Xadd(addr, 1L); 

            // Easy case: no waiters?
            // This check must happen after the xadd, to avoid a missed wakeup
            // (see loop in semacquire).
            if (atomic.Load(_addr_root.nwait) == 0L)
            {
                return ;
            } 

            // Harder case: search for a waiter and wake it.
            lockWithRank(_addr_root.@lock, lockRankRoot);
            if (atomic.Load(_addr_root.nwait) == 0L)
            { 
                // The count is already consumed by another goroutine,
                // so no need to wake up another goroutine.
                unlock(_addr_root.@lock);
                return ;

            }

            var (s, t0) = root.dequeue(addr);
            if (s != null)
            {
                atomic.Xadd(_addr_root.nwait, -1L);
            }

            unlock(_addr_root.@lock);
            if (s != null)
            { // May be slow or even yield, so unlock first
                var acquiretime = s.acquiretime;
                if (acquiretime != 0L)
                {
                    mutexevent(t0 - acquiretime, 3L + skipframes);
                }

                if (s.ticket != 0L)
                {
                    throw("corrupted semaphore ticket");
                }

                if (handoff && cansemacquire(_addr_addr))
                {
                    s.ticket = 1L;
                }

                readyWithTime(_addr_s, 5L + skipframes);
                if (s.ticket == 1L && getg().m.locks == 0L)
                { 
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

        private static ptr<semaRoot> semroot(ptr<uint> _addr_addr)
        {
            ref uint addr = ref _addr_addr.val;

            return _addr__addr_semtable[(uintptr(@unsafe.Pointer(addr)) >> (int)(3L)) % semTabSize].root!;
        }

        private static bool cansemacquire(ptr<uint> _addr_addr)
        {
            ref uint addr = ref _addr_addr.val;

            while (true)
            {
                var v = atomic.Load(addr);
                if (v == 0L)
                {
                    return false;
                }

                if (atomic.Cas(addr, v, v - 1L))
                {
                    return true;
                }

            }


        }

        // queue adds s to the blocked goroutines in semaRoot.
        private static void queue(this ptr<semaRoot> _addr_root, ptr<uint> _addr_addr, ptr<sudog> _addr_s, bool lifo) => func((_, panic, __) =>
        {
            ref semaRoot root = ref _addr_root.val;
            ref uint addr = ref _addr_addr.val;
            ref sudog s = ref _addr_s.val;

            s.g = getg();
            s.elem = @unsafe.Pointer(addr);
            s.next = null;
            s.prev = null;

            ptr<sudog> last;
            var pt = _addr_root.treap;
            {
                var t = pt.val;

                while (t != null)
                {
                    if (t.elem == @unsafe.Pointer(addr))
                    { 
                        // Already have addr in list.
                        if (lifo)
                        { 
                            // Substitute s in t's place in treap.
                            pt.val = s;
                            s.ticket = t.ticket;
                            s.acquiretime = t.acquiretime;
                            s.parent = t.parent;
                            s.prev = t.prev;
                            s.next = t.next;
                            if (s.prev != null)
                            {
                                s.prev.parent = s;
                    t = pt.val;
                            }

                            if (s.next != null)
                            {
                                s.next.parent = s;
                            } 
                            // Add t first in s's wait list.
                            s.waitlink = t;
                            s.waittail = t.waittail;
                            if (s.waittail == null)
                            {
                                s.waittail = t;
                            }

                            t.parent = null;
                            t.prev = null;
                            t.next = null;
                            t.waittail = null;

                        }
                        else
                        { 
                            // Add s to end of t's wait list.
                            if (t.waittail == null)
                            {
                                t.waitlink = s;
                            }
                            else
                            {
                                t.waittail.waitlink = s;
                            }

                            t.waittail = s;
                            s.waitlink = null;

                        }

                        return ;

                    }

                    last = t;
                    if (uintptr(@unsafe.Pointer(addr)) < uintptr(t.elem))
                    {
                        pt = _addr_t.prev;
                    }
                    else
                    {
                        pt = _addr_t.next;
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
            s.ticket = fastrand() | 1L;
            s.parent = last;
            pt.val = s; 

            // Rotate up into tree according to ticket (priority).
            while (s.parent != null && s.parent.ticket > s.ticket)
            {
                if (s.parent.prev == s)
                {
                    root.rotateRight(s.parent);
                }
                else
                {
                    if (s.parent.next != s)
                    {
                        panic("semaRoot queue");
                    }

                    root.rotateLeft(s.parent);

                }

            }


        });

        // dequeue searches for and finds the first goroutine
        // in semaRoot blocked on addr.
        // If the sudog was being profiled, dequeue returns the time
        // at which it was woken up as now. Otherwise now is 0.
        private static (ptr<sudog>, long) dequeue(this ptr<semaRoot> _addr_root, ptr<uint> _addr_addr)
        {
            ptr<sudog> found = default!;
            long now = default;
            ref semaRoot root = ref _addr_root.val;
            ref uint addr = ref _addr_addr.val;

            var ps = _addr_root.treap;
            var s = ps.val;
            while (s != null)
            {
                if (s.elem == @unsafe.Pointer(addr))
                {
                    goto Found;
                s = ps.val;
                }

                if (uintptr(@unsafe.Pointer(addr)) < uintptr(s.elem))
                {
                    ps = _addr_s.prev;
                }
                else
                {
                    ps = _addr_s.next;
                }

            }

            return (_addr_null!, 0L);

Found:
            now = int64(0L);
            if (s.acquiretime != 0L)
            {
                now = cputicks();
            }

            {
                var t = s.waitlink;

                if (t != null)
                { 
                    // Substitute t, also waiting on addr, for s in root tree of unique addrs.
                    ps.val = t;
                    t.ticket = s.ticket;
                    t.parent = s.parent;
                    t.prev = s.prev;
                    if (t.prev != null)
                    {
                        t.prev.parent = t;
                    }

                    t.next = s.next;
                    if (t.next != null)
                    {
                        t.next.parent = t;
                    }

                    if (t.waitlink != null)
                    {
                        t.waittail = s.waittail;
                    }
                    else
                    {
                        t.waittail = null;
                    }

                    t.acquiretime = now;
                    s.waitlink = null;
                    s.waittail = null;

                }
                else
                { 
                    // Rotate s down to be leaf of tree for removal, respecting priorities.
                    while (s.next != null || s.prev != null)
                    {
                        if (s.next == null || s.prev != null && s.prev.ticket < s.next.ticket)
                        {
                            root.rotateRight(s);
                        }
                        else
                        {
                            root.rotateLeft(s);
                        }

                    } 
                    // Remove s, now a leaf.
 
                    // Remove s, now a leaf.
                    if (s.parent != null)
                    {
                        if (s.parent.prev == s)
                        {
                            s.parent.prev = null;
                        }
                        else
                        {
                            s.parent.next = null;
                        }

                    }
                    else
                    {
                        root.treap = null;
                    }

                }

            }

            s.parent = null;
            s.elem = null;
            s.next = null;
            s.prev = null;
            s.ticket = 0L;
            return (_addr_s!, now);

        }

        // rotateLeft rotates the tree rooted at node x.
        // turning (x a (y b c)) into (y (x a b) c).
        private static void rotateLeft(this ptr<semaRoot> _addr_root, ptr<sudog> _addr_x)
        {
            ref semaRoot root = ref _addr_root.val;
            ref sudog x = ref _addr_x.val;
 
            // p -> (x a (y b c))
            var p = x.parent;
            var y = x.next;
            var b = y.prev;

            y.prev = x;
            x.parent = y;
            x.next = b;
            if (b != null)
            {
                b.parent = x;
            }

            y.parent = p;
            if (p == null)
            {
                root.treap = y;
            }
            else if (p.prev == x)
            {
                p.prev = y;
            }
            else
            {
                if (p.next != x)
                {
                    throw("semaRoot rotateLeft");
                }

                p.next = y;

            }

        }

        // rotateRight rotates the tree rooted at node y.
        // turning (y (x a b) c) into (x a (y b c)).
        private static void rotateRight(this ptr<semaRoot> _addr_root, ptr<sudog> _addr_y)
        {
            ref semaRoot root = ref _addr_root.val;
            ref sudog y = ref _addr_y.val;
 
            // p -> (y (x a b) c)
            var p = y.parent;
            var x = y.prev;
            var b = x.next;

            x.next = y;
            y.parent = x;
            y.prev = b;
            if (b != null)
            {
                b.parent = y;
            }

            x.parent = p;
            if (p == null)
            {
                root.treap = x;
            }
            else if (p.prev == y)
            {
                p.prev = x;
            }
            else
            {
                if (p.next != y)
                {
                    throw("semaRoot rotateRight");
                }

                p.next = x;

            }

        }

        // notifyList is a ticket-based notification list used to implement sync.Cond.
        //
        // It must be kept in sync with the sync package.
        private partial struct notifyList
        {
            public uint wait; // notify is the ticket number of the next waiter to be notified. It can
// be read outside the lock, but is only written to with lock held.
//
// Both wait & notify can wrap around, and such cases will be correctly
// handled as long as their "unwrapped" difference is bounded by 2^31.
// For this not to be the case, we'd need to have 2^31+ goroutines
// blocked on the same condvar, which is currently not possible.
            public uint notify; // List of parked waiters.
            public mutex @lock;
            public ptr<sudog> head;
            public ptr<sudog> tail;
        }

        // less checks if a < b, considering a & b running counts that may overflow the
        // 32-bit range, and that their "unwrapped" difference is always less than 2^31.
        private static bool less(uint a, uint b)
        {
            return int32(a - b) < 0L;
        }

        // notifyListAdd adds the caller to a notify list such that it can receive
        // notifications. The caller must eventually call notifyListWait to wait for
        // such a notification, passing the returned ticket number.
        //go:linkname notifyListAdd sync.runtime_notifyListAdd
        private static uint notifyListAdd(ptr<notifyList> _addr_l)
        {
            ref notifyList l = ref _addr_l.val;
 
            // This may be called concurrently, for example, when called from
            // sync.Cond.Wait while holding a RWMutex in read mode.
            return atomic.Xadd(_addr_l.wait, 1L) - 1L;

        }

        // notifyListWait waits for a notification. If one has been sent since
        // notifyListAdd was called, it returns immediately. Otherwise, it blocks.
        //go:linkname notifyListWait sync.runtime_notifyListWait
        private static void notifyListWait(ptr<notifyList> _addr_l, uint t)
        {
            ref notifyList l = ref _addr_l.val;

            lockWithRank(_addr_l.@lock, lockRankNotifyList); 

            // Return right away if this ticket has already been notified.
            if (less(t, l.notify))
            {
                unlock(_addr_l.@lock);
                return ;
            } 

            // Enqueue itself.
            var s = acquireSudog();
            s.g = getg();
            s.ticket = t;
            s.releasetime = 0L;
            var t0 = int64(0L);
            if (blockprofilerate > 0L)
            {
                t0 = cputicks();
                s.releasetime = -1L;
            }

            if (l.tail == null)
            {
                l.head = s;
            }
            else
            {
                l.tail.next = s;
            }

            l.tail = s;
            goparkunlock(_addr_l.@lock, waitReasonSyncCondWait, traceEvGoBlockCond, 3L);
            if (t0 != 0L)
            {
                blockevent(s.releasetime - t0, 2L);
            }

            releaseSudog(s);

        }

        // notifyListNotifyAll notifies all entries in the list.
        //go:linkname notifyListNotifyAll sync.runtime_notifyListNotifyAll
        private static void notifyListNotifyAll(ptr<notifyList> _addr_l)
        {
            ref notifyList l = ref _addr_l.val;
 
            // Fast-path: if there are no new waiters since the last notification
            // we don't need to acquire the lock.
            if (atomic.Load(_addr_l.wait) == atomic.Load(_addr_l.notify))
            {
                return ;
            } 

            // Pull the list out into a local variable, waiters will be readied
            // outside the lock.
            lockWithRank(_addr_l.@lock, lockRankNotifyList);
            var s = l.head;
            l.head = null;
            l.tail = null; 

            // Update the next ticket to be notified. We can set it to the current
            // value of wait because any previous waiters are already in the list
            // or will notice that they have already been notified when trying to
            // add themselves to the list.
            atomic.Store(_addr_l.notify, atomic.Load(_addr_l.wait));
            unlock(_addr_l.@lock); 

            // Go through the local list and ready all waiters.
            while (s != null)
            {
                var next = s.next;
                s.next = null;
                readyWithTime(_addr_s, 4L);
                s = next;
            }


        }

        // notifyListNotifyOne notifies one entry in the list.
        //go:linkname notifyListNotifyOne sync.runtime_notifyListNotifyOne
        private static void notifyListNotifyOne(ptr<notifyList> _addr_l)
        {
            ref notifyList l = ref _addr_l.val;
 
            // Fast-path: if there are no new waiters since the last notification
            // we don't need to acquire the lock at all.
            if (atomic.Load(_addr_l.wait) == atomic.Load(_addr_l.notify))
            {
                return ;
            }

            lockWithRank(_addr_l.@lock, lockRankNotifyList); 

            // Re-check under the lock if we need to do anything.
            var t = l.notify;
            if (t == atomic.Load(_addr_l.wait))
            {
                unlock(_addr_l.@lock);
                return ;
            } 

            // Update the next notify ticket number.
            atomic.Store(_addr_l.notify, t + 1L); 

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
            {
                var p = (sudog.val)(null);
                var s = l.head;

                while (s != null)
                {
                    if (s.ticket == t)
                    {
                        var n = s.next;
                        if (p != null)
                        {
                            p.next = n;
                    p = s;
                s = s.next;
                        }
                        else
                        {
                            l.head = n;
                        }

                        if (n == null)
                        {
                            l.tail = p;
                        }

                        unlock(_addr_l.@lock);
                        s.next = null;
                        readyWithTime(_addr_s, 4L);
                        return ;

                    }

                }

            }
            unlock(_addr_l.@lock);

        }

        //go:linkname notifyListCheck sync.runtime_notifyListCheck
        private static void notifyListCheck(System.UIntPtr sz)
        {
            if (sz != @unsafe.Sizeof(new notifyList()))
            {
                print("runtime: bad notifyList size - sync=", sz, " runtime=", @unsafe.Sizeof(new notifyList()), "\n");
                throw("bad notifyList size");
            }

        }

        //go:linkname sync_nanotime sync.runtime_nanotime
        private static long sync_nanotime()
        {
            return nanotime();
        }
    }
}
