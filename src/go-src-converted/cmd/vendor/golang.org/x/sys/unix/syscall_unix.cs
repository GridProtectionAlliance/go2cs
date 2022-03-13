// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build aix || darwin || dragonfly || freebsd || linux || netbsd || openbsd || solaris
// +build aix darwin dragonfly freebsd linux netbsd openbsd solaris

// package unix -- go2cs converted at 2022 March 13 06:41:25 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\sys\unix\syscall_unix.go
namespace go.cmd.vendor.golang.org.x.sys;

using bytes = bytes_package;
using sort = sort_package;
using sync = sync_package;
using syscall = syscall_package;
using @unsafe = @unsafe_package;

using unsafeheader = golang.org.x.sys.@internal.unsafeheader_package;
using System;

public static partial class unix_package {

public static nint Stdin = 0;public static nint Stdout = 1;public static nint Stderr = 2;

// Do the interface allocations only once for common
// Errno values.
private static error errEAGAIN = error.As(syscall.EAGAIN)!;private static error errEINVAL = error.As(syscall.EINVAL)!;private static error errENOENT = error.As(syscall.ENOENT)!;

private static sync.Once signalNameMapOnce = default;private static map<@string, syscall.Signal> signalNameMap = default;

// errnoErr returns common boxed Errno values, to prevent
// allocations at runtime.
private static error errnoErr(syscall.Errno e) {

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
public static @string ErrnoName(syscall.Errno e) {
    var i = sort.Search(len(errorList), i => errorList[i].num >= e);
    if (i < len(errorList) && errorList[i].num == e) {
        return errorList[i].name;
    }
    return "";
}

// SignalName returns the signal name for signal number s.
public static @string SignalName(syscall.Signal s) {
    var i = sort.Search(len(signalList), i => signalList[i].num >= s);
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
    ref slice<byte> b = ref heap(out ptr<slice<byte>> _addr_b);
    var hdr = (unsafeheader.Slice.val)(@unsafe.Pointer(_addr_b));
    hdr.Data = @unsafe.Pointer(addr);
    hdr.Cap = length;
    hdr.Len = length; 

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

// Lutimes sets the access and modification times tv on path. If path refers to
// a symlink, it is not dereferenced and the timestamps are set on the symlink.
// If tv is nil, the access and modification times are set to the current time.
// Otherwise tv must contain exactly 2 elements, with access time as the first
// element and modification time as the second element.
public static error Lutimes(@string path, slice<Timeval> tv) {
    if (tv == null) {
        return error.As(UtimesNanoAt(AT_FDCWD, path, null, AT_SYMLINK_NOFOLLOW))!;
    }
    if (len(tv) != 2) {
        return error.As(EINVAL)!;
    }
    Timespec ts = new slice<Timespec>(new Timespec[] { NsecToTimespec(TimevalToNsec(tv[0])), NsecToTimespec(TimevalToNsec(tv[1])) });
    return error.As(UtimesNanoAt(AT_FDCWD, path, ts, AT_SYMLINK_NOFOLLOW))!;
}

} // end unix_package
