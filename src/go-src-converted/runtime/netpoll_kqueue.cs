// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin dragonfly freebsd netbsd openbsd

// package runtime -- go2cs converted at 2020 August 29 08:18:37 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\netpoll_kqueue.go
// Integrated network poller (kqueue-based implementation).

using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        private static int kqueue()
;

        //go:noescape
        private static int kevent(int kq, ref keventt ch, int nch, ref keventt ev, int nev, ref timespec ts)
;
        private static void closeonexec(int fd)
;

        private static int kq = -1L;

        private static void netpollinit()
        {
            kq = kqueue();
            if (kq < 0L)
            {>>MARKER:FUNCTION_closeonexec_BLOCK_PREFIX<<
                println("runtime: kqueue failed with", -kq);
                throw("runtime: netpollinit failed");
            }
            closeonexec(kq);
        }

        private static System.UIntPtr netpolldescriptor()
        {
            return uintptr(kq);
        }

        private static int netpollopen(System.UIntPtr fd, ref pollDesc pd)
        { 
            // Arm both EVFILT_READ and EVFILT_WRITE in edge-triggered mode (EV_CLEAR)
            // for the whole fd lifetime. The notifications are automatically unregistered
            // when fd is closed.
            array<keventt> ev = new array<keventt>(2L);
            (uintptr.Value)(@unsafe.Pointer(ref ev[0L].ident)).Value;

            fd;
            ev[0L].filter = _EVFILT_READ;
            ev[0L].flags = _EV_ADD | _EV_CLEAR;
            ev[0L].fflags = 0L;
            ev[0L].data = 0L;
            ev[0L].udata = (byte.Value)(@unsafe.Pointer(pd));
            ev[1L] = ev[0L];
            ev[1L].filter = _EVFILT_WRITE;
            var n = kevent(kq, ref ev[0L], 2L, null, 0L, null);
            if (n < 0L)
            {>>MARKER:FUNCTION_kevent_BLOCK_PREFIX<<
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

        private static void netpollarm(ref pollDesc pd, long mode)
        {
            throw("runtime: unused");
        }

        // Polls for ready network connections.
        // Returns list of goroutines that become runnable.
        private static ref g netpoll(bool block)
        {
            if (kq == -1L)
            {>>MARKER:FUNCTION_kqueue_BLOCK_PREFIX<<
                return null;
            }
            ref timespec tp = default;
            timespec ts = default;
            if (!block)
            {
                tp = ref ts;
            }
            array<keventt> events = new array<keventt>(64L);
retry:
            var n = kevent(kq, null, 0L, ref events[0L], int32(len(events)), tp);
            if (n < 0L)
            {
                if (n != -_EINTR)
                {
                    println("runtime: kevent on fd", kq, "failed with", -n);
                    throw("runtime: netpoll failed");
                }
                goto retry;
            }
            guintptr gp = default;
            for (long i = 0L; i < int(n); i++)
            {
                var ev = ref events[i];
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
                    netpollready(ref gp, (pollDesc.Value)(@unsafe.Pointer(ev.udata)), mode);
                }
            }

            if (block && gp == 0L)
            {
                goto retry;
            }
            return gp.ptr();
        }
    }
}
