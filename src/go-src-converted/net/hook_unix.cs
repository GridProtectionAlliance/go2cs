// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build aix || darwin || dragonfly || freebsd || (js && wasm) || linux || netbsd || openbsd || solaris
// +build aix darwin dragonfly freebsd js,wasm linux netbsd openbsd solaris

// package net -- go2cs converted at 2022 March 13 05:29:46 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Program Files\Go\src\net\hook_unix.go
namespace go;

using syscall = syscall_package;
using System;

public static partial class net_package {

private static Action testHookDialChannel = () => {
};private static Action testHookCanceledDial = () => {
};private static Func<nint, nint, nint, (nint, error)> socketFunc = syscall.Socket;private static Func<nint, syscall.Sockaddr, error> connectFunc = syscall.Connect;private static Func<nint, nint, error> listenFunc = syscall.Listen;private static Func<nint, nint, nint, (nint, error)> getsockoptIntFunc = syscall.GetsockoptInt;

} // end net_package
