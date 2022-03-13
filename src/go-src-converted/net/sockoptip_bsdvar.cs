// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build aix || darwin || dragonfly || freebsd || netbsd || openbsd || solaris
// +build aix darwin dragonfly freebsd netbsd openbsd solaris

// package net -- go2cs converted at 2022 March 13 05:30:05 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Program Files\Go\src\net\sockoptip_bsdvar.go
namespace go;

using runtime = runtime_package;
using syscall = syscall_package;

public static partial class net_package {

private static error setIPv4MulticastInterface(ptr<netFD> _addr_fd, ptr<Interface> _addr_ifi) {
    ref netFD fd = ref _addr_fd.val;
    ref Interface ifi = ref _addr_ifi.val;

    var (ip, err) = interfaceToIPv4Addr(ifi);
    if (err != null) {
        return error.As(wrapSyscallError("setsockopt", err))!;
    }
    array<byte> a = new array<byte>(4);
    copy(a[..], ip.To4());
    err = fd.pfd.SetsockoptInet4Addr(syscall.IPPROTO_IP, syscall.IP_MULTICAST_IF, a);
    runtime.KeepAlive(fd);
    return error.As(wrapSyscallError("setsockopt", err))!;
}

private static error setIPv4MulticastLoopback(ptr<netFD> _addr_fd, bool v) {
    ref netFD fd = ref _addr_fd.val;

    var err = fd.pfd.SetsockoptByte(syscall.IPPROTO_IP, syscall.IP_MULTICAST_LOOP, byte(boolint(v)));
    runtime.KeepAlive(fd);
    return error.As(wrapSyscallError("setsockopt", err))!;
}

} // end net_package
