// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package net -- go2cs converted at 2022 March 06 22:15:42 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Program Files\Go\src\net\error_windows.go
using syscall = go.syscall_package;

namespace go;

public static partial class net_package {

private static bool isConnError(error err) {
    {
        syscall.Errno (se, ok) = err._<syscall.Errno>();

        if (ok) {
            return se == syscall.WSAECONNRESET || se == syscall.WSAECONNABORTED;
        }
    }

    return false;

}

} // end net_package
