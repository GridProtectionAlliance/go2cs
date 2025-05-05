// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using os = os_package;
using syscall = syscall_package;

partial class net_package {

internal static (Conn, error) fileConn(ж<os.File> Ꮡf) {
    ref var f = ref Ꮡf.val;

    // TODO: Implement this
    return (default!, syscall.EWINDOWS);
}

internal static (Listener, error) fileListener(ж<os.File> Ꮡf) {
    ref var f = ref Ꮡf.val;

    // TODO: Implement this
    return (default!, syscall.EWINDOWS);
}

internal static (PacketConn, error) filePacketConn(ж<os.File> Ꮡf) {
    ref var f = ref Ꮡf.val;

    // TODO: Implement this
    return (default!, syscall.EWINDOWS);
}

} // end net_package
