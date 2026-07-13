// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix || windows
namespace go;

using Δruntime = runtime_package;
using syscall = syscall_package;
using @internal;

partial class net_package {

internal static error joinIPv4Group(ж<netFD> Ꮡfd, ж<Interface> Ꮡifi, IP ip) {
    var mreq = Ꮡ(new syscall.IPMreq(Multiaddr: new byte[]{ip[0], ip[1], ip[2], ip[3]}.array()));
    {
        var errΔ1 = setIPv4MreqToInterface(mreq, Ꮡifi); if (errΔ1 != default!) {
            return errΔ1;
        }
    }
    var err = Ꮡfd.of(netFD.Ꮡpfd).SetsockoptIPMreq(syscall.IPPROTO_IP, syscall.IP_ADD_MEMBERSHIP, mreq);
    Δruntime.KeepAlive(Ꮡfd);
    return wrapSyscallError("setsockopt"u8, err);
}

internal static error setIPv6MulticastInterface(ж<netFD> Ꮡfd, ж<Interface> Ꮡifi) {
    ref var ifi = ref Ꮡifi.DerefOrNil();

    nint v = default!;
    if (Ꮡifi != nil) {
        v = ifi.Index;
    }
    var err = Ꮡfd.of(netFD.Ꮡpfd).SetsockoptInt(syscall.IPPROTO_IPV6, syscall.IPV6_MULTICAST_IF, v);
    Δruntime.KeepAlive(Ꮡfd);
    return wrapSyscallError("setsockopt"u8, err);
}

internal static error setIPv6MulticastLoopback(ж<netFD> Ꮡfd, bool v) {
    var err = Ꮡfd.of(netFD.Ꮡpfd).SetsockoptInt(syscall.IPPROTO_IPV6, syscall.IPV6_MULTICAST_LOOP, boolint(v));
    Δruntime.KeepAlive(Ꮡfd);
    return wrapSyscallError("setsockopt"u8, err);
}

internal static error joinIPv6Group(ж<netFD> Ꮡfd, ж<Interface> Ꮡifi, IP ip) {
    ref var ifi = ref Ꮡifi.DerefOrNil();

    var mreq = Ꮡ(new syscall.IPv6Mreq(nil));
    copy((~mreq).Multiaddr[..], ip);
    if (Ꮡifi != nil) {
        mreq.Value.Interface = (uint32)ifi.Index;
    }
    var err = Ꮡfd.of(netFD.Ꮡpfd).SetsockoptIPv6Mreq(syscall.IPPROTO_IPV6, syscall.IPV6_JOIN_GROUP, mreq);
    Δruntime.KeepAlive(Ꮡfd);
    return wrapSyscallError("setsockopt"u8, err);
}

} // end net_package
