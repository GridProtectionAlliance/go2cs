// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build aix darwin dragonfly freebsd js,wasm linux netbsd openbsd solaris windows

// package os -- go2cs converted at 2020 October 08 03:44:32 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Go\src\os\exec_posix.go
using execenv = go.@internal.syscall.execenv_package;
using runtime = go.runtime_package;
using syscall = go.syscall_package;
using static go.builtin;

namespace go
{
    public static partial class os_package
    {
        // The only signal values guaranteed to be present in the os package on all
        // systems are os.Interrupt (send the process an interrupt) and os.Kill (force
        // the process to exit). On Windows, sending os.Interrupt to a process with
        // os.Process.Signal is not implemented; it will return an error instead of
        // sending a signal.
        public static Signal Interrupt = syscall.SIGINT;        public static Signal Kill = syscall.SIGKILL;

        private static (ptr<Process>, error) startProcess(@string name, slice<@string> argv, ptr<ProcAttr> _addr_attr)
        {
            ptr<Process> p = default!;
            error err = default!;
            ref ProcAttr attr = ref _addr_attr.val;
 
            // If there is no SysProcAttr (ie. no Chroot or changed
            // UID/GID), double-check existence of the directory we want
            // to chdir into. We can make the error clearer this way.
            if (attr != null && attr.Sys == null && attr.Dir != "")
            {
                {
                    var (_, err) = Stat(attr.Dir);

                    if (err != null)
                    {
                        ptr<PathError> pe = err._<ptr<PathError>>();
                        pe.Op = "chdir";
                        return (_addr_null!, error.As(pe)!);
                    }

                }

            }

            ptr<syscall.ProcAttr> sysattr = addr(new syscall.ProcAttr(Dir:attr.Dir,Env:attr.Env,Sys:attr.Sys,));
            if (sysattr.Env == null)
            {
                sysattr.Env, err = execenv.Default(sysattr.Sys);
                if (err != null)
                {
                    return (_addr_null!, error.As(err)!);
                }

            }

            sysattr.Files = make_slice<System.UIntPtr>(0L, len(attr.Files));
            foreach (var (_, f) in attr.Files)
            {
                sysattr.Files = append(sysattr.Files, f.Fd());
            }
            var (pid, h, e) = syscall.StartProcess(name, argv, sysattr); 

            // Make sure we don't run the finalizers of attr.Files.
            runtime.KeepAlive(attr);

            if (e != null)
            {
                return (_addr_null!, error.As(addr(new PathError("fork/exec",name,e))!)!);
            }

            return (_addr_newProcess(pid, h)!, error.As(null!)!);

        }

        private static error kill(this ptr<Process> _addr_p)
        {
            ref Process p = ref _addr_p.val;

            return error.As(p.Signal(Kill))!;
        }

        // ProcessState stores information about a process, as reported by Wait.
        public partial struct ProcessState
        {
            public long pid; // The process's id.
            public syscall.WaitStatus status; // System-dependent status info.
            public ptr<syscall.Rusage> rusage;
        }

        // Pid returns the process id of the exited process.
        private static long Pid(this ptr<ProcessState> _addr_p)
        {
            ref ProcessState p = ref _addr_p.val;

            return p.pid;
        }

        private static bool exited(this ptr<ProcessState> _addr_p)
        {
            ref ProcessState p = ref _addr_p.val;

            return p.status.Exited();
        }

        private static bool success(this ptr<ProcessState> _addr_p)
        {
            ref ProcessState p = ref _addr_p.val;

            return p.status.ExitStatus() == 0L;
        }

        private static void sys(this ptr<ProcessState> _addr_p)
        {
            ref ProcessState p = ref _addr_p.val;

            return p.status;
        }

        private static void sysUsage(this ptr<ProcessState> _addr_p)
        {
            ref ProcessState p = ref _addr_p.val;

            return p.rusage;
        }

        private static @string String(this ptr<ProcessState> _addr_p)
        {
            ref ProcessState p = ref _addr_p.val;

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

        // ExitCode returns the exit code of the exited process, or -1
        // if the process hasn't exited or was terminated by a signal.
        private static long ExitCode(this ptr<ProcessState> _addr_p)
        {
            ref ProcessState p = ref _addr_p.val;
 
            // return -1 if the process hasn't started.
            if (p == null)
            {
                return -1L;
            }

            return p.status.ExitStatus();

        }
    }
}
