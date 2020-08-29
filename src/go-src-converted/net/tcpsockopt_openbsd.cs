// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package net -- go2cs converted at 2020 August 29 08:27:50 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\tcpsockopt_openbsd.go
using syscall = go.syscall_package;
using time = go.time_package;
using static go.builtin;

namespace go
{
    public static partial class net_package
    {
        private static error setKeepAlivePeriod(ref netFD fd, time.Duration d)
        { 
            // OpenBSD has no user-settable per-socket TCP keepalive
            // options.
            return error.As(syscall.ENOPROTOOPT);
        }
    }
}
