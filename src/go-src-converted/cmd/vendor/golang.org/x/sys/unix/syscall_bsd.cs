// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build darwin || dragonfly || freebsd || netbsd || openbsd
// +build darwin dragonfly freebsd netbsd openbsd

// BSD system call wrappers shared by *BSD based systems
// including OS X (Darwin) and FreeBSD.  Like the other
// syscall_*.go files it is compiled as Go code but also
// used as input to mksyscall which parses the //sys
// lines and generates system call stubs.

// package unix -- go2cs converted at 2022 March 13 06:41:22 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\sys\unix\syscall_bsd.go
namespace go.cmd.vendor.golang.org.x.sys;

using runtime = runtime_package;
using syscall = syscall_package;
using @unsafe = @unsafe_package;

public static partial class unix_package {

public static readonly var ImplementsGetwd = true;



public static (@string, error) Getwd() {
    @string _p0 = default;
    error _p0 = default!;

    array<byte> buf = new array<byte>(PathMax);
    var (_, err) = Getcwd(buf[(int)0..]);
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
private static readonly nint killed = 9;
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

public static bool Killed(this WaitStatus w) {
    return w & mask == killed && syscall.Signal(w >> (int)(shift)) != SIGKILL;
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

//sys    accept(s int, rsa *RawSockaddrAny, addrlen *_Socklen) (fd int, err error)
//sys    bind(s int, addr unsafe.Pointer, addrlen _Socklen) (err error)
//sys    connect(s int, addr unsafe.Pointer, addrlen _Socklen) (err error)
//sysnb    socket(domain int, typ int, proto int) (fd int, err error)
//sys    getsockopt(s int, level int, name int, val unsafe.Pointer, vallen *_Socklen) (err error)
//sys    setsockopt(s int, level int, name int, val unsafe.Pointer, vallen uintptr) (err error)
//sysnb    getpeername(fd int, rsa *RawSockaddrAny, addrlen *_Socklen) (err error)
//sysnb    getsockname(fd int, rsa *RawSockaddrAny, addrlen *_Socklen) (err error)
//sys    Shutdown(s int, how int) (err error)

private static (unsafe.Pointer, _Socklen, error) sockaddr(this ptr<SockaddrInet4> _addr_sa) {
    unsafe.Pointer _p0 = default;
    _Socklen _p0 = default;
    error _p0 = default!;
    ref SockaddrInet4 sa = ref _addr_sa.val;

    if (sa.Port < 0 || sa.Port > 0xFFFF) {
        return (null, 0, error.As(EINVAL)!);
    }
    sa.raw.Len = SizeofSockaddrInet4;
    sa.raw.Family = AF_INET;
    ptr<array<byte>> p = new ptr<ptr<array<byte>>>(@unsafe.Pointer(_addr_sa.raw.Port));
    p[0] = byte(sa.Port >> 8);
    p[1] = byte(sa.Port);
    for (nint i = 0; i < len(sa.Addr); i++) {
        sa.raw.Addr[i] = sa.Addr[i];
    }
    return (@unsafe.Pointer(_addr_sa.raw), _Socklen(sa.raw.Len), error.As(null!)!);
}

private static (unsafe.Pointer, _Socklen, error) sockaddr(this ptr<SockaddrInet6> _addr_sa) {
    unsafe.Pointer _p0 = default;
    _Socklen _p0 = default;
    error _p0 = default!;
    ref SockaddrInet6 sa = ref _addr_sa.val;

    if (sa.Port < 0 || sa.Port > 0xFFFF) {
        return (null, 0, error.As(EINVAL)!);
    }
    sa.raw.Len = SizeofSockaddrInet6;
    sa.raw.Family = AF_INET6;
    ptr<array<byte>> p = new ptr<ptr<array<byte>>>(@unsafe.Pointer(_addr_sa.raw.Port));
    p[0] = byte(sa.Port >> 8);
    p[1] = byte(sa.Port);
    sa.raw.Scope_id = sa.ZoneId;
    for (nint i = 0; i < len(sa.Addr); i++) {
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
    if (n >= len(sa.raw.Path) || n == 0) {
        return (null, 0, error.As(EINVAL)!);
    }
    sa.raw.Len = byte(3 + n); // 2 for Family, Len; 1 for NUL
    sa.raw.Family = AF_UNIX;
    for (nint i = 0; i < n; i++) {
        sa.raw.Path[i] = int8(name[i]);
    }
    return (@unsafe.Pointer(_addr_sa.raw), _Socklen(sa.raw.Len), error.As(null!)!);
}

private static (unsafe.Pointer, _Socklen, error) sockaddr(this ptr<SockaddrDatalink> _addr_sa) {
    unsafe.Pointer _p0 = default;
    _Socklen _p0 = default;
    error _p0 = default!;
    ref SockaddrDatalink sa = ref _addr_sa.val;

    if (sa.Index == 0) {
        return (null, 0, error.As(EINVAL)!);
    }
    sa.raw.Len = sa.Len;
    sa.raw.Family = AF_LINK;
    sa.raw.Index = sa.Index;
    sa.raw.Type = sa.Type;
    sa.raw.Nlen = sa.Nlen;
    sa.raw.Alen = sa.Alen;
    sa.raw.Slen = sa.Slen;
    for (nint i = 0; i < len(sa.raw.Data); i++) {
        sa.raw.Data[i] = sa.Data[i];
    }
    return (@unsafe.Pointer(_addr_sa.raw), SizeofSockaddrDatalink, error.As(null!)!);
}

private static (Sockaddr, error) anyToSockaddr(nint fd, ptr<RawSockaddrAny> _addr_rsa) {
    Sockaddr _p0 = default;
    error _p0 = default!;
    ref RawSockaddrAny rsa = ref _addr_rsa.val;


    if (rsa.Addr.Family == AF_LINK) 
        var pp = (RawSockaddrDatalink.val)(@unsafe.Pointer(rsa));
        ptr<SockaddrDatalink> sa = @new<SockaddrDatalink>();
        sa.Len = pp.Len;
        sa.Family = pp.Family;
        sa.Index = pp.Index;
        sa.Type = pp.Type;
        sa.Nlen = pp.Nlen;
        sa.Alen = pp.Alen;
        sa.Slen = pp.Slen;
        {
            nint i__prev1 = i;

            for (nint i = 0; i < len(sa.Data); i++) {
                sa.Data[i] = pp.Data[i];
            }


            i = i__prev1;
        }
        return (sa, error.As(null!)!);
    else if (rsa.Addr.Family == AF_UNIX) 
        pp = (RawSockaddrUnix.val)(@unsafe.Pointer(rsa));
        if (pp.Len < 2 || pp.Len > SizeofSockaddrUnix) {
            return (null, error.As(EINVAL)!);
        }
        sa = @new<SockaddrUnix>(); 

        // Some BSDs include the trailing NUL in the length, whereas
        // others do not. Work around this by subtracting the leading
        // family and len. The path is then scanned to see if a NUL
        // terminator still exists within the length.
        var n = int(pp.Len) - 2; // subtract leading Family, Len
        {
            nint i__prev1 = i;

            for (i = 0; i < n; i++) {
                if (pp.Path[i] == 0) { 
                    // found early NUL; assume Len included the NUL
                    // or was overestimating.
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
        return anyToSockaddrGOOS(fd, rsa);
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
    if ((runtime.GOOS == "darwin" || runtime.GOOS == "ios") && len == 0) { 
        // Accepted socket has no address.
        // This is likely due to a bug in xnu kernels,
        // where instead of ECONNABORTED error socket
        // is accepted, but has no address.
        Close(nfd);
        return (0, null, error.As(ECONNABORTED)!);
    }
    sa, err = anyToSockaddr(fd, _addr_rsa);
    if (err != null) {
        Close(nfd);
        nfd = 0;
    }
    return ;
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
    if (runtime.GOOS == "dragonfly" && rsa.Addr.Family == AF_UNSPEC && rsa.Addr.Len == 0) {
        rsa.Addr.Family = AF_UNIX;
        rsa.Addr.Len = SizeofSockaddrUnix;
    }
    return anyToSockaddr(fd, _addr_rsa);
}

//sysnb    socketpair(domain int, typ int, proto int, fd *[2]int32) (err error)

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

//sys    recvfrom(fd int, p []byte, flags int, from *RawSockaddrAny, fromlen *_Socklen) (n int, err error)
//sys    sendto(s int, buf []byte, flags int, to unsafe.Pointer, addrlen _Socklen) (err error)
//sys    recvmsg(s int, msg *Msghdr, flags int) (n int, err error)

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
        from, err = anyToSockaddr(fd, _addr_rsa);
    }
    return ;
}

//sys    sendmsg(s int, msg *Msghdr, flags int) (n int, err error)

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

//sys    kevent(kq int, change unsafe.Pointer, nchange int, event unsafe.Pointer, nevent int, timeout *Timespec) (n int, err error)

public static (nint, error) Kevent(nint kq, slice<Kevent_t> changes, slice<Kevent_t> events, ptr<Timespec> _addr_timeout) {
    nint n = default;
    error err = default!;
    ref Timespec timeout = ref _addr_timeout.val;

    unsafe.Pointer change = default;    unsafe.Pointer @event = default;

    if (len(changes) > 0) {
        change = @unsafe.Pointer(_addr_changes[0]);
    }
    if (len(events) > 0) {
        event = @unsafe.Pointer(_addr_events[0]);
    }
    return kevent(kq, change, len(changes), event, len(events), timeout);
}

// sysctlmib translates name to mib number and appends any additional args.
private static (slice<_C_int>, error) sysctlmib(@string name, params nint[] args) {
    slice<_C_int> _p0 = default;
    error _p0 = default!;
    args = args.Clone();
 
    // Translate name to mib number.
    var (mib, err) = nametomib(name);
    if (err != null) {
        return (null, error.As(err)!);
    }
    foreach (var (_, a) in args) {
        mib = append(mib, _C_int(a));
    }    return (mib, error.As(null!)!);
}

public static (@string, error) Sysctl(@string name) {
    @string _p0 = default;
    error _p0 = default!;

    return SysctlArgs(name);
}

public static (@string, error) SysctlArgs(@string name, params nint[] args) {
    @string _p0 = default;
    error _p0 = default!;
    args = args.Clone();

    var (buf, err) = SysctlRaw(name, args);
    if (err != null) {
        return ("", error.As(err)!);
    }
    var n = len(buf); 

    // Throw away terminating NUL.
    if (n > 0 && buf[n - 1] == '\x00') {
        n--;
    }
    return (string(buf[(int)0..(int)n]), error.As(null!)!);
}

public static (uint, error) SysctlUint32(@string name) {
    uint _p0 = default;
    error _p0 = default!;

    return SysctlUint32Args(name);
}

public static (uint, error) SysctlUint32Args(@string name, params nint[] args) {
    uint _p0 = default;
    error _p0 = default!;
    args = args.Clone();

    var (mib, err) = sysctlmib(name, args);
    if (err != null) {
        return (0, error.As(err)!);
    }
    ref var n = ref heap(uintptr(4), out ptr<var> _addr_n);
    var buf = make_slice<byte>(4);
    {
        var err = sysctl(mib, _addr_buf[0], _addr_n, null, 0);

        if (err != null) {
            return (0, error.As(err)!);
        }
    }
    if (n != 4) {
        return (0, error.As(EIO)!);
    }
    return (new ptr<ptr<ptr<uint>>>(@unsafe.Pointer(_addr_buf[0])), error.As(null!)!);
}

public static (ulong, error) SysctlUint64(@string name, params nint[] args) {
    ulong _p0 = default;
    error _p0 = default!;
    args = args.Clone();

    var (mib, err) = sysctlmib(name, args);
    if (err != null) {
        return (0, error.As(err)!);
    }
    ref var n = ref heap(uintptr(8), out ptr<var> _addr_n);
    var buf = make_slice<byte>(8);
    {
        var err = sysctl(mib, _addr_buf[0], _addr_n, null, 0);

        if (err != null) {
            return (0, error.As(err)!);
        }
    }
    if (n != 8) {
        return (0, error.As(EIO)!);
    }
    return (new ptr<ptr<ptr<ulong>>>(@unsafe.Pointer(_addr_buf[0])), error.As(null!)!);
}

public static (slice<byte>, error) SysctlRaw(@string name, params nint[] args) {
    slice<byte> _p0 = default;
    error _p0 = default!;
    args = args.Clone();

    var (mib, err) = sysctlmib(name, args);
    if (err != null) {
        return (null, error.As(err)!);
    }
    ref var n = ref heap(uintptr(0), out ptr<var> _addr_n);
    {
        var err__prev1 = err;

        var err = sysctl(mib, null, _addr_n, null, 0);

        if (err != null) {
            return (null, error.As(err)!);
        }
        err = err__prev1;

    }
    if (n == 0) {
        return (null, error.As(null!)!);
    }
    var buf = make_slice<byte>(n);
    {
        var err__prev1 = err;

        err = sysctl(mib, _addr_buf[0], _addr_n, null, 0);

        if (err != null) {
            return (null, error.As(err)!);
        }
        err = err__prev1;

    } 

    // The actual call may return less than the original reported required
    // size so ensure we deal with that.
    return (buf[..(int)n], error.As(null!)!);
}

public static (ptr<Clockinfo>, error) SysctlClockinfo(@string name) {
    ptr<Clockinfo> _p0 = default!;
    error _p0 = default!;

    var (mib, err) = sysctlmib(name);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    ref var n = ref heap(uintptr(SizeofClockinfo), out ptr<var> _addr_n);
    ref Clockinfo ci = ref heap(out ptr<Clockinfo> _addr_ci);
    {
        var err = sysctl(mib, (byte.val)(@unsafe.Pointer(_addr_ci)), _addr_n, null, 0);

        if (err != null) {
            return (_addr_null!, error.As(err)!);
        }
    }
    if (n != SizeofClockinfo) {
        return (_addr_null!, error.As(EIO)!);
    }
    return (_addr__addr_ci!, error.As(null!)!);
}

public static (ptr<Timeval>, error) SysctlTimeval(@string name) {
    ptr<Timeval> _p0 = default!;
    error _p0 = default!;

    var (mib, err) = sysctlmib(name);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    ref Timeval tv = ref heap(out ptr<Timeval> _addr_tv);
    ref var n = ref heap(uintptr(@unsafe.Sizeof(tv)), out ptr<var> _addr_n);
    {
        var err = sysctl(mib, (byte.val)(@unsafe.Pointer(_addr_tv)), _addr_n, null, 0);

        if (err != null) {
            return (_addr_null!, error.As(err)!);
        }
    }
    if (n != @unsafe.Sizeof(tv)) {
        return (_addr_null!, error.As(EIO)!);
    }
    return (_addr__addr_tv!, error.As(null!)!);
}

//sys    utimes(path string, timeval *[2]Timeval) (err error)

public static error Utimes(@string path, slice<Timeval> tv) {
    if (tv == null) {
        return error.As(utimes(path, null))!;
    }
    if (len(tv) != 2) {
        return error.As(EINVAL)!;
    }
    return error.As(utimes(path, new ptr<ptr<array<Timeval>>>(@unsafe.Pointer(_addr_tv[0]))))!;
}

public static error UtimesNano(@string path, slice<Timespec> ts) {
    if (ts == null) {
        var err = utimensat(AT_FDCWD, path, null, 0);
        if (err != ENOSYS) {
            return error.As(err)!;
        }
        return error.As(utimes(path, null))!;
    }
    if (len(ts) != 2) {
        return error.As(EINVAL)!;
    }
    err = setattrlistTimes(path, ts, 0);
    if (err != ENOSYS) {
        return error.As(err)!;
    }
    err = utimensat(AT_FDCWD, path, new ptr<ptr<array<Timespec>>>(@unsafe.Pointer(_addr_ts[0])), 0);
    if (err != ENOSYS) {
        return error.As(err)!;
    }
    array<Timeval> tv = new array<Timeval>(new Timeval[] { NsecToTimeval(TimespecToNsec(ts[0])), NsecToTimeval(TimespecToNsec(ts[1])) });
    return error.As(utimes(path, new ptr<ptr<array<Timeval>>>(@unsafe.Pointer(_addr_tv[0]))))!;
}

public static error UtimesNanoAt(nint dirfd, @string path, slice<Timespec> ts, nint flags) {
    if (ts == null) {
        return error.As(utimensat(dirfd, path, null, flags))!;
    }
    if (len(ts) != 2) {
        return error.As(EINVAL)!;
    }
    var err = setattrlistTimes(path, ts, flags);
    if (err != ENOSYS) {
        return error.As(err)!;
    }
    return error.As(utimensat(dirfd, path, new ptr<ptr<array<Timespec>>>(@unsafe.Pointer(_addr_ts[0])), flags))!;
}

//sys    futimes(fd int, timeval *[2]Timeval) (err error)

public static error Futimes(nint fd, slice<Timeval> tv) {
    if (tv == null) {
        return error.As(futimes(fd, null))!;
    }
    if (len(tv) != 2) {
        return error.As(EINVAL)!;
    }
    return error.As(futimes(fd, new ptr<ptr<array<Timeval>>>(@unsafe.Pointer(_addr_tv[0]))))!;
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

// TODO: wrap
//    Acct(name nil-string) (err error)
//    Gethostuuid(uuid *byte, timeout *Timespec) (err error)
//    Ptrace(req int, pid int, addr uintptr, data int) (ret uintptr, err error)

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

//sys    Madvise(b []byte, behav int) (err error)
//sys    Mlock(b []byte) (err error)
//sys    Mlockall(flags int) (err error)
//sys    Mprotect(b []byte, prot int) (err error)
//sys    Msync(b []byte, flags int) (err error)
//sys    Munlock(b []byte) (err error)
//sys    Munlockall() (err error)

} // end unix_package
