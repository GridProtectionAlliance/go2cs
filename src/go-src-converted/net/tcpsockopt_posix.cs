// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix || windows
namespace go;

using Δruntime = runtime_package;
using syscall = syscall_package;
using @internal;

partial class net_package {

internal static error setNoDelay(ж<netFD> Ꮡfd, bool noDelay) {
    ref var fd = ref Ꮡfd.Value;

    var err = Ꮡfd.of(netFD.Ꮡpfd).SetsockoptInt(syscall.IPPROTO_TCP, syscall.TCP_NODELAY, boolint(noDelay));
    Δruntime.KeepAlive(fd);
    return wrapSyscallError("setsockopt"u8, err);
}

} // end net_package
