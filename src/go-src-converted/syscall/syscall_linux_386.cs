// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package syscall -- go2cs converted at 2022 March 06 22:27:03 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Program Files\Go\src\syscall\syscall_linux_386.go
using @unsafe = go.@unsafe_package;

namespace go;

public static partial class syscall_package {

    // archHonorsR2 captures the fact that r2 is honored by the
    // runtime.GOARCH.  Syscall conventions are generally r1, r2, err :=
    // syscall(trap, ...).  Not all architectures define r2 in their
    // ABI. See "man syscall".
private static readonly var archHonorsR2 = true;



private static readonly var _SYS_setgroups = SYS_SETGROUPS32;



private static Timespec setTimespec(long sec, long nsec) {
    return new Timespec(Sec:int32(sec),Nsec:int32(nsec));
}

private static Timeval setTimeval(long sec, long usec) {
    return new Timeval(Sec:int32(sec),Usec:int32(usec));
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

// 64-bit file system and 32-bit uid calls
// (386 default is 32-bit file system and 16-bit uid).
//sys    Dup2(oldfd int, newfd int) (err error)
//sysnb    EpollCreate(size int) (fd int, err error)
//sys    Fchown(fd int, uid int, gid int) (err error) = SYS_FCHOWN32
//sys    Fstat(fd int, stat *Stat_t) (err error) = SYS_FSTAT64
//sys    fstatat(dirfd int, path string, stat *Stat_t, flags int) (err error) = SYS_FSTATAT64
//sys    Ftruncate(fd int, length int64) (err error) = SYS_FTRUNCATE64
//sysnb    Getegid() (egid int) = SYS_GETEGID32
//sysnb    Geteuid() (euid int) = SYS_GETEUID32
//sysnb    Getgid() (gid int) = SYS_GETGID32
//sysnb    Getuid() (uid int) = SYS_GETUID32
//sysnb    InotifyInit() (fd int, err error)
//sys    Ioperm(from int, num int, on int) (err error)
//sys    Iopl(level int) (err error)
//sys    Pause() (err error)
//sys    Pread(fd int, p []byte, offset int64) (n int, err error) = SYS_PREAD64
//sys    Pwrite(fd int, p []byte, offset int64) (n int, err error) = SYS_PWRITE64
//sys    Renameat(olddirfd int, oldpath string, newdirfd int, newpath string) (err error)
//sys    sendfile(outfd int, infd int, offset *int64, count int) (written int, err error) = SYS_SENDFILE64
//sys    Setfsgid(gid int) (err error) = SYS_SETFSGID32
//sys    Setfsuid(uid int) (err error) = SYS_SETFSUID32
//sys    Splice(rfd int, roff *int64, wfd int, woff *int64, len int, flags int) (n int, err error)
//sys    SyncFileRange(fd int, off int64, n int64, flags int) (err error)
//sys    Truncate(path string, length int64) (err error) = SYS_TRUNCATE64
//sys    Ustat(dev int, ubuf *Ustat_t) (err error)
//sysnb    getgroups(n int, list *_Gid_t) (nn int, err error) = SYS_GETGROUPS32
//sys    Select(nfd int, r *FdSet, w *FdSet, e *FdSet, timeout *Timeval) (n int, err error) = SYS__NEWSELECT

//sys    mmap2(addr uintptr, length uintptr, prot int, flags int, fd int, pageOffset uintptr) (xaddr uintptr, err error)
//sys    EpollWait(epfd int, events []EpollEvent, msec int) (n int, err error)

public static error Stat(@string path, ptr<Stat_t> _addr_stat) {
    error err = default!;
    ref Stat_t stat = ref _addr_stat.val;

    return error.As(fstatat(_AT_FDCWD, path, stat, 0))!;
}

public static error Lchown(@string path, nint uid, nint gid) {
    error err = default!;

    return error.As(Fchownat(_AT_FDCWD, path, uid, gid, _AT_SYMLINK_NOFOLLOW))!;
}

public static error Lstat(@string path, ptr<Stat_t> _addr_stat) {
    error err = default!;
    ref Stat_t stat = ref _addr_stat.val;

    return error.As(fstatat(_AT_FDCWD, path, stat, _AT_SYMLINK_NOFOLLOW))!;
}

private static (System.UIntPtr, error) mmap(System.UIntPtr addr, System.UIntPtr length, nint prot, nint flags, nint fd, long offset) {
    System.UIntPtr xaddr = default;
    error err = default!;

    var page = uintptr(offset / 4096);
    if (offset != int64(page) * 4096) {
        return (0, error.As(EINVAL)!);
    }
    return mmap2(addr, length, prot, flags, fd, page);

}

private partial struct rlimit32 {
    public uint Cur;
    public uint Max;
}

//sysnb getrlimit(resource int, rlim *rlimit32) (err error) = SYS_GETRLIMIT

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

//sysnb setrlimit(resource int, rlim *rlimit32) (err error) = SYS_SETRLIMIT

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

// Underlying system call writes to newoffset via pointer.
// Implemented in assembly to avoid allocation.
private static (long, Errno) seek(nint fd, long offset, nint whence);

public static (long, error) Seek(nint fd, long offset, nint whence) {
    long newoffset = default;
    error err = default!;

    var (newoffset, errno) = seek(fd, offset, whence);
    if (errno != 0) {>>MARKER:FUNCTION_seek_BLOCK_PREFIX<<
        return (0, error.As(errno)!);
    }
    return (newoffset, error.As(null!)!);

}

//sys    futimesat(dirfd int, path string, times *[2]Timeval) (err error)
//sysnb    Gettimeofday(tv *Timeval) (err error)
//sysnb    Time(t *Time_t) (tt Time_t, err error)
//sys    Utime(path string, buf *Utimbuf) (err error)
//sys    utimes(path string, times *[2]Timeval) (err error)

// On x86 Linux, all the socket calls go through an extra indirection,
// I think because the 5-register system call interface can't handle
// the 6-argument calls like sendto and recvfrom. Instead the
// arguments to the underlying system call are the number below
// and a pointer to an array of uintptr. We hide the pointer in the
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

private static ulong PC(this ptr<PtraceRegs> _addr_r) {
    ref PtraceRegs r = ref _addr_r.val;

    return uint64(uint32(r.Eip));
}

private static void SetPC(this ptr<PtraceRegs> _addr_r, ulong pc) {
    ref PtraceRegs r = ref _addr_r.val;

    r.Eip = int32(pc);
}

private static void SetLen(this ptr<Iovec> _addr_iov, nint length) {
    ref Iovec iov = ref _addr_iov.val;

    iov.Len = uint32(length);
}

private static void SetControllen(this ptr<Msghdr> _addr_msghdr, nint length) {
    ref Msghdr msghdr = ref _addr_msghdr.val;

    msghdr.Controllen = uint32(length);
}

private static void SetLen(this ptr<Cmsghdr> _addr_cmsg, nint length) {
    ref Cmsghdr cmsg = ref _addr_cmsg.val;

    cmsg.Len = uint32(length);
}

private static (System.UIntPtr, Errno) rawVforkSyscall(System.UIntPtr trap, System.UIntPtr a1);

} // end syscall_package
