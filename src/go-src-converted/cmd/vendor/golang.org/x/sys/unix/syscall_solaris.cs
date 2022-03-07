// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Solaris system calls.
// This file is compiled as ordinary Go code,
// but it is also input to mksyscall,
// which parses the //sys lines and generates system call stubs.
// Note that sometimes we use a lowercase //sys name and wrap
// it in our own nicer implementation, either here or in
// syscall_solaris.go or syscall_unix.go.

// package unix -- go2cs converted at 2022 March 06 23:27:17 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\sys\unix\syscall_solaris.go
using runtime = go.runtime_package;
using syscall = go.syscall_package;
using @unsafe = go.@unsafe_package;

namespace go.cmd.vendor.golang.org.x.sys;

public static partial class unix_package {

    // Implemented in runtime/syscall_solaris.go.
private partial struct syscallFunc { // : System.UIntPtr
}

private static (System.UIntPtr, System.UIntPtr, syscall.Errno) rawSysvicall6(System.UIntPtr trap, System.UIntPtr nargs, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6);
private static (System.UIntPtr, System.UIntPtr, syscall.Errno) sysvicall6(System.UIntPtr trap, System.UIntPtr nargs, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6);

// SockaddrDatalink implements the Sockaddr interface for AF_LINK type sockets.
public partial struct SockaddrDatalink {
    public ushort Family;
    public ushort Index;
    public byte Type;
    public byte Nlen;
    public byte Alen;
    public byte Slen;
    public array<sbyte> Data;
    public RawSockaddrDatalink raw;
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
    if (!ok) {>>MARKER:FUNCTION_sysvicall6_BLOCK_PREFIX<<
        return (0, false);
    }
    return (reclen - uint64(@unsafe.Offsetof(new Dirent().Name)), true);

}

//sysnb    pipe(p *[2]_C_int) (n int, err error)

public static error Pipe(slice<nint> p) {
    error err = default!;

    if (len(p) != 2) {>>MARKER:FUNCTION_rawSysvicall6_BLOCK_PREFIX<<
        return error.As(EINVAL)!;
    }
    ref array<_C_int> pp = ref heap(new array<_C_int>(2), out ptr<array<_C_int>> _addr_pp);
    var (n, err) = pipe(_addr_pp);
    if (n != 0) {
        return error.As(err)!;
    }
    p[0] = int(pp[0]);
    p[1] = int(pp[1]);
    return error.As(null!)!;

}

//sysnb    pipe2(p *[2]_C_int, flags int) (err error)

public static error Pipe2(slice<nint> p, nint flags) {
    if (len(p) != 2) {
        return error.As(EINVAL)!;
    }
    ref array<_C_int> pp = ref heap(new array<_C_int>(2), out ptr<array<_C_int>> _addr_pp);
    var err = pipe2(_addr_pp, flags);
    p[0] = int(pp[0]);
    p[1] = int(pp[1]);
    return error.As(err)!;

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
    if (n >= len(sa.raw.Path)) {
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

//sys    getsockname(fd int, rsa *RawSockaddrAny, addrlen *_Socklen) (err error) = libsocket.getsockname

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

public static readonly var ImplementsGetwd = true;

//sys    Getcwd(buf []byte) (n int, err error)



//sys    Getcwd(buf []byte) (n int, err error)

public static (@string, error) Getwd() {
    @string wd = default;
    error err = default!;

    array<byte> buf = new array<byte>(PathMax); 
    // Getcwd will return an error if it failed for any reason.
    _, err = Getcwd(buf[(int)0..]);
    if (err != null) {
        return ("", error.As(err)!);
    }
    var n = clen(buf[..]);
    if (n < 1) {
        return ("", error.As(EINVAL)!);
    }
    return (string(buf[..(int)n]), error.As(null!)!);

}

/*
 * Wrapped
 */

//sysnb    getgroups(ngid int, gid *_Gid_t) (n int, err error)
//sysnb    setgroups(ngid int, gid *_Gid_t) (err error)

public static (slice<nint>, error) Getgroups() {
    slice<nint> gids = default;
    error err = default!;

    var (n, err) = getgroups(0, null); 
    // Check for error and sanity check group count. Newer versions of
    // Solaris allow up to 1024 (NGROUPS_MAX).
    if (n < 0 || n > 1024) {
        if (err != null) {
            return (null, error.As(err)!);
        }
        return (null, error.As(EINVAL)!);

    }
    else if (n == 0) {
        return (null, error.As(null!)!);
    }
    var a = make_slice<_Gid_t>(n);
    n, err = getgroups(n, _addr_a[0]);
    if (n == -1) {
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

// ReadDirent reads directory entries from fd and writes them into buf.
public static (nint, error) ReadDirent(nint fd, slice<byte> buf) {
    nint n = default;
    error err = default!;
 
    // Final argument is (basep *uintptr) and the syscall doesn't take nil.
    // TODO(rsc): Can we use a single global basep for all calls?
    return Getdents(fd, buf, @new<uintptr>());

}

// Wait status is 7 bits at bottom, either 0 (exited),
// 0x7F (stopped), or a signal number that caused an exit.
// The 0x80 bit is whether there was a core dump.
// An extra number (exit code, signal causing a stop)
// is in the high bits.

public partial struct WaitStatus { // : uint
}

private static readonly nuint mask = 0x7F;
private static readonly nuint core = 0x80;
private static readonly nint shift = 8;

private static readonly nint exited = 0;
private static readonly nuint stopped = 0x7F;


public static bool Exited(this WaitStatus w) {
    return w & mask == exited;
}

public static nint ExitStatus(this WaitStatus w) {
    if (w & mask != exited) {
        return -1;
    }
    return int(w >> (int)(shift));

}

public static bool Signaled(this WaitStatus w) {
    return w & mask != stopped && w & mask != 0;
}

public static syscall.Signal Signal(this WaitStatus w) {
    var sig = syscall.Signal(w & mask);
    if (sig == stopped || sig == 0) {
        return -1;
    }
    return sig;

}

public static bool CoreDump(this WaitStatus w) {
    return w.Signaled() && w & core != 0;
}

public static bool Stopped(this WaitStatus w) {
    return w & mask == stopped && syscall.Signal(w >> (int)(shift)) != SIGSTOP;
}

public static bool Continued(this WaitStatus w) {
    return w & mask == stopped && syscall.Signal(w >> (int)(shift)) == SIGSTOP;
}

public static syscall.Signal StopSignal(this WaitStatus w) {
    if (!w.Stopped()) {
        return -1;
    }
    return syscall.Signal(w >> (int)(shift)) & 0xFF;

}

public static nint TrapCause(this WaitStatus w) {
    return -1;
}

//sys    wait4(pid int32, statusp *_C_int, options int, rusage *Rusage) (wpid int32, err error)

public static (nint, error) Wait4(nint pid, ptr<WaitStatus> _addr_wstatus, nint options, ptr<Rusage> _addr_rusage) {
    nint _p0 = default;
    error _p0 = default!;
    ref WaitStatus wstatus = ref _addr_wstatus.val;
    ref Rusage rusage = ref _addr_rusage.val;

    ref _C_int status = ref heap(out ptr<_C_int> _addr_status);
    var (rpid, err) = wait4(int32(pid), _addr_status, options, rusage);
    var wpid = int(rpid);
    if (wpid == -1) {
        return (wpid, error.As(err)!);
    }
    if (wstatus != null) {
        wstatus = WaitStatus(status);
    }
    return (wpid, error.As(null!)!);

}

//sys    gethostname(buf []byte) (n int, err error)

public static (@string, error) Gethostname() {
    @string name = default;
    error err = default!;

    array<byte> buf = new array<byte>(MaxHostNameLen);
    var (n, err) = gethostname(buf[..]);
    if (n != 0) {
        return ("", error.As(err)!);
    }
    n = clen(buf[..]);
    if (n < 1) {
        return ("", error.As(EFAULT)!);
    }
    return (string(buf[..(int)n]), error.As(null!)!);

}

//sys    utimes(path string, times *[2]Timeval) (err error)

public static error Utimes(@string path, slice<Timeval> tv) {
    error err = default!;

    if (tv == null) {
        return error.As(utimes(path, null))!;
    }
    if (len(tv) != 2) {
        return error.As(EINVAL)!;
    }
    return error.As(utimes(path, new ptr<ptr<array<Timeval>>>(@unsafe.Pointer(_addr_tv[0]))))!;

}

//sys    utimensat(fd int, path string, times *[2]Timespec, flag int) (err error)

public static error UtimesNano(@string path, slice<Timespec> ts) {
    if (ts == null) {
        return error.As(utimensat(AT_FDCWD, path, null, 0))!;
    }
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

//sys    fcntl(fd int, cmd int, arg int) (val int, err error)

// FcntlInt performs a fcntl syscall on fd with the provided command and argument.
public static (nint, error) FcntlInt(System.UIntPtr fd, nint cmd, nint arg) {
    nint _p0 = default;
    error _p0 = default!;

    var (valptr, _, errno) = sysvicall6(uintptr(@unsafe.Pointer(_addr_procfcntl)), 3, uintptr(fd), uintptr(cmd), uintptr(arg), 0, 0, 0);
    error err = default!;
    if (errno != 0) {
        err = error.As(errno)!;
    }
    return (int(valptr), error.As(err)!);

}

// FcntlFlock performs a fcntl syscall for the F_GETLK, F_SETLK or F_SETLKW command.
public static error FcntlFlock(System.UIntPtr fd, nint cmd, ptr<Flock_t> _addr_lk) {
    ref Flock_t lk = ref _addr_lk.val;

    var (_, _, e1) = sysvicall6(uintptr(@unsafe.Pointer(_addr_procfcntl)), 3, uintptr(fd), uintptr(cmd), uintptr(@unsafe.Pointer(lk)), 0, 0, 0);
    if (e1 != 0) {
        return error.As(e1)!;
    }
    return error.As(null!)!;

}

//sys    futimesat(fildes int, path *byte, times *[2]Timeval) (err error)

public static error Futimesat(nint dirfd, @string path, slice<Timeval> tv) {
    var (pathp, err) = BytePtrFromString(path);
    if (err != null) {
        return error.As(err)!;
    }
    if (tv == null) {
        return error.As(futimesat(dirfd, pathp, null))!;
    }
    if (len(tv) != 2) {
        return error.As(EINVAL)!;
    }
    return error.As(futimesat(dirfd, pathp, new ptr<ptr<array<Timeval>>>(@unsafe.Pointer(_addr_tv[0]))))!;

}

// Solaris doesn't have an futimes function because it allows NULL to be
// specified as the path for futimesat. However, Go doesn't like
// NULL-style string interfaces, so this simple wrapper is provided.
public static error Futimes(nint fd, slice<Timeval> tv) {
    if (tv == null) {
        return error.As(futimesat(fd, null, null))!;
    }
    if (len(tv) != 2) {
        return error.As(EINVAL)!;
    }
    return error.As(futimesat(fd, null, new ptr<ptr<array<Timeval>>>(@unsafe.Pointer(_addr_tv[0]))))!;

}

private static (Sockaddr, error) anyToSockaddr(nint fd, ptr<RawSockaddrAny> _addr_rsa) {
    Sockaddr _p0 = default;
    error _p0 = default!;
    ref RawSockaddrAny rsa = ref _addr_rsa.val;


    if (rsa.Addr.Family == AF_UNIX) 
        var pp = (RawSockaddrUnix.val)(@unsafe.Pointer(rsa));
        ptr<SockaddrUnix> sa = @new<SockaddrUnix>(); 
        // Assume path ends at NUL.
        // This is not technically the Solaris semantics for
        // abstract Unix domain sockets -- they are supposed
        // to be uninterpreted fixed-size binary blobs -- but
        // everyone uses this convention.
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

//sys    accept(s int, rsa *RawSockaddrAny, addrlen *_Socklen) (fd int, err error) = libsocket.accept

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

//sys    recvmsg(s int, msg *Msghdr, flags int) (n int, err error) = libsocket.__xnet_recvmsg

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
        iov.Base = (int8.val)(@unsafe.Pointer(_addr_p[0]));
        iov.SetLen(len(p));
    }
    ref sbyte dummy = ref heap(out ptr<sbyte> _addr_dummy);
    if (len(oob) > 0) { 
        // receive at least one normal byte
        if (len(p) == 0) {
            _addr_iov.Base = _addr_dummy;
            iov.Base = ref _addr_iov.Base.val;
            iov.SetLen(1);

        }
        msg.Accrightslen = int32(len(oob));

    }
    _addr_msg.Iov = _addr_iov;
    msg.Iov = ref _addr_msg.Iov.val;
    msg.Iovlen = 1;
    n, err = recvmsg(fd, _addr_msg, flags);

    if (n == -1) {
        return ;
    }
    oobn = int(msg.Accrightslen); 
    // source address is only specified if the socket is unconnected
    if (rsa.Addr.Family != AF_UNSPEC) {
        from, err = anyToSockaddr(fd, _addr_rsa);
    }
    return ;

}

public static error Sendmsg(nint fd, slice<byte> p, slice<byte> oob, Sockaddr to, nint flags) {
    error err = default!;

    _, err = SendmsgN(fd, p, oob, to, flags);
    return ;
}

//sys    sendmsg(s int, msg *Msghdr, flags int) (n int, err error) = libsocket.__xnet_sendmsg

public static (nint, error) SendmsgN(nint fd, slice<byte> p, slice<byte> oob, Sockaddr to, nint flags) {
    nint n = default;
    error err = default!;

    unsafe.Pointer ptr = default;
    _Socklen salen = default;
    if (to != null) {
        ptr, salen, err = to.sockaddr();
        if (err != null) {
            return (0, error.As(err)!);
        }
    }
    ref Msghdr msg = ref heap(out ptr<Msghdr> _addr_msg);
    msg.Name = (byte.val)(@unsafe.Pointer(ptr));
    msg.Namelen = uint32(salen);
    ref Iovec iov = ref heap(out ptr<Iovec> _addr_iov);
    if (len(p) > 0) {
        iov.Base = (int8.val)(@unsafe.Pointer(_addr_p[0]));
        iov.SetLen(len(p));
    }
    ref sbyte dummy = ref heap(out ptr<sbyte> _addr_dummy);
    if (len(oob) > 0) { 
        // send at least one normal byte
        if (len(p) == 0) {
            _addr_iov.Base = _addr_dummy;
            iov.Base = ref _addr_iov.Base.val;
            iov.SetLen(1);

        }
        msg.Accrightslen = int32(len(oob));

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

//sys    acct(path *byte) (err error)

public static error Acct(@string path) {
    error err = default!;

    if (len(path) == 0) { 
        // Assume caller wants to disable accounting.
        return error.As(acct(null))!;

    }
    var (pathp, err) = BytePtrFromString(path);
    if (err != null) {
        return error.As(err)!;
    }
    return error.As(acct(pathp))!;

}

//sys    __makedev(version int, major uint, minor uint) (val uint64)

public static ulong Mkdev(uint major, uint minor) {
    return __makedev(NEWDEV, uint(major), uint(minor));
}

//sys    __major(version int, dev uint64) (val uint)

public static uint Major(ulong dev) {
    return uint32(__major(NEWDEV, dev));
}

//sys    __minor(version int, dev uint64) (val uint)

public static uint Minor(ulong dev) {
    return uint32(__minor(NEWDEV, dev));
}

/*
 * Expose the ioctl function
 */

//sys    ioctlRet(fd int, req uint, arg uintptr) (ret int, err error) = libc.ioctl

private static error ioctl(nint fd, nuint req, System.UIntPtr arg) {
    error err = default!;

    _, err = ioctlRet(fd, req, arg);
    return error.As(err)!;
}

public static error IoctlSetTermio(nint fd, nuint req, ptr<Termio> _addr_value) {
    ref Termio value = ref _addr_value.val;

    var err = ioctl(fd, req, uintptr(@unsafe.Pointer(value)));
    runtime.KeepAlive(value);
    return error.As(err)!;
}

public static (ptr<Termio>, error) IoctlGetTermio(nint fd, nuint req) {
    ptr<Termio> _p0 = default!;
    error _p0 = default!;

    ref Termio value = ref heap(out ptr<Termio> _addr_value);
    var err = ioctl(fd, req, uintptr(@unsafe.Pointer(_addr_value)));
    return (_addr__addr_value!, error.As(err)!);
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

public static (nint, error) Sendfile(nint outfd, nint infd, ptr<long> _addr_offset, nint count) {
    nint written = default;
    error err = default!;
    ref long offset = ref _addr_offset.val;

    if (raceenabled) {
        raceReleaseMerge(@unsafe.Pointer(_addr_ioSync));
    }
    return sendfile(outfd, infd, offset, count);

}

/*
 * Exposed directly
 */
//sys    Access(path string, mode uint32) (err error)
//sys    Adjtime(delta *Timeval, olddelta *Timeval) (err error)
//sys    Chdir(path string) (err error)
//sys    Chmod(path string, mode uint32) (err error)
//sys    Chown(path string, uid int, gid int) (err error)
//sys    Chroot(path string) (err error)
//sys    Close(fd int) (err error)
//sys    Creat(path string, mode uint32) (fd int, err error)
//sys    Dup(fd int) (nfd int, err error)
//sys    Dup2(oldfd int, newfd int) (err error)
//sys    Exit(code int)
//sys    Faccessat(dirfd int, path string, mode uint32, flags int) (err error)
//sys    Fchdir(fd int) (err error)
//sys    Fchmod(fd int, mode uint32) (err error)
//sys    Fchmodat(dirfd int, path string, mode uint32, flags int) (err error)
//sys    Fchown(fd int, uid int, gid int) (err error)
//sys    Fchownat(dirfd int, path string, uid int, gid int, flags int) (err error)
//sys    Fdatasync(fd int) (err error)
//sys    Flock(fd int, how int) (err error)
//sys    Fpathconf(fd int, name int) (val int, err error)
//sys    Fstat(fd int, stat *Stat_t) (err error)
//sys    Fstatat(fd int, path string, stat *Stat_t, flags int) (err error)
//sys    Fstatvfs(fd int, vfsstat *Statvfs_t) (err error)
//sys    Getdents(fd int, buf []byte, basep *uintptr) (n int, err error)
//sysnb    Getgid() (gid int)
//sysnb    Getpid() (pid int)
//sysnb    Getpgid(pid int) (pgid int, err error)
//sysnb    Getpgrp() (pgid int, err error)
//sys    Geteuid() (euid int)
//sys    Getegid() (egid int)
//sys    Getppid() (ppid int)
//sys    Getpriority(which int, who int) (n int, err error)
//sysnb    Getrlimit(which int, lim *Rlimit) (err error)
//sysnb    Getrusage(who int, rusage *Rusage) (err error)
//sysnb    Gettimeofday(tv *Timeval) (err error)
//sysnb    Getuid() (uid int)
//sys    Kill(pid int, signum syscall.Signal) (err error)
//sys    Lchown(path string, uid int, gid int) (err error)
//sys    Link(path string, link string) (err error)
//sys    Listen(s int, backlog int) (err error) = libsocket.__xnet_llisten
//sys    Lstat(path string, stat *Stat_t) (err error)
//sys    Madvise(b []byte, advice int) (err error)
//sys    Mkdir(path string, mode uint32) (err error)
//sys    Mkdirat(dirfd int, path string, mode uint32) (err error)
//sys    Mkfifo(path string, mode uint32) (err error)
//sys    Mkfifoat(dirfd int, path string, mode uint32) (err error)
//sys    Mknod(path string, mode uint32, dev int) (err error)
//sys    Mknodat(dirfd int, path string, mode uint32, dev int) (err error)
//sys    Mlock(b []byte) (err error)
//sys    Mlockall(flags int) (err error)
//sys    Mprotect(b []byte, prot int) (err error)
//sys    Msync(b []byte, flags int) (err error)
//sys    Munlock(b []byte) (err error)
//sys    Munlockall() (err error)
//sys    Nanosleep(time *Timespec, leftover *Timespec) (err error)
//sys    Open(path string, mode int, perm uint32) (fd int, err error)
//sys    Openat(dirfd int, path string, flags int, mode uint32) (fd int, err error)
//sys    Pathconf(path string, name int) (val int, err error)
//sys    Pause() (err error)
//sys    Pread(fd int, p []byte, offset int64) (n int, err error)
//sys    Pwrite(fd int, p []byte, offset int64) (n int, err error)
//sys    read(fd int, p []byte) (n int, err error)
//sys    Readlink(path string, buf []byte) (n int, err error)
//sys    Rename(from string, to string) (err error)
//sys    Renameat(olddirfd int, oldpath string, newdirfd int, newpath string) (err error)
//sys    Rmdir(path string) (err error)
//sys    Seek(fd int, offset int64, whence int) (newoffset int64, err error) = lseek
//sys    Select(nfd int, r *FdSet, w *FdSet, e *FdSet, timeout *Timeval) (n int, err error)
//sysnb    Setegid(egid int) (err error)
//sysnb    Seteuid(euid int) (err error)
//sysnb    Setgid(gid int) (err error)
//sys    Sethostname(p []byte) (err error)
//sysnb    Setpgid(pid int, pgid int) (err error)
//sys    Setpriority(which int, who int, prio int) (err error)
//sysnb    Setregid(rgid int, egid int) (err error)
//sysnb    Setreuid(ruid int, euid int) (err error)
//sysnb    Setrlimit(which int, lim *Rlimit) (err error)
//sysnb    Setsid() (pid int, err error)
//sysnb    Setuid(uid int) (err error)
//sys    Shutdown(s int, how int) (err error) = libsocket.shutdown
//sys    Stat(path string, stat *Stat_t) (err error)
//sys    Statvfs(path string, vfsstat *Statvfs_t) (err error)
//sys    Symlink(path string, link string) (err error)
//sys    Sync() (err error)
//sys    Sysconf(which int) (n int64, err error)
//sysnb    Times(tms *Tms) (ticks uintptr, err error)
//sys    Truncate(path string, length int64) (err error)
//sys    Fsync(fd int) (err error)
//sys    Ftruncate(fd int, length int64) (err error)
//sys    Umask(mask int) (oldmask int)
//sysnb    Uname(buf *Utsname) (err error)
//sys    Unmount(target string, flags int) (err error) = libc.umount
//sys    Unlink(path string) (err error)
//sys    Unlinkat(dirfd int, path string, flags int) (err error)
//sys    Ustat(dev int, ubuf *Ustat_t) (err error)
//sys    Utime(path string, buf *Utimbuf) (err error)
//sys    bind(s int, addr unsafe.Pointer, addrlen _Socklen) (err error) = libsocket.__xnet_bind
//sys    connect(s int, addr unsafe.Pointer, addrlen _Socklen) (err error) = libsocket.__xnet_connect
//sys    mmap(addr uintptr, length uintptr, prot int, flag int, fd int, pos int64) (ret uintptr, err error)
//sys    munmap(addr uintptr, length uintptr) (err error)
//sys    sendfile(outfd int, infd int, offset *int64, count int) (written int, err error) = libsendfile.sendfile
//sys    sendto(s int, buf []byte, flags int, to unsafe.Pointer, addrlen _Socklen) (err error) = libsocket.__xnet_sendto
//sys    socket(domain int, typ int, proto int) (fd int, err error) = libsocket.__xnet_socket
//sysnb    socketpair(domain int, typ int, proto int, fd *[2]int32) (err error) = libsocket.__xnet_socketpair
//sys    write(fd int, p []byte) (n int, err error)
//sys    getsockopt(s int, level int, name int, val unsafe.Pointer, vallen *_Socklen) (err error) = libsocket.__xnet_getsockopt
//sysnb    getpeername(fd int, rsa *RawSockaddrAny, addrlen *_Socklen) (err error) = libsocket.getpeername
//sys    setsockopt(s int, level int, name int, val unsafe.Pointer, vallen uintptr) (err error) = libsocket.setsockopt
//sys    recvfrom(fd int, p []byte, flags int, from *RawSockaddrAny, fromlen *_Socklen) (n int, err error) = libsocket.recvfrom

private static (nint, error) readlen(nint fd, ptr<byte> _addr_buf, nint nbuf) {
    nint n = default;
    error err = default!;
    ref byte buf = ref _addr_buf.val;

    var (r0, _, e1) = sysvicall6(uintptr(@unsafe.Pointer(_addr_procread)), 3, uintptr(fd), uintptr(@unsafe.Pointer(buf)), uintptr(nbuf), 0, 0, 0);
    n = int(r0);
    if (e1 != 0) {
        err = e1;
    }
    return ;

}

private static (nint, error) writelen(nint fd, ptr<byte> _addr_buf, nint nbuf) {
    nint n = default;
    error err = default!;
    ref byte buf = ref _addr_buf.val;

    var (r0, _, e1) = sysvicall6(uintptr(@unsafe.Pointer(_addr_procwrite)), 3, uintptr(fd), uintptr(@unsafe.Pointer(buf)), uintptr(nbuf), 0, 0, 0);
    n = int(r0);
    if (e1 != 0) {
        err = e1;
    }
    return ;

}

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

} // end unix_package
