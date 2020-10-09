// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package time -- go2cs converted at 2020 October 09 05:06:05 UTC
// import "time" ==> using time = go.time_package
// Original source: C:\Go\src\time\sys_windows.go
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
        }

        private static (System.UIntPtr, error) open(@string name)
        {
            System.UIntPtr _p0 = default;
            error _p0 = default!;

            var (fd, err) = syscall.Open(name, syscall.O_RDONLY, 0L);
            if (err != null)
            {
                return (0L, error.As(err)!);
            }

            return (uintptr(fd), error.As(null!)!);

        }

        private static (long, error) read(System.UIntPtr fd, slice<byte> buf)
        {
            long _p0 = default;
            error _p0 = default!;

            return syscall.Read(syscall.Handle(fd), buf);
        }

        private static void closefd(System.UIntPtr fd)
        {
            syscall.Close(syscall.Handle(fd));
        }

        private static error preadn(System.UIntPtr fd, slice<byte> buf, long off)
        {
            var whence = seekStart;
            if (off < 0L)
            {
                whence = seekEnd;
            }

            {
                var (_, err) = syscall.Seek(syscall.Handle(fd), int64(off), whence);

                if (err != null)
                {
                    return error.As(err)!;
                }

            }

            while (len(buf) > 0L)
            {
                var (m, err) = syscall.Read(syscall.Handle(fd), buf);
                if (m <= 0L)
                {
                    if (err == null)
                    {
                        return error.As(errors.New("short read"))!;
                    }

                    return error.As(err)!;

                }

                buf = buf[m..];

            }

            return error.As(null!)!;

        }
    }
}
