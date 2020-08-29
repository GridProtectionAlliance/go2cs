// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package net -- go2cs converted at 2020 August 29 08:26:37 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\interface_solaris.go
using syscall = go.syscall_package;

using lif = go.golang_org.x.net.lif_package;
using static go.builtin;

namespace go
{
    public static partial class net_package
    {
        // If the ifindex is zero, interfaceTable returns mappings of all
        // network interfaces. Otherwise it returns a mapping of a specific
        // interface.
        private static (slice<Interface>, error) interfaceTable(long ifindex)
        {
            var (lls, err) = lif.Links(syscall.AF_UNSPEC, "");
            if (err != null)
            {
                return (null, err);
            }
            slice<Interface> ift = default;
            foreach (var (_, ll) in lls)
            {
                if (ifindex != 0L && ifindex != ll.Index)
                {
                    continue;
                }
                Interface ifi = new Interface(Index:ll.Index,MTU:ll.MTU,Name:ll.Name,Flags:linkFlags(ll.Flags));
                if (len(ll.Addr) > 0L)
                {
                    ifi.HardwareAddr = HardwareAddr(ll.Addr);
                }
                ift = append(ift, ifi);
            }            return (ift, null);
        }

        private static readonly ulong sysIFF_UP = 0x1UL;
        private static readonly ulong sysIFF_BROADCAST = 0x2UL;
        private static readonly ulong sysIFF_DEBUG = 0x4UL;
        private static readonly ulong sysIFF_LOOPBACK = 0x8UL;
        private static readonly ulong sysIFF_POINTOPOINT = 0x10UL;
        private static readonly ulong sysIFF_NOTRAILERS = 0x20UL;
        private static readonly ulong sysIFF_RUNNING = 0x40UL;
        private static readonly ulong sysIFF_NOARP = 0x80UL;
        private static readonly ulong sysIFF_PROMISC = 0x100UL;
        private static readonly ulong sysIFF_ALLMULTI = 0x200UL;
        private static readonly ulong sysIFF_INTELLIGENT = 0x400UL;
        private static readonly ulong sysIFF_MULTICAST = 0x800UL;
        private static readonly ulong sysIFF_MULTI_BCAST = 0x1000UL;
        private static readonly ulong sysIFF_UNNUMBERED = 0x2000UL;
        private static readonly ulong sysIFF_PRIVATE = 0x8000UL;

        private static Flags linkFlags(long rawFlags)
        {
            Flags f = default;
            if (rawFlags & sysIFF_UP != 0L)
            {
                f |= FlagUp;
            }
            if (rawFlags & sysIFF_BROADCAST != 0L)
            {
                f |= FlagBroadcast;
            }
            if (rawFlags & sysIFF_LOOPBACK != 0L)
            {
                f |= FlagLoopback;
            }
            if (rawFlags & sysIFF_POINTOPOINT != 0L)
            {
                f |= FlagPointToPoint;
            }
            if (rawFlags & sysIFF_MULTICAST != 0L)
            {
                f |= FlagMulticast;
            }
            return f;
        }

        // If the ifi is nil, interfaceAddrTable returns addresses for all
        // network interfaces. Otherwise it returns addresses for a specific
        // interface.
        private static (slice<Addr>, error) interfaceAddrTable(ref Interface ifi)
        {
            @string name = default;
            if (ifi != null)
            {
                name = ifi.Name;
            }
            var (as, err) = lif.Addrs(syscall.AF_UNSPEC, name);
            if (err != null)
            {
                return (null, err);
            }
            slice<Addr> ifat = default;
            {
                var a__prev1 = a;

                foreach (var (_, __a) in as)
                {
                    a = __a;
                    IP ip = default;
                    IPMask mask = default;
                    switch (a.type())
                    {
                        case ref lif.Inet4Addr a:
                            ip = IPv4(a.IP[0L], a.IP[1L], a.IP[2L], a.IP[3L]);
                            mask = CIDRMask(a.PrefixLen, 8L * IPv4len);
                            break;
                        case ref lif.Inet6Addr a:
                            ip = make(IP, IPv6len);
                            copy(ip, a.IP[..]);
                            mask = CIDRMask(a.PrefixLen, 8L * IPv6len);
                            break;
                    }
                    ifat = append(ifat, ref new IPNet(IP:ip,Mask:mask));
                }

                a = a__prev1;
            }

            return (ifat, null);
        }

        // interfaceMulticastAddrTable returns addresses for a specific
        // interface.
        private static (slice<Addr>, error) interfaceMulticastAddrTable(ref Interface ifi)
        {
            return (null, null);
        }
    }
}
