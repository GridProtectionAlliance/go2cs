// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin dragonfly freebsd linux netbsd openbsd solaris windows

// package net -- go2cs converted at 2020 August 29 08:27:22 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\sockoptip_posix.go
using runtime = go.runtime_package;
using syscall = go.syscall_package;
using static go.builtin;

namespace go
{
    public static partial class net_package
    {
        private static error joinIPv4Group(ref netFD fd, ref Interface ifi, IP ip)
        {
            syscall.IPMreq mreq = ref new syscall.IPMreq(Multiaddr:[4]byte{ip[0],ip[1],ip[2],ip[3]});
            {
                var err__prev1 = err;

                var err = setIPv4MreqToInterface(mreq, ifi);

                if (err != null)
                {
                    return error.As(err);
                }
                err = err__prev1;

            }
            err = fd.pfd.SetsockoptIPMreq(syscall.IPPROTO_IP, syscall.IP_ADD_MEMBERSHIP, mreq);
            runtime.KeepAlive(fd);
            return error.As(wrapSyscallError("setsockopt", err));
        }

        private static error setIPv6MulticastInterface(ref netFD fd, ref Interface ifi)
        {
            long v = default;
            if (ifi != null)
            {
                v = ifi.Index;
            }
            var err = fd.pfd.SetsockoptInt(syscall.IPPROTO_IPV6, syscall.IPV6_MULTICAST_IF, v);
            runtime.KeepAlive(fd);
            return error.As(wrapSyscallError("setsockopt", err));
        }

        private static error setIPv6MulticastLoopback(ref netFD fd, bool v)
        {
            var err = fd.pfd.SetsockoptInt(syscall.IPPROTO_IPV6, syscall.IPV6_MULTICAST_LOOP, boolint(v));
            runtime.KeepAlive(fd);
            return error.As(wrapSyscallError("setsockopt", err));
        }

        private static error joinIPv6Group(ref netFD fd, ref Interface ifi, IP ip)
        {
            syscall.IPv6Mreq mreq = ref new syscall.IPv6Mreq();
            copy(mreq.Multiaddr[..], ip);
            if (ifi != null)
            {
                mreq.Interface = uint32(ifi.Index);
            }
            var err = fd.pfd.SetsockoptIPv6Mreq(syscall.IPPROTO_IPV6, syscall.IPV6_JOIN_GROUP, mreq);
            runtime.KeepAlive(fd);
            return error.As(wrapSyscallError("setsockopt", err));
        }
    }
}
