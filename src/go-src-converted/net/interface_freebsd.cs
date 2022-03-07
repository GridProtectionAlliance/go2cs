// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package net -- go2cs converted at 2022 March 06 22:16:00 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Program Files\Go\src\net\interface_freebsd.go
using syscall = go.syscall_package;

using route = go.golang.org.x.net.route_package;

namespace go;

public static partial class net_package {

private static (slice<route.Message>, error) interfaceMessages(nint ifindex) {
    slice<route.Message> _p0 = default;
    error _p0 = default!;

    var typ = route.RIBType(syscall.NET_RT_IFLISTL);
    var (rib, err) = route.FetchRIB(syscall.AF_UNSPEC, typ, ifindex);
    if (err != null) {
        typ = route.RIBType(syscall.NET_RT_IFLIST);
        rib, err = route.FetchRIB(syscall.AF_UNSPEC, typ, ifindex);
        if (err != null) {
            return (null, error.As(err)!);
        }
    }
    return route.ParseRIB(typ, rib);

}

// interfaceMulticastAddrTable returns addresses for a specific
// interface.
private static (slice<Addr>, error) interfaceMulticastAddrTable(ptr<Interface> _addr_ifi) {
    slice<Addr> _p0 = default;
    error _p0 = default!;
    ref Interface ifi = ref _addr_ifi.val;

    var (rib, err) = route.FetchRIB(syscall.AF_UNSPEC, syscall.NET_RT_IFMALIST, ifi.Index);
    if (err != null) {
        return (null, error.As(err)!);
    }
    var (msgs, err) = route.ParseRIB(syscall.NET_RT_IFMALIST, rib);
    if (err != null) {
        return (null, error.As(err)!);
    }
    var ifmat = make_slice<Addr>(0, len(msgs));
    {
        var m__prev1 = m;

        foreach (var (_, __m) in msgs) {
            m = __m;
            switch (m.type()) {
                case ptr<route.InterfaceMulticastAddrMessage> m:
                    if (ifi.Index != m.Index) {
                        continue;
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
                    if (ip != null) {
                        ifmat = append(ifmat, addr(new IPAddr(IP:ip)));
                    }

                    break;
            }

        }
        m = m__prev1;
    }

    return (ifmat, error.As(null!)!);

}

} // end net_package
