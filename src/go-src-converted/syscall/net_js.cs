// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// js/wasm uses fake networking directly implemented in the net package.
// This file only exists to make the compiler happy.

//go:build js && wasm
// +build js,wasm

// package syscall -- go2cs converted at 2022 March 06 22:26:40 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Program Files\Go\src\syscall\net_js.go


namespace go;

public static partial class syscall_package {

public static readonly var AF_UNSPEC = iota;
public static readonly var AF_UNIX = 0;
public static readonly var AF_INET = 1;
public static readonly var AF_INET6 = 2;


public static readonly nint SOCK_STREAM = 1 + iota;
public static readonly var SOCK_DGRAM = 0;
public static readonly var SOCK_RAW = 1;
public static readonly var SOCK_SEQPACKET = 2;


public static readonly nint IPPROTO_IP = 0;
public static readonly nint IPPROTO_IPV4 = 4;
public static readonly nuint IPPROTO_IPV6 = 0x29;
public static readonly nint IPPROTO_TCP = 6;
public static readonly nuint IPPROTO_UDP = 0x11;


private static readonly var _ = iota;
public static readonly var IPV6_V6ONLY = 0;
public static readonly var SOMAXCONN = 1;
public static readonly var SO_ERROR = 2;


// Misc constants expected by package net but not supported.
private static readonly var _ = iota;
public static readonly SYS_FCNTL F_DUPFD_CLOEXEC = 500; // unsupported

public partial interface Sockaddr {
}

public partial struct SockaddrInet4 {
    public nint Port;
    public array<byte> Addr;
}

public partial struct SockaddrInet6 {
    public nint Port;
    public uint ZoneId;
    public array<byte> Addr;
}

public partial struct SockaddrUnix {
    public @string Name;
}

public static (nint, error) Socket(nint proto, nint sotype, nint unused) {
    nint fd = default;
    error err = default!;

    return (0, error.As(ENOSYS)!);
}

public static error Bind(nint fd, Sockaddr sa) {
    return error.As(ENOSYS)!;
}

public static error StopIO(nint fd) {
    return error.As(ENOSYS)!;
}

public static error Listen(nint fd, nint backlog) {
    return error.As(ENOSYS)!;
}

public static (nint, Sockaddr, error) Accept(nint fd) {
    nint newfd = default;
    Sockaddr sa = default;
    error err = default!;

    return (0, null, error.As(ENOSYS)!);
}

public static error Connect(nint fd, Sockaddr sa) {
    return error.As(ENOSYS)!;
}

public static (nint, Sockaddr, error) Recvfrom(nint fd, slice<byte> p, nint flags) {
    nint n = default;
    Sockaddr from = default;
    error err = default!;

    return (0, null, error.As(ENOSYS)!);
}

public static error Sendto(nint fd, slice<byte> p, nint flags, Sockaddr to) {
    return error.As(ENOSYS)!;
}

public static (nint, nint, nint, Sockaddr, error) Recvmsg(nint fd, slice<byte> p, slice<byte> oob, nint flags) {
    nint n = default;
    nint oobn = default;
    nint recvflags = default;
    Sockaddr from = default;
    error err = default!;

    return (0, 0, 0, null, error.As(ENOSYS)!);
}

public static (nint, error) SendmsgN(nint fd, slice<byte> p, slice<byte> oob, Sockaddr to, nint flags) {
    nint n = default;
    error err = default!;

    return (0, error.As(ENOSYS)!);
}

public static (nint, error) GetsockoptInt(nint fd, nint level, nint opt) {
    nint value = default;
    error err = default!;

    return (0, error.As(ENOSYS)!);
}

public static error SetsockoptInt(nint fd, nint level, nint opt, nint value) {
    return error.As(null!)!;
}

public static error SetReadDeadline(nint fd, long t) {
    return error.As(ENOSYS)!;
}

public static error SetWriteDeadline(nint fd, long t) {
    return error.As(ENOSYS)!;
}

public static error Shutdown(nint fd, nint how) {
    return error.As(ENOSYS)!;
}

public static error SetNonblock(nint fd, bool nonblocking) {
    return error.As(null!)!;
}

} // end syscall_package
