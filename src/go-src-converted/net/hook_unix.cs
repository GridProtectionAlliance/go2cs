// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build aix darwin dragonfly freebsd js,wasm linux netbsd openbsd solaris

// package net -- go2cs converted at 2020 October 08 03:33:07 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\hook_unix.go
using syscall = go.syscall_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class net_package
    {
        private static Action testHookDialChannel = () =>
        {
        };        private static Action testHookCanceledDial = () =>
        {
        };        private static Func<long, long, long, (long, error)> socketFunc = syscall.Socket;        private static Func<long, syscall.Sockaddr, error> connectFunc = syscall.Connect;        private static Func<long, long, error> listenFunc = syscall.Listen;        private static Func<long, long, long, (long, error)> getsockoptIntFunc = syscall.GetsockoptInt;
    }
}
