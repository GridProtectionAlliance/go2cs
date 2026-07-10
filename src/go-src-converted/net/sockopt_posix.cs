// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix || windows
namespace go;

using bytealg = @internal.bytealg_package;
using Δruntime = runtime_package;
using syscall = syscall_package;
using @internal;

partial class net_package {

// Boolean to int.
internal static nint boolint(bool b) {
    if (b) {
        return 1;
    }
    return 0;
}

internal static (IP, error) interfaceToIPv4Addr(ж<Interface> Ꮡifi) {
    ref var ifi = ref Ꮡifi.DerefOrNil();

    if (Ꮡifi == nil) {
        return (IPv4zero, default!);
    }
    var (ifat, err) = Ꮡifi.Addrs();
    if (err != default!) {
        return (default!, err);
    }
    foreach (var (_, ifa) in ifat) {
        switch (ifa.type()) {
        case ж<IPAddr> v: {
            if ((~v).IP.To4() != default!) {
                return ((~v).IP, default!);
            }
            break;
        }
        case ж<IPNet> v: {
            if ((~v).IP.To4() != default!) {
                return ((~v).IP, default!);
            }
            break;
        }}
    }
    return (default!, errNoSuchInterface);
}

internal static error setIPv4MreqToInterface(ж<syscall.IPMreq> Ꮡmreq, ж<Interface> Ꮡifi) {
    ref var mreq = ref Ꮡmreq.Value;
    ref var ifi = ref Ꮡifi.DerefOrNil();

    if (Ꮡifi == nil) {
        return default!;
    }
    var (ifat, err) = Ꮡifi.Addrs();
    if (err != default!) {
        return err;
    }
    foreach (var (_, ifa) in ifat) {
        switch (ifa.type()) {
        case ж<IPAddr> v: {
            {
                var a = (~v).IP.To4(); if (a != default!) {
                    copy(mreq.Interface[..], a);
                    goto done;
                }
            }
            break;
        }
        case ж<IPNet> v: {
            {
                var a = (~v).IP.To4(); if (a != default!) {
                    copy(mreq.Interface[..], a);
                    goto done;
                }
            }
            break;
        }}
    }
done:
    if (bytealg.Equal(mreq.Multiaddr[..], IPv4zero.To4())) {
        return errNoSuchMulticastInterface;
    }
    return default!;
}

internal static error setReadBuffer(ж<netFD> Ꮡfd, nint bytes) {
    ref var fd = ref Ꮡfd.Value;

    var err = Ꮡfd.of(netFD.Ꮡpfd).SetsockoptInt(syscall.SOL_SOCKET, syscall.SO_RCVBUF, bytes);
    Δruntime.KeepAlive(fd);
    return wrapSyscallError("setsockopt"u8, err);
}

internal static error setWriteBuffer(ж<netFD> Ꮡfd, nint bytes) {
    ref var fd = ref Ꮡfd.Value;

    var err = Ꮡfd.of(netFD.Ꮡpfd).SetsockoptInt(syscall.SOL_SOCKET, syscall.SO_SNDBUF, bytes);
    Δruntime.KeepAlive(fd);
    return wrapSyscallError("setsockopt"u8, err);
}

internal static error setKeepAlive(ж<netFD> Ꮡfd, bool keepalive) {
    ref var fd = ref Ꮡfd.Value;

    var err = Ꮡfd.of(netFD.Ꮡpfd).SetsockoptInt(syscall.SOL_SOCKET, syscall.SO_KEEPALIVE, boolint(keepalive));
    Δruntime.KeepAlive(fd);
    return wrapSyscallError("setsockopt"u8, err);
}

internal static error setLinger(ж<netFD> Ꮡfd, nint sec) {
    ref var fd = ref Ꮡfd.Value;

    ref var l = ref heap(new syscall.Linger(), out var Ꮡl);
    if (sec >= 0){
        l.Onoff = 1;
        l.ΔLinger = (int32)sec;
    } else {
        l.Onoff = 0;
        l.ΔLinger = 0;
    }
    var err = Ꮡfd.of(netFD.Ꮡpfd).SetsockoptLinger(syscall.SOL_SOCKET, syscall.SO_LINGER, Ꮡl);
    Δruntime.KeepAlive(fd);
    return wrapSyscallError("setsockopt"u8, err);
}

} // end net_package
