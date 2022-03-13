// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build aix || darwin || dragonfly || freebsd || (js && wasm) || linux || netbsd || openbsd || solaris
// +build aix darwin dragonfly freebsd js,wasm linux netbsd openbsd solaris

// package os -- go2cs converted at 2022 March 13 05:27:56 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Program Files\Go\src\os\exec_unix.go
namespace go;

using errors = errors_package;
using runtime = runtime_package;
using syscall = syscall_package;
using time = time_package;

public static partial class os_package {

private static (ptr<ProcessState>, error) wait(this ptr<Process> _addr_p) {
    ptr<ProcessState> ps = default!;
    error err = default!;
    ref Process p = ref _addr_p.val;

    if (p.Pid == -1) {
        return (_addr_null!, error.As(syscall.EINVAL)!);
    }
    var (ready, err) = p.blockUntilWaitable();
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    if (ready) { 
        // Mark the process done now, before the call to Wait4,
        // so that Process.signal will not send a signal.
        p.setDone(); 
        // Acquire a write lock on sigMu to wait for any
        // active call to the signal method to complete.
        p.sigMu.Lock();
        p.sigMu.Unlock();
    }
    ref syscall.WaitStatus status = ref heap(out ptr<syscall.WaitStatus> _addr_status);    ref syscall.Rusage rusage = ref heap(out ptr<syscall.Rusage> _addr_rusage);    nint pid1 = default;    error e = default!;
    while (true) {
        pid1, e = syscall.Wait4(p.Pid, _addr_status, 0, _addr_rusage);
        if (e != syscall.EINTR) {
            break;
        }
    }
    if (e != null) {
        return (_addr_null!, error.As(NewSyscallError("wait", e))!);
    }
    if (pid1 != 0) {
        p.setDone();
    }
    ps = addr(new ProcessState(pid:pid1,status:status,rusage:&rusage,));
    return (_addr_ps!, error.As(null!)!);
}

private static error signal(this ptr<Process> _addr_p, Signal sig) => func((defer, _, _) => {
    ref Process p = ref _addr_p.val;

    if (p.Pid == -1) {
        return error.As(errors.New("os: process already released"))!;
    }
    if (p.Pid == 0) {
        return error.As(errors.New("os: process not initialized"))!;
    }
    p.sigMu.RLock();
    defer(p.sigMu.RUnlock());
    if (p.done()) {
        return error.As(ErrProcessDone)!;
    }
    syscall.Signal (s, ok) = sig._<syscall.Signal>();
    if (!ok) {
        return error.As(errors.New("os: unsupported signal type"))!;
    }
    {
        var e = syscall.Kill(p.Pid, s);

        if (e != null) {
            if (e == syscall.ESRCH) {
                return error.As(ErrProcessDone)!;
            }
            return error.As(e)!;
        }
    }
    return error.As(null!)!;
});

private static error release(this ptr<Process> _addr_p) {
    ref Process p = ref _addr_p.val;
 
    // NOOP for unix.
    p.Pid = -1; 
    // no need for a finalizer anymore
    runtime.SetFinalizer(p, null);
    return error.As(null!)!;
}

private static (ptr<Process>, error) findProcess(nint pid) {
    ptr<Process> p = default!;
    error err = default!;
 
    // NOOP for unix.
    return (_addr_newProcess(pid, 0)!, error.As(null!)!);
}

private static time.Duration userTime(this ptr<ProcessState> _addr_p) {
    ref ProcessState p = ref _addr_p.val;

    return time.Duration(p.rusage.Utime.Nano()) * time.Nanosecond;
}

private static time.Duration systemTime(this ptr<ProcessState> _addr_p) {
    ref ProcessState p = ref _addr_p.val;

    return time.Duration(p.rusage.Stime.Nano()) * time.Nanosecond;
}

} // end os_package
