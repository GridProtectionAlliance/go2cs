// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build js && wasm
// +build js,wasm

// package net -- go2cs converted at 2022 March 13 05:30:05 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Program Files\Go\src\net\sockoptip_stub.go
namespace go;

using syscall = syscall_package;

public static partial class net_package {

private static error setIPv4MulticastInterface(ptr<netFD> _addr_fd, ptr<Interface> _addr_ifi) {
    ref netFD fd = ref _addr_fd.val;
    ref Interface ifi = ref _addr_ifi.val;
 
    // See golang.org/issue/7399.
    return error.As(syscall.ENOPROTOOPT)!;
}

private static error setIPv4MulticastLoopback(ptr<netFD> _addr_fd, bool v) {
    ref netFD fd = ref _addr_fd.val;
 
    // See golang.org/issue/7399.
    return error.As(syscall.ENOPROTOOPT)!;
}

private static error joinIPv4Group(ptr<netFD> _addr_fd, ptr<Interface> _addr_ifi, IP ip) {
    ref netFD fd = ref _addr_fd.val;
    ref Interface ifi = ref _addr_ifi.val;
 
    // See golang.org/issue/7399.
    return error.As(syscall.ENOPROTOOPT)!;
}

private static error setIPv6MulticastInterface(ptr<netFD> _addr_fd, ptr<Interface> _addr_ifi) {
    ref netFD fd = ref _addr_fd.val;
    ref Interface ifi = ref _addr_ifi.val;
 
    // See golang.org/issue/7399.
    return error.As(syscall.ENOPROTOOPT)!;
}

private static error setIPv6MulticastLoopback(ptr<netFD> _addr_fd, bool v) {
    ref netFD fd = ref _addr_fd.val;
 
    // See golang.org/issue/7399.
    return error.As(syscall.ENOPROTOOPT)!;
}

private static error joinIPv6Group(ptr<netFD> _addr_fd, ptr<Interface> _addr_ifi, IP ip) {
    ref netFD fd = ref _addr_fd.val;
    ref Interface ifi = ref _addr_ifi.val;
 
    // See golang.org/issue/7399.
    return error.As(syscall.ENOPROTOOPT)!;
}

} // end net_package
