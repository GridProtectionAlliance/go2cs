// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !linux
namespace go;

using context = context_package;

partial class net_package {

internal static (ж<TCPConn>, error) dialMPTCP(this ж<sysDialer> Ꮡsd, context.Context ctx, ж<TCPAddr> Ꮡladdr, ж<TCPAddr> Ꮡraddr) {
    ref var sd = ref Ꮡsd.Value;
    ref var laddr = ref Ꮡladdr.Value;
    ref var raddr = ref Ꮡraddr.Value;

    return Ꮡsd.dialTCP(ctx, Ꮡladdr, Ꮡraddr);
}

internal static (ж<TCPListener>, error) listenMPTCP(this ж<sysListener> Ꮡsl, context.Context ctx, ж<TCPAddr> Ꮡladdr) {
    ref var sl = ref Ꮡsl.Value;
    ref var laddr = ref Ꮡladdr.Value;

    return Ꮡsl.listenTCP(ctx, Ꮡladdr);
}

internal static bool isUsingMultipathTCP(ж<netFD> Ꮡfd) {
    ref var fd = ref Ꮡfd.Value;

    return false;
}

} // end net_package
