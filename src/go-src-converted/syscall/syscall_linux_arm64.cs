// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package syscall -- go2cs converted at 2020 August 29 08:38:07 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\syscall_linux_arm64.go

using static go.builtin;

namespace go
{
    public static partial class syscall_package
    {
        private static readonly var _SYS_dup = SYS_DUP3;
        private static readonly var _SYS_setgroups = SYS_SETGROUPS;

        //sys    Fchown(fd int, uid int, gid int) (err error)
        //sys    Fstat(fd int, stat *Stat_t) (err error)
        //sys    Fstatat(fd int, path string, stat *Stat_t, flags int) (err error)
        //sys    Fstatfs(fd int, buf *Statfs_t) (err error)
        //sys    Ftruncate(fd int, length int64) (err error)
        //sysnb    Getegid() (egid int)
        //sysnb    Geteuid() (euid int)
        //sysnb    Getgid() (gid int)
        //sysnb    Getrlimit(resource int, rlim *Rlimit) (err error)
        //sysnb    Getuid() (uid int)
        //sys    Listen(s int, n int) (err error)
        //sys    Pread(fd int, p []byte, offset int64) (n int, err error) = SYS_PREAD64
        //sys    Pwrite(fd int, p []byte, offset int64) (n int, err error) = SYS_PWRITE64
        //sys    Seek(fd int, offset int64, whence int) (off int64, err error) = SYS_LSEEK
        //sys    sendfile(outfd int, infd int, offset *int64, count int) (written int, err error)
        //sys    Setfsgid(gid int) (err error)
        //sys    Setfsuid(uid int) (err error)
        //sysnb    Setregid(rgid int, egid int) (err error)
        //sysnb    Setresgid(rgid int, egid int, sgid int) (err error)
        //sysnb    Setresuid(ruid int, euid int, suid int) (err error)
        //sysnb    Setrlimit(resource int, rlim *Rlimit) (err error)
        //sysnb    Setreuid(ruid int, euid int) (err error)
        //sys    Shutdown(fd int, how int) (err error)
        //sys    Splice(rfd int, roff *int64, wfd int, woff *int64, len int, flags int) (n int64, err error)

        public static error Stat(@string path, ref Stat_t stat)
        {
            return error.As(Fstatat(_AT_FDCWD, path, stat, 0L));
        }

        public static error Lchown(@string path, long uid, long gid)
        {
            return error.As(Fchownat(_AT_FDCWD, path, uid, gid, _AT_SYMLINK_NOFOLLOW));
        }

        public static error Lstat(@string path, ref Stat_t stat)
        {
            return error.As(Fstatat(_AT_FDCWD, path, stat, _AT_SYMLINK_NOFOLLOW));
        }

        //sys    Statfs(path string, buf *Statfs_t) (err error)
        //sys    SyncFileRange(fd int, off int64, n int64, flags int) (err error) = SYS_SYNC_FILE_RANGE2
        //sys    Truncate(path string, length int64) (err error)
        //sys    accept(s int, rsa *RawSockaddrAny, addrlen *_Socklen) (fd int, err error)
        //sys    accept4(s int, rsa *RawSockaddrAny, addrlen *_Socklen, flags int) (fd int, err error)
        //sys    bind(s int, addr unsafe.Pointer, addrlen _Socklen) (err error)
        //sys    connect(s int, addr unsafe.Pointer, addrlen _Socklen) (err error)
        //sysnb    getgroups(n int, list *_Gid_t) (nn int, err error)
        //sysnb    setgroups(n int, list *_Gid_t) (err error)
        //sys    getsockopt(s int, level int, name int, val unsafe.Pointer, vallen *_Socklen) (err error)
        //sys    setsockopt(s int, level int, name int, val unsafe.Pointer, vallen uintptr) (err error)
        //sysnb    socket(domain int, typ int, proto int) (fd int, err error)
        //sysnb    socketpair(domain int, typ int, proto int, fd *[2]int32) (err error)
        //sysnb    getpeername(fd int, rsa *RawSockaddrAny, addrlen *_Socklen) (err error)
        //sysnb    getsockname(fd int, rsa *RawSockaddrAny, addrlen *_Socklen) (err error)
        //sys    recvfrom(fd int, p []byte, flags int, from *RawSockaddrAny, fromlen *_Socklen) (n int, err error)
        //sys    sendto(s int, buf []byte, flags int, to unsafe.Pointer, addrlen _Socklen) (err error)
        //sys    recvmsg(s int, msg *Msghdr, flags int) (n int, err error)
        //sys    sendmsg(s int, msg *Msghdr, flags int) (n int, err error)
        //sys    mmap(addr uintptr, length uintptr, prot int, flags int, fd int, offset int64) (xaddr uintptr, err error)

