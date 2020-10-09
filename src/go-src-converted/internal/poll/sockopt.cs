// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build aix darwin dragonfly freebsd linux netbsd openbsd solaris windows

// package poll -- go2cs converted at 2020 October 09 04:51:22 UTC
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
        private static error SetsockoptInt(this ptr<FD> _addr_fd, long level, long name, long arg) => func((defer, _, __) =>
        {
            ref FD fd = ref _addr_fd.val;

            {
                var err = fd.incref();

                if (err != null)
                {
                    return error.As(err)!;
                }
            }

            defer(fd.decref());
            return error.As(syscall.SetsockoptInt(fd.Sysfd, level, name, arg))!;

        });

        // SetsockoptInet4Addr wraps the setsockopt network call with an IPv4 address.
        private static error SetsockoptInet4Addr(this ptr<FD> _addr_fd, long level, long name, array<byte> arg) => func((defer, _, __) =>
        {
            arg = arg.Clone();
            ref FD fd = ref _addr_fd.val;

            {
                var err = fd.incref();

                if (err != null)
                {
                    return error.As(err)!;
                }

            }

            defer(fd.decref());
            return error.As(syscall.SetsockoptInet4Addr(fd.Sysfd, level, name, arg))!;

        });

        // SetsockoptLinger wraps the setsockopt network call with a Linger argument.
        private static error SetsockoptLinger(this ptr<FD> _addr_fd, long level, long name, ptr<syscall.Linger> _addr_l) => func((defer, _, __) =>
        {
            ref FD fd = ref _addr_fd.val;
            ref syscall.Linger l = ref _addr_l.val;

            {
                var err = fd.incref();

                if (err != null)
                {
                    return error.As(err)!;
                }

            }

            defer(fd.decref());
            return error.As(syscall.SetsockoptLinger(fd.Sysfd, level, name, l))!;

        });
    }
}}
