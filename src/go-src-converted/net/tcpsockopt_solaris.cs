// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package net -- go2cs converted at 2020 October 08 03:34:47 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\tcpsockopt_solaris.go
using runtime = go.runtime_package;
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
 
            // The kernel expects milliseconds so round to next highest
            // millisecond.
            var msecs = int(roundDurationUp(d, time.Millisecond)); 

            // Normally we'd do
            //    syscall.SetsockoptInt(fd.sysfd, syscall.IPPROTO_TCP, syscall.TCP_KEEPINTVL, secs)
            // here, but we can't because Solaris does not have TCP_KEEPINTVL.
            // Solaris has TCP_KEEPALIVE_ABORT_THRESHOLD, but it's not the same
            // thing, it refers to the total time until aborting (not between
            // probes), and it uses an exponential backoff algorithm instead of
            // waiting the same time between probes. We can't hope for the best
            // and do it anyway, like on Darwin, because Solaris might eventually
            // allocate a constant with a different meaning for the value of
            // TCP_KEEPINTVL on illumos.

            var err = fd.pfd.SetsockoptInt(syscall.IPPROTO_TCP, syscall.TCP_KEEPALIVE_THRESHOLD, msecs);
            runtime.KeepAlive(fd);
            return error.As(wrapSyscallError("setsockopt", err))!;

        }
    }
}