        private partial struct sigset_t
        {
            public array<ulong> X__val;
        }

        //sys    pselect(nfd int, r *FdSet, w *FdSet, e *FdSet, timeout *Timespec, sigmask *sigset_t) (n int, err error) = SYS_PSELECT6

        public static (long, error) Select(long nfd, ref FdSet r, ref FdSet w, ref FdSet e, ref Timeval timeout)
        {
            Timespec ts = new Timespec(Sec:timeout.Sec,Nsec:timeout.Usec*1000);
            return pselect(nfd, r, w, e, ref ts, null);
        }

        //sysnb    Gettimeofday(tv *Timeval) (err error)
        //sysnb    Time(t *Time_t) (tt Time_t, err error)

        private static Timespec setTimespec(long sec, long nsec)
        {
            return new Timespec(Sec:sec,Nsec:nsec);
        }

        private static Timeval setTimeval(long sec, long usec)
        {
            return new Timeval(Sec:sec,Usec:usec);
        }

        public static error Pipe(slice<long> p)
        {
            if (len(p) != 2L)
            {
                return error.As(EINVAL);
            }
            array<_C_int> pp = new array<_C_int>(2L);
            err = pipe2(ref pp, 0L);
            p[0L] = int(pp[0L]);
            p[1L] = int(pp[1L]);
            return;
        }

        //sysnb pipe2(p *[2]_C_int, flags int) (err error)

        public static error Pipe2(slice<long> p, long flags)
        {
            if (len(p) != 2L)
            {
                return error.As(EINVAL);
            }
            array<_C_int> pp = new array<_C_int>(2L);
            err = pipe2(ref pp, flags);
            p[0L] = int(pp[0L]);
            p[1L] = int(pp[1L]);
            return;
        }

        private static ulong PC(this ref PtraceRegs r)
        {
            return r.Pc;
        }

        private static void SetPC(this ref PtraceRegs r, ulong pc)
        {
            r.Pc = pc;

        }

        private static void SetLen(this ref Iovec iov, long length)
        {
            iov.Len = uint64(length);
        }

        private static void SetControllen(this ref Msghdr msghdr, long length)
        {
            msghdr.Controllen = uint64(length);
        }

        private static void SetLen(this ref Cmsghdr cmsg, long length)
        {
            cmsg.Len = uint64(length);
        }

        public static (long, error) InotifyInit()
        {
            return InotifyInit1(0L);
        }

        // TODO(dfc): constants that should be in zsysnum_linux_arm64.go, remove
        // these when the deprecated syscalls that the syscall package relies on
        // are removed.
        public static readonly long SYS_GETPGRP = 1060L;
        public static readonly long SYS_UTIMES = 1037L;
        public static readonly long SYS_FUTIMESAT = 1066L;
        public static readonly long SYS_PAUSE = 1061L;
        public static readonly long SYS_USTAT = 1070L;
        public static readonly long SYS_UTIME = 1063L;
        public static readonly long SYS_LCHOWN = 1032L;
        public static readonly long SYS_TIME = 1062L;
        public static readonly long SYS_EPOLL_CREATE = 1042L;
        public static readonly long SYS_EPOLL_WAIT = 1069L;

        private static (System.UIntPtr, Errno) rawVforkSyscall(System.UIntPtr trap, System.UIntPtr a1) => func((_, panic, __) =>
        {
            panic("not implemented");
        });
    }
}
