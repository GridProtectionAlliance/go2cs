// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build plan9

// package os -- go2cs converted at 2020 October 09 05:07:04 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Go\src\os\executable_plan9.go
using syscall = go.syscall_package;
using static go.builtin;

namespace go
{
    public static partial class os_package
    {
        private static (@string, error) executable() => func((defer, _, __) =>
        {
            @string _p0 = default;
            error _p0 = default!;

            @string fn = "/proc/" + itoa(Getpid()) + "/text";
            var (f, err) = Open(fn);
            if (err != null)
            {
                return ("", error.As(err)!);
            }
            defer(f.Close());
            return syscall.Fd2path(int(f.Fd()));

        });
    }
}
