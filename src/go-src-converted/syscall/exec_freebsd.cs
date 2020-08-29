// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package syscall -- go2cs converted at 2020 August 29 08:36:52 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\exec_freebsd.go

using static go.builtin;

namespace go
{
    public static partial class syscall_package
    {
        private static error forkExecPipe(slice<long> p)
        {
            return error.As(Pipe2(p, O_CLOEXEC));
        }
    }
}
