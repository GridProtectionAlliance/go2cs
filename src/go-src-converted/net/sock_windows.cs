// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package net -- go2cs converted at 2022 March 06 22:16:43 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Program Files\Go\src\net\sock_windows.go
using windows = go.@internal.syscall.windows_package;
using os = go.os_package;
using syscall = go.syscall_package;

namespace go;

public static partial class net_package {

private static nint maxListenerBacklog() { 
    // TODO: Implement this
    // NOTE: Never return a number bigger than 1<<16 - 1. See issue 5030.
    return syscall.SOMAXCONN;

}

private static (syscall.Handle, error) sysSocket(nint family, nint sotype, nint proto) {
    syscall.Handle _p0 = default;
    error _p0 = default!;

    var (s, err) = wsaSocketFunc(int32(family), int32(sotype), int32(proto), null, 0, windows.WSA_FLAG_OVERLAPPED | windows.WSA_FLAG_NO_HANDLE_INHERIT);
    if (err == null) {
        return (s, error.As(null!)!);
    }
    syscall.ForkLock.RLock();
    s, err = socketFunc(family, sotype, proto);
    if (err == null) {
        syscall.CloseOnExec(s);
    }
    syscall.ForkLock.RUnlock();
    if (err != null) {
        return (syscall.InvalidHandle, error.As(os.NewSyscallError("socket", err))!);
    }
    return (s, error.As(null!)!);

}

} // end net_package
