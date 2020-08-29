// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package net -- go2cs converted at 2020 August 29 08:26:34 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\interface_linux.go
using os = go.os_package;
using syscall = go.syscall_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static unsafe partial class net_package
    {
        // If the ifindex is zero, interfaceTable returns mappings of all
        // network interfaces. Otherwise it returns a mapping of a specific
        // interface.
        private static (slice<Interface>, error) interfaceTable(long ifindex)
        {
            var (tab, err) = syscall.NetlinkRIB(syscall.RTM_GETLINK, syscall.AF_UNSPEC);
            if (err != null)
            {
                return (null, os.NewSyscallError("netlinkrib", err));
            }
            var (msgs, err) = syscall.ParseNetlinkMessage(tab);
            if (err != null)
            {
                return (null, os.NewSyscallError("parsenetlinkmessage", err));
            }
            slice<Interface> ift = default;
loop:
            foreach (var (_, m) in msgs)
            {

                if (m.Header.Type == syscall.NLMSG_DONE) 
                    _breakloop = true;
                    break;
                else if (m.Header.Type == syscall.RTM_NEWLINK) 
                    var ifim = (syscall.IfInfomsg.Value)(@unsafe.Pointer(ref m.Data[0L]));
                    if (ifindex == 0L || ifindex == int(ifim.Index))
                    {
                        var (attrs, err) = syscall.ParseNetlinkRouteAttr(ref m);
                        if (err != null)
                        {
                            return (null, os.NewSyscallError("parsenetlinkrouteattr", err));
                        }
                        ift = append(ift, newLink(ifim, attrs).Value);
                        if (ifindex == int(ifim.Index))
                        {
                            _breakloop = true;
                            break;
                        }
                    }
                            }            return (ift, null);
        }

 
        // See linux/if_arp.h.
        // Note that Linux doesn't support IPv4 over IPv6 tunneling.
        private static readonly long sysARPHardwareIPv4IPv4 = 768L; // IPv4 over IPv4 tunneling
        private static readonly long sysARPHardwareIPv6IPv6 = 769L; // IPv6 over IPv6 tunneling
        private static readonly long sysARPHardwareIPv6IPv4 = 776L; // IPv6 over IPv4 tunneling
        private static readonly long sysARPHardwareGREIPv4 = 778L; // any over GRE over IPv4 tunneling
        private static readonly long sysARPHardwareGREIPv6 = 823L; // any over GRE over IPv6 tunneling

        private static ref Interface newLink(ref syscall.IfInfomsg ifim, slice<syscall.NetlinkRouteAttr> attrs)
        {
            Interface ifi = ref new Interface(Index:int(ifim.Index),Flags:linkFlags(ifim.Flags));
            foreach (var (_, a) in attrs)
            {

                if (a.Attr.Type == syscall.IFLA_ADDRESS) 
                    // We never return any /32 or /128 IP address
                    // prefix on any IP tunnel interface as the
                    // hardware address.

                    if (len(a.Value) == IPv4len) 

                        if (ifim.Type == sysARPHardwareIPv4IPv4 || ifim.Type == sysARPHardwareGREIPv4 || ifim.Type == sysARPHardwareIPv6IPv4) 
                            continue;
                                            else if (len(a.Value) == IPv6len) 

                        if (ifim.Type == sysARPHardwareIPv6IPv6 || ifim.Type == sysARPHardwareGREIPv6) 
                            continue;
                                                                bool nonzero = default;
                    foreach (var (_, b) in a.Value)
                    {
                        if (b != 0L)
                        {
                            nonzero = true;
                            break;
                        }
                    }
                    if (nonzero)
                    {
                        ifi.HardwareAddr = a.Value[..];
                    }
                else if (a.Attr.Type == syscall.IFLA_IFNAME) 
                    ifi.Name = string(a.Value[..len(a.Value) - 1L]);
                else if (a.Attr.Type == syscall.IFLA_MTU) 
                    ifi.MTU = int(@unsafe.Pointer(ref a.Value[..4L][0L]).Value);
                            }
            return ifi;
        }

        private static Flags linkFlags(uint rawFlags)
        {
            Flags f = default;
            if (rawFlags & syscall.IFF_UP != 0L)
            {
                f |= FlagUp;
            }
            if (rawFlags & syscall.IFF_BROADCAST != 0L)
            {
                f |= FlagBroadcast;
            }
            if (rawFlags & syscall.IFF_LOOPBACK != 0L)
            {
                f |= FlagLoopback;
            }
            if (rawFlags & syscall.IFF_POINTOPOINT != 0L)
            {
                f |= FlagPointToPoint;
            }
            if (rawFlags & syscall.IFF_MULTICAST != 0L)
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
            var (tab, err) = syscall.NetlinkRIB(syscall.RTM_GETADDR, syscall.AF_UNSPEC);
            if (err != null)
            {
                return (null, os.NewSyscallError("netlinkrib", err));
            }
            var (msgs, err) = syscall.ParseNetlinkMessage(tab);
            if (err != null)
            {
                return (null, os.NewSyscallError("parsenetlinkmessage", err));
            }
            slice<Interface> ift = default;
            if (ifi == null)
            {
                error err = default;
                ift, err = interfaceTable(0L);
                if (err != null)
                {
                    return (null, err);
                }
            }
            var (ifat, err) = addrTable(ift, ifi, msgs);
            if (err != null)
            {
                return (null, err);
            }
            return (ifat, null);
        }

        private static (slice<Addr>, error) addrTable(slice<Interface> ift, ref Interface ifi, slice<syscall.NetlinkMessage> msgs)
        {
            slice<Addr> ifat = default;
loop:
            foreach (var (_, m) in msgs)
            {

                if (m.Header.Type == syscall.NLMSG_DONE) 
                    _breakloop = true;
                    break;
                else if (m.Header.Type == syscall.RTM_NEWADDR) 
                    var ifam = (syscall.IfAddrmsg.Value)(@unsafe.Pointer(ref m.Data[0L]));
                    if (len(ift) != 0L || ifi.Index == int(ifam.Index))
                    {
                        if (len(ift) != 0L)
                        {
                            error err = default;
                            ifi, err = interfaceByIndex(ift, int(ifam.Index));
                            if (err != null)
                            {
                                return (null, err);
                            }
                        }
                        var (attrs, err) = syscall.ParseNetlinkRouteAttr(ref m);
                        if (err != null)
                        {
                            return (null, os.NewSyscallError("parsenetlinkrouteattr", err));
                        }
                        var ifa = newAddr(ifam, attrs);
                        if (ifa != null)
                        {
                            ifat = append(ifat, ifa);
                        }
                    }
                            }
            return (ifat, null);
        }

        private static Addr newAddr(ref syscall.IfAddrmsg ifam, slice<syscall.NetlinkRouteAttr> attrs)
        {
            bool ipPointToPoint = default; 
            // Seems like we need to make sure whether the IP interface
            // stack consists of IP point-to-point numbered or unnumbered
            // addressing.
            {
                var a__prev1 = a;

                foreach (var (_, __a) in attrs)
                {
                    a = __a;
                    if (a.Attr.Type == syscall.IFA_LOCAL)
                    {
                        ipPointToPoint = true;
                        break;
                    }
                }

                a = a__prev1;
            }

            {
                var a__prev1 = a;

                foreach (var (_, __a) in attrs)
                {
                    a = __a;
                    if (ipPointToPoint && a.Attr.Type == syscall.IFA_ADDRESS)
                    {
                        continue;
                    }

                    if (ifam.Family == syscall.AF_INET) 
                        return ref new IPNet(IP:IPv4(a.Value[0],a.Value[1],a.Value[2],a.Value[3]),Mask:CIDRMask(int(ifam.Prefixlen),8*IPv4len));
                    else if (ifam.Family == syscall.AF_INET6) 
                        IPNet ifa = ref new IPNet(IP:make(IP,IPv6len),Mask:CIDRMask(int(ifam.Prefixlen),8*IPv6len));
                        copy(ifa.IP, a.Value[..]);
                        return ifa;
                                    }

                a = a__prev1;
            }

            return null;
        }

        // interfaceMulticastAddrTable returns addresses for a specific
        // interface.
        private static (slice<Addr>, error) interfaceMulticastAddrTable(ref Interface ifi)
        {
            var ifmat4 = parseProcNetIGMP("/proc/net/igmp", ifi);
            var ifmat6 = parseProcNetIGMP6("/proc/net/igmp6", ifi);
            return (append(ifmat4, ifmat6), null);
        }

        private static slice<Addr> parseProcNetIGMP(@string path, ref Interface _ifi) => func(_ifi, (ref Interface ifi, Defer defer, Panic _, Recover __) =>
        {
            var (fd, err) = open(path);
            if (err != null)
            {
                return null;
            }
            defer(fd.close());
            slice<Addr> ifmat = default;            @string name = default;
            fd.readLine(); // skip first line
            var b = make_slice<byte>(IPv4len);
            {
                var (l, ok) = fd.readLine();

                while (ok)
                {
                    var f = splitAtBytes(l, " :\r\t\n");
                    if (len(f) < 4L)
                    {
                        continue;
                    l, ok = fd.readLine();
                    }

                    if (l[0L] != ' ' && l[0L] != '\t') // new interface line
                        name = f[1L];
                    else if (len(f[0L]) == 8L) 
                        if (ifi == null || name == ifi.Name)
                        { 
                            // The Linux kernel puts the IP
                            // address in /proc/net/igmp in native
                            // endianness.
                            {
                                long i__prev2 = i;

                                long i = 0L;

                                while (i + 1L < len(f[0L]))
                                {
                                    b[i / 2L], _ = xtoi2(f[0L][i..i + 2L], 0L);
                                    i += 2L;
                                }


                                i = i__prev2;
                            }
                            i = @unsafe.Pointer(ref b[..4L][0L]).Value;
                            IPAddr ifma = ref new IPAddr(IP:IPv4(byte(i>>24),byte(i>>16),byte(i>>8),byte(i)));
                            ifmat = append(ifmat, ifma);
                        }
                                    }

            }
            return ifmat;
        });

        private static slice<Addr> parseProcNetIGMP6(@string path, ref Interface _ifi) => func(_ifi, (ref Interface ifi, Defer defer, Panic _, Recover __) =>
        {
            var (fd, err) = open(path);
            if (err != null)
            {
                return null;
            }
            defer(fd.close());
            slice<Addr> ifmat = default;
            var b = make_slice<byte>(IPv6len);
            {
                var (l, ok) = fd.readLine();

                while (ok)
                {
                    var f = splitAtBytes(l, " \r\t\n");
                    if (len(f) < 6L)
                    {
                        continue;
                    l, ok = fd.readLine();
                    }
                    if (ifi == null || f[1L] == ifi.Name)
                    {
                        {
                            long i = 0L;

                            while (i + 1L < len(f[2L]))
                            {
                                b[i / 2L], _ = xtoi2(f[2L][i..i + 2L], 0L);
                                i += 2L;
                            }

                        }
                        IPAddr ifma = ref new IPAddr(IP:IP{b[0],b[1],b[2],b[3],b[4],b[5],b[6],b[7],b[8],b[9],b[10],b[11],b[12],b[13],b[14],b[15]});
                        ifmat = append(ifmat, ifma);
                    }
                }

            }
            return ifmat;
        });
    }
}
