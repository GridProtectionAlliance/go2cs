// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build darwin || dragonfly || freebsd || netbsd || openbsd
// +build darwin dragonfly freebsd netbsd openbsd

// package runtime -- go2cs converted at 2022 March 13 05:26:04 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\netpoll_kqueue.go
namespace go;
// Integrated network poller (kqueue-based implementation).


using atomic = runtime.@internal.atomic_package;
using @unsafe = @unsafe_package;

public static partial class runtime_package {

private static int kq = -1;private static System.UIntPtr netpollBreakRd = default;private static System.UIntPtr netpollBreakWr = default; // for netpollBreak

private static uint netpollWakeSig = default;

private static void netpollinit() {
    kq = kqueue();
    if (kq < 0) {
        println("runtime: kqueue failed with", -kq);
        throw("runtime: netpollinit failed");
    }
    closeonexec(kq);
    var (r, w, errno) = nonblockingPipe();
    if (errno != 0) {
        println("runtime: pipe failed with", -errno);
        throw("runtime: pipe failed");
    }
    ref keventt ev = ref heap(new keventt(filter:_EVFILT_READ,flags:_EV_ADD,) * (uintptr.val), out ptr<keventt> _addr_ev);

    (@unsafe.Pointer(_addr_ev.ident)) = uintptr(r);
    var n = kevent(kq, _addr_ev, 1, null, 0, null);
    if (n < 0) {
        println("runtime: kevent failed with", -n);
        throw("runtime: kevent failed");
    }
    netpollBreakRd = uintptr(r);
    netpollBreakWr = uintptr(w);
}

private static bool netpollIsPollDescriptor(System.UIntPtr fd) {
    return fd == uintptr(kq) || fd == netpollBreakRd || fd == netpollBreakWr;
}

private static int netpollopen(System.UIntPtr fd, ptr<pollDesc> _addr_pd) {
    ref pollDesc pd = ref _addr_pd.val;
 
    // Arm both EVFILT_READ and EVFILT_WRITE in edge-triggered mode (EV_CLEAR)
    // for the whole fd lifetime. The notifications are automatically unregistered
    // when fd is closed.
    array<keventt> ev = new array<keventt>(2);
    (uintptr.val).val;

    (@unsafe.Pointer(_addr_ev[0].ident)) = fd;
    ev[0].filter = _EVFILT_READ;
    ev[0].flags = _EV_ADD | _EV_CLEAR;
    ev[0].fflags = 0;
    ev[0].data = 0;
    ev[0].udata = (byte.val)(@unsafe.Pointer(pd));
    ev[1] = ev[0];
    ev[1].filter = _EVFILT_WRITE;
    var n = kevent(kq, _addr_ev[0], 2, null, 0, null);
    if (n < 0) {
        return -n;
    }
    return 0;
}

private static int netpollclose(System.UIntPtr fd) { 
    // Don't need to unregister because calling close()
    // on fd will remove any kevents that reference the descriptor.
    return 0;
}

private static void netpollarm(ptr<pollDesc> _addr_pd, nint mode) {
    ref pollDesc pd = ref _addr_pd.val;

    throw("runtime: unused");
}

// netpollBreak interrupts a kevent.
private static void netpollBreak() {
    if (atomic.Cas(_addr_netpollWakeSig, 0, 1)) {
        while (true) {
            ref byte b = ref heap(out ptr<byte> _addr_b);
            var n = write(netpollBreakWr, @unsafe.Pointer(_addr_b), 1);
            if (n == 1 || n == -_EAGAIN) {
                break;
            }
            if (n == -_EINTR) {
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
private static gList netpoll(long delay) {
    if (kq == -1) {
        return new gList();
    }
    ptr<timespec> tp;
    ref timespec ts = ref heap(out ptr<timespec> _addr_ts);
    if (delay < 0) {
        tp = null;
    }
    else if (delay == 0) {
        tp = _addr_ts;
    }
    else
 {
        ts.setNsec(delay);
        if (ts.tv_sec > 1e6F) { 
            // Darwin returns EINVAL if the sleep time is too long.
            ts.tv_sec = 1e6F;
        }
        tp = _addr_ts;
    }
    array<keventt> events = new array<keventt>(64);
retry:
    var n = kevent(kq, null, 0, _addr_events[0], int32(len(events)), tp);
    if (n < 0) {
        if (n != -_EINTR) {
            println("runtime: kevent on fd", kq, "failed with", -n);
            throw("runtime: netpoll failed");
        }
        if (delay > 0) {
            return new gList();
        }
        goto retry;
    }
    ref gList toRun = ref heap(out ptr<gList> _addr_toRun);
    for (nint i = 0; i < int(n); i++) {
        var ev = _addr_events[i];

        if (uintptr(ev.ident) == netpollBreakRd) {
            if (ev.filter != _EVFILT_READ) {
                println("runtime: netpoll: break fd ready for", ev.filter);
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
            if (ev.flags & _EV_EOF != 0) {
                mode += 'w';
            }
        else if (ev.filter == _EVFILT_WRITE) 
            mode += 'w';
                if (mode != 0) {
            var pd = (pollDesc.val)(@unsafe.Pointer(ev.udata));
            pd.everr = false;
            if (ev.flags == _EV_ERROR) {
                pd.everr = true;
            }
            netpollready(_addr_toRun, pd, mode);
        }
    }
    return toRun;
}

} // end runtime_package
