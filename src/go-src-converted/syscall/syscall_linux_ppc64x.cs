// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build linux && (ppc64 || ppc64le)
// +build linux
// +build ppc64 ppc64le

// package syscall -- go2cs converted at 2022 March 06 22:27:06 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Program Files\Go\src\syscall\syscall_linux_ppc64x.go


namespace go;

public static partial class syscall_package {

    // archHonorsR2 captures the fact that r2 is honored by the
    // runtime.GOARCH.  Syscall conventions are generally r1, r2, err :=
    // syscall(trap, ...).  Not all architectures define r2 in their
    // ABI. See "man syscall".
private static readonly var archHonorsR2 = false;



private static readonly var _SYS_setgroups = SYS_SETGROUPS;

//sys    Dup2(oldfd int, newfd int) (err error)
//sysnb    EpollCreate(size int) (fd int, err error)
//sys    EpollWait(epfd int, events []EpollEvent, msec int) (n int, err error)
//sys    Fchown(fd int, uid int, gid int) (err error)
//sys    Fstat(fd int, stat *Stat_t) (err error)
//sys    fstatat(dirfd int, path string, stat *Stat_t, flags int) (err error) = SYS_NEWFSTATAT
//sys    Fstatfs(fd int, buf *Statfs_t) (err error)
//sys    Ftruncate(fd int, length int64) (err error)
//sysnb    Getegid() (egid int)
//sysnb    Geteuid() (euid int)
//sysnb    Getgid() (gid int)
//sysnb    Getrlimit(resource int, rlim *Rlimit) (err error) = SYS_UGETRLIMIT
//sysnb    Getuid() (uid int)
//sysnb    InotifyInit() (fd int, err error)
//sys    Ioperm(from int, num int, on int) (err error)
//sys    Iopl(level int) (err error)
//sys    Lchown(path string, uid int, gid int) (err error)
//sys    Listen(s int, n int) (err error)
//sys    Lstat(path string, stat *Stat_t) (err error)
//sys    Pause() (err error)
//sys    Pread(fd int, p []byte, offset int64) (n int, err error) = SYS_PREAD64
//sys    Pwrite(fd int, p []byte, offset int64) (n int, err error) = SYS_PWRITE64
//sys    Renameat(olddirfd int, oldpath string, newdirfd int, newpath string) (err error)
//sys    Seek(fd int, offset int64, whence int) (off int64, err error) = SYS_LSEEK
//sys    Select(nfd int, r *FdSet, w *FdSet, e *FdSet, timeout *Timeval) (n int, err error) = SYS__NEWSELECT
//sys    sendfile(outfd int, infd int, offset *int64, count int) (written int, err error)
//sys    Setfsgid(gid int) (err error)
//sys    Setfsuid(uid int) (err error)
//sysnb    Setrlimit(resource int, rlim *Rlimit) (err error)
//sys    Shutdown(fd int, how int) (err error)
//sys    Splice(rfd int, roff *int64, wfd int, woff *int64, len int, flags int) (n int64, err error)
//sys    Stat(path string, stat *Stat_t) (err error)
//sys    Statfs(path string, buf *Statfs_t) (err error)
//sys    Truncate(path string, length int64) (err error)
//sys    Ustat(dev int, ubuf *Ustat_t) (err error)
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

//sys    futimesat(dirfd int, path string, times *[2]Timeval) (err error)
//sysnb    Gettimeofday(tv *Timeval) (err error)
//sysnb    Time(t *Time_t) (tt Time_t, err error)
//sys    Utime(path string, buf *Utimbuf) (err error)
//sys    utimes(path string, times *[2]Timeval) (err error)



//sys    Dup2(oldfd int, newfd int) (err error)
//sysnb    EpollCreate(size int) (fd int, err error)
//sys    EpollWait(epfd int, events []EpollEvent, msec int) (n int, err error)
//sys    Fchown(fd int, uid int, gid int) (err error)
//sys    Fstat(fd int, stat *Stat_t) (err error)
//sys    fstatat(dirfd int, path string, stat *Stat_t, flags int) (err error) = SYS_NEWFSTATAT
//sys    Fstatfs(fd int, buf *Statfs_t) (err error)
//sys    Ftruncate(fd int, length int64) (err error)
//sysnb    Getegid() (egid int)
//sysnb    Geteuid() (euid int)
//sysnb    Getgid() (gid int)
//sysnb    Getrlimit(resource int, rlim *Rlimit) (err error) = SYS_UGETRLIMIT
//sysnb    Getuid() (uid int)
//sysnb    InotifyInit() (fd int, err error)
//sys    Ioperm(from int, num int, on int) (err error)
//sys    Iopl(level int) (err error)
//sys    Lchown(path string, uid int, gid int) (err error)
//sys    Listen(s int, n int) (err error)
//sys    Lstat(path string, stat *Stat_t) (err error)
//sys    Pause() (err error)
//sys    Pread(fd int, p []byte, offset int64) (n int, err error) = SYS_PREAD64
//sys    Pwrite(fd int, p []byte, offset int64) (n int, err error) = SYS_PWRITE64
//sys    Renameat(olddirfd int, oldpath string, newdirfd int, newpath string) (err error)
//sys    Seek(fd int, offset int64, whence int) (off int64, err error) = SYS_LSEEK
//sys    Select(nfd int, r *FdSet, w *FdSet, e *FdSet, timeout *Timeval) (n int, err error) = SYS__NEWSELECT
//sys    sendfile(outfd int, infd int, offset *int64, count int) (written int, err error)
//sys    Setfsgid(gid int) (err error)
//sys    Setfsuid(uid int) (err error)
//sysnb    Setrlimit(resource int, rlim *Rlimit) (err error)
//sys    Shutdown(fd int, how int) (err error)
//sys    Splice(rfd int, roff *int64, wfd int, woff *int64, len int, flags int) (n int64, err error)
//sys    Stat(path string, stat *Stat_t) (err error)
//sys    Statfs(path string, buf *Statfs_t) (err error)
//sys    Truncate(path string, length int64) (err error)
//sys    Ustat(dev int, ubuf *Ustat_t) (err error)
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

//sys    futimesat(dirfd int, path string, times *[2]Timeval) (err error)
//sysnb    Gettimeofday(tv *Timeval) (err error)
//sysnb    Time(t *Time_t) (tt Time_t, err error)
//sys    Utime(path string, buf *Utimbuf) (err error)
//sys    utimes(path string, times *[2]Timeval) (err error)

private static Timespec setTimespec(long sec, long nsec) {
    return new Timespec(Sec:sec,Nsec:nsec);
}

private static Timeval setTimeval(long sec, long usec) {
    return new Timeval(Sec:sec,Usec:usec);
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

private static ulong PC(this ptr<PtraceRegs> _addr_r) {
    ref PtraceRegs r = ref _addr_r.val;

    return r.Nip;
}

private static void SetPC(this ptr<PtraceRegs> _addr_r, ulong pc) {
    ref PtraceRegs r = ref _addr_r.val;

    r.Nip = pc;
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

private static (System.UIntPtr, Errno) rawVforkSyscall(System.UIntPtr trap, System.UIntPtr a1);

//sys    syncFileRange2(fd int, flags int, off int64, n int64) (err error) = SYS_SYNC_FILE_RANGE2

public static error SyncFileRange(nint fd, long off, long n, nint flags) { 
    // The sync_file_range and sync_file_range2 syscalls differ only in the
    // order of their arguments.
    return error.As(syncFileRange2(fd, flags, off, n))!;

}

} // end syscall_package
