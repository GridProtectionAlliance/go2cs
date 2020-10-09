// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package os -- go2cs converted at 2020 October 09 05:07:08 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Go\src\os\exec_windows.go
using errors = go.errors_package;
using windows = go.@internal.syscall.windows_package;
using runtime = go.runtime_package;
using atomic = go.sync.atomic_package;
using syscall = go.syscall_package;
using time = go.time_package;
using static go.builtin;

namespace go
{
    public static partial class os_package
    {
        private static (ptr<ProcessState>, error) wait(this ptr<Process> _addr_p) => func((defer, _, __) =>
        {
            ptr<ProcessState> ps = default!;
            error err = default!;
            ref Process p = ref _addr_p.val;

            var handle = atomic.LoadUintptr(_addr_p.handle);
            var (s, e) = syscall.WaitForSingleObject(syscall.Handle(handle), syscall.INFINITE);

            if (s == syscall.WAIT_OBJECT_0) 
                break;
            else if (s == syscall.WAIT_FAILED) 
                return (_addr_null!, error.As(NewSyscallError("WaitForSingleObject", e))!);
            else 
                return (_addr_null!, error.As(errors.New("os: unexpected result from WaitForSingleObject"))!);
                        ref uint ec = ref heap(out ptr<uint> _addr_ec);
            e = syscall.GetExitCodeProcess(syscall.Handle(handle), _addr_ec);
            if (e != null)
            {
                return (_addr_null!, error.As(NewSyscallError("GetExitCodeProcess", e))!);
            }
            ref syscall.Rusage u = ref heap(out ptr<syscall.Rusage> _addr_u);
            e = syscall.GetProcessTimes(syscall.Handle(handle), _addr_u.CreationTime, _addr_u.ExitTime, _addr_u.KernelTime, _addr_u.UserTime);
            if (e != null)
            {
                return (_addr_null!, error.As(NewSyscallError("GetProcessTimes", e))!);
            }
            p.setDone(); 
            // NOTE(brainman): It seems that sometimes process is not dead
            // when WaitForSingleObject returns. But we do not know any
            // other way to wait for it. Sleeping for a while seems to do
            // the trick sometimes.
            // See https://golang.org/issue/25965 for details.
            defer(time.Sleep(5L * time.Millisecond));
            defer(p.Release());
            return (addr(new ProcessState(p.Pid,syscall.WaitStatus{ExitCode:ec},&u)), error.As(null!)!);

        });

        private static error terminateProcess(long pid, long exitcode) => func((defer, _, __) =>
        {
            var (h, e) = syscall.OpenProcess(syscall.PROCESS_TERMINATE, false, uint32(pid));
            if (e != null)
            {
                return error.As(NewSyscallError("OpenProcess", e))!;
            }

            defer(syscall.CloseHandle(h));
            e = syscall.TerminateProcess(h, uint32(exitcode));
            return error.As(NewSyscallError("TerminateProcess", e))!;

        });

        private static error signal(this ptr<Process> _addr_p, Signal sig)
        {
            ref Process p = ref _addr_p.val;

            var handle = atomic.LoadUintptr(_addr_p.handle);
            if (handle == uintptr(syscall.InvalidHandle))
            {
                return error.As(syscall.EINVAL)!;
            }

            if (p.done())
            {
                return error.As(errors.New("os: process already finished"))!;
            }

            if (sig == Kill)
            {
                var err = terminateProcess(p.Pid, 1L);
                runtime.KeepAlive(p);
                return error.As(err)!;
            } 
            // TODO(rsc): Handle Interrupt too?
            return error.As(syscall.Errno(syscall.EWINDOWS))!;

        }

        private static error release(this ptr<Process> _addr_p)
        {
            ref Process p = ref _addr_p.val;

            var handle = atomic.LoadUintptr(_addr_p.handle);
            if (handle == uintptr(syscall.InvalidHandle))
            {
                return error.As(syscall.EINVAL)!;
            }

            var e = syscall.CloseHandle(syscall.Handle(handle));
            if (e != null)
            {
                return error.As(NewSyscallError("CloseHandle", e))!;
            }

            atomic.StoreUintptr(_addr_p.handle, uintptr(syscall.InvalidHandle)); 
            // no need for a finalizer anymore
            runtime.SetFinalizer(p, null);
            return error.As(null!)!;

        }

