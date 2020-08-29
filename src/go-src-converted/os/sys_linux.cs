// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Linux-specific

// package os -- go2cs converted at 2020 August 29 08:44:33 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Go\src\os\sys_linux.go

using static go.builtin;

namespace go
{
    public static partial class os_package
    {
        private static (@string, error) hostname() => func((defer, _, __) =>
        {
            var (f, err) = Open("/proc/sys/kernel/hostname");
            if (err != null)
            {
                return ("", err);
            }
            defer(f.Close());

            array<byte> buf = new array<byte>(512L); // Enough for a DNS name.
            var (n, err) = f.Read(buf[0L..]);
            if (err != null)
            {
                return ("", err);
            }
            if (n > 0L && buf[n - 1L] == '\n')
            {
                n--;
            }
            return (string(buf[0L..n]), null);
        });
    }
}
