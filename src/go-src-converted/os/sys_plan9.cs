// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package os -- go2cs converted at 2020 October 09 05:07:29 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Go\src\os\sys_plan9.go

using static go.builtin;

namespace go
{
    public static partial class os_package
    {
        private static (@string, error) hostname() => func((defer, _, __) =>
        {
            @string name = default;
            error err = default!;

            var (f, err) = Open("#c/sysname");
            if (err != null)
            {
                return ("", error.As(err)!);
            }
            defer(f.Close());

            array<byte> buf = new array<byte>(128L);
            var (n, err) = f.Read(buf[..len(buf) - 1L]);

            if (err != null)
            {
                return ("", error.As(err)!);
            }
            if (n > 0L)
            {
                buf[n] = 0L;
            }
            return (string(buf[0L..n]), error.As(null!)!);

        });
    }
}
