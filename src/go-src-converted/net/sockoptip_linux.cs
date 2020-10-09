// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package net -- go2cs converted at 2020 October 09 04:52:14 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\sockoptip_linux.go
using runtime = go.runtime_package;
using syscall = go.syscall_package;
using static go.builtin;

namespace go
{
    public static partial class net_package
    {
        private static error setIPv4MulticastInterface(ptr<netFD> _addr_fd, ptr<Interface> _addr_ifi)
        {
            ref netFD fd = ref _addr_fd.val;
            ref Interface ifi = ref _addr_ifi.val;

            int v = default;
            if (ifi != null)
            {
                v = int32(ifi.Index);
            }
            ptr<syscall.IPMreqn> mreq = addr(new syscall.IPMreqn(Ifindex:v));
            var err = fd.pfd.SetsockoptIPMreqn(syscall.IPPROTO_IP, syscall.IP_MULTICAST_IF, mreq);
            runtime.KeepAlive(fd);
            return error.As(wrapSyscallError("setsockopt", err))!;

        }

        private static error setIPv4MulticastLoopback(ptr<netFD> _addr_fd, bool v)
        {
            ref netFD fd = ref _addr_fd.val;

            var err = fd.pfd.SetsockoptInt(syscall.IPPROTO_IP, syscall.IP_MULTICAST_LOOP, boolint(v));
            runtime.KeepAlive(fd);
            return error.As(wrapSyscallError("setsockopt", err))!;
        }
    }
}
