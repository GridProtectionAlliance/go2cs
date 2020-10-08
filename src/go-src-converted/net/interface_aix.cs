// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package net -- go2cs converted at 2020 October 08 03:33:11 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\interface_aix.go
using poll = go.@internal.poll_package;
using unix = go.@internal.syscall.unix_package;
using syscall = go.syscall_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class net_package
    {
        private partial struct rawSockaddrDatalink
        {
            public byte Len;
            public byte Family;
            public ushort Index;
            public byte Type;
            public byte Nlen;
            public byte Alen;
            public byte Slen;
            public array<byte> Data;
        }

        private partial struct ifreq
        {
            public array<byte> Name;
            public array<byte> Ifru;
        }

        private static readonly ulong _KINFO_RT_IFLIST = (ulong)(0x1UL << (int)(8L)) | 3L | (1L << (int)(30L));



        private static readonly long _RTAX_NETMASK = (long)2L;

        private static readonly long _RTAX_IFA = (long)5L;

        private static readonly long _RTAX_MAX = (long)8L;



        private static (slice<byte>, error) getIfList()
        {
            slice<byte> _p0 = default;
            error _p0 = default!;

            var (needed, err) = syscall.Getkerninfo(_KINFO_RT_IFLIST, 0L, 0L, 0L);
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            var tab = make_slice<byte>(needed);
            _, err = syscall.Getkerninfo(_KINFO_RT_IFLIST, uintptr(@unsafe.Pointer(_addr_tab[0L])), uintptr(@unsafe.Pointer(_addr_needed)), 0L);
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            return (tab[..needed], error.As(null!)!);

        }

        // If the ifindex is zero, interfaceTable returns mappings of all
        // network interfaces. Otherwise it returns a mapping of a specific
        // interface.
        private static (slice<Interface>, error) interfaceTable(long ifindex) => func((defer, _, __) =>
        {
            slice<Interface> _p0 = default;
            error _p0 = default!;

            var (tab, err) = getIfList();
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            var (sock, err) = sysSocket(syscall.AF_INET, syscall.SOCK_DGRAM, 0L);
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            defer(poll.CloseFunc(sock));

            slice<Interface> ift = default;
            while (len(tab) > 0L)
            {
                var ifm = (syscall.IfMsgHdr.val)(@unsafe.Pointer(_addr_tab[0L]));
                if (ifm.Msglen == 0L)
                {
                    break;
                }

                if (ifm.Type == syscall.RTM_IFINFO)
                {
                    if (ifindex == 0L || ifindex == int(ifm.Index))
                    {
                        var sdl = (rawSockaddrDatalink.val)(@unsafe.Pointer(_addr_tab[syscall.SizeofIfMsghdr]));

                        ptr<Interface> ifi = addr(new Interface(Index:int(ifm.Index),Flags:linkFlags(ifm.Flags)));
                        ifi.Name = string(sdl.Data[..sdl.Nlen]);
                        ifi.HardwareAddr = sdl.Data[sdl.Nlen..sdl.Nlen + sdl.Alen]; 

                        // Retrieve MTU
                        ptr<ifreq> ifr = addr(new ifreq());
                        copy(ifr.Name[..], ifi.Name);
                        err = unix.Ioctl(sock, syscall.SIOCGIFMTU, uintptr(@unsafe.Pointer(ifr)));
                        if (err != null)
                        {
                            return (null, error.As(err)!);
                        }

                        ifi.MTU = int(ifr.Ifru[0L]) << (int)(24L) | int(ifr.Ifru[1L]) << (int)(16L) | int(ifr.Ifru[2L]) << (int)(8L) | int(ifr.Ifru[3L]);

                        ift = append(ift, ifi.val);
                        if (ifindex == int(ifm.Index))
                        {
                            break;
                        }

                    }

                }

                tab = tab[ifm.Msglen..];

            }


            return (ift, error.As(null!)!);

        });

        private static Flags linkFlags(int rawFlags)
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
        private static (slice<Addr>, error) interfaceAddrTable(ptr<Interface> _addr_ifi)
        {
            slice<Addr> _p0 = default;
            error _p0 = default!;
            ref Interface ifi = ref _addr_ifi.val;

            var (tab, err) = getIfList();
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            slice<Addr> ifat = default;
            while (len(tab) > 0L)
            {
                var ifm = (syscall.IfMsgHdr.val)(@unsafe.Pointer(_addr_tab[0L]));
                if (ifm.Msglen == 0L)
                {
                    break;
                }

                if (ifm.Type == syscall.RTM_NEWADDR)
                {
                    if (ifi == null || ifi.Index == int(ifm.Index))
                    {
                        var mask = ifm.Addrs;
                        var off = uint(syscall.SizeofIfMsghdr);

                        ptr<syscall.RawSockaddr> iprsa;                        ptr<syscall.RawSockaddr> nmrsa;

                        for (var i = uint(0L); i < _RTAX_MAX; i++)
                        {
                            if (mask & (1L << (int)(i)) == 0L)
                            {
                                continue;
                            }

                            var rsa = (syscall.RawSockaddr.val)(@unsafe.Pointer(_addr_tab[off]));
                            if (i == _RTAX_NETMASK)
                            {
                                nmrsa = rsa;
                            }

                            if (i == _RTAX_IFA)
                            {
                                iprsa = rsa;
                            }

                            off += (uint(rsa.Len) + 3L) & ~3L;

                        }

                        if (iprsa != null && nmrsa != null)
                        {
                            mask = default;
                            IP ip = default;


                            if (iprsa.Family == syscall.AF_INET) 
                                var ipsa = (syscall.RawSockaddrInet4.val)(@unsafe.Pointer(iprsa));
                                var nmsa = (syscall.RawSockaddrInet4.val)(@unsafe.Pointer(nmrsa));
                                ip = IPv4(ipsa.Addr[0L], ipsa.Addr[1L], ipsa.Addr[2L], ipsa.Addr[3L]);
                                mask = IPv4Mask(nmsa.Addr[0L], nmsa.Addr[1L], nmsa.Addr[2L], nmsa.Addr[3L]);
                            else if (iprsa.Family == syscall.AF_INET6) 
                                ipsa = (syscall.RawSockaddrInet6.val)(@unsafe.Pointer(iprsa));
                                nmsa = (syscall.RawSockaddrInet6.val)(@unsafe.Pointer(nmrsa));
                                ip = make(IP, IPv6len);
                                copy(ip, ipsa.Addr[..]);
                                mask = make(IPMask, IPv6len);
                                copy(mask, nmsa.Addr[..]);
                                                        ptr<IPNet> ifa = addr(new IPNet(IP:ip,Mask:mask));
                            ifat = append(ifat, ifa);

                        }

                    }

                }

                tab = tab[ifm.Msglen..];

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

            return (null, error.As(null!)!);
        }
    }
}
