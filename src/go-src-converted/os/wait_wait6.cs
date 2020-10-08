// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build freebsd

// package os -- go2cs converted at 2020 October 08 03:45:26 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Go\src\os\wait_wait6.go
using runtime = go.runtime_package;
using syscall = go.syscall_package;
using static go.builtin;

namespace go
{
    public static partial class os_package
    {
        private static readonly long _P_PID = (long)0L;

        // blockUntilWaitable attempts to block until a call to p.Wait will
        // succeed immediately, and reports whether it has done so.
        // It does not actually call p.Wait.


        // blockUntilWaitable attempts to block until a call to p.Wait will
        // succeed immediately, and reports whether it has done so.
        // It does not actually call p.Wait.
        private static (bool, error) blockUntilWaitable(this ptr<Process> _addr_p)
        {
            bool _p0 = default;
            error _p0 = default!;
            ref Process p = ref _addr_p.val;

            syscall.Errno errno = default;
            while (true)
            { 
                // The arguments on 32-bit FreeBSD look like the following:
                // - freebsd32_wait6_args{ idtype, id1, id2, status, options, wrusage, info } or
                // - freebsd32_wait6_args{ idtype, pad, id1, id2, status, options, wrusage, info } when PAD64_REQUIRED=1 on ARM, MIPS or PowerPC
                if (runtime.GOARCH == "386")
                {
                    _, _, errno = syscall.Syscall9(syscall.SYS_WAIT6, _P_PID, uintptr(p.Pid), 0L, 0L, syscall.WEXITED | syscall.WNOWAIT, 0L, 0L, 0L, 0L);
                }
                else if (runtime.GOARCH == "arm")
                {
                    _, _, errno = syscall.Syscall9(syscall.SYS_WAIT6, _P_PID, 0L, uintptr(p.Pid), 0L, 0L, syscall.WEXITED | syscall.WNOWAIT, 0L, 0L, 0L);
                }
                else
                {
                    _, _, errno = syscall.Syscall6(syscall.SYS_WAIT6, _P_PID, uintptr(p.Pid), 0L, syscall.WEXITED | syscall.WNOWAIT, 0L, 0L);
                }

                if (errno != syscall.EINTR)
                {
                    break;
                }

            }

            runtime.KeepAlive(p);
            if (errno != 0L)
            {
                return (false, error.As(NewSyscallError("wait6", errno))!);
            }

            return (true, error.As(null!)!);

        }
    }
}
