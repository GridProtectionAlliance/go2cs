// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package net -- go2cs converted at 2022 March 06 22:15:49 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Program Files\Go\src\net\hook_windows.go
using windows = go.@internal.syscall.windows_package;
using syscall = go.syscall_package;
using time = go.time_package;
using System;


namespace go;

public static partial class net_package {

private static Action testHookDialChannel = () => {
    time.Sleep(time.Millisecond);
};private static Func<nint, nint, nint, (syscall.Handle, error)> socketFunc = syscall.Socket;private static Func<int, int, int, ptr<syscall.WSAProtocolInfo>, uint, uint, (syscall.Handle, error)> wsaSocketFunc = windows.WSASocket;private static Func<syscall.Handle, syscall.Sockaddr, error> connectFunc = syscall.Connect;private static Func<syscall.Handle, nint, error> listenFunc = syscall.Listen;

} // end net_package
