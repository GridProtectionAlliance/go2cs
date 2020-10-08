// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 October 08 03:21:36 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\netpoll_aix.go
using atomic = go.runtime.@internal.atomic_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        // This is based on the former libgo/runtime/netpoll_select.c implementation
        // except that it uses poll instead of select and is written in Go.
        // It's also based on Solaris implementation for the arming mechanisms

        //go:cgo_import_dynamic libc_poll poll "libc.a/shr_64.o"
        //go:linkname libc_poll libc_poll
        private static libFunc libc_poll = default;

        //go:nosplit
        private static (int, int) poll(ptr<pollfd> _addr_pfds, System.UIntPtr npfds, System.UIntPtr timeout)
        {
            int _p0 = default;
            int _p0 = default;
            ref pollfd pfds = ref _addr_pfds.val;

            var (r, err) = syscall3(_addr_libc_poll, uintptr(@unsafe.Pointer(pfds)), npfds, timeout);
            return (int32(r), int32(err));
        }

        // pollfd represents the poll structure for AIX operating system.
        private partial struct pollfd
        {
            public int fd;
            public short events;
            public short revents;
        }

        private static readonly ulong _POLLIN = (ulong)0x0001UL;

        private static readonly ulong _POLLOUT = (ulong)0x0002UL;

        private static readonly ulong _POLLHUP = (ulong)0x2000UL;

        private static readonly ulong _POLLERR = (ulong)0x4000UL;



        private static slice<pollfd> pfds = default;        private static slice<ptr<pollDesc>> pds = default;        private static mutex mtxpoll = default;        private static mutex mtxset = default;        private static int rdwake = default;        private static int wrwake = default;        private static int pendingUpdates = default;        private static uint netpollWakeSig = default;

        private static void netpollinit()
        { 
            // Create the pipe we use to wakeup poll.
            var (r, w, errno) = nonblockingPipe();
            if (errno != 0L)
            {
                throw("netpollinit: failed to create pipe");
            }

            rdwake = r;
            wrwake = w; 

            // Pre-allocate array of pollfd structures for poll.
            pfds = make_slice<pollfd>(1L, 128L); 

            // Poll the read side of the pipe.
            pfds[0L].fd = rdwake;
            pfds[0L].events = _POLLIN;

            pds = make_slice<ptr<pollDesc>>(1L, 128L);
            pds[0L] = null;

        }

        private static bool netpollIsPollDescriptor(System.UIntPtr fd)
        {
            return fd == uintptr(rdwake) || fd == uintptr(wrwake);
        }

        // netpollwakeup writes on wrwake to wakeup poll before any changes.
        private static void netpollwakeup()
        {
            if (pendingUpdates == 0L)
            {
                pendingUpdates = 1L;
                array<byte> b = new array<byte>(new byte[] { 0 });
                write(uintptr(wrwake), @unsafe.Pointer(_addr_b[0L]), 1L);
            }

        }

        private static int netpollopen(System.UIntPtr fd, ptr<pollDesc> _addr_pd)
        {
            ref pollDesc pd = ref _addr_pd.val;

            lock(_addr_mtxpoll);
            netpollwakeup();

            lock(_addr_mtxset);
            unlock(_addr_mtxpoll);

            pd.user = uint32(len(pfds));
            pfds = append(pfds, new pollfd(fd:int32(fd)));
            pds = append(pds, pd);
            unlock(_addr_mtxset);
            return 0L;
        }

        private static int netpollclose(System.UIntPtr fd)
        {
            lock(_addr_mtxpoll);
            netpollwakeup();

            lock(_addr_mtxset);
            unlock(_addr_mtxpoll);

            for (long i = 0L; i < len(pfds); i++)
            {
                if (pfds[i].fd == int32(fd))
                {
                    pfds[i] = pfds[len(pfds) - 1L];
                    pfds = pfds[..len(pfds) - 1L];

                    pds[i] = pds[len(pds) - 1L];
                    pds[i].user = uint32(i);
                    pds = pds[..len(pds) - 1L];
                    break;
                }

            }

            unlock(_addr_mtxset);
            return 0L;

        }

        private static void netpollarm(ptr<pollDesc> _addr_pd, long mode)
        {
            ref pollDesc pd = ref _addr_pd.val;

            lock(_addr_mtxpoll);
            netpollwakeup();

            lock(_addr_mtxset);
            unlock(_addr_mtxpoll);

            switch (mode)
            {
                case 'r': 
                    pfds[pd.user].events |= _POLLIN;
                    break;
                case 'w': 
                    pfds[pd.user].events |= _POLLOUT;
                    break;
            }
            unlock(_addr_mtxset);

        }

        // netpollBreak interrupts a poll.
        private static void netpollBreak()
        {
            if (atomic.Cas(_addr_netpollWakeSig, 0L, 1L))
            {
                array<byte> b = new array<byte>(new byte[] { 0 });
                write(uintptr(wrwake), @unsafe.Pointer(_addr_b[0L]), 1L);
            }

        }

        // netpoll checks for ready network connections.
        // Returns list of goroutines that become runnable.
        // delay < 0: blocks indefinitely
        // delay == 0: does not block, just polls
        // delay > 0: block for up to that many nanoseconds
        //go:nowritebarrierrec
        private static gList netpoll(long delay)
        {
            System.UIntPtr timeout = default;
            if (delay < 0L)
            {
                timeout = ~uintptr(0L);
            }
            else if (delay == 0L)
            { 
                // TODO: call poll with timeout == 0
                return new gList();

            }
            else if (delay < 1e6F)
            {
                timeout = 1L;
            }
            else if (delay < 1e15F)
            {
                timeout = uintptr(delay / 1e6F);
            }
            else
            { 
                // An arbitrary cap on how long to wait for a timer.
                // 1e9 ms == ~11.5 days.
                timeout = 1e9F;

            }

retry:
            lock(_addr_mtxpoll);
            lock(_addr_mtxset);
            pendingUpdates = 0L;
            unlock(_addr_mtxpoll);

            var (n, e) = poll(_addr_pfds[0L], uintptr(len(pfds)), timeout);
            if (n < 0L)
            {
                if (e != _EINTR)
                {
                    println("errno=", e, " len(pfds)=", len(pfds));
                    throw("poll failed");
                }

                unlock(_addr_mtxset); 
                // If a timed sleep was interrupted, just return to
                // recalculate how long we should sleep now.
                if (timeout > 0L)
                {
                    return new gList();
                }

                goto retry;

            } 
            // Check if some descriptors need to be changed
            if (n != 0L && pfds[0L].revents & (_POLLIN | _POLLHUP | _POLLERR) != 0L)
            {
                if (delay != 0L)
                { 
                    // A netpollwakeup could be picked up by a
                    // non-blocking poll. Only clear the wakeup
                    // if blocking.
                    array<byte> b = new array<byte>(1L);
                    while (read(rdwake, @unsafe.Pointer(_addr_b[0L]), 1L) == 1L)
                    {
                    }

                    atomic.Store(_addr_netpollWakeSig, 0L);

                } 
                // Still look at the other fds even if the mode may have
                // changed, as netpollBreak might have been called.
                n--;

            }

            ref gList toRun = ref heap(out ptr<gList> _addr_toRun);
            for (long i = 1L; i < len(pfds) && n > 0L; i++)
            {
                var pfd = _addr_pfds[i];

                int mode = default;
                if (pfd.revents & (_POLLIN | _POLLHUP | _POLLERR) != 0L)
                {
                    mode += 'r';
                    pfd.events &= ~_POLLIN;
                }

                if (pfd.revents & (_POLLOUT | _POLLHUP | _POLLERR) != 0L)
                {
                    mode += 'w';
                    pfd.events &= ~_POLLOUT;
                }

                if (mode != 0L)
                {
                    pds[i].everr = false;
                    if (pfd.revents == _POLLERR)
                    {
                        pds[i].everr = true;
                    }

                    netpollready(_addr_toRun, pds[i], mode);
                    n--;

                }

            }

            unlock(_addr_mtxset);
            return toRun;

        }
    }
}
