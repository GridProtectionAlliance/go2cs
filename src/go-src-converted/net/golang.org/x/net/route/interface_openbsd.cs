// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package route -- go2cs converted at 2022 March 06 22:15:56 UTC
// import "golang.org/x/net/route" ==> using route = go.golang.org.x.net.route_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\net\route\interface_openbsd.go


namespace go.golang.org.x.net;

public static partial class route_package {

private static (Message, error) parseInterfaceMessage(this ptr<wireFormat> _addr__p0, RIBType _, slice<byte> b) {
    Message _p0 = default;
    error _p0 = default!;
    ref wireFormat _p0 = ref _addr__p0.val;

    if (len(b) < 32) {
        return (null, error.As(errMessageTooShort)!);
    }
    var l = int(nativeEndian.Uint16(b[..(int)2]));
    if (len(b) < l) {
        return (null, error.As(errInvalidMessage)!);
    }
    var attrs = uint(nativeEndian.Uint32(b[(int)12..(int)16]));
    if (attrs & sysRTA_IFP == 0) {
        return (null, error.As(null!)!);
    }
    ptr<InterfaceMessage> m = addr(new InterfaceMessage(Version:int(b[2]),Type:int(b[3]),Flags:int(nativeEndian.Uint32(b[16:20])),Index:int(nativeEndian.Uint16(b[6:8])),Addrs:make([]Addr,sysRTAX_MAX),raw:b[:l],));
    var ll = int(nativeEndian.Uint16(b[(int)4..(int)6]));
    if (len(b) < ll) {
        return (null, error.As(errInvalidMessage)!);
    }
    var (a, err) = parseLinkAddr(b[(int)ll..]);
    if (err != null) {
        return (null, error.As(err)!);
    }
    m.Addrs[sysRTAX_IFP] = a;
    m.Name = a._<ptr<LinkAddr>>().Name;
    return (m, error.As(null!)!);

}

private static (Message, error) parseInterfaceAddrMessage(this ptr<wireFormat> _addr__p0, RIBType _, slice<byte> b) {
    Message _p0 = default;
    error _p0 = default!;
    ref wireFormat _p0 = ref _addr__p0.val;

    if (len(b) < 24) {
        return (null, error.As(errMessageTooShort)!);
    }
    var l = int(nativeEndian.Uint16(b[..(int)2]));
    if (len(b) < l) {
        return (null, error.As(errInvalidMessage)!);
    }
    var bodyOff = int(nativeEndian.Uint16(b[(int)4..(int)6]));
    if (len(b) < bodyOff) {
        return (null, error.As(errInvalidMessage)!);
    }
    ptr<InterfaceAddrMessage> m = addr(new InterfaceAddrMessage(Version:int(b[2]),Type:int(b[3]),Flags:int(nativeEndian.Uint32(b[12:16])),Index:int(nativeEndian.Uint16(b[6:8])),raw:b[:l],));
    error err = default!;
    m.Addrs, err = parseAddrs(uint(nativeEndian.Uint32(b[(int)12..(int)16])), parseKernelInetAddr, b[(int)bodyOff..]);
    if (err != null) {
        return (null, error.As(err)!);
    }
    return (m, error.As(null!)!);

}

private static (Message, error) parseInterfaceAnnounceMessage(this ptr<wireFormat> _addr__p0, RIBType _, slice<byte> b) {
    Message _p0 = default;
    error _p0 = default!;
    ref wireFormat _p0 = ref _addr__p0.val;

    if (len(b) < 26) {
        return (null, error.As(errMessageTooShort)!);
    }
    var l = int(nativeEndian.Uint16(b[..(int)2]));
    if (len(b) < l) {
        return (null, error.As(errInvalidMessage)!);
    }
    ptr<InterfaceAnnounceMessage> m = addr(new InterfaceAnnounceMessage(Version:int(b[2]),Type:int(b[3]),Index:int(nativeEndian.Uint16(b[6:8])),What:int(nativeEndian.Uint16(b[8:10])),raw:b[:l],));
    for (nint i = 0; i < 16; i++) {
        if (b[10 + i] != 0) {
            continue;
        }
        m.Name = string(b[(int)10..(int)10 + i]);
        break;

    }
    return (m, error.As(null!)!);

}

} // end route_package
