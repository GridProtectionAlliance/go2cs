// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build aix darwin dragonfly freebsd js linux netbsd openbsd solaris

// package net -- go2cs converted at 2020 October 08 03:31:43 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\error_unix.go
using syscall = go.syscall_package;
using static go.builtin;

namespace go
{
    public static partial class net_package
    {
        private static bool isConnError(error err)
        {
            {
                syscall.Errno (se, ok) = err._<syscall.Errno>();

                if (ok)
                {
                    return se == syscall.ECONNRESET || se == syscall.ECONNABORTED;
                }
            }

            return false;

        }
    }
}
