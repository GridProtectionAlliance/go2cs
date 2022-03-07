// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build linux
// +build linux

// package runtime -- go2cs converted at 2022 March 06 22:10:15 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\netpoll_epoll.go
using atomic = go.runtime.@internal.atomic_package;
using @unsafe = go.@unsafe_package;

namespace go;

public static partial class runtime_package {

private static int epollcreate(int size);
private static int epollcreate1(int flags);

//go:noescape
private static int epollctl(int epfd, int op, int fd, ptr<epollevent> ev);

//go:noescape
private static int epollwait(int epfd, ptr<epollevent> ev, int nev, int timeout);
private static void closeonexec(int fd);

private static int epfd = -1;private static System.UIntPtr netpollBreakRd = default;private static System.UIntPtr netpollBreakWr = default; // for netpollBreak

private static uint netpollWakeSig = default;

private static void netpollinit() {
    epfd = epollcreate1(_EPOLL_CLOEXEC);
    if (epfd < 0) {>>MARKER:FUNCTION_closeonexec_BLOCK_PREFIX<<
        epfd = epollcreate(1024);
        if (epfd < 0) {>>MARKER:FUNCTION_epollwait_BLOCK_PREFIX<<
            println("runtime: epollcreate failed with", -epfd);
            throw("runtime: netpollinit failed");
        }
        closeonexec(epfd);

    }
    var (r, w, errno) = nonblockingPipe();
    if (errno != 0) {>>MARKER:FUNCTION_epollctl_BLOCK_PREFIX<<
        println("runtime: pipe failed with", -errno);
        throw("runtime: pipe failed");
    }
    ref epollevent ev = ref heap(new epollevent(events:_EPOLLIN,) * (uintptr.val)(@unsafe.Pointer(_addr_ev.data)), out ptr<epollevent> _addr_ev);

    _addr_netpollBreakRd;
    errno = epollctl(epfd, _EPOLL_CTL_ADD, r, _addr_ev);
    if (errno != 0) {>>MARKER:FUNCTION_epollcreate1_BLOCK_PREFIX<<
        println("runtime: epollctl failed with", -errno);
        throw("runtime: epollctl failed");
    }
    netpollBreakRd = uintptr(r);
    netpollBreakWr = uintptr(w);

}

private static bool netpollIsPollDescriptor(System.UIntPtr fd) {
    return fd == uintptr(epfd) || fd == netpollBreakRd || fd == netpollBreakWr;
}

private static int netpollopen(System.UIntPtr fd, ptr<pollDesc> _addr_pd) {
    ref pollDesc pd = ref _addr_pd.val;

    ref epollevent ev = ref heap(out ptr<epollevent> _addr_ev);
    ev.events = _EPOLLIN | _EPOLLOUT | _EPOLLRDHUP | _EPOLLET * (pollDesc.val)(@unsafe.Pointer(_addr_ev.data));

    pd;
    return -epollctl(epfd, _EPOLL_CTL_ADD, int32(fd), _addr_ev);

}

private static int netpollclose(System.UIntPtr fd) {
    ref epollevent ev = ref heap(out ptr<epollevent> _addr_ev);
    return -epollctl(epfd, _EPOLL_CTL_DEL, int32(fd), _addr_ev);
}

private static void netpollarm(ptr<pollDesc> _addr_pd, nint mode) {
    ref pollDesc pd = ref _addr_pd.val;

    throw("runtime: unused");
}

// netpollBreak interrupts an epollwait.
private static void netpollBreak() {
    if (atomic.Cas(_addr_netpollWakeSig, 0, 1)) {>>MARKER:FUNCTION_epollcreate_BLOCK_PREFIX<<
        while (true) {
            ref byte b = ref heap(out ptr<byte> _addr_b);
            var n = write(netpollBreakWr, @unsafe.Pointer(_addr_b), 1);
            if (n == 1) {
                break;
            }
            if (n == -_EINTR) {
                continue;
            }
            if (n == -_EAGAIN) {
                return ;
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
private static gList netpoll(long delay) {
    if (epfd == -1) {
        return new gList();
    }
    int waitms = default;
    if (delay < 0) {
        waitms = -1;
    }
    else if (delay == 0) {
        waitms = 0;
    }
    else if (delay < 1e6F) {
        waitms = 1;
    }
    else if (delay < 1e15F) {
        waitms = int32(delay / 1e6F);
    }
    else
 { 
        // An arbitrary cap on how long to wait for a timer.
        // 1e9 ms == ~11.5 days.
        waitms = 1e9F;

    }
    array<epollevent> events = new array<epollevent>(128);
retry:
    var n = epollwait(epfd, _addr_events[0], int32(len(events)), waitms);
    if (n < 0) {
        if (n != -_EINTR) {
            println("runtime: epollwait on fd", epfd, "failed with", -n);
            throw("runtime: netpoll failed");
        }
        if (waitms > 0) {
            return new gList();
        }
        goto retry;

    }
    ref gList toRun = ref heap(out ptr<gList> _addr_toRun);
    for (var i = int32(0); i < n; i++) {
        var ev = _addr_events[i];
        if (ev.events == 0) {
            continue;
        }
        if (new ptr<ptr<ptr<ptr<System.UIntPtr>>>>(@unsafe.Pointer(_addr_ev.data)) == _addr_netpollBreakRd) {
            if (ev.events != _EPOLLIN) {
                println("runtime: netpoll: break fd ready for", ev.events);
                throw("runtime: netpoll: break fd ready for something unexpected");
            }
            if (delay != 0) { 
                // netpollBreak could be picked up by a
                // nonblocking poll. Only read the byte
                // if blocking.
                array<byte> tmp = new array<byte>(16);
                read(int32(netpollBreakRd), noescape(@unsafe.Pointer(_addr_tmp[0])), int32(len(tmp)));
                atomic.Store(_addr_netpollWakeSig, 0);

            }

            continue;

        }
        int mode = default;
        if (ev.events & (_EPOLLIN | _EPOLLRDHUP | _EPOLLHUP | _EPOLLERR) != 0) {
            mode += 'r';
        }
        if (ev.events & (_EPOLLOUT | _EPOLLHUP | _EPOLLERR) != 0) {
            mode += 'w';
        }
        if (mode != 0) {
            ptr<ptr<ptr<pollDesc>>> pd = new ptr<ptr<ptr<ptr<pollDesc>>>>(@unsafe.Pointer(_addr_ev.data));
            pd.everr = false;
            if (ev.events == _EPOLLERR) {
                pd.everr = true;
            }
            netpollready(_addr_toRun, pd, mode);
        }
    }
    return toRun;

}

} // end runtime_package
