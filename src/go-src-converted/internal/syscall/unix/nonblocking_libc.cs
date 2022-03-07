// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build aix || darwin || solaris
// +build aix darwin solaris

// package unix -- go2cs converted at 2022 March 06 22:12:55 UTC
// import "internal/syscall/unix" ==> using unix = go.@internal.syscall.unix_package
// Original source: C:\Program Files\Go\src\internal\syscall\unix\nonblocking_libc.go
using syscall = go.syscall_package;
using _@unsafe_ = go.@unsafe_package;

namespace go.@internal.syscall;

public static partial class unix_package {

public static (bool, error) IsNonblock(nint fd) {
    bool nonblocking = default;
    error err = default!;

    var (flag, e1) = fcntl(fd, syscall.F_GETFL, 0);
    if (e1 != null) {
        return (false, error.As(e1)!);
    }
    return (flag & syscall.O_NONBLOCK != 0, error.As(null!)!);

}

// Implemented in the syscall package.
//go:linkname fcntl syscall.fcntl
private static (nint, error) fcntl(nint fd, nint cmd, nint arg);

} // end unix_package
