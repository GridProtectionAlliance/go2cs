// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build aix || darwin || dragonfly || freebsd || linux || netbsd || openbsd || solaris || windows
// +build aix darwin dragonfly freebsd linux netbsd openbsd solaris windows

// package net -- go2cs converted at 2022 March 13 05:30:05 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Program Files\Go\src\net\sockoptip_posix.go
namespace go;

using runtime = runtime_package;
using syscall = syscall_package;

public static partial class net_package {

private static error joinIPv4Group(ptr<netFD> _addr_fd, ptr<Interface> _addr_ifi, IP ip) {
    ref netFD fd = ref _addr_fd.val;
    ref Interface ifi = ref _addr_ifi.val;

    ptr<syscall.IPMreq> mreq = addr(new syscall.IPMreq(Multiaddr:[4]byte{ip[0],ip[1],ip[2],ip[3]}));
    {
        var err__prev1 = err;

        var err = setIPv4MreqToInterface(mreq, ifi);

        if (err != null) {
            return error.As(err)!;
        }
        err = err__prev1;

    }
    err = fd.pfd.SetsockoptIPMreq(syscall.IPPROTO_IP, syscall.IP_ADD_MEMBERSHIP, mreq);
    runtime.KeepAlive(fd);
    return error.As(wrapSyscallError("setsockopt", err))!;
}

private static error setIPv6MulticastInterface(ptr<netFD> _addr_fd, ptr<Interface> _addr_ifi) {
    ref netFD fd = ref _addr_fd.val;
    ref Interface ifi = ref _addr_ifi.val;

    nint v = default;
    if (ifi != null) {
        v = ifi.Index;
    }
    var err = fd.pfd.SetsockoptInt(syscall.IPPROTO_IPV6, syscall.IPV6_MULTICAST_IF, v);
    runtime.KeepAlive(fd);
    return error.As(wrapSyscallError("setsockopt", err))!;
}

private static error setIPv6MulticastLoopback(ptr<netFD> _addr_fd, bool v) {
    ref netFD fd = ref _addr_fd.val;

    var err = fd.pfd.SetsockoptInt(syscall.IPPROTO_IPV6, syscall.IPV6_MULTICAST_LOOP, boolint(v));
    runtime.KeepAlive(fd);
    return error.As(wrapSyscallError("setsockopt", err))!;
}

private static error joinIPv6Group(ptr<netFD> _addr_fd, ptr<Interface> _addr_ifi, IP ip) {
    ref netFD fd = ref _addr_fd.val;
    ref Interface ifi = ref _addr_ifi.val;

    ptr<syscall.IPv6Mreq> mreq = addr(new syscall.IPv6Mreq());
    copy(mreq.Multiaddr[..], ip);
    if (ifi != null) {
        mreq.Interface = uint32(ifi.Index);
    }
    var err = fd.pfd.SetsockoptIPv6Mreq(syscall.IPPROTO_IPV6, syscall.IPV6_JOIN_GROUP, mreq);
    runtime.KeepAlive(fd);
    return error.As(wrapSyscallError("setsockopt", err))!;
}

} // end net_package
