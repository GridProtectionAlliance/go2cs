// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin dragonfly netbsd

// package route -- go2cs converted at 2020 October 08 05:01:37 UTC
// import "vendor/golang.org/x/net/route" ==> using route = go.vendor.golang.org.x.net.route_package
// Original source: C:\Go\src\vendor\golang.org\x\net\route\interface_classic.go
using runtime = go.runtime_package;
using static go.builtin;

namespace go {
namespace vendor {
namespace golang.org {
namespace x {
namespace net
{
    public static partial class route_package
    {
        private static (Message, error) parseInterfaceMessage(this ptr<wireFormat> _addr_w, RIBType _, slice<byte> b)
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
            var attrs = uint(nativeEndian.Uint32(b[4L..8L]));
            if (attrs & sysRTA_IFP == 0L)
            {
                return (null, error.As(null!)!);
            }
            ptr<InterfaceMessage> m = addr(new InterfaceMessage(Version:int(b[2]),Type:int(b[3]),Addrs:make([]Addr,sysRTAX_MAX),Flags:int(nativeEndian.Uint32(b[8:12])),Index:int(nativeEndian.Uint16(b[12:14])),extOff:w.extOff,raw:b[:l],));
            var (a, err) = parseLinkAddr(b[w.bodyOff..]);
            if (err != null)
            {
                return (null, error.As(err)!);
            }
            m.Addrs[sysRTAX_IFP] = a;
            m.Name = a._<ptr<LinkAddr>>().Name;
            return (m, error.As(null!)!);

        }

        private static (Message, error) parseInterfaceAddrMessage(this ptr<wireFormat> _addr_w, RIBType _, slice<byte> b)
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

            ptr<InterfaceAddrMessage> m = addr(new InterfaceAddrMessage(Version:int(b[2]),Type:int(b[3]),Flags:int(nativeEndian.Uint32(b[8:12])),raw:b[:l],));
            if (runtime.GOOS == "netbsd")
            {
                m.Index = int(nativeEndian.Uint16(b[16L..18L]));
            }
            else
            {
                m.Index = int(nativeEndian.Uint16(b[12L..14L]));
            }

            error err = default!;
            m.Addrs, err = parseAddrs(uint(nativeEndian.Uint32(b[4L..8L])), parseKernelInetAddr, b[w.bodyOff..]);
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            return (m, error.As(null!)!);

        }
    }
}}}}}
