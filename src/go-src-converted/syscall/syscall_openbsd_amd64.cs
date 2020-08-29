// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package syscall -- go2cs converted at 2020 August 29 08:38:16 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\syscall_openbsd_amd64.go

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

        public static void SetKevent(ref Kevent_t k, long fd, long mode, long flags)
        {
            k.Ident = uint64(fd);
            k.Filter = int16(mode);
            k.Flags = uint16(flags);
        }

        private static void SetLen(this ref Iovec iov, long length)
        {
            iov.Len = uint64(length);
        }

        private static void SetControllen(this ref Msghdr msghdr, long length)
        {
            msghdr.Controllen = uint32(length);
        }

        private static void SetLen(this ref Cmsghdr cmsg, long length)
        {
            cmsg.Len = uint32(length);
        }
    }
}
