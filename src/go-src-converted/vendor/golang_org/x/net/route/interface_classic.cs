// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin dragonfly netbsd

// package route -- go2cs converted at 2020 August 29 10:12:32 UTC
// import "vendor/golang_org/x/net/route" ==> using route = go.vendor.golang_org.x.net.route_package
// Original source: C:\Go\src\vendor\golang_org\x\net\route\interface_classic.go
using runtime = go.runtime_package;
using static go.builtin;

namespace go {
namespace vendor {
namespace golang_org {
namespace x {
namespace net
{
    public static partial class route_package
    {
        private static (Message, error) parseInterfaceMessage(this ref wireFormat w, RIBType _, slice<byte> b)
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
            var attrs = uint(nativeEndian.Uint32(b[4L..8L]));
            if (attrs & sysRTA_IFP == 0L)
            {
                return (null, null);
            }
            InterfaceMessage m = ref new InterfaceMessage(Version:int(b[2]),Type:int(b[3]),Addrs:make([]Addr,sysRTAX_MAX),Flags:int(nativeEndian.Uint32(b[8:12])),Index:int(nativeEndian.Uint16(b[12:14])),extOff:w.extOff,raw:b[:l],);
            var (a, err) = parseLinkAddr(b[w.bodyOff..]);
            if (err != null)
            {
                return (null, err);
            }
            m.Addrs[sysRTAX_IFP] = a;
            m.Name = a._<ref LinkAddr>().Name;
            return (m, null);
        }

        private static (Message, error) parseInterfaceAddrMessage(this ref wireFormat w, RIBType _, slice<byte> b)
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
            InterfaceAddrMessage m = ref new InterfaceAddrMessage(Version:int(b[2]),Type:int(b[3]),Flags:int(nativeEndian.Uint32(b[8:12])),raw:b[:l],);
            if (runtime.GOOS == "netbsd")
            {
                m.Index = int(nativeEndian.Uint16(b[16L..18L]));
            }
            else
            {
                m.Index = int(nativeEndian.Uint16(b[12L..14L]));
            }
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
