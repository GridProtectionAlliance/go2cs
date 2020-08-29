// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin dragonfly freebsd linux nacl netbsd openbsd solaris

// package os -- go2cs converted at 2020 August 29 08:43:37 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Go\src\os\error_unix.go
using syscall = go.syscall_package;
using static go.builtin;

namespace go
{
    public static partial class os_package
    {
        private static bool isExist(error err)
        {
            err = underlyingError(err);
            return err == syscall.EEXIST || err == syscall.ENOTEMPTY || err == ErrExist;
        }

        private static bool isNotExist(error err)
        {
            err = underlyingError(err);
            return err == syscall.ENOENT || err == ErrNotExist;
        }

        private static bool isPermission(error err)
        {
            err = underlyingError(err);
            return err == syscall.EACCES || err == syscall.EPERM || err == ErrPermission;
        }
    }
}
