// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build aix darwin dragonfly freebsd js,wasm linux netbsd openbsd solaris windows

// package poll -- go2cs converted at 2020 October 09 04:51:06 UTC
// import "internal/poll" ==> using poll = go.@internal.poll_package
// Original source: C:\Go\src\internal\poll\fd_posix.go
using io = go.io_package;
using syscall = go.syscall_package;
using static go.builtin;
using System;

namespace go {
namespace @internal
{
    public static partial class poll_package
    {
        // eofError returns io.EOF when fd is available for reading end of
        // file.
        private static error eofError(this ptr<FD> _addr_fd, long n, error err)
        {
            ref FD fd = ref _addr_fd.val;

            if (n == 0L && err == null && fd.ZeroReadIsEOF)
            {
                return error.As(io.EOF)!;
            }
            return error.As(err)!;

        }

        // Shutdown wraps syscall.Shutdown.
        private static error Shutdown(this ptr<FD> _addr_fd, long how) => func((defer, _, __) =>
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
            return error.As(syscall.Shutdown(fd.Sysfd, how))!;

        });

        // Fchmod wraps syscall.Fchmod.
        private static error Fchmod(this ptr<FD> _addr_fd, uint mode) => func((defer, _, __) =>
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
            return error.As(syscall.Fchmod(fd.Sysfd, mode))!;

        });

        // Fchown wraps syscall.Fchown.
        private static error Fchown(this ptr<FD> _addr_fd, long uid, long gid) => func((defer, _, __) =>
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
            return error.As(syscall.Fchown(fd.Sysfd, uid, gid))!;

        });

        // Ftruncate wraps syscall.Ftruncate.
        private static error Ftruncate(this ptr<FD> _addr_fd, long size) => func((defer, _, __) =>
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
            return error.As(syscall.Ftruncate(fd.Sysfd, size))!;

        });

        // RawControl invokes the user-defined function f for a non-IO
        // operation.
        private static error RawControl(this ptr<FD> _addr_fd, Action<System.UIntPtr> f) => func((defer, _, __) =>
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
            f(uintptr(fd.Sysfd));
            return error.As(null!)!;

        });
    }
}}
