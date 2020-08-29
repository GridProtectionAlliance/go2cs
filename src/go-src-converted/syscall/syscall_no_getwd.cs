// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build dragonfly freebsd netbsd openbsd

// package syscall -- go2cs converted at 2020 August 29 08:38:14 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\syscall_no_getwd.go

using static go.builtin;

namespace go
{
    public static partial class syscall_package
    {
        public static readonly var ImplementsGetwd = false;



        public static (@string, error) Getwd()
        {
            return ("", ENOTSUP);
        }
    }
}