        private static (ptr<Process>, error) findProcess(long pid)
        {
            ptr<Process> p = default!;
            error err = default!;

            const var da = syscall.STANDARD_RIGHTS_READ | syscall.PROCESS_QUERY_INFORMATION | syscall.SYNCHRONIZE;

            var (h, e) = syscall.OpenProcess(da, false, uint32(pid));
            if (e != null)
            {
                return (_addr_null!, error.As(NewSyscallError("OpenProcess", e))!);
            }

            return (_addr_newProcess(pid, uintptr(h))!, error.As(null!)!);

        }

        private static void init()
        {
            var cmd = windows.UTF16PtrToString(syscall.GetCommandLine());
            if (len(cmd) == 0L)
            {
                var (arg0, _) = Executable();
                Args = new slice<@string>(new @string[] { arg0 });
            }
            else
            {
                Args = commandLineToArgv(cmd);
            }

        }

        // appendBSBytes appends n '\\' bytes to b and returns the resulting slice.
        private static slice<byte> appendBSBytes(slice<byte> b, long n)
        {
            while (n > 0L)
            {
                b = append(b, '\\');
                n--;
            }

            return b;

        }

        // readNextArg splits command line string cmd into next
        // argument and command line remainder.
        private static (slice<byte>, @string) readNextArg(@string cmd)
        {
            slice<byte> arg = default;
            @string rest = default;

            slice<byte> b = default;
            bool inquote = default;
            long nslash = default;
            while (len(cmd) > 0L)
            {
                var c = cmd[0L];
                switch (c)
                {
                    case ' ': 

                    case '\t': 
                                        if (!inquote)
                                        {
                                            return (appendBSBytes(b, nslash), cmd[1L..]);
                                    cmd = cmd[1L..];
                                        }

                        break;
                    case '"': 
                        b = appendBSBytes(b, nslash / 2L);
                        if (nslash % 2L == 0L)
                        { 
                            // use "Prior to 2008" rule from
                            // http://daviddeley.com/autohotkey/parameters/parameters.htm
                            // section 5.2 to deal with double double quotes
                            if (inquote && len(cmd) > 1L && cmd[1L] == '"')
                            {
                                b = append(b, c);
                                cmd = cmd[1L..];
                            }

                            inquote = !inquote;

                        }
                        else
                        {
                            b = append(b, c);
                        }

                        nslash = 0L;
                        continue;
                        break;
                    case '\\': 
                        nslash++;
                        continue;
                        break;
                }
                b = appendBSBytes(b, nslash);
                nslash = 0L;
                b = append(b, c);

            }

            return (appendBSBytes(b, nslash), "");

        }

        // commandLineToArgv splits a command line into individual argument
        // strings, following the Windows conventions documented
        // at http://daviddeley.com/autohotkey/parameters/parameters.htm#WINARGV
        private static slice<@string> commandLineToArgv(@string cmd)
        {
            slice<@string> args = default;
            while (len(cmd) > 0L)
            {
                if (cmd[0L] == ' ' || cmd[0L] == '\t')
                {
                    cmd = cmd[1L..];
                    continue;
                }

                slice<byte> arg = default;
                arg, cmd = readNextArg(cmd);
                args = append(args, string(arg));

            }

            return args;

        }

        private static time.Duration ftToDuration(ptr<syscall.Filetime> _addr_ft)
        {
            ref syscall.Filetime ft = ref _addr_ft.val;

            var n = int64(ft.HighDateTime) << (int)(32L) + int64(ft.LowDateTime); // in 100-nanosecond intervals
            return time.Duration(n * 100L) * time.Nanosecond;

        }

        private static time.Duration userTime(this ptr<ProcessState> _addr_p)
        {
            ref ProcessState p = ref _addr_p.val;

            return ftToDuration(_addr_p.rusage.UserTime);
        }

        private static time.Duration systemTime(this ptr<ProcessState> _addr_p)
        {
            ref ProcessState p = ref _addr_p.val;

            return ftToDuration(_addr_p.rusage.KernelTime);
        }
    }
}
