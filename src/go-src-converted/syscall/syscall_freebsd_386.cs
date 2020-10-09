// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package syscall -- go2cs converted at 2020 October 09 05:01:42 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\syscall_freebsd_386.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class syscall_package
    {
        private static Timespec setTimespec(long sec, long nsec)
        {
            return new Timespec(Sec:int32(sec),Nsec:int32(nsec));
        }

        private static Timeval setTimeval(long sec, long usec)
        {
            return new Timeval(Sec:int32(sec),Usec:int32(usec));
        }

        public static void SetKevent(ptr<Kevent_t> _addr_k, long fd, long mode, long flags)
        {
            ref Kevent_t k = ref _addr_k.val;

            k.Ident = uint32(fd);
            k.Filter = int16(mode);
            k.Flags = uint16(flags);
        }

        private static void SetLen(this ptr<Iovec> _addr_iov, long length)
        {
            ref Iovec iov = ref _addr_iov.val;

            iov.Len = uint32(length);
        }

        private static void SetControllen(this ptr<Msghdr> _addr_msghdr, long length)
        {
            ref Msghdr msghdr = ref _addr_msghdr.val;

            msghdr.Controllen = uint32(length);
        }

        private static void SetLen(this ptr<Cmsghdr> _addr_cmsg, long length)
        {
            ref Cmsghdr cmsg = ref _addr_cmsg.val;

            cmsg.Len = uint32(length);
        }

        private static (long, error) sendfile(long outfd, long infd, ptr<long> _addr_offset, long count)
        {
            long written = default;
            error err = default!;
            ref long offset = ref _addr_offset.val;

            ref ulong writtenOut = ref heap(0L, out ptr<ulong> _addr_writtenOut);
            var (_, _, e1) = Syscall9(SYS_SENDFILE, uintptr(infd), uintptr(outfd), uintptr(offset), uintptr((offset) >> (int)(32L)), uintptr(count), 0L, uintptr(@unsafe.Pointer(_addr_writtenOut)), 0L, 0L);

            written = int(writtenOut);

            if (e1 != 0L)
            {
                err = e1;
            }

            return ;

        }

        public static (System.UIntPtr, System.UIntPtr, Errno) Syscall9(System.UIntPtr num, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6, System.UIntPtr a7, System.UIntPtr a8, System.UIntPtr a9)
; // sic
    }
}
