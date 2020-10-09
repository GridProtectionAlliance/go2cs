// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build aix darwin dragonfly freebsd linux netbsd openbsd solaris windows

// package net -- go2cs converted at 2020 October 09 04:52:28 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\tcpsockopt_posix.go
using runtime = go.runtime_package;
using syscall = go.syscall_package;
using static go.builtin;

namespace go
{
    public static partial class net_package
    {
        private static error setNoDelay(ptr<netFD> _addr_fd, bool noDelay)
        {
            ref netFD fd = ref _addr_fd.val;

            var err = fd.pfd.SetsockoptInt(syscall.IPPROTO_TCP, syscall.TCP_NODELAY, boolint(noDelay));
            runtime.KeepAlive(fd);
            return error.As(wrapSyscallError("setsockopt", err))!;
        }
    }
}
