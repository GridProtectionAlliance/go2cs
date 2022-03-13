// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build plan9
// +build plan9

// package runtime -- go2cs converted at 2022 March 13 05:26:04 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\netpoll_stub.go
namespace go;

using atomic = runtime.@internal.atomic_package;

public static partial class runtime_package {

private static uint netpollInited = default;
private static uint netpollWaiters = default;

private static mutex netpollStubLock = default;
private static note netpollNote = default;

// netpollBroken, protected by netpollBrokenLock, avoids a double notewakeup.
private static mutex netpollBrokenLock = default;
private static bool netpollBroken = default;

private static void netpollGenericInit() {
    atomic.Store(_addr_netpollInited, 1);
}

private static void netpollBreak() {
    lock(_addr_netpollBrokenLock);
    var broken = netpollBroken;
    netpollBroken = true;
    if (!broken) {
        notewakeup(_addr_netpollNote);
    }
    unlock(_addr_netpollBrokenLock);
}

// Polls for ready network connections.
// Returns list of goroutines that become runnable.
private static gList netpoll(long delay) { 
    // Implementation for platforms that do not support
    // integrated network poller.
    if (delay != 0) { 
        // This lock ensures that only one goroutine tries to use
        // the note. It should normally be completely uncontended.
        lock(_addr_netpollStubLock);

        lock(_addr_netpollBrokenLock);
        noteclear(_addr_netpollNote);
        netpollBroken = false;
        unlock(_addr_netpollBrokenLock);

        notetsleep(_addr_netpollNote, delay);
        unlock(_addr_netpollStubLock); 
        // Guard against starvation in case the lock is contended
        // (eg when running TestNetpollBreak).
        osyield();
    }
    return new gList();
}

private static bool netpollinited() {
    return atomic.Load(_addr_netpollInited) != 0;
}

} // end runtime_package
