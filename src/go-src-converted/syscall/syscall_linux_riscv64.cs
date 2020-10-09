// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package syscall -- go2cs converted at 2020 October 09 05:01:51 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\syscall_linux_riscv64.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class syscall_package
    {
        private static readonly var _SYS_setgroups = SYS_SETGROUPS;



        public static (long, error) EpollCreate(long size)
        {
            long fd = default;
            error err = default!;

            if (size <= 0L)
            {
                return (-1L, error.As(EINVAL)!);
            }

            return EpollCreate1(0L);

        }

        //sys    EpollWait(epfd int, events []EpollEvent, msec int) (n int, err error) = SYS_EPOLL_PWAIT
        //sys    Fchown(fd int, uid int, gid int) (err error)
        //sys    Fstat(fd int, stat *Stat_t) (err error)
        //sys    Fstatat(fd int, path string, stat *Stat_t, flags int) (err error)
        //sys    fstatat(dirfd int, path string, stat *Stat_t, flags int) (err error)
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
        //sys    renameat2(olddirfd int, oldpath string, newdirfd int, newpath string, flags uint) (err error)
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

        public static error Renameat(long olddirfd, @string oldpath, long newdirfd, @string newpath)
        {
            error err = default!;

            return error.As(renameat2(olddirfd, oldpath, newdirfd, newpath, 0L))!;
        }

        public static error Stat(@string path, ptr<Stat_t> _addr_stat)
        {
            error err = default!;
            ref Stat_t stat = ref _addr_stat.val;

            return error.As(Fstatat(_AT_FDCWD, path, stat, 0L))!;
        }

        public static error Lchown(@string path, long uid, long gid)
        {
            error err = default!;

            return error.As(Fchownat(_AT_FDCWD, path, uid, gid, _AT_SYMLINK_NOFOLLOW))!;
        }

        public static error Lstat(@string path, ptr<Stat_t> _addr_stat)
        {
            error err = default!;
            ref Stat_t stat = ref _addr_stat.val;

            return error.As(Fstatat(_AT_FDCWD, path, stat, _AT_SYMLINK_NOFOLLOW))!;
        }

        //sys    Statfs(path string, buf *Statfs_t) (err error)
        //sys    SyncFileRange(fd int, off int64, n int64, flags int) (err error)
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

        public static (long, error) Select(long nfd, ptr<FdSet> _addr_r, ptr<FdSet> _addr_w, ptr<FdSet> _addr_e, ptr<Timeval> _addr_timeout)
        {
            long n = default;
            error err = default!;
            ref FdSet r = ref _addr_r.val;
            ref FdSet w = ref _addr_w.val;
            ref FdSet e = ref _addr_e.val;
            ref Timeval timeout = ref _addr_timeout.val;

            ptr<Timespec> ts;
            if (timeout != null)
            {
                ts = addr(new Timespec(Sec:timeout.Sec,Nsec:timeout.Usec*1000));
            }

            return pselect(nfd, r, w, e, ts, null);

        }

        //sysnb    Gettimeofday(tv *Timeval) (err error)

        private static Timespec setTimespec(long sec, long nsec)
        {
            return new Timespec(Sec:sec,Nsec:nsec);
        }

        private static Timeval setTimeval(long sec, long usec)
        {
            return new Timeval(Sec:sec,Usec:usec);
        }

        private static error futimesat(long dirfd, @string path, ptr<array<Timeval>> _addr_tv)
        {
            error err = default!;
            ref array<Timeval> tv = ref _addr_tv.val;

            if (tv == null)
            {
                return error.As(utimensat(dirfd, path, null, 0L))!;
            }

            Timespec ts = new slice<Timespec>(new Timespec[] { NsecToTimespec(TimevalToNsec(tv[0])), NsecToTimespec(TimevalToNsec(tv[1])) });
            return error.As(utimensat(dirfd, path, new ptr<ptr<array<Timespec>>>(@unsafe.Pointer(_addr_ts[0L])), 0L))!;

        }

        public static (Time_t, error) Time(ptr<Time_t> _addr_t)
        {
            Time_t _p0 = default;
            error _p0 = default!;
            ref Time_t t = ref _addr_t.val;

            ref Timeval tv = ref heap(out ptr<Timeval> _addr_tv);
            var err = Gettimeofday(_addr_tv);
            if (err != null)
            {
                return (0L, error.As(err)!);
            }

            if (t != null)
            {
                t = Time_t(tv.Sec);
            }

            return (Time_t(tv.Sec), error.As(null!)!);

        }

        public static error Utime(@string path, ptr<Utimbuf> _addr_buf)
        {
            ref Utimbuf buf = ref _addr_buf.val;

            Timeval tv = new slice<Timeval>(new Timeval[] { {Sec:buf.Actime}, {Sec:buf.Modtime} });
            return error.As(Utimes(path, tv))!;
        }

        private static error utimes(@string path, ptr<array<Timeval>> _addr_tv)
        {
            error err = default!;
            ref array<Timeval> tv = ref _addr_tv.val;

            if (tv == null)
            {
                return error.As(utimensat(_AT_FDCWD, path, null, 0L))!;
            }

            Timespec ts = new slice<Timespec>(new Timespec[] { NsecToTimespec(TimevalToNsec(tv[0])), NsecToTimespec(TimevalToNsec(tv[1])) });
            return error.As(utimensat(_AT_FDCWD, path, new ptr<ptr<array<Timespec>>>(@unsafe.Pointer(_addr_ts[0L])), 0L))!;

        }

        public static error Pipe(slice<long> p)
        {
            error err = default!;

            if (len(p) != 2L)
            {
                return error.As(EINVAL)!;
            }

            ref array<_C_int> pp = ref heap(new array<_C_int>(2L), out ptr<array<_C_int>> _addr_pp);
            err = pipe2(_addr_pp, 0L);
            p[0L] = int(pp[0L]);
            p[1L] = int(pp[1L]);
            return ;

        }

        //sysnb pipe2(p *[2]_C_int, flags int) (err error)

        public static error Pipe2(slice<long> p, long flags)
        {
            error err = default!;

            if (len(p) != 2L)
            {
                return error.As(EINVAL)!;
            }

            ref array<_C_int> pp = ref heap(new array<_C_int>(2L), out ptr<array<_C_int>> _addr_pp);
            err = pipe2(_addr_pp, flags);
            p[0L] = int(pp[0L]);
            p[1L] = int(pp[1L]);
            return ;

        }

        private static ulong PC(this ptr<PtraceRegs> _addr_r)
        {
            ref PtraceRegs r = ref _addr_r.val;

            return r.Pc;
        }

        private static void SetPC(this ptr<PtraceRegs> _addr_r, ulong pc)
        {
            ref PtraceRegs r = ref _addr_r.val;

            r.Pc = pc;
        }

        private static void SetLen(this ptr<Iovec> _addr_iov, long length)
        {
            ref Iovec iov = ref _addr_iov.val;

            iov.Len = uint64(length);
        }

        private static void SetControllen(this ptr<Msghdr> _addr_msghdr, long length)
        {
            ref Msghdr msghdr = ref _addr_msghdr.val;

            msghdr.Controllen = uint64(length);
        }

        private static void SetLen(this ptr<Cmsghdr> _addr_cmsg, long length)
        {
            ref Cmsghdr cmsg = ref _addr_cmsg.val;

            cmsg.Len = uint64(length);
        }

        public static (long, error) InotifyInit()
        {
            long fd = default;
            error err = default!;

            return InotifyInit1(0L);
        }

        //sys    ppoll(fds *pollFd, nfds int, timeout *Timespec, sigmask *sigset_t) (n int, err error)

        public static error Pause()
        {
            var (_, err) = ppoll(null, 0L, null, null);
            return error.As(err)!;
        }

        private static (System.UIntPtr, Errno) rawVforkSyscall(System.UIntPtr trap, System.UIntPtr a1) => func((_, panic, __) =>
        {
            System.UIntPtr r1 = default;
            Errno err = default;

            panic("not implemented");
        });
    }
}
