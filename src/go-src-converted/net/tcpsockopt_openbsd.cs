// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package net -- go2cs converted at 2020 October 09 04:52:26 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\tcpsockopt_openbsd.go
using syscall = go.syscall_package;
using time = go.time_package;
using static go.builtin;

namespace go
{
    public static partial class net_package
    {
        private static error setKeepAlivePeriod(ptr<netFD> _addr_fd, time.Duration d)
        {
            ref netFD fd = ref _addr_fd.val;
 
            // OpenBSD has no user-settable per-socket TCP keepalive
            // options.
            return error.As(syscall.ENOPROTOOPT)!;

        }
    }
}
