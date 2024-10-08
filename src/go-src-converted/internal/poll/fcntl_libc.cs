// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build aix || darwin || solaris
// +build aix darwin solaris

// package poll -- go2cs converted at 2022 March 13 05:27:49 UTC
// import "internal/poll" ==> using poll = go.@internal.poll_package
// Original source: C:\Program Files\Go\src\internal\poll\fcntl_libc.go
namespace go.@internal;

using _@unsafe_ = @unsafe_package;

public static partial class poll_package { // for go:linkname

// Implemented in the syscall package.
//go:linkname fcntl syscall.fcntl
private static (nint, error) fcntl(nint fd, nint cmd, nint arg);

} // end poll_package
