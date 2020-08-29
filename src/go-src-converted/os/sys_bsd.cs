// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin dragonfly freebsd nacl netbsd openbsd

// os code shared between *BSD systems including OS X (Darwin)
// and FreeBSD.

// package os -- go2cs converted at 2020 August 29 08:44:30 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Go\src\os\sys_bsd.go
using syscall = go.syscall_package;
using static go.builtin;

namespace go
{
    public static partial class os_package
    {
        private static (@string, error) hostname()
        {
            name, err = syscall.Sysctl("kern.hostname");
            if (err != null)
            {
                return ("", NewSyscallError("sysctl kern.hostname", err));
            }
            return (name, null);
        }
    }
}
