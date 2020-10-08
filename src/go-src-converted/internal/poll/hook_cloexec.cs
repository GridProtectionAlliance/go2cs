// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build dragonfly freebsd linux netbsd openbsd

// package poll -- go2cs converted at 2020 October 08 03:32:39 UTC
// import "internal/poll" ==> using poll = go.@internal.poll_package
// Original source: C:\Go\src\internal\poll\hook_cloexec.go
using syscall = go.syscall_package;
using static go.builtin;
using System;

namespace go {
namespace @internal
{
    public static partial class poll_package
    {
        // Accept4Func is used to hook the accept4 call.
        public static Func<long, long, (long, syscall.Sockaddr, error)> Accept4Func = syscall.Accept4;
    }
}}
