// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin dragonfly freebsd

// package route -- go2cs converted at 2020 October 09 04:51:38 UTC
// import "golang.org/x/net/route" ==> using route = go.golang.org.x.net.route_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\net\route\interface_multicast.go

using static go.builtin;

namespace go {
namespace golang.org {
namespace x {
namespace net
{
    public static partial class route_package
    {
        private static (Message, error) parseInterfaceMulticastAddrMessage(this ptr<wireFormat> _addr_w, RIBType _, slice<byte> b)
        {
            Message _p0 = default;
            error _p0 = default!;
            ref wireFormat w = ref _addr_w.val;

            if (len(b) < w.bodyOff)
            {
                return (null, error.As(errMessageTooShort)!);
            }
            var l = int(nativeEndian.Uint16(b[..2L]));
            if (len(b) < l)
            {
                return (null, error.As(errInvalidMessage)!);
            }
            ptr<InterfaceMulticastAddrMessage> m = addr(new InterfaceMulticastAddrMessage(Version:int(b[2]),Type:int(b[3]),Flags:int(nativeEndian.Uint32(b[8:12])),Index:int(nativeEndian.Uint16(b[12:14])),raw:b[:l],));
            error err = default!;
            m.Addrs, err = parseAddrs(uint(nativeEndian.Uint32(b[4L..8L])), parseKernelInetAddr, b[w.bodyOff..]);
            if (err != null)
            {
                return (null, error.As(err)!);
            }
            return (m, error.As(null!)!);

        }
    }
}}}}
