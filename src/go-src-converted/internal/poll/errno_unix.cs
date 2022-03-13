// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build aix || darwin || dragonfly || freebsd || linux || netbsd || openbsd || solaris
// +build aix darwin dragonfly freebsd linux netbsd openbsd solaris

// package poll -- go2cs converted at 2022 March 13 05:27:49 UTC
// import "internal/poll" ==> using poll = go.@internal.poll_package
// Original source: C:\Program Files\Go\src\internal\poll\errno_unix.go
namespace go.@internal;

using syscall = syscall_package;

public static partial class poll_package {

// Do the interface allocations only once for common
// Errno values.
private static error errEAGAIN = error.As(syscall.EAGAIN)!;private static error errEINVAL = error.As(syscall.EINVAL)!;private static error errENOENT = error.As(syscall.ENOENT)!;

// errnoErr returns common boxed Errno values, to prevent
// allocations at runtime.
private static error errnoErr(syscall.Errno e) {

    if (e == 0) 
        return error.As(null!)!;
    else if (e == syscall.EAGAIN) 
        return error.As(errEAGAIN)!;
    else if (e == syscall.EINVAL) 
        return error.As(errEINVAL)!;
    else if (e == syscall.ENOENT) 
        return error.As(errENOENT)!;
        return error.As(e)!;
}

} // end poll_package
