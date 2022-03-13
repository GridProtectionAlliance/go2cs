// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build aix || darwin || dragonfly || freebsd || linux || netbsd || openbsd || solaris || windows
// +build aix darwin dragonfly freebsd linux netbsd openbsd solaris windows

// package net -- go2cs converted at 2022 March 13 05:29:44 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Program Files\Go\src\net\fd_posix.go
namespace go;

using poll = @internal.poll_package;
using runtime = runtime_package;
using syscall = syscall_package;
using time = time_package;


// Network file descriptor.

public static partial class net_package {

private partial struct netFD {
    public poll.FD pfd; // immutable until Close
    public nint family;
    public nint sotype;
    public bool isConnected; // handshake completed or use of association with peer
    public @string net;
    public Addr laddr;
    public Addr raddr;
}

private static void setAddr(this ptr<netFD> _addr_fd, Addr laddr, Addr raddr) {
    ref netFD fd = ref _addr_fd.val;

    fd.laddr = laddr;
    fd.raddr = raddr;
    runtime.SetFinalizer(fd, (netFD.val).Close);
}

private static error Close(this ptr<netFD> _addr_fd) {
    ref netFD fd = ref _addr_fd.val;

    runtime.SetFinalizer(fd, null);
    return error.As(fd.pfd.Close())!;
}

private static error shutdown(this ptr<netFD> _addr_fd, nint how) {
    ref netFD fd = ref _addr_fd.val;

    var err = fd.pfd.Shutdown(how);
    runtime.KeepAlive(fd);
    return error.As(wrapSyscallError("shutdown", err))!;
}

private static error closeRead(this ptr<netFD> _addr_fd) {
    ref netFD fd = ref _addr_fd.val;

    return error.As(fd.shutdown(syscall.SHUT_RD))!;
}

private static error closeWrite(this ptr<netFD> _addr_fd) {
    ref netFD fd = ref _addr_fd.val;

    return error.As(fd.shutdown(syscall.SHUT_WR))!;
}

private static (nint, error) Read(this ptr<netFD> _addr_fd, slice<byte> p) {
    nint n = default;
    error err = default!;
    ref netFD fd = ref _addr_fd.val;

    n, err = fd.pfd.Read(p);
    runtime.KeepAlive(fd);
    return (n, error.As(wrapSyscallError(readSyscallName, err))!);
}

private static (nint, syscall.Sockaddr, error) readFrom(this ptr<netFD> _addr_fd, slice<byte> p) {
    nint n = default;
    syscall.Sockaddr sa = default;
    error err = default!;
    ref netFD fd = ref _addr_fd.val;

    n, sa, err = fd.pfd.ReadFrom(p);
    runtime.KeepAlive(fd);
    return (n, sa, error.As(wrapSyscallError(readFromSyscallName, err))!);
}

private static (nint, nint, nint, syscall.Sockaddr, error) readMsg(this ptr<netFD> _addr_fd, slice<byte> p, slice<byte> oob, nint flags) {
    nint n = default;
    nint oobn = default;
    nint retflags = default;
    syscall.Sockaddr sa = default;
    error err = default!;
    ref netFD fd = ref _addr_fd.val;

    n, oobn, retflags, sa, err = fd.pfd.ReadMsg(p, oob, flags);
    runtime.KeepAlive(fd);
    return (n, oobn, retflags, sa, error.As(wrapSyscallError(readMsgSyscallName, err))!);
}

private static (nint, error) Write(this ptr<netFD> _addr_fd, slice<byte> p) {
    nint nn = default;
    error err = default!;
    ref netFD fd = ref _addr_fd.val;

    nn, err = fd.pfd.Write(p);
    runtime.KeepAlive(fd);
    return (nn, error.As(wrapSyscallError(writeSyscallName, err))!);
}

private static (nint, error) writeTo(this ptr<netFD> _addr_fd, slice<byte> p, syscall.Sockaddr sa) {
    nint n = default;
    error err = default!;
    ref netFD fd = ref _addr_fd.val;

    n, err = fd.pfd.WriteTo(p, sa);
    runtime.KeepAlive(fd);
    return (n, error.As(wrapSyscallError(writeToSyscallName, err))!);
}

private static (nint, nint, error) writeMsg(this ptr<netFD> _addr_fd, slice<byte> p, slice<byte> oob, syscall.Sockaddr sa) {
    nint n = default;
    nint oobn = default;
    error err = default!;
    ref netFD fd = ref _addr_fd.val;

    n, oobn, err = fd.pfd.WriteMsg(p, oob, sa);
    runtime.KeepAlive(fd);
    return (n, oobn, error.As(wrapSyscallError(writeMsgSyscallName, err))!);
}

private static error SetDeadline(this ptr<netFD> _addr_fd, time.Time t) {
    ref netFD fd = ref _addr_fd.val;

    return error.As(fd.pfd.SetDeadline(t))!;
}

private static error SetReadDeadline(this ptr<netFD> _addr_fd, time.Time t) {
    ref netFD fd = ref _addr_fd.val;

    return error.As(fd.pfd.SetReadDeadline(t))!;
}

private static error SetWriteDeadline(this ptr<netFD> _addr_fd, time.Time t) {
    ref netFD fd = ref _addr_fd.val;

    return error.As(fd.pfd.SetWriteDeadline(t))!;
}

} // end net_package
