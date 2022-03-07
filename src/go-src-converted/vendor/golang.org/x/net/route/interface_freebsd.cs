// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package route -- go2cs converted at 2022 March 06 23:38:13 UTC
// import "vendor/golang.org/x/net/route" ==> using route = go.vendor.golang.org.x.net.route_package
// Original source: C:\Program Files\Go\src\vendor\golang.org\x\net\route\interface_freebsd.go


namespace go.vendor.golang.org.x.net;

public static partial class route_package {

private static (Message, error) parseInterfaceMessage(this ptr<wireFormat> _addr_w, RIBType typ, slice<byte> b) {
    Message _p0 = default;
    error _p0 = default!;
    ref wireFormat w = ref _addr_w.val;

    nint extOff = default;    nint bodyOff = default;

    if (typ == sysNET_RT_IFLISTL) {
        if (len(b) < 20) {
            return (null, error.As(errMessageTooShort)!);
        }
        extOff = int(nativeEndian.Uint16(b[(int)18..(int)20]));
        bodyOff = int(nativeEndian.Uint16(b[(int)16..(int)18]));

    }
    else
 {
        extOff = w.extOff;
        bodyOff = w.bodyOff;
    }
    if (len(b) < extOff || len(b) < bodyOff) {
        return (null, error.As(errInvalidMessage)!);
    }
    var l = int(nativeEndian.Uint16(b[..(int)2]));
    if (len(b) < l) {
        return (null, error.As(errInvalidMessage)!);
    }
    var attrs = uint(nativeEndian.Uint32(b[(int)4..(int)8]));
    if (attrs & sysRTA_IFP == 0) {
        return (null, error.As(null!)!);
    }
    ptr<InterfaceMessage> m = addr(new InterfaceMessage(Version:int(b[2]),Type:int(b[3]),Flags:int(nativeEndian.Uint32(b[8:12])),Index:int(nativeEndian.Uint16(b[12:14])),Addrs:make([]Addr,sysRTAX_MAX),extOff:extOff,raw:b[:l],));
    var (a, err) = parseLinkAddr(b[(int)bodyOff..]);
    if (err != null) {
        return (null, error.As(err)!);
    }
    m.Addrs[sysRTAX_IFP] = a;
    m.Name = a._<ptr<LinkAddr>>().Name;
    return (m, error.As(null!)!);

}

private static (Message, error) parseInterfaceAddrMessage(this ptr<wireFormat> _addr_w, RIBType typ, slice<byte> b) {
    Message _p0 = default;
    error _p0 = default!;
    ref wireFormat w = ref _addr_w.val;

    nint bodyOff = default;
    if (typ == sysNET_RT_IFLISTL) {
        if (len(b) < 24) {
            return (null, error.As(errMessageTooShort)!);
        }
        bodyOff = int(nativeEndian.Uint16(b[(int)16..(int)18]));

    }
    else
 {
        bodyOff = w.bodyOff;
    }
    if (len(b) < bodyOff) {
        return (null, error.As(errInvalidMessage)!);
    }
    var l = int(nativeEndian.Uint16(b[..(int)2]));
    if (len(b) < l) {
        return (null, error.As(errInvalidMessage)!);
    }
    ptr<InterfaceAddrMessage> m = addr(new InterfaceAddrMessage(Version:int(b[2]),Type:int(b[3]),Flags:int(nativeEndian.Uint32(b[8:12])),Index:int(nativeEndian.Uint16(b[12:14])),raw:b[:l],));
    error err = default!;
    m.Addrs, err = parseAddrs(uint(nativeEndian.Uint32(b[(int)4..(int)8])), parseKernelInetAddr, b[(int)bodyOff..]);
    if (err != null) {
        return (null, error.As(err)!);
    }
    return (m, error.As(null!)!);

}

} // end route_package
