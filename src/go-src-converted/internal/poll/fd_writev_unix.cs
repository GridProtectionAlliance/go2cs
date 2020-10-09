// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build dragonfly freebsd linux netbsd openbsd

// package poll -- go2cs converted at 2020 October 09 04:51:19 UTC
// import "internal/poll" ==> using poll = go.@internal.poll_package
// Original source: C:\Go\src\internal\poll\fd_writev_unix.go
using syscall = go.syscall_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go {
namespace @internal
{
    public static partial class poll_package
    {
        private static (System.UIntPtr, error) writev(long fd, slice<syscall.Iovec> iovecs)
        {
            System.UIntPtr _p0 = default;
            error _p0 = default!;

            System.UIntPtr r = default;            syscall.Errno e = default;
            while (true)
            {
                r, _, e = syscall.Syscall(syscall.SYS_WRITEV, uintptr(fd), uintptr(@unsafe.Pointer(_addr_iovecs[0L])), uintptr(len(iovecs)));
                if (e != syscall.EINTR)
                {
                    break;
                }
            }
            if (e != 0L)
            {
                return (r, error.As(e)!);
            }
            return (r, error.As(null!)!);

        }
    }
}}
