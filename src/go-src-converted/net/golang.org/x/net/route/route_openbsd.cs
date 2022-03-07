// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package route -- go2cs converted at 2022 March 06 22:15:57 UTC
// import "golang.org/x/net/route" ==> using route = go.golang.org.x.net.route_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\net\route\route_openbsd.go
using syscall = go.syscall_package;

namespace go.golang.org.x.net;

public static partial class route_package {

private static (slice<byte>, error) marshal(this ptr<RouteMessage> _addr_m) {
    slice<byte> _p0 = default;
    error _p0 = default!;
    ref RouteMessage m = ref _addr_m.val;

    var l = sizeofRtMsghdr + addrsSpace(m.Addrs);
    var b = make_slice<byte>(l);
    nativeEndian.PutUint16(b[..(int)2], uint16(l));
    if (m.Version == 0) {
        b[2] = sysRTM_VERSION;
    }
    else
 {
        b[2] = byte(m.Version);
    }
    b[3] = byte(m.Type);
    nativeEndian.PutUint16(b[(int)4..(int)6], uint16(sizeofRtMsghdr));
    nativeEndian.PutUint32(b[(int)16..(int)20], uint32(m.Flags));
    nativeEndian.PutUint16(b[(int)6..(int)8], uint16(m.Index));
    nativeEndian.PutUint32(b[(int)24..(int)28], uint32(m.ID));
    nativeEndian.PutUint32(b[(int)28..(int)32], uint32(m.Seq));
    var (attrs, err) = marshalAddrs(b[(int)sizeofRtMsghdr..], m.Addrs);
    if (err != null) {
        return (null, error.As(err)!);
    }
    if (attrs > 0) {
        nativeEndian.PutUint32(b[(int)12..(int)16], uint32(attrs));
    }
    return (b, error.As(null!)!);

}

private static (Message, error) parseRouteMessage(this ptr<wireFormat> _addr__p0, RIBType _, slice<byte> b) {
    Message _p0 = default;
    error _p0 = default!;
    ref wireFormat _p0 = ref _addr__p0.val;

    if (len(b) < sizeofRtMsghdr) {
        return (null, error.As(errMessageTooShort)!);
    }
    var l = int(nativeEndian.Uint16(b[..(int)2]));
    if (len(b) < l) {
        return (null, error.As(errInvalidMessage)!);
    }
    ptr<RouteMessage> m = addr(new RouteMessage(Version:int(b[2]),Type:int(b[3]),Flags:int(nativeEndian.Uint32(b[16:20])),Index:int(nativeEndian.Uint16(b[6:8])),ID:uintptr(nativeEndian.Uint32(b[24:28])),Seq:int(nativeEndian.Uint32(b[28:32])),raw:b[:l],));
    var ll = int(nativeEndian.Uint16(b[(int)4..(int)6]));
    if (len(b) < ll) {
        return (null, error.As(errInvalidMessage)!);
    }
    var errno = syscall.Errno(nativeEndian.Uint32(b[(int)32..(int)36]));
    if (errno != 0) {
        m.Err = errno;
    }
    var (as, err) = parseAddrs(uint(nativeEndian.Uint32(b[(int)12..(int)16])), parseKernelInetAddr, b[(int)ll..]);
    if (err != null) {
        return (null, error.As(err)!);
    }
    m.Addrs = as;
    return (m, error.As(null!)!);

}

} // end route_package
