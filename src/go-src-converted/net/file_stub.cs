// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build js && wasm
// +build js,wasm

// package net -- go2cs converted at 2022 March 13 05:29:46 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Program Files\Go\src\net\file_stub.go
namespace go;

using os = os_package;
using syscall = syscall_package;

public static partial class net_package {

private static (Conn, error) fileConn(ptr<os.File> _addr_f) {
    Conn _p0 = default;
    error _p0 = default!;
    ref os.File f = ref _addr_f.val;

    return (null, error.As(syscall.ENOPROTOOPT)!);
}
private static (Listener, error) fileListener(ptr<os.File> _addr_f) {
    Listener _p0 = default;
    error _p0 = default!;
    ref os.File f = ref _addr_f.val;

    return (null, error.As(syscall.ENOPROTOOPT)!);
}
private static (PacketConn, error) filePacketConn(ptr<os.File> _addr_f) {
    PacketConn _p0 = default;
    error _p0 = default!;
    ref os.File f = ref _addr_f.val;

    return (null, error.As(syscall.ENOPROTOOPT)!);
}

} // end net_package
