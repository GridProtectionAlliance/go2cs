// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package net -- go2cs converted at 2020 August 29 08:27:43 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\sock_windows.go
using windows = go.@internal.syscall.windows_package;
using os = go.os_package;
using syscall = go.syscall_package;
using static go.builtin;

namespace go
{
    public static partial class net_package
    {
        private static long maxListenerBacklog()
        { 
            // TODO: Implement this
            // NOTE: Never return a number bigger than 1<<16 - 1. See issue 5030.
            return syscall.SOMAXCONN;
        }

        private static (syscall.Handle, error) sysSocket(long family, long sotype, long proto)
        {
            var (s, err) = wsaSocketFunc(int32(family), int32(sotype), int32(proto), null, 0L, windows.WSA_FLAG_OVERLAPPED | windows.WSA_FLAG_NO_HANDLE_INHERIT);
            if (err == null)
            {
                return (s, null);
            } 
            // WSA_FLAG_NO_HANDLE_INHERIT flag is not supported on some
            // old versions of Windows, see
            // https://msdn.microsoft.com/en-us/library/windows/desktop/ms742212(v=vs.85).aspx
            // for details. Just use syscall.Socket, if windows.WSASocket failed.

            // See ../syscall/exec_unix.go for description of ForkLock.
            syscall.ForkLock.RLock();
            s, err = socketFunc(family, sotype, proto);
            if (err == null)
            {
                syscall.CloseOnExec(s);
            }
            syscall.ForkLock.RUnlock();
            if (err != null)
            {
                return (syscall.InvalidHandle, os.NewSyscallError("socket", err));
            }
            return (s, null);
        }
    }
}
