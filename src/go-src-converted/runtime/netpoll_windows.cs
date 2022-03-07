// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2022 March 06 22:10:18 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\netpoll_windows.go
using atomic = go.runtime.@internal.atomic_package;
using @unsafe = go.@unsafe_package;

namespace go;

public static partial class runtime_package {

private static readonly nuint _DWORD_MAX = 0xffffffff;



private static readonly var _INVALID_HANDLE_VALUE = ~uintptr(0);

// net_op must be the same as beginning of internal/poll.operation.
// Keep these in sync.


// net_op must be the same as beginning of internal/poll.operation.
// Keep these in sync.
private partial struct net_op {
    public overlapped o; // used by netpoll
    public ptr<pollDesc> pd;
    public int mode;
    public int errno;
    public uint qty;
}

private partial struct overlappedEntry {
    public System.UIntPtr key;
    public ptr<net_op> op; // In reality it's *overlapped, but we cast it to *net_op anyway.
    public System.UIntPtr @internal;
    public uint qty;
}

private static System.UIntPtr iocphandle = _INVALID_HANDLE_VALUE;private static uint netpollWakeSig = default;

private static void netpollinit() {
    iocphandle = stdcall4(_CreateIoCompletionPort, _INVALID_HANDLE_VALUE, 0, 0, _DWORD_MAX);
    if (iocphandle == 0) {
        println("runtime: CreateIoCompletionPort failed (errno=", getlasterror(), ")");
        throw("runtime: netpollinit failed");
    }
}

private static bool netpollIsPollDescriptor(System.UIntPtr fd) {
    return fd == iocphandle;
}

private static int netpollopen(System.UIntPtr fd, ptr<pollDesc> _addr_pd) {
    ref pollDesc pd = ref _addr_pd.val;

    if (stdcall4(_CreateIoCompletionPort, fd, iocphandle, 0, 0) == 0) {
        return int32(getlasterror());
    }
    return 0;

}

private static int netpollclose(System.UIntPtr fd) { 
    // nothing to do
    return 0;

}

private static void netpollarm(ptr<pollDesc> _addr_pd, nint mode) {
    ref pollDesc pd = ref _addr_pd.val;

    throw("runtime: unused");
}

private static void netpollBreak() {
    if (atomic.Cas(_addr_netpollWakeSig, 0, 1)) {
        if (stdcall4(_PostQueuedCompletionStatus, iocphandle, 0, 0, 0) == 0) {
            println("runtime: netpoll: PostQueuedCompletionStatus failed (errno=", getlasterror(), ")");
            throw("runtime: netpoll: PostQueuedCompletionStatus failed");
        }
    }
}

// netpoll checks for ready network connections.
// Returns list of goroutines that become runnable.
// delay < 0: blocks indefinitely
// delay == 0: does not block, just polls
// delay > 0: block for up to that many nanoseconds
private static gList netpoll(long delay) {
    array<overlappedEntry> entries = new array<overlappedEntry>(64);
    uint wait = default;    ref uint qty = ref heap(out ptr<uint> _addr_qty);    ref uint flags = ref heap(out ptr<uint> _addr_flags);    ref uint n = ref heap(out ptr<uint> _addr_n);    uint i = default;

    int errno = default;
    ptr<net_op> op;
    ref gList toRun = ref heap(out ptr<gList> _addr_toRun);

    var mp = getg().m;

    if (iocphandle == _INVALID_HANDLE_VALUE) {
        return new gList();
    }
    if (delay < 0) {
        wait = _INFINITE;
    }
    else if (delay == 0) {
        wait = 0;
    }
    else if (delay < 1e6F) {
        wait = 1;
    }
    else if (delay < 1e15F) {
        wait = uint32(delay / 1e6F);
    }
    else
 { 
        // An arbitrary cap on how long to wait for a timer.
        // 1e9 ms == ~11.5 days.
        wait = 1e9F;

    }
    n = uint32(len(entries) / int(gomaxprocs));
    if (n < 8) {
        n = 8;
    }
    if (delay != 0) {
        mp.blocked = true;
    }
    if (stdcall6(_GetQueuedCompletionStatusEx, iocphandle, uintptr(@unsafe.Pointer(_addr_entries[0])), uintptr(n), uintptr(@unsafe.Pointer(_addr_n)), uintptr(wait), 0) == 0) {
        mp.blocked = false;
        errno = int32(getlasterror());
        if (errno == _WAIT_TIMEOUT) {
            return new gList();
        }
        println("runtime: GetQueuedCompletionStatusEx failed (errno=", errno, ")");
        throw("runtime: netpoll failed");

    }
    mp.blocked = false;
    for (i = 0; i < n; i++) {
        op = entries[i].op;
        if (op != null) {
            errno = 0;
            qty = 0;
            if (stdcall5(_WSAGetOverlappedResult, op.pd.fd, uintptr(@unsafe.Pointer(op)), uintptr(@unsafe.Pointer(_addr_qty)), 0, uintptr(@unsafe.Pointer(_addr_flags))) == 0) {
                errno = int32(getlasterror());
            }
            handlecompletion(_addr_toRun, op, errno, qty);
        }
        else
 {
            atomic.Store(_addr_netpollWakeSig, 0);
            if (delay == 0) { 
                // Forward the notification to the
                // blocked poller.
                netpollBreak();

            }

        }
    }
    return toRun;

}

private static void handlecompletion(ptr<gList> _addr_toRun, ptr<net_op> _addr_op, int errno, uint qty) {
    ref gList toRun = ref _addr_toRun.val;
    ref net_op op = ref _addr_op.val;

    var mode = op.mode;
    if (mode != 'r' && mode != 'w') {
        println("runtime: GetQueuedCompletionStatusEx returned invalid mode=", mode);
        throw("runtime: netpoll failed");
    }
    op.errno = errno;
    op.qty = qty;
    netpollready(toRun, op.pd, mode);

}

} // end runtime_package
