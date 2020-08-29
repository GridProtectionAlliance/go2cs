// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package os -- go2cs converted at 2020 August 29 08:43:40 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Go\src\os\exec.go
using testlog = go.@internal.testlog_package;
using runtime = go.runtime_package;
using sync = go.sync_package;
using atomic = go.sync.atomic_package;
using syscall = go.syscall_package;
using time = go.time_package;
using static go.builtin;

namespace go
{
    public static partial class os_package
    {
        // Process stores the information about a process created by StartProcess.
        public partial struct Process
        {
            public long Pid;
            public System.UIntPtr handle; // handle is accessed atomically on Windows
            public uint isdone; // process has been successfully waited on, non zero if true
            public sync.RWMutex sigMu; // avoid race between wait and signal
        }

        private static ref Process newProcess(long pid, System.UIntPtr handle)
        {
            Process p = ref new Process(Pid:pid,handle:handle);
            runtime.SetFinalizer(p, ref Process);
            return p;
        }

        private static void setDone(this ref Process p)
        {
            atomic.StoreUint32(ref p.isdone, 1L);
        }

        private static bool done(this ref Process p)
        {
            return atomic.LoadUint32(ref p.isdone) > 0L;
        }

        // ProcAttr holds the attributes that will be applied to a new process
        // started by StartProcess.
        public partial struct ProcAttr
        {
            public @string Dir; // If Env is non-nil, it gives the environment variables for the
// new process in the form returned by Environ.
// If it is nil, the result of Environ will be used.
            public slice<@string> Env; // Files specifies the open files inherited by the new process. The
// first three entries correspond to standard input, standard output, and
// standard error. An implementation may support additional entries,
// depending on the underlying operating system. A nil entry corresponds
// to that file being closed when the process starts.
            public slice<ref File> Files; // Operating system-specific process creation attributes.
// Note that setting this field means that your program
// may not execute properly or even compile on some
// operating systems.
            public ptr<syscall.SysProcAttr> Sys;
        }

        // A Signal represents an operating system signal.
        // The usual underlying implementation is operating system-dependent:
        // on Unix it is syscall.Signal.
        public partial interface Signal
        {
            @string String();
            @string Signal(); // to distinguish from other Stringers
        }

        // Getpid returns the process id of the caller.
        public static long Getpid()
        {
            return syscall.Getpid();
        }

        // Getppid returns the process id of the caller's parent.
        public static long Getppid()
        {
            return syscall.Getppid();
        }

        // FindProcess looks for a running process by its pid.
        //
        // The Process it returns can be used to obtain information
        // about the underlying operating system process.
        //
        // On Unix systems, FindProcess always succeeds and returns a Process
        // for the given pid, regardless of whether the process exists.
        public static (ref Process, error) FindProcess(long pid)
        {
            return findProcess(pid);
        }

        // StartProcess starts a new process with the program, arguments and attributes
        // specified by name, argv and attr. The argv slice will become os.Args in the
        // new process, so it normally starts with the program name.
        //
        // If the calling goroutine has locked the operating system thread
        // with runtime.LockOSThread and modified any inheritable OS-level
        // thread state (for example, Linux or Plan 9 name spaces), the new
        // process will inherit the caller's thread state.
        //
        // StartProcess is a low-level interface. The os/exec package provides
        // higher-level interfaces.
        //
        // If there is an error, it will be of type *PathError.
        public static (ref Process, error) StartProcess(@string name, slice<@string> argv, ref ProcAttr attr)
        {
            testlog.Open(name);
            return startProcess(name, argv, attr);
        }

        // Release releases any resources associated with the Process p,
        // rendering it unusable in the future.
        // Release only needs to be called if Wait is not.
        private static error Release(this ref Process p)
        {
            return error.As(p.release());
        }

        // Kill causes the Process to exit immediately.
        private static error Kill(this ref Process p)
        {
            return error.As(p.kill());
        }

        // Wait waits for the Process to exit, and then returns a
        // ProcessState describing its status and an error, if any.
        // Wait releases any resources associated with the Process.
        // On most operating systems, the Process must be a child
        // of the current process or an error will be returned.
        private static (ref ProcessState, error) Wait(this ref Process p)
        {
            return p.wait();
        }

        // Signal sends a signal to the Process.
        // Sending Interrupt on Windows is not implemented.
        private static error Signal(this ref Process p, Signal sig)
        {
            return error.As(p.signal(sig));
        }

        // UserTime returns the user CPU time of the exited process and its children.
        private static time.Duration UserTime(this ref ProcessState p)
        {
            return p.userTime();
        }

        // SystemTime returns the system CPU time of the exited process and its children.
        private static time.Duration SystemTime(this ref ProcessState p)
        {
            return p.systemTime();
        }

        // Exited reports whether the program has exited.
        private static bool Exited(this ref ProcessState p)
        {
            return p.exited();
        }

        // Success reports whether the program exited successfully,
        // such as with exit status 0 on Unix.
        private static bool Success(this ref ProcessState p)
        {
            return p.success();
        }

        // Sys returns system-dependent exit information about
        // the process. Convert it to the appropriate underlying
        // type, such as syscall.WaitStatus on Unix, to access its contents.
        private static void Sys(this ref ProcessState p)
        {
            return p.sys();
        }

        // SysUsage returns system-dependent resource usage information about
        // the exited process. Convert it to the appropriate underlying
        // type, such as *syscall.Rusage on Unix, to access its contents.
        // (On Unix, *syscall.Rusage matches struct rusage as defined in the
        // getrusage(2) manual page.)
        private static void SysUsage(this ref ProcessState p)
        {
            return p.sysUsage();
        }
    }
}
