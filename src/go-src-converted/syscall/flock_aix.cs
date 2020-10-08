// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package syscall -- go2cs converted at 2020 October 08 03:26:37 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\flock_aix.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class syscall_package
    {
        // On AIX, there is no flock() system call.

        // FcntlFlock performs a fcntl syscall for the F_GETLK, F_SETLK or F_SETLKW command.
        public static error FcntlFlock(System.UIntPtr fd, long cmd, ptr<Flock_t> _addr_lk)
        {
            error err = default!;
            ref Flock_t lk = ref _addr_lk.val;

            var (_, _, e1) = syscall6(uintptr(@unsafe.Pointer(_addr_libc_fcntl)), 3L, uintptr(fd), uintptr(cmd), uintptr(@unsafe.Pointer(lk)), 0L, 0L, 0L);
            if (e1 != 0L)
            {
                err = errnoErr(e1);
            }
            return ;

        }
    }
}
