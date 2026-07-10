// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix || windows
namespace go;

using poll = @internal.poll_package;
using Δruntime = runtime_package;
using syscall = syscall_package;
using time = time_package;
using @internal;

partial class net_package {

// Network file descriptor.
[GoType] partial struct netFD {
    internal poll.FD pfd;
    // immutable until Close
    internal nint family;
    internal nint sotype;
    internal bool isConnected; // handshake completed or use of association with peer
    internal @string net;
    internal ΔAddr laddr;
    internal ΔAddr raddr;
}

internal static void setAddr(this ж<netFD> Ꮡfd, ΔAddr laddr, ΔAddr raddr) {
    ref var fd = ref Ꮡfd.Value;

    fd.laddr = laddr;
    fd.raddr = raddr;
    Δruntime.SetFinalizer(fd, (Func<ж<netFD>, error>)(Close));
}

internal static error Close(this ж<netFD> Ꮡfd) {
    ref var fd = ref Ꮡfd.Value;

    Δruntime.SetFinalizer(fd, default!);
    return Ꮡfd.of(netFD.Ꮡpfd).Close();
}

internal static error shutdown(this ж<netFD> Ꮡfd, nint how) {
    ref var fd = ref Ꮡfd.Value;

    var err = Ꮡfd.of(netFD.Ꮡpfd).Shutdown(how);
    Δruntime.KeepAlive(fd);
    return wrapSyscallError("shutdown"u8, err);
}

internal static error closeRead(this ж<netFD> Ꮡfd) {
    ref var fd = ref Ꮡfd.Value;

    return Ꮡfd.shutdown(syscall.SHUT_RD);
}

internal static error closeWrite(this ж<netFD> Ꮡfd) {
    ref var fd = ref Ꮡfd.Value;

    return Ꮡfd.shutdown(syscall.SHUT_WR);
}

internal static (nint n, error err) Read(this ж<netFD> Ꮡfd, slice<byte> p) {
    nint n = default!;
    error err = default!;

    ref var fd = ref Ꮡfd.Value;
    (n, err) = Ꮡfd.of(netFD.Ꮡpfd).Read(p);
    Δruntime.KeepAlive(fd);
    return (n, wrapSyscallError(readSyscallName, err));
}

internal static (nint n, syscallꓸSockaddr sa, error err) readFrom(this ж<netFD> Ꮡfd, slice<byte> p) {
    nint n = default!;
    syscallꓸSockaddr sa = default!;
    error err = default!;

    ref var fd = ref Ꮡfd.Value;
    (n, sa, err) = Ꮡfd.of(netFD.Ꮡpfd).ReadFrom(p);
    Δruntime.KeepAlive(fd);
    return (n, sa, wrapSyscallError(readFromSyscallName, err));
}

internal static (nint n, error err) readFromInet4(this ж<netFD> Ꮡfd, slice<byte> p, ж<syscall.SockaddrInet4> Ꮡfrom) {
    nint n = default!;
    error err = default!;

    ref var fd = ref Ꮡfd.Value;
    ref var from = ref Ꮡfrom.Value;
    (n, err) = Ꮡfd.of(netFD.Ꮡpfd).ReadFromInet4(p, Ꮡfrom);
    Δruntime.KeepAlive(fd);
    return (n, wrapSyscallError(readFromSyscallName, err));
}

internal static (nint n, error err) readFromInet6(this ж<netFD> Ꮡfd, slice<byte> p, ж<syscall.SockaddrInet6> Ꮡfrom) {
    nint n = default!;
    error err = default!;

    ref var fd = ref Ꮡfd.Value;
    ref var from = ref Ꮡfrom.Value;
    (n, err) = Ꮡfd.of(netFD.Ꮡpfd).ReadFromInet6(p, Ꮡfrom);
    Δruntime.KeepAlive(fd);
    return (n, wrapSyscallError(readFromSyscallName, err));
}

internal static (nint n, nint oobn, nint retflags, syscallꓸSockaddr sa, error err) readMsg(this ж<netFD> Ꮡfd, slice<byte> p, slice<byte> oob, nint flags) {
    nint n = default!;
    nint oobn = default!;
    nint retflags = default!;
    syscallꓸSockaddr sa = default!;
    error err = default!;

    ref var fd = ref Ꮡfd.Value;
    (n, oobn, retflags, sa, err) = Ꮡfd.of(netFD.Ꮡpfd).ReadMsg(p, oob, flags);
    Δruntime.KeepAlive(fd);
    return (n, oobn, retflags, sa, wrapSyscallError(readMsgSyscallName, err));
}

internal static (nint n, nint oobn, nint retflags, error err) readMsgInet4(this ж<netFD> Ꮡfd, slice<byte> p, slice<byte> oob, nint flags, ж<syscall.SockaddrInet4> Ꮡsa) {
    nint n = default!;
    nint oobn = default!;
    nint retflags = default!;
    error err = default!;

    ref var fd = ref Ꮡfd.Value;
    ref var sa = ref Ꮡsa.Value;
    (n, oobn, retflags, err) = Ꮡfd.of(netFD.Ꮡpfd).ReadMsgInet4(p, oob, flags, Ꮡsa);
    Δruntime.KeepAlive(fd);
    return (n, oobn, retflags, wrapSyscallError(readMsgSyscallName, err));
}

internal static (nint n, nint oobn, nint retflags, error err) readMsgInet6(this ж<netFD> Ꮡfd, slice<byte> p, slice<byte> oob, nint flags, ж<syscall.SockaddrInet6> Ꮡsa) {
    nint n = default!;
    nint oobn = default!;
    nint retflags = default!;
    error err = default!;

    ref var fd = ref Ꮡfd.Value;
    ref var sa = ref Ꮡsa.Value;
    (n, oobn, retflags, err) = Ꮡfd.of(netFD.Ꮡpfd).ReadMsgInet6(p, oob, flags, Ꮡsa);
    Δruntime.KeepAlive(fd);
    return (n, oobn, retflags, wrapSyscallError(readMsgSyscallName, err));
}

internal static (nint nn, error err) Write(this ж<netFD> Ꮡfd, slice<byte> p) {
    nint nn = default!;
    error err = default!;

    ref var fd = ref Ꮡfd.Value;
    (nn, err) = Ꮡfd.of(netFD.Ꮡpfd).Write(p);
    Δruntime.KeepAlive(fd);
    return (nn, wrapSyscallError(writeSyscallName, err));
}

internal static (nint n, error err) writeTo(this ж<netFD> Ꮡfd, slice<byte> p, syscallꓸSockaddr sa) {
    nint n = default!;
    error err = default!;

    ref var fd = ref Ꮡfd.Value;
    (n, err) = Ꮡfd.of(netFD.Ꮡpfd).WriteTo(p, sa);
    Δruntime.KeepAlive(fd);
    return (n, wrapSyscallError(writeToSyscallName, err));
}

internal static (nint n, error err) writeToInet4(this ж<netFD> Ꮡfd, slice<byte> p, ж<syscall.SockaddrInet4> Ꮡsa) {
    nint n = default!;
    error err = default!;

    ref var fd = ref Ꮡfd.Value;
    ref var sa = ref Ꮡsa.Value;
    (n, err) = Ꮡfd.of(netFD.Ꮡpfd).WriteToInet4(p, Ꮡsa);
    Δruntime.KeepAlive(fd);
    return (n, wrapSyscallError(writeToSyscallName, err));
}

internal static (nint n, error err) writeToInet6(this ж<netFD> Ꮡfd, slice<byte> p, ж<syscall.SockaddrInet6> Ꮡsa) {
    nint n = default!;
    error err = default!;

    ref var fd = ref Ꮡfd.Value;
    ref var sa = ref Ꮡsa.Value;
    (n, err) = Ꮡfd.of(netFD.Ꮡpfd).WriteToInet6(p, Ꮡsa);
    Δruntime.KeepAlive(fd);
    return (n, wrapSyscallError(writeToSyscallName, err));
}

internal static (nint n, nint oobn, error err) writeMsg(this ж<netFD> Ꮡfd, slice<byte> p, slice<byte> oob, syscallꓸSockaddr sa) {
    nint n = default!;
    nint oobn = default!;
    error err = default!;

    ref var fd = ref Ꮡfd.Value;
    (n, oobn, err) = Ꮡfd.of(netFD.Ꮡpfd).WriteMsg(p, oob, sa);
    Δruntime.KeepAlive(fd);
    return (n, oobn, wrapSyscallError(writeMsgSyscallName, err));
}

internal static (nint n, nint oobn, error err) writeMsgInet4(this ж<netFD> Ꮡfd, slice<byte> p, slice<byte> oob, ж<syscall.SockaddrInet4> Ꮡsa) {
    nint n = default!;
    nint oobn = default!;
    error err = default!;

    ref var fd = ref Ꮡfd.Value;
    ref var sa = ref Ꮡsa.Value;
    (n, oobn, err) = Ꮡfd.of(netFD.Ꮡpfd).WriteMsgInet4(p, oob, Ꮡsa);
    Δruntime.KeepAlive(fd);
    return (n, oobn, wrapSyscallError(writeMsgSyscallName, err));
}

internal static (nint n, nint oobn, error err) writeMsgInet6(this ж<netFD> Ꮡfd, slice<byte> p, slice<byte> oob, ж<syscall.SockaddrInet6> Ꮡsa) {
    nint n = default!;
    nint oobn = default!;
    error err = default!;

    ref var fd = ref Ꮡfd.Value;
    ref var sa = ref Ꮡsa.Value;
    (n, oobn, err) = Ꮡfd.of(netFD.Ꮡpfd).WriteMsgInet6(p, oob, Ꮡsa);
    Δruntime.KeepAlive(fd);
    return (n, oobn, wrapSyscallError(writeMsgSyscallName, err));
}

internal static error SetDeadline(this ж<netFD> Ꮡfd, time.Time t) {
    ref var fd = ref Ꮡfd.Value;

    return Ꮡfd.of(netFD.Ꮡpfd).SetDeadline(t);
}

internal static error SetReadDeadline(this ж<netFD> Ꮡfd, time.Time t) {
    ref var fd = ref Ꮡfd.Value;

    return Ꮡfd.of(netFD.Ꮡpfd).SetReadDeadline(t);
}

internal static error SetWriteDeadline(this ж<netFD> Ꮡfd, time.Time t) {
    ref var fd = ref Ꮡfd.Value;

    return Ꮡfd.of(netFD.Ꮡpfd).SetWriteDeadline(t);
}

} // end net_package
