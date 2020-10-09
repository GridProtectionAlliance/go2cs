// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build aix darwin dragonfly freebsd js,wasm linux netbsd openbsd solaris

// package os -- go2cs converted at 2020 October 09 05:07:07 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Go\src\os\exec_unix.go
using errors = go.errors_package;
using runtime = go.runtime_package;
using syscall = go.syscall_package;
using time = go.time_package;
using static go.builtin;

namespace go
{
    public static partial class os_package
    {
        private static (ptr<ProcessState>, error) wait(this ptr<Process> _addr_p)
        {
            ptr<ProcessState> ps = default!;
            error err = default!;
            ref Process p = ref _addr_p.val;

            if (p.Pid == -1L)
            {
                return (_addr_null!, error.As(syscall.EINVAL)!);
            }
            var (ready, err) = p.blockUntilWaitable();
            if (err != null)
            {
                return (_addr_null!, error.As(err)!);
            }
            if (ready)
            { 
                // Mark the process done now, before the call to Wait4,
                // so that Process.signal will not send a signal.
                p.setDone(); 
                // Acquire a write lock on sigMu to wait for any
                // active call to the signal method to complete.
                p.sigMu.Lock();
                p.sigMu.Unlock();

            }
            ref syscall.WaitStatus status = ref heap(out ptr<syscall.WaitStatus> _addr_status);            ref syscall.Rusage rusage = ref heap(out ptr<syscall.Rusage> _addr_rusage);            long pid1 = default;            error e = default!;
            while (true)
            {
                pid1, e = syscall.Wait4(p.Pid, _addr_status, 0L, _addr_rusage);
                if (e != syscall.EINTR)
                {
                    break;
                }
            }
            if (e != null)
            {
                return (_addr_null!, error.As(NewSyscallError("wait", e))!);
            }
            if (pid1 != 0L)
            {
                p.setDone();
            }
            ps = addr(new ProcessState(pid:pid1,status:status,rusage:&rusage,));
            return (_addr_ps!, error.As(null!)!);

        }

        private static var errFinished = errors.New("os: process already finished");

        private static error signal(this ptr<Process> _addr_p, Signal sig) => func((defer, _, __) =>
        {
            ref Process p = ref _addr_p.val;

            if (p.Pid == -1L)
            {
                return error.As(errors.New("os: process already released"))!;
            }

            if (p.Pid == 0L)
            {
                return error.As(errors.New("os: process not initialized"))!;
            }

            p.sigMu.RLock();
            defer(p.sigMu.RUnlock());
            if (p.done())
            {
                return error.As(errFinished)!;
            }

            syscall.Signal (s, ok) = sig._<syscall.Signal>();
            if (!ok)
            {
                return error.As(errors.New("os: unsupported signal type"))!;
            }

            {
                var e = syscall.Kill(p.Pid, s);

                if (e != null)
                {
                    if (e == syscall.ESRCH)
                    {
                        return error.As(errFinished)!;
                    }

                    return error.As(e)!;

                }

            }

            return error.As(null!)!;

        });

        private static error release(this ptr<Process> _addr_p)
        {
            ref Process p = ref _addr_p.val;
 
            // NOOP for unix.
            p.Pid = -1L; 
            // no need for a finalizer anymore
            runtime.SetFinalizer(p, null);
            return error.As(null!)!;

        }

        private static (ptr<Process>, error) findProcess(long pid)
        {
            ptr<Process> p = default!;
            error err = default!;
 
            // NOOP for unix.
            return (_addr_newProcess(pid, 0L)!, error.As(null!)!);

        }

        private static time.Duration userTime(this ptr<ProcessState> _addr_p)
        {
            ref ProcessState p = ref _addr_p.val;

            return time.Duration(p.rusage.Utime.Nano()) * time.Nanosecond;
        }

        private static time.Duration systemTime(this ptr<ProcessState> _addr_p)
        {
            ref ProcessState p = ref _addr_p.val;

            return time.Duration(p.rusage.Stime.Nano()) * time.Nanosecond;
        }
    }
}
