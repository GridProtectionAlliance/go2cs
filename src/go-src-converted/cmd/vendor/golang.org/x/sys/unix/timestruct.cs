// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build aix || darwin || dragonfly || freebsd || linux || netbsd || openbsd || solaris || zos
// +build aix darwin dragonfly freebsd linux netbsd openbsd solaris zos

// package unix -- go2cs converted at 2022 March 13 06:41:26 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\sys\unix\timestruct.go
namespace go.cmd.vendor.golang.org.x.sys;

using time = time_package;

public static partial class unix_package {

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

// TimeToTimespec converts t into a Timespec.
// On some 32-bit systems the range of valid Timespec values are smaller
// than that of time.Time values.  So if t is out of the valid range of
// Timespec, it returns a zero Timespec and ERANGE.
public static (Timespec, error) TimeToTimespec(time.Time t) {
    Timespec _p0 = default;
    error _p0 = default!;

    var sec = t.Unix();
    var nsec = int64(t.Nanosecond());
    var ts = setTimespec(sec, nsec); 

    // Currently all targets have either int32 or int64 for Timespec.Sec.
    // If there were a new target with floating point type for it, we have
    // to consider the rounding error.
    if (int64(ts.Sec) != sec) {
        return (new Timespec(), error.As(ERANGE)!);
    }
    return (ts, error.As(null!)!);
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

// Unix returns the time stored in ts as seconds plus nanoseconds.
private static (long, long) Unix(this ptr<Timespec> _addr_ts) {
    long sec = default;
    long nsec = default;
    ref Timespec ts = ref _addr_ts.val;

    return (int64(ts.Sec), int64(ts.Nsec));
}

// Unix returns the time stored in tv as seconds plus nanoseconds.
private static (long, long) Unix(this ptr<Timeval> _addr_tv) {
    long sec = default;
    long nsec = default;
    ref Timeval tv = ref _addr_tv.val;

    return (int64(tv.Sec), int64(tv.Usec) * 1000);
}

// Nano returns the time stored in ts as nanoseconds.
private static long Nano(this ptr<Timespec> _addr_ts) {
    ref Timespec ts = ref _addr_ts.val;

    return int64(ts.Sec) * 1e9F + int64(ts.Nsec);
}

// Nano returns the time stored in tv as nanoseconds.
private static long Nano(this ptr<Timeval> _addr_tv) {
    ref Timeval tv = ref _addr_tv.val;

    return int64(tv.Sec) * 1e9F + int64(tv.Usec) * 1000;
}

} // end unix_package
