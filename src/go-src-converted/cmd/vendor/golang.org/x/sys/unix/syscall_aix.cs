// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build aix
// +build aix

// Aix system calls.
// This file is compiled as ordinary Go code,
// but it is also input to mksyscall,
// which parses the //sys lines and generates system call stubs.
// Note that sometimes we use a lowercase //sys name and
// wrap it in our own nicer implementation.

// package unix -- go2cs converted at 2022 March 06 23:26:42 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\sys\unix\syscall_aix.go
using @unsafe = go.@unsafe_package;

namespace go.cmd.vendor.golang.org.x.sys;

public static partial class unix_package {

    /*
     * Wrapped
     */
public static error Access(@string path, uint mode) {
    error err = default!;

    return error.As(Faccessat(AT_FDCWD, path, mode, 0))!;
}

public static error Chmod(@string path, uint mode) {
    error err = default!;

    return error.As(Fchmodat(AT_FDCWD, path, mode, 0))!;
}

public static error Chown(@string path, nint uid, nint gid) {
    error err = default!;

    return error.As(Fchownat(AT_FDCWD, path, uid, gid, 0))!;
}

public static (nint, error) Creat(@string path, uint mode) {
    nint fd = default;
    error err = default!;

    return Open(path, O_CREAT | O_WRONLY | O_TRUNC, mode);
}

//sys    utimes(path string, times *[2]Timeval) (err error)
public static error Utimes(@string path, slice<Timeval> tv) {
    if (len(tv) != 2) {
        return error.As(EINVAL)!;
    }
    return error.As(utimes(path, new ptr<ptr<array<Timeval>>>(@unsafe.Pointer(_addr_tv[0]))))!;

}

//sys    utimensat(dirfd int, path string, times *[2]Timespec, flag int) (err error)
public static error UtimesNano(@string path, slice<Timespec> ts) {
    if (len(ts) != 2) {
        return error.As(EINVAL)!;
    }
    return error.As(utimensat(AT_FDCWD, path, new ptr<ptr<array<Timespec>>>(@unsafe.Pointer(_addr_ts[0])), 0))!;

}

public static error UtimesNanoAt(nint dirfd, @string path, slice<Timespec> ts, nint flags) {
    if (ts == null) {
        return error.As(utimensat(dirfd, path, null, flags))!;
    }
    if (len(ts) != 2) {
        return error.As(EINVAL)!;
    }
    return error.As(utimensat(dirfd, path, new ptr<ptr<array<Timespec>>>(@unsafe.Pointer(_addr_ts[0])), flags))!;

}

private static (unsafe.Pointer, _Socklen, error) sockaddr(this ptr<SockaddrInet4> _addr_sa) {
    unsafe.Pointer _p0 = default;
    _Socklen _p0 = default;
    error _p0 = default!;
    ref SockaddrInet4 sa = ref _addr_sa.val;

    if (sa.Port < 0 || sa.Port > 0xFFFF) {
        return (null, 0, error.As(EINVAL)!);
    }
    sa.raw.Family = AF_INET;
    ptr<array<byte>> p = new ptr<ptr<array<byte>>>(@unsafe.Pointer(_addr_sa.raw.Port));
    p[0] = byte(sa.Port >> 8);
    p[1] = byte(sa.Port);
    for (nint i = 0; i < len(sa.Addr); i++) {
        sa.raw.Addr[i] = sa.Addr[i];
    }
    return (@unsafe.Pointer(_addr_sa.raw), SizeofSockaddrInet4, error.As(null!)!);

}

private static (unsafe.Pointer, _Socklen, error) sockaddr(this ptr<SockaddrInet6> _addr_sa) {
    unsafe.Pointer _p0 = default;
    _Socklen _p0 = default;
    error _p0 = default!;
    ref SockaddrInet6 sa = ref _addr_sa.val;

    if (sa.Port < 0 || sa.Port > 0xFFFF) {
        return (null, 0, error.As(EINVAL)!);
    }
    sa.raw.Family = AF_INET6;
    ptr<array<byte>> p = new ptr<ptr<array<byte>>>(@unsafe.Pointer(_addr_sa.raw.Port));
    p[0] = byte(sa.Port >> 8);
    p[1] = byte(sa.Port);
    sa.raw.Scope_id = sa.ZoneId;
    for (nint i = 0; i < len(sa.Addr); i++) {
        sa.raw.Addr[i] = sa.Addr[i];
    }
    return (@unsafe.Pointer(_addr_sa.raw), SizeofSockaddrInet6, error.As(null!)!);

}

private static (unsafe.Pointer, _Socklen, error) sockaddr(this ptr<SockaddrUnix> _addr_sa) {
    unsafe.Pointer _p0 = default;
    _Socklen _p0 = default;
    error _p0 = default!;
    ref SockaddrUnix sa = ref _addr_sa.val;

    var name = sa.Name;
    var n = len(name);
    if (n > len(sa.raw.Path)) {
        return (null, 0, error.As(EINVAL)!);
    }
    if (n == len(sa.raw.Path) && name[0] != '@') {
        return (null, 0, error.As(EINVAL)!);
    }
    sa.raw.Family = AF_UNIX;
    for (nint i = 0; i < n; i++) {
        sa.raw.Path[i] = uint8(name[i]);
    } 
    // length is family (uint16), name, NUL.
    var sl = _Socklen(2);
    if (n > 0) {
        sl += _Socklen(n) + 1;
    }
    if (sa.raw.Path[0] == '@') {
        sa.raw.Path[0] = 0; 
        // Don't count trailing NUL for abstract address.
        sl--;

    }
    return (@unsafe.Pointer(_addr_sa.raw), sl, error.As(null!)!);

}

public static (Sockaddr, error) Getsockname(nint fd) {
    Sockaddr sa = default;
    error err = default!;

    ref RawSockaddrAny rsa = ref heap(out ptr<RawSockaddrAny> _addr_rsa);
    ref _Socklen len = ref heap(SizeofSockaddrAny, out ptr<_Socklen> _addr_len);
    err = getsockname(fd, _addr_rsa, _addr_len);

    if (err != null) {
        return ;
    }
    return anyToSockaddr(fd, _addr_rsa);

}

//sys    getcwd(buf []byte) (err error)

public static readonly var ImplementsGetwd = true;



public static (@string, error) Getwd() {
    @string ret = default;
    error err = default!;

    {
        var len = uint64(4096);

        while (>>MARKER:FOREXPRESSION_LEVEL_1<<) {
            var b = make_slice<byte>(len);
            var err = getcwd(b);
            if (err == null) {
                nint i = 0;
                while (b[i] != 0) {
                    i++;
            len *= 2;
                }

                return (string(b[(int)0..(int)i]), error.As(null!)!);

            }

            if (err != ERANGE) {
                return ("", error.As(err)!);
            }

        }
    }

}

public static (nint, error) Getcwd(slice<byte> buf) {
    nint n = default;
    error err = default!;

    err = getcwd(buf);
    if (err == null) {
        nint i = 0;
        while (buf[i] != 0) {
            i++;
        }
        n = i + 1;
    }
    return ;

}

public static (slice<nint>, error) Getgroups() {
    slice<nint> gids = default;
    error err = default!;

    var (n, err) = getgroups(0, null);
    if (err != null) {
        return (null, error.As(err)!);
    }
    if (n == 0) {
        return (null, error.As(null!)!);
    }
    if (n < 0 || n > 1000) {
        return (null, error.As(EINVAL)!);
    }
    var a = make_slice<_Gid_t>(n);
    n, err = getgroups(n, _addr_a[0]);
    if (err != null) {
        return (null, error.As(err)!);
    }
    gids = make_slice<nint>(n);
    foreach (var (i, v) in a[(int)0..(int)n]) {
        gids[i] = int(v);
    }    return ;

}

public static error Setgroups(slice<nint> gids) {
    error err = default!;

    if (len(gids) == 0) {
        return error.As(setgroups(0, null))!;
    }
    var a = make_slice<_Gid_t>(len(gids));
    foreach (var (i, v) in gids) {
        a[i] = _Gid_t(v);
    }    return error.As(setgroups(len(a), _addr_a[0]))!;

}

/*
 * Socket
 */

//sys    accept(s int, rsa *RawSockaddrAny, addrlen *_Socklen) (fd int, err error)

public static (nint, Sockaddr, error) Accept(nint fd) {
    nint nfd = default;
    Sockaddr sa = default;
    error err = default!;

    ref RawSockaddrAny rsa = ref heap(out ptr<RawSockaddrAny> _addr_rsa);
    ref _Socklen len = ref heap(SizeofSockaddrAny, out ptr<_Socklen> _addr_len);
    nfd, err = accept(fd, _addr_rsa, _addr_len);
    if (nfd == -1) {
        return ;
    }
    sa, err = anyToSockaddr(fd, _addr_rsa);
    if (err != null) {
        Close(nfd);
        nfd = 0;
    }
    return ;

}

public static (nint, nint, nint, Sockaddr, error) Recvmsg(nint fd, slice<byte> p, slice<byte> oob, nint flags) {
    nint n = default;
    nint oobn = default;
    nint recvflags = default;
    Sockaddr from = default;
    error err = default!;
 
    // Recvmsg not implemented on AIX
    ptr<SockaddrUnix> sa = @new<SockaddrUnix>();
    return (-1, -1, -1, sa, error.As(ENOSYS)!);

}

public static error Sendmsg(nint fd, slice<byte> p, slice<byte> oob, Sockaddr to, nint flags) {
    error err = default!;

    _, err = SendmsgN(fd, p, oob, to, flags);
    return ;
}

public static (nint, error) SendmsgN(nint fd, slice<byte> p, slice<byte> oob, Sockaddr to, nint flags) {
    nint n = default;
    error err = default!;
 
    // SendmsgN not implemented on AIX
    return (-1, error.As(ENOSYS)!);

}

private static (Sockaddr, error) anyToSockaddr(nint fd, ptr<RawSockaddrAny> _addr_rsa) {
    Sockaddr _p0 = default;
    error _p0 = default!;
    ref RawSockaddrAny rsa = ref _addr_rsa.val;



    if (rsa.Addr.Family == AF_UNIX) 
        var pp = (RawSockaddrUnix.val)(@unsafe.Pointer(rsa));
        ptr<SockaddrUnix> sa = @new<SockaddrUnix>(); 

        // Some versions of AIX have a bug in getsockname (see IV78655).
        // We can't rely on sa.Len being set correctly.
        var n = SizeofSockaddrUnix - 3; // subtract leading Family, Len, terminating NUL.
        {
            nint i__prev1 = i;

            for (nint i = 0; i < n; i++) {
                if (pp.Path[i] == 0) {
                    n = i;
                    break;
                }
            }


            i = i__prev1;
        }

        ptr<array<byte>> bytes = new ptr<ptr<array<byte>>>(@unsafe.Pointer(_addr_pp.Path[0]))[(int)0..(int)n];
        sa.Name = string(bytes);
        return (sa, error.As(null!)!);
    else if (rsa.Addr.Family == AF_INET) 
        pp = (RawSockaddrInet4.val)(@unsafe.Pointer(rsa));
        sa = @new<SockaddrInet4>();
        ptr<array<byte>> p = new ptr<ptr<array<byte>>>(@unsafe.Pointer(_addr_pp.Port));
        sa.Port = int(p[0]) << 8 + int(p[1]);
        {
            nint i__prev1 = i;

            for (i = 0; i < len(sa.Addr); i++) {
                sa.Addr[i] = pp.Addr[i];
            }


            i = i__prev1;
        }
        return (sa, error.As(null!)!);
    else if (rsa.Addr.Family == AF_INET6) 
        pp = (RawSockaddrInet6.val)(@unsafe.Pointer(rsa));
        sa = @new<SockaddrInet6>();
        p = new ptr<ptr<array<byte>>>(@unsafe.Pointer(_addr_pp.Port));
        sa.Port = int(p[0]) << 8 + int(p[1]);
        sa.ZoneId = pp.Scope_id;
        {
            nint i__prev1 = i;

            for (i = 0; i < len(sa.Addr); i++) {
                sa.Addr[i] = pp.Addr[i];
            }


            i = i__prev1;
        }
        return (sa, error.As(null!)!);
        return (null, error.As(EAFNOSUPPORT)!);

}

public static error Gettimeofday(ptr<Timeval> _addr_tv) {
    error err = default!;
    ref Timeval tv = ref _addr_tv.val;

    err = gettimeofday(tv, null);
    return ;
}

public static (nint, error) Sendfile(nint outfd, nint infd, ptr<long> _addr_offset, nint count) {
    nint written = default;
    error err = default!;
    ref long offset = ref _addr_offset.val;

    if (raceenabled) {
        raceReleaseMerge(@unsafe.Pointer(_addr_ioSync));
    }
    return sendfile(outfd, infd, _addr_offset, count);

}

// TODO
private static (nint, error) sendfile(nint outfd, nint infd, ptr<long> _addr_offset, nint count) {
    nint written = default;
    error err = default!;
    ref long offset = ref _addr_offset.val;

    return (-1, error.As(ENOSYS)!);
}

private static (ulong, bool) direntIno(slice<byte> buf) {
    ulong _p0 = default;
    bool _p0 = default;

    return readInt(buf, @unsafe.Offsetof(new Dirent().Ino), @unsafe.Sizeof(new Dirent().Ino));
}

private static (ulong, bool) direntReclen(slice<byte> buf) {
    ulong _p0 = default;
    bool _p0 = default;

    return readInt(buf, @unsafe.Offsetof(new Dirent().Reclen), @unsafe.Sizeof(new Dirent().Reclen));
}

private static (ulong, bool) direntNamlen(slice<byte> buf) {
    ulong _p0 = default;
    bool _p0 = default;

    var (reclen, ok) = direntReclen(buf);
    if (!ok) {
        return (0, false);
    }
    return (reclen - uint64(@unsafe.Offsetof(new Dirent().Name)), true);

}

//sys    getdirent(fd int, buf []byte) (n int, err error)
public static (nint, error) Getdents(nint fd, slice<byte> buf) {
    nint n = default;
    error err = default!;

    return getdirent(fd, buf);
}

//sys    wait4(pid Pid_t, status *_C_int, options int, rusage *Rusage) (wpid Pid_t, err error)
public static (nint, error) Wait4(nint pid, ptr<WaitStatus> _addr_wstatus, nint options, ptr<Rusage> _addr_rusage) {
    nint wpid = default;
    error err = default!;
    ref WaitStatus wstatus = ref _addr_wstatus.val;
    ref Rusage rusage = ref _addr_rusage.val;

    ref _C_int status = ref heap(out ptr<_C_int> _addr_status);
    Pid_t r = default;
    err = ERESTART; 
    // AIX wait4 may return with ERESTART errno, while the processus is still
    // active.
    while (err == ERESTART) {
        r, err = wait4(Pid_t(pid), _addr_status, options, rusage);
    }
    wpid = int(r);
    if (wstatus != null) {
        wstatus = WaitStatus(status);
    }
    return ;

}

/*
 * Wait
 */

public partial struct WaitStatus { // : uint
}

public static bool Stopped(this WaitStatus w) {
    return w & 0x40 != 0;
}
public static Signal StopSignal(this WaitStatus w) {
    if (!w.Stopped()) {
        return -1;
    }
    return Signal(w >> 8) & 0xFF;

}

public static bool Exited(this WaitStatus w) {
    return w & 0xFF == 0;
}
public static nint ExitStatus(this WaitStatus w) {
    if (!w.Exited()) {
        return -1;
    }
    return int((w >> 8) & 0xFF);

}

public static bool Signaled(this WaitStatus w) {
    return w & 0x40 == 0 && w & 0xFF != 0;
}
public static Signal Signal(this WaitStatus w) {
    if (!w.Signaled()) {
        return -1;
    }
    return Signal(w >> 16) & 0xFF;

}

public static bool Continued(this WaitStatus w) {
    return w & 0x01000000 != 0;
}

public static bool CoreDump(this WaitStatus w) {
    return w & 0x80 == 0x80;
}

public static nint TrapCause(this WaitStatus w) {
    return -1;
}

//sys    ioctl(fd int, req uint, arg uintptr) (err error)

// fcntl must never be called with cmd=F_DUP2FD because it doesn't work on AIX
// There is no way to create a custom fcntl and to keep //sys fcntl easily,
// Therefore, the programmer must call dup2 instead of fcntl in this case.

// FcntlInt performs a fcntl syscall on fd with the provided command and argument.
//sys    FcntlInt(fd uintptr, cmd int, arg int) (r int,err error) = fcntl

// FcntlFlock performs a fcntl syscall for the F_GETLK, F_SETLK or F_SETLKW command.
//sys    FcntlFlock(fd uintptr, cmd int, lk *Flock_t) (err error) = fcntl

//sys    fcntl(fd int, cmd int, arg int) (val int, err error)

/*
 * Direct access
 */

//sys    Acct(path string) (err error)
//sys    Chdir(path string) (err error)
//sys    Chroot(path string) (err error)
//sys    Close(fd int) (err error)
//sys    Dup(oldfd int) (fd int, err error)
//sys    Exit(code int)
//sys    Faccessat(dirfd int, path string, mode uint32, flags int) (err error)
//sys    Fchdir(fd int) (err error)
//sys    Fchmod(fd int, mode uint32) (err error)
//sys    Fchmodat(dirfd int, path string, mode uint32, flags int) (err error)
//sys    Fchownat(dirfd int, path string, uid int, gid int, flags int) (err error)
//sys    Fdatasync(fd int) (err error)
//sys    Fsync(fd int) (err error)
// readdir_r
//sysnb    Getpgid(pid int) (pgid int, err error)

//sys    Getpgrp() (pid int)

//sysnb    Getpid() (pid int)
//sysnb    Getppid() (ppid int)
//sys    Getpriority(which int, who int) (prio int, err error)
//sysnb    Getrusage(who int, rusage *Rusage) (err error)
//sysnb    Getsid(pid int) (sid int, err error)
//sysnb    Kill(pid int, sig Signal) (err error)
//sys    Klogctl(typ int, buf []byte) (n int, err error) = syslog
//sys    Mkdir(dirfd int, path string, mode uint32) (err error)
//sys    Mkdirat(dirfd int, path string, mode uint32) (err error)
//sys    Mkfifo(path string, mode uint32) (err error)
//sys    Mknod(path string, mode uint32, dev int) (err error)
//sys    Mknodat(dirfd int, path string, mode uint32, dev int) (err error)
//sys    Nanosleep(time *Timespec, leftover *Timespec) (err error)
//sys    Open(path string, mode int, perm uint32) (fd int, err error) = open64
//sys    Openat(dirfd int, path string, flags int, mode uint32) (fd int, err error)
//sys    read(fd int, p []byte) (n int, err error)
//sys    Readlink(path string, buf []byte) (n int, err error)
//sys    Renameat(olddirfd int, oldpath string, newdirfd int, newpath string) (err error)
//sys    Setdomainname(p []byte) (err error)
//sys    Sethostname(p []byte) (err error)
//sysnb    Setpgid(pid int, pgid int) (err error)
//sysnb    Setsid() (pid int, err error)
//sysnb    Settimeofday(tv *Timeval) (err error)

//sys    Setuid(uid int) (err error)
//sys    Setgid(uid int) (err error)

//sys    Setpriority(which int, who int, prio int) (err error)
//sys    Statx(dirfd int, path string, flags int, mask int, stat *Statx_t) (err error)
//sys    Sync()
//sysnb    Times(tms *Tms) (ticks uintptr, err error)
//sysnb    Umask(mask int) (oldmask int)
//sysnb    Uname(buf *Utsname) (err error)
//sys    Unlink(path string) (err error)
//sys    Unlinkat(dirfd int, path string, flags int) (err error)
//sys    Ustat(dev int, ubuf *Ustat_t) (err error)
//sys    write(fd int, p []byte) (n int, err error)
//sys    readlen(fd int, p *byte, np int) (n int, err error) = read
//sys    writelen(fd int, p *byte, np int) (n int, err error) = write

//sys    Dup2(oldfd int, newfd int) (err error)
//sys    Fadvise(fd int, offset int64, length int64, advice int) (err error) = posix_fadvise64
//sys    Fchown(fd int, uid int, gid int) (err error)
//sys    fstat(fd int, stat *Stat_t) (err error)
//sys    fstatat(dirfd int, path string, stat *Stat_t, flags int) (err error) = fstatat
//sys    Fstatfs(fd int, buf *Statfs_t) (err error)
//sys    Ftruncate(fd int, length int64) (err error)
//sysnb    Getegid() (egid int)
//sysnb    Geteuid() (euid int)
//sysnb    Getgid() (gid int)
//sysnb    Getuid() (uid int)
//sys    Lchown(path string, uid int, gid int) (err error)
//sys    Listen(s int, n int) (err error)
//sys    lstat(path string, stat *Stat_t) (err error)
//sys    Pause() (err error)
//sys    Pread(fd int, p []byte, offset int64) (n int, err error) = pread64
//sys    Pwrite(fd int, p []byte, offset int64) (n int, err error) = pwrite64
//sys    Select(nfd int, r *FdSet, w *FdSet, e *FdSet, timeout *Timeval) (n int, err error)
//sys    Pselect(nfd int, r *FdSet, w *FdSet, e *FdSet, timeout *Timespec, sigmask *Sigset_t) (n int, err error)
//sysnb    Setregid(rgid int, egid int) (err error)
//sysnb    Setreuid(ruid int, euid int) (err error)
//sys    Shutdown(fd int, how int) (err error)
//sys    Splice(rfd int, roff *int64, wfd int, woff *int64, len int, flags int) (n int64, err error)
//sys    stat(path string, statptr *Stat_t) (err error)
//sys    Statfs(path string, buf *Statfs_t) (err error)
//sys    Truncate(path string, length int64) (err error)

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

// In order to use msghdr structure with Control, Controllen, nrecvmsg and nsendmsg must be used.
//sys    recvmsg(s int, msg *Msghdr, flags int) (n int, err error) = nrecvmsg
//sys    sendmsg(s int, msg *Msghdr, flags int) (n int, err error) = nsendmsg

//sys    munmap(addr uintptr, length uintptr) (err error)

private static ptr<mmapper> mapper = addr(new mmapper(active:make(map[*byte][]byte),mmap:mmap,munmap:munmap,));

public static (slice<byte>, error) Mmap(nint fd, long offset, nint length, nint prot, nint flags) {
    slice<byte> data = default;
    error err = default!;

    return mapper.Mmap(fd, offset, length, prot, flags);
}

public static error Munmap(slice<byte> b) {
    error err = default!;

    return error.As(mapper.Munmap(b))!;
}

//sys    Madvise(b []byte, advice int) (err error)
//sys    Mprotect(b []byte, prot int) (err error)
//sys    Mlock(b []byte) (err error)
//sys    Mlockall(flags int) (err error)
//sys    Msync(b []byte, flags int) (err error)
//sys    Munlock(b []byte) (err error)
//sys    Munlockall() (err error)

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

//sys    poll(fds *PollFd, nfds int, timeout int) (n int, err error)

public static (nint, error) Poll(slice<PollFd> fds, nint timeout) {
    nint n = default;
    error err = default!;

    if (len(fds) == 0) {
        return poll(null, 0, timeout);
    }
    return poll(_addr_fds[0], len(fds), timeout);

}

//sys    gettimeofday(tv *Timeval, tzp *Timezone) (err error)
//sysnb    Time(t *Time_t) (tt Time_t, err error)
//sys    Utime(path string, buf *Utimbuf) (err error)

//sys    Getsystemcfg(label int) (n uint64)

//sys    umount(target string) (err error)
public static error Unmount(@string target, nint flags) {
    error err = default!;

    if (flags != 0) { 
        // AIX doesn't have any flags for umount.
        return error.As(ENOSYS)!;

    }
    return error.As(umount(target))!;

}

} // end unix_package
