// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package os -- go2cs converted at 2020 October 09 05:07:17 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Go\src\os\getwd_darwin.go
using syscall = go.syscall_package;
using static go.builtin;

namespace go
{
    public static partial class os_package
    {
        private static void init()
        {
            useSyscallwd = useSyscallwdDarwin;
        }

        private static bool useSyscallwdDarwin(error err)
        {
            return err != syscall.ENOTSUP;
        }
    }
}
