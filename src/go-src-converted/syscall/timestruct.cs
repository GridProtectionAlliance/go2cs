// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build aix darwin dragonfly freebsd js,wasm linux netbsd openbsd solaris

// package syscall -- go2cs converted at 2020 October 08 03:27:52 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\timestruct.go

using static go.builtin;

namespace go
{
    public static partial class syscall_package
    {
        // TimespecToNsec converts a Timespec value into a number of
        // nanoseconds since the Unix epoch.
        public static long TimespecToNsec(Timespec ts)
        {
            return ts.Nano();
        }

        // NsecToTimespec takes a number of nanoseconds since the Unix epoch
        // and returns the corresponding Timespec value.
        public static Timespec NsecToTimespec(long nsec)
        {
            var sec = nsec / 1e9F;
            nsec = nsec % 1e9F;
            if (nsec < 0L)
            {
                nsec += 1e9F;
                sec--;
            }

            return setTimespec(sec, nsec);

        }

        // TimevalToNsec converts a Timeval value into a number of nanoseconds
        // since the Unix epoch.
        public static long TimevalToNsec(Timeval tv)
        {
            return tv.Nano();
        }

        // NsecToTimeval takes a number of nanoseconds since the Unix epoch
        // and returns the corresponding Timeval value.
        public static Timeval NsecToTimeval(long nsec)
        {
            nsec += 999L; // round up to microsecond
            var usec = nsec % 1e9F / 1e3F;
            var sec = nsec / 1e9F;
            if (usec < 0L)
            {
                usec += 1e6F;
                sec--;
            }

            return setTimeval(sec, usec);

        }
    }
}
