// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package syscall -- go2cs converted at 2020 August 29 08:37:48 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\syscall_darwin_amd64.go
using @unsafe = go.@unsafe_package;
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
            return new Timeval(Sec:sec,Usec:int32(usec));
        }

        //sysnb    gettimeofday(tp *Timeval) (sec int64, usec int32, err error)
        public static error Gettimeofday(ref Timeval tv)
        { 
            // The tv passed to gettimeofday must be non-nil.
            // Before macOS Sierra (10.12), tv was otherwise unused and
            // the answers came back in the two registers.
            // As of Sierra, gettimeofday return zeros and populates
            // tv itself.
            var (sec, usec, err) = gettimeofday(tv);
            if (err != null)
            {
                return error.As(err);
            }
            if (sec != 0L || usec != 0L)
            {
                tv.Sec = sec;
                tv.Usec = usec;
            }
            return error.As(null);
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

        private static (long, error) sendfile(long outfd, long infd, ref long offset, long count)
        {
            var length = uint64(count);

            var (_, _, e1) = Syscall6(SYS_SENDFILE, uintptr(infd), uintptr(outfd), uintptr(offset.Value), uintptr(@unsafe.Pointer(ref length)), 0L, 0L);

            written = int(length);

            if (e1 != 0L)
            {
                err = e1;
            }
            return;
        }

        public static (System.UIntPtr, System.UIntPtr, Errno) Syscall9(System.UIntPtr trap, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6, System.UIntPtr a7, System.UIntPtr a8, System.UIntPtr a9)
;
    }
}
