// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// TCP socket options for plan9

// package net -- go2cs converted at 2020 October 08 03:34:45 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\tcpsockopt_plan9.go
using syscall = go.syscall_package;
using time = go.time_package;
using static go.builtin;

namespace go
{
    public static partial class net_package
    {
        private static error setNoDelay(ptr<netFD> _addr_fd, bool noDelay)
        {
            ref netFD fd = ref _addr_fd.val;

            return error.As(syscall.EPLAN9)!;
        }

        // Set keep alive period.
        private static error setKeepAlivePeriod(ptr<netFD> _addr_fd, time.Duration d)
        {
            ref netFD fd = ref _addr_fd.val;

            @string cmd = "keepalive " + itoa(int(d / time.Millisecond));
            var (_, e) = fd.ctl.WriteAt((slice<byte>)cmd, 0L);
            return error.As(e)!;
        }
    }
}
