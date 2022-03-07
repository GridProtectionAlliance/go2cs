// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package net -- go2cs converted at 2022 March 06 22:15:52 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Program Files\Go\src\net\interface_aix.go
using poll = go.@internal.poll_package;
using unix = go.@internal.syscall.unix_package;
using syscall = go.syscall_package;
using @unsafe = go.@unsafe_package;

namespace go;

public static partial class net_package {

private partial struct rawSockaddrDatalink {
    public byte Len;
    public byte Family;
    public ushort Index;
    public byte Type;
    public byte Nlen;
    public byte Alen;
    public byte Slen;
    public array<byte> Data;
}

private partial struct ifreq {
    public array<byte> Name;
    public array<byte> Ifru;
}

private static readonly nuint _KINFO_RT_IFLIST = (0x1 << 8) | 3 | (1 << 30);



private static readonly nint _RTAX_NETMASK = 2;

private static readonly nint _RTAX_IFA = 5;

private static readonly nint _RTAX_MAX = 8;



private static (slice<byte>, error) getIfList() {
    slice<byte> _p0 = default;
    error _p0 = default!;

    var (needed, err) = syscall.Getkerninfo(_KINFO_RT_IFLIST, 0, 0, 0);
    if (err != null) {
        return (null, error.As(err)!);
    }
    var tab = make_slice<byte>(needed);
    _, err = syscall.Getkerninfo(_KINFO_RT_IFLIST, uintptr(@unsafe.Pointer(_addr_tab[0])), uintptr(@unsafe.Pointer(_addr_needed)), 0);
    if (err != null) {
        return (null, error.As(err)!);
    }
    return (tab[..(int)needed], error.As(null!)!);

}

// If the ifindex is zero, interfaceTable returns mappings of all
// network interfaces. Otherwise it returns a mapping of a specific
// interface.
private static (slice<Interface>, error) interfaceTable(nint ifindex) => func((defer, _, _) => {
    slice<Interface> _p0 = default;
    error _p0 = default!;

    var (tab, err) = getIfList();
    if (err != null) {
        return (null, error.As(err)!);
    }
    var (sock, err) = sysSocket(syscall.AF_INET, syscall.SOCK_DGRAM, 0);
    if (err != null) {
        return (null, error.As(err)!);
    }
    defer(poll.CloseFunc(sock));

    slice<Interface> ift = default;
    while (len(tab) > 0) {
        var ifm = (syscall.IfMsgHdr.val)(@unsafe.Pointer(_addr_tab[0]));
        if (ifm.Msglen == 0) {
            break;
        }
        if (ifm.Type == syscall.RTM_IFINFO) {
            if (ifindex == 0 || ifindex == int(ifm.Index)) {
                var sdl = (rawSockaddrDatalink.val)(@unsafe.Pointer(_addr_tab[syscall.SizeofIfMsghdr]));

                ptr<Interface> ifi = addr(new Interface(Index:int(ifm.Index),Flags:linkFlags(ifm.Flags)));
                ifi.Name = string(sdl.Data[..(int)sdl.Nlen]);
                ifi.HardwareAddr = sdl.Data[(int)sdl.Nlen..(int)sdl.Nlen + sdl.Alen]; 

                // Retrieve MTU
                ptr<ifreq> ifr = addr(new ifreq());
                copy(ifr.Name[..], ifi.Name);
                err = unix.Ioctl(sock, syscall.SIOCGIFMTU, uintptr(@unsafe.Pointer(ifr)));
                if (err != null) {
                    return (null, error.As(err)!);
                }

                ifi.MTU = int(ifr.Ifru[0]) << 24 | int(ifr.Ifru[1]) << 16 | int(ifr.Ifru[2]) << 8 | int(ifr.Ifru[3]);

                ift = append(ift, ifi.val);
                if (ifindex == int(ifm.Index)) {
                    break;
                }

            }

        }
        tab = tab[(int)ifm.Msglen..];

    }

    return (ift, error.As(null!)!);

});

private static Flags linkFlags(int rawFlags) {
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

    var (tab, err) = getIfList();
    if (err != null) {
        return (null, error.As(err)!);
    }
    slice<Addr> ifat = default;
    while (len(tab) > 0) {
        var ifm = (syscall.IfMsgHdr.val)(@unsafe.Pointer(_addr_tab[0]));
        if (ifm.Msglen == 0) {
            break;
        }
        if (ifm.Type == syscall.RTM_NEWADDR) {
            if (ifi == null || ifi.Index == int(ifm.Index)) {
                var mask = ifm.Addrs;
                var off = uint(syscall.SizeofIfMsghdr);

                ptr<syscall.RawSockaddr> iprsa;                ptr<syscall.RawSockaddr> nmrsa;

                for (var i = uint(0); i < _RTAX_MAX; i++) {
                    if (mask & (1 << (int)(i)) == 0) {
                        continue;
                    }
                    var rsa = (syscall.RawSockaddr.val)(@unsafe.Pointer(_addr_tab[off]));
                    if (i == _RTAX_NETMASK) {
                        nmrsa = rsa;
                    }
                    if (i == _RTAX_IFA) {
                        iprsa = rsa;
                    }
                    off += (uint(rsa.Len) + 3) & ~3;
                }

                if (iprsa != null && nmrsa != null) {
                    mask = default;
                    IP ip = default;


                    if (iprsa.Family == syscall.AF_INET) 
                        var ipsa = (syscall.RawSockaddrInet4.val)(@unsafe.Pointer(iprsa));
                        var nmsa = (syscall.RawSockaddrInet4.val)(@unsafe.Pointer(nmrsa));
                        ip = IPv4(ipsa.Addr[0], ipsa.Addr[1], ipsa.Addr[2], ipsa.Addr[3]);
                        mask = IPv4Mask(nmsa.Addr[0], nmsa.Addr[1], nmsa.Addr[2], nmsa.Addr[3]);
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
        tab = tab[(int)ifm.Msglen..];

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
