// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build darwin || dragonfly || netbsd
// +build darwin dragonfly netbsd

// package route -- go2cs converted at 2022 March 06 23:38:13 UTC
// import "vendor/golang.org/x/net/route" ==> using route = go.vendor.golang.org.x.net.route_package
// Original source: C:\Program Files\Go\src\vendor\golang.org\x\net\route\interface_classic.go
using runtime = go.runtime_package;

namespace go.vendor.golang.org.x.net;

public static partial class route_package {

private static (Message, error) parseInterfaceMessage(this ptr<wireFormat> _addr_w, RIBType _, slice<byte> b) {
    Message _p0 = default;
    error _p0 = default!;
    ref wireFormat w = ref _addr_w.val;

    if (len(b) < w.bodyOff) {
        return (null, error.As(errMessageTooShort)!);
    }
    var l = int(nativeEndian.Uint16(b[..(int)2]));
    if (len(b) < l) {
        return (null, error.As(errInvalidMessage)!);
    }
    var attrs = uint(nativeEndian.Uint32(b[(int)4..(int)8]));
    if (attrs & sysRTA_IFP == 0) {
        return (null, error.As(null!)!);
    }
    ptr<InterfaceMessage> m = addr(new InterfaceMessage(Version:int(b[2]),Type:int(b[3]),Addrs:make([]Addr,sysRTAX_MAX),Flags:int(nativeEndian.Uint32(b[8:12])),Index:int(nativeEndian.Uint16(b[12:14])),extOff:w.extOff,raw:b[:l],));
    var (a, err) = parseLinkAddr(b[(int)w.bodyOff..]);
    if (err != null) {
        return (null, error.As(err)!);
    }
    m.Addrs[sysRTAX_IFP] = a;
    m.Name = a._<ptr<LinkAddr>>().Name;
    return (m, error.As(null!)!);

}

private static (Message, error) parseInterfaceAddrMessage(this ptr<wireFormat> _addr_w, RIBType _, slice<byte> b) {
    Message _p0 = default;
    error _p0 = default!;
    ref wireFormat w = ref _addr_w.val;

    if (len(b) < w.bodyOff) {
        return (null, error.As(errMessageTooShort)!);
    }
    var l = int(nativeEndian.Uint16(b[..(int)2]));
    if (len(b) < l) {
        return (null, error.As(errInvalidMessage)!);
    }
    ptr<InterfaceAddrMessage> m = addr(new InterfaceAddrMessage(Version:int(b[2]),Type:int(b[3]),Flags:int(nativeEndian.Uint32(b[8:12])),raw:b[:l],));
    if (runtime.GOOS == "netbsd") {
        m.Index = int(nativeEndian.Uint16(b[(int)16..(int)18]));
    }
    else
 {
        m.Index = int(nativeEndian.Uint16(b[(int)12..(int)14]));
    }
    error err = default!;
    m.Addrs, err = parseAddrs(uint(nativeEndian.Uint32(b[(int)4..(int)8])), parseKernelInetAddr, b[(int)w.bodyOff..]);
    if (err != null) {
        return (null, error.As(err)!);
    }
    return (m, error.As(null!)!);

}

} // end route_package
