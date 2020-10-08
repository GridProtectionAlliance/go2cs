// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package os -- go2cs converted at 2020 October 08 03:44:30 UTC
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

        private static (ptr<Process>, error) startProcess(@string name, slice<@string> argv, ptr<ProcAttr> _addr_attr)
        {
            ptr<Process> p = default!;
            error err = default!;
            ref ProcAttr attr = ref _addr_attr.val;

            ptr<syscall.ProcAttr> sysattr = addr(new syscall.ProcAttr(Dir:attr.Dir,Env:attr.Env,Sys:attr.Sys,));

            sysattr.Files = make_slice<System.UIntPtr>(0L, len(attr.Files));
            foreach (var (_, f) in attr.Files)
            {
                sysattr.Files = append(sysattr.Files, f.Fd());
            }
            var (pid, h, e) = syscall.StartProcess(name, argv, sysattr);
            if (e != null)
            {
                return (_addr_null!, error.As(addr(new PathError("fork/exec",name,e))!)!);
            }

            return (_addr_newProcess(pid, h)!, error.As(null!)!);

        }

        private static error writeProcFile(this ptr<Process> _addr_p, @string file, @string data) => func((defer, _, __) =>
        {
            ref Process p = ref _addr_p.val;

            var (f, e) = OpenFile("/proc/" + itoa(p.Pid) + "/" + file, O_WRONLY, 0L);
            if (e != null)
            {
                return error.As(e)!;
            }

            defer(f.Close());
            _, e = f.Write((slice<byte>)data);
            return error.As(e)!;

        });

        private static error signal(this ptr<Process> _addr_p, Signal sig)
        {
            ref Process p = ref _addr_p.val;

            if (p.done())
            {
                return error.As(errors.New("os: process already finished"))!;
            }

            {
                var e = p.writeProcFile("note", sig.String());

                if (e != null)
                {
                    return error.As(NewSyscallError("signal", e))!;
                }

            }

            return error.As(null!)!;

        }

        private static error kill(this ptr<Process> _addr_p)
        {
            ref Process p = ref _addr_p.val;

            return error.As(p.signal(Kill))!;
        }

        private static (ptr<ProcessState>, error) wait(this ptr<Process> _addr_p)
        {
            ptr<ProcessState> ps = default!;
            error err = default!;
            ref Process p = ref _addr_p.val;

            ref syscall.Waitmsg waitmsg = ref heap(out ptr<syscall.Waitmsg> _addr_waitmsg);

            if (p.Pid == -1L)
            {
                return (_addr_null!, error.As(ErrInvalid)!);
            }

            err = syscall.WaitProcess(p.Pid, _addr_waitmsg);
            if (err != null)
            {
                return (_addr_null!, error.As(NewSyscallError("wait", err))!);
            }

            p.setDone();
            ps = addr(new ProcessState(pid:waitmsg.Pid,status:&waitmsg,));
            return (_addr_ps!, error.As(null!)!);

        }

        private static error release(this ptr<Process> _addr_p)
        {
            ref Process p = ref _addr_p.val;
 
            // NOOP for Plan 9.
            p.Pid = -1L; 
            // no need for a finalizer anymore
            runtime.SetFinalizer(p, null);
            return error.As(null!)!;

        }

        private static (ptr<Process>, error) findProcess(long pid)
        {
            ptr<Process> p = default!;
            error err = default!;
 
            // NOOP for Plan 9.
            return (_addr_newProcess(pid, 0L)!, error.As(null!)!);

        }

        // ProcessState stores information about a process, as reported by Wait.
        public partial struct ProcessState
        {
            public long pid; // The process's id.
            public ptr<syscall.Waitmsg> status; // System-dependent status info.
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

            return p.status;
        }

        private static time.Duration userTime(this ptr<ProcessState> _addr_p)
        {
            ref ProcessState p = ref _addr_p.val;

            return time.Duration(p.status.Time[0L]) * time.Millisecond;
        }

        private static time.Duration systemTime(this ptr<ProcessState> _addr_p)
        {
            ref ProcessState p = ref _addr_p.val;

            return time.Duration(p.status.Time[1L]) * time.Millisecond;
        }

        private static @string String(this ptr<ProcessState> _addr_p)
        {
            ref ProcessState p = ref _addr_p.val;

            if (p == null)
            {
                return "<nil>";
            }

            return "exit status: " + p.status.Msg;

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
