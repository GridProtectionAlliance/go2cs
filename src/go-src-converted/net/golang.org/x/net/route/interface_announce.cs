// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build dragonfly freebsd netbsd

// package route -- go2cs converted at 2022 March 06 22:15:55 UTC
// import "golang.org/x/net/route" ==> using route = go.golang.org.x.net.route_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\net\route\interface_announce.go


namespace go.golang.org.x.net;

public static partial class route_package {

private static (Message, error) parseInterfaceAnnounceMessage(this ptr<wireFormat> _addr_w, RIBType _, slice<byte> b) {
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
    ptr<InterfaceAnnounceMessage> m = addr(new InterfaceAnnounceMessage(Version:int(b[2]),Type:int(b[3]),Index:int(nativeEndian.Uint16(b[4:6])),What:int(nativeEndian.Uint16(b[22:24])),raw:b[:l],));
    for (nint i = 0; i < 16; i++) {
        if (b[6 + i] != 0) {
            continue;
        }
        m.Name = string(b[(int)6..(int)6 + i]);
        break;

    }
    return (m, error.As(null!)!);

}

} // end route_package
