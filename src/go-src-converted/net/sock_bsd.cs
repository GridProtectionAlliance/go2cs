// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin dragonfly freebsd netbsd openbsd

// package net -- go2cs converted at 2020 October 09 04:52:20 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\sock_bsd.go
using runtime = go.runtime_package;
using syscall = go.syscall_package;
using static go.builtin;

namespace go
{
    public static partial class net_package
    {
        private static long maxListenerBacklog()
        {
            uint n = default;            error err = default!;
            switch (runtime.GOOS)
            {
                case "darwin": 
                    n, err = syscall.SysctlUint32("kern.ipc.somaxconn");
                    break;
                case "freebsd": 
                    n, err = syscall.SysctlUint32("kern.ipc.soacceptqueue");
                    break;
                case "netbsd": 
                    break;
                case "openbsd": 
                    n, err = syscall.SysctlUint32("kern.somaxconn");
                    break;
            }
            if (n == 0L || err != null)
            {
                return syscall.SOMAXCONN;
            }
            if (n > 1L << (int)(16L) - 1L)
            {
                n = 1L << (int)(16L) - 1L;
            }
            return int(n);

        }
    }
}
