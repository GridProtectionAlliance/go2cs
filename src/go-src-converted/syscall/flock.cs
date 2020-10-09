// +build linux freebsd openbsd netbsd dragonfly

// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package syscall -- go2cs converted at 2020 October 09 05:01:20 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\flock.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class syscall_package
    {
        // fcntl64Syscall is usually SYS_FCNTL, but is overridden on 32-bit Linux
        // systems by flock_linux_32bit.go to be SYS_FCNTL64.
        private static System.UIntPtr fcntl64Syscall = SYS_FCNTL;

        // FcntlFlock performs a fcntl syscall for the F_GETLK, F_SETLK or F_SETLKW command.
        public static error FcntlFlock(System.UIntPtr fd, long cmd, ptr<Flock_t> _addr_lk)
        {
            ref Flock_t lk = ref _addr_lk.val;

            var (_, _, errno) = Syscall(fcntl64Syscall, fd, uintptr(cmd), uintptr(@unsafe.Pointer(lk)));
            if (errno == 0L)
            {
                return error.As(null!)!;
            }

            return error.As(errno)!;

        }
    }
}
