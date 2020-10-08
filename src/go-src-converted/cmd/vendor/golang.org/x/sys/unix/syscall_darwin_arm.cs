// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package unix -- go2cs converted at 2020 October 08 04:46:55 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\sys\unix\syscall_darwin_arm.go
using syscall = go.syscall_package;
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
        private static error ptrace(long request, long pid, System.UIntPtr addr, System.UIntPtr data)
        {
            return error.As(ENOTSUP)!;
        }

        private static Timespec setTimespec(long sec, long nsec)
        {
            return new Timespec(Sec:int32(sec),Nsec:int32(nsec));
        }

        private static Timeval setTimeval(long sec, long usec)
        {
            return new Timeval(Sec:int32(sec),Usec:int32(usec));
        }

        //sysnb    gettimeofday(tp *Timeval) (sec int32, usec int32, err error)
        public static error Gettimeofday(ptr<Timeval> _addr_tv)
        {
            error err = default!;
            ref Timeval tv = ref _addr_tv.val;
 
            // The tv passed to gettimeofday must be non-nil
            // but is otherwise unused. The answers come back
            // in the two registers.
            var (sec, usec, err) = gettimeofday(tv);
            tv.Sec = int32(sec);
            tv.Usec = int32(usec);
            return error.As(err)!;

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

        public static (System.UIntPtr, System.UIntPtr, syscall.Errno) Syscall9(System.UIntPtr num, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6, System.UIntPtr a7, System.UIntPtr a8, System.UIntPtr a9)
; // sic

        // SYS___SYSCTL is used by syscall_bsd.go for all BSDs, but in modern versions
        // of darwin/arm the syscall is called sysctl instead of __sysctl.
        public static readonly var SYS___SYSCTL = (var)SYS_SYSCTL;

        //sys    Fstat(fd int, stat *Stat_t) (err error)
        //sys    Fstatat(fd int, path string, stat *Stat_t, flags int) (err error)
        //sys    Fstatfs(fd int, stat *Statfs_t) (err error)
        //sys    getfsstat(buf unsafe.Pointer, size uintptr, flags int) (n int, err error) = SYS_GETFSSTAT
        //sys    Lstat(path string, stat *Stat_t) (err error)
        //sys    Stat(path string, stat *Stat_t) (err error)
        //sys    Statfs(path string, stat *Statfs_t) (err error)


        //sys    Fstat(fd int, stat *Stat_t) (err error)
        //sys    Fstatat(fd int, path string, stat *Stat_t, flags int) (err error)
        //sys    Fstatfs(fd int, stat *Statfs_t) (err error)
        //sys    getfsstat(buf unsafe.Pointer, size uintptr, flags int) (n int, err error) = SYS_GETFSSTAT
        //sys    Lstat(path string, stat *Stat_t) (err error)
        //sys    Stat(path string, stat *Stat_t) (err error)
        //sys    Statfs(path string, stat *Statfs_t) (err error)
    }
}}}}}}
