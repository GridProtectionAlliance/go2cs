// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package poll -- go2cs converted at 2020 August 29 08:25:35 UTC
// import "internal/poll" ==> using poll = go.@internal.poll_package
// Original source: C:\Go\src\internal\poll\hook_windows.go
using syscall = go.syscall_package;
using static go.builtin;
using System;

namespace go {
namespace @internal
{
    public static partial class poll_package
    {
        // CloseFunc is used to hook the close call.
        public static Func<syscall.Handle, error> CloseFunc = syscall.Closesocket;

        // AcceptFunc is used to hook the accept call.
        public static Func<syscall.Handle, syscall.Handle, ref byte, uint, uint, uint, ref uint, ref syscall.Overlapped, error> AcceptFunc = syscall.AcceptEx;

        // ConnectExFunc is used to hook the ConnectEx call.
        public static Func<syscall.Handle, syscall.Sockaddr, ref byte, uint, ref uint, ref syscall.Overlapped, error> ConnectExFunc = syscall.ConnectEx;
    }
}}
