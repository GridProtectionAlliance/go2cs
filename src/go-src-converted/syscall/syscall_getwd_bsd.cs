// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build dragonfly freebsd netbsd openbsd

// package syscall -- go2cs converted at 2020 October 08 03:27:23 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\syscall_getwd_bsd.go

using static go.builtin;

namespace go
{
    public static partial class syscall_package
    {
        public static readonly var ImplementsGetwd = (var)true;



        public static (@string, error) Getwd()
        {
            @string _p0 = default;
            error _p0 = default!;

            array<byte> buf = new array<byte>(pathMax);
            var (_, err) = getcwd(buf[..]);
            if (err != null)
            {
                return ("", error.As(err)!);
            }

            var n = clen(buf[..]);
            if (n < 1L)
            {
                return ("", error.As(EINVAL)!);
            }

            return (string(buf[..n]), error.As(null!)!);

        }
    }
}
