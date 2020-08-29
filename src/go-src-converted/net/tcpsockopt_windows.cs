// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package net -- go2cs converted at 2020 August 29 08:27:58 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\tcpsockopt_windows.go
using os = go.os_package;
using runtime = go.runtime_package;
using syscall = go.syscall_package;
using time = go.time_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class net_package
    {
        private static error setKeepAlivePeriod(ref netFD fd, time.Duration d)
        { 
            // The kernel expects milliseconds so round to next highest
            // millisecond.
            d += (time.Millisecond - time.Nanosecond);
            var msecs = uint32(d / time.Millisecond);
            syscall.TCPKeepalive ka = new syscall.TCPKeepalive(OnOff:1,Time:msecs,Interval:msecs,);
            var ret = uint32(0L);
            var size = uint32(@unsafe.Sizeof(ka));
            var err = fd.pfd.WSAIoctl(syscall.SIO_KEEPALIVE_VALS, (byte.Value)(@unsafe.Pointer(ref ka)), size, null, 0L, ref ret, null, 0L);
            runtime.KeepAlive(fd);
            return error.As(os.NewSyscallError("wsaioctl", err));
        }
    }
}
