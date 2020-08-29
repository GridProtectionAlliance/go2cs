// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package os -- go2cs converted at 2020 August 29 08:43:41 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Go\src\os\executable_freebsd.go
using syscall = go.syscall_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class os_package
    {
        private static (@string, error) executable()
        {
            array<int> mib = new array<int>(new int[] { 1, 14, 12, -1 });

            var n = uintptr(0L); 
            // get length
            var (_, _, err) = syscall.Syscall6(syscall.SYS___SYSCTL, uintptr(@unsafe.Pointer(ref mib[0L])), 4L, 0L, uintptr(@unsafe.Pointer(ref n)), 0L, 0L);
            if (err != 0L)
            {
                return ("", err);
            }
            if (n == 0L)
            { // shouldn't happen
                return ("", null);
            }
            var buf = make_slice<byte>(n);
            _, _, err = syscall.Syscall6(syscall.SYS___SYSCTL, uintptr(@unsafe.Pointer(ref mib[0L])), 4L, uintptr(@unsafe.Pointer(ref buf[0L])), uintptr(@unsafe.Pointer(ref n)), 0L, 0L);
            if (err != 0L)
            {
                return ("", err);
            }
            if (n == 0L)
            { // shouldn't happen
                return ("", null);
            }
            return (string(buf[..n - 1L]), null);
        }
    }
}
