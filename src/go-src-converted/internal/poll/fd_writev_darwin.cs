// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build darwin
// +build darwin

// package poll -- go2cs converted at 2022 March 06 22:13:16 UTC
// import "internal/poll" ==> using poll = go.@internal.poll_package
// Original source: C:\Program Files\Go\src\internal\poll\fd_writev_darwin.go
using syscall = go.syscall_package;
using _@unsafe_ = go.@unsafe_package;

namespace go.@internal;

public static partial class poll_package {

    // Implemented in syscall/syscall_darwin.go.
    //go:linkname writev syscall.writev
private static (System.UIntPtr, error) writev(nint fd, slice<syscall.Iovec> iovecs);

} // end poll_package
