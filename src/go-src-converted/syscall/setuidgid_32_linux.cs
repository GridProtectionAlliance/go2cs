// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build linux
// +build 386 arm

// package syscall -- go2cs converted at 2020 October 09 05:01:31 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\setuidgid_32_linux.go

using static go.builtin;

namespace go
{
    public static partial class syscall_package
    {
        private static readonly var sys_GETEUID = SYS_GETEUID32;

        private static readonly var sys_SETGID = SYS_SETGID32;
        private static readonly var sys_SETUID = SYS_SETUID32;

    }
}
