// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements sysSocket and accept for platforms that
// provide a fast path for setting SetNonblock and CloseOnExec.

// +build dragonfly freebsd linux

// package net -- go2cs converted at 2020 August 29 08:27:38 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\sock_cloexec.go
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
            var (s, err) = socketFunc(family, sotype | syscall.SOCK_NONBLOCK | syscall.SOCK_CLOEXEC, proto); 
            // On Linux the SOCK_NONBLOCK and SOCK_CLOEXEC flags were
            // introduced in 2.6.27 kernel and on FreeBSD both flags were
            // introduced in 10 kernel. If we get an EINVAL error on Linux
            // or EPROTONOSUPPORT error on FreeBSD, fall back to using
            // socket without them.

            if (err == null) 
                return (s, null);
            else if (err == syscall.EPROTONOSUPPORT || err == syscall.EINVAL)             else 
                return (-1L, os.NewSyscallError("socket", err));
            // See ../syscall/exec_unix.go for description of ForkLock.
            syscall.ForkLock.RLock();
            s, err = socketFunc(family, sotype, proto);
            if (err == null)
            {
                syscall.CloseOnExec(s);
            }
            syscall.ForkLock.RUnlock();
            if (err != null)
            {
                return (-1L, os.NewSyscallError("socket", err));
            }
            err = syscall.SetNonblock(s, true);

            if (err != null)
            {
                poll.CloseFunc(s);
                return (-1L, os.NewSyscallError("setnonblock", err));
            }
            return (s, null);
        }
    }
}
