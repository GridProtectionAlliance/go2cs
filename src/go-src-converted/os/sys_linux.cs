// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package os -- go2cs converted at 2020 October 09 05:07:28 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Go\src\os\sys_linux.go
using runtime = go.runtime_package;
using syscall = go.syscall_package;
using static go.builtin;

namespace go
{
    public static partial class os_package
    {
        private static (@string, error) hostname() => func((defer, _, __) =>
        {
            @string name = default;
            error err = default!;
 
            // Try uname first, as it's only one system call and reading
            // from /proc is not allowed on Android.
            ref syscall.Utsname un = ref heap(out ptr<syscall.Utsname> _addr_un);
            err = syscall.Uname(_addr_un);

            array<byte> buf = new array<byte>(512L); // Enough for a DNS name.
            foreach (var (i, b) in un.Nodename[..])
            {
                buf[i] = uint8(b);
                if (b == 0L)
                {
                    name = string(buf[..i]);
                    break;
                }
            }            if (err == null && len(name) > 0L && len(name) < 64L)
            {
                return (name, error.As(null!)!);
            }
            if (runtime.GOOS == "android")
            {
                if (name != "")
                {
                    return (name, error.As(null!)!);
                }
                return ("localhost", error.As(null!)!);

            }
            var (f, err) = Open("/proc/sys/kernel/hostname");
            if (err != null)
            {
                return ("", error.As(err)!);
            }
            defer(f.Close());

            var (n, err) = f.Read(buf[..]);
            if (err != null)
            {
                return ("", error.As(err)!);
            }
            if (n > 0L && buf[n - 1L] == '\n')
            {
                n--;
            }
            return (string(buf[..n]), error.As(null!)!);

        });
    }
}
