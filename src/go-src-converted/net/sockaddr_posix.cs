// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build aix || darwin || dragonfly || freebsd || (js && wasm) || linux || netbsd || openbsd || solaris || windows
// +build aix darwin dragonfly freebsd js,wasm linux netbsd openbsd solaris windows

// package net -- go2cs converted at 2022 March 06 22:16:32 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Program Files\Go\src\net\sockaddr_posix.go
using syscall = go.syscall_package;

namespace go;

public static partial class net_package {

    // A sockaddr represents a TCP, UDP, IP or Unix network endpoint
    // address that can be converted into a syscall.Sockaddr.
private partial interface sockaddr {
    sockaddr family(); // isWildcard reports whether the address is a wildcard
// address.
    sockaddr isWildcard(); // sockaddr returns the address converted into a syscall
// sockaddr type that implements syscall.Sockaddr
// interface. It returns a nil interface when the address is
// nil.
    sockaddr sockaddr(nint family); // toLocal maps the zero address to a local system address (127.0.0.1 or ::1)
    sockaddr toLocal(@string net);
}

} // end net_package
