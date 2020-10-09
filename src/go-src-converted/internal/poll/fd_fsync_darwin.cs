// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package poll -- go2cs converted at 2020 October 09 04:51:01 UTC
// import "internal/poll" ==> using poll = go.@internal.poll_package
// Original source: C:\Go\src\internal\poll\fd_fsync_darwin.go
using syscall = go.syscall_package;
using static go.builtin;

namespace go {
namespace @internal
{
    public static partial class poll_package
    {
        // Fsync invokes SYS_FCNTL with SYS_FULLFSYNC because
        // on OS X, SYS_FSYNC doesn't fully flush contents to disk.
        // See Issue #26650 as well as the man page for fsync on OS X.
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

            var (_, e1) = fcntl(fd.Sysfd, syscall.F_FULLFSYNC, 0L);
            return error.As(e1)!;

        });
    }
}}
