// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build js && wasm
// +build js,wasm

// package net -- go2cs converted at 2022 March 13 05:30:06 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Program Files\Go\src\net\sockopt_stub.go
namespace go;

using syscall = syscall_package;

public static partial class net_package {

private static error setDefaultSockopts(nint s, nint family, nint sotype, bool ipv6only) {
    return error.As(null!)!;
}

private static error setDefaultListenerSockopts(nint s) {
    return error.As(null!)!;
}

private static error setDefaultMulticastSockopts(nint s) {
    return error.As(null!)!;
}

private static error setReadBuffer(ptr<netFD> _addr_fd, nint bytes) {
    ref netFD fd = ref _addr_fd.val;

    return error.As(syscall.ENOPROTOOPT)!;
}

private static error setWriteBuffer(ptr<netFD> _addr_fd, nint bytes) {
    ref netFD fd = ref _addr_fd.val;

    return error.As(syscall.ENOPROTOOPT)!;
}

private static error setKeepAlive(ptr<netFD> _addr_fd, bool keepalive) {
    ref netFD fd = ref _addr_fd.val;

    return error.As(syscall.ENOPROTOOPT)!;
}

private static error setLinger(ptr<netFD> _addr_fd, nint sec) {
    ref netFD fd = ref _addr_fd.val;

    return error.As(syscall.ENOPROTOOPT)!;
}

} // end net_package
