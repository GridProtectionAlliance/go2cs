// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build aix || darwin || dragonfly || freebsd || (js && wasm) || linux || netbsd || openbsd || solaris
// +build aix darwin dragonfly freebsd js,wasm linux netbsd openbsd solaris

// package socktest -- go2cs converted at 2022 March 13 05:40:14 UTC
// import "net/internal/socktest" ==> using socktest = go.net.@internal.socktest_package
// Original source: C:\Program Files\Go\src\net\internal\socktest\switch_unix.go
namespace go.net.@internal;

public static partial class socktest_package {

// Sockets maps a socket descriptor to the status of socket.
public partial struct Sockets { // : map<nint, Status>
}

private static ptr<Status> sockso(this ptr<Switch> _addr_sw, nint s) => func((defer, _, _) => {
    ref Switch sw = ref _addr_sw.val;

    sw.smu.RLock();
    defer(sw.smu.RUnlock());
    var (so, ok) = sw.sotab[s];
    if (!ok) {
        return _addr_null!;
    }
    return _addr__addr_so!;
});

// addLocked returns a new Status without locking.
// sw.smu must be held before call.
private static ptr<Status> addLocked(this ptr<Switch> _addr_sw, nint s, nint family, nint sotype, nint proto) {
    ref Switch sw = ref _addr_sw.val;

    sw.once.Do(sw.init);
    ref Status so = ref heap(new Status(Cookie:cookie(family,sotype,proto)), out ptr<Status> _addr_so);
    sw.sotab[s] = so;
    return _addr__addr_so!;
}

} // end socktest_package
