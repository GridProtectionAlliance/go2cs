// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin dragonfly freebsd linux nacl netbsd openbsd solaris

// package os -- go2cs converted at 2020 August 29 08:43:50 UTC
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
        private static (ref ProcessState, error) wait(this ref Process p)
        {
            if (p.Pid == -1L)
            {
                return (null, syscall.EINVAL);
            }
            var (ready, err) = p.blockUntilWaitable();
            if (err != null)
            {
                return (null, err);
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
            syscall.WaitStatus status = default;
            syscall.Rusage rusage = default;
            var (pid1, e) = syscall.Wait4(p.Pid, ref status, 0L, ref rusage);
            if (e != null)
            {
                return (null, NewSyscallError("wait", e));
            }
            if (pid1 != 0L)
            {
                p.setDone();
            }
            ps = ref new ProcessState(pid:pid1,status:status,rusage:&rusage,);
            return (ps, null);
        }

        private static var errFinished = errors.New("os: process already finished");

        private static error signal(this ref Process _p, Signal sig) => func(_p, (ref Process p, Defer defer, Panic _, Recover __) =>
        {
            if (p.Pid == -1L)
            {
                return error.As(errors.New("os: process already released"));
            }
            if (p.Pid == 0L)
            {
                return error.As(errors.New("os: process not initialized"));
            }
            p.sigMu.RLock();
            defer(p.sigMu.RUnlock());
            if (p.done())
            {
                return error.As(errFinished);
            }
            syscall.Signal (s, ok) = sig._<syscall.Signal>();
            if (!ok)
            {
                return error.As(errors.New("os: unsupported signal type"));
            }
            {
                var e = syscall.Kill(p.Pid, s);

                if (e != null)
                {
                    if (e == syscall.ESRCH)
                    {
                        return error.As(errFinished);
                    }
                    return error.As(e);
                }

            }
            return error.As(null);
        });

        private static error release(this ref Process p)
        { 
            // NOOP for unix.
            p.Pid = -1L; 
            // no need for a finalizer anymore
            runtime.SetFinalizer(p, null);
            return error.As(null);
        }

        private static (ref Process, error) findProcess(long pid)
        { 
            // NOOP for unix.
            return (newProcess(pid, 0L), null);
        }

        private static time.Duration userTime(this ref ProcessState p)
        {
            return time.Duration(p.rusage.Utime.Nano()) * time.Nanosecond;
        }

        private static time.Duration systemTime(this ref ProcessState p)
        {
            return time.Duration(p.rusage.Stime.Nano()) * time.Nanosecond;
        }
    }
}
