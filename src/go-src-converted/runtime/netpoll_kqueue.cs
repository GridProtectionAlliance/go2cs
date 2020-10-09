// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin dragonfly freebsd netbsd openbsd

// package runtime -- go2cs converted at 2020 October 09 04:47:15 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\netpoll_kqueue.go
// Integrated network poller (kqueue-based implementation).

using atomic = go.runtime.@internal.atomic_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        private static int kq = -1L;        private static System.UIntPtr netpollBreakRd = default;        private static System.UIntPtr netpollBreakWr = default; // for netpollBreak

        private static uint netpollWakeSig = default;

        private static void netpollinit()
        {
            kq = kqueue();
            if (kq < 0L)
            {
                println("runtime: kqueue failed with", -kq);
                throw("runtime: netpollinit failed");
            }

            closeonexec(kq);
            var (r, w, errno) = nonblockingPipe();
            if (errno != 0L)
            {
                println("runtime: pipe failed with", -errno);
                throw("runtime: pipe failed");
            }

            ref keventt ev = ref heap(new keventt(filter:_EVFILT_READ,flags:_EV_ADD,) * (uintptr.val)(@unsafe.Pointer(_addr_ev.ident)), out ptr<keventt> _addr_ev);

            uintptr(r);
            var n = kevent(kq, _addr_ev, 1L, null, 0L, null);
            if (n < 0L)
            {
                println("runtime: kevent failed with", -n);
                throw("runtime: kevent failed");
            }

            netpollBreakRd = uintptr(r);
            netpollBreakWr = uintptr(w);

        }

        private static bool netpollIsPollDescriptor(System.UIntPtr fd)
        {
            return fd == uintptr(kq) || fd == netpollBreakRd || fd == netpollBreakWr;
        }

        private static int netpollopen(System.UIntPtr fd, ptr<pollDesc> _addr_pd)
        {
            ref pollDesc pd = ref _addr_pd.val;
 
            // Arm both EVFILT_READ and EVFILT_WRITE in edge-triggered mode (EV_CLEAR)
            // for the whole fd lifetime. The notifications are automatically unregistered
            // when fd is closed.
            array<keventt> ev = new array<keventt>(2L);
            (uintptr.val)(@unsafe.Pointer(_addr_ev[0L].ident)).val;

            fd;
            ev[0L].filter = _EVFILT_READ;
            ev[0L].flags = _EV_ADD | _EV_CLEAR;
            ev[0L].fflags = 0L;
            ev[0L].data = 0L;
            ev[0L].udata = (byte.val)(@unsafe.Pointer(pd));
            ev[1L] = ev[0L];
            ev[1L].filter = _EVFILT_WRITE;
            var n = kevent(kq, _addr_ev[0L], 2L, null, 0L, null);
            if (n < 0L)
            {
                return -n;
            }

            return 0L;

        }

        private static int netpollclose(System.UIntPtr fd)
        { 
            // Don't need to unregister because calling close()
            // on fd will remove any kevents that reference the descriptor.
            return 0L;

        }

        private static void netpollarm(ptr<pollDesc> _addr_pd, long mode)
        {
            ref pollDesc pd = ref _addr_pd.val;

            throw("runtime: unused");
        }

        // netpollBreak interrupts a kevent.
        private static void netpollBreak()
        {
            if (atomic.Cas(_addr_netpollWakeSig, 0L, 1L))
            {
                while (true)
                {
                    ref byte b = ref heap(out ptr<byte> _addr_b);
                    var n = write(netpollBreakWr, @unsafe.Pointer(_addr_b), 1L);
                    if (n == 1L || n == -_EAGAIN)
                    {
                        break;
                    }

                    if (n == -_EINTR)
                    {
                        continue;
                    }

                    println("runtime: netpollBreak write failed with", -n);
                    throw("runtime: netpollBreak write failed");

                }


            }

        }

        // netpoll checks for ready network connections.
        // Returns list of goroutines that become runnable.
        // delay < 0: blocks indefinitely
        // delay == 0: does not block, just polls
        // delay > 0: block for up to that many nanoseconds
        private static gList netpoll(long delay)
        {
            if (kq == -1L)
            {
                return new gList();
            }

            ptr<timespec> tp;
            ref timespec ts = ref heap(out ptr<timespec> _addr_ts);
            if (delay < 0L)
            {
                tp = null;
            }
            else if (delay == 0L)
            {
                tp = _addr_ts;
            }
            else
            {
                ts.setNsec(delay);
                if (ts.tv_sec > 1e6F)
                { 
                    // Darwin returns EINVAL if the sleep time is too long.
                    ts.tv_sec = 1e6F;

                }

                tp = _addr_ts;

            }

            array<keventt> events = new array<keventt>(64L);
retry:
            var n = kevent(kq, null, 0L, _addr_events[0L], int32(len(events)), tp);
            if (n < 0L)
            {
                if (n != -_EINTR)
                {
                    println("runtime: kevent on fd", kq, "failed with", -n);
                    throw("runtime: netpoll failed");
                } 
                // If a timed sleep was interrupted, just return to
                // recalculate how long we should sleep now.
                if (delay > 0L)
                {
                    return new gList();
                }

                goto retry;

            }

            ref gList toRun = ref heap(out ptr<gList> _addr_toRun);
            for (long i = 0L; i < int(n); i++)
            {
                var ev = _addr_events[i];

                if (uintptr(ev.ident) == netpollBreakRd)
                {
                    if (ev.filter != _EVFILT_READ)
                    {
                        println("runtime: netpoll: break fd ready for", ev.filter);
                        throw("runtime: netpoll: break fd ready for something unexpected");
                    }

                    if (delay != 0L)
                    { 
                        // netpollBreak could be picked up by a
                        // nonblocking poll. Only read the byte
                        // if blocking.
                        array<byte> tmp = new array<byte>(16L);
                        read(int32(netpollBreakRd), noescape(@unsafe.Pointer(_addr_tmp[0L])), int32(len(tmp)));
                        atomic.Store(_addr_netpollWakeSig, 0L);

                    }

                    continue;

                }

                int mode = default;

                if (ev.filter == _EVFILT_READ) 
                    mode += 'r'; 

                    // On some systems when the read end of a pipe
                    // is closed the write end will not get a
                    // _EVFILT_WRITE event, but will get a
                    // _EVFILT_READ event with EV_EOF set.
                    // Note that setting 'w' here just means that we
                    // will wake up a goroutine waiting to write;
                    // that goroutine will try the write again,
                    // and the appropriate thing will happen based
                    // on what that write returns (success, EPIPE, EAGAIN).
                    if (ev.flags & _EV_EOF != 0L)
                    {
                        mode += 'w';
                    }

                else if (ev.filter == _EVFILT_WRITE) 
                    mode += 'w';
                                if (mode != 0L)
                {
                    var pd = (pollDesc.val)(@unsafe.Pointer(ev.udata));
                    pd.everr = false;
                    if (ev.flags == _EV_ERROR)
                    {
                        pd.everr = true;
                    }

                    netpollready(_addr_toRun, pd, mode);

                }

            }

            return toRun;

        }
    }
}
