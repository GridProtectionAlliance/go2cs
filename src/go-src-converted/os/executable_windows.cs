// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package os -- go2cs converted at 2020 August 29 08:43:45 UTC
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
            var n = uint32(1024L);
            slice<ushort> buf = default;
            while (true)
            {
                buf = make_slice<ushort>(n);
                var (r, err) = windows.GetModuleFileName(handle, ref buf[0L], n);
                if (err != null)
                {
                    return ("", err);
                }
                if (r < n)
                {
                    break;
                }
                n += 1024L;
            }
            return (syscall.UTF16ToString(buf), null);
        }

        private static (@string, error) executable()
        {
            return getModuleFileName(0L);
        }
    }
}
