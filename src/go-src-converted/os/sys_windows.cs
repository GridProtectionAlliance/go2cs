// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package os -- go2cs converted at 2020 August 29 08:44:34 UTC
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
            // Use PhysicalDnsHostname to uniquely identify host in a cluster
            const var format = windows.ComputerNamePhysicalDnsHostname;



            var n = uint32(64L);
            while (true)
            {
                var b = make_slice<ushort>(n);
                var err = windows.GetComputerNameEx(format, ref b[0L], ref n);
                if (err == null)
                {
                    return (syscall.UTF16ToString(b[..n]), null);
                }
                if (err != syscall.ERROR_MORE_DATA)
                {
                    return ("", NewSyscallError("ComputerNameEx", err));
                }
                if (n <= uint32(len(b)))
                {
                    return ("", NewSyscallError("ComputerNameEx", err));
                }
            }
        }
    }
}
