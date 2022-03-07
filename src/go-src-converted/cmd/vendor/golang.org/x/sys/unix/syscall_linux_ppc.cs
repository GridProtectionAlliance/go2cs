// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build linux && ppc
// +build linux
// +build ppc

// package unix -- go2cs converted at 2022 March 06 23:27:10 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\sys\unix\syscall_linux_ppc.go
using syscall = go.syscall_package;
using @unsafe = go.@unsafe_package;

namespace go.cmd.vendor.golang.org.x.sys;

public static partial class unix_package {

    //sys    dup2(oldfd int, newfd int) (err error)
    //sysnb    EpollCreate(size int) (fd int, err error)
    //sys    EpollWait(epfd int, events []EpollEvent, msec int) (n int, err error)
    //sys    Fchown(fd int, uid int, gid int) (err error)
    //sys    Fstat(fd int, stat *Stat_t) (err error) = SYS_FSTAT64
    //sys    Fstatat(dirfd int, path string, stat *Stat_t, flags int) (err error) = SYS_FSTATAT64
    //sys    Ftruncate(fd int, length int64) (err error) = SYS_FTRUNCATE64
    //sysnb    Getegid() (egid int)
    //sysnb    Geteuid() (euid int)
    //sysnb    Getgid() (gid int)
    //sysnb    Getuid() (uid int)
    //sysnb    InotifyInit() (fd int, err error)
    //sys    Ioperm(from int, num int, on int) (err error)
    //sys    Iopl(level int) (err error)
    //sys    Lchown(path string, uid int, gid int) (err error)
    //sys    Listen(s int, n int) (err error)
    //sys    Lstat(path string, stat *Stat_t) (err error) = SYS_LSTAT64
    //sys    Pause() (err error)
    //sys    Pread(fd int, p []byte, offset int64) (n int, err error) = SYS_PREAD64
    //sys    Pwrite(fd int, p []byte, offset int64) (n int, err error) = SYS_PWRITE64
    //sys    Renameat(olddirfd int, oldpath string, newdirfd int, newpath string) (err error)
    //sys    Select(nfd int, r *FdSet, w *FdSet, e *FdSet, timeout *Timeval) (n int, err error) = SYS__NEWSELECT
    //sys    sendfile(outfd int, infd int, offset *int64, count int) (written int, err error) = SYS_SENDFILE64
    //sys    setfsgid(gid int) (prev int, err error)
    //sys    setfsuid(uid int) (prev int, err error)
    //sysnb    Setregid(rgid int, egid int) (err error)
    //sysnb    Setresgid(rgid int, egid int, sgid int) (err error)
    //sysnb    Setresuid(ruid int, euid int, suid int) (err error)
    //sysnb    Setreuid(ruid int, euid int) (err error)
    //sys    Shutdown(fd int, how int) (err error)
    //sys    Splice(rfd int, roff *int64, wfd int, woff *int64, len int, flags int) (n int, err error)
    //sys    Stat(path string, stat *Stat_t) (err error) = SYS_STAT64
    //sys    Truncate(path string, length int64) (err error) = SYS_TRUNCATE64
    //sys    Ustat(dev int, ubuf *Ustat_t) (err error)
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

