// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build aix darwin dragonfly freebsd js,wasm linux netbsd openbsd solaris windows

// package net -- go2cs converted at 2020 October 09 04:50:51 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\error_posix.go
using os = go.os_package;
using syscall = go.syscall_package;
using static go.builtin;

namespace go
{
    public static partial class net_package
    {
        // wrapSyscallError takes an error and a syscall name. If the error is
        // a syscall.Errno, it wraps it in a os.SyscallError using the syscall name.
        private static error wrapSyscallError(@string name, error err)
        {
            {
                syscall.Errno (_, ok) = err._<syscall.Errno>();

                if (ok)
                {
                    err = os.NewSyscallError(name, err);
                }
            }

            return error.As(err)!;

        }
    }
}
