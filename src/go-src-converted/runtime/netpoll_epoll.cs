// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build linux

// package runtime -- go2cs converted at 2020 August 29 08:18:36 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\netpoll_epoll.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static unsafe partial class runtime_package
    {
        private static int epollcreate(int size)
;
        private static int epollcreate1(int flags)
;

        //go:noescape
        private static int epollctl(int epfd, int op, int fd, ref epollevent ev)
;

        //go:noescape
        private static int epollwait(int epfd, ref epollevent ev, int nev, int timeout)
;
        private static void closeonexec(int fd)
;

        private static int epfd = -1L;

        private static void netpollinit()
        {
            epfd = epollcreate1(_EPOLL_CLOEXEC);
            if (epfd >= 0L)
            {>>MARKER:FUNCTION_closeonexec_BLOCK_PREFIX<<
                return;
            }
            epfd = epollcreate(1024L);
            if (epfd >= 0L)
            {>>MARKER:FUNCTION_epollwait_BLOCK_PREFIX<<
                closeonexec(epfd);
                return;
            }
            println("runtime: epollcreate failed with", -epfd);
            throw("runtime: netpollinit failed");
        }

        private static System.UIntPtr netpolldescriptor()
        {
            return uintptr(epfd);
        }

        private static int netpollopen(System.UIntPtr fd, ref pollDesc pd)
        {
            epollevent ev = default;
            ev.events = _EPOLLIN | _EPOLLOUT | _EPOLLRDHUP | _EPOLLET * (pollDesc.Value.Value)(@unsafe.Pointer(ref ev.data));

            pd;
            return -epollctl(epfd, _EPOLL_CTL_ADD, int32(fd), ref ev);
        }

        private static int netpollclose(System.UIntPtr fd)
        {
            epollevent ev = default;
            return -epollctl(epfd, _EPOLL_CTL_DEL, int32(fd), ref ev);
        }

        private static void netpollarm(ref pollDesc pd, long mode)
        {
            throw("runtime: unused");
        }

        // polls for ready network connections
        // returns list of goroutines that become runnable
        private static ref g netpoll(bool block)
        {
            if (epfd == -1L)
            {>>MARKER:FUNCTION_epollctl_BLOCK_PREFIX<<
                return null;
            }
            var waitms = int32(-1L);
            if (!block)
            {>>MARKER:FUNCTION_epollcreate1_BLOCK_PREFIX<<
                waitms = 0L;
            }
            array<epollevent> events = new array<epollevent>(128L);
retry:
            var n = epollwait(epfd, ref events[0L], int32(len(events)), waitms);
            if (n < 0L)
            {>>MARKER:FUNCTION_epollcreate_BLOCK_PREFIX<<
                if (n != -_EINTR)
                {
                    println("runtime: epollwait on fd", epfd, "failed with", -n);
                    throw("runtime: netpoll failed");
                }
                goto retry;
            }
            guintptr gp = default;
            for (var i = int32(0L); i < n; i++)
            {
                var ev = ref events[i];
                if (ev.events == 0L)
                {
                    continue;
                }
                int mode = default;
                if (ev.events & (_EPOLLIN | _EPOLLRDHUP | _EPOLLHUP | _EPOLLERR) != 0L)
                {
                    mode += 'r';
                }
                if (ev.events & (_EPOLLOUT | _EPOLLHUP | _EPOLLERR) != 0L)
                {
                    mode += 'w';
                }
                if (mode != 0L)
                {
                    *(ptr<ptr<pollDesc>>) pd = new ptr<*(ptr<ptr<pollDesc>>)>(@unsafe.Pointer(ref ev.data));

                    netpollready(ref gp, pd, mode);
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
