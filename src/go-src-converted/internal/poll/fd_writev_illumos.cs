// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build illumos
// +build illumos

// package poll -- go2cs converted at 2022 March 06 22:13:17 UTC
// import "internal/poll" ==> using poll = go.@internal.poll_package
// Original source: C:\Program Files\Go\src\internal\poll\fd_writev_illumos.go
using unix = go.@internal.syscall.unix_package;
using syscall = go.syscall_package;

namespace go.@internal;

public static partial class poll_package {

private static (System.UIntPtr, error) writev(nint fd, slice<syscall.Iovec> iovecs) {
    System.UIntPtr _p0 = default;
    error _p0 = default!;

    return unix.Writev(fd, iovecs);
}

} // end poll_package
