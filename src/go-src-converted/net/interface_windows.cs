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
internal static (slice<windows.IpAdapterAddresses>, error) adapterAddresses() {
    slice<byte> b = default!;
    ref var l = ref heap<uint32>(out var Ꮡl);
    l = ((uint32)15000);
    // recommended initial size
    while (ᐧ) {
        b = new slice<byte>(l);
        static readonly UntypedInt flags = /* windows.GAA_FLAG_INCLUDE_PREFIX | windows.GAA_FLAG_INCLUDE_GATEWAYS */ 144;
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
        if (l <= ((uint32)len(b))) {
            return (default!, os.NewSyscallError("getadaptersaddresses"u8, err));
        }
    }
    slice<windows.IpAdapterAddresses> aas = default!;
    for (var aa = (ж<windows.IpAdapterAddresses>)(uintptr)(new @unsafe.Pointer(Ꮡ(b, 0))); aa != nil; aa = aa.val.Next) {
        aas = append(aas, aa);
    }
    return (aas, default!);
}

// If the ifindex is zero, interfaceTable returns mappings of all
// network interfaces. Otherwise it returns a mapping of a specific
// interface.
internal static (slice<Interface>, error) interfaceTable(nint ifindex) {
    (aas, err) = adapterAddresses();
    if (err != default!) {
        return (default!, err);
    }
    slice<Interface> ift = default!;
    foreach (var (_, aa) in aas) {
        var index = aa.val.IfIndex;
        if (index == 0) {
            // ipv6IfIndex is a substitute for ifIndex
            index = aa.val.Ipv6IfIndex;
        }
        if (ifindex == 0 || ifindex == ((nint)index)) {
            var ifi = new Interface(
                Index: ((nint)index),
                Name: windows.UTF16PtrToString((~aa).FriendlyName)
            );
            if ((~aa).OperStatus == windows.IfOperStatusUp) {
                ifi.Flags |= (Flags)(FlagUp);
                ifi.Flags |= (Flags)(FlagRunning);
            }
            // For now we need to infer link-layer service
            // capabilities from media types.
            // TODO: use MIB_IF_ROW2.AccessType now that we no longer support
            // Windows XP.
            switch ((~aa).IfType) {
            case windows.IF_TYPE_ETHERNET_CSMACD or windows.IF_TYPE_ISO88025_TOKENRING or windows.IF_TYPE_IEEE80211 or windows.IF_TYPE_IEEE1394: {
                ifi.Flags |= (Flags)((Flags)(FlagBroadcast | FlagMulticast));
                break;
            }
            case windows.IF_TYPE_PPP or windows.IF_TYPE_TUNNEL: {
                ifi.Flags |= (Flags)((Flags)(FlagPointToPoint | FlagMulticast));
                break;
            }
            case windows.IF_TYPE_SOFTWARE_LOOPBACK: {
                ifi.Flags |= (Flags)((Flags)(FlagLoopback | FlagMulticast));
                break;
            }
            case windows.IF_TYPE_ATM: {
                ifi.Flags |= (Flags)((Flags)((Flags)(FlagBroadcast | FlagPointToPoint) | FlagMulticast));
                break;
            }}

            // assume all services available; LANE, point-to-point and point-to-multipoint
            if ((~aa).Mtu == (nint)4294967295L){
                ifi.MTU = -1;
            } else {
                ifi.MTU = ((nint)(~aa).Mtu);
            }
            if ((~aa).PhysicalAddressLength > 0) {
                ifi.HardwareAddr = new HardwareAddr((~aa).PhysicalAddressLength);
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
    ref var ifi = ref Ꮡifi.val;

    (aas, err) = adapterAddresses();
    if (err != default!) {
        return (default!, err);
    }
    slice<ΔAddr> ifat = default!;
    foreach (var (_, aa) in aas) {
        var index = aa.val.IfIndex;
        if (index == 0) {
            // ipv6IfIndex is a substitute for ifIndex
            index = aa.val.Ipv6IfIndex;
        }
        if (ifi == nil || ifi.Index == ((nint)index)) {
            for (var puni = aa.val.FirstUnicastAddress; puni != nil; puni = puni.val.Next) {
                (sa, errΔ1) = (~puni).Address.Sockaddr.Sockaddr();
                if (errΔ1 != default!) {
                    return (default!, os.NewSyscallError("sockaddr"u8, errΔ1));
                }
                switch (sa.type()) {
                case ж<syscall.SockaddrInet4> sa: {
                    ifat = append(ifat, new IPNet(IP: IPv4((~sa).Addr[0], (~sa).Addr[1], (~sa).Addr[2], (~sa).Addr[3]), Mask: CIDRMask(((nint)(~puni).OnLinkPrefixLength), 8 * IPv4len)));
                    break;
                }
                case ж<syscall.SockaddrInet6> sa: {
                    var ifa = Ꮡ(new IPNet(IP: new IP(IPv6len), Mask: CIDRMask(((nint)(~puni).OnLinkPrefixLength), 8 * IPv6len)));
                    copy((~ifa).IP, (~sa).Addr[..]);
                    ifat = append(ifat, ~ifa);
                    break;
                }}
            }
            for (var pany = aa.val.FirstAnycastAddress; pany != nil; pany = pany.val.Next) {
                (sa, errΔ2) = (~pany).Address.Sockaddr.Sockaddr();
                if (errΔ2 != default!) {
                    return (default!, os.NewSyscallError("sockaddr"u8, errΔ2));
                }
                switch (sa.type()) {
                case ж<syscall.SockaddrInet4> sa: {
                    ifat = append(ifat, new IPAddr(IP: IPv4((~sa).Addr[0], (~sa).Addr[1], (~sa).Addr[2], (~sa).Addr[3])));
                    break;
                }
                case ж<syscall.SockaddrInet6> sa: {
                    var ifa = Ꮡ(new IPAddr(IP: new IP(IPv6len)));
                    copy((~ifa).IP, (~sa).Addr[..]);
                    ifat = append(ifat, ~ifa);
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
    ref var ifi = ref Ꮡifi.val;

    (aas, err) = adapterAddresses();
    if (err != default!) {
        return (default!, err);
    }
    slice<ΔAddr> ifat = default!;
    foreach (var (_, aa) in aas) {
        var index = aa.val.IfIndex;
        if (index == 0) {
            // ipv6IfIndex is a substitute for ifIndex
            index = aa.val.Ipv6IfIndex;
        }
        if (ifi == nil || ifi.Index == ((nint)index)) {
            for (var pmul = aa.val.FirstMulticastAddress; pmul != nil; pmul = pmul.val.Next) {
                (sa, errΔ1) = (~pmul).Address.Sockaddr.Sockaddr();
                if (errΔ1 != default!) {
                    return (default!, os.NewSyscallError("sockaddr"u8, errΔ1));
                }
                switch (sa.type()) {
                case ж<syscall.SockaddrInet4> sa: {
                    ifat = append(ifat, new IPAddr(IP: IPv4((~sa).Addr[0], (~sa).Addr[1], (~sa).Addr[2], (~sa).Addr[3])));
                    break;
                }
                case ж<syscall.SockaddrInet6> sa: {
                    var ifa = Ꮡ(new IPAddr(IP: new IP(IPv6len)));
                    copy((~ifa).IP, (~sa).Addr[..]);
                    ifat = append(ifat, ~ifa);
                    break;
                }}
            }
        }
    }
    return (ifat, default!);
}

} // end net_package
