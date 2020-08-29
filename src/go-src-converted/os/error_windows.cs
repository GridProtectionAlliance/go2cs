// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package os -- go2cs converted at 2020 August 29 08:43:38 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Go\src\os\error_windows.go
using syscall = go.syscall_package;
using static go.builtin;

namespace go
{
    public static partial class os_package
    {
        private static bool isExist(error err)
        {
            err = underlyingError(err);
            return err == syscall.ERROR_ALREADY_EXISTS || err == syscall.ERROR_DIR_NOT_EMPTY || err == syscall.ERROR_FILE_EXISTS || err == ErrExist;
        }

        private static readonly var _ERROR_BAD_NETPATH = syscall.Errno(53L);



        private static bool isNotExist(error err)
        {
            err = underlyingError(err);
            return err == syscall.ERROR_FILE_NOT_FOUND || err == _ERROR_BAD_NETPATH || err == syscall.ERROR_PATH_NOT_FOUND || err == ErrNotExist;
        }

        private static bool isPermission(error err)
        {
            err = underlyingError(err);
            return err == syscall.ERROR_ACCESS_DENIED || err == ErrPermission;
        }
    }
}
