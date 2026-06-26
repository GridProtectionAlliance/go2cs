// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using os = os_package;
using runtime = runtime_package;
using syscall = syscall_package;

partial class net_package {

internal static error setIPv4MulticastInterface(ж<netFD> Ꮡfd, ж<Interface> Ꮡifi) {
    ref var fd = ref Ꮡfd.val;
    ref var ifi = ref Ꮡifi.val;

    (ip, err) = interfaceToIPv4Addr(Ꮡifi);
    if (err != default!) {
        return os.NewSyscallError("setsockopt"u8, err);
    }
    array<byte> a = new(4);
    copy(a[..], ip.To4());
    err = fd.pfd.SetsockoptInet4Addr(syscall.IPPROTO_IP, syscall.IP_MULTICAST_IF, a);
    runtime.KeepAlive(fd);
    return wrapSyscallError("setsockopt"u8, err);
}

internal static error setIPv4MulticastLoopback(ж<netFD> Ꮡfd, bool v) {
    ref var fd = ref Ꮡfd.val;

    var err = fd.pfd.SetsockoptInt(syscall.IPPROTO_IP, syscall.IP_MULTICAST_LOOP, boolint(v));
    runtime.KeepAlive(fd);
    return wrapSyscallError("setsockopt"u8, err);
}

} // end net_package
