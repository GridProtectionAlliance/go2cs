// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package os -- go2cs converted at 2020 October 09 05:07:05 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Go\src\os\executable_windows.go
using windows = go.@internal.syscall.windows_package;
using syscall = go.syscall_package;
using static go.builtin;

namespace go
{
    public static partial class os_package
    {
        private static (@string, error) getModuleFileName(syscall.Handle handle)
        {
            @string _p0 = default;
            error _p0 = default!;

            var n = uint32(1024L);
            slice<ushort> buf = default;
            while (true)
            {
                buf = make_slice<ushort>(n);
                var (r, err) = windows.GetModuleFileName(handle, _addr_buf[0L], n);
                if (err != null)
                {
                    return ("", error.As(err)!);
                }
                if (r < n)
                {
                    break;
                }
                n += 1024L;

            }
            return (syscall.UTF16ToString(buf), error.As(null!)!);

        }

        private static (@string, error) executable()
        {
            @string _p0 = default;
            error _p0 = default!;

            return getModuleFileName(0L);
        }
    }
}
