// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin dragonfly freebsd

// package route -- go2cs converted at 2020 August 29 10:12:33 UTC
// import "vendor/golang_org/x/net/route" ==> using route = go.vendor.golang_org.x.net.route_package
// Original source: C:\Go\src\vendor\golang_org\x\net\route\interface_multicast.go

using static go.builtin;

namespace go {
namespace vendor {
namespace golang_org {
namespace x {
namespace net
{
    public static partial class route_package
    {
        private static (Message, error) parseInterfaceMulticastAddrMessage(this ref wireFormat w, RIBType _, slice<byte> b)
        {
            if (len(b) < w.bodyOff)
            {
                return (null, errMessageTooShort);
            }
            var l = int(nativeEndian.Uint16(b[..2L]));
            if (len(b) < l)
            {
                return (null, errInvalidMessage);
            }
            InterfaceMulticastAddrMessage m = ref new InterfaceMulticastAddrMessage(Version:int(b[2]),Type:int(b[3]),Flags:int(nativeEndian.Uint32(b[8:12])),Index:int(nativeEndian.Uint16(b[12:14])),raw:b[:l],);
            error err = default;
            m.Addrs, err = parseAddrs(uint(nativeEndian.Uint32(b[4L..8L])), parseKernelInetAddr, b[w.bodyOff..]);
            if (err != null)
            {
                return (null, err);
            }
            return (m, null);
        }
    }
}}}}}