    //sys    futimesat(dirfd int, path string, times *[2]Timeval) (err error)
    //sysnb    Gettimeofday(tv *Timeval) (err error)
    //sysnb    Time(t *Time_t) (tt Time_t, err error)
    //sys    Utime(path string, buf *Utimbuf) (err error)
    //sys    utimes(path string, times *[2]Timeval) (err error)
public static error Fadvise(nint fd, long offset, long length, nint advice) {
    error err = default!;

    var (_, _, e1) = Syscall6(SYS_FADVISE64_64, uintptr(fd), uintptr(advice), uintptr(offset >> 32), uintptr(offset), uintptr(length >> 32), uintptr(length));
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return ;

}

private static (long, syscall.Errno) seek(nint fd, long offset, nint whence) {
    long _p0 = default;
    syscall.Errno _p0 = default;

    ref long newoffset = ref heap(out ptr<long> _addr_newoffset);
    var offsetLow = uint32(offset & 0xffffffff);
    var offsetHigh = uint32((offset >> 32) & 0xffffffff);
    var (_, _, err) = Syscall6(SYS__LLSEEK, uintptr(fd), uintptr(offsetHigh), uintptr(offsetLow), uintptr(@unsafe.Pointer(_addr_newoffset)), uintptr(whence), 0);
    return (newoffset, err);
}

public static (long, error) Seek(nint fd, long offset, nint whence) {
    long newoffset = default;
    error err = default!;

    var (newoffset, errno) = seek(fd, offset, whence);
    if (errno != 0) {
        return (0, error.As(errno)!);
    }
    return (newoffset, error.As(null!)!);

}

public static error Fstatfs(nint fd, ptr<Statfs_t> _addr_buf) {
    error err = default!;
    ref Statfs_t buf = ref _addr_buf.val;

    var (_, _, e) = Syscall(SYS_FSTATFS64, uintptr(fd), @unsafe.Sizeof(buf), uintptr(@unsafe.Pointer(buf)));
    if (e != 0) {
        err = e;
    }
    return ;

}

public static error Statfs(@string path, ptr<Statfs_t> _addr_buf) {
    error err = default!;
    ref Statfs_t buf = ref _addr_buf.val;

    var (pathp, err) = BytePtrFromString(path);
    if (err != null) {
        return error.As(err)!;
    }
    var (_, _, e) = Syscall(SYS_STATFS64, uintptr(@unsafe.Pointer(pathp)), @unsafe.Sizeof(buf), uintptr(@unsafe.Pointer(buf)));
    if (e != 0) {
        err = e;
    }
    return ;

}

//sys    mmap2(addr uintptr, length uintptr, prot int, flags int, fd int, pageOffset uintptr) (xaddr uintptr, err error)

private static (System.UIntPtr, error) mmap(System.UIntPtr addr, System.UIntPtr length, nint prot, nint flags, nint fd, long offset) {
    System.UIntPtr xaddr = default;
    error err = default!;

    var page = uintptr(offset / 4096);
    if (offset != int64(page) * 4096) {
        return (0, error.As(EINVAL)!);
    }
    return mmap2(addr, length, prot, flags, fd, page);

}

private static Timespec setTimespec(long sec, long nsec) {
    return new Timespec(Sec:int32(sec),Nsec:int32(nsec));
}

private static Timeval setTimeval(long sec, long usec) {
    return new Timeval(Sec:int32(sec),Usec:int32(usec));
}

private partial struct rlimit32 {
    public uint Cur;
    public uint Max;
}

//sysnb    getrlimit(resource int, rlim *rlimit32) (err error) = SYS_UGETRLIMIT

private static readonly var rlimInf32 = ~uint32(0);

private static readonly var rlimInf64 = ~uint64(0);



public static error Getrlimit(nint resource, ptr<Rlimit> _addr_rlim) {
    error err = default!;
    ref Rlimit rlim = ref _addr_rlim.val;

    err = prlimit(0, resource, null, rlim);
    if (err != ENOSYS) {
        return error.As(err)!;
    }
    ref rlimit32 rl = ref heap(new rlimit32(), out ptr<rlimit32> _addr_rl);
    err = getrlimit(resource, _addr_rl);
    if (err != null) {
        return ;
    }
    if (rl.Cur == rlimInf32) {
        rlim.Cur = rlimInf64;
    }
    else
 {
        rlim.Cur = uint64(rl.Cur);
    }
    if (rl.Max == rlimInf32) {
        rlim.Max = rlimInf64;
    }
    else
 {
        rlim.Max = uint64(rl.Max);
    }
    return ;

}

//sysnb    setrlimit(resource int, rlim *rlimit32) (err error) = SYS_SETRLIMIT

public static error Setrlimit(nint resource, ptr<Rlimit> _addr_rlim) {
    error err = default!;
    ref Rlimit rlim = ref _addr_rlim.val;

    err = prlimit(0, resource, rlim, null);
    if (err != ENOSYS) {
        return error.As(err)!;
    }
    ref rlimit32 rl = ref heap(new rlimit32(), out ptr<rlimit32> _addr_rl);
    if (rlim.Cur == rlimInf64) {
        rl.Cur = rlimInf32;
    }
    else if (rlim.Cur < uint64(rlimInf32)) {
        rl.Cur = uint32(rlim.Cur);
    }
    else
 {
        return error.As(EINVAL)!;
    }
    if (rlim.Max == rlimInf64) {
        rl.Max = rlimInf32;
    }
    else if (rlim.Max < uint64(rlimInf32)) {
        rl.Max = uint32(rlim.Max);
    }
    else
 {
        return error.As(EINVAL)!;
    }
    return error.As(setrlimit(resource, _addr_rl))!;

}

private static uint PC(this ptr<PtraceRegs> _addr_r) {
    ref PtraceRegs r = ref _addr_r.val;

    return r.Nip;
}

private static void SetPC(this ptr<PtraceRegs> _addr_r, uint pc) {
    ref PtraceRegs r = ref _addr_r.val;

    r.Nip = pc;
}

private static void SetLen(this ptr<Iovec> _addr_iov, nint length) {
    ref Iovec iov = ref _addr_iov.val;

    iov.Len = uint32(length);
}

private static void SetControllen(this ptr<Msghdr> _addr_msghdr, nint length) {
    ref Msghdr msghdr = ref _addr_msghdr.val;

    msghdr.Controllen = uint32(length);
}

private static void SetIovlen(this ptr<Msghdr> _addr_msghdr, nint length) {
    ref Msghdr msghdr = ref _addr_msghdr.val;

    msghdr.Iovlen = uint32(length);
}

private static void SetLen(this ptr<Cmsghdr> _addr_cmsg, nint length) {
    ref Cmsghdr cmsg = ref _addr_cmsg.val;

    cmsg.Len = uint32(length);
}

//sysnb    pipe(p *[2]_C_int) (err error)

public static error Pipe(slice<nint> p) {
    error err = default!;

    if (len(p) != 2) {
        return error.As(EINVAL)!;
    }
    ref array<_C_int> pp = ref heap(new array<_C_int>(2), out ptr<array<_C_int>> _addr_pp);
    err = pipe(_addr_pp);
    p[0] = int(pp[0]);
    p[1] = int(pp[1]);
    return ;

}

//sysnb    pipe2(p *[2]_C_int, flags int) (err error)

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

//sys    poll(fds *PollFd, nfds int, timeout int) (n int, err error)

public static (nint, error) Poll(slice<PollFd> fds, nint timeout) {
    nint n = default;
    error err = default!;

    if (len(fds) == 0) {
        return poll(null, 0, timeout);
    }
    return poll(_addr_fds[0], len(fds), timeout);

}

//sys    syncFileRange2(fd int, flags int, off int64, n int64) (err error) = SYS_SYNC_FILE_RANGE2

public static error SyncFileRange(nint fd, long off, long n, nint flags) { 
    // The sync_file_range and sync_file_range2 syscalls differ only in the
    // order of their arguments.
    return error.As(syncFileRange2(fd, flags, off, n))!;

}

//sys    kexecFileLoad(kernelFd int, initrdFd int, cmdlineLen int, cmdline string, flags int) (err error)

public static error KexecFileLoad(nint kernelFd, nint initrdFd, @string cmdline, nint flags) {
    var cmdlineLen = len(cmdline);
    if (cmdlineLen > 0) { 
        // Account for the additional NULL byte added by
        // BytePtrFromString in kexecFileLoad. The kexec_file_load
        // syscall expects a NULL-terminated string.
        cmdlineLen++;

    }
    return error.As(kexecFileLoad(kernelFd, initrdFd, cmdlineLen, cmdline, flags))!;

}

} // end unix_package
