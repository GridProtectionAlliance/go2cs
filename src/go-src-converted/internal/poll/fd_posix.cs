// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin dragonfly freebsd linux nacl netbsd openbsd solaris windows

// package poll -- go2cs converted at 2020 August 29 08:25:23 UTC
// import "internal/poll" ==> using poll = go.@internal.poll_package
// Original source: C:\Go\src\internal\poll\fd_posix.go
using io = go.io_package;
using syscall = go.syscall_package;
using static go.builtin;

namespace go {
namespace @internal
{
    public static partial class poll_package
    {
        // eofError returns io.EOF when fd is available for reading end of
        // file.
        private static error eofError(this ref FD fd, long n, error err)
        {
            if (n == 0L && err == null && fd.ZeroReadIsEOF)
            {
                return error.As(io.EOF);
            }
            return error.As(err);
        }

        // Fchmod wraps syscall.Fchmod.
        private static error Fchmod(this ref FD _fd, uint mode) => func(_fd, (ref FD fd, Defer defer, Panic _, Recover __) =>
        {
            {
                var err = fd.incref();

                if (err != null)
                {
                    return error.As(err);
                }

            }
            defer(fd.decref());
            return error.As(syscall.Fchmod(fd.Sysfd, mode));
        });

        // Fchown wraps syscall.Fchown.
        private static error Fchown(this ref FD _fd, long uid, long gid) => func(_fd, (ref FD fd, Defer defer, Panic _, Recover __) =>
        {
            {
                var err = fd.incref();

                if (err != null)
                {
                    return error.As(err);
                }

            }
            defer(fd.decref());
            return error.As(syscall.Fchown(fd.Sysfd, uid, gid));
        });

        // Ftruncate wraps syscall.Ftruncate.
        private static error Ftruncate(this ref FD _fd, long size) => func(_fd, (ref FD fd, Defer defer, Panic _, Recover __) =>
        {
            {
                var err = fd.incref();

                if (err != null)
                {
                    return error.As(err);
                }

            }
            defer(fd.decref());
            return error.As(syscall.Ftruncate(fd.Sysfd, size));
        });

        // Fsync wraps syscall.Fsync.
        private static error Fsync(this ref FD _fd) => func(_fd, (ref FD fd, Defer defer, Panic _, Recover __) =>
        {
            {
                var err = fd.incref();

                if (err != null)
                {
                    return error.As(err);
                }

            }
            defer(fd.decref());
            return error.As(syscall.Fsync(fd.Sysfd));
        });
    }
}}
