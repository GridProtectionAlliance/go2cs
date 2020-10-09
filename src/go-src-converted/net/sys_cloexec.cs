// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements sysSocket and accept for platforms that do not
// provide a fast path for setting SetNonblock and CloseOnExec.

// +build aix darwin solaris

// package net -- go2cs converted at 2020 October 09 04:52:24 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\sys_cloexec.go
using poll = go.@internal.poll_package;
using os = go.os_package;
using syscall = go.syscall_package;
using static go.builtin;

namespace go
{
    public static partial class net_package
    {
        // Wrapper around the socket system call that marks the returned file
        // descriptor as nonblocking and close-on-exec.
        private static (long, error) sysSocket(long family, long sotype, long proto)
        {
            long _p0 = default;
            error _p0 = default!;
 
            // See ../syscall/exec_unix.go for description of ForkLock.
            syscall.ForkLock.RLock();
            var (s, err) = socketFunc(family, sotype, proto);
            if (err == null)
            {
                syscall.CloseOnExec(s);
            }
            syscall.ForkLock.RUnlock();
            if (err != null)
            {
                return (-1L, error.As(os.NewSyscallError("socket", err))!);
            }
            err = syscall.SetNonblock(s, true);

            if (err != null)
            {
                poll.CloseFunc(s);
                return (-1L, error.As(os.NewSyscallError("setnonblock", err))!);
            }
            return (s, error.As(null!)!);

        }
    }
}
