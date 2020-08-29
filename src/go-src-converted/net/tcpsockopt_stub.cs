// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build nacl

// package net -- go2cs converted at 2020 August 29 08:27:54 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\tcpsockopt_stub.go
using syscall = go.syscall_package;
using time = go.time_package;
using static go.builtin;

namespace go
{
    public static partial class net_package
    {
        private static error setNoDelay(ref netFD fd, bool noDelay)
        {
            return error.As(syscall.ENOPROTOOPT);
        }

        private static error setKeepAlivePeriod(ref netFD fd, time.Duration d)
        {
            return error.As(syscall.ENOPROTOOPT);
        }
    }
}
