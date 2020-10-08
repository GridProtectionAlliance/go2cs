// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package net -- go2cs converted at 2020 October 08 03:33:28 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\interface_darwin.go
using syscall = go.syscall_package;

using route = go.golang.org.x.net.route_package;
using static go.builtin;

namespace go
{
    public static partial class net_package
    {
        private static (slice<route.Message>, error) interfaceMessages(long ifindex)
        {
            slice<route.Message> _p0 = default;
            error _p0 = default!;

            var (rib, err) = route.FetchRIB(syscall.AF_UNSPEC, syscall.NET_RT_IFLIST, ifindex);
            if (err != null)
            {
                return (null, error.As(err)!);
            }
            return route.ParseRIB(syscall.NET_RT_IFLIST, rib);

        }

        // interfaceMulticastAddrTable returns addresses for a specific
        // interface.
        private static (slice<Addr>, error) interfaceMulticastAddrTable(ptr<Interface> _addr_ifi)
        {
            slice<Addr> _p0 = default;
            error _p0 = default!;
            ref Interface ifi = ref _addr_ifi.val;

            var (rib, err) = route.FetchRIB(syscall.AF_UNSPEC, syscall.NET_RT_IFLIST2, ifi.Index);
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            var (msgs, err) = route.ParseRIB(syscall.NET_RT_IFLIST2, rib);
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            var ifmat = make_slice<Addr>(0L, len(msgs));
            {
                var m__prev1 = m;

                foreach (var (_, __m) in msgs)
                {
                    m = __m;
                    switch (m.type())
                    {
                        case ptr<route.InterfaceMulticastAddrMessage> m:
                            if (ifi.Index != m.Index)
                            {
                                continue;
                            }

                            IP ip = default;
                            switch (m.Addrs[syscall.RTAX_IFA].type())
                            {
                                case ptr<route.Inet4Addr> sa:
                                    ip = IPv4(sa.IP[0L], sa.IP[1L], sa.IP[2L], sa.IP[3L]);
                                    break;
                                case ptr<route.Inet6Addr> sa:
                                    ip = make(IP, IPv6len);
                                    copy(ip, sa.IP[..]);
                                    break;
                            }
                            if (ip != null)
                            {
                                ifmat = append(ifmat, addr(new IPAddr(IP:ip)));
                            }

                            break;
                    }

                }

                m = m__prev1;
            }

            return (ifmat, error.As(null!)!);

        }
    }
}
