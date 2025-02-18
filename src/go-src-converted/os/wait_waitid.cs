// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// We used to used this code for Darwin, but according to issue #19314
// waitid returns if the process is stopped, even when using WEXITED.

//go:build linux
// +build linux

// package os -- go2cs converted at 2022 March 13 05:28:06 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Program Files\Go\src\os\wait_waitid.go
namespace go;

using runtime = runtime_package;
using syscall = syscall_package;
using @unsafe = @unsafe_package;

public static partial class os_package {

private static readonly nint _P_PID = 1;

// blockUntilWaitable attempts to block until a call to p.Wait will
// succeed immediately, and reports whether it has done so.
// It does not actually call p.Wait.


// blockUntilWaitable attempts to block until a call to p.Wait will
// succeed immediately, and reports whether it has done so.
// It does not actually call p.Wait.
private static (bool, error) blockUntilWaitable(this ptr<Process> _addr_p) {
    bool _p0 = default;
    error _p0 = default!;
    ref Process p = ref _addr_p.val;
 
    // The waitid system call expects a pointer to a siginfo_t,
    // which is 128 bytes on all Linux systems.
    // On darwin/amd64, it requires 104 bytes.
    // We don't care about the values it returns.
    array<ulong> siginfo = new array<ulong>(16);
    var psig = _addr_siginfo[0];
    syscall.Errno e = default;
    while (true) {
        _, _, e = syscall.Syscall6(syscall.SYS_WAITID, _P_PID, uintptr(p.Pid), uintptr(@unsafe.Pointer(psig)), syscall.WEXITED | syscall.WNOWAIT, 0, 0);
        if (e != syscall.EINTR) {
            break;
        }
    }
    runtime.KeepAlive(p);
    if (e != 0) { 
        // waitid has been available since Linux 2.6.9, but
        // reportedly is not available in Ubuntu on Windows.
        // See issue 16610.
        if (e == syscall.ENOSYS) {
            return (false, error.As(null!)!);
        }
        return (false, error.As(NewSyscallError("waitid", e))!);
    }
    return (true, error.As(null!)!);
}

} // end os_package
