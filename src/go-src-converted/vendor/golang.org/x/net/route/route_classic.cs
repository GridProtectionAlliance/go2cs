// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build darwin || dragonfly || freebsd || netbsd
// +build darwin dragonfly freebsd netbsd

// package route -- go2cs converted at 2022 March 06 23:38:14 UTC
// import "vendor/golang.org/x/net/route" ==> using route = go.vendor.golang.org.x.net.route_package
// Original source: C:\Program Files\Go\src\vendor\golang.org\x\net\route\route_classic.go
using runtime = go.runtime_package;
using syscall = go.syscall_package;

namespace go.vendor.golang.org.x.net;

public static partial class route_package {

private static (slice<byte>, error) marshal(this ptr<RouteMessage> _addr_m) {
    slice<byte> _p0 = default;
    error _p0 = default!;
    ref RouteMessage m = ref _addr_m.val;

    var (w, ok) = wireFormats[m.Type];
    if (!ok) {
        return (null, error.As(errUnsupportedMessage)!);
    }
    var l = w.bodyOff + addrsSpace(m.Addrs);
    if (runtime.GOOS == "darwin" || runtime.GOOS == "ios") { 
        // Fix stray pointer writes on macOS.
        // See golang.org/issue/22456.
        l += 1024;

    }
    var b = make_slice<byte>(l);
    nativeEndian.PutUint16(b[..(int)2], uint16(l));
    if (m.Version == 0) {
        b[2] = rtmVersion;
    }
    else
 {
        b[2] = byte(m.Version);
    }
    b[3] = byte(m.Type);
    nativeEndian.PutUint32(b[(int)8..(int)12], uint32(m.Flags));
    nativeEndian.PutUint16(b[(int)4..(int)6], uint16(m.Index));
    nativeEndian.PutUint32(b[(int)16..(int)20], uint32(m.ID));
    nativeEndian.PutUint32(b[(int)20..(int)24], uint32(m.Seq));
    var (attrs, err) = marshalAddrs(b[(int)w.bodyOff..], m.Addrs);
    if (err != null) {
        return (null, error.As(err)!);
    }
    if (attrs > 0) {
        nativeEndian.PutUint32(b[(int)12..(int)16], uint32(attrs));
    }
    return (b, error.As(null!)!);

}

private static (Message, error) parseRouteMessage(this ptr<wireFormat> _addr_w, RIBType typ, slice<byte> b) {
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
    ptr<RouteMessage> m = addr(new RouteMessage(Version:int(b[2]),Type:int(b[3]),Flags:int(nativeEndian.Uint32(b[8:12])),Index:int(nativeEndian.Uint16(b[4:6])),ID:uintptr(nativeEndian.Uint32(b[16:20])),Seq:int(nativeEndian.Uint32(b[20:24])),extOff:w.extOff,raw:b[:l],));
    var errno = syscall.Errno(nativeEndian.Uint32(b[(int)28..(int)32]));
    if (errno != 0) {
        m.Err = errno;
    }
    error err = default!;
    m.Addrs, err = parseAddrs(uint(nativeEndian.Uint32(b[(int)12..(int)16])), parseKernelInetAddr, b[(int)w.bodyOff..]);
    if (err != null) {
        return (null, error.As(err)!);
    }
    return (m, error.As(null!)!);

}

} // end route_package
