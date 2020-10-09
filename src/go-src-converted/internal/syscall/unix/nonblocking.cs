// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build dragonfly freebsd linux netbsd openbsd

// package unix -- go2cs converted at 2020 October 09 04:50:59 UTC
// import "internal/syscall/unix" ==> using unix = go.@internal.syscall.unix_package
// Original source: C:\Go\src\internal\syscall\unix\nonblocking.go
using syscall = go.syscall_package;
using static go.builtin;

namespace go {
namespace @internal {
namespace syscall
{
    public static partial class unix_package
    {
        // FcntlSyscall is the number for the fcntl system call. This is
        // usually SYS_FCNTL, but can be overridden to SYS_FCNTL64.
        public static System.UIntPtr FcntlSyscall = syscall.SYS_FCNTL;

        public static (bool, error) IsNonblock(long fd)
        {
            bool nonblocking = default;
            error err = default!;

            var (flag, _, e1) = syscall.Syscall(FcntlSyscall, uintptr(fd), uintptr(syscall.F_GETFL), 0L);
            if (e1 != 0L)
            {
                return (false, error.As(e1)!);
            }

            return (flag & syscall.O_NONBLOCK != 0L, error.As(null!)!);

        }
    }
}}}
