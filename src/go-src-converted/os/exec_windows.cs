// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package os -- go2cs converted at 2022 March 06 22:13:31 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Program Files\Go\src\os\exec_windows.go
using errors = go.errors_package;
using windows = go.@internal.syscall.windows_package;
using runtime = go.runtime_package;
using atomic = go.sync.atomic_package;
using syscall = go.syscall_package;
using time = go.time_package;

namespace go;

public static partial class os_package {

private static (ptr<ProcessState>, error) wait(this ptr<Process> _addr_p) => func((defer, _, _) => {
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
    if (e != null) {
        return (_addr_null!, error.As(NewSyscallError("GetExitCodeProcess", e))!);
    }
    ref syscall.Rusage u = ref heap(out ptr<syscall.Rusage> _addr_u);
    e = syscall.GetProcessTimes(syscall.Handle(handle), _addr_u.CreationTime, _addr_u.ExitTime, _addr_u.KernelTime, _addr_u.UserTime);
    if (e != null) {
        return (_addr_null!, error.As(NewSyscallError("GetProcessTimes", e))!);
    }
    p.setDone(); 
    // NOTE(brainman): It seems that sometimes process is not dead
    // when WaitForSingleObject returns. But we do not know any
    // other way to wait for it. Sleeping for a while seems to do
    // the trick sometimes.
    // See https://golang.org/issue/25965 for details.
    defer(time.Sleep(5 * time.Millisecond));
    defer(p.Release());
    return (addr(new ProcessState(p.Pid,syscall.WaitStatus{ExitCode:ec},&u)), error.As(null!)!);

});

private static error signal(this ptr<Process> _addr_p, Signal sig) => func((defer, _, _) => {
    ref Process p = ref _addr_p.val;

    var handle = atomic.LoadUintptr(_addr_p.handle);
    if (handle == uintptr(syscall.InvalidHandle)) {
        return error.As(syscall.EINVAL)!;
    }
    if (p.done()) {
        return error.As(ErrProcessDone)!;
    }
    if (sig == Kill) {
        ref syscall.Handle terminationHandle = ref heap(out ptr<syscall.Handle> _addr_terminationHandle);
        var e = syscall.DuplicateHandle(~syscall.Handle(0), syscall.Handle(handle), ~syscall.Handle(0), _addr_terminationHandle, syscall.PROCESS_TERMINATE, false, 0);
        if (e != null) {
            return error.As(NewSyscallError("DuplicateHandle", e))!;
        }
        runtime.KeepAlive(p);
        defer(syscall.CloseHandle(terminationHandle));
        e = syscall.TerminateProcess(syscall.Handle(terminationHandle), 1);
        return error.As(NewSyscallError("TerminateProcess", e))!;

    }
    return error.As(syscall.Errno(syscall.EWINDOWS))!;

});

private static error release(this ptr<Process> _addr_p) {
    ref Process p = ref _addr_p.val;

    var handle = atomic.SwapUintptr(_addr_p.handle, uintptr(syscall.InvalidHandle));
    if (handle == uintptr(syscall.InvalidHandle)) {
        return error.As(syscall.EINVAL)!;
    }
    var e = syscall.CloseHandle(syscall.Handle(handle));
    if (e != null) {
        return error.As(NewSyscallError("CloseHandle", e))!;
    }
    runtime.SetFinalizer(p, null);
    return error.As(null!)!;

}

private static (ptr<Process>, error) findProcess(nint pid) {
    ptr<Process> p = default!;
    error err = default!;

    const var da = syscall.STANDARD_RIGHTS_READ | syscall.PROCESS_QUERY_INFORMATION | syscall.SYNCHRONIZE;

    var (h, e) = syscall.OpenProcess(da, false, uint32(pid));
    if (e != null) {
        return (_addr_null!, error.As(NewSyscallError("OpenProcess", e))!);
    }
    return (_addr_newProcess(pid, uintptr(h))!, error.As(null!)!);

}

private static void init() {
    var cmd = windows.UTF16PtrToString(syscall.GetCommandLine());
    if (len(cmd) == 0) {
        var (arg0, _) = Executable();
        Args = new slice<@string>(new @string[] { arg0 });
    }
    else
 {
        Args = commandLineToArgv(cmd);
    }
}

// appendBSBytes appends n '\\' bytes to b and returns the resulting slice.
private static slice<byte> appendBSBytes(slice<byte> b, nint n) {
    while (n > 0) {
        b = append(b, '\\');
        n--;
    }
    return b;

}

// readNextArg splits command line string cmd into next
// argument and command line remainder.
private static (slice<byte>, @string) readNextArg(@string cmd) {
    slice<byte> arg = default;
    @string rest = default;

    slice<byte> b = default;
    bool inquote = default;
    nint nslash = default;
    while (len(cmd) > 0) {
        var c = cmd[0];
        switch (c) {
            case ' ': 

            case '\t': 
                        if (!inquote) {
                            return (appendBSBytes(b, nslash), cmd[(int)1..]);
                    cmd = cmd[(int)1..];
                        }

                break;
            case '"': 
                           b = appendBSBytes(b, nslash / 2);
                           if (nslash % 2 == 0) { 
                               // use "Prior to 2008" rule from
                               // http://daviddeley.com/autohotkey/parameters/parameters.htm
                               // section 5.2 to deal with double double quotes
                               if (inquote && len(cmd) > 1 && cmd[1] == '"') {
                                   b = append(b, c);
                                   cmd = cmd[(int)1..];
                               }

                               inquote = !inquote;

                           }
                           else
                {
                               b = append(b, c);
                           }

                           nslash = 0;
                           continue;

                break;
            case '\\': 
                nslash++;
                continue;
                break;
        }
        b = appendBSBytes(b, nslash);
        nslash = 0;
        b = append(b, c);

    }
    return (appendBSBytes(b, nslash), "");

}

// commandLineToArgv splits a command line into individual argument
// strings, following the Windows conventions documented
// at http://daviddeley.com/autohotkey/parameters/parameters.htm#WINARGV
private static slice<@string> commandLineToArgv(@string cmd) {
    slice<@string> args = default;
    while (len(cmd) > 0) {
        if (cmd[0] == ' ' || cmd[0] == '\t') {
            cmd = cmd[(int)1..];
            continue;
        }
        slice<byte> arg = default;
        arg, cmd = readNextArg(cmd);
        args = append(args, string(arg));

    }
    return args;

}

private static time.Duration ftToDuration(ptr<syscall.Filetime> _addr_ft) {
    ref syscall.Filetime ft = ref _addr_ft.val;

    var n = int64(ft.HighDateTime) << 32 + int64(ft.LowDateTime); // in 100-nanosecond intervals
    return time.Duration(n * 100) * time.Nanosecond;

}

private static time.Duration userTime(this ptr<ProcessState> _addr_p) {
    ref ProcessState p = ref _addr_p.val;

    return ftToDuration(_addr_p.rusage.UserTime);
}

private static time.Duration systemTime(this ptr<ProcessState> _addr_p) {
    ref ProcessState p = ref _addr_p.val;

    return ftToDuration(_addr_p.rusage.KernelTime);
}

} // end os_package
