// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build s390x && linux
// +build s390x,linux

// package unix -- go2cs converted at 2022 March 06 23:27:12 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\sys\unix\syscall_linux_s390x.go
using @unsafe = go.@unsafe_package;

namespace go.cmd.vendor.golang.org.x.sys;

public static partial class unix_package {

    //sys    dup2(oldfd int, newfd int) (err error)
    //sysnb    EpollCreate(size int) (fd int, err error)
    //sys    EpollWait(epfd int, events []EpollEvent, msec int) (n int, err error)
    //sys    Fadvise(fd int, offset int64, length int64, advice int) (err error) = SYS_FADVISE64
    //sys    Fchown(fd int, uid int, gid int) (err error)
    //sys    Fstat(fd int, stat *Stat_t) (err error)
    //sys    Fstatat(dirfd int, path string, stat *Stat_t, flags int) (err error) = SYS_NEWFSTATAT
    //sys    Fstatfs(fd int, buf *Statfs_t) (err error)
    //sys    Ftruncate(fd int, length int64) (err error)
    //sysnb    Getegid() (egid int)
    //sysnb    Geteuid() (euid int)
    //sysnb    Getgid() (gid int)
    //sysnb    Getrlimit(resource int, rlim *Rlimit) (err error)
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
    //sys    setfsgid(gid int) (prev int, err error)
    //sys    setfsuid(uid int) (prev int, err error)
    //sysnb    Setregid(rgid int, egid int) (err error)
    //sysnb    Setresgid(rgid int, egid int, sgid int) (err error)
    //sysnb    Setresuid(ruid int, euid int, suid int) (err error)
    //sysnb    Setrlimit(resource int, rlim *Rlimit) (err error)
    //sysnb    Setreuid(ruid int, euid int) (err error)
    //sys    Splice(rfd int, roff *int64, wfd int, woff *int64, len int, flags int) (n int64, err error)
    //sys    Stat(path string, stat *Stat_t) (err error)
    //sys    Statfs(path string, buf *Statfs_t) (err error)
    //sys    SyncFileRange(fd int, off int64, n int64, flags int) (err error)
    //sys    Truncate(path string, length int64) (err error)
    //sys    Ustat(dev int, ubuf *Ustat_t) (err error)
    //sysnb    getgroups(n int, list *_Gid_t) (nn int, err error)
    //sysnb    setgroups(n int, list *_Gid_t) (err error)

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

//sysnb    pipe2(p *[2]_C_int, flags int) (err error)

public static error Pipe(slice<nint> p) {
    error err = default!;

    if (len(p) != 2) {
        return error.As(EINVAL)!;
    }
    ref array<_C_int> pp = ref heap(new array<_C_int>(2), out ptr<array<_C_int>> _addr_pp);
    err = pipe2(_addr_pp, 0); // pipe2 is the same as pipe when flags are set to 0.
    p[0] = int(pp[0]);
    p[1] = int(pp[1]);
    return ;

}

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

public static error Ioperm(nint from, nint num, nint on) {
    error err = default!;

    return error.As(ENOSYS)!;
}

public static error Iopl(nint level) {
    error err = default!;

    return error.As(ENOSYS)!;
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

private static void SetIovlen(this ptr<Msghdr> _addr_msghdr, nint length) {
    ref Msghdr msghdr = ref _addr_msghdr.val;

    msghdr.Iovlen = uint64(length);
}

private static void SetLen(this ptr<Cmsghdr> _addr_cmsg, nint length) {
    ref Cmsghdr cmsg = ref _addr_cmsg.val;

    cmsg.Len = uint64(length);
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
// The arguments to the underlying system call (SYS_SOCKETCALL) are the
// number below and a pointer to an array of uintptr.
 
// see linux/net.h
private static readonly nint netSocket = 1;
private static readonly nint netBind = 2;
private static readonly nint netConnect = 3;
private static readonly nint netListen = 4;
private static readonly nint netAccept = 5;
private static readonly nint netGetSockName = 6;
private static readonly nint netGetPeerName = 7;
private static readonly nint netSocketPair = 8;
private static readonly nint netSend = 9;
private static readonly nint netRecv = 10;
private static readonly nint netSendTo = 11;
private static readonly nint netRecvFrom = 12;
private static readonly nint netShutdown = 13;
private static readonly nint netSetSockOpt = 14;
private static readonly nint netGetSockOpt = 15;
private static readonly nint netSendMsg = 16;
private static readonly nint netRecvMsg = 17;
private static readonly nint netAccept4 = 18;
private static readonly nint netRecvMMsg = 19;
private static readonly nint netSendMMsg = 20;


private static (nint, error) accept(nint s, ptr<RawSockaddrAny> _addr_rsa, ptr<_Socklen> _addr_addrlen) {
    nint _p0 = default;
    error _p0 = default!;
    ref RawSockaddrAny rsa = ref _addr_rsa.val;
    ref _Socklen addrlen = ref _addr_addrlen.val;

    ref array<System.UIntPtr> args = ref heap(new array<System.UIntPtr>(new System.UIntPtr[] { uintptr(s), uintptr(unsafe.Pointer(rsa)), uintptr(unsafe.Pointer(addrlen)) }), out ptr<array<System.UIntPtr>> _addr_args);
    var (fd, _, err) = Syscall(SYS_SOCKETCALL, netAccept, uintptr(@unsafe.Pointer(_addr_args)), 0);
    if (err != 0) {
        return (0, error.As(err)!);
    }
    return (int(fd), error.As(null!)!);

}

private static (nint, error) accept4(nint s, ptr<RawSockaddrAny> _addr_rsa, ptr<_Socklen> _addr_addrlen, nint flags) {
    nint _p0 = default;
    error _p0 = default!;
    ref RawSockaddrAny rsa = ref _addr_rsa.val;
    ref _Socklen addrlen = ref _addr_addrlen.val;

    ref array<System.UIntPtr> args = ref heap(new array<System.UIntPtr>(new System.UIntPtr[] { uintptr(s), uintptr(unsafe.Pointer(rsa)), uintptr(unsafe.Pointer(addrlen)), uintptr(flags) }), out ptr<array<System.UIntPtr>> _addr_args);
    var (fd, _, err) = Syscall(SYS_SOCKETCALL, netAccept4, uintptr(@unsafe.Pointer(_addr_args)), 0);
    if (err != 0) {
        return (0, error.As(err)!);
    }
    return (int(fd), error.As(null!)!);

}

private static error getsockname(nint s, ptr<RawSockaddrAny> _addr_rsa, ptr<_Socklen> _addr_addrlen) {
    ref RawSockaddrAny rsa = ref _addr_rsa.val;
    ref _Socklen addrlen = ref _addr_addrlen.val;

    ref array<System.UIntPtr> args = ref heap(new array<System.UIntPtr>(new System.UIntPtr[] { uintptr(s), uintptr(unsafe.Pointer(rsa)), uintptr(unsafe.Pointer(addrlen)) }), out ptr<array<System.UIntPtr>> _addr_args);
    var (_, _, err) = RawSyscall(SYS_SOCKETCALL, netGetSockName, uintptr(@unsafe.Pointer(_addr_args)), 0);
    if (err != 0) {
        return error.As(err)!;
    }
    return error.As(null!)!;

}

private static error getpeername(nint s, ptr<RawSockaddrAny> _addr_rsa, ptr<_Socklen> _addr_addrlen) {
    ref RawSockaddrAny rsa = ref _addr_rsa.val;
    ref _Socklen addrlen = ref _addr_addrlen.val;

    ref array<System.UIntPtr> args = ref heap(new array<System.UIntPtr>(new System.UIntPtr[] { uintptr(s), uintptr(unsafe.Pointer(rsa)), uintptr(unsafe.Pointer(addrlen)) }), out ptr<array<System.UIntPtr>> _addr_args);
    var (_, _, err) = RawSyscall(SYS_SOCKETCALL, netGetPeerName, uintptr(@unsafe.Pointer(_addr_args)), 0);
    if (err != 0) {
        return error.As(err)!;
    }
    return error.As(null!)!;

}

private static error socketpair(nint domain, nint typ, nint flags, ptr<array<int>> _addr_fd) {
    ref array<int> fd = ref _addr_fd.val;

    ref array<System.UIntPtr> args = ref heap(new array<System.UIntPtr>(new System.UIntPtr[] { uintptr(domain), uintptr(typ), uintptr(flags), uintptr(unsafe.Pointer(fd)) }), out ptr<array<System.UIntPtr>> _addr_args);
    var (_, _, err) = RawSyscall(SYS_SOCKETCALL, netSocketPair, uintptr(@unsafe.Pointer(_addr_args)), 0);
    if (err != 0) {
        return error.As(err)!;
    }
    return error.As(null!)!;

}

private static error bind(nint s, unsafe.Pointer addr, _Socklen addrlen) {
    ref array<System.UIntPtr> args = ref heap(new array<System.UIntPtr>(new System.UIntPtr[] { uintptr(s), uintptr(addr), uintptr(addrlen) }), out ptr<array<System.UIntPtr>> _addr_args);
    var (_, _, err) = Syscall(SYS_SOCKETCALL, netBind, uintptr(@unsafe.Pointer(_addr_args)), 0);
    if (err != 0) {
        return error.As(err)!;
    }
    return error.As(null!)!;

}

private static error connect(nint s, unsafe.Pointer addr, _Socklen addrlen) {
    ref array<System.UIntPtr> args = ref heap(new array<System.UIntPtr>(new System.UIntPtr[] { uintptr(s), uintptr(addr), uintptr(addrlen) }), out ptr<array<System.UIntPtr>> _addr_args);
    var (_, _, err) = Syscall(SYS_SOCKETCALL, netConnect, uintptr(@unsafe.Pointer(_addr_args)), 0);
    if (err != 0) {
        return error.As(err)!;
    }
    return error.As(null!)!;

}

private static (nint, error) socket(nint domain, nint typ, nint proto) {
    nint _p0 = default;
    error _p0 = default!;

    ref array<System.UIntPtr> args = ref heap(new array<System.UIntPtr>(new System.UIntPtr[] { uintptr(domain), uintptr(typ), uintptr(proto) }), out ptr<array<System.UIntPtr>> _addr_args);
    var (fd, _, err) = RawSyscall(SYS_SOCKETCALL, netSocket, uintptr(@unsafe.Pointer(_addr_args)), 0);
    if (err != 0) {
        return (0, error.As(err)!);
    }
    return (int(fd), error.As(null!)!);

}

private static error getsockopt(nint s, nint level, nint name, unsafe.Pointer val, ptr<_Socklen> _addr_vallen) {
    ref _Socklen vallen = ref _addr_vallen.val;

    ref array<System.UIntPtr> args = ref heap(new array<System.UIntPtr>(new System.UIntPtr[] { uintptr(s), uintptr(level), uintptr(name), uintptr(val), uintptr(unsafe.Pointer(vallen)) }), out ptr<array<System.UIntPtr>> _addr_args);
    var (_, _, err) = Syscall(SYS_SOCKETCALL, netGetSockOpt, uintptr(@unsafe.Pointer(_addr_args)), 0);
    if (err != 0) {
        return error.As(err)!;
    }
    return error.As(null!)!;

}

private static error setsockopt(nint s, nint level, nint name, unsafe.Pointer val, System.UIntPtr vallen) {
    ref array<System.UIntPtr> args = ref heap(new array<System.UIntPtr>(new System.UIntPtr[] { uintptr(s), uintptr(level), uintptr(name), uintptr(val), vallen }), out ptr<array<System.UIntPtr>> _addr_args);
    var (_, _, err) = Syscall(SYS_SOCKETCALL, netSetSockOpt, uintptr(@unsafe.Pointer(_addr_args)), 0);
    if (err != 0) {
        return error.As(err)!;
    }
    return error.As(null!)!;

}

private static (nint, error) recvfrom(nint s, slice<byte> p, nint flags, ptr<RawSockaddrAny> _addr_from, ptr<_Socklen> _addr_fromlen) {
    nint _p0 = default;
    error _p0 = default!;
    ref RawSockaddrAny from = ref _addr_from.val;
    ref _Socklen fromlen = ref _addr_fromlen.val;

    System.UIntPtr @base = default;
    if (len(p) > 0) {
        base = uintptr(@unsafe.Pointer(_addr_p[0]));
    }
    ref array<System.UIntPtr> args = ref heap(new array<System.UIntPtr>(new System.UIntPtr[] { uintptr(s), base, uintptr(len(p)), uintptr(flags), uintptr(unsafe.Pointer(from)), uintptr(unsafe.Pointer(fromlen)) }), out ptr<array<System.UIntPtr>> _addr_args);
    var (n, _, err) = Syscall(SYS_SOCKETCALL, netRecvFrom, uintptr(@unsafe.Pointer(_addr_args)), 0);
    if (err != 0) {
        return (0, error.As(err)!);
    }
    return (int(n), error.As(null!)!);

}

private static error sendto(nint s, slice<byte> p, nint flags, unsafe.Pointer to, _Socklen addrlen) {
    System.UIntPtr @base = default;
    if (len(p) > 0) {
        base = uintptr(@unsafe.Pointer(_addr_p[0]));
    }
    ref array<System.UIntPtr> args = ref heap(new array<System.UIntPtr>(new System.UIntPtr[] { uintptr(s), base, uintptr(len(p)), uintptr(flags), uintptr(to), uintptr(addrlen) }), out ptr<array<System.UIntPtr>> _addr_args);
    var (_, _, err) = Syscall(SYS_SOCKETCALL, netSendTo, uintptr(@unsafe.Pointer(_addr_args)), 0);
    if (err != 0) {
        return error.As(err)!;
    }
    return error.As(null!)!;

}

private static (nint, error) recvmsg(nint s, ptr<Msghdr> _addr_msg, nint flags) {
    nint _p0 = default;
    error _p0 = default!;
    ref Msghdr msg = ref _addr_msg.val;

    ref array<System.UIntPtr> args = ref heap(new array<System.UIntPtr>(new System.UIntPtr[] { uintptr(s), uintptr(unsafe.Pointer(msg)), uintptr(flags) }), out ptr<array<System.UIntPtr>> _addr_args);
    var (n, _, err) = Syscall(SYS_SOCKETCALL, netRecvMsg, uintptr(@unsafe.Pointer(_addr_args)), 0);
    if (err != 0) {
        return (0, error.As(err)!);
    }
    return (int(n), error.As(null!)!);

}

private static (nint, error) sendmsg(nint s, ptr<Msghdr> _addr_msg, nint flags) {
    nint _p0 = default;
    error _p0 = default!;
    ref Msghdr msg = ref _addr_msg.val;

    ref array<System.UIntPtr> args = ref heap(new array<System.UIntPtr>(new System.UIntPtr[] { uintptr(s), uintptr(unsafe.Pointer(msg)), uintptr(flags) }), out ptr<array<System.UIntPtr>> _addr_args);
    var (n, _, err) = Syscall(SYS_SOCKETCALL, netSendMsg, uintptr(@unsafe.Pointer(_addr_args)), 0);
    if (err != 0) {
        return (0, error.As(err)!);
    }
    return (int(n), error.As(null!)!);

}

public static error Listen(nint s, nint n) {
    ref array<System.UIntPtr> args = ref heap(new array<System.UIntPtr>(new System.UIntPtr[] { uintptr(s), uintptr(n) }), out ptr<array<System.UIntPtr>> _addr_args);
    var (_, _, err) = Syscall(SYS_SOCKETCALL, netListen, uintptr(@unsafe.Pointer(_addr_args)), 0);
    if (err != 0) {
        return error.As(err)!;
    }
    return error.As(null!)!;

}

public static error Shutdown(nint s, nint how) {
    ref array<System.UIntPtr> args = ref heap(new array<System.UIntPtr>(new System.UIntPtr[] { uintptr(s), uintptr(how) }), out ptr<array<System.UIntPtr>> _addr_args);
    var (_, _, err) = Syscall(SYS_SOCKETCALL, netShutdown, uintptr(@unsafe.Pointer(_addr_args)), 0);
    if (err != 0) {
        return error.As(err)!;
    }
    return error.As(null!)!;

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
