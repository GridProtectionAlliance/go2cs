// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements sysSocket and accept for platforms that
// provide a fast path for setting SetNonblock and CloseOnExec.

// +build dragonfly freebsd linux netbsd openbsd

// package poll -- go2cs converted at 2020 October 09 04:51:24 UTC
// import "internal/poll" ==> using poll = go.@internal.poll_package
// Original source: C:\Go\src\internal\poll\sock_cloexec.go
using syscall = go.syscall_package;
using static go.builtin;

namespace go {
namespace @internal
{
    public static partial class poll_package
    {
        // Wrapper around the accept system call that marks the returned file
        // descriptor as nonblocking and close-on-exec.
        private static (long, syscall.Sockaddr, @string, error) accept(long s)
        {
            long _p0 = default;
            syscall.Sockaddr _p0 = default;
            @string _p0 = default;
            error _p0 = default!;

            var (ns, sa, err) = Accept4Func(s, syscall.SOCK_NONBLOCK | syscall.SOCK_CLOEXEC); 
            // On Linux the accept4 system call was introduced in 2.6.28
            // kernel and on FreeBSD it was introduced in 10 kernel. If we
            // get an ENOSYS error on both Linux and FreeBSD, or EINVAL
            // error on Linux, fall back to using accept.

            if (err == null) 
                return (ns, sa, "", error.As(null!)!);
            else if (err == syscall.ENOSYS)             else if (err == syscall.EINVAL)             else if (err == syscall.EACCES)             else if (err == syscall.EFAULT)             else // errors other than the ones listed
                return (-1L, sa, "accept4", error.As(err)!);
            // See ../syscall/exec_unix.go for description of ForkLock.
            // It is probably okay to hold the lock across syscall.Accept
            // because we have put fd.sysfd into non-blocking mode.
            // However, a call to the File method will put it back into
            // blocking mode. We can't take that risk, so no use of ForkLock here.
            ns, sa, err = AcceptFunc(s);
            if (err == null)
            {
                syscall.CloseOnExec(ns);
            }
            if (err != null)
            {
                return (-1L, null, "accept", error.As(err)!);
            }
            err = syscall.SetNonblock(ns, true);

            if (err != null)
            {
                CloseFunc(ns);
                return (-1L, null, "setnonblock", error.As(err)!);
            }
            return (ns, sa, "", error.As(null!)!);

        }
    }
}}
