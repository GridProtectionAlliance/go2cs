// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build amd64,solaris

// package syscall -- go2cs converted at 2020 August 29 08:42:02 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\zsysnum_solaris_amd64.go

using static go.builtin;

namespace go
{
    public static partial class syscall_package
    {
        // TODO(aram): remove these before Go 1.3.
        public static readonly long SYS_EXECVE = 59L;
        public static readonly long SYS_FCNTL = 62L;
    }
}
