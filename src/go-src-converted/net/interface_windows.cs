// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package net -- go2cs converted at 2020 October 09 04:51:50 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\interface_windows.go
using windows = go.@internal.syscall.windows_package;
using os = go.os_package;
using syscall = go.syscall_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class net_package
    {
        // adapterAddresses returns a list of IP adapter and address
        // structures. The structure contains an IP adapter and flattened
        // multiple IP addresses including unicast, anycast and multicast
        // addresses.
        private static (slice<ptr<windows.IpAdapterAddresses>>, error) adapterAddresses()
        {
            slice<ptr<windows.IpAdapterAddresses>> _p0 = default;
            error _p0 = default!;

            slice<byte> b = default;
            ref var l = ref heap(uint32(15000L), out ptr<var> _addr_l); // recommended initial size
            while (true)
            {
                b = make_slice<byte>(l);
                var err = windows.GetAdaptersAddresses(syscall.AF_UNSPEC, windows.GAA_FLAG_INCLUDE_PREFIX, 0L, (windows.IpAdapterAddresses.val)(@unsafe.Pointer(_addr_b[0L])), _addr_l);
                if (err == null)
                {
                    if (l == 0L)
                    {
                        return (null, error.As(null!)!);
                    }
                    break;

                }
                if (err._<syscall.Errno>() != syscall.ERROR_BUFFER_OVERFLOW)
                {
                    return (null, error.As(os.NewSyscallError("getadaptersaddresses", err))!);
                }
                if (l <= uint32(len(b)))
                {
                    return (null, error.As(os.NewSyscallError("getadaptersaddresses", err))!);
                }
            }
            slice<ptr<windows.IpAdapterAddresses>> aas = default;
            {
                var aa = (windows.IpAdapterAddresses.val)(@unsafe.Pointer(_addr_b[0L]));

                while (aa != null)
                {
                    aas = append(aas, aa);
                    aa = aa.Next;
                }
            }
            return (aas, error.As(null!)!);

        }

        // If the ifindex is zero, interfaceTable returns mappings of all
        // network interfaces. Otherwise it returns a mapping of a specific
        // interface.
        private static (slice<Interface>, error) interfaceTable(long ifindex)
        {
            slice<Interface> _p0 = default;
            error _p0 = default!;

            var (aas, err) = adapterAddresses();
            if (err != null)
            {
                return (null, error.As(err)!);
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
                    Interface ifi = new Interface(Index:int(index),Name:windows.UTF16PtrToString(aa.FriendlyName),);
                    if (aa.OperStatus == windows.IfOperStatusUp)
                    {
                        ifi.Flags |= FlagUp;
                    } 
                    // For now we need to infer link-layer service
                    // capabilities from media types.
                    // TODO: use MIB_IF_ROW2.AccessType now that we no longer support
                    // Windows XP.

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
            return (ift, error.As(null!)!);

        }

        // If the ifi is nil, interfaceAddrTable returns addresses for all
        // network interfaces. Otherwise it returns addresses for a specific
        // interface.
        private static (slice<Addr>, error) interfaceAddrTable(ptr<Interface> _addr_ifi)
        {
            slice<Addr> _p0 = default;
            error _p0 = default!;
            ref Interface ifi = ref _addr_ifi.val;

            var (aas, err) = adapterAddresses();
            if (err != null)
            {
                return (null, error.As(err)!);
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
                        var puni = aa.FirstUnicastAddress;

                        while (puni != null)
                        {
                            var (sa, err) = puni.Address.Sockaddr.Sockaddr();
                            if (err != null)
                            {
                                return (null, error.As(os.NewSyscallError("sockaddr", err))!);
                            puni = puni.Next;
                            }

                            switch (sa.type())
                            {
                                case ptr<syscall.SockaddrInet4> sa:
                                    ifat = append(ifat, addr(new IPNet(IP:IPv4(sa.Addr[0],sa.Addr[1],sa.Addr[2],sa.Addr[3]),Mask:CIDRMask(int(puni.OnLinkPrefixLength),8*IPv4len))));
                                    break;
                                case ptr<syscall.SockaddrInet6> sa:
                                    ptr<IPNet> ifa = addr(new IPNet(IP:make(IP,IPv6len),Mask:CIDRMask(int(puni.OnLinkPrefixLength),8*IPv6len)));
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
                                return (null, error.As(os.NewSyscallError("sockaddr", err))!);
                            pany = pany.Next;
                            }

                            switch (sa.type())
                            {
                                case ptr<syscall.SockaddrInet4> sa:
                                    ifat = append(ifat, addr(new IPAddr(IP:IPv4(sa.Addr[0],sa.Addr[1],sa.Addr[2],sa.Addr[3]))));
                                    break;
                                case ptr<syscall.SockaddrInet6> sa:
                                    ifa = addr(new IPAddr(IP:make(IP,IPv6len)));
                                    copy(ifa.IP, sa.Addr[..]);
                                    ifat = append(ifat, ifa);
                                    break;
                            }

                        }

                    }

                }

            }
            return (ifat, error.As(null!)!);

        }

        // interfaceMulticastAddrTable returns addresses for a specific
        // interface.
        private static (slice<Addr>, error) interfaceMulticastAddrTable(ptr<Interface> _addr_ifi)
        {
            slice<Addr> _p0 = default;
            error _p0 = default!;
            ref Interface ifi = ref _addr_ifi.val;

            var (aas, err) = adapterAddresses();
            if (err != null)
            {
                return (null, error.As(err)!);
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
                                return (null, error.As(os.NewSyscallError("sockaddr", err))!);
                            pmul = pmul.Next;
                            }

                            switch (sa.type())
                            {
                                case ptr<syscall.SockaddrInet4> sa:
                                    ifat = append(ifat, addr(new IPAddr(IP:IPv4(sa.Addr[0],sa.Addr[1],sa.Addr[2],sa.Addr[3]))));
                                    break;
                                case ptr<syscall.SockaddrInet6> sa:
                                    ptr<IPAddr> ifa = addr(new IPAddr(IP:make(IP,IPv6len)));
                                    copy(ifa.IP, sa.Addr[..]);
                                    ifat = append(ifat, ifa);
                                    break;
                            }

                        }

                    }

                }

            }
            return (ifat, error.As(null!)!);

        }
    }
}
