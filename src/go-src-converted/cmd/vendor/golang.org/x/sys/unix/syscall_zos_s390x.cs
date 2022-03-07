// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build zos && s390x
// +build zos,s390x

// package unix -- go2cs converted at 2022 March 06 23:27:25 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\sys\unix\syscall_zos_s390x.go
using bytes = go.bytes_package;
using runtime = go.runtime_package;
using sort = go.sort_package;
using sync = go.sync_package;
using syscall = go.syscall_package;
using @unsafe = go.@unsafe_package;
using System;


namespace go.cmd.vendor.golang.org.x.sys;

public static partial class unix_package {

public static readonly nint O_CLOEXEC = 0; // Dummy value (not supported).
public static readonly var AF_LOCAL = AF_UNIX; // AF_LOCAL is an alias for AF_UNIX

private static (System.UIntPtr, System.UIntPtr, Errno) syscall_syscall(System.UIntPtr trap, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3);
private static (System.UIntPtr, System.UIntPtr, Errno) syscall_rawsyscall(System.UIntPtr trap, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3);
private static (System.UIntPtr, System.UIntPtr, Errno) syscall_syscall6(System.UIntPtr trap, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6);
private static (System.UIntPtr, System.UIntPtr, Errno) syscall_rawsyscall6(System.UIntPtr trap, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6);
private static (System.UIntPtr, System.UIntPtr, Errno) syscall_syscall9(System.UIntPtr trap, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6, System.UIntPtr a7, System.UIntPtr a8, System.UIntPtr a9);
private static (System.UIntPtr, System.UIntPtr, Errno) syscall_rawsyscall9(System.UIntPtr trap, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6, System.UIntPtr a7, System.UIntPtr a8, System.UIntPtr a9);

private static void copyStat(ptr<Stat_t> _addr_stat, ptr<Stat_LE_t> _addr_statLE) {
    ref Stat_t stat = ref _addr_stat.val;
    ref Stat_LE_t statLE = ref _addr_statLE.val;

    stat.Dev = uint64(statLE.Dev);
    stat.Ino = uint64(statLE.Ino);
    stat.Nlink = uint64(statLE.Nlink);
    stat.Mode = uint32(statLE.Mode);
    stat.Uid = uint32(statLE.Uid);
    stat.Gid = uint32(statLE.Gid);
    stat.Rdev = uint64(statLE.Rdev);
    stat.Size = statLE.Size;
    stat.Atim.Sec = int64(statLE.Atim);
    stat.Atim.Nsec = 0; //zos doesn't return nanoseconds
    stat.Mtim.Sec = int64(statLE.Mtim);
    stat.Mtim.Nsec = 0; //zos doesn't return nanoseconds
    stat.Ctim.Sec = int64(statLE.Ctim);
    stat.Ctim.Nsec = 0; //zos doesn't return nanoseconds
    stat.Blksize = int64(statLE.Blksize);
    stat.Blocks = statLE.Blocks;

}

private static void svcCall(unsafe.Pointer fnptr, ptr<unsafe.Pointer> argv, ptr<ulong> dsa);
private static unsafe.Pointer svcLoad(ptr<byte> name);
private static long svcUnload(ptr<byte> name, unsafe.Pointer fnptr);

private static @string NameString(this ptr<Dirent> _addr_d) {
    ref Dirent d = ref _addr_d.val;

    if (d == null) {>>MARKER:FUNCTION_svcUnload_BLOCK_PREFIX<<
        return "";
    }
    return string(d.Name[..(int)d.Namlen]);

}

private static (unsafe.Pointer, _Socklen, error) sockaddr(this ptr<SockaddrInet4> _addr_sa) {
    unsafe.Pointer _p0 = default;
    _Socklen _p0 = default;
    error _p0 = default!;
    ref SockaddrInet4 sa = ref _addr_sa.val;

    if (sa.Port < 0 || sa.Port > 0xFFFF) {>>MARKER:FUNCTION_svcLoad_BLOCK_PREFIX<<
        return (null, 0, error.As(EINVAL)!);
    }
    sa.raw.Len = SizeofSockaddrInet4;
    sa.raw.Family = AF_INET;
    ptr<array<byte>> p = new ptr<ptr<array<byte>>>(@unsafe.Pointer(_addr_sa.raw.Port));
    p[0] = byte(sa.Port >> 8);
    p[1] = byte(sa.Port);
    for (nint i = 0; i < len(sa.Addr); i++) {>>MARKER:FUNCTION_svcCall_BLOCK_PREFIX<<
        sa.raw.Addr[i] = sa.Addr[i];
    }
    return (@unsafe.Pointer(_addr_sa.raw), _Socklen(sa.raw.Len), error.As(null!)!);

}

private static (unsafe.Pointer, _Socklen, error) sockaddr(this ptr<SockaddrInet6> _addr_sa) {
    unsafe.Pointer _p0 = default;
    _Socklen _p0 = default;
    error _p0 = default!;
    ref SockaddrInet6 sa = ref _addr_sa.val;

    if (sa.Port < 0 || sa.Port > 0xFFFF) {>>MARKER:FUNCTION_syscall_rawsyscall9_BLOCK_PREFIX<<
        return (null, 0, error.As(EINVAL)!);
    }
    sa.raw.Len = SizeofSockaddrInet6;
    sa.raw.Family = AF_INET6;
    ptr<array<byte>> p = new ptr<ptr<array<byte>>>(@unsafe.Pointer(_addr_sa.raw.Port));
    p[0] = byte(sa.Port >> 8);
    p[1] = byte(sa.Port);
    sa.raw.Scope_id = sa.ZoneId;
    for (nint i = 0; i < len(sa.Addr); i++) {>>MARKER:FUNCTION_syscall_syscall9_BLOCK_PREFIX<<
        sa.raw.Addr[i] = sa.Addr[i];
    }
    return (@unsafe.Pointer(_addr_sa.raw), _Socklen(sa.raw.Len), error.As(null!)!);

}

private static (unsafe.Pointer, _Socklen, error) sockaddr(this ptr<SockaddrUnix> _addr_sa) {
    unsafe.Pointer _p0 = default;
    _Socklen _p0 = default;
    error _p0 = default!;
    ref SockaddrUnix sa = ref _addr_sa.val;

    var name = sa.Name;
    var n = len(name);
    if (n >= len(sa.raw.Path) || n == 0) {>>MARKER:FUNCTION_syscall_rawsyscall6_BLOCK_PREFIX<<
        return (null, 0, error.As(EINVAL)!);
    }
    sa.raw.Len = byte(3 + n); // 2 for Family, Len; 1 for NUL
    sa.raw.Family = AF_UNIX;
    for (nint i = 0; i < n; i++) {>>MARKER:FUNCTION_syscall_syscall6_BLOCK_PREFIX<<
        sa.raw.Path[i] = int8(name[i]);
    }
    return (@unsafe.Pointer(_addr_sa.raw), _Socklen(sa.raw.Len), error.As(null!)!);

}

private static (Sockaddr, error) anyToSockaddr(nint _, ptr<RawSockaddrAny> _addr_rsa) {
    Sockaddr _p0 = default;
    error _p0 = default!;
    ref RawSockaddrAny rsa = ref _addr_rsa.val;
 
    // TODO(neeilan): Implement use of first param (fd)

    if (rsa.Addr.Family == AF_UNIX) 
        var pp = (RawSockaddrUnix.val)(@unsafe.Pointer(rsa));
        ptr<SockaddrUnix> sa = @new<SockaddrUnix>(); 
        // For z/OS, only replace NUL with @ when the
        // length is not zero.
        if (pp.Len != 0 && pp.Path[0] == 0) {>>MARKER:FUNCTION_syscall_rawsyscall_BLOCK_PREFIX<< 
            // "Abstract" Unix domain socket.
            // Rewrite leading NUL as @ for textual display.
            // (This is the standard convention.)
            // Not friendly to overwrite in place,
            // but the callers below don't care.
            pp.Path[0] = '@';

        }
        nint n = 0;
        while (n < int(pp.Len) && pp.Path[n] != 0) {>>MARKER:FUNCTION_syscall_syscall_BLOCK_PREFIX<<
            n++;
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

            for (nint i = 0; i < len(sa.Addr); i++) {
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

public static (nint, Sockaddr, error) Accept(nint fd) {
    nint nfd = default;
    Sockaddr sa = default;
    error err = default!;

    ref RawSockaddrAny rsa = ref heap(out ptr<RawSockaddrAny> _addr_rsa);
    ref _Socklen len = ref heap(SizeofSockaddrAny, out ptr<_Socklen> _addr_len);
    nfd, err = accept(fd, _addr_rsa, _addr_len);
    if (err != null) {
        return ;
    }
    sa, err = anyToSockaddr(0, _addr_rsa);
    if (err != null) {
        Close(nfd);
        nfd = 0;
    }
    return ;

}

private static void SetLen(this ptr<Iovec> _addr_iov, nint length) {
    ref Iovec iov = ref _addr_iov.val;

    iov.Len = uint64(length);
}

private static void SetControllen(this ptr<Msghdr> _addr_msghdr, nint length) {
    ref Msghdr msghdr = ref _addr_msghdr.val;

    msghdr.Controllen = int32(length);
}

private static void SetLen(this ptr<Cmsghdr> _addr_cmsg, nint length) {
    ref Cmsghdr cmsg = ref _addr_cmsg.val;

    cmsg.Len = int32(length);
}

//sys   fcntl(fd int, cmd int, arg int) (val int, err error)
//sys    read(fd int, p []byte) (n int, err error)
//sys   readlen(fd int, buf *byte, nbuf int) (n int, err error) = SYS_READ
//sys    write(fd int, p []byte) (n int, err error)

//sys    accept(s int, rsa *RawSockaddrAny, addrlen *_Socklen) (fd int, err error) = SYS___ACCEPT_A
//sys    bind(s int, addr unsafe.Pointer, addrlen _Socklen) (err error) = SYS___BIND_A
//sys    connect(s int, addr unsafe.Pointer, addrlen _Socklen) (err error) = SYS___CONNECT_A
//sysnb    getgroups(n int, list *_Gid_t) (nn int, err error)
//sysnb    setgroups(n int, list *_Gid_t) (err error)
//sys    getsockopt(s int, level int, name int, val unsafe.Pointer, vallen *_Socklen) (err error)
//sys    setsockopt(s int, level int, name int, val unsafe.Pointer, vallen uintptr) (err error)
//sysnb    socket(domain int, typ int, proto int) (fd int, err error)
//sysnb    socketpair(domain int, typ int, proto int, fd *[2]int32) (err error)
//sysnb    getpeername(fd int, rsa *RawSockaddrAny, addrlen *_Socklen) (err error) = SYS___GETPEERNAME_A
//sysnb    getsockname(fd int, rsa *RawSockaddrAny, addrlen *_Socklen) (err error) = SYS___GETSOCKNAME_A
//sys    recvfrom(fd int, p []byte, flags int, from *RawSockaddrAny, fromlen *_Socklen) (n int, err error) = SYS___RECVFROM_A
//sys    sendto(s int, buf []byte, flags int, to unsafe.Pointer, addrlen _Socklen) (err error) = SYS___SENDTO_A
//sys    recvmsg(s int, msg *Msghdr, flags int) (n int, err error) = SYS___RECVMSG_A
//sys    sendmsg(s int, msg *Msghdr, flags int) (n int, err error) = SYS___SENDMSG_A
//sys   mmap(addr uintptr, length uintptr, prot int, flag int, fd int, pos int64) (ret uintptr, err error) = SYS_MMAP
//sys   munmap(addr uintptr, length uintptr) (err error) = SYS_MUNMAP
//sys   ioctl(fd int, req uint, arg uintptr) (err error) = SYS_IOCTL

//sys   Access(path string, mode uint32) (err error) = SYS___ACCESS_A
//sys   Chdir(path string) (err error) = SYS___CHDIR_A
//sys    Chown(path string, uid int, gid int) (err error) = SYS___CHOWN_A
//sys    Chmod(path string, mode uint32) (err error) = SYS___CHMOD_A
//sys   Creat(path string, mode uint32) (fd int, err error) = SYS___CREAT_A
//sys    Dup(oldfd int) (fd int, err error)
//sys    Dup2(oldfd int, newfd int) (err error)
//sys    Errno2() (er2 int) = SYS___ERRNO2
//sys    Err2ad() (eadd *int) = SYS___ERR2AD
//sys    Exit(code int)
//sys    Fchdir(fd int) (err error)
//sys    Fchmod(fd int, mode uint32) (err error)
//sys    Fchown(fd int, uid int, gid int) (err error)
//sys    FcntlInt(fd uintptr, cmd int, arg int) (retval int, err error) = SYS_FCNTL
//sys    fstat(fd int, stat *Stat_LE_t) (err error)

public static error Fstat(nint fd, ptr<Stat_t> _addr_stat) {
    error err = default!;
    ref Stat_t stat = ref _addr_stat.val;

    ref Stat_LE_t statLE = ref heap(out ptr<Stat_LE_t> _addr_statLE);
    err = fstat(fd, _addr_statLE);
    copyStat(_addr_stat, _addr_statLE);
    return ;
}

//sys    Fstatvfs(fd int, stat *Statvfs_t) (err error) = SYS_FSTATVFS
//sys    Fsync(fd int) (err error)
//sys    Ftruncate(fd int, length int64) (err error)
//sys   Getpagesize() (pgsize int) = SYS_GETPAGESIZE
//sys   Mprotect(b []byte, prot int) (err error) = SYS_MPROTECT
//sys   Msync(b []byte, flags int) (err error) = SYS_MSYNC
//sys   Poll(fds []PollFd, timeout int) (n int, err error) = SYS_POLL
//sys   Times(tms *Tms) (ticks uintptr, err error) = SYS_TIMES
//sys   W_Getmntent(buff *byte, size int) (lastsys int, err error) = SYS_W_GETMNTENT
//sys   W_Getmntent_A(buff *byte, size int) (lastsys int, err error) = SYS___W_GETMNTENT_A

//sys   mount_LE(path string, filesystem string, fstype string, mtm uint32, parmlen int32, parm string) (err error) = SYS___MOUNT_A
//sys   unmount(filesystem string, mtm int) (err error) = SYS___UMOUNT_A
//sys   Chroot(path string) (err error) = SYS___CHROOT_A
//sys   Select(nmsgsfds int, r *FdSet, w *FdSet, e *FdSet, timeout *Timeval) (ret int, err error) = SYS_SELECT
//sysnb Uname(buf *Utsname) (err error) = SYS___UNAME_A

public static (@string, error) Ptsname(nint fd) {
    @string name = default;
    error err = default!;

    var (r0, _, e1) = syscall_syscall(SYS___PTSNAME_A, uintptr(fd), 0, 0);
    name = u2s(@unsafe.Pointer(r0));
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return ;

}

private static @string u2s(unsafe.Pointer cstr) {
    ptr<array<byte>> str = new ptr<ptr<array<byte>>>(cstr);
    nint i = 0;
    while (str[i] != 0) {
        i++;
    }
    return string(str[..(int)i]);
}

public static error Close(nint fd) {
    error err = default!;

    var (_, _, e1) = syscall_syscall(SYS_CLOSE, uintptr(fd), 0, 0);
    for (nint i = 0; e1 == EAGAIN && i < 10; i++) {
        _, _, _ = syscall_syscall(SYS_USLEEP, uintptr(10), 0, 0);
        _, _, e1 = syscall_syscall(SYS_CLOSE, uintptr(fd), 0, 0);
    }
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return ;

}

private static ptr<mmapper> mapper = addr(new mmapper(active:make(map[*byte][]byte),mmap:mmap,munmap:munmap,));

// Dummy function: there are no semantics for Madvise on z/OS
public static error Madvise(slice<byte> b, nint advice) {
    error err = default!;

    return ;
}

public static (slice<byte>, error) Mmap(nint fd, long offset, nint length, nint prot, nint flags) {
    slice<byte> data = default;
    error err = default!;

    return mapper.Mmap(fd, offset, length, prot, flags);
}

public static error Munmap(slice<byte> b) {
    error err = default!;

    return error.As(mapper.Munmap(b))!;
}

//sys   Gethostname(buf []byte) (err error) = SYS___GETHOSTNAME_A
//sysnb    Getegid() (egid int)
//sysnb    Geteuid() (uid int)
//sysnb    Getgid() (gid int)
//sysnb    Getpid() (pid int)
//sysnb    Getpgid(pid int) (pgid int, err error) = SYS_GETPGID

public static nint Getpgrp() {
    nint pid = default;

    pid, _ = Getpgid(0);
    return ;
}

//sysnb    Getppid() (pid int)
//sys    Getpriority(which int, who int) (prio int, err error)
//sysnb    Getrlimit(resource int, rlim *Rlimit) (err error) = SYS_GETRLIMIT

//sysnb getrusage(who int, rusage *rusage_zos) (err error) = SYS_GETRUSAGE

public static error Getrusage(nint who, ptr<Rusage> _addr_rusage) {
    error err = default!;
    ref Rusage rusage = ref _addr_rusage.val;

    ref rusage_zos ruz = ref heap(out ptr<rusage_zos> _addr_ruz);
    err = getrusage(who, _addr_ruz); 
    //Only the first two fields of Rusage are set
    rusage.Utime.Sec = ruz.Utime.Sec;
    rusage.Utime.Usec = int64(ruz.Utime.Usec);
    rusage.Stime.Sec = ruz.Stime.Sec;
    rusage.Stime.Usec = int64(ruz.Stime.Usec);
    return ;

}

//sysnb Getsid(pid int) (sid int, err error) = SYS_GETSID
//sysnb    Getuid() (uid int)
//sysnb    Kill(pid int, sig Signal) (err error)
//sys    Lchown(path string, uid int, gid int) (err error) = SYS___LCHOWN_A
//sys    Link(path string, link string) (err error) = SYS___LINK_A
//sys    Listen(s int, n int) (err error)
//sys    lstat(path string, stat *Stat_LE_t) (err error) = SYS___LSTAT_A

public static error Lstat(@string path, ptr<Stat_t> _addr_stat) {
    error err = default!;
    ref Stat_t stat = ref _addr_stat.val;

    ref Stat_LE_t statLE = ref heap(out ptr<Stat_LE_t> _addr_statLE);
    err = lstat(path, _addr_statLE);
    copyStat(_addr_stat, _addr_statLE);
    return ;
}

//sys    Mkdir(path string, mode uint32) (err error) = SYS___MKDIR_A
//sys   Mkfifo(path string, mode uint32) (err error) = SYS___MKFIFO_A
//sys    Mknod(path string, mode uint32, dev int) (err error) = SYS___MKNOD_A
//sys    Pread(fd int, p []byte, offset int64) (n int, err error)
//sys    Pwrite(fd int, p []byte, offset int64) (n int, err error)
//sys    Readlink(path string, buf []byte) (n int, err error) = SYS___READLINK_A
//sys    Rename(from string, to string) (err error) = SYS___RENAME_A
//sys    Rmdir(path string) (err error) = SYS___RMDIR_A
//sys   Seek(fd int, offset int64, whence int) (off int64, err error) = SYS_LSEEK
//sys    Setpriority(which int, who int, prio int) (err error)
//sysnb    Setpgid(pid int, pgid int) (err error) = SYS_SETPGID
//sysnb    Setrlimit(resource int, lim *Rlimit) (err error)
//sysnb    Setregid(rgid int, egid int) (err error) = SYS_SETREGID
//sysnb    Setreuid(ruid int, euid int) (err error) = SYS_SETREUID
//sysnb    Setsid() (pid int, err error) = SYS_SETSID
//sys    Setuid(uid int) (err error) = SYS_SETUID
//sys    Setgid(uid int) (err error) = SYS_SETGID
//sys    Shutdown(fd int, how int) (err error)
//sys    stat(path string, statLE *Stat_LE_t) (err error) = SYS___STAT_A

public static error Stat(@string path, ptr<Stat_t> _addr_sta) {
    error err = default!;
    ref Stat_t sta = ref _addr_sta.val;

    ref Stat_LE_t statLE = ref heap(out ptr<Stat_LE_t> _addr_statLE);
    err = stat(path, _addr_statLE);
    copyStat(_addr_sta, _addr_statLE);
    return ;
}

//sys    Symlink(path string, link string) (err error) = SYS___SYMLINK_A
//sys    Sync() = SYS_SYNC
//sys    Truncate(path string, length int64) (err error) = SYS___TRUNCATE_A
//sys    Tcgetattr(fildes int, termptr *Termios) (err error) = SYS_TCGETATTR
//sys    Tcsetattr(fildes int, when int, termptr *Termios) (err error) = SYS_TCSETATTR
//sys    Umask(mask int) (oldmask int)
//sys    Unlink(path string) (err error) = SYS___UNLINK_A
//sys    Utime(path string, utim *Utimbuf) (err error) = SYS___UTIME_A

//sys    open(path string, mode int, perm uint32) (fd int, err error) = SYS___OPEN_A

public static (nint, error) Open(@string path, nint mode, uint perm) {
    nint fd = default;
    error err = default!;

    return open(path, mode, perm);
}

public static error Mkfifoat(nint dirfd, @string path, uint mode) => func((defer, _, _) => {
    error err = default!;

    var (wd, err) = Getwd();
    if (err != null) {
        return error.As(err)!;
    }
    {
        var err = Fchdir(dirfd);

        if (err != null) {
            return error.As(err)!;
        }
    }

    defer(Chdir(wd));

    return error.As(Mkfifo(path, mode))!;

});

//sys    remove(path string) (err error)

public static error Remove(@string path) {
    return error.As(remove(path))!;
}

public static readonly var ImplementsGetwd = true;



public static (nint, error) Getcwd(slice<byte> buf) {
    nint n = default;
    error err = default!;

    unsafe.Pointer p = default;
    if (len(buf) > 0) {
        p = @unsafe.Pointer(_addr_buf[0]);
    }
    else
 {
        p = @unsafe.Pointer(_addr__zero);
    }
    var (_, _, e) = syscall_syscall(SYS___GETCWD_A, uintptr(p), uintptr(len(buf)), 0);
    n = clen(buf) + 1;
    if (e != 0) {
        err = errnoErr(e);
    }
    return ;

}

public static (@string, error) Getwd() {
    @string wd = default;
    error err = default!;

    array<byte> buf = new array<byte>(PathMax);
    var (n, err) = Getcwd(buf[(int)0..]);
    if (err != null) {
        return ("", error.As(err)!);
    }
    if (n < 1 || n > len(buf) || buf[n - 1] != 0) {
        return ("", error.As(EINVAL)!);
    }
    return (string(buf[(int)0..(int)n - 1]), error.As(null!)!);

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
    if (n < 0 || n > 1 << 20) {
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

private static ulong gettid();

public static nint Gettid() {
    nint tid = default;

    return int(gettid());
}

public partial struct WaitStatus { // : uint
}

// Wait status is 7 bits at bottom, either 0 (exited),
// 0x7F (stopped), or a signal number that caused an exit.
// The 0x80 bit is whether there was a core dump.
// An extra number (exit code, signal causing a stop)
// is in the high bits.  At least that's the idea.
// There are various irregularities.  For example, the
// "continued" status is 0xFFFF, distinguishing itself
// from stopped via the core dump bit.

private static readonly nuint mask = 0x7F;
private static readonly nuint core = 0x80;
private static readonly nuint exited = 0x00;
private static readonly nuint stopped = 0x7F;
private static readonly nint shift = 8;


public static bool Exited(this WaitStatus w) {
    return w & mask == exited;
}

public static bool Signaled(this WaitStatus w) {
    return w & mask != stopped && w & mask != exited;
}

public static bool Stopped(this WaitStatus w) {
    return w & 0xFF == stopped;
}

public static bool Continued(this WaitStatus w) {
    return w == 0xFFFF;
}

public static bool CoreDump(this WaitStatus w) {
    return w.Signaled() && w & core != 0;
}

public static nint ExitStatus(this WaitStatus w) {
    if (!w.Exited()) {>>MARKER:FUNCTION_gettid_BLOCK_PREFIX<<
        return -1;
    }
    return int(w >> (int)(shift)) & 0xFF;

}

public static Signal Signal(this WaitStatus w) {
    if (!w.Signaled()) {
        return -1;
    }
    return Signal(w & mask);

}

public static Signal StopSignal(this WaitStatus w) {
    if (!w.Stopped()) {
        return -1;
    }
    return Signal(w >> (int)(shift)) & 0xFF;

}

public static nint TrapCause(this WaitStatus w) {
    return -1;
}

//sys    waitpid(pid int, wstatus *_C_int, options int) (wpid int, err error)

public static (nint, error) Wait4(nint pid, ptr<WaitStatus> _addr_wstatus, nint options, ptr<Rusage> _addr_rusage) {
    nint wpid = default;
    error err = default!;
    ref WaitStatus wstatus = ref _addr_wstatus.val;
    ref Rusage rusage = ref _addr_rusage.val;
 
    // TODO(mundaym): z/OS doesn't have wait4. I don't think getrusage does what we want.
    // At the moment rusage will not be touched.
    ref _C_int status = ref heap(out ptr<_C_int> _addr_status);
    wpid, err = waitpid(pid, _addr_status, options);
    if (wstatus != null) {
        wstatus = WaitStatus(status);
    }
    return ;

}

//sysnb    gettimeofday(tv *timeval_zos) (err error)

public static error Gettimeofday(ptr<Timeval> _addr_tv) {
    error err = default!;
    ref Timeval tv = ref _addr_tv.val;

    ref timeval_zos tvz = ref heap(out ptr<timeval_zos> _addr_tvz);
    err = gettimeofday(_addr_tvz);
    tv.Sec = tvz.Sec;
    tv.Usec = int64(tvz.Usec);
    return ;
}

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

private static Timespec setTimespec(long sec, long nsec) {
    return new Timespec(Sec:sec,Nsec:nsec);
}

private static Timeval setTimeval(long sec, long usec) { //fix
    return new Timeval(Sec:sec,Usec:usec);

}

//sysnb pipe(p *[2]_C_int) (err error)

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

//sys    utimes(path string, timeval *[2]Timeval) (err error) = SYS___UTIMES_A

public static error Utimes(@string path, slice<Timeval> tv) {
    error err = default!;

    if (len(tv) != 2) {
        return error.As(EINVAL)!;
    }
    return error.As(utimes(path, new ptr<ptr<array<Timeval>>>(@unsafe.Pointer(_addr_tv[0]))))!;

}

public static error UtimesNano(@string path, slice<Timespec> ts) {
    if (len(ts) != 2) {
        return error.As(EINVAL)!;
    }
    array<Timeval> tv = new array<Timeval>(new Timeval[] { NsecToTimeval(TimespecToNsec(ts[0])), NsecToTimeval(TimespecToNsec(ts[1])) });
    return error.As(utimes(path, new ptr<ptr<array<Timeval>>>(@unsafe.Pointer(_addr_tv[0]))))!;

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
    return anyToSockaddr(0, _addr_rsa);

}

 
// identifier constants
private static readonly nuint nwmHeaderIdentifier = 0xd5e6d4c8;
private static readonly nuint nwmFilterIdentifier = 0xd5e6d4c6;
private static readonly nuint nwmTCPConnIdentifier = 0xd5e6d4c3;
private static readonly nuint nwmRecHeaderIdentifier = 0xd5e6d4d9;
private static readonly nuint nwmIPStatsIdentifier = 0xd5e6d4c9d7e2e340;
private static readonly nuint nwmIPGStatsIdentifier = 0xd5e6d4c9d7c7e2e3;
private static readonly nuint nwmTCPStatsIdentifier = 0xd5e6d4e3c3d7e2e3;
private static readonly nuint nwmUDPStatsIdentifier = 0xd5e6d4e4c4d7e2e3;
private static readonly nuint nwmICMPGStatsEntry = 0xd5e6d4c9c3d4d7c7;
private static readonly nuint nwmICMPTStatsEntry = 0xd5e6d4c9c3d4d7e3; 

// nwmHeader constants
private static readonly nint nwmVersion1 = 1;
private static readonly nint nwmVersion2 = 2;
private static readonly nint nwmCurrentVer = 2;

private static readonly nint nwmTCPConnType = 1;
private static readonly nint nwmGlobalStatsType = 14; 

// nwmFilter constants
private static readonly nuint nwmFilterLclAddrMask = 0x20000000; // Local address
private static readonly nuint nwmFilterSrcAddrMask = 0x20000000; // Source address
private static readonly nuint nwmFilterLclPortMask = 0x10000000; // Local port
private static readonly nuint nwmFilterSrcPortMask = 0x10000000; // Source port

// nwmConnEntry constants
private static readonly nint nwmTCPStateClosed = 1;
private static readonly nint nwmTCPStateListen = 2;
private static readonly nint nwmTCPStateSynSent = 3;
private static readonly nint nwmTCPStateSynRcvd = 4;
private static readonly nint nwmTCPStateEstab = 5;
private static readonly nint nwmTCPStateFinWait1 = 6;
private static readonly nint nwmTCPStateFinWait2 = 7;
private static readonly nint nwmTCPStateClosWait = 8;
private static readonly nint nwmTCPStateLastAck = 9;
private static readonly nint nwmTCPStateClosing = 10;
private static readonly nint nwmTCPStateTimeWait = 11;
private static readonly nint nwmTCPStateDeletTCB = 12; 

// Existing constants on linux
public static readonly nint BPF_TCP_CLOSE = 1;
public static readonly nint BPF_TCP_LISTEN = 2;
public static readonly nint BPF_TCP_SYN_SENT = 3;
public static readonly nint BPF_TCP_SYN_RECV = 4;
public static readonly nint BPF_TCP_ESTABLISHED = 5;
public static readonly nint BPF_TCP_FIN_WAIT1 = 6;
public static readonly nint BPF_TCP_FIN_WAIT2 = 7;
public static readonly nint BPF_TCP_CLOSE_WAIT = 8;
public static readonly nint BPF_TCP_LAST_ACK = 9;
public static readonly nint BPF_TCP_CLOSING = 10;
public static readonly nint BPF_TCP_TIME_WAIT = 11;
public static readonly nint BPF_TCP_NEW_SYN_RECV = -1;
public static readonly nint BPF_TCP_MAX_STATES = -2;


private partial struct nwmTriplet {
    public uint offset;
    public uint length;
    public uint number;
}

private partial struct nwmQuadruplet {
    public uint offset;
    public uint length;
    public uint number;
    public uint match;
}

private partial struct nwmHeader {
    public uint ident;
    public uint length;
    public ushort version;
    public ushort nwmType;
    public uint bytesNeeded;
    public uint options;
    public array<byte> _;
    public nwmTriplet inputDesc;
    public nwmQuadruplet outputDesc;
}

private partial struct nwmFilter {
    public uint ident;
    public uint flags;
    public array<byte> resourceName;
    public uint resourceId;
    public uint listenerId;
    public array<byte> local; // union of sockaddr4 and sockaddr6
    public array<byte> remote; // union of sockaddr4 and sockaddr6
    public ushort _;
    public ushort _;
    public ushort asid;
    public array<byte> _;
    public array<byte> tnLuName;
    public uint tnMonGrp;
    public array<byte> tnAppl;
    public array<byte> applData;
    public array<byte> nInterface;
    public array<byte> dVipa;
    public ushort dVipaPfx;
    public ushort dVipaPort;
    public byte dVipaFamily;
    public array<byte> _;
    public array<byte> destXCF;
    public ushort destXCFPfx;
    public byte destXCFFamily;
    public array<byte> _;
    public array<byte> targIP;
    public ushort targIPPfx;
    public byte targIPFamily;
    public array<byte> _;
    public array<byte> _;
}

private partial struct nwmRecHeader {
    public uint ident;
    public uint length;
    public byte number;
    public array<byte> _;
}

private partial struct nwmTCPStatsEntry {
    public ulong ident;
    public uint currEstab;
    public uint activeOpened;
    public uint passiveOpened;
    public uint connClosed;
    public uint estabResets;
    public uint attemptFails;
    public uint passiveDrops;
    public uint timeWaitReused;
    public ulong inSegs;
    public uint predictAck;
    public uint predictData;
    public uint inDupAck;
    public uint inBadSum;
    public uint inBadLen;
    public uint inShort;
    public uint inDiscOldTime;
    public uint inAllBeforeWin;
    public uint inSomeBeforeWin;
    public uint inAllAfterWin;
    public uint inSomeAfterWin;
    public uint inOutOfOrder;
    public uint inAfterClose;
    public uint inWinProbes;
    public uint inWinUpdates;
    public uint outWinUpdates;
    public ulong outSegs;
    public uint outDelayAcks;
    public uint outRsts;
    public uint retransSegs;
    public uint retransTimeouts;
    public uint retransDrops;
    public uint pmtuRetrans;
    public uint pmtuErrors;
    public uint outWinProbes;
    public uint probeDrops;
    public uint keepAliveProbes;
    public uint keepAliveDrops;
    public uint finwait2Drops;
    public ulong acceptCount;
    public ulong inBulkQSegs;
    public ulong inDiscards;
    public uint connFloods;
    public uint connStalls;
    public ushort cfgEphemDef;
    public ushort ephemInUse;
    public ushort ephemHiWater;
    public byte flags;
    public array<byte> _;
    public uint ephemExhaust;
    public uint smcRCurrEstabLnks;
    public uint smcRLnkActTimeOut;
    public uint smcRActLnkOpened;
    public uint smcRPasLnkOpened;
    public uint smcRLnksClosed;
    public uint smcRCurrEstab;
    public uint smcRActiveOpened;
    public uint smcRPassiveOpened;
    public uint smcRConnClosed;
    public ulong smcRInSegs;
    public ulong smcROutSegs;
    public uint smcRInRsts;
    public uint smcROutRsts;
    public uint smcDCurrEstabLnks;
    public uint smcDActLnkOpened;
    public uint smcDPasLnkOpened;
    public uint smcDLnksClosed;
    public uint smcDCurrEstab;
    public uint smcDActiveOpened;
    public uint smcDPassiveOpened;
    public uint smcDConnClosed;
    public ulong smcDInSegs;
    public ulong smcDOutSegs;
    public uint smcDInRsts;
    public uint smcDOutRsts;
}

private partial struct nwmConnEntry {
    public uint ident;
    public array<byte> local; // union of sockaddr4 and sockaddr6
    public array<byte> remote; // union of sockaddr4 and sockaddr6
    public array<byte> startTime; // uint64, changed to prevent padding from being inserted
    public array<byte> lastActivity; // uint64
    public array<byte> bytesIn; // uint64
    public array<byte> bytesOut; // uint64
    public array<byte> inSegs; // uint64
    public array<byte> outSegs; // uint64
    public ushort state;
    public byte activeOpen;
    public byte flag01;
    public uint outBuffered;
    public uint inBuffered;
    public uint maxSndWnd;
    public uint reXmtCount;
    public uint congestionWnd;
    public uint ssThresh;
    public uint roundTripTime;
    public uint roundTripVar;
    public uint sendMSS;
    public uint sndWnd;
    public uint rcvBufSize;
    public uint sndBufSize;
    public uint outOfOrderCount;
    public uint lcl0WindowCount;
    public uint rmt0WindowCount;
    public uint dupacks;
    public byte flag02;
    public byte sockOpt6Cont;
    public ushort asid;
    public array<byte> resourceName;
    public uint resourceId;
    public uint subtask;
    public byte sockOpt;
    public byte sockOpt6;
    public byte clusterConnFlag;
    public byte proto;
    public array<byte> targetAppl;
    public array<byte> luName;
    public array<byte> clientUserId;
    public array<byte> logMode;
    public uint timeStamp;
    public uint timeStampAge;
    public uint serverResourceId;
    public array<byte> intfName;
    public byte ttlsStatPol;
    public byte ttlsStatConn;
    public ushort ttlsSSLProt;
    public array<byte> ttlsNegCiph;
    public byte ttlsSecType;
    public byte ttlsFIPS140Mode;
    public array<byte> ttlsUserID;
    public array<byte> applData;
    public array<byte> inOldestTime; // uint64
    public array<byte> outOldestTime; // uint64
    public byte tcpTrustedPartner;
    public array<byte> _;
    public array<byte> bulkDataIntfName;
    public array<byte> ttlsNegCiph4;
    public uint smcReason;
    public uint lclSMCLinkId;
    public uint rmtSMCLinkId;
    public byte smcStatus;
    public byte smcFlags;
    public array<byte> _;
    public uint rcvWnd;
    public uint lclSMCBufSz;
    public uint rmtSMCBufSz;
    public array<byte> ttlsSessID;
    public short ttlsSessIDLen;
    public array<byte> _;
    public byte smcDStatus;
    public uint smcDReason;
}

private static slice<slice<byte>> svcNameTable = new slice<slice<byte>>(new slice<byte>[] { []byte("\xc5\xe9\xc2\xd5\xd4\xc9\xc6\xf4") });

private static readonly nint svc_EZBNMIF4 = 0;


public static (ptr<TCPInfo>, error) GetsockoptTCPInfo(nint fd, nint level, nint opt) {
    ptr<TCPInfo> _p0 = default!;
    error _p0 = default!;

    slice<byte> jobname = (slice<byte>)"\x5c\x40\x40\x40\x40\x40\x40\x40"; // "*"
    array<byte> responseBuffer = new array<byte>(new byte[] { 0 });
    ref uint bufferAlet = ref heap(0, out ptr<uint> _addr_bufferAlet);    ref uint reasonCode = ref heap(0, out ptr<uint> _addr_reasonCode);

    ref int bufferLen = ref heap(4096, out ptr<int> _addr_bufferLen);    ref int returnValue = ref heap(0, out ptr<int> _addr_returnValue);    ref int returnCode = ref heap(0, out ptr<int> _addr_returnCode);



    array<ulong> dsa = new array<ulong>(new ulong[] { 0 });
    array<unsafe.Pointer> argv = new array<unsafe.Pointer>(7);
    argv[0] = @unsafe.Pointer(_addr_jobname[0]);
    argv[1] = @unsafe.Pointer(_addr_responseBuffer[0]);
    argv[2] = @unsafe.Pointer(_addr_bufferAlet);
    argv[3] = @unsafe.Pointer(_addr_bufferLen);
    argv[4] = @unsafe.Pointer(_addr_returnValue);
    argv[5] = @unsafe.Pointer(_addr_returnCode);
    argv[6] = @unsafe.Pointer(_addr_reasonCode);

    var EZBNMIF4 = svcLoad(_addr_svcNameTable[svc_EZBNMIF4][0]);
    if (EZBNMIF4 == null) {
        return (_addr_null!, error.As(errnoErr(EINVAL))!);
    }
    request.header.ident = nwmHeaderIdentifier;
    request.header.length = uint32(@unsafe.Sizeof(request.header));
    request.header.version = nwmCurrentVer;
    request.header.nwmType = nwmGlobalStatsType;
    request.header.options = 0x80000000;

    svcCall(EZBNMIF4, _addr_argv[0], _addr_dsa[0]); 

    // outputDesc field is filled by EZBNMIF4 on success
    if (returnCode != 0 || request.header.outputDesc.offset == 0) {
        return (_addr_null!, error.As(errnoErr(EINVAL))!);
    }
    var recHeader = (nwmRecHeader.val)(@unsafe.Pointer(_addr_responseBuffer[request.header.outputDesc.offset]));
    if (recHeader.ident != nwmRecHeaderIdentifier) {
        return (_addr_null!, error.As(errnoErr(EINVAL))!);
    }
    slice<ptr<ulong>> sections = default;
    ptr<nwmTriplet> sectionDesc(nwmTriplet.val)(@unsafe.Pointer(_addr_responseBuffer[0]));
    {
        var i__prev1 = i;

        for (var i = uint32(0); i < uint32(recHeader.number); i++) {
            var offset = request.header.outputDesc.offset + uint32(@unsafe.Sizeof(recHeader.val)) + i * uint32(@unsafe.Sizeof(sectionDesc.val));
            sectionDesc = (nwmTriplet.val)(@unsafe.Pointer(_addr_responseBuffer[offset]));
            for (var j = uint32(0); j < sectionDesc.number; j++) {
                offset = request.header.outputDesc.offset + sectionDesc.offset + j * sectionDesc.length;
                sections = append(sections, (uint64.val)(@unsafe.Pointer(_addr_responseBuffer[offset])));
            }
        }

        i = i__prev1;
    } 

    // Find nwmTCPStatsEntry in returned entries
    ptr<nwmTCPStatsEntry> tcpStatsnull;
    foreach (var (_, ptr) in sections) {

        if (ptr.val == nwmTCPStatsIdentifier) 
            if (tcpStats != null) {
                return (_addr_null!, error.As(errnoErr(EINVAL))!);
            }
            tcpStats = (nwmTCPStatsEntry.val)(@unsafe.Pointer(ptr));
        else if (ptr.val == nwmIPStatsIdentifier)         else if (ptr.val == nwmIPGStatsIdentifier)         else if (ptr.val == nwmUDPStatsIdentifier)         else if (ptr.val == nwmICMPGStatsEntry)         else if (ptr.val == nwmICMPTStatsEntry)         else 
            return (_addr_null!, error.As(errnoErr(EINVAL))!);
        
    }    if (tcpStats == null) {
        return (_addr_null!, error.As(errnoErr(EINVAL))!);
    }
    responseBuffer = new array<byte>(new byte[] { 0 });
    dsa = new array<ulong>(new ulong[] { 0 });
    (bufferAlet, reasonCode) = (0, 0);    (bufferLen, returnValue, returnCode) = (4096, 0, 0);    var nameptr = (uint32.val)(@unsafe.Pointer(uintptr(0x21c))); // Get jobname of current process
    nameptr = (uint32.val)(@unsafe.Pointer(uintptr(nameptr + 12.val)));
    argv[0] = @unsafe.Pointer(uintptr(nameptr.val));

    request.header.ident = nwmHeaderIdentifier;
    request.header.length = uint32(@unsafe.Sizeof(request.header));
    request.header.version = nwmCurrentVer;
    request.header.nwmType = nwmTCPConnType;
    request.header.options = 0x80000000;

    request.filter.ident = nwmFilterIdentifier;

    ref RawSockaddrAny localSockaddr = ref heap(out ptr<RawSockaddrAny> _addr_localSockaddr);
    ref var socklen = ref heap(_Socklen(SizeofSockaddrAny), out ptr<var> _addr_socklen);
    var err = getsockname(fd, _addr_localSockaddr, _addr_socklen);
    if (err != null) {
        return (_addr_null!, error.As(errnoErr(EINVAL))!);
    }
    if (localSockaddr.Addr.Family == AF_INET) {
        localSockaddr = (RawSockaddrInet4.val)(@unsafe.Pointer(_addr_localSockaddr.Addr));
        var localSockFilter = (RawSockaddrInet4.val)(@unsafe.Pointer(_addr_request.filter.local[0]));
        localSockFilter.Family = AF_INET;
        i = default;
        for (i = 0; i < 4; i++) {
            if (localSockaddr.Addr[i] != 0) {
                break;
            }
        }
        if (i != 4) {
            request.filter.flags |= nwmFilterLclAddrMask;
            for (i = 0; i < 4; i++) {
                localSockFilter.Addr[i] = localSockaddr.Addr[i];
            }
        }
        if (localSockaddr.Port != 0) {
            request.filter.flags |= nwmFilterLclPortMask;
            localSockFilter.Port = localSockaddr.Port;
        }
    }
    else if (localSockaddr.Addr.Family == AF_INET6) {
        localSockaddr = (RawSockaddrInet6.val)(@unsafe.Pointer(_addr_localSockaddr.Addr));
        localSockFilter = (RawSockaddrInet6.val)(@unsafe.Pointer(_addr_request.filter.local[0]));
        localSockFilter.Family = AF_INET6;
        i = default;
        for (i = 0; i < 16; i++) {
            if (localSockaddr.Addr[i] != 0) {
                break;
            }
        }
        if (i != 16) {
            request.filter.flags |= nwmFilterLclAddrMask;
            for (i = 0; i < 16; i++) {
                localSockFilter.Addr[i] = localSockaddr.Addr[i];
            }
        }
        if (localSockaddr.Port != 0) {
            request.filter.flags |= nwmFilterLclPortMask;
            localSockFilter.Port = localSockaddr.Port;
        }
    }
    svcCall(EZBNMIF4, _addr_argv[0], _addr_dsa[0]); 

    // outputDesc field is filled by EZBNMIF4 on success
    if (returnCode != 0 || request.header.outputDesc.offset == 0) {
        return (_addr_null!, error.As(errnoErr(EINVAL))!);
    }
    var conn = (nwmConnEntry.val)(@unsafe.Pointer(_addr_responseBuffer[request.header.outputDesc.offset]));
    if (conn.ident != nwmTCPConnIdentifier) {
        return (_addr_null!, error.As(errnoErr(EINVAL))!);
    }
    ref TCPInfo tcpinfo = ref heap(out ptr<TCPInfo> _addr_tcpinfo);
    tcpinfo.State = uint8(conn.state);
    tcpinfo.Ca_state = 0; // dummy
    tcpinfo.Retransmits = uint8(tcpStats.retransSegs);
    tcpinfo.Probes = uint8(tcpStats.outWinProbes);
    tcpinfo.Backoff = 0; // dummy
    tcpinfo.Options = 0; // dummy
    tcpinfo.Rto = tcpStats.retransTimeouts;
    tcpinfo.Ato = tcpStats.outDelayAcks;
    tcpinfo.Snd_mss = conn.sendMSS;
    tcpinfo.Rcv_mss = conn.sendMSS; // dummy
    tcpinfo.Unacked = 0; // dummy
    tcpinfo.Sacked = 0; // dummy
    tcpinfo.Lost = 0; // dummy
    tcpinfo.Retrans = conn.reXmtCount;
    tcpinfo.Fackets = 0; // dummy
    tcpinfo.Last_data_sent = uint32(new ptr<ptr<ptr<ulong>>>(@unsafe.Pointer(_addr_conn.lastActivity[0])));
    tcpinfo.Last_ack_sent = uint32(new ptr<ptr<ptr<ulong>>>(@unsafe.Pointer(_addr_conn.outOldestTime[0])));
    tcpinfo.Last_data_recv = uint32(new ptr<ptr<ptr<ulong>>>(@unsafe.Pointer(_addr_conn.inOldestTime[0])));
    tcpinfo.Last_ack_recv = uint32(new ptr<ptr<ptr<ulong>>>(@unsafe.Pointer(_addr_conn.inOldestTime[0])));
    tcpinfo.Pmtu = conn.sendMSS; // dummy, NWMIfRouteMtu is a candidate
    tcpinfo.Rcv_ssthresh = conn.ssThresh;
    tcpinfo.Rtt = conn.roundTripTime;
    tcpinfo.Rttvar = conn.roundTripVar;
    tcpinfo.Snd_ssthresh = conn.ssThresh; // dummy
    tcpinfo.Snd_cwnd = conn.congestionWnd;
    tcpinfo.Advmss = conn.sendMSS; // dummy
    tcpinfo.Reordering = 0; // dummy
    tcpinfo.Rcv_rtt = conn.roundTripTime; // dummy
    tcpinfo.Rcv_space = conn.sendMSS; // dummy
    tcpinfo.Total_retrans = conn.reXmtCount;

    svcUnload(_addr_svcNameTable[svc_EZBNMIF4][0], EZBNMIF4);

    return (_addr__addr_tcpinfo!, error.As(null!)!);

}

// GetsockoptString returns the string value of the socket option opt for the
// socket associated with fd at the given socket level.
public static (@string, error) GetsockoptString(nint fd, nint level, nint opt) {
    @string _p0 = default;
    error _p0 = default!;

    var buf = make_slice<byte>(256);
    ref var vallen = ref heap(_Socklen(len(buf)), out ptr<var> _addr_vallen);
    var err = getsockopt(fd, level, opt, @unsafe.Pointer(_addr_buf[0]), _addr_vallen);
    if (err != null) {
        return ("", error.As(err)!);
    }
    return (string(buf[..(int)vallen - 1]), error.As(null!)!);

}

public static (nint, nint, nint, Sockaddr, error) Recvmsg(nint fd, slice<byte> p, slice<byte> oob, nint flags) {
    nint n = default;
    nint oobn = default;
    nint recvflags = default;
    Sockaddr from = default;
    error err = default!;

    ref Msghdr msg = ref heap(out ptr<Msghdr> _addr_msg);
    ref RawSockaddrAny rsa = ref heap(out ptr<RawSockaddrAny> _addr_rsa);
    msg.Name = (byte.val)(@unsafe.Pointer(_addr_rsa));
    msg.Namelen = SizeofSockaddrAny;
    ref Iovec iov = ref heap(out ptr<Iovec> _addr_iov);
    if (len(p) > 0) {
        iov.Base = (byte.val)(@unsafe.Pointer(_addr_p[0]));
        iov.SetLen(len(p));
    }
    ref byte dummy = ref heap(out ptr<byte> _addr_dummy);
    if (len(oob) > 0) { 
        // receive at least one normal byte
        if (len(p) == 0) {
            _addr_iov.Base = _addr_dummy;
            iov.Base = ref _addr_iov.Base.val;
            iov.SetLen(1);

        }
        msg.Control = (byte.val)(@unsafe.Pointer(_addr_oob[0]));
        msg.SetControllen(len(oob));

    }
    _addr_msg.Iov = _addr_iov;
    msg.Iov = ref _addr_msg.Iov.val;
    msg.Iovlen = 1;
    n, err = recvmsg(fd, _addr_msg, flags);

    if (err != null) {
        return ;
    }
    oobn = int(msg.Controllen);
    recvflags = int(msg.Flags); 
    // source address is only specified if the socket is unconnected
    if (rsa.Addr.Family != AF_UNSPEC) { 
        // TODO(neeilan): Remove 0 arg added to get this compiling on z/OS
        from, err = anyToSockaddr(0, _addr_rsa);

    }
    return ;

}

public static error Sendmsg(nint fd, slice<byte> p, slice<byte> oob, Sockaddr to, nint flags) {
    error err = default!;

    _, err = SendmsgN(fd, p, oob, to, flags);
    return ;
}

public static (nint, error) SendmsgN(nint fd, slice<byte> p, slice<byte> oob, Sockaddr to, nint flags) {
    nint n = default;
    error err = default!;

    unsafe.Pointer ptr = default;
    _Socklen salen = default;
    if (to != null) {
        error err = default!;
        ptr, salen, err = to.sockaddr();
        if (err != null) {
            return (0, error.As(err)!);
        }
    }
    ref Msghdr msg = ref heap(out ptr<Msghdr> _addr_msg);
    msg.Name = (byte.val)(@unsafe.Pointer(ptr));
    msg.Namelen = int32(salen);
    ref Iovec iov = ref heap(out ptr<Iovec> _addr_iov);
    if (len(p) > 0) {
        iov.Base = (byte.val)(@unsafe.Pointer(_addr_p[0]));
        iov.SetLen(len(p));
    }
    ref byte dummy = ref heap(out ptr<byte> _addr_dummy);
    if (len(oob) > 0) { 
        // send at least one normal byte
        if (len(p) == 0) {
            _addr_iov.Base = _addr_dummy;
            iov.Base = ref _addr_iov.Base.val;
            iov.SetLen(1);

        }
        msg.Control = (byte.val)(@unsafe.Pointer(_addr_oob[0]));
        msg.SetControllen(len(oob));

    }
    _addr_msg.Iov = _addr_iov;
    msg.Iov = ref _addr_msg.Iov.val;
    msg.Iovlen = 1;
    n, err = sendmsg(fd, _addr_msg, flags);

    if (err != null) {
        return (0, error.As(err)!);
    }
    if (len(oob) > 0 && len(p) == 0) {
        n = 0;
    }
    return (n, error.As(null!)!);

}

public static (System.UIntPtr, error) Opendir(@string name) {
    System.UIntPtr _p0 = default;
    error _p0 = default!;

    var (p, err) = BytePtrFromString(name);
    if (err != null) {
        return (0, error.As(err)!);
    }
    var (dir, _, e) = syscall_syscall(SYS___OPENDIR_A, uintptr(@unsafe.Pointer(p)), 0, 0);
    runtime.KeepAlive(@unsafe.Pointer(p));
    if (e != 0) {
        err = errnoErr(e);
    }
    return (dir, error.As(err)!);

}

// clearsyscall.Errno resets the errno value to 0.
private static void clearErrno();

public static (ptr<Dirent>, error) Readdir(System.UIntPtr dir) {
    ptr<Dirent> _p0 = default!;
    error _p0 = default!;

    ref Dirent ent = ref heap(out ptr<Dirent> _addr_ent);
    ref System.UIntPtr res = ref heap(out ptr<System.UIntPtr> _addr_res); 
    // __readdir_r_a returns errno at the end of the directory stream, rather than 0.
    // Therefore to avoid false positives we clear errno before calling it.

    // TODO(neeilan): Commented this out to get sys/unix compiling on z/OS. Uncomment and fix. Error: "undefined: clearsyscall"
    //clearsyscall.Errno() // TODO(mundaym): check pre-emption rules.

    var (e, _, _) = syscall_syscall(SYS___READDIR_R_A, dir, uintptr(@unsafe.Pointer(_addr_ent)), uintptr(@unsafe.Pointer(_addr_res)));
    error err = default!;
    if (e != 0) {>>MARKER:FUNCTION_clearErrno_BLOCK_PREFIX<<
        err = error.As(errnoErr(Errno(e)))!;
    }
    if (res == 0) {
        return (_addr_null!, error.As(err)!);
    }
    return (_addr__addr_ent!, error.As(err)!);

}

public static error Closedir(System.UIntPtr dir) {
    var (_, _, e) = syscall_syscall(SYS_CLOSEDIR, dir, 0, 0);
    if (e != 0) {
        return error.As(errnoErr(e))!;
    }
    return error.As(null!)!;

}

public static void Seekdir(System.UIntPtr dir, nint pos) {
    _, _, _ = syscall_syscall(SYS_SEEKDIR, dir, uintptr(pos), 0);
}

public static (nint, error) Telldir(System.UIntPtr dir) {
    nint _p0 = default;
    error _p0 = default!;

    var (p, _, e) = syscall_syscall(SYS_TELLDIR, dir, 0, 0);
    var pos = int(p);
    if (pos == -1) {
        return (pos, error.As(errnoErr(e))!);
    }
    return (pos, error.As(null!)!);

}

// FcntlFlock performs a fcntl syscall for the F_GETLK, F_SETLK or F_SETLKW command.
public static error FcntlFlock(System.UIntPtr fd, nint cmd, ptr<Flock_t> _addr_lk) {
    ref Flock_t lk = ref _addr_lk.val;
 
    // struct flock is packed on z/OS. We can't emulate that in Go so
    // instead we pack it here.
    ref array<byte> flock = ref heap(new array<byte>(24), out ptr<array<byte>> _addr_flock);
    (int16.val)(@unsafe.Pointer(_addr_flock[0])).val;

    lk.Type * (int16.val)(@unsafe.Pointer(_addr_flock[2]));

    lk.Whence * (int64.val)(@unsafe.Pointer(_addr_flock[4]));

    lk.Start * (int64.val)(@unsafe.Pointer(_addr_flock[12]));

    lk.Len * (int32.val)(@unsafe.Pointer(_addr_flock[20]));

    lk.Pid;
    var (_, _, errno) = syscall_syscall(SYS_FCNTL, fd, uintptr(cmd), uintptr(@unsafe.Pointer(_addr_flock)));
    lk.Type = new ptr<ptr<ptr<short>>>(@unsafe.Pointer(_addr_flock[0]));
    lk.Whence = new ptr<ptr<ptr<short>>>(@unsafe.Pointer(_addr_flock[2]));
    lk.Start = new ptr<ptr<ptr<long>>>(@unsafe.Pointer(_addr_flock[4]));
    lk.Len = new ptr<ptr<ptr<long>>>(@unsafe.Pointer(_addr_flock[12]));
    lk.Pid = new ptr<ptr<ptr<int>>>(@unsafe.Pointer(_addr_flock[20]));
    if (errno == 0) {
        return error.As(null!)!;
    }
    return error.As(errno)!;

}

public static error Flock(nint fd, nint how) {
    short flock_type = default;
    nint fcntl_cmd = default;


    if (how == LOCK_SH | LOCK_NB) 
        flock_type = F_RDLCK;
        fcntl_cmd = F_SETLK;
    else if (how == LOCK_EX | LOCK_NB) 
        flock_type = F_WRLCK;
        fcntl_cmd = F_SETLK;
    else if (how == LOCK_EX) 
        flock_type = F_WRLCK;
        fcntl_cmd = F_SETLKW;
    else if (how == LOCK_UN) 
        flock_type = F_UNLCK;
        fcntl_cmd = F_SETLKW;
    else         ref Flock_t flock = ref heap(new Flock_t(Type:int16(flock_type),Whence:int16(0),Start:int64(0),Len:int64(0),Pid:int32(Getppid()),), out ptr<Flock_t> _addr_flock);

    var err = FcntlFlock(uintptr(fd), fcntl_cmd, _addr_flock);
    return error.As(err)!;

}

public static error Mlock(slice<byte> b) {
    error err = default!;

    var (_, _, e1) = syscall_syscall(SYS___MLOCKALL, _BPX_NONSWAP, 0, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return ;

}

public static error Mlock2(slice<byte> b, nint flags) {
    error err = default!;

    var (_, _, e1) = syscall_syscall(SYS___MLOCKALL, _BPX_NONSWAP, 0, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return ;

}

public static error Mlockall(nint flags) {
    error err = default!;

    var (_, _, e1) = syscall_syscall(SYS___MLOCKALL, _BPX_NONSWAP, 0, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return ;

}

public static error Munlock(slice<byte> b) {
    error err = default!;

    var (_, _, e1) = syscall_syscall(SYS___MLOCKALL, _BPX_SWAP, 0, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return ;

}

public static error Munlockall() {
    error err = default!;

    var (_, _, e1) = syscall_syscall(SYS___MLOCKALL, _BPX_SWAP, 0, 0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return ;

}

public static error ClockGettime(int clockid, ptr<Timespec> _addr_ts) {
    ref Timespec ts = ref _addr_ts.val;

    uint ticks_per_sec = 100; //TODO(kenan): value is currently hardcoded; need sysconf() call otherwise
    long nsec_per_sec = 1000000000;

    if (ts == null) {
        return error.As(EFAULT)!;
    }
    if (clockid == CLOCK_REALTIME || clockid == CLOCK_MONOTONIC) {
        long nanotime = runtime.Nanotime1();
        ts.Sec = nanotime / nsec_per_sec;
        ts.Nsec = nanotime % nsec_per_sec;
    }
    else if (clockid == CLOCK_PROCESS_CPUTIME_ID || clockid == CLOCK_THREAD_CPUTIME_ID) {
        ref Tms tm = ref heap(out ptr<Tms> _addr_tm);
        var (_, err) = Times(_addr_tm);
        if (err != null) {
            return error.As(EFAULT)!;
        }
        ts.Sec = int64(tm.Utime / ticks_per_sec);
        ts.Nsec = int64(tm.Utime) * nsec_per_sec / int64(ticks_per_sec);

    }
    else
 {
        return error.As(EINVAL)!;
    }
    return error.As(null!)!;

}

public static error Statfs(@string path, ptr<Statfs_t> _addr_stat) => func((defer, _, _) => {
    error err = default!;
    ref Statfs_t stat = ref _addr_stat.val;

    var (fd, err) = open(path, O_RDONLY, 0);
    defer(Close(fd));
    if (err != null) {
        return error.As(err)!;
    }
    return error.As(Fstatfs(fd, stat))!;

});

public static nint Stdin = 0;public static nint Stdout = 1;public static nint Stderr = 2;

// Do the interface allocations only once for common
// Errno values.
private static error errEAGAIN = error.As(syscall.EAGAIN)!;private static error errEINVAL = error.As(syscall.EINVAL)!;private static error errENOENT = error.As(syscall.ENOENT)!;

private static sync.Once signalNameMapOnce = default;private static map<@string, syscall.Signal> signalNameMap = default;

// errnoErr returns common boxed Errno values, to prevent
// allocations at runtime.
private static error errnoErr(Errno e) {

    if (e == 0) 
        return error.As(null!)!;
    else if (e == EAGAIN) 
        return error.As(errEAGAIN)!;
    else if (e == EINVAL) 
        return error.As(errEINVAL)!;
    else if (e == ENOENT) 
        return error.As(errENOENT)!;
        return error.As(e)!;

}

// ErrnoName returns the error name for error number e.
public static @string ErrnoName(Errno e) {
    var i = sort.Search(len(errorList), i => {
        return errorList[i].num >= e;
    });
    if (i < len(errorList) && errorList[i].num == e) {
        return errorList[i].name;
    }
    return "";

}

// SignalName returns the signal name for signal number s.
public static @string SignalName(syscall.Signal s) {
    var i = sort.Search(len(signalList), i => {
        return signalList[i].num >= s;
    });
    if (i < len(signalList) && signalList[i].num == s) {
        return signalList[i].name;
    }
    return "";

}

// SignalNum returns the syscall.Signal for signal named s,
// or 0 if a signal with such name is not found.
// The signal name should start with "SIG".
public static syscall.Signal SignalNum(@string s) {
    signalNameMapOnce.Do(() => {
        signalNameMap = make_map<@string, syscall.Signal>(len(signalList));
        foreach (var (_, signal) in signalList) {
            signalNameMap[signal.name] = signal.num;
        }
    });
    return signalNameMap[s];

}

// clen returns the index of the first NULL byte in n or len(n) if n contains no NULL byte.
private static nint clen(slice<byte> n) {
    var i = bytes.IndexByte(n, 0);
    if (i == -1) {
        i = len(n);
    }
    return i;

}

// Mmap manager, for use by operating system-specific implementations.

private partial struct mmapper {
    public ref sync.Mutex Mutex => ref Mutex_val;
    public map<ptr<byte>, slice<byte>> active; // active mappings; key is last byte in mapping
    public Func<System.UIntPtr, System.UIntPtr, nint, nint, nint, long, (System.UIntPtr, error)> mmap;
    public Func<System.UIntPtr, System.UIntPtr, error> munmap;
}

private static (slice<byte>, error) Mmap(this ptr<mmapper> _addr_m, nint fd, long offset, nint length, nint prot, nint flags) => func((defer, _, _) => {
    slice<byte> data = default;
    error err = default!;
    ref mmapper m = ref _addr_m.val;

    if (length <= 0) {
        return (null, error.As(EINVAL)!);
    }
    var (addr, errno) = m.mmap(0, uintptr(length), prot, flags, fd, offset);
    if (errno != null) {
        return (null, error.As(errno)!);
    }
    ref struct{addruintptrlenintcapint} sl = ref heap(/* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{addruintptrlenintcapint}{addr,length,length}, out ptr<struct{addruintptrlenintcapint}> _addr_sl); 

    // Use unsafe to turn sl into a []byte.
    ptr<ptr<slice<byte>>> b = new ptr<ptr<ptr<slice<byte>>>>(@unsafe.Pointer(_addr_sl)); 

    // Register mapping in m and return it.
    var p = _addr_b[cap(b) - 1];
    m.Lock();
    defer(m.Unlock());
    m.active[p] = b;
    return (b, error.As(null!)!);

});

private static error Munmap(this ptr<mmapper> _addr_m, slice<byte> data) => func((defer, _, _) => {
    error err = default!;
    ref mmapper m = ref _addr_m.val;

    if (len(data) == 0 || len(data) != cap(data)) {
        return error.As(EINVAL)!;
    }
    var p = _addr_data[cap(data) - 1];
    m.Lock();
    defer(m.Unlock());
    var b = m.active[p];
    if (b == null || _addr_b[0] != _addr_data[0]) {
        return error.As(EINVAL)!;
    }
    {
        var errno = m.munmap(uintptr(@unsafe.Pointer(_addr_b[0])), uintptr(len(b)));

        if (errno != null) {
            return error.As(errno)!;
        }
    }

    delete(m.active, p);
    return error.As(null!)!;

});

public static (nint, error) Read(nint fd, slice<byte> p) {
    nint n = default;
    error err = default!;

    n, err = read(fd, p);
    if (raceenabled) {
        if (n > 0) {
            raceWriteRange(@unsafe.Pointer(_addr_p[0]), n);
        }
        if (err == null) {
            raceAcquire(@unsafe.Pointer(_addr_ioSync));
        }
    }
    return ;

}

public static (nint, error) Write(nint fd, slice<byte> p) {
    nint n = default;
    error err = default!;

    if (raceenabled) {
        raceReleaseMerge(@unsafe.Pointer(_addr_ioSync));
    }
    n, err = write(fd, p);
    if (raceenabled && n > 0) {
        raceReadRange(@unsafe.Pointer(_addr_p[0]), n);
    }
    return ;

}

// For testing: clients can set this flag to force
// creation of IPv6 sockets to return EAFNOSUPPORT.
public static bool SocketDisableIPv6 = default;

// Sockaddr represents a socket address.
public partial interface Sockaddr {
    (unsafe.Pointer, _Socklen, error) sockaddr(); // lowercase; only we can define Sockaddrs
}

// SockaddrInet4 implements the Sockaddr interface for AF_INET type sockets.
public partial struct SockaddrInet4 {
    public nint Port;
    public array<byte> Addr;
    public RawSockaddrInet4 raw;
}

// SockaddrInet6 implements the Sockaddr interface for AF_INET6 type sockets.
public partial struct SockaddrInet6 {
    public nint Port;
    public uint ZoneId;
    public array<byte> Addr;
    public RawSockaddrInet6 raw;
}

// SockaddrUnix implements the Sockaddr interface for AF_UNIX type sockets.
public partial struct SockaddrUnix {
    public @string Name;
    public RawSockaddrUnix raw;
}

public static error Bind(nint fd, Sockaddr sa) {
    error err = default!;

    var (ptr, n, err) = sa.sockaddr();
    if (err != null) {
        return error.As(err)!;
    }
    return error.As(bind(fd, ptr, n))!;

}

public static error Connect(nint fd, Sockaddr sa) {
    error err = default!;

    var (ptr, n, err) = sa.sockaddr();
    if (err != null) {
        return error.As(err)!;
    }
    return error.As(connect(fd, ptr, n))!;

}

public static (Sockaddr, error) Getpeername(nint fd) {
    Sockaddr sa = default;
    error err = default!;

    ref RawSockaddrAny rsa = ref heap(out ptr<RawSockaddrAny> _addr_rsa);
    ref _Socklen len = ref heap(SizeofSockaddrAny, out ptr<_Socklen> _addr_len);
    err = getpeername(fd, _addr_rsa, _addr_len);

    if (err != null) {
        return ;
    }
    return anyToSockaddr(fd, _addr_rsa);

}

public static (byte, error) GetsockoptByte(nint fd, nint level, nint opt) {
    byte value = default;
    error err = default!;

    ref byte n = ref heap(out ptr<byte> _addr_n);
    ref var vallen = ref heap(_Socklen(1), out ptr<var> _addr_vallen);
    err = getsockopt(fd, level, opt, @unsafe.Pointer(_addr_n), _addr_vallen);
    return (n, error.As(err)!);
}

public static (nint, error) GetsockoptInt(nint fd, nint level, nint opt) {
    nint value = default;
    error err = default!;

    ref int n = ref heap(out ptr<int> _addr_n);
    ref var vallen = ref heap(_Socklen(4), out ptr<var> _addr_vallen);
    err = getsockopt(fd, level, opt, @unsafe.Pointer(_addr_n), _addr_vallen);
    return (int(n), error.As(err)!);
}

public static (array<byte>, error) GetsockoptInet4Addr(nint fd, nint level, nint opt) {
    array<byte> value = default;
    error err = default!;

    ref var vallen = ref heap(_Socklen(4), out ptr<var> _addr_vallen);
    err = getsockopt(fd, level, opt, @unsafe.Pointer(_addr_value[0]), _addr_vallen);
    return (value, error.As(err)!);
}

public static (ptr<IPMreq>, error) GetsockoptIPMreq(nint fd, nint level, nint opt) {
    ptr<IPMreq> _p0 = default!;
    error _p0 = default!;

    ref IPMreq value = ref heap(out ptr<IPMreq> _addr_value);
    ref var vallen = ref heap(_Socklen(SizeofIPMreq), out ptr<var> _addr_vallen);
    var err = getsockopt(fd, level, opt, @unsafe.Pointer(_addr_value), _addr_vallen);
    return (_addr__addr_value!, error.As(err)!);
}

public static (ptr<IPv6Mreq>, error) GetsockoptIPv6Mreq(nint fd, nint level, nint opt) {
    ptr<IPv6Mreq> _p0 = default!;
    error _p0 = default!;

    ref IPv6Mreq value = ref heap(out ptr<IPv6Mreq> _addr_value);
    ref var vallen = ref heap(_Socklen(SizeofIPv6Mreq), out ptr<var> _addr_vallen);
    var err = getsockopt(fd, level, opt, @unsafe.Pointer(_addr_value), _addr_vallen);
    return (_addr__addr_value!, error.As(err)!);
}

public static (ptr<IPv6MTUInfo>, error) GetsockoptIPv6MTUInfo(nint fd, nint level, nint opt) {
    ptr<IPv6MTUInfo> _p0 = default!;
    error _p0 = default!;

    ref IPv6MTUInfo value = ref heap(out ptr<IPv6MTUInfo> _addr_value);
    ref var vallen = ref heap(_Socklen(SizeofIPv6MTUInfo), out ptr<var> _addr_vallen);
    var err = getsockopt(fd, level, opt, @unsafe.Pointer(_addr_value), _addr_vallen);
    return (_addr__addr_value!, error.As(err)!);
}

public static (ptr<ICMPv6Filter>, error) GetsockoptICMPv6Filter(nint fd, nint level, nint opt) {
    ptr<ICMPv6Filter> _p0 = default!;
    error _p0 = default!;

    ref ICMPv6Filter value = ref heap(out ptr<ICMPv6Filter> _addr_value);
    ref var vallen = ref heap(_Socklen(SizeofICMPv6Filter), out ptr<var> _addr_vallen);
    var err = getsockopt(fd, level, opt, @unsafe.Pointer(_addr_value), _addr_vallen);
    return (_addr__addr_value!, error.As(err)!);
}

public static (ptr<Linger>, error) GetsockoptLinger(nint fd, nint level, nint opt) {
    ptr<Linger> _p0 = default!;
    error _p0 = default!;

    ref Linger linger = ref heap(out ptr<Linger> _addr_linger);
    ref var vallen = ref heap(_Socklen(SizeofLinger), out ptr<var> _addr_vallen);
    var err = getsockopt(fd, level, opt, @unsafe.Pointer(_addr_linger), _addr_vallen);
    return (_addr__addr_linger!, error.As(err)!);
}

public static (ptr<Timeval>, error) GetsockoptTimeval(nint fd, nint level, nint opt) {
    ptr<Timeval> _p0 = default!;
    error _p0 = default!;

    ref Timeval tv = ref heap(out ptr<Timeval> _addr_tv);
    ref var vallen = ref heap(_Socklen(@unsafe.Sizeof(tv)), out ptr<var> _addr_vallen);
    var err = getsockopt(fd, level, opt, @unsafe.Pointer(_addr_tv), _addr_vallen);
    return (_addr__addr_tv!, error.As(err)!);
}

public static (ulong, error) GetsockoptUint64(nint fd, nint level, nint opt) {
    ulong value = default;
    error err = default!;

    ref ulong n = ref heap(out ptr<ulong> _addr_n);
    ref var vallen = ref heap(_Socklen(8), out ptr<var> _addr_vallen);
    err = getsockopt(fd, level, opt, @unsafe.Pointer(_addr_n), _addr_vallen);
    return (n, error.As(err)!);
}

public static (nint, Sockaddr, error) Recvfrom(nint fd, slice<byte> p, nint flags) {
    nint n = default;
    Sockaddr from = default;
    error err = default!;

    ref RawSockaddrAny rsa = ref heap(out ptr<RawSockaddrAny> _addr_rsa);
    ref _Socklen len = ref heap(SizeofSockaddrAny, out ptr<_Socklen> _addr_len);
    n, err = recvfrom(fd, p, flags, _addr_rsa, _addr_len);

    if (err != null) {
        return ;
    }
    if (rsa.Addr.Family != AF_UNSPEC) {
        from, err = anyToSockaddr(fd, _addr_rsa);
    }
    return ;

}

public static error Sendto(nint fd, slice<byte> p, nint flags, Sockaddr to) {
    error err = default!;

    var (ptr, n, err) = to.sockaddr();
    if (err != null) {
        return error.As(err)!;
    }
    return error.As(sendto(fd, p, flags, ptr, n))!;

}

public static error SetsockoptByte(nint fd, nint level, nint opt, byte value) {
    error err = default!;

    return error.As(setsockopt(fd, level, opt, @unsafe.Pointer(_addr_value), 1))!;
}

public static error SetsockoptInt(nint fd, nint level, nint opt, nint value) {
    error err = default!;

    ref var n = ref heap(int32(value), out ptr<var> _addr_n);
    return error.As(setsockopt(fd, level, opt, @unsafe.Pointer(_addr_n), 4))!;
}

public static error SetsockoptInet4Addr(nint fd, nint level, nint opt, array<byte> value) {
    error err = default!;
    value = value.Clone();

    return error.As(setsockopt(fd, level, opt, @unsafe.Pointer(_addr_value[0]), 4))!;
}

public static error SetsockoptIPMreq(nint fd, nint level, nint opt, ptr<IPMreq> _addr_mreq) {
    error err = default!;
    ref IPMreq mreq = ref _addr_mreq.val;

    return error.As(setsockopt(fd, level, opt, @unsafe.Pointer(mreq), SizeofIPMreq))!;
}

public static error SetsockoptIPv6Mreq(nint fd, nint level, nint opt, ptr<IPv6Mreq> _addr_mreq) {
    error err = default!;
    ref IPv6Mreq mreq = ref _addr_mreq.val;

    return error.As(setsockopt(fd, level, opt, @unsafe.Pointer(mreq), SizeofIPv6Mreq))!;
}

public static error SetsockoptICMPv6Filter(nint fd, nint level, nint opt, ptr<ICMPv6Filter> _addr_filter) {
    ref ICMPv6Filter filter = ref _addr_filter.val;

    return error.As(setsockopt(fd, level, opt, @unsafe.Pointer(filter), SizeofICMPv6Filter))!;
}

public static error SetsockoptLinger(nint fd, nint level, nint opt, ptr<Linger> _addr_l) {
    error err = default!;
    ref Linger l = ref _addr_l.val;

    return error.As(setsockopt(fd, level, opt, @unsafe.Pointer(l), SizeofLinger))!;
}

public static error SetsockoptString(nint fd, nint level, nint opt, @string s) {
    error err = default!;

    unsafe.Pointer p = default;
    if (len(s) > 0) {
        p = @unsafe.Pointer(_addr_(slice<byte>)s[0]);
    }
    return error.As(setsockopt(fd, level, opt, p, uintptr(len(s))))!;

}

public static error SetsockoptTimeval(nint fd, nint level, nint opt, ptr<Timeval> _addr_tv) {
    error err = default!;
    ref Timeval tv = ref _addr_tv.val;

    return error.As(setsockopt(fd, level, opt, @unsafe.Pointer(tv), @unsafe.Sizeof(tv)))!;
}

public static error SetsockoptUint64(nint fd, nint level, nint opt, ulong value) {
    error err = default!;

    return error.As(setsockopt(fd, level, opt, @unsafe.Pointer(_addr_value), 8))!;
}

public static (nint, error) Socket(nint domain, nint typ, nint proto) {
    nint fd = default;
    error err = default!;

    if (domain == AF_INET6 && SocketDisableIPv6) {
        return (-1, error.As(EAFNOSUPPORT)!);
    }
    fd, err = socket(domain, typ, proto);
    return ;

}

public static (array<nint>, error) Socketpair(nint domain, nint typ, nint proto) {
    array<nint> fd = default;
    error err = default!;

    ref array<int> fdx = ref heap(new array<int>(2), out ptr<array<int>> _addr_fdx);
    err = socketpair(domain, typ, proto, _addr_fdx);
    if (err == null) {
        fd[0] = int(fdx[0]);
        fd[1] = int(fdx[1]);
    }
    return ;

}

private static long ioSync = default;

public static void CloseOnExec(nint fd) {
    fcntl(fd, F_SETFD, FD_CLOEXEC);
}

public static error SetNonblock(nint fd, bool nonblocking) {
    error err = default!;

    var (flag, err) = fcntl(fd, F_GETFL, 0);
    if (err != null) {
        return error.As(err)!;
    }
    if (nonblocking) {
        flag |= O_NONBLOCK;
    }
    else
 {
        flag &= ~O_NONBLOCK;
    }
    _, err = fcntl(fd, F_SETFL, flag);
    return error.As(err)!;

}

// Exec calls execve(2), which replaces the calling executable in the process
// tree. argv0 should be the full path to an executable ("/bin/ls") and the
// executable name should also be the first argument in argv (["ls", "-l"]).
// envv are the environment variables that should be passed to the new
// process (["USER=go", "PWD=/tmp"]).
public static error Exec(@string argv0, slice<@string> argv, slice<@string> envv) {
    return error.As(syscall.Exec(argv0, argv, envv))!;
}

public static error Mount(@string source, @string target, @string fstype, System.UIntPtr flags, @string data) {
    error err = default!;

    {
        nint needspace = 8 - len(fstype);

        if (needspace <= 0) {
            fstype = fstype[..(int)8];
        }
        else
 {
            fstype += "        "[..(int)needspace];
        }
    }

    return error.As(mount_LE(target, source, fstype, uint32(flags), int32(len(data)), data))!;

}

public static error Unmount(@string name, nint mtm) {
    error err = default!;
 
    // mountpoint is always a full path and starts with a '/'
    // check if input string is not a mountpoint but a filesystem name
    if (name[0] != '/') {
        return error.As(unmount(name, mtm))!;
    }
    Func<slice<byte>, @string> b2s = arr => {
        var nulli = bytes.IndexByte(arr, 0);
        if (nulli == -1) {
            return error.As(string(arr))!;
        }
        else
 {
            return error.As(string(arr[..(int)nulli]))!;
        }
    };
    ref var buffer = ref heap(out ptr<var> _addr_buffer);
    var (fsCount, err) = W_Getmntent_A((byte.val)(@unsafe.Pointer(_addr_buffer)), int(@unsafe.Sizeof(buffer)));
    if (err != null) {
        return error.As(err)!;
    }
    if (fsCount == 0) {
        return error.As(EINVAL)!;
    }
    for (nint i = 0; i < fsCount; i++) {
        if (b2s(buffer.fsinfo[i].Mountpoint[..]) == name) {
            err = unmount(b2s(buffer.fsinfo[i].Fsname[..]), mtm);
            break;
        }
    }
    return error.As(err)!;

}

} // end unix_package
