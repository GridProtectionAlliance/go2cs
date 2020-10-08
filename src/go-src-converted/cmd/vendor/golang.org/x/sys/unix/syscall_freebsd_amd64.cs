// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build amd64,freebsd

// package unix -- go2cs converted at 2020 October 08 04:47:09 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\sys\unix\syscall_freebsd_amd64.go
using syscall = go.syscall_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace vendor {
namespace golang.org {
namespace x {
namespace sys
{
    public static partial class unix_package
    {
        private static Timespec setTimespec(long sec, long nsec)
        {
            return new Timespec(Sec:sec,Nsec:nsec);
        }

        private static Timeval setTimeval(long sec, long usec)
        {
            return new Timeval(Sec:sec,Usec:usec);
        }

        public static void SetKevent(ptr<Kevent_t> _addr_k, long fd, long mode, long flags)
        {
            ref Kevent_t k = ref _addr_k.val;

            k.Ident = uint64(fd);
            k.Filter = int16(mode);
            k.Flags = uint16(flags);
        }

        private static void SetLen(this ptr<Iovec> _addr_iov, long length)
        {
            ref Iovec iov = ref _addr_iov.val;

            iov.Len = uint64(length);
        }

        private static void SetControllen(this ptr<Msghdr> _addr_msghdr, long length)
        {
            ref Msghdr msghdr = ref _addr_msghdr.val;

            msghdr.Controllen = uint32(length);
        }

        private static void SetIovlen(this ptr<Msghdr> _addr_msghdr, long length)
        {
            ref Msghdr msghdr = ref _addr_msghdr.val;

            msghdr.Iovlen = int32(length);
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
            var (_, _, e1) = Syscall9(SYS_SENDFILE, uintptr(infd), uintptr(outfd), uintptr(offset), uintptr(count), 0L, uintptr(@unsafe.Pointer(_addr_writtenOut)), 0L, 0L, 0L);

            written = int(writtenOut);

            if (e1 != 0L)
            {
                err = e1;
            }

            return ;

        }

        public static (System.UIntPtr, System.UIntPtr, syscall.Errno) Syscall9(System.UIntPtr num, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6, System.UIntPtr a7, System.UIntPtr a8, System.UIntPtr a9)
;

        public static error PtraceGetFsBase(long pid, ptr<long> _addr_fsbase)
        {
            error err = default!;
            ref long fsbase = ref _addr_fsbase.val;

            return error.As(ptrace(PTRACE_GETFSBASE, pid, uintptr(@unsafe.Pointer(fsbase)), 0L))!;
        }

        public static (long, error) PtraceIO(long req, long pid, System.UIntPtr addr, slice<byte> @out, long countin)
        {
            long count = default;
            error err = default!;

            ref PtraceIoDesc ioDesc = ref heap(new PtraceIoDesc(Op:int32(req),Offs:(*byte)(unsafe.Pointer(addr)),Addr:(*byte)(unsafe.Pointer(&out[0])),Len:uint64(countin)), out ptr<PtraceIoDesc> _addr_ioDesc);
            err = ptrace(PTRACE_IO, pid, uintptr(@unsafe.Pointer(_addr_ioDesc)), 0L);
            return (int(ioDesc.Len), error.As(err)!);
        }
    }
}}}}}}
