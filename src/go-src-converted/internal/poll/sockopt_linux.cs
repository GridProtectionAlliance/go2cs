// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package poll -- go2cs converted at 2020 August 29 08:25:43 UTC
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
        private static error SetsockoptIPMreqn(this ref FD _fd, long level, long name, ref syscall.IPMreqn _mreq) => func(_fd, _mreq, (ref FD fd, ref syscall.IPMreqn mreq, Defer defer, Panic _, Recover __) =>
        {
            {
                var err = fd.incref();

                if (err != null)
                {
                    return error.As(err);
                }
            }
            defer(fd.decref());
            return error.As(syscall.SetsockoptIPMreqn(fd.Sysfd, level, name, mreq));
        });
    }
}}
