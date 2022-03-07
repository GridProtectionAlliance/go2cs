// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package poll -- go2cs converted at 2022 March 06 22:13:17 UTC
// import "internal/poll" ==> using poll = go.@internal.poll_package
// Original source: C:\Program Files\Go\src\internal\poll\hook_windows.go
using syscall = go.syscall_package;
using System;


namespace go.@internal;

public static partial class poll_package {

    // CloseFunc is used to hook the close call.
public static Func<syscall.Handle, error> CloseFunc = syscall.Closesocket;

// AcceptFunc is used to hook the accept call.
public static Func<syscall.Handle, syscall.Handle, ptr<byte>, uint, uint, uint, ptr<uint>, ptr<syscall.Overlapped>, error> AcceptFunc = syscall.AcceptEx;

// ConnectExFunc is used to hook the ConnectEx call.
public static Func<syscall.Handle, syscall.Sockaddr, ptr<byte>, uint, ptr<uint>, ptr<syscall.Overlapped>, error> ConnectExFunc = syscall.ConnectEx;

} // end poll_package
