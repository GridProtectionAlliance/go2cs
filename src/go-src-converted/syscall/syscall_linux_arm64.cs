// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package syscall -- go2cs converted at 2022 March 06 22:27:05 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Program Files\Go\src\syscall\syscall_linux_arm64.go
using @unsafe = go.@unsafe_package;

namespace go;

public static partial class syscall_package {

    // archHonorsR2 captures the fact that r2 is honored by the
    // runtime.GOARCH.  Syscall conventions are generally r1, r2, err :=
    // syscall(trap, ...).  Not all architectures define r2 in their
    // ABI. See "man syscall".
private static readonly var archHonorsR2 = true;



private static readonly var _SYS_setgroups = SYS_SETGROUPS;



public static (nint, error) EpollCreate(nint size) {
    nint fd = default;
    error err = default!;

    if (size <= 0) {
        return (-1, error.As(EINVAL)!);
    }
    return EpollCreate1(0);

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
//sysnb    getrlimit(resource int, rlim *Rlimit) (err error)
//sysnb    Getuid() (uid int)
//sys    Listen(s int, n int) (err error)
//sys    Pread(fd int, p []byte, offset int64) (n int, err error) = SYS_PREAD64
//sys    Pwrite(fd int, p []byte, offset int64) (n int, err error) = SYS_PWRITE64
//sys    Renameat(olddirfd int, oldpath string, newdirfd int, newpath string) (err error)
//sys    Seek(fd int, offset int64, whence int) (off int64, err error) = SYS_LSEEK
//sys    sendfile(outfd int, infd int, offset *int64, count int) (written int, err error)
//sys    Setfsgid(gid int) (err error)
//sys    Setfsuid(uid int) (err error)
//sysnb    setrlimit(resource int, rlim *Rlimit) (err error)
//sysnb    Setreuid(ruid int, euid int) (err error)
//sys    Shutdown(fd int, how int) (err error)
//sys    Splice(rfd int, roff *int64, wfd int, woff *int64, len int, flags int) (n int64, err error)

public static error Stat(@string path, ptr<Stat_t> _addr_stat) {
    error err = default!;
    ref Stat_t stat = ref _addr_stat.val;

    return error.As(Fstatat(_AT_FDCWD, path, stat, 0))!;
}

public static error Lchown(@string path, nint uid, nint gid) {
    error err = default!;

    return error.As(Fchownat(_AT_FDCWD, path, uid, gid, _AT_SYMLINK_NOFOLLOW))!;
}

public static error Lstat(@string path, ptr<Stat_t> _addr_stat) {
    error err = default!;
    ref Stat_t stat = ref _addr_stat.val;

    return error.As(Fstatat(_AT_FDCWD, path, stat, _AT_SYMLINK_NOFOLLOW))!;
}

//sys    Statfs(path string, buf *Statfs_t) (err error)
//sys    SyncFileRange(fd int, off int64, n int64, flags int) (err error) = SYS_SYNC_FILE_RANGE2
//sys    Truncate(path string, length int64) (err error)
//sys    accept(s int, rsa *RawSockaddrAny, addrlen *_Socklen) (fd int, err error)
//sys    accept4(s int, rsa *RawSockaddrAny, addrlen *_Socklen, flags int) (fd int, err error)
//sys    bind(s int, addr unsafe.Pointer, addrlen _Socklen) (err error)
//sys    connect(s int, addr unsafe.Pointer, addrlen _Socklen) (err error)
//sysnb    getgroups(n int, list *_Gid_t) (nn int, err error)
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

private partial struct sigset_t {
    public array<ulong> X__val;
}

//sys    pselect(nfd int, r *FdSet, w *FdSet, e *FdSet, timeout *Timespec, sigmask *sigset_t) (n int, err error) = SYS_PSELECT6

public static (nint, error) Select(nint nfd, ptr<FdSet> _addr_r, ptr<FdSet> _addr_w, ptr<FdSet> _addr_e, ptr<Timeval> _addr_timeout) {
    nint n = default;
    error err = default!;
    ref FdSet r = ref _addr_r.val;
    ref FdSet w = ref _addr_w.val;
    ref FdSet e = ref _addr_e.val;
    ref Timeval timeout = ref _addr_timeout.val;

    ptr<Timespec> ts;
    if (timeout != null) {
        ts = addr(new Timespec(Sec:timeout.Sec,Nsec:timeout.Usec*1000));
    }
    return pselect(nfd, r, w, e, ts, null);

}

//sysnb    Gettimeofday(tv *Timeval) (err error)

private static Timespec setTimespec(long sec, long nsec) {
    return new Timespec(Sec:sec,Nsec:nsec);
}

private static Timeval setTimeval(long sec, long usec) {
    return new Timeval(Sec:sec,Usec:usec);
}

private static error futimesat(nint dirfd, @string path, ptr<array<Timeval>> _addr_tv) {
    error err = default!;
    ref array<Timeval> tv = ref _addr_tv.val;

    if (tv == null) {
        return error.As(utimensat(dirfd, path, null, 0))!;
    }
    Timespec ts = new slice<Timespec>(new Timespec[] { NsecToTimespec(TimevalToNsec(tv[0])), NsecToTimespec(TimevalToNsec(tv[1])) });
    return error.As(utimensat(dirfd, path, new ptr<ptr<array<Timespec>>>(@unsafe.Pointer(_addr_ts[0])), 0))!;

}

public static (Time_t, error) Time(ptr<Time_t> _addr_t) {
    Time_t _p0 = default;
    error _p0 = default!;
    ref Time_t t = ref _addr_t.val;

    ref Timeval tv = ref heap(out ptr<Timeval> _addr_tv);
    var err = Gettimeofday(_addr_tv);
    if (err != null) {
        return (0, error.As(err)!);
    }
    if (t != null) {
        t = Time_t(tv.Sec);
    }
    return (Time_t(tv.Sec), error.As(null!)!);

}

public static error Utime(@string path, ptr<Utimbuf> _addr_buf) {
    ref Utimbuf buf = ref _addr_buf.val;

    Timeval tv = new slice<Timeval>(new Timeval[] { {Sec:buf.Actime}, {Sec:buf.Modtime} });
    return error.As(Utimes(path, tv))!;
}

private static error utimes(@string path, ptr<array<Timeval>> _addr_tv) {
    error err = default!;
    ref array<Timeval> tv = ref _addr_tv.val;

    if (tv == null) {
        return error.As(utimensat(_AT_FDCWD, path, null, 0))!;
    }
    Timespec ts = new slice<Timespec>(new Timespec[] { NsecToTimespec(TimevalToNsec(tv[0])), NsecToTimespec(TimevalToNsec(tv[1])) });
    return error.As(utimensat(_AT_FDCWD, path, new ptr<ptr<array<Timespec>>>(@unsafe.Pointer(_addr_ts[0])), 0))!;

}

public static error Pipe(slice<nint> p) {
    error err = default!;

    if (len(p) != 2) {
        return error.As(EINVAL)!;
    }
    ref array<_C_int> pp = ref heap(new array<_C_int>(2), out ptr<array<_C_int>> _addr_pp);
    err = pipe2(_addr_pp, 0);
    p[0] = int(pp[0]);
    p[1] = int(pp[1]);
    return ;

}

//sysnb pipe2(p *[2]_C_int, flags int) (err error)

public static error Pipe2(slice<nint> p, nint flags) {
    error err = default!;

    if (len(p) != 2) {
        return error.As(EINVAL)!;
    }
    ref array<_C_int> pp = ref heap(new array<_C_int>(2), out ptr<array<_C_int>> _addr_pp);
    err = pipe2(_addr_pp, flags);
    p[0] = int(pp[0]);
    p[1] = int(pp[1]);
    return ;

}

// Getrlimit prefers the prlimit64 system call. See issue 38604.
public static error Getrlimit(nint resource, ptr<Rlimit> _addr_rlim) {
    ref Rlimit rlim = ref _addr_rlim.val;

    var err = prlimit(0, resource, null, rlim);
    if (err != ENOSYS) {
        return error.As(err)!;
    }
    return error.As(getrlimit(resource, rlim))!;

}

// Setrlimit prefers the prlimit64 system call. See issue 38604.
public static error Setrlimit(nint resource, ptr<Rlimit> _addr_rlim) {
    ref Rlimit rlim = ref _addr_rlim.val;

    var err = prlimit(0, resource, rlim, null);
    if (err != ENOSYS) {
        return error.As(err)!;
    }
    return error.As(setrlimit(resource, rlim))!;

}

private static ulong PC(this ptr<PtraceRegs> _addr_r) {
    ref PtraceRegs r = ref _addr_r.val;

    return r.Pc;
}

private static void SetPC(this ptr<PtraceRegs> _addr_r, ulong pc) {
    ref PtraceRegs r = ref _addr_r.val;

    r.Pc = pc;
}

private static void SetLen(this ptr<Iovec> _addr_iov, nint length) {
    ref Iovec iov = ref _addr_iov.val;

    iov.Len = uint64(length);
}

private static void SetControllen(this ptr<Msghdr> _addr_msghdr, nint length) {
    ref Msghdr msghdr = ref _addr_msghdr.val;

    msghdr.Controllen = uint64(length);
}

private static void SetLen(this ptr<Cmsghdr> _addr_cmsg, nint length) {
    ref Cmsghdr cmsg = ref _addr_cmsg.val;

    cmsg.Len = uint64(length);
}

public static (nint, error) InotifyInit() {
    nint fd = default;
    error err = default!;

    return InotifyInit1(0);
}

//sys    ppoll(fds *pollFd, nfds int, timeout *Timespec, sigmask *sigset_t) (n int, err error)

public static error Pause() {
    var (_, err) = ppoll(null, 0, null, null);
    return error.As(err)!;
}

private static (System.UIntPtr, Errno) rawVforkSyscall(System.UIntPtr trap, System.UIntPtr a1);

} // end syscall_package
