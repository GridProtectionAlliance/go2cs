// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package net -- go2cs converted at 2020 August 29 08:27:30 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\sockopt_plan9.go
using syscall = go.syscall_package;
using static go.builtin;

namespace go
{
    public static partial class net_package
    {
        private static error setKeepAlive(ref netFD fd, bool keepalive)
        {
            if (keepalive)
            {
                var (_, e) = fd.ctl.WriteAt((slice<byte>)"keepalive", 0L);
                return error.As(e);
            }
            return error.As(null);
        }

        private static error setLinger(ref netFD fd, long sec)
        {
            return error.As(syscall.EPLAN9);
        }
    }
}
