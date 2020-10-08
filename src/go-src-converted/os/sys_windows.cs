// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package os -- go2cs converted at 2020 October 08 03:45:20 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Go\src\os\sys_windows.go
using windows = go.@internal.syscall.windows_package;
using syscall = go.syscall_package;
using static go.builtin;

namespace go
{
    public static partial class os_package
    {
        private static (@string, error) hostname()
        {
            @string name = default;
            error err = default!;
 
            // Use PhysicalDnsHostname to uniquely identify host in a cluster
            const var format = (var)windows.ComputerNamePhysicalDnsHostname;



            ref var n = ref heap(uint32(64L), out ptr<var> _addr_n);
            while (true)
            {
                var b = make_slice<ushort>(n);
                var err = windows.GetComputerNameEx(format, _addr_b[0L], _addr_n);
                if (err == null)
                {
                    return (syscall.UTF16ToString(b[..n]), error.As(null!)!);
                }
                if (err != syscall.ERROR_MORE_DATA)
                {
                    return ("", error.As(NewSyscallError("ComputerNameEx", err))!);
                }
                if (n <= uint32(len(b)))
                {
                    return ("", error.As(NewSyscallError("ComputerNameEx", err))!);
                }
            }

        }
    }
}
