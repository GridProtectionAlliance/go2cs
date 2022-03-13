// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build aix || darwin || dragonfly || freebsd || (js && wasm) || linux || netbsd || openbsd || solaris || windows
// +build aix darwin dragonfly freebsd js,wasm linux netbsd openbsd solaris windows

// package net -- go2cs converted at 2022 March 13 05:29:54 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Program Files\Go\src\net\ipsock_posix.go
namespace go;

using context = context_package;
using poll = @internal.poll_package;
using runtime = runtime_package;
using syscall = syscall_package;


// probe probes IPv4, IPv6 and IPv4-mapped IPv6 communication
// capabilities which are controlled by the IPV6_V6ONLY socket option
// and kernel configuration.
//
// Should we try to use the IPv4 socket interface if we're only
// dealing with IPv4 sockets? As long as the host system understands
// IPv4-mapped IPv6, it's okay to pass IPv4-mapped IPv6 addresses to
// the IPv6 interface. That simplifies our code and is most
// general. Unfortunately, we need to run on kernels built without
// IPv6 support too. So probe the kernel to figure it out.

using System;
public static partial class net_package {

private static void probe(this ptr<ipStackCapabilities> _addr_p) => func((defer, _, _) => {
    ref ipStackCapabilities p = ref _addr_p.val;

    var (s, err) = sysSocket(syscall.AF_INET, syscall.SOCK_STREAM, syscall.IPPROTO_TCP);

    if (err == syscall.EAFNOSUPPORT || err == syscall.EPROTONOSUPPORT)     else if (err == null) 
        poll.CloseFunc(s);
        p.ipv4Enabled = true;
    
    switch (runtime.GOOS) {
        case "dragonfly": 
            // The latest DragonFly BSD and OpenBSD kernels don't
            // support IPV6_V6ONLY=0. They always return an error
            // and we don't need to probe the capability.

        case "openbsd": 
            // The latest DragonFly BSD and OpenBSD kernels don't
            // support IPV6_V6ONLY=0. They always return an error
            // and we don't need to probe the capability.
            probes = probes[..(int)1];
            break;
    }
    foreach (var (i) in probes) {
        (s, err) = sysSocket(syscall.AF_INET6, syscall.SOCK_STREAM, syscall.IPPROTO_TCP);
        if (err != null) {
            continue;
        }
        defer(poll.CloseFunc(s));
        syscall.SetsockoptInt(s, syscall.IPPROTO_IPV6, syscall.IPV6_V6ONLY, probes[i].value);
        var (sa, err) = probes[i].laddr.sockaddr(syscall.AF_INET6);
        if (err != null) {
            continue;
        }
        {
            var err = syscall.Bind(s, sa);

            if (err != null) {
                continue;
            }
        }
        if (i == 0) {
            p.ipv6Enabled = true;
        }
        else
 {
            p.ipv4MappedIPv6Enabled = true;
        }
    }
});

// favoriteAddrFamily returns the appropriate address family for the
// given network, laddr, raddr and mode.
//
// If mode indicates "listen" and laddr is a wildcard, we assume that
// the user wants to make a passive-open connection with a wildcard
// address family, both AF_INET and AF_INET6, and a wildcard address
// like the following:
//
//    - A listen for a wildcard communication domain, "tcp" or
//      "udp", with a wildcard address: If the platform supports
//      both IPv6 and IPv4-mapped IPv6 communication capabilities,
//      or does not support IPv4, we use a dual stack, AF_INET6 and
//      IPV6_V6ONLY=0, wildcard address listen. The dual stack
//      wildcard address listen may fall back to an IPv6-only,
//      AF_INET6 and IPV6_V6ONLY=1, wildcard address listen.
//      Otherwise we prefer an IPv4-only, AF_INET, wildcard address
//      listen.
//
//    - A listen for a wildcard communication domain, "tcp" or
//      "udp", with an IPv4 wildcard address: same as above.
//
//    - A listen for a wildcard communication domain, "tcp" or
//      "udp", with an IPv6 wildcard address: same as above.
//
//    - A listen for an IPv4 communication domain, "tcp4" or "udp4",
//      with an IPv4 wildcard address: We use an IPv4-only, AF_INET,
//      wildcard address listen.
//
//    - A listen for an IPv6 communication domain, "tcp6" or "udp6",
//      with an IPv6 wildcard address: We use an IPv6-only, AF_INET6
//      and IPV6_V6ONLY=1, wildcard address listen.
//
// Otherwise guess: If the addresses are IPv4 then returns AF_INET,
// or else returns AF_INET6. It also returns a boolean value what
// designates IPV6_V6ONLY option.
//
// Note that the latest DragonFly BSD and OpenBSD kernels allow
// neither "net.inet6.ip6.v6only=1" change nor IPPROTO_IPV6 level
// IPV6_V6ONLY socket option setting.
private static (nint, bool) favoriteAddrFamily(@string network, sockaddr laddr, sockaddr raddr, @string mode) {
    nint family = default;
    bool ipv6only = default;

    switch (network[len(network) - 1]) {
        case '4': 
            return (syscall.AF_INET, false);
            break;
        case '6': 
            return (syscall.AF_INET6, true);
            break;
    }

    if (mode == "listen" && (laddr == null || laddr.isWildcard())) {
        if (supportsIPv4map() || !supportsIPv4()) {
            return (syscall.AF_INET6, false);
        }
        if (laddr == null) {
            return (syscall.AF_INET, false);
        }
        return (laddr.family(), false);
    }
    if ((laddr == null || laddr.family() == syscall.AF_INET) && (raddr == null || raddr.family() == syscall.AF_INET)) {
        return (syscall.AF_INET, false);
    }
    return (syscall.AF_INET6, false);
}

private static (ptr<netFD>, error) internetSocket(context.Context ctx, @string net, sockaddr laddr, sockaddr raddr, nint sotype, nint proto, @string mode, Func<@string, @string, syscall.RawConn, error> ctrlFn) {
    ptr<netFD> fd = default!;
    error err = default!;

    if ((runtime.GOOS == "aix" || runtime.GOOS == "windows" || runtime.GOOS == "openbsd") && mode == "dial" && raddr.isWildcard()) {
        raddr = raddr.toLocal(net);
    }
    var (family, ipv6only) = favoriteAddrFamily(net, laddr, raddr, mode);
    return _addr_socket(ctx, net, family, sotype, proto, ipv6only, laddr, raddr, ctrlFn)!;
}

private static (syscall.Sockaddr, error) ipToSockaddr(nint family, IP ip, nint port, @string zone) {
    syscall.Sockaddr _p0 = default;
    error _p0 = default!;


    if (family == syscall.AF_INET) 
        if (len(ip) == 0) {
            ip = IPv4zero;
        }
        var ip4 = ip.To4();
        if (ip4 == null) {
            return (null, error.As(addr(new AddrError(Err:"non-IPv4 address",Addr:ip.String()))!)!);
        }
        ptr<syscall.SockaddrInet4> sa = addr(new syscall.SockaddrInet4(Port:port));
        copy(sa.Addr[..], ip4);
        return (sa, error.As(null!)!);
    else if (family == syscall.AF_INET6) 
        // In general, an IP wildcard address, which is either
        // "0.0.0.0" or "::", means the entire IP addressing
        // space. For some historical reason, it is used to
        // specify "any available address" on some operations
        // of IP node.
        //
        // When the IP node supports IPv4-mapped IPv6 address,
        // we allow a listener to listen to the wildcard
        // address of both IP addressing spaces by specifying
        // IPv6 wildcard address.
        if (len(ip) == 0 || ip.Equal(IPv4zero)) {
            ip = IPv6zero;
        }
        var ip6 = ip.To16();
        if (ip6 == null) {
            return (null, error.As(addr(new AddrError(Err:"non-IPv6 address",Addr:ip.String()))!)!);
        }
        sa = addr(new syscall.SockaddrInet6(Port:port,ZoneId:uint32(zoneCache.index(zone))));
        copy(sa.Addr[..], ip6);
        return (sa, error.As(null!)!);
        return (null, error.As(addr(new AddrError(Err:"invalid address family",Addr:ip.String()))!)!);
}

} // end net_package
