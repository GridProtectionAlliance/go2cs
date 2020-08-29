// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package net -- go2cs converted at 2020 August 29 08:27:39 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\sock_linux.go
using syscall = go.syscall_package;
using static go.builtin;

namespace go
{
    public static partial class net_package
    {
        private static long maxListenerBacklog() => func((defer, _, __) =>
        {
            var (fd, err) = open("/proc/sys/net/core/somaxconn");
            if (err != null)
            {
                return syscall.SOMAXCONN;
            }
            defer(fd.close());
            var (l, ok) = fd.readLine();
            if (!ok)
            {
                return syscall.SOMAXCONN;
            }
            var f = getFields(l);
            var (n, _, ok) = dtoi(f[0L]);
            if (n == 0L || !ok)
            {
                return syscall.SOMAXCONN;
            }
            if (n > 1L << (int)(16L) - 1L)
            {
                n = 1L << (int)(16L) - 1L;
            }
            return n;
        });
    }
}
