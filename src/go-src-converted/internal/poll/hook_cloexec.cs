// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build dragonfly || freebsd || illumos || linux || netbsd || openbsd
// +build dragonfly freebsd illumos linux netbsd openbsd

// package poll -- go2cs converted at 2022 March 06 22:13:17 UTC
// import "internal/poll" ==> using poll = go.@internal.poll_package
// Original source: C:\Program Files\Go\src\internal\poll\hook_cloexec.go
using syscall = go.syscall_package;
using System;


namespace go.@internal;

public static partial class poll_package {

    // Accept4Func is used to hook the accept4 call.
public static Func<nint, nint, (nint, syscall.Sockaddr, error)> Accept4Func = syscall.Accept4;

} // end poll_package
