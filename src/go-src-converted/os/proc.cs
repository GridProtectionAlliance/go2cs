// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Process etc.

// package os -- go2cs converted at 2022 March 13 05:28:03 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Program Files\Go\src\os\proc.go
namespace go;

using testlog = @internal.testlog_package;
using runtime = runtime_package;
using syscall = syscall_package;


// Args hold the command-line arguments, starting with the program name.

public static partial class os_package {

public static slice<@string> Args = default;

private static void init() {
    if (runtime.GOOS == "windows") { 
        // Initialized in exec_windows.go.
        return ;
    }
    Args = runtime_args();
}

private static slice<@string> runtime_args(); // in package runtime

// Getuid returns the numeric user id of the caller.
//
// On Windows, it returns -1.
public static nint Getuid() {
    return syscall.Getuid();
}

// Geteuid returns the numeric effective user id of the caller.
//
// On Windows, it returns -1.
public static nint Geteuid() {
    return syscall.Geteuid();
}

// Getgid returns the numeric group id of the caller.
//
// On Windows, it returns -1.
public static nint Getgid() {
    return syscall.Getgid();
}

// Getegid returns the numeric effective group id of the caller.
//
// On Windows, it returns -1.
public static nint Getegid() {
    return syscall.Getegid();
}

// Getgroups returns a list of the numeric ids of groups that the caller belongs to.
//
// On Windows, it returns syscall.EWINDOWS. See the os/user package
// for a possible alternative.
public static (slice<nint>, error) Getgroups() {
    slice<nint> _p0 = default;
    error _p0 = default!;

    var (gids, e) = syscall.Getgroups();
    return (gids, error.As(NewSyscallError("getgroups", e))!);
}

// Exit causes the current program to exit with the given status code.
// Conventionally, code zero indicates success, non-zero an error.
// The program terminates immediately; deferred functions are not run.
//
// For portability, the status code should be in the range [0, 125].
public static void Exit(nint code) => func((_, panic, _) => {
    if (code == 0) {>>MARKER:FUNCTION_runtime_args_BLOCK_PREFIX<<
        if (testlog.PanicOnExit0()) { 
            // We were told to panic on calls to os.Exit(0).
            // This is used to fail tests that make an early
            // unexpected call to os.Exit(0).
            panic("unexpected call to os.Exit(0) during test");
        }
        runtime_beforeExit();
    }
    syscall.Exit(code);
});

private static void runtime_beforeExit(); // implemented in runtime

} // end os_package
