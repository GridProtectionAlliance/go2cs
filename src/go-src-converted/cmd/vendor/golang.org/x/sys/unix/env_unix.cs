// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build aix darwin dragonfly freebsd linux netbsd openbsd solaris

// Unix environment variables.

// package unix -- go2cs converted at 2020 October 08 04:46:15 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\sys\unix\env_unix.go
using syscall = go.syscall_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace vendor {
namespace golang.org {
namespace x {
namespace sys
{
    public static partial class unix_package
    {
        public static (@string, bool) Getenv(@string key)
        {
            @string value = default;
            bool found = default;

            return syscall.Getenv(key);
        }

        public static error Setenv(@string key, @string value)
        {
            return error.As(syscall.Setenv(key, value))!;
        }

        public static void Clearenv()
        {
            syscall.Clearenv();
        }

        public static slice<@string> Environ()
        {
            return syscall.Environ();
        }

        public static error Unsetenv(@string key)
        {
            return error.As(syscall.Unsetenv(key))!;
        }
    }
}}}}}}
