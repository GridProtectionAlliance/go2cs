// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Linux system calls.
// This file is compiled as ordinary Go code,
// but it is also input to mksyscall,
// which parses the //sys lines and generates system call stubs.
// Note that sometimes we use a lowercase //sys name and
// wrap it in our own nicer implementation.

// package syscall -- go2cs converted at 2022 March 06 22:27:02 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Program Files\Go\src\syscall\syscall_linux.go
using itoa = go.@internal.itoa_package;
using @unsafe = go.@unsafe_package;
using System;


namespace go;

public static partial class syscall_package {

private static (System.UIntPtr, System.UIntPtr) rawSyscallNoError(System.UIntPtr trap, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3);

/*
 * Wrapped
 */

public static error Access(@string path, uint mode) {
    error err = default!;

    return error.As(Faccessat(_AT_FDCWD, path, mode, 0))!;
}

public static error Chmod(@string path, uint mode) {
    error err = default!;

    return error.As(Fchmodat(_AT_FDCWD, path, mode, 0))!;
}

public static error Chown(@string path, nint uid, nint gid) {
    error err = default!;

    return error.As(Fchownat(_AT_FDCWD, path, uid, gid, 0))!;
}

public static (nint, error) Creat(@string path, uint mode) {
    nint fd = default;
    error err = default!;

    return Open(path, O_CREAT | O_WRONLY | O_TRUNC, mode);
}

private static bool isGroupMember(nint gid) {
    var (groups, err) = Getgroups();
    if (err != null) {>>MARKER:FUNCTION_rawSyscallNoError_BLOCK_PREFIX<<
        return false;
    }
    foreach (var (_, g) in groups) {
        if (g == gid) {
            return true;
        }
    }    return false;

}

//sys    faccessat(dirfd int, path string, mode uint32) (err error)

public static error Faccessat(nint dirfd, @string path, uint mode, nint flags) {
    error err = default!;

    if (flags & ~(_AT_SYMLINK_NOFOLLOW | _AT_EACCESS) != 0) {
        return error.As(EINVAL)!;
    }
    if (flags == 0) {
        return error.As(faccessat(dirfd, path, mode))!;
    }
    ref Stat_t st = ref heap(out ptr<Stat_t> _addr_st);
    {
        var err = fstatat(dirfd, path, _addr_st, flags & _AT_SYMLINK_NOFOLLOW);

        if (err != null) {
            return error.As(err)!;
        }
    }


    mode &= 7;
    if (mode == 0) {
        return error.As(null!)!;
    }
    nint uid = default;
    if (flags & _AT_EACCESS != 0) {
        uid = Geteuid();
    }
    else
 {
        uid = Getuid();
    }
    if (uid == 0) {
        if (mode & 1 == 0) { 
            // Root can read and write any file.
            return error.As(null!)!;

        }
        if (st.Mode & 0111 != 0) { 
            // Root can execute any file that anybody can execute.
            return error.As(null!)!;

        }
        return error.As(EACCES)!;

    }
    uint fmode = default;
    if (uint32(uid) == st.Uid) {
        fmode = (st.Mode >> 6) & 7;
    }
    else
 {
        nint gid = default;
        if (flags & _AT_EACCESS != 0) {
            gid = Getegid();
        }
        else
 {
            gid = Getgid();
        }
        if (uint32(gid) == st.Gid || isGroupMember(gid)) {
            fmode = (st.Mode >> 3) & 7;
        }
        else
 {
            fmode = st.Mode & 7;
        }
    }
    if (fmode & mode == mode) {
        return error.As(null!)!;
    }
    return error.As(EACCES)!;

}

//sys    fchmodat(dirfd int, path string, mode uint32) (err error)

public static error Fchmodat(nint dirfd, @string path, uint mode, nint flags) {
    error err = default!;
 
    // Linux fchmodat doesn't support the flags parameter. Mimick glibc's behavior
    // and check the flags. Otherwise the mode would be applied to the symlink
    // destination which is not what the user expects.
    if (flags & ~_AT_SYMLINK_NOFOLLOW != 0) {
        return error.As(EINVAL)!;
    }
    else if (flags & _AT_SYMLINK_NOFOLLOW != 0) {
        return error.As(EOPNOTSUPP)!;
    }
    return error.As(fchmodat(dirfd, path, mode))!;

}

//sys    linkat(olddirfd int, oldpath string, newdirfd int, newpath string, flags int) (err error)

public static error Link(@string oldpath, @string newpath) {
    error err = default!;

    return error.As(linkat(_AT_FDCWD, oldpath, _AT_FDCWD, newpath, 0))!;
}

public static error Mkdir(@string path, uint mode) {
    error err = default!;

    return error.As(Mkdirat(_AT_FDCWD, path, mode))!;
}

public static error Mknod(@string path, uint mode, nint dev) {
    error err = default!;

    return error.As(Mknodat(_AT_FDCWD, path, mode, dev))!;
}

public static (nint, error) Open(@string path, nint mode, uint perm) {
    nint fd = default;
    error err = default!;

    return openat(_AT_FDCWD, path, mode | O_LARGEFILE, perm);
}

//sys    openat(dirfd int, path string, flags int, mode uint32) (fd int, err error)

public static (nint, error) Openat(nint dirfd, @string path, nint flags, uint mode) {
    nint fd = default;
    error err = default!;

    return openat(dirfd, path, flags | O_LARGEFILE, mode);
}

//sys    readlinkat(dirfd int, path string, buf []byte) (n int, err error)

public static (nint, error) Readlink(@string path, slice<byte> buf) {
    nint n = default;
    error err = default!;

    return readlinkat(_AT_FDCWD, path, buf);
}

public static error Rename(@string oldpath, @string newpath) {
    error err = default!;

    return error.As(Renameat(_AT_FDCWD, oldpath, _AT_FDCWD, newpath))!;
}

public static error Rmdir(@string path) {
    return error.As(unlinkat(_AT_FDCWD, path, _AT_REMOVEDIR))!;
}

//sys    symlinkat(oldpath string, newdirfd int, newpath string) (err error)

public static error Symlink(@string oldpath, @string newpath) {
    error err = default!;

    return error.As(symlinkat(oldpath, _AT_FDCWD, newpath))!;
}

public static error Unlink(@string path) {
    return error.As(unlinkat(_AT_FDCWD, path, 0))!;
}

//sys    unlinkat(dirfd int, path string, flags int) (err error)

public static error Unlinkat(nint dirfd, @string path) {
    return error.As(unlinkat(dirfd, path, 0))!;
}

public static error Utimes(@string path, slice<Timeval> tv) {
    error err = default!;

    if (len(tv) != 2) {
        return error.As(EINVAL)!;
    }
    return error.As(utimes(path, new ptr<ptr<array<Timeval>>>(@unsafe.Pointer(_addr_tv[0]))))!;

}

//sys    utimensat(dirfd int, path string, times *[2]Timespec, flag int) (err error)

public static error UtimesNano(@string path, slice<Timespec> ts) {
    error err = default!;

    if (len(ts) != 2) {
        return error.As(EINVAL)!;
    }
    err = utimensat(_AT_FDCWD, path, new ptr<ptr<array<Timespec>>>(@unsafe.Pointer(_addr_ts[0])), 0);
    if (err != ENOSYS) {
        return error.As(err)!;
    }
    array<Timeval> tv = new array<Timeval>(2);
    for (nint i = 0; i < 2; i++) {
        tv[i].Sec = ts[i].Sec;
        tv[i].Usec = ts[i].Nsec / 1000;
    }
    return error.As(utimes(path, new ptr<ptr<array<Timeval>>>(@unsafe.Pointer(_addr_tv[0]))))!;

}

public static error Futimesat(nint dirfd, @string path, slice<Timeval> tv) {
    error err = default!;

    if (len(tv) != 2) {
        return error.As(EINVAL)!;
    }
    return error.As(futimesat(dirfd, path, new ptr<ptr<array<Timeval>>>(@unsafe.Pointer(_addr_tv[0]))))!;

}

public static error Futimes(nint fd, slice<Timeval> tv) {
    error err = default!;
 
    // Believe it or not, this is the best we can do on Linux
    // (and is what glibc does).
    return error.As(Utimes("/proc/self/fd/" + itoa.Itoa(fd), tv))!;

}

public static readonly var ImplementsGetwd = true;

//sys    Getcwd(buf []byte) (n int, err error)



//sys    Getcwd(buf []byte) (n int, err error)

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

private static unsafe.Pointer cgo_libc_setgroups = default; // non-nil if cgo linked.

public static error Setgroups(slice<nint> gids) {
    error err = default!;

    var n = uintptr(len(gids));
    if (n == 0) {
        if (cgo_libc_setgroups == null) {
            {
                var (_, _, e1) = AllThreadsSyscall(_SYS_setgroups, 0, 0, 0);

                if (e1 != 0) {
                    err = errnoErr(e1);
                }

            }

            return ;

        }
        {
            var ret__prev2 = ret;

            var ret = cgocaller(cgo_libc_setgroups, 0, 0);

            if (ret != 0) {
                err = errnoErr(Errno(ret));
            }

            ret = ret__prev2;

        }

        return ;

    }
    var a = make_slice<_Gid_t>(len(gids));
    foreach (var (i, v) in gids) {
        a[i] = _Gid_t(v);
    }    if (cgo_libc_setgroups == null) {
        {
            (_, _, e1) = AllThreadsSyscall(_SYS_setgroups, n, uintptr(@unsafe.Pointer(_addr_a[0])), 0);

            if (e1 != 0) {
                err = errnoErr(e1);
            }

        }

        return ;

    }
    {
        var ret__prev1 = ret;

        ret = cgocaller(cgo_libc_setgroups, n, uintptr(@unsafe.Pointer(_addr_a[0])));

        if (ret != 0) {
            err = errnoErr(Errno(ret));
        }
        ret = ret__prev1;

    }

    return ;

}

public partial struct WaitStatus { // : uint
}

// Wait status is 7 bits at bottom, either 0 (exited),
// 0x7F (stopped), or a signal number that caused an exit.
// The 0x80 bit is whether there was a core dump.
// An extra number (exit code, signal causing a stop)
// is in the high bits. At least that's the idea.
// There are various irregularities. For example, the
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
    if (!w.Exited()) {
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
    if (w.StopSignal() != SIGTRAP) {
        return -1;
    }
    return int(w >> (int)(shift)) >> 8;

}

//sys    wait4(pid int, wstatus *_C_int, options int, rusage *Rusage) (wpid int, err error)

public static (nint, error) Wait4(nint pid, ptr<WaitStatus> _addr_wstatus, nint options, ptr<Rusage> _addr_rusage) {
    nint wpid = default;
    error err = default!;
    ref WaitStatus wstatus = ref _addr_wstatus.val;
    ref Rusage rusage = ref _addr_rusage.val;

    ref _C_int status = ref heap(out ptr<_C_int> _addr_status);
    wpid, err = wait4(pid, _addr_status, options, rusage);
    if (wstatus != null) {
        wstatus = WaitStatus(status);
    }
    return ;

}

public static error Mkfifo(@string path, uint mode) {
    error err = default!;

    return error.As(Mknod(path, mode | S_IFIFO, 0))!;
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
        sa.raw.Path[i] = int8(name[i]);
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

public partial struct SockaddrLinklayer {
    public ushort Protocol;
    public nint Ifindex;
    public ushort Hatype;
    public byte Pkttype;
    public byte Halen;
    public array<byte> Addr;
    public RawSockaddrLinklayer raw;
}

private static (unsafe.Pointer, _Socklen, error) sockaddr(this ptr<SockaddrLinklayer> _addr_sa) {
    unsafe.Pointer _p0 = default;
    _Socklen _p0 = default;
    error _p0 = default!;
    ref SockaddrLinklayer sa = ref _addr_sa.val;

    if (sa.Ifindex < 0 || sa.Ifindex > 0x7fffffff) {
        return (null, 0, error.As(EINVAL)!);
    }
    sa.raw.Family = AF_PACKET;
    sa.raw.Protocol = sa.Protocol;
    sa.raw.Ifindex = int32(sa.Ifindex);
    sa.raw.Hatype = sa.Hatype;
    sa.raw.Pkttype = sa.Pkttype;
    sa.raw.Halen = sa.Halen;
    for (nint i = 0; i < len(sa.Addr); i++) {
        sa.raw.Addr[i] = sa.Addr[i];
    }
    return (@unsafe.Pointer(_addr_sa.raw), SizeofSockaddrLinklayer, error.As(null!)!);

}

public partial struct SockaddrNetlink {
    public ushort Family;
    public ushort Pad;
    public uint Pid;
    public uint Groups;
    public RawSockaddrNetlink raw;
}

private static (unsafe.Pointer, _Socklen, error) sockaddr(this ptr<SockaddrNetlink> _addr_sa) {
    unsafe.Pointer _p0 = default;
    _Socklen _p0 = default;
    error _p0 = default!;
    ref SockaddrNetlink sa = ref _addr_sa.val;

    sa.raw.Family = AF_NETLINK;
    sa.raw.Pad = sa.Pad;
    sa.raw.Pid = sa.Pid;
    sa.raw.Groups = sa.Groups;
    return (@unsafe.Pointer(_addr_sa.raw), SizeofSockaddrNetlink, error.As(null!)!);
}

private static (Sockaddr, error) anyToSockaddr(ptr<RawSockaddrAny> _addr_rsa) {
    Sockaddr _p0 = default;
    error _p0 = default!;
    ref RawSockaddrAny rsa = ref _addr_rsa.val;


    if (rsa.Addr.Family == AF_NETLINK) 
        var pp = (RawSockaddrNetlink.val)(@unsafe.Pointer(rsa));
        ptr<SockaddrNetlink> sa = @new<SockaddrNetlink>();
        sa.Family = pp.Family;
        sa.Pad = pp.Pad;
        sa.Pid = pp.Pid;
        sa.Groups = pp.Groups;
        return (sa, error.As(null!)!);
    else if (rsa.Addr.Family == AF_PACKET) 
        pp = (RawSockaddrLinklayer.val)(@unsafe.Pointer(rsa));
        sa = @new<SockaddrLinklayer>();
        sa.Protocol = pp.Protocol;
        sa.Ifindex = int(pp.Ifindex);
        sa.Hatype = pp.Hatype;
        sa.Pkttype = pp.Pkttype;
        sa.Halen = pp.Halen;
        {
            nint i__prev1 = i;

            for (nint i = 0; i < len(sa.Addr); i++) {
                sa.Addr[i] = pp.Addr[i];
            }


            i = i__prev1;
        }
        return (sa, error.As(null!)!);
    else if (rsa.Addr.Family == AF_UNIX) 
        pp = (RawSockaddrUnix.val)(@unsafe.Pointer(rsa));
        sa = @new<SockaddrUnix>();
        if (pp.Path[0] == 0) { 
            // "Abstract" Unix domain socket.
            // Rewrite leading NUL as @ for textual display.
            // (This is the standard convention.)
            // Not friendly to overwrite in place,
            // but the callers below don't care.
            pp.Path[0] = '@';

        }
        nint n = 0;
        while (n < len(pp.Path) && pp.Path[n] != 0) {
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

public static (nint, Sockaddr, error) Accept(nint fd) {
    nint nfd = default;
    Sockaddr sa = default;
    error err = default!;

    ref RawSockaddrAny rsa = ref heap(out ptr<RawSockaddrAny> _addr_rsa);
    ref _Socklen len = ref heap(SizeofSockaddrAny, out ptr<_Socklen> _addr_len); 
    // Try accept4 first for Android, then try accept for kernel older than 2.6.28
    nfd, err = accept4(fd, _addr_rsa, _addr_len, 0);
    if (err == ENOSYS) {
        nfd, err = accept(fd, _addr_rsa, _addr_len);
    }
    if (err != null) {
        return ;
    }
    sa, err = anyToSockaddr(_addr_rsa);
    if (err != null) {
        Close(nfd);
        nfd = 0;
    }
    return ;

}

public static (nint, Sockaddr, error) Accept4(nint fd, nint flags) => func((_, panic, _) => {
    nint nfd = default;
    Sockaddr sa = default;
    error err = default!;

    ref RawSockaddrAny rsa = ref heap(out ptr<RawSockaddrAny> _addr_rsa);
    ref _Socklen len = ref heap(SizeofSockaddrAny, out ptr<_Socklen> _addr_len);
    nfd, err = accept4(fd, _addr_rsa, _addr_len, flags);
    if (err != null) {
        return ;
    }
    if (len > SizeofSockaddrAny) {
        panic("RawSockaddrAny too small");
    }
    sa, err = anyToSockaddr(_addr_rsa);
    if (err != null) {
        Close(nfd);
        nfd = 0;
    }
    return ;

});

public static (Sockaddr, error) Getsockname(nint fd) {
    Sockaddr sa = default;
    error err = default!;

    ref RawSockaddrAny rsa = ref heap(out ptr<RawSockaddrAny> _addr_rsa);
    ref _Socklen len = ref heap(SizeofSockaddrAny, out ptr<_Socklen> _addr_len);
    err = getsockname(fd, _addr_rsa, _addr_len);

    if (err != null) {
        return ;
    }
    return anyToSockaddr(_addr_rsa);

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

public static (ptr<IPMreqn>, error) GetsockoptIPMreqn(nint fd, nint level, nint opt) {
    ptr<IPMreqn> _p0 = default!;
    error _p0 = default!;

    ref IPMreqn value = ref heap(out ptr<IPMreqn> _addr_value);
    ref var vallen = ref heap(_Socklen(SizeofIPMreqn), out ptr<var> _addr_vallen);
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

public static (ptr<Ucred>, error) GetsockoptUcred(nint fd, nint level, nint opt) {
    ptr<Ucred> _p0 = default!;
    error _p0 = default!;

    ref Ucred value = ref heap(out ptr<Ucred> _addr_value);
    ref var vallen = ref heap(_Socklen(SizeofUcred), out ptr<var> _addr_vallen);
    var err = getsockopt(fd, level, opt, @unsafe.Pointer(_addr_value), _addr_vallen);
    return (_addr__addr_value!, error.As(err)!);
}

public static error SetsockoptIPMreqn(nint fd, nint level, nint opt, ptr<IPMreqn> _addr_mreq) {
    error err = default!;
    ref IPMreqn mreq = ref _addr_mreq.val;

    return error.As(setsockopt(fd, level, opt, @unsafe.Pointer(mreq), @unsafe.Sizeof(mreq)))!;
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
    msg.Namelen = uint32(SizeofSockaddrAny);
    ref Iovec iov = ref heap(out ptr<Iovec> _addr_iov);
    if (len(p) > 0) {
        iov.Base = _addr_p[0];
        iov.SetLen(len(p));
    }
    ref byte dummy = ref heap(out ptr<byte> _addr_dummy);
    if (len(oob) > 0) {
        if (len(p) == 0) {
            nint sockType = default;
            sockType, err = GetsockoptInt(fd, SOL_SOCKET, SO_TYPE);
            if (err != null) {
                return ;
            } 
            // receive at least one normal byte
            if (sockType != SOCK_DGRAM) {
                _addr_iov.Base = _addr_dummy;
                iov.Base = ref _addr_iov.Base.val;
                iov.SetLen(1);

            }

        }
        msg.Control = _addr_oob[0];
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
        from, err = anyToSockaddr(_addr_rsa);
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
    msg.Name = (byte.val)(ptr);
    msg.Namelen = uint32(salen);
    ref Iovec iov = ref heap(out ptr<Iovec> _addr_iov);
    if (len(p) > 0) {
        iov.Base = _addr_p[0];
        iov.SetLen(len(p));
    }
    ref byte dummy = ref heap(out ptr<byte> _addr_dummy);
    if (len(oob) > 0) {
        if (len(p) == 0) {
            nint sockType = default;
            sockType, err = GetsockoptInt(fd, SOL_SOCKET, SO_TYPE);
            if (err != null) {
                return (0, error.As(err)!);
            } 
            // send at least one normal byte
            if (sockType != SOCK_DGRAM) {
                _addr_iov.Base = _addr_dummy;
                iov.Base = ref _addr_iov.Base.val;
                iov.SetLen(1);

            }

        }
        msg.Control = _addr_oob[0];
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

// BindToDevice binds the socket associated with fd to device.
public static error BindToDevice(nint fd, @string device) {
    error err = default!;

    return error.As(SetsockoptString(fd, SOL_SOCKET, SO_BINDTODEVICE, device))!;
}

//sys    ptrace(request int, pid int, addr uintptr, data uintptr) (err error)

private static (nint, error) ptracePeek(nint req, nint pid, System.UIntPtr addr, slice<byte> @out) {
    nint count = default;
    error err = default!;
 
    // The peek requests are machine-size oriented, so we wrap it
    // to retrieve arbitrary-length data.

    // The ptrace syscall differs from glibc's ptrace.
    // Peeks returns the word in *data, not as the return value.

    array<byte> buf = new array<byte>(sizeofPtr); 

    // Leading edge. PEEKTEXT/PEEKDATA don't require aligned
    // access (PEEKUSER warns that it might), but if we don't
    // align our reads, we might straddle an unmapped page
    // boundary and not get the bytes leading up to the page
    // boundary.
    nint n = 0;
    if (addr % sizeofPtr != 0) {
        err = ptrace(req, pid, addr - addr % sizeofPtr, uintptr(@unsafe.Pointer(_addr_buf[0])));
        if (err != null) {
            return (0, error.As(err)!);
        }
        n += copy(out, buf[(int)addr % sizeofPtr..]);
        out = out[(int)n..];

    }
    while (len(out) > 0) { 
        // We use an internal buffer to guarantee alignment.
        // It's not documented if this is necessary, but we're paranoid.
        err = ptrace(req, pid, addr + uintptr(n), uintptr(@unsafe.Pointer(_addr_buf[0])));
        if (err != null) {
            return (n, error.As(err)!);
        }
        var copied = copy(out, buf[(int)0..]);
        n += copied;
        out = out[(int)copied..];

    }

    return (n, error.As(null!)!);

}

public static (nint, error) PtracePeekText(nint pid, System.UIntPtr addr, slice<byte> @out) {
    nint count = default;
    error err = default!;

    return ptracePeek(PTRACE_PEEKTEXT, pid, addr, out);
}

public static (nint, error) PtracePeekData(nint pid, System.UIntPtr addr, slice<byte> @out) {
    nint count = default;
    error err = default!;

    return ptracePeek(PTRACE_PEEKDATA, pid, addr, out);
}

private static (nint, error) ptracePoke(nint pokeReq, nint peekReq, nint pid, System.UIntPtr addr, slice<byte> data) {
    nint count = default;
    error err = default!;
 
    // As for ptracePeek, we need to align our accesses to deal
    // with the possibility of straddling an invalid page.

    // Leading edge.
    nint n = 0;
    if (addr % sizeofPtr != 0) {
        array<byte> buf = new array<byte>(sizeofPtr);
        err = ptrace(peekReq, pid, addr - addr % sizeofPtr, uintptr(@unsafe.Pointer(_addr_buf[0])));
        if (err != null) {
            return (0, error.As(err)!);
        }
        n += copy(buf[(int)addr % sizeofPtr..], data);
        var word = ((uintptr.val)(@unsafe.Pointer(_addr_buf[0]))).val;
        err = ptrace(pokeReq, pid, addr - addr % sizeofPtr, word);
        if (err != null) {
            return (0, error.As(err)!);
        }
        data = data[(int)n..];

    }
    while (len(data) > sizeofPtr) {
        word = ((uintptr.val)(@unsafe.Pointer(_addr_data[0]))).val;
        err = ptrace(pokeReq, pid, addr + uintptr(n), word);
        if (err != null) {
            return (n, error.As(err)!);
        }
        n += sizeofPtr;
        data = data[(int)sizeofPtr..];

    } 

    // Trailing edge.
    if (len(data) > 0) {
        buf = new array<byte>(sizeofPtr);
        err = ptrace(peekReq, pid, addr + uintptr(n), uintptr(@unsafe.Pointer(_addr_buf[0])));
        if (err != null) {
            return (n, error.As(err)!);
        }
        copy(buf[(int)0..], data);
        word = ((uintptr.val)(@unsafe.Pointer(_addr_buf[0]))).val;
        err = ptrace(pokeReq, pid, addr + uintptr(n), word);
        if (err != null) {
            return (n, error.As(err)!);
        }
        n += len(data);

    }
    return (n, error.As(null!)!);

}

public static (nint, error) PtracePokeText(nint pid, System.UIntPtr addr, slice<byte> data) {
    nint count = default;
    error err = default!;

    return ptracePoke(PTRACE_POKETEXT, PTRACE_PEEKTEXT, pid, addr, data);
}

public static (nint, error) PtracePokeData(nint pid, System.UIntPtr addr, slice<byte> data) {
    nint count = default;
    error err = default!;

    return ptracePoke(PTRACE_POKEDATA, PTRACE_PEEKDATA, pid, addr, data);
}

public static error PtraceGetRegs(nint pid, ptr<PtraceRegs> _addr_regsout) {
    error err = default!;
    ref PtraceRegs regsout = ref _addr_regsout.val;

    return error.As(ptrace(PTRACE_GETREGS, pid, 0, uintptr(@unsafe.Pointer(regsout))))!;
}

public static error PtraceSetRegs(nint pid, ptr<PtraceRegs> _addr_regs) {
    error err = default!;
    ref PtraceRegs regs = ref _addr_regs.val;

    return error.As(ptrace(PTRACE_SETREGS, pid, 0, uintptr(@unsafe.Pointer(regs))))!;
}

public static error PtraceSetOptions(nint pid, nint options) {
    error err = default!;

    return error.As(ptrace(PTRACE_SETOPTIONS, pid, 0, uintptr(options)))!;
}

public static (nuint, error) PtraceGetEventMsg(nint pid) {
    nuint msg = default;
    error err = default!;

    ref _C_long data = ref heap(out ptr<_C_long> _addr_data);
    err = ptrace(PTRACE_GETEVENTMSG, pid, 0, uintptr(@unsafe.Pointer(_addr_data)));
    msg = uint(data);
    return ;
}

public static error PtraceCont(nint pid, nint signal) {
    error err = default!;

    return error.As(ptrace(PTRACE_CONT, pid, 0, uintptr(signal)))!;
}

public static error PtraceSyscall(nint pid, nint signal) {
    error err = default!;

    return error.As(ptrace(PTRACE_SYSCALL, pid, 0, uintptr(signal)))!;
}

public static error PtraceSingleStep(nint pid) {
    error err = default!;

    return error.As(ptrace(PTRACE_SINGLESTEP, pid, 0, 0))!;
}

public static error PtraceAttach(nint pid) {
    error err = default!;

    return error.As(ptrace(PTRACE_ATTACH, pid, 0, 0))!;
}

public static error PtraceDetach(nint pid) {
    error err = default!;

    return error.As(ptrace(PTRACE_DETACH, pid, 0, 0))!;
}

//sys    reboot(magic1 uint, magic2 uint, cmd int, arg string) (err error)

public static error Reboot(nint cmd) {
    error err = default!;

    return error.As(reboot(LINUX_REBOOT_MAGIC1, LINUX_REBOOT_MAGIC2, cmd, ""))!;
}

public static (nint, error) ReadDirent(nint fd, slice<byte> buf) {
    nint n = default;
    error err = default!;

    return Getdents(fd, buf);
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

//sys    mount(source string, target string, fstype string, flags uintptr, data *byte) (err error)

public static error Mount(@string source, @string target, @string fstype, System.UIntPtr flags, @string data) {
    error err = default!;
 
    // Certain file systems get rather angry and EINVAL if you give
    // them an empty string of data, rather than NULL.
    if (data == "") {
        return error.As(mount(source, target, fstype, flags, null))!;
    }
    var (datap, err) = BytePtrFromString(data);
    if (err != null) {
        return error.As(err)!;
    }
    return error.As(mount(source, target, fstype, flags, datap))!;

}

// Sendto
// Recvfrom
// Socketpair

/*
 * Direct access
 */
//sys    Acct(path string) (err error)
//sys    Adjtimex(buf *Timex) (state int, err error)
//sys    Chdir(path string) (err error)
//sys    Chroot(path string) (err error)
//sys    Close(fd int) (err error)
//sys    Dup(oldfd int) (fd int, err error)
//sys    Dup3(oldfd int, newfd int, flags int) (err error)
//sysnb    EpollCreate1(flag int) (fd int, err error)
//sysnb    EpollCtl(epfd int, op int, fd int, event *EpollEvent) (err error)
//sys    Fallocate(fd int, mode uint32, off int64, len int64) (err error)
//sys    Fchdir(fd int) (err error)
//sys    Fchmod(fd int, mode uint32) (err error)
//sys    Fchownat(dirfd int, path string, uid int, gid int, flags int) (err error)
//sys    fcntl(fd int, cmd int, arg int) (val int, err error)
//sys    Fdatasync(fd int) (err error)
//sys    Flock(fd int, how int) (err error)
//sys    Fsync(fd int) (err error)
//sys    Getdents(fd int, buf []byte) (n int, err error) = SYS_GETDENTS64
//sysnb    Getpgid(pid int) (pgid int, err error)

public static nint Getpgrp() {
    nint pid = default;

    pid, _ = Getpgid(0);
    return ;
}

//sysnb    Getpid() (pid int)
//sysnb    Getppid() (ppid int)
//sys    Getpriority(which int, who int) (prio int, err error)
//sysnb    Getrusage(who int, rusage *Rusage) (err error)
//sysnb    Gettid() (tid int)
//sys    Getxattr(path string, attr string, dest []byte) (sz int, err error)
//sys    InotifyAddWatch(fd int, pathname string, mask uint32) (watchdesc int, err error)
//sysnb    InotifyInit1(flags int) (fd int, err error)
//sysnb    InotifyRmWatch(fd int, watchdesc uint32) (success int, err error)
//sysnb    Kill(pid int, sig Signal) (err error)
//sys    Klogctl(typ int, buf []byte) (n int, err error) = SYS_SYSLOG
//sys    Listxattr(path string, dest []byte) (sz int, err error)
//sys    Mkdirat(dirfd int, path string, mode uint32) (err error)
//sys    Mknodat(dirfd int, path string, mode uint32, dev int) (err error)
//sys    Nanosleep(time *Timespec, leftover *Timespec) (err error)
//sys    PivotRoot(newroot string, putold string) (err error) = SYS_PIVOT_ROOT
//sysnb prlimit(pid int, resource int, newlimit *Rlimit, old *Rlimit) (err error) = SYS_PRLIMIT64
//sys    read(fd int, p []byte) (n int, err error)
//sys    Removexattr(path string, attr string) (err error)
//sys    Setdomainname(p []byte) (err error)
//sys    Sethostname(p []byte) (err error)
//sysnb    Setpgid(pid int, pgid int) (err error)
//sysnb    Setsid() (pid int, err error)
//sysnb    Settimeofday(tv *Timeval) (err error)

// allThreadsCaller holds the input and output state for performing a
// allThreadsSyscall that needs to synchronize all OS thread state. Linux
// generally does not always support this natively, so we have to
// manipulate the runtime to fix things up.
private partial struct allThreadsCaller {
    public System.UIntPtr trap; // return values (only set by 0th invocation)
    public System.UIntPtr a1; // return values (only set by 0th invocation)
    public System.UIntPtr a2; // return values (only set by 0th invocation)
    public System.UIntPtr a3; // return values (only set by 0th invocation)
    public System.UIntPtr a4; // return values (only set by 0th invocation)
    public System.UIntPtr a5; // return values (only set by 0th invocation)
    public System.UIntPtr a6; // return values (only set by 0th invocation)
    public System.UIntPtr r1; // err is the error code
    public System.UIntPtr r2; // err is the error code
    public Errno err;
}

// doSyscall is a callback for executing a syscall on the current m
// (OS thread).
//go:nosplit
//go:norace
private static bool doSyscall(this ptr<allThreadsCaller> _addr_pc, bool initial) => func((_, panic, _) => {
    ref allThreadsCaller pc = ref _addr_pc.val;

    var (r1, r2, err) = RawSyscall(pc.trap, pc.a1, pc.a2, pc.a3);
    if (initial) {
        pc.r1 = r1;
        pc.r2 = r2;
        pc.err = err;
    }
    else if (pc.r1 != r1 || (archHonorsR2 && pc.r2 != r2) || pc.err != err) {
        print("trap:", pc.trap, ", a123=[", pc.a1, ",", pc.a2, ",", pc.a3, "]\n");
        print("results: got {r1=", r1, ",r2=", r2, ",err=", err, "}, want {r1=", pc.r1, ",r2=", pc.r2, ",r3=", pc.err, "}\n");
        panic("AllThreadsSyscall results differ between threads; runtime corrupted");
    }
    return err == 0;

});

// doSyscall6 is a callback for executing a syscall6 on the current m
// (OS thread).
//go:nosplit
//go:norace
private static bool doSyscall6(this ptr<allThreadsCaller> _addr_pc, bool initial) => func((_, panic, _) => {
    ref allThreadsCaller pc = ref _addr_pc.val;

    var (r1, r2, err) = RawSyscall6(pc.trap, pc.a1, pc.a2, pc.a3, pc.a4, pc.a5, pc.a6);
    if (initial) {
        pc.r1 = r1;
        pc.r2 = r2;
        pc.err = err;
    }
    else if (pc.r1 != r1 || (archHonorsR2 && pc.r2 != r2) || pc.err != err) {
        print("trap:", pc.trap, ", a123456=[", pc.a1, ",", pc.a2, ",", pc.a3, ",", pc.a4, ",", pc.a5, ",", pc.a6, "]\n");
        print("results: got {r1=", r1, ",r2=", r2, ",err=", err, "}, want {r1=", pc.r1, ",r2=", pc.r2, ",r3=", pc.err, "}\n");
        panic("AllThreadsSyscall6 results differ between threads; runtime corrupted");
    }
    return err == 0;

});

// Provided by runtime.syscall_runtime_doAllThreadsSyscall which
// serializes the world and invokes the fn on each OS thread (what the
// runtime refers to as m's). Once this function returns, all threads
// are in sync.
private static bool runtime_doAllThreadsSyscall(Func<bool, bool> fn);

// AllThreadsSyscall performs a syscall on each OS thread of the Go
// runtime. It first invokes the syscall on one thread. Should that
// invocation fail, it returns immediately with the error status.
// Otherwise, it invokes the syscall on all of the remaining threads
// in parallel. It will terminate the program if it observes any
// invoked syscall's return value differs from that of the first
// invocation.
//
// AllThreadsSyscall is intended for emulating simultaneous
// process-wide state changes that require consistently modifying
// per-thread state of the Go runtime.
//
// AllThreadsSyscall is unaware of any threads that are launched
// explicitly by cgo linked code, so the function always returns
// ENOTSUP in binaries that use cgo.
//go:uintptrescapes
public static (System.UIntPtr, System.UIntPtr, Errno) AllThreadsSyscall(System.UIntPtr trap, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3) {
    System.UIntPtr r1 = default;
    System.UIntPtr r2 = default;
    Errno err = default;

    if (cgo_libc_setegid != null) {>>MARKER:FUNCTION_runtime_doAllThreadsSyscall_BLOCK_PREFIX<<
        return (minus1, minus1, ENOTSUP);
    }
    ptr<allThreadsCaller> pc = addr(new allThreadsCaller(trap:trap,a1:a1,a2:a2,a3:a3,));
    runtime_doAllThreadsSyscall(pc.doSyscall);
    r1 = pc.r1;
    r2 = pc.r2;
    err = pc.err;
    return ;

}

// AllThreadsSyscall6 is like AllThreadsSyscall, but extended to six
// arguments.
//go:uintptrescapes
public static (System.UIntPtr, System.UIntPtr, Errno) AllThreadsSyscall6(System.UIntPtr trap, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6) {
    System.UIntPtr r1 = default;
    System.UIntPtr r2 = default;
    Errno err = default;

    if (cgo_libc_setegid != null) {
        return (minus1, minus1, ENOTSUP);
    }
    ptr<allThreadsCaller> pc = addr(new allThreadsCaller(trap:trap,a1:a1,a2:a2,a3:a3,a4:a4,a5:a5,a6:a6,));
    runtime_doAllThreadsSyscall(pc.doSyscall6);
    r1 = pc.r1;
    r2 = pc.r2;
    err = pc.err;
    return ;

}

// linked by runtime.cgocall.go
//go:uintptrescapes
private static System.UIntPtr cgocaller(unsafe.Pointer _p0, params System.UIntPtr _p0);

private static unsafe.Pointer cgo_libc_setegid = default; // non-nil if cgo linked.

private static readonly var minus1 = ~uintptr(0);



public static error Setegid(nint egid) {
    error err = default!;

    if (cgo_libc_setegid == null) {>>MARKER:FUNCTION_cgocaller_BLOCK_PREFIX<<
        {
            var (_, _, e1) = AllThreadsSyscall(SYS_SETRESGID, minus1, uintptr(egid), minus1);

            if (e1 != 0) {
                err = errnoErr(e1);
            }

        }

    }    {
        var ret = cgocaller(cgo_libc_setegid, uintptr(egid));


        else if (ret != 0) {
            err = errnoErr(Errno(ret));
        }
    }

    return ;

}

private static unsafe.Pointer cgo_libc_seteuid = default; // non-nil if cgo linked.

public static error Seteuid(nint euid) {
    error err = default!;

    if (cgo_libc_seteuid == null) {
        {
            var (_, _, e1) = AllThreadsSyscall(SYS_SETRESUID, minus1, uintptr(euid), minus1);

            if (e1 != 0) {
                err = errnoErr(e1);
            }

        }

    }    {
        var ret = cgocaller(cgo_libc_seteuid, uintptr(euid));


        else if (ret != 0) {
            err = errnoErr(Errno(ret));
        }
    }

    return ;

}

private static unsafe.Pointer cgo_libc_setgid = default; // non-nil if cgo linked.

public static error Setgid(nint gid) {
    error err = default!;

    if (cgo_libc_setgid == null) {
        {
            var (_, _, e1) = AllThreadsSyscall(sys_SETGID, uintptr(gid), 0, 0);

            if (e1 != 0) {
                err = errnoErr(e1);
            }

        }

    }    {
        var ret = cgocaller(cgo_libc_setgid, uintptr(gid));


        else if (ret != 0) {
            err = errnoErr(Errno(ret));
        }
    }

    return ;

}

private static unsafe.Pointer cgo_libc_setregid = default; // non-nil if cgo linked.

public static error Setregid(nint rgid, nint egid) {
    error err = default!;

    if (cgo_libc_setregid == null) {
        {
            var (_, _, e1) = AllThreadsSyscall(sys_SETREGID, uintptr(rgid), uintptr(egid), 0);

            if (e1 != 0) {
                err = errnoErr(e1);
            }

        }

    }    {
        var ret = cgocaller(cgo_libc_setregid, uintptr(rgid), uintptr(egid));


        else if (ret != 0) {
            err = errnoErr(Errno(ret));
        }
    }

    return ;

}

private static unsafe.Pointer cgo_libc_setresgid = default; // non-nil if cgo linked.

public static error Setresgid(nint rgid, nint egid, nint sgid) {
    error err = default!;

    if (cgo_libc_setresgid == null) {
        {
            var (_, _, e1) = AllThreadsSyscall(sys_SETRESGID, uintptr(rgid), uintptr(egid), uintptr(sgid));

            if (e1 != 0) {
                err = errnoErr(e1);
            }

        }

    }    {
        var ret = cgocaller(cgo_libc_setresgid, uintptr(rgid), uintptr(egid), uintptr(sgid));


        else if (ret != 0) {
            err = errnoErr(Errno(ret));
        }
    }

    return ;

}

private static unsafe.Pointer cgo_libc_setresuid = default; // non-nil if cgo linked.

public static error Setresuid(nint ruid, nint euid, nint suid) {
    error err = default!;

    if (cgo_libc_setresuid == null) {
        {
            var (_, _, e1) = AllThreadsSyscall(sys_SETRESUID, uintptr(ruid), uintptr(euid), uintptr(suid));

            if (e1 != 0) {
                err = errnoErr(e1);
            }

        }

    }    {
        var ret = cgocaller(cgo_libc_setresuid, uintptr(ruid), uintptr(euid), uintptr(suid));


        else if (ret != 0) {
            err = errnoErr(Errno(ret));
        }
    }

    return ;

}

private static unsafe.Pointer cgo_libc_setreuid = default; // non-nil if cgo linked.

public static error Setreuid(nint ruid, nint euid) {
    error err = default!;

    if (cgo_libc_setreuid == null) {
        {
            var (_, _, e1) = AllThreadsSyscall(sys_SETREUID, uintptr(ruid), uintptr(euid), 0);

            if (e1 != 0) {
                err = errnoErr(e1);
            }

        }

    }    {
        var ret = cgocaller(cgo_libc_setreuid, uintptr(ruid), uintptr(euid));


        else if (ret != 0) {
            err = errnoErr(Errno(ret));
        }
    }

    return ;

}

private static unsafe.Pointer cgo_libc_setuid = default; // non-nil if cgo linked.

public static error Setuid(nint uid) {
    error err = default!;

    if (cgo_libc_setuid == null) {
        {
            var (_, _, e1) = AllThreadsSyscall(sys_SETUID, uintptr(uid), 0, 0);

            if (e1 != 0) {
                err = errnoErr(e1);
            }

        }

    }    {
        var ret = cgocaller(cgo_libc_setuid, uintptr(uid));


        else if (ret != 0) {
            err = errnoErr(Errno(ret));
        }
    }

    return ;

}

//sys    Setpriority(which int, who int, prio int) (err error)
//sys    Setxattr(path string, attr string, data []byte, flags int) (err error)
//sys    Sync()
//sysnb    Sysinfo(info *Sysinfo_t) (err error)
//sys    Tee(rfd int, wfd int, len int, flags int) (n int64, err error)
//sysnb    Tgkill(tgid int, tid int, sig Signal) (err error)
//sysnb    Times(tms *Tms) (ticks uintptr, err error)
//sysnb    Umask(mask int) (oldmask int)
//sysnb    Uname(buf *Utsname) (err error)
//sys    Unmount(target string, flags int) (err error) = SYS_UMOUNT2
//sys    Unshare(flags int) (err error)
//sys    write(fd int, p []byte) (n int, err error)
//sys    exitThread(code int) (err error) = SYS_EXIT
//sys    readlen(fd int, p *byte, np int) (n int, err error) = SYS_READ
//sys    writelen(fd int, p *byte, np int) (n int, err error) = SYS_WRITE

// mmap varies by architecture; see syscall_linux_*.go.
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
//sys    Munlock(b []byte) (err error)
//sys    Mlockall(flags int) (err error)
//sys    Munlockall() (err error)

} // end syscall_package
