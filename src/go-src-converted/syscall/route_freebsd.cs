// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package syscall -- go2cs converted at 2022 March 06 22:26:44 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Program Files\Go\src\syscall\route_freebsd.go
using @unsafe = go.@unsafe_package;

namespace go;

public static partial class syscall_package {

private static void init() {
    var (conf, _) = Sysctl("kern.conftxt");
    for (nint i = 0;
    nint j = 0; j < len(conf); j++) {
        if (conf[j] != '\n') {
            continue;
        }
        var s = conf[(int)i..(int)j];
        i = j + 1;
        if (len(s) > len("machine") && s[..(int)len("machine")] == "machine") {
            s = s[(int)len("machine")..];
            for (nint k = 0; k < len(s); k++) {
                if (s[k] == ' ' || s[k] == '\t') {
                    s = s[(int)1..];
                }
                break;

            }
            freebsdConfArch = s;
            break;

        }
    }

}

private static RoutingMessage toRoutingMessage(this ptr<anyMessage> _addr_any, slice<byte> b) {
    ref anyMessage any = ref _addr_any.val;


    if (any.Type == RTM_ADD || any.Type == RTM_DELETE || any.Type == RTM_CHANGE || any.Type == RTM_GET || any.Type == RTM_LOSING || any.Type == RTM_REDIRECT || any.Type == RTM_MISS || any.Type == RTM_LOCK || any.Type == RTM_RESOLVE) 
        return any.parseRouteMessage(b);
    else if (any.Type == RTM_IFINFO) 
        return any.parseInterfaceMessage(b);
    else if (any.Type == RTM_IFANNOUNCE) 
        var p = (InterfaceAnnounceMessage.val)(@unsafe.Pointer(any));
        return addr(new InterfaceAnnounceMessage(Header:p.Header));
    else if (any.Type == RTM_NEWADDR || any.Type == RTM_DELADDR) 
        p = (InterfaceAddrMessage.val)(@unsafe.Pointer(any));
        return addr(new InterfaceAddrMessage(Header:p.Header,Data:b[SizeofIfaMsghdr:any.Msglen]));
    else if (any.Type == RTM_NEWMADDR || any.Type == RTM_DELMADDR) 
        p = (InterfaceMulticastAddrMessage.val)(@unsafe.Pointer(any));
        return addr(new InterfaceMulticastAddrMessage(Header:p.Header,Data:b[SizeofIfmaMsghdr:any.Msglen]));
        return null;

}

// InterfaceAnnounceMessage represents a routing message containing
// network interface arrival and departure information.
//
// Deprecated: Use golang.org/x/net/route instead.
public partial struct InterfaceAnnounceMessage {
    public IfAnnounceMsghdr Header;
}

private static (slice<Sockaddr>, error) sockaddr(this ptr<InterfaceAnnounceMessage> _addr_m) {
    slice<Sockaddr> _p0 = default;
    error _p0 = default!;
    ref InterfaceAnnounceMessage m = ref _addr_m.val;

    return (null, error.As(null!)!);
}

// InterfaceMulticastAddrMessage represents a routing message
// containing network interface address entries.
//
// Deprecated: Use golang.org/x/net/route instead.
public partial struct InterfaceMulticastAddrMessage {
    public IfmaMsghdr Header;
    public slice<byte> Data;
}

private static (slice<Sockaddr>, error) sockaddr(this ptr<InterfaceMulticastAddrMessage> _addr_m) {
    slice<Sockaddr> _p0 = default;
    error _p0 = default!;
    ref InterfaceMulticastAddrMessage m = ref _addr_m.val;

    array<Sockaddr> sas = new array<Sockaddr>(RTAX_MAX);
    var b = m.Data[..];
    for (var i = uint(0); i < RTAX_MAX && len(b) >= minRoutingSockaddrLen; i++) {
        if (m.Header.Addrs & (1 << (int)(i)) == 0) {
            continue;
        }
        var rsa = (RawSockaddr.val)(@unsafe.Pointer(_addr_b[0]));

        if (rsa.Family == AF_LINK) 
            var (sa, err) = parseSockaddrLink(b);
            if (err != null) {
                return (null, error.As(err)!);
            }
            sas[i] = sa;
            b = b[(int)rsaAlignOf(int(rsa.Len))..];
        else if (rsa.Family == AF_INET || rsa.Family == AF_INET6) 
            (sa, err) = parseSockaddrInet(b, rsa.Family);
            if (err != null) {
                return (null, error.As(err)!);
            }
            sas[i] = sa;
            b = b[(int)rsaAlignOf(int(rsa.Len))..];
        else 
            var (sa, l, err) = parseLinkLayerAddr(b);
            if (err != null) {
                return (null, error.As(err)!);
            }
            sas[i] = sa;
            b = b[(int)l..];
        
    }
    return (sas[..], error.As(null!)!);

}

} // end syscall_package
