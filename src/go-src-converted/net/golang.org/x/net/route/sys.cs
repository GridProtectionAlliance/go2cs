// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin dragonfly freebsd netbsd openbsd

// package route -- go2cs converted at 2022 March 06 22:15:57 UTC
// import "golang.org/x/net/route" ==> using route = go.golang.org.x.net.route_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\net\route\sys.go
using @unsafe = go.@unsafe_package;
using System;


namespace go.golang.org.x.net;

public static partial class route_package {

private static binaryByteOrder nativeEndian = default;private static nint kernelAlign = default;private static byte rtmVersion = default;private static map<nint, ptr<wireFormat>> wireFormats = default;

private static void init() {
    ref var i = ref heap(uint32(1), out ptr<var> _addr_i);
    ptr<array<byte>> b = new ptr<ptr<array<byte>>>(@unsafe.Pointer(_addr_i));
    if (b[0] == 1) {
        nativeEndian = littleEndian;
    }
    else
 {
        nativeEndian = bigEndian;
    }
    rtmVersion = sysRTM_VERSION;
    kernelAlign, wireFormats = probeRoutingStack();

}

private static nint roundup(nint l) {
    if (l == 0) {
        return kernelAlign;
    }
    return (l + kernelAlign - 1) & ~(kernelAlign - 1);

}

private partial struct wireFormat {
    public nint extOff; // offset of header extension
    public nint bodyOff; // offset of message body
    public Func<RIBType, slice<byte>, (Message, error)> parse;
}

} // end route_package
