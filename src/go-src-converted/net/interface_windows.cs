// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package net -- go2cs converted at 2020 August 29 08:26:39 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\interface_windows.go
using windows = go.@internal.syscall.windows_package;
using os = go.os_package;
using syscall = go.syscall_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static unsafe partial class net_package
    {
        // supportsVistaIP reports whether the platform implements new IP
        // stack and ABIs supported on Windows Vista and above.
        private static bool supportsVistaIP = default;

        private static void init()
        {
            supportsVistaIP = probeWindowsIPStack();
        }

        private static bool probeWindowsIPStack()
        {
            var (v, err) = syscall.GetVersion();
            if (err != null)
            {
                return true; // Windows 10 and above will deprecate this API
            }
            return byte(v) >= 6L; // major version of Windows Vista is 6
        }

        // adapterAddresses returns a list of IP adapter and address
        // structures. The structure contains an IP adapter and flattened
        // multiple IP addresses including unicast, anycast and multicast
        // addresses.
        private static (slice<ref windows.IpAdapterAddresses>, error) adapterAddresses()
        {
            slice<byte> b = default;
            var l = uint32(15000L); // recommended initial size
            while (true)
            {
                b = make_slice<byte>(l);
                var err = windows.GetAdaptersAddresses(syscall.AF_UNSPEC, windows.GAA_FLAG_INCLUDE_PREFIX, 0L, (windows.IpAdapterAddresses.Value)(@unsafe.Pointer(ref b[0L])), ref l);
                if (err == null)
                {
                    if (l == 0L)
                    {
                        return (null, null);
                    }
                    break;
                }
                if (err._<syscall.Errno>() != syscall.ERROR_BUFFER_OVERFLOW)
                {
                    return (null, os.NewSyscallError("getadaptersaddresses", err));
                }
                if (l <= uint32(len(b)))
                {
                    return (null, os.NewSyscallError("getadaptersaddresses", err));
                }
            }

            slice<ref windows.IpAdapterAddresses> aas = default;
            {
                var aa = (windows.IpAdapterAddresses.Value)(@unsafe.Pointer(ref b[0L]));

                while (aa != null)
                {
                    aas = append(aas, aa);
                    aa = aa.Next;
                }

            }
            return (aas, null);
        }

        // If the ifindex is zero, interfaceTable returns mappings of all
        // network interfaces. Otherwise it returns a mapping of a specific
        // interface.
        private static (slice<Interface>, error) interfaceTable(long ifindex)
        {
            var (aas, err) = adapterAddresses();
            if (err != null)
            {
                return (null, err);
            }
            slice<Interface> ift = default;
            foreach (var (_, aa) in aas)
            {
                var index = aa.IfIndex;
                if (index == 0L)
                { // ipv6IfIndex is a substitute for ifIndex
                    index = aa.Ipv6IfIndex;
                }
                if (ifindex == 0L || ifindex == int(index))
                {
                    Interface ifi = new Interface(Index:int(index),Name:syscall.UTF16ToString((*(*[10000]uint16)(unsafe.Pointer(aa.FriendlyName)))[:]),);
                    if (aa.OperStatus == windows.IfOperStatusUp)
                    {
                        ifi.Flags |= FlagUp;
                    } 
                    // For now we need to infer link-layer service
                    // capabilities from media types.
                    // We will be able to use
                    // MIB_IF_ROW2.AccessType once we drop support
                    // for Windows XP.

                    if (aa.IfType == windows.IF_TYPE_ETHERNET_CSMACD || aa.IfType == windows.IF_TYPE_ISO88025_TOKENRING || aa.IfType == windows.IF_TYPE_IEEE80211 || aa.IfType == windows.IF_TYPE_IEEE1394) 
                        ifi.Flags |= FlagBroadcast | FlagMulticast;
                    else if (aa.IfType == windows.IF_TYPE_PPP || aa.IfType == windows.IF_TYPE_TUNNEL) 
                        ifi.Flags |= FlagPointToPoint | FlagMulticast;
                    else if (aa.IfType == windows.IF_TYPE_SOFTWARE_LOOPBACK) 
                        ifi.Flags |= FlagLoopback | FlagMulticast;
                    else if (aa.IfType == windows.IF_TYPE_ATM) 
                        ifi.Flags |= FlagBroadcast | FlagPointToPoint | FlagMulticast; // assume all services available; LANE, point-to-point and point-to-multipoint
                                        if (aa.Mtu == 0xffffffffUL)
                    {
                        ifi.MTU = -1L;
                    }
                    else
                    {
                        ifi.MTU = int(aa.Mtu);
                    }
                    if (aa.PhysicalAddressLength > 0L)
                    {
                        ifi.HardwareAddr = make(HardwareAddr, aa.PhysicalAddressLength);
                        copy(ifi.HardwareAddr, aa.PhysicalAddress[..]);
                    }
                    ift = append(ift, ifi);
                    if (ifindex == ifi.Index)
                    {
                        break;
                    }
                }
            }
            return (ift, null);
        }

        // If the ifi is nil, interfaceAddrTable returns addresses for all
        // network interfaces. Otherwise it returns addresses for a specific
        // interface.
        private static (slice<Addr>, error) interfaceAddrTable(ref Interface ifi)
        {
            var (aas, err) = adapterAddresses();
            if (err != null)
            {
                return (null, err);
            }
            slice<Addr> ifat = default;
            foreach (var (_, aa) in aas)
            {
                var index = aa.IfIndex;
                if (index == 0L)
                { // ipv6IfIndex is a substitute for ifIndex
                    index = aa.Ipv6IfIndex;
                }
                slice<IPNet> pfx4 = default;                slice<IPNet> pfx6 = default;

                if (!supportsVistaIP)
                {
                    pfx4, pfx6, err = addrPrefixTable(aa);
                    if (err != null)
                    {
                        return (null, err);
                    }
                }
                if (ifi == null || ifi.Index == int(index))
                {
                    {
                        var puni = aa.FirstUnicastAddress;

                        while (puni != null)
                        {
                            var (sa, err) = puni.Address.Sockaddr.Sockaddr();
                            if (err != null)
                            {
                                return (null, os.NewSyscallError("sockaddr", err));
                            puni = puni.Next;
                            }
                            long l = default;
                            switch (sa.type())
                            {
                                case ref syscall.SockaddrInet4 sa:
                                    if (supportsVistaIP)
                                    {
                                        l = int(puni.OnLinkPrefixLength);
                                    }
                                    else
                                    {
                                        l = addrPrefixLen(pfx4, IP(sa.Addr[..]));
                                    }
                                    ifat = append(ifat, ref new IPNet(IP:IPv4(sa.Addr[0],sa.Addr[1],sa.Addr[2],sa.Addr[3]),Mask:CIDRMask(l,8*IPv4len)));
                                    break;
                                case ref syscall.SockaddrInet6 sa:
                                    if (supportsVistaIP)
                                    {
                                        l = int(puni.OnLinkPrefixLength);
                                    }
                                    else
                                    {
                                        l = addrPrefixLen(pfx6, IP(sa.Addr[..]));
                                    }
                                    IPNet ifa = ref new IPNet(IP:make(IP,IPv6len),Mask:CIDRMask(l,8*IPv6len));
                                    copy(ifa.IP, sa.Addr[..]);
                                    ifat = append(ifat, ifa);
                                    break;
                            }
                        }

                    }
                    {
                        var pany = aa.FirstAnycastAddress;

                        while (pany != null)
                        {
                            (sa, err) = pany.Address.Sockaddr.Sockaddr();
                            if (err != null)
                            {
                                return (null, os.NewSyscallError("sockaddr", err));
                            pany = pany.Next;
                            }
                            switch (sa.type())
                            {
                                case ref syscall.SockaddrInet4 sa:
                                    ifat = append(ifat, ref new IPAddr(IP:IPv4(sa.Addr[0],sa.Addr[1],sa.Addr[2],sa.Addr[3])));
                                    break;
                                case ref syscall.SockaddrInet6 sa:
                                    ifa = ref new IPAddr(IP:make(IP,IPv6len));
                                    copy(ifa.IP, sa.Addr[..]);
                                    ifat = append(ifat, ifa);
                                    break;
                            }
                        }

                    }
                }
            }
            return (ifat, null);
        }

        private static (slice<IPNet>, slice<IPNet>, error) addrPrefixTable(ref windows.IpAdapterAddresses aa)
        {
            {
                var p = aa.FirstPrefix;

                while (p != null)
                {
                    var (sa, err) = p.Address.Sockaddr.Sockaddr();
                    if (err != null)
                    {
                        return (null, null, os.NewSyscallError("sockaddr", err));
                    p = p.Next;
                    }
                    switch (sa.type())
                    {
                        case ref syscall.SockaddrInet4 sa:
                            IPNet pfx = new IPNet(IP:IP(sa.Addr[:]),Mask:CIDRMask(int(p.PrefixLength),8*IPv4len));
                            pfx4 = append(pfx4, pfx);
                            break;
                        case ref syscall.SockaddrInet6 sa:
                            pfx = new IPNet(IP:IP(sa.Addr[:]),Mask:CIDRMask(int(p.PrefixLength),8*IPv6len));
                            pfx6 = append(pfx6, pfx);
                            break;
                    }
                }

            }
            return;
        }

        // addrPrefixLen returns an appropriate prefix length in bits for ip
        // from pfxs. It returns 32 or 128 when no appropriate on-link address
        // prefix found.
        //
        // NOTE: This is pretty naive implementation that contains many
        // allocations and non-effective linear search, and should not be used
        // freely.
        private static long addrPrefixLen(slice<IPNet> pfxs, IP ip)
        {
            long l = default;
            ref IPNet cand = default;
            foreach (var (i) in pfxs)
            {
                if (!pfxs[i].Contains(ip))
                {
                    continue;
                }
                if (cand == null)
                {
                    l, _ = pfxs[i].Mask.Size();
                    cand = ref pfxs[i];
                    continue;
                }
                var (m, _) = pfxs[i].Mask.Size();
                if (m > l)
                {
                    l = m;
                    cand = ref pfxs[i];
                    continue;
                }
            }
            if (l > 0L)
            {
                return l;
            }
            if (ip.To4() != null)
            {
                return 8L * IPv4len;
            }
            return 8L * IPv6len;
        }

        // interfaceMulticastAddrTable returns addresses for a specific
        // interface.
        private static (slice<Addr>, error) interfaceMulticastAddrTable(ref Interface ifi)
        {
            var (aas, err) = adapterAddresses();
            if (err != null)
            {
                return (null, err);
            }
            slice<Addr> ifat = default;
            foreach (var (_, aa) in aas)
            {
                var index = aa.IfIndex;
                if (index == 0L)
                { // ipv6IfIndex is a substitute for ifIndex
                    index = aa.Ipv6IfIndex;
                }
                if (ifi == null || ifi.Index == int(index))
                {
                    {
                        var pmul = aa.FirstMulticastAddress;

                        while (pmul != null)
                        {
                            var (sa, err) = pmul.Address.Sockaddr.Sockaddr();
                            if (err != null)
                            {
                                return (null, os.NewSyscallError("sockaddr", err));
                            pmul = pmul.Next;
                            }
                            switch (sa.type())
                            {
                                case ref syscall.SockaddrInet4 sa:
                                    ifat = append(ifat, ref new IPAddr(IP:IPv4(sa.Addr[0],sa.Addr[1],sa.Addr[2],sa.Addr[3])));
                                    break;
                                case ref syscall.SockaddrInet6 sa:
                                    IPAddr ifa = ref new IPAddr(IP:make(IP,IPv6len));
                                    copy(ifa.IP, sa.Addr[..]);
                                    ifat = append(ifat, ifa);
                                    break;
                            }
                        }

                    }
                }
            }
            return (ifat, null);
        }
    }
}
