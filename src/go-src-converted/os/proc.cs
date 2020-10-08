// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Process etc.

// package os -- go2cs converted at 2020 October 08 03:44:56 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Go\src\os\proc.go
using runtime = go.runtime_package;
using syscall = go.syscall_package;
using static go.builtin;

namespace go
{
    public static partial class os_package
    {
        // Args hold the command-line arguments, starting with the program name.
        public static slice<@string> Args = default;

        private static void init()
        {
            if (runtime.GOOS == "windows")
            { 
                // Initialized in exec_windows.go.
                return ;

            }

            Args = runtime_args();

        }

        private static slice<@string> runtime_args()
; // in package runtime

        // Getuid returns the numeric user id of the caller.
        //
        // On Windows, it returns -1.
        public static long Getuid()
        {
            return syscall.Getuid();
        }

        // Geteuid returns the numeric effective user id of the caller.
        //
        // On Windows, it returns -1.
        public static long Geteuid()
        {
            return syscall.Geteuid();
        }

        // Getgid returns the numeric group id of the caller.
        //
        // On Windows, it returns -1.
        public static long Getgid()
        {
            return syscall.Getgid();
        }

        // Getegid returns the numeric effective group id of the caller.
        //
        // On Windows, it returns -1.
        public static long Getegid()
        {
            return syscall.Getegid();
        }

        // Getgroups returns a list of the numeric ids of groups that the caller belongs to.
        //
        // On Windows, it returns syscall.EWINDOWS. See the os/user package
        // for a possible alternative.
        public static (slice<long>, error) Getgroups()
        {
            slice<long> _p0 = default;
            error _p0 = default!;

            var (gids, e) = syscall.Getgroups();
            return (gids, error.As(NewSyscallError("getgroups", e))!);
        }

        // Exit causes the current program to exit with the given status code.
        // Conventionally, code zero indicates success, non-zero an error.
        // The program terminates immediately; deferred functions are not run.
        //
        // For portability, the status code should be in the range [0, 125].
        public static void Exit(long code)
        {
            if (code == 0L)
            {>>MARKER:FUNCTION_runtime_args_BLOCK_PREFIX<< 
                // Give race detector a chance to fail the program.
                // Racy programs do not have the right to finish successfully.
                runtime_beforeExit();

            }

            syscall.Exit(code);

        }

        private static void runtime_beforeExit()
; // implemented in runtime
    }
}
