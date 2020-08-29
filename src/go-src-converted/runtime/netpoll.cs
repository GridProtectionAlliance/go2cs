// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin dragonfly freebsd linux nacl netbsd openbsd solaris windows

// package runtime -- go2cs converted at 2020 August 29 08:18:35 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\netpoll.go
using atomic = go.runtime.@internal.atomic_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        // Integrated network poller (platform-independent part).
        // A particular implementation (epoll/kqueue) must define the following functions:
        // func netpollinit()            // to initialize the poller
        // func netpollopen(fd uintptr, pd *pollDesc) int32    // to arm edge-triggered notifications
        // and associate fd with pd.
        // An implementation must call the following function to denote that the pd is ready.
        // func netpollready(gpp **g, pd *pollDesc, mode int32)

        // pollDesc contains 2 binary semaphores, rg and wg, to park reader and writer
        // goroutines respectively. The semaphore can be in the following states:
        // pdReady - io readiness notification is pending;
        //           a goroutine consumes the notification by changing the state to nil.
        // pdWait - a goroutine prepares to park on the semaphore, but not yet parked;
        //          the goroutine commits to park by changing the state to G pointer,
        //          or, alternatively, concurrent io notification changes the state to READY,
        //          or, alternatively, concurrent timeout/close changes the state to nil.
        // G pointer - the goroutine is blocked on the semaphore;
        //             io notification or timeout/close changes the state to READY or nil respectively
        //             and unparks the goroutine.
        // nil - nothing of the above.
        private static readonly System.UIntPtr pdReady = 1L;
        private static readonly System.UIntPtr pdWait = 2L;

        private static readonly long pollBlockSize = 4L * 1024L;

        // Network poller descriptor.
        //
        // No heap pointers.
        //
        //go:notinheap


        // Network poller descriptor.
        //
        // No heap pointers.
        //
        //go:notinheap
        private partial struct pollDesc
        {
            public ptr<pollDesc> link; // in pollcache, protected by pollcache.lock

// The lock protects pollOpen, pollSetDeadline, pollUnblock and deadlineimpl operations.
// This fully covers seq, rt and wt variables. fd is constant throughout the PollDesc lifetime.
// pollReset, pollWait, pollWaitCanceled and runtime·netpollready (IO readiness notification)
// proceed w/o taking the lock. So closing, rg, rd, wg and wd are manipulated
// in a lock-free way by all operations.
// NOTE(dvyukov): the following code uses uintptr to store *g (rg/wg),
// that will blow up when GC starts moving objects.
            public mutex @lock; // protects the following fields
            public System.UIntPtr fd;
            public bool closing;
            public System.UIntPtr seq; // protects from stale timers and ready notifications
            public System.UIntPtr rg; // pdReady, pdWait, G waiting for read or nil
            public timer rt; // read deadline timer (set if rt.f != nil)
            public long rd; // read deadline
            public System.UIntPtr wg; // pdReady, pdWait, G waiting for write or nil
            public timer wt; // write deadline timer
            public long wd; // write deadline
            public uint user; // user settable cookie
        }

        private partial struct pollCache
        {
            public mutex @lock;
            public ptr<pollDesc> first; // PollDesc objects must be type-stable,
// because we can get ready notification from epoll/kqueue
// after the descriptor is closed/reused.
// Stale notifications are detected using seq variable,
// seq is incremented when deadlines are changed or descriptor is reused.
        }

        private static uint netpollInited = default;        private static pollCache pollcache = default;        private static uint netpollWaiters = default;

        //go:linkname poll_runtime_pollServerInit internal/poll.runtime_pollServerInit
        private static void poll_runtime_pollServerInit()
        {
            netpollinit();
            atomic.Store(ref netpollInited, 1L);
        }

        private static bool netpollinited()
        {
            return atomic.Load(ref netpollInited) != 0L;
        }

        //go:linkname poll_runtime_pollServerDescriptor internal/poll.runtime_pollServerDescriptor

        // poll_runtime_pollServerDescriptor returns the descriptor being used,
        // or ^uintptr(0) if the system does not use a poll descriptor.
        private static System.UIntPtr poll_runtime_pollServerDescriptor()
        {
            return netpolldescriptor();
        }

        //go:linkname poll_runtime_pollOpen internal/poll.runtime_pollOpen
        private static (ref pollDesc, long) poll_runtime_pollOpen(System.UIntPtr fd)
        {
            var pd = pollcache.alloc();
            lock(ref pd.@lock);
            if (pd.wg != 0L && pd.wg != pdReady)
            {
                throw("runtime: blocked write on free polldesc");
            }
            if (pd.rg != 0L && pd.rg != pdReady)
            {
                throw("runtime: blocked read on free polldesc");
            }
            pd.fd = fd;
            pd.closing = false;
            pd.seq++;
            pd.rg = 0L;
            pd.rd = 0L;
            pd.wg = 0L;
            pd.wd = 0L;
            unlock(ref pd.@lock);

            int errno = default;
            errno = netpollopen(fd, pd);
            return (pd, int(errno));
        }

        //go:linkname poll_runtime_pollClose internal/poll.runtime_pollClose
        private static void poll_runtime_pollClose(ref pollDesc pd)
        {
            if (!pd.closing)
            {
                throw("runtime: close polldesc w/o unblock");
            }
            if (pd.wg != 0L && pd.wg != pdReady)
            {
                throw("runtime: blocked write on closing polldesc");
            }
            if (pd.rg != 0L && pd.rg != pdReady)
            {
                throw("runtime: blocked read on closing polldesc");
            }
            netpollclose(pd.fd);
            pollcache.free(pd);
        }

        private static void free(this ref pollCache c, ref pollDesc pd)
        {
            lock(ref c.@lock);
            pd.link = c.first;
            c.first = pd;
            unlock(ref c.@lock);
        }

        //go:linkname poll_runtime_pollReset internal/poll.runtime_pollReset
        private static long poll_runtime_pollReset(ref pollDesc pd, long mode)
        {
            var err = netpollcheckerr(pd, int32(mode));
            if (err != 0L)
            {
                return err;
            }
            if (mode == 'r')
            {
                pd.rg = 0L;
            }
            else if (mode == 'w')
            {
                pd.wg = 0L;
            }
            return 0L;
        }

        //go:linkname poll_runtime_pollWait internal/poll.runtime_pollWait
        private static long poll_runtime_pollWait(ref pollDesc pd, long mode)
        {
            var err = netpollcheckerr(pd, int32(mode));
            if (err != 0L)
            {
                return err;
            } 
            // As for now only Solaris uses level-triggered IO.
            if (GOOS == "solaris")
            {
                netpollarm(pd, mode);
            }
            while (!netpollblock(pd, int32(mode), false))
            {
                err = netpollcheckerr(pd, int32(mode));
                if (err != 0L)
                {
                    return err;
                } 
                // Can happen if timeout has fired and unblocked us,
                // but before we had a chance to run, timeout has been reset.
                // Pretend it has not happened and retry.
            }

            return 0L;
        }

        //go:linkname poll_runtime_pollWaitCanceled internal/poll.runtime_pollWaitCanceled
        private static void poll_runtime_pollWaitCanceled(ref pollDesc pd, long mode)
        { 
            // This function is used only on windows after a failed attempt to cancel
            // a pending async IO operation. Wait for ioready, ignore closing or timeouts.
            while (!netpollblock(pd, int32(mode), true))
            {
            }

        }

        //go:linkname poll_runtime_pollSetDeadline internal/poll.runtime_pollSetDeadline
        private static void poll_runtime_pollSetDeadline(ref pollDesc pd, long d, long mode)
        {
            lock(ref pd.@lock);
            if (pd.closing)
            {
                unlock(ref pd.@lock);
                return;
            }
            pd.seq++; // invalidate current timers
            // Reset current timers.
            if (pd.rt.f != null)
            {
                deltimer(ref pd.rt);
                pd.rt.f = null;
            }
            if (pd.wt.f != null)
            {
                deltimer(ref pd.wt);
                pd.wt.f = null;
            } 
            // Setup new timers.
            if (d != 0L && d <= nanotime())
            {
                d = -1L;
            }
            if (mode == 'r' || mode == 'r' + 'w')
            {
                pd.rd = d;
            }
            if (mode == 'w' || mode == 'r' + 'w')
            {
                pd.wd = d;
            }
            if (pd.rd > 0L && pd.rd == pd.wd)
            {
                pd.rt.f = netpollDeadline;
                pd.rt.when = pd.rd; 
                // Copy current seq into the timer arg.
                // Timer func will check the seq against current descriptor seq,
                // if they differ the descriptor was reused or timers were reset.
                pd.rt.arg = pd;
                pd.rt.seq = pd.seq;
                addtimer(ref pd.rt);
            }
            else
            {
                if (pd.rd > 0L)
                {
                    pd.rt.f = netpollReadDeadline;
                    pd.rt.when = pd.rd;
                    pd.rt.arg = pd;
                    pd.rt.seq = pd.seq;
                    addtimer(ref pd.rt);
                }
                if (pd.wd > 0L)
                {
                    pd.wt.f = netpollWriteDeadline;
                    pd.wt.when = pd.wd;
                    pd.wt.arg = pd;
                    pd.wt.seq = pd.seq;
                    addtimer(ref pd.wt);
                }
            } 
            // If we set the new deadline in the past, unblock currently pending IO if any.
            ref g rg = default;            ref g wg = default;

            atomicstorep(@unsafe.Pointer(ref wg), null); // full memory barrier between stores to rd/wd and load of rg/wg in netpollunblock
            if (pd.rd < 0L)
            {
                rg = netpollunblock(pd, 'r', false);
            }
            if (pd.wd < 0L)
            {
                wg = netpollunblock(pd, 'w', false);
            }
            unlock(ref pd.@lock);
            if (rg != null)
            {
                netpollgoready(rg, 3L);
            }
            if (wg != null)
            {
                netpollgoready(wg, 3L);
            }
        }

        //go:linkname poll_runtime_pollUnblock internal/poll.runtime_pollUnblock
        private static void poll_runtime_pollUnblock(ref pollDesc pd)
        {
            lock(ref pd.@lock);
            if (pd.closing)
            {
                throw("runtime: unblock on closing polldesc");
            }
            pd.closing = true;
            pd.seq++;
            ref g rg = default;            ref g wg = default;

            atomicstorep(@unsafe.Pointer(ref rg), null); // full memory barrier between store to closing and read of rg/wg in netpollunblock
            rg = netpollunblock(pd, 'r', false);
            wg = netpollunblock(pd, 'w', false);
            if (pd.rt.f != null)
            {
                deltimer(ref pd.rt);
                pd.rt.f = null;
            }
            if (pd.wt.f != null)
            {
                deltimer(ref pd.wt);
                pd.wt.f = null;
            }
            unlock(ref pd.@lock);
            if (rg != null)
            {
                netpollgoready(rg, 3L);
            }
            if (wg != null)
            {
                netpollgoready(wg, 3L);
            }
        }

        // make pd ready, newly runnable goroutines (if any) are returned in rg/wg
        // May run during STW, so write barriers are not allowed.
        //go:nowritebarrier
        private static void netpollready(ref guintptr gpp, ref pollDesc pd, int mode)
        {
            guintptr rg = default;            guintptr wg = default;

            if (mode == 'r' || mode == 'r' + 'w')
            {
                rg.set(netpollunblock(pd, 'r', true));
            }
            if (mode == 'w' || mode == 'r' + 'w')
            {
                wg.set(netpollunblock(pd, 'w', true));
            }
            if (rg != 0L)
            {
                rg.ptr().schedlink = gpp.Value;
                gpp.Value = rg;
            }
            if (wg != 0L)
            {
                wg.ptr().schedlink = gpp.Value;
                gpp.Value = wg;
            }
        }

        private static long netpollcheckerr(ref pollDesc pd, int mode)
        {
            if (pd.closing)
            {
                return 1L; // errClosing
            }
            if ((mode == 'r' && pd.rd < 0L) || (mode == 'w' && pd.wd < 0L))
            {
                return 2L; // errTimeout
            }
            return 0L;
        }

        private static bool netpollblockcommit(ref g gp, unsafe.Pointer gpp)
        {
            var r = atomic.Casuintptr((uintptr.Value)(gpp), pdWait, uintptr(@unsafe.Pointer(gp)));
            if (r)
            { 
                // Bump the count of goroutines waiting for the poller.
                // The scheduler uses this to decide whether to block
                // waiting for the poller if there is nothing else to do.
                atomic.Xadd(ref netpollWaiters, 1L);
            }
            return r;
        }

        private static void netpollgoready(ref g gp, long traceskip)
        {
            atomic.Xadd(ref netpollWaiters, -1L);
            goready(gp, traceskip + 1L);
        }

        // returns true if IO is ready, or false if timedout or closed
        // waitio - wait only for completed IO, ignore errors
        private static bool netpollblock(ref pollDesc pd, int mode, bool waitio)
        {
            var gpp = ref pd.rg;
            if (mode == 'w')
            {
                gpp = ref pd.wg;
            } 

            // set the gpp semaphore to WAIT
            while (true)
            {
                var old = gpp.Value;
                if (old == pdReady)
                {
                    gpp.Value = 0L;
                    return true;
                }
                if (old != 0L)
                {
                    throw("runtime: double wait");
                }
                if (atomic.Casuintptr(gpp, 0L, pdWait))
                {
                    break;
                }
            } 

            // need to recheck error states after setting gpp to WAIT
            // this is necessary because runtime_pollUnblock/runtime_pollSetDeadline/deadlineimpl
            // do the opposite: store to closing/rd/wd, membarrier, load of rg/wg
 

            // need to recheck error states after setting gpp to WAIT
            // this is necessary because runtime_pollUnblock/runtime_pollSetDeadline/deadlineimpl
            // do the opposite: store to closing/rd/wd, membarrier, load of rg/wg
            if (waitio || netpollcheckerr(pd, mode) == 0L)
            {
                gopark(netpollblockcommit, @unsafe.Pointer(gpp), "IO wait", traceEvGoBlockNet, 5L);
            } 
            // be careful to not lose concurrent READY notification
            old = atomic.Xchguintptr(gpp, 0L);
            if (old > pdWait)
            {
                throw("runtime: corrupted polldesc");
            }
            return old == pdReady;
        }

        private static ref g netpollunblock(ref pollDesc pd, int mode, bool ioready)
        {
            var gpp = ref pd.rg;
            if (mode == 'w')
            {
                gpp = ref pd.wg;
            }
            while (true)
            {
                var old = gpp.Value;
                if (old == pdReady)
                {
                    return null;
                }
                if (old == 0L && !ioready)
                { 
                    // Only set READY for ioready. runtime_pollWait
                    // will check for timeout/cancel before waiting.
                    return null;
                }
                System.UIntPtr @new = default;
                if (ioready)
                {
                    new = pdReady;
                }
                if (atomic.Casuintptr(gpp, old, new))
                {
                    if (old == pdReady || old == pdWait)
                    {
                        old = 0L;
                    }
                    return (g.Value)(@unsafe.Pointer(old));
                }
            }

        }

        private static void netpolldeadlineimpl(ref pollDesc pd, System.UIntPtr seq, bool read, bool write)
        {
            lock(ref pd.@lock); 
            // Seq arg is seq when the timer was set.
            // If it's stale, ignore the timer event.
            if (seq != pd.seq)
            { 
                // The descriptor was reused or timers were reset.
                unlock(ref pd.@lock);
                return;
            }
            ref g rg = default;
            if (read)
            {
                if (pd.rd <= 0L || pd.rt.f == null)
                {
                    throw("runtime: inconsistent read deadline");
                }
                pd.rd = -1L;
                atomicstorep(@unsafe.Pointer(ref pd.rt.f), null); // full memory barrier between store to rd and load of rg in netpollunblock
                rg = netpollunblock(pd, 'r', false);
            }
            ref g wg = default;
            if (write)
            {
                if (pd.wd <= 0L || pd.wt.f == null && !read)
                {
                    throw("runtime: inconsistent write deadline");
                }
                pd.wd = -1L;
                atomicstorep(@unsafe.Pointer(ref pd.wt.f), null); // full memory barrier between store to wd and load of wg in netpollunblock
                wg = netpollunblock(pd, 'w', false);
            }
            unlock(ref pd.@lock);
            if (rg != null)
            {
                netpollgoready(rg, 0L);
            }
            if (wg != null)
            {
                netpollgoready(wg, 0L);
            }
        }

        private static void netpollDeadline(object arg, System.UIntPtr seq)
        {
            netpolldeadlineimpl(arg._<ref pollDesc>(), seq, true, true);
        }

        private static void netpollReadDeadline(object arg, System.UIntPtr seq)
        {
            netpolldeadlineimpl(arg._<ref pollDesc>(), seq, true, false);
        }

        private static void netpollWriteDeadline(object arg, System.UIntPtr seq)
        {
            netpolldeadlineimpl(arg._<ref pollDesc>(), seq, false, true);
        }

        private static ref pollDesc alloc(this ref pollCache c)
        {
            lock(ref c.@lock);
            if (c.first == null)
            {
                const var pdSize = @unsafe.Sizeof(new pollDesc());

                var n = pollBlockSize / pdSize;
                if (n == 0L)
                {
                    n = 1L;
                } 
                // Must be in non-GC memory because can be referenced
                // only from epoll/kqueue internals.
                var mem = persistentalloc(n * pdSize, 0L, ref memstats.other_sys);
                for (var i = uintptr(0L); i < n; i++)
                {
                    var pd = (pollDesc.Value)(add(mem, i * pdSize));
                    pd.link = c.first;
                    c.first = pd;
                }

            }
            pd = c.first;
            c.first = pd.link;
            unlock(ref c.@lock);
            return pd;
        }
    }
}
