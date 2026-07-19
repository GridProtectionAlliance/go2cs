// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using errors = errors_package;
using windows = @internal.syscall.windows_package;
using Δruntime = runtime_package;
using syscall = syscall_package;
using time = time_package;
using @internal.syscall;

partial class os_package {

// Note that Process.mode is always modeHandle because Windows always requires
// a handle. A manually-created Process literal is not valid.
internal static (ж<ProcessState> ps, error err) wait(this ж<Process> Ꮡp) {
    ж<ProcessState> ps = default!;
    error err = default!;
    func((defer, recover) => {
    ref var p = ref Ꮡp.Value;

        var (handle, status) = Ꮡp.handleTransientAcquire();
        var exprᴛ1 = status;
        if (exprᴛ1 == statusDone) {
            (ps, err) = (default!, ErrProcessDone); return;
        }
        if (exprᴛ1 == statusReleased) {
            (ps, err) = (default!, syscall.EINVAL); return;
        }

        defer(Ꮡp.handleTransientRelease);
        var (s, e) = syscall.WaitForSingleObject(((syscallꓸHandle)handle), syscall.INFINITE);
        var exprᴛ2 = s;
        if (exprᴛ2 == syscall.WAIT_OBJECT_0) {
            do {
                break;
            } while (false);
        }
        else if (exprᴛ2 == syscall.WAIT_FAILED) {
            (ps, err) = (default!, NewSyscallError("WaitForSingleObject"u8, e)); return;
        }
        else { /* default: */
            (ps, err) = (default!, errors.New("os: unexpected result from WaitForSingleObject"u8)); return;
        }

        ref var ec = ref heap(new uint32(), out var Ꮡec);
        e = syscall.GetExitCodeProcess(((syscallꓸHandle)handle), Ꮡec);
        if (e != default!) {
            (ps, err) = (default!, NewSyscallError("GetExitCodeProcess"u8, e)); return;
        }
        ref var u = ref heap(new syscall.Rusage(), out var Ꮡu);
        e = syscall.GetProcessTimes(((syscallꓸHandle)handle), Ꮡu.of(syscall.Rusage.ᏑCreationTime), Ꮡu.of(syscall.Rusage.ᏑExitTime), Ꮡu.of(syscall.Rusage.ᏑKernelTime), Ꮡu.of(syscall.Rusage.ᏑUserTime));
        if (e != default!) {
            (ps, err) = (default!, NewSyscallError("GetProcessTimes"u8, e)); return;
        }
        defer(() => Ꮡp.Release());
        (ps, err) = (Ꮡ(new ProcessState(p.Pid, new syscall.WaitStatus(ExitCode: ec), Ꮡu)), default!);
    });
    return (ps, err);
}

internal static error signal(this ж<Process> Ꮡp, ΔSignal sig) => func<error>((defer, recover) => {
    var (handle, status) = Ꮡp.handleTransientAcquire();
    var exprᴛ1 = status;
    if (exprᴛ1 == statusDone) {
        return ErrProcessDone;
    }
    if (exprᴛ1 == statusReleased) {
        return syscall.EINVAL;
    }

    defer(Ꮡp.handleTransientRelease);
    if (AreEqual(sig, ΔKill)) {
        ref var terminationHandle = ref heap(new syscallꓸHandle(), out var ᏑterminationHandle);
        var e = syscall.DuplicateHandle(~((syscallꓸHandle)((syscallꓸHandle)0)), ((syscallꓸHandle)handle), ~((syscallꓸHandle)((syscallꓸHandle)0)), ᏑterminationHandle, syscall.PROCESS_TERMINATE, false, 0);
        if (e != default!) {
            return NewSyscallError("DuplicateHandle"u8, e);
        }
        Δruntime.KeepAlive(Ꮡp);
        deferǃ(syscall.CloseHandle, terminationHandle, defer);
        e = syscall.TerminateProcess(terminationHandle, 1);
        return NewSyscallError("TerminateProcess"u8, e);
    }
    // TODO(rsc): Handle Interrupt too?
    return ((syscall.Errno)syscall.EWINDOWS);
});

internal static error release(this ж<Process> Ꮡp) {
    // Drop the Process' reference and mark handle unusable for
    // future calls.
    //
    // The API on Windows expects EINVAL if Release is called multiple
    // times.
    {
        var old = Ꮡp.handlePersistentRelease(statusReleased); if (old == statusReleased) {
            return syscall.EINVAL;
        }
    }
    // no need for a finalizer anymore
    Δruntime.SetFinalizer(Ꮡp, default!);
    return default!;
}

[GoRecv] internal static void closeHandle(this ref Process p) {
    syscall.CloseHandle(((syscallꓸHandle)p.handle));
}

internal static (ж<Process> p, error err) findProcess(nint pid) {
    ж<Process> p = default!;
    error err = default!;

    const uint32 da = /* syscall.STANDARD_RIGHTS_READ |
	syscall.PROCESS_QUERY_INFORMATION | syscall.SYNCHRONIZE */ 1180672;
    var (h, e) = syscall.OpenProcess(da, false, (uint32)pid);
    if (e != default!) {
        return (default!, NewSyscallError("OpenProcess"u8, e));
    }
    return (newHandleProcess(pid, (uintptr)h), default!);
}

[GoInit] internal static void init() {
    @string cmd = windows.UTF16PtrToString(syscall.GetCommandLine());
    if (len(cmd) == 0){
        var (arg0, _) = Executable();
        Args = new @string[]{arg0}.slice();
    } else {
        Args = commandLineToArgv(cmd);
    }
}

// appendBSBytes appends n '\\' bytes to b and returns the resulting slice.
internal static slice<byte> appendBSBytes(slice<byte> b, nint n) {
    for (; n > 0; n--) {
        b = append(b, (byte)((rune)'\\'));
    }
    return b;
}

// readNextArg splits command line string cmd into next
// argument and command line remainder.
internal static (slice<byte> arg, @string rest) readNextArg(@string cmd) {
    slice<byte> arg = default!;
    @string rest = default!;

    slice<byte> b = default!;
    bool inquote = default!;
    nint nslash = default!;
    for (; len(cmd) > 0; cmd = cmd[1..]) {
        var c = cmd[0];
        switch (c) {
        case (rune)' ' or (rune)'\t': {
            if (!inquote) {
                return (appendBSBytes(b, nslash), cmd[1..]);
            }
            break;
        }
        case (rune)'"': {
            b = appendBSBytes(b, nslash / 2);
            if (nslash % 2 == 0){
                // use "Prior to 2008" rule from
                // http://daviddeley.com/autohotkey/parameters/parameters.htm
                // section 5.2 to deal with double double quotes
                if (inquote && len(cmd) > 1 && cmd[1] == (rune)'"') {
                    b = append(b, c);
                    cmd = cmd[1..];
                }
                inquote = !inquote;
            } else {
                b = append(b, c);
            }
            nslash = 0;
            continue;
            break;
        }
        case (rune)'\\': {
            nslash++;
            continue;
            break;
        }}

        b = appendBSBytes(b, nslash);
        nslash = 0;
        b = append(b, c);
    }
    return (appendBSBytes(b, nslash), "");
}

// commandLineToArgv splits a command line into individual argument
// strings, following the Windows conventions documented
// at http://daviddeley.com/autohotkey/parameters/parameters.htm#WINARGV
internal static slice<@string> commandLineToArgv(@string cmd) {
    slice<@string> args = default!;
    while (len(cmd) > 0) {
        if (cmd[0] == (rune)' ' || cmd[0] == (rune)'\t') {
            cmd = cmd[1..];
            continue;
        }
        slice<byte> arg = default!;
        (arg, cmd) = readNextArg(cmd);
        args = append(args, ((@string)arg));
    }
    return args;
}

internal static time.Duration ftToDuration(ж<syscall.Filetime> Ꮡft) {
    ref var ft = ref Ꮡft.Value;

    var n = ((int64)ft.HighDateTime << (int)(32)) + (int64)ft.LowDateTime;
    // in 100-nanosecond intervals
    return ((time.Duration)(n * 100)) * time.ΔNanosecond;
}

[GoRecv] internal static time.Duration userTime(this ref ProcessState p) {
    return ftToDuration(p.rusage.of(syscall.Rusage.ᏑUserTime));
}

[GoRecv] internal static time.Duration systemTime(this ref ProcessState p) {
    return ftToDuration(p.rusage.of(syscall.Rusage.ᏑKernelTime));
}

} // end os_package
