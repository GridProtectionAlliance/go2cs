// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin dragonfly freebsd linux nacl netbsd openbsd solaris

// package poll -- go2cs converted at 2020 August 29 08:25:35 UTC
// import "internal/poll" ==> using poll = go.@internal.poll_package
// Original source: C:\Go\src\internal\poll\hook_unix.go
using syscall = go.syscall_package;
using static go.builtin;
using System;

namespace go {
namespace @internal
{
    public static partial class poll_package
    {
        // CloseFunc is used to hook the close call.
        public static Func<long, error> CloseFunc = syscall.Close;

        // AcceptFunc is used to hook the accept call.
        public static Func<long, (long, syscall.Sockaddr, error)> AcceptFunc = syscall.Accept;
    }
}}
