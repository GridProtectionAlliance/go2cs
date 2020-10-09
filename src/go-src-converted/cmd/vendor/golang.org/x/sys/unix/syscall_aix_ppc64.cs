// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build aix
// +build ppc64

// package unix -- go2cs converted at 2020 October 09 05:56:23 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\sys\unix\syscall_aix_ppc64.go

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
        //sysnb    Getrlimit(resource int, rlim *Rlimit) (err error)
        //sysnb    Setrlimit(resource int, rlim *Rlimit) (err error)
        //sys    Seek(fd int, offset int64, whence int) (off int64, err error) = lseek

        //sys    mmap(addr uintptr, length uintptr, prot int, flags int, fd int, offset int64) (xaddr uintptr, err error) = mmap64
        private static Timespec setTimespec(long sec, long nsec)
        {
            return new Timespec(Sec:sec,Nsec:nsec);
        }

        private static Timeval setTimeval(long sec, long usec)
        {
            return new Timeval(Sec:int64(sec),Usec:int32(usec));
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

        // In order to only have Timespec structure, type of Stat_t's fields
        // Atim, Mtim and Ctim is changed from StTimespec to Timespec during
        // ztypes generation.
        // On ppc64, Timespec.Nsec is an int64 while StTimespec.Nsec is an
        // int32, so the fields' value must be modified.
        private static void fixStatTimFields(ptr<Stat_t> _addr_stat)
        {
            ref Stat_t stat = ref _addr_stat.val;

            stat.Atim.Nsec >>= 32L;
            stat.Mtim.Nsec >>= 32L;
            stat.Ctim.Nsec >>= 32L;
        }

        public static error Fstat(long fd, ptr<Stat_t> _addr_stat)
        {
            ref Stat_t stat = ref _addr_stat.val;

            var err = fstat(fd, stat);
            if (err != null)
            {
                return error.As(err)!;
            }

            fixStatTimFields(_addr_stat);
            return error.As(null!)!;

        }

        public static error Fstatat(long dirfd, @string path, ptr<Stat_t> _addr_stat, long flags)
        {
            ref Stat_t stat = ref _addr_stat.val;

            var err = fstatat(dirfd, path, stat, flags);
            if (err != null)
            {
                return error.As(err)!;
            }

            fixStatTimFields(_addr_stat);
            return error.As(null!)!;

        }

        public static error Lstat(@string path, ptr<Stat_t> _addr_stat)
        {
            ref Stat_t stat = ref _addr_stat.val;

            var err = lstat(path, stat);
            if (err != null)
            {
                return error.As(err)!;
            }

            fixStatTimFields(_addr_stat);
            return error.As(null!)!;

        }

        public static error Stat(@string path, ptr<Stat_t> _addr_statptr)
        {
            ref Stat_t statptr = ref _addr_statptr.val;

            var err = stat(path, statptr);
            if (err != null)
            {
                return error.As(err)!;
            }

            fixStatTimFields(_addr_statptr);
            return error.As(null!)!;

        }
    }
}}}}}}
