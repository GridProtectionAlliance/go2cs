// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin dragonfly freebsd linux netbsd openbsd solaris windows

// package poll -- go2cs converted at 2020 August 29 08:25:42 UTC
// import "internal/poll" ==> using poll = go.@internal.poll_package
// Original source: C:\Go\src\internal\poll\sockoptip.go
using syscall = go.syscall_package;
using static go.builtin;

namespace go {
namespace @internal
{
    public static partial class poll_package
    {
        // SetsockoptIPMreq wraps the setsockopt network call with an IPMreq argument.
        private static error SetsockoptIPMreq(this ref FD _fd, long level, long name, ref syscall.IPMreq _mreq) => func(_fd, _mreq, (ref FD fd, ref syscall.IPMreq mreq, Defer defer, Panic _, Recover __) =>
        {
            {
                var err = fd.incref();

                if (err != null)
                {
                    return error.As(err);
                }
            }
            defer(fd.decref());
            return error.As(syscall.SetsockoptIPMreq(fd.Sysfd, level, name, mreq));
        });

        // SetsockoptIPv6Mreq wraps the setsockopt network call with an IPv6Mreq argument.
        private static error SetsockoptIPv6Mreq(this ref FD _fd, long level, long name, ref syscall.IPv6Mreq _mreq) => func(_fd, _mreq, (ref FD fd, ref syscall.IPv6Mreq mreq, Defer defer, Panic _, Recover __) =>
        {
            {
                var err = fd.incref();

                if (err != null)
                {
                    return error.As(err);
                }

            }
            defer(fd.decref());
            return error.As(syscall.SetsockoptIPv6Mreq(fd.Sysfd, level, name, mreq));
        });
    }
}}
