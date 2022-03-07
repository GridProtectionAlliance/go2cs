// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package syscall -- go2cs converted at 2022 March 06 22:27:07 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Program Files\Go\src\syscall\syscall_linux_s390x.go
using @unsafe = go.@unsafe_package;

namespace go;

public static partial class syscall_package {

    // archHonorsR2 captures the fact that r2 is honored by the
    // runtime.GOARCH.  Syscall conventions are generally r1, r2, err :=
    // syscall(trap, ...).  Not all architectures define r2 in their
    // ABI. See "man syscall".
private static readonly var archHonorsR2 = true;



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
//sysnb    Getrlimit(resource int, rlim *Rlimit) (err error) = SYS_GETRLIMIT
//sysnb    Getuid() (uid int)
//sysnb    InotifyInit() (fd int, err error)
//sys    Lchown(path string, uid int, gid int) (err error)
//sys    Lstat(path string, stat *Stat_t) (err error)
//sys    Pause() (err error)
//sys    Pread(fd int, p []byte, offset int64) (n int, err error) = SYS_PREAD64
//sys    Pwrite(fd int, p []byte, offset int64) (n int, err error) = SYS_PWRITE64
//sys    Renameat(olddirfd int, oldpath string, newdirfd int, newpath string) (err error)
//sys    Seek(fd int, offset int64, whence int) (off int64, err error) = SYS_LSEEK
//sys    Select(nfd int, r *FdSet, w *FdSet, e *FdSet, timeout *Timeval) (n int, err error)
//sys    sendfile(outfd int, infd int, offset *int64, count int) (written int, err error)
//sys    Setfsgid(gid int) (err error)
//sys    Setfsuid(uid int) (err error)
//sysnb    Setrlimit(resource int, rlim *Rlimit) (err error)
//sys    Splice(rfd int, roff *int64, wfd int, woff *int64, len int, flags int) (n int64, err error)
//sys    Stat(path string, stat *Stat_t) (err error)
//sys    Statfs(path string, buf *Statfs_t) (err error)
//sys    SyncFileRange(fd int, off int64, n int64, flags int) (err error) = SYS_SYNC_FILE_RANGE
//sys    Truncate(path string, length int64) (err error)
//sys    Ustat(dev int, ubuf *Ustat_t) (err error)
//sysnb    getgroups(n int, list *_Gid_t) (nn int, err error)

//sys    futimesat(dirfd int, path string, times *[2]Timeval) (err error)
//sysnb    Gettimeofday(tv *Timeval) (err error)



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
//sysnb    Getrlimit(resource int, rlim *Rlimit) (err error) = SYS_GETRLIMIT
//sysnb    Getuid() (uid int)
//sysnb    InotifyInit() (fd int, err error)
//sys    Lchown(path string, uid int, gid int) (err error)
//sys    Lstat(path string, stat *Stat_t) (err error)
//sys    Pause() (err error)
//sys    Pread(fd int, p []byte, offset int64) (n int, err error) = SYS_PREAD64
//sys    Pwrite(fd int, p []byte, offset int64) (n int, err error) = SYS_PWRITE64
//sys    Renameat(olddirfd int, oldpath string, newdirfd int, newpath string) (err error)
//sys    Seek(fd int, offset int64, whence int) (off int64, err error) = SYS_LSEEK
//sys    Select(nfd int, r *FdSet, w *FdSet, e *FdSet, timeout *Timeval) (n int, err error)
//sys    sendfile(outfd int, infd int, offset *int64, count int) (written int, err error)
//sys    Setfsgid(gid int) (err error)
//sys    Setfsuid(uid int) (err error)
//sysnb    Setrlimit(resource int, rlim *Rlimit) (err error)
//sys    Splice(rfd int, roff *int64, wfd int, woff *int64, len int, flags int) (n int64, err error)
//sys    Stat(path string, stat *Stat_t) (err error)
//sys    Statfs(path string, buf *Statfs_t) (err error)
//sys    SyncFileRange(fd int, off int64, n int64, flags int) (err error) = SYS_SYNC_FILE_RANGE
//sys    Truncate(path string, length int64) (err error)
//sys    Ustat(dev int, ubuf *Ustat_t) (err error)
//sysnb    getgroups(n int, list *_Gid_t) (nn int, err error)

//sys    futimesat(dirfd int, path string, times *[2]Timeval) (err error)
//sysnb    Gettimeofday(tv *Timeval) (err error)

public static (Time_t, error) Time(ptr<Time_t> _addr_t) {
    Time_t tt = default;
    error err = default!;
    ref Time_t t = ref _addr_t.val;

    ref Timeval tv = ref heap(out ptr<Timeval> _addr_tv);
    err = Gettimeofday(_addr_tv);
    if (err != null) {
        return (0, error.As(err)!);
    }
    if (t != null) {
        t = Time_t(tv.Sec);
    }
    return (Time_t(tv.Sec), error.As(null!)!);

}

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

// Linux on s390x uses the old mmap interface, which requires arguments to be passed in a struct.
// mmap2 also requires arguments to be passed in a struct; it is currently not exposed in <asm/unistd.h>.
private static (System.UIntPtr, error) mmap(System.UIntPtr addr, System.UIntPtr length, nint prot, nint flags, nint fd, long offset) {
    System.UIntPtr xaddr = default;
    error err = default!;

    array<System.UIntPtr> mmap_args = new array<System.UIntPtr>(new System.UIntPtr[] { addr, length, uintptr(prot), uintptr(flags), uintptr(fd), uintptr(offset) });
    var (r0, _, e1) = Syscall(SYS_MMAP, uintptr(@unsafe.Pointer(_addr_mmap_args[0])), 0, 0);
    xaddr = uintptr(r0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return ;

}

// On s390x Linux, all the socket calls go through an extra indirection.
// The arguments to the underlying system call are the number below
// and a pointer to an array of uintptr.  We hide the pointer in the
// socketcall assembly to avoid allocation on every system call.

 
// see linux/net.h
private static readonly nint _SOCKET = 1;
private static readonly nint _BIND = 2;
private static readonly nint _CONNECT = 3;
private static readonly nint _LISTEN = 4;
private static readonly nint _ACCEPT = 5;
private static readonly nint _GETSOCKNAME = 6;
private static readonly nint _GETPEERNAME = 7;
private static readonly nint _SOCKETPAIR = 8;
private static readonly nint _SEND = 9;
private static readonly nint _RECV = 10;
private static readonly nint _SENDTO = 11;
private static readonly nint _RECVFROM = 12;
private static readonly nint _SHUTDOWN = 13;
private static readonly nint _SETSOCKOPT = 14;
private static readonly nint _GETSOCKOPT = 15;
private static readonly nint _SENDMSG = 16;
private static readonly nint _RECVMSG = 17;
private static readonly nint _ACCEPT4 = 18;
private static readonly nint _RECVMMSG = 19;
private static readonly nint _SENDMMSG = 20;


private static (nint, Errno) socketcall(nint call, System.UIntPtr a0, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5);
private static (nint, Errno) rawsocketcall(nint call, System.UIntPtr a0, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5);

private static (nint, error) accept(nint s, ptr<RawSockaddrAny> _addr_rsa, ptr<_Socklen> _addr_addrlen) {
    nint fd = default;
    error err = default!;
    ref RawSockaddrAny rsa = ref _addr_rsa.val;
    ref _Socklen addrlen = ref _addr_addrlen.val;

    var (fd, e) = socketcall(_ACCEPT, uintptr(s), uintptr(@unsafe.Pointer(rsa)), uintptr(@unsafe.Pointer(addrlen)), 0, 0, 0);
    if (e != 0) {>>MARKER:FUNCTION_rawsocketcall_BLOCK_PREFIX<<
        err = e;
    }
    return ;

}

private static (nint, error) accept4(nint s, ptr<RawSockaddrAny> _addr_rsa, ptr<_Socklen> _addr_addrlen, nint flags) {
    nint fd = default;
    error err = default!;
    ref RawSockaddrAny rsa = ref _addr_rsa.val;
    ref _Socklen addrlen = ref _addr_addrlen.val;

    var (fd, e) = socketcall(_ACCEPT4, uintptr(s), uintptr(@unsafe.Pointer(rsa)), uintptr(@unsafe.Pointer(addrlen)), uintptr(flags), 0, 0);
    if (e != 0) {>>MARKER:FUNCTION_socketcall_BLOCK_PREFIX<<
        err = e;
    }
    return ;

}

private static error getsockname(nint s, ptr<RawSockaddrAny> _addr_rsa, ptr<_Socklen> _addr_addrlen) {
    error err = default!;
    ref RawSockaddrAny rsa = ref _addr_rsa.val;
    ref _Socklen addrlen = ref _addr_addrlen.val;

    var (_, e) = rawsocketcall(_GETSOCKNAME, uintptr(s), uintptr(@unsafe.Pointer(rsa)), uintptr(@unsafe.Pointer(addrlen)), 0, 0, 0);
    if (e != 0) {
        err = e;
    }
    return ;

}

private static error getpeername(nint s, ptr<RawSockaddrAny> _addr_rsa, ptr<_Socklen> _addr_addrlen) {
    error err = default!;
    ref RawSockaddrAny rsa = ref _addr_rsa.val;
    ref _Socklen addrlen = ref _addr_addrlen.val;

    var (_, e) = rawsocketcall(_GETPEERNAME, uintptr(s), uintptr(@unsafe.Pointer(rsa)), uintptr(@unsafe.Pointer(addrlen)), 0, 0, 0);
    if (e != 0) {
        err = e;
    }
    return ;

}

private static error socketpair(nint domain, nint typ, nint flags, ptr<array<int>> _addr_fd) {
    error err = default!;
    ref array<int> fd = ref _addr_fd.val;

    var (_, e) = rawsocketcall(_SOCKETPAIR, uintptr(domain), uintptr(typ), uintptr(flags), uintptr(@unsafe.Pointer(fd)), 0, 0);
    if (e != 0) {
        err = e;
    }
    return ;

}

private static error bind(nint s, unsafe.Pointer addr, _Socklen addrlen) {
    error err = default!;

    var (_, e) = socketcall(_BIND, uintptr(s), uintptr(addr), uintptr(addrlen), 0, 0, 0);
    if (e != 0) {
        err = e;
    }
    return ;

}

private static error connect(nint s, unsafe.Pointer addr, _Socklen addrlen) {
    error err = default!;

    var (_, e) = socketcall(_CONNECT, uintptr(s), uintptr(addr), uintptr(addrlen), 0, 0, 0);
    if (e != 0) {
        err = e;
    }
    return ;

}

private static (nint, error) socket(nint domain, nint typ, nint proto) {
    nint fd = default;
    error err = default!;

    var (fd, e) = rawsocketcall(_SOCKET, uintptr(domain), uintptr(typ), uintptr(proto), 0, 0, 0);
    if (e != 0) {
        err = e;
    }
    return ;

}

private static error getsockopt(nint s, nint level, nint name, unsafe.Pointer val, ptr<_Socklen> _addr_vallen) {
    error err = default!;
    ref _Socklen vallen = ref _addr_vallen.val;

    var (_, e) = socketcall(_GETSOCKOPT, uintptr(s), uintptr(level), uintptr(name), uintptr(val), uintptr(@unsafe.Pointer(vallen)), 0);
    if (e != 0) {
        err = e;
    }
    return ;

}

private static error setsockopt(nint s, nint level, nint name, unsafe.Pointer val, System.UIntPtr vallen) {
    error err = default!;

    var (_, e) = socketcall(_SETSOCKOPT, uintptr(s), uintptr(level), uintptr(name), uintptr(val), vallen, 0);
    if (e != 0) {
        err = e;
    }
    return ;

}

private static (nint, error) recvfrom(nint s, slice<byte> p, nint flags, ptr<RawSockaddrAny> _addr_from, ptr<_Socklen> _addr_fromlen) {
    nint n = default;
    error err = default!;
    ref RawSockaddrAny from = ref _addr_from.val;
    ref _Socklen fromlen = ref _addr_fromlen.val;

    System.UIntPtr @base = default;
    if (len(p) > 0) {
        base = uintptr(@unsafe.Pointer(_addr_p[0]));
    }
    var (n, e) = socketcall(_RECVFROM, uintptr(s), base, uintptr(len(p)), uintptr(flags), uintptr(@unsafe.Pointer(from)), uintptr(@unsafe.Pointer(fromlen)));
    if (e != 0) {
        err = e;
    }
    return ;

}

private static error sendto(nint s, slice<byte> p, nint flags, unsafe.Pointer to, _Socklen addrlen) {
    error err = default!;

    System.UIntPtr @base = default;
    if (len(p) > 0) {
        base = uintptr(@unsafe.Pointer(_addr_p[0]));
    }
    var (_, e) = socketcall(_SENDTO, uintptr(s), base, uintptr(len(p)), uintptr(flags), uintptr(to), uintptr(addrlen));
    if (e != 0) {
        err = e;
    }
    return ;

}

private static (nint, error) recvmsg(nint s, ptr<Msghdr> _addr_msg, nint flags) {
    nint n = default;
    error err = default!;
    ref Msghdr msg = ref _addr_msg.val;

    var (n, e) = socketcall(_RECVMSG, uintptr(s), uintptr(@unsafe.Pointer(msg)), uintptr(flags), 0, 0, 0);
    if (e != 0) {
        err = e;
    }
    return ;

}

private static (nint, error) sendmsg(nint s, ptr<Msghdr> _addr_msg, nint flags) {
    nint n = default;
    error err = default!;
    ref Msghdr msg = ref _addr_msg.val;

    var (n, e) = socketcall(_SENDMSG, uintptr(s), uintptr(@unsafe.Pointer(msg)), uintptr(flags), 0, 0, 0);
    if (e != 0) {
        err = e;
    }
    return ;

}

public static error Listen(nint s, nint n) {
    error err = default!;

    var (_, e) = socketcall(_LISTEN, uintptr(s), uintptr(n), 0, 0, 0, 0);
    if (e != 0) {
        err = e;
    }
    return ;

}

public static error Shutdown(nint s, nint how) {
    error err = default!;

    var (_, e) = socketcall(_SHUTDOWN, uintptr(s), uintptr(how), 0, 0, 0, 0);
    if (e != 0) {
        err = e;
    }
    return ;

}

private static ulong PC(this ptr<PtraceRegs> _addr_r) {
    ref PtraceRegs r = ref _addr_r.val;

    return r.Psw.Addr;
}

private static void SetPC(this ptr<PtraceRegs> _addr_r, ulong pc) {
    ref PtraceRegs r = ref _addr_r.val;

    r.Psw.Addr = pc;
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

} // end syscall_package
