// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build windows
// +build windows

// package poll -- go2cs converted at 2022 March 06 22:12:56 UTC
// import "internal/poll" ==> using poll = go.@internal.poll_package
// Original source: C:\Program Files\Go\src\internal\poll\errno_windows.go
using syscall = go.syscall_package;

namespace go.@internal;

public static partial class poll_package {

    // Do the interface allocations only once for common
    // Errno values.
private static error errERROR_IO_PENDING = error.As(syscall.Errno(syscall.ERROR_IO_PENDING))!;

// ErrnoErr returns common boxed Errno values, to prevent
// allocations at runtime.
private static error errnoErr(syscall.Errno e) {

    if (e == 0) 
        return error.As(null!)!;
    else if (e == syscall.ERROR_IO_PENDING) 
        return error.As(errERROR_IO_PENDING)!;
    // TODO: add more here, after collecting data on the common
    // error values see on Windows. (perhaps when running
    // all.bat?)
    return error.As(e)!;

}

} // end poll_package
