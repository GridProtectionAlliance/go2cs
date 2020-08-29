// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package net -- go2cs converted at 2020 August 29 08:27:25 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\sockoptip_windows.go
using os = go.os_package;
using runtime = go.runtime_package;
using syscall = go.syscall_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class net_package
    {
        private static error setIPv4MulticastInterface(ref netFD fd, ref Interface ifi)
        {
            var (ip, err) = interfaceToIPv4Addr(ifi);
            if (err != null)
            {
                return error.As(os.NewSyscallError("setsockopt", err));
            }
            array<byte> a = new array<byte>(4L);
            copy(a[..], ip.To4());
            err = fd.pfd.Setsockopt(syscall.IPPROTO_IP, syscall.IP_MULTICAST_IF, (byte.Value)(@unsafe.Pointer(ref a[0L])), 4L);
            runtime.KeepAlive(fd);
            return error.As(wrapSyscallError("setsockopt", err));
        }

        private static error setIPv4MulticastLoopback(ref netFD fd, bool v)
        {
            var err = fd.pfd.SetsockoptInt(syscall.IPPROTO_IP, syscall.IP_MULTICAST_LOOP, boolint(v));
            runtime.KeepAlive(fd);
            return error.As(wrapSyscallError("setsockopt", err));
        }
    }
}
