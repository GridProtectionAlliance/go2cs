// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build freebsd linux netbsd openbsd solaris

// package runtime -- go2cs converted at 2020 October 08 03:21:34 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\nbpipe_pipe2.go

using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        private static (int, int, int) nonblockingPipe()
        {
            int r = default;
            int w = default;
            int errno = default;

            r, w, errno = pipe2(_O_NONBLOCK | _O_CLOEXEC);
            if (errno == -_ENOSYS)
            {
                r, w, errno = pipe();
                if (errno != 0L)
                {
                    return (-1L, -1L, errno);
                }
                closeonexec(r);
                setNonblock(r);
                closeonexec(w);
                setNonblock(w);

            }
            return (r, w, errno);

        }
    }
}
