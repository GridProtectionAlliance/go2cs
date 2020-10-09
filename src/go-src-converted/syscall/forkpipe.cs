// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build aix darwin dragonfly solaris

// package syscall -- go2cs converted at 2020 October 09 05:01:21 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\forkpipe.go

using static go.builtin;

namespace go
{
    public static partial class syscall_package
    {
        // Try to open a pipe with O_CLOEXEC set on both file descriptors.
        private static error forkExecPipe(slice<long> p)
        {
            var err = Pipe(p);
            if (err != null)
            {
                return error.As(err)!;
            }
            _, err = fcntl(p[0L], F_SETFD, FD_CLOEXEC);
            if (err != null)
            {
                return error.As(err)!;
            }
            _, err = fcntl(p[1L], F_SETFD, FD_CLOEXEC);
            return error.As(err)!;

        }
    }
}
