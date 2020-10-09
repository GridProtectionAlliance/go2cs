// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build aix darwin solaris

// package unix -- go2cs converted at 2020 October 09 04:50:59 UTC
// import "internal/syscall/unix" ==> using unix = go.@internal.syscall.unix_package
// Original source: C:\Go\src\internal\syscall\unix\nonblocking_libc.go
using syscall = go.syscall_package;
using _@unsafe_ = go.@unsafe_package;
using static go.builtin;

namespace go {
namespace @internal {
namespace syscall
{
    public static partial class unix_package
    {
        public static (bool, error) IsNonblock(long fd)
        {
            bool nonblocking = default;
            error err = default!;

            var (flag, e1) = fcntl(fd, syscall.F_GETFL, 0L);
            if (e1 != null)
            {
                return (false, error.As(e1)!);
            }
            return (flag & syscall.O_NONBLOCK != 0L, error.As(null!)!);

        }

        // Implemented in the syscall package.
        //go:linkname fcntl syscall.fcntl
        private static (long, error) fcntl(long fd, long cmd, long arg)
;
    }
}}}
