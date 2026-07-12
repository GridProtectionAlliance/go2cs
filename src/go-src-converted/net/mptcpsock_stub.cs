// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !linux
namespace go;

using context = context_package;

partial class net_package {

internal static (ж<TCPConn>, error) dialMPTCP(this ж<sysDialer> Ꮡsd, context.Context ctx, ж<TCPAddr> Ꮡladdr, ж<TCPAddr> Ꮡraddr) {
    return Ꮡsd.dialTCP(ctx, Ꮡladdr, Ꮡraddr);
}

internal static (ж<TCPListener>, error) listenMPTCP(this ж<sysListener> Ꮡsl, context.Context ctx, ж<TCPAddr> Ꮡladdr) {
    return Ꮡsl.listenTCP(ctx, Ꮡladdr);
}

internal static bool isUsingMultipathTCP(ж<netFD> Ꮡfd) {
    return false;
}

} // end net_package
