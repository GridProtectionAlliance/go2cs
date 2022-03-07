// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build aix || darwin || dragonfly || freebsd || (js && wasm) || linux || netbsd || openbsd || solaris
// +build aix darwin dragonfly freebsd js,wasm linux netbsd openbsd solaris

// package poll -- go2cs converted at 2022 March 06 22:13:17 UTC
// import "internal/poll" ==> using poll = go.@internal.poll_package
// Original source: C:\Program Files\Go\src\internal\poll\hook_unix.go
using syscall = go.syscall_package;
using System;


namespace go.@internal;

public static partial class poll_package {

    // CloseFunc is used to hook the close call.
public static Func<nint, error> CloseFunc = syscall.Close;

// AcceptFunc is used to hook the accept call.
public static Func<nint, (nint, syscall.Sockaddr, error)> AcceptFunc = syscall.Accept;

} // end poll_package
