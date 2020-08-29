// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin dragonfly freebsd linux nacl netbsd openbsd solaris

// package time -- go2cs converted at 2020 August 29 08:42:18 UTC
// import "time" ==> using time = go.time_package
// Original source: C:\Go\src\time\sys_unix.go
using errors = go.errors_package;
using syscall = go.syscall_package;
using static go.builtin;

namespace go
{
    public static partial class time_package
    {
        // for testing: whatever interrupts a sleep
        private static void interrupt()
        {
            syscall.Kill(syscall.Getpid(), syscall.SIGCHLD);
        }

        private static (System.UIntPtr, error) open(@string name)
        {
            var (fd, err) = syscall.Open(name, syscall.O_RDONLY, 0L);
            if (err != null)
            {
                return (0L, err);
            }
            return (uintptr(fd), null);
        }

        private static (long, error) read(System.UIntPtr fd, slice<byte> buf)
        {
            return syscall.Read(int(fd), buf);
        }

        private static void closefd(System.UIntPtr fd)
        {
            syscall.Close(int(fd));
        }

        private static error preadn(System.UIntPtr fd, slice<byte> buf, long off)
        {
            var whence = seekStart;
            if (off < 0L)
            {
                whence = seekEnd;
            }
            {
                var (_, err) = syscall.Seek(int(fd), int64(off), whence);

                if (err != null)
                {
                    return error.As(err);
                }

            }
            while (len(buf) > 0L)
            {
                var (m, err) = syscall.Read(int(fd), buf);
                if (m <= 0L)
                {
                    if (err == null)
                    {
                        return error.As(errors.New("short read"));
                    }
                    return error.As(err);
                }
                buf = buf[m..];
            }

            return error.As(null);
        }
    }
}
