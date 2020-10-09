// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2020 October 09 05:00:51 UTC
// Original source: C:\Go\src\runtime\testdata\testprog\syscalls_linux.go
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using ioutil = go.io.ioutil_package;
using os = go.os_package;
using syscall = go.syscall_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private static long gettid()
        {
            return syscall.Gettid();
        }

        private static (bool, bool) tidExists(long tid)
        {
            bool exists = default;
            bool supported = default;

            var (stat, err) = ioutil.ReadFile(fmt.Sprintf("/proc/self/task/%d/stat", tid));
            if (os.IsNotExist(err))
            {
                return (false, true);
            } 
            // Check if it's a zombie thread.
            var state = bytes.Fields(stat)[2L];
            return (!(len(state) == 1L && state[0L] == 'Z'), true);

        }

        private static (@string, error) getcwd()
        {
            @string _p0 = default;
            error _p0 = default!;

            if (!syscall.ImplementsGetwd)
            {
                return ("", error.As(null!)!);
            } 
            // Use the syscall to get the current working directory.
            // This is imperative for checking for OS thread state
            // after an unshare since os.Getwd might just check the
            // environment, or use some other mechanism.
            array<byte> buf = new array<byte>(4096L);
            var (n, err) = syscall.Getcwd(buf[..]);
            if (err != null)
            {
                return ("", error.As(err)!);
            } 
            // Subtract one for null terminator.
            return (string(buf[..n - 1L]), error.As(null!)!);

        }

        private static error unshareFs()
        {
            var err = syscall.Unshare(syscall.CLONE_FS);
            if (err != null)
            {
                syscall.Errno (errno, ok) = err._<syscall.Errno>();
                if (ok && errno == syscall.EPERM)
                {
                    return error.As(errNotPermitted)!;
                }

            }

            return error.As(err)!;

        }

        private static error chdir(@string path)
        {
            return error.As(syscall.Chdir(path))!;
        }
    }
}
