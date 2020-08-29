// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package syscall -- go2cs converted at 2020 August 29 08:38:21 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\syscall_solaris_amd64.go

using static go.builtin;

namespace go
{
    public static partial class syscall_package
    {
        private static Timespec setTimespec(long sec, long nsec)
        {
            return new Timespec(Sec:sec,Nsec:nsec);
        }

        private static Timeval setTimeval(long sec, long usec)
        {
            return new Timeval(Sec:sec,Usec:usec);
        }

        private static void SetLen(this ref Iovec iov, long length)
        {
            iov.Len = uint64(length);
        }

        private static void SetLen(this ref Cmsghdr cmsg, long length)
        {
            cmsg.Len = uint32(length);
        }
    }
}
