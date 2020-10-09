// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build aix darwin dragonfly freebsd linux netbsd openbsd solaris

// package poll -- go2cs converted at 2020 October 09 04:51:00 UTC
// import "internal/poll" ==> using poll = go.@internal.poll_package
// Original source: C:\Go\src\internal\poll\errno_unix.go
using syscall = go.syscall_package;
using static go.builtin;

namespace go {
namespace @internal
{
    public static partial class poll_package
    {
        // Do the interface allocations only once for common
        // Errno values.
        private static error errEAGAIN = error.As(syscall.EAGAIN)!;        private static error errEINVAL = error.As(syscall.EINVAL)!;        private static error errENOENT = error.As(syscall.ENOENT)!;

        // errnoErr returns common boxed Errno values, to prevent
        // allocations at runtime.
        private static error errnoErr(syscall.Errno e)
        {

            if (e == 0L) 
                return error.As(null!)!;
            else if (e == syscall.EAGAIN) 
                return error.As(errEAGAIN)!;
            else if (e == syscall.EINVAL) 
                return error.As(errEINVAL)!;
            else if (e == syscall.ENOENT) 
                return error.As(errENOENT)!;
                        return error.As(e)!;

        }
    }
}}
