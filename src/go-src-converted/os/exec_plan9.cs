// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package os -- go2cs converted at 2020 August 29 08:43:47 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Go\src\os\exec_plan9.go
using errors = go.errors_package;
using runtime = go.runtime_package;
using syscall = go.syscall_package;
using time = go.time_package;
using static go.builtin;

namespace go
{
    public static partial class os_package
    {
        // The only signal values guaranteed to be present in the os package
        // on all systems are Interrupt (send the process an interrupt) and
        // Kill (force the process to exit). Interrupt is not implemented on
        // Windows; using it with os.Process.Signal will return an error.
        public static Signal Interrupt = syscall.Note("interrupt");        public static Signal Kill = syscall.Note("kill");

        private static (ref Process, error) startProcess(@string name, slice<@string> argv, ref ProcAttr attr)
        {
            syscall.ProcAttr sysattr = ref new syscall.ProcAttr(Dir:attr.Dir,Env:attr.Env,Sys:attr.Sys,);

            foreach (var (_, f) in attr.Files)
            {
                sysattr.Files = append(sysattr.Files, f.Fd());
            }
            var (pid, h, e) = syscall.StartProcess(name, argv, sysattr);
            if (e != null)
            {
                return (null, ref new PathError("fork/exec",name,e));
            }
            return (newProcess(pid, h), null);
        }

        private static error writeProcFile(this ref Process _p, @string file, @string data) => func(_p, (ref Process p, Defer defer, Panic _, Recover __) =>
        {
            var (f, e) = OpenFile("/proc/" + itoa(p.Pid) + "/" + file, O_WRONLY, 0L);
            if (e != null)
            {
                return error.As(e);
            }
            defer(f.Close());
            _, e = f.Write((slice<byte>)data);
            return error.As(e);
        });

        private static error signal(this ref Process p, Signal sig)
        {
            if (p.done())
            {
                return error.As(errors.New("os: process already finished"));
            }
            {
                var e = p.writeProcFile("note", sig.String());

                if (e != null)
                {
                    return error.As(NewSyscallError("signal", e));
                }

            }
            return error.As(null);
        }

        private static error kill(this ref Process p)
        {
            return error.As(p.signal(Kill));
        }

        private static (ref ProcessState, error) wait(this ref Process p)
        {
            syscall.Waitmsg waitmsg = default;

            if (p.Pid == -1L)
            {
                return (null, ErrInvalid);
            }
            err = syscall.WaitProcess(p.Pid, ref waitmsg);
            if (err != null)
            {
                return (null, NewSyscallError("wait", err));
            }
            p.setDone();
            ps = ref new ProcessState(pid:waitmsg.Pid,status:&waitmsg,);
            return (ps, null);
        }

        private static error release(this ref Process p)
        { 
            // NOOP for Plan 9.
            p.Pid = -1L; 
            // no need for a finalizer anymore
            runtime.SetFinalizer(p, null);
            return error.As(null);
        }

        private static (ref Process, error) findProcess(long pid)
        { 
            // NOOP for Plan 9.
            return (newProcess(pid, 0L), null);
        }

        // ProcessState stores information about a process, as reported by Wait.
        public partial struct ProcessState
        {
            public long pid; // The process's id.
            public ptr<syscall.Waitmsg> status; // System-dependent status info.
        }

        // Pid returns the process id of the exited process.
        private static long Pid(this ref ProcessState p)
        {
            return p.pid;
        }

        private static bool exited(this ref ProcessState p)
        {
            return p.status.Exited();
        }

        private static bool success(this ref ProcessState p)
        {
            return p.status.ExitStatus() == 0L;
        }

        private static void sys(this ref ProcessState p)
        {
            return p.status;
        }

        private static void sysUsage(this ref ProcessState p)
        {
            return p.status;
        }

        private static time.Duration userTime(this ref ProcessState p)
        {
            return time.Duration(p.status.Time[0L]) * time.Millisecond;
        }

        private static time.Duration systemTime(this ref ProcessState p)
        {
            return time.Duration(p.status.Time[1L]) * time.Millisecond;
        }

        private static @string String(this ref ProcessState p)
        {
            if (p == null)
            {
                return "<nil>";
            }
            return "exit status: " + p.status.Msg;
        }
    }
}
