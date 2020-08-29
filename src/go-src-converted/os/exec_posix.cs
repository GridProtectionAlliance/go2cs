// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin dragonfly freebsd linux nacl netbsd openbsd solaris windows

// package os -- go2cs converted at 2020 August 29 08:43:49 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Go\src\os\exec_posix.go
using syscall = go.syscall_package;
using static go.builtin;

namespace go
{
    public static partial class os_package
    {
        // The only signal values guaranteed to be present in the os package
        // on all systems are Interrupt (send the process an interrupt) and
        // Kill (force the process to exit). Interrupt is not implemented on
        // Windows; using it with os.Process.Signal will return an error.
        public static Signal Interrupt = syscall.SIGINT;        public static Signal Kill = syscall.SIGKILL;

        private static (ref Process, error) startProcess(@string name, slice<@string> argv, ref ProcAttr attr)
        { 
            // If there is no SysProcAttr (ie. no Chroot or changed
            // UID/GID), double-check existence of the directory we want
            // to chdir into. We can make the error clearer this way.
            if (attr != null && attr.Sys == null && attr.Dir != "")
            {
                {
                    var (_, err) = Stat(attr.Dir);

                    if (err != null)
                    {
                        ref PathError pe = err._<ref PathError>();
                        pe.Op = "chdir";
                        return (null, pe);
                    }

                }
            }
            syscall.ProcAttr sysattr = ref new syscall.ProcAttr(Dir:attr.Dir,Env:attr.Env,Sys:attr.Sys,);
            if (sysattr.Env == null)
            {
                sysattr.Env = Environ();
            }
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

        private static error kill(this ref Process p)
        {
            return error.As(p.Signal(Kill));
        }

        // ProcessState stores information about a process, as reported by Wait.
        public partial struct ProcessState
        {
            public long pid; // The process's id.
            public syscall.WaitStatus status; // System-dependent status info.
            public ptr<syscall.Rusage> rusage;
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
            return p.rusage;
        }

        private static @string String(this ref ProcessState p)
        {
            if (p == null)
            {
                return "<nil>";
            }
            syscall.WaitStatus status = p.Sys()._<syscall.WaitStatus>();
            @string res = "";

            if (status.Exited()) 
                res = "exit status " + itoa(status.ExitStatus());
            else if (status.Signaled()) 
                res = "signal: " + status.Signal().String();
            else if (status.Stopped()) 
                res = "stop signal: " + status.StopSignal().String();
                if (status.StopSignal() == syscall.SIGTRAP && status.TrapCause() != 0L)
                {
                    res += " (trap " + itoa(status.TrapCause()) + ")";
                }
            else if (status.Continued()) 
                res = "continued";
                        if (status.CoreDump())
            {
                res += " (core dumped)";
            }
            return res;
        }
    }
}
