// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build aix || darwin || dragonfly || freebsd || linux || netbsd || openbsd || solaris
// +build aix darwin dragonfly freebsd linux netbsd openbsd solaris

// package syscall -- go2cs converted at 2022 March 13 05:40:38 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Program Files\Go\src\syscall\syscall_unix.go
namespace go;

using itoa = @internal.itoa_package;
using oserror = @internal.oserror_package;
using race = @internal.race_package;
using unsafeheader = @internal.unsafeheader_package;
using runtime = runtime_package;
using sync = sync_package;
using @unsafe = @unsafe_package;
using System;

public static partial class syscall_package {

public static nint Stdin = 0;public static nint Stdout = 1;public static nint Stderr = 2;

private static readonly var darwin64Bit = (runtime.GOOS == "darwin" || runtime.GOOS == "ios") && sizeofPtr == 8;
private static readonly var netbsd32Bit = runtime.GOOS == "netbsd" && sizeofPtr == 4;

public static (System.UIntPtr, System.UIntPtr, Errno) Syscall(System.UIntPtr trap, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3);
public static (System.UIntPtr, System.UIntPtr, Errno) Syscall6(System.UIntPtr trap, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6);
public static (System.UIntPtr, System.UIntPtr, Errno) RawSyscall(System.UIntPtr trap, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3);
public static (System.UIntPtr, System.UIntPtr, Errno) RawSyscall6(System.UIntPtr trap, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6);

// clen returns the index of the first NULL byte in n or len(n) if n contains no NULL byte.
private static nint clen(slice<byte> n) {
    for (nint i = 0; i < len(n); i++) {>>MARKER:FUNCTION_RawSyscall6_BLOCK_PREFIX<<
        if (n[i] == 0) {>>MARKER:FUNCTION_RawSyscall_BLOCK_PREFIX<<
            return i;
        }
    }
    return len(n);
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

    if (length <= 0) {>>MARKER:FUNCTION_Syscall6_BLOCK_PREFIX<<
        return (null, error.As(EINVAL)!);
    }
    var (addr, errno) = m.mmap(0, uintptr(length), prot, flags, fd, offset);
    if (errno != null) {>>MARKER:FUNCTION_Syscall_BLOCK_PREFIX<<
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

// An Errno is an unsigned number describing an error condition.
// It implements the error interface. The zero Errno is by convention
// a non-error, so code to convert from Errno to error should use:
//    err = nil
//    if errno != 0 {
//        err = errno
//    }
//
// Errno values can be tested against error values from the os package
// using errors.Is. For example:
//
//    _, _, err := syscall.Syscall(...)
//    if errors.Is(err, fs.ErrNotExist) ...
public partial struct Errno { // : System.UIntPtr
}

public static @string Error(this Errno e) {
    if (0 <= int(e) && int(e) < len(errors)) {
        var s = errors[e];
        if (s != "") {
            return s;
        }
    }
    return "errno " + itoa.Itoa(int(e));
}

public static bool Is(this Errno e, error target) {

    if (target == oserror.ErrPermission) 
        return e == EACCES || e == EPERM;
    else if (target == oserror.ErrExist) 
        return e == EEXIST || e == ENOTEMPTY;
    else if (target == oserror.ErrNotExist) 
        return e == ENOENT;
        return false;
}

public static bool Temporary(this Errno e) {
    return e == EINTR || e == EMFILE || e == ENFILE || e.Timeout();
}

public static bool Timeout(this Errno e) {
    return e == EAGAIN || e == EWOULDBLOCK || e == ETIMEDOUT;
}

// Do the interface allocations only once for common
// Errno values.
private static error errEAGAIN = error.As(EAGAIN)!;private static error errEINVAL = error.As(EINVAL)!;private static error errENOENT = error.As(ENOENT)!;

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

// A Signal is a number describing a process signal.
// It implements the os.Signal interface.
public partial struct Signal { // : nint
}

public static void Signal(this Signal s) {
}

public static @string String(this Signal s) {
    if (0 <= s && int(s) < len(signals)) {
        var str = signals[s];
        if (str != "") {
            return str;
        }
    }
    return "signal " + itoa.Itoa(int(s));
}

public static (nint, error) Read(nint fd, slice<byte> p) {
    nint n = default;
    error err = default!;

    n, err = read(fd, p);
    if (race.Enabled) {
        if (n > 0) {
            race.WriteRange(@unsafe.Pointer(_addr_p[0]), n);
        }
        if (err == null) {
            race.Acquire(@unsafe.Pointer(_addr_ioSync));
        }
    }
    if (msanenabled && n > 0) {
        msanWrite(@unsafe.Pointer(_addr_p[0]), n);
    }
    return ;
}

public static (nint, error) Write(nint fd, slice<byte> p) {
    nint n = default;
    error err = default!;

    if (race.Enabled) {
        race.ReleaseMerge(@unsafe.Pointer(_addr_ioSync));
    }
    if (faketime && (fd == 1 || fd == 2)) {
        n = faketimeWrite(fd, p);
        if (n < 0) {
            (n, err) = (0, errnoErr(Errno(-n)));
        }
    }
    else
 {
        n, err = write(fd, p);
    }
    if (race.Enabled && n > 0) {
        race.ReadRange(@unsafe.Pointer(_addr_p[0]), n);
    }
    if (msanenabled && n > 0) {
        msanRead(@unsafe.Pointer(_addr_p[0]), n);
    }
    return ;
}

// For testing: clients can set this flag to force
// creation of IPv6 sockets to return EAFNOSUPPORT.
public static bool SocketDisableIPv6 = default;

public partial interface Sockaddr {
    (unsafe.Pointer, _Socklen, error) sockaddr(); // lowercase; only we can define Sockaddrs
}

public partial struct SockaddrInet4 {
    public nint Port;
    public array<byte> Addr;
    public RawSockaddrInet4 raw;
}

public partial struct SockaddrInet6 {
    public nint Port;
    public uint ZoneId;
    public array<byte> Addr;
    public RawSockaddrInet6 raw;
}

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
    return anyToSockaddr(_addr_rsa);
}

public static (nint, error) GetsockoptInt(nint fd, nint level, nint opt) {
    nint value = default;
    error err = default!;

    ref int n = ref heap(out ptr<int> _addr_n);
    ref var vallen = ref heap(_Socklen(4), out ptr<var> _addr_vallen);
    err = getsockopt(fd, level, opt, @unsafe.Pointer(_addr_n), _addr_vallen);
    return (int(n), error.As(err)!);
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
        from, err = anyToSockaddr(_addr_rsa);
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

public static (nint, error) Sendfile(nint outfd, nint infd, ptr<long> _addr_offset, nint count) {
    nint written = default;
    error err = default!;
    ref long offset = ref _addr_offset.val;

    if (race.Enabled) {
        race.ReleaseMerge(@unsafe.Pointer(_addr_ioSync));
    }
    return sendfile(outfd, infd, offset, count);
}

private static long ioSync = default;

} // end syscall_package
