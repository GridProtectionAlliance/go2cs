// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build aix dragonfly freebsd js,wasm linux netbsd openbsd solaris

// package poll -- go2cs converted at 2020 October 09 04:51:01 UTC
// import "internal/poll" ==> using poll = go.@internal.poll_package
// Original source: C:\Go\src\internal\poll\fd_fsync_posix.go
using syscall = go.syscall_package;
using static go.builtin;

namespace go {
namespace @internal
{
    public static partial class poll_package
    {
        // Fsync wraps syscall.Fsync.
        private static error Fsync(this ptr<FD> _addr_fd) => func((defer, _, __) =>
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
            return error.As(syscall.Fsync(fd.Sysfd))!;

        });
    }
}}
