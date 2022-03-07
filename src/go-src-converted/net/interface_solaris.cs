// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package net -- go2cs converted at 2022 March 06 22:16:04 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Program Files\Go\src\net\interface_solaris.go
using syscall = go.syscall_package;

using lif = go.golang.org.x.net.lif_package;

namespace go;

public static partial class net_package {

    // If the ifindex is zero, interfaceTable returns mappings of all
    // network interfaces. Otherwise it returns a mapping of a specific
    // interface.
private static (slice<Interface>, error) interfaceTable(nint ifindex) {
    slice<Interface> _p0 = default;
    error _p0 = default!;

    var (lls, err) = lif.Links(syscall.AF_UNSPEC, "");
    if (err != null) {
        return (null, error.As(err)!);
    }
    slice<Interface> ift = default;
    foreach (var (_, ll) in lls) {
        if (ifindex != 0 && ifindex != ll.Index) {
            continue;
        }
        Interface ifi = new Interface(Index:ll.Index,MTU:ll.MTU,Name:ll.Name,Flags:linkFlags(ll.Flags));
        if (len(ll.Addr) > 0) {
            ifi.HardwareAddr = HardwareAddr(ll.Addr);
        }
        ift = append(ift, ifi);

    }    return (ift, error.As(null!)!);

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

    @string name = default;
    if (ifi != null) {
        name = ifi.Name;
    }
    var (as, err) = lif.Addrs(syscall.AF_UNSPEC, name);
    if (err != null) {
        return (null, error.As(err)!);
    }
    slice<Addr> ifat = default;
    {
        var a__prev1 = a;

        foreach (var (_, __a) in as) {
            a = __a;
            IP ip = default;
            IPMask mask = default;
            switch (a.type()) {
                case ptr<lif.Inet4Addr> a:
                    ip = IPv4(a.IP[0], a.IP[1], a.IP[2], a.IP[3]);
                    mask = CIDRMask(a.PrefixLen, 8 * IPv4len);
                    break;
                case ptr<lif.Inet6Addr> a:
                    ip = make(IP, IPv6len);
                    copy(ip, a.IP[..]);
                    mask = CIDRMask(a.PrefixLen, 8 * IPv6len);
                    break;
            }
            ifat = append(ifat, addr(new IPNet(IP:ip,Mask:mask)));

        }
        a = a__prev1;
    }

    return (ifat, error.As(null!)!);

}

// interfaceMulticastAddrTable returns addresses for a specific
// interface.
private static (slice<Addr>, error) interfaceMulticastAddrTable(ptr<Interface> _addr_ifi) {
    slice<Addr> _p0 = default;
    error _p0 = default!;
    ref Interface ifi = ref _addr_ifi.val;

    return (null, error.As(null!)!);
}

} // end net_package
