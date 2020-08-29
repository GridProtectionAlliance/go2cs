// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package syscall contains an interface to the low-level operating system
// primitives. The details vary depending on the underlying system, and
// by default, godoc will display the syscall documentation for the current
// system. If you want godoc to display syscall documentation for another
// system, set $GOOS and $GOARCH to the desired system. For example, if
// you want to view documentation for freebsd/arm on linux/amd64, set $GOOS
// to freebsd and $GOARCH to arm.
// The primary use of syscall is inside other packages that provide a more
// portable interface to the system, such as "os", "time" and "net".  Use
// those packages rather than this one if you can.
// For details of the functions and data types in this package consult
// the manuals for the appropriate operating system.
// These calls return err == nil to indicate success; otherwise
// err is an operating system error describing the failure.
// On most systems, that error has type syscall.Errno.
//
// NOTE: This package is locked down. Code outside the standard
// Go repository should be migrated to use the corresponding
// package in the golang.org/x/sys repository. That is also where updates
// required by new systems or versions should be applied.
// Signal, Errno and SysProcAttr are not yet available in
// golang.org/x/sys and must still be referenced from the
// syscall package. See https://golang.org/s/go1.4-syscall
// for more information.
//
// package syscall -- go2cs converted at 2020 August 29 08:37:42 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\syscall.go

using static go.builtin;

namespace go
{
    public static partial class syscall_package
    {
        //go:generate go run mksyscall_windows.go -systemdll -output zsyscall_windows.go syscall_windows.go security_windows.go

        // StringByteSlice converts a string to a NUL-terminated []byte,
        // If s contains a NUL byte this function panics instead of
        // returning an error.
        //
        // Deprecated: Use ByteSliceFromString instead.
        public static slice<byte> StringByteSlice(@string s) => func((_, panic, __) =>
        {
            var (a, err) = ByteSliceFromString(s);
            if (err != null)
            {
                panic("syscall: string with NUL passed to StringByteSlice");
            }
            return a;
        });

        // ByteSliceFromString returns a NUL-terminated slice of bytes
        // containing the text of s. If s contains a NUL byte at any
        // location, it returns (nil, EINVAL).
        public static (slice<byte>, error) ByteSliceFromString(@string s)
        {
            for (long i = 0L; i < len(s); i++)
            {
                if (s[i] == 0L)
                {
                    return (null, EINVAL);
                }
            }

            var a = make_slice<byte>(len(s) + 1L);
            copy(a, s);
            return (a, null);
        }

        // StringBytePtr returns a pointer to a NUL-terminated array of bytes.
        // If s contains a NUL byte this function panics instead of returning
        // an error.
        //
        // Deprecated: Use BytePtrFromString instead.
        public static ref byte StringBytePtr(@string s)
        {
            return ref StringByteSlice(s)[0L];
        }

        // BytePtrFromString returns a pointer to a NUL-terminated array of
        // bytes containing the text of s. If s contains a NUL byte at any
        // location, it returns (nil, EINVAL).
        public static (ref byte, error) BytePtrFromString(@string s)
        {
            var (a, err) = ByteSliceFromString(s);
            if (err != null)
            {
                return (null, err);
            }
            return (ref a[0L], null);
        }

        // Single-word zero for use when we need a valid pointer to 0 bytes.
        // See mksyscall.pl.
        private static System.UIntPtr _zero = default;

        // Unix returns ts as the number of seconds and nanoseconds elapsed since the
        // Unix epoch.
        private static (long, long) Unix(this ref Timespec ts)
        {
            return (int64(ts.Sec), int64(ts.Nsec));
        }

        // Unix returns tv as the number of seconds and nanoseconds elapsed since the
        // Unix epoch.
        private static (long, long) Unix(this ref Timeval tv)
        {
            return (int64(tv.Sec), int64(tv.Usec) * 1000L);
        }

        // Nano returns ts as the number of nanoseconds elapsed since the Unix epoch.
        private static long Nano(this ref Timespec ts)
        {
            return int64(ts.Sec) * 1e9F + int64(ts.Nsec);
        }

        // Nano returns tv as the number of nanoseconds elapsed since the Unix epoch.
        private static long Nano(this ref Timeval tv)
        {
            return int64(tv.Sec) * 1e9F + int64(tv.Usec) * 1000L;
        }

        // Getpagesize and Exit are provided by the runtime.

        public static long Getpagesize()
;
        public static void Exit(long code)
;
    }
}
