// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build aix || darwin || dragonfly || freebsd || (js && wasm) || linux || netbsd || openbsd || solaris
// +build aix darwin dragonfly freebsd js,wasm linux netbsd openbsd solaris

// package syscall -- go2cs converted at 2022 March 06 22:27:18 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Program Files\Go\src\syscall\timestruct.go


namespace go;

public static partial class syscall_package {

    // TimespecToNSec returns the time stored in ts as nanoseconds.
public static long TimespecToNsec(Timespec ts) {
    return ts.Nano();
}

// NsecToTimespec converts a number of nanoseconds into a Timespec.
public static Timespec NsecToTimespec(long nsec) {
    var sec = nsec / 1e9F;
    nsec = nsec % 1e9F;
    if (nsec < 0) {
        nsec += 1e9F;
        sec--;
    }
    return setTimespec(sec, nsec);

}

// TimevalToNsec returns the time stored in tv as nanoseconds.
public static long TimevalToNsec(Timeval tv) {
    return tv.Nano();
}

// NsecToTimeval converts a number of nanoseconds into a Timeval.
public static Timeval NsecToTimeval(long nsec) {
    nsec += 999; // round up to microsecond
    var usec = nsec % 1e9F / 1e3F;
    var sec = nsec / 1e9F;
    if (usec < 0) {
        usec += 1e6F;
        sec--;
    }
    return setTimeval(sec, usec);

}

} // end syscall_package
