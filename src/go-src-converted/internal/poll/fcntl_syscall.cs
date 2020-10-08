// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build dragonfly freebsd linux netbsd openbsd

// package poll -- go2cs converted at 2020 October 08 03:32:08 UTC
// import "internal/poll" ==> using poll = go.@internal.poll_package
// Original source: C:\Go\src\internal\poll\fcntl_syscall.go
using unix = go.@internal.syscall.unix_package;
using syscall = go.syscall_package;
using static go.builtin;

namespace go {
namespace @internal
{
    public static partial class poll_package
    {
        private static (long, error) fcntl(long fd, long cmd, long arg)
        {
            long _p0 = default;
            error _p0 = default!;

            var (r, _, e) = syscall.Syscall(unix.FcntlSyscall, uintptr(fd), uintptr(cmd), uintptr(arg));
            if (e != 0L)
            {
                return (int(r), error.As(syscall.Errno(e))!);
            }
            return (int(r), error.As(null!)!);

        }
    }
}}
