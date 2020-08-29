// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin dragonfly freebsd linux netbsd openbsd solaris windows

// package poll -- go2cs converted at 2020 August 29 08:25:41 UTC
// import "internal/poll" ==> using poll = go.@internal.poll_package
// Original source: C:\Go\src\internal\poll\sockopt.go
using syscall = go.syscall_package;
using static go.builtin;

namespace go {
namespace @internal
{
    public static partial class poll_package
    {
        // SetsockoptInt wraps the setsockopt network call with an int argument.
        private static error SetsockoptInt(this ref FD _fd, long level, long name, long arg) => func(_fd, (ref FD fd, Defer defer, Panic _, Recover __) =>
        {
            {
                var err = fd.incref();

                if (err != null)
                {
                    return error.As(err);
                }
            }
            defer(fd.decref());
            return error.As(syscall.SetsockoptInt(fd.Sysfd, level, name, arg));
        });

        // SetsockoptInet4Addr wraps the setsockopt network call with an IPv4 address.
        private static error SetsockoptInet4Addr(this ref FD _fd, long level, long name, array<byte> arg) => func(_fd, (ref FD fd, Defer defer, Panic _, Recover __) =>
        {
            {
                var err = fd.incref();

                if (err != null)
                {
                    return error.As(err);
                }

            }
            defer(fd.decref());
            return error.As(syscall.SetsockoptInet4Addr(fd.Sysfd, level, name, arg));
        });

        // SetsockoptLinger wraps the setsockopt network call with a Linger argument.
        private static error SetsockoptLinger(this ref FD _fd, long level, long name, ref syscall.Linger _l) => func(_fd, _l, (ref FD fd, ref syscall.Linger l, Defer defer, Panic _, Recover __) =>
        {
            {
                var err = fd.incref();

                if (err != null)
                {
                    return error.As(err);
                }

            }
            defer(fd.decref());
            return error.As(syscall.SetsockoptLinger(fd.Sysfd, level, name, l));
        });
    }
}}
