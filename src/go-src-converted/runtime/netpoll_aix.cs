// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2022 March 06 22:10:15 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\netpoll_aix.go
using atomic = go.runtime.@internal.atomic_package;
using @unsafe = go.@unsafe_package;

namespace go;

public static partial class runtime_package {

    // This is based on the former libgo/runtime/netpoll_select.c implementation
    // except that it uses poll instead of select and is written in Go.
    // It's also based on Solaris implementation for the arming mechanisms

    //go:cgo_import_dynamic libc_poll poll "libc.a/shr_64.o"
    //go:linkname libc_poll libc_poll
private static libFunc libc_poll = default;

//go:nosplit
private static (int, int) poll(ptr<pollfd> _addr_pfds, System.UIntPtr npfds, System.UIntPtr timeout) {
    int _p0 = default;
    int _p0 = default;
    ref pollfd pfds = ref _addr_pfds.val;

    var (r, err) = syscall3(_addr_libc_poll, uintptr(@unsafe.Pointer(pfds)), npfds, timeout);
    return (int32(r), int32(err));
}

// pollfd represents the poll structure for AIX operating system.
private partial struct pollfd {
    public int fd;
    public short events;
    public short revents;
}

private static readonly nuint _POLLIN = 0x0001;

private static readonly nuint _POLLOUT = 0x0002;

private static readonly nuint _POLLHUP = 0x2000;

private static readonly nuint _POLLERR = 0x4000;



private static slice<pollfd> pfds = default;private static slice<ptr<pollDesc>> pds = default;private static mutex mtxpoll = default;private static mutex mtxset = default;private static int rdwake = default;private static int wrwake = default;private static int pendingUpdates = default;private static uint netpollWakeSig = default;

private static void netpollinit() { 
    // Create the pipe we use to wakeup poll.
    var (r, w, errno) = nonblockingPipe();
    if (errno != 0) {
        throw("netpollinit: failed to create pipe");
    }
    rdwake = r;
    wrwake = w; 

    // Pre-allocate array of pollfd structures for poll.
    pfds = make_slice<pollfd>(1, 128); 

    // Poll the read side of the pipe.
    pfds[0].fd = rdwake;
    pfds[0].events = _POLLIN;

    pds = make_slice<ptr<pollDesc>>(1, 128);
    pds[0] = null;

}

private static bool netpollIsPollDescriptor(System.UIntPtr fd) {
    return fd == uintptr(rdwake) || fd == uintptr(wrwake);
}

// netpollwakeup writes on wrwake to wakeup poll before any changes.
private static void netpollwakeup() {
    if (pendingUpdates == 0) {
        pendingUpdates = 1;
        array<byte> b = new array<byte>(new byte[] { 0 });
        write(uintptr(wrwake), @unsafe.Pointer(_addr_b[0]), 1);
    }
}

private static int netpollopen(System.UIntPtr fd, ptr<pollDesc> _addr_pd) {
    ref pollDesc pd = ref _addr_pd.val;

    lock(_addr_mtxpoll);
    netpollwakeup();

    lock(_addr_mtxset);
    unlock(_addr_mtxpoll);

    pd.user = uint32(len(pfds));
    pfds = append(pfds, new pollfd(fd:int32(fd)));
    pds = append(pds, pd);
    unlock(_addr_mtxset);
    return 0;
}

private static int netpollclose(System.UIntPtr fd) {
    lock(_addr_mtxpoll);
    netpollwakeup();

    lock(_addr_mtxset);
    unlock(_addr_mtxpoll);

    for (nint i = 0; i < len(pfds); i++) {
        if (pfds[i].fd == int32(fd)) {
            pfds[i] = pfds[len(pfds) - 1];
            pfds = pfds[..(int)len(pfds) - 1];

            pds[i] = pds[len(pds) - 1];
            pds[i].user = uint32(i);
            pds = pds[..(int)len(pds) - 1];
            break;
        }
    }
    unlock(_addr_mtxset);
    return 0;

}

private static void netpollarm(ptr<pollDesc> _addr_pd, nint mode) {
    ref pollDesc pd = ref _addr_pd.val;

    lock(_addr_mtxpoll);
    netpollwakeup();

    lock(_addr_mtxset);
    unlock(_addr_mtxpoll);

    switch (mode) {
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
private static void netpollBreak() {
    if (atomic.Cas(_addr_netpollWakeSig, 0, 1)) {
        array<byte> b = new array<byte>(new byte[] { 0 });
        write(uintptr(wrwake), @unsafe.Pointer(_addr_b[0]), 1);
    }
}

// netpoll checks for ready network connections.
// Returns list of goroutines that become runnable.
// delay < 0: blocks indefinitely
// delay == 0: does not block, just polls
// delay > 0: block for up to that many nanoseconds
//go:nowritebarrierrec
private static gList netpoll(long delay) {
    System.UIntPtr timeout = default;
    if (delay < 0) {
        timeout = ~uintptr(0);
    }
    else if (delay == 0) { 
        // TODO: call poll with timeout == 0
        return new gList();

    }
    else if (delay < 1e6F) {
        timeout = 1;
    }
    else if (delay < 1e15F) {
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
    pendingUpdates = 0;
    unlock(_addr_mtxpoll);

    var (n, e) = poll(_addr_pfds[0], uintptr(len(pfds)), timeout);
    if (n < 0) {
        if (e != _EINTR) {
            println("errno=", e, " len(pfds)=", len(pfds));
            throw("poll failed");
        }
        unlock(_addr_mtxset); 
        // If a timed sleep was interrupted, just return to
        // recalculate how long we should sleep now.
        if (timeout > 0) {
            return new gList();
        }
        goto retry;

    }
    if (n != 0 && pfds[0].revents & (_POLLIN | _POLLHUP | _POLLERR) != 0) {
        if (delay != 0) { 
            // A netpollwakeup could be picked up by a
            // non-blocking poll. Only clear the wakeup
            // if blocking.
            array<byte> b = new array<byte>(1);
            while (read(rdwake, @unsafe.Pointer(_addr_b[0]), 1) == 1)             }

            atomic.Store(_addr_netpollWakeSig, 0);

        }
        n--;

    }
    ref gList toRun = ref heap(out ptr<gList> _addr_toRun);
    for (nint i = 1; i < len(pfds) && n > 0; i++) {
        var pfd = _addr_pfds[i];

        int mode = default;
        if (pfd.revents & (_POLLIN | _POLLHUP | _POLLERR) != 0) {
            mode += 'r';
            pfd.events &= ~_POLLIN;
        }
        if (pfd.revents & (_POLLOUT | _POLLHUP | _POLLERR) != 0) {
            mode += 'w';
            pfd.events &= ~_POLLOUT;
        }
        if (mode != 0) {
            pds[i].everr = false;
            if (pfd.revents == _POLLERR) {
                pds[i].everr = true;
            }
            netpollready(_addr_toRun, pds[i], mode);
            n--;
        }
    }
    unlock(_addr_mtxset);
    return toRun;

}

} // end runtime_package
