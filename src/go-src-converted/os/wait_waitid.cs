// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// We used to used this code for Darwin, but according to issue #19314
// waitid returns if the process is stopped, even when using WEXITED.

// +build linux

// package os -- go2cs converted at 2020 August 29 08:44:42 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Go\src\os\wait_waitid.go
using runtime = go.runtime_package;
using syscall = go.syscall_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class os_package
    {
        private static readonly long _P_PID = 1L;

        // blockUntilWaitable attempts to block until a call to p.Wait will
        // succeed immediately, and returns whether it has done so.
        // It does not actually call p.Wait.


        // blockUntilWaitable attempts to block until a call to p.Wait will
        // succeed immediately, and returns whether it has done so.
        // It does not actually call p.Wait.
        private static (bool, error) blockUntilWaitable(this ref Process p)
        { 
            // The waitid system call expects a pointer to a siginfo_t,
            // which is 128 bytes on all GNU/Linux systems.
            // On Darwin, it requires greater than or equal to 64 bytes
            // for darwin/{386,arm} and 104 bytes for darwin/amd64.
            // We don't care about the values it returns.
            array<ulong> siginfo = new array<ulong>(16L);
            var psig = ref siginfo[0L];
            var (_, _, e) = syscall.Syscall6(syscall.SYS_WAITID, _P_PID, uintptr(p.Pid), uintptr(@unsafe.Pointer(psig)), syscall.WEXITED | syscall.WNOWAIT, 0L, 0L);
            runtime.KeepAlive(p);
            if (e != 0L)
            { 
                // waitid has been available since Linux 2.6.9, but
                // reportedly is not available in Ubuntu on Windows.
                // See issue 16610.
                if (e == syscall.ENOSYS)
                {
                    return (false, null);
                }
                return (false, NewSyscallError("waitid", e));
            }
            return (true, null);
        }
    }
}
