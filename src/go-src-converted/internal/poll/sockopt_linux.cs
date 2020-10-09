// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package poll -- go2cs converted at 2020 October 09 04:51:22 UTC
// import "internal/poll" ==> using poll = go.@internal.poll_package
// Original source: C:\Go\src\internal\poll\sockopt_linux.go
using syscall = go.syscall_package;
using static go.builtin;

namespace go {
namespace @internal
{
    public static partial class poll_package
    {
        // SetsockoptIPMreqn wraps the setsockopt network call with an IPMreqn argument.
        private static error SetsockoptIPMreqn(this ptr<FD> _addr_fd, long level, long name, ptr<syscall.IPMreqn> _addr_mreq) => func((defer, _, __) =>
        {
            ref FD fd = ref _addr_fd.val;
            ref syscall.IPMreqn mreq = ref _addr_mreq.val;

            {
                var err = fd.incref();

                if (err != null)
                {
                    return error.As(err)!;
                }
            }

            defer(fd.decref());
            return error.As(syscall.SetsockoptIPMreqn(fd.Sysfd, level, name, mreq))!;

        });
    }
}}
