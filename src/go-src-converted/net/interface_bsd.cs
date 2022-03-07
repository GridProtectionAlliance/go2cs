// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build darwin || dragonfly || freebsd || netbsd || openbsd
// +build darwin dragonfly freebsd netbsd openbsd

// package net -- go2cs converted at 2022 March 06 22:15:52 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Program Files\Go\src\net\interface_bsd.go
using syscall = go.syscall_package;

using route = go.golang.org.x.net.route_package;

namespace go;

public static partial class net_package {

    // If the ifindex is zero, interfaceTable returns mappings of all
    // network interfaces. Otherwise it returns a mapping of a specific
    // interface.
private static (slice<Interface>, error) interfaceTable(nint ifindex) {
    slice<Interface> _p0 = default;
    error _p0 = default!;

    var (msgs, err) = interfaceMessages(ifindex);
    if (err != null) {
        return (null, error.As(err)!);
    }
    var n = len(msgs);
    if (ifindex != 0) {
        n = 1;
    }
    var ift = make_slice<Interface>(n);
    n = 0;
    {
        var m__prev1 = m;

        foreach (var (_, __m) in msgs) {
            m = __m;
            switch (m.type()) {
                case ptr<route.InterfaceMessage> m:
                    if (ifindex != 0 && ifindex != m.Index) {
                        continue;
                    }
                    ift[n].Index = m.Index;
                    ift[n].Name = m.Name;
                    ift[n].Flags = linkFlags(m.Flags);
                    {
                        ptr<route.LinkAddr> (sa, ok) = m.Addrs[syscall.RTAX_IFP]._<ptr<route.LinkAddr>>();

                        if (ok && len(sa.Addr) > 0) {
                            ift[n].HardwareAddr = make_slice<byte>(len(sa.Addr));
                            copy(ift[n].HardwareAddr, sa.Addr);
                        }
                    }

                    foreach (var (_, sys) in m.Sys()) {
                        {
                            ptr<route.InterfaceMetrics> (imx, ok) = sys._<ptr<route.InterfaceMetrics>>();

                            if (ok) {
                                ift[n].MTU = imx.MTU;
                                break;
                            }
                        }

                    }                    n++;
                    if (ifindex == m.Index) {
                        return (ift[..(int)n], error.As(null!)!);
                    }
                    break;
            }

        }
        m = m__prev1;
    }

    return (ift[..(int)n], error.As(null!)!);

}

private static Flags linkFlags(nint rawFlags) {
    Flags f = default;
    if (rawFlags & syscall.IFF_UP != 0) {
        f |= FlagUp;
    }
    if (rawFlags & syscall.IFF_BROADCAST != 0) {
        f |= FlagBroadcast;
    }
    if (rawFlags & syscall.IFF_LOOPBACK != 0) {
        f |= FlagLoopback;
    }
    if (rawFlags & syscall.IFF_POINTOPOINT != 0) {
        f |= FlagPointToPoint;
    }
    if (rawFlags & syscall.IFF_MULTICAST != 0) {
        f |= FlagMulticast;
    }
    return f;

}

// If the ifi is nil, interfaceAddrTable returns addresses for all
// network interfaces. Otherwise it returns addresses for a specific
// interface.
private static (slice<Addr>, error) interfaceAddrTable(ptr<Interface> _addr_ifi) {
    slice<Addr> _p0 = default;
    error _p0 = default!;
    ref Interface ifi = ref _addr_ifi.val;

    nint index = 0;
    if (ifi != null) {
        index = ifi.Index;
    }
    var (msgs, err) = interfaceMessages(index);
    if (err != null) {
        return (null, error.As(err)!);
    }
    var ifat = make_slice<Addr>(0, len(msgs));
    {
        var m__prev1 = m;

        foreach (var (_, __m) in msgs) {
            m = __m;
            switch (m.type()) {
                case ptr<route.InterfaceAddrMessage> m:
                    if (index != 0 && index != m.Index) {
                        continue;
                    }
                    IPMask mask = default;
                    switch (m.Addrs[syscall.RTAX_NETMASK].type()) {
                        case ptr<route.Inet4Addr> sa:
                            mask = IPv4Mask(sa.IP[0], sa.IP[1], sa.IP[2], sa.IP[3]);
                            break;
                        case ptr<route.Inet6Addr> sa:
                            mask = make(IPMask, IPv6len);
                            copy(mask, sa.IP[..]);
                            break;
                    }
                    IP ip = default;
                    switch (m.Addrs[syscall.RTAX_IFA].type()) {
                        case ptr<route.Inet4Addr> sa:
                            ip = IPv4(sa.IP[0], sa.IP[1], sa.IP[2], sa.IP[3]);
                            break;
                        case ptr<route.Inet6Addr> sa:
                            ip = make(IP, IPv6len);
                            copy(ip, sa.IP[..]);
                            break;
                    }
                    if (ip != null && mask != null) { // NetBSD may contain route.LinkAddr
                        ifat = append(ifat, addr(new IPNet(IP:ip,Mask:mask)));

                    }

                    break;
            }

        }
        m = m__prev1;
    }

    return (ifat, error.As(null!)!);

}

} // end net_package
