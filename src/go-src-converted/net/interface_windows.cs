// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using windows = @internal.syscall.windows_package;
using os = os_package;
using syscall = syscall_package;
using @unsafe = unsafe_package;
using @internal.syscall;

partial class net_package {

// adapterAddresses returns a list of IP adapter and address
// structures. The structure contains an IP adapter and flattened
// multiple IP addresses including unicast, anycast and multicast
// addresses.
internal static (slice<ж<windows.IpAdapterAddresses>>, error) adapterAddresses() {
    slice<byte> b = default!;
    ref var l = ref heap<uint32>(out var Ꮡl);
    l = (uint32)15000;
    // recommended initial size
    while (ᐧ) {
        b = new slice<byte>((nint)(l));
        UntypedInt flags = /* windows.GAA_FLAG_INCLUDE_PREFIX | windows.GAA_FLAG_INCLUDE_GATEWAYS */ 144;
        var err = windows.GetAdaptersAddresses(syscall.AF_UNSPEC, flags, 0, (ж<windows.IpAdapterAddresses>)(uintptr)(new @unsafe.Pointer(Ꮡ(b, 0))), Ꮡl);
        if (err == default!) {
            if (l == 0) {
                return (default!, default!);
            }
            break;
        }
        if (err._<syscall.Errno>() != syscall.ERROR_BUFFER_OVERFLOW) {
            return (default!, os.NewSyscallError("getadaptersaddresses"u8, err));
        }
        if (l <= (uint32)len(b)) {
            return (default!, os.NewSyscallError("getadaptersaddresses"u8, err));
        }
    }
    slice<ж<windows.IpAdapterAddresses>> aas = default!;
    for (var aa = (ж<windows.IpAdapterAddresses>)(uintptr)(new @unsafe.Pointer(Ꮡ(b, 0))); aa != nil; aa = aa.Value.Next) {
        aas = append(aas, aa);
    }
    return (aas, default!);
}

// If the ifindex is zero, interfaceTable returns mappings of all
// network interfaces. Otherwise it returns a mapping of a specific
// interface.
internal static (slice<Interface>, error) interfaceTable(nint ifindex) {
    var (aas, err) = adapterAddresses();
    if (err != default!) {
        return (default!, err);
    }
    slice<Interface> ift = default!;
    foreach (var (_, aa) in aas) {
        var index = aa.Value.IfIndex;
        if (index == 0) {
            // ipv6IfIndex is a substitute for ifIndex
            index = aa.Value.Ipv6IfIndex;
        }
        if (ifindex == 0 || ifindex == (nint)index) {
            var ifi = new Interface(
                Index: (nint)index,
                Name: windows.UTF16PtrToString((~aa).FriendlyName)
            );
            if ((~aa).OperStatus == windows.IfOperStatusUp) {
                ifi.Flags |= FlagUp;
                ifi.Flags |= FlagRunning;
            }
            // For now we need to infer link-layer service
            // capabilities from media types.
            // TODO: use MIB_IF_ROW2.AccessType now that we no longer support
            // Windows XP.
            var exprᴛ1 = (~aa).IfType;
            if (exprᴛ1 == windows.IF_TYPE_ETHERNET_CSMACD || exprᴛ1 == windows.IF_TYPE_ISO88025_TOKENRING || exprᴛ1 == windows.IF_TYPE_IEEE80211 || exprᴛ1 == windows.IF_TYPE_IEEE1394) {
                ifi.Flags |= (Flags)(FlagBroadcast | FlagMulticast);
            }
            else if (exprᴛ1 == windows.IF_TYPE_PPP || exprᴛ1 == windows.IF_TYPE_TUNNEL) {
                ifi.Flags |= (Flags)(FlagPointToPoint | FlagMulticast);
            }
            else if (exprᴛ1 == windows.IF_TYPE_SOFTWARE_LOOPBACK) {
                ifi.Flags |= (Flags)(FlagLoopback | FlagMulticast);
            }
            else if (exprᴛ1 == windows.IF_TYPE_ATM) {
                ifi.Flags |= (Flags)((Flags)(FlagBroadcast | FlagPointToPoint) | FlagMulticast);
            }

            // assume all services available; LANE, point-to-point and point-to-multipoint
            if ((~aa).Mtu == 0xffffffffU){
                ifi.MTU = -1;
            } else {
                ifi.MTU = (nint)(~aa).Mtu;
            }
            if ((~aa).PhysicalAddressLength > 0) {
                ifi.HardwareAddr = new HardwareAddr((nint)((~aa).PhysicalAddressLength));
                copy(ifi.HardwareAddr, (~aa).PhysicalAddress[..]);
            }
            ift = append(ift, ifi);
            if (ifindex == ifi.Index) {
                break;
            }
        }
    }
    return (ift, default!);
}

// If the ifi is nil, interfaceAddrTable returns addresses for all
// network interfaces. Otherwise it returns addresses for a specific
// interface.
internal static (slice<ΔAddr>, error) interfaceAddrTable(ж<Interface> Ꮡifi) {
    ref var ifi = ref Ꮡifi.DerefOrNil();

    var (aas, err) = adapterAddresses();
    if (err != default!) {
        return (default!, err);
    }
    slice<ΔAddr> ifat = default!;
    foreach (var (_, aa) in aas) {
        var index = aa.Value.IfIndex;
        if (index == 0) {
            // ipv6IfIndex is a substitute for ifIndex
            index = aa.Value.Ipv6IfIndex;
        }
        if (Ꮡifi == nil || ifi.Index == (nint)index) {
            for (var puni = aa.Value.FirstUnicastAddress; puni != nil; puni = puni.Value.Next) {
                var (sa, errΔ1) = (~puni).Address.Sockaddr.Sockaddr();
                if (errΔ1 != default!) {
                    return (default!, os.NewSyscallError("sockaddr"u8, errΔ1));
                }
                switch (sa.type()) {
                case ж<syscall.SockaddrInet4> saΔ1: {
                    ifat = append(ifat, (ΔAddr)(new IPNetжΔAddr(Ꮡ(new IPNet(IP: IPv4((~saΔ1).Addr[0], (~saΔ1).Addr[1], (~saΔ1).Addr[2], (~saΔ1).Addr[3]), Mask: CIDRMask((nint)(~puni).OnLinkPrefixLength, 8 * IPv4len))))));
                    break;
                }
                case ж<syscall.SockaddrInet6> saΔ1: {
                    var ifa = Ꮡ(new IPNet(IP: new IP(IPv6len), Mask: CIDRMask((nint)(~puni).OnLinkPrefixLength, 8 * IPv6len)));
                    copy((~ifa).IP, (~saΔ1).Addr[..]);
                    ifat = append(ifat, (ΔAddr)(new IPNetжΔAddr(ifa)));
                    break;
                }}
            }
            for (var pany = aa.Value.FirstAnycastAddress; pany != nil; pany = pany.Value.Next) {
                var (sa, errΔ2) = (~pany).Address.Sockaddr.Sockaddr();
                if (errΔ2 != default!) {
                    return (default!, os.NewSyscallError("sockaddr"u8, errΔ2));
                }
                switch (sa.type()) {
                case ж<syscall.SockaddrInet4> saΔ1: {
                    ifat = append(ifat, (ΔAddr)(new IPAddrжΔAddr(Ꮡ(new IPAddr(IP: IPv4((~saΔ1).Addr[0], (~saΔ1).Addr[1], (~saΔ1).Addr[2], (~saΔ1).Addr[3]))))));
                    break;
                }
                case ж<syscall.SockaddrInet6> saΔ1: {
                    var ifa = Ꮡ(new IPAddr(IP: new IP(IPv6len)));
                    copy((~ifa).IP, (~saΔ1).Addr[..]);
                    ifat = append(ifat, (ΔAddr)(new IPAddrжΔAddr(ifa)));
                    break;
                }}
            }
        }
    }
    return (ifat, default!);
}

// interfaceMulticastAddrTable returns addresses for a specific
// interface.
internal static (slice<ΔAddr>, error) interfaceMulticastAddrTable(ж<Interface> Ꮡifi) {
    ref var ifi = ref Ꮡifi.DerefOrNil();

    var (aas, err) = adapterAddresses();
    if (err != default!) {
        return (default!, err);
    }
    slice<ΔAddr> ifat = default!;
    foreach (var (_, aa) in aas) {
        var index = aa.Value.IfIndex;
        if (index == 0) {
            // ipv6IfIndex is a substitute for ifIndex
            index = aa.Value.Ipv6IfIndex;
        }
        if (Ꮡifi == nil || ifi.Index == (nint)index) {
            for (var pmul = aa.Value.FirstMulticastAddress; pmul != nil; pmul = pmul.Value.Next) {
                var (sa, errΔ1) = (~pmul).Address.Sockaddr.Sockaddr();
                if (errΔ1 != default!) {
                    return (default!, os.NewSyscallError("sockaddr"u8, errΔ1));
                }
                switch (sa.type()) {
                case ж<syscall.SockaddrInet4> saΔ1: {
                    ifat = append(ifat, (ΔAddr)(new IPAddrжΔAddr(Ꮡ(new IPAddr(IP: IPv4((~saΔ1).Addr[0], (~saΔ1).Addr[1], (~saΔ1).Addr[2], (~saΔ1).Addr[3]))))));
                    break;
                }
                case ж<syscall.SockaddrInet6> saΔ1: {
                    var ifa = Ꮡ(new IPAddr(IP: new IP(IPv6len)));
                    copy((~ifa).IP, (~saΔ1).Addr[..]);
                    ifat = append(ifat, (ΔAddr)(new IPAddrжΔAddr(ifa)));
                    break;
                }}
            }
        }
    }
    return (ifat, default!);
}

} // end net_package
